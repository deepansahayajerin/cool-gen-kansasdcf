// Program: OE_EAB_WRITE_FCR_REQUESTS, ID: 373528647, model: 746.
// Short name: SWEXEW01
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_EAB_WRITE_FCR_REQUESTS.
/// </summary>
[Serializable]
public partial class OeEabWriteFcrRequests: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_EAB_WRITE_FCR_REQUESTS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeEabWriteFcrRequests(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeEabWriteFcrRequests.
  /// </summary>
  public OeEabWriteFcrRequests(IContext context, Import import, Export export):
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
      "SWEXEW01", context, import, export, EabOptions.Hpvp);
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
    /// A value of ExternalFplsRequest.
    /// </summary>
    [JsonPropertyName("externalFplsRequest")]
    [Member(Index = 1, AccessFields = false, Members = new[]
    {
      "StateAbbr",
      "StationNumber",
      "TransactionType",
      "Ssn",
      "ApCsePersonNumber",
      "LocalCode",
      "UsersField",
      "ApFirstName",
      "ApMiddleName",
      "ApFirstLastName",
      "ApDateOfBirth",
      "Sex",
      "CollectAllResponsesTogether",
      "SendRequestTo",
      "TransactionError",
      "ApCityOfBirth",
      "ApStateOrCountryOfBirth",
      "ApsFathersFirstName",
      "ApsFathersMi",
      "ApsFathersLastName",
      "ApsMothersFirstName",
      "ApsMothersMi",
      "ApsMothersMaidenName",
      "CpSsn"
    })]
    public ExternalFplsRequest ExternalFplsRequest
    {
      get => externalFplsRequest ??= new();
      set => externalFplsRequest = value;
    }

    /// <summary>
    /// A value of RecordIdentifier.
    /// </summary>
    [JsonPropertyName("recordIdentifier")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "Text3" })]
    public WorkArea RecordIdentifier
    {
      get => recordIdentifier ??= new();
      set => recordIdentifier = value;
    }

    /// <summary>
    /// A value of ActionTypeCode.
    /// </summary>
    [JsonPropertyName("actionTypeCode")]
    [Member(Index = 3, AccessFields = false, Members = new[] { "Text1" })]
    public TextWorkArea ActionTypeCode
    {
      get => actionTypeCode ??= new();
      set => actionTypeCode = value;
    }

    /// <summary>
    /// A value of CaseId.
    /// </summary>
    [JsonPropertyName("caseId")]
    [Member(Index = 4, AccessFields = false, Members = new[] { "Text10" })]
    public WorkArea CaseId
    {
      get => caseId ??= new();
      set => caseId = value;
    }

    /// <summary>
    /// A value of CaseType.
    /// </summary>
    [JsonPropertyName("caseType")]
    [Member(Index = 5, AccessFields = false, Members = new[] { "Text1" })]
    public TextWorkArea CaseType
    {
      get => caseType ??= new();
      set => caseType = value;
    }

    /// <summary>
    /// A value of OrderIndicator.
    /// </summary>
    [JsonPropertyName("orderIndicator")]
    [Member(Index = 6, AccessFields = false, Members = new[] { "Text1" })]
    public TextWorkArea OrderIndicator
    {
      get => orderIndicator ??= new();
      set => orderIndicator = value;
    }

    /// <summary>
    /// A value of ParticipantType.
    /// </summary>
    [JsonPropertyName("participantType")]
    [Member(Index = 7, AccessFields = false, Members = new[] { "Text3" })]
    public WorkArea ParticipantType
    {
      get => participantType ??= new();
      set => participantType = value;
    }

    /// <summary>
    /// A value of FamilyViolence.
    /// </summary>
    [JsonPropertyName("familyViolence")]
    [Member(Index = 8, AccessFields = false, Members = new[] { "Text3" })]
    public WorkArea FamilyViolence
    {
      get => familyViolence ??= new();
      set => familyViolence = value;
    }

    /// <summary>
    /// A value of AdditionalSsn2.
    /// </summary>
    [JsonPropertyName("additionalSsn2")]
    [Member(Index = 9, AccessFields = false, Members = new[] { "Ssn" })]
    public ExternalFplsRequest AdditionalSsn2
    {
      get => additionalSsn2 ??= new();
      set => additionalSsn2 = value;
    }

    /// <summary>
    /// A value of AdditionalSsn1.
    /// </summary>
    [JsonPropertyName("additionalSsn1")]
    [Member(Index = 10, AccessFields = false, Members = new[] { "Ssn" })]
    public ExternalFplsRequest AdditionalSsn1
    {
      get => additionalSsn1 ??= new();
      set => additionalSsn1 = value;
    }

    /// <summary>
    /// A value of AdditionalFirstName1.
    /// </summary>
    [JsonPropertyName("additionalFirstName1")]
    [Member(Index = 11, AccessFields = false, Members = new[] { "Text16" })]
    public WorkArea AdditionalFirstName1
    {
      get => additionalFirstName1 ??= new();
      set => additionalFirstName1 = value;
    }

    /// <summary>
    /// A value of AdditionalMiddleName1.
    /// </summary>
    [JsonPropertyName("additionalMiddleName1")]
    [Member(Index = 12, AccessFields = false, Members = new[] { "Text16" })]
    public WorkArea AdditionalMiddleName1
    {
      get => additionalMiddleName1 ??= new();
      set => additionalMiddleName1 = value;
    }

    /// <summary>
    /// A value of AdditionalLastName1.
    /// </summary>
    [JsonPropertyName("additionalLastName1")]
    [Member(Index = 13, AccessFields = false, Members = new[] { "Text32" })]
    public WorkArea AdditionalLastName1
    {
      get => additionalLastName1 ??= new();
      set => additionalLastName1 = value;
    }

    /// <summary>
    /// A value of AdditionalFirstName2.
    /// </summary>
    [JsonPropertyName("additionalFirstName2")]
    [Member(Index = 14, AccessFields = false, Members = new[] { "Text16" })]
    public WorkArea AdditionalFirstName2
    {
      get => additionalFirstName2 ??= new();
      set => additionalFirstName2 = value;
    }

    /// <summary>
    /// A value of AdditionalMiddleName2.
    /// </summary>
    [JsonPropertyName("additionalMiddleName2")]
    [Member(Index = 15, AccessFields = false, Members = new[] { "Text16" })]
    public WorkArea AdditionalMiddleName2
    {
      get => additionalMiddleName2 ??= new();
      set => additionalMiddleName2 = value;
    }

    /// <summary>
    /// A value of AdditionalLastName2.
    /// </summary>
    [JsonPropertyName("additionalLastName2")]
    [Member(Index = 16, AccessFields = false, Members = new[] { "Text32" })]
    public WorkArea AdditionalLastName2
    {
      get => additionalLastName2 ??= new();
      set => additionalLastName2 = value;
    }

    /// <summary>
    /// A value of AdditionalFirstName3.
    /// </summary>
    [JsonPropertyName("additionalFirstName3")]
    [Member(Index = 17, AccessFields = false, Members = new[] { "Text16" })]
    public WorkArea AdditionalFirstName3
    {
      get => additionalFirstName3 ??= new();
      set => additionalFirstName3 = value;
    }

    /// <summary>
    /// A value of AdditionalMiddleName3.
    /// </summary>
    [JsonPropertyName("additionalMiddleName3")]
    [Member(Index = 18, AccessFields = false, Members = new[] { "Text16" })]
    public WorkArea AdditionalMiddleName3
    {
      get => additionalMiddleName3 ??= new();
      set => additionalMiddleName3 = value;
    }

    /// <summary>
    /// A value of AdditionalLastName3.
    /// </summary>
    [JsonPropertyName("additionalLastName3")]
    [Member(Index = 19, AccessFields = false, Members = new[] { "Text32" })]
    public WorkArea AdditionalLastName3
    {
      get => additionalLastName3 ??= new();
      set => additionalLastName3 = value;
    }

    /// <summary>
    /// A value of AdditionalFirstName4.
    /// </summary>
    [JsonPropertyName("additionalFirstName4")]
    [Member(Index = 20, AccessFields = false, Members = new[] { "Text16" })]
    public WorkArea AdditionalFirstName4
    {
      get => additionalFirstName4 ??= new();
      set => additionalFirstName4 = value;
    }

    /// <summary>
    /// A value of AdditionalMiddleName4.
    /// </summary>
    [JsonPropertyName("additionalMiddleName4")]
    [Member(Index = 21, AccessFields = false, Members = new[] { "Text16" })]
    public WorkArea AdditionalMiddleName4
    {
      get => additionalMiddleName4 ??= new();
      set => additionalMiddleName4 = value;
    }

    /// <summary>
    /// A value of AddtionalLastName4.
    /// </summary>
    [JsonPropertyName("addtionalLastName4")]
    [Member(Index = 22, AccessFields = false, Members = new[] { "Text32" })]
    public WorkArea AddtionalLastName4
    {
      get => addtionalLastName4 ??= new();
      set => addtionalLastName4 = value;
    }

    /// <summary>
    /// A value of NewMemberId.
    /// </summary>
    [JsonPropertyName("newMemberId")]
    [Member(Index = 23, AccessFields = false, Members
      = new[] { "ApCsePersonNumber" })]
    public ExternalFplsRequest NewMemberId
    {
      get => newMemberId ??= new();
      set => newMemberId = value;
    }

    /// <summary>
    /// A value of Irs1099.
    /// </summary>
    [JsonPropertyName("irs1099")]
    [Member(Index = 24, AccessFields = false, Members = new[] { "Flag" })]
    public Common Irs1099
    {
      get => irs1099 ??= new();
      set => irs1099 = value;
    }

    /// <summary>
    /// A value of LocateSource1.
    /// </summary>
    [JsonPropertyName("locateSource1")]
    [Member(Index = 25, AccessFields = false, Members = new[] { "Text3" })]
    public WorkArea LocateSource1
    {
      get => locateSource1 ??= new();
      set => locateSource1 = value;
    }

    /// <summary>
    /// A value of LocateSource2.
    /// </summary>
    [JsonPropertyName("locateSource2")]
    [Member(Index = 26, AccessFields = false, Members = new[] { "Text3" })]
    public WorkArea LocateSource2
    {
      get => locateSource2 ??= new();
      set => locateSource2 = value;
    }

    /// <summary>
    /// A value of LocateSource3.
    /// </summary>
    [JsonPropertyName("locateSource3")]
    [Member(Index = 27, AccessFields = false, Members = new[] { "Text3" })]
    public WorkArea LocateSource3
    {
      get => locateSource3 ??= new();
      set => locateSource3 = value;
    }

    /// <summary>
    /// A value of LocateSource4.
    /// </summary>
    [JsonPropertyName("locateSource4")]
    [Member(Index = 28, AccessFields = false, Members = new[] { "Text3" })]
    public WorkArea LocateSource4
    {
      get => locateSource4 ??= new();
      set => locateSource4 = value;
    }

    /// <summary>
    /// A value of LocateSource5.
    /// </summary>
    [JsonPropertyName("locateSource5")]
    [Member(Index = 29, AccessFields = false, Members = new[] { "Text3" })]
    public WorkArea LocateSource5
    {
      get => locateSource5 ??= new();
      set => locateSource5 = value;
    }

    /// <summary>
    /// A value of LocateSource6.
    /// </summary>
    [JsonPropertyName("locateSource6")]
    [Member(Index = 30, AccessFields = false, Members = new[] { "Text3" })]
    public WorkArea LocateSource6
    {
      get => locateSource6 ??= new();
      set => locateSource6 = value;
    }

    /// <summary>
    /// A value of LocateSource7.
    /// </summary>
    [JsonPropertyName("locateSource7")]
    [Member(Index = 31, AccessFields = false, Members = new[] { "Text3" })]
    public WorkArea LocateSource7
    {
      get => locateSource7 ??= new();
      set => locateSource7 = value;
    }

    /// <summary>
    /// A value of LocateSource8.
    /// </summary>
    [JsonPropertyName("locateSource8")]
    [Member(Index = 32, AccessFields = false, Members = new[] { "Text3" })]
    public WorkArea LocateSource8
    {
      get => locateSource8 ??= new();
      set => locateSource8 = value;
    }

    /// <summary>
    /// A value of ProcessDate.
    /// </summary>
    [JsonPropertyName("processDate")]
    [Member(Index = 33, AccessFields = false, Members = new[] { "Date" })]
    public DateWorkArea ProcessDate
    {
      get => processDate ??= new();
      set => processDate = value;
    }

    /// <summary>
    /// A value of RecordCounts.
    /// </summary>
    [JsonPropertyName("recordCounts")]
    [Member(Index = 34, AccessFields = false, Members = new[] { "Count" })]
    public Common RecordCounts
    {
      get => recordCounts ??= new();
      set => recordCounts = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    [Member(Index = 35, AccessFields = false, Members
      = new[] { "Action", "Status" })]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private ExternalFplsRequest externalFplsRequest;
    private WorkArea recordIdentifier;
    private TextWorkArea actionTypeCode;
    private WorkArea caseId;
    private TextWorkArea caseType;
    private TextWorkArea orderIndicator;
    private WorkArea participantType;
    private WorkArea familyViolence;
    private ExternalFplsRequest additionalSsn2;
    private ExternalFplsRequest additionalSsn1;
    private WorkArea additionalFirstName1;
    private WorkArea additionalMiddleName1;
    private WorkArea additionalLastName1;
    private WorkArea additionalFirstName2;
    private WorkArea additionalMiddleName2;
    private WorkArea additionalLastName2;
    private WorkArea additionalFirstName3;
    private WorkArea additionalMiddleName3;
    private WorkArea additionalLastName3;
    private WorkArea additionalFirstName4;
    private WorkArea additionalMiddleName4;
    private WorkArea addtionalLastName4;
    private ExternalFplsRequest newMemberId;
    private Common irs1099;
    private WorkArea locateSource1;
    private WorkArea locateSource2;
    private WorkArea locateSource3;
    private WorkArea locateSource4;
    private WorkArea locateSource5;
    private WorkArea locateSource6;
    private WorkArea locateSource7;
    private WorkArea locateSource8;
    private DateWorkArea processDate;
    private Common recordCounts;
    private EabFileHandling eabFileHandling;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "Status" })]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private EabFileHandling eabFileHandling;
  }
#endregion
}
