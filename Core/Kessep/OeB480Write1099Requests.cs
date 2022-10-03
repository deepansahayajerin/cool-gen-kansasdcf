// Program: OE_B480_WRITE_1099_REQUESTS, ID: 371802985, model: 746.
// Short name: SWEE480B
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
/// A program: OE_B480_WRITE_1099_REQUESTS.
/// </para>
/// <para>
/// Resp:OBLGEST
/// This Procedure(PRAD) reads all 1099_REQUESTS and writes an External Format 
/// for transmission to the IRS, for all those Requests that have not previously
/// been sent. The REQUEST_SENT_DATE will be updated to reflect the fact that
/// the request has now been sent.
/// Should this procedure abort for any reason it can be restarted successfully 
/// within a 5 day period.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class OeB480Write1099Requests: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B480_WRITE_1099_REQUESTS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB480Write1099Requests(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB480Write1099Requests.
  /// </summary>
  public OeB480Write1099Requests(IContext context, Import import, Export export):
    
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
    // 06/27/96 T.O.Redmond 	Only include Write/no Restart
    // 10/28/98 D. King               Modified name field to include
    //                                            
    // middle initial
    // ************************************************
    // ************************************************
    // *For IEF Developers. Please note that all of   *
    // *the Read Each statements in this procedure are*
    // *coded to use CURSOR with Hold.This is required*
    // *to enable the Commit to occur.                *
    // ************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    // ***********************************************
    // *Set External File flags to Initial State.    *
    // ***********************************************
    local.FileOpened.Flag = "N";

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
    // *Get the DB2 commit frequency counts, and     *
    // *determine if we are in a restart situation.  *
    // ***********************************************
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // -------------------------------------------------------------
    // 4.9.100
    // Beginning Of Change
    // Open Error Report DDNAME=RPT99 and Control Report DDNAME = RPT98.
    // -------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    local.EabReportSend.ProgramName = "SWEEB480";
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
    // *Call External to Open the Flat File.          *
    // ************************************************
    local.PassArea.FileInstruction = "OPEN";
    UseOeEabSend1099Request1();

    if (!IsEmpty(local.PassArea.TextReturnCode))
    {
      // -----------------------------------------------------------------
      // 4.9.100
      // Beginning Of Change
      // Write error to Error Report.
      // -----------------------------------------------------------------
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered opening external output file in oe_eab_send_1099_request.";
        
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      // -----------------------------------------------------------------
      // 4.9.100
      // End Of Change
      // -----------------------------------------------------------------
      ExitState = "ZD_FILE_OPEN_ERROR_WITH_AB";

      return;
    }

    local.FileOpened.Flag = "Y";

    // ************************************************
    // *Read the program_run each time we come into   *
    // *this program so that we will have currency for*
    // *creating any error rows or control total rows.*
    // ************************************************
    if (!ReadProgramRun())
    {
      ExitState = "PROGRAM_RUN_NF_RB";

      return;
    }

    local.NoOf1099RecsCreated.Count = 0;

    foreach(var item in Read1099LocateRequest())
    {
      ++local.NumberOfReads.Count;
      local.Data1099LocateRequest.
        Assign(entities.Existingdata1099LocateRequest);

      // ************************************************
      // *Prepare 1099 OCSE Locate Request External     *
      // *Format.
      // 
      // *
      // ************************************************
      export.ExternalOcse1099Request.SubmittingState = "KS";
      export.ExternalOcse1099Request.Ssn =
        entities.Existingdata1099LocateRequest.Ssn;
      export.ExternalOcse1099Request.LocalFipsCode = 0;
      export.ExternalOcse1099Request.CaseIdNumber =
        entities.Existingdata1099LocateRequest.CaseIdNo ?? Spaces(15);
      export.ExternalOcse1099Request.LastName =
        entities.Existingdata1099LocateRequest.LastName ?? Spaces(20);
      export.ExternalOcse1099Request.FirstName =
        entities.Existingdata1099LocateRequest.FirstName ?? Spaces(15);
      export.ExternalOcse1099Request.CaseTypeAfdcNafdc =
        entities.Existingdata1099LocateRequest.AfdcCode ?? Spaces(1);
      export.ExternalOcse1099Request.CourtAdminOrderIndicator =
        entities.Existingdata1099LocateRequest.CourtOrAdminOrdInd ?? Spaces(1);
      export.ExternalOcse1099Request.Blanks = "";

      // ******************************************
      // *Write to External 1099 request Tape     *
      // ******************************************
      local.PassArea.FileInstruction = "WRITE";
      UseOeEabSend1099Request2();

      if (!IsEmpty(local.PassArea.TextReturnCode))
      {
        // -----------------------------------------------------------------
        // 4.9.100
        // Beginning Of Change
        // Write error to Error Report.
        // -----------------------------------------------------------------
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error encountered writing external output file in oe_eab_send_1099_request.";
          
        UseCabErrorReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        // -----------------------------------------------------------------
        // 4.9.100
        // End Of Change
        // -----------------------------------------------------------------
        ExitState = "FILE_WRITE_ERROR_RB";

        return;
      }

      ++local.NoOf1099RequestsSent.Count;

      // ******************************************
      // *Update 1099 Request with Current Date   *
      // ******************************************
      try
      {
        Update1099LocateRequest();
        ++local.NumberOfUpdates.Count;

        // ********************************************
        // Insert Event for 1099 request sent
        // 12/15/97 R Grey move Raise Event from prev to current
        // location in logic path to resolve Abend.
        // ********************************************
        local.Infrastructure.SituationNumber = 0;
        local.Infrastructure.EventId = 10;
        local.Infrastructure.UserId = "1099";
        local.Infrastructure.ReasonCode = "1099SENT";
        local.Infrastructure.BusinessObjectCd = "T99";
        local.Infrastructure.DenormNumeric12 =
          entities.Existingdata1099LocateRequest.Identifier;
        local.Infrastructure.ReferenceDate =
          entities.Existingdata1099LocateRequest.RequestSentDate;
        local.ConvertDateDateWorkArea.Date =
          entities.Existingdata1099LocateRequest.RequestSentDate;
        UseCabConvertDate2String();
        local.Infrastructure.ProcessStatus = "Q";
        local.Infrastructure.Detail = "1099 Locate Request Sent Date : " + TrimEnd
          (local.ConvertDateTextWorkArea.Text8);

        if (ReadCsePerson())
        {
          // --------------------------------------------------------
          // PR80718
          // Beginning Of Change
          // The following statement is commented out because infrastructure 
          // records will not be created at case_unit level but at case level
          // and that code is included here.
          // -------------------------------------------------------
          local.Infrastructure.CsePersonNumber =
            entities.ExistingCsePerson.Number;

          if (ReadCase())
          {
            local.Infrastructure.CaseNumber = entities.Case1.Number;

            if (ReadInterstateRequest())
            {
              if (AsChar(entities.InterstateRequest.KsCaseInd) == 'Y')
              {
                local.Infrastructure.InitiatingStateCode = "KS";
              }
              else
              {
                local.Infrastructure.InitiatingStateCode = "OS";
              }
            }
            else
            {
              local.Infrastructure.InitiatingStateCode = "KS";
            }

            UseSpCabCreateInfrastructure();
          }
          else
          {
            ExitState = "CASE_NF";
          }

          // --------------------------------------------------------
          // PR80718
          // End Of Change
          // -------------------------------------------------------
          // --------------------------------------------------------------
          // 4.9.100
          // Beginning Of Change
          // Write error to error report.
          // Handle error coming from oe_cab_raise_event(do not abend job).
          // ---------------------------------------------------------------
          if (IsExitState("CASE_NF"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Cann't insert event because case not found for cse person : " + entities
              .ExistingCsePerson.Number;
            UseCabErrorReport1();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            ExitState = "ACO_NN0000_ALL_OK";
          }
          else if (IsExitState("SP0000_EVENT_DETAIL_NF"))
          {
            local.EabReportSend.RptDetail =
              "Cann't insert event because event detail not found in sp_cab_create_infrastructure :" +
              entities.ExistingCsePerson.Number;
            local.EabFileHandling.Action = "WRITE";
            UseCabErrorReport1();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            ExitState = "ACO_NN0000_ALL_OK";
          }

          // --------------------------------------------------------------
          // 4.9.100
          // End Of Change
          // ---------------------------------------------------------------
          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            // ************************************************
            // *Rollback all updates to the last successfull  *
            // *commit.
            // 
            // *
            // ************************************************
            UseEabRollbackSql();

            return;
          }

          // --------------------------------------------------------------
          // 4.9.100
          // Beginning Of Change
          // Handle -904 resource unavailable caused due to 'Row level' locking 
          // on infrasturcture table, need to do commit after it reach to update
          // frequency count so that it will free page lock.
          // ---------------------------------------------------------------
          ++local.TotalUpdateCount.Count;

          if (local.TotalUpdateCount.Count >= local
            .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
          {
            UseExtToDoACommit();

            if (local.PassArea.NumericReturnCode != 0)
            {
              ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

              return;
            }

            local.TotalUpdateCount.Count = 0;
          }

          // --------------------------------------------------------------
          // 4.9.100
          // End Of Change
          // ---------------------------------------------------------------
        }

        // ***	End of code for Insert Event for 1099 request sent	***
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "SYSTEM_ERROR_HAS_OCCURRED_RB";

            return;
          case ErrorCode.PermittedValueViolation:
            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    // ************************************************
    // *Final commit.
    // 
    // *
    // ************************************************
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
    local.EabReportSend.RptDetail = "1099 : Number Of Updates : " + NumberToString
      (local.NumberOfUpdates.Count, 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while writing Control Report (Number Of Updates).";
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
    local.EabReportSend.RptDetail = "1099 : Number Of Requests Read : " + NumberToString
      (local.NumberOfReads.Count, 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while writing Control Report (Number Of Requests Read).";
        
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // --------------------------------------------------------------
    // 4.9.100
    // End Of Change
    // --------------------------------------------------------------
    // --------------------------------------------------------------
    // 4.9.100
    // Beginning Of Change
    // Remove Create_program_control_total  and
    // use Cab_control_report to write control totals to Control Report.
    // ---------------------------------------------------------------
    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail = "1099 : Number Of Requests Sent : " + NumberToString
      (local.NoOf1099RequestsSent.Count, 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while writing Control Report (Number Of Requests Sent).";
        
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // --------------------------------------------------------------
    // 4.9.100
    // End Of Change
    // ---------------------------------------------------------------
    if (AsChar(local.FileOpened.Flag) == 'Y')
    {
      // ******************************************
      // *Close External 1099 request Tape        *
      // ******************************************
      local.PassArea.FileInstruction = "CLOSE";
      UseOeEabSend1099Request1();

      if (!IsEmpty(local.PassArea.TextReturnCode))
      {
        // -----------------------------------------------------------------
        // 4.9.100
        // Beginning Of Change
        // Write error to Error Report.
        // -----------------------------------------------------------------
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error encountered closing external output file in oe_eab_send_1099_request.";
          
        UseCabErrorReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        // -----------------------------------------------------------------
        // 4.9.100
        // End Of Change
        // -----------------------------------------------------------------
        ExitState = "ZD_FILE_CLOSE_ERROR_AB";

        return;
      }
    }

    // ***********************************************
    // *Record the Program run date and time.        *
    // ***********************************************
    UseUpdateProgramRun();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while updating program run (update_program_run).";
      UseCabErrorReport1();
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

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveExternal(External source, External target)
  {
    target.NumericReturnCode = source.NumericReturnCode;
    target.TextReturnCode = source.TextReturnCode;
    target.TextLine80 = source.TextLine80;
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.Function = source.Function;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.EventType = source.EventType;
    target.EventDetailName = source.EventDetailName;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormText12 = source.DenormText12;
    target.DenormDate = source.DenormDate;
    target.DenormTimestamp = source.DenormTimestamp;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.CsenetInOutCode = source.CsenetInOutCode;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.UserId = source.UserId;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
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

  private void UseCabConvertDate2String()
  {
    var useImport = new CabConvertDate2String.Import();
    var useExport = new CabConvertDate2String.Export();

    useImport.DateWorkArea.Date = local.ConvertDateDateWorkArea.Date;

    Call(CabConvertDate2String.Execute, useImport, useExport);

    local.ConvertDateTextWorkArea.Text8 = useExport.TextWorkArea.Text8;
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

  private void UseEabRollbackSql()
  {
    var useImport = new EabRollbackSql.Import();
    var useExport = new EabRollbackSql.Export();

    useExport.External.NumericReturnCode = local.PassArea.NumericReturnCode;

    Call(EabRollbackSql.Execute, useImport, useExport);

    local.PassArea.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.PassArea.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.PassArea.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseOeEabSend1099Request1()
  {
    var useImport = new OeEabSend1099Request.Import();
    var useExport = new OeEabSend1099Request.Export();

    useImport.External.FileInstruction = local.PassArea.FileInstruction;
    useExport.External.Assign(local.PassArea);

    Call(OeEabSend1099Request.Execute, useImport, useExport);

    MoveExternal(useExport.External, local.PassArea);
  }

  private void UseOeEabSend1099Request2()
  {
    var useImport = new OeEabSend1099Request.Import();
    var useExport = new OeEabSend1099Request.Export();

    useImport.External.FileInstruction = local.PassArea.FileInstruction;
    useImport.ExternalOcse1099Request.Assign(export.ExternalOcse1099Request);
    useExport.External.Assign(local.PassArea);

    Call(OeEabSend1099Request.Execute, useImport, useExport);

    MoveExternal(useExport.External, local.PassArea);
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

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    MoveInfrastructure(useExport.Infrastructure, local.Infrastructure);
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

  private IEnumerable<bool> Read1099LocateRequest()
  {
    entities.Existingdata1099LocateRequest.Populated = false;

    return ReadEach("Read1099LocateRequest",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "processDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
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
        entities.Existingdata1099LocateRequest.LastUpdatedBy =
          db.GetString(reader, 9);
        entities.Existingdata1099LocateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 10);
        entities.Existingdata1099LocateRequest.FirstName =
          db.GetNullableString(reader, 11);
        entities.Existingdata1099LocateRequest.RequestSentDate =
          db.GetNullableDate(reader, 12);
        entities.Existingdata1099LocateRequest.MiddleInitial =
          db.GetNullableString(reader, 13);
        entities.Existingdata1099LocateRequest.Populated = true;

        return true;
      });
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspNoAp", entities.ExistingCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(
      entities.Existingdata1099LocateRequest.Populated);
    entities.ExistingCsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(
          command, "numb", entities.Existingdata1099LocateRequest.CspNumber);
      },
      (db, reader) =>
      {
        entities.ExistingCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingCsePerson.Type1 = db.GetString(reader, 1);
        entities.ExistingCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ExistingCsePerson.Type1);
      });
  }

  private bool ReadInterstateRequest()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 1);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 2);
        entities.InterstateRequest.Populated = true;
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

  private void Update1099LocateRequest()
  {
    System.Diagnostics.Debug.Assert(
      entities.Existingdata1099LocateRequest.Populated);

    var lastUpdatedBy = global.TranCode;
    var lastUpdatedTimestamp = Now();
    var requestSentDate = local.ProgramProcessingInfo.ProcessDate;

    entities.Existingdata1099LocateRequest.Populated = false;
    Update("Update1099LocateRequest",
      (db, command) =>
      {
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetNullableDate(command, "requestSentDate", requestSentDate);
        db.SetString(
          command, "cspNumber",
          entities.Existingdata1099LocateRequest.CspNumber);
        db.SetInt32(
          command, "identifier",
          entities.Existingdata1099LocateRequest.Identifier);
      });

    entities.Existingdata1099LocateRequest.LastUpdatedBy = lastUpdatedBy;
    entities.Existingdata1099LocateRequest.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.Existingdata1099LocateRequest.RequestSentDate = requestSentDate;
    entities.Existingdata1099LocateRequest.Populated = true;
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
    /// A value of TotalUpdateCount.
    /// </summary>
    [JsonPropertyName("totalUpdateCount")]
    public Common TotalUpdateCount
    {
      get => totalUpdateCount ??= new();
      set => totalUpdateCount = value;
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
    /// A value of RestartAp.
    /// </summary>
    [JsonPropertyName("restartAp")]
    public Case1 RestartAp
    {
      get => restartAp ??= new();
      set => restartAp = value;
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
    /// A value of ApCaseRoleFound.
    /// </summary>
    [JsonPropertyName("apCaseRoleFound")]
    public Common ApCaseRoleFound
    {
      get => apCaseRoleFound ??= new();
      set => apCaseRoleFound = value;
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
    /// A value of PassArea.
    /// </summary>
    [JsonPropertyName("passArea")]
    public External PassArea
    {
      get => passArea ??= new();
      set => passArea = value;
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
    /// A value of NoOf1099RequestsSent.
    /// </summary>
    [JsonPropertyName("noOf1099RequestsSent")]
    public Common NoOf1099RequestsSent
    {
      get => noOf1099RequestsSent ??= new();
      set => noOf1099RequestsSent = value;
    }

    /// <summary>
    /// A value of NoOfRequestsRead.
    /// </summary>
    [JsonPropertyName("noOfRequestsRead")]
    public Common NoOfRequestsRead
    {
      get => noOfRequestsRead ??= new();
      set => noOfRequestsRead = value;
    }

    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of ConvertDateDateWorkArea.
    /// </summary>
    [JsonPropertyName("convertDateDateWorkArea")]
    public DateWorkArea ConvertDateDateWorkArea
    {
      get => convertDateDateWorkArea ??= new();
      set => convertDateDateWorkArea = value;
    }

    /// <summary>
    /// A value of ConvertDateTextWorkArea.
    /// </summary>
    [JsonPropertyName("convertDateTextWorkArea")]
    public TextWorkArea ConvertDateTextWorkArea
    {
      get => convertDateTextWorkArea ??= new();
      set => convertDateTextWorkArea = value;
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

    private Common totalUpdateCount;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private TempCheckpointRestart tempCheckpointRestart;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Case1 restartAp;
    private Common local1099RequestFound;
    private Data1099LocateRequest new1;
    private Common apCaseRoleFound;
    private DateWorkArea nullDate;
    private Common fileOpened;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Data1099LocateRequest data1099LocateRequest;
    private Common noOf1099RecsCreated;
    private ProgramRun programRun;
    private ProgramProcessingInfo programProcessingInfo;
    private External passArea;
    private CsePerson csePerson;
    private Common numberOfUpdates;
    private Common numberOfReads;
    private ProgramError programError;
    private ProgramControlTotal programControlTotal;
    private Common noOf1099RequestsSent;
    private Common noOfRequestsRead;
    private Infrastructure infrastructure;
    private DateWorkArea convertDateDateWorkArea;
    private TextWorkArea convertDateTextWorkArea;
    private Common numberOfOpenCasesRead;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    /// <summary>
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
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
    /// A value of Existingdata1099LocateRequest.
    /// </summary>
    [JsonPropertyName("existingdata1099LocateRequest")]
    public Data1099LocateRequest Existingdata1099LocateRequest
    {
      get => existingdata1099LocateRequest ??= new();
      set => existingdata1099LocateRequest = value;
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
    /// A value of ExistingCsePerson.
    /// </summary>
    [JsonPropertyName("existingCsePerson")]
    public CsePerson ExistingCsePerson
    {
      get => existingCsePerson ??= new();
      set => existingCsePerson = value;
    }

    private InterstateRequest interstateRequest;
    private CaseUnit caseUnit;
    private Case1 case1;
    private Data1099LocateRequest existingdata1099LocateRequest;
    private ProgramRun existingProgramRun;
    private ProgramProcessingInfo existingProgramProcessingInfo;
    private CsePerson existingCsePerson;
  }
#endregion
}
