// The source file: TRANSACTION_COMMAND, ID: 371422806, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SECUR
/// THIS IS THE VALID COMMANDS FOR EACH OF THE TRANSACTIONS.
/// </summary>
[Serializable]
public partial class TransactionCommand: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public TransactionCommand()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public TransactionCommand(TransactionCommand that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new TransactionCommand Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(TransactionCommand that)
  {
    base.Assign(that);
    id = that.id;
    fkCmdValue = that.fkCmdValue;
    fkTrnTrancode = that.fkTrnTrancode;
    fkTrnScreenid = that.fkTrnScreenid;
  }

  /// <summary>
  /// The value of the ID attribute.
  /// The attribute identifier of the transaction_command.
  /// </summary>
  [JsonPropertyName("id")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 1)]
  public int Id
  {
    get => id;
    set => id = value;
  }

  /// <summary>Length of the VALUE attribute.</summary>
  public const int FkCmdValue_MaxLength = 8;

  /// <summary>
  /// The value of the VALUE attribute.
  /// command value available to a transcation(screen).
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = FkCmdValue_MaxLength)]
  public string FkCmdValue
  {
    get => fkCmdValue ?? "";
    set => fkCmdValue = TrimEnd(Substring(value, 1, FkCmdValue_MaxLength));
  }

  /// <summary>
  /// The json value of the FkCmdValue attribute.</summary>
  [JsonPropertyName("fkCmdValue")]
  [Computed]
  public string FkCmdValue_Json
  {
    get => NullIf(FkCmdValue, "");
    set => FkCmdValue = value;
  }

  /// <summary>Length of the TRANCODE attribute.</summary>
  public const int FkTrnTrancode_MaxLength = 4;

  /// <summary>
  /// The value of the TRANCODE attribute.
  /// the related trancode for the screen id.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = FkTrnTrancode_MaxLength)]
  public string FkTrnTrancode
  {
    get => fkTrnTrancode ?? "";
    set => fkTrnTrancode =
      TrimEnd(Substring(value, 1, FkTrnTrancode_MaxLength));
  }

  /// <summary>
  /// The json value of the FkTrnTrancode attribute.</summary>
  [JsonPropertyName("fkTrnTrancode")]
  [Computed]
  public string FkTrnTrancode_Json
  {
    get => NullIf(FkTrnTrancode, "");
    set => FkTrnTrancode = value;
  }

  /// <summary>Length of the SCREEN_ID attribute.</summary>
  public const int FkTrnScreenid_MaxLength = 4;

  /// <summary>
  /// The value of the SCREEN_ID attribute.
  /// the screen id for the transaction(procedure)
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = FkTrnScreenid_MaxLength)]
  public string FkTrnScreenid
  {
    get => fkTrnScreenid ?? "";
    set => fkTrnScreenid =
      TrimEnd(Substring(value, 1, FkTrnScreenid_MaxLength));
  }

  /// <summary>
  /// The json value of the FkTrnScreenid attribute.</summary>
  [JsonPropertyName("fkTrnScreenid")]
  [Computed]
  public string FkTrnScreenid_Json
  {
    get => NullIf(FkTrnScreenid, "");
    set => FkTrnScreenid = value;
  }

  private int id;
  private string fkCmdValue;
  private string fkTrnTrancode;
  private string fkTrnScreenid;
}
