// Program: FN_B644_MTHLY_OBLIGATION_SUMMARY, ID: 372691286, model: 746.
// Short name: SWEF644B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B644_MTHLY_OBLIGATION_SUMMARY.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB644MthlyObligationSummary: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B644_MTHLY_OBLIGATION_SUMMARY program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB644MthlyObligationSummary(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB644MthlyObligationSummary.
  /// </summary>
  public FnB644MthlyObligationSummary(IContext context, Import import,
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
    // ***************************************************************************
    // * The objective of this procedure is to build monthly obligation 
    // summaries.
    // * The only information that is summarized is the outstanding
    // * balance.  This information will be reported on the obligor's activity
    // * statement, so a snapshot of the outstanding balance is taken at the end
    // * of the month.  This outstanding balance serves as the ending balance 
    // for
    // * one month and the beginning balance for the next month.
    // ***************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    UseFnB644Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // **********************************************************
    // Obligor is a subtype of cse_person_account.
    // Client is a subtype of cse_person.
    // **********************************************************
    local.EabFileHandling.Action = "WRITE";
    local.NextCommit.Count =
      local.ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault();

    if (!IsEmpty(local.PpiParameter.ObligorPersonNumber))
    {
      local.ReportNeeded.Flag = "Y";

      if (ReadClientObligor())
      {
        ++local.ObligationsProcessed.Count;

        foreach(var item in ReadObligation())
        {
          if (AsChar(local.ReportNeeded.Flag) == 'Y')
          {
            local.EabConvertNumeric.SendAmount =
              NumberToString(entities.Obligation.SystemGeneratedIdentifier, 15);
              
            UseEabConvertNumeric1();
            local.EabReportSend.RptDetail = "Obligor = " + entities
              .Client.Number + " Obligation Id = " + Substring
              (local.EabConvertNumeric.ReturnNoCommasInNonDecimal,
              EabConvertNumeric2.ReturnNoCommasInNonDecimal_MaxLength, 14, 3) +
              "  " + "  " + "  ";
            UseCabBusinessReport01();
          }

          UseFnB644ProcessSummaries();

          if (IsExitState("OBLIGATION_NOT_ACTIVE"))
          {
            ++local.ObligationsNotYetActive.Count;
            ExitState = "ACO_NN0000_ALL_OK";

            continue;
          }
          else if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else
          {
            goto Test;
          }
        }
      }
      else
      {
        local.EabReportSend.RptDetail = "Obligor not found.  Number = " + (
          local.PpiParameter.ObligorPersonNumber ?? "") + "  " + "  " + "  " + "  ";
          
        UseCabErrorReport();
      }
    }
    else
    {
      local.ReportNeeded.Flag = "N";

      foreach(var item in ReadObligorClient())
      {
        ++local.ObligationsProcessed.Count;

        foreach(var item1 in ReadObligation())
        {
          if (AsChar(local.ReportNeeded.Flag) == 'Y')
          {
            local.EabConvertNumeric.SendAmount =
              NumberToString(entities.Obligation.SystemGeneratedIdentifier, 15);
              
            UseEabConvertNumeric1();
            local.EabReportSend.RptDetail = "Obligor = " + entities
              .Client.Number + " Obligation Id = " + Substring
              (local.EabConvertNumeric.ReturnNoCommasInNonDecimal,
              EabConvertNumeric2.ReturnNoCommasInNonDecimal_MaxLength, 14, 3) +
              "  " + "  " + "  ";
            UseCabBusinessReport01();
          }

          UseFnB644ProcessSummaries();

          if (IsExitState("OBLIGATION_NOT_ACTIVE"))
          {
            ++local.ObligationsNotYetActive.Count;
            ExitState = "ACO_NN0000_ALL_OK";

            continue;
          }
          else if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            if (local.MonthlySummariesCreated.Count >= local.NextCommit.Count)
            {
              local.ProgramCheckpointRestart.RestartInd = "Y";
              local.ProgramCheckpointRestart.RestartInfo =
                entities.Client.Number;
              UseUpdatePgmCheckpointRestart();

              // ***** Call an external that does a DB2 commit using a Cobol 
              // program.
              UseExtToDoACommit();
              local.Date.Text10 = NumberToString(Now().Date.Year, 12, 4) + "-"
                + NumberToString(Now().Date.Month, 14, 2) + "-" + NumberToString
                (Now().Date.Day, 14, 2);
              local.Time.Text8 = NumberToString(Time(Now()).Hours, 14, 2) + ":"
                + NumberToString(Time(Now()).Minutes, 14, 2) + ":" + NumberToString
                (Time(Now()).Seconds, 14, 2);
              local.EabReportSend.RptDetail =
                "Checkpoint taken, obligor number: " + entities
                .Client.Number + " at: " + local.Date.Text10 + "  " + local
                .Time.Text8 + "  after " + NumberToString
                (local.NextCommit.Count, 5, 11) + " updates";
              UseCabControlReport();
              local.NextCommit.Count += local.ProgramCheckpointRestart.
                UpdateFrequencyCount.GetValueOrDefault();
            }
          }
          else
          {
            goto ReadEach;
          }
        }
      }

ReadEach:
      ;
    }

