// The source file: JOB, ID: 371436691, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// This entity contains that job name and description for each of the jobs that
/// we want to report the status of the runs for on an online screen for review
/// by CSE staff.
/// </summary>
[Serializable]
public partial class Job: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public Job()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public Job(Job that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new Job Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(Job that)
  {
    base.Assign(that);
    name = that.name;
    zdelSequence = that.zdelSequence;
    zdelRecord = that.zdelRecord;
    description = that.description;
    tranId = that.tranId;
  }

  /// <summary>Length of the NAME attribute.</summary>
  public const int Name_MaxLength = 8;

  /// <summary>
  /// The value of the NAME attribute.
  /// The name of the job (from the job card statement in the JCL) that is to be
  /// run.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = Name_MaxLength)]
  public string Name
  {
    get => name ?? "";
    set => name = TrimEnd(Substring(value, 1, Name_MaxLength));
  }

  /// <summary>
  /// The json value of the Name attribute.</summary>
  [JsonPropertyName("name")]
  [Computed]
  public string Name_Json
  {
    get => NullIf(Name, "");
    set => Name = value;
  }

  /// <summary>
  /// The value of the ZDEL_SEQUENCE attribute.
  /// Sequential number within the JOB to uniquely identify each record.
  /// </summary>
  [JsonPropertyName("zdelSequence")]
  [Member(Index = 2, Type = MemberType.Number, Length = 4, Optional = true)]
  public int? ZdelSequence
  {
    get => zdelSequence;
    set => zdelSequence = value;
  }

  /// <summary>Length of the ZDEL_RECORD attribute.</summary>
  public const int ZdelRecord_MaxLength = 80;

  /// <summary>
  /// The value of the ZDEL_RECORD attribute.
  /// One record of JCL.
  /// </summary>
  [JsonPropertyName("zdelRecord")]
  [Member(Index = 3, Type = MemberType.Char, Length = ZdelRecord_MaxLength, Optional
    = true)]
  public string ZdelRecord
  {
    get => zdelRecord;
    set => zdelRecord = value != null
      ? TrimEnd(Substring(value, 1, ZdelRecord_MaxLength)) : null;
  }

  /// <summary>Length of the DESCRIPTION attribute.</summary>
  public const int Description_MaxLength = 40;

  /// <summary>
  /// The value of the DESCRIPTION attribute.
  /// Describes the title of the job/report.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = Description_MaxLength)]
  public string Description
  {
    get => description ?? "";
    set => description = TrimEnd(Substring(value, 1, Description_MaxLength));
  }

  /// <summary>
  /// The json value of the Description attribute.</summary>
  [JsonPropertyName("description")]
  [Computed]
  public string Description_Json
  {
    get => NullIf(Description, "");
    set => Description = value;
  }

  /// <summary>Length of the TRAN_ID attribute.</summary>
  public const int TranId_MaxLength = 4;

  /// <summary>
  /// The value of the TRAN_ID attribute.
  /// Represents the CICS TRAN ID of the screen to which this job/report is 
  /// associated.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = TranId_MaxLength)]
  public string TranId
  {
    get => tranId ?? "";
    set => tranId = TrimEnd(Substring(value, 1, TranId_MaxLength));
  }

  /// <summary>
  /// The json value of the TranId attribute.</summary>
  [JsonPropertyName("tranId")]
  [Computed]
  public string TranId_Json
  {
    get => NullIf(TranId, "");
    set => TranId = value;
  }

  private string name;
  private int? zdelSequence;
  private string zdelRecord;
  private string description;
  private string tranId;
}
