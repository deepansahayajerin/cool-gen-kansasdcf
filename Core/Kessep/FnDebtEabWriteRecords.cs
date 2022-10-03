// Program: FN_DEBT_EAB_WRITE_RECORDS, ID: 373512140, model: 746.
// Short name: SWEX0002
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_DEBT_EAB_WRITE_RECORDS.
/// </summary>
[Serializable]
public partial class FnDebtEabWriteRecords: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DEBT_EAB_WRITE_RECORDS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDebtEabWriteRecords(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDebtEabWriteRecords.
  /// </summary>
  public FnDebtEabWriteRecords(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    GetService<IEabStub>().Execute(
      "SWEX0002", context, import, export, EabOptions.NoAS);
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
    /// A value of Icommon.
    /// </summary>
    [JsonPropertyName("icommon")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "ActionEntry" })]
    public Common Icommon
    {
      get => icommon ??= new();
      set => icommon = value;
    }

    /// <summary>
    /// A value of IlegalAction.
    /// </summary>
    [JsonPropertyName("ilegalAction")]
    [Member(Index = 2, AccessFields = false, Members = new[]
    {
      "Identifier",
      "FiledDate",
      "CourtCaseNumber",
      "StandardNumber"
    })]
    public LegalAction IlegalAction
    {
      get => ilegalAction ??= new();
      set => ilegalAction = value;
    }

    /// <summary>
    /// A value of IcsePerson.
    /// </summary>
    [JsonPropertyName("icsePerson")]
    [Member(Index = 3, AccessFields = false, Members
      = new[] { "Type1", "Number" })]
    public CsePerson IcsePerson
    {
      get => icsePerson ??= new();
      set => icsePerson = value;
    }

    /// <summary>
    /// A value of Iperson.
    /// </summary>
    [JsonPropertyName("iperson")]
    [Member(Index = 4, AccessFields = false, Members = new[]
    {
      "FormattedName",
      "FirstName",
      "MiddleInitial",
      "LastName",
      "Dob"
    })]
    public CsePersonsWorkSet Iperson
    {
      get => iperson ??= new();
      set => iperson = value;
    }

    /// <summary>
    /// A value of IlegalActionPerson.
    /// </summary>
    [JsonPropertyName("ilegalActionPerson")]
    [Member(Index = 5, AccessFields = false, Members = new[]
    {
      "Identifier",
      "AccountType",
      "Role",
      "EffectiveDate",
      "EndDate",
      "ArrearsAmount",
      "CurrentAmount",
      "JudgementAmount"
    })]
    public LegalActionPerson IlegalActionPerson
    {
      get => ilegalActionPerson ??= new();
      set => ilegalActionPerson = value;
    }

    /// <summary>
    /// A value of IlegalActionDetail.
    /// </summary>
    [JsonPropertyName("ilegalActionDetail")]
    [Member(Index = 6, AccessFields = false, Members = new[]
    {
      "Number",
      "DetailType",
      "EndDate",
      "EffectiveDate",
      "ArrearsAmount",
      "CurrentAmount",
      "JudgementAmount",
      "FreqPeriodCode",
      "DayOfWeek",
      "DayOfMonth1",
      "DayOfMonth2",
      "PeriodInd"
    })]
    public LegalActionDetail IlegalActionDetail
    {
      get => ilegalActionDetail ??= new();
      set => ilegalActionDetail = value;
    }

    /// <summary>
    /// A value of IobligationType.
    /// </summary>
    [JsonPropertyName("iobligationType")]
    [Member(Index = 7, Members = new[]
    {
      "SystemGeneratedIdentifier",
      "Code",
      "Name",
      "Classification"
    })]
    public ObligationType IobligationType
    {
      get => iobligationType ??= new();
      set => iobligationType = value;
    }

    /// <summary>
    /// A value of IobligationTransaction.
    /// </summary>
    [JsonPropertyName("iobligationTransaction")]
    [Member(Index = 8, AccessFields = false, Members = new[]
    {
      "SystemGeneratedIdentifier",
      "Type1",
      "Amount",
      "DebtType",
      "DebtAdjustmentInd",
      "CreatedTmst",
      "CreatedBy",
      "LastUpdatedBy",
      "LastUpdatedTmst",
      "DebtAdjustmentDt",
      "DebtAdjCollAdjProcReqInd",
      "DebtAdjCollAdjProcDt",
      "DebtAdjustmentType",
      "DebtAdjustmentProcessDate",
      "ReasonCode",
      "ReverseCollectionsInd",
      "VoluntaryPercentageAmount",
      "NewDebtProcessDate"
    })]
    public ObligationTransaction IobligationTransaction
    {
      get => iobligationTransaction ??= new();
      set => iobligationTransaction = value;
    }

    /// <summary>
    /// A value of Iobligation.
    /// </summary>
    [JsonPropertyName("iobligation")]
    [Member(Index = 9, Members
      = new[] { "SystemGeneratedIdentifier", "OrderTypeCode" })]
    public Obligation Iobligation
    {
      get => iobligation ??= new();
      set => iobligation = value;
    }

    /// <summary>
    /// A value of IdebtDetail.
    /// </summary>
    [JsonPropertyName("idebtDetail")]
    [Member(Index = 10, Members = new[]
    {
      "DueDt",
      "BalanceDueAmt",
      "InterestBalanceDueAmt"
    })]
    public DebtDetail IdebtDetail
    {
      get => idebtDetail ??= new();
      set => idebtDetail = value;
    }

    /// <summary>
    /// A value of Icollection.
    /// </summary>
    [JsonPropertyName("icollection")]
    [Member(Index = 11, Members = new[]
    {
      "SystemGeneratedIdentifier",
      "Amount",
      "AppliedToCode",
      "CollectionDt",
      "DisbursementDt",
      "AdjustedInd",
      "AppliedToOrderTypeCode",
      "CollectionAdjustmentDt",
      "CreatedTmst"
    })]
    public Collection Icollection
    {
      get => icollection ??= new();
      set => icollection = value;
    }

    /// <summary>
    /// A value of Ifips.
    /// </summary>
    [JsonPropertyName("ifips")]
    [Member(Index = 12, AccessFields = false, Members
      = new[] { "StateAbbreviation", "CountyDescription" })]
    public Fips Ifips
    {
      get => ifips ??= new();
      set => ifips = value;
    }

    /// <summary>
    /// A value of IeabFileHandling.
    /// </summary>
    [JsonPropertyName("ieabFileHandling")]
    [Member(Index = 13, Members = new[] { "Action", "Status" })]
    public EabFileHandling IeabFileHandling
    {
      get => ieabFileHandling ??= new();
      set => ieabFileHandling = value;
    }

    /// <summary>
    /// A value of IrespondantCsePerson.
    /// </summary>
    [JsonPropertyName("irespondantCsePerson")]
    [Member(Index = 14, AccessFields = false, Members
      = new[] { "Type1", "Number" })]
    public CsePerson IrespondantCsePerson
    {
      get => irespondantCsePerson ??= new();
      set => irespondantCsePerson = value;
    }

    /// <summary>
    /// A value of IrespondantCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("irespondantCsePersonsWorkSet")]
    [Member(Index = 15, AccessFields = false, Members = new[]
    {
      "FormattedName",
      "FirstName",
      "MiddleInitial",
      "LastName",
      "Dob"
    })]
    public CsePersonsWorkSet IrespondantCsePersonsWorkSet
    {
      get => irespondantCsePersonsWorkSet ??= new();
      set => irespondantCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of IpetitionerCsePerson.
    /// </summary>
    [JsonPropertyName("ipetitionerCsePerson")]
    [Member(Index = 16, AccessFields = false, Members
      = new[] { "Type1", "Number" })]
    public CsePerson IpetitionerCsePerson
    {
      get => ipetitionerCsePerson ??= new();
      set => ipetitionerCsePerson = value;
    }

    /// <summary>
    /// A value of IpetitionerCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("ipetitionerCsePersonsWorkSet")]
    [Member(Index = 17, AccessFields = false, Members = new[]
    {
      "FormattedName",
      "FirstName",
      "MiddleInitial",
      "LastName",
      "Dob"
    })]
    public CsePersonsWorkSet IpetitionerCsePersonsWorkSet
    {
      get => ipetitionerCsePersonsWorkSet ??= new();
      set => ipetitionerCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of IobligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("iobligationPaymentSchedule")]
    [Member(Index = 18, AccessFields = false, Members = new[]
    {
      "FrequencyCode",
      "DayOfWeek",
      "DayOfMonth1",
      "DayOfMonth2",
      "PeriodInd",
      "Amount",
      "StartDt",
      "EndDt"
    })]
    public ObligationPaymentSchedule IobligationPaymentSchedule
    {
      get => iobligationPaymentSchedule ??= new();
      set => iobligationPaymentSchedule = value;
    }

    private Common icommon;
    private LegalAction ilegalAction;
    private CsePerson icsePerson;
    private CsePersonsWorkSet iperson;
    private LegalActionPerson ilegalActionPerson;
    private LegalActionDetail ilegalActionDetail;
    private ObligationType iobligationType;
    private ObligationTransaction iobligationTransaction;
    private Obligation iobligation;
    private DebtDetail idebtDetail;
    private Collection icollection;
    private Fips ifips;
    private EabFileHandling ieabFileHandling;
    private CsePerson irespondantCsePerson;
    private CsePersonsWorkSet irespondantCsePersonsWorkSet;
    private CsePerson ipetitionerCsePerson;
    private CsePersonsWorkSet ipetitionerCsePersonsWorkSet;
    private ObligationPaymentSchedule iobligationPaymentSchedule;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }
#endregion
}
