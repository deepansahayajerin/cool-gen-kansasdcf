// Program: UPDATE_CR_DETAIL_ADDRESS, ID: 371770025, model: 746.
// Short name: SWE01471
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: UPDATE_CR_DETAIL_ADDRESS.
/// </para>
/// <para>
/// RESP: FNCLMGMNT
/// This action block will alow a cash receipt detail address to be updated if 
/// it us associated to only one of either a cash receipt detail or a receipt
/// refund.  If it is associated to two things then it cannot be updated for
/// audit reasons.
/// </para>
/// </summary>
[Serializable]
public partial class UpdateCrDetailAddress: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the UPDATE_CR_DETAIL_ADDRESS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new UpdateCrDetailAddress(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of UpdateCrDetailAddress.
  /// </summary>
  public UpdateCrDetailAddress(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (ReadCashReceiptDetailAddress())
    {
      // check to see if an update really needs to be performed.
      if (!Equal(import.CashReceiptDetailAddress.Street1,
        entities.CashReceiptDetailAddress.Street1))
      {
      }
      else if (!Equal(import.CashReceiptDetailAddress.Street2,
        entities.CashReceiptDetailAddress.Street2))
      {
      }
      else if (!Equal(import.CashReceiptDetailAddress.City,
        entities.CashReceiptDetailAddress.City))
      {
      }
      else if (!Equal(import.CashReceiptDetailAddress.State,
        entities.CashReceiptDetailAddress.State))
      {
      }
      else if (!Equal(import.CashReceiptDetailAddress.ZipCode3,
        entities.CashReceiptDetailAddress.ZipCode3))
      {
      }
      else if (!Equal(import.CashReceiptDetailAddress.ZipCode4,
        entities.CashReceiptDetailAddress.ZipCode4))
      {
      }
      else if (!Equal(import.CashReceiptDetailAddress.ZipCode5,
        entities.CashReceiptDetailAddress.ZipCode5))
      {
      }
      else
      {
        return;
      }

      if (ReadCashReceiptDetail())
      {
        ++local.AddressUsage.Count;
      }

      foreach(var item in ReadReceiptRefund())
      {
        ++local.AddressUsage.Count;
      }

      if (local.AddressUsage.Count > 1)
      {
        return;
      }

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

  private bool ReadCashReceiptDetail()
  {
    System.Diagnostics.Debug.
      Assert(entities.CashReceiptDetailAddress.Populated);
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId",
          entities.CashReceiptDetailAddress.CrdIdentifier.GetValueOrDefault());
        db.SetInt32(
          command, "crvIdentifier",
          entities.CashReceiptDetailAddress.CrvIdentifier.GetValueOrDefault());
        db.SetInt32(
          command, "cstIdentifier",
          entities.CashReceiptDetailAddress.CstIdentifier.GetValueOrDefault());
        db.SetInt32(
          command, "crtIdentifier",
          entities.CashReceiptDetailAddress.CrtIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.Populated = true;
      });
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
        entities.CashReceiptDetailAddress.CrtIdentifier =
          db.GetNullableInt32(reader, 8);
        entities.CashReceiptDetailAddress.CstIdentifier =
          db.GetNullableInt32(reader, 9);
        entities.CashReceiptDetailAddress.CrvIdentifier =
          db.GetNullableInt32(reader, 10);
        entities.CashReceiptDetailAddress.CrdIdentifier =
          db.GetNullableInt32(reader, 11);
        entities.CashReceiptDetailAddress.Populated = true;
      });
  }

  private IEnumerable<bool> ReadReceiptRefund()
  {
    entities.ReceiptRefund.Populated = false;

    return ReadEach("ReadReceiptRefund",
      (db, command) =>
      {
        db.SetNullableDateTime(
          command, "cdaIdentifier",
          entities.CashReceiptDetailAddress.SystemGeneratedIdentifier.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ReceiptRefund.CreatedTimestamp = db.GetDateTime(reader, 0);
        entities.ReceiptRefund.CdaIdentifier =
          db.GetNullableDateTime(reader, 1);
        entities.ReceiptRefund.Populated = true;

        return true;
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
  protected readonly Local local = new();
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
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of AddressUsage.
    /// </summary>
    [JsonPropertyName("addressUsage")]
    public Common AddressUsage
    {
      get => addressUsage ??= new();
      set => addressUsage = value;
    }

    private Common addressUsage;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ReceiptRefund.
    /// </summary>
    [JsonPropertyName("receiptRefund")]
    public ReceiptRefund ReceiptRefund
    {
      get => receiptRefund ??= new();
      set => receiptRefund = value;
    }

    /// <summary>
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailAddress.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailAddress")]
    public CashReceiptDetailAddress CashReceiptDetailAddress
    {
      get => cashReceiptDetailAddress ??= new();
      set => cashReceiptDetailAddress = value;
    }

    private ReceiptRefund receiptRefund;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptDetailAddress cashReceiptDetailAddress;
  }
#endregion
}
