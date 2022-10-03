// The source file: INTERFACE_PAYMENT_HISTORY_REQUES, ID: 372291424, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANACE
/// 
/// A designer added entity type which will record all user requests (
/// initiated on the REIP screen) to convert KAECSES payment history data
/// for a specified Obligor CSE Person Number and Court Order Number to
/// the KESSEP database. This table will provide a tracking mechanism for
/// payment history data conversion activity.
/// </summary>
[Serializable]
public partial class InterfacePaymentHistoryReques: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public InterfacePaymentHistoryReques()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public InterfacePaymentHistoryReques(InterfacePaymentHistoryReques that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new InterfacePaymentHistoryReques Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(InterfacePaymentHistoryReques that)
  {
    base.Assign(that);
    obligorCsePersonNumber = that.obligorCsePersonNumber;
    kaecsesCourtOrderNumber = that.kaecsesCourtOrderNumber;
    requestingUserId = that.requestingUserId;
    dateRequested = that.dateRequested;
    dateProcessed = that.dateProcessed;
    successfullyConverted = that.successfullyConverted;
  }

  /// <summary>Length of the OBLIGOR_CSE_PERSON_NUMBER attribute.</summary>
  public const int ObligorCsePersonNumber_MaxLength = 10;

  /// <summary>
  /// The value of the OBLIGOR_CSE_PERSON_NUMBER attribute.
  /// The CSE Person Number of the obligor associated with the KAECSES Court 
  /// Order Number.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length
    = ObligorCsePersonNumber_MaxLength)]
  public string ObligorCsePersonNumber
  {
    get => obligorCsePersonNumber ?? "";
    set => obligorCsePersonNumber =
      TrimEnd(Substring(value, 1, ObligorCsePersonNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the ObligorCsePersonNumber attribute.</summary>
  [JsonPropertyName("obligorCsePersonNumber")]
  [Computed]
  public string ObligorCsePersonNumber_Json
  {
    get => NullIf(ObligorCsePersonNumber, "");
    set => ObligorCsePersonNumber = value;
  }

  /// <summary>Length of the KAECSES_COURT_ORDER_NUMBER attribute.</summary>
  public const int KaecsesCourtOrderNumber_MaxLength = 14;

  /// <summary>
  /// The value of the KAECSES_COURT_ORDER_NUMBER attribute.
  /// The Court Order Number for which payment history will be migrated from 
  /// KAECSES to KESSEP as stored on the DAECSES Receipt history DBF database.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length
    = KaecsesCourtOrderNumber_MaxLength)]
  public string KaecsesCourtOrderNumber
  {
    get => kaecsesCourtOrderNumber ?? "";
    set => kaecsesCourtOrderNumber =
      TrimEnd(Substring(value, 1, KaecsesCourtOrderNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the KaecsesCourtOrderNumber attribute.</summary>
  [JsonPropertyName("kaecsesCourtOrderNumber")]
  [Computed]
  public string KaecsesCourtOrderNumber_Json
  {
    get => NullIf(KaecsesCourtOrderNumber, "");
    set => KaecsesCourtOrderNumber = value;
  }

  /// <summary>Length of the REQUESTING_USER_ID attribute.</summary>
  public const int RequestingUserId_MaxLength = 8;

  /// <summary>
  /// The value of the REQUESTING_USER_ID attribute.
  /// The User ID of the worker who initiated the data migration request on the 
  /// REIP screen.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = RequestingUserId_MaxLength)
    ]
  public string RequestingUserId
  {
    get => requestingUserId ?? "";
    set => requestingUserId =
      TrimEnd(Substring(value, 1, RequestingUserId_MaxLength));
  }

  /// <summary>
  /// The json value of the RequestingUserId attribute.</summary>
  [JsonPropertyName("requestingUserId")]
  [Computed]
  public string RequestingUserId_Json
  {
    get => NullIf(RequestingUserId, "");
    set => RequestingUserId = value;
  }

  /// <summary>
  /// The value of the DATE_REQUESTED attribute.
  /// The date that the request to migrate payment history from KAECSES to 
  /// KESSEP was initiated from the REIP.
  /// </summary>
  [JsonPropertyName("dateRequested")]
  [Member(Index = 4, Type = MemberType.Date)]
  public DateTime? DateRequested
  {
    get => dateRequested;
    set => dateRequested = value;
  }

  /// <summary>
  /// The value of the DATE_PROCESSED attribute.
  /// The date that the payment history migration batch job was executed to load
  /// the KAESCES data into KESSEP. This value will be null when the request is
  /// initiated and setto current date when the data is stored in KESSEP. The
  /// batch job will only pick up rows with a null Date Processed.
  /// </summary>
  [JsonPropertyName("dateProcessed")]
  [Member(Index = 5, Type = MemberType.Date, Optional = true)]
  public DateTime? DateProcessed
  {
    get => dateProcessed;
    set => dateProcessed = value;
  }

  /// <summary>Length of the SUCCESSFULLY_CONVERTED attribute.</summary>
  public const int SuccessfullyConverted_MaxLength = 1;

  /// <summary>
  /// The value of the SUCCESSFULLY_CONVERTED attribute.
  /// An indicator which identifies which data migration requests were 
  /// successfully processed. When a request for migration is logged, the value
  /// will be spaces (null). Requests successfully processed will have a value
  /// of Y and requests that errored out during processing will have a value of
  /// N.
  /// </summary>
  [JsonPropertyName("successfullyConverted")]
  [Member(Index = 6, Type = MemberType.Char, Length
    = SuccessfullyConverted_MaxLength, Optional = true)]
  public string SuccessfullyConverted
  {
    get => successfullyConverted;
    set => successfullyConverted = value != null
      ? TrimEnd(Substring(value, 1, SuccessfullyConverted_MaxLength)) : null;
  }

  private string obligorCsePersonNumber;
  private string kaecsesCourtOrderNumber;
  private string requestingUserId;
  private DateTime? dateRequested;
  private DateTime? dateProcessed;
  private string successfullyConverted;
}
