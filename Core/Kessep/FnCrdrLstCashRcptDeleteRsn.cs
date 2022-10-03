// Program: FN_CRDR_LST_CASH_RCPT_DELETE_RSN, ID: 371873072, model: 746.
// Short name: SWECRDRP
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
/// A program: FN_CRDR_LST_CASH_RCPT_DELETE_RSN.
/// </para>
/// <para>
/// This PRAD will list cash receipt delete reason for selection and return to 
/// the screen which called it.
///     Data to be sent back should be:
///     Export_hidden_selection cash_receipt delete_reason
///     Flow back on Exit State:
///     Return to link
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnCrdrLstCashRcptDeleteRsn: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CRDR_LST_CASH_RCPT_DELETE_RSN program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCrdrLstCashRcptDeleteRsn(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCrdrLstCashRcptDeleteRsn.
  /// </summary>
  public FnCrdrLstCashRcptDeleteRsn(IContext context, Import import,
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
    // -------------------------------------------------------------------
    // Every initial development and change to that development needs to be 
    // documented.
    // -------------------------------------------------------------------
    // -------------------------------------------------------------------
    // Date            Developer 	     Description
    // 02/16/96  H. Kennedy   Retrofits
    // 02/21/96  H. Kennedy   Flow returning to Cash Management.Menu cannot be 
    // an auto flow.  A flow also exists to the Cash Management Admin Menu.  A
    // flag is passed to this screen from the Admin Menu and is evaluated when
    // PF3 is pressed to determine which menu to flow to.
    // 12/13/96  R. Marchman  Add new security/next tran
    // 04/28/97  J Caillouet  Change Current Date
    // 08/22/97  J Caillouet  Changes for Link to CRAS
    // 10/2/98   S. Newman    Removed an escape from xxnextxx so that a display 
    // can occur when you use the next tran to CRDR.
    // 
    // 10/5/98   S. Newman    Add Successful Display exit state and edits for a
    // full and empty group export
    // 
    // 2/5/99    S. Newman    Added enter as invalid command, changed exit
    // states for CRAS
    // --------------------------------------------------------------------
    local.Current.Date = Now().Date;

    // Set initial EXIT STATE.
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);
    export.FromAdminMenu.Flag = import.FromAdminMenu.Flag;

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    local.Common.Count = 0;

    // Move all IMPORTs to EXPORTs and at the same time save the selection.
    MoveCommon(import.History, export.History);
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
      export.Export1.Update.CashReceiptDeleteReason.Assign(
        import.Import1.Item.CashReceiptDeleteReason);

      // Check to see if a selection has been made.
      if (AsChar(import.Import1.Item.Common.SelectChar) == 'S' || AsChar
        (import.Import1.Item.Common.SelectChar) == 's')
      {
        export.FlowSelection.
          Assign(import.Import1.Item.CashReceiptDeleteReason);
        ++local.Common.Count;
      }
      else if (IsEmpty(import.Import1.Item.Common.SelectChar))
      {
        // Do nothing
      }
      else
      {
        // Selections other than 'S'
        ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

        var field = GetField(export.Export1.Item.Common, "selectChar");

        field.Error = true;
      }

      export.Export1.Next();
    }

    if (Equal(global.Command, "RETURN"))
    {
      if (local.Common.Count == 1)
      {
      }
      else if (local.Common.Count > 1)
      {
        ExitState = "ZD_ACO_NE0_INVALID_MULTIPLE_SEL1";
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // Displayed all group view, even with error on selection code, before
    // skip Case of command if invalid selection (Others than 'S')
    if (IsExitState("ACO_NE0000_INVALID_SELECT_CODE1"))
    {
      return;
    }

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

    // *******************************************************************
    // Removed the escape after COMMAND IS display in the IF COMMAND IS
    // EQUAL TO xxnextxx logic.  This escape prevented a display when the
    // next tran to CRDR is used.
    // *******************************************************************
    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";

      return;
    }

    // to validate action level security
    if (Equal(global.Command, "CRAS") || Equal(global.Command, "EXIT"))
    {
    }
    else
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
      case "CRAS":
        ExitState = "ACO_NE0000_OPTION_UNAVAILABLE";

        break;
      case "HELP":
        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "DISPLAY":
        // This case will display a list of cash receipt delete reasons.
        if (AsChar(import.History.SelectChar) == 'Y')
        {
          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadCashReceiptDeleteReason2())
          {
            export.Export1.Update.CashReceiptDeleteReason.Assign(
              entities.CashReceiptDeleteReason);
            local.Max.Date = entities.CashReceiptDeleteReason.DiscontinueDate;
            export.Export1.Update.CashReceiptDeleteReason.DiscontinueDate =
              UseCabSetMaximumDiscontinueDate();
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
        else if (AsChar(import.History.SelectChar) == 'N' || IsEmpty
          (import.History.SelectChar))
        {
          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadCashReceiptDeleteReason1())
          {
            export.Export1.Update.CashReceiptDeleteReason.Assign(
              entities.CashReceiptDeleteReason);
            local.Max.Date = entities.CashReceiptDeleteReason.DiscontinueDate;
            export.Export1.Update.CashReceiptDeleteReason.DiscontinueDate =
              UseCabSetMaximumDiscontinueDate();
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
          var field = GetField(export.History, "selectChar");

          field.Error = true;

          ExitState = "ZD_ACO_NE00_INVALID_INDICATOR_YN";
        }

        break;
      case "EXIT":
        // ************************************************
        // *Evaluate the Admin menu flag and determine    *
        // *which menu to return to.                      *
        // ************************************************
        if (AsChar(export.FromAdminMenu.Flag) == 'Y')
        {
          ExitState = "ECO_LNK_RETURN_TO_MENU";
        }
        else
        {
          ExitState = "ECO_XFR_TO_CASH_MNGMNT_MENU";
        }

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

  private IEnumerable<bool> ReadCashReceiptDeleteReason1()
  {
    return ReadEach("ReadCashReceiptDeleteReason1",
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

        entities.CashReceiptDeleteReason.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDeleteReason.Code = db.GetString(reader, 1);
        entities.CashReceiptDeleteReason.Name = db.GetString(reader, 2);
        entities.CashReceiptDeleteReason.EffectiveDate = db.GetDate(reader, 3);
        entities.CashReceiptDeleteReason.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.CashReceiptDeleteReason.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptDeleteReason2()
  {
    return ReadEach("ReadCashReceiptDeleteReason2",
      null,
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.CashReceiptDeleteReason.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDeleteReason.Code = db.GetString(reader, 1);
        entities.CashReceiptDeleteReason.Name = db.GetString(reader, 2);
        entities.CashReceiptDeleteReason.EffectiveDate = db.GetDate(reader, 3);
        entities.CashReceiptDeleteReason.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.CashReceiptDeleteReason.Populated = true;

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
      /// A value of CashReceiptDeleteReason.
      /// </summary>
      [JsonPropertyName("cashReceiptDeleteReason")]
      public CashReceiptDeleteReason CashReceiptDeleteReason
      {
        get => cashReceiptDeleteReason ??= new();
        set => cashReceiptDeleteReason = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 52;

      private Common common;
      private CashReceiptDeleteReason cashReceiptDeleteReason;
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
    /// A value of History.
    /// </summary>
    [JsonPropertyName("history")]
    public Common History
    {
      get => history ??= new();
      set => history = value;
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
    private Common history;
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
      /// A value of CashReceiptDeleteReason.
      /// </summary>
      [JsonPropertyName("cashReceiptDeleteReason")]
      public CashReceiptDeleteReason CashReceiptDeleteReason
      {
        get => cashReceiptDeleteReason ??= new();
        set => cashReceiptDeleteReason = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 52;

      private Common common;
      private CashReceiptDeleteReason cashReceiptDeleteReason;
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
    /// A value of History.
    /// </summary>
    [JsonPropertyName("history")]
    public Common History
    {
      get => history ??= new();
      set => history = value;
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
    public CashReceiptDeleteReason FlowSelection
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

    private Common fromAdminMenu;
    private Common history;
    private Array<ExportGroup> export1;
    private CashReceiptDeleteReason flowSelection;
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
    /// A value of CashReceiptDeleteReason.
    /// </summary>
    [JsonPropertyName("cashReceiptDeleteReason")]
    public CashReceiptDeleteReason CashReceiptDeleteReason
    {
      get => cashReceiptDeleteReason ??= new();
      set => cashReceiptDeleteReason = value;
    }

    private CashReceiptDeleteReason cashReceiptDeleteReason;
  }
#endregion
}
