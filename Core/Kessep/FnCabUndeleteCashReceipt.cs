// Program: FN_CAB_UNDELETE_CASH_RECEIPT, ID: 371721491, model: 746.
// Short name: SWE02263
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_CAB_UNDELETE_CASH_RECEIPT.
/// </summary>
[Serializable]
public partial class FnCabUndeleteCashReceipt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CAB_UNDELETE_CASH_RECEIPT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCabUndeleteCashReceipt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCabUndeleteCashReceipt.
  /// </summary>
  public FnCabUndeleteCashReceipt(IContext context, Import import, Export export)
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
    // ------------------------------------------------------------------------
    // 06/07/99  J. Katz - SRG		Analyzed READ statements and changed
    // 				read property to Select Only where
    // 				appropriate.
    // ------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;
    local.Current.Timestamp = Now();
    local.HardcodedCrtManint.SystemGeneratedIdentifier = 12;
    UseCabSetMaximumDiscontinueDate();
    UseFnHardcodedCashReceipting();

    // ------------------------------------------------------------
    // Read Cash Receipt to be undeleted.
    // ------------------------------------------------------------
    if (!ReadCashReceiptCashReceiptType())
    {
      ExitState = "FN0084_CASH_RCPT_NF";

      return;
    }

    // ------------------------------------------------------------
    // Determine active status of Cash Receipt.
    // ------------------------------------------------------------
    if (ReadCashReceiptStatusCashReceiptStatusHistory())
    {
      // ------------------------------------------------------------
      // Active status for the Cash Receipt must be DEL to perform
      // undelete action.
      // ------------------------------------------------------------
      if (entities.ActiveCashReceiptStatus.SystemGeneratedIdentifier != local
        .HardcodedCrsDeleted.SystemGeneratedIdentifier)
      {
        ExitState = "FN0000_UNDELETE_NOT_ALLOWED";

        return;
      }

      // ------------------------------------------------------------
      // Cash Receipt can only be undeleted on the same day that it
      // was deleted.
      // ------------------------------------------------------------
      if (!Equal(Date(entities.ActiveCashReceiptStatusHistory.CreatedTimestamp),
        local.Current.Date))
      {
        ExitState = "FN0000_UNDELETE_NOT_ALLOWED";

        return;
      }
    }
    else
    {
      ExitState = "FN0102_CASH_RCPT_STAT_HIST_NF";

      return;
    }

    // ------------------------------------------------------------
    // Determine status of Cash Receipt prior to it being deleted.
    // ------------------------------------------------------------
    local.PriorStatus.Flag = "N";

    if (ReadCashReceiptStatusHistoryCashReceiptStatus())
    {
      local.PriorStatus.Flag = "Y";
    }

    // ------------------------------------------------------------
    // Prior status of Cash Receipt must be either Receipted or
    // Interfaced.
    // ------------------------------------------------------------
    if (AsChar(local.PriorStatus.Flag) == 'N')
    {
      ExitState = "FN0000_PRIOR_STATUS_NF";

      return;
    }

    if (entities.PriorCashReceiptStatus.SystemGeneratedIdentifier == local
      .HardcodedCrsReceipted.SystemGeneratedIdentifier || entities
      .PriorCashReceiptStatus.SystemGeneratedIdentifier == local
      .HardcodedCrsInterfaced.SystemGeneratedIdentifier)
    {
      // ------------------------------------------------------------
      // Perform undelete action.
      // ------------------------------------------------------------
      try
      {
        UpdateCashReceiptStatusHistory();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "CASH_RCPT_STATUS_HISTORY_NU_W_RB";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0104_CASH_RCPT_STAT_HIST_PV";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
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

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0104_CASH_RCPT_STAT_HIST_PV";

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
      ExitState = "FN0000_UNDELETE_NOT_ALLOWED";
    }
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.Max.Date = useExport.DateWorkArea.Date;
  }

  private void UseFnHardcodedCashReceipting()
  {
    var useImport = new FnHardcodedCashReceipting.Import();
    var useExport = new FnHardcodedCashReceipting.Export();

    Call(FnHardcodedCashReceipting.Execute, useImport, useExport);

    local.HardcodedCrsDeleted.SystemGeneratedIdentifier =
      useExport.CrsSystemId.CrsIdDeleted.SystemGeneratedIdentifier;
    local.HardcodedCrsInterfaced.SystemGeneratedIdentifier =
      useExport.CrsSystemId.CrsIdInterface.SystemGeneratedIdentifier;
    local.HardcodedCrsReceipted.SystemGeneratedIdentifier =
      useExport.CrsSystemId.CrsIdRecorded.SystemGeneratedIdentifier;
  }

  private void CreateCashReceiptStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCashReceipt.Populated);

    var crtIdentifier = entities.ExistingCashReceipt.CrtIdentifier;
    var cstIdentifier = entities.ExistingCashReceipt.CstIdentifier;
    var crvIdentifier = entities.ExistingCashReceipt.CrvIdentifier;
    var crsIdentifier =
      entities.PriorCashReceiptStatus.SystemGeneratedIdentifier;
    var createdTimestamp = local.Current.Timestamp;
    var createdBy = global.UserId;
    var discontinueDate = local.Max.Date;

    entities.PriorCashReceiptStatusHistory.Populated = false;
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

    entities.PriorCashReceiptStatusHistory.CrtIdentifier = crtIdentifier;
    entities.PriorCashReceiptStatusHistory.CstIdentifier = cstIdentifier;
    entities.PriorCashReceiptStatusHistory.CrvIdentifier = crvIdentifier;
    entities.PriorCashReceiptStatusHistory.CrsIdentifier = crsIdentifier;
    entities.PriorCashReceiptStatusHistory.CreatedTimestamp = createdTimestamp;
    entities.PriorCashReceiptStatusHistory.CreatedBy = createdBy;
    entities.PriorCashReceiptStatusHistory.DiscontinueDate = discontinueDate;
    entities.PriorCashReceiptStatusHistory.Populated = true;
  }

  private bool ReadCashReceiptCashReceiptType()
  {
    entities.ExistingCashReceipt.Populated = false;
    entities.ExistingCashReceiptType.Populated = false;

    return Read("ReadCashReceiptCashReceiptType",
      (db, command) =>
      {
        db.SetInt32(
          command, "cashReceiptId", import.CashReceipt.SequentialNumber);
      },
      (db, reader) =>
      {
        entities.ExistingCashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.ExistingCashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.ExistingCashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.ExistingCashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingCashReceipt.SequentialNumber = db.GetInt32(reader, 3);
        entities.ExistingCashReceipt.Populated = true;
        entities.ExistingCashReceiptType.Populated = true;
      });
  }

  private bool ReadCashReceiptStatusCashReceiptStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCashReceipt.Populated);
    entities.ActiveCashReceiptStatusHistory.Populated = false;
    entities.ActiveCashReceiptStatus.Populated = false;

    return Read("ReadCashReceiptStatusCashReceiptStatusHistory",
      (db, command) =>
      {
        db.SetInt32(
          command, "crtIdentifier", entities.ExistingCashReceipt.CrtIdentifier);
          
        db.SetInt32(
          command, "cstIdentifier", entities.ExistingCashReceipt.CstIdentifier);
          
        db.SetInt32(
          command, "crvIdentifier", entities.ExistingCashReceipt.CrvIdentifier);
          
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ActiveCashReceiptStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ActiveCashReceiptStatusHistory.CrsIdentifier =
          db.GetInt32(reader, 0);
        entities.ActiveCashReceiptStatusHistory.CrtIdentifier =
          db.GetInt32(reader, 1);
        entities.ActiveCashReceiptStatusHistory.CstIdentifier =
          db.GetInt32(reader, 2);
        entities.ActiveCashReceiptStatusHistory.CrvIdentifier =
          db.GetInt32(reader, 3);
        entities.ActiveCashReceiptStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.ActiveCashReceiptStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.ActiveCashReceiptStatusHistory.Populated = true;
        entities.ActiveCashReceiptStatus.Populated = true;
      });
  }

  private bool ReadCashReceiptStatusHistoryCashReceiptStatus()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCashReceipt.Populated);
    entities.PriorCashReceiptStatus.Populated = false;
    entities.PriorCashReceiptStatusHistory.Populated = false;

    return Read("ReadCashReceiptStatusHistoryCashReceiptStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "crtIdentifier", entities.ExistingCashReceipt.CrtIdentifier);
          
        db.SetInt32(
          command, "cstIdentifier", entities.ExistingCashReceipt.CstIdentifier);
          
        db.SetInt32(
          command, "crvIdentifier", entities.ExistingCashReceipt.CrvIdentifier);
          
        db.SetNullableDate(
          command, "discontinueDate1", local.Max.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate2", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PriorCashReceiptStatusHistory.CrtIdentifier =
          db.GetInt32(reader, 0);
        entities.PriorCashReceiptStatusHistory.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.PriorCashReceiptStatusHistory.CrvIdentifier =
          db.GetInt32(reader, 2);
        entities.PriorCashReceiptStatusHistory.CrsIdentifier =
          db.GetInt32(reader, 3);
        entities.PriorCashReceiptStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.PriorCashReceiptStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.PriorCashReceiptStatusHistory.CreatedBy =
          db.GetString(reader, 5);
        entities.PriorCashReceiptStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.PriorCashReceiptStatus.EffectiveDate = db.GetDate(reader, 7);
        entities.PriorCashReceiptStatus.DiscontinueDate =
          db.GetNullableDate(reader, 8);
        entities.PriorCashReceiptStatus.Populated = true;
        entities.PriorCashReceiptStatusHistory.Populated = true;
      });
  }

  private void UpdateCashReceiptStatusHistory()
  {
    System.Diagnostics.Debug.Assert(
      entities.ActiveCashReceiptStatusHistory.Populated);

    var discontinueDate = local.Current.Date;

    entities.ActiveCashReceiptStatusHistory.Populated = false;
    Update("UpdateCashReceiptStatusHistory",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetInt32(
          command, "crtIdentifier",
          entities.ActiveCashReceiptStatusHistory.CrtIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          entities.ActiveCashReceiptStatusHistory.CstIdentifier);
        db.SetInt32(
          command, "crvIdentifier",
          entities.ActiveCashReceiptStatusHistory.CrvIdentifier);
        db.SetInt32(
          command, "crsIdentifier",
          entities.ActiveCashReceiptStatusHistory.CrsIdentifier);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.ActiveCashReceiptStatusHistory.CreatedTimestamp.
            GetValueOrDefault());
      });

    entities.ActiveCashReceiptStatusHistory.DiscontinueDate = discontinueDate;
    entities.ActiveCashReceiptStatusHistory.Populated = true;
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
    /// A value of Clear.
    /// </summary>
    [JsonPropertyName("clear")]
    public DateWorkArea Clear
    {
      get => clear ??= new();
      set => clear = value;
    }

    /// <summary>
    /// A value of PriorStatus.
    /// </summary>
    [JsonPropertyName("priorStatus")]
    public Common PriorStatus
    {
      get => priorStatus ??= new();
      set => priorStatus = value;
    }

    /// <summary>
    /// A value of HardcodedCrsDeleted.
    /// </summary>
    [JsonPropertyName("hardcodedCrsDeleted")]
    public CashReceiptStatus HardcodedCrsDeleted
    {
      get => hardcodedCrsDeleted ??= new();
      set => hardcodedCrsDeleted = value;
    }

    /// <summary>
    /// A value of HardcodedCrsInterfaced.
    /// </summary>
    [JsonPropertyName("hardcodedCrsInterfaced")]
    public CashReceiptStatus HardcodedCrsInterfaced
    {
      get => hardcodedCrsInterfaced ??= new();
      set => hardcodedCrsInterfaced = value;
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
    /// A value of HardcodedCrtManint.
    /// </summary>
    [JsonPropertyName("hardcodedCrtManint")]
    public CashReceiptStatus HardcodedCrtManint
    {
      get => hardcodedCrtManint ??= new();
      set => hardcodedCrtManint = value;
    }

    private DateWorkArea current;
    private DateWorkArea max;
    private DateWorkArea clear;
    private Common priorStatus;
    private CashReceiptStatus hardcodedCrsDeleted;
    private CashReceiptStatus hardcodedCrsInterfaced;
    private CashReceiptStatus hardcodedCrsReceipted;
    private CashReceiptStatus hardcodedCrtManint;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingCashReceipt.
    /// </summary>
    [JsonPropertyName("existingCashReceipt")]
    public CashReceipt ExistingCashReceipt
    {
      get => existingCashReceipt ??= new();
      set => existingCashReceipt = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptType.
    /// </summary>
    [JsonPropertyName("existingCashReceiptType")]
    public CashReceiptType ExistingCashReceiptType
    {
      get => existingCashReceiptType ??= new();
      set => existingCashReceiptType = value;
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
    /// A value of PriorCashReceiptStatus.
    /// </summary>
    [JsonPropertyName("priorCashReceiptStatus")]
    public CashReceiptStatus PriorCashReceiptStatus
    {
      get => priorCashReceiptStatus ??= new();
      set => priorCashReceiptStatus = value;
    }

    /// <summary>
    /// A value of PriorCashReceiptStatusHistory.
    /// </summary>
    [JsonPropertyName("priorCashReceiptStatusHistory")]
    public CashReceiptStatusHistory PriorCashReceiptStatusHistory
    {
      get => priorCashReceiptStatusHistory ??= new();
      set => priorCashReceiptStatusHistory = value;
    }

    private CashReceipt existingCashReceipt;
    private CashReceiptType existingCashReceiptType;
    private CashReceiptStatusHistory activeCashReceiptStatusHistory;
    private CashReceiptStatus activeCashReceiptStatus;
    private CashReceiptStatus priorCashReceiptStatus;
    private CashReceiptStatusHistory priorCashReceiptStatusHistory;
  }
#endregion
}
