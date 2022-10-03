// Program: FN_B643_ACTIVE_OBLIG_NO_PAYHIST, ID: 372921899, model: 746.
// Short name: SWE02596
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B643_ACTIVE_OBLIG_NO_PAYHIST.
/// </summary>
[Serializable]
public partial class FnB643ActiveObligNoPayhist: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B643_ACTIVE_OBLIG_NO_PAYHIST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB643ActiveObligNoPayhist(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB643ActiveObligNoPayhist.
  /// </summary>
  public FnB643ActiveObligNoPayhist(IContext context, Import import,
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
    // ********************************************************************
    // * PR NUM     DATE     NAME      DESCRIPTION                        *
    // * ------  ----------  --------  
    // ----------------------------------
    // *
    // * #78285  11-15-1999  Ed Lyman  Do not print obligations, if no    *
    // *
    // 
    // activity and zero balance.
    // *
    // ********************************************************************
    export.ActiveAndNoPayhistInd.Flag = "N";

    // *********************************************************************
    // * CHECK IF HISTORICAL PAYMENT DUE WOULD APPEAR ON CURRENT STATEMENT *
    // *********************************************************************
    ReadDebtDetailDebt();

    if (local.TotalPayHistCount.Count > 0)
    {
      return;
    }

    // *********************************************************************
    // * CHECK IF PAY HISTORY COLLECTION WOULD APPEAR ON CURRENT STATEMENT *
    // *********************************************************************
    ReadCollection2();

    if (local.TotalPayHistCount.Count > 0)
    {
      return;
    }

    // *********************************************************************
    // * CHECK IF PAY HISTORY COLLECTION ADJ WOULD APPEAR ON CURRENT STMT  *
    // *********************************************************************
    ReadCollection1();

    if (local.TotalPayHistCount.Count > 0)
    {
      return;
    }

    // **********************************************************
    // * CHECK IF AT LEAST ONE ACTIVE DEBT                      *
    // **********************************************************
    ReadDebtDetail();

    if (local.TotalAmountDue.TotalCurrency > 0)
    {
      export.ActiveAndNoPayhistInd.Flag = "Y";

      return;
    }

    // **********************************************************
    // * CHECK IF ACCRUAL INSTRUCTIONS EXIST                    *
    // **********************************************************
    if (ReadAccrualInstructions())
    {
      // **********************************************************
      // * CHECK IF ANY COLLECTION OR COLL ADJUSTMENT ACTIVITY    *
      // **********************************************************
      ReadCollection4();

      if (local.TotalPayHistCount.Count > 0)
      {
        export.ActiveAndNoPayhistInd.Flag = "Y";

        return;
      }

      ReadCollection3();

      if (local.TotalPayHistCount.Count > 0)
      {
        export.ActiveAndNoPayhistInd.Flag = "Y";
      }

      // **********************************************************
      // * OBLIGATION HAS NO ACTIVITY                             *
      // **********************************************************
    }
    else
    {
      // **********************************************************
      // * OBLIGATION IS INACTIVE
      // 
      // *
      // **********************************************************
    }
  }

  private bool ReadAccrualInstructions()
  {
    System.Diagnostics.Debug.Assert(import.Obligation.Populated);
    entities.AccrualInstructions.Populated = false;

    return Read("ReadAccrualInstructions",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDt", import.StmtBegin.Date.GetValueOrDefault());
        db.SetInt32(command, "otyId", import.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          import.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.Obligation.CspNumber);
        db.SetString(command, "cpaType", import.Obligation.CpaType);
      },
      (db, reader) =>
      {
        entities.AccrualInstructions.OtrType = db.GetString(reader, 0);
        entities.AccrualInstructions.OtyId = db.GetInt32(reader, 1);
        entities.AccrualInstructions.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.AccrualInstructions.CspNumber = db.GetString(reader, 3);
        entities.AccrualInstructions.CpaType = db.GetString(reader, 4);
        entities.AccrualInstructions.OtrGeneratedId = db.GetInt32(reader, 5);
        entities.AccrualInstructions.DiscontinueDt =
          db.GetNullableDate(reader, 6);
        entities.AccrualInstructions.Populated = true;
      });
  }

  private bool ReadCollection1()
  {
    System.Diagnostics.Debug.Assert(import.Obligation.Populated);

    return Read("ReadCollection1",
      (db, command) =>
      {
        db.SetInt32(command, "otyId", import.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgId", import.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.Obligation.CspNumber);
        db.SetString(command, "cpaType", import.Obligation.CpaType);
        db.SetNullableDateTime(
          command, "lastUpdatedTmst",
          import.StmtBegin.Timestamp.GetValueOrDefault());
        db.SetInt32(
          command, "systemGeneratedIdentifier1",
          import.FcrtRec.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier2",
          import.FdirPmt.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        local.TotalPayHistCount.Count = db.GetInt32(reader, 0);
      });
  }

  private bool ReadCollection2()
  {
    System.Diagnostics.Debug.Assert(import.Obligation.Populated);

    return Read("ReadCollection2",
      (db, command) =>
      {
        db.SetInt32(command, "otyId", import.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgId", import.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.Obligation.CspNumber);
        db.SetString(command, "cpaType", import.Obligation.CpaType);
        db.SetDateTime(
          command, "createdTmst",
          import.StmtBegin.Timestamp.GetValueOrDefault());
        db.SetInt32(
          command, "systemGeneratedIdentifier1",
          import.FcrtRec.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier2",
          import.FdirPmt.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        local.TotalPayHistCount.Count = db.GetInt32(reader, 0);
      });
  }

  private bool ReadCollection3()
  {
    System.Diagnostics.Debug.Assert(import.Obligation.Populated);

    return Read("ReadCollection3",
      (db, command) =>
      {
        db.SetInt32(command, "otyId", import.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgId", import.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.Obligation.CspNumber);
        db.SetString(command, "cpaType", import.Obligation.CpaType);
        db.SetDateTime(
          command, "timestamp1",
          import.StmtBegin.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2", import.StmtEnd.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        local.TotalPayHistCount.Count = db.GetInt32(reader, 0);
      });
  }

  private bool ReadCollection4()
  {
    System.Diagnostics.Debug.Assert(import.Obligation.Populated);

    return Read("ReadCollection4",
      (db, command) =>
      {
        db.SetInt32(command, "otyId", import.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgId", import.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.Obligation.CspNumber);
        db.SetString(command, "cpaType", import.Obligation.CpaType);
        db.SetDateTime(
          command, "timestamp1",
          import.StmtBegin.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2", import.StmtEnd.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        local.TotalPayHistCount.Count = db.GetInt32(reader, 0);
      });
  }

  private bool ReadDebtDetail()
  {
    System.Diagnostics.Debug.Assert(import.Obligation.Populated);

    return Read("ReadDebtDetail",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "retiredDt", local.Null1.Date.GetValueOrDefault());
        db.SetInt32(command, "otyType", import.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          import.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.Obligation.CspNumber);
        db.SetString(command, "cpaType", import.Obligation.CpaType);
      },
      (db, reader) =>
      {
        local.TotalAmountDue.TotalCurrency = db.GetDecimal(reader, 0);
      });
  }

  private bool ReadDebtDetailDebt()
  {
    System.Diagnostics.Debug.Assert(import.Obligation.Populated);

    return Read("ReadDebtDetailDebt",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", import.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          import.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.Obligation.CspNumber);
        db.SetString(command, "cpaType", import.Obligation.CpaType);
        db.SetDateTime(
          command, "createdTmst",
          import.StmtBegin.Timestamp.GetValueOrDefault());
        db.SetDate(command, "dueDt", import.StmtBegin.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        local.TotalPayHistCount.Count = db.GetInt32(reader, 0);
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
    /// A value of FdirPmt.
    /// </summary>
    [JsonPropertyName("fdirPmt")]
    public CashReceiptType FdirPmt
    {
      get => fdirPmt ??= new();
      set => fdirPmt = value;
    }

    /// <summary>
    /// A value of FcrtRec.
    /// </summary>
    [JsonPropertyName("fcrtRec")]
    public CashReceiptType FcrtRec
    {
      get => fcrtRec ??= new();
      set => fcrtRec = value;
    }

    /// <summary>
    /// A value of StmtBegin.
    /// </summary>
    [JsonPropertyName("stmtBegin")]
    public DateWorkArea StmtBegin
    {
      get => stmtBegin ??= new();
      set => stmtBegin = value;
    }

    /// <summary>
    /// A value of StmtEnd.
    /// </summary>
    [JsonPropertyName("stmtEnd")]
    public DateWorkArea StmtEnd
    {
      get => stmtEnd ??= new();
      set => stmtEnd = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    private CashReceiptType fdirPmt;
    private CashReceiptType fcrtRec;
    private DateWorkArea stmtBegin;
    private DateWorkArea stmtEnd;
    private Obligation obligation;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ActiveAndNoPayhistInd.
    /// </summary>
    [JsonPropertyName("activeAndNoPayhistInd")]
    public Common ActiveAndNoPayhistInd
    {
      get => activeAndNoPayhistInd ??= new();
      set => activeAndNoPayhistInd = value;
    }

    private Common activeAndNoPayhistInd;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of TotalPayHistCount.
    /// </summary>
    [JsonPropertyName("totalPayHistCount")]
    public Common TotalPayHistCount
    {
      get => totalPayHistCount ??= new();
      set => totalPayHistCount = value;
    }

    /// <summary>
    /// A value of TotalAmountDue.
    /// </summary>
    [JsonPropertyName("totalAmountDue")]
    public Common TotalAmountDue
    {
      get => totalAmountDue ??= new();
      set => totalAmountDue = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    private Common totalPayHistCount;
    private Common totalAmountDue;
    private DateWorkArea null1;
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
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    /// <summary>
    /// A value of Debt.
    /// </summary>
    [JsonPropertyName("debt")]
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
    }

    /// <summary>
    /// A value of AccrualInstructions.
    /// </summary>
    [JsonPropertyName("accrualInstructions")]
    public AccrualInstructions AccrualInstructions
    {
      get => accrualInstructions ??= new();
      set => accrualInstructions = value;
    }

    private CashReceipt cashReceipt;
    private CashReceiptType cashReceiptType;
    private CashReceiptDetail cashReceiptDetail;
    private Collection collection;
    private DebtDetail debtDetail;
    private ObligationTransaction debt;
    private AccrualInstructions accrualInstructions;
  }
#endregion
}
