// Program: OE_B470_WRITE_FPLS_REQUEST, ID: 372364604, model: 746.
// Short name: SWEE470B
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
/// A program: OE_B470_WRITE_FPLS_REQUEST.
/// </para>
/// <para>
/// Resp:OBLMGMT
/// This procedure will read all FPLS Requests and create the actual 
/// transmission for distribution to the Federal Parent Locator Service.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class OeB470WriteFplsRequest: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B470_WRITE_FPLS_REQUEST program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB470WriteFplsRequest(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB470WriteFplsRequest.
  /// </summary>
  public OeB470WriteFplsRequest(IContext context, Import import, Export export):
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
    // 06/27/96 T.O.Redmond	Initial Creation.
    // 1/6/97	 Sid Chowdhary	Event Insertions.
    // 12/12/97 R Grey		remove ref to control tbl and situation no
    // ************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.NumberOfUpdates.Count = 0;
    local.NoOfRequestsRead.Count = 0;
    local.NoOfFplsRequestsSent.Count = 0;
    local.TotalRequestsRead.Count = 0;
    local.TotalUpdates.Count = 0;

    // ***********************************************
    // *Get the run parameters for this program.     *
    // ***********************************************
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.CurrentDate.Date = local.ProgramProcessingInfo.ProcessDate;
    local.CurrentDateLessFive.Date =
      AddDays(local.ProgramProcessingInfo.ProcessDate, -5);

    // -----------------------------------------------------------
    // 4.10.100
    // Beginning of Change
    // Open Error Report DDNAME=RPT99 and Control Report DDNAME = RPT98 .
    // -----------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    local.EabReportSend.ProgramName = "SWEEB470";
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
    // ************************************************
    // *Record the start time of this program.        *
    // ************************************************
    UseCreateProgramRun();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      // -----------------------------------------------------------
      // 4.10.100
      // Beginning of Change
      // Write error to error report.
      // -----------------------------------------------------------
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error in Create_Program_Run.";
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
      return;
    }

    // ***********************************************
    // *Get the DB2 commit frequency counts.         *
    // ***********************************************
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      // -----------------------------------------------------------
      // 4.10.100
      // Beginning of Change
      // Write error to error report.
      // -----------------------------------------------------------
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error in 'read_pgm_checkpoint_restart.";
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
      return;
    }

    // ************************************************
    // *Call External to Open the Flat File.          *
    // ************************************************
    local.PassArea.FileInstruction = "OPEN";
    UseOeFplsEabSendFplsRequest1();

    if (!IsEmpty(local.PassArea.TextReturnCode))
    {
      // -----------------------------------------------------------
      // 4.10.100
      // Beginning of Change
      // Write error to error report.
      // -----------------------------------------------------------
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error in file open for 'oe_fpls_eab_send_fpls_request'.";
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
      ExitState = "FILE_OPEN_ERROR";

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
      // -----------------------------------------------------------
      // 4.10.100
      // Beginning of Change
      // Write error to error report.
      // -----------------------------------------------------------
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error in Program_run : Program run not found.";
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
      ExitState = "PROGRAM_RUN_NF_RB";

      return;
    }

    // ***********************************************
    // * Process the selected records in groups based*
    // * upon the commit frequencies. Do a DB2 commit*
    // * at the end of each group.                   *
    // ***********************************************
    UseOeCabSetMnemonics();
    local.NoOfRequestsRead.Count = 0;
    local.NumberOfUpdates.Count = 0;
    local.NoOfFplsRequestsSent.Count = 0;
    local.FplsRequestFound.Flag = "N";

    foreach(var item in ReadFplsLocateRequest())
    {
      ++local.NoOfRequestsRead.Count;

      try
      {
        UpdateFplsLocateRequest();
        ++local.NumberOfUpdates.Count;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            // -----------------------------------------------------------
            // 4.10.100
            // Beginning of Change
            // Write error to error report.
            // -----------------------------------------------------------
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Update for fpls_locate_request is 'not unique'.";
            UseCabErrorReport1();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            ExitState = "OE0000_FPLS_REQUEST_NU";

            // -----------------------------------------------------------
            // 4.10.100
            // End of Change
            // -----------------------------------------------------------
            break;
          case ErrorCode.PermittedValueViolation:
            // -----------------------------------------------------------
            // 4.10.100
            // Beginning of Change
            // Write error to error report.
            // -----------------------------------------------------------
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Update for fpls_locate_request ,permitted value violation.";
            UseCabErrorReport1();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            ExitState = "OE0000_FPLS_REQUEST_PV";

            // -----------------------------------------------------------
            // 4.10.100
            // End of Change
            // -----------------------------------------------------------
            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      // -----------------------------------------------------------
      // As per change in requirment,following statements are commented out and 
      // now request will be send to all agencies.
      // 4.10.100
      // -----------------------------------------------------------
      // -------------------------------------------------------------
      // Beginning Of Change
      // 4.10.100
      // -------------------------------------------------------------
      export.ExternalFplsRequest.SendRequestTo = "A01A02C01C02E01E02F01";

      // -------------------------------------------------------------
      // Beginning Of Change
      // 4.10.100
      // -------------------------------------------------------------
      // ************************************************
      // *Create External Format for transmission to the*
      // *Federal Parent Locator Service.               *
      // ************************************************
      export.ExternalFplsRequest.ApCityOfBirth =
        entities.ExistingFplsLocateRequest.ApCityOfBirth ?? Spaces(16);
      export.ExternalFplsRequest.ApCsePersonNumber =
        Substring(entities.ExistingFplsLocateRequest.CaseId, 1, 10);
      export.ExternalFplsRequest.FplsLocateRequestIdentifier =
        entities.ExistingFplsLocateRequest.Identifier;

      // External Date of Birth is in MMDDYY format.
      // --------------------------------------------------------
      // 4.10.100
      // Beginning Of Change
      // --------------------------------------------------------
      export.ExternalFplsRequest.ApDateOfBirth =
        entities.ExistingFplsLocateRequest.ApDateOfBirth;

      // --------------------------------------------------------
      // 4.10.100
      // End Of Change
      // --------------------------------------------------------
      export.ExternalFplsRequest.ApFirstLastName =
        entities.ExistingFplsLocateRequest.ApFirstLastName ?? Spaces(20);
      export.ExternalFplsRequest.ApFirstName =
        entities.ExistingFplsLocateRequest.ApFirstName ?? Spaces(16);
      export.ExternalFplsRequest.ApMiddleName =
        entities.ExistingFplsLocateRequest.ApMiddleName ?? Spaces(16);
      export.ExternalFplsRequest.ApSecondLastName =
        entities.ExistingFplsLocateRequest.ApSecondLastName ?? Spaces(20);
      export.ExternalFplsRequest.ApThirdLastName =
        entities.ExistingFplsLocateRequest.ApThirdLastName ?? Spaces(20);
      export.ExternalFplsRequest.ApStateOrCountryOfBirth =
        entities.ExistingFplsLocateRequest.ApStateOrCountryOfBirth ?? Spaces
        (3);
      export.ExternalFplsRequest.ApsFathersFirstName =
        entities.ExistingFplsLocateRequest.ApsFathersFirstName ?? Spaces(13);
      export.ExternalFplsRequest.ApsFathersLastName =
        entities.ExistingFplsLocateRequest.ApsFathersLastName ?? Spaces(16);
      export.ExternalFplsRequest.ApsFathersMi =
        entities.ExistingFplsLocateRequest.ApsFathersMi ?? Spaces(1);
      export.ExternalFplsRequest.ApsMothersFirstName =
        entities.ExistingFplsLocateRequest.ApsMothersFirstName ?? Spaces(13);
      export.ExternalFplsRequest.ApsMothersMaidenName =
        entities.ExistingFplsLocateRequest.ApsMothersMaidenName ?? Spaces(16);
      export.ExternalFplsRequest.ApsMothersMi =
        entities.ExistingFplsLocateRequest.ApsMothersMi ?? Spaces(1);
      export.ExternalFplsRequest.CollectAllResponsesTogether =
        entities.ExistingFplsLocateRequest.CollectAllResponsesTogether ?? Spaces
        (1);
      export.ExternalFplsRequest.CpSsn =
        entities.ExistingFplsLocateRequest.CpSsn ?? Spaces(9);
      export.ExternalFplsRequest.Sex =
        entities.ExistingFplsLocateRequest.Sex ?? Spaces(1);
      export.ExternalFplsRequest.Ssn =
        entities.ExistingFplsLocateRequest.Ssn ?? Spaces(9);
      export.ExternalFplsRequest.StateAbbr =
        entities.ExistingFplsLocateRequest.StateAbbr ?? Spaces(2);
      export.ExternalFplsRequest.StationNumber =
        entities.ExistingFplsLocateRequest.StationNumber ?? Spaces(2);
      export.ExternalFplsRequest.TransactionError =
        entities.ExistingFplsLocateRequest.TransactionError ?? Spaces(10);
      export.ExternalFplsRequest.TransactionType =
        entities.ExistingFplsLocateRequest.TransactionType ?? Spaces(1);
      export.ExternalFplsRequest.TypeOfCase =
        entities.ExistingFplsLocateRequest.TypeOfCase ?? Spaces(1);
      export.ExternalFplsRequest.UsersField =
        entities.ExistingFplsLocateRequest.UsersField ?? Spaces(7);
      export.ExternalFplsRequest.Spaces1 = "";
      export.ExternalFplsRequest.Spaces2 = "";
      export.ExternalFplsRequest.Spaces3 = "";
      export.ExternalFplsRequest.Spaces4 = "";
      export.ExternalFplsRequest.TransactionType =
        entities.ExistingFplsLocateRequest.TransactionType ?? Spaces(1);
      export.ExternalFplsRequest.TypeOfCase =
        entities.ExistingFplsLocateRequest.TypeOfCase ?? Spaces(1);

      // ******************************************
      // *Write FPLS Detail to External File.     *
      // ******************************************
      local.PassArea.FileInstruction = "WRITED";
      UseOeFplsEabSendFplsRequest2();

      if (!IsEmpty(local.PassArea.TextReturnCode))
      {
        // -----------------------------------------------------------
        // 4.10.100
        // Beginning of Change
        // Write error to error report.
        // -----------------------------------------------------------
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error in writting external file for 'oe_fpls_eab_send_fpls_request'.";
          
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
        ExitState = "FILE_WRITE_ERROR_RB";

        return;
      }
      else
      {
        // ***	Insert Event for FPLS SESA request sent	***
        local.Infrastructure.SituationNumber = 0;
        local.Infrastructure.EventId = 10;
        local.Infrastructure.UserId = "FPLS";
        local.Infrastructure.ReasonCode = "FPLSSENT";
        local.Infrastructure.BusinessObjectCd = "FPL";
        local.Infrastructure.DenormNumeric12 =
          entities.ExistingFplsLocateRequest.Identifier;
        local.Infrastructure.ReferenceDate =
          entities.ExistingFplsLocateRequest.RequestSentDate;
        local.ConvertDateDateWorkArea.Date =
          entities.ExistingFplsLocateRequest.RequestSentDate;
        UseCabConvertDate2String();
        local.Infrastructure.Detail = "FPLS Locate Request Sent Date : " + TrimEnd
          (local.ConvertDateTextWorkArea.Text8);
        local.Infrastructure.Detail = Spaces(Infrastructure.Detail_MaxLength);
        local.Infrastructure.ProcessStatus = "Q";

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
          // -----------------------------------------------------------
          // 4.10.100
          // Beginning of Change
          // Write error to error report.
          // Handle error coming from oe_cab_raise_event(do not abend  job).
          // -----------------------------------------------------------
          if (IsExitState("CASE_NF"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Cann't insert event because CASE not found for cse person : " + entities
              .ExistingCsePerson.Number;
            UseCabErrorReport1();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            ExitState = "ACO_NN0000_ALL_OK";

            // -----------------------------------------------------------
            // 4.10.100
            // End of Change
            // -----------------------------------------------------------
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
        }
        else
        {
          ExitState = "CSE_PERSON_NF";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          // ************************************************
          // *Rollback all updates to the last successfull  *
          // *commit. Write Error Report                    *
          // ************************************************
          UseEabRollbackSql();
          UseExtToDoACommit();

          return;
        }

        // ***	End of code for Insert Event for FPLS SESA request sent	***
      }

      ++local.NoOfFplsRequestsSent.Count;

      // ************************************************
      // *Check the number of reads, and updates that   *
      // *have occurred since the last checkpoint.      *
      // ************************************************
      if (local.NoOfRequestsRead.Count >= local
        .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault() || local
        .NumberOfUpdates.Count >= local
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        local.TotalRequestsRead.Count += local.NoOfRequestsRead.Count;
        local.NoOfRequestsRead.Count = 0;
        local.TotalUpdates.Count += local.NumberOfUpdates.Count;
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
    // *Final Commit.
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
    // -----------------------------------------------------------------
    // 4.10.100
    // Beginning Of Change
    // Write all totals to Control Report
    // -----------------------------------------------------------------
    local.EabReportSend.RptDetail = "FPLS Number Of Request Read       :" + NumberToString
      (local.TotalRequestsRead.Count, 15);
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

    local.EabReportSend.RptDetail = "FPLS Number Of Request Sent       :" + NumberToString
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

    local.EabReportSend.RptDetail = "FPLS Number Of Request Updates       :" + NumberToString
      (local.TotalUpdates.Count, 15);
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

    if (AsChar(local.FileOpened.Flag) == 'Y')
    {
      // ******************************************
      // *Close External FPLS Request Tape        *
      // ******************************************
      local.PassArea.FileInstruction = "CLOSE";
      UseOeFplsEabSendFplsRequest1();

      if (!IsEmpty(export.External.TextReturnCode))
      {
        ExitState = "FILE_CLOSE_ERROR";

        return;
      }
    }

    // ---------------------------------------------------------------
    // 4.10.100
    // Beginning Of Change
    // Close Error Report and Control Report files.
    // ---------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

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

  private void UseOeCabSetMnemonics()
  {
    var useImport = new OeCabSetMnemonics.Import();
    var useExport = new OeCabSetMnemonics.Export();

    Call(OeCabSetMnemonics.Execute, useImport, useExport);

    local.MaxDate.ExpirationDate = useExport.MaxDate.ExpirationDate;
  }

  private void UseOeFplsEabSendFplsRequest1()
  {
    var useImport = new OeFplsEabSendFplsRequest.Import();
    var useExport = new OeFplsEabSendFplsRequest.Export();

    useImport.External.FileInstruction = local.PassArea.FileInstruction;
    useExport.External.Assign(local.PassArea);

    Call(OeFplsEabSendFplsRequest.Execute, useImport, useExport);

    local.PassArea.Assign(useExport.External);
  }

  private void UseOeFplsEabSendFplsRequest2()
  {
    var useImport = new OeFplsEabSendFplsRequest.Import();
    var useExport = new OeFplsEabSendFplsRequest.Export();

    useImport.ExternalFplsRequest.Assign(export.ExternalFplsRequest);
    useImport.External.FileInstruction = local.PassArea.FileInstruction;
    useExport.External.Assign(local.PassArea);

    Call(OeFplsEabSendFplsRequest.Execute, useImport, useExport);

    local.PassArea.Assign(useExport.External);
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

    MoveProgramProcessingInfo(local.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);
    useImport.ProgramRun.StartTimestamp = local.ProgramRun.StartTimestamp;

    Call(UpdateProgramRun.Execute, useImport, useExport);
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
    System.Diagnostics.Debug.
      Assert(entities.ExistingFplsLocateRequest.Populated);
    entities.ExistingCsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(
          command, "numb", entities.ExistingFplsLocateRequest.CspNumber);
      },
      (db, reader) =>
      {
        entities.ExistingCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingCsePerson.Type1 = db.GetString(reader, 1);
        entities.ExistingCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ExistingCsePerson.Type1);
      });
  }

  private IEnumerable<bool> ReadFplsLocateRequest()
  {
    entities.ExistingFplsLocateRequest.Populated = false;

    return ReadEach("ReadFplsLocateRequest",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "requestSentDate1",
          local.CurrentDateLessFive.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "requestSentDate2", local.NullDate.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.ExistingFplsLocateRequest.CspNumber = db.GetString(reader, 0);
        entities.ExistingFplsLocateRequest.Identifier = db.GetInt32(reader, 1);
        entities.ExistingFplsLocateRequest.TransactionStatus =
          db.GetNullableString(reader, 2);
        entities.ExistingFplsLocateRequest.StateAbbr =
          db.GetNullableString(reader, 3);
        entities.ExistingFplsLocateRequest.StationNumber =
          db.GetNullableString(reader, 4);
        entities.ExistingFplsLocateRequest.TransactionType =
          db.GetNullableString(reader, 5);
        entities.ExistingFplsLocateRequest.Ssn =
          db.GetNullableString(reader, 6);
        entities.ExistingFplsLocateRequest.CaseId =
          db.GetNullableString(reader, 7);
        entities.ExistingFplsLocateRequest.LocalCode =
          db.GetNullableString(reader, 8);
        entities.ExistingFplsLocateRequest.UsersField =
          db.GetNullableString(reader, 9);
        entities.ExistingFplsLocateRequest.TypeOfCase =
          db.GetNullableString(reader, 10);
        entities.ExistingFplsLocateRequest.ApFirstName =
          db.GetNullableString(reader, 11);
        entities.ExistingFplsLocateRequest.ApMiddleName =
          db.GetNullableString(reader, 12);
        entities.ExistingFplsLocateRequest.ApFirstLastName =
          db.GetNullableString(reader, 13);
        entities.ExistingFplsLocateRequest.ApSecondLastName =
          db.GetNullableString(reader, 14);
        entities.ExistingFplsLocateRequest.ApThirdLastName =
          db.GetNullableString(reader, 15);
        entities.ExistingFplsLocateRequest.ApDateOfBirth =
          db.GetNullableDate(reader, 16);
        entities.ExistingFplsLocateRequest.Sex =
          db.GetNullableString(reader, 17);
        entities.ExistingFplsLocateRequest.CollectAllResponsesTogether =
          db.GetNullableString(reader, 18);
        entities.ExistingFplsLocateRequest.TransactionError =
          db.GetNullableString(reader, 19);
        entities.ExistingFplsLocateRequest.ApCityOfBirth =
          db.GetNullableString(reader, 20);
        entities.ExistingFplsLocateRequest.ApStateOrCountryOfBirth =
          db.GetNullableString(reader, 21);
        entities.ExistingFplsLocateRequest.ApsFathersFirstName =
          db.GetNullableString(reader, 22);
        entities.ExistingFplsLocateRequest.ApsFathersMi =
          db.GetNullableString(reader, 23);
        entities.ExistingFplsLocateRequest.ApsFathersLastName =
          db.GetNullableString(reader, 24);
        entities.ExistingFplsLocateRequest.ApsMothersFirstName =
          db.GetNullableString(reader, 25);
        entities.ExistingFplsLocateRequest.ApsMothersMi =
          db.GetNullableString(reader, 26);
        entities.ExistingFplsLocateRequest.ApsMothersMaidenName =
          db.GetNullableString(reader, 27);
        entities.ExistingFplsLocateRequest.CpSsn =
          db.GetNullableString(reader, 28);
        entities.ExistingFplsLocateRequest.CreatedBy = db.GetString(reader, 29);
        entities.ExistingFplsLocateRequest.CreatedTimestamp =
          db.GetDateTime(reader, 30);
        entities.ExistingFplsLocateRequest.LastUpdatedBy =
          db.GetString(reader, 31);
        entities.ExistingFplsLocateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 32);
        entities.ExistingFplsLocateRequest.RequestSentDate =
          db.GetNullableDate(reader, 33);
        entities.ExistingFplsLocateRequest.SendRequestTo =
          db.GetNullableString(reader, 34);
        entities.ExistingFplsLocateRequest.Populated = true;

        return true;
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

  private void UpdateFplsLocateRequest()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingFplsLocateRequest.Populated);

    var transactionStatus = "S";
    var lastUpdatedBy = global.TranCode;
    var lastUpdatedTimestamp = Now();
    var requestSentDate = local.CurrentDate.Date;
    var sendRequestTo = "A01A02C01C02E01E02F01";

    entities.ExistingFplsLocateRequest.Populated = false;
    Update("UpdateFplsLocateRequest",
      (db, command) =>
      {
        db.SetNullableString(command, "transactionStatus", transactionStatus);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTimes", lastUpdatedTimestamp);
        db.SetNullableDate(command, "requestSentDate", requestSentDate);
        db.SetNullableString(command, "sendRequestTo", sendRequestTo);
        db.SetString(
          command, "cspNumber", entities.ExistingFplsLocateRequest.CspNumber);
        db.SetInt32(
          command, "identifier", entities.ExistingFplsLocateRequest.Identifier);
          
      });

    entities.ExistingFplsLocateRequest.TransactionStatus = transactionStatus;
    entities.ExistingFplsLocateRequest.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingFplsLocateRequest.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.ExistingFplsLocateRequest.RequestSentDate = requestSentDate;
    entities.ExistingFplsLocateRequest.SendRequestTo = sendRequestTo;
    entities.ExistingFplsLocateRequest.Populated = true;
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
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    public External External
    {
      get => external ??= new();
      set => external = value;
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

    private ExternalFplsRequest externalFplsRequest;
    private External external;
    private ProgramProcessingInfo programProcessingInfo;
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
    /// A value of FplsIndicator.
    /// </summary>
    [JsonPropertyName("fplsIndicator")]
    public Common FplsIndicator
    {
      get => fplsIndicator ??= new();
      set => fplsIndicator = value;
    }

    /// <summary>
    /// A value of Length.
    /// </summary>
    [JsonPropertyName("length")]
    public Common Length
    {
      get => length ??= new();
      set => length = value;
    }

    /// <summary>
    /// A value of SesaFipsCodes.
    /// </summary>
    [JsonPropertyName("sesaFipsCodes")]
    public FplsLocateRequest SesaFipsCodes
    {
      get => sesaFipsCodes ??= new();
      set => sesaFipsCodes = value;
    }

    /// <summary>
    /// A value of CurrentDateLessFive.
    /// </summary>
    [JsonPropertyName("currentDateLessFive")]
    public DateWorkArea CurrentDateLessFive
    {
      get => currentDateLessFive ??= new();
      set => currentDateLessFive = value;
    }

    /// <summary>
    /// A value of CurrentDate.
    /// </summary>
    [JsonPropertyName("currentDate")]
    public DateWorkArea CurrentDate
    {
      get => currentDate ??= new();
      set => currentDate = value;
    }

    /// <summary>
    /// A value of TotalUpdates.
    /// </summary>
    [JsonPropertyName("totalUpdates")]
    public Common TotalUpdates
    {
      get => totalUpdates ??= new();
      set => totalUpdates = value;
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
    /// A value of NoOfFplsRequestsSent.
    /// </summary>
    [JsonPropertyName("noOfFplsRequestsSent")]
    public Common NoOfFplsRequestsSent
    {
      get => noOfFplsRequestsSent ??= new();
      set => noOfFplsRequestsSent = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
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
    /// A value of TempCheckpointRestart.
    /// </summary>
    [JsonPropertyName("tempCheckpointRestart")]
    public TempCheckpointRestart TempCheckpointRestart
    {
      get => tempCheckpointRestart ??= new();
      set => tempCheckpointRestart = value;
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
    /// A value of NullDate.
    /// </summary>
    [JsonPropertyName("nullDate")]
    public DateWorkArea NullDate
    {
      get => nullDate ??= new();
      set => nullDate = value;
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
    /// A value of SesaBatch.
    /// </summary>
    [JsonPropertyName("sesaBatch")]
    public WorkArea SesaBatch
    {
      get => sesaBatch ??= new();
      set => sesaBatch = value;
    }

    /// <summary>
    /// A value of FplsRequestFound.
    /// </summary>
    [JsonPropertyName("fplsRequestFound")]
    public Common FplsRequestFound
    {
      get => fplsRequestFound ??= new();
      set => fplsRequestFound = value;
    }

    /// <summary>
    /// A value of SendDirective.
    /// </summary>
    [JsonPropertyName("sendDirective")]
    public FplsLocateRequest SendDirective
    {
      get => sendDirective ??= new();
      set => sendDirective = value;
    }

    /// <summary>
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

    /// <summary>
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
    }

    /// <summary>
    /// A value of InvalidCode.
    /// </summary>
    [JsonPropertyName("invalidCode")]
    public Common InvalidCode
    {
      get => invalidCode ??= new();
      set => invalidCode = value;
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

    private EabReportSend neededToWrite;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private Common fplsIndicator;
    private Common length;
    private FplsLocateRequest sesaFipsCodes;
    private DateWorkArea currentDateLessFive;
    private DateWorkArea currentDate;
    private Common totalUpdates;
    private Common totalRequestsRead;
    private Common noOfRequestsRead;
    private Common noOfFplsRequestsSent;
    private Common numberOfUpdates;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramRun programRun;
    private ProgramCheckpointRestart programCheckpointRestart;
    private External passArea;
    private TempCheckpointRestart tempCheckpointRestart;
    private Common fileOpened;
    private ProgramError programError;
    private ProgramControlTotal programControlTotal;
    private Code maxDate;
    private DateWorkArea nullDate;
    private Common actionNeeded;
    private WorkArea sesaBatch;
    private Common fplsRequestFound;
    private FplsLocateRequest sendDirective;
    private Code code;
    private CodeValue codeValue;
    private Common invalidCode;
    private Infrastructure infrastructure;
    private DateWorkArea convertDateDateWorkArea;
    private TextWorkArea convertDateTextWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ExistingFplsLocateRequest.
    /// </summary>
    [JsonPropertyName("existingFplsLocateRequest")]
    public FplsLocateRequest ExistingFplsLocateRequest
    {
      get => existingFplsLocateRequest ??= new();
      set => existingFplsLocateRequest = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

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

    private ProgramRun existingProgramRun;
    private ProgramProcessingInfo existingProgramProcessingInfo;
    private FplsLocateRequest existingFplsLocateRequest;
    private CsePerson existingCsePerson;
    private Case1 case1;
    private InterstateRequest interstateRequest;
    private CaseUnit caseUnit;
  }
#endregion
}
