// The source file: WS_FIELD_VALUES, ID: 372953898, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class WsFieldValues: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public WsFieldValues()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public WsFieldValues(WsFieldValues that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new WsFieldValues Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(WsFieldValues that)
  {
    base.Assign(that);
    archiveDate = that.archiveDate;
    infId = that.infId;
    docName = that.docName;
    docEffectiveDate = that.docEffectiveDate;
    fldName = that.fldName;
    valu = that.valu;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTstamp = that.lastUpdatedTstamp;
  }

  /// <summary>
  /// The value of the ARCHIVE_DATE attribute.
  /// </summary>
  [JsonPropertyName("archiveDate")]
  [Member(Index = 1, Type = MemberType.Date)]
  public DateTime? ArchiveDate
  {
    get => archiveDate;
    set => archiveDate = value;
  }

  /// <summary>
  /// The value of the INF_ID attribute.
  /// </summary>
  [JsonPropertyName("infId")]
  [DefaultValue(0)]
  [Member(Index = 2, Type = MemberType.Number, Length = 9)]
  public int InfId
  {
    get => infId;
    set => infId = value;
  }

  /// <summary>Length of the DOC_NAME attribute.</summary>
  public const int DocName_MaxLength = 8;

  /// <summary>
  /// The value of the DOC_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = DocName_MaxLength)]
  public string DocName
  {
    get => docName ?? "";
    set => docName = TrimEnd(Substring(value, 1, DocName_MaxLength));
  }

  /// <summary>
  /// The json value of the DocName attribute.</summary>
  [JsonPropertyName("docName")]
  [Computed]
  public string DocName_Json
  {
    get => NullIf(DocName, "");
    set => DocName = value;
  }

  /// <summary>
  /// The value of the DOC_EFFECTIVE_DATE attribute.
  /// </summary>
  [JsonPropertyName("docEffectiveDate")]
  [Member(Index = 4, Type = MemberType.Date)]
  public DateTime? DocEffectiveDate
  {
    get => docEffectiveDate;
    set => docEffectiveDate = value;
  }

  /// <summary>Length of the FLD_NAME attribute.</summary>
  public const int FldName_MaxLength = 10;

  /// <summary>
  /// The value of the FLD_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = FldName_MaxLength)]
  public string FldName
  {
    get => fldName ?? "";
    set => fldName = TrimEnd(Substring(value, 1, FldName_MaxLength));
  }

  /// <summary>
  /// The json value of the FldName attribute.</summary>
  [JsonPropertyName("fldName")]
  [Computed]
  public string FldName_Json
  {
    get => NullIf(FldName, "");
    set => FldName = value;
  }

  /// <summary>Length of the VALU attribute.</summary>
  public const int Valu_MaxLength = 245;

  /// <summary>
  /// The value of the VALU attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = Valu_MaxLength)]
  public string Valu
  {
    get => valu ?? "";
    set => valu = TrimEnd(Substring(value, 1, Valu_MaxLength));
  }

  /// <summary>
  /// The json value of the Valu attribute.</summary>
  [JsonPropertyName("valu")]
  [Computed]
  public string Valu_Json
  {
    get => NullIf(Valu, "");
    set => Valu = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
  public string CreatedBy
  {
    get => createdBy ?? "";
    set => createdBy = TrimEnd(Substring(value, 1, CreatedBy_MaxLength));
  }

  /// <summary>
  /// The json value of the CreatedBy attribute.</summary>
  [JsonPropertyName("createdBy")]
  [Computed]
  public string CreatedBy_Json
  {
    get => NullIf(CreatedBy, "");
    set => CreatedBy = value;
  }

  /// <summary>
  /// The value of the CREATED_TIMESTAMP attribute.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 8, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy ?? "";
    set => lastUpdatedBy =
      TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength));
  }

  /// <summary>
  /// The json value of the LastUpdatedBy attribute.</summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Computed]
  public string LastUpdatedBy_Json
  {
    get => NullIf(LastUpdatedBy, "");
    set => LastUpdatedBy = value;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TSTAMP attribute.
  /// </summary>
  [JsonPropertyName("lastUpdatedTstamp")]
  [Member(Index = 10, Type = MemberType.Timestamp)]
  public DateTime? LastUpdatedTstamp
  {
    get => lastUpdatedTstamp;
    set => lastUpdatedTstamp = value;
  }

  private DateTime? archiveDate;
  private int infId;
  private string docName;
  private DateTime? docEffectiveDate;
  private string fldName;
  private string valu;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTstamp;
}
