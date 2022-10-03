// The source file: NARRATIVE_DETAIL, ID: 370944025, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP:  SRVPLAN
/// 
/// This entity contains narrative detail for an event.
/// </summary>
[Serializable]
public partial class NarrativeDetail: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public NarrativeDetail()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public NarrativeDetail(NarrativeDetail that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new NarrativeDetail Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(NarrativeDetail that)
  {
    base.Assign(that);
    infrastructureId = that.infrastructureId;
    createdTimestamp = that.createdTimestamp;
    createdBy = that.createdBy;
    caseNumber = that.caseNumber;
    narrativeText = that.narrativeText;
    lineNumber = that.lineNumber;
  }

  /// <summary>
  /// The value of the INFRASTRUCTURE_ID attribute.
  /// A unique, system generated number that distinguishes one occurrence of the
  /// Infrastructure entity type from another.
  /// </summary>
  [JsonPropertyName("infrastructureId")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 9)]
  public int InfrastructureId
  {
    get => infrastructureId;
    set => infrastructureId = value;
  }

  /// <summary>
  /// The value of the CREATED_TIMESTAMP attribute.
  /// Timestamp when the Narrative Detail was created.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 2, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The user Id or Program Id responsible for creating the Narrative Detail.
  /// </summary>
  [JsonPropertyName("createdBy")]
  [Member(Index = 3, Type = MemberType.Char, Length = CreatedBy_MaxLength, Optional
    = true)]
  public string CreatedBy
  {
    get => createdBy;
    set => createdBy = value != null
      ? TrimEnd(Substring(value, 1, CreatedBy_MaxLength)) : null;
  }

  /// <summary>Length of the CASE_NUMBER attribute.</summary>
  public const int CaseNumber_MaxLength = 10;

  /// <summary>
  /// The value of the CASE_NUMBER attribute.
  /// The case number related to Narrative.
  /// </summary>
  [JsonPropertyName("caseNumber")]
  [Member(Index = 4, Type = MemberType.Char, Length = CaseNumber_MaxLength, Optional
    = true)]
  public string CaseNumber
  {
    get => caseNumber;
    set => caseNumber = value != null
      ? TrimEnd(Substring(value, 1, CaseNumber_MaxLength)) : null;
  }

  /// <summary>Length of the NARRATIVE_TEXT attribute.</summary>
  public const int NarrativeText_MaxLength = 68;

  /// <summary>
  /// The value of the NARRATIVE_TEXT attribute.
  /// The textual content for the Narrative Detail.
  /// </summary>
  [JsonPropertyName("narrativeText")]
  [Member(Index = 5, Type = MemberType.Char, Length = NarrativeText_MaxLength, Optional
    = true)]
  public string NarrativeText
  {
    get => narrativeText;
    set => narrativeText = value != null
      ? TrimEnd(Substring(value, 1, NarrativeText_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LINE_NUMBER attribute.
  /// The line number within the Narrative Text block.
  /// </summary>
  [JsonPropertyName("lineNumber")]
  [DefaultValue(0)]
  [Member(Index = 6, Type = MemberType.Number, Length = 3)]
  public int LineNumber
  {
    get => lineNumber;
    set => lineNumber = value;
  }

  private int infrastructureId;
  private DateTime? createdTimestamp;
  private string createdBy;
  private string caseNumber;
  private string narrativeText;
  private int lineNumber;
}
