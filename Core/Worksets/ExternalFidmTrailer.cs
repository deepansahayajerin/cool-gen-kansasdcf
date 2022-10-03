// The source file: EXTERNAL_FIDM_TRAILER, ID: 374402440, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

/// <summary>
/// This is trailer record for FIDM
/// </summary>
[Serializable]
public partial class ExternalFidmTrailer: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ExternalFidmTrailer()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ExternalFidmTrailer(ExternalFidmTrailer that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ExternalFidmTrailer Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ExternalFidmTrailer that)
  {
    base.Assign(that);
    recordType = that.recordType;
    totalNoInquiryRec = that.totalNoInquiryRec;
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

  /// <summary>
  /// The value of the TOTAL_NO_INQUIRY_REC attribute.
  /// </summary>
  [JsonPropertyName("totalNoInquiryRec")]
  [DefaultValue(0L)]
  [Member(Index = 2, Type = MemberType.Number, Length = 10)]
  public long TotalNoInquiryRec
  {
    get => totalNoInquiryRec;
    set => totalNoInquiryRec = value;
  }

  private string recordType;
  private long totalNoInquiryRec;
}
