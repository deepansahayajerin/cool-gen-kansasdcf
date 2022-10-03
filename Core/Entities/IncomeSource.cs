// The source file: INCOME_SOURCE, ID: 371435487, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVINIT
/// Contains details about the source of an income for a CSE PERSON.
/// </summary>
[Serializable]
public partial class IncomeSource: Heap, ICloneable
{
  /// <summary>Default constructor.</summary>
  public IncomeSource()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public IncomeSource(IncomeSource that):
    base(that)
  {
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new IncomeSource Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>
  /// The value of the LAST_UPDATED_TIMESTAMP attribute.
  /// Timestamp of last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTimestamp")]
  [Member(Index = 1, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTimestamp
  {
    get => Get<DateTime?>("lastUpdatedTimestamp");
    set => Set("lastUpdatedTimestamp", value);
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// User ID or Program ID responsible for the last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 2, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength, Optional
    = true)]
  public string LastUpdatedBy
  {
    get => Get<string>("lastUpdatedBy");
    set => Set(
      "lastUpdatedBy", TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)));
  }

  /// <summary>
  /// The value of the CREATED_TIMESTAMP attribute.
  /// Timestamp of creation of the occurrence.
  /// 	
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 3, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => Get<DateTime?>("createdTimestamp");
    set => Set("createdTimestamp", value);
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// USER ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
  public string CreatedBy
  {
    get => Get<string>("createdBy") ?? "";
    set => Set(
      "createdBy", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, CreatedBy_MaxLength)));
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
  /// The value of the IDENTIFIER attribute.
  /// This is a system generated identifier of no business meaning.
  /// </summary>
  [JsonPropertyName("identifier")]
  [Member(Index = 5, Type = MemberType.Timestamp)]
  public DateTime? Identifier
  {
    get => Get<DateTime?>("identifier");
    set => Set("identifier", value);
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int Type1_MaxLength = 1;

  /// <summary>
  /// The value of the TYPE attribute.
  /// This describes the source of the income,
  /// For example:
  /// E - Employer
  /// O - Other
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = Type1_MaxLength)]
  [Value("R")]
  [Value("E")]
  [Value("O")]
  [Value("M")]
  public string Type1
  {
    get => Get<string>("type1") ?? "";
    set => Set(
      "type1", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, Type1_MaxLength)));
  }

  /// <summary>
  /// The json value of the Type1 attribute.</summary>
  [JsonPropertyName("type1")]
  [Computed]
  public string Type1_Json
  {
    get => NullIf(Type1, "");
    set => Type1 = value;
  }

  /// <summary>
  /// The value of the LAST_QTR_INCOME attribute.
  /// This is the amount of income, reported by DHR, in the last qtr.
  /// </summary>
  [JsonPropertyName("lastQtrIncome")]
  [Member(Index = 7, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? LastQtrIncome
  {
    get => Get<decimal?>("lastQtrIncome");
    set => Set("lastQtrIncome", Truncate(value, 2));
  }

  /// <summary>Length of the LAST_QTR attribute.</summary>
  public const int LastQtr_MaxLength = 1;

  /// <summary>
  /// The value of the LAST_QTR attribute.
  /// The quarter in which the last income was reported by DHR
  /// </summary>
  [JsonPropertyName("lastQtr")]
  [Member(Index = 8, Type = MemberType.Char, Length = LastQtr_MaxLength, Optional
    = true)]
  public string LastQtr
  {
    get => Get<string>("lastQtr");
    set => Set("lastQtr", TrimEnd(Substring(value, 1, LastQtr_MaxLength)));
  }

  /// <summary>
  /// The value of the LAST_QTR_YR attribute.
  /// The year in which the last quarter income was reported by DHR
  /// </summary>
  [JsonPropertyName("lastQtrYr")]
  [Member(Index = 9, Type = MemberType.Number, Length = 4, Optional = true)]
  public int? LastQtrYr
  {
    get => Get<int?>("lastQtrYr");
    set => Set("lastQtrYr", value);
  }

  /// <summary>
  /// The value of the 2ND_QTR_INCOME attribute.
  /// This is the amount of income,reported by DHR, in the 2nd qtr.
  /// </summary>
  [JsonPropertyName("attribute2NdQtrIncome")]
  [Member(Index = 10, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? Attribute2NdQtrIncome
  {
    get => Get<decimal?>("attribute2NdQtrIncome");
    set => Set("attribute2NdQtrIncome", Truncate(value, 2));
  }

  /// <summary>Length of the 2ND_QTR attribute.</summary>
  public const int Attribute2NdQtr_MaxLength = 1;

  /// <summary>
  /// The value of the 2ND_QTR attribute.
  /// The quarter that the 2nd quarter income was reported by DHR
  /// </summary>
  [JsonPropertyName("attribute2NdQtr")]
  [Member(Index = 11, Type = MemberType.Char, Length
    = Attribute2NdQtr_MaxLength, Optional = true)]
  public string Attribute2NdQtr
  {
    get => Get<string>("attribute2NdQtr");
    set => Set(
      "attribute2NdQtr",
      TrimEnd(Substring(value, 1, Attribute2NdQtr_MaxLength)));
  }

  /// <summary>
  /// The value of the 2ND_QTR_YR attribute.
  /// The year that the 2nd quarter income was reported by DHR
  /// </summary>
  [JsonPropertyName("attribute2NdQtrYr")]
  [Member(Index = 12, Type = MemberType.Number, Length = 4, Optional = true)]
  public int? Attribute2NdQtrYr
  {
    get => Get<int?>("attribute2NdQtrYr");
    set => Set("attribute2NdQtrYr", value);
  }

  /// <summary>
  /// The value of the 3RD_QTR_INCOME attribute.
  /// This is the amount of income reported by DHR for the 3rd qtr.
  /// </summary>
  [JsonPropertyName("attribute3RdQtrIncome")]
  [Member(Index = 13, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? Attribute3RdQtrIncome
  {
    get => Get<decimal?>("attribute3RdQtrIncome");
    set => Set("attribute3RdQtrIncome", Truncate(value, 2));
  }

  /// <summary>Length of the 3RD_QTR attribute.</summary>
  public const int Attribute3RdQtr_MaxLength = 1;

  /// <summary>
  /// The value of the 3RD_QTR attribute.
  /// The quarter in which the 3rd quarter income was reported by DHR
  /// </summary>
  [JsonPropertyName("attribute3RdQtr")]
  [Member(Index = 14, Type = MemberType.Char, Length
    = Attribute3RdQtr_MaxLength, Optional = true)]
  public string Attribute3RdQtr
  {
    get => Get<string>("attribute3RdQtr");
    set => Set(
      "attribute3RdQtr",
      TrimEnd(Substring(value, 1, Attribute3RdQtr_MaxLength)));
  }

  /// <summary>
  /// The value of the 3RD_QTR_YR attribute.
  /// The year in which the 3rd quarter income was reported by DHR
  /// </summary>
  [JsonPropertyName("attribute3RdQtrYr")]
  [Member(Index = 15, Type = MemberType.Number, Length = 4, Optional = true)]
  public int? Attribute3RdQtrYr
  {
    get => Get<int?>("attribute3RdQtrYr");
    set => Set("attribute3RdQtrYr", value);
  }

  /// <summary>
  /// The value of the 4TH_QTR_INCOME attribute.
  /// This is the amount of income reported by DHR  in the 4th qtr.
  /// </summary>
  [JsonPropertyName("attribute4ThQtrIncome")]
  [Member(Index = 16, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? Attribute4ThQtrIncome
  {
    get => Get<decimal?>("attribute4ThQtrIncome");
    set => Set("attribute4ThQtrIncome", Truncate(value, 2));
  }

  /// <summary>Length of the 4TH_QTR attribute.</summary>
  public const int Attribute4ThQtr_MaxLength = 1;

  /// <summary>
  /// The value of the 4TH_QTR attribute.
  /// the quarter in which the 4th quarter income was reported by DHR
  /// </summary>
  [JsonPropertyName("attribute4ThQtr")]
  [Member(Index = 17, Type = MemberType.Char, Length
    = Attribute4ThQtr_MaxLength, Optional = true)]
  public string Attribute4ThQtr
  {
    get => Get<string>("attribute4ThQtr");
    set => Set(
      "attribute4ThQtr",
      TrimEnd(Substring(value, 1, Attribute4ThQtr_MaxLength)));
  }

  /// <summary>
  /// The value of the 4TH_QTR_YR attribute.
  /// the year in which the 4th quarter income was reported by DHR
  /// </summary>
  [JsonPropertyName("attribute4ThQtrYr")]
  [Member(Index = 18, Type = MemberType.Number, Length = 4, Optional = true)]
  public int? Attribute4ThQtrYr
  {
    get => Get<int?>("attribute4ThQtrYr");
    set => Set("attribute4ThQtrYr", value);
  }

  /// <summary>
  /// The value of the SENT_DT attribute.
  /// This is the date that a letter was sent to the income source regarding a 
  /// particular case and AP.
  /// </summary>
  [JsonPropertyName("sentDt")]
  [Member(Index = 19, Type = MemberType.Date, Optional = true)]
  public DateTime? SentDt
  {
    get => Get<DateTime?>("sentDt");
    set => Set("sentDt", value);
  }

  /// <summary>Length of the SEND_TO attribute.</summary>
  public const int SendTo_MaxLength = 2;

  /// <summary>
  /// The value of the SEND_TO attribute.
  /// Indicates which employer address to send an Income Verification Letter, as
  /// defined by the Employer Relation.
  /// </summary>
  [JsonPropertyName("sendTo")]
  [Member(Index = 20, Type = MemberType.Char, Length = SendTo_MaxLength, Optional
    = true)]
  [Value(null)]
  [Value("HQ")]
  [Value("WS")]
  public string SendTo
  {
    get => Get<string>("sendTo");
    set => Set("sendTo", TrimEnd(Substring(value, 1, SendTo_MaxLength)));
  }

  /// <summary>
  /// The value of the RETURN_DT attribute.
  /// This is the date that the income source responded to a letter previously 
  /// sent regarding a case and AP.
  /// </summary>
  [JsonPropertyName("returnDt")]
  [Member(Index = 21, Type = MemberType.Date, Optional = true)]
  public DateTime? ReturnDt
  {
    get => Get<DateTime?>("returnDt");
    set => Set("returnDt", value);
  }

  /// <summary>Length of the RETURN_CD attribute.</summary>
  public const int ReturnCd_MaxLength = 1;

  /// <summary>
  /// The value of the RETURN_CD attribute.
  /// This describes the response from the income source regarding a case and 
  /// AP.
  /// </summary>
  [JsonPropertyName("returnCd")]
  [Member(Index = 22, Type = MemberType.Char, Length = ReturnCd_MaxLength, Optional
    = true)]
  public string ReturnCd
  {
    get => Get<string>("returnCd");
    set => Set("returnCd", TrimEnd(Substring(value, 1, ReturnCd_MaxLength)));
  }

  /// <summary>Length of the WORKER_ID attribute.</summary>
  public const int WorkerId_MaxLength = 8;

  /// <summary>
  /// The value of the WORKER_ID attribute.
  /// User Id of the person responsible for sending an Income Source Letter of 
  /// verification.
  /// </summary>
  [JsonPropertyName("workerId")]
  [Member(Index = 23, Type = MemberType.Char, Length = WorkerId_MaxLength, Optional
    = true)]
  public string WorkerId
  {
    get => Get<string>("workerId");
    set => Set("workerId", TrimEnd(Substring(value, 1, WorkerId_MaxLength)));
  }

  /// <summary>Length of the NOTE attribute.</summary>
  public const int Note_MaxLength = 70;

  /// <summary>
  /// The value of the NOTE attribute.
  /// Free Form area for any additional information on an income source
  /// </summary>
  [JsonPropertyName("note")]
  [Member(Index = 24, Type = MemberType.Varchar, Length = Note_MaxLength, Optional
    = true)]
  public string Note
  {
    get => Get<string>("note");
    set => Set("note", Substring(value, 1, Note_MaxLength));
  }

  /// <summary>Length of the NAME attribute.</summary>
  public const int Name_MaxLength = 36;

  /// <summary>
  /// The value of the NAME attribute.
  /// This is the name of the income source.  This will either be an institution
  /// name or a contact name.
  /// </summary>
  [JsonPropertyName("name")]
  [Member(Index = 25, Type = MemberType.Char, Length = Name_MaxLength, Optional
    = true)]
  public string Name
  {
    get => Get<string>("name");
    set => Set("name", TrimEnd(Substring(value, 1, Name_MaxLength)));
  }

  /// <summary>
  /// The value of the START_DT attribute.
  /// Date that the imcome was first received from the source.
  /// </summary>
  [JsonPropertyName("startDt")]
  [Member(Index = 26, Type = MemberType.Date, Optional = true)]
  public DateTime? StartDt
  {
    get => Get<DateTime?>("startDt");
    set => Set("startDt", value);
  }

  /// <summary>
  /// The value of the END_DT attribute.
  /// Date that the income was last received from the source.
  /// </summary>
  [JsonPropertyName("endDt")]
  [Member(Index = 27, Type = MemberType.Date, Optional = true)]
  public DateTime? EndDt
  {
    get => Get<DateTime?>("endDt");
    set => Set("endDt", value);
  }

  /// <summary>Length of the CODE attribute.</summary>
  public const int Code_MaxLength = 2;

  /// <summary>
  /// The value of the CODE attribute.
  /// Identifies the source of income for types other than those from 
  /// employment, military and resources.
  /// The permitted values are maintained in CODE_VALUE entity for CODE_NAME 
  /// OTHER_INCOME_SOURCE_TYPE
  /// </summary>
  [JsonPropertyName("code")]
  [Member(Index = 28, Type = MemberType.Char, Length = Code_MaxLength, Optional
    = true)]
  public string Code
  {
    get => Get<string>("code");
    set => Set("code", TrimEnd(Substring(value, 1, Code_MaxLength)));
  }

  /// <summary>Length of the MILITARY_CODE attribute.</summary>
  public const int MilitaryCode_MaxLength = 1;

  /// <summary>
  /// The value of the MILITARY_CODE attribute.
  /// A code to describe the type of military income. e.g. BAQ or rate of pay.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 29, Type = MemberType.Char, Length = MilitaryCode_MaxLength)]
  public string MilitaryCode
  {
    get => Get<string>("militaryCode") ?? "";
    set => Set(
      "militaryCode", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, MilitaryCode_MaxLength)));
  }

  /// <summary>
  /// The json value of the MilitaryCode attribute.</summary>
  [JsonPropertyName("militaryCode")]
  [Computed]
  public string MilitaryCode_Json
  {
    get => NullIf(MilitaryCode, "");
    set => MilitaryCode = value;
  }

  /// <summary>Length of the SELF_EMPLOYED_IND attribute.</summary>
  public const int SelfEmployedInd_MaxLength = 1;

  /// <summary>
  /// The value of the SELF_EMPLOYED_IND attribute.
  /// This indicates whether the CSE PERSON is self-employed.
  /// </summary>
  [JsonPropertyName("selfEmployedInd")]
  [Member(Index = 30, Type = MemberType.Char, Length
    = SelfEmployedInd_MaxLength, Optional = true)]
  public string SelfEmployedInd
  {
    get => Get<string>("selfEmployedInd");
    set => Set(
      "selfEmployedInd",
      TrimEnd(Substring(value, 1, SelfEmployedInd_MaxLength)));
  }

  /// <summary>Length of the NOTE_2 attribute.</summary>
  public const int Note2_MaxLength = 70;

  /// <summary>
  /// The value of the NOTE_2 attribute.
  /// Any additional information about an employer or registered agent
  /// </summary>
  [JsonPropertyName("note2")]
  [Member(Index = 31, Type = MemberType.Varchar, Length = Note2_MaxLength, Optional
    = true)]
  public string Note2
  {
    get => Get<string>("note2");
    set => Set("note2", Substring(value, 1, Note2_MaxLength));
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspINumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 32, Type = MemberType.Char, Length = CspINumber_MaxLength)]
  public string CspINumber
  {
    get => Get<string>("cspINumber") ?? "";
    set => Set(
      "cspINumber", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, CspINumber_MaxLength)));
  }

  /// <summary>
  /// The json value of the CspINumber attribute.</summary>
  [JsonPropertyName("cspINumber")]
  [Computed]
  public string CspINumber_Json
  {
    get => NullIf(CspINumber, "");
    set => CspINumber = value;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// A unique sequential number (See CONTROL_TABLE) that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("empId")]
  [Member(Index = 33, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? EmpId
  {
    get => Get<int?>("empId");
    set => Set("empId", value);
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int OraCreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The User ID of the logged on user that is responsible for the creation of 
  /// this occurrence of the entity.
  /// </summary>
  [JsonPropertyName("oraCreatedBy")]
  [Member(Index = 34, Type = MemberType.Char, Length = OraCreatedBy_MaxLength, Optional
    = true)]
  public string OraCreatedBy
  {
    get => Get<string>("oraCreatedBy");
    set => Set(
      "oraCreatedBy", TrimEnd(Substring(value, 1, OraCreatedBy_MaxLength)));
  }

  /// <summary>
  /// The value of the CREATED_TIMESTAMP attribute.
  /// A timestamp for the creation of the occurrence.
  /// </summary>
  [JsonPropertyName("oraTstamp")]
  [Member(Index = 35, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? OraTstamp
  {
    get => Get<DateTime?>("oraTstamp");
    set => Set("oraTstamp", value);
  }

  /// <summary>
  /// The value of the RESOURCE_NO attribute.
  /// A running serial number of the resource owned by the CSE Person.
  /// </summary>
  [JsonPropertyName("cprResourceNo")]
  [Member(Index = 36, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? CprResourceNo
  {
    get => Get<int?>("cprResourceNo");
    set => Set("cprResourceNo", value);
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonPropertyName("cspNumber")]
  [Member(Index = 37, Type = MemberType.Char, Length = CspNumber_MaxLength, Optional
    = true)]
  public string CspNumber
  {
    get => Get<string>("cspNumber");
    set => Set("cspNumber", TrimEnd(Substring(value, 1, CspNumber_MaxLength)));
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// This attribue specifies the date the service in the branch and rank 
  /// started.
  /// </summary>
  [JsonPropertyName("mseEffectiveDate")]
  [Member(Index = 38, Type = MemberType.Date, Optional = true)]
  public DateTime? MseEffectiveDate
  {
    get => Get<DateTime?>("mseEffectiveDate");
    set => Set("mseEffectiveDate", value);
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspSNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonPropertyName("cspSNumber")]
  [Member(Index = 39, Type = MemberType.Char, Length = CspSNumber_MaxLength, Optional
    = true)]
  public string CspSNumber
  {
    get => Get<string>("cspSNumber");
    set =>
      Set("cspSNumber", TrimEnd(Substring(value, 1, CspSNumber_MaxLength)));
  }
}
