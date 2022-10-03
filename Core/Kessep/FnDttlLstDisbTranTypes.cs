// Program: FN_DTTL_LST_DISB_TRAN_TYPES, ID: 371836599, model: 746.
// Short name: SWEDTTLP
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
/// A program: FN_DTTL_LST_DISB_TRAN_TYPES.
/// </para>
/// <para>
/// RESP: DISB
/// Will list all transaction Types w/in Disbursement Transaction
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnDttlLstDisbTranTypes: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DTTL_LST_DISB_TRAN_TYPES program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDttlLstDisbTranTypes(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDttlLstDisbTranTypes.
  /// </summary>
  public FnDttlLstDisbTranTypes(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // *********************************************
    // 12/04/96	R. Marchman	Add new security and next tran
    // 03/10/97	JF. Caillouet	Display / Dialog Design changes.
    // 04/30/97	JF. Caillouet	Change Current Date
    // *********************************************
    local.Current.Date = Now().Date;

    // Set initial EXIT STATE.
    ExitState = "ACO_NN0000_ALL_OK";
    local.Common.Count = 0;
    local.Error.Count = 0;
    local.Max.Date = UseCabSetMaximumDiscontinueDate();
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (Equal(global.Command, "XXNEXTXX"))
      {
        UseScCabNextTranGet();
        global.Command = "DISPLAY";
      }

      // Set command when comming in from a menu.
      if (Equal(global.Command, "XXFMMENU"))
      {
        global.Command = "DISPLAY";
      }
    }
    else
    {
      return;
    }

    // Move all IMPORTs to EXPORTs.
    export.Hidden.Assign(import.Hidden);
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
      export.Export1.Update.DisbursementTransactionType.Assign(
        import.Import1.Item.DisbursementTransactionType);

      if (AsChar(import.Import1.Item.Common.SelectChar) == 'S')
      {
        export.FlowSelection.Assign(
          import.Import1.Item.DisbursementTransactionType);
        ++local.Common.Count;
      }
      else if (IsEmpty(import.Import1.Item.Common.SelectChar))
      {
      }
      else
      {
        ++local.Error.Count;
      }

      export.Export1.Next();
    }

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

    if (Equal(global.Command, "RLCNAME") || Equal(global.Command, "DTTM"))
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

    // Want to display all group view, even with error on selection  codes,
    // before skip main case of command
    if (local.Error.Count > 0)
    {
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (!IsEmpty(export.Export1.Item.Common.SelectChar) && AsChar
          (export.Export1.Item.Common.SelectChar) != 'S')
        {
          var field = GetField(export.Export1.Item.Common, "selectChar");

          field.Error = true;
        }

        ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
      }
    }

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

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ***********************
    // Main CASE OF COMMAND.
    // ***********************
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        // If Show history = Y then display all records
        if (AsChar(import.ShowHistory.SelectChar) == 'Y')
        {
          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadDisbursementTransactionType2())
          {
            export.Export1.Update.DisbursementTransactionType.Assign(
              entities.DisbursementTransactionType);

            if (Equal(entities.DisbursementTransactionType.DiscontinueDate,
              local.Max.Date))
            {
              export.Export1.Update.DisbursementTransactionType.
                DiscontinueDate = local.Zero.Date;
            }

            export.Export1.Next();
          }

          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }
        else if (AsChar(import.ShowHistory.SelectChar) == 'N' || IsEmpty
          (import.ShowHistory.SelectChar))
        {
          // Else display records that are currently in effect or will be 
          // effective in future
          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadDisbursementTransactionType1())
          {
            export.Export1.Update.DisbursementTransactionType.Assign(
              entities.DisbursementTransactionType);

            if (Equal(entities.DisbursementTransactionType.DiscontinueDate,
              local.Max.Date))
            {
              export.Export1.Update.DisbursementTransactionType.
                DiscontinueDate = local.Zero.Date;
            }

            export.Export1.Next();
          }

          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }
        else
        {
          ExitState = "ZD_ACO_NE00_INVALID_INDICATOR_YN";

          var field = GetField(export.ShowHistory, "selectChar");

          field.Error = true;
        }

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "RETURN":
        if (local.Common.Count == 1)
        {
          export.Flag.Flag = "Y";
          ExitState = "ACO_NE0000_RETURN";
        }
        else if (local.Common.Count > 1)
        {
          ExitState = "ZD_ACO_NE0_INVALID_MULTIPLE_SEL1";
        }
        else
        {
          export.Flag.Flag = "Y";
          ExitState = "ACO_NE0000_RETURN";
        }

        break;
      case "NEXT":
        ExitState = "ACO_NE0000_INVALID_FORWARD";

        break;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      case "DTTM":
        if (local.Common.Count == 0 && local.Error.Count == 0)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        }

        export.Flag.Flag = "Y";
        ExitState = "ECO_XFR_TO_MTN_DISB_TRAN_TYPE";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

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

  private IEnumerable<bool> ReadDisbursementTransactionType1()
  {
    return ReadEach("ReadDisbursementTransactionType1",
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

        entities.DisbursementTransactionType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementTransactionType.Code = db.GetString(reader, 1);
        entities.DisbursementTransactionType.Name = db.GetString(reader, 2);
        entities.DisbursementTransactionType.EffectiveDate =
          db.GetDate(reader, 3);
        entities.DisbursementTransactionType.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.DisbursementTransactionType.CreatedBy =
          db.GetString(reader, 5);
        entities.DisbursementTransactionType.CreatedTmst =
          db.GetDateTime(reader, 6);
        entities.DisbursementTransactionType.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.DisbursementTransactionType.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 8);
        entities.DisbursementTransactionType.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadDisbursementTransactionType2()
  {
    return ReadEach("ReadDisbursementTransactionType2",
      null,
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.DisbursementTransactionType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementTransactionType.Code = db.GetString(reader, 1);
        entities.DisbursementTransactionType.Name = db.GetString(reader, 2);
        entities.DisbursementTransactionType.EffectiveDate =
          db.GetDate(reader, 3);
        entities.DisbursementTransactionType.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.DisbursementTransactionType.CreatedBy =
          db.GetString(reader, 5);
        entities.DisbursementTransactionType.CreatedTmst =
          db.GetDateTime(reader, 6);
        entities.DisbursementTransactionType.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.DisbursementTransactionType.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 8);
        entities.DisbursementTransactionType.Populated = true;

        return true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
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
      /// A value of DisbursementTransactionType.
      /// </summary>
      [JsonPropertyName("disbursementTransactionType")]
      public DisbursementTransactionType DisbursementTransactionType
      {
        get => disbursementTransactionType ??= new();
        set => disbursementTransactionType = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common common;
      private DisbursementTransactionType disbursementTransactionType;
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
      /// A value of DisbursementTransactionType.
      /// </summary>
      [JsonPropertyName("disbursementTransactionType")]
      public DisbursementTransactionType DisbursementTransactionType
      {
        get => disbursementTransactionType ??= new();
        set => disbursementTransactionType = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common common;
      private DisbursementTransactionType disbursementTransactionType;
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
    public DisbursementTransactionType FlowSelection
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
    private DisbursementTransactionType flowSelection;
    private NextTranInfo hidden;
    private Standard standard;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    private Common error;
    private DateWorkArea current;
    private DateWorkArea max;
    private DateWorkArea zero;
    private Common common;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of DisbursementTransactionType.
    /// </summary>
    [JsonPropertyName("disbursementTransactionType")]
    public DisbursementTransactionType DisbursementTransactionType
    {
      get => disbursementTransactionType ??= new();
      set => disbursementTransactionType = value;
    }

    private DisbursementTransactionType disbursementTransactionType;
  }
#endregion
}
