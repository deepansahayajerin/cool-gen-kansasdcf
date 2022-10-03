// Program: FN_B644_CREATE_ONE_MONTH_SUMMARY, ID: 372691891, model: 746.
// Short name: SWE02406
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B644_CREATE_ONE_MONTH_SUMMARY.
/// </summary>
[Serializable]
public partial class FnB644CreateOneMonthSummary: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B644_CREATE_ONE_MONTH_SUMMARY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB644CreateOneMonthSummary(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB644CreateOneMonthSummary.
  /// </summary>
  public FnB644CreateOneMonthSummary(IContext context, Import import,
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
    // ****************************************************************************
    // * Create the summary for this month.
    // ****************************************************************************
    local.New1.ForMthCurrBal =
      import.LastMonth.ForMthCurrBal.GetValueOrDefault();

    // ****************************************************************************
    // * Include new debts in the summary, that is, debts that accrued during 
    // the
    // * month or historical debts that were added this month.
    // ****************************************************************************
    foreach(var item in ReadDebtDetailDebt())
    {
      if (Lt(import.End.Date, entities.DebtDetail.DueDt))
      {
        continue;
      }

      local.New1.ForMthCurrBal =
        local.New1.ForMthCurrBal.GetValueOrDefault() + entities.Debt.Amount;

      if (AsChar(import.ReportNeeded.Flag) == 'Y')
      {
        local.EabFileHandling.Action = "WRITE";
        local.DueDate.Text10 =
          NumberToString(Month(entities.DebtDetail.DueDt), 14, 2) + "/" + NumberToString
          (Day(entities.DebtDetail.DueDt), 14, 2) + "/" + NumberToString
          (Year(entities.DebtDetail.DueDt), 12, 4);

        // *******************************
        // *   Format the Amount Due     *
        // *******************************
        local.EabConvertNumeric.SendAmount =
          NumberToString((long)(entities.Debt.Amount * 100), 15);
        UseEabConvertNumeric1();
        local.AmountDue.Text10 =
          Substring(local.EabConvertNumeric.ReturnCurrencySigned, 12, 10);

        // *******************************
        // *   Format the Amount Paid    *
        // *******************************
        local.AmountPaid.Text10 = "";

        // *******************************
        // *   Format the Balance Due    *
        // *******************************
        local.EabConvertNumeric.SendAmount =
          NumberToString((long)(local.New1.ForMthCurrBal.GetValueOrDefault() * 100
          ), 15);
        UseEabConvertNumeric1();
        local.BalanceDue.Text10 =
          Substring(local.EabConvertNumeric.ReturnCurrencySigned, 12, 10);
        local.EabReportSend.RptDetail = local.DueDate.Text10 + "  " + "Debt Detail" +
          "  " + local.AmountDue.Text10 + "   " + local.AmountPaid.Text10 + "   " +
          local.BalanceDue.Text10;
        UseCabBusinessReport01();
      }
    }

    // 1)  Were there any payments?
    foreach(var item in ReadCollection2())
    {
      // ***************************************************************************
      // *  Record the collection even if it was subsequently adjusted.
      // ***************************************************************************
      local.New1.ForMthCurrBal =
        local.New1.ForMthCurrBal.GetValueOrDefault() - entities
        .Collection.Amount;

      if (AsChar(import.ReportNeeded.Flag) == 'Y')
      {
        local.EabFileHandling.Action = "WRITE";
        local.DueDate.Text10 =
          NumberToString(Month(entities.Collection.CollectionDt), 14, 2) + "/"
          + NumberToString(Day(entities.Collection.CollectionDt), 14, 2) + "/"
          + NumberToString(Year(entities.Collection.CollectionDt), 12, 4);

        // *******************************
        // *   Format the Amount Due     *
        // *******************************
        local.AmountDue.Text10 = "";

        // *******************************
        // *   Format the Amount Paid    *
        // *******************************
        local.EabConvertNumeric.SendAmount =
          NumberToString((long)(entities.Collection.Amount * 100), 15);
        UseEabConvertNumeric1();
        local.AmountPaid.Text10 =
          Substring(local.EabConvertNumeric.ReturnCurrencySigned, 12, 10);

        // *******************************
        // *   Format the Balance Due    *
        // *******************************
        local.EabConvertNumeric.SendAmount =
          NumberToString((long)(local.New1.ForMthCurrBal.GetValueOrDefault() * 100
          ), 15);
        UseEabConvertNumeric1();
        local.BalanceDue.Text10 =
          Substring(local.EabConvertNumeric.ReturnCurrencySigned, 12, 10);
        local.EabReportSend.RptDetail = local.DueDate.Text10 + "  " + "Collection " +
          "  " + local.AmountDue.Text10 + "   " + local.AmountPaid.Text10 + "   " +
          local.BalanceDue.Text10;
        UseCabBusinessReport01();
      }
    }

    // 2)  Were there any collection adjustments?
    foreach(var item in ReadCollection1())
    {
      // ***************************************************************************
      // *  Report the collection adjustment.
      // ***************************************************************************
      local.New1.ForMthCurrBal =
        local.New1.ForMthCurrBal.GetValueOrDefault() + entities
        .Collection.Amount;

      if (AsChar(import.ReportNeeded.Flag) == 'Y')
      {
        local.EabFileHandling.Action = "WRITE";
        local.DueDate.Text10 =
          NumberToString(Month(entities.Collection.CollectionAdjustmentDt), 14,
          2) + "/" + NumberToString
          (Day(entities.Collection.CollectionAdjustmentDt), 14, 2) + "/" + NumberToString
          (Year(entities.Collection.CollectionAdjustmentDt), 12, 4);

        // *******************************
        // *   Format the Amount Due     *
        // *******************************
        local.EabConvertNumeric.SendAmount =
          NumberToString((long)(entities.Collection.Amount * 100), 15);
        UseEabConvertNumeric1();
        local.AmountDue.Text10 =
          Substring(local.EabConvertNumeric.ReturnCurrencySigned, 12, 10);

        // *******************************
        // *   Format the Amount Paid    *
        // *******************************
        local.AmountPaid.Text10 = "";

        // *******************************
        // *   Format the Balance Due    *
        // *******************************
        local.EabConvertNumeric.SendAmount =
          NumberToString((long)(local.New1.ForMthCurrBal.GetValueOrDefault() * 100
          ), 15);
        UseEabConvertNumeric1();
        local.BalanceDue.Text10 =
          Substring(local.EabConvertNumeric.ReturnCurrencySigned, 12, 10);
        local.EabReportSend.RptDetail = local.DueDate.Text10 + "  " + "Coll Adjust" +
          "  " + local.AmountDue.Text10 + "   " + local.AmountPaid.Text10 + "   " +
          local.BalanceDue.Text10;
        UseCabBusinessReport01();
      }
    }

    // 3)  Were there any debt adjustments?
    foreach(var item in ReadObligationTransaction())
    {
      // *  Back off the Debt Adjustment (usually decrease amount owed)
      if (AsChar(entities.ObligationTransaction.DebtAdjustmentType) == 'I')
      {
        local.New1.ForMthCurrBal =
          local.New1.ForMthCurrBal.GetValueOrDefault() + entities
          .ObligationTransaction.Amount;
      }
      else
      {
        local.New1.ForMthCurrBal =
          local.New1.ForMthCurrBal.GetValueOrDefault() - entities
          .ObligationTransaction.Amount;
      }

      if (AsChar(import.ReportNeeded.Flag) == 'Y')
      {
        local.EabFileHandling.Action = "WRITE";
        local.DueDate.Text10 =
          NumberToString(Month(entities.ObligationTransaction.DebtAdjustmentDt),
          14, 2) + "/" + NumberToString
          (Day(entities.ObligationTransaction.DebtAdjustmentDt), 14, 2) + "/"
          + NumberToString
          (Year(entities.ObligationTransaction.DebtAdjustmentDt), 12, 4);

        if (AsChar(entities.ObligationTransaction.DebtAdjustmentType) == 'I')
        {
          // *******************************
          // *   Format the Amount Due     *
          // *******************************
          local.EabConvertNumeric.SendAmount =
            NumberToString((long)(entities.ObligationTransaction.Amount * 100),
            15);
          UseEabConvertNumeric1();
          local.AmountDue.Text10 =
            Substring(local.EabConvertNumeric.ReturnCurrencySigned, 12, 10);
        }
        else
        {
          // *******************************
          // *   Format the Amount Due     *
          // *******************************
          local.EabConvertNumeric.SendAmount =
            NumberToString((long)(entities.ObligationTransaction.Amount * -100),
            15);
          UseEabConvertNumeric1();
          local.AmountDue.Text10 =
            Substring(local.EabConvertNumeric.ReturnCurrencySigned, 12, 10);
        }

        // *******************************
        // *   Format the Amount Paid    *
        // *******************************
        local.AmountPaid.Text10 = "";

        // *******************************
        // *   Format the Balance Due    *
        // *******************************
        local.EabConvertNumeric.SendAmount =
          NumberToString((long)(local.New1.ForMthCurrBal.GetValueOrDefault() * 100
          ), 15);
        UseEabConvertNumeric1();
        local.BalanceDue.Text10 =
          Substring(local.EabConvertNumeric.ReturnCurrencySigned, 12, 10);
        local.EabReportSend.RptDetail = local.DueDate.Text10 + "  " + "Debt Adjust" +
          "  " + local.AmountDue.Text10 + "   " + local.AmountPaid.Text10 + "   " +
          local.BalanceDue.Text10;
        UseCabBusinessReport01();
      }
    }

    local.New1.Type1 = "O";
    local.New1.YearMonth = import.NewSummary.YearMonth;
    UseFnCabCreateObligationSummary();

    if (AsChar(import.ReportNeeded.Flag) == 'Y')
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabConvertNumeric.SendAmount =
        NumberToString((long)(local.New1.ForMthCurrBal.GetValueOrDefault() * 100)
        , 1, 15);
      UseEabConvertNumeric1();
      local.EabReportSend.RptDetail =
        NumberToString(local.New1.YearMonth, 10, 6) + "    " + "Summary Added" +
        "                 " + "" + local
        .EabConvertNumeric.ReturnCurrencySigned;
      UseCabBusinessReport01();
    }

    export.LastMonth.Assign(local.New1);
  }

  private static void MoveEabConvertNumeric2(EabConvertNumeric2 source,
    EabConvertNumeric2 target)
  {
    target.ReturnCurrencySigned = source.ReturnCurrencySigned;
    target.ReturnCurrencyNegInParens = source.ReturnCurrencyNegInParens;
    target.ReturnAmountNonDecimalSigned = source.ReturnAmountNonDecimalSigned;
    target.ReturnNoCommasInNonDecimal = source.ReturnNoCommasInNonDecimal;
    target.ReturnOkFlag = source.ReturnOkFlag;
  }

  private void UseCabBusinessReport01()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabConvertNumeric1()
  {
    var useImport = new EabConvertNumeric1.Import();
    var useExport = new EabConvertNumeric1.Export();

    useImport.EabConvertNumeric.Assign(local.EabConvertNumeric);
    useExport.EabConvertNumeric.Assign(local.EabConvertNumeric);

    Call(EabConvertNumeric1.Execute, useImport, useExport);

    MoveEabConvertNumeric2(useExport.EabConvertNumeric, local.EabConvertNumeric);
      
  }

  private void UseFnCabCreateObligationSummary()
  {
    var useImport = new FnCabCreateObligationSummary.Import();
    var useExport = new FnCabCreateObligationSummary.Export();

    useImport.Obligation.Assign(import.Obligation);
    useImport.New1.Assign(local.New1);

    Call(FnCabCreateObligationSummary.Execute, useImport, useExport);

    import.Obligation.Assign(useImport.Obligation);
  }

  private IEnumerable<bool> ReadCollection1()
  {
    System.Diagnostics.Debug.Assert(import.Obligation.Populated);
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection1",
      (db, command) =>
      {
        db.SetInt32(command, "otyId", import.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgId", import.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.Obligation.CspNumber);
        db.SetString(command, "cpaType", import.Obligation.CpaType);
        db.SetDate(command, "date1", import.Begin.Date.GetValueOrDefault());
        db.SetDate(command, "date2", import.End.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.CollectionDt = db.GetDate(reader, 1);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 2);
        entities.Collection.ConcurrentInd = db.GetString(reader, 3);
        entities.Collection.CrtType = db.GetInt32(reader, 4);
        entities.Collection.CstId = db.GetInt32(reader, 5);
        entities.Collection.CrvId = db.GetInt32(reader, 6);
        entities.Collection.CrdId = db.GetInt32(reader, 7);
        entities.Collection.ObgId = db.GetInt32(reader, 8);
        entities.Collection.CspNumber = db.GetString(reader, 9);
        entities.Collection.CpaType = db.GetString(reader, 10);
        entities.Collection.OtrId = db.GetInt32(reader, 11);
        entities.Collection.OtrType = db.GetString(reader, 12);
        entities.Collection.OtyId = db.GetInt32(reader, 13);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 14);
        entities.Collection.CollectionAdjProcessDate = db.GetDate(reader, 15);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 16);
        entities.Collection.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 17);
        entities.Collection.Amount = db.GetDecimal(reader, 18);
        entities.Collection.DistributionMethod = db.GetString(reader, 19);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("DistributionMethod",
          entities.Collection.DistributionMethod);

        return true;
      });
  }

  private IEnumerable<bool> ReadCollection2()
  {
    System.Diagnostics.Debug.Assert(import.Obligation.Populated);
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection2",
      (db, command) =>
      {
        db.SetInt32(command, "otyId", import.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgId", import.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.Obligation.CspNumber);
        db.SetString(command, "cpaType", import.Obligation.CpaType);
        db.SetDateTime(
          command, "timestamp1", import.Begin.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2", import.End.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.CollectionDt = db.GetDate(reader, 1);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 2);
        entities.Collection.ConcurrentInd = db.GetString(reader, 3);
        entities.Collection.CrtType = db.GetInt32(reader, 4);
        entities.Collection.CstId = db.GetInt32(reader, 5);
        entities.Collection.CrvId = db.GetInt32(reader, 6);
        entities.Collection.CrdId = db.GetInt32(reader, 7);
        entities.Collection.ObgId = db.GetInt32(reader, 8);
        entities.Collection.CspNumber = db.GetString(reader, 9);
        entities.Collection.CpaType = db.GetString(reader, 10);
        entities.Collection.OtrId = db.GetInt32(reader, 11);
        entities.Collection.OtrType = db.GetString(reader, 12);
        entities.Collection.OtyId = db.GetInt32(reader, 13);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 14);
        entities.Collection.CollectionAdjProcessDate = db.GetDate(reader, 15);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 16);
        entities.Collection.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 17);
        entities.Collection.Amount = db.GetDecimal(reader, 18);
        entities.Collection.DistributionMethod = db.GetString(reader, 19);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("DistributionMethod",
          entities.Collection.DistributionMethod);

        return true;
      });
  }

  private IEnumerable<bool> ReadDebtDetailDebt()
  {
    System.Diagnostics.Debug.Assert(import.Obligation.Populated);
    entities.DebtDetail.Populated = false;
    entities.Debt.Populated = false;

    return ReadEach("ReadDebtDetailDebt",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", import.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          import.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.Obligation.CspNumber);
        db.SetString(command, "cpaType", import.Obligation.CpaType);
        db.SetDateTime(
          command, "timestamp1", import.Begin.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2", import.End.Timestamp.GetValueOrDefault());
        db.SetDate(command, "date1", import.Begin.Date.GetValueOrDefault());
        db.SetDate(command, "date2", import.End.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.Debt.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.Debt.Type1 = db.GetString(reader, 5);
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 7);
        entities.DebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 8);
        entities.DebtDetail.LastUpdatedTmst = db.GetNullableDateTime(reader, 9);
        entities.DebtDetail.LastUpdatedBy = db.GetNullableString(reader, 10);
        entities.Debt.Amount = db.GetDecimal(reader, 11);
        entities.Debt.DebtAdjustmentInd = db.GetString(reader, 12);
        entities.Debt.CreatedTmst = db.GetDateTime(reader, 13);
        entities.Debt.DebtType = db.GetString(reader, 14);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 15);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 16);
        entities.DebtDetail.Populated = true;
        entities.Debt.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<ObligationTransaction>("CpaType", entities.Debt.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);
        CheckValid<ObligationTransaction>("DebtAdjustmentInd",
          entities.Debt.DebtAdjustmentInd);
        CheckValid<ObligationTransaction>("DebtType", entities.Debt.DebtType);
        CheckValid<ObligationTransaction>("CpaSupType", entities.Debt.CpaSupType);
          

        return true;
      });
  }

  private IEnumerable<bool> ReadObligationTransaction()
  {
    System.Diagnostics.Debug.Assert(import.Obligation.Populated);
    entities.ObligationTransaction.Populated = false;

    return ReadEach("ReadObligationTransaction",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", import.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          import.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.Obligation.CspNumber);
        db.SetString(command, "cpaType", import.Obligation.CpaType);
        db.SetDateTime(
          command, "createdTmst1", import.Begin.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTmst2", import.End.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 1);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 2);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 4);
        entities.ObligationTransaction.Amount = db.GetDecimal(reader, 5);
        entities.ObligationTransaction.DebtAdjustmentType =
          db.GetString(reader, 6);
        entities.ObligationTransaction.DebtAdjustmentDt = db.GetDate(reader, 7);
        entities.ObligationTransaction.CreatedTmst = db.GetDateTime(reader, 8);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 9);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 10);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 11);
        entities.ObligationTransaction.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.ObligationTransaction.CpaType);
        CheckValid<ObligationTransaction>("Type1",
          entities.ObligationTransaction.Type1);
        CheckValid<ObligationTransaction>("DebtAdjustmentType",
          entities.ObligationTransaction.DebtAdjustmentType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ObligationTransaction.CpaSupType);

        return true;
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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

    /// <summary>
    /// A value of Begin.
    /// </summary>
    [JsonPropertyName("begin")]
    public DateWorkArea Begin
    {
      get => begin ??= new();
      set => begin = value;
    }

    /// <summary>
    /// A value of End.
    /// </summary>
    [JsonPropertyName("end")]
    public DateWorkArea End
    {
      get => end ??= new();
      set => end = value;
    }

    /// <summary>
    /// A value of LastMonth.
    /// </summary>
    [JsonPropertyName("lastMonth")]
    public MonthlyObligorSummary LastMonth
    {
      get => lastMonth ??= new();
      set => lastMonth = value;
    }

    /// <summary>
    /// A value of NewSummary.
    /// </summary>
    [JsonPropertyName("newSummary")]
    public DateWorkArea NewSummary
    {
      get => newSummary ??= new();
      set => newSummary = value;
    }

    /// <summary>
    /// A value of ReportNeeded.
    /// </summary>
    [JsonPropertyName("reportNeeded")]
    public Common ReportNeeded
    {
      get => reportNeeded ??= new();
      set => reportNeeded = value;
    }

    private CsePersonAccount obligor;
    private Obligation obligation;
    private DateWorkArea begin;
    private DateWorkArea end;
    private MonthlyObligorSummary lastMonth;
    private DateWorkArea newSummary;
    private Common reportNeeded;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of LastMonth.
    /// </summary>
    [JsonPropertyName("lastMonth")]
    public MonthlyObligorSummary LastMonth
    {
      get => lastMonth ??= new();
      set => lastMonth = value;
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

    private MonthlyObligorSummary lastMonth;
    private EabFileHandling eabFileHandling;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of BalanceDue.
    /// </summary>
    [JsonPropertyName("balanceDue")]
    public TextWorkArea BalanceDue
    {
      get => balanceDue ??= new();
      set => balanceDue = value;
    }

    /// <summary>
    /// A value of AmountPaid.
    /// </summary>
    [JsonPropertyName("amountPaid")]
    public TextWorkArea AmountPaid
    {
      get => amountPaid ??= new();
      set => amountPaid = value;
    }

    /// <summary>
    /// A value of AmountDue.
    /// </summary>
    [JsonPropertyName("amountDue")]
    public TextWorkArea AmountDue
    {
      get => amountDue ??= new();
      set => amountDue = value;
    }

    /// <summary>
    /// A value of EabConvertNumeric.
    /// </summary>
    [JsonPropertyName("eabConvertNumeric")]
    public EabConvertNumeric2 EabConvertNumeric
    {
      get => eabConvertNumeric ??= new();
      set => eabConvertNumeric = value;
    }

    /// <summary>
    /// A value of DueDate.
    /// </summary>
    [JsonPropertyName("dueDate")]
    public TextWorkArea DueDate
    {
      get => dueDate ??= new();
      set => dueDate = value;
    }

    /// <summary>
    /// A value of BeginBalanceAmt.
    /// </summary>
    [JsonPropertyName("beginBalanceAmt")]
    public Common BeginBalanceAmt
    {
      get => beginBalanceAmt ??= new();
      set => beginBalanceAmt = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public MonthlyObligorSummary New1
    {
      get => new1 ??= new();
      set => new1 = value;
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

    /// <summary>
    /// A value of BeginSummaryPeriod.
    /// </summary>
    [JsonPropertyName("beginSummaryPeriod")]
    public TextWorkArea BeginSummaryPeriod
    {
      get => beginSummaryPeriod ??= new();
      set => beginSummaryPeriod = value;
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

    private TextWorkArea balanceDue;
    private TextWorkArea amountPaid;
    private TextWorkArea amountDue;
    private EabConvertNumeric2 eabConvertNumeric;
    private TextWorkArea dueDate;
    private Common beginBalanceAmt;
    private MonthlyObligorSummary new1;
    private EabReportSend eabReportSend;
    private TextWorkArea beginSummaryPeriod;
    private EabFileHandling eabFileHandling;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
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

    private ObligationTransaction obligationTransaction;
    private Collection collection;
    private DebtDetail debtDetail;
    private ObligationTransaction debt;
  }
#endregion
}
