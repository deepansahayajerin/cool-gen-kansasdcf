// The source file: ABEND_CICS_TEST, ID: 371826090, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

/// <summary>
/// RESP: OBLGESTB
/// </summary>
[Serializable]
public partial class AbendCicsTest: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public AbendCicsTest()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public AbendCicsTest(AbendCicsTest that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new AbendCicsTest Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(AbendCicsTest that)
  {
    base.Assign(that);
    eibrsrce = that.eibrsrce;
    eibfn = that.eibfn;
    eibrcode = that.eibrcode;
  }

  /// <summary>Length of the EIBRSRCE attribute.</summary>
  public const int Eibrsrce_MaxLength = 8;

  /// <summary>
  /// The value of the EIBRSRCE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = Eibrsrce_MaxLength)]
  public string Eibrsrce
  {
    get => eibrsrce ?? "";
    set => eibrsrce = TrimEnd(Substring(value, 1, Eibrsrce_MaxLength));
  }

  /// <summary>
  /// The json value of the Eibrsrce attribute.</summary>
  [JsonPropertyName("eibrsrce")]
  [Computed]
  public string Eibrsrce_Json
  {
    get => NullIf(Eibrsrce, "");
    set => Eibrsrce = value;
  }

  /// <summary>Length of the EIBFN attribute.</summary>
  public const int Eibfn_MaxLength = 2;

  /// <summary>
  /// The value of the EIBFN attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = Eibfn_MaxLength)]
  public string Eibfn
  {
    get => eibfn ?? "";
    set => eibfn = TrimEnd(Substring(value, 1, Eibfn_MaxLength));
  }

  /// <summary>
  /// The json value of the Eibfn attribute.</summary>
  [JsonPropertyName("eibfn")]
  [Computed]
  public string Eibfn_Json
  {
    get => NullIf(Eibfn, "");
    set => Eibfn = value;
  }

  /// <summary>Length of the EIBRCODE attribute.</summary>
  public const int Eibrcode_MaxLength = 6;

  /// <summary>
  /// The value of the EIBRCODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = Eibrcode_MaxLength)]
  public string Eibrcode
  {
    get => eibrcode ?? "";
    set => eibrcode = TrimEnd(Substring(value, 1, Eibrcode_MaxLength));
  }

  /// <summary>
  /// The json value of the Eibrcode attribute.</summary>
  [JsonPropertyName("eibrcode")]
  [Computed]
  public string Eibrcode_Json
  {
    get => NullIf(Eibrcode, "");
    set => Eibrcode = value;
  }

  private string eibrsrce;
  private string eibfn;
  private string eibrcode;
}
