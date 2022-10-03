// The source file: METRO2_CRA_HEADER_RECORD, ID: 1902630969, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class Metro2CraHeaderRecord: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public Metro2CraHeaderRecord()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public Metro2CraHeaderRecord(Metro2CraHeaderRecord that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new Metro2CraHeaderRecord Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(Metro2CraHeaderRecord that)
  {
    base.Assign(that);
    recordDescriptorWord = that.recordDescriptorWord;
    recordIdentifier = that.recordIdentifier;
    cycleIdentifier = that.cycleIdentifier;
    innovisProgramIdentifier = that.innovisProgramIdentifier;
    equifaxProgramIdentifier = that.equifaxProgramIdentifier;
    experianProgramIdentifier = that.experianProgramIdentifier;
    transunionProgramIdentifier = that.transunionProgramIdentifier;
    activityDate = that.activityDate;
    dateCreated = that.dateCreated;
    programDate = that.programDate;
    programRevisionDate = that.programRevisionDate;
    reporterName = that.reporterName;
    reporterAddress = that.reporterAddress;
    reporterTelephoneNumber = that.reporterTelephoneNumber;
    softwareVendorName = that.softwareVendorName;
    softwareVersionNumber = that.softwareVersionNumber;
    microbiltprbcProgramIdentifier = that.microbiltprbcProgramIdentifier;
    reserved = that.reserved;
  }

  /// <summary>Length of the RECORD_DESCRIPTOR_WORD attribute.</summary>
  public const int RecordDescriptorWord_MaxLength = 4;

  /// <summary>
  /// The value of the RECORD_DESCRIPTOR_WORD attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length
    = RecordDescriptorWord_MaxLength)]
  public string RecordDescriptorWord
  {
    get => recordDescriptorWord ?? "";
    set => recordDescriptorWord =
      TrimEnd(Substring(value, 1, RecordDescriptorWord_MaxLength));
  }

  /// <summary>
  /// The json value of the RecordDescriptorWord attribute.</summary>
  [JsonPropertyName("recordDescriptorWord")]
  [Computed]
  public string RecordDescriptorWord_Json
  {
    get => NullIf(RecordDescriptorWord, "");
    set => RecordDescriptorWord = value;
  }

  /// <summary>Length of the RECORD_IDENTIFIER attribute.</summary>
  public const int RecordIdentifier_MaxLength = 6;

  /// <summary>
  /// The value of the RECORD_IDENTIFIER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = RecordIdentifier_MaxLength)
    ]
  public string RecordIdentifier
  {
    get => recordIdentifier ?? "";
    set => recordIdentifier =
      TrimEnd(Substring(value, 1, RecordIdentifier_MaxLength));
  }

  /// <summary>
  /// The json value of the RecordIdentifier attribute.</summary>
  [JsonPropertyName("recordIdentifier")]
  [Computed]
  public string RecordIdentifier_Json
  {
    get => NullIf(RecordIdentifier, "");
    set => RecordIdentifier = value;
  }

  /// <summary>Length of the CYCLE_IDENTIFIER attribute.</summary>
  public const int CycleIdentifier_MaxLength = 2;

  /// <summary>
  /// The value of the CYCLE_IDENTIFIER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = CycleIdentifier_MaxLength)]
    
  public string CycleIdentifier
  {
    get => cycleIdentifier ?? "";
    set => cycleIdentifier =
      TrimEnd(Substring(value, 1, CycleIdentifier_MaxLength));
  }

  /// <summary>
  /// The json value of the CycleIdentifier attribute.</summary>
  [JsonPropertyName("cycleIdentifier")]
  [Computed]
  public string CycleIdentifier_Json
  {
    get => NullIf(CycleIdentifier, "");
    set => CycleIdentifier = value;
  }

  /// <summary>Length of the INNOVIS_PROGRAM_IDENTIFIER attribute.</summary>
  public const int InnovisProgramIdentifier_MaxLength = 10;

  /// <summary>
  /// The value of the INNOVIS_PROGRAM_IDENTIFIER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length
    = InnovisProgramIdentifier_MaxLength)]
  public string InnovisProgramIdentifier
  {
    get => innovisProgramIdentifier ?? "";
    set => innovisProgramIdentifier =
      TrimEnd(Substring(value, 1, InnovisProgramIdentifier_MaxLength));
  }

  /// <summary>
  /// The json value of the InnovisProgramIdentifier attribute.</summary>
  [JsonPropertyName("innovisProgramIdentifier")]
  [Computed]
  public string InnovisProgramIdentifier_Json
  {
    get => NullIf(InnovisProgramIdentifier, "");
    set => InnovisProgramIdentifier = value;
  }

  /// <summary>Length of the EQUIFAX_PROGRAM_IDENTIFIER attribute.</summary>
  public const int EquifaxProgramIdentifier_MaxLength = 10;

  /// <summary>
  /// The value of the EQUIFAX_PROGRAM_IDENTIFIER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length
    = EquifaxProgramIdentifier_MaxLength)]
  public string EquifaxProgramIdentifier
  {
    get => equifaxProgramIdentifier ?? "";
    set => equifaxProgramIdentifier =
      TrimEnd(Substring(value, 1, EquifaxProgramIdentifier_MaxLength));
  }

  /// <summary>
  /// The json value of the EquifaxProgramIdentifier attribute.</summary>
  [JsonPropertyName("equifaxProgramIdentifier")]
  [Computed]
  public string EquifaxProgramIdentifier_Json
  {
    get => NullIf(EquifaxProgramIdentifier, "");
    set => EquifaxProgramIdentifier = value;
  }

  /// <summary>Length of the EXPERIAN_PROGRAM_IDENTIFIER attribute.</summary>
  public const int ExperianProgramIdentifier_MaxLength = 5;

  /// <summary>
  /// The value of the EXPERIAN_PROGRAM_IDENTIFIER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length
    = ExperianProgramIdentifier_MaxLength)]
  public string ExperianProgramIdentifier
  {
    get => experianProgramIdentifier ?? "";
    set => experianProgramIdentifier =
      TrimEnd(Substring(value, 1, ExperianProgramIdentifier_MaxLength));
  }

  /// <summary>
  /// The json value of the ExperianProgramIdentifier attribute.</summary>
  [JsonPropertyName("experianProgramIdentifier")]
  [Computed]
  public string ExperianProgramIdentifier_Json
  {
    get => NullIf(ExperianProgramIdentifier, "");
    set => ExperianProgramIdentifier = value;
  }

  /// <summary>Length of the TRANSUNION_PROGRAM_IDENTIFIER attribute.</summary>
  public const int TransunionProgramIdentifier_MaxLength = 10;

  /// <summary>
  /// The value of the TRANSUNION_PROGRAM_IDENTIFIER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length
    = TransunionProgramIdentifier_MaxLength)]
  public string TransunionProgramIdentifier
  {
    get => transunionProgramIdentifier ?? "";
    set => transunionProgramIdentifier =
      TrimEnd(Substring(value, 1, TransunionProgramIdentifier_MaxLength));
  }

  /// <summary>
  /// The json value of the TransunionProgramIdentifier attribute.</summary>
  [JsonPropertyName("transunionProgramIdentifier")]
  [Computed]
  public string TransunionProgramIdentifier_Json
  {
    get => NullIf(TransunionProgramIdentifier, "");
    set => TransunionProgramIdentifier = value;
  }

  /// <summary>Length of the ACTIVITY_DATE attribute.</summary>
  public const int ActivityDate_MaxLength = 8;

  /// <summary>
  /// The value of the ACTIVITY_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = ActivityDate_MaxLength)]
  public string ActivityDate
  {
    get => activityDate ?? "";
    set => activityDate = TrimEnd(Substring(value, 1, ActivityDate_MaxLength));
  }

  /// <summary>
  /// The json value of the ActivityDate attribute.</summary>
  [JsonPropertyName("activityDate")]
  [Computed]
  public string ActivityDate_Json
  {
    get => NullIf(ActivityDate, "");
    set => ActivityDate = value;
  }

  /// <summary>Length of the DATE_CREATED attribute.</summary>
  public const int DateCreated_MaxLength = 8;

  /// <summary>
  /// The value of the DATE_CREATED attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = DateCreated_MaxLength)]
  public string DateCreated
  {
    get => dateCreated ?? "";
    set => dateCreated = TrimEnd(Substring(value, 1, DateCreated_MaxLength));
  }

  /// <summary>
  /// The json value of the DateCreated attribute.</summary>
  [JsonPropertyName("dateCreated")]
  [Computed]
  public string DateCreated_Json
  {
    get => NullIf(DateCreated, "");
    set => DateCreated = value;
  }

  /// <summary>Length of the PROGRAM_DATE attribute.</summary>
  public const int ProgramDate_MaxLength = 8;

  /// <summary>
  /// The value of the PROGRAM_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = ProgramDate_MaxLength)]
  public string ProgramDate
  {
    get => programDate ?? "";
    set => programDate = TrimEnd(Substring(value, 1, ProgramDate_MaxLength));
  }

  /// <summary>
  /// The json value of the ProgramDate attribute.</summary>
  [JsonPropertyName("programDate")]
  [Computed]
  public string ProgramDate_Json
  {
    get => NullIf(ProgramDate, "");
    set => ProgramDate = value;
  }

  /// <summary>Length of the PROGRAM_REVISION_DATE attribute.</summary>
  public const int ProgramRevisionDate_MaxLength = 8;

  /// <summary>
  /// The value of the PROGRAM_REVISION_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length
    = ProgramRevisionDate_MaxLength)]
  public string ProgramRevisionDate
  {
    get => programRevisionDate ?? "";
    set => programRevisionDate =
      TrimEnd(Substring(value, 1, ProgramRevisionDate_MaxLength));
  }

  /// <summary>
  /// The json value of the ProgramRevisionDate attribute.</summary>
  [JsonPropertyName("programRevisionDate")]
  [Computed]
  public string ProgramRevisionDate_Json
  {
    get => NullIf(ProgramRevisionDate, "");
    set => ProgramRevisionDate = value;
  }

  /// <summary>Length of the REPORTER_NAME attribute.</summary>
  public const int ReporterName_MaxLength = 40;

  /// <summary>
  /// The value of the REPORTER_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length = ReporterName_MaxLength)]
  public string ReporterName
  {
    get => reporterName ?? "";
    set => reporterName = TrimEnd(Substring(value, 1, ReporterName_MaxLength));
  }

  /// <summary>
  /// The json value of the ReporterName attribute.</summary>
  [JsonPropertyName("reporterName")]
  [Computed]
  public string ReporterName_Json
  {
    get => NullIf(ReporterName, "");
    set => ReporterName = value;
  }

  /// <summary>Length of the REPORTER_ADDRESS attribute.</summary>
  public const int ReporterAddress_MaxLength = 96;

  /// <summary>
  /// The value of the REPORTER_ADDRESS attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length = ReporterAddress_MaxLength)
    ]
  public string ReporterAddress
  {
    get => reporterAddress ?? "";
    set => reporterAddress =
      TrimEnd(Substring(value, 1, ReporterAddress_MaxLength));
  }

  /// <summary>
  /// The json value of the ReporterAddress attribute.</summary>
  [JsonPropertyName("reporterAddress")]
  [Computed]
  public string ReporterAddress_Json
  {
    get => NullIf(ReporterAddress, "");
    set => ReporterAddress = value;
  }

  /// <summary>Length of the REPORTER_TELEPHONE_NUMBER attribute.</summary>
  public const int ReporterTelephoneNumber_MaxLength = 10;

  /// <summary>
  /// The value of the REPORTER_TELEPHONE_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 14, Type = MemberType.Char, Length
    = ReporterTelephoneNumber_MaxLength)]
  public string ReporterTelephoneNumber
  {
    get => reporterTelephoneNumber ?? "";
    set => reporterTelephoneNumber =
      TrimEnd(Substring(value, 1, ReporterTelephoneNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the ReporterTelephoneNumber attribute.</summary>
  [JsonPropertyName("reporterTelephoneNumber")]
  [Computed]
  public string ReporterTelephoneNumber_Json
  {
    get => NullIf(ReporterTelephoneNumber, "");
    set => ReporterTelephoneNumber = value;
  }

  /// <summary>Length of the SOFTWARE_VENDOR_NAME attribute.</summary>
  public const int SoftwareVendorName_MaxLength = 40;

  /// <summary>
  /// The value of the SOFTWARE_VENDOR_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 15, Type = MemberType.Char, Length
    = SoftwareVendorName_MaxLength)]
  public string SoftwareVendorName
  {
    get => softwareVendorName ?? "";
    set => softwareVendorName =
      TrimEnd(Substring(value, 1, SoftwareVendorName_MaxLength));
  }

  /// <summary>
  /// The json value of the SoftwareVendorName attribute.</summary>
  [JsonPropertyName("softwareVendorName")]
  [Computed]
  public string SoftwareVendorName_Json
  {
    get => NullIf(SoftwareVendorName, "");
    set => SoftwareVendorName = value;
  }

  /// <summary>Length of the SOFTWARE_VERSION_NUMBER attribute.</summary>
  public const int SoftwareVersionNumber_MaxLength = 5;

  /// <summary>
  /// The value of the SOFTWARE_VERSION_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 16, Type = MemberType.Char, Length
    = SoftwareVersionNumber_MaxLength)]
  public string SoftwareVersionNumber
  {
    get => softwareVersionNumber ?? "";
    set => softwareVersionNumber =
      TrimEnd(Substring(value, 1, SoftwareVersionNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the SoftwareVersionNumber attribute.</summary>
  [JsonPropertyName("softwareVersionNumber")]
  [Computed]
  public string SoftwareVersionNumber_Json
  {
    get => NullIf(SoftwareVersionNumber, "");
    set => SoftwareVersionNumber = value;
  }

  /// <summary>Length of the MICROBILTPRBC_PROGRAM_IDENTIFIER attribute.
  /// </summary>
  public const int MicrobiltprbcProgramIdentifier_MaxLength = 10;

  /// <summary>
  /// The value of the MICROBILTPRBC_PROGRAM_IDENTIFIER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 17, Type = MemberType.Char, Length
    = MicrobiltprbcProgramIdentifier_MaxLength)]
  public string MicrobiltprbcProgramIdentifier
  {
    get => microbiltprbcProgramIdentifier ?? "";
    set => microbiltprbcProgramIdentifier =
      TrimEnd(Substring(value, 1, MicrobiltprbcProgramIdentifier_MaxLength));
  }

  /// <summary>
  /// The json value of the MicrobiltprbcProgramIdentifier attribute.</summary>
  [JsonPropertyName("microbiltprbcProgramIdentifier")]
  [Computed]
  public string MicrobiltprbcProgramIdentifier_Json
  {
    get => NullIf(MicrobiltprbcProgramIdentifier, "");
    set => MicrobiltprbcProgramIdentifier = value;
  }

  /// <summary>Length of the RESERVED attribute.</summary>
  public const int Reserved_MaxLength = 146;

  /// <summary>
  /// The value of the RESERVED attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 18, Type = MemberType.Char, Length = Reserved_MaxLength)]
  public string Reserved
  {
    get => reserved ?? "";
    set => reserved = TrimEnd(Substring(value, 1, Reserved_MaxLength));
  }

  /// <summary>
  /// The json value of the Reserved attribute.</summary>
  [JsonPropertyName("reserved")]
  [Computed]
  public string Reserved_Json
  {
    get => NullIf(Reserved, "");
    set => Reserved = value;
  }

  private string recordDescriptorWord;
  private string recordIdentifier;
  private string cycleIdentifier;
  private string innovisProgramIdentifier;
  private string equifaxProgramIdentifier;
  private string experianProgramIdentifier;
  private string transunionProgramIdentifier;
  private string activityDate;
  private string dateCreated;
  private string programDate;
  private string programRevisionDate;
  private string reporterName;
  private string reporterAddress;
  private string reporterTelephoneNumber;
  private string softwareVendorName;
  private string softwareVersionNumber;
  private string microbiltprbcProgramIdentifier;
  private string reserved;
}
