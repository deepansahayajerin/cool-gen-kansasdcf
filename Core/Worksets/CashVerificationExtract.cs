// The source file: CASH_VERIFICATION_EXTRACT, ID: 372939645, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class CashVerificationExtract: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public CashVerificationExtract()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public CashVerificationExtract(CashVerificationExtract that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new CashVerificationExtract Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(CashVerificationExtract that)
  {
    base.Assign(that);
    detailsForLine = that.detailsForLine;
    ddDueDt = that.ddDueDt;
    ddCreatedTmst = that.ddCreatedTmst;
    ddCreatedBy = that.ddCreatedBy;
    obTranIdentifier = that.obTranIdentifier;
    obTranType = that.obTranType;
    obTranAmount = that.obTranAmount;
    obTranCreatedBy = that.obTranCreatedBy;
    obTranCreatedTmst = that.obTranCreatedTmst;
    obTranDebtAdjustmentDt = that.obTranDebtAdjustmentDt;
    obTranDebtAdjustmentType = that.obTranDebtAdjustmentType;
    obTypeCode = that.obTypeCode;
    obTypeClassification = that.obTypeClassification;
    cpaType = that.cpaType;
    cpaCreatedTmst = that.cpaCreatedTmst;
    cpType = that.cpType;
    cpCreatedTimestamp = that.cpCreatedTimestamp;
    cpNumber = that.cpNumber;
    ppCreatedTimestamp = that.ppCreatedTimestamp;
    ppEffDate = that.ppEffDate;
    ppDiscDate = that.ppDiscDate;
    pcode = that.pcode;
    peffDate = that.peffDate;
    pdiscDate = that.pdiscDate;
    otrIdentifier = that.otrIdentifier;
    otrCreatedTmst = that.otrCreatedTmst;
    collProgramAppliedTo = that.collProgramAppliedTo;
    collIdentifier = that.collIdentifier;
    collAmount = that.collAmount;
    collAppliedToCode = that.collAppliedToCode;
    collAdjustedInd = that.collAdjustedInd;
    collConcurrentInd = that.collConcurrentInd;
    collCreatedTmst = that.collCreatedTmst;
    crtIdentifier = that.crtIdentifier;
    obIdentifier = that.obIdentifier;
    obCreatedTmst = that.obCreatedTmst;
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

  /// <summary>
  /// The value of the DD_DUE_DT attribute.
  /// The date the payment is due and payable for that occurence of SET AMOUNT 
  /// DEBT.  Set amount debts not collected by this date will be considered past
  /// due.
  /// </summary>
  [JsonPropertyName("ddDueDt")]
  [Member(Index = 2, Type = MemberType.Date)]
  public DateTime? DdDueDt
  {
    get => ddDueDt;
    set => ddDueDt = value;
  }

  /// <summary>
  /// The value of the DD_CREATED_TMST attribute.
  /// The clock time the entity type was created.
  /// </summary>
  [JsonPropertyName("ddCreatedTmst")]
  [Member(Index = 3, Type = MemberType.Timestamp)]
  public DateTime? DdCreatedTmst
  {
    get => ddCreatedTmst;
    set => ddCreatedTmst = value;
  }

  /// <summary>Length of the DD_CREATED_BY attribute.</summary>
  public const int DdCreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the DD_CREATED_BY attribute.
  /// Logon ID of the user that executed the CICS transaction that created the 
  /// entity; or, the name of the batch job that created the entity.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = DdCreatedBy_MaxLength)]
  public string DdCreatedBy
  {
    get => ddCreatedBy ?? "";
    set => ddCreatedBy = TrimEnd(Substring(value, 1, DdCreatedBy_MaxLength));
  }

  /// <summary>
  /// The json value of the DdCreatedBy attribute.</summary>
  [JsonPropertyName("ddCreatedBy")]
  [Computed]
  public string DdCreatedBy_Json
  {
    get => NullIf(DdCreatedBy, "");
    set => DdCreatedBy = value;
  }

  /// <summary>
  /// The value of the OB_TRAN_IDENTIFIER attribute.
  /// A unique, system generated random number to provide a unique identifier 
  /// for each transaction.
  /// </summary>
  [JsonPropertyName("obTranIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 5, Type = MemberType.Number, Length = 9)]
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
  [Member(Index = 6, Type = MemberType.Char, Length = ObTranType_MaxLength)]
  [Value("DE")]
  [Value("Z2")]
  [Value("DA")]
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
  /// The value of the OB_TRAN_AMOUNT attribute.
  /// Amount of the obligation transaction.
  /// Examples: Amount of a debt or debt adjustment.
  /// </summary>
  [JsonPropertyName("obTranAmount")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 7, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal ObTranAmount
  {
    get => obTranAmount;
    set => obTranAmount = Truncate(value, 2);
  }

  /// <summary>Length of the OB_TRAN_CREATED_BY attribute.</summary>
  public const int ObTranCreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the OB_TRAN_CREATED_BY attribute.
  /// The User ID or Program ID responsible for the creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = ObTranCreatedBy_MaxLength)]
    
  public string ObTranCreatedBy
  {
    get => obTranCreatedBy ?? "";
    set => obTranCreatedBy =
      TrimEnd(Substring(value, 1, ObTranCreatedBy_MaxLength));
  }

  /// <summary>
  /// The json value of the ObTranCreatedBy attribute.</summary>
  [JsonPropertyName("obTranCreatedBy")]
  [Computed]
  public string ObTranCreatedBy_Json
  {
    get => NullIf(ObTranCreatedBy, "");
    set => ObTranCreatedBy = value;
  }

  /// <summary>
  /// The value of the OB_TRAN_CREATED_TMST attribute.
  /// The timestamp the occurrence was created.
  /// </summary>
  [JsonPropertyName("obTranCreatedTmst")]
  [Member(Index = 9, Type = MemberType.Timestamp)]
  public DateTime? ObTranCreatedTmst
  {
    get => obTranCreatedTmst;
    set => obTranCreatedTmst = value;
  }

  /// <summary>
  /// The value of the OB_TRAN_DEBT_ADJUSTMENT_DT attribute.
  /// The date the Debt Adjustment was applied to a specific Debt.
  /// </summary>
  [JsonPropertyName("obTranDebtAdjustmentDt")]
  [Member(Index = 10, Type = MemberType.Date)]
  public DateTime? ObTranDebtAdjustmentDt
  {
    get => obTranDebtAdjustmentDt;
    set => obTranDebtAdjustmentDt = value;
  }

  /// <summary>Length of the OB_TRAN_DEBT_ADJUSTMENT_TYPE attribute.</summary>
  public const int ObTranDebtAdjustmentType_MaxLength = 1;

  /// <summary>
  /// The value of the OB_TRAN_DEBT_ADJUSTMENT_TYPE attribute.
  /// Determines whether the Debt Adjustment increases or decreases the debt.
  ///   - I : Increase
  ///   - D : Decrease
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length
    = ObTranDebtAdjustmentType_MaxLength)]
  [Value("D")]
  [Value("I")]
  [ImplicitValue("I")]
  public string ObTranDebtAdjustmentType
  {
    get => obTranDebtAdjustmentType ?? "";
    set => obTranDebtAdjustmentType =
      TrimEnd(Substring(value, 1, ObTranDebtAdjustmentType_MaxLength));
  }

  /// <summary>
  /// The json value of the ObTranDebtAdjustmentType attribute.</summary>
  [JsonPropertyName("obTranDebtAdjustmentType")]
  [Computed]
  public string ObTranDebtAdjustmentType_Json
  {
    get => NullIf(ObTranDebtAdjustmentType, "");
    set => ObTranDebtAdjustmentType = value;
  }

  /// <summary>Length of the OB_TYPE_CODE attribute.</summary>
  public const int ObTypeCode_MaxLength = 7;

  /// <summary>
  /// The value of the OB_TYPE_CODE attribute.
  /// A short representation of the name for the purpose of quick 
  /// identification.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length = ObTypeCode_MaxLength)]
  public string ObTypeCode
  {
    get => obTypeCode ?? "";
    set => obTypeCode = TrimEnd(Substring(value, 1, ObTypeCode_MaxLength));
  }

  /// <summary>
  /// The json value of the ObTypeCode attribute.</summary>
  [JsonPropertyName("obTypeCode")]
  [Computed]
  public string ObTypeCode_Json
  {
    get => NullIf(ObTypeCode, "");
    set => ObTypeCode = value;
  }

  /// <summary>Length of the OB_TYPE_CLASSIFICATION attribute.</summary>
  public const int ObTypeClassification_MaxLength = 1;

  /// <summary>
  /// The value of the OB_TYPE_CLASSIFICATION attribute.
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
  [Member(Index = 13, Type = MemberType.Char, Length
    = ObTypeClassification_MaxLength)]
  [Value("N")]
  [Value("V")]
  [Value("H")]
  [Value("F")]
  [Value("O")]
  [Value("A")]
  [Value("R")]
  [Value("M")]
  [Value("S")]
  [ImplicitValue("S")]
  public string ObTypeClassification
  {
    get => obTypeClassification ?? "";
    set => obTypeClassification =
      TrimEnd(Substring(value, 1, ObTypeClassification_MaxLength));
  }

  /// <summary>
  /// The json value of the ObTypeClassification attribute.</summary>
  [JsonPropertyName("obTypeClassification")]
  [Computed]
  public string ObTypeClassification_Json
  {
    get => NullIf(ObTypeClassification, "");
    set => ObTypeClassification = value;
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
  [Member(Index = 14, Type = MemberType.Char, Length = CpaType_MaxLength)]
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
  [Member(Index = 15, Type = MemberType.Timestamp)]
  public DateTime? CpaCreatedTmst
  {
    get => cpaCreatedTmst;
    set => cpaCreatedTmst = value;
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
  [Member(Index = 16, Type = MemberType.Char, Length = CpType_MaxLength)]
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

  /// <summary>
  /// The value of the CP_CREATED_TIMESTAMP attribute.
  /// Timestamp of creation of the occurrence.
  /// 	
  /// </summary>
  [JsonPropertyName("cpCreatedTimestamp")]
  [Member(Index = 17, Type = MemberType.Timestamp)]
  public DateTime? CpCreatedTimestamp
  {
    get => cpCreatedTimestamp;
    set => cpCreatedTimestamp = value;
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
  [Member(Index = 18, Type = MemberType.Char, Length = CpNumber_MaxLength)]
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
  [Member(Index = 19, Type = MemberType.Timestamp)]
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
  [Member(Index = 20, Type = MemberType.Date)]
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
  [Member(Index = 21, Type = MemberType.Date)]
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
  [Member(Index = 22, Type = MemberType.Char, Length = Pcode_MaxLength)]
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
  [Member(Index = 23, Type = MemberType.Date)]
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
  [Member(Index = 24, Type = MemberType.Date)]
  public DateTime? PdiscDate
  {
    get => pdiscDate;
    set => pdiscDate = value;
  }

  /// <summary>
  /// The value of the OTR_IDENTIFIER attribute.
  /// A unique, system generated random number that distinguishes one occurrence
  /// of the entity type from another.
  /// </summary>
  [JsonPropertyName("otrIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 25, Type = MemberType.Number, Length = 9)]
  public int OtrIdentifier
  {
    get => otrIdentifier;
    set => otrIdentifier = value;
  }

  /// <summary>
  /// The value of the OTR_CREATED_TMST attribute.
  /// The timestamp of when the occurrence of the entity was created.
  /// </summary>
  [JsonPropertyName("otrCreatedTmst")]
  [Member(Index = 26, Type = MemberType.Timestamp)]
  public DateTime? OtrCreatedTmst
  {
    get => otrCreatedTmst;
    set => otrCreatedTmst = value;
  }

  /// <summary>Length of the COLL_PROGRAM_APPLIED_TO attribute.</summary>
  public const int CollProgramAppliedTo_MaxLength = 3;

  /// <summary>
  /// The value of the COLL_PROGRAM_APPLIED_TO attribute.
  /// The distribution program type for the active program for the supported 
  /// person at the time the collection was distributed.
  /// Values are AF,FC,NA,NF or spaces.
  /// Distribution program types AF and FC are considered to be ADC collections 
  /// where the state retains the money; whereas, NA and NF are NADC collections
  /// where the money is distributed to the applicant recipient.
  /// Per. Beth Burrel 12/23/96
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 27, Type = MemberType.Char, Length
    = CollProgramAppliedTo_MaxLength)]
  [Value("")]
  [Value("NF")]
  [Value("NA")]
  [Value("NC")]
  [Value("NAI")]
  [Value("FCI")]
  [Value("AFI")]
  [Value("AF")]
  [Value("FC")]
  public string CollProgramAppliedTo
  {
    get => collProgramAppliedTo ?? "";
    set => collProgramAppliedTo =
      TrimEnd(Substring(value, 1, CollProgramAppliedTo_MaxLength));
  }

  /// <summary>
  /// The json value of the CollProgramAppliedTo attribute.</summary>
  [JsonPropertyName("collProgramAppliedTo")]
  [Computed]
  public string CollProgramAppliedTo_Json
  {
    get => NullIf(CollProgramAppliedTo, "");
    set => CollProgramAppliedTo = value;
  }

  /// <summary>
  /// The value of the COLL_IDENTIFIER attribute.
  /// A unique, system generated random number to provide a unique identifier 
  /// for each collection.
  /// </summary>
  [JsonPropertyName("collIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 28, Type = MemberType.Number, Length = 9)]
  public int CollIdentifier
  {
    get => collIdentifier;
    set => collIdentifier = value;
  }

  /// <summary>
  /// The value of the COLL_AMOUNT attribute.
  /// Amount of the Cash Receipt Detail that was applied to the Debt as a 
  /// COLLECTION.
  /// </summary>
  [JsonPropertyName("collAmount")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 29, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal CollAmount
  {
    get => collAmount;
    set => collAmount = Truncate(value, 2);
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
  [Member(Index = 30, Type = MemberType.Char, Length
    = CollAppliedToCode_MaxLength)]
  [Value("C")]
  [Value("I")]
  [Value("A")]
  [Value("G")]
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
  [Member(Index = 31, Type = MemberType.Char, Length = CollAdjustedInd_MaxLength)
    ]
  [Value("N")]
  [Value("Y")]
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
  [Member(Index = 32, Type = MemberType.Char, Length
    = CollConcurrentInd_MaxLength)]
  [Value("Y")]
  [Value("N")]
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
  [Member(Index = 33, Type = MemberType.Timestamp)]
  public DateTime? CollCreatedTmst
  {
    get => collCreatedTmst;
    set => collCreatedTmst = value;
  }

  /// <summary>
  /// The value of the CRT_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("crtIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 34, Type = MemberType.Number, Length = 3)]
  public int CrtIdentifier
  {
    get => crtIdentifier;
    set => crtIdentifier = value;
  }

  /// <summary>
  /// The value of the OB_IDENTIFIER attribute.
  /// A unique, system generated sequential number (sort descending) that, along
  /// with CSE Person, distinguishes one occurrence of the entity type from
  /// another.
  /// </summary>
  [JsonPropertyName("obIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 35, Type = MemberType.Number, Length = 3)]
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
  [Member(Index = 36, Type = MemberType.Timestamp)]
  public DateTime? ObCreatedTmst
  {
    get => obCreatedTmst;
    set => obCreatedTmst = value;
  }

  private string detailsForLine;
  private DateTime? ddDueDt;
  private DateTime? ddCreatedTmst;
  private string ddCreatedBy;
  private int obTranIdentifier;
  private string obTranType;
  private decimal obTranAmount;
  private string obTranCreatedBy;
  private DateTime? obTranCreatedTmst;
  private DateTime? obTranDebtAdjustmentDt;
  private string obTranDebtAdjustmentType;
  private string obTypeCode;
  private string obTypeClassification;
  private string cpaType;
  private DateTime? cpaCreatedTmst;
  private string cpType;
  private DateTime? cpCreatedTimestamp;
  private string cpNumber;
  private DateTime? ppCreatedTimestamp;
  private DateTime? ppEffDate;
  private DateTime? ppDiscDate;
  private string pcode;
  private DateTime? peffDate;
  private DateTime? pdiscDate;
  private int otrIdentifier;
  private DateTime? otrCreatedTmst;
  private string collProgramAppliedTo;
  private int collIdentifier;
  private decimal collAmount;
  private string collAppliedToCode;
  private string collAdjustedInd;
  private string collConcurrentInd;
  private DateTime? collCreatedTmst;
  private int crtIdentifier;
  private int obIdentifier;
  private DateTime? obCreatedTmst;
}
