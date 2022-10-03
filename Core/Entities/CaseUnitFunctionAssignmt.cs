﻿// The source file: CASE_UNIT_FUNCTION_ASSIGNMT, ID: 371431266, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVPLAN	
/// Case Units are not assignedin the manner in which a Case is assigned to an 
/// office service provider.
/// Case Units may be assigned via the case unit state function (locate, 
/// paternity, obligation establishment, enforcement).  The association between
/// the case unit and the assigned function are maintained through Case Unit
/// Function Assignments
/// DATA MODEL ALERT!!!!
/// *	The relationship between OFFICE SERVICE PROVIDER and CASE UNIT ASSIGNMENT 
/// is not drawn.
/// each OFFICE SERVICE PROVIDER sometimes is assigned to one or more CASE UNIT 
/// ASSIGNMENT
/// each CASE UNIT ASSIGNMENT always assigns one OFFICE SERVICE PROVIDER
/// *	Both relationships should be a part of the identifier.
/// </summary>
[Serializable]
public partial class CaseUnitFunctionAssignmt: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public CaseUnitFunctionAssignmt()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public CaseUnitFunctionAssignmt(CaseUnitFunctionAssignmt that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new CaseUnitFunctionAssignmt Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(CaseUnitFunctionAssignmt that)
  {
    base.Assign(that);
    reasonCode = that.reasonCode;
    overrideInd = that.overrideInd;
    effectiveDate = that.effectiveDate;
    discontinueDate = that.discontinueDate;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    function = that.function;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    casNo = that.casNo;
    csuNo = that.csuNo;
    ospDate = that.ospDate;
    ospCode = that.ospCode;
    offId = that.offId;
    spdId = that.spdId;
  }

  /// <summary>Length of the REASON_CODE attribute.</summary>
  public const int ReasonCode_MaxLength = 3;

  /// <summary>
  /// The value of the REASON_CODE attribute.
  /// Specifies the business reason for the assignment.
  /// For example:
  /// 	RSP denotes Responsibility
  ///         INF denotes Informational.
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

  /// <summary>Length of the OVERRIDE_IND attribute.</summary>
  public const int OverrideInd_MaxLength = 1;

  /// <summary>
  /// The value of the OVERRIDE_IND attribute.
  /// This switch allows an office service provider to flag assignments as not 
  /// available for reassignment.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = OverrideInd_MaxLength)]
  public string OverrideInd
  {
    get => overrideInd ?? "";
    set => overrideInd = TrimEnd(Substring(value, 1, OverrideInd_MaxLength));
  }

  /// <summary>
  /// The json value of the OverrideInd attribute.</summary>
  [JsonPropertyName("overrideInd")]
  [Computed]
  public string OverrideInd_Json
  {
    get => NullIf(OverrideInd, "");
    set => OverrideInd = value;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// The date upon which this occurrence of the entity is activated by the 
  /// system.  An effective date allows the entity to be entered into the system
  /// with a future date.  The occurrence of the entity will &quot;take effect
  /// &quot; on the effective date.
  /// </summary>
  [JsonPropertyName("effectiveDate")]
  [Member(Index = 3, Type = MemberType.Date)]
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
  [Member(Index = 4, Type = MemberType.Date, Optional = true)]
  public DateTime? DiscontinueDate
  {
    get => discontinueDate;
    set => discontinueDate = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// USER ID or Program ID responsible for creation of the occurrence.
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
  /// The value of the CREATED_TIMESTAMP attribute.
  /// Timestamp of creation of the occurrence.
  /// 	
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 6, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>Length of the FUNCTION attribute.</summary>
  public const int Function_MaxLength = 3;

  /// <summary>
  /// The value of the FUNCTION attribute.
  /// Specifies the function being assigned for the Case Unit.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = Function_MaxLength)]
  public string Function
  {
    get => function ?? "";
    set => function = TrimEnd(Substring(value, 1, Function_MaxLength));
  }

  /// <summary>
  /// The json value of the Function attribute.</summary>
  [JsonPropertyName("function")]
  [Computed]
  public string Function_Json
  {
    get => NullIf(Function, "");
    set => Function = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// User ID or Program ID responsible for the last update of the occurrence.
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
  /// The value of the LAST_UPDATED_TIMESTAMP attribute.
  /// Timestamp of last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTimestamp")]
  [Member(Index = 9, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CasNo_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// A unique number assigned to a request or referral.  Becomes the Case 
  /// number.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = CasNo_MaxLength)]
  public string CasNo
  {
    get => casNo ?? "";
    set => casNo = TrimEnd(Substring(value, 1, CasNo_MaxLength));
  }

  /// <summary>
  /// The json value of the CasNo attribute.</summary>
  [JsonPropertyName("casNo")]
  [Computed]
  public string CasNo_Json
  {
    get => NullIf(CasNo, "");
    set => CasNo = value;
  }

  /// <summary>
  /// The value of the CU_NUMBER attribute.
  /// The extension to the Case Number which uniquely identifies this case unit.
  /// </summary>
  [JsonPropertyName("csuNo")]
  [DefaultValue(0)]
  [Member(Index = 11, Type = MemberType.Number, Length = 3)]
  public int CsuNo
  {
    get => csuNo;
    set => csuNo = value;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// * Draft *
  /// The date upon which this occurrence of the entity is activated by the 
  /// system.  An effective date allows the entity to be entered into the system
  /// with a future date.  The occurrence of the entity will &quot;take effect
  /// &quot; on the effective date.
  /// </summary>
  [JsonPropertyName("ospDate")]
  [Member(Index = 12, Type = MemberType.Date)]
  public DateTime? OspDate
  {
    get => ospDate;
    set => ospDate = value;
  }

  /// <summary>Length of the ROLE_CODE attribute.</summary>
  public const int OspCode_MaxLength = 2;

  /// <summary>
  /// The value of the ROLE_CODE attribute.
  /// This is the job title or role the person is fulfilling at or for a 
  /// particular location.
  /// A short representation of the name for the purpose of quick 
  /// identification.
  /// Use Set Mnemonics
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length = OspCode_MaxLength)]
  public string OspCode
  {
    get => ospCode ?? "";
    set => ospCode = TrimEnd(Substring(value, 1, OspCode_MaxLength));
  }

  /// <summary>
  /// The json value of the OspCode attribute.</summary>
  [JsonPropertyName("ospCode")]
  [Computed]
  public string OspCode_Json
  {
    get => NullIf(OspCode, "");
    set => OspCode = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_ID attribute.
  /// A unique sequential number (See CONTROL_TABLE) that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("offId")]
  [DefaultValue(0)]
  [Member(Index = 14, Type = MemberType.Number, Length = 4)]
  public int OffId
  {
    get => offId;
    set => offId = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_ID attribute.
  /// A unique sequential number (See CONTROL_TABLE) that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("spdId")]
  [DefaultValue(0)]
  [Member(Index = 15, Type = MemberType.Number, Length = 5)]
  public int SpdId
  {
    get => spdId;
    set => spdId = value;
  }

  private string reasonCode;
  private string overrideInd;
  private DateTime? effectiveDate;
  private DateTime? discontinueDate;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string function;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private string casNo;
  private int csuNo;
  private DateTime? ospDate;
  private string ospCode;
  private int offId;
  private int spdId;
}
