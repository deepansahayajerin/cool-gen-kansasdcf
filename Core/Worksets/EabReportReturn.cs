// The source file: EAB_REPORT_RETURN, ID: 371790866, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class EabReportReturn: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public EabReportReturn()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public EabReportReturn(EabReportReturn that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new EabReportReturn Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(EabReportReturn that)
  {
    base.Assign(that);
    pageNumber = that.pageNumber;
    lineNumber = that.lineNumber;
    linesRemaining = that.linesRemaining;
  }

  /// <summary>
  /// The value of the PAGE_NUMBER attribute.
  /// </summary>
  [JsonPropertyName("pageNumber")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 9)]
  public int PageNumber
  {
    get => pageNumber;
    set => pageNumber = value;
  }

  /// <summary>
  /// The value of the LINE_NUMBER attribute.
  /// </summary>
  [JsonPropertyName("lineNumber")]
  [DefaultValue(0)]
  [Member(Index = 2, Type = MemberType.Number, Length = 2)]
  public int LineNumber
  {
    get => lineNumber;
    set => lineNumber = value;
  }

  /// <summary>
  /// The value of the LINES_REMAINING attribute.
  /// </summary>
  [JsonPropertyName("linesRemaining")]
  [DefaultValue(0)]
  [Member(Index = 3, Type = MemberType.Number, Length = 2)]
  public int LinesRemaining
  {
    get => linesRemaining;
    set => linesRemaining = value;
  }

  private int pageNumber;
  private int lineNumber;
  private int linesRemaining;
}
