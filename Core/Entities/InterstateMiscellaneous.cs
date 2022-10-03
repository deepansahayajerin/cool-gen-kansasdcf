// The source file: INTERSTATE_MISCELLANEOUS, ID: 371436402, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVINIT
/// Miscellaneous information about an Interstate case that was received or sent
/// through CSENet.
/// </summary>
[Serializable]
public partial class InterstateMiscellaneous: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public InterstateMiscellaneous()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public InterstateMiscellaneous(InterstateMiscellaneous that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new InterstateMiscellaneous Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(InterstateMiscellaneous that)
  {
    base.Assign(that);
    statusChangeCode = that.statusChangeCode;
    newCaseId = that.newCaseId;
    informationTextLine1 = that.informationTextLine1;
    informationTextLine2 = that.informationTextLine2;
    informationTextLine3 = that.informationTextLine3;
    informationTextLine4 = that.informationTextLine4;
    informationTextLine5 = that.informationTextLine5;
    ccaTransactionDt = that.ccaTransactionDt;
    ccaTransSerNum = that.ccaTransSerNum;
  }

  /// <summary>Length of the STATUS_CHANGE_CODE attribute.</summary>
  public const int StatusChangeCode_MaxLength = 1;

  /// <summary>
  /// The value of the STATUS_CHANGE_CODE attribute.
  /// A code to indicate the latest change to Case status.
  /// O - Open
  /// C - Closed
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = StatusChangeCode_MaxLength)
    ]
  public string StatusChangeCode
  {
    get => statusChangeCode ?? "";
    set => statusChangeCode =
      TrimEnd(Substring(value, 1, StatusChangeCode_MaxLength));
  }

  /// <summary>
  /// The json value of the StatusChangeCode attribute.</summary>
  [JsonPropertyName("statusChangeCode")]
  [Computed]
  public string StatusChangeCode_Json
  {
    get => NullIf(StatusChangeCode, "");
    set => StatusChangeCode = value;
  }

  /// <summary>Length of the NEW_CASE_ID attribute.</summary>
  public const int NewCaseId_MaxLength = 15;

  /// <summary>
  /// The value of the NEW_CASE_ID attribute.
  /// The new Case Id from the other state.
  /// </summary>
  [JsonPropertyName("newCaseId")]
  [Member(Index = 2, Type = MemberType.Char, Length = NewCaseId_MaxLength, Optional
    = true)]
  public string NewCaseId
  {
    get => newCaseId;
    set => newCaseId = value != null
      ? TrimEnd(Substring(value, 1, NewCaseId_MaxLength)) : null;
  }

  /// <summary>Length of the INFORMATION_TEXT_LINE_1 attribute.</summary>
  public const int InformationTextLine1_MaxLength = 80;

  /// <summary>
  /// The value of the INFORMATION_TEXT_LINE_1 attribute.
  /// Misc text field 1
  /// </summary>
  [JsonPropertyName("informationTextLine1")]
  [Member(Index = 3, Type = MemberType.Char, Length
    = InformationTextLine1_MaxLength, Optional = true)]
  public string InformationTextLine1
  {
    get => informationTextLine1;
    set => informationTextLine1 = value != null
      ? TrimEnd(Substring(value, 1, InformationTextLine1_MaxLength)) : null;
  }

  /// <summary>Length of the INFORMATION_TEXT_LINE_2 attribute.</summary>
  public const int InformationTextLine2_MaxLength = 80;

  /// <summary>
  /// The value of the INFORMATION_TEXT_LINE_2 attribute.
  /// Misc text field 2
  /// </summary>
  [JsonPropertyName("informationTextLine2")]
  [Member(Index = 4, Type = MemberType.Char, Length
    = InformationTextLine2_MaxLength, Optional = true)]
  public string InformationTextLine2
  {
    get => informationTextLine2;
    set => informationTextLine2 = value != null
      ? TrimEnd(Substring(value, 1, InformationTextLine2_MaxLength)) : null;
  }

  /// <summary>Length of the INFORMATION_TEXT_LINE_3 attribute.</summary>
  public const int InformationTextLine3_MaxLength = 80;

  /// <summary>
  /// The value of the INFORMATION_TEXT_LINE_3 attribute.
  /// Misc text field 3
  /// </summary>
  [JsonPropertyName("informationTextLine3")]
  [Member(Index = 5, Type = MemberType.Char, Length
    = InformationTextLine3_MaxLength, Optional = true)]
  public string InformationTextLine3
  {
    get => informationTextLine3;
    set => informationTextLine3 = value != null
      ? TrimEnd(Substring(value, 1, InformationTextLine3_MaxLength)) : null;
  }

  /// <summary>Length of the INFORMATION_TEXT_LINE_4 attribute.</summary>
  public const int InformationTextLine4_MaxLength = 80;

  /// <summary>
  /// The value of the INFORMATION_TEXT_LINE_4 attribute.
  /// Miscellaneous text field 4.
  /// </summary>
  [JsonPropertyName("informationTextLine4")]
  [Member(Index = 6, Type = MemberType.Char, Length
    = InformationTextLine4_MaxLength, Optional = true)]
  public string InformationTextLine4
  {
    get => informationTextLine4;
    set => informationTextLine4 = value != null
      ? TrimEnd(Substring(value, 1, InformationTextLine4_MaxLength)) : null;
  }

  /// <summary>Length of the INFORMATION_TEXT_LINE_5 attribute.</summary>
  public const int InformationTextLine5_MaxLength = 80;

  /// <summary>
  /// The value of the INFORMATION_TEXT_LINE_5 attribute.
  /// Miscellaneous text field 5.
  /// </summary>
  [JsonPropertyName("informationTextLine5")]
  [Member(Index = 7, Type = MemberType.Char, Length
    = InformationTextLine5_MaxLength, Optional = true)]
  public string InformationTextLine5
  {
    get => informationTextLine5;
    set => informationTextLine5 = value != null
      ? TrimEnd(Substring(value, 1, InformationTextLine5_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the TRANSACTION_DATE attribute.
  /// This is the date on which CSENet transmitted the Referral.
  /// </summary>
  [JsonPropertyName("ccaTransactionDt")]
  [Member(Index = 8, Type = MemberType.Date)]
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
  [Member(Index = 9, Type = MemberType.Number, Length = 12)]
  public long CcaTransSerNum
  {
    get => ccaTransSerNum;
    set => ccaTransSerNum = value;
  }

  private string statusChangeCode;
  private string newCaseId;
  private string informationTextLine1;
  private string informationTextLine2;
  private string informationTextLine3;
  private string informationTextLine4;
  private string informationTextLine5;
  private DateTime? ccaTransactionDt;
  private long ccaTransSerNum;
}
