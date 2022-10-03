// Program: FN_CLBR_LST_CASH_RCPT_BAL_RSNS, ID: 371874974, model: 746.
// Short name: SWECLBRP
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
/// A program: FN_CLBR_LST_CASH_RCPT_BAL_RSNS.
/// </para>
/// <para>
/// RESP: CASHMGMT
/// This procedure will list cash receipt balance reasons for selection.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnClbrLstCashRcptBalRsns: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CLBR_LST_CASH_RCPT_BAL_RSNS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnClbrLstCashRcptBalRsns(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnClbrLstCashRcptBalRsns.
  /// </summary>
  public FnClbrLstCashRcptBalRsns(IContext context, Import import, Export export)
    :
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
    // ---------------------------------------------------------------------------
    // Date 	  	Developer Name		Request #	Description
    // 02/07/96	H. Kennedy-MTW	Retrofits
    // 
    // 02/21/96	H. Kennedy-MTW	Flow returning to Cash Management
    // Menu cannot be an auto flow.  A flow also exists to the
    // Cash Management Admin Menu.  A flag is passed to this
    // screen from the Admin Menu and is evaluated when PF3 is
    // pressed to determine which menu to flow to.
    // 12/13/96	R. Marchman	Add new security/next tran
    // 04/28/97        G P Kim         Change current date
    // 
    // 10/01/98        S Newman        Removed an escape from xxnextxx so that a
    // display can occur when you use the next tran to CLBR
    // 
    // 10/05/98        S. Newman       Added Successful Display exit state
    // 2/4/99          S. Newman       Added
    // Maintain to security, Enter as an invalid command, and changed screen
    // literal from maintain to CMBR
    // ----------------------------------------------------------------------------
    local.Current.Date = Now().Date;

    // Set initial EXIT STATE.
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // Move all IMPORTs to EXPORTs.
    export.ShowHistory.Flag = import.ShowHistory.Flag;
    export.FromAdminMenu.Flag = import.FromAdminMenu.Flag;

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
      export.Export1.Update.CashReceiptRlnRsn.Assign(
        import.Import1.Item.CashReceiptRlnRsn);
      export.Export1.Next();
    }

    // *****
    // Next Tran/Security logic
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
      // No views to populate
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
    // next tran to CLBR is used.
    // *******************************************************************
    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ****
      // No views to populate
      // ****
      UseScCabNextTranGet();
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";

      return;
    }

    // Check to see if a selection has been made
    local.SelectionCounter.Count = 0;

    for(export.Export1.Index = 0; export.Export1.Index < export.Export1.Count; ++
      export.Export1.Index)
    {
      if (!IsEmpty(export.Export1.Item.Common.SelectChar))
      {
        if (AsChar(export.Export1.Item.Common.SelectChar) == 'S')
        {
          ++local.SelectionCounter.Count;

          var field = GetField(export.Export1.Item.Common, "selectChar");

          field.Error = true;

          export.HiddenSelection.Assign(export.Export1.Item.CashReceiptRlnRsn);
        }
        else
        {
          // Invalid selection made.  Must use "S" in selection field to pick 
          // cash receipt balance reason.
          var field = GetField(export.Export1.Item.Common, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

          return;
        }
      }
    }

    if (local.SelectionCounter.Count > 1)
    {
      ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

      return;
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
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
        if (local.SelectionCounter.Count == 1)
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (!IsEmpty(export.Export1.Item.Common.SelectChar))
            {
              export.Export1.Update.Common.SelectChar = "";
            }
          }

          ExitState = "ECO_LNK_TO_CMBR";
        }
        else
        {
          ExitState = "ACO_NE0000_SELECTION_REQUIRED";
        }

        break;
      case "EXIT":
        // *****
        // Evaluate the value of the from admin menu flag to determine to which 
        // menu to flow.
        // *****
        if (AsChar(export.FromAdminMenu.Flag) == 'Y')
        {
          ExitState = "ECO_LNK_RETURN_TO_MENU";
        }
        else
        {
          ExitState = "ECO_XFR_TO_CASH_MNGMNT_MENU";
        }

        break;
      case "DISPLAY":
        if (local.SelectionCounter.Count > 0)
        {
          ExitState = "NO_SELECT_WITH_DISPLAY_OPTION";

          return;
        }

        // READ EACH for selection list.
        if (AsChar(import.ShowHistory.Flag) == 'Y')
        {
          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadCashReceiptRlnRsn2())
          {
            export.Export1.Update.CashReceiptRlnRsn.Assign(
              entities.CashReceiptRlnRsn);
            local.ReceiptRlnReason.Date =
              export.Export1.Item.CashReceiptRlnRsn.DiscontinueDate;
            UseCabSetMaximumDiscontinueDate();
            export.Export1.Update.CashReceiptRlnRsn.DiscontinueDate =
              local.ReceiptRlnReason.Date;
            export.Export1.Next();
          }

          if (export.Export1.IsEmpty)
          {
            ExitState = "ACO_NI0000_NO_INFO_FOR_LIST";

            return;
          }

          if (export.Export1.IsFull)
          {
            ExitState = "ACO_NI0000_LIST_EXCEED_MAX_LNGTH";

            return;
          }

          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }
        else
        {
          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadCashReceiptRlnRsn1())
          {
            export.Export1.Update.CashReceiptRlnRsn.Assign(
              entities.CashReceiptRlnRsn);
            local.ReceiptRlnReason.Date =
              export.Export1.Item.CashReceiptRlnRsn.DiscontinueDate;
            UseCabSetMaximumDiscontinueDate();
            export.Export1.Update.CashReceiptRlnRsn.DiscontinueDate =
              local.ReceiptRlnReason.Date;
            export.Export1.Next();
          }

          if (export.Export1.IsEmpty)
          {
            ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
            ExitState = "ACO_NI0000_NO_INFO_FOR_LIST";

            return;
          }

          if (export.Export1.IsFull)
          {
            ExitState = "ACO_NI0000_LIST_EXCEED_MAX_LNGTH";

            return;
          }

          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
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

    // Add any common logic that must occur at
    // the end of every pass.
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

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.ReceiptRlnReason.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.ReceiptRlnReason.Date = useExport.DateWorkArea.Date;
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

  private IEnumerable<bool> ReadCashReceiptRlnRsn1()
  {
    return ReadEach("ReadCashReceiptRlnRsn1",
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

        entities.CashReceiptRlnRsn.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptRlnRsn.Code = db.GetString(reader, 1);
        entities.CashReceiptRlnRsn.Name = db.GetString(reader, 2);
        entities.CashReceiptRlnRsn.EffectiveDate = db.GetDate(reader, 3);
        entities.CashReceiptRlnRsn.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.CashReceiptRlnRsn.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptRlnRsn2()
  {
    return ReadEach("ReadCashReceiptRlnRsn2",
      null,
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.CashReceiptRlnRsn.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptRlnRsn.Code = db.GetString(reader, 1);
        entities.CashReceiptRlnRsn.Name = db.GetString(reader, 2);
        entities.CashReceiptRlnRsn.EffectiveDate = db.GetDate(reader, 3);
        entities.CashReceiptRlnRsn.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.CashReceiptRlnRsn.Populated = true;

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
      /// A value of CashReceiptRlnRsn.
      /// </summary>
      [JsonPropertyName("cashReceiptRlnRsn")]
      public CashReceiptRlnRsn CashReceiptRlnRsn
      {
        get => cashReceiptRlnRsn ??= new();
        set => cashReceiptRlnRsn = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 90;

      private Common common;
      private CashReceiptRlnRsn cashReceiptRlnRsn;
    }

    /// <summary>
    /// A value of FromAdminMenu.
    /// </summary>
    [JsonPropertyName("fromAdminMenu")]
    public Common FromAdminMenu
    {
      get => fromAdminMenu ??= new();
      set => fromAdminMenu = value;
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

    private Common fromAdminMenu;
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
      /// A value of CashReceiptRlnRsn.
      /// </summary>
      [JsonPropertyName("cashReceiptRlnRsn")]
      public CashReceiptRlnRsn CashReceiptRlnRsn
      {
        get => cashReceiptRlnRsn ??= new();
        set => cashReceiptRlnRsn = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 90;

      private Common common;
      private CashReceiptRlnRsn cashReceiptRlnRsn;
    }

    /// <summary>
    /// A value of FromAdminMenu.
    /// </summary>
    [JsonPropertyName("fromAdminMenu")]
    public Common FromAdminMenu
    {
      get => fromAdminMenu ??= new();
      set => fromAdminMenu = value;
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
    /// A value of HiddenSelection.
    /// </summary>
    [JsonPropertyName("hiddenSelection")]
    public CashReceiptRlnRsn HiddenSelection
    {
      get => hiddenSelection ??= new();
      set => hiddenSelection = value;
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

    private Common fromAdminMenu;
    private Common showHistory;
    private Array<ExportGroup> export1;
    private CashReceiptRlnRsn hiddenSelection;
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
    /// A value of ReceiptRlnReason.
    /// </summary>
    [JsonPropertyName("receiptRlnReason")]
    public DateWorkArea ReceiptRlnReason
    {
      get => receiptRlnReason ??= new();
      set => receiptRlnReason = value;
    }

    /// <summary>
    /// A value of SelectionCounter.
    /// </summary>
    [JsonPropertyName("selectionCounter")]
    public Common SelectionCounter
    {
      get => selectionCounter ??= new();
      set => selectionCounter = value;
    }

    private DateWorkArea current;
    private DateWorkArea receiptRlnReason;
    private Common selectionCounter;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CashReceiptRlnRsn.
    /// </summary>
    [JsonPropertyName("cashReceiptRlnRsn")]
    public CashReceiptRlnRsn CashReceiptRlnRsn
    {
      get => cashReceiptRlnRsn ??= new();
      set => cashReceiptRlnRsn = value;
    }

    private CashReceiptRlnRsn cashReceiptRlnRsn;
  }
#endregion
}
