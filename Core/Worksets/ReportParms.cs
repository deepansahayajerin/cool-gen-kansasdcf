// The source file: REPORT_PARMS, ID: 372576147, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class ReportParms: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ReportParms()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ReportParms(ReportParms that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ReportParms Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ReportParms that)
  {
    base.Assign(that);
    parm1 = that.parm1;
    parm2 = that.parm2;
    subreportCode = that.subreportCode;
  }

  /// <summary>Length of the PARM1 attribute.</summary>
  public const int Parm1_MaxLength = 2;

  /// <summary>
  /// The value of the PARM1 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = Parm1_MaxLength)]
  public string Parm1
  {
    get => parm1 ?? "";
    set => parm1 = TrimEnd(Substring(value, 1, Parm1_MaxLength));
  }

  /// <summary>
  /// The json value of the Parm1 attribute.</summary>
  [JsonPropertyName("parm1")]
  [Computed]
  public string Parm1_Json
  {
    get => NullIf(Parm1, "");
    set => Parm1 = value;
  }

  /// <summary>Length of the PARM2 attribute.</summary>
  public const int Parm2_MaxLength = 2;

  /// <summary>
  /// The value of the PARM2 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = Parm2_MaxLength)]
  public string Parm2
  {
    get => parm2 ?? "";
    set => parm2 = TrimEnd(Substring(value, 1, Parm2_MaxLength));
  }

  /// <summary>
  /// The json value of the Parm2 attribute.</summary>
  [JsonPropertyName("parm2")]
  [Computed]
  public string Parm2_Json
  {
    get => NullIf(Parm2, "");
    set => Parm2 = value;
  }

  /// <summary>Length of the SUBREPORT_CODE attribute.</summary>
  public const int SubreportCode_MaxLength = 4;

  /// <summary>
  /// The value of the SUBREPORT_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = SubreportCode_MaxLength)]
  public string SubreportCode
  {
    get => subreportCode ?? "";
    set => subreportCode =
      TrimEnd(Substring(value, 1, SubreportCode_MaxLength));
  }

  /// <summary>
  /// The json value of the SubreportCode attribute.</summary>
  [JsonPropertyName("subreportCode")]
  [Computed]
  public string SubreportCode_Json
  {
    get => NullIf(SubreportCode, "");
    set => SubreportCode = value;
  }

  private string parm1;
  private string parm2;
  private string subreportCode;
}
