// The source file: OBLIGATION_RECORD, ID: 374396762, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class ObligationRecord: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ObligationRecord()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ObligationRecord(ObligationRecord that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ObligationRecord Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ObligationRecord that)
  {
    base.Assign(that);
    recordType = that.recordType;
    amount = that.amount;
    frequency = that.frequency;
    startDate = that.startDate;
    endDate = that.endDate;
    seasonalFlag = that.seasonalFlag;
    filler = that.filler;
    newAmount = that.newAmount;
  }

  /// <summary>Length of the RECORD_TYPE attribute.</summary>
  public const int RecordType_MaxLength = 1;

  /// <summary>
  /// The value of the RECORD_TYPE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = RecordType_MaxLength)]
  public string RecordType
  {
    get => recordType ?? "";
    set => recordType = TrimEnd(Substring(value, 1, RecordType_MaxLength));
  }

  /// <summary>
  /// The json value of the RecordType attribute.</summary>
  [JsonPropertyName("recordType")]
  [Computed]
  public string RecordType_Json
  {
    get => NullIf(RecordType, "");
    set => RecordType = value;
  }

  /// <summary>Length of the AMOUNT attribute.</summary>
  public const int Amount_MaxLength = 10;

  /// <summary>
  /// The value of the AMOUNT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = Amount_MaxLength)]
  public string Amount
  {
    get => amount ?? "";
    set => amount = TrimEnd(Substring(value, 1, Amount_MaxLength));
  }

  /// <summary>
  /// The json value of the Amount attribute.</summary>
  [JsonPropertyName("amount")]
  [Computed]
  public string Amount_Json
  {
    get => NullIf(Amount, "");
    set => Amount = value;
  }

  /// <summary>Length of the FREQUENCY attribute.</summary>
  public const int Frequency_MaxLength = 12;

  /// <summary>
  /// The value of the FREQUENCY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = Frequency_MaxLength)]
  public string Frequency
  {
    get => frequency ?? "";
    set => frequency = TrimEnd(Substring(value, 1, Frequency_MaxLength));
  }

  /// <summary>
  /// The json value of the Frequency attribute.</summary>
  [JsonPropertyName("frequency")]
  [Computed]
  public string Frequency_Json
  {
    get => NullIf(Frequency, "");
    set => Frequency = value;
  }

  /// <summary>Length of the START_DATE attribute.</summary>
  public const int StartDate_MaxLength = 8;

  /// <summary>
  /// The value of the START_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = StartDate_MaxLength)]
  public string StartDate
  {
    get => startDate ?? "";
    set => startDate = TrimEnd(Substring(value, 1, StartDate_MaxLength));
  }

  /// <summary>
  /// The json value of the StartDate attribute.</summary>
  [JsonPropertyName("startDate")]
  [Computed]
  public string StartDate_Json
  {
    get => NullIf(StartDate, "");
    set => StartDate = value;
  }

  /// <summary>Length of the END_DATE attribute.</summary>
  public const int EndDate_MaxLength = 8;

  /// <summary>
  /// The value of the END_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = EndDate_MaxLength)]
  public string EndDate
  {
    get => endDate ?? "";
    set => endDate = TrimEnd(Substring(value, 1, EndDate_MaxLength));
  }

  /// <summary>
  /// The json value of the EndDate attribute.</summary>
  [JsonPropertyName("endDate")]
  [Computed]
  public string EndDate_Json
  {
    get => NullIf(EndDate, "");
    set => EndDate = value;
  }

  /// <summary>Length of the SEASONAL_FLAG attribute.</summary>
  public const int SeasonalFlag_MaxLength = 1;

  /// <summary>
  /// The value of the SEASONAL_FLAG attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = SeasonalFlag_MaxLength)]
  public string SeasonalFlag
  {
    get => seasonalFlag ?? "";
    set => seasonalFlag = TrimEnd(Substring(value, 1, SeasonalFlag_MaxLength));
  }

  /// <summary>
  /// The json value of the SeasonalFlag attribute.</summary>
  [JsonPropertyName("seasonalFlag")]
  [Computed]
  public string SeasonalFlag_Json
  {
    get => NullIf(SeasonalFlag, "");
    set => SeasonalFlag = value;
  }

  /// <summary>Length of the FILLER attribute.</summary>
  public const int Filler_MaxLength = 156;

  /// <summary>
  /// The value of the FILLER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = Filler_MaxLength)]
  public string Filler
  {
    get => filler ?? "";
    set => filler = TrimEnd(Substring(value, 1, Filler_MaxLength));
  }

  /// <summary>
  /// The json value of the Filler attribute.</summary>
  [JsonPropertyName("filler")]
  [Computed]
  public string Filler_Json
  {
    get => NullIf(Filler, "");
    set => Filler = value;
  }

  /// <summary>
  /// The value of the NEW_AMOUNT attribute.
  /// </summary>
  [JsonPropertyName("newAmount")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 8, Type = MemberType.Number, Length = 8, Precision = 2)]
  public decimal NewAmount
  {
    get => newAmount;
    set => newAmount = Truncate(value, 2);
  }

  private string recordType;
  private string amount;
  private string frequency;
  private string startDate;
  private string endDate;
  private string seasonalFlag;
  private string filler;
  private decimal newAmount;
}
