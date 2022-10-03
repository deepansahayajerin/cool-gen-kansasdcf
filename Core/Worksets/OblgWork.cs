// The source file: OBLG_WORK, ID: 371790904, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

/// <summary>
/// RESP: OBLGESTB
/// This work set attributes are for OBLIGATION ESTABLISHMENT PROCEDURES and 
/// ACTION BLOCKS.
/// </summary>
[Serializable]
public partial class OblgWork: Heap, ICloneable
{
  /// <summary>Default constructor.</summary>
  public OblgWork()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public OblgWork(OblgWork that):
    base(that)
  {
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new OblgWork Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Length of the IM_CASE attribute.</summary>
  public const int ImCase_MaxLength = 8;

  /// <summary>
  /// The value of the IM_CASE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = ImCase_MaxLength)]
  public string ImCase
  {
    get => Get<string>("imCase") ?? "";
    set => Set(
      "imCase", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ImCase_MaxLength)));
  }

  /// <summary>
  /// The json value of the ImCase attribute.</summary>
  [JsonPropertyName("imCase")]
  [Computed]
  public string ImCase_Json
  {
    get => NullIf(ImCase, "");
    set => ImCase = value;
  }

  /// <summary>
  /// The value of the IM_BENIFIT attribute.
  /// </summary>
  [JsonPropertyName("imBenifit")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 2, Type = MemberType.Number, Length = 6, Precision = 2)]
  public decimal ImBenifit
  {
    get => Get<decimal?>("imBenifit") ?? 0M;
    set =>
      Set("imBenifit", value == 0M ? null : Truncate(value, 2) as decimal?);
  }

  /// <summary>
  /// The value of the IM_IMPREST attribute.
  /// </summary>
  [JsonPropertyName("imImprest")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 3, Type = MemberType.Number, Length = 6, Precision = 2)]
  public decimal ImImprest
  {
    get => Get<decimal?>("imImprest") ?? 0M;
    set =>
      Set("imImprest", value == 0M ? null : Truncate(value, 2) as decimal?);
  }

  /// <summary>
  /// The value of the IM_ALLOW attribute.
  /// </summary>
  [JsonPropertyName("imAllow")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 4, Type = MemberType.Number, Length = 6, Precision = 2)]
  public decimal ImAllow
  {
    get => Get<decimal?>("imAllow") ?? 0M;
    set => Set("imAllow", value == 0M ? null : Truncate(value, 2) as decimal?);
  }

  /// <summary>
  /// The value of the IM_ISS_AMT attribute.
  /// </summary>
  [JsonPropertyName("imIssAmt")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 5, Type = MemberType.Number, Length = 6, Precision = 2)]
  public decimal ImIssAmt
  {
    get => Get<decimal?>("imIssAmt") ?? 0M;
    set => Set("imIssAmt", value == 0M ? null : Truncate(value, 2) as decimal?);
  }

  /// <summary>Length of the ADDR_ST attribute.</summary>
  public const int AddrSt_MaxLength = 2;

  /// <summary>
  /// The value of the ADDR_ST attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = AddrSt_MaxLength)]
  public string AddrSt
  {
    get => Get<string>("addrSt") ?? "";
    set => Set(
      "addrSt", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, AddrSt_MaxLength)));
  }

  /// <summary>
  /// The json value of the AddrSt attribute.</summary>
  [JsonPropertyName("addrSt")]
  [Computed]
  public string AddrSt_Json
  {
    get => NullIf(AddrSt, "");
    set => AddrSt = value;
  }

  /// <summary>Length of the ADDR_CITY attribute.</summary>
  public const int AddrCity_MaxLength = 25;

  /// <summary>
  /// The value of the ADDR_CITY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = AddrCity_MaxLength)]
  public string AddrCity
  {
    get => Get<string>("addrCity") ?? "";
    set => Set(
      "addrCity", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, AddrCity_MaxLength)));
  }

  /// <summary>
  /// The json value of the AddrCity attribute.</summary>
  [JsonPropertyName("addrCity")]
  [Computed]
  public string AddrCity_Json
  {
    get => NullIf(AddrCity, "");
    set => AddrCity = value;
  }

  /// <summary>Length of the ADDR_ZIP_4 attribute.</summary>
  public const int AddrZip4_MaxLength = 4;

  /// <summary>
  /// The value of the ADDR_ZIP_4 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = AddrZip4_MaxLength)]
  public string AddrZip4
  {
    get => Get<string>("addrZip4") ?? "";
    set => Set(
      "addrZip4", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, AddrZip4_MaxLength)));
  }

  /// <summary>
  /// The json value of the AddrZip4 attribute.</summary>
  [JsonPropertyName("addrZip4")]
  [Computed]
  public string AddrZip4_Json
  {
    get => NullIf(AddrZip4, "");
    set => AddrZip4 = value;
  }

  /// <summary>Length of the ADDR_ZIP_5 attribute.</summary>
  public const int AddrZip5_MaxLength = 5;

  /// <summary>
  /// The value of the ADDR_ZIP_5 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = AddrZip5_MaxLength)]
  public string AddrZip5
  {
    get => Get<string>("addrZip5") ?? "";
    set => Set(
      "addrZip5", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, AddrZip5_MaxLength)));
  }

  /// <summary>
  /// The json value of the AddrZip5 attribute.</summary>
  [JsonPropertyName("addrZip5")]
  [Computed]
  public string AddrZip5_Json
  {
    get => NullIf(AddrZip5, "");
    set => AddrZip5 = value;
  }

  /// <summary>
  /// The value of the ADDR_LENGTH attribute.
  /// </summary>
  [JsonPropertyName("addrLength")]
  [DefaultValue(0)]
  [Member(Index = 10, Type = MemberType.Number, Length = 3)]
  public int AddrLength
  {
    get => Get<int?>("addrLength") ?? 0;
    set => Set("addrLength", value == 0 ? null : value as int?);
  }

  /// <summary>
  /// The value of the YYYYMM attribute.
  /// Year and month format.
  /// </summary>
  [JsonPropertyName("yyyymm")]
  [DefaultValue(0)]
  [Member(Index = 11, Type = MemberType.Number, Length = 6)]
  public int Yyyymm
  {
    get => Get<int?>("yyyymm") ?? 0;
    set => Set("yyyymm", value == 0 ? null : value as int?);
  }

  /// <summary>
  /// The value of the HOUSE_MEM_CNT attribute.
  /// </summary>
  [JsonPropertyName("houseMemCnt")]
  [DefaultValue(0)]
  [Member(Index = 12, Type = MemberType.Number, Length = 2)]
  public int HouseMemCnt
  {
    get => Get<int?>("houseMemCnt") ?? 0;
    set => Set("houseMemCnt", value == 0 ? null : value as int?);
  }

  /// <summary>
  /// The value of the MONTH_PASS_TTL attribute.
  /// </summary>
  [JsonPropertyName("monthPassTtl")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 13, Type = MemberType.Number, Length = 5, Precision = 2)]
  public decimal MonthPassTtl
  {
    get => Get<decimal?>("monthPassTtl") ?? 0M;
    set => Set(
      "monthPassTtl", value == 0M ? null : Truncate(value, 2) as decimal?);
  }

  /// <summary>
  /// The value of the CHILD_PASS_TR attribute.
  /// </summary>
  [JsonPropertyName("childPassTr")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 14, Type = MemberType.Number, Length = 4, Precision = 2)]
  public decimal ChildPassTr
  {
    get => Get<decimal?>("childPassTr") ?? 0M;
    set => Set(
      "childPassTr", value == 0M ? null : Truncate(value, 2) as decimal?);
  }

  /// <summary>
  /// The value of the TOTAL_GRNT attribute.
  /// </summary>
  [JsonPropertyName("totalGrnt")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 15, Type = MemberType.Number, Length = 8, Precision = 2)]
  public decimal TotalGrnt
  {
    get => Get<decimal?>("totalGrnt") ?? 0M;
    set =>
      Set("totalGrnt", value == 0M ? null : Truncate(value, 2) as decimal?);
  }

  /// <summary>
  /// The value of the DET_PERS_URAA attribute.
  /// </summary>
  [JsonPropertyName("detPersUraa")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 16, Type = MemberType.Number, Length = 6, Precision = 2)]
  public decimal DetPersUraa
  {
    get => Get<decimal?>("detPersUraa") ?? 0M;
    set => Set(
      "detPersUraa", value == 0M ? null : Truncate(value, 2) as decimal?);
  }

  /// <summary>
  /// The value of the DET_MNTH_COLL attribute.
  /// </summary>
  [JsonPropertyName("detMnthColl")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 17, Type = MemberType.Number, Length = 6, Precision = 2)]
  public decimal DetMnthColl
  {
    get => Get<decimal?>("detMnthColl") ?? 0M;
    set => Set(
      "detMnthColl", value == 0M ? null : Truncate(value, 2) as decimal?);
  }

  /// <summary>
  /// The value of the DET_PASS_THRU attribute.
  /// </summary>
  [JsonPropertyName("detPassThru")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 18, Type = MemberType.Number, Length = 6, Precision = 2)]
  public decimal DetPassThru
  {
    get => Get<decimal?>("detPassThru") ?? 0M;
    set => Set(
      "detPassThru", value == 0M ? null : Truncate(value, 2) as decimal?);
  }

  /// <summary>
  /// The value of the DET_MEDI_URAA attribute.
  /// </summary>
  [JsonPropertyName("detMediUraa")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 19, Type = MemberType.Number, Length = 6, Precision = 2)]
  public decimal DetMediUraa
  {
    get => Get<decimal?>("detMediUraa") ?? 0M;
    set => Set(
      "detMediUraa", value == 0M ? null : Truncate(value, 2) as decimal?);
  }

  /// <summary>
  /// The value of the DET_PERS_GRNT attribute.
  /// </summary>
  [JsonPropertyName("detPersGrnt")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 20, Type = MemberType.Number, Length = 6, Precision = 2)]
  public decimal DetPersGrnt
  {
    get => Get<decimal?>("detPersGrnt") ?? 0M;
    set => Set(
      "detPersGrnt", value == 0M ? null : Truncate(value, 2) as decimal?);
  }

  /// <summary>
  /// The value of the PROC_MONTH attribute.
  /// </summary>
  [JsonPropertyName("procMonth")]
  [Member(Index = 21, Type = MemberType.Date)]
  public DateTime? ProcMonth
  {
    get => Get<DateTime?>("procMonth");
    set => Set("procMonth", value);
  }

  /// <summary>
  /// The value of the WORK_DATE attribute.
  /// </summary>
  [JsonPropertyName("workDate")]
  [DefaultValue(0)]
  [Member(Index = 22, Type = MemberType.Number, Length = 8)]
  public int WorkDate
  {
    get => Get<int?>("workDate") ?? 0;
    set => Set("workDate", value == 0 ? null : value as int?);
  }

  /// <summary>Length of the MONTH_YEAR attribute.</summary>
  public const int MonthYear_MaxLength = 8;

  /// <summary>
  /// The value of the MONTH_YEAR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 23, Type = MemberType.Char, Length = MonthYear_MaxLength)]
  public string MonthYear
  {
    get => Get<string>("monthYear") ?? "";
    set => Set(
      "monthYear", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, MonthYear_MaxLength)));
  }

  /// <summary>
  /// The json value of the MonthYear attribute.</summary>
  [JsonPropertyName("monthYear")]
  [Computed]
  public string MonthYear_Json
  {
    get => NullIf(MonthYear, "");
    set => MonthYear = value;
  }

  /// <summary>Length of the TEXT_DATE attribute.</summary>
  public const int TextDate_MaxLength = 8;

  /// <summary>
  /// The value of the TEXT_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 24, Type = MemberType.Char, Length = TextDate_MaxLength)]
  public string TextDate
  {
    get => Get<string>("textDate") ?? "";
    set => Set(
      "textDate", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, TextDate_MaxLength)));
  }

  /// <summary>
  /// The json value of the TextDate attribute.</summary>
  [JsonPropertyName("textDate")]
  [Computed]
  public string TextDate_Json
  {
    get => NullIf(TextDate, "");
    set => TextDate = value;
  }

  /// <summary>Length of the TEXT_TIME attribute.</summary>
  public const int TextTime_MaxLength = 6;

  /// <summary>
  /// The value of the TEXT_TIME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 25, Type = MemberType.Char, Length = TextTime_MaxLength)]
  public string TextTime
  {
    get => Get<string>("textTime") ?? "";
    set => Set(
      "textTime", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, TextTime_MaxLength)));
  }

  /// <summary>
  /// The json value of the TextTime attribute.</summary>
  [JsonPropertyName("textTime")]
  [Computed]
  public string TextTime_Json
  {
    get => NullIf(TextTime, "");
    set => TextTime = value;
  }

  /// <summary>Length of the TEXT_MILLISECOND attribute.</summary>
  public const int TextMillisecond_MaxLength = 6;

  /// <summary>
  /// The value of the TEXT_MILLISECOND attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 26, Type = MemberType.Char, Length = TextMillisecond_MaxLength)
    ]
  public string TextMillisecond
  {
    get => Get<string>("textMillisecond") ?? "";
    set => Set(
      "textMillisecond", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, TextMillisecond_MaxLength)));
  }

  /// <summary>
  /// The json value of the TextMillisecond attribute.</summary>
  [JsonPropertyName("textMillisecond")]
  [Computed]
  public string TextMillisecond_Json
  {
    get => NullIf(TextMillisecond, "");
    set => TextMillisecond = value;
  }

  /// <summary>Length of the TEXT_TIMESTAMP attribute.</summary>
  public const int TextTimestamp_MaxLength = 20;

  /// <summary>
  /// The value of the TEXT_TIMESTAMP attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 27, Type = MemberType.Char, Length = TextTimestamp_MaxLength)]
  public string TextTimestamp
  {
    get => Get<string>("textTimestamp") ?? "";
    set => Set(
      "textTimestamp", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, TextTimestamp_MaxLength)));
  }

  /// <summary>
  /// The json value of the TextTimestamp attribute.</summary>
  [JsonPropertyName("textTimestamp")]
  [Computed]
  public string TextTimestamp_Json
  {
    get => NullIf(TextTimestamp, "");
    set => TextTimestamp = value;
  }

  /// <summary>
  /// The value of the PERS_URA attribute.
  /// This work set is used to compute individuals URA for DISPLAY-HOUSEHOLD-URA
  /// Procedure.
  /// </summary>
  [JsonPropertyName("persUra")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 28, Type = MemberType.Number, Length = 6, Precision = 2)]
  public decimal PersUra
  {
    get => Get<decimal?>("persUra") ?? 0M;
    set => Set("persUra", value == 0M ? null : Truncate(value, 2) as decimal?);
  }

  /// <summary>
  /// The value of the MONTH attribute.
  /// </summary>
  [JsonPropertyName("month")]
  [DefaultValue(0)]
  [Member(Index = 29, Type = MemberType.Number, Length = 2)]
  public int Month
  {
    get => Get<int?>("month") ?? 0;
    set => Set("month", value == 0 ? null : value as int?);
  }

  /// <summary>
  /// The value of the YEAR attribute.
  /// </summary>
  [JsonPropertyName("year")]
  [DefaultValue(0)]
  [Member(Index = 30, Type = MemberType.Number, Length = 4)]
  public int Year
  {
    get => Get<int?>("year") ?? 0;
    set => Set("year", value == 0 ? null : value as int?);
  }

  /// <summary>
  /// The value of the TOTAL_PURA attribute.
  /// This work set is used to compute total ura for household for DISPLAY-
  /// HOUSEHOLD-URA.
  /// </summary>
  [JsonPropertyName("totalPura")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 31, Type = MemberType.Number, Length = 8, Precision = 2)]
  public decimal TotalPura
  {
    get => Get<decimal?>("totalPura") ?? 0M;
    set =>
      Set("totalPura", value == 0M ? null : Truncate(value, 2) as decimal?);
  }

  /// <summary>
  /// The value of the TOTAL_MEDI attribute.
  /// This work set is used to compute total medical ura for household for 
  /// DISPLAY-HOUSEHOLD-URA.
  /// </summary>
  [JsonPropertyName("totalMedi")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 32, Type = MemberType.Number, Length = 8, Precision = 2)]
  public decimal TotalMedi
  {
    get => Get<decimal?>("totalMedi") ?? 0M;
    set =>
      Set("totalMedi", value == 0M ? null : Truncate(value, 2) as decimal?);
  }

  /// <summary>
  /// The value of the TOTAL_COLL attribute.
  /// This work set is used to compute total collection amount for DISPLAY-
  /// HOUSEHOLD-URA Procedure.
  /// </summary>
  [JsonPropertyName("totalColl")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 33, Type = MemberType.Number, Length = 8, Precision = 2)]
  public decimal TotalColl
  {
    get => Get<decimal?>("totalColl") ?? 0M;
    set =>
      Set("totalColl", value == 0M ? null : Truncate(value, 2) as decimal?);
  }

  /// <summary>
  /// The value of the TOTAL_PASS attribute.
  /// This work set is used to compute total pass-thru amount for DISPLAY-
  /// HOUSEHOLD-URA Procedure.
  /// </summary>
  [JsonPropertyName("totalPass")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 34, Type = MemberType.Number, Length = 8, Precision = 2)]
  public decimal TotalPass
  {
    get => Get<decimal?>("totalPass") ?? 0M;
    set =>
      Set("totalPass", value == 0M ? null : Truncate(value, 2) as decimal?);
  }

  /// <summary>
  /// The value of the TOTAL_SUMG attribute.
  /// This work set is used to compute total grant amount for DISPLAY-HOUSEHOLD-
  /// URA procedure.
  /// </summary>
  [JsonPropertyName("totalSumg")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 35, Type = MemberType.Number, Length = 8, Precision = 2)]
  public decimal TotalSumg
  {
    get => Get<decimal?>("totalSumg") ?? 0M;
    set =>
      Set("totalSumg", value == 0M ? null : Truncate(value, 2) as decimal?);
  }

  /// <summary>Length of the STATE attribute.</summary>
  public const int State_MaxLength = 2;

  /// <summary>
  /// The value of the STATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 36, Type = MemberType.Char, Length = State_MaxLength)]
  public string State
  {
    get => Get<string>("state") ?? "";
    set => Set(
      "state", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, State_MaxLength)));
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

  /// <summary>Length of the ADDR_ZIP attribute.</summary>
  public const int AddrZip_MaxLength = 9;

  /// <summary>
  /// The value of the ADDR_ZIP attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 37, Type = MemberType.Char, Length = AddrZip_MaxLength)]
  public string AddrZip
  {
    get => Get<string>("addrZip") ?? "";
    set => Set(
      "addrZip", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, AddrZip_MaxLength)));
  }

  /// <summary>
  /// The json value of the AddrZip attribute.</summary>
  [JsonPropertyName("addrZip")]
  [Computed]
  public string AddrZip_Json
  {
    get => NullIf(AddrZip, "");
    set => AddrZip = value;
  }

  /// <summary>Length of the ADDR_TEXT attribute.</summary>
  public const int AddrText_MaxLength = 185;

  /// <summary>
  /// The value of the ADDR_TEXT attribute.
  /// This ADDR-TEXT is used as work area for FPLS-LOCATE-RESPONSE to substring 
  /// into address array delemeted by '\'.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 38, Type = MemberType.Char, Length = AddrText_MaxLength)]
  public string AddrText
  {
    get => Get<string>("addrText") ?? "";
    set => Set(
      "addrText", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, AddrText_MaxLength)));
  }

  /// <summary>
  /// The json value of the AddrText attribute.</summary>
  [JsonPropertyName("addrText")]
  [Computed]
  public string AddrText_Json
  {
    get => NullIf(AddrText, "");
    set => AddrText = value;
  }
}
