// The source file: OUT_DOC_RTRN_ADDR, ID: 371427268, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVPLAN
/// Each occurrence represents outgoing document return address informaiton for 
/// a specific online session.  Each occurrence may also represent outgoing
/// document return address information for batch generated documents.
/// </summary>
[Serializable]
public partial class OutDocRtrnAddr: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public OutDocRtrnAddr()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public OutDocRtrnAddr(OutDocRtrnAddr that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new OutDocRtrnAddr Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(OutDocRtrnAddr that)
  {
    base.Assign(that);
    ospWorkPhoneNumber = that.ospWorkPhoneNumber;
    ospWorkPhoneAreaCode = that.ospWorkPhoneAreaCode;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    ospWorkPhoneExtension = that.ospWorkPhoneExtension;
    ospCertificationNumber = that.ospCertificationNumber;
    ospLocalContactCode = that.ospLocalContactCode;
    ospRoleCode = that.ospRoleCode;
    ospEffectiveDate = that.ospEffectiveDate;
    officeSysGenId = that.officeSysGenId;
    officeName = that.officeName;
    offcAddrStreet1 = that.offcAddrStreet1;
    offcAddrStreet2 = that.offcAddrStreet2;
    offcAddrCity = that.offcAddrCity;
    offcAddrState = that.offcAddrState;
    offcAddrPostalCode = that.offcAddrPostalCode;
    offcAddrZip = that.offcAddrZip;
    offcAddrZip4 = that.offcAddrZip4;
    offcAddrCountry = that.offcAddrCountry;
    servProvSysGenId = that.servProvSysGenId;
    servProvUserId = that.servProvUserId;
    servProvLastName = that.servProvLastName;
    servProvFirstName = that.servProvFirstName;
    servProvMi = that.servProvMi;
    servProvAddrStree1 = that.servProvAddrStree1;
    servProvAddrStreet2 = that.servProvAddrStreet2;
    servProvAddrCity = that.servProvAddrCity;
    servProvAddrSt = that.servProvAddrSt;
    servProvAddrPostalCode = that.servProvAddrPostalCode;
    servProvAddrZip = that.servProvAddrZip;
    servProvAddrZip4 = that.servProvAddrZip4;
    servProvAddrCountry = that.servProvAddrCountry;
    offcAddrZip3 = that.offcAddrZip3;
    servProvAddrZip3 = that.servProvAddrZip3;
  }

  /// <summary>
  /// The value of the OSP_WORK_PHONE_NUMBER attribute.
  /// The work phone number for the Office Service Provider that this occurrence
  /// represents.
  /// </summary>
  [JsonPropertyName("ospWorkPhoneNumber")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 7)]
  public int OspWorkPhoneNumber
  {
    get => ospWorkPhoneNumber;
    set => ospWorkPhoneNumber = value;
  }

  /// <summary>
  /// The value of the OSP_WORK_PHONE_AREA_CODE attribute.
  /// The area code for the voice work phone number for the Office Service 
  /// Provider that this occurrence represents.
  /// </summary>
  [JsonPropertyName("ospWorkPhoneAreaCode")]
  [DefaultValue(0)]
  [Member(Index = 2, Type = MemberType.Number, Length = 3)]
  public int OspWorkPhoneAreaCode
  {
    get => ospWorkPhoneAreaCode;
    set => ospWorkPhoneAreaCode = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The User ID of the logged on user that is responsible for the creation of 
  /// this occurrence of the entity.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// A timestamp for the creation of the occurrence.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 4, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>Length of the OSP_WORK_PHONE_EXTENSION attribute.</summary>
  public const int OspWorkPhoneExtension_MaxLength = 5;

  /// <summary>
  /// The value of the OSP_WORK_PHONE_EXTENSION attribute.
  /// The optional 5 charactor extension for the voice work phone number for the
  /// office service provider that this occurrence of the entity represents.
  /// </summary>
  [JsonPropertyName("ospWorkPhoneExtension")]
  [Member(Index = 5, Type = MemberType.Char, Length
    = OspWorkPhoneExtension_MaxLength, Optional = true)]
  public string OspWorkPhoneExtension
  {
    get => ospWorkPhoneExtension;
    set => ospWorkPhoneExtension = value != null
      ? TrimEnd(Substring(value, 1, OspWorkPhoneExtension_MaxLength)) : null;
  }

  /// <summary>Length of the OSP_CERTIFICATION_NUMBER attribute.</summary>
  public const int OspCertificationNumber_MaxLength = 10;

  /// <summary>
  /// The value of the OSP_CERTIFICATION_NUMBER attribute.
  /// This attribute is used to identify the Office Service Provider represented
  /// by this occurrence of the entity to something outside of the Kessep
  /// system.
  /// </summary>
  [JsonPropertyName("ospCertificationNumber")]
  [Member(Index = 6, Type = MemberType.Char, Length
    = OspCertificationNumber_MaxLength, Optional = true)]
  public string OspCertificationNumber
  {
    get => ospCertificationNumber;
    set => ospCertificationNumber = value != null
      ? TrimEnd(Substring(value, 1, OspCertificationNumber_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the OSP_LOCAL_CONTACT_CODE attribute.
  /// This attribute specifies the Local Contact Code (for the Office Service 
  /// Provider represented by this occurrence of the entity) to be sent with
  /// FDSO certifications.  We need to send an address tape containing these
  /// local codes and the corresponding contact addresses.  These contact
  /// addresses will be used in the pre-offset notices that are sent to the
  /// obligors by O.C.S.E.
  /// </summary>
  [JsonPropertyName("ospLocalContactCode")]
  [Member(Index = 7, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? OspLocalContactCode
  {
    get => ospLocalContactCode;
    set => ospLocalContactCode = value;
  }

  /// <summary>Length of the OSP_ROLE_CODE attribute.</summary>
  public const int OspRoleCode_MaxLength = 2;

  /// <summary>
  /// The value of the OSP_ROLE_CODE attribute.
  /// This is the Office Service Provider ROLE_CODE that represents the job 
  /// title or role that the Office Service Provider represented by this
  /// occurrence is fulfilling at a particular CSE Office.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = OspRoleCode_MaxLength)]
  public string OspRoleCode
  {
    get => ospRoleCode ?? "";
    set => ospRoleCode = TrimEnd(Substring(value, 1, OspRoleCode_MaxLength));
  }

  /// <summary>
  /// The json value of the OspRoleCode attribute.</summary>
  [JsonPropertyName("ospRoleCode")]
  [Computed]
  public string OspRoleCode_Json
  {
    get => NullIf(OspRoleCode, "");
    set => OspRoleCode = value;
  }

  /// <summary>
  /// The value of the OSP_EFFECTIVE_DATE attribute.
  /// The effective date of the Office Service Provider that this occurrence of 
  /// the entity represents.
  /// </summary>
  [JsonPropertyName("ospEffectiveDate")]
  [Member(Index = 9, Type = MemberType.Date)]
  public DateTime? OspEffectiveDate
  {
    get => ospEffectiveDate;
    set => ospEffectiveDate = value;
  }

  /// <summary>
  /// The value of the OFFICE_SYS_GEN_ID attribute.
  /// A unique number that identifies the CSE Office that the Office Service 
  /// Provider represented by this occurrence is assigned to.
  /// </summary>
  [JsonPropertyName("officeSysGenId")]
  [DefaultValue(0)]
  [Member(Index = 10, Type = MemberType.Number, Length = 4)]
  public int OfficeSysGenId
  {
    get => officeSysGenId;
    set => officeSysGenId = value;
  }

  /// <summary>Length of the OFFICE_NAME attribute.</summary>
  public const int OfficeName_MaxLength = 30;

  /// <summary>
  /// The value of the OFFICE_NAME attribute.
  /// The name of the Office that the Office Service Provider represented by 
  /// this occurrence is assigned to.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length = OfficeName_MaxLength)]
  public string OfficeName
  {
    get => officeName ?? "";
    set => officeName = TrimEnd(Substring(value, 1, OfficeName_MaxLength));
  }

  /// <summary>
  /// The json value of the OfficeName attribute.</summary>
  [JsonPropertyName("officeName")]
  [Computed]
  public string OfficeName_Json
  {
    get => NullIf(OfficeName, "");
    set => OfficeName = value;
  }

  /// <summary>Length of the OFFC_ADDR_STREET_1 attribute.</summary>
  public const int OffcAddrStreet1_MaxLength = 25;

  /// <summary>
  /// The value of the OFFC_ADDR_STREET_1 attribute.
  /// Line one of the street name/number for the mailing Office Address that the
  /// Office Service Provider represented by this occurrence is assigned to.
  /// </summary>
  [JsonPropertyName("offcAddrStreet1")]
  [Member(Index = 12, Type = MemberType.Char, Length
    = OffcAddrStreet1_MaxLength, Optional = true)]
  public string OffcAddrStreet1
  {
    get => offcAddrStreet1;
    set => offcAddrStreet1 = value != null
      ? TrimEnd(Substring(value, 1, OffcAddrStreet1_MaxLength)) : null;
  }

  /// <summary>Length of the OFFC_ADDR_STREET_2 attribute.</summary>
  public const int OffcAddrStreet2_MaxLength = 25;

  /// <summary>
  /// The value of the OFFC_ADDR_STREET_2 attribute.
  /// Line two of the street name/number for the mailing Office Address that the
  /// Office Service Provider represented by this occurrence is assigned to.
  /// </summary>
  [JsonPropertyName("offcAddrStreet2")]
  [Member(Index = 13, Type = MemberType.Char, Length
    = OffcAddrStreet2_MaxLength, Optional = true)]
  public string OffcAddrStreet2
  {
    get => offcAddrStreet2;
    set => offcAddrStreet2 = value != null
      ? TrimEnd(Substring(value, 1, OffcAddrStreet2_MaxLength)) : null;
  }

  /// <summary>Length of the OFFC_ADDR_CITY attribute.</summary>
  public const int OffcAddrCity_MaxLength = 15;

  /// <summary>
  /// The value of the OFFC_ADDR_CITY attribute.
  /// The name of the city for the mailing Office Address that the Office 
  /// Service Provider represented by this occurrence is assigned to.
  /// </summary>
  [JsonPropertyName("offcAddrCity")]
  [Member(Index = 14, Type = MemberType.Char, Length = OffcAddrCity_MaxLength, Optional
    = true)]
  public string OffcAddrCity
  {
    get => offcAddrCity;
    set => offcAddrCity = value != null
      ? TrimEnd(Substring(value, 1, OffcAddrCity_MaxLength)) : null;
  }

  /// <summary>Length of the OFFC_ADDR_STATE attribute.</summary>
  public const int OffcAddrState_MaxLength = 2;

  /// <summary>
  /// The value of the OFFC_ADDR_STATE attribute.
  /// The 2 character abbreviation for the name of the state or province for the
  /// mailing Office Address that the Office Service Provider, represented by
  /// this occurrence, is assigned to.
  /// </summary>
  [JsonPropertyName("offcAddrState")]
  [Member(Index = 15, Type = MemberType.Char, Length
    = OffcAddrState_MaxLength, Optional = true)]
  public string OffcAddrState
  {
    get => offcAddrState;
    set => offcAddrState = value != null
      ? TrimEnd(Substring(value, 1, OffcAddrState_MaxLength)) : null;
  }

  /// <summary>Length of the OFFC_ADDR_POSTAL_CODE attribute.</summary>
  public const int OffcAddrPostalCode_MaxLength = 10;

  /// <summary>
  /// The value of the OFFC_ADDR_POSTAL_CODE attribute.
  /// The postal code for the mailing Office Address that the Office Service 
  /// Provider represented by this occurrence is assigned to.  This is not the
  /// Zip Code.
  /// </summary>
  [JsonPropertyName("offcAddrPostalCode")]
  [Member(Index = 16, Type = MemberType.Char, Length
    = OffcAddrPostalCode_MaxLength, Optional = true)]
  public string OffcAddrPostalCode
  {
    get => offcAddrPostalCode;
    set => offcAddrPostalCode = value != null
      ? TrimEnd(Substring(value, 1, OffcAddrPostalCode_MaxLength)) : null;
  }

  /// <summary>Length of the OFFC_ADDR_ZIP attribute.</summary>
  public const int OffcAddrZip_MaxLength = 5;

  /// <summary>
  /// The value of the OFFC_ADDR_ZIP attribute.
  /// The 5 digit Zip Code for the mailing Office Address that the Office 
  /// Service Provider represented by this occurrence is assigned to.
  /// </summary>
  [JsonPropertyName("offcAddrZip")]
  [Member(Index = 17, Type = MemberType.Char, Length = OffcAddrZip_MaxLength, Optional
    = true)]
  public string OffcAddrZip
  {
    get => offcAddrZip;
    set => offcAddrZip = value != null
      ? TrimEnd(Substring(value, 1, OffcAddrZip_MaxLength)) : null;
  }

  /// <summary>Length of the OFFC_ADDR_ZIP4 attribute.</summary>
  public const int OffcAddrZip4_MaxLength = 4;

  /// <summary>
  /// The value of the OFFC_ADDR_ZIP4 attribute.
  /// The 4 digit zip code extension for the mailing Office Address that the 
  /// Office Service Provider reprensent by this occurrence is assigned to.
  /// </summary>
  [JsonPropertyName("offcAddrZip4")]
  [Member(Index = 18, Type = MemberType.Char, Length = OffcAddrZip4_MaxLength, Optional
    = true)]
  public string OffcAddrZip4
  {
    get => offcAddrZip4;
    set => offcAddrZip4 = value != null
      ? TrimEnd(Substring(value, 1, OffcAddrZip4_MaxLength)) : null;
  }

  /// <summary>Length of the OFFC_ADDR_COUNTRY attribute.</summary>
  public const int OffcAddrCountry_MaxLength = 10;

  /// <summary>
  /// The value of the OFFC_ADDR_COUNTRY attribute.
  /// The name of the country for the mailing Office Address tha the Office 
  /// Service Provider represented by this occurrence is assigne to.
  /// </summary>
  [JsonPropertyName("offcAddrCountry")]
  [Member(Index = 19, Type = MemberType.Char, Length
    = OffcAddrCountry_MaxLength, Optional = true)]
  public string OffcAddrCountry
  {
    get => offcAddrCountry;
    set => offcAddrCountry = value != null
      ? TrimEnd(Substring(value, 1, OffcAddrCountry_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the SERV_PROV_SYS_GEN_ID attribute.
  /// The unique number of the Service Provider that is linked to the Office 
  /// Service Provider represented by this occurrence.
  /// </summary>
  [JsonPropertyName("servProvSysGenId")]
  [DefaultValue(0)]
  [Member(Index = 20, Type = MemberType.Number, Length = 5)]
  public int ServProvSysGenId
  {
    get => servProvSysGenId;
    set => servProvSysGenId = value;
  }

  /// <summary>Length of the SERV_PROV_USER_ID attribute.</summary>
  public const int ServProvUserId_MaxLength = 8;

  /// <summary>
  /// The value of the SERV_PROV_USER_ID attribute.
  /// The Kessep sign-on User ID of the Service Provider linked to the Office 
  /// Service Provider represented by this occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 21, Type = MemberType.Char, Length = ServProvUserId_MaxLength)]
    
  public string ServProvUserId
  {
    get => servProvUserId ?? "";
    set => servProvUserId =
      TrimEnd(Substring(value, 1, ServProvUserId_MaxLength));
  }

  /// <summary>
  /// The json value of the ServProvUserId attribute.</summary>
  [JsonPropertyName("servProvUserId")]
  [Computed]
  public string ServProvUserId_Json
  {
    get => NullIf(ServProvUserId, "");
    set => ServProvUserId = value;
  }

  /// <summary>Length of the SERV_PROV_LAST_NAME attribute.</summary>
  public const int ServProvLastName_MaxLength = 17;

  /// <summary>
  /// The value of the SERV_PROV_LAST_NAME attribute.
  /// The last name of the Service Provider linked to the Office Service 
  /// Provider represented by this occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 22, Type = MemberType.Char, Length
    = ServProvLastName_MaxLength)]
  public string ServProvLastName
  {
    get => servProvLastName ?? "";
    set => servProvLastName =
      TrimEnd(Substring(value, 1, ServProvLastName_MaxLength));
  }

  /// <summary>
  /// The json value of the ServProvLastName attribute.</summary>
  [JsonPropertyName("servProvLastName")]
  [Computed]
  public string ServProvLastName_Json
  {
    get => NullIf(ServProvLastName, "");
    set => ServProvLastName = value;
  }

  /// <summary>Length of the SERV_PROV_FIRST_NAME attribute.</summary>
  public const int ServProvFirstName_MaxLength = 12;

  /// <summary>
  /// The value of the SERV_PROV_FIRST_NAME attribute.
  /// The first name of the Service Provider linked to the Office Service 
  /// Provider represented by this occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 23, Type = MemberType.Char, Length
    = ServProvFirstName_MaxLength)]
  public string ServProvFirstName
  {
    get => servProvFirstName ?? "";
    set => servProvFirstName =
      TrimEnd(Substring(value, 1, ServProvFirstName_MaxLength));
  }

  /// <summary>
  /// The json value of the ServProvFirstName attribute.</summary>
  [JsonPropertyName("servProvFirstName")]
  [Computed]
  public string ServProvFirstName_Json
  {
    get => NullIf(ServProvFirstName, "");
    set => ServProvFirstName = value;
  }

  /// <summary>Length of the SERV_PROV_MI attribute.</summary>
  public const int ServProvMi_MaxLength = 1;

  /// <summary>
  /// The value of the SERV_PROV_MI attribute.
  /// The middle initial of the Service Provider linked to the Office Service 
  /// Provider represented by this occurrence
  /// </summary>
  [JsonPropertyName("servProvMi")]
  [Member(Index = 24, Type = MemberType.Char, Length = ServProvMi_MaxLength, Optional
    = true)]
  public string ServProvMi
  {
    get => servProvMi;
    set => servProvMi = value != null
      ? TrimEnd(Substring(value, 1, ServProvMi_MaxLength)) : null;
  }

  /// <summary>Length of the SERV_PROV_ADDR_STREE_1 attribute.</summary>
  public const int ServProvAddrStree1_MaxLength = 25;

  /// <summary>
  /// The value of the SERV_PROV_ADDR_STREE_1 attribute.
  /// Line one of the Street Name/Number for the address for the Service 
  /// Provider linked to the Office Service Provider represented by this
  /// occurrence.
  /// </summary>
  [JsonPropertyName("servProvAddrStree1")]
  [Member(Index = 25, Type = MemberType.Char, Length
    = ServProvAddrStree1_MaxLength, Optional = true)]
  public string ServProvAddrStree1
  {
    get => servProvAddrStree1;
    set => servProvAddrStree1 = value != null
      ? TrimEnd(Substring(value, 1, ServProvAddrStree1_MaxLength)) : null;
  }

  /// <summary>Length of the SERV_PROV_ADDR_STREET_2 attribute.</summary>
  public const int ServProvAddrStreet2_MaxLength = 25;

  /// <summary>
  /// The value of the SERV_PROV_ADDR_STREET_2 attribute.
  /// Line two of the Street Name/Number for the address for the Service 
  /// Provider linked to the Office Service Provider represented by this
  /// occurrence.
  /// </summary>
  [JsonPropertyName("servProvAddrStreet2")]
  [Member(Index = 26, Type = MemberType.Char, Length
    = ServProvAddrStreet2_MaxLength, Optional = true)]
  public string ServProvAddrStreet2
  {
    get => servProvAddrStreet2;
    set => servProvAddrStreet2 = value != null
      ? TrimEnd(Substring(value, 1, ServProvAddrStreet2_MaxLength)) : null;
  }

  /// <summary>Length of the SERV_PROV_ADDR_CITY attribute.</summary>
  public const int ServProvAddrCity_MaxLength = 15;

  /// <summary>
  /// The value of the SERV_PROV_ADDR_CITY attribute.
  /// The name of the city for the address for the Service Provider linked to 
  /// the Office Service Provider represented by this occurrence.
  /// </summary>
  [JsonPropertyName("servProvAddrCity")]
  [Member(Index = 27, Type = MemberType.Char, Length
    = ServProvAddrCity_MaxLength, Optional = true)]
  public string ServProvAddrCity
  {
    get => servProvAddrCity;
    set => servProvAddrCity = value != null
      ? TrimEnd(Substring(value, 1, ServProvAddrCity_MaxLength)) : null;
  }

  /// <summary>Length of the SERV_PROV_ADDR_ST attribute.</summary>
  public const int ServProvAddrSt_MaxLength = 2;

  /// <summary>
  /// The value of the SERV_PROV_ADDR_ST attribute.
  /// The two character abbreviation for the state or province of the address 
  /// for the Service Provider linked to the Office Service Provider represented
  /// by this occurrence.
  /// </summary>
  [JsonPropertyName("servProvAddrSt")]
  [Member(Index = 28, Type = MemberType.Char, Length
    = ServProvAddrSt_MaxLength, Optional = true)]
  public string ServProvAddrSt
  {
    get => servProvAddrSt;
    set => servProvAddrSt = value != null
      ? TrimEnd(Substring(value, 1, ServProvAddrSt_MaxLength)) : null;
  }

  /// <summary>Length of the SERV_PROV_ADDR_POSTAL_CODE attribute.</summary>
  public const int ServProvAddrPostalCode_MaxLength = 10;

  /// <summary>
  /// The value of the SERV_PROV_ADDR_POSTAL_CODE attribute.
  /// The postal code of the address for the Service Provider linked to the 
  /// Office Service Provider represented by this occurrence. This is not the
  /// zip code.
  /// </summary>
  [JsonPropertyName("servProvAddrPostalCode")]
  [Member(Index = 29, Type = MemberType.Char, Length
    = ServProvAddrPostalCode_MaxLength, Optional = true)]
  public string ServProvAddrPostalCode
  {
    get => servProvAddrPostalCode;
    set => servProvAddrPostalCode = value != null
      ? TrimEnd(Substring(value, 1, ServProvAddrPostalCode_MaxLength)) : null;
  }

  /// <summary>Length of the SERV_PROV_ADDR_ZIP attribute.</summary>
  public const int ServProvAddrZip_MaxLength = 5;

  /// <summary>
  /// The value of the SERV_PROV_ADDR_ZIP attribute.
  /// The five digit zip code of the address for the Service Provider linked to 
  /// the Office Service Provider represented by this occurrence.	
  /// </summary>
  [JsonPropertyName("servProvAddrZip")]
  [Member(Index = 30, Type = MemberType.Char, Length
    = ServProvAddrZip_MaxLength, Optional = true)]
  public string ServProvAddrZip
  {
    get => servProvAddrZip;
    set => servProvAddrZip = value != null
      ? TrimEnd(Substring(value, 1, ServProvAddrZip_MaxLength)) : null;
  }

  /// <summary>Length of the SERV_PROV_ADDR_ZIP4 attribute.</summary>
  public const int ServProvAddrZip4_MaxLength = 4;

  /// <summary>
  /// The value of the SERV_PROV_ADDR_ZIP4 attribute.
  /// The four digit zip code extension of the address for the Service Provider 
  /// linked to the Office Service Provider represented by this occurrence.
  /// </summary>
  [JsonPropertyName("servProvAddrZip4")]
  [Member(Index = 31, Type = MemberType.Char, Length
    = ServProvAddrZip4_MaxLength, Optional = true)]
  public string ServProvAddrZip4
  {
    get => servProvAddrZip4;
    set => servProvAddrZip4 = value != null
      ? TrimEnd(Substring(value, 1, ServProvAddrZip4_MaxLength)) : null;
  }

  /// <summary>Length of the SERV_PROV_ADDR_COUNTRY attribute.</summary>
  public const int ServProvAddrCountry_MaxLength = 10;

  /// <summary>
  /// The value of the SERV_PROV_ADDR_COUNTRY attribute.
  /// The country name of the address for the Service Provider linked to the 
  /// Office Service Provider represented by this occurrence.
  /// </summary>
  [JsonPropertyName("servProvAddrCountry")]
  [Member(Index = 32, Type = MemberType.Char, Length
    = ServProvAddrCountry_MaxLength, Optional = true)]
  public string ServProvAddrCountry
  {
    get => servProvAddrCountry;
    set => servProvAddrCountry = value != null
      ? TrimEnd(Substring(value, 1, ServProvAddrCountry_MaxLength)) : null;
  }

  /// <summary>Length of the OFFC_ADDR_ZIP3 attribute.</summary>
  public const int OffcAddrZip3_MaxLength = 3;

  /// <summary>
  /// The value of the OFFC_ADDR_ZIP3 attribute.
  /// the zip3 of the income source address. Generated
  /// </summary>
  [JsonPropertyName("offcAddrZip3")]
  [Member(Index = 33, Type = MemberType.Char, Length = OffcAddrZip3_MaxLength, Optional
    = true)]
  public string OffcAddrZip3
  {
    get => offcAddrZip3;
    set => offcAddrZip3 = value != null
      ? TrimEnd(Substring(value, 1, OffcAddrZip3_MaxLength)) : null;
  }

  /// <summary>Length of the SERV_PROV_ADDR_ZIP3 attribute.</summary>
  public const int ServProvAddrZip3_MaxLength = 3;

  /// <summary>
  /// The value of the SERV_PROV_ADDR_ZIP3 attribute.
  /// The  three-digit ZIP add on to four-digit and five-digit ZIP
  /// </summary>
  [JsonPropertyName("servProvAddrZip3")]
  [Member(Index = 34, Type = MemberType.Char, Length
    = ServProvAddrZip3_MaxLength, Optional = true)]
  public string ServProvAddrZip3
  {
    get => servProvAddrZip3;
    set => servProvAddrZip3 = value != null
      ? TrimEnd(Substring(value, 1, ServProvAddrZip3_MaxLength)) : null;
  }

  private int ospWorkPhoneNumber;
  private int ospWorkPhoneAreaCode;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string ospWorkPhoneExtension;
  private string ospCertificationNumber;
  private int? ospLocalContactCode;
  private string ospRoleCode;
  private DateTime? ospEffectiveDate;
  private int officeSysGenId;
  private string officeName;
  private string offcAddrStreet1;
  private string offcAddrStreet2;
  private string offcAddrCity;
  private string offcAddrState;
  private string offcAddrPostalCode;
  private string offcAddrZip;
  private string offcAddrZip4;
  private string offcAddrCountry;
  private int servProvSysGenId;
  private string servProvUserId;
  private string servProvLastName;
  private string servProvFirstName;
  private string servProvMi;
  private string servProvAddrStree1;
  private string servProvAddrStreet2;
  private string servProvAddrCity;
  private string servProvAddrSt;
  private string servProvAddrPostalCode;
  private string servProvAddrZip;
  private string servProvAddrZip4;
  private string servProvAddrCountry;
  private string offcAddrZip3;
  private string servProvAddrZip3;
}
