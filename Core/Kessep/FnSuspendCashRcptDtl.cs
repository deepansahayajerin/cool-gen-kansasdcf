// Program: FN_SUSPEND_CASH_RCPT_DTL, ID: 372279908, model: 746.
// Short name: SWE02261
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_SUSPEND_CASH_RCPT_DTL.
/// </summary>
[Serializable]
public partial class FnSuspendCashRcptDtl: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_SUSPEND_CASH_RCPT_DTL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnSuspendCashRcptDtl(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnSuspendCashRcptDtl.
  /// </summary>
  public FnSuspendCashRcptDtl(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (!import.Persistant.Populated)
    {
      ExitState = "FN0053_CASH_RCPT_DTL_NF_RB";

      return;
    }

    // IS LOCKED expression is used.
    // Entity is considered to be locked during the call.
    if (!true)
    {
      ExitState = "FN0053_CASH_RCPT_DTL_NF_RB";

      return;
    }

    if (ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus())
    {
      if (entities.ExistingCashReceiptDetailStatus.SystemGeneratedIdentifier ==
        import.HardcodedSuspended.SystemGeneratedIdentifier && Equal
        (entities.ExistingCashReceiptDetailStatHistory.ReasonCodeId,
        import.CashReceiptDetailStatHistory.ReasonCodeId))
      {
        goto Read;
      }

      try
      {
        UpdateCashReceiptDetailStatHistory();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_CASH_RCPT_DTL_S_HST_NU_RB";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_CASH_RCPT_DTL_S_HST_PV_RB";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      if (ReadCashReceiptDetailStatus())
      {
        try
        {
          CreateCashReceiptDetailStatHistory();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_CASH_RCPT_DTL_S_HST_AE_RB";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_CASH_RCPT_DTL_S_HST_PV_RB";

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
        ExitState = "FN0072_CASH_RCPT_DTL_STAT_NF_RB";

        return;
      }
    }
    else
    {
      ExitState = "FN0064_CSH_RCPT_DTL_ST_HST_NF_RB";

      return;
    }

Read:

    if (Equal(import.ApplRunMode.Text8, "BATCH"))
    {
      UseFnB632PrintSuspendInfo();
    }

    UseFnRaiseEventForSuspendedCrd();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (Equal(import.ApplRunMode.Text8, "BATCH"))
      {
        local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();
        ExitState = "ACO_NN0000_ALL_OK";
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail =
          "An error has occurred attempting to generate an event/alert on the previously suspended CRD.";
          
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        local.NeededToWrite.RptDetail = "Exit State: " + local
          .ExitStateWorkArea.Message;
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        local.NeededToWrite.RptDetail =
          "------------------------------------------------------------------------------------------------------------------------------------";
          
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
        }
      }
      else
      {
        ExitState = "ACO_NN0000_ALL_OK";
      }
    }
  }

  private static void MoveCashReceiptDetail1(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.CollectionDate = source.CollectionDate;
    target.ObligorPersonNumber = source.ObligorPersonNumber;
  }

  private static void MoveCashReceiptDetail2(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.ObligorPersonNumber = source.ObligorPersonNumber;
  }

  private static void MoveCashReceiptDetailStatHistory(
    CashReceiptDetailStatHistory source, CashReceiptDetailStatHistory target)
  {
    target.ReasonCodeId = source.ReasonCodeId;
    target.ReasonText = source.ReasonText;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private string UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    return useExport.ExitStateWorkArea.Message;
  }

  private void UseFnB632PrintSuspendInfo()
  {
    var useImport = new FnB632PrintSuspendInfo.Import();
    var useExport = new FnB632PrintSuspendInfo.Export();

    useImport.CashReceipt.SequentialNumber =
      import.CashReceipt.SequentialNumber;
    MoveCashReceiptDetail2(import.Persistant, useImport.CashReceiptDetail);
    MoveCashReceiptDetailStatHistory(import.CashReceiptDetailStatHistory,
      useImport.CashReceiptDetailStatHistory);

    Call(FnB632PrintSuspendInfo.Execute, useImport, useExport);
  }

  private void UseFnRaiseEventForSuspendedCrd()
  {
    var useImport = new FnRaiseEventForSuspendedCrd.Import();
    var useExport = new FnRaiseEventForSuspendedCrd.Export();

    useImport.CashReceipt.SequentialNumber =
      import.CashReceipt.SequentialNumber;
    MoveCashReceiptDetail1(import.Persistant, useImport.CashReceiptDetail);
    useImport.CashReceiptDetailStatHistory.ReasonCodeId =
      import.CashReceiptDetailStatHistory.ReasonCodeId;

    Call(FnRaiseEventForSuspendedCrd.Execute, useImport, useExport);
  }

  private void CreateCashReceiptDetailStatHistory()
  {
    System.Diagnostics.Debug.Assert(import.Persistant.Populated);

    var crdIdentifier = import.Persistant.SequentialIdentifier;
    var crvIdentifier = import.Persistant.CrvIdentifier;
    var cstIdentifier = import.Persistant.CstIdentifier;
    var crtIdentifier = import.Persistant.CrtIdentifier;
    var cdsIdentifier =
      entities.ExistingCashReceiptDetailStatus.SystemGeneratedIdentifier;
    var createdTimestamp = import.Current.Timestamp;
    var reasonCodeId = import.CashReceiptDetailStatHistory.ReasonCodeId ?? "";
    var createdBy = import.UserId.Text8;
    var discontinueDate = import.MaximumDiscontinue.Date;
    var reasonText = import.CashReceiptDetailStatHistory.ReasonText ?? "";

    entities.New1.Populated = false;
    Update("CreateCashReceiptDetailStatHistory",
      (db, command) =>
      {
        db.SetInt32(command, "crdIdentifier", crdIdentifier);
        db.SetInt32(command, "crvIdentifier", crvIdentifier);
        db.SetInt32(command, "cstIdentifier", cstIdentifier);
        db.SetInt32(command, "crtIdentifier", crtIdentifier);
        db.SetInt32(command, "cdsIdentifier", cdsIdentifier);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "reasonCodeId", reasonCodeId);
        db.SetString(command, "createdBy", createdBy);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "reasonText", reasonText);
      });

    entities.New1.CrdIdentifier = crdIdentifier;
    entities.New1.CrvIdentifier = crvIdentifier;
    entities.New1.CstIdentifier = cstIdentifier;
    entities.New1.CrtIdentifier = crtIdentifier;
    entities.New1.CdsIdentifier = cdsIdentifier;
    entities.New1.CreatedTimestamp = createdTimestamp;
    entities.New1.ReasonCodeId = reasonCodeId;
    entities.New1.CreatedBy = createdBy;
    entities.New1.DiscontinueDate = discontinueDate;
    entities.New1.ReasonText = reasonText;
    entities.New1.Populated = true;
  }

  private bool ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus()
  {
    System.Diagnostics.Debug.Assert(import.Persistant.Populated);
    entities.ExistingCashReceiptDetailStatus.Populated = false;
    entities.ExistingCashReceiptDetailStatHistory.Populated = false;

    return Read("ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDate",
          import.MaximumDiscontinue.Date.GetValueOrDefault());
        db.SetInt32(
          command, "crdIdentifier", import.Persistant.SequentialIdentifier);
        db.SetInt32(command, "crvIdentifier", import.Persistant.CrvIdentifier);
        db.SetInt32(command, "cstIdentifier", import.Persistant.CstIdentifier);
        db.SetInt32(command, "crtIdentifier", import.Persistant.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingCashReceiptDetailStatHistory.CrdIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptDetailStatHistory.CrvIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingCashReceiptDetailStatHistory.CstIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingCashReceiptDetailStatHistory.CrtIdentifier =
          db.GetInt32(reader, 3);
        entities.ExistingCashReceiptDetailStatHistory.CdsIdentifier =
          db.GetInt32(reader, 4);
        entities.ExistingCashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.ExistingCashReceiptDetailStatHistory.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.ExistingCashReceiptDetailStatHistory.ReasonCodeId =
          db.GetNullableString(reader, 6);
        entities.ExistingCashReceiptDetailStatHistory.DiscontinueDate =
          db.GetNullableDate(reader, 7);
        entities.ExistingCashReceiptDetailStatus.Populated = true;
        entities.ExistingCashReceiptDetailStatHistory.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailStatus()
  {
    entities.ExistingCashReceiptDetailStatus.Populated = false;

    return Read("ReadCashReceiptDetailStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdetailStatId",
          import.HardcodedSuspended.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingCashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptDetailStatus.Populated = true;
      });
  }

  private void UpdateCashReceiptDetailStatHistory()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingCashReceiptDetailStatHistory.Populated);

    var discontinueDate = import.Current.Date;

    entities.ExistingCashReceiptDetailStatHistory.Populated = false;
    Update("UpdateCashReceiptDetailStatHistory",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetInt32(
          command, "crdIdentifier",
          entities.ExistingCashReceiptDetailStatHistory.CrdIdentifier);
        db.SetInt32(
          command, "crvIdentifier",
          entities.ExistingCashReceiptDetailStatHistory.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          entities.ExistingCashReceiptDetailStatHistory.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          entities.ExistingCashReceiptDetailStatHistory.CrtIdentifier);
        db.SetInt32(
          command, "cdsIdentifier",
          entities.ExistingCashReceiptDetailStatHistory.CdsIdentifier);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.ExistingCashReceiptDetailStatHistory.CreatedTimestamp.
            GetValueOrDefault());
      });

    entities.ExistingCashReceiptDetailStatHistory.DiscontinueDate =
      discontinueDate;
    entities.ExistingCashReceiptDetailStatHistory.Populated = true;
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
    /// A value of Persistant.
    /// </summary>
    [JsonPropertyName("persistant")]
    public CashReceiptDetail Persistant
    {
      get => persistant ??= new();
      set => persistant = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailStatHistory.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatHistory")]
    public CashReceiptDetailStatHistory CashReceiptDetailStatHistory
    {
      get => cashReceiptDetailStatHistory ??= new();
      set => cashReceiptDetailStatHistory = value;
    }

    /// <summary>
    /// A value of UserId.
    /// </summary>
    [JsonPropertyName("userId")]
    public TextWorkArea UserId
    {
      get => userId ??= new();
      set => userId = value;
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
    /// A value of MaximumDiscontinue.
    /// </summary>
    [JsonPropertyName("maximumDiscontinue")]
    public DateWorkArea MaximumDiscontinue
    {
      get => maximumDiscontinue ??= new();
      set => maximumDiscontinue = value;
    }

    /// <summary>
    /// A value of HardcodedSuspended.
    /// </summary>
    [JsonPropertyName("hardcodedSuspended")]
    public CashReceiptDetailStatus HardcodedSuspended
    {
      get => hardcodedSuspended ??= new();
      set => hardcodedSuspended = value;
    }

    /// <summary>
    /// A value of ApplRunMode.
    /// </summary>
    [JsonPropertyName("applRunMode")]
    public TextWorkArea ApplRunMode
    {
      get => applRunMode ??= new();
      set => applRunMode = value;
    }

    private CashReceipt cashReceipt;
    private CashReceiptDetail persistant;
    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private TextWorkArea userId;
    private DateWorkArea current;
    private DateWorkArea maximumDiscontinue;
    private CashReceiptDetailStatus hardcodedSuspended;
    private TextWorkArea applRunMode;
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
    /// A value of CrdCrComboNo.
    /// </summary>
    [JsonPropertyName("crdCrComboNo")]
    public CrdCrComboNo CrdCrComboNo
    {
      get => crdCrComboNo ??= new();
      set => crdCrComboNo = value;
    }

    /// <summary>
    /// A value of HardcodedSuspended.
    /// </summary>
    [JsonPropertyName("hardcodedSuspended")]
    public CashReceiptDetailStatus HardcodedSuspended
    {
      get => hardcodedSuspended ??= new();
      set => hardcodedSuspended = value;
    }

    /// <summary>
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of LastProcessedCashReceipt.
    /// </summary>
    [JsonPropertyName("lastProcessedCashReceipt")]
    public CashReceipt LastProcessedCashReceipt
    {
      get => lastProcessedCashReceipt ??= new();
      set => lastProcessedCashReceipt = value;
    }

    /// <summary>
    /// A value of LastProcessedCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("lastProcessedCashReceiptDetail")]
    public CashReceiptDetail LastProcessedCashReceiptDetail
    {
      get => lastProcessedCashReceiptDetail ??= new();
      set => lastProcessedCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
    }

    private CrdCrComboNo crdCrComboNo;
    private CashReceiptDetailStatus hardcodedSuspended;
    private EabReportSend neededToWrite;
    private EabFileHandling eabFileHandling;
    private CashReceipt lastProcessedCashReceipt;
    private CashReceiptDetail lastProcessedCashReceiptDetail;
    private ExitStateWorkArea exitStateWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingCashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("existingCashReceiptDetailStatus")]
    public CashReceiptDetailStatus ExistingCashReceiptDetailStatus
    {
      get => existingCashReceiptDetailStatus ??= new();
      set => existingCashReceiptDetailStatus = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptDetailStatHistory.
    /// </summary>
    [JsonPropertyName("existingCashReceiptDetailStatHistory")]
    public CashReceiptDetailStatHistory ExistingCashReceiptDetailStatHistory
    {
      get => existingCashReceiptDetailStatHistory ??= new();
      set => existingCashReceiptDetailStatHistory = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public CashReceiptDetailStatHistory New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    private CashReceiptDetailStatus existingCashReceiptDetailStatus;
    private CashReceiptDetailStatHistory existingCashReceiptDetailStatHistory;
    private CashReceiptDetailStatHistory new1;
  }
#endregion
}
