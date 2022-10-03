// The source file: OBLIGATION, ID: 371437759, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// A requirement on an a CSE Person, which may be court ordered, to pay a 
/// specified amount.
/// This amount may be for monetary and/or health insurance coverage for 
/// children and spouses or as a recovery for overpayment, injuried spouse,
/// withheld child support, duplicate payments, blood test fees, etc.).
/// </summary>
[Serializable]
public partial class Obligation: Heap, ICloneable
{
  /// <summary>Default constructor.</summary>
  public Obligation()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public Obligation(Obligation that):
    base(that)
  {
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new Obligation Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Length of the OTHER_STATE_ABBR attribute.</summary>
  public const int OtherStateAbbr_MaxLength = 2;

  /// <summary>
  /// The value of the OTHER_STATE_ABBR attribute.
  /// This attribute specifies the 2 character alphabetic State Code of the 
  /// other state in the case of incoming interstate obligation. The valid
  /// values are maintained in CODE VALUE table for the Code Name 'STATE CODE'.
  /// </summary>
  [JsonPropertyName("otherStateAbbr")]
  [Member(Index = 1, Type = MemberType.Char, Length
    = OtherStateAbbr_MaxLength, Optional = true)]
  public string OtherStateAbbr
  {
    get => Get<string>("otherStateAbbr");
    set => Set(
      "otherStateAbbr", TrimEnd(Substring(value, 1, OtherStateAbbr_MaxLength)));
      
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number (sort descending) that, along
  /// with CSE Person, distinguishes one occurrence of the entity type from
  /// another.
  /// </summary>
  [JsonPropertyName("systemGeneratedIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 2, Type = MemberType.Number, Length = 3)]
  public int SystemGeneratedIdentifier
  {
    get => Get<int?>("systemGeneratedIdentifier") ?? 0;
    set => Set("systemGeneratedIdentifier", value == 0 ? null : value as int?);
  }

  /// <summary>Length of the DESCRIPTION attribute.</summary>
  public const int Description_MaxLength = 72;

  /// <summary>
  /// The value of the DESCRIPTION attribute.
  /// A description of the obligation.
  /// </summary>
  [JsonPropertyName("description")]
  [Member(Index = 3, Type = MemberType.Char, Length = Description_MaxLength, Optional
    = true)]
  public string Description
  {
    get => Get<string>("description");
    set => Set(
      "description", TrimEnd(Substring(value, 1, Description_MaxLength)));
  }

  /// <summary>Length of the HISTORY_IND attribute.</summary>
  public const int HistoryInd_MaxLength = 1;

  /// <summary>
  /// The value of the HISTORY_IND attribute.
  /// This attribute specifies whether or not the obligation is a history 
  /// record.
  /// Y - The obligation is a historical obligation proior to CSE involvement/
  /// KESSEP conversion.
  /// N - The obligation is not a historical obligation.
  /// The historical obligations are distributed only by REIP collection. The 
  /// nonhistorical obligations are distributed only by non-REIP collections.
  /// 				
  /// </summary>
  [JsonPropertyName("historyInd")]
  [Member(Index = 4, Type = MemberType.Char, Length = HistoryInd_MaxLength, Optional
    = true)]
  public string HistoryInd
  {
    get => Get<string>("historyInd");
    set =>
      Set("historyInd", TrimEnd(Substring(value, 1, HistoryInd_MaxLength)));
  }

  /// <summary>Length of the PRIMARY_SECONDARY_CODE attribute.</summary>
  public const int PrimarySecondaryCode_MaxLength = 1;

  /// <summary>
  /// The value of the PRIMARY_SECONDARY_CODE attribute.
  /// Defines whether or not an Obligation is part of a Concurrent Obligation or
  /// not.  In addition, if the Obligation is part of a Concurrent Obligation,
  /// it defines whether it is a primary or secondary.  This attribute is to aid
  /// in performance.  It identifies whether or not a CONCURRENT_OBLIGATION_RLN
  /// exists and if so, which relationship (i.e. primary or secondary).
  /// </summary>
  [JsonPropertyName("primarySecondaryCode")]
  [Member(Index = 5, Type = MemberType.Char, Length
    = PrimarySecondaryCode_MaxLength, Optional = true)]
  [Value(null)]
  [Value("J")]
  [Value("")]
  [Value("S")]
  [Value("P")]
  public string PrimarySecondaryCode
  {
    get => Get<string>("primarySecondaryCode");
    set => Set(
      "primarySecondaryCode", TrimEnd(Substring(value, 1,
      PrimarySecondaryCode_MaxLength)));
  }

  /// <summary>
  /// The value of the PRE_CONVERSION_DEBT_NUMBER attribute.
  /// Pre-Conversion debt number represents the debt number on the orignal 
  /// system.
  /// </summary>
  [JsonPropertyName("preConversionDebtNumber")]
  [Member(Index = 6, Type = MemberType.Number, Length = 2, Optional = true)]
  public int? PreConversionDebtNumber
  {
    get => Get<int?>("preConversionDebtNumber");
    set => Set("preConversionDebtNumber", value);
  }

  /// <summary>Length of the PRE_CONVERSION_CASE_NUMBER attribute.</summary>
  public const int PreConversionCaseNumber_MaxLength = 12;

  /// <summary>
  /// The value of the PRE_CONVERSION_CASE_NUMBER attribute.
  /// THE CASE NUMBER OF THIS DEBT IN THE kaecses SYSTEM.  uSED DURING 
  /// CONVERSION AND TO MAINTAIN TIE BACK TO KAECSES IS ELIMINATED.
  /// </summary>
  [JsonPropertyName("preConversionCaseNumber")]
  [Member(Index = 7, Type = MemberType.Char, Length
    = PreConversionCaseNumber_MaxLength, Optional = true)]
  public string PreConversionCaseNumber
  {
    get => Get<string>("preConversionCaseNumber");
    set => Set(
      "preConversionCaseNumber", TrimEnd(Substring(value, 1,
      PreConversionCaseNumber_MaxLength)));
  }

  /// <summary>
  /// The value of the AS_OF_DT_NAD_ARR_BAL attribute.
  /// NADC arrears balance as of date.
  /// </summary>
  [JsonPropertyName("asOfDtNadArrBal")]
  [Member(Index = 8, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? AsOfDtNadArrBal
  {
    get => Get<decimal?>("asOfDtNadArrBal");
    set => Set("asOfDtNadArrBal", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the AS_OF_DT_NAD_INT_BAL attribute.
  /// NADC interest balance as of date.
  /// </summary>
  [JsonPropertyName("asOfDtNadIntBal")]
  [Member(Index = 9, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? AsOfDtNadIntBal
  {
    get => Get<decimal?>("asOfDtNadIntBal");
    set => Set("asOfDtNadIntBal", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the AS_OF_DT_ADC_ARR_BAL attribute.
  /// ADC arrears balance as of date.
  /// </summary>
  [JsonPropertyName("asOfDtAdcArrBal")]
  [Member(Index = 10, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? AsOfDtAdcArrBal
  {
    get => Get<decimal?>("asOfDtAdcArrBal");
    set => Set("asOfDtAdcArrBal", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the AS_OF_DT_ADC_INT_BAL attribute.
  /// ADC interest balance as of date.
  /// </summary>
  [JsonPropertyName("asOfDtAdcIntBal")]
  [Member(Index = 11, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? AsOfDtAdcIntBal
  {
    get => Get<decimal?>("asOfDtAdcIntBal");
    set => Set("asOfDtAdcIntBal", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the AS_OF_DT_REC_BAL attribute.
  /// Total recovery balance as of date.
  /// </summary>
  [JsonPropertyName("asOfDtRecBal")]
  [Member(Index = 12, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? AsOfDtRecBal
  {
    get => Get<decimal?>("asOfDtRecBal");
    set => Set("asOfDtRecBal", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the AS_OD_DT_REC_INT_BAL attribute.
  /// Total recovery interest balance as of date.
  /// </summary>
  [JsonPropertyName("asOdDtRecIntBal")]
  [Member(Index = 13, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? AsOdDtRecIntBal
  {
    get => Get<decimal?>("asOdDtRecIntBal");
    set => Set("asOdDtRecIntBal", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the AS_OF_DT_FEE_BAL attribute.
  /// Total fee balance as of date.
  /// </summary>
  [JsonPropertyName("asOfDtFeeBal")]
  [Member(Index = 14, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? AsOfDtFeeBal
  {
    get => Get<decimal?>("asOfDtFeeBal");
    set => Set("asOfDtFeeBal", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the AS_OF_DT_FEE_INT_BAL attribute.
  /// Total fee interest balance as of date.
  /// </summary>
  [JsonPropertyName("asOfDtFeeIntBal")]
  [Member(Index = 15, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? AsOfDtFeeIntBal
  {
    get => Get<decimal?>("asOfDtFeeIntBal");
    set => Set("asOfDtFeeIntBal", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the AS_OF_DT_TOT_BAL_CURR_ARR attribute.
  /// Total balance as of date arrears + current.
  /// </summary>
  [JsonPropertyName("asOfDtTotBalCurrArr")]
  [Member(Index = 16, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? AsOfDtTotBalCurrArr
  {
    get => Get<decimal?>("asOfDtTotBalCurrArr");
    set => Set("asOfDtTotBalCurrArr", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TILL_DT_CS_COLL_CURR_ARR attribute.
  /// Total child support collection till date applied to current and arrears.
  /// </summary>
  [JsonPropertyName("tillDtCsCollCurrArr")]
  [Member(Index = 17, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TillDtCsCollCurrArr
  {
    get => Get<decimal?>("tillDtCsCollCurrArr");
    set => Set("tillDtCsCollCurrArr", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TILL_DT_SP_COLL_CURR_ARR attribute.
  /// Total spousal support collection till date applied to current and arrears.
  /// </summary>
  [JsonPropertyName("tillDtSpCollCurrArr")]
  [Member(Index = 18, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TillDtSpCollCurrArr
  {
    get => Get<decimal?>("tillDtSpCollCurrArr");
    set => Set("tillDtSpCollCurrArr", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TILL_DT_MS_COLL_CURR_ARR attribute.
  /// Total medical support collection till date applied to current and arrears.
  /// </summary>
  [JsonPropertyName("tillDtMsCollCurrArr")]
  [Member(Index = 19, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TillDtMsCollCurrArr
  {
    get => Get<decimal?>("tillDtMsCollCurrArr");
    set => Set("tillDtMsCollCurrArr", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TILL_DT_NAD_ARR_COLL attribute.
  /// Total NADC arrears collection till date.
  /// </summary>
  [JsonPropertyName("tillDtNadArrColl")]
  [Member(Index = 20, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TillDtNadArrColl
  {
    get => Get<decimal?>("tillDtNadArrColl");
    set => Set("tillDtNadArrColl", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TILL_DT_NAD_INT_COLL attribute.
  /// Total NADC interest collection till date.
  /// </summary>
  [JsonPropertyName("tillDtNadIntColl")]
  [Member(Index = 21, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TillDtNadIntColl
  {
    get => Get<decimal?>("tillDtNadIntColl");
    set => Set("tillDtNadIntColl", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TILL_DT_ADC_ARR_COLL attribute.
  /// Total ADC arrears collection till date.
  /// </summary>
  [JsonPropertyName("tillDtAdcArrColl")]
  [Member(Index = 22, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TillDtAdcArrColl
  {
    get => Get<decimal?>("tillDtAdcArrColl");
    set => Set("tillDtAdcArrColl", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TILL_DT_ADC_INT_COLL attribute.
  /// Total ADC interest collection till date.
  /// </summary>
  [JsonPropertyName("tillDtAdcIntColl")]
  [Member(Index = 23, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TillDtAdcIntColl
  {
    get => Get<decimal?>("tillDtAdcIntColl");
    set => Set("tillDtAdcIntColl", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the AS_OF_DT_TOT_REC_COLL attribute.
  /// Total recovery collection as of date.
  /// </summary>
  [JsonPropertyName("asOfDtTotRecColl")]
  [Member(Index = 24, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? AsOfDtTotRecColl
  {
    get => Get<decimal?>("asOfDtTotRecColl");
    set => Set("asOfDtTotRecColl", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the AS_OF_DT_TOT_REC_INT_COLL attribute.
  /// Total recovery interest collection as of date.
  /// </summary>
  [JsonPropertyName("asOfDtTotRecIntColl")]
  [Member(Index = 25, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? AsOfDtTotRecIntColl
  {
    get => Get<decimal?>("asOfDtTotRecIntColl");
    set => Set("asOfDtTotRecIntColl", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the AS_OF_DT_TOT_FEE_COLL attribute.
  /// Total fee collection as of date.
  /// </summary>
  [JsonPropertyName("asOfDtTotFeeColl")]
  [Member(Index = 26, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? AsOfDtTotFeeColl
  {
    get => Get<decimal?>("asOfDtTotFeeColl");
    set => Set("asOfDtTotFeeColl", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the AS_OF_DT_TOT_FEE_INT_COLL attribute.
  /// Total fee interest collection as of date.
  /// </summary>
  [JsonPropertyName("asOfDtTotFeeIntColl")]
  [Member(Index = 27, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? AsOfDtTotFeeIntColl
  {
    get => Get<decimal?>("asOfDtTotFeeIntColl");
    set => Set("asOfDtTotFeeIntColl", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the AS_OF_DT_TOT_COLL_ALL attribute.
  /// Total collection till date applied to all.
  /// </summary>
  [JsonPropertyName("asOfDtTotCollAll")]
  [Member(Index = 28, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? AsOfDtTotCollAll
  {
    get => Get<decimal?>("asOfDtTotCollAll");
    set => Set("asOfDtTotCollAll", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the LAST_COLL_AMT attribute.
  /// The amount of the last payment by the obligor or someone in behalf of the 
  /// obligor.
  /// </summary>
  [JsonPropertyName("lastCollAmt")]
  [Member(Index = 29, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? LastCollAmt
  {
    get => Get<decimal?>("lastCollAmt");
    set => Set("lastCollAmt", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the AS_OF_DT_TOT_GIFT_COLL attribute.
  /// Total gift payments applied to the obligation.
  /// </summary>
  [JsonPropertyName("asOfDtTotGiftColl")]
  [Member(Index = 30, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? AsOfDtTotGiftColl
  {
    get => Get<decimal?>("asOfDtTotGiftColl");
    set => Set("asOfDtTotGiftColl", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the LAST_COLL_DT attribute.
  /// The date of the last payment by the obligor or someone on behalf of the 
  /// obligor.
  /// </summary>
  [JsonPropertyName("lastCollDt")]
  [Member(Index = 31, Type = MemberType.Date, Optional = true)]
  public DateTime? LastCollDt
  {
    get => Get<DateTime?>("lastCollDt");
    set => Set("lastCollDt", value);
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The User ID or Program ID responsible for the creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 32, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// The value of the CREATED_TMST attribute.
  /// The timestamp the occurrence was created.
  /// </summary>
  [JsonPropertyName("createdTmst")]
  [Member(Index = 33, Type = MemberType.Timestamp)]
  public DateTime? CreatedTmst
  {
    get => Get<DateTime?>("createdTmst");
    set => Set("createdTmst", value);
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// The User ID or Program ID responsible for the last update to the 
  /// occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 34, Type = MemberType.Char, Length
    = LastUpdatedBy_MaxLength, Optional = true)]
  public string LastUpdatedBy
  {
    get => Get<string>("lastUpdatedBy");
    set => Set(
      "lastUpdatedBy", TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)));
  }

  /// <summary>
  /// The value of the LAST_UPDATE_TMST attribute.
  /// The timestamp of the most recent update to the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdateTmst")]
  [Member(Index = 35, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdateTmst
  {
    get => Get<DateTime?>("lastUpdateTmst");
    set => Set("lastUpdateTmst", value);
  }

  /// <summary>Length of the ORDER_TYPE_CODE attribute.</summary>
  public const int OrderTypeCode_MaxLength = 1;

  /// <summary>
  /// The value of the ORDER_TYPE_CODE attribute.
  /// Defines the type of obligation as either and incoming Interstate or a 
  /// Kansas Obligation.
  /// 	I - Incoming Interstate Obligation
  /// 	K - Kansas Obligation (default)
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 36, Type = MemberType.Char, Length = OrderTypeCode_MaxLength)]
  [Value("K")]
  [Value("I")]
  [ImplicitValue("K")]
  public string OrderTypeCode
  {
    get => Get<string>("orderTypeCode") ?? "";
    set => Set(
      "orderTypeCode", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, OrderTypeCode_MaxLength)));
  }

  /// <summary>
  /// The json value of the OrderTypeCode attribute.</summary>
  [JsonPropertyName("orderTypeCode")]
  [Computed]
  public string OrderTypeCode_Json
  {
    get => NullIf(OrderTypeCode, "");
    set => OrderTypeCode = value;
  }

  /// <summary>Length of the DORMANT_IND attribute.</summary>
  public const int DormantInd_MaxLength = 1;

  /// <summary>
  /// The value of the DORMANT_IND attribute.
  /// This attribute specifies whether or not the obligation is dormant. If the 
  /// obligation is set to dormant, no action is taken against this obligation.
  /// This attribute helps to deactivate and reactivate an obligation when a AR
  /// attempts to get services from another CSE office.
  /// Y - The obligation is set to 'dormant'
  /// N - The obligation is not dormant.
  /// </summary>
  [JsonPropertyName("dormantInd")]
  [Member(Index = 37, Type = MemberType.Char, Length = DormantInd_MaxLength, Optional
    = true)]
  public string DormantInd
  {
    get => Get<string>("dormantInd");
    set =>
      Set("dormantInd", TrimEnd(Substring(value, 1, DormantInd_MaxLength)));
  }

  /// <summary>
  /// The value of the TOTAL_DEBT_FOR_HIST_OBL attribute.
  /// This attribute specifies the total debt amount accrued for the historical 
  /// obligation between the specified start and discontinue dates of the
  /// obligation. This is computed and updated when the historical obligation is
  /// first setup OR when the Start date or Discontinue date is changed for the
  /// historical obligation.
  /// This attribute helps to reconcile between REIP collections against the 
  /// debt amount for the historical obligation.
  /// </summary>
  [JsonPropertyName("totalDebtForHistObl")]
  [Member(Index = 38, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TotalDebtForHistObl
  {
    get => Get<decimal?>("totalDebtForHistObl");
    set => Set("totalDebtForHistObl", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TILL DT CS COLL CURR attribute.
  /// Total child support collection till date applied to current only.
  /// </summary>
  [JsonPropertyName("tillDtCsCollCurr")]
  [Member(Index = 39, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TillDtCsCollCurr
  {
    get => Get<decimal?>("tillDtCsCollCurr");
    set => Set("tillDtCsCollCurr", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TILL DT SP COLL CURR attribute.
  /// Total spousal support collection till date applied to current only.
  /// </summary>
  [JsonPropertyName("tillDtSpCollCurr")]
  [Member(Index = 40, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TillDtSpCollCurr
  {
    get => Get<decimal?>("tillDtSpCollCurr");
    set => Set("tillDtSpCollCurr", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TILL DT MS COLL CURR attribute.
  /// Total medical support collection till date applied to current only.
  /// </summary>
  [JsonPropertyName("tillDtMsCollCurr")]
  [Member(Index = 41, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TillDtMsCollCurr
  {
    get => Get<decimal?>("tillDtMsCollCurr");
    set => Set("tillDtMsCollCurr", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the AS OF DT NF ARR BAL attribute.
  /// NADC fostercare arrears balance as of date.
  /// </summary>
  [JsonPropertyName("asOfDtNfArrBal")]
  [Member(Index = 42, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? AsOfDtNfArrBal
  {
    get => Get<decimal?>("asOfDtNfArrBal");
    set => Set("asOfDtNfArrBal", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the AS OF DT NF INT BAL attribute.
  /// NADC fostercare interest balance as of date.
  /// </summary>
  [JsonPropertyName("asOfDtNfIntBal")]
  [Member(Index = 43, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? AsOfDtNfIntBal
  {
    get => Get<decimal?>("asOfDtNfIntBal");
    set => Set("asOfDtNfIntBal", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the AS OF DT FC ARR BAL attribute.
  /// ADC fostercare arrears balance as of date.
  /// </summary>
  [JsonPropertyName("asOfDtFcArrBal")]
  [Member(Index = 44, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? AsOfDtFcArrBal
  {
    get => Get<decimal?>("asOfDtFcArrBal");
    set => Set("asOfDtFcArrBal", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the AS OF DT FC INT BAL attribute.
  /// ADC fostercare interest balance as of date.
  /// </summary>
  [JsonPropertyName("asOfDtFcIntBal")]
  [Member(Index = 45, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? AsOfDtFcIntBal
  {
    get => Get<decimal?>("asOfDtFcIntBal");
    set => Set("asOfDtFcIntBal", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TILL DT NF ARR COLL attribute.
  /// Total NADC fostercare arrears collection till date.
  /// </summary>
  [JsonPropertyName("tillDtNfArrColl")]
  [Member(Index = 46, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TillDtNfArrColl
  {
    get => Get<decimal?>("tillDtNfArrColl");
    set => Set("tillDtNfArrColl", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TILL DT NF INT COLL attribute.
  /// Total NADC fostercare interest collection till date.
  /// </summary>
  [JsonPropertyName("tillDtNfIntColl")]
  [Member(Index = 47, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TillDtNfIntColl
  {
    get => Get<decimal?>("tillDtNfIntColl");
    set => Set("tillDtNfIntColl", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TILL DT FC ARR COLL attribute.
  /// Total ADC fostercare arrears collection till date.
  /// </summary>
  [JsonPropertyName("tillDtFcArrColl")]
  [Member(Index = 48, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TillDtFcArrColl
  {
    get => Get<decimal?>("tillDtFcArrColl");
    set => Set("tillDtFcArrColl", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TILL DT FC INT COLL attribute.
  /// Total ADC fostercare interest collection till date.
  /// </summary>
  [JsonPropertyName("tillDtFcIntColl")]
  [Member(Index = 49, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TillDtFcIntColl
  {
    get => Get<decimal?>("tillDtFcIntColl");
    set => Set("tillDtFcIntColl", Truncate(value, 2));
  }

  /// <summary>Length of the LAST_OBLIGATION_EVENT attribute.</summary>
  public const int LastObligationEvent_MaxLength = 1;

  /// <summary>
  /// The value of the LAST_OBLIGATION_EVENT attribute.
  /// This attribute is populated with the most recent status event for all 
  /// debt_details for this obligation. The values are blank for none, A for
  /// Activation, or D for Deactivation.
  /// </summary>
  [JsonPropertyName("lastObligationEvent")]
  [Member(Index = 50, Type = MemberType.Char, Length
    = LastObligationEvent_MaxLength, Optional = true)]
  public string LastObligationEvent
  {
    get => Get<string>("lastObligationEvent");
    set => Set(
      "lastObligationEvent", TrimEnd(Substring(value, 1,
      LastObligationEvent_MaxLength)));
  }

  /// <summary>Length of the DELINQUENT_IND attribute.</summary>
  public const int DelinquentInd_MaxLength = 1;

  /// <summary>
  /// The value of the DELINQUENT_IND attribute.
  /// This attribute indicates delinquency on a court order level. It is used 
  /// for accruing obligations only. If an obligation has arrears equal to or
  /// greater than the monthly support amount, it is considered delinquent, and
  /// this indicator will be set to 'Y' for all obligations on the court order
  /// that the delinquent obligation is associated with.
  /// </summary>
  [JsonPropertyName("delinquentInd")]
  [Member(Index = 51, Type = MemberType.Char, Length
    = DelinquentInd_MaxLength, Optional = true)]
  public string DelinquentInd
  {
    get => Get<string>("delinquentInd");
    set => Set(
      "delinquentInd", TrimEnd(Substring(value, 1, DelinquentInd_MaxLength)));
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspPNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonPropertyName("cspPNumber")]
  [Member(Index = 52, Type = MemberType.Char, Length = CspPNumber_MaxLength, Optional
    = true)]
  public string CspPNumber
  {
    get => Get<string>("cspPNumber");
    set =>
      Set("cspPNumber", TrimEnd(Substring(value, 1, CspPNumber_MaxLength)));
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// This is a system generated number to uniquely identify a legal action.
  /// </summary>
  [JsonPropertyName("lgaId")]
  [Member(Index = 53, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? LgaId
  {
    get => Get<int?>("lgaId");
    set => Set("lgaId", value);
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("dtyGeneratedId")]
  [DefaultValue(0)]
  [Member(Index = 54, Type = MemberType.Number, Length = 3)]
  public int DtyGeneratedId
  {
    get => Get<int?>("dtyGeneratedId") ?? 0;
    set => Set("dtyGeneratedId", value == 0 ? null : value as int?);
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
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 55, Type = MemberType.Char, Length = CpaType_MaxLength)]
  [Value("S")]
  [Value("R")]
  [Value("E")]
  [ImplicitValue("S")]
  public string CpaType
  {
    get => Get<string>("cpaType") ?? "";
    set => Set(
      "cpaType", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, CpaType_MaxLength)));
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

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 56, Type = MemberType.Char, Length = CspNumber_MaxLength)]
  public string CspNumber
  {
    get => Get<string>("cspNumber") ?? "";
    set => Set(
      "cspNumber", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, CspNumber_MaxLength)));
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

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated timestamp that distinguishes one occurrence of 
  /// the entity type from another.
  /// </summary>
  [JsonPropertyName("prqId")]
  [Member(Index = 57, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? PrqId
  {
    get => Get<int?>("prqId");
    set => Set("prqId", value);
  }

  /// <summary>
  /// The value of the NUMBER attribute.
  /// A number, unique within the legal action, used to identify each detail of 
  /// the Legal Action.  Starts with one and moves sequentially.
  /// </summary>
  [JsonPropertyName("ladNumber")]
  [Member(Index = 58, Type = MemberType.Number, Length = 2, Optional = true)]
  public int? LadNumber
  {
    get => Get<int?>("ladNumber");
    set => Set("ladNumber", value);
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// This is a system generated number to uniquely identify a legal action.
  /// </summary>
  [JsonPropertyName("lgaIdentifier")]
  [Member(Index = 59, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? LgaIdentifier
  {
    get => Get<int?>("lgaIdentifier");
    set => Set("lgaIdentifier", value);
  }
}
