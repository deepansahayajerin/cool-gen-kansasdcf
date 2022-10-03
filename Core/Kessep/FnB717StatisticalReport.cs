// Program: FN_B717_STATISTICAL_REPORT, ID: 373365809, model: 746.
// Short name: SWEF717B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B717_STATISTICAL_REPORT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB717StatisticalReport: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B717_STATISTICAL_REPORT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB717StatisticalReport(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB717StatisticalReport.
  /// </summary>
  public FnB717StatisticalReport(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -----------------------------------------------------------
    // Initial Version -  01/2002.
    // -----------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.ProgramProcessingInfo.Name = global.UserId;
    UseFnB717BatchInitialization();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.EabFileHandling.Action = "WRITE";

    if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      UseFnOcse157GetRestartLineNbr();
    }

    if (!Lt("01", local.Restart.LineNumber) && local
      .From.LineNumber.GetValueOrDefault() <= 1 && local
      .To.LineNumber.GetValueOrDefault() >= 1)
    {
      UseFnB717Line1();

      if (AsChar(local.AbortProgram.Flag) == 'Y')
      {
        return;
      }
    }

    if (!Lt("03", local.Restart.LineNumber) && local
      .From.LineNumber.GetValueOrDefault() <= 3 && local
      .To.LineNumber.GetValueOrDefault() >= 3)
    {
      UseFnB717Line3();

      if (AsChar(local.AbortProgram.Flag) == 'Y')
      {
        return;
      }
    }

    if (!Lt("05", local.Restart.LineNumber) && local
      .From.LineNumber.GetValueOrDefault() <= 5 && local
      .To.LineNumber.GetValueOrDefault() >= 5)
    {
      UseFnB717Line5();

      if (AsChar(local.AbortProgram.Flag) == 'Y')
      {
        return;
      }
    }

    if (!Lt("13", local.Restart.LineNumber) && local
      .From.LineNumber.GetValueOrDefault() <= 13 && local
      .To.LineNumber.GetValueOrDefault() >= 13)
    {
      UseFnB717Line13();

      if (AsChar(local.AbortProgram.Flag) == 'Y')
      {
        return;
      }
    }

    if (!Lt("15", local.Restart.LineNumber) && local
      .From.LineNumber.GetValueOrDefault() <= 15 && local
      .To.LineNumber.GetValueOrDefault() >= 15)
    {
      UseFnB717Line15();

      if (AsChar(local.AbortProgram.Flag) == 'Y')
      {
        return;
      }
    }

    if (!Lt("17", local.Restart.LineNumber) && local
      .From.LineNumber.GetValueOrDefault() <= 17 && local
      .To.LineNumber.GetValueOrDefault() >= 17)
    {
      UseFnB717Line17();

      if (AsChar(local.AbortProgram.Flag) == 'Y')
      {
        return;
      }
    }

    if (!Lt("18", local.Restart.LineNumber) && local
      .From.LineNumber.GetValueOrDefault() <= 18 && local
      .To.LineNumber.GetValueOrDefault() >= 18)
    {
      UseFnB717Line18();

      if (AsChar(local.AbortProgram.Flag) == 'Y')
      {
        return;
      }
    }

    if (!Lt("27", local.Restart.LineNumber) && local
      .From.LineNumber.GetValueOrDefault() <= 27 && local
      .To.LineNumber.GetValueOrDefault() >= 27)
    {
      UseFnB717Line27();

      if (AsChar(local.AbortProgram.Flag) == 'Y')
      {
        return;
      }
    }

    if (!Lt("28", local.Restart.LineNumber) && local
      .From.LineNumber.GetValueOrDefault() <= 28 && local
      .To.LineNumber.GetValueOrDefault() >= 28)
    {
      UseFnB717Driver28Thru34();

      if (AsChar(local.AbortProgram.Flag) == 'Y')
      {
        return;
      }
    }

    if (!Lt("31", local.Restart.LineNumber) && local
      .From.LineNumber.GetValueOrDefault() <= 31 && local
      .To.LineNumber.GetValueOrDefault() >= 31)
    {
      UseFnB717Line3132();

      if (AsChar(local.AbortProgram.Flag) == 'Y')
      {
        return;
      }
    }

    if (!Lt("33", local.Restart.LineNumber) && local
      .From.LineNumber.GetValueOrDefault() <= 33 && local
      .To.LineNumber.GetValueOrDefault() >= 33)
    {
      UseFnB717Line33Update();

      if (AsChar(local.AbortProgram.Flag) == 'Y')
      {
        return;
      }
    }

    if (!Lt("35", local.Restart.LineNumber) && local
      .From.LineNumber.GetValueOrDefault() <= 35 && local
      .To.LineNumber.GetValueOrDefault() >= 35)
    {
      UseFnB717Line27Update();

      if (AsChar(local.AbortProgram.Flag) == 'Y')
      {
      }
    }
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
  }

  private static void MoveGroup1(FnB717Driver28Thru34.Import.GroupGroup source,
    Local.GroupGroup target)
  {
    target.StatsReport.Assign(source.StatsReport);
  }

  private static void MoveGroup2(FnB717Line1.Import.GroupGroup source,
    Local.GroupGroup target)
  {
    target.StatsReport.Assign(source.StatsReport);
  }

  private static void MoveGroup3(FnB717Line15.Import.GroupGroup source,
    Local.GroupGroup target)
  {
    target.StatsReport.Assign(source.StatsReport);
  }

  private static void MoveGroup4(Local.GroupGroup source,
    FnB717Driver28Thru34.Import.GroupGroup target)
  {
    target.StatsReport.Assign(source.StatsReport);
  }

  private static void MoveGroup5(Local.GroupGroup source,
    FnB717Line1.Import.GroupGroup target)
  {
    target.StatsReport.Assign(source.StatsReport);
  }

  private static void MoveGroup6(Local.GroupGroup source,
    FnB717Line15.Import.GroupGroup target)
  {
    target.StatsReport.Assign(source.StatsReport);
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.RestartInd = source.RestartInd;
    target.RestartInfo = source.RestartInfo;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private static void MoveStatsReport1(StatsReport source, StatsReport target)
  {
    target.YearMonth = source.YearMonth;
    target.FirstRunNumber = source.FirstRunNumber;
  }

  private static void MoveStatsReport2(StatsReport source, StatsReport target)
  {
    target.LineNumber = source.LineNumber;
    target.OfficeId = source.OfficeId;
  }

  private void UseFnB717BatchInitialization()
  {
    var useImport = new FnB717BatchInitialization.Import();
    var useExport = new FnB717BatchInitialization.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(FnB717BatchInitialization.Execute, useImport, useExport);

    MoveStatsReport1(useExport.StatsReport, local.StatsReport);
    MoveStatsReport2(useExport.To, local.To);
    MoveStatsReport2(useExport.From, local.From);
    MoveProgramProcessingInfo(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
    local.TestRun.Flag = useExport.TestRunInd.Flag;
    local.DisplayInd.Flag = useExport.DisplayInd.Flag;
    MoveDateWorkArea(useExport.ReportStartDate, local.ReportStartDate);
    MoveDateWorkArea(useExport.ReportEndDate, local.ReportEndDate);
  }

  private void UseFnB717Driver28Thru34()
  {
    var useImport = new FnB717Driver28Thru34.Import();
    var useExport = new FnB717Driver28Thru34.Export();

    local.Group.CopyTo(useImport.Group, MoveGroup4);
    MoveStatsReport1(local.StatsReport, useImport.StatsReport);
    MoveStatsReport2(local.To, useImport.To);
    MoveStatsReport2(local.From, useImport.From);
    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);
    useImport.DisplayInd.Flag = local.DisplayInd.Flag;
    MoveDateWorkArea(local.ReportStartDate, useImport.ReportStartDate);
    MoveDateWorkArea(local.ReportEndDate, useImport.ReportEndDate);

    Call(FnB717Driver28Thru34.Execute, useImport, useExport);

    useImport.Group.CopyTo(local.Group, MoveGroup1);
    local.AbortProgram.Flag = useExport.Abort.Flag;
  }

  private void UseFnB717Line1()
  {
    var useImport = new FnB717Line1.Import();
    var useExport = new FnB717Line1.Export();

    local.Group.CopyTo(useImport.Group, MoveGroup5);
    MoveStatsReport1(local.StatsReport, useImport.StatsReport);
    MoveStatsReport2(local.To, useImport.To);
    MoveStatsReport2(local.From, useImport.From);
    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);
    useImport.DisplayInd.Flag = local.DisplayInd.Flag;
    MoveDateWorkArea(local.ReportStartDate, useImport.ReportStartDate);
    MoveDateWorkArea(local.ReportEndDate, useImport.ReportEndDate);

    Call(FnB717Line1.Execute, useImport, useExport);

    useImport.Group.CopyTo(local.Group, MoveGroup2);
    local.AbortProgram.Flag = useExport.Abort.Flag;
  }

  private void UseFnB717Line13()
  {
    var useImport = new FnB717Line13.Import();
    var useExport = new FnB717Line13.Export();

    MoveStatsReport1(local.StatsReport, useImport.StatsReport);
    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(FnB717Line13.Execute, useImport, useExport);

    local.AbortProgram.Flag = useExport.Abort.Flag;
  }

  private void UseFnB717Line15()
  {
    var useImport = new FnB717Line15.Import();
    var useExport = new FnB717Line15.Export();

    local.Group.CopyTo(useImport.Group, MoveGroup6);
    MoveStatsReport1(local.StatsReport, useImport.StatsReport);
    useImport.To.OfficeId = local.To.OfficeId;
    useImport.From.OfficeId = local.From.OfficeId;
    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);
    useImport.DisplayInd.Flag = local.DisplayInd.Flag;
    MoveDateWorkArea(local.ReportStartDate, useImport.ReportStartDate);
    MoveDateWorkArea(local.ReportEndDate, useImport.ReportEndDate);

    Call(FnB717Line15.Execute, useImport, useExport);

    useImport.Group.CopyTo(local.Group, MoveGroup3);
    local.AbortProgram.Flag = useExport.Abort.Flag;
  }

  private void UseFnB717Line17()
  {
    var useImport = new FnB717Line17.Import();
    var useExport = new FnB717Line17.Export();

    MoveStatsReport1(local.StatsReport, useImport.StatsReport);
    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);
    MoveDateWorkArea(local.ReportStartDate, useImport.ReportStartDate);
    MoveDateWorkArea(local.ReportEndDate, useImport.ReportEndDate);

    Call(FnB717Line17.Execute, useImport, useExport);

    local.AbortProgram.Flag = useExport.Abort.Flag;
  }

  private void UseFnB717Line18()
  {
    var useImport = new FnB717Line18.Import();
    var useExport = new FnB717Line18.Export();

    MoveStatsReport1(local.StatsReport, useImport.StatsReport);
    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);
    MoveDateWorkArea(local.ReportStartDate, useImport.ReportStartDate);
    MoveDateWorkArea(local.ReportEndDate, useImport.ReportEndDate);

    Call(FnB717Line18.Execute, useImport, useExport);

    local.AbortProgram.Flag = useExport.Abort.Flag;
  }

  private void UseFnB717Line27()
  {
    var useImport = new FnB717Line27.Import();
    var useExport = new FnB717Line27.Export();

    MoveStatsReport1(local.StatsReport, useImport.StatsReport);
    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);
    useImport.DisplayInd.Flag = local.DisplayInd.Flag;
    MoveDateWorkArea(local.ReportStartDate, useImport.ReportStartDate);
    MoveDateWorkArea(local.ReportEndDate, useImport.ReportEndDate);

    Call(FnB717Line27.Execute, useImport, useExport);

    local.AbortProgram.Flag = useExport.Abort.Flag;
  }

  private void UseFnB717Line27Update()
  {
    var useImport = new FnB717Line27Update.Import();
    var useExport = new FnB717Line27Update.Export();

    MoveStatsReport1(local.StatsReport, useImport.StatsReport);
    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(FnB717Line27Update.Execute, useImport, useExport);

    local.AbortProgram.Flag = useExport.Abort.Flag;
  }

  private void UseFnB717Line3()
  {
    var useImport = new FnB717Line3.Import();
    var useExport = new FnB717Line3.Export();

    MoveStatsReport1(local.StatsReport, useImport.StatsReport);
    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(FnB717Line3.Execute, useImport, useExport);

    local.AbortProgram.Flag = useExport.Abort.Flag;
  }

  private void UseFnB717Line3132()
  {
    var useImport = new FnB717Line3132.Import();
    var useExport = new FnB717Line3132.Export();

    MoveStatsReport1(local.StatsReport, useImport.StatsReport);
    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(FnB717Line3132.Execute, useImport, useExport);

    local.AbortProgram.Flag = useExport.Abort.Flag;
  }

  private void UseFnB717Line33Update()
  {
    var useImport = new FnB717Line33Update.Import();
    var useExport = new FnB717Line33Update.Export();

    MoveStatsReport1(local.StatsReport, useImport.StatsReport);
    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(FnB717Line33Update.Execute, useImport, useExport);

    local.AbortProgram.Flag = useExport.Abort.Flag;
  }

  private void UseFnB717Line5()
  {
    var useImport = new FnB717Line5.Import();
    var useExport = new FnB717Line5.Export();

    MoveStatsReport1(local.StatsReport, useImport.StatsReport);
    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(FnB717Line5.Execute, useImport, useExport);

    local.AbortProgram.Flag = useExport.Abort.Flag;
  }

  private void UseFnOcse157GetRestartLineNbr()
  {
    var useImport = new FnOcse157GetRestartLineNbr.Import();
    var useExport = new FnOcse157GetRestartLineNbr.Export();

    MoveProgramCheckpointRestart(local.ProgramCheckpointRestart,
      useImport.ProgramCheckpointRestart);

    Call(FnOcse157GetRestartLineNbr.Execute, useImport, useExport);

    local.Restart.LineNumber = useExport.Restart.LineNumber;
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
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of StatsReport.
      /// </summary>
      [JsonPropertyName("statsReport")]
      public StatsReport StatsReport
      {
        get => statsReport ??= new();
        set => statsReport = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 35;

      private StatsReport statsReport;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    /// <summary>
    /// A value of StatsReport.
    /// </summary>
    [JsonPropertyName("statsReport")]
    public StatsReport StatsReport
    {
      get => statsReport ??= new();
      set => statsReport = value;
    }

    /// <summary>
    /// A value of To.
    /// </summary>
    [JsonPropertyName("to")]
    public StatsReport To
    {
      get => to ??= new();
      set => to = value;
    }

    /// <summary>
    /// A value of From.
    /// </summary>
    [JsonPropertyName("from")]
    public StatsReport From
    {
      get => from ??= new();
      set => from = value;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    /// <summary>
    /// A value of TestRun.
    /// </summary>
    [JsonPropertyName("testRun")]
    public Common TestRun
    {
      get => testRun ??= new();
      set => testRun = value;
    }

    /// <summary>
    /// A value of DisplayInd.
    /// </summary>
    [JsonPropertyName("displayInd")]
    public Common DisplayInd
    {
      get => displayInd ??= new();
      set => displayInd = value;
    }

    /// <summary>
    /// A value of ReportStartDate.
    /// </summary>
    [JsonPropertyName("reportStartDate")]
    public DateWorkArea ReportStartDate
    {
      get => reportStartDate ??= new();
      set => reportStartDate = value;
    }

    /// <summary>
    /// A value of ReportEndDate.
    /// </summary>
    [JsonPropertyName("reportEndDate")]
    public DateWorkArea ReportEndDate
    {
      get => reportEndDate ??= new();
      set => reportEndDate = value;
    }

    /// <summary>
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public Ocse157Verification Restart
    {
      get => restart ??= new();
      set => restart = value;
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
    /// A value of AbortProgram.
    /// </summary>
    [JsonPropertyName("abortProgram")]
    public Common AbortProgram
    {
      get => abortProgram ??= new();
      set => abortProgram = value;
    }

    private Array<GroupGroup> group;
    private StatsReport statsReport;
    private StatsReport to;
    private StatsReport from;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Common testRun;
    private Common displayInd;
    private DateWorkArea reportStartDate;
    private DateWorkArea reportEndDate;
    private Ocse157Verification restart;
    private EabFileHandling eabFileHandling;
    private Common abortProgram;
  }
#endregion
}
