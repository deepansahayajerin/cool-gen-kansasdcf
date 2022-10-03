// Program: FN_B798_TEXTING_OPT_OUTS, ID: 1902556252, model: 746.
// Short name: SWEF798B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B798_TEXTING_OPT_OUTS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB798TextingOptOuts: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B798_TEXTING_OPT_OUTS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB798TextingOptOuts(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB798TextingOptOuts.
  /// </summary>
  public FnB798TextingOptOuts(IContext context, Import import, Export export):
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
    // 07/07/16  GVandy	CQ52170		Initial Development.
    // --------------------------------------------------------------------------------------------------
    // --------------------------------------------------------------------------------------------------
    // Overview
    //   1. Process a file containing cell phone numbers that have opted out of 
    // text messaging.
    //   2. Update each cse person record with the cell phone number to indicate
    // they do not wish
    //      to receive text communications.
    //   3. Create HIST records for each active case role for the person 
    // indicating they have
    //      opted out.
    //   4. If more than one person shares the same cell phone number then mark 
    // each person as
    //      having opted out and log an informational message to the error 
    // report.
    //   5. Log totals to the control report.
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
      // 	001-012   Last Cell Phone Number Processed
      // 	013-013   Blank
      // 	014-022   Total Number of Phone Numbers Read from the File
      // 	023-023   Blank
      // 	024-032   Total Number of CSE Person opted out
      // -------------------------------------------------------------------------------------
      local.Restart.OtherAreaCode =
        (int?)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 1, 3));
      local.Restart.OtherNumber =
        (int?)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 5, 3) +
        Substring(local.ProgramCheckpointRestart.RestartInfo, 250, 9, 4));
      local.TotalNumbOptOutPhones.Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 14, 9));
      local.TotalNumbOptedOut.Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 24, 9));

      // -------------------------------------------------------------------------------------
      // --  Open the Input File.
      // -------------------------------------------------------------------------------------
      local.EabFileHandling.Action = "OPEN";
      UseFnB798ReadTextOptOutFile2();

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
        UseFnB798ReadTextOptOutFile1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          if (Equal(local.EabFileHandling.Status, "EF"))
          {
            local.EabReportSend.RptDetail =
              "End of file encountered before finding restart NCP " + local
              .Restart.Type1;
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

        if (Equal(ToUpper(
          Substring(local.FileRecord.Text12, WorkArea.Text12_MaxLength, 1, 6)),
          "NUMBER"))
        {
          // -- This is the header record in the file.  Skip to the next record.
          continue;
        }

        local.OptOut.OtherAreaCode =
          (int?)StringToNumber(Substring(
            local.FileRecord.Text12, WorkArea.Text12_MaxLength, 1, 3));
        local.OptOut.OtherNumber =
          (int?)StringToNumber(Substring(
            local.FileRecord.Text12, WorkArea.Text12_MaxLength, 5, 3) +
          Substring(local.FileRecord.Text12, WorkArea.Text12_MaxLength, 9, 4));
      }
      while(local.OptOut.OtherAreaCode.GetValueOrDefault() != local
        .Restart.OtherAreaCode.GetValueOrDefault() || local
        .OptOut.OtherNumber.GetValueOrDefault() != local
        .Restart.OtherNumber.GetValueOrDefault());

      // -------------------------------------------------------------------------------------
      // --  Log restart info to the Control Report.
      // -------------------------------------------------------------------------------------
      for(local.Common.Count = 1; local.Common.Count <= 7; ++local.Common.Count)
      {
        switch(local.Common.Count)
        {
          case 1:
            local.EabReportSend.RptDetail = "Restarting at phone number " + local
              .FileRecord.Text12;

            break;
          case 3:
            local.EabReportSend.RptDetail =
              "Number of Phone #s Read from Blacklist File in Previous Run.........." +
              NumberToString(local.TotalNumbOptOutPhones.Count, 9, 7);

            break;
          case 4:
            local.EabReportSend.RptDetail =
              "Number of CSE Persons Opted Out of Text Messaging in Previous Run...." +
              NumberToString(local.TotalNumbOptedOut.Count, 9, 7);

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
      local.TotalNumbOptOutPhones.Count = 0;
      local.TotalNumbOptedOut.Count = 0;

      // -------------------------------------------------------------------------------------
      // --  Open the Input File.
      // -------------------------------------------------------------------------------------
      local.EabFileHandling.Action = "OPEN";
      UseFnB798ReadTextOptOutFile2();

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
    // -- Read each opt out cell phone number from the input file.
    // -------------------------------------------------------------------------------------
    do
    {
      // ---------------------------------------------------------------------------------------------------
      // OPT OUT FILE RECORD LAYOUT
      // 			Data
      // Data Element		Type	Length	Start	End	Note
      // ---------------------	-------	
      // ------	-----	---	
      // ----------------------------------
      // Opt Out Phone Number	Text	12	1	12	Format XXX-XXX-XXXX
      // ---------------------------------------------------------------------------------------------------
      ++local.NumbRecordsSinceChckpnt.Count;

      // -------------------------------------------------------------------------------------
      // -- Read opt out record from the input file.
      // -------------------------------------------------------------------------------------
      local.EabFileHandling.Action = "READ";
      UseFnB798ReadTextOptOutFile1();

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

      if (Equal(ToUpper(
        Substring(local.FileRecord.Text12, WorkArea.Text12_MaxLength, 1, 6)),
        "NUMBER"))
      {
        // -- This is the header record in the file.  Skip to the next record.
        continue;
      }

      ++local.TotalNumbOptOutPhones.Count;

      // -------------------------------------------------------------------------------------
      // -- Extract cell phone number from the opt out file record.
      // -------------------------------------------------------------------------------------
      local.OptOut.OtherAreaCode =
        (int?)StringToNumber(Substring(
          local.FileRecord.Text12, WorkArea.Text12_MaxLength, 1, 3));
      local.OptOut.OtherNumber =
        (int?)StringToNumber(Substring(
          local.FileRecord.Text12, WorkArea.Text12_MaxLength, 5, 3) +
        Substring(local.FileRecord.Text12, WorkArea.Text12_MaxLength, 9, 4));

      // -------------------------------------------------------------------------------------
      // -- Determine how many people use this cell phone number and are not 
      // already opted out.
      // -------------------------------------------------------------------------------------
      ReadCsePerson1();

      if (local.NumbPersonsUsingPhone.Count > 0)
      {
        local.Infrastructure.Assign(local.NullInfrastructure);

        // -- Find each person using the cell phone number who is not already 
        // opted out.
        foreach(var item in ReadCsePerson2())
        {
          for(local.Common.Count = 1; local.Common.Count <= 2; ++
            local.Common.Count)
          {
            local.Infrastructure.UserId = global.UserId;
            local.Infrastructure.BusinessObjectCd = "CAU";
            local.Infrastructure.ReferenceDate = local.NullDateWorkArea.Date;
            local.Infrastructure.EventId = 46;
            local.Infrastructure.Detail =
              "The text message indicator has been changed from: " + entities
              .CsePerson.TextMessageIndicator + " to " + "N";

            switch(local.Common.Count)
            {
              case 1:
                local.Apar.Text1 = "R";
                local.Infrastructure.ReasonCode = "TXTMSGUPDAR";

                break;
              case 2:
                local.Apar.Text1 = "P";
                local.Infrastructure.ReasonCode = "TXTMSGUPDAP";

                break;
              default:
                break;
            }

            // -- Action block si_addr_raise_event needs exit state set to 
            // aco_ni0000_sucessful_add in order to function correctly.
            ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
            UseSiAddrRaiseEvent();

            if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
            {
              // -- Reset exit state to aco_nn0000_all_ok
              ExitState = "ACO_NN0000_ALL_OK";
            }
            else
            {
              UseEabExtractExitStateMessage();
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "Error from si_addr_raise_event... " + local
                .ExitStateWorkArea.Message;
              UseCabErrorReport2();

              // -- Set Abort exit state and escape...
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              goto AfterCycle;
            }
          }

          try
          {
            UpdateCsePerson();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "CSE_PERSON not unique.  CSE Person number " + entities
                  .CsePerson.Number;
                UseCabErrorReport2();

                // -- Set Abort exit state and escape...
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                goto AfterCycle;
              case ErrorCode.PermittedValueViolation:
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "CSE_PERSON permitted value violation.  CSE Person number " +
                  entities.CsePerson.Number;
                UseCabErrorReport2();

                // -- Set Abort exit state and escape...
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                goto AfterCycle;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          ++local.TotalNumbOptedOut.Count;

          if (local.NumbPersonsUsingPhone.Count > 1)
          {
            // -- Log there are multiple people using the phone number.
            local.EabReportSend.RptDetail = "Phone Number " + local
              .FileRecord.Text12 + " used by multiple people.  CSP Number " + entities
              .CsePerson.Number;
            local.EabFileHandling.Action = "WRITE";
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              // -- Set Abort exit state and escape...
              ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

              goto Test1;
            }
          }
        }
      }

Test1:

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
        // 	001-012   Last Cell Phone Number Processed
        // 	013-013   Blank
        // 	014-022   Total Number of Phone Numbers Read from the File
        // 	023-023   Blank
        // 	024-032   Total Number of CSE Person opted out
        // -------------------------------------------------------------------------------------
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.RestartInfo = local.FileRecord.Text12 + " " +
          NumberToString(local.TotalNumbOptOutPhones.Count, 7, 9) + " " + NumberToString
          (local.TotalNumbOptedOut.Count, 7, 9);
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

AfterCycle:

    // -------------------------------------------------------------------------------------
    // --  Check for an empty file.
    // -------------------------------------------------------------------------------------
    if (local.TotalNumbOptOutPhones.Count == 0)
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error - Blacklist file (SR.SR01809.OPTOUT) is empty.   File must be populated using Contact Wireless Blacklist file.";
        
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
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
              "Number of Phone #s Read from Blacklist File.........." + NumberToString
              (local.TotalNumbOptOutPhones.Count, 9, 7);

            break;
          case 3:
            local.EabReportSend.RptDetail =
              "Number of CSE Persons Opted Out of Text Messaging...." + NumberToString
              (local.TotalNumbOptedOut.Count, 9, 7);

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

          goto Test2;
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

      // -------------------------------------------------------------------------------------
      // --  Clear out the data in the Input File.
      // -------------------------------------------------------------------------------------
      local.EabFileHandling.Action = "RESET";
      UseFnB798ReadTextOptOutFile2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error reseting input file.  Return status = " + local
          .EabFileHandling.Status;
        UseCabErrorReport2();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      }
    }

Test2:

    // -------------------------------------------------------------------------------------
    // --  Close the Input File.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseFnB798ReadTextOptOutFile2();

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

  private static void MoveInfrastructure1(Infrastructure source,
    Infrastructure target)
  {
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private static void MoveInfrastructure2(Infrastructure source,
    Infrastructure target)
  {
    target.EventId = source.EventId;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.UserId = source.UserId;
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

  private void UseFnB798ReadTextOptOutFile1()
  {
    var useImport = new FnB798ReadTextOptOutFile.Import();
    var useExport = new FnB798ReadTextOptOutFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;
    useExport.FileRecord.Text12 = local.FileRecord.Text12;

    Call(FnB798ReadTextOptOutFile.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    local.FileRecord.Text12 = useExport.FileRecord.Text12;
  }

  private void UseFnB798ReadTextOptOutFile2()
  {
    var useImport = new FnB798ReadTextOptOutFile.Import();
    var useExport = new FnB798ReadTextOptOutFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(FnB798ReadTextOptOutFile.Execute, useImport, useExport);

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

  private void UseSiAddrRaiseEvent()
  {
    var useImport = new SiAddrRaiseEvent.Import();
    var useExport = new SiAddrRaiseEvent.Export();

    useImport.CsePerson.Number = entities.CsePerson.Number;
    useImport.AparSelection.Text1 = local.Apar.Text1;
    MoveInfrastructure2(local.Infrastructure, useImport.Infrastructure);

    Call(SiAddrRaiseEvent.Execute, useImport, useExport);

    MoveInfrastructure1(useExport.Infrastructure, local.Infrastructure);
  }

  private void UseUpdateCheckpointRstAndCommit()
  {
    var useImport = new UpdateCheckpointRstAndCommit.Import();
    var useExport = new UpdateCheckpointRstAndCommit.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdateCheckpointRstAndCommit.Execute, useImport, useExport);
  }

  private bool ReadCsePerson1()
  {
    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "otherAreaCode",
          local.OptOut.OtherAreaCode.GetValueOrDefault());
        db.SetNullableInt32(
          command, "otherNumber", local.OptOut.OtherNumber.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        local.NumbPersonsUsingPhone.Count = db.GetInt32(reader, 0);
      });
  }

  private IEnumerable<bool> ReadCsePerson2()
  {
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePerson2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "otherAreaCode",
          local.OptOut.OtherAreaCode.GetValueOrDefault());
        db.SetNullableInt32(
          command, "otherNumber", local.OptOut.OtherNumber.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OtherNumber = db.GetNullableInt32(reader, 2);
        entities.CsePerson.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 3);
        entities.CsePerson.LastUpdatedBy = db.GetNullableString(reader, 4);
        entities.CsePerson.OtherAreaCode = db.GetNullableInt32(reader, 5);
        entities.CsePerson.OtherPhoneType = db.GetNullableString(reader, 6);
        entities.CsePerson.TextMessageIndicator =
          db.GetNullableString(reader, 7);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private void UpdateCsePerson()
  {
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;
    var textMessageIndicator = "N";

    entities.CsePerson.Populated = false;
    Update("UpdateCsePerson",
      (db, command) =>
      {
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableString(command, "textMessageInd", textMessageIndicator);
        db.SetString(command, "numb", entities.CsePerson.Number);
      });

    entities.CsePerson.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CsePerson.LastUpdatedBy = lastUpdatedBy;
    entities.CsePerson.TextMessageIndicator = textMessageIndicator;
    entities.CsePerson.Populated = true;
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
    /// A value of NullInfrastructure.
    /// </summary>
    [JsonPropertyName("nullInfrastructure")]
    public Infrastructure NullInfrastructure
    {
      get => nullInfrastructure ??= new();
      set => nullInfrastructure = value;
    }

    /// <summary>
    /// A value of Apar.
    /// </summary>
    [JsonPropertyName("apar")]
    public WorkArea Apar
    {
      get => apar ??= new();
      set => apar = value;
    }

    /// <summary>
    /// A value of NumbPersonsUsingPhone.
    /// </summary>
    [JsonPropertyName("numbPersonsUsingPhone")]
    public Common NumbPersonsUsingPhone
    {
      get => numbPersonsUsingPhone ??= new();
      set => numbPersonsUsingPhone = value;
    }

    /// <summary>
    /// A value of FileRecord.
    /// </summary>
    [JsonPropertyName("fileRecord")]
    public WorkArea FileRecord
    {
      get => fileRecord ??= new();
      set => fileRecord = value;
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
    /// A value of OptOut.
    /// </summary>
    [JsonPropertyName("optOut")]
    public CsePerson OptOut
    {
      get => optOut ??= new();
      set => optOut = value;
    }

    /// <summary>
    /// A value of TotalNumbOptedOut.
    /// </summary>
    [JsonPropertyName("totalNumbOptedOut")]
    public Common TotalNumbOptedOut
    {
      get => totalNumbOptedOut ??= new();
      set => totalNumbOptedOut = value;
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
    /// A value of TotalNumbOptOutPhones.
    /// </summary>
    [JsonPropertyName("totalNumbOptOutPhones")]
    public Common TotalNumbOptOutPhones
    {
      get => totalNumbOptOutPhones ??= new();
      set => totalNumbOptOutPhones = value;
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
    /// A value of NullDateWorkArea.
    /// </summary>
    [JsonPropertyName("nullDateWorkArea")]
    public DateWorkArea NullDateWorkArea
    {
      get => nullDateWorkArea ??= new();
      set => nullDateWorkArea = value;
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

    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    private Infrastructure nullInfrastructure;
    private WorkArea apar;
    private Common numbPersonsUsingPhone;
    private WorkArea fileRecord;
    private CsePerson restart;
    private CsePerson optOut;
    private Common totalNumbOptedOut;
    private Common numbRecordsSinceChckpnt;
    private Common totalNumbOptOutPhones;
    private ProgramCheckpointRestart programCheckpointRestart;
    private DateWorkArea nullDateWorkArea;
    private Common common;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private ExitStateWorkArea exitStateWorkArea;
    private Infrastructure infrastructure;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
#endregion
}
