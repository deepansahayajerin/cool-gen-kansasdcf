// The source file: REPORT_LITERALS, ID: 372848496, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class ReportLiterals: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ReportLiterals()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ReportLiterals(ReportLiterals that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ReportLiterals Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ReportLiterals that)
  {
    base.Assign(that);
    subHeading1 = that.subHeading1;
    subHeading2 = that.subHeading2;
    subHeading3 = that.subHeading3;
  }

  /// <summary>Length of the SUB_HEADING_1 attribute.</summary>
  public const int SubHeading1_MaxLength = 30;

  /// <summary>
  /// The value of the SUB_HEADING_1 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = SubHeading1_MaxLength)]
  public string SubHeading1
  {
    get => subHeading1 ?? "";
    set => subHeading1 = TrimEnd(Substring(value, 1, SubHeading1_MaxLength));
  }

  /// <summary>
  /// The json value of the SubHeading1 attribute.</summary>
  [JsonPropertyName("subHeading1")]
  [Computed]
  public string SubHeading1_Json
  {
    get => NullIf(SubHeading1, "");
    set => SubHeading1 = value;
  }

  /// <summary>Length of the SUB_HEADING_2 attribute.</summary>
  public const int SubHeading2_MaxLength = 14;

  /// <summary>
  /// The value of the SUB_HEADING_2 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = SubHeading2_MaxLength)]
  public string SubHeading2
  {
    get => subHeading2 ?? "";
    set => subHeading2 = TrimEnd(Substring(value, 1, SubHeading2_MaxLength));
  }

  /// <summary>
  /// The json value of the SubHeading2 attribute.</summary>
  [JsonPropertyName("subHeading2")]
  [Computed]
  public string SubHeading2_Json
  {
    get => NullIf(SubHeading2, "");
    set => SubHeading2 = value;
  }

  /// <summary>Length of the SUB_HEADING_3 attribute.</summary>
  public const int SubHeading3_MaxLength = 5;

  /// <summary>
  /// The value of the SUB_HEADING_3 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = SubHeading3_MaxLength)]
  public string SubHeading3
  {
    get => subHeading3 ?? "";
    set => subHeading3 = TrimEnd(Substring(value, 1, SubHeading3_MaxLength));
  }

  /// <summary>
  /// The json value of the SubHeading3 attribute.</summary>
  [JsonPropertyName("subHeading3")]
  [Computed]
  public string SubHeading3_Json
  {
    get => NullIf(SubHeading3, "");
    set => SubHeading3 = value;
  }

  private string subHeading1;
  private string subHeading2;
  private string subHeading3;
}
