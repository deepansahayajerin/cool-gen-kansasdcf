// The source file: EXTERNAL_FIDM_DETAIL, ID: 374402429, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

/// <summary>
/// This is detail record for FIDM
/// </summary>
[Serializable]
public partial class ExternalFidmDetail: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ExternalFidmDetail()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ExternalFidmDetail(ExternalFidmDetail that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ExternalFidmDetail Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ExternalFidmDetail that)
  {
    base.Assign(that);
    recordType = that.recordType;
    ssn = that.ssn;
    lastName = that.lastName;
    firstName = that.firstName;
    csePersonNo = that.csePersonNo;
    fips = that.fips;
  }

  /// <summary>Length of the RECORD_TYPE attribute.</summary>
  public const int RecordType_MaxLength = 1;

  /// <summary>
  /// The value of the RECORD_TYPE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = RecordType_MaxLength)]
  public string RecordType
  {
    get => recordType ?? "";
    set => recordType = TrimEnd(Substring(value, 1, RecordType_MaxLength));
  }

  /// <summary>
  /// The json value of the RecordType attribute.</summary>
  [JsonPropertyName("recordType")]
  [Computed]
  public string RecordType_Json
  {
    get => NullIf(RecordType, "");
    set => RecordType = value;
  }

  /// <summary>Length of the SSN attribute.</summary>
  public const int Ssn_MaxLength = 9;

  /// <summary>
  /// The value of the SSN attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = Ssn_MaxLength)]
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

  /// <summary>Length of the LAST_NAME attribute.</summary>
  public const int LastName_MaxLength = 20;

  /// <summary>
  /// The value of the LAST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = LastName_MaxLength)]
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
  public const int FirstName_MaxLength = 16;

  /// <summary>
  /// The value of the FIRST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = FirstName_MaxLength)]
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

  /// <summary>Length of the CSE_PERSON_NO attribute.</summary>
  public const int CsePersonNo_MaxLength = 15;

  /// <summary>
  /// The value of the CSE_PERSON_NO attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = CsePersonNo_MaxLength)]
  public string CsePersonNo
  {
    get => csePersonNo ?? "";
    set => csePersonNo = TrimEnd(Substring(value, 1, CsePersonNo_MaxLength));
  }

  /// <summary>
  /// The json value of the CsePersonNo attribute.</summary>
  [JsonPropertyName("csePersonNo")]
  [Computed]
  public string CsePersonNo_Json
  {
    get => NullIf(CsePersonNo, "");
    set => CsePersonNo = value;
  }

  /// <summary>Length of the FIPS attribute.</summary>
  public const int Fips_MaxLength = 5;

  /// <summary>
  /// The value of the FIPS attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = Fips_MaxLength)]
  public string Fips
  {
    get => fips ?? "";
    set => fips = TrimEnd(Substring(value, 1, Fips_MaxLength));
  }

  /// <summary>
  /// The json value of the Fips attribute.</summary>
  [JsonPropertyName("fips")]
  [Computed]
  public string Fips_Json
  {
    get => NullIf(Fips, "");
    set => Fips = value;
  }

  private string recordType;
  private string ssn;
  private string lastName;
  private string firstName;
  private string csePersonNo;
  private string fips;
}
