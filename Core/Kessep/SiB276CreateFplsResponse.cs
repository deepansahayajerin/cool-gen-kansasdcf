// Program: SI_B276_CREATE_FPLS_RESPONSE, ID: 373400581, model: 746.
// Short name: SWE01301
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B276_CREATE_FPLS_RESPONSE.
/// </summary>
[Serializable]
public partial class SiB276CreateFplsResponse: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B276_CREATE_FPLS_RESPONSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB276CreateFplsResponse(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB276CreateFplsResponse.
  /// </summary>
  public SiB276CreateFplsResponse(IContext context, Import import, Export export)
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
    if (ReadFplsLocateRequest())
    {
      try
      {
        CreateFplsLocateResponse();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FPLS_LOCATE_RESPONSE_AE";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FPLS_LOCATE_RESPONSE_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    else
    {
      ExitState = "FPLS_LOCATE_REQUEST_NF";
    }
  }

  private void CreateFplsLocateResponse()
  {
    System.Diagnostics.Debug.Assert(entities.FplsLocateRequest.Populated);

    var flqIdentifier = entities.FplsLocateRequest.Identifier;
    var cspNumber = entities.FplsLocateRequest.CspNumber;
    var identifier = import.FplsLocateResponse.Identifier;
    var dateReceived = import.FplsLocateResponse.DateReceived;
    var usageStatus = import.FplsLocateResponse.UsageStatus ?? "";
    var dateUsed = import.FplsLocateResponse.DateUsed;
    var agencyCode = import.FplsLocateResponse.AgencyCode ?? "";
    var nameSentInd = import.FplsLocateResponse.NameSentInd ?? "";
    var apNameReturned = import.FplsLocateResponse.ApNameReturned ?? "";
    var addrDateFormatInd = import.FplsLocateResponse.AddrDateFormatInd ?? "";
    var dateOfAddress = import.FplsLocateResponse.DateOfAddress;
    var responseCode = import.FplsLocateResponse.ResponseCode ?? "";
    var addressFormatInd = import.FplsLocateResponse.AddressFormatInd ?? "";
    var dodEligibilityCode = import.FplsLocateResponse.DodEligibilityCode ?? "";
    var dodDateOfDeathOrSeparation =
      import.FplsLocateResponse.DodDateOfDeathOrSeparation;
    var dodStatus = import.FplsLocateResponse.DodStatus ?? "";
    var dodServiceCode = import.FplsLocateResponse.DodServiceCode ?? "";
    var dodPayGradeCode = import.FplsLocateResponse.DodPayGradeCode ?? "";
    var sesaRespondingState = import.FplsLocateResponse.SesaRespondingState ?? ""
      ;
    var sesaWageClaimInd = import.FplsLocateResponse.SesaWageClaimInd ?? "";
    var sesaWageAmount =
      import.FplsLocateResponse.SesaWageAmount.GetValueOrDefault();
    var irsNameControl = import.FplsLocateResponse.IrsNameControl ?? "";
    var irsTaxYear = import.FplsLocateResponse.IrsTaxYear.GetValueOrDefault();
    var nprcEmpdOrSepd = import.FplsLocateResponse.NprcEmpdOrSepd ?? "";
    var ssaFederalOrMilitary =
      import.FplsLocateResponse.SsaFederalOrMilitary ?? "";
    var ssaCorpDivision = import.FplsLocateResponse.SsaCorpDivision ?? "";
    var mbrBenefitAmount =
      import.FplsLocateResponse.MbrBenefitAmount.GetValueOrDefault();
    var mbrDateOfDeath = import.FplsLocateResponse.MbrDateOfDeath;
    var vaBenefitCode = import.FplsLocateResponse.VaBenefitCode ?? "";
    var vaDateOfDeath = import.FplsLocateResponse.VaDateOfDeath;
    var vaAmtOfAwardEffectiveDate =
      import.FplsLocateResponse.VaAmtOfAwardEffectiveDate;
    var vaAmountOfAward =
      import.FplsLocateResponse.VaAmountOfAward.GetValueOrDefault();
    var vaSuspenseCode = import.FplsLocateResponse.VaSuspenseCode ?? "";
    var vaIncarcerationCode = import.FplsLocateResponse.VaIncarcerationCode ?? ""
      ;
    var vaRetirementPayCode = import.FplsLocateResponse.VaRetirementPayCode ?? ""
      ;
    var createdBy = import.FplsLocateResponse.CreatedBy ?? "";
    var createdTimestamp = Now();
    var lastUpdatedBy = import.FplsLocateResponse.LastUpdatedBy ?? "";
    var returnedAddress = import.FplsLocateResponse.ReturnedAddress ?? "";
    var ssnReturned = import.FplsLocateResponse.SsnReturned ?? "";
    var dodAnnualSalary =
      import.FplsLocateResponse.DodAnnualSalary.GetValueOrDefault();
    var dodDateOfBirth = import.FplsLocateResponse.DodDateOfBirth;
    var submittingOffice = import.FplsLocateResponse.SubmittingOffice ?? "";
    var addressIndType = import.FplsLocateResponse.AddressIndType ?? "";
    var healthInsBenefitIndicator =
      import.FplsLocateResponse.HealthInsBenefitIndicator ?? "";
    var employmentStatus = import.FplsLocateResponse.EmploymentStatus ?? "";
    var employmentInd = import.FplsLocateResponse.EmploymentInd ?? "";
    var dateOfHire = import.FplsLocateResponse.DateOfHire;
    var reportingFedAgency = import.FplsLocateResponse.ReportingFedAgency ?? "";
    var fein = import.FplsLocateResponse.Fein ?? "";
    var correctedAdditionMultipleSsn =
      import.FplsLocateResponse.CorrectedAdditionMultipleSsn ?? "";
    var ssnMatchInd = import.FplsLocateResponse.SsnMatchInd ?? "";
    var reportingQuarter = import.FplsLocateResponse.ReportingQuarter ?? "";
    var ndnhResponse = import.FplsLocateResponse.NdnhResponse ?? "";

    entities.FplsLocateResponse.Populated = false;
    Update("CreateFplsLocateResponse",
      (db, command) =>
      {
        db.SetInt32(command, "flqIdentifier", flqIdentifier);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "identifier", identifier);
        db.SetNullableDate(command, "dateReceived", dateReceived);
        db.SetNullableString(command, "usageStatus", usageStatus);
        db.SetNullableDate(command, "dateUsed", dateUsed);
        db.SetNullableString(command, "agencyCode", agencyCode);
        db.SetNullableString(command, "nameSentInd", nameSentInd);
        db.SetNullableString(command, "apNameReturned", apNameReturned);
        db.SetNullableString(command, "addrDateFormatI", addrDateFormatInd);
        db.SetNullableDate(command, "dateOfAddress", dateOfAddress);
        db.SetNullableString(command, "responseCode", responseCode);
        db.SetNullableString(command, "addressFormatInd", addressFormatInd);
        db.SetNullableString(command, "dodEligCode", dodEligibilityCode);
        db.
          SetNullableDate(command, "dodDtDeathSepn", dodDateOfDeathOrSeparation);
          
        db.SetNullableString(command, "dodStatus", dodStatus);
        db.SetNullableString(command, "dodServiceCode", dodServiceCode);
        db.SetNullableString(command, "dodPayGradeCode", dodPayGradeCode);
        db.SetNullableString(command, "sesaRespondingSt", sesaRespondingState);
        db.SetNullableString(command, "sesaWageClmInd", sesaWageClaimInd);
        db.SetNullableInt32(command, "sesaWageAmount", sesaWageAmount);
        db.SetNullableString(command, "irsNameControl", irsNameControl);
        db.SetNullableInt32(command, "irsTaxYear", irsTaxYear);
        db.SetNullableString(command, "nprcEmpdOrSepd", nprcEmpdOrSepd);
        db.SetNullableString(command, "ssaFedMilitary", ssaFederalOrMilitary);
        db.SetNullableString(command, "ssaCorpDivision", ssaCorpDivision);
        db.SetNullableInt32(command, "mbrBenefitAmount", mbrBenefitAmount);
        db.SetNullableDate(command, "mbrDateOfDeath", mbrDateOfDeath);
        db.SetNullableString(command, "vaBenefitCode", vaBenefitCode);
        db.SetNullableDate(command, "vaDateOfDeath", vaDateOfDeath);
        db.SetNullableDate(command, "vaAwdAmtEffDt", vaAmtOfAwardEffectiveDate);
        db.SetNullableInt32(command, "vaAmountOfAward", vaAmountOfAward);
        db.SetNullableString(command, "vaSuspenseCode", vaSuspenseCode);
        db.SetNullableString(command, "vaIncarcCode", vaIncarcerationCode);
        db.SetNullableString(command, "vaRetirePayCode", vaRetirementPayCode);
        db.SetNullableString(command, "createdBy", createdBy);
        db.SetNullableDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetNullableString(command, "returnedAddress", returnedAddress);
        db.SetNullableString(command, "ssnReturned", ssnReturned);
        db.SetNullableInt32(command, "dodAnnualSalary", dodAnnualSalary);
        db.SetNullableDate(command, "dodDateOfBirth", dodDateOfBirth);
        db.SetNullableString(command, "submittingOffice", submittingOffice);
        db.SetNullableString(command, "addressIndType", addressIndType);
        db.SetNullableString(
          command, "healthInsBenInd", healthInsBenefitIndicator);
        db.SetNullableString(command, "employmentStatus", employmentStatus);
        db.SetNullableString(command, "employmentInd", employmentInd);
        db.SetNullableDate(command, "dateOfHire", dateOfHire);
        db.SetNullableString(command, "repFedAgency", reportingFedAgency);
        db.SetNullableString(command, "fein", fein);
        db.SetNullableString(
          command, "corAddMulSsn", correctedAdditionMultipleSsn);
        db.SetNullableString(command, "ssnMatchInd", ssnMatchInd);
        db.SetNullableString(command, "reportingQuarter", reportingQuarter);
        db.SetNullableString(command, "ndnhResponse", ndnhResponse);
      });

    entities.FplsLocateResponse.FlqIdentifier = flqIdentifier;
    entities.FplsLocateResponse.CspNumber = cspNumber;
    entities.FplsLocateResponse.Identifier = identifier;
    entities.FplsLocateResponse.DateReceived = dateReceived;
    entities.FplsLocateResponse.UsageStatus = usageStatus;
    entities.FplsLocateResponse.DateUsed = dateUsed;
    entities.FplsLocateResponse.AgencyCode = agencyCode;
    entities.FplsLocateResponse.NameSentInd = nameSentInd;
    entities.FplsLocateResponse.ApNameReturned = apNameReturned;
    entities.FplsLocateResponse.AddrDateFormatInd = addrDateFormatInd;
    entities.FplsLocateResponse.DateOfAddress = dateOfAddress;
    entities.FplsLocateResponse.ResponseCode = responseCode;
    entities.FplsLocateResponse.AddressFormatInd = addressFormatInd;
    entities.FplsLocateResponse.DodEligibilityCode = dodEligibilityCode;
    entities.FplsLocateResponse.DodDateOfDeathOrSeparation =
      dodDateOfDeathOrSeparation;
    entities.FplsLocateResponse.DodStatus = dodStatus;
    entities.FplsLocateResponse.DodServiceCode = dodServiceCode;
    entities.FplsLocateResponse.DodPayGradeCode = dodPayGradeCode;
    entities.FplsLocateResponse.SesaRespondingState = sesaRespondingState;
    entities.FplsLocateResponse.SesaWageClaimInd = sesaWageClaimInd;
    entities.FplsLocateResponse.SesaWageAmount = sesaWageAmount;
    entities.FplsLocateResponse.IrsNameControl = irsNameControl;
    entities.FplsLocateResponse.IrsTaxYear = irsTaxYear;
    entities.FplsLocateResponse.NprcEmpdOrSepd = nprcEmpdOrSepd;
    entities.FplsLocateResponse.SsaFederalOrMilitary = ssaFederalOrMilitary;
    entities.FplsLocateResponse.SsaCorpDivision = ssaCorpDivision;
    entities.FplsLocateResponse.MbrBenefitAmount = mbrBenefitAmount;
    entities.FplsLocateResponse.MbrDateOfDeath = mbrDateOfDeath;
    entities.FplsLocateResponse.VaBenefitCode = vaBenefitCode;
    entities.FplsLocateResponse.VaDateOfDeath = vaDateOfDeath;
    entities.FplsLocateResponse.VaAmtOfAwardEffectiveDate =
      vaAmtOfAwardEffectiveDate;
    entities.FplsLocateResponse.VaAmountOfAward = vaAmountOfAward;
    entities.FplsLocateResponse.VaSuspenseCode = vaSuspenseCode;
    entities.FplsLocateResponse.VaIncarcerationCode = vaIncarcerationCode;
    entities.FplsLocateResponse.VaRetirementPayCode = vaRetirementPayCode;
    entities.FplsLocateResponse.CreatedBy = createdBy;
    entities.FplsLocateResponse.CreatedTimestamp = createdTimestamp;
    entities.FplsLocateResponse.LastUpdatedBy = lastUpdatedBy;
    entities.FplsLocateResponse.LastUpdatedTimestamp = createdTimestamp;
    entities.FplsLocateResponse.ReturnedAddress = returnedAddress;
    entities.FplsLocateResponse.SsnReturned = ssnReturned;
    entities.FplsLocateResponse.DodAnnualSalary = dodAnnualSalary;
    entities.FplsLocateResponse.DodDateOfBirth = dodDateOfBirth;
    entities.FplsLocateResponse.SubmittingOffice = submittingOffice;
    entities.FplsLocateResponse.AddressIndType = addressIndType;
    entities.FplsLocateResponse.HealthInsBenefitIndicator =
      healthInsBenefitIndicator;
    entities.FplsLocateResponse.EmploymentStatus = employmentStatus;
    entities.FplsLocateResponse.EmploymentInd = employmentInd;
    entities.FplsLocateResponse.DateOfHire = dateOfHire;
    entities.FplsLocateResponse.ReportingFedAgency = reportingFedAgency;
    entities.FplsLocateResponse.Fein = fein;
    entities.FplsLocateResponse.CorrectedAdditionMultipleSsn =
      correctedAdditionMultipleSsn;
    entities.FplsLocateResponse.SsnMatchInd = ssnMatchInd;
    entities.FplsLocateResponse.ReportingQuarter = reportingQuarter;
    entities.FplsLocateResponse.NdnhResponse = ndnhResponse;
    entities.FplsLocateResponse.Populated = true;
  }

  private bool ReadFplsLocateRequest()
  {
    entities.FplsLocateRequest.Populated = false;

    return Read("ReadFplsLocateRequest",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", import.FplsLocateRequest.Identifier);
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.FplsLocateRequest.CspNumber = db.GetString(reader, 0);
        entities.FplsLocateRequest.Identifier = db.GetInt32(reader, 1);
        entities.FplsLocateRequest.Populated = true;
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
    /// A value of FplsLocateResponse.
    /// </summary>
    [JsonPropertyName("fplsLocateResponse")]
    public FplsLocateResponse FplsLocateResponse
    {
      get => fplsLocateResponse ??= new();
      set => fplsLocateResponse = value;
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

    /// <summary>
    /// A value of FplsLocateRequest.
    /// </summary>
    [JsonPropertyName("fplsLocateRequest")]
    public FplsLocateRequest FplsLocateRequest
    {
      get => fplsLocateRequest ??= new();
      set => fplsLocateRequest = value;
    }

    private FplsLocateResponse fplsLocateResponse;
    private CsePerson csePerson;
    private FplsLocateRequest fplsLocateRequest;
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
    /// A value of FplsLocateResponse.
    /// </summary>
    [JsonPropertyName("fplsLocateResponse")]
    public FplsLocateResponse FplsLocateResponse
    {
      get => fplsLocateResponse ??= new();
      set => fplsLocateResponse = value;
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

    /// <summary>
    /// A value of FplsLocateRequest.
    /// </summary>
    [JsonPropertyName("fplsLocateRequest")]
    public FplsLocateRequest FplsLocateRequest
    {
      get => fplsLocateRequest ??= new();
      set => fplsLocateRequest = value;
    }

    private FplsLocateResponse fplsLocateResponse;
    private CsePerson csePerson;
    private FplsLocateRequest fplsLocateRequest;
  }
#endregion
}
