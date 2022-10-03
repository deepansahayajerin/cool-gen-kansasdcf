// Program: OE_B465_HOUSEKEEPING, ID: 370980956, model: 746.
// Short name: SWE00502
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B465_HOUSEKEEPING.
/// </summary>
[Serializable]
public partial class OeB465Housekeeping: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B465_HOUSEKEEPING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB465Housekeeping(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB465Housekeeping.
  /// </summary>
  public OeB465Housekeeping(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    switch(AsChar(import.Common.SelectChar))
    {
      case 'O':
        // ***** Get the run parameters for this program.
        export.ProgramCheckpointRestart.ProgramName = global.UserId;
        local.EabReportSend.ProcessDate = Now().Date;
        export.ProgramProcessingInfo.Name = global.UserId;
        local.EabReportSend.ProgramName = global.UserId;
        export.ProgramProcessingInfo.CreatedBy = global.UserId;
        UseReadProgramProcessingInfo();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (Equal(export.ProgramProcessingInfo.ProcessDate, local.Null1.Date))
        {
          ExitState = "OE0000_B465_NULL_PROCESS_DATE_RB";

          return;
        }

        local.EabReportSend.ProgramName = export.ProgramProcessingInfo.Name;
        local.EabReportSend.ProcessDate =
          export.ProgramProcessingInfo.ProcessDate;
        export.EabFileHandling.Action = "OPEN";
        UseCabErrorReport2();

        if (!Equal(export.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NE0000_ERROR_OPEN_ERROR_RPT";

          return;
        }

        export.EabFileHandling.Action = "OPEN";
        UseCabControlReport2();

        if (!Equal(export.EabFileHandling.Status, "OK"))
        {
          export.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = global.UserId + ": Unable to open report control file.";
            
          UseCabErrorReport1();
          ExitState = "ACO_RC_AB0008_INVALID_RETURN_CD";

          return;
        }

        // ****************************************************************************
        // This action block will retrieve the checkpoint restart values from 
        // the
        // program checkpoint restart table. These values will be used by this 
        // module
        // to determine it's processing state of execution - initial run or 
        // restart.
        // ****************************************************************************
        UseReadPgmCheckpointRestart();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "ERROR: Unable to Retrieve Program Restart Parameters.";
          UseCabErrorReport1();

          if (!Equal(export.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_RC_AB0008_INVALID_RETURN_CD";

            return;
          }

          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        if (AsChar(export.ProgramCheckpointRestart.RestartInd) == 'Y')
        {
          // ***********************************************************************
          // Load AE Case Number to restart with.
          // ***********************************************************************
          export.Restart.AeCaseNo =
            Substring(export.ProgramCheckpointRestart.RestartInfo, 1, 8);

          // ***********************************************************************
          // Write restart information to the control report.
          // ***********************************************************************
          export.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Program is being restarted after AE case number: " + export
            .Restart.AeCaseNo;
          UseCabControlReport1();

          if (!Equal(export.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_RC_AB0008_INVALID_RETURN_CD";

            return;
          }

          export.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "";
          UseCabControlReport1();

          if (!Equal(export.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_RC_AB0008_INVALID_RETURN_CD";

            return;
          }
        }

        // ******************************************************************
        // OPEN THE BENEFITS FILE AND THE EAB's CONTROL FILE
        // ******************************************************************
        export.EabFileHandling.Action = "OPEN";
        UseOeB465EabReadAeBenefitFile();

        if (!Equal(export.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_RC_AB0008_INVALID_RETURN_CD";

          return;
        }

        // ******************************************************************
        // READ THE TRAILER RECORD FROM THE BENEFITS FILE
        // AND COMPARE TO THE CONTROL FILE.  THIS IS
        // DONE EXTERNAL TO THIS PROGRAM (IN THE EAB).
        // ******************************************************************
        export.EabFileHandling.Action = "CONTROL1";
        UseOeB465EabReadAeBenefitFile();

        if (Equal(export.EabFileHandling.Status, "EOF"))
        {
          // ******************************************************************
          // THE BENEFITS FILE IS EMPTY
          // ******************************************************************
          local.EabReportSend.RptDetail = "ERROR:  THE BENEFITS FILE IS EMPTY";
          export.EabFileHandling.Action = "WRITE";
          UseCabErrorReport1();

          if (!Equal(export.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_RC_AB0008_INVALID_RETURN_CD";
          }
        }
        else if (!Equal(export.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_RC_AB0008_INVALID_RETURN_CD";
        }

        break;
      case 'C':
        // ******************************************************************
        // COMPARE THE FILE COUNT KEPT IN THE EAB TO THE TRAILER RECORD.
        // THIS IS DONE EXTERNAL TO THIS PROGRAM (IN THE EAB).
        // ******************************************************************
        export.EabFileHandling.Action = "CONTROL2";
        UseOeB465EabReadAeBenefitFile();

        if (!Equal(export.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_RC_AB0008_INVALID_RETURN_CD";

          return;
        }

        export.EabFileHandling.Action = "CLOSE";
        UseOeB465EabReadAeBenefitFile();

        if (!Equal(export.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_RC_AB0008_INVALID_RETURN_CD";

          return;
        }

        // ******************************************************
        // Close Reports
        // ******************************************************
        export.EabFileHandling.Action = "CLOSE";
        UseCabControlReport3();

        if (!Equal(export.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_RC_AB0008_INVALID_RETURN_CD";

          return;
        }

        UseCabErrorReport3();

        if (!Equal(export.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_RC_AB0008_INVALID_RETURN_CD";
        }

        break;
      default:
        ExitState = "ACO_RC_AB0004_INVALID_PARM1_CODE";

        break;
    }
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
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
    target.ReadFrequencyCount = source.ReadFrequencyCount;
    target.CheckpointCount = source.CheckpointCount;
    target.LastCheckpointTimestamp = source.LastCheckpointTimestamp;
    target.RestartInd = source.RestartInd;
    target.RestartInfo = source.RestartInfo;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = export.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = export.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport3()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = export.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = export.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = export.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = export.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseOeB465EabReadAeBenefitFile()
  {
    var useImport = new OeB465EabReadAeBenefitFile.Import();
    var useExport = new OeB465EabReadAeBenefitFile.Export();

    useImport.EabFileHandling.Action = export.EabFileHandling.Action;
    useExport.EabFileHandling.Status = export.EabFileHandling.Status;

    Call(OeB465EabReadAeBenefitFile.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseReadPgmCheckpointRestart()
  {
    var useImport = new ReadPgmCheckpointRestart.Import();
    var useExport = new ReadPgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      export.ProgramCheckpointRestart.ProgramName;

    Call(ReadPgmCheckpointRestart.Execute, useImport, useExport);

    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      export.ProgramCheckpointRestart);
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = export.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    export.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    private Common common;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    /// <summary>
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public ImHousehold Restart
    {
      get => restart ??= new();
      set => restart = value;
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

    private EabFileHandling eabFileHandling;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ImHousehold restart;
    private ProgramProcessingInfo programProcessingInfo;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    private EabReportSend eabReportSend;
    private DateWorkArea null1;
  }
#endregion
}
