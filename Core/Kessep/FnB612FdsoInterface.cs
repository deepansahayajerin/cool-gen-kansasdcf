// Program: FN_B612_FDSO_INTERFACE, ID: 372528793, model: 746.
// Short name: SWEF612B
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
/// A program: FN_B612_FDSO_INTERFACE.
/// </para>
/// <para>
/// This skeleton is an example that uses:
/// A sequential driver file
/// An external to do a DB2 commit
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB612FdsoInterface: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B612_FDSO_INTERFACE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB612FdsoInterface(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB612FdsoInterface.
  /// </summary>
  public FnB612FdsoInterface(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***************************************************
    // Every initial development and change to that
    // development needs to be documented.
    // ***************************************************
    // *********************************************************************
    // Date      Developers Name         Request #  Description
    // --------  ----------------------  ---------  ------------------------
    // 05/30/96  Holly Kennedy - MTW                Initial-Cloned for Court 
    // Interface.
    // 01/20/99  SWSRPDP                            Changes to meet NEW DIR
    // Reporting Standards
    // 05/13/99  SWSRPDP                            The layout (Exhibit N) says 
    // pos. 15-29 of the INPUT file contains the CASE NUMBER. This is NOT
    // correct. The certification program is sending the Person Number in this
    // Field and the Feds are passing it back.
    // 09/11/99   H00073496  pphinney  Net Amount Problem   Program was NOT 
    // initially designed to handle Adjustment amounts greater than Collection
    // Amounts.  Made change to display and balance correctly if this occurs.
    // Also changed A/B FN_PROCESS_FDSO_INT_TOTAL_RECORD  to update Cash_Receipt
    // and Cash_Receipt_Event correctly - using negatives.
    // 09/30/99   H00075922  pphinney  Added code to prevent BATCHes containing 
    // DUPLICATE values from being added. (ie - Preventing same BATCH File from
    // being entered more than once). FN_FDSO_SDSO_FIND_DUP_BATCH.
    // 10/27/99   H00078281  pphinney  Added Person Name to ERROR message for 
    // Adjustments.
    // 12/17/99   H00082440  pphinney  Modified Report Layout.
    // 06/15/00   H00093019  pphinney  Added Joint Return Name -As this field 
    // was previously used to store the Case Type the name will be concatenated
    // after Case Type Information in the Joint Return Name field.
    // 06/15/00   H00093019  pphinney   pos 1-25 Case Type  - - pos 26-60 (35 
    // characters)  Joint Return Name.
    // ****************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.Save.Date = Now().Date;
    local.HardcodedEft.SystemGeneratedIdentifier = 6;
    local.HardcodeOpen.FileInstruction = "OPEN";
    local.HardcodeRead.FileInstruction = "READ";
    local.HardcodePosition.FileInstruction = "POSITION";
    local.FirstTime.Flag = "Y";
    local.NumberOfLines.Count = 75;

    // *****
    // Get the run parameters for this program.
    // *****
    local.ProgramProcessingInfo.Name = global.UserId;
    ReadProgramProcessingInfo1();
    UseReadProgramProcessingInfo();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // 09/30/99   H00075922  pphinney  Added code to prevent BATCHes 
      // containing DUPLICATE values from being added. This sets the NUMBER of
      // previous days to check for duplicates - IF ZERO do NOT check for
      // Duplicates
      local.FindDuplicateDaysCount.Count =
        (int)StringToNumber(Substring(
          local.ProgramProcessingInfo.ParameterList, 1, 3));
    }
    else
    {
      return;
    }

    // *****
    // Get the DB2 commit frequency counts and find out if we are in a restart 
    // situation.
    // *****
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmCheckpointRestart();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // ALWAYS ReStarts from Beginning
      // * * * NO Restart Logic * * *
    }
    else
    {
      return;
    }

    for(local.ControlTotals.Index = 0; local.ControlTotals.Index < 4; ++
      local.ControlTotals.Index)
    {
      if (!local.ControlTotals.CheckSize())
      {
        break;
      }

      switch(local.ControlTotals.Index + 1)
      {
        case 1:
          local.ControlTotals.Update.ProgramControlTotal.
            SystemGeneratedIdentifier = local.ControlTotals.Index + 1;
          local.ControlTotals.Update.ProgramControlTotal.Name =
            "Number of collections read       =";

          break;
        case 2:
          local.ControlTotals.Update.ProgramControlTotal.
            SystemGeneratedIdentifier = local.ControlTotals.Index + 1;
          local.ControlTotals.Update.ProgramControlTotal.Name =
            "Number of adjustments read       =";

          break;
        case 3:
          local.ControlTotals.Update.ProgramControlTotal.
            SystemGeneratedIdentifier = local.ControlTotals.Index + 1;
          local.ControlTotals.Update.ProgramControlTotal.Name =
            "Number of collections processed  =";

          break;
        case 4:
          local.ControlTotals.Update.ProgramControlTotal.
            SystemGeneratedIdentifier = local.ControlTotals.Index + 1;
          local.ControlTotals.Update.ProgramControlTotal.Name =
            "Number of adjustments processed  =";

          break;
        default:
          break;
      }
    }

    local.ControlTotals.CheckIndex();

    // *****
    // Open the Input File.  If this program run is a restart, open and 
    // reposition the file to the next record to be processed.
    // *****
    // *****
    // Call external to open the driver file.
    // *****
    local.PassArea.FileInstruction = local.HardcodeOpen.FileInstruction;
    UseEabFdsoInterfaceDrvr1();

    if (!Equal(local.PassArea.TextReturnCode, "00"))
    {
      ExitState = "ZD_FILE_OPEN_ERROR_WITH_AB";

      // 01/19/99     SWSRPDP    Set ERROR Flag
      local.AbendErrorFound.Flag = "Y";
    }

    local.Interfund.SystemGeneratedIdentifier = 10;

    // *****
    // Process driver records until we have reached the end of file.
    // *****
    local.Common.Count = 0;
    local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
      SystemGeneratedIdentifier = 0;

    do
    {
      if (AsChar(local.AbendErrorFound.Flag) == 'Y')
      {
        break;
      }

      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // *
      // Open successful - continue processing
      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // *
      global.Command = "";

      // ***** Call external to read the driver file.
      local.PassArea.FileInstruction = local.HardcodeRead.FileInstruction;
      UseEabFdsoInterfaceDrvr2();

      switch(TrimEnd(local.PassArea.TextReturnCode))
      {
        case "00":
          ++local.Common.Count;

          // 01/21/1999  SWSRPDP --- RESTART Logic
          if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
          {
            local.ProgramCheckpointRestart.RestartInd = "N";
          }

          ++local.ReadSinceLastCommit.Count;

          // Check the STATE in EACH DETAIL Record
          if (!Equal(local.AdditionalInfo.Adjustment.InterfaceTransId, 5, 2,
            "KS") && local.RecordTypeReturned.Count == 1)
          {
            local.AaaGroupLocalErrors.Index = 0;
            local.AaaGroupLocalErrors.CheckSize();

            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo = "Rec# " + NumberToString
              (local.ReadSinceLastCommit.Count, 10, 6);
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " Person Number = " +
              (local.CollectionRecord.Detail.ObligorPersonNumber ?? "");
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + ", SSN = " + (
                local.CollectionRecord.Detail.ObligorSocialSecurityNumber ?? ""
              );
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + ", TAX Yr = " +
              NumberToString
              (local.AdditionalInfo.Adjustment.OffsetTaxYear.
                GetValueOrDefault(), 12, 4);
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " **  State NOT = KS ";
              
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + ", Invalid State = " +
              Substring
              (local.AdditionalInfo.Adjustment.InterfaceTransId, 14, 5, 2);
            ExitState = "STATE_MUST_BE_KS";
            UseEabRollbackSql();
            local.AbendErrorFound.Flag = "Y";

            goto AfterCycle;
          }

          break;
        case "EF":
          if (local.Common.Count == 0)
          {
            ExitState = "ACO_RE0000_INPUT_FILE_EMPTY_RB";
            UseEabRollbackSql();

            // 01/19/99     SWSRPDP    Set ERROR Flag
            local.AbendErrorFound.Flag = "Y";

            goto AfterCycle;
          }

          // *****  End Of File.  The EAB closed the file.  Complete processing.
          goto AfterCycle;
        default:
          UseEabRollbackSql();
          ExitState = "FILE_READ_ERROR_WITH_RB";

          // *****
          // Set the Error Message Text to the exit state description.
          // *****
          local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();
          local.AaaGroupLocalErrors.Update.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo = ", ES = " + local
            .ExitStateWorkArea.Message;
          local.AaaGroupLocalErrors.Update.
            AaaGroupLocalErrorsDetailProgramError.SystemGeneratedIdentifier =
              local.AaaGroupLocalErrors.Item.
              AaaGroupLocalErrorsDetailProgramError.SystemGeneratedIdentifier +
            1;
          local.AbendErrorFound.Flag = "Y";

          goto AfterCycle;
      }

      // *****
      // Re-Initialize the error group view.  Set all of the attributes to 
      // spaces in case the new error does not overwrite the previous data.
      // Execute a "Repeat Targeting" to reset the cardinality to zero.
      // *****
      for(local.AaaGroupLocalErrors.Index = 0; local
        .AaaGroupLocalErrors.Index < local.AaaGroupLocalErrors.Count; ++
        local.AaaGroupLocalErrors.Index)
      {
        if (!local.AaaGroupLocalErrors.CheckSize())
        {
          break;
        }

        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          KeyInfo = "";
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          ProgramError1 = "";
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailStandard.
          Command = "";
      }

      local.AaaGroupLocalErrors.CheckIndex();
      local.AaaGroupLocalErrors.Index = -1;

      if (local.Common.Count == 1)
      {
        // *****
        // Process the Cash Receipt Event and Cash Receipt at the first detail 
        // record.
        // *****
        // *****
        // Set the Source Creation text date.  This date will be used to write 
        // out any errors found while processing the high level entities.
        // *****
        local.SourceCreation.TextDate =
          NumberToString(DateToInt(local.ProgramProcessingInfo.ProcessDate), 8,
          8);

        // * * * * * * * * * * * * * * * * * * * * * * * * * * *
        // TEST TEST TEST
        // For testing use PPI date - Normally will use Current_Date
        // This will allow multiple test in "T" on the same date
        // As long as the PPI date and Current Date are NOT the same
        // * * * * * * * * * * * * * * * * * * * * * * * * * * *
        local.CashReceiptEvent.SourceCreationDate =
          local.ProgramProcessingInfo.ProcessDate;

        // * * * * * * * * * * * * * * * * * * * * * * * * * * *
        // *
        // attention * * *   Disable the NEXT line for TESTing
        // attention * * *   Multiple runs in the same day.
        // *
        // * * * * * * * * * * * * * * * * * * * * * * * * * * *
        export.CashReceiptSourceType.Code = "FDSO";
        UseFnAddFdsoSdsoHeaderInfo();

        // *****
        // Check for errors processing the high level entities.  If an error was
        // encountered end the program.
        // *****
        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          // *** This is a critical error. Cannot proceed because of this error 
          // ***
          UseEabRollbackSql();

          local.AaaGroupLocalErrors.Index = 0;
          local.AaaGroupLocalErrors.CheckSize();

          // *****
          // Set the Error Message Text to the exit state description.
          // *****
          local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();

          if (IsExitState("INTERFACE_ALREADY_PROCESSED_RB"))
          {
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo = " DATE = " + local
              .SourceCreation.TextDate;
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                Substring(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo, 254, 1, 21) + " FDSO ";
              
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                Substring(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo, 254, 1, 27) + local
              .ExitStateWorkArea.Message;
          }
          else
          {
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo = " ES = " + local
              .ExitStateWorkArea.Message;
          }

          local.AbendErrorFound.Flag = "Y";

          break;
        }
      }

      switch(local.RecordTypeReturned.Count)
      {
        case 1:
          // *****
          // Record Type 1 is a Detail Record.  This could be just a payment or 
          // an adjustment and a payment.
          // *****
          // *****
          // Read and establish currency on the Cash Receipts.  This will allow 
          // matching to persistent views in called CABs.
          // *****
          if (ReadCashReceipt())
          {
            if (local.CollectionRecord.Detail.CollectionAmount != 0)
            {
              local.ControlTotals.Index = 0;
              local.ControlTotals.CheckSize();

              local.ControlTotals.Update.ProgramControlTotal.Value =
                local.ControlTotals.Item.ProgramControlTotal.Value.
                  GetValueOrDefault() + 1;
              UseFnProcessFdsoIntCollDtlRec();

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                local.ControlTotals.Index = 2;
                local.ControlTotals.CheckSize();

                local.ControlTotals.Update.ProgramControlTotal.Value =
                  local.ControlTotals.Item.ProgramControlTotal.Value.
                    GetValueOrDefault() + 1;
              }
            }

            if (local.CollectionRecord.DetailAdjustmentAmt.TotalCurrency != 0)
            {
              local.ControlTotals.Index = 1;
              local.ControlTotals.CheckSize();

              local.ControlTotals.Update.ProgramControlTotal.Value =
                local.ControlTotals.Item.ProgramControlTotal.Value.
                  GetValueOrDefault() + 1;
              UseFnProcessFdsoIntAdjustRec();

              if (IsExitState("FN0000_COLL_ADJUSTMENT_ADDED"))
              {
                // 12/17/99   H00082440  pphinney  Modified Report Layout.
                // This is NO LONGER reported
                ExitState = "ACO_NN0000_ALL_OK";
              }
              else
              {
              }

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                local.ControlTotals.Index = 3;
                local.ControlTotals.CheckSize();

                local.ControlTotals.Update.ProgramControlTotal.Value =
                  local.ControlTotals.Item.ProgramControlTotal.Value.
                    GetValueOrDefault() + 1;
              }
            }

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
            }
            else
            {
              // 12/17/99   H00082440  pphinney  Modified Report Layout.
              // * * * * * * * * * * * * * * * * * *
              local.AaaGroupLocalErrors.Index = 0;
              local.AaaGroupLocalErrors.CheckSize();

              // SSN to Report
              local.AaaGroupLocalErrors.Update.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                  local.CollectionRecord.Detail.ObligorSocialSecurityNumber ?? ""
                ;
              local.AaaGroupLocalErrors.Update.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                  Substring(local.AaaGroupLocalErrors.Item.
                  AaaGroupLocalErrorsDetailProgramError.KeyInfo, 254, 1, 3) + "-"
                + Substring
                (local.CollectionRecord.Detail.ObligorSocialSecurityNumber, 9,
                4, 2);
              local.AaaGroupLocalErrors.Update.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                  Substring(local.AaaGroupLocalErrors.Item.
                  AaaGroupLocalErrorsDetailProgramError.KeyInfo, 254, 1, 6) + "-"
                + Substring
                (local.CollectionRecord.Detail.ObligorSocialSecurityNumber, 9,
                6, 4);

              // Person Number to Report
              local.AaaGroupLocalErrors.Update.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                  Substring(local.AaaGroupLocalErrors.Item.
                  AaaGroupLocalErrorsDetailProgramError.KeyInfo, 254, 1, 13) + (
                  local.CollectionRecord.Detail.ObligorPersonNumber ?? "");

              // AP Name to Report
              local.AaaGroupLocalErrors.Update.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                  Substring(local.AaaGroupLocalErrors.Item.
                  AaaGroupLocalErrorsDetailProgramError.KeyInfo, 254, 1, 26) + (
                  local.CollectionRecord.Detail.ObligorLastName ?? "");
              local.AaaGroupLocalErrors.Update.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                  TrimEnd(local.AaaGroupLocalErrors.Item.
                  AaaGroupLocalErrorsDetailProgramError.KeyInfo) + ", " + (
                  local.CollectionRecord.Detail.ObligorFirstName ?? "");

              // Collection Amount to Report
              if (IsExitState("FN0000_ADJUSTMENT_AMOUNT_2_HIGH"))
              {
                local.AaaGroupLocalErrors.Update.
                  AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                    Substring(local.AaaGroupLocalErrors.Item.
                    AaaGroupLocalErrorsDetailProgramError.KeyInfo, 254, 1, 54) +
                  NumberToString
                  ((long)local.AvailiableCollections.TotalCurrency, 9, 7);
                local.AaaGroupLocalErrors.Update.
                  AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                    TrimEnd(local.AaaGroupLocalErrors.Item.
                    AaaGroupLocalErrorsDetailProgramError.KeyInfo) + "." + NumberToString
                  ((long)(local.AvailiableCollections.TotalCurrency * 100), 14,
                  2);
              }
              else
              {
                local.AaaGroupLocalErrors.Update.
                  AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                    Substring(local.AaaGroupLocalErrors.Item.
                    AaaGroupLocalErrorsDetailProgramError.KeyInfo, 254, 1, 54) +
                  " ";
              }

              // Adjustment Amount to Report
              local.AaaGroupLocalErrors.Update.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                  Substring(local.AaaGroupLocalErrors.Item.
                  AaaGroupLocalErrorsDetailProgramError.KeyInfo, 254, 1, 65) + NumberToString
                ((long)local.CollectionRecord.DetailAdjustmentAmt.TotalCurrency,
                9, 7);
              local.AaaGroupLocalErrors.Update.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                  TrimEnd(local.AaaGroupLocalErrors.Item.
                  AaaGroupLocalErrorsDetailProgramError.KeyInfo) + "." + NumberToString
                ((long)(local.CollectionRecord.DetailAdjustmentAmt.
                  TotalCurrency * 100), 14, 2);

              // YEAR to Report
              local.AaaGroupLocalErrors.Update.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                  Substring(local.AaaGroupLocalErrors.Item.
                  AaaGroupLocalErrorsDetailProgramError.KeyInfo, 254, 1, 76) + NumberToString
                (local.AdditionalInfo.Adjustment.OffsetTaxYear.
                  GetValueOrDefault(), 12, 4);

              // ERROR MESSAGE to Report
              // WR010504 - RETRO -12/03/01  - Added Exit States
              // sp0000_event_nf    cse_person_nf_rb   and    case_nf
              if (IsExitState("FN0000_ADJUSTMENT_AMOUNT_2_HIGH"))
              {
                local.AaaGroupLocalErrors.Update.
                  AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                    Substring(local.AaaGroupLocalErrors.Item.
                    AaaGroupLocalErrorsDetailProgramError.KeyInfo, 254, 1, 82) +
                  "Adjustment Amount Greater than Collection Amount";
              }
              else if (IsExitState("FN0000_COLLECTION_NOT_AVAILABLE"))
              {
                if (AsChar(local.NoMatchOnYear.Flag) == 'Y')
                {
                  // PR125     AUG. 30,1999
                  local.AaaGroupLocalErrors.Update.
                    AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                      Substring(local.AaaGroupLocalErrors.Item.
                      AaaGroupLocalErrorsDetailProgramError.KeyInfo, 254, 1,
                    82) + "No matching collection for SSN/Year Combination";
                }
                else
                {
                  // PR125     AUG. 30,1999
                  local.AaaGroupLocalErrors.Update.
                    AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                      Substring(local.AaaGroupLocalErrors.Item.
                      AaaGroupLocalErrorsDetailProgramError.KeyInfo, 254, 1,
                    82) + "No matching collection for this SSN";
                }
              }
              else if (IsExitState("CSE_PERSON_NF"))
              {
                local.AaaGroupLocalErrors.Update.
                  AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                    Substring(local.AaaGroupLocalErrors.Item.
                    AaaGroupLocalErrorsDetailProgramError.KeyInfo, 254, 1, 82) +
                  "Person Number NOT Found";
              }
              else if (IsExitState("CSE_PERSON_ADDRESS_NF_RB"))
              {
                local.AaaGroupLocalErrors.Update.
                  AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                    Substring(local.AaaGroupLocalErrors.Item.
                    AaaGroupLocalErrorsDetailProgramError.KeyInfo, 254, 1, 82) +
                  "Person Address NOT Found";
              }
              else if (IsExitState("CSE_PERSON_NF_RB"))
              {
                // WR010504 - RETRO -12/03/01  - Added Exit States
                --local.Swefb612AdjustErrCount.Count;
                local.Swefb612AdjErrAmount.TotalCurrency -= local.
                  CollectionRecord.DetailAdjustmentAmt.TotalCurrency;
                local.AaaGroupLocalErrors.Update.
                  AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                    Substring(local.AaaGroupLocalErrors.Item.
                    AaaGroupLocalErrorsDetailProgramError.KeyInfo, 254, 1, 82) +
                  "INFO - Prot Col Alert NOT Sent - AP Information NF";
              }
              else if (IsExitState("CASE_NF"))
              {
                // WR010504 - RETRO -12/03/01  - Added Exit States
                --local.Swefb612AdjustErrCount.Count;
                local.Swefb612AdjErrAmount.TotalCurrency -= local.
                  CollectionRecord.DetailAdjustmentAmt.TotalCurrency;
                local.AaaGroupLocalErrors.Update.
                  AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                    Substring(local.AaaGroupLocalErrors.Item.
                    AaaGroupLocalErrorsDetailProgramError.KeyInfo, 254, 1, 82) +
                  "INFO - Prot Col Alert Not Sent - AP/CASE Info NF";
              }
              else if (IsExitState("SP0000_EVENT_NF"))
              {
                --local.Swefb612AdjustErrCount.Count;
                local.Swefb612AdjErrAmount.TotalCurrency -= local.
                  CollectionRecord.DetailAdjustmentAmt.TotalCurrency;

                // WR010504 - RETRO -12/03/01  - Added Exit States
                local.AaaGroupLocalErrors.Update.
                  AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                    Substring(local.AaaGroupLocalErrors.Item.
                    AaaGroupLocalErrorsDetailProgramError.KeyInfo, 254, 1, 82) +
                  "INFO - Prot Col Alert NOT Sent - Creation Error";
              }
              else
              {
                local.AaaGroupLocalErrors.Update.
                  AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                    Substring(local.AaaGroupLocalErrors.Item.
                    AaaGroupLocalErrorsDetailProgramError.KeyInfo, 254, 1, 82) +
                  "MAJOR ERROR - Program  Abend";
                UseEabRollbackSql();
                local.ExitStateWorkArea.Message =
                  UseEabExtractExitStateMessage();
                local.AbendErrorFound.Flag = "Y";
              }

              // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
              // * * * * * *
              // Open the ERROR Report
              // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
              // * * * * * *
              if (AsChar(local.ErrorOpened.Flag) != 'Y')
              {
                local.ReportHandlingEabReportSend.ProcessDate =
                  local.ProgramProcessingInfo.ProcessDate;
                local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
                local.ReportHandlingEabFileHandling.Action = "OPEN";
                UseCabErrorReport2();

                if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
                {
                }
                else
                {
                  UseEabRollbackSql();
                  ExitState = "SI0000_ERR_OPENING_ERR_RPT_FILE";

                  return;
                }

                local.ErrorOpened.Flag = "Y";
              }

              if (local.NumberOfLines.Count > 48)
              {
                // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
                // * * * * * * *
                // Write the ERROR Report
                // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
                // * * * * * * *
                local.ReportHandlingEabReportSend.RptDetail =
                  "SSN          CSE person   AP Name                     Col. Amt.  Adj. Amt.  Year  Message";
                  
                local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
                local.ReportHandlingEabFileHandling.Action = "WRITE";
                UseCabErrorReport1();

                if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
                {
                }
                else
                {
                  UseEabRollbackSql();
                  ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

                  return;
                }

                // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
                // * * * * * * *
                // Write the ERROR Report -- Spacing
                // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
                // * * * * * * *
                local.ReportHandlingEabReportSend.RptDetail = "";
                local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
                local.ReportHandlingEabFileHandling.Action = "WRITE";
                UseCabErrorReport1();

                if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
                {
                }
                else
                {
                  UseEabRollbackSql();
                  ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

                  return;
                }

                local.NumberOfLines.Count = 0;
              }

              // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
              // * * * * * *
              // Write the ERROR Report
              // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
              // * * * * * *
              local.ReportHandlingEabReportSend.RptDetail =
                local.AaaGroupLocalErrors.Item.
                  AaaGroupLocalErrorsDetailProgramError.KeyInfo ?? Spaces(132);
              local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
              local.ReportHandlingEabFileHandling.Action = "WRITE";
              UseCabErrorReport1();

              if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
              {
              }
              else
              {
                UseEabRollbackSql();
                ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

                return;
              }

              ++local.NumberOfLines.Count;
              ++local.Swefb612AdjustErrCount.Count;
              local.Swefb612AdjErrAmount.TotalCurrency += local.
                CollectionRecord.DetailAdjustmentAmt.TotalCurrency;

              // WR010504 - RETRO -12/03/01  - Added Exit States
              // sp0000_event_nf    cse_person_nf_rb   and    case_nf
              if (IsExitState("FN0000_ADJUSTMENT_AMOUNT_2_HIGH") || IsExitState
                ("FN0000_COLLECTION_NOT_AVAILABLE") || IsExitState
                ("CSE_PERSON_ADDRESS_NF_RB") || IsExitState
                ("CSE_PERSON_NF") || IsExitState
                ("FN0000_COLL_ADJUSTMENT_ADDED") || IsExitState
                ("SP0000_EVENT_NF") || IsExitState("CSE_PERSON_NF_RB") || IsExitState
                ("CASE_NF"))
              {
                ExitState = "ACO_NN0000_ALL_OK";

                continue;
              }
              else
              {
                local.AbendErrorFound.Flag = "Y";
                local.AaaGroupLocalErrors.Update.
                  AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                    TrimEnd(local.AaaGroupLocalErrors.Item.
                    AaaGroupLocalErrorsDetailProgramError.KeyInfo) + ", ES= " +
                  local.ExitStateWorkArea.Message;
                local.AaaGroupLocalErrors.Update.
                  AaaGroupLocalErrorsDetailProgramError.
                    SystemGeneratedIdentifier =
                    local.AaaGroupLocalErrors.Item.
                    AaaGroupLocalErrorsDetailProgramError.
                    SystemGeneratedIdentifier + 1;

                goto AfterCycle;
              }
            }
          }
          else
          {
            UseEabRollbackSql();

            local.AaaGroupLocalErrors.Index = 0;
            local.AaaGroupLocalErrors.CheckSize();

            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.ProgramError1 =
                "failed on read cash receipt";
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo = "SSN = " + (
                local.CollectionRecord.Detail.ObligorSocialSecurityNumber ?? ""
              );
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + ", Record number in error = " +
              NumberToString(local.Common.Count, 15);
            local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + ", ES= " + local
              .ExitStateWorkArea.Message;
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.SystemGeneratedIdentifier =
                local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.
                SystemGeneratedIdentifier + 1;
            local.AbendErrorFound.Flag = "Y";

            goto AfterCycle;
          }

          break;
        case 2:
          // *****
          // Record type 2 is the total record.
          // *****
          UseFnProcessFdsoIntTotalRecord();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            local.AaaGroupLocalErrors.Index = 0;
            local.AaaGroupLocalErrors.CheckSize();

            if (local.Swefb612CollectionAmt.TotalCurrency != local
              .TotalRecord.DetailCashReceipt.ReceiptAmount || local
              .Swefb612CollectionCount.Count != local
              .TotalRecord.DetailCashReceipt.TotalNoncashTransactionCount.
                GetValueOrDefault())
            {
              // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
              // * * * * * *
              // Collections OUT of BALANCE
              // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
              // * * * * * *
              // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
              // * *
              // Open the ERROR Report
              // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
              // * *
              if (AsChar(local.ErrorOpened.Flag) != 'Y')
              {
                local.ReportHandlingEabReportSend.ProcessDate =
                  local.ProgramProcessingInfo.ProcessDate;
                local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
                local.ReportHandlingEabFileHandling.Action = "OPEN";
                UseCabErrorReport2();

                if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
                {
                }
                else
                {
                  UseEabRollbackSql();
                  ExitState = "SI0000_ERR_OPENING_ERR_RPT_FILE";

                  return;
                }

                local.ErrorOpened.Flag = "Y";
              }

              // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
              // * * * * * *
              // Write the ERROR Report - Spacing
              // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
              // * * * * * *
              local.ReportHandlingEabReportSend.RptDetail = "";
              local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
              local.ReportHandlingEabFileHandling.Action = "WRITE";
              UseCabErrorReport1();

              if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
              {
                local.AbendErrorFound.Flag = "Y";
              }
              else
              {
                UseEabRollbackSql();
                ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

                return;
              }

              // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
              // * * * * * *
              // Write the ERROR Report - Header
              // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
              // * * * * * *
              local.ReportHandlingEabReportSend.RptDetail =
                "*** COLLECTIONS Out of Balance ***";
              local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
              local.ReportHandlingEabFileHandling.Action = "WRITE";
              UseCabErrorReport1();

              if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
              {
                local.AbendErrorFound.Flag = "Y";
              }
              else
              {
                UseEabRollbackSql();
                ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

                return;
              }

              // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
              // * * * * * *
              // Write the ERROR Report - Feds Sent
              // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
              // * * * * * *
              local.ReportHandlingEabReportSend.RptDetail =
                "Federal FDSO Total Record: Collection Amount =";
              local.ReportHandlingEabReportSend.RptDetail =
                Substring(local.ReportHandlingEabReportSend.RptDetail,
                EabReportSend.RptDetail_MaxLength, 1, 47) + NumberToString
                ((long)(local.TotalRecord.DetailCashReceipt.ReceiptAmount * 100),
                15);
              local.ReportHandlingEabReportSend.RptDetail =
                Substring(local.ReportHandlingEabReportSend.RptDetail,
                EabReportSend.RptDetail_MaxLength, 1, 60) + "." + Substring
                (local.ReportHandlingEabReportSend.RptDetail,
                EabReportSend.RptDetail_MaxLength, 61, 2);
              local.ReportHandlingEabReportSend.RptDetail =
                Substring(local.ReportHandlingEabReportSend.RptDetail,
                EabReportSend.RptDetail_MaxLength, 1, 64) + "Collection Count =";
                
              local.ReportHandlingEabReportSend.RptDetail =
                Substring(local.ReportHandlingEabReportSend.RptDetail,
                EabReportSend.RptDetail_MaxLength, 1, 84) + NumberToString
                (local.TotalRecord.DetailCashReceipt.
                  TotalNoncashTransactionCount.GetValueOrDefault(), 15);
              local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
              local.ReportHandlingEabFileHandling.Action = "WRITE";
              UseCabErrorReport1();

              if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
              {
                local.AbendErrorFound.Flag = "Y";
              }
              else
              {
                UseEabRollbackSql();
                ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

                return;
              }

              // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
              // * * * * * *
              // Write the ERROR Report - Calculated
              // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
              // * * * * * *
              local.ReportHandlingEabReportSend.RptDetail =
                "System Calculated Totals:  Collection Amount =";
              local.ReportHandlingEabReportSend.RptDetail =
                Substring(local.ReportHandlingEabReportSend.RptDetail,
                EabReportSend.RptDetail_MaxLength, 1, 47) + NumberToString
                ((long)(local.Swefb612CollectionAmt.TotalCurrency * 100), 15);
              local.ReportHandlingEabReportSend.RptDetail =
                Substring(local.ReportHandlingEabReportSend.RptDetail,
                EabReportSend.RptDetail_MaxLength, 1, 60) + "." + Substring
                (local.ReportHandlingEabReportSend.RptDetail,
                EabReportSend.RptDetail_MaxLength, 61, 2);
              local.ReportHandlingEabReportSend.RptDetail =
                Substring(local.ReportHandlingEabReportSend.RptDetail,
                EabReportSend.RptDetail_MaxLength, 1, 64) + "Collection Count =";
                
              local.ReportHandlingEabReportSend.RptDetail =
                Substring(local.ReportHandlingEabReportSend.RptDetail,
                EabReportSend.RptDetail_MaxLength, 1, 84) + NumberToString
                (local.Swefb612CollectionCount.Count, 15);
              local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
              local.ReportHandlingEabFileHandling.Action = "WRITE";
              UseCabErrorReport1();

              if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
              {
                local.AbendErrorFound.Flag = "Y";
              }
              else
              {
                UseEabRollbackSql();
                ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

                return;
              }

              local.AbendErrorFound.Flag = "Y";
            }

            if (local.Swefb612AdjustmentAmt.TotalCurrency + local
              .Swefb612AdjErrAmount.TotalCurrency != local
              .TotalRecord.DetailAdjAmt.TotalCurrency || local
              .Swefb612AdjustmentCount.Count + local
              .Swefb612AdjustErrCount.Count != local
              .TotalRecord.DetailCashReceiptEvent.TotalAdjustmentCount.
                GetValueOrDefault())
            {
              // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
              // * *
              // Open the ERROR Report
              // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
              // * *
              if (AsChar(local.ErrorOpened.Flag) != 'Y')
              {
                local.ReportHandlingEabReportSend.ProcessDate =
                  local.ProgramProcessingInfo.ProcessDate;
                local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
                local.ReportHandlingEabFileHandling.Action = "OPEN";
                UseCabErrorReport2();

                if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
                {
                }
                else
                {
                  UseEabRollbackSql();
                  ExitState = "SI0000_ERR_OPENING_ERR_RPT_FILE";

                  return;
                }

                local.ErrorOpened.Flag = "Y";
              }

              // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
              // * * * * * *
              // Write the ERROR Report - Spacing
              // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
              // * * * * * *
              local.ReportHandlingEabReportSend.RptDetail = "";
              local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
              local.ReportHandlingEabFileHandling.Action = "WRITE";
              UseCabErrorReport1();

              if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
              {
                local.AbendErrorFound.Flag = "Y";
              }
              else
              {
                UseEabRollbackSql();
                ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

                return;
              }

              // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
              // * * * * * *
              // Write the ERROR Report - Header
              // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
              // * * * * * *
              local.ReportHandlingEabReportSend.RptDetail =
                "*** ADJUSTMENTS Out of Balance ***";
              local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
              local.ReportHandlingEabFileHandling.Action = "WRITE";
              UseCabErrorReport1();

              if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
              {
                local.AbendErrorFound.Flag = "Y";
              }
              else
              {
                UseEabRollbackSql();
                ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

                return;
              }

              // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
              // * * * * * *
              // Write the ERROR Report - Feds Sent
              // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
              // * * * * * *
              local.ReportHandlingEabReportSend.RptDetail =
                "Federal FDSO Total Record: Adjustment Amount =";
              local.ReportHandlingEabReportSend.RptDetail =
                Substring(local.ReportHandlingEabReportSend.RptDetail,
                EabReportSend.RptDetail_MaxLength, 1, 47) + NumberToString
                ((long)(local.TotalRecord.DetailAdjAmt.TotalCurrency * 100), 15);
                
              local.ReportHandlingEabReportSend.RptDetail =
                Substring(local.ReportHandlingEabReportSend.RptDetail,
                EabReportSend.RptDetail_MaxLength, 1, 60) + "." + Substring
                (local.ReportHandlingEabReportSend.RptDetail,
                EabReportSend.RptDetail_MaxLength, 61, 2);
              local.ReportHandlingEabReportSend.RptDetail =
                Substring(local.ReportHandlingEabReportSend.RptDetail,
                EabReportSend.RptDetail_MaxLength, 1, 64) + "Adjustment Count =";
                
              local.ReportHandlingEabReportSend.RptDetail =
                Substring(local.ReportHandlingEabReportSend.RptDetail,
                EabReportSend.RptDetail_MaxLength, 1, 84) + NumberToString
                (local.TotalRecord.DetailCashReceiptEvent.TotalAdjustmentCount.
                  GetValueOrDefault(), 15);
              local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
              local.ReportHandlingEabFileHandling.Action = "WRITE";
              UseCabErrorReport1();

              if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
              {
                local.AbendErrorFound.Flag = "Y";
              }
              else
              {
                UseEabRollbackSql();
                ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

                return;
              }

              // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
              // * * * * * *
              // Write the ERROR Report - Calculated
              // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
              // * * * * * *
              local.ReportHandlingEabReportSend.RptDetail =
                "System Calculated Totals:  Adjustment Amount =";
              local.ReportHandlingEabReportSend.RptDetail =
                Substring(local.ReportHandlingEabReportSend.RptDetail,
                EabReportSend.RptDetail_MaxLength, 1, 47) + NumberToString
                ((long)(local.Swefb612AdjustmentAmt.TotalCurrency * 100), 15);
              local.ReportHandlingEabReportSend.RptDetail =
                Substring(local.ReportHandlingEabReportSend.RptDetail,
                EabReportSend.RptDetail_MaxLength, 1, 60) + "." + Substring
                (local.ReportHandlingEabReportSend.RptDetail,
                EabReportSend.RptDetail_MaxLength, 61, 2);
              local.ReportHandlingEabReportSend.RptDetail =
                Substring(local.ReportHandlingEabReportSend.RptDetail,
                EabReportSend.RptDetail_MaxLength, 1, 64) + "Adjustment Count =";
                
              local.ReportHandlingEabReportSend.RptDetail =
                Substring(local.ReportHandlingEabReportSend.RptDetail,
                EabReportSend.RptDetail_MaxLength, 1, 84) + NumberToString
                (local.Swefb612AdjustmentCount.Count, 15);
              local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
              local.ReportHandlingEabFileHandling.Action = "WRITE";
              UseCabErrorReport1();

              if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
              {
                local.AbendErrorFound.Flag = "Y";
              }
              else
              {
                UseEabRollbackSql();
                ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

                return;
              }

              // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
              // * * * * * *
              // Write the ERROR Report - Bypassed
              // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
              // * * * * * *
              local.ReportHandlingEabReportSend.RptDetail =
                "Errors ByPassed:           Adjustment Amount =";
              local.ReportHandlingEabReportSend.RptDetail =
                Substring(local.ReportHandlingEabReportSend.RptDetail,
                EabReportSend.RptDetail_MaxLength, 1, 47) + NumberToString
                ((long)(local.Swefb612AdjErrAmount.TotalCurrency * 100), 15);
              local.ReportHandlingEabReportSend.RptDetail =
                Substring(local.ReportHandlingEabReportSend.RptDetail,
                EabReportSend.RptDetail_MaxLength, 1, 60) + "." + Substring
                (local.ReportHandlingEabReportSend.RptDetail,
                EabReportSend.RptDetail_MaxLength, 61, 2);
              local.ReportHandlingEabReportSend.RptDetail =
                Substring(local.ReportHandlingEabReportSend.RptDetail,
                EabReportSend.RptDetail_MaxLength, 1, 64) + "Adjustment Count =";
                
              local.ReportHandlingEabReportSend.RptDetail =
                Substring(local.ReportHandlingEabReportSend.RptDetail,
                EabReportSend.RptDetail_MaxLength, 1, 84) + NumberToString
                (local.Swefb612AdjustErrCount.Count, 15);
              local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
              local.ReportHandlingEabFileHandling.Action = "WRITE";
              UseCabErrorReport1();

              if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
              {
                local.AbendErrorFound.Flag = "Y";
              }
              else
              {
                UseEabRollbackSql();
                ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

                return;
              }

              local.AbendErrorFound.Flag = "Y";
            }

            // Need to subtract the ByPassed records from Net Amount
            local.Swefb612NetAmount.TotalCurrency -= local.Swefb612AdjErrAmount.
              TotalCurrency;

            // 09/11/99   H00073496  pphinney  Net Amount Problem   FIX here?
            if (local.Swefb612NetAmount.TotalCurrency < 0)
            {
              local.NegativeNetFlag.Flag = "Y";
            }

            // * * * * * * * * * * * * * * * * * * *
            // 09/11/99   H00073496  pphinney  Net Amount Problem   FIX here?
            // If Calculated is Negative and Total record is Positive -- Make 
            // Calculated Positive,
            if (local.Swefb612NetAmount.TotalCurrency < 0 && local
              .TotalRecord.NetAmount.TotalCurrency == 0)
            {
              local.Swefb612NetAmount.TotalCurrency =
                -local.Swefb612NetAmount.TotalCurrency;
              local.TotalRecord.NetAmount.TotalCurrency =
                local.Swefb612NetAmount.TotalCurrency;
            }

            // * * * * * * * * * * * * * * * * * * *
            if (local.Swefb612NetAmount.TotalCurrency < 0 && local
              .TotalRecord.NetAmount.TotalCurrency > 0)
            {
              local.Swefb612NetAmount.TotalCurrency =
                -local.Swefb612NetAmount.TotalCurrency;
            }

            if (local.Swefb612NetAmount.TotalCurrency != local
              .TotalRecord.NetAmount.TotalCurrency)
            {
              // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
              // * *
              // Open the ERROR Report
              // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
              // * *
              if (AsChar(local.ErrorOpened.Flag) != 'Y')
              {
                local.ReportHandlingEabReportSend.ProcessDate =
                  local.ProgramProcessingInfo.ProcessDate;
                local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
                local.ReportHandlingEabFileHandling.Action = "OPEN";
                UseCabErrorReport2();

                if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
                {
                }
                else
                {
                  UseEabRollbackSql();
                  ExitState = "SI0000_ERR_OPENING_ERR_RPT_FILE";

                  return;
                }

                local.ErrorOpened.Flag = "Y";
              }

              // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
              // * * * * * *
              // Write the ERROR Report - Spacing
              // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
              // * * * * * *
              local.ReportHandlingEabReportSend.RptDetail = "";
              local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
              local.ReportHandlingEabFileHandling.Action = "WRITE";
              UseCabErrorReport1();

              if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
              {
                local.AbendErrorFound.Flag = "Y";
              }
              else
              {
                UseEabRollbackSql();
                ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

                return;
              }

              // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
              // * * * * * *
              // Write the ERROR Report - Header
              // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
              // * * * * * *
              local.ReportHandlingEabReportSend.RptDetail =
                "*** NET TOTALS Out of Balance ***";
              local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
              local.ReportHandlingEabFileHandling.Action = "WRITE";
              UseCabErrorReport1();

              if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
              {
                local.AbendErrorFound.Flag = "Y";
              }
              else
              {
                UseEabRollbackSql();
                ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

                return;
              }

              // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
              // * * * * * *
              // Write the ERROR Report - Feds Sent (Total Record)
              // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
              // * * * * * *
              local.ReportHandlingEabReportSend.RptDetail =
                "Federal FDSO Total Record:        Net Amount =";
              local.ReportHandlingEabReportSend.RptDetail =
                Substring(local.ReportHandlingEabReportSend.RptDetail,
                EabReportSend.RptDetail_MaxLength, 1, 47) + NumberToString
                ((long)(local.TotalRecord.NetAmount.TotalCurrency * 100), 15);
              local.ReportHandlingEabReportSend.RptDetail =
                Substring(local.ReportHandlingEabReportSend.RptDetail,
                EabReportSend.RptDetail_MaxLength, 1, 60) + "." + Substring
                (local.ReportHandlingEabReportSend.RptDetail,
                EabReportSend.RptDetail_MaxLength, 61, 2);
              local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
              local.ReportHandlingEabFileHandling.Action = "WRITE";
              UseCabErrorReport1();

              if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
              {
                local.AbendErrorFound.Flag = "Y";
              }
              else
              {
                UseEabRollbackSql();
                ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

                return;
              }

              // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
              // * * * * * *
              // Write the ERROR Report - Calculated
              // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
              // * * * * * *
              local.ReportHandlingEabReportSend.RptDetail =
                "System Calculated Totals:         Net Amount =";
              local.ReportHandlingEabReportSend.RptDetail =
                Substring(local.ReportHandlingEabReportSend.RptDetail,
                EabReportSend.RptDetail_MaxLength, 1, 47) + NumberToString
                ((long)(local.Swefb612NetAmount.TotalCurrency * 100), 15);
              local.ReportHandlingEabReportSend.RptDetail =
                Substring(local.ReportHandlingEabReportSend.RptDetail,
                EabReportSend.RptDetail_MaxLength, 1, 60) + "." + Substring
                (local.ReportHandlingEabReportSend.RptDetail,
                EabReportSend.RptDetail_MaxLength, 61, 2);
              local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
              local.ReportHandlingEabFileHandling.Action = "WRITE";
              UseCabErrorReport1();

              if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
              {
                local.AbendErrorFound.Flag = "Y";
              }
              else
              {
                UseEabRollbackSql();
                ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

                return;
              }

              local.AbendErrorFound.Flag = "Y";
            }

            ExitState = "ACO_NN0000_ALL_OK";
          }
          else if (IsExitState("ACO_NI0000_PROCESSING_COMPLETE"))
          {
          }
          else
          {
            UseEabRollbackSql();

            local.AaaGroupLocalErrors.Index = 0;
            local.AaaGroupLocalErrors.CheckSize();

            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.ProgramError1 =
                "failed on fn_process_fdso_int_total_record";
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo = "SSN = " + (
                local.CollectionRecord.Detail.ObligorSocialSecurityNumber ?? ""
              );
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + ", Record number in error = " +
              NumberToString(local.Common.Count, 15);
            local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + ", ES = " + local
              .ExitStateWorkArea.Message;
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.SystemGeneratedIdentifier =
                local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.
                SystemGeneratedIdentifier + 1;
            local.AbendErrorFound.Flag = "Y";

            goto AfterCycle;
          }

          break;
        default:
          ExitState = "ACO_RE0000_INPUT_RECORD_TYPE_INV";
          UseEabRollbackSql();

          local.AaaGroupLocalErrors.Index = 0;
          local.AaaGroupLocalErrors.CheckSize();

          local.AaaGroupLocalErrors.Update.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo = "SSN = " + (
              local.CollectionRecord.Detail.ObligorSocialSecurityNumber ?? "");
          local.AaaGroupLocalErrors.Update.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo =
              TrimEnd(local.AaaGroupLocalErrors.Item.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo) + ", Record number in error = " +
            NumberToString(local.Common.Count, 15);
          local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();
          local.AaaGroupLocalErrors.Update.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo =
              TrimEnd(local.AaaGroupLocalErrors.Item.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo) + ", ES = " + local
            .ExitStateWorkArea.Message;
          local.AaaGroupLocalErrors.Update.
            AaaGroupLocalErrorsDetailProgramError.SystemGeneratedIdentifier =
              local.AaaGroupLocalErrors.Item.
              AaaGroupLocalErrorsDetailProgramError.SystemGeneratedIdentifier +
            1;
          local.AbendErrorFound.Flag = "Y";

          goto AfterCycle;
      }
    }
    while(!Equal(local.PassArea.TextReturnCode, "EF"));

