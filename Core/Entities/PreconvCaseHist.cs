// The source file: PRECONV_CASE_HIST, ID: 371439584, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVINIT
/// The ADABAS file, CSE_CASE_COMP_DBF is the primary source data file for this 
/// entity, and PRECONV_CASE_HIST ooccurrences represention AP's and CH's are
/// obtained from this file. Each occurrence represents the KAECSES
/// preconversion case structure for a given AP or CH on a given KAECSES case
/// prior to transition to the Kessep application. Begin and end dates for the
/// AP and CH correspond to the client start and end dates on the
/// CSE_CASE_COMP_DBF data file. For the CH these are the same dates which show
/// on the KAECSES CSLC screen. In addition to the occurrences converted
/// directly from this source data file, occurrences represention the
/// participation of the AR on the KAECSES case need to be created. These
/// occurrences will be derived from the CSE_CASE_DBF ADABAS file. Begin and end
/// dates for the AR on a DAECSES case correspond to the IV_D program open and
/// close dates on the KAECSES CSE_CASE_DBF data record for the case in
/// question. The initial population of this entity is the total population of
/// this entity, since in DB2 this will be a static table once Kessep is in
/// production.
/// </summary>
[Serializable]
public partial class PreconvCaseHist: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public PreconvCaseHist()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public PreconvCaseHist(PreconvCaseHist that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new PreconvCaseHist Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(PreconvCaseHist that)
  {
    base.Assign(that);
    csePersonNumber = that.csePersonNumber;
    kaecsesCaseNumber = that.kaecsesCaseNumber;
    kaecsesRelationship = that.kaecsesRelationship;
    kaecsesStartDate = that.kaecsesStartDate;
    kaecsesEndDate = that.kaecsesEndDate;
    createdTimestamp = that.createdTimestamp;
  }

  /// <summary>Length of the CSE_PERSON_NUMBER attribute.</summary>
  public const int CsePersonNumber_MaxLength = 10;

  /// <summary>
  /// The value of the CSE_PERSON_NUMBER attribute.
  /// A unique system generated number used by both Kessep and KAECSES to 
  /// identify persons and organizations with a relationship to CSE that must be
  /// represented within CSE automated systems. This attribute is half of the
  /// composite identifier for the parent entity.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = CsePersonNumber_MaxLength)]
    
  public string CsePersonNumber
  {
    get => csePersonNumber ?? "";
    set => csePersonNumber =
      TrimEnd(Substring(value, 1, CsePersonNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the CsePersonNumber attribute.</summary>
  [JsonPropertyName("csePersonNumber")]
  [Computed]
  public string CsePersonNumber_Json
  {
    get => NullIf(CsePersonNumber, "");
    set => CsePersonNumber = value;
  }

  /// <summary>Length of the KAECSES_CASE_NUMBER attribute.</summary>
  public const int KaecsesCaseNumber_MaxLength = 12;

  /// <summary>
  /// The value of the KAECSES_CASE_NUMBER attribute.
  /// A unique number that identifies CSE cases within the KAECSES application. 
  /// This attribute is half of the composite identifier for the parent entity.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length
    = KaecsesCaseNumber_MaxLength)]
  public string KaecsesCaseNumber
  {
    get => kaecsesCaseNumber ?? "";
    set => kaecsesCaseNumber =
      TrimEnd(Substring(value, 1, KaecsesCaseNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the KaecsesCaseNumber attribute.</summary>
  [JsonPropertyName("kaecsesCaseNumber")]
  [Computed]
  public string KaecsesCaseNumber_Json
  {
    get => NullIf(KaecsesCaseNumber, "");
    set => KaecsesCaseNumber = value;
  }

  /// <summary>Length of the KAECSES_RELATIONSHIP attribute.</summary>
  public const int KaecsesRelationship_MaxLength = 2;

  /// <summary>
  /// The value of the KAECSES_RELATIONSHIP attribute.
  /// Represents the case role (AR, AP, CH) that the CSE_Person represented in 
  /// this occurrence played on the given KAECSES case represented by this
  /// occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length
    = KaecsesRelationship_MaxLength)]
  public string KaecsesRelationship
  {
    get => kaecsesRelationship ?? "";
    set => kaecsesRelationship =
      TrimEnd(Substring(value, 1, KaecsesRelationship_MaxLength));
  }

  /// <summary>
  /// The json value of the KaecsesRelationship attribute.</summary>
  [JsonPropertyName("kaecsesRelationship")]
  [Computed]
  public string KaecsesRelationship_Json
  {
    get => NullIf(KaecsesRelationship, "");
    set => KaecsesRelationship = value;
  }

  /// <summary>
  /// The value of the KAECSES_START_DATE attribute.
  /// The date that the CSE_Person identified in this occurrence started their 
  /// role within the KAECSES case identified in this occurrence.
  /// </summary>
  [JsonPropertyName("kaecsesStartDate")]
  [Member(Index = 4, Type = MemberType.Date)]
  public DateTime? KaecsesStartDate
  {
    get => kaecsesStartDate;
    set => kaecsesStartDate = value;
  }

  /// <summary>
  /// The value of the KAECSES_END_DATE attribute.
  /// The date that the CSE_Person identified in this occurrence ended their 
  /// role within the KAECSES case identified in this occurrence.
  /// </summary>
  [JsonPropertyName("kaecsesEndDate")]
  [Member(Index = 5, Type = MemberType.Date)]
  public DateTime? KaecsesEndDate
  {
    get => kaecsesEndDate;
    set => kaecsesEndDate = value;
  }

  /// <summary>
  /// The value of the CREATED_TIMESTAMP attribute.
  /// The stystem timestamp for when this occurrence was created.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 6, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  private string csePersonNumber;
  private string kaecsesCaseNumber;
  private string kaecsesRelationship;
  private DateTime? kaecsesStartDate;
  private DateTime? kaecsesEndDate;
  private DateTime? createdTimestamp;
}
