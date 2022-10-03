// Program: FN_B643_OBLIGOR_SUPPRESSION, ID: 372683791, model: 746.
// Short name: SWE02403
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B643_OBLIGOR_SUPPRESSION.
/// </summary>
[Serializable]
public partial class FnB643ObligorSuppression: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B643_OBLIGOR_SUPPRESSION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB643ObligorSuppression(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB643ObligorSuppression.
  /// </summary>
  public FnB643ObligorSuppression(IContext context, Import import, Export export)
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
    // ********************************************************************
    // * PR NUM     DATE     NAME      DESCRIPTION                        *
    // * ------  ----------  --------  
    // ----------------------------------
    // *
    // * #78285  11-15-1999  Ed Lyman  Do not print stmt for obligor, if  *
    // *
    // 
    // any undistributed from
    // conversion. *
    // ********************************************************************
    export.CpnsSuppressByObligor.Count = import.CpnsSuppressByObligor.Count;
    export.StmtSuppressByObligor.Count = import.StmtSuppressByObligor.Count;

    // ***********************************************************
    // DETERMINE IF ANY UNDISTRIBUTED FUNDS FROM CONVERSION REMAIN
    // ***********************************************************
    if (ReadCashReceiptDetail())
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "No statement because undistributed funds remain from conversion for Obligor number = " +
        import.Client.Number;
      UseCabErrorReport();

      // *****  Statements and Coupons suppressed *****
      ++export.StmtSuppressByObligor.Count;
      ++export.CpnsSuppressByObligor.Count;
      export.SuppressBothByObligor.Flag = "Y";

      return;
    }

    // **********************************************************
    // DETERMINE IF OBLIGOR IS ALIVE
    // **********************************************************
    if (!Lt(import.Process.Date, import.Client.DateOfDeath) && Lt
      (local.Null1.Date, import.Client.DateOfDeath))
    {
      // *****  Statements and Coupons suppressed *****
      ++export.StmtSuppressByObligor.Count;
      ++export.CpnsSuppressByObligor.Count;
      export.SuppressBothByObligor.Flag = "Y";

      return;
    }

    // **********************************************************
    // DETERMINE IF GOOD CAUSE FOR NOT SENDING STATEMENT
    // OR IF GOOD CAUSE PENDING FOR NOT SENDING STATEMENT.
    // **********************************************************
    foreach(var item in ReadAbsentParent())
    {
      if (ReadGoodCause())
      {
        if (Equal(entities.GoodCause.Code, "GC") || Equal
          (entities.GoodCause.Code, "PD"))
        {
          // *****  Statements and Coupons suppressed *****
          ++export.StmtSuppressByObligor.Count;
          ++export.CpnsSuppressByObligor.Count;
          export.SuppressBothByObligor.Flag = "Y";

          return;
        }
      }
    }

    // **********************************************************
    // DETERMINE IF OBLIGOR IS BANKRUPT
    // **********************************************************
    if (ReadBankruptcy())
    {
      if (Lt(local.Null1.Date, entities.Bankruptcy.BankruptcyDischargeDate))
      {
        goto Read;
      }

      // *****  Statements and Coupons suppressed *****
      ++export.StmtSuppressByObligor.Count;
      ++export.CpnsSuppressByObligor.Count;
      export.SuppressBothByObligor.Flag = "Y";

      return;
    }

