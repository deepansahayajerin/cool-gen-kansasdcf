// The source file: LEGAL_REFERRAL, ID: 371436961, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: OBLGESTB
/// This entity describes the legal referral/ recommendation for action to be 
/// taken sent to Contractor/ SRS attorney.
/// </summary>
[Serializable]
public partial class LegalReferral: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public LegalReferral()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public LegalReferral(LegalReferral that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new LegalReferral Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(LegalReferral that)
  {
    base.Assign(that);
    referredByUserId = that.referredByUserId;
    identifier = that.identifier;
    referralDate = that.referralDate;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    statusDate = that.statusDate;
    referralReason1 = that.referralReason1;
    referralReason2 = that.referralReason2;
    referralReason3 = that.referralReason3;
    referralReason5 = that.referralReason5;
    referralReason4 = that.referralReason4;
    status = that.status;
    courtCaseNumber = that.courtCaseNumber;
    tribunalId = that.tribunalId;
    casNumber = that.casNumber;
    oraCreatedBy = that.oraCreatedBy;
    oraTstamp = that.oraTstamp;
  }

  /// <summary>Length of the REFERRED_BY_USER_ID attribute.</summary>
  public const int ReferredByUserId_MaxLength = 8;

  /// <summary>
  /// The value of the REFERRED_BY_USER_ID attribute.
  /// This attribute specifies the user who created the legal referral.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = ReferredByUserId_MaxLength)
    ]
  public string ReferredByUserId
  {
    get => referredByUserId ?? "";
    set => referredByUserId =
      TrimEnd(Substring(value, 1, ReferredByUserId_MaxLength));
  }

  /// <summary>
  /// The json value of the ReferredByUserId attribute.</summary>
  [JsonPropertyName("referredByUserId")]
  [Computed]
  public string ReferredByUserId_Json
  {
    get => NullIf(ReferredByUserId, "");
    set => ReferredByUserId = value;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// Attribute to uniquely identify a legal referral associated within a case.
  /// This will be a system-generated number.
  /// </summary>
  [JsonPropertyName("identifier")]
  [DefaultValue(0)]
  [Member(Index = 2, Type = MemberType.Number, Length = 3)]
  public int Identifier
  {
    get => identifier;
    set => identifier = value;
  }

  /// <summary>
  /// The value of the REFERRAL_DATE attribute.
  /// The date on which the legal referral was raised.
  /// </summary>
  [JsonPropertyName("referralDate")]
  [Member(Index = 3, Type = MemberType.Date)]
  public DateTime? ReferralDate
  {
    get => referralDate;
    set => referralDate = value;
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

  /// <summary>
  /// The value of the STATUS_DATE attribute.
  /// This specifies the date on which the legal referral status was effective.
  /// e.g. If status was &quot;S&quot; (Sent) then status date gives the Date 
  /// the Referral was Sent. If the status was &quot;R&quot; (Received) then
  /// status date gives the date on which the referral was received and so on.
  /// </summary>
  [JsonPropertyName("statusDate")]
  [Member(Index = 8, Type = MemberType.Date, Optional = true)]
  public DateTime? StatusDate
  {
    get => statusDate;
    set => statusDate = value;
  }

  /// <summary>Length of the REFERRAL_REASON_1 attribute.</summary>
  public const int ReferralReason1_MaxLength = 4;

  /// <summary>
  /// The value of the REFERRAL_REASON_1 attribute.
  /// This attribute specifies the reason for which the legal referral is being 
  /// raised.
  /// e.g. Establish paternity,
  ///      Establish child support order, etc.
  /// The valid values are maintained in CODE_VALUE entity for code name LAGAL 
  /// REFERRAL REASON.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = ReferralReason1_MaxLength)]
    
  public string ReferralReason1
  {
    get => referralReason1 ?? "";
    set => referralReason1 =
      TrimEnd(Substring(value, 1, ReferralReason1_MaxLength));
  }

  /// <summary>
  /// The json value of the ReferralReason1 attribute.</summary>
  [JsonPropertyName("referralReason1")]
  [Computed]
  public string ReferralReason1_Json
  {
    get => NullIf(ReferralReason1, "");
    set => ReferralReason1 = value;
  }

  /// <summary>Length of the REFERRAL_REASON_2 attribute.</summary>
  public const int ReferralReason2_MaxLength = 4;

  /// <summary>
  /// The value of the REFERRAL_REASON_2 attribute.
  /// This attribute specifies the reason for which the legal referral is being 
  /// raised.
  /// e.g. Establish paternity,
  ///      Establish child support order, etc.
  /// The valid values are maintained in CODE_VALUE entity for code name LAGAL 
  /// REFERRAL REASON.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = ReferralReason2_MaxLength)
    ]
  public string ReferralReason2
  {
    get => referralReason2 ?? "";
    set => referralReason2 =
      TrimEnd(Substring(value, 1, ReferralReason2_MaxLength));
  }

  /// <summary>
  /// The json value of the ReferralReason2 attribute.</summary>
  [JsonPropertyName("referralReason2")]
  [Computed]
  public string ReferralReason2_Json
  {
    get => NullIf(ReferralReason2, "");
    set => ReferralReason2 = value;
  }

  /// <summary>Length of the REFERRAL_REASON_3 attribute.</summary>
  public const int ReferralReason3_MaxLength = 4;

  /// <summary>
  /// The value of the REFERRAL_REASON_3 attribute.
  /// This attribute specifies the reason for which the legal referral is being 
  /// raised.
  /// e.g. Establish paternity,
  ///      Establish child support order, etc.
  /// The valid values are maintained in CODE_VALUE entity for code name LAGAL 
  /// REFERRAL REASON.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length = ReferralReason3_MaxLength)
    ]
  public string ReferralReason3
  {
    get => referralReason3 ?? "";
    set => referralReason3 =
      TrimEnd(Substring(value, 1, ReferralReason3_MaxLength));
  }

  /// <summary>
  /// The json value of the ReferralReason3 attribute.</summary>
  [JsonPropertyName("referralReason3")]
  [Computed]
  public string ReferralReason3_Json
  {
    get => NullIf(ReferralReason3, "");
    set => ReferralReason3 = value;
  }

  /// <summary>Length of the REFERRAL_REASON_5 attribute.</summary>
  public const int ReferralReason5_MaxLength = 4;

  /// <summary>
  /// The value of the REFERRAL_REASON_5 attribute.
  /// This attribute specifies the reason for which the legal referral is being 
  /// raised.
  /// e.g. Establish paternity,
  ///      Establish child support order, etc.
  /// The valid values are maintained in CODE_VALUE entity for code name LAGAL 
  /// REFERRAL REASON.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length = ReferralReason5_MaxLength)
    ]
  public string ReferralReason5
  {
    get => referralReason5 ?? "";
    set => referralReason5 =
      TrimEnd(Substring(value, 1, ReferralReason5_MaxLength));
  }

  /// <summary>
  /// The json value of the ReferralReason5 attribute.</summary>
  [JsonPropertyName("referralReason5")]
  [Computed]
  public string ReferralReason5_Json
  {
    get => NullIf(ReferralReason5, "");
    set => ReferralReason5 = value;
  }

  /// <summary>Length of the REFERRAL_REASON_4 attribute.</summary>
  public const int ReferralReason4_MaxLength = 4;

  /// <summary>
  /// The value of the REFERRAL_REASON_4 attribute.
  /// This attribute specifies the reason for which the legal referral is being 
  /// raised.
  /// e.g. Establish paternity,
  ///      Establish child support order, etc.
  /// The valid values are maintained in CODE_VALUE entity for code name LAGAL 
  /// REFERRAL REASON.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length = ReferralReason4_MaxLength)
    ]
  public string ReferralReason4
  {
    get => referralReason4 ?? "";
    set => referralReason4 =
      TrimEnd(Substring(value, 1, ReferralReason4_MaxLength));
  }

  /// <summary>
  /// The json value of the ReferralReason4 attribute.</summary>
  [JsonPropertyName("referralReason4")]
  [Computed]
  public string ReferralReason4_Json
  {
    get => NullIf(ReferralReason4, "");
    set => ReferralReason4 = value;
  }

  /// <summary>Length of the STATUS attribute.</summary>
  public const int Status_MaxLength = 1;

  /// <summary>
  /// The value of the STATUS attribute.
  /// This describes the status of the legal referral.
  /// e.g. S - Legal referral has been sent.
  ///      R - Legal referral has been received.
  ///      ...
  ///      C - Closed etc
  /// Permitted values are maintained in the entity CODE_VALUE for CODE_NAME 
  /// LEGAL_REFERRAL_STATUS
  /// If the status values and status date need to be maintained for status 
  /// values like &quot;Received&quot; etc, they will be added later.
  /// </summary>
  [JsonPropertyName("status")]
  [Member(Index = 14, Type = MemberType.Char, Length = Status_MaxLength, Optional
    = true)]
  public string Status
  {
    get => status;
    set => status = value != null
      ? TrimEnd(Substring(value, 1, Status_MaxLength)) : null;
  }

  /// <summary>Length of the COURT_CASE_NUMBER attribute.</summary>
  public const int CourtCaseNumber_MaxLength = 17;

  /// <summary>
  /// The value of the COURT_CASE_NUMBER attribute.
  /// A number assigned by a tribunal to identify a court case. Typically, only 
  /// legal referrals with an ENF reason code will have the value populated.
  /// </summary>
  [JsonPropertyName("courtCaseNumber")]
  [Member(Index = 15, Type = MemberType.Char, Length
    = CourtCaseNumber_MaxLength, Optional = true)]
  public string CourtCaseNumber
  {
    get => courtCaseNumber;
    set => courtCaseNumber = value != null
      ? TrimEnd(Substring(value, 1, CourtCaseNumber_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the TRIBUNAL_ID attribute.
  /// This attribute uniquely identifies the tribunal record that presides over 
  /// the court case related to the referral. Typically, only legal referrals
  /// with an ENF reason code will have the value populated.
  /// </summary>
  [JsonPropertyName("tribunalId")]
  [Member(Index = 16, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? TribunalId
  {
    get => tribunalId;
    set => tribunalId = value;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CasNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// A unique number assigned to a request or referral.  Becomes the Case 
  /// number.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 17, Type = MemberType.Char, Length = CasNumber_MaxLength)]
  public string CasNumber
  {
    get => casNumber ?? "";
    set => casNumber = TrimEnd(Substring(value, 1, CasNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the CasNumber attribute.</summary>
  [JsonPropertyName("casNumber")]
  [Computed]
  public string CasNumber_Json
  {
    get => NullIf(CasNumber, "");
    set => CasNumber = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int OraCreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The User ID of the logged on user that is responsible for the creation of 
  /// this occurrence of the entity.
  /// </summary>
  [JsonPropertyName("oraCreatedBy")]
  [Member(Index = 18, Type = MemberType.Char, Length = OraCreatedBy_MaxLength, Optional
    = true)]
  public string OraCreatedBy
  {
    get => oraCreatedBy;
    set => oraCreatedBy = value != null
      ? TrimEnd(Substring(value, 1, OraCreatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the CREATED_TIMESTAMP attribute.
  /// A timestamp for the creation of the occurrence.
  /// </summary>
  [JsonPropertyName("oraTstamp")]
  [Member(Index = 19, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? OraTstamp
  {
    get => oraTstamp;
    set => oraTstamp = value;
  }

  private string referredByUserId;
  private int identifier;
  private DateTime? referralDate;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private DateTime? statusDate;
  private string referralReason1;
  private string referralReason2;
  private string referralReason3;
  private string referralReason5;
  private string referralReason4;
  private string status;
  private string courtCaseNumber;
  private int? tribunalId;
  private string casNumber;
  private string oraCreatedBy;
  private DateTime? oraTstamp;
}
