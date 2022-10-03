// Program: FN_DDMM_DEBT_DSTRBTN_MNGMNT_MENU, ID: 371742939, model: 746.
// Short name: SWEDDMMP
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_DDMM_DEBT_DSTRBTN_MNGMNT_MENU.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnDdmmDebtDstrbtnMngmntMenu: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DDMM_DEBT_DSTRBTN_MNGMNT_MENU program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDdmmDebtDstrbtnMngmntMenu(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDdmmDebtDstrbtnMngmntMenu.
  /// </summary>
  public FnDdmmDebtDstrbtnMngmntMenu(IContext context, Import import,
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
    // --------------------------------------------------------------------------
    // CHANGE LOG
    // Date	  Programmer		#	Description
    // 10/17/96  Holly Kennedy-MTW		Retrofit data level security, and fix
    // 					Next tran.
    // 					Cosmetic surgery on the screen.
    // 11/27/96	R. Marchman		Fix next tran
    // 10/17/96  Holly Kennedy-MTW		Remove RERE
    // 12/03/97  govind			Added POFF to menu
    // 04/11/99  Maureen Brown: Removed APAY from the menu (option 11),
    //  and changed option 12 (POFF) to 11.
    // --------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR") || Equal(global.Command, "XXNEXTXX") || IsEmpty
      (global.Command))
    {
      return;
    }

    MoveStandard(import.Standard, export.Standard);

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      UseScCabNextTranPut();

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ****
      // this is where you set your export value to the export hidden next tran 
      // values if the user is comming into this procedure on a next tran
      // action.
      // ****
      UseScCabNextTranGet();
      global.Command = "DISPLAY";

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      // ****
      // this is where you set your command to do whatever is necessary to do on
      // a flow from the menu, maybe just escape....
      // ****
      // You should get this information from the Dialog Flow Diagram.  It is 
      // the SEND CMD on the propertis for a Transfer from one  procedure to
      // another.
      // *** the statement would read COMMAND IS display   *****
      // *** if the dialog flow property was display first, just add an escape 
      // completely out of the procedure  ****
      global.Command = "DISPLAY";

      return;
    }

    switch(TrimEnd(global.Command))
    {
      case "ENTER":
        switch(import.Standard.MenuOption)
        {
          case 3:
            ExitState = "ECO_LNK_TO_OFEE1";

            break;
          case 1:
            ExitState = "ECO_LNK_TO_MTN_RECOVERY_OBLIG";

            break;
          case 2:
            ExitState = "ECO_LNK_TO_MTN_VOLUNTARY_OBLIG";

            break;
          case 4:
            ExitState = "ECO_XFR_TO_LST_MTN_PRSN_S_C_SUP";

            break;
          case 5:
            ExitState = "ECO_LNK_LST_NON_CSE_PRSN_ADDR";

            break;
          case 6:
            export.FromDdmmFlag.Flag = "Y";
            ExitState = "ECO_XFR_TO_ORGANIZATIONS";

            break;
          case 7:
            ExitState = "ECO_XFR_TO_OVRPYMNT_INTENT";

            break;
          case 8:
            ExitState = "ECO_XFR_TO_NADR";

            break;
          case 9:
            ExitState = "ECO_XFR_TO_NADS";

            break;
          case 10:
            ExitState = "ECO_LNK_TO_DSPLY_AP_PYR_ASUM";

            break;
          case 11:
            ExitState = "ACO_NE0000_OPTION_UNAVAILABLE";

            break;
          case 20:
            ExitState = "ECO_LNK_LST_OBLIG_BY_AP_PAYOR";

            break;
          case 21:
            ExitState = "ECO_LNK_TO_LST_OBLIG_BY_CRT_ORDR";

            break;
          case 22:
            ExitState = "ECO_LNK_TO_LST_DBT_ACT_BY_APPYR";

            break;
          case 23:
            ExitState = "ECO_LNK_TO_LST_COLL_BY_AP_PYR";

            break;
          case 24:
            ExitState = "ECO_LNK_LST_POTNTL_RCVRY_OBLG";

            break;
          case 25:
            ExitState = "ECO_LNK_TO_REC_COLL_ADJMNT";

            break;
          case 26:
            ExitState = "ECO_XFR_TO_LCDA";

            break;
          case 27:
            ExitState = "ECO_LNK_DMVL_MOTOR_VECHICLE_LIST";

            break;
          case 30:
            ExitState = "ECO_LNK_TO_REC_IND_PYMNT_HIST";

            break;
          case 31:
            ExitState = "ECO_LNK_LST_TO_KDMV";

            break;
          default:
            ExitState = "ACO_NE0000_INVALID_OPTION";

            break;
        }

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
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
    /// A value of FromDdmmFlag.
    /// </summary>
    [JsonPropertyName("fromDdmmFlag")]
    public Common FromDdmmFlag
    {
      get => fromDdmmFlag ??= new();
      set => fromDdmmFlag = value;
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

    private Common fromDdmmFlag;
    private Standard standard;
    private NextTranInfo hidden;
  }
#endregion
}
