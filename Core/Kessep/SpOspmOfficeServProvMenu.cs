// Program: SP_OSPM_OFFICE_SERV_PROV_MENU, ID: 371783454, model: 746.
// Short name: SWEOSPMP
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_OSPM_OFFICE_SERV_PROV_MENU.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpOspmOfficeServProvMenu: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_OSPM_OFFICE_SERV_PROV_MENU program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpOspmOfficeServProvMenu(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpOspmOfficeServProvMenu.
  /// </summary>
  public SpOspmOfficeServProvMenu(IContext context, Import import, Export export)
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
    // ---------------------------------------------
    //        M A I N T E N A N C E   L O G
    //  Date    Developer       request #    Description
    // 02/02/96 Alan Hackler                retro fits
    // 10/15/96 R. Marchman		     add new security and next tran
    // ---------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);
    local.NextTranInfo.Assign(import.Hidden);
    MoveStandard(import.Standard, export.Standard);

    if (Equal(global.Command, "SIGNOFF"))
    {
      UseScCabSignoff();

      return;
    }
    else if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
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
      if (export.Standard.MenuOption > 0)
      {
        goto Test;
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
      }

      return;
    }

Test:

    if (Equal(global.Command, "XXNEXTXX") || Equal(global.Command, "XXFMMENU"))
    {
      if (Equal(global.Command, "XXFMMENU"))
      {
        return;
      }

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        // ****
        // this is where you set your export value to the export hidden next 
        // tran values if the user is comming into this procedure on a next tran
        // action.
        // This is not always necessary in the case of a menu procedure..
        // ****
        UseScCabNextTranGet();
      }

      return;
    }

    UseScCabTestSecurity();

    switch(TrimEnd(global.Command))
    {
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "":
        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "ENTER":
        switch(import.Standard.MenuOption)
        {
          case 1:
            ExitState = "ECO_XFR_TO_OFFICE_MAINTENANCE";

            break;
          case 2:
            ExitState = "ECO_XFR_TO_LIST_OFFICE";

            break;
          case 3:
            ExitState = "ECO_XFR_TO_SERVIC_PROVIDR_MAINT";

            break;
          case 4:
            ExitState = "ECO_XFR_TO_LIST_SERVICE_PROVIDR";

            break;
          case 5:
            ExitState = "ECO_XFR_TO_OFF_SERV_PROV_MAINT";

            break;
          case 6:
            ExitState = "ECO_XFR_TO_CASELOAD_ASSIGN_INQ";

            break;
          case 7:
            ExitState = "ECO_XFR_TO_ASSIGNMENT_PLAN";

            break;
          case 8:
            ExitState = "ECO_XFR_2_SUPERVISORY_HIERARCHY";

            break;
          case 9:
            ExitState = "ECO_XFR_TO_PROGRAM_MAINTENANCE";

            break;
          case 10:
            ExitState = "ECO_XFR_TO_CSE_ORGANIZ_MAINT";

            break;
          case 11:
            ExitState = "ECO_XFR_TO_CSE_ORG_HIER_MAINT";

            break;
          case 12:
            ExitState = "ECO_XFR_TO_CSE_JUD_DIST_HIST";

            break;
          case 13:
            ExitState = "ECO_XFR_TO_COUNTY_SERVICE_MAINT";

            break;
          case 14:
            ExitState = "ECO_XFR_2_LIST_OFFIC_4_SRV_PRVD";

            break;
          case 15:
            ExitState = "ECO_XFR_TO_FTE_MAINT";

            break;
          case 16:
            ExitState = "ECO_XFR_TO_DASHBOARD_GOAL_MAINT";

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

    local.NextTranInfo.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveNextTranInfo(local.NextTranInfo, useImport.NextTranInfo);

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

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of NextTranInfo.
    /// </summary>
    [JsonPropertyName("nextTranInfo")]
    public NextTranInfo NextTranInfo
    {
      get => nextTranInfo ??= new();
      set => nextTranInfo = value;
    }

    private NextTranInfo nextTranInfo;
  }
#endregion
}
