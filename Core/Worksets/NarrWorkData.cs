// The source file: NARR_WORK_DATA, ID: 371751772, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

/// <summary>
/// RESP: SRVPLAN	
///   WORK AREA FOR KAECSES-CSE NARRATIVE RECS
/// </summary>
[Serializable]
public partial class NarrWorkData: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public NarrWorkData()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public NarrWorkData(NarrWorkData that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new NarrWorkData Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(NarrWorkData that)
  {
    base.Assign(that);
    cseCaseNumber = that.cseCaseNumber;
    narrativeDate = that.narrativeDate;
    narrativeType = that.narrativeType;
    narrativeDesc = that.narrativeDesc;
    region = that.region;
    team = that.team;
  }

  /// <summary>Length of the CSE_CASE_NUMBER attribute.</summary>
  public const int CseCaseNumber_MaxLength = 12;

  /// <summary>
  /// The value of the CSE_CASE_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = CseCaseNumber_MaxLength)]
  public string CseCaseNumber
  {
    get => cseCaseNumber ?? "";
    set => cseCaseNumber =
      TrimEnd(Substring(value, 1, CseCaseNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the CseCaseNumber attribute.</summary>
  [JsonPropertyName("cseCaseNumber")]
  [Computed]
  public string CseCaseNumber_Json
  {
    get => NullIf(CseCaseNumber, "");
    set => CseCaseNumber = value;
  }

  /// <summary>
  /// The value of the NARRATIVE_DATE attribute.
  /// </summary>
  [JsonPropertyName("narrativeDate")]
  [Member(Index = 2, Type = MemberType.Date)]
  public DateTime? NarrativeDate
  {
    get => narrativeDate;
    set => narrativeDate = value;
  }

  /// <summary>Length of the NARRATIVE_TYPE attribute.</summary>
  public const int NarrativeType_MaxLength = 1;

  /// <summary>
  /// The value of the NARRATIVE_TYPE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = NarrativeType_MaxLength)]
  public string NarrativeType
  {
    get => narrativeType ?? "";
    set => narrativeType =
      TrimEnd(Substring(value, 1, NarrativeType_MaxLength));
  }

  /// <summary>
  /// The json value of the NarrativeType attribute.</summary>
  [JsonPropertyName("narrativeType")]
  [Computed]
  public string NarrativeType_Json
  {
    get => NullIf(NarrativeType, "");
    set => NarrativeType = value;
  }

  /// <summary>Length of the NARRATIVE_DESC attribute.</summary>
  public const int NarrativeDesc_MaxLength = 62;

  /// <summary>
  /// The value of the NARRATIVE_DESC attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = NarrativeDesc_MaxLength)]
  public string NarrativeDesc
  {
    get => narrativeDesc ?? "";
    set => narrativeDesc =
      TrimEnd(Substring(value, 1, NarrativeDesc_MaxLength));
  }

  /// <summary>
  /// The json value of the NarrativeDesc attribute.</summary>
  [JsonPropertyName("narrativeDesc")]
  [Computed]
  public string NarrativeDesc_Json
  {
    get => NullIf(NarrativeDesc, "");
    set => NarrativeDesc = value;
  }

  /// <summary>Length of the REGION attribute.</summary>
  public const int Region_MaxLength = 2;

  /// <summary>
  /// The value of the REGION attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = Region_MaxLength)]
  public string Region
  {
    get => region ?? "";
    set => region = TrimEnd(Substring(value, 1, Region_MaxLength));
  }

  /// <summary>
  /// The json value of the Region attribute.</summary>
  [JsonPropertyName("region")]
  [Computed]
  public string Region_Json
  {
    get => NullIf(Region, "");
    set => Region = value;
  }

  /// <summary>Length of the TEAM attribute.</summary>
  public const int Team_MaxLength = 1;

  /// <summary>
  /// The value of the TEAM attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = Team_MaxLength)]
  public string Team
  {
    get => team ?? "";
    set => team = TrimEnd(Substring(value, 1, Team_MaxLength));
  }

  /// <summary>
  /// The json value of the Team attribute.</summary>
  [JsonPropertyName("team")]
  [Computed]
  public string Team_Json
  {
    get => NullIf(Team, "");
    set => Team = value;
  }

  private string cseCaseNumber;
  private DateTime? narrativeDate;
  private string narrativeType;
  private string narrativeDesc;
  private string region;
  private string team;
}
