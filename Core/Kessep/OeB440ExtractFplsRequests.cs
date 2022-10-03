// Program: OE_B440_EXTRACT_FPLS_REQUESTS, ID: 372362293, model: 746.
// Short name: SWEE440B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_B440_EXTRACT_FPLS_REQUESTS.
/// </para>
/// <para>
/// Resp:OBLMGMT
/// This Procedure(PRAD) is designed to determine if a request should be send to
/// the Federal Parent Locator Service (FPLS), if the decision is yes then a
/// FPLS_REQUEST row does not exist this procedure will create the row.
/// Finally this procedure will cause a record to be written to a batch routine 
/// which creates a tape for transmission to FPLS using an External Action
/// Block.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class OeB440ExtractFplsRequests: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B440_EXTRACT_FPLS_REQUESTS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB440ExtractFplsRequests(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB440ExtractFplsRequests.
  /// </summary>
  public OeB440ExtractFplsRequests(IContext context, Import import,
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
    // ************************************************
    // Date	 Developer	Description
    // 01/13/96 T.O.Redmond    Initial Creation
    // 04/26/96 T.O.Redmond	Modify Rules - FPLS send
    // 05/27/96 T.O.Redmond	Use new Batch Si-Read
    // 06/27/96 T.O.Redmond	Remove batch Write
    // 07/18/96 T.O.Redmond	Remove Task/Plan Task from check fpls criteria
    // 1/2/98   Siraj Konkader
    // Modified calls to Create_Program_Error, Create_Program_Control_Total - 
    // removed persistent views beacuse of performance problems. Also removed
    // calls to assign_program_error_id and  assign_program_control_total_id.
    // The function of these cabs were to set the identifiers of Program Error
    // Id and Program Control Total.  However, since both the above tables are
    // used in conjunction with Program Run, the identifiers will always start
    // from 1 and not any "last used" value + 1.
    // ************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.NullDate.Date = null;
    UseOeCabSetMnemonics();

    // ***********************************************
    // *Get the run parameters for this program.     *
    // ***********************************************
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ************************************************
    // *Record the start time of this program.        *
    // ************************************************
    UseCreateProgramRun();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ***********************************************
    // *Get the DB2 commit frequency counts.         *
    // ***********************************************
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // -----------------------------------------------------------
    // 4.10.100
    // Beginning of Change
    // Open Error Report DDNAME=RPT99 and Control Report DDNAME = RPT98 .
    // -----------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    local.EabReportSend.ProgramName = "SWEEB440";
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered opening control report.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -----------------------------------------------------------
    // 4.10.100
    // End of Change
    // -----------------------------------------------------------
    if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      local.TempCheckpointRestart.RestartInfo = "";
    }
    else
    {
      // ************************************************
      // *Initialize the checkpoint count since this is *
      // *the initial submission of this program.       *
      // *Without doing this the counter would contain  *
      // *the number of checkpoints from the last       *
      // *program execution.                            *
      // ************************************************
      local.ProgramCheckpointRestart.CheckpointCount = 0;
    }

    // ************************************************
    // *Read the program_run each time we come into   *
    // *this program so that we will have currency for*
    // *creating any error rows or control total rows.*
    // ************************************************
    if (ReadProgramRun())
    {
      // ***********************************************
      // *Get the next error number and control total  *
      // *number so that it can be incremented below.  *
      // *Only want to read the database once to get   *
      // *the next number, not every insert.           *
      // ***********************************************
      // **** Above stmt incorrect. Deleted calls to cabs. See modification log
      // ... SAK 1/2/98
    }
    else
    {
      ExitState = "PROGRAM_RUN_NF_RB";

      return;
    }

    // ***********************************************
    // * Process the selected records in groups based*
    // * upon the commit frequencies. Do a DB2 commit*
    // * at the end of each group.                   *
    // ***********************************************
    local.ProgramCheckpointRestart.CheckpointCount = 0;
    local.NoOfCasesInput.Count = 0;
    local.NoOfCasesNoActNeeded.Count = 0;

    // ************************************************
    // *On a Restart - We may be starting on a case   *
    // *that has already been processed and could be  *
    // *starting with an AP that has already been     *
    // *processed, however later logic will prevent   *
    // *duplicates from being send based upon the date*
    // *sent.
    // 
    // *
    // ************************************************
    local.RestartCase.Number = TrimEnd(local.TempCheckpointRestart.RestartInfo);

    foreach(var item in ReadAbsentParentCase())
    {
      local.AbsentParent.Type1 = entities.ExistingAbsentParent.Type1;
      local.Case1.Number = entities.ExistingCase.Number;
      ++local.NoOfCasesInput.Count;
      local.ActionNeeded.ActionEntry = "";
      ++local.NumberOfReads.Count;

      if (ReadCsePerson())
      {
        local.CsePerson.Number = entities.ExistingCsePerson.Number;
      }
      else
      {
        // -----------------------------------------------------------
        // 4.10.100
        // Beginning of Change
        // Write error to error report.
        // -----------------------------------------------------------
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "FPLS :  CSE Person not found.";
        UseCabErrorReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
        else
        {
          continue;
        }

        // -----------------------------------------------------------
        // 4.10.100
        // End of Change
        // -----------------------------------------------------------
      }

      // ************************************************
      // *If any of the following statements are true   *
      // *then we do not send a FPLS Request.           *
      // ************************************************
      local.CsePersonsWorkSet.Number = entities.ExistingCsePerson.Number;
      UseSiReadCsePersonBatch();

      // ************************************************
      // *SSN, DOB,Name (2 out of 3 must exist) *
      // ************************************************
      if (!IsEmpty(local.CsePersonsWorkSet.Ssn) && StringToNumber
        (local.CsePersonsWorkSet.Ssn) != 0)
      {
        if (!Equal(local.CsePersonsWorkSet.Dob, local.NullDate.Date))
        {
          // ********continue*******
        }
        else if (IsEmpty(local.CsePersonsWorkSet.FirstName) || IsEmpty
          (local.CsePersonsWorkSet.LastName))
        {
          continue;
        }
      }
      else if (!Equal(local.CsePersonsWorkSet.Dob, local.NullDate.Date))
      {
        if (IsEmpty(local.CsePersonsWorkSet.FirstName) || IsEmpty
          (local.CsePersonsWorkSet.LastName))
        {
          continue;
        }
      }
      else
      {
        continue;
      }

      // ************************************************
      // *If the CSE Person is Incarcerated then we do  *
      // *not send a FPLS Request.                      *
      // ************************************************
      if (ReadIncarceration())
      {
        ++local.NoOfCasesNoActNeeded.Count;

        continue;
      }

      // ************************************************
      // *If the CSE Person is Dead then we do not send *
      // *a FPLS Request.
      // 
      // *
      // ************************************************
      if (Lt(local.NullDate.Date, entities.ExistingCsePerson.DateOfDeath))
      {
        ++local.NoOfCasesNoActNeeded.Count;

        continue;
      }

      // ------------------------------------------------------------
      // 4.10.100
      // Beginning Of Change
      // Since Zdel_start_date and Zdel_verified_code are going to be removed 
      // from entity,following statements are commented out and a
      // si_get_cse_person_mailing_address is used to find out current address
      // for CSE Person.
      // -------------------------------------------------------------
      // ************************************************
      // *If we have either a current residential or    *
      // *mailing address, then do not send a FPLS      *
      // *Request.
      // 
      // *
      // ************************************************
      UseSiGetCsePersonMailingAddr();

      if (!IsEmpty(local.CsePersonAddress.LocationType))
      {
        if (Lt(local.NullDate.Date, local.CsePersonAddress.VerifiedDate) && !
          Lt(Now().Date, local.CsePersonAddress.VerifiedDate) && !
          Lt(local.CsePersonAddress.EndDate, Now().Date))
        {
          ++local.NoOfCasesNoActNeeded.Count;

          continue;
        }
      }

      // --------------------------------------------------------
      // 4.10.100
      // End Of Change
      // --------------------------------------------------------
      // ************************************************
      // *If we have a validated INCOME SOURCE do not   *
      // *submit a FPLS Request.                        *
      // ************************************************
      if (ReadIncomeSource())
      {
        ++local.NoOfCasesNoActNeeded.Count;

        continue;
      }

      UseOeCheckFplsRequestCriteria();

      // ************************************************
      // *Determine what action should be taken next.   *
      // *Value "N" -- No further action required       *
      // *Value "S" -- Create a New request and send    *
      // ************************************************
      switch(TrimEnd(local.ActionNeeded.ActionEntry))
      {
        case "N":
          ++local.NoOfCasesNoActNeeded.Count;

          break;
        case "S":
          // ******************************************
          // *Create a New Request                    *
          // ******************************************
          UseOeCreateFplsLocateReqBatch();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            // -----------------------------------------------------------
            // 4.10.100
            // Beginning of Change
            // Write error to error report.
            // -----------------------------------------------------------
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Error Creating New Request , CSE Person Number :" + local
              .CsePerson.Number;
            UseCabErrorReport1();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            // -----------------------------------------------------------
            // 4.10.100
            // End of Change
            // -----------------------------------------------------------
          }
          else
          {
            ++local.NoOfFplsRequestsSent.Count;
            ++local.NumberOfUpdates.Count;
            UseUpdatePgmCheckpointRestart2();
          }

          break;
        default:
          break;
      }

      // ************************************************
      // *Check the number of reads, and updates that   *
      // *have occurred since the last checkpoint.      *
      // ************************************************
      if (local.NumberOfReads.Count >= local
        .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault() || local
        .NumberOfUpdates.Count >= local
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        local.NumberOfReads.Count = 0;
        local.NumberOfUpdates.Count = 0;

        // ************************************************
        // *Call an external that does a DB2 commit using *
        // *a Cobol program.
        // 
        // *
        // ************************************************
        UseExtToDoACommit();

        if (local.PassArea.NumericReturnCode != 0)
        {
          ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

          return;
        }
      }
    }

    // ************************************************
    // *Generate Control Totals                       *
    // ************************************************
    // -----------------------------------------------------------------
    // 4.10.100
    // Beginning Of Change
    // Write all totals to Control Report
    // -----------------------------------------------------------------
    local.EabReportSend.RptDetail = "FPLS Input AP Records Count       :" + NumberToString
      (local.NoOfCasesInput.Count, 15);
    local.EabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while writting control report(number of request read).";
        
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "FPLS No Action Needed Count       :" + NumberToString
      (local.NoOfCasesNoActNeeded.Count, 15);
    local.EabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while writting control report(number of request read).";
        
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail =
      "FPLS Number Of Requests Created Count       :" + NumberToString
      (local.NoOfFplsRequestsSent.Count, 15);
    local.EabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while writting control report(number of request read).";
        
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -----------------------------------------------------------------
    // 4.10.100
    // End Of Change
    // -----------------------------------------------------------------
    // ************************************************
    // *Record the program end time.                  *
    // ************************************************
    UseUpdateProgramRun();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ***********************************************
    // *Set restart indicator to no because we       *
    // *successfully finished this program.          *
    // ***********************************************
    local.ProgramCheckpointRestart.RestartInd = "N";
    UseUpdatePgmCheckpointRestart1();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ---------------------------------------------------------------
    // 4.10.100
    // Beginning Of Change
    // Close Error Report and Control Report files.
    // ---------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while closing control report.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabErrorReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ---------------------------------------------------------------
    // 4.10.100
    // End Of Change
    // ---------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
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

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCreateProgramRun()
  {
    var useImport = new CreateProgramRun.Import();
    var useExport = new CreateProgramRun.Export();

    MoveProgramProcessingInfo(local.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);

    Call(CreateProgramRun.Execute, useImport, useExport);

    local.ProgramRun.StartTimestamp = useExport.ProgramRun.StartTimestamp;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.PassArea.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.PassArea.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseOeCabSetMnemonics()
  {
    var useImport = new OeCabSetMnemonics.Import();
    var useExport = new OeCabSetMnemonics.Export();

    Call(OeCabSetMnemonics.Execute, useImport, useExport);

    local.MaxDate.ExpirationDate = useExport.MaxDate.ExpirationDate;
  }

  private void UseOeCheckFplsRequestCriteria()
  {
    var useImport = new OeCheckFplsRequestCriteria.Import();
    var useExport = new OeCheckFplsRequestCriteria.Export();

    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;
    useImport.CsePerson.Number = local.CsePerson.Number;
    useImport.Case1.Number = local.Case1.Number;

    Call(OeCheckFplsRequestCriteria.Execute, useImport, useExport);

    local.FplsLocateRequest.Identifier = useExport.FplsLocateRequest.Identifier;
    local.CsePerson.Number = useExport.CsePerson.Number;
    local.ActionNeeded.ActionEntry = useExport.ActionNeeded.ActionEntry;
  }

  private void UseOeCreateFplsLocateReqBatch()
  {
    var useImport = new OeCreateFplsLocateReqBatch.Import();
    var useExport = new OeCreateFplsLocateReqBatch.Export();

    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;
    useImport.BatchRequestIndicator.Flag = export.LocalFplsBatch.Flag;
    useImport.CsePerson.Number = local.CsePerson.Number;

    Call(OeCreateFplsLocateReqBatch.Execute, useImport, useExport);

    local.FplsLocateRequest.Identifier = useExport.FplsLocateRequest.Identifier;
    export.ExternalFplsRequest.Assign(useExport.ExternalFplsRequest);
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

  private void UseSiGetCsePersonMailingAddr()
  {
    var useImport = new SiGetCsePersonMailingAddr.Import();
    var useExport = new SiGetCsePersonMailingAddr.Export();

    useImport.CsePerson.Number = entities.ExistingCsePerson.Number;

    Call(SiGetCsePersonMailingAddr.Execute, useImport, useExport);

    local.CsePersonAddress.Assign(useExport.CsePersonAddress);
  }

  private void UseSiReadCsePersonBatch()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseUpdatePgmCheckpointRestart1()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);
  }

  private void UseUpdatePgmCheckpointRestart2()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }

  private void UseUpdateProgramRun()
  {
    var useImport = new UpdateProgramRun.Import();
    var useExport = new UpdateProgramRun.Export();

    useImport.ProgramRun.StartTimestamp = local.ProgramRun.StartTimestamp;
    MoveProgramProcessingInfo(local.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);

    Call(UpdateProgramRun.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadAbsentParentCase()
  {
    entities.ExistingAbsentParent.Populated = false;
    entities.ExistingCase.Populated = false;

    return ReadEach("ReadAbsentParentCase",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "endDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetString(command, "numb", local.RestartCase.Number);
      },
      (db, reader) =>
      {
        entities.ExistingAbsentParent.CasNumber = db.GetString(reader, 0);
        entities.ExistingCase.Number = db.GetString(reader, 0);
        entities.ExistingAbsentParent.CspNumber = db.GetString(reader, 1);
        entities.ExistingAbsentParent.Type1 = db.GetString(reader, 2);
        entities.ExistingAbsentParent.Identifier = db.GetInt32(reader, 3);
        entities.ExistingAbsentParent.StartDate = db.GetNullableDate(reader, 4);
        entities.ExistingAbsentParent.EndDate = db.GetNullableDate(reader, 5);
        entities.ExistingCase.Status = db.GetNullableString(reader, 6);
        entities.ExistingCase.CseOpenDate = db.GetNullableDate(reader, 7);
        entities.ExistingAbsentParent.Populated = true;
        entities.ExistingCase.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingAbsentParent.Type1);

        return true;
      });
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingAbsentParent.Populated);
    entities.ExistingCsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.ExistingAbsentParent.CspNumber);
      },
      (db, reader) =>
      {
        entities.ExistingCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingCsePerson.Type1 = db.GetString(reader, 1);
        entities.ExistingCsePerson.DateOfDeath = db.GetNullableDate(reader, 2);
        entities.ExistingCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ExistingCsePerson.Type1);
      });
  }

  private bool ReadIncarceration()
  {
    entities.ExistingIncarceration.Populated = false;

    return Read("ReadIncarceration",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
        db.SetNullableDate(
          command, "startDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingIncarceration.CspNumber = db.GetString(reader, 0);
        entities.ExistingIncarceration.Identifier = db.GetInt32(reader, 1);
        entities.ExistingIncarceration.VerifiedDate =
          db.GetNullableDate(reader, 2);
        entities.ExistingIncarceration.EndDate = db.GetNullableDate(reader, 3);
        entities.ExistingIncarceration.StartDate =
          db.GetNullableDate(reader, 4);
        entities.ExistingIncarceration.Populated = true;
      });
  }

  private bool ReadIncomeSource()
  {
    entities.ExistingIncomeSource.Populated = false;

    return Read("ReadIncomeSource",
      (db, command) =>
      {
        db.SetString(command, "cspINumber", entities.ExistingCsePerson.Number);
        db.SetNullableDate(
          command, "endDt",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "returnDt", local.NullDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingIncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.ExistingIncomeSource.Type1 = db.GetString(reader, 1);
        entities.ExistingIncomeSource.ReturnDt = db.GetNullableDate(reader, 2);
        entities.ExistingIncomeSource.ReturnCd =
          db.GetNullableString(reader, 3);
        entities.ExistingIncomeSource.CspINumber = db.GetString(reader, 4);
        entities.ExistingIncomeSource.StartDt = db.GetNullableDate(reader, 5);
        entities.ExistingIncomeSource.EndDt = db.GetNullableDate(reader, 6);
        entities.ExistingIncomeSource.Populated = true;
        CheckValid<IncomeSource>("Type1", entities.ExistingIncomeSource.Type1);
      });
  }

  private bool ReadProgramRun()
  {
    entities.ExistingProgramRun.Populated = false;

    return Read("ReadProgramRun",
      (db, command) =>
      {
        db.SetDateTime(
          command, "startTimestamp",
          local.ProgramRun.StartTimestamp.GetValueOrDefault());
        db.SetString(command, "ppiName", global.UserId);
        db.SetDateTime(
          command, "ppiCreatedTstamp",
          local.ProgramProcessingInfo.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingProgramRun.PpiCreatedTstamp =
          db.GetDateTime(reader, 0);
        entities.ExistingProgramRun.PpiName = db.GetString(reader, 1);
        entities.ExistingProgramRun.StartTimestamp = db.GetDateTime(reader, 2);
        entities.ExistingProgramRun.Populated = true;
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
    /// <summary>
    /// A value of ExternalFplsRequest.
    /// </summary>
    [JsonPropertyName("externalFplsRequest")]
    public ExternalFplsRequest ExternalFplsRequest
    {
      get => externalFplsRequest ??= new();
      set => externalFplsRequest = value;
    }

    /// <summary>
    /// A value of LocalFplsBatch.
    /// </summary>
    [JsonPropertyName("localFplsBatch")]
    public Common LocalFplsBatch
    {
      get => localFplsBatch ??= new();
      set => localFplsBatch = value;
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

    private ExternalFplsRequest externalFplsRequest;
    private Common localFplsBatch;
    private External external;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
    }

    /// <summary>
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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
    /// A value of TempCheckpointRestart.
    /// </summary>
    [JsonPropertyName("tempCheckpointRestart")]
    public TempCheckpointRestart TempCheckpointRestart
    {
      get => tempCheckpointRestart ??= new();
      set => tempCheckpointRestart = value;
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
    /// A value of RestartCase.
    /// </summary>
    [JsonPropertyName("restartCase")]
    public Case1 RestartCase
    {
      get => restartCase ??= new();
      set => restartCase = value;
    }

    /// <summary>
    /// A value of NoOfCasesNoActNeeded.
    /// </summary>
    [JsonPropertyName("noOfCasesNoActNeeded")]
    public Common NoOfCasesNoActNeeded
    {
      get => noOfCasesNoActNeeded ??= new();
      set => noOfCasesNoActNeeded = value;
    }

    /// <summary>
    /// A value of NoOfFplsRequestsSent.
    /// </summary>
    [JsonPropertyName("noOfFplsRequestsSent")]
    public Common NoOfFplsRequestsSent
    {
      get => noOfFplsRequestsSent ??= new();
      set => noOfFplsRequestsSent = value;
    }

    /// <summary>
    /// A value of NoOfCasesInput.
    /// </summary>
    [JsonPropertyName("noOfCasesInput")]
    public Common NoOfCasesInput
    {
      get => noOfCasesInput ??= new();
      set => noOfCasesInput = value;
    }

    /// <summary>
    /// A value of ProgramRun.
    /// </summary>
    [JsonPropertyName("programRun")]
    public ProgramRun ProgramRun
    {
      get => programRun ??= new();
      set => programRun = value;
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
    /// A value of PassArea.
    /// </summary>
    [JsonPropertyName("passArea")]
    public External PassArea
    {
      get => passArea ??= new();
      set => passArea = value;
    }

    /// <summary>
    /// A value of NumberOfUpdates.
    /// </summary>
    [JsonPropertyName("numberOfUpdates")]
    public Common NumberOfUpdates
    {
      get => numberOfUpdates ??= new();
      set => numberOfUpdates = value;
    }

    /// <summary>
    /// A value of NumberOfReads.
    /// </summary>
    [JsonPropertyName("numberOfReads")]
    public Common NumberOfReads
    {
      get => numberOfReads ??= new();
      set => numberOfReads = value;
    }

    /// <summary>
    /// A value of ProgramError.
    /// </summary>
    [JsonPropertyName("programError")]
    public ProgramError ProgramError
    {
      get => programError ??= new();
      set => programError = value;
    }

    /// <summary>
    /// A value of ProgramControlTotal.
    /// </summary>
    [JsonPropertyName("programControlTotal")]
    public ProgramControlTotal ProgramControlTotal
    {
      get => programControlTotal ??= new();
      set => programControlTotal = value;
    }

    /// <summary>
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public Code MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

    /// <summary>
    /// A value of AbsentParent.
    /// </summary>
    [JsonPropertyName("absentParent")]
    public CaseRole AbsentParent
    {
      get => absentParent ??= new();
      set => absentParent = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of ActionNeeded.
    /// </summary>
    [JsonPropertyName("actionNeeded")]
    public Common ActionNeeded
    {
      get => actionNeeded ??= new();
      set => actionNeeded = value;
    }

    /// <summary>
    /// A value of FplsLocateRequest.
    /// </summary>
    [JsonPropertyName("fplsLocateRequest")]
    public FplsLocateRequest FplsLocateRequest
    {
      get => fplsLocateRequest ??= new();
      set => fplsLocateRequest = value;
    }

    /// <summary>
    /// A value of FileOpened.
    /// </summary>
    [JsonPropertyName("fileOpened")]
    public Common FileOpened
    {
      get => fileOpened ??= new();
      set => fileOpened = value;
    }

    /// <summary>
    /// A value of ExternalFplsRequest.
    /// </summary>
    [JsonPropertyName("externalFplsRequest")]
    public ExternalFplsRequest ExternalFplsRequest
    {
      get => externalFplsRequest ??= new();
      set => externalFplsRequest = value;
    }

    /// <summary>
    /// A value of TotalRequestsRead.
    /// </summary>
    [JsonPropertyName("totalRequestsRead")]
    public Common TotalRequestsRead
    {
      get => totalRequestsRead ??= new();
      set => totalRequestsRead = value;
    }

    private CsePersonAddress csePersonAddress;
    private EabReportSend neededToWrite;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private CsePersonsWorkSet csePersonsWorkSet;
    private DateWorkArea nullDate;
    private TempCheckpointRestart tempCheckpointRestart;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Case1 restartCase;
    private Common noOfCasesNoActNeeded;
    private Common noOfFplsRequestsSent;
    private Common noOfCasesInput;
    private ProgramRun programRun;
    private ProgramProcessingInfo programProcessingInfo;
    private External passArea;
    private Common numberOfUpdates;
    private Common numberOfReads;
    private ProgramError programError;
    private ProgramControlTotal programControlTotal;
    private Code maxDate;
    private CaseRole absentParent;
    private Case1 case1;
    private CsePerson csePerson;
    private Common actionNeeded;
    private FplsLocateRequest fplsLocateRequest;
    private Common fileOpened;
    private ExternalFplsRequest externalFplsRequest;
    private Common totalRequestsRead;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingIncarceration.
    /// </summary>
    [JsonPropertyName("existingIncarceration")]
    public Incarceration ExistingIncarceration
    {
      get => existingIncarceration ??= new();
      set => existingIncarceration = value;
    }

    /// <summary>
    /// A value of ExistingIncomeSource.
    /// </summary>
    [JsonPropertyName("existingIncomeSource")]
    public IncomeSource ExistingIncomeSource
    {
      get => existingIncomeSource ??= new();
      set => existingIncomeSource = value;
    }

    /// <summary>
    /// A value of ExistingProgramRun.
    /// </summary>
    [JsonPropertyName("existingProgramRun")]
    public ProgramRun ExistingProgramRun
    {
      get => existingProgramRun ??= new();
      set => existingProgramRun = value;
    }

    /// <summary>
    /// A value of ExistingProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("existingProgramProcessingInfo")]
    public ProgramProcessingInfo ExistingProgramProcessingInfo
    {
      get => existingProgramProcessingInfo ??= new();
      set => existingProgramProcessingInfo = value;
    }

    /// <summary>
    /// A value of ExistingAbsentParent.
    /// </summary>
    [JsonPropertyName("existingAbsentParent")]
    public CaseRole ExistingAbsentParent
    {
      get => existingAbsentParent ??= new();
      set => existingAbsentParent = value;
    }

    /// <summary>
    /// A value of ExistingCsePerson.
    /// </summary>
    [JsonPropertyName("existingCsePerson")]
    public CsePerson ExistingCsePerson
    {
      get => existingCsePerson ??= new();
      set => existingCsePerson = value;
    }

    /// <summary>
    /// A value of ExistingCase.
    /// </summary>
    [JsonPropertyName("existingCase")]
    public Case1 ExistingCase
    {
      get => existingCase ??= new();
      set => existingCase = value;
    }

    /// <summary>
    /// A value of ExistingAp.
    /// </summary>
    [JsonPropertyName("existingAp")]
    public CsePerson ExistingAp
    {
      get => existingAp ??= new();
      set => existingAp = value;
    }

    private Incarceration existingIncarceration;
    private IncomeSource existingIncomeSource;
    private ProgramRun existingProgramRun;
    private ProgramProcessingInfo existingProgramProcessingInfo;
    private CaseRole existingAbsentParent;
    private CsePerson existingCsePerson;
    private Case1 existingCase;
    private CsePerson existingAp;
  }
#endregion
}
