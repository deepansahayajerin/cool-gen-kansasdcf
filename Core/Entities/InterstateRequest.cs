// The source file: INTERSTATE_REQUEST, ID: 371422182, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVINIT
/// An Incoming or Outgoing request to or from another state to take specific 
/// actions regarding an Absent Parent presumed to be in the state.
/// </summary>
[Serializable]
public partial class InterstateRequest: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public InterstateRequest()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public InterstateRequest(InterstateRequest that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new InterstateRequest Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(InterstateRequest that)
  {
    base.Assign(that);
    intHGeneratedId = that.intHGeneratedId;
    otherStateFips = that.otherStateFips;
    otherStateCaseId = that.otherStateCaseId;
    otherStateCaseStatus = that.otherStateCaseStatus;
    caseType = that.caseType;
    ksCaseInd = that.ksCaseInd;
    otherStateCaseClosureReason = that.otherStateCaseClosureReason;
    otherStateCaseClosureDate = that.otherStateCaseClosureDate;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    country = that.country;
    tribalAgency = that.tribalAgency;
    casINumber = that.casINumber;
    croId = that.croId;
    croType = that.croType;
    cspNumber = that.cspNumber;
    casNumber = that.casNumber;
  }

  /// <summary>
  /// The value of the INT_H_GENERATED_ID attribute.
  /// Attribute to uniquely identify an interstate referral associated within a 
  /// case.
  /// This will be a system-generated number.
  /// </summary>
  [JsonPropertyName("intHGeneratedId")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 8)]
  public int IntHGeneratedId
  {
    get => intHGeneratedId;
    set => intHGeneratedId = value;
  }

  /// <summary>
  /// The value of the OTHER_STATE_FIPS attribute.
  /// The code for the other state involved in this interstate referral.
  /// </summary>
  [JsonPropertyName("otherStateFips")]
  [DefaultValue(0)]
  [Member(Index = 2, Type = MemberType.Number, Length = 2)]
  public int OtherStateFips
  {
    get => otherStateFips;
    set => otherStateFips = value;
  }

  /// <summary>Length of the OTHER_STATE_CASE_ID attribute.</summary>
  public const int OtherStateCaseId_MaxLength = 15;

  /// <summary>
  /// The value of the OTHER_STATE_CASE_ID attribute.
  /// The Case Id in the state that originated this CSENet Referral.
  /// </summary>
  [JsonPropertyName("otherStateCaseId")]
  [Member(Index = 3, Type = MemberType.Char, Length
    = OtherStateCaseId_MaxLength, Optional = true)]
  public string OtherStateCaseId
  {
    get => otherStateCaseId;
    set => otherStateCaseId = value != null
      ? TrimEnd(Substring(value, 1, OtherStateCaseId_MaxLength)) : null;
  }

  /// <summary>Length of the OTHER_STATE_CASE_STATUS attribute.</summary>
  public const int OtherStateCaseStatus_MaxLength = 1;

  /// <summary>
  /// The value of the OTHER_STATE_CASE_STATUS attribute.
  /// CSENet Values:
  /// O - Open
  /// C - Closed
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length
    = OtherStateCaseStatus_MaxLength)]
  public string OtherStateCaseStatus
  {
    get => otherStateCaseStatus ?? "";
    set => otherStateCaseStatus =
      TrimEnd(Substring(value, 1, OtherStateCaseStatus_MaxLength));
  }

  /// <summary>
  /// The json value of the OtherStateCaseStatus attribute.</summary>
  [JsonPropertyName("otherStateCaseStatus")]
  [Computed]
  public string OtherStateCaseStatus_Json
  {
    get => NullIf(OtherStateCaseStatus, "");
    set => OtherStateCaseStatus = value;
  }

  /// <summary>Length of the CASE_TYPE attribute.</summary>
  public const int CaseType_MaxLength = 3;

  /// <summary>
  /// The value of the CASE_TYPE attribute.
  /// CSENet Values:
  /// AD-AFDC
  /// NA-Non-AFDC
  /// AF-Foster Care
  /// MA-Medical Need Only
  /// </summary>
  [JsonPropertyName("caseType")]
  [Member(Index = 5, Type = MemberType.Char, Length = CaseType_MaxLength, Optional
    = true)]
  public string CaseType
  {
    get => caseType;
    set => caseType = value != null
      ? TrimEnd(Substring(value, 1, CaseType_MaxLength)) : null;
  }

  /// <summary>Length of the KS_CASE_IND attribute.</summary>
  public const int KsCaseInd_MaxLength = 1;

  /// <summary>
  /// The value of the KS_CASE_IND attribute.
  /// This indicates whether the case in an Kansas Case or an Out Of State Case.
  /// Values - Y
  ///          N.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = KsCaseInd_MaxLength)]
  public string KsCaseInd
  {
    get => ksCaseInd ?? "";
    set => ksCaseInd = TrimEnd(Substring(value, 1, KsCaseInd_MaxLength));
  }

  /// <summary>
  /// The json value of the KsCaseInd attribute.</summary>
  [JsonPropertyName("ksCaseInd")]
  [Computed]
  public string KsCaseInd_Json
  {
    get => NullIf(KsCaseInd, "");
    set => KsCaseInd = value;
  }

  /// <summary>Length of the OTHER_STATE_CASE_CLOSURE_REASON attribute.
  /// </summary>
  public const int OtherStateCaseClosureReason_MaxLength = 2;

  /// <summary>
  /// The value of the OTHER_STATE_CASE_CLOSURE_REASON attribute.
  /// This is the reason the other state case was closed. Should have a Other 
  /// state case status of &quot;C&quot;.
  /// </summary>
  [JsonPropertyName("otherStateCaseClosureReason")]
  [Member(Index = 7, Type = MemberType.Char, Length
    = OtherStateCaseClosureReason_MaxLength, Optional = true)]
  public string OtherStateCaseClosureReason
  {
    get => otherStateCaseClosureReason;
    set => otherStateCaseClosureReason = value != null
      ? TrimEnd(Substring(value, 1, OtherStateCaseClosureReason_MaxLength)) : null
      ;
  }

  /// <summary>
  /// The value of the OTHER_STATE_CASE_CLOSURE_DATE attribute.
  /// the date the other state case was closed.
  /// </summary>
  [JsonPropertyName("otherStateCaseClosureDate")]
  [Member(Index = 8, Type = MemberType.Date, Optional = true)]
  public DateTime? OtherStateCaseClosureDate
  {
    get => otherStateCaseClosureDate;
    set => otherStateCaseClosureDate = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// User ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
  public string CreatedBy
  {
    get => createdBy ?? "";
    set => createdBy = TrimEnd(Substring(value, 1, CreatedBy_MaxLength));
  }

  /// <summary>
  /// The json value of the CreatedBy attribute.</summary>
  [JsonPropertyName("createdBy")]
  [Computed]
  public string CreatedBy_Json
  {
    get => NullIf(CreatedBy, "");
    set => CreatedBy = value;
  }

  /// <summary>
  /// The value of the CREATED_TIMESTAMP attribute.
  /// Timestamp of creation of the occurrence.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 10, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// User ID or Program ID responsible for the last update of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy ?? "";
    set => lastUpdatedBy =
      TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength));
  }

  /// <summary>
  /// The json value of the LastUpdatedBy attribute.</summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Computed]
  public string LastUpdatedBy_Json
  {
    get => NullIf(LastUpdatedBy, "");
    set => LastUpdatedBy = value;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TIMESTAMP attribute.
  /// Timestamp of last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTimestamp")]
  [Member(Index = 12, Type = MemberType.Timestamp)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  /// <summary>Length of the COUNTRY attribute.</summary>
  public const int Country_MaxLength = 2;

  /// <summary>
  /// The value of the COUNTRY attribute.
  /// Code indicating the country in which the interstate referral is located.
  /// </summary>
  [JsonPropertyName("country")]
  [Member(Index = 13, Type = MemberType.Char, Length = Country_MaxLength, Optional
    = true)]
  public string Country
  {
    get => country;
    set => country = value != null
      ? TrimEnd(Substring(value, 1, Country_MaxLength)) : null;
  }

  /// <summary>Length of the TRIBAL_AGENCY attribute.</summary>
  public const int TribalAgency_MaxLength = 4;

  /// <summary>
  /// The value of the TRIBAL_AGENCY attribute.
  /// The code of a tribal IV-D agancy which initiates an incoming interstate 
  /// request or responds to an outgoing interstate request.
  /// </summary>
  [JsonPropertyName("tribalAgency")]
  [Member(Index = 14, Type = MemberType.Char, Length = TribalAgency_MaxLength, Optional
    = true)]
  public string TribalAgency
  {
    get => tribalAgency;
    set => tribalAgency = value != null
      ? TrimEnd(Substring(value, 1, TribalAgency_MaxLength)) : null;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CasINumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// A unique number assigned to a request or referral.  Becomes the Case 
  /// number.
  /// </summary>
  [JsonPropertyName("casINumber")]
  [Member(Index = 15, Type = MemberType.Char, Length = CasINumber_MaxLength, Optional
    = true)]
  public string CasINumber
  {
    get => casINumber;
    set => casINumber = value != null
      ? TrimEnd(Substring(value, 1, CasINumber_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// The unique identifier.  System generated using a 3-digit random generator
  /// </summary>
  [JsonPropertyName("croId")]
  [Member(Index = 16, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? CroId
  {
    get => croId;
    set => croId = value;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int CroType_MaxLength = 2;

  /// <summary>
  /// The value of the TYPE attribute.
  /// The type of role played by a CSE Person in a given case.
  /// </summary>
  [JsonPropertyName("croType")]
  [Member(Index = 17, Type = MemberType.Char, Length = CroType_MaxLength, Optional
    = true)]
  [Value(null)]
  [Value("MO")]
  [Value("FA")]
  [Value("CH")]
  [Value("AR")]
  [Value("AP")]
  public string CroType
  {
    get => croType;
    set => croType = value != null
      ? TrimEnd(Substring(value, 1, CroType_MaxLength)) : null;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonPropertyName("cspNumber")]
  [Member(Index = 18, Type = MemberType.Char, Length = CspNumber_MaxLength, Optional
    = true)]
  public string CspNumber
  {
    get => cspNumber;
    set => cspNumber = value != null
      ? TrimEnd(Substring(value, 1, CspNumber_MaxLength)) : null;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CasNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// A unique number assigned to a request or referral.  Becomes the Case 
  /// number.
  /// </summary>
  [JsonPropertyName("casNumber")]
  [Member(Index = 19, Type = MemberType.Char, Length = CasNumber_MaxLength, Optional
    = true)]
  public string CasNumber
  {
    get => casNumber;
    set => casNumber = value != null
      ? TrimEnd(Substring(value, 1, CasNumber_MaxLength)) : null;
  }

  private int intHGeneratedId;
  private int otherStateFips;
  private string otherStateCaseId;
  private string otherStateCaseStatus;
  private string caseType;
  private string ksCaseInd;
  private string otherStateCaseClosureReason;
  private DateTime? otherStateCaseClosureDate;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private string country;
  private string tribalAgency;
  private string casINumber;
  private int? croId;
  private string croType;
  private string cspNumber;
  private string casNumber;
}
