// The source file: EXTERNAL_OCSE_1099_RESPONSE, ID: 372376984, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

/// <summary>
/// Resp: OBLGEST		
/// This Work Set represents the specifications of the 1099 OCSE return response
/// to the State of Kansas.
/// This data will be received on Tape as 9 track, Odd Parity, EBCDIC, with no 
/// volume Labels - a block size of 31,800 characters and a record size of 600
/// characters.
/// </summary>
[Serializable]
public partial class ExternalOcse1099Response: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ExternalOcse1099Response()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ExternalOcse1099Response(ExternalOcse1099Response that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ExternalOcse1099Response Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ExternalOcse1099Response that)
  {
    base.Assign(that);
    ssn = that.ssn;
    submittingStateCode = that.submittingStateCode;
    localFipsCode = that.localFipsCode;
    csePersonNumber = that.csePersonNumber;
    attribute1099RequestIdentifier = that.attribute1099RequestIdentifier;
    lastName = that.lastName;
    firstName = that.firstName;
    ocseMatchCode = that.ocseMatchCode;
    caseTypeAdcNadc = that.caseTypeAdcNadc;
    courtAdminOrderIndicator = that.courtAdminOrderIndicator;
    payeeName1 = that.payeeName1;
    payeeName2 = that.payeeName2;
    payeeStreet = that.payeeStreet;
    payeeCity = that.payeeCity;
    payeeState = that.payeeState;
    payeeZipCode = that.payeeZipCode;
    payorEin = that.payorEin;
    payorName1 = that.payorName1;
    payorName2 = that.payorName2;
    payorStreet = that.payorStreet;
    payorCityStateZip = that.payorCityStateZip;
    taxYear = that.taxYear;
    accountCode = that.accountCode;
    documentCode = that.documentCode;
    amountInd1 = that.amountInd1;
    amount1 = that.amount1;
    amountInd2 = that.amountInd2;
    amount2 = that.amount2;
    amountInd3 = that.amountInd3;
    amount3 = that.amount3;
    amountInd4 = that.amountInd4;
    amount4 = that.amount4;
    amountInd5 = that.amountInd5;
    amount5 = that.amount5;
    amountInd6 = that.amountInd6;
    amount6 = that.amount6;
    amountInd7 = that.amountInd7;
    amount7 = that.amount7;
    amountInd8 = that.amountInd8;
    amount8 = that.amount8;
    amountInd9 = that.amountInd9;
    amount9 = that.amount9;
    amountInd10 = that.amountInd10;
    amount10 = that.amount10;
    amountInd11 = that.amountInd11;
    amount11 = that.amount11;
    amountInd12 = that.amountInd12;
    amount12 = that.amount12;
  }

  /// <summary>Length of the SSN attribute.</summary>
  public const int Ssn_MaxLength = 9;

  /// <summary>
  /// The value of the SSN attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = Ssn_MaxLength)]
  public string Ssn
  {
    get => ssn ?? "";
    set => ssn = TrimEnd(Substring(value, 1, Ssn_MaxLength));
  }

  /// <summary>
  /// The json value of the Ssn attribute.</summary>
  [JsonPropertyName("ssn")]
  [Computed]
  public string Ssn_Json
  {
    get => NullIf(Ssn, "");
    set => Ssn = value;
  }

  /// <summary>Length of the SUBMITTING_STATE_CODE attribute.</summary>
  public const int SubmittingStateCode_MaxLength = 2;

  /// <summary>
  /// The value of the SUBMITTING_STATE_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length
    = SubmittingStateCode_MaxLength)]
  public string SubmittingStateCode
  {
    get => submittingStateCode ?? "";
    set => submittingStateCode =
      TrimEnd(Substring(value, 1, SubmittingStateCode_MaxLength));
  }

  /// <summary>
  /// The json value of the SubmittingStateCode attribute.</summary>
  [JsonPropertyName("submittingStateCode")]
  [Computed]
  public string SubmittingStateCode_Json
  {
    get => NullIf(SubmittingStateCode, "");
    set => SubmittingStateCode = value;
  }

  /// <summary>
  /// The value of the LOCAL_FIPS_CODE attribute.
  /// </summary>
  [JsonPropertyName("localFipsCode")]
  [DefaultValue(0)]
  [Member(Index = 3, Type = MemberType.Number, Length = 3)]
  public int LocalFipsCode
  {
    get => localFipsCode;
    set => localFipsCode = value;
  }

  /// <summary>Length of the CSE_PERSON_NUMBER attribute.</summary>
  public const int CsePersonNumber_MaxLength = 10;

  /// <summary>
  /// The value of the CSE_PERSON_NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = CsePersonNumber_MaxLength)]
    
  public string CsePersonNumber
  {
    get => csePersonNumber ?? "";
    set => csePersonNumber =
      TrimEnd(Substring(value, 1, CsePersonNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the CsePersonNumber attribute.</summary>
  [JsonPropertyName("csePersonNumber")]
  [Computed]
  public string CsePersonNumber_Json
  {
    get => NullIf(CsePersonNumber, "");
    set => CsePersonNumber = value;
  }

  /// <summary>
  /// The value of the 1099_REQUEST_IDENTIFIER attribute.
  /// This is the identifier for the 1099 Request that was originally sent to 
  /// the IRS.
  /// </summary>
  [JsonPropertyName("attribute1099RequestIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 5, Type = MemberType.Number, Length = 5)]
  public int Attribute1099RequestIdentifier
  {
    get => attribute1099RequestIdentifier;
    set => attribute1099RequestIdentifier = value;
  }

  /// <summary>Length of the LAST_NAME attribute.</summary>
  public const int LastName_MaxLength = 20;

  /// <summary>
  /// The value of the LAST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = LastName_MaxLength)]
  public string LastName
  {
    get => lastName ?? "";
    set => lastName = TrimEnd(Substring(value, 1, LastName_MaxLength));
  }

  /// <summary>
  /// The json value of the LastName attribute.</summary>
  [JsonPropertyName("lastName")]
  [Computed]
  public string LastName_Json
  {
    get => NullIf(LastName, "");
    set => LastName = value;
  }

  /// <summary>Length of the FIRST_NAME attribute.</summary>
  public const int FirstName_MaxLength = 15;

  /// <summary>
  /// The value of the FIRST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = FirstName_MaxLength)]
  public string FirstName
  {
    get => firstName ?? "";
    set => firstName = TrimEnd(Substring(value, 1, FirstName_MaxLength));
  }

  /// <summary>
  /// The json value of the FirstName attribute.</summary>
  [JsonPropertyName("firstName")]
  [Computed]
  public string FirstName_Json
  {
    get => NullIf(FirstName, "");
    set => FirstName = value;
  }

  /// <summary>Length of the OCSE_MATCH_CODE attribute.</summary>
  public const int OcseMatchCode_MaxLength = 2;

  /// <summary>
  /// The value of the OCSE_MATCH_CODE attribute.
  /// This attribute represents the way this request was matched by Internal 
  /// revenue.		
  /// As of 1/14/1996 the matching codes are:-
  /// 00 = Matched IRS file Financial Info returned	
  /// 18 = SSN not on IRS File No Information Returned
  /// 19 = Name submitted by state does not match with
  ///      SSA name. No Information is returned.
  /// 20 = Information unavailable.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = OcseMatchCode_MaxLength)]
  public string OcseMatchCode
  {
    get => ocseMatchCode ?? "";
    set => ocseMatchCode =
      TrimEnd(Substring(value, 1, OcseMatchCode_MaxLength));
  }

  /// <summary>
  /// The json value of the OcseMatchCode attribute.</summary>
  [JsonPropertyName("ocseMatchCode")]
  [Computed]
  public string OcseMatchCode_Json
  {
    get => NullIf(OcseMatchCode, "");
    set => OcseMatchCode = value;
  }

  /// <summary>Length of the CASE_TYPE_ADC_NADC attribute.</summary>
  public const int CaseTypeAdcNadc_MaxLength = 1;

  /// <summary>
  /// The value of the CASE_TYPE_ADC_NADC attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = CaseTypeAdcNadc_MaxLength)]
    
  public string CaseTypeAdcNadc
  {
    get => caseTypeAdcNadc ?? "";
    set => caseTypeAdcNadc =
      TrimEnd(Substring(value, 1, CaseTypeAdcNadc_MaxLength));
  }

  /// <summary>
  /// The json value of the CaseTypeAdcNadc attribute.</summary>
  [JsonPropertyName("caseTypeAdcNadc")]
  [Computed]
  public string CaseTypeAdcNadc_Json
  {
    get => NullIf(CaseTypeAdcNadc, "");
    set => CaseTypeAdcNadc = value;
  }

  /// <summary>Length of the COURT_ADMIN_ORDER_INDICATOR attribute.</summary>
  public const int CourtAdminOrderIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the COURT_ADMIN_ORDER_INDICATOR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length
    = CourtAdminOrderIndicator_MaxLength)]
  public string CourtAdminOrderIndicator
  {
    get => courtAdminOrderIndicator ?? "";
    set => courtAdminOrderIndicator =
      TrimEnd(Substring(value, 1, CourtAdminOrderIndicator_MaxLength));
  }

  /// <summary>
  /// The json value of the CourtAdminOrderIndicator attribute.</summary>
  [JsonPropertyName("courtAdminOrderIndicator")]
  [Computed]
  public string CourtAdminOrderIndicator_Json
  {
    get => NullIf(CourtAdminOrderIndicator, "");
    set => CourtAdminOrderIndicator = value;
  }

  /// <summary>Length of the PAYEE_NAME_1 attribute.</summary>
  public const int PayeeName1_MaxLength = 40;

  /// <summary>
  /// The value of the PAYEE_NAME_1 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length = PayeeName1_MaxLength)]
  public string PayeeName1
  {
    get => payeeName1 ?? "";
    set => payeeName1 = TrimEnd(Substring(value, 1, PayeeName1_MaxLength));
  }

  /// <summary>
  /// The json value of the PayeeName1 attribute.</summary>
  [JsonPropertyName("payeeName1")]
  [Computed]
  public string PayeeName1_Json
  {
    get => NullIf(PayeeName1, "");
    set => PayeeName1 = value;
  }

  /// <summary>Length of the PAYEE_NAME_2 attribute.</summary>
  public const int PayeeName2_MaxLength = 40;

  /// <summary>
  /// The value of the PAYEE_NAME_2 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length = PayeeName2_MaxLength)]
  public string PayeeName2
  {
    get => payeeName2 ?? "";
    set => payeeName2 = TrimEnd(Substring(value, 1, PayeeName2_MaxLength));
  }

  /// <summary>
  /// The json value of the PayeeName2 attribute.</summary>
  [JsonPropertyName("payeeName2")]
  [Computed]
  public string PayeeName2_Json
  {
    get => NullIf(PayeeName2, "");
    set => PayeeName2 = value;
  }

  /// <summary>Length of the PAYEE_STREET attribute.</summary>
  public const int PayeeStreet_MaxLength = 40;

  /// <summary>
  /// The value of the PAYEE_STREET attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length = PayeeStreet_MaxLength)]
  public string PayeeStreet
  {
    get => payeeStreet ?? "";
    set => payeeStreet = TrimEnd(Substring(value, 1, PayeeStreet_MaxLength));
  }

  /// <summary>
  /// The json value of the PayeeStreet attribute.</summary>
  [JsonPropertyName("payeeStreet")]
  [Computed]
  public string PayeeStreet_Json
  {
    get => NullIf(PayeeStreet, "");
    set => PayeeStreet = value;
  }

  /// <summary>Length of the PAYEE_CITY attribute.</summary>
  public const int PayeeCity_MaxLength = 40;

  /// <summary>
  /// The value of the PAYEE_CITY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 14, Type = MemberType.Char, Length = PayeeCity_MaxLength)]
  public string PayeeCity
  {
    get => payeeCity ?? "";
    set => payeeCity = TrimEnd(Substring(value, 1, PayeeCity_MaxLength));
  }

  /// <summary>
  /// The json value of the PayeeCity attribute.</summary>
  [JsonPropertyName("payeeCity")]
  [Computed]
  public string PayeeCity_Json
  {
    get => NullIf(PayeeCity, "");
    set => PayeeCity = value;
  }

  /// <summary>Length of the PAYEE_STATE attribute.</summary>
  public const int PayeeState_MaxLength = 2;

  /// <summary>
  /// The value of the PAYEE_STATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 15, Type = MemberType.Char, Length = PayeeState_MaxLength)]
  public string PayeeState
  {
    get => payeeState ?? "";
    set => payeeState = TrimEnd(Substring(value, 1, PayeeState_MaxLength));
  }

  /// <summary>
  /// The json value of the PayeeState attribute.</summary>
  [JsonPropertyName("payeeState")]
  [Computed]
  public string PayeeState_Json
  {
    get => NullIf(PayeeState, "");
    set => PayeeState = value;
  }

  /// <summary>Length of the PAYEE_ZIP_CODE attribute.</summary>
  public const int PayeeZipCode_MaxLength = 9;

  /// <summary>
  /// The value of the PAYEE_ZIP_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 16, Type = MemberType.Char, Length = PayeeZipCode_MaxLength)]
  public string PayeeZipCode
  {
    get => payeeZipCode ?? "";
    set => payeeZipCode = TrimEnd(Substring(value, 1, PayeeZipCode_MaxLength));
  }

  /// <summary>
  /// The json value of the PayeeZipCode attribute.</summary>
  [JsonPropertyName("payeeZipCode")]
  [Computed]
  public string PayeeZipCode_Json
  {
    get => NullIf(PayeeZipCode, "");
    set => PayeeZipCode = value;
  }

  /// <summary>Length of the PAYOR_EIN attribute.</summary>
  public const int PayorEin_MaxLength = 9;

  /// <summary>
  /// The value of the PAYOR_EIN attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 17, Type = MemberType.Char, Length = PayorEin_MaxLength)]
  public string PayorEin
  {
    get => payorEin ?? "";
    set => payorEin = TrimEnd(Substring(value, 1, PayorEin_MaxLength));
  }

  /// <summary>
  /// The json value of the PayorEin attribute.</summary>
  [JsonPropertyName("payorEin")]
  [Computed]
  public string PayorEin_Json
  {
    get => NullIf(PayorEin, "");
    set => PayorEin = value;
  }

  /// <summary>Length of the PAYOR_NAME_1 attribute.</summary>
  public const int PayorName1_MaxLength = 40;

  /// <summary>
  /// The value of the PAYOR_NAME_1 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 18, Type = MemberType.Char, Length = PayorName1_MaxLength)]
  public string PayorName1
  {
    get => payorName1 ?? "";
    set => payorName1 = TrimEnd(Substring(value, 1, PayorName1_MaxLength));
  }

  /// <summary>
  /// The json value of the PayorName1 attribute.</summary>
  [JsonPropertyName("payorName1")]
  [Computed]
  public string PayorName1_Json
  {
    get => NullIf(PayorName1, "");
    set => PayorName1 = value;
  }

  /// <summary>Length of the PAYOR_NAME_2 attribute.</summary>
  public const int PayorName2_MaxLength = 40;

  /// <summary>
  /// The value of the PAYOR_NAME_2 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 19, Type = MemberType.Char, Length = PayorName2_MaxLength)]
  public string PayorName2
  {
    get => payorName2 ?? "";
    set => payorName2 = TrimEnd(Substring(value, 1, PayorName2_MaxLength));
  }

  /// <summary>
  /// The json value of the PayorName2 attribute.</summary>
  [JsonPropertyName("payorName2")]
  [Computed]
  public string PayorName2_Json
  {
    get => NullIf(PayorName2, "");
    set => PayorName2 = value;
  }

  /// <summary>Length of the PAYOR_STREET attribute.</summary>
  public const int PayorStreet_MaxLength = 40;

  /// <summary>
  /// The value of the PAYOR_STREET attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 20, Type = MemberType.Char, Length = PayorStreet_MaxLength)]
  public string PayorStreet
  {
    get => payorStreet ?? "";
    set => payorStreet = TrimEnd(Substring(value, 1, PayorStreet_MaxLength));
  }

  /// <summary>
  /// The json value of the PayorStreet attribute.</summary>
  [JsonPropertyName("payorStreet")]
  [Computed]
  public string PayorStreet_Json
  {
    get => NullIf(PayorStreet, "");
    set => PayorStreet = value;
  }

  /// <summary>Length of the PAYOR_CITY_STATE_ZIP attribute.</summary>
  public const int PayorCityStateZip_MaxLength = 40;

  /// <summary>
  /// The value of the PAYOR_CITY_STATE_ZIP attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 21, Type = MemberType.Char, Length
    = PayorCityStateZip_MaxLength)]
  public string PayorCityStateZip
  {
    get => payorCityStateZip ?? "";
    set => payorCityStateZip =
      TrimEnd(Substring(value, 1, PayorCityStateZip_MaxLength));
  }

  /// <summary>
  /// The json value of the PayorCityStateZip attribute.</summary>
  [JsonPropertyName("payorCityStateZip")]
  [Computed]
  public string PayorCityStateZip_Json
  {
    get => NullIf(PayorCityStateZip, "");
    set => PayorCityStateZip = value;
  }

  /// <summary>
  /// The value of the TAX_YEAR attribute.
  /// </summary>
  [JsonPropertyName("taxYear")]
  [DefaultValue(0)]
  [Member(Index = 22, Type = MemberType.Number, Length = 4)]
  public int TaxYear
  {
    get => taxYear;
    set => taxYear = value;
  }

  /// <summary>Length of the ACCOUNT_CODE attribute.</summary>
  public const int AccountCode_MaxLength = 20;

  /// <summary>
  /// The value of the ACCOUNT_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 23, Type = MemberType.Char, Length = AccountCode_MaxLength)]
  public string AccountCode
  {
    get => accountCode ?? "";
    set => accountCode = TrimEnd(Substring(value, 1, AccountCode_MaxLength));
  }

  /// <summary>
  /// The json value of the AccountCode attribute.</summary>
  [JsonPropertyName("accountCode")]
  [Computed]
  public string AccountCode_Json
  {
    get => NullIf(AccountCode, "");
    set => AccountCode = value;
  }

  /// <summary>Length of the DOCUMENT_CODE attribute.</summary>
  public const int DocumentCode_MaxLength = 2;

  /// <summary>
  /// The value of the DOCUMENT_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 24, Type = MemberType.Char, Length = DocumentCode_MaxLength)]
  public string DocumentCode
  {
    get => documentCode ?? "";
    set => documentCode = TrimEnd(Substring(value, 1, DocumentCode_MaxLength));
  }

  /// <summary>
  /// The json value of the DocumentCode attribute.</summary>
  [JsonPropertyName("documentCode")]
  [Computed]
  public string DocumentCode_Json
  {
    get => NullIf(DocumentCode, "");
    set => DocumentCode = value;
  }

  /// <summary>Length of the AMOUNT_IND_1 attribute.</summary>
  public const int AmountInd1_MaxLength = 2;

  /// <summary>
  /// The value of the AMOUNT_IND_1 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 25, Type = MemberType.Char, Length = AmountInd1_MaxLength)]
  public string AmountInd1
  {
    get => amountInd1 ?? "";
    set => amountInd1 = TrimEnd(Substring(value, 1, AmountInd1_MaxLength));
  }

  /// <summary>
  /// The json value of the AmountInd1 attribute.</summary>
  [JsonPropertyName("amountInd1")]
  [Computed]
  public string AmountInd1_Json
  {
    get => NullIf(AmountInd1, "");
    set => AmountInd1 = value;
  }

  /// <summary>
  /// The value of the AMOUNT_1 attribute.
  /// </summary>
  [JsonPropertyName("amount1")]
  [DefaultValue(0L)]
  [Member(Index = 26, Type = MemberType.Number, Length = 12)]
  public long Amount1
  {
    get => amount1;
    set => amount1 = value;
  }

  /// <summary>Length of the AMOUNT_IND_2 attribute.</summary>
  public const int AmountInd2_MaxLength = 2;

  /// <summary>
  /// The value of the AMOUNT_IND_2 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 27, Type = MemberType.Char, Length = AmountInd2_MaxLength)]
  public string AmountInd2
  {
    get => amountInd2 ?? "";
    set => amountInd2 = TrimEnd(Substring(value, 1, AmountInd2_MaxLength));
  }

  /// <summary>
  /// The json value of the AmountInd2 attribute.</summary>
  [JsonPropertyName("amountInd2")]
  [Computed]
  public string AmountInd2_Json
  {
    get => NullIf(AmountInd2, "");
    set => AmountInd2 = value;
  }

  /// <summary>
  /// The value of the AMOUNT_2 attribute.
  /// </summary>
  [JsonPropertyName("amount2")]
  [DefaultValue(0L)]
  [Member(Index = 28, Type = MemberType.Number, Length = 12)]
  public long Amount2
  {
    get => amount2;
    set => amount2 = value;
  }

  /// <summary>Length of the AMOUNT_IND_3 attribute.</summary>
  public const int AmountInd3_MaxLength = 2;

  /// <summary>
  /// The value of the AMOUNT_IND_3 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 29, Type = MemberType.Char, Length = AmountInd3_MaxLength)]
  public string AmountInd3
  {
    get => amountInd3 ?? "";
    set => amountInd3 = TrimEnd(Substring(value, 1, AmountInd3_MaxLength));
  }

  /// <summary>
  /// The json value of the AmountInd3 attribute.</summary>
  [JsonPropertyName("amountInd3")]
  [Computed]
  public string AmountInd3_Json
  {
    get => NullIf(AmountInd3, "");
    set => AmountInd3 = value;
  }

  /// <summary>
  /// The value of the AMOUNT_3 attribute.
  /// </summary>
  [JsonPropertyName("amount3")]
  [DefaultValue(0L)]
  [Member(Index = 30, Type = MemberType.Number, Length = 12)]
  public long Amount3
  {
    get => amount3;
    set => amount3 = value;
  }

  /// <summary>Length of the AMOUNT_IND_4 attribute.</summary>
  public const int AmountInd4_MaxLength = 2;

  /// <summary>
  /// The value of the AMOUNT_IND_4 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 31, Type = MemberType.Char, Length = AmountInd4_MaxLength)]
  public string AmountInd4
  {
    get => amountInd4 ?? "";
    set => amountInd4 = TrimEnd(Substring(value, 1, AmountInd4_MaxLength));
  }

  /// <summary>
  /// The json value of the AmountInd4 attribute.</summary>
  [JsonPropertyName("amountInd4")]
  [Computed]
  public string AmountInd4_Json
  {
    get => NullIf(AmountInd4, "");
    set => AmountInd4 = value;
  }

  /// <summary>
  /// The value of the AMOUNT_4 attribute.
  /// </summary>
  [JsonPropertyName("amount4")]
  [DefaultValue(0L)]
  [Member(Index = 32, Type = MemberType.Number, Length = 12)]
  public long Amount4
  {
    get => amount4;
    set => amount4 = value;
  }

  /// <summary>Length of the AMOUNT_IND_5 attribute.</summary>
  public const int AmountInd5_MaxLength = 2;

  /// <summary>
  /// The value of the AMOUNT_IND_5 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 33, Type = MemberType.Char, Length = AmountInd5_MaxLength)]
  public string AmountInd5
  {
    get => amountInd5 ?? "";
    set => amountInd5 = TrimEnd(Substring(value, 1, AmountInd5_MaxLength));
  }

  /// <summary>
  /// The json value of the AmountInd5 attribute.</summary>
  [JsonPropertyName("amountInd5")]
  [Computed]
  public string AmountInd5_Json
  {
    get => NullIf(AmountInd5, "");
    set => AmountInd5 = value;
  }

  /// <summary>
  /// The value of the AMOUNT_5 attribute.
  /// </summary>
  [JsonPropertyName("amount5")]
  [DefaultValue(0L)]
  [Member(Index = 34, Type = MemberType.Number, Length = 12)]
  public long Amount5
  {
    get => amount5;
    set => amount5 = value;
  }

  /// <summary>Length of the AMOUNT_IND_6 attribute.</summary>
  public const int AmountInd6_MaxLength = 2;

  /// <summary>
  /// The value of the AMOUNT_IND_6 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 35, Type = MemberType.Char, Length = AmountInd6_MaxLength)]
  public string AmountInd6
  {
    get => amountInd6 ?? "";
    set => amountInd6 = TrimEnd(Substring(value, 1, AmountInd6_MaxLength));
  }

  /// <summary>
  /// The json value of the AmountInd6 attribute.</summary>
  [JsonPropertyName("amountInd6")]
  [Computed]
  public string AmountInd6_Json
  {
    get => NullIf(AmountInd6, "");
    set => AmountInd6 = value;
  }

  /// <summary>
  /// The value of the AMOUNT_6 attribute.
  /// </summary>
  [JsonPropertyName("amount6")]
  [DefaultValue(0L)]
  [Member(Index = 36, Type = MemberType.Number, Length = 12)]
  public long Amount6
  {
    get => amount6;
    set => amount6 = value;
  }

  /// <summary>Length of the AMOUNT_IND_7 attribute.</summary>
  public const int AmountInd7_MaxLength = 2;

  /// <summary>
  /// The value of the AMOUNT_IND_7 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 37, Type = MemberType.Char, Length = AmountInd7_MaxLength)]
  public string AmountInd7
  {
    get => amountInd7 ?? "";
    set => amountInd7 = TrimEnd(Substring(value, 1, AmountInd7_MaxLength));
  }

  /// <summary>
  /// The json value of the AmountInd7 attribute.</summary>
  [JsonPropertyName("amountInd7")]
  [Computed]
  public string AmountInd7_Json
  {
    get => NullIf(AmountInd7, "");
    set => AmountInd7 = value;
  }

  /// <summary>
  /// The value of the AMOUNT_7 attribute.
  /// </summary>
  [JsonPropertyName("amount7")]
  [DefaultValue(0L)]
  [Member(Index = 38, Type = MemberType.Number, Length = 12)]
  public long Amount7
  {
    get => amount7;
    set => amount7 = value;
  }

  /// <summary>Length of the AMOUNT_IND_8 attribute.</summary>
  public const int AmountInd8_MaxLength = 2;

  /// <summary>
  /// The value of the AMOUNT_IND_8 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 39, Type = MemberType.Char, Length = AmountInd8_MaxLength)]
  public string AmountInd8
  {
    get => amountInd8 ?? "";
    set => amountInd8 = TrimEnd(Substring(value, 1, AmountInd8_MaxLength));
  }

  /// <summary>
  /// The json value of the AmountInd8 attribute.</summary>
  [JsonPropertyName("amountInd8")]
  [Computed]
  public string AmountInd8_Json
  {
    get => NullIf(AmountInd8, "");
    set => AmountInd8 = value;
  }

  /// <summary>
  /// The value of the AMOUNT_8 attribute.
  /// </summary>
  [JsonPropertyName("amount8")]
  [DefaultValue(0L)]
  [Member(Index = 40, Type = MemberType.Number, Length = 12)]
  public long Amount8
  {
    get => amount8;
    set => amount8 = value;
  }

  /// <summary>Length of the AMOUNT_IND_9 attribute.</summary>
  public const int AmountInd9_MaxLength = 2;

  /// <summary>
  /// The value of the AMOUNT_IND_9 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 41, Type = MemberType.Char, Length = AmountInd9_MaxLength)]
  public string AmountInd9
  {
    get => amountInd9 ?? "";
    set => amountInd9 = TrimEnd(Substring(value, 1, AmountInd9_MaxLength));
  }

  /// <summary>
  /// The json value of the AmountInd9 attribute.</summary>
  [JsonPropertyName("amountInd9")]
  [Computed]
  public string AmountInd9_Json
  {
    get => NullIf(AmountInd9, "");
    set => AmountInd9 = value;
  }

  /// <summary>
  /// The value of the AMOUNT_9 attribute.
  /// </summary>
  [JsonPropertyName("amount9")]
  [DefaultValue(0L)]
  [Member(Index = 42, Type = MemberType.Number, Length = 12)]
  public long Amount9
  {
    get => amount9;
    set => amount9 = value;
  }

  /// <summary>Length of the AMOUNT_IND_10 attribute.</summary>
  public const int AmountInd10_MaxLength = 2;

  /// <summary>
  /// The value of the AMOUNT_IND_10 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 43, Type = MemberType.Char, Length = AmountInd10_MaxLength)]
  public string AmountInd10
  {
    get => amountInd10 ?? "";
    set => amountInd10 = TrimEnd(Substring(value, 1, AmountInd10_MaxLength));
  }

  /// <summary>
  /// The json value of the AmountInd10 attribute.</summary>
  [JsonPropertyName("amountInd10")]
  [Computed]
  public string AmountInd10_Json
  {
    get => NullIf(AmountInd10, "");
    set => AmountInd10 = value;
  }

  /// <summary>
  /// The value of the AMOUNT_10 attribute.
  /// </summary>
  [JsonPropertyName("amount10")]
  [DefaultValue(0L)]
  [Member(Index = 44, Type = MemberType.Number, Length = 12)]
  public long Amount10
  {
    get => amount10;
    set => amount10 = value;
  }

  /// <summary>Length of the AMOUNT_IND_11 attribute.</summary>
  public const int AmountInd11_MaxLength = 2;

  /// <summary>
  /// The value of the AMOUNT_IND_11 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 45, Type = MemberType.Char, Length = AmountInd11_MaxLength)]
  public string AmountInd11
  {
    get => amountInd11 ?? "";
    set => amountInd11 = TrimEnd(Substring(value, 1, AmountInd11_MaxLength));
  }

  /// <summary>
  /// The json value of the AmountInd11 attribute.</summary>
  [JsonPropertyName("amountInd11")]
  [Computed]
  public string AmountInd11_Json
  {
    get => NullIf(AmountInd11, "");
    set => AmountInd11 = value;
  }

  /// <summary>
  /// The value of the AMOUNT_11 attribute.
  /// </summary>
  [JsonPropertyName("amount11")]
  [DefaultValue(0L)]
  [Member(Index = 46, Type = MemberType.Number, Length = 12)]
  public long Amount11
  {
    get => amount11;
    set => amount11 = value;
  }

  /// <summary>Length of the AMOUNT_IND_12 attribute.</summary>
  public const int AmountInd12_MaxLength = 2;

  /// <summary>
  /// The value of the AMOUNT_IND_12 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 47, Type = MemberType.Char, Length = AmountInd12_MaxLength)]
  public string AmountInd12
  {
    get => amountInd12 ?? "";
    set => amountInd12 = TrimEnd(Substring(value, 1, AmountInd12_MaxLength));
  }

  /// <summary>
  /// The json value of the AmountInd12 attribute.</summary>
  [JsonPropertyName("amountInd12")]
  [Computed]
  public string AmountInd12_Json
  {
    get => NullIf(AmountInd12, "");
    set => AmountInd12 = value;
  }

  /// <summary>
  /// The value of the AMOUNT_12 attribute.
  /// </summary>
  [JsonPropertyName("amount12")]
  [DefaultValue(0L)]
  [Member(Index = 48, Type = MemberType.Number, Length = 12)]
  public long Amount12
  {
    get => amount12;
    set => amount12 = value;
  }

  private string ssn;
  private string submittingStateCode;
  private int localFipsCode;
  private string csePersonNumber;
  private int attribute1099RequestIdentifier;
  private string lastName;
  private string firstName;
  private string ocseMatchCode;
  private string caseTypeAdcNadc;
  private string courtAdminOrderIndicator;
  private string payeeName1;
  private string payeeName2;
  private string payeeStreet;
  private string payeeCity;
  private string payeeState;
  private string payeeZipCode;
  private string payorEin;
  private string payorName1;
  private string payorName2;
  private string payorStreet;
  private string payorCityStateZip;
  private int taxYear;
  private string accountCode;
  private string documentCode;
  private string amountInd1;
  private long amount1;
  private string amountInd2;
  private long amount2;
  private string amountInd3;
  private long amount3;
  private string amountInd4;
  private long amount4;
  private string amountInd5;
  private long amount5;
  private string amountInd6;
  private long amount6;
  private string amountInd7;
  private long amount7;
  private string amountInd8;
  private long amount8;
  private string amountInd9;
  private long amount9;
  private string amountInd10;
  private long amount10;
  private string amountInd11;
  private long amount11;
  private string amountInd12;
  private long amount12;
}
