// Program: FN_RSDL_LST_CASH_RCPT_DTL_STATUS, ID: 371785395, model: 746.
// Short name: SWERSDLP
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
/// A program: FN_RSDL_LST_CASH_RCPT_DTL_STATUS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnRsdlLstCashRcptDtlStatus: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_RSDL_LST_CASH_RCPT_DTL_STATUS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnRsdlLstCashRcptDtlStatus(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnRsdlLstCashRcptDtlStatus.
  /// </summary>
  public FnRsdlLstCashRcptDtlStatus(IContext context, Import import,
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
    // -------------------------------------------------------------------
    // Date 	   Developer 	     Description
    // 02/13/96   H. Kennedy-MTW    Retrofits
    // 12/13/96   R. Marchman	     Add new security/next tran
    // 04/28/97   Sheraz Malik	     Change Current Date
    // 10/07/98   S. Newman
    // Removed an escape from xxnextxx so that a display can occur when you next
    // tran to RSDL.  Alphabetized display list of Cash Receipt Detail
    // Statuses.  Revised Status Code filter.  Added edits for full and empty
    // group exports.  Put cursor on status code instead of next tran.  Added a
    // 'Display Successful' message after display.
    // 
    // 10/12/98   S. Newman         Removed an escape from the Case of
    // Group_import _detail  ief_supplied select_char which prevented a full
    // display of the group.  Also, checked for All OK and then escaped if it
    // wasn't.
    // -------------------------------------------------------------------
    // ***********************
    // Set Initial EXIT STATE.
    // ***********************
    local.DateWorkArea.Date = Now().Date;
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // ************************
    // Move Imports to Exports.
    // ************************
    export.CashReceiptDetailStatus.Code = import.CashReceiptDetailStatus.Code;
    export.ShowHistory.Flag = import.ShowHistory.Flag;
    export.MenuIndicator.Flag = import.MenuIndicator.Flag;
    local.Common.Count = 0;

    export.Export1.Index = 0;
    export.Export1.Clear();

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (export.Export1.IsFull)
      {
        break;
      }

      export.Export1.Update.DetailCashReceiptDetailStatus.Assign(
        import.Import1.Item.DetailCashReceiptDetailStatus);
      export.Export1.Update.DetailCommon.SelectChar =
        import.Import1.Item.DetailCommon.SelectChar;

      switch(AsChar(import.Import1.Item.DetailCommon.SelectChar))
      {
        case ' ':
          break;
        case '+':
          break;
        case 'S':
          export.CashReceiptDetailStatus.Code =
            export.Export1.Item.DetailCashReceiptDetailStatus.Code;
          ++local.Common.Count;

          break;
        default:
          var field = GetField(export.Export1.Item.DetailCommon, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

          break;
      }

      export.Export1.Next();
    }

    if (local.Common.Count > 1)
    {
      ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

      return;
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ************************
    // Next Tran/Security logic
    // ************************
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      // ********************
      // No views
      // to populate
      // 
      // ********************
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

    // ****************************************************************************
    // Removed the escape after COMMAND IS display in the IF COMMAND IS EQUAL to
    // xxnextxx logic.  This escape prevented a display when the next tran to
    // RSTL is used.
    // **********************************************************************
    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ********************
      // 
      // No views to populate
      // 
      // ********************
      UseScCabNextTranGet();
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";

      return;
    }

    // to validate action level security
    UseScCabTestSecurity();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    // Main CASE OF COMMAND.
    switch(TrimEnd(global.Command))
    {
      case "":
        break;
      case "MAINTAIN":
        ExitState = "ACO_NE0000_OPTION_UNAVAILABLE";

        break;
      case "DISPLAY":
        if (AsChar(export.ShowHistory.Flag) == 'Y')
        {
          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadCashReceiptDetailStatus2())
          {
            export.Export1.Update.DetailCashReceiptDetailStatus.Assign(
              entities.CashReceiptDetailStatus);
            local.DateWorkArea.Date =
              entities.CashReceiptDetailStatus.DiscontinueDate;
            export.Export1.Update.DetailCashReceiptDetailStatus.
              DiscontinueDate = UseCabSetMaximumDiscontinueDate();
            export.Export1.Next();
          }

          if (export.Export1.IsFull)
          {
            ExitState = "ACO_NI0000_LIST_IS_FULL";

            return;
          }

          if (export.Export1.IsEmpty)
          {
            ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";

            return;
          }

          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }
        else
        {
          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadCashReceiptDetailStatus1())
          {
            export.Export1.Update.DetailCashReceiptDetailStatus.Assign(
              entities.CashReceiptDetailStatus);
            local.DateWorkArea.Date =
              entities.CashReceiptDetailStatus.DiscontinueDate;
            export.Export1.Update.DetailCashReceiptDetailStatus.
              DiscontinueDate = UseCabSetMaximumDiscontinueDate();
            export.Export1.Next();
          }

          if (export.Export1.IsFull)
          {
            ExitState = "ACO_NI0000_LIST_IS_FULL";

            return;
          }

          if (export.Export1.IsEmpty)
          {
            ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";

            return;
          }

          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }

        break;
      case "HELP":
        break;
      case "EXIT":
        if (AsChar(export.MenuIndicator.Flag) == 'Y')
        {
          ExitState = "ECO_LNK_RETURN_TO_MENU";
        }
        else
        {
          ExitState = "ECO_XFR_TO_CASH_MNGMNT_MENU";
        }

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

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
      default:
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

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

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

  private IEnumerable<bool> ReadCashReceiptDetailStatus1()
  {
    return ReadEach("ReadCashReceiptDetailStatus1",
      (db, command) =>
      {
        db.SetString(command, "code", export.CashReceiptDetailStatus.Code);
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.CashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetailStatus.Code = db.GetString(reader, 1);
        entities.CashReceiptDetailStatus.Name = db.GetString(reader, 2);
        entities.CashReceiptDetailStatus.EffectiveDate = db.GetDate(reader, 3);
        entities.CashReceiptDetailStatus.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.CashReceiptDetailStatus.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptDetailStatus2()
  {
    return ReadEach("ReadCashReceiptDetailStatus2",
      (db, command) =>
      {
        db.SetString(command, "code", export.CashReceiptDetailStatus.Code);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.CashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetailStatus.Code = db.GetString(reader, 1);
        entities.CashReceiptDetailStatus.Name = db.GetString(reader, 2);
        entities.CashReceiptDetailStatus.EffectiveDate = db.GetDate(reader, 3);
        entities.CashReceiptDetailStatus.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.CashReceiptDetailStatus.Populated = true;

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
      /// A value of DetailCommon.
      /// </summary>
      [JsonPropertyName("detailCommon")]
      public Common DetailCommon
      {
        get => detailCommon ??= new();
        set => detailCommon = value;
      }

      /// <summary>
      /// A value of DetailCashReceiptDetailStatus.
      /// </summary>
      [JsonPropertyName("detailCashReceiptDetailStatus")]
      public CashReceiptDetailStatus DetailCashReceiptDetailStatus
      {
        get => detailCashReceiptDetailStatus ??= new();
        set => detailCashReceiptDetailStatus = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 196;

      private Common detailCommon;
      private CashReceiptDetailStatus detailCashReceiptDetailStatus;
    }

    /// <summary>
    /// A value of MenuIndicator.
    /// </summary>
    [JsonPropertyName("menuIndicator")]
    public Common MenuIndicator
    {
      get => menuIndicator ??= new();
      set => menuIndicator = value;
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
    /// A value of CashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatus")]
    public CashReceiptDetailStatus CashReceiptDetailStatus
    {
      get => cashReceiptDetailStatus ??= new();
      set => cashReceiptDetailStatus = value;
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

    private Common menuIndicator;
    private Common showHistory;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
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
      /// A value of DetailCommon.
      /// </summary>
      [JsonPropertyName("detailCommon")]
      public Common DetailCommon
      {
        get => detailCommon ??= new();
        set => detailCommon = value;
      }

      /// <summary>
      /// A value of DetailCashReceiptDetailStatus.
      /// </summary>
      [JsonPropertyName("detailCashReceiptDetailStatus")]
      public CashReceiptDetailStatus DetailCashReceiptDetailStatus
      {
        get => detailCashReceiptDetailStatus ??= new();
        set => detailCashReceiptDetailStatus = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 196;

      private Common detailCommon;
      private CashReceiptDetailStatus detailCashReceiptDetailStatus;
    }

    /// <summary>
    /// A value of MenuIndicator.
    /// </summary>
    [JsonPropertyName("menuIndicator")]
    public Common MenuIndicator
    {
      get => menuIndicator ??= new();
      set => menuIndicator = value;
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
    /// A value of CashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatus")]
    public CashReceiptDetailStatus CashReceiptDetailStatus
    {
      get => cashReceiptDetailStatus ??= new();
      set => cashReceiptDetailStatus = value;
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

    private Common menuIndicator;
    private Common showHistory;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private Array<ExportGroup> export1;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    private DateWorkArea current;
    private DateWorkArea null1;
    private Common common;
    private DateWorkArea dateWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatus")]
    public CashReceiptDetailStatus CashReceiptDetailStatus
    {
      get => cashReceiptDetailStatus ??= new();
      set => cashReceiptDetailStatus = value;
    }

    private CashReceiptDetailStatus cashReceiptDetailStatus;
  }
#endregion
}
