// The source file: KPC_EXTERNAL_PARMS, ID: 374396744, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class KpcExternalParms: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public KpcExternalParms()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public KpcExternalParms(KpcExternalParms that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new KpcExternalParms Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(KpcExternalParms that)
  {
    base.Assign(that);
    parm1 = that.parm1;
    parm2 = that.parm2;
    filename = that.filename;
  }

  /// <summary>Length of the PARM1 attribute.</summary>
  public const int Parm1_MaxLength = 2;

  /// <summary>
  /// The value of the PARM1 attribute.
  /// Used to send an action code to the generated stub. It can have one of nine
  /// values:
  /// OF - Open File
  /// AF - Append File
  /// GR - Get Record, generate record, or generate report.
  /// CF - Close File
  /// VR - View Report
  /// PR - Print Report
  /// FR - File Report (GUI Reports only)
  /// PS - Printer Setup dialog box display (GUI Reports Only)
  /// PU - Pack/Unpack
  /// Upon return from the USE of the EAB, PARM1 will contain a return code.  It
  /// can contain the following values:
  /// Spaces - No errors were encountered
  /// EO - Error on Open File
  /// EC - Error on Close File
  /// ER - Error on GR action
  /// II - Invalid PARM1 code
  /// RI - Invalid Runtime Option (PARM2 code)
  /// EF - End of File (File Reader only)
  /// EN - Error on numeric data field read by file reader
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
  /// Overrides report properties or printing options at runtime (keep detail 
  /// lines together, suppress detail lines, and suppress a starting page eject
  /// ).
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

  /// <summary>Length of the FILENAME attribute.</summary>
  public const int Filename_MaxLength = 255;

  /// <summary>
  /// The value of the FILENAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = Filename_MaxLength)]
  public string Filename
  {
    get => filename ?? "";
    set => filename = TrimEnd(Substring(value, 1, Filename_MaxLength));
  }

  /// <summary>
  /// The json value of the Filename attribute.</summary>
  [JsonPropertyName("filename")]
  [Computed]
  public string Filename_Json
  {
    get => NullIf(Filename, "");
    set => Filename = value;
  }

  private string parm1;
  private string parm2;
  private string filename;
}
