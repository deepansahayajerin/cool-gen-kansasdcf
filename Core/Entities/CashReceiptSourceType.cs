// The source file: CASH_RECEIPT_SOURCE_TYPE, ID: 371432012, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// Scope: The origin of the cash receipt received.
/// Qualifcation: Person or Organization.
/// Purpose: To track the origin of cash receipts received.
/// Examples:  Court, State Government, Federal Government, Field Office, Payor 
/// Responsible for Debt, etc.
/// Integrity Conditions:  Source Type can not deleted if used by Cash Receipt 
/// Event.
/// </summary>
[Serializable]
public partial class CashReceiptSourceType: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public CashReceiptSourceType()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public CashReceiptSourceType(CashReceiptSourceType that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new CashReceiptSourceType Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(CashReceiptSourceType that)
  {
    base.Assign(that);
    interfaceIndicator = that.interfaceIndicator;
    systemGeneratedIdentifier = that.systemGeneratedIdentifier;
    code = that.code;
    name = that.name;
    description = that.description;
    effectiveDate = that.effectiveDate;
    discontinueDate = that.discontinueDate;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTmst = that.lastUpdatedTmst;
    courtInd = that.courtInd;
    state = that.state;
    county = that.county;
    location = that.location;
  }

  /// <summary>Length of the INTERFACE_INDICATOR attribute.</summary>
  public const int InterfaceIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the INTERFACE_INDICATOR attribute.
  /// This indicator specifies if the source type provides an electronic 
  /// interface.  This is used to ensure a match is made between the actual
  /// online receipting when the cash actually arrives with the receipt created
  /// during transmission process that is created in an 'interfaced' status.
  /// Valid values are Y and N.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length
    = InterfaceIndicator_MaxLength)]
  [Value("N")]
  [Value("Y")]
  [ImplicitValue("N")]
  public string InterfaceIndicator
  {
    get => interfaceIndicator ?? "";
    set => interfaceIndicator =
      TrimEnd(Substring(value, 1, InterfaceIndicator_MaxLength));
  }

  /// <summary>
  /// The json value of the InterfaceIndicator attribute.</summary>
  [JsonPropertyName("interfaceIndicator")]
  [Computed]
  public string InterfaceIndicator_Json
  {
    get => NullIf(InterfaceIndicator, "");
    set => InterfaceIndicator = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("systemGeneratedIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 2, Type = MemberType.Number, Length = 3)]
  public int SystemGeneratedIdentifier
  {
    get => systemGeneratedIdentifier;
    set => systemGeneratedIdentifier = value;
  }

  /// <summary>Length of the CODE attribute.</summary>
  public const int Code_MaxLength = 10;

  /// <summary>
  /// The value of the CODE attribute.
  /// A short representation of the name for the purpose of quick 
  /// indentification.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = Code_MaxLength)]
  public string Code
  {
    get => code ?? "";
    set => code = TrimEnd(Substring(value, 1, Code_MaxLength));
  }

  /// <summary>
  /// The json value of the Code attribute.</summary>
  [JsonPropertyName("code")]
  [Computed]
  public string Code_Json
  {
    get => NullIf(Code, "");
    set => Code = value;
  }

  /// <summary>Length of the NAME attribute.</summary>
  public const int Name_MaxLength = 40;

  /// <summary>
  /// The value of the NAME attribute.
  /// The name of the code.  This name will be more descriptive than the code 
  /// but will be much less shorter than the description.  The reason for
  /// needing the name is that the code is not descriptive enough and the
  /// description is too long for the list screens.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = Name_MaxLength)]
  public string Name
  {
    get => name ?? "";
    set => name = TrimEnd(Substring(value, 1, Name_MaxLength));
  }

  /// <summary>
  /// The json value of the Name attribute.</summary>
  [JsonPropertyName("name")]
  [Computed]
  public string Name_Json
  {
    get => NullIf(Name, "");
    set => Name = value;
  }

  /// <summary>Length of the DESCRIPTION attribute.</summary>
  public const int Description_MaxLength = 240;

  /// <summary>
  /// The value of the DESCRIPTION attribute.
  /// * Draft *
  /// An explanation of the entity type.  The description should be specific 
  /// enough to allow a person to distinguish/understand the entity type.
  /// </summary>
  [JsonPropertyName("description")]
  [Member(Index = 5, Type = MemberType.Varchar, Length
    = Description_MaxLength, Optional = true)]
  public string Description
  {
    get => description;
    set => description = value != null
      ? Substring(value, 1, Description_MaxLength) : null;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// The date upon which this occurrence of the entity is activated by the 
  /// system.  An effective date allows the entity to be entered into the system
  /// with a future date.  The occurrence of the entity will &quot;take effect
  /// &quot; on the effective date.
  /// </summary>
  [JsonPropertyName("effectiveDate")]
  [Member(Index = 6, Type = MemberType.Date)]
  public DateTime? EffectiveDate
  {
    get => effectiveDate;
    set => effectiveDate = value;
  }

  /// <summary>
  /// The value of the DISCONTINUE_DATE attribute.
  /// The date upon which this occurrence of the entity can no longer be used.
  /// </summary>
  [JsonPropertyName("discontinueDate")]
  [Member(Index = 7, Type = MemberType.Date, Optional = true)]
  public DateTime? DiscontinueDate
  {
    get => discontinueDate;
    set => discontinueDate = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The user id or program id responsible for the creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// The value of the CREATED_TIMESTAMP attribute.
  /// The timestamp of when the occurrence of the entity was created.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 9, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// The user id or program id responsible for the last update to the 
  /// occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 10, Type = MemberType.Char, Length
    = LastUpdatedBy_MaxLength, Optional = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TMST attribute.
  /// The timestamp of the most recent update to the entity occurence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTmst")]
  [Member(Index = 11, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTmst
  {
    get => lastUpdatedTmst;
    set => lastUpdatedTmst = value;
  }

  /// <summary>Length of the COURT_IND attribute.</summary>
  public const int CourtInd_MaxLength = 1;

  /// <summary>
  /// The value of the COURT_IND attribute.
  ///  Defines the type of source as being Court or not. We need to notify the 
  /// courts of any payments received so that they can credit the obligor for
  /// these payments on their records.
  /// 	Values: C - Court
  ///                 N - Non Court
  /// </summary>
  [JsonPropertyName("courtInd")]
  [Member(Index = 12, Type = MemberType.Char, Length = CourtInd_MaxLength, Optional
    = true)]
  public string CourtInd
  {
    get => courtInd;
    set => courtInd = value != null
      ? TrimEnd(Substring(value, 1, CourtInd_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the STATE attribute.
  /// The first two character of the FIPS code which identify the state.
  /// </summary>
  [JsonPropertyName("state")]
  [Member(Index = 13, Type = MemberType.Number, Length = 2, Optional = true)]
  public int? State
  {
    get => state;
    set => state = value;
  }

  /// <summary>
  /// The value of the COUNTY attribute.
  /// This is 3-5 position of the FIPS code identifying the county.
  /// </summary>
  [JsonPropertyName("county")]
  [Member(Index = 14, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? County
  {
    get => county;
    set => county = value;
  }

  /// <summary>
  /// The value of the LOCATION attribute.
  /// The last two positions of the FIPS code which identify a location within 
  /// the county.
  /// </summary>
  [JsonPropertyName("location")]
  [Member(Index = 15, Type = MemberType.Number, Length = 2, Optional = true)]
  public int? Location
  {
    get => location;
    set => location = value;
  }

  private string interfaceIndicator;
  private int systemGeneratedIdentifier;
  private string code;
  private string name;
  private string description;
  private DateTime? effectiveDate;
  private DateTime? discontinueDate;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTmst;
  private string courtInd;
  private int? state;
  private int? county;
  private int? location;
}
