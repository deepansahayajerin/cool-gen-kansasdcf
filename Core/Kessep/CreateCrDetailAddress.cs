// Program: CREATE_CR_DETAIL_ADDRESS, ID: 371770024, model: 746.
// Short name: SWE00132
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: CREATE_CR_DETAIL_ADDRESS.
/// </para>
/// <para>
/// RESP: FINCLMGMNT	
/// This action block will create a cash receipt detail address related to the 
/// cash receipt detail.
/// </para>
/// </summary>
[Serializable]
public partial class CreateCrDetailAddress: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CREATE_CR_DETAIL_ADDRESS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CreateCrDetailAddress(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CreateCrDetailAddress.
  /// </summary>
  public CreateCrDetailAddress(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (ReadCashReceiptDetail())
    {
      try
      {
        CreateCashReceiptDetailAddress();
        export.CashReceiptDetailAddress.SystemGeneratedIdentifier =
          entities.CashReceiptDetailAddress.SystemGeneratedIdentifier;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0038_CASH_RCPT_DTL_ADDR_AE";

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
      ExitState = "FN0052_CASH_RCPT_DTL_NF";
    }
  }

  private void CreateCashReceiptDetailAddress()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);

    var systemGeneratedIdentifier = Now();
    var street1 = import.CashReceiptDetailAddress.Street1;
    var street2 = import.CashReceiptDetailAddress.Street2 ?? "";
    var city = import.CashReceiptDetailAddress.City;
    var state = import.CashReceiptDetailAddress.State;
    var zipCode5 = import.CashReceiptDetailAddress.ZipCode5;
    var zipCode4 = import.CashReceiptDetailAddress.ZipCode4 ?? "";
    var zipCode3 = import.CashReceiptDetailAddress.ZipCode3 ?? "";
    var crtIdentifier = entities.CashReceiptDetail.CrtIdentifier;
    var cstIdentifier = entities.CashReceiptDetail.CstIdentifier;
    var crvIdentifier = entities.CashReceiptDetail.CrvIdentifier;
    var crdIdentifier = entities.CashReceiptDetail.SequentialIdentifier;

    entities.CashReceiptDetailAddress.Populated = false;
    Update("CreateCashReceiptDetailAddress",
      (db, command) =>
      {
        db.SetDateTime(command, "crdetailAddressI", systemGeneratedIdentifier);
        db.SetString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetString(command, "city", city);
        db.SetString(command, "state", state);
        db.SetString(command, "zipCode5", zipCode5);
        db.SetNullableString(command, "zipCode4", zipCode4);
        db.SetNullableString(command, "zipCode3", zipCode3);
        db.SetNullableInt32(command, "crtIdentifier", crtIdentifier);
        db.SetNullableInt32(command, "cstIdentifier", cstIdentifier);
        db.SetNullableInt32(command, "crvIdentifier", crvIdentifier);
        db.SetNullableInt32(command, "crdIdentifier", crdIdentifier);
      });

    entities.CashReceiptDetailAddress.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.CashReceiptDetailAddress.Street1 = street1;
    entities.CashReceiptDetailAddress.Street2 = street2;
    entities.CashReceiptDetailAddress.City = city;
    entities.CashReceiptDetailAddress.State = state;
    entities.CashReceiptDetailAddress.ZipCode5 = zipCode5;
    entities.CashReceiptDetailAddress.ZipCode4 = zipCode4;
    entities.CashReceiptDetailAddress.ZipCode3 = zipCode3;
    entities.CashReceiptDetailAddress.CrtIdentifier = crtIdentifier;
    entities.CashReceiptDetailAddress.CstIdentifier = cstIdentifier;
    entities.CashReceiptDetailAddress.CrvIdentifier = crvIdentifier;
    entities.CashReceiptDetailAddress.CrdIdentifier = crdIdentifier;
    entities.CashReceiptDetailAddress.Populated = true;
  }

  private bool ReadCashReceiptDetail()
  {
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId", import.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "crvIdentifier",
          import.CashReceiptEvent.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          import.CashReceiptSourceType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          import.CashReceiptType.SystemGeneratedIdentifier);
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
    /// A value of CashReceiptEvent.
    /// </summary>
    [JsonPropertyName("cashReceiptEvent")]
    public CashReceiptEvent CashReceiptEvent
    {
      get => cashReceiptEvent ??= new();
      set => cashReceiptEvent = value;
    }

    /// <summary>
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
    }

    private CashReceiptDetailAddress cashReceiptDetailAddress;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptType cashReceiptType;
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
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
    }

    /// <summary>
    /// A value of CashReceiptEvent.
    /// </summary>
    [JsonPropertyName("cashReceiptEvent")]
    public CashReceiptEvent CashReceiptEvent
    {
      get => cashReceiptEvent ??= new();
      set => cashReceiptEvent = value;
    }

    /// <summary>
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
    }

    private CashReceiptDetailAddress cashReceiptDetailAddress;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceipt cashReceipt;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptType cashReceiptType;
  }
#endregion
}
