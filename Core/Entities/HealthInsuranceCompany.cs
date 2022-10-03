// The source file: HEALTH_INSURANCE_COMPANY, ID: 371435004, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: OBLGESTB
/// This entity contains the details of Insurance Companies.
/// (Volume of the occurrence of this entity type fixed arbitrarily. needs to be
/// fixed).
/// </summary>
[Serializable]
public partial class HealthInsuranceCompany: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public HealthInsuranceCompany()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public HealthInsuranceCompany(HealthInsuranceCompany that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new HealthInsuranceCompany Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(HealthInsuranceCompany that)
  {
    base.Assign(that);
    insurerPhoneAreaCode = that.insurerPhoneAreaCode;
    insurerFaxAreaCode = that.insurerFaxAreaCode;
    insurerFaxExt = that.insurerFaxExt;
    insurerPhoneExt = that.insurerPhoneExt;
    identifier = that.identifier;
    carrierCode = that.carrierCode;
    insurancePolicyCarrier = that.insurancePolicyCarrier;
    contactName = that.contactName;
    zdelContPerFrst = that.zdelContPerFrst;
    zdelContPerMi = that.zdelContPerMi;
    insurerPhone = that.insurerPhone;
    insurerFax = that.insurerFax;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    startDate = that.startDate;
    endDate = that.endDate;
  }

  /// <summary>
  /// The value of the INSURER_PHONE_AREA_CODE attribute.
  /// The 3-digit area code for the phone number of the insurer.
  /// </summary>
  [JsonPropertyName("insurerPhoneAreaCode")]
  [Member(Index = 1, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? InsurerPhoneAreaCode
  {
    get => insurerPhoneAreaCode;
    set => insurerPhoneAreaCode = value;
  }

  /// <summary>
  /// The value of the INSURER_FAX_AREA_CODE attribute.
  /// The 3-digit area code for the fax number of the insurer.
  /// </summary>
  [JsonPropertyName("insurerFaxAreaCode")]
  [Member(Index = 2, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? InsurerFaxAreaCode
  {
    get => insurerFaxAreaCode;
    set => insurerFaxAreaCode = value;
  }

  /// <summary>Length of the INSURER_FAX_EXT attribute.</summary>
  public const int InsurerFaxExt_MaxLength = 5;

  /// <summary>
  /// The value of the INSURER_FAX_EXT attribute.
  /// This is the 5 digit extension for the fax number of the health insurance 
  /// carrier.
  /// </summary>
  [JsonPropertyName("insurerFaxExt")]
  [Member(Index = 3, Type = MemberType.Char, Length = InsurerFaxExt_MaxLength, Optional
    = true)]
  public string InsurerFaxExt
  {
    get => insurerFaxExt;
    set => insurerFaxExt = value != null
      ? TrimEnd(Substring(value, 1, InsurerFaxExt_MaxLength)) : null;
  }

  /// <summary>Length of the INSURER_PHONE_EXT attribute.</summary>
  public const int InsurerPhoneExt_MaxLength = 5;

  /// <summary>
  /// The value of the INSURER_PHONE_EXT attribute.
  /// The 5 digit extension for phone numbers of health insurance carriers.
  /// </summary>
  [JsonPropertyName("insurerPhoneExt")]
  [Member(Index = 4, Type = MemberType.Char, Length
    = InsurerPhoneExt_MaxLength, Optional = true)]
  public string InsurerPhoneExt
  {
    get => insurerPhoneExt;
    set => insurerPhoneExt = value != null
      ? TrimEnd(Substring(value, 1, InsurerPhoneExt_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// Artificial attribute to uniquely identify the entity occurrence.
  /// </summary>
  [JsonPropertyName("identifier")]
  [DefaultValue(0)]
  [Member(Index = 5, Type = MemberType.Number, Length = 7)]
  public int Identifier
  {
    get => identifier;
    set => identifier = value;
  }

  /// <summary>Length of the CARRIER_CODE attribute.</summary>
  public const int CarrierCode_MaxLength = 7;

  /// <summary>
  /// The value of the CARRIER_CODE attribute.
  /// MEDI carrier code
  /// </summary>
  [JsonPropertyName("carrierCode")]
  [Member(Index = 6, Type = MemberType.Char, Length = CarrierCode_MaxLength, Optional
    = true)]
  public string CarrierCode
  {
    get => carrierCode;
    set => carrierCode = value != null
      ? TrimEnd(Substring(value, 1, CarrierCode_MaxLength)) : null;
  }

  /// <summary>Length of the INSURANCE_POLICY_CARRIER attribute.</summary>
  public const int InsurancePolicyCarrier_MaxLength = 45;

  /// <summary>
  /// The value of the INSURANCE_POLICY_CARRIER attribute.
  /// Name of the insurance company that is carrying the insurance policy.
  /// </summary>
  [JsonPropertyName("insurancePolicyCarrier")]
  [Member(Index = 7, Type = MemberType.Char, Length
    = InsurancePolicyCarrier_MaxLength, Optional = true)]
  public string InsurancePolicyCarrier
  {
    get => insurancePolicyCarrier;
    set => insurancePolicyCarrier = value != null
      ? TrimEnd(Substring(value, 1, InsurancePolicyCarrier_MaxLength)) : null;
  }

  /// <summary>Length of the CONTACT_NAME attribute.</summary>
  public const int ContactName_MaxLength = 40;

  /// <summary>
  /// The value of the CONTACT_NAME attribute.
  /// The name of the contact person at the insurer.
  /// </summary>
  [JsonPropertyName("contactName")]
  [Member(Index = 8, Type = MemberType.Char, Length = ContactName_MaxLength, Optional
    = true)]
  public string ContactName
  {
    get => contactName;
    set => contactName = value != null
      ? TrimEnd(Substring(value, 1, ContactName_MaxLength)) : null;
  }

  /// <summary>Length of the ZDEL_CONT_PER_FRST attribute.</summary>
  public const int ZdelContPerFrst_MaxLength = 12;

  /// <summary>
  /// The value of the ZDEL_CONT_PER_FRST attribute.
  /// The given name of the contact person within the health insurance company.
  /// </summary>
  [JsonPropertyName("zdelContPerFrst")]
  [Member(Index = 9, Type = MemberType.Char, Length
    = ZdelContPerFrst_MaxLength, Optional = true)]
  public string ZdelContPerFrst
  {
    get => zdelContPerFrst;
    set => zdelContPerFrst = value != null
      ? TrimEnd(Substring(value, 1, ZdelContPerFrst_MaxLength)) : null;
  }

  /// <summary>Length of the ZDEL_CONT_PER_MI attribute.</summary>
  public const int ZdelContPerMi_MaxLength = 1;

  /// <summary>
  /// The value of the ZDEL_CONT_PER_MI attribute.
  /// The first letter of the middle name of the contact person within the 
  /// health insurance company.
  /// </summary>
  [JsonPropertyName("zdelContPerMi")]
  [Member(Index = 10, Type = MemberType.Char, Length
    = ZdelContPerMi_MaxLength, Optional = true)]
  public string ZdelContPerMi
  {
    get => zdelContPerMi;
    set => zdelContPerMi = value != null
      ? TrimEnd(Substring(value, 1, ZdelContPerMi_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the INSURER_PHONE attribute.
  /// The phone number of a contact at the insurance company that can relate 
  /// information concerning the insurance policy in question.
  /// The phone number is specified as 7 digit phone number.
  /// </summary>
  [JsonPropertyName("insurerPhone")]
  [Member(Index = 11, Type = MemberType.Number, Length = 7, Optional = true)]
  public int? InsurerPhone
  {
    get => insurerPhone;
    set => insurerPhone = value;
  }

  /// <summary>
  /// The value of the INSURER_FAX attribute.
  /// The 7-digit fax number of the insurer.
  /// </summary>
  [JsonPropertyName("insurerFax")]
  [Member(Index = 12, Type = MemberType.Number, Length = 7, Optional = true)]
  public int? InsurerFax
  {
    get => insurerFax;
    set => insurerFax = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// User ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  [Member(Index = 14, Type = MemberType.Timestamp)]
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
  [Member(Index = 15, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength)]
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
  [Member(Index = 16, Type = MemberType.Timestamp)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  /// <summary>
  /// The value of the START_DATE attribute.
  /// Date that the Health insurance Company activated.
  /// </summary>
  [JsonPropertyName("startDate")]
  [Member(Index = 17, Type = MemberType.Date)]
  public DateTime? StartDate
  {
    get => startDate;
    set => startDate = value;
  }

  /// <summary>
  /// The value of the END_DATE attribute.
  /// The date upon which this occurrence of the entity can no longer be used.
  /// </summary>
  [JsonPropertyName("endDate")]
  [Member(Index = 18, Type = MemberType.Date)]
  public DateTime? EndDate
  {
    get => endDate;
    set => endDate = value;
  }

  private int? insurerPhoneAreaCode;
  private int? insurerFaxAreaCode;
  private string insurerFaxExt;
  private string insurerPhoneExt;
  private int identifier;
  private string carrierCode;
  private string insurancePolicyCarrier;
  private string contactName;
  private string zdelContPerFrst;
  private string zdelContPerMi;
  private int? insurerPhone;
  private int? insurerFax;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private DateTime? startDate;
  private DateTime? endDate;
}
