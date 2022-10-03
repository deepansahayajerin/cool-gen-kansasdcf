// Program: OE_B453_PROCESS_KDWP_FILE, ID: 371320623, model: 746.
// Short name: SWEE453B
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B453_PROCESS_KDWP_FILE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class OeB453ProcessKdwpFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B453_PROCESS_KDWP_FILE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB453ProcessKdwpFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB453ProcessKdwpFile.
  /// </summary>
  public OeB453ProcessKdwpFile(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***********************************************************************************************
    // DATE		Developer	Description
    // 02/19/2007      DDupree   	Initial Creation - WR280421
    // 08/23/2010      GVandy		CQ19415 - Add parameters signifying 1) the number
    // of days in the past
    // 				that an obligation must have been created before its debts are
    // 				included in the arrears calculations  2) the number of days in the
    // 				past that the obligation began accruing (or the debt due
    // 				date for non accruing obligations).
    // ***********************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    UseOeB453Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    do
    {
      local.Postion.Text1 =
        Substring(local.ProgramProcessingInfo.ParameterList,
        local.CurrentPosition.Count, 1);

      if (AsChar(local.Postion.Text1) == ',')
      {
        ++local.FieldNumber.Count;
        local.WorkArea.Text15 = "";

        switch(local.FieldNumber.Count)
        {
          case 1:
            if (local.Current.Count == 1)
            {
              local.MiniumTarget.TotalCurrency = 0;
            }
            else
            {
              local.WorkArea.Text15 =
                Substring(local.ProgramProcessingInfo.ParameterList,
                local.Start.Count, local.Current.Count - 1);
              local.MiniumTarget.TotalCurrency =
                StringToNumber(local.WorkArea.Text15);
            }

            local.Start.Count = local.CurrentPosition.Count + 1;
            local.Current.Count = 0;

            break;
          case 2:
            if (local.Current.Count == 1)
            {
              local.NumberOfDays.Count = 0;
            }
            else
            {
              local.WorkArea.Text15 =
                Substring(local.ProgramProcessingInfo.ParameterList,
                local.Start.Count, local.Current.Count - 1);
              local.NumberOfDays.Count =
                (int)StringToNumber(local.WorkArea.Text15);
            }

            local.Start.Count = local.CurrentPosition.Count + 1;
            local.Current.Count = 0;

            break;
          case 3:
            if (local.Current.Count == 1)
            {
              local.StartDate.Date = Now().Date;
            }
            else
            {
              local.WorkArea.Text15 =
                Substring(local.ProgramProcessingInfo.ParameterList,
                local.Start.Count, local.Current.Count - 1);
              local.StartDate.Date = StringToDate(local.WorkArea.Text15);
            }

            local.Start.Count = local.CurrentPosition.Count + 1;
            local.Current.Count = 0;

            break;
          case 4:
            if (local.Current.Count == 1)
            {
              local.MiniumPayment.TotalCurrency = 0;
            }
            else
            {
              local.WorkArea.Text15 =
                Substring(local.ProgramProcessingInfo.ParameterList,
                local.Start.Count, local.Current.Count - 1);
              local.MiniumPayment.TotalCurrency =
                StringToNumber(local.WorkArea.Text15);
            }

            local.Start.Count = local.CurrentPosition.Count + 1;
            local.Current.Count = 0;

            break;
          case 5:
            if (local.Current.Count == 1)
            {
              local.IncludeArrearsOnly.Flag = "N";
            }
            else
            {
              local.IncludeArrearsOnly.Flag =
                Substring(local.ProgramProcessingInfo.ParameterList,
                local.Start.Count, local.Current.Count - 1);
            }

            local.Start.Count = local.CurrentPosition.Count + 1;
            local.Current.Count = 0;

            break;
          case 6:
            // -- 08/23/2010 CQ19415 - Add support for parameter indicating the 
            // number of days in the past an obligation must have been created.
            if (local.Current.Count == 1)
            {
              local.NumDaysSinceObCreatedCommon.Count = 0;
            }
            else
            {
              local.WorkArea.Text15 =
                Substring(local.ProgramProcessingInfo.ParameterList,
                local.Start.Count, local.Current.Count - 1);
              local.NumDaysSinceObCreatedCommon.Count =
                (int)StringToNumber(local.WorkArea.Text15);
            }

            local.Start.Count = local.CurrentPosition.Count + 1;
            local.Current.Count = 0;

            break;
          case 7:
            // -- 08/23/2010 CQ19415 - Add support for parameter indicating the 
            // number of days in the past that the obligation began accruing (or
            // the debt due date for non accruing obligations).
            if (local.Current.Count == 1)
            {
              local.NumDaysAccruingOrDueCommon.Count = 0;
            }
            else
            {
              local.WorkArea.Text15 =
                Substring(local.ProgramProcessingInfo.ParameterList,
                local.Start.Count, local.Current.Count - 1);
              local.NumDaysAccruingOrDueCommon.Count =
                (int)StringToNumber(local.WorkArea.Text15);
            }

            local.Start.Count = local.CurrentPosition.Count + 1;
            local.Current.Count = 0;

            break;
          case 8:
            if (local.Current.Count == 1)
            {
              local.NumMonthsIwoPeriod.Count = 12;
            }
            else
            {
              local.WorkArea.Text15 =
                Substring(local.ProgramProcessingInfo.ParameterList,
                local.Start.Count, local.Current.Count - 1);
              local.NumMonthsIwoPeriod.Count =
                (int)StringToNumber(local.WorkArea.Text15);
            }

            local.Start.Count = local.CurrentPosition.Count + 1;
            local.Current.Count = 0;

            break;
          default:
            break;
        }
      }
      else if (IsEmpty(local.Postion.Text1))
      {
        break;
      }

      ++local.CurrentPosition.Count;
      ++local.Current.Count;
    }
    while(!Equal(global.Command, "COMMAND"));

    UseOeProcessLicenseSanctions();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseOeB453Close();
      local.PassArea.FileInstruction = "CLOSE";
      UseOeEabRequestToKdwp();

      if (!Equal(local.PassArea.TextReturnCode, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      local.PassArea.FileInstruction = "CLOSE";
      UseOeEabSsnAndDobMissRpt();

      if (!Equal(local.PassArea.TextReturnCode, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      local.ProgramCheckpointRestart.RestartInd = "N";
      local.ProgramCheckpointRestart.ProgramName =
        local.ProgramProcessingInfo.Name;
      local.ProgramCheckpointRestart.RestartInfo = "";
      UseUpdatePgmCheckpointRestart();
      local.StartDate.Date = AddDays(local.StartDate.Date, 7);
      UseCabDate2TextWithHyphens();
      local.WorkArea.Text15 =
        NumberToString((long)local.MiniumTarget.TotalCurrency, 15);
      local.MinTarget.Text5 = Substring(local.WorkArea.Text15, 11, 5);
      local.WorkArea.Text15 =
        NumberToString((long)local.MiniumPayment.TotalCurrency, 15);
      local.MinPayment.Text5 = Substring(local.WorkArea.Text15, 11, 5);
      local.WorkArea.Text15 = NumberToString(local.NumberOfDays.Count, 15);
      local.NumOfDays.Text3 = Substring(local.WorkArea.Text15, 13, 3);
      local.WorkArea.Text15 =
        NumberToString(local.NumDaysSinceObCreatedCommon.Count, 15);
      local.NumDaysSinceObCreatedWorkArea.Text3 =
        Substring(local.WorkArea.Text15, 13, 3);
      local.WorkArea.Text15 =
        NumberToString(local.NumDaysAccruingOrDueCommon.Count, 15);
      local.NumDaysAccruingOrDueWorkArea.Text3 =
        Substring(local.WorkArea.Text15, 13, 3);
      local.WorkArea.Text15 =
        NumberToString(local.NumMonthsIwoPeriod.Count, 15);
      local.NumOfIwoMonths.Text3 = Substring(local.WorkArea.Text15, 13, 3);
      local.ProgramProcessingInfo.ParameterList = local.MinTarget.Text5 + ","
        + local.NumOfDays.Text3 + "," + local.UpdateProgrProces.Text10 + "," + local
        .MinPayment.Text5 + "," + local.IncludeArrearsOnly.Flag + "," + local
        .NumDaysSinceObCreatedWorkArea.Text3 + "," + local
        .NumDaysAccruingOrDueWorkArea.Text3 + ",";
      local.ProgramProcessingInfo.ParameterList =
        TrimEnd(local.ProgramProcessingInfo.ParameterList) + local
        .NumOfIwoMonths.Text3 + ",";
      UseUpdateProgramProcessingInfo();
    }
    else
    {
      UseEabExtractExitStateMessage();
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Program abended because: " + local
        .ExitStateWorkArea.Message;
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      UseOeB453Close();
      local.PassArea.FileInstruction = "CLOSE";
      UseOeEabRequestToKdwp();
      local.PassArea.FileInstruction = "CLOSE";
      UseOeEabSsnAndDobMissRpt();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Count = source.Count;
    target.TotalCurrency = source.TotalCurrency;
  }

  private static void MoveExternal(External source, External target)
  {
    target.FileInstruction = source.FileInstruction;
    target.TextReturnCode = source.TextReturnCode;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
    target.ParameterList = source.ParameterList;
  }

  private void UseCabDate2TextWithHyphens()
  {
    var useImport = new CabDate2TextWithHyphens.Import();
    var useExport = new CabDate2TextWithHyphens.Export();

    useImport.DateWorkArea.Date = local.StartDate.Date;

    Call(CabDate2TextWithHyphens.Execute, useImport, useExport);

    local.UpdateProgrProces.Text10 = useExport.TextWorkArea.Text10;
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

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseOeB453Close()
  {
    var useImport = new OeB453Close.Import();
    var useExport = new OeB453Close.Export();

    useImport.NumberOfRecordsRead.Count = local.NumberOfRecordsRead.Count;
    useImport.TotalNumRecsAdded.Count = local.TotalNumRecsAdded.Count;
    MoveCommon(local.DollarAmtDebtsOwed, useImport.DollarAmtDebtsOwed);
    useImport.NumErrorRecords.Count = local.NumErrorRecords.Count;

    Call(OeB453Close.Execute, useImport, useExport);
  }

  private void UseOeB453Housekeeping()
  {
    var useImport = new OeB453Housekeeping.Import();
    var useExport = new OeB453Housekeeping.Export();

    Call(OeB453Housekeeping.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseOeEabRequestToKdwp()
  {
    var useImport = new OeEabRequestToKdwp.Import();
    var useExport = new OeEabRequestToKdwp.Export();

    useImport.External.FileInstruction = local.PassArea.FileInstruction;
    MoveExternal(local.PassArea, useExport.External);

    Call(OeEabRequestToKdwp.Execute, useImport, useExport);

    MoveExternal(useExport.External, local.PassArea);
  }

  private void UseOeEabSsnAndDobMissRpt()
  {
    var useImport = new OeEabSsnAndDobMissRpt.Import();
    var useExport = new OeEabSsnAndDobMissRpt.Export();

    useImport.External.FileInstruction = local.PassArea.FileInstruction;
    MoveExternal(local.PassArea, useExport.External);

    Call(OeEabSsnAndDobMissRpt.Execute, useImport, useExport);

    MoveExternal(useExport.External, local.PassArea);
  }

  private void UseOeProcessLicenseSanctions()
  {
    var useImport = new OeProcessLicenseSanctions.Import();
    var useExport = new OeProcessLicenseSanctions.Export();

    useImport.ProgramProcessingInfo.Assign(local.ProgramProcessingInfo);
    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(OeProcessLicenseSanctions.Execute, useImport, useExport);

    local.DollarAmtDebtsOwed.TotalCurrency =
      useExport.TotalAmtDebtOwed.TotalCurrency;
    local.NumErrorRecords.Count = useExport.TotalNumErrorRecords.Count;
    local.TotalNumRecsAdded.Count = useExport.TotalNumRecsAdded.Count;
    local.NumberOfRecordsRead.Count = useExport.NumberOfRecordsRead.Count;
  }

  private void UseUpdatePgmCheckpointRestart()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }

  private void UseUpdateProgramProcessingInfo()
  {
    var useImport = new UpdateProgramProcessingInfo.Import();
    var useExport = new UpdateProgramProcessingInfo.Export();

    MoveProgramProcessingInfo(local.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);

    Call(UpdateProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of RecordProcessed.
    /// </summary>
    [JsonPropertyName("recordProcessed")]
    public Common RecordProcessed
    {
      get => recordProcessed ??= new();
      set => recordProcessed = value;
    }

    private Common recordProcessed;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of RecordProcessed.
    /// </summary>
    [JsonPropertyName("recordProcessed")]
    public Common RecordProcessed
    {
      get => recordProcessed ??= new();
      set => recordProcessed = value;
    }

    private Common recordProcessed;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of NumOfIwoMonths.
    /// </summary>
    [JsonPropertyName("numOfIwoMonths")]
    public WorkArea NumOfIwoMonths
    {
      get => numOfIwoMonths ??= new();
      set => numOfIwoMonths = value;
    }

    /// <summary>
    /// A value of NumDaysAccruingOrDueWorkArea.
    /// </summary>
    [JsonPropertyName("numDaysAccruingOrDueWorkArea")]
    public WorkArea NumDaysAccruingOrDueWorkArea
    {
      get => numDaysAccruingOrDueWorkArea ??= new();
      set => numDaysAccruingOrDueWorkArea = value;
    }

    /// <summary>
    /// A value of NumDaysAccruingOrDueCommon.
    /// </summary>
    [JsonPropertyName("numDaysAccruingOrDueCommon")]
    public Common NumDaysAccruingOrDueCommon
    {
      get => numDaysAccruingOrDueCommon ??= new();
      set => numDaysAccruingOrDueCommon = value;
    }

    /// <summary>
    /// A value of NumDaysSinceObCreatedWorkArea.
    /// </summary>
    [JsonPropertyName("numDaysSinceObCreatedWorkArea")]
    public WorkArea NumDaysSinceObCreatedWorkArea
    {
      get => numDaysSinceObCreatedWorkArea ??= new();
      set => numDaysSinceObCreatedWorkArea = value;
    }

    /// <summary>
    /// A value of NumDaysSinceObCreatedCommon.
    /// </summary>
    [JsonPropertyName("numDaysSinceObCreatedCommon")]
    public Common NumDaysSinceObCreatedCommon
    {
      get => numDaysSinceObCreatedCommon ??= new();
      set => numDaysSinceObCreatedCommon = value;
    }

    /// <summary>
    /// A value of NumErrorRecords.
    /// </summary>
    [JsonPropertyName("numErrorRecords")]
    public Common NumErrorRecords
    {
      get => numErrorRecords ??= new();
      set => numErrorRecords = value;
    }

    /// <summary>
    /// A value of DollarAmtDebtsOwed.
    /// </summary>
    [JsonPropertyName("dollarAmtDebtsOwed")]
    public Common DollarAmtDebtsOwed
    {
      get => dollarAmtDebtsOwed ??= new();
      set => dollarAmtDebtsOwed = value;
    }

    /// <summary>
    /// A value of TotalNumRecsAdded.
    /// </summary>
    [JsonPropertyName("totalNumRecsAdded")]
    public Common TotalNumRecsAdded
    {
      get => totalNumRecsAdded ??= new();
      set => totalNumRecsAdded = value;
    }

    /// <summary>
    /// A value of NumberOfRecordsRead.
    /// </summary>
    [JsonPropertyName("numberOfRecordsRead")]
    public Common NumberOfRecordsRead
    {
      get => numberOfRecordsRead ??= new();
      set => numberOfRecordsRead = value;
    }

    /// <summary>
    /// A value of MinPayment.
    /// </summary>
    [JsonPropertyName("minPayment")]
    public WorkArea MinPayment
    {
      get => minPayment ??= new();
      set => minPayment = value;
    }

    /// <summary>
    /// A value of MinTarget.
    /// </summary>
    [JsonPropertyName("minTarget")]
    public WorkArea MinTarget
    {
      get => minTarget ??= new();
      set => minTarget = value;
    }

    /// <summary>
    /// A value of NumOfDays.
    /// </summary>
    [JsonPropertyName("numOfDays")]
    public WorkArea NumOfDays
    {
      get => numOfDays ??= new();
      set => numOfDays = value;
    }

    /// <summary>
    /// A value of UpdateProgrProces.
    /// </summary>
    [JsonPropertyName("updateProgrProces")]
    public TextWorkArea UpdateProgrProces
    {
      get => updateProgrProces ??= new();
      set => updateProgrProces = value;
    }

    /// <summary>
    /// A value of IncludeArrearsOnly.
    /// </summary>
    [JsonPropertyName("includeArrearsOnly")]
    public Common IncludeArrearsOnly
    {
      get => includeArrearsOnly ??= new();
      set => includeArrearsOnly = value;
    }

    /// <summary>
    /// A value of MiniumPayment.
    /// </summary>
    [JsonPropertyName("miniumPayment")]
    public Common MiniumPayment
    {
      get => miniumPayment ??= new();
      set => miniumPayment = value;
    }

    /// <summary>
    /// A value of StartDate.
    /// </summary>
    [JsonPropertyName("startDate")]
    public DateWorkArea StartDate
    {
      get => startDate ??= new();
      set => startDate = value;
    }

    /// <summary>
    /// A value of NumberOfDays.
    /// </summary>
    [JsonPropertyName("numberOfDays")]
    public Common NumberOfDays
    {
      get => numberOfDays ??= new();
      set => numberOfDays = value;
    }

    /// <summary>
    /// A value of WorkArea.
    /// </summary>
    [JsonPropertyName("workArea")]
    public WorkArea WorkArea
    {
      get => workArea ??= new();
      set => workArea = value;
    }

    /// <summary>
    /// A value of MiniumTarget.
    /// </summary>
    [JsonPropertyName("miniumTarget")]
    public Common MiniumTarget
    {
      get => miniumTarget ??= new();
      set => miniumTarget = value;
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
    /// A value of PassArea.
    /// </summary>
    [JsonPropertyName("passArea")]
    public External PassArea
    {
      get => passArea ??= new();
      set => passArea = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
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

    /// <summary>
    /// A value of Postion.
    /// </summary>
    [JsonPropertyName("postion")]
    public TextWorkArea Postion
    {
      get => postion ??= new();
      set => postion = value;
    }

    /// <summary>
    /// A value of CurrentPosition.
    /// </summary>
    [JsonPropertyName("currentPosition")]
    public Common CurrentPosition
    {
      get => currentPosition ??= new();
      set => currentPosition = value;
    }

    /// <summary>
    /// A value of FieldNumber.
    /// </summary>
    [JsonPropertyName("fieldNumber")]
    public Common FieldNumber
    {
      get => fieldNumber ??= new();
      set => fieldNumber = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public Common Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    public Common Start
    {
      get => start ??= new();
      set => start = value;
    }

    /// <summary>
    /// A value of NumMonthsIwoPeriod.
    /// </summary>
    [JsonPropertyName("numMonthsIwoPeriod")]
    public Common NumMonthsIwoPeriod
    {
      get => numMonthsIwoPeriod ??= new();
      set => numMonthsIwoPeriod = value;
    }

    private WorkArea numOfIwoMonths;
    private WorkArea numDaysAccruingOrDueWorkArea;
    private Common numDaysAccruingOrDueCommon;
    private WorkArea numDaysSinceObCreatedWorkArea;
    private Common numDaysSinceObCreatedCommon;
    private Common numErrorRecords;
    private Common dollarAmtDebtsOwed;
    private Common totalNumRecsAdded;
    private Common numberOfRecordsRead;
    private WorkArea minPayment;
    private WorkArea minTarget;
    private WorkArea numOfDays;
    private TextWorkArea updateProgrProces;
    private Common includeArrearsOnly;
    private Common miniumPayment;
    private DateWorkArea startDate;
    private Common numberOfDays;
    private WorkArea workArea;
    private Common miniumTarget;
    private ExitStateWorkArea exitStateWorkArea;
    private External passArea;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private TextWorkArea postion;
    private Common currentPosition;
    private Common fieldNumber;
    private Common current;
    private Common start;
    private Common numMonthsIwoPeriod;
  }
#endregion
}
