// Program: FN_REIP_CHANGE_SUSP_TO_ADJ, ID: 373004129, model: 746.
// Short name: SWE02429
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_REIP_CHANGE_SUSP_TO_ADJ.
/// </summary>
[Serializable]
public partial class FnReipChangeSuspToAdj: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_REIP_CHANGE_SUSP_TO_ADJ program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnReipChangeSuspToAdj(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnReipChangeSuspToAdj.
  /// </summary>
  public FnReipChangeSuspToAdj(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // 01/31/00  P. Phinney  H00084245  Add PFkey to change Susp to Adj
    // ---------------------------------------------------------------------
    // --------------------------------------------------------------------
    // Set up initial values for local views.
    // --------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Timestamp = Now();
    local.Current.Date = Now().Date;
    local.Max.Date = UseCabSetMaximumDiscontinueDate();
    UseFnHardcodedCashReceipting();

    // ----------------------------------------------------------------------
    // Read the new ADJUSTED Cash Receipt Detail Status
    // ----------------------------------------------------------------------
    if (!ReadCashReceiptDetailStatus())
    {
      ExitState = "FN0071_CASH_RCPT_DTL_STAT_NF";

      return;
    }

    // --------------------------------------------------------------------
    // Read cash receipt to be updated.
    // --------------------------------------------------------------------
    if (ReadCashReceipt())
    {
      // -->  CONTINUE
    }
    else
    {
      ExitState = "FN0086_CASH_RCPT_NF_RB";

      return;
    }

    // --------------------------------------------------------------------
    // Read cash receipt detail information.  If the detail has a
    // Distributed Amount or Refunded Amount greater than zero,
    // record cannot be changed. 
    // -----------------------------------------------------------------
    if (ReadCashReceiptDetail())
    {
      if (Lt(0, entities.CashReceiptDetail.DistributedAmount) || Lt
        (0, entities.CashReceiptDetail.RefundedAmount))
      {
        ExitState = "FN0000_COLL_OR_REF_EXIST_NO_UD";

        return;
      }
      else
      {
        // -->  CONTINUE
      }
    }
    else
    {
      ExitState = "FN0000_CASH_RECEIPT_DETAIL_NF_RB";

      return;
    }

    // --------------------------------------------------------------------
    // Validate that active status is SUSP.
    // The cash receipt detail must be in this status for
    // the update action.
    // --------------------------------------------------------------------
    if (ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus())
    {
      if (entities.ActiveCashReceiptDetailStatus.SystemGeneratedIdentifier != local
        .HardcodedCrdsSuspended.SystemGeneratedIdentifier)
      {
        ExitState = "FN0000_PAYMENT_HIST_NOT_SUSP";

        return;
      }
    }
    else
    {
      ExitState = "FN0064_CSH_RCPT_DTL_ST_HST_NF_RB";

      return;
    }

    // -----------------------------------------------------------
    // The active status is SUSP, change the status to ADJ.
    // -----------------------------------------------------------
    // Set the association to SUSPENDED to end TODAY
    try
    {
      UpdateCashReceiptDetailStatHistory();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0065_CSH_RCPT_DTL_ST_HST_NU_RB";

          return;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0068_CSH_RCPT_DTL_ST_HS_PV_RB";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    // Create the association to ADJUSTED
    try
    {
      CreateCashReceiptDetailStatHistory();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0067_CSH_RCPT_DTL_ST_HS_AE_RB";

          return;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0068_CSH_RCPT_DTL_ST_HS_PV_RB";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    // -------------------------------------------------------------
    // Bypass manual distribution when releasing payment
    // history records per PR #238.
    // No Cash Receipt Detail History record is required since the
    // Override Manual Dist Ind is not part of that table.
    // JLK  08/26/99
    // -------------------------------------------------------------
    try
    {
      UpdateCashReceiptDetail();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0000_CASH_RCPT_DTL_NU_RB";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0000_CASH_RCPT_DTL_PV_RB";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseFnHardcodedCashReceipting()
  {
    var useImport = new FnHardcodedCashReceipting.Import();
    var useExport = new FnHardcodedCashReceipting.Export();

    Call(FnHardcodedCashReceipting.Execute, useImport, useExport);

    local.HardcodedCrdsSuspended.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdSuspended.SystemGeneratedIdentifier;
    local.HardcodedCrdsAdjusted.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdAdjusted.SystemGeneratedIdentifier;
  }

  private void CreateCashReceiptDetailStatHistory()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);

    var crdIdentifier = entities.CashReceiptDetail.SequentialIdentifier;
    var crvIdentifier = entities.CashReceiptDetail.CrvIdentifier;
    var cstIdentifier = entities.CashReceiptDetail.CstIdentifier;
    var crtIdentifier = entities.CashReceiptDetail.CrtIdentifier;
    var cdsIdentifier = entities.NewAdjusted.SystemGeneratedIdentifier;
    var createdTimestamp = local.Current.Timestamp;
    var createdBy = global.UserId;
    var discontinueDate = local.Max.Date;

    entities.ActiveCashReceiptDetailStatHistory.Populated = false;
    Update("CreateCashReceiptDetailStatHistory",
      (db, command) =>
      {
        db.SetInt32(command, "crdIdentifier", crdIdentifier);
        db.SetInt32(command, "crvIdentifier", crvIdentifier);
        db.SetInt32(command, "cstIdentifier", cstIdentifier);
        db.SetInt32(command, "crtIdentifier", crtIdentifier);
        db.SetInt32(command, "cdsIdentifier", cdsIdentifier);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "reasonCodeId", "");
        db.SetString(command, "createdBy", createdBy);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "reasonText", "");
      });

    entities.ActiveCashReceiptDetailStatHistory.CrdIdentifier = crdIdentifier;
    entities.ActiveCashReceiptDetailStatHistory.CrvIdentifier = crvIdentifier;
    entities.ActiveCashReceiptDetailStatHistory.CstIdentifier = cstIdentifier;
    entities.ActiveCashReceiptDetailStatHistory.CrtIdentifier = crtIdentifier;
    entities.ActiveCashReceiptDetailStatHistory.CdsIdentifier = cdsIdentifier;
    entities.ActiveCashReceiptDetailStatHistory.CreatedTimestamp =
      createdTimestamp;
    entities.ActiveCashReceiptDetailStatHistory.CreatedBy = createdBy;
    entities.ActiveCashReceiptDetailStatHistory.DiscontinueDate =
      discontinueDate;
    entities.ActiveCashReceiptDetailStatHistory.Populated = true;
  }

  private bool ReadCashReceipt()
  {
    entities.CashReceipt.Populated = false;

    return Read("ReadCashReceipt",
      (db, command) =>
      {
        db.SetInt32(
          command, "cashReceiptId", import.CashReceipt.SequentialNumber);
      },
      (db, reader) =>
      {
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 3);
        entities.CashReceipt.Populated = true;
      });
  }

  private bool ReadCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetail",
      (db, command) =>
      {
        db.
          SetInt32(command, "crtIdentifier", entities.CashReceipt.CrtIdentifier);
          
        db.
          SetInt32(command, "cstIdentifier", entities.CashReceipt.CstIdentifier);
          
        db.
          SetInt32(command, "crvIdentifier", entities.CashReceipt.CrvIdentifier);
          
        db.SetInt32(
          command, "crdId", import.CashReceiptDetail.SequentialIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.AdjustmentInd =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 6);
        entities.CashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 7);
        entities.CashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 8);
        entities.CashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 9);
        entities.CashReceiptDetail.OverrideManualDistInd =
          db.GetNullableString(reader, 10);
        entities.CashReceiptDetail.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.ActiveCashReceiptDetailStatHistory.Populated = false;
    entities.ActiveCashReceiptDetailStatus.Populated = false;

    return Read("ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdIdentifier",
          entities.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ActiveCashReceiptDetailStatHistory.CrdIdentifier =
          db.GetInt32(reader, 0);
        entities.ActiveCashReceiptDetailStatHistory.CrvIdentifier =
          db.GetInt32(reader, 1);
        entities.ActiveCashReceiptDetailStatHistory.CstIdentifier =
          db.GetInt32(reader, 2);
        entities.ActiveCashReceiptDetailStatHistory.CrtIdentifier =
          db.GetInt32(reader, 3);
        entities.ActiveCashReceiptDetailStatHistory.CdsIdentifier =
          db.GetInt32(reader, 4);
        entities.ActiveCashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.ActiveCashReceiptDetailStatHistory.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.ActiveCashReceiptDetailStatHistory.CreatedBy =
          db.GetString(reader, 6);
        entities.ActiveCashReceiptDetailStatHistory.DiscontinueDate =
          db.GetNullableDate(reader, 7);
        entities.ActiveCashReceiptDetailStatHistory.Populated = true;
        entities.ActiveCashReceiptDetailStatus.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailStatus()
  {
    entities.NewAdjusted.Populated = false;

    return Read("ReadCashReceiptDetailStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdetailStatId",
          local.HardcodedCrdsAdjusted.SystemGeneratedIdentifier);
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.NewAdjusted.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.NewAdjusted.DiscontinueDate = db.GetNullableDate(reader, 1);
        entities.NewAdjusted.Populated = true;
      });
  }

  private void UpdateCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();
    var collectionAmtFullyAppliedInd = "Y";

    CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
      collectionAmtFullyAppliedInd);
    entities.CashReceiptDetail.Populated = false;
    Update("UpdateCashReceiptDetail",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableString(
          command, "collamtApplInd", collectionAmtFullyAppliedInd);
        db.SetNullableString(
          command, "ovrrdMnlDistInd", collectionAmtFullyAppliedInd);
        db.SetInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
        db.SetInt32(
          command, "crdId", entities.CashReceiptDetail.SequentialIdentifier);
      });

    entities.CashReceiptDetail.LastUpdatedBy = lastUpdatedBy;
    entities.CashReceiptDetail.LastUpdatedTmst = lastUpdatedTmst;
    entities.CashReceiptDetail.CollectionAmtFullyAppliedInd =
      collectionAmtFullyAppliedInd;
    entities.CashReceiptDetail.OverrideManualDistInd =
      collectionAmtFullyAppliedInd;
    entities.CashReceiptDetail.Populated = true;
  }

  private void UpdateCashReceiptDetailStatHistory()
  {
    System.Diagnostics.Debug.Assert(
      entities.ActiveCashReceiptDetailStatHistory.Populated);

    var discontinueDate = local.Current.Date;

    entities.ActiveCashReceiptDetailStatHistory.Populated = false;
    Update("UpdateCashReceiptDetailStatHistory",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetInt32(
          command, "crdIdentifier",
          entities.ActiveCashReceiptDetailStatHistory.CrdIdentifier);
        db.SetInt32(
          command, "crvIdentifier",
          entities.ActiveCashReceiptDetailStatHistory.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          entities.ActiveCashReceiptDetailStatHistory.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          entities.ActiveCashReceiptDetailStatHistory.CrtIdentifier);
        db.SetInt32(
          command, "cdsIdentifier",
          entities.ActiveCashReceiptDetailStatHistory.CdsIdentifier);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.ActiveCashReceiptDetailStatHistory.CreatedTimestamp.
            GetValueOrDefault());
      });

    entities.ActiveCashReceiptDetailStatHistory.DiscontinueDate =
      discontinueDate;
    entities.ActiveCashReceiptDetailStatHistory.Populated = true;
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
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
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

    private CashReceiptDetail cashReceiptDetail;
    private CashReceipt cashReceipt;
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of HardcodedCrdsSuspended.
    /// </summary>
    [JsonPropertyName("hardcodedCrdsSuspended")]
    public CashReceiptDetailStatus HardcodedCrdsSuspended
    {
      get => hardcodedCrdsSuspended ??= new();
      set => hardcodedCrdsSuspended = value;
    }

    /// <summary>
    /// A value of HardcodedCrdsAdjusted.
    /// </summary>
    [JsonPropertyName("hardcodedCrdsAdjusted")]
    public CashReceiptDetailStatus HardcodedCrdsAdjusted
    {
      get => hardcodedCrdsAdjusted ??= new();
      set => hardcodedCrdsAdjusted = value;
    }

    private DateWorkArea current;
    private DateWorkArea max;
    private CashReceiptDetailStatus hardcodedCrdsSuspended;
    private CashReceiptDetailStatus hardcodedCrdsAdjusted;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
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
    /// A value of ActiveCashReceiptDetailStatHistory.
    /// </summary>
    [JsonPropertyName("activeCashReceiptDetailStatHistory")]
    public CashReceiptDetailStatHistory ActiveCashReceiptDetailStatHistory
    {
      get => activeCashReceiptDetailStatHistory ??= new();
      set => activeCashReceiptDetailStatHistory = value;
    }

    /// <summary>
    /// A value of ActiveCashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("activeCashReceiptDetailStatus")]
    public CashReceiptDetailStatus ActiveCashReceiptDetailStatus
    {
      get => activeCashReceiptDetailStatus ??= new();
      set => activeCashReceiptDetailStatus = value;
    }

    /// <summary>
    /// A value of NewAdjusted.
    /// </summary>
    [JsonPropertyName("newAdjusted")]
    public CashReceiptDetailStatus NewAdjusted
    {
      get => newAdjusted ??= new();
      set => newAdjusted = value;
    }

    private CashReceiptDetail cashReceiptDetail;
    private CashReceipt cashReceipt;
    private CashReceiptDetailStatHistory activeCashReceiptDetailStatHistory;
    private CashReceiptDetailStatus activeCashReceiptDetailStatus;
    private CashReceiptDetailStatus newAdjusted;
  }
#endregion
}
