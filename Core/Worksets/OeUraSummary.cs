// The source file: OE_URA_SUMMARY, ID: 372637211, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

/// <summary>
/// RESP: OBLGESTB
/// This attribute set contains URA related summary figures (cumulative or 
/// monthly ..)
/// 	Grant
/// 	Passthru
/// 	Child Support Collection
/// 	Spousal Support Collection
/// 	Medical Support Collection
/// 	Medical URA
/// 	Adjustment to ADC Grant
/// 	Adjustment to FC Grant
/// 	Adjustment to passthru
/// 	Adjustment to Medical Assistance
/// 	Adjustment to Household URA
/// This workset has been created because the sizes of Monthly Summary entities 
/// may not be adequate for Cumulative Totals.
/// </summary>
[Serializable]
public partial class OeUraSummary: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public OeUraSummary()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public OeUraSummary(OeUraSummary that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new OeUraSummary Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(OeUraSummary that)
  {
    base.Assign(that);
    grant = that.grant;
    passthru = that.passthru;
    childSupportCollection = that.childSupportCollection;
    spousalSupportCollection = that.spousalSupportCollection;
    ura = that.ura;
    uraExcess = that.uraExcess;
    medicalAssistance = that.medicalAssistance;
    medicalSupportCollection = that.medicalSupportCollection;
    medicalUra = that.medicalUra;
    adjAdcGrant = that.adjAdcGrant;
    adjPassthru = that.adjPassthru;
    adjFcGrant = that.adjFcGrant;
    adjMedAssistance = that.adjMedAssistance;
    adjHouseholdUra = that.adjHouseholdUra;
    adjMediUra = that.adjMediUra;
    adjUnused1 = that.adjUnused1;
    adjUnused2 = that.adjUnused2;
    adjUnused3 = that.adjUnused3;
  }

  /// <summary>
  /// The value of the GRANT attribute.
  /// This attribute contains grant paid.
  /// </summary>
  [JsonPropertyName("grant")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 1, Type = MemberType.Number, Length = 8, Precision = 2)]
  public decimal Grant
  {
    get => grant;
    set => grant = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the PASSTHRU attribute.
  /// This attribute contains passthru amount paid
  /// </summary>
  [JsonPropertyName("passthru")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 2, Type = MemberType.Number, Length = 8, Precision = 2)]
  public decimal Passthru
  {
    get => passthru;
    set => passthru = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the CHILD_SUPPORT_COLLECTION attribute.
  /// This attribute contains child support collection.
  /// </summary>
  [JsonPropertyName("childSupportCollection")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 3, Type = MemberType.Number, Length = 8, Precision = 2)]
  public decimal ChildSupportCollection
  {
    get => childSupportCollection;
    set => childSupportCollection = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the SPOUSAL_SUPPORT_COLLECTION attribute.
  /// This attribute contains spousal  support collection.
  /// </summary>
  [JsonPropertyName("spousalSupportCollection")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 4, Type = MemberType.Number, Length = 8, Precision = 2)]
  public decimal SpousalSupportCollection
  {
    get => spousalSupportCollection;
    set => spousalSupportCollection = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the URA attribute.
  /// This attribute contains URA (Grant + passthru - child support collection -
  /// spousal support collection)
  /// </summary>
  [JsonPropertyName("ura")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 5, Type = MemberType.Number, Length = 8, Precision = 2)]
  public decimal Ura
  {
    get => ura;
    set => ura = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the URA_EXCESS attribute.
  /// </summary>
  [JsonPropertyName("uraExcess")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 6, Type = MemberType.Number, Length = 8, Precision = 2)]
  public decimal UraExcess
  {
    get => uraExcess;
    set => uraExcess = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the MEDICAL_ASSISTANCE attribute.
  /// This field contains the medical assistance paid
  /// </summary>
  [JsonPropertyName("medicalAssistance")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 7, Type = MemberType.Number, Length = 8, Precision = 2)]
  public decimal MedicalAssistance
  {
    get => medicalAssistance;
    set => medicalAssistance = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the MEDICAL_SUPPORT_COLLECTION attribute.
  /// This attribute contains the medical support collection.
  /// </summary>
  [JsonPropertyName("medicalSupportCollection")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 8, Type = MemberType.Number, Length = 8, Precision = 2)]
  public decimal MedicalSupportCollection
  {
    get => medicalSupportCollection;
    set => medicalSupportCollection = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the MEDICAL_URA attribute.
  /// This attribute contains medical URA (Medical assistance paid - medical 
  /// support collection).
  /// </summary>
  [JsonPropertyName("medicalUra")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 9, Type = MemberType.Number, Length = 8, Precision = 2)]
  public decimal MedicalUra
  {
    get => medicalUra;
    set => medicalUra = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the ADJ_ADC_GRANT attribute.
  /// This attribute specifies the adjustment to ADC grant amount. It can be 
  /// positive or negative
  /// </summary>
  [JsonPropertyName("adjAdcGrant")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 10, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal AdjAdcGrant
  {
    get => adjAdcGrant;
    set => adjAdcGrant = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the ADJ PASSTHRU attribute.
  /// This attribute specifies the adjustment to passthru amount. It can be 
  /// positive or negative
  /// </summary>
  [JsonPropertyName("adjPassthru")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 11, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal AdjPassthru
  {
    get => adjPassthru;
    set => adjPassthru = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the ADJ FC GRANT attribute.
  /// This attribute specifies the adjustment to FC grant amount. It can be 
  /// positive or negative
  /// </summary>
  [JsonPropertyName("adjFcGrant")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 12, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal AdjFcGrant
  {
    get => adjFcGrant;
    set => adjFcGrant = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the ADJ MED ASSISTANCE attribute.
  /// This attribute specifies the adjustment to medical assistance paid. It can
  /// be positive or negative
  /// </summary>
  [JsonPropertyName("adjMedAssistance")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 13, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal AdjMedAssistance
  {
    get => adjMedAssistance;
    set => adjMedAssistance = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the ADJ HOUSEHOLD URA attribute.
  /// This attribute specifies the adjustment to household URA. It can be 
  /// positive or negative.
  /// This is the amount of (excess) collection that is applied to another 
  /// household's URA
  /// </summary>
  [JsonPropertyName("adjHouseholdUra")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 14, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal AdjHouseholdUra
  {
    get => adjHouseholdUra;
    set => adjHouseholdUra = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the ADJ MEDI URA attribute.
  /// This attribute specifies the adjustment to household URA. It can be 
  /// positive or negative.
  /// This is the amount of (excess) collection that is applied to another 
  /// household's URA
  /// </summary>
  [JsonPropertyName("adjMediUra")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 15, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal AdjMediUra
  {
    get => adjMediUra;
    set => adjMediUra = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the ADJ UNUSED 1 attribute.
  /// This attribute specifies the adjustment to ... amount. It can be positive 
  /// or negative.
  /// This is an unused field for future use.
  /// </summary>
  [JsonPropertyName("adjUnused1")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 16, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal AdjUnused1
  {
    get => adjUnused1;
    set => adjUnused1 = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the ADJ UNUSED 2 attribute.
  /// This attribute specifies the adjustment to ... amount. It can be positive 
  /// or negative.
  /// This is an unused field for future use.
  /// </summary>
  [JsonPropertyName("adjUnused2")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 17, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal AdjUnused2
  {
    get => adjUnused2;
    set => adjUnused2 = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the ADJ UNUSED 3 attribute.
  /// This attribute specifies the adjustment to ... amount. It can be positive 
  /// or negative.
  /// This is an unused field for future use.
  /// </summary>
  [JsonPropertyName("adjUnused3")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 18, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal AdjUnused3
  {
    get => adjUnused3;
    set => adjUnused3 = Truncate(value, 2);
  }

  private decimal grant;
  private decimal passthru;
  private decimal childSupportCollection;
  private decimal spousalSupportCollection;
  private decimal ura;
  private decimal uraExcess;
  private decimal medicalAssistance;
  private decimal medicalSupportCollection;
  private decimal medicalUra;
  private decimal adjAdcGrant;
  private decimal adjPassthru;
  private decimal adjFcGrant;
  private decimal adjMedAssistance;
  private decimal adjHouseholdUra;
  private decimal adjMediUra;
  private decimal adjUnused1;
  private decimal adjUnused2;
  private decimal adjUnused3;
}
