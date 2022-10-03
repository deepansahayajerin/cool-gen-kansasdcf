// Program: FN_DMEN_DISB_MANAGEMENT_MENU, ID: 371800432, model: 746.
// Short name: SWEDMENP
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
/// A program: FN_DMEN_DISB_MANAGEMENT_MENU.
/// </para>
/// <para>
/// Resp: Finance	
/// This is the Financial System Disbursement Menu.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnDmenDisbManagementMenu: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DMEN_DISB_MANAGEMENT_MENU program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDmenDisbManagementMenu(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDmenDisbManagementMenu.
  /// </summary>
  public FnDmenDisbManagementMenu(IContext context, Import import, Export export)
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
    // ------------------------------------------------------------------
    // Date 	  	Developer Name		Request #	Description
    // 10/17/96	Holly Kennedy-MTW			New Security Next Tran Retrofit
    // 7/12/99   Fangman   Disabled the Case conditions for 24 & 25. The flows 
    // were left in so that the Journal Voucher screens could be easily
    // reactivated in the future.
    // 9/6/00 Kalpesh Doshi
    // Disabled case condition 7 for DXUA and dialog flow.
    // ------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR") || Equal(global.Command, "XXNEXTXX") || IsEmpty
      (global.Command))
    {
      return;
    }

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      UseScCabNextTranPut();

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();
      global.Command = "DISPLAY";

      return;
    }

    // Main CASE OF COMMAND.
    switch(TrimEnd(global.Command))
    {
      case "ENTER":
        switch(import.Standard.MenuOption)
        {
          case 1:
            ExitState = "ECO_XFR_TO_MTN_WARRANT_REQUEST";

            break;
          case 2:
            ExitState = "ECO_XFR_TO_MTN_RECAPTURE_INSTRC";

            break;
          case 3:
            ExitState = "ECO_XFR_TO_MTN_PERSON_DISB_SUPP";

            break;
          case 4:
            ExitState = "ECO_XFR_TO_MTN_COLL_DISB_SUPPR";

            break;
          case 5:
            ExitState = "ECO_XFR_TO_MTN_DESIGNATED_PAYEE";

            break;
          case 6:
            // -----------------------
            // Naveen - 12/31/1998
            // Added case stmnt to flow to PPMT screen.
            // -----------------------
            ExitState = "ECO_XFR_TO_MTN_PREF_PMNT_METHOD";

            break;
          case 20:
            break;
          case 21:
            ExitState = "ECO_XFR_TO_LST_EFTS";

            break;
          case 22:
            ExitState = "ECO_XFR_TO_LST_EFT_DETAIL";

            break;
          case 23:
            ExitState = "ECO_XFR_TO_LST_EFT_STAT_HIST";

            // Commented out Case 24 & 25 for journal vouchers per Judy' s 
            // request.  The flow is being left in so that they can be easily
            // added back at a later date.
            // CASE 24
            // CASE 25
            break;
          case 26:
            export.EftTransmissionFileInfo.TransmissionType = "O";
            ExitState = "ECO_LNK_LST_EFT_TRAN_INFO";

            break;
          case 27:
            export.ElectronicFundTransmission.TransmissionType = "O";
            export.ElectronicFundTransmission.TransmissionStatusCode = "";
            ExitState = "ECO_LNK_LST_EFT_TRANSMISSION";

            break;
          case 30:
            ExitState = "ECO_XFR_TO_LST_MNTHLY_PAYEE_SUM";

            break;
          case 31:
            ExitState = "ECO_XFR_TO_LST_PAYEE_ACCT";

            break;
          case 32:
            ExitState = "ECO_XFR_TO_LST_WARRANTS";

            break;
          case 33:
            ExitState = "ECO_XFR_TO_LST_WARRANT_DETAIL";

            break;
          case 34:
            ExitState = "ECO_XFR_TO_LST_WARR_STAT_HIST";

            break;
          case 35:
            ExitState = "ECO_XFR_TO_LST_WARR_ADDR";

            break;
          case 46:
            ExitState = "ECO_XFR_TO_LST_RECAP_RULES_HIST";

            break;
          case 47:
            ExitState = "ECO_XFR_TO_LST_OBLG_RECAP_HIST";

            break;
          case 48:
            ExitState = "ECO_XFR_TO_LST_PAYEE_W_DISB_SUP";

            break;
          case 49:
            ExitState = "ECO_XFR_TO_LST_MTN_DISB_SUPP";

            break;
          case 50:
            ExitState = "ECO_XFR_TO_LST_DSGND_PAYEES";

            break;
          case 51:
            // -----------------------
            // Naveen - 12/31/1998
            // Added case stmnt to flow to PPLT screen.
            // -----------------------
            ExitState = "ECO_XFR_TO_LST_PREF_PMT_METHD";

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
    /// A value of DisbMenu.
    /// </summary>
    [JsonPropertyName("disbMenu")]
    public Common DisbMenu
    {
      get => disbMenu ??= new();
      set => disbMenu = value;
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

    /// <summary>
    /// A value of ElectronicFundTransmission.
    /// </summary>
    [JsonPropertyName("electronicFundTransmission")]
    public ElectronicFundTransmission ElectronicFundTransmission
    {
      get => electronicFundTransmission ??= new();
      set => electronicFundTransmission = value;
    }

    /// <summary>
    /// A value of EftTransmissionFileInfo.
    /// </summary>
    [JsonPropertyName("eftTransmissionFileInfo")]
    public EftTransmissionFileInfo EftTransmissionFileInfo
    {
      get => eftTransmissionFileInfo ??= new();
      set => eftTransmissionFileInfo = value;
    }

    private Common disbMenu;
    private Standard standard;
    private NextTranInfo hidden;
    private ElectronicFundTransmission electronicFundTransmission;
    private EftTransmissionFileInfo eftTransmissionFileInfo;
  }
#endregion
}
