// Program: OE_B485_DELETE_LOCATE_REQUESTS, ID: 374439316, model: 746.
// Short name: SWEE485B
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_B485_DELETE_LOCATE_REQUESTS.
/// </para>
/// <para>
/// The purpose of this program is to purge LOCATE_REQUEST entities which have 
/// not had any activity performed on them in the last 18 months
/// AND / OR for LOCATE_REQUESTS involving agencies which have multiple sources 
/// which are ready to have the request resubmitted.
/// This program will need to be run immediately before the following:
/// OE_B486_CREATE_LOCATE_REQUESTS
/// OE_B487_WRITE_LOCATE_REQUESTS
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class OeB485DeleteLocateRequests: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B485_DELETE_LOCATE_REQUESTS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB485DeleteLocateRequests(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB485DeleteLocateRequests.
  /// </summary>
  public OeB485DeleteLocateRequests(IContext context, Import import,
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
    // --------------------------------------------------------------
    // CHANGE LOG:
    // 07/10/2000	PMcElderry
    // Original coding
    // --------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      // -------------------------------
      // Get DB2 commit frequency counts
      // -------------------------------
      local.ProgramCheckpointRestart.ProgramName = global.UserId;
      UseReadPgmCheckpointRestart();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
        {
          local.ProgramCheckpointRestart.RestartInfo =
            local.ProgramCheckpointRestart.RestartInfo ?? "";
        }
        else
        {
          local.ProgramCheckpointRestart.RestartInfo = "";
        }

        // -------------------------------------------------
        // Open Error Report DDNAME=RPT99 and Control Report
        // DDNAME = RPT98
        // -------------------------------------------------
        local.EabFileHandling.Action = "OPEN";
        local.EabReportSend.ProcessDate =
          local.ProgramProcessingInfo.ProcessDate;
        local.EabReportSend.ProgramName = "SWEEB485";
        UseCabErrorReport3();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
        }
        else
        {
          UseCabControlReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Error encountered opening control report.";
            UseCabErrorReport2();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
          }
          else
          {
            UseOeDeleteLocateRequest();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              local.EabReportSend.RptDetail =
                "Total Number of Locate Requests deleted: " + NumberToString
                (local.TotalDeletes.Count, 15);
              local.EabFileHandling.Action = "WRITE";
              UseCabControlReport1();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "Error encountered while writting control report for number of requests deleted.";
                  
                UseCabErrorReport2();
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
              }
              else
              {
                // -------------------------------------------------------------
                // Set restart indicator to "N" -  successfully finished program
                // -------------------------------------------------------------
                local.ProgramCheckpointRestart.RestartInd = "N";
                local.ProgramCheckpointRestart.ProgramName = global.UserId;
                local.ProgramCheckpointRestart.RestartInfo = "";
                UseUpdatePgmCheckpointRestart();
                local.EabFileHandling.Action = "CLOSE";
                UseCabControlReport1();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  local.EabFileHandling.Action = "WRITE";
                  local.EabReportSend.RptDetail =
                    "Error encountered while closing control report.";
                  UseCabErrorReport2();
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
                }
                else
                {
                  UseCabErrorReport1();

                  if (!Equal(local.EabFileHandling.Status, "OK"))
                  {
                    ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
                  }
                  else
                  {
                    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
                  }
                }
              }
            }
          }
        }
      }
    }

    local.CloseCsePersonsWorkSet.Ssn = "close";
    UseEabReadCsePersonUsingSsn();

    // Do NOT need to verify on Return
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveProgramCheckpointRestart1(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.ProgramName = source.ProgramName;
    target.RestartInd = source.RestartInd;
    target.RestartInfo = source.RestartInfo;
  }

  private static void MoveProgramCheckpointRestart2(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
    target.RestartInd = source.RestartInd;
    target.RestartInfo = source.RestartInfo;
  }

  private static void MoveProgramCheckpointRestart3(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
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

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabReadCsePersonUsingSsn()
  {
    var useImport = new EabReadCsePersonUsingSsn.Import();
    var useExport = new EabReadCsePersonUsingSsn.Export();

    useImport.CsePersonsWorkSet.Ssn = local.CloseCsePersonsWorkSet.Ssn;
    useExport.CsePersonsWorkSet.Assign(local.CloseReturned);
    useExport.AbendData.Assign(local.CloseAbendData);

    Call(EabReadCsePersonUsingSsn.Execute, useImport, useExport);

    local.CloseReturned.Assign(useExport.CsePersonsWorkSet);
    local.CloseAbendData.Assign(useExport.AbendData);
  }

  private void UseOeDeleteLocateRequest()
  {
    var useImport = new OeDeleteLocateRequest.Import();
    var useExport = new OeDeleteLocateRequest.Export();

    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;
    MoveProgramCheckpointRestart3(local.ProgramCheckpointRestart,
      useImport.ProgramCheckpointRestart);

    Call(OeDeleteLocateRequest.Execute, useImport, useExport);

    local.TotalDeletes.Count = useExport.TotalDeleteCount.Count;
  }

  private void UseReadPgmCheckpointRestart()
  {
    var useImport = new ReadPgmCheckpointRestart.Import();
    var useExport = new ReadPgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      local.ProgramCheckpointRestart.ProgramName;

    Call(ReadPgmCheckpointRestart.Execute, useImport, useExport);

    MoveProgramCheckpointRestart2(useExport.ProgramCheckpointRestart,
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

  private void UseUpdatePgmCheckpointRestart()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    MoveProgramCheckpointRestart1(local.ProgramCheckpointRestart,
      useImport.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
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
    /// A value of PassArea.
    /// </summary>
    [JsonPropertyName("passArea")]
    public External PassArea
    {
      get => passArea ??= new();
      set => passArea = value;
    }

    /// <summary>
    /// A value of TotalDeletes.
    /// </summary>
    [JsonPropertyName("totalDeletes")]
    public Common TotalDeletes
    {
      get => totalDeletes ??= new();
      set => totalDeletes = value;
    }

    /// <summary>
    /// A value of CloseCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("closeCsePersonsWorkSet")]
    public CsePersonsWorkSet CloseCsePersonsWorkSet
    {
      get => closeCsePersonsWorkSet ??= new();
      set => closeCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of CloseReturned.
    /// </summary>
    [JsonPropertyName("closeReturned")]
    public CsePersonsWorkSet CloseReturned
    {
      get => closeReturned ??= new();
      set => closeReturned = value;
    }

    /// <summary>
    /// A value of CloseAbendData.
    /// </summary>
    [JsonPropertyName("closeAbendData")]
    public AbendData CloseAbendData
    {
      get => closeAbendData ??= new();
      set => closeAbendData = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private External passArea;
    private Common totalDeletes;
    private CsePersonsWorkSet closeCsePersonsWorkSet;
    private CsePersonsWorkSet closeReturned;
    private AbendData closeAbendData;
  }
#endregion
}
