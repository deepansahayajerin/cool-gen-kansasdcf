// Program: SI_B287_KDOR_DL_MATCH, ID: 1625320699, model: 746.
// Short name: SWEI287B
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B287_KDOR_DL_MATCH.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SiB287KdorDlMatch: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B287_KDOR_DL_MATCH program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB287KdorDlMatch(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB287KdorDlMatch.
  /// </summary>
  public SiB287KdorDlMatch(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // --------------------------------------------------------------------------------------------------
    // Date      Developer	Request #	Description
    // --------  ----------	----------	
    // ---------------------------------------------------------
    // 11/27/18  GVandy	CQ61419		Initial Development.
    // --------------------------------------------------------------------------------------------------
    // --------------------------------------------------------------------------------------------------
    // Overview
    //   1. Process a file containing Driver's License matches from KDOR.
    //   2. Store the information for display on the KDOR screen, create 
    // appropriate alerts and
    //      conditionally update information on APDS.
    //   3. Log totals to the control report.
    // --------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // -------------------------------------------------------------------------------------
    // --  Read the PPI Record.
    // -------------------------------------------------------------------------------------
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    // ------------------------------------------------------------------------------
    // -- Read for restart info.
    // ------------------------------------------------------------------------------
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // -------------------------------------------------------------------------------------
    // --  Open the Error Report.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProgramName = global.UserId;
    local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    UseCabErrorReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_OPENING_ERROR_RPT";

      return;
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      // -- This could have resulted from not finding the PPI record.
      // -- Extract the exit state message and write to the error report.
      UseEabExtractExitStateMessage();
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Initialization Error..." + local
        .ExitStateWorkArea.Message;
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------
    // --  Open the Control Report.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    UseCabControlReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error opening the control report.  Return status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ------------------------------------------------------------------------------
    // -- Determine if we're restarting.
    // ------------------------------------------------------------------------------
    if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      // -------------------------------------------------------------------------------------
      //  Checkpoint Info...
      // 	Position  Description
      // 	--------  
      // ---------------------------------------------------------
      // 	001-010   Last Person Number Processed
      // 	011-011   Blank
      // 	012-020   Total Number of Match Records Read from the File
      // -------------------------------------------------------------------------------------
      local.Restart.Number =
        Substring(local.ProgramCheckpointRestart.RestartInfo, 1, 10);
      local.TotalNumbRecordsRead.Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 12, 9));

      // -------------------------------------------------------------------------------------
      // --  Open the Input File.
      // -------------------------------------------------------------------------------------
      local.EabFileHandling.Action = "OPEN";
      UseSiB287ReadDlMatchFile2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error opening input file on restart.  Return status = " + local
          .EabFileHandling.Status;
        UseCabErrorReport2();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      // -------------------------------------------------------------------------------------
      // --  Position the Input File.
      // -------------------------------------------------------------------------------------
      do
      {
        local.EabFileHandling.Action = "READ";
        UseSiB287ReadDlMatchFile1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          if (Equal(local.EabFileHandling.Status, "EF"))
          {
            local.EabReportSend.RptDetail =
              "End of file encountered before finding restart NCP " + local
              .Restart.Number;
          }
          else
          {
            local.EabReportSend.RptDetail =
              "Error positioning input file.  Return status = " + local
              .EabFileHandling.Status;
          }

          local.EabFileHandling.Action = "WRITE";
          UseCabErrorReport2();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }
      while(!Equal(local.KdorDlMatchRecord.PersonNumber, local.Restart.Number));

      // -------------------------------------------------------------------------------------
      // --  Log restart info to the Control Report.
      // -------------------------------------------------------------------------------------
      for(local.Common.Count = 1; local.Common.Count <= 6; ++local.Common.Count)
      {
        switch(local.Common.Count)
        {
          case 1:
            local.EabReportSend.RptDetail = "Restarting at person number " + local
              .KdorDlMatchRecord.PersonNumber;

            break;
          case 3:
            local.EabReportSend.RptDetail =
              "Number of Records Read from DL Match File in Previous Run.........." +
              NumberToString(local.TotalNumbRecordsRead.Count, 9, 7);

            break;
          default:
            local.EabReportSend.RptDetail = "";

            break;
        }

        local.EabFileHandling.Action = "WRITE";
        UseCabControlReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          // -- Write to the error report.
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "(01) Error Writing Control Report...  Returned Status = " + local
            .EabFileHandling.Status;
          UseCabErrorReport2();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }
    }
    else
    {
      local.TotalNumbRecordsRead.Count = 0;

      // -------------------------------------------------------------------------------------
      // --  Open the Input File.
      // -------------------------------------------------------------------------------------
      local.EabFileHandling.Action = "OPEN";
      UseSiB287ReadDlMatchFile2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error opening input file.  Return status = " + local
          .EabFileHandling.Status;
        UseCabErrorReport2();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    local.NumbRecordsSinceChckpnt.Count = 0;

    // -------------------------------------------------------------------------------------
    // -- Read each drivers license match record from the input file.
    // -------------------------------------------------------------------------------------
    do
    {
      // ---------------------------------------------------------------------------------------------------
      // DRIVER'S LICENSE MATCH FILE RECORD LAYOUT
      // 			Data Type
      // Field Name		on Website	Length
      // --------------------	----------	------
      // LastName		Varchar		17
      // FirstName		Varchar		12
      // SSN			Varchar		9
      // DateOfBirth		Varchar		10
      // PersonNumber		Varchar		10
      // DriverLicenseNumber	Varchar		9
      // DLClass			Varchar		1	1 for Yes; 0 for No
      // MotorcycleClass		Varchar		1	1 for Yes; 0 for No
      // CDLClass		Varchar		1	1 for Yes; 0 for No
      // ExpirationDate		Varchar		10
      // Gender			Varchar		1
      // AddressLine1		Varchar		20
      // AddressLine2		Varchar		20
      // City			Varchar		15
      // State			Varchar		2
      // ZipCode			Varchar		9
      // HeightFT		Varchar		1
      // HeightIn		Varchar		2
      // Weight			Varchar		3
      // EyeColor		Varchar		3	
      // ---------------------------------------------------------------------------------------------------
      ++local.NumbRecordsSinceChckpnt.Count;

      // -------------------------------------------------------------------------------------
      // -- Read drivers license match record from the input file.
      // -------------------------------------------------------------------------------------
      local.EabFileHandling.Action = "READ";
      UseSiB287ReadDlMatchFile1();

      if (Equal(local.EabFileHandling.Status, "EF"))
      {
        continue;
      }

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabReportSend.RptDetail =
          "Error reading input file.  Return status = " + local
          .EabFileHandling.Status;
        local.EabFileHandling.Action = "WRITE";
        UseCabErrorReport2();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        break;
      }

      ++local.TotalNumbRecordsRead.Count;

      // -------------------------------------------------------------------------------------
      // -- Process the driver's license match record.
      // -------------------------------------------------------------------------------------
      UseSiB287ProcessDlMatch();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        // -- Extract the exit state message and write to the error report.
        UseEabExtractExitStateMessage();
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "Error Processing CSP # " + local
          .KdorDlMatchRecord.PersonNumber + " - " + local
          .ExitStateWorkArea.Message;
        UseCabErrorReport2();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      // -- Commit processing.
      if (local.NumbRecordsSinceChckpnt.Count > local
        .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault())
      {
        // -- Checkpoint.
        // -------------------------------------------------------------------------------------
        //  Checkpoint Info...
        // 	Position  Description
        // 	--------  
        // ---------------------------------------------------------
        // 	001-010   Last Person Number Processed
        // 	011-011   Blank
        // 	012-020   Total Number of Match Records Read from the File
        // -------------------------------------------------------------------------------------
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.RestartInfo =
          local.KdorDlMatchRecord.PersonNumber + " " + NumberToString
          (local.TotalNumbRecordsRead.Count, 7, 9);
        UseUpdateCheckpointRstAndCommit();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "Error committing.";
          UseCabErrorReport2();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          break;
        }

        local.NumbRecordsSinceChckpnt.Count = 0;
      }
    }
    while(!Equal(local.EabFileHandling.Status, "EF"));

    // -------------------------------------------------------------------------------------
    // --  Check for an empty file.
    // -------------------------------------------------------------------------------------
    if (local.TotalNumbRecordsRead.Count == 0)
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error - Driver's License Match file from KDOR (SR.RJE.SR29712.DLINFO) is empty.";
        
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // -------------------------------------------------------------------------------------
      // --  Write Totals to the Control Report.
      // -------------------------------------------------------------------------------------
      for(local.Common.Count = 1; local.Common.Count <= 3; ++local.Common.Count)
      {
        if (local.Common.Count == 1)
        {
          local.EabReportSend.RptDetail =
            "Number of Records Read from DL Match File.........." + NumberToString
            (local.TotalNumbRecordsRead.Count, 9, 7);
        }
        else
        {
          local.EabReportSend.RptDetail = "";
        }

        local.EabFileHandling.Action = "WRITE";
        UseCabControlReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          // -- Write to the error report.
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "(02) Error Writing Control Report...  Returned Status = " + local
            .EabFileHandling.Status;
          UseCabErrorReport2();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          goto Test;
        }
      }

      // ------------------------------------------------------------------------------
      // -- Take a final checkpoint.
      // ------------------------------------------------------------------------------
      local.ProgramCheckpointRestart.RestartInd = "N";
      local.ProgramCheckpointRestart.RestartInfo = "";
      UseUpdateCheckpointRstAndCommit();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "Error taking final checkpoint.";
        UseCabErrorReport2();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      }
    }

