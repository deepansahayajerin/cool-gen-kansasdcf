// Program: SI_PAR3_PA_REFERRAL_FOSTER_CARE, ID: 371760953, model: 746.
// Short name: SWEPAR3P
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
/// A program: SI_PAR3_PA_REFERRAL_FOSTER_CARE.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiPar3PaReferralFosterCare: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_PAR3_PA_REFERRAL_FOSTER_CARE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiPar3PaReferralFosterCare(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiPar3PaReferralFosterCare.
  /// </summary>
  public SiPar3PaReferralFosterCare(IContext context, Import import,
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
    // ------------------------------------------------------------
    //        M A I N T E N A N C E   L O G
    // Date	  Developer
    // 08/29/95  Alan Hackler		Initial Development
    // 11/03/96  G. Lofton - MTW	Add new security and removed
    // 				old.
    // 11/25/02  Kalpesh Doshi		Fix Screen Help
    // ------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);
    export.PaParticipantAddress.Assign(import.PaParticipantAddress);
    export.PaReferral.Assign(import.PaReferral);
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // ------------------------------------------------------------
    // If the next tran info is not equal to spaces, this implies
    // the user requested a next tran action. now validate
    // ------------------------------------------------------------
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      UseScCabNextTranPut();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();

      return;
    }

    // to validate action level security
    UseScCabTestSecurity();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    switch(TrimEnd(global.Command))
    {
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "DISPLAY":
        UseSiReadPaReferralFc();

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

  private static void MovePaReferral1(PaReferral source, PaReferral target)
  {
    target.From = source.From;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.CaseNumber = source.CaseNumber;
    target.Number = source.Number;
    target.Type1 = source.Type1;
    target.FcNextJuvenileCtDt = source.FcNextJuvenileCtDt;
    target.FcOrderEstBy = source.FcOrderEstBy;
    target.FcJuvenileCourtOrder = source.FcJuvenileCourtOrder;
    target.FcJuvenileOffenderInd = source.FcJuvenileOffenderInd;
    target.FcCincInd = source.FcCincInd;
    target.FcPlacementDate = source.FcPlacementDate;
    target.FcSrsPayee = source.FcSrsPayee;
    target.FcCostOfCareFreq = source.FcCostOfCareFreq;
    target.FcCostOfCare = source.FcCostOfCare;
    target.FcAdoptionDisruptionInd = source.FcAdoptionDisruptionInd;
    target.FcPlacementType = source.FcPlacementType;
    target.FcPreviousPa = source.FcPreviousPa;
    target.FcRightsSevered = source.FcRightsSevered;
    target.FcPlacementName = source.FcPlacementName;
    target.FcSourceOfFunding = source.FcSourceOfFunding;
    target.FcOtherBenefitInd = source.FcOtherBenefitInd;
    target.FcZebInd = source.FcZebInd;
    target.FcVaInd = source.FcVaInd;
    target.FcSsi = source.FcSsi;
    target.FcSsa = source.FcSsa;
    target.FcWardsAccount = source.FcWardsAccount;
    target.FcCountyChildRemovedFrom = source.FcCountyChildRemovedFrom;
    target.FcApNotified = source.FcApNotified;
  }

  private static void MovePaReferral2(PaReferral source, PaReferral target)
  {
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.Number = source.Number;
    target.Type1 = source.Type1;
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

  private void UseSiReadPaReferralFc()
  {
    var useImport = new SiReadPaReferralFc.Import();
    var useExport = new SiReadPaReferralFc.Export();

    MovePaReferral2(import.PaReferral, useImport.PaReferral);

    Call(SiReadPaReferralFc.Execute, useImport, useExport);

    export.PaParticipantAddress.Assign(useExport.PaParticipantAddress);
    MovePaReferral1(useExport.PaReferral, export.PaReferral);
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
    /// A value of ReturnRouting.
    /// </summary>
    [JsonPropertyName("returnRouting")]
    public Common ReturnRouting
    {
      get => returnRouting ??= new();
      set => returnRouting = value;
    }

    /// <summary>
    /// A value of PaParticipantAddress.
    /// </summary>
    [JsonPropertyName("paParticipantAddress")]
    public PaParticipantAddress PaParticipantAddress
    {
      get => paParticipantAddress ??= new();
      set => paParticipantAddress = value;
    }

    /// <summary>
    /// A value of PaReferral.
    /// </summary>
    [JsonPropertyName("paReferral")]
    public PaReferral PaReferral
    {
      get => paReferral ??= new();
      set => paReferral = value;
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

    private Common returnRouting;
    private PaParticipantAddress paParticipantAddress;
    private PaReferral paReferral;
    private NextTranInfo hidden;
    private Standard standard;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of PaParticipantAddress.
    /// </summary>
    [JsonPropertyName("paParticipantAddress")]
    public PaParticipantAddress PaParticipantAddress
    {
      get => paParticipantAddress ??= new();
      set => paParticipantAddress = value;
    }

    /// <summary>
    /// A value of PaReferral.
    /// </summary>
    [JsonPropertyName("paReferral")]
    public PaReferral PaReferral
    {
      get => paReferral ??= new();
      set => paReferral = value;
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

    private PaParticipantAddress paParticipantAddress;
    private PaReferral paReferral;
    private NextTranInfo hidden;
    private Standard standard;
  }
#endregion
}
