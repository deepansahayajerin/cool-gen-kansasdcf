// Program: FN_DTRL_LST_DISB_TRAN_RLN_RSN, ID: 371833966, model: 746.
// Short name: SWEDTRLP
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
/// A program: FN_DTRL_LST_DISB_TRAN_RLN_RSN.
/// </para>
/// <para>
/// RESP: DISB	
/// This will list disbursement transaction reasons
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnDtrlLstDisbTranRlnRsn: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DTRL_LST_DISB_TRAN_RLN_RSN program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDtrlLstDisbTranRlnRsn(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDtrlLstDisbTranRlnRsn.
  /// </summary>
  public FnDtrlLstDisbTranRlnRsn(IContext context, Import import, Export export):
    
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
    // 04/28/97        Ty Hill-MTW     Change Current_date
    // *********************************************
    // Set initial EXIT STATE.
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    local.Common.Count = 0;
    local.Error.Count = 0;
    local.Max.Date = UseCabSetMaximumDiscontinueDate();
    local.Current.Date = Now().Date;
    export.Hidden.Assign(import.Hidden);

    // Move all IMPORTs to EXPORTs.
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (Equal(global.Command, "XXNEXTXX"))
      {
        // ****
        // this is where you set your export value to the export hidden next 
        // tran values if the user is comming into this procedure on a next tran
        // action.
        // ****
        UseScCabNextTranGet();
        global.Command = "DISPLAY";
      }

      if (Equal(global.Command, "XXFMMENU"))
      {
        // ****
        // this is where you set your command to do whatever is necessary to do 
        // on a flow from the menu, maybe just escape....
        // ****
        // You should get this information from the Dialog Flow Diagram.  It is 
        // the SEND CMD on the propertis for a Transfer from one  procedure to
        // another.
        // *** the statement would read COMMAND IS display   *****
        // *** if the dialog flow property was display first, just add an escape
        // completely out of the procedure  ****
        global.Command = "DISPLAY";
      }
    }
    else
    {
      return;
    }

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
      export.Export1.Update.DisbursementTranRlnRsn.Assign(
        import.Import1.Item.DisbursementTranRlnRsn);

      if (AsChar(import.Import1.Item.Common.SelectChar) == 'S')
      {
        export.FlowSelection.Assign(import.Import1.Item.DisbursementTranRlnRsn);
        ++local.Common.Count;
      }
      else if (IsEmpty(import.Import1.Item.Common.SelectChar))
      {
        // Do nothing
      }
      else
      {
        // Users entered selection other than 'S'
        ++local.Error.Count;
      }

      export.Export1.Next();
    }

    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      // ****
      // ****
      // this is where you would set the local next_tran_info attributes to the 
      // import view attributes for the data to be passed to the next
      // transaction
      // ****
      // ****
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

    // Main CASE OF COMMAND.
    // to validate action level security
    if (!Equal(global.Command, "DTRM"))
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

    // Skip case of COMMAND if selection code is not 'S'
    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        // ********************
        // This will display a list of Disbursement Transaction Relation Reasons
        // ********************
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.Common.SelectChar) == 'S')
          {
            export.Export1.Update.Common.SelectChar = "";
          }
        }

        // If Show History = Y then display all record
        if (AsChar(import.ShowHistory.SelectChar) == 'Y')
        {
          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadDisbursementTranRlnRsn2())
          {
            export.Export1.Update.DisbursementTranRlnRsn.Assign(
              entities.DisbursementTranRlnRsn);

            if (Equal(entities.DisbursementTranRlnRsn.DiscontinueDate,
              local.Max.Date))
            {
              export.Export1.Update.DisbursementTranRlnRsn.DiscontinueDate =
                local.Zero.Date;
            }

            export.Export1.Next();
          }

          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }
        else if (AsChar(import.ShowHistory.SelectChar) == 'N' || IsEmpty
          (import.ShowHistory.SelectChar))
        {
          // Else display records that are currently in effect or will be in 
          // effect
          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadDisbursementTranRlnRsn1())
          {
            export.Export1.Update.DisbursementTranRlnRsn.Assign(
              entities.DisbursementTranRlnRsn);

            if (Equal(entities.DisbursementTranRlnRsn.DiscontinueDate,
              local.Max.Date))
            {
              export.Export1.Update.DisbursementTranRlnRsn.DiscontinueDate =
                local.Zero.Date;
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
      case "NEXT":
        ExitState = "ACO_NE0000_INVALID_FORWARD";

        break;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      case "RETURN":
        // This case will return to the previous menu via EXIT STATE Return
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
      case "DTRM":
        if (local.Common.Count == 0 && local.Error.Count == 0)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        }

        export.Flag.Flag = "Y";
        ExitState = "ECO_XFR_TO_MTN_DISB_TRN_RLN_RSN";

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

  private IEnumerable<bool> ReadDisbursementTranRlnRsn1()
  {
    return ReadEach("ReadDisbursementTranRlnRsn1",
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

        entities.DisbursementTranRlnRsn.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementTranRlnRsn.Code = db.GetString(reader, 1);
        entities.DisbursementTranRlnRsn.Name = db.GetString(reader, 2);
        entities.DisbursementTranRlnRsn.EffectiveDate = db.GetDate(reader, 3);
        entities.DisbursementTranRlnRsn.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.DisbursementTranRlnRsn.CreatedBy = db.GetString(reader, 5);
        entities.DisbursementTranRlnRsn.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.DisbursementTranRlnRsn.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.DisbursementTranRlnRsn.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 8);
        entities.DisbursementTranRlnRsn.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadDisbursementTranRlnRsn2()
  {
    return ReadEach("ReadDisbursementTranRlnRsn2",
      null,
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.DisbursementTranRlnRsn.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementTranRlnRsn.Code = db.GetString(reader, 1);
        entities.DisbursementTranRlnRsn.Name = db.GetString(reader, 2);
        entities.DisbursementTranRlnRsn.EffectiveDate = db.GetDate(reader, 3);
        entities.DisbursementTranRlnRsn.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.DisbursementTranRlnRsn.CreatedBy = db.GetString(reader, 5);
        entities.DisbursementTranRlnRsn.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.DisbursementTranRlnRsn.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.DisbursementTranRlnRsn.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 8);
        entities.DisbursementTranRlnRsn.Populated = true;

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
      /// A value of DisbursementTranRlnRsn.
      /// </summary>
      [JsonPropertyName("disbursementTranRlnRsn")]
      public DisbursementTranRlnRsn DisbursementTranRlnRsn
      {
        get => disbursementTranRlnRsn ??= new();
        set => disbursementTranRlnRsn = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common common;
      private DisbursementTranRlnRsn disbursementTranRlnRsn;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
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

    private Common showHistory;
    private Array<ImportGroup> import1;
    private Standard standard;
    private NextTranInfo hidden;
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
      /// A value of DisbursementTranRlnRsn.
      /// </summary>
      [JsonPropertyName("disbursementTranRlnRsn")]
      public DisbursementTranRlnRsn DisbursementTranRlnRsn
      {
        get => disbursementTranRlnRsn ??= new();
        set => disbursementTranRlnRsn = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common common;
      private DisbursementTranRlnRsn disbursementTranRlnRsn;
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
    public DisbursementTranRlnRsn FlowSelection
    {
      get => flowSelection ??= new();
      set => flowSelection = value;
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

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    private Common flag;
    private Common showHistory;
    private Array<ExportGroup> export1;
    private DisbursementTranRlnRsn flowSelection;
    private Standard standard;
    private NextTranInfo hidden;
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
    /// A value of DisbursementTranRlnRsn.
    /// </summary>
    [JsonPropertyName("disbursementTranRlnRsn")]
    public DisbursementTranRlnRsn DisbursementTranRlnRsn
    {
      get => disbursementTranRlnRsn ??= new();
      set => disbursementTranRlnRsn = value;
    }

    private DisbursementTranRlnRsn disbursementTranRlnRsn;
  }
#endregion
}
