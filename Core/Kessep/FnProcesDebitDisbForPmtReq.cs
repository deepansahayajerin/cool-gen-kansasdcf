// Program: FN_PROCES_DEBIT_DISB_FOR_PMT_REQ, ID: 372673987, model: 746.
// Short name: SWE02584
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_PROCES_DEBIT_DISB_FOR_PMT_REQ.
/// </summary>
[Serializable]
public partial class FnProcesDebitDisbForPmtReq: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_PROCES_DEBIT_DISB_FOR_PMT_REQ program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnProcesDebitDisbForPmtReq(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnProcesDebitDisbForPmtReq.
  /// </summary>
  public FnProcesDebitDisbForPmtReq(IContext context, Import import,
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
    // To process this disbursement three things must be done:
    // 1.  Associate the debit disbursement being processed to the newly created
    // payment request.
    // 2.  "End" the current status of the disbursement.
    // 3.  "Set" the disbursement to a new status of "PROCESSED".
    AssociateDisbursementTransaction();

    try
    {
      UpdateDisbursementTransaction();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          export.EabReportSend.RptDetail =
            "Error:  Not Unique updating the process date for payee " + import
            .PersistentObligee.Number + " Debt Disb ID " + NumberToString
            (import.PersistentDebit.SystemGeneratedIdentifier, 15);

          return;
        case ErrorCode.PermittedValueViolation:
          export.EabReportSend.RptDetail =
            "Error:  Permitted Value updating the process date for payee " + import
            .PersistentObligee.Number + " Debt Disb ID " + NumberToString
            (import.PersistentDebit.SystemGeneratedIdentifier, 15);

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    try
    {
      UpdateDisbursementStatusHistory();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          export.EabReportSend.RptDetail =
            "Error:  Not Unique updating the current status for payee " + import
            .PersistentObligee.Number + " Debt Disb ID " + NumberToString
            (import.PersistentDebit.SystemGeneratedIdentifier, 15);

          return;
        case ErrorCode.PermittedValueViolation:
          export.EabReportSend.RptDetail =
            "Error:  Permitted Value updating the current status for payee " + import
            .PersistentObligee.Number + " Debt Disb ID " + NumberToString
            (import.PersistentDebit.SystemGeneratedIdentifier, 15);

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    try
    {
      CreateDisbursementStatusHistory();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          export.EabReportSend.RptDetail =
            "Error:  Already Exists creating the new status for payee " + import
            .PersistentObligee.Number + " Debt Disb ID " + NumberToString
            (import.PersistentDebit.SystemGeneratedIdentifier, 15);

          break;
        case ErrorCode.PermittedValueViolation:
          export.EabReportSend.RptDetail =
            "Error:  Permitted Value creating the new status for payee " + import
            .PersistentObligee.Number + " Debt Disb ID " + NumberToString
            (import.PersistentDebit.SystemGeneratedIdentifier, 15);

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private void AssociateDisbursementTransaction()
  {
    System.Diagnostics.Debug.Assert(import.PersistentDebit.Populated);

    var prqGeneratedId =
      import.PersistentPaymentRequest.SystemGeneratedIdentifier;

    import.PersistentDebit.Populated = false;
    Update("AssociateDisbursementTransaction",
      (db, command) =>
      {
        db.SetNullableInt32(command, "prqGeneratedId", prqGeneratedId);
        db.SetString(command, "cpaType", import.PersistentDebit.CpaType);
        db.SetString(command, "cspNumber", import.PersistentDebit.CspNumber);
        db.SetInt32(
          command, "disbTranId",
          import.PersistentDebit.SystemGeneratedIdentifier);
      });

    import.PersistentDebit.PrqGeneratedId = prqGeneratedId;
    import.PersistentDebit.Populated = true;
  }

  private void CreateDisbursementStatusHistory()
  {
    System.Diagnostics.Debug.Assert(import.PersistentDebit.Populated);

    var dbsGeneratedId = import.PersistentProcessed.SystemGeneratedIdentifier;
    var dtrGeneratedId = import.PersistentDebit.SystemGeneratedIdentifier;
    var cspNumber = import.PersistentDebit.CspNumber;
    var cpaType = import.PersistentDebit.CpaType;
    var systemGeneratedIdentifier =
      import.PersistentDisbursementStatusHistory.SystemGeneratedIdentifier + 1;
    var effectiveDate = import.ProgramProcessingInfo.ProcessDate;
    var discontinueDate = import.Maximum.Date;
    var createdBy = global.UserId;
    var createdTimestamp = Now();

    CheckValid<DisbursementStatusHistory>("CpaType", cpaType);
    entities.DisbursementStatusHistory.Populated = false;
    Update("CreateDisbursementStatusHistory",
      (db, command) =>
      {
        db.SetInt32(command, "dbsGeneratedId", dbsGeneratedId);
        db.SetInt32(command, "dtrGeneratedId", dtrGeneratedId);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "cpaType", cpaType);
        db.SetInt32(command, "disbStatHistId", systemGeneratedIdentifier);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "reasonText", "");
        db.SetNullableString(command, "suppressionReason", "");
      });

    entities.DisbursementStatusHistory.DbsGeneratedId = dbsGeneratedId;
    entities.DisbursementStatusHistory.DtrGeneratedId = dtrGeneratedId;
    entities.DisbursementStatusHistory.CspNumber = cspNumber;
    entities.DisbursementStatusHistory.CpaType = cpaType;
    entities.DisbursementStatusHistory.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.DisbursementStatusHistory.EffectiveDate = effectiveDate;
    entities.DisbursementStatusHistory.DiscontinueDate = discontinueDate;
    entities.DisbursementStatusHistory.CreatedBy = createdBy;
    entities.DisbursementStatusHistory.CreatedTimestamp = createdTimestamp;
    entities.DisbursementStatusHistory.Populated = true;
  }

  private void UpdateDisbursementStatusHistory()
  {
    System.Diagnostics.Debug.Assert(
      import.PersistentDisbursementStatusHistory.Populated);

    var discontinueDate = import.ProgramProcessingInfo.ProcessDate;
    var reasonText = "PROCESSED";

    import.PersistentDisbursementStatusHistory.Populated = false;
    Update("UpdateDisbursementStatusHistory",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "reasonText", reasonText);
        db.SetInt32(
          command, "dbsGeneratedId",
          import.PersistentDisbursementStatusHistory.DbsGeneratedId);
        db.SetInt32(
          command, "dtrGeneratedId",
          import.PersistentDisbursementStatusHistory.DtrGeneratedId);
        db.SetString(
          command, "cspNumber",
          import.PersistentDisbursementStatusHistory.CspNumber);
        db.SetString(
          command, "cpaType",
          import.PersistentDisbursementStatusHistory.CpaType);
        db.SetInt32(
          command, "disbStatHistId",
          import.PersistentDisbursementStatusHistory.SystemGeneratedIdentifier);
          
      });

    import.PersistentDisbursementStatusHistory.DiscontinueDate =
      discontinueDate;
    import.PersistentDisbursementStatusHistory.ReasonText = reasonText;
    import.PersistentDisbursementStatusHistory.Populated = true;
  }

  private void UpdateDisbursementTransaction()
  {
    System.Diagnostics.Debug.Assert(import.PersistentDebit.Populated);

    var processDate = import.ProgramProcessingInfo.ProcessDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdateTmst = Now();

    import.PersistentDebit.Populated = false;
    Update("UpdateDisbursementTransaction",
      (db, command) =>
      {
        db.SetNullableDate(command, "processDate", processDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdateTmst", lastUpdateTmst);
        db.SetString(command, "cpaType", import.PersistentDebit.CpaType);
        db.SetString(command, "cspNumber", import.PersistentDebit.CspNumber);
        db.SetInt32(
          command, "disbTranId",
          import.PersistentDebit.SystemGeneratedIdentifier);
      });

    import.PersistentDebit.ProcessDate = processDate;
    import.PersistentDebit.LastUpdatedBy = lastUpdatedBy;
    import.PersistentDebit.LastUpdateTmst = lastUpdateTmst;
    import.PersistentDebit.Populated = true;
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
    /// A value of PersistentObligee.
    /// </summary>
    [JsonPropertyName("persistentObligee")]
    public CsePerson PersistentObligee
    {
      get => persistentObligee ??= new();
      set => persistentObligee = value;
    }

    /// <summary>
    /// A value of PersistentPaymentRequest.
    /// </summary>
    [JsonPropertyName("persistentPaymentRequest")]
    public PaymentRequest PersistentPaymentRequest
    {
      get => persistentPaymentRequest ??= new();
      set => persistentPaymentRequest = value;
    }

    /// <summary>
    /// A value of PersistentDebit.
    /// </summary>
    [JsonPropertyName("persistentDebit")]
    public DisbursementTransaction PersistentDebit
    {
      get => persistentDebit ??= new();
      set => persistentDebit = value;
    }

    /// <summary>
    /// A value of PersistentDisbursementStatusHistory.
    /// </summary>
    [JsonPropertyName("persistentDisbursementStatusHistory")]
    public DisbursementStatusHistory PersistentDisbursementStatusHistory
    {
      get => persistentDisbursementStatusHistory ??= new();
      set => persistentDisbursementStatusHistory = value;
    }

    /// <summary>
    /// A value of PersistentProcessed.
    /// </summary>
    [JsonPropertyName("persistentProcessed")]
    public DisbursementStatus PersistentProcessed
    {
      get => persistentProcessed ??= new();
      set => persistentProcessed = value;
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

    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    private CsePerson persistentObligee;
    private PaymentRequest persistentPaymentRequest;
    private DisbursementTransaction persistentDebit;
    private DisbursementStatusHistory persistentDisbursementStatusHistory;
    private DisbursementStatus persistentProcessed;
    private DateWorkArea maximum;
    private ProgramProcessingInfo programProcessingInfo;
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
    /// A value of DisbursementStatusHistory.
    /// </summary>
    [JsonPropertyName("disbursementStatusHistory")]
    public DisbursementStatusHistory DisbursementStatusHistory
    {
      get => disbursementStatusHistory ??= new();
      set => disbursementStatusHistory = value;
    }

    private DisbursementStatusHistory disbursementStatusHistory;
  }
#endregion
}
