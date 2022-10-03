// Program: OE_EAB_READ_FCR_MASTER_RECORD, ID: 374565567, model: 746.
// Short name: SWEXER15
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_EAB_READ_FCR_MASTER_RECORD.
/// </summary>
[Serializable]
public partial class OeEabReadFcrMasterRecord: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_EAB_READ_FCR_MASTER_RECORD program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeEabReadFcrMasterRecord(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeEabReadFcrMasterRecord.
  /// </summary>
  public OeEabReadFcrMasterRecord(IContext context, Import import, Export export)
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
      "SWEXER15", context, import, export, EabOptions.Hpvp);
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
    /// A value of ExternalFileStatus.
    /// </summary>
    [JsonPropertyName("externalFileStatus")]
    [Member(Index = 1, AccessFields = false, Members = new[]
    {
      "FileInstruction",
      "NumericReturnCode",
      "TextReturnCode",
      "TextLine130",
      "TextLine80",
      "TextLine8"
    })]
    public External ExternalFileStatus
    {
      get => externalFileStatus ??= new();
      set => externalFileStatus = value;
    }

    private External externalFileStatus;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ExternalFileStatus.
    /// </summary>
    [JsonPropertyName("externalFileStatus")]
    [Member(Index = 1, AccessFields = false, Members = new[]
    {
      "FileInstruction",
      "NumericReturnCode",
      "TextReturnCode",
      "TextLine130",
      "TextLine80",
      "TextLine8"
    })]
    public External ExternalFileStatus
    {
      get => externalFileStatus ??= new();
      set => externalFileStatus = value;
    }

    /// <summary>
    /// A value of FcrMasterCaseRecord.
    /// </summary>
    [JsonPropertyName("fcrMasterCaseRecord")]
    [Member(Index = 2, AccessFields = false, Members = new[]
    {
      "CaseId",
      "OrderIndicator",
      "ActionTypeCode",
      "BatchNumber",
      "FipsCountyCode",
      "PreviousCaseId",
      "CaseSentDateToFcr",
      "FcrCaseComments",
      "FcrCaseResponseDate",
      "AcknowlegementCode",
      "ErrorCode1",
      "ErrorCode2",
      "ErrorCode3",
      "ErrorCode4",
      "ErrorCode5",
      "CreatedBy",
      "CreatedTimestamp",
      "RecordIdentifier",
      "CaseUserField"
    })]
    public FcrMasterCaseRecord FcrMasterCaseRecord
    {
      get => fcrMasterCaseRecord ??= new();
      set => fcrMasterCaseRecord = value;
    }

    /// <summary>
    /// A value of FcrMasterMemberRecord.
    /// </summary>
    [JsonPropertyName("fcrMasterMemberRecord")]
    [Member(Index = 3, AccessFields = false, Members = new[]
    {
      "CaseId",
      "MemberId",
      "ActionTypeCode",
      "LocateRequestType",
      "RecordIdentifier",
      "ParticipantType",
      "SexCode",
      "DateOfBirth",
      "Ssn",
      "FirstName",
      "MiddleName",
      "LastName",
      "FipsCountyCode",
      "FamilyViolence",
      "PreviousSsn",
      "CityOfBirth",
      "StateOrCountryOfBirth",
      "FathersFirstName",
      "FathersMiddleInitial",
      "FathersLastName",
      "MothersFirstName",
      "MothersMiddleInitial",
      "MothersMaidenNm",
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
      "AdditionalSsn2ValidityCode",
      "BundleFplsLocateResults",
      "SsaCityOfLastResidence",
      "SsaStateOfLastResidence",
      "SsaCityOfLumpSumPayment",
      "SsaStateOfLumpSumPayment",
      "LastSentDtToFcr",
      "LastReceivedDtFromFcr",
      "MemberUserField"
    })]
    public FcrMasterMemberRecord FcrMasterMemberRecord
    {
      get => fcrMasterMemberRecord ??= new();
      set => fcrMasterMemberRecord = value;
    }

    private External externalFileStatus;
    private FcrMasterCaseRecord fcrMasterCaseRecord;
    private FcrMasterMemberRecord fcrMasterMemberRecord;
  }
#endregion
}
