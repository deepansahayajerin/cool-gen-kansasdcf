// The source file: PAY_TAPE, ID: 371439031, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// Records the information about a pay tapes.  Primarily the creation date, 
/// voucher number, total number of records and total amount on the pay tape.
/// Each individual warrant on the pay tape is tied to this entity thru a
/// mandatory relationship so that we can always obtain the individual details
/// of a pay tape by knowing the date of the pay tape.
/// </summary>
[Serializable]
public partial class PayTape: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public PayTape()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public PayTape(PayTape that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new PayTape Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(PayTape that)
  {
    base.Assign(that);
    processDate = that.processDate;
    identCode = that.identCode;
    voucherNumber = that.voucherNumber;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
  }

  /// <summary>
  /// The value of the PROCESS_DATE attribute.
  /// The process date for which all of the warrants are created.
  /// </summary>
  [JsonPropertyName("processDate")]
  [Member(Index = 1, Type = MemberType.Date)]
  public DateTime? ProcessDate
  {
    get => processDate;
    set => processDate = value;
  }

  /// <summary>Length of the IDENT_CODE attribute.</summary>
  public const int IdentCode_MaxLength = 3;

  /// <summary>
  /// The value of the IDENT_CODE attribute.
  /// This is an identification code required by DOA.
  /// </summary>
  [JsonPropertyName("identCode")]
  [Member(Index = 2, Type = MemberType.Char, Length = IdentCode_MaxLength, Optional
    = true)]
  public string IdentCode
  {
    get => identCode;
    set => identCode = value != null
      ? TrimEnd(Substring(value, 1, IdentCode_MaxLength)) : null;
  }

  /// <summary>Length of the VOUCHER_NUMBER attribute.</summary>
  public const int VoucherNumber_MaxLength = 5;

  /// <summary>
  /// The value of the VOUCHER_NUMBER attribute.
  /// This is a number used by DOA and Finance.
  /// </summary>
  [JsonPropertyName("voucherNumber")]
  [Member(Index = 3, Type = MemberType.Char, Length = VoucherNumber_MaxLength, Optional
    = true)]
  public string VoucherNumber
  {
    get => voucherNumber;
    set => voucherNumber = value != null
      ? TrimEnd(Substring(value, 1, VoucherNumber_MaxLength)) : null;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The user id or program id responsible for the creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  [Member(Index = 5, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  private DateTime? processDate;
  private string identCode;
  private string voucherNumber;
  private string createdBy;
  private DateTime? createdTimestamp;
}
