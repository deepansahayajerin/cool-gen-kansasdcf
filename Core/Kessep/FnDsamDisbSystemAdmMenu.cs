// Program: FN_DSAM_DISB_SYSTEM_ADM_MENU, ID: 371743014, model: 746.
// Short name: SWEDSAMP
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_DSAM_DISB_SYSTEM_ADM_MENU.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnDsamDisbSystemAdmMenu: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DSAM_DISB_SYSTEM_ADM_MENU program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDsamDisbSystemAdmMenu(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDsamDisbSystemAdmMenu.
  /// </summary>
  public FnDsamDisbSystemAdmMenu(IContext context, Import import, Export export):
    
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
    // 11/27/96	R. Marchman	Add new next tran
    // *********************************************
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "SIGNOFF"))
    {
      UseScCabSignoff();

      return;
    }

    if (Equal(global.Command, "XFERMENU"))
    {
      return;
    }

    MoveStandard(import.Standard, export.Standard);

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

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();
      global.Command = "DISPLAY";

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";

      return;
    }

    switch(TrimEnd(global.Command))
    {
      case "ENTER":
        switch(import.Standard.MenuOption)
        {
          case 1:
            ExitState = "ECO_XFR_TO_MTN_DISB_TYPE";

            break;
          case 2:
            ExitState = "ECO_XFR_TO_MTN_PAYMENT_STAT";

            break;
          case 3:
            ExitState = "ECO_XFR_TO_MTN_DISB_STATUS";

            break;
          case 4:
            ExitState = "ECO_XFR_TO_MTN_DISB_TRAN_TYPE";

            break;
          case 5:
            ExitState = "ECO_XFR_TO_MTN_DISB_METHOD_TYPE";

            break;
          case 6:
            ExitState = "ECO_XFR_TO_MTN_DISB_TRN_RLN_RSN";

            break;
          case 7:
            ExitState = "ECO_XFR_TO_MTN_DEF_RECAP_INSTR";

            break;
          case 8:
            ExitState = "ECO_XFR_TO_MTN_MAXIMUM_PASSTHRU";

            break;
          case 9:
            ExitState = "ECO_XFR_TO_MTN_COST_REC_FEE_INF";

            break;
          case 10:
            ExitState = "ECO_XFR_TO_MTN_COLL_AGENCY_FEES";

            break;
          case 20:
            ExitState = "ECO_XFR_TO_LST_DISB_TYPE";

            break;
          case 21:
            ExitState = "ECO_XFR_TO_LST_PAYMENT_STAT";

            break;
          case 22:
            ExitState = "ECO_XFR_TO_LST_DISB_STAT";

            break;
          case 23:
            ExitState = "ECO_XFR_TO_LST_DISB_TRAN_TYPE";

            break;
          case 24:
            ExitState = "ECO_XFR_TO_LST_DISB_METHOD_TYPE";

            break;
          case 25:
            ExitState = "ECO_XFR_TO_LST_DISB_TRN_RLN_RSN";

            break;
          case 26:
            ExitState = "ECO_XFR_TO_LST_DEF_RECAP_RULES";

            break;
          case 27:
            ExitState = "ECO_XFR_TO_LST_MAX_PASSTHRU_HST";

            break;
          case 28:
            ExitState = "ECO_XFR_TO_LST_COST_REC_FEE_INF";

            break;
          case 30:
            ExitState = "ECO_XFR_TO_LST_COLL_AGENCY_FEES";

            break;
          default:
            ExitState = "INVALID_OPTION_SELECTED";

            break;
        }

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "":
        // the command will only be spaces if  you are returning from a 
        // procedure to the menu because of a security violation.
        UseScCabSecurityViolationCheck();

        break;
      case "SIGNOFF":
        break;
      case "DMEN":
        ExitState = "ECO_XFR_TO_DISB_MGMNT_MENU";

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

  private void UseScCabSecurityViolationCheck()
  {
    var useImport = new ScCabSecurityViolationCheck.Import();
    var useExport = new ScCabSecurityViolationCheck.Export();

    Call(ScCabSecurityViolationCheck.Execute, useImport, useExport);
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
