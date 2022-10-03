// The source file: APPOINTMENT, ID: 371430680, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVPLAN
/// Records office service provider appointment details for scheduled or 
/// rescheduled appointments with CSE Persons.  Details the reason of the
/// appointment.
/// DATA MODEL ALERT!!!!!
/// *	The relationship between APPOINTMENT and OFFICE SERVICE PROVIDER is not 
/// drawn.
/// each APPOINTMENT always schedules one OFFICE SERVICE PROVIDER
/// each OFFICE SERVICE PROVIDER sometimes is scheduled via one or more 
/// APPOINTMENT
/// </summary>
[Serializable]
public partial class Appointment: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public Appointment()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public Appointment(Appointment that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new Appointment Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(Appointment that)
  {
    base.Assign(that);
    reasonCode = that.reasonCode;
    type1 = that.type1;
    result = that.result;
    date = that.date;
    time = that.time;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    croId = that.croId;
    croType = that.croType;
    cspNumber = that.cspNumber;
    casNumber = that.casNumber;
    appTstamp = that.appTstamp;
    infId = that.infId;
    ospDate = that.ospDate;
    ospRoleCode = that.ospRoleCode;
    offId = that.offId;
    spdId = that.spdId;
  }

  /// <summary>Length of the REASON_CODE attribute.</summary>
  public const int ReasonCode_MaxLength = 1;

  /// <summary>
  /// The value of the REASON_CODE attribute.
  /// The Reason Code specifies the reason for which the appointment has been 
  /// setup.
  /// Examples:
  /// 	L = locate
  /// 	P = paternity
  /// 	H = hearing
  /// 	O = other
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

  /// <summary>Length of the TYPE attribute.</summary>
  public const int Type1_MaxLength = 1;

  /// <summary>
  /// The value of the TYPE attribute.
  /// Type classifies the appointment as:
  /// 	New
  /// 	Rescheduled
  /// 	Activity (an infrastructure record is created so that narrative may be 
  /// included in the case diary regarding the appointment.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = Type1_MaxLength)]
  public string Type1
  {
    get => type1 ?? "";
    set => type1 = TrimEnd(Substring(value, 1, Type1_MaxLength));
  }

  /// <summary>
  /// The json value of the Type1 attribute.</summary>
  [JsonPropertyName("type1")]
  [Computed]
  public string Type1_Json
  {
    get => NullIf(Type1, "");
    set => Type1 = value;
  }

  /// <summary>Length of the RESULT attribute.</summary>
  public const int Result_MaxLength = 1;

  /// <summary>
  /// The value of the RESULT attribute.
  /// Result identifies the disposition of the appointment.
  /// Examples:
  /// 	R = reschedule appointment
  /// 	C = cancel appointment
  /// 	U = user cancelled appointment
  /// 	H = appointment conducted
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = Result_MaxLength)]
  public string Result
  {
    get => result ?? "";
    set => result = TrimEnd(Substring(value, 1, Result_MaxLength));
  }

  /// <summary>
  /// The json value of the Result attribute.</summary>
  [JsonPropertyName("result")]
  [Computed]
  public string Result_Json
  {
    get => NullIf(Result, "");
    set => Result = value;
  }

  /// <summary>
  /// The value of the DATE attribute.
  /// Date identifies the scheduled appointment date.
  /// </summary>
  [JsonPropertyName("date")]
  [Member(Index = 4, Type = MemberType.Date)]
  public DateTime? Date
  {
    get => date;
    set => date = value;
  }

  /// <summary>
  /// The value of the TIME attribute.
  /// Time identifies the scheduled appointment time.
  /// </summary>
  [JsonPropertyName("time")]
  [Member(Index = 5, Type = MemberType.Time)]
  public TimeSpan Time
  {
    get => time;
    set => time = value;
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

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// The unique identifier.  System generated using a 3-digit random generator
  /// </summary>
  [JsonPropertyName("croId")]
  [Member(Index = 10, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? CroId
  {
    get => croId;
    set => croId = value;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int CroType_MaxLength = 2;

  /// <summary>
  /// The value of the TYPE attribute.
  /// The type of role played by a CSE Person in a given case.
  /// </summary>
  [JsonPropertyName("croType")]
  [Member(Index = 11, Type = MemberType.Char, Length = CroType_MaxLength, Optional
    = true)]
  [Value(null)]
  [Value("MO")]
  [Value("FA")]
  [Value("CH")]
  [Value("AR")]
  [Value("AP")]
  public string CroType
  {
    get => croType;
    set => croType = value != null
      ? TrimEnd(Substring(value, 1, CroType_MaxLength)) : null;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonPropertyName("cspNumber")]
  [Member(Index = 12, Type = MemberType.Char, Length = CspNumber_MaxLength, Optional
    = true)]
  public string CspNumber
  {
    get => cspNumber;
    set => cspNumber = value != null
      ? TrimEnd(Substring(value, 1, CspNumber_MaxLength)) : null;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CasNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// A unique number assigned to a request or referral.  Becomes the Case 
  /// number.
  /// </summary>
  [JsonPropertyName("casNumber")]
  [Member(Index = 13, Type = MemberType.Char, Length = CasNumber_MaxLength, Optional
    = true)]
  public string CasNumber
  {
    get => casNumber;
    set => casNumber = value != null
      ? TrimEnd(Substring(value, 1, CasNumber_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the CREATED_TIMESTAMP attribute.
  /// Timestamp of creation of the occurrence.
  /// 	
  /// </summary>
  [JsonPropertyName("appTstamp")]
  [Member(Index = 14, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? AppTstamp
  {
    get => appTstamp;
    set => appTstamp = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("infId")]
  [Member(Index = 15, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? InfId
  {
    get => infId;
    set => infId = value;
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
  [Member(Index = 16, Type = MemberType.Date, Optional = true)]
  public DateTime? OspDate
  {
    get => ospDate;
    set => ospDate = value;
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
  [Member(Index = 17, Type = MemberType.Char, Length = OspRoleCode_MaxLength, Optional
    = true)]
  public string OspRoleCode
  {
    get => ospRoleCode;
    set => ospRoleCode = value != null
      ? TrimEnd(Substring(value, 1, OspRoleCode_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_ID attribute.
  /// A unique sequential number (See CONTROL_TABLE) that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("offId")]
  [Member(Index = 18, Type = MemberType.Number, Length = 4, Optional = true)]
  public int? OffId
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
  [Member(Index = 19, Type = MemberType.Number, Length = 5, Optional = true)]
  public int? SpdId
  {
    get => spdId;
    set => spdId = value;
  }

  private string reasonCode;
  private string type1;
  private string result;
  private DateTime? date;
  private TimeSpan time;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private int? croId;
  private string croType;
  private string cspNumber;
  private string casNumber;
  private DateTime? appTstamp;
  private int? infId;
  private DateTime? ospDate;
  private string ospRoleCode;
  private int? offId;
  private int? spdId;
}
