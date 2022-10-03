// Program: LE_LEGL_LEGAL_ACTIONS_MENU, ID: 371801005, model: 746.
// Short name: SWELEGLP
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_LEGL_LEGAL_ACTIONS_MENU.
/// </para>
/// <para>
/// This procedure directs the flow to all Legal Procedures.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class LeLeglLegalActionsMenu: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_LEGL_LEGAL_ACTIONS_MENU program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeLeglLegalActionsMenu(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeLeglLegalActionsMenu.
  /// </summary>
  public LeLeglLegalActionsMenu(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------
    // Date		Developer	Description
    // 04/27/95	Dave Allen	Initial Code
    // 02/01/96         alan hackler	retro fits
    // 01/03/97	R. Marchman	Add new security/next tran.
    // 11/21/98	P McElderry	Fixes to clear screen input
    // -----------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // **** begin group A ****
    export.Hidden.Assign(import.Hidden);

    // **** end   group A ****
    MoveStandard(import.Standard, export.Standard);

    // ------------------------------------------------------------
    // if the next tran info is not equal to spaces, this implies the
    // user requested a next tran action. now validate
    // -------------------------------------------------------------
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

    if (Equal(global.Command, "XXNEXTXX") || Equal(global.Command, "XXFMMENU"))
    {
      UseScCabNextTranGet();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        // ------------------------------------------------------------
        // this is where you set your export value to the export hidden
        // next tran values if the user is comming into this procedure
        // on a next tran action.
        // This is not always necessary in the case of a menu procedure
        // ------------------------------------------------------------
      }

      return;
    }

    switch(TrimEnd(global.Command))
    {
      case "":
        // -----------------------------------------------------------
        // the command will only be spaces if  you are returning from
        // a procedure to the menu because of a security violation.
        // -----------------------------------------------------------
        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "ENTER":
        switch(import.Standard.MenuOption)
        {
          case 1:
            ExitState = "ECO_XFR_TO_LEGAL_ACTION";

            break;
          case 2:
            ExitState = "ECO_XFR_TO_LEGAL_ROLE";

            break;
          case 3:
            ExitState = "ECO_XFR_TO_LEGAL_DETAIL";

            break;
          case 4:
            ExitState = "ECO_XFR_TO_SERVICE_INFORMATION";

            break;
          case 5:
            ExitState = "ECO_XFR_TO_LEGAL_RESPONSE";

            break;
          case 6:
            ExitState = "ECO_XFR_TO_RECORD_DISCVRY_INFO";

            break;
          case 7:
            ExitState = "ECO_XFR_TO_LEGAL_HEARING";

            break;
          case 8:
            ExitState = "ECO_XFR_TO_LEGAL_APPEAL";

            break;
          case 9:
            ExitState = "ECO_XFR_MENU_TO_BKRP";

            break;
          case 10:
            ExitState = "ECO_XFR_MENU_TO_ATTY";

            break;
          case 11:
            ExitState = "CO0000_LIST_LEGL_ACT_BY_CSE_CASE";

            break;
          case 12:
            ExitState = "ECO_XFR_TO_LIST_LEGAL_ACT_BY_CC";

            break;
          case 13:
            ExitState = "ECO_XFR_TO_LIST_LEG_ACT_BY_PRSN";

            break;
          case 14:
            ExitState = "ECO_XFR_TO_LIST_CSE_CASES_BY_CC";

            break;
          case 15:
            ExitState = "ECO_XFR_TO_LIST_PRS_BY_COURT_CS";

            break;
          case 16:
            ExitState = "ECO_XFR_TO_LIST_TRIBUNALS";

            break;
          case 17:
            ExitState = "ECO_XFR_TO_EIWL";

            break;
          case 18:
            ExitState = "ECO_XFR_TO_DELETE_LEGAL_ACTION";

            break;
          default:
            ExitState = "ACO_NE0000_INVALID_OPTION";

            break;
        }

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

  private static void MoveStandard(Standard source, Standard target)
  {
    target.NextTransaction = source.NextTransaction;
    target.MenuOption = source.MenuOption;
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
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
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

    private Standard standard;
    private NextTranInfo hidden;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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

    private Standard standard;
    private NextTranInfo hidden;
  }
#endregion
}
