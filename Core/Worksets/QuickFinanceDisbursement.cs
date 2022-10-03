// The source file: QUICK_FINANCE_DISBURSEMENT, ID: 374543727, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class QuickFinanceDisbursement: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public QuickFinanceDisbursement()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public QuickFinanceDisbursement(QuickFinanceDisbursement that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new QuickFinanceDisbursement Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(QuickFinanceDisbursement that)
  {
    base.Assign(that);
    date = that.date;
    recipientName = that.recipientName;
    amount = that.amount;
    instrumentNumber = that.instrumentNumber;
  }

  /// <summary>
  /// The value of the DATE attribute.
  /// </summary>
  [JsonPropertyName("date")]
  [Member(Index = 1, Type = MemberType.Date)]
  public DateTime? Date
  {
    get => date;
    set => date = value;
  }

  /// <summary>Length of the RECIPIENT_NAME attribute.</summary>
  public const int RecipientName_MaxLength = 33;

  /// <summary>
  /// The value of the RECIPIENT_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = RecipientName_MaxLength)]
  public string RecipientName
  {
    get => recipientName ?? "";
    set => recipientName =
      TrimEnd(Substring(value, 1, RecipientName_MaxLength));
  }

  /// <summary>
  /// The json value of the RecipientName attribute.</summary>
  [JsonPropertyName("recipientName")]
  [Computed]
  public string RecipientName_Json
  {
    get => NullIf(RecipientName, "");
    set => RecipientName = value;
  }

  /// <summary>
  /// The value of the AMOUNT attribute.
  /// </summary>
  [JsonPropertyName("amount")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 3, Type = MemberType.Number, Length = 11, Precision = 2)]
  public decimal Amount
  {
    get => amount;
    set => amount = Truncate(value, 2);
  }

  /// <summary>Length of the INSTRUMENT_NUMBER attribute.</summary>
  public const int InstrumentNumber_MaxLength = 15;

  /// <summary>
  /// The value of the INSTRUMENT_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = InstrumentNumber_MaxLength)
    ]
  public string InstrumentNumber
  {
    get => instrumentNumber ?? "";
    set => instrumentNumber =
      TrimEnd(Substring(value, 1, InstrumentNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the InstrumentNumber attribute.</summary>
  [JsonPropertyName("instrumentNumber")]
  [Computed]
  public string InstrumentNumber_Json
  {
    get => NullIf(InstrumentNumber, "");
    set => InstrumentNumber = value;
  }

  private DateTime? date;
  private string recipientName;
  private decimal amount;
  private string instrumentNumber;
}
