// The source file: BANKRUPTCY, ID: 371430698, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: OBLGESTB
/// This entity contains information regarding bankruptcies filed by a CSE 
/// Person.
/// </summary>
[Serializable]
public partial class Bankruptcy: Heap, ICloneable
{
  /// <summary>Default constructor.</summary>
  public Bankruptcy()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public Bankruptcy(Bankruptcy that):
    base(that)
  {
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new Bankruptcy Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>
  /// The value of the BTO_FAX_AREA_CODE attribute.
  /// The 3-digit area code for fax number of the Bankruptcy Trustee Officer.
  /// </summary>
  [JsonPropertyName("btoFaxAreaCode")]
  [Member(Index = 1, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? BtoFaxAreaCode
  {
    get => Get<int?>("btoFaxAreaCode");
    set => Set("btoFaxAreaCode", value);
  }

  /// <summary>
  /// The value of the BTO_PHONE_AREA_CODE attribute.
  /// The 3-digit area code for the phone number of the Bankruptcy Trustee 
  /// Officer.
  /// </summary>
  [JsonPropertyName("btoPhoneAreaCode")]
  [Member(Index = 2, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? BtoPhoneAreaCode
  {
    get => Get<int?>("btoPhoneAreaCode");
    set => Set("btoPhoneAreaCode", value);
  }

  /// <summary>
  /// The value of the BDC_PHONE_AREA_CODE attribute.
  /// The 3-digit area code for the phone number of the Bankruptcy District 
  /// Court.
  /// </summary>
  [JsonPropertyName("bdcPhoneAreaCode")]
  [Member(Index = 3, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? BdcPhoneAreaCode
  {
    get => Get<int?>("bdcPhoneAreaCode");
    set => Set("bdcPhoneAreaCode", value);
  }

  /// <summary>
  /// The value of the AP_ATTORNEY_FAX_AREA_CODE attribute.
  /// The 3-digit area code for the fax number of the AP's Bankruptcy Attorney.
  /// </summary>
  [JsonPropertyName("apAttorneyFaxAreaCode")]
  [Member(Index = 4, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? ApAttorneyFaxAreaCode
  {
    get => Get<int?>("apAttorneyFaxAreaCode");
    set => Set("apAttorneyFaxAreaCode", value);
  }

  /// <summary>
  /// The value of the AP_ATTORNEY_PHONE_AREA_CODE attribute.
  /// The 3-digit area code for the phone number of the AP's Bankruptcy 
  /// Attorney.
  /// </summary>
  [JsonPropertyName("apAttorneyPhoneAreaCode")]
  [Member(Index = 5, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? ApAttorneyPhoneAreaCode
  {
    get => Get<int?>("apAttorneyPhoneAreaCode");
    set => Set("apAttorneyPhoneAreaCode", value);
  }

  /// <summary>Length of the BTO_PHONE_EXT attribute.</summary>
  public const int BtoPhoneExt_MaxLength = 5;

  /// <summary>
  /// The value of the BTO_PHONE_EXT attribute.
  /// The 5 digit extension for the phone number of the Bankruptcy Trustee 
  /// Officer.
  /// </summary>
  [JsonPropertyName("btoPhoneExt")]
  [Member(Index = 6, Type = MemberType.Char, Length = BtoPhoneExt_MaxLength, Optional
    = true)]
  public string BtoPhoneExt
  {
    get => Get<string>("btoPhoneExt");
    set => Set(
      "btoPhoneExt", TrimEnd(Substring(value, 1, BtoPhoneExt_MaxLength)));
  }

  /// <summary>Length of the BTO_FAX_EXT attribute.</summary>
  public const int BtoFaxExt_MaxLength = 5;

  /// <summary>
  /// The value of the BTO_FAX_EXT attribute.
  /// The 5 digit extension for the fax number of the bankruptcy trustee 
  /// officer.
  /// </summary>
  [JsonPropertyName("btoFaxExt")]
  [Member(Index = 7, Type = MemberType.Char, Length = BtoFaxExt_MaxLength, Optional
    = true)]
  public string BtoFaxExt
  {
    get => Get<string>("btoFaxExt");
    set => Set("btoFaxExt", TrimEnd(Substring(value, 1, BtoFaxExt_MaxLength)));
  }

  /// <summary>Length of the BDC_PHONE_EXT attribute.</summary>
  public const int BdcPhoneExt_MaxLength = 5;

  /// <summary>
  /// The value of the BDC_PHONE_EXT attribute.
  /// The 5 digit extension number for the phone number of the bankruptcy 
  /// district court.
  /// </summary>
  [JsonPropertyName("bdcPhoneExt")]
  [Member(Index = 8, Type = MemberType.Char, Length = BdcPhoneExt_MaxLength, Optional
    = true)]
  public string BdcPhoneExt
  {
    get => Get<string>("bdcPhoneExt");
    set => Set(
      "bdcPhoneExt", TrimEnd(Substring(value, 1, BdcPhoneExt_MaxLength)));
  }

  /// <summary>Length of the AP_ATTORNEY_FAX_EXT attribute.</summary>
  public const int ApAttorneyFaxExt_MaxLength = 5;

  /// <summary>
  /// The value of the AP_ATTORNEY_FAX_EXT attribute.
  /// The 5 digit extension number of the AP's bankruptcy attorney's fax number.
  /// </summary>
  [JsonPropertyName("apAttorneyFaxExt")]
  [Member(Index = 9, Type = MemberType.Char, Length
    = ApAttorneyFaxExt_MaxLength, Optional = true)]
  public string ApAttorneyFaxExt
  {
    get => Get<string>("apAttorneyFaxExt");
    set => Set(
      "apAttorneyFaxExt",
      TrimEnd(Substring(value, 1, ApAttorneyFaxExt_MaxLength)));
  }

  /// <summary>Length of the AP_ATTORNEY_PHONE_EXT attribute.</summary>
  public const int ApAttorneyPhoneExt_MaxLength = 5;

  /// <summary>
  /// The value of the AP_ATTORNEY_PHONE_EXT attribute.
  /// The 5 digit extension of the phone number of the AP's Bankruptcy attorney.
  /// </summary>
  [JsonPropertyName("apAttorneyPhoneExt")]
  [Member(Index = 10, Type = MemberType.Char, Length
    = ApAttorneyPhoneExt_MaxLength, Optional = true)]
  public string ApAttorneyPhoneExt
  {
    get => Get<string>("apAttorneyPhoneExt");
    set => Set(
      "apAttorneyPhoneExt", TrimEnd(Substring(value, 1,
      ApAttorneyPhoneExt_MaxLength)));
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// The attribute, which together with the relation with CSE_PERSON, uniquely 
  /// identifies one occurrence of BANKRUPTCY.
  /// This will be generated by the system, as 1 for the first Bankruptcy 
  /// occurrence for the CSE Person, 2 for the next Bankruptcy occurrence and so
  /// on.
  /// </summary>
  [JsonPropertyName("identifier")]
  [DefaultValue(0)]
  [Member(Index = 11, Type = MemberType.Number, Length = 3)]
  public int Identifier
  {
    get => Get<int?>("identifier") ?? 0;
    set => Set("identifier", value == 0 ? null : value as int?);
  }

  /// <summary>Length of the BANKRUPTCY_COURT_ACTION_NO attribute.</summary>
  public const int BankruptcyCourtActionNo_MaxLength = 17;

  /// <summary>
  /// The value of the BANKRUPTCY_COURT_ACTION_NO attribute.
  /// Alphanumeric number that describes the bankruptcy action.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length
    = BankruptcyCourtActionNo_MaxLength)]
  public string BankruptcyCourtActionNo
  {
    get => Get<string>("bankruptcyCourtActionNo") ?? "";
    set => Set(
      "bankruptcyCourtActionNo", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, BankruptcyCourtActionNo_MaxLength)));
  }

  /// <summary>
  /// The json value of the BankruptcyCourtActionNo attribute.</summary>
  [JsonPropertyName("bankruptcyCourtActionNo")]
  [Computed]
  public string BankruptcyCourtActionNo_Json
  {
    get => NullIf(BankruptcyCourtActionNo, "");
    set => BankruptcyCourtActionNo = value;
  }

  /// <summary>Length of the BANKRUPTCY_TYPE attribute.</summary>
  public const int BankruptcyType_MaxLength = 2;

  /// <summary>
  /// The value of the BANKRUPTCY_TYPE attribute.
  /// This indicates the type of bankruptcy being filed.
  /// Examples include Chapter 11 and Chapter 7.
  /// The permitted values are maintained in CODE_VALUE entity.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length = BankruptcyType_MaxLength)]
    
  public string BankruptcyType
  {
    get => Get<string>("bankruptcyType") ?? "";
    set => Set(
      "bankruptcyType", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, BankruptcyType_MaxLength)));
  }

  /// <summary>
  /// The json value of the BankruptcyType attribute.</summary>
  [JsonPropertyName("bankruptcyType")]
  [Computed]
  public string BankruptcyType_Json
  {
    get => NullIf(BankruptcyType, "");
    set => BankruptcyType = value;
  }

  /// <summary>
  /// The value of the BANKRUPTCY_FILING_DATE attribute.
  /// The date the Bankruptcy is filed in the tribunal.
  /// </summary>
  [JsonPropertyName("bankruptcyFilingDate")]
  [Member(Index = 14, Type = MemberType.Date)]
  public DateTime? BankruptcyFilingDate
  {
    get => Get<DateTime?>("bankruptcyFilingDate");
    set => Set("bankruptcyFilingDate", value);
  }

  /// <summary>
  /// The value of the BANKRUPTCY_DISCHARGE_DATE attribute.
  /// This date is the first day the bankruptcy is final.
  /// </summary>
  [JsonPropertyName("bankruptcyDischargeDate")]
  [Member(Index = 15, Type = MemberType.Date, Optional = true)]
  public DateTime? BankruptcyDischargeDate
  {
    get => Get<DateTime?>("bankruptcyDischargeDate");
    set => Set("bankruptcyDischargeDate", value);
  }

  /// <summary>
  /// The value of the BANKRUPTCY_CONFIRMATION_DATE attribute.
  /// The date the bankruptcy plan is approved by the tribunal and the terms 
  /// become effective.
  /// </summary>
  [JsonPropertyName("bankruptcyConfirmationDate")]
  [Member(Index = 16, Type = MemberType.Date, Optional = true)]
  public DateTime? BankruptcyConfirmationDate
  {
    get => Get<DateTime?>("bankruptcyConfirmationDate");
    set => Set("bankruptcyConfirmationDate", value);
  }

  /// <summary>
  /// The value of the EXPECTED_BKRP_DISCHARGE_DATE attribute.
  /// THIS IS THE EXPECTED DISCHARGE DATE FOR BANKRUPTCY
  /// </summary>
  [JsonPropertyName("expectedBkrpDischargeDate")]
  [Member(Index = 17, Type = MemberType.Date, Optional = true)]
  public DateTime? ExpectedBkrpDischargeDate
  {
    get => Get<DateTime?>("expectedBkrpDischargeDate");
    set => Set("expectedBkrpDischargeDate", value);
  }

  /// <summary>
  /// The value of the PROOF_OF_CLAIM_FILED_DATE attribute.
  /// The date that the Proof of Claim is filed.
  /// </summary>
  [JsonPropertyName("proofOfClaimFiledDate")]
  [Member(Index = 18, Type = MemberType.Date, Optional = true)]
  public DateTime? ProofOfClaimFiledDate
  {
    get => Get<DateTime?>("proofOfClaimFiledDate");
    set => Set("proofOfClaimFiledDate", value);
  }

  /// <summary>
  /// The value of the DATE_REQUESTED_MOTION_TO_LIFT attribute.
  /// The date we requested the motion to be lifted at the Bankruptcy court.
  /// </summary>
  [JsonPropertyName("dateRequestedMotionToLift")]
  [Member(Index = 19, Type = MemberType.Date, Optional = true)]
  public DateTime? DateRequestedMotionToLift
  {
    get => Get<DateTime?>("dateRequestedMotionToLift");
    set => Set("dateRequestedMotionToLift", value);
  }

  /// <summary>
  /// The value of the DATE_MOTION_TO_LIFT_GRANTED attribute.
  /// The date the motion to lift Stay granted at the Bankruptcy Court.
  /// </summary>
  [JsonPropertyName("dateMotionToLiftGranted")]
  [Member(Index = 20, Type = MemberType.Date, Optional = true)]
  public DateTime? DateMotionToLiftGranted
  {
    get => Get<DateTime?>("dateMotionToLiftGranted");
    set => Set("dateMotionToLiftGranted", value);
  }

  /// <summary>Length of the TRUSTEE_LAST_NAME attribute.</summary>
  public const int TrusteeLastName_MaxLength = 17;

  /// <summary>
  /// The value of the TRUSTEE_LAST_NAME attribute.
  /// The last name of the person who is entrusted with the property of another 
  /// in a bankruptcy proceeding.
  /// </summary>
  [JsonPropertyName("trusteeLastName")]
  [Member(Index = 21, Type = MemberType.Char, Length
    = TrusteeLastName_MaxLength, Optional = true)]
  public string TrusteeLastName
  {
    get => Get<string>("trusteeLastName");
    set => Set(
      "trusteeLastName",
      TrimEnd(Substring(value, 1, TrusteeLastName_MaxLength)));
  }

  /// <summary>Length of the TRUSTEE_FIRST_NAME attribute.</summary>
  public const int TrusteeFirstName_MaxLength = 12;

  /// <summary>
  /// The value of the TRUSTEE_FIRST_NAME attribute.
  /// The first name of the person who is entrusted with the property of another
  /// in a bankruptcy proceeding.
  /// </summary>
  [JsonPropertyName("trusteeFirstName")]
  [Member(Index = 22, Type = MemberType.Char, Length
    = TrusteeFirstName_MaxLength, Optional = true)]
  public string TrusteeFirstName
  {
    get => Get<string>("trusteeFirstName");
    set => Set(
      "trusteeFirstName",
      TrimEnd(Substring(value, 1, TrusteeFirstName_MaxLength)));
  }

  /// <summary>Length of the TRUSTEE_MIDDLE_INT attribute.</summary>
  public const int TrusteeMiddleInt_MaxLength = 1;

  /// <summary>
  /// The value of the TRUSTEE_MIDDLE_INT attribute.
  /// The middle initial of the person who is entrusted with the property of 
  /// another in a bankruptcy proceeding.
  /// </summary>
  [JsonPropertyName("trusteeMiddleInt")]
  [Member(Index = 23, Type = MemberType.Char, Length
    = TrusteeMiddleInt_MaxLength, Optional = true)]
  public string TrusteeMiddleInt
  {
    get => Get<string>("trusteeMiddleInt");
    set => Set(
      "trusteeMiddleInt",
      TrimEnd(Substring(value, 1, TrusteeMiddleInt_MaxLength)));
  }

  /// <summary>Length of the TRUSTEE_SUFFIX attribute.</summary>
  public const int TrusteeSuffix_MaxLength = 3;

  /// <summary>
  /// The value of the TRUSTEE_SUFFIX attribute.
  /// The name suffix of the person who is entrusted with the property of 
  /// another in a bankruptcy proceeding.
  /// Ex. Jr, Sr, I, II, etc.
  /// </summary>
  [JsonPropertyName("trusteeSuffix")]
  [Member(Index = 24, Type = MemberType.Char, Length
    = TrusteeSuffix_MaxLength, Optional = true)]
  public string TrusteeSuffix
  {
    get => Get<string>("trusteeSuffix");
    set => Set(
      "trusteeSuffix", TrimEnd(Substring(value, 1, TrusteeSuffix_MaxLength)));
  }

  /// <summary>
  /// The value of the BTO_PHONE_NO attribute.
  /// The 7-digit phone number of the Bankruptcy Trusty Officer.
  /// </summary>
  [JsonPropertyName("btoPhoneNo")]
  [Member(Index = 25, Type = MemberType.Number, Length = 7, Optional = true)]
  public int? BtoPhoneNo
  {
    get => Get<int?>("btoPhoneNo");
    set => Set("btoPhoneNo", value);
  }

  /// <summary>
  /// The value of the BTO_FAX attribute.
  /// The 7-digit Fax number of the Bankruptcy Trustee Officer.
  /// </summary>
  [JsonPropertyName("btoFax")]
  [Member(Index = 26, Type = MemberType.Number, Length = 7, Optional = true)]
  public int? BtoFax
  {
    get => Get<int?>("btoFax");
    set => Set("btoFax", value);
  }

  /// <summary>Length of the BTO_ADDR_STREET_1 attribute.</summary>
  public const int BtoAddrStreet1_MaxLength = 25;

  /// <summary>
  /// The value of the BTO_ADDR_STREET_1 attribute.
  /// Bankruptcy Trustee Officer address street-1
  /// </summary>
  [JsonPropertyName("btoAddrStreet1")]
  [Member(Index = 27, Type = MemberType.Char, Length
    = BtoAddrStreet1_MaxLength, Optional = true)]
  public string BtoAddrStreet1
  {
    get => Get<string>("btoAddrStreet1");
    set => Set(
      "btoAddrStreet1", TrimEnd(Substring(value, 1, BtoAddrStreet1_MaxLength)));
      
  }

  /// <summary>Length of the BTO_ADDR_STREET_2 attribute.</summary>
  public const int BtoAddrStreet2_MaxLength = 25;

  /// <summary>
  /// The value of the BTO_ADDR_STREET_2 attribute.
  /// Bankruptcy Trustee Officer address street-2
  /// </summary>
  [JsonPropertyName("btoAddrStreet2")]
  [Member(Index = 28, Type = MemberType.Char, Length
    = BtoAddrStreet2_MaxLength, Optional = true)]
  public string BtoAddrStreet2
  {
    get => Get<string>("btoAddrStreet2");
    set => Set(
      "btoAddrStreet2", TrimEnd(Substring(value, 1, BtoAddrStreet2_MaxLength)));
      
  }

  /// <summary>Length of the BTO_ADDR_CITY attribute.</summary>
  public const int BtoAddrCity_MaxLength = 15;

  /// <summary>
  /// The value of the BTO_ADDR_CITY attribute.
  /// Bankruptcy Trustee Officer address city
  /// </summary>
  [JsonPropertyName("btoAddrCity")]
  [Member(Index = 29, Type = MemberType.Char, Length = BtoAddrCity_MaxLength, Optional
    = true)]
  public string BtoAddrCity
  {
    get => Get<string>("btoAddrCity");
    set => Set(
      "btoAddrCity", TrimEnd(Substring(value, 1, BtoAddrCity_MaxLength)));
  }

  /// <summary>Length of the BTO_ADDR_STATE attribute.</summary>
  public const int BtoAddrState_MaxLength = 2;

  /// <summary>
  /// The value of the BTO_ADDR_STATE attribute.
  /// Bankruptcy Trustee Officer address state
  /// </summary>
  [JsonPropertyName("btoAddrState")]
  [Member(Index = 30, Type = MemberType.Char, Length = BtoAddrState_MaxLength, Optional
    = true)]
  public string BtoAddrState
  {
    get => Get<string>("btoAddrState");
    set => Set(
      "btoAddrState", TrimEnd(Substring(value, 1, BtoAddrState_MaxLength)));
  }

  /// <summary>Length of the BTO_ADDR_ZIP5 attribute.</summary>
  public const int BtoAddrZip5_MaxLength = 5;

  /// <summary>
  /// The value of the BTO_ADDR_ZIP5 attribute.
  /// Bankruptcy Trustee Officer address 5-digit ZIP code.
  /// </summary>
  [JsonPropertyName("btoAddrZip5")]
  [Member(Index = 31, Type = MemberType.Char, Length = BtoAddrZip5_MaxLength, Optional
    = true)]
  public string BtoAddrZip5
  {
    get => Get<string>("btoAddrZip5");
    set => Set(
      "btoAddrZip5", TrimEnd(Substring(value, 1, BtoAddrZip5_MaxLength)));
  }

  /// <summary>Length of the BTO_ADDR_ZIP4 attribute.</summary>
  public const int BtoAddrZip4_MaxLength = 4;

  /// <summary>
  /// The value of the BTO_ADDR_ZIP4 attribute.
  /// Bankruptcy Trustee Officer address 4-digit ZIP extension.
  /// </summary>
  [JsonPropertyName("btoAddrZip4")]
  [Member(Index = 32, Type = MemberType.Char, Length = BtoAddrZip4_MaxLength, Optional
    = true)]
  public string BtoAddrZip4
  {
    get => Get<string>("btoAddrZip4");
    set => Set(
      "btoAddrZip4", TrimEnd(Substring(value, 1, BtoAddrZip4_MaxLength)));
  }

  /// <summary>Length of the BTO_ADDR_ZIP3 attribute.</summary>
  public const int BtoAddrZip3_MaxLength = 3;

  /// <summary>
  /// The value of the BTO_ADDR_ZIP3 attribute.
  /// Bankruptcy Trustee Officer address 3-digit ZIP code extension.
  /// </summary>
  [JsonPropertyName("btoAddrZip3")]
  [Member(Index = 33, Type = MemberType.Char, Length = BtoAddrZip3_MaxLength, Optional
    = true)]
  public string BtoAddrZip3
  {
    get => Get<string>("btoAddrZip3");
    set => Set(
      "btoAddrZip3", TrimEnd(Substring(value, 1, BtoAddrZip3_MaxLength)));
  }

  /// <summary>Length of the BANKRUPTCY_DISTRICT_COURT attribute.</summary>
  public const int BankruptcyDistrictCourt_MaxLength = 30;

  /// <summary>
  /// The value of the BANKRUPTCY_DISTRICT_COURT attribute.
  /// The name of the court where that bankruptcy documents were filed.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 34, Type = MemberType.Char, Length
    = BankruptcyDistrictCourt_MaxLength)]
  public string BankruptcyDistrictCourt
  {
    get => Get<string>("bankruptcyDistrictCourt") ?? "";
    set => Set(
      "bankruptcyDistrictCourt", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, BankruptcyDistrictCourt_MaxLength)));
  }

  /// <summary>
  /// The json value of the BankruptcyDistrictCourt attribute.</summary>
  [JsonPropertyName("bankruptcyDistrictCourt")]
  [Computed]
  public string BankruptcyDistrictCourt_Json
  {
    get => NullIf(BankruptcyDistrictCourt, "");
    set => BankruptcyDistrictCourt = value;
  }

  /// <summary>
  /// The value of the BDC_PHONE_NO attribute.
  /// The 7-digit phone number of the Bankruptcy District Court.
  /// </summary>
  [JsonPropertyName("bdcPhoneNo")]
  [Member(Index = 35, Type = MemberType.Number, Length = 7, Optional = true)]
  public int? BdcPhoneNo
  {
    get => Get<int?>("bdcPhoneNo");
    set => Set("bdcPhoneNo", value);
  }

  /// <summary>Length of the BDC_ADDR_STREET_1 attribute.</summary>
  public const int BdcAddrStreet1_MaxLength = 25;

  /// <summary>
  /// The value of the BDC_ADDR_STREET_1 attribute.
  /// Bankruptcy district court address street-1
  /// </summary>
  [JsonPropertyName("bdcAddrStreet1")]
  [Member(Index = 36, Type = MemberType.Char, Length
    = BdcAddrStreet1_MaxLength, Optional = true)]
  public string BdcAddrStreet1
  {
    get => Get<string>("bdcAddrStreet1");
    set => Set(
      "bdcAddrStreet1", TrimEnd(Substring(value, 1, BdcAddrStreet1_MaxLength)));
      
  }

  /// <summary>Length of the BDC_ADDR_STREET_2 attribute.</summary>
  public const int BdcAddrStreet2_MaxLength = 25;

  /// <summary>
  /// The value of the BDC_ADDR_STREET_2 attribute.
  /// bankruptcy district court address street-2
  /// </summary>
  [JsonPropertyName("bdcAddrStreet2")]
  [Member(Index = 37, Type = MemberType.Char, Length
    = BdcAddrStreet2_MaxLength, Optional = true)]
  public string BdcAddrStreet2
  {
    get => Get<string>("bdcAddrStreet2");
    set => Set(
      "bdcAddrStreet2", TrimEnd(Substring(value, 1, BdcAddrStreet2_MaxLength)));
      
  }

  /// <summary>Length of the BDC_ADDR_CITY attribute.</summary>
  public const int BdcAddrCity_MaxLength = 15;

  /// <summary>
  /// The value of the BDC_ADDR_CITY attribute.
  /// Bankruptcy district court address city
  /// </summary>
  [JsonPropertyName("bdcAddrCity")]
  [Member(Index = 38, Type = MemberType.Char, Length = BdcAddrCity_MaxLength, Optional
    = true)]
  public string BdcAddrCity
  {
    get => Get<string>("bdcAddrCity");
    set => Set(
      "bdcAddrCity", TrimEnd(Substring(value, 1, BdcAddrCity_MaxLength)));
  }

  /// <summary>Length of the BDC_ADDR_STATE attribute.</summary>
  public const int BdcAddrState_MaxLength = 2;

  /// <summary>
  /// The value of the BDC_ADDR_STATE attribute.
  /// Bankruptcy district court address state
  /// </summary>
  [JsonPropertyName("bdcAddrState")]
  [Member(Index = 39, Type = MemberType.Char, Length = BdcAddrState_MaxLength, Optional
    = true)]
  public string BdcAddrState
  {
    get => Get<string>("bdcAddrState");
    set => Set(
      "bdcAddrState", TrimEnd(Substring(value, 1, BdcAddrState_MaxLength)));
  }

  /// <summary>Length of the BDC_ADDR_ZIP5 attribute.</summary>
  public const int BdcAddrZip5_MaxLength = 5;

  /// <summary>
  /// The value of the BDC_ADDR_ZIP5 attribute.
  /// Bankruptcy district court address 5-digit ZIP code.
  /// </summary>
  [JsonPropertyName("bdcAddrZip5")]
  [Member(Index = 40, Type = MemberType.Char, Length = BdcAddrZip5_MaxLength, Optional
    = true)]
  public string BdcAddrZip5
  {
    get => Get<string>("bdcAddrZip5");
    set => Set(
      "bdcAddrZip5", TrimEnd(Substring(value, 1, BdcAddrZip5_MaxLength)));
  }

  /// <summary>Length of the BDC_ADDR_ZIP4 attribute.</summary>
  public const int BdcAddrZip4_MaxLength = 4;

  /// <summary>
  /// The value of the BDC_ADDR_ZIP4 attribute.
  /// Bankruptcy district court address 4-digit ZIP extension.
  /// </summary>
  [JsonPropertyName("bdcAddrZip4")]
  [Member(Index = 41, Type = MemberType.Char, Length = BdcAddrZip4_MaxLength, Optional
    = true)]
  public string BdcAddrZip4
  {
    get => Get<string>("bdcAddrZip4");
    set => Set(
      "bdcAddrZip4", TrimEnd(Substring(value, 1, BdcAddrZip4_MaxLength)));
  }

  /// <summary>Length of the BDC_ADDR_ZIP3 attribute.</summary>
  public const int BdcAddrZip3_MaxLength = 3;

  /// <summary>
  /// The value of the BDC_ADDR_ZIP3 attribute.
  /// Bankruptcy district court address 3-digit ZIP code extension.
  /// </summary>
  [JsonPropertyName("bdcAddrZip3")]
  [Member(Index = 42, Type = MemberType.Char, Length = BdcAddrZip3_MaxLength, Optional
    = true)]
  public string BdcAddrZip3
  {
    get => Get<string>("bdcAddrZip3");
    set => Set(
      "bdcAddrZip3", TrimEnd(Substring(value, 1, BdcAddrZip3_MaxLength)));
  }

  /// <summary>Length of the AP_ATTORNEY_FIRM_NAME attribute.</summary>
  public const int ApAttorneyFirmName_MaxLength = 30;

  /// <summary>
  /// The value of the AP_ATTORNEY_FIRM_NAME attribute.
  /// Name of AP's Bankruptcy Attorney's Firm.
  /// </summary>
  [JsonPropertyName("apAttorneyFirmName")]
  [Member(Index = 43, Type = MemberType.Char, Length
    = ApAttorneyFirmName_MaxLength, Optional = true)]
  public string ApAttorneyFirmName
  {
    get => Get<string>("apAttorneyFirmName");
    set => Set(
      "apAttorneyFirmName", TrimEnd(Substring(value, 1,
      ApAttorneyFirmName_MaxLength)));
  }

  /// <summary>Length of the AP_ATTORNEY_LAST_NAME attribute.</summary>
  public const int ApAttorneyLastName_MaxLength = 17;

  /// <summary>
  /// The value of the AP_ATTORNEY_LAST_NAME attribute.
  /// The last name of the AP Bankruptcy attorney
  /// </summary>
  [JsonPropertyName("apAttorneyLastName")]
  [Member(Index = 44, Type = MemberType.Char, Length
    = ApAttorneyLastName_MaxLength, Optional = true)]
  public string ApAttorneyLastName
  {
    get => Get<string>("apAttorneyLastName");
    set => Set(
      "apAttorneyLastName", TrimEnd(Substring(value, 1,
      ApAttorneyLastName_MaxLength)));
  }

  /// <summary>Length of the AP_ATTORNEY_FIRST_NAME attribute.</summary>
  public const int ApAttorneyFirstName_MaxLength = 12;

  /// <summary>
  /// The value of the AP_ATTORNEY_FIRST_NAME attribute.
  /// The first name of the AP Bankruptcy attorney
  /// </summary>
  [JsonPropertyName("apAttorneyFirstName")]
  [Member(Index = 45, Type = MemberType.Char, Length
    = ApAttorneyFirstName_MaxLength, Optional = true)]
  public string ApAttorneyFirstName
  {
    get => Get<string>("apAttorneyFirstName");
    set => Set(
      "apAttorneyFirstName", TrimEnd(Substring(value, 1,
      ApAttorneyFirstName_MaxLength)));
  }

  /// <summary>Length of the AP_ATTORNEY_MI attribute.</summary>
  public const int ApAttorneyMi_MaxLength = 1;

  /// <summary>
  /// The value of the AP_ATTORNEY_MI attribute.
  /// The middle initial of the AP Bankruptcy attorney
  /// </summary>
  [JsonPropertyName("apAttorneyMi")]
  [Member(Index = 46, Type = MemberType.Char, Length = ApAttorneyMi_MaxLength, Optional
    = true)]
  public string ApAttorneyMi
  {
    get => Get<string>("apAttorneyMi");
    set => Set(
      "apAttorneyMi", TrimEnd(Substring(value, 1, ApAttorneyMi_MaxLength)));
  }

  /// <summary>Length of the AP_ATTORNEY_SUFFIX attribute.</summary>
  public const int ApAttorneySuffix_MaxLength = 3;

  /// <summary>
  /// The value of the AP_ATTORNEY_SUFFIX attribute.
  /// The name suffix of the AP's Attorney.
  /// Ex. Jr, Sr, I, II, etc.
  /// </summary>
  [JsonPropertyName("apAttorneySuffix")]
  [Member(Index = 47, Type = MemberType.Char, Length
    = ApAttorneySuffix_MaxLength, Optional = true)]
  public string ApAttorneySuffix
  {
    get => Get<string>("apAttorneySuffix");
    set => Set(
      "apAttorneySuffix",
      TrimEnd(Substring(value, 1, ApAttorneySuffix_MaxLength)));
  }

  /// <summary>
  /// The value of the AP_ATTORNEY_PHONE_NO attribute.
  /// The 7-digit phone number of the AP's Bankruptcy Attorney.
  /// </summary>
  [JsonPropertyName("apAttorneyPhoneNo")]
  [Member(Index = 48, Type = MemberType.Number, Length = 7, Optional = true)]
  public int? ApAttorneyPhoneNo
  {
    get => Get<int?>("apAttorneyPhoneNo");
    set => Set("apAttorneyPhoneNo", value);
  }

  /// <summary>
  /// The value of the AP_ATTORNEY_FAX attribute.
  /// The 7-digit fax number of the AP's Bankruptcy Attorney.
  /// </summary>
  [JsonPropertyName("apAttorneyFax")]
  [Member(Index = 49, Type = MemberType.Number, Length = 7, Optional = true)]
  public int? ApAttorneyFax
  {
    get => Get<int?>("apAttorneyFax");
    set => Set("apAttorneyFax", value);
  }

  /// <summary>Length of the AP_ATTR_ADDR_STREET_1 attribute.</summary>
  public const int ApAttrAddrStreet1_MaxLength = 25;

  /// <summary>
  /// The value of the AP_ATTR_ADDR_STREET_1 attribute.
  /// AP's Bankruptcy Attorney address street-1
  /// </summary>
  [JsonPropertyName("apAttrAddrStreet1")]
  [Member(Index = 50, Type = MemberType.Char, Length
    = ApAttrAddrStreet1_MaxLength, Optional = true)]
  public string ApAttrAddrStreet1
  {
    get => Get<string>("apAttrAddrStreet1");
    set => Set(
      "apAttrAddrStreet1", TrimEnd(Substring(value, 1,
      ApAttrAddrStreet1_MaxLength)));
  }

  /// <summary>Length of the AP_ATTR_ADDR_STREET_2 attribute.</summary>
  public const int ApAttrAddrStreet2_MaxLength = 25;

  /// <summary>
  /// The value of the AP_ATTR_ADDR_STREET_2 attribute.
  /// AP's Bankruptcy Attorney address street-2
  /// </summary>
  [JsonPropertyName("apAttrAddrStreet2")]
  [Member(Index = 51, Type = MemberType.Char, Length
    = ApAttrAddrStreet2_MaxLength, Optional = true)]
  public string ApAttrAddrStreet2
  {
    get => Get<string>("apAttrAddrStreet2");
    set => Set(
      "apAttrAddrStreet2", TrimEnd(Substring(value, 1,
      ApAttrAddrStreet2_MaxLength)));
  }

  /// <summary>Length of the AP_ATTR_ADDR_CITY attribute.</summary>
  public const int ApAttrAddrCity_MaxLength = 15;

  /// <summary>
  /// The value of the AP_ATTR_ADDR_CITY attribute.
  /// AP's Bankruptcy Attorney address city
  /// </summary>
  [JsonPropertyName("apAttrAddrCity")]
  [Member(Index = 52, Type = MemberType.Char, Length
    = ApAttrAddrCity_MaxLength, Optional = true)]
  public string ApAttrAddrCity
  {
    get => Get<string>("apAttrAddrCity");
    set => Set(
      "apAttrAddrCity", TrimEnd(Substring(value, 1, ApAttrAddrCity_MaxLength)));
      
  }

  /// <summary>Length of the AP_ATTR_ADDR_STATE attribute.</summary>
  public const int ApAttrAddrState_MaxLength = 2;

  /// <summary>
  /// The value of the AP_ATTR_ADDR_STATE attribute.
  /// AP's Bankruptcy Attorney address state
  /// </summary>
  [JsonPropertyName("apAttrAddrState")]
  [Member(Index = 53, Type = MemberType.Char, Length
    = ApAttrAddrState_MaxLength, Optional = true)]
  public string ApAttrAddrState
  {
    get => Get<string>("apAttrAddrState");
    set => Set(
      "apAttrAddrState",
      TrimEnd(Substring(value, 1, ApAttrAddrState_MaxLength)));
  }

  /// <summary>Length of the AP_ATTR_ADDR_ZIP5 attribute.</summary>
  public const int ApAttrAddrZip5_MaxLength = 5;

  /// <summary>
  /// The value of the AP_ATTR_ADDR_ZIP5 attribute.
  /// AP's Bankruptcy Attorney address 5-digit ZIP code.
  /// </summary>
  [JsonPropertyName("apAttrAddrZip5")]
  [Member(Index = 54, Type = MemberType.Char, Length
    = ApAttrAddrZip5_MaxLength, Optional = true)]
  public string ApAttrAddrZip5
  {
    get => Get<string>("apAttrAddrZip5");
    set => Set(
      "apAttrAddrZip5", TrimEnd(Substring(value, 1, ApAttrAddrZip5_MaxLength)));
      
  }

  /// <summary>Length of the AP_ATTR_ADDR_ZIP4 attribute.</summary>
  public const int ApAttrAddrZip4_MaxLength = 4;

  /// <summary>
  /// The value of the AP_ATTR_ADDR_ZIP4 attribute.
  /// AP's Bankruptcy Attorney address 4-digit ZIP extension.
  /// </summary>
  [JsonPropertyName("apAttrAddrZip4")]
  [Member(Index = 55, Type = MemberType.Char, Length
    = ApAttrAddrZip4_MaxLength, Optional = true)]
  public string ApAttrAddrZip4
  {
    get => Get<string>("apAttrAddrZip4");
    set => Set(
      "apAttrAddrZip4", TrimEnd(Substring(value, 1, ApAttrAddrZip4_MaxLength)));
      
  }

  /// <summary>Length of the AP_ATTR_ADDR_ZIP3 attribute.</summary>
  public const int ApAttrAddrZip3_MaxLength = 3;

  /// <summary>
  /// The value of the AP_ATTR_ADDR_ZIP3 attribute.
  /// AP's Bankruptcy Attorney address 3-digit ZIP code extension.
  /// </summary>
  [JsonPropertyName("apAttrAddrZip3")]
  [Member(Index = 56, Type = MemberType.Char, Length
    = ApAttrAddrZip3_MaxLength, Optional = true)]
  public string ApAttrAddrZip3
  {
    get => Get<string>("apAttrAddrZip3");
    set => Set(
      "apAttrAddrZip3", TrimEnd(Substring(value, 1, ApAttrAddrZip3_MaxLength)));
      
  }

  /// <summary>Length of the DISCHARGE_DATE_MOD_IND attribute.</summary>
  public const int DischargeDateModInd_MaxLength = 1;

  /// <summary>
  /// The value of the DISCHARGE_DATE_MOD_IND attribute.
  /// To facilitate batch processing of this entity, this indicator is set to a 
  /// value of Y when an occurrence is created and the DISCHARGE_DATE attribute
  /// is not null (otherwise value is N), or, when an occurrence of this entity
  /// is updated and the new DISCHARGE_DATE value is not equal to the old
  /// DISCHARGE_DATE value and the DISCHARGE_DATE is not null (otherwise value
  /// is N). Managed exclusively by the application and never displayed to the
  /// end user.
  /// </summary>
  [JsonPropertyName("dischargeDateModInd")]
  [Member(Index = 57, Type = MemberType.Char, Length
    = DischargeDateModInd_MaxLength, Optional = true)]
  public string DischargeDateModInd
  {
    get => Get<string>("dischargeDateModInd");
    set => Set(
      "dischargeDateModInd", TrimEnd(Substring(value, 1,
      DischargeDateModInd_MaxLength)));
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// User ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 58, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// The value of the CREATED_TIMESTAMP attribute.
  /// Timestamp of creation of the occurrence.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 59, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => Get<DateTime?>("createdTimestamp");
    set => Set("createdTimestamp", value);
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// User ID or Program ID responsible for the last update of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 60, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength)]
  public string LastUpdatedBy
  {
    get => Get<string>("lastUpdatedBy") ?? "";
    set => Set(
      "lastUpdatedBy", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, LastUpdatedBy_MaxLength)));
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
  [Member(Index = 61, Type = MemberType.Timestamp)]
  public DateTime? LastUpdatedTimestamp
  {
    get => Get<DateTime?>("lastUpdatedTimestamp");
    set => Set("lastUpdatedTimestamp", value);
  }

  /// <summary>Length of the NARRATIVE attribute.</summary>
  public const int Narrative_MaxLength = 72;

  /// <summary>
  /// The value of the NARRATIVE attribute.
  /// This is a narrative describing some of the bankruptcy details.
  /// </summary>
  [JsonPropertyName("narrative")]
  [Member(Index = 62, Type = MemberType.Varchar, Length = Narrative_MaxLength, Optional
    = true)]
  public string Narrative
  {
    get => Get<string>("narrative");
    set => Set("narrative", Substring(value, 1, Narrative_MaxLength));
  }

  /// <summary>
  /// The value of the BANKRUPTCY_DISMISS_WITHDRAW_DATE attribute.
  /// This attribute indicates when a bankruptcy has been dismissed or 
  /// withdrawn.
  /// </summary>
  [JsonPropertyName("bankruptcyDismissWithdrawDate")]
  [Member(Index = 63, Type = MemberType.Date, Optional = true)]
  public DateTime? BankruptcyDismissWithdrawDate
  {
    get => Get<DateTime?>("bankruptcyDismissWithdrawDate");
    set => Set("bankruptcyDismissWithdrawDate", value);
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
  [Member(Index = 64, Type = MemberType.Char, Length = CspNumber_MaxLength)]
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
