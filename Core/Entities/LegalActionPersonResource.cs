// The source file: LEGAL_ACTION_PERSON_RESOURCE, ID: 371436894, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: LGLENFAC
/// The date a Lien is placed against a financial and tangible resource which is
/// owned by a CSE person. This is to eliminate a many to many relationship
/// between legal action and CSE Person Resource.
/// </summary>
[Serializable]
public partial class LegalActionPersonResource: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public LegalActionPersonResource()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public LegalActionPersonResource(LegalActionPersonResource that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new LegalActionPersonResource Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(LegalActionPersonResource that)
  {
    base.Assign(that);
    lienType = that.lienType;
    effectiveDate = that.effectiveDate;
    endDate = that.endDate;
    createdTstamp = that.createdTstamp;
    createdBy = that.createdBy;
    identifier = that.identifier;
    cprResourceNo = that.cprResourceNo;
    cspNumber = that.cspNumber;
    lgaIdentifier = that.lgaIdentifier;
  }

  /// <summary>Length of the LIEN_TYPE attribute.</summary>
  public const int LienType_MaxLength = 1;

  /// <summary>
  /// The value of the LIEN_TYPE attribute.
  /// This identifies the type of lien being placed on the resource.
  /// Valid value are:
  /// 	P - Personal Property
  /// 	R - Real Property
  /// </summary>
  [JsonPropertyName("lienType")]
  [Member(Index = 1, Type = MemberType.Char, Length = LienType_MaxLength, Optional
    = true)]
  public string LienType
  {
    get => lienType;
    set => lienType = value != null
      ? TrimEnd(Substring(value, 1, LienType_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// The beginning date that a lien is placed on a person's personal or real 
  /// property.
  /// </summary>
  [JsonPropertyName("effectiveDate")]
  [Member(Index = 2, Type = MemberType.Date)]
  public DateTime? EffectiveDate
  {
    get => effectiveDate;
    set => effectiveDate = value;
  }

  /// <summary>
  /// The value of the END_DATE attribute.
  /// The date that a lien is no longer in effect on a person's personal or real
  /// property
  /// </summary>
  [JsonPropertyName("endDate")]
  [Member(Index = 3, Type = MemberType.Date, Optional = true)]
  public DateTime? EndDate
  {
    get => endDate;
    set => endDate = value;
  }

  /// <summary>
  /// The value of the CREATED_TSTAMP attribute.
  /// The date a Lien from a legal action has been placed on a financial and 
  /// tangible resource owned by a CSE Person.
  /// </summary>
  [JsonPropertyName("createdTstamp")]
  [Member(Index = 4, Type = MemberType.Timestamp)]
  public DateTime? CreatedTstamp
  {
    get => createdTstamp;
    set => createdTstamp = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The signon of the person that created the occurrance of the entity.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// The value of the IDENTIFIER attribute.
  /// This attribute together with the relationships with CSE PERSON RESOURCE 
  /// and LEGAL ACTION uniquely identifies an occurrence of this entity type.
  /// </summary>
  [JsonPropertyName("identifier")]
  [DefaultValue(0)]
  [Member(Index = 6, Type = MemberType.Number, Length = 3)]
  public int Identifier
  {
    get => identifier;
    set => identifier = value;
  }

  /// <summary>
  /// The value of the RESOURCE_NO attribute.
  /// A running serial number of the resource owned by the CSE Person.
  /// </summary>
  [JsonPropertyName("cprResourceNo")]
  [DefaultValue(0)]
  [Member(Index = 7, Type = MemberType.Number, Length = 3)]
  public int CprResourceNo
  {
    get => cprResourceNo;
    set => cprResourceNo = value;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = CspNumber_MaxLength)]
  public string CspNumber
  {
    get => cspNumber ?? "";
    set => cspNumber = TrimEnd(Substring(value, 1, CspNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the CspNumber attribute.</summary>
  [JsonPropertyName("cspNumber")]
  [Computed]
  public string CspNumber_Json
  {
    get => NullIf(CspNumber, "");
    set => CspNumber = value;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// This is a system generated number to uniquely identify a legal action.
  /// </summary>
  [JsonPropertyName("lgaIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 9, Type = MemberType.Number, Length = 9)]
  public int LgaIdentifier
  {
    get => lgaIdentifier;
    set => lgaIdentifier = value;
  }

  private string lienType;
  private DateTime? effectiveDate;
  private DateTime? endDate;
  private DateTime? createdTstamp;
  private string createdBy;
  private int identifier;
  private int cprResourceNo;
  private string cspNumber;
  private int lgaIdentifier;
}
