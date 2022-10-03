// The source file: QUICK_IN_QUERY, ID: 374543751, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class QuickInQuery: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public QuickInQuery()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public QuickInQuery(QuickInQuery that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new QuickInQuery Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(QuickInQuery that)
  {
    base.Assign(that);
    endDate = that.endDate;
    stDate = that.stDate;
    osFips = that.osFips;
    caseId = that.caseId;
  }

  /// <summary>Length of the END_DATE attribute.</summary>
  public const int EndDate_MaxLength = 8;

  /// <summary>
  /// The value of the END_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = EndDate_MaxLength)]
  public string EndDate
  {
    get => endDate ?? "";
    set => endDate = TrimEnd(Substring(value, 1, EndDate_MaxLength));
  }

  /// <summary>
  /// The json value of the EndDate attribute.</summary>
  [JsonPropertyName("endDate")]
  [Computed]
  public string EndDate_Json
  {
    get => NullIf(EndDate, "");
    set => EndDate = value;
  }

  /// <summary>Length of the ST_DATE attribute.</summary>
  public const int StDate_MaxLength = 8;

  /// <summary>
  /// The value of the ST_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = StDate_MaxLength)]
  public string StDate
  {
    get => stDate ?? "";
    set => stDate = TrimEnd(Substring(value, 1, StDate_MaxLength));
  }

  /// <summary>
  /// The json value of the StDate attribute.</summary>
  [JsonPropertyName("stDate")]
  [Computed]
  public string StDate_Json
  {
    get => NullIf(StDate, "");
    set => StDate = value;
  }

  /// <summary>
  /// The value of the OS_FIPS attribute.
  /// </summary>
  [JsonPropertyName("osFips")]
  [DefaultValue(0)]
  [Member(Index = 3, Type = MemberType.Number, Length = 2)]
  public int OsFips
  {
    get => osFips;
    set => osFips = value;
  }

  /// <summary>Length of the CASE_ID attribute.</summary>
  public const int CaseId_MaxLength = 15;

  /// <summary>
  /// The value of the CASE_ID attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = CaseId_MaxLength)]
  public string CaseId
  {
    get => caseId ?? "";
    set => caseId = TrimEnd(Substring(value, 1, CaseId_MaxLength));
  }

  /// <summary>
  /// The json value of the CaseId attribute.</summary>
  [JsonPropertyName("caseId")]
  [Computed]
  public string CaseId_Json
  {
    get => NullIf(CaseId, "");
    set => CaseId = value;
  }

  private string endDate;
  private string stDate;
  private int osFips;
  private string caseId;
}
