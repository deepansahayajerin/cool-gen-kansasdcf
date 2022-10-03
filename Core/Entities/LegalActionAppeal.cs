// The source file: LEGAL_ACTION_APPEAL, ID: 371436716, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: LGLEN
/// This entity resolves the many-to-many relationship between LEGAL_ACTION and 
/// APPEAL.
/// One appeal may be filed against one or more legal action.  One legal action 
/// may be filed by more than one person.  E.g. both the AP and the CSE appeal
/// against the judgement -- AP appealing the decision of paternity and CSE
/// appealing the decision of support order.  However these appeals will be
/// treated by the appelate court as one appeal only and they will be assigned
/// the same appeal docket number.
/// It is also important to note that one legal action cannot be appealed more 
/// than one by the same person against the same legal action over time.
/// </summary>
[Serializable]
public partial class LegalActionAppeal: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public LegalActionAppeal()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public LegalActionAppeal(LegalActionAppeal that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new LegalActionAppeal Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(LegalActionAppeal that)
  {
    base.Assign(that);
    identifier = that.identifier;
    createdBy = that.createdBy;
    createdTmst = that.createdTmst;
    aplId = that.aplId;
    lgaId = that.lgaId;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// This attribute together with its relationships to LEGAL_ACTION and APPEAL 
  /// uniquely identifies one occurrence of the entity.
  /// </summary>
  [JsonPropertyName("identifier")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 3)]
  public int Identifier
  {
    get => identifier;
    set => identifier = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The User ID or Program ID responsible for the creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// The timestamp the occurrence was created.
  /// </summary>
  [JsonPropertyName("createdTmst")]
  [Member(Index = 3, Type = MemberType.Timestamp)]
  public DateTime? CreatedTmst
  {
    get => createdTmst;
    set => createdTmst = value;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// This is a system generated number to uniquely identify an appeal.
  /// </summary>
  [JsonPropertyName("aplId")]
  [DefaultValue(0)]
  [Member(Index = 4, Type = MemberType.Number, Length = 9)]
  public int AplId
  {
    get => aplId;
    set => aplId = value;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// This is a system generated number to uniquely identify a legal action.
  /// </summary>
  [JsonPropertyName("lgaId")]
  [DefaultValue(0)]
  [Member(Index = 5, Type = MemberType.Number, Length = 9)]
  public int LgaId
  {
    get => lgaId;
    set => lgaId = value;
  }

  private int identifier;
  private string createdBy;
  private DateTime? createdTmst;
  private int aplId;
  private int lgaId;
}
