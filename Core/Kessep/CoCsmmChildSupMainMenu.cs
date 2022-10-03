﻿// Program: CO_CSMM_CHILD_SUP_MAIN_MENU, ID: 371451643, model: 746.
// Short name: SWECSMMP
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
/// A program: CO_CSMM_CHILD_SUP_MAIN_MENU.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class CoCsmmChildSupMainMenu: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CO_CSMM_CHILD_SUP_MAIN_MENU program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CoCsmmChildSupMainMenu(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CoCsmmChildSupMainMenu.
  /// </summary>
  public CoCsmmChildSupMainMenu(IContext context, Import import, Export export):
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
    // Date	  Developer		Description
    // 05/24/95  Alan Hackler		Initial Code
    // 02/05/96  Sid Chowdhary		Retrofits
    // 09/05/96  Jack Rookard		Add Return Address processing.
    // 11/05/96  G.Lofton - MTW	Add new tran cabs.
    // 11/25/96  Jack Rookard          Add new dialog flow to CPMM Service Plan 
    // Maintenance Menu in support of new Service Plan architecture and
    // functionality.
    // 11/26/96   Jenny Howard         Add new flow with PFkey
    //                                 
    // functionality to SYSE
    //                                 
    // System Selection Menu.
    // 04/11/97   Jack Rookard         Fix bug regarding discontinued Office 
    // Service Providers.
    // 04/18/97   Jack Rookard         Temporary add of exit state for PF3 Exit 
    // stating that this function key is not currently available.
    // 04/28/97   Jack Rookard         Change current date processing to 
    // reference Local Current Date Work Area Date view.  Also modified Create
    // Out Doc Rtrn Addr and Read Out Doc Rtrn Addr. Per request of Matt
    // Wheaton.
    // 10/31/1998  M Ramirez		Changed exit to perform a signoff
    // 10/31/1998  M Ramirez		Removed ODRA
    // 02/23/1999  J Caillouet         Added Flow to QAMN
    // ------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;
    export.Hidden.Assign(import.Hidden);
    MoveStandard(import.Standard, export.Standard);

    switch(TrimEnd(global.Command))
    {
      case "XXNEXTXX":
        UseScCabNextTranGet();

        break;
      case "EXIT":
        // If an exit was requested, the nexttran is used to exit to
        // the SYSE, System Selection Menu.
        UseEabXctlToKaecsesWUserid();

        // mjr--->  Only gets here, if it returns from 
        // eab_xctl_to_kaecses_w_userid
        // 	This means we are in Region 'SRCICST'
        UseScCabSignoff();

        break;
      case "XXFMMENU":
        break;
      case "":
        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "ENTER":
        if (!IsEmpty(import.Standard.NextTransaction))
        {
          // The user requested a next tran action
          UseScCabNextTranPut();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            var field = GetField(export.Standard, "nextTransaction");

            field.Error = true;
          }

          return;
        }

        switch(import.Standard.MenuOption)
        {
          case 1:
            ExitState = "ECO_XFR_TO_SERVICE_INIT_MENU";

            break;
          case 2:
            ExitState = "ECO_XFR_2_SERVICE_PLAN_MGT_MENU";

            break;
          case 3:
            ExitState = "ECO_XFR_TO_OBLM_OBLIGATION_MENU";

            break;
          case 4:
            ExitState = "ECO_XFR_TO_LEGAL_ENFORCMNT_MENU";

            break;
          case 5:
            ExitState = "ECO_XFR_TO_FMEN_MENU";

            break;
          case 6:
            ExitState = "ECO_XFR_TO_QAMN_MENU";

            break;
          case 7:
            ExitState = "ECO_XFR_TO_STBM_SUPPRT_TBL_MENU";

            break;
          case 8:
            ExitState = "ECO_XFR_TO_CPMM_MENU";

            break;
          case 9:
            ExitState = "ECO_LNK_TO_BCPM";

            break;
          default:
            var field = GetField(export.Standard, "menuOption");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_OPTION";

            break;
        }

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

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

  private void UseEabXctlToKaecsesWUserid()
  {
    var useImport = new EabXctlToKaecsesWUserid.Import();
    var useExport = new EabXctlToKaecsesWUserid.Export();

    useImport.Security.Userid = local.PassForKaecses.Userid;

    Call(EabXctlToKaecsesWUserid.Execute, useImport, useExport);
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
    /// A value of PassForKaecses.
    /// </summary>
    [JsonPropertyName("passForKaecses")]
    public Security2 PassForKaecses
    {
      get => passForKaecses ??= new();
      set => passForKaecses = value;
    }

    /// <summary>
    /// A value of Exit.
    /// </summary>
    [JsonPropertyName("exit")]
    public Standard Exit
    {
      get => exit ??= new();
      set => exit = value;
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

    private DateWorkArea current;
    private Security2 passForKaecses;
    private Standard exit;
    private DateWorkArea null1;
  }
#endregion
}
