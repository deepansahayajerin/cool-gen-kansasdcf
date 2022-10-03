// The source file: URA_COLLECTION_APPLICATION, ID: 374416431, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP:  OBLGESTB
/// 
/// This entity will describe the application of a collection to a
/// household to offset the households URA. Any one collection may be
/// applied to one or more households up to the amount of the collection.
/// Only collections that have a program applied to code of &quot;AF
/// &quot; and a program state applied to code of &quot;PA&quot;,&quot;TA
/// &quot; or &quot;CA&quot; can be associated to households.
/// </summary>
[Serializable]
public partial class UraCollectionApplication: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public UraCollectionApplication()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public UraCollectionApplication(UraCollectionApplication that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new UraCollectionApplication Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(UraCollectionApplication that)
  {
    base.Assign(that);
    collectionAmountApplied = that.collectionAmountApplied;
    createdBy = that.createdBy;
    createdTstamp = that.createdTstamp;
    type1 = that.type1;
    cspNumber0 = that.cspNumber0;
    imsYear = that.imsYear;
    imsMonth = that.imsMonth;
    imhAeCaseNo = that.imhAeCaseNo;
    obgIdentifier = that.obgIdentifier;
    cpaType = that.cpaType;
    otyIdentifier = that.otyIdentifier;
    cspNumber = that.cspNumber;
    otrIdentifier = that.otrIdentifier;
    crvIdentifier = that.crvIdentifier;
    colIdentifier = that.colIdentifier;
    crdIdentifier = that.crdIdentifier;
    cstIdentifier = that.cstIdentifier;
    crtIdentifier = that.crtIdentifier;
    otrType = that.otrType;
  }

  /// <summary>
  /// The value of the COLLECTION_AMOUNT_APPLIED attribute.
  /// This attribute will contain the amount of a collection that has been 
  /// applied to a household to offset the URA for that household.
  /// </summary>
  [JsonPropertyName("collectionAmountApplied")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 1, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal CollectionAmountApplied
  {
    get => collectionAmountApplied;
    set => collectionAmountApplied = Truncate(value, 2);
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// Standard attribute description.
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
  /// The value of the CREATED_TSTAMP attribute.
  /// Standard Definition
  /// </summary>
  [JsonPropertyName("createdTstamp")]
  [Member(Index = 3, Type = MemberType.Timestamp)]
  public DateTime? CreatedTstamp
  {
    get => createdTstamp;
    set => createdTstamp = value;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int Type1_MaxLength = 1;

  /// <summary>
  /// The value of the TYPE attribute.
  /// This attribute will type of URA that the collection was applied to.
  /// 
  /// Applied to:
  /// 
  /// A-AF/FC URA Balance
  /// M-
  /// Medical URA Balance
  /// </summary>
  [JsonPropertyName("type1")]
  [Member(Index = 4, Type = MemberType.Char, Length = Type1_MaxLength, Optional
    = true)]
  public string Type1
  {
    get => type1;
    set => type1 = value != null
      ? TrimEnd(Substring(value, 1, Type1_MaxLength)) : null;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspNumber0_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = CspNumber0_MaxLength)]
  public string CspNumber0
  {
    get => cspNumber0 ?? "";
    set => cspNumber0 = TrimEnd(Substring(value, 1, CspNumber0_MaxLength));
  }

  /// <summary>
  /// The json value of the CspNumber0 attribute.</summary>
  [JsonPropertyName("cspNumber0")]
  [Computed]
  public string CspNumber0_Json
  {
    get => NullIf(CspNumber0, "");
    set => CspNumber0 = value;
  }

  /// <summary>
  /// The value of the YEAR attribute.
  /// The year in which the member received some for of grant
  /// </summary>
  [JsonPropertyName("imsYear")]
  [DefaultValue(0)]
  [Member(Index = 6, Type = MemberType.Number, Length = 4)]
  public int ImsYear
  {
    get => imsYear;
    set => imsYear = value;
  }

  /// <summary>
  /// The value of the MONTH attribute.
  /// The month in which the member received some for of grant.
  /// </summary>
  [JsonPropertyName("imsMonth")]
  [DefaultValue(0)]
  [Member(Index = 7, Type = MemberType.Number, Length = 2)]
  public int ImsMonth
  {
    get => imsMonth;
    set => imsMonth = value;
  }

  /// <summary>Length of the AE_CASE_NO attribute.</summary>
  public const int ImhAeCaseNo_MaxLength = 8;

  /// <summary>
  /// The value of the AE_CASE_NO attribute.
  /// This attribute defines the AE Case No for the IM Household.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = ImhAeCaseNo_MaxLength)]
  public string ImhAeCaseNo
  {
    get => imhAeCaseNo ?? "";
    set => imhAeCaseNo = TrimEnd(Substring(value, 1, ImhAeCaseNo_MaxLength));
  }

  /// <summary>
  /// The json value of the ImhAeCaseNo attribute.</summary>
  [JsonPropertyName("imhAeCaseNo")]
  [Computed]
  public string ImhAeCaseNo_Json
  {
    get => NullIf(ImhAeCaseNo, "");
    set => ImhAeCaseNo = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number (sort descending) that, along
  /// with CSE Person, distinguishes one occurrence of the entity type from
  /// another.
  /// </summary>
  [JsonPropertyName("obgIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 9, Type = MemberType.Number, Length = 3)]
  public int ObgIdentifier
  {
    get => obgIdentifier;
    set => obgIdentifier = value;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int CpaType_MaxLength = 1;

  /// <summary>
  /// The value of the TYPE attribute.
  /// Defines the type of account the entity type represents.
  /// Examples:
  /// 	- Obligor
  /// 	- Obligee
  /// 	- Supported Person
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = CpaType_MaxLength)]
  [Value("S")]
  [Value("R")]
  [Value("E")]
  [ImplicitValue("S")]
  public string CpaType
  {
    get => cpaType ?? "";
    set => cpaType = TrimEnd(Substring(value, 1, CpaType_MaxLength));
  }

  /// <summary>
  /// The json value of the CpaType attribute.</summary>
  [JsonPropertyName("cpaType")]
  [Computed]
  public string CpaType_Json
  {
    get => NullIf(CpaType, "");
    set => CpaType = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("otyIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 11, Type = MemberType.Number, Length = 3)]
  public int OtyIdentifier
  {
    get => otyIdentifier;
    set => otyIdentifier = value;
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
  [Member(Index = 12, Type = MemberType.Char, Length = CspNumber_MaxLength)]
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
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated random number to provide a unique identifier 
  /// for each transaction.
  /// </summary>
  [JsonPropertyName("otrIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 13, Type = MemberType.Number, Length = 9)]
  public int OtrIdentifier
  {
    get => otrIdentifier;
    set => otrIdentifier = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique sequential number that identifies each negotiable instrument in 
  /// any form of money received by CSE.  Or any information of money received
  /// by CSE.
  /// Examples:
  /// Cash, checks, court transmittals.
  /// </summary>
  [JsonPropertyName("crvIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 14, Type = MemberType.Number, Length = 9)]
  public int CrvIdentifier
  {
    get => crvIdentifier;
    set => crvIdentifier = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated random number to provide a unique identifier 
  /// for each collection.
  /// </summary>
  [JsonPropertyName("colIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 15, Type = MemberType.Number, Length = 9)]
  public int ColIdentifier
  {
    get => colIdentifier;
    set => colIdentifier = value;
  }

  /// <summary>
  /// The value of the SEQUENTIAL_IDENTIFIER attribute.
  /// A unique sequential number within CASH_RECEIPT that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("crdIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 16, Type = MemberType.Number, Length = 4)]
  public int CrdIdentifier
  {
    get => crdIdentifier;
    set => crdIdentifier = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("cstIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 17, Type = MemberType.Number, Length = 3)]
  public int CstIdentifier
  {
    get => cstIdentifier;
    set => cstIdentifier = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("crtIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 18, Type = MemberType.Number, Length = 3)]
  public int CrtIdentifier
  {
    get => crtIdentifier;
    set => crtIdentifier = value;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int OtrType_MaxLength = 2;

  /// <summary>
  /// The value of the TYPE attribute.
  /// Defines the obligation transaction type.
  /// Permitted Values:
  /// 	- DE = DEBT
  /// 	- DA = DEBT ADJUSTMENT
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 19, Type = MemberType.Char, Length = OtrType_MaxLength)]
  [Value("DE")]
  [Value("Z2")]
  [Value("DA")]
  [ImplicitValue("DE")]
  public string OtrType
  {
    get => otrType ?? "";
    set => otrType = TrimEnd(Substring(value, 1, OtrType_MaxLength));
  }

  /// <summary>
  /// The json value of the OtrType attribute.</summary>
  [JsonPropertyName("otrType")]
  [Computed]
  public string OtrType_Json
  {
    get => NullIf(OtrType, "");
    set => OtrType = value;
  }

  private decimal collectionAmountApplied;
  private string createdBy;
  private DateTime? createdTstamp;
  private string type1;
  private string cspNumber0;
  private int imsYear;
  private int imsMonth;
  private string imhAeCaseNo;
  private int obgIdentifier;
  private string cpaType;
  private int otyIdentifier;
  private string cspNumber;
  private int otrIdentifier;
  private int crvIdentifier;
  private int colIdentifier;
  private int crdIdentifier;
  private int cstIdentifier;
  private int crtIdentifier;
  private string otrType;
}
