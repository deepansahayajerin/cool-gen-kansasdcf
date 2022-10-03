// The source file: PAYMENT_REQUEST, ID: 371439081, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// This entity records all of the information relevent to paying one or more 
/// disbursements to a single Obligee.  This method of payment is described thru
/// a relationship with Payment Method Type.  The payment method must be either
/// a Warrant, Interfund Voucher or EFT.
/// </summary>
[Serializable]
public partial class PaymentRequest: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public PaymentRequest()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public PaymentRequest(PaymentRequest that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new PaymentRequest Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(PaymentRequest that)
  {
    base.Assign(that);
    systemGeneratedIdentifier = that.systemGeneratedIdentifier;
    type1 = that.type1;
    classification = that.classification;
    imprestFundCode = that.imprestFundCode;
    amount = that.amount;
    processDate = that.processDate;
    csePersonNumber = that.csePersonNumber;
    designatedPayeeCsePersonNo = that.designatedPayeeCsePersonNo;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    interstateInd = that.interstateInd;
    recoupmentIndKpc = that.recoupmentIndKpc;
    recoveryFiller = that.recoveryFiller;
    number = that.number;
    printDate = that.printDate;
    interfundVoucherFiller = that.interfundVoucherFiller;
    achFormatCode = that.achFormatCode;
    recaptureFiller = that.recaptureFiller;
    prqRGeneratedId = that.prqRGeneratedId;
    rctRTstamp = that.rctRTstamp;
    ptpProcessDate = that.ptpProcessDate;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated timestamp that distinguishes one occurrence of 
  /// the entity type from another.
  /// </summary>
  [JsonPropertyName("systemGeneratedIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 9)]
  public int SystemGeneratedIdentifier
  {
    get => systemGeneratedIdentifier;
    set => systemGeneratedIdentifier = value;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int Type1_MaxLength = 3;

  /// <summary>
  /// The value of the TYPE attribute.
  /// This type defines all of the possible payment requests.
  /// Examples include:
  /// WAR
  /// IFV
  /// EFT
  /// RCP
  /// RCV
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = Type1_MaxLength)]
  [Value("RCV")]
  [Value("EFT")]
  [Value("RCP")]
  [Value("WAR")]
  [Value("IFV")]
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

  /// <summary>Length of the CLASSIFICATION attribute.</summary>
  public const int Classification_MaxLength = 3;

  /// <summary>
  /// The value of the CLASSIFICATION attribute.
  /// This field is used to show that the warrant is either a cash receipt 
  /// Refund, a Support (child or spousal) or an Advancement.	
  /// Permitted values are REF, SUP, ADV.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = Classification_MaxLength)]
  public string Classification
  {
    get => classification ?? "";
    set => classification =
      TrimEnd(Substring(value, 1, Classification_MaxLength));
  }

  /// <summary>
  /// The json value of the Classification attribute.</summary>
  [JsonPropertyName("classification")]
  [Computed]
  public string Classification_Json
  {
    get => NullIf(Classification, "");
    set => Classification = value;
  }

  /// <summary>Length of the IMPREST_FUND_CODE attribute.</summary>
  public const int ImprestFundCode_MaxLength = 1;

  /// <summary>
  /// The value of the IMPREST_FUND_CODE attribute.
  /// This field is used to show that the warrant was written from an imprest 
  /// fund so that we will know that it does not need to be put on a tape to
  /// DOA.  A &quot;C&quot; indicates it was a Central imprest fund warrant and
  /// a &quot;L&quot; indicates that it was a Local imprest fund warrant.  A
  /// space will indicate that it is not an imprest fund warrant.
  /// Permitted Values are C, L and space.
  /// </summary>
  [JsonPropertyName("imprestFundCode")]
  [Member(Index = 4, Type = MemberType.Char, Length
    = ImprestFundCode_MaxLength, Optional = true)]
  public string ImprestFundCode
  {
    get => imprestFundCode;
    set => imprestFundCode = value != null
      ? TrimEnd(Substring(value, 1, ImprestFundCode_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the AMOUNT attribute.
  /// Amount of the obligation transaction.
  /// Examples: Amount of a debt, refund, collection or adjustment.
  /// </summary>
  [JsonPropertyName("amount")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 5, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal Amount
  {
    get => amount;
    set => amount = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the PROCESS_DATE attribute.
  /// The process date for which all of the warrants are created.
  /// </summary>
  [JsonPropertyName("processDate")]
  [Member(Index = 6, Type = MemberType.Date)]
  public DateTime? ProcessDate
  {
    get => processDate;
    set => processDate = value;
  }

  /// <summary>Length of the CSE_PERSON_NUMBER attribute.</summary>
  public const int CsePersonNumber_MaxLength = 10;

  /// <summary>
  /// The value of the CSE_PERSON_NUMBER attribute.
  /// This field contains the CSE Person Number of the person on whose behalf 
  /// the warrant is being created.  This may or may not be the same as the CSE
  /// Person who is receiving the check.  If the CSE Person (typically the AR)
  /// has a designated payee then the check will be made out to and sent to the
  /// designated payee on the behalf of the AR.
  /// </summary>
  [JsonPropertyName("csePersonNumber")]
  [Member(Index = 7, Type = MemberType.Char, Length
    = CsePersonNumber_MaxLength, Optional = true)]
  public string CsePersonNumber
  {
    get => csePersonNumber;
    set => csePersonNumber = value != null
      ? TrimEnd(Substring(value, 1, CsePersonNumber_MaxLength)) : null;
  }

  /// <summary>Length of the DESIGNATED_PAYEE_CSE_PERSON_NO attribute.</summary>
  public const int DesignatedPayeeCsePersonNo_MaxLength = 10;

  /// <summary>
  /// The value of the DESIGNATED_PAYEE_CSE_PERSON_NO attribute.
  /// This field contains the CSE Person Number of the designated payee. If the 
  /// CSE Person whe is the payee has a designated payee at the time the warrant
  /// is issued then the warrant will be made out to that designated payee.
  /// </summary>
  [JsonPropertyName("designatedPayeeCsePersonNo")]
  [Member(Index = 8, Type = MemberType.Char, Length
    = DesignatedPayeeCsePersonNo_MaxLength, Optional = true)]
  public string DesignatedPayeeCsePersonNo
  {
    get => designatedPayeeCsePersonNo;
    set => designatedPayeeCsePersonNo = value != null
      ? TrimEnd(Substring(value, 1, DesignatedPayeeCsePersonNo_MaxLength)) : null
      ;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The user id or program id responsible for the creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// The timestamp of when the occurrence of the entity was created.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 10, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>Length of the INTERSTATE_IND attribute.</summary>
  public const int InterstateInd_MaxLength = 1;

  /// <summary>
  /// The value of the INTERSTATE_IND attribute.
  /// This attribute specifies whether or not the payment request is for an 
  /// interstate payment.	The valid values are:			&quot;Y&quot; It is an
  /// interstate payment		&quot;N&quot; It is not an interstate payment.
  /// </summary>
  [JsonPropertyName("interstateInd")]
  [Member(Index = 11, Type = MemberType.Char, Length
    = InterstateInd_MaxLength, Optional = true)]
  public string InterstateInd
  {
    get => interstateInd;
    set => interstateInd = value != null
      ? TrimEnd(Substring(value, 1, InterstateInd_MaxLength)) : null;
  }

  /// <summary>Length of the RECOUPMENT_IND_KPC attribute.</summary>
  public const int RecoupmentIndKpc_MaxLength = 1;

  /// <summary>
  /// The value of the RECOUPMENT_IND_KPC attribute.
  /// Indicates if recoupment was taken by KPC.
  /// </summary>
  [JsonPropertyName("recoupmentIndKpc")]
  [Member(Index = 12, Type = MemberType.Char, Length
    = RecoupmentIndKpc_MaxLength, Optional = true)]
  public string RecoupmentIndKpc
  {
    get => recoupmentIndKpc;
    set => recoupmentIndKpc = value != null
      ? TrimEnd(Substring(value, 1, RecoupmentIndKpc_MaxLength)) : null;
  }

  /// <summary>Length of the RECOVERY_FILLER attribute.</summary>
  public const int RecoveryFiller_MaxLength = 1;

  /// <summary>
  /// The value of the RECOVERY_FILLER attribute.
  /// This is a filler attribute because IEF requires that all subtypes have at 
  /// least one attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length = RecoveryFiller_MaxLength)]
    
  public string RecoveryFiller
  {
    get => recoveryFiller ?? "";
    set => recoveryFiller =
      TrimEnd(Substring(value, 1, RecoveryFiller_MaxLength));
  }

  /// <summary>
  /// The json value of the RecoveryFiller attribute.</summary>
  [JsonPropertyName("recoveryFiller")]
  [Computed]
  public string RecoveryFiller_Json
  {
    get => NullIf(RecoveryFiller, "");
    set => RecoveryFiller = value;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int Number_MaxLength = 9;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// The actual number printed on the check.
  /// </summary>
  [JsonPropertyName("number")]
  [Member(Index = 14, Type = MemberType.Char, Length = Number_MaxLength, Optional
    = true)]
  public string Number
  {
    get => number;
    set => number = value != null
      ? TrimEnd(Substring(value, 1, Number_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the PRINT_DATE attribute.
  /// This date is the approximately the date that the warrant/check was printed
  /// or written out by hand.  For warrants processed through DOA we actually
  /// assume that the print date or issue date is two days after we receive the
  /// DOA file back from DOA.
  /// </summary>
  [JsonPropertyName("printDate")]
  [Member(Index = 15, Type = MemberType.Date, Optional = true)]
  public DateTime? PrintDate
  {
    get => printDate;
    set => printDate = value;
  }

  /// <summary>Length of the INTERFUND_VOUCHER_FILLER attribute.</summary>
  public const int InterfundVoucherFiller_MaxLength = 1;

  /// <summary>
  /// The value of the INTERFUND_VOUCHER_FILLER attribute.
  /// This filler attribute was added because IEF requires that all subtypes 
  /// have at least one attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 16, Type = MemberType.Char, Length
    = InterfundVoucherFiller_MaxLength)]
  public string InterfundVoucherFiller
  {
    get => interfundVoucherFiller ?? "";
    set => interfundVoucherFiller =
      TrimEnd(Substring(value, 1, InterfundVoucherFiller_MaxLength));
  }

  /// <summary>
  /// The json value of the InterfundVoucherFiller attribute.</summary>
  [JsonPropertyName("interfundVoucherFiller")]
  [Computed]
  public string InterfundVoucherFiller_Json
  {
    get => NullIf(InterfundVoucherFiller, "");
    set => InterfundVoucherFiller = value;
  }

  /// <summary>Length of the ACH_FORMAT_CODE attribute.</summary>
  public const int AchFormatCode_MaxLength = 3;

  /// <summary>
  /// The value of the ACH_FORMAT_CODE attribute.
  /// The ACH (Account Clearing House) file format required to transmit this EFT
  /// information to it's RDFI.
  /// </summary>
  [JsonPropertyName("achFormatCode")]
  [Member(Index = 17, Type = MemberType.Char, Length
    = AchFormatCode_MaxLength, Optional = true)]
  public string AchFormatCode
  {
    get => achFormatCode;
    set => achFormatCode = value != null
      ? TrimEnd(Substring(value, 1, AchFormatCode_MaxLength)) : null;
  }

  /// <summary>Length of the RECAPTURE_FILLER attribute.</summary>
  public const int RecaptureFiller_MaxLength = 1;

  /// <summary>
  /// The value of the RECAPTURE_FILLER attribute.
  /// This attribute is a filler because IEF requires that all subtypes have an 
  /// attribute
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 18, Type = MemberType.Char, Length = RecaptureFiller_MaxLength)
    ]
  public string RecaptureFiller
  {
    get => recaptureFiller ?? "";
    set => recaptureFiller =
      TrimEnd(Substring(value, 1, RecaptureFiller_MaxLength));
  }

  /// <summary>
  /// The json value of the RecaptureFiller attribute.</summary>
  [JsonPropertyName("recaptureFiller")]
  [Computed]
  public string RecaptureFiller_Json
  {
    get => NullIf(RecaptureFiller, "");
    set => RecaptureFiller = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated timestamp that distinguishes one occurrence of 
  /// the entity type from another.
  /// </summary>
  [JsonPropertyName("prqRGeneratedId")]
  [Member(Index = 19, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? PrqRGeneratedId
  {
    get => prqRGeneratedId;
    set => prqRGeneratedId = value;
  }

  /// <summary>
  /// The value of the CREATED_TIMESTAMP attribute.
  /// The system time the occurrence was created.
  /// </summary>
  [JsonPropertyName("rctRTstamp")]
  [Member(Index = 20, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? RctRTstamp
  {
    get => rctRTstamp;
    set => rctRTstamp = value;
  }

  /// <summary>
  /// The value of the PROCESS_DATE attribute.
  /// The process date for which all of the warrants are created.
  /// </summary>
  [JsonPropertyName("ptpProcessDate")]
  [Member(Index = 21, Type = MemberType.Date, Optional = true)]
  public DateTime? PtpProcessDate
  {
    get => ptpProcessDate;
    set => ptpProcessDate = value;
  }

  private int systemGeneratedIdentifier;
  private string type1;
  private string classification;
  private string imprestFundCode;
  private decimal amount;
  private DateTime? processDate;
  private string csePersonNumber;
  private string designatedPayeeCsePersonNo;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string interstateInd;
  private string recoupmentIndKpc;
  private string recoveryFiller;
  private string number;
  private DateTime? printDate;
  private string interfundVoucherFiller;
  private string achFormatCode;
  private string recaptureFiller;
  private int? prqRGeneratedId;
  private DateTime? rctRTstamp;
  private DateTime? ptpProcessDate;
}
