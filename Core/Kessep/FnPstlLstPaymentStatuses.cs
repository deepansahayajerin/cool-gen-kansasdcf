// Program: FN_PSTL_LST_PAYMENT_STATUSES, ID: 371777960, model: 746.
// Short name: SWEPSTLP
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_PSTL_LST_PAYMENT_STATUSES.
/// </para>
/// <para>
/// RESP: DISB	
/// List statuses of payments
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnPstlLstPaymentStatuses: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_PSTL_LST_PAYMENT_STATUSES program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnPstlLstPaymentStatuses(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnPstlLstPaymentStatuses.
  /// </summary>
  public FnPstlLstPaymentStatuses(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
    this.local = context.GetData<Local>();
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // **********************************************
    // 12/11/96	R. Marchman	Add new security and next tran
    // 03/10/97	JF. Caillouet	Display / Dialog Design command changes.
    // 04/29/97	JF. Caillouet 	Change Current Date
    // 11/01/98        N.Engoor        Made changes to the Dialog flow.
    // **********************************************
    local.Current.Date = Now().Date;

    // Set initial EXIT STATE.
    ExitState = "ACO_NN0000_ALL_OK";
    local.Error.Count = 0;
    local.Common.Count = 0;
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    // Move all IMPORTs to EXPORTs.
    export.ShowHistory.SelectChar = import.ShowHistory.SelectChar;

    export.Export1.Index = 0;
    export.Export1.Clear();

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (export.Export1.IsFull)
      {
        break;
      }

      export.Export1.Update.Common.SelectChar =
        import.Import1.Item.Common.SelectChar;
      export.Export1.Update.PaymentStatus.Assign(
        import.Import1.Item.PaymentStatus);

      if (AsChar(import.Import1.Item.Common.SelectChar) == 'S')
      {
        export.FlowSelection.Assign(import.Import1.Item.PaymentStatus);
        ++local.Common.Count;
      }
      else if (IsEmpty(import.Import1.Item.Common.SelectChar))
      {
        // Do nothing
      }
      else
      {
        ++local.Error.Count;
      }

      export.Export1.Next();
    }

    // **** begin group B ****
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      UseScCabNextTranPut();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    // ----------------------------
    // Naveen - 12/01/1998
    // If multiple selects with a value of 'S' entered highlight Sel fields and 
    // display error message.
    // ----------------------------
    if (local.Common.Count > 1)
    {
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (AsChar(export.Export1.Item.Common.SelectChar) == 'S')
        {
          var field = GetField(export.Export1.Item.Common, "selectChar");

          field.Error = true;
        }

        ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
      }
    }

    // ----------------------------
    // Naveen - 12/01/1998
    // If multiple selects with a value other than  'S' entered highlight Sel 
    // fields and display error message.
    // ----------------------------
    if (local.Error.Count > 0)
    {
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (AsChar(export.Export1.Item.Common.SelectChar) != 'S' && !
          IsEmpty(export.Export1.Item.Common.SelectChar))
        {
          var field = GetField(export.Export1.Item.Common, "selectChar");

          field.Error = true;
        }

        ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
      }
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    // to validate action level security
    if (Equal(global.Command, "PSTM"))
    {
    }
    else
    {
      UseScCabTestSecurity();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }

    // Main CASE OF COMMAND.
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        // This case will display a list of payment statuses.
        // If show history = Y then display all records
        if (AsChar(import.ShowHistory.SelectChar) == 'Y')
        {
          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadPaymentStatus2())
          {
            // =================================================
            // 10/16/98 - Bud Adams  -  These 2 statuses are used only to
            //   communicate between OREL and OREC.  They are not to be
            //   assigned by any user or used for any other purpose.
            // =================================================
            if (Equal(entities.PaymentStatus.Code, "RCVCRE") || Equal
              (entities.PaymentStatus.Code, "RCVDEN"))
            {
              export.Export1.Next();

              continue;
            }

            export.Export1.Update.PaymentStatus.Assign(entities.PaymentStatus);

            if (Equal(entities.PaymentStatus.DiscontinueDate, local.Max.Date))
            {
              export.Export1.Update.PaymentStatus.DiscontinueDate =
                local.Zero.Date;
            }

            export.Export1.Next();
          }
        }
        else if (IsEmpty(import.ShowHistory.SelectChar) || AsChar
          (import.ShowHistory.SelectChar) == 'N')
        {
          // --------------------
          // Else display all records that are currently in effect or will be 
          // effective in future
          // --------------------
          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadPaymentStatus1())
          {
            // =================================================
            // 10/16/98 - Bud Adams  -  These 2 statuses are used only to
            //   communicate between OREL and OREC.  They are not to be
            //   assigned by any user or used for any other purpose.
            // =================================================
            if (Equal(entities.PaymentStatus.Code, "RCVCRE") || Equal
              (entities.PaymentStatus.Code, "RCVDEN"))
            {
              export.Export1.Next();

              continue;
            }

            export.Export1.Update.PaymentStatus.Assign(entities.PaymentStatus);

            if (Equal(entities.PaymentStatus.DiscontinueDate, local.Max.Date))
            {
              export.Export1.Update.PaymentStatus.DiscontinueDate =
                local.Zero.Date;
            }

            export.Export1.Next();
          }
        }
        else
        {
          ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";

          var field = GetField(export.ShowHistory, "selectChar");

          field.Error = true;
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }

        break;
      case "RETURN":
        if (local.Common.Count <= 1)
        {
          // ----------------------
          // This flag is being set and passed to PSTM.
          // ----------------------
          export.Flag.Flag = "Y";
          ExitState = "ACO_NE0000_RETURN";
        }
        else
        {
          ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
        }

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "NEXT":
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "PSTM":
        // ----------------------
        // Only one selection is allowed while flowing to PSTM. If no selection 
        // is done it escapes out displaying an error message.
        // ----------------------
        if (local.Common.Count < 1)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";
        }
        else if (local.Common.Count == 1)
        {
          export.Flag.Flag = "Y";
          ExitState = "ECO_XFR_TO_MTN_PAYMENT_STAT";
        }
        else
        {
          ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
        }

        break;
      default:
        if (IsEmpty(global.Command))
        {
          return;
        }

        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveNextTranInfo(NextTranInfo source, NextTranInfo target)
  {
    target.LegalActionIdentifier = source.LegalActionIdentifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CsePersonNumberAp = source.CsePersonNumberAp;
    target.CsePersonNumberObligee = source.CsePersonNumberObligee;
    target.CsePersonNumberObligor = source.CsePersonNumberObligor;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.ObligationId = source.ObligationId;
    target.StandardCrtOrdNumber = source.StandardCrtOrdNumber;
    target.InfrastructureId = source.InfrastructureId;
    target.MiscText1 = source.MiscText1;
    target.MiscText2 = source.MiscText2;
    target.MiscNum1 = source.MiscNum1;
    target.MiscNum2 = source.MiscNum2;
    target.MiscNum1V2 = source.MiscNum1V2;
    target.MiscNum2V2 = source.MiscNum2V2;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.Max.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.Hidden.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveNextTranInfo(export.Hidden, useImport.NextTranInfo);

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabSignoff()
  {
    var useImport = new ScCabSignoff.Import();
    var useExport = new ScCabSignoff.Export();

    Call(ScCabSignoff.Execute, useImport, useExport);
  }

  private void UseScCabTestSecurity()
  {
    var useImport = new ScCabTestSecurity.Import();
    var useExport = new ScCabTestSecurity.Export();

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadPaymentStatus1()
  {
    return ReadEach("ReadPaymentStatus1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.PaymentStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentStatus.Code = db.GetString(reader, 1);
        entities.PaymentStatus.Name = db.GetString(reader, 2);
        entities.PaymentStatus.EffectiveDate = db.GetDate(reader, 3);
        entities.PaymentStatus.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.PaymentStatus.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadPaymentStatus2()
  {
    return ReadEach("ReadPaymentStatus2",
      null,
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.PaymentStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentStatus.Code = db.GetString(reader, 1);
        entities.PaymentStatus.Name = db.GetString(reader, 2);
        entities.PaymentStatus.EffectiveDate = db.GetDate(reader, 3);
        entities.PaymentStatus.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.PaymentStatus.Populated = true;

        return true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local;
  protected readonly Entities entities = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
      /// <summary>
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>
      /// A value of PaymentStatus.
      /// </summary>
      [JsonPropertyName("paymentStatus")]
      public PaymentStatus PaymentStatus
      {
        get => paymentStatus ??= new();
        set => paymentStatus = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common common;
      private PaymentStatus paymentStatus;
    }

    /// <summary>
    /// A value of ShowHistory.
    /// </summary>
    [JsonPropertyName("showHistory")]
    public Common ShowHistory
    {
      get => showHistory ??= new();
      set => showHistory = value;
    }

    /// <summary>
    /// Gets a value of Import1.
    /// </summary>
    [JsonIgnore]
    public Array<ImportGroup> Import1 => import1 ??= new(ImportGroup.Capacity);

    /// <summary>
    /// Gets a value of Import1 for json serialization.
    /// </summary>
    [JsonPropertyName("import1")]
    [Computed]
    public IList<ImportGroup> Import1_Json
    {
      get => import1;
      set => Import1.Assign(value);
    }

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    private Common showHistory;
    private Array<ImportGroup> import1;
    private NextTranInfo hidden;
    private Standard standard;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
      /// <summary>
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>
      /// A value of PaymentStatus.
      /// </summary>
      [JsonPropertyName("paymentStatus")]
      public PaymentStatus PaymentStatus
      {
        get => paymentStatus ??= new();
        set => paymentStatus = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common common;
      private PaymentStatus paymentStatus;
    }

    /// <summary>
    /// A value of Flag.
    /// </summary>
    [JsonPropertyName("flag")]
    public Common Flag
    {
      get => flag ??= new();
      set => flag = value;
    }

    /// <summary>
    /// A value of ShowHistory.
    /// </summary>
    [JsonPropertyName("showHistory")]
    public Common ShowHistory
    {
      get => showHistory ??= new();
      set => showHistory = value;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 => export1 ??= new(ExportGroup.Capacity);

    /// <summary>
    /// Gets a value of Export1 for json serialization.
    /// </summary>
    [JsonPropertyName("export1")]
    [Computed]
    public IList<ExportGroup> Export1_Json
    {
      get => export1;
      set => Export1.Assign(value);
    }

    /// <summary>
    /// A value of FlowSelection.
    /// </summary>
    [JsonPropertyName("flowSelection")]
    public PaymentStatus FlowSelection
    {
      get => flowSelection ??= new();
      set => flowSelection = value;
    }

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    private Common flag;
    private Common showHistory;
    private Array<ExportGroup> export1;
    private PaymentStatus flowSelection;
    private NextTranInfo hidden;
    private Standard standard;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local: IInitializable
  {
    /// <summary>
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public Common Error
    {
      get => error ??= new();
      set => error = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public DateWorkArea Zero
    {
      get => zero ??= new();
      set => zero = value;
    }

    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    /// <para>Resets the state.</para>
    void IInitializable.Initialize()
    {
      error = null;
      zero = null;
      max = null;
      common = null;
    }

    private Common error;
    private DateWorkArea current;
    private DateWorkArea zero;
    private DateWorkArea max;
    private Common common;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of PaymentStatus.
    /// </summary>
    [JsonPropertyName("paymentStatus")]
    public PaymentStatus PaymentStatus
    {
      get => paymentStatus ??= new();
      set => paymentStatus = value;
    }

    private PaymentStatus paymentStatus;
  }
#endregion
}
