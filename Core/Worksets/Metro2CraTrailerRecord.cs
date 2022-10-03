// The source file: METRO2_CRA_TRAILER_RECORD, ID: 1902630988, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class Metro2CraTrailerRecord: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public Metro2CraTrailerRecord()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public Metro2CraTrailerRecord(Metro2CraTrailerRecord that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new Metro2CraTrailerRecord Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(Metro2CraTrailerRecord that)
  {
    base.Assign(that);
    recordDesciptorWord = that.recordDesciptorWord;
    recordIdentifier = that.recordIdentifier;
    totalBaseRecords = that.totalBaseRecords;
    reserved4 = that.reserved4;
    totalOfStatusCodeDf = that.totalOfStatusCodeDf;
    totalJ1Segments = that.totalJ1Segments;
    totalJ2Segments = that.totalJ2Segments;
    blockCount = that.blockCount;
    totalOfStatusCodeDa = that.totalOfStatusCodeDa;
    totalOfStatusCode05 = that.totalOfStatusCode05;
    totalOfStatusCode11 = that.totalOfStatusCode11;
    totalOfStatusCode13 = that.totalOfStatusCode13;
    totalOfStatusCode61 = that.totalOfStatusCode61;
    totalOfStatusCode62 = that.totalOfStatusCode62;
    totalOfStatusCode63 = that.totalOfStatusCode63;
    totalOfStatusCode64 = that.totalOfStatusCode64;
    totalOfStatusCode65 = that.totalOfStatusCode65;
    totalOfStatusCode71 = that.totalOfStatusCode71;
    totalOfStatusCode78 = that.totalOfStatusCode78;
    totalOfStatusCode80 = that.totalOfStatusCode80;
    totalOfStatusCode82 = that.totalOfStatusCode82;
    totalOfStatusCode83 = that.totalOfStatusCode83;
    totalOfStatusCode84 = that.totalOfStatusCode84;
    totalOfStatusCode88 = that.totalOfStatusCode88;
    totalOfStatusCode89 = that.totalOfStatusCode89;
    totalOfStatusCode93 = that.totalOfStatusCode93;
    totalOfStatusCode94 = that.totalOfStatusCode94;
    totalOfStatusCode95 = that.totalOfStatusCode95;
    totalOfStatusCode96 = that.totalOfStatusCode96;
    totalOfStatusCode97 = that.totalOfStatusCode97;
    totalOfEcoaCodeZ = that.totalOfEcoaCodeZ;
    totalEmploymentSegments = that.totalEmploymentSegments;
    totalOriginalCreditorSegments = that.totalOriginalCreditorSegments;
    totalPurchasedFromSoldToSeg = that.totalPurchasedFromSoldToSeg;
    totalMorgageInformationSegmen = that.totalMorgageInformationSegmen;
    totalSpecicalizedPaymentInfo = that.totalSpecicalizedPaymentInfo;
    totalChangeSegements = that.totalChangeSegements;
    totalSsnsAllSegments = that.totalSsnsAllSegments;
    totalSsnsBaseSegments = that.totalSsnsBaseSegments;
    totalSsnsJ1Segements = that.totalSsnsJ1Segements;
    totalSsnsJ2Segments = that.totalSsnsJ2Segments;
    totalBirthDatesAllSegments = that.totalBirthDatesAllSegments;
    totalBirthDatesBaseSegments = that.totalBirthDatesBaseSegments;
    totalBirthDatesJ1Segments = that.totalBirthDatesJ1Segments;
    totalBirthDatesJ2Segments = that.totalBirthDatesJ2Segments;
    totalPhoneNumberAllSegments = that.totalPhoneNumberAllSegments;
    reserved47 = that.reserved47;
  }

  /// <summary>Length of the RECORD_DESCIPTOR_WORD attribute.</summary>
  public const int RecordDesciptorWord_MaxLength = 4;

  /// <summary>
  /// The value of the RECORD_DESCIPTOR_WORD attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length
    = RecordDesciptorWord_MaxLength)]
  public string RecordDesciptorWord
  {
    get => recordDesciptorWord ?? "";
    set => recordDesciptorWord =
      TrimEnd(Substring(value, 1, RecordDesciptorWord_MaxLength));
  }

  /// <summary>
  /// The json value of the RecordDesciptorWord attribute.</summary>
  [JsonPropertyName("recordDesciptorWord")]
  [Computed]
  public string RecordDesciptorWord_Json
  {
    get => NullIf(RecordDesciptorWord, "");
    set => RecordDesciptorWord = value;
  }

  /// <summary>Length of the RECORD_IDENTIFIER attribute.</summary>
  public const int RecordIdentifier_MaxLength = 7;

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

  /// <summary>Length of the TOTAL_BASE_RECORDS attribute.</summary>
  public const int TotalBaseRecords_MaxLength = 9;

  /// <summary>
  /// The value of the TOTAL_BASE_RECORDS attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = TotalBaseRecords_MaxLength)
    ]
  public string TotalBaseRecords
  {
    get => totalBaseRecords ?? "";
    set => totalBaseRecords =
      TrimEnd(Substring(value, 1, TotalBaseRecords_MaxLength));
  }

  /// <summary>
  /// The json value of the TotalBaseRecords attribute.</summary>
  [JsonPropertyName("totalBaseRecords")]
  [Computed]
  public string TotalBaseRecords_Json
  {
    get => NullIf(TotalBaseRecords, "");
    set => TotalBaseRecords = value;
  }

  /// <summary>Length of the RESERVED_4 attribute.</summary>
  public const int Reserved4_MaxLength = 9;

  /// <summary>
  /// The value of the RESERVED_4 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = Reserved4_MaxLength)]
  public string Reserved4
  {
    get => reserved4 ?? "";
    set => reserved4 = TrimEnd(Substring(value, 1, Reserved4_MaxLength));
  }

  /// <summary>
  /// The json value of the Reserved4 attribute.</summary>
  [JsonPropertyName("reserved4")]
  [Computed]
  public string Reserved4_Json
  {
    get => NullIf(Reserved4, "");
    set => Reserved4 = value;
  }

  /// <summary>Length of the TOTAL_OF_STATUS_CODE_DF attribute.</summary>
  public const int TotalOfStatusCodeDf_MaxLength = 9;

  /// <summary>
  /// The value of the TOTAL_OF_STATUS_CODE_DF attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length
    = TotalOfStatusCodeDf_MaxLength)]
  public string TotalOfStatusCodeDf
  {
    get => totalOfStatusCodeDf ?? "";
    set => totalOfStatusCodeDf =
      TrimEnd(Substring(value, 1, TotalOfStatusCodeDf_MaxLength));
  }

  /// <summary>
  /// The json value of the TotalOfStatusCodeDf attribute.</summary>
  [JsonPropertyName("totalOfStatusCodeDf")]
  [Computed]
  public string TotalOfStatusCodeDf_Json
  {
    get => NullIf(TotalOfStatusCodeDf, "");
    set => TotalOfStatusCodeDf = value;
  }

  /// <summary>Length of the TOTAL_J1_SEGMENTS attribute.</summary>
  public const int TotalJ1Segments_MaxLength = 9;

  /// <summary>
  /// The value of the TOTAL_J1_SEGMENTS attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = TotalJ1Segments_MaxLength)]
    
  public string TotalJ1Segments
  {
    get => totalJ1Segments ?? "";
    set => totalJ1Segments =
      TrimEnd(Substring(value, 1, TotalJ1Segments_MaxLength));
  }

  /// <summary>
  /// The json value of the TotalJ1Segments attribute.</summary>
  [JsonPropertyName("totalJ1Segments")]
  [Computed]
  public string TotalJ1Segments_Json
  {
    get => NullIf(TotalJ1Segments, "");
    set => TotalJ1Segments = value;
  }

  /// <summary>Length of the TOTAL_J2_SEGMENTS attribute.</summary>
  public const int TotalJ2Segments_MaxLength = 9;

  /// <summary>
  /// The value of the TOTAL_J2_SEGMENTS attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = TotalJ2Segments_MaxLength)]
    
  public string TotalJ2Segments
  {
    get => totalJ2Segments ?? "";
    set => totalJ2Segments =
      TrimEnd(Substring(value, 1, TotalJ2Segments_MaxLength));
  }

  /// <summary>
  /// The json value of the TotalJ2Segments attribute.</summary>
  [JsonPropertyName("totalJ2Segments")]
  [Computed]
  public string TotalJ2Segments_Json
  {
    get => NullIf(TotalJ2Segments, "");
    set => TotalJ2Segments = value;
  }

  /// <summary>Length of the BLOCK_COUNT attribute.</summary>
  public const int BlockCount_MaxLength = 9;

  /// <summary>
  /// The value of the BLOCK_COUNT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = BlockCount_MaxLength)]
  public string BlockCount
  {
    get => blockCount ?? "";
    set => blockCount = TrimEnd(Substring(value, 1, BlockCount_MaxLength));
  }

  /// <summary>
  /// The json value of the BlockCount attribute.</summary>
  [JsonPropertyName("blockCount")]
  [Computed]
  public string BlockCount_Json
  {
    get => NullIf(BlockCount, "");
    set => BlockCount = value;
  }

  /// <summary>Length of the TOTAL_OF_STATUS_CODE_DA attribute.</summary>
  public const int TotalOfStatusCodeDa_MaxLength = 9;

  /// <summary>
  /// The value of the TOTAL_OF_STATUS_CODE_DA attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length
    = TotalOfStatusCodeDa_MaxLength)]
  public string TotalOfStatusCodeDa
  {
    get => totalOfStatusCodeDa ?? "";
    set => totalOfStatusCodeDa =
      TrimEnd(Substring(value, 1, TotalOfStatusCodeDa_MaxLength));
  }

  /// <summary>
  /// The json value of the TotalOfStatusCodeDa attribute.</summary>
  [JsonPropertyName("totalOfStatusCodeDa")]
  [Computed]
  public string TotalOfStatusCodeDa_Json
  {
    get => NullIf(TotalOfStatusCodeDa, "");
    set => TotalOfStatusCodeDa = value;
  }

  /// <summary>Length of the TOTAL_OF_STATUS_CODE_05 attribute.</summary>
  public const int TotalOfStatusCode05_MaxLength = 9;

  /// <summary>
  /// The value of the TOTAL_OF_STATUS_CODE_05 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length
    = TotalOfStatusCode05_MaxLength)]
  public string TotalOfStatusCode05
  {
    get => totalOfStatusCode05 ?? "";
    set => totalOfStatusCode05 =
      TrimEnd(Substring(value, 1, TotalOfStatusCode05_MaxLength));
  }

  /// <summary>
  /// The json value of the TotalOfStatusCode05 attribute.</summary>
  [JsonPropertyName("totalOfStatusCode05")]
  [Computed]
  public string TotalOfStatusCode05_Json
  {
    get => NullIf(TotalOfStatusCode05, "");
    set => TotalOfStatusCode05 = value;
  }

  /// <summary>Length of the TOTAL_OF_STATUS_CODE_11 attribute.</summary>
  public const int TotalOfStatusCode11_MaxLength = 9;

  /// <summary>
  /// The value of the TOTAL_OF_STATUS_CODE_11 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length
    = TotalOfStatusCode11_MaxLength)]
  public string TotalOfStatusCode11
  {
    get => totalOfStatusCode11 ?? "";
    set => totalOfStatusCode11 =
      TrimEnd(Substring(value, 1, TotalOfStatusCode11_MaxLength));
  }

  /// <summary>
  /// The json value of the TotalOfStatusCode11 attribute.</summary>
  [JsonPropertyName("totalOfStatusCode11")]
  [Computed]
  public string TotalOfStatusCode11_Json
  {
    get => NullIf(TotalOfStatusCode11, "");
    set => TotalOfStatusCode11 = value;
  }

  /// <summary>Length of the TOTAL_OF_STATUS_CODE_13 attribute.</summary>
  public const int TotalOfStatusCode13_MaxLength = 9;

  /// <summary>
  /// The value of the TOTAL_OF_STATUS_CODE_13 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length
    = TotalOfStatusCode13_MaxLength)]
  public string TotalOfStatusCode13
  {
    get => totalOfStatusCode13 ?? "";
    set => totalOfStatusCode13 =
      TrimEnd(Substring(value, 1, TotalOfStatusCode13_MaxLength));
  }

  /// <summary>
  /// The json value of the TotalOfStatusCode13 attribute.</summary>
  [JsonPropertyName("totalOfStatusCode13")]
  [Computed]
  public string TotalOfStatusCode13_Json
  {
    get => NullIf(TotalOfStatusCode13, "");
    set => TotalOfStatusCode13 = value;
  }

  /// <summary>Length of the TOTAL_OF_STATUS_CODE_61 attribute.</summary>
  public const int TotalOfStatusCode61_MaxLength = 9;

  /// <summary>
  /// The value of the TOTAL_OF_STATUS_CODE_61 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length
    = TotalOfStatusCode61_MaxLength)]
  public string TotalOfStatusCode61
  {
    get => totalOfStatusCode61 ?? "";
    set => totalOfStatusCode61 =
      TrimEnd(Substring(value, 1, TotalOfStatusCode61_MaxLength));
  }

  /// <summary>
  /// The json value of the TotalOfStatusCode61 attribute.</summary>
  [JsonPropertyName("totalOfStatusCode61")]
  [Computed]
  public string TotalOfStatusCode61_Json
  {
    get => NullIf(TotalOfStatusCode61, "");
    set => TotalOfStatusCode61 = value;
  }

  /// <summary>Length of the TOTAL_OF_STATUS_CODE_62 attribute.</summary>
  public const int TotalOfStatusCode62_MaxLength = 9;

  /// <summary>
  /// The value of the TOTAL_OF_STATUS_CODE_62 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 14, Type = MemberType.Char, Length
    = TotalOfStatusCode62_MaxLength)]
  public string TotalOfStatusCode62
  {
    get => totalOfStatusCode62 ?? "";
    set => totalOfStatusCode62 =
      TrimEnd(Substring(value, 1, TotalOfStatusCode62_MaxLength));
  }

  /// <summary>
  /// The json value of the TotalOfStatusCode62 attribute.</summary>
  [JsonPropertyName("totalOfStatusCode62")]
  [Computed]
  public string TotalOfStatusCode62_Json
  {
    get => NullIf(TotalOfStatusCode62, "");
    set => TotalOfStatusCode62 = value;
  }

  /// <summary>Length of the TOTAL_OF_STATUS_CODE_63 attribute.</summary>
  public const int TotalOfStatusCode63_MaxLength = 9;

  /// <summary>
  /// The value of the TOTAL_OF_STATUS_CODE_63 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 15, Type = MemberType.Char, Length
    = TotalOfStatusCode63_MaxLength)]
  public string TotalOfStatusCode63
  {
    get => totalOfStatusCode63 ?? "";
    set => totalOfStatusCode63 =
      TrimEnd(Substring(value, 1, TotalOfStatusCode63_MaxLength));
  }

  /// <summary>
  /// The json value of the TotalOfStatusCode63 attribute.</summary>
  [JsonPropertyName("totalOfStatusCode63")]
  [Computed]
  public string TotalOfStatusCode63_Json
  {
    get => NullIf(TotalOfStatusCode63, "");
    set => TotalOfStatusCode63 = value;
  }

  /// <summary>Length of the TOTAL_OF_STATUS_CODE_64 attribute.</summary>
  public const int TotalOfStatusCode64_MaxLength = 9;

  /// <summary>
  /// The value of the TOTAL_OF_STATUS_CODE_64 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 16, Type = MemberType.Char, Length
    = TotalOfStatusCode64_MaxLength)]
  public string TotalOfStatusCode64
  {
    get => totalOfStatusCode64 ?? "";
    set => totalOfStatusCode64 =
      TrimEnd(Substring(value, 1, TotalOfStatusCode64_MaxLength));
  }

  /// <summary>
  /// The json value of the TotalOfStatusCode64 attribute.</summary>
  [JsonPropertyName("totalOfStatusCode64")]
  [Computed]
  public string TotalOfStatusCode64_Json
  {
    get => NullIf(TotalOfStatusCode64, "");
    set => TotalOfStatusCode64 = value;
  }

  /// <summary>Length of the TOTAL_OF_STATUS_CODE_65 attribute.</summary>
  public const int TotalOfStatusCode65_MaxLength = 9;

  /// <summary>
  /// The value of the TOTAL_OF_STATUS_CODE_65 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 17, Type = MemberType.Char, Length
    = TotalOfStatusCode65_MaxLength)]
  public string TotalOfStatusCode65
  {
    get => totalOfStatusCode65 ?? "";
    set => totalOfStatusCode65 =
      TrimEnd(Substring(value, 1, TotalOfStatusCode65_MaxLength));
  }

  /// <summary>
  /// The json value of the TotalOfStatusCode65 attribute.</summary>
  [JsonPropertyName("totalOfStatusCode65")]
  [Computed]
  public string TotalOfStatusCode65_Json
  {
    get => NullIf(TotalOfStatusCode65, "");
    set => TotalOfStatusCode65 = value;
  }

  /// <summary>Length of the TOTAL_OF_STATUS_CODE_71 attribute.</summary>
  public const int TotalOfStatusCode71_MaxLength = 9;

  /// <summary>
  /// The value of the TOTAL_OF_STATUS_CODE_71 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 18, Type = MemberType.Char, Length
    = TotalOfStatusCode71_MaxLength)]
  public string TotalOfStatusCode71
  {
    get => totalOfStatusCode71 ?? "";
    set => totalOfStatusCode71 =
      TrimEnd(Substring(value, 1, TotalOfStatusCode71_MaxLength));
  }

  /// <summary>
  /// The json value of the TotalOfStatusCode71 attribute.</summary>
  [JsonPropertyName("totalOfStatusCode71")]
  [Computed]
  public string TotalOfStatusCode71_Json
  {
    get => NullIf(TotalOfStatusCode71, "");
    set => TotalOfStatusCode71 = value;
  }

  /// <summary>Length of the TOTAL_OF_STATUS_CODE_78 attribute.</summary>
  public const int TotalOfStatusCode78_MaxLength = 9;

  /// <summary>
  /// The value of the TOTAL_OF_STATUS_CODE_78 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 19, Type = MemberType.Char, Length
    = TotalOfStatusCode78_MaxLength)]
  public string TotalOfStatusCode78
  {
    get => totalOfStatusCode78 ?? "";
    set => totalOfStatusCode78 =
      TrimEnd(Substring(value, 1, TotalOfStatusCode78_MaxLength));
  }

  /// <summary>
  /// The json value of the TotalOfStatusCode78 attribute.</summary>
  [JsonPropertyName("totalOfStatusCode78")]
  [Computed]
  public string TotalOfStatusCode78_Json
  {
    get => NullIf(TotalOfStatusCode78, "");
    set => TotalOfStatusCode78 = value;
  }

  /// <summary>Length of the TOTAL_OF_STATUS_CODE_80 attribute.</summary>
  public const int TotalOfStatusCode80_MaxLength = 9;

  /// <summary>
  /// The value of the TOTAL_OF_STATUS_CODE_80 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 20, Type = MemberType.Char, Length
    = TotalOfStatusCode80_MaxLength)]
  public string TotalOfStatusCode80
  {
    get => totalOfStatusCode80 ?? "";
    set => totalOfStatusCode80 =
      TrimEnd(Substring(value, 1, TotalOfStatusCode80_MaxLength));
  }

  /// <summary>
  /// The json value of the TotalOfStatusCode80 attribute.</summary>
  [JsonPropertyName("totalOfStatusCode80")]
  [Computed]
  public string TotalOfStatusCode80_Json
  {
    get => NullIf(TotalOfStatusCode80, "");
    set => TotalOfStatusCode80 = value;
  }

  /// <summary>Length of the TOTAL_OF_STATUS_CODE_82 attribute.</summary>
  public const int TotalOfStatusCode82_MaxLength = 9;

  /// <summary>
  /// The value of the TOTAL_OF_STATUS_CODE_82 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 21, Type = MemberType.Char, Length
    = TotalOfStatusCode82_MaxLength)]
  public string TotalOfStatusCode82
  {
    get => totalOfStatusCode82 ?? "";
    set => totalOfStatusCode82 =
      TrimEnd(Substring(value, 1, TotalOfStatusCode82_MaxLength));
  }

  /// <summary>
  /// The json value of the TotalOfStatusCode82 attribute.</summary>
  [JsonPropertyName("totalOfStatusCode82")]
  [Computed]
  public string TotalOfStatusCode82_Json
  {
    get => NullIf(TotalOfStatusCode82, "");
    set => TotalOfStatusCode82 = value;
  }

  /// <summary>Length of the TOTAL_OF_STATUS_CODE_83 attribute.</summary>
  public const int TotalOfStatusCode83_MaxLength = 9;

  /// <summary>
  /// The value of the TOTAL_OF_STATUS_CODE_83 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 22, Type = MemberType.Char, Length
    = TotalOfStatusCode83_MaxLength)]
  public string TotalOfStatusCode83
  {
    get => totalOfStatusCode83 ?? "";
    set => totalOfStatusCode83 =
      TrimEnd(Substring(value, 1, TotalOfStatusCode83_MaxLength));
  }

  /// <summary>
  /// The json value of the TotalOfStatusCode83 attribute.</summary>
  [JsonPropertyName("totalOfStatusCode83")]
  [Computed]
  public string TotalOfStatusCode83_Json
  {
    get => NullIf(TotalOfStatusCode83, "");
    set => TotalOfStatusCode83 = value;
  }

  /// <summary>Length of the TOTAL_OF_STATUS_CODE_84 attribute.</summary>
  public const int TotalOfStatusCode84_MaxLength = 9;

  /// <summary>
  /// The value of the TOTAL_OF_STATUS_CODE_84 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 23, Type = MemberType.Char, Length
    = TotalOfStatusCode84_MaxLength)]
  public string TotalOfStatusCode84
  {
    get => totalOfStatusCode84 ?? "";
    set => totalOfStatusCode84 =
      TrimEnd(Substring(value, 1, TotalOfStatusCode84_MaxLength));
  }

  /// <summary>
  /// The json value of the TotalOfStatusCode84 attribute.</summary>
  [JsonPropertyName("totalOfStatusCode84")]
  [Computed]
  public string TotalOfStatusCode84_Json
  {
    get => NullIf(TotalOfStatusCode84, "");
    set => TotalOfStatusCode84 = value;
  }

  /// <summary>Length of the TOTAL_OF_STATUS_CODE_88 attribute.</summary>
  public const int TotalOfStatusCode88_MaxLength = 9;

  /// <summary>
  /// The value of the TOTAL_OF_STATUS_CODE_88 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 24, Type = MemberType.Char, Length
    = TotalOfStatusCode88_MaxLength)]
  public string TotalOfStatusCode88
  {
    get => totalOfStatusCode88 ?? "";
    set => totalOfStatusCode88 =
      TrimEnd(Substring(value, 1, TotalOfStatusCode88_MaxLength));
  }

  /// <summary>
  /// The json value of the TotalOfStatusCode88 attribute.</summary>
  [JsonPropertyName("totalOfStatusCode88")]
  [Computed]
  public string TotalOfStatusCode88_Json
  {
    get => NullIf(TotalOfStatusCode88, "");
    set => TotalOfStatusCode88 = value;
  }

  /// <summary>Length of the TOTAL_OF_STATUS_CODE_89 attribute.</summary>
  public const int TotalOfStatusCode89_MaxLength = 9;

  /// <summary>
  /// The value of the TOTAL_OF_STATUS_CODE_89 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 25, Type = MemberType.Char, Length
    = TotalOfStatusCode89_MaxLength)]
  public string TotalOfStatusCode89
  {
    get => totalOfStatusCode89 ?? "";
    set => totalOfStatusCode89 =
      TrimEnd(Substring(value, 1, TotalOfStatusCode89_MaxLength));
  }

  /// <summary>
  /// The json value of the TotalOfStatusCode89 attribute.</summary>
  [JsonPropertyName("totalOfStatusCode89")]
  [Computed]
  public string TotalOfStatusCode89_Json
  {
    get => NullIf(TotalOfStatusCode89, "");
    set => TotalOfStatusCode89 = value;
  }

  /// <summary>Length of the TOTAL_OF_STATUS_CODE_93 attribute.</summary>
  public const int TotalOfStatusCode93_MaxLength = 9;

  /// <summary>
  /// The value of the TOTAL_OF_STATUS_CODE_93 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 26, Type = MemberType.Char, Length
    = TotalOfStatusCode93_MaxLength)]
  public string TotalOfStatusCode93
  {
    get => totalOfStatusCode93 ?? "";
    set => totalOfStatusCode93 =
      TrimEnd(Substring(value, 1, TotalOfStatusCode93_MaxLength));
  }

  /// <summary>
  /// The json value of the TotalOfStatusCode93 attribute.</summary>
  [JsonPropertyName("totalOfStatusCode93")]
  [Computed]
  public string TotalOfStatusCode93_Json
  {
    get => NullIf(TotalOfStatusCode93, "");
    set => TotalOfStatusCode93 = value;
  }

  /// <summary>Length of the TOTAL_OF_STATUS_CODE_94 attribute.</summary>
  public const int TotalOfStatusCode94_MaxLength = 9;

  /// <summary>
  /// The value of the TOTAL_OF_STATUS_CODE_94 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 27, Type = MemberType.Char, Length
    = TotalOfStatusCode94_MaxLength)]
  public string TotalOfStatusCode94
  {
    get => totalOfStatusCode94 ?? "";
    set => totalOfStatusCode94 =
      TrimEnd(Substring(value, 1, TotalOfStatusCode94_MaxLength));
  }

  /// <summary>
  /// The json value of the TotalOfStatusCode94 attribute.</summary>
  [JsonPropertyName("totalOfStatusCode94")]
  [Computed]
  public string TotalOfStatusCode94_Json
  {
    get => NullIf(TotalOfStatusCode94, "");
    set => TotalOfStatusCode94 = value;
  }

  /// <summary>Length of the TOTAL_OF_STATUS_CODE_95 attribute.</summary>
  public const int TotalOfStatusCode95_MaxLength = 9;

  /// <summary>
  /// The value of the TOTAL_OF_STATUS_CODE_95 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 28, Type = MemberType.Char, Length
    = TotalOfStatusCode95_MaxLength)]
  public string TotalOfStatusCode95
  {
    get => totalOfStatusCode95 ?? "";
    set => totalOfStatusCode95 =
      TrimEnd(Substring(value, 1, TotalOfStatusCode95_MaxLength));
  }

  /// <summary>
  /// The json value of the TotalOfStatusCode95 attribute.</summary>
  [JsonPropertyName("totalOfStatusCode95")]
  [Computed]
  public string TotalOfStatusCode95_Json
  {
    get => NullIf(TotalOfStatusCode95, "");
    set => TotalOfStatusCode95 = value;
  }

  /// <summary>Length of the TOTAL_OF_STATUS_CODE_96 attribute.</summary>
  public const int TotalOfStatusCode96_MaxLength = 9;

  /// <summary>
  /// The value of the TOTAL_OF_STATUS_CODE_96 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 29, Type = MemberType.Char, Length
    = TotalOfStatusCode96_MaxLength)]
  public string TotalOfStatusCode96
  {
    get => totalOfStatusCode96 ?? "";
    set => totalOfStatusCode96 =
      TrimEnd(Substring(value, 1, TotalOfStatusCode96_MaxLength));
  }

  /// <summary>
  /// The json value of the TotalOfStatusCode96 attribute.</summary>
  [JsonPropertyName("totalOfStatusCode96")]
  [Computed]
  public string TotalOfStatusCode96_Json
  {
    get => NullIf(TotalOfStatusCode96, "");
    set => TotalOfStatusCode96 = value;
  }

  /// <summary>Length of the TOTAL_OF_STATUS_CODE_97 attribute.</summary>
  public const int TotalOfStatusCode97_MaxLength = 9;

  /// <summary>
  /// The value of the TOTAL_OF_STATUS_CODE_97 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 30, Type = MemberType.Char, Length
    = TotalOfStatusCode97_MaxLength)]
  public string TotalOfStatusCode97
  {
    get => totalOfStatusCode97 ?? "";
    set => totalOfStatusCode97 =
      TrimEnd(Substring(value, 1, TotalOfStatusCode97_MaxLength));
  }

  /// <summary>
  /// The json value of the TotalOfStatusCode97 attribute.</summary>
  [JsonPropertyName("totalOfStatusCode97")]
  [Computed]
  public string TotalOfStatusCode97_Json
  {
    get => NullIf(TotalOfStatusCode97, "");
    set => TotalOfStatusCode97 = value;
  }

  /// <summary>Length of the TOTAL_OF_ECOA_CODE_Z attribute.</summary>
  public const int TotalOfEcoaCodeZ_MaxLength = 9;

  /// <summary>
  /// The value of the TOTAL_OF_ECOA_CODE_Z attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 31, Type = MemberType.Char, Length
    = TotalOfEcoaCodeZ_MaxLength)]
  public string TotalOfEcoaCodeZ
  {
    get => totalOfEcoaCodeZ ?? "";
    set => totalOfEcoaCodeZ =
      TrimEnd(Substring(value, 1, TotalOfEcoaCodeZ_MaxLength));
  }

  /// <summary>
  /// The json value of the TotalOfEcoaCodeZ attribute.</summary>
  [JsonPropertyName("totalOfEcoaCodeZ")]
  [Computed]
  public string TotalOfEcoaCodeZ_Json
  {
    get => NullIf(TotalOfEcoaCodeZ, "");
    set => TotalOfEcoaCodeZ = value;
  }

  /// <summary>Length of the TOTAL_EMPLOYMENT_SEGMENTS attribute.</summary>
  public const int TotalEmploymentSegments_MaxLength = 9;

  /// <summary>
  /// The value of the TOTAL_EMPLOYMENT_SEGMENTS attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 32, Type = MemberType.Char, Length
    = TotalEmploymentSegments_MaxLength)]
  public string TotalEmploymentSegments
  {
    get => totalEmploymentSegments ?? "";
    set => totalEmploymentSegments =
      TrimEnd(Substring(value, 1, TotalEmploymentSegments_MaxLength));
  }

  /// <summary>
  /// The json value of the TotalEmploymentSegments attribute.</summary>
  [JsonPropertyName("totalEmploymentSegments")]
  [Computed]
  public string TotalEmploymentSegments_Json
  {
    get => NullIf(TotalEmploymentSegments, "");
    set => TotalEmploymentSegments = value;
  }

  /// <summary>Length of the TOTAL_ORIGINAL_CREDITOR_SEGMENTS attribute.
  /// </summary>
  public const int TotalOriginalCreditorSegments_MaxLength = 9;

  /// <summary>
  /// The value of the TOTAL_ORIGINAL_CREDITOR_SEGMENTS attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 33, Type = MemberType.Char, Length
    = TotalOriginalCreditorSegments_MaxLength)]
  public string TotalOriginalCreditorSegments
  {
    get => totalOriginalCreditorSegments ?? "";
    set => totalOriginalCreditorSegments =
      TrimEnd(Substring(value, 1, TotalOriginalCreditorSegments_MaxLength));
  }

  /// <summary>
  /// The json value of the TotalOriginalCreditorSegments attribute.</summary>
  [JsonPropertyName("totalOriginalCreditorSegments")]
  [Computed]
  public string TotalOriginalCreditorSegments_Json
  {
    get => NullIf(TotalOriginalCreditorSegments, "");
    set => TotalOriginalCreditorSegments = value;
  }

  /// <summary>Length of the TOTAL_PURCHASED_FROM_SOLD_TO_SEG attribute.
  /// </summary>
  public const int TotalPurchasedFromSoldToSeg_MaxLength = 9;

  /// <summary>
  /// The value of the TOTAL_PURCHASED_FROM_SOLD_TO_SEG attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 34, Type = MemberType.Char, Length
    = TotalPurchasedFromSoldToSeg_MaxLength)]
  public string TotalPurchasedFromSoldToSeg
  {
    get => totalPurchasedFromSoldToSeg ?? "";
    set => totalPurchasedFromSoldToSeg =
      TrimEnd(Substring(value, 1, TotalPurchasedFromSoldToSeg_MaxLength));
  }

  /// <summary>
  /// The json value of the TotalPurchasedFromSoldToSeg attribute.</summary>
  [JsonPropertyName("totalPurchasedFromSoldToSeg")]
  [Computed]
  public string TotalPurchasedFromSoldToSeg_Json
  {
    get => NullIf(TotalPurchasedFromSoldToSeg, "");
    set => TotalPurchasedFromSoldToSeg = value;
  }

  /// <summary>Length of the TOTAL_MORGAGE_INFORMATION_SEGMEN attribute.
  /// </summary>
  public const int TotalMorgageInformationSegmen_MaxLength = 9;

  /// <summary>
  /// The value of the TOTAL_MORGAGE_INFORMATION_SEGMEN attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 35, Type = MemberType.Char, Length
    = TotalMorgageInformationSegmen_MaxLength)]
  public string TotalMorgageInformationSegmen
  {
    get => totalMorgageInformationSegmen ?? "";
    set => totalMorgageInformationSegmen =
      TrimEnd(Substring(value, 1, TotalMorgageInformationSegmen_MaxLength));
  }

  /// <summary>
  /// The json value of the TotalMorgageInformationSegmen attribute.</summary>
  [JsonPropertyName("totalMorgageInformationSegmen")]
  [Computed]
  public string TotalMorgageInformationSegmen_Json
  {
    get => NullIf(TotalMorgageInformationSegmen, "");
    set => TotalMorgageInformationSegmen = value;
  }

  /// <summary>Length of the TOTAL_SPECICALIZED_PAYMENT_INFO attribute.
  /// </summary>
  public const int TotalSpecicalizedPaymentInfo_MaxLength = 9;

  /// <summary>
  /// The value of the TOTAL_SPECICALIZED_PAYMENT_INFO attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 36, Type = MemberType.Char, Length
    = TotalSpecicalizedPaymentInfo_MaxLength)]
  public string TotalSpecicalizedPaymentInfo
  {
    get => totalSpecicalizedPaymentInfo ?? "";
    set => totalSpecicalizedPaymentInfo =
      TrimEnd(Substring(value, 1, TotalSpecicalizedPaymentInfo_MaxLength));
  }

  /// <summary>
  /// The json value of the TotalSpecicalizedPaymentInfo attribute.</summary>
  [JsonPropertyName("totalSpecicalizedPaymentInfo")]
  [Computed]
  public string TotalSpecicalizedPaymentInfo_Json
  {
    get => NullIf(TotalSpecicalizedPaymentInfo, "");
    set => TotalSpecicalizedPaymentInfo = value;
  }

  /// <summary>Length of the TOTAL_CHANGE_SEGEMENTS attribute.</summary>
  public const int TotalChangeSegements_MaxLength = 9;

  /// <summary>
  /// The value of the TOTAL_CHANGE_SEGEMENTS attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 37, Type = MemberType.Char, Length
    = TotalChangeSegements_MaxLength)]
  public string TotalChangeSegements
  {
    get => totalChangeSegements ?? "";
    set => totalChangeSegements =
      TrimEnd(Substring(value, 1, TotalChangeSegements_MaxLength));
  }

  /// <summary>
  /// The json value of the TotalChangeSegements attribute.</summary>
  [JsonPropertyName("totalChangeSegements")]
  [Computed]
  public string TotalChangeSegements_Json
  {
    get => NullIf(TotalChangeSegements, "");
    set => TotalChangeSegements = value;
  }

  /// <summary>Length of the TOTAL_SSNS_ALL_SEGMENTS attribute.</summary>
  public const int TotalSsnsAllSegments_MaxLength = 9;

  /// <summary>
  /// The value of the TOTAL_SSNS_ALL_SEGMENTS attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 38, Type = MemberType.Char, Length
    = TotalSsnsAllSegments_MaxLength)]
  public string TotalSsnsAllSegments
  {
    get => totalSsnsAllSegments ?? "";
    set => totalSsnsAllSegments =
      TrimEnd(Substring(value, 1, TotalSsnsAllSegments_MaxLength));
  }

  /// <summary>
  /// The json value of the TotalSsnsAllSegments attribute.</summary>
  [JsonPropertyName("totalSsnsAllSegments")]
  [Computed]
  public string TotalSsnsAllSegments_Json
  {
    get => NullIf(TotalSsnsAllSegments, "");
    set => TotalSsnsAllSegments = value;
  }

  /// <summary>Length of the TOTAL_SSNS_BASE_SEGMENTS attribute.</summary>
  public const int TotalSsnsBaseSegments_MaxLength = 9;

  /// <summary>
  /// The value of the TOTAL_SSNS_BASE_SEGMENTS attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 39, Type = MemberType.Char, Length
    = TotalSsnsBaseSegments_MaxLength)]
  public string TotalSsnsBaseSegments
  {
    get => totalSsnsBaseSegments ?? "";
    set => totalSsnsBaseSegments =
      TrimEnd(Substring(value, 1, TotalSsnsBaseSegments_MaxLength));
  }

  /// <summary>
  /// The json value of the TotalSsnsBaseSegments attribute.</summary>
  [JsonPropertyName("totalSsnsBaseSegments")]
  [Computed]
  public string TotalSsnsBaseSegments_Json
  {
    get => NullIf(TotalSsnsBaseSegments, "");
    set => TotalSsnsBaseSegments = value;
  }

  /// <summary>Length of the TOTAL_SSNS_J1_SEGEMENTS attribute.</summary>
  public const int TotalSsnsJ1Segements_MaxLength = 9;

  /// <summary>
  /// The value of the TOTAL_SSNS_J1_SEGEMENTS attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 40, Type = MemberType.Char, Length
    = TotalSsnsJ1Segements_MaxLength)]
  public string TotalSsnsJ1Segements
  {
    get => totalSsnsJ1Segements ?? "";
    set => totalSsnsJ1Segements =
      TrimEnd(Substring(value, 1, TotalSsnsJ1Segements_MaxLength));
  }

  /// <summary>
  /// The json value of the TotalSsnsJ1Segements attribute.</summary>
  [JsonPropertyName("totalSsnsJ1Segements")]
  [Computed]
  public string TotalSsnsJ1Segements_Json
  {
    get => NullIf(TotalSsnsJ1Segements, "");
    set => TotalSsnsJ1Segements = value;
  }

  /// <summary>Length of the TOTAL_SSNS_J2_SEGMENTS attribute.</summary>
  public const int TotalSsnsJ2Segments_MaxLength = 9;

  /// <summary>
  /// The value of the TOTAL_SSNS_J2_SEGMENTS attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 41, Type = MemberType.Char, Length
    = TotalSsnsJ2Segments_MaxLength)]
  public string TotalSsnsJ2Segments
  {
    get => totalSsnsJ2Segments ?? "";
    set => totalSsnsJ2Segments =
      TrimEnd(Substring(value, 1, TotalSsnsJ2Segments_MaxLength));
  }

  /// <summary>
  /// The json value of the TotalSsnsJ2Segments attribute.</summary>
  [JsonPropertyName("totalSsnsJ2Segments")]
  [Computed]
  public string TotalSsnsJ2Segments_Json
  {
    get => NullIf(TotalSsnsJ2Segments, "");
    set => TotalSsnsJ2Segments = value;
  }

  /// <summary>Length of the TOTAL_BIRTH_DATES_ALL_SEGMENTS attribute.</summary>
  public const int TotalBirthDatesAllSegments_MaxLength = 9;

  /// <summary>
  /// The value of the TOTAL_BIRTH_DATES_ALL_SEGMENTS attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 42, Type = MemberType.Char, Length
    = TotalBirthDatesAllSegments_MaxLength)]
  public string TotalBirthDatesAllSegments
  {
    get => totalBirthDatesAllSegments ?? "";
    set => totalBirthDatesAllSegments =
      TrimEnd(Substring(value, 1, TotalBirthDatesAllSegments_MaxLength));
  }

  /// <summary>
  /// The json value of the TotalBirthDatesAllSegments attribute.</summary>
  [JsonPropertyName("totalBirthDatesAllSegments")]
  [Computed]
  public string TotalBirthDatesAllSegments_Json
  {
    get => NullIf(TotalBirthDatesAllSegments, "");
    set => TotalBirthDatesAllSegments = value;
  }

  /// <summary>Length of the TOTAL_BIRTH_DATES_BASE_SEGMENTS attribute.
  /// </summary>
  public const int TotalBirthDatesBaseSegments_MaxLength = 9;

  /// <summary>
  /// The value of the TOTAL_BIRTH_DATES_BASE_SEGMENTS attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 43, Type = MemberType.Char, Length
    = TotalBirthDatesBaseSegments_MaxLength)]
  public string TotalBirthDatesBaseSegments
  {
    get => totalBirthDatesBaseSegments ?? "";
    set => totalBirthDatesBaseSegments =
      TrimEnd(Substring(value, 1, TotalBirthDatesBaseSegments_MaxLength));
  }

  /// <summary>
  /// The json value of the TotalBirthDatesBaseSegments attribute.</summary>
  [JsonPropertyName("totalBirthDatesBaseSegments")]
  [Computed]
  public string TotalBirthDatesBaseSegments_Json
  {
    get => NullIf(TotalBirthDatesBaseSegments, "");
    set => TotalBirthDatesBaseSegments = value;
  }

  /// <summary>Length of the TOTAL_BIRTH_DATES_J1_SEGMENTS attribute.</summary>
  public const int TotalBirthDatesJ1Segments_MaxLength = 9;

  /// <summary>
  /// The value of the TOTAL_BIRTH_DATES_J1_SEGMENTS attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 44, Type = MemberType.Char, Length
    = TotalBirthDatesJ1Segments_MaxLength)]
  public string TotalBirthDatesJ1Segments
  {
    get => totalBirthDatesJ1Segments ?? "";
    set => totalBirthDatesJ1Segments =
      TrimEnd(Substring(value, 1, TotalBirthDatesJ1Segments_MaxLength));
  }

  /// <summary>
  /// The json value of the TotalBirthDatesJ1Segments attribute.</summary>
  [JsonPropertyName("totalBirthDatesJ1Segments")]
  [Computed]
  public string TotalBirthDatesJ1Segments_Json
  {
    get => NullIf(TotalBirthDatesJ1Segments, "");
    set => TotalBirthDatesJ1Segments = value;
  }

  /// <summary>Length of the TOTAL_BIRTH_DATES_J2_SEGMENTS attribute.</summary>
  public const int TotalBirthDatesJ2Segments_MaxLength = 9;

  /// <summary>
  /// The value of the TOTAL_BIRTH_DATES_J2_SEGMENTS attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 45, Type = MemberType.Char, Length
    = TotalBirthDatesJ2Segments_MaxLength)]
  public string TotalBirthDatesJ2Segments
  {
    get => totalBirthDatesJ2Segments ?? "";
    set => totalBirthDatesJ2Segments =
      TrimEnd(Substring(value, 1, TotalBirthDatesJ2Segments_MaxLength));
  }

  /// <summary>
  /// The json value of the TotalBirthDatesJ2Segments attribute.</summary>
  [JsonPropertyName("totalBirthDatesJ2Segments")]
  [Computed]
  public string TotalBirthDatesJ2Segments_Json
  {
    get => NullIf(TotalBirthDatesJ2Segments, "");
    set => TotalBirthDatesJ2Segments = value;
  }

  /// <summary>Length of the TOTAL_PHONE_NUMBER_ALL_SEGMENTS attribute.
  /// </summary>
  public const int TotalPhoneNumberAllSegments_MaxLength = 9;

  /// <summary>
  /// The value of the TOTAL_PHONE_NUMBER_ALL_SEGMENTS attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 46, Type = MemberType.Char, Length
    = TotalPhoneNumberAllSegments_MaxLength)]
  public string TotalPhoneNumberAllSegments
  {
    get => totalPhoneNumberAllSegments ?? "";
    set => totalPhoneNumberAllSegments =
      TrimEnd(Substring(value, 1, TotalPhoneNumberAllSegments_MaxLength));
  }

  /// <summary>
  /// The json value of the TotalPhoneNumberAllSegments attribute.</summary>
  [JsonPropertyName("totalPhoneNumberAllSegments")]
  [Computed]
  public string TotalPhoneNumberAllSegments_Json
  {
    get => NullIf(TotalPhoneNumberAllSegments, "");
    set => TotalPhoneNumberAllSegments = value;
  }

  /// <summary>Length of the RESERVED_47 attribute.</summary>
  public const int Reserved47_MaxLength = 19;

  /// <summary>
  /// The value of the RESERVED_47 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 47, Type = MemberType.Char, Length = Reserved47_MaxLength)]
  public string Reserved47
  {
    get => reserved47 ?? "";
    set => reserved47 = TrimEnd(Substring(value, 1, Reserved47_MaxLength));
  }

  /// <summary>
  /// The json value of the Reserved47 attribute.</summary>
  [JsonPropertyName("reserved47")]
  [Computed]
  public string Reserved47_Json
  {
    get => NullIf(Reserved47, "");
    set => Reserved47 = value;
  }

  private string recordDesciptorWord;
  private string recordIdentifier;
  private string totalBaseRecords;
  private string reserved4;
  private string totalOfStatusCodeDf;
  private string totalJ1Segments;
  private string totalJ2Segments;
  private string blockCount;
  private string totalOfStatusCodeDa;
  private string totalOfStatusCode05;
  private string totalOfStatusCode11;
  private string totalOfStatusCode13;
  private string totalOfStatusCode61;
  private string totalOfStatusCode62;
  private string totalOfStatusCode63;
  private string totalOfStatusCode64;
  private string totalOfStatusCode65;
  private string totalOfStatusCode71;
  private string totalOfStatusCode78;
  private string totalOfStatusCode80;
  private string totalOfStatusCode82;
  private string totalOfStatusCode83;
  private string totalOfStatusCode84;
  private string totalOfStatusCode88;
  private string totalOfStatusCode89;
  private string totalOfStatusCode93;
  private string totalOfStatusCode94;
  private string totalOfStatusCode95;
  private string totalOfStatusCode96;
  private string totalOfStatusCode97;
  private string totalOfEcoaCodeZ;
  private string totalEmploymentSegments;
  private string totalOriginalCreditorSegments;
  private string totalPurchasedFromSoldToSeg;
  private string totalMorgageInformationSegmen;
  private string totalSpecicalizedPaymentInfo;
  private string totalChangeSegements;
  private string totalSsnsAllSegments;
  private string totalSsnsBaseSegments;
  private string totalSsnsJ1Segements;
  private string totalSsnsJ2Segments;
  private string totalBirthDatesAllSegments;
  private string totalBirthDatesBaseSegments;
  private string totalBirthDatesJ1Segments;
  private string totalBirthDatesJ2Segments;
  private string totalPhoneNumberAllSegments;
  private string reserved47;
}
