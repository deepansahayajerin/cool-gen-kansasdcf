// The source file: THIRD_PARTY_RECORD, ID: 374396791, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class ThirdPartyRecord: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ThirdPartyRecord()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ThirdPartyRecord(ThirdPartyRecord that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ThirdPartyRecord Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ThirdPartyRecord that)
  {
    base.Assign(that);
    recordType = that.recordType;
    role = that.role;
    type1 = that.type1;
    srsPersonNumber = that.srsPersonNumber;
    agencyName = that.agencyName;
    pin = that.pin;
    source = that.source;
    filler = that.filler;
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

  /// <summary>Length of the ROLE attribute.</summary>
  public const int Role_MaxLength = 4;

  /// <summary>
  /// The value of the ROLE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = Role_MaxLength)]
  public string Role
  {
    get => role ?? "";
    set => role = TrimEnd(Substring(value, 1, Role_MaxLength));
  }

  /// <summary>
  /// The json value of the Role attribute.</summary>
  [JsonPropertyName("role")]
  [Computed]
  public string Role_Json
  {
    get => NullIf(Role, "");
    set => Role = value;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int Type1_MaxLength = 4;

  /// <summary>
  /// The value of the TYPE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = Type1_MaxLength)]
  public string Type1
  {
    get => type1 ?? "";
    set => type1 = TrimEnd(Substring(value, 1, Type1_MaxLength));
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

  /// <summary>Length of the SRS_PERSON_NUMBER attribute.</summary>
  public const int SrsPersonNumber_MaxLength = 15;

  /// <summary>
  /// The value of the SRS_PERSON_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = SrsPersonNumber_MaxLength)]
    
  public string SrsPersonNumber
  {
    get => srsPersonNumber ?? "";
    set => srsPersonNumber =
      TrimEnd(Substring(value, 1, SrsPersonNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the SrsPersonNumber attribute.</summary>
  [JsonPropertyName("srsPersonNumber")]
  [Computed]
  public string SrsPersonNumber_Json
  {
    get => NullIf(SrsPersonNumber, "");
    set => SrsPersonNumber = value;
  }

  /// <summary>Length of the AGENCY_NAME attribute.</summary>
  public const int AgencyName_MaxLength = 50;

  /// <summary>
  /// The value of the AGENCY_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = AgencyName_MaxLength)]
  public string AgencyName
  {
    get => agencyName ?? "";
    set => agencyName = TrimEnd(Substring(value, 1, AgencyName_MaxLength));
  }

  /// <summary>
  /// The json value of the AgencyName attribute.</summary>
  [JsonPropertyName("agencyName")]
  [Computed]
  public string AgencyName_Json
  {
    get => NullIf(AgencyName, "");
    set => AgencyName = value;
  }

  /// <summary>Length of the PIN attribute.</summary>
  public const int Pin_MaxLength = 8;

  /// <summary>
  /// The value of the PIN attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = Pin_MaxLength)]
  public string Pin
  {
    get => pin ?? "";
    set => pin = TrimEnd(Substring(value, 1, Pin_MaxLength));
  }

  /// <summary>
  /// The json value of the Pin attribute.</summary>
  [JsonPropertyName("pin")]
  [Computed]
  public string Pin_Json
  {
    get => NullIf(Pin, "");
    set => Pin = value;
  }

  /// <summary>Length of the SOURCE attribute.</summary>
  public const int Source_MaxLength = 4;

  /// <summary>
  /// The value of the SOURCE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = Source_MaxLength)]
  public string Source
  {
    get => source ?? "";
    set => source = TrimEnd(Substring(value, 1, Source_MaxLength));
  }

  /// <summary>
  /// The json value of the Source attribute.</summary>
  [JsonPropertyName("source")]
  [Computed]
  public string Source_Json
  {
    get => NullIf(Source, "");
    set => Source = value;
  }

  /// <summary>Length of the FILLER attribute.</summary>
  public const int Filler_MaxLength = 113;

  /// <summary>
  /// The value of the FILLER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = Filler_MaxLength)]
  public string Filler
  {
    get => filler ?? "";
    set => filler = TrimEnd(Substring(value, 1, Filler_MaxLength));
  }

  /// <summary>
  /// The json value of the Filler attribute.</summary>
  [JsonPropertyName("filler")]
  [Computed]
  public string Filler_Json
  {
    get => NullIf(Filler, "");
    set => Filler = value;
  }

  private string recordType;
  private string role;
  private string type1;
  private string srsPersonNumber;
  private string agencyName;
  private string pin;
  private string source;
  private string filler;
}
