// Program: FN_ADD_FDSO_SDSO_HEADER_INFO, ID: 372428126, model: 746.
// Short name: SWE01523
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_ADD_FDSO_SDSO_HEADER_INFO.
/// </summary>
[Serializable]
public partial class FnAddFdsoSdsoHeaderInfo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_ADD_FDSO_SDSO_HEADER_INFO program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnAddFdsoSdsoHeaderInfo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnAddFdsoSdsoHeaderInfo.
  /// </summary>
  public FnAddFdsoSdsoHeaderInfo(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ************************************************************************************************
    // Date      Developers Name         Request #	Description
    // --------  ----------------------  ---------  
    // ---------------------------------------------------
    // 06/03/96  Holly Kennedy - MTW			Initial
    // 04/28/97  SHERAZ MALIK				CHANGE CURRENT DATE
    // 01/28/99  SWSRPDP				Added check of Created_By to avoid
    // 						Already_Exist if ANOTHER program created
    // 						a Cash_Receipt_Event Today.
    // 02/12/13  GVandy		   CQ37941	Modify edit to allow only one UI Cash 
    // Receipt
    // 						per day from the UI (SWELB578) batch.
    // ************************************************************************************************
    // *****
    // Set the hardcoded views.
    // *****
    local.Current.Date = Now().Date;
    UseFnHardcodedCashReceipting();
    local.HardcodedViews.HardcodedInterfund.SystemGeneratedIdentifier = 10;
    local.HardcodedViews.HardcodedEft.SystemGeneratedIdentifier = 6;

    // *****
    // Read and establish currency on the Cash Receipt Source Type.
    // *****
    if (ReadCashReceiptSourceType())
    {
      ++export.ImportNumberOfReads.Count;
      MoveCashReceiptSourceType(entities.CashReceiptSourceType,
        export.CashReceiptSourceType);
    }
    else
    {
      ExitState = "FN0000_COURT_CR_SOURCE_TYPE_NF";

      return;
    }

    // *****
    // Edit to make sure this interface file has not already been loaded.
    // *****
    // -- 02/12/2013 GVandy Modify read to allow only one UI cash receipt per 
    // day from SWELB578 (DOL UI Batch).
    if (ReadCashReceiptEvent())
    {
      ++export.ImportNumberOfReads.Count;
      ExitState = "INTERFACE_ALREADY_PROCESSED_RB";

      return;
    }
    else
    {
      // *****  Continue processing.
    }

    // *****  Create the Cash Receipt Event.
    export.CashReceiptEvent.SourceCreationDate =
      import.CashReceiptEvent.SourceCreationDate;
    export.CashReceiptEvent.ReceivedDate =
      import.CashReceiptEvent.SourceCreationDate;
    UseFnCreateCashRcptEvent();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    // *****
    // Create the Cash Receipt for the Event.  This will allow each of the 
    // interface record CABs to associate its detail records to a Cash Receipt
    // without having to validate that the records have been created first.  If
    // all of the detail and adjustment records error out and there are no Cash
    // Receipt Details associated to this Cash Receipt at the end of processing,
    // the Cash Receipt will be deleted.
    // *****
    // *****
    // Create the Cash Receipt.
    // *****
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // This A/B is coded for FDSO = EFT
    // anything else = INTERFUND
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    if (Equal(import.CashReceiptSourceType.Code, "FDSO"))
    {
      local.HardcodedCreate.SystemGeneratedIdentifier =
        local.HardcodedViews.HardcodedEft.SystemGeneratedIdentifier;
      local.CashReceipt.PayorOrganization = "FEDERAL MANAGEMENT SERVICES";
    }
    else
    {
      local.HardcodedCreate.SystemGeneratedIdentifier =
        local.HardcodedViews.HardcodedInterfund.SystemGeneratedIdentifier;
    }

    local.CashReceipt.ReceiptDate = local.Current.Date;
    local.CashReceipt.ReceivedDate = import.CashReceiptEvent.SourceCreationDate;
    local.CashReceipt.CheckType = "CSE";

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // 01/20/1999  SWSRPDP  -- Social Security Number was NOT being initialized 
    // correctly.
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.CashReceipt.PayorSocialSecurityNumber = "";
    UseFnCreateCashReceipt();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
    }
  }

  private static void MoveCashReceipt(CashReceipt source, CashReceipt target)
  {
    target.ReceiptDate = source.ReceiptDate;
    target.CheckType = source.CheckType;
    target.ReceivedDate = source.ReceivedDate;
    target.PayorOrganization = source.PayorOrganization;
    target.PayorSocialSecurityNumber = source.PayorSocialSecurityNumber;
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

  private void UseFnCreateCashRcptEvent()
  {
    var useImport = new FnCreateCashRcptEvent.Import();
    var useExport = new FnCreateCashRcptEvent.Export();

    MoveCashReceiptEvent(export.CashReceiptEvent, useImport.CashReceiptEvent);
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      export.CashReceiptSourceType.SystemGeneratedIdentifier;
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

    useImport.CashReceiptType.SystemGeneratedIdentifier =
      local.HardcodedCreate.SystemGeneratedIdentifier;
    MoveCashReceipt(local.CashReceipt, useImport.CashReceipt);
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      export.CashReceiptSourceType.SystemGeneratedIdentifier;
    MoveCashReceiptEvent(export.CashReceiptEvent, useImport.CashReceiptEvent);
    useImport.CashReceiptStatus.SystemGeneratedIdentifier =
      local.HardcodedViews.HardcodedInterface.SystemGeneratedIdentifier;
    useExport.ImportNumberOfReads.Count = export.ImportNumberOfReads.Count;
    useExport.ImportNumberOfUpdates.Count = export.ImportNumberOfUpdates.Count;

    Call(FnCreateCashReceipt.Execute, useImport, useExport);

    export.ImportNumberOfReads.Count = useExport.ImportNumberOfReads.Count;
    export.ImportNumberOfUpdates.Count = useExport.ImportNumberOfUpdates.Count;
    export.NonCash.SequentialNumber = useExport.CashReceipt.SequentialNumber;
  }

  private void UseFnHardcodedCashReceipting()
  {
    var useImport = new FnHardcodedCashReceipting.Import();
    var useExport = new FnHardcodedCashReceipting.Export();

    Call(FnHardcodedCashReceipting.Execute, useImport, useExport);

    local.HardcodedViews.HardcodedInterface.SystemGeneratedIdentifier =
      useExport.CrsSystemId.CrsIdInterface.SystemGeneratedIdentifier;
  }

  private bool ReadCashReceiptEvent()
  {
    entities.CashReceiptEvent.Populated = false;

    return Read("ReadCashReceiptEvent",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "sourceCreationDt",
          import.CashReceiptEvent.SourceCreationDate.GetValueOrDefault());
        db.SetInt32(
          command, "cstIdentifier",
          entities.CashReceiptSourceType.SystemGeneratedIdentifier);
        db.SetString(command, "userId", global.UserId);
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
      });
  }

  private bool ReadCashReceiptSourceType()
  {
    entities.CashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptSourceType",
      (db, command) =>
      {
        db.SetString(command, "code", import.CashReceiptSourceType.Code);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
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

    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of ImportNumberOfUpdates.
    /// </summary>
    [JsonPropertyName("importNumberOfUpdates")]
    public Common ImportNumberOfUpdates
    {
      get => importNumberOfUpdates ??= new();
      set => importNumberOfUpdates = value;
    }

    /// <summary>
    /// A value of NonCash.
    /// </summary>
    [JsonPropertyName("nonCash")]
    public CashReceipt NonCash
    {
      get => nonCash ??= new();
      set => nonCash = value;
    }

    private Common importNumberOfReads;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private Common importNumberOfUpdates;
    private CashReceipt nonCash;
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
      /// A value of HardcodedEft.
      /// </summary>
      [JsonPropertyName("hardcodedEft")]
      public CashReceiptType HardcodedEft
      {
        get => hardcodedEft ??= new();
        set => hardcodedEft = value;
      }

      /// <summary>
      /// A value of HardcodedInterfund.
      /// </summary>
      [JsonPropertyName("hardcodedInterfund")]
      public CashReceiptType HardcodedInterfund
      {
        get => hardcodedInterfund ??= new();
        set => hardcodedInterfund = value;
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

      private CashReceiptType hardcodedEft;
      private CashReceiptType hardcodedInterfund;
      private CashReceiptStatus hardcodedInterface;
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
    /// Gets a value of HardcodedViews.
    /// </summary>
    [JsonPropertyName("hardcodedViews")]
    public HardcodedViewsGroup HardcodedViews
    {
      get => hardcodedViews ?? (hardcodedViews = new());
      set => hardcodedViews = value;
    }

    /// <summary>
    /// A value of HardcodedCreate.
    /// </summary>
    [JsonPropertyName("hardcodedCreate")]
    public CashReceiptType HardcodedCreate
    {
      get => hardcodedCreate ??= new();
      set => hardcodedCreate = value;
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

    private DateWorkArea current;
    private HardcodedViewsGroup hardcodedViews;
    private CashReceiptType hardcodedCreate;
    private CashReceipt cashReceipt;
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

    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
  }
#endregion
}
