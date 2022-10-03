// The source file: BATCH_ATTRIBUTES, ID: 372367162, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

/// <summary>
/// RESP: FNCLMNGT
/// This work set is for any batch fields of that are needed for many different 
/// procesess.
/// </summary>
[Serializable]
public partial class BatchAttributes: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public BatchAttributes()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public BatchAttributes(BatchAttributes that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new BatchAttributes Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(BatchAttributes that)
  {
    base.Assign(that);
    continuationInd = that.continuationInd;
    programName = that.programName;
  }

  /// <summary>Length of the CONTINUATION_IND attribute.</summary>
  public const int ContinuationInd_MaxLength = 1;

  /// <summary>
  /// The value of the CONTINUATION_IND attribute.
  /// The continuation indicator is used to tell a batch process that is flowing
  /// to itself that it is in a continuation cycle.  A &quot;Y&quot; means that
  /// it has called itself.  If it is the first time thru then it will contain
  /// spaces.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = ContinuationInd_MaxLength)]
    
  public string ContinuationInd
  {
    get => continuationInd ?? "";
    set => continuationInd =
      TrimEnd(Substring(value, 1, ContinuationInd_MaxLength));
  }

  /// <summary>
  /// The json value of the ContinuationInd attribute.</summary>
  [JsonPropertyName("continuationInd")]
  [Computed]
  public string ContinuationInd_Json
  {
    get => NullIf(ContinuationInd, "");
    set => ContinuationInd = value;
  }

  /// <summary>Length of the PROGRAM_NAME attribute.</summary>
  public const int ProgramName_MaxLength = 8;

  /// <summary>
  /// The value of the PROGRAM_NAME attribute.
  /// text field of 8 characters.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = ProgramName_MaxLength)]
  public string ProgramName
  {
    get => programName ?? "";
    set => programName = TrimEnd(Substring(value, 1, ProgramName_MaxLength));
  }

  /// <summary>
  /// The json value of the ProgramName attribute.</summary>
  [JsonPropertyName("programName")]
  [Computed]
  public string ProgramName_Json
  {
    get => NullIf(ProgramName, "");
    set => ProgramName = value;
  }

  private string continuationInd;
  private string programName;
}
