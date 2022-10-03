// Program: OE_EAB_RECEIVE_FPLS_RESPONSE, ID: 372364149, model: 746.
// Short name: SWEXEE05
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_EAB_RECEIVE_FPLS_RESPONSE.
/// </para>
/// <para>
/// Resp:OBLGEST
/// This EXTERNAL procedure carries view that will be used for receiving the 
/// FPLS responses from the Federal Parent Locator Service in reaction to
/// FPLS_LOCATE_REQUEST's.
/// </para>
/// </summary>
[Serializable]
public partial class OeEabReceiveFplsResponse: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_EAB_RECEIVE_FPLS_RESPONSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeEabReceiveFplsResponse(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeEabReceiveFplsResponse.
  /// </summary>
  public OeEabReceiveFplsResponse(IContext context, Import import, Export export)
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
    GetService<IEabStub>().Execute(
      "SWEXEE05", context, import, export, EabOptions.Hpvp);
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
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    [Member(Index = 1, Members = new[] { "FileInstruction" })]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    private External external;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ExternalFplsResponse.
    /// </summary>
    [JsonPropertyName("externalFplsResponse")]
    [Member(Index = 1, Members = new[]
    {
      "NdnhResponse",
      "StateAbbreviation",
      "AgencyCode",
      "NameSentInd",
      "ApFirstName",
      "ApMiddleName",
      "ApFirstLastName",
      "ApSecondLastName",
      "ApThirdLastName",
      "ApNameReturned",
      "SsnSubmitted",
      "ApCsePersonNumber",
      "FplsRequestIdentifier",
      "UsersField",
      "LocalCode",
      "TypeOfCase",
      "AddrDateFormatInd",
      "DateOfAddress",
      "ResponseCode",
      "AddressFormatInd",
      "ReturnedAddress",
      "DodStatus",
      "DodServiceCode",
      "DodPayGradeCode",
      "DodAnnualSalary",
      "DodDateOfBirth",
      "SubmittingOffice",
      "SesaRespondingState",
      "SesaWageClaimInd",
      "SesaWageAmount",
      "IrsNameControl",
      "CpSsn",
      "IrsTaxYear",
      "NprcEmpdOrSepd",
      "SsaFederalOrMilitary",
      "SsaCorpDivision",
      "MbrBenefitAmount",
      "MbrDateOfDeath",
      "VaBenefitCode",
      "VaDateOfDeath",
      "VaAmtOfAwardEffectiveDate",
      "VaAmountOfAward",
      "VaSuspenseCode",
      "VaIncarcerationCode",
      "VaRetirementPayCode",
      "DodDateOfDeathOrSeparation",
      "DodEligibilityCode",
      "AddressIndType",
      "HealthInsBenefitIndicator",
      "EmploymentStatus",
      "EmploymentInd",
      "DateOfHire",
      "ReportingFedAgency",
      "Fein",
      "CorrectedAdditionMultipleSsn",
      "SsnMatchInd",
      "ReportingQuarter",
      "StationNumber",
      "NsaDateOfDeath"
    })]
    public ExternalFplsResponse ExternalFplsResponse
    {
      get => externalFplsResponse ??= new();
      set => externalFplsResponse = value;
    }

    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    [Member(Index = 2, Members = new[]
    {
      "FileInstruction",
      "NumericReturnCode",
      "TextReturnCode",
      "TextLine80"
    })]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    /// <summary>
    /// A value of DateOfDeathIndicator.
    /// </summary>
    [JsonPropertyName("dateOfDeathIndicator")]
    [Member(Index = 3, AccessFields = false, Members = new[] { "Text1" })]
    public TextWorkArea DateOfDeathIndicator
    {
      get => dateOfDeathIndicator ??= new();
      set => dateOfDeathIndicator = value;
    }

    private ExternalFplsResponse externalFplsResponse;
    private External external;
    private TextWorkArea dateOfDeathIndicator;
  }
#endregion
}
