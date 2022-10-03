// Program: SI_REGI_CREATE_IC_REQ_PAY_ADDR, ID: 373468015, model: 746.
// Short name: SWE02109
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_REGI_CREATE_IC_REQ_PAY_ADDR.
/// </summary>
[Serializable]
public partial class SiRegiCreateIcReqPayAddr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_REGI_CREATE_IC_REQ_PAY_ADDR program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiRegiCreateIcReqPayAddr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiRegiCreateIcReqPayAddr.
  /// </summary>
  public SiRegiCreateIcReqPayAddr(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -----------------------------------------
    // 03/19/99 W.Campbell     Added set statements
    //                         for mandatory attributes in the
    //                         create statement.  Also added
    //                         a local timestamp view and
    //                         modified statements to use it.
    //                         Also added PV and AE exit states.
    // -----------------------------------------
    // 06/23/99  M. Lachowicz  Change property of READ
    //                         (Select Only or Cursor Only)
    // ------------------------------------------------------------
    // 03/12/01  swsrchf  I00115299 Fixed the payment address:
    //                              set the Payable_To_Name to 
    // Payment_Mailing_Address_Line_1
    //                              set Street1 to Payment_Address_Line_2
    // ------------------------------------------------------------
    local.Current.Timestamp = Now();
    UseOeCabSetMnemonics();

    // 06/23/99 M.L
    //              Change property of the following READ to generate
    //              SELECT ONLY
    if (ReadInterstateRequest())
    {
      if (ReadInterstatePaymentAddress())
      {
        DeleteInterstatePaymentAddress();
      }

      try
      {
        CreateInterstatePaymentAddress();
        export.InterstatePaymentAddress.
          Assign(entities.InterstatePaymentAddress);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "INTERSTATE_PAYMENT_ADDRESS_AE";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "INTERSTATE_PAYMENT_ADDRESS_PV";

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
      ExitState = "INTERSTATE_REQUEST_NF";
    }
  }

  private void UseOeCabSetMnemonics()
  {
    var useImport = new OeCabSetMnemonics.Import();
    var useExport = new OeCabSetMnemonics.Export();

    Call(OeCabSetMnemonics.Execute, useImport, useExport);

    local.MaxDate.ExpirationDate = useExport.MaxDate.ExpirationDate;
  }

  private void CreateInterstatePaymentAddress()
  {
    var intGeneratedId = entities.InterstateRequest.IntHGeneratedId;
    var addressStartDate = import.InterstateCase.TransactionDate;
    var type1 = "PY";
    var street1 = import.InterstateCase.PaymentAddressLine2 ?? "";
    var city = import.InterstateCase.PaymentCity ?? "";
    var addressEndDate = local.MaxDate.ExpirationDate;
    var createdBy = global.UserId;
    var createdTimestamp = local.Current.Timestamp;
    var payableToName = import.InterstateCase.PaymentMailingAddressLine1 ?? "";
    var state = import.InterstateCase.PaymentState ?? "";
    var zipCode = import.InterstateCase.PaymentZipCode5 ?? "";
    var zip4 = import.InterstateCase.PaymentZipCode4 ?? "";
    var locationType = "D";
    var routingNumberAba =
      import.InterstateCase.SendPaymentsRoutingCode.GetValueOrDefault();
    var accountNumberDfi =
      Substring(import.InterstateCase.SendPaymentsBankAccount, 1, 17);

    CheckValid<InterstatePaymentAddress>("LocationType", locationType);
    entities.InterstatePaymentAddress.Populated = false;
    Update("CreateInterstatePaymentAddress",
      (db, command) =>
      {
        db.SetInt32(command, "intGeneratedId", intGeneratedId);
        db.SetDate(command, "addressStartDate", addressStartDate);
        db.SetNullableString(command, "type", type1);
        db.SetString(command, "street1", street1);
        db.SetNullableString(command, "street2", "");
        db.SetString(command, "city", city);
        db.SetNullableString(command, "zip5", "");
        db.SetNullableDate(command, "addressEndDate", addressEndDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", createdBy);
        db.SetDateTime(command, "lastUpdatedTimes", createdTimestamp);
        db.SetNullableString(command, "payableToName", payableToName);
        db.SetNullableString(command, "state", state);
        db.SetNullableString(command, "zipCode", zipCode);
        db.SetNullableString(command, "zip4", zip4);
        db.SetNullableString(command, "zip3", "");
        db.SetNullableString(command, "county", "");
        db.SetNullableString(command, "street3", "");
        db.SetNullableString(command, "postalCode", "");
        db.SetString(command, "locationType", locationType);
        db.SetNullableInt64(command, "routingNumberAba", routingNumberAba);
        db.SetNullableString(command, "accountNumberDfi", accountNumberDfi);
        db.SetNullableString(command, "accountType", "");
      });

    entities.InterstatePaymentAddress.IntGeneratedId = intGeneratedId;
    entities.InterstatePaymentAddress.AddressStartDate = addressStartDate;
    entities.InterstatePaymentAddress.Type1 = type1;
    entities.InterstatePaymentAddress.Street1 = street1;
    entities.InterstatePaymentAddress.Street2 = "";
    entities.InterstatePaymentAddress.City = city;
    entities.InterstatePaymentAddress.AddressEndDate = addressEndDate;
    entities.InterstatePaymentAddress.CreatedBy = createdBy;
    entities.InterstatePaymentAddress.CreatedTimestamp = createdTimestamp;
    entities.InterstatePaymentAddress.LastUpdatedBy = createdBy;
    entities.InterstatePaymentAddress.LastUpdatedTimestamp = createdTimestamp;
    entities.InterstatePaymentAddress.PayableToName = payableToName;
    entities.InterstatePaymentAddress.State = state;
    entities.InterstatePaymentAddress.ZipCode = zipCode;
    entities.InterstatePaymentAddress.Zip4 = zip4;
    entities.InterstatePaymentAddress.LocationType = locationType;
    entities.InterstatePaymentAddress.RoutingNumberAba = routingNumberAba;
    entities.InterstatePaymentAddress.AccountNumberDfi = accountNumberDfi;
    entities.InterstatePaymentAddress.Populated = true;
  }

  private void DeleteInterstatePaymentAddress()
  {
    Update("DeleteInterstatePaymentAddress",
      (db, command) =>
      {
        db.SetInt32(
          command, "intGeneratedId",
          entities.InterstatePaymentAddress.IntGeneratedId);
        db.SetDate(
          command, "addressStartDate",
          entities.InterstatePaymentAddress.AddressStartDate.
            GetValueOrDefault());
      });
  }

  private bool ReadInterstatePaymentAddress()
  {
    entities.InterstatePaymentAddress.Populated = false;

    return Read("ReadInterstatePaymentAddress",
      (db, command) =>
      {
        db.SetInt32(
          command, "intGeneratedId",
          entities.InterstateRequest.IntHGeneratedId);
      },
      (db, reader) =>
      {
        entities.InterstatePaymentAddress.IntGeneratedId =
          db.GetInt32(reader, 0);
        entities.InterstatePaymentAddress.AddressStartDate =
          db.GetDate(reader, 1);
        entities.InterstatePaymentAddress.Type1 =
          db.GetNullableString(reader, 2);
        entities.InterstatePaymentAddress.Street1 = db.GetString(reader, 3);
        entities.InterstatePaymentAddress.Street2 =
          db.GetNullableString(reader, 4);
        entities.InterstatePaymentAddress.City = db.GetString(reader, 5);
        entities.InterstatePaymentAddress.AddressEndDate =
          db.GetNullableDate(reader, 6);
        entities.InterstatePaymentAddress.CreatedBy = db.GetString(reader, 7);
        entities.InterstatePaymentAddress.CreatedTimestamp =
          db.GetDateTime(reader, 8);
        entities.InterstatePaymentAddress.LastUpdatedBy =
          db.GetString(reader, 9);
        entities.InterstatePaymentAddress.LastUpdatedTimestamp =
          db.GetDateTime(reader, 10);
        entities.InterstatePaymentAddress.PayableToName =
          db.GetNullableString(reader, 11);
        entities.InterstatePaymentAddress.State =
          db.GetNullableString(reader, 12);
        entities.InterstatePaymentAddress.ZipCode =
          db.GetNullableString(reader, 13);
        entities.InterstatePaymentAddress.Zip4 =
          db.GetNullableString(reader, 14);
        entities.InterstatePaymentAddress.LocationType =
          db.GetString(reader, 15);
        entities.InterstatePaymentAddress.RoutingNumberAba =
          db.GetNullableInt64(reader, 16);
        entities.InterstatePaymentAddress.AccountNumberDfi =
          db.GetNullableString(reader, 17);
        entities.InterstatePaymentAddress.Populated = true;
      });
  }

  private bool ReadInterstateRequest()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier", import.InterstateRequest.IntHGeneratedId);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.Populated = true;
      });
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
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    /// <summary>
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    private InterstateCase interstateCase;
    private InterstateRequest interstateRequest;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of InterstatePaymentAddress.
    /// </summary>
    [JsonPropertyName("interstatePaymentAddress")]
    public InterstatePaymentAddress InterstatePaymentAddress
    {
      get => interstatePaymentAddress ??= new();
      set => interstatePaymentAddress = value;
    }

    private InterstatePaymentAddress interstatePaymentAddress;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public Code MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

    private DateWorkArea current;
    private Code maxDate;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    /// <summary>
    /// A value of InterstatePaymentAddress.
    /// </summary>
    [JsonPropertyName("interstatePaymentAddress")]
    public InterstatePaymentAddress InterstatePaymentAddress
    {
      get => interstatePaymentAddress ??= new();
      set => interstatePaymentAddress = value;
    }

    private InterstateRequest interstateRequest;
    private InterstatePaymentAddress interstatePaymentAddress;
  }
#endregion
}
