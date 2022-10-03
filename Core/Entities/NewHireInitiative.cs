// The source file: NEW_HIRE_INITIATIVE, ID: 1902519435, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// Employer EINs that are being tracked for New Reporting compliance.
/// </summary>
[Serializable]
public partial class NewHireInitiative: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public NewHireInitiative()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public NewHireInitiative(NewHireInitiative that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new NewHireInitiative Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(NewHireInitiative that)
  {
    base.Assign(that);
    groupId = that.groupId;
    fein = that.fein;
    interventionType = that.interventionType;
  }

  /// <summary>
  /// The value of the GROUP_ID attribute.
  /// Number used to identify the initiative grouping.
  /// </summary>
  [JsonPropertyName("groupId")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 9)]
  public int GroupId
  {
    get => groupId;
    set => groupId = value;
  }

  /// <summary>Length of the FEIN attribute.</summary>
  public const int Fein_MaxLength = 9;

  /// <summary>
  /// The value of the FEIN attribute.
  /// The Federal Employer Identification Number being tracked.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = Fein_MaxLength)]
  public string Fein
  {
    get => fein ?? "";
    set => fein = TrimEnd(Substring(value, 1, Fein_MaxLength));
  }

  /// <summary>
  /// The json value of the Fein attribute.</summary>
  [JsonPropertyName("fein")]
  [Computed]
  public string Fein_Json
  {
    get => NullIf(Fein, "");
    set => Fein = value;
  }

  /// <summary>Length of the INTERVENTION_TYPE attribute.</summary>
  public const int InterventionType_MaxLength = 15;

  /// <summary>
  /// The value of the INTERVENTION_TYPE attribute.
  /// The type of intervention that was provided to the employer.  (Letter, 
  /// Postcard, Pamphlet, Phone Call, Control Group)
  /// </summary>
  [JsonPropertyName("interventionType")]
  [Member(Index = 3, Type = MemberType.Char, Length
    = InterventionType_MaxLength, Optional = true)]
  public string InterventionType
  {
    get => interventionType;
    set => interventionType = value != null
      ? TrimEnd(Substring(value, 1, InterventionType_MaxLength)) : null;
  }

  private int groupId;
  private string fein;
  private string interventionType;
}
