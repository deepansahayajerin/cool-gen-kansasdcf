// The source file: IM_HOUSEHOLD_MEMBER, ID: 371435320, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: OBLGESTB
/// This entity defines the members in the IM Household during the period 
/// specified by START_DATE and END_DATE both days inclusive.
/// It keeps track of movements of persons in and out of an IM Household.
/// </summary>
[Serializable]
public partial class ImHouseholdMember: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ImHouseholdMember()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ImHouseholdMember(ImHouseholdMember that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ImHouseholdMember Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ImHouseholdMember that)
  {
    base.Assign(that);
    relationship = that.relationship;
    startDate = that.startDate;
    endDate = that.endDate;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    cseCaseNumber = that.cseCaseNumber;
    aeParticipationCode = that.aeParticipationCode;
    endCollectionDate = that.endCollectionDate;
    cspNumber = that.cspNumber;
    imhAeCaseNo = that.imhAeCaseNo;
  }

  /// <summary>Length of the RELATIONSHIP attribute.</summary>
  public const int Relationship_MaxLength = 2;

  /// <summary>
  /// The value of the RELATIONSHIP attribute.
  /// This attribute describes the relationship of the household member to the 
  /// Primary Information Person.
  /// EX:  PI - Primary Information Person (AR)
  ///      CH - Child
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = Relationship_MaxLength)]
  public string Relationship
  {
    get => relationship ?? "";
    set => relationship = TrimEnd(Substring(value, 1, Relationship_MaxLength));
  }

  /// <summary>
  /// The json value of the Relationship attribute.</summary>
  [JsonPropertyName("relationship")]
  [Computed]
  public string Relationship_Json
  {
    get => NullIf(Relationship, "");
    set => Relationship = value;
  }

  /// <summary>
  /// The value of the START_DATE attribute.
  /// This attribute defines the date on which the participation of the CSE 
  /// Person started on an ADC program in the IM Household.
  /// </summary>
  [JsonPropertyName("startDate")]
  [Member(Index = 2, Type = MemberType.Date)]
  public DateTime? StartDate
  {
    get => startDate;
    set => startDate = value;
  }

  /// <summary>
  /// The value of the END_DATE attribute.
  /// This attribute defines the date on which the participation of the CSE 
  /// person ended on an ADC program in the IM household.
  /// </summary>
  [JsonPropertyName("endDate")]
  [Member(Index = 3, Type = MemberType.Date)]
  public DateTime? EndDate
  {
    get => endDate;
    set => endDate = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// User ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  [Member(Index = 5, Type = MemberType.Timestamp)]
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
  [Member(Index = 6, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength)]
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
  [Member(Index = 7, Type = MemberType.Timestamp)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  /// <summary>Length of the CSE_CASE_NUMBER attribute.</summary>
  public const int CseCaseNumber_MaxLength = 10;

  /// <summary>
  /// The value of the CSE_CASE_NUMBER attribute.
  /// The CSE case number which is the identifier for the Case entity type. It 
  /// will identify the CSE Case to which the person on the specific AE Case
  /// belongs.
  /// </summary>
  [JsonPropertyName("cseCaseNumber")]
  [Member(Index = 8, Type = MemberType.Char, Length = CseCaseNumber_MaxLength, Optional
    = true)]
  public string CseCaseNumber
  {
    get => cseCaseNumber;
    set => cseCaseNumber = value != null
      ? TrimEnd(Substring(value, 1, CseCaseNumber_MaxLength)) : null;
  }

  /// <summary>Length of the AE_PARTICIPATION_CODE attribute.</summary>
  public const int AeParticipationCode_MaxLength = 2;

  /// <summary>
  /// The value of the AE_PARTICIPATION_CODE attribute.
  /// The setup participation code (SEPA code) of the IM Household Member as 
  /// found on the AE system.
  /// </summary>
  [JsonPropertyName("aeParticipationCode")]
  [Member(Index = 9, Type = MemberType.Char, Length
    = AeParticipationCode_MaxLength, Optional = true)]
  public string AeParticipationCode
  {
    get => aeParticipationCode;
    set => aeParticipationCode = value != null
      ? TrimEnd(Substring(value, 1, AeParticipationCode_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the END_COLLECTION_DATE attribute.
  /// The end_collection_date is used to identify the end of the period this 
  /// member may be have collections processed for this thousehold. This
  /// attribute is populated during the adding of an Im Household Member. When
  /// adding a member to a household, the adding program will need to determine
  /// if that CSE Person has another record that has a &quot;max&quot;
  /// end_collection_date. That rows end_collection_date will be set to the last
  /// date of the prior month. The new row will have a &quot;max&quot;
  /// end_collection_date. There will always be one row in the household for
  /// each person that will contain a max end_collection_date. This attribute
  /// will allow for better performance of the excess URA processing.
  /// </summary>
  [JsonPropertyName("endCollectionDate")]
  [Member(Index = 10, Type = MemberType.Date)]
  public DateTime? EndCollectionDate
  {
    get => endCollectionDate;
    set => endCollectionDate = value;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length = CspNumber_MaxLength)]
  public string CspNumber
  {
    get => cspNumber ?? "";
    set => cspNumber = TrimEnd(Substring(value, 1, CspNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the CspNumber attribute.</summary>
  [JsonPropertyName("cspNumber")]
  [Computed]
  public string CspNumber_Json
  {
    get => NullIf(CspNumber, "");
    set => CspNumber = value;
  }

  /// <summary>Length of the AE_CASE_NO attribute.</summary>
  public const int ImhAeCaseNo_MaxLength = 8;

  /// <summary>
  /// The value of the AE_CASE_NO attribute.
  /// This attribute defines the AE Case No for the IM Household.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length = ImhAeCaseNo_MaxLength)]
  public string ImhAeCaseNo
  {
    get => imhAeCaseNo ?? "";
    set => imhAeCaseNo = TrimEnd(Substring(value, 1, ImhAeCaseNo_MaxLength));
  }

  /// <summary>
  /// The json value of the ImhAeCaseNo attribute.</summary>
  [JsonPropertyName("imhAeCaseNo")]
  [Computed]
  public string ImhAeCaseNo_Json
  {
    get => NullIf(ImhAeCaseNo, "");
    set => ImhAeCaseNo = value;
  }

  private string relationship;
  private DateTime? startDate;
  private DateTime? endDate;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private string cseCaseNumber;
  private string aeParticipationCode;
  private DateTime? endCollectionDate;
  private string cspNumber;
  private string imhAeCaseNo;
}
