// Program: LE_B520_WRITE_FDSO_TO_TAPE, ID: 372667616, model: 746.
// Short name: SWEL520B
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
/// A program: LE_B520_WRITE_FDSO_TO_TAPE.
/// </para>
/// <para>
/// This skeleton uses:
/// A DB2 table to drive processing
/// An external to do DB2 commits
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class LeB520WriteFdsoToTape: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B520_WRITE_FDSO_TO_TAPE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB520WriteFdsoToTape(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB520WriteFdsoToTape.
  /// </summary>
  public LeB520WriteFdsoToTape(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ----------------------------------------------------------------------------------------------------------------------
    // DATE        DEVELOPER   REQUEST         DESCRIPTION
    // ----------  ----------	----------	
    // ------------------------------------------------------------------------------
    // ??/??/????  ????????	?????		Initial Coding
    // 11/27/2001  EShirk	PR#131415	Modified section of code that builds the 'R'
    // type (replace exemption ind)
    // 					transactions.   We have learned that the fed keeps seperate records 
    // of
    // 					exemption information for TANF and non-TANF.   Thus we need to build
    // 					and send two seperate TANF and non-TANF transaction when pertinent.
    // 03/28/2002  EShirk	PR#141754	Altered code to no longer set the adc or non
    // -adc amount to .01 when no
    // 					change in arrears occurred from the previous certificaiton amounts.
    // Setting
    // 					the amount to zero from this point forward.
    // 04/01/2002  EShirk	PR#134321	Altered process to round the adc/non-adc 
    // amounts prior to
    // 					sending to the tape build process.
    // 10/11/2005  MJQuinn	PR#158589	Only send a DELETE if ADC or Non-ADC  falls
    // to zero and both are now at zero.
    // 05/23/2007  GVandy	PR131994	When SSN changes, send a delete transaction 
    // to the feds for the old
    // 					SSN and an add transaction for the new SSN.
    // 05/23/2007  GVandy	WR 289942	Add support for B (Name Change) and Z (
    // Address Changes) transaction types.
    // 07/25/2007  GVandy	PR313068	Program was  re-structured/re-written to 
    // correctly send transactions
    // 					per case type (i.e. adc verses non-adc).
    // 					Also added lots of formatting, comments, cleanup, and general error 
    // checking.
    // ----------------------------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // -----------------------------------------------------------------------------------------------
    // Retrieve the PPI info.
    // -----------------------------------------------------------------------------------------------
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // -----------------------------------------------------------------------------------------------
    // Open Error Report
    // -----------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    local.NeededToOpen.ProgramName = global.TranCode;
    local.NeededToOpen.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_OPENING_ERROR_RPT";

      return;
    }

    // -----------------------------------------------------------------------------------------------
    // Open Control Report
    // -----------------------------------------------------------------------------------------------
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered opening control report.";
      UseCabErrorReport4();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -----------------------------------------------------------------------------------------------
    // Open Output FDSO Transaction File
    // -----------------------------------------------------------------------------------------------
    UseEabWriteFdsoToTransfile2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered opening Output FDSO Transaction File.  Return Code = " +
        local.EabFileHandling.Status;
      UseCabErrorReport4();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -----------------------------------------------------------------------------------------------
    // Get the DB2 commit frequency counts and determine if we are restarting.
    // -----------------------------------------------------------------------------------------------
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmChkpntRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      // -- The CSP Number is stored on the checkpoint/restart record but it is 
      // not used on restarts.
      //    Instead a read is done for all FDSO records with a null sent date.  
      // This way all records that are ready to be sent to OCSE will be
      // processed.
      // -- Write the restart info to the control report.
      for(local.Common.Count = 1; local.Common.Count <= 2; ++local.Common.Count)
      {
        switch(local.Common.Count)
        {
          case 1:
            local.NeededToWrite.RptDetail = "Process restarting...";

            break;
          case 2:
            local.NeededToWrite.RptDetail = "";

            break;
          default:
            break;
        }

        local.EabFileHandling.Action = "WRITE";
        UseCabControlReport3();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "Error encountered writing Restart Info to control report.";
          UseCabErrorReport4();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }
    }

    // -----------------------------------------------------------------------------------------------
    // Extract parameters from the PPI record.
    // -----------------------------------------------------------------------------------------------
    // -- This program does not use parameters from the PPI record.
    local.UpdatesSinceCheckpoint.Count = 0;

    // -----------------------------------------------------------------------------------------------
    // Process all FDSO records with a null sent date.
    // -----------------------------------------------------------------------------------------------
    foreach(var item in ReadFederalDebtSetoffCsePersonObligor())
    {
      ++local.NumberOfReads.Count;
      local.FdsoCertificationTapeRecord.SubmittingState = "KS";
      local.FdsoCertificationTapeRecord.LocalCode =
        entities.FederalDebtSetoff.LocalCode ?? Spaces(3);
      local.FdsoCertificationTapeRecord.Ssn =
        NumberToString(entities.FederalDebtSetoff.Ssn, 9);
      local.FdsoCertificationTapeRecord.CaseNumber =
        entities.FederalDebtSetoff.CaseNumber;
      local.FdsoCertificationTapeRecord.LastName =
        entities.FederalDebtSetoff.LastName;
      local.FdsoCertificationTapeRecord.FirstName =
        entities.FederalDebtSetoff.FirstName;
      local.FdsoCertificationTapeRecord.TransferState =
        entities.FederalDebtSetoff.TransferState ?? Spaces(2);
      local.FdsoCertificationTapeRecord.LocalForTransfer =
        entities.FederalDebtSetoff.LocalForTransfer.GetValueOrDefault();
      local.FdsoCertificationTapeRecord.ProcessYear =
        NumberToString(entities.FederalDebtSetoff.ProcessYear.
          GetValueOrDefault(), 4);
      local.FdsoCertificationTapeRecord.AddressLine1 =
        entities.FederalDebtSetoff.AddressStreet1 ?? Spaces(30);
      local.FdsoCertificationTapeRecord.AddressLine2 =
        entities.FederalDebtSetoff.AddressStreet2 ?? Spaces(30);
      local.FdsoCertificationTapeRecord.City =
        entities.FederalDebtSetoff.AddressCity ?? Spaces(25);
      local.FdsoCertificationTapeRecord.StateCode =
        entities.FederalDebtSetoff.AddressState ?? Spaces(2);
      local.FdsoCertificationTapeRecord.ZipCode =
        entities.FederalDebtSetoff.AddressZip ?? Spaces(9);

      // -- Round the ADC and Non-ADC amounts prior to writing to file.
      local.Common.TotalInteger =
        (long)Math.Round(
          entities.FederalDebtSetoff.AdcAmount.GetValueOrDefault(),
        MidpointRounding.AwayFromZero);
      local.FdsoCertificationTapeRecord.AdcAmount = local.Common.TotalInteger;
      local.Common.TotalInteger =
        (long)Math.Round(
          entities.FederalDebtSetoff.NonAdcAmount.GetValueOrDefault(),
        MidpointRounding.AwayFromZero);
      local.FdsoCertificationTapeRecord.NonAdcAmount =
        local.Common.TotalInteger;

      // -- Concat all the exclusion types into one attribute
      UseCabFdsoConcatExclusion();

      for(local.Common.Count = 1; local.Common.Count <= 9; ++local.Common.Count)
      {
        // ------------------------------------------------------------------------------------------------
        // The ttype_x_xxxxxxxxxxx attributes in the FDSO record will contain 
        // either a space, A, N, or B.
        // These values indicate...
        // Space - the transaction type is not to be generated
        // A - the transaction type should be generated for ADC
        // N - the transaction type should be generated for Non-ADC
        // B - the transaction type should be generated for Both ADC and Non-ADC
        // This value is passed in the CASE_TYPE_IND attribute to the eab 
        // eab_write_fdso_to_transfile
        // which then uses this value to write the correct transaction(s) to the
        // output file.
        // ------------------------------------------------------------------------------------------------
        local.TransactionTotals.Index = local.Common.Count - 1;
        local.TransactionTotals.CheckSize();

        switch(local.Common.Count)
        {
          case 1:
            // ***************************************************
            // **     A - Add new case
            // ***************************************************
            if (IsEmpty(entities.FederalDebtSetoff.TtypeAAddNewCase))
            {
              continue;
            }
            else
            {
              local.FdsoCertificationTapeRecord.TransactionType = "A";
              local.FdsoCertificationTapeRecord.CaseTypeInd =
                entities.FederalDebtSetoff.TtypeAAddNewCase ?? Spaces(1);
            }

            break;
          case 2:
            // ***************************************************
            // **    B - Change obligor name
            // ***************************************************
            if (IsEmpty(entities.FederalDebtSetoff.TtypeBNameChange))
            {
              continue;
            }
            else
            {
              local.FdsoCertificationTapeRecord.TransactionType = "B";
              local.FdsoCertificationTapeRecord.CaseTypeInd =
                entities.FederalDebtSetoff.TtypeBNameChange ?? Spaces(1);
            }

            break;
          case 3:
            // ******************************************************
            // **     D - Delete case
            // ******************************************************
            if (IsEmpty(entities.FederalDebtSetoff.TtypeDDeleteCertification))
            {
              continue;
            }
            else
            {
              local.FdsoCertificationTapeRecord.TransactionType = "D";
              local.FdsoCertificationTapeRecord.CaseTypeInd =
                entities.FederalDebtSetoff.TtypeDDeleteCertification ?? Spaces
                (1);
            }

            break;
          case 4:
            // ***************************************************
            // **    L - Change submitting state local
            // ***************************************************
            if (IsEmpty(entities.FederalDebtSetoff.TtypeLChangeSubmittingState))
            {
              continue;
            }
            else
            {
              local.FdsoCertificationTapeRecord.TransactionType = "L";
              local.FdsoCertificationTapeRecord.CaseTypeInd =
                entities.FederalDebtSetoff.TtypeLChangeSubmittingState ?? Spaces
                (1);
            }

            break;
          case 5:
            // ******************************************************
            // **    M - Modify decrease or increase amount
            // ******************************************************
            if (IsEmpty(entities.FederalDebtSetoff.TtypeMModifyAmount))
            {
              continue;
            }
            else
            {
              local.FdsoCertificationTapeRecord.TransactionType = "M";
              local.FdsoCertificationTapeRecord.CaseTypeInd =
                entities.FederalDebtSetoff.TtypeMModifyAmount ?? Spaces(1);
            }

            break;
          case 6:
            // ******************************************************
            // **    R - Replace Exclusion Indicators
            // ******************************************************
            if (IsEmpty(entities.FederalDebtSetoff.TtypeRModifyExclusion))
            {
              continue;
            }
            else
            {
              local.FdsoCertificationTapeRecord.TransactionType = "R";
              local.FdsoCertificationTapeRecord.CaseTypeInd =
                entities.FederalDebtSetoff.TtypeRModifyExclusion ?? Spaces(1);
            }

            break;
          case 7:
            // ******************************************************
            // **     T - Transfer for administrative review to state with the 
            // order
            // ******************************************************
            // ------------------------------------------------------------------------
            // Note that SWELB500 does not currently create T transactions.
            // ------------------------------------------------------------------------
            if (IsEmpty(entities.FederalDebtSetoff.TtypeTTransferAdminReview))
            {
              continue;
            }
            else
            {
              local.FdsoCertificationTapeRecord.TransactionType = "T";
              local.FdsoCertificationTapeRecord.CaseTypeInd =
                entities.FederalDebtSetoff.TtypeTTransferAdminReview ?? Spaces
                (1);
            }

            break;
          case 8:
            // ******************************************************
            // **    Z - Change obligor address
            // ******************************************************
            if (IsEmpty(entities.FederalDebtSetoff.TtypeZAddressChange))
            {
              continue;
            }
            else
            {
              local.FdsoCertificationTapeRecord.TransactionType = "Z";
              local.FdsoCertificationTapeRecord.CaseTypeInd =
                entities.FederalDebtSetoff.TtypeZAddressChange ?? Spaces(1);
            }

            break;
          case 9:
            // ******************************************************
            // **    Process Delete transaction due to SSN Change
            // ******************************************************
            // ------------------------------------------------------------------------
            // 05/23/2007  PR131994  GVandy   When SSN changes, send a delete 
            // transaction
            // to the feds for the old SSN and an add transaction for the new 
            // SSN.
            // ------------------------------------------------------------------------
            // ------------------------------------------------------------------------
            // The change SSN transaction needs to be the last transaction 
            // generated
            // because we overwrite the attributes in the 
            // fdso_certification_tape_record
            // with attributes from the prior certification record.
            // ------------------------------------------------------------------------
            if (IsEmpty(entities.FederalDebtSetoff.ChangeSsnInd))
            {
              continue;
            }
            else
            {
              local.FdsoCertificationTapeRecord.TransactionType = "D";

              // -- Find previous FDSO certification for this obligor and set 
              // the attributes for the external.
              if (ReadFederalDebtSetoff())
              {
                local.FdsoCertificationTapeRecord.CaseNumber =
                  entities.PreviousSsn.CaseNumber;
                local.FdsoCertificationTapeRecord.Ssn =
                  NumberToString(entities.PreviousSsn.Ssn, 9);
                local.FdsoCertificationTapeRecord.LastName =
                  entities.PreviousSsn.LastName;

                if (Lt(0, entities.PreviousSsn.AdcAmount) && Lt
                  (0, entities.PreviousSsn.NonAdcAmount))
                {
                  local.FdsoCertificationTapeRecord.CaseTypeInd = "B";
                }
                else if (Lt(0, entities.PreviousSsn.AdcAmount))
                {
                  local.FdsoCertificationTapeRecord.CaseTypeInd = "A";
                }
                else if (Lt(0, entities.PreviousSsn.NonAdcAmount))
                {
                  local.FdsoCertificationTapeRecord.CaseTypeInd = "N";
                }
                else if (Equal(entities.PreviousSsn.AdcAmount, 0) && Equal
                  (entities.PreviousSsn.NonAdcAmount, 0))
                {
                  // -- Previous record was a decertification.  No need to send 
                  // another decertification record.
                  continue;
                }

                // -- Setting the following attributes just for consistency with
                // how we create the other transactions.
                //    According to the fed documentation these attributes will 
                // be ignored when the transaction is processed.
                local.FdsoCertificationTapeRecord.FirstName =
                  entities.PreviousSsn.FirstName;
                local.FdsoCertificationTapeRecord.LocalCode =
                  entities.PreviousSsn.LocalCode ?? Spaces(3);
                local.FdsoCertificationTapeRecord.ProcessYear =
                  NumberToString(entities.PreviousSsn.ProcessYear.
                    GetValueOrDefault(), 4);
                local.FdsoCertificationTapeRecord.AddressLine1 =
                  entities.PreviousSsn.AddressStreet1 ?? Spaces(30);
                local.FdsoCertificationTapeRecord.AddressLine2 =
                  entities.PreviousSsn.AddressStreet2 ?? Spaces(30);
                local.FdsoCertificationTapeRecord.City =
                  entities.PreviousSsn.AddressCity ?? Spaces(25);
                local.FdsoCertificationTapeRecord.StateCode =
                  entities.PreviousSsn.AddressState ?? Spaces(2);
                local.FdsoCertificationTapeRecord.ZipCode =
                  entities.PreviousSsn.AddressZip ?? Spaces(9);
              }

              if (!entities.PreviousSsn.Populated)
              {
                // -- Didn't find a previous certification.
                local.EabFileHandling.Action = "WRITE";
                local.NeededToWrite.RptDetail =
                  "Error reading previous FDSO certification for SSN change.  CSE Person Number : " +
                  local.FdsoCertificationTapeRecord.CaseNumber;
                UseCabErrorReport4();
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }
            }

            break;
          default:
            break;
        }

        // -- Increment transaction counts and arrears amounts owed by 
        // transaction type/case type.
        if (local.Common.Count == 9)
        {
          // -- Add number of cases deleted due to SSN changes to the number of 
          // cases deleted for other reasons.
          local.TransactionTotals.Index = 2;
          local.TransactionTotals.CheckSize();
        }
        else
        {
          local.TransactionTotals.Index = local.Common.Count - 1;
          local.TransactionTotals.CheckSize();
        }

        if (AsChar(local.FdsoCertificationTapeRecord.CaseTypeInd) == 'A' || AsChar
          (local.FdsoCertificationTapeRecord.CaseTypeInd) == 'B')
        {
          local.TransactionTotals.Update.GrLocalAdcTransactionTotal.Count =
            local.TransactionTotals.Item.GrLocalAdcTransactionTotal.Count + 1;

          if (AsChar(local.FdsoCertificationTapeRecord.TransactionType) == 'A'
            || AsChar(local.FdsoCertificationTapeRecord.TransactionType) == 'M'
            )
          {
            local.TransactionTotals.Update.GrLocalAdcTransactionTotal.
              TotalCurrency =
                local.TransactionTotals.Item.GrLocalAdcTransactionTotal.
                TotalCurrency + local.FdsoCertificationTapeRecord.AdcAmount;
          }
        }

        if (AsChar(local.FdsoCertificationTapeRecord.CaseTypeInd) == 'N' || AsChar
          (local.FdsoCertificationTapeRecord.CaseTypeInd) == 'B')
        {
          local.TransactionTotals.Update.GrLocalNonAdcTransactionTot.Count =
            local.TransactionTotals.Item.GrLocalNonAdcTransactionTot.Count + 1;

          if (AsChar(local.FdsoCertificationTapeRecord.TransactionType) == 'A'
            || AsChar(local.FdsoCertificationTapeRecord.TransactionType) == 'M'
            )
          {
            local.TransactionTotals.Update.GrLocalNonAdcTransactionTot.
              TotalCurrency =
                local.TransactionTotals.Item.GrLocalNonAdcTransactionTot.
                TotalCurrency + local.FdsoCertificationTapeRecord.NonAdcAmount;
          }
        }

        // -- Write transaction to the FDSO transaction file.
        local.EabFileHandling.Action = "WRITE";
        UseEabWriteFdsoToTransfile1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "Error occured writing FDSO transaction record to file, CSE Person Number : " +
            entities.FederalDebtSetoff.CaseNumber + "  Return Code = " + local
            .EabFileHandling.Status;
          UseCabErrorReport2();
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }

      // -- Update federal_debt_setoff date_sent to indicate that the record has
      // been sent to the feds.
      try
      {
        UpdateFederalDebtSetoff();
        ++local.UpdatesSinceCheckpoint.Count;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail =
              "Update for federal_debt_setoff is 'not unique', CSE Person Number :" +
              entities.FederalDebtSetoff.CaseNumber;
            UseCabErrorReport2();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          case ErrorCode.PermittedValueViolation:
            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      // --  Check for commit point.
      if (local.UpdatesSinceCheckpoint.Count >= local
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        // --  Update program checkpoint restart with current commit position.
        local.ProgramCheckpointRestart.RestartInd = "Y";

        // -- The CSE person number is being stored even though it is not used 
        // on a restart.
        //    In the event of an abend this will provide some useful information
        // as to how far along the process was when it abended.
        local.ProgramCheckpointRestart.RestartInfo = entities.CsePerson.Number;
        UseUpdatePgmCheckpointRestart();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "Update of checkpoint restart failed at person number -   " + entities
            .CsePerson.Number;
          UseCabErrorReport2();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        UseExtToDoACommit();

        if (local.External.NumericReturnCode != 0)
        {
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "Commit Failed at person number -   " + entities.CsePerson.Number;
          UseCabErrorReport4();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        local.UpdatesSinceCheckpoint.Count = 0;
      }
    }

    // -----------------------------------------------------------------------------------------------
    // Write totals to the control report.
    // -----------------------------------------------------------------------------------------------
    // -- The following deliberately starts at -2 so that 3 header lines are 
    // written before indexing into the local group to extract the totals.
    for(local.Common.Count = -2; local.Common.Count <= 16; ++local.Common.Count)
    {
      switch(local.Common.Count)
      {
        case -2:
          local.NeededToWrite.RptDetail =
            "                                                                      A D C                       N o n - A D C";
            

          break;
        case -1:
          local.NeededToWrite.RptDetail =
            "                                                               # Cases     $ Amount           # Cases     $ Amount";
            

          break;
        case 0:
          local.NeededToWrite.RptDetail =
            "                                                               -------    ----------          -------    ----------";
            

          break;
        case 1:
          local.NeededToWrite.RptDetail = "Transaction Type \"A\" (Add Case)";

          break;
        case 2:
          local.NeededToWrite.RptDetail =
            "Transaction Type \"B\" (Name Change)";

          break;
        case 3:
          local.NeededToWrite.RptDetail =
            "Transaction Type \"D\" (Delete Case)";

          break;
        case 4:
          local.NeededToWrite.RptDetail =
            "Transaction Type \"L\" (Local Code Change)";

          break;
        case 5:
          local.NeededToWrite.RptDetail =
            "Transaction Type \"M\" (Modify Arrearage Amount)";

          break;
        case 6:
          local.NeededToWrite.RptDetail =
            "Transaction Type \"R\" (Replace Exclusion Indicators)";

          break;
        case 7:
          local.NeededToWrite.RptDetail =
            "Transaction Type \"T\" (Transfer for Administrative Review)";

          break;
        case 8:
          local.NeededToWrite.RptDetail =
            "Transaction Type \"Z\" (Address Change)";

          break;
        case 9:
          local.NeededToWrite.RptDetail =
            "                                                               -------    ----------          -------    ----------";
            

          break;
        case 10:
          local.NeededToWrite.RptDetail = "TOTALS by Case Type";
          local.NeededToWrite.RptDetail =
            Substring(local.NeededToWrite.RptDetail,
            EabReportSend.RptDetail_MaxLength, 1, 63) + NumberToString
            (local.GrandTotals.GrLocalAdcSubtotal.Count, 9, 7) + "    " + NumberToString
            ((long)local.GrandTotals.GrLocalAdcSubtotal.TotalCurrency, 6, 10) +
            "          " + NumberToString
            (local.GrandTotals.GrLocalNonAdcSubtotal.Count, 9, 7) + "    " + NumberToString
            ((long)local.GrandTotals.GrLocalNonAdcSubtotal.TotalCurrency, 6, 10) +
            "  ";

          break;
        case 11:
          local.NeededToWrite.RptDetail = "";

          break;
        case 12:
          local.GrandTotals.GrLocalGrandTotal.Count =
            local.GrandTotals.GrLocalAdcSubtotal.Count + local
            .GrandTotals.GrLocalNonAdcSubtotal.Count;
          local.NeededToWrite.RptDetail =
            "TOTAL Number of FDSO Transactions Written to File : " + NumberToString
            (local.GrandTotals.GrLocalGrandTotal.Count, 6, 10);

          break;
        case 13:
          continue;
        case 14:
          local.NeededToWrite.RptDetail = "";

          break;
        case 15:
          local.NeededToWrite.RptDetail =
            "TOTAL Number of Admin_Act_Cert records processed  : " + NumberToString
            (local.NumberOfReads.Count, 6, 10);

          break;
        case 16:
          local.NeededToWrite.RptDetail = "";

          break;
        default:
          break;
      }

      if (local.Common.Count >= 1 && local.Common.Count <= 8)
      {
        // -- Place the ADC and Non-ADC counts and amounts on the output line.
        local.TransactionTotals.Index = local.Common.Count - 1;
        local.TransactionTotals.CheckSize();

        if (local.Common.Count == 1 || local.Common.Count == 5)
        {
          // -- Only display amounts for Add and Modify transactions.
          local.NeededToWrite.RptDetail =
            Substring(local.NeededToWrite.RptDetail,
            EabReportSend.RptDetail_MaxLength, 1, 63) + NumberToString
            (local.TransactionTotals.Item.GrLocalAdcTransactionTotal.Count, 9, 7)
            + "    " + NumberToString
            ((long)local.TransactionTotals.Item.GrLocalAdcTransactionTotal.
              TotalCurrency, 6, 10) + "          " + NumberToString
            (local.TransactionTotals.Item.GrLocalNonAdcTransactionTot.Count, 9,
            7) + "    " + NumberToString
            ((long)local.TransactionTotals.Item.GrLocalNonAdcTransactionTot.
              TotalCurrency, 6, 10) + "  ";
        }
        else
        {
          local.NeededToWrite.RptDetail =
            Substring(local.NeededToWrite.RptDetail,
            EabReportSend.RptDetail_MaxLength, 1, 63) + NumberToString
            (local.TransactionTotals.Item.GrLocalAdcTransactionTotal.Count, 9, 7)
            + "             -          " + NumberToString
            (local.TransactionTotals.Item.GrLocalNonAdcTransactionTot.Count, 9,
            7) + "             -          ";
        }

        // -- Keep a running subtotal for ADC and Non-ADC counts and amounts.
        local.GrandTotals.GrLocalAdcSubtotal.Count += local.TransactionTotals.
          Item.GrLocalAdcTransactionTotal.Count;
        local.GrandTotals.GrLocalAdcSubtotal.TotalCurrency += local.
          TransactionTotals.Item.GrLocalAdcTransactionTotal.TotalCurrency;
        local.GrandTotals.GrLocalNonAdcSubtotal.Count += local.
          TransactionTotals.Item.GrLocalNonAdcTransactionTot.Count;
        local.GrandTotals.GrLocalNonAdcSubtotal.TotalCurrency += local.
          TransactionTotals.Item.GrLocalNonAdcTransactionTot.TotalCurrency;
      }

      local.EabFileHandling.Action = "WRITE";
      UseCabControlReport3();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail =
          "Error encountered writing Totals to control report.";
        UseCabErrorReport4();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    // -----------------------------------------------------------------------------------------------
    // Take a final checkpoint.
    // -----------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.ProgramCheckpointRestart.RestartInd = "N";
    local.ProgramCheckpointRestart.RestartInfo = "";
    UseUpdatePgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Final update of checkpoint restart table failed.";
      UseCabErrorReport4();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -----------------------------------------------------------------------------------------------
    // Close Output FDSO Transaction File
    // -----------------------------------------------------------------------------------------------
    // -- Note that the EAB writes the footer record to the file as part of the 
    // CLOSE processing.
    local.EabFileHandling.Action = "CLOSE";
    UseEabWriteFdsoToTransfile2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered while closing Output FDSO Transaction File.";
      UseCabErrorReport4();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -----------------------------------------------------------------------------------------------
    // Close Control Report
    // -----------------------------------------------------------------------------------------------
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered while closing control report.";
      UseCabErrorReport4();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -----------------------------------------------------------------------------------------------
    // Close Error Report
    // -----------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private static void MoveAdministrativeActCertification(
    AdministrativeActCertification source,
    AdministrativeActCertification target)
  {
    target.EtypeAdmBankrupt = source.EtypeAdmBankrupt;
    target.EtypeAdministrativeOffset = source.EtypeAdministrativeOffset;
    target.EtypeFederalRetirement = source.EtypeFederalRetirement;
    target.EtypeFederalSalary = source.EtypeFederalSalary;
    target.EtypeTaxRefund = source.EtypeTaxRefund;
    target.EtypeVendorPaymentOrMisc = source.EtypeVendorPaymentOrMisc;
    target.EtypePassportDenial = source.EtypePassportDenial;
    target.EtypeFinancialInstitution = source.EtypeFinancialInstitution;
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
    target.ProgramName = source.ProgramName;
    target.RestartInd = source.RestartInd;
    target.RestartInfo = source.RestartInfo;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport3()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
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

  private void UseCabErrorReport4()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabFdsoConcatExclusion()
  {
    var useImport = new CabFdsoConcatExclusion.Import();
    var useExport = new CabFdsoConcatExclusion.Export();

    MoveAdministrativeActCertification(entities.FederalDebtSetoff,
      useImport.FederalDebtSetoff);

    Call(CabFdsoConcatExclusion.Execute, useImport, useExport);

    local.FdsoCertificationTapeRecord.OffsetExclusionType =
      useExport.FdsoCertificationTapeRecord.OffsetExclusionType;
  }

  private void UseEabWriteFdsoToTransfile1()
  {
    var useImport = new EabWriteFdsoToTransfile.Import();
    var useExport = new EabWriteFdsoToTransfile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.FdsoCertificationTapeRecord.Assign(
      local.FdsoCertificationTapeRecord);
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(EabWriteFdsoToTransfile.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabWriteFdsoToTransfile2()
  {
    var useImport = new EabWriteFdsoToTransfile.Import();
    var useExport = new EabWriteFdsoToTransfile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(EabWriteFdsoToTransfile.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.External.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.External.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseReadPgmChkpntRestart()
  {
    var useImport = new ReadPgmChkpntRestart.Import();
    var useExport = new ReadPgmChkpntRestart.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      local.ProgramCheckpointRestart.ProgramName;

    Call(ReadPgmChkpntRestart.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseUpdatePgmCheckpointRestart()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    MoveProgramCheckpointRestart(local.ProgramCheckpointRestart,
      useImport.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }

  private bool ReadFederalDebtSetoff()
  {
    System.Diagnostics.Debug.Assert(entities.Obligor.Populated);
    entities.PreviousSsn.Populated = false;

    return Read("ReadFederalDebtSetoff",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.Obligor.Type1);
        db.SetString(command, "cspNumber", entities.Obligor.CspNumber);
        db.SetDate(
          command, "takenDt",
          entities.FederalDebtSetoff.TakenDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PreviousSsn.CpaType = db.GetString(reader, 0);
        entities.PreviousSsn.CspNumber = db.GetString(reader, 1);
        entities.PreviousSsn.Type1 = db.GetString(reader, 2);
        entities.PreviousSsn.TakenDate = db.GetDate(reader, 3);
        entities.PreviousSsn.AdcAmount = db.GetNullableDecimal(reader, 4);
        entities.PreviousSsn.NonAdcAmount = db.GetNullableDecimal(reader, 5);
        entities.PreviousSsn.LocalCode = db.GetNullableString(reader, 6);
        entities.PreviousSsn.Ssn = db.GetInt32(reader, 7);
        entities.PreviousSsn.CaseNumber = db.GetString(reader, 8);
        entities.PreviousSsn.LastName = db.GetString(reader, 9);
        entities.PreviousSsn.FirstName = db.GetString(reader, 10);
        entities.PreviousSsn.ProcessYear = db.GetNullableInt32(reader, 11);
        entities.PreviousSsn.TanfCode = db.GetString(reader, 12);
        entities.PreviousSsn.AddressStreet1 = db.GetNullableString(reader, 13);
        entities.PreviousSsn.AddressStreet2 = db.GetNullableString(reader, 14);
        entities.PreviousSsn.AddressCity = db.GetNullableString(reader, 15);
        entities.PreviousSsn.AddressState = db.GetNullableString(reader, 16);
        entities.PreviousSsn.AddressZip = db.GetNullableString(reader, 17);
        entities.PreviousSsn.Populated = true;
        CheckValid<AdministrativeActCertification>("CpaType",
          entities.PreviousSsn.CpaType);
        CheckValid<AdministrativeActCertification>("Type1",
          entities.PreviousSsn.Type1);
      });
  }

  private IEnumerable<bool> ReadFederalDebtSetoffCsePersonObligor()
  {
    entities.CsePerson.Populated = false;
    entities.Obligor.Populated = false;
    entities.FederalDebtSetoff.Populated = false;

    return ReadEach("ReadFederalDebtSetoffCsePersonObligor",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "dateSent", local.Null1.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.FederalDebtSetoff.CpaType = db.GetString(reader, 0);
        entities.Obligor.Type1 = db.GetString(reader, 0);
        entities.FederalDebtSetoff.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.Obligor.CspNumber = db.GetString(reader, 1);
        entities.Obligor.CspNumber = db.GetString(reader, 1);
        entities.FederalDebtSetoff.Type1 = db.GetString(reader, 2);
        entities.FederalDebtSetoff.TakenDate = db.GetDate(reader, 3);
        entities.FederalDebtSetoff.OriginalAmount =
          db.GetNullableDecimal(reader, 4);
        entities.FederalDebtSetoff.CurrentAmount =
          db.GetNullableDecimal(reader, 5);
        entities.FederalDebtSetoff.CurrentAmountDate =
          db.GetNullableDate(reader, 6);
        entities.FederalDebtSetoff.DecertifiedDate =
          db.GetNullableDate(reader, 7);
        entities.FederalDebtSetoff.NotificationDate =
          db.GetNullableDate(reader, 8);
        entities.FederalDebtSetoff.CreatedBy = db.GetString(reader, 9);
        entities.FederalDebtSetoff.CreatedTstamp = db.GetDateTime(reader, 10);
        entities.FederalDebtSetoff.LastUpdatedBy =
          db.GetNullableString(reader, 11);
        entities.FederalDebtSetoff.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 12);
        entities.FederalDebtSetoff.AdcAmount =
          db.GetNullableDecimal(reader, 13);
        entities.FederalDebtSetoff.NonAdcAmount =
          db.GetNullableDecimal(reader, 14);
        entities.FederalDebtSetoff.InjuredSpouseDate =
          db.GetNullableDate(reader, 15);
        entities.FederalDebtSetoff.NotifiedBy =
          db.GetNullableString(reader, 16);
        entities.FederalDebtSetoff.DateSent = db.GetNullableDate(reader, 17);
        entities.FederalDebtSetoff.EtypeAdministrativeOffset =
          db.GetNullableString(reader, 18);
        entities.FederalDebtSetoff.LocalCode = db.GetNullableString(reader, 19);
        entities.FederalDebtSetoff.Ssn = db.GetInt32(reader, 20);
        entities.FederalDebtSetoff.CaseNumber = db.GetString(reader, 21);
        entities.FederalDebtSetoff.LastName = db.GetString(reader, 22);
        entities.FederalDebtSetoff.FirstName = db.GetString(reader, 23);
        entities.FederalDebtSetoff.AmountOwed = db.GetInt32(reader, 24);
        entities.FederalDebtSetoff.TtypeAAddNewCase =
          db.GetNullableString(reader, 25);
        entities.FederalDebtSetoff.CaseType = db.GetString(reader, 26);
        entities.FederalDebtSetoff.TransferState =
          db.GetNullableString(reader, 27);
        entities.FederalDebtSetoff.LocalForTransfer =
          db.GetNullableInt32(reader, 28);
        entities.FederalDebtSetoff.ProcessYear =
          db.GetNullableInt32(reader, 29);
        entities.FederalDebtSetoff.TanfCode = db.GetString(reader, 30);
        entities.FederalDebtSetoff.TtypeDDeleteCertification =
          db.GetNullableString(reader, 31);
        entities.FederalDebtSetoff.TtypeLChangeSubmittingState =
          db.GetNullableString(reader, 32);
        entities.FederalDebtSetoff.TtypeMModifyAmount =
          db.GetNullableString(reader, 33);
        entities.FederalDebtSetoff.TtypeRModifyExclusion =
          db.GetNullableString(reader, 34);
        entities.FederalDebtSetoff.TtypeSStatePayment =
          db.GetNullableString(reader, 35);
        entities.FederalDebtSetoff.TtypeTTransferAdminReview =
          db.GetNullableString(reader, 36);
        entities.FederalDebtSetoff.TtypeBNameChange =
          db.GetNullableString(reader, 37);
        entities.FederalDebtSetoff.TtypeZAddressChange =
          db.GetNullableString(reader, 38);
        entities.FederalDebtSetoff.EtypeFederalRetirement =
          db.GetNullableString(reader, 39);
        entities.FederalDebtSetoff.EtypeFederalSalary =
          db.GetNullableString(reader, 40);
        entities.FederalDebtSetoff.EtypeTaxRefund =
          db.GetNullableString(reader, 41);
        entities.FederalDebtSetoff.EtypeVendorPaymentOrMisc =
          db.GetNullableString(reader, 42);
        entities.FederalDebtSetoff.EtypePassportDenial =
          db.GetNullableString(reader, 43);
        entities.FederalDebtSetoff.EtypeFinancialInstitution =
          db.GetNullableString(reader, 44);
        entities.FederalDebtSetoff.ChangeSsnInd =
          db.GetNullableString(reader, 45);
        entities.FederalDebtSetoff.EtypeAdmBankrupt =
          db.GetNullableString(reader, 46);
        entities.FederalDebtSetoff.DecertificationReason =
          db.GetNullableString(reader, 47);
        entities.FederalDebtSetoff.AddressStreet1 =
          db.GetNullableString(reader, 48);
        entities.FederalDebtSetoff.AddressStreet2 =
          db.GetNullableString(reader, 49);
        entities.FederalDebtSetoff.AddressCity =
          db.GetNullableString(reader, 50);
        entities.FederalDebtSetoff.AddressState =
          db.GetNullableString(reader, 51);
        entities.FederalDebtSetoff.AddressZip =
          db.GetNullableString(reader, 52);
        entities.CsePerson.Populated = true;
        entities.Obligor.Populated = true;
        entities.FederalDebtSetoff.Populated = true;
        CheckValid<AdministrativeActCertification>("CpaType",
          entities.FederalDebtSetoff.CpaType);
        CheckValid<CsePersonAccount>("Type1", entities.Obligor.Type1);
        CheckValid<AdministrativeActCertification>("Type1",
          entities.FederalDebtSetoff.Type1);

        return true;
      });
  }

  private void UpdateFederalDebtSetoff()
  {
    System.Diagnostics.Debug.Assert(entities.FederalDebtSetoff.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatedTstamp = Now();
    var dateSent = local.ProgramProcessingInfo.ProcessDate;

    entities.FederalDebtSetoff.Populated = false;
    Update("UpdateFederalDebtSetoff",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetNullableDate(command, "dateSent", dateSent);
        db.SetString(command, "cpaType", entities.FederalDebtSetoff.CpaType);
        db.
          SetString(command, "cspNumber", entities.FederalDebtSetoff.CspNumber);
          
        db.SetString(command, "type", entities.FederalDebtSetoff.Type1);
        db.SetDate(
          command, "takenDt",
          entities.FederalDebtSetoff.TakenDate.GetValueOrDefault());
        db.SetString(command, "tanfCode", entities.FederalDebtSetoff.TanfCode);
      });

    entities.FederalDebtSetoff.LastUpdatedBy = lastUpdatedBy;
    entities.FederalDebtSetoff.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.FederalDebtSetoff.DateSent = dateSent;
    entities.FederalDebtSetoff.Populated = true;
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
    /// <summary>A TransactionTotalsGroup group.</summary>
    [Serializable]
    public class TransactionTotalsGroup
    {
      /// <summary>
      /// A value of GrLocalNonAdcTransactionTot.
      /// </summary>
      [JsonPropertyName("grLocalNonAdcTransactionTot")]
      public Common GrLocalNonAdcTransactionTot
      {
        get => grLocalNonAdcTransactionTot ??= new();
        set => grLocalNonAdcTransactionTot = value;
      }

      /// <summary>
      /// A value of GrLocalAdcTransactionTotal.
      /// </summary>
      [JsonPropertyName("grLocalAdcTransactionTotal")]
      public Common GrLocalAdcTransactionTotal
      {
        get => grLocalAdcTransactionTotal ??= new();
        set => grLocalAdcTransactionTotal = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 8;

      private Common grLocalNonAdcTransactionTot;
      private Common grLocalAdcTransactionTotal;
    }

    /// <summary>A GrandTotalsGroup group.</summary>
    [Serializable]
    public class GrandTotalsGroup
    {
      /// <summary>
      /// A value of GrLocalGrandTotal.
      /// </summary>
      [JsonPropertyName("grLocalGrandTotal")]
      public Common GrLocalGrandTotal
      {
        get => grLocalGrandTotal ??= new();
        set => grLocalGrandTotal = value;
      }

      /// <summary>
      /// A value of GrLocalAdcSubtotal.
      /// </summary>
      [JsonPropertyName("grLocalAdcSubtotal")]
      public Common GrLocalAdcSubtotal
      {
        get => grLocalAdcSubtotal ??= new();
        set => grLocalAdcSubtotal = value;
      }

      /// <summary>
      /// A value of GrLocalNonAdcSubtotal.
      /// </summary>
      [JsonPropertyName("grLocalNonAdcSubtotal")]
      public Common GrLocalNonAdcSubtotal
      {
        get => grLocalNonAdcSubtotal ??= new();
        set => grLocalNonAdcSubtotal = value;
      }

      private Common grLocalGrandTotal;
      private Common grLocalAdcSubtotal;
      private Common grLocalNonAdcSubtotal;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    /// A value of NeededToOpen.
    /// </summary>
    [JsonPropertyName("neededToOpen")]
    public EabReportSend NeededToOpen
    {
      get => neededToOpen ??= new();
      set => neededToOpen = value;
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of FdsoCertificationTapeRecord.
    /// </summary>
    [JsonPropertyName("fdsoCertificationTapeRecord")]
    public FdsoCertificationTapeRecord FdsoCertificationTapeRecord
    {
      get => fdsoCertificationTapeRecord ??= new();
      set => fdsoCertificationTapeRecord = value;
    }

    /// <summary>
    /// A value of Xxx.
    /// </summary>
    [JsonPropertyName("xxx")]
    public FdsoCertificationTotalRecord Xxx
    {
      get => xxx ??= new();
      set => xxx = value;
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
    /// A value of UpdatesSinceCheckpoint.
    /// </summary>
    [JsonPropertyName("updatesSinceCheckpoint")]
    public Common UpdatesSinceCheckpoint
    {
      get => updatesSinceCheckpoint ??= new();
      set => updatesSinceCheckpoint = value;
    }

    /// <summary>
    /// Gets a value of TransactionTotals.
    /// </summary>
    [JsonIgnore]
    public Array<TransactionTotalsGroup> TransactionTotals =>
      transactionTotals ??= new(TransactionTotalsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of TransactionTotals for json serialization.
    /// </summary>
    [JsonPropertyName("transactionTotals")]
    [Computed]
    public IList<TransactionTotalsGroup> TransactionTotals_Json
    {
      get => transactionTotals;
      set => TransactionTotals.Assign(value);
    }

    /// <summary>
    /// Gets a value of GrandTotals.
    /// </summary>
    [JsonPropertyName("grandTotals")]
    public GrandTotalsGroup GrandTotals
    {
      get => grandTotals ?? (grandTotals = new());
      set => grandTotals = value;
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

    private Common common;
    private DateWorkArea null1;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private EabReportSend neededToOpen;
    private EabReportSend neededToWrite;
    private EabFileHandling eabFileHandling;
    private FdsoCertificationTapeRecord fdsoCertificationTapeRecord;
    private FdsoCertificationTotalRecord xxx;
    private Common numberOfReads;
    private Common updatesSinceCheckpoint;
    private Array<TransactionTotalsGroup> transactionTotals;
    private GrandTotalsGroup grandTotals;
    private External external;
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
    /// A value of FederalDebtSetoff.
    /// </summary>
    [JsonPropertyName("federalDebtSetoff")]
    public AdministrativeActCertification FederalDebtSetoff
    {
      get => federalDebtSetoff ??= new();
      set => federalDebtSetoff = value;
    }

    /// <summary>
    /// A value of PreviousSsn.
    /// </summary>
    [JsonPropertyName("previousSsn")]
    public AdministrativeActCertification PreviousSsn
    {
      get => previousSsn ??= new();
      set => previousSsn = value;
    }

    private CsePerson csePerson;
    private CsePersonAccount obligor;
    private AdministrativeActCertification federalDebtSetoff;
    private AdministrativeActCertification previousSsn;
  }
#endregion
}
