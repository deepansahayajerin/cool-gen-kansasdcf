// The source file: INTERSTAT_WARRANT, ID: 372550440, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class InterstatWarrant: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public InterstatWarrant()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public InterstatWarrant(InterstatWarrant that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new InterstatWarrant Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(InterstatWarrant that)
  {
    base.Assign(that);
    interstateInd = that.interstateInd;
    shortArName = that.shortArName;
    fips = that.fips;
    medicalInd = that.medicalInd;
    shortApName = that.shortApName;
    apSsn = that.apSsn;
    otherStateCaseNo = that.otherStateCaseNo;
    courtOrderNo = that.courtOrderNo;
  }

  /// <summary>Length of the INTERSTATE_IND attribute.</summary>
  public const int InterstateInd_MaxLength = 1;

  /// <summary>
  /// The value of the INTERSTATE_IND attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = InterstateInd_MaxLength)]
  public string InterstateInd
  {
    get => interstateInd ?? "";
    set => interstateInd =
      TrimEnd(Substring(value, 1, InterstateInd_MaxLength));
  }

  /// <summary>
  /// The json value of the InterstateInd attribute.</summary>
  [JsonPropertyName("interstateInd")]
  [Computed]
  public string InterstateInd_Json
  {
    get => NullIf(InterstateInd, "");
    set => InterstateInd = value;
  }

  /// <summary>Length of the SHORT_AR_NAME attribute.</summary>
  public const int ShortArName_MaxLength = 12;

  /// <summary>
  /// The value of the SHORT_AR_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = ShortArName_MaxLength)]
  public string ShortArName
  {
    get => shortArName ?? "";
    set => shortArName = TrimEnd(Substring(value, 1, ShortArName_MaxLength));
  }

  /// <summary>
  /// The json value of the ShortArName attribute.</summary>
  [JsonPropertyName("shortArName")]
  [Computed]
  public string ShortArName_Json
  {
    get => NullIf(ShortArName, "");
    set => ShortArName = value;
  }

  /// <summary>Length of the FIPS attribute.</summary>
  public const int Fips_MaxLength = 8;

  /// <summary>
  /// The value of the FIPS attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = Fips_MaxLength)]
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

  /// <summary>Length of the MEDICAL_IND attribute.</summary>
  public const int MedicalInd_MaxLength = 1;

  /// <summary>
  /// The value of the MEDICAL_IND attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = MedicalInd_MaxLength)]
  public string MedicalInd
  {
    get => medicalInd ?? "";
    set => medicalInd = TrimEnd(Substring(value, 1, MedicalInd_MaxLength));
  }

  /// <summary>
  /// The json value of the MedicalInd attribute.</summary>
  [JsonPropertyName("medicalInd")]
  [Computed]
  public string MedicalInd_Json
  {
    get => NullIf(MedicalInd, "");
    set => MedicalInd = value;
  }

  /// <summary>Length of the SHORT_AP_NAME attribute.</summary>
  public const int ShortApName_MaxLength = 12;

  /// <summary>
  /// The value of the SHORT_AP_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = ShortApName_MaxLength)]
  public string ShortApName
  {
    get => shortApName ?? "";
    set => shortApName = TrimEnd(Substring(value, 1, ShortApName_MaxLength));
  }

  /// <summary>
  /// The json value of the ShortApName attribute.</summary>
  [JsonPropertyName("shortApName")]
  [Computed]
  public string ShortApName_Json
  {
    get => NullIf(ShortApName, "");
    set => ShortApName = value;
  }

  /// <summary>Length of the AP_SSN attribute.</summary>
  public const int ApSsn_MaxLength = 11;

  /// <summary>
  /// The value of the AP_SSN attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = ApSsn_MaxLength)]
  public string ApSsn
  {
    get => apSsn ?? "";
    set => apSsn = TrimEnd(Substring(value, 1, ApSsn_MaxLength));
  }

  /// <summary>
  /// The json value of the ApSsn attribute.</summary>
  [JsonPropertyName("apSsn")]
  [Computed]
  public string ApSsn_Json
  {
    get => NullIf(ApSsn, "");
    set => ApSsn = value;
  }

  /// <summary>Length of the OTHER_STATE_CASE_NO attribute.</summary>
  public const int OtherStateCaseNo_MaxLength = 20;

  /// <summary>
  /// The value of the OTHER_STATE_CASE_NO attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = OtherStateCaseNo_MaxLength)
    ]
  public string OtherStateCaseNo
  {
    get => otherStateCaseNo ?? "";
    set => otherStateCaseNo =
      TrimEnd(Substring(value, 1, OtherStateCaseNo_MaxLength));
  }

  /// <summary>
  /// The json value of the OtherStateCaseNo attribute.</summary>
  [JsonPropertyName("otherStateCaseNo")]
  [Computed]
  public string OtherStateCaseNo_Json
  {
    get => NullIf(OtherStateCaseNo, "");
    set => OtherStateCaseNo = value;
  }

  /// <summary>Length of the COURT_ORDER_NO attribute.</summary>
  public const int CourtOrderNo_MaxLength = 12;

  /// <summary>
  /// The value of the COURT_ORDER_NO attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = CourtOrderNo_MaxLength)]
  public string CourtOrderNo
  {
    get => courtOrderNo ?? "";
    set => courtOrderNo = TrimEnd(Substring(value, 1, CourtOrderNo_MaxLength));
  }

  /// <summary>
  /// The json value of the CourtOrderNo attribute.</summary>
  [JsonPropertyName("courtOrderNo")]
  [Computed]
  public string CourtOrderNo_Json
  {
    get => NullIf(CourtOrderNo, "");
    set => CourtOrderNo = value;
  }

  private string interstateInd;
  private string shortArName;
  private string fips;
  private string medicalInd;
  private string shortApName;
  private string apSsn;
  private string otherStateCaseNo;
  private string courtOrderNo;
}
