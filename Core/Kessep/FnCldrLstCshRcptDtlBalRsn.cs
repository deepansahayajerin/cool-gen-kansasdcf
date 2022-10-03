// Program: FN_CLDR_LST_CSH_RCPT_DTL_BAL_RSN, ID: 371872411, model: 746.
// Short name: SWECLDRP
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
/// A program: FN_CLDR_LST_CSH_RCPT_DTL_BAL_RSN.
/// </para>
/// <para>
/// This PRAD will list Cash Receipt Detail Relation Reasons for selection and 
/// return to the screen which called this.
/// Data send back will be:
///      export_hidden_selection_cash_receipt_detail_rln_rsn
/// Flow back on exit state:
///      Return to link
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnCldrLstCshRcptDtlBalRsn: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CLDR_LST_CSH_RCPT_DTL_BAL_RSN program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCldrLstCshRcptDtlBalRsn(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCldrLstCshRcptDtlBalRsn.
  /// </summary>
  public FnCldrLstCshRcptDtlBalRsn(IContext context, Import import,
    Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // --------------------------------------------
    // Every initial development and change to that
    // development needs to be documented.
    // --------------------------------------------
    // --------------------------------------------------------------------------
    // Date 	  	Developer Name	      Description
    // 02/03/96	H. Kennedy-MTW        Retrofits
    // 12/13/96	R. Marchman	      Add new Security/next tran
    // 04/28/97	JF. Caillouet	      Change Current Date
    // 10/1/98         S. Newman             Remove escape from xxnextxx command
    // so that a display can occur when you next tran to CLDR.
    // 10/5
    // /98         S. Newman             Add 'Successful Display' exit state and
    // changed Invalid Command Exit State to one with system identifier.  Also,
    // added edits for empty and full group-exports.      2/4/99          S.
    // Newman             Added maintain to security, made enter an invalid
    // command and changed screen literal from maintain to CMDR
    // ----------------------------------------------------------------------------
    local.Current.Date = Now().Date;

    // Set initial EXIT STATE.
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    local.Common.Count = 0;
    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    // Move all IMPORTs to EXPORTs and at the same time, save the first 
    // selection.
    MoveCommon(import.ShowHistory, export.ShowHistory);

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
      export.Export1.Update.CashReceiptDetailRlnRsn.Assign(
        import.Import1.Item.CashReceiptDetailRlnRsn);

      // Check to see if a selection has been made.
      if (AsChar(import.Import1.Item.Common.SelectChar) == 'S' || AsChar
        (import.Import1.Item.Common.SelectChar) == 's')
      {
        export.FlowSelection.
          Assign(import.Import1.Item.CashReceiptDetailRlnRsn);
        ++local.Common.Count;
      }
      else if (IsEmpty(import.Import1.Item.Common.SelectChar))
      {
        // Do nothing
      }
      else
      {
        // User enter selection other than 'S'
        ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

        var field = GetField(export.Export1.Item.Common, "selectChar");

        field.Error = true;
      }

      export.Export1.Next();
    }

    // If user selected other than 'S' on selection field then skip
    // Case of Command.
    if (IsExitState("ACO_NE0000_INVALID_SELECT_CODE1"))
    {
      return;
    }

    // *****
    // Next Tran Logic
    // *****
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      // ****
      // No valid values to set
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

    // *******************************************************************
    // Removed the escape after COMMAND IS display in the IF COMMAND IS
    // EQUAL TO xxnextxx logic.  This escape prevented a display when the
    // next tran to CLDR is used.
    // *******************************************************************
    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ****
      // No valid values to set
      // ****
      UseScCabNextTranGet();
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";

      return;
    }

    // to validate action level security
    if (!Equal(global.Command, "ENTER") && !Equal(global.Command, "MAINTAIN"))
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
      case "":
        break;
      case "MAINTAIN":
        ExitState = "ACO_NE0000_OPTION_UNAVAILABLE";

        break;
      case "HELP":
        break;
      case "RETURN":
        if (local.Common.Count == 1)
        {
          // Successful return, one selection made
        }
        else if (local.Common.Count > 1)
        {
          // User selected more than one selection
          ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

          return;
        }
        else
        {
        }

        ExitState = "ACO_NE0000_RETURN";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "DISPLAY":
        // This case will display a list of detail balance reasons sorted by 
        // ascending reason codes
        if (AsChar(import.ShowHistory.SelectChar) == 'Y')
        {
          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadCashReceiptDetailRlnRsn2())
          {
            export.Export1.Update.CashReceiptDetailRlnRsn.Assign(
              entities.CashReceiptDetailRlnRsn);

            if (Equal(entities.CashReceiptDetailRlnRsn.DiscontinueDate,
              local.Max.Date))
            {
              export.Export1.Update.CashReceiptDetailRlnRsn.DiscontinueDate =
                local.Zero.Date;
            }

            if (export.Export1.IsFull)
            {
              ExitState = "ACO_NI0000_LIST_EXCEED_MAX_LNGTH";
              export.Export1.Next();

              return;
            }

            ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
            export.Export1.Next();
          }

          if (export.Export1.IsEmpty)
          {
            ExitState = "ACO_NI0000_NO_INFO_FOR_LIST";
          }
        }
        else if (AsChar(import.ShowHistory.SelectChar) == 'N' || IsEmpty
          (import.ShowHistory.SelectChar))
        {
          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadCashReceiptDetailRlnRsn1())
          {
            export.Export1.Update.CashReceiptDetailRlnRsn.Assign(
              entities.CashReceiptDetailRlnRsn);

            if (Equal(entities.CashReceiptDetailRlnRsn.DiscontinueDate,
              local.Max.Date))
            {
              export.Export1.Update.CashReceiptDetailRlnRsn.DiscontinueDate =
                local.Zero.Date;
            }

            if (export.Export1.IsFull)
            {
              ExitState = "ACO_NI0000_LIST_EXCEED_MAX_LNGTH";
              export.Export1.Next();

              return;
            }

            ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
            export.Export1.Next();
          }

          if (export.Export1.IsEmpty)
          {
            ExitState = "ACO_NI0000_NO_INFO_FOR_LIST";
          }
        }
        else
        {
          var field = GetField(export.ShowHistory, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";
        }

        break;
      case "EXIT":
        ExitState = "ECO_XFR_TO_CASH_MGMNT_MENU";

        break;
      case "NEXT":
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      default:
        if (Equal(global.Command, "ENTER"))
        {
          ExitState = "ACO_NE0000_INVALID_COMMAND";
        }
        else
        {
          ExitState = "ACO_NE0000_INVALID_PF_KEY";
        }

        break;
    }
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Flag = source.Flag;
    target.SelectChar = source.SelectChar;
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

  private IEnumerable<bool> ReadCashReceiptDetailRlnRsn1()
  {
    return ReadEach("ReadCashReceiptDetailRlnRsn1",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.CashReceiptDetailRlnRsn.SequentialIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetailRlnRsn.Code = db.GetString(reader, 1);
        entities.CashReceiptDetailRlnRsn.Name = db.GetString(reader, 2);
        entities.CashReceiptDetailRlnRsn.EffectiveDate = db.GetDate(reader, 3);
        entities.CashReceiptDetailRlnRsn.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.CashReceiptDetailRlnRsn.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptDetailRlnRsn2()
  {
    return ReadEach("ReadCashReceiptDetailRlnRsn2",
      null,
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.CashReceiptDetailRlnRsn.SequentialIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetailRlnRsn.Code = db.GetString(reader, 1);
        entities.CashReceiptDetailRlnRsn.Name = db.GetString(reader, 2);
        entities.CashReceiptDetailRlnRsn.EffectiveDate = db.GetDate(reader, 3);
        entities.CashReceiptDetailRlnRsn.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.CashReceiptDetailRlnRsn.Populated = true;

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
      /// A value of CashReceiptDetailRlnRsn.
      /// </summary>
      [JsonPropertyName("cashReceiptDetailRlnRsn")]
      public CashReceiptDetailRlnRsn CashReceiptDetailRlnRsn
      {
        get => cashReceiptDetailRlnRsn ??= new();
        set => cashReceiptDetailRlnRsn = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 52;

      private Common common;
      private CashReceiptDetailRlnRsn cashReceiptDetailRlnRsn;
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
      /// A value of CashReceiptDetailRlnRsn.
      /// </summary>
      [JsonPropertyName("cashReceiptDetailRlnRsn")]
      public CashReceiptDetailRlnRsn CashReceiptDetailRlnRsn
      {
        get => cashReceiptDetailRlnRsn ??= new();
        set => cashReceiptDetailRlnRsn = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 52;

      private Common common;
      private CashReceiptDetailRlnRsn cashReceiptDetailRlnRsn;
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
    public CashReceiptDetailRlnRsn FlowSelection
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

    private Common showHistory;
    private Array<ExportGroup> export1;
    private CashReceiptDetailRlnRsn flowSelection;
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
    /// A value of CashReceiptDetailRlnRsn.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailRlnRsn")]
    public CashReceiptDetailRlnRsn CashReceiptDetailRlnRsn
    {
      get => cashReceiptDetailRlnRsn ??= new();
      set => cashReceiptDetailRlnRsn = value;
    }

    private CashReceiptDetailRlnRsn cashReceiptDetailRlnRsn;
  }
#endregion
}
