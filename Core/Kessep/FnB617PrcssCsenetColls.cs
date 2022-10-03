// Program: FN_B617_PRCSS_CSENET_COLLS, ID: 372623607, model: 746.
// Short name: SWEF617B
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
/// A program: FN_B617_PRCSS_CSENET_COLLS.
/// </para>
/// <para>
/// This skeleton is an example that uses:
/// A sequential driver file
/// An external to do a DB2 commit
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB617PrcssCsenetColls: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B617_PRCSS_CSENET_COLLS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB617PrcssCsenetColls(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB617PrcssCsenetColls.
  /// </summary>
  public FnB617PrcssCsenetColls(IContext context, Import import, Export export):
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
    // ***************************************************************
    // Date      Developers Name     Request #  Description
    // ********  ******************  *********  *********************
    // 09/12/96  Holly Kennedy - MTW                Initial
    // 03/05/99  Paul Phinney                       Add changes for
    // current standards
    // COMPLETE ReWrite
    // to make Program work.
    // 08/24/00  Paul Phinney         H00101885     Change logic to CAUSE ABEND
    // when ERROR condition occurs.
    // 07/05/01  Paul Phinney         I00122679     Change Sort Order
    // TO: Collection_Type, Source_Type
    // 01/07/09  Joyce Harden        CQ 7809       Close ADABAS
    // ***************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    // *****
    // Get the run parameters for this program.
    // *****
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
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
      // *****  Reset the checkpoint restart commit count to zero.
      local.ProgramCheckpointRestart.CheckpointCount = 0;

      if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'N')
      {
        if (ReadCashReceiptCashReceiptEvent2())
        {
          local.BatchAlreadyProcessed.Flag = "Y";
          ExitState = "INTERFACE_ALREADY_PROCESSED_RB";

          goto Test1;
        }
      }

      local.BatchAlreadyProcessed.Flag = "N";
    }
    else
    {
      return;
    }

Test1:

    // *****
    // Process Interstate Collection records until no more exists.  the process 
    // will involve adding a Cash Receipt Event and a Cash Receipt for each
    // source type.  Each Interstate Collection that has null Date of processing
    // will be processed.  If an error occurs populate the error table, and
    // commit the Collections that have been successfully added.
    // *****
    // *****
    // Set the hardcoded values for the Interstate Collection
    // *****
    // Comes in with a "I" which is converted to a "F" when loaded into the 
    // Interstate_Collection File
    // * "8" is the IDentifier for CSENET
    local.HardcodedCsenet.SystemGeneratedIdentifier = 8;

    // (3) is FEDeral Offset -- is this CORRECT?
    local.SetCollectionType.SequentialIdentifier = 3;

    // *****
    // Re-Initialize the error group view.  Set all of the attributes to spaces 
    // in case the new error does not overwrite the previous data.  Execute a "
    // Repeat Targeting" to reset the cardinality to zero.  If returning from an
    // error repopulate the error group
    // *****
    for(local.AaaGroupLocalErrors.Index = 0; local.AaaGroupLocalErrors.Index < local
      .AaaGroupLocalErrors.Count; ++local.AaaGroupLocalErrors.Index)
    {
      if (!local.AaaGroupLocalErrors.CheckSize())
      {
        break;
      }

      local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
        Assign(local.Initialized);
      local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailStandard.
        Command = "";
    }

    local.AaaGroupLocalErrors.CheckIndex();

    // *****
    // Prepare control total group to record number of errors
    // *****
    local.ControlTotals.Index = 0;
    local.ControlTotals.Clear();

    do
    {
      if (local.ControlTotals.IsFull)
      {
        break;
      }

      ++local.Count.Count;
      local.ControlTotals.Update.ProgramControlTotal.SystemGeneratedIdentifier =
        local.Count.Count;
      local.ControlTotals.Update.ProgramControlTotal.Value = 0;
      local.ControlTotals.Update.Common.TotalCurrency = 0;

      switch(local.ControlTotals.Item.ProgramControlTotal.
        SystemGeneratedIdentifier)
      {
        case 1:
          local.ControlTotals.Update.ProgramControlTotal.Name =
            "Number of Collections processed by SWEFB617.................";

          break;
        case 2:
          // This count is no longer used - left in ony to hold place for 
          // accumulators
          local.ControlTotals.Update.ProgramControlTotal.Name =
            "Number of Cash Receipt Details processed....................";

          break;
        case 3:
          local.ControlTotals.Update.ProgramControlTotal.Name =
            "Number of Cash Receipt Details Suspended....................";

          break;
        case 4:
          local.ControlTotals.Update.ProgramControlTotal.Name =
            "Number of Cash Receipt Details Released.....................";

          break;
        case 5:
          local.ControlTotals.Update.ProgramControlTotal.Name =
            "Number of Adjustments processed.............................";

          break;
        case 6:
          local.ControlTotals.Update.ProgramControlTotal.Name =
            "Number of FDSO Cash Receipt Details processed ..............";

          break;
        case 7:
          local.ControlTotals.Update.ProgramControlTotal.Name =
            "Number of SDSO Cash Receipt Details processed ..............";

          break;
        case 8:
          local.ControlTotals.Update.ProgramControlTotal.Name =
            "Number of MISC Cash Receipt Details processed ..............";

          break;
        case 9:
          local.ControlTotals.Update.ProgramControlTotal.Name =
            "Number of errors during processing of SWEFB617..............";

          break;
        case 10:
          local.ControlTotals.Update.ProgramControlTotal.
            SystemGeneratedIdentifier = 0;
          local.ControlTotals.Next();

          goto AfterCycle1;
        default:
          local.ControlTotals.Update.ProgramControlTotal.
            SystemGeneratedIdentifier = 0;
          local.ControlTotals.Next();

          goto AfterCycle1;
      }

      local.ControlTotals.Next();
    }
    while(!Equal(global.Command, "END"));

