// The source file: APPEAL, ID: 371430596, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: LGLENFAC
/// A formal request for a hearing by a higher court to review the rulings and 
/// judgement of a Court Order.
/// </summary>
[Serializable]
public partial class Appeal: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public Appeal()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public Appeal(Appeal that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new Appeal Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(Appeal that)
  {
    base.Assign(that);
    identifier = that.identifier;
    docketNumber = that.docketNumber;
    filedByLastName = that.filedByLastName;
    filedByFirstName = that.filedByFirstName;
    filedByMi = that.filedByMi;
    appealDate = that.appealDate;
    docketingStmtFiledDate = that.docketingStmtFiledDate;
    attorneyLastName = that.attorneyLastName;
    attorneyFirstName = that.attorneyFirstName;
    attorneyMiddleInitial = that.attorneyMiddleInitial;
    attorneySuffix = that.attorneySuffix;
    appellantBriefDate = that.appellantBriefDate;
    replyBriefDate = that.replyBriefDate;
    oralArgumentDate = that.oralArgumentDate;
    decisionDate = that.decisionDate;
    decisionResult = that.decisionResult;
    furtherAppealIndicator = that.furtherAppealIndicator;
    extentionReqGrantedDate = that.extentionReqGrantedDate;
    dateExtensionGranted = that.dateExtensionGranted;
    createdBy = that.createdBy;
    createdTstamp = that.createdTstamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTstamp = that.lastUpdatedTstamp;
    trbId = that.trbId;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// This is a system generated number to uniquely identify an appeal.
  /// </summary>
  [JsonPropertyName("identifier")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 9)]
  public int Identifier
  {
    get => identifier;
    set => identifier = value;
  }

  /// <summary>Length of the DOCKET_NUMBER attribute.</summary>
  public const int DocketNumber_MaxLength = 10;

  /// <summary>
  /// The value of the DOCKET_NUMBER attribute.
  /// Identifying number assigned by the Court of Appeals.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = DocketNumber_MaxLength)]
  public string DocketNumber
  {
    get => docketNumber ?? "";
    set => docketNumber = TrimEnd(Substring(value, 1, DocketNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the DocketNumber attribute.</summary>
  [JsonPropertyName("docketNumber")]
  [Computed]
  public string DocketNumber_Json
  {
    get => NullIf(DocketNumber, "");
    set => DocketNumber = value;
  }

  /// <summary>Length of the FILED_BY_LAST_NAME attribute.</summary>
  public const int FiledByLastName_MaxLength = 17;

  /// <summary>
  /// The value of the FILED_BY_LAST_NAME attribute.
  /// This attribute specifies the last name of the person appealing the action.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = FiledByLastName_MaxLength)]
    
  public string FiledByLastName
  {
    get => filedByLastName ?? "";
    set => filedByLastName =
      TrimEnd(Substring(value, 1, FiledByLastName_MaxLength));
  }

  /// <summary>
  /// The json value of the FiledByLastName attribute.</summary>
  [JsonPropertyName("filedByLastName")]
  [Computed]
  public string FiledByLastName_Json
  {
    get => NullIf(FiledByLastName, "");
    set => FiledByLastName = value;
  }

  /// <summary>Length of the FILED_BY_FIRST_NAME attribute.</summary>
  public const int FiledByFirstName_MaxLength = 12;

  /// <summary>
  /// The value of the FILED_BY_FIRST_NAME attribute.
  /// This attribute specifies the first name of the person appealing the 
  /// action.
  /// </summary>
  [JsonPropertyName("filedByFirstName")]
  [Member(Index = 4, Type = MemberType.Char, Length
    = FiledByFirstName_MaxLength, Optional = true)]
  public string FiledByFirstName
  {
    get => filedByFirstName;
    set => filedByFirstName = value != null
      ? TrimEnd(Substring(value, 1, FiledByFirstName_MaxLength)) : null;
  }

  /// <summary>Length of the FILED_BY_MI attribute.</summary>
  public const int FiledByMi_MaxLength = 1;

  /// <summary>
  /// The value of the FILED_BY_MI attribute.
  /// This attribute specifies the middle initial of the person appealing the 
  /// action.
  /// </summary>
  [JsonPropertyName("filedByMi")]
  [Member(Index = 5, Type = MemberType.Char, Length = FiledByMi_MaxLength, Optional
    = true)]
  public string FiledByMi
  {
    get => filedByMi;
    set => filedByMi = value != null
      ? TrimEnd(Substring(value, 1, FiledByMi_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the APPEAL_DATE attribute.
  /// The date the Notice of Appeal (Legal Action) is filed with the Tribunal.
  /// </summary>
  [JsonPropertyName("appealDate")]
  [Member(Index = 6, Type = MemberType.Date)]
  public DateTime? AppealDate
  {
    get => appealDate;
    set => appealDate = value;
  }

  /// <summary>
  /// The value of the DOCKETING_STMT_FILED_DATE attribute.
  /// The date the Docketing Statement is filed with the Court of Appeals.
  /// </summary>
  [JsonPropertyName("docketingStmtFiledDate")]
  [Member(Index = 7, Type = MemberType.Date)]
  public DateTime? DocketingStmtFiledDate
  {
    get => docketingStmtFiledDate;
    set => docketingStmtFiledDate = value;
  }

  /// <summary>Length of the ATTORNEY_LAST_NAME attribute.</summary>
  public const int AttorneyLastName_MaxLength = 17;

  /// <summary>
  /// The value of the ATTORNEY_LAST_NAME attribute.
  /// The CSE Attorney responsible for the appeal.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = AttorneyLastName_MaxLength)
    ]
  public string AttorneyLastName
  {
    get => attorneyLastName ?? "";
    set => attorneyLastName =
      TrimEnd(Substring(value, 1, AttorneyLastName_MaxLength));
  }

  /// <summary>
  /// The json value of the AttorneyLastName attribute.</summary>
  [JsonPropertyName("attorneyLastName")]
  [Computed]
  public string AttorneyLastName_Json
  {
    get => NullIf(AttorneyLastName, "");
    set => AttorneyLastName = value;
  }

  /// <summary>Length of the ATTORNEY_FIRST_NAME attribute.</summary>
  public const int AttorneyFirstName_MaxLength = 12;

  /// <summary>
  /// The value of the ATTORNEY_FIRST_NAME attribute.
  /// The first name of the CSE Attorney responsible for the appeal.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length
    = AttorneyFirstName_MaxLength)]
  public string AttorneyFirstName
  {
    get => attorneyFirstName ?? "";
    set => attorneyFirstName =
      TrimEnd(Substring(value, 1, AttorneyFirstName_MaxLength));
  }

  /// <summary>
  /// The json value of the AttorneyFirstName attribute.</summary>
  [JsonPropertyName("attorneyFirstName")]
  [Computed]
  public string AttorneyFirstName_Json
  {
    get => NullIf(AttorneyFirstName, "");
    set => AttorneyFirstName = value;
  }

  /// <summary>Length of the ATTORNEY_MIDDLE_INITIAL attribute.</summary>
  public const int AttorneyMiddleInitial_MaxLength = 1;

  /// <summary>
  /// The value of the ATTORNEY_MIDDLE_INITIAL attribute.
  /// The middle initial of the CSE Attorney responsible for the appeal.
  /// </summary>
  [JsonPropertyName("attorneyMiddleInitial")]
  [Member(Index = 10, Type = MemberType.Char, Length
    = AttorneyMiddleInitial_MaxLength, Optional = true)]
  public string AttorneyMiddleInitial
  {
    get => attorneyMiddleInitial;
    set => attorneyMiddleInitial = value != null
      ? TrimEnd(Substring(value, 1, AttorneyMiddleInitial_MaxLength)) : null;
  }

  /// <summary>Length of the ATTORNEY_SUFFIX attribute.</summary>
  public const int AttorneySuffix_MaxLength = 3;

  /// <summary>
  /// The value of the ATTORNEY_SUFFIX attribute.
  /// The suffix of the CSE Attorney responsible for the appeal.
  /// </summary>
  [JsonPropertyName("attorneySuffix")]
  [Member(Index = 11, Type = MemberType.Char, Length
    = AttorneySuffix_MaxLength, Optional = true)]
  public string AttorneySuffix
  {
    get => attorneySuffix;
    set => attorneySuffix = value != null
      ? TrimEnd(Substring(value, 1, AttorneySuffix_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the APPELLANT_BRIEF_DATE attribute.
  /// The date the Appellant Brief is prepared.
  /// </summary>
  [JsonPropertyName("appellantBriefDate")]
  [Member(Index = 12, Type = MemberType.Date, Optional = true)]
  public DateTime? AppellantBriefDate
  {
    get => appellantBriefDate;
    set => appellantBriefDate = value;
  }

  /// <summary>
  /// The value of the REPLY_BRIEF_DATE attribute.
  /// The date the reply brief was received or sent.
  /// </summary>
  [JsonPropertyName("replyBriefDate")]
  [Member(Index = 13, Type = MemberType.Date, Optional = true)]
  public DateTime? ReplyBriefDate
  {
    get => replyBriefDate;
    set => replyBriefDate = value;
  }

  /// <summary>
  /// The value of the ORAL_ARGUMENT_DATE attribute.
  /// The date the oral arguments are heard in a tribunal.
  /// </summary>
  [JsonPropertyName("oralArgumentDate")]
  [Member(Index = 14, Type = MemberType.Date, Optional = true)]
  public DateTime? OralArgumentDate
  {
    get => oralArgumentDate;
    set => oralArgumentDate = value;
  }

  /// <summary>
  /// The value of the DECISION_DATE attribute.
  /// The date a decision on the appeal is handed down by the tribunal.
  /// </summary>
  [JsonPropertyName("decisionDate")]
  [Member(Index = 15, Type = MemberType.Date, Optional = true)]
  public DateTime? DecisionDate
  {
    get => decisionDate;
    set => decisionDate = value;
  }

  /// <summary>Length of the DECISION_RESULT attribute.</summary>
  public const int DecisionResult_MaxLength = 240;

  /// <summary>
  /// The value of the DECISION_RESULT attribute.
  /// The decision on the appeal handed down by the tribunal.
  /// </summary>
  [JsonPropertyName("decisionResult")]
  [Member(Index = 16, Type = MemberType.Varchar, Length
    = DecisionResult_MaxLength, Optional = true)]
  public string DecisionResult
  {
    get => decisionResult;
    set => decisionResult = value != null
      ? Substring(value, 1, DecisionResult_MaxLength) : null;
  }

  /// <summary>Length of the FURTHER_APPEAL_INDICATOR attribute.</summary>
  public const int FurtherAppealIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the FURTHER_APPEAL_INDICATOR attribute.
  /// Indicates whether the appeal has been referred to a higher court.
  /// </summary>
  [JsonPropertyName("furtherAppealIndicator")]
  [Member(Index = 17, Type = MemberType.Char, Length
    = FurtherAppealIndicator_MaxLength, Optional = true)]
  public string FurtherAppealIndicator
  {
    get => furtherAppealIndicator;
    set => furtherAppealIndicator = value != null
      ? TrimEnd(Substring(value, 1, FurtherAppealIndicator_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the EXTENTION_REQ_GRANTED_DATE attribute.
  /// The date that a request for an extension is made.
  /// </summary>
  [JsonPropertyName("extentionReqGrantedDate")]
  [Member(Index = 18, Type = MemberType.Date, Optional = true)]
  public DateTime? ExtentionReqGrantedDate
  {
    get => extentionReqGrantedDate;
    set => extentionReqGrantedDate = value;
  }

  /// <summary>
  /// The value of the DATE_EXTENSION_GRANTED attribute.
  /// The date the tribunal set for the oral argument date.
  /// </summary>
  [JsonPropertyName("dateExtensionGranted")]
  [Member(Index = 19, Type = MemberType.Date, Optional = true)]
  public DateTime? DateExtensionGranted
  {
    get => dateExtensionGranted;
    set => dateExtensionGranted = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The signon of the person that created the occurrance of the entity.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 20, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// The value of the CREATED_TSTAMP attribute.
  /// The date and time that the occurrance of the entity was created.
  /// </summary>
  [JsonPropertyName("createdTstamp")]
  [Member(Index = 21, Type = MemberType.Timestamp)]
  public DateTime? CreatedTstamp
  {
    get => createdTstamp;
    set => createdTstamp = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// The signon of the person that last updated the occurrance of the entity.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 22, Type = MemberType.Char, Length
    = LastUpdatedBy_MaxLength, Optional = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TSTAMP attribute.
  /// The date and time that the occurrance of the entity was last updated.
  /// </summary>
  [JsonPropertyName("lastUpdatedTstamp")]
  [Member(Index = 23, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTstamp
  {
    get => lastUpdatedTstamp;
    set => lastUpdatedTstamp = value;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// This attribute uniquely identifies a tribunal record. It is automatically 
  /// generated by the system starting from 1.
  /// </summary>
  [JsonPropertyName("trbId")]
  [Member(Index = 24, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? TrbId
  {
    get => trbId;
    set => trbId = value;
  }

  private int identifier;
  private string docketNumber;
  private string filedByLastName;
  private string filedByFirstName;
  private string filedByMi;
  private DateTime? appealDate;
  private DateTime? docketingStmtFiledDate;
  private string attorneyLastName;
  private string attorneyFirstName;
  private string attorneyMiddleInitial;
  private string attorneySuffix;
  private DateTime? appellantBriefDate;
  private DateTime? replyBriefDate;
  private DateTime? oralArgumentDate;
  private DateTime? decisionDate;
  private string decisionResult;
  private string furtherAppealIndicator;
  private DateTime? extentionReqGrantedDate;
  private DateTime? dateExtensionGranted;
  private string createdBy;
  private DateTime? createdTstamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTstamp;
  private int? trbId;
}
