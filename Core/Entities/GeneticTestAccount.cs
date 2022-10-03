// The source file: GENETIC_TEST_ACCOUNT, ID: 371434929, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: OBLGESTB
/// The CSE account number used to authorize the lab to perform a genetic test.
/// </summary>
[Serializable]
public partial class GeneticTestAccount: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public GeneticTestAccount()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public GeneticTestAccount(GeneticTestAccount that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new GeneticTestAccount Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(GeneticTestAccount that)
  {
    base.Assign(that);
    accountNumber = that.accountNumber;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    ospRoleCode = that.ospRoleCode;
    ospEffectiveDate = that.ospEffectiveDate;
    offGeneratedId = that.offGeneratedId;
    spdGeneratedId = that.spdGeneratedId;
  }

  /// <summary>Length of the ACCOUNT_NUMBER attribute.</summary>
  public const int AccountNumber_MaxLength = 8;

  /// <summary>
  /// The value of the ACCOUNT_NUMBER attribute.
  /// A billing number for the cost of test result to a particular CSE office.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = AccountNumber_MaxLength)]
  public string AccountNumber
  {
    get => accountNumber ?? "";
    set => accountNumber =
      TrimEnd(Substring(value, 1, AccountNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the AccountNumber attribute.</summary>
  [JsonPropertyName("accountNumber")]
  [Computed]
  public string AccountNumber_Json
  {
    get => NullIf(AccountNumber, "");
    set => AccountNumber = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// User ID or Program ID responsible for creation of the occurrence.
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
  /// The value of the CREATED_TIMESTAMP attribute.
  /// Timestamp of creation of the occurrence.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 3, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// User ID or Program ID responsible for the last update of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy ?? "";
    set => lastUpdatedBy =
      TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength));
  }

  /// <summary>
  /// The json value of the LastUpdatedBy attribute.</summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Computed]
  public string LastUpdatedBy_Json
  {
    get => NullIf(LastUpdatedBy, "");
    set => LastUpdatedBy = value;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TIMESTAMP attribute.
  /// Timestamp of last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTimestamp")]
  [Member(Index = 5, Type = MemberType.Timestamp)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  /// <summary>Length of the ROLE_CODE attribute.</summary>
  public const int OspRoleCode_MaxLength = 2;

  /// <summary>
  /// The value of the ROLE_CODE attribute.
  /// This is the job title or role the person is fulfilling at or for a 
  /// particular location.
  /// A short representation of the name for the purpose of quick 
  /// identification.
  /// Use Set Mnemonics
  /// </summary>
  [JsonPropertyName("ospRoleCode")]
  [Member(Index = 6, Type = MemberType.Char, Length = OspRoleCode_MaxLength, Optional
    = true)]
  public string OspRoleCode
  {
    get => ospRoleCode;
    set => ospRoleCode = value != null
      ? TrimEnd(Substring(value, 1, OspRoleCode_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// * Draft *
  /// The date upon which this occurrence of the entity is activated by the 
  /// system.  An effective date allows the entity to be entered into the system
  /// with a future date.  The occurrence of the entity will &quot;take effect
  /// &quot; on the effective date.
  /// </summary>
  [JsonPropertyName("ospEffectiveDate")]
  [Member(Index = 7, Type = MemberType.Date, Optional = true)]
  public DateTime? OspEffectiveDate
  {
    get => ospEffectiveDate;
    set => ospEffectiveDate = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_ID attribute.
  /// A unique sequential number (See CONTROL_TABLE) that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("offGeneratedId")]
  [Member(Index = 8, Type = MemberType.Number, Length = 4, Optional = true)]
  public int? OffGeneratedId
  {
    get => offGeneratedId;
    set => offGeneratedId = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_ID attribute.
  /// A unique sequential number (See CONTROL_TABLE) that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("spdGeneratedId")]
  [Member(Index = 9, Type = MemberType.Number, Length = 5, Optional = true)]
  public int? SpdGeneratedId
  {
    get => spdGeneratedId;
    set => spdGeneratedId = value;
  }

  private string accountNumber;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private string ospRoleCode;
  private DateTime? ospEffectiveDate;
  private int? offGeneratedId;
  private int? spdGeneratedId;
}
