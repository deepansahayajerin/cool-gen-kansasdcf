// The source file: EAB_REPORT_SEND, ID: 371790870, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class EabReportSend: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public EabReportSend()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public EabReportSend(EabReportSend that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new EabReportSend Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(EabReportSend that)
  {
    base.Assign(that);
    reportNumber = that.reportNumber;
    command = that.command;
    blankLineAfterHeading = that.blankLineAfterHeading;
    blankLineAfterColHead = that.blankLineAfterColHead;
    blankLineBeforeFooters = that.blankLineBeforeFooters;
    numberOfColHeadings = that.numberOfColHeadings;
    numberOfFooters = that.numberOfFooters;
    overridePageNo = that.overridePageNo;
    overrideLinesPerPage = that.overrideLinesPerPage;
    overrideCarriageControl = that.overrideCarriageControl;
    processDate = that.processDate;
    programName = that.programName;
    reportNoPart2 = that.reportNoPart2;
    runDate = that.runDate;
    runTime = that.runTime;
    rptHeading1 = that.rptHeading1;
    rptHeading2 = that.rptHeading2;
    rptHeading3 = that.rptHeading3;
    colHeading1 = that.colHeading1;
    colHeading2 = that.colHeading2;
    colHeading3 = that.colHeading3;
    rptDetail = that.rptDetail;
    rptFooter1 = that.rptFooter1;
    rptFooter2 = that.rptFooter2;
    rptFooter3 = that.rptFooter3;
  }

  /// <summary>
  /// The value of the REPORT_NUMBER attribute.
  /// </summary>
  [JsonPropertyName("reportNumber")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 2)]
  public int ReportNumber
  {
    get => reportNumber;
    set => reportNumber = value;
  }

  /// <summary>Length of the COMMAND attribute.</summary>
  public const int Command_MaxLength = 7;

  /// <summary>
  /// The value of the COMMAND attribute.
  /// Specifies a command to be completed by the report writer.  The following 
  /// are the only valid values:
  ///    REFRESH will reset page counter
  ///    HEADING will force up to three headings to be written
  ///    DETAIL will force a detail line to be written
  ///    CLOSING will force an end of report line to be written
  ///    FOOTERS will force up to three footers to be written.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = Command_MaxLength)]
  public string Command
  {
    get => command ?? "";
    set => command = TrimEnd(Substring(value, 1, Command_MaxLength));
  }

  /// <summary>
  /// The json value of the Command attribute.</summary>
  [JsonPropertyName("command")]
  [Computed]
  public string Command_Json
  {
    get => NullIf(Command, "");
    set => Command = value;
  }

  /// <summary>Length of the BLANK_LINE_AFTER_HEADING attribute.</summary>
  public const int BlankLineAfterHeading_MaxLength = 1;

  /// <summary>
  /// The value of the BLANK_LINE_AFTER_HEADING attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length
    = BlankLineAfterHeading_MaxLength)]
  public string BlankLineAfterHeading
  {
    get => blankLineAfterHeading ?? "";
    set => blankLineAfterHeading =
      TrimEnd(Substring(value, 1, BlankLineAfterHeading_MaxLength));
  }

  /// <summary>
  /// The json value of the BlankLineAfterHeading attribute.</summary>
  [JsonPropertyName("blankLineAfterHeading")]
  [Computed]
  public string BlankLineAfterHeading_Json
  {
    get => NullIf(BlankLineAfterHeading, "");
    set => BlankLineAfterHeading = value;
  }

  /// <summary>Length of the BLANK_LINE_AFTER_COL_HEAD attribute.</summary>
  public const int BlankLineAfterColHead_MaxLength = 1;

  /// <summary>
  /// The value of the BLANK_LINE_AFTER_COL_HEAD attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length
    = BlankLineAfterColHead_MaxLength)]
  public string BlankLineAfterColHead
  {
    get => blankLineAfterColHead ?? "";
    set => blankLineAfterColHead =
      TrimEnd(Substring(value, 1, BlankLineAfterColHead_MaxLength));
  }

  /// <summary>
  /// The json value of the BlankLineAfterColHead attribute.</summary>
  [JsonPropertyName("blankLineAfterColHead")]
  [Computed]
  public string BlankLineAfterColHead_Json
  {
    get => NullIf(BlankLineAfterColHead, "");
    set => BlankLineAfterColHead = value;
  }

  /// <summary>Length of the BLANK_LINE_BEFORE_FOOTERS attribute.</summary>
  public const int BlankLineBeforeFooters_MaxLength = 1;

  /// <summary>
  /// The value of the BLANK_LINE_BEFORE_FOOTERS attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length
    = BlankLineBeforeFooters_MaxLength)]
  public string BlankLineBeforeFooters
  {
    get => blankLineBeforeFooters ?? "";
    set => blankLineBeforeFooters =
      TrimEnd(Substring(value, 1, BlankLineBeforeFooters_MaxLength));
  }

  /// <summary>
  /// The json value of the BlankLineBeforeFooters attribute.</summary>
  [JsonPropertyName("blankLineBeforeFooters")]
  [Computed]
  public string BlankLineBeforeFooters_Json
  {
    get => NullIf(BlankLineBeforeFooters, "");
    set => BlankLineBeforeFooters = value;
  }

  /// <summary>
  /// The value of the NUMBER_OF_COL_HEADINGS attribute.
  /// </summary>
  [JsonPropertyName("numberOfColHeadings")]
  [DefaultValue(0)]
  [Member(Index = 6, Type = MemberType.Number, Length = 1)]
  public int NumberOfColHeadings
  {
    get => numberOfColHeadings;
    set => numberOfColHeadings = value;
  }

  /// <summary>
  /// The value of the NUMBER_OF_FOOTERS attribute.
  /// </summary>
  [JsonPropertyName("numberOfFooters")]
  [DefaultValue(0)]
  [Member(Index = 7, Type = MemberType.Number, Length = 1)]
  public int NumberOfFooters
  {
    get => numberOfFooters;
    set => numberOfFooters = value;
  }

  /// <summary>
  /// The value of the OVERRIDE_PAGE_NO attribute.
  /// </summary>
  [JsonPropertyName("overridePageNo")]
  [DefaultValue(0)]
  [Member(Index = 8, Type = MemberType.Number, Length = 9)]
  public int OverridePageNo
  {
    get => overridePageNo;
    set => overridePageNo = value;
  }

  /// <summary>Length of the OVERRIDE_LINES_PER_PAGE attribute.</summary>
  public const int OverrideLinesPerPage_MaxLength = 2;

  /// <summary>
  /// The value of the OVERRIDE_LINES_PER_PAGE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length
    = OverrideLinesPerPage_MaxLength)]
  public string OverrideLinesPerPage
  {
    get => overrideLinesPerPage ?? "";
    set => overrideLinesPerPage =
      TrimEnd(Substring(value, 1, OverrideLinesPerPage_MaxLength));
  }

  /// <summary>
  /// The json value of the OverrideLinesPerPage attribute.</summary>
  [JsonPropertyName("overrideLinesPerPage")]
  [Computed]
  public string OverrideLinesPerPage_Json
  {
    get => NullIf(OverrideLinesPerPage, "");
    set => OverrideLinesPerPage = value;
  }

  /// <summary>Length of the OVERRIDE_CARRIAGE_CONTROL attribute.</summary>
  public const int OverrideCarriageControl_MaxLength = 1;

  /// <summary>
  /// The value of the OVERRIDE_CARRIAGE_CONTROL attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length
    = OverrideCarriageControl_MaxLength)]
  public string OverrideCarriageControl
  {
    get => overrideCarriageControl ?? "";
    set => overrideCarriageControl =
      TrimEnd(Substring(value, 1, OverrideCarriageControl_MaxLength));
  }

  /// <summary>
  /// The json value of the OverrideCarriageControl attribute.</summary>
  [JsonPropertyName("overrideCarriageControl")]
  [Computed]
  public string OverrideCarriageControl_Json
  {
    get => NullIf(OverrideCarriageControl, "");
    set => OverrideCarriageControl = value;
  }

  /// <summary>
  /// The value of the PROCESS_DATE attribute.
  /// </summary>
  [JsonPropertyName("processDate")]
  [Member(Index = 11, Type = MemberType.Date)]
  public DateTime? ProcessDate
  {
    get => processDate;
    set => processDate = value;
  }

  /// <summary>Length of the PROGRAM_NAME attribute.</summary>
  public const int ProgramName_MaxLength = 8;

  /// <summary>
  /// The value of the PROGRAM_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length = ProgramName_MaxLength)]
  public string ProgramName
  {
    get => programName ?? "";
    set => programName = TrimEnd(Substring(value, 1, ProgramName_MaxLength));
  }

  /// <summary>
  /// The json value of the ProgramName attribute.</summary>
  [JsonPropertyName("programName")]
  [Computed]
  public string ProgramName_Json
  {
    get => NullIf(ProgramName, "");
    set => ProgramName = value;
  }

  /// <summary>Length of the REPORT_NO_PART2 attribute.</summary>
  public const int ReportNoPart2_MaxLength = 2;

  /// <summary>
  /// The value of the REPORT_NO_PART2 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length = ReportNoPart2_MaxLength)]
  public string ReportNoPart2
  {
    get => reportNoPart2 ?? "";
    set => reportNoPart2 =
      TrimEnd(Substring(value, 1, ReportNoPart2_MaxLength));
  }

  /// <summary>
  /// The json value of the ReportNoPart2 attribute.</summary>
  [JsonPropertyName("reportNoPart2")]
  [Computed]
  public string ReportNoPart2_Json
  {
    get => NullIf(ReportNoPart2, "");
    set => ReportNoPart2 = value;
  }

  /// <summary>
  /// The value of the RUN_DATE attribute.
  /// </summary>
  [JsonPropertyName("runDate")]
  [Member(Index = 14, Type = MemberType.Date)]
  public DateTime? RunDate
  {
    get => runDate;
    set => runDate = value;
  }

  /// <summary>
  /// The value of the RUN_TIME attribute.
  /// </summary>
  [JsonPropertyName("runTime")]
  [Member(Index = 15, Type = MemberType.Time)]
  public TimeSpan RunTime
  {
    get => runTime;
    set => runTime = value;
  }

  /// <summary>Length of the RPT_HEADING_1 attribute.</summary>
  public const int RptHeading1_MaxLength = 80;

  /// <summary>
  /// The value of the RPT_HEADING_1 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 16, Type = MemberType.Char, Length = RptHeading1_MaxLength)]
  public string RptHeading1
  {
    get => rptHeading1 ?? "";
    set => rptHeading1 = TrimEnd(Substring(value, 1, RptHeading1_MaxLength));
  }

  /// <summary>
  /// The json value of the RptHeading1 attribute.</summary>
  [JsonPropertyName("rptHeading1")]
  [Computed]
  public string RptHeading1_Json
  {
    get => NullIf(RptHeading1, "");
    set => RptHeading1 = value;
  }

  /// <summary>Length of the RPT_HEADING_2 attribute.</summary>
  public const int RptHeading2_MaxLength = 80;

  /// <summary>
  /// The value of the RPT_HEADING_2 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 17, Type = MemberType.Char, Length = RptHeading2_MaxLength)]
  public string RptHeading2
  {
    get => rptHeading2 ?? "";
    set => rptHeading2 = TrimEnd(Substring(value, 1, RptHeading2_MaxLength));
  }

  /// <summary>
  /// The json value of the RptHeading2 attribute.</summary>
  [JsonPropertyName("rptHeading2")]
  [Computed]
  public string RptHeading2_Json
  {
    get => NullIf(RptHeading2, "");
    set => RptHeading2 = value;
  }

  /// <summary>Length of the RPT_HEADING_3 attribute.</summary>
  public const int RptHeading3_MaxLength = 80;

  /// <summary>
  /// The value of the RPT_HEADING_3 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 18, Type = MemberType.Char, Length = RptHeading3_MaxLength)]
  public string RptHeading3
  {
    get => rptHeading3 ?? "";
    set => rptHeading3 = TrimEnd(Substring(value, 1, RptHeading3_MaxLength));
  }

  /// <summary>
  /// The json value of the RptHeading3 attribute.</summary>
  [JsonPropertyName("rptHeading3")]
  [Computed]
  public string RptHeading3_Json
  {
    get => NullIf(RptHeading3, "");
    set => RptHeading3 = value;
  }

  /// <summary>Length of the COL_HEADING_1 attribute.</summary>
  public const int ColHeading1_MaxLength = 132;

  /// <summary>
  /// The value of the COL_HEADING_1 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 19, Type = MemberType.Char, Length = ColHeading1_MaxLength)]
  public string ColHeading1
  {
    get => colHeading1 ?? "";
    set => colHeading1 = TrimEnd(Substring(value, 1, ColHeading1_MaxLength));
  }

  /// <summary>
  /// The json value of the ColHeading1 attribute.</summary>
  [JsonPropertyName("colHeading1")]
  [Computed]
  public string ColHeading1_Json
  {
    get => NullIf(ColHeading1, "");
    set => ColHeading1 = value;
  }

  /// <summary>Length of the COL_HEADING_2 attribute.</summary>
  public const int ColHeading2_MaxLength = 132;

  /// <summary>
  /// The value of the COL_HEADING_2 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 20, Type = MemberType.Char, Length = ColHeading2_MaxLength)]
  public string ColHeading2
  {
    get => colHeading2 ?? "";
    set => colHeading2 = TrimEnd(Substring(value, 1, ColHeading2_MaxLength));
  }

  /// <summary>
  /// The json value of the ColHeading2 attribute.</summary>
  [JsonPropertyName("colHeading2")]
  [Computed]
  public string ColHeading2_Json
  {
    get => NullIf(ColHeading2, "");
    set => ColHeading2 = value;
  }

  /// <summary>Length of the COL_HEADING_3 attribute.</summary>
  public const int ColHeading3_MaxLength = 132;

  /// <summary>
  /// The value of the COL_HEADING_3 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 21, Type = MemberType.Char, Length = ColHeading3_MaxLength)]
  public string ColHeading3
  {
    get => colHeading3 ?? "";
    set => colHeading3 = TrimEnd(Substring(value, 1, ColHeading3_MaxLength));
  }

  /// <summary>
  /// The json value of the ColHeading3 attribute.</summary>
  [JsonPropertyName("colHeading3")]
  [Computed]
  public string ColHeading3_Json
  {
    get => NullIf(ColHeading3, "");
    set => ColHeading3 = value;
  }

  /// <summary>Length of the RPT_DETAIL attribute.</summary>
  public const int RptDetail_MaxLength = 132;

  /// <summary>
  /// The value of the RPT_DETAIL attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 22, Type = MemberType.Char, Length = RptDetail_MaxLength)]
  public string RptDetail
  {
    get => rptDetail ?? "";
    set => rptDetail = TrimEnd(Substring(value, 1, RptDetail_MaxLength));
  }

  /// <summary>
  /// The json value of the RptDetail attribute.</summary>
  [JsonPropertyName("rptDetail")]
  [Computed]
  public string RptDetail_Json
  {
    get => NullIf(RptDetail, "");
    set => RptDetail = value;
  }

  /// <summary>Length of the RPT_FOOTER_1 attribute.</summary>
  public const int RptFooter1_MaxLength = 132;

  /// <summary>
  /// The value of the RPT_FOOTER_1 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 23, Type = MemberType.Char, Length = RptFooter1_MaxLength)]
  public string RptFooter1
  {
    get => rptFooter1 ?? "";
    set => rptFooter1 = TrimEnd(Substring(value, 1, RptFooter1_MaxLength));
  }

  /// <summary>
  /// The json value of the RptFooter1 attribute.</summary>
  [JsonPropertyName("rptFooter1")]
  [Computed]
  public string RptFooter1_Json
  {
    get => NullIf(RptFooter1, "");
    set => RptFooter1 = value;
  }

  /// <summary>Length of the RPT_FOOTER_2 attribute.</summary>
  public const int RptFooter2_MaxLength = 132;

  /// <summary>
  /// The value of the RPT_FOOTER_2 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 24, Type = MemberType.Char, Length = RptFooter2_MaxLength)]
  public string RptFooter2
  {
    get => rptFooter2 ?? "";
    set => rptFooter2 = TrimEnd(Substring(value, 1, RptFooter2_MaxLength));
  }

  /// <summary>
  /// The json value of the RptFooter2 attribute.</summary>
  [JsonPropertyName("rptFooter2")]
  [Computed]
  public string RptFooter2_Json
  {
    get => NullIf(RptFooter2, "");
    set => RptFooter2 = value;
  }

  /// <summary>Length of the RPT_FOOTER_3 attribute.</summary>
  public const int RptFooter3_MaxLength = 132;

  /// <summary>
  /// The value of the RPT_FOOTER_3 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 25, Type = MemberType.Char, Length = RptFooter3_MaxLength)]
  public string RptFooter3
  {
    get => rptFooter3 ?? "";
    set => rptFooter3 = TrimEnd(Substring(value, 1, RptFooter3_MaxLength));
  }

  /// <summary>
  /// The json value of the RptFooter3 attribute.</summary>
  [JsonPropertyName("rptFooter3")]
  [Computed]
  public string RptFooter3_Json
  {
    get => NullIf(RptFooter3, "");
    set => RptFooter3 = value;
  }

  private int reportNumber;
  private string command;
  private string blankLineAfterHeading;
  private string blankLineAfterColHead;
  private string blankLineBeforeFooters;
  private int numberOfColHeadings;
  private int numberOfFooters;
  private int overridePageNo;
  private string overrideLinesPerPage;
  private string overrideCarriageControl;
  private DateTime? processDate;
  private string programName;
  private string reportNoPart2;
  private DateTime? runDate;
  private TimeSpan runTime;
  private string rptHeading1;
  private string rptHeading2;
  private string rptHeading3;
  private string colHeading1;
  private string colHeading2;
  private string colHeading3;
  private string rptDetail;
  private string rptFooter1;
  private string rptFooter2;
  private string rptFooter3;
}