AfterCycle1:

    // *****
    // Add Events, Receipts, and Details for all Interstate Collections.
    // *****
    local.AdjustmentsFound.Flag = "N";
    local.AaaGroupLocalErrors.Index = -1;

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // *      START of NEW logic
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // Check the SPECIFIC requirements for the INTERSTATE_COLLECTION
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // * * NOT Previously Processed - Date of Posting = Initial Date
    // * * Amount NOT EQUAL to ZERO - collection Amount > 0
    local.CompareZero.DateOfPosting = local.NullDate.DateOfPosting;
    local.CompareZero.PaymentAmount = 0;

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // Check the SPECIFIC requirements for the INTERSTATE_CASE
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // * * Action_Reason_Code = 'CITAX'
    // * * Type = 'COL'
    local.Compare.ActionReasonCode = "CITAX";
    local.Compare.FunctionalTypeCode = "COL";

    // 07/05/01  Paul Phinney         I00122679     Change Sort Order
    // TO: Collection_Type, Source_Type
    foreach(var item in ReadInterstateCollectionInterstateCase())
    {
      if (AsChar(local.BatchAlreadyProcessed.Flag) == 'Y')
      {
        break;
      }

      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // Check the SPECIFIC requirements for EACH additional Entity
      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // * * Must have a Tran Envelope
      local.TranEnvelopeFound.Flag = "N";

      if (ReadCsenetTransactionEnvelop())
      {
        local.Save.Assign(entities.GetNextInterstateCase);
        local.TranEnvelopeFound.Flag = "Y";
      }

      if (AsChar(local.TranEnvelopeFound.Flag) == 'N')
      {
        continue;
      }

      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // Check the SPECIFIC requirements for EACH additional Entity
      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      ExitState = "ACO_NN0000_ALL_OK";
      local.Pass.Assign(entities.GetNextInterstateCollection);

      // Collection TYPE
      // * * ONLY Collection types of S, U, and F will be processed
      switch(AsChar(entities.GetNextInterstateCollection.PaymentSource))
      {
        case 'F':
          local.SetCollectionType.Code = "7";

          for(local.ControlTotals.Index = 0; local.ControlTotals.Index < local
            .ControlTotals.Count; ++local.ControlTotals.Index)
          {
            switch(local.ControlTotals.Item.ProgramControlTotal.
              SystemGeneratedIdentifier)
            {
              case 1:
                break;
              case 2:
                break;
              case 3:
                break;
              case 4:
                break;
              case 5:
                break;
              case 6:
                local.ControlTotals.Update.ProgramControlTotal.Value =
                  local.ControlTotals.Item.ProgramControlTotal.Value.
                    GetValueOrDefault() + 1;
                local.ControlTotals.Update.Common.TotalCurrency =
                  local.ControlTotals.Item.Common.TotalCurrency + entities
                  .GetNextInterstateCollection.PaymentAmount.
                    GetValueOrDefault();

                break;
              default:
                goto AfterCycle2;
            }
          }

AfterCycle2:

          break;
        case 'S':
          local.SetCollectionType.Code = "8";

          for(local.ControlTotals.Index = 0; local.ControlTotals.Index < local
            .ControlTotals.Count; ++local.ControlTotals.Index)
          {
            switch(local.ControlTotals.Item.ProgramControlTotal.
              SystemGeneratedIdentifier)
            {
              case 1:
                break;
              case 2:
                break;
              case 3:
                break;
              case 4:
                break;
              case 5:
                break;
              case 6:
                break;
              case 7:
                local.ControlTotals.Update.ProgramControlTotal.Value =
                  local.ControlTotals.Item.ProgramControlTotal.Value.
                    GetValueOrDefault() + 1;
                local.ControlTotals.Update.Common.TotalCurrency =
                  local.ControlTotals.Item.Common.TotalCurrency + entities
                  .GetNextInterstateCollection.PaymentAmount.
                    GetValueOrDefault();

                break;
              case 8:
                goto AfterCycle3;
              case 9:
                break;
              default:
                goto AfterCycle3;
            }
          }

AfterCycle3:

          break;
        case 'U':
          local.SetCollectionType.Code = "9";

          for(local.ControlTotals.Index = 0; local.ControlTotals.Index < local
            .ControlTotals.Count; ++local.ControlTotals.Index)
          {
            switch(local.ControlTotals.Item.ProgramControlTotal.
              SystemGeneratedIdentifier)
            {
              case 1:
                break;
              case 2:
                break;
              case 3:
                break;
              case 4:
                break;
              case 5:
                break;
              case 6:
                break;
              case 7:
                break;
              case 8:
                local.ControlTotals.Update.ProgramControlTotal.Value =
                  local.ControlTotals.Item.ProgramControlTotal.Value.
                    GetValueOrDefault() + 1;
                local.ControlTotals.Update.Common.TotalCurrency =
                  local.ControlTotals.Item.Common.TotalCurrency + entities
                  .GetNextInterstateCollection.PaymentAmount.
                    GetValueOrDefault();

                goto AfterCycle4;
              case 9:
                break;
              default:
                goto AfterCycle4;
            }
          }

AfterCycle4:

          break;
        default:
          continue;
      }

      // Set Source to OTHER STATE
      // * *   MUST be a Valid STATE Fips Code
      local.FindState.State = entities.GetNextInterstateCase.OtherFipsState;

      if (ReadFips())
      {
        MoveFips(entities.Fips, local.FindState);
      }
      else
      {
        continue;
      }

      local.SetCashReceiptSourceType.Code =
        local.FindState.StateAbbreviation + " STATE";

      if (ReadCashReceiptSourceType())
      {
        MoveCashReceiptSourceType(entities.CashReceiptSourceType,
          local.SetCashReceiptSourceType);
      }
      else
      {
        continue;
      }

      // Add to RECORDS Read Accumulator
      for(local.ControlTotals.Index = 0; local.ControlTotals.Index < local
        .ControlTotals.Count; ++local.ControlTotals.Index)
      {
        switch(local.ControlTotals.Item.ProgramControlTotal.
          SystemGeneratedIdentifier)
        {
          case 1:
            local.ControlTotals.Update.ProgramControlTotal.Value =
              local.ControlTotals.Item.ProgramControlTotal.Value.
                GetValueOrDefault() + 1;
            local.ControlTotals.Update.Common.TotalCurrency =
              local.ControlTotals.Item.Common.TotalCurrency + entities
              .GetNextInterstateCollection.PaymentAmount.GetValueOrDefault();

            break;
          case 2:
            goto AfterCycle5;
          default:
            goto AfterCycle5;
        }
      }

AfterCycle5:

      // * * Amount is EQUAL to ZERO - Bypass
      if (Equal(entities.GetNextInterstateCollection.PaymentAmount, 0))
      {
        continue;
      }

      if (Lt(entities.GetNextInterstateCollection.PaymentAmount, 0))
      {
        // *****
        // Process adjustment logic    DISPLAY as much info as possible on ERROR
        // Report Only
        // *****
        local.AdjustmentsFound.Flag = "Y";

        if (AsChar(local.ErrorReportOpened.Flag) != 'Y')
        {
          local.ReportProcessing.Action = "OPEN";
          local.ReportHandling.ProgramName = "SWEFB617";
          local.ReportHandling.ProcessDate =
            local.ProgramProcessingInfo.ProcessDate;
          UseCabErrorReport2();

          if (Equal(local.ReportProcessing.Status, "OK"))
          {
          }
          else
          {
            ExitState = "ACO_RE0000_ERR_OPNG_ERROR_RPT_A";

            return;
          }

          local.ErrorReportOpened.Flag = "Y";
        }

        // * * * This logic was added because the AP Identifier is OPTIONAL
        local.CsePersonsWorkSet.Assign(local.Blank);

        if (ReadInterstateApIdentification())
        {
          // If SSN is supplied - try to get CSE Person Number
          if (Lt("000000000", entities.InterstateApIdentification.Ssn))
          {
            local.CsePersonsWorkSet.Ssn =
              entities.InterstateApIdentification.Ssn ?? Spaces(9);
            UseEabReadCsePersonUsingSsn();
            local.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;
            local.CsePersonsWorkSet.Ssn =
              entities.InterstateApIdentification.Ssn ?? Spaces(9);
          }
          else
          {
            local.CsePersonsWorkSet.Ssn = "";
          }

          // Format the NAME from the AP_Identification Entity
          local.CsePersonsWorkSet.FormattedName =
            entities.InterstateApIdentification.NameFirst;
          local.CsePersonsWorkSet.FormattedName =
            TrimEnd(local.CsePersonsWorkSet.FormattedName) + " " + entities
            .InterstateApIdentification.MiddleName;
          local.CsePersonsWorkSet.FormattedName =
            TrimEnd(local.CsePersonsWorkSet.FormattedName) + " " + entities
            .InterstateApIdentification.NameLast;
        }
        else
        {
          // * * * This logic was added because the AP Identifier is OPTIONAL
          local.Test.Number =
            Substring(entities.GetNextInterstateCase.KsCaseId, 1, 10);

          if (IsEmpty(local.Test.Number))
          {
            goto Read;
          }

          if (ReadCase())
          {
            if (ReadCaseRoleCsePersonCsePersonAccount())
            {
              local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
            }
            else
            {
              UseSiGetApForInterstateCase();
              local.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

              if (IsEmpty(local.CsePersonsWorkSet.FormattedName))
              {
                // Format the NAME if NOT returned
                local.CsePersonsWorkSet.FormattedName =
                  local.CsePersonsWorkSet.FirstName;
                local.CsePersonsWorkSet.FormattedName =
                  TrimEnd(local.CsePersonsWorkSet.FormattedName) + " " + local
                  .CsePersonsWorkSet.MiddleInitial;
                local.CsePersonsWorkSet.FormattedName =
                  TrimEnd(local.CsePersonsWorkSet.FormattedName) + " " + local
                  .CsePersonsWorkSet.LastName;
              }
            }
          }
          else
          {
            UseSiGetApForInterstateCase();
            local.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

            if (IsEmpty(local.CsePersonsWorkSet.FormattedName))
            {
              // Format the NAME if NOT returned
              local.CsePersonsWorkSet.FormattedName =
                local.CsePersonsWorkSet.FirstName;
              local.CsePersonsWorkSet.FormattedName =
                TrimEnd(local.CsePersonsWorkSet.FormattedName) + " " + local
                .CsePersonsWorkSet.MiddleInitial;
              local.CsePersonsWorkSet.FormattedName =
                TrimEnd(local.CsePersonsWorkSet.FormattedName) + " " + local
                .CsePersonsWorkSet.LastName;
            }
          }
        }

Read:

        local.ReportHandling.RptDetail =
          "A Negative Payment has been received for Person Number";

        if (!IsEmpty(local.CsePersonsWorkSet.Number))
        {
          local.ReportHandling.RptDetail =
            TrimEnd(local.ReportHandling.RptDetail) + " " + local
            .CsePersonsWorkSet.Number;

          if (!IsEmpty(local.CsePersonsWorkSet.FormattedName))
          {
            goto Test2;
          }

          UseSiReadCsePersonBatch();
        }
        else
        {
          local.ReportHandling.RptDetail =
            TrimEnd(local.ReportHandling.RptDetail) + " " + "*UNKNOWN *";
        }

Test2:

        // Format the NAME
        if (!IsEmpty(local.CsePersonsWorkSet.FormattedName))
        {
          local.FormatName.KeyInfo = local.CsePersonsWorkSet.FormattedName;
        }
        else
        {
          if (!IsEmpty(local.CsePersonsWorkSet.FirstName) || !
            IsEmpty(local.CsePersonsWorkSet.LastName))
          {
            local.FormatName.KeyInfo = local.CsePersonsWorkSet.FirstName;
            local.FormatName.KeyInfo = TrimEnd(local.FormatName.KeyInfo) + " " +
              local.CsePersonsWorkSet.MiddleInitial;
            local.FormatName.KeyInfo = TrimEnd(local.FormatName.KeyInfo) + " " +
              local.CsePersonsWorkSet.LastName;

            goto Test3;
          }
          else
          {
          }

          local.FormatName.KeyInfo = "*Name was NOT Found*";
        }

Test3:

        ExitState = "ACO_NN0000_ALL_OK";

        // Format the DATE
        local.SaveFormatDate.Text10 =
          NumberToString(DateToInt(
            entities.GetNextInterstateCollection.DateOfCollection), 10);
        local.FormatDate.Text10 =
          Substring(local.SaveFormatDate.Text10, TextWorkArea.Text10_MaxLength,
          7, 2) + "/" + Substring
          (local.SaveFormatDate.Text10, TextWorkArea.Text10_MaxLength, 9, 2) + "/"
          + Substring
          (local.SaveFormatDate.Text10, TextWorkArea.Text10_MaxLength, 3, 4);

        // Format the AMOUNT
        local.FormatAmount.Text10 =
          NumberToString((long)entities.GetNextInterstateCollection.
            PaymentAmount.GetValueOrDefault(), 9, 7);
        local.FormatAmount.Text10 =
          Substring(local.FormatAmount.Text10, TextWorkArea.Text10_MaxLength, 1,
          7) + "." + NumberToString
          ((long)(entities.GetNextInterstateCollection.PaymentAmount.
            GetValueOrDefault() * 100), 14, 2);
        local.ReportHandling.RptDetail =
          TrimEnd(local.ReportHandling.RptDetail) + " on Case " + entities
          .GetNextInterstateCase.KsCaseId;
        local.ReportHandling.RptDetail =
          TrimEnd(local.ReportHandling.RptDetail) + " in the Amount of " + local
          .FormatAmount.Text10;
        local.ReportProcessing.Action = "WRITE";
        local.ReportHandling.ProgramName = "SWEFB617";
        UseCabErrorReport1();

        if (Equal(local.ReportProcessing.Status, "OK"))
        {
        }
        else
        {
          ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

          return;
        }

        local.ReportHandling.RptDetail = "";
        local.ReportHandling.RptDetail =
          TrimEnd(local.ReportHandling.RptDetail) + " Collection Type = '" + entities
          .GetNextInterstateCollection.PaymentSource;
        local.ReportHandling.RptDetail =
          TrimEnd(local.ReportHandling.RptDetail) + "' Source = '" + local
          .SetCashReceiptSourceType.Code;
        local.ReportHandling.RptDetail =
          TrimEnd(local.ReportHandling.RptDetail) + "' Date = " + local
          .FormatDate.Text10;

        if (!IsEmpty(local.CsePersonsWorkSet.Ssn))
        {
          local.ReportHandling.RptDetail =
            TrimEnd(local.ReportHandling.RptDetail) + " SSN = " + local
            .CsePersonsWorkSet.Ssn;
        }
        else
        {
          local.ReportHandling.RptDetail =
            TrimEnd(local.ReportHandling.RptDetail) + " SSN = " + "*UNKNOWN*";
        }

        local.ReportHandling.RptDetail =
          TrimEnd(local.ReportHandling.RptDetail) + " Name = " + TrimEnd
          (local.FormatName.KeyInfo);
        local.ReportProcessing.Action = "WRITE";
        local.ReportHandling.ProgramName = "SWEFB617";
        UseCabErrorReport1();

        if (Equal(local.ReportProcessing.Status, "OK"))
        {
        }
        else
        {
          ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

          return;
        }

        local.ReportHandling.RptDetail = "";
        local.ReportProcessing.Action = "WRITE";
        local.ReportHandling.ProgramName = "SWEFB617";
        UseCabErrorReport1();

        if (Equal(local.ReportProcessing.Status, "OK"))
        {
        }
        else
        {
          ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

          return;
        }

        for(local.ControlTotals.Index = 0; local.ControlTotals.Index < local
          .ControlTotals.Count; ++local.ControlTotals.Index)
        {
          switch(local.ControlTotals.Item.ProgramControlTotal.
            SystemGeneratedIdentifier)
          {
            case 1:
              break;
            case 2:
              break;
            case 3:
              break;
            case 4:
              break;
            case 5:
              local.ControlTotals.Update.ProgramControlTotal.Value =
                local.ControlTotals.Item.ProgramControlTotal.Value.
                  GetValueOrDefault() + 1;
              local.ControlTotals.Update.Common.TotalCurrency =
                local.ControlTotals.Item.Common.TotalCurrency + entities
                .GetNextInterstateCollection.PaymentAmount.GetValueOrDefault();

              break;
            case 6:
              break;
            default:
              goto AfterCycle6;
          }
        }

AfterCycle6:

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          try
          {
            UpdateInterstateCollection();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_COLLECTION_NU_RB";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_COLLECTION_PV_RB";

                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          try
          {
            UpdateCsenetTransactionEnvelop();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "CSENET_TRANSACTION_ENVELOP_NU";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "CSENET_ENVELOPE_PV";

                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
        else
        {
          break;
        }

        continue;
      }
      else
      {
        // Process Collection
        if (ReadCashReceiptCashReceiptEvent1())
        {
          local.ReturnedCashReceipt.SequentialNumber =
            entities.CashReceipt.SequentialNumber;
          local.ReturnedCashReceiptEvent.SystemGeneratedIdentifier =
            entities.CashReceiptEvent.SystemGeneratedIdentifier;
        }
        else
        {
          local.Pass.Assign(entities.GetNextInterstateCollection);
          local.Pass.DateOfCollection = local.ProgramProcessingInfo.ProcessDate;
          UseFnAbAddCsenetCashReceipt();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            break;
          }

          // * Verify that Cash_Receipt was Created
          // *  and SET CURRENCY
          if (ReadCashReceiptCashReceiptEvent3())
          {
            local.ReturnedCashReceipt.SequentialNumber =
              entities.CashReceipt.SequentialNumber;
          }
          else
          {
            ExitState = "FN0000_CASH_RCPT_EVENT_NF";

            break;
          }
        }

        local.ProcessInterstateCase.Assign(entities.GetNextInterstateCase);
        local.ProcessInterstateCollection.Assign(
          entities.GetNextInterstateCollection);
        UseFnProcessCsenetIntrStColl();

        if (local.NumberOfSusp.Count > 0)
        {
          for(local.ControlTotals.Index = 0; local.ControlTotals.Index < local
            .ControlTotals.Count; ++local.ControlTotals.Index)
          {
            switch(local.ControlTotals.Item.ProgramControlTotal.
              SystemGeneratedIdentifier)
            {
              case 1:
                break;
              case 2:
                break;
              case 3:
                local.ControlTotals.Update.ProgramControlTotal.Value =
                  local.ControlTotals.Item.ProgramControlTotal.Value.
                    GetValueOrDefault() + 1;
                local.ControlTotals.Update.Common.TotalCurrency =
                  local.ControlTotals.Item.Common.TotalCurrency + entities
                  .GetNextInterstateCollection.PaymentAmount.
                    GetValueOrDefault();

                break;
              case 4:
                goto AfterCycle7;
              case 5:
                break;
              case 6:
                break;
              case 7:
                break;
              case 8:
                break;
              case 9:
                break;
              default:
                goto AfterCycle7;
            }
          }

AfterCycle7:
          ;
        }
        else
        {
          for(local.ControlTotals.Index = 0; local.ControlTotals.Index < local
            .ControlTotals.Count; ++local.ControlTotals.Index)
          {
            switch(local.ControlTotals.Item.ProgramControlTotal.
              SystemGeneratedIdentifier)
            {
              case 1:
                break;
              case 2:
                break;
              case 3:
                break;
              case 4:
                local.ControlTotals.Update.ProgramControlTotal.Value =
                  local.ControlTotals.Item.ProgramControlTotal.Value.
                    GetValueOrDefault() + 1;
                local.ControlTotals.Update.Common.TotalCurrency =
                  local.ControlTotals.Item.Common.TotalCurrency + entities
                  .GetNextInterstateCollection.PaymentAmount.
                    GetValueOrDefault();

                break;
              case 5:
                goto AfterCycle8;
              case 6:
                break;
              case 7:
                break;
              case 8:
                break;
              case 9:
                break;
              default:
                goto AfterCycle8;
            }
          }

AfterCycle8:
          ;
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          break;
        }
      }

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        // ASSOCIATE this CASH RECEIPT to ANY Fund Transaction if not already 
        // associated
        if (ReadFundTransaction1())
        {
          try
          {
            UpdateFundTransaction();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_FUND_TRANS_NU_RB";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_FUND_TRANS_PV_RB";

                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
        else
        {
          ReadFundTransaction2();

          if (!entities.Associated.Populated)
          {
            ExitState = "FN0000_FUND_TRANS_NF_RB";

            break;
          }

          AssociateCashReceipt();

          try
          {
            UpdateFundTransaction();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_FUND_TRANS_NU_RB";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_FUND_TRANS_PV_RB";

                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }

        try
        {
          UpdateCashReceipt();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0089_CASH_RCPT_NU_RB";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0091_CASH_RCPT_PV_RB";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        try
        {
          UpdateCashReceiptEvent();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0079_CASH_RCPT_EVENT_NU_W_RB";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0080_CASH_RCPT_EVENT_PV_W_RB";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
      else
      {
        break;
      }
    }

    // END of Records to be Processed
    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // *****
      // Set restart indicator to no because we successfully finished this 
      // program.
      // *****
      local.ProgramCheckpointRestart.RestartInfo = "";
      local.ProgramCheckpointRestart.RestartInd = "N";
      UseUpdatePgmCheckpointRestart();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }

      UseExtToDoACommit();

      if (local.PassArea.NumericReturnCode != 0)
      {
        ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

        return;
      }
    }
    else
    {
      UseEabRollbackSql();

      if (local.PassArea.NumericReturnCode != 0)
      {
        ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

        return;
      }

      for(local.ControlTotals.Index = 0; local.ControlTotals.Index < local
        .ControlTotals.Count; ++local.ControlTotals.Index)
      {
        switch(local.ControlTotals.Item.ProgramControlTotal.
          SystemGeneratedIdentifier)
        {
          case 1:
            break;
          case 2:
            break;
          case 3:
            break;
          case 4:
            break;
          case 5:
            break;
          case 6:
            break;
          case 7:
            break;
          case 8:
            break;
          case 9:
            local.ControlTotals.Update.ProgramControlTotal.Value =
              local.ControlTotals.Item.ProgramControlTotal.Value.
                GetValueOrDefault() + 1;
            local.ControlTotals.Update.Common.TotalCurrency =
              local.ControlTotals.Item.Common.TotalCurrency + entities
              .GetNextInterstateCollection.PaymentAmount.GetValueOrDefault();

            break;
          default:
            goto AfterCycle9;
        }
      }

AfterCycle9:

      // PRINT Error Report
      if (AsChar(local.ErrorReportOpened.Flag) != 'Y')
      {
        local.ReportProcessing.Action = "OPEN";
        local.ReportHandling.ProgramName = "SWEFB617";
        local.ReportHandling.ProcessDate =
          local.ProgramProcessingInfo.ProcessDate;
        UseCabErrorReport2();

        if (Equal(local.ReportProcessing.Status, "OK"))
        {
        }
        else
        {
          ExitState = "ACO_RE0000_ERR_OPNG_ERROR_RPT_A";

          return;
        }

        local.ErrorReportOpened.Flag = "Y";
      }

      ++local.AaaGroupLocalErrors.Index;
      local.AaaGroupLocalErrors.CheckSize();

      if (!IsExitState("INTERFACE_ALREADY_PROCESSED_RB"))
      {
        // *****
        // Set the Key Info with the Local Code, SSN number, and the creation 
        // date.
        // *****
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          KeyInfo = "For Interstate CASE Serial # = " + NumberToString
          (entities.GetNextInterstateCase.TransSerialNumber, 15);
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          KeyInfo =
            TrimEnd(local.AaaGroupLocalErrors.Item.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " with Seq Num of ";
          
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          KeyInfo =
            TrimEnd(local.AaaGroupLocalErrors.Item.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " " + NumberToString
          (entities.GetNextInterstateCollection.SystemGeneratedSequenceNum, 15);
          
      }

      // *****
      // Set the Error Message Text to the exit state description.
      // *****
      local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();
      local.ReportHandling.RptDetail =
        TrimEnd(local.AaaGroupLocalErrors.Item.
          AaaGroupLocalErrorsDetailProgramError.KeyInfo) + "  ES = " + local
        .ExitStateWorkArea.Message;
      local.ReportProcessing.Action = "WRITE";
      local.ReportHandling.ProgramName = "SWEFB617";
      UseCabErrorReport1();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
        // 08/24/00  Paul Phinney         H00101885     Change logic to CAUSE 
        // ABEND
        // when ERROR condition occurs.
        ExitState = "ACO_NN0000_ALL_OK";
      }
      else
      {
        ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

        return;
      }

      local.ReportHandling.RptDetail = "";
      local.ReportProcessing.Action = "WRITE";
      local.ReportHandling.ProgramName = "SWEFB617";
      UseCabErrorReport1();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
      }
      else
      {
        ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

        return;
      }

      // *****
      // Set the termination action.
      // *****
      local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailStandard.
        Command = "ROLLBACK";
      local.AbendError.Flag = "Y";
    }

    local.ReportProcessing.Action = "OPEN";
    local.ReportHandling.ProgramName = "SWEFB617";
    local.ReportHandling.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    UseCabControlReport2();

    if (Equal(local.ReportProcessing.Status, "OK"))
    {
    }
    else
    {
      ExitState = "FN0000_ERROR_OPENING_CONTROL_RPT";

      return;
    }

    // PRINT the Control Report
    // PRINT COUNTS
    for(local.ControlTotals.Index = 0; local.ControlTotals.Index < local
      .ControlTotals.Count; ++local.ControlTotals.Index)
    {
      if (local.ControlTotals.Item.ProgramControlTotal.
        SystemGeneratedIdentifier == 0)
      {
        continue;
      }

      // This count is no longer used - left in only to hold place for 
      // accumulators
      if (local.ControlTotals.Item.ProgramControlTotal.
        SystemGeneratedIdentifier == 2)
      {
        continue;
      }

      local.ReportHandling.RptDetail =
        (local.ControlTotals.Item.ProgramControlTotal.Name ?? "") + NumberToString
        ((long)local.ControlTotals.Item.ProgramControlTotal.Value.
          GetValueOrDefault(), 7, 9);
      local.ReportProcessing.Action = "WRITE";
      local.ReportHandling.ProgramName = "SWEFB617";
      UseCabControlReport1();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
      }
      else
      {
        ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

        return;
      }
    }

    // SPACING
    local.ReportHandling.RptDetail = "";
    local.ReportProcessing.Action = "WRITE";
    local.ReportHandling.ProgramName = "SWEFB617";
    UseCabControlReport1();

    if (Equal(local.ReportProcessing.Status, "OK"))
    {
    }
    else
    {
      ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

      return;
    }

    // PRINT AMOUNTS
    for(local.ControlTotals.Index = 0; local.ControlTotals.Index < local
      .ControlTotals.Count; ++local.ControlTotals.Index)
    {
      if (local.ControlTotals.Item.ProgramControlTotal.
        SystemGeneratedIdentifier == 0)
      {
        continue;
      }

      // This count is no longer used - left in only to hold place for 
      // accumulators
      if (local.ControlTotals.Item.ProgramControlTotal.
        SystemGeneratedIdentifier == 2)
      {
        continue;
      }

      local.ReportHandling.RptDetail = "Amount" + Substring
        (local.ControlTotals.Item.ProgramControlTotal.Name, 7, 54);
      local.ReportHandling.RptDetail =
        Substring(local.ReportHandling.RptDetail,
        EabReportSend.RptDetail_MaxLength, 1, 60) + NumberToString
        ((long)local.ControlTotals.Item.Common.TotalCurrency, 10, 6) + "." + NumberToString
        ((long)(local.ControlTotals.Item.Common.TotalCurrency * 100), 14, 2);

      if (local.ControlTotals.Item.Common.TotalCurrency < 0)
      {
        local.ReportHandling.RptDetail =
          TrimEnd(local.ReportHandling.RptDetail) + "-";
      }

      local.ReportProcessing.Action = "WRITE";
      local.ReportHandling.ProgramName = "SWEFB617";
      UseCabControlReport1();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
      }
      else
      {
        ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

        return;
      }
    }

    // SPACING
    local.ReportHandling.RptDetail = "";
    local.ReportProcessing.Action = "WRITE";
    local.ReportHandling.ProgramName = "SWEFB617";
    UseCabControlReport1();

    if (Equal(local.ReportProcessing.Status, "OK"))
    {
    }
    else
    {
      ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

      return;
    }

    if (AsChar(local.AbendError.Flag) == 'Y')
    {
      local.ProgramCheckpointRestart.RestartInd = "Y";
      local.ReportHandling.RptDetail =
        "* * * Abend ERROR - See ERROR Report * * *";
    }
    else if (AsChar(local.AdjustmentsFound.Flag) == 'Y')
    {
      local.ProgramCheckpointRestart.RestartInd = "N";
      local.ReportHandling.RptDetail =
        "* * * Adjustments found - See ERROR Report * * *";
    }
    else
    {
      local.ProgramCheckpointRestart.RestartInd = "N";
      local.ReportHandling.RptDetail = "* * * Successful End of Job * * *";
    }

    // FOOTER
    local.ReportProcessing.Action = "WRITE";
    local.ReportHandling.ProgramName = "SWEFB617";
    UseCabControlReport1();

    if (Equal(local.ReportProcessing.Status, "OK"))
    {
    }
    else
    {
      ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

      return;
    }

    // SPACING
    local.ReportHandling.RptDetail = "";
    local.ReportProcessing.Action = "WRITE";
    local.ReportHandling.ProgramName = "SWEFB617";
    UseCabControlReport1();

    if (Equal(local.ReportProcessing.Status, "OK"))
    {
    }
    else
    {
      ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

      return;
    }

    local.ReportProcessing.Action = "CLOSE";
    local.ReportHandling.ProgramName = "SWEFB617";
    local.ReportHandling.ProcessDate =
      entities.ProgramProcessingInfo.ProcessDate;
    UseCabControlReport2();

    if (Equal(local.ReportProcessing.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_CLOSING_CTL_RPT_FILE";

      return;
    }

    if (AsChar(local.ErrorReportOpened.Flag) == 'Y')
    {
      local.ReportProcessing.Action = "CLOSE";
      local.ReportHandling.ProgramName = "SWEFB617";
      local.ReportHandling.ProcessDate =
        local.ProgramProcessingInfo.ProcessDate;
      UseCabErrorReport2();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
      }
      else
      {
        ExitState = "SI0000_ERR_CLOSING_ERR_RPT_FILE";

        return;
      }

      // 08/24/00  Paul Phinney         H00101885     Change logic to CAUSE 
      // ABEND
      // Changed ESCAPE to NOT exit program.
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // *      END of NEW logic
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    UseFnAbCsenetFinalProcessing();

    // *****
    // Set restart indicator to no because we successfully finished this 
    // program.
    // *****
    local.ProgramCheckpointRestart.RestartInfo = "";
    UseUpdatePgmCheckpointRestart();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    if (AsChar(local.AbendError.Flag) == 'Y')
    {
      UseEabRollbackSql();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
    else
    {
      ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
      UseSiCloseAdabas();
    }

    UseSiCloseAdabas();
  }

  private static void MoveCashReceiptSourceType(CashReceiptSourceType source,
    CashReceiptSourceType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveCollectionType(CollectionType source,
    CollectionType target)
  {
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.Code = source.Code;
  }

  private static void MoveCsePerson1(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.CreatedBy = source.CreatedBy;
    target.Number = source.Number;
    target.KscaresNumber = source.KscaresNumber;
    target.Occupation = source.Occupation;
    target.AeCaseNumber = source.AeCaseNumber;
    target.DateOfDeath = source.DateOfDeath;
    target.IllegalAlienIndicator = source.IllegalAlienIndicator;
    target.CurrentSpouseMi = source.CurrentSpouseMi;
    target.CurrentSpouseFirstName = source.CurrentSpouseFirstName;
    target.BirthPlaceState = source.BirthPlaceState;
    target.EmergencyPhone = source.EmergencyPhone;
    target.NameMiddle = source.NameMiddle;
    target.NameMaiden = source.NameMaiden;
    target.HomePhone = source.HomePhone;
    target.OtherNumber = source.OtherNumber;
    target.BirthPlaceCity = source.BirthPlaceCity;
    target.Weight = source.Weight;
    target.OtherIdInfo = source.OtherIdInfo;
    target.CurrentMaritalStatus = source.CurrentMaritalStatus;
    target.CurrentSpouseLastName = source.CurrentSpouseLastName;
    target.Race = source.Race;
    target.HeightFt = source.HeightFt;
    target.HeightIn = source.HeightIn;
    target.HairColor = source.HairColor;
    target.EyeColor = source.EyeColor;
    target.HomePhoneAreaCode = source.HomePhoneAreaCode;
    target.EmergencyAreaCode = source.EmergencyAreaCode;
    target.OtherAreaCode = source.OtherAreaCode;
    target.OtherPhoneType = source.OtherPhoneType;
    target.WorkPhoneAreaCode = source.WorkPhoneAreaCode;
    target.WorkPhone = source.WorkPhone;
    target.WorkPhoneExt = source.WorkPhoneExt;
    target.UnemploymentInd = source.UnemploymentInd;
    target.FederalInd = source.FederalInd;
  }

  private static void MoveCsePerson2(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
    target.KscaresNumber = source.KscaresNumber;
    target.Occupation = source.Occupation;
    target.AeCaseNumber = source.AeCaseNumber;
    target.DateOfDeath = source.DateOfDeath;
    target.IllegalAlienIndicator = source.IllegalAlienIndicator;
    target.CurrentSpouseMi = source.CurrentSpouseMi;
    target.CurrentSpouseFirstName = source.CurrentSpouseFirstName;
    target.BirthPlaceState = source.BirthPlaceState;
    target.EmergencyPhone = source.EmergencyPhone;
    target.NameMiddle = source.NameMiddle;
    target.NameMaiden = source.NameMaiden;
    target.HomePhone = source.HomePhone;
    target.OtherNumber = source.OtherNumber;
    target.BirthPlaceCity = source.BirthPlaceCity;
    target.Weight = source.Weight;
    target.OtherIdInfo = source.OtherIdInfo;
    target.CurrentMaritalStatus = source.CurrentMaritalStatus;
    target.CurrentSpouseLastName = source.CurrentSpouseLastName;
    target.Race = source.Race;
    target.HeightFt = source.HeightFt;
    target.HeightIn = source.HeightIn;
    target.HairColor = source.HairColor;
    target.EyeColor = source.EyeColor;
    target.HomePhoneAreaCode = source.HomePhoneAreaCode;
    target.EmergencyAreaCode = source.EmergencyAreaCode;
    target.OtherAreaCode = source.OtherAreaCode;
    target.OtherPhoneType = source.OtherPhoneType;
    target.WorkPhoneAreaCode = source.WorkPhoneAreaCode;
    target.WorkPhone = source.WorkPhone;
    target.WorkPhoneExt = source.WorkPhoneExt;
    target.UnemploymentInd = source.UnemploymentInd;
    target.FederalInd = source.FederalInd;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveFips(Fips source, Fips target)
  {
    target.StateAbbreviation = source.StateAbbreviation;
    target.State = source.State;
  }

  private static void MoveInterstateCase(InterstateCase source,
    InterstateCase target)
  {
    target.TransSerialNumber = source.TransSerialNumber;
    target.TransactionDate = source.TransactionDate;
  }

  private static void MoveInterstateCollection(InterstateCollection source,
    InterstateCollection target)
  {
    target.SystemGeneratedSequenceNum = source.SystemGeneratedSequenceNum;
    target.DateOfCollection = source.DateOfCollection;
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

    useImport.EabFileHandling.Action = local.ReportProcessing.Action;
    useImport.NeededToWrite.RptDetail = local.ReportHandling.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.ReportProcessing.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.ReportProcessing.Action;
    MoveEabReportSend(local.ReportHandling, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    local.ReportProcessing.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.ReportProcessing.Action;
    useImport.NeededToWrite.RptDetail = local.ReportHandling.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.ReportProcessing.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.ReportProcessing.Action;
    MoveEabReportSend(local.ReportHandling, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.ReportProcessing.Status = useExport.EabFileHandling.Status;
  }

  private string UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    return useExport.ExitStateWorkArea.Message;
  }

  private void UseEabReadCsePersonUsingSsn()
  {
    var useImport = new EabReadCsePersonUsingSsn.Import();
    var useExport = new EabReadCsePersonUsingSsn.Export();

    useImport.CsePersonsWorkSet.Ssn = local.CsePersonsWorkSet.Ssn;
    MoveCsePersonsWorkSet(local.CsePersonsWorkSet, useExport.CsePersonsWorkSet);

    Call(EabReadCsePersonUsingSsn.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
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

  private void UseFnAbAddCsenetCashReceipt()
  {
    var useImport = new FnAbAddCsenetCashReceipt.Import();
    var useExport = new FnAbAddCsenetCashReceipt.Export();

    useImport.CashReceiptSourceType.Code = local.SetCashReceiptSourceType.Code;
    MoveInterstateCollection(local.Pass, useImport.InterstateCollection);

    Call(FnAbAddCsenetCashReceipt.Execute, useImport, useExport);

    MoveCashReceiptSourceType(useExport.Exoprt,
      local.ReturnedCashReceiptSourceType);
    local.ReturnedCashReceiptEvent.SystemGeneratedIdentifier =
      useExport.CashReceiptEvent.SystemGeneratedIdentifier;
    local.ReturnedCashReceipt.SequentialNumber =
      useExport.CashReceipt.SequentialNumber;
  }

  private void UseFnAbCsenetFinalProcessing()
  {
    var useImport = new FnAbCsenetFinalProcessing.Import();
    var useExport = new FnAbCsenetFinalProcessing.Export();

    useImport.CashReceiptType.SystemGeneratedIdentifier =
      local.HardcodedCsenet.SystemGeneratedIdentifier;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      local.ReturnedCashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      local.ReturnedCashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceipt.SequentialNumber =
      local.ReturnedCashReceipt.SequentialNumber;

    Call(FnAbCsenetFinalProcessing.Execute, useImport, useExport);

    export.CashReceipt.SequentialNumber =
      useExport.CashReceipt.SequentialNumber;
  }

  private void UseFnProcessCsenetIntrStColl()
  {
    var useImport = new FnProcessCsenetIntrStColl.Import();
    var useExport = new FnProcessCsenetIntrStColl.Export();

    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;
    useImport.InterstateCase.Assign(local.ProcessInterstateCase);
    useImport.CashReceipt.SequentialNumber =
      local.ReturnedCashReceipt.SequentialNumber;
    MoveCollectionType(local.SetCollectionType, useImport.CollectionType);
    useImport.InterstateCollection.Assign(local.ProcessInterstateCollection);
    useImport.P.Assign(entities.CashReceipt);
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      local.ReturnedCashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      local.ReturnedCashReceiptSourceType.SystemGeneratedIdentifier;
    useExport.ImportNextCsenetId.SequentialIdentifier =
      local.NextFdsoId.SequentialIdentifier;
    useExport.ImportNumberOfRel.Count = local.NumberOfRel.Count;
    useExport.ImportNumberOfSusp.Count = local.NumberOfSusp.Count;

    Call(FnProcessCsenetIntrStColl.Execute, useImport, useExport);

    entities.CashReceipt.SequentialNumber = useImport.P.SequentialNumber;
    local.NextFdsoId.SequentialIdentifier =
      useExport.ImportNextCsenetId.SequentialIdentifier;
    local.NumberOfRel.Count = useExport.ImportNumberOfRel.Count;
    local.NumberOfSusp.Count = useExport.ImportNumberOfSusp.Count;
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

  private void UseSiCloseAdabas()
  {
    var useImport = new SiCloseAdabas.Import();
    var useExport = new SiCloseAdabas.Export();

    Call(SiCloseAdabas.Execute, useImport, useExport);
  }

  private void UseSiGetApForInterstateCase()
  {
    var useImport = new SiGetApForInterstateCase.Import();
    var useExport = new SiGetApForInterstateCase.Export();

    MoveInterstateCase(entities.GetNextInterstateCase, useImport.InterstateCase);
      

    Call(SiGetApForInterstateCase.Execute, useImport, useExport);

    MoveCsePerson2(useExport.CsePerson, local.CsePerson);
    local.CsePersonsWorkSet.Number = useExport.CsePersonsWorkSet.Number;
  }

  private void UseSiReadCsePersonBatch()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    MoveCsePerson1(useExport.CsePerson, local.CsePerson);
    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseUpdatePgmCheckpointRestart()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);
  }

  private void AssociateCashReceipt()
  {
    System.Diagnostics.Debug.Assert(entities.Associated.Populated);
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);

    var fttIdentifier = entities.Associated.FttIdentifier;
    var pcaCode = entities.Associated.PcaCode;
    var pcaEffectiveDate = entities.Associated.PcaEffectiveDate;
    var funIdentifier = entities.Associated.FunIdentifier;
    var ftrIdentifier = entities.Associated.SystemGeneratedIdentifier;

    entities.CashReceipt.Populated = false;
    Update("AssociateCashReceipt",
      (db, command) =>
      {
        db.SetNullableInt32(command, "fttIdentifier", fttIdentifier);
        db.SetNullableString(command, "pcaCode", pcaCode);
        db.SetNullableDate(command, "pcaEffectiveDate", pcaEffectiveDate);
        db.SetNullableInt32(command, "funIdentifier", funIdentifier);
        db.SetNullableInt32(command, "ftrIdentifier", ftrIdentifier);
        db.
          SetInt32(command, "crvIdentifier", entities.CashReceipt.CrvIdentifier);
          
        db.
          SetInt32(command, "cstIdentifier", entities.CashReceipt.CstIdentifier);
          
        db.
          SetInt32(command, "crtIdentifier", entities.CashReceipt.CrtIdentifier);
          
      });

    entities.CashReceipt.FttIdentifier = fttIdentifier;
    entities.CashReceipt.PcaCode = pcaCode;
    entities.CashReceipt.PcaEffectiveDate = pcaEffectiveDate;
    entities.CashReceipt.FunIdentifier = funIdentifier;
    entities.CashReceipt.FtrIdentifier = ftrIdentifier;
    entities.CashReceipt.Populated = true;
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", local.Test.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseRoleCsePersonCsePersonAccount()
  {
    entities.CsePersonAccount.Populated = false;
    entities.CsePerson.Populated = false;
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRoleCsePersonCsePersonAccount",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 4);
        entities.CsePersonAccount.Type1 = db.GetString(reader, 5);
        entities.CsePersonAccount.Populated = true;
        entities.CsePerson.Populated = true;
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<CsePersonAccount>("Type1", entities.CsePersonAccount.Type1);
      });
  }

  private bool ReadCashReceiptCashReceiptEvent1()
  {
    entities.CashReceiptEvent.Populated = false;
    entities.CashReceipt.Populated = false;

    return Read("ReadCashReceiptCashReceiptEvent1",
      (db, command) =>
      {
        db.SetDate(
          command, "receivedDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetInt32(
          command, "crtIdentifier",
          local.HardcodedCsenet.SystemGeneratedIdentifier);
        db.SetString(command, "code", local.SetCashReceiptSourceType.Code);
      },
      (db, reader) =>
      {
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceipt.ReceiptAmount = db.GetDecimal(reader, 3);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 4);
        entities.CashReceipt.ReceiptDate = db.GetDate(reader, 5);
        entities.CashReceipt.CheckType = db.GetNullableString(reader, 6);
        entities.CashReceipt.ReceivedDate = db.GetDate(reader, 7);
        entities.CashReceipt.DepositReleaseDate = db.GetNullableDate(reader, 8);
        entities.CashReceipt.BalancedTimestamp =
          db.GetNullableDateTime(reader, 9);
        entities.CashReceipt.TotalNoncashTransactionAmount =
          db.GetNullableDecimal(reader, 10);
        entities.CashReceipt.TotalNoncashTransactionCount =
          db.GetNullableInt32(reader, 11);
        entities.CashReceipt.CreatedBy = db.GetString(reader, 12);
        entities.CashReceipt.FttIdentifier = db.GetNullableInt32(reader, 13);
        entities.CashReceipt.PcaCode = db.GetNullableString(reader, 14);
        entities.CashReceipt.PcaEffectiveDate = db.GetNullableDate(reader, 15);
        entities.CashReceipt.FunIdentifier = db.GetNullableInt32(reader, 16);
        entities.CashReceipt.FtrIdentifier = db.GetNullableInt32(reader, 17);
        entities.CashReceipt.TotalNonCashFeeAmount =
          db.GetNullableDecimal(reader, 18);
        entities.CashReceipt.LastUpdatedBy = db.GetNullableString(reader, 19);
        entities.CashReceipt.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 20);
        entities.CashReceiptEvent.CreatedBy = db.GetString(reader, 21);
        entities.CashReceiptEvent.LastUpdatedBy =
          db.GetNullableString(reader, 22);
        entities.CashReceiptEvent.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 23);
        entities.CashReceiptEvent.TotalNonCashTransactionCount =
          db.GetNullableInt32(reader, 24);
        entities.CashReceiptEvent.TotalNonCashFeeAmt =
          db.GetNullableDecimal(reader, 25);
        entities.CashReceiptEvent.TotalNonCashAmt =
          db.GetNullableDecimal(reader, 26);
        entities.CashReceiptEvent.Populated = true;
        entities.CashReceipt.Populated = true;
      });
  }

  private bool ReadCashReceiptCashReceiptEvent2()
  {
    entities.CashReceiptEvent.Populated = false;
    entities.CashReceipt.Populated = false;

    return Read("ReadCashReceiptCashReceiptEvent2",
      (db, command) =>
      {
        db.SetDate(
          command, "receivedDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetInt32(
          command, "crtIdentifier",
          local.HardcodedCsenet.SystemGeneratedIdentifier);
        db.SetString(command, "createdBy", global.UserId);
      },
      (db, reader) =>
      {
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceipt.ReceiptAmount = db.GetDecimal(reader, 3);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 4);
        entities.CashReceipt.ReceiptDate = db.GetDate(reader, 5);
        entities.CashReceipt.CheckType = db.GetNullableString(reader, 6);
        entities.CashReceipt.ReceivedDate = db.GetDate(reader, 7);
        entities.CashReceipt.DepositReleaseDate = db.GetNullableDate(reader, 8);
        entities.CashReceipt.BalancedTimestamp =
          db.GetNullableDateTime(reader, 9);
        entities.CashReceipt.TotalNoncashTransactionAmount =
          db.GetNullableDecimal(reader, 10);
        entities.CashReceipt.TotalNoncashTransactionCount =
          db.GetNullableInt32(reader, 11);
        entities.CashReceipt.CreatedBy = db.GetString(reader, 12);
        entities.CashReceipt.FttIdentifier = db.GetNullableInt32(reader, 13);
        entities.CashReceipt.PcaCode = db.GetNullableString(reader, 14);
        entities.CashReceipt.PcaEffectiveDate = db.GetNullableDate(reader, 15);
        entities.CashReceipt.FunIdentifier = db.GetNullableInt32(reader, 16);
        entities.CashReceipt.FtrIdentifier = db.GetNullableInt32(reader, 17);
        entities.CashReceipt.TotalNonCashFeeAmount =
          db.GetNullableDecimal(reader, 18);
        entities.CashReceipt.LastUpdatedBy = db.GetNullableString(reader, 19);
        entities.CashReceipt.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 20);
        entities.CashReceiptEvent.CreatedBy = db.GetString(reader, 21);
        entities.CashReceiptEvent.LastUpdatedBy =
          db.GetNullableString(reader, 22);
        entities.CashReceiptEvent.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 23);
        entities.CashReceiptEvent.TotalNonCashTransactionCount =
          db.GetNullableInt32(reader, 24);
        entities.CashReceiptEvent.TotalNonCashFeeAmt =
          db.GetNullableDecimal(reader, 25);
        entities.CashReceiptEvent.TotalNonCashAmt =
          db.GetNullableDecimal(reader, 26);
        entities.CashReceiptEvent.Populated = true;
        entities.CashReceipt.Populated = true;
      });
  }

  private bool ReadCashReceiptCashReceiptEvent3()
  {
    entities.CashReceiptEvent.Populated = false;
    entities.CashReceipt.Populated = false;

    return Read("ReadCashReceiptCashReceiptEvent3",
      (db, command) =>
      {
        db.SetInt32(
          command, "cashReceiptId", local.ReturnedCashReceipt.SequentialNumber);
          
        db.SetInt32(
          command, "creventId",
          local.ReturnedCashReceiptEvent.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceipt.ReceiptAmount = db.GetDecimal(reader, 3);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 4);
        entities.CashReceipt.ReceiptDate = db.GetDate(reader, 5);
        entities.CashReceipt.CheckType = db.GetNullableString(reader, 6);
        entities.CashReceipt.ReceivedDate = db.GetDate(reader, 7);
        entities.CashReceipt.DepositReleaseDate = db.GetNullableDate(reader, 8);
        entities.CashReceipt.BalancedTimestamp =
          db.GetNullableDateTime(reader, 9);
        entities.CashReceipt.TotalNoncashTransactionAmount =
          db.GetNullableDecimal(reader, 10);
        entities.CashReceipt.TotalNoncashTransactionCount =
          db.GetNullableInt32(reader, 11);
        entities.CashReceipt.CreatedBy = db.GetString(reader, 12);
        entities.CashReceipt.FttIdentifier = db.GetNullableInt32(reader, 13);
        entities.CashReceipt.PcaCode = db.GetNullableString(reader, 14);
        entities.CashReceipt.PcaEffectiveDate = db.GetNullableDate(reader, 15);
        entities.CashReceipt.FunIdentifier = db.GetNullableInt32(reader, 16);
        entities.CashReceipt.FtrIdentifier = db.GetNullableInt32(reader, 17);
        entities.CashReceipt.TotalNonCashFeeAmount =
          db.GetNullableDecimal(reader, 18);
        entities.CashReceipt.LastUpdatedBy = db.GetNullableString(reader, 19);
        entities.CashReceipt.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 20);
        entities.CashReceiptEvent.CreatedBy = db.GetString(reader, 21);
        entities.CashReceiptEvent.LastUpdatedBy =
          db.GetNullableString(reader, 22);
        entities.CashReceiptEvent.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 23);
        entities.CashReceiptEvent.TotalNonCashTransactionCount =
          db.GetNullableInt32(reader, 24);
        entities.CashReceiptEvent.TotalNonCashFeeAmt =
          db.GetNullableDecimal(reader, 25);
        entities.CashReceiptEvent.TotalNonCashAmt =
          db.GetNullableDecimal(reader, 26);
        entities.CashReceiptEvent.Populated = true;
        entities.CashReceipt.Populated = true;
      });
  }

  private bool ReadCashReceiptSourceType()
  {
    entities.CashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptSourceType",
      (db, command) =>
      {
        db.SetString(command, "code", local.SetCashReceiptSourceType.Code);
      },
      (db, reader) =>
      {
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 1);
        entities.CashReceiptSourceType.Populated = true;
      });
  }

  private bool ReadCsenetTransactionEnvelop()
  {
    entities.CsenetTransactionEnvelop.Populated = false;

    return Read("ReadCsenetTransactionEnvelop",
      (db, command) =>
      {
        db.SetDate(
          command, "ccaTransactionDt",
          entities.GetNextInterstateCase.TransactionDate.GetValueOrDefault());
        db.SetInt64(
          command, "ccaTransSerNum",
          entities.GetNextInterstateCase.TransSerialNumber);
      },
      (db, reader) =>
      {
        entities.CsenetTransactionEnvelop.CcaTransactionDt =
          db.GetDate(reader, 0);
        entities.CsenetTransactionEnvelop.CcaTransSerNum =
          db.GetInt64(reader, 1);
        entities.CsenetTransactionEnvelop.LastUpdatedBy =
          db.GetString(reader, 2);
        entities.CsenetTransactionEnvelop.LastUpdatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.CsenetTransactionEnvelop.DirectionInd =
          db.GetString(reader, 4);
        entities.CsenetTransactionEnvelop.ProcessingStatusCode =
          db.GetString(reader, 5);
        entities.CsenetTransactionEnvelop.Populated = true;
      });
  }

  private bool ReadFips()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.SetInt32(command, "state", local.FindState.State);
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.StateAbbreviation = db.GetString(reader, 3);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadFundTransaction1()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.Associated.Populated = false;

    return Read("ReadFundTransaction1",
      (db, command) =>
      {
        db.SetInt32(
          command, "fundTransId",
          entities.CashReceipt.FtrIdentifier.GetValueOrDefault());
        db.SetInt32(
          command, "funIdentifier",
          entities.CashReceipt.FunIdentifier.GetValueOrDefault());
        db.SetInt32(
          command, "fttIdentifier",
          entities.CashReceipt.FttIdentifier.GetValueOrDefault());
        db.SetDate(
          command, "pcaEffectiveDate",
          entities.CashReceipt.PcaEffectiveDate.GetValueOrDefault());
        db.SetString(command, "pcaCode", entities.CashReceipt.PcaCode ?? "");
      },
      (db, reader) =>
      {
        entities.Associated.FttIdentifier = db.GetInt32(reader, 0);
        entities.Associated.PcaCode = db.GetString(reader, 1);
        entities.Associated.PcaEffectiveDate = db.GetDate(reader, 2);
        entities.Associated.FunIdentifier = db.GetInt32(reader, 3);
        entities.Associated.SystemGeneratedIdentifier = db.GetInt32(reader, 4);
        entities.Associated.Amount = db.GetDecimal(reader, 5);
        entities.Associated.BusinessDate = db.GetDate(reader, 6);
        entities.Associated.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.Associated.LastUpdatedTmst = db.GetNullableDateTime(reader, 8);
        entities.Associated.Populated = true;
      });
  }

  private bool ReadFundTransaction2()
  {
    entities.Associated.Populated = false;

    return Read("ReadFundTransaction2",
      null,
      (db, reader) =>
      {
        entities.Associated.FttIdentifier = db.GetInt32(reader, 0);
        entities.Associated.PcaCode = db.GetString(reader, 1);
        entities.Associated.PcaEffectiveDate = db.GetDate(reader, 2);
        entities.Associated.FunIdentifier = db.GetInt32(reader, 3);
        entities.Associated.SystemGeneratedIdentifier = db.GetInt32(reader, 4);
        entities.Associated.Amount = db.GetDecimal(reader, 5);
        entities.Associated.BusinessDate = db.GetDate(reader, 6);
        entities.Associated.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.Associated.LastUpdatedTmst = db.GetNullableDateTime(reader, 8);
        entities.Associated.Populated = true;
      });
  }

  private bool ReadInterstateApIdentification()
  {
    entities.InterstateApIdentification.Populated = false;

    return Read("ReadInterstateApIdentification",
      (db, command) =>
      {
        db.SetDate(
          command, "ccaTransactionDt",
          entities.GetNextInterstateCase.TransactionDate.GetValueOrDefault());
        db.SetInt64(
          command, "ccaTransSerNum",
          entities.GetNextInterstateCase.TransSerialNumber);
      },
      (db, reader) =>
      {
        entities.InterstateApIdentification.CcaTransactionDt =
          db.GetDate(reader, 0);
        entities.InterstateApIdentification.CcaTransSerNum =
          db.GetInt64(reader, 1);
        entities.InterstateApIdentification.Ssn =
          db.GetNullableString(reader, 2);
        entities.InterstateApIdentification.NameSuffix =
          db.GetNullableString(reader, 3);
        entities.InterstateApIdentification.NameFirst = db.GetString(reader, 4);
        entities.InterstateApIdentification.NameLast =
          db.GetNullableString(reader, 5);
        entities.InterstateApIdentification.MiddleName =
          db.GetNullableString(reader, 6);
        entities.InterstateApIdentification.Populated = true;
      });
  }

  private IEnumerable<bool> ReadInterstateCollectionInterstateCase()
  {
    entities.GetNextInterstateCollection.Populated = false;
    entities.GetNextInterstateCase.Populated = false;

    return ReadEach("ReadInterstateCollectionInterstateCase",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "dateOfPosting",
          local.CompareZero.DateOfPosting.GetValueOrDefault());
        db.SetNullableDecimal(
          command, "paymentAmount",
          local.CompareZero.PaymentAmount.GetValueOrDefault());
        db.SetNullableString(
          command, "actionReasonCode", local.Compare.ActionReasonCode ?? "");
        db.SetString(
          command, "functionalTypeCo", local.Compare.FunctionalTypeCode);
      },
      (db, reader) =>
      {
        entities.GetNextInterstateCollection.CcaTransactionDt =
          db.GetDate(reader, 0);
        entities.GetNextInterstateCase.TransactionDate = db.GetDate(reader, 0);
        entities.GetNextInterstateCollection.CcaTransSerNum =
          db.GetInt64(reader, 1);
        entities.GetNextInterstateCase.TransSerialNumber =
          db.GetInt64(reader, 1);
        entities.GetNextInterstateCollection.SystemGeneratedSequenceNum =
          db.GetInt32(reader, 2);
        entities.GetNextInterstateCollection.DateOfCollection =
          db.GetNullableDate(reader, 3);
        entities.GetNextInterstateCollection.DateOfPosting =
          db.GetNullableDate(reader, 4);
        entities.GetNextInterstateCollection.PaymentAmount =
          db.GetNullableDecimal(reader, 5);
        entities.GetNextInterstateCollection.PaymentSource =
          db.GetNullableString(reader, 6);
        entities.GetNextInterstateCase.OtherFipsState = db.GetInt32(reader, 7);
        entities.GetNextInterstateCase.FunctionalTypeCode =
          db.GetString(reader, 8);
        entities.GetNextInterstateCase.KsCaseId =
          db.GetNullableString(reader, 9);
        entities.GetNextInterstateCase.InterstateCaseId =
          db.GetNullableString(reader, 10);
        entities.GetNextInterstateCase.ActionReasonCode =
          db.GetNullableString(reader, 11);
        entities.GetNextInterstateCase.ApIdentificationInd =
          db.GetNullableInt32(reader, 12);
        entities.GetNextInterstateCollection.Populated = true;
        entities.GetNextInterstateCase.Populated = true;

        return true;
      });
  }

  private void UpdateCashReceipt()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);

    var receiptAmount =
      entities.CashReceipt.ReceiptAmount +
      local.Pass.PaymentAmount.GetValueOrDefault();
    var receiptDate = Now().Date;
    var depositReleaseDate = entities.Associated.BusinessDate;
    var balancedTimestamp = entities.Associated.LastUpdatedTmst;
    var totalNoncashTransactionAmount =
      entities.CashReceipt.TotalNoncashTransactionAmount.GetValueOrDefault() +
      local.Pass.PaymentAmount.GetValueOrDefault();
    var totalNoncashTransactionCount =
      entities.CashReceipt.TotalNoncashTransactionCount.GetValueOrDefault() + 1;
      
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.CashReceipt.Populated = false;
    Update("UpdateCashReceipt",
      (db, command) =>
      {
        db.SetDecimal(command, "receiptAmount", receiptAmount);
        db.SetDate(command, "receiptDate", receiptDate);
        db.SetNullableDate(command, "depositRlseDt", depositReleaseDate);
        db.SetNullableDateTime(command, "balTmst", balancedTimestamp);
        db.SetNullableDecimal(
          command, "totNoncshTrnAmt", totalNoncashTransactionAmount);
        db.SetNullableInt32(
          command, "totNocshTranCnt", totalNoncashTransactionCount);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.
          SetInt32(command, "crvIdentifier", entities.CashReceipt.CrvIdentifier);
          
        db.
          SetInt32(command, "cstIdentifier", entities.CashReceipt.CstIdentifier);
          
        db.
          SetInt32(command, "crtIdentifier", entities.CashReceipt.CrtIdentifier);
          
      });

    entities.CashReceipt.ReceiptAmount = receiptAmount;
    entities.CashReceipt.ReceiptDate = receiptDate;
    entities.CashReceipt.DepositReleaseDate = depositReleaseDate;
    entities.CashReceipt.BalancedTimestamp = balancedTimestamp;
    entities.CashReceipt.TotalNoncashTransactionAmount =
      totalNoncashTransactionAmount;
    entities.CashReceipt.TotalNoncashTransactionCount =
      totalNoncashTransactionCount;
    entities.CashReceipt.LastUpdatedBy = lastUpdatedBy;
    entities.CashReceipt.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CashReceipt.Populated = true;
  }

  private void UpdateCashReceiptEvent()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptEvent.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();
    var totalNonCashTransactionCount =
      entities.CashReceiptEvent.TotalNonCashTransactionCount.
        GetValueOrDefault() + 1;
    var totalNonCashAmt =
      entities.CashReceiptEvent.TotalNonCashAmt.GetValueOrDefault() +
      local.Pass.PaymentAmount.GetValueOrDefault();

    entities.CashReceiptEvent.Populated = false;
    Update("UpdateCashReceiptEvent",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableInt32(
          command, "totNonCshtrnCnt", totalNonCashTransactionCount);
        db.SetNullableDecimal(command, "totNonCashAmt", totalNonCashAmt);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptEvent.CstIdentifier);
        db.SetInt32(
          command, "creventId",
          entities.CashReceiptEvent.SystemGeneratedIdentifier);
      });

    entities.CashReceiptEvent.LastUpdatedBy = lastUpdatedBy;
    entities.CashReceiptEvent.LastUpdatedTmst = lastUpdatedTmst;
    entities.CashReceiptEvent.TotalNonCashTransactionCount =
      totalNonCashTransactionCount;
    entities.CashReceiptEvent.TotalNonCashAmt = totalNonCashAmt;
    entities.CashReceiptEvent.Populated = true;
  }

  private void UpdateCsenetTransactionEnvelop()
  {
    System.Diagnostics.Debug.
      Assert(entities.CsenetTransactionEnvelop.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();
    var processingStatusCode = "P";

    entities.CsenetTransactionEnvelop.Populated = false;
    Update("UpdateCsenetTransactionEnvelop",
      (db, command) =>
      {
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTimes", lastUpdatedTimestamp);
        db.SetString(command, "processingStatus", processingStatusCode);
        db.SetDate(
          command, "ccaTransactionDt",
          entities.CsenetTransactionEnvelop.CcaTransactionDt.
            GetValueOrDefault());
        db.SetInt64(
          command, "ccaTransSerNum",
          entities.CsenetTransactionEnvelop.CcaTransSerNum);
      });

    entities.CsenetTransactionEnvelop.LastUpdatedBy = lastUpdatedBy;
    entities.CsenetTransactionEnvelop.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.CsenetTransactionEnvelop.ProcessingStatusCode =
      processingStatusCode;
    entities.CsenetTransactionEnvelop.Populated = true;
  }

  private void UpdateFundTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.Associated.Populated);

    var amount =
      entities.Associated.Amount + local.Pass.PaymentAmount.GetValueOrDefault();
      
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();

    entities.Associated.Populated = false;
    Update("UpdateFundTransaction",
      (db, command) =>
      {
        db.SetDecimal(command, "amount", amount);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.
          SetInt32(command, "fttIdentifier", entities.Associated.FttIdentifier);
          
        db.SetString(command, "pcaCode", entities.Associated.PcaCode);
        db.SetDate(
          command, "pcaEffectiveDate",
          entities.Associated.PcaEffectiveDate.GetValueOrDefault());
        db.
          SetInt32(command, "funIdentifier", entities.Associated.FunIdentifier);
          
        db.SetInt32(
          command, "fundTransId",
          entities.Associated.SystemGeneratedIdentifier);
      });

    entities.Associated.Amount = amount;
    entities.Associated.LastUpdatedBy = lastUpdatedBy;
    entities.Associated.LastUpdatedTmst = lastUpdatedTmst;
    entities.Associated.Populated = true;
  }

  private void UpdateInterstateCollection()
  {
    System.Diagnostics.Debug.Assert(
      entities.GetNextInterstateCollection.Populated);

    var dateOfPosting = local.ProgramProcessingInfo.ProcessDate;

    entities.GetNextInterstateCollection.Populated = false;
    Update("UpdateInterstateCollection",
      (db, command) =>
      {
        db.SetNullableDate(command, "dateOfPosting", dateOfPosting);
        db.SetDate(
          command, "ccaTransactionDt",
          entities.GetNextInterstateCollection.CcaTransactionDt.
            GetValueOrDefault());
        db.SetInt64(
          command, "ccaTransSerNum",
          entities.GetNextInterstateCollection.CcaTransSerNum);
        db.SetInt32(
          command, "sysGeneratedId",
          entities.GetNextInterstateCollection.SystemGeneratedSequenceNum);
      });

    entities.GetNextInterstateCollection.DateOfPosting = dateOfPosting;
    entities.GetNextInterstateCollection.Populated = true;
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
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public CashReceiptDetail Next
    {
      get => next ??= new();
      set => next = value;
    }

    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
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
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
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
    /// A value of CashReceiptEvent.
    /// </summary>
    [JsonPropertyName("cashReceiptEvent")]
    public CashReceiptEvent CashReceiptEvent
    {
      get => cashReceiptEvent ??= new();
      set => cashReceiptEvent = value;
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

    /// <summary>
    /// A value of InterstateCollection.
    /// </summary>
    [JsonPropertyName("interstateCollection")]
    public InterstateCollection InterstateCollection
    {
      get => interstateCollection ??= new();
      set => interstateCollection = value;
    }

    private CashReceiptDetail next;
    private InterstateCase interstateCase;
    private ProgramControlTotal programControlTotal;
    private CashReceiptType cashReceiptType;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceipt cashReceipt;
    private InterstateCollection interstateCollection;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public CashReceiptDetail Next
    {
      get => next ??= new();
      set => next = value;
    }

    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
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
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
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
    /// A value of CashReceiptEvent.
    /// </summary>
    [JsonPropertyName("cashReceiptEvent")]
    public CashReceiptEvent CashReceiptEvent
    {
      get => cashReceiptEvent ??= new();
      set => cashReceiptEvent = value;
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

    /// <summary>
    /// A value of InterstateCollection.
    /// </summary>
    [JsonPropertyName("interstateCollection")]
    public InterstateCollection InterstateCollection
    {
      get => interstateCollection ??= new();
      set => interstateCollection = value;
    }

    private CashReceiptDetail next;
    private InterstateCase interstateCase;
    private ProgramControlTotal programControlTotal;
    private CashReceiptType cashReceiptType;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceipt cashReceipt;
    private InterstateCollection interstateCollection;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A ControlTotalsGroup group.</summary>
    [Serializable]
    public class ControlTotalsGroup
    {
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
      /// A value of ProgramControlTotal.
      /// </summary>
      [JsonPropertyName("programControlTotal")]
      public ProgramControlTotal ProgramControlTotal
      {
        get => programControlTotal ??= new();
        set => programControlTotal = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private Common common;
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
      public const int Capacity = 10;

      private ProgramError aaaGroupLocalErrorsDetailProgramError;
      private Standard aaaGroupLocalErrorsDetailStandard;
    }

    /// <summary>
    /// A value of CompareZero.
    /// </summary>
    [JsonPropertyName("compareZero")]
    public InterstateCollection CompareZero
    {
      get => compareZero ??= new();
      set => compareZero = value;
    }

    /// <summary>
    /// A value of Compare.
    /// </summary>
    [JsonPropertyName("compare")]
    public InterstateCase Compare
    {
      get => compare ??= new();
      set => compare = value;
    }

    /// <summary>
    /// A value of Test.
    /// </summary>
    [JsonPropertyName("test")]
    public Case1 Test
    {
      get => test ??= new();
      set => test = value;
    }

    /// <summary>
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
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
    /// A value of Save.
    /// </summary>
    [JsonPropertyName("save")]
    public InterstateCase Save
    {
      get => save ??= new();
      set => save = value;
    }

    /// <summary>
    /// A value of HardcodedDposited.
    /// </summary>
    [JsonPropertyName("hardcodedDposited")]
    public CashReceiptStatus HardcodedDposited
    {
      get => hardcodedDposited ??= new();
      set => hardcodedDposited = value;
    }

    /// <summary>
    /// A value of ProcessInterstateCollection.
    /// </summary>
    [JsonPropertyName("processInterstateCollection")]
    public InterstateCollection ProcessInterstateCollection
    {
      get => processInterstateCollection ??= new();
      set => processInterstateCollection = value;
    }

    /// <summary>
    /// A value of ProcessInterstateCase.
    /// </summary>
    [JsonPropertyName("processInterstateCase")]
    public InterstateCase ProcessInterstateCase
    {
      get => processInterstateCase ??= new();
      set => processInterstateCase = value;
    }

    /// <summary>
    /// A value of FindState.
    /// </summary>
    [JsonPropertyName("findState")]
    public Fips FindState
    {
      get => findState ??= new();
      set => findState = value;
    }

    /// <summary>
    /// A value of TranEnvelopeFound.
    /// </summary>
    [JsonPropertyName("tranEnvelopeFound")]
    public Common TranEnvelopeFound
    {
      get => tranEnvelopeFound ??= new();
      set => tranEnvelopeFound = value;
    }

    /// <summary>
    /// A value of BatchAlreadyProcessed.
    /// </summary>
    [JsonPropertyName("batchAlreadyProcessed")]
    public Common BatchAlreadyProcessed
    {
      get => batchAlreadyProcessed ??= new();
      set => batchAlreadyProcessed = value;
    }

    /// <summary>
    /// A value of SaveFormatDate.
    /// </summary>
    [JsonPropertyName("saveFormatDate")]
    public TextWorkArea SaveFormatDate
    {
      get => saveFormatDate ??= new();
      set => saveFormatDate = value;
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
    /// A value of FormatAmount.
    /// </summary>
    [JsonPropertyName("formatAmount")]
    public TextWorkArea FormatAmount
    {
      get => formatAmount ??= new();
      set => formatAmount = value;
    }

    /// <summary>
    /// A value of FormatDate.
    /// </summary>
    [JsonPropertyName("formatDate")]
    public TextWorkArea FormatDate
    {
      get => formatDate ??= new();
      set => formatDate = value;
    }

    /// <summary>
    /// A value of FormatName.
    /// </summary>
    [JsonPropertyName("formatName")]
    public ProgramError FormatName
    {
      get => formatName ??= new();
      set => formatName = value;
    }

    /// <summary>
    /// A value of ApIdFound.
    /// </summary>
    [JsonPropertyName("apIdFound")]
    public Common ApIdFound
    {
      get => apIdFound ??= new();
      set => apIdFound = value;
    }

    /// <summary>
    /// A value of Count.
    /// </summary>
    [JsonPropertyName("count")]
    public Common Count
    {
      get => count ??= new();
      set => count = value;
    }

    /// <summary>
    /// A value of AbendError.
    /// </summary>
    [JsonPropertyName("abendError")]
    public Common AbendError
    {
      get => abendError ??= new();
      set => abendError = value;
    }

    /// <summary>
    /// A value of AdjustmentsFound.
    /// </summary>
    [JsonPropertyName("adjustmentsFound")]
    public Common AdjustmentsFound
    {
      get => adjustmentsFound ??= new();
      set => adjustmentsFound = value;
    }

    /// <summary>
    /// A value of ErrorReportOpened.
    /// </summary>
    [JsonPropertyName("errorReportOpened")]
    public Common ErrorReportOpened
    {
      get => errorReportOpened ??= new();
      set => errorReportOpened = value;
    }

    /// <summary>
    /// A value of NullDate.
    /// </summary>
    [JsonPropertyName("nullDate")]
    public InterstateCollection NullDate
    {
      get => nullDate ??= new();
      set => nullDate = value;
    }

    /// <summary>
    /// A value of Pass.
    /// </summary>
    [JsonPropertyName("pass")]
    public InterstateCollection Pass
    {
      get => pass ??= new();
      set => pass = value;
    }

    /// <summary>
    /// A value of Implementation.
    /// </summary>
    [JsonPropertyName("implementation")]
    public DateWorkArea Implementation
    {
      get => implementation ??= new();
      set => implementation = value;
    }

    /// <summary>
    /// A value of SetCollectionType.
    /// </summary>
    [JsonPropertyName("setCollectionType")]
    public CollectionType SetCollectionType
    {
      get => setCollectionType ??= new();
      set => setCollectionType = value;
    }

    /// <summary>
    /// A value of NextFdsoId.
    /// </summary>
    [JsonPropertyName("nextFdsoId")]
    public CashReceiptDetail NextFdsoId
    {
      get => nextFdsoId ??= new();
      set => nextFdsoId = value;
    }

    /// <summary>
    /// A value of ReturnedCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("returnedCashReceiptSourceType")]
    public CashReceiptSourceType ReturnedCashReceiptSourceType
    {
      get => returnedCashReceiptSourceType ??= new();
      set => returnedCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of ReturnedCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("returnedCashReceiptEvent")]
    public CashReceiptEvent ReturnedCashReceiptEvent
    {
      get => returnedCashReceiptEvent ??= new();
      set => returnedCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of ReturnedCashReceipt.
    /// </summary>
    [JsonPropertyName("returnedCashReceipt")]
    public CashReceipt ReturnedCashReceipt
    {
      get => returnedCashReceipt ??= new();
      set => returnedCashReceipt = value;
    }

    /// <summary>
    /// A value of SetCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("setCashReceiptSourceType")]
    public CashReceiptSourceType SetCashReceiptSourceType
    {
      get => setCashReceiptSourceType ??= new();
      set => setCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of HardcodedCsenet.
    /// </summary>
    [JsonPropertyName("hardcodedCsenet")]
    public CashReceiptType HardcodedCsenet
    {
      get => hardcodedCsenet ??= new();
      set => hardcodedCsenet = value;
    }

    /// <summary>
    /// A value of TbdHardcodedSdso.
    /// </summary>
    [JsonPropertyName("tbdHardcodedSdso")]
    public InterstateCollection TbdHardcodedSdso
    {
      get => tbdHardcodedSdso ??= new();
      set => tbdHardcodedSdso = value;
    }

    /// <summary>
    /// A value of TbdHardcodedFdso.
    /// </summary>
    [JsonPropertyName("tbdHardcodedFdso")]
    public InterstateCollection TbdHardcodedFdso
    {
      get => tbdHardcodedFdso ??= new();
      set => tbdHardcodedFdso = value;
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
    /// Gets a value of ControlTotals.
    /// </summary>
    [JsonIgnore]
    public Array<ControlTotalsGroup> ControlTotals => controlTotals ??= new(
      ControlTotalsGroup.Capacity);

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
    /// A value of NumberOfErrors.
    /// </summary>
    [JsonPropertyName("numberOfErrors")]
    public Common NumberOfErrors
    {
      get => numberOfErrors ??= new();
      set => numberOfErrors = value;
    }

    /// <summary>
    /// A value of NumberOfSusp.
    /// </summary>
    [JsonPropertyName("numberOfSusp")]
    public Common NumberOfSusp
    {
      get => numberOfSusp ??= new();
      set => numberOfSusp = value;
    }

    /// <summary>
    /// A value of NumberOfRel.
    /// </summary>
    [JsonPropertyName("numberOfRel")]
    public Common NumberOfRel
    {
      get => numberOfRel ??= new();
      set => numberOfRel = value;
    }

    /// <summary>
    /// A value of Tbd.
    /// </summary>
    [JsonPropertyName("tbd")]
    public ProgramRun Tbd
    {
      get => tbd ??= new();
      set => tbd = value;
    }

    /// <summary>
    /// A value of ReportProcessing.
    /// </summary>
    [JsonPropertyName("reportProcessing")]
    public EabFileHandling ReportProcessing
    {
      get => reportProcessing ??= new();
      set => reportProcessing = value;
    }

    /// <summary>
    /// A value of ReportHandling.
    /// </summary>
    [JsonPropertyName("reportHandling")]
    public EabReportSend ReportHandling
    {
      get => reportHandling ??= new();
      set => reportHandling = value;
    }

    /// <summary>
    /// A value of SetCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("setCashReceiptEvent")]
    public CashReceiptEvent SetCashReceiptEvent
    {
      get => setCashReceiptEvent ??= new();
      set => setCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public CsePersonsWorkSet Blank
    {
      get => blank ??= new();
      set => blank = value;
    }

    private InterstateCollection compareZero;
    private InterstateCase compare;
    private Case1 test;
    private AbendData abendData;
    private CsePerson csePerson;
    private InterstateCase save;
    private CashReceiptStatus hardcodedDposited;
    private InterstateCollection processInterstateCollection;
    private InterstateCase processInterstateCase;
    private Fips findState;
    private Common tranEnvelopeFound;
    private Common batchAlreadyProcessed;
    private TextWorkArea saveFormatDate;
    private CsePersonsWorkSet csePersonsWorkSet;
    private TextWorkArea formatAmount;
    private TextWorkArea formatDate;
    private ProgramError formatName;
    private Common apIdFound;
    private Common count;
    private Common abendError;
    private Common adjustmentsFound;
    private Common errorReportOpened;
    private InterstateCollection nullDate;
    private InterstateCollection pass;
    private DateWorkArea implementation;
    private CollectionType setCollectionType;
    private CashReceiptDetail nextFdsoId;
    private CashReceiptSourceType returnedCashReceiptSourceType;
    private CashReceiptEvent returnedCashReceiptEvent;
    private CashReceipt returnedCashReceipt;
    private CashReceiptSourceType setCashReceiptSourceType;
    private CashReceiptType hardcodedCsenet;
    private InterstateCollection tbdHardcodedSdso;
    private InterstateCollection tbdHardcodedFdso;
    private ProgramError initialized;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramError nextId;
    private External passArea;
    private ExitStateWorkArea exitStateWorkArea;
    private Array<ControlTotalsGroup> controlTotals;
    private Array<AaaGroupLocalErrorsGroup> aaaGroupLocalErrors;
    private Common numberOfErrors;
    private Common numberOfSusp;
    private Common numberOfRel;
    private ProgramRun tbd;
    private EabFileHandling reportProcessing;
    private EabReportSend reportHandling;
    private CashReceiptEvent setCashReceiptEvent;
    private CsePersonsWorkSet blank;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    /// <summary>
    /// A value of FundTransactionType.
    /// </summary>
    [JsonPropertyName("fundTransactionType")]
    public FundTransactionType FundTransactionType
    {
      get => fundTransactionType ??= new();
      set => fundTransactionType = value;
    }

    /// <summary>
    /// A value of Associated.
    /// </summary>
    [JsonPropertyName("associated")]
    public FundTransaction Associated
    {
      get => associated ??= new();
      set => associated = value;
    }

    /// <summary>
    /// A value of GetNextInterstateCollection.
    /// </summary>
    [JsonPropertyName("getNextInterstateCollection")]
    public InterstateCollection GetNextInterstateCollection
    {
      get => getNextInterstateCollection ??= new();
      set => getNextInterstateCollection = value;
    }

    /// <summary>
    /// A value of GetNextInterstateCase.
    /// </summary>
    [JsonPropertyName("getNextInterstateCase")]
    public InterstateCase GetNextInterstateCase
    {
      get => getNextInterstateCase ??= new();
      set => getNextInterstateCase = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
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
    /// A value of PaReferral.
    /// </summary>
    [JsonPropertyName("paReferral")]
    public PaReferral PaReferral
    {
      get => paReferral ??= new();
      set => paReferral = value;
    }

    /// <summary>
    /// A value of InterstateApIdentification.
    /// </summary>
    [JsonPropertyName("interstateApIdentification")]
    public InterstateApIdentification InterstateApIdentification
    {
      get => interstateApIdentification ??= new();
      set => interstateApIdentification = value;
    }

    /// <summary>
    /// A value of CsenetTransactionEnvelop.
    /// </summary>
    [JsonPropertyName("csenetTransactionEnvelop")]
    public CsenetTransactionEnvelop CsenetTransactionEnvelop
    {
      get => csenetTransactionEnvelop ??= new();
      set => csenetTransactionEnvelop = value;
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
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    /// <summary>
    /// A value of InterstateCollection.
    /// </summary>
    [JsonPropertyName("interstateCollection")]
    public InterstateCollection InterstateCollection
    {
      get => interstateCollection ??= new();
      set => interstateCollection = value;
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

    private CsePersonAccount csePersonAccount;
    private CsePerson csePerson;
    private CaseRole caseRole;
    private FundTransactionType fundTransactionType;
    private FundTransaction associated;
    private InterstateCollection getNextInterstateCollection;
    private InterstateCase getNextInterstateCase;
    private Fips fips;
    private Case1 case1;
    private PaReferral paReferral;
    private InterstateApIdentification interstateApIdentification;
    private CsenetTransactionEnvelop csenetTransactionEnvelop;
    private ProgramControlTotal programControlTotal;
    private InterstateCase interstateCase;
    private InterstateCollection interstateCollection;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptType cashReceiptType;
    private CashReceiptEvent cashReceiptEvent;
    private ProgramRun programRun;
    private ProgramProcessingInfo programProcessingInfo;
    private CashReceipt cashReceipt;
  }
#endregion
}
