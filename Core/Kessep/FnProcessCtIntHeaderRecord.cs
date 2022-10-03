// Program: FN_PROCESS_CT_INT_HEADER_RECORD, ID: 372565722, model: 746.
// Short name: SWE00521
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_PROCESS_CT_INT_HEADER_RECORD.
/// </para>
/// <para>
/// RESP:  FINANCE	
/// This action block will process the header record (type = 1) from the court 
/// interface.
/// It will create the cash receive event and prepare for processing of detail 
/// records.
/// </para>
/// </summary>
[Serializable]
public partial class FnProcessCtIntHeaderRecord: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_PROCESS_CT_INT_HEADER_RECORD program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnProcessCtIntHeaderRecord(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnProcessCtIntHeaderRecord.
  /// </summary>
  public FnProcessCtIntHeaderRecord(IContext context, Import import,
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
    // *****************************************************************
    // RICH GALICHON - 2/99
    // Unit test and fixes.
    // 04/09/99	J Katz		Removed logic which supported
    // 				non cash receipts resulting
    // 				from court disbursed money.
    // 				Added logic to ensure that an
    // 				interface receipt has not already
    // 				been processed for the imported
    // 				source code and source creation
    // 				date.
    // 05/20/99	J Katz		Set up WY court for EFT processing.
    // *****************************************************************
    // ***********************
    // SET THE HARDCODED VIEWS
    // ***********************
    local.UserId.Text8 = global.UserId;
    local.Max.Date = UseCabSetMaximumDiscontinueDate();
    UseFnHardcodedCashReceipting();

    // ################################################
    // RAJG - 3/99
    // Add a CASE for any COURT that wishes to send payments
    // via EFT.   The <EFT HERE> CASE is just an example.
    // ################################################
    if (Equal(import.HeaderRecord.HeaderCashReceiptSourceType.Code, "<EFT HERE>"))
      
    {
      export.CashReceiptType.SystemGeneratedIdentifier =
        local.HardcodedViews.HardcodedEft.SystemGeneratedIdentifier;
    }
    else
    {
      // *****************************************************************
      // For all other courts, the default value is CHECK.
      // This value was set in the fn_hardcoded_cash_receipting
      // action block.
      // *****************************************************************
      export.CashReceiptType.SystemGeneratedIdentifier =
        local.HardcodedViews.HardcodedCheck.SystemGeneratedIdentifier;
    }

    // *****************************************************************
    // Read and establish currency on the Cash Receipt Source Type.
    // *****************************************************************
    if (ReadCashReceiptSourceType())
    {
      ++export.ImportNumberOfReads.Count;
      MoveCashReceiptSourceType(entities.CashReceiptSourceType,
        export.CashReceiptSourceType);

      // *****************************************************************
      // Edit to make sure this interface file has not already been
      // loaded.    JLK  04/09/99
      // *****************************************************************
      foreach(var item in ReadCashReceiptEvent())
      {
        ++export.ImportNumberOfReads.Count;

        // ***********************************************************
        // Determine if interface receipt has been deleted.
        // JLK  09/11/99
        // ***********************************************************
        if (ReadCashReceiptStatus())
        {
          ++export.ImportNumberOfReads.Count;

          if (entities.ExistingCashReceiptStatus.SystemGeneratedIdentifier == local
            .HardcodedViews.HardcodedCrsDeleted.SystemGeneratedIdentifier)
          {
            // --->  Interface receipt does not exist.  OK to continue.
            continue;
          }
          else
          {
            ExitState = "INTERFACE_ALREADY_PROCESSED_RB";

            return;
          }
        }
        else
        {
          ExitState = "FN0103_CASH_RCPT_STAT_HIST_NF_RB";

          return;
        }
      }

      // *****************************
      // CREATE THE CASH RECEIPT EVENT
      // *****************************
      export.CashReceiptEvent.SourceCreationDate =
        import.HeaderRecord.HeaderCashReceiptEvent.SourceCreationDate;
      export.CashReceiptEvent.ReceivedDate =
        import.HeaderRecord.HeaderCashReceiptEvent.SourceCreationDate;
      UseFnCreateCashRcptEvent();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }

      // *****************************************************************
      // Create the Cash Receipt for this Cash Receipt  Event.
      // This will allow each of the interface record CABs to
      // associate its detail records to a Cash Receipt without
      // having to validate that the records have been created first.
      // If all of the detail and adjustment records error out and there
      // are no Cash Receipt Details associated to these Cash
      // Receipts at the end of processing, the Cash Receipt will be
      // deleted.
      // *****************************************************************
      local.CashReceipt.ReceivedDate =
        import.HeaderRecord.HeaderCashReceiptEvent.SourceCreationDate;
      local.CashReceipt.ReceiptDate = Now().Date;
      local.CashReceipt.CheckDate = new DateTime(1, 1, 1);
      local.CashReceipt.CheckType = "CSE";
      UseFnCreateCashReceipt();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
      }
    }
    else
    {
      ExitState = "FN0000_COURT_CR_SOURCE_TYPE_NF";
    }
  }

  private static void MoveCashReceipt(CashReceipt source, CashReceipt target)
  {
    target.ReceiptDate = source.ReceiptDate;
    target.CheckType = source.CheckType;
    target.CheckDate = source.CheckDate;
    target.ReceivedDate = source.ReceivedDate;
  }

  private static void MoveCashReceiptEvent(CashReceiptEvent source,
    CashReceiptEvent target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.ReceivedDate = source.ReceivedDate;
    target.SourceCreationDate = source.SourceCreationDate;
  }

  private static void MoveCashReceiptSourceType(CashReceiptSourceType source,
    CashReceiptSourceType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseFnCreateCashRcptEvent()
  {
    var useImport = new FnCreateCashRcptEvent.Import();
    var useExport = new FnCreateCashRcptEvent.Export();

    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      export.CashReceiptSourceType.SystemGeneratedIdentifier;
    MoveCashReceiptEvent(export.CashReceiptEvent, useImport.CashReceiptEvent);
    useExport.ImportNumberOfReads.Count = export.ImportNumberOfReads.Count;
    useExport.ImportNumberOfUpdates.Count = export.ImportNumberOfUpdates.Count;

    Call(FnCreateCashRcptEvent.Execute, useImport, useExport);

    MoveCashReceiptEvent(useExport.CashReceiptEvent, export.CashReceiptEvent);
    export.ImportNumberOfReads.Count = useExport.ImportNumberOfReads.Count;
    export.ImportNumberOfUpdates.Count = useExport.ImportNumberOfUpdates.Count;
  }

  private void UseFnCreateCashReceipt()
  {
    var useImport = new FnCreateCashReceipt.Import();
    var useExport = new FnCreateCashReceipt.Export();

    MoveCashReceipt(local.CashReceipt, useImport.CashReceipt);
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      export.CashReceiptSourceType.SystemGeneratedIdentifier;
    MoveCashReceiptEvent(export.CashReceiptEvent, useImport.CashReceiptEvent);
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      export.CashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptStatus.SystemGeneratedIdentifier =
      local.HardcodedViews.HardcodedInterface.SystemGeneratedIdentifier;
    useExport.ImportNumberOfReads.Count = export.ImportNumberOfReads.Count;
    useExport.ImportNumberOfUpdates.Count = export.ImportNumberOfUpdates.Count;

    Call(FnCreateCashReceipt.Execute, useImport, useExport);

    export.Check.SequentialNumber = useExport.CashReceipt.SequentialNumber;
    export.ImportNumberOfReads.Count = useExport.ImportNumberOfReads.Count;
    export.ImportNumberOfUpdates.Count = useExport.ImportNumberOfUpdates.Count;
  }

  private void UseFnHardcodedCashReceipting()
  {
    var useImport = new FnHardcodedCashReceipting.Import();
    var useExport = new FnHardcodedCashReceipting.Export();

    Call(FnHardcodedCashReceipting.Execute, useImport, useExport);

    local.HardcodedViews.HardcodedCheck.SystemGeneratedIdentifier =
      useExport.CrtSystemId.CrtIdCheck.SystemGeneratedIdentifier;
    local.HardcodedViews.HardcodedInterface.SystemGeneratedIdentifier =
      useExport.CrsSystemId.CrsIdInterface.SystemGeneratedIdentifier;
    local.HardcodedViews.HardcodedEft.SystemGeneratedIdentifier =
      useExport.CrtSystemId.CrtIdEft.SystemGeneratedIdentifier;
    local.HardcodedViews.HardcodedCrsDeleted.SystemGeneratedIdentifier =
      useExport.CrsSystemId.CrsIdDeleted.SystemGeneratedIdentifier;
  }

  private IEnumerable<bool> ReadCashReceiptEvent()
  {
    entities.CashReceiptEvent.Populated = false;

    return ReadEach("ReadCashReceiptEvent",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "sourceCreationDt",
          import.HeaderRecord.HeaderCashReceiptEvent.SourceCreationDate.
            GetValueOrDefault());
        db.SetInt32(
          command, "cstIdentifier",
          entities.CashReceiptSourceType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptEvent.SourceCreationDate =
          db.GetNullableDate(reader, 2);
        entities.CashReceiptEvent.CreatedBy = db.GetString(reader, 3);
        entities.CashReceiptEvent.Populated = true;

        return true;
      });
  }

  private bool ReadCashReceiptSourceType()
  {
    entities.CashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptSourceType",
      (db, command) =>
      {
        db.SetString(
          command, "code",
          import.HeaderRecord.HeaderCashReceiptSourceType.Code);
        db.SetDate(
          command, "effectiveDate",
          import.HeaderRecord.HeaderCashReceiptEvent.SourceCreationDate.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 1);
        entities.CashReceiptSourceType.EffectiveDate = db.GetDate(reader, 2);
        entities.CashReceiptSourceType.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.CashReceiptSourceType.Populated = true;
      });
  }

  private bool ReadCashReceiptStatus()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptEvent.Populated);
    entities.ExistingCashReceiptStatus.Populated = false;

    return Read("ReadCashReceiptStatus",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
        db.SetInt32(
          command, "crvIdentifier",
          entities.CashReceiptEvent.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptEvent.CstIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingCashReceiptStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptStatus.Populated = true;
      });
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
    /// <summary>A HeaderRecordGroup group.</summary>
    [Serializable]
    public class HeaderRecordGroup
    {
      /// <summary>
      /// A value of HeaderCashReceiptSourceType.
      /// </summary>
      [JsonPropertyName("headerCashReceiptSourceType")]
      public CashReceiptSourceType HeaderCashReceiptSourceType
      {
        get => headerCashReceiptSourceType ??= new();
        set => headerCashReceiptSourceType = value;
      }

      /// <summary>
      /// A value of HeaderCashReceiptEvent.
      /// </summary>
      [JsonPropertyName("headerCashReceiptEvent")]
      public CashReceiptEvent HeaderCashReceiptEvent
      {
        get => headerCashReceiptEvent ??= new();
        set => headerCashReceiptEvent = value;
      }

      private CashReceiptSourceType headerCashReceiptSourceType;
      private CashReceiptEvent headerCashReceiptEvent;
    }

    /// <summary>
    /// Gets a value of HeaderRecord.
    /// </summary>
    [JsonPropertyName("headerRecord")]
    public HeaderRecordGroup HeaderRecord
    {
      get => headerRecord ?? (headerRecord = new());
      set => headerRecord = value;
    }

    private HeaderRecordGroup headerRecord;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of CashReceiptEvent.
    /// </summary>
    [JsonPropertyName("cashReceiptEvent")]
    public CashReceiptEvent CashReceiptEvent
    {
      get => cashReceiptEvent ??= new();
      set => cashReceiptEvent = value;
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
    /// A value of Check.
    /// </summary>
    [JsonPropertyName("check")]
    public CashReceipt Check
    {
      get => check ??= new();
      set => check = value;
    }

    /// <summary>
    /// A value of ImportNumberOfReads.
    /// </summary>
    [JsonPropertyName("importNumberOfReads")]
    public Common ImportNumberOfReads
    {
      get => importNumberOfReads ??= new();
      set => importNumberOfReads = value;
    }

    /// <summary>
    /// A value of ImportNumberOfUpdates.
    /// </summary>
    [JsonPropertyName("importNumberOfUpdates")]
    public Common ImportNumberOfUpdates
    {
      get => importNumberOfUpdates ??= new();
      set => importNumberOfUpdates = value;
    }

    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptType cashReceiptType;
    private CashReceipt check;
    private Common importNumberOfReads;
    private Common importNumberOfUpdates;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A HardcodedViewsGroup group.</summary>
    [Serializable]
    public class HardcodedViewsGroup
    {
      /// <summary>
      /// A value of HardcodedCheck.
      /// </summary>
      [JsonPropertyName("hardcodedCheck")]
      public CashReceiptType HardcodedCheck
      {
        get => hardcodedCheck ??= new();
        set => hardcodedCheck = value;
      }

      /// <summary>
      /// A value of HardcodedEft.
      /// </summary>
      [JsonPropertyName("hardcodedEft")]
      public CashReceiptType HardcodedEft
      {
        get => hardcodedEft ??= new();
        set => hardcodedEft = value;
      }

      /// <summary>
      /// A value of HardcodedInterface.
      /// </summary>
      [JsonPropertyName("hardcodedInterface")]
      public CashReceiptStatus HardcodedInterface
      {
        get => hardcodedInterface ??= new();
        set => hardcodedInterface = value;
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

      private CashReceiptType hardcodedCheck;
      private CashReceiptType hardcodedEft;
      private CashReceiptStatus hardcodedInterface;
      private CashReceiptStatus hardcodedCrsDeleted;
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
    /// A value of UserId.
    /// </summary>
    [JsonPropertyName("userId")]
    public TextWorkArea UserId
    {
      get => userId ??= new();
      set => userId = value;
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
    /// Gets a value of HardcodedViews.
    /// </summary>
    [JsonPropertyName("hardcodedViews")]
    public HardcodedViewsGroup HardcodedViews
    {
      get => hardcodedViews ?? (hardcodedViews = new());
      set => hardcodedViews = value;
    }

    private DateWorkArea max;
    private TextWorkArea userId;
    private CashReceipt cashReceipt;
    private HardcodedViewsGroup hardcodedViews;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CashReceiptEvent.
    /// </summary>
    [JsonPropertyName("cashReceiptEvent")]
    public CashReceiptEvent CashReceiptEvent
    {
      get => cashReceiptEvent ??= new();
      set => cashReceiptEvent = value;
    }

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
    /// A value of ExistingCashReceiptStatusHistory.
    /// </summary>
    [JsonPropertyName("existingCashReceiptStatusHistory")]
    public CashReceiptStatusHistory ExistingCashReceiptStatusHistory
    {
      get => existingCashReceiptStatusHistory ??= new();
      set => existingCashReceiptStatusHistory = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptStatus.
    /// </summary>
    [JsonPropertyName("existingCashReceiptStatus")]
    public CashReceiptStatus ExistingCashReceiptStatus
    {
      get => existingCashReceiptStatus ??= new();
      set => existingCashReceiptStatus = value;
    }

    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceipt existingCashReceipt;
    private CashReceiptStatusHistory existingCashReceiptStatusHistory;
    private CashReceiptStatus existingCashReceiptStatus;
  }
#endregion
}
