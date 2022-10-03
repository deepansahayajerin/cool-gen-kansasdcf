// The source file: CSE_ORGANIZATION_RELATIONSHIP, ID: 371432843, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVPLAN
/// A geographic partitioning of the State of Kansas, based on:
/// Division/Region
/// The 2 Divisions and their 7 current CSE regions:
/// (Div 1= Hayes,Silina,Wichita,
///  Div 2= NE, SE, KC, and Topeka).
/// Each area and their 12 area offices.
/// Also includes:
/// STATE/COUNTY relationship
/// STATE/DISTRICT relationship
/// EXCLUDES:
/// All Courts and offices.
/// Central office, Interstate Unit, and Receivables, CSE Field, CSE 
/// administration, Courts.  These are all in LOCATION and relate to STATE
/// ORGANIZATION RELATIONSHIP.
/// </summary>
[Serializable]
public partial class CseOrganizationRelationship: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public CseOrganizationRelationship()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public CseOrganizationRelationship(CseOrganizationRelationship that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new CseOrganizationRelationship Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(CseOrganizationRelationship that)
  {
    base.Assign(that);
    reasonCode = that.reasonCode;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    cogParentType = that.cogParentType;
    cogParentCode = that.cogParentCode;
    cogChildType = that.cogChildType;
    cogChildCode = that.cogChildCode;
  }

  /// <summary>Length of the REASON_CODE attribute.</summary>
  public const int ReasonCode_MaxLength = 2;

  /// <summary>
  /// The value of the REASON_CODE attribute.
  /// This is to document as well as help enforce integraty.  If a relationship 
  /// reason is selected to two occurances should be of the type that the reason
  /// would suggest.
  /// Ex. Reason code 'SD&quot; State to Division
  /// If SD is used, the system should not allow a state to be related to a 
  /// region.
  /// A short representation of the name for the purpose of quick 
  /// identification.
  /// Use Set Mnemonics
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = ReasonCode_MaxLength)]
  public string ReasonCode
  {
    get => reasonCode ?? "";
    set => reasonCode = TrimEnd(Substring(value, 1, ReasonCode_MaxLength));
  }

  /// <summary>
  /// The json value of the ReasonCode attribute.</summary>
  [JsonPropertyName("reasonCode")]
  [Computed]
  public string ReasonCode_Json
  {
    get => NullIf(ReasonCode, "");
    set => ReasonCode = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The user id or program id responsible for the creation of the occurrence.
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
  /// The timestamp of when the occurrence of the entity was created.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 3, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int CogParentType_MaxLength = 1;

  /// <summary>
  /// The value of the TYPE attribute.
  /// A short representation of the name for the purpose of quick 
  /// identification.
  /// Use Set Mnemonics
  /// REGION
  /// DIVISION
  /// COUNTY
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = CogParentType_MaxLength)]
  public string CogParentType
  {
    get => cogParentType ?? "";
    set => cogParentType =
      TrimEnd(Substring(value, 1, CogParentType_MaxLength));
  }

  /// <summary>
  /// The json value of the CogParentType attribute.</summary>
  [JsonPropertyName("cogParentType")]
  [Computed]
  public string CogParentType_Json
  {
    get => NullIf(CogParentType, "");
    set => CogParentType = value;
  }

  /// <summary>Length of the CODE attribute.</summary>
  public const int CogParentCode_MaxLength = 2;

  /// <summary>
  /// The value of the CODE attribute.
  /// Identifies the CSE organization.  It distinguishes one occurrence of the 
  /// entity type from another.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = CogParentCode_MaxLength)]
  public string CogParentCode
  {
    get => cogParentCode ?? "";
    set => cogParentCode =
      TrimEnd(Substring(value, 1, CogParentCode_MaxLength));
  }

  /// <summary>
  /// The json value of the CogParentCode attribute.</summary>
  [JsonPropertyName("cogParentCode")]
  [Computed]
  public string CogParentCode_Json
  {
    get => NullIf(CogParentCode, "");
    set => CogParentCode = value;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int CogChildType_MaxLength = 1;

  /// <summary>
  /// The value of the TYPE attribute.
  /// A short representation of the name for the purpose of quick 
  /// identification.
  /// Use Set Mnemonics
  /// REGION
  /// DIVISION
  /// COUNTY
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = CogChildType_MaxLength)]
  public string CogChildType
  {
    get => cogChildType ?? "";
    set => cogChildType = TrimEnd(Substring(value, 1, CogChildType_MaxLength));
  }

  /// <summary>
  /// The json value of the CogChildType attribute.</summary>
  [JsonPropertyName("cogChildType")]
  [Computed]
  public string CogChildType_Json
  {
    get => NullIf(CogChildType, "");
    set => CogChildType = value;
  }

  /// <summary>Length of the CODE attribute.</summary>
  public const int CogChildCode_MaxLength = 2;

  /// <summary>
  /// The value of the CODE attribute.
  /// Identifies the CSE organization.  It distinguishes one occurrence of the 
  /// entity type from another.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = CogChildCode_MaxLength)]
  public string CogChildCode
  {
    get => cogChildCode ?? "";
    set => cogChildCode = TrimEnd(Substring(value, 1, CogChildCode_MaxLength));
  }

  /// <summary>
  /// The json value of the CogChildCode attribute.</summary>
  [JsonPropertyName("cogChildCode")]
  [Computed]
  public string CogChildCode_Json
  {
    get => NullIf(CogChildCode, "");
    set => CogChildCode = value;
  }

  private string reasonCode;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string cogParentType;
  private string cogParentCode;
  private string cogChildType;
  private string cogChildCode;
}
