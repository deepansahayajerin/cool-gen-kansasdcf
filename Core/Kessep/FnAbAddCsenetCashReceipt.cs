// Program: FN_AB_ADD_CSENET_CASH_RECEIPT, ID: 372623965, model: 746.
// Short name: SWE01637
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_AB_ADD_CSENET_CASH_RECEIPT.
/// </summary>
[Serializable]
public partial class FnAbAddCsenetCashReceipt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_AB_ADD_CSENET_CASH_RECEIPT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnAbAddCsenetCashReceipt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnAbAddCsenetCashReceipt.
  /// </summary>
  public FnAbAddCsenetCashReceipt(IContext context, Import import, Export export)
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
    // ***************************************************
    // Every initial development and change to that
    // development needs to be documented.
    // ***************************************************
    // *****************************************************************************************
    // Date      Developers Name         Request #  Description
    // --------  ----------------------  ---------
    // 
    // --------------------------------------------
    // 09/12/96  Holly Kennedy - MTW                Initial
    // 04/28/97  Ty Hill - MTW                      Change Current_date
    // *****************************************************************************************
    // *****
    // Set Hardcoded Values.  Set CSENET value within CAB until Hardcode Cash 
    // Receipting can be retrieved in Modify to add that view there.
    // *****
    local.Current.Date = Now().Date;
    local.HardcodedDepositedFundTransactionType.SystemGeneratedIdentifier = 1;
    local.HardcodedCsenet.SystemGeneratedIdentifier = 8;
    UseFnHardcodedCashReceipting();

    if (ReadCashReceiptSourceType())
    {
      MoveCashReceiptSourceType(entities.CashReceiptSourceType, export.Exoprt);
    }
    else
    {
      ExitState = "FN0000_COURT_CR_SOURCE_TYPE_NF";

      return;
    }

    local.CashReceiptEvent.ReceivedDate =
      import.InterstateCollection.DateOfCollection;
    local.CashReceiptEvent.SourceCreationDate = Now().Date;

    // If Cash_Receipt_Event DOES NOT exist -- Create One
    if (ReadCashReceiptEvent())
    {
      export.CashReceiptEvent.SystemGeneratedIdentifier =
        entities.CashReceiptEvent.SystemGeneratedIdentifier;
    }
    else
    {
      UseFnCreateCashRcptEvent();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else if (IsExitState("FN0076_CASH_RCPT_EVENT_AE"))
      {
      }
      else
      {
        return;
      }
    }

    local.CashReceipt.CheckType = "CSE";
    local.CashReceipt.ReceiptDate = Now().Date;
    local.CashReceipt.ReceivedDate =
      import.InterstateCollection.DateOfCollection;
    local.CashReceipt.ReferenceNumber =
      NumberToString(import.InterstateCollection.SystemGeneratedSequenceNum, 12);
      
    UseFnCreateCashReceipt();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    // Read fund_transaction and associate it to Cash_Receipt
    if (ReadCashReceipt())
    {
      ReadFundTransaction();

      if (!entities.FundTransaction.Populated)
      {
        ExitState = "FN0000_FUND_TRANS_NF_RB";

        return;
      }

      AssociateCashReceipt();
    }
    else
    {
      ExitState = "FN0000_CASH_RECEIPT_NF";
    }
  }

  private static void MoveCashReceipt(CashReceipt source, CashReceipt target)
  {
    target.ReceiptAmount = source.ReceiptAmount;
    target.SequentialNumber = source.SequentialNumber;
    target.ReceiptDate = source.ReceiptDate;
    target.CheckType = source.CheckType;
    target.ReceivedDate = source.ReceivedDate;
    target.ReferenceNumber = source.ReferenceNumber;
  }

  private static void MoveCashReceiptEvent(CashReceiptEvent source,
    CashReceiptEvent target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.ReceivedDate = source.ReceivedDate;
    target.SourceCreationDate = source.SourceCreationDate;
    target.TotalCashAmt = source.TotalCashAmt;
    target.TotalCashTransactionCount = source.TotalCashTransactionCount;
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

    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      entities.CashReceiptSourceType.SystemGeneratedIdentifier;
    MoveCashReceiptEvent(local.CashReceiptEvent, useImport.CashReceiptEvent);

    Call(FnCreateCashRcptEvent.Execute, useImport, useExport);

    export.CashReceiptEvent.SystemGeneratedIdentifier =
      useExport.CashReceiptEvent.SystemGeneratedIdentifier;
  }

  private void UseFnCreateCashReceipt()
  {
    var useImport = new FnCreateCashReceipt.Import();
    var useExport = new FnCreateCashReceipt.Export();

    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      entities.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CashReceiptStatus.SystemGeneratedIdentifier =
      local.HardcodedDepositedCashReceiptStatus.SystemGeneratedIdentifier;
    MoveCashReceipt(local.CashReceipt, useImport.CashReceipt);
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      local.HardcodedCsenet.SystemGeneratedIdentifier;
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      export.CashReceiptEvent.SystemGeneratedIdentifier;
    useExport.ImportNumberOfUpdates.Count = export.ImportNumberOfUpdates.Count;
    useExport.ImportNumberOfReads.Count = export.ImportNumberOfReads.Count;

    Call(FnCreateCashReceipt.Execute, useImport, useExport);

    export.ImportNumberOfUpdates.Count = useExport.ImportNumberOfUpdates.Count;
    export.CashReceiptEvent.SystemGeneratedIdentifier =
      useExport.CashReceiptEvent.SystemGeneratedIdentifier;
    export.CashReceipt.SequentialNumber =
      useExport.CashReceipt.SequentialNumber;
    export.ImportNumberOfReads.Count = useExport.ImportNumberOfReads.Count;
  }

  private void UseFnHardcodedCashReceipting()
  {
    var useImport = new FnHardcodedCashReceipting.Import();
    var useExport = new FnHardcodedCashReceipting.Export();

    Call(FnHardcodedCashReceipting.Execute, useImport, useExport);

    local.HardcodedDepositedCashReceiptStatus.SystemGeneratedIdentifier =
      useExport.CrsSystemId.CrsIdDeposited.SystemGeneratedIdentifier;
  }

  private void AssociateCashReceipt()
  {
    System.Diagnostics.Debug.Assert(entities.Associate.Populated);
    System.Diagnostics.Debug.Assert(entities.FundTransaction.Populated);

    var fttIdentifier = entities.FundTransaction.FttIdentifier;
    var pcaCode = entities.FundTransaction.PcaCode;
    var pcaEffectiveDate = entities.FundTransaction.PcaEffectiveDate;
    var funIdentifier = entities.FundTransaction.FunIdentifier;
    var ftrIdentifier = entities.FundTransaction.SystemGeneratedIdentifier;

    entities.Associate.Populated = false;
    Update("AssociateCashReceipt",
      (db, command) =>
      {
        db.SetNullableInt32(command, "fttIdentifier", fttIdentifier);
        db.SetNullableString(command, "pcaCode", pcaCode);
        db.SetNullableDate(command, "pcaEffectiveDate", pcaEffectiveDate);
        db.SetNullableInt32(command, "funIdentifier", funIdentifier);
        db.SetNullableInt32(command, "ftrIdentifier", ftrIdentifier);
        db.SetInt32(command, "crvIdentifier", entities.Associate.CrvIdentifier);
        db.SetInt32(command, "cstIdentifier", entities.Associate.CstIdentifier);
        db.SetInt32(command, "crtIdentifier", entities.Associate.CrtIdentifier);
      });

    entities.Associate.FttIdentifier = fttIdentifier;
    entities.Associate.PcaCode = pcaCode;
    entities.Associate.PcaEffectiveDate = pcaEffectiveDate;
    entities.Associate.FunIdentifier = funIdentifier;
    entities.Associate.FtrIdentifier = ftrIdentifier;
    entities.Associate.Populated = true;
  }

  private bool ReadCashReceipt()
  {
    entities.Associate.Populated = false;

    return Read("ReadCashReceipt",
      (db, command) =>
      {
        db.SetInt32(
          command, "cashReceiptId", export.CashReceipt.SequentialNumber);
      },
      (db, reader) =>
      {
        entities.Associate.CrvIdentifier = db.GetInt32(reader, 0);
        entities.Associate.CstIdentifier = db.GetInt32(reader, 1);
        entities.Associate.CrtIdentifier = db.GetInt32(reader, 2);
        entities.Associate.SequentialNumber = db.GetInt32(reader, 3);
        entities.Associate.FttIdentifier = db.GetNullableInt32(reader, 4);
        entities.Associate.PcaCode = db.GetNullableString(reader, 5);
        entities.Associate.PcaEffectiveDate = db.GetNullableDate(reader, 6);
        entities.Associate.FunIdentifier = db.GetNullableInt32(reader, 7);
        entities.Associate.FtrIdentifier = db.GetNullableInt32(reader, 8);
        entities.Associate.Populated = true;
      });
  }

  private bool ReadCashReceiptEvent()
  {
    entities.CashReceiptEvent.Populated = false;

    return Read("ReadCashReceiptEvent",
      (db, command) =>
      {
        db.SetString(command, "createdBy", global.UserId);
        db.SetDate(
          command, "receivedDate",
          local.CashReceiptEvent.ReceivedDate.GetValueOrDefault());
        db.SetInt32(
          command, "cstIdentifier",
          entities.CashReceiptSourceType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptEvent.ReceivedDate = db.GetDate(reader, 2);
        entities.CashReceiptEvent.SourceCreationDate =
          db.GetNullableDate(reader, 3);
        entities.CashReceiptEvent.CreatedBy = db.GetString(reader, 4);
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

  private bool ReadFundTransaction()
  {
    entities.FundTransaction.Populated = false;

    return Read("ReadFundTransaction",
      (db, command) =>
      {
        db.SetInt32(
          command, "fttIdentifier",
          local.HardcodedDepositedFundTransactionType.
            SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.FundTransaction.FttIdentifier = db.GetInt32(reader, 0);
        entities.FundTransaction.PcaCode = db.GetString(reader, 1);
        entities.FundTransaction.PcaEffectiveDate = db.GetDate(reader, 2);
        entities.FundTransaction.FunIdentifier = db.GetInt32(reader, 3);
        entities.FundTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.FundTransaction.Amount = db.GetDecimal(reader, 5);
        entities.FundTransaction.BusinessDate = db.GetDate(reader, 6);
        entities.FundTransaction.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.FundTransaction.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 8);
        entities.FundTransaction.Populated = true;
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
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of InterstateCollection.
    /// </summary>
    [JsonPropertyName("interstateCollection")]
    public InterstateCollection InterstateCollection
    {
      get => interstateCollection ??= new();
      set => interstateCollection = value;
    }

    private CashReceiptSourceType cashReceiptSourceType;
    private InterstateCollection interstateCollection;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Exoprt.
    /// </summary>
    [JsonPropertyName("exoprt")]
    public CashReceiptSourceType Exoprt
    {
      get => exoprt ??= new();
      set => exoprt = value;
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
    /// A value of CashReceiptEvent.
    /// </summary>
    [JsonPropertyName("cashReceiptEvent")]
    public CashReceiptEvent CashReceiptEvent
    {
      get => cashReceiptEvent ??= new();
      set => cashReceiptEvent = value;
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
    /// A value of ImportNumberOfReads.
    /// </summary>
    [JsonPropertyName("importNumberOfReads")]
    public Common ImportNumberOfReads
    {
      get => importNumberOfReads ??= new();
      set => importNumberOfReads = value;
    }

    private CashReceiptSourceType exoprt;
    private Common importNumberOfUpdates;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceipt cashReceipt;
    private Common importNumberOfReads;
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
    /// A value of HardcodedDepositedCashReceiptStatus.
    /// </summary>
    [JsonPropertyName("hardcodedDepositedCashReceiptStatus")]
    public CashReceiptStatus HardcodedDepositedCashReceiptStatus
    {
      get => hardcodedDepositedCashReceiptStatus ??= new();
      set => hardcodedDepositedCashReceiptStatus = value;
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
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
    }

    /// <summary>
    /// A value of HardcodedCsenet.
    /// </summary>
    [JsonPropertyName("hardcodedCsenet")]
    public CashReceiptType HardcodedCsenet
    {
      get => hardcodedCsenet ??= new();
      set => hardcodedCsenet = value;
    }

    /// <summary>
    /// A value of HardcodedDepositedFundTransactionType.
    /// </summary>
    [JsonPropertyName("hardcodedDepositedFundTransactionType")]
    public FundTransactionType HardcodedDepositedFundTransactionType
    {
      get => hardcodedDepositedFundTransactionType ??= new();
      set => hardcodedDepositedFundTransactionType = value;
    }

    private DateWorkArea current;
    private CashReceiptStatus hardcodedDepositedCashReceiptStatus;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceipt cashReceipt;
    private CashReceiptType hardcodedCsenet;
    private FundTransactionType hardcodedDepositedFundTransactionType;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Associate.
    /// </summary>
    [JsonPropertyName("associate")]
    public CashReceipt Associate
    {
      get => associate ??= new();
      set => associate = value;
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
    /// A value of CashReceiptStatus.
    /// </summary>
    [JsonPropertyName("cashReceiptStatus")]
    public CashReceiptStatus CashReceiptStatus
    {
      get => cashReceiptStatus ??= new();
      set => cashReceiptStatus = value;
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
    /// A value of FundTransaction.
    /// </summary>
    [JsonPropertyName("fundTransaction")]
    public FundTransaction FundTransaction
    {
      get => fundTransaction ??= new();
      set => fundTransaction = value;
    }

    /// <summary>
    /// A value of FundTransactionType.
    /// </summary>
    [JsonPropertyName("fundTransactionType")]
    public FundTransactionType FundTransactionType
    {
      get => fundTransactionType ??= new();
      set => fundTransactionType = value;
    }

    private CashReceipt associate;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptStatus cashReceiptStatus;
    private CashReceiptSourceType cashReceiptSourceType;
    private FundTransaction fundTransaction;
    private FundTransactionType fundTransactionType;
  }
#endregion
}