Test:

    // -------------------------------------------------------------------------------------
    // --  Close the Input File.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseSiB287ReadDlMatchFile2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error closing input file.  Return status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }

    // -------------------------------------------------------------------------------------
    // --  Close the Control Report.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // -- Write to the error report.
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error Closing Control Report...  Returned Status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }

    // -------------------------------------------------------------------------------------
    // --  Close the Error Report.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "SI0000_ERR_CLOSING_ERR_RPT_FILE";
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
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

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport3()
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

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
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

  private void UseSiB287ProcessDlMatch()
  {
    var useImport = new SiB287ProcessDlMatch.Import();
    var useExport = new SiB287ProcessDlMatch.Export();

    useImport.KdorDlMatchRecord.Assign(local.KdorDlMatchRecord);

    Call(SiB287ProcessDlMatch.Execute, useImport, useExport);
  }

  private void UseSiB287ReadDlMatchFile1()
  {
    var useImport = new SiB287ReadDlMatchFile.Import();
    var useExport = new SiB287ReadDlMatchFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;
    useExport.KdorDlMatchRecord.Assign(local.KdorDlMatchRecord);

    Call(SiB287ReadDlMatchFile.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    local.KdorDlMatchRecord.Assign(useExport.KdorDlMatchRecord);
  }

  private void UseSiB287ReadDlMatchFile2()
  {
    var useImport = new SiB287ReadDlMatchFile.Import();
    var useExport = new SiB287ReadDlMatchFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(SiB287ReadDlMatchFile.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseUpdateCheckpointRstAndCommit()
  {
    var useImport = new UpdateCheckpointRstAndCommit.Import();
    var useExport = new UpdateCheckpointRstAndCommit.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdateCheckpointRstAndCommit.Execute, useImport, useExport);
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
    /// A value of KdorDlMatchRecord.
    /// </summary>
    [JsonPropertyName("kdorDlMatchRecord")]
    public KdorDlMatchRecord KdorDlMatchRecord
    {
      get => kdorDlMatchRecord ??= new();
      set => kdorDlMatchRecord = value;
    }

    /// <summary>
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public CsePerson Restart
    {
      get => restart ??= new();
      set => restart = value;
    }

    /// <summary>
    /// A value of NumbRecordsSinceChckpnt.
    /// </summary>
    [JsonPropertyName("numbRecordsSinceChckpnt")]
    public Common NumbRecordsSinceChckpnt
    {
      get => numbRecordsSinceChckpnt ??= new();
      set => numbRecordsSinceChckpnt = value;
    }

    /// <summary>
    /// A value of TotalNumbRecordsRead.
    /// </summary>
    [JsonPropertyName("totalNumbRecordsRead")]
    public Common TotalNumbRecordsRead
    {
      get => totalNumbRecordsRead ??= new();
      set => totalNumbRecordsRead = value;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
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
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
    }

    private KdorDlMatchRecord kdorDlMatchRecord;
    private CsePerson restart;
    private Common numbRecordsSinceChckpnt;
    private Common totalNumbRecordsRead;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Common common;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private ExitStateWorkArea exitStateWorkArea;
  }
#endregion
}
