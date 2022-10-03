// Program: LE_B571_ALASKA_PFD_MATCH_REQUEST, ID: 1902529316, model: 746.
// Short name: SWEL571B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_B571_ALASKA_PFD_MATCH_REQUEST.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class LeB571AlaskaPfdMatchRequest: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B571_ALASKA_PFD_MATCH_REQUEST program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB571AlaskaPfdMatchRequest(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB571AlaskaPfdMatchRequest.
  /// </summary>
  public LeB571AlaskaPfdMatchRequest(IContext context, Import import,
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
    // --------------------------------------------------------------------------------------------------
    // Date      Developer	Request #	Description
    // --------  ----------	----------	
    // ---------------------------------------------------------
    // 05/06/16  GVandy	CQ51956		Initial Development.
    // --------------------------------------------------------------------------------------------------
    // --------------------------------------------------------------------------------------------------
    // Business Rules
    // 	1. We must send KS obligor data by May 31st for matching to AKs PFD for
    // this years
    // 	   (2016) funds.
    // 		a. Must have an active NCP role.
    // 		b. Must have a Social Security Number
    // 	2. KS will receive a return match file the 2nd week of June.
    // 		a. Once we receive the match file back, workers determine which 
    // obligors
    // 		   qualify for PFDO program and begin sending requests to AK.
    // 			a. Must owe $50.00 in arrears
    // 			b. Do not send a PFDO transmittal if there is an open case
    // 			   between AK and KS, these cases will have an automatic interception
    // 			   of the AK funds.
    // 			c. Do not send a PFDO transmittal if Good Cause is set.
    // 		b. After the match all requests from KS must be received by AK no later
    // 		   than Monday, August 1st to ensure set up and collection of the PFD.
    // 		c. All submissions need to be submitted electronically by fax or email.
    // 		   Workers will prepare a transmittal #1.
    // 		d. AK will only honor withholding orders from AKs CSS Division, this 
    // is
    // 		   the only way States can collect AK funds.
    // 	3. A CSENet transaction must be sent prior to sending the required 
    // documents if
    // 	   KS is active with AK on CSENet.
    // 		a. Indicate on CSENet the transaction is for PFD only.
    // 		b. Send a CSENet closure if a CSENet open was sent to AK, do not use
    // 		   miscellaneous as a closure reason.
    // 	4. Required Documents:
    // 		a. CSE Transmittal #1 with the appropriate areas completed.
    // 		b. A copy of the signed order or judgment.
    // 		c. The direct phone number of the CS contact for KS.
    // 	5. A HIST and ALRT record will be created indicating the case(s) have a 
    // match with
    // 	   a AK Permanent Fund Dividend.
    // 	6. A narrative will be created on CSLN that will contain the information
    // from the
    // 	   results file.
    // 	7. An AK PFDM report will be created and be sorted by Contractor, Office
    // and Worker.
    // 	   The report will contain the information from the results file and the
    // following
    // 	   information:
    // 		a. Person Number
    // 		b. Case Number
    // 		c. Worker Name
    // 		d. Office Number
    // 	8. The report will be emailed to Deanne Dinkel with a cc to Ashley 
    // Dexter and
    // 	   Julie Heiman.
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
    // -- This program does not checkpoint.  It will always run from the 
    // beginning.
    // -------------------------------------------------------------------------------------
    // --  Open the Output File.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    UseLeB571WriteToMatchFile2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error opening output file.  Return status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.TotalNumbOfNcpsWritten.Count = 0;
    local.TotalNumbOfNcpsWoSsn.Count = 0;
    local.NumbOfNcpsSinceChckpnt.Count = 0;

    // -------------------------------------------------------------------------------------
    // -- Find each NCP with an active AP case role.
    // -- This read each is set to retrieve DISTINCT NCP person numbers.
    // -------------------------------------------------------------------------------------
    foreach(var item in ReadCsePerson())
    {
      ++local.NumbOfNcpsSinceChckpnt.Count;

      // -- Get NCP name, SSN, and Date of Birth from adabase
      local.Ncp.Number = entities.Ncp.Number;
      UseSiReadCsePerson();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        UseEabExtractExitStateMessage();
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "Error Reading ADABAS...NCP " + entities
          .Ncp.Number + " " + local.ExitStateWorkArea.Message;
        UseCabErrorReport2();

        if (IsExitState("ACO_ADABAS_UNAVAILABLE"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          break;
        }
        else
        {
          ExitState = "ACO_NN0000_ALL_OK";

          continue;
        }
      }

      if (IsEmpty(local.Ncp.Ssn) || Equal(local.Ncp.Ssn, "000000000"))
      {
        ++local.TotalNumbOfNcpsWoSsn.Count;

        continue;
      }

      // -- Format the match record.
      // 					    Start     End
      // Data Element		Data Type   Length  Position  Position	Note
      // ---------------------	----------
      // ------  --------  --------
      // ---------------------------------
      // FIPS State Code		Numeric		 2	  1	    2	20
      // FIPS County Code	Numeric		 3	  3	    5	000
      // NCP SSN			Numeric		 9	  6	   14
      // NCP Person Number	Text		15	 15	   29	Left justified w/ trailing blanks
      // NCP Last Name		Text		20	 30	   49	Left justified w/ trailing blanks
      // NCP First Name		Text		15	 50	   64	Left justified w/ trailing blanks
      // NCP Date of Birth	Numeric		 8	 65	   72	CCYYMMDD or 00000000
      // Filler			Text		 8	 73	   80	Blanks
      local.MatchFile.Text80 = "";
      local.MatchFile.Text80 = "20000" + local.Ncp.Ssn;
      local.MatchFile.Text80 =
        Substring(local.MatchFile.Text80, WorkArea.Text80_MaxLength, 1, 14) + entities
        .Ncp.Number;
      local.MatchFile.Text80 =
        Substring(local.MatchFile.Text80, WorkArea.Text80_MaxLength, 1, 29) + local
        .Ncp.LastName;
      local.MatchFile.Text80 =
        Substring(local.MatchFile.Text80, WorkArea.Text80_MaxLength, 1, 49) + local
        .Ncp.FirstName;

      if (Equal(local.Ncp.Dob, local.Null1.Date))
      {
        local.TextDate.Text8 = "00000000";
      }
      else
      {
        local.TextDate.Text8 = NumberToString(Year(local.Ncp.Dob), 12, 4) + NumberToString
          (Month(local.Ncp.Dob), 14, 2) + NumberToString
          (Day(local.Ncp.Dob), 14, 2);
      }

      local.MatchFile.Text80 =
        Substring(local.MatchFile.Text80, WorkArea.Text80_MaxLength, 1, 64) + local
        .TextDate.Text8;
      local.EabFileHandling.Action = "WRITE";
      UseLeB571WriteToMatchFile1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        // -- Write to the error report.
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error Writing NCP File...  Returned Status = " + local
          .EabFileHandling.Status;
        UseCabErrorReport2();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        break;
      }

      // -- Increment the NCP count.
      ++local.TotalNumbOfNcpsWritten.Count;

      // -- Commit processing.
      if (local.NumbOfNcpsSinceChckpnt.Count > local
        .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault())
      {
        // -- Checkpoint.
        // -------------------------------------------------------------------------------------
        //  Checkpoint Info...
        // 	None.  Commit only.
        // -------------------------------------------------------------------------------------
        local.ProgramCheckpointRestart.RestartInd = "N";
        UseUpdateCheckpointRstAndCommit();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "Error committing.";
          UseCabErrorReport2();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          break;
        }

        local.NumbOfNcpsSinceChckpnt.Count = 0;
      }
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // -------------------------------------------------------------------------------------
      // --  Write Totals to the Control Report.
      // -------------------------------------------------------------------------------------
      for(local.Common.Count = 1; local.Common.Count <= 5; ++local.Common.Count)
      {
        switch(local.Common.Count)
        {
          case 1:
            local.EabReportSend.RptDetail =
              "Number of NCPs Written to Alaska Permanent Fund Match File.........." +
              NumberToString(local.TotalNumbOfNcpsWritten.Count, 9, 7);

            break;
          case 3:
            local.EabReportSend.RptDetail =
              "Number of NCPs Excluded Due to no SSN..............................." +
              NumberToString(local.TotalNumbOfNcpsWoSsn.Count, 9, 7);

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
            "(02) Error Writing Control Report...  Returned Status = " + local
            .EabFileHandling.Status;
          UseCabErrorReport2();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          goto Test;
        }
      }
    }

