// The source file: FUND, ID: 371434685, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// A Fund represents a separation of money for keeping track of certain kinds 
/// of receipts coming in.  The money for these separate accounts may actually
/// be in one bank account but they are separate accounts in CSE's system.  Only
/// money that needs to be tracked for government programs is accounted for in
/// a special fund.
/// Examples: Clearing Account Fund, ADC Fund, Foster Care Fund, State Fee Fund.
/// </summary>
[Serializable]
public partial class Fund: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public Fund()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public Fund(Fund that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new Fund Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(Fund that)
  {
    base.Assign(that);
    systemGeneratedIdentifier = that.systemGeneratedIdentifier;
    number = that.number;
    name = that.name;
    description = that.description;
    balanceAmount = that.balanceAmount;
    createdBy = that.createdBy;
    createdTimetstamp = that.createdTimetstamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTmst = that.lastUpdatedTmst;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("systemGeneratedIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 3)]
  public int SystemGeneratedIdentifier
  {
    get => systemGeneratedIdentifier;
    set => systemGeneratedIdentifier = value;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int Number_MaxLength = 5;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// A short representation for the purpose of quick identification.  This code
  /// is usually a fund number.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = Number_MaxLength)]
  public string Number
  {
    get => number ?? "";
    set => number = TrimEnd(Substring(value, 1, Number_MaxLength));
  }

  /// <summary>
  /// The json value of the Number attribute.</summary>
  [JsonPropertyName("number")]
  [Computed]
  public string Number_Json
  {
    get => NullIf(Number, "");
    set => Number = value;
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
  [Member(Index = 3, Type = MemberType.Char, Length = Name_MaxLength)]
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
  /// An explanation of the fund usage intent.
  /// </summary>
  [JsonPropertyName("description")]
  [Member(Index = 4, Type = MemberType.Varchar, Length
    = Description_MaxLength, Optional = true)]
  public string Description
  {
    get => description;
    set => description = value != null
      ? Substring(value, 1, Description_MaxLength) : null;
  }

  /// <summary>
  /// The value of the BALANCE_AMOUNT attribute.
  /// The balance amount is the current balance of the particular fund.  This 
  /// amount field will not be updated directly but only thru fund transactions.
  /// There will not be a process that allows a person to go in and directly
  /// input this amount for a fund.
  /// Example:  The ADC fund may have 2,000,000 dollars at a particular point of
  /// time.
  /// </summary>
  [JsonPropertyName("balanceAmount")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 5, Type = MemberType.Number, Length = 14, Precision = 2)]
  public decimal BalanceAmount
  {
    get => balanceAmount;
    set => balanceAmount = Truncate(value, 2);
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The user id or program id responsible for the creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// The value of the CREATED_TIMETSTAMP attribute.
  /// The timestamp of when the occurrence of the entity type was created.
  /// </summary>
  [JsonPropertyName("createdTimetstamp")]
  [Member(Index = 7, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimetstamp
  {
    get => createdTimetstamp;
    set => createdTimetstamp = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// The user id or program id responsible for the last update to any of the 
  /// fund information (excluding the amount field which is only updated by fund
  /// transactions - not by a user going in and setting a fund amount).
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 8, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength, Optional
    = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TMST attribute.
  /// The timestamp of the most recent update to the description on the fund.  
  /// This field will not be updated everytime the balance is affected by a fund
  /// transaction.  Those updates will be audited by the fund transaction.
  /// </summary>
  [JsonPropertyName("lastUpdatedTmst")]
  [Member(Index = 9, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTmst
  {
    get => lastUpdatedTmst;
    set => lastUpdatedTmst = value;
  }

  private int systemGeneratedIdentifier;
  private string number;
  private string name;
  private string description;
  private decimal balanceAmount;
  private string createdBy;
  private DateTime? createdTimetstamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTmst;
}
