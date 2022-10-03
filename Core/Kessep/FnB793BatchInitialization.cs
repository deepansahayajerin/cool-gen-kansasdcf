// Program: FN_B793_BATCH_INITIALIZATION, ID: 1902420745, model: 746.
// Short name: SWE03734
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B793_BATCH_INITIALIZATION.
/// </summary>
[Serializable]
public partial class FnB793BatchInitialization: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B793_BATCH_INITIALIZATION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB793BatchInitialization(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB793BatchInitialization.
  /// </summary>
  public FnB793BatchInitialization(IContext context, Import import,
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
    // -----------------------------------------------------------------------------------------------
    // Date      Developer	Request #	Description
    // --------  ----------	----------	
    // -------------------------------------------------------
    // 10/09/2013  GVandy	CQ38344		Initial Development.
    // -----------------------------------------------------------------------------------------------
    // -----------------------------------------------------------------------------------------------
    // --  Retrieve the PPI Record.
    // -----------------------------------------------------------------------------------------------
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    // -----------------------------------------------------------------------------------------------
    // --  Open the Error Report.
    // -----------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProgramName = global.UserId;
    local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_OPENING_ERROR_RPT";

      return;
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      // -- This could have resulted from not finding the PPI record.  Had to 
      // open the error report before escaping to the PrAD.
      return;
    }

    // -----------------------------------------------------------------------------------------------
    // --  Open the Control Report.
    // -----------------------------------------------------------------------------------------------
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_OPENING_CONTROL_RPT";

      return;
    }

    // -----------------------------------------------------------------------------------------------
    // --  Open the Extract File.
    // -----------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    UseFnB793WriteExtractFile2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "File Open Return Status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport1();
      ExitState = "OE0000_ERROR_OPENING_EXT_FILE";

      return;
    }

    // -----------------------------------------------------------------------------------------------
    // --  Write Header (Record Type = 1) to the Extract File.
    // -----------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "WRITE";
    local.RecordType.Text1 = "1";
    UseFnB793WriteExtractFile1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error writing header info to extract file...  Returned Status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport1();
      ExitState = "ERROR_WRITING_TO_FILE_AB";

      return;
    }

    // -----------------------------------------------------------------------------------------------
    // --  Extract the number of days to be reported from the PPI record.  This 
    // is an
    // --  optional parameter and will default to 90 days if not specified on 
    // the PPI record.
    // --
    // --  Position  1 - 3   Number of Report Days (Optional, will default to 90
    // days)
    // -----------------------------------------------------------------------------------------------
    if (IsEmpty(Substring(local.ProgramProcessingInfo.ParameterList, 1, 3)))
    {
      // -- Reporting period number of days was not specified on the PPI record.
      // -- The report period end date will default to the PPI Processing Date 
      // and the
      // -- report period start date will default to the PPI Processing Date - 
      // 89 days.
      // -- This will yield a 90 day reporting period based on the PPI 
      // processing date.
      export.ReportingPeriodEnding.Date =
        local.ProgramProcessingInfo.ProcessDate;
      export.ReportingPeriodStarting.Date =
        AddDays(export.ReportingPeriodEnding.Date, -89);
    }
    else
    {
      // -- Set the reporting period using info on the PPI record.
      export.ReportingPeriodEnding.Date =
        local.ProgramProcessingInfo.ProcessDate;
      export.ReportingPeriodStarting.Date =
        AddDays(export.ReportingPeriodEnding.Date,
        (int)(-(
          StringToNumber(Substring(
          local.ProgramProcessingInfo.ParameterList, 1, 3)) - 1)));
    }

    for(local.Common.Count = 1; local.Common.Count <= 4; ++local.Common.Count)
    {
      switch(local.Common.Count)
      {
        case 1:
          // -----------------------------------------------------------------------------------------------
          // --  Write the Reporting Period Number of Days to the Control 
          // Report.
          // -----------------------------------------------------------------------------------------------
          if (IsEmpty(Substring(local.ProgramProcessingInfo.ParameterList, 1, 3)))
            
          {
            local.EabReportSend.RptDetail =
              "Reporting Period # of Days . . . . .090 (Default)";
          }
          else
          {
            local.EabReportSend.RptDetail =
              "Reporting Period # of Days . . . . ." + Substring
              (local.ProgramProcessingInfo.ParameterList, 1, 3);
          }

          break;
        case 2:
          // -----------------------------------------------------------------------------------------------
          // --  Write the Reporting Period Start Date to the Control Report.
          // -----------------------------------------------------------------------------------------------
          local.EabReportSend.RptDetail =
            "Reporting Period Start Date. . . . .";
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + NumberToString
            (Month(export.ReportingPeriodStarting.Date), 14, 2);
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "/";
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + NumberToString
            (Day(export.ReportingPeriodStarting.Date), 14, 2);
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "/";
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + NumberToString
            (Year(export.ReportingPeriodStarting.Date), 12, 4);

          break;
        case 3:
          // -----------------------------------------------------------------------------------------------
          // --  Write the Reporting Period End Date to the Control Report.
          // -----------------------------------------------------------------------------------------------
          local.EabReportSend.RptDetail =
            "Reporting Period End Date. . . . . .";
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + NumberToString
            (Month(export.ReportingPeriodEnding.Date), 14, 2);
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "/";
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + NumberToString
            (Day(export.ReportingPeriodEnding.Date), 14, 2);
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "/";
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + NumberToString
            (Year(export.ReportingPeriodEnding.Date), 12, 4);

          break;
        case 4:
          // -----------------------------------------------------------------------------------------------
          // --  Write the File Header Date to the Control Report.
          // -----------------------------------------------------------------------------------------------
          local.EabReportSend.RptDetail = "";
          local.EabReportSend.RptDetail =
            "IVR NCP File Header Date . . . . . .";
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + NumberToString
            (Month(local.ProgramProcessingInfo.ProcessDate), 14, 2);
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "/";
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + NumberToString
            (Day(local.ProgramProcessingInfo.ProcessDate), 14, 2);
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "/";
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + NumberToString
            (Year(local.ProgramProcessingInfo.ProcessDate), 12, 4);

          break;
        default:
          break;
      }

      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

        return;
      }
    }

    // -----------------------------------------------------------------------------------------------
    // --  Get commit frequency.  Checkpoint/Restart is deliberately ommitted.
    // --  The program will always start from the beginning and create the 
    // entire file.
    // -----------------------------------------------------------------------------------------------
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    export.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.ReadFrequencyCount = source.ReadFrequencyCount;
    target.RestartInd = source.RestartInd;
    target.RestartInfo = source.RestartInfo;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnB793WriteExtractFile1()
  {
    var useImport = new FnB793WriteExtractFile.Import();
    var useExport = new FnB793WriteExtractFile.Export();

    useImport.RecordType.Text1 = local.RecordType.Text1;
    useImport.Header.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(FnB793WriteExtractFile.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnB793WriteExtractFile2()
  {
    var useImport = new FnB793WriteExtractFile.Import();
    var useExport = new FnB793WriteExtractFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(FnB793WriteExtractFile.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseReadPgmCheckpointRestart()
  {
    var useImport = new ReadPgmCheckpointRestart.Import();
    var useExport = new ReadPgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      local.ProgramCheckpointRestart.ProgramName;

    Call(ReadPgmCheckpointRestart.Execute, useImport, useExport);

    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

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
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ReportingPeriodStarting.
    /// </summary>
    [JsonPropertyName("reportingPeriodStarting")]
    public DateWorkArea ReportingPeriodStarting
    {
      get => reportingPeriodStarting ??= new();
      set => reportingPeriodStarting = value;
    }

    /// <summary>
    /// A value of ReportingPeriodEnding.
    /// </summary>
    [JsonPropertyName("reportingPeriodEnding")]
    public DateWorkArea ReportingPeriodEnding
    {
      get => reportingPeriodEnding ??= new();
      set => reportingPeriodEnding = value;
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

    private DateWorkArea reportingPeriodStarting;
    private DateWorkArea reportingPeriodEnding;
    private ProgramCheckpointRestart programCheckpointRestart;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of RecordType.
    /// </summary>
    [JsonPropertyName("recordType")]
    public TextWorkArea RecordType
    {
      get => recordType ??= new();
      set => recordType = value;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
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

    private TextWorkArea recordType;
    private Common numberOfDays;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private Common common;
    private ProgramCheckpointRestart programCheckpointRestart;
  }
#endregion
}
