// Program: LE_AAP2_ADMINISTRATIVE_APPEAL_2, ID: 372605110, model: 746.
// Short name: SWEAAP2P
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_AAP2_ADMINISTRATIVE_APPEAL_2.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class LeAap2AdministrativeAppeal2: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_AAP2_ADMINISTRATIVE_APPEAL_2 program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeAap2AdministrativeAppeal2(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeAap2AdministrativeAppeal2.
  /// </summary>
  public LeAap2AdministrativeAppeal2(IContext context, Import import,
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
    // --------------------------------------------------------------------
    // Every initial development and change to that development needs
    // to be documented below.
    // --------------------------------------------------------------------
    //   Date	  Developer	Request #	Description
    // --------------------------------------------------------------------
    // 05-30-95  S. Benton			Initial development
    // 07-09-96  Govindaraj			Removed PF5 Add.  (The code,
    // 					however, has been left)
    // 06-09-97  M. D. Wheaton 		Removed datenum function calls
    // 01/07/99  J. Katz			Remove logic for unused
    // 					commands Add, AAAD, AHEA, and
    // 					Post.
    // 					Remove logic for the Clear
    // 					command.
    // 					Modify the PF key commands to
    // 					agree with functionality in
    // 					program.
    // 					Add validation logic as detailed
    // 					in the Screen Assessment and
    // 					Correction Form.
    // --------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // *********************************************
    // Move Imports to Exports
    // *********************************************
    MoveCsePersonsWorkSet(import.CsePersonsWorkSet, export.CsePersonsWorkSet);
    MoveAdministrativeAction(import.AdministrativeAction,
      export.AdministrativeAction);
    export.AdministrativeAppeal.Assign(import.AdministrativeAppeal);
    export.ActionTaken.Date = import.ActionTaken.Date;

    // *********************************************
    // Move Hidden Imports to Hidden Exports.
    // *********************************************
    export.HiddenAdministrativeAppeal.Assign(import.HiddenAdministrativeAppeal);
    MoveAdministrativeAction(import.HiddenAdministrativeAction,
      export.HiddenAdministrativeAction);
    export.HiddenActionTaken.Date = import.HiddenActionTaken.Date;
    MoveCsePersonsWorkSet(import.HiddenCsePersonsWorkSet,
      export.HiddenCsePersonsWorkSet);
    export.HiddenStandard.NextTransaction =
      import.HiddenStandard.NextTransaction;

    // ------------------------------------------------------------
    // Nexttran Logic
    // Removed logic for command 'xxnextxx' since the next tran
    // action is not allowed for AAP2.   JLK  01/07/99
    // ------------------------------------------------------------
    if (!IsEmpty(import.HiddenStandard.NextTransaction))
    {
      // ---------------------------------------------
      // User is going out of this screen to another.
      // Set up local next_tran_info for saving the
      // current values for the next screen.
      // ---------------------------------------------
      local.NextTranInfo.CsePersonNumber = export.CsePersonsWorkSet.Number;
      UseScCabNextTranPut();

      return;
    }

    // ---------------------------------------------
    // Security Logic
    // ---------------------------------------------
    if (Equal(global.Command, "DELETE") || Equal(global.Command, "UPDATE"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // -------------------------------------------------------
    // Perform validations common to both CREATEs and UPDATEs.
    // -------------------------------------------------------
    if (Equal(global.Command, "UPDATE"))
    {
      // *********************************************
      // Cross Field  EDIT LOGIC
      // *********************************************
      if (IsEmpty(export.AdministrativeAppeal.Outcome) && (
        Lt(local.Null1.Date,
        export.AdministrativeAppeal.RequestFurtherReviewDate) || !
        IsEmpty(export.AdministrativeAppeal.RequestFurtherReview) || !
        IsEmpty(export.AdministrativeAppeal.ReviewOutcome) || AsChar
        (export.AdministrativeAppeal.JudicialReviewInd) == 'Y'))
      {
        var field = GetField(export.AdministrativeAppeal, "outcome");

        field.Error = true;

        ExitState = "LE0000_APPEAL_MUST_HAVE_OUTCOME";

        return;
      }

      if (Equal(export.AdministrativeAppeal.RequestFurtherReviewDate,
        local.Null1.Date) && IsEmpty
        (export.AdministrativeAppeal.RequestFurtherReview) && (
          !IsEmpty(export.AdministrativeAppeal.ReviewOutcome) || AsChar
        (export.AdministrativeAppeal.JudicialReviewInd) == 'Y'))
      {
        var field1 =
          GetField(export.AdministrativeAppeal, "requestFurtherReviewDate");

        field1.Error = true;

        var field2 =
          GetField(export.AdministrativeAppeal, "requestFurtherReview");

        field2.Error = true;

        ExitState = "LE0000_APPEAL_MUST_REQUEST_RVW";

        return;
      }

      if (!Equal(import.AdministrativeAppeal.RequestFurtherReviewDate,
        local.Null1.Date) && IsEmpty
        (import.AdministrativeAppeal.RequestFurtherReview))
      {
        var field1 =
          GetField(export.AdministrativeAppeal, "requestFurtherReviewDate");

        field1.Error = true;

        var field2 =
          GetField(export.AdministrativeAppeal, "requestFurtherReview");

        field2.Error = true;

        ExitState = "LE0000_REQUEST_CROSS_EDIT_REQ";

        return;
      }

      if (!IsEmpty(import.AdministrativeAppeal.RequestFurtherReview) && Equal
        (import.AdministrativeAppeal.RequestFurtherReviewDate, local.Null1.Date))
        
      {
        var field1 =
          GetField(export.AdministrativeAppeal, "requestFurtherReviewDate");

        field1.Error = true;

        var field2 =
          GetField(export.AdministrativeAppeal, "requestFurtherReview");

        field2.Error = true;

        ExitState = "LE0000_REQUEST_CROSS_EDIT_REQ";

        return;
      }

      if (Lt(local.Null1.Date,
        export.AdministrativeAppeal.RequestFurtherReviewDate))
      {
        if (Lt(Now().Date, export.AdministrativeAppeal.RequestFurtherReviewDate))
          
        {
          var field =
            GetField(export.AdministrativeAppeal, "requestFurtherReviewDate");

          field.Error = true;

          ExitState = "LE0000_AAPP_FRTH_RVW_GT_CUR_DATE";

          return;
        }

        if (Lt(export.AdministrativeAppeal.RequestFurtherReviewDate,
          export.AdministrativeAppeal.RequestDate))
        {
          var field =
            GetField(export.AdministrativeAppeal, "requestFurtherReviewDate");

          field.Error = true;

          ExitState = "LE0000_AAPP_FRTH_RVW_DT_LT_REQDT";

          return;
        }

        if (Lt(export.AdministrativeAppeal.RequestFurtherReviewDate,
          export.AdministrativeAppeal.ReceivedDate))
        {
          var field =
            GetField(export.AdministrativeAppeal, "requestFurtherReviewDate");

          field.Error = true;

          ExitState = "LE0000_AAPP_FRTH_RVW_DT_LT_RCVDT";

          return;
        }

        if (IsEmpty(export.AdministrativeAppeal.ReviewOutcome) && AsChar
          (export.AdministrativeAppeal.JudicialReviewInd) == 'Y')
        {
          var field = GetField(export.AdministrativeAppeal, "reviewOutcome");

          field.Error = true;

          ExitState = "LE0000_REVIEW_OUTCOME_REQUIRED";

          return;
        }
      }

      if (!IsEmpty(export.AdministrativeAppeal.JudicialReviewInd) && AsChar
        (export.AdministrativeAppeal.JudicialReviewInd) != 'Y' && AsChar
        (export.AdministrativeAppeal.JudicialReviewInd) != 'N')
      {
        var field = GetField(export.AdministrativeAppeal, "judicialReviewInd");

        field.Error = true;

        ExitState = "LE0000_JUD_REVIEW_MUST_BE_Y_OR_N";

        return;
      }
    }

    // *********************************************
    //        P F K E Y   P R O C E S S I N G
    // *********************************************
    switch(TrimEnd(global.Command))
    {
      case "EXIT":
        // ---------------------------------------------
        // PF3  Exit
        // ---------------------------------------------
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      case "UPDATE":
        // ---------------------------------------------
        // PF6  Update
        // ---------------------------------------------
        UseResolveAdministrativeAppeal();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        }
        else if (IsExitState("LE0000_ADMINISTRATIVE_APPEAL_NF"))
        {
          UseEabRollbackCics();
          global.Command = "RTNERR";
          ExitState = "ACO_NE0000_RETURN";
        }
        else
        {
          UseEabRollbackCics();
          global.Command = "RTNERR2";
          ExitState = "ACO_NE0000_RETURN";
        }

        break;
      case "RETURN":
        // ---------------------------------------------
        // PF9  Return
        // ---------------------------------------------
        global.Command = "DISPLAY";
        ExitState = "ACO_NE0000_RETURN";

        return;
      case "DELETE":
        // ---------------------------------------------
        // PF10  Delete
        // ---------------------------------------------
        UseLeDeleteAdministrativeAppeal();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          MoveAdministrativeAction(local.ClearAdministrativeAction,
            export.AdministrativeAction);
          export.AdministrativeAppeal.Assign(local.ClearAdministrativeAppeal);
          export.ActionTaken.Date = local.Null1.Date;
          ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";
        }
        else
        {
          UseEabRollbackCics();
          global.Command = "RTNERR";
          ExitState = "ACO_NE0000_RETURN";

          return;
        }

        break;
      case "SIGNOFF":
        // ---------------------------------------------
        // PF12 Signoff
        // Sign the user off the Kessep system
        // ---------------------------------------------
        UseScCabSignoff();

        return;
      case "INVALID":
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        return;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
    }

    // -------------------------------------------------------
    // If all processing completed successfully,
    // move all imports to previous exports .
    // -------------------------------------------------------
    MoveCsePersonsWorkSet(import.CsePersonsWorkSet,
      export.HiddenCsePersonsWorkSet);
    MoveAdministrativeAction(import.AdministrativeAction,
      export.HiddenAdministrativeAction);
    export.HiddenAdministrativeAppeal.Assign(import.AdministrativeAppeal);
    export.HiddenActionTaken.Date = import.ActionTaken.Date;
  }

  private static void MoveAdministrativeAction(AdministrativeAction source,
    AdministrativeAction target)
  {
    target.Type1 = source.Type1;
    target.Description = source.Description;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseLeDeleteAdministrativeAppeal()
  {
    var useImport = new LeDeleteAdministrativeAppeal.Import();
    var useExport = new LeDeleteAdministrativeAppeal.Export();

    useImport.AdministrativeAppeal.Identifier =
      import.AdministrativeAppeal.Identifier;

    Call(LeDeleteAdministrativeAppeal.Execute, useImport, useExport);
  }

  private void UseResolveAdministrativeAppeal()
  {
    var useImport = new ResolveAdministrativeAppeal.Import();
    var useExport = new ResolveAdministrativeAppeal.Export();

    useImport.AdministrativeAppeal.Assign(import.AdministrativeAppeal);

    Call(ResolveAdministrativeAppeal.Execute, useImport, useExport);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.HiddenStandard.NextTransaction;
    useImport.NextTranInfo.CsePersonNumber = local.NextTranInfo.CsePersonNumber;

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
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of AdministrativeAction.
    /// </summary>
    [JsonPropertyName("administrativeAction")]
    public AdministrativeAction AdministrativeAction
    {
      get => administrativeAction ??= new();
      set => administrativeAction = value;
    }

    /// <summary>
    /// A value of AdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("administrativeAppeal")]
    public AdministrativeAppeal AdministrativeAppeal
    {
      get => administrativeAppeal ??= new();
      set => administrativeAppeal = value;
    }

    /// <summary>
    /// A value of ActionTaken.
    /// </summary>
    [JsonPropertyName("actionTaken")]
    public DateWorkArea ActionTaken
    {
      get => actionTaken ??= new();
      set => actionTaken = value;
    }

    /// <summary>
    /// A value of HiddenCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("hiddenCsePersonsWorkSet")]
    public CsePersonsWorkSet HiddenCsePersonsWorkSet
    {
      get => hiddenCsePersonsWorkSet ??= new();
      set => hiddenCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of HiddenAdministrativeAction.
    /// </summary>
    [JsonPropertyName("hiddenAdministrativeAction")]
    public AdministrativeAction HiddenAdministrativeAction
    {
      get => hiddenAdministrativeAction ??= new();
      set => hiddenAdministrativeAction = value;
    }

    /// <summary>
    /// A value of HiddenAdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("hiddenAdministrativeAppeal")]
    public AdministrativeAppeal HiddenAdministrativeAppeal
    {
      get => hiddenAdministrativeAppeal ??= new();
      set => hiddenAdministrativeAppeal = value;
    }

    /// <summary>
    /// A value of HiddenActionTaken.
    /// </summary>
    [JsonPropertyName("hiddenActionTaken")]
    public DateWorkArea HiddenActionTaken
    {
      get => hiddenActionTaken ??= new();
      set => hiddenActionTaken = value;
    }

    /// <summary>
    /// A value of HiddenStandard.
    /// </summary>
    [JsonPropertyName("hiddenStandard")]
    public Standard HiddenStandard
    {
      get => hiddenStandard ??= new();
      set => hiddenStandard = value;
    }

    /// <summary>
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    /// <summary>
    /// A value of HiddenSecurity.
    /// </summary>
    [JsonPropertyName("hiddenSecurity")]
    public Security2 HiddenSecurity
    {
      get => hiddenSecurity ??= new();
      set => hiddenSecurity = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private AdministrativeAction administrativeAction;
    private AdministrativeAppeal administrativeAppeal;
    private DateWorkArea actionTaken;
    private CsePersonsWorkSet hiddenCsePersonsWorkSet;
    private AdministrativeAction hiddenAdministrativeAction;
    private AdministrativeAppeal hiddenAdministrativeAppeal;
    private DateWorkArea hiddenActionTaken;
    private Standard hiddenStandard;
    private NextTranInfo hiddenNextTranInfo;
    private Security2 hiddenSecurity;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of AdministrativeAction.
    /// </summary>
    [JsonPropertyName("administrativeAction")]
    public AdministrativeAction AdministrativeAction
    {
      get => administrativeAction ??= new();
      set => administrativeAction = value;
    }

    /// <summary>
    /// A value of AdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("administrativeAppeal")]
    public AdministrativeAppeal AdministrativeAppeal
    {
      get => administrativeAppeal ??= new();
      set => administrativeAppeal = value;
    }

    /// <summary>
    /// A value of ActionTaken.
    /// </summary>
    [JsonPropertyName("actionTaken")]
    public DateWorkArea ActionTaken
    {
      get => actionTaken ??= new();
      set => actionTaken = value;
    }

    /// <summary>
    /// A value of HiddenCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("hiddenCsePersonsWorkSet")]
    public CsePersonsWorkSet HiddenCsePersonsWorkSet
    {
      get => hiddenCsePersonsWorkSet ??= new();
      set => hiddenCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of HiddenAdministrativeAction.
    /// </summary>
    [JsonPropertyName("hiddenAdministrativeAction")]
    public AdministrativeAction HiddenAdministrativeAction
    {
      get => hiddenAdministrativeAction ??= new();
      set => hiddenAdministrativeAction = value;
    }

    /// <summary>
    /// A value of HiddenAdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("hiddenAdministrativeAppeal")]
    public AdministrativeAppeal HiddenAdministrativeAppeal
    {
      get => hiddenAdministrativeAppeal ??= new();
      set => hiddenAdministrativeAppeal = value;
    }

    /// <summary>
    /// A value of HiddenActionTaken.
    /// </summary>
    [JsonPropertyName("hiddenActionTaken")]
    public DateWorkArea HiddenActionTaken
    {
      get => hiddenActionTaken ??= new();
      set => hiddenActionTaken = value;
    }

    /// <summary>
    /// A value of HiddenStandard.
    /// </summary>
    [JsonPropertyName("hiddenStandard")]
    public Standard HiddenStandard
    {
      get => hiddenStandard ??= new();
      set => hiddenStandard = value;
    }

    /// <summary>
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    /// <summary>
    /// A value of HiddenSecurity.
    /// </summary>
    [JsonPropertyName("hiddenSecurity")]
    public Security2 HiddenSecurity
    {
      get => hiddenSecurity ??= new();
      set => hiddenSecurity = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private AdministrativeAction administrativeAction;
    private AdministrativeAppeal administrativeAppeal;
    private DateWorkArea actionTaken;
    private CsePersonsWorkSet hiddenCsePersonsWorkSet;
    private AdministrativeAction hiddenAdministrativeAction;
    private AdministrativeAppeal hiddenAdministrativeAppeal;
    private DateWorkArea hiddenActionTaken;
    private Standard hiddenStandard;
    private NextTranInfo hiddenNextTranInfo;
    private Security2 hiddenSecurity;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of NextTranInfo.
    /// </summary>
    [JsonPropertyName("nextTranInfo")]
    public NextTranInfo NextTranInfo
    {
      get => nextTranInfo ??= new();
      set => nextTranInfo = value;
    }

    /// <summary>
    /// A value of ClearAdministrativeAction.
    /// </summary>
    [JsonPropertyName("clearAdministrativeAction")]
    public AdministrativeAction ClearAdministrativeAction
    {
      get => clearAdministrativeAction ??= new();
      set => clearAdministrativeAction = value;
    }

    /// <summary>
    /// A value of ClearAdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("clearAdministrativeAppeal")]
    public AdministrativeAppeal ClearAdministrativeAppeal
    {
      get => clearAdministrativeAppeal ??= new();
      set => clearAdministrativeAppeal = value;
    }

    private DateWorkArea null1;
    private NextTranInfo nextTranInfo;
    private AdministrativeAction clearAdministrativeAction;
    private AdministrativeAppeal clearAdministrativeAppeal;
  }
#endregion
}