Test:

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.ProgramCheckpointRestart.RestartInd = "N";
      local.ProgramCheckpointRestart.RestartInfo = "";
      UseUpdatePgmCheckpointRestart();
      UseFnB644Closing();
    }
    else
    {
      UseEabExtractExitStateMessage();
      local.EabReportSend.RptDetail = "Person number = " + entities
        .Client.Number + " Message: " + local.ExitStateWorkArea.Message + "  " +
        "  ";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ALL_OK";
      UseFnB644Closing();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
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

  private static void MoveObligation(Obligation source, Obligation target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.PrimarySecondaryCode = source.PrimarySecondaryCode;
    target.OrderTypeCode = source.OrderTypeCode;
    target.DormantInd = source.DormantInd;
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

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
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

  private void UseEabConvertNumeric1()
  {
    var useImport = new EabConvertNumeric1.Import();
    var useExport = new EabConvertNumeric1.Export();

    useImport.EabConvertNumeric.Assign(local.EabConvertNumeric);
    useExport.EabConvertNumeric.Assign(local.EabConvertNumeric);

    Call(EabConvertNumeric1.Execute, useImport, useExport);

    MoveEabConvertNumeric2(useExport.EabConvertNumeric, local.EabConvertNumeric);
      
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.External.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.External.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseFnB644Closing()
  {
    var useImport = new FnB644Closing.Import();
    var useExport = new FnB644Closing.Export();

    useImport.ObligationsProcessed.Count = local.ObligationsProcessed.Count;
    useImport.ObligationsNotActive.Count = local.ObligationsNotYetActive.Count;
    useImport.MonthlySummariesCreated.Count =
      local.MonthlySummariesCreated.Count;

    Call(FnB644Closing.Execute, useImport, useExport);
  }

  private void UseFnB644Housekeeping()
  {
    var useImport = new FnB644Housekeeping.Import();
    var useExport = new FnB644Housekeeping.Export();

    Call(FnB644Housekeeping.Execute, useImport, useExport);

    local.Conversion.Date = useExport.Conversion.Date;
    local.Process.Date = useExport.Process.Date;
    local.ProcessDate.Text10 = useExport.ProcessDate.Text10;
    MoveDateWorkArea(useExport.BeginSummaryPeriodDateWorkArea,
      local.BeginSummaryPeriodDateWorkArea);
    MoveDateWorkArea(useExport.EndSummaryPeriodDateWorkArea,
      local.EndSummaryPeriodDateWorkArea);
    local.BeginSummaryPeriodTextWorkArea.Text10 =
      useExport.BeginSummaryPeriodTextWorkArea.Text10;
    local.EndSummaryPeriodTextWorkArea.Text10 =
      useExport.EndSummaryPeriodTextWorkArea.Text10;
    local.PreviousSummaryPeriod.YearMonth =
      useExport.PreviousSummaryPeriod.YearMonth;
    local.CurrentSummaryPeriod.YearMonth =
      useExport.CurrentSummaryPeriod.YearMonth;
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
    local.PpiParameter.ObligorPersonNumber =
      useExport.PpiParameter.ObligorPersonNumber;
    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
    local.Restart.ObligorPersonNumber = useExport.Restart.ObligorPersonNumber;
    local.LocateCreateMissing.Flag = useExport.CreateMissing.Flag;
  }

  private void UseFnB644ProcessSummaries()
  {
    var useImport = new FnB644ProcessSummaries.Import();
    var useExport = new FnB644ProcessSummaries.Export();

    useImport.ReportNeeded.Flag = local.ReportNeeded.Flag;
    useImport.Conversion.Date = local.Conversion.Date;
    useImport.Obligation.Assign(entities.Obligation);
    useImport.Obligor.Assign(entities.Obligor);
    MoveDateWorkArea(local.EndSummaryPeriodDateWorkArea,
      useImport.EndSummaryPeriod);
    MoveDateWorkArea(local.BeginSummaryPeriodDateWorkArea,
      useImport.BeginSummaryPeriod);
    useImport.PreviousPeriod.YearMonth = local.PreviousSummaryPeriod.YearMonth;
    useImport.CurrentPeriod.YearMonth = local.CurrentSummaryPeriod.YearMonth;
    useImport.SummariesCreated.Count = local.MonthlySummariesCreated.Count;
    useImport.CreateMissing.Flag = local.LocateCreateMissing.Flag;

    Call(FnB644ProcessSummaries.Execute, useImport, useExport);

    MoveObligation(useImport.Obligation, entities.Obligation);
    entities.Obligor.Type1 = useImport.Obligor.Type1;
    local.MonthlySummariesCreated.Count = useExport.SummariesCreated.Count;
  }

  private void UseUpdatePgmCheckpointRestart()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }

  private bool ReadClientObligor()
  {
    entities.Client.Populated = false;
    entities.Obligor.Populated = false;

    return Read("ReadClientObligor",
      (db, command) =>
      {
        db.SetString(
          command, "numb", local.PpiParameter.ObligorPersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Client.Number = db.GetString(reader, 0);
        entities.Obligor.CspNumber = db.GetString(reader, 0);
        entities.Client.DateOfDeath = db.GetNullableDate(reader, 1);
        entities.Obligor.Type1 = db.GetString(reader, 2);
        entities.Client.Populated = true;
        entities.Obligor.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.Obligor.Type1);
      });
  }

  private IEnumerable<bool> ReadObligation()
  {
    System.Diagnostics.Debug.Assert(entities.Obligor.Populated);
    entities.Obligation.Populated = false;

    return ReadEach("ReadObligation",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.Obligor.Type1);
        db.SetString(command, "cspNumber", entities.Obligor.CspNumber);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 4);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 5);
        entities.Obligation.DormantInd = db.GetNullableString(reader, 6);
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.Obligation.OrderTypeCode);

        return true;
      });
  }

  private IEnumerable<bool> ReadObligorClient()
  {
    entities.Client.Populated = false;
    entities.Obligor.Populated = false;

    return ReadEach("ReadObligorClient",
      (db, command) =>
      {
        db.SetString(command, "numb", local.Restart.ObligorPersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Obligor.CspNumber = db.GetString(reader, 0);
        entities.Client.Number = db.GetString(reader, 0);
        entities.Obligor.Type1 = db.GetString(reader, 1);
        entities.Client.DateOfDeath = db.GetNullableDate(reader, 2);
        entities.Client.Populated = true;
        entities.Obligor.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.Obligor.Type1);

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
    /// A value of LocateCreateMissing.
    /// </summary>
    [JsonPropertyName("locateCreateMissing")]
    public Common LocateCreateMissing
    {
      get => locateCreateMissing ??= new();
      set => locateCreateMissing = value;
    }

    /// <summary>
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
    }

    /// <summary>
    /// A value of Date.
    /// </summary>
    [JsonPropertyName("date")]
    public TextWorkArea Date
    {
      get => date ??= new();
      set => date = value;
    }

    /// <summary>
    /// A value of Time.
    /// </summary>
    [JsonPropertyName("time")]
    public TextWorkArea Time
    {
      get => time ??= new();
      set => time = value;
    }

    /// <summary>
    /// A value of MonthlySummariesCreated.
    /// </summary>
    [JsonPropertyName("monthlySummariesCreated")]
    public Common MonthlySummariesCreated
    {
      get => monthlySummariesCreated ??= new();
      set => monthlySummariesCreated = value;
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
    /// A value of EabConvertNumeric.
    /// </summary>
    [JsonPropertyName("eabConvertNumeric")]
    public EabConvertNumeric2 EabConvertNumeric
    {
      get => eabConvertNumeric ??= new();
      set => eabConvertNumeric = value;
    }

    /// <summary>
    /// A value of ObligationsProcessed.
    /// </summary>
    [JsonPropertyName("obligationsProcessed")]
    public Common ObligationsProcessed
    {
      get => obligationsProcessed ??= new();
      set => obligationsProcessed = value;
    }

    /// <summary>
    /// A value of ObligationsNotYetActive.
    /// </summary>
    [JsonPropertyName("obligationsNotYetActive")]
    public Common ObligationsNotYetActive
    {
      get => obligationsNotYetActive ??= new();
      set => obligationsNotYetActive = value;
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
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    /// <summary>
    /// A value of ProcessDate.
    /// </summary>
    [JsonPropertyName("processDate")]
    public TextWorkArea ProcessDate
    {
      get => processDate ??= new();
      set => processDate = value;
    }

    /// <summary>
    /// A value of CurrentSummaryPeriod.
    /// </summary>
    [JsonPropertyName("currentSummaryPeriod")]
    public MonthlyObligorSummary CurrentSummaryPeriod
    {
      get => currentSummaryPeriod ??= new();
      set => currentSummaryPeriod = value;
    }

    /// <summary>
    /// A value of PreviousSummaryPeriod.
    /// </summary>
    [JsonPropertyName("previousSummaryPeriod")]
    public MonthlyObligorSummary PreviousSummaryPeriod
    {
      get => previousSummaryPeriod ??= new();
      set => previousSummaryPeriod = value;
    }

    /// <summary>
    /// A value of EndSummaryPeriodTextWorkArea.
    /// </summary>
    [JsonPropertyName("endSummaryPeriodTextWorkArea")]
    public TextWorkArea EndSummaryPeriodTextWorkArea
    {
      get => endSummaryPeriodTextWorkArea ??= new();
      set => endSummaryPeriodTextWorkArea = value;
    }

    /// <summary>
    /// A value of BeginSummaryPeriodTextWorkArea.
    /// </summary>
    [JsonPropertyName("beginSummaryPeriodTextWorkArea")]
    public TextWorkArea BeginSummaryPeriodTextWorkArea
    {
      get => beginSummaryPeriodTextWorkArea ??= new();
      set => beginSummaryPeriodTextWorkArea = value;
    }

    /// <summary>
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public CashReceiptDetail Restart
    {
      get => restart ??= new();
      set => restart = value;
    }

    /// <summary>
    /// A value of PpiParameter.
    /// </summary>
    [JsonPropertyName("ppiParameter")]
    public CashReceiptDetail PpiParameter
    {
      get => ppiParameter ??= new();
      set => ppiParameter = value;
    }

    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
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
    /// A value of Process.
    /// </summary>
    [JsonPropertyName("process")]
    public DateWorkArea Process
    {
      get => process ??= new();
      set => process = value;
    }

    /// <summary>
    /// A value of BeginSummaryPeriodDateWorkArea.
    /// </summary>
    [JsonPropertyName("beginSummaryPeriodDateWorkArea")]
    public DateWorkArea BeginSummaryPeriodDateWorkArea
    {
      get => beginSummaryPeriodDateWorkArea ??= new();
      set => beginSummaryPeriodDateWorkArea = value;
    }

    /// <summary>
    /// A value of EndSummaryPeriodDateWorkArea.
    /// </summary>
    [JsonPropertyName("endSummaryPeriodDateWorkArea")]
    public DateWorkArea EndSummaryPeriodDateWorkArea
    {
      get => endSummaryPeriodDateWorkArea ??= new();
      set => endSummaryPeriodDateWorkArea = value;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    /// <summary>
    /// A value of NextCommit.
    /// </summary>
    [JsonPropertyName("nextCommit")]
    public Common NextCommit
    {
      get => nextCommit ??= new();
      set => nextCommit = value;
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

    private Common locateCreateMissing;
    private ExitStateWorkArea exitStateWorkArea;
    private TextWorkArea date;
    private TextWorkArea time;
    private Common monthlySummariesCreated;
    private Common reportNeeded;
    private EabConvertNumeric2 eabConvertNumeric;
    private Common obligationsProcessed;
    private Common obligationsNotYetActive;
    private DateWorkArea conversion;
    private External external;
    private TextWorkArea processDate;
    private MonthlyObligorSummary currentSummaryPeriod;
    private MonthlyObligorSummary previousSummaryPeriod;
    private TextWorkArea endSummaryPeriodTextWorkArea;
    private TextWorkArea beginSummaryPeriodTextWorkArea;
    private CashReceiptDetail restart;
    private CashReceiptDetail ppiParameter;
    private ProgramProcessingInfo programProcessingInfo;
    private DateWorkArea max;
    private DateWorkArea process;
    private DateWorkArea beginSummaryPeriodDateWorkArea;
    private DateWorkArea endSummaryPeriodDateWorkArea;
    private EabFileHandling eabFileHandling;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Common nextCommit;
    private EabReportSend eabReportSend;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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

    private CsePerson client;
    private CsePersonAccount obligor;
    private Obligation obligation;
  }
#endregion
}
