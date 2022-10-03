// The source file: JOB_RUN, ID: 371436700, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// This entity will contain start and stop time  information about each 
/// instance of a job run.
/// </summary>
[Serializable]
public partial class JobRun: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public JobRun()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public JobRun(JobRun that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new JobRun Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(JobRun that)
  {
    base.Assign(that);
    startTimestamp = that.startTimestamp;
    endTimestamp = that.endTimestamp;
    zdelUserId = that.zdelUserId;
    zdelPersonNumber = that.zdelPersonNumber;
    zdelLegActionId = that.zdelLegActionId;
    status = that.status;
    printerId = that.printerId;
    outputType = that.outputType;
    errorMsg = that.errorMsg;
    emailAddress = that.emailAddress;
    parmInfo = that.parmInfo;
    systemGenId = that.systemGenId;
    spdSrvcPrvderId = that.spdSrvcPrvderId;
    jobName = that.jobName;
  }

  /// <summary>
  /// The value of the START_TIMESTAMP attribute.
  /// The date and time that the job was executed.
  /// </summary>
  [JsonPropertyName("startTimestamp")]
  [Member(Index = 1, Type = MemberType.Timestamp)]
  public DateTime? StartTimestamp
  {
    get => startTimestamp;
    set => startTimestamp = value;
  }

  /// <summary>
  /// The value of the END_TIMESTAMP attribute.
  /// The date and time that the job ended.
  /// </summary>
  [JsonPropertyName("endTimestamp")]
  [Member(Index = 2, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? EndTimestamp
  {
    get => endTimestamp;
    set => endTimestamp = value;
  }

  /// <summary>Length of the ZDEL_USER_ID attribute.</summary>
  public const int ZdelUserId_MaxLength = 8;

  /// <summary>
  /// The value of the ZDEL_USER_ID attribute.
  /// This is for the user Identification.
  /// </summary>
  [JsonPropertyName("zdelUserId")]
  [Member(Index = 3, Type = MemberType.Char, Length = ZdelUserId_MaxLength, Optional
    = true)]
  public string ZdelUserId
  {
    get => zdelUserId;
    set => zdelUserId = value != null
      ? TrimEnd(Substring(value, 1, ZdelUserId_MaxLength)) : null;
  }

  /// <summary>Length of the ZDEL_PERSON_NUMBER attribute.</summary>
  public const int ZdelPersonNumber_MaxLength = 10;

  /// <summary>
  /// The value of the ZDEL_PERSON_NUMBER attribute.
  /// The AP PERSON NBR submitted for the job.
  /// </summary>
  [JsonPropertyName("zdelPersonNumber")]
  [Member(Index = 4, Type = MemberType.Char, Length
    = ZdelPersonNumber_MaxLength, Optional = true)]
  public string ZdelPersonNumber
  {
    get => zdelPersonNumber;
    set => zdelPersonNumber = value != null
      ? TrimEnd(Substring(value, 1, ZdelPersonNumber_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the ZDEL_LEG_ACTION_ID attribute.
  /// The LEGAL ACTION submitted for the job.
  /// </summary>
  [JsonPropertyName("zdelLegActionId")]
  [Member(Index = 5, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? ZdelLegActionId
  {
    get => zdelLegActionId;
    set => zdelLegActionId = value;
  }

  /// <summary>Length of the STATUS attribute.</summary>
  public const int Status_MaxLength = 10;

  /// <summary>
  /// The value of the STATUS attribute.
  /// The STATUS of the submitted job.
  /// 
  /// Values: R-running C-Complete.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = Status_MaxLength)]
  public string Status
  {
    get => status ?? "";
    set => status = TrimEnd(Substring(value, 1, Status_MaxLength));
  }

  /// <summary>
  /// The json value of the Status attribute.</summary>
  [JsonPropertyName("status")]
  [Computed]
  public string Status_Json
  {
    get => NullIf(Status, "");
    set => Status = value;
  }

  /// <summary>Length of the PRINTER_ID attribute.</summary>
  public const int PrinterId_MaxLength = 8;

  /// <summary>
  /// The value of the PRINTER_ID attribute.
  /// Denormalized value for Printer_ID.
  /// </summary>
  [JsonPropertyName("printerId")]
  [Member(Index = 7, Type = MemberType.Char, Length = PrinterId_MaxLength, Optional
    = true)]
  public string PrinterId
  {
    get => printerId;
    set => printerId = value != null
      ? TrimEnd(Substring(value, 1, PrinterId_MaxLength)) : null;
  }

  /// <summary>Length of the OUTPUT_TYPE attribute.</summary>
  public const int OutputType_MaxLength = 10;

  /// <summary>
  /// The value of the OUTPUT_TYPE attribute.
  /// Defines the type of output in which the report will be produced.
  /// 
  /// Example:
  /// 
  /// Printer Format will be a hardcopy report to a sent to a printer defined by
  /// Printer_Id.
  /// Wordpfct
  /// Format will be WordPerfect. Reoport will be emailed to the user based on
  /// the Email_Address.                     Html Format will be Html. Report
  /// will be emailed to the user based on the Email_Address.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = OutputType_MaxLength)]
  public string OutputType
  {
    get => outputType ?? "";
    set => outputType = TrimEnd(Substring(value, 1, OutputType_MaxLength));
  }

  /// <summary>
  /// The json value of the OutputType attribute.</summary>
  [JsonPropertyName("outputType")]
  [Computed]
  public string OutputType_Json
  {
    get => NullIf(OutputType, "");
    set => OutputType = value;
  }

  /// <summary>Length of the ERROR_MSG attribute.</summary>
  public const int ErrorMsg_MaxLength = 80;

  /// <summary>
  /// The value of the ERROR_MSG attribute.
  /// Contains the error message for the job if a failure occurs.
  /// </summary>
  [JsonPropertyName("errorMsg")]
  [Member(Index = 9, Type = MemberType.Char, Length = ErrorMsg_MaxLength, Optional
    = true)]
  public string ErrorMsg
  {
    get => errorMsg;
    set => errorMsg = value != null
      ? TrimEnd(Substring(value, 1, ErrorMsg_MaxLength)) : null;
  }

  /// <summary>Length of the EMAIL_ADDRESS attribute.</summary>
  public const int EmailAddress_MaxLength = 50;

  /// <summary>
  /// The value of the EMAIL_ADDRESS attribute.
  /// Represents the denormalized value of the email address from the Service 
  /// Provider entity.
  /// </summary>
  [JsonPropertyName("emailAddress")]
  [Member(Index = 10, Type = MemberType.Char, Length = EmailAddress_MaxLength, Optional
    = true)]
  public string EmailAddress
  {
    get => emailAddress;
    set => emailAddress = value != null
      ? TrimEnd(Substring(value, 1, EmailAddress_MaxLength)) : null;
  }

  /// <summary>Length of the PARM_INFO attribute.</summary>
  public const int ParmInfo_MaxLength = 240;

  /// <summary>
  /// The value of the PARM_INFO attribute.
  /// Represents the parameter information to be used by the batch process to 
  /// control the functionaliyty of the report. Examples are Obligor, Court
  /// Order Number, Service Provider, various filters, etc. This data will be
  /// accessed directly by each report program.
  /// </summary>
  [JsonPropertyName("parmInfo")]
  [Member(Index = 11, Type = MemberType.Varchar, Length = ParmInfo_MaxLength, Optional
    = true)]
  public string ParmInfo
  {
    get => parmInfo;
    set => parmInfo = value != null
      ? Substring(value, 1, ParmInfo_MaxLength) : null;
  }

  /// <summary>
  /// The value of the SYSTEM_GEN_ID attribute.
  /// Defines an unique ID for the Job Run row.
  /// </summary>
  [JsonPropertyName("systemGenId")]
  [DefaultValue(0)]
  [Member(Index = 12, Type = MemberType.Number, Length = 9)]
  public int SystemGenId
  {
    get => systemGenId;
    set => systemGenId = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_ID attribute.
  /// A unique sequential number (See CONTROL_TABLE) that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("spdSrvcPrvderId")]
  [Member(Index = 13, Type = MemberType.Number, Length = 5, Optional = true)]
  public int? SpdSrvcPrvderId
  {
    get => spdSrvcPrvderId;
    set => spdSrvcPrvderId = value;
  }

  /// <summary>Length of the NAME attribute.</summary>
  public const int JobName_MaxLength = 8;

  /// <summary>
  /// The value of the NAME attribute.
  /// The name of the job (from the job card statement in the JCL) that is to be
  /// run.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 14, Type = MemberType.Char, Length = JobName_MaxLength)]
  public string JobName
  {
    get => jobName ?? "";
    set => jobName = TrimEnd(Substring(value, 1, JobName_MaxLength));
  }

  /// <summary>
  /// The json value of the JobName attribute.</summary>
  [JsonPropertyName("jobName")]
  [Computed]
  public string JobName_Json
  {
    get => NullIf(JobName, "");
    set => JobName = value;
  }

  private DateTime? startTimestamp;
  private DateTime? endTimestamp;
  private string zdelUserId;
  private string zdelPersonNumber;
  private int? zdelLegActionId;
  private string status;
  private string printerId;
  private string outputType;
  private string errorMsg;
  private string emailAddress;
  private string parmInfo;
  private int systemGenId;
  private int? spdSrvcPrvderId;
  private string jobName;
}
