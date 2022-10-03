// The source file: EXTERNAL_FIDM_HEADER, ID: 374402436, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

/// <summary>
/// This is header record for FIDM
/// </summary>
[Serializable]
public partial class ExternalFidmHeader: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ExternalFidmHeader()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ExternalFidmHeader(ExternalFidmHeader that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ExternalFidmHeader Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ExternalFidmHeader that)
  {
    base.Assign(that);
    recordType = that.recordType;
    yyyymm = that.yyyymm;
    dataMatchFi = that.dataMatchFi;
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

  /// <summary>Length of the YYYYMM attribute.</summary>
  public const int Yyyymm_MaxLength = 6;

  /// <summary>
  /// The value of the YYYYMM attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = Yyyymm_MaxLength)]
  public string Yyyymm
  {
    get => yyyymm ?? "";
    set => yyyymm = TrimEnd(Substring(value, 1, Yyyymm_MaxLength));
  }

  /// <summary>
  /// The json value of the Yyyymm attribute.</summary>
  [JsonPropertyName("yyyymm")]
  [Computed]
  public string Yyyymm_Json
  {
    get => NullIf(Yyyymm, "");
    set => Yyyymm = value;
  }

  /// <summary>Length of the DATA_MATCH_FI attribute.</summary>
  public const int DataMatchFi_MaxLength = 1;

  /// <summary>
  /// The value of the DATA_MATCH_FI attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = DataMatchFi_MaxLength)]
  public string DataMatchFi
  {
    get => dataMatchFi ?? "";
    set => dataMatchFi = TrimEnd(Substring(value, 1, DataMatchFi_MaxLength));
  }

  /// <summary>
  /// The json value of the DataMatchFi attribute.</summary>
  [JsonPropertyName("dataMatchFi")]
  [Computed]
  public string DataMatchFi_Json
  {
    get => NullIf(DataMatchFi, "");
    set => DataMatchFi = value;
  }

  private string recordType;
  private string yyyymm;
  private string dataMatchFi;
}
