// The source file: CSE_PERSON_ACCOUNT, ID: 371432855, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// This respresents a persons accounts.  Valid types are Obligor, Obligee and 
/// Supported Person.  Summary data is stored here to enhance performance.
/// </summary>
[Serializable]
public partial class CsePersonAccount: Heap, ICloneable
{
  /// <summary>Default constructor.</summary>
  public CsePersonAccount()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public CsePersonAccount(CsePersonAccount that):
    base(that)
  {
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new CsePersonAccount Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Length of the TYPE attribute.</summary>
  public const int Type1_MaxLength = 1;

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
  [Member(Index = 1, Type = MemberType.Char, Length = Type1_MaxLength)]
  [Value("S")]
  [Value("R")]
  [Value("E")]
  [ImplicitValue("S")]
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

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The userid of the person or program that created the occurrance of the 
  /// entity.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// The date and time that the occurrance of the entity was created.
  /// </summary>
  [JsonPropertyName("createdTmst")]
  [Member(Index = 3, Type = MemberType.Timestamp)]
  public DateTime? CreatedTmst
  {
    get => Get<DateTime?>("createdTmst");
    set => Set("createdTmst", value);
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// The userid of the person or program that last updated the occurrance of 
  /// the entity.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 4, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength, Optional
    = true)]
  public string LastUpdatedBy
  {
    get => Get<string>("lastUpdatedBy");
    set => Set(
      "lastUpdatedBy", TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)));
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TMST attribute.
  /// The date and time that the occurrance of the entity was last updated.
  /// </summary>
  [JsonPropertyName("lastUpdatedTmst")]
  [Member(Index = 5, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTmst
  {
    get => Get<DateTime?>("lastUpdatedTmst");
    set => Set("lastUpdatedTmst", value);
  }

  /// <summary>
  /// The value of the PGM_CHG_EFFECTIVE_DATE attribute.
  /// Used as a driver for the batch program change processing. If the value is 
  /// NULL, then no change has occurred. If the value is greater than NULL, then
  /// all Collections which were applied to the Supported Person are to be
  /// adjusted back to this date.			
  /// </summary>
  [JsonPropertyName("pgmChgEffectiveDate")]
  [Member(Index = 6, Type = MemberType.Date, Optional = true)]
  public DateTime? PgmChgEffectiveDate
  {
    get => Get<DateTime?>("pgmChgEffectiveDate");
    set => Set("pgmChgEffectiveDate", value);
  }

  /// <summary>Length of the TRIGGER_TYPE attribute.</summary>
  public const int TriggerType_MaxLength = 1;

  /// <summary>
  /// The value of the TRIGGER_TYPE attribute.
  /// This attribute will be used in conjunction with 
  /// PGM_CHG_EFFECTIVE_DATE on CSE_PERSON_ACCOUNT to signal the batch
  /// trigger program. This attribute will indicator which type of trigger
  /// has been set. The two options represent a Person Program change or
  /// a URA modification (Codes:P=Person Program,U=URA).
  /// Once the trigger has been
  /// process, both the date attribute and this attribute will be reset to
  /// spaces or null. The current Person Program functionality will not
  /// change. For URA, the date and this attribute will be set by the URA
  /// processes (on-line &amp; batch) to a date that represents the date
  /// to which collections should be reversed and reapplied.
  /// 
  /// The existing date will always be set to the first day of the month
  /// in which the adjustment or new grant information is added.
  /// </summary>
  [JsonPropertyName("triggerType")]
  [Member(Index = 7, Type = MemberType.Char, Length = TriggerType_MaxLength, Optional
    = true)]
  public string TriggerType
  {
    get => Get<string>("triggerType");
    set => Set(
      "triggerType", TrimEnd(Substring(value, 1, TriggerType_MaxLength)));
  }

  /// <summary>
  /// The value of the SUPP_TILL_DT_TOT_GIFT_COLL attribute.
  /// Total gift collection as of date.
  /// </summary>
  [JsonPropertyName("suppTillDtTotGiftColl")]
  [Member(Index = 8, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? SuppTillDtTotGiftColl
  {
    get => Get<decimal?>("suppTillDtTotGiftColl");
    set => Set("suppTillDtTotGiftColl", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the SUPP_AS_OF_DT_NAD_ARR_BAL attribute.
  /// NADC Arrears balance as of date.
  /// </summary>
  [JsonPropertyName("suppAsOfDtNadArrBal")]
  [Member(Index = 9, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? SuppAsOfDtNadArrBal
  {
    get => Get<decimal?>("suppAsOfDtNadArrBal");
    set => Set("suppAsOfDtNadArrBal", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the SUPP_AS_OF_DT_NAD_INT_BAL attribute.
  /// NADC interest balance as of date.
  /// </summary>
  [JsonPropertyName("suppAsOfDtNadIntBal")]
  [Member(Index = 10, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? SuppAsOfDtNadIntBal
  {
    get => Get<decimal?>("suppAsOfDtNadIntBal");
    set => Set("suppAsOfDtNadIntBal", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the SUPP_AS_OF_DT_ADC_ARR_BAL attribute.
  /// ADC arrears balance as of date.
  /// </summary>
  [JsonPropertyName("suppAsOfDtAdcArrBal")]
  [Member(Index = 11, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? SuppAsOfDtAdcArrBal
  {
    get => Get<decimal?>("suppAsOfDtAdcArrBal");
    set => Set("suppAsOfDtAdcArrBal", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the SUPP_AS_OF_DT_ADC_INT_BAL attribute.
  /// ADC interest balance as of date.
  /// </summary>
  [JsonPropertyName("suppAsOfDtAdcIntBal")]
  [Member(Index = 12, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? SuppAsOfDtAdcIntBal
  {
    get => Get<decimal?>("suppAsOfDtAdcIntBal");
    set => Set("suppAsOfDtAdcIntBal", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the SUPP_AS_OF_DT_TOT_BAL_CURR_ARR attribute.
  /// Total balance as of date (arrears + current).
  /// </summary>
  [JsonPropertyName("suppAsOfDtTotBalCurrArr")]
  [Member(Index = 13, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? SuppAsOfDtTotBalCurrArr
  {
    get => Get<decimal?>("suppAsOfDtTotBalCurrArr");
    set => Set("suppAsOfDtTotBalCurrArr", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the SUPP_TILL_DT_NAD_ARR_COLL attribute.
  /// NADC arrears collection till date.
  /// </summary>
  [JsonPropertyName("suppTillDtNadArrColl")]
  [Member(Index = 14, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? SuppTillDtNadArrColl
  {
    get => Get<decimal?>("suppTillDtNadArrColl");
    set => Set("suppTillDtNadArrColl", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the SUPP_TILL_DT_NAD_INT_COLL attribute.
  /// NADC interest collection till date.
  /// </summary>
  [JsonPropertyName("suppTillDtNadIntColl")]
  [Member(Index = 15, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? SuppTillDtNadIntColl
  {
    get => Get<decimal?>("suppTillDtNadIntColl");
    set => Set("suppTillDtNadIntColl", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the SUPP_TILL_DT_ADC_ARR_COLL attribute.
  /// ADC arrears collection till date.
  /// </summary>
  [JsonPropertyName("suppTillDtAdcArrColl")]
  [Member(Index = 16, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? SuppTillDtAdcArrColl
  {
    get => Get<decimal?>("suppTillDtAdcArrColl");
    set => Set("suppTillDtAdcArrColl", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the SUPP_TILL_DT_ADC_INT_COLL attribute.
  /// ADC interest collection till date.
  /// </summary>
  [JsonPropertyName("suppTillDtAdcIntColl")]
  [Member(Index = 17, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? SuppTillDtAdcIntColl
  {
    get => Get<decimal?>("suppTillDtAdcIntColl");
    set => Set("suppTillDtAdcIntColl", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the SUPP_TILL_DT_TOT_COLL_ALL attribute.
  /// Total collection as of date applied to all.
  /// </summary>
  [JsonPropertyName("suppTillDtTotCollAll")]
  [Member(Index = 18, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? SuppTillDtTotCollAll
  {
    get => Get<decimal?>("suppTillDtTotCollAll");
    set => Set("suppTillDtTotCollAll", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TOT_PER_CHLD_NON_ADC_UNDISB_AMT attribute.
  /// Total of all undisbursed Non-ADC amounts for a specific child.
  /// </summary>
  [JsonPropertyName("totPerChldNonAdcUndisbAmt")]
  [Member(Index = 19, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TotPerChldNonAdcUndisbAmt
  {
    get => Get<decimal?>("totPerChldNonAdcUndisbAmt");
    set => Set("totPerChldNonAdcUndisbAmt", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TOT_PER_CHLD_MED_UNDISB_AMT attribute.
  /// Total of all undisbursed medical amounts for a specific child.
  /// </summary>
  [JsonPropertyName("totPerChldMedUndisbAmt")]
  [Member(Index = 20, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TotPerChldMedUndisbAmt
  {
    get => Get<decimal?>("totPerChldMedUndisbAmt");
    set => Set("totPerChldMedUndisbAmt", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TOT_PER_CHLD_PASSTHRU_UNDISB_AMT attribute.
  /// Total of all undisbursed passthru amounts for a specific child.
  /// </summary>
  [JsonPropertyName("totPerChldPassthruUndisbAmt")]
  [Member(Index = 21, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TotPerChldPassthruUndisbAmt
  {
    get => Get<decimal?>("totPerChldPassthruUndisbAmt");
    set => Set("totPerChldPassthruUndisbAmt", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TOT_PER_CHLD_NON_ADC_DISB_AMT attribute.
  /// Total of all disbursed Non-ADC amounts for a specific child.
  /// </summary>
  [JsonPropertyName("totPerChldNonAdcDisbAmt")]
  [Member(Index = 22, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TotPerChldNonAdcDisbAmt
  {
    get => Get<decimal?>("totPerChldNonAdcDisbAmt");
    set => Set("totPerChldNonAdcDisbAmt", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TOT_PER_CHLD_MED_DISB_AMT attribute.
  /// Total of all disbursed medical amounts for a specific child.
  /// </summary>
  [JsonPropertyName("totPerChldMedDisbAmt")]
  [Member(Index = 23, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TotPerChldMedDisbAmt
  {
    get => Get<decimal?>("totPerChldMedDisbAmt");
    set => Set("totPerChldMedDisbAmt", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TOT_PER_CHLD_PASSTHRU_DISB_AMT attribute.
  /// Total of all disbursed passthru amounts for a specific child.
  /// </summary>
  [JsonPropertyName("totPerChldPassthruDisbAmt")]
  [Member(Index = 24, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TotPerChldPassthruDisbAmt
  {
    get => Get<decimal?>("totPerChldPassthruDisbAmt");
    set => Set("totPerChldPassthruDisbAmt", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the LAST_PER_CHLD_DISB_AMT attribute.
  /// Amount of last disbursement for a specific child.
  /// </summary>
  [JsonPropertyName("lastPerChldDisbAmt")]
  [Member(Index = 25, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? LastPerChldDisbAmt
  {
    get => Get<decimal?>("lastPerChldDisbAmt");
    set => Set("lastPerChldDisbAmt", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the LAST_PER_CHLD_DISB_DT attribute.
  /// Date of last disbursement for a specific child.
  /// </summary>
  [JsonPropertyName("lastPerChldDisbDt")]
  [Member(Index = 26, Type = MemberType.Date, Optional = true)]
  public DateTime? LastPerChldDisbDt
  {
    get => Get<DateTime?>("lastPerChldDisbDt");
    set => Set("lastPerChldDisbDt", value);
  }

  /// <summary>
  /// The value of the SUPP_TILL_DT_CS_COLL_CURR_ARR attribute.
  /// Total Child support collection as of date applied to current and arrears.
  /// </summary>
  [JsonPropertyName("suppTillDtCsCollCurrArr")]
  [Member(Index = 27, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? SuppTillDtCsCollCurrArr
  {
    get => Get<decimal?>("suppTillDtCsCollCurrArr");
    set => Set("suppTillDtCsCollCurrArr", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the SUPP_TILL_DT_SP_COLL_CURR_ARR attribute.
  /// Total spousal support collection as of date applied to current and 
  /// arrears.
  /// </summary>
  [JsonPropertyName("suppTillDtSpCollCurrArr")]
  [Member(Index = 28, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? SuppTillDtSpCollCurrArr
  {
    get => Get<decimal?>("suppTillDtSpCollCurrArr");
    set => Set("suppTillDtSpCollCurrArr", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the SUPP_TILL_DT_MS_COLL_CURR_ARR attribute.
  /// Total Medical support collection as of date applied to current and 
  /// arrears.
  /// </summary>
  [JsonPropertyName("suppTillDtMsCollCurrArr")]
  [Member(Index = 29, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? SuppTillDtMsCollCurrArr
  {
    get => Get<decimal?>("suppTillDtMsCollCurrArr");
    set => Set("suppTillDtMsCollCurrArr", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the SUPP_TILL_DT_CS_COLL_CURR attribute.
  /// Total Child support collection as of date applied to current only.
  /// </summary>
  [JsonPropertyName("suppTillDtCsCollCurr")]
  [Member(Index = 30, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? SuppTillDtCsCollCurr
  {
    get => Get<decimal?>("suppTillDtCsCollCurr");
    set => Set("suppTillDtCsCollCurr", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the SUPP_TILL_DT_SP_COLL_CURR attribute.
  /// Total spousal support collection as of date applied to current only.
  /// </summary>
  [JsonPropertyName("suppTillDtSpCollCurr")]
  [Member(Index = 31, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? SuppTillDtSpCollCurr
  {
    get => Get<decimal?>("suppTillDtSpCollCurr");
    set => Set("suppTillDtSpCollCurr", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the SUPP_TILL_DT_MS_COLL_CURR attribute.
  /// Total Medical support collection as of date applied to current only.
  /// </summary>
  [JsonPropertyName("suppTillDtMsCollCurr")]
  [Member(Index = 32, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? SuppTillDtMsCollCurr
  {
    get => Get<decimal?>("suppTillDtMsCollCurr");
    set => Set("suppTillDtMsCollCurr", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the SUPP AS OF DT NF ARR BAL attribute.
  /// Non ADC Fostercare Arrears balance as of date.
  /// </summary>
  [JsonPropertyName("suppAsOfDtNfArrBal")]
  [Member(Index = 33, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? SuppAsOfDtNfArrBal
  {
    get => Get<decimal?>("suppAsOfDtNfArrBal");
    set => Set("suppAsOfDtNfArrBal", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the SUPP AS OF DT NF INT BAL attribute.
  /// Non ADC Fostercare interest balance as of date.
  /// </summary>
  [JsonPropertyName("suppAsOfDtNfIntBal")]
  [Member(Index = 34, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? SuppAsOfDtNfIntBal
  {
    get => Get<decimal?>("suppAsOfDtNfIntBal");
    set => Set("suppAsOfDtNfIntBal", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the SUPP AS OF DT FC ARR BAL attribute.
  /// ADC Fostercare arrears balance as of date.
  /// </summary>
  [JsonPropertyName("suppAsOfDtFcArrBal")]
  [Member(Index = 35, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? SuppAsOfDtFcArrBal
  {
    get => Get<decimal?>("suppAsOfDtFcArrBal");
    set => Set("suppAsOfDtFcArrBal", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the SUPP AS OF DT FC INT BAL attribute.
  /// ADC fostercare interest balance as of date.
  /// </summary>
  [JsonPropertyName("suppAsOfDtFcIntBal")]
  [Member(Index = 36, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? SuppAsOfDtFcIntBal
  {
    get => Get<decimal?>("suppAsOfDtFcIntBal");
    set => Set("suppAsOfDtFcIntBal", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the SUPP TILL DT NF ARR COLL attribute.
  /// NADC Fostercare arrears collection till date.
  /// </summary>
  [JsonPropertyName("suppTillDtNfArrColl")]
  [Member(Index = 37, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? SuppTillDtNfArrColl
  {
    get => Get<decimal?>("suppTillDtNfArrColl");
    set => Set("suppTillDtNfArrColl", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the SUPP TILL DT NF INT COLL attribute.
  /// NADC Fostercare interest collection till date.
  /// </summary>
  [JsonPropertyName("suppTillDtNfIntColl")]
  [Member(Index = 38, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? SuppTillDtNfIntColl
  {
    get => Get<decimal?>("suppTillDtNfIntColl");
    set => Set("suppTillDtNfIntColl", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the SUPP TILL DT FC ARR COLL attribute.
  /// ADC Fostercare arrears collection till date.
  /// </summary>
  [JsonPropertyName("suppTillDtFcArrColl")]
  [Member(Index = 39, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? SuppTillDtFcArrColl
  {
    get => Get<decimal?>("suppTillDtFcArrColl");
    set => Set("suppTillDtFcArrColl", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the SUPP TILL DT FC INT COLL attribute.
  /// ADC Fostercare interest collection till date.
  /// </summary>
  [JsonPropertyName("suppTillDtFcIntColl")]
  [Member(Index = 40, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? SuppTillDtFcIntColl
  {
    get => Get<decimal?>("suppTillDtFcIntColl");
    set => Set("suppTillDtFcIntColl", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TOT_NON_ADC_DISB_AMT attribute.
  /// Total of all disbursed Non-ADC amounts for all of the children under the 
  /// AR's care.
  /// </summary>
  [JsonPropertyName("totNonAdcDisbAmt")]
  [Member(Index = 41, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TotNonAdcDisbAmt
  {
    get => Get<decimal?>("totNonAdcDisbAmt");
    set => Set("totNonAdcDisbAmt", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TOT_ADC_DISB_AMT attribute.
  /// Total of all undisbursed passthru amounts for all of the children under 
  /// the AR's care.
  /// </summary>
  [JsonPropertyName("totAdcDisbAmt")]
  [Member(Index = 42, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TotAdcDisbAmt
  {
    get => Get<decimal?>("totAdcDisbAmt");
    set => Set("totAdcDisbAmt", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TOT_MED_DISB_AMT attribute.
  /// Total of all disbursed medical amounts for all of the children under the 
  /// AR's care.
  /// </summary>
  [JsonPropertyName("totMedDisbAmt")]
  [Member(Index = 43, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TotMedDisbAmt
  {
    get => Get<decimal?>("totMedDisbAmt");
    set => Set("totMedDisbAmt", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TOT_PASSTHRU_DISB_AMT attribute.
  /// Total of all disbursed passthru amounts for all of the children under the 
  /// AR's care.
  /// </summary>
  [JsonPropertyName("totPassthruDisbAmt")]
  [Member(Index = 44, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TotPassthruDisbAmt
  {
    get => Get<decimal?>("totPassthruDisbAmt");
    set => Set("totPassthruDisbAmt", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TOT_NON_ADC_UNDISB_AMT attribute.
  /// Total of all undisbursed Non-ADC amounts for all supported children under 
  /// the AR's care.
  /// </summary>
  [JsonPropertyName("totNonAdcUndisbAmt")]
  [Member(Index = 45, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TotNonAdcUndisbAmt
  {
    get => Get<decimal?>("totNonAdcUndisbAmt");
    set => Set("totNonAdcUndisbAmt", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TOT_ADC_UNDISB_AMT attribute.
  /// Total of all ADC undisbursed collections for all of the children under the
  /// AR's care.
  /// </summary>
  [JsonPropertyName("totAdcUndisbAmt")]
  [Member(Index = 46, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TotAdcUndisbAmt
  {
    get => Get<decimal?>("totAdcUndisbAmt");
    set => Set("totAdcUndisbAmt", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the LAST_DISB_AMT attribute.
  /// Amount of last disbursement.
  /// </summary>
  [JsonPropertyName("lastDisbAmt")]
  [Member(Index = 47, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? LastDisbAmt
  {
    get => Get<decimal?>("lastDisbAmt");
    set => Set("lastDisbAmt", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the LAST_DISB_DT attribute.
  /// Date of last disbursement.
  /// </summary>
  [JsonPropertyName("lastDisbDt")]
  [Member(Index = 48, Type = MemberType.Date, Optional = true)]
  public DateTime? LastDisbDt
  {
    get => Get<DateTime?>("lastDisbDt");
    set => Set("lastDisbDt", value);
  }

  /// <summary>
  /// The value of the AS_OF_DT_TOT_GIFT_COLL attribute.
  /// Total gift collection as of date.
  /// </summary>
  [JsonPropertyName("asOfDtTotGiftColl")]
  [Member(Index = 49, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? AsOfDtTotGiftColl
  {
    get => Get<decimal?>("asOfDtTotGiftColl");
    set => Set("asOfDtTotGiftColl", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the OVERPAYMENT_LETTER_SEND_DATE attribute.
  /// Represents the date the future payment letter (the letter requesting the 
  /// obligor to selection the intent of his/her overpayment) was sent.  There
  /// are three options:
  /// 	1 - Refund the money
  /// 	2 - Treat as a future payment
  /// 	3 - Treat as a gift
  /// </summary>
  [JsonPropertyName("overpaymentLetterSendDate")]
  [Member(Index = 50, Type = MemberType.Date, Optional = true)]
  public DateTime? OverpaymentLetterSendDate
  {
    get => Get<DateTime?>("overpaymentLetterSendDate");
    set => Set("overpaymentLetterSendDate", value);
  }

  /// <summary>
  /// The value of the LAST_MANUAL_DISTRIBUTION_DATE attribute.
  /// Represents the date that the last manual distribution was applied to any 
  /// obligation for a Obligor.  This is assist in determining whether or not
  /// retroactive collections can be automatically distributed.  With a
  /// collection date less or equal to the manual distribution date, the
  /// collection will be suspended.  With a collection date greater than the
  /// manual distribution date, the collection will be processed normally.
  /// </summary>
  [JsonPropertyName("lastManualDistributionDate")]
  [Member(Index = 51, Type = MemberType.Date, Optional = true)]
  public DateTime? LastManualDistributionDate
  {
    get => Get<DateTime?>("lastManualDistributionDate");
    set => Set("lastManualDistributionDate", value);
  }

  /// <summary>
  /// The value of the AS_OF_DT_NAD_ARR_BAL attribute.
  /// Defines the Non-ADC arrears balance as of date.
  /// </summary>
  [JsonPropertyName("asOfDtNadArrBal")]
  [Member(Index = 52, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? AsOfDtNadArrBal
  {
    get => Get<decimal?>("asOfDtNadArrBal");
    set => Set("asOfDtNadArrBal", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the AS_OF_DT_NAD_INT_BAL attribute.
  /// Defines the Non-ADC interest balance as of date.
  /// </summary>
  [JsonPropertyName("asOfDtNadIntBal")]
  [Member(Index = 53, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? AsOfDtNadIntBal
  {
    get => Get<decimal?>("asOfDtNadIntBal");
    set => Set("asOfDtNadIntBal", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the AS_OF_DT_ADC_ARR_BAL attribute.
  /// Defines the ADC arrears balance as of date.
  /// </summary>
  [JsonPropertyName("asOfDtAdcArrBal")]
  [Member(Index = 54, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? AsOfDtAdcArrBal
  {
    get => Get<decimal?>("asOfDtAdcArrBal");
    set => Set("asOfDtAdcArrBal", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the AS_OF_DT_ADC_INT_BAL attribute.
  /// Defines the ADC interest balance as of date.
  /// </summary>
  [JsonPropertyName("asOfDtAdcIntBal")]
  [Member(Index = 55, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? AsOfDtAdcIntBal
  {
    get => Get<decimal?>("asOfDtAdcIntBal");
    set => Set("asOfDtAdcIntBal", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the AS_OF_DT_REC_BAL attribute.
  /// Totoal recovery balance as of date.
  /// </summary>
  [JsonPropertyName("asOfDtRecBal")]
  [Member(Index = 56, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? AsOfDtRecBal
  {
    get => Get<decimal?>("asOfDtRecBal");
    set => Set("asOfDtRecBal", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the AS_OF_DT_TOT_REC_INT_BAL attribute.
  /// Total recovery interest balance as of date.
  /// </summary>
  [JsonPropertyName("asOfDtTotRecIntBal")]
  [Member(Index = 57, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? AsOfDtTotRecIntBal
  {
    get => Get<decimal?>("asOfDtTotRecIntBal");
    set => Set("asOfDtTotRecIntBal", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the AS_OF_DT_TOT_FEE_BAL attribute.
  /// Total Fee Balance as of date.
  /// </summary>
  [JsonPropertyName("asOfDtTotFeeBal")]
  [Member(Index = 58, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? AsOfDtTotFeeBal
  {
    get => Get<decimal?>("asOfDtTotFeeBal");
    set => Set("asOfDtTotFeeBal", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the AS_OF_DT_TOT_FEE_INT_BAL attribute.
  /// Total Fee Interest Balance as of date.
  /// </summary>
  [JsonPropertyName("asOfDtTotFeeIntBal")]
  [Member(Index = 59, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? AsOfDtTotFeeIntBal
  {
    get => Get<decimal?>("asOfDtTotFeeIntBal");
    set => Set("asOfDtTotFeeIntBal", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the AS_OF_DT_TOT_BAL_CURR_ARR attribute.
  /// Total balance as of date (arrears &amp; current).
  /// </summary>
  [JsonPropertyName("asOfDtTotBalCurrArr")]
  [Member(Index = 60, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? AsOfDtTotBalCurrArr
  {
    get => Get<decimal?>("asOfDtTotBalCurrArr");
    set => Set("asOfDtTotBalCurrArr", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TILL_DT_CS_COLL_CURR_ARR attribute.
  /// Total of all child support collection as of date applied to current and 
  /// arrears.
  /// </summary>
  [JsonPropertyName("tillDtCsCollCurrArr")]
  [Member(Index = 61, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TillDtCsCollCurrArr
  {
    get => Get<decimal?>("tillDtCsCollCurrArr");
    set => Set("tillDtCsCollCurrArr", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TILL_DT_SP_COLL_CURR_ARR attribute.
  /// Total of all spousal support collection as of date applied to current and 
  /// arrears.
  /// </summary>
  [JsonPropertyName("tillDtSpCollCurrArr")]
  [Member(Index = 62, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TillDtSpCollCurrArr
  {
    get => Get<decimal?>("tillDtSpCollCurrArr");
    set => Set("tillDtSpCollCurrArr", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TILL_DT_MS_COLL_CURR_ARR attribute.
  /// Total of all medical support collection as of date applied to current and 
  /// arrears.
  /// </summary>
  [JsonPropertyName("tillDtMsCollCurrArr")]
  [Member(Index = 63, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TillDtMsCollCurrArr
  {
    get => Get<decimal?>("tillDtMsCollCurrArr");
    set => Set("tillDtMsCollCurrArr", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TILL_DT_CS_COLL_CURR attribute.
  /// Total child support collection as of date applied to current only.
  /// </summary>
  [JsonPropertyName("tillDtCsCollCurr")]
  [Member(Index = 64, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TillDtCsCollCurr
  {
    get => Get<decimal?>("tillDtCsCollCurr");
    set => Set("tillDtCsCollCurr", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TILL_DT_SP_COLL_CURR attribute.
  /// Total spousal support collection as of date applied to current only.
  /// </summary>
  [JsonPropertyName("tillDtSpCollCurr")]
  [Member(Index = 65, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TillDtSpCollCurr
  {
    get => Get<decimal?>("tillDtSpCollCurr");
    set => Set("tillDtSpCollCurr", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TILL_DT_MS_COLL_CURR attribute.
  /// Total medical support collection as of date applied to current only.
  /// </summary>
  [JsonPropertyName("tillDtMsCollCurr")]
  [Member(Index = 66, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TillDtMsCollCurr
  {
    get => Get<decimal?>("tillDtMsCollCurr");
    set => Set("tillDtMsCollCurr", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TILL_DT_NAD_ARR_COLL attribute.
  /// Total of all Non-ADC arrears collection till date.
  /// </summary>
  [JsonPropertyName("tillDtNadArrColl")]
  [Member(Index = 67, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TillDtNadArrColl
  {
    get => Get<decimal?>("tillDtNadArrColl");
    set => Set("tillDtNadArrColl", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TILL_DT_NAD_INT_COLL attribute.
  /// Total amount of all Non-ADC interest collection till date.
  /// </summary>
  [JsonPropertyName("tillDtNadIntColl")]
  [Member(Index = 68, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TillDtNadIntColl
  {
    get => Get<decimal?>("tillDtNadIntColl");
    set => Set("tillDtNadIntColl", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TILL_DT_ADC_ARR_COLL attribute.
  /// Total amount of all ADC arrears collection till date.
  /// </summary>
  [JsonPropertyName("tillDtAdcArrColl")]
  [Member(Index = 69, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TillDtAdcArrColl
  {
    get => Get<decimal?>("tillDtAdcArrColl");
    set => Set("tillDtAdcArrColl", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TILL_DT_ADC_INT_COLL attribute.
  /// Total amount of all ADC interest collection till date.
  /// </summary>
  [JsonPropertyName("tillDtAdcIntColl")]
  [Member(Index = 70, Type = MemberType.Number, Length = 9, Precision = 2, Optional
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
  [Member(Index = 71, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? AsOfDtTotRecColl
  {
    get => Get<decimal?>("asOfDtTotRecColl");
    set => Set("asOfDtTotRecColl", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the AS_OF_DT_TOT_REC_INT_COLL attribute.
  /// Total recovery interest collection amount as of date.
  /// </summary>
  [JsonPropertyName("asOfDtTotRecIntColl")]
  [Member(Index = 72, Type = MemberType.Number, Length = 9, Precision = 2, Optional
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
  [Member(Index = 73, Type = MemberType.Number, Length = 9, Precision = 2, Optional
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
  [Member(Index = 74, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? AsOfDtTotFeeIntColl
  {
    get => Get<decimal?>("asOfDtTotFeeIntColl");
    set => Set("asOfDtTotFeeIntColl", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the AS_OF_DT_TOT_COLL_ALL attribute.
  /// Total collection as of date(applied to all).
  /// </summary>
  [JsonPropertyName("asOfDtTotCollAll")]
  [Member(Index = 75, Type = MemberType.Number, Length = 9, Precision = 2, Optional
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
  [Member(Index = 76, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? LastCollAmt
  {
    get => Get<decimal?>("lastCollAmt");
    set => Set("lastCollAmt", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the LAST_COLL_DT attribute.
  /// The date of the last payment by the obligor or someone on behalf of the 
  /// obligor.
  /// </summary>
  [JsonPropertyName("lastCollDt")]
  [Member(Index = 77, Type = MemberType.Date, Optional = true)]
  public DateTime? LastCollDt
  {
    get => Get<DateTime?>("lastCollDt");
    set => Set("lastCollDt", value);
  }

  /// <summary>
  /// The value of the AS OF DT NF ARR BAL attribute.
  /// Defines the Non-ADC fostercare arrears balance as of date.
  /// </summary>
  [JsonPropertyName("asOfDtNfArrBal")]
  [Member(Index = 78, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? AsOfDtNfArrBal
  {
    get => Get<decimal?>("asOfDtNfArrBal");
    set => Set("asOfDtNfArrBal", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the AS OF DT NF INT BAL attribute.
  /// Defines the Non-ADC fostercare interest balance as of date.
  /// </summary>
  [JsonPropertyName("asOfDtNfIntBal")]
  [Member(Index = 79, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? AsOfDtNfIntBal
  {
    get => Get<decimal?>("asOfDtNfIntBal");
    set => Set("asOfDtNfIntBal", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the AS OF DT FC ARR BAL attribute.
  /// Defines the ADC fostercare arrears balance as of date.
  /// </summary>
  [JsonPropertyName("asOfDtFcArrBal")]
  [Member(Index = 80, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? AsOfDtFcArrBal
  {
    get => Get<decimal?>("asOfDtFcArrBal");
    set => Set("asOfDtFcArrBal", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the AS OF DT FC INT BAL attribute.
  /// Defines the ADC fostercare interest balance as of date.
  /// </summary>
  [JsonPropertyName("asOfDtFcIntBal")]
  [Member(Index = 81, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? AsOfDtFcIntBal
  {
    get => Get<decimal?>("asOfDtFcIntBal");
    set => Set("asOfDtFcIntBal", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TILL DT NF ARR COLL attribute.
  /// Total of all Non-ADC fostercare arrears collection till date.
  /// </summary>
  [JsonPropertyName("tillDtNfArrColl")]
  [Member(Index = 82, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TillDtNfArrColl
  {
    get => Get<decimal?>("tillDtNfArrColl");
    set => Set("tillDtNfArrColl", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TILL DT NF INT COLL attribute.
  /// Total amount of all Non-ADC fostercare interest collection till date.
  /// </summary>
  [JsonPropertyName("tillDtNfIntColl")]
  [Member(Index = 83, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TillDtNfIntColl
  {
    get => Get<decimal?>("tillDtNfIntColl");
    set => Set("tillDtNfIntColl", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TILL DT FC ARR COLL attribute.
  /// Total amount of all ADC fostercare arrears collection till date.
  /// </summary>
  [JsonPropertyName("tillDtFcArrColl")]
  [Member(Index = 84, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TillDtFcArrColl
  {
    get => Get<decimal?>("tillDtFcArrColl");
    set => Set("tillDtFcArrColl", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TILL DT FC INT COLL attribute.
  /// Total amount of all ADC fostercare interest collection till date.
  /// </summary>
  [JsonPropertyName("tillDtFcIntColl")]
  [Member(Index = 85, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TillDtFcIntColl
  {
    get => Get<decimal?>("tillDtFcIntColl");
    set => Set("tillDtFcIntColl", Truncate(value, 2));
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
  [Member(Index = 86, Type = MemberType.Char, Length = CspNumber_MaxLength)]
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
}
