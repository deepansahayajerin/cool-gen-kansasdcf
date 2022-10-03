// The source file: PERSON_PREFERRED_PAYMENT_METHOD, ID: 371439381, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// The person preferred payment method records the payment method that a cse 
/// person chooses for a given period of time.  This information is captured
/// thru a relationship with the cse person account and a relationship to
/// payment method type.  A date range is used to record the peroiod of time
/// that a particular method was choosen for a person.
/// The default payment method will be warrant.  If no payment method exists for
/// an Obligee then we will disburse using a warrant.  This saves us from
/// having to store the selection of warrant as the payment method for 99.9% of
/// disbursements when a warrant payment method requires no additional special
/// information.
/// Example:
/// EFT - as payment method
/// 	Bank account number
/// </summary>
[Serializable]
public partial class PersonPreferredPaymentMethod: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public PersonPreferredPaymentMethod()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public PersonPreferredPaymentMethod(PersonPreferredPaymentMethod that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new PersonPreferredPaymentMethod Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(PersonPreferredPaymentMethod that)
  {
    base.Assign(that);
    systemGeneratedIdentifier = that.systemGeneratedIdentifier;
    abaRoutingNumber = that.abaRoutingNumber;
    dfiAccountNumber = that.dfiAccountNumber;
    description = that.description;
    effectiveDate = that.effectiveDate;
    discontinueDate = that.discontinueDate;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdateBy = that.lastUpdateBy;
    lastUpdateTmst = that.lastUpdateTmst;
    accountType = that.accountType;
    cspPNumber = that.cspPNumber;
    pmtGeneratedId = that.pmtGeneratedId;
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

  /// <summary>
  /// The value of the ABA_ROUTING_NUMBER attribute.
  /// A number used by the banking industry to facilitate the routing of 
  /// electronic fund transaction information.
  /// </summary>
  [JsonPropertyName("abaRoutingNumber")]
  [Member(Index = 2, Type = MemberType.Number, Length = 10, Optional = true)]
  public long? AbaRoutingNumber
  {
    get => abaRoutingNumber;
    set => abaRoutingNumber = value;
  }

  /// <summary>Length of the DFI_ACCOUNT_NUMBER attribute.</summary>
  public const int DfiAccountNumber_MaxLength = 17;

  /// <summary>
  /// The value of the DFI_ACCOUNT_NUMBER attribute.
  /// The checking account number of the account that will be used to receive 
  /// electronic fund transactions for the given Obligee.
  /// </summary>
  [JsonPropertyName("dfiAccountNumber")]
  [Member(Index = 3, Type = MemberType.Char, Length
    = DfiAccountNumber_MaxLength, Optional = true)]
  public string DfiAccountNumber
  {
    get => dfiAccountNumber;
    set => dfiAccountNumber = value != null
      ? TrimEnd(Substring(value, 1, DfiAccountNumber_MaxLength)) : null;
  }

  /// <summary>Length of the DESCRIPTION attribute.</summary>
  public const int Description_MaxLength = 240;

  /// <summary>
  /// The value of the DESCRIPTION attribute.
  /// A description of the status of a occurrance at a certain point in time.
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
  /// The value of the EFFECTIVE_DATE attribute.
  /// The date upon which this occurrence of the entity is activated by the 
  /// system.  An effective date allows the entity to be entered into the system
  /// with a future date.  The occurrence of the entity will &quot;take effect
  /// &quot; on the effective date.
  /// </summary>
  [JsonPropertyName("effectiveDate")]
  [Member(Index = 5, Type = MemberType.Date)]
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
  [Member(Index = 6, Type = MemberType.Date, Optional = true)]
  public DateTime? DiscontinueDate
  {
    get => discontinueDate;
    set => discontinueDate = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The user id or program id responsible for the last update to the 
  /// occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// The timestamp of when the occurrence of the entity type was created.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 8, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>Length of the LAST_UPDATE_BY attribute.</summary>
  public const int LastUpdateBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATE_BY attribute.
  /// The User ID or Program ID responsible for the last update to the 
  /// occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdateBy")]
  [Member(Index = 9, Type = MemberType.Char, Length = LastUpdateBy_MaxLength, Optional
    = true)]
  public string LastUpdateBy
  {
    get => lastUpdateBy;
    set => lastUpdateBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdateBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LAST_UPDATE_TMST attribute.
  /// The timestamp of the most recent update to the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdateTmst")]
  [Member(Index = 10, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdateTmst
  {
    get => lastUpdateTmst;
    set => lastUpdateTmst = value;
  }

  /// <summary>Length of the ACCOUNT_TYPE attribute.</summary>
  public const int AccountType_MaxLength = 1;

  /// <summary>
  /// The value of the ACCOUNT_TYPE attribute.
  /// Tells about the kind of payment method that the person chose to pay during
  /// a particular period in time. Permissible values are:
  /// 
  /// C Checking Account
  /// S Savings
  /// Account
  /// </summary>
  [JsonPropertyName("accountType")]
  [Member(Index = 11, Type = MemberType.Char, Length = AccountType_MaxLength, Optional
    = true)]
  public string AccountType
  {
    get => accountType;
    set => accountType = value != null
      ? TrimEnd(Substring(value, 1, AccountType_MaxLength)) : null;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspPNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length = CspPNumber_MaxLength)]
  public string CspPNumber
  {
    get => cspPNumber ?? "";
    set => cspPNumber = TrimEnd(Substring(value, 1, CspPNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the CspPNumber attribute.</summary>
  [JsonPropertyName("cspPNumber")]
  [Computed]
  public string CspPNumber_Json
  {
    get => NullIf(CspPNumber, "");
    set => CspPNumber = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("pmtGeneratedId")]
  [DefaultValue(0)]
  [Member(Index = 13, Type = MemberType.Number, Length = 3)]
  public int PmtGeneratedId
  {
    get => pmtGeneratedId;
    set => pmtGeneratedId = value;
  }

  private int systemGeneratedIdentifier;
  private long? abaRoutingNumber;
  private string dfiAccountNumber;
  private string description;
  private DateTime? effectiveDate;
  private DateTime? discontinueDate;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdateBy;
  private DateTime? lastUpdateTmst;
  private string accountType;
  private string cspPNumber;
  private int pmtGeneratedId;
}