Test:

    // -------------------------------------------------------------------------------------
    // --  Close the Output File.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseLeB571WriteToMatchFile2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error closing output file.  Return status = " + local
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

    // -------------------------------------------------------------------------------------
    // --  Close Adabas.
    // ---------------------------------------------------------------------------
    local.Ncp.Number = "CLOSE";
    UseEabReadCsePersonBatch();

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

  private void UseEabReadCsePersonBatch()
  {
    var useImport = new EabReadCsePersonBatch.Import();
    var useExport = new EabReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.Ncp.Number;

    Call(EabReadCsePersonBatch.Execute, useImport, useExport);
  }

  private void UseLeB571WriteToMatchFile1()
  {
    var useImport = new LeB571WriteToMatchFile.Import();
    var useExport = new LeB571WriteToMatchFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.WorkArea.Text80 = local.MatchFile.Text80;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(LeB571WriteToMatchFile.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseLeB571WriteToMatchFile2()
  {
    var useImport = new LeB571WriteToMatchFile.Import();
    var useExport = new LeB571WriteToMatchFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(LeB571WriteToMatchFile.Execute, useImport, useExport);

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

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.Ncp.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.Ncp.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseUpdateCheckpointRstAndCommit()
  {
    var useImport = new UpdateCheckpointRstAndCommit.Import();
    var useExport = new UpdateCheckpointRstAndCommit.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdateCheckpointRstAndCommit.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadCsePerson()
  {
    entities.Ncp.Populated = false;

    return ReadEach("ReadCsePerson",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Ncp.Number = db.GetString(reader, 0);
        entities.Ncp.Populated = true;

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
    /// A value of MatchFile.
    /// </summary>
    [JsonPropertyName("matchFile")]
    public WorkArea MatchFile
    {
      get => matchFile ??= new();
      set => matchFile = value;
    }

    /// <summary>
    /// A value of TotalNumbOfNcpsWoSsn.
    /// </summary>
    [JsonPropertyName("totalNumbOfNcpsWoSsn")]
    public Common TotalNumbOfNcpsWoSsn
    {
      get => totalNumbOfNcpsWoSsn ??= new();
      set => totalNumbOfNcpsWoSsn = value;
    }

    /// <summary>
    /// A value of NumbOfNcpsSinceChckpnt.
    /// </summary>
    [JsonPropertyName("numbOfNcpsSinceChckpnt")]
    public Common NumbOfNcpsSinceChckpnt
    {
      get => numbOfNcpsSinceChckpnt ??= new();
      set => numbOfNcpsSinceChckpnt = value;
    }

    /// <summary>
    /// A value of TotalNumbOfNcpsWritten.
    /// </summary>
    [JsonPropertyName("totalNumbOfNcpsWritten")]
    public Common TotalNumbOfNcpsWritten
    {
      get => totalNumbOfNcpsWritten ??= new();
      set => totalNumbOfNcpsWritten = value;
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
    /// A value of TextDate.
    /// </summary>
    [JsonPropertyName("textDate")]
    public TextWorkArea TextDate
    {
      get => textDate ??= new();
      set => textDate = value;
    }

    /// <summary>
    /// A value of Ncp.
    /// </summary>
    [JsonPropertyName("ncp")]
    public CsePersonsWorkSet Ncp
    {
      get => ncp ??= new();
      set => ncp = value;
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

    private WorkArea matchFile;
    private Common totalNumbOfNcpsWoSsn;
    private Common numbOfNcpsSinceChckpnt;
    private Common totalNumbOfNcpsWritten;
    private ProgramCheckpointRestart programCheckpointRestart;
    private TextWorkArea textDate;
    private CsePersonsWorkSet ncp;
    private DateWorkArea null1;
    private Common common;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private ExitStateWorkArea exitStateWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    /// <summary>
    /// A value of Ncp.
    /// </summary>
    [JsonPropertyName("ncp")]
    public CsePerson Ncp
    {
      get => ncp ??= new();
      set => ncp = value;
    }

    private CaseRole caseRole;
    private CsePerson ncp;
  }
#endregion
}
