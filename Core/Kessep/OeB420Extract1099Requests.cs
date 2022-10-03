// Program: OE_B420_EXTRACT_1099_REQUESTS, ID: 371801915, model: 746.
// Short name: SWEE420B
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
/// A program: OE_B420_EXTRACT_1099_REQUESTS.
/// </para>
/// <para>
/// Resp:OBLGEST
/// This Procedure(PRAD) determines via selection criteria if a request for 1099
/// information should be sent to the IRS. Once determined it will cause a
/// 1099_LOCATE_REQUEST to be created(If it does not exist).
/// We will generate a 1099_LOCATE_REQUEST for transmittal to IRS using a 
/// different procedure OE WRITE 1099 requests to tape(This procedure calls an
/// External Action Block).
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class OeB420Extract1099Requests: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B420_EXTRACT_1099_REQUESTS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB420Extract1099Requests(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB420Extract1099Requests.
  /// </summary>
  public OeB420Extract1099Requests(IContext context, Import import,
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
    // 05/27/96 T.O.Redmond	Use new Batch Si-Read
    // 06/27/96 T.O.Redmond	Split Write from Extract
    // 1/2/98   Siraj Konkader
    // Modified calls to Create_Program_Error, Create_Program_Control_Total - 
    // removed persistent views beacuse of performance problems.
    // Also removed calls to assign_program_error_id and  
    // assign_program_control_total_id. The function of these cabs were to set
    // the identifiers of Program Error Id and Program Control Total.  However,
    // since both the above tables are used in conjunction with Program Run, the
    // identifiers will always start from 1 and not any "last used" value + 1.
    // 11/11/98 D. King  Removed READ EACH and replaced with READ statement when
    // checking for existing 1099 requests to improve performance
    // ************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    // ***********************************************
    // *Get the run parameters for this program.     *
    // ***********************************************
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }
    else
    {
      local.SixMonthPrior.RequestSentDate =
        AddMonths(local.ProgramProcessingInfo.ProcessDate, -6);
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
    // *Get the DB2 commit frequency counts, and     *
    // *determine if we are in a restart situation.  *
    // ***********************************************
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
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

    // -------------------------------------------------------------
    // 4.9.100
    // Beginning Of Change
    // Open Error Report DDNAME=RPT99 and Control Report DDNAME = RPT98.
    // -------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    local.EabReportSend.ProgramName = "SWEEB420";
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
        "Error encountered opening Control Report.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------
    // 4.9.100
    // End Of Change
    // -------------------------------------------------------------
    // ************************************************
    // *Read all Open CASE's.                         *
    // ************************************************
    foreach(var item in ReadCase())
    {
      ++local.NumberOfOpenCasesRead.Count;

      // ************************************************
      // *Read the CSE_PERSON that fills the role of AP *
      // *on an Open Case.
      // 
      // *
      // ************************************************
      foreach(var item1 in ReadCaseRoleCsePerson())
      {
        ++local.NumberOfReads.Count;

        // *******************************************************
        // Removed Read Each using date function  and replaced
        // with single READ using local variable
        // D. King CMI 11/11/98
        //  
        // *******************************************************
        if (Read1099LocateRequest2())
        {
          continue;
        }
        else
        {
          // *********** Continue
        }

        // ************************************************
        // *Decide if there is already an existing 1099   *
        // *Request that has not been sent to the IRS.  If*
        // *we do have an existing request then we do not *
        // *want to send a new one. This logic also works *
        // *well should this process abort so that we do  *
        // *not end up creating multiples.                *
        // ************************************************
        if (Read1099LocateRequest1())
        {
          continue;
        }
        else
        {
          export.CsePersonsWorkSet.Number = entities.ExistingApCsePerson.Number;
          UseSiReadCsePersonBatch();

          // ************************************************
          // *Do NOT send a Request if the SSN is = spaces  *
          // ************************************************
          if (IsEmpty(export.CsePersonsWorkSet.Ssn) || !
            Lt("000000000", export.CsePersonsWorkSet.Ssn))
          {
            continue;
          }

          local.New1.Identifier = 0;
          local.New1.CaseIdNo = entities.ExistingApCase.Number;
          local.New1.FirstName = export.CsePersonsWorkSet.FirstName;
          local.New1.Ssn = export.CsePersonsWorkSet.Ssn;
          local.New1.MiddleInitial = export.CsePersonsWorkSet.MiddleInitial;
          local.New1.LastName = export.CsePersonsWorkSet.LastName;
          local.New1.RequestSentDate = local.NullDate.Date;
          UseOeCreate1099LocateBatch();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ++local.NumberOfUpdates.Count;
            local.ProgramCheckpointRestart.CheckpointCount =
              local.ProgramCheckpointRestart.CheckpointCount.
                GetValueOrDefault() + 1;
            local.TempCheckpointRestart.ProgramName =
              local.ProgramCheckpointRestart.ProgramName;
            UseUpdatePgmCheckpointRestart();
          }
          else if (IsExitState("SI0000_PERSON_PROGRAM_CASE_NF"))
          {
            // --------------------------------------------------------------
            // 4.9.100
            // Beginning Of Change
            // Remove Create_program_error  and
            // use Cab_error_report to write error to Error Report.
            // ---------------------------------------------------------------
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "1099 Request : 'No Person Programs are currently active for this case', CSE Person Number : " +
              entities.ExistingApCsePerson.Number + "Case Number : " + entities
              .ExistingApCase.Number;
            UseCabErrorReport1();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            continue;

            // --------------------------------------------------------------
            // 4.9.100
            // End Of Change
            // ---------------------------------------------------------------
          }
          else
          {
            // --------------------------------------------------------------
            // 4.9.100
            // Beginning Of Change
            // Remove Create_program_error  and
            // use Cab_error_report to write error to Error Report.
            // ---------------------------------------------------------------
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "1099 Request : Error in CAB oe_create_1099_locate_batch, CSE Person Number : " +
              entities.ExistingApCsePerson.Number + "Case Number : " + entities
              .ExistingApCase.Number;
            UseCabErrorReport1();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            continue;

            // --------------------------------------------------------------
            // 4.9.100
            // End Of Change
            // ---------------------------------------------------------------
          }

          if (local.NumberOfReads.Count >= local
            .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault() ||
            local.NumberOfUpdates.Count >= local
            .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
          {
            local.TotalNumberOfReads.Count += local.NumberOfReads.Count;
            local.TotalNumberOfUpdates.Count += local.NumberOfUpdates.Count;
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
      }
    }

    // ************************************************
    // *Final Commit.
    // 
    // *
    // ************************************************
    local.TotalNumberOfReads.Count += local.NumberOfReads.Count;
    local.TotalNumberOfUpdates.Count += local.NumberOfUpdates.Count;
    UseExtToDoACommit();

    if (local.PassArea.NumericReturnCode != 0)
    {
      ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

      return;
    }

    // ************************************************
    // *Generate Control Totals                       *
    // ************************************************
    // --------------------------------------------------------------
    // 4.9.100
    // Beginning Of Change
    // Remove Create_program_control_total  and
    // use Cab_control_report to write control totals to Control Report.
    // ---------------------------------------------------------------
    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail =
      "1099 : Number Of 1099 Requests Created : " + NumberToString
      (local.TotalNumberOfUpdates.Count, 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while writing Control Report (Number Of 1099 Requests Created).";
        
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // --------------------------------------------------------------
    // 4.9.100
    // End Of Change
    // ---------------------------------------------------------------
    // --------------------------------------------------------------
    // 4.9.100
    // Beginning Of Change
    // Remove Create_program_control_total  and
    // use Cab_control_report to write control totals to Control Report.
    // ---------------------------------------------------------------
    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail = "1099 : Number Of AP Records Read : " + NumberToString
      (local.TotalNumberOfReads.Count, 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while writing Control Report (Number Of AP Records Read).";
        
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // --------------------------------------------------------------
    // 4.9.100
    // End Of Change
    // ---------------------------------------------------------------
    // --------------------------------------------------------------
    // 4.9.100
    // Beginning Of Change
    // Remove Create_program_control_total  and
    // use Cab_control_report to write control totals to Control Report.
    // ---------------------------------------------------------------
    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail = "1099 : Number Of Open Cases Read : " + NumberToString
      (local.NumberOfOpenCasesRead.Count, 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while writing Control Report (Number Of Open Cases Read).";
        
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // --------------------------------------------------------------
    // 4.9.100
    // End Of Change
    // ---------------------------------------------------------------
    // ***********************************************
    // *Record the Program run date and time.        *
    // ***********************************************
    UseUpdateProgramRun();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      // --------------------------------------------------------------
      // 4.9.100
      // Beginning Of Change
      // Write error to error report.
      // ---------------------------------------------------------------
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while updating program run (Update_program_run).";
      UseCabErrorReport1();

      // --------------------------------------------------------------
      // 4.9.100
      // End Of Change
      // ---------------------------------------------------------------
    }

    // --------------------------------------------------------------
    // 4.9.100
    // Beginning Of Change
    // Close Error Report and Control_report files.
    // ---------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while Closing Control Report.";
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

    // --------------------------------------------------------------
    // 4.9.100
    // End Of Change
    // ---------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Sex = source.Sex;
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.FormattedName = source.FormattedName;
    target.LastName = source.LastName;
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

  private void UseOeCreate1099LocateBatch()
  {
    var useImport = new OeCreate1099LocateBatch.Import();
    var useExport = new OeCreate1099LocateBatch.Export();

    useImport.CsePerson.Number = entities.ExistingApCsePerson.Number;
    useImport.Data1099LocateRequest.Assign(local.New1);
    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;

    Call(OeCreate1099LocateBatch.Execute, useImport, useExport);
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

  private void UseSiReadCsePersonBatch()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.CsePersonsWorkSet);
      
  }

  private void UseUpdatePgmCheckpointRestart()
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

  private bool Read1099LocateRequest1()
  {
    entities.Existingdata1099LocateRequest.Populated = false;

    return Read("Read1099LocateRequest1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingApCsePerson.Number);
        db.SetNullableDate(
          command, "requestSentDate", local.NullDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Existingdata1099LocateRequest.CspNumber =
          db.GetString(reader, 0);
        entities.Existingdata1099LocateRequest.Identifier =
          db.GetInt32(reader, 1);
        entities.Existingdata1099LocateRequest.Ssn = db.GetString(reader, 2);
        entities.Existingdata1099LocateRequest.LocalCode =
          db.GetNullableString(reader, 3);
        entities.Existingdata1099LocateRequest.LastName =
          db.GetNullableString(reader, 4);
        entities.Existingdata1099LocateRequest.AfdcCode =
          db.GetNullableString(reader, 5);
        entities.Existingdata1099LocateRequest.CaseIdNo =
          db.GetNullableString(reader, 6);
        entities.Existingdata1099LocateRequest.CourtOrAdminOrdInd =
          db.GetNullableString(reader, 7);
        entities.Existingdata1099LocateRequest.NoMatchCode =
          db.GetNullableString(reader, 8);
        entities.Existingdata1099LocateRequest.FirstName =
          db.GetNullableString(reader, 9);
        entities.Existingdata1099LocateRequest.RequestSentDate =
          db.GetNullableDate(reader, 10);
        entities.Existingdata1099LocateRequest.MiddleInitial =
          db.GetNullableString(reader, 11);
        entities.Existingdata1099LocateRequest.Populated = true;
      });
  }

  private bool Read1099LocateRequest2()
  {
    entities.ExistingLast.Populated = false;

    return Read("Read1099LocateRequest2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingApCsePerson.Number);
        db.SetNullableDate(
          command, "requestSentDate",
          local.SixMonthPrior.RequestSentDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingLast.CspNumber = db.GetString(reader, 0);
        entities.ExistingLast.Identifier = db.GetInt32(reader, 1);
        entities.ExistingLast.RequestSentDate = db.GetNullableDate(reader, 2);
        entities.ExistingLast.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCase()
  {
    entities.ExistingApCase.Populated = false;

    return ReadEach("ReadCase",
      null,
      (db, reader) =>
      {
        entities.ExistingApCase.Number = db.GetString(reader, 0);
        entities.ExistingApCase.Status = db.GetNullableString(reader, 1);
        entities.ExistingApCase.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseRoleCsePerson()
  {
    entities.ExistingApCaseRole.Populated = false;
    entities.ExistingApCsePerson.Populated = false;

    return ReadEach("ReadCaseRoleCsePerson",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ExistingApCase.Number);
        db.SetNullableDate(
          command, "startDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingApCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ExistingApCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ExistingApCsePerson.Number = db.GetString(reader, 1);
        entities.ExistingApCaseRole.Type1 = db.GetString(reader, 2);
        entities.ExistingApCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ExistingApCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ExistingApCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ExistingApCaseRole.Populated = true;
        entities.ExistingApCsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingApCaseRole.Type1);

        return true;
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
    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ExternalOcse1099Request.
    /// </summary>
    [JsonPropertyName("externalOcse1099Request")]
    public ExternalOcse1099Request ExternalOcse1099Request
    {
      get => externalOcse1099Request ??= new();
      set => externalOcse1099Request = value;
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

    private ExternalOcse1099Request externalOcse1099Request;
    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of SixMonthPrior.
    /// </summary>
    [JsonPropertyName("sixMonthPrior")]
    public Data1099LocateRequest SixMonthPrior
    {
      get => sixMonthPrior ??= new();
      set => sixMonthPrior = value;
    }

    /// <summary>
    /// A value of NumberOfOpenCasesRead.
    /// </summary>
    [JsonPropertyName("numberOfOpenCasesRead")]
    public Common NumberOfOpenCasesRead
    {
      get => numberOfOpenCasesRead ??= new();
      set => numberOfOpenCasesRead = value;
    }

    /// <summary>
    /// A value of TotalNumberOfUpdates.
    /// </summary>
    [JsonPropertyName("totalNumberOfUpdates")]
    public Common TotalNumberOfUpdates
    {
      get => totalNumberOfUpdates ??= new();
      set => totalNumberOfUpdates = value;
    }

    /// <summary>
    /// A value of TotalNumberOfReads.
    /// </summary>
    [JsonPropertyName("totalNumberOfReads")]
    public Common TotalNumberOfReads
    {
      get => totalNumberOfReads ??= new();
      set => totalNumberOfReads = value;
    }

    /// <summary>
    /// A value of Local1099RequestFound.
    /// </summary>
    [JsonPropertyName("local1099RequestFound")]
    public Common Local1099RequestFound
    {
      get => local1099RequestFound ??= new();
      set => local1099RequestFound = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public Data1099LocateRequest New1
    {
      get => new1 ??= new();
      set => new1 = value;
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
    /// A value of FileOpened.
    /// </summary>
    [JsonPropertyName("fileOpened")]
    public Common FileOpened
    {
      get => fileOpened ??= new();
      set => fileOpened = value;
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
    /// A value of Data1099LocateRequest.
    /// </summary>
    [JsonPropertyName("data1099LocateRequest")]
    public Data1099LocateRequest Data1099LocateRequest
    {
      get => data1099LocateRequest ??= new();
      set => data1099LocateRequest = value;
    }

    /// <summary>
    /// A value of NoOf1099RecsCreated.
    /// </summary>
    [JsonPropertyName("noOf1099RecsCreated")]
    public Common NoOf1099RecsCreated
    {
      get => noOf1099RecsCreated ??= new();
      set => noOf1099RecsCreated = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
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
    /// A value of TempCheckpointRestart.
    /// </summary>
    [JsonPropertyName("tempCheckpointRestart")]
    public TempCheckpointRestart TempCheckpointRestart
    {
      get => tempCheckpointRestart ??= new();
      set => tempCheckpointRestart = value;
    }

    private EabReportSend neededToWrite;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private Data1099LocateRequest sixMonthPrior;
    private Common numberOfOpenCasesRead;
    private Common totalNumberOfUpdates;
    private Common totalNumberOfReads;
    private Common local1099RequestFound;
    private Data1099LocateRequest new1;
    private DateWorkArea nullDate;
    private Common fileOpened;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Data1099LocateRequest data1099LocateRequest;
    private Common noOf1099RecsCreated;
    private ProgramRun programRun;
    private ProgramProcessingInfo programProcessingInfo;
    private CsePerson csePerson;
    private Common numberOfReads;
    private ProgramError programError;
    private ProgramControlTotal programControlTotal;
    private ProgramCheckpointRestart programCheckpointRestart;
    private External passArea;
    private Common numberOfUpdates;
    private TempCheckpointRestart tempCheckpointRestart;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Existingdata1099LocateRequest.
    /// </summary>
    [JsonPropertyName("existingdata1099LocateRequest")]
    public Data1099LocateRequest Existingdata1099LocateRequest
    {
      get => existingdata1099LocateRequest ??= new();
      set => existingdata1099LocateRequest = value;
    }

    /// <summary>
    /// A value of ExistingApCaseRole.
    /// </summary>
    [JsonPropertyName("existingApCaseRole")]
    public CaseRole ExistingApCaseRole
    {
      get => existingApCaseRole ??= new();
      set => existingApCaseRole = value;
    }

    /// <summary>
    /// A value of ExistingApCsePerson.
    /// </summary>
    [JsonPropertyName("existingApCsePerson")]
    public CsePerson ExistingApCsePerson
    {
      get => existingApCsePerson ??= new();
      set => existingApCsePerson = value;
    }

    /// <summary>
    /// A value of ExistingApCase.
    /// </summary>
    [JsonPropertyName("existingApCase")]
    public Case1 ExistingApCase
    {
      get => existingApCase ??= new();
      set => existingApCase = value;
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
    /// A value of ExistingLast.
    /// </summary>
    [JsonPropertyName("existingLast")]
    public Data1099LocateRequest ExistingLast
    {
      get => existingLast ??= new();
      set => existingLast = value;
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

    private Data1099LocateRequest existingdata1099LocateRequest;
    private CaseRole existingApCaseRole;
    private CsePerson existingApCsePerson;
    private Case1 existingApCase;
    private ProgramRun existingProgramRun;
    private ProgramProcessingInfo existingProgramProcessingInfo;
    private Data1099LocateRequest existingLast;
    private Case1 existingCase;
  }
#endregion
}
