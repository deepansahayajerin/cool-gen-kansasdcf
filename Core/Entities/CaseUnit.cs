// The source file: CASE_UNIT, ID: 371427732, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVPLAN
/// A mutually exclusive combination of an AR, Child, and an Absent Parent (or 
/// alleged AP).
/// Life cycle or function are really indeteminable for an entire case.  In 
/// orderto determine the function or to follow the lifecycle, a case must be
/// broken down into it's most basic unit.  The basic building blocks (or units)
/// of a case are the different combinations or AR, Child, and AP.  Since many
/// things can be going on in a case at once and each child can be in different
/// functions, the life cycle state and function for each unit can be different.
/// Only one Absent Parent per case unit.
/// If the AR changes on the case, then new case units are opened for the new AR
/// and the old ones are closed.
/// The CSE Person composition of a case unit NEVER changes.
/// DATA MODEL ALERT!!!!!
/// The following relationships have NOT been drawn yet:
/// * CASE UNIT to CASE
/// One CASE UNIT always BREAKS DOWN one CASE
/// * CASE UNIT to OFFICE SERVICE PROVIDER
/// One CASE UNIT always IS ASSIGNED TO one OFFICE SERVICE PROVIDER.
/// * CASE UNIT to CSE PERSON
/// One CASE UNIT always HAS AS ABSENT PARENT one CSE PERSON.
/// Add the relationship to CASE to the Identifier.
/// One CASE UNIT always HAS AS CHILD one CSE PERSON
/// </summary>
[Serializable]
public partial class CaseUnit: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public CaseUnit()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public CaseUnit(CaseUnit that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new CaseUnit Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(CaseUnit that)
  {
    base.Assign(that);
    cuNumber = that.cuNumber;
    state = that.state;
    startDate = that.startDate;
    closureDate = that.closureDate;
    closureReasonCode = that.closureReasonCode;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    casNo = that.casNo;
    cspNoChild = that.cspNoChild;
    cspNoAp = that.cspNoAp;
    cspNoAr = that.cspNoAr;
  }

  /// <summary>
  /// The value of the CU_NUMBER attribute.
  /// The extension to the Case Number which uniquely identifies this case unit.
  /// </summary>
  [JsonPropertyName("cuNumber")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 3)]
  public int CuNumber
  {
    get => cuNumber;
    set => cuNumber = value;
  }

  /// <summary>Length of the STATE attribute.</summary>
  public const int State_MaxLength = 5;

  /// <summary>
  /// The value of the STATE attribute.
  /// State specifies the lifecycle state properties for a case unit.
  /// State expresses property values for:
  /// Function	(L/P/O/E)
  /// Service Type	(F/L/E)
  /// Is Located	(Y/N)
  /// Is an AP	(Y/N/U)
  /// Is obligated	(Y/N/U)
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = State_MaxLength)]
  public string State
  {
    get => state ?? "";
    set => state = TrimEnd(Substring(value, 1, State_MaxLength));
  }

  /// <summary>
  /// The json value of the State attribute.</summary>
  [JsonPropertyName("state")]
  [Computed]
  public string State_Json
  {
    get => NullIf(State, "");
    set => State = value;
  }

  /// <summary>
  /// The value of the START_DATE attribute.
  /// Start Date indicates the last date on which the Case Unit was activated.
  /// </summary>
  [JsonPropertyName("startDate")]
  [Member(Index = 3, Type = MemberType.Date)]
  public DateTime? StartDate
  {
    get => startDate;
    set => startDate = value;
  }

  /// <summary>
  /// The value of the CLOSURE_DATE attribute.
  /// Closure Date identifies the date on which the case unit last became 
  /// inactive.
  /// </summary>
  [JsonPropertyName("closureDate")]
  [Member(Index = 4, Type = MemberType.Date, Optional = true)]
  public DateTime? ClosureDate
  {
    get => closureDate;
    set => closureDate = value;
  }

  /// <summary>Length of the CLOSURE_REASON_CODE attribute.</summary>
  public const int ClosureReasonCode_MaxLength = 2;

  /// <summary>
  /// The value of the CLOSURE_REASON_CODE attribute.
  /// Closure Reason Code specifies the reason for placing the case unit in a 
  /// discontinued status.
  /// Closure Reason Codes are validated in the Code Value table.
  /// </summary>
  [JsonPropertyName("closureReasonCode")]
  [Member(Index = 5, Type = MemberType.Char, Length
    = ClosureReasonCode_MaxLength, Optional = true)]
  public string ClosureReasonCode
  {
    get => closureReasonCode;
    set => closureReasonCode = value != null
      ? TrimEnd(Substring(value, 1, ClosureReasonCode_MaxLength)) : null;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// USER ID or Program ID responsible for creation of the occurrence.
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
  /// The value of the CREATED_TIMESTAMP attribute.
  /// Timestamp of creation of the occurrence.
  /// 	
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 7, Type = MemberType.Timestamp)]
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

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspNoChild_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonPropertyName("cspNoChild")]
  [Member(Index = 11, Type = MemberType.Char, Length = CspNoChild_MaxLength, Optional
    = true)]
  public string CspNoChild
  {
    get => cspNoChild;
    set => cspNoChild = value != null
      ? TrimEnd(Substring(value, 1, CspNoChild_MaxLength)) : null;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspNoAp_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonPropertyName("cspNoAp")]
  [Member(Index = 12, Type = MemberType.Char, Length = CspNoAp_MaxLength, Optional
    = true)]
  public string CspNoAp
  {
    get => cspNoAp;
    set => cspNoAp = value != null
      ? TrimEnd(Substring(value, 1, CspNoAp_MaxLength)) : null;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspNoAr_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonPropertyName("cspNoAr")]
  [Member(Index = 13, Type = MemberType.Char, Length = CspNoAr_MaxLength, Optional
    = true)]
  public string CspNoAr
  {
    get => cspNoAr;
    set => cspNoAr = value != null
      ? TrimEnd(Substring(value, 1, CspNoAr_MaxLength)) : null;
  }

  private int cuNumber;
  private string state;
  private DateTime? startDate;
  private DateTime? closureDate;
  private string closureReasonCode;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private string casNo;
  private string cspNoChild;
  private string cspNoAp;
  private string cspNoAr;
}