Read:

    // **********************************************************
    // DETERMINE IF STMTS OR COUPONS ARE SUPPRESSED BY OBLIGOR
    // **********************************************************
    // STMT_COUPON_SUPP_STATUS_HIST IS SUBTYPED AS FOLLOWS:
    // "R" = The suppression is tied to the Obligor
    // 
    // "O" = The suppression is tied to the Obligation
    // **********************************************************
    if (ReadStmtCpnSuppStatObligor())
    {
      if (AsChar(entities.StmtCpnSuppStatObligor.DocTypeToSuppress) == 'S')
      {
        // *****  Statements and Coupons suppressed *****
        ++export.StmtSuppressByObligor.Count;
        ++export.CpnsSuppressByObligor.Count;
        export.SuppressBothByObligor.Flag = "Y";
      }
      else
      {
        // *****  Only the Coupons are suppressed *****
        export.SuppressCpnsByObligor.Flag = "Y";
        ++export.CpnsSuppressByObligor.Count;
      }
    }
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private IEnumerable<bool> ReadAbsentParent()
  {
    entities.AbsentParent.Populated = false;

    return ReadEach("ReadAbsentParent",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.Client.Number);
      },
      (db, reader) =>
      {
        entities.AbsentParent.CasNumber = db.GetString(reader, 0);
        entities.AbsentParent.CspNumber = db.GetString(reader, 1);
        entities.AbsentParent.Type1 = db.GetString(reader, 2);
        entities.AbsentParent.Identifier = db.GetInt32(reader, 3);
        entities.AbsentParent.StartDate = db.GetNullableDate(reader, 4);
        entities.AbsentParent.EndDate = db.GetNullableDate(reader, 5);
        entities.AbsentParent.Populated = true;
        CheckValid<CaseRole>("Type1", entities.AbsentParent.Type1);

        return true;
      });
  }

  private bool ReadBankruptcy()
  {
    entities.Bankruptcy.Populated = false;

    return Read("ReadBankruptcy",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.Client.Number);
      },
      (db, reader) =>
      {
        entities.Bankruptcy.CspNumber = db.GetString(reader, 0);
        entities.Bankruptcy.Identifier = db.GetInt32(reader, 1);
        entities.Bankruptcy.BankruptcyDischargeDate =
          db.GetNullableDate(reader, 2);
        entities.Bankruptcy.Populated = true;
      });
  }

  private bool ReadCashReceiptDetail()
  {
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetail",
      (db, command) =>
      {
        db.SetNullableString(command, "oblgorPrsnNbr", import.Client.Number);
        db.SetInt32(
          command, "systemGeneratedIdentifier1",
          import.FcrtRec.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier2",
          import.FdirPmt.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.Populated = true;
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.CashReceiptDetail.CollectionAmtFullyAppliedInd);
      });
  }

  private bool ReadGoodCause()
  {
    System.Diagnostics.Debug.Assert(entities.AbsentParent.Populated);
    entities.GoodCause.Populated = false;

    return Read("ReadGoodCause",
      (db, command) =>
      {
        db.SetNullableString(
          command, "casNumber1", entities.AbsentParent.CasNumber);
        db.SetNullableInt32(
          command, "croIdentifier1", entities.AbsentParent.Identifier);
        db.SetNullableString(command, "croType1", entities.AbsentParent.Type1);
        db.SetNullableString(
          command, "cspNumber1", entities.AbsentParent.CspNumber);
        db.SetNullableDate(
          command, "effectiveDate", import.Process.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.GoodCause.Code = db.GetNullableString(reader, 0);
        entities.GoodCause.EffectiveDate = db.GetNullableDate(reader, 1);
        entities.GoodCause.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.GoodCause.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.GoodCause.CasNumber = db.GetString(reader, 4);
        entities.GoodCause.CspNumber = db.GetString(reader, 5);
        entities.GoodCause.CroType = db.GetString(reader, 6);
        entities.GoodCause.CroIdentifier = db.GetInt32(reader, 7);
        entities.GoodCause.CasNumber1 = db.GetNullableString(reader, 8);
        entities.GoodCause.CspNumber1 = db.GetNullableString(reader, 9);
        entities.GoodCause.CroType1 = db.GetNullableString(reader, 10);
        entities.GoodCause.CroIdentifier1 = db.GetNullableInt32(reader, 11);
        entities.GoodCause.Populated = true;
        CheckValid<GoodCause>("CroType", entities.GoodCause.CroType);
        CheckValid<GoodCause>("CroType1", entities.GoodCause.CroType1);
      });
  }

  private bool ReadStmtCpnSuppStatObligor()
  {
    System.Diagnostics.Debug.Assert(import.Obligor.Populated);
    entities.StmtCpnSuppStatObligor.Populated = false;

    return Read("ReadStmtCpnSuppStatObligor",
      (db, command) =>
      {
        db.SetString(command, "cpaType", import.Obligor.Type1);
        db.SetString(command, "cspNumber", import.Obligor.CspNumber);
        db.SetDate(
          command, "effectiveDate", import.Process.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.StmtCpnSuppStatObligor.CpaType = db.GetString(reader, 0);
        entities.StmtCpnSuppStatObligor.CspNumber = db.GetString(reader, 1);
        entities.StmtCpnSuppStatObligor.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.StmtCpnSuppStatObligor.Type1 = db.GetString(reader, 3);
        entities.StmtCpnSuppStatObligor.EffectiveDate = db.GetDate(reader, 4);
        entities.StmtCpnSuppStatObligor.DiscontinueDate = db.GetDate(reader, 5);
        entities.StmtCpnSuppStatObligor.DocTypeToSuppress =
          db.GetString(reader, 6);
        entities.StmtCpnSuppStatObligor.Populated = true;
        CheckValid<StmtCouponSuppStatusHist>("CpaType",
          entities.StmtCpnSuppStatObligor.CpaType);
        CheckValid<StmtCouponSuppStatusHist>("Type1",
          entities.StmtCpnSuppStatObligor.Type1);
        CheckValid<StmtCouponSuppStatusHist>("DocTypeToSuppress",
          entities.StmtCpnSuppStatObligor.DocTypeToSuppress);
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
    /// A value of Client.
    /// </summary>
    [JsonPropertyName("client")]
    public CsePerson Client
    {
      get => client ??= new();
      set => client = value;
    }

    /// <summary>
    /// A value of CpnsSuppressByObligor.
    /// </summary>
    [JsonPropertyName("cpnsSuppressByObligor")]
    public Common CpnsSuppressByObligor
    {
      get => cpnsSuppressByObligor ??= new();
      set => cpnsSuppressByObligor = value;
    }

    /// <summary>
    /// A value of StmtSuppressByObligor.
    /// </summary>
    [JsonPropertyName("stmtSuppressByObligor")]
    public Common StmtSuppressByObligor
    {
      get => stmtSuppressByObligor ??= new();
      set => stmtSuppressByObligor = value;
    }

    /// <summary>
    /// A value of Process.
    /// </summary>
    [JsonPropertyName("process")]
    public DateWorkArea Process
    {
      get => process ??= new();
      set => process = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    private CashReceiptType fdirPmt;
    private CashReceiptType fcrtRec;
    private CsePerson client;
    private Common cpnsSuppressByObligor;
    private Common stmtSuppressByObligor;
    private DateWorkArea process;
    private CsePersonAccount obligor;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of SuppressCpnsByObligor.
    /// </summary>
    [JsonPropertyName("suppressCpnsByObligor")]
    public Common SuppressCpnsByObligor
    {
      get => suppressCpnsByObligor ??= new();
      set => suppressCpnsByObligor = value;
    }

    /// <summary>
    /// A value of SuppressBothByObligor.
    /// </summary>
    [JsonPropertyName("suppressBothByObligor")]
    public Common SuppressBothByObligor
    {
      get => suppressBothByObligor ??= new();
      set => suppressBothByObligor = value;
    }

    /// <summary>
    /// A value of CpnsSuppressByObligor.
    /// </summary>
    [JsonPropertyName("cpnsSuppressByObligor")]
    public Common CpnsSuppressByObligor
    {
      get => cpnsSuppressByObligor ??= new();
      set => cpnsSuppressByObligor = value;
    }

    /// <summary>
    /// A value of StmtSuppressByObligor.
    /// </summary>
    [JsonPropertyName("stmtSuppressByObligor")]
    public Common StmtSuppressByObligor
    {
      get => stmtSuppressByObligor ??= new();
      set => stmtSuppressByObligor = value;
    }

    private Common suppressCpnsByObligor;
    private Common suppressBothByObligor;
    private Common cpnsSuppressByObligor;
    private Common stmtSuppressByObligor;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    private DateWorkArea null1;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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

    /// <summary>
    /// A value of AbsentParent.
    /// </summary>
    [JsonPropertyName("absentParent")]
    public CaseRole AbsentParent
    {
      get => absentParent ??= new();
      set => absentParent = value;
    }

    /// <summary>
    /// A value of GoodCause.
    /// </summary>
    [JsonPropertyName("goodCause")]
    public GoodCause GoodCause
    {
      get => goodCause ??= new();
      set => goodCause = value;
    }

    /// <summary>
    /// A value of Bankruptcy.
    /// </summary>
    [JsonPropertyName("bankruptcy")]
    public Bankruptcy Bankruptcy
    {
      get => bankruptcy ??= new();
      set => bankruptcy = value;
    }

    /// <summary>
    /// A value of StmtCpnSuppStatObligor.
    /// </summary>
    [JsonPropertyName("stmtCpnSuppStatObligor")]
    public StmtCouponSuppStatusHist StmtCpnSuppStatObligor
    {
      get => stmtCpnSuppStatObligor ??= new();
      set => stmtCpnSuppStatObligor = value;
    }

    private CashReceiptType cashReceiptType;
    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
    private CaseRole absentParent;
    private GoodCause goodCause;
    private Bankruptcy bankruptcy;
    private StmtCouponSuppStatusHist stmtCpnSuppStatObligor;
  }
#endregion
}
