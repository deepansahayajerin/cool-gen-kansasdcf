// The source file: IM_HOUSEHOLD_MBR_MNTHLY_SUM, ID: 374416406, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP:  OBLGESTB
/// 
/// This entity tracks member participation in IM Households by month.
/// Montly grants, adjustments and URA are stored by IM Household and
/// person.
/// </summary>
[Serializable]
public partial class ImHouseholdMbrMnthlySum: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ImHouseholdMbrMnthlySum()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ImHouseholdMbrMnthlySum(ImHouseholdMbrMnthlySum that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ImHouseholdMbrMnthlySum Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ImHouseholdMbrMnthlySum that)
  {
    base.Assign(that);
    year = that.year;
    month = that.month;
    relationship = that.relationship;
    grantAmount = that.grantAmount;
    grantMedicalAmount = that.grantMedicalAmount;
    uraAmount = that.uraAmount;
    uraMedicalAmount = that.uraMedicalAmount;
    createdBy = that.createdBy;
    createdTmst = that.createdTmst;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTmst = that.lastUpdatedTmst;
    cspNumber = that.cspNumber;
    imhAeCaseNo = that.imhAeCaseNo;
  }

  /// <summary>
  /// The value of the YEAR attribute.
  /// The year in which the member received some for of grant
  /// </summary>
  [JsonPropertyName("year")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 4)]
  public int Year
  {
    get => year;
    set => year = value;
  }

  /// <summary>
  /// The value of the MONTH attribute.
  /// The month in which the member received some for of grant.
  /// </summary>
  [JsonPropertyName("month")]
  [DefaultValue(0)]
  [Member(Index = 2, Type = MemberType.Number, Length = 2)]
  public int Month
  {
    get => month;
    set => month = value;
  }

  /// <summary>Length of the RELATIONSHIP attribute.</summary>
  public const int Relationship_MaxLength = 2;

  /// <summary>
  /// The value of the RELATIONSHIP attribute.
  /// This attribute describes the relationship of the household member to the 
  /// Primary Information Person.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = Relationship_MaxLength)]
  public string Relationship
  {
    get => relationship ?? "";
    set => relationship = TrimEnd(Substring(value, 1, Relationship_MaxLength));
  }

  /// <summary>
  /// The json value of the Relationship attribute.</summary>
  [JsonPropertyName("relationship")]
  [Computed]
  public string Relationship_Json
  {
    get => NullIf(Relationship, "");
    set => Relationship = value;
  }

  /// <summary>
  /// The value of the GRANT_AMOUNT attribute.
  /// This attribute will contain the total amount of grants disbursed by AE for
  /// a specific year/month by member (cse_person).
  /// </summary>
  [JsonPropertyName("grantAmount")]
  [Member(Index = 4, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? GrantAmount
  {
    get => grantAmount;
    set => grantAmount = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the GRANT_MEDICAL_AMOUNT attribute.
  /// This attribute will contain the total amount of medical grants disbursed 
  /// for a specific year/month by member (cse_person).
  /// </summary>
  [JsonPropertyName("grantMedicalAmount")]
  [Member(Index = 5, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? GrantMedicalAmount
  {
    get => grantMedicalAmount;
    set => grantMedicalAmount = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the URA_AMOUNT attribute.
  /// This attribute will contain the computed URA amount for the household. 
  /// Grants, positive URA adjustments and associated collection adjustments
  /// will increase the URA amount while collections and negative URA
  /// adjustments will decrease the URA amount. This amount is NOT cumulative.
  /// It represents the exact URA amount for that specific year/month only. If a
  /// process needs to know the total URA for a household, it will sum all year
  /// /month occurrences.
  /// </summary>
  [JsonPropertyName("uraAmount")]
  [Member(Index = 6, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? UraAmount
  {
    get => uraAmount;
    set => uraAmount = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the URA_MEDICAL_AMOUNT attribute.
  /// This attribute will contain the computed URA medical amount for the 
  /// household. Medical grants, positive medical URA adjustments and associated
  /// medical collection adjustments will increase the medical URA amount while
  /// medical collections and negative medical URA adjustments will decrease
  /// the medical URA amount. This amount is NOT cumulative. It represents the
  /// exact medical URA amount for that specific year/month only.  If a process
  /// needs to know the total medical URA for a household, it will sum all year/
  /// month occurrences.
  /// </summary>
  [JsonPropertyName("uraMedicalAmount")]
  [Member(Index = 7, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? UraMedicalAmount
  {
    get => uraMedicalAmount;
    set => uraMedicalAmount = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// Standard Definition.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// The value of the CREATED_TMST attribute.
  /// Standard Definition.
  /// </summary>
  [JsonPropertyName("createdTmst")]
  [Member(Index = 9, Type = MemberType.Timestamp)]
  public DateTime? CreatedTmst
  {
    get => createdTmst;
    set => createdTmst = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// Standard Definition.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 10, Type = MemberType.Char, Length
    = LastUpdatedBy_MaxLength, Optional = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TMST attribute.
  /// Standard Definition.
  /// </summary>
  [JsonPropertyName("lastUpdatedTmst")]
  [Member(Index = 11, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTmst
  {
    get => lastUpdatedTmst;
    set => lastUpdatedTmst = value;
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
  [Member(Index = 12, Type = MemberType.Char, Length = CspNumber_MaxLength)]
  public string CspNumber
  {
    get => cspNumber ?? "";
    set => cspNumber = TrimEnd(Substring(value, 1, CspNumber_MaxLength));
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

  /// <summary>Length of the AE_CASE_NO attribute.</summary>
  public const int ImhAeCaseNo_MaxLength = 8;

  /// <summary>
  /// The value of the AE_CASE_NO attribute.
  /// This attribute defines the AE Case No for the IM Household.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length = ImhAeCaseNo_MaxLength)]
  public string ImhAeCaseNo
  {
    get => imhAeCaseNo ?? "";
    set => imhAeCaseNo = TrimEnd(Substring(value, 1, ImhAeCaseNo_MaxLength));
  }

  /// <summary>
  /// The json value of the ImhAeCaseNo attribute.</summary>
  [JsonPropertyName("imhAeCaseNo")]
  [Computed]
  public string ImhAeCaseNo_Json
  {
    get => NullIf(ImhAeCaseNo, "");
    set => ImhAeCaseNo = value;
  }

  private int year;
  private int month;
  private string relationship;
  private decimal? grantAmount;
  private decimal? grantMedicalAmount;
  private decimal? uraAmount;
  private decimal? uraMedicalAmount;
  private string createdBy;
  private DateTime? createdTmst;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTmst;
  private string cspNumber;
  private string imhAeCaseNo;
}
