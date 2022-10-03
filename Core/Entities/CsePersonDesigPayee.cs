// The source file: CSE_PERSON_DESIG_PAYEE, ID: 371433070, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: OBLGEST				
/// This is to store all designated payee for a particular AR in KESSEP.
/// </summary>
[Serializable]
public partial class CsePersonDesigPayee: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public CsePersonDesigPayee()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public CsePersonDesigPayee(CsePersonDesigPayee that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new CsePersonDesigPayee Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(CsePersonDesigPayee that)
  {
    base.Assign(that);
    sequentialIdentifier = that.sequentialIdentifier;
    effectiveDate = that.effectiveDate;
    discontinueDate = that.discontinueDate;
    createdBy = that.createdBy;
    createdTmst = that.createdTmst;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTmst = that.lastUpdatedTmst;
    notes = that.notes;
    csePersoNum = that.csePersoNum;
    csePersNum = that.csePersNum;
  }

  /// <summary>
  /// The value of the SEQUENTIAL_IDENTIFIER attribute.
  /// </summary>
  [JsonPropertyName("sequentialIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 3)]
  public int SequentialIdentifier
  {
    get => sequentialIdentifier;
    set => sequentialIdentifier = value;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// </summary>
  [JsonPropertyName("effectiveDate")]
  [Member(Index = 2, Type = MemberType.Date)]
  public DateTime? EffectiveDate
  {
    get => effectiveDate;
    set => effectiveDate = value;
  }

  /// <summary>
  /// The value of the DISCONTINUE_DATE attribute.
  /// </summary>
  [JsonPropertyName("discontinueDate")]
  [Member(Index = 3, Type = MemberType.Date, Optional = true)]
  public DateTime? DiscontinueDate
  {
    get => discontinueDate;
    set => discontinueDate = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
  public string CreatedBy
  {
    get => createdBy ?? "";
    set => createdBy = TrimEnd(Substring(value, 1, CreatedBy_MaxLength));
  }

  /// <summary>
  /// The json value of the CreatedBy attribute.</summary>
  [JsonPropertyName("createdBy")]
  [Computed]
  public string CreatedBy_Json
  {
    get => NullIf(CreatedBy, "");
    set => CreatedBy = value;
  }

  /// <summary>
  /// The value of the CREATED_TMST attribute.
  /// </summary>
  [JsonPropertyName("createdTmst")]
  [Member(Index = 5, Type = MemberType.Timestamp)]
  public DateTime? CreatedTmst
  {
    get => createdTmst;
    set => createdTmst = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 6, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength, Optional
    = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TMST attribute.
  /// </summary>
  [JsonPropertyName("lastUpdatedTmst")]
  [Member(Index = 7, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTmst
  {
    get => lastUpdatedTmst;
    set => lastUpdatedTmst = value;
  }

  /// <summary>Length of the NOTES attribute.</summary>
  public const int Notes_MaxLength = 240;

  /// <summary>
  /// The value of the NOTES attribute.
  /// </summary>
  [JsonPropertyName("notes")]
  [Member(Index = 8, Type = MemberType.Char, Length = Notes_MaxLength, Optional
    = true)]
  public string Notes
  {
    get => notes;
    set => notes = value != null
      ? TrimEnd(Substring(value, 1, Notes_MaxLength)) : null;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CsePersoNum_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = CsePersoNum_MaxLength)]
  public string CsePersoNum
  {
    get => csePersoNum ?? "";
    set => csePersoNum = TrimEnd(Substring(value, 1, CsePersoNum_MaxLength));
  }

  /// <summary>
  /// The json value of the CsePersoNum attribute.</summary>
  [JsonPropertyName("csePersoNum")]
  [Computed]
  public string CsePersoNum_Json
  {
    get => NullIf(CsePersoNum, "");
    set => CsePersoNum = value;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CsePersNum_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonPropertyName("csePersNum")]
  [Member(Index = 10, Type = MemberType.Char, Length = CsePersNum_MaxLength, Optional
    = true)]
  public string CsePersNum
  {
    get => csePersNum;
    set => csePersNum = value != null
      ? TrimEnd(Substring(value, 1, CsePersNum_MaxLength)) : null;
  }

  private int sequentialIdentifier;
  private DateTime? effectiveDate;
  private DateTime? discontinueDate;
  private string createdBy;
  private DateTime? createdTmst;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTmst;
  private string notes;
  private string csePersoNum;
  private string csePersNum;
}
