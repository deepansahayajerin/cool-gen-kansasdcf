// The source file: CASE_VERIFICATION_EXTRACT, ID: 372939563, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class CaseVerificationExtract: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public CaseVerificationExtract()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public CaseVerificationExtract(CaseVerificationExtract that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new CaseVerificationExtract Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(CaseVerificationExtract that)
  {
    base.Assign(that);
    detailsForLine = that.detailsForLine;
    cnumber = that.cnumber;
    cstatus = that.cstatus;
    cstatusDate = that.cstatusDate;
    copenDate = that.copenDate;
    crIdentifier = that.crIdentifier;
    crType = that.crType;
    crStartDate = that.crStartDate;
    crEndDate = that.crEndDate;
    cpType = that.cpType;
    cpNumber = that.cpNumber;
    ppCreatedTimestamp = that.ppCreatedTimestamp;
    ppEffDate = that.ppEffDate;
    ppDiscDate = that.ppDiscDate;
    pcode = that.pcode;
    peffDate = that.peffDate;
    pdiscDate = that.pdiscDate;
    lacrCreatedTstamp = that.lacrCreatedTstamp;
    laIdentifier = that.laIdentifier;
    laActionTaken = that.laActionTaken;
    laCreatedTstamp = that.laCreatedTstamp;
    laFiledDate = that.laFiledDate;
    ladNumber = that.ladNumber;
    ladDetailType = that.ladDetailType;
    ladCreatedTstamp = that.ladCreatedTstamp;
    ladNonFinOblgType = that.ladNonFinOblgType;
    otCode = that.otCode;
    otClassification = that.otClassification;
    ddDueDt = that.ddDueDt;
    ddBalanceDueAmt = that.ddBalanceDueAmt;
    ddInterestBalanceDueAmt = that.ddInterestBalanceDueAmt;
    ddCreatedTmst = that.ddCreatedTmst;
    cpaType = that.cpaType;
    cpaCreatedTmst = that.cpaCreatedTmst;
    obTranIdentifier = that.obTranIdentifier;
    obTranType = that.obTranType;
    obTranCreatedTmst = that.obTranCreatedTmst;
    obIdentifier = that.obIdentifier;
    obCreatedTmst = that.obCreatedTmst;
    collIdentifier = that.collIdentifier;
    collAppliedToCode = that.collAppliedToCode;
    collAdjustedInd = that.collAdjustedInd;
    collConcurrentInd = that.collConcurrentInd;
    collCreatedTmst = that.collCreatedTmst;
  }

  /// <summary>Length of the DETAILS_FOR_LINE attribute.</summary>
  public const int DetailsForLine_MaxLength = 15;

  /// <summary>
  /// The value of the DETAILS_FOR_LINE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = DetailsForLine_MaxLength)]
  public string DetailsForLine
  {
    get => detailsForLine ?? "";
    set => detailsForLine =
      TrimEnd(Substring(value, 1, DetailsForLine_MaxLength));
  }

  /// <summary>
  /// The json value of the DetailsForLine attribute.</summary>
  [JsonPropertyName("detailsForLine")]
  [Computed]
  public string DetailsForLine_Json
  {
    get => NullIf(DetailsForLine, "");
    set => DetailsForLine = value;
  }

  /// <summary>Length of the C_NUMBER attribute.</summary>
  public const int Cnumber_MaxLength = 10;

  /// <summary>
  /// The value of the C_NUMBER attribute.
  /// A unique number assigned to a request or referral.  Becomes the Case 
  /// number.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = Cnumber_MaxLength)]
  public string Cnumber
  {
    get => cnumber ?? "";
    set => cnumber = TrimEnd(Substring(value, 1, Cnumber_MaxLength));
  }

  /// <summary>
  /// The json value of the Cnumber attribute.</summary>
  [JsonPropertyName("cnumber")]
  [Computed]
  public string Cnumber_Json
  {
    get => NullIf(Cnumber, "");
    set => Cnumber = value;
  }

  /// <summary>Length of the C_STATUS attribute.</summary>
  public const int Cstatus_MaxLength = 1;

  /// <summary>
  /// The value of the C_STATUS attribute.
  /// The current status of the case
  /// e.g. Opened, Closed
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = Cstatus_MaxLength)]
  public string Cstatus
  {
    get => cstatus ?? "";
    set => cstatus = TrimEnd(Substring(value, 1, Cstatus_MaxLength));
  }

  /// <summary>
  /// The json value of the Cstatus attribute.</summary>
  [JsonPropertyName("cstatus")]
  [Computed]
  public string Cstatus_Json
  {
    get => NullIf(Cstatus, "");
    set => Cstatus = value;
  }

  /// <summary>
  /// The value of the C_STATUS_DATE attribute.
  /// The date the current status was set
  /// </summary>
  [JsonPropertyName("cstatusDate")]
  [Member(Index = 4, Type = MemberType.Date)]
  public DateTime? CstatusDate
  {
    get => cstatusDate;
    set => cstatusDate = value;
  }

  /// <summary>
  /// The value of the C_OPEN_DATE attribute.
  /// The date the case was established
  /// FR(III-2)
  /// </summary>
  [JsonPropertyName("copenDate")]
  [Member(Index = 5, Type = MemberType.Date)]
  public DateTime? CopenDate
  {
    get => copenDate;
    set => copenDate = value;
  }

  /// <summary>
  /// The value of the CR_IDENTIFIER attribute.
  /// The unique identifier.  System generated using a 3-digit random generator
  /// </summary>
  [JsonPropertyName("crIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 6, Type = MemberType.Number, Length = 3)]
  public int CrIdentifier
  {
    get => crIdentifier;
    set => crIdentifier = value;
  }

  /// <summary>Length of the CR_TYPE attribute.</summary>
  public const int CrType_MaxLength = 2;

  /// <summary>
  /// The value of the CR_TYPE attribute.
  /// The type of role played by a CSE Person in a given case.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = CrType_MaxLength)]
  [Value("FA")]
  [Value("MO")]
  [Value("AP")]
  [Value("AR")]
  [Value("CH")]
  public string CrType
  {
    get => crType ?? "";
    set => crType = TrimEnd(Substring(value, 1, CrType_MaxLength));
  }

  /// <summary>
  /// The json value of the CrType attribute.</summary>
  [JsonPropertyName("crType")]
  [Computed]
  public string CrType_Json
  {
    get => NullIf(CrType, "");
    set => CrType = value;
  }

  /// <summary>
  /// The value of the CR_START_DATE attribute.
  /// The date that the CSE Person takes on a role in a Case
  /// </summary>
  [JsonPropertyName("crStartDate")]
  [Member(Index = 8, Type = MemberType.Date)]
  public DateTime? CrStartDate
  {
    get => crStartDate;
    set => crStartDate = value;
  }

  /// <summary>
  /// The value of the CR_END_DATE attribute.
  /// The date a CSE Person ceases to play a given role within a given Case
  /// </summary>
  [JsonPropertyName("crEndDate")]
  [Member(Index = 9, Type = MemberType.Date)]
  public DateTime? CrEndDate
  {
    get => crEndDate;
    set => crEndDate = value;
  }

  /// <summary>Length of the CP_TYPE attribute.</summary>
  public const int CpType_MaxLength = 1;

  /// <summary>
  /// The value of the CP_TYPE attribute.
  /// This type defines whether the CSE Person is a Client or an organization.
  /// C - Client	
  /// O - Organization
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = CpType_MaxLength)]
  [Value("C")]
  [Value("O")]
  public string CpType
  {
    get => cpType ?? "";
    set => cpType = TrimEnd(Substring(value, 1, CpType_MaxLength));
  }

  /// <summary>
  /// The json value of the CpType attribute.</summary>
  [JsonPropertyName("cpType")]
  [Computed]
  public string CpType_Json
  {
    get => NullIf(CpType, "");
    set => CpType = value;
  }

  /// <summary>Length of the CP_NUMBER attribute.</summary>
  public const int CpNumber_MaxLength = 10;

  /// <summary>
  /// The value of the CP_NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length = CpNumber_MaxLength)]
  public string CpNumber
  {
    get => cpNumber ?? "";
    set => cpNumber = TrimEnd(Substring(value, 1, CpNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the CpNumber attribute.</summary>
  [JsonPropertyName("cpNumber")]
  [Computed]
  public string CpNumber_Json
  {
    get => NullIf(CpNumber, "");
    set => CpNumber = value;
  }

  /// <summary>
  /// The value of the PP_CREATED_TIMESTAMP attribute.
  /// * Draft *
  /// The timestamp of when the occurrence of the entity was created.
  /// </summary>
  [JsonPropertyName("ppCreatedTimestamp")]
  [Member(Index = 12, Type = MemberType.Timestamp)]
  public DateTime? PpCreatedTimestamp
  {
    get => ppCreatedTimestamp;
    set => ppCreatedTimestamp = value;
  }

  /// <summary>
  /// The value of the PP_EFF_DATE attribute.
  /// This date is used to show when the program is effective for a Cse Person.
  /// For example, a Cse Person may be placed on a Program in the middle of
  /// the month(ASSIGNED_DATE), however, the program was effective from the
  /// first day of that month(EFFECTIVE_BEGIN_DATE).
  /// </summary>
  [JsonPropertyName("ppEffDate")]
  [Member(Index = 13, Type = MemberType.Date)]
  public DateTime? PpEffDate
  {
    get => ppEffDate;
    set => ppEffDate = value;
  }

  /// <summary>
  /// The value of the PP_DISC_DATE attribute.
  /// This date is the date the program for a Cse Person is closed.
  /// </summary>
  [JsonPropertyName("ppDiscDate")]
  [Member(Index = 14, Type = MemberType.Date)]
  public DateTime? PpDiscDate
  {
    get => ppDiscDate;
    set => ppDiscDate = value;
  }

  /// <summary>Length of the P_CODE attribute.</summary>
  public const int Pcode_MaxLength = 3;

  /// <summary>
  /// The value of the P_CODE attribute.
  /// the code describing the program
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 15, Type = MemberType.Char, Length = Pcode_MaxLength)]
  public string Pcode
  {
    get => pcode ?? "";
    set => pcode = TrimEnd(Substring(value, 1, Pcode_MaxLength));
  }

  /// <summary>
  /// The json value of the Pcode attribute.</summary>
  [JsonPropertyName("pcode")]
  [Computed]
  public string Pcode_Json
  {
    get => NullIf(Pcode, "");
    set => Pcode = value;
  }

  /// <summary>
  /// The value of the P_EFF_DATE attribute.
  /// date program begins
  /// </summary>
  [JsonPropertyName("peffDate")]
  [Member(Index = 16, Type = MemberType.Date)]
  public DateTime? PeffDate
  {
    get => peffDate;
    set => peffDate = value;
  }

  /// <summary>
  /// The value of the P_DISC_DATE attribute.
  /// date program ends.
  /// </summary>
  [JsonPropertyName("pdiscDate")]
  [Member(Index = 17, Type = MemberType.Date)]
  public DateTime? PdiscDate
  {
    get => pdiscDate;
    set => pdiscDate = value;
  }

  /// <summary>
  /// The value of the LACR_CREATED_TSTAMP attribute.
  /// Legal Action Case Role created timestamp
  /// </summary>
  [JsonPropertyName("lacrCreatedTstamp")]
  [Member(Index = 18, Type = MemberType.Timestamp)]
  public DateTime? LacrCreatedTstamp
  {
    get => lacrCreatedTstamp;
    set => lacrCreatedTstamp = value;
  }

  /// <summary>
  /// The value of the LA_IDENTIFIER attribute.
  /// This is a system generated number to uniquely identify a legal action.
  /// </summary>
  [JsonPropertyName("laIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 19, Type = MemberType.Number, Length = 9)]
  public int LaIdentifier
  {
    get => laIdentifier;
    set => laIdentifier = value;
  }

  /// <summary>Length of the LA_ACTION_TAKEN attribute.</summary>
  public const int LaActionTaken_MaxLength = 30;

  /// <summary>
  /// The value of the LA_ACTION_TAKEN attribute.
  /// The specific type of classification action taken.
  /// Actions include:
  /// Attachment
  /// Support
  /// Paternity
  /// Interstate order
  /// Income Withholding
  /// Garnishment
  /// Order to Appear and Show Cause
  /// Notice of Intent
  /// Bond
  /// Lien
  /// Pay In Order
  /// Criminal Non-Support
  /// URESA
  /// Contempt
  /// Dormant Reviver
  /// Aid and Execution
  /// Judgement only
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 20, Type = MemberType.Char, Length = LaActionTaken_MaxLength)]
  public string LaActionTaken
  {
    get => laActionTaken ?? "";
    set => laActionTaken =
      TrimEnd(Substring(value, 1, LaActionTaken_MaxLength));
  }

  /// <summary>
  /// The json value of the LaActionTaken attribute.</summary>
  [JsonPropertyName("laActionTaken")]
  [Computed]
  public string LaActionTaken_Json
  {
    get => NullIf(LaActionTaken, "");
    set => LaActionTaken = value;
  }

  /// <summary>
  /// The value of the LA_CREATED_TSTAMP attribute.
  /// The date and time that the occurrence of the entity was created.
  /// </summary>
  [JsonPropertyName("laCreatedTstamp")]
  [Member(Index = 21, Type = MemberType.Timestamp)]
  public DateTime? LaCreatedTstamp
  {
    get => laCreatedTstamp;
    set => laCreatedTstamp = value;
  }

  /// <summary>
  /// The value of the LA_FILED_DATE attribute.
  /// The specific date that the court or administrative document was file 
  /// stamped and accepted.
  /// </summary>
  [JsonPropertyName("laFiledDate")]
  [Member(Index = 22, Type = MemberType.Date)]
  public DateTime? LaFiledDate
  {
    get => laFiledDate;
    set => laFiledDate = value;
  }

  /// <summary>
  /// The value of the LAD_NUMBER attribute.
  /// A number, unique within the legal action, used to identify each detail of 
  /// the Legal Action.  Starts with one and moves sequentially.
  /// </summary>
  [JsonPropertyName("ladNumber")]
  [DefaultValue(0)]
  [Member(Index = 23, Type = MemberType.Number, Length = 2)]
  public int LadNumber
  {
    get => ladNumber;
    set => ladNumber = value;
  }

  /// <summary>Length of the LAD_DETAIL_TYPE attribute.</summary>
  public const int LadDetailType_MaxLength = 1;

  /// <summary>
  /// The value of the LAD_DETAIL_TYPE attribute.
  /// This partitioning attribute is used to identity the type of detail, &quot;
  /// F&quot; for financial and &quot;N&quot; for non-financial.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 24, Type = MemberType.Char, Length = LadDetailType_MaxLength)]
  [Value("N")]
  [Value("F")]
  public string LadDetailType
  {
    get => ladDetailType ?? "";
    set => ladDetailType =
      TrimEnd(Substring(value, 1, LadDetailType_MaxLength));
  }

  /// <summary>
  /// The json value of the LadDetailType attribute.</summary>
  [JsonPropertyName("ladDetailType")]
  [Computed]
  public string LadDetailType_Json
  {
    get => NullIf(LadDetailType, "");
    set => LadDetailType = value;
  }

  /// <summary>
  /// The value of the LAD_CREATED_TSTAMP attribute.
  /// The date and time that the occurrance of the entity was created.
  /// </summary>
  [JsonPropertyName("ladCreatedTstamp")]
  [Member(Index = 25, Type = MemberType.Timestamp)]
  public DateTime? LadCreatedTstamp
  {
    get => ladCreatedTstamp;
    set => ladCreatedTstamp = value;
  }

  /// <summary>Length of the LAD_NON_FIN_OBLG_TYPE attribute.</summary>
  public const int LadNonFinOblgType_MaxLength = 4;

  /// <summary>
  /// The value of the LAD_NON_FIN_OBLG_TYPE attribute.
  /// This attribute specifies the type of non-financial obligation type 
  /// specified in the court order. The valid values are maintained in
  /// CODE_VALUE entity for code name LA DET NON FIN OBLG TYPE.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 26, Type = MemberType.Char, Length
    = LadNonFinOblgType_MaxLength)]
  public string LadNonFinOblgType
  {
    get => ladNonFinOblgType ?? "";
    set => ladNonFinOblgType =
      TrimEnd(Substring(value, 1, LadNonFinOblgType_MaxLength));
  }

  /// <summary>
  /// The json value of the LadNonFinOblgType attribute.</summary>
  [JsonPropertyName("ladNonFinOblgType")]
  [Computed]
  public string LadNonFinOblgType_Json
  {
    get => NullIf(LadNonFinOblgType, "");
    set => LadNonFinOblgType = value;
  }

  /// <summary>Length of the OT_CODE attribute.</summary>
  public const int OtCode_MaxLength = 7;

  /// <summary>
  /// The value of the OT_CODE attribute.
  /// A short representation of the name for the purpose of quick 
  /// identification.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 27, Type = MemberType.Char, Length = OtCode_MaxLength)]
  public string OtCode
  {
    get => otCode ?? "";
    set => otCode = TrimEnd(Substring(value, 1, OtCode_MaxLength));
  }

  /// <summary>
  /// The json value of the OtCode attribute.</summary>
  [JsonPropertyName("otCode")]
  [Computed]
  public string OtCode_Json
  {
    get => NullIf(OtCode, "");
    set => OtCode = value;
  }

  /// <summary>Length of the OT_CLASSIFICATION attribute.</summary>
  public const int OtClassification_MaxLength = 1;

  /// <summary>
  /// The value of the OT_CLASSIFICATION attribute.
  /// Defines a class of debt type to aid in determining how processes will 
  /// enteract with the various debt types.
  /// Examples:
  ///   A - Accrual Instructions
  ///   S - Support Debts (Result of Accrual)
  ///   R - Recovery Debts
  ///   M - Medical Debts
  ///   O - Other Valid Debts
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 28, Type = MemberType.Char, Length
    = OtClassification_MaxLength)]
  [Value("F")]
  [Value("V")]
  [Value("N")]
  [Value("H")]
  [Value("O")]
  [Value("A")]
  [Value("R")]
  [Value("M")]
  [Value("S")]
  [ImplicitValue("S")]
  public string OtClassification
  {
    get => otClassification ?? "";
    set => otClassification =
      TrimEnd(Substring(value, 1, OtClassification_MaxLength));
  }

  /// <summary>
  /// The json value of the OtClassification attribute.</summary>
  [JsonPropertyName("otClassification")]
  [Computed]
  public string OtClassification_Json
  {
    get => NullIf(OtClassification, "");
    set => OtClassification = value;
  }

  /// <summary>
  /// The value of the DD_DUE_DT attribute.
  /// The date the payment is due and payable for that occurence of SET AMOUNT 
  /// DEBT.  Set amount debts not collected by this date will be considered past
  /// due.
  /// </summary>
  [JsonPropertyName("ddDueDt")]
  [Member(Index = 29, Type = MemberType.Date)]
  public DateTime? DdDueDt
  {
    get => ddDueDt;
    set => ddDueDt = value;
  }

  /// <summary>
  /// The value of the DD_BALANCE_DUE_AMT attribute.
  /// The amount remaining after all collections and adjustments have been 
  /// applied.  When the Balance Due amount is zero, the debt is retired.
  /// </summary>
  [JsonPropertyName("ddBalanceDueAmt")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 30, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal DdBalanceDueAmt
  {
    get => ddBalanceDueAmt;
    set => ddBalanceDueAmt = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the DD_INTEREST_BALANCE_DUE_AMT attribute.
  /// Represents the total interest balance due for this specific debt.
  /// </summary>
  [JsonPropertyName("ddInterestBalanceDueAmt")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 31, Type = MemberType.Number, Length = 6, Precision = 2)]
  public decimal DdInterestBalanceDueAmt
  {
    get => ddInterestBalanceDueAmt;
    set => ddInterestBalanceDueAmt = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the DD_CREATED_TMST attribute.
  /// The clock time the entity type was created.
  /// </summary>
  [JsonPropertyName("ddCreatedTmst")]
  [Member(Index = 32, Type = MemberType.Timestamp)]
  public DateTime? DdCreatedTmst
  {
    get => ddCreatedTmst;
    set => ddCreatedTmst = value;
  }

  /// <summary>Length of the CPA_TYPE attribute.</summary>
  public const int CpaType_MaxLength = 1;

  /// <summary>
  /// The value of the CPA_TYPE attribute.
  /// Defines the type of account the entity type represents.
  /// Examples:
  /// 	- Obligor
  /// 	- Obligee
  /// 	- Supported Person
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 33, Type = MemberType.Char, Length = CpaType_MaxLength)]
  [Value("S")]
  [Value("R")]
  [Value("E")]
  [ImplicitValue("S")]
  public string CpaType
  {
    get => cpaType ?? "";
    set => cpaType = TrimEnd(Substring(value, 1, CpaType_MaxLength));
  }

  /// <summary>
  /// The json value of the CpaType attribute.</summary>
  [JsonPropertyName("cpaType")]
  [Computed]
  public string CpaType_Json
  {
    get => NullIf(CpaType, "");
    set => CpaType = value;
  }

  /// <summary>
  /// The value of the CPA_CREATED_TMST attribute.
  /// The date and time that the occurrance of the entity was created.
  /// </summary>
  [JsonPropertyName("cpaCreatedTmst")]
  [Member(Index = 34, Type = MemberType.Timestamp)]
  public DateTime? CpaCreatedTmst
  {
    get => cpaCreatedTmst;
    set => cpaCreatedTmst = value;
  }

  /// <summary>
  /// The value of the OB_TRAN_IDENTIFIER attribute.
  /// A unique, system generated random number to provide a unique identifier 
  /// for each transaction.
  /// </summary>
  [JsonPropertyName("obTranIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 35, Type = MemberType.Number, Length = 9)]
  public int ObTranIdentifier
  {
    get => obTranIdentifier;
    set => obTranIdentifier = value;
  }

  /// <summary>Length of the OB_TRAN_TYPE attribute.</summary>
  public const int ObTranType_MaxLength = 2;

  /// <summary>
  /// The value of the OB_TRAN_TYPE attribute.
  /// Defines the obligation transaction type.
  /// Permitted Values:
  /// 	- DE = DEBT
  /// 	- DA = DEBT ADJUSTMENT
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 36, Type = MemberType.Char, Length = ObTranType_MaxLength)]
  [Value("DA")]
  [Value("Z2")]
  [Value("DE")]
  [ImplicitValue("DE")]
  public string ObTranType
  {
    get => obTranType ?? "";
    set => obTranType = TrimEnd(Substring(value, 1, ObTranType_MaxLength));
  }

  /// <summary>
  /// The json value of the ObTranType attribute.</summary>
  [JsonPropertyName("obTranType")]
  [Computed]
  public string ObTranType_Json
  {
    get => NullIf(ObTranType, "");
    set => ObTranType = value;
  }

  /// <summary>
  /// The value of the OB_TRAN_CREATED_TMST attribute.
  /// The timestamp the occurrence was created.
  /// </summary>
  [JsonPropertyName("obTranCreatedTmst")]
  [Member(Index = 37, Type = MemberType.Timestamp)]
  public DateTime? ObTranCreatedTmst
  {
    get => obTranCreatedTmst;
    set => obTranCreatedTmst = value;
  }

  /// <summary>
  /// The value of the OB_IDENTIFIER attribute.
  /// A unique, system generated sequential number (sort descending) that, along
  /// with CSE Person, distinguishes one occurrence of the entity type from
  /// another.
  /// </summary>
  [JsonPropertyName("obIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 38, Type = MemberType.Number, Length = 3)]
  public int ObIdentifier
  {
    get => obIdentifier;
    set => obIdentifier = value;
  }

  /// <summary>
  /// The value of the OB_CREATED_TMST attribute.
  /// The timestamp the occurrence was created.
  /// </summary>
  [JsonPropertyName("obCreatedTmst")]
  [Member(Index = 39, Type = MemberType.Timestamp)]
  public DateTime? ObCreatedTmst
  {
    get => obCreatedTmst;
    set => obCreatedTmst = value;
  }

  /// <summary>
  /// The value of the COLL_IDENTIFIER attribute.
  /// A unique, system generated random number to provide a unique identifier 
  /// for each collection.
  /// </summary>
  [JsonPropertyName("collIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 40, Type = MemberType.Number, Length = 9)]
  public int CollIdentifier
  {
    get => collIdentifier;
    set => collIdentifier = value;
  }

  /// <summary>Length of the COLL_APPLIED_TO_CODE attribute.</summary>
  public const int CollAppliedToCode_MaxLength = 1;

  /// <summary>
  /// The value of the COLL_APPLIED_TO_CODE attribute.
  /// Defines whether the collection was applied to CURRENT, ARREARS, GIFT or 
  /// INTEREST.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 41, Type = MemberType.Char, Length
    = CollAppliedToCode_MaxLength)]
  [Value("G")]
  [Value("A")]
  [Value("I")]
  [Value("C")]
  public string CollAppliedToCode
  {
    get => collAppliedToCode ?? "";
    set => collAppliedToCode =
      TrimEnd(Substring(value, 1, CollAppliedToCode_MaxLength));
  }

  /// <summary>
  /// The json value of the CollAppliedToCode attribute.</summary>
  [JsonPropertyName("collAppliedToCode")]
  [Computed]
  public string CollAppliedToCode_Json
  {
    get => NullIf(CollAppliedToCode, "");
    set => CollAppliedToCode = value;
  }

  /// <summary>Length of the COLL_ADJUSTED_IND attribute.</summary>
  public const int CollAdjustedInd_MaxLength = 1;

  /// <summary>
  /// The value of the COLL_ADJUSTED_IND attribute.
  /// Indicate whether or not the collection has been adjusted.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 42, Type = MemberType.Char, Length = CollAdjustedInd_MaxLength)
    ]
  [Value("Y")]
  [Value("N")]
  [ImplicitValue("N")]
  public string CollAdjustedInd
  {
    get => collAdjustedInd ?? "";
    set => collAdjustedInd =
      TrimEnd(Substring(value, 1, CollAdjustedInd_MaxLength));
  }

  /// <summary>
  /// The json value of the CollAdjustedInd attribute.</summary>
  [JsonPropertyName("collAdjustedInd")]
  [Computed]
  public string CollAdjustedInd_Json
  {
    get => NullIf(CollAdjustedInd, "");
    set => CollAdjustedInd = value;
  }

  /// <summary>Length of the COLL_CONCURRENT_IND attribute.</summary>
  public const int CollConcurrentInd_MaxLength = 1;

  /// <summary>
  /// The value of the COLL_CONCURRENT_IND attribute.
  /// Identifies whether a specific collection is a for a concurrent obligation.
  /// If the collection represents a concurrent collection, then the funding
  /// and disbursement processes iqnore the collection.  Both the reqular and
  /// concurrent collections will be tied to the same cash receipt detail.
  /// Permitted Values:
  ///   Y - Yes
  ///   N - No
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 43, Type = MemberType.Char, Length
    = CollConcurrentInd_MaxLength)]
  [Value("N")]
  [Value("Y")]
  [ImplicitValue("N")]
  public string CollConcurrentInd
  {
    get => collConcurrentInd ?? "";
    set => collConcurrentInd =
      TrimEnd(Substring(value, 1, CollConcurrentInd_MaxLength));
  }

  /// <summary>
  /// The json value of the CollConcurrentInd attribute.</summary>
  [JsonPropertyName("collConcurrentInd")]
  [Computed]
  public string CollConcurrentInd_Json
  {
    get => NullIf(CollConcurrentInd, "");
    set => CollConcurrentInd = value;
  }

  /// <summary>
  /// The value of the COLL_CREATED_TMST attribute.
  /// The timestamp the occurrence was created.
  /// </summary>
  [JsonPropertyName("collCreatedTmst")]
  [Member(Index = 44, Type = MemberType.Timestamp)]
  public DateTime? CollCreatedTmst
  {
    get => collCreatedTmst;
    set => collCreatedTmst = value;
  }

  private string detailsForLine;
  private string cnumber;
  private string cstatus;
  private DateTime? cstatusDate;
  private DateTime? copenDate;
  private int crIdentifier;
  private string crType;
  private DateTime? crStartDate;
  private DateTime? crEndDate;
  private string cpType;
  private string cpNumber;
  private DateTime? ppCreatedTimestamp;
  private DateTime? ppEffDate;
  private DateTime? ppDiscDate;
  private string pcode;
  private DateTime? peffDate;
  private DateTime? pdiscDate;
  private DateTime? lacrCreatedTstamp;
  private int laIdentifier;
  private string laActionTaken;
  private DateTime? laCreatedTstamp;
  private DateTime? laFiledDate;
  private int ladNumber;
  private string ladDetailType;
  private DateTime? ladCreatedTstamp;
  private string ladNonFinOblgType;
  private string otCode;
  private string otClassification;
  private DateTime? ddDueDt;
  private decimal ddBalanceDueAmt;
  private decimal ddInterestBalanceDueAmt;
  private DateTime? ddCreatedTmst;
  private string cpaType;
  private DateTime? cpaCreatedTmst;
  private int obTranIdentifier;
  private string obTranType;
  private DateTime? obTranCreatedTmst;
  private int obIdentifier;
  private DateTime? obCreatedTmst;
  private int collIdentifier;
  private string collAppliedToCode;
  private string collAdjustedInd;
  private string collConcurrentInd;
  private DateTime? collCreatedTmst;
}
