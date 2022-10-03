// The source file: CASE_ROLE, ID: 371430945, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVINIT
/// A reason for association of a person with a case.
/// Example: AP, AR, CHILD, etc.
/// </summary>
[Serializable]
public partial class CaseRole: Heap, ICloneable
{
  /// <summary>Default constructor.</summary>
  public CaseRole()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public CaseRole(CaseRole that):
    base(that)
  {
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new CaseRole Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// The unique identifier.  System generated using a 3-digit random generator
  /// </summary>
  [JsonPropertyName("identifier")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 3)]
  public int Identifier
  {
    get => Get<int?>("identifier") ?? 0;
    set => Set("identifier", value == 0 ? null : value as int?);
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TIMESTAMP attribute.
  /// Timestamp of last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTimestamp")]
  [Member(Index = 2, Type = MemberType.Timestamp, Optional = true)]
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
  [Member(Index = 3, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength, Optional
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
  [Member(Index = 4, Type = MemberType.Timestamp)]
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
  [Member(Index = 5, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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

  /// <summary>Length of the TYPE attribute.</summary>
  public const int Type1_MaxLength = 2;

  /// <summary>
  /// The value of the TYPE attribute.
  /// The type of role played by a CSE Person in a given case.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = Type1_MaxLength)]
  [Value("MO")]
  [Value("FA")]
  [Value("CH")]
  [Value("AR")]
  [Value("AP")]
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
  /// The value of the START_DATE attribute.
  /// The date that the CSE Person takes on a role in a Case
  /// </summary>
  [JsonPropertyName("startDate")]
  [Member(Index = 7, Type = MemberType.Date, Optional = true)]
  public DateTime? StartDate
  {
    get => Get<DateTime?>("startDate");
    set => Set("startDate", value);
  }

  /// <summary>
  /// The value of the END_DATE attribute.
  /// The date a CSE Person ceases to play a given role within a given Case
  /// </summary>
  [JsonPropertyName("endDate")]
  [Member(Index = 8, Type = MemberType.Date, Optional = true)]
  public DateTime? EndDate
  {
    get => Get<DateTime?>("endDate");
    set => Set("endDate", value);
  }

  /// <summary>Length of the NOTE attribute.</summary>
  public const int Note_MaxLength = 78;

  /// <summary>
  /// The value of the NOTE attribute.
  /// Free-form text for additional information.
  /// </summary>
  [JsonPropertyName("note")]
  [Member(Index = 9, Type = MemberType.Varchar, Length = Note_MaxLength, Optional
    = true)]
  public string Note
  {
    get => Get<string>("note");
    set => Set("note", Substring(value, 1, Note_MaxLength));
  }

  /// <summary>Length of the ON_SS_IND attribute.</summary>
  public const int OnSsInd_MaxLength = 1;

  /// <summary>
  /// The value of the ON_SS_IND attribute.
  /// Indicates whether a CSE Person is receiving social security benefits
  /// </summary>
  [JsonPropertyName("onSsInd")]
  [Member(Index = 10, Type = MemberType.Char, Length = OnSsInd_MaxLength, Optional
    = true)]
  public string OnSsInd
  {
    get => Get<string>("onSsInd");
    set => Set("onSsInd", TrimEnd(Substring(value, 1, OnSsInd_MaxLength)));
  }

  /// <summary>Length of the HEALTH_INSURANCE_INDICATOR attribute.</summary>
  public const int HealthInsuranceIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the HEALTH_INSURANCE_INDICATOR attribute.
  /// This indicates whether the person is covered by health insurance.
  /// Applicable to AR and Child only
  /// </summary>
  [JsonPropertyName("healthInsuranceIndicator")]
  [Member(Index = 11, Type = MemberType.Char, Length
    = HealthInsuranceIndicator_MaxLength, Optional = true)]
  public string HealthInsuranceIndicator
  {
    get => Get<string>("healthInsuranceIndicator");
    set => Set(
      "healthInsuranceIndicator", TrimEnd(Substring(value, 1,
      HealthInsuranceIndicator_MaxLength)));
  }

  /// <summary>Length of the MEDICAL_SUPPORT_INDICATOR attribute.</summary>
  public const int MedicalSupportIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the MEDICAL_SUPPORT_INDICATOR attribute.
  /// Indicates whether or not the person is receiving medical support.
  /// Applicable to AR and Child only
  /// </summary>
  [JsonPropertyName("medicalSupportIndicator")]
  [Member(Index = 12, Type = MemberType.Char, Length
    = MedicalSupportIndicator_MaxLength, Optional = true)]
  public string MedicalSupportIndicator
  {
    get => Get<string>("medicalSupportIndicator");
    set => Set(
      "medicalSupportIndicator", TrimEnd(Substring(value, 1,
      MedicalSupportIndicator_MaxLength)));
  }

  /// <summary>Length of the MOTHERS_FIRST_NAME attribute.</summary>
  public const int MothersFirstName_MaxLength = 12;

  /// <summary>
  /// The value of the MOTHERS_FIRST_NAME attribute.
  /// The first name of the AP's mother
  /// </summary>
  [JsonPropertyName("mothersFirstName")]
  [Member(Index = 13, Type = MemberType.Char, Length
    = MothersFirstName_MaxLength, Optional = true)]
  public string MothersFirstName
  {
    get => Get<string>("mothersFirstName");
    set => Set(
      "mothersFirstName",
      TrimEnd(Substring(value, 1, MothersFirstName_MaxLength)));
  }

  /// <summary>Length of the MOTHERS_MIDDLE_INITIAL attribute.</summary>
  public const int MothersMiddleInitial_MaxLength = 1;

  /// <summary>
  /// The value of the MOTHERS_MIDDLE_INITIAL attribute.
  /// The middle initial of the AP's mother
  /// </summary>
  [JsonPropertyName("mothersMiddleInitial")]
  [Member(Index = 14, Type = MemberType.Char, Length
    = MothersMiddleInitial_MaxLength, Optional = true)]
  public string MothersMiddleInitial
  {
    get => Get<string>("mothersMiddleInitial");
    set => Set(
      "mothersMiddleInitial", TrimEnd(Substring(value, 1,
      MothersMiddleInitial_MaxLength)));
  }

  /// <summary>Length of the FATHERS_LAST_NAME attribute.</summary>
  public const int FathersLastName_MaxLength = 17;

  /// <summary>
  /// The value of the FATHERS_LAST_NAME attribute.
  /// The last name of the AP's father
  /// </summary>
  [JsonPropertyName("fathersLastName")]
  [Member(Index = 15, Type = MemberType.Char, Length
    = FathersLastName_MaxLength, Optional = true)]
  public string FathersLastName
  {
    get => Get<string>("fathersLastName");
    set => Set(
      "fathersLastName",
      TrimEnd(Substring(value, 1, FathersLastName_MaxLength)));
  }

  /// <summary>Length of the FATHERS_MIDDLE_INITIAL attribute.</summary>
  public const int FathersMiddleInitial_MaxLength = 1;

  /// <summary>
  /// The value of the FATHERS_MIDDLE_INITIAL attribute.
  /// The middle initial of the AP's father
  /// </summary>
  [JsonPropertyName("fathersMiddleInitial")]
  [Member(Index = 16, Type = MemberType.Char, Length
    = FathersMiddleInitial_MaxLength, Optional = true)]
  public string FathersMiddleInitial
  {
    get => Get<string>("fathersMiddleInitial");
    set => Set(
      "fathersMiddleInitial", TrimEnd(Substring(value, 1,
      FathersMiddleInitial_MaxLength)));
  }

  /// <summary>Length of the FATHERS_FIRST_NAME attribute.</summary>
  public const int FathersFirstName_MaxLength = 12;

  /// <summary>
  /// The value of the FATHERS_FIRST_NAME attribute.
  /// The first name of the AP's father
  /// </summary>
  [JsonPropertyName("fathersFirstName")]
  [Member(Index = 17, Type = MemberType.Char, Length
    = FathersFirstName_MaxLength, Optional = true)]
  public string FathersFirstName
  {
    get => Get<string>("fathersFirstName");
    set => Set(
      "fathersFirstName",
      TrimEnd(Substring(value, 1, FathersFirstName_MaxLength)));
  }

  /// <summary>Length of the MOTHERS_MAIDEN_LAST_NAME attribute.</summary>
  public const int MothersMaidenLastName_MaxLength = 17;

  /// <summary>
  /// The value of the MOTHERS_MAIDEN_LAST_NAME attribute.
  /// The last name of the Absent Parent's Mother before she was married.
  /// </summary>
  [JsonPropertyName("mothersMaidenLastName")]
  [Member(Index = 18, Type = MemberType.Char, Length
    = MothersMaidenLastName_MaxLength, Optional = true)]
  public string MothersMaidenLastName
  {
    get => Get<string>("mothersMaidenLastName");
    set => Set(
      "mothersMaidenLastName", TrimEnd(Substring(value, 1,
      MothersMaidenLastName_MaxLength)));
  }

  /// <summary>Length of the PARENT_TYPE attribute.</summary>
  public const int ParentType_MaxLength = 2;

  /// <summary>
  /// The value of the PARENT_TYPE attribute.
  /// Current understanding of the AP's role as regards the children:
  /// Alleged Father
  /// Putative Father
  /// Acknowledged Father
  /// Mother
  /// </summary>
  [JsonPropertyName("parentType")]
  [Member(Index = 19, Type = MemberType.Char, Length = ParentType_MaxLength, Optional
    = true)]
  [Value(null)]
  [Value("AF")]
  [Value("PF")]
  [Value("M")]
  [Value("F")]
  public string ParentType
  {
    get => Get<string>("parentType");
    set =>
      Set("parentType", TrimEnd(Substring(value, 1, ParentType_MaxLength)));
  }

  /// <summary>
  /// The value of the NOTIFIED_DATE attribute.
  /// Date the AP was notified of his/her involvement with a case.
  /// </summary>
  [JsonPropertyName("notifiedDate")]
  [Member(Index = 20, Type = MemberType.Date, Optional = true)]
  public DateTime? NotifiedDate
  {
    get => Get<DateTime?>("notifiedDate");
    set => Set("notifiedDate", value);
  }

  /// <summary>
  /// The value of the NUMBER_OF_CHILDREN attribute.
  /// The number of children this AP has in total.
  /// </summary>
  [JsonPropertyName("numberOfChildren")]
  [Member(Index = 21, Type = MemberType.Number, Length = 2, Optional = true)]
  [ImplicitValue(0)]
  public int? NumberOfChildren
  {
    get => Get<int?>("numberOfChildren");
    set => Set("numberOfChildren", value);
  }

  /// <summary>Length of the LIVING_WITH_AR_INDICATOR attribute.</summary>
  public const int LivingWithArIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the LIVING_WITH_AR_INDICATOR attribute.
  /// An indication whether the AP is living with the AR on the case.
  /// </summary>
  [JsonPropertyName("livingWithArIndicator")]
  [Member(Index = 22, Type = MemberType.Char, Length
    = LivingWithArIndicator_MaxLength, Optional = true)]
  [Value(null)]
  [Value("N")]
  [Value("Y")]
  [ImplicitValue("N")]
  public string LivingWithArIndicator
  {
    get => Get<string>("livingWithArIndicator");
    set => Set(
      "livingWithArIndicator", TrimEnd(Substring(value, 1,
      LivingWithArIndicator_MaxLength)));
  }

  /// <summary>Length of the NONPAYMENT_CATEGORY attribute.</summary>
  public const int NonpaymentCategory_MaxLength = 1;

  /// <summary>
  /// The value of the NONPAYMENT_CATEGORY attribute.
  /// Category of reasons for collection record.
  /// Possibles:
  /// - Collections are expected; IWO in place
  ///   and/or regular collection history
  /// - Court Order exists but no collections; No
  ///   regular collection history, may or may not
  ///   have IWO, sporadic or partial collections,
  ///   UI
  /// - Excluded SDSO or FDSO.  These are not
  ///   regular collections, one time or irregular.
  /// - No locations on AP
  /// - collections in compliance, no IWO.
  /// </summary>
  [JsonPropertyName("nonpaymentCategory")]
  [Member(Index = 23, Type = MemberType.Char, Length
    = NonpaymentCategory_MaxLength, Optional = true)]
  public string NonpaymentCategory
  {
    get => Get<string>("nonpaymentCategory");
    set => Set(
      "nonpaymentCategory", TrimEnd(Substring(value, 1,
      NonpaymentCategory_MaxLength)));
  }

  /// <summary>Length of the CONFIRMED_TYPE attribute.</summary>
  public const int ConfirmedType_MaxLength = 1;

  /// <summary>
  /// The value of the CONFIRMED_TYPE attribute.
  /// This attributes describes the kind of confirmed father,
  /// 	(e.g., L - Legal,
  /// 	       B - Biological,
  /// 	       A - Adoptive)	
  /// </summary>
  [JsonPropertyName("confirmedType")]
  [Member(Index = 24, Type = MemberType.Char, Length
    = ConfirmedType_MaxLength, Optional = true)]
  public string ConfirmedType
  {
    get => Get<string>("confirmedType");
    set => Set(
      "confirmedType", TrimEnd(Substring(value, 1, ConfirmedType_MaxLength)));
  }

  /// <summary>Length of the ABSENCE_REASON_CODE attribute.</summary>
  public const int AbsenceReasonCode_MaxLength = 2;

  /// <summary>
  /// The value of the ABSENCE_REASON_CODE attribute.
  /// The reason a CSE Person is absent
  /// </summary>
  [JsonPropertyName("absenceReasonCode")]
  [Member(Index = 25, Type = MemberType.Char, Length
    = AbsenceReasonCode_MaxLength, Optional = true)]
  public string AbsenceReasonCode
  {
    get => Get<string>("absenceReasonCode");
    set => Set(
      "absenceReasonCode", TrimEnd(Substring(value, 1,
      AbsenceReasonCode_MaxLength)));
  }

  /// <summary>Length of the BIRTH_CERT_FATHERS_MI attribute.</summary>
  public const int BirthCertFathersMi_MaxLength = 1;

  /// <summary>
  /// The value of the BIRTH_CERT_FATHERS_MI attribute.
  /// Fathers middle initial on the birth certificate.
  /// </summary>
  [JsonPropertyName("birthCertFathersMi")]
  [Member(Index = 26, Type = MemberType.Char, Length
    = BirthCertFathersMi_MaxLength, Optional = true)]
  public string BirthCertFathersMi
  {
    get => Get<string>("birthCertFathersMi");
    set => Set(
      "birthCertFathersMi", TrimEnd(Substring(value, 1,
      BirthCertFathersMi_MaxLength)));
  }

  /// <summary>Length of the BIRTH_CERT_FATHERS_FIRST_NAME attribute.</summary>
  public const int BirthCertFathersFirstName_MaxLength = 12;

  /// <summary>
  /// The value of the BIRTH_CERT_FATHERS_FIRST_NAME attribute.
  /// First name of father on birth certificate.
  /// </summary>
  [JsonPropertyName("birthCertFathersFirstName")]
  [Member(Index = 27, Type = MemberType.Char, Length
    = BirthCertFathersFirstName_MaxLength, Optional = true)]
  public string BirthCertFathersFirstName
  {
    get => Get<string>("birthCertFathersFirstName");
    set => Set(
      "birthCertFathersFirstName", TrimEnd(Substring(value, 1,
      BirthCertFathersFirstName_MaxLength)));
  }

  /// <summary>
  /// The value of the PRIOR_MEDICAL_SUPPORT attribute.
  /// The amount of prior medical support provided for this child.
  /// </summary>
  [JsonPropertyName("priorMedicalSupport")]
  [Member(Index = 28, Type = MemberType.Number, Length = 10, Precision = 2, Optional
    = true)]
  public decimal? PriorMedicalSupport
  {
    get => Get<decimal?>("priorMedicalSupport");
    set => Set("priorMedicalSupport", Truncate(value, 2));
  }

  /// <summary>Length of the BIRTH_CERTIFICATE_SIGNATURE attribute.</summary>
  public const int BirthCertificateSignature_MaxLength = 1;

  /// <summary>
  /// The value of the BIRTH_CERTIFICATE_SIGNATURE attribute.
  /// Indicates whether or not the father's signature is on the birth 
  /// certificate
  /// </summary>
  [JsonPropertyName("birthCertificateSignature")]
  [Member(Index = 29, Type = MemberType.Char, Length
    = BirthCertificateSignature_MaxLength, Optional = true)]
  public string BirthCertificateSignature
  {
    get => Get<string>("birthCertificateSignature");
    set => Set(
      "birthCertificateSignature", TrimEnd(Substring(value, 1,
      BirthCertificateSignature_MaxLength)));
  }

  /// <summary>Length of the AR_WAIVED_INSURANCE attribute.</summary>
  public const int ArWaivedInsurance_MaxLength = 1;

  /// <summary>
  /// The value of the AR_WAIVED_INSURANCE attribute.
  /// The AR chose to waive insurance for this child.
  /// </summary>
  [JsonPropertyName("arWaivedInsurance")]
  [Member(Index = 30, Type = MemberType.Char, Length
    = ArWaivedInsurance_MaxLength, Optional = true)]
  public string ArWaivedInsurance
  {
    get => Get<string>("arWaivedInsurance");
    set => Set(
      "arWaivedInsurance", TrimEnd(Substring(value, 1,
      ArWaivedInsurance_MaxLength)));
  }

  /// <summary>Length of the BIRTH_CERT_FATHERS_LAST_NAME attribute.</summary>
  public const int BirthCertFathersLastName_MaxLength = 17;

  /// <summary>
  /// The value of the BIRTH_CERT_FATHERS_LAST_NAME attribute.
  /// Last name of father on birth certificate.
  /// </summary>
  [JsonPropertyName("birthCertFathersLastName")]
  [Member(Index = 31, Type = MemberType.Char, Length
    = BirthCertFathersLastName_MaxLength, Optional = true)]
  public string BirthCertFathersLastName
  {
    get => Get<string>("birthCertFathersLastName");
    set => Set(
      "birthCertFathersLastName", TrimEnd(Substring(value, 1,
      BirthCertFathersLastName_MaxLength)));
  }

  /// <summary>Length of the BORN_OUT_OF_WEDLOCK attribute.</summary>
  public const int BornOutOfWedlock_MaxLength = 1;

  /// <summary>
  /// The value of the BORN_OUT_OF_WEDLOCK attribute.
  /// The child was born out of wedlock.
  /// </summary>
  [JsonPropertyName("bornOutOfWedlock")]
  [Member(Index = 32, Type = MemberType.Char, Length
    = BornOutOfWedlock_MaxLength, Optional = true)]
  public string BornOutOfWedlock
  {
    get => Get<string>("bornOutOfWedlock");
    set => Set(
      "bornOutOfWedlock",
      TrimEnd(Substring(value, 1, BornOutOfWedlock_MaxLength)));
  }

  /// <summary>
  /// The value of the DATE_OF_EMANCIPATION attribute.
  /// Date the child is no longer considered a child for CSE purposes.
  /// </summary>
  [JsonPropertyName("dateOfEmancipation")]
  [Member(Index = 33, Type = MemberType.Date, Optional = true)]
  public DateTime? DateOfEmancipation
  {
    get => Get<DateTime?>("dateOfEmancipation");
    set => Set("dateOfEmancipation", value);
  }

  /// <summary>Length of the FC_ADOPTION_DISRUPTION_IND attribute.</summary>
  public const int FcAdoptionDisruptionInd_MaxLength = 1;

  /// <summary>
  /// The value of the FC_ADOPTION_DISRUPTION_IND attribute.
  /// Indicates adoption disruption.
  /// 	Y - Yes
  /// 	N - No
  /// </summary>
  [JsonPropertyName("fcAdoptionDisruptionInd")]
  [Member(Index = 34, Type = MemberType.Char, Length
    = FcAdoptionDisruptionInd_MaxLength, Optional = true)]
  public string FcAdoptionDisruptionInd
  {
    get => Get<string>("fcAdoptionDisruptionInd");
    set => Set(
      "fcAdoptionDisruptionInd", TrimEnd(Substring(value, 1,
      FcAdoptionDisruptionInd_MaxLength)));
  }

  /// <summary>Length of the FC_AP_NOTIFIED attribute.</summary>
  public const int FcApNotified_MaxLength = 1;

  /// <summary>
  /// The value of the FC_AP_NOTIFIED attribute.
  /// AP has been notified of responsibility to pay.
  /// </summary>
  [JsonPropertyName("fcApNotified")]
  [Member(Index = 35, Type = MemberType.Char, Length = FcApNotified_MaxLength, Optional
    = true)]
  public string FcApNotified
  {
    get => Get<string>("fcApNotified");
    set => Set(
      "fcApNotified", TrimEnd(Substring(value, 1, FcApNotified_MaxLength)));
  }

  /// <summary>Length of the FC_CINC_IND attribute.</summary>
  public const int FcCincInd_MaxLength = 1;

  /// <summary>
  /// The value of the FC_CINC_IND attribute.
  /// Child in need of care
  /// </summary>
  [JsonPropertyName("fcCincInd")]
  [Member(Index = 36, Type = MemberType.Char, Length = FcCincInd_MaxLength, Optional
    = true)]
  public string FcCincInd
  {
    get => Get<string>("fcCincInd");
    set => Set("fcCincInd", TrimEnd(Substring(value, 1, FcCincInd_MaxLength)));
  }

  /// <summary>
  /// The value of the FC_COST_OF_CARE attribute.
  /// The cost of care for a foster care child
  /// </summary>
  [JsonPropertyName("fcCostOfCare")]
  [Member(Index = 37, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? FcCostOfCare
  {
    get => Get<decimal?>("fcCostOfCare");
    set => Set("fcCostOfCare", Truncate(value, 2));
  }

  /// <summary>Length of the FC_COST_OF_CARE_FREQ attribute.</summary>
  public const int FcCostOfCareFreq_MaxLength = 1;

  /// <summary>
  /// The value of the FC_COST_OF_CARE_FREQ attribute.
  /// How often the cost of care occurs
  /// </summary>
  [JsonPropertyName("fcCostOfCareFreq")]
  [Member(Index = 38, Type = MemberType.Char, Length
    = FcCostOfCareFreq_MaxLength, Optional = true)]
  public string FcCostOfCareFreq
  {
    get => Get<string>("fcCostOfCareFreq");
    set => Set(
      "fcCostOfCareFreq",
      TrimEnd(Substring(value, 1, FcCostOfCareFreq_MaxLength)));
  }

  /// <summary>Length of the FC_COUNTY_CHILD_REMOVED_FROM attribute.</summary>
  public const int FcCountyChildRemovedFrom_MaxLength = 2;

  /// <summary>
  /// The value of the FC_COUNTY_CHILD_REMOVED_FROM attribute.
  /// The county the foster child was removed from
  /// </summary>
  [JsonPropertyName("fcCountyChildRemovedFrom")]
  [Member(Index = 39, Type = MemberType.Char, Length
    = FcCountyChildRemovedFrom_MaxLength, Optional = true)]
  public string FcCountyChildRemovedFrom
  {
    get => Get<string>("fcCountyChildRemovedFrom");
    set => Set(
      "fcCountyChildRemovedFrom", TrimEnd(Substring(value, 1,
      FcCountyChildRemovedFrom_MaxLength)));
  }

  /// <summary>
  /// The value of the FC_DATE_OF_INITIAL_CUSTODY attribute.
  /// Date child was initially taken into custody of foster parents
  /// </summary>
  [JsonPropertyName("fcDateOfInitialCustody")]
  [Member(Index = 40, Type = MemberType.Date, Optional = true)]
  public DateTime? FcDateOfInitialCustody
  {
    get => Get<DateTime?>("fcDateOfInitialCustody");
    set => Set("fcDateOfInitialCustody", value);
  }

  /// <summary>Length of the FC_IN_HOME_SERVICE_IND attribute.</summary>
  public const int FcInHomeServiceInd_MaxLength = 1;

  /// <summary>
  /// The value of the FC_IN_HOME_SERVICE_IND attribute.
  /// </summary>
  [JsonPropertyName("fcInHomeServiceInd")]
  [Member(Index = 41, Type = MemberType.Char, Length
    = FcInHomeServiceInd_MaxLength, Optional = true)]
  public string FcInHomeServiceInd
  {
    get => Get<string>("fcInHomeServiceInd");
    set => Set(
      "fcInHomeServiceInd", TrimEnd(Substring(value, 1,
      FcInHomeServiceInd_MaxLength)));
  }

  /// <summary>Length of the FC_IV_E_CASE_NUMBER attribute.</summary>
  public const int FcIvECaseNumber_MaxLength = 10;

  /// <summary>
  /// The value of the FC_IV_E_CASE_NUMBER attribute.
  /// Foster Care IV-E Case #
  /// </summary>
  [JsonPropertyName("fcIvECaseNumber")]
  [Member(Index = 42, Type = MemberType.Char, Length
    = FcIvECaseNumber_MaxLength, Optional = true)]
  public string FcIvECaseNumber
  {
    get => Get<string>("fcIvECaseNumber");
    set => Set(
      "fcIvECaseNumber",
      TrimEnd(Substring(value, 1, FcIvECaseNumber_MaxLength)));
  }

  /// <summary>Length of the FC_JUVENILE_COURT_ORDER attribute.</summary>
  public const int FcJuvenileCourtOrder_MaxLength = 12;

  /// <summary>
  /// The value of the FC_JUVENILE_COURT_ORDER attribute.
  /// Juvenile Court Order number.
  /// </summary>
  [JsonPropertyName("fcJuvenileCourtOrder")]
  [Member(Index = 43, Type = MemberType.Char, Length
    = FcJuvenileCourtOrder_MaxLength, Optional = true)]
  public string FcJuvenileCourtOrder
  {
    get => Get<string>("fcJuvenileCourtOrder");
    set => Set(
      "fcJuvenileCourtOrder", TrimEnd(Substring(value, 1,
      FcJuvenileCourtOrder_MaxLength)));
  }

  /// <summary>Length of the FC_JUVENILE_OFFENDER_IND attribute.</summary>
  public const int FcJuvenileOffenderInd_MaxLength = 1;

  /// <summary>
  /// The value of the FC_JUVENILE_OFFENDER_IND attribute.
  /// Indicates child was a juvenile offender when placed in Foster Care.
  /// 	
  /// 	Y - Yes
  /// 	N - No
  /// </summary>
  [JsonPropertyName("fcJuvenileOffenderInd")]
  [Member(Index = 44, Type = MemberType.Char, Length
    = FcJuvenileOffenderInd_MaxLength, Optional = true)]
  public string FcJuvenileOffenderInd
  {
    get => Get<string>("fcJuvenileOffenderInd");
    set => Set(
      "fcJuvenileOffenderInd", TrimEnd(Substring(value, 1,
      FcJuvenileOffenderInd_MaxLength)));
  }

  /// <summary>Length of the FC_LEVEL_OF_CARE attribute.</summary>
  public const int FcLevelOfCare_MaxLength = 1;

  /// <summary>
  /// The value of the FC_LEVEL_OF_CARE attribute.
  /// The level of care to be assigned to the child
  /// </summary>
  [JsonPropertyName("fcLevelOfCare")]
  [Member(Index = 45, Type = MemberType.Char, Length
    = FcLevelOfCare_MaxLength, Optional = true)]
  public string FcLevelOfCare
  {
    get => Get<string>("fcLevelOfCare");
    set => Set(
      "fcLevelOfCare", TrimEnd(Substring(value, 1, FcLevelOfCare_MaxLength)));
  }

  /// <summary>
  /// The value of the FC_NEXT_JUVENILE_CT_DT attribute.
  /// The next date the foster child is in juvenile court
  /// </summary>
  [JsonPropertyName("fcNextJuvenileCtDt")]
  [Member(Index = 46, Type = MemberType.Date, Optional = true)]
  public DateTime? FcNextJuvenileCtDt
  {
    get => Get<DateTime?>("fcNextJuvenileCtDt");
    set => Set("fcNextJuvenileCtDt", value);
  }

  /// <summary>Length of the FC_ORDER_EST_BY attribute.</summary>
  public const int FcOrderEstBy_MaxLength = 2;

  /// <summary>
  /// The value of the FC_ORDER_EST_BY attribute.
  /// Indicates who established the Foster Care order.
  /// 	PA - Public Attorney
  /// 	SA - SRS Attorney
  /// 	CA - County Attorney
  /// 	OT - Other
  /// </summary>
  [JsonPropertyName("fcOrderEstBy")]
  [Member(Index = 47, Type = MemberType.Char, Length = FcOrderEstBy_MaxLength, Optional
    = true)]
  public string FcOrderEstBy
  {
    get => Get<string>("fcOrderEstBy");
    set => Set(
      "fcOrderEstBy", TrimEnd(Substring(value, 1, FcOrderEstBy_MaxLength)));
  }

  /// <summary>Length of the FC_OTHER_BENEFIT_IND attribute.</summary>
  public const int FcOtherBenefitInd_MaxLength = 1;

  /// <summary>
  /// The value of the FC_OTHER_BENEFIT_IND attribute.
  /// Indicates whether the foster child is receiving any other benefits
  /// </summary>
  [JsonPropertyName("fcOtherBenefitInd")]
  [Member(Index = 48, Type = MemberType.Char, Length
    = FcOtherBenefitInd_MaxLength, Optional = true)]
  public string FcOtherBenefitInd
  {
    get => Get<string>("fcOtherBenefitInd");
    set => Set(
      "fcOtherBenefitInd", TrimEnd(Substring(value, 1,
      FcOtherBenefitInd_MaxLength)));
  }

  /// <summary>Length of the FC_PARENTAL_RIGHTS attribute.</summary>
  public const int FcParentalRights_MaxLength = 1;

  /// <summary>
  /// The value of the FC_PARENTAL_RIGHTS attribute.
  /// Indicates the status of rights that the parents have for a foster child
  /// S	Severed
  /// P	Pending
  /// E	Existing
  /// </summary>
  [JsonPropertyName("fcParentalRights")]
  [Member(Index = 49, Type = MemberType.Char, Length
    = FcParentalRights_MaxLength, Optional = true)]
  public string FcParentalRights
  {
    get => Get<string>("fcParentalRights");
    set => Set(
      "fcParentalRights",
      TrimEnd(Substring(value, 1, FcParentalRights_MaxLength)));
  }

  /// <summary>Length of the FC_PREV_PAYEE_FIRST_NAME attribute.</summary>
  public const int FcPrevPayeeFirstName_MaxLength = 12;

  /// <summary>
  /// The value of the FC_PREV_PAYEE_FIRST_NAME attribute.
  /// The first name of the previous foster care payee
  /// </summary>
  [JsonPropertyName("fcPrevPayeeFirstName")]
  [Member(Index = 50, Type = MemberType.Char, Length
    = FcPrevPayeeFirstName_MaxLength, Optional = true)]
  public string FcPrevPayeeFirstName
  {
    get => Get<string>("fcPrevPayeeFirstName");
    set => Set(
      "fcPrevPayeeFirstName", TrimEnd(Substring(value, 1,
      FcPrevPayeeFirstName_MaxLength)));
  }

  /// <summary>Length of the FC_PREV_PAYEE_MIDDLE_INITIAL attribute.</summary>
  public const int FcPrevPayeeMiddleInitial_MaxLength = 1;

  /// <summary>
  /// The value of the FC_PREV_PAYEE_MIDDLE_INITIAL attribute.
  /// The middle initial of the previous foster care payee
  /// </summary>
  [JsonPropertyName("fcPrevPayeeMiddleInitial")]
  [Member(Index = 51, Type = MemberType.Char, Length
    = FcPrevPayeeMiddleInitial_MaxLength, Optional = true)]
  public string FcPrevPayeeMiddleInitial
  {
    get => Get<string>("fcPrevPayeeMiddleInitial");
    set => Set(
      "fcPrevPayeeMiddleInitial", TrimEnd(Substring(value, 1,
      FcPrevPayeeMiddleInitial_MaxLength)));
  }

  /// <summary>
  /// The value of the FC_PLACEMENT_DATE attribute.
  /// Date the child was placed
  /// </summary>
  [JsonPropertyName("fcPlacementDate")]
  [Member(Index = 52, Type = MemberType.Date, Optional = true)]
  public DateTime? FcPlacementDate
  {
    get => Get<DateTime?>("fcPlacementDate");
    set => Set("fcPlacementDate", value);
  }

  /// <summary>Length of the FC_PLACEMENT_NAME attribute.</summary>
  public const int FcPlacementName_MaxLength = 25;

  /// <summary>
  /// The value of the FC_PLACEMENT_NAME attribute.
  /// Name of person child is placed with
  /// </summary>
  [JsonPropertyName("fcPlacementName")]
  [Member(Index = 53, Type = MemberType.Char, Length
    = FcPlacementName_MaxLength, Optional = true)]
  public string FcPlacementName
  {
    get => Get<string>("fcPlacementName");
    set => Set(
      "fcPlacementName",
      TrimEnd(Substring(value, 1, FcPlacementName_MaxLength)));
  }

  /// <summary>Length of the FC_PLACEMENT_REASON attribute.</summary>
  public const int FcPlacementReason_MaxLength = 2;

  /// <summary>
  /// The value of the FC_PLACEMENT_REASON attribute.
  /// Reason Foster Care was selected
  /// </summary>
  [JsonPropertyName("fcPlacementReason")]
  [Member(Index = 54, Type = MemberType.Char, Length
    = FcPlacementReason_MaxLength, Optional = true)]
  public string FcPlacementReason
  {
    get => Get<string>("fcPlacementReason");
    set => Set(
      "fcPlacementReason", TrimEnd(Substring(value, 1,
      FcPlacementReason_MaxLength)));
  }

  /// <summary>Length of the FC_PREVIOUS_PA attribute.</summary>
  public const int FcPreviousPa_MaxLength = 1;

  /// <summary>
  /// The value of the FC_PREVIOUS_PA attribute.
  /// Previous Public Assistance such as ADC
  /// </summary>
  [JsonPropertyName("fcPreviousPa")]
  [Member(Index = 55, Type = MemberType.Char, Length = FcPreviousPa_MaxLength, Optional
    = true)]
  public string FcPreviousPa
  {
    get => Get<string>("fcPreviousPa");
    set => Set(
      "fcPreviousPa", TrimEnd(Substring(value, 1, FcPreviousPa_MaxLength)));
  }

  /// <summary>Length of the FC_PREVIOUS_PAYEE_LAST_NAME attribute.</summary>
  public const int FcPreviousPayeeLastName_MaxLength = 17;

  /// <summary>
  /// The value of the FC_PREVIOUS_PAYEE_LAST_NAME attribute.
  /// The last name of the previous foster care payee
  /// </summary>
  [JsonPropertyName("fcPreviousPayeeLastName")]
  [Member(Index = 56, Type = MemberType.Char, Length
    = FcPreviousPayeeLastName_MaxLength, Optional = true)]
  public string FcPreviousPayeeLastName
  {
    get => Get<string>("fcPreviousPayeeLastName");
    set => Set(
      "fcPreviousPayeeLastName", TrimEnd(Substring(value, 1,
      FcPreviousPayeeLastName_MaxLength)));
  }

  /// <summary>Length of the FC_SOURCE_OF_FUNDING attribute.</summary>
  public const int FcSourceOfFunding_MaxLength = 2;

  /// <summary>
  /// The value of the FC_SOURCE_OF_FUNDING attribute.
  /// The source of funding for a foster care child
  /// </summary>
  [JsonPropertyName("fcSourceOfFunding")]
  [Member(Index = 57, Type = MemberType.Char, Length
    = FcSourceOfFunding_MaxLength, Optional = true)]
  public string FcSourceOfFunding
  {
    get => Get<string>("fcSourceOfFunding");
    set => Set(
      "fcSourceOfFunding", TrimEnd(Substring(value, 1,
      FcSourceOfFunding_MaxLength)));
  }

  /// <summary>Length of the FC_SRS_PAYEE attribute.</summary>
  public const int FcSrsPayee_MaxLength = 1;

  /// <summary>
  /// The value of the FC_SRS_PAYEE attribute.
  /// Indicates whether or not SRS has been determined to be the payee.
  /// </summary>
  [JsonPropertyName("fcSrsPayee")]
  [Member(Index = 58, Type = MemberType.Char, Length = FcSrsPayee_MaxLength, Optional
    = true)]
  public string FcSrsPayee
  {
    get => Get<string>("fcSrsPayee");
    set =>
      Set("fcSrsPayee", TrimEnd(Substring(value, 1, FcSrsPayee_MaxLength)));
  }

  /// <summary>Length of the FC_SSA attribute.</summary>
  public const int FcSsa_MaxLength = 1;

  /// <summary>
  /// The value of the FC_SSA attribute.
  /// Foster Care - any social security death stuff
  /// </summary>
  [JsonPropertyName("fcSsa")]
  [Member(Index = 59, Type = MemberType.Char, Length = FcSsa_MaxLength, Optional
    = true)]
  public string FcSsa
  {
    get => Get<string>("fcSsa");
    set => Set("fcSsa", TrimEnd(Substring(value, 1, FcSsa_MaxLength)));
  }

  /// <summary>Length of the FC_SSI attribute.</summary>
  public const int FcSsi_MaxLength = 1;

  /// <summary>
  /// The value of the FC_SSI attribute.
  /// Social Security for disability indicator
  /// </summary>
  [JsonPropertyName("fcSsi")]
  [Member(Index = 60, Type = MemberType.Char, Length = FcSsi_MaxLength, Optional
    = true)]
  public string FcSsi
  {
    get => Get<string>("fcSsi");
    set => Set("fcSsi", TrimEnd(Substring(value, 1, FcSsi_MaxLength)));
  }

  /// <summary>Length of the FC_VA_IND attribute.</summary>
  public const int FcVaInd_MaxLength = 1;

  /// <summary>
  /// The value of the FC_VA_IND attribute.
  /// Veterans Administration benefits.
  /// 	Y - Yes
  /// 	N - No
  /// </summary>
  [JsonPropertyName("fcVaInd")]
  [Member(Index = 61, Type = MemberType.Char, Length = FcVaInd_MaxLength, Optional
    = true)]
  public string FcVaInd
  {
    get => Get<string>("fcVaInd");
    set => Set("fcVaInd", TrimEnd(Substring(value, 1, FcVaInd_MaxLength)));
  }

  /// <summary>Length of the FC_WARDS_ACCOUNT attribute.</summary>
  public const int FcWardsAccount_MaxLength = 1;

  /// <summary>
  /// The value of the FC_WARDS_ACCOUNT attribute.
  /// Money available to the child after foster care is completed
  /// </summary>
  [JsonPropertyName("fcWardsAccount")]
  [Member(Index = 62, Type = MemberType.Char, Length
    = FcWardsAccount_MaxLength, Optional = true)]
  public string FcWardsAccount
  {
    get => Get<string>("fcWardsAccount");
    set => Set(
      "fcWardsAccount", TrimEnd(Substring(value, 1, FcWardsAccount_MaxLength)));
      
  }

  /// <summary>Length of the FC_ZEB_IND attribute.</summary>
  public const int FcZebInd_MaxLength = 1;

  /// <summary>
  /// The value of the FC_ZEB_IND attribute.
  /// A lawsuit filed against social security on behalf of children denied ssi 
  /// benefits as their disabilities were determined using adult standards not
  /// children standards.  There was a settlement and ssi payments to children
  /// who were denied ssi based on adult standards and then approved using child
  /// standards are placed in a &quot;zeb&quot;ley account.
  /// </summary>
  [JsonPropertyName("fcZebInd")]
  [Member(Index = 63, Type = MemberType.Char, Length = FcZebInd_MaxLength, Optional
    = true)]
  public string FcZebInd
  {
    get => Get<string>("fcZebInd");
    set => Set("fcZebInd", TrimEnd(Substring(value, 1, FcZebInd_MaxLength)));
  }

  /// <summary>Length of the OVER_18_AND_IN_SCHOOL attribute.</summary>
  public const int Over18AndInSchool_MaxLength = 1;

  /// <summary>
  /// The value of the OVER_18_AND_IN_SCHOOL attribute.
  /// The child is over 18 years old and still attending school
  /// </summary>
  [JsonPropertyName("over18AndInSchool")]
  [Member(Index = 64, Type = MemberType.Char, Length
    = Over18AndInSchool_MaxLength, Optional = true)]
  public string Over18AndInSchool
  {
    get => Get<string>("over18AndInSchool");
    set => Set(
      "over18AndInSchool", TrimEnd(Substring(value, 1,
      Over18AndInSchool_MaxLength)));
  }

  /// <summary>Length of the PATERNITY_ESTABLISHED_INDICATOR attribute.
  /// </summary>
  public const int PaternityEstablishedIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the PATERNITY_ESTABLISHED_INDICATOR attribute.
  /// Indicates whether or not paternity has been established
  /// </summary>
  [JsonPropertyName("paternityEstablishedIndicator")]
  [Member(Index = 65, Type = MemberType.Char, Length
    = PaternityEstablishedIndicator_MaxLength, Optional = true)]
  public string PaternityEstablishedIndicator
  {
    get => Get<string>("paternityEstablishedIndicator");
    set => Set(
      "paternityEstablishedIndicator", TrimEnd(Substring(value, 1,
      PaternityEstablishedIndicator_MaxLength)));
  }

  /// <summary>Length of the RESIDES_WITH_AR_INDICATOR attribute.</summary>
  public const int ResidesWithArIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the RESIDES_WITH_AR_INDICATOR attribute.
  /// An indicator of whether the Child resides with the AR or not.
  /// </summary>
  [JsonPropertyName("residesWithArIndicator")]
  [Member(Index = 66, Type = MemberType.Char, Length
    = ResidesWithArIndicator_MaxLength, Optional = true)]
  [Value(null)]
  [Value("N")]
  [Value("Y")]
  public string ResidesWithArIndicator
  {
    get => Get<string>("residesWithArIndicator");
    set => Set(
      "residesWithArIndicator", TrimEnd(Substring(value, 1,
      ResidesWithArIndicator_MaxLength)));
  }

  /// <summary>Length of the SPECIALTY_AREA attribute.</summary>
  public const int SpecialtyArea_MaxLength = 1;

  /// <summary>
  /// The value of the SPECIALTY_AREA attribute.
  /// The area of specialization required to work the case.  There are currently
  /// two defined:  Enforcement and Paternity.
  /// </summary>
  [JsonPropertyName("specialtyArea")]
  [Member(Index = 67, Type = MemberType.Char, Length
    = SpecialtyArea_MaxLength, Optional = true)]
  [Value(null)]
  [Value("E")]
  [Value("P")]
  public string SpecialtyArea
  {
    get => Get<string>("specialtyArea");
    set => Set(
      "specialtyArea", TrimEnd(Substring(value, 1, SpecialtyArea_MaxLength)));
  }

  /// <summary>Length of the REL TO AR attribute.</summary>
  public const int RelToAr_MaxLength = 2;

  /// <summary>
  /// The value of the REL TO AR attribute.
  /// The relationship of the child to the AR in a given case.
  /// EX: son
  /// </summary>
  [JsonPropertyName("relToAr")]
  [Member(Index = 68, Type = MemberType.Char, Length = RelToAr_MaxLength, Optional
    = true)]
  public string RelToAr
  {
    get => Get<string>("relToAr");
    set => Set("relToAr", TrimEnd(Substring(value, 1, RelToAr_MaxLength)));
  }

  /// <summary>Length of the CONTACT_FIRST_NAME attribute.</summary>
  public const int ContactFirstName_MaxLength = 12;

  /// <summary>
  /// The value of the CONTACT_FIRST_NAME attribute.
  /// The first name of the person contacted for the AR
  /// </summary>
  [JsonPropertyName("contactFirstName")]
  [Member(Index = 69, Type = MemberType.Char, Length
    = ContactFirstName_MaxLength, Optional = true)]
  public string ContactFirstName
  {
    get => Get<string>("contactFirstName");
    set => Set(
      "contactFirstName",
      TrimEnd(Substring(value, 1, ContactFirstName_MaxLength)));
  }

  /// <summary>Length of the CONTACT_MIDDLE_INITIAL attribute.</summary>
  public const int ContactMiddleInitial_MaxLength = 1;

  /// <summary>
  /// The value of the CONTACT_MIDDLE_INITIAL attribute.
  /// The middle initial of the AR's contact
  /// </summary>
  [JsonPropertyName("contactMiddleInitial")]
  [Member(Index = 70, Type = MemberType.Char, Length
    = ContactMiddleInitial_MaxLength, Optional = true)]
  public string ContactMiddleInitial
  {
    get => Get<string>("contactMiddleInitial");
    set => Set(
      "contactMiddleInitial", TrimEnd(Substring(value, 1,
      ContactMiddleInitial_MaxLength)));
  }

  /// <summary>Length of the CONTACT_PHONE attribute.</summary>
  public const int ContactPhone_MaxLength = 10;

  /// <summary>
  /// The value of the CONTACT_PHONE attribute.
  /// The telephone number of the AR's contact
  /// </summary>
  [JsonPropertyName("contactPhone")]
  [Member(Index = 71, Type = MemberType.Char, Length = ContactPhone_MaxLength, Optional
    = true)]
  public string ContactPhone
  {
    get => Get<string>("contactPhone");
    set => Set(
      "contactPhone", TrimEnd(Substring(value, 1, ContactPhone_MaxLength)));
  }

  /// <summary>Length of the CONTACT_LAST_NAME attribute.</summary>
  public const int ContactLastName_MaxLength = 17;

  /// <summary>
  /// The value of the CONTACT_LAST_NAME attribute.
  /// The last name of the AR's contact
  /// </summary>
  [JsonPropertyName("contactLastName")]
  [Member(Index = 72, Type = MemberType.Char, Length
    = ContactLastName_MaxLength, Optional = true)]
  public string ContactLastName
  {
    get => Get<string>("contactLastName");
    set => Set(
      "contactLastName",
      TrimEnd(Substring(value, 1, ContactLastName_MaxLength)));
  }

  /// <summary>
  /// The value of the CHILD_CARE_EXPENSES attribute.
  /// Expenses of Child Care for children in the home.
  /// </summary>
  [JsonPropertyName("childCareExpenses")]
  [Member(Index = 73, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? ChildCareExpenses
  {
    get => Get<decimal?>("childCareExpenses");
    set => Set("childCareExpenses", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the ASSIGNMENT_DATE attribute.
  /// Date Assignment of Rights was applied	
  /// </summary>
  [JsonPropertyName("assignmentDate")]
  [Member(Index = 74, Type = MemberType.Date, Optional = true)]
  public DateTime? AssignmentDate
  {
    get => Get<DateTime?>("assignmentDate");
    set => Set("assignmentDate", value);
  }

  /// <summary>Length of the ASSIGNMENT_TERMINATION_CODE attribute.</summary>
  public const int AssignmentTerminationCode_MaxLength = 2;

  /// <summary>
  /// The value of the ASSIGNMENT_TERMINATION_CODE attribute.
  /// Termination status:
  /// Paid - All SRS Arrears paid in Full, no ongoing current support assigned.
  /// Settled - Arrears settled through valid settlement agreement with SRS.
  /// Claim Paid - SRS claim paid in full, other arrearages reassigned to 
  /// obligee.
  /// Claim Settled - SRS claim settled through valid settlement agreement with 
  /// SRS; other arrearages reassigned to obligee.
  /// Closed - the Non-ADC case has been closed, and all rights reassigned to 
  /// the obligee.
  /// Arrears only - Current support terminated,
  /// </summary>
  [JsonPropertyName("assignmentTerminationCode")]
  [Member(Index = 75, Type = MemberType.Char, Length
    = AssignmentTerminationCode_MaxLength, Optional = true)]
  public string AssignmentTerminationCode
  {
    get => Get<string>("assignmentTerminationCode");
    set => Set(
      "assignmentTerminationCode", TrimEnd(Substring(value, 1,
      AssignmentTerminationCode_MaxLength)));
  }

  /// <summary>Length of the ASSIGNMENT_OF_RIGHTS attribute.</summary>
  public const int AssignmentOfRights_MaxLength = 1;

  /// <summary>
  /// The value of the ASSIGNMENT_OF_RIGHTS attribute.
  /// A flag indicating whether the AR's rights have been assigned to SRS or 
  /// not.
  /// </summary>
  [JsonPropertyName("assignmentOfRights")]
  [Member(Index = 76, Type = MemberType.Char, Length
    = AssignmentOfRights_MaxLength, Optional = true)]
  public string AssignmentOfRights
  {
    get => Get<string>("assignmentOfRights");
    set => Set(
      "assignmentOfRights", TrimEnd(Substring(value, 1,
      AssignmentOfRights_MaxLength)));
  }

  /// <summary>
  /// The value of the ASSIGNMENT_TERMINATED_DT attribute.
  /// The date the assignment of rights was terminated
  /// </summary>
  [JsonPropertyName("assignmentTerminatedDt")]
  [Member(Index = 77, Type = MemberType.Date, Optional = true)]
  public DateTime? AssignmentTerminatedDt
  {
    get => Get<DateTime?>("assignmentTerminatedDt");
    set => Set("assignmentTerminatedDt", value);
  }

  /// <summary>Length of the AR_CHG_PROC_REQ_IND attribute.</summary>
  public const int ArChgProcReqInd_MaxLength = 1;

  /// <summary>
  /// The value of the AR_CHG_PROC_REQ_IND attribute.
  /// Defines whether or not Finance processing is required when an AR 
  /// occurrance is created. If the occurrance is the result of and AR change
  /// and the effective date is less than current date than the 'CSE ROLE
  /// CHANGE' process mut be executed to handle any possible recovery debts.
  /// This process will evaluate the change and make the necessary adjustments.
  ///      Values: &quot;Y&quot; - Yes, AR Change Processing Required
  ///              &quot;N&quot; - No, AR Change Processing NOT Required (
  /// Default).
  /// </summary>
  [JsonPropertyName("arChgProcReqInd")]
  [Member(Index = 78, Type = MemberType.Char, Length
    = ArChgProcReqInd_MaxLength, Optional = true)]
  public string ArChgProcReqInd
  {
    get => Get<string>("arChgProcReqInd");
    set => Set(
      "arChgProcReqInd",
      TrimEnd(Substring(value, 1, ArChgProcReqInd_MaxLength)));
  }

  /// <summary>
  /// The value of the AR_CHG_PROCESSED_DATE attribute.
  /// Defines the date AR Change processing is completed.
  /// </summary>
  [JsonPropertyName("arChgProcessedDate")]
  [Member(Index = 79, Type = MemberType.Date, Optional = true)]
  public DateTime? ArChgProcessedDate
  {
    get => Get<DateTime?>("arChgProcessedDate");
    set => Set("arChgProcessedDate", value);
  }

  /// <summary>Length of the AR_INVALID_IND attribute.</summary>
  public const int ArInvalidInd_MaxLength = 1;

  /// <summary>
  /// The value of the AR_INVALID_IND attribute.
  /// Identifies and occurrence of an AR CASE ROLE as having been set up i9n 
  /// error on a case for a given time. This will reflect the AR assignment over
  /// the life of the case and will help to explain certain recovery
  /// situations.
  ///     Values: &quot;I&quot; - AR is/was invalid
  ///              blank - AR is/was valid (Default)
  /// </summary>
  [JsonPropertyName("arInvalidInd")]
  [Member(Index = 80, Type = MemberType.Char, Length = ArInvalidInd_MaxLength, Optional
    = true)]
  public string ArInvalidInd
  {
    get => Get<string>("arInvalidInd");
    set => Set(
      "arInvalidInd", TrimEnd(Substring(value, 1, ArInvalidInd_MaxLength)));
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CasNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// A unique number assigned to a request or referral.  Becomes the Case 
  /// number.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 81, Type = MemberType.Char, Length = CasNumber_MaxLength)]
  public string CasNumber
  {
    get => Get<string>("casNumber") ?? "";
    set => Set(
      "casNumber", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, CasNumber_MaxLength)));
  }

  /// <summary>
  /// The json value of the CasNumber attribute.</summary>
  [JsonPropertyName("casNumber")]
  [Computed]
  public string CasNumber_Json
  {
    get => NullIf(CasNumber, "");
    set => CasNumber = value;
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
  [Member(Index = 82, Type = MemberType.Char, Length = CspNumber_MaxLength)]
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
