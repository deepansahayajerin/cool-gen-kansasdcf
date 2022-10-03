// Program: CREATE_FUND_TRANSACT_STATUS_HIST, ID: 371768516, model: 746.
// Short name: SWE00143
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: CREATE_FUND_TRANSACT_STATUS_HIST.
/// </para>
/// <para>
/// RESP: CASHMGMT
/// This action block will create a fund transaction status history.  It is to 
/// be used by all the processes that change the status of a fund transaction.
/// </para>
/// </summary>
[Serializable]
public partial class CreateFundTransactStatusHist: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CREATE_FUND_TRANSACT_STATUS_HIST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CreateFundTransactStatusHist(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CreateFundTransactStatusHist.
  /// </summary>
  public CreateFundTransactStatusHist(IContext context, Import import,
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
    local.Current.Timestamp = Now();

    try
    {
      CreateFundTransactionStatusHistory();
      MoveFundTransactionStatusHistory(entities.FundTransactionStatusHistory,
        export.FundTransactionStatusHistory);
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0000_FUND_TRANS_STAT_HIST_AE";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0000_FUND_TRANS_STAT_HIST_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private static void MoveFundTransactionStatusHistory(
    FundTransactionStatusHistory source, FundTransactionStatusHistory target)
  {
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private void CreateFundTransactionStatusHistory()
  {
    System.Diagnostics.Debug.Assert(import.FundTransaction.Populated);

    var ftrIdentifier = import.FundTransaction.SystemGeneratedIdentifier;
    var funIdentifier = import.FundTransaction.FunIdentifier;
    var pcaEffectiveDate = import.FundTransaction.PcaEffectiveDate;
    var pcaCode = import.FundTransaction.PcaCode;
    var fttIdentifier = import.FundTransaction.FttIdentifier;
    var ftsIdentifier = import.FundTransactionStatus.SystemGeneratedIdentifier;
    var effectiveTmst = import.FundTransactionStatusHistory.EffectiveTmst;
    var createdBy = global.UserId;
    var createdTimestamp = local.Current.Timestamp;
    var reasonText = import.FundTransactionStatusHistory.ReasonText ?? "";

    entities.FundTransactionStatusHistory.Populated = false;
    Update("CreateFundTransactionStatusHistory",
      (db, command) =>
      {
        db.SetInt32(command, "ftrIdentifier", ftrIdentifier);
        db.SetInt32(command, "funIdentifier", funIdentifier);
        db.SetDate(command, "pcaEffectiveDate", pcaEffectiveDate);
        db.SetString(command, "pcaCode", pcaCode);
        db.SetInt32(command, "fttIdentifier", fttIdentifier);
        db.SetInt32(command, "ftsIdentifier", ftsIdentifier);
        db.SetDateTime(command, "effectiveTmst", effectiveTmst);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "reasonText", reasonText);
      });

    entities.FundTransactionStatusHistory.FtrIdentifier = ftrIdentifier;
    entities.FundTransactionStatusHistory.FunIdentifier = funIdentifier;
    entities.FundTransactionStatusHistory.PcaEffectiveDate = pcaEffectiveDate;
    entities.FundTransactionStatusHistory.PcaCode = pcaCode;
    entities.FundTransactionStatusHistory.FttIdentifier = fttIdentifier;
    entities.FundTransactionStatusHistory.FtsIdentifier = ftsIdentifier;
    entities.FundTransactionStatusHistory.EffectiveTmst = effectiveTmst;
    entities.FundTransactionStatusHistory.CreatedBy = createdBy;
    entities.FundTransactionStatusHistory.CreatedTimestamp = createdTimestamp;
    entities.FundTransactionStatusHistory.ReasonText = reasonText;
    entities.FundTransactionStatusHistory.Populated = true;
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
    /// A value of FundTransactionStatusHistory.
    /// </summary>
    [JsonPropertyName("fundTransactionStatusHistory")]
    public FundTransactionStatusHistory FundTransactionStatusHistory
    {
      get => fundTransactionStatusHistory ??= new();
      set => fundTransactionStatusHistory = value;
    }

    /// <summary>
    /// A value of FundTransaction.
    /// </summary>
    [JsonPropertyName("fundTransaction")]
    public FundTransaction FundTransaction
    {
      get => fundTransaction ??= new();
      set => fundTransaction = value;
    }

    /// <summary>
    /// A value of FundTransactionStatus.
    /// </summary>
    [JsonPropertyName("fundTransactionStatus")]
    public FundTransactionStatus FundTransactionStatus
    {
      get => fundTransactionStatus ??= new();
      set => fundTransactionStatus = value;
    }

    private FundTransactionStatusHistory fundTransactionStatusHistory;
    private FundTransaction fundTransaction;
    private FundTransactionStatus fundTransactionStatus;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of FundTransactionStatusHistory.
    /// </summary>
    [JsonPropertyName("fundTransactionStatusHistory")]
    public FundTransactionStatusHistory FundTransactionStatusHistory
    {
      get => fundTransactionStatusHistory ??= new();
      set => fundTransactionStatusHistory = value;
    }

    private FundTransactionStatusHistory fundTransactionStatusHistory;
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

    private DateWorkArea current;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of FundTransactionStatusHistory.
    /// </summary>
    [JsonPropertyName("fundTransactionStatusHistory")]
    public FundTransactionStatusHistory FundTransactionStatusHistory
    {
      get => fundTransactionStatusHistory ??= new();
      set => fundTransactionStatusHistory = value;
    }

    private FundTransactionStatusHistory fundTransactionStatusHistory;
  }
#endregion
}
