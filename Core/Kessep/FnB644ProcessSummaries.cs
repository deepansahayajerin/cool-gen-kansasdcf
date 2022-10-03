// Program: FN_B644_PROCESS_SUMMARIES, ID: 372691362, model: 746.
// Short name: SWE02424
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B644_PROCESS_SUMMARIES.
/// </summary>
[Serializable]
public partial class FnB644ProcessSummaries: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B644_PROCESS_SUMMARIES program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB644ProcessSummaries(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB644ProcessSummaries.
  /// </summary>
  public FnB644ProcessSummaries(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // **************************************************************
    // Make sure new summary does not exist.
    // **************************************************************
    export.SummariesCreated.Count = import.SummariesCreated.Count;

    if (AsChar(import.CreateMissing.Flag) == 'Y')
    {
      // **************************************************************
      // Create all missing summaries starting with conversion date or
      // obligation start date, whichever is greater.
      // **************************************************************
      UseFnB644CreateMissingSummaries();

      return;
    }

    if (ReadMonthlyObligorSummary1())
    {
      return;
    }

    // **************************************************************
    // Get last month's summary.
    // **************************************************************
    if (ReadMonthlyObligorSummary2())
    {
      local.BeginBalanceAmt.AverageCurrency =
        entities.MonthlyObligorSummary.ForMthCurrBal.GetValueOrDefault();
      local.EndingSummary.YearMonth = import.CurrentPeriod.YearMonth;
      UseFnB644CreateOneMonthSummary();
      ++export.SummariesCreated.Count;
    }
    else
    {
      // **************************************************************
      // Create all missing summaries starting with conversion date or
      // obligation start date, whichever is greater.
      // **************************************************************
      UseFnB644CreateMissingSummaries();
    }
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
  }

  private void UseFnB644CreateMissingSummaries()
  {
    var useImport = new FnB644CreateMissingSummaries.Import();
    var useExport = new FnB644CreateMissingSummaries.Export();

    useImport.ReportNeeded.Flag = import.ReportNeeded.Flag;
    useImport.Conversion.Date = import.Conversion.Date;
    useImport.Obligation.Assign(import.Obligation);
    useImport.Obligor.Assign(import.Obligor);
    MoveDateWorkArea(import.EndSummaryPeriod, useImport.EndSummaryPeriod);
    MoveDateWorkArea(import.BeginSummaryPeriod, useImport.BeginSummaryPeriod);
    useImport.PreviousPeriod.YearMonth = import.PreviousPeriod.YearMonth;
    useImport.SummariesCreated.Count = export.SummariesCreated.Count;

    Call(FnB644CreateMissingSummaries.Execute, useImport, useExport);

    import.Obligation.Assign(useImport.Obligation);
    import.Obligor.Assign(useImport.Obligor);
    export.SummariesCreated.Count = useExport.SummariesCreated.Count;
  }

  private void UseFnB644CreateOneMonthSummary()
  {
    var useImport = new FnB644CreateOneMonthSummary.Import();
    var useExport = new FnB644CreateOneMonthSummary.Export();

    useImport.LastMonth.Assign(entities.MonthlyObligorSummary);
    useImport.ReportNeeded.Flag = import.ReportNeeded.Flag;
    useImport.Obligation.Assign(import.Obligation);
    useImport.Obligor.Assign(import.Obligor);
    MoveDateWorkArea(import.EndSummaryPeriod, useImport.End);
    MoveDateWorkArea(import.BeginSummaryPeriod, useImport.Begin);
    useImport.NewSummary.YearMonth = local.EndingSummary.YearMonth;

    Call(FnB644CreateOneMonthSummary.Execute, useImport, useExport);

    import.Obligation.Assign(useImport.Obligation);
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    local.Beginning.Assign(useExport.LastMonth);
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
        db.SetInt32(command, "fnclMsumYrMth", import.CurrentPeriod.YearMonth);
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
        db.SetInt32(command, "fnclMsumYrMth", import.PreviousPeriod.YearMonth);
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
    /// A value of CreateMissing.
    /// </summary>
    [JsonPropertyName("createMissing")]
    public Common CreateMissing
    {
      get => createMissing ??= new();
      set => createMissing = value;
    }

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
    /// A value of Conversion.
    /// </summary>
    [JsonPropertyName("conversion")]
    public DateWorkArea Conversion
    {
      get => conversion ??= new();
      set => conversion = value;
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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
    /// A value of PreviousPeriod.
    /// </summary>
    [JsonPropertyName("previousPeriod")]
    public MonthlyObligorSummary PreviousPeriod
    {
      get => previousPeriod ??= new();
      set => previousPeriod = value;
    }

    /// <summary>
    /// A value of CurrentPeriod.
    /// </summary>
    [JsonPropertyName("currentPeriod")]
    public MonthlyObligorSummary CurrentPeriod
    {
      get => currentPeriod ??= new();
      set => currentPeriod = value;
    }

    private Common createMissing;
    private Common summariesCreated;
    private Common reportNeeded;
    private DateWorkArea conversion;
    private Obligation obligation;
    private CsePersonAccount obligor;
    private DateWorkArea endSummaryPeriod;
    private DateWorkArea beginSummaryPeriod;
    private MonthlyObligorSummary previousPeriod;
    private MonthlyObligorSummary currentPeriod;
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of EndingSummary.
    /// </summary>
    [JsonPropertyName("endingSummary")]
    public DateWorkArea EndingSummary
    {
      get => endingSummary ??= new();
      set => endingSummary = value;
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
    /// A value of BeginBalanceAmt.
    /// </summary>
    [JsonPropertyName("beginBalanceAmt")]
    public Common BeginBalanceAmt
    {
      get => beginBalanceAmt ??= new();
      set => beginBalanceAmt = value;
    }

    /// <summary>
    /// A value of Beginning.
    /// </summary>
    [JsonPropertyName("beginning")]
    public MonthlyObligorSummary Beginning
    {
      get => beginning ??= new();
      set => beginning = value;
    }

    private EabFileHandling eabFileHandling;
    private DateWorkArea endingSummary;
    private DateWorkArea obligationStartDate;
    private Common beginBalanceAmt;
    private MonthlyObligorSummary beginning;
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
