// Program: FN_SET_DISB_TRAN_AS_PROCESSED, ID: 372677677, model: 746.
// Short name: SWE02116
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_SET_DISB_TRAN_AS_PROCESSED.
/// </para>
/// <para>
/// RESP: FINANCE
/// This action block will process each disbursement disbursement_transaction 
/// and associate it to the payment_request.
/// </para>
/// </summary>
[Serializable]
public partial class FnSetDisbTranAsProcessed: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_SET_DISB_TRAN_AS_PROCESSED program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnSetDisbTranAsProcessed(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnSetDisbTranAsProcessed.
  /// </summary>
  public FnSetDisbTranAsProcessed(IContext context, Import import, Export export)
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
    // --------------------------------------------------------------------
    // Date	By	IDCR#	Description
    // --------------------------------------------------------------------
    // ??????	??????		Initial code
    // 11/97	govind		Removed the persistent views
    // 020598	govind		Fixed bug in sequence  number for Disb Stat History
    // 05/22/99  Fangman           Added timestamp sort to Status History read 
    // each.
    // --------------------------------------------------------------------
    local.MaxDate.Date = new DateTime(2099, 12, 31);
    local.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;

    if (local.ProgramProcessingInfo.ProcessDate == null)
    {
      local.ProgramProcessingInfo.ProcessDate = Now().Date;
    }

    if (ReadDisbursementStatus())
    {
      if (ReadDisbursementTransaction())
      {
        try
        {
          UpdateDisbursementTransaction();

          // ***** Change the status of the disbursement to Processed by 
          // discontinuing the previous status and creating a new one.
          if (ReadDisbursementStatusHistory())
          {
            local.LastGenerated.SystemGeneratedIdentifier =
              entities.DisbursementStatusHistory.SystemGeneratedIdentifier;

            try
            {
              UpdateDisbursementStatusHistory();
            }
            catch(Exception e1)
            {
              switch(GetErrorCode(e1))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "FN0000_DISB_STAT_HIST_NU_RB";

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "FN0000_DISB_STAT_HIST_PV_RB";

                  break;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_DISB_TRANS_NU_RB";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_DISB_TRANS_PV_RB";

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
        ExitState = "FN0000_DISB_TRANSACTION_NF";
      }
    }
    else
    {
      ExitState = "FN0000_DISB_STATUS_NF";
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
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
          ExitState = "FN0000_DISB_STATUS_HISTORY_AE";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0000_DISB_STATUS_HISTORY_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private void CreateDisbursementStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.DisbursementTransaction.Populated);

    var dbsGeneratedId = entities.Processed.SystemGeneratedIdentifier;
    var dtrGeneratedId =
      entities.DisbursementTransaction.SystemGeneratedIdentifier;
    var cspNumber = entities.DisbursementTransaction.CspNumber;
    var cpaType = entities.DisbursementTransaction.CpaType;
    var systemGeneratedIdentifier =
      local.LastGenerated.SystemGeneratedIdentifier + 1;
    var effectiveDate = local.ProgramProcessingInfo.ProcessDate;
    var discontinueDate = local.MaxDate.Date;
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

  private bool ReadDisbursementStatus()
  {
    entities.Processed.Populated = false;

    return Read("ReadDisbursementStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "disbStatusId", import.Processed.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Processed.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Processed.Populated = true;
      });
  }

  private bool ReadDisbursementStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.DisbursementTransaction.Populated);
    entities.DisbursementStatusHistory.Populated = false;

    return Read("ReadDisbursementStatusHistory",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtrGeneratedId",
          entities.DisbursementTransaction.SystemGeneratedIdentifier);
        db.SetString(
          command, "cspNumber", entities.DisbursementTransaction.CspNumber);
        db.SetString(
          command, "cpaType", entities.DisbursementTransaction.CpaType);
      },
      (db, reader) =>
      {
        entities.DisbursementStatusHistory.DbsGeneratedId =
          db.GetInt32(reader, 0);
        entities.DisbursementStatusHistory.DtrGeneratedId =
          db.GetInt32(reader, 1);
        entities.DisbursementStatusHistory.CspNumber = db.GetString(reader, 2);
        entities.DisbursementStatusHistory.CpaType = db.GetString(reader, 3);
        entities.DisbursementStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.DisbursementStatusHistory.EffectiveDate =
          db.GetDate(reader, 5);
        entities.DisbursementStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.DisbursementStatusHistory.CreatedBy = db.GetString(reader, 7);
        entities.DisbursementStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 8);
        entities.DisbursementStatusHistory.Populated = true;
        CheckValid<DisbursementStatusHistory>("CpaType",
          entities.DisbursementStatusHistory.CpaType);
      });
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
        entities.DisbursementTransaction.ProcessDate =
          db.GetNullableDate(reader, 3);
        entities.DisbursementTransaction.LastUpdatedBy =
          db.GetNullableString(reader, 4);
        entities.DisbursementTransaction.LastUpdateTmst =
          db.GetNullableDateTime(reader, 5);
        entities.DisbursementTransaction.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.DisbursementTransaction.CpaType);
      });
  }

  private void UpdateDisbursementStatusHistory()
  {
    System.Diagnostics.Debug.
      Assert(entities.DisbursementStatusHistory.Populated);

    var discontinueDate = local.ProgramProcessingInfo.ProcessDate;

    entities.DisbursementStatusHistory.Populated = false;
    Update("UpdateDisbursementStatusHistory",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetInt32(
          command, "dbsGeneratedId",
          entities.DisbursementStatusHistory.DbsGeneratedId);
        db.SetInt32(
          command, "dtrGeneratedId",
          entities.DisbursementStatusHistory.DtrGeneratedId);
        db.SetString(
          command, "cspNumber", entities.DisbursementStatusHistory.CspNumber);
        db.SetString(
          command, "cpaType", entities.DisbursementStatusHistory.CpaType);
        db.SetInt32(
          command, "disbStatHistId",
          entities.DisbursementStatusHistory.SystemGeneratedIdentifier);
      });

    entities.DisbursementStatusHistory.DiscontinueDate = discontinueDate;
    entities.DisbursementStatusHistory.Populated = true;
  }

  private void UpdateDisbursementTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.DisbursementTransaction.Populated);

    var processDate = local.ProgramProcessingInfo.ProcessDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdateTmst = Now();

    entities.DisbursementTransaction.Populated = false;
    Update("UpdateDisbursementTransaction",
      (db, command) =>
      {
        db.SetNullableDate(command, "processDate", processDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdateTmst", lastUpdateTmst);
        db.SetString(
          command, "cpaType", entities.DisbursementTransaction.CpaType);
        db.SetString(
          command, "cspNumber", entities.DisbursementTransaction.CspNumber);
        db.SetInt32(
          command, "disbTranId",
          entities.DisbursementTransaction.SystemGeneratedIdentifier);
      });

    entities.DisbursementTransaction.ProcessDate = processDate;
    entities.DisbursementTransaction.LastUpdatedBy = lastUpdatedBy;
    entities.DisbursementTransaction.LastUpdateTmst = lastUpdateTmst;
    entities.DisbursementTransaction.Populated = true;
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
    /// A value of Processed.
    /// </summary>
    [JsonPropertyName("processed")]
    public DisbursementStatus Processed
    {
      get => processed ??= new();
      set => processed = value;
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

    private ProgramProcessingInfo programProcessingInfo;
    private DisbursementStatus processed;
    private DisbursementTransaction disbursementTransaction;
    private CsePerson obligee;
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
    /// A value of LastGenerated.
    /// </summary>
    [JsonPropertyName("lastGenerated")]
    public DisbursementStatusHistory LastGenerated
    {
      get => lastGenerated ??= new();
      set => lastGenerated = value;
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
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public DateWorkArea MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

    private DisbursementStatusHistory lastGenerated;
    private ProgramProcessingInfo programProcessingInfo;
    private DateWorkArea maxDate;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Processed.
    /// </summary>
    [JsonPropertyName("processed")]
    public DisbursementStatus Processed
    {
      get => processed ??= new();
      set => processed = value;
    }

    /// <summary>
    /// A value of DisbursementStatusHistory.
    /// </summary>
    [JsonPropertyName("disbursementStatusHistory")]
    public DisbursementStatusHistory DisbursementStatusHistory
    {
      get => disbursementStatusHistory ??= new();
      set => disbursementStatusHistory = value;
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
    /// A value of Obligee1.
    /// </summary>
    [JsonPropertyName("obligee1")]
    public CsePersonAccount Obligee1
    {
      get => obligee1 ??= new();
      set => obligee1 = value;
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

    private DisbursementStatus processed;
    private DisbursementStatusHistory disbursementStatusHistory;
    private DisbursementTransaction disbursementTransaction;
    private CsePersonAccount obligee1;
    private CsePerson obligee2;
  }
#endregion
}
