// The source file: REPORT_DATA, ID: 371126934, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP Finance:
/// 
/// Defines the type of report data record (H-Header,D-Detail).
/// </summary>
[Serializable]
public partial class ReportData: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ReportData()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ReportData(ReportData that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ReportData Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ReportData that)
  {
    base.Assign(that);
    type1 = that.type1;
    sequenceNumber = that.sequenceNumber;
    firstPageOnlyInd = that.firstPageOnlyInd;
    lineControl = that.lineControl;
    lineText = that.lineText;
    jobName = that.jobName;
    jruSystemGenId = that.jruSystemGenId;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int Type1_MaxLength = 1;

  /// <summary>
  /// The value of the TYPE attribute.
  /// Defines the type of report data record (H-Header,D-Detail).
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = Type1_MaxLength)]
  public string Type1
  {
    get => type1 ?? "";
    set => type1 = TrimEnd(Substring(value, 1, Type1_MaxLength));
  }

  /// <summary>
  /// The json value of the Type1 attribute.</summary>
  [JsonPropertyName("type1")]
  [Computed]
  public string Type1_Json
  {
    get => NullIf(Type1, "");
    set => Type1 = value;
  }

  /// <summary>
  /// The value of the SEQUENCE_NUMBER attribute.
  /// Defines the order in which the rows of a report are to be produced.
  /// </summary>
  [JsonPropertyName("sequenceNumber")]
  [DefaultValue(0)]
  [Member(Index = 2, Type = MemberType.Number, Length = 9)]
  public int SequenceNumber
  {
    get => sequenceNumber;
    set => sequenceNumber = value;
  }

  /// <summary>Length of the FIRST_PAGE_ONLY_IND attribute.</summary>
  public const int FirstPageOnlyInd_MaxLength = 1;

  /// <summary>
  /// The value of the FIRST_PAGE_ONLY_IND attribute.
  /// A Y/N indicator. Represents whether or not the lines should be printed on 
  /// the first page of the report only or on all pages.
  /// </summary>
  [JsonPropertyName("firstPageOnlyInd")]
  [Member(Index = 3, Type = MemberType.Char, Length
    = FirstPageOnlyInd_MaxLength, Optional = true)]
  public string FirstPageOnlyInd
  {
    get => firstPageOnlyInd;
    set => firstPageOnlyInd = value != null
      ? TrimEnd(Substring(value, 1, FirstPageOnlyInd_MaxLength)) : null;
  }

  /// <summary>Length of the LINE_CONTROL attribute.</summary>
  public const int LineControl_MaxLength = 2;

  /// <summary>
  /// The value of the LINE_CONTROL attribute.
  /// Represents the number of print lines to be 
  /// skipped before printing the current line. An
  /// example is as follows:                   Value
  /// P    Skip to the beginning of a new page
  /// 
  /// 1    Skip one line then print the current line
  /// of text                   5    Sip five
  /// lines, then print the current line of text
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = LineControl_MaxLength)]
  public string LineControl
  {
    get => lineControl ?? "";
    set => lineControl = TrimEnd(Substring(value, 1, LineControl_MaxLength));
  }

  /// <summary>
  /// The json value of the LineControl attribute.</summary>
  [JsonPropertyName("lineControl")]
  [Computed]
  public string LineControl_Json
  {
    get => NullIf(LineControl, "");
    set => LineControl = value;
  }

  /// <summary>Length of the LINE_TEXT attribute.</summary>
  public const int LineText_MaxLength = 132;

  /// <summary>
  /// The value of the LINE_TEXT attribute.
  /// Represents the actual report text.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = LineText_MaxLength)]
  public string LineText
  {
    get => lineText ?? "";
    set => lineText = TrimEnd(Substring(value, 1, LineText_MaxLength));
  }

  /// <summary>
  /// The json value of the LineText attribute.</summary>
  [JsonPropertyName("lineText")]
  [Computed]
  public string LineText_Json
  {
    get => NullIf(LineText, "");
    set => LineText = value;
  }

  /// <summary>Length of the NAME attribute.</summary>
  public const int JobName_MaxLength = 8;

  /// <summary>
  /// The value of the NAME attribute.
  /// The name of the job (from the job card statement in the JCL) that is to be
  /// run.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = JobName_MaxLength)]
  public string JobName
  {
    get => jobName ?? "";
    set => jobName = TrimEnd(Substring(value, 1, JobName_MaxLength));
  }

  /// <summary>
  /// The json value of the JobName attribute.</summary>
  [JsonPropertyName("jobName")]
  [Computed]
  public string JobName_Json
  {
    get => NullIf(JobName, "");
    set => JobName = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GEN_ID attribute.
  /// Defines an unique ID for the Job Run row.
  /// </summary>
  [JsonPropertyName("jruSystemGenId")]
  [DefaultValue(0)]
  [Member(Index = 7, Type = MemberType.Number, Length = 9)]
  public int JruSystemGenId
  {
    get => jruSystemGenId;
    set => jruSystemGenId = value;
  }

  private string type1;
  private int sequenceNumber;
  private string firstPageOnlyInd;
  private string lineControl;
  private string lineText;
  private string jobName;
  private int jruSystemGenId;
}
