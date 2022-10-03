// The source file: JCL_TEMPLATE, ID: 371126889, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP FINANCE:
/// 
/// Defines the JCL structure for the JOB.
/// </summary>
[Serializable]
public partial class JclTemplate: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public JclTemplate()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public JclTemplate(JclTemplate that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new JclTemplate Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(JclTemplate that)
  {
    base.Assign(that);
    sequenceNumber = that.sequenceNumber;
    outputType = that.outputType;
    recordText = that.recordText;
    jobName = that.jobName;
  }

  /// <summary>
  /// The value of the SEQUENCE_NUMBER attribute.
  /// Defines the order of the JCL structure for the JOB.
  /// </summary>
  [JsonPropertyName("sequenceNumber")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 3)]
  public int SequenceNumber
  {
    get => sequenceNumber;
    set => sequenceNumber = value;
  }

  /// <summary>Length of the OUTPUT_TYPE attribute.</summary>
  public const int OutputType_MaxLength = 10;

  /// <summary>
  /// The value of the OUTPUT_TYPE attribute.
  /// Defines the type of output in which the report will be produced.
  /// 
  /// Example:
  /// 
  /// PRINTER   Format will be a hardcopy report to a sent to a printer defined
  /// by the Printer_id.                                              WORDPFCT
  /// Format will be wordperfect. Report will be emailed to the user based on
  /// the email_address.                     HTML     Format will be html.
  /// Report will be emailed to the user based on the email_address.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = OutputType_MaxLength)]
  public string OutputType
  {
    get => outputType ?? "";
    set => outputType = TrimEnd(Substring(value, 1, OutputType_MaxLength));
  }

  /// <summary>
  /// The json value of the OutputType attribute.</summary>
  [JsonPropertyName("outputType")]
  [Computed]
  public string OutputType_Json
  {
    get => NullIf(OutputType, "");
    set => OutputType = value;
  }

  /// <summary>Length of the RECORD_TEXT attribute.</summary>
  public const int RecordText_MaxLength = 80;

  /// <summary>
  /// The value of the RECORD_TEXT attribute.
  /// Defines a JCL line in a job stream for the JOB.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = RecordText_MaxLength)]
  public string RecordText
  {
    get => recordText ?? "";
    set => recordText = TrimEnd(Substring(value, 1, RecordText_MaxLength));
  }

  /// <summary>
  /// The json value of the RecordText attribute.</summary>
  [JsonPropertyName("recordText")]
  [Computed]
  public string RecordText_Json
  {
    get => NullIf(RecordText, "");
    set => RecordText = value;
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
  [Member(Index = 4, Type = MemberType.Char, Length = JobName_MaxLength)]
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

  private int sequenceNumber;
  private string outputType;
  private string recordText;
  private string jobName;
}
