// The source file: SDSO_CERTIFICATION_TAPE_RECORD, ID: 372665100, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

/// <summary>
/// RESP: LGLENFAC
/// This workset defines the fields for SDSO certifications sent.
/// </summary>
[Serializable]
public partial class SdsoCertificationTapeRecord: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public SdsoCertificationTapeRecord()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public SdsoCertificationTapeRecord(SdsoCertificationTapeRecord that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new SdsoCertificationTapeRecord Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(SdsoCertificationTapeRecord that)
  {
    base.Assign(that);
    certificationRecord = that.certificationRecord;
    zdelAgency = that.zdelAgency;
    zdelSubAgency = that.zdelSubAgency;
    zdelTransCode = that.zdelTransCode;
    caNumber = that.caNumber;
    debtorIdCode = that.debtorIdCode;
    ssn = that.ssn;
    firstName = that.firstName;
    middleInitial = that.middleInitial;
    lastName = that.lastName;
    accountIdCode = that.accountIdCode;
    accountIdNumber = that.accountIdNumber;
    debtCodeType = that.debtCodeType;
    debtDesc = that.debtDesc;
    debtAmount = that.debtAmount;
    filler12 = that.filler12;
    driversLicenseNumber = that.driversLicenseNumber;
  }

  /// <summary>Length of the CERTIFICATION_RECORD attribute.</summary>
  public const int CertificationRecord_MaxLength = 182;

  /// <summary>
  /// The value of the CERTIFICATION_RECORD attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length
    = CertificationRecord_MaxLength)]
  public string CertificationRecord
  {
    get => certificationRecord ?? "";
    set => certificationRecord =
      TrimEnd(Substring(value, 1, CertificationRecord_MaxLength));
  }

  /// <summary>
  /// The json value of the CertificationRecord attribute.</summary>
  [JsonPropertyName("certificationRecord")]
  [Computed]
  public string CertificationRecord_Json
  {
    get => NullIf(CertificationRecord, "");
    set => CertificationRecord = value;
  }

  /// <summary>Length of the ZDEL_AGENCY attribute.</summary>
  public const int ZdelAgency_MaxLength = 3;

  /// <summary>
  /// The value of the ZDEL_AGENCY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = ZdelAgency_MaxLength)]
  public string ZdelAgency
  {
    get => zdelAgency ?? "";
    set => zdelAgency = TrimEnd(Substring(value, 1, ZdelAgency_MaxLength));
  }

  /// <summary>
  /// The json value of the ZdelAgency attribute.</summary>
  [JsonPropertyName("zdelAgency")]
  [Computed]
  public string ZdelAgency_Json
  {
    get => NullIf(ZdelAgency, "");
    set => ZdelAgency = value;
  }

  /// <summary>Length of the ZDEL_SUB_AGENCY attribute.</summary>
  public const int ZdelSubAgency_MaxLength = 2;

  /// <summary>
  /// The value of the ZDEL_SUB_AGENCY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = ZdelSubAgency_MaxLength)]
  public string ZdelSubAgency
  {
    get => zdelSubAgency ?? "";
    set => zdelSubAgency =
      TrimEnd(Substring(value, 1, ZdelSubAgency_MaxLength));
  }

  /// <summary>
  /// The json value of the ZdelSubAgency attribute.</summary>
  [JsonPropertyName("zdelSubAgency")]
  [Computed]
  public string ZdelSubAgency_Json
  {
    get => NullIf(ZdelSubAgency, "");
    set => ZdelSubAgency = value;
  }

  /// <summary>Length of the ZDEL_TRANS_CODE attribute.</summary>
  public const int ZdelTransCode_MaxLength = 1;

  /// <summary>
  /// The value of the ZDEL_TRANS_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = ZdelTransCode_MaxLength)]
  public string ZdelTransCode
  {
    get => zdelTransCode ?? "";
    set => zdelTransCode =
      TrimEnd(Substring(value, 1, ZdelTransCode_MaxLength));
  }

  /// <summary>
  /// The json value of the ZdelTransCode attribute.</summary>
  [JsonPropertyName("zdelTransCode")]
  [Computed]
  public string ZdelTransCode_Json
  {
    get => NullIf(ZdelTransCode, "");
    set => ZdelTransCode = value;
  }

  /// <summary>Length of the CA_NUMBER attribute.</summary>
  public const int CaNumber_MaxLength = 11;

  /// <summary>
  /// The value of the CA_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = CaNumber_MaxLength)]
  public string CaNumber
  {
    get => caNumber ?? "";
    set => caNumber = TrimEnd(Substring(value, 1, CaNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the CaNumber attribute.</summary>
  [JsonPropertyName("caNumber")]
  [Computed]
  public string CaNumber_Json
  {
    get => NullIf(CaNumber, "");
    set => CaNumber = value;
  }

  /// <summary>Length of the DEBTOR_ID_CODE attribute.</summary>
  public const int DebtorIdCode_MaxLength = 1;

  /// <summary>
  /// The value of the DEBTOR_ID_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = DebtorIdCode_MaxLength)]
  public string DebtorIdCode
  {
    get => debtorIdCode ?? "";
    set => debtorIdCode = TrimEnd(Substring(value, 1, DebtorIdCode_MaxLength));
  }

  /// <summary>
  /// The json value of the DebtorIdCode attribute.</summary>
  [JsonPropertyName("debtorIdCode")]
  [Computed]
  public string DebtorIdCode_Json
  {
    get => NullIf(DebtorIdCode, "");
    set => DebtorIdCode = value;
  }

  /// <summary>Length of the SSN attribute.</summary>
  public const int Ssn_MaxLength = 9;

  /// <summary>
  /// The value of the SSN attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = Ssn_MaxLength)]
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

  /// <summary>Length of the FIRST_NAME attribute.</summary>
  public const int FirstName_MaxLength = 35;

  /// <summary>
  /// The value of the FIRST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = FirstName_MaxLength)]
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

  /// <summary>Length of the MIDDLE_INITIAL attribute.</summary>
  public const int MiddleInitial_MaxLength = 1;

  /// <summary>
  /// The value of the MIDDLE_INITIAL attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = MiddleInitial_MaxLength)]
  public string MiddleInitial
  {
    get => middleInitial ?? "";
    set => middleInitial =
      TrimEnd(Substring(value, 1, MiddleInitial_MaxLength));
  }

  /// <summary>
  /// The json value of the MiddleInitial attribute.</summary>
  [JsonPropertyName("middleInitial")]
  [Computed]
  public string MiddleInitial_Json
  {
    get => NullIf(MiddleInitial, "");
    set => MiddleInitial = value;
  }

  /// <summary>Length of the LAST_NAME attribute.</summary>
  public const int LastName_MaxLength = 35;

  /// <summary>
  /// The value of the LAST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = LastName_MaxLength)]
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

  /// <summary>Length of the ACCOUNT_ID_CODE attribute.</summary>
  public const int AccountIdCode_MaxLength = 1;

  /// <summary>
  /// The value of the ACCOUNT_ID_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length = AccountIdCode_MaxLength)]
  public string AccountIdCode
  {
    get => accountIdCode ?? "";
    set => accountIdCode =
      TrimEnd(Substring(value, 1, AccountIdCode_MaxLength));
  }

  /// <summary>
  /// The json value of the AccountIdCode attribute.</summary>
  [JsonPropertyName("accountIdCode")]
  [Computed]
  public string AccountIdCode_Json
  {
    get => NullIf(AccountIdCode, "");
    set => AccountIdCode = value;
  }

  /// <summary>Length of the ACCOUNT_ID_NUMBER attribute.</summary>
  public const int AccountIdNumber_MaxLength = 18;

  /// <summary>
  /// The value of the ACCOUNT_ID_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length = AccountIdNumber_MaxLength)
    ]
  public string AccountIdNumber
  {
    get => accountIdNumber ?? "";
    set => accountIdNumber =
      TrimEnd(Substring(value, 1, AccountIdNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the AccountIdNumber attribute.</summary>
  [JsonPropertyName("accountIdNumber")]
  [Computed]
  public string AccountIdNumber_Json
  {
    get => NullIf(AccountIdNumber, "");
    set => AccountIdNumber = value;
  }

  /// <summary>Length of the DEBT_CODE_TYPE attribute.</summary>
  public const int DebtCodeType_MaxLength = 1;

  /// <summary>
  /// The value of the DEBT_CODE_TYPE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length = DebtCodeType_MaxLength)]
  public string DebtCodeType
  {
    get => debtCodeType ?? "";
    set => debtCodeType = TrimEnd(Substring(value, 1, DebtCodeType_MaxLength));
  }

  /// <summary>
  /// The json value of the DebtCodeType attribute.</summary>
  [JsonPropertyName("debtCodeType")]
  [Computed]
  public string DebtCodeType_Json
  {
    get => NullIf(DebtCodeType, "");
    set => DebtCodeType = value;
  }

  /// <summary>Length of the DEBT_DESC attribute.</summary>
  public const int DebtDesc_MaxLength = 30;

  /// <summary>
  /// The value of the DEBT_DESC attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 14, Type = MemberType.Char, Length = DebtDesc_MaxLength)]
  public string DebtDesc
  {
    get => debtDesc ?? "";
    set => debtDesc = TrimEnd(Substring(value, 1, DebtDesc_MaxLength));
  }

  /// <summary>
  /// The json value of the DebtDesc attribute.</summary>
  [JsonPropertyName("debtDesc")]
  [Computed]
  public string DebtDesc_Json
  {
    get => NullIf(DebtDesc, "");
    set => DebtDesc = value;
  }

  /// <summary>Length of the DEBT_AMOUNT attribute.</summary>
  public const int DebtAmount_MaxLength = 13;

  /// <summary>
  /// The value of the DEBT_AMOUNT attribute.
  /// 9(6)v9(2)
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 15, Type = MemberType.Char, Length = DebtAmount_MaxLength)]
  public string DebtAmount
  {
    get => debtAmount ?? "";
    set => debtAmount = TrimEnd(Substring(value, 1, DebtAmount_MaxLength));
  }

  /// <summary>
  /// The json value of the DebtAmount attribute.</summary>
  [JsonPropertyName("debtAmount")]
  [Computed]
  public string DebtAmount_Json
  {
    get => NullIf(DebtAmount, "");
    set => DebtAmount = value;
  }

  /// <summary>Length of the FILLER12 attribute.</summary>
  public const int Filler12_MaxLength = 12;

  /// <summary>
  /// The value of the FILLER12 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 16, Type = MemberType.Char, Length = Filler12_MaxLength)]
  public string Filler12
  {
    get => filler12 ?? "";
    set => filler12 = TrimEnd(Substring(value, 1, Filler12_MaxLength));
  }

  /// <summary>
  /// The json value of the Filler12 attribute.</summary>
  [JsonPropertyName("filler12")]
  [Computed]
  public string Filler12_Json
  {
    get => NullIf(Filler12, "");
    set => Filler12 = value;
  }

  /// <summary>Length of the DRIVERS_LICENSE_NUMBER attribute.</summary>
  public const int DriversLicenseNumber_MaxLength = 15;

  /// <summary>
  /// The value of the DRIVERS_LICENSE_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 17, Type = MemberType.Char, Length
    = DriversLicenseNumber_MaxLength)]
  public string DriversLicenseNumber
  {
    get => driversLicenseNumber ?? "";
    set => driversLicenseNumber =
      TrimEnd(Substring(value, 1, DriversLicenseNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the DriversLicenseNumber attribute.</summary>
  [JsonPropertyName("driversLicenseNumber")]
  [Computed]
  public string DriversLicenseNumber_Json
  {
    get => NullIf(DriversLicenseNumber, "");
    set => DriversLicenseNumber = value;
  }

  private string certificationRecord;
  private string zdelAgency;
  private string zdelSubAgency;
  private string zdelTransCode;
  private string caNumber;
  private string debtorIdCode;
  private string ssn;
  private string firstName;
  private string middleInitial;
  private string lastName;
  private string accountIdCode;
  private string accountIdNumber;
  private string debtCodeType;
  private string debtDesc;
  private string debtAmount;
  private string filler12;
  private string driversLicenseNumber;
}
