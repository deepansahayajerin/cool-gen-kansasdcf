// The source file: PROFILE_AUTHORIZATION, ID: 371422655, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SECUR
/// THIS WILL BE THE MANY TO MANY RESOLVER TABLE BETWEEN SERVICE PROVIDER AND 
/// ACTION.  IT WILL CONTAIN THE OCCURANCES OF WHAT ACTIONS A SERVICE PROVIDER
/// IS AUTHORIZED FOR A GIVEN SCREEN
/// </summary>
[Serializable]
public partial class ProfileAuthorization: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ProfileAuthorization()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ProfileAuthorization(ProfileAuthorization that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ProfileAuthorization Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ProfileAuthorization that)
  {
    base.Assign(that);
    createdTimestamp = that.createdTimestamp;
    activeInd = that.activeInd;
    activeCount = that.activeCount;
    createdBy = that.createdBy;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTstamp = that.lastUpdatedTstamp;
    caseAuth = that.caseAuth;
    legalActionAuth = that.legalActionAuth;
    fkProName = that.fkProName;
    fkCmdValue = that.fkCmdValue;
    fkTrnScreenid = that.fkTrnScreenid;
    fkTrnTrancode = that.fkTrnTrancode;
  }

  /// <summary>
  /// The value of the CREATED_TIMESTAMP attribute.
  /// The date and time when the profile authorization is created (date is 
  /// inclusive).
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 1, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>Length of the ACTIVE_IND attribute.</summary>
  public const int ActiveInd_MaxLength = 1;

  /// <summary>
  /// The value of the ACTIVE_IND attribute.
  /// THIS INDICATES WETHER OR NOT THE COMMAND IS ACTIVE .
  /// 	Y - ACTIVE
  /// 	N - NOT ACTIVE	
  /// </summary>
  [JsonPropertyName("activeInd")]
  [Member(Index = 2, Type = MemberType.Char, Length = ActiveInd_MaxLength, Optional
    = true)]
  public string ActiveInd
  {
    get => activeInd;
    set => activeInd = value != null
      ? TrimEnd(Substring(value, 1, ActiveInd_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the ACTIVE_COUNT attribute.
  /// INDICATES THE COUNT OF BATCH PROCEDURES WHICH HAVE SET THE ACTIVE 
  /// INDICATOR TO N.
  /// </summary>
  [JsonPropertyName("activeCount")]
  [Member(Index = 3, Type = MemberType.Number, Length = 5, Optional = true)]
  public int? ActiveCount
  {
    get => activeCount;
    set => activeCount = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The signon of the person or program that created the occurrance of the 
  /// entity.
  /// </summary>
  [JsonPropertyName("createdBy")]
  [Member(Index = 4, Type = MemberType.Char, Length = CreatedBy_MaxLength, Optional
    = true)]
  public string CreatedBy
  {
    get => createdBy;
    set => createdBy = value != null
      ? TrimEnd(Substring(value, 1, CreatedBy_MaxLength)) : null;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// The signon of the person or program that last updated the occurrance of 
  /// the entity.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 5, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength, Optional
    = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TSTAMP attribute.
  /// The date and time that the occurrance of the entity was last updated.
  /// </summary>
  [JsonPropertyName("lastUpdatedTstamp")]
  [Member(Index = 6, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTstamp
  {
    get => lastUpdatedTstamp;
    set => lastUpdatedTstamp = value;
  }

  /// <summary>Length of the CASE_AUTH attribute.</summary>
  public const int CaseAuth_MaxLength = 1;

  /// <summary>
  /// The value of the CASE_AUTH attribute.
  /// This is a flag that indicates whether a profile access for the associated 
  /// transaction and command by case assignment. The default value is N. The
  /// only other valid value is a Y.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = CaseAuth_MaxLength)]
  public string CaseAuth
  {
    get => caseAuth ?? "";
    set => caseAuth = TrimEnd(Substring(value, 1, CaseAuth_MaxLength));
  }

  /// <summary>
  /// The json value of the CaseAuth attribute.</summary>
  [JsonPropertyName("caseAuth")]
  [Computed]
  public string CaseAuth_Json
  {
    get => NullIf(CaseAuth, "");
    set => CaseAuth = value;
  }

  /// <summary>Length of the LEGAL_ACTION_AUTH attribute.</summary>
  public const int LegalActionAuth_MaxLength = 1;

  /// <summary>
  /// The value of the LEGAL_ACTION_AUTH attribute.
  /// This is a flag that indicates whether a profile restricts access for the 
  /// associated transaction and command by legal action assignment. The default
  /// value is N. The only other valid value is Y.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = LegalActionAuth_MaxLength)]
    
  public string LegalActionAuth
  {
    get => legalActionAuth ?? "";
    set => legalActionAuth =
      TrimEnd(Substring(value, 1, LegalActionAuth_MaxLength));
  }

  /// <summary>
  /// The json value of the LegalActionAuth attribute.</summary>
  [JsonPropertyName("legalActionAuth")]
  [Computed]
  public string LegalActionAuth_Json
  {
    get => NullIf(LegalActionAuth, "");
    set => LegalActionAuth = value;
  }

  /// <summary>Length of the NAME attribute.</summary>
  public const int FkProName_MaxLength = 10;

  /// <summary>
  /// The value of the NAME attribute.
  /// NAME OF THE GROUP
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = FkProName_MaxLength)]
  public string FkProName
  {
    get => fkProName ?? "";
    set => fkProName = TrimEnd(Substring(value, 1, FkProName_MaxLength));
  }

  /// <summary>
  /// The json value of the FkProName attribute.</summary>
  [JsonPropertyName("fkProName")]
  [Computed]
  public string FkProName_Json
  {
    get => NullIf(FkProName, "");
    set => FkProName = value;
  }

  /// <summary>Length of the VALUE attribute.</summary>
  public const int FkCmdValue_MaxLength = 8;

  /// <summary>
  /// The value of the VALUE attribute.
  /// command value available to a transcation(screen).
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = FkCmdValue_MaxLength)]
  public string FkCmdValue
  {
    get => fkCmdValue ?? "";
    set => fkCmdValue = TrimEnd(Substring(value, 1, FkCmdValue_MaxLength));
  }

  /// <summary>
  /// The json value of the FkCmdValue attribute.</summary>
  [JsonPropertyName("fkCmdValue")]
  [Computed]
  public string FkCmdValue_Json
  {
    get => NullIf(FkCmdValue, "");
    set => FkCmdValue = value;
  }

  /// <summary>Length of the SCREEN_ID attribute.</summary>
  public const int FkTrnScreenid_MaxLength = 4;

  /// <summary>
  /// The value of the SCREEN_ID attribute.
  /// the screen id for the transaction(procedure)
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length = FkTrnScreenid_MaxLength)]
  public string FkTrnScreenid
  {
    get => fkTrnScreenid ?? "";
    set => fkTrnScreenid =
      TrimEnd(Substring(value, 1, FkTrnScreenid_MaxLength));
  }

  /// <summary>
  /// The json value of the FkTrnScreenid attribute.</summary>
  [JsonPropertyName("fkTrnScreenid")]
  [Computed]
  public string FkTrnScreenid_Json
  {
    get => NullIf(FkTrnScreenid, "");
    set => FkTrnScreenid = value;
  }

  /// <summary>Length of the TRANCODE attribute.</summary>
  public const int FkTrnTrancode_MaxLength = 4;

  /// <summary>
  /// The value of the TRANCODE attribute.
  /// the related trancode for the screen id.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length = FkTrnTrancode_MaxLength)]
  public string FkTrnTrancode
  {
    get => fkTrnTrancode ?? "";
    set => fkTrnTrancode =
      TrimEnd(Substring(value, 1, FkTrnTrancode_MaxLength));
  }

  /// <summary>
  /// The json value of the FkTrnTrancode attribute.</summary>
  [JsonPropertyName("fkTrnTrancode")]
  [Computed]
  public string FkTrnTrancode_Json
  {
    get => NullIf(FkTrnTrancode, "");
    set => FkTrnTrancode = value;
  }

  private DateTime? createdTimestamp;
  private string activeInd;
  private int? activeCount;
  private string createdBy;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTstamp;
  private string caseAuth;
  private string legalActionAuth;
  private string fkProName;
  private string fkCmdValue;
  private string fkTrnScreenid;
  private string fkTrnTrancode;
}
