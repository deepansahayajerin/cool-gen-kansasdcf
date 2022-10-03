// The source file: FPLS_WORK_AREA, ID: 372955853, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class FplsWorkArea: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public FplsWorkArea()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public FplsWorkArea(FplsWorkArea that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new FplsWorkArea Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(FplsWorkArea that)
  {
    base.Assign(that);
    text39 = that.text39;
    text18 = that.text18;
    addrZip = that.addrZip;
    addrSt = that.addrSt;
    addrCity = that.addrCity;
  }

  /// <summary>Length of the TEXT_39 attribute.</summary>
  public const int Text39_MaxLength = 39;

  /// <summary>
  /// The value of the TEXT_39 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = Text39_MaxLength)]
  public string Text39
  {
    get => text39 ?? "";
    set => text39 = TrimEnd(Substring(value, 1, Text39_MaxLength));
  }

  /// <summary>
  /// The json value of the Text39 attribute.</summary>
  [JsonPropertyName("text39")]
  [Computed]
  public string Text39_Json
  {
    get => NullIf(Text39, "");
    set => Text39 = value;
  }

  /// <summary>Length of the TEXT_18 attribute.</summary>
  public const int Text18_MaxLength = 18;

  /// <summary>
  /// The value of the TEXT_18 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = Text18_MaxLength)]
  public string Text18
  {
    get => text18 ?? "";
    set => text18 = TrimEnd(Substring(value, 1, Text18_MaxLength));
  }

  /// <summary>
  /// The json value of the Text18 attribute.</summary>
  [JsonPropertyName("text18")]
  [Computed]
  public string Text18_Json
  {
    get => NullIf(Text18, "");
    set => Text18 = value;
  }

  /// <summary>Length of the ADDR_ZIP attribute.</summary>
  public const int AddrZip_MaxLength = 15;

  /// <summary>
  /// The value of the ADDR_ZIP attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = AddrZip_MaxLength)]
  public string AddrZip
  {
    get => addrZip ?? "";
    set => addrZip = TrimEnd(Substring(value, 1, AddrZip_MaxLength));
  }

  /// <summary>
  /// The json value of the AddrZip attribute.</summary>
  [JsonPropertyName("addrZip")]
  [Computed]
  public string AddrZip_Json
  {
    get => NullIf(AddrZip, "");
    set => AddrZip = value;
  }

  /// <summary>Length of the ADDR_ST attribute.</summary>
  public const int AddrSt_MaxLength = 2;

  /// <summary>
  /// The value of the ADDR_ST attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = AddrSt_MaxLength)]
  public string AddrSt
  {
    get => addrSt ?? "";
    set => addrSt = TrimEnd(Substring(value, 1, AddrSt_MaxLength));
  }

  /// <summary>
  /// The json value of the AddrSt attribute.</summary>
  [JsonPropertyName("addrSt")]
  [Computed]
  public string AddrSt_Json
  {
    get => NullIf(AddrSt, "");
    set => AddrSt = value;
  }

  /// <summary>Length of the ADDR_CITY attribute.</summary>
  public const int AddrCity_MaxLength = 30;

  /// <summary>
  /// The value of the ADDR_CITY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = AddrCity_MaxLength)]
  public string AddrCity
  {
    get => addrCity ?? "";
    set => addrCity = TrimEnd(Substring(value, 1, AddrCity_MaxLength));
  }

  /// <summary>
  /// The json value of the AddrCity attribute.</summary>
  [JsonPropertyName("addrCity")]
  [Computed]
  public string AddrCity_Json
  {
    get => NullIf(AddrCity, "");
    set => AddrCity = value;
  }

  private string text39;
  private string text18;
  private string addrZip;
  private string addrSt;
  private string addrCity;
}
