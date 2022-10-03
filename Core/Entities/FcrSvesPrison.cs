// The source file: FCR_SVES_PRISON, ID: 945065591, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

[Serializable]
public partial class FcrSvesPrison: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public FcrSvesPrison()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public FcrSvesPrison(FcrSvesPrison that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new FcrSvesPrison Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(FcrSvesPrison that)
  {
    base.Assign(that);
    seqNo = that.seqNo;
    prisonFacilityType = that.prisonFacilityType;
    prisonFacilityName = that.prisonFacilityName;
    prisonFacilityContactName = that.prisonFacilityContactName;
    prisonFacilityPhone = that.prisonFacilityPhone;
    prisonFacilityFaxNum = that.prisonFacilityFaxNum;
    prisonReportedSsn = that.prisonReportedSsn;
    confinementDate = that.confinementDate;
    releaseDate = that.releaseDate;
    reportDate = that.reportDate;
    prisonerReporterName = that.prisonerReporterName;
    prisonerIdNumber = that.prisonerIdNumber;
    prisonReportedSuffix = that.prisonReportedSuffix;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    fcgLSRspAgy = that.fcgLSRspAgy;
    fcgMemberId = that.fcgMemberId;
  }

  /// <summary>
  /// The value of the SEQ_NO attribute.
  /// This field contains a value to identify a unique Prison record.
  /// </summary>
  [JsonPropertyName("seqNo")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 2)]
  public int SeqNo
  {
    get => seqNo;
    set => seqNo = value;
  }

  /// <summary>Length of the PRISON_FACILITY_TYPE attribute.</summary>
  public const int PrisonFacilityType_MaxLength = 2;

  /// <summary>
  /// The value of the PRISON_FACILITY_TYPE attribute.
  /// This field contains one of the following codes:
  /// 01		State Prison
  /// 02		County Prison
  /// 03		Federal Correctional Institute
  /// 04		Mental Correctional Institute
  /// 05		Boot Camp
  /// 06		Medical Correctional Institute
  /// 07		Work Camp
  /// 08		Detention Center
  /// 09		Juvenile Detention Center
  /// </summary>
  [JsonPropertyName("prisonFacilityType")]
  [Member(Index = 2, Type = MemberType.Char, Length
    = PrisonFacilityType_MaxLength, Optional = true)]
  public string PrisonFacilityType
  {
    get => prisonFacilityType;
    set => prisonFacilityType = value != null
      ? TrimEnd(Substring(value, 1, PrisonFacilityType_MaxLength)) : null;
  }

  /// <summary>Length of the PRISON_FACILITY_NAME attribute.</summary>
  public const int PrisonFacilityName_MaxLength = 60;

  /// <summary>
  /// The value of the PRISON_FACILITY_NAME attribute.
  /// This field contains the name of the prison/facility.
  /// </summary>
  [JsonPropertyName("prisonFacilityName")]
  [Member(Index = 3, Type = MemberType.Varchar, Length
    = PrisonFacilityName_MaxLength, Optional = true)]
  public string PrisonFacilityName
  {
    get => prisonFacilityName;
    set => prisonFacilityName = value != null
      ? Substring(value, 1, PrisonFacilityName_MaxLength) : null;
  }

  /// <summary>Length of the PRISON_FACILITY_CONTACT_NAME attribute.</summary>
  public const int PrisonFacilityContactName_MaxLength = 35;

  /// <summary>
  /// The value of the PRISON_FACILITY_CONTACT_NAME attribute.
  /// This field contains the name of the contact person for the prison/
  /// facility.
  /// </summary>
  [JsonPropertyName("prisonFacilityContactName")]
  [Member(Index = 4, Type = MemberType.Varchar, Length
    = PrisonFacilityContactName_MaxLength, Optional = true)]
  public string PrisonFacilityContactName
  {
    get => prisonFacilityContactName;
    set => prisonFacilityContactName = value != null
      ? Substring(value, 1, PrisonFacilityContactName_MaxLength) : null;
  }

  /// <summary>Length of the PRISON_FACILITY_PHONE attribute.</summary>
  public const int PrisonFacilityPhone_MaxLength = 10;

  /// <summary>
  /// The value of the PRISON_FACILITY_PHONE attribute.
  /// This field contains the area code and telephone number of the prison/
  /// facility.
  /// If no number is available, this field contains spaces.
  /// </summary>
  [JsonPropertyName("prisonFacilityPhone")]
  [Member(Index = 5, Type = MemberType.Char, Length
    = PrisonFacilityPhone_MaxLength, Optional = true)]
  public string PrisonFacilityPhone
  {
    get => prisonFacilityPhone;
    set => prisonFacilityPhone = value != null
      ? TrimEnd(Substring(value, 1, PrisonFacilityPhone_MaxLength)) : null;
  }

  /// <summary>Length of the PRISON_FACILITY_FAX_NUM attribute.</summary>
  public const int PrisonFacilityFaxNum_MaxLength = 10;

  /// <summary>
  /// The value of the PRISON_FACILITY_FAX_NUM attribute.
  /// This field contains the name of the FAX number of the prison/facility.
  /// If no number is available, this field contains spaces.
  /// </summary>
  [JsonPropertyName("prisonFacilityFaxNum")]
  [Member(Index = 6, Type = MemberType.Char, Length
    = PrisonFacilityFaxNum_MaxLength, Optional = true)]
  public string PrisonFacilityFaxNum
  {
    get => prisonFacilityFaxNum;
    set => prisonFacilityFaxNum = value != null
      ? TrimEnd(Substring(value, 1, PrisonFacilityFaxNum_MaxLength)) : null;
  }

  /// <summary>Length of the PRISON_REPORTED_SSN attribute.</summary>
  public const int PrisonReportedSsn_MaxLength = 9;

  /// <summary>
  /// The value of the PRISON_REPORTED_SSN attribute.
  /// This field contains the SSN that was reported by the prison to SSA.
  /// </summary>
  [JsonPropertyName("prisonReportedSsn")]
  [Member(Index = 7, Type = MemberType.Char, Length
    = PrisonReportedSsn_MaxLength, Optional = true)]
  public string PrisonReportedSsn
  {
    get => prisonReportedSsn;
    set => prisonReportedSsn = value != null
      ? TrimEnd(Substring(value, 1, PrisonReportedSsn_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the CONFINEMENT_DATE attribute.
  /// This field will contain the date that the prisoner was confined to a 
  /// prison/facility.
  /// If this field was returned from SVES with an invalid date, this field will
  /// contain spaces.
  /// </summary>
  [JsonPropertyName("confinementDate")]
  [Member(Index = 8, Type = MemberType.Date, Optional = true)]
  public DateTime? ConfinementDate
  {
    get => confinementDate;
    set => confinementDate = value;
  }

  /// <summary>
  /// The value of the RELEASE_DATE attribute.
  /// This field will contain the date that the prisoner was released from the 
  /// prison/facility.
  ///  If this field was returned from SVES with an invalid date, this field 
  /// will contain spaces.
  /// </summary>
  [JsonPropertyName("releaseDate")]
  [Member(Index = 9, Type = MemberType.Date, Optional = true)]
  public DateTime? ReleaseDate
  {
    get => releaseDate;
    set => releaseDate = value;
  }

  /// <summary>
  /// The value of the REPORT_DATE attribute.
  ///  This field will contain the date that SSA received the prisoner 
  /// information.
  /// If this field was returned from SVES with an invalid date, this field will
  /// contain spaces.
  /// </summary>
  [JsonPropertyName("reportDate")]
  [Member(Index = 10, Type = MemberType.Date, Optional = true)]
  public DateTime? ReportDate
  {
    get => reportDate;
    set => reportDate = value;
  }

  /// <summary>Length of the PRISONER_REPORTER_NAME attribute.</summary>
  public const int PrisonerReporterName_MaxLength = 60;

  /// <summary>
  /// The value of the PRISONER_REPORTER_NAME attribute.
  /// This field will contain the name of the source that provided the prisoner 
  /// information to SSA.
  /// </summary>
  [JsonPropertyName("prisonerReporterName")]
  [Member(Index = 11, Type = MemberType.Varchar, Length
    = PrisonerReporterName_MaxLength, Optional = true)]
  public string PrisonerReporterName
  {
    get => prisonerReporterName;
    set => prisonerReporterName = value != null
      ? Substring(value, 1, PrisonerReporterName_MaxLength) : null;
  }

  /// <summary>Length of the PRISONER_ID_NUMBER attribute.</summary>
  public const int PrisonerIdNumber_MaxLength = 10;

  /// <summary>
  /// The value of the PRISONER_ID_NUMBER attribute.
  /// This field will contain the Prisoner Identification Number that was 
  /// returned on the Locate Response.
  ///  If no number is available, this field will contain spaces.
  /// </summary>
  [JsonPropertyName("prisonerIdNumber")]
  [Member(Index = 12, Type = MemberType.Char, Length
    = PrisonerIdNumber_MaxLength, Optional = true)]
  public string PrisonerIdNumber
  {
    get => prisonerIdNumber;
    set => prisonerIdNumber = value != null
      ? TrimEnd(Substring(value, 1, PrisonerIdNumber_MaxLength)) : null;
  }

  /// <summary>Length of the PRISON_REPORTED_SUFFIX attribute.</summary>
  public const int PrisonReportedSuffix_MaxLength = 4;

  /// <summary>
  /// The value of the PRISON_REPORTED_SUFFIX attribute.
  /// This field contains the suffix that was reported to SSA by the prison.
  ///  If no suffix is available, this field contains spaces.
  /// </summary>
  [JsonPropertyName("prisonReportedSuffix")]
  [Member(Index = 13, Type = MemberType.Char, Length
    = PrisonReportedSuffix_MaxLength, Optional = true)]
  public string PrisonReportedSuffix
  {
    get => prisonReportedSuffix;
    set => prisonReportedSuffix = value != null
      ? TrimEnd(Substring(value, 1, PrisonReportedSuffix_MaxLength)) : null;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// USER ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 14, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  [Member(Index = 15, Type = MemberType.Timestamp)]
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
  [Member(Index = 16, Type = MemberType.Char, Length
    = LastUpdatedBy_MaxLength, Optional = true)]
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
  [Member(Index = 17, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  /// <summary>Length of the LOCATE_SOURCE_RESPONSE_AGENCY_CO attribute.
  /// </summary>
  public const int FcgLSRspAgy_MaxLength = 3;

  /// <summary>
  /// The value of the LOCATE_SOURCE_RESPONSE_AGENCY_CO attribute.
  /// This field contains the code that identifies the Locate Source.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 18, Type = MemberType.Char, Length = FcgLSRspAgy_MaxLength)]
  public string FcgLSRspAgy
  {
    get => fcgLSRspAgy ?? "";
    set => fcgLSRspAgy = TrimEnd(Substring(value, 1, FcgLSRspAgy_MaxLength));
  }

  /// <summary>
  /// The json value of the FcgLSRspAgy attribute.</summary>
  [JsonPropertyName("fcgLSRspAgy")]
  [Computed]
  public string FcgLSRspAgy_Json
  {
    get => NullIf(FcgLSRspAgy, "");
    set => FcgLSRspAgy = value;
  }

  /// <summary>Length of the MEMBER_ID attribute.</summary>
  public const int FcgMemberId_MaxLength = 15;

  /// <summary>
  /// The value of the MEMBER_ID attribute.
  /// This field contains the Member ID that was provided by the submitter of 
  /// the Locate Request.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 19, Type = MemberType.Char, Length = FcgMemberId_MaxLength)]
  public string FcgMemberId
  {
    get => fcgMemberId ?? "";
    set => fcgMemberId = TrimEnd(Substring(value, 1, FcgMemberId_MaxLength));
  }

  /// <summary>
  /// The json value of the FcgMemberId attribute.</summary>
  [JsonPropertyName("fcgMemberId")]
  [Computed]
  public string FcgMemberId_Json
  {
    get => NullIf(FcgMemberId, "");
    set => FcgMemberId = value;
  }

  private int seqNo;
  private string prisonFacilityType;
  private string prisonFacilityName;
  private string prisonFacilityContactName;
  private string prisonFacilityPhone;
  private string prisonFacilityFaxNum;
  private string prisonReportedSsn;
  private DateTime? confinementDate;
  private DateTime? releaseDate;
  private DateTime? reportDate;
  private string prisonerReporterName;
  private string prisonerIdNumber;
  private string prisonReportedSuffix;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private string fcgLSRspAgy;
  private string fcgMemberId;
}
