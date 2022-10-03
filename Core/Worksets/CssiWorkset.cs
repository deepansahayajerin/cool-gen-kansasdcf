// The source file: CSSI_WORKSET, ID: 1902453334, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class CssiWorkset: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public CssiWorkset()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public CssiWorkset(CssiWorkset that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new CssiWorkset Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(CssiWorkset that)
  {
    base.Assign(that);
    ncpAddressFileLayout = that.ncpAddressFileLayout;
    ncpChildFileLayout = that.ncpChildFileLayout;
    ncpFileLayout = that.ncpFileLayout;
  }

  /// <summary>Length of the NCP_ADDRESS_FILE_LAYOUT attribute.</summary>
  public const int NcpAddressFileLayout_MaxLength = 133;

  /// <summary>
  /// The value of the NCP_ADDRESS_FILE_LAYOUT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length
    = NcpAddressFileLayout_MaxLength)]
  public string NcpAddressFileLayout
  {
    get => ncpAddressFileLayout ?? "";
    set => ncpAddressFileLayout =
      TrimEnd(Substring(value, 1, NcpAddressFileLayout_MaxLength));
  }

  /// <summary>
  /// The json value of the NcpAddressFileLayout attribute.</summary>
  [JsonPropertyName("ncpAddressFileLayout")]
  [Computed]
  public string NcpAddressFileLayout_Json
  {
    get => NullIf(NcpAddressFileLayout, "");
    set => NcpAddressFileLayout = value;
  }

  /// <summary>Length of the NCP_CHILD_FILE_LAYOUT attribute.</summary>
  public const int NcpChildFileLayout_MaxLength = 102;

  /// <summary>
  /// The value of the NCP_CHILD_FILE_LAYOUT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length
    = NcpChildFileLayout_MaxLength)]
  public string NcpChildFileLayout
  {
    get => ncpChildFileLayout ?? "";
    set => ncpChildFileLayout =
      TrimEnd(Substring(value, 1, NcpChildFileLayout_MaxLength));
  }

  /// <summary>
  /// The json value of the NcpChildFileLayout attribute.</summary>
  [JsonPropertyName("ncpChildFileLayout")]
  [Computed]
  public string NcpChildFileLayout_Json
  {
    get => NullIf(NcpChildFileLayout, "");
    set => NcpChildFileLayout = value;
  }

  /// <summary>Length of the NCP_FILE_LAYOUT attribute.</summary>
  public const int NcpFileLayout_MaxLength = 126;

  /// <summary>
  /// The value of the NCP_FILE_LAYOUT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = NcpFileLayout_MaxLength)]
  public string NcpFileLayout
  {
    get => ncpFileLayout ?? "";
    set => ncpFileLayout =
      TrimEnd(Substring(value, 1, NcpFileLayout_MaxLength));
  }

  /// <summary>
  /// The json value of the NcpFileLayout attribute.</summary>
  [JsonPropertyName("ncpFileLayout")]
  [Computed]
  public string NcpFileLayout_Json
  {
    get => NullIf(NcpFileLayout, "");
    set => NcpFileLayout = value;
  }

  private string ncpAddressFileLayout;
  private string ncpChildFileLayout;
  private string ncpFileLayout;
}
