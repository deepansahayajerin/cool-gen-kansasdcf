// Program: OE_B430_LOAD_1099_RESPONSES, ID: 372374764, model: 746.
// Short name: SWEE430B
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_B430_LOAD_1099_RESPONSES.
/// </para>
/// <para>
/// Resp: OBLGEST			
/// This procedure reads an external file containing responses to 1099_REQUEST's
/// for information that has been sent to the IRS. Responses are matched with
/// the corresponding request. If a request has been deleted before the
/// 1099_RESPONSE is received back from the IRS an error message will be entered
/// in the program error log. This procedure creates a row for each
/// 1099_RESPONSE received and associates it with its corresponding
/// 1099_REQUEST.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class OeB430Load1099Responses: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B430_LOAD_1099_RESPONSES program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB430Load1099Responses(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB430Load1099Responses.
  /// </summary>
  public OeB430Load1099Responses(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------
    //   Date		Developer	Description
    // 01/13/96   	T.O.Redmond 	Initial Creation
    // This Action Block contains all the processing
    // for creating 1099 Responses to 1099 Requests.
    // Import is from a batch process.
    // 1/2/98   Siraj Konkader
    // Modified calls to Create_Program_Error, Create_Program_Control_Total - 
    // removed persistent views beacuse of performance problems.
    // Also removed calls to assign_program_error_id and  
    // assign_program_control_total_id. The function of these cabs were to set
    // the identifiers of Program Error Id and Program Control Total.  However,
    // since both the above tables are used in conjunction with Program Run, the
    // identifiers will always start from 1 and not any "last used" value + 1.
    // ---------------------------------------------
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

    // ************************************************************
    // Open error report
    // ************************************************************
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = export.ProgramProcessingInfo.ProcessDate;
    local.EabReportSend.ProgramName = "SWEEB430";
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // ************************************************************
    // Open control report
    // ************************************************************
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // ***********************************************
    // *Initialize flags                             *
    // ***********************************************
    local.FileOpened.Flag = "N";
    local.Last1099Response.Identifier = 999;
    local.ProgramCheckpointRestart.CheckpointCount = 0;
    local.NoOf1099RecsRead.Count = 0;
    local.No1099ResponseUpdates.Count = 0;

    // ************************************************
    // *Find out if we are in a continuation cycle    *
    // *(recursive flow) or if this is the first time *
    // *into the program.                             *
    // ************************************************
    if (AsChar(import.BatchAttributes.ContinuationInd) == 'Y')
    {
      // ************************************************
      // *Since the Continuation_Ind is 'Y' this means  *
      // *that we are in a continuation cycle.          *
      // ************************************************
      // ************************************************
      // *Move imports to exports for the recursive flow*
      // ************************************************
      export.BatchAttributes.ContinuationInd =
        import.BatchAttributes.ContinuationInd;
      MoveProgramProcessingInfo2(import.ProgramProcessingInfo,
        export.ProgramProcessingInfo);
      export.ProgramRun.StartTimestamp = import.ProgramRun.StartTimestamp;

      // ************************************************
      // *Get latest commit frequencies numbers.        *
      // ************************************************
      local.ProgramCheckpointRestart.ProgramName = global.UserId;
      UseReadPgmCheckpointRestart();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }
    else
    {
      // ************************************************
      // *Since the Continuation_Ind is not 'Y' this    *
      // *means that this is the first time into this   *
      // *program.
      // 
      // *
      // ************************************************
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
      // *FInd out if we are in a Restart situation.    *
      // ************************************************
      local.ProgramCheckpointRestart.ProgramName = global.UserId;
      UseReadPgmCheckpointRestart();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      // ************************************************
      // *Record the start time of this program.        *
      // ************************************************
      export.ProgramRun.FromRestartInd =
        local.ProgramCheckpointRestart.RestartInd ?? "";
      local.Restart.Number = "0000000000";
      UseCreateProgramRun();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
      {
        // ************************************************
        // * This is not required since we are not        *
        // * handling restart at this time.  RVW 8/7/96   *
        // *
        // 
        // *
        // ************************************************
        goto Test1;

        // ************************************************
        // * Call external to re-POSITION the flat file.  *
        // * The restart will be initiated based upon the *
        // * total number of records processed thru the   *
        // * last checkpoint.                             *
        // ************************************************
        local.Restart.Number =
          Substring(local.ProgramCheckpointRestart.RestartInfo, 1, 10);
        local.TotalRecordsProcessed.Count =
          (int)StringToNumber(Substring(
            local.ProgramCheckpointRestart.RestartInfo, 250, 11, 20));
        local.PassArea.TextLine80 =
          local.ProgramCheckpointRestart.RestartInfo ?? Spaces(130);
        local.PassArea.FileInstruction = "POSITION";
        UseOeEabReceive1099Response1();

        if (!IsEmpty(local.PassArea.TextReturnCode))
        {
          ExitState = "FILE_POSITION_ERROR_WITH_RB";

          return;
        }

        local.FileOpened.Flag = "Y";
      }
      else
      {
        // Initialize the checkpoint count since this is the initial submission 
        // of this program.  Without doing this the counter would contain the
        // number of checkpoints from the last program execution.
        local.ProgramCheckpointRestart.CheckpointCount = 0;

        // ************************************************
        // * Call external to OPEN the driver file.       *
        // ************************************************
        local.PassArea.FileInstruction = "OPEN";
        UseOeEabReceive1099Response2();

        if (!IsEmpty(local.PassArea.TextReturnCode))
        {
          // -----------------------------------------------------------------
          // 4.9.150
          // Beginning Of Change
          // Write error to Error Report.
          // -----------------------------------------------------------------
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error encountered opening external input file in oe_eab_receive_1099_response.";
            
          UseCabErrorReport1();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          // -----------------------------------------------------------------
          // 4.9.150
          // End Of Change
          // -----------------------------------------------------------------
          ExitState = "ZD_FILE_OPEN_ERROR_WITH_AB";

          return;
        }

        local.FileOpened.Flag = "Y";
      }
    }

Test1:

    // ************************************************
    // *Read the program_run each time we come into   *
    // *this program so that we will have currency for*
    // *creating any error rows or control total rows.*
    // ************************************************
    if (ReadProgramRun())
    {
      // Get the next error number and control total number so that it can be 
      // incremented below.  Only want to read the database once to get the next
      // number, not every insert.
      // **** Above stmt incorrect. Deleted calls to cabs. See modification log
      // ... SAK 1/2/98
    }
    else
    {
      ExitState = "PROGRAM_RUN_NF_RB";

      return;
    }

    // ************************************************
    // *Process driver records until we need to do a  *
    // *commit or until we have reached the end of    *
    // *file.
    // 
    // *
    // ************************************************
    local.NoOfExtFileRecsWrittn.Count = 0;

    do
    {
      local.Last1099Response.Identifier = 999;

      // ************************************************
      // *Call external to READ the flat file.          *
      // ************************************************
      ++local.NoOfExtFileRecsWrittn.Count;
      local.PassArea.FileInstruction = "READ";
      UseOeEabReceive1099Response3();

      // -----------------------------------------------------------------
      // 4.9.150
      // Beginning Of Change
      // Added End Of File check here so that it will come out of loop.
      // -----------------------------------------------------------------
      if (Equal(local.PassArea.TextReturnCode, "EF"))
      {
        break;
      }

      // -----------------------------------------------------------------
      // 4.9.150
      // End Of Change
      // -----------------------------------------------------------------
      if (!IsEmpty(local.PassArea.TextReturnCode))
      {
        // -----------------------------------------------------------------
        // 4.9.150
        // Beginning Of Change
        // Write error to Error Report.
        // -----------------------------------------------------------------
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error encountered reading external input file in oe_eab_receive_1099_response.";
          
        UseCabErrorReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        // -----------------------------------------------------------------
        // 4.9.150
        // End Of Change
        // -----------------------------------------------------------------
        return;
      }

      ++local.NoOf1099RecsRead.Count;
      local.ProgramCheckpointRestart.LastCheckpointTimestamp = Now();
      local.ProgramCheckpointRestart.RestartInd = "N";
      local.ProgramCheckpointRestart.RestartInfo = entities.ExistingAp.Number;
      local.ProgramCheckpointRestart.CheckpointCount =
        local.ProgramCheckpointRestart.CheckpointCount.GetValueOrDefault() + 1;
      UseUpdatePgmCheckpointRestart2();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      // ************************************************
      // *Move all information from the external to the *
      // *entity format for the 1099 response.          *
      // ************************************************
      local.Data1099LocateResponse.Identifier =
        export.ExternalOcse1099Response.Attribute1099RequestIdentifier;
      local.Data1099LocateResponse.DateReceived =
        export.ProgramProcessingInfo.ProcessDate;
      local.Data1099LocateResponse.UsageStatus = "";
      local.Data1099LocateResponse.PayerEin =
        export.ExternalOcse1099Response.PayorEin;
      local.Data1099LocateResponse.TaxYear =
        export.ExternalOcse1099Response.TaxYear;
      local.Data1099LocateResponse.PayerAccountNo =
        export.ExternalOcse1099Response.AccountCode;
      local.Data1099LocateResponse.DocumentCode =
        export.ExternalOcse1099Response.DocumentCode;
      local.Data1099LocateResponse.PayeeLine1 =
        export.ExternalOcse1099Response.PayeeName1;
      local.Data1099LocateResponse.PayeeLine2 =
        export.ExternalOcse1099Response.PayeeName2;
      local.Data1099LocateResponse.PayeeLine3 =
        export.ExternalOcse1099Response.PayeeStreet;
      local.Data1099LocateResponse.PayeeLine4 =
        export.ExternalOcse1099Response.PayeeCity;
      local.Data1099LocateResponse.StateCode =
        export.ExternalOcse1099Response.PayeeState;
      local.Data1099LocateResponse.ZipCode =
        export.ExternalOcse1099Response.PayeeZipCode;
      local.Data1099LocateResponse.PayerEin =
        export.ExternalOcse1099Response.PayorEin;
      local.Data1099LocateResponse.PayerLine1 =
        export.ExternalOcse1099Response.PayorName1;
      local.Data1099LocateResponse.PayerLine2 =
        export.ExternalOcse1099Response.PayorName2;
      local.Data1099LocateResponse.PayerLine3 =
        export.ExternalOcse1099Response.PayorStreet;
      local.Data1099LocateResponse.PayerLine4 =
        export.ExternalOcse1099Response.PayorCityStateZip;
      local.Data1099LocateResponse.AmountInd1 =
        export.ExternalOcse1099Response.AmountInd1;
      local.Data1099LocateResponse.AmountInd2 =
        export.ExternalOcse1099Response.AmountInd2;
      local.Data1099LocateResponse.AmountInd3 =
        export.ExternalOcse1099Response.AmountInd3;
      local.Data1099LocateResponse.AmountInd4 =
        export.ExternalOcse1099Response.AmountInd4;
      local.Data1099LocateResponse.AmountInd5 =
        export.ExternalOcse1099Response.AmountInd5;
      local.Data1099LocateResponse.AmountInd6 =
        export.ExternalOcse1099Response.AmountInd6;
      local.Data1099LocateResponse.AmountInd7 =
        export.ExternalOcse1099Response.AmountInd7;
      local.Data1099LocateResponse.AmountInd8 =
        export.ExternalOcse1099Response.AmountInd8;
      local.Data1099LocateResponse.AmountInd9 =
        export.ExternalOcse1099Response.AmountInd9;
      local.Data1099LocateResponse.AmountInd10 =
        export.ExternalOcse1099Response.AmountInd10;
      local.Data1099LocateResponse.AmountInd11 =
        export.ExternalOcse1099Response.AmountInd11;
      local.Data1099LocateResponse.AmountInd12 =
        export.ExternalOcse1099Response.AmountInd12;
      local.Data1099LocateResponse.Amount1 =
        export.ExternalOcse1099Response.Amount1;
      local.Data1099LocateResponse.Amount2 =
        export.ExternalOcse1099Response.Amount2;
      local.Data1099LocateResponse.Amount3 =
        export.ExternalOcse1099Response.Amount3;
      local.Data1099LocateResponse.Amount4 =
        export.ExternalOcse1099Response.Amount4;
      local.Data1099LocateResponse.Amount5 =
        export.ExternalOcse1099Response.Amount5;
      local.Data1099LocateResponse.Amount6 =
        export.ExternalOcse1099Response.Amount6;
      local.Data1099LocateResponse.Amount7 =
        export.ExternalOcse1099Response.Amount7;
      local.Data1099LocateResponse.Amount8 =
        export.ExternalOcse1099Response.Amount8;
      local.Data1099LocateResponse.Amount9 =
        export.ExternalOcse1099Response.Amount9;
      local.Data1099LocateResponse.Amount10 =
        export.ExternalOcse1099Response.Amount10;
      local.Data1099LocateResponse.Amount11 =
        export.ExternalOcse1099Response.Amount11;
      local.Data1099LocateResponse.Amount12 =
        export.ExternalOcse1099Response.Amount12;
      local.Local1099RequestFound.Flag = "N";

      if (ReadCsePerson1())
      {
        if (Read1099LocateRequest())
        {
          local.Local1099RequestFound.Flag = "Y";

          try
          {
            Update1099LocateRequest();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "1099_LOCATE_REQUEST_NU";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "1099_LOCATE_REQUEST_PV";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
        else
        {
          try
          {
            Create1099LocateRequest();
            local.Local1099RequestFound.Flag = "Y";
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "1099_LOCATE_REQUEST_PV";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }

        local.ResponseFound.Flag = "N";

        if (AsChar(local.Local1099RequestFound.Flag) != 'Y')
        {
          // ************************************************
          // *Here you must interrogate every exit state to *
          // *determine what should be done.                *
          // *For critical errors that need to abend the    *
          // *program, set an abort exit state and escape.  *
          // *For non-critical errors you may write an error*
          // *record to the program error entity type.  The *
          // *exit state must be turned into an error code  *
          // *that is a valid value on the codes entity type*
          // ************************************************
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            export.ExternalOcse1099Response.Ssn + export
            .ExternalOcse1099Response.CsePersonNumber + NumberToString
            (export.ExternalOcse1099Response.Attribute1099RequestIdentifier, 15) +
            export.ExternalOcse1099Response.FirstName + " " + export
            .ExternalOcse1099Response.LastName + ": Request not found";
          UseCabErrorReport1();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }
        else
        {
          if (local.Last1099Response.Identifier == 999)
          {
            if (Read1099LocateResponse())
            {
              local.ResponseFound.Flag = "Y";
            }

            if (AsChar(local.ResponseFound.Flag) == 'Y')
            {
              local.Last1099Response.Identifier =
                entities.Existingdata1099LocateResponse.Identifier;
            }
            else
            {
              local.Last1099Response.Identifier = 0;
            }
          }

          // ************************************************
          // *If the response just received is the same as  *
          // *the last response updated, then we will not   *
          // *allow a duplicate update to occur. Not all    *
          // *data will be examined for duplication.          *
          // ************************************************
          local.ResponseDuplicate.Flag = "N";

          if (AsChar(local.ResponseFound.Flag) == 'Y')
          {
            local.ResponseDuplicate.Flag = "Y";

            if (!Equal(entities.Existingdata1099LocateResponse.Amount12,
              local.Data1099LocateResponse.Amount12.GetValueOrDefault()))
            {
              local.ResponseDuplicate.Flag = "N";

              goto Test2;
            }

            if (!Equal(entities.Existingdata1099LocateResponse.Amount11,
              local.Data1099LocateResponse.Amount11.GetValueOrDefault()))
            {
              local.ResponseDuplicate.Flag = "N";

              goto Test2;
            }

            if (!Equal(entities.Existingdata1099LocateResponse.Amount10,
              local.Data1099LocateResponse.Amount10.GetValueOrDefault()))
            {
              local.ResponseDuplicate.Flag = "N";

              goto Test2;
            }

            if (!Equal(entities.Existingdata1099LocateResponse.Amount9,
              local.Data1099LocateResponse.Amount9.GetValueOrDefault()))
            {
              local.ResponseDuplicate.Flag = "N";

              goto Test2;
            }

            if (!Equal(entities.Existingdata1099LocateResponse.Amount8,
              local.Data1099LocateResponse.Amount8.GetValueOrDefault()))
            {
              local.ResponseDuplicate.Flag = "N";

              goto Test2;
            }

            if (!Equal(entities.Existingdata1099LocateResponse.Amount7,
              local.Data1099LocateResponse.Amount7.GetValueOrDefault()))
            {
              local.ResponseDuplicate.Flag = "N";

              goto Test2;
            }

            if (!Equal(entities.Existingdata1099LocateResponse.Amount6,
              local.Data1099LocateResponse.Amount6.GetValueOrDefault()))
            {
              local.ResponseDuplicate.Flag = "N";

              goto Test2;
            }

            if (!Equal(entities.Existingdata1099LocateResponse.Amount5,
              local.Data1099LocateResponse.Amount5.GetValueOrDefault()))
            {
              local.ResponseDuplicate.Flag = "N";

              goto Test2;
            }

            if (!Equal(entities.Existingdata1099LocateResponse.Amount4,
              local.Data1099LocateResponse.Amount4.GetValueOrDefault()))
            {
              local.ResponseDuplicate.Flag = "N";

              goto Test2;
            }

            if (!Equal(entities.Existingdata1099LocateResponse.Amount3,
              local.Data1099LocateResponse.Amount3.GetValueOrDefault()))
            {
              local.ResponseDuplicate.Flag = "N";

              goto Test2;
            }

            if (!Equal(entities.Existingdata1099LocateResponse.Amount2,
              local.Data1099LocateResponse.Amount2.GetValueOrDefault()))
            {
              local.ResponseDuplicate.Flag = "N";

              goto Test2;
            }

            if (!Equal(entities.Existingdata1099LocateResponse.Amount1,
              local.Data1099LocateResponse.Amount1.GetValueOrDefault()))
            {
              local.ResponseDuplicate.Flag = "N";

              goto Test2;
            }

            if (!Equal(entities.Existingdata1099LocateResponse.AmountInd1,
              local.Data1099LocateResponse.AmountInd1))
            {
              local.ResponseDuplicate.Flag = "N";

              goto Test2;
            }

            if (!Equal(entities.Existingdata1099LocateResponse.AmountInd2,
              local.Data1099LocateResponse.AmountInd2))
            {
              local.ResponseDuplicate.Flag = "N";

              goto Test2;
            }

            if (!Equal(entities.Existingdata1099LocateResponse.AmountInd3,
              local.Data1099LocateResponse.AmountInd3))
            {
              local.ResponseDuplicate.Flag = "N";

              goto Test2;
            }

            if (!Equal(entities.Existingdata1099LocateResponse.DocumentCode,
              local.Data1099LocateResponse.DocumentCode))
            {
              local.ResponseDuplicate.Flag = "N";

              goto Test2;
            }

            if (!Equal(entities.Existingdata1099LocateResponse.PayeeLine1,
              local.Data1099LocateResponse.PayeeLine1))
            {
              local.ResponseDuplicate.Flag = "N";

              goto Test2;
            }

            if (!Equal(entities.Existingdata1099LocateResponse.PayerLine1,
              local.Data1099LocateResponse.PayerLine1))
            {
              local.ResponseDuplicate.Flag = "N";

              goto Test2;
            }

            if (!Equal(entities.Existingdata1099LocateResponse.PayerAccountNo,
              local.Data1099LocateResponse.PayerAccountNo))
            {
              local.ResponseDuplicate.Flag = "N";
            }
          }

Test2:

          if (AsChar(local.ResponseDuplicate.Flag) != 'Y')
          {
            // ************************************************
            // *This is not a duplicate response.             *
            // ************************************************
            try
            {
              Create1099LocateResponse();
              ++local.No1099ResponseUpdates.Count;
              ++local.Last1099Response.Identifier;

              // ********************************************************
              // Insert Event for 1099 Response received.
              // 02/10/98 JF.Caillouet
              // ********************************************************
              local.Infrastructure.SituationNumber = 0;
              local.Infrastructure.EventId = 10;
              local.Infrastructure.ReasonCode = "1099RCVD";
              local.Infrastructure.BusinessObjectCd = "CAU";
              local.Infrastructure.UserId = "1099";
              local.Infrastructure.DenormNumeric12 =
                entities.Existingdata1099LocateResponse.Identifier;
              local.Infrastructure.ReferenceDate =
                entities.Existingdata1099LocateResponse.DateReceived;
              local.ConvertDateDateWorkArea.Date =
                entities.Existingdata1099LocateResponse.DateReceived;
              UseCabConvertDate2String();
              local.Infrastructure.Detail = "1099 Response Received Date:" + TrimEnd
                (local.ConvertDateTextWorkArea.Text8);
              local.Infrastructure.ProcessStatus = "Q";

              if (ReadCsePerson2())
              {
                // --------------------------------------------------------
                // 4.9.150
                // Beginning Of Change
                // The following statement is commented out because it is 
                // passing 'Export Cse_person' which is not at all used.
                // Replaced with following USE.
                // -------------------------------------------------------
                // --------------------------------------------------------
                // PR80718
                // Beginning Of Change
                // The following statement is commented out because 
                // infrastructure records will not be created at case_unit level
                // but at case level and that code is included here.
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
                // ----------------------------------------------------------
                // Beginning of Change.
                // 4.10.100
                // Handle exit states case_nf and cse_person_nf coming from 
                // oe_cab_raise_event
                // ----------------------------------------------------------
                if (IsExitState("CASE_NF"))
                {
                  ExitState = "ACO_NN0000_ALL_OK";
                }
                else if (IsExitState("CSE_PERSON_NF"))
                {
                  local.EabReportSend.RptDetail =
                    "Cann't insert event because cse person not found in oe_cab_raise_event :" +
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

                // ----------------------------------------------------------
                // End of Change.
                // 4.10.100
                // ----------------------------------------------------------
                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  // :
                  // *********************************************************
                  // * Rollback all updates to the last successfull commit   *
                  // *********************************************************
                  UseEabRollbackSql();
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                }
              }

              // *********End of Insert Event 1099 Response Received******
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "1099_LOCATE_RESPONSE_NU";

                  return;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "1099_LOCATE_RESPONSE_PV";

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }
          else if (AsChar(local.ResponseDuplicate.Flag) == 'Y')
          {
            // -----------------------------------------------------------------
            // 4.9.150
            // Beginning Of Change
            // This else-if is added to write duplicate records to Error Report.
            // -----------------------------------------------------------------
            ++local.ResponseDuplicateCount.Count;
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              export.ExternalOcse1099Response.Ssn + export
              .ExternalOcse1099Response.CsePersonNumber + NumberToString
              (export.ExternalOcse1099Response.Attribute1099RequestIdentifier,
              15) + export.ExternalOcse1099Response.FirstName + " " + export
              .ExternalOcse1099Response.LastName + ": Duplicate Record.";
            UseCabErrorReport1();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            // -----------------------------------------------------------------
            // 4.9.150
            // End Of Change
            // -----------------------------------------------------------------
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabRollbackSql();

            if (local.PassArea.NumericReturnCode != 0)
            {
              ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

              return;
            }
          }
          else
          {
          }
        }
      }
      else
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = export.ExternalOcse1099Response.Ssn + export
          .ExternalOcse1099Response.CsePersonNumber + NumberToString
          (export.ExternalOcse1099Response.Attribute1099RequestIdentifier, 15) +
          export.ExternalOcse1099Response.FirstName + " " + export
          .ExternalOcse1099Response.LastName + ": CSE not found";
        UseCabErrorReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        // -----------------------------------------------------------------
        // 4.9.150
        // Beginning Of Change
        // If cse_person not found, do not abort , go to next record.
        // -----------------------------------------------------------------
        // -----------------------------------------------------------------
        // 4.9.150
        // End Of Change
        // -----------------------------------------------------------------
      }

      ++local.TotalRecordsProcessed.Count;
    }
    while(!Equal(local.PassArea.TextReturnCode, "EF"));

    if (Equal(local.PassArea.TextReturnCode, "EF"))
    {
      // ************************************************
      // *The external hit the end of the driver file,  *
      // *closed the file and returned an EF (EOF)      *
      // *indicator.
      // 
      // *
      // ************************************************
      // ************************************************
      // *          INSTRUCTIONAL NOTE                  *
      // *In this section you should create any control *
      // *totals and close all open files.              *
      // *The input files are automatically closed in   *
      // *the externals when EOF is reached but output  *
      // *files should be explicitly closed.            *
      // ************************************************
      // --------------------------------------------------------------
      // 4.9.150
      // Beginning Of Change
      // Remove Create_program_control_total  and
      // use Cab_control_report to write control totals to Control Report.
      // ---------------------------------------------------------------
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "1099 : Number Of Records Read : " + NumberToString
        (local.NoOf1099RecsRead.Count, 15);
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

      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "1099 : Number Of Response Updates : " + NumberToString
        (local.No1099ResponseUpdates.Count, 15);
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

      local.EabReportSend.RptDetail =
        "1099 : Response- Total Duplicate records: " + NumberToString
        (local.ResponseDuplicateCount.Count, 15);
      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error encountered while writing Control Report (Duplicate Records ).";
          
        UseCabErrorReport1();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      // --------------------------------------------------------------
      // 4.9.150
      // End Of Change
      // --------------------------------------------------------------
      // ************************************************
      // *Record the program end time.                  *
      // ************************************************
      // --------------------------------------------------------------
      // 4.9.150
      // Beginning Of Change
      // Removed 'existing program_processing_info' and added
      // 'export program_processing_info'.
      // ---------------------------------------------------------------
      UseUpdateProgramRun();

      // --------------------------------------------------------------
      // 4.9.150
      // End Of Change
      // --------------------------------------------------------------
      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      // ************************************************
      // * Set restart indicator to no because we       *
      // * successfully finished this program.          *
      // ************************************************
      local.ProgramCheckpointRestart.RestartInfo = "";
      local.ProgramCheckpointRestart.RestartInd = "N";
      UseUpdatePgmCheckpointRestart1();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      // --------------------------------------------------------------
      // 4.9.150
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
      // 4.9.150
      // End Of Change
      // ---------------------------------------------------------------
      ExitState = "ACO_NN0000_ALL_OK";

      return;
    }
    else
    {
      // ************************************************
      // *Record the number of checkpoints and the last *
      // *checkpoint time and set the restart indicator *
      // *to yes.
      // 
      // *
      // ************************************************
      local.ProgramCheckpointRestart.CheckpointCount =
        local.ProgramCheckpointRestart.CheckpointCount.GetValueOrDefault() + 1;
      local.ProgramCheckpointRestart.LastCheckpointTimestamp = Now();
      local.ProgramCheckpointRestart.RestartInd = "N";

      // ************************************************
      // *We need to store the last Case Number used and*
      // *the total number of records processed to      *
      // *assist in the Restart Operation.              *
      // ************************************************
      local.ProgramCheckpointRestart.RestartInfo =
        entities.ExistingAp.Number + NumberToString
        (local.TotalRecordsProcessed.Count, 15);
      UseUpdatePgmCheckpointRestart1();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      ExitState = "DO_A_RECURSIVE_FLOW";

      return;
    }

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

  private static void MoveProgramProcessingInfo1(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveProgramProcessingInfo2(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ParameterList = source.ParameterList;
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

    MoveProgramProcessingInfo1(export.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);
    useImport.ProgramRun.FromRestartInd = export.ProgramRun.FromRestartInd;

    Call(CreateProgramRun.Execute, useImport, useExport);

    export.ProgramRun.StartTimestamp = useExport.ProgramRun.StartTimestamp;
  }

  private void UseEabRollbackSql()
  {
    var useImport = new EabRollbackSql.Import();
    var useExport = new EabRollbackSql.Export();

    useExport.External.NumericReturnCode = local.PassArea.NumericReturnCode;

    Call(EabRollbackSql.Execute, useImport, useExport);

    local.PassArea.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseOeEabReceive1099Response1()
  {
    var useImport = new OeEabReceive1099Response.Import();
    var useExport = new OeEabReceive1099Response.Export();

    useImport.RestartAfterThisCount.Count = local.TotalRecordsProcessed.Count;
    useImport.External.FileInstruction = local.PassArea.FileInstruction;
    useExport.External.Assign(local.PassArea);

    Call(OeEabReceive1099Response.Execute, useImport, useExport);

    local.PassArea.Assign(useExport.External);
  }

  private void UseOeEabReceive1099Response2()
  {
    var useImport = new OeEabReceive1099Response.Import();
    var useExport = new OeEabReceive1099Response.Export();

    useImport.External.FileInstruction = local.PassArea.FileInstruction;
    useExport.External.Assign(local.PassArea);

    Call(OeEabReceive1099Response.Execute, useImport, useExport);

    local.PassArea.Assign(useExport.External);
  }

  private void UseOeEabReceive1099Response3()
  {
    var useImport = new OeEabReceive1099Response.Import();
    var useExport = new OeEabReceive1099Response.Export();

    useImport.External.FileInstruction = local.PassArea.FileInstruction;
    useExport.ExternalOcse1099Response.Assign(export.ExternalOcse1099Response);
    useExport.External.Assign(local.PassArea);

    Call(OeEabReceive1099Response.Execute, useImport, useExport);

    export.ExternalOcse1099Response.Assign(useExport.ExternalOcse1099Response);
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

    export.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    MoveInfrastructure(useExport.Infrastructure, local.Infrastructure);
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

    MoveProgramProcessingInfo1(export.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);
    useImport.ProgramRun.StartTimestamp = export.ProgramRun.StartTimestamp;

    Call(UpdateProgramRun.Execute, useImport, useExport);
  }

  private void Create1099LocateRequest()
  {
    var cspNumber = entities.ExistingCsePerson.Number;
    var identifier = 1;
    var ssn = export.ExternalOcse1099Response.Ssn;
    var localCode = "000";
    var lastName = export.ExternalOcse1099Response.LastName;
    var caseIdNo =
      Substring(entities.ExistingCsePerson.Number +
      NumberToString(entities.Existingdata1099LocateRequest.Identifier, 15), 1,
      15);
    var firstName = export.ExternalOcse1099Response.FirstName;

    entities.Existingdata1099LocateRequest.Populated = false;

    // WARNING: An attribute LOCAL_CODE(371440625) refers to view NEW(374403678)
    // instead of view (372375021). WARNING: An attribute NO_MATCH_CODE(
    // 371440630) refers to view NEW(374403678) instead of view (372375021).
    Update("Create1099LocateRequest",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "identifier", identifier);
        db.SetString(command, "ssn", ssn);
        db.SetNullableString(command, "localCode", localCode);
        db.SetNullableString(command, "lastName", lastName);
        db.SetNullableString(command, "afdcCode", "");
        db.SetNullableString(command, "caseIdNo", caseIdNo);
        db.SetNullableString(command, "ctOrAdmOrdInd", "");
        db.SetNullableString(command, "noMatchCode", "");
        db.SetString(command, "createdBy", "");
        db.SetDateTime(command, "createdTimestamp", default(DateTime));
        db.SetNullableString(command, "firstName", firstName);
        db.SetNullableDate(command, "requestSentDate", default(DateTime));
        db.SetNullableString(command, "middleInitial", "");
      });

    entities.Existingdata1099LocateRequest.CspNumber = cspNumber;
    entities.Existingdata1099LocateRequest.Identifier = identifier;
    entities.Existingdata1099LocateRequest.Ssn = ssn;
    entities.Existingdata1099LocateRequest.LocalCode = localCode;
    entities.Existingdata1099LocateRequest.LastName = lastName;
    entities.Existingdata1099LocateRequest.AfdcCode = "";
    entities.Existingdata1099LocateRequest.CaseIdNo = caseIdNo;
    entities.Existingdata1099LocateRequest.CourtOrAdminOrdInd = "";
    entities.Existingdata1099LocateRequest.NoMatchCode = "";
    entities.Existingdata1099LocateRequest.FirstName = firstName;
    entities.Existingdata1099LocateRequest.Populated = true;
  }

  private void Create1099LocateResponse()
  {
    System.Diagnostics.Debug.Assert(
      entities.Existingdata1099LocateRequest.Populated);

    var lrqIdentifier = entities.Existingdata1099LocateRequest.Identifier;
    var cspNumber = entities.Existingdata1099LocateRequest.CspNumber;
    var identifier = local.Last1099Response.Identifier + 1;
    var dateReceived = local.Data1099LocateResponse.DateReceived;
    var dateUsed = local.NullDate.Date;
    var stateCode = local.Data1099LocateResponse.StateCode ?? "";
    var zipCode = local.Data1099LocateResponse.ZipCode ?? "";
    var payerEin = local.Data1099LocateResponse.PayerEin ?? "";
    var taxYear = local.Data1099LocateResponse.TaxYear.GetValueOrDefault();
    var payerAccountNo = local.Data1099LocateResponse.PayerAccountNo ?? "";
    var documentCode = local.Data1099LocateResponse.DocumentCode ?? "";
    var amountInd1 = local.Data1099LocateResponse.AmountInd1 ?? "";
    var amount1 = local.Data1099LocateResponse.Amount1.GetValueOrDefault();
    var amountInd2 = local.Data1099LocateResponse.AmountInd2 ?? "";
    var amount2 = local.Data1099LocateResponse.Amount2.GetValueOrDefault();
    var amountInd3 = local.Data1099LocateResponse.AmountInd3 ?? "";
    var amount3 = local.Data1099LocateResponse.Amount3.GetValueOrDefault();
    var amountInd4 = local.Data1099LocateResponse.AmountInd4 ?? "";
    var amount4 = local.Data1099LocateResponse.Amount4.GetValueOrDefault();
    var amountInd5 = local.Data1099LocateResponse.AmountInd5 ?? "";
    var amount5 = local.Data1099LocateResponse.Amount5.GetValueOrDefault();
    var createdBy = global.TranCode;
    var createdTimestamp = Now();
    var amountInd6 = local.Data1099LocateResponse.AmountInd6 ?? "";
    var amount6 = local.Data1099LocateResponse.Amount6.GetValueOrDefault();
    var amountInd7 = local.Data1099LocateResponse.AmountInd7 ?? "";
    var amount7 = local.Data1099LocateResponse.Amount7.GetValueOrDefault();
    var amountInd8 = local.Data1099LocateResponse.AmountInd8 ?? "";
    var amount8 = local.Data1099LocateResponse.Amount8.GetValueOrDefault();
    var amountInd9 = local.Data1099LocateResponse.AmountInd9 ?? "";
    var amount9 = local.Data1099LocateResponse.Amount9.GetValueOrDefault();
    var amountInd10 = local.Data1099LocateResponse.AmountInd10 ?? "";
    var amount10 = local.Data1099LocateResponse.Amount10.GetValueOrDefault();
    var amountInd11 = local.Data1099LocateResponse.AmountInd11 ?? "";
    var amount11 = local.Data1099LocateResponse.Amount11.GetValueOrDefault();
    var amountInd12 = local.Data1099LocateResponse.AmountInd12 ?? "";
    var amount12 = local.Data1099LocateResponse.Amount12.GetValueOrDefault();
    var payeeLine1 = local.Data1099LocateResponse.PayeeLine1 ?? "";
    var payeeLine2 = local.Data1099LocateResponse.PayeeLine2 ?? "";
    var payeeLine3 = local.Data1099LocateResponse.PayeeLine3 ?? "";
    var payeeLine4 = local.Data1099LocateResponse.PayeeLine4 ?? "";
    var payerLine1 = local.Data1099LocateResponse.PayerLine1 ?? "";
    var payerLine2 = local.Data1099LocateResponse.PayerLine2 ?? "";
    var payerLine3 = local.Data1099LocateResponse.PayerLine3 ?? "";
    var payerLine4 = local.Data1099LocateResponse.PayerLine4 ?? "";

    entities.Existingdata1099LocateResponse.Populated = false;
    Update("Create1099LocateResponse",
      (db, command) =>
      {
        db.SetInt32(command, "lrqIdentifier", lrqIdentifier);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "identifier", identifier);
        db.SetNullableDate(command, "dateReceived", dateReceived);
        db.SetNullableString(command, "usageStatus", "");
        db.SetNullableDate(command, "dateUsed", dateUsed);
        db.SetNullableString(command, "stateCode", stateCode);
        db.SetNullableString(command, "zipCode", zipCode);
        db.SetNullableString(command, "payerEin", payerEin);
        db.SetNullableInt32(command, "taxYear", taxYear);
        db.SetNullableString(command, "payerAccountNo", payerAccountNo);
        db.SetNullableString(command, "documentCode", documentCode);
        db.SetNullableString(command, "amountInd1", amountInd1);
        db.SetNullableInt64(command, "amount1", amount1);
        db.SetNullableString(command, "amountInd2", amountInd2);
        db.SetNullableInt64(command, "amount2", amount2);
        db.SetNullableString(command, "amountInd3", amountInd3);
        db.SetNullableInt64(command, "amount3", amount3);
        db.SetNullableString(command, "amountInd4", amountInd4);
        db.SetNullableInt64(command, "amount4", amount4);
        db.SetNullableString(command, "amountInd5", amountInd5);
        db.SetNullableInt64(command, "amount5", amount5);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", createdBy);
        db.SetDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetNullableString(command, "amountInd6", amountInd6);
        db.SetNullableInt64(command, "amount6", amount6);
        db.SetNullableString(command, "amountInd7", amountInd7);
        db.SetNullableInt64(command, "amount7", amount7);
        db.SetNullableString(command, "amountInd8", amountInd8);
        db.SetNullableInt64(command, "amount8", amount8);
        db.SetNullableString(command, "amountInd9", amountInd9);
        db.SetNullableInt64(command, "amount9", amount9);
        db.SetNullableString(command, "amountInd10", amountInd10);
        db.SetNullableInt64(command, "amount10", amount10);
        db.SetNullableString(command, "amountInd11", amountInd11);
        db.SetNullableInt64(command, "amount11", amount11);
        db.SetNullableString(command, "amountInd12", amountInd12);
        db.SetNullableInt64(command, "amount12", amount12);
        db.SetNullableString(command, "payeeLine1", payeeLine1);
        db.SetNullableString(command, "payeeLine2", payeeLine2);
        db.SetNullableString(command, "payeeLine3", payeeLine3);
        db.SetNullableString(command, "payeeLine4", payeeLine4);
        db.SetNullableString(command, "payerLine1", payerLine1);
        db.SetNullableString(command, "payerLine2", payerLine2);
        db.SetNullableString(command, "payerLine3", payerLine3);
        db.SetNullableString(command, "payerLine4", payerLine4);
      });

    entities.Existingdata1099LocateResponse.LrqIdentifier = lrqIdentifier;
    entities.Existingdata1099LocateResponse.CspNumber = cspNumber;
    entities.Existingdata1099LocateResponse.Identifier = identifier;
    entities.Existingdata1099LocateResponse.DateReceived = dateReceived;
    entities.Existingdata1099LocateResponse.UsageStatus = "";
    entities.Existingdata1099LocateResponse.DateUsed = dateUsed;
    entities.Existingdata1099LocateResponse.StateCode = stateCode;
    entities.Existingdata1099LocateResponse.ZipCode = zipCode;
    entities.Existingdata1099LocateResponse.PayerEin = payerEin;
    entities.Existingdata1099LocateResponse.TaxYear = taxYear;
    entities.Existingdata1099LocateResponse.PayerAccountNo = payerAccountNo;
    entities.Existingdata1099LocateResponse.DocumentCode = documentCode;
    entities.Existingdata1099LocateResponse.AmountInd1 = amountInd1;
    entities.Existingdata1099LocateResponse.Amount1 = amount1;
    entities.Existingdata1099LocateResponse.AmountInd2 = amountInd2;
    entities.Existingdata1099LocateResponse.Amount2 = amount2;
    entities.Existingdata1099LocateResponse.AmountInd3 = amountInd3;
    entities.Existingdata1099LocateResponse.Amount3 = amount3;
    entities.Existingdata1099LocateResponse.AmountInd4 = amountInd4;
    entities.Existingdata1099LocateResponse.Amount4 = amount4;
    entities.Existingdata1099LocateResponse.AmountInd5 = amountInd5;
    entities.Existingdata1099LocateResponse.Amount5 = amount5;
    entities.Existingdata1099LocateResponse.CreatedBy = createdBy;
    entities.Existingdata1099LocateResponse.CreatedTimestamp = createdTimestamp;
    entities.Existingdata1099LocateResponse.LastUpdatedBy = createdBy;
    entities.Existingdata1099LocateResponse.LastUpdatedTimestamp =
      createdTimestamp;
    entities.Existingdata1099LocateResponse.AmountInd6 = amountInd6;
    entities.Existingdata1099LocateResponse.Amount6 = amount6;
    entities.Existingdata1099LocateResponse.AmountInd7 = amountInd7;
    entities.Existingdata1099LocateResponse.Amount7 = amount7;
    entities.Existingdata1099LocateResponse.AmountInd8 = amountInd8;
    entities.Existingdata1099LocateResponse.Amount8 = amount8;
    entities.Existingdata1099LocateResponse.AmountInd9 = amountInd9;
    entities.Existingdata1099LocateResponse.Amount9 = amount9;
    entities.Existingdata1099LocateResponse.AmountInd10 = amountInd10;
    entities.Existingdata1099LocateResponse.Amount10 = amount10;
    entities.Existingdata1099LocateResponse.AmountInd11 = amountInd11;
    entities.Existingdata1099LocateResponse.Amount11 = amount11;
    entities.Existingdata1099LocateResponse.AmountInd12 = amountInd12;
    entities.Existingdata1099LocateResponse.Amount12 = amount12;
    entities.Existingdata1099LocateResponse.PayeeLine1 = payeeLine1;
    entities.Existingdata1099LocateResponse.PayeeLine2 = payeeLine2;
    entities.Existingdata1099LocateResponse.PayeeLine3 = payeeLine3;
    entities.Existingdata1099LocateResponse.PayeeLine4 = payeeLine4;
    entities.Existingdata1099LocateResponse.PayerLine1 = payerLine1;
    entities.Existingdata1099LocateResponse.PayerLine2 = payerLine2;
    entities.Existingdata1099LocateResponse.PayerLine3 = payerLine3;
    entities.Existingdata1099LocateResponse.PayerLine4 = payerLine4;
    entities.Existingdata1099LocateResponse.Populated = true;
  }

  private bool Read1099LocateRequest()
  {
    entities.Existingdata1099LocateRequest.Populated = false;

    return Read("Read1099LocateRequest",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
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
        entities.Existingdata1099LocateRequest.Populated = true;
      });
  }

  private bool Read1099LocateResponse()
  {
    System.Diagnostics.Debug.Assert(
      entities.Existingdata1099LocateRequest.Populated);
    entities.Existingdata1099LocateResponse.Populated = false;

    return Read("Read1099LocateResponse",
      (db, command) =>
      {
        db.SetInt32(
          command, "lrqIdentifier",
          entities.Existingdata1099LocateRequest.Identifier);
        db.SetString(
          command, "cspNumber",
          entities.Existingdata1099LocateRequest.CspNumber);
      },
      (db, reader) =>
      {
        entities.Existingdata1099LocateResponse.LrqIdentifier =
          db.GetInt32(reader, 0);
        entities.Existingdata1099LocateResponse.CspNumber =
          db.GetString(reader, 1);
        entities.Existingdata1099LocateResponse.Identifier =
          db.GetInt32(reader, 2);
        entities.Existingdata1099LocateResponse.DateReceived =
          db.GetNullableDate(reader, 3);
        entities.Existingdata1099LocateResponse.UsageStatus =
          db.GetNullableString(reader, 4);
        entities.Existingdata1099LocateResponse.DateUsed =
          db.GetNullableDate(reader, 5);
        entities.Existingdata1099LocateResponse.StateCode =
          db.GetNullableString(reader, 6);
        entities.Existingdata1099LocateResponse.ZipCode =
          db.GetNullableString(reader, 7);
        entities.Existingdata1099LocateResponse.PayerEin =
          db.GetNullableString(reader, 8);
        entities.Existingdata1099LocateResponse.TaxYear =
          db.GetNullableInt32(reader, 9);
        entities.Existingdata1099LocateResponse.PayerAccountNo =
          db.GetNullableString(reader, 10);
        entities.Existingdata1099LocateResponse.DocumentCode =
          db.GetNullableString(reader, 11);
        entities.Existingdata1099LocateResponse.AmountInd1 =
          db.GetNullableString(reader, 12);
        entities.Existingdata1099LocateResponse.Amount1 =
          db.GetNullableInt64(reader, 13);
        entities.Existingdata1099LocateResponse.AmountInd2 =
          db.GetNullableString(reader, 14);
        entities.Existingdata1099LocateResponse.Amount2 =
          db.GetNullableInt64(reader, 15);
        entities.Existingdata1099LocateResponse.AmountInd3 =
          db.GetNullableString(reader, 16);
        entities.Existingdata1099LocateResponse.Amount3 =
          db.GetNullableInt64(reader, 17);
        entities.Existingdata1099LocateResponse.AmountInd4 =
          db.GetNullableString(reader, 18);
        entities.Existingdata1099LocateResponse.Amount4 =
          db.GetNullableInt64(reader, 19);
        entities.Existingdata1099LocateResponse.AmountInd5 =
          db.GetNullableString(reader, 20);
        entities.Existingdata1099LocateResponse.Amount5 =
          db.GetNullableInt64(reader, 21);
        entities.Existingdata1099LocateResponse.CreatedBy =
          db.GetString(reader, 22);
        entities.Existingdata1099LocateResponse.CreatedTimestamp =
          db.GetDateTime(reader, 23);
        entities.Existingdata1099LocateResponse.LastUpdatedBy =
          db.GetString(reader, 24);
        entities.Existingdata1099LocateResponse.LastUpdatedTimestamp =
          db.GetDateTime(reader, 25);
        entities.Existingdata1099LocateResponse.AmountInd6 =
          db.GetNullableString(reader, 26);
        entities.Existingdata1099LocateResponse.Amount6 =
          db.GetNullableInt64(reader, 27);
        entities.Existingdata1099LocateResponse.AmountInd7 =
          db.GetNullableString(reader, 28);
        entities.Existingdata1099LocateResponse.Amount7 =
          db.GetNullableInt64(reader, 29);
        entities.Existingdata1099LocateResponse.AmountInd8 =
          db.GetNullableString(reader, 30);
        entities.Existingdata1099LocateResponse.Amount8 =
          db.GetNullableInt64(reader, 31);
        entities.Existingdata1099LocateResponse.AmountInd9 =
          db.GetNullableString(reader, 32);
        entities.Existingdata1099LocateResponse.Amount9 =
          db.GetNullableInt64(reader, 33);
        entities.Existingdata1099LocateResponse.AmountInd10 =
          db.GetNullableString(reader, 34);
        entities.Existingdata1099LocateResponse.Amount10 =
          db.GetNullableInt64(reader, 35);
        entities.Existingdata1099LocateResponse.AmountInd11 =
          db.GetNullableString(reader, 36);
        entities.Existingdata1099LocateResponse.Amount11 =
          db.GetNullableInt64(reader, 37);
        entities.Existingdata1099LocateResponse.AmountInd12 =
          db.GetNullableString(reader, 38);
        entities.Existingdata1099LocateResponse.Amount12 =
          db.GetNullableInt64(reader, 39);
        entities.Existingdata1099LocateResponse.PayeeLine1 =
          db.GetNullableString(reader, 40);
        entities.Existingdata1099LocateResponse.PayeeLine2 =
          db.GetNullableString(reader, 41);
        entities.Existingdata1099LocateResponse.PayeeLine3 =
          db.GetNullableString(reader, 42);
        entities.Existingdata1099LocateResponse.PayeeLine4 =
          db.GetNullableString(reader, 43);
        entities.Existingdata1099LocateResponse.PayerLine1 =
          db.GetNullableString(reader, 44);
        entities.Existingdata1099LocateResponse.PayerLine2 =
          db.GetNullableString(reader, 45);
        entities.Existingdata1099LocateResponse.PayerLine3 =
          db.GetNullableString(reader, 46);
        entities.Existingdata1099LocateResponse.PayerLine4 =
          db.GetNullableString(reader, 47);
        entities.Existingdata1099LocateResponse.Populated = true;
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

  private bool ReadCsePerson1()
  {
    entities.ExistingCsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(
          command, "numb", export.ExternalOcse1099Response.CsePersonNumber);
      },
      (db, reader) =>
      {
        entities.ExistingCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingCsePerson.Type1 = db.GetString(reader, 1);
        entities.ExistingCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ExistingCsePerson.Type1);
      });
  }

  private bool ReadCsePerson2()
  {
    System.Diagnostics.Debug.Assert(
      entities.Existingdata1099LocateRequest.Populated);
    entities.ExistingCsePerson.Populated = false;

    return Read("ReadCsePerson2",
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
          export.ProgramRun.StartTimestamp.GetValueOrDefault());
        db.SetString(command, "ppiName", global.UserId);
        db.SetDateTime(
          command, "ppiCreatedTstamp",
          export.ProgramProcessingInfo.CreatedTimestamp.GetValueOrDefault());
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

    var noMatchCode = export.ExternalOcse1099Response.OcseMatchCode;

    entities.Existingdata1099LocateRequest.Populated = false;
    Update("Update1099LocateRequest",
      (db, command) =>
      {
        db.SetNullableString(command, "noMatchCode", noMatchCode);
        db.SetString(
          command, "cspNumber",
          entities.Existingdata1099LocateRequest.CspNumber);
        db.SetInt32(
          command, "identifier",
          entities.Existingdata1099LocateRequest.Identifier);
      });

    entities.Existingdata1099LocateRequest.NoMatchCode = noMatchCode;
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
    /// A value of BatchAttributes.
    /// </summary>
    [JsonPropertyName("batchAttributes")]
    public BatchAttributes BatchAttributes
    {
      get => batchAttributes ??= new();
      set => batchAttributes = value;
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

    private BatchAttributes batchAttributes;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramRun programRun;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ExternalOcse1099Response.
    /// </summary>
    [JsonPropertyName("externalOcse1099Response")]
    public ExternalOcse1099Response ExternalOcse1099Response
    {
      get => externalOcse1099Response ??= new();
      set => externalOcse1099Response = value;
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
    /// A value of BatchAttributes.
    /// </summary>
    [JsonPropertyName("batchAttributes")]
    public BatchAttributes BatchAttributes
    {
      get => batchAttributes ??= new();
      set => batchAttributes = value;
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

    private ExternalOcse1099Response externalOcse1099Response;
    private CsePerson csePerson;
    private BatchAttributes batchAttributes;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramRun programRun;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ResponseDuplicateCount.
    /// </summary>
    [JsonPropertyName("responseDuplicateCount")]
    public Common ResponseDuplicateCount
    {
      get => responseDuplicateCount ??= new();
      set => responseDuplicateCount = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of ResponseDuplicate.
    /// </summary>
    [JsonPropertyName("responseDuplicate")]
    public Common ResponseDuplicate
    {
      get => responseDuplicate ??= new();
      set => responseDuplicate = value;
    }

    /// <summary>
    /// A value of ResponseFound.
    /// </summary>
    [JsonPropertyName("responseFound")]
    public Common ResponseFound
    {
      get => responseFound ??= new();
      set => responseFound = value;
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
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public Case1 Restart
    {
      get => restart ??= new();
      set => restart = value;
    }

    /// <summary>
    /// A value of TotalRecordsProcessed.
    /// </summary>
    [JsonPropertyName("totalRecordsProcessed")]
    public Common TotalRecordsProcessed
    {
      get => totalRecordsProcessed ??= new();
      set => totalRecordsProcessed = value;
    }

    /// <summary>
    /// A value of Last1099Response.
    /// </summary>
    [JsonPropertyName("last1099Response")]
    public Data1099LocateResponse Last1099Response
    {
      get => last1099Response ??= new();
      set => last1099Response = value;
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
    /// A value of Data1099LocateResponse.
    /// </summary>
    [JsonPropertyName("data1099LocateResponse")]
    public Data1099LocateResponse Data1099LocateResponse
    {
      get => data1099LocateResponse ??= new();
      set => data1099LocateResponse = value;
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
    /// A value of NoOfExtFileRecsWrittn.
    /// </summary>
    [JsonPropertyName("noOfExtFileRecsWrittn")]
    public Common NoOfExtFileRecsWrittn
    {
      get => noOfExtFileRecsWrittn ??= new();
      set => noOfExtFileRecsWrittn = value;
    }

    /// <summary>
    /// A value of NoOf1099RecsRead.
    /// </summary>
    [JsonPropertyName("noOf1099RecsRead")]
    public Common NoOf1099RecsRead
    {
      get => noOf1099RecsRead ??= new();
      set => noOf1099RecsRead = value;
    }

    /// <summary>
    /// A value of No1099ResponseUpdates.
    /// </summary>
    [JsonPropertyName("no1099ResponseUpdates")]
    public Common No1099ResponseUpdates
    {
      get => no1099ResponseUpdates ??= new();
      set => no1099ResponseUpdates = value;
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
    /// A value of PassArea.
    /// </summary>
    [JsonPropertyName("passArea")]
    public External PassArea
    {
      get => passArea ??= new();
      set => passArea = value;
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
    /// A value of NumberOfUpdates.
    /// </summary>
    [JsonPropertyName("numberOfUpdates")]
    public Common NumberOfUpdates
    {
      get => numberOfUpdates ??= new();
      set => numberOfUpdates = value;
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

    private Common responseDuplicateCount;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private DateWorkArea convertDateDateWorkArea;
    private TextWorkArea convertDateTextWorkArea;
    private Infrastructure infrastructure;
    private Common responseDuplicate;
    private Common responseFound;
    private DateWorkArea nullDate;
    private Case1 restart;
    private Common totalRecordsProcessed;
    private Data1099LocateResponse last1099Response;
    private Common local1099RequestFound;
    private Data1099LocateResponse data1099LocateResponse;
    private Case1 case1;
    private Common fileOpened;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Data1099LocateRequest data1099LocateRequest;
    private Common noOfExtFileRecsWrittn;
    private Common noOf1099RecsRead;
    private Common no1099ResponseUpdates;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
    private CsePerson csePerson;
    private ProgramError programError;
    private ProgramControlTotal programControlTotal;
    private External passArea;
    private Common numberOfReads;
    private Common numberOfUpdates;
    private Common noOf1099RequestsSent;
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
    /// A value of Existingdata1099LocateResponse.
    /// </summary>
    [JsonPropertyName("existingdata1099LocateResponse")]
    public Data1099LocateResponse Existingdata1099LocateResponse
    {
      get => existingdata1099LocateResponse ??= new();
      set => existingdata1099LocateResponse = value;
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
    /// A value of ExistingAp.
    /// </summary>
    [JsonPropertyName("existingAp")]
    public Case1 ExistingAp
    {
      get => existingAp ??= new();
      set => existingAp = value;
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
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public Data1099LocateRequest New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    private InterstateRequest interstateRequest;
    private CaseUnit caseUnit;
    private Case1 case1;
    private Data1099LocateResponse existingdata1099LocateResponse;
    private Data1099LocateRequest existingdata1099LocateRequest;
    private Case1 existingAp;
    private CsePerson existingCsePerson;
    private ProgramRun existingProgramRun;
    private ProgramProcessingInfo existingProgramProcessingInfo;
    private Data1099LocateRequest new1;
  }
#endregion
}
