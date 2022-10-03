// Program: FN_INCLUDE_DISB_TRAN_TO_PMT_REQ, ID: 372676268, model: 746.
// Short name: SWE02110
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_INCLUDE_DISB_TRAN_TO_PMT_REQ.
/// </para>
/// <para>
/// RESP: FINANCE
/// This action block attaches the disbursement transaction to the Payment 
/// Request and sets the Disbursement Transaction as processed.
/// </para>
/// </summary>
[Serializable]
public partial class FnIncludeDisbTranToPmtReq: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_INCLUDE_DISB_TRAN_TO_PMT_REQ program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnIncludeDisbTranToPmtReq(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnIncludeDisbTranToPmtReq.
  /// </summary>
  public FnIncludeDisbTranToPmtReq(IContext context, Import import,
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
    // Date	By	IDCR#	Description
    // ---------------------------------------------
    // 101597	govind		Initial code
    // 112097	govind		Removed persistent views
    // 120697	govind		Fixed not to use the import disb tran amount in Update 
    // Payment Request stmt (instead use the entity action view read)
    // 122397	govind		Set designated payee from Disb Tran
    // 17dec98	LXJ		Removed unnecessery ABs,
    // 			views and I/Os.
    // 505/22/99  Fangman     Removed use of Disbursement_Transaction 
    // Designated_Payee.
    // 12/29/99   N.Engoor      Added export view to pass back the updated 
    // payment request amount.
    // ---------------------------------------------
    local.Current.Date = Now().Date;
    local.Current.Timestamp = Now();
    local.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;

    if (!Lt(local.Zero.Date, local.ProgramProcessingInfo.ProcessDate))
    {
      local.ProgramProcessingInfo.ProcessDate = local.Current.Date;
    }

    local.HardcodeProcessed.SystemGeneratedIdentifier = 2;
    local.HardcodeDisbursement.Type1 = "D";

    if (ReadDisbursementTransaction())
    {
      if (ReadPaymentRequest())
      {
        try
        {
          UpdatePaymentRequest();

          if (Equal(entities.PaymentRequest.Type1, "EFT"))
          {
            if (ReadElectronicFundTransmission())
            {
              try
              {
                UpdateElectronicFundTransmission();
              }
              catch(Exception e1)
              {
                switch(GetErrorCode(e1))
                {
                  case ErrorCode.AlreadyExists:
                    break;
                  case ErrorCode.PermittedValueViolation:
                    break;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }
            }
          }

          UseFnSetDisbTranAsProcessed();
          MovePaymentRequest(entities.PaymentRequest, export.Pass652);
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_PAYMENT_REQUEST_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_PAYMENT_REQUEST_PV";

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
        ExitState = "FN0000_PAYMENT_REQUEST_NF";
      }
    }
    else
    {
      ExitState = "FN0000_DISB_TRANSACTION_NF";
    }
  }

  private static void MovePaymentRequest(PaymentRequest source,
    PaymentRequest target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Type1 = source.Type1;
    target.Amount = source.Amount;
  }

  private void UseFnSetDisbTranAsProcessed()
  {
    var useImport = new FnSetDisbTranAsProcessed.Import();
    var useExport = new FnSetDisbTranAsProcessed.Export();

    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;
    useImport.Processed.SystemGeneratedIdentifier =
      local.HardcodeProcessed.SystemGeneratedIdentifier;
    useImport.DisbursementTransaction.SystemGeneratedIdentifier =
      entities.DisbursementTransaction.SystemGeneratedIdentifier;
    useImport.Obligee.Number = import.Obligee.Number;

    Call(FnSetDisbTranAsProcessed.Execute, useImport, useExport);
  }

  private bool ReadDisbursementTransaction()
  {
    entities.DisbursementTransaction.Populated = false;

    return Read("ReadDisbursementTransaction",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.Obligee.Number);
        db.SetInt32(
          command, "disbTranId",
          import.DisbursementTransaction.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.DisbursementTransaction.CpaType = db.GetString(reader, 0);
        entities.DisbursementTransaction.CspNumber = db.GetString(reader, 1);
        entities.DisbursementTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DisbursementTransaction.Amount = db.GetDecimal(reader, 3);
        entities.DisbursementTransaction.PrqGeneratedId =
          db.GetNullableInt32(reader, 4);
        entities.DisbursementTransaction.DesignatedPayee =
          db.GetNullableString(reader, 5);
        entities.DisbursementTransaction.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.DisbursementTransaction.CpaType);
      });
  }

  private bool ReadElectronicFundTransmission()
  {
    entities.ElectronicFundTransmission.Populated = false;

    return Read("ReadElectronicFundTransmission",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prqGeneratedId",
          entities.PaymentRequest.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ElectronicFundTransmission.TransmittalAmount =
          db.GetDecimal(reader, 0);
        entities.ElectronicFundTransmission.TransmissionType =
          db.GetString(reader, 1);
        entities.ElectronicFundTransmission.TransmissionIdentifier =
          db.GetInt32(reader, 2);
        entities.ElectronicFundTransmission.PrqGeneratedId =
          db.GetNullableInt32(reader, 3);
        entities.ElectronicFundTransmission.CollectionAmount =
          db.GetNullableDecimal(reader, 4);
        entities.ElectronicFundTransmission.Populated = true;
      });
  }

  private bool ReadPaymentRequest()
  {
    entities.PaymentRequest.Populated = false;

    return Read("ReadPaymentRequest",
      (db, command) =>
      {
        db.SetInt32(
          command, "paymentRequestId",
          import.PaymentRequest.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentRequest.ProcessDate = db.GetDate(reader, 1);
        entities.PaymentRequest.Amount = db.GetDecimal(reader, 2);
        entities.PaymentRequest.CreatedBy = db.GetString(reader, 3);
        entities.PaymentRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.PaymentRequest.DesignatedPayeeCsePersonNo =
          db.GetNullableString(reader, 5);
        entities.PaymentRequest.CsePersonNumber =
          db.GetNullableString(reader, 6);
        entities.PaymentRequest.ImprestFundCode =
          db.GetNullableString(reader, 7);
        entities.PaymentRequest.Classification = db.GetString(reader, 8);
        entities.PaymentRequest.AchFormatCode = db.GetNullableString(reader, 9);
        entities.PaymentRequest.Type1 = db.GetString(reader, 10);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 11);
        entities.PaymentRequest.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.PaymentRequest.Type1);
      });
  }

  private void UpdateElectronicFundTransmission()
  {
    var transmittalAmount = entities.PaymentRequest.Amount;

    entities.ElectronicFundTransmission.Populated = false;
    Update("UpdateElectronicFundTransmission",
      (db, command) =>
      {
        db.SetDecimal(command, "transmittalAmount", transmittalAmount);
        db.SetNullableDecimal(command, "collectionAmount", transmittalAmount);
        db.SetString(
          command, "transmissionType",
          entities.ElectronicFundTransmission.TransmissionType);
        db.SetInt32(
          command, "transmissionId",
          entities.ElectronicFundTransmission.TransmissionIdentifier);
      });

    entities.ElectronicFundTransmission.TransmittalAmount = transmittalAmount;
    entities.ElectronicFundTransmission.CollectionAmount = transmittalAmount;
    entities.ElectronicFundTransmission.Populated = true;
  }

  private void UpdatePaymentRequest()
  {
    System.Diagnostics.Debug.Assert(entities.DisbursementTransaction.Populated);

    var amount =
      entities.PaymentRequest.Amount + entities.DisbursementTransaction.Amount;
    var systemGeneratedIdentifier =
      entities.PaymentRequest.SystemGeneratedIdentifier;

    entities.DisbursementTransaction.Populated = false;
    entities.PaymentRequest.Populated = false;
    Update("UpdatePaymentRequest#1",
      (db, command) =>
      {
        db.SetDecimal(command, "amount", amount);
        db.SetInt32(command, "paymentRequestId", systemGeneratedIdentifier);
      });

    Update("UpdatePaymentRequest#2",
      (db, command) =>
      {
        db.
          SetNullableInt32(command, "prqGeneratedId", systemGeneratedIdentifier);
          
        db.SetString(
          command, "cpaType", entities.DisbursementTransaction.CpaType);
        db.SetString(
          command, "cspNumber", entities.DisbursementTransaction.CspNumber);
        db.SetInt32(
          command, "disbTranId",
          entities.DisbursementTransaction.SystemGeneratedIdentifier);
      });

    entities.PaymentRequest.Amount = amount;
    entities.DisbursementTransaction.PrqGeneratedId = systemGeneratedIdentifier;
    entities.DisbursementTransaction.Populated = true;
    entities.PaymentRequest.Populated = true;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
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
    /// A value of Obligee.
    /// </summary>
    [JsonPropertyName("obligee")]
    public CsePerson Obligee
    {
      get => obligee ??= new();
      set => obligee = value;
    }

    /// <summary>
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
    private DisbursementTransaction disbursementTransaction;
    private CsePerson obligee;
    private PaymentRequest paymentRequest;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Pass652.
    /// </summary>
    [JsonPropertyName("pass652")]
    public PaymentRequest Pass652
    {
      get => pass652 ??= new();
      set => pass652 = value;
    }

    private PaymentRequest pass652;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ZdeleteMeZzzzzzzzzzzzzzzzzzzzz.
    /// </summary>
    [JsonPropertyName("zdeleteMeZzzzzzzzzzzzzzzzzzzzz")]
    public PaymentRequest ZdeleteMeZzzzzzzzzzzzzzzzzzzzz
    {
      get => zdeleteMeZzzzzzzzzzzzzzzzzzzzz ??= new();
      set => zdeleteMeZzzzzzzzzzzzzzzzzzzzz = value;
    }

    /// <summary>
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public DateWorkArea Zero
    {
      get => zero ??= new();
      set => zero = value;
    }

    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

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
    /// A value of HardcodeProcessed.
    /// </summary>
    [JsonPropertyName("hardcodeProcessed")]
    public DisbursementStatus HardcodeProcessed
    {
      get => hardcodeProcessed ??= new();
      set => hardcodeProcessed = value;
    }

    /// <summary>
    /// A value of HardcodeDisbursement.
    /// </summary>
    [JsonPropertyName("hardcodeDisbursement")]
    public DisbursementTransaction HardcodeDisbursement
    {
      get => hardcodeDisbursement ??= new();
      set => hardcodeDisbursement = value;
    }

    private PaymentRequest zdeleteMeZzzzzzzzzzzzzzzzzzzzz;
    private DateWorkArea zero;
    private ProgramProcessingInfo programProcessingInfo;
    private DateWorkArea current;
    private DisbursementStatus hardcodeProcessed;
    private DisbursementTransaction hardcodeDisbursement;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ElectronicFundTransmission.
    /// </summary>
    [JsonPropertyName("electronicFundTransmission")]
    public ElectronicFundTransmission ElectronicFundTransmission
    {
      get => electronicFundTransmission ??= new();
      set => electronicFundTransmission = value;
    }

    /// <summary>
    /// A value of Obligee1.
    /// </summary>
    [JsonPropertyName("obligee1")]
    public CsePersonAccount Obligee1
    {
      get => obligee1 ??= new();
      set => obligee1 = value;
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
    /// A value of Obligee2.
    /// </summary>
    [JsonPropertyName("obligee2")]
    public CsePerson Obligee2
    {
      get => obligee2 ??= new();
      set => obligee2 = value;
    }

    /// <summary>
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
    }

    /// <summary>
    /// A value of Processed.
    /// </summary>
    [JsonPropertyName("processed")]
    public DisbursementStatus Processed
    {
      get => processed ??= new();
      set => processed = value;
    }

    private ElectronicFundTransmission electronicFundTransmission;
    private CsePersonAccount obligee1;
    private DisbursementTransaction disbursementTransaction;
    private CsePerson obligee2;
    private PaymentRequest paymentRequest;
    private DisbursementStatus processed;
  }
#endregion
}
