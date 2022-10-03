// Program: FN_CREATE_PAYMENT_STATUS_HIST, ID: 372430659, model: 746.
// Short name: SWE00385
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_CREATE_PAYMENT_STATUS_HIST.
/// </para>
/// <para>
/// RESP: FINANCE
/// This action block will create a payment status history.
/// </para>
/// </summary>
[Serializable]
public partial class FnCreatePaymentStatusHist: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CREATE_PAYMENT_STATUS_HIST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCreatePaymentStatusHist(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCreatePaymentStatusHist.
  /// </summary>
  public FnCreatePaymentStatusHist(IContext context, Import import,
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
    // ??????	??????		Initial code
    // 112097	govind		Removed the persistent views
    // 020599  N.Engoor        Added READ to update the discontinue date for the
    // last history record before creating a new one.
    // 05/22/99  Fangman   Added changes discovered in walkthru.
    // 06/17/99  Fangman   Changed read of existing status history to exclude 
    // qualification based upon status.  Previously read was looking for status
    // history assoc to status that the payment was about to be set to.
    // ---------------------------------------------
    // ***** Set the status for this payment request.
    local.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;

    if (local.ProgramProcessingInfo.ProcessDate == null)
    {
      local.ProgramProcessingInfo.ProcessDate = Now().Date;
    }

    if (!ReadPaymentStatus())
    {
      ExitState = "PAYMENT_STATUS_NF";

      return;
    }

    if (!ReadPaymentRequest())
    {
      ExitState = "FN0000_PAYMENT_REQUEST_NF";

      return;
    }

    // --------------------------
    // N.Engoor  -  02/05/99
    // Before creating a new Payment request status history record update the 
    // previous one with the discontinue date set to the process date.
    // --------------------------
    local.Max.Date = new DateTime(2099, 12, 31);

    if (ReadPaymentStatusHistory())
    {
      local.PaymentStatusHistory.SystemGeneratedIdentifier =
        entities.PaymentStatusHistory.SystemGeneratedIdentifier;

      try
      {
        UpdatePaymentStatusHistory();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_PMNT_STAT_HIST_NU";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_PMNT_STAT_HIST_PV";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    else
    {
      // This is OK if this is a new Payment Request without any preceeding 
      // statuses.
    }

    try
    {
      CreatePaymentStatusHistory();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0000_PYMNT_STAT_HIST_AE_RB";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "ZD_FN0000_PYMNT_STAT_HIST_PV_RB1";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private void CreatePaymentStatusHistory()
  {
    var pstGeneratedId = entities.ReqOrRcvpot.SystemGeneratedIdentifier;
    var prqGeneratedId = entities.PaymentRequest.SystemGeneratedIdentifier;
    var systemGeneratedIdentifier =
      local.PaymentStatusHistory.SystemGeneratedIdentifier + 1;
    var effectiveDate = import.ProgramProcessingInfo.ProcessDate;
    var discontinueDate = local.Max.Date;
    var createdBy = global.UserId;
    var createdTimestamp = Now();

    entities.PaymentStatusHistory.Populated = false;
    Update("CreatePaymentStatusHistory",
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
    entities.PaymentStatusHistory.Populated = true;
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
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 1);
        entities.PaymentRequest.Populated = true;
      });
  }

  private bool ReadPaymentStatus()
  {
    entities.ReqOrRcvpot.Populated = false;

    return Read("ReadPaymentStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "paymentStatusId",
          import.Requested.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ReqOrRcvpot.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.ReqOrRcvpot.Populated = true;
      });
  }

  private bool ReadPaymentStatusHistory()
  {
    entities.PaymentStatusHistory.Populated = false;

    return Read("ReadPaymentStatusHistory",
      (db, command) =>
      {
        db.SetInt32(
          command, "prqGeneratedId",
          entities.PaymentRequest.SystemGeneratedIdentifier);
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PaymentStatusHistory.PstGeneratedId = db.GetInt32(reader, 0);
        entities.PaymentStatusHistory.PrqGeneratedId = db.GetInt32(reader, 1);
        entities.PaymentStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.PaymentStatusHistory.EffectiveDate = db.GetDate(reader, 3);
        entities.PaymentStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.PaymentStatusHistory.CreatedBy = db.GetString(reader, 5);
        entities.PaymentStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.PaymentStatusHistory.Populated = true;
      });
  }

  private void UpdatePaymentStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.PaymentStatusHistory.Populated);

    var discontinueDate = local.ProgramProcessingInfo.ProcessDate;

    entities.PaymentStatusHistory.Populated = false;
    Update("UpdatePaymentStatusHistory",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
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

    entities.PaymentStatusHistory.DiscontinueDate = discontinueDate;
    entities.PaymentStatusHistory.Populated = true;
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
    /// A value of Requested.
    /// </summary>
    [JsonPropertyName("requested")]
    public PaymentStatus Requested
    {
      get => requested ??= new();
      set => requested = value;
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
    private PaymentStatus requested;
    private PaymentRequest paymentRequest;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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

    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
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
    /// A value of ZdeleteMeZzzzzzzzzzzzzzzzzzzzz.
    /// </summary>
    [JsonPropertyName("zdeleteMeZzzzzzzzzzzzzzzzzzzzz")]
    public Common ZdeleteMeZzzzzzzzzzzzzzzzzzzzz
    {
      get => zdeleteMeZzzzzzzzzzzzzzzzzzzzz ??= new();
      set => zdeleteMeZzzzzzzzzzzzzzzzzzzzz = value;
    }

    private PaymentStatusHistory paymentStatusHistory;
    private DateWorkArea max;
    private ProgramProcessingInfo programProcessingInfo;
    private Common zdeleteMeZzzzzzzzzzzzzzzzzzzzz;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ReqOrRcvpot.
    /// </summary>
    [JsonPropertyName("reqOrRcvpot")]
    public PaymentStatus ReqOrRcvpot
    {
      get => reqOrRcvpot ??= new();
      set => reqOrRcvpot = value;
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
    /// A value of PaymentStatusHistory.
    /// </summary>
    [JsonPropertyName("paymentStatusHistory")]
    public PaymentStatusHistory PaymentStatusHistory
    {
      get => paymentStatusHistory ??= new();
      set => paymentStatusHistory = value;
    }

    private PaymentStatus reqOrRcvpot;
    private PaymentRequest paymentRequest;
    private PaymentStatusHistory paymentStatusHistory;
  }
#endregion
}
