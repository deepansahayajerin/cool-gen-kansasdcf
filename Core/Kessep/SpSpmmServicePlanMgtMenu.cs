// Program: SP_SPMM_SERVICE_PLAN_MGT_MENU, ID: 371800627, model: 746.
// Short name: SWESPMMP
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_SPMM_SERVICE_PLAN_MGT_MENU.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpSpmmServicePlanMgtMenu: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_SPMM_SERVICE_PLAN_MGT_MENU program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpSpmmServicePlanMgtMenu(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpSpmmServicePlanMgtMenu.
  /// </summary>
  public SpSpmmServicePlanMgtMenu(IContext context, Import import, Export export)
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
    //  Date    Developer 	Request #    	Description
    // 02/1596 Alan Hackler                	retrofits
    // 10/16/96 R. Marchman                	Add new security and next tran
    // 11/27/96 J.Rookard & M.Ramirez      	Modify to reflect new Service Plan 
    // architecture and functionality.
    // 06/07/00 SWSRCHF 	000170 		Added entry for NATE
    // 12/03/10 GVandy 	CQ109  		Add new option for LAAL
    //  
    // -------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // **** begin group A ****
    export.Hidden.Assign(import.Hidden);
    local.NextTranInfo.Assign(import.Hidden);

    // **** end   group A ****
    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // **** begin group B ****
    MoveStandard(import.Standard, export.Standard);

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
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

    // **** end   group B ****
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
            ExitState = "ECO_XFR_TO_SP_HIST";

            break;
          case 2:
            ExitState = "ECO_XFR_TO_ALRT";

            break;
          case 3:
            ExitState = "ECO_XFR_TO_SPAD1";

            break;
          case 4:
            ExitState = "ECO_XFR_TO_MONA";

            break;
          case 5:
            ExitState = "ECO_XFR_TO_DMON";

            break;
          case 6:
            ExitState = "ECO_XFR_TO_ODCM";

            break;
          case 7:
            ExitState = "ECO_XFR_TO_AMEN";

            break;
          case 8:
            ExitState = "ECO_XFR_TO_SP_MENU_CASE_REVIEW";

            break;
          case 9:
            ExitState = "ECO_XFR_TO_KCAS";

            break;
          case 10:
            ExitState = "ECO_XFR_TO_GBOR";

            break;
          case 11:
            ExitState = "ECO_XFR_TO_MANAGE_A_CASE";

            break;
          case 12:
            ExitState = "ECO_XFR_TO_LRAL";

            break;
          case 13:
            ExitState = "ECO_XFR_TO_LAAL";

            break;
          case 14:
            ExitState = "ECO_XFR_TO_NATE";

            break;
          case 15:
            ExitState = "ECO_XFER_TO_CSLN";

            break;
          default:
            if (IsEmpty(import.Standard.NextTransaction))
            {
            }
            else
            {
              // ****
              // ****
              // this is where you would set the local next_tran_info attributes
              // to the import view attributes for the data to be passed to the
              // next transaction
              // ****
              // ****
              UseScCabNextTranPut();

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
              }
              else
              {
                var field1 = GetField(export.Standard, "nextTransaction");

                field1.Error = true;

                return;
              }

              return;
            }

            var field = GetField(export.Standard, "menuOption");

            field.Error = true;

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
