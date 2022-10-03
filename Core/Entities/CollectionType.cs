// The source file: COLLECTION_TYPE, ID: 371432436, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// Represents the form of collection.
/// Example: Payment, debt set-off, IWO, direct pay, etc.
/// </summary>
[Serializable]
public partial class CollectionType: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public CollectionType()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public CollectionType(CollectionType that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new CollectionType Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(CollectionType that)
  {
    base.Assign(that);
    printName = that.printName;
    sequentialIdentifier = that.sequentialIdentifier;
    code = that.code;
    name = that.name;
    description = that.description;
    cashNonCashInd = that.cashNonCashInd;
    disbursementInd = that.disbursementInd;
    effectiveDate = that.effectiveDate;
    discontinueDate = that.discontinueDate;
    createdBy = that.createdBy;
    createdTmst = that.createdTmst;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTmst = that.lastUpdatedTmst;
  }

  /// <summary>Length of the PRINT_NAME attribute.</summary>
  public const int PrintName_MaxLength = 20;

  /// <summary>
  /// The value of the PRINT_NAME attribute.
  /// This print name will be used on all printable materials.  This is so that 
  /// the code used by SRS is not shown to applicant recipients in the case of
  /// unemployment where such information is confidential to the absent parent.
  /// </summary>
  [JsonPropertyName("printName")]
  [Member(Index = 1, Type = MemberType.Char, Length = PrintName_MaxLength, Optional
    = true)]
  public string PrintName
  {
    get => printName;
    set => printName = value != null
      ? TrimEnd(Substring(value, 1, PrintName_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the SEQUENTIAL_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("sequentialIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 2, Type = MemberType.Number, Length = 3)]
  public int SequentialIdentifier
  {
    get => sequentialIdentifier;
    set => sequentialIdentifier = value;
  }

  /// <summary>Length of the CODE attribute.</summary>
  public const int Code_MaxLength = 10;

  /// <summary>
  /// The value of the CODE attribute.
  /// A short representation of the name for the purpose of quick 
  /// identification.
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
  /// The name that is used to uniquely describe the entity type.
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

  /// <summary>Length of the CASH_NON_CASH_IND attribute.</summary>
  public const int CashNonCashInd_MaxLength = 1;

  /// <summary>
  /// The value of the CASH_NON_CASH_IND attribute.
  /// An indication of the kind of collection received.  Cash or Non-Cash.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = CashNonCashInd_MaxLength)]
  [Value("N")]
  [Value("C")]
  [ImplicitValue("C")]
  public string CashNonCashInd
  {
    get => cashNonCashInd ?? "";
    set => cashNonCashInd =
      TrimEnd(Substring(value, 1, CashNonCashInd_MaxLength));
  }

  /// <summary>
  /// The json value of the CashNonCashInd attribute.</summary>
  [JsonPropertyName("cashNonCashInd")]
  [Computed]
  public string CashNonCashInd_Json
  {
    get => NullIf(CashNonCashInd, "");
    set => CashNonCashInd = value;
  }

  /// <summary>Length of the DISBURSEMENT_IND attribute.</summary>
  public const int DisbursementInd_MaxLength = 1;

  /// <summary>
  /// The value of the DISBURSEMENT_IND attribute.
  /// A indicator used to determine if this collection should be used to 
  /// generate a disbursement.  Some collections will not end up creating a
  /// disbursement because the disbursement was previously done by the court,
  /// etc.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = DisbursementInd_MaxLength)]
    
  [Value("Y")]
  [Value("N")]
  [ImplicitValue("Y")]
  public string DisbursementInd
  {
    get => disbursementInd ?? "";
    set => disbursementInd =
      TrimEnd(Substring(value, 1, DisbursementInd_MaxLength));
  }

  /// <summary>
  /// The json value of the DisbursementInd attribute.</summary>
  [JsonPropertyName("disbursementInd")]
  [Computed]
  public string DisbursementInd_Json
  {
    get => NullIf(DisbursementInd, "");
    set => DisbursementInd = value;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// The date upon which this occurrence of the entity is activated by the 
  /// system.  An EFFECTIVE_DATE allows the entity to be entered into the system
  /// with a future date.  The occurrence of the entity will &quot;take effect
  /// &quot; on the EFFECTIVE_DATE.
  /// </summary>
  [JsonPropertyName("effectiveDate")]
  [Member(Index = 8, Type = MemberType.Date)]
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
  [Member(Index = 9, Type = MemberType.Date, Optional = true)]
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
  [Member(Index = 10, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// The timestamp of when the occurrence of the entity type was created.
  /// </summary>
  [JsonPropertyName("createdTmst")]
  [Member(Index = 11, Type = MemberType.Timestamp)]
  public DateTime? CreatedTmst
  {
    get => createdTmst;
    set => createdTmst = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// The user id or program id responsible for the last update to the 
  /// occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 12, Type = MemberType.Char, Length
    = LastUpdatedBy_MaxLength, Optional = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TMST attribute.
  /// The timestamp of the most recent update to the entity occcurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTmst")]
  [Member(Index = 13, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTmst
  {
    get => lastUpdatedTmst;
    set => lastUpdatedTmst = value;
  }

  private string printName;
  private int sequentialIdentifier;
  private string code;
  private string name;
  private string description;
  private string cashNonCashInd;
  private string disbursementInd;
  private DateTime? effectiveDate;
  private DateTime? discontinueDate;
  private string createdBy;
  private DateTime? createdTmst;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTmst;
}
