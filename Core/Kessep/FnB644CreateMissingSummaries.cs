// Program: FN_B644_CREATE_MISSING_SUMMARIES, ID: 372691892, model: 746.
// Short name: SWE02456
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B644_CREATE_MISSING_SUMMARIES.
/// </summary>
[Serializable]
public partial class FnB644CreateMissingSummaries: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B644_CREATE_MISSING_SUMMARIES program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB644CreateMissingSummaries(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB644CreateMissingSummaries.
  /// </summary>
  public FnB644CreateMissingSummaries(IContext context, Import import,
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
    export.SummariesCreated.Count = import.SummariesCreated.Count;

    // *******************************************************************
    // Create all missing summaries starting with the month prior to
    // the conversion date or obligation start date, whichever is greater.
    // It is possible that some summaries may already exist.  If so, go
    // ahead and process the next month.
    // *******************************************************************
    UseFnCabDetermineOblgStartDate();

    if (Equal(local.ObligationStartDate.Date, local.NullDate.Date))
    {
      return;
    }

    if (Lt(import.EndSummaryPeriod.Date, local.ObligationStartDate.Date))
    {
      return;
    }

    if (Lt(local.ObligationStartDate.Date, import.Conversion.Date))
    {
      local.ObligationStartDate.Date = import.Conversion.Date;
    }

    // **********************************************************
    // CREATE INITIAL OBLIGATION SUMMARY WITH ZERO BALANCE
    // **********************************************************
    local.DateWorkArea.Date = AddMonths(local.ObligationStartDate.Date, -1);
    local.DateWorkArea.YearMonth = Year(local.DateWorkArea.Date) * 100 + Month
      (local.DateWorkArea.Date);
    local.Conversion.YearMonth = Year(import.Conversion.Date) * 100 + Month
      (import.Conversion.Date);

    if (ReadMonthlyObligorSummary2())
    {
      // **********************************************************
      // INITIAL OBLIGATION SUMMARY ALREADY EXISTS
      // **********************************************************
    }
    else
    {
      local.LastMonth.ForMthCurrBal = 0;
      local.LastMonth.Type1 = "O";
      local.LastMonth.YearMonth = local.DateWorkArea.YearMonth;
      UseFnCabCreateObligationSummary();
      ++export.SummariesCreated.Count;

      if (AsChar(import.ReportNeeded.Flag) == 'Y')
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabConvertNumeric.SendAmount =
          NumberToString((long)(local.LastMonth.ForMthCurrBal.
            GetValueOrDefault() * 100), 1, 15);
        UseEabConvertNumeric1();
        local.EabReportSend.RptDetail =
          NumberToString(local.LastMonth.YearMonth, 10, 6) + "    " + "Summary Added" +
          "                 " + "" + local
          .EabConvertNumeric.ReturnCurrencySigned;
        UseCabBusinessReport01();
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // **********************************************************
    // CREATE EACH SUBSEQUENT OBLIGATION SUMMARY
    // **********************************************************
    while(!Lt(import.EndSummaryPeriod.Date, local.End.Date))
    {
      local.DateWorkArea.Date = AddMonths(local.DateWorkArea.Date, 1);

      if (Lt(import.EndSummaryPeriod.Date, local.DateWorkArea.Date))
      {
        return;
      }

      // **********************************************************
      // DETERMINE OBLIGATION SUMMARY DATE AND TIMESTAMP RANGES
      // **********************************************************
      local.DateWorkArea.YearMonth = Year(local.DateWorkArea.Date) * 100 + Month
        (local.DateWorkArea.Date);

      if (ReadMonthlyObligorSummary1())
      {
        local.LastMonth.Assign(entities.MonthlyObligorSummary);

        continue;
      }
      else
      {
        local.Begin.Date = IntToDate(Year(local.DateWorkArea.Date) * 10000 + Month
          (local.DateWorkArea.Date) * 100 + 1);
        local.Begin.Timestamp =
          Timestamp(NumberToString(Year(local.Begin.Date), 12, 4) + "-" + NumberToString
          (Month(local.Begin.Date), 14, 2) + "-" + NumberToString
          (Day(local.Begin.Date), 14, 2) + "-00.00.00.000001");
        local.End.Date = AddMonths(local.Begin.Date, 1);
        local.End.Date = AddDays(local.End.Date, -1);
        local.End.Timestamp =
          Timestamp(NumberToString(Year(local.End.Date), 12, 4) + "-" + NumberToString
          (Month(local.End.Date), 14, 2) + "-" + NumberToString
          (Day(local.End.Date), 14, 2) + "-23.59.59.999999");
        UseFnB644CreateOneMonthSummary();
        ++export.SummariesCreated.Count;
        local.End.Date = AddMonths(local.Begin.Date, 2);
        local.End.Date = AddDays(local.End.Date, -1);
      }
    }
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
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

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
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

  private void UseFnB644CreateOneMonthSummary()
  {
    var useImport = new FnB644CreateOneMonthSummary.Import();
    var useExport = new FnB644CreateOneMonthSummary.Export();

    useImport.Obligor.Assign(import.Obligor);
    useImport.Obligation.Assign(import.Obligation);
    MoveDateWorkArea(local.Begin, useImport.Begin);
    MoveDateWorkArea(local.End, useImport.End);
    useImport.LastMonth.Assign(local.LastMonth);
    useImport.NewSummary.YearMonth = local.DateWorkArea.YearMonth;
    useImport.ReportNeeded.Flag = import.ReportNeeded.Flag;

    Call(FnB644CreateOneMonthSummary.Execute, useImport, useExport);

    import.Obligation.Assign(useImport.Obligation);
    local.LastMonth.Assign(useExport.LastMonth);
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnCabCreateObligationSummary()
  {
    var useImport = new FnCabCreateObligationSummary.Import();
    var useExport = new FnCabCreateObligationSummary.Export();

    useImport.Obligation.Assign(import.Obligation);
    useImport.New1.Assign(local.LastMonth);

    Call(FnCabCreateObligationSummary.Execute, useImport, useExport);

    import.Obligation.Assign(useImport.Obligation);
  }

  private void UseFnCabDetermineOblgStartDate()
  {
    var useImport = new FnCabDetermineOblgStartDate.Import();
    var useExport = new FnCabDetermineOblgStartDate.Export();

    useImport.Obligation.Assign(import.Obligation);
    MoveDateWorkArea(import.EndSummaryPeriod, useImport.StmtEnd);

    Call(FnCabDetermineOblgStartDate.Execute, useImport, useExport);

    local.ObligationStartDate.Date = useExport.ObligationStartDate.Date;
  }

  private bool ReadMonthlyObligorSummary1()
  {
    System.Diagnostics.Debug.Assert(import.Obligation.Populated);
    entities.MonthlyObligorSummary.Populated = false;

    return Read("ReadMonthlyObligorSummary1",
      (db, command) =>
      {
        db.
          SetNullableInt32(command, "otyType", import.Obligation.DtyGeneratedId);
          
        db.SetNullableInt32(
          command, "obgSGeneratedId",
          import.Obligation.SystemGeneratedIdentifier);
        db.
          SetNullableString(command, "cspSNumber", import.Obligation.CspNumber);
          
        db.SetNullableString(command, "cpaSType", import.Obligation.CpaType);
        db.SetInt32(command, "fnclMsumYrMth", local.DateWorkArea.YearMonth);
      },
      (db, reader) =>
      {
        entities.MonthlyObligorSummary.Type1 = db.GetString(reader, 0);
        entities.MonthlyObligorSummary.YearMonth = db.GetInt32(reader, 1);
        entities.MonthlyObligorSummary.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.MonthlyObligorSummary.CpaSType =
          db.GetNullableString(reader, 3);
        entities.MonthlyObligorSummary.CspSNumber =
          db.GetNullableString(reader, 4);
        entities.MonthlyObligorSummary.ObgSGeneratedId =
          db.GetNullableInt32(reader, 5);
        entities.MonthlyObligorSummary.OtyType = db.GetNullableInt32(reader, 6);
        entities.MonthlyObligorSummary.ForMthCurrBal =
          db.GetNullableDecimal(reader, 7);
        entities.MonthlyObligorSummary.Populated = true;
        CheckValid<MonthlyObligorSummary>("Type1",
          entities.MonthlyObligorSummary.Type1);
        CheckValid<MonthlyObligorSummary>("CpaSType",
          entities.MonthlyObligorSummary.CpaSType);
      });
  }

  private bool ReadMonthlyObligorSummary2()
  {
    System.Diagnostics.Debug.Assert(import.Obligation.Populated);
    entities.MonthlyObligorSummary.Populated = false;

    return Read("ReadMonthlyObligorSummary2",
      (db, command) =>
      {
        db.
          SetNullableInt32(command, "otyType", import.Obligation.DtyGeneratedId);
          
        db.SetNullableInt32(
          command, "obgSGeneratedId",
          import.Obligation.SystemGeneratedIdentifier);
        db.
          SetNullableString(command, "cspSNumber", import.Obligation.CspNumber);
          
        db.SetNullableString(command, "cpaSType", import.Obligation.CpaType);
        db.SetInt32(command, "fnclMsumYrMth", local.DateWorkArea.YearMonth);
      },
      (db, reader) =>
      {
        entities.MonthlyObligorSummary.Type1 = db.GetString(reader, 0);
        entities.MonthlyObligorSummary.YearMonth = db.GetInt32(reader, 1);
        entities.MonthlyObligorSummary.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.MonthlyObligorSummary.CpaSType =
          db.GetNullableString(reader, 3);
        entities.MonthlyObligorSummary.CspSNumber =
          db.GetNullableString(reader, 4);
        entities.MonthlyObligorSummary.ObgSGeneratedId =
          db.GetNullableInt32(reader, 5);
        entities.MonthlyObligorSummary.OtyType = db.GetNullableInt32(reader, 6);
        entities.MonthlyObligorSummary.ForMthCurrBal =
          db.GetNullableDecimal(reader, 7);
        entities.MonthlyObligorSummary.Populated = true;
        CheckValid<MonthlyObligorSummary>("Type1",
          entities.MonthlyObligorSummary.Type1);
        CheckValid<MonthlyObligorSummary>("CpaSType",
          entities.MonthlyObligorSummary.CpaSType);
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
    /// A value of SummariesCreated.
    /// </summary>
    [JsonPropertyName("summariesCreated")]
    public Common SummariesCreated
    {
      get => summariesCreated ??= new();
      set => summariesCreated = value;
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
    /// A value of PreviousPeriod.
    /// </summary>
    [JsonPropertyName("previousPeriod")]
    public MonthlyObligorSummary PreviousPeriod
    {
      get => previousPeriod ??= new();
      set => previousPeriod = value;
    }

    /// <summary>
    /// A value of EndSummaryPeriod.
    /// </summary>
    [JsonPropertyName("endSummaryPeriod")]
    public DateWorkArea EndSummaryPeriod
    {
      get => endSummaryPeriod ??= new();
      set => endSummaryPeriod = value;
    }

    /// <summary>
    /// A value of BeginSummaryPeriod.
    /// </summary>
    [JsonPropertyName("beginSummaryPeriod")]
    public DateWorkArea BeginSummaryPeriod
    {
      get => beginSummaryPeriod ??= new();
      set => beginSummaryPeriod = value;
    }

    /// <summary>
    /// A value of Conversion.
    /// </summary>
    [JsonPropertyName("conversion")]
    public DateWorkArea Conversion
    {
      get => conversion ??= new();
      set => conversion = value;
    }

    private Common summariesCreated;
    private Common reportNeeded;
    private CsePersonAccount obligor;
    private Obligation obligation;
    private MonthlyObligorSummary previousPeriod;
    private DateWorkArea endSummaryPeriod;
    private DateWorkArea beginSummaryPeriod;
    private DateWorkArea conversion;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of SummariesCreated.
    /// </summary>
    [JsonPropertyName("summariesCreated")]
    public Common SummariesCreated
    {
      get => summariesCreated ??= new();
      set => summariesCreated = value;
    }

    private Common summariesCreated;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Conversion.
    /// </summary>
    [JsonPropertyName("conversion")]
    public DateWorkArea Conversion
    {
      get => conversion ??= new();
      set => conversion = value;
    }

    /// <summary>
    /// A value of ObligationStartDate.
    /// </summary>
    [JsonPropertyName("obligationStartDate")]
    public DateWorkArea ObligationStartDate
    {
      get => obligationStartDate ??= new();
      set => obligationStartDate = value;
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
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of NullDate.
    /// </summary>
    [JsonPropertyName("nullDate")]
    public DateWorkArea NullDate
    {
      get => nullDate ??= new();
      set => nullDate = value;
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
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
    /// A value of LastMonth.
    /// </summary>
    [JsonPropertyName("lastMonth")]
    public MonthlyObligorSummary LastMonth
    {
      get => lastMonth ??= new();
      set => lastMonth = value;
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

    private DateWorkArea conversion;
    private DateWorkArea obligationStartDate;
    private DateWorkArea begin;
    private DateWorkArea end;
    private DateWorkArea dateWorkArea;
    private DateWorkArea nullDate;
    private EabConvertNumeric2 eabConvertNumeric;
    private EabFileHandling eabFileHandling;
    private Common beginBalanceAmt;
    private MonthlyObligorSummary lastMonth;
    private EabReportSend eabReportSend;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of MonthlyObligorSummary.
    /// </summary>
    [JsonPropertyName("monthlyObligorSummary")]
    public MonthlyObligorSummary MonthlyObligorSummary
    {
      get => monthlyObligorSummary ??= new();
      set => monthlyObligorSummary = value;
    }

    private MonthlyObligorSummary monthlyObligorSummary;
  }
#endregion
}
