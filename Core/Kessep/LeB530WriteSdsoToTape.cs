// Program: LE_B530_WRITE_SDSO_TO_TAPE, ID: 372663485, model: 746.
// Short name: SWEL530B
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
/// A program: LE_B530_WRITE_SDSO_TO_TAPE.
/// </para>
/// <para>
/// This skeleton uses:
/// A DB2 table to drive processing
/// An external to do DB2 commits
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class LeB530WriteSdsoToTape: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B530_WRITE_SDSO_TO_TAPE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB530WriteSdsoToTape(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB530WriteSdsoToTape.
  /// </summary>
  public LeB530WriteSdsoToTape(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // --------------------------------------------------------------------------------
    // By		Date		IDCR#/Description
    // Henry Hooks(MTW ?		Initial creation.
    // govind		111397		Fixed to set 3-digit office Identifier; it was
    // 				setting 2-digit office identifier
    // govind		111897		Used Central Receivables Unit for recovery debts
    // govind		120497		Removed the usage of persistent views on Program Run
    // PMcElderry	01/18,23-26/99	Added logic to put SERVICE PROVIDER Id on 
    // record.
    // 				Added CAB ERROR and CAB WRITE logic, removed existing
    // 				logic using Program_Error, Program_Control_Totals,
    // 				and Program_Run.
    // 				Inserted exit state logic, interstate case logic.
    // PMcElderry	04/15/99	Inserted logic to create two seperate records per CSE
    // 				person if CS and Recovery amounts exist.
    // PMcElderry	08/16/99	Logic to close adabase; subagency code for recovery
    // 				certifications changed to 00 from 21
    // PMcElderry	09/24/99	Logic to include cent amount on certifications -
    // 				state formerly wanted rounding down of amounts.
    // GVandy		10/31/2014	CQ45957  Program completely restructured to correct
    // 				multiple errors.  Changes to file layout per DofA.
    // --------------------------------------------------------------------------------
    // --------------------------------------------------------------------------------
    // This procedure step does not require a recursive flow since the only 
    // errors that
    // are possible are file I/O errors that need the program to abort.
    // --------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.HardcodeOpen.FileInstruction = "OPEN";
    local.HardcodeExtend.FileInstruction = "EXTEND";
    local.HardcodeClose.FileInstruction = "CLOSE";
    local.HardcodeWrite.FileInstruction = "WRITE";
    local.HardcodeCommit.FileInstruction = "COMMIT";

    // --------------------------------------------------------------------------------
    // Get run parameters for program
    // --------------------------------------------------------------------------------
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // --------------------------------------------------------------------------------
    // Get DB2 commit frequency counts
    // --------------------------------------------------------------------------------
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // --------------------------------------------------------------------------------
    // Open ERROR REPORT
    // --------------------------------------------------------------------------------
    local.EabReportSend.ProgramName = global.UserId;
    local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    local.EabFileHandling.Action = "OPEN";
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_OPENING_ERROR_RPT";

      return;
    }

    // --------------------------------------------------------------------------------
    // Open the CONTROL REPORT
    // --------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error opening the control report.  Return status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport1();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // --------------------------------------------------------------------------------
    // Determine if this is a RESTART.
    // --------------------------------------------------------------------------------
    if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      // -- Determine restart person number.
      local.Restart.Number =
        TrimEnd(local.ProgramCheckpointRestart.RestartInfo);

      // --------------------------------------------------------------------------------
      // If this is a restart, then the external output file must be appended 
      // to.
      // Hence, we must set the pass_area external file_instruction to EXTEND'.
      // ---------------------------------------------------------------------------------
      local.PassArea.FileInstruction = local.HardcodeExtend.FileInstruction;
      UseExtWriteSdsoToTape1();

      if (!IsEmpty(local.PassArea.TextReturnCode))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error EXTENDing the certification file.  Return status = " + local
          .PassArea.TextReturnCode;
        UseCabErrorReport1();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      // --------------------------------------------------------------------------------
      // Log restart info to control report
      // --------------------------------------------------------------------------------
      for(local.Common.Count = 1; local.Common.Count <= 3; ++local.Common.Count)
      {
        if (local.Common.Count == 1)
        {
          local.EabReportSend.RptDetail = "Restarting at person number " + local
            .Restart.Number;
        }
        else
        {
          local.EabReportSend.RptDetail = "";
        }

        local.EabFileHandling.Action = "WRITE";
        UseCabControlReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error writing restart information to the control report.  Return status = " +
            local.EabFileHandling.Status;
          UseCabErrorReport1();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }
    }
    else
    {
      local.Restart.Number = "";

      // --------------------------------------------------------------------------------
      // Call external to open the output file
      // --------------------------------------------------------------------------------
      local.PassArea.FileInstruction = local.HardcodeOpen.FileInstruction;
      UseExtWriteSdsoToTape1();

      if (!IsEmpty(local.PassArea.TextReturnCode))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error OPENing the certification file.  Return status = " + local
          .PassArea.TextReturnCode;
        UseCabErrorReport1();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    local.ProgramCheckpointRestart.CheckpointCount = 0;

    // --------------------------------------------------------------------------------
    // Process the selected records in groups based upon the commit frequencies.
    // Do a DB2 commit at the end of each group.
    // --------------------------------------------------------------------------------
    local.NumberOfRecordsRead.Count = 0;
    local.TotalRecordsWritten.Count = 0;
    local.NullDate.DateSent = null;

    foreach(var item in ReadOffice())
    {
      if (Find(entities.CseReceivablesUnit.Name, "CENTRAL") > 0)
      {
        local.CseReceivables.SystemGeneratedId =
          entities.CseReceivablesUnit.SystemGeneratedId;

        break;
      }

      if (Find(entities.CseReceivablesUnit.Name, "RECEIV") > 0)
      {
        local.CseReceivables.SystemGeneratedId =
          entities.CseReceivablesUnit.SystemGeneratedId;

        break;
      }

      if (Find(entities.CseReceivablesUnit.Name, "CRU") > 0)
      {
        local.CseReceivables.SystemGeneratedId =
          entities.CseReceivablesUnit.SystemGeneratedId;

        break;
      }

      local.CseReceivables.SystemGeneratedId =
        entities.CseReceivablesUnit.SystemGeneratedId;
    }

    // -- Read each obligor on the system.
    foreach(var item in ReadCsePersonObligor())
    {
      // ------------
      // reinitialize
      // ------------
      local.ExitStateWorkArea.Message = "";
      local.TempEabReportSend.RptDetail = "";
      local.CentralDebtOfficeNumber.Flag = "";

      // -- Determine if this obligor was certified for SDSO today.
      if (ReadAdministrativeActCertification())
      {
        // -- continue processing
      }
      else
      {
        continue;
      }

      // -- Skip this NCP if the child support and recovery amounts are zero.
      if (entities.AdministrativeActCertification.ChildSupportRelatedAmount == 0
        && Equal(entities.AdministrativeActCertification.RecoveryAmount, 0))
      {
        continue;
      }

      ++local.NumberOfRecordsRead.Count;
      ++local.TotalNoOfRecsProcessed.Count;

      // -- Get person name and SSN.
      local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
      UseSiReadCsePersonBatch();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.TempEabReportSend.RptDetail = UseEabExtractExitStateMessage();
        local.EabReportSend.RptDetail = "Error reading cse person  " + entities
          .CsePerson.Number + ".  " + local.TempEabReportSend.RptDetail;
        UseCabErrorReport1();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      // -- Determine office and service provider.
      UseLeCabGetOspForSdso();

      // -- No error checking is required since the cab does not set any error 
      // exit states.
      if (local.OspOffice.SystemGeneratedId == 0)
      {
        // --------------------------------------------------------------
        // This would set the office to Receivables when osp could not
        // be determined, even for child support related debts.
        // --------------------------------------------------------------
        local.OspOffice.SystemGeneratedId =
          local.CseReceivables.SystemGeneratedId;
      }

      if (local.OspServiceProvider.SystemGeneratedId == 0)
      {
        local.SdsoCertificationTapeRecord.Filler12 = "";
      }
      else
      {
        local.SdsoCertificationTapeRecord.Filler12 =
          NumberToString(local.OspServiceProvider.SystemGeneratedId, 12);
      }

      local.SdsoCertificationTapeRecord.CaNumber = "62900001000";
      local.SdsoCertificationTapeRecord.DriversLicenseNumber = "";

      if (AsChar(entities.CsePerson.Type1) == 'C')
      {
        local.SdsoCertificationTapeRecord.DebtorIdCode = "2";
      }
      else
      {
        local.SdsoCertificationTapeRecord.DebtorIdCode = "1";
      }

      local.SdsoCertificationTapeRecord.Ssn = local.CsePersonsWorkSet.Ssn;
      local.SdsoCertificationTapeRecord.FirstName =
        local.CsePersonsWorkSet.FirstName;
      local.SdsoCertificationTapeRecord.MiddleInitial =
        local.CsePersonsWorkSet.MiddleInitial;
      local.SdsoCertificationTapeRecord.LastName =
        local.CsePersonsWorkSet.LastName;
      local.SdsoCertificationTapeRecord.AccountIdCode = "1";

      if (AsChar(local.CentralDebtOfficeNumber.Flag) == 'Y')
      {
        // -----------------------------------------------------------------------
        // 6/30 change - DOA now wants a filler space at the end of this record
        // ------------------------------------------------------------------------
        local.SdsoCertificationTapeRecord.AccountIdNumber = "024" + local
          .CsePersonsWorkSet.Number + " ";
        local.SdsoCertificationTapeRecord.Filler12 = "";
      }
      else
      {
        // -----------------------------------------------------------------------
        // 6/30 change - DOA now wants a filler space at the end of this record
        // ------------------------------------------------------------------------
        local.SdsoCertificationTapeRecord.AccountIdNumber =
          NumberToString(local.OspOffice.SystemGeneratedId, 13, 3) + local
          .CsePersonsWorkSet.Number + " ";
      }

      // --------------------------------------------------------------------------------
      // Two records must be sent to DOA if the member has both CS and Recovery 
      // amounts.
      // --------------------------------------------------------------------------------
      if (entities.AdministrativeActCertification.ChildSupportRelatedAmount > 0)
      {
        // --------------------------------------------------------------------------------
        // Write Child Support certification info to file.
        // --------------------------------------------------------------------------------
        ++local.TotalRecordsWritten.Count;
        local.TotalRecordsWritten.TotalCurrency += entities.
          AdministrativeActCertification.ChildSupportRelatedAmount;
        local.SdsoCertificationTapeRecord.DebtCodeType = "C";
        local.SdsoCertificationTapeRecord.DebtDesc = "CHILD SUPPORT";

        // -- The State Debt Setoff original amount is multiplied by 100 so that
        // the decimal
        //    amount can also be passed using the textnum function.  Otherwise, 
        // the decimal is
        //    truncated.
        local.SdsoCertificationTapeRecord.DebtAmount =
          NumberToString((long)(entities.AdministrativeActCertification.
            ChildSupportRelatedAmount * 100), 13);
        local.SdsoCertificationTapeRecord.CertificationRecord =
          local.SdsoCertificationTapeRecord.CaNumber + local
          .SdsoCertificationTapeRecord.DebtorIdCode + local
          .SdsoCertificationTapeRecord.Ssn + local
          .SdsoCertificationTapeRecord.FirstName + local
          .SdsoCertificationTapeRecord.MiddleInitial + local
          .SdsoCertificationTapeRecord.LastName + local
          .SdsoCertificationTapeRecord.AccountIdCode + local
          .SdsoCertificationTapeRecord.AccountIdNumber + local
          .SdsoCertificationTapeRecord.DebtCodeType + local
          .SdsoCertificationTapeRecord.DebtDesc + local
          .SdsoCertificationTapeRecord.DebtAmount + local
          .SdsoCertificationTapeRecord.Filler12 + local
          .SdsoCertificationTapeRecord.DriversLicenseNumber;
        local.PassArea.FileInstruction = local.HardcodeWrite.FileInstruction;
        UseExtWriteSdsoToTape2();

        if (!IsEmpty(local.PassArea.TextReturnCode))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error writing Child Support to the certification file.  Return status = " +
            local.PassArea.TextReturnCode;
          UseCabErrorReport1();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }

      if (Lt(0, entities.AdministrativeActCertification.RecoveryAmount))
      {
        // --------------------------------------------------------------------------------
        // Write Child Support Recovery certification info to file.
        // --------------------------------------------------------------------------------
        ++local.TotalRecordsWritten.Count;
        local.TotalRecordsWritten.TotalCurrency += entities.
          AdministrativeActCertification.RecoveryAmount.GetValueOrDefault();
        local.SdsoCertificationTapeRecord.DebtCodeType = "R";
        local.SdsoCertificationTapeRecord.DebtDesc = "CHILD SUPPORT RECOVERY";

        // -- The State Debt Setoff original amount is multiplied by 100 so that
        // the decimal
        //    amount can also be passed using the textnum function.  Otherwise, 
        // the decimal is
        //    truncated.
        local.SdsoCertificationTapeRecord.DebtAmount =
          NumberToString((long)(entities.AdministrativeActCertification.
            RecoveryAmount.GetValueOrDefault() * 100), 13);
        local.SdsoCertificationTapeRecord.CertificationRecord =
          local.SdsoCertificationTapeRecord.CaNumber + local
          .SdsoCertificationTapeRecord.DebtorIdCode + local
          .SdsoCertificationTapeRecord.Ssn + local
          .SdsoCertificationTapeRecord.FirstName + local
          .SdsoCertificationTapeRecord.MiddleInitial + local
          .SdsoCertificationTapeRecord.LastName + local
          .SdsoCertificationTapeRecord.AccountIdCode + local
          .SdsoCertificationTapeRecord.AccountIdNumber + local
          .SdsoCertificationTapeRecord.DebtCodeType + local
          .SdsoCertificationTapeRecord.DebtDesc + local
          .SdsoCertificationTapeRecord.DebtAmount + local
          .SdsoCertificationTapeRecord.Filler12 + local
          .SdsoCertificationTapeRecord.DriversLicenseNumber;
        local.PassArea.FileInstruction = local.HardcodeWrite.FileInstruction;
        UseExtWriteSdsoToTape2();

        if (!IsEmpty(local.PassArea.TextReturnCode))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error writing Recovery to the certification file.  Return status = " +
            local.PassArea.TextReturnCode;
          UseCabErrorReport1();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }

      // -- Mark the certification record as processed.
      try
      {
        UpdateAdministrativeActCertification();

        // -- continue processing
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Administrative_Action_Certification Not Unique.  CSE Person " + entities
              .CsePerson.Number;
            UseCabErrorReport1();

            // -- Set Abort exit state and escape...
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          case ErrorCode.PermittedValueViolation:
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Administrative_Action_Certification Permitted Value Violation.  CSE Person " +
              entities.CsePerson.Number;
            UseCabErrorReport1();

            // -- Set Abort exit state and escape...
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      if (local.NumberOfRecordsRead.Count >= local
        .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault())
      {
        // --------------------------------------------------------------
        // Record the number of checkpoints and the last checkpoint
        // time and set the restart indicator to yes.
        // Also return the checkpoint frequency counts in case they
        // been changed since the last read.
        // --------------------------------------------------------------
        local.ProgramCheckpointRestart.RestartInfo = entities.CsePerson.Number;
        local.ProgramCheckpointRestart.LastCheckpointTimestamp = Now();
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.NumberOfRecordsRead.Count = 0;
        UseUpdatePgmCheckpointRestart2();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.TempEabReportSend.RptDetail = UseEabExtractExitStateMessage();
          local.EabReportSend.RptDetail =
            "Error updating program checkpoint record.  " + local
            .TempEabReportSend.RptDetail;
          UseCabErrorReport1();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        // --------------------------------------------------------------
        // Call an external that does a DB2 commit using a Cobol program.
        // --------------------------------------------------------------
        UseExtToDoACommit();

        if (local.PassArea.NumericReturnCode != 0)
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error taking checkpoint.  Return Code = " + NumberToString
            (local.PassArea.NumericReturnCode, 15);
          UseCabErrorReport1();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        // --------------------------------------------------------------------------------
        // Write all external output file records to permanent output file.
        // --------------------------------------------------------------------------------
        local.PassArea.FileInstruction = local.HardcodeCommit.FileInstruction;
        UseExtWriteSdsoToTape1();

        if (!IsEmpty(local.PassArea.TextReturnCode))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error COMMITing the certification file.  Return status = " + local
            .PassArea.TextReturnCode;
          UseCabErrorReport1();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }
    }

    // --------------------------------------------------------------------------------
    // WRITE control report totals
    // --------------------------------------------------------------------------------
    for(local.Common.Count = 1; local.Common.Count <= 5; ++local.Common.Count)
    {
      local.WorkArea.Text16 = "";
      local.WorkArea.Text20 = "";
      local.I.Count = 0;

      switch(local.Common.Count)
      {
        case 1:
          local.TempCommon.TotalCurrency = local.TotalRecordsWritten.Count;
          UseSiCabAmount2Text();

          for(local.J.Count = Length(TrimEnd(local.WorkArea.Text16)); local
            .J.Count >= 1; local.J.Count += -1)
          {
            ++local.I.Count;

            switch(local.I.Count)
            {
              case 7:
                local.WorkArea.Text20 = "," + local.WorkArea.Text20;

                break;
              case 10:
                local.WorkArea.Text20 = "," + local.WorkArea.Text20;

                break;
              case 13:
                local.WorkArea.Text20 = "," + local.WorkArea.Text20;

                break;
              case 16:
                local.WorkArea.Text20 = "," + local.WorkArea.Text20;

                break;
              default:
                break;
            }

            if (local.I.Count > 3)
            {
              local.WorkArea.Text20 =
                Substring(local.WorkArea.Text16, WorkArea.Text16_MaxLength,
                local.J.Count, 1) + local.WorkArea.Text20;
            }
          }

          local.EabReportSend.RptDetail =
            "                    TOTAL NUMBER OF ACCOUNTS  " + local
            .WorkArea.Text20;

          break;
        case 2:
          local.TempCommon.TotalCurrency =
            local.TotalRecordsWritten.TotalCurrency;
          UseSiCabAmount2Text();

          for(local.J.Count = Length(TrimEnd(local.WorkArea.Text16)); local
            .J.Count >= 1; local.J.Count += -1)
          {
            ++local.I.Count;

            switch(local.I.Count)
            {
              case 7:
                local.WorkArea.Text20 = "," + local.WorkArea.Text20;

                break;
              case 10:
                local.WorkArea.Text20 = "," + local.WorkArea.Text20;

                break;
              case 13:
                local.WorkArea.Text20 = "," + local.WorkArea.Text20;

                break;
              case 16:
                local.WorkArea.Text20 = "," + local.WorkArea.Text20;

                break;
              default:
                break;
            }

            local.WorkArea.Text20 =
              Substring(local.WorkArea.Text16, WorkArea.Text16_MaxLength,
              local.J.Count, 1) + local.WorkArea.Text20;
          }

          local.EabReportSend.RptDetail =
            "                    TOTAL DOLLAR AMOUNT      $" + local
            .WorkArea.Text20;

          break;
        default:
          local.EabReportSend.RptDetail = "";

          break;
      }

      local.EabFileHandling.Action = "WRITE";
      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error writing control report totals.  Return status = " + local
          .EabFileHandling.Status;
        UseCabErrorReport1();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    // --------------------------------------------------------------------------------
    // ABEND if no records were processed.
    // --------------------------------------------------------------------------------
    if (local.TotalNoOfRecsProcessed.Count == 0)
    {
      for(local.Common.Count = 1; local.Common.Count <= 3; ++local.Common.Count)
      {
        if (local.Common.Count == 1)
        {
          local.EabReportSend.RptDetail =
            "Error.  No records found to process.";
        }
        else
        {
          local.EabReportSend.RptDetail = "";
        }

        local.EabFileHandling.Action = "WRITE";
        UseCabControlReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error writing control report totals.  Return status = " + local
            .EabFileHandling.Status;
          UseCabErrorReport1();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -----------------------------------------------------
    // Finish restart logic - successfully finished program
    // ------------------------------------------------------
    local.ProgramCheckpointRestart.RestartInd = "N";
    local.ProgramCheckpointRestart.RestartInfo = "";
    UseUpdatePgmCheckpointRestart1();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.TempEabReportSend.RptDetail = UseEabExtractExitStateMessage();
      local.EabReportSend.RptDetail =
        "(2)Error updating program checkpoint record.  " + local
        .TempEabReportSend.RptDetail;
      UseCabErrorReport1();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // --------------------------------------------------------------------------------
    // Write all remaining external output file records to permanent output 
    // file.
    // --------------------------------------------------------------------------------
    local.PassArea.FileInstruction = local.HardcodeCommit.FileInstruction;
    UseExtWriteSdsoToTape1();

    if (!IsEmpty(local.PassArea.TextReturnCode))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "(2)Error COMMITing the certification file.  Return status = " + local
        .PassArea.TextReturnCode;
      UseCabErrorReport1();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // --------------------------------------------------------------------------------
    // Call external to close the output file
    // --------------------------------------------------------------------------------
    local.PassArea.FileInstruction = local.HardcodeClose.FileInstruction;
    UseExtWriteSdsoToTape1();

    if (!IsEmpty(local.PassArea.TextReturnCode))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error closing the certification file.  Return status = " + local
        .PassArea.TextReturnCode;
      UseCabErrorReport1();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // --------------------------------------------------------------------------------
    // CLOSE control report
    // --------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error closing the control report.  Return status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport1();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // --------------------------------------------------------------------------------
    // CLOSE error report
    // --------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_CLOSING_EXT_FILE";

      return;
    }

    // --------------------------------------------------------------------------------
    // close adabase files
    // --------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    UseSiCloseAdabas();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
    }
    else
    {
      // -- this is a soft error so processing can continue
    }
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

  private string UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    return useExport.ExitStateWorkArea.Message;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.PassArea.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.PassArea.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseExtWriteSdsoToTape1()
  {
    var useImport = new ExtWriteSdsoToTape.Import();
    var useExport = new ExtWriteSdsoToTape.Export();

    useImport.External.FileInstruction = local.PassArea.FileInstruction;
    MoveExternal(local.PassArea, useExport.External);

    Call(ExtWriteSdsoToTape.Execute, useImport, useExport);

    MoveExternal(useExport.External, local.PassArea);
  }

  private void UseExtWriteSdsoToTape2()
  {
    var useImport = new ExtWriteSdsoToTape.Import();
    var useExport = new ExtWriteSdsoToTape.Export();

    useImport.External.FileInstruction = local.PassArea.FileInstruction;
    useImport.SdsoCertificationTapeRecord.CertificationRecord =
      local.SdsoCertificationTapeRecord.CertificationRecord;
    MoveExternal(local.PassArea, useExport.External);

    Call(ExtWriteSdsoToTape.Execute, useImport, useExport);

    MoveExternal(useExport.External, local.PassArea);
  }

  private void UseLeCabGetOspForSdso()
  {
    var useImport = new LeCabGetOspForSdso.Import();
    var useExport = new LeCabGetOspForSdso.Export();

    useImport.CsePerson.Number = entities.CsePerson.Number;
    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;

    Call(LeCabGetOspForSdso.Execute, useImport, useExport);

    local.CentralDebtOfficeNumber.Flag = useExport.CentralDebtOffice.Flag;
    local.OspOffice.SystemGeneratedId = useExport.Office.SystemGeneratedId;
    local.OspServiceProvider.SystemGeneratedId =
      useExport.ServiceProvider.SystemGeneratedId;
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

  private void UseSiCabAmount2Text()
  {
    var useImport = new SiCabAmount2Text.Import();
    var useExport = new SiCabAmount2Text.Export();

    useImport.Currency.TotalCurrency = local.TempCommon.TotalCurrency;

    Call(SiCabAmount2Text.Execute, useImport, useExport);

    local.WorkArea.Text16 = useExport.Text.Text16;
  }

  private void UseSiCloseAdabas()
  {
    var useImport = new SiCloseAdabas.Import();
    var useExport = new SiCloseAdabas.Export();

    Call(SiCloseAdabas.Execute, useImport, useExport);
  }

  private void UseSiReadCsePersonBatch()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    local.ReadCsePersonAdabas.Type1 = useExport.AbendData.Type1;
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

  private bool ReadAdministrativeActCertification()
  {
    System.Diagnostics.Debug.Assert(entities.Obligor.Populated);
    entities.AdministrativeActCertification.Populated = false;

    return Read("ReadAdministrativeActCertification",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.Obligor.Type1);
        db.SetString(command, "cspNumber", entities.Obligor.CspNumber);
        db.SetDate(
          command, "takenDt",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "dateSent", local.NullDate.DateSent.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.AdministrativeActCertification.CpaType =
          db.GetString(reader, 0);
        entities.AdministrativeActCertification.CspNumber =
          db.GetString(reader, 1);
        entities.AdministrativeActCertification.Type1 = db.GetString(reader, 2);
        entities.AdministrativeActCertification.TakenDate =
          db.GetDate(reader, 3);
        entities.AdministrativeActCertification.CreatedTstamp =
          db.GetDateTime(reader, 4);
        entities.AdministrativeActCertification.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.AdministrativeActCertification.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 6);
        entities.AdministrativeActCertification.RecoveryAmount =
          db.GetNullableDecimal(reader, 7);
        entities.AdministrativeActCertification.ChildSupportRelatedAmount =
          db.GetDecimal(reader, 8);
        entities.AdministrativeActCertification.DateSent =
          db.GetNullableDate(reader, 9);
        entities.AdministrativeActCertification.TanfCode =
          db.GetString(reader, 10);
        entities.AdministrativeActCertification.Populated = true;
        CheckValid<AdministrativeActCertification>("CpaType",
          entities.AdministrativeActCertification.CpaType);
        CheckValid<AdministrativeActCertification>("Type1",
          entities.AdministrativeActCertification.Type1);
      });
  }

  private IEnumerable<bool> ReadCsePersonObligor()
  {
    entities.Obligor.Populated = false;
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePersonObligor",
      (db, command) =>
      {
        db.SetString(command, "numb", local.Restart.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.Obligor.CspNumber = db.GetString(reader, 2);
        entities.Obligor.Type1 = db.GetString(reader, 3);
        entities.Obligor.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
        CheckValid<CsePersonAccount>("Type1", entities.Obligor.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadOffice()
  {
    entities.CseReceivablesUnit.Populated = false;

    return ReadEach("ReadOffice",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CseReceivablesUnit.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.CseReceivablesUnit.TypeCode = db.GetString(reader, 1);
        entities.CseReceivablesUnit.Name = db.GetString(reader, 2);
        entities.CseReceivablesUnit.EffectiveDate = db.GetDate(reader, 3);
        entities.CseReceivablesUnit.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.CseReceivablesUnit.OffOffice = db.GetNullableInt32(reader, 5);
        entities.CseReceivablesUnit.Populated = true;

        return true;
      });
  }

  private void UpdateAdministrativeActCertification()
  {
    System.Diagnostics.Debug.Assert(
      entities.AdministrativeActCertification.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatedTstamp = Now();
    var dateSent = local.ProgramProcessingInfo.ProcessDate;

    entities.AdministrativeActCertification.Populated = false;
    Update("UpdateAdministrativeActCertification",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetNullableDate(command, "dateSent", dateSent);
        db.SetString(
          command, "cpaType", entities.AdministrativeActCertification.CpaType);
        db.SetString(
          command, "cspNumber",
          entities.AdministrativeActCertification.CspNumber);
        db.SetString(
          command, "type", entities.AdministrativeActCertification.Type1);
        db.SetDate(
          command, "takenDt",
          entities.AdministrativeActCertification.TakenDate.
            GetValueOrDefault());
        db.SetString(
          command, "tanfCode",
          entities.AdministrativeActCertification.TanfCode);
      });

    entities.AdministrativeActCertification.LastUpdatedBy = lastUpdatedBy;
    entities.AdministrativeActCertification.LastUpdatedTstamp =
      lastUpdatedTstamp;
    entities.AdministrativeActCertification.DateSent = dateSent;
    entities.AdministrativeActCertification.Populated = true;
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
    /// A value of J.
    /// </summary>
    [JsonPropertyName("j")]
    public Common J
    {
      get => j ??= new();
      set => j = value;
    }

    /// <summary>
    /// A value of I.
    /// </summary>
    [JsonPropertyName("i")]
    public Common I
    {
      get => i ??= new();
      set => i = value;
    }

    /// <summary>
    /// A value of TempCommon.
    /// </summary>
    [JsonPropertyName("tempCommon")]
    public Common TempCommon
    {
      get => tempCommon ??= new();
      set => tempCommon = value;
    }

    /// <summary>
    /// A value of WorkArea.
    /// </summary>
    [JsonPropertyName("workArea")]
    public WorkArea WorkArea
    {
      get => workArea ??= new();
      set => workArea = value;
    }

    /// <summary>
    /// A value of ReturnAnInteger.
    /// </summary>
    [JsonPropertyName("returnAnInteger")]
    public Common ReturnAnInteger
    {
      get => returnAnInteger ??= new();
      set => returnAnInteger = value;
    }

    /// <summary>
    /// A value of CommasRequired.
    /// </summary>
    [JsonPropertyName("commasRequired")]
    public Common CommasRequired
    {
      get => commasRequired ??= new();
      set => commasRequired = value;
    }

    /// <summary>
    /// A value of HardcodeOpen.
    /// </summary>
    [JsonPropertyName("hardcodeOpen")]
    public External HardcodeOpen
    {
      get => hardcodeOpen ??= new();
      set => hardcodeOpen = value;
    }

    /// <summary>
    /// A value of HardcodeExtend.
    /// </summary>
    [JsonPropertyName("hardcodeExtend")]
    public External HardcodeExtend
    {
      get => hardcodeExtend ??= new();
      set => hardcodeExtend = value;
    }

    /// <summary>
    /// A value of HardcodeClose.
    /// </summary>
    [JsonPropertyName("hardcodeClose")]
    public External HardcodeClose
    {
      get => hardcodeClose ??= new();
      set => hardcodeClose = value;
    }

    /// <summary>
    /// A value of HardcodeWrite.
    /// </summary>
    [JsonPropertyName("hardcodeWrite")]
    public External HardcodeWrite
    {
      get => hardcodeWrite ??= new();
      set => hardcodeWrite = value;
    }

    /// <summary>
    /// A value of HardcodeCommit.
    /// </summary>
    [JsonPropertyName("hardcodeCommit")]
    public External HardcodeCommit
    {
      get => hardcodeCommit ??= new();
      set => hardcodeCommit = value;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
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
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public CsePerson Restart
    {
      get => restart ??= new();
      set => restart = value;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    /// <summary>
    /// A value of NumberOfRecordsRead.
    /// </summary>
    [JsonPropertyName("numberOfRecordsRead")]
    public Common NumberOfRecordsRead
    {
      get => numberOfRecordsRead ??= new();
      set => numberOfRecordsRead = value;
    }

    /// <summary>
    /// A value of TotalRecordsWritten.
    /// </summary>
    [JsonPropertyName("totalRecordsWritten")]
    public Common TotalRecordsWritten
    {
      get => totalRecordsWritten ??= new();
      set => totalRecordsWritten = value;
    }

    /// <summary>
    /// A value of NullDate.
    /// </summary>
    [JsonPropertyName("nullDate")]
    public AdministrativeActCertification NullDate
    {
      get => nullDate ??= new();
      set => nullDate = value;
    }

    /// <summary>
    /// A value of CseReceivables.
    /// </summary>
    [JsonPropertyName("cseReceivables")]
    public Office CseReceivables
    {
      get => cseReceivables ??= new();
      set => cseReceivables = value;
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
    /// A value of TempEabReportSend.
    /// </summary>
    [JsonPropertyName("tempEabReportSend")]
    public EabReportSend TempEabReportSend
    {
      get => tempEabReportSend ??= new();
      set => tempEabReportSend = value;
    }

    /// <summary>
    /// A value of CentralDebtOfficeNumber.
    /// </summary>
    [JsonPropertyName("centralDebtOfficeNumber")]
    public Common CentralDebtOfficeNumber
    {
      get => centralDebtOfficeNumber ??= new();
      set => centralDebtOfficeNumber = value;
    }

    /// <summary>
    /// A value of TotalNoOfRecsProcessed.
    /// </summary>
    [JsonPropertyName("totalNoOfRecsProcessed")]
    public Common TotalNoOfRecsProcessed
    {
      get => totalNoOfRecsProcessed ??= new();
      set => totalNoOfRecsProcessed = value;
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
    /// A value of ReadCsePersonAdabas.
    /// </summary>
    [JsonPropertyName("readCsePersonAdabas")]
    public AbendData ReadCsePersonAdabas
    {
      get => readCsePersonAdabas ??= new();
      set => readCsePersonAdabas = value;
    }

    /// <summary>
    /// A value of OspOffice.
    /// </summary>
    [JsonPropertyName("ospOffice")]
    public Office OspOffice
    {
      get => ospOffice ??= new();
      set => ospOffice = value;
    }

    /// <summary>
    /// A value of OspServiceProvider.
    /// </summary>
    [JsonPropertyName("ospServiceProvider")]
    public ServiceProvider OspServiceProvider
    {
      get => ospServiceProvider ??= new();
      set => ospServiceProvider = value;
    }

    /// <summary>
    /// A value of SdsoCertificationTapeRecord.
    /// </summary>
    [JsonPropertyName("sdsoCertificationTapeRecord")]
    public SdsoCertificationTapeRecord SdsoCertificationTapeRecord
    {
      get => sdsoCertificationTapeRecord ??= new();
      set => sdsoCertificationTapeRecord = value;
    }

    private Common j;
    private Common i;
    private Common tempCommon;
    private WorkArea workArea;
    private Common returnAnInteger;
    private Common commasRequired;
    private External hardcodeOpen;
    private External hardcodeExtend;
    private External hardcodeClose;
    private External hardcodeWrite;
    private External hardcodeCommit;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private CsePerson restart;
    private External passArea;
    private Common common;
    private Common numberOfRecordsRead;
    private Common totalRecordsWritten;
    private AdministrativeActCertification nullDate;
    private Office cseReceivables;
    private ExitStateWorkArea exitStateWorkArea;
    private EabReportSend tempEabReportSend;
    private Common centralDebtOfficeNumber;
    private Common totalNoOfRecsProcessed;
    private CsePersonsWorkSet csePersonsWorkSet;
    private AbendData readCsePersonAdabas;
    private Office ospOffice;
    private ServiceProvider ospServiceProvider;
    private SdsoCertificationTapeRecord sdsoCertificationTapeRecord;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CseReceivablesUnit.
    /// </summary>
    [JsonPropertyName("cseReceivablesUnit")]
    public Office CseReceivablesUnit
    {
      get => cseReceivablesUnit ??= new();
      set => cseReceivablesUnit = value;
    }

    /// <summary>
    /// A value of AdministrativeActCertification.
    /// </summary>
    [JsonPropertyName("administrativeActCertification")]
    public AdministrativeActCertification AdministrativeActCertification
    {
      get => administrativeActCertification ??= new();
      set => administrativeActCertification = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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

    private Office cseReceivablesUnit;
    private AdministrativeActCertification administrativeActCertification;
    private CsePersonAccount obligor;
    private CsePerson csePerson;
  }
#endregion
}
