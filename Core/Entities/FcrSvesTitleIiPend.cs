// The source file: FCR_SVES_TITLE_II_PEND, ID: 945065672, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// This Entity Type stores SVES Title II Pending Claims (E04) response 
/// information from FCR.
/// </summary>
[Serializable]
public partial class FcrSvesTitleIiPend: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public FcrSvesTitleIiPend()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public FcrSvesTitleIiPend(FcrSvesTitleIiPend that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new FcrSvesTitleIiPend Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(FcrSvesTitleIiPend that)
  {
    base.Assign(that);
    seqNo = that.seqNo;
    nameMatchedCode = that.nameMatchedCode;
    firstNameText = that.firstNameText;
    middleNameText = that.middleNameText;
    lastNameText = that.lastNameText;
    additionalFirstName1Text = that.additionalFirstName1Text;
    additionalMiddleName1Text = that.additionalMiddleName1Text;
    additionalLastName1Text = that.additionalLastName1Text;
    additionalFirstName2Text = that.additionalFirstName2Text;
    additionalMiddleName2Text = that.additionalMiddleName2Text;
    additionalLastName2Text = that.additionalLastName2Text;
    responseDate = that.responseDate;
    otherSsn = that.otherSsn;
    ssnMatchCode = that.ssnMatchCode;
    claimTypeCode = that.claimTypeCode;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    fcgLSRspAgy = that.fcgLSRspAgy;
    fcgMemberId = that.fcgMemberId;
  }

  /// <summary>
  /// The value of the SEQ_NO attribute.
  /// This field contains a value to identify unique Title II Pending record.
  /// </summary>
  [JsonPropertyName("seqNo")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 2)]
  public int SeqNo
  {
    get => seqNo;
    set => seqNo = value;
  }

  /// <summary>Length of the NAME_MATCHED_CODE attribute.</summary>
  public const int NameMatchedCode_MaxLength = 1;

  /// <summary>
  /// The value of the NAME_MATCHED_CODE attribute.
  /// This field contains a value to indicate which name matched the name on the
  /// Title II Pending Claim record:
  /// 1  First letter of First Name, first four letters of Last Name
  /// 2  First letter of Additional First Name 1, first four letters of 
  /// Additional Last Name 1
  /// 3  First letter of Additional First Name 2, first four letters of 
  /// Additional Last Name 2
  /// If the Name or Additional Names do not match a Title II Pending Claim 
  /// Returned Name, this
  /// field contains a space.
  /// </summary>
  [JsonPropertyName("nameMatchedCode")]
  [Member(Index = 2, Type = MemberType.Char, Length
    = NameMatchedCode_MaxLength, Optional = true)]
  public string NameMatchedCode
  {
    get => nameMatchedCode;
    set => nameMatchedCode = value != null
      ? TrimEnd(Substring(value, 1, NameMatchedCode_MaxLength)) : null;
  }

  /// <summary>Length of the FIRST_NAME_TEXT attribute.</summary>
  public const int FirstNameText_MaxLength = 16;

  /// <summary>
  /// The value of the FIRST_NAME_TEXT attribute.
  /// This field contains the First Name that is stored on the FCR Database.
  /// </summary>
  [JsonPropertyName("firstNameText")]
  [Member(Index = 3, Type = MemberType.Varchar, Length
    = FirstNameText_MaxLength, Optional = true)]
  public string FirstNameText
  {
    get => firstNameText;
    set => firstNameText = value != null
      ? Substring(value, 1, FirstNameText_MaxLength) : null;
  }

  /// <summary>Length of the MIDDLE_NAME_TEXT attribute.</summary>
  public const int MiddleNameText_MaxLength = 16;

  /// <summary>
  /// The value of the MIDDLE_NAME_TEXT attribute.
  /// This field contains the Middle Name that is stored on the FCR Database.
  /// </summary>
  [JsonPropertyName("middleNameText")]
  [Member(Index = 4, Type = MemberType.Varchar, Length
    = MiddleNameText_MaxLength, Optional = true)]
  public string MiddleNameText
  {
    get => middleNameText;
    set => middleNameText = value != null
      ? Substring(value, 1, MiddleNameText_MaxLength) : null;
  }

  /// <summary>Length of the LAST_NAME_TEXT attribute.</summary>
  public const int LastNameText_MaxLength = 30;

  /// <summary>
  /// The value of the LAST_NAME_TEXT attribute.
  /// This field contains the Last Name that is stored on the FCR Database.
  /// </summary>
  [JsonPropertyName("lastNameText")]
  [Member(Index = 5, Type = MemberType.Varchar, Length
    = LastNameText_MaxLength, Optional = true)]
  public string LastNameText
  {
    get => lastNameText;
    set => lastNameText = value != null
      ? Substring(value, 1, LastNameText_MaxLength) : null;
  }

  /// <summary>Length of the ADDITIONAL_FIRST_NAME_1_TEXT attribute.</summary>
  public const int AdditionalFirstName1Text_MaxLength = 16;

  /// <summary>
  /// The value of the ADDITIONAL_FIRST_NAME_1_TEXT attribute.
  /// If an Additional First Name 1 on the FCR Database was used in the match, 
  /// this field contains the name that was used.
  /// </summary>
  [JsonPropertyName("additionalFirstName1Text")]
  [Member(Index = 6, Type = MemberType.Varchar, Length
    = AdditionalFirstName1Text_MaxLength, Optional = true)]
  public string AdditionalFirstName1Text
  {
    get => additionalFirstName1Text;
    set => additionalFirstName1Text = value != null
      ? Substring(value, 1, AdditionalFirstName1Text_MaxLength) : null;
  }

  /// <summary>Length of the ADDITIONAL_MIDDLE_NAME_1_TEXT attribute.</summary>
  public const int AdditionalMiddleName1Text_MaxLength = 16;

  /// <summary>
  /// The value of the ADDITIONAL_MIDDLE_NAME_1_TEXT attribute.
  /// This field contains the Additional Middle Name 1 that is stored on the FCR
  /// Database.
  /// </summary>
  [JsonPropertyName("additionalMiddleName1Text")]
  [Member(Index = 7, Type = MemberType.Varchar, Length
    = AdditionalMiddleName1Text_MaxLength, Optional = true)]
  public string AdditionalMiddleName1Text
  {
    get => additionalMiddleName1Text;
    set => additionalMiddleName1Text = value != null
      ? Substring(value, 1, AdditionalMiddleName1Text_MaxLength) : null;
  }

  /// <summary>Length of the ADDITIONAL_LAST_NAME_1_TEXT attribute.</summary>
  public const int AdditionalLastName1Text_MaxLength = 30;

  /// <summary>
  /// The value of the ADDITIONAL_LAST_NAME_1_TEXT attribute.
  /// If an Additional Last Name 1 on the FCR Database was used in the match, 
  /// this field contains the name that was used.
  /// </summary>
  [JsonPropertyName("additionalLastName1Text")]
  [Member(Index = 8, Type = MemberType.Varchar, Length
    = AdditionalLastName1Text_MaxLength, Optional = true)]
  public string AdditionalLastName1Text
  {
    get => additionalLastName1Text;
    set => additionalLastName1Text = value != null
      ? Substring(value, 1, AdditionalLastName1Text_MaxLength) : null;
  }

  /// <summary>Length of the ADDITIONAL_FIRST_NAME_2_TEXT attribute.</summary>
  public const int AdditionalFirstName2Text_MaxLength = 16;

  /// <summary>
  /// The value of the ADDITIONAL_FIRST_NAME_2_TEXT attribute.
  /// If an Additional First Name 2 on the FCR Database was used in the match, 
  /// this field contains the name that was used.
  /// </summary>
  [JsonPropertyName("additionalFirstName2Text")]
  [Member(Index = 9, Type = MemberType.Varchar, Length
    = AdditionalFirstName2Text_MaxLength, Optional = true)]
  public string AdditionalFirstName2Text
  {
    get => additionalFirstName2Text;
    set => additionalFirstName2Text = value != null
      ? Substring(value, 1, AdditionalFirstName2Text_MaxLength) : null;
  }

  /// <summary>Length of the ADDITIONAL_MIDDLE_NAME_2_TEXT attribute.</summary>
  public const int AdditionalMiddleName2Text_MaxLength = 16;

  /// <summary>
  /// The value of the ADDITIONAL_MIDDLE_NAME_2_TEXT attribute.
  /// This field contains the Additional Middle Name 2 that is stored on the FCR
  /// Database.
  /// </summary>
  [JsonPropertyName("additionalMiddleName2Text")]
  [Member(Index = 10, Type = MemberType.Varchar, Length
    = AdditionalMiddleName2Text_MaxLength, Optional = true)]
  public string AdditionalMiddleName2Text
  {
    get => additionalMiddleName2Text;
    set => additionalMiddleName2Text = value != null
      ? Substring(value, 1, AdditionalMiddleName2Text_MaxLength) : null;
  }

  /// <summary>Length of the ADDITIONAL_LAST_NAME_2_TEXT attribute.</summary>
  public const int AdditionalLastName2Text_MaxLength = 30;

  /// <summary>
  /// The value of the ADDITIONAL_LAST_NAME_2_TEXT attribute.
  /// If an Additional Last Name 2 on the FCR Database was used in the match, 
  /// this field contains the name that was used.
  /// </summary>
  [JsonPropertyName("additionalLastName2Text")]
  [Member(Index = 11, Type = MemberType.Varchar, Length
    = AdditionalLastName2Text_MaxLength, Optional = true)]
  public string AdditionalLastName2Text
  {
    get => additionalLastName2Text;
    set => additionalLastName2Text = value != null
      ? Substring(value, 1, AdditionalLastName2Text_MaxLength) : null;
  }

  /// <summary>
  /// The value of the RESPONSE_DATE attribute.
  /// This field contains the date that the Title II Pending Claim Response 
  /// Record was returned to FCR.  The date is in CCYYMMDD format.
  /// </summary>
  [JsonPropertyName("responseDate")]
  [Member(Index = 12, Type = MemberType.Date, Optional = true)]
  public DateTime? ResponseDate
  {
    get => responseDate;
    set => responseDate = value;
  }

  /// <summary>Length of the OTHER_SSN attribute.</summary>
  public const int OtherSsn_MaxLength = 9;

  /// <summary>
  /// The value of the OTHER_SSN attribute.
  /// This field contains the SSN that was used in the match:
  ///  If the SSN Match Code is an M, this field contains the Additional/ 
  /// Multiple SSN that was used in the match. (The SSN in this field is
  /// different from the SSN in the SSN field.)
  ///  If the SSN Match Code is a V, this field contains spaces. (The SSN 
  /// that was used in the match is in the SSN field.)
  /// </summary>
  [JsonPropertyName("otherSsn")]
  [Member(Index = 13, Type = MemberType.Char, Length = OtherSsn_MaxLength, Optional
    = true)]
  public string OtherSsn
  {
    get => otherSsn;
    set => otherSsn = value != null
      ? TrimEnd(Substring(value, 1, OtherSsn_MaxLength)) : null;
  }

  /// <summary>Length of the SSN_MATCH_CODE attribute.</summary>
  public const int SsnMatchCode_MaxLength = 1;

  /// <summary>
  /// The value of the SSN_MATCH_CODE attribute.
  /// This field contains a value to indicate which SSN was used in match.
  /// M   The Other (Additional/Multiple) SSN was used in the match.
  /// V   The SSN was used in the match.
  /// </summary>
  [JsonPropertyName("ssnMatchCode")]
  [Member(Index = 14, Type = MemberType.Char, Length = SsnMatchCode_MaxLength, Optional
    = true)]
  public string SsnMatchCode
  {
    get => ssnMatchCode;
    set => ssnMatchCode = value != null
      ? TrimEnd(Substring(value, 1, SsnMatchCode_MaxLength)) : null;
  }

  /// <summary>Length of the CLAIM_TYPE_CODE attribute.</summary>
  public const int ClaimTypeCode_MaxLength = 2;

  /// <summary>
  /// The value of the CLAIM_TYPE_CODE attribute.
  /// This field contains a value to indicate the claim type:
  /// AU  Auxiliary
  /// DI  Disability
  /// RI  Retirement
  /// SU  Survivor Benefits
  ///  This field contains spaces if claim type is not available.
  /// </summary>
  [JsonPropertyName("claimTypeCode")]
  [Member(Index = 15, Type = MemberType.Char, Length
    = ClaimTypeCode_MaxLength, Optional = true)]
  public string ClaimTypeCode
  {
    get => claimTypeCode;
    set => claimTypeCode = value != null
      ? TrimEnd(Substring(value, 1, ClaimTypeCode_MaxLength)) : null;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// USER ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 16, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// 	
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 17, Type = MemberType.Timestamp)]
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
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 18, Type = MemberType.Char, Length
    = LastUpdatedBy_MaxLength, Optional = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TIMESTAMP attribute.
  /// Timestamp of last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTimestamp")]
  [Member(Index = 19, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  /// <summary>Length of the LOCATE_SOURCE_RESPONSE_AGENCY_CO attribute.
  /// </summary>
  public const int FcgLSRspAgy_MaxLength = 3;

  /// <summary>
  /// The value of the LOCATE_SOURCE_RESPONSE_AGENCY_CO attribute.
  /// This field contains the code that identifies the Locate Source.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 20, Type = MemberType.Char, Length = FcgLSRspAgy_MaxLength)]
  public string FcgLSRspAgy
  {
    get => fcgLSRspAgy ?? "";
    set => fcgLSRspAgy = TrimEnd(Substring(value, 1, FcgLSRspAgy_MaxLength));
  }

  /// <summary>
  /// The json value of the FcgLSRspAgy attribute.</summary>
  [JsonPropertyName("fcgLSRspAgy")]
  [Computed]
  public string FcgLSRspAgy_Json
  {
    get => NullIf(FcgLSRspAgy, "");
    set => FcgLSRspAgy = value;
  }

  /// <summary>Length of the MEMBER_ID attribute.</summary>
  public const int FcgMemberId_MaxLength = 15;

  /// <summary>
  /// The value of the MEMBER_ID attribute.
  /// This field contains the Member ID that was provided by the submitter of 
  /// the Locate Request.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 21, Type = MemberType.Char, Length = FcgMemberId_MaxLength)]
  public string FcgMemberId
  {
    get => fcgMemberId ?? "";
    set => fcgMemberId = TrimEnd(Substring(value, 1, FcgMemberId_MaxLength));
  }

  /// <summary>
  /// The json value of the FcgMemberId attribute.</summary>
  [JsonPropertyName("fcgMemberId")]
  [Computed]
  public string FcgMemberId_Json
  {
    get => NullIf(FcgMemberId, "");
    set => FcgMemberId = value;
  }

  private int seqNo;
  private string nameMatchedCode;
  private string firstNameText;
  private string middleNameText;
  private string lastNameText;
  private string additionalFirstName1Text;
  private string additionalMiddleName1Text;
  private string additionalLastName1Text;
  private string additionalFirstName2Text;
  private string additionalMiddleName2Text;
  private string additionalLastName2Text;
  private DateTime? responseDate;
  private string otherSsn;
  private string ssnMatchCode;
  private string claimTypeCode;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private string fcgLSRspAgy;
  private string fcgMemberId;
}
