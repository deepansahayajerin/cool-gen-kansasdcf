// Program: FN_HARDCODED_DEBT_DISTRIBUTION, ID: 371737112, model: 746.
// Short name: SWE00489
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_HARDCODED_DEBT_DISTRIBUTION.
/// </para>
/// <para>
/// RESP: FINANCE
/// This process stores the hardcoded types and statuses used in the obligation 
/// management area.
/// </para>
/// </summary>
[Serializable]
public partial class FnHardcodedDebtDistribution: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_HARDCODED_DEBT_DISTRIBUTION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnHardcodedDebtDistribution(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnHardcodedDebtDistribution.
  /// </summary>
  public FnHardcodedDebtDistribution(IContext context, Import import,
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
    // ---------------------------------------------
    // Date	  By	  IDCR#	  Description
    // ---------------------------------------------
    // ??????	  ??????	  Initial code
    // 120197	  govind	  Changed Debt Debt Type
    //                           to "D" for voluntary (instead  of "V")
    // 09/18/98  Y.Campbell      Made changes to some of
    //                           the existing set statements to change
    //                           values being set.  Also added some NEW
    //                           views and associated set statement.
    // ---------------------------------------------
    export.PgmAdc.ProgramTypeInd = "AF";
    export.PgmAdcInterstate.ProgramTypeInd = "AFI";
    export.PgmAdcFosterCare.ProgramTypeInd = "FC";
    export.PgmAdcFosterCareInterstate.ProgramTypeInd = "FCI";
    export.PgmNonAdc.ProgramTypeInd = "NA";
    export.PgmNonAdcInterstate.ProgramTypeInd = "NAI";
    export.PgmNonAdcFosterCare.ProgramTypeInd = "NF";
    export.PgmNonAdcChildInInstitute.ProgramTypeInd = "NC";
    export.CpaObligor.Type1 = "R";
    export.CpaSupportedPerson.Type1 = "S";
    export.CpaObligee.Type1 = "E";
    export.MosCsePersonAccount.Type1 = "A";
    export.MosObligation.Type1 = "O";
    export.MspsCsePersonAccount.Type1 = "A";
    export.MspsObligation.Type1 = "O";
    export.ObligPrimaryConcurrent.PrimarySecondaryCode = "P";
    export.ObligSecondaryConcurrent.PrimarySecondaryCode = "S";
    export.ObligJointSeveralConcurrent.PrimarySecondaryCode = "J";
    export.OtrnTDebt.Type1 = "DE";
    export.OtrnTDebtAdjustment.Type1 = "DA";
    export.OtrnDtAccrualInstructions.DebtType = "A";
    export.OtrnDtDebtDetail.DebtType = "D";
    export.OtrnDtVoluntary.DebtType = "D";
    export.OtrnDatIncrease.DebtAdjustmentType = "I";
    export.OtrnDatDecrease.DebtAdjustmentType = "D";
    export.OtCAccruingClassification.Classification = "A";
    export.OtCRecoverClassification.Classification = "R";
    export.OtCMedicalClassification.Classification = "M";
    export.OtCNonAccruingClassification.Classification = "N";
    export.OtCVoluntaryClassification.Classification = "V";
    export.OtCFeesClassification.Classification = "F";
    export.DdshActiveStatus.Code = "A";
    export.DdshDeactivedStatus.Code = "D";
    export.OrrPrimarySecondary.SequentialGeneratedIdentifier = 1;
    export.OrrJointSeveral.SequentialGeneratedIdentifier = 4;
    export.OtrrAccrual.SystemGeneratedIdentifier = 1;
    export.OtrrDaBalanceOwedAdj.SystemGeneratedIdentifier = 2;
    export.OtrrDaInterestOwedAdj.SystemGeneratedIdentifier = 3;
    export.OtrrGiftAdjustment.SystemGeneratedIdentifier = 4;
    export.OtrrConcurrentObligation.SystemGeneratedIdentifier = 5;
    export.OpsCBiMonthly.FrequencyCode = "BM";
    export.OpsCBiWeekly.FrequencyCode = "BW";
    export.OpsCMonthly.FrequencyCode = "M";
    export.OpsCSemiMonthly.FrequencyCode = "SM";
    export.OpsCWeekly.FrequencyCode = "W";
    export.OpsPiEvenPeriod.PeriodInd = "E";
    export.OpsPiOddPeriod.PeriodInd = "O";
    export.OtChildSupport.SystemGeneratedIdentifier = 1;
    export.OtSpousalSupport.SystemGeneratedIdentifier = 2;
    export.OtMedicalSupport.SystemGeneratedIdentifier = 3;
    export.OtIvdRecovery.SystemGeneratedIdentifier = 4;
    export.OtIrsNegative.SystemGeneratedIdentifier = 5;
    export.OtArMisdirectedPymnt.SystemGeneratedIdentifier = 6;
    export.OtApMisdirectedPymnt.SystemGeneratedIdentifier = 7;
    export.OtNonCsePrnMisdirectedPymnt.SystemGeneratedIdentifier = 8;
    export.OtBadCheck.SystemGeneratedIdentifier = 9;
    export.OtMedicalJudgement.SystemGeneratedIdentifier = 10;
    export.OtPctUnisuredMedExpJudgemnt.SystemGeneratedIdentifier = 11;
    export.OtInterestJudgement.SystemGeneratedIdentifier = 12;
    export.OtArrearsJudgement.SystemGeneratedIdentifier = 13;
    export.OtCostOfRasingChild.SystemGeneratedIdentifier = 14;
    export.OtApFees.SystemGeneratedIdentifier = 15;
    export.OtVoluntary.SystemGeneratedIdentifier = 16;
    export.OtSpousalArrearsJudgement.SystemGeneratedIdentifier = 17;
    export.Ot718BUraJudgement.SystemGeneratedIdentifier = 18;
    export.OtMedicalSupportForCash.SystemGeneratedIdentifier = 19;
    export.OtGift.SystemGeneratedIdentifier = 22;
    export.CollArrears.AppliedToCode = "A";
    export.CollCurrent.AppliedToCode = "C";
    export.CollGift.AppliedToCode = "G";
    export.CollInterest.AppliedToCode = "I";
    export.CollPAf.ProgramAppliedTo = "AF";
    export.CollPAfi.ProgramAppliedTo = "AFI";
    export.CollPNa.ProgramAppliedTo = "NA";
    export.CollPNai.ProgramAppliedTo = "NAI";
    export.CollPFc.ProgramAppliedTo = "FC";
    export.CollPFci.ProgramAppliedTo = "FCI";
    export.CollPNc.ProgramAppliedTo = "NC";
    export.CollPNf.ProgramAppliedTo = "NF";
    export.CarRetroProgramChange.SystemGeneratedIdentifier = 2;
    export.CarRetroCollection.SystemGeneratedIdentifier = 3;
    export.CarRetroDebtAdjustment.SystemGeneratedIdentifier = 4;
    export.CarPostedToTheWrongAcct.SystemGeneratedIdentifier = 5;
    export.CarIrsNegRevOutTheColl.SystemGeneratedIdentifier = 6;
    export.CarBadCheckByPayor.SystemGeneratedIdentifier = 7;
    export.CarCollectionRefunded.SystemGeneratedIdentifier = 8;
    export.CarStopPayment.SystemGeneratedIdentifier = 9;
    export.CarNonAdcChgNadcFosterCare.SystemGeneratedIdentifier = 10;
    export.CarNonAdcChangedToAdc.SystemGeneratedIdentifier = 11;
    export.CarNonAdcChngAdcFosterCare.SystemGeneratedIdentifier = 12;
    export.CarNonAdcFosterCareChgNadc.SystemGeneratedIdentifier = 13;
    export.CarNadcFosterCareChgToAdc.SystemGeneratedIdentifier = 14;
    export.CarNadcFcChgAdcFc.SystemGeneratedIdentifier = 15;
    export.CarAdcChangedToNonAdc.SystemGeneratedIdentifier = 16;
    export.CarAdcChgToNadcFosterCare.SystemGeneratedIdentifier = 17;
    export.CarAdcChgToAdcFosterCare.SystemGeneratedIdentifier = 18;
    export.CarAdcFosterCareChgToNadc.SystemGeneratedIdentifier = 19;
    export.CarAdcFcChgNadcFc.SystemGeneratedIdentifier = 20;
    export.CarAdcFosterCareChgToAdc.SystemGeneratedIdentifier = 21;
    export.CarInterestRecalculation.SystemGeneratedIdentifier = 22;
    export.CarCollAdjmtDueToFdsoAdj.SystemGeneratedIdentifier = 23;
    export.CarCollAdjmtDueToCourtAdj.SystemGeneratedIdentifier = 24;
    export.CarCollAdjmtDueToSdsoAdj.SystemGeneratedIdentifier = 25;
    export.CarCollAdjmtDueToNewDebts.SystemGeneratedIdentifier = 26;
    export.CarSubseqCollection.SystemGeneratedIdentifier = 27;
    export.CarArCaseRoleChange.SystemGeneratedIdentifier = 28;
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
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of PgmAdcFosterCareInterstate.
    /// </summary>
    [JsonPropertyName("pgmAdcFosterCareInterstate")]
    public ProgramScreenAttributes PgmAdcFosterCareInterstate
    {
      get => pgmAdcFosterCareInterstate ??= new();
      set => pgmAdcFosterCareInterstate = value;
    }

    /// <summary>
    /// A value of PgmAdcFosterCare.
    /// </summary>
    [JsonPropertyName("pgmAdcFosterCare")]
    public ProgramScreenAttributes PgmAdcFosterCare
    {
      get => pgmAdcFosterCare ??= new();
      set => pgmAdcFosterCare = value;
    }

    /// <summary>
    /// A value of PgmAdcInterstate.
    /// </summary>
    [JsonPropertyName("pgmAdcInterstate")]
    public ProgramScreenAttributes PgmAdcInterstate
    {
      get => pgmAdcInterstate ??= new();
      set => pgmAdcInterstate = value;
    }

    /// <summary>
    /// A value of PgmAdc.
    /// </summary>
    [JsonPropertyName("pgmAdc")]
    public ProgramScreenAttributes PgmAdc
    {
      get => pgmAdc ??= new();
      set => pgmAdc = value;
    }

    /// <summary>
    /// A value of PgmNonAdcChildInInstitute.
    /// </summary>
    [JsonPropertyName("pgmNonAdcChildInInstitute")]
    public ProgramScreenAttributes PgmNonAdcChildInInstitute
    {
      get => pgmNonAdcChildInInstitute ??= new();
      set => pgmNonAdcChildInInstitute = value;
    }

    /// <summary>
    /// A value of PgmNonAdcFosterCare.
    /// </summary>
    [JsonPropertyName("pgmNonAdcFosterCare")]
    public ProgramScreenAttributes PgmNonAdcFosterCare
    {
      get => pgmNonAdcFosterCare ??= new();
      set => pgmNonAdcFosterCare = value;
    }

    /// <summary>
    /// A value of PgmNonAdc.
    /// </summary>
    [JsonPropertyName("pgmNonAdc")]
    public ProgramScreenAttributes PgmNonAdc
    {
      get => pgmNonAdc ??= new();
      set => pgmNonAdc = value;
    }

    /// <summary>
    /// A value of PgmNonAdcInterstate.
    /// </summary>
    [JsonPropertyName("pgmNonAdcInterstate")]
    public ProgramScreenAttributes PgmNonAdcInterstate
    {
      get => pgmNonAdcInterstate ??= new();
      set => pgmNonAdcInterstate = value;
    }

    /// <summary>
    /// A value of CpaObligor.
    /// </summary>
    [JsonPropertyName("cpaObligor")]
    public CsePersonAccount CpaObligor
    {
      get => cpaObligor ??= new();
      set => cpaObligor = value;
    }

    /// <summary>
    /// A value of CpaSupportedPerson.
    /// </summary>
    [JsonPropertyName("cpaSupportedPerson")]
    public CsePersonAccount CpaSupportedPerson
    {
      get => cpaSupportedPerson ??= new();
      set => cpaSupportedPerson = value;
    }

    /// <summary>
    /// A value of CpaObligee.
    /// </summary>
    [JsonPropertyName("cpaObligee")]
    public CsePersonAccount CpaObligee
    {
      get => cpaObligee ??= new();
      set => cpaObligee = value;
    }

    /// <summary>
    /// A value of MosCsePersonAccount.
    /// </summary>
    [JsonPropertyName("mosCsePersonAccount")]
    public MonthlyObligorSummary MosCsePersonAccount
    {
      get => mosCsePersonAccount ??= new();
      set => mosCsePersonAccount = value;
    }

    /// <summary>
    /// A value of MosObligation.
    /// </summary>
    [JsonPropertyName("mosObligation")]
    public MonthlyObligorSummary MosObligation
    {
      get => mosObligation ??= new();
      set => mosObligation = value;
    }

    /// <summary>
    /// A value of MspsCsePersonAccount.
    /// </summary>
    [JsonPropertyName("mspsCsePersonAccount")]
    public MonthlySupportedPersonSummary MspsCsePersonAccount
    {
      get => mspsCsePersonAccount ??= new();
      set => mspsCsePersonAccount = value;
    }

    /// <summary>
    /// A value of MspsObligation.
    /// </summary>
    [JsonPropertyName("mspsObligation")]
    public MonthlySupportedPersonSummary MspsObligation
    {
      get => mspsObligation ??= new();
      set => mspsObligation = value;
    }

    /// <summary>
    /// A value of ObligPrimaryConcurrent.
    /// </summary>
    [JsonPropertyName("obligPrimaryConcurrent")]
    public Obligation ObligPrimaryConcurrent
    {
      get => obligPrimaryConcurrent ??= new();
      set => obligPrimaryConcurrent = value;
    }

    /// <summary>
    /// A value of ObligSecondaryConcurrent.
    /// </summary>
    [JsonPropertyName("obligSecondaryConcurrent")]
    public Obligation ObligSecondaryConcurrent
    {
      get => obligSecondaryConcurrent ??= new();
      set => obligSecondaryConcurrent = value;
    }

    /// <summary>
    /// A value of ObligJointSeveralConcurrent.
    /// </summary>
    [JsonPropertyName("obligJointSeveralConcurrent")]
    public Obligation ObligJointSeveralConcurrent
    {
      get => obligJointSeveralConcurrent ??= new();
      set => obligJointSeveralConcurrent = value;
    }

    /// <summary>
    /// A value of OtrnTDebt.
    /// </summary>
    [JsonPropertyName("otrnTDebt")]
    public ObligationTransaction OtrnTDebt
    {
      get => otrnTDebt ??= new();
      set => otrnTDebt = value;
    }

    /// <summary>
    /// A value of OtrnTDebtAdjustment.
    /// </summary>
    [JsonPropertyName("otrnTDebtAdjustment")]
    public ObligationTransaction OtrnTDebtAdjustment
    {
      get => otrnTDebtAdjustment ??= new();
      set => otrnTDebtAdjustment = value;
    }

    /// <summary>
    /// A value of OtrnDtAccrualInstructions.
    /// </summary>
    [JsonPropertyName("otrnDtAccrualInstructions")]
    public ObligationTransaction OtrnDtAccrualInstructions
    {
      get => otrnDtAccrualInstructions ??= new();
      set => otrnDtAccrualInstructions = value;
    }

    /// <summary>
    /// A value of OtrnDtDebtDetail.
    /// </summary>
    [JsonPropertyName("otrnDtDebtDetail")]
    public ObligationTransaction OtrnDtDebtDetail
    {
      get => otrnDtDebtDetail ??= new();
      set => otrnDtDebtDetail = value;
    }

    /// <summary>
    /// A value of OtrnDtVoluntary.
    /// </summary>
    [JsonPropertyName("otrnDtVoluntary")]
    public ObligationTransaction OtrnDtVoluntary
    {
      get => otrnDtVoluntary ??= new();
      set => otrnDtVoluntary = value;
    }

    /// <summary>
    /// A value of OtrnDatDecrease.
    /// </summary>
    [JsonPropertyName("otrnDatDecrease")]
    public ObligationTransaction OtrnDatDecrease
    {
      get => otrnDatDecrease ??= new();
      set => otrnDatDecrease = value;
    }

    /// <summary>
    /// A value of OtrnDatIncrease.
    /// </summary>
    [JsonPropertyName("otrnDatIncrease")]
    public ObligationTransaction OtrnDatIncrease
    {
      get => otrnDatIncrease ??= new();
      set => otrnDatIncrease = value;
    }

    /// <summary>
    /// A value of DdshActiveStatus.
    /// </summary>
    [JsonPropertyName("ddshActiveStatus")]
    public DebtDetailStatusHistory DdshActiveStatus
    {
      get => ddshActiveStatus ??= new();
      set => ddshActiveStatus = value;
    }

    /// <summary>
    /// A value of DdshDeactivedStatus.
    /// </summary>
    [JsonPropertyName("ddshDeactivedStatus")]
    public DebtDetailStatusHistory DdshDeactivedStatus
    {
      get => ddshDeactivedStatus ??= new();
      set => ddshDeactivedStatus = value;
    }

    /// <summary>
    /// A value of OtrrAccrual.
    /// </summary>
    [JsonPropertyName("otrrAccrual")]
    public ObligationTransactionRlnRsn OtrrAccrual
    {
      get => otrrAccrual ??= new();
      set => otrrAccrual = value;
    }

    /// <summary>
    /// A value of OtrrDaBalanceOwedAdj.
    /// </summary>
    [JsonPropertyName("otrrDaBalanceOwedAdj")]
    public ObligationTransactionRlnRsn OtrrDaBalanceOwedAdj
    {
      get => otrrDaBalanceOwedAdj ??= new();
      set => otrrDaBalanceOwedAdj = value;
    }

    /// <summary>
    /// A value of OtrrDaInterestOwedAdj.
    /// </summary>
    [JsonPropertyName("otrrDaInterestOwedAdj")]
    public ObligationTransactionRlnRsn OtrrDaInterestOwedAdj
    {
      get => otrrDaInterestOwedAdj ??= new();
      set => otrrDaInterestOwedAdj = value;
    }

    /// <summary>
    /// A value of OtrrGiftAdjustment.
    /// </summary>
    [JsonPropertyName("otrrGiftAdjustment")]
    public ObligationTransactionRlnRsn OtrrGiftAdjustment
    {
      get => otrrGiftAdjustment ??= new();
      set => otrrGiftAdjustment = value;
    }

    /// <summary>
    /// A value of OtrrConcurrentObligation.
    /// </summary>
    [JsonPropertyName("otrrConcurrentObligation")]
    public ObligationTransactionRlnRsn OtrrConcurrentObligation
    {
      get => otrrConcurrentObligation ??= new();
      set => otrrConcurrentObligation = value;
    }

    /// <summary>
    /// A value of OtChildSupport.
    /// </summary>
    [JsonPropertyName("otChildSupport")]
    public ObligationType OtChildSupport
    {
      get => otChildSupport ??= new();
      set => otChildSupport = value;
    }

    /// <summary>
    /// A value of OtSpousalSupport.
    /// </summary>
    [JsonPropertyName("otSpousalSupport")]
    public ObligationType OtSpousalSupport
    {
      get => otSpousalSupport ??= new();
      set => otSpousalSupport = value;
    }

    /// <summary>
    /// A value of OtMedicalSupport.
    /// </summary>
    [JsonPropertyName("otMedicalSupport")]
    public ObligationType OtMedicalSupport
    {
      get => otMedicalSupport ??= new();
      set => otMedicalSupport = value;
    }

    /// <summary>
    /// A value of OtIvdRecovery.
    /// </summary>
    [JsonPropertyName("otIvdRecovery")]
    public ObligationType OtIvdRecovery
    {
      get => otIvdRecovery ??= new();
      set => otIvdRecovery = value;
    }

    /// <summary>
    /// A value of OtIrsNegative.
    /// </summary>
    [JsonPropertyName("otIrsNegative")]
    public ObligationType OtIrsNegative
    {
      get => otIrsNegative ??= new();
      set => otIrsNegative = value;
    }

    /// <summary>
    /// A value of OtArMisdirectedPymnt.
    /// </summary>
    [JsonPropertyName("otArMisdirectedPymnt")]
    public ObligationType OtArMisdirectedPymnt
    {
      get => otArMisdirectedPymnt ??= new();
      set => otArMisdirectedPymnt = value;
    }

    /// <summary>
    /// A value of OtApMisdirectedPymnt.
    /// </summary>
    [JsonPropertyName("otApMisdirectedPymnt")]
    public ObligationType OtApMisdirectedPymnt
    {
      get => otApMisdirectedPymnt ??= new();
      set => otApMisdirectedPymnt = value;
    }

    /// <summary>
    /// A value of OtNonCsePrnMisdirectedPymnt.
    /// </summary>
    [JsonPropertyName("otNonCsePrnMisdirectedPymnt")]
    public ObligationType OtNonCsePrnMisdirectedPymnt
    {
      get => otNonCsePrnMisdirectedPymnt ??= new();
      set => otNonCsePrnMisdirectedPymnt = value;
    }

    /// <summary>
    /// A value of OtBadCheck.
    /// </summary>
    [JsonPropertyName("otBadCheck")]
    public ObligationType OtBadCheck
    {
      get => otBadCheck ??= new();
      set => otBadCheck = value;
    }

    /// <summary>
    /// A value of OtMedicalJudgement.
    /// </summary>
    [JsonPropertyName("otMedicalJudgement")]
    public ObligationType OtMedicalJudgement
    {
      get => otMedicalJudgement ??= new();
      set => otMedicalJudgement = value;
    }

    /// <summary>
    /// A value of OtPctUnisuredMedExpJudgemnt.
    /// </summary>
    [JsonPropertyName("otPctUnisuredMedExpJudgemnt")]
    public ObligationType OtPctUnisuredMedExpJudgemnt
    {
      get => otPctUnisuredMedExpJudgemnt ??= new();
      set => otPctUnisuredMedExpJudgemnt = value;
    }

    /// <summary>
    /// A value of OtInterestJudgement.
    /// </summary>
    [JsonPropertyName("otInterestJudgement")]
    public ObligationType OtInterestJudgement
    {
      get => otInterestJudgement ??= new();
      set => otInterestJudgement = value;
    }

    /// <summary>
    /// A value of OtArrearsJudgement.
    /// </summary>
    [JsonPropertyName("otArrearsJudgement")]
    public ObligationType OtArrearsJudgement
    {
      get => otArrearsJudgement ??= new();
      set => otArrearsJudgement = value;
    }

    /// <summary>
    /// A value of OtCostOfRasingChild.
    /// </summary>
    [JsonPropertyName("otCostOfRasingChild")]
    public ObligationType OtCostOfRasingChild
    {
      get => otCostOfRasingChild ??= new();
      set => otCostOfRasingChild = value;
    }

    /// <summary>
    /// A value of OtApFees.
    /// </summary>
    [JsonPropertyName("otApFees")]
    public ObligationType OtApFees
    {
      get => otApFees ??= new();
      set => otApFees = value;
    }

    /// <summary>
    /// A value of OtVoluntary.
    /// </summary>
    [JsonPropertyName("otVoluntary")]
    public ObligationType OtVoluntary
    {
      get => otVoluntary ??= new();
      set => otVoluntary = value;
    }

    /// <summary>
    /// A value of OtSpousalArrearsJudgement.
    /// </summary>
    [JsonPropertyName("otSpousalArrearsJudgement")]
    public ObligationType OtSpousalArrearsJudgement
    {
      get => otSpousalArrearsJudgement ??= new();
      set => otSpousalArrearsJudgement = value;
    }

    /// <summary>
    /// A value of Ot718BUraJudgement.
    /// </summary>
    [JsonPropertyName("ot718BUraJudgement")]
    public ObligationType Ot718BUraJudgement
    {
      get => ot718BUraJudgement ??= new();
      set => ot718BUraJudgement = value;
    }

    /// <summary>
    /// A value of OtMedicalSupportForCash.
    /// </summary>
    [JsonPropertyName("otMedicalSupportForCash")]
    public ObligationType OtMedicalSupportForCash
    {
      get => otMedicalSupportForCash ??= new();
      set => otMedicalSupportForCash = value;
    }

    /// <summary>
    /// A value of OtGift.
    /// </summary>
    [JsonPropertyName("otGift")]
    public ObligationType OtGift
    {
      get => otGift ??= new();
      set => otGift = value;
    }

    /// <summary>
    /// A value of OrrPrimarySecondary.
    /// </summary>
    [JsonPropertyName("orrPrimarySecondary")]
    public ObligationRlnRsn OrrPrimarySecondary
    {
      get => orrPrimarySecondary ??= new();
      set => orrPrimarySecondary = value;
    }

    /// <summary>
    /// A value of OrrJointSeveral.
    /// </summary>
    [JsonPropertyName("orrJointSeveral")]
    public ObligationRlnRsn OrrJointSeveral
    {
      get => orrJointSeveral ??= new();
      set => orrJointSeveral = value;
    }

    /// <summary>
    /// A value of OtCAccruingClassification.
    /// </summary>
    [JsonPropertyName("otCAccruingClassification")]
    public ObligationType OtCAccruingClassification
    {
      get => otCAccruingClassification ??= new();
      set => otCAccruingClassification = value;
    }

    /// <summary>
    /// A value of OtCMedicalClassification.
    /// </summary>
    [JsonPropertyName("otCMedicalClassification")]
    public ObligationType OtCMedicalClassification
    {
      get => otCMedicalClassification ??= new();
      set => otCMedicalClassification = value;
    }

    /// <summary>
    /// A value of OtCNonAccruingClassification.
    /// </summary>
    [JsonPropertyName("otCNonAccruingClassification")]
    public ObligationType OtCNonAccruingClassification
    {
      get => otCNonAccruingClassification ??= new();
      set => otCNonAccruingClassification = value;
    }

    /// <summary>
    /// A value of OtCRecoverClassification.
    /// </summary>
    [JsonPropertyName("otCRecoverClassification")]
    public ObligationType OtCRecoverClassification
    {
      get => otCRecoverClassification ??= new();
      set => otCRecoverClassification = value;
    }

    /// <summary>
    /// A value of OtCVoluntaryClassification.
    /// </summary>
    [JsonPropertyName("otCVoluntaryClassification")]
    public ObligationType OtCVoluntaryClassification
    {
      get => otCVoluntaryClassification ??= new();
      set => otCVoluntaryClassification = value;
    }

    /// <summary>
    /// A value of OtCFeesClassification.
    /// </summary>
    [JsonPropertyName("otCFeesClassification")]
    public ObligationType OtCFeesClassification
    {
      get => otCFeesClassification ??= new();
      set => otCFeesClassification = value;
    }

    /// <summary>
    /// A value of OpsCMonthly.
    /// </summary>
    [JsonPropertyName("opsCMonthly")]
    public ObligationPaymentSchedule OpsCMonthly
    {
      get => opsCMonthly ??= new();
      set => opsCMonthly = value;
    }

    /// <summary>
    /// A value of OpsCBiMonthly.
    /// </summary>
    [JsonPropertyName("opsCBiMonthly")]
    public ObligationPaymentSchedule OpsCBiMonthly
    {
      get => opsCBiMonthly ??= new();
      set => opsCBiMonthly = value;
    }

    /// <summary>
    /// A value of OpsCSemiMonthly.
    /// </summary>
    [JsonPropertyName("opsCSemiMonthly")]
    public ObligationPaymentSchedule OpsCSemiMonthly
    {
      get => opsCSemiMonthly ??= new();
      set => opsCSemiMonthly = value;
    }

    /// <summary>
    /// A value of OpsCWeekly.
    /// </summary>
    [JsonPropertyName("opsCWeekly")]
    public ObligationPaymentSchedule OpsCWeekly
    {
      get => opsCWeekly ??= new();
      set => opsCWeekly = value;
    }

    /// <summary>
    /// A value of OpsCBiWeekly.
    /// </summary>
    [JsonPropertyName("opsCBiWeekly")]
    public ObligationPaymentSchedule OpsCBiWeekly
    {
      get => opsCBiWeekly ??= new();
      set => opsCBiWeekly = value;
    }

    /// <summary>
    /// A value of OpsPiEvenPeriod.
    /// </summary>
    [JsonPropertyName("opsPiEvenPeriod")]
    public ObligationPaymentSchedule OpsPiEvenPeriod
    {
      get => opsPiEvenPeriod ??= new();
      set => opsPiEvenPeriod = value;
    }

    /// <summary>
    /// A value of OpsPiOddPeriod.
    /// </summary>
    [JsonPropertyName("opsPiOddPeriod")]
    public ObligationPaymentSchedule OpsPiOddPeriod
    {
      get => opsPiOddPeriod ??= new();
      set => opsPiOddPeriod = value;
    }

    /// <summary>
    /// A value of CollCurrent.
    /// </summary>
    [JsonPropertyName("collCurrent")]
    public Collection CollCurrent
    {
      get => collCurrent ??= new();
      set => collCurrent = value;
    }

    /// <summary>
    /// A value of CollArrears.
    /// </summary>
    [JsonPropertyName("collArrears")]
    public Collection CollArrears
    {
      get => collArrears ??= new();
      set => collArrears = value;
    }

    /// <summary>
    /// A value of CollInterest.
    /// </summary>
    [JsonPropertyName("collInterest")]
    public Collection CollInterest
    {
      get => collInterest ??= new();
      set => collInterest = value;
    }

    /// <summary>
    /// A value of CollGift.
    /// </summary>
    [JsonPropertyName("collGift")]
    public Collection CollGift
    {
      get => collGift ??= new();
      set => collGift = value;
    }

    /// <summary>
    /// A value of CollPNa.
    /// </summary>
    [JsonPropertyName("collPNa")]
    public Collection CollPNa
    {
      get => collPNa ??= new();
      set => collPNa = value;
    }

    /// <summary>
    /// A value of CollPAf.
    /// </summary>
    [JsonPropertyName("collPAf")]
    public Collection CollPAf
    {
      get => collPAf ??= new();
      set => collPAf = value;
    }

    /// <summary>
    /// A value of CollPAfi.
    /// </summary>
    [JsonPropertyName("collPAfi")]
    public Collection CollPAfi
    {
      get => collPAfi ??= new();
      set => collPAfi = value;
    }

    /// <summary>
    /// A value of CollPFc.
    /// </summary>
    [JsonPropertyName("collPFc")]
    public Collection CollPFc
    {
      get => collPFc ??= new();
      set => collPFc = value;
    }

    /// <summary>
    /// A value of CollPNf.
    /// </summary>
    [JsonPropertyName("collPNf")]
    public Collection CollPNf
    {
      get => collPNf ??= new();
      set => collPNf = value;
    }

    /// <summary>
    /// A value of CollPNc.
    /// </summary>
    [JsonPropertyName("collPNc")]
    public Collection CollPNc
    {
      get => collPNc ??= new();
      set => collPNc = value;
    }

    /// <summary>
    /// A value of CollPNai.
    /// </summary>
    [JsonPropertyName("collPNai")]
    public Collection CollPNai
    {
      get => collPNai ??= new();
      set => collPNai = value;
    }

    /// <summary>
    /// A value of CollPFci.
    /// </summary>
    [JsonPropertyName("collPFci")]
    public Collection CollPFci
    {
      get => collPFci ??= new();
      set => collPFci = value;
    }

    /// <summary>
    /// A value of CarRetroProgramChange.
    /// </summary>
    [JsonPropertyName("carRetroProgramChange")]
    public CollectionAdjustmentReason CarRetroProgramChange
    {
      get => carRetroProgramChange ??= new();
      set => carRetroProgramChange = value;
    }

    /// <summary>
    /// A value of CarRetroCollection.
    /// </summary>
    [JsonPropertyName("carRetroCollection")]
    public CollectionAdjustmentReason CarRetroCollection
    {
      get => carRetroCollection ??= new();
      set => carRetroCollection = value;
    }

    /// <summary>
    /// A value of CarRetroDebtAdjustment.
    /// </summary>
    [JsonPropertyName("carRetroDebtAdjustment")]
    public CollectionAdjustmentReason CarRetroDebtAdjustment
    {
      get => carRetroDebtAdjustment ??= new();
      set => carRetroDebtAdjustment = value;
    }

    /// <summary>
    /// A value of CarPostedToTheWrongAcct.
    /// </summary>
    [JsonPropertyName("carPostedToTheWrongAcct")]
    public CollectionAdjustmentReason CarPostedToTheWrongAcct
    {
      get => carPostedToTheWrongAcct ??= new();
      set => carPostedToTheWrongAcct = value;
    }

    /// <summary>
    /// A value of CarIrsNegRevOutTheColl.
    /// </summary>
    [JsonPropertyName("carIrsNegRevOutTheColl")]
    public CollectionAdjustmentReason CarIrsNegRevOutTheColl
    {
      get => carIrsNegRevOutTheColl ??= new();
      set => carIrsNegRevOutTheColl = value;
    }

    /// <summary>
    /// A value of CarBadCheckByPayor.
    /// </summary>
    [JsonPropertyName("carBadCheckByPayor")]
    public CollectionAdjustmentReason CarBadCheckByPayor
    {
      get => carBadCheckByPayor ??= new();
      set => carBadCheckByPayor = value;
    }

    /// <summary>
    /// A value of CarCollectionRefunded.
    /// </summary>
    [JsonPropertyName("carCollectionRefunded")]
    public CollectionAdjustmentReason CarCollectionRefunded
    {
      get => carCollectionRefunded ??= new();
      set => carCollectionRefunded = value;
    }

    /// <summary>
    /// A value of CarStopPayment.
    /// </summary>
    [JsonPropertyName("carStopPayment")]
    public CollectionAdjustmentReason CarStopPayment
    {
      get => carStopPayment ??= new();
      set => carStopPayment = value;
    }

    /// <summary>
    /// A value of CarNonAdcChngAdcFosterCare.
    /// </summary>
    [JsonPropertyName("carNonAdcChngAdcFosterCare")]
    public CollectionAdjustmentReason CarNonAdcChngAdcFosterCare
    {
      get => carNonAdcChngAdcFosterCare ??= new();
      set => carNonAdcChngAdcFosterCare = value;
    }

    /// <summary>
    /// A value of CarAdcChangedToNonAdc.
    /// </summary>
    [JsonPropertyName("carAdcChangedToNonAdc")]
    public CollectionAdjustmentReason CarAdcChangedToNonAdc
    {
      get => carAdcChangedToNonAdc ??= new();
      set => carAdcChangedToNonAdc = value;
    }

    /// <summary>
    /// A value of CarNadcFcChgAdcFc.
    /// </summary>
    [JsonPropertyName("carNadcFcChgAdcFc")]
    public CollectionAdjustmentReason CarNadcFcChgAdcFc
    {
      get => carNadcFcChgAdcFc ??= new();
      set => carNadcFcChgAdcFc = value;
    }

    /// <summary>
    /// A value of CarNadcFosterCareChgToAdc.
    /// </summary>
    [JsonPropertyName("carNadcFosterCareChgToAdc")]
    public CollectionAdjustmentReason CarNadcFosterCareChgToAdc
    {
      get => carNadcFosterCareChgToAdc ??= new();
      set => carNadcFosterCareChgToAdc = value;
    }

    /// <summary>
    /// A value of CarNonAdcFosterCareChgNadc.
    /// </summary>
    [JsonPropertyName("carNonAdcFosterCareChgNadc")]
    public CollectionAdjustmentReason CarNonAdcFosterCareChgNadc
    {
      get => carNonAdcFosterCareChgNadc ??= new();
      set => carNonAdcFosterCareChgNadc = value;
    }

    /// <summary>
    /// A value of CarNonAdcChangedToAdc.
    /// </summary>
    [JsonPropertyName("carNonAdcChangedToAdc")]
    public CollectionAdjustmentReason CarNonAdcChangedToAdc
    {
      get => carNonAdcChangedToAdc ??= new();
      set => carNonAdcChangedToAdc = value;
    }

    /// <summary>
    /// A value of CarAdcFosterCareChgToNadc.
    /// </summary>
    [JsonPropertyName("carAdcFosterCareChgToNadc")]
    public CollectionAdjustmentReason CarAdcFosterCareChgToNadc
    {
      get => carAdcFosterCareChgToNadc ??= new();
      set => carAdcFosterCareChgToNadc = value;
    }

    /// <summary>
    /// A value of CarNonAdcChgNadcFosterCare.
    /// </summary>
    [JsonPropertyName("carNonAdcChgNadcFosterCare")]
    public CollectionAdjustmentReason CarNonAdcChgNadcFosterCare
    {
      get => carNonAdcChgNadcFosterCare ??= new();
      set => carNonAdcChgNadcFosterCare = value;
    }

    /// <summary>
    /// A value of CarAdcFcChgNadcFc.
    /// </summary>
    [JsonPropertyName("carAdcFcChgNadcFc")]
    public CollectionAdjustmentReason CarAdcFcChgNadcFc
    {
      get => carAdcFcChgNadcFc ??= new();
      set => carAdcFcChgNadcFc = value;
    }

    /// <summary>
    /// A value of CarAdcChgToNadcFosterCare.
    /// </summary>
    [JsonPropertyName("carAdcChgToNadcFosterCare")]
    public CollectionAdjustmentReason CarAdcChgToNadcFosterCare
    {
      get => carAdcChgToNadcFosterCare ??= new();
      set => carAdcChgToNadcFosterCare = value;
    }

    /// <summary>
    /// A value of CarAdcChgToAdcFosterCare.
    /// </summary>
    [JsonPropertyName("carAdcChgToAdcFosterCare")]
    public CollectionAdjustmentReason CarAdcChgToAdcFosterCare
    {
      get => carAdcChgToAdcFosterCare ??= new();
      set => carAdcChgToAdcFosterCare = value;
    }

    /// <summary>
    /// A value of CarAdcFosterCareChgToAdc.
    /// </summary>
    [JsonPropertyName("carAdcFosterCareChgToAdc")]
    public CollectionAdjustmentReason CarAdcFosterCareChgToAdc
    {
      get => carAdcFosterCareChgToAdc ??= new();
      set => carAdcFosterCareChgToAdc = value;
    }

    /// <summary>
    /// A value of CarInterestRecalculation.
    /// </summary>
    [JsonPropertyName("carInterestRecalculation")]
    public CollectionAdjustmentReason CarInterestRecalculation
    {
      get => carInterestRecalculation ??= new();
      set => carInterestRecalculation = value;
    }

    /// <summary>
    /// A value of CarCollAdjmtDueToFdsoAdj.
    /// </summary>
    [JsonPropertyName("carCollAdjmtDueToFdsoAdj")]
    public CollectionAdjustmentReason CarCollAdjmtDueToFdsoAdj
    {
      get => carCollAdjmtDueToFdsoAdj ??= new();
      set => carCollAdjmtDueToFdsoAdj = value;
    }

    /// <summary>
    /// A value of CarCollAdjmtDueToCourtAdj.
    /// </summary>
    [JsonPropertyName("carCollAdjmtDueToCourtAdj")]
    public CollectionAdjustmentReason CarCollAdjmtDueToCourtAdj
    {
      get => carCollAdjmtDueToCourtAdj ??= new();
      set => carCollAdjmtDueToCourtAdj = value;
    }

    /// <summary>
    /// A value of CarCollAdjmtDueToSdsoAdj.
    /// </summary>
    [JsonPropertyName("carCollAdjmtDueToSdsoAdj")]
    public CollectionAdjustmentReason CarCollAdjmtDueToSdsoAdj
    {
      get => carCollAdjmtDueToSdsoAdj ??= new();
      set => carCollAdjmtDueToSdsoAdj = value;
    }

    /// <summary>
    /// A value of CarCollAdjmtDueToNewDebts.
    /// </summary>
    [JsonPropertyName("carCollAdjmtDueToNewDebts")]
    public CollectionAdjustmentReason CarCollAdjmtDueToNewDebts
    {
      get => carCollAdjmtDueToNewDebts ??= new();
      set => carCollAdjmtDueToNewDebts = value;
    }

    /// <summary>
    /// A value of CarSubseqCollection.
    /// </summary>
    [JsonPropertyName("carSubseqCollection")]
    public CollectionAdjustmentReason CarSubseqCollection
    {
      get => carSubseqCollection ??= new();
      set => carSubseqCollection = value;
    }

    /// <summary>
    /// A value of CarArCaseRoleChange.
    /// </summary>
    [JsonPropertyName("carArCaseRoleChange")]
    public CollectionAdjustmentReason CarArCaseRoleChange
    {
      get => carArCaseRoleChange ??= new();
      set => carArCaseRoleChange = value;
    }

    private ProgramScreenAttributes pgmAdcFosterCareInterstate;
    private ProgramScreenAttributes pgmAdcFosterCare;
    private ProgramScreenAttributes pgmAdcInterstate;
    private ProgramScreenAttributes pgmAdc;
    private ProgramScreenAttributes pgmNonAdcChildInInstitute;
    private ProgramScreenAttributes pgmNonAdcFosterCare;
    private ProgramScreenAttributes pgmNonAdc;
    private ProgramScreenAttributes pgmNonAdcInterstate;
    private CsePersonAccount cpaObligor;
    private CsePersonAccount cpaSupportedPerson;
    private CsePersonAccount cpaObligee;
    private MonthlyObligorSummary mosCsePersonAccount;
    private MonthlyObligorSummary mosObligation;
    private MonthlySupportedPersonSummary mspsCsePersonAccount;
    private MonthlySupportedPersonSummary mspsObligation;
    private Obligation obligPrimaryConcurrent;
    private Obligation obligSecondaryConcurrent;
    private Obligation obligJointSeveralConcurrent;
    private ObligationTransaction otrnTDebt;
    private ObligationTransaction otrnTDebtAdjustment;
    private ObligationTransaction otrnDtAccrualInstructions;
    private ObligationTransaction otrnDtDebtDetail;
    private ObligationTransaction otrnDtVoluntary;
    private ObligationTransaction otrnDatDecrease;
    private ObligationTransaction otrnDatIncrease;
    private DebtDetailStatusHistory ddshActiveStatus;
    private DebtDetailStatusHistory ddshDeactivedStatus;
    private ObligationTransactionRlnRsn otrrAccrual;
    private ObligationTransactionRlnRsn otrrDaBalanceOwedAdj;
    private ObligationTransactionRlnRsn otrrDaInterestOwedAdj;
    private ObligationTransactionRlnRsn otrrGiftAdjustment;
    private ObligationTransactionRlnRsn otrrConcurrentObligation;
    private ObligationType otChildSupport;
    private ObligationType otSpousalSupport;
    private ObligationType otMedicalSupport;
    private ObligationType otIvdRecovery;
    private ObligationType otIrsNegative;
    private ObligationType otArMisdirectedPymnt;
    private ObligationType otApMisdirectedPymnt;
    private ObligationType otNonCsePrnMisdirectedPymnt;
    private ObligationType otBadCheck;
    private ObligationType otMedicalJudgement;
    private ObligationType otPctUnisuredMedExpJudgemnt;
    private ObligationType otInterestJudgement;
    private ObligationType otArrearsJudgement;
    private ObligationType otCostOfRasingChild;
    private ObligationType otApFees;
    private ObligationType otVoluntary;
    private ObligationType otSpousalArrearsJudgement;
    private ObligationType ot718BUraJudgement;
    private ObligationType otMedicalSupportForCash;
    private ObligationType otGift;
    private ObligationRlnRsn orrPrimarySecondary;
    private ObligationRlnRsn orrJointSeveral;
    private ObligationType otCAccruingClassification;
    private ObligationType otCMedicalClassification;
    private ObligationType otCNonAccruingClassification;
    private ObligationType otCRecoverClassification;
    private ObligationType otCVoluntaryClassification;
    private ObligationType otCFeesClassification;
    private ObligationPaymentSchedule opsCMonthly;
    private ObligationPaymentSchedule opsCBiMonthly;
    private ObligationPaymentSchedule opsCSemiMonthly;
    private ObligationPaymentSchedule opsCWeekly;
    private ObligationPaymentSchedule opsCBiWeekly;
    private ObligationPaymentSchedule opsPiEvenPeriod;
    private ObligationPaymentSchedule opsPiOddPeriod;
    private Collection collCurrent;
    private Collection collArrears;
    private Collection collInterest;
    private Collection collGift;
    private Collection collPNa;
    private Collection collPAf;
    private Collection collPAfi;
    private Collection collPFc;
    private Collection collPNf;
    private Collection collPNc;
    private Collection collPNai;
    private Collection collPFci;
    private CollectionAdjustmentReason carRetroProgramChange;
    private CollectionAdjustmentReason carRetroCollection;
    private CollectionAdjustmentReason carRetroDebtAdjustment;
    private CollectionAdjustmentReason carPostedToTheWrongAcct;
    private CollectionAdjustmentReason carIrsNegRevOutTheColl;
    private CollectionAdjustmentReason carBadCheckByPayor;
    private CollectionAdjustmentReason carCollectionRefunded;
    private CollectionAdjustmentReason carStopPayment;
    private CollectionAdjustmentReason carNonAdcChngAdcFosterCare;
    private CollectionAdjustmentReason carAdcChangedToNonAdc;
    private CollectionAdjustmentReason carNadcFcChgAdcFc;
    private CollectionAdjustmentReason carNadcFosterCareChgToAdc;
    private CollectionAdjustmentReason carNonAdcFosterCareChgNadc;
    private CollectionAdjustmentReason carNonAdcChangedToAdc;
    private CollectionAdjustmentReason carAdcFosterCareChgToNadc;
    private CollectionAdjustmentReason carNonAdcChgNadcFosterCare;
    private CollectionAdjustmentReason carAdcFcChgNadcFc;
    private CollectionAdjustmentReason carAdcChgToNadcFosterCare;
    private CollectionAdjustmentReason carAdcChgToAdcFosterCare;
    private CollectionAdjustmentReason carAdcFosterCareChgToAdc;
    private CollectionAdjustmentReason carInterestRecalculation;
    private CollectionAdjustmentReason carCollAdjmtDueToFdsoAdj;
    private CollectionAdjustmentReason carCollAdjmtDueToCourtAdj;
    private CollectionAdjustmentReason carCollAdjmtDueToSdsoAdj;
    private CollectionAdjustmentReason carCollAdjmtDueToNewDebts;
    private CollectionAdjustmentReason carSubseqCollection;
    private CollectionAdjustmentReason carArCaseRoleChange;
  }
#endregion
}
