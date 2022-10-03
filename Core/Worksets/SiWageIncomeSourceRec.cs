// The source file: SI_WAGE_INCOME_SOURCE_REC, ID: 371790975, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class SiWageIncomeSourceRec: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public SiWageIncomeSourceRec()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public SiWageIncomeSourceRec(SiWageIncomeSourceRec that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new SiWageIncomeSourceRec Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(SiWageIncomeSourceRec that)
  {
    base.Assign(that);
    cseIndicator = that.cseIndicator;
    personNumber = that.personNumber;
    personSsn = that.personSsn;
    recordTypeIndicator = that.recordTypeIndicator;
    bwQtr = that.bwQtr;
    bwYr = that.bwYr;
    nhUiDate = that.nhUiDate;
    wageOrUiAmt = that.wageOrUiAmt;
    empId = that.empId;
    empName = that.empName;
    street1 = that.street1;
    city = that.city;
    state = that.state;
    zipCode = that.zipCode;
    zip4 = that.zip4;
    uiBeginningBalance = that.uiBeginningBalance;
    uiStartDate = that.uiStartDate;
    uiEndDate = that.uiEndDate;
  }

  /// <summary>Length of the CSE_INDICATOR attribute.</summary>
  public const int CseIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the CSE_INDICATOR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = CseIndicator_MaxLength)]
  public string CseIndicator
  {
    get => cseIndicator ?? "";
    set => cseIndicator = TrimEnd(Substring(value, 1, CseIndicator_MaxLength));
  }

  /// <summary>
  /// The json value of the CseIndicator attribute.</summary>
  [JsonPropertyName("cseIndicator")]
  [Computed]
  public string CseIndicator_Json
  {
    get => NullIf(CseIndicator, "");
    set => CseIndicator = value;
  }

  /// <summary>Length of the PERSON_NUMBER attribute.</summary>
  public const int PersonNumber_MaxLength = 10;

  /// <summary>
  /// The value of the PERSON_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = PersonNumber_MaxLength)]
  public string PersonNumber
  {
    get => personNumber ?? "";
    set => personNumber = TrimEnd(Substring(value, 1, PersonNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the PersonNumber attribute.</summary>
  [JsonPropertyName("personNumber")]
  [Computed]
  public string PersonNumber_Json
  {
    get => NullIf(PersonNumber, "");
    set => PersonNumber = value;
  }

  /// <summary>Length of the PERSON_SSN attribute.</summary>
  public const int PersonSsn_MaxLength = 9;

  /// <summary>
  /// The value of the PERSON_SSN attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = PersonSsn_MaxLength)]
  public string PersonSsn
  {
    get => personSsn ?? "";
    set => personSsn = TrimEnd(Substring(value, 1, PersonSsn_MaxLength));
  }

  /// <summary>
  /// The json value of the PersonSsn attribute.</summary>
  [JsonPropertyName("personSsn")]
  [Computed]
  public string PersonSsn_Json
  {
    get => NullIf(PersonSsn, "");
    set => PersonSsn = value;
  }

  /// <summary>Length of the RECORD_TYPE_INDICATOR attribute.</summary>
  public const int RecordTypeIndicator_MaxLength = 2;

  /// <summary>
  /// The value of the RECORD_TYPE_INDICATOR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length
    = RecordTypeIndicator_MaxLength)]
  public string RecordTypeIndicator
  {
    get => recordTypeIndicator ?? "";
    set => recordTypeIndicator =
      TrimEnd(Substring(value, 1, RecordTypeIndicator_MaxLength));
  }

  /// <summary>
  /// The json value of the RecordTypeIndicator attribute.</summary>
  [JsonPropertyName("recordTypeIndicator")]
  [Computed]
  public string RecordTypeIndicator_Json
  {
    get => NullIf(RecordTypeIndicator, "");
    set => RecordTypeIndicator = value;
  }

  /// <summary>
  /// The value of the BW_QTR attribute.
  /// </summary>
  [JsonPropertyName("bwQtr")]
  [DefaultValue(0)]
  [Member(Index = 5, Type = MemberType.Number, Length = 1)]
  public int BwQtr
  {
    get => bwQtr;
    set => bwQtr = value;
  }

  /// <summary>
  /// The value of the BW_YR attribute.
  /// </summary>
  [JsonPropertyName("bwYr")]
  [DefaultValue(0)]
  [Member(Index = 6, Type = MemberType.Number, Length = 4)]
  public int BwYr
  {
    get => bwYr;
    set => bwYr = value;
  }

  /// <summary>
  /// The value of the NH_UI_DATE attribute.
  /// </summary>
  [JsonPropertyName("nhUiDate")]
  [Member(Index = 7, Type = MemberType.Date)]
  public DateTime? NhUiDate
  {
    get => nhUiDate;
    set => nhUiDate = value;
  }

  /// <summary>
  /// The value of the WAGE_OR_UI_AMT attribute.
  /// </summary>
  [JsonPropertyName("wageOrUiAmt")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 8, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal WageOrUiAmt
  {
    get => wageOrUiAmt;
    set => wageOrUiAmt = Truncate(value, 2);
  }

  /// <summary>Length of the EMP_ID attribute.</summary>
  public const int EmpId_MaxLength = 10;

  /// <summary>
  /// The value of the EMP_ID attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = EmpId_MaxLength)]
  public string EmpId
  {
    get => empId ?? "";
    set => empId = TrimEnd(Substring(value, 1, EmpId_MaxLength));
  }

  /// <summary>
  /// The json value of the EmpId attribute.</summary>
  [JsonPropertyName("empId")]
  [Computed]
  public string EmpId_Json
  {
    get => NullIf(EmpId, "");
    set => EmpId = value;
  }

  /// <summary>Length of the EMP_NAME attribute.</summary>
  public const int EmpName_MaxLength = 33;

  /// <summary>
  /// The value of the EMP_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = EmpName_MaxLength)]
  public string EmpName
  {
    get => empName ?? "";
    set => empName = TrimEnd(Substring(value, 1, EmpName_MaxLength));
  }

  /// <summary>
  /// The json value of the EmpName attribute.</summary>
  [JsonPropertyName("empName")]
  [Computed]
  public string EmpName_Json
  {
    get => NullIf(EmpName, "");
    set => EmpName = value;
  }

  /// <summary>Length of the STREET1 attribute.</summary>
  public const int Street1_MaxLength = 25;

  /// <summary>
  /// The value of the STREET1 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length = Street1_MaxLength)]
  public string Street1
  {
    get => street1 ?? "";
    set => street1 = TrimEnd(Substring(value, 1, Street1_MaxLength));
  }

  /// <summary>
  /// The json value of the Street1 attribute.</summary>
  [JsonPropertyName("street1")]
  [Computed]
  public string Street1_Json
  {
    get => NullIf(Street1, "");
    set => Street1 = value;
  }

  /// <summary>Length of the CITY attribute.</summary>
  public const int City_MaxLength = 15;

  /// <summary>
  /// The value of the CITY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length = City_MaxLength)]
  public string City
  {
    get => city ?? "";
    set => city = TrimEnd(Substring(value, 1, City_MaxLength));
  }

  /// <summary>
  /// The json value of the City attribute.</summary>
  [JsonPropertyName("city")]
  [Computed]
  public string City_Json
  {
    get => NullIf(City, "");
    set => City = value;
  }

  /// <summary>Length of the STATE attribute.</summary>
  public const int State_MaxLength = 2;

  /// <summary>
  /// The value of the STATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length = State_MaxLength)]
  public string State
  {
    get => state ?? "";
    set => state = TrimEnd(Substring(value, 1, State_MaxLength));
  }

  /// <summary>
  /// The json value of the State attribute.</summary>
  [JsonPropertyName("state")]
  [Computed]
  public string State_Json
  {
    get => NullIf(State, "");
    set => State = value;
  }

  /// <summary>Length of the ZIP_CODE attribute.</summary>
  public const int ZipCode_MaxLength = 5;

  /// <summary>
  /// The value of the ZIP_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 14, Type = MemberType.Char, Length = ZipCode_MaxLength)]
  public string ZipCode
  {
    get => zipCode ?? "";
    set => zipCode = TrimEnd(Substring(value, 1, ZipCode_MaxLength));
  }

  /// <summary>
  /// The json value of the ZipCode attribute.</summary>
  [JsonPropertyName("zipCode")]
  [Computed]
  public string ZipCode_Json
  {
    get => NullIf(ZipCode, "");
    set => ZipCode = value;
  }

  /// <summary>Length of the ZIP4 attribute.</summary>
  public const int Zip4_MaxLength = 4;

  /// <summary>
  /// The value of the ZIP4 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 15, Type = MemberType.Char, Length = Zip4_MaxLength)]
  public string Zip4
  {
    get => zip4 ?? "";
    set => zip4 = TrimEnd(Substring(value, 1, Zip4_MaxLength));
  }

  /// <summary>
  /// The json value of the Zip4 attribute.</summary>
  [JsonPropertyName("zip4")]
  [Computed]
  public string Zip4_Json
  {
    get => NullIf(Zip4, "");
    set => Zip4 = value;
  }

  /// <summary>
  /// The value of the UI_BEGINNING_BALANCE attribute.
  /// </summary>
  [JsonPropertyName("uiBeginningBalance")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 16, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal UiBeginningBalance
  {
    get => uiBeginningBalance;
    set => uiBeginningBalance = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the UI_START_DATE attribute.
  /// </summary>
  [JsonPropertyName("uiStartDate")]
  [Member(Index = 17, Type = MemberType.Date)]
  public DateTime? UiStartDate
  {
    get => uiStartDate;
    set => uiStartDate = value;
  }

  /// <summary>
  /// The value of the UI_END_DATE attribute.
  /// </summary>
  [JsonPropertyName("uiEndDate")]
  [Member(Index = 18, Type = MemberType.Date)]
  public DateTime? UiEndDate
  {
    get => uiEndDate;
    set => uiEndDate = value;
  }

  private string cseIndicator;
  private string personNumber;
  private string personSsn;
  private string recordTypeIndicator;
  private int bwQtr;
  private int bwYr;
  private DateTime? nhUiDate;
  private decimal wageOrUiAmt;
  private string empId;
  private string empName;
  private string street1;
  private string city;
  private string state;
  private string zipCode;
  private string zip4;
  private decimal uiBeginningBalance;
  private DateTime? uiStartDate;
  private DateTime? uiEndDate;
}
