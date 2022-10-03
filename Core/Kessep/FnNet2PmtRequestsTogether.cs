// Program: FN_NET_2_PMT_REQUESTS_TOGETHER, ID: 372727531, model: 746.
// Short name: SWE02589
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_NET_2_PMT_REQUESTS_TOGETHER.
/// </summary>
[Serializable]
public partial class FnNet2PmtRequestsTogether: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_NET_2_PMT_REQUESTS_TOGETHER program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnNet2PmtRequestsTogether(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnNet2PmtRequestsTogether.
  /// </summary>
  public FnNet2PmtRequestsTogether(IContext context, Import import,
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
    // ---------------------------------------------
    // This AB will net 2 payment requests by updating the amount of the 
    // previous pmt request and associating the disbursements of the current pmt
    // request with the previous pmt request and then deleting the current pmt
    // request.
    // If the request type is "EFT" then the previous EFT trans rec is updated 
    // with the current EFT trans rec and then the current EFT trans rec is
    // deleted.
    // ---------------------------------------------
    // ---------------------------------------------
    // Date	By	Description
    // 051399  Fangman   Initial code.
    // 062299  Fangman   Added code to delete the Payment_Status_history prior 
    // to deleting the Payment_Status.
    // 021201  Fangman   WR 284 - Don't issue Interstate EFTs
    // ---------------------------------------------
    if (!ReadPaymentRequest())
    {
      export.EabReportSend.RptDetail =
        "Error: Not Found reading the previous payment request for Payee " + import
        .Obligee.Number + " Payment Request # " + NumberToString
        (import.Previous.SystemGeneratedIdentifier, 15);

      return;
    }

    try
    {
      UpdatePaymentRequest();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          export.EabReportSend.RptDetail =
            "Error: Not Unique updating the previous payment request for Payee " +
            import.Obligee.Number + " Payment Request # " + NumberToString
            (entities.PreviousPaymentRequest.SystemGeneratedIdentifier, 15);

          return;
        case ErrorCode.PermittedValueViolation:
          export.EabReportSend.RptDetail =
            "Error: Permitted Value Violation updating the previous payment request for Payee " +
            import.Obligee.Number + " Payment Request # " + NumberToString
            (entities.PreviousPaymentRequest.SystemGeneratedIdentifier, 15);

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    foreach(var item in ReadDisbursementTransaction())
    {
      DisassociateDisbursementTransaction();
      AssociateDisbursementTransaction();
    }

    foreach(var item in ReadPaymentStatusHistory())
    {
      DeletePaymentStatusHistory();
    }

    DeletePaymentRequest();
  }

  private void AssociateDisbursementTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.DisbursementTransaction.Populated);

    var prqGeneratedId =
      entities.PreviousPaymentRequest.SystemGeneratedIdentifier;

    entities.DisbursementTransaction.Populated = false;
    Update("AssociateDisbursementTransaction",
      (db, command) =>
      {
        db.SetNullableInt32(command, "prqGeneratedId", prqGeneratedId);
        db.SetString(
          command, "cpaType", entities.DisbursementTransaction.CpaType);
        db.SetString(
          command, "cspNumber", entities.DisbursementTransaction.CspNumber);
        db.SetInt32(
          command, "disbTranId",
          entities.DisbursementTransaction.SystemGeneratedIdentifier);
      });

    entities.DisbursementTransaction.PrqGeneratedId = prqGeneratedId;
    entities.DisbursementTransaction.Populated = true;
  }

  private void DeletePaymentRequest()
  {
    bool exists;

    Update("DeletePaymentRequest#1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prqGeneratedId",
          import.Persistent.SystemGeneratedIdentifier);
      });

    exists = Read("DeletePaymentRequest#2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prqGeneratedId",
          import.Persistent.SystemGeneratedIdentifier);
      },
      null);

    if (exists)
    {
      throw DataError("Restrict violation on table \"CKT_ELEC_FUND_TRAN\".",
        "50001");
    }

    Update("DeletePaymentRequest#3",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prqGeneratedId",
          import.Persistent.SystemGeneratedIdentifier);
      });

    Update("DeletePaymentRequest#4",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prqGeneratedId",
          import.Persistent.SystemGeneratedIdentifier);
      });

    exists = Read("DeletePaymentRequest#5",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prqGeneratedId",
          import.Persistent.SystemGeneratedIdentifier);
      },
      null);

    if (exists)
    {
      throw DataError("Restrict violation on table \"CKT_POT_RECOVERY\".",
        "50001");
    }

    Update("DeletePaymentRequest#6",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prqGeneratedId",
          import.Persistent.SystemGeneratedIdentifier);
      });
  }

  private void DeletePaymentStatusHistory()
  {
    Update("DeletePaymentStatusHistory",
      (db, command) =>
      {
        db.SetInt32(
          command, "pstGeneratedId",
          entities.PaymentStatusHistory.PstGeneratedId);
        db.SetInt32(
          command, "prqGeneratedId",
          entities.PaymentStatusHistory.PrqGeneratedId);
        db.SetInt32(
          command, "pymntStatHistId",
          entities.PaymentStatusHistory.SystemGeneratedIdentifier);
      });
  }

  private void DisassociateDisbursementTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.DisbursementTransaction.Populated);
    entities.DisbursementTransaction.Populated = false;
    Update("DisassociateDisbursementTransaction",
      (db, command) =>
      {
        db.SetString(
          command, "cpaType", entities.DisbursementTransaction.CpaType);
        db.SetString(
          command, "cspNumber", entities.DisbursementTransaction.CspNumber);
        db.SetInt32(
          command, "disbTranId",
          entities.DisbursementTransaction.SystemGeneratedIdentifier);
      });

    entities.DisbursementTransaction.PrqGeneratedId = null;
    entities.DisbursementTransaction.Populated = true;
  }

  private IEnumerable<bool> ReadDisbursementTransaction()
  {
    entities.DisbursementTransaction.Populated = false;

    return ReadEach("ReadDisbursementTransaction",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prqGeneratedId",
          import.Persistent.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.DisbursementTransaction.CpaType = db.GetString(reader, 0);
        entities.DisbursementTransaction.CspNumber = db.GetString(reader, 1);
        entities.DisbursementTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DisbursementTransaction.Type1 = db.GetString(reader, 3);
        entities.DisbursementTransaction.DisbursementDate =
          db.GetNullableDate(reader, 4);
        entities.DisbursementTransaction.PrqGeneratedId =
          db.GetNullableInt32(reader, 5);
        entities.DisbursementTransaction.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.DisbursementTransaction.CpaType);
        CheckValid<DisbursementTransaction>("Type1",
          entities.DisbursementTransaction.Type1);

        return true;
      });
  }

  private bool ReadPaymentRequest()
  {
    entities.PreviousPaymentRequest.Populated = false;

    return Read("ReadPaymentRequest",
      (db, command) =>
      {
        db.SetInt32(
          command, "paymentRequestId",
          import.Previous.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.PreviousPaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PreviousPaymentRequest.Amount = db.GetDecimal(reader, 1);
        entities.PreviousPaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 2);
        entities.PreviousPaymentRequest.Populated = true;
      });
  }

  private IEnumerable<bool> ReadPaymentStatusHistory()
  {
    entities.PaymentStatusHistory.Populated = false;

    return ReadEach("ReadPaymentStatusHistory",
      (db, command) =>
      {
        db.SetInt32(
          command, "prqGeneratedId",
          import.Persistent.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.PaymentStatusHistory.PstGeneratedId = db.GetInt32(reader, 0);
        entities.PaymentStatusHistory.PrqGeneratedId = db.GetInt32(reader, 1);
        entities.PaymentStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.PaymentStatusHistory.Populated = true;

        return true;
      });
  }

  private void UpdatePaymentRequest()
  {
    var amount =
      entities.PreviousPaymentRequest.Amount + import.Persistent.Amount;

    entities.PreviousPaymentRequest.Populated = false;
    Update("UpdatePaymentRequest",
      (db, command) =>
      {
        db.SetDecimal(command, "amount", amount);
        db.SetInt32(
          command, "paymentRequestId",
          entities.PreviousPaymentRequest.SystemGeneratedIdentifier);
      });

    entities.PreviousPaymentRequest.Amount = amount;
    entities.PreviousPaymentRequest.Populated = true;
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
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public PaymentRequest Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of Persistent.
    /// </summary>
    [JsonPropertyName("persistent")]
    public PaymentRequest Persistent
    {
      get => persistent ??= new();
      set => persistent = value;
    }

    /// <summary>
    /// A value of Obligee.
    /// </summary>
    [JsonPropertyName("obligee")]
    public CsePerson Obligee
    {
      get => obligee ??= new();
      set => obligee = value;
    }

    private PaymentRequest previous;
    private PaymentRequest persistent;
    private CsePerson obligee;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    private EabReportSend eabReportSend;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of PreviousPaymentRequest.
    /// </summary>
    [JsonPropertyName("previousPaymentRequest")]
    public PaymentRequest PreviousPaymentRequest
    {
      get => previousPaymentRequest ??= new();
      set => previousPaymentRequest = value;
    }

    /// <summary>
    /// A value of DisbursementTransaction.
    /// </summary>
    [JsonPropertyName("disbursementTransaction")]
    public DisbursementTransaction DisbursementTransaction
    {
      get => disbursementTransaction ??= new();
      set => disbursementTransaction = value;
    }

    /// <summary>
    /// A value of PreviousElectronicFundTransmission.
    /// </summary>
    [JsonPropertyName("previousElectronicFundTransmission")]
    public ElectronicFundTransmission PreviousElectronicFundTransmission
    {
      get => previousElectronicFundTransmission ??= new();
      set => previousElectronicFundTransmission = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public ElectronicFundTransmission Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of PaymentStatusHistory.
    /// </summary>
    [JsonPropertyName("paymentStatusHistory")]
    public PaymentStatusHistory PaymentStatusHistory
    {
      get => paymentStatusHistory ??= new();
      set => paymentStatusHistory = value;
    }

    private PaymentRequest previousPaymentRequest;
    private DisbursementTransaction disbursementTransaction;
    private ElectronicFundTransmission previousElectronicFundTransmission;
    private ElectronicFundTransmission current;
    private PaymentStatusHistory paymentStatusHistory;
  }
#endregion
}
