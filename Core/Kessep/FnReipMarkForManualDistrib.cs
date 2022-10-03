// Program: FN_REIP_MARK_FOR_MANUAL_DISTRIB, ID: 372418913, model: 746.
// Short name: SWE02432
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_REIP_MARK_FOR_MANUAL_DISTRIB.
/// </summary>
[Serializable]
public partial class FnReipMarkForManualDistrib: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_REIP_MARK_FOR_MANUAL_DISTRIB program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnReipMarkForManualDistrib(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnReipMarkForManualDistrib.
  /// </summary>
  public FnReipMarkForManualDistrib(IContext context, Import import,
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
    // ---------------------------------------------------------------------
    //                              Change Log
    // ---------------------------------------------------------------------
    // Date		Developer	Description
    // ---------------------------------------------------------------------
    // 06/08/99	J. Katz		Analyzed READ statements and changed
    // 				read property to Select Only where
    // 				appropriate.
    // ---------------------------------------------------------------------
    // -------------------------------------------------------------------
    // Set up local views.
    // -------------------------------------------------------------------
    local.Current.Timestamp = Now();
    local.Current.Date = Now().Date;
    local.Max.Date = UseCabSetMaximumDiscontinueDate();
    UseFnHardcodedCashReceipting();

    // -------------------------------------------------------------------
    // Read Cash Receipt information.
    // -------------------------------------------------------------------
    if (!ReadCashReceipt())
    {
      ExitState = "FN0086_CASH_RCPT_NF_RB";

      return;
    }

    if (ReadCashReceiptStatusHistoryCashReceiptStatus())
    {
      // ----------------------------------------------------------------
      // Cash Receipt must be in REC status.
      // ----------------------------------------------------------------
      if (entities.ActiveCashReceiptStatus.SystemGeneratedIdentifier != local
        .HardcodedCrsReceipted.SystemGeneratedIdentifier)
      {
        ExitState = "FN0000_INVALD_STAT_4_REQ_ACT_RB";

        return;
      }
    }
    else
    {
      if (!ReadCashReceiptStatus())
      {
        ExitState = "FN0109_CASH_RCPT_STAT_NF_RB";
      }

      try
      {
        CreateCashReceiptStatusHistory();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0101_CASH_RCPT_STAT_HIST_AE_RB";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0105_CASH_RCPT_STAT_HIST_PV_RB";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    // -------------------------------------------------------------------
    // Read Cash Receipt Detail information.
    // -------------------------------------------------------------------
    if (ReadCashReceiptDetail())
    {
      if (AsChar(entities.CashReceiptDetail.CollectionAmtFullyAppliedInd) == 'Y'
        )
      {
        ExitState = "FN0000_COLL_FULLY_APPLIED";

        return;
      }
    }
    else
    {
      ExitState = "FN0053_CASH_RCPT_DTL_NF_RB";

      return;
    }

    if (ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus())
    {
      // ----------------------------------------------------------------
      // Cash Receipt Detail must be in REC status.
      // ----------------------------------------------------------------
      if (entities.ActiveCashReceiptDetailStatus.SystemGeneratedIdentifier != local
        .HardcodedCrdsRecorded.SystemGeneratedIdentifier)
      {
        ExitState = "FN0000_INVALD_STAT_4_REQ_ACT_RB";

        return;
      }
    }
    else
    {
      ExitState = "FN0064_CSH_RCPT_DTL_ST_HST_NF_RB";

      return;
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

    if (!ReadCashReceiptDetailStatus())
    {
      ExitState = "FN0072_CASH_RCPT_DTL_STAT_NF_RB";

      return;
    }

    try
    {
      CreateCashReceiptDetailStatHistory();
      export.New1.Code = entities.NewSuspended.Code;
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0067_CSH_RCPT_DTL_ST_HS_AE_RB";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0068_CSH_RCPT_DTL_ST_HS_PV_RB";

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

    local.HardcodedCrsReceipted.SystemGeneratedIdentifier =
      useExport.CrsSystemId.CrsIdRecorded.SystemGeneratedIdentifier;
    local.HardcodedCrdsRecorded.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdRecorded.SystemGeneratedIdentifier;
    local.HardcodedCrdsSuspended.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdSuspended.SystemGeneratedIdentifier;
  }

  private void CreateCashReceiptDetailStatHistory()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);

    var crdIdentifier = entities.CashReceiptDetail.SequentialIdentifier;
    var crvIdentifier = entities.CashReceiptDetail.CrvIdentifier;
    var cstIdentifier = entities.CashReceiptDetail.CstIdentifier;
    var crtIdentifier = entities.CashReceiptDetail.CrtIdentifier;
    var cdsIdentifier = entities.NewSuspended.SystemGeneratedIdentifier;
    var createdTimestamp = local.Current.Timestamp;
    var reasonCodeId = "MANUALDIST";
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
        db.SetNullableString(command, "reasonCodeId", reasonCodeId);
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
    entities.ActiveCashReceiptDetailStatHistory.ReasonCodeId = reasonCodeId;
    entities.ActiveCashReceiptDetailStatHistory.CreatedBy = createdBy;
    entities.ActiveCashReceiptDetailStatHistory.DiscontinueDate =
      discontinueDate;
    entities.ActiveCashReceiptDetailStatHistory.Populated = true;
  }

  private void CreateCashReceiptStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);

    var crtIdentifier = entities.CashReceipt.CrtIdentifier;
    var cstIdentifier = entities.CashReceipt.CstIdentifier;
    var crvIdentifier = entities.CashReceipt.CrvIdentifier;
    var crsIdentifier =
      entities.ActiveCashReceiptStatus.SystemGeneratedIdentifier;
    var createdTimestamp = local.Current.Timestamp;
    var createdBy = global.UserId;
    var discontinueDate = local.Max.Date;

    entities.ActiveCashReceiptStatusHistory.Populated = false;
    Update("CreateCashReceiptStatusHistory",
      (db, command) =>
      {
        db.SetInt32(command, "crtIdentifier", crtIdentifier);
        db.SetInt32(command, "cstIdentifier", cstIdentifier);
        db.SetInt32(command, "crvIdentifier", crvIdentifier);
        db.SetInt32(command, "crsIdentifier", crsIdentifier);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "createdBy", createdBy);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "reasonText", "");
      });

    entities.ActiveCashReceiptStatusHistory.CrtIdentifier = crtIdentifier;
    entities.ActiveCashReceiptStatusHistory.CstIdentifier = cstIdentifier;
    entities.ActiveCashReceiptStatusHistory.CrvIdentifier = crvIdentifier;
    entities.ActiveCashReceiptStatusHistory.CrsIdentifier = crsIdentifier;
    entities.ActiveCashReceiptStatusHistory.CreatedTimestamp = createdTimestamp;
    entities.ActiveCashReceiptStatusHistory.CreatedBy = createdBy;
    entities.ActiveCashReceiptStatusHistory.DiscontinueDate = discontinueDate;
    entities.ActiveCashReceiptStatusHistory.Populated = true;
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
        entities.CashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.Populated = true;
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.CashReceiptDetail.CollectionAmtFullyAppliedInd);
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
        entities.ActiveCashReceiptDetailStatHistory.ReasonCodeId =
          db.GetNullableString(reader, 6);
        entities.ActiveCashReceiptDetailStatHistory.CreatedBy =
          db.GetString(reader, 7);
        entities.ActiveCashReceiptDetailStatHistory.DiscontinueDate =
          db.GetNullableDate(reader, 8);
        entities.ActiveCashReceiptDetailStatHistory.Populated = true;
        entities.ActiveCashReceiptDetailStatus.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailStatus()
  {
    entities.NewSuspended.Populated = false;

    return Read("ReadCashReceiptDetailStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdetailStatId",
          local.HardcodedCrdsSuspended.SystemGeneratedIdentifier);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.NewSuspended.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.NewSuspended.Code = db.GetString(reader, 1);
        entities.NewSuspended.EffectiveDate = db.GetDate(reader, 2);
        entities.NewSuspended.DiscontinueDate = db.GetNullableDate(reader, 3);
        entities.NewSuspended.Populated = true;
      });
  }

  private bool ReadCashReceiptStatus()
  {
    entities.ActiveCashReceiptStatus.Populated = false;

    return Read("ReadCashReceiptStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "crStatusId",
          local.HardcodedCrsReceipted.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ActiveCashReceiptStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ActiveCashReceiptStatus.Populated = true;
      });
  }

  private bool ReadCashReceiptStatusHistoryCashReceiptStatus()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.ActiveCashReceiptStatusHistory.Populated = false;
    entities.ActiveCashReceiptStatus.Populated = false;

    return Read("ReadCashReceiptStatusHistoryCashReceiptStatus",
      (db, command) =>
      {
        db.
          SetInt32(command, "crtIdentifier", entities.CashReceipt.CrtIdentifier);
          
        db.
          SetInt32(command, "cstIdentifier", entities.CashReceipt.CstIdentifier);
          
        db.
          SetInt32(command, "crvIdentifier", entities.CashReceipt.CrvIdentifier);
          
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ActiveCashReceiptStatusHistory.CrtIdentifier =
          db.GetInt32(reader, 0);
        entities.ActiveCashReceiptStatusHistory.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.ActiveCashReceiptStatusHistory.CrvIdentifier =
          db.GetInt32(reader, 2);
        entities.ActiveCashReceiptStatusHistory.CrsIdentifier =
          db.GetInt32(reader, 3);
        entities.ActiveCashReceiptStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ActiveCashReceiptStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.ActiveCashReceiptStatusHistory.CreatedBy =
          db.GetString(reader, 5);
        entities.ActiveCashReceiptStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.ActiveCashReceiptStatusHistory.Populated = true;
        entities.ActiveCashReceiptStatus.Populated = true;
      });
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
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
    }

    /// <summary>
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public CashReceiptDetailStatus New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    private CashReceiptDetailStatus new1;
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
    /// A value of HardcodedCrsReceipted.
    /// </summary>
    [JsonPropertyName("hardcodedCrsReceipted")]
    public CashReceiptStatus HardcodedCrsReceipted
    {
      get => hardcodedCrsReceipted ??= new();
      set => hardcodedCrsReceipted = value;
    }

    /// <summary>
    /// A value of HardcodedCrdsRecorded.
    /// </summary>
    [JsonPropertyName("hardcodedCrdsRecorded")]
    public CashReceiptDetailStatus HardcodedCrdsRecorded
    {
      get => hardcodedCrdsRecorded ??= new();
      set => hardcodedCrdsRecorded = value;
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

    private DateWorkArea current;
    private DateWorkArea max;
    private CashReceiptStatus hardcodedCrsReceipted;
    private CashReceiptDetailStatus hardcodedCrdsRecorded;
    private CashReceiptDetailStatus hardcodedCrdsSuspended;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
    /// A value of ActiveCashReceiptStatusHistory.
    /// </summary>
    [JsonPropertyName("activeCashReceiptStatusHistory")]
    public CashReceiptStatusHistory ActiveCashReceiptStatusHistory
    {
      get => activeCashReceiptStatusHistory ??= new();
      set => activeCashReceiptStatusHistory = value;
    }

    /// <summary>
    /// A value of ActiveCashReceiptStatus.
    /// </summary>
    [JsonPropertyName("activeCashReceiptStatus")]
    public CashReceiptStatus ActiveCashReceiptStatus
    {
      get => activeCashReceiptStatus ??= new();
      set => activeCashReceiptStatus = value;
    }

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
    /// A value of NewSuspended.
    /// </summary>
    [JsonPropertyName("newSuspended")]
    public CashReceiptDetailStatus NewSuspended
    {
      get => newSuspended ??= new();
      set => newSuspended = value;
    }

    private CashReceipt cashReceipt;
    private CashReceiptStatusHistory activeCashReceiptStatusHistory;
    private CashReceiptStatus activeCashReceiptStatus;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptDetailStatHistory activeCashReceiptDetailStatHistory;
    private CashReceiptDetailStatus activeCashReceiptDetailStatus;
    private CashReceiptDetailStatus newSuspended;
  }
#endregion
}
