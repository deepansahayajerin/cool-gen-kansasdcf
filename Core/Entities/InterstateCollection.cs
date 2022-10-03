// The source file: INTERSTATE_COLLECTION, ID: 371436319, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVINIT
/// Information about interstate collections received or sent on an interstate 
/// case and transmitted through CSENet.
/// </summary>
[Serializable]
public partial class InterstateCollection: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public InterstateCollection()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public InterstateCollection(InterstateCollection that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new InterstateCollection Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(InterstateCollection that)
  {
    base.Assign(that);
    systemGeneratedSequenceNum = that.systemGeneratedSequenceNum;
    dateOfCollection = that.dateOfCollection;
    dateOfPosting = that.dateOfPosting;
    paymentAmount = that.paymentAmount;
    paymentSource = that.paymentSource;
    interstatePaymentMethod = that.interstatePaymentMethod;
    rdfiId = that.rdfiId;
    rdfiAccountNum = that.rdfiAccountNum;
    ccaTransactionDt = that.ccaTransactionDt;
    ccaTransSerNum = that.ccaTransSerNum;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_SEQUENCE_NUM attribute.
  /// </summary>
  [JsonPropertyName("systemGeneratedSequenceNum")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 1)]
  public int SystemGeneratedSequenceNum
  {
    get => systemGeneratedSequenceNum;
    set => systemGeneratedSequenceNum = value;
  }

  /// <summary>
  /// The value of the DATE_OF_COLLECTION attribute.
  /// Date this Collection was made.
  /// </summary>
  [JsonPropertyName("dateOfCollection")]
  [Member(Index = 2, Type = MemberType.Date, Optional = true)]
  public DateTime? DateOfCollection
  {
    get => dateOfCollection;
    set => dateOfCollection = value;
  }

  /// <summary>
  /// The value of the DATE_OF_POSTING attribute.
  /// Date this collection was posted.  Must be greater than collection date if 
  /// both entered.
  /// </summary>
  [JsonPropertyName("dateOfPosting")]
  [Member(Index = 3, Type = MemberType.Date, Optional = true)]
  public DateTime? DateOfPosting
  {
    get => dateOfPosting;
    set => dateOfPosting = value;
  }

  /// <summary>
  /// The value of the PAYMENT_AMOUNT attribute.
  /// Total amount of collected payment
  /// </summary>
  [JsonPropertyName("paymentAmount")]
  [Member(Index = 4, Type = MemberType.Number, Length = 8, Precision = 2, Optional
    = true)]
  public decimal? PaymentAmount
  {
    get => paymentAmount;
    set => paymentAmount = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>Length of the PAYMENT_SOURCE attribute.</summary>
  public const int PaymentSource_MaxLength = 1;

  /// <summary>
  /// The value of the PAYMENT_SOURCE attribute.
  /// Interstate payment source:
  /// A - Wage Assignment
  /// G - Garnishment
  /// I - IRS Tax Intercept
  /// S - State Tax Intercept
  /// U - UIB Intercept
  /// N - Normal
  /// O - Other
  /// W - Workman's Compensation
  /// </summary>
  [JsonPropertyName("paymentSource")]
  [Member(Index = 5, Type = MemberType.Char, Length = PaymentSource_MaxLength, Optional
    = true)]
  public string PaymentSource
  {
    get => paymentSource;
    set => paymentSource = value != null
      ? TrimEnd(Substring(value, 1, PaymentSource_MaxLength)) : null;
  }

  /// <summary>Length of the INTERSTATE_PAYMENT_METHOD attribute.</summary>
  public const int InterstatePaymentMethod_MaxLength = 1;

  /// <summary>
  /// The value of the INTERSTATE_PAYMENT_METHOD attribute.
  /// Method of interstate payment:
  /// E - EFT
  /// M - Manual
  /// O - Other
  /// </summary>
  [JsonPropertyName("interstatePaymentMethod")]
  [Member(Index = 6, Type = MemberType.Char, Length
    = InterstatePaymentMethod_MaxLength, Optional = true)]
  public string InterstatePaymentMethod
  {
    get => interstatePaymentMethod;
    set => interstatePaymentMethod = value != null
      ? TrimEnd(Substring(value, 1, InterstatePaymentMethod_MaxLength)) : null;
  }

  /// <summary>Length of the RDFI_ID attribute.</summary>
  public const int RdfiId_MaxLength = 20;

  /// <summary>
  /// The value of the RDFI_ID attribute.
  /// Reserved for EFT use
  /// </summary>
  [JsonPropertyName("rdfiId")]
  [Member(Index = 7, Type = MemberType.Char, Length = RdfiId_MaxLength, Optional
    = true)]
  public string RdfiId
  {
    get => rdfiId;
    set => rdfiId = value != null
      ? TrimEnd(Substring(value, 1, RdfiId_MaxLength)) : null;
  }

  /// <summary>Length of the RDFI_ACCOUNT_NUM attribute.</summary>
  public const int RdfiAccountNum_MaxLength = 20;

  /// <summary>
  /// The value of the RDFI_ACCOUNT_NUM attribute.
  /// Reserved for EFT use.
  /// </summary>
  [JsonPropertyName("rdfiAccountNum")]
  [Member(Index = 8, Type = MemberType.Char, Length
    = RdfiAccountNum_MaxLength, Optional = true)]
  public string RdfiAccountNum
  {
    get => rdfiAccountNum;
    set => rdfiAccountNum = value != null
      ? TrimEnd(Substring(value, 1, RdfiAccountNum_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the TRANSACTION_DATE attribute.
  /// This is the date on which CSENet transmitted the Referral.
  /// </summary>
  [JsonPropertyName("ccaTransactionDt")]
  [Member(Index = 9, Type = MemberType.Date)]
  public DateTime? CcaTransactionDt
  {
    get => ccaTransactionDt;
    set => ccaTransactionDt = value;
  }

  /// <summary>
  /// The value of the TRANS_SERIAL_NUMBER attribute.
  /// This is a unique number assigned to each CSENet transaction.  It has no 
  /// place in the KESSEP system but is required to provide a key used to
  /// process CSENet Referrals.
  /// </summary>
  [JsonPropertyName("ccaTransSerNum")]
  [DefaultValue(0L)]
  [Member(Index = 10, Type = MemberType.Number, Length = 12)]
  public long CcaTransSerNum
  {
    get => ccaTransSerNum;
    set => ccaTransSerNum = value;
  }

  private int systemGeneratedSequenceNum;
  private DateTime? dateOfCollection;
  private DateTime? dateOfPosting;
  private decimal? paymentAmount;
  private string paymentSource;
  private string interstatePaymentMethod;
  private string rdfiId;
  private string rdfiAccountNum;
  private DateTime? ccaTransactionDt;
  private long ccaTransSerNum;
}
