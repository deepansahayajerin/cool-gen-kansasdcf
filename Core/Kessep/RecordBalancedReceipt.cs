// Program: RECORD_BALANCED_RECEIPT, ID: 371727201, model: 746.
// Short name: SWE01051
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: RECORD_BALANCED_RECEIPT.
/// </para>
/// <para>
/// RESP: FINCLMGMNT	
/// This action block will move the status of a cash receipt to balanced and 
/// record the balanced timestamp (denormalized field).	
/// It is an elementary process just in case it doesn't get promoted to the 
/// hierarchy before you read this.
/// </para>
/// </summary>
[Serializable]
public partial class RecordBalancedReceipt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the RECORD_BALANCED_RECEIPT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new RecordBalancedReceipt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of RecordBalancedReceipt.
  /// </summary>
  public RecordBalancedReceipt(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***********************************************************
    // 4/29/97		SHERAZ MALIK	CHANGE CURRENT_DATE
    // 10/12/98	J. Katz		Add SET statements to Cash
    // 				Receipt UPDATE action for
    // 				Last Updated By and Last
    // 				Updated Timestamp.
    // 06/04/99  J. Katz		Analyzed READ statements and
    // 				set read property to Select
    // 				Only where appropriate.
    // ***********************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;
    local.Current.Timestamp = Now();
    UseFnHardcodedCashReceipting();

    if (!ReadCashReceiptStatus())
    {
      ExitState = "FN0108_CASH_RCPT_STAT_NF";

      return;
    }

    if (ReadCashReceipt())
    {
      if (ReadCashReceiptStatusCashReceiptStatusHistory())
      {
        if (entities.CurrentCashReceiptStatus.SystemGeneratedIdentifier != local
          .HardcodedCrsRecorded.SystemGeneratedIdentifier)
        {
          ExitState = "FN0000_INVALID_STAT_4_REQ_ACTION";

          return;
        }
      }
      else
      {
        ExitState = "FN0102_CASH_RCPT_STAT_HIST_NF";

        return;
      }
    }
    else
    {
      ExitState = "FN0084_CASH_RCPT_NF";

      return;
    }

    UseFnChangeCashRcptStatusHist();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    try
    {
      UpdateCashReceipt();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0089_CASH_RCPT_NU_RB";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0091_CASH_RCPT_PV_RB";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private static void MoveCashReceiptStatus(CashReceiptStatus source,
    CashReceiptStatus target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private void UseFnChangeCashRcptStatusHist()
  {
    var useImport = new FnChangeCashRcptStatusHist.Import();
    var useExport = new FnChangeCashRcptStatusHist.Export();

    useImport.NewPersistent.Assign(entities.NewBalanced);
    useImport.CashReceipt.SequentialNumber =
      import.CashReceipt.SequentialNumber;
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      import.CashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      import.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      import.CashReceiptType.SystemGeneratedIdentifier;

    Call(FnChangeCashRcptStatusHist.Execute, useImport, useExport);

    MoveCashReceiptStatus(useImport.NewPersistent, entities.NewBalanced);
  }

  private void UseFnHardcodedCashReceipting()
  {
    var useImport = new FnHardcodedCashReceipting.Import();
    var useExport = new FnHardcodedCashReceipting.Export();

    Call(FnHardcodedCashReceipting.Execute, useImport, useExport);

    local.HardcodedCrsRecorded.SystemGeneratedIdentifier =
      useExport.CrsSystemId.CrsIdRecorded.SystemGeneratedIdentifier;
    local.HardcodedCrsBalanced.SystemGeneratedIdentifier =
      useExport.CrsSystemId.CrsIdBalanced.SystemGeneratedIdentifier;
    local.HardcodedCrsForwarded.SystemGeneratedIdentifier =
      useExport.CrsSystemId.CrsIdForwarded.SystemGeneratedIdentifier;
  }

  private bool ReadCashReceipt()
  {
    entities.CashReceipt.Populated = false;

    return Read("ReadCashReceipt",
      (db, command) =>
      {
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
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 3);
        entities.CashReceipt.BalancedTimestamp =
          db.GetNullableDateTime(reader, 4);
        entities.CashReceipt.LastUpdatedBy = db.GetNullableString(reader, 5);
        entities.CashReceipt.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.CashReceipt.Populated = true;
      });
  }

  private bool ReadCashReceiptStatus()
  {
    entities.NewBalanced.Populated = false;

    return Read("ReadCashReceiptStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "crStatusId",
          local.HardcodedCrsBalanced.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.NewBalanced.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.NewBalanced.Code = db.GetString(reader, 1);
        entities.NewBalanced.Populated = true;
      });
  }

  private bool ReadCashReceiptStatusCashReceiptStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.CurrentCashReceiptStatusHistory.Populated = false;
    entities.CurrentCashReceiptStatus.Populated = false;

    return Read("ReadCashReceiptStatusCashReceiptStatusHistory",
      (db, command) =>
      {
        db.
          SetInt32(command, "crtIdentifier", entities.CashReceipt.CrtIdentifier);
          
        db.
          SetInt32(command, "cstIdentifier", entities.CashReceipt.CstIdentifier);
          
        db.
          SetInt32(command, "crvIdentifier", entities.CashReceipt.CrvIdentifier);
          
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CurrentCashReceiptStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CurrentCashReceiptStatusHistory.CrsIdentifier =
          db.GetInt32(reader, 0);
        entities.CurrentCashReceiptStatusHistory.CrtIdentifier =
          db.GetInt32(reader, 1);
        entities.CurrentCashReceiptStatusHistory.CstIdentifier =
          db.GetInt32(reader, 2);
        entities.CurrentCashReceiptStatusHistory.CrvIdentifier =
          db.GetInt32(reader, 3);
        entities.CurrentCashReceiptStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.CurrentCashReceiptStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.CurrentCashReceiptStatusHistory.Populated = true;
        entities.CurrentCashReceiptStatus.Populated = true;
      });
  }

  private void UpdateCashReceipt()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);

    var balancedTimestamp = import.CashReceipt.BalancedTimestamp;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = local.Current.Timestamp;

    entities.CashReceipt.Populated = false;
    Update("UpdateCashReceipt",
      (db, command) =>
      {
        db.SetNullableDateTime(command, "balTmst", balancedTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.
          SetInt32(command, "crvIdentifier", entities.CashReceipt.CrvIdentifier);
          
        db.
          SetInt32(command, "cstIdentifier", entities.CashReceipt.CstIdentifier);
          
        db.
          SetInt32(command, "crtIdentifier", entities.CashReceipt.CrtIdentifier);
          
      });

    entities.CashReceipt.BalancedTimestamp = balancedTimestamp;
    entities.CashReceipt.LastUpdatedBy = lastUpdatedBy;
    entities.CashReceipt.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CashReceipt.Populated = true;
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

    private CashReceipt cashReceipt;
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
    /// A value of HardcodedCrsRecorded.
    /// </summary>
    [JsonPropertyName("hardcodedCrsRecorded")]
    public CashReceiptStatus HardcodedCrsRecorded
    {
      get => hardcodedCrsRecorded ??= new();
      set => hardcodedCrsRecorded = value;
    }

    /// <summary>
    /// A value of HardcodedCrsBalanced.
    /// </summary>
    [JsonPropertyName("hardcodedCrsBalanced")]
    public CashReceiptStatus HardcodedCrsBalanced
    {
      get => hardcodedCrsBalanced ??= new();
      set => hardcodedCrsBalanced = value;
    }

    /// <summary>
    /// A value of HardcodedCrsForwarded.
    /// </summary>
    [JsonPropertyName("hardcodedCrsForwarded")]
    public CashReceiptStatus HardcodedCrsForwarded
    {
      get => hardcodedCrsForwarded ??= new();
      set => hardcodedCrsForwarded = value;
    }

    private DateWorkArea current;
    private CashReceiptStatus hardcodedCrsRecorded;
    private CashReceiptStatus hardcodedCrsBalanced;
    private CashReceiptStatus hardcodedCrsForwarded;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of NewBalanced.
    /// </summary>
    [JsonPropertyName("newBalanced")]
    public CashReceiptStatus NewBalanced
    {
      get => newBalanced ??= new();
      set => newBalanced = value;
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

    /// <summary>
    /// A value of CurrentCashReceiptStatusHistory.
    /// </summary>
    [JsonPropertyName("currentCashReceiptStatusHistory")]
    public CashReceiptStatusHistory CurrentCashReceiptStatusHistory
    {
      get => currentCashReceiptStatusHistory ??= new();
      set => currentCashReceiptStatusHistory = value;
    }

    /// <summary>
    /// A value of CurrentCashReceiptStatus.
    /// </summary>
    [JsonPropertyName("currentCashReceiptStatus")]
    public CashReceiptStatus CurrentCashReceiptStatus
    {
      get => currentCashReceiptStatus ??= new();
      set => currentCashReceiptStatus = value;
    }

    private CashReceiptStatus newBalanced;
    private CashReceipt cashReceipt;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptType cashReceiptType;
    private CashReceiptStatusHistory currentCashReceiptStatusHistory;
    private CashReceiptStatus currentCashReceiptStatus;
  }
#endregion
}
