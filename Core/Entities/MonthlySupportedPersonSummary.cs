// The source file: MONTHLY_SUPPORTED_PERSON_SUMMARY, ID: 371437642, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// The purpose of this entity is to store monthly summary information regarding
/// Supported Persons and Obligations.  A separate occurrence will be stored
/// for each year/month and CSE Person Account Supported and Obligation.
/// </summary>
[Serializable]
public partial class MonthlySupportedPersonSummary: Heap, ICloneable
{
  /// <summary>Default constructor.</summary>
  public MonthlySupportedPersonSummary()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public MonthlySupportedPersonSummary(MonthlySupportedPersonSummary that):
    base(that)
  {
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new MonthlySupportedPersonSummary Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>
  /// The value of the TILL_MTH_GIFT_COLL attribute.
  /// Total gift collection as at end of the month.
  /// </summary>
  [JsonPropertyName("tillMthGiftColl")]
  [Member(Index = 1, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TillMthGiftColl
  {
    get => Get<decimal?>("tillMthGiftColl");
    set => Set("tillMthGiftColl", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_INDENTIFIER attribute.
  /// A unique system generated random number that uniquely identifies each 
  /// occurrence of this entity type.
  /// </summary>
  [JsonPropertyName("systemGeneratedIndentifier")]
  [DefaultValue(0)]
  [Member(Index = 2, Type = MemberType.Number, Length = 9)]
  public int SystemGeneratedIndentifier
  {
    get => Get<int?>("systemGeneratedIndentifier") ?? 0;
    set => Set("systemGeneratedIndentifier", value == 0 ? null : value as int?);
  }

  /// <summary>
  /// The value of the YEAR_MONTH attribute.
  /// Defines the year and month in which this entity occurrence applies.
  /// </summary>
  [JsonPropertyName("yearMonth")]
  [DefaultValue(0)]
  [Member(Index = 3, Type = MemberType.Number, Length = 6)]
  public int YearMonth
  {
    get => Get<int?>("yearMonth") ?? 0;
    set => Set("yearMonth", value == 0 ? null : value as int?);
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int Type1_MaxLength = 1;

  /// <summary>
  /// The value of the TYPE attribute.
  /// Defines whether this entity occurrence is summary information of a 
  /// Supported Person's Account or Obligations.
  /// 	Examples: S = Supported Person
  /// 		  O = Obligation
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = Type1_MaxLength)]
  [Value("A")]
  [Value("O")]
  [ImplicitValue("A")]
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
  /// The value of the FOR_MTH_CS_CURR_BAL attribute.
  /// Total Child Support balance for Current for the month
  /// </summary>
  [JsonPropertyName("forMthCsCurrBal")]
  [Member(Index = 5, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? ForMthCsCurrBal
  {
    get => Get<decimal?>("forMthCsCurrBal");
    set => Set("forMthCsCurrBal", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the FOR_MTH_SP_CURR_BAL attribute.
  /// Total spousal support balance for current due for the month
  /// </summary>
  [JsonPropertyName("forMthSpCurrBal")]
  [Member(Index = 6, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? ForMthSpCurrBal
  {
    get => Get<decimal?>("forMthSpCurrBal");
    set => Set("forMthSpCurrBal", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the FOR_MTH_MS_CURR_BAL attribute.
  /// Total medical balance for current due for the month
  /// </summary>
  [JsonPropertyName("forMthMsCurrBal")]
  [Member(Index = 7, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? ForMthMsCurrBal
  {
    get => Get<decimal?>("forMthMsCurrBal");
    set => Set("forMthMsCurrBal", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the FOR_MTH_NAD_ARR_PRD_BAL attribute.
  /// NADC arrears periodic payment balance for the month.
  /// </summary>
  [JsonPropertyName("forMthNadArrPrdBal")]
  [Member(Index = 8, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? ForMthNadArrPrdBal
  {
    get => Get<decimal?>("forMthNadArrPrdBal");
    set => Set("forMthNadArrPrdBal", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the FOR_MTH_ADC_ARR_PRD_BAL attribute.
  /// ADC arrears periodic payment balance for the month.
  /// </summary>
  [JsonPropertyName("forMthAdcArrPrdBal")]
  [Member(Index = 9, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? ForMthAdcArrPrdBal
  {
    get => Get<decimal?>("forMthAdcArrPrdBal");
    set => Set("forMthAdcArrPrdBal", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the FOR_MTH_CURR_BALL attribute.
  /// Total balance for current for the month.
  /// </summary>
  [JsonPropertyName("forMthCurrBall")]
  [Member(Index = 10, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? ForMthCurrBall
  {
    get => Get<decimal?>("forMthCurrBall");
    set => Set("forMthCurrBall", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the FOR_MTH_CS_COLL_CURR_ARR attribute.
  /// Total child collection for the month applied to current and arrears.
  /// </summary>
  [JsonPropertyName("forMthCsCollCurrArr")]
  [Member(Index = 11, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? ForMthCsCollCurrArr
  {
    get => Get<decimal?>("forMthCsCollCurrArr");
    set => Set("forMthCsCollCurrArr", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the FOR_MTH_SP_COLL_CURR_ARR attribute.
  /// Total spousal support collection for the month applied to current and 
  /// arrears.
  /// </summary>
  [JsonPropertyName("forMthSpCollCurrArr")]
  [Member(Index = 12, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? ForMthSpCollCurrArr
  {
    get => Get<decimal?>("forMthSpCollCurrArr");
    set => Set("forMthSpCollCurrArr", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the FOR_MTH_MS_COLL_CURR_ARR attribute.
  /// Total medical support collection for the month applied to current and 
  /// arrears.
  /// </summary>
  [JsonPropertyName("forMthMsCollCurrArr")]
  [Member(Index = 13, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? ForMthMsCollCurrArr
  {
    get => Get<decimal?>("forMthMsCollCurrArr");
    set => Set("forMthMsCollCurrArr", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TILL_MTH_NAD_ARR_COLL attribute.
  /// NADC arrears collection till end of the month.
  /// </summary>
  [JsonPropertyName("tillMthNadArrColl")]
  [Member(Index = 14, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TillMthNadArrColl
  {
    get => Get<decimal?>("tillMthNadArrColl");
    set => Set("tillMthNadArrColl", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TILL_MTH_NAD_INT_COLL attribute.
  /// NADC interest collection till the end of the month
  /// </summary>
  [JsonPropertyName("tillMthNadIntColl")]
  [Member(Index = 15, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TillMthNadIntColl
  {
    get => Get<decimal?>("tillMthNadIntColl");
    set => Set("tillMthNadIntColl", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TILL_MTH_ADC_ARR_COLL attribute.
  /// ADC arrears collection till the end of the month.
  /// </summary>
  [JsonPropertyName("tillMthAdcArrColl")]
  [Member(Index = 16, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TillMthAdcArrColl
  {
    get => Get<decimal?>("tillMthAdcArrColl");
    set => Set("tillMthAdcArrColl", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TILL_MTH_ADC_INT_COLL attribute.
  /// ADC interest collection till the end of the month
  /// </summary>
  [JsonPropertyName("tillMthAdcIntColl")]
  [Member(Index = 17, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TillMthAdcIntColl
  {
    get => Get<decimal?>("tillMthAdcIntColl");
    set => Set("tillMthAdcIntColl", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the FOR_MTH_FUTURE_COLL attribute.
  /// Total future collection during the month.
  /// </summary>
  [JsonPropertyName("forMthFutureColl")]
  [Member(Index = 18, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? ForMthFutureColl
  {
    get => Get<decimal?>("forMthFutureColl");
    set => Set("forMthFutureColl", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TILL_MTH_COLL_ALL attribute.
  /// Total collections as at end of the month applied to all.
  /// </summary>
  [JsonPropertyName("tillMthCollAll")]
  [Member(Index = 19, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TillMthCollAll
  {
    get => Get<decimal?>("tillMthCollAll");
    set => Set("tillMthCollAll", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the GRANT_AMT attribute.
  /// Total amount of the grant for a specific month.
  /// </summary>
  [JsonPropertyName("grantAmt")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 20, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal GrantAmt
  {
    get => Get<decimal?>("grantAmt") ?? 0M;
    set => Set("grantAmt", value == 0M ? null : Truncate(value, 2) as decimal?);
  }

  /// <summary>
  /// The value of the MEDICAL_GRANT_AMT attribute.
  /// Total amount for the medical grant for a specific month.
  /// </summary>
  [JsonPropertyName("medicalGrantAmt")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 21, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal MedicalGrantAmt
  {
    get => Get<decimal?>("medicalGrantAmt") ?? 0M;
    set => Set(
      "medicalGrantAmt", value == 0M ? null : Truncate(value, 2) as decimal?);
  }

  /// <summary>
  /// The value of the EOF_MTH_NAD_ARR_BAL attribute.
  /// NADC arrears balance as at end of the month.
  /// </summary>
  [JsonPropertyName("eofMthNadArrBal")]
  [Member(Index = 22, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? EofMthNadArrBal
  {
    get => Get<decimal?>("eofMthNadArrBal");
    set => Set("eofMthNadArrBal", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the EOF_MTH_ADC_ARR_BAL attribute.
  /// ADC arrears balance as at end of month.
  /// </summary>
  [JsonPropertyName("eofMthAdcArrBal")]
  [Member(Index = 23, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? EofMthAdcArrBal
  {
    get => Get<decimal?>("eofMthAdcArrBal");
    set => Set("eofMthAdcArrBal", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the EOF_MTH_NAD_INT_BAL attribute.
  /// NADC interest balance as at end of the month.
  /// </summary>
  [JsonPropertyName("eofMthNadIntBal")]
  [Member(Index = 24, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? EofMthNadIntBal
  {
    get => Get<decimal?>("eofMthNadIntBal");
    set => Set("eofMthNadIntBal", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the EOF_MTH_ADC_INT_BAL attribute.
  /// ADC interest balance as at end of the month.
  /// </summary>
  [JsonPropertyName("eofMthAdcIntBal")]
  [Member(Index = 25, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? EofMthAdcIntBal
  {
    get => Get<decimal?>("eofMthAdcIntBal");
    set => Set("eofMthAdcIntBal", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TILL_MTH_CS_COLL_CURR_ARR attribute.
  /// Total Child support collection as at end of the month applied to current 
  /// and arrears.
  /// </summary>
  [JsonPropertyName("tillMthCsCollCurrArr")]
  [Member(Index = 26, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TillMthCsCollCurrArr
  {
    get => Get<decimal?>("tillMthCsCollCurrArr");
    set => Set("tillMthCsCollCurrArr", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TILL_MTH_SP_COLL_CURR_ARR attribute.
  /// Total spousal support collection as at the end of the month applied to 
  /// Current and Arrears
  /// </summary>
  [JsonPropertyName("tillMthSpCollCurrArr")]
  [Member(Index = 27, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TillMthSpCollCurrArr
  {
    get => Get<decimal?>("tillMthSpCollCurrArr");
    set => Set("tillMthSpCollCurrArr", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TILL_MTH_MS_COLL_CURR_ARR attribute.
  /// Total medical support collections as at the end of the month  applied to 
  /// Current and Arrears.
  /// </summary>
  [JsonPropertyName("tillMthMsCollCurrArr")]
  [Member(Index = 28, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TillMthMsCollCurrArr
  {
    get => Get<decimal?>("tillMthMsCollCurrArr");
    set => Set("tillMthMsCollCurrArr", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TILL_MTH_CS_COLL_CURR attribute.
  /// total child support collection as at end of the month applied to current 
  /// only.
  /// </summary>
  [JsonPropertyName("tillMthCsCollCurr")]
  [Member(Index = 29, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TillMthCsCollCurr
  {
    get => Get<decimal?>("tillMthCsCollCurr");
    set => Set("tillMthCsCollCurr", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TILL_MTH_SP_COLL_CURR attribute.
  /// Total spousal support collection as at end of the month applied to current
  /// only.
  /// </summary>
  [JsonPropertyName("tillMthSpCollCurr")]
  [Member(Index = 30, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TillMthSpCollCurr
  {
    get => Get<decimal?>("tillMthSpCollCurr");
    set => Set("tillMthSpCollCurr", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TILL_MTH_MS_COLL_CURR attribute.
  /// Total medical support collection as at end of the month applied to current
  /// only.
  /// </summary>
  [JsonPropertyName("tillMthMsCollCurr")]
  [Member(Index = 31, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TillMthMsCollCurr
  {
    get => Get<decimal?>("tillMthMsCollCurr");
    set => Set("tillMthMsCollCurr", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the FOR_MTH_CS_COLL_CURR attribute.
  /// Total child support collection for the month applied to current only.
  /// </summary>
  [JsonPropertyName("forMthCsCollCurr")]
  [Member(Index = 32, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? ForMthCsCollCurr
  {
    get => Get<decimal?>("forMthCsCollCurr");
    set => Set("forMthCsCollCurr", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the FOR_MTH_SP_COLL_CURR attribute.
  /// Total spousal support collection for the month applied to current only.
  /// </summary>
  [JsonPropertyName("forMthSpCollCurr")]
  [Member(Index = 33, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? ForMthSpCollCurr
  {
    get => Get<decimal?>("forMthSpCollCurr");
    set => Set("forMthSpCollCurr", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the FOR_MTH_MS_COLL_CURR attribute.
  /// Total medical support collection for the month applied to current only.
  /// </summary>
  [JsonPropertyName("forMthMsCollCurr")]
  [Member(Index = 34, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? ForMthMsCollCurr
  {
    get => Get<decimal?>("forMthMsCollCurr");
    set => Set("forMthMsCollCurr", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the EOF_MTH_BAL_CURR_ARR attribute.
  /// Total balance as at end of the month arrears + current.
  /// </summary>
  [JsonPropertyName("eofMthBalCurrArr")]
  [Member(Index = 35, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? EofMthBalCurrArr
  {
    get => Get<decimal?>("eofMthBalCurrArr");
    set => Set("eofMthBalCurrArr", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the FOR MTH NF ARR PRD BAL attribute.
  /// NADC fostercare arrears periodic payment balance for the month.
  /// </summary>
  [JsonPropertyName("forMthNfArrPrdBal")]
  [Member(Index = 36, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? ForMthNfArrPrdBal
  {
    get => Get<decimal?>("forMthNfArrPrdBal");
    set => Set("forMthNfArrPrdBal", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the FOR MTH FC ARR PRD BAL attribute.
  /// ADC fostercare arrears periodic payment balance for the month.
  /// </summary>
  [JsonPropertyName("forMthFcArrPrdBal")]
  [Member(Index = 37, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? ForMthFcArrPrdBal
  {
    get => Get<decimal?>("forMthFcArrPrdBal");
    set => Set("forMthFcArrPrdBal", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TILL MTH NF ARR COLL attribute.
  /// NADC fostercare arrears collection till end of the month.
  /// </summary>
  [JsonPropertyName("tillMthNfArrColl")]
  [Member(Index = 38, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TillMthNfArrColl
  {
    get => Get<decimal?>("tillMthNfArrColl");
    set => Set("tillMthNfArrColl", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TILL MTH NF INT COLL attribute.
  /// NADC fostercare interest collection till the end of the month
  /// </summary>
  [JsonPropertyName("tillMthNfIntColl")]
  [Member(Index = 39, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TillMthNfIntColl
  {
    get => Get<decimal?>("tillMthNfIntColl");
    set => Set("tillMthNfIntColl", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TILL MTH FC ARR COLL attribute.
  /// ADC fostercare arrears collection till the end of the month.
  /// </summary>
  [JsonPropertyName("tillMthFcArrColl")]
  [Member(Index = 40, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TillMthFcArrColl
  {
    get => Get<decimal?>("tillMthFcArrColl");
    set => Set("tillMthFcArrColl", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TILL MTH FC INT COLL attribute.
  /// ADC fostercare interest collection till the end of the month
  /// </summary>
  [JsonPropertyName("tillMthFcIntColl")]
  [Member(Index = 41, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TillMthFcIntColl
  {
    get => Get<decimal?>("tillMthFcIntColl");
    set => Set("tillMthFcIntColl", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the EOF MTH NF ARR BAL attribute.
  /// NADC fostercare arrears balance as at end of the month.
  /// </summary>
  [JsonPropertyName("eofMthNfArrBal")]
  [Member(Index = 42, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? EofMthNfArrBal
  {
    get => Get<decimal?>("eofMthNfArrBal");
    set => Set("eofMthNfArrBal", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the EOF MTH FC ARR BAL attribute.
  /// ADC fostercare arrears balance as at end of month.
  /// </summary>
  [JsonPropertyName("eofMthFcArrBal")]
  [Member(Index = 43, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? EofMthFcArrBal
  {
    get => Get<decimal?>("eofMthFcArrBal");
    set => Set("eofMthFcArrBal", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the EOF MTH NF INT BAL attribute.
  /// NADC fostercare interest balance as at end of the month.
  /// </summary>
  [JsonPropertyName("eofMthNfIntBal")]
  [Member(Index = 44, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? EofMthNfIntBal
  {
    get => Get<decimal?>("eofMthNfIntBal");
    set => Set("eofMthNfIntBal", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the EOF MTH FC INT BAL attribute.
  /// ADC fostercare interest balance as at end of the month.
  /// </summary>
  [JsonPropertyName("eofMthFcIntBal")]
  [Member(Index = 45, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? EofMthFcIntBal
  {
    get => Get<decimal?>("eofMthFcIntBal");
    set => Set("eofMthFcIntBal", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("otyType")]
  [Member(Index = 46, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? OtyType
  {
    get => Get<int?>("otyType");
    set => Set("otyType", value);
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number (sort descending) that, along
  /// with CSE Person, distinguishes one occurrence of the entity type from
  /// another.
  /// </summary>
  [JsonPropertyName("obgIGeneratedId")]
  [Member(Index = 47, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? ObgIGeneratedId
  {
    get => Get<int?>("obgIGeneratedId");
    set => Set("obgIGeneratedId", value);
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspINumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonPropertyName("cspINumber")]
  [Member(Index = 48, Type = MemberType.Char, Length = CspINumber_MaxLength, Optional
    = true)]
  public string CspINumber
  {
    get => Get<string>("cspINumber");
    set =>
      Set("cspINumber", TrimEnd(Substring(value, 1, CspINumber_MaxLength)));
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int CpaIType_MaxLength = 1;

  /// <summary>
  /// The value of the TYPE attribute.
  /// Defines the type of account the entity type represents.
  /// Examples:
  /// 	- Obligor
  /// 	- Obligee
  /// 	- Supported Person
  /// </summary>
  [JsonPropertyName("cpaIType")]
  [Member(Index = 49, Type = MemberType.Char, Length = CpaIType_MaxLength, Optional
    = true)]
  [Value(null)]
  [Value("S")]
  [Value("R")]
  [Value("E")]
  [ImplicitValue("S")]
  public string CpaIType
  {
    get => Get<string>("cpaIType");
    set => Set("cpaIType", TrimEnd(Substring(value, 1, CpaIType_MaxLength)));
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int CpaType_MaxLength = 1;

  /// <summary>
  /// The value of the TYPE attribute.
  /// Defines the type of account the entity type represents.
  /// Examples:
  /// 	- Obligor
  /// 	- Obligee
  /// 	- Supported Person
  /// </summary>
  [JsonPropertyName("cpaType")]
  [Member(Index = 50, Type = MemberType.Char, Length = CpaType_MaxLength, Optional
    = true)]
  [Value(null)]
  [Value("S")]
  [Value("R")]
  [Value("E")]
  [ImplicitValue("S")]
  public string CpaType
  {
    get => Get<string>("cpaType");
    set => Set("cpaType", TrimEnd(Substring(value, 1, CpaType_MaxLength)));
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonPropertyName("cspNumber")]
  [Member(Index = 51, Type = MemberType.Char, Length = CspNumber_MaxLength, Optional
    = true)]
  public string CspNumber
  {
    get => Get<string>("cspNumber");
    set => Set("cspNumber", TrimEnd(Substring(value, 1, CspNumber_MaxLength)));
  }
}