AfterCycle:

    // *****
    // The external hit the end of the driver file,
    // closed the file and returned an EF (EOF) indicator.
    // *****
    // 09/30/99   H00075922  pphinney  Added code to prevent BATCHes containing 
    // DUPLICATE values from being added. (ie - Preventing same BATCH File from
    // being entered more than once). FN_FDSO_SDSO_CHECK_FOR_DUP_BATCH
    if (AsChar(local.AbendErrorFound.Flag) != 'Y')
    {
      // IF PPI Number of Days had been set to ZERO
      // DO NOT check for Duplicate Amounts
      if (local.FindDuplicateDaysCount.Count == 0)
      {
        goto Test;
      }

      local.FindDuplicate.ReceiptDate = entities.CashReceipt.ReceiptDate;
      local.FindDuplicate.ReceivedDate = entities.CashReceipt.ReceivedDate;
      UseFnFdsoSdsoCheckForDupBatch();

      if (local.FindDuplicateCount.Count > 0)
      {
        local.AbendErrorFound.Flag = "Y";
        UseEabRollbackSql();

        // LINE 1
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          KeyInfo = "Current Batch File for " + NumberToString
          (DateToInt(local.CashReceiptEvent.ReceivedDate), 12, 2) + "/";
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          KeyInfo =
            TrimEnd(local.AaaGroupLocalErrors.Item.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo) + TrimEnd
          (NumberToString(DateToInt(local.CashReceiptEvent.ReceivedDate), 14, 2))
          + "/";
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          KeyInfo =
            TrimEnd(local.AaaGroupLocalErrors.Item.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo) + TrimEnd
          (NumberToString(DateToInt(local.CashReceiptEvent.ReceivedDate), 8, 4)) +
          " matches the counts and amounts from Already Existing Cash Receipt";
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          KeyInfo =
            TrimEnd(local.AaaGroupLocalErrors.Item.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " Number " + NumberToString
          (local.FindDuplicate.SequentialNumber, 6, 10);

        // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
        // * * *
        // Open the ERROR Report
        // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
        // * * *
        if (AsChar(local.ErrorOpened.Flag) != 'Y')
        {
          local.ReportHandlingEabReportSend.ProcessDate =
            local.ProgramProcessingInfo.ProcessDate;
          local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
          local.ReportHandlingEabFileHandling.Action = "OPEN";
          UseCabErrorReport2();

          if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
          {
          }
          else
          {
            UseEabRollbackSql();
            ExitState = "SI0000_ERR_OPENING_ERR_RPT_FILE";

            return;
          }

          local.ErrorOpened.Flag = "Y";
        }

        // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
        // * * *
        // Write the ERROR Report
        // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
        // * * *
        local.ReportHandlingEabReportSend.RptDetail =
          local.AaaGroupLocalErrors.Item.AaaGroupLocalErrorsDetailProgramError.
            KeyInfo ?? Spaces(132);
        local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
        local.ReportHandlingEabFileHandling.Action = "WRITE";
        UseCabErrorReport1();

        if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
        {
        }
        else
        {
          UseEabRollbackSql();
          ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

          return;
        }

        // LINE 2
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          KeyInfo = " Received on " + NumberToString
          (DateToInt(local.FindDuplicate.ReceivedDate), 12, 1);
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          KeyInfo =
            TrimEnd(local.AaaGroupLocalErrors.Item.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo) + TrimEnd
          (NumberToString(DateToInt(local.FindDuplicate.ReceivedDate), 13, 1)) +
          "/";
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          KeyInfo =
            TrimEnd(local.AaaGroupLocalErrors.Item.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo) + TrimEnd
          (NumberToString(DateToInt(local.FindDuplicate.ReceivedDate), 14, 2)) +
          "/";
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          KeyInfo =
            TrimEnd(local.AaaGroupLocalErrors.Item.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo) + TrimEnd
          (NumberToString(DateToInt(local.FindDuplicate.ReceivedDate), 8, 4)) +
          " ";
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          KeyInfo =
            TrimEnd(local.AaaGroupLocalErrors.Item.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " with a Cash amount of " +
          NumberToString
          ((long)local.FindDuplicate.TotalCashTransactionAmount.
            GetValueOrDefault(), 6, 10);
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          KeyInfo =
            TrimEnd(local.AaaGroupLocalErrors.Item.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo) + "." + NumberToString
          ((long)(local.FindDuplicate.TotalCashTransactionAmount.
            GetValueOrDefault() * 100), 14, 2);

        if (local.FindDuplicate.TotalCashTransactionAmount.
          GetValueOrDefault() < 0)
        {
          local.AaaGroupLocalErrors.Update.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo =
              TrimEnd(local.AaaGroupLocalErrors.Item.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo) + "-";
        }

        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          KeyInfo =
            TrimEnd(local.AaaGroupLocalErrors.Item.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " and a Cash Due amount of " +
          NumberToString
          ((long)local.FindDuplicate.CashDue.GetValueOrDefault(), 6, 10);
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          KeyInfo =
            TrimEnd(local.AaaGroupLocalErrors.Item.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo) + "." + NumberToString
          ((long)(local.FindDuplicate.CashDue.GetValueOrDefault() * 100), 14, 2);
          

        if (local.FindDuplicate.CashDue.GetValueOrDefault() < 0)
        {
          local.AaaGroupLocalErrors.Update.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo =
              TrimEnd(local.AaaGroupLocalErrors.Item.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo) + "-";
        }

        // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
        // * * *
        // Write the ERROR Report
        // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
        // * * *
        local.ReportHandlingEabReportSend.RptDetail =
          local.AaaGroupLocalErrors.Item.AaaGroupLocalErrorsDetailProgramError.
            KeyInfo ?? Spaces(132);
        local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
        local.ReportHandlingEabFileHandling.Action = "WRITE";
        UseCabErrorReport1();

        if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
        {
        }
        else
        {
          UseEabRollbackSql();
          ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

          return;
        }

        // LINE 3
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          KeyInfo = "";
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          KeyInfo =
            TrimEnd(local.AaaGroupLocalErrors.Item.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " with " + NumberToString
          (local.FindDuplicate.TotalCashTransactionCount.GetValueOrDefault(), 6,
          10);
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          KeyInfo =
            TrimEnd(local.AaaGroupLocalErrors.Item.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " Collections and " +
          NumberToString
          (local.FindDuplicate.TotalDetailAdjustmentCount.GetValueOrDefault(),
          6, 10);
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          KeyInfo =
            TrimEnd(local.AaaGroupLocalErrors.Item.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " " + "Adjustments * * *  VERIFY that FILE is a DUPLICATE ---  Program has ABENDED * * *";
          

        // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
        // * * *
        // Write the ERROR Report
        // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
        // * * *
        local.ReportHandlingEabReportSend.RptDetail =
          local.AaaGroupLocalErrors.Item.AaaGroupLocalErrorsDetailProgramError.
            KeyInfo ?? Spaces(132);
        local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
        local.ReportHandlingEabFileHandling.Action = "WRITE";
        UseCabErrorReport1();

        if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
        {
        }
        else
        {
          UseEabRollbackSql();
          ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

          return;
        }
      }
    }

Test:

    if (AsChar(local.AbendErrorFound.Flag) == 'Y')
    {
      UseEabRollbackSql();

      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // * *
      // Open the ERROR Report
      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // * *
      if (AsChar(local.ErrorOpened.Flag) != 'Y')
      {
        local.ReportHandlingEabReportSend.ProcessDate =
          local.ProgramProcessingInfo.ProcessDate;
        local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
        local.ReportHandlingEabFileHandling.Action = "OPEN";
        UseCabErrorReport2();

        if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
        {
        }
        else
        {
          ExitState = "SI0000_ERR_OPENING_ERR_RPT_FILE";

          return;
        }

        local.ErrorOpened.Flag = "Y";
      }

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
        // * * *
        // Write the ERROR Report
        // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
        // * * *
        if (IsEmpty(local.AaaGroupLocalErrors.Item.
          AaaGroupLocalErrorsDetailProgramError.KeyInfo))
        {
          local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();
          local.AaaGroupLocalErrors.Update.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo =
              local.ExitStateWorkArea.Message;
        }

        local.ReportHandlingEabReportSend.RptDetail =
          local.AaaGroupLocalErrors.Item.AaaGroupLocalErrorsDetailProgramError.
            KeyInfo ?? Spaces(132);
        local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
        local.ReportHandlingEabFileHandling.Action = "WRITE";
        UseCabErrorReport1();

        if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
        {
        }
        else
        {
          ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

          return;
        }
      }

      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // * *
      // Write the ERROR Report -- Spacing
      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // * *
      local.ReportHandlingEabReportSend.RptDetail = "";
      local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
      local.ReportHandlingEabFileHandling.Action = "WRITE";
      UseCabErrorReport1();

      if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
      {
      }
      else
      {
        ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

        return;
      }
    }
    else
    {
      // *****
      // Set restart indicator to no because we successfully finished this 
      // program.
      // *****
      local.ProgramCheckpointRestart.RestartInfo = "";
      local.ProgramCheckpointRestart.RestartInd = "N";
      local.ProgramCheckpointRestart.CheckpointCount = -1;
      UseUpdatePgmCheckpointRestart();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }

    if (AsChar(local.ErrorOpened.Flag) == 'Y')
    {
      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // * *
      // Close the ERROR Report
      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // * *
      local.ReportHandlingEabReportSend.ProcessDate =
        local.ProgramProcessingInfo.ProcessDate;
      local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
      local.ReportHandlingEabFileHandling.Action = "CLOSE";
      UseCabErrorReport2();

      if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
      {
      }
      else
      {
        ExitState = "SI0000_ERR_CLOSING_ERR_RPT_FILE";

        return;
      }
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Open the Control Report
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ReportHandlingEabReportSend.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
    local.ReportHandlingEabFileHandling.Action = "OPEN";
    UseCabControlReport2();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_OPENING_CTL_RPT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Write the Control Report -- Records processed Header
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ReportHandlingEabReportSend.RptDetail =
      "Total Count of FDSO Records Processed:";
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Write the Control Report -- Spacing
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ReportHandlingEabReportSend.RptDetail = "";
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Write the Control Report -- Total Count of ALL Records Read
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ControlTotals.Index = 0;
    local.ControlTotals.CheckSize();

    local.ReportHandlingEabReportSend.RptDetail =
      "Total Number of FDSO Records Read..........................";
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 50) + NumberToString
      ((long)(local.ControlTotals.Item.ProgramControlTotal.Value.
        GetValueOrDefault() + local.Swefb612AdjustErrCount.Count + local
      .Swefb612AdjustmentCount.Count), 15);
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Write the Control Report -- Table 1 line - Collections Read
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ControlTotals.Index = 0;
    local.ControlTotals.CheckSize();

    local.ReportHandlingEabReportSend.RptDetail =
      "Total Number of Collections Read.............................";
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 50) + NumberToString
      ((long)local.ControlTotals.Item.ProgramControlTotal.Value.
        GetValueOrDefault(), 15);
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Write the Control Report -- Table 2 line - Adjustments Read
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ControlTotals.Index = 1;
    local.ControlTotals.CheckSize();

    local.ReportHandlingEabReportSend.RptDetail =
      "Total Number of Adjustments Read............................";
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 50) + NumberToString
      ((long)local.ControlTotals.Item.ProgramControlTotal.Value.
        GetValueOrDefault(), 15);
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    if (local.Swefb612AdjustErrCount.Count > 0 || local
      .Swefb612AdjErrAmount.TotalCurrency > 0)
    {
      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // * *
      // Write the Control Report --  Adjustments ByPassed
      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // * *
      local.ReportHandlingEabReportSend.RptDetail =
        "Total Number of Adjustments Errored..........................";
      local.ReportHandlingEabReportSend.RptDetail =
        Substring(local.ReportHandlingEabReportSend.RptDetail,
        EabReportSend.RptDetail_MaxLength, 1, 50) + NumberToString
        (local.Swefb612AdjustErrCount.Count, 15);
      local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
      local.ReportHandlingEabFileHandling.Action = "WRITE";
      UseCabControlReport1();

      if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
      {
      }
      else
      {
        ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

        return;
      }
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Write the Control Report -- Spacing
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ReportHandlingEabReportSend.RptDetail = "";
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Write the Control Report -- Records processed Amount Header
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ReportHandlingEabReportSend.RptDetail =
      "Total Amount of FDSO Records Processed:";
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Write the Control Report -- Spacing
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ReportHandlingEabReportSend.RptDetail = "";
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Write the Control Report -- Total of ALL FDSO records Read
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ControlTotals.Index = 0;
    local.ControlTotals.CheckSize();

    local.ReportHandlingEabReportSend.RptDetail =
      "Total Amount of ALL FDSO Records Read..........................";

    // HERE SWSRPDP
    local.TotalOfAll.CollectionAmount =
      local.Swefb612AdjErrAmount.TotalCurrency + local
      .Swefb612AdjustmentAmt.TotalCurrency + local
      .Swefb612CollectionAmt.TotalCurrency;
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 53) + NumberToString
      ((long)local.TotalOfAll.CollectionAmount, 15) + "." + NumberToString
      ((long)(local.TotalOfAll.CollectionAmount * 100), 14, 2);
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 53) + Substring
      (local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 60, 12);
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Write the Control Report -- Table 1 line - Collections Read
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ControlTotals.Index = 0;
    local.ControlTotals.CheckSize();

    local.ReportHandlingEabReportSend.RptDetail =
      "Total Amount of FDSO Collections Read..........................";
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 53) + NumberToString
      ((long)local.Swefb612CollectionAmt.TotalCurrency, 15) + "." + NumberToString
      ((long)(local.Swefb612CollectionAmt.TotalCurrency * 100), 14, 2);
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 53) + Substring
      (local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 60, 12);
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Write the Control Report -- Table 2 line - Adjustments Read
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ControlTotals.Index = 1;
    local.ControlTotals.CheckSize();

    local.ReportHandlingEabReportSend.RptDetail =
      "Total Amount of FDSO Adjustments Applied..........................";
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 53) + NumberToString
      ((long)local.Swefb612AdjustmentAmt.TotalCurrency, 15) + "." + NumberToString
      ((long)(local.Swefb612AdjustmentAmt.TotalCurrency * 100), 14, 2);
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 53) + Substring
      (local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 60, 12);

    // 09/11/99   H00073496  pphinney  Net Amount Problem   FIX here?
    if (local.Swefb612AdjustmentAmt.TotalCurrency > 0)
    {
      local.ReportHandlingEabReportSend.RptDetail =
        TrimEnd(local.ReportHandlingEabReportSend.RptDetail) + "-";
    }

    local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    if (local.Swefb612AdjustErrCount.Count > 0 || local
      .Swefb612AdjErrAmount.TotalCurrency > 0)
    {
      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // * *
      // Write the Control Report --  Adjustments ByPassed
      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // * *
      local.ReportHandlingEabReportSend.RptDetail =
        "Total Amount of FDSO Adjustments Errored..........................";
      local.ReportHandlingEabReportSend.RptDetail =
        Substring(local.ReportHandlingEabReportSend.RptDetail,
        EabReportSend.RptDetail_MaxLength, 1, 53) + NumberToString
        ((long)(local.Swefb612AdjErrAmount.TotalCurrency * 100), 15);
      local.ReportHandlingEabReportSend.RptDetail =
        Substring(local.ReportHandlingEabReportSend.RptDetail,
        EabReportSend.RptDetail_MaxLength, 1, 53) + NumberToString
        ((long)local.Swefb612AdjErrAmount.TotalCurrency, 15) + "." + NumberToString
        ((long)(local.Swefb612AdjErrAmount.TotalCurrency * 100), 14, 2);
      local.ReportHandlingEabReportSend.RptDetail =
        Substring(local.ReportHandlingEabReportSend.RptDetail,
        EabReportSend.RptDetail_MaxLength, 1, 53) + Substring
        (local.ReportHandlingEabReportSend.RptDetail,
        EabReportSend.RptDetail_MaxLength, 60, 12);

      // 09/11/99   H00073496  pphinney  Net Amount Problem   FIX here?
      local.ReportHandlingEabReportSend.RptDetail =
        TrimEnd(local.ReportHandlingEabReportSend.RptDetail) + "-";
      local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
      local.ReportHandlingEabFileHandling.Action = "WRITE";
      UseCabControlReport1();

      if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
      {
      }
      else
      {
        ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

        return;
      }
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Write the Control Report -- NET Amount
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ControlTotals.Index = 1;
    local.ControlTotals.CheckSize();

    local.ReportHandlingEabReportSend.RptDetail =
      "Total NET Amount of FDSO Records..........................................";
      
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 53) + NumberToString
      ((long)local.Swefb612NetAmount.TotalCurrency, 15) + "." + NumberToString
      ((long)(local.Swefb612NetAmount.TotalCurrency * 100), 14, 2);
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 53) + Substring
      (local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 60, 12);

    // 09/11/99   H00073496  pphinney  Net Amount Problem   FIX here?
    if (AsChar(local.NegativeNetFlag.Flag) == 'Y')
    {
      local.ReportHandlingEabReportSend.RptDetail =
        TrimEnd(local.ReportHandlingEabReportSend.RptDetail) + "-";
    }

    local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Write the Control Report -- Spacing
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ReportHandlingEabReportSend.RptDetail = "";
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Write the Control Report -- Records processed Header
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ReportHandlingEabReportSend.RptDetail =
      "Total Count of FDSO Records Recorded as Collections:";
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Write the Control Report -- Spacing
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ReportHandlingEabReportSend.RptDetail = "";
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Write the Control Report -- Total Count of ALL Records Recorded
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ControlTotals.Index = 0;
    local.ControlTotals.CheckSize();

    local.ReportHandlingEabReportSend.RptDetail =
      "Total Number of FDSO Records Recorded.........................................";
      
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 50) + NumberToString
      (local.RecCollections.Count, 15);
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Write the Control Report -- Total Count of Releassed
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ControlTotals.Index = 0;
    local.ControlTotals.CheckSize();

    local.ReportHandlingEabReportSend.RptDetail =
      "Total Number of FDSO Records Released................................";
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 50) + NumberToString
      (local.RelCollections.Count, 15);
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Write the Control Report -- Total Count of Suspended
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ControlTotals.Index = 0;
    local.ControlTotals.CheckSize();

    local.ReportHandlingEabReportSend.RptDetail =
      "Total Number of FDSO Records Suspended..............................";
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 50) + NumberToString
      (local.SusCollections.Count, 15);
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Write the Control Report -- Total Count of Refunded
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ControlTotals.Index = 0;
    local.ControlTotals.CheckSize();

    local.ReportHandlingEabReportSend.RptDetail =
      "Total Number of FDSO Records Refunded.................................";
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 50) + NumberToString
      (local.RefCollections.Count, 15);
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // Pended is NO LONGER USED
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Write the Control Report -- Spacing
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ReportHandlingEabReportSend.RptDetail = "";
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Write the Control Report -- Total Amount of Recorded
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ReportHandlingEabReportSend.RptDetail =
      "Total Amount of FDSO Records Recorded.................................";
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 53) + NumberToString
      ((long)local.RecCollectionsAmt.TotalCurrency, 15) + "." + NumberToString
      ((long)(local.RecCollectionsAmt.TotalCurrency * 100), 14, 2);
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 53) + Substring
      (local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 60, 12);
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Write the Control Report -- Total Amount of Releassed
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ReportHandlingEabReportSend.RptDetail =
      "Total Amount of FDSO Records Released..................................";
      
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 53) + NumberToString
      ((long)local.RelCollectionsAmt.TotalCurrency, 15) + "." + NumberToString
      ((long)(local.RelCollectionsAmt.TotalCurrency * 100), 14, 2);
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 53) + Substring
      (local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 60, 12);
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Write the Control Report -- Total Amount of Suspended
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ReportHandlingEabReportSend.RptDetail =
      "Total Amount of FDSO Records Suspended...............................";
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 53) + NumberToString
      ((long)local.SusCollectionsAmt.TotalCurrency, 15) + "." + NumberToString
      ((long)(local.SusCollectionsAmt.TotalCurrency * 100), 14, 2);
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 53) + Substring
      (local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 60, 12);
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Write the Control Report -- Total Amount of Refunded
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ReportHandlingEabReportSend.RptDetail =
      "Total Amount of FDSO Records Refunded..................................";
      
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 53) + NumberToString
      ((long)local.RefCollectionsAmt.TotalCurrency, 15) + "." + NumberToString
      ((long)(local.RefCollectionsAmt.TotalCurrency * 100), 14, 2);
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 53) + Substring
      (local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 60, 12);
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Write the Control Report -- Spacing
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ReportHandlingEabReportSend.RptDetail = "";
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Write the Control Report -- Records Adjusted Header
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ReportHandlingEabReportSend.RptDetail =
      "Total Count of FDSO Records Recorded as Adjustments:";
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Write the Control Report -- Spacing
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ReportHandlingEabReportSend.RptDetail = "";
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Write the Control Report -- Total Count of Adjustments Refunded
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ControlTotals.Index = 0;
    local.ControlTotals.CheckSize();

    local.ReportHandlingEabReportSend.RptDetail =
      "Total Number of Adjustment Records Refunded........................";

    // HERE SWSRPDP
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 50) + NumberToString
      (local.AdjustmentsRef.Count, 15);
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Write the Control Report -- Total Count of Adjustments Recorded - Curr
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ControlTotals.Index = 0;
    local.ControlTotals.CheckSize();

    local.ReportHandlingEabReportSend.RptDetail =
      "Number of Adjustment Refunded - Current...................................";
      

    // HERE SWSRPDP
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 50) + NumberToString
      (local.AdjustmentsRefCurr.Count, 15);
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Write the Control Report -- Total Count of Adjustments Recorded - Prev
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ControlTotals.Index = 0;
    local.ControlTotals.CheckSize();

    local.ReportHandlingEabReportSend.RptDetail =
      "Number of Adjustment Refunded - Previous................................";
      

    // HERE SWSRPDP
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 50) + NumberToString
      (local.AdjustmentsRefPrev.Count, 15);
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Write the Control Report -- Spacing
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ReportHandlingEabReportSend.RptDetail = "";
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Write the Control Report -- Total Amount of Adjustments Refunded
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ReportHandlingEabReportSend.RptDetail =
      "Total Amount of Adjustment Records Refunded....................";
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 53) + NumberToString
      ((long)local.AdjRefAmt.TotalCurrency, 15) + "." + NumberToString
      ((long)(local.AdjRefAmt.TotalCurrency * 100), 14, 2);
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 53) + Substring
      (local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 60, 12);
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Write the Control Report -- Total Amount of Adjustments Recorded - Curr
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ReportHandlingEabReportSend.RptDetail =
      "Adjustment Records Refunded - Current..........................";
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 53) + NumberToString
      ((long)local.AdjRefAmtCurr.TotalCurrency, 15) + "." + NumberToString
      ((long)(local.AdjRefAmtCurr.TotalCurrency * 100), 14, 2);
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 53) + Substring
      (local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 60, 12);
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Write the Control Report -- Total Amount of Adjustments Recorded - Prev
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ReportHandlingEabReportSend.RptDetail =
      "Adjustment Records Refunded - Previous..........................";
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 53) + NumberToString
      ((long)local.AdjRefAmtPrev.TotalCurrency, 15) + "." + NumberToString
      ((long)(local.AdjRefAmtPrev.TotalCurrency * 100), 14, 2);
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 53) + Substring
      (local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 60, 12);
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Write the Control Report -- Spacing
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ReportHandlingEabReportSend.RptDetail = "";
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Write the Control Report -- Total Count of Adjusted
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ControlTotals.Index = 0;
    local.ControlTotals.CheckSize();

    local.ReportHandlingEabReportSend.RptDetail =
      "Total Number of Adjustment Records Adjusted..........................";

    // HERE SWSRPDP
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 50) + NumberToString
      (local.AdjustmentsAdj.Count, 15);
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Write the Control Report -- Total Count of Adjustments Recorded - Curr
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ControlTotals.Index = 0;
    local.ControlTotals.CheckSize();

    local.ReportHandlingEabReportSend.RptDetail =
      "Number of Adjustments - Current...............................................";
      

    // HERE SWSRPDP
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 50) + NumberToString
      (local.AdjustmentsAdjCurr.Count, 15);
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Write the Control Report -- Total Count of Adjustments Recorded - Prev
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ControlTotals.Index = 0;
    local.ControlTotals.CheckSize();

    local.ReportHandlingEabReportSend.RptDetail =
      "Number of Adjustments - Previous.............................................";
      

    // HERE SWSRPDP
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 50) + NumberToString
      (local.AdjustmentsAdjPrev.Count, 15);
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Write the Control Report -- Spacing
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ReportHandlingEabReportSend.RptDetail = "";
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Write the Control Report -- Total Amount of Adjusted
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ReportHandlingEabReportSend.RptDetail =
      "Total Amount of Adjustment Records Adjusted.....................";
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 53) + NumberToString
      ((long)local.AdjAdjAmt.TotalCurrency, 15) + "." + NumberToString
      ((long)(local.AdjAdjAmt.TotalCurrency * 100), 14, 2);
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 53) + Substring
      (local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 60, 12);
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Write the Control Report -- Total Amount of Adjustments Recorded - Curr
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ReportHandlingEabReportSend.RptDetail =
      "Adjustment Records Adjusted - Current..............................";
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 53) + NumberToString
      ((long)local.AdjAdjAmtCurr.TotalCurrency, 15) + "." + NumberToString
      ((long)(local.AdjAdjAmtCurr.TotalCurrency * 100), 14, 2);
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 53) + Substring
      (local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 60, 12);
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Write the Control Report -- Total Amount of Adjustments Recorded - Prev
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ReportHandlingEabReportSend.RptDetail =
      "Adjustment Records Adjusted - Previous.............................";
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 53) + NumberToString
      ((long)local.AdjAdjAmtPrev.TotalCurrency, 15) + "." + NumberToString
      ((long)(local.AdjAdjAmtPrev.TotalCurrency * 100), 14, 2);
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 53) + Substring
      (local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 60, 12);
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Write the Control Report -- Spacing
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ReportHandlingEabReportSend.RptDetail = "";
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // If an ERROR Occurred -- Display Additional Control Report Message
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    if (AsChar(local.ErrorOpened.Flag) == 'Y')
    {
      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // * *
      // Write the Control Report -- ERROR Message
      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // * *
      local.ReportHandlingEabReportSend.RptDetail =
        "* * * * * *   ERRORS Occurred -- See ERROR Report for Additional Information   * * * * * *";
        
      local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
      local.ReportHandlingEabFileHandling.Action = "WRITE";
      UseCabControlReport1();

      if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
      {
      }
      else
      {
        ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

        return;
      }

      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // * *
      // Write the Control Report -- Spacing
      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // * *
      local.ReportHandlingEabReportSend.RptDetail = "";
      local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
      local.ReportHandlingEabFileHandling.Action = "WRITE";
      UseCabControlReport1();

      if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
      {
      }
      else
      {
        ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

        return;
      }
    }
    else
    {
      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // * *
      // Write the Control Report -- SUCCESSFUL Message
      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // * *
      local.ReportHandlingEabReportSend.RptDetail =
        "* * * * * *   Successful END of JOB   * * * * * *";
      local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
      local.ReportHandlingEabFileHandling.Action = "WRITE";
      UseCabControlReport1();

      if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
      {
      }
      else
      {
        ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

        return;
      }

      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // * *
      // Write the Control Report -- Spacing
      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // * *
      local.ReportHandlingEabReportSend.RptDetail = "";
      local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
      local.ReportHandlingEabFileHandling.Action = "WRITE";
      UseCabControlReport1();

      if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
      {
      }
      else
      {
        ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

        return;
      }
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Close the Control Report
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ReportHandlingEabReportSend.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB612";
    local.ReportHandlingEabFileHandling.Action = "CLOSE";
    UseCabControlReport2();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    if (AsChar(local.AbendErrorFound.Flag) == 'Y')
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
    else
    {
      ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
    }

    // 05/18/99 Added CLOSE per e-mail from Terri Struder
    // "batch jobs that call adabas"  05/15/99  10:50 AM
    local.CloseCsePersonsWorkSet.Ssn = "close";
    UseEabReadCsePersonUsingSsn();

    // Do NOT need to verify on Return
  }

  private static void MoveCashReceipt1(CashReceipt source, CashReceipt target)
  {
    target.TotalCashFeeAmount = source.TotalCashFeeAmount;
    target.TotalNonCashFeeAmount = source.TotalNonCashFeeAmount;
    target.ReceiptAmount = source.ReceiptAmount;
    target.SequentialNumber = source.SequentialNumber;
    target.ReceiptDate = source.ReceiptDate;
    target.CheckType = source.CheckType;
    target.CheckNumber = source.CheckNumber;
    target.CheckDate = source.CheckDate;
    target.ReceivedDate = source.ReceivedDate;
    target.DepositReleaseDate = source.DepositReleaseDate;
    target.ReferenceNumber = source.ReferenceNumber;
    target.Note = source.Note;
    target.PayorOrganization = source.PayorOrganization;
    target.PayorFirstName = source.PayorFirstName;
    target.PayorMiddleName = source.PayorMiddleName;
    target.PayorLastName = source.PayorLastName;
    target.ForwardedToName = source.ForwardedToName;
    target.ForwardedStreet1 = source.ForwardedStreet1;
    target.ForwardedStreet2 = source.ForwardedStreet2;
    target.ForwardedCity = source.ForwardedCity;
    target.ForwardedState = source.ForwardedState;
    target.ForwardedZip5 = source.ForwardedZip5;
    target.ForwardedZip4 = source.ForwardedZip4;
    target.ForwardedZip3 = source.ForwardedZip3;
    target.BalancedTimestamp = source.BalancedTimestamp;
    target.TotalCashTransactionAmount = source.TotalCashTransactionAmount;
    target.TotalNoncashTransactionAmount = source.TotalNoncashTransactionAmount;
    target.TotalCashTransactionCount = source.TotalCashTransactionCount;
    target.TotalNoncashTransactionCount = source.TotalNoncashTransactionCount;
    target.TotalDetailAdjustmentCount = source.TotalDetailAdjustmentCount;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.CashDue = source.CashDue;
    target.CashBalanceAmt = source.CashBalanceAmt;
    target.CashBalanceReason = source.CashBalanceReason;
  }

  private static void MoveCashReceipt2(CashReceipt source, CashReceipt target)
  {
    target.SequentialNumber = source.SequentialNumber;
    target.ReceiptDate = source.ReceiptDate;
    target.ReceivedDate = source.ReceivedDate;
    target.TotalCashTransactionAmount = source.TotalCashTransactionAmount;
    target.TotalCashTransactionCount = source.TotalCashTransactionCount;
    target.TotalDetailAdjustmentCount = source.TotalDetailAdjustmentCount;
    target.CreatedBy = source.CreatedBy;
    target.CashDue = source.CashDue;
  }

  private static void MoveCashReceipt3(CashReceipt source, CashReceipt target)
  {
    target.SequentialNumber = source.SequentialNumber;
    target.ReceiptDate = source.ReceiptDate;
    target.ReceivedDate = source.ReceivedDate;
    target.TotalCashTransactionAmount = source.TotalCashTransactionAmount;
    target.TotalCashTransactionCount = source.TotalCashTransactionCount;
    target.TotalDetailAdjustmentCount = source.TotalDetailAdjustmentCount;
    target.CashDue = source.CashDue;
  }

  private static void MoveCashReceipt4(CashReceipt source, CashReceipt target)
  {
    target.SequentialNumber = source.SequentialNumber;
    target.TotalDetailAdjustmentCount = source.TotalDetailAdjustmentCount;
  }

  private static void MoveCashReceiptDetail1(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.InterfaceTransId = source.InterfaceTransId;
    target.OffsetTaxYear = source.OffsetTaxYear;
  }

  private static void MoveCashReceiptDetail2(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.CollectionAmount = source.CollectionAmount;
  }

  private static void MoveCashReceiptEvent(CashReceiptEvent source,
    CashReceiptEvent target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.SourceCreationDate = source.SourceCreationDate;
  }

  private static void MoveCashReceiptSourceType(CashReceiptSourceType source,
    CashReceiptSourceType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveCollectionRecord1(Local.CollectionRecordGroup source,
    FnProcessFdsoIntAdjustRec.Import.CollectionGroup target)
  {
    target.DetalLocalCode.Text4 = source.DetailLocalCode.Text4;
    target.Detail.Assign(source.Detail);
    target.DetailAdjustmentAmt.TotalCurrency =
      source.DetailAdjustmentAmt.TotalCurrency;
  }

  private static void MoveCollectionRecord2(Local.CollectionRecordGroup source,
    FnProcessFdsoIntCollDtlRec.Import.CollectionRecordGroup target)
  {
    target.DetailLocalCode.Text4 = source.DetailLocalCode.Text4;
    target.Detail.Assign(source.Detail);
    target.DetailAdjAmt.TotalCurrency =
      source.DetailAdjustmentAmt.TotalCurrency;
  }

  private static void MoveCollectionRecord3(Local.CollectionRecordGroup source,
    EabFdsoInterfaceDrvr.Export.CollectionRecordGroup target)
  {
    target.DetailLocalCode.Text4 = source.DetailLocalCode.Text4;
    target.Detail.Assign(source.Detail);
    target.LocalGrpDetailAdjAmount.TotalCurrency =
      source.DetailAdjustmentAmt.TotalCurrency;
  }

  private static void MoveCollectionRecord4(EabFdsoInterfaceDrvr.Export.
    CollectionRecordGroup source, Local.CollectionRecordGroup target)
  {
    target.DetailLocalCode.Text4 = source.DetailLocalCode.Text4;
    target.Detail.Assign(source.Detail);
    target.DetailAdjustmentAmt.TotalCurrency =
      source.LocalGrpDetailAdjAmount.TotalCurrency;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveErrors(FnProcessFdsoIntTotalRecord.Export.
    ErrorsGroup source, Local.AaaGroupLocalErrorsGroup target)
  {
    target.AaaGroupLocalErrorsDetailProgramError.Assign(
      source.ErrorsDetailProgramError);
    target.AaaGroupLocalErrorsDetailStandard.Command =
      source.ErrorsDetailStandard.Command;
  }

  private static void MoveExternal1(External source, External target)
  {
    target.FileInstruction = source.FileInstruction;
    target.TextLine80 = source.TextLine80;
  }

  private static void MoveExternal2(External source, External target)
  {
    target.TextReturnCode = source.TextReturnCode;
    target.TextLine80 = source.TextLine80;
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
    target.ProcessDate = source.ProcessDate;
  }

  private static void MoveTotalRecord1(Local.TotalRecordGroup source,
    FnProcessFdsoIntTotalRecord.Import.TotalRecordGroup target)
  {
    target.DetailAdjAmt.TotalCurrency = source.DetailAdjAmt.TotalCurrency;
    target.DetailAdjCount.TotalAdjustmentCount =
      source.DetailCashReceiptEvent.TotalAdjustmentCount;
    target.Detail.Assign(source.DetailCashReceipt);
    target.NetAmount.TotalCurrency = source.NetAmount.TotalCurrency;
  }

  private static void MoveTotalRecord2(Local.TotalRecordGroup source,
    EabFdsoInterfaceDrvr.Export.TotalRecordGroup target)
  {
    target.DetailTotAdjAmt.TotalCurrency = source.DetailAdjAmt.TotalCurrency;
    target.DetailCashReceiptEvent.TotalAdjustmentCount =
      source.DetailCashReceiptEvent.TotalAdjustmentCount;
    target.DetailCashReceipt.Assign(source.DetailCashReceipt);
    target.DetailNetAmount.TotalCurrency = source.NetAmount.TotalCurrency;
  }

  private static void MoveTotalRecord3(EabFdsoInterfaceDrvr.Export.
    TotalRecordGroup source, Local.TotalRecordGroup target)
  {
    target.DetailAdjAmt.TotalCurrency = source.DetailTotAdjAmt.TotalCurrency;
    target.DetailCashReceiptEvent.TotalAdjustmentCount =
      source.DetailCashReceiptEvent.TotalAdjustmentCount;
    target.DetailCashReceipt.Assign(source.DetailCashReceipt);
    target.NetAmount.TotalCurrency = source.DetailNetAmount.TotalCurrency;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action =
      local.ReportHandlingEabFileHandling.Action;
    useImport.NeededToWrite.RptDetail =
      local.ReportHandlingEabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.ReportHandlingEabFileHandling.Status =
      useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action =
      local.ReportHandlingEabFileHandling.Action;
    MoveEabReportSend(local.ReportHandlingEabReportSend, useImport.NeededToOpen);
      

    Call(CabControlReport.Execute, useImport, useExport);

    local.ReportHandlingEabFileHandling.Status =
      useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action =
      local.ReportHandlingEabFileHandling.Action;
    useImport.NeededToWrite.RptDetail =
      local.ReportHandlingEabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.ReportHandlingEabFileHandling.Status =
      useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action =
      local.ReportHandlingEabFileHandling.Action;
    MoveEabReportSend(local.ReportHandlingEabReportSend, useImport.NeededToOpen);
      

    Call(CabErrorReport.Execute, useImport, useExport);

    local.ReportHandlingEabFileHandling.Status =
      useExport.EabFileHandling.Status;
  }

  private string UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    return useExport.ExitStateWorkArea.Message;
  }

  private void UseEabFdsoInterfaceDrvr1()
  {
    var useImport = new EabFdsoInterfaceDrvr.Import();
    var useExport = new EabFdsoInterfaceDrvr.Export();

    MoveExternal1(local.PassArea, useImport.External);
    MoveExternal2(local.PassArea, useExport.External);

    Call(EabFdsoInterfaceDrvr.Execute, useImport, useExport);

    MoveExternal2(useExport.External, local.PassArea);
  }

  private void UseEabFdsoInterfaceDrvr2()
  {
    var useImport = new EabFdsoInterfaceDrvr.Import();
    var useExport = new EabFdsoInterfaceDrvr.Export();

    MoveExternal1(local.PassArea, useImport.External);
    MoveExternal2(local.PassArea, useExport.External);
    useExport.RecordTypeReturned.Count = local.RecordTypeReturned.Count;
    MoveCollectionRecord3(local.CollectionRecord, useExport.CollectionRecord);
    MoveTotalRecord2(local.TotalRecord, useExport.TotalRecord);
    MoveCashReceiptDetail1(local.AdditionalInfo.Adjustment,
      useExport.AdditionalInfo.GrDetailAdjAmount);
    useExport.AdditionalInfo.DetailAddress.Assign(local.AdditionalInfo.Address);

    Call(EabFdsoInterfaceDrvr.Execute, useImport, useExport);

    MoveExternal2(useExport.External, local.PassArea);
    local.RecordTypeReturned.Count = useExport.RecordTypeReturned.Count;
    MoveCollectionRecord4(useExport.CollectionRecord, local.CollectionRecord);
    MoveTotalRecord3(useExport.TotalRecord, local.TotalRecord);
    MoveCashReceiptDetail1(useExport.AdditionalInfo.GrDetailAdjAmount,
      local.AdditionalInfo.Adjustment);
    local.AdditionalInfo.Address.Assign(useExport.AdditionalInfo.DetailAddress);
  }

  private void UseEabReadCsePersonUsingSsn()
  {
    var useImport = new EabReadCsePersonUsingSsn.Import();
    var useExport = new EabReadCsePersonUsingSsn.Export();

    useImport.CsePersonsWorkSet.Ssn = local.CloseCsePersonsWorkSet.Ssn;
    useExport.CsePersonsWorkSet.Assign(local.Returned);
    useExport.AbendData.Assign(local.CloseAbendData);

    Call(EabReadCsePersonUsingSsn.Execute, useImport, useExport);

    local.Returned.Assign(useExport.CsePersonsWorkSet);
    local.CloseAbendData.Assign(useExport.AbendData);
  }

  private void UseEabRollbackSql()
  {
    var useImport = new EabRollbackSql.Import();
    var useExport = new EabRollbackSql.Export();

    Call(EabRollbackSql.Execute, useImport, useExport);
  }

  private void UseFnAddFdsoSdsoHeaderInfo()
  {
    var useImport = new FnAddFdsoSdsoHeaderInfo.Import();
    var useExport = new FnAddFdsoSdsoHeaderInfo.Export();

    useImport.CashReceiptEvent.SourceCreationDate =
      local.CashReceiptEvent.SourceCreationDate;
    useImport.CashReceiptSourceType.Code = export.CashReceiptSourceType.Code;

    Call(FnAddFdsoSdsoHeaderInfo.Execute, useImport, useExport);

    local.CashReceiptEvent.Assign(useExport.CashReceiptEvent);
    MoveCashReceiptSourceType(useExport.CashReceiptSourceType,
      local.CashReceiptSourceType);
    local.Fdso.Assign(useExport.NonCash);
  }

  private void UseFnFdsoSdsoCheckForDupBatch()
  {
    var useImport = new FnFdsoSdsoCheckForDupBatch.Import();
    var useExport = new FnFdsoSdsoCheckForDupBatch.Export();

    useImport.FindDuplicateDaysCount.Count = local.FindDuplicateDaysCount.Count;
    useImport.CashReceipt.Assign(local.FindDuplicate);

    Call(FnFdsoSdsoCheckForDupBatch.Execute, useImport, useExport);

    local.FindDuplicateCount.Count = useExport.DuplicateCount.Count;
    MoveCashReceipt3(useExport.Found, local.FindDuplicate);
  }

  private void UseFnProcessFdsoIntAdjustRec()
  {
    var useImport = new FnProcessFdsoIntAdjustRec.Import();
    var useExport = new FnProcessFdsoIntAdjustRec.Export();

    useImport.AdjRefAmt.TotalCurrency = local.AdjRefAmt.TotalCurrency;
    useImport.AdjRefAmtPrev.TotalCurrency = local.AdjRefAmtPrev.TotalCurrency;
    useImport.AdjRefAmtCurr.TotalCurrency = local.AdjRefAmtCurr.TotalCurrency;
    useImport.AdjAdjAmt.TotalCurrency = local.AdjAdjAmt.TotalCurrency;
    useImport.AdjAdjAmtPrev.TotalCurrency = local.AdjAdjAmtPrev.TotalCurrency;
    useImport.AdjAdjAmtCurr.TotalCurrency = local.AdjAdjAmtCurr.TotalCurrency;
    useImport.AdjRefCurrCnt.Count = local.AdjustmentsRefCurr.Count;
    useImport.AdjRefPrevCnt.Count = local.AdjustmentsRefPrev.Count;
    useImport.AdjAdjCurrCnt.Count = local.AdjustmentsAdjCurr.Count;
    useImport.AdjAdjPrevCnt.Count = local.AdjustmentsAdjPrev.Count;
    useImport.AdjAdjCnt.Count = local.AdjustmentsAdj.Count;
    useImport.AdjRefCnt.Count = local.AdjustmentsRef.Count;
    useImport.Swefb612NetAmount.TotalCurrency =
      local.Swefb612NetAmount.TotalCurrency;
    useImport.Swefb612AdjustmentAmt.TotalCurrency =
      local.Swefb612AdjustmentAmt.TotalCurrency;
    useImport.Swefb612AdjustCount.Count = local.Swefb612AdjustmentCount.Count;
    MoveCollectionRecord1(local.CollectionRecord, useImport.Collection);
    MoveCashReceiptEvent(local.CashReceiptEvent, useImport.CashReceiptEvent);
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      local.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.AdditionalInformation.Adjustment.OffsetTaxYear =
      local.AdditionalInfo.Adjustment.OffsetTaxYear;
    MoveProgramProcessingInfo(local.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);
    useImport.Save.Date = local.Save.Date;
    useExport.ImportAdjustment.TotalCurrency =
      local.TotalAdjustement.TotalCurrency;
    MoveCashReceiptDetail2(local.CashReceiptDetail,
      useExport.ImportCashReceiptDetail);
    useExport.ImportNet.TotalCurrency = local.Net.TotalCurrency;
    MoveCashReceipt4(local.Fdso, useExport.ImportCashReceipt);
    useExport.ImportNextCheckId.SequentialIdentifier =
      local.NextCheckId.SequentialIdentifier;

    Call(FnProcessFdsoIntAdjustRec.Execute, useImport, useExport);

    local.AvailiableCollections.TotalCurrency =
      useExport.AmtOfCollectionsCalc.TotalCurrency;
    local.AdjRefAmt.TotalCurrency = useExport.AdjRefAmt.TotalCurrency;
    local.AdjRefAmtPrev.TotalCurrency = useExport.AdjRefAmtPrev.TotalCurrency;
    local.AdjRefAmtCurr.TotalCurrency = useExport.AdjRefAmtCurr.TotalCurrency;
    local.AdjAdjAmt.TotalCurrency = useExport.AdjAdjAmt.TotalCurrency;
    local.AdjAdjAmtPrev.TotalCurrency = useExport.AdjAdjAmtPrev.TotalCurrency;
    local.AdjAdjAmtCurr.TotalCurrency = useExport.AdjAdjAmtCurr.TotalCurrency;
    local.AdjustmentsRefPrev.Count = useExport.AdjRefPrevCnt.Count;
    local.AdjustmentsRefCurr.Count = useExport.AdjRefCurrCnt.Count;
    local.AdjustmentsAdjPrev.Count = useExport.AdjAdjPrevCnt.Count;
    local.AdjustmentsAdjCurr.Count = useExport.AdjAdjCurrCnt.Count;
    local.AdjustmentsAdj.Count = useExport.AdjAdjCnt.Count;
    local.AdjustmentsRef.Count = useExport.AdjRefCnt.Count;
    local.NoMatchOnYear.Flag = useExport.NoMatchOnYear.Flag;
    local.Swefb612AdjustmentCount.Count = useExport.Swefb612AdjustCount.Count;
    local.Swefb612AdjustmentAmt.TotalCurrency =
      useExport.Swefb612AdjustmentAmt.TotalCurrency;
    local.Swefb612NetAmount.TotalCurrency =
      useExport.Swefb612NetAmount.TotalCurrency;
    local.TotalAdjustement.TotalCurrency =
      useExport.ImportAdjustment.TotalCurrency;
    MoveCashReceiptDetail2(useExport.ImportCashReceiptDetail,
      local.CashReceiptDetail);
    local.Net.TotalCurrency = useExport.ImportNet.TotalCurrency;
    MoveCashReceipt4(useExport.ImportCashReceipt, local.Fdso);
    local.NextCheckId.SequentialIdentifier =
      useExport.ImportNextCheckId.SequentialIdentifier;
  }

  private void UseFnProcessFdsoIntCollDtlRec()
  {
    var useImport = new FnProcessFdsoIntCollDtlRec.Import();
    var useExport = new FnProcessFdsoIntCollDtlRec.Export();

    useImport.RecCollectionsAmt.TotalCurrency =
      local.RecCollectionsAmt.TotalCurrency;
    useImport.RefCollectionsAmt.TotalCurrency =
      local.RefCollectionsAmt.TotalCurrency;
    useImport.RelCollectionsAmt.TotalCurrency =
      local.RelCollectionsAmt.TotalCurrency;
    useImport.SusCollectionsAmt.TotalCurrency =
      local.SusCollectionsAmt.TotalCurrency;
    useImport.PndCollectionsAmt.TotalCurrency =
      local.PndCollectionsAmt.TotalCurrency;
    useImport.RecCollections.Count = local.RecCollections.Count;
    useImport.RefCollections.Count = local.RefCollections.Count;
    useImport.RelCollections.Count = local.RelCollections.Count;
    useImport.SusCollections.Count = local.SusCollections.Count;
    useImport.PndCollections.Count = local.PndCollections.Count;
    useImport.Swefb612CollectionAmt.TotalCurrency =
      local.Swefb612CollectionAmt.TotalCurrency;
    useImport.Swefb612CollectionCount.Count =
      local.Swefb612CollectionCount.Count;
    useImport.Swefb612NetAmt.TotalCurrency =
      local.Swefb612NetAmount.TotalCurrency;
    useImport.P.Assign(entities.CashReceipt);
    MoveCollectionRecord2(local.CollectionRecord, useImport.CollectionRecord);
    MoveCashReceiptDetail1(local.AdditionalInfo.Adjustment,
      useImport.AdditionalInfo.Adjustment);
    useImport.AdditionalInfo.Address.Assign(local.AdditionalInfo.Address);
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      local.CashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      local.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.Save.Date = local.Save.Date;
    MoveCashReceiptDetail2(local.CashReceiptDetail,
      useExport.ImportCashReceiptDetail);
    useExport.ImportNet.TotalCurrency = local.Net.TotalCurrency;
    useExport.ImportCashReceipt.Assign(local.Fdso);
    useExport.ImportNextCrdId.SequentialIdentifier =
      local.NextCheckId.SequentialIdentifier;

    Call(FnProcessFdsoIntCollDtlRec.Execute, useImport, useExport);

    MoveCashReceipt1(useImport.P, entities.CashReceipt);
    local.RecCollectionsAmt.TotalCurrency =
      useExport.RecCollectionsAmt.TotalCurrency;
    local.RefCollectionsAmt.TotalCurrency =
      useExport.RefCollectionsAmt.TotalCurrency;
    local.RelCollectionsAmt.TotalCurrency =
      useExport.RelCollectionsAmt.TotalCurrency;
    local.SusCollectionsAmt.TotalCurrency =
      useExport.SusCollectionsAmt.TotalCurrency;
    local.PndCollectionsAmt.TotalCurrency =
      useExport.PndCollectionsAmt.TotalCurrency;
    local.RecCollections.Count = useExport.RecCollections.Count;
    local.RefCollections.Count = useExport.RefCollections.Count;
    local.RelCollections.Count = useExport.RelCollections.Count;
    local.SusCollections.Count = useExport.SusCollections.Count;
    local.PndCollections.Count = useExport.PndCollections.Count;
    local.Swefb612NetAmount.TotalCurrency =
      useExport.Swefb612NetAmt.TotalCurrency;
    local.Swefb612CollectionAmt.TotalCurrency =
      useExport.Swefb612CollectionAmt.TotalCurrency;
    local.Swefb612CollectionCount.Count =
      useExport.Swefb612CollectionCount.Count;
    MoveCashReceiptDetail2(useExport.ImportCashReceiptDetail,
      local.CashReceiptDetail);
    local.Net.TotalCurrency = useExport.ImportNet.TotalCurrency;
    local.Fdso.Assign(useExport.ImportCashReceipt);
    local.NextCheckId.SequentialIdentifier =
      useExport.ImportNextCrdId.SequentialIdentifier;
  }

  private void UseFnProcessFdsoIntTotalRecord()
  {
    var useImport = new FnProcessFdsoIntTotalRecord.Import();
    var useExport = new FnProcessFdsoIntTotalRecord.Export();

    useImport.Swefb612AdjError.TotalCurrency =
      local.Swefb612AdjErrAmount.TotalCurrency;
    useImport.Swefb612CollectionCount.Count =
      local.Swefb612CollectionCount.Count;
    useImport.Swefb612AdjustmentCount.Count =
      local.Swefb612AdjustmentCount.Count;
    useImport.Swefb612NetAmt.TotalCurrency =
      local.Swefb612NetAmount.TotalCurrency;
    useImport.Swefb612AdjustmentAmt.TotalCurrency =
      local.Swefb612AdjustmentAmt.TotalCurrency;
    useImport.Swefb612CollectionAmt.TotalCurrency =
      local.Swefb612CollectionAmt.TotalCurrency;
    useImport.AdjustmentCommon.TotalCurrency = local.Adjustment.TotalCurrency;
    MoveTotalRecord1(local.TotalRecord, useImport.TotalRecord);
    useImport.Interfund.Assign(local.Fdso);
    useImport.SourceCreation.TextDate = local.SourceCreation.TextDate;
    MoveCashReceiptSourceType(local.CashReceiptSourceType,
      useImport.CashReceiptSourceType);
    useImport.CashReceiptEvent.Assign(local.CashReceiptEvent);
    MoveCashReceipt2(local.FindDuplicate, useExport.ImportCashReceipt);

    Call(FnProcessFdsoIntTotalRecord.Execute, useImport, useExport);

    useExport.Errors.CopyTo(local.AaaGroupLocalErrors, MoveErrors);
    local.FindDuplicate.Assign(useExport.ImportCashReceipt);
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

  private void UseUpdatePgmCheckpointRestart()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);
  }

  private bool ReadCashReceipt()
  {
    entities.CashReceipt.Populated = false;

    return Read("ReadCashReceipt",
      (db, command) =>
      {
        db.SetInt32(command, "cashReceiptId", local.Fdso.SequentialNumber);
        db.SetInt32(
          command, "crvIdentifier",
          local.CashReceiptEvent.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          local.CashReceiptSourceType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          local.HardcodedEft.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceipt.ReceiptAmount = db.GetDecimal(reader, 3);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 4);
        entities.CashReceipt.ReceiptDate = db.GetDate(reader, 5);
        entities.CashReceipt.CheckType = db.GetNullableString(reader, 6);
        entities.CashReceipt.CheckNumber = db.GetNullableString(reader, 7);
        entities.CashReceipt.CheckDate = db.GetNullableDate(reader, 8);
        entities.CashReceipt.ReceivedDate = db.GetDate(reader, 9);
        entities.CashReceipt.DepositReleaseDate =
          db.GetNullableDate(reader, 10);
        entities.CashReceipt.ReferenceNumber = db.GetNullableString(reader, 11);
        entities.CashReceipt.PayorOrganization =
          db.GetNullableString(reader, 12);
        entities.CashReceipt.PayorFirstName = db.GetNullableString(reader, 13);
        entities.CashReceipt.PayorMiddleName = db.GetNullableString(reader, 14);
        entities.CashReceipt.PayorLastName = db.GetNullableString(reader, 15);
        entities.CashReceipt.ForwardedToName = db.GetNullableString(reader, 16);
        entities.CashReceipt.ForwardedStreet1 =
          db.GetNullableString(reader, 17);
        entities.CashReceipt.ForwardedStreet2 =
          db.GetNullableString(reader, 18);
        entities.CashReceipt.ForwardedCity = db.GetNullableString(reader, 19);
        entities.CashReceipt.ForwardedState = db.GetNullableString(reader, 20);
        entities.CashReceipt.ForwardedZip5 = db.GetNullableString(reader, 21);
        entities.CashReceipt.ForwardedZip4 = db.GetNullableString(reader, 22);
        entities.CashReceipt.ForwardedZip3 = db.GetNullableString(reader, 23);
        entities.CashReceipt.BalancedTimestamp =
          db.GetNullableDateTime(reader, 24);
        entities.CashReceipt.TotalCashTransactionAmount =
          db.GetNullableDecimal(reader, 25);
        entities.CashReceipt.TotalNoncashTransactionAmount =
          db.GetNullableDecimal(reader, 26);
        entities.CashReceipt.TotalCashTransactionCount =
          db.GetNullableInt32(reader, 27);
        entities.CashReceipt.TotalNoncashTransactionCount =
          db.GetNullableInt32(reader, 28);
        entities.CashReceipt.TotalDetailAdjustmentCount =
          db.GetNullableInt32(reader, 29);
        entities.CashReceipt.CreatedBy = db.GetString(reader, 30);
        entities.CashReceipt.CreatedTimestamp = db.GetDateTime(reader, 31);
        entities.CashReceipt.CashBalanceAmt = db.GetNullableDecimal(reader, 32);
        entities.CashReceipt.CashBalanceReason =
          db.GetNullableString(reader, 33);
        entities.CashReceipt.CashDue = db.GetNullableDecimal(reader, 34);
        entities.CashReceipt.TotalNonCashFeeAmount =
          db.GetNullableDecimal(reader, 35);
        entities.CashReceipt.TotalCashFeeAmount =
          db.GetNullableDecimal(reader, 36);
        entities.CashReceipt.Note = db.GetNullableString(reader, 37);
        entities.CashReceipt.Populated = true;
        CheckValid<CashReceipt>("CashBalanceReason",
          entities.CashReceipt.CashBalanceReason);
      });
  }

  private bool ReadProgramProcessingInfo1()
  {
    entities.ProgramProcessingInfo.Populated = false;

    return Read("ReadProgramProcessingInfo",
      (db, command) =>
      {
        db.SetString(command, "name", local.ProgramProcessingInfo.Name);
      },
      (db, reader) =>
      {
        entities.ProgramProcessingInfo.Name = db.GetString(reader, 0);
        entities.ProgramProcessingInfo.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.ProgramProcessingInfo.Populated = true;
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
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
    }

    private CashReceiptSourceType cashReceiptSourceType;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A CollectionRecordGroup group.</summary>
    [Serializable]
    public class CollectionRecordGroup
    {
      /// <summary>
      /// A value of DetailLocalCode.
      /// </summary>
      [JsonPropertyName("detailLocalCode")]
      public TextWorkArea DetailLocalCode
      {
        get => detailLocalCode ??= new();
        set => detailLocalCode = value;
      }

      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public CashReceiptDetail Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>
      /// A value of DetailAdjustmentAmt.
      /// </summary>
      [JsonPropertyName("detailAdjustmentAmt")]
      public Common DetailAdjustmentAmt
      {
        get => detailAdjustmentAmt ??= new();
        set => detailAdjustmentAmt = value;
      }

      private TextWorkArea detailLocalCode;
      private CashReceiptDetail detail;
      private Common detailAdjustmentAmt;
    }

    /// <summary>A TotalRecordGroup group.</summary>
    [Serializable]
    public class TotalRecordGroup
    {
      /// <summary>
      /// A value of DetailAdjAmt.
      /// </summary>
      [JsonPropertyName("detailAdjAmt")]
      public Common DetailAdjAmt
      {
        get => detailAdjAmt ??= new();
        set => detailAdjAmt = value;
      }

      /// <summary>
      /// A value of DetailCashReceiptEvent.
      /// </summary>
      [JsonPropertyName("detailCashReceiptEvent")]
      public CashReceiptEvent DetailCashReceiptEvent
      {
        get => detailCashReceiptEvent ??= new();
        set => detailCashReceiptEvent = value;
      }

      /// <summary>
      /// A value of DetailCashReceipt.
      /// </summary>
      [JsonPropertyName("detailCashReceipt")]
      public CashReceipt DetailCashReceipt
      {
        get => detailCashReceipt ??= new();
        set => detailCashReceipt = value;
      }

      /// <summary>
      /// A value of NetAmount.
      /// </summary>
      [JsonPropertyName("netAmount")]
      public Common NetAmount
      {
        get => netAmount ??= new();
        set => netAmount = value;
      }

      private Common detailAdjAmt;
      private CashReceiptEvent detailCashReceiptEvent;
      private CashReceipt detailCashReceipt;
      private Common netAmount;
    }

    /// <summary>A AdditionalInfoGroup group.</summary>
    [Serializable]
    public class AdditionalInfoGroup
    {
      /// <summary>
      /// A value of Adjustment.
      /// </summary>
      [JsonPropertyName("adjustment")]
      public CashReceiptDetail Adjustment
      {
        get => adjustment ??= new();
        set => adjustment = value;
      }

      /// <summary>
      /// A value of Address.
      /// </summary>
      [JsonPropertyName("address")]
      public CashReceiptDetailAddress Address
      {
        get => address ??= new();
        set => address = value;
      }

      private CashReceiptDetail adjustment;
      private CashReceiptDetailAddress address;
    }

    /// <summary>A ControlTotalsGroup group.</summary>
    [Serializable]
    public class ControlTotalsGroup
    {
      /// <summary>
      /// A value of ProgramControlTotal.
      /// </summary>
      [JsonPropertyName("programControlTotal")]
      public ProgramControlTotal ProgramControlTotal
      {
        get => programControlTotal ??= new();
        set => programControlTotal = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private ProgramControlTotal programControlTotal;
    }

    /// <summary>A AaaGroupLocalErrorsGroup group.</summary>
    [Serializable]
    public class AaaGroupLocalErrorsGroup
    {
      /// <summary>
      /// A value of AaaGroupLocalErrorsDetailProgramError.
      /// </summary>
      [JsonPropertyName("aaaGroupLocalErrorsDetailProgramError")]
      public ProgramError AaaGroupLocalErrorsDetailProgramError
      {
        get => aaaGroupLocalErrorsDetailProgramError ??= new();
        set => aaaGroupLocalErrorsDetailProgramError = value;
      }

      /// <summary>
      /// A value of AaaGroupLocalErrorsDetailStandard.
      /// </summary>
      [JsonPropertyName("aaaGroupLocalErrorsDetailStandard")]
      public Standard AaaGroupLocalErrorsDetailStandard
      {
        get => aaaGroupLocalErrorsDetailStandard ??= new();
        set => aaaGroupLocalErrorsDetailStandard = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1;

      private ProgramError aaaGroupLocalErrorsDetailProgramError;
      private Standard aaaGroupLocalErrorsDetailStandard;
    }

    /// <summary>
    /// A value of NumberOfLines.
    /// </summary>
    [JsonPropertyName("numberOfLines")]
    public Common NumberOfLines
    {
      get => numberOfLines ??= new();
      set => numberOfLines = value;
    }

    /// <summary>
    /// A value of FindDuplicateDaysCount.
    /// </summary>
    [JsonPropertyName("findDuplicateDaysCount")]
    public Common FindDuplicateDaysCount
    {
      get => findDuplicateDaysCount ??= new();
      set => findDuplicateDaysCount = value;
    }

    /// <summary>
    /// A value of FindDuplicateCount.
    /// </summary>
    [JsonPropertyName("findDuplicateCount")]
    public Common FindDuplicateCount
    {
      get => findDuplicateCount ??= new();
      set => findDuplicateCount = value;
    }

    /// <summary>
    /// A value of FindDuplicate.
    /// </summary>
    [JsonPropertyName("findDuplicate")]
    public CashReceipt FindDuplicate
    {
      get => findDuplicate ??= new();
      set => findDuplicate = value;
    }

    /// <summary>
    /// A value of NegativeNetFlag.
    /// </summary>
    [JsonPropertyName("negativeNetFlag")]
    public Common NegativeNetFlag
    {
      get => negativeNetFlag ??= new();
      set => negativeNetFlag = value;
    }

    /// <summary>
    /// A value of Save.
    /// </summary>
    [JsonPropertyName("save")]
    public DateWorkArea Save
    {
      get => save ??= new();
      set => save = value;
    }

    /// <summary>
    /// A value of Returned.
    /// </summary>
    [JsonPropertyName("returned")]
    public CsePersonsWorkSet Returned
    {
      get => returned ??= new();
      set => returned = value;
    }

    /// <summary>
    /// A value of CloseAbendData.
    /// </summary>
    [JsonPropertyName("closeAbendData")]
    public AbendData CloseAbendData
    {
      get => closeAbendData ??= new();
      set => closeAbendData = value;
    }

    /// <summary>
    /// A value of CloseCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("closeCsePersonsWorkSet")]
    public CsePersonsWorkSet CloseCsePersonsWorkSet
    {
      get => closeCsePersonsWorkSet ??= new();
      set => closeCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of CalculateDifference.
    /// </summary>
    [JsonPropertyName("calculateDifference")]
    public Common CalculateDifference
    {
      get => calculateDifference ??= new();
      set => calculateDifference = value;
    }

    /// <summary>
    /// A value of AvailiableCollections.
    /// </summary>
    [JsonPropertyName("availiableCollections")]
    public Common AvailiableCollections
    {
      get => availiableCollections ??= new();
      set => availiableCollections = value;
    }

    /// <summary>
    /// A value of AdjRefAmt.
    /// </summary>
    [JsonPropertyName("adjRefAmt")]
    public Common AdjRefAmt
    {
      get => adjRefAmt ??= new();
      set => adjRefAmt = value;
    }

    /// <summary>
    /// A value of AdjRefAmtPrev.
    /// </summary>
    [JsonPropertyName("adjRefAmtPrev")]
    public Common AdjRefAmtPrev
    {
      get => adjRefAmtPrev ??= new();
      set => adjRefAmtPrev = value;
    }

    /// <summary>
    /// A value of AdjRefAmtCurr.
    /// </summary>
    [JsonPropertyName("adjRefAmtCurr")]
    public Common AdjRefAmtCurr
    {
      get => adjRefAmtCurr ??= new();
      set => adjRefAmtCurr = value;
    }

    /// <summary>
    /// A value of AdjAdjAmt.
    /// </summary>
    [JsonPropertyName("adjAdjAmt")]
    public Common AdjAdjAmt
    {
      get => adjAdjAmt ??= new();
      set => adjAdjAmt = value;
    }

    /// <summary>
    /// A value of AdjAdjAmtPrev.
    /// </summary>
    [JsonPropertyName("adjAdjAmtPrev")]
    public Common AdjAdjAmtPrev
    {
      get => adjAdjAmtPrev ??= new();
      set => adjAdjAmtPrev = value;
    }

    /// <summary>
    /// A value of AdjAdjAmtCurr.
    /// </summary>
    [JsonPropertyName("adjAdjAmtCurr")]
    public Common AdjAdjAmtCurr
    {
      get => adjAdjAmtCurr ??= new();
      set => adjAdjAmtCurr = value;
    }

    /// <summary>
    /// A value of PndCollectionsAmt.
    /// </summary>
    [JsonPropertyName("pndCollectionsAmt")]
    public Common PndCollectionsAmt
    {
      get => pndCollectionsAmt ??= new();
      set => pndCollectionsAmt = value;
    }

    /// <summary>
    /// A value of SusCollectionsAmt.
    /// </summary>
    [JsonPropertyName("susCollectionsAmt")]
    public Common SusCollectionsAmt
    {
      get => susCollectionsAmt ??= new();
      set => susCollectionsAmt = value;
    }

    /// <summary>
    /// A value of RelCollectionsAmt.
    /// </summary>
    [JsonPropertyName("relCollectionsAmt")]
    public Common RelCollectionsAmt
    {
      get => relCollectionsAmt ??= new();
      set => relCollectionsAmt = value;
    }

    /// <summary>
    /// A value of RefCollectionsAmt.
    /// </summary>
    [JsonPropertyName("refCollectionsAmt")]
    public Common RefCollectionsAmt
    {
      get => refCollectionsAmt ??= new();
      set => refCollectionsAmt = value;
    }

    /// <summary>
    /// A value of RecCollectionsAmt.
    /// </summary>
    [JsonPropertyName("recCollectionsAmt")]
    public Common RecCollectionsAmt
    {
      get => recCollectionsAmt ??= new();
      set => recCollectionsAmt = value;
    }

    /// <summary>
    /// A value of AdjustmentsRef.
    /// </summary>
    [JsonPropertyName("adjustmentsRef")]
    public Common AdjustmentsRef
    {
      get => adjustmentsRef ??= new();
      set => adjustmentsRef = value;
    }

    /// <summary>
    /// A value of AdjustmentsRefCurr.
    /// </summary>
    [JsonPropertyName("adjustmentsRefCurr")]
    public Common AdjustmentsRefCurr
    {
      get => adjustmentsRefCurr ??= new();
      set => adjustmentsRefCurr = value;
    }

    /// <summary>
    /// A value of AdjustmentsRefPrev.
    /// </summary>
    [JsonPropertyName("adjustmentsRefPrev")]
    public Common AdjustmentsRefPrev
    {
      get => adjustmentsRefPrev ??= new();
      set => adjustmentsRefPrev = value;
    }

    /// <summary>
    /// A value of AdjustmentsAdj.
    /// </summary>
    [JsonPropertyName("adjustmentsAdj")]
    public Common AdjustmentsAdj
    {
      get => adjustmentsAdj ??= new();
      set => adjustmentsAdj = value;
    }

    /// <summary>
    /// A value of AdjustmentsAdjCurr.
    /// </summary>
    [JsonPropertyName("adjustmentsAdjCurr")]
    public Common AdjustmentsAdjCurr
    {
      get => adjustmentsAdjCurr ??= new();
      set => adjustmentsAdjCurr = value;
    }

    /// <summary>
    /// A value of AdjustmentsAdjPrev.
    /// </summary>
    [JsonPropertyName("adjustmentsAdjPrev")]
    public Common AdjustmentsAdjPrev
    {
      get => adjustmentsAdjPrev ??= new();
      set => adjustmentsAdjPrev = value;
    }

    /// <summary>
    /// A value of PndCollections.
    /// </summary>
    [JsonPropertyName("pndCollections")]
    public Common PndCollections
    {
      get => pndCollections ??= new();
      set => pndCollections = value;
    }

    /// <summary>
    /// A value of SusCollections.
    /// </summary>
    [JsonPropertyName("susCollections")]
    public Common SusCollections
    {
      get => susCollections ??= new();
      set => susCollections = value;
    }

    /// <summary>
    /// A value of RelCollections.
    /// </summary>
    [JsonPropertyName("relCollections")]
    public Common RelCollections
    {
      get => relCollections ??= new();
      set => relCollections = value;
    }

    /// <summary>
    /// A value of RefCollections.
    /// </summary>
    [JsonPropertyName("refCollections")]
    public Common RefCollections
    {
      get => refCollections ??= new();
      set => refCollections = value;
    }

    /// <summary>
    /// A value of RecCollections.
    /// </summary>
    [JsonPropertyName("recCollections")]
    public Common RecCollections
    {
      get => recCollections ??= new();
      set => recCollections = value;
    }

    /// <summary>
    /// A value of TotalOfAll.
    /// </summary>
    [JsonPropertyName("totalOfAll")]
    public CashReceiptDetail TotalOfAll
    {
      get => totalOfAll ??= new();
      set => totalOfAll = value;
    }

    /// <summary>
    /// A value of NoMatchOnYear.
    /// </summary>
    [JsonPropertyName("noMatchOnYear")]
    public Common NoMatchOnYear
    {
      get => noMatchOnYear ??= new();
      set => noMatchOnYear = value;
    }

    /// <summary>
    /// A value of JobRestarted.
    /// </summary>
    [JsonPropertyName("jobRestarted")]
    public Common JobRestarted
    {
      get => jobRestarted ??= new();
      set => jobRestarted = value;
    }

    /// <summary>
    /// A value of Total.
    /// </summary>
    [JsonPropertyName("total")]
    public DateWorkArea Total
    {
      get => total ??= new();
      set => total = value;
    }

    /// <summary>
    /// A value of End.
    /// </summary>
    [JsonPropertyName("end")]
    public DateWorkArea End
    {
      get => end ??= new();
      set => end = value;
    }

    /// <summary>
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    public DateWorkArea Start
    {
      get => start ??= new();
      set => start = value;
    }

    /// <summary>
    /// A value of ErrorOpened.
    /// </summary>
    [JsonPropertyName("errorOpened")]
    public Common ErrorOpened
    {
      get => errorOpened ??= new();
      set => errorOpened = value;
    }

    /// <summary>
    /// A value of ReportHandlingEabFileHandling.
    /// </summary>
    [JsonPropertyName("reportHandlingEabFileHandling")]
    public EabFileHandling ReportHandlingEabFileHandling
    {
      get => reportHandlingEabFileHandling ??= new();
      set => reportHandlingEabFileHandling = value;
    }

    /// <summary>
    /// A value of ReportHandlingEabReportSend.
    /// </summary>
    [JsonPropertyName("reportHandlingEabReportSend")]
    public EabReportSend ReportHandlingEabReportSend
    {
      get => reportHandlingEabReportSend ??= new();
      set => reportHandlingEabReportSend = value;
    }

    /// <summary>
    /// A value of AbendErrorFound.
    /// </summary>
    [JsonPropertyName("abendErrorFound")]
    public Common AbendErrorFound
    {
      get => abendErrorFound ??= new();
      set => abendErrorFound = value;
    }

    /// <summary>
    /// A value of FirstTime.
    /// </summary>
    [JsonPropertyName("firstTime")]
    public Common FirstTime
    {
      get => firstTime ??= new();
      set => firstTime = value;
    }

    /// <summary>
    /// A value of ControlCount.
    /// </summary>
    [JsonPropertyName("controlCount")]
    public Common ControlCount
    {
      get => controlCount ??= new();
      set => controlCount = value;
    }

    /// <summary>
    /// A value of Swefb612AdjustmentCount.
    /// </summary>
    [JsonPropertyName("swefb612AdjustmentCount")]
    public Common Swefb612AdjustmentCount
    {
      get => swefb612AdjustmentCount ??= new();
      set => swefb612AdjustmentCount = value;
    }

    /// <summary>
    /// A value of Swefb612AdjustErrCount.
    /// </summary>
    [JsonPropertyName("swefb612AdjustErrCount")]
    public Common Swefb612AdjustErrCount
    {
      get => swefb612AdjustErrCount ??= new();
      set => swefb612AdjustErrCount = value;
    }

    /// <summary>
    /// A value of Swefb612CollectionCount.
    /// </summary>
    [JsonPropertyName("swefb612CollectionCount")]
    public Common Swefb612CollectionCount
    {
      get => swefb612CollectionCount ??= new();
      set => swefb612CollectionCount = value;
    }

    /// <summary>
    /// A value of Swefb612AdjustmentAmt.
    /// </summary>
    [JsonPropertyName("swefb612AdjustmentAmt")]
    public Common Swefb612AdjustmentAmt
    {
      get => swefb612AdjustmentAmt ??= new();
      set => swefb612AdjustmentAmt = value;
    }

    /// <summary>
    /// A value of Swefb612AdjErrAmount.
    /// </summary>
    [JsonPropertyName("swefb612AdjErrAmount")]
    public Common Swefb612AdjErrAmount
    {
      get => swefb612AdjErrAmount ??= new();
      set => swefb612AdjErrAmount = value;
    }

    /// <summary>
    /// A value of Swefb612CollectionAmt.
    /// </summary>
    [JsonPropertyName("swefb612CollectionAmt")]
    public Common Swefb612CollectionAmt
    {
      get => swefb612CollectionAmt ??= new();
      set => swefb612CollectionAmt = value;
    }

    /// <summary>
    /// A value of Swefb612NetAmount.
    /// </summary>
    [JsonPropertyName("swefb612NetAmount")]
    public Common Swefb612NetAmount
    {
      get => swefb612NetAmount ??= new();
      set => swefb612NetAmount = value;
    }

    /// <summary>
    /// A value of RestartRecordNumber.
    /// </summary>
    [JsonPropertyName("restartRecordNumber")]
    public Common RestartRecordNumber
    {
      get => restartRecordNumber ??= new();
      set => restartRecordNumber = value;
    }

    /// <summary>
    /// A value of ReadSinceLastCommit.
    /// </summary>
    [JsonPropertyName("readSinceLastCommit")]
    public Common ReadSinceLastCommit
    {
      get => readSinceLastCommit ??= new();
      set => readSinceLastCommit = value;
    }

    /// <summary>
    /// A value of AdjAmt.
    /// </summary>
    [JsonPropertyName("adjAmt")]
    public Common AdjAmt
    {
      get => adjAmt ??= new();
      set => adjAmt = value;
    }

    /// <summary>
    /// A value of NextCheckId.
    /// </summary>
    [JsonPropertyName("nextCheckId")]
    public CashReceiptDetail NextCheckId
    {
      get => nextCheckId ??= new();
      set => nextCheckId = value;
    }

    /// <summary>
    /// A value of TotalAdjustement.
    /// </summary>
    [JsonPropertyName("totalAdjustement")]
    public Common TotalAdjustement
    {
      get => totalAdjustement ??= new();
      set => totalAdjustement = value;
    }

    /// <summary>
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    /// <summary>
    /// A value of Interfund.
    /// </summary>
    [JsonPropertyName("interfund")]
    public CashReceiptType Interfund
    {
      get => interfund ??= new();
      set => interfund = value;
    }

    /// <summary>
    /// A value of Net.
    /// </summary>
    [JsonPropertyName("net")]
    public Common Net
    {
      get => net ??= new();
      set => net = value;
    }

    /// <summary>
    /// A value of SkipToErrorProcessing.
    /// </summary>
    [JsonPropertyName("skipToErrorProcessing")]
    public Common SkipToErrorProcessing
    {
      get => skipToErrorProcessing ??= new();
      set => skipToErrorProcessing = value;
    }

    /// <summary>
    /// A value of Adjustment.
    /// </summary>
    [JsonPropertyName("adjustment")]
    public Common Adjustment
    {
      get => adjustment ??= new();
      set => adjustment = value;
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
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public ProgramError Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
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
    /// A value of ProgramRun.
    /// </summary>
    [JsonPropertyName("programRun")]
    public ProgramRun ProgramRun
    {
      get => programRun ??= new();
      set => programRun = value;
    }

    /// <summary>
    /// A value of CashReceiptEvent.
    /// </summary>
    [JsonPropertyName("cashReceiptEvent")]
    public CashReceiptEvent CashReceiptEvent
    {
      get => cashReceiptEvent ??= new();
      set => cashReceiptEvent = value;
    }

    /// <summary>
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of Fdso.
    /// </summary>
    [JsonPropertyName("fdso")]
    public CashReceipt Fdso
    {
      get => fdso ??= new();
      set => fdso = value;
    }

    /// <summary>
    /// A value of NextId.
    /// </summary>
    [JsonPropertyName("nextId")]
    public ProgramError NextId
    {
      get => nextId ??= new();
      set => nextId = value;
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
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
    }

    /// <summary>
    /// A value of SourceCreation.
    /// </summary>
    [JsonPropertyName("sourceCreation")]
    public DateWorkArea SourceCreation
    {
      get => sourceCreation ??= new();
      set => sourceCreation = value;
    }

    /// <summary>
    /// A value of RecordTypeReturned.
    /// </summary>
    [JsonPropertyName("recordTypeReturned")]
    public Common RecordTypeReturned
    {
      get => recordTypeReturned ??= new();
      set => recordTypeReturned = value;
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
    /// A value of HardcodeRead.
    /// </summary>
    [JsonPropertyName("hardcodeRead")]
    public External HardcodeRead
    {
      get => hardcodeRead ??= new();
      set => hardcodeRead = value;
    }

    /// <summary>
    /// A value of HardcodePosition.
    /// </summary>
    [JsonPropertyName("hardcodePosition")]
    public External HardcodePosition
    {
      get => hardcodePosition ??= new();
      set => hardcodePosition = value;
    }

    /// <summary>
    /// Gets a value of CollectionRecord.
    /// </summary>
    [JsonPropertyName("collectionRecord")]
    public CollectionRecordGroup CollectionRecord
    {
      get => collectionRecord ?? (collectionRecord = new());
      set => collectionRecord = value;
    }

    /// <summary>
    /// Gets a value of TotalRecord.
    /// </summary>
    [JsonPropertyName("totalRecord")]
    public TotalRecordGroup TotalRecord
    {
      get => totalRecord ?? (totalRecord = new());
      set => totalRecord = value;
    }

    /// <summary>
    /// Gets a value of AdditionalInfo.
    /// </summary>
    [JsonPropertyName("additionalInfo")]
    public AdditionalInfoGroup AdditionalInfo
    {
      get => additionalInfo ?? (additionalInfo = new());
      set => additionalInfo = value;
    }

    /// <summary>
    /// Gets a value of ControlTotals.
    /// </summary>
    [JsonIgnore]
    public Array<ControlTotalsGroup> ControlTotals => controlTotals ??= new(
      ControlTotalsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ControlTotals for json serialization.
    /// </summary>
    [JsonPropertyName("controlTotals")]
    [Computed]
    public IList<ControlTotalsGroup> ControlTotals_Json
    {
      get => controlTotals;
      set => ControlTotals.Assign(value);
    }

    /// <summary>
    /// Gets a value of AaaGroupLocalErrors.
    /// </summary>
    [JsonIgnore]
    public Array<AaaGroupLocalErrorsGroup> AaaGroupLocalErrors =>
      aaaGroupLocalErrors ??= new(AaaGroupLocalErrorsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of AaaGroupLocalErrors for json serialization.
    /// </summary>
    [JsonPropertyName("aaaGroupLocalErrors")]
    [Computed]
    public IList<AaaGroupLocalErrorsGroup> AaaGroupLocalErrors_Json
    {
      get => aaaGroupLocalErrors;
      set => AaaGroupLocalErrors.Assign(value);
    }

    /// <summary>
    /// A value of HardcodedEft.
    /// </summary>
    [JsonPropertyName("hardcodedEft")]
    public CashReceiptType HardcodedEft
    {
      get => hardcodedEft ??= new();
      set => hardcodedEft = value;
    }

    private Common numberOfLines;
    private Common findDuplicateDaysCount;
    private Common findDuplicateCount;
    private CashReceipt findDuplicate;
    private Common negativeNetFlag;
    private DateWorkArea save;
    private CsePersonsWorkSet returned;
    private AbendData closeAbendData;
    private CsePersonsWorkSet closeCsePersonsWorkSet;
    private Common calculateDifference;
    private Common availiableCollections;
    private Common adjRefAmt;
    private Common adjRefAmtPrev;
    private Common adjRefAmtCurr;
    private Common adjAdjAmt;
    private Common adjAdjAmtPrev;
    private Common adjAdjAmtCurr;
    private Common pndCollectionsAmt;
    private Common susCollectionsAmt;
    private Common relCollectionsAmt;
    private Common refCollectionsAmt;
    private Common recCollectionsAmt;
    private Common adjustmentsRef;
    private Common adjustmentsRefCurr;
    private Common adjustmentsRefPrev;
    private Common adjustmentsAdj;
    private Common adjustmentsAdjCurr;
    private Common adjustmentsAdjPrev;
    private Common pndCollections;
    private Common susCollections;
    private Common relCollections;
    private Common refCollections;
    private Common recCollections;
    private CashReceiptDetail totalOfAll;
    private Common noMatchOnYear;
    private Common jobRestarted;
    private DateWorkArea total;
    private DateWorkArea end;
    private DateWorkArea start;
    private Common errorOpened;
    private EabFileHandling reportHandlingEabFileHandling;
    private EabReportSend reportHandlingEabReportSend;
    private Common abendErrorFound;
    private Common firstTime;
    private Common controlCount;
    private Common swefb612AdjustmentCount;
    private Common swefb612AdjustErrCount;
    private Common swefb612CollectionCount;
    private Common swefb612AdjustmentAmt;
    private Common swefb612AdjErrAmount;
    private Common swefb612CollectionAmt;
    private Common swefb612NetAmount;
    private Common restartRecordNumber;
    private Common readSinceLastCommit;
    private Common adjAmt;
    private CashReceiptDetail nextCheckId;
    private Common totalAdjustement;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptType interfund;
    private Common net;
    private Common skipToErrorProcessing;
    private Common adjustment;
    private Common common;
    private ProgramError initialized;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramRun programRun;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceipt fdso;
    private ProgramError nextId;
    private External passArea;
    private ExitStateWorkArea exitStateWorkArea;
    private DateWorkArea sourceCreation;
    private Common recordTypeReturned;
    private External hardcodeOpen;
    private External hardcodeRead;
    private External hardcodePosition;
    private CollectionRecordGroup collectionRecord;
    private TotalRecordGroup totalRecord;
    private AdditionalInfoGroup additionalInfo;
    private Array<ControlTotalsGroup> controlTotals;
    private Array<AaaGroupLocalErrorsGroup> aaaGroupLocalErrors;
    private CashReceiptType hardcodedEft;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
    }

    /// <summary>
    /// A value of CashReceiptEvent.
    /// </summary>
    [JsonPropertyName("cashReceiptEvent")]
    public CashReceiptEvent CashReceiptEvent
    {
      get => cashReceiptEvent ??= new();
      set => cashReceiptEvent = value;
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
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
    }

    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptType cashReceiptType;
    private CashReceiptEvent cashReceiptEvent;
    private ProgramRun programRun;
    private ProgramProcessingInfo programProcessingInfo;
    private CashReceipt cashReceipt;
  }
#endregion
}
