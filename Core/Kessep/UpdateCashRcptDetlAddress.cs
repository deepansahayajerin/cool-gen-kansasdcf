// Program: UPDATE_CASH_RCPT_DETL_ADDRESS, ID: 371726653, model: 746.
// Short name: SWE01465
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: UPDATE_CASH_RCPT_DETL_ADDRESS.
/// </para>
/// <para>
/// RESP:  CASHMGMT
/// This action block updates the cash receipt detail address entity type.
/// </para>
/// </summary>
[Serializable]
public partial class UpdateCashRcptDetlAddress: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the UPDATE_CASH_RCPT_DETL_ADDRESS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new UpdateCashRcptDetlAddress(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of UpdateCashRcptDetlAddress.
  /// </summary>
  public UpdateCashRcptDetlAddress(IContext context, Import import,
    Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    ExitState = "ACO_NN0000_ALL_OK";

    if (ReadCashReceiptDetailAddress())
    {
      try
      {
        UpdateCashReceiptDetailAddress();
        export.CashReceiptDetailAddress.
          Assign(entities.CashReceiptDetailAddress);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0040_CASH_RCPT_DTL_ADDR_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0041_CASH_RCPT_DTL_ADDR_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    else
    {
      ExitState = "FN0039_CASH_RCPT_DTL_ADDR_NF";
    }
  }

  private bool ReadCashReceiptDetailAddress()
  {
    entities.CashReceiptDetailAddress.Populated = false;

    return Read("ReadCashReceiptDetailAddress",
      (db, command) =>
      {
        db.SetDateTime(
          command, "crdetailAddressI",
          import.CashReceiptDetailAddress.SystemGeneratedIdentifier.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptDetailAddress.SystemGeneratedIdentifier =
          db.GetDateTime(reader, 0);
        entities.CashReceiptDetailAddress.Street1 = db.GetString(reader, 1);
        entities.CashReceiptDetailAddress.Street2 =
          db.GetNullableString(reader, 2);
        entities.CashReceiptDetailAddress.City = db.GetString(reader, 3);
        entities.CashReceiptDetailAddress.State = db.GetString(reader, 4);
        entities.CashReceiptDetailAddress.ZipCode5 = db.GetString(reader, 5);
        entities.CashReceiptDetailAddress.ZipCode4 =
          db.GetNullableString(reader, 6);
        entities.CashReceiptDetailAddress.ZipCode3 =
          db.GetNullableString(reader, 7);
        entities.CashReceiptDetailAddress.Populated = true;
      });
  }

  private void UpdateCashReceiptDetailAddress()
  {
    var street1 = import.CashReceiptDetailAddress.Street1;
    var street2 = import.CashReceiptDetailAddress.Street2 ?? "";
    var city = import.CashReceiptDetailAddress.City;
    var state = import.CashReceiptDetailAddress.State;
    var zipCode5 = import.CashReceiptDetailAddress.ZipCode5;
    var zipCode4 = import.CashReceiptDetailAddress.ZipCode4 ?? "";
    var zipCode3 = import.CashReceiptDetailAddress.ZipCode3 ?? "";

    entities.CashReceiptDetailAddress.Populated = false;
    Update("UpdateCashReceiptDetailAddress",
      (db, command) =>
      {
        db.SetString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetString(command, "city", city);
        db.SetString(command, "state", state);
        db.SetString(command, "zipCode5", zipCode5);
        db.SetNullableString(command, "zipCode4", zipCode4);
        db.SetNullableString(command, "zipCode3", zipCode3);
        db.SetDateTime(
          command, "crdetailAddressI",
          entities.CashReceiptDetailAddress.SystemGeneratedIdentifier.
            GetValueOrDefault());
      });

    entities.CashReceiptDetailAddress.Street1 = street1;
    entities.CashReceiptDetailAddress.Street2 = street2;
    entities.CashReceiptDetailAddress.City = city;
    entities.CashReceiptDetailAddress.State = state;
    entities.CashReceiptDetailAddress.ZipCode5 = zipCode5;
    entities.CashReceiptDetailAddress.ZipCode4 = zipCode4;
    entities.CashReceiptDetailAddress.ZipCode3 = zipCode3;
    entities.CashReceiptDetailAddress.Populated = true;
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Entities entities = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of CashReceiptDetailAddress.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailAddress")]
    public CashReceiptDetailAddress CashReceiptDetailAddress
    {
      get => cashReceiptDetailAddress ??= new();
      set => cashReceiptDetailAddress = value;
    }

    private CashReceiptDetailAddress cashReceiptDetailAddress;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CashReceiptDetailAddress.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailAddress")]
    public CashReceiptDetailAddress CashReceiptDetailAddress
    {
      get => cashReceiptDetailAddress ??= new();
      set => cashReceiptDetailAddress = value;
    }

    private CashReceiptDetailAddress cashReceiptDetailAddress;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CashReceiptDetailAddress.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailAddress")]
    public CashReceiptDetailAddress CashReceiptDetailAddress
    {
      get => cashReceiptDetailAddress ??= new();
      set => cashReceiptDetailAddress = value;
    }

    private CashReceiptDetailAddress cashReceiptDetailAddress;
  }
#endregion
}
