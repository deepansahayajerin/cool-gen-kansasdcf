// Program: OE_EAB_READ_FCR_ACK_RECORDS, ID: 373550409, model: 746.
// Short name: SWEXER06
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_EAB_READ_FCR_ACK_RECORDS.
/// </para>
/// <para>
/// This EAB returns the PROACTIVE MATCH RESPONSE records one at a time.
/// </para>
/// </summary>
[Serializable]
public partial class OeEabReadFcrAckRecords: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_EAB_READ_FCR_ACK_RECORDS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeEabReadFcrAckRecords(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeEabReadFcrAckRecords.
  /// </summary>
  public OeEabReadFcrAckRecords(IContext context, Import import, Export export):
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
      "SWEXER06", context, import, export, EabOptions.Hpvp);
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
    [Member(Index = 1, AccessFields = false, Members = new[]
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "Count" })]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    private External external;
    private Common common;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    [Member(Index = 1, AccessFields = false, Members = new[]
    {
      "FileInstruction",
      "NumericReturnCode",
      "TextReturnCode",
      "TextLine80",
      "TextLine8"
    })]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    /// <summary>
    /// A value of FcrCaseAckErrorRecord.
    /// </summary>
    [JsonPropertyName("fcrCaseAckErrorRecord")]
    [Member(Index = 2, AccessFields = false, Members = new[]
    {
      "RecordIdentifier",
      "ActionTypeCode",
      "CaseId",
      "CaseType",
      "OrderIndicator",
      "FipsCountyCode",
      "UserField",
      "PreviousCaseId",
      "BatchNumber",
      "AcknowledgementCode",
      "ErrorCode1",
      "ErrorCode2",
      "ErrorCode3",
      "ErrorCode4",
      "ErrorCode5"
    })]
    public FcrCaseAckErrorRecord FcrCaseAckErrorRecord
    {
      get => fcrCaseAckErrorRecord ??= new();
      set => fcrCaseAckErrorRecord = value;
    }

    /// <summary>
    /// A value of FcrPersonAckErrorRecord.
    /// </summary>
    [JsonPropertyName("fcrPersonAckErrorRecord")]
    [Member(Index = 3, AccessFields = false, Members = new[]
    {
      "RecordIdentifier",
      "ActionTypeCode",
      "CaseId",
      "UserField",
      "FipsCountyCode",
      "LocateRequestType",
      "BundleFplsLocateResults",
      "ParticipantType",
      "FamilyViolence",
      "MemberId",
      "SexCode",
      "DateOfBirth",
      "Ssn",
      "PreviousSsn",
      "FirstName",
      "MiddleName",
      "LastName",
      "CityOfBirth",
      "StateOrCountryOfBirth",
      "FathersFirstName",
      "FathersMiddleInitial",
      "FathersLastName",
      "MothersFirstName",
      "MothersMiddleInitial",
      "MothersMaidenName",
      "IrsUSsn",
      "AdditionalSsn1",
      "AdditionalSsn2",
      "AdditionalFirstName1",
      "AdditionalMiddleName1",
      "AdditionalLastName1",
      "AdditionalFirstName2",
      "AdditionalMiddleName2",
      "AdditionalLastName2",
      "AdditionalFirstName3",
      "AdditionalMiddleName3",
      "AdditionalLastName3",
      "AdditionalFirstName4",
      "AdditionalMiddleName4",
      "AdditionalLastName4",
      "NewMemberId",
      "Irs1099",
      "LocateSource1",
      "LocateSource2",
      "LocateSource3",
      "LocateSource4",
      "LocateSource5",
      "LocateSource6",
      "LocateSource7",
      "LocateSource8",
      "SsnValidityCode",
      "ProvidedOrCorrectedSsn",
      "MultipleSsn1",
      "MultipleSsn2",
      "MultipleSsn3",
      "SsaDateOfBirthIndicator",
      "BatchNumber",
      "DateOfDeath",
      "SsaZipCodeOfLastResidence",
      "SsaZipCodeOfLumpSumPayment",
      "FcrPrimarySsn",
      "FcrPrimaryFirstName",
      "FcrPrimaryMiddleName",
      "FcrPrimaryLastName",
      "AcknowledgementCode",
      "ErrorCode1",
      "ErrorCode2",
      "ErrorCode3",
      "ErrorCode4",
      "ErrorCode5",
      "AdditionalSsn1ValidityCode",
      "AdditionalSsn2ValidityCode"
    })]
    public FcrPersonAckErrorRecord FcrPersonAckErrorRecord
    {
      get => fcrPersonAckErrorRecord ??= new();
      set => fcrPersonAckErrorRecord = value;
    }

    /// <summary>
    /// A value of SsaCityLastResidence.
    /// </summary>
    [JsonPropertyName("ssaCityLastResidence")]
    [Member(Index = 4, AccessFields = false, Members = new[] { "Text15" })]
    public WorkArea SsaCityLastResidence
    {
      get => ssaCityLastResidence ??= new();
      set => ssaCityLastResidence = value;
    }

    /// <summary>
    /// A value of SsaStateLastResidence.
    /// </summary>
    [JsonPropertyName("ssaStateLastResidence")]
    [Member(Index = 5, AccessFields = false, Members = new[] { "Text2" })]
    public WorkArea SsaStateLastResidence
    {
      get => ssaStateLastResidence ??= new();
      set => ssaStateLastResidence = value;
    }

    /// <summary>
    /// A value of SsaCityLumpSumPayment.
    /// </summary>
    [JsonPropertyName("ssaCityLumpSumPayment")]
    [Member(Index = 6, AccessFields = false, Members = new[] { "Text15" })]
    public WorkArea SsaCityLumpSumPayment
    {
      get => ssaCityLumpSumPayment ??= new();
      set => ssaCityLumpSumPayment = value;
    }

    /// <summary>
    /// A value of SsaStateLumpSumPaymnt.
    /// </summary>
    [JsonPropertyName("ssaStateLumpSumPaymnt")]
    [Member(Index = 7, AccessFields = false, Members = new[] { "Text2" })]
    public WorkArea SsaStateLumpSumPaymnt
    {
      get => ssaStateLumpSumPaymnt ??= new();
      set => ssaStateLumpSumPaymnt = value;
    }

    /// <summary>
    /// A value of DateOfDeathIndicator.
    /// </summary>
    [JsonPropertyName("dateOfDeathIndicator")]
    [Member(Index = 8, AccessFields = false, Members = new[] { "Flag" })]
    public Common DateOfDeathIndicator
    {
      get => dateOfDeathIndicator ??= new();
      set => dateOfDeathIndicator = value;
    }

    private External external;
    private FcrCaseAckErrorRecord fcrCaseAckErrorRecord;
    private FcrPersonAckErrorRecord fcrPersonAckErrorRecord;
    private WorkArea ssaCityLastResidence;
    private WorkArea ssaStateLastResidence;
    private WorkArea ssaCityLumpSumPayment;
    private WorkArea ssaStateLumpSumPaymnt;
    private Common dateOfDeathIndicator;
  }
#endregion
}
