// Program: FN_COMPLETE_PAYMENT_REQUEST, ID: 372673979, model: 746.
// Short name: SWE02585
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_COMPLETE_PAYMENT_REQUEST.
/// </summary>
[Serializable]
public partial class FnCompletePaymentRequest: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_COMPLETE_PAYMENT_REQUEST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCompletePaymentRequest(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCompletePaymentRequest.
  /// </summary>
  public FnCompletePaymentRequest(IContext context, Import import, Export export)
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
    // ---------------------------------------------
    // This AB will finish a Payment request by setting the final amount and 
    // creating a status history.
    // ---------------------------------------------
    // ---------------------------------------------
    // Date	By	Description
    // 042299  Fangman   Initial code.
    // 041200  Fangman   PR 93146 - "automatically" cancel warrants & deny 
    // recoveries if the amount is less than $1.00.
    // 06/05/12 GVandy CQ33868  Do not automatically cancel Warrants < $1.00.
    // ---------------------------------------------
    export.EabReportSend.RptDetail = "";

    if (import.ForUpdate.Amount == 0)
    {
      // ---------------------------------------------
      // If the entire warrant or EFT was recaptured then we will delete the 
      // payment request.
      // ---------------------------------------------
      DeletePaymentRequest();

      return;
    }
    else if (import.ForUpdate.Amount < 0)
    {
      // ---------------------------------------------
      // If the payment amount is negative then we change it to positive for the
      // potential recovery obligation.
      // ---------------------------------------------
      local.PaymentRequest.Amount = -import.ForUpdate.Amount;
    }
    else
    {
      local.PaymentRequest.Amount = import.ForUpdate.Amount;
    }

    // 06/05/12 GVandy CQ33868  Set process date to null date for Warrants < $1.
    // 00.
    if (local.PaymentRequest.Amount < 1M && !
      Equal(import.ForUpdate.Type1, "WAR"))
    {
      local.PaymentRequest.ProcessDate =
        import.ProgramProcessingInfo.ProcessDate;
    }
    else
    {
      local.PaymentRequest.ProcessDate = local.Initialized.ProcessDate;
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
            "Error: Not Unique updating the payment request for Payee " + import
            .Obligee.Number + " Payment Request # " + NumberToString
            (import.Persistent.SystemGeneratedIdentifier, 15);

          return;
        case ErrorCode.PermittedValueViolation:
          export.EabReportSend.RptDetail =
            "Error: Permitted Value Violation updating the payment request for Payee " +
            import.Obligee.Number + " Payment Request # " + NumberToString
            (import.Persistent.SystemGeneratedIdentifier, 15);

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    if (Equal(import.ForUpdate.Type1, "RCV"))
    {
      if (local.PaymentRequest.Amount < 1M)
      {
        try
        {
          CreatePaymentStatusHistory3();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              export.EabReportSend.RptDetail =
                "Error: Already Exists creating \"RCVPOT\" payment status history for Payee " +
                import.Obligee.Number + " Payment Request # " + NumberToString
                (import.Persistent.SystemGeneratedIdentifier, 15) + " Status ID of " +
                NumberToString
                (import.PersistentDenied.SystemGeneratedIdentifier, 15);

              break;
            case ErrorCode.PermittedValueViolation:
              export.EabReportSend.RptDetail =
                "Error: Permitted Value Violation creating \"RCVPOT\" payment status history for Payee " +
                import.Obligee.Number + " Payment Request # " + NumberToString
                (import.Persistent.SystemGeneratedIdentifier, 15) + " Status ID of " +
                NumberToString
                (import.PersistentDenied.SystemGeneratedIdentifier, 15);

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
        try
        {
          CreatePaymentStatusHistory1();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              export.EabReportSend.RptDetail =
                "Error: Already Exists creating \"RCVPOT\" payment status history for Payee " +
                import.Obligee.Number + " Payment Request # " + NumberToString
                (import.Persistent.SystemGeneratedIdentifier, 15) + " Status ID of " +
                NumberToString
                (import.PersistentRcvpot.SystemGeneratedIdentifier, 15);

              break;
            case ErrorCode.PermittedValueViolation:
              export.EabReportSend.RptDetail =
                "Error: Permitted Value Violation creating \"RCVPOT\" payment status history for Payee " +
                import.Obligee.Number + " Payment Request # " + NumberToString
                (import.Persistent.SystemGeneratedIdentifier, 15) + " Status ID of " +
                NumberToString
                (import.PersistentRcvpot.SystemGeneratedIdentifier, 15);

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
    }
    else
    {
      // 06/05/12 GVandy CQ33868  Do not automatically cancel Warrants < $1.00.
      try
      {
        CreatePaymentStatusHistory2();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            export.EabReportSend.RptDetail =
              "Error: Already Exists creating \"REQ\" payment status history for Payee " +
              import.Obligee.Number + " Payment Request # " + NumberToString
              (import.Persistent.SystemGeneratedIdentifier, 15) + " Status ID of " +
              NumberToString
              (import.PersistentRequested.SystemGeneratedIdentifier, 15);

            break;
          case ErrorCode.PermittedValueViolation:
            export.EabReportSend.RptDetail =
              "Error: Permitted Value Violation creating \"REQ\" payment status history for Payee " +
              import.Obligee.Number + " Payment Request # " + NumberToString
              (import.Persistent.SystemGeneratedIdentifier, 15) + " Status ID of " +
              NumberToString
              (import.PersistentRequested.SystemGeneratedIdentifier, 15);

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
  }

  private void CreatePaymentStatusHistory1()
  {
    var pstGeneratedId = import.PersistentRcvpot.SystemGeneratedIdentifier;
    var prqGeneratedId = import.Persistent.SystemGeneratedIdentifier;
    var systemGeneratedIdentifier = 1;
    var effectiveDate = import.ProgramProcessingInfo.ProcessDate;
    var discontinueDate = import.Maximum.Date;
    var createdBy = global.UserId;
    var createdTimestamp = Now();

    entities.PaymentStatusHistory.Populated = false;
    Update("CreatePaymentStatusHistory1",
      (db, command) =>
      {
        db.SetInt32(command, "pstGeneratedId", pstGeneratedId);
        db.SetInt32(command, "prqGeneratedId", prqGeneratedId);
        db.SetInt32(command, "pymntStatHistId", systemGeneratedIdentifier);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "reasonText", "");
      });

    entities.PaymentStatusHistory.PstGeneratedId = pstGeneratedId;
    entities.PaymentStatusHistory.PrqGeneratedId = prqGeneratedId;
    entities.PaymentStatusHistory.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.PaymentStatusHistory.EffectiveDate = effectiveDate;
    entities.PaymentStatusHistory.DiscontinueDate = discontinueDate;
    entities.PaymentStatusHistory.CreatedBy = createdBy;
    entities.PaymentStatusHistory.CreatedTimestamp = createdTimestamp;
    entities.PaymentStatusHistory.ReasonText = "";
    entities.PaymentStatusHistory.Populated = true;
  }

  private void CreatePaymentStatusHistory2()
  {
    var pstGeneratedId = import.PersistentRequested.SystemGeneratedIdentifier;
    var prqGeneratedId = import.Persistent.SystemGeneratedIdentifier;
    var systemGeneratedIdentifier = 1;
    var effectiveDate = import.ProgramProcessingInfo.ProcessDate;
    var discontinueDate = import.Maximum.Date;
    var createdBy = global.UserId;
    var createdTimestamp = Now();

    entities.PaymentStatusHistory.Populated = false;
    Update("CreatePaymentStatusHistory2",
      (db, command) =>
      {
        db.SetInt32(command, "pstGeneratedId", pstGeneratedId);
        db.SetInt32(command, "prqGeneratedId", prqGeneratedId);
        db.SetInt32(command, "pymntStatHistId", systemGeneratedIdentifier);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "reasonText", "");
      });

    entities.PaymentStatusHistory.PstGeneratedId = pstGeneratedId;
    entities.PaymentStatusHistory.PrqGeneratedId = prqGeneratedId;
    entities.PaymentStatusHistory.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.PaymentStatusHistory.EffectiveDate = effectiveDate;
    entities.PaymentStatusHistory.DiscontinueDate = discontinueDate;
    entities.PaymentStatusHistory.CreatedBy = createdBy;
    entities.PaymentStatusHistory.CreatedTimestamp = createdTimestamp;
    entities.PaymentStatusHistory.ReasonText = "";
    entities.PaymentStatusHistory.Populated = true;
  }

  private void CreatePaymentStatusHistory3()
  {
    var pstGeneratedId = import.PersistentDenied.SystemGeneratedIdentifier;
    var prqGeneratedId = import.Persistent.SystemGeneratedIdentifier;
    var systemGeneratedIdentifier = 1;
    var effectiveDate = import.ProgramProcessingInfo.ProcessDate;
    var discontinueDate = import.Maximum.Date;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var reasonText = "Denied due to potential recovery less than $1.00";

    entities.PaymentStatusHistory.Populated = false;
    Update("CreatePaymentStatusHistory3",
      (db, command) =>
      {
        db.SetInt32(command, "pstGeneratedId", pstGeneratedId);
        db.SetInt32(command, "prqGeneratedId", prqGeneratedId);
        db.SetInt32(command, "pymntStatHistId", systemGeneratedIdentifier);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "reasonText", reasonText);
      });

    entities.PaymentStatusHistory.PstGeneratedId = pstGeneratedId;
    entities.PaymentStatusHistory.PrqGeneratedId = prqGeneratedId;
    entities.PaymentStatusHistory.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.PaymentStatusHistory.EffectiveDate = effectiveDate;
    entities.PaymentStatusHistory.DiscontinueDate = discontinueDate;
    entities.PaymentStatusHistory.CreatedBy = createdBy;
    entities.PaymentStatusHistory.CreatedTimestamp = createdTimestamp;
    entities.PaymentStatusHistory.ReasonText = reasonText;
    entities.PaymentStatusHistory.Populated = true;
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

  private void UpdatePaymentRequest()
  {
    var processDate = local.PaymentRequest.ProcessDate;
    var amount = local.PaymentRequest.Amount;
    var type1 = import.ForUpdate.Type1;

    CheckValid<PaymentRequest>("Type1", type1);
    import.Persistent.Populated = false;
    Update("UpdatePaymentRequest",
      (db, command) =>
      {
        db.SetDate(command, "processDate", processDate);
        db.SetDecimal(command, "amount", amount);
        db.SetString(command, "type", type1);
        db.SetInt32(
          command, "paymentRequestId",
          import.Persistent.SystemGeneratedIdentifier);
      });

    import.Persistent.ProcessDate = processDate;
    import.Persistent.Amount = amount;
    import.Persistent.Type1 = type1;
    import.Persistent.Populated = true;
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
    /// A value of Obligee.
    /// </summary>
    [JsonPropertyName("obligee")]
    public CsePerson Obligee
    {
      get => obligee ??= new();
      set => obligee = value;
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
    /// A value of ForUpdate.
    /// </summary>
    [JsonPropertyName("forUpdate")]
    public PaymentRequest ForUpdate
    {
      get => forUpdate ??= new();
      set => forUpdate = value;
    }

    /// <summary>
    /// A value of PersistentRequested.
    /// </summary>
    [JsonPropertyName("persistentRequested")]
    public PaymentStatus PersistentRequested
    {
      get => persistentRequested ??= new();
      set => persistentRequested = value;
    }

    /// <summary>
    /// A value of PersistentRcvpot.
    /// </summary>
    [JsonPropertyName("persistentRcvpot")]
    public PaymentStatus PersistentRcvpot
    {
      get => persistentRcvpot ??= new();
      set => persistentRcvpot = value;
    }

    /// <summary>
    /// A value of PersistentCancelled.
    /// </summary>
    [JsonPropertyName("persistentCancelled")]
    public PaymentStatus PersistentCancelled
    {
      get => persistentCancelled ??= new();
      set => persistentCancelled = value;
    }

    /// <summary>
    /// A value of PersistentDenied.
    /// </summary>
    [JsonPropertyName("persistentDenied")]
    public PaymentStatus PersistentDenied
    {
      get => persistentDenied ??= new();
      set => persistentDenied = value;
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
    /// A value of Maximum.
    /// </summary>
    [JsonPropertyName("maximum")]
    public DateWorkArea Maximum
    {
      get => maximum ??= new();
      set => maximum = value;
    }

    private CsePerson obligee;
    private PaymentRequest persistent;
    private PaymentRequest forUpdate;
    private PaymentStatus persistentRequested;
    private PaymentStatus persistentRcvpot;
    private PaymentStatus persistentCancelled;
    private PaymentStatus persistentDenied;
    private ProgramProcessingInfo programProcessingInfo;
    private DateWorkArea maximum;
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
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Zzzzzzzzzzzzzzzzzzzzzzzzzz.
    /// </summary>
    [JsonPropertyName("zzzzzzzzzzzzzzzzzzzzzzzzzz")]
    public EabFileHandling Zzzzzzzzzzzzzzzzzzzzzzzzzz
    {
      get => zzzzzzzzzzzzzzzzzzzzzzzzzz ??= new();
      set => zzzzzzzzzzzzzzzzzzzzzzzzzz = value;
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

    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public PaymentRequest Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
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

    private EabFileHandling zzzzzzzzzzzzzzzzzzzzzzzzzz;
    private PaymentStatusHistory paymentStatusHistory;
    private PaymentRequest initialized;
    private PaymentRequest paymentRequest;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of PaymentStatusHistory.
    /// </summary>
    [JsonPropertyName("paymentStatusHistory")]
    public PaymentStatusHistory PaymentStatusHistory
    {
      get => paymentStatusHistory ??= new();
      set => paymentStatusHistory = value;
    }

    private PaymentStatusHistory paymentStatusHistory;
  }
#endregion
}
