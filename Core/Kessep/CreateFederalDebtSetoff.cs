// Program: CREATE_FEDERAL_DEBT_SETOFF, ID: 372665443, model: 746.
// Short name: SWE02380
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: CREATE_FEDERAL_DEBT_SETOFF.
/// </para>
/// <para>
/// Create Federal_Debt_Setoff for FDSO reporting.
/// </para>
/// </summary>
[Serializable]
public partial class CreateFederalDebtSetoff: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CREATE_FEDERAL_DEBT_SETOFF program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CreateFederalDebtSetoff(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CreateFederalDebtSetoff.
  /// </summary>
  public CreateFederalDebtSetoff(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (!ReadObligorCsePerson())
    {
      ExitState = "CSE_PERSON_ACCOUNT_NF";

      return;
    }

    if (!ReadAdministrativeAction())
    {
      ExitState = "ADMINISTRATIVE_ACTION_NF";

      return;
    }

    try
    {
      CreateFederalDebtSetoff1();
      import.Export1.CreatedBy = entities.New1.CreatedBy;
      import.Export1.CreatedTstamp = entities.New1.CreatedTstamp;
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FEDERAL_DEBT_SETOFF_AE";

          break;
        case ErrorCode.PermittedValueViolation:
          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private void CreateFederalDebtSetoff1()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingObligor.Populated);

    var cpaType = entities.ExistingObligor.Type1;
    var cspNumber = entities.ExistingObligor.CspNumber;
    var type1 = "FDSO";
    var takenDate = import.Export1.TakenDate;
    var aatType = entities.ExistingAdministrativeAction.Type1;
    var originalAmount = import.Export1.OriginalAmount.GetValueOrDefault();
    var currentAmount = import.Export1.CurrentAmount.GetValueOrDefault();
    var currentAmountDate = import.Export1.CurrentAmountDate;
    var decertifiedDate = import.Export1.DecertifiedDate;
    var createdBy = global.UserId;
    var createdTstamp = Now();
    var adcAmount = import.Export1.AdcAmount.GetValueOrDefault();
    var nonAdcAmount = import.Export1.NonAdcAmount.GetValueOrDefault();
    var injuredSpouseDate = import.Export1.InjuredSpouseDate;
    var etypeAdministrativeOffset =
      import.Export1.EtypeAdministrativeOffset ?? "";
    var localCode = import.Export1.LocalCode ?? "";
    var ssn = import.Export1.Ssn;
    var caseNumber = import.Export1.CaseNumber;
    var lastName = import.Export1.LastName;
    var firstName = import.Export1.FirstName;
    var amountOwed = import.Export1.AmountOwed;
    var ttypeAAddNewCase = import.Export1.TtypeAAddNewCase ?? "";
    var caseType = import.Export1.CaseType;
    var transferState = import.Export1.TransferState ?? "";
    var localForTransfer = import.Export1.LocalForTransfer.GetValueOrDefault();
    var processYear = import.Export1.ProcessYear.GetValueOrDefault();
    var tanfCode = import.Export1.TanfCode;
    var ttypeDDeleteCertification =
      import.Export1.TtypeDDeleteCertification ?? "";
    var ttypeLChangeSubmittingState =
      import.Export1.TtypeLChangeSubmittingState ?? "";
    var ttypeMModifyAmount = import.Export1.TtypeMModifyAmount ?? "";
    var ttypeRModifyExclusion = import.Export1.TtypeRModifyExclusion ?? "";
    var ttypeSStatePayment = import.Export1.TtypeSStatePayment ?? "";
    var ttypeTTransferAdminReview =
      import.Export1.TtypeTTransferAdminReview ?? "";
    var ttypeBNameChange = import.Export1.TtypeBNameChange ?? "";
    var ttypeZAddressChange = import.Export1.TtypeZAddressChange ?? "";
    var etypeFederalRetirement = import.Export1.EtypeFederalRetirement ?? "";
    var etypeFederalSalary = import.Export1.EtypeFederalSalary ?? "";
    var etypeTaxRefund = import.Export1.EtypeTaxRefund ?? "";
    var etypeVendorPaymentOrMisc = import.Export1.EtypeVendorPaymentOrMisc ?? ""
      ;
    var etypePassportDenial = import.Export1.EtypePassportDenial ?? "";
    var etypeFinancialInstitution =
      import.Export1.EtypeFinancialInstitution ?? "";
    var changeSsnInd = import.Export1.ChangeSsnInd ?? "";
    var etypeAdmBankrupt = import.Export1.EtypeAdmBankrupt ?? "";
    var decertificationReason = import.Export1.DecertificationReason ?? "";
    var addressStreet1 = import.Export1.AddressStreet1 ?? "";
    var addressStreet2 = import.Export1.AddressStreet2 ?? "";
    var addressCity = import.Export1.AddressCity ?? "";
    var addressState = import.Export1.AddressState ?? "";
    var addressZip = import.Export1.AddressZip ?? "";

    CheckValid<AdministrativeActCertification>("CpaType", cpaType);
    CheckValid<AdministrativeActCertification>("Type1", type1);
    entities.New1.Populated = false;
    Update("CreateFederalDebtSetoff",
      (db, command) =>
      {
        db.SetString(command, "cpaType", cpaType);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "type", type1);
        db.SetDate(command, "takenDt", takenDate);
        db.SetNullableString(command, "aatType", aatType);
        db.SetNullableDecimal(command, "originalAmt", originalAmount);
        db.SetNullableDecimal(command, "currentAmt", currentAmount);
        db.SetNullableDate(command, "currentAmtDt", currentAmountDate);
        db.SetNullableDate(command, "decertifiedDt", decertifiedDate);
        db.SetNullableDate(command, "notificationDt", null);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdTstamp", null);
        db.SetNullableDate(command, "referralDt", default(DateTime));
        db.SetNullableDecimal(command, "recoveryAmt", 0M);
        db.SetNullableDecimal(command, "adcAmt", adcAmount);
        db.SetNullableDecimal(command, "nonAdcAmt", nonAdcAmount);
        db.SetNullableDate(command, "injuredSpouseDt", injuredSpouseDate);
        db.SetString(command, "witness", "");
        db.SetNullableString(command, "notifiedBy", "");
        db.SetNullableString(command, "reasonWithdraw", "");
        db.SetNullableString(command, "denialReason", "");
        db.SetNullableDate(command, "dateSent", null);
        db.SetNullableString(
          command, "etypeAdminOffset", etypeAdministrativeOffset);
        db.SetNullableString(command, "localCode", localCode);
        db.SetInt32(command, "ssn", ssn);
        db.SetString(command, "caseNumber", caseNumber);
        db.SetString(command, "lastName", lastName);
        db.SetString(command, "firstName", firstName);
        db.SetInt32(command, "amountOwed", amountOwed);
        db.SetNullableString(command, "ttypeAddNewCase", ttypeAAddNewCase);
        db.SetString(command, "caseType", caseType);
        db.SetNullableString(command, "transferState", transferState);
        db.SetNullableInt32(command, "localForTransfer", localForTransfer);
        db.SetNullableInt32(command, "processYear", processYear);
        db.SetString(command, "tanfCode", tanfCode);
        db.SetNullableString(
          command, "ttypeDeleteCert", ttypeDDeleteCertification);
        db.SetNullableString(
          command, "ttypeChngSubSt", ttypeLChangeSubmittingState);
        db.SetNullableString(command, "ttypeModifyAmnt", ttypeMModifyAmount);
        db.SetNullableString(command, "ttypeModifyExcl", ttypeRModifyExclusion);
        db.SetNullableString(command, "ttypeStatePymnt", ttypeSStatePayment);
        db.SetNullableString(
          command, "ttypeXferAdmRvw", ttypeTTransferAdminReview);
        db.SetNullableString(command, "ttypeNameChange", ttypeBNameChange);
        db.SetNullableString(command, "ttypeAddressChg", ttypeZAddressChange);
        db.
          SetNullableString(command, "etypeFedRetrmnt", etypeFederalRetirement);
          
        db.SetNullableString(command, "etypeFedSalary", etypeFederalSalary);
        db.SetNullableString(command, "etypeTaxRefund", etypeTaxRefund);
        db.SetNullableString(
          command, "etypeVndrPymntM", etypeVendorPaymentOrMisc);
        db.SetNullableString(command, "etypePsprtDenial", etypePassportDenial);
        db.
          SetNullableString(command, "etypeFinInst", etypeFinancialInstitution);
          
        db.SetNullableString(command, "returnStatus", "");
        db.SetNullableString(command, "changeSsnInd", changeSsnInd);
        db.SetNullableString(command, "etypeAdmBankrupt", etypeAdmBankrupt);
        db.SetNullableString(command, "decertifyReason", decertificationReason);
        db.SetNullableString(command, "addressStreet1", addressStreet1);
        db.SetNullableString(command, "addressStreet2", addressStreet2);
        db.SetNullableString(command, "addressCity", addressCity);
        db.SetNullableString(command, "addressState", addressState);
        db.SetNullableString(command, "addressZip", addressZip);
        db.SetNullableInt32(command, "numCourtOrders", 0);
      });

    entities.New1.CpaType = cpaType;
    entities.New1.CspNumber = cspNumber;
    entities.New1.Type1 = type1;
    entities.New1.TakenDate = takenDate;
    entities.New1.AatType = aatType;
    entities.New1.OriginalAmount = originalAmount;
    entities.New1.CurrentAmount = currentAmount;
    entities.New1.CurrentAmountDate = currentAmountDate;
    entities.New1.DecertifiedDate = decertifiedDate;
    entities.New1.NotificationDate = null;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CreatedTstamp = createdTstamp;
    entities.New1.LastUpdatedBy = "";
    entities.New1.LastUpdatedTstamp = null;
    entities.New1.AdcAmount = adcAmount;
    entities.New1.NonAdcAmount = nonAdcAmount;
    entities.New1.InjuredSpouseDate = injuredSpouseDate;
    entities.New1.NotifiedBy = "";
    entities.New1.DateSent = null;
    entities.New1.EtypeAdministrativeOffset = etypeAdministrativeOffset;
    entities.New1.LocalCode = localCode;
    entities.New1.Ssn = ssn;
    entities.New1.CaseNumber = caseNumber;
    entities.New1.LastName = lastName;
    entities.New1.FirstName = firstName;
    entities.New1.AmountOwed = amountOwed;
    entities.New1.TtypeAAddNewCase = ttypeAAddNewCase;
    entities.New1.CaseType = caseType;
    entities.New1.TransferState = transferState;
    entities.New1.LocalForTransfer = localForTransfer;
    entities.New1.ProcessYear = processYear;
    entities.New1.TanfCode = tanfCode;
    entities.New1.TtypeDDeleteCertification = ttypeDDeleteCertification;
    entities.New1.TtypeLChangeSubmittingState = ttypeLChangeSubmittingState;
    entities.New1.TtypeMModifyAmount = ttypeMModifyAmount;
    entities.New1.TtypeRModifyExclusion = ttypeRModifyExclusion;
    entities.New1.TtypeSStatePayment = ttypeSStatePayment;
    entities.New1.TtypeTTransferAdminReview = ttypeTTransferAdminReview;
    entities.New1.TtypeBNameChange = ttypeBNameChange;
    entities.New1.TtypeZAddressChange = ttypeZAddressChange;
    entities.New1.EtypeFederalRetirement = etypeFederalRetirement;
    entities.New1.EtypeFederalSalary = etypeFederalSalary;
    entities.New1.EtypeTaxRefund = etypeTaxRefund;
    entities.New1.EtypeVendorPaymentOrMisc = etypeVendorPaymentOrMisc;
    entities.New1.EtypePassportDenial = etypePassportDenial;
    entities.New1.EtypeFinancialInstitution = etypeFinancialInstitution;
    entities.New1.ChangeSsnInd = changeSsnInd;
    entities.New1.EtypeAdmBankrupt = etypeAdmBankrupt;
    entities.New1.DecertificationReason = decertificationReason;
    entities.New1.AddressStreet1 = addressStreet1;
    entities.New1.AddressStreet2 = addressStreet2;
    entities.New1.AddressCity = addressCity;
    entities.New1.AddressState = addressState;
    entities.New1.AddressZip = addressZip;
    entities.New1.Populated = true;
  }

  private bool ReadAdministrativeAction()
  {
    entities.ExistingAdministrativeAction.Populated = false;

    return Read("ReadAdministrativeAction",
      null,
      (db, reader) =>
      {
        entities.ExistingAdministrativeAction.Type1 = db.GetString(reader, 0);
        entities.ExistingAdministrativeAction.Populated = true;
      });
  }

  private bool ReadObligorCsePerson()
  {
    entities.ExistingCsePerson.Populated = false;
    entities.ExistingObligor.Populated = false;

    return Read("ReadObligorCsePerson",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingObligor.CspNumber = db.GetString(reader, 0);
        entities.ExistingCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingObligor.Type1 = db.GetString(reader, 1);
        entities.ExistingCsePerson.Populated = true;
        entities.ExistingObligor.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.ExistingObligor.Type1);
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Entities entities = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of Export1.
    /// </summary>
    [JsonPropertyName("export1")]
    public AdministrativeActCertification Export1
    {
      get => export1 ??= new();
      set => export1 = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
    private AdministrativeActCertification export1;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public AdministrativeActCertification New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of ExistingAdministrativeAction.
    /// </summary>
    [JsonPropertyName("existingAdministrativeAction")]
    public AdministrativeAction ExistingAdministrativeAction
    {
      get => existingAdministrativeAction ??= new();
      set => existingAdministrativeAction = value;
    }

    /// <summary>
    /// A value of ExistingCsePerson.
    /// </summary>
    [JsonPropertyName("existingCsePerson")]
    public CsePerson ExistingCsePerson
    {
      get => existingCsePerson ??= new();
      set => existingCsePerson = value;
    }

    /// <summary>
    /// A value of ExistingObligor.
    /// </summary>
    [JsonPropertyName("existingObligor")]
    public CsePersonAccount ExistingObligor
    {
      get => existingObligor ??= new();
      set => existingObligor = value;
    }

    private AdministrativeActCertification new1;
    private AdministrativeAction existingAdministrativeAction;
    private CsePerson existingCsePerson;
    private CsePersonAccount existingObligor;
  }
#endregion
}
