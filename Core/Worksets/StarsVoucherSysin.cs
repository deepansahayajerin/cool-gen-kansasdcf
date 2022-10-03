// The source file: STARS_VOUCHER_SYSIN, ID: 372886502, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class StarsVoucherSysin: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public StarsVoucherSysin()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public StarsVoucherSysin(StarsVoucherSysin that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new StarsVoucherSysin Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(StarsVoucherSysin that)
  {
    base.Assign(that);
    voucherNumber = that.voucherNumber;
  }

  /// <summary>Length of the VOUCHER_NUMBER attribute.</summary>
  public const int VoucherNumber_MaxLength = 5;

  /// <summary>
  /// The value of the VOUCHER_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = VoucherNumber_MaxLength)]
  public string VoucherNumber
  {
    get => voucherNumber ?? "";
    set => voucherNumber =
      TrimEnd(Substring(value, 1, VoucherNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the VoucherNumber attribute.</summary>
  [JsonPropertyName("voucherNumber")]
  [Computed]
  public string VoucherNumber_Json
  {
    get => NullIf(VoucherNumber, "");
    set => VoucherNumber = value;
  }

  private string voucherNumber;
}
