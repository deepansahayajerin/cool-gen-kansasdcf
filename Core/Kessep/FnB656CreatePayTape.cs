// Program: FN_B656_CREATE_PAY_TAPE, ID: 372720348, model: 746.
// Short name: SWEF656B
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
/// A program: FN_B656_CREATE_PAY_TAPE.
/// </para>
/// <para>
/// This skeleton is an example that uses:
/// A sequential driver file
/// An external to do a DB2 commit
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB656CreatePayTape: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B656_CREATE_PAY_TAPE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB656CreatePayTape(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB656CreatePayTape.
  /// </summary>
  public FnB656CreatePayTape(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***************************************************************************************
    // Initial Version - ??????
    // 09/24/99 SWSRKXD PR# 74957
    // For REFunds and ADVancements payee_name should be
    // read from Receipt_Refund and not ADABAS.
    // 04/24/00 SWSRKXD PR#93541
    // Do not send warrants to end dated addresses.
    // 09/12/00 SWSRKXD PR#103334
    // Performance change - remove redundant ADABAS calls.
    // 10/12/00 SWSRKXD PR#102420
    // Create AE Income notification records for 'FS'.
    // ('AF' payments are notified by B640 on a monthly basis).
    // 10/18/2000 - SWSRKXD - Rework production fix.
    // Change READ property to 'Select and Cursor'.
    // 11/28/2000 - SWSRKXD -  Display return code from ADABAS in error log.
    // 12/27/00 - SWSRKXD #107373 - Skip ADABAS calls to get name when payor 
    // does not exist
    // 12/27/00 - SWSRKXD #102272 - Change KPC phone numbers on warrant stub.
    // 01/08/01 - SWSRKXD #102272 - Changed FDSO phone number to (785)-296-5018
    // 01/30/01 - SWSRKXD WR263 -  Create 'KPC' payment status instead of DOA. 
    // Remove literal 'DOA' from PStep name.
    // 05/17/01 - SWSRKXD PR#119275 - Fix problem with refunds creating AE 
    // Income records.
    // 08/21/01 - SWSRGMF PR# 125746 - Fix problem with table exceeding limit.
    // 10/18/01 - SWSRGAV WR# 20183 - Create an additional error report to 
    // document 'new' errors only.
    // 11/21/03 - FANGMAN PR# 00193492 - Change read of dibursements to qualify 
    // on AR as well as warrant ID.
    // 03/10/09 - Arun Mathias CQ#8211 - Change the words in the message and 
    // also, change the word "Warrant" to "Payment"
    //                                   
    // in the external action block
    // SWEXF656.
    // 02/15/10 SWSRGAV  CQ#15235 - Write dollar amount of payment requests 
    // read, processed, and errored to the control report.
    // 05/05/11 SWSRGAV  CQ#26668 - Do not print FDSO message for F type 
    // collections.  Also, always
    // write the collection_type print_name on the warrant to identify the type 
    // of payment.
    // 12/03/14 SWDPLSS  CQ#46048 - Occurrences Exceed Maximum List Length.
    // Increased group size of local_group_dtl_crd and local_group_sum_crd from 
    // 2000 to 4000.
    // 02/12/16 SWSRGAV  CQ#47700 - Interstate Warrants should not error due to 
    // payee date of death.
    // ********************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.HardcodedExUra.SystemGeneratedIdentifier = 348;
    local.CloseInd.Flag = "Y";
    local.PaytapeAlreadyCreated.Flag = "N";
    local.InterfaceIncomeNotification.PersonProgram = "FS";
    local.InterfaceIncomeNotification.ProcessDate = local.NullDateWorkArea.Date;
    local.InterfaceIncomeNotification.AppliedToCode = "C";
    local.InterfaceIncomeNotification.ProgramAppliedTo = "NA";

    // ***********************************************************
    // 12/27/00 - SWSRKXD #102272 - Change KPC phone numbers on warrant stub.
    // **********************************************************
    // *** CQ#8211 Changes Begin Here ***
    local.OverflowMsgText1.Text80 =
      "If you have questions regarding this payment,";
    local.OverflowMsgText2.Text80 =
      "call 1-888-7-KS-CHILD (1-888-757-2445) or TTY 1-888-688-1666.";

    // *** CQ#8211 Changes End   Here ***
    local.FdsoMsgText.Index = 0;
    local.FdsoMsgText.CheckSize();

    local.FdsoMsgText.Update.FdsoMsgText1.Text80 =
      "Federal law permits adjustments to this Federal payment interception within";
      

    local.FdsoMsgText.Index = 1;
    local.FdsoMsgText.CheckSize();

    // *** CQ#8211 Changes Begin Here ***
    local.FdsoMsgText.Update.FdsoMsgText1.Text80 =
      "the next 6 years. Since the Kansas SRS, Child Support Enforcement Program, is";
      

    // *** CQ#8211 Changes End   Here ***
    local.FdsoMsgText.Index = 2;
    local.FdsoMsgText.CheckSize();

    // *** CQ#8211 Changes Begin Here ***
    local.FdsoMsgText.Update.FdsoMsgText1.Text80 =
      "releasing it to you now, part or all of this payment MAY be subject to recovery";
      

    // *** CQ#8211 Changes End   Here ***
    local.FdsoMsgText.Index = 3;
    local.FdsoMsgText.CheckSize();

    local.FdsoMsgText.Update.FdsoMsgText1.Text80 =
      "by SRS. Recovery methods include retaining of future Federal and State payments";
      

    local.FdsoMsgText.Index = 4;
    local.FdsoMsgText.CheckSize();

    local.FdsoMsgText.Update.FdsoMsgText1.Text80 =
      "that are applied to arrearage and/or repayment plans discussed with you.";
      

    local.FdsoMsgText.Index = 5;
    local.FdsoMsgText.CheckSize();

    // ***********************************************************
    // 12/27/00 - SWSRKXD #102272 - Change KPC phone numbers on warrant stub.
    // 01/08/00 - #102272 Change phone number to 296-5018.
    // **********************************************************
    // *** CQ#8211 Changes Begin Here ***
    local.FdsoMsgText.Update.FdsoMsgText1.Text80 =
      "Call 1-888-7-KS-CHILD (1-888-757-2445) or TTY 1-888-688-1666 with any questions.";
      

    // *** CQ#8211 Changes End   Here ***
    local.PayTape.FileInstruction = "WRITE";

    // *** Hard-Code Area
    local.HardcodedCrfee.SystemGeneratedIdentifier = 73;
    local.HardcodedCoagfee.SystemGeneratedIdentifier = 74;
    local.HardcodedRequested.SystemGeneratedIdentifier = 1;

    // -------------------------------------------------
    // 01/30/01 - SWSRKXD WR263 -  Change DOA to KPC.
    // -------------------------------------------------
    local.HardcodedKpc.SystemGeneratedIdentifier = 31;
    local.HardcodedFType.SequentialIdentifier = 3;
    local.HardcodedKType.SequentialIdentifier = 10;
    local.HardcodedRType.SequentialIdentifier = 11;
    local.HardcodedSType.SequentialIdentifier = 4;
    local.HardcodedTType.SequentialIdentifier = 19;
    local.HardcodedUType.SequentialIdentifier = 5;
    local.HardcodedYType.SequentialIdentifier = 25;
    local.HardcodedZType.SequentialIdentifier = 26;
    UseFnHardcodedDisbursementInfo();
    UseFnB656Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.UserId.Text8 = global.UserId;
    local.Process.Date = local.VoucherNo.ProcessDate;
    local.Current.Timestamp = Now();
    local.MaxDiscontinue.Date = UseCabSetMaximumDiscontinueDate();

    // --------------------------------------------------------------------------------------------
    // -- 10/18/01 GVandy.  Establish a cutoff time to determine whether errors 
    // encountered
    // processing a warrant will be written to the daily (i.e. new error) error 
    // report.  Only
    // warrants created after 12:00 noon will be written to the daily report.
    // --------------------------------------------------------------------------------------------
    local.DailyErrorReportCutoff.Date = local.VoucherNo.ProcessDate;
    local.DailyErrorReportCutoff.Time =
      StringToTime("12 PM").GetValueOrDefault();
    UseFnBuildTimestampFrmDateTime();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // : Obtain currency on KPC status for subsequent processing.
      // -------------------------------------------------
      // 01/30/01 - SWSRKXD WR263 -  Change DOA to KPC.
      // -------------------------------------------------
      if (!ReadPaymentStatus())
      {
        ExitState = "PAYMENT_STATUS_NF";

        goto Test3;
      }

      foreach(var item in ReadWarrant())
      {
        // -- Set a flag to indicate if this is newly created warrant.  The flag
        // is passed to the print error cab and will determine whether the
        // error is also written to the daily (i.e. new error) error report.
        if (!Lt(entities.ExistingWarrant.CreatedTimestamp,
          local.DailyErrorReportCutoff.Timestamp))
        {
          local.NewWarrant.Flag = "Y";
        }
        else
        {
          local.NewWarrant.Flag = "N";
        }

        if (AsChar(local.RunInTestMode.Flag) != 'Y')
        {
          if (!ReadPaymentStatusHistory())
          {
            continue;
          }
        }

        ++local.NoOfPymntReqRead.Count;
        local.NoOfPymntReqRead.TotalCurrency += entities.ExistingWarrant.Amount;
        local.AmtOfPymntReqRead.Amount += entities.ExistingWarrant.Amount;
        local.WarrantMailingAddr.Assign(local.NullWarrantMailingAddr);
        local.InterstatWarrant.Assign(local.NullInterstatWarrant);
        local.PrintFdsoMsgInd.Flag = "N";
        local.FipsRetrievedInd.Flag = "N";

        if (entities.ExistingWarrant.Amount <= 0)
        {
          ExitState = "FN0000_AMT_CANT_BE_ZERO_OR_NEG";
          UseFnB656PrintErrorLine3();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          continue;
        }

        // : Determine Payee & Designated Payee.
        //   Verify Person Number and get the formatted name.
        if (Equal(entities.ExistingWarrant.Classification, "REF") || Equal
          (entities.ExistingWarrant.Classification, "ADV"))
        {
          // **** REFUND OR ADVANCEMENT ****
          // -------------------------------------------------------------
          // 09/24/99 PR# H74957  SWSRKXD
          // Moved READ here from below (IF stmt for warrant stub details).
          // --------------------------------------------------------------
          if (!ReadReceiptRefund())
          {
            ExitState = "FN0000_RCPT_REFUND_NF";
            UseFnB656PrintErrorLine3();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            continue;
          }

          // -----------------------------------------------
          // 09/24/99 PR# H74957  SWSRKXD
          // For refunds and advances, print payee_name from
          // receipt_refund.
          // Per Lori Glissman and Sunya Sharp, there should
          // always be a name entered. Error off records w/o
          // payee_name.
          // ----------------------------------------------
          if (IsEmpty(entities.ExistingReceiptRefund.PayeeName))
          {
            ExitState = "FN0000_PAYEE_NF_ON_RCPT_REFUND";
            UseFnB656PrintErrorLine3();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            continue;
          }

          if (IsEmpty(entities.ExistingWarrant.CsePersonNumber))
          {
            // -----------------------------------------------
            // 09/24/99 PR# H74957  SWSRKXD
            // Okay to not have cse_person_nbr set.
            // ----------------------------------------------
            local.WarrantPayeeCsePerson.Assign(local.NullCsePerson);
            local.WarrantPayeeCsePersonsWorkSet.Number = "";
          }
          else
          {
            local.WarrantPayeeCsePerson.Number =
              entities.ExistingWarrant.CsePersonNumber ?? Spaces(10);
            local.WarrantPayeeCsePersonsWorkSet.Number =
              entities.ExistingWarrant.CsePersonNumber ?? Spaces(10);
            UseSiReadCsePersonBatch2();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "FN0000_PAYEE_NOT_FOUND";
              UseFnB656PrintErrorLine3();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }

              continue;
            }
          }

          local.WarrantPayeeCsePersonsWorkSet.FormattedName =
            entities.ExistingReceiptRefund.PayeeName ?? Spaces(33);
          local.WarrantPayeePrint.Assign(local.WarrantPayeeCsePersonsWorkSet);

          // -----------------------------------------------
          // 09/24/99 PR# H74957  SWSRKXD
          // Set Designated Payee views to Nulls.
          // ----------------------------------------------
          local.UseDesignatedPayeeInd.Flag = "N";
          local.WarrantStubForCsePerson.Assign(local.NullCsePerson);
          local.WarrantStubForCsePersonsWorkSet.Assign(
            local.NullCsePersonsWorkSet);
        }
        else
        {
          if (Equal(entities.ExistingWarrant.CsePersonNumber,
            entities.ExistingWarrant.DesignatedPayeeCsePersonNo) || IsEmpty
            (entities.ExistingWarrant.DesignatedPayeeCsePersonNo))
          {
            // : No Active Designated Payee for the Payee.
            local.UseDesignatedPayeeInd.Flag = "N";
            local.WarrantPayeeCsePerson.Assign(local.NullCsePerson);
            local.WarrantPayeeCsePersonsWorkSet.Assign(
              local.NullCsePersonsWorkSet);
            local.WarrantPayeeCsePersonsWorkSet.Number =
              entities.ExistingWarrant.CsePersonNumber ?? Spaces(10);
            UseSiReadCsePersonBatch2();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "FN0000_PAYEE_NOT_FOUND";
              UseFnB656PrintErrorLine3();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }

              continue;
            }

            local.WarrantStubForCsePerson.Assign(local.NullCsePerson);
            local.WarrantStubForCsePersonsWorkSet.Assign(
              local.NullCsePersonsWorkSet);
          }
          else
          {
            local.UseDesignatedPayeeInd.Flag = "Y";
            local.WarrantPayeeCsePerson.Assign(local.NullCsePerson);
            local.WarrantPayeeCsePersonsWorkSet.Assign(
              local.NullCsePersonsWorkSet);
            local.WarrantPayeeCsePersonsWorkSet.Number =
              entities.ExistingWarrant.DesignatedPayeeCsePersonNo ?? Spaces
              (10);
            UseSiReadCsePersonBatch2();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "FN0000_DESIG_PAYEE_NF";
              UseFnB656PrintErrorLine3();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }

              continue;
            }

            local.WarrantStubForCsePersonsWorkSet.Number =
              entities.ExistingWarrant.CsePersonNumber ?? Spaces(10);
            UseSiReadCsePersonBatch3();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "FN0000_PAYEE_NOT_FOUND";
              UseFnB656PrintErrorLine3();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }

              continue;
            }
          }

          local.WarrantPayeePrint.Assign(local.WarrantPayeeCsePersonsWorkSet);
        }

        // 02/12/16 SWSRGAV  CQ#47700 - Interstate Warrants should not error due
        // to payee date of death.
        // This logic was moved further below so as to not execute when the 
        // warrant is interstate.
        // : Get the Warrant Address.
        if (Equal(entities.ExistingWarrant.Classification, "REF") || Equal
          (entities.ExistingWarrant.Classification, "ADV"))
        {
          // **** REFUND OR ADVANCEMENT ****
          if (Lt(local.NullDateWorkArea.Date,
            local.WarrantPayeeCsePerson.DateOfDeath))
          {
            // 02/12/16 SWSRGAV  CQ#47700 - Interstate Warrants should not error
            // due to payee date of death.  This logic was moved from up above.
            // ***********************************************************
            // 12/17/2014 SWDPDJD CQ#45988
            // Do not send warrants to a payee or designated payee who
            // is deceased.
            // ************************************************************
            ExitState = "FN0000_PAYEE_DESIGN_PAYEE_DECEAS";
            UseFnB656PrintErrorLine3();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            continue;
          }

          // : Get the address information from Cash Receipt Detail Address.
          //   If it is not found, get the Address of the Warrant Payee.
          if (ReadCashReceiptDetailAddress())
          {
            // <<< Move the CRD Address to the Local_Warrant_Mailing_Address >>>
            local.WarrantMailingAddr.LocationType = "D";
            local.WarrantMailingAddr.City =
              entities.ExistingCashReceiptDetailAddress.City;
            local.WarrantMailingAddr.State =
              entities.ExistingCashReceiptDetailAddress.State;
            local.WarrantMailingAddr.Street1 =
              entities.ExistingCashReceiptDetailAddress.Street1;
            local.WarrantMailingAddr.Street2 =
              entities.ExistingCashReceiptDetailAddress.Street2;
            local.WarrantMailingAddr.ZipCode =
              entities.ExistingCashReceiptDetailAddress.ZipCode5;
            local.WarrantMailingAddr.Zip4 =
              entities.ExistingCashReceiptDetailAddress.ZipCode4;
          }
          else
          {
            UseSiGetCsePersonMailingAddr();

            if (IsEmpty(local.WarrantMailingAddr.LocationType))
            {
              ExitState = "ADDRESS_NF";
              UseFnB656PrintErrorLine3();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }

              continue;

              // ***********************************************************
              // 04/24/00 SWSRKXD PR#93541
              // Do not send warrants to end dated addresses.
              // ************************************************************
            }
            else if (Lt(local.WarrantMailingAddr.EndDate, Now().Date))
            {
              ExitState = "FN0000_ACTIVE_ADDR_SSN_DOB_NF";
              UseFnB656PrintErrorLine3();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }

              continue;
            }
          }
        }
        else if (AsChar(entities.ExistingWarrant.InterstateInd) == 'Y')
        {
          // **** INTERSTATE WARRANT ****
          if (!ReadInterstateRequest())
          {
            ExitState = "INTERSTATE_REQUEST_NF";
            UseFnB656PrintErrorLine3();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            continue;
          }

          // ----------------------------------------------
          // SWSRKXD - 10/18/2000
          // Rework production fix. Change READ property to 'Select and Cursor'.
          // ----------------------------------------------
          if (ReadInterstatePaymentAddress())
          {
            // <<< Move the Interstate Payment Address to the local Warrant 
            // Mailing Address >>>
            local.WarrantPayeePrint.FormattedName =
              entities.ExistingInterstatePaymentAddress.PayableToName ?? Spaces
              (33);
            local.WarrantMailingAddr.LocationType =
              entities.ExistingInterstatePaymentAddress.LocationType;
            local.WarrantMailingAddr.Street1 =
              entities.ExistingInterstatePaymentAddress.Street1;
            local.WarrantMailingAddr.Street2 =
              entities.ExistingInterstatePaymentAddress.Street2;
            local.WarrantMailingAddr.Street3 =
              entities.ExistingInterstatePaymentAddress.Street3;
            local.WarrantMailingAddr.Street4 =
              entities.ExistingInterstatePaymentAddress.Street4;
            local.WarrantMailingAddr.City =
              entities.ExistingInterstatePaymentAddress.City;
            local.WarrantMailingAddr.County =
              entities.ExistingInterstatePaymentAddress.County;
            local.WarrantMailingAddr.State =
              entities.ExistingInterstatePaymentAddress.State;
            local.WarrantMailingAddr.Province =
              entities.ExistingInterstatePaymentAddress.Province;
            local.WarrantMailingAddr.ZipCode =
              entities.ExistingInterstatePaymentAddress.ZipCode;
            local.WarrantMailingAddr.Zip4 =
              entities.ExistingInterstatePaymentAddress.Zip4;
            local.WarrantMailingAddr.PostalCode =
              entities.ExistingInterstatePaymentAddress.PostalCode;
            local.WarrantMailingAddr.Country =
              entities.ExistingInterstatePaymentAddress.Country;
            local.WarrantMailingAddr.EndDate =
              entities.ExistingInterstatePaymentAddress.AddressEndDate;
          }
          else
          {
            ExitState = "FN0000_INTERSTATE_ADDRESS_NF";
            UseFnB656PrintErrorLine3();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              break;
            }

            continue;
          }

          if (ReadCase())
          {
            local.InterstatWarrant.CourtOrderNo = entities.ExistingCase.Number;
            local.InterstatWarrant.OtherStateCaseNo =
              entities.ExistingInterstateRequest.OtherStateCaseId ?? Spaces
              (20);
            local.InterstatWarrant.MedicalInd = "N";

            foreach(var item1 in ReadCaseRole())
            {
              if (AsChar(local.InterstatWarrant.MedicalInd) == 'Y')
              {
                local.InterstatWarrant.MedicalInd = "Y";

                break;
              }
            }
          }
          else
          {
            ExitState = "FN0000_INVALID_INTERSTATE_IND";
            UseFnB656PrintErrorLine3();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            continue;
          }
        }
        else
        {
          // **** STANDARD SUPPORT WARRANT ****
          // 02/12/16 SWSRGAV  CQ#47700 - Interstate Warrants should not error 
          // due to payee date of death.  This logic was moved from up above.
          if (Lt(local.NullDateWorkArea.Date,
            local.WarrantPayeeCsePerson.DateOfDeath))
          {
            // ***********************************************************
            // 12/17/2014 SWDPDJD CQ#45988
            // Do not send warrants to a payee or designated payee who
            // is deceased.
            // ************************************************************
            ExitState = "FN0000_PAYEE_DESIGN_PAYEE_DECEAS";
            UseFnB656PrintErrorLine3();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            continue;
          }

          UseSiGetCsePersonMailingAddr();

          if (IsEmpty(local.WarrantMailingAddr.LocationType))
          {
            ExitState = "ADDRESS_NF";
            UseFnB656PrintErrorLine3();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            continue;
          }
          else if (Lt(local.WarrantMailingAddr.EndDate, Now().Date))
          {
            if (AsChar(local.UseDesignatedPayeeInd.Flag) == 'Y')
            {
              if (IsEmpty(local.WarrantPayeeCsePersonsWorkSet.Ssn) || !
                Lt("000000000", local.WarrantPayeeCsePersonsWorkSet.Ssn) || !
                Lt(local.NullDateWorkArea.Date,
                local.WarrantPayeeCsePersonsWorkSet.Dob))
              {
                // ***********************************************************
                // 12/18/2014 SWDPDJD CQ#45988
                // Do not send warrants to end dated addresses and the payee
                // does not have either a SSN or DOB.
                // ************************************************************
                ExitState = "FN0000_ACTIVE_ADDR_SSN_DOB_NF";
                UseFnB656PrintErrorLine3();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  return;
                }

                continue;
              }
            }
            else if (IsEmpty(local.WarrantPayeeCsePersonsWorkSet.Ssn) || !
              Lt("000000000", local.WarrantPayeeCsePersonsWorkSet.Ssn) || !
              Lt(local.NullDateWorkArea.Date,
              local.WarrantPayeeCsePersonsWorkSet.Dob))
            {
              // ***********************************************************
              // 12/18/2014 SWDPDJD CQ#45988
              // Do not send warrants to end dated addresses and the payee
              // does not have either a SSN or DOB.
              // ************************************************************
              ExitState = "FN0000_ACTIVE_ADDR_SSN_DOB_NF";
              UseFnB656PrintErrorLine3();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }

              continue;
            }
          }
        }

        // : Process the Warrant Stub Details.
        // -----------------------------------------------
        // 05/17/01 PR# 119275  SWSRKXD
        // Initialize work view of na_current_supp so that no income
        // record is created for refunds.
        // ----------------------------------------------
        local.NaCurrentSuppForWarr.TotalCurrency = 0;
        local.SuppPersons.Index = -1;
        local.SuppPersons.Count = 0;
        local.LatestCollectionDate.Date = local.NullDateWorkArea.Date;

        if (Equal(entities.ExistingWarrant.Classification, "REF") || Equal
          (entities.ExistingWarrant.Classification, "ADV"))
        {
          // -----------------------------------------------
          // 09/24/99 PR# H74957  SWSRKXD
          // Move Read of receipt_refund into the prev IF statement.
          // ----------------------------------------------
        }
        else
        {
          local.DtlCrd.Index = -1;
          local.DtlCrd.Count = 0;
          local.TotalDisbTrans.TotalCurrency = 0;

          foreach(var item1 in ReadDisbursementTransaction2())
          {
            local.TotalDisbTrans.TotalCurrency += entities.ExistingDebit.Amount;

            if (ReadDisbursementTransaction1())
            {
              if (AsChar(local.RunInTestMode.Flag) == 'Y')
              {
                // *****************  temp test print  ****************
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail = "";
                UseCabErrorReport();
                local.EabReportSend.RptDetail = "  CR - AR " + entities
                  .ExistingWarrant.CsePersonNumber + " Ref # " + entities
                  .ExistingCredit.ReferenceNumber + " Disb ID " + NumberToString
                  (entities.ExistingCredit.SystemGeneratedIdentifier, 7, 9) + " Amt " +
                  NumberToString
                  ((long)(entities.ExistingCredit.Amount * 100), 7, 9);
                UseCabErrorReport();

                // *****************  temp test print  ****************
              }
            }
            else
            {
              ExitState = "FN0000_CREDIT_DISB_TRAN_NF";
              UseFnB656PrintErrorLine3();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }

              goto ReadEach;
            }

            if (local.DtlCrd.Index + 1 >= Local.DtlCrdGroup.Capacity)
            {
              ExitState = "ACO_NI0000_LIST_EXCEED_MAX_LNGTH";
              local.TextMsg.Text30 = "crd table " + NumberToString
                (local.DtlCrd.Index + 1, 11, 5);
              UseFnB656PrintErrorLine1();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }

              goto ReadEach;
            }

            ++local.DtlCrd.Index;
            local.DtlCrd.CheckSize();

            MoveDisbursementTransaction1(entities.ExistingCredit,
              local.DtlCrd.Update.DtlCrdDisbursementTransaction);

            if (AsChar(entities.ExistingCredit.Type1) == 'C')
            {
              if (ReadCollectionCollectionType())
              {
                MoveCollection(entities.ExistingCollection,
                  local.DtlCrd.Update.DtlCrdCollection);
                local.DtlCrd.Update.DtlCrdCollectionType.PrintName =
                  entities.ExistingCollectionType.PrintName;

                if (ReadCsePerson2())
                {
                  local.DtlCrd.Update.DtlCrdPayor.Number =
                    entities.ExistingPayor.Number;
                }
                else
                {
                  ExitState = "FN0000_AP_PAYOR_NF";
                }
              }
              else
              {
                ExitState = "FN0000_COLLECTION_TYPE_NF";
              }

              if (AsChar(entities.ExistingWarrant.InterstateInd) == 'Y')
              {
                if (AsChar(local.FipsRetrievedInd.Flag) == 'N')
                {
                  local.FipsRetrievedInd.Flag = "Y";

                  if (ReadCashReceiptSourceType())
                  {
                    local.TmpFips.TotalInteger =
                      entities.ExistingCashReceiptSourceType.State.
                        GetValueOrDefault() * 100000 + entities
                      .ExistingCashReceiptSourceType.County.
                        GetValueOrDefault() * 100 + (
                        long)entities.ExistingCashReceiptSourceType.Location.
                        GetValueOrDefault();

                    if (local.TmpFips.TotalInteger == 0)
                    {
                      local.InterstatWarrant.Fips = "N/A";
                    }
                    else
                    {
                      local.InterstatWarrant.Fips =
                        NumberToString(local.TmpFips.TotalInteger, 8);
                      local.InterstatWarrant.Fips =
                        Substring(local.InterstatWarrant.Fips, 2, 7);
                    }
                  }
                  else
                  {
                    ExitState = "CASH_RECEIPT_SOURCE_TYPE_NF";
                  }
                }
              }
              else
              {
                local.DtlCrd.Update.DtlCrdPayor.Ssn = "";
              }

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                UseFnB656PrintErrorLine3();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  return;
                }

                goto ReadEach;
              }

              if (entities.ExistingCollectionType.SequentialIdentifier == local
                .HardcodedFType.SequentialIdentifier)
              {
                // 05/05/11 SWSRGAV  CQ#26668 - Do not print FDSO message for F 
                // type collections.  Disabling the statement below
                // will suppress the message.  The framework for creating the 
                // message is left intact in case a message is needed in the
                // future.
              }
            }
            else
            {
              local.DtlCrd.Update.DtlCrdPayor.
                Assign(local.NullCsePersonsWorkSet);
              local.DtlCrd.Update.DtlCrdCollection.CourtOrderAppliedTo =
                local.NullCollection.CourtOrderAppliedTo;
              local.DtlCrd.Update.DtlCrdCollectionType.PrintName =
                local.NullCollectionType.PrintName;
            }

            local.DtlCrd.Item.DtlDbt.Index = -1;
            local.DtlCrd.Item.DtlDbt.Count = 0;

            foreach(var item2 in ReadDisbursementTransactionDisbursementType())
            {
              if (AsChar(local.RunInTestMode.Flag) == 'Y')
              {
                // *****************  temp test print  ****************
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail = "  DB - AR " + entities
                  .ExistingWarrant.CsePersonNumber + " Ref # " + entities
                  .ExistingDebitDtl.ReferenceNumber + " Disb ID " + NumberToString
                  (entities.ExistingDebitDtl.SystemGeneratedIdentifier, 7, 9) +
                  " Amt " + NumberToString
                  ((long)(entities.ExistingDebitDtl.Amount * 100), 7, 9);
                UseCabErrorReport();

                // *****************  temp test print  ****************
              }

              if (local.DtlCrd.Item.DtlDbt.Index + 1 >= Local
                .DtlDbtGroup.Capacity)
              {
                ExitState = "ACO_NI0000_LIST_EXCEED_MAX_LNGTH";
                local.TextMsg.Text30 = "dbt table " + NumberToString
                  (local.DtlCrd.Item.DtlDbt.Index + 1, 11, 5);
                UseFnB656PrintErrorLine1();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  return;
                }

                goto ReadEach;
              }

              ++local.DtlCrd.Item.DtlDbt.Index;
              local.DtlCrd.Item.DtlDbt.CheckSize();

              local.DtlCrd.Update.DtlDbt.Update.DtlDbtDisbursementTransaction.
                Assign(entities.ExistingDebitDtl);
              local.DtlCrd.Update.DtlDbt.Update.DtlDbtDisbursementType.Assign(
                entities.ExistingDisbursementType);

              if (entities.ExistingDebit.SystemGeneratedIdentifier == entities
                .ExistingDebitDtl.SystemGeneratedIdentifier)
              {
                local.DtlCrd.Update.DtlDbt.Update.DtlDbtPayoutInd.Flag = "Y";
              }
              else
              {
                local.DtlCrd.Update.DtlDbt.Update.DtlDbtPayoutInd.Flag = "N";
              }

              if (entities.ExistingDisbursementType.SystemGeneratedIdentifier ==
                local.HardcodedPt.SystemGeneratedIdentifier)
              {
                local.DtlCrd.Update.DtlDbt.Update.DtlDbtDisbursementType.Code =
                  "";
                local.DtlCrd.Update.DtlDbt.Update.DtlDbtDisbursementType.Name =
                  "";
              }

              // ***********************************************************
              // 10/12/00 SWSRKXD PR#102420
              // Create AE Income notification records for 'FS'.
              // ('AF' payments are notified by B640 on a monthly basis).
              // **********************************************************
              if (entities.ExistingDisbursementType.SystemGeneratedIdentifier ==
                local.HardcodedNaCcs.SystemGeneratedIdentifier || entities
                .ExistingDisbursementType.SystemGeneratedIdentifier == local
                .HardcodedNaCsp.SystemGeneratedIdentifier)
              {
                if (!ReadCsePerson1())
                {
                  ExitState = "CSE_PERSON_NF";
                  UseFnB656PrintErrorLine3();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    return;
                  }

                  goto ReadEach;
                }

                if (ReadProgram1())
                {
                  // ***********************************************************
                  // Already accounted for in B640. Bypass this record.
                  // **********************************************************
                  goto Test1;
                }
                else
                {
                  // ************************
                  // Now look for FS.
                  // ************************
                }

                if (!ReadProgram2())
                {
                  // ****************************
                  // Don't report this to AE.
                  // ****************************
                  goto Test1;
                }

                // *****************************************************
                // You are now looking at a CS disbursement (+ve or -ve)
                // for a supported person on FS.
                // ****************************************************
                // -------------------------------------------------------------
                // Build an array of all supp persons who received CS
                // disbursements as part of this warrant and also have an
                // active FS program (but no AF).
                // -------------------------------------------------------------
                local.PersonExistsInGrp.Flag = "N";

                for(local.SuppPersons.Index = 0; local.SuppPersons.Index < local
                  .SuppPersons.Count; ++local.SuppPersons.Index)
                {
                  if (!local.SuppPersons.CheckSize())
                  {
                    break;
                  }

                  // ------------------------------------------------------------
                  // Check to see if this person is already there in our array.
                  // ------------------------------------------------------------
                  if (Equal(local.SuppPersons.Item.SuppPerson.Number,
                    entities.Supported.Number))
                  {
                    local.PersonExistsInGrp.Flag = "Y";

                    break;
                  }
                }

                local.SuppPersons.CheckIndex();

                if (AsChar(local.PersonExistsInGrp.Flag) == 'N')
                {
                  if (local.SuppPersons.Count == Local
                    .SuppPersonsGroup.Capacity)
                  {
                    ExitState = "ACO_NI0000_LIST_EXCEED_MAX_LNGTH";
                    local.TextMsg.Text30 = "supp persons table " + NumberToString
                      (local.SuppPersons.Index + 1, 11, 5);
                    UseFnB656PrintErrorLine1();

                    if (!IsExitState("ACO_NN0000_ALL_OK"))
                    {
                      return;
                    }

                    goto ReadEach;
                  }

                  local.SuppPersons.Index = local.SuppPersons.Count;
                  local.SuppPersons.CheckSize();

                  local.SuppPersons.Update.SuppPerson.Number =
                    entities.Supported.Number;
                }

                local.NaCurrentSuppForWarr.TotalCurrency += entities.
                  ExistingDebit.Amount;

                if (Lt(local.LatestCollectionDate.Date,
                  entities.ExistingCollection.CollectionDt))
                {
                  local.LatestCollectionDate.Date =
                    entities.ExistingCollection.CollectionDt;
                }
              }

Test1:
              ;
            }
          }

          if (local.DtlCrd.IsEmpty)
          {
            ExitState = "FN0000_NO_WAR_STUB_DTLS_FND";
            UseFnB656PrintErrorLine3();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            continue;
          }

          // : Verify that the total of the Disbursement Debts with payout is 
          // equal to the Warrant Amount.
          if (local.TotalDisbTrans.TotalCurrency != entities
            .ExistingWarrant.Amount)
          {
            ExitState = "FN0000_WAR_DTLS_DO_NOT_ADD_UP";
            UseFnB656PrintErrorLine3();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            continue;
          }
        }

        // : Consolidate Disbursement Transactions.
        if (Equal(entities.ExistingWarrant.Classification, "REF") || Equal
          (entities.ExistingWarrant.Classification, "ADV"))
        {
          // : Continue Processing - Refunds & Advancements will be processed in
          // the next section.
        }
        else
        {
          local.SumCrd.Index = -1;
          local.SumCrd.Count = 0;
          local.DtlCrd.Index = 0;

          for(var limit = local.DtlCrd.Count; local.DtlCrd.Index < limit; ++
            local.DtlCrd.Index)
          {
            if (!local.DtlCrd.CheckSize())
            {
              break;
            }

            if (local.SumCrd.Index == -1)
            {
              local.SumCrd.Index = 0;
              local.SumCrd.CheckSize();

              local.SumCrd.Item.SumDbt.Index = -1;
              local.SumCrd.Item.SumDbt.Count = 0;
              local.SumCrd.Update.SumCrdDisbursementTransaction.Assign(
                local.DtlCrd.Item.DtlCrdDisbursementTransaction);
              local.SumCrd.Update.SumCrdPayor.Assign(
                local.DtlCrd.Item.DtlCrdPayor);
              MoveCollection(local.DtlCrd.Item.DtlCrdCollection,
                local.SumCrd.Update.SumCrdCollection);
              local.SumCrd.Update.SumCrdCollectionType.PrintName =
                local.DtlCrd.Item.DtlCrdCollectionType.PrintName;

              for(local.DtlCrd.Item.DtlDbt.Index = 0; local
                .DtlCrd.Item.DtlDbt.Index < local.DtlCrd.Item.DtlDbt.Count; ++
                local.DtlCrd.Item.DtlDbt.Index)
              {
                if (!local.DtlCrd.Item.DtlDbt.CheckSize())
                {
                  break;
                }

                local.SumCrd.Item.SumDbt.Index = local.DtlCrd.Item.DtlDbt.Index;
                local.SumCrd.Item.SumDbt.CheckSize();

                local.SumCrd.Update.SumDbt.Update.SumDbtDisbursementTransaction.
                  Assign(local.DtlCrd.Item.DtlDbt.Item.
                    DtlDbtDisbursementTransaction);
                local.SumCrd.Update.SumDbt.Update.SumDbtDisbursementType.Assign(
                  local.DtlCrd.Item.DtlDbt.Item.DtlDbtDisbursementType);
                local.SumCrd.Update.SumDbt.Update.SumDbtPayoutInd.Flag =
                  local.DtlCrd.Item.DtlDbt.Item.DtlDbtPayoutInd.Flag;
              }

              local.DtlCrd.Item.DtlDbt.CheckIndex();

              continue;
            }

            if (AsChar(local.DtlCrd.Item.DtlCrdDisbursementTransaction.Type1) ==
              'P' || AsChar
              (local.DtlCrd.Item.DtlCrdDisbursementTransaction.Type1) == 'X'
              || local
              .DtlCrd.Item.DtlDbt.Item.DtlDbtDisbursementType.
                SystemGeneratedIdentifier == local
              .HardcodedPt.SystemGeneratedIdentifier || local
              .DtlCrd.Item.DtlDbt.Item.DtlDbtDisbursementType.
                SystemGeneratedIdentifier == local
              .HardcodedExUra.SystemGeneratedIdentifier)
            {
              // : Do NOT consolidate transactions for Passthru or Excess URA.
            }
            else
            {
              local.SumCrd.Index = 0;

              for(var limit1 = local.SumCrd.Count; local.SumCrd.Index < limit1; ++
                local.SumCrd.Index)
              {
                if (!local.SumCrd.CheckSize())
                {
                  break;
                }

                if (Equal(local.DtlCrd.Item.DtlCrdDisbursementTransaction.
                  ReferenceNumber,
                  local.SumCrd.Item.SumCrdDisbursementTransaction.
                    ReferenceNumber) && Equal
                  (local.DtlCrd.Item.DtlCrdDisbursementTransaction.
                    CollectionDate,
                  local.SumCrd.Item.SumCrdDisbursementTransaction.
                    CollectionDate))
                {
                  local.SumCrd.Update.SumCrdDisbursementTransaction.Amount =
                    local.SumCrd.Item.SumCrdDisbursementTransaction.Amount + local
                    .DtlCrd.Item.DtlCrdDisbursementTransaction.Amount;
                  local.DtlCrd.Item.DtlDbt.Index = 0;

                  for(var limit2 = local.DtlCrd.Item.DtlDbt.Count; local
                    .DtlCrd.Item.DtlDbt.Index < limit2; ++
                    local.DtlCrd.Item.DtlDbt.Index)
                  {
                    if (!local.DtlCrd.Item.DtlDbt.CheckSize())
                    {
                      break;
                    }

                    for(local.SumCrd.Item.SumDbt.Index = 0; local
                      .SumCrd.Item.SumDbt.Index < local
                      .SumCrd.Item.SumDbt.Count; ++
                      local.SumCrd.Item.SumDbt.Index)
                    {
                      if (!local.SumCrd.Item.SumDbt.CheckSize())
                      {
                        break;
                      }

                      if (AsChar(local.DtlCrd.Item.DtlDbt.Item.
                        DtlDbtDisbursementTransaction.Type1) == AsChar
                        (local.SumCrd.Item.SumDbt.Item.
                          SumDbtDisbursementTransaction.Type1) && Equal
                        (local.DtlCrd.Item.DtlDbt.Item.
                          DtlDbtDisbursementTransaction.ReferenceNumber,
                        local.SumCrd.Item.SumDbt.Item.
                          SumDbtDisbursementTransaction.ReferenceNumber) && local
                        .DtlCrd.Item.DtlDbt.Item.DtlDbtDisbursementType.
                          SystemGeneratedIdentifier == local
                        .SumCrd.Item.SumDbt.Item.SumDbtDisbursementType.
                          SystemGeneratedIdentifier && AsChar
                        (local.DtlCrd.Item.DtlDbt.Item.DtlDbtPayoutInd.Flag) ==
                          AsChar
                        (local.SumCrd.Item.SumDbt.Item.SumDbtPayoutInd.Flag))
                      {
                        local.SumCrd.Update.SumDbt.Update.
                          SumDbtDisbursementTransaction.Amount =
                            local.SumCrd.Item.SumDbt.Item.
                            SumDbtDisbursementTransaction.Amount + local
                          .DtlCrd.Item.DtlDbt.Item.
                            DtlDbtDisbursementTransaction.Amount;

                        goto Next1;
                      }
                    }

                    local.SumCrd.Item.SumDbt.CheckIndex();

                    if (local.SumCrd.Item.SumDbt.Index + 1 >= Local
                      .SumDbtGroup.Capacity)
                    {
                      ExitState = "ACO_NI0000_LIST_EXCEED_MAX_LNGTH";
                      local.TextMsg.Text30 = "sum dbt table " + NumberToString
                        (local.SumCrd.Item.SumDbt.Index + 1, 11, 5);
                      UseFnB656PrintErrorLine1();

                      if (!IsExitState("ACO_NN0000_ALL_OK"))
                      {
                        goto Test3;
                      }

                      goto ReadEach;
                    }

                    local.SumCrd.Item.SumDbt.Index =
                      local.SumCrd.Item.SumDbt.Count;
                    local.SumCrd.Item.SumDbt.CheckSize();

                    local.SumCrd.Update.SumDbt.Update.
                      SumDbtDisbursementTransaction.Assign(
                        local.DtlCrd.Item.DtlDbt.Item.
                        DtlDbtDisbursementTransaction);
                    local.SumCrd.Update.SumDbt.Update.SumDbtDisbursementType.
                      Assign(local.DtlCrd.Item.DtlDbt.Item.
                        DtlDbtDisbursementType);
                    local.SumCrd.Update.SumDbt.Update.SumDbtPayoutInd.Flag =
                      local.DtlCrd.Item.DtlDbt.Item.DtlDbtPayoutInd.Flag;

Next1:
                    ;
                  }

                  local.DtlCrd.Item.DtlDbt.CheckIndex();

                  goto Next2;
                }
              }

              local.SumCrd.CheckIndex();
            }

            if (local.SumCrd.Index + 1 >= Local.SumCrdGroup.Capacity)
            {
              ExitState = "ACO_NI0000_LIST_EXCEED_MAX_LNGTH";
              local.TextMsg.Text30 = "sum crd table " + NumberToString
                (local.SumCrd.Index + 1, 11, 5);
              UseFnB656PrintErrorLine1();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                goto Test3;
              }

              goto ReadEach;
            }

            local.SumCrd.Index = local.SumCrd.Count;
            local.SumCrd.CheckSize();

            local.SumCrd.Item.SumDbt.Index = -1;
            local.SumCrd.Item.SumDbt.Count = 0;
            local.SumCrd.Update.SumCrdDisbursementTransaction.Assign(
              local.DtlCrd.Item.DtlCrdDisbursementTransaction);
            local.SumCrd.Update.SumCrdPayor.
              Assign(local.DtlCrd.Item.DtlCrdPayor);
            MoveCollection(local.DtlCrd.Item.DtlCrdCollection,
              local.SumCrd.Update.SumCrdCollection);
            local.SumCrd.Update.SumCrdCollectionType.PrintName =
              local.DtlCrd.Item.DtlCrdCollectionType.PrintName;

            for(local.DtlCrd.Item.DtlDbt.Index = 0; local
              .DtlCrd.Item.DtlDbt.Index < local.DtlCrd.Item.DtlDbt.Count; ++
              local.DtlCrd.Item.DtlDbt.Index)
            {
              if (!local.DtlCrd.Item.DtlDbt.CheckSize())
              {
                break;
              }

              local.SumCrd.Item.SumDbt.Index = local.DtlCrd.Item.DtlDbt.Index;
              local.SumCrd.Item.SumDbt.CheckSize();

              local.SumCrd.Update.SumDbt.Update.SumDbtDisbursementTransaction.
                Assign(local.DtlCrd.Item.DtlDbt.Item.
                  DtlDbtDisbursementTransaction);
              local.SumCrd.Update.SumDbt.Update.SumDbtDisbursementType.Assign(
                local.DtlCrd.Item.DtlDbt.Item.DtlDbtDisbursementType);
              local.SumCrd.Update.SumDbt.Update.SumDbtPayoutInd.Flag =
                local.DtlCrd.Item.DtlDbt.Item.DtlDbtPayoutInd.Flag;
            }

            local.DtlCrd.Item.DtlDbt.CheckIndex();

Next2:
            ;
          }

          local.DtlCrd.CheckIndex();

          if (local.SumCrd.IsEmpty)
          {
            ExitState = "ACO_NI0000_GROUP_VIEW_IS_EMPTY";
            UseFnB656PrintErrorLine3();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            continue;
          }
        }

        // ----------------------------------------------------------
        // 09/12/00 - #103334
        // Optimize ADABAS calls to improve performance.
        // ----------------------------------------------------------
        local.SumCrd.Index = 0;

        for(var limit = local.SumCrd.Count; local.SumCrd.Index < limit; ++
          local.SumCrd.Index)
        {
          if (!local.SumCrd.CheckSize())
          {
            break;
          }

          // ----------------------------------------------------------
          // 12/27/00 - #107373
          // Do not call ADABAS to get name when payor does not exist.
          // ----------------------------------------------------------
          if (IsEmpty(local.SumCrd.Item.SumCrdPayor.Number))
          {
            continue;
          }

          local.Payor.Number = local.SumCrd.Item.SumCrdPayor.Number;
          UseSiReadCsePersonBatch1();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseFnB656PrintErrorNAbendData();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            goto ReadEach;
          }
        }

        local.SumCrd.CheckIndex();

        if (local.NaCurrentSuppForWarr.TotalCurrency > entities
          .ExistingWarrant.Amount)
        {
          local.NaCurrentSuppForWarr.TotalCurrency =
            entities.ExistingWarrant.Amount;
        }

        // ***********************************************
        // A failure after this point will cause an ABEND.
        // ***********************************************
        if (local.NaCurrentSuppForWarr.TotalCurrency > 0)
        {
          local.NbrOfMembers.Count = local.SuppPersons.Count;
          local.RemainingCurrentSupp.TotalCurrency =
            local.NaCurrentSuppForWarr.TotalCurrency;
          local.InterfaceIncomeNotification.ObligorCsePersonNumber =
            local.WarrantPayeeCsePerson.Number;
          local.InterfaceIncomeNotification.CollectionDate =
            local.LatestCollectionDate.Date;
          local.InterfaceIncomeNotification.DistributionDate =
            local.Process.Date;

          for(local.SuppPersons.Index = 0; local.SuppPersons.Index < local
            .SuppPersons.Count; ++local.SuppPersons.Index)
          {
            if (!local.SuppPersons.CheckSize())
            {
              break;
            }

            if (local.SuppPersons.Index + 1 == local.SuppPersons.Count)
            {
              local.MemberShareOfCurrSupp.TotalCurrency =
                local.RemainingCurrentSupp.TotalCurrency;
            }
            else
            {
              local.MemberShareOfCurrSupp.TotalCurrency =
                local.NaCurrentSuppForWarr.TotalCurrency / local
                .NbrOfMembers.Count;
              local.RemainingCurrentSupp.TotalCurrency -= local.
                MemberShareOfCurrSupp.TotalCurrency;
            }

            local.InterfaceIncomeNotification.SupportedCsePersonNumber =
              local.SuppPersons.Item.SuppPerson.Number;
            local.InterfaceIncomeNotification.CollectionAmount =
              local.MemberShareOfCurrSupp.TotalCurrency;
            UseFnNotifyAeOfPayment();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              // ------------------
              // Abend Program
              // ------------------
              goto Test3;
            }

            ++local.NoOfAeRecordsCreated.Count;
            local.NoOfAeRecordsCreated.TotalCurrency += local.
              InterfaceIncomeNotification.CollectionAmount;
          }

          local.SuppPersons.CheckIndex();
        }

        // : Output the Warrant Header & Address Information.
        // : RECORD TYPE 2  -  Warrant Payee Information Record
        local.RecordType.Text4 = "2";
        UseFnExtProcessDoaPayTape6();

        if (local.PayTape.NumericReturnCode != 0)
        {
          ExitState = "FN0000_DOA_PAY_TAPE_WRITE_ERR";

          goto Test3;
        }

        // : RECORD TYPE 3  -  Warrant Mailing Address Record
        local.RecordType.Text4 = "3";
        UseFnExtProcessDoaPayTape7();

        if (local.PayTape.NumericReturnCode != 0)
        {
          ExitState = "FN0000_DOA_PAY_TAPE_WRITE_ERR";

          goto Test3;
        }

        // : RECORD TYPE 4  -  Warrant Stub Payee Information Record
        local.RecordType.Text4 = "4";
        UseFnExtProcessDoaPayTape6();

        if (local.PayTape.NumericReturnCode != 0)
        {
          ExitState = "FN0000_DOA_PAY_TAPE_WRITE_ERR";

          goto Test3;
        }

        local.StubLinesPrinted.Count = 1;
        local.PrintOverflowMessageInd.Flag = "N";

        if (AsChar(entities.ExistingWarrant.InterstateInd) == 'Y')
        {
          // : RECORD TYPE 5  -  Interstate Payee Detail Information Record
          local.RecordType.Text4 = "5-1";
          local.StubLinesPrinted.Count += 2;
          UseFnExtProcessDoaPayTape3();

          if (local.PayTape.NumericReturnCode != 0)
          {
            ExitState = "FN0000_DOA_PAY_TAPE_WRITE_ERR";

            goto Test3;
          }

          // : RECORD TYPE 5  -  Interstate Detail Information Record
          local.RecordType.Text4 = "5-2";
          UseFnExtProcessDoaPayTape5();

          if (local.PayTape.NumericReturnCode != 0)
          {
            ExitState = "FN0000_DOA_PAY_TAPE_WRITE_ERR";

            goto Test3;
          }
        }

        if (AsChar(local.UseDesignatedPayeeInd.Flag) == 'Y')
        {
          // : RECORD TYPE 5  -  Designated Payee Record
          local.RecordType.Text4 = "5-3";
          ++local.StubLinesPrinted.Count;
          UseFnExtProcessDoaPayTape4();

          if (local.PayTape.NumericReturnCode != 0)
          {
            ExitState = "FN0000_DOA_PAY_TAPE_WRITE_ERR";

            goto Test3;
          }
        }

        // : Output Stub Detail Lines.
        if (AsChar(local.PrintFdsoMsgInd.Flag) == 'Y')
        {
          // --------------------------------------------------------------------------------------------------------
          // 05/05/11 SWSRGAV  CQ#26668 - Do not print FDSO message for F type 
          // collections.  The statement further up
          // in the code which set the local_print_fdso_msg_ind to "Y" was 
          // disabled so that this IF statement will not
          // execute.  The IF statement and its logic are left in intact in case
          // a message is needed in the future.
          // --------------------------------------------------------------------------------------------------------
          // : RECORD TYPE 5  -  FDSO Message Text
          local.RecordType.Text4 = "MSG";
          local.StubLinesPrinted.Count += 11;

          for(local.FdsoMsgText.Index = 0; local.FdsoMsgText.Index < 10; ++
            local.FdsoMsgText.Index)
          {
            if (!local.FdsoMsgText.CheckSize())
            {
              break;
            }

            UseFnExtProcessDoaPayTape14();

            if (local.PayTape.NumericReturnCode != 0)
            {
              ExitState = "FN0000_DOA_PAY_TAPE_WRITE_ERR";

              goto Test3;
            }
          }

          local.FdsoMsgText.CheckIndex();

          // : RECORD TYPE 5  - Blank Line
          local.RecordType.Text4 = "MSG";
          UseFnExtProcessDoaPayTape2();

          if (local.PayTape.NumericReturnCode != 0)
          {
            ExitState = "FN0000_DOA_PAY_TAPE_WRITE_ERR";

            goto Test3;
          }
        }

        // : RECORD TYPE 5  -  Warrant Stub Title Line Record
        local.RecordType.Text4 = "5-0";
        local.StubLinesPrinted.Count += 2;
        UseFnExtProcessDoaPayTape2();

        if (local.PayTape.NumericReturnCode != 0)
        {
          ExitState = "FN0000_DOA_PAY_TAPE_WRITE_ERR";

          goto Test3;
        }

        // : RECORD TYPE 5  - Blank Line
        local.RecordType.Text4 = "MSG";
        UseFnExtProcessDoaPayTape2();

        if (local.PayTape.NumericReturnCode != 0)
        {
          ExitState = "FN0000_DOA_PAY_TAPE_WRITE_ERR";

          goto Test3;
        }

        if (Equal(entities.ExistingWarrant.Classification, "REF") || Equal
          (entities.ExistingWarrant.Classification, "ADV"))
        {
          // : RECORD TYPE 5  -  Refund/Advancement Detail Record
          local.RecordType.Text4 = "5-4";
          UseFnExtProcessDoaPayTape1();

          if (local.PayTape.NumericReturnCode != 0)
          {
            ExitState = "FN0000_DOA_PAY_TAPE_WRITE_ERR";

            goto Test3;
          }

          // : RECORD TYPE 5  -  Refund/Advancement Message Record
          local.RecordType.Text4 = "5-5";
          UseFnExtProcessDoaPayTape1();

          if (local.PayTape.NumericReturnCode != 0)
          {
            ExitState = "FN0000_DOA_PAY_TAPE_WRITE_ERR";

            goto Test3;
          }
        }
        else
        {
          local.SumCrd.Index = 0;

          for(var limit = local.SumCrd.Count; local.SumCrd.Index < limit; ++
            local.SumCrd.Index)
          {
            if (!local.SumCrd.CheckSize())
            {
              break;
            }

            switch(AsChar(local.SumCrd.Item.SumCrdDisbursementTransaction.Type1))
              
            {
              case 'C':
                // : RECORD TYPE 5  -  Collection Record - Payor Line
                local.RecordType.Text4 = "5-6";

                if (local.StubLinesPrinted.Count >= local
                  .MaxPrintLinesPerStub.Count - 2)
                {
                  local.PrintOverflowMessageInd.Flag = "Y";

                  goto Test2;
                }

                local.StubLinesPrinted.Count += 2;
                UseFnExtProcessDoaPayTape13();

                if (local.PayTape.NumericReturnCode != 0)
                {
                  ExitState = "FN0000_DOA_PAY_TAPE_WRITE_ERR";

                  goto Test3;
                }

                // : RECORD TYPE 5  -  Collection Record - Detail Line
                local.RecordType.Text4 = "5-7";
                UseFnExtProcessDoaPayTape11();

                if (local.PayTape.NumericReturnCode != 0)
                {
                  ExitState = "FN0000_DOA_PAY_TAPE_WRITE_ERR";

                  goto Test3;
                }

                break;
              case 'P':
                // : RECORD TYPE 5  -  Passthru Record
                local.RecordType.Text4 = "5-8";

                if (local.StubLinesPrinted.Count >= local
                  .MaxPrintLinesPerStub.Count - 2)
                {
                  local.PrintOverflowMessageInd.Flag = "Y";

                  goto Test2;
                }

                ++local.StubLinesPrinted.Count;
                UseFnExtProcessDoaPayTape10();

                if (local.PayTape.NumericReturnCode != 0)
                {
                  ExitState = "FN0000_DOA_PAY_TAPE_WRITE_ERR";

                  goto Test3;
                }

                break;
              case 'X':
                // : RECORD TYPE 5  -  Excess URA Record
                local.RecordType.Text4 = "5-9";

                if (local.StubLinesPrinted.Count >= local
                  .MaxPrintLinesPerStub.Count - 2)
                {
                  local.PrintOverflowMessageInd.Flag = "Y";

                  goto Test2;
                }

                ++local.StubLinesPrinted.Count;
                UseFnExtProcessDoaPayTape10();

                if (local.PayTape.NumericReturnCode != 0)
                {
                  ExitState = "FN0000_DOA_PAY_TAPE_WRITE_ERR";

                  goto Test3;
                }

                break;
              default:
                break;
            }

            local.SumCrd.Item.SumDbt.Index = 0;

            for(var limit1 = local.SumCrd.Item.SumDbt.Count; local
              .SumCrd.Item.SumDbt.Index < limit1; ++
              local.SumCrd.Item.SumDbt.Index)
            {
              if (!local.SumCrd.Item.SumDbt.CheckSize())
              {
                break;
              }

              if (local.StubLinesPrinted.Count >= local
                .MaxPrintLinesPerStub.Count - 2)
              {
                local.PrintOverflowMessageInd.Flag = "Y";

                goto Test2;
              }

              ++local.StubLinesPrinted.Count;

              if (AsChar(local.SumCrd.Item.SumDbt.Item.SumDbtPayoutInd.Flag) ==
                'Y')
              {
                // : RECORD TYPE 5  -  Debit Disbursement Record - Payout Detail
                local.RecordType.Text4 = "5-11";
                UseFnExtProcessDoaPayTape12();

                if (local.PayTape.NumericReturnCode != 0)
                {
                  ExitState = "FN0000_DOA_PAY_TAPE_WRITE_ERR";

                  goto Test3;
                }
              }
              else
              {
                // : RECORD TYPE 5  -  Debit Disbursement Record - Non-Payout 
                // Detail
                local.RecordType.Text4 = "5-10";
                UseFnExtProcessDoaPayTape12();

                if (local.PayTape.NumericReturnCode != 0)
                {
                  ExitState = "FN0000_DOA_PAY_TAPE_WRITE_ERR";

                  goto Test3;
                }
              }
            }

            local.SumCrd.Item.SumDbt.CheckIndex();
          }

          local.SumCrd.CheckIndex();
        }

Test2:

        // : RECORD TYPE 5  -  Warrant Total Record
        local.RecordType.Text4 = "5-12";
        UseFnExtProcessDoaPayTape2();

        if (local.PayTape.NumericReturnCode != 0)
        {
          ExitState = "FN0000_DOA_PAY_TAPE_WRITE_ERR";

          goto Test3;
        }

        if (AsChar(local.PrintOverflowMessageInd.Flag) == 'Y')
        {
          // : RECORD TYPE 5  -  Overflow Message 1
          local.RecordType.Text4 = "MSG";
          UseFnExtProcessDoaPayTape8();

          if (local.PayTape.NumericReturnCode != 0)
          {
            ExitState = "FN0000_DOA_PAY_TAPE_WRITE_ERR";

            goto Test3;
          }

          // : RECORD TYPE 5  -  Overflow Message 2
          local.RecordType.Text4 = "MSG";
          UseFnExtProcessDoaPayTape9();

          if (local.PayTape.NumericReturnCode != 0)
          {
            ExitState = "FN0000_DOA_PAY_TAPE_WRITE_ERR";

            goto Test3;
          }
        }

        // : Apply all DB2 Updates.
        if (AsChar(local.RunInTestMode.Flag) == 'N')
        {
          try
          {
            UpdateWarrant();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_PYMNT_STAT_HISTORY_NF";

                goto Test3;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_PYMNT_STAT_HISTORY_NF";

                goto Test3;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          try
          {
            UpdatePaymentStatusHistory();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_PYMNT_STAT_HISTORY_NF";

                goto Test3;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_PYMNT_STAT_HISTORY_NF";

                goto Test3;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          // -------------------------------------------------
          // 01/30/01 - SWSRKXD WR263 -  Change DOA to KPC.
          // -------------------------------------------------
          try
          {
            CreatePaymentStatusHistory();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_PYMNT_STAT_HISTORY_AE";

                goto Test3;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_PYMNT_STAT_HISTORY_PV";

                goto Test3;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          if (AsChar(local.PaytapeAlreadyCreated.Flag) == 'Y')
          {
            AssociateWarrant();
          }
          else
          {
            try
            {
              CreatePayTape();
              local.PaytapeAlreadyCreated.Flag = "Y";
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "FN0000_DOA_PAY_TAPE_AE";

                  goto Test3;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "FN0000_DOA_PAY_TAPE_PV";

                  goto Test3;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }

          // : Store the Mailing Name and Address in the Warrant Remail Address 
          // entity type for reference by the Online Warrant screens.
          for(local.CreateCount.Count = 1; local.CreateCount.Count <= 1; local
            .CreateCount.Count += 10)
          {
            try
            {
              CreateWarrantRemailAddress();

              break;
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  if (local.CreateCount.Count >= 10)
                  {
                    ExitState = "FN0000_PYMNT_STAT_HISTORY_AE";

                    goto Test3;
                  }

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "FN0000_WARRANT_REMAIL_ADDR_PV";

                  goto Test3;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }
        }

        ++local.NoOfPymntReqProcessed.Count;
        local.NoOfPymntReqProcessed.TotalCurrency += entities.ExistingWarrant.
          Amount;
        local.AmtOfPymntReqProcessed.Amount += entities.ExistingWarrant.Amount;

ReadEach:
        ;
      }
    }

Test3:

    local.PayTape.FileInstruction = "CLOSE";
    UseFnExtProcessDoaPayTape15();

    if (local.PayTape.NumericReturnCode != 0)
    {
      ExitState = "FN0000_DOA_PAY_TAPE_WRITE_ERR";

      return;
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // : Close the Error Report.
      UseFnB656PrintErrorLine4();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      UseFnB656PrintControlTotals();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }
    else
    {
      // : Report the error that occurred and close the Error Report.
      //   ABEND the procedure once reporting is complete.
      UseFnB656PrintErrorLine2();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      UseFnB656PrintControlTotals();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }

    local.Close.Number = "CLOSE";
    UseEabReadCsePersonBatch();
  }

  private static void MoveCollection(Collection source, Collection target)
  {
    target.CollectionDt = source.CollectionDt;
    target.CourtOrderAppliedTo = source.CourtOrderAppliedTo;
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Count = source.Count;
    target.TotalCurrency = source.TotalCurrency;
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
    target.DateOfDeath = source.DateOfDeath;
  }

  private static void MoveCsePersonAddress(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.EndDate = source.EndDate;
    target.County = source.County;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
    target.Street3 = source.Street3;
    target.Street4 = source.Street4;
    target.Province = source.Province;
    target.PostalCode = source.PostalCode;
    target.Country = source.Country;
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Time = source.Time;
  }

  private static void MoveDisbursementTransaction1(
    DisbursementTransaction source, DisbursementTransaction target)
  {
    target.ReferenceNumber = source.ReferenceNumber;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Type1 = source.Type1;
    target.Amount = source.Amount;
    target.ProcessDate = source.ProcessDate;
    target.CollectionDate = source.CollectionDate;
    target.PassthruDate = source.PassthruDate;
  }

  private static void MoveDisbursementTransaction2(
    DisbursementTransaction source, DisbursementTransaction target)
  {
    target.ReferenceNumber = source.ReferenceNumber;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Type1 = source.Type1;
    target.Amount = source.Amount;
    target.ProcessDate = source.ProcessDate;
    target.DisbursementDate = source.DisbursementDate;
  }

  private static void MovePayTape(PayTape source, PayTape target)
  {
    target.ProcessDate = source.ProcessDate;
    target.VoucherNumber = source.VoucherNumber;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private int UseCabGenerate3DigitRandomNum()
  {
    var useImport = new CabGenerate3DigitRandomNum.Import();
    var useExport = new CabGenerate3DigitRandomNum.Export();

    Call(CabGenerate3DigitRandomNum.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute3DigitRandomNumber;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseEabReadCsePersonBatch()
  {
    var useImport = new EabReadCsePersonBatch.Import();
    var useExport = new EabReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.Close.Number;

    Call(EabReadCsePersonBatch.Execute, useImport, useExport);
  }

  private void UseFnB656Housekeeping()
  {
    var useImport = new FnB656Housekeeping.Import();
    var useExport = new FnB656Housekeeping.Export();

    Call(FnB656Housekeeping.Execute, useImport, useExport);

    MovePayTape(useExport.PayTape, local.VoucherNo);
    local.Start.SystemGeneratedIdentifier =
      useExport.Start.SystemGeneratedIdentifier;
    local.End.SystemGeneratedIdentifier =
      useExport.End.SystemGeneratedIdentifier;
    local.RunInTestMode.Flag = useExport.RunInTestMode.Flag;
    local.MaxPrintLinesPerStub.Count = useExport.MaxPrintLinesPerStub.Count;
  }

  private void UseFnB656PrintControlTotals()
  {
    var useImport = new FnB656PrintControlTotals.Import();
    var useExport = new FnB656PrintControlTotals.Export();

    MoveCommon(local.NoOfAeRecordsCreated, useImport.NoAeRecordsCreated);
    useImport.NoOfPaymentRequestRead.Count = local.NoOfPymntReqRead.Count;
    useImport.NoOfPaymentsProcessed.Count = local.NoOfPymntReqProcessed.Count;
    useImport.CloseInd.Flag = local.CloseInd.Flag;

    Call(FnB656PrintControlTotals.Execute, useImport, useExport);
  }

  private void UseFnB656PrintErrorLine1()
  {
    var useImport = new FnB656PrintErrorLine.Import();
    var useExport = new FnB656PrintErrorLine.Export();

    useImport.TextWorkArea.Text30 = local.TextMsg.Text30;
    useImport.PaymentRequest.Assign(entities.ExistingWarrant);
    useImport.NewWarrant.Flag = local.NewWarrant.Flag;

    Call(FnB656PrintErrorLine.Execute, useImport, useExport);
  }

  private void UseFnB656PrintErrorLine2()
  {
    var useImport = new FnB656PrintErrorLine.Import();
    var useExport = new FnB656PrintErrorLine.Export();

    useImport.PaymentRequest.Assign(entities.ExistingWarrant);
    useImport.CloseInd.Flag = local.CloseInd.Flag;
    useImport.NewWarrant.Flag = local.NewWarrant.Flag;

    Call(FnB656PrintErrorLine.Execute, useImport, useExport);
  }

  private void UseFnB656PrintErrorLine3()
  {
    var useImport = new FnB656PrintErrorLine.Import();
    var useExport = new FnB656PrintErrorLine.Export();

    useImport.PaymentRequest.Assign(entities.ExistingWarrant);
    useImport.NewWarrant.Flag = local.NewWarrant.Flag;

    Call(FnB656PrintErrorLine.Execute, useImport, useExport);
  }

  private void UseFnB656PrintErrorLine4()
  {
    var useImport = new FnB656PrintErrorLine.Import();
    var useExport = new FnB656PrintErrorLine.Export();

    useImport.CloseInd.Flag = local.CloseInd.Flag;
    useImport.NewWarrant.Flag = local.NewWarrant.Flag;

    Call(FnB656PrintErrorLine.Execute, useImport, useExport);
  }

  private void UseFnB656PrintErrorNAbendData()
  {
    var useImport = new FnB656PrintErrorNAbendData.Import();
    var useExport = new FnB656PrintErrorNAbendData.Export();

    useImport.AbendData.Assign(local.AbendData);
    useImport.PaymentRequest.Assign(entities.ExistingWarrant);

    Call(FnB656PrintErrorNAbendData.Execute, useImport, useExport);
  }

  private void UseFnBuildTimestampFrmDateTime()
  {
    var useImport = new FnBuildTimestampFrmDateTime.Import();
    var useExport = new FnBuildTimestampFrmDateTime.Export();

    MoveDateWorkArea(local.DailyErrorReportCutoff, useImport.DateWorkArea);

    Call(FnBuildTimestampFrmDateTime.Execute, useImport, useExport);

    local.DailyErrorReportCutoff.Assign(useExport.DateWorkArea);
  }

  private void UseFnExtProcessDoaPayTape1()
  {
    var useImport = new FnExtProcessDoaPayTape.Import();
    var useExport = new FnExtProcessDoaPayTape.Export();

    useImport.External.FileInstruction = local.PayTape.FileInstruction;
    useImport.RecordType.Text4 = local.RecordType.Text4;
    useImport.Warrant.Assign(entities.ExistingWarrant);
    useImport.ReceiptRefund.Assign(entities.ExistingReceiptRefund);
    useExport.External.NumericReturnCode = local.PayTape.NumericReturnCode;

    Call(FnExtProcessDoaPayTape.Execute, useImport, useExport);

    local.PayTape.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseFnExtProcessDoaPayTape2()
  {
    var useImport = new FnExtProcessDoaPayTape.Import();
    var useExport = new FnExtProcessDoaPayTape.Export();

    useImport.External.FileInstruction = local.PayTape.FileInstruction;
    useImport.RecordType.Text4 = local.RecordType.Text4;
    useImport.Warrant.Assign(entities.ExistingWarrant);
    useExport.External.NumericReturnCode = local.PayTape.NumericReturnCode;

    Call(FnExtProcessDoaPayTape.Execute, useImport, useExport);

    local.PayTape.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseFnExtProcessDoaPayTape3()
  {
    var useImport = new FnExtProcessDoaPayTape.Import();
    var useExport = new FnExtProcessDoaPayTape.Export();

    useImport.External.FileInstruction = local.PayTape.FileInstruction;
    useImport.RecordType.Text4 = local.RecordType.Text4;
    useImport.Warrant.Assign(entities.ExistingWarrant);
    useImport.WarrantPayee.Assign(local.WarrantPayeeCsePersonsWorkSet);
    useExport.External.NumericReturnCode = local.PayTape.NumericReturnCode;

    Call(FnExtProcessDoaPayTape.Execute, useImport, useExport);

    local.PayTape.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseFnExtProcessDoaPayTape4()
  {
    var useImport = new FnExtProcessDoaPayTape.Import();
    var useExport = new FnExtProcessDoaPayTape.Export();

    useImport.External.FileInstruction = local.PayTape.FileInstruction;
    useImport.RecordType.Text4 = local.RecordType.Text4;
    useImport.Warrant.Assign(entities.ExistingWarrant);
    useImport.WarrantStubFor.Assign(local.WarrantStubForCsePersonsWorkSet);
    useExport.External.NumericReturnCode = local.PayTape.NumericReturnCode;

    Call(FnExtProcessDoaPayTape.Execute, useImport, useExport);

    local.PayTape.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseFnExtProcessDoaPayTape5()
  {
    var useImport = new FnExtProcessDoaPayTape.Import();
    var useExport = new FnExtProcessDoaPayTape.Export();

    useImport.External.FileInstruction = local.PayTape.FileInstruction;
    useImport.RecordType.Text4 = local.RecordType.Text4;
    useImport.Warrant.Assign(entities.ExistingWarrant);
    useImport.InterstatWarrant.Assign(local.InterstatWarrant);
    useExport.External.NumericReturnCode = local.PayTape.NumericReturnCode;

    Call(FnExtProcessDoaPayTape.Execute, useImport, useExport);

    local.PayTape.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseFnExtProcessDoaPayTape6()
  {
    var useImport = new FnExtProcessDoaPayTape.Import();
    var useExport = new FnExtProcessDoaPayTape.Export();

    useImport.External.FileInstruction = local.PayTape.FileInstruction;
    useImport.RecordType.Text4 = local.RecordType.Text4;
    useImport.Warrant.Assign(entities.ExistingWarrant);
    useImport.WarrantPayeePrint.Assign(local.WarrantPayeePrint);
    useExport.External.NumericReturnCode = local.PayTape.NumericReturnCode;

    Call(FnExtProcessDoaPayTape.Execute, useImport, useExport);

    local.PayTape.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseFnExtProcessDoaPayTape7()
  {
    var useImport = new FnExtProcessDoaPayTape.Import();
    var useExport = new FnExtProcessDoaPayTape.Export();

    useImport.External.FileInstruction = local.PayTape.FileInstruction;
    useImport.RecordType.Text4 = local.RecordType.Text4;
    useImport.Warrant.Assign(entities.ExistingWarrant);
    useImport.WarrantMailingAddr.Assign(local.WarrantMailingAddr);
    useExport.External.NumericReturnCode = local.PayTape.NumericReturnCode;

    Call(FnExtProcessDoaPayTape.Execute, useImport, useExport);

    local.PayTape.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseFnExtProcessDoaPayTape8()
  {
    var useImport = new FnExtProcessDoaPayTape.Import();
    var useExport = new FnExtProcessDoaPayTape.Export();

    useImport.External.FileInstruction = local.PayTape.FileInstruction;
    useImport.RecordType.Text4 = local.RecordType.Text4;
    useImport.Warrant.Assign(entities.ExistingWarrant);
    useImport.PrintMsg.Text80 = local.OverflowMsgText1.Text80;
    useExport.External.NumericReturnCode = local.PayTape.NumericReturnCode;

    Call(FnExtProcessDoaPayTape.Execute, useImport, useExport);

    local.PayTape.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseFnExtProcessDoaPayTape9()
  {
    var useImport = new FnExtProcessDoaPayTape.Import();
    var useExport = new FnExtProcessDoaPayTape.Export();

    useImport.External.FileInstruction = local.PayTape.FileInstruction;
    useImport.RecordType.Text4 = local.RecordType.Text4;
    useImport.Warrant.Assign(entities.ExistingWarrant);
    useImport.PrintMsg.Text80 = local.OverflowMsgText2.Text80;
    useExport.External.NumericReturnCode = local.PayTape.NumericReturnCode;

    Call(FnExtProcessDoaPayTape.Execute, useImport, useExport);

    local.PayTape.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseFnExtProcessDoaPayTape10()
  {
    var useImport = new FnExtProcessDoaPayTape.Import();
    var useExport = new FnExtProcessDoaPayTape.Export();

    useImport.External.FileInstruction = local.PayTape.FileInstruction;
    useImport.RecordType.Text4 = local.RecordType.Text4;
    useImport.Warrant.Assign(entities.ExistingWarrant);
    MoveDisbursementTransaction1(local.SumCrd.Item.
      SumCrdDisbursementTransaction, useImport.DisbursementTransaction);
    useExport.External.NumericReturnCode = local.PayTape.NumericReturnCode;

    Call(FnExtProcessDoaPayTape.Execute, useImport, useExport);

    local.PayTape.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseFnExtProcessDoaPayTape11()
  {
    var useImport = new FnExtProcessDoaPayTape.Import();
    var useExport = new FnExtProcessDoaPayTape.Export();

    useImport.External.FileInstruction = local.PayTape.FileInstruction;
    useImport.RecordType.Text4 = local.RecordType.Text4;
    useImport.Warrant.Assign(entities.ExistingWarrant);
    MoveDisbursementTransaction1(local.SumCrd.Item.
      SumCrdDisbursementTransaction, useImport.DisbursementTransaction);
    MoveCollection(local.SumCrd.Item.SumCrdCollection, useImport.Collection);
    useImport.CollectionType.PrintName =
      local.SumCrd.Item.SumCrdCollectionType.PrintName;
    useExport.External.NumericReturnCode = local.PayTape.NumericReturnCode;

    Call(FnExtProcessDoaPayTape.Execute, useImport, useExport);

    local.PayTape.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseFnExtProcessDoaPayTape12()
  {
    var useImport = new FnExtProcessDoaPayTape.Import();
    var useExport = new FnExtProcessDoaPayTape.Export();

    useImport.Warrant.Assign(entities.ExistingWarrant);
    useImport.External.FileInstruction = local.PayTape.FileInstruction;
    useImport.RecordType.Text4 = local.RecordType.Text4;
    MoveDisbursementTransaction2(local.SumCrd.Item.SumDbt.Item.
      SumDbtDisbursementTransaction, useImport.DisbursementTransaction);
    useImport.DisbursementType.Assign(
      local.SumCrd.Item.SumDbt.Item.SumDbtDisbursementType);
    useExport.External.NumericReturnCode = local.PayTape.NumericReturnCode;

    Call(FnExtProcessDoaPayTape.Execute, useImport, useExport);

    local.PayTape.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseFnExtProcessDoaPayTape13()
  {
    var useImport = new FnExtProcessDoaPayTape.Import();
    var useExport = new FnExtProcessDoaPayTape.Export();

    useImport.External.FileInstruction = local.PayTape.FileInstruction;
    useImport.RecordType.Text4 = local.RecordType.Text4;
    useImport.Warrant.Assign(entities.ExistingWarrant);
    useImport.Payor.Assign(local.SumCrd.Item.SumCrdPayor);
    MoveCollection(local.SumCrd.Item.SumCrdCollection, useImport.Collection);
    useExport.External.NumericReturnCode = local.PayTape.NumericReturnCode;

    Call(FnExtProcessDoaPayTape.Execute, useImport, useExport);

    local.PayTape.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseFnExtProcessDoaPayTape14()
  {
    var useImport = new FnExtProcessDoaPayTape.Import();
    var useExport = new FnExtProcessDoaPayTape.Export();

    useImport.External.FileInstruction = local.PayTape.FileInstruction;
    useImport.RecordType.Text4 = local.RecordType.Text4;
    useImport.Warrant.Assign(entities.ExistingWarrant);
    useImport.PrintMsg.Text80 = local.FdsoMsgText.Item.FdsoMsgText1.Text80;
    useExport.External.NumericReturnCode = local.PayTape.NumericReturnCode;

    Call(FnExtProcessDoaPayTape.Execute, useImport, useExport);

    local.PayTape.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseFnExtProcessDoaPayTape15()
  {
    var useImport = new FnExtProcessDoaPayTape.Import();
    var useExport = new FnExtProcessDoaPayTape.Export();

    useImport.External.FileInstruction = local.PayTape.FileInstruction;
    useExport.External.NumericReturnCode = local.PayTape.NumericReturnCode;

    Call(FnExtProcessDoaPayTape.Execute, useImport, useExport);

    local.PayTape.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseFnHardcodedDisbursementInfo()
  {
    var useImport = new FnHardcodedDisbursementInfo.Import();
    var useExport = new FnHardcodedDisbursementInfo.Export();

    Call(FnHardcodedDisbursementInfo.Execute, useImport, useExport);

    local.HardcodedNaCsp.SystemGeneratedIdentifier =
      useExport.NaCsp.SystemGeneratedIdentifier;
    local.HardcodedNaCcs.SystemGeneratedIdentifier =
      useExport.NaCcs.SystemGeneratedIdentifier;
    local.HardcodedPt.SystemGeneratedIdentifier =
      useExport.Pt.SystemGeneratedIdentifier;
    local.HardcodedRequested.SystemGeneratedIdentifier =
      useExport.Req.SystemGeneratedIdentifier;
  }

  private void UseFnNotifyAeOfPayment()
  {
    var useImport = new FnNotifyAeOfPayment.Import();
    var useExport = new FnNotifyAeOfPayment.Export();

    useImport.InterfaceIncomeNotification.Assign(
      local.InterfaceIncomeNotification);

    Call(FnNotifyAeOfPayment.Execute, useImport, useExport);
  }

  private void UseSiGetCsePersonMailingAddr()
  {
    var useImport = new SiGetCsePersonMailingAddr.Import();
    var useExport = new SiGetCsePersonMailingAddr.Export();

    useImport.CsePerson.Number = local.WarrantPayeeCsePerson.Number;

    Call(SiGetCsePersonMailingAddr.Execute, useImport, useExport);

    MoveCsePersonAddress(useExport.CsePersonAddress, local.WarrantMailingAddr);
  }

  private void UseSiReadCsePersonBatch1()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.Payor.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

    local.SumCrd.Update.SumCrdPayor.Assign(useExport.CsePersonsWorkSet);
    local.AbendData.Assign(useExport.AbendData);
  }

  private void UseSiReadCsePersonBatch2()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number =
      local.WarrantPayeeCsePersonsWorkSet.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

    MoveCsePerson(useExport.CsePerson, local.WarrantPayeeCsePerson);
    local.WarrantPayeeCsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseSiReadCsePersonBatch3()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number =
      local.WarrantStubForCsePersonsWorkSet.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

    MoveCsePerson(useExport.CsePerson, local.WarrantStubForCsePerson);
    local.WarrantStubForCsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void AssociateWarrant()
  {
    var ptpProcessDate = entities.NewPayTape.ProcessDate;

    entities.ExistingWarrant.Populated = false;
    Update("AssociateWarrant",
      (db, command) =>
      {
        db.SetNullableDate(command, "ptpProcessDate", ptpProcessDate);
        db.SetInt32(
          command, "paymentRequestId",
          entities.ExistingWarrant.SystemGeneratedIdentifier);
      });

    entities.ExistingWarrant.PtpProcessDate = ptpProcessDate;
    entities.ExistingWarrant.Populated = true;
  }

  private void CreatePayTape()
  {
    var processDate = local.VoucherNo.ProcessDate;
    var createdBy = local.UserId.Text8;
    var createdTimestamp = local.Current.Timestamp;
    var identCode = "521";
    var voucherNumber = local.VoucherNo.VoucherNumber ?? "";

    entities.ExistingWarrant.Populated = false;
    entities.NewPayTape.Populated = false;
    Update("CreatePayTape#1",
      (db, command) =>
      {
        db.SetDate(command, "processDate", processDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "identCode", identCode);
        db.SetNullableString(command, "voucherNumber", voucherNumber);
      });

    Update("CreatePayTape#3",
      (db, command) =>
      {
        db.SetNullableDate(command, "ptpProcessDate", processDate);
        db.SetInt32(
          command, "paymentRequestId",
          entities.ExistingWarrant.SystemGeneratedIdentifier);
      });

    entities.NewPayTape.ProcessDate = processDate;
    entities.NewPayTape.CreatedBy = createdBy;
    entities.NewPayTape.CreatedTimestamp = createdTimestamp;
    entities.NewPayTape.IdentCode = identCode;
    entities.NewPayTape.VoucherNumber = voucherNumber;
    entities.ExistingWarrant.PtpProcessDate = processDate;
    entities.ExistingWarrant.Populated = true;
    entities.NewPayTape.Populated = true;
  }

  private void CreatePaymentStatusHistory()
  {
    var pstGeneratedId = entities.ExistingKpc.SystemGeneratedIdentifier;
    var prqGeneratedId = entities.ExistingWarrant.SystemGeneratedIdentifier;
    var systemGeneratedIdentifier =
      entities.ExistingPaymentStatusHistory.SystemGeneratedIdentifier + 1;
    var effectiveDate = local.Process.Date;
    var discontinueDate = local.MaxDiscontinue.Date;
    var createdBy = local.UserId.Text8;
    var createdTimestamp = local.Current.Timestamp;
    var reasonText = "Added to Pay Tape";

    entities.NewPaymentStatusHistory.Populated = false;
    Update("CreatePaymentStatusHistory",
      (db, command) =>
      {
        db.SetInt32(command, "pstGeneratedId", pstGeneratedId);
        db.SetInt32(command, "prqGeneratedId", prqGeneratedId);
        db.SetInt32(command, "pymntStatHistId", systemGeneratedIdentifier);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "reasonText", reasonText);
      });

    entities.NewPaymentStatusHistory.PstGeneratedId = pstGeneratedId;
    entities.NewPaymentStatusHistory.PrqGeneratedId = prqGeneratedId;
    entities.NewPaymentStatusHistory.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.NewPaymentStatusHistory.EffectiveDate = effectiveDate;
    entities.NewPaymentStatusHistory.DiscontinueDate = discontinueDate;
    entities.NewPaymentStatusHistory.CreatedBy = createdBy;
    entities.NewPaymentStatusHistory.CreatedTimestamp = createdTimestamp;
    entities.NewPaymentStatusHistory.ReasonText = reasonText;
    entities.NewPaymentStatusHistory.Populated = true;
  }

  private void CreateWarrantRemailAddress()
  {
    var systemGeneratedIdentifier = UseCabGenerate3DigitRandomNum();
    var street1 = local.WarrantMailingAddr.Street1 ?? "";
    var street2 = local.WarrantMailingAddr.Street2 ?? "";
    var city = local.WarrantMailingAddr.City ?? "";
    var state = local.WarrantMailingAddr.State ?? "";
    var zipCode4 = local.WarrantMailingAddr.Zip4 ?? "";
    var zipCode5 = local.WarrantMailingAddr.ZipCode ?? "";
    var name = local.WarrantPayeePrint.FormattedName;
    var remailDate = local.Process.Date;
    var createdBy = local.UserId.Text8;
    var createdTimestamp = local.Current.Timestamp;
    var prqId = entities.ExistingWarrant.SystemGeneratedIdentifier;

    entities.NewWarrantRemailAddress.Populated = false;
    Update("CreateWarrantRemailAddress",
      (db, command) =>
      {
        db.SetInt32(command, "warrantRemailId", systemGeneratedIdentifier);
        db.SetString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetString(command, "city", city);
        db.SetString(command, "state", state);
        db.SetNullableString(command, "zipCode4", zipCode4);
        db.SetString(command, "zipCode5", zipCode5);
        db.SetNullableString(command, "zipCode3", "");
        db.SetNullableString(command, "name", name);
        db.SetDate(command, "remailDate", remailDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetInt32(command, "prqId", prqId);
      });

    entities.NewWarrantRemailAddress.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.NewWarrantRemailAddress.Street1 = street1;
    entities.NewWarrantRemailAddress.Street2 = street2;
    entities.NewWarrantRemailAddress.City = city;
    entities.NewWarrantRemailAddress.State = state;
    entities.NewWarrantRemailAddress.ZipCode4 = zipCode4;
    entities.NewWarrantRemailAddress.ZipCode5 = zipCode5;
    entities.NewWarrantRemailAddress.Name = name;
    entities.NewWarrantRemailAddress.RemailDate = remailDate;
    entities.NewWarrantRemailAddress.CreatedBy = createdBy;
    entities.NewWarrantRemailAddress.CreatedTimestamp = createdTimestamp;
    entities.NewWarrantRemailAddress.PrqId = prqId;
    entities.NewWarrantRemailAddress.Populated = true;
  }

  private bool ReadCase()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingInterstateRequest.Populated);
    entities.ExistingCase.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(
          command, "numb", entities.ExistingInterstateRequest.CasINumber ?? ""
          );
      },
      (db, reader) =>
      {
        entities.ExistingCase.Number = db.GetString(reader, 0);
        entities.ExistingCase.PaMedicalService =
          db.GetNullableString(reader, 1);
        entities.ExistingCase.InterstateCaseId =
          db.GetNullableString(reader, 2);
        entities.ExistingCase.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCaseRole()
  {
    entities.ExistingCaseRole.Populated = false;

    return ReadEach("ReadCaseRole",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
        db.SetNullableDate(
          command, "startDate", local.Process.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ExistingCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ExistingCaseRole.Type1 = db.GetString(reader, 2);
        entities.ExistingCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ExistingCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ExistingCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ExistingCaseRole.MedicalSupportIndicator =
          db.GetNullableString(reader, 6);
        entities.ExistingCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingCaseRole.Type1);

        return true;
      });
  }

  private bool ReadCashReceiptDetailAddress()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingReceiptRefund.Populated);
    entities.ExistingCashReceiptDetailAddress.Populated = false;

    return Read("ReadCashReceiptDetailAddress",
      (db, command) =>
      {
        db.SetDateTime(
          command, "crdetailAddressI",
          entities.ExistingReceiptRefund.CdaIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCashReceiptDetailAddress.SystemGeneratedIdentifier =
          db.GetDateTime(reader, 0);
        entities.ExistingCashReceiptDetailAddress.Street1 =
          db.GetString(reader, 1);
        entities.ExistingCashReceiptDetailAddress.Street2 =
          db.GetNullableString(reader, 2);
        entities.ExistingCashReceiptDetailAddress.City =
          db.GetString(reader, 3);
        entities.ExistingCashReceiptDetailAddress.State =
          db.GetString(reader, 4);
        entities.ExistingCashReceiptDetailAddress.ZipCode5 =
          db.GetString(reader, 5);
        entities.ExistingCashReceiptDetailAddress.ZipCode4 =
          db.GetNullableString(reader, 6);
        entities.ExistingCashReceiptDetailAddress.Populated = true;
      });
  }

  private bool ReadCashReceiptSourceType()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCollection.Populated);
    entities.ExistingCashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptSourceType",
      (db, command) =>
      {
        db.SetInt32(command, "crSrceTypeId", entities.ExistingCollection.CstId);
      },
      (db, reader) =>
      {
        entities.ExistingCashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptSourceType.State =
          db.GetNullableInt32(reader, 1);
        entities.ExistingCashReceiptSourceType.County =
          db.GetNullableInt32(reader, 2);
        entities.ExistingCashReceiptSourceType.Location =
          db.GetNullableInt32(reader, 3);
        entities.ExistingCashReceiptSourceType.Populated = true;
      });
  }

  private bool ReadCollectionCollectionType()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCredit.Populated);
    entities.ExistingCollectionType.Populated = false;
    entities.ExistingCollection.Populated = false;

    return Read("ReadCollectionCollectionType",
      (db, command) =>
      {
        db.SetInt32(
          command, "collId", entities.ExistingCredit.ColId.GetValueOrDefault());
          
        db.SetInt32(
          command, "otyId", entities.ExistingCredit.OtyId.GetValueOrDefault());
        db.SetInt32(
          command, "obgId", entities.ExistingCredit.ObgId.GetValueOrDefault());
        db.SetString(
          command, "cspNumber", entities.ExistingCredit.CspNumberDisb ?? "");
        db.SetString(
          command, "cpaType", entities.ExistingCredit.CpaTypeDisb ?? "");
        db.SetInt32(
          command, "otrId", entities.ExistingCredit.OtrId.GetValueOrDefault());
        db.SetString(
          command, "otrType", entities.ExistingCredit.OtrTypeDisb ?? "");
        db.SetInt32(
          command, "crtType",
          entities.ExistingCredit.CrtId.GetValueOrDefault());
        db.SetInt32(
          command, "cstId", entities.ExistingCredit.CstId.GetValueOrDefault());
        db.SetInt32(
          command, "crvId", entities.ExistingCredit.CrvId.GetValueOrDefault());
        db.SetInt32(
          command, "crdId", entities.ExistingCredit.CrdId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCollection.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCollection.AppliedToCode = db.GetString(reader, 1);
        entities.ExistingCollection.CollectionDt = db.GetDate(reader, 2);
        entities.ExistingCollection.CrtType = db.GetInt32(reader, 3);
        entities.ExistingCollection.CstId = db.GetInt32(reader, 4);
        entities.ExistingCollection.CrvId = db.GetInt32(reader, 5);
        entities.ExistingCollection.CrdId = db.GetInt32(reader, 6);
        entities.ExistingCollection.ObgId = db.GetInt32(reader, 7);
        entities.ExistingCollection.CspNumber = db.GetString(reader, 8);
        entities.ExistingCollection.CpaType = db.GetString(reader, 9);
        entities.ExistingCollection.OtrId = db.GetInt32(reader, 10);
        entities.ExistingCollection.OtrType = db.GetString(reader, 11);
        entities.ExistingCollection.OtyId = db.GetInt32(reader, 12);
        entities.ExistingCollection.CreatedTmst = db.GetDateTime(reader, 13);
        entities.ExistingCollection.ProgramAppliedTo = db.GetString(reader, 14);
        entities.ExistingCollection.CourtOrderAppliedTo =
          db.GetNullableString(reader, 15);
        entities.ExistingCollectionType.SequentialIdentifier =
          db.GetInt32(reader, 16);
        entities.ExistingCollectionType.PrintName =
          db.GetNullableString(reader, 17);
        entities.ExistingCollectionType.Populated = true;
        entities.ExistingCollection.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.ExistingCollection.AppliedToCode);
        CheckValid<Collection>("CpaType", entities.ExistingCollection.CpaType);
        CheckValid<Collection>("OtrType", entities.ExistingCollection.OtrType);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.ExistingCollection.ProgramAppliedTo);
      });
  }

  private bool ReadCsePerson1()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCollection.Populated);
    entities.Supported.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.ExistingCollection.OtyId);
        db.SetString(command, "obTrnTyp", entities.ExistingCollection.OtrType);
        db.SetInt32(command, "obTrnId", entities.ExistingCollection.OtrId);
        db.SetString(command, "cpaType", entities.ExistingCollection.CpaType);
        db.
          SetString(command, "cspNumber", entities.ExistingCollection.CspNumber);
          
        db.
          SetInt32(command, "obgGeneratedId", entities.ExistingCollection.ObgId);
          
      },
      (db, reader) =>
      {
        entities.Supported.Number = db.GetString(reader, 0);
        entities.Supported.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCollection.Populated);
    entities.ExistingPayor.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.ExistingCollection.CspNumber);
      },
      (db, reader) =>
      {
        entities.ExistingPayor.Number = db.GetString(reader, 0);
        entities.ExistingPayor.Populated = true;
      });
  }

  private bool ReadDisbursementTransaction1()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingDebit.Populated);
    entities.ExistingCredit.Populated = false;

    return Read("ReadDisbursementTransaction1",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtrGeneratedId",
          entities.ExistingDebit.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", entities.ExistingDebit.CpaType);
        db.SetString(command, "cspNumber", entities.ExistingDebit.CspNumber);
      },
      (db, reader) =>
      {
        entities.ExistingCredit.CpaType = db.GetString(reader, 0);
        entities.ExistingCredit.CspNumber = db.GetString(reader, 1);
        entities.ExistingCredit.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingCredit.Type1 = db.GetString(reader, 3);
        entities.ExistingCredit.Amount = db.GetDecimal(reader, 4);
        entities.ExistingCredit.ProcessDate = db.GetNullableDate(reader, 5);
        entities.ExistingCredit.CollectionDate = db.GetNullableDate(reader, 6);
        entities.ExistingCredit.PassthruDate = db.GetDate(reader, 7);
        entities.ExistingCredit.OtyId = db.GetNullableInt32(reader, 8);
        entities.ExistingCredit.OtrTypeDisb = db.GetNullableString(reader, 9);
        entities.ExistingCredit.OtrId = db.GetNullableInt32(reader, 10);
        entities.ExistingCredit.CpaTypeDisb = db.GetNullableString(reader, 11);
        entities.ExistingCredit.CspNumberDisb =
          db.GetNullableString(reader, 12);
        entities.ExistingCredit.ObgId = db.GetNullableInt32(reader, 13);
        entities.ExistingCredit.CrdId = db.GetNullableInt32(reader, 14);
        entities.ExistingCredit.CrvId = db.GetNullableInt32(reader, 15);
        entities.ExistingCredit.CstId = db.GetNullableInt32(reader, 16);
        entities.ExistingCredit.CrtId = db.GetNullableInt32(reader, 17);
        entities.ExistingCredit.ColId = db.GetNullableInt32(reader, 18);
        entities.ExistingCredit.ReferenceNumber =
          db.GetNullableString(reader, 19);
        entities.ExistingCredit.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.ExistingCredit.CpaType);
        CheckValid<DisbursementTransaction>("Type1",
          entities.ExistingCredit.Type1);
        CheckValid<DisbursementTransaction>("OtrTypeDisb",
          entities.ExistingCredit.OtrTypeDisb);
        CheckValid<DisbursementTransaction>("CpaTypeDisb",
          entities.ExistingCredit.CpaTypeDisb);
      });
  }

  private IEnumerable<bool> ReadDisbursementTransaction2()
  {
    entities.ExistingDebit.Populated = false;

    return ReadEach("ReadDisbursementTransaction2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prqGeneratedId",
          entities.ExistingWarrant.SystemGeneratedIdentifier);
        db.SetString(
          command, "cspNumber", entities.ExistingWarrant.CsePersonNumber ?? ""
          );
      },
      (db, reader) =>
      {
        entities.ExistingDebit.CpaType = db.GetString(reader, 0);
        entities.ExistingDebit.CspNumber = db.GetString(reader, 1);
        entities.ExistingDebit.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingDebit.Type1 = db.GetString(reader, 3);
        entities.ExistingDebit.Amount = db.GetDecimal(reader, 4);
        entities.ExistingDebit.CollectionDate = db.GetNullableDate(reader, 5);
        entities.ExistingDebit.PrqGeneratedId = db.GetNullableInt32(reader, 6);
        entities.ExistingDebit.IntInterId = db.GetNullableInt32(reader, 7);
        entities.ExistingDebit.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.ExistingDebit.CpaType);
        CheckValid<DisbursementTransaction>("Type1",
          entities.ExistingDebit.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadDisbursementTransactionDisbursementType()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCredit.Populated);
    entities.ExistingDebitDtl.Populated = false;
    entities.ExistingDisbursementType.Populated = false;

    return ReadEach("ReadDisbursementTransactionDisbursementType",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtrPGeneratedId",
          entities.ExistingCredit.SystemGeneratedIdentifier);
        db.SetString(command, "cpaPType", entities.ExistingCredit.CpaType);
        db.SetString(command, "cspPNumber", entities.ExistingCredit.CspNumber);
      },
      (db, reader) =>
      {
        entities.ExistingDebitDtl.CpaType = db.GetString(reader, 0);
        entities.ExistingDebitDtl.CspNumber = db.GetString(reader, 1);
        entities.ExistingDebitDtl.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingDebitDtl.Type1 = db.GetString(reader, 3);
        entities.ExistingDebitDtl.Amount = db.GetDecimal(reader, 4);
        entities.ExistingDebitDtl.ProcessDate = db.GetNullableDate(reader, 5);
        entities.ExistingDebitDtl.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.ExistingDebitDtl.DisbursementDate =
          db.GetNullableDate(reader, 7);
        entities.ExistingDebitDtl.DbtGeneratedId =
          db.GetNullableInt32(reader, 8);
        entities.ExistingDisbursementType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 8);
        entities.ExistingDebitDtl.ReferenceNumber =
          db.GetNullableString(reader, 9);
        entities.ExistingDisbursementType.Code = db.GetString(reader, 10);
        entities.ExistingDisbursementType.Name = db.GetString(reader, 11);
        entities.ExistingDebitDtl.Populated = true;
        entities.ExistingDisbursementType.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.ExistingDebitDtl.CpaType);
        CheckValid<DisbursementTransaction>("Type1",
          entities.ExistingDebitDtl.Type1);

        return true;
      });
  }

  private bool ReadInterstatePaymentAddress()
  {
    entities.ExistingInterstatePaymentAddress.Populated = false;

    return Read("ReadInterstatePaymentAddress",
      (db, command) =>
      {
        db.SetInt32(
          command, "intGeneratedId",
          entities.ExistingInterstateRequest.IntHGeneratedId);
        db.SetDate(
          command, "addressStartDate", local.Process.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingInterstatePaymentAddress.IntGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingInterstatePaymentAddress.AddressStartDate =
          db.GetDate(reader, 1);
        entities.ExistingInterstatePaymentAddress.Type1 =
          db.GetNullableString(reader, 2);
        entities.ExistingInterstatePaymentAddress.Street1 =
          db.GetString(reader, 3);
        entities.ExistingInterstatePaymentAddress.Street2 =
          db.GetNullableString(reader, 4);
        entities.ExistingInterstatePaymentAddress.City =
          db.GetString(reader, 5);
        entities.ExistingInterstatePaymentAddress.Zip5 =
          db.GetNullableString(reader, 6);
        entities.ExistingInterstatePaymentAddress.AddressEndDate =
          db.GetNullableDate(reader, 7);
        entities.ExistingInterstatePaymentAddress.CreatedBy =
          db.GetString(reader, 8);
        entities.ExistingInterstatePaymentAddress.CreatedTimestamp =
          db.GetDateTime(reader, 9);
        entities.ExistingInterstatePaymentAddress.LastUpdatedBy =
          db.GetString(reader, 10);
        entities.ExistingInterstatePaymentAddress.LastUpdatedTimestamp =
          db.GetDateTime(reader, 11);
        entities.ExistingInterstatePaymentAddress.PayableToName =
          db.GetNullableString(reader, 12);
        entities.ExistingInterstatePaymentAddress.State =
          db.GetNullableString(reader, 13);
        entities.ExistingInterstatePaymentAddress.ZipCode =
          db.GetNullableString(reader, 14);
        entities.ExistingInterstatePaymentAddress.Zip4 =
          db.GetNullableString(reader, 15);
        entities.ExistingInterstatePaymentAddress.Zip3 =
          db.GetNullableString(reader, 16);
        entities.ExistingInterstatePaymentAddress.County =
          db.GetNullableString(reader, 17);
        entities.ExistingInterstatePaymentAddress.Street3 =
          db.GetNullableString(reader, 18);
        entities.ExistingInterstatePaymentAddress.Street4 =
          db.GetNullableString(reader, 19);
        entities.ExistingInterstatePaymentAddress.Province =
          db.GetNullableString(reader, 20);
        entities.ExistingInterstatePaymentAddress.PostalCode =
          db.GetNullableString(reader, 21);
        entities.ExistingInterstatePaymentAddress.Country =
          db.GetNullableString(reader, 22);
        entities.ExistingInterstatePaymentAddress.LocationType =
          db.GetString(reader, 23);
        entities.ExistingInterstatePaymentAddress.FipsCounty =
          db.GetNullableString(reader, 24);
        entities.ExistingInterstatePaymentAddress.FipsState =
          db.GetNullableString(reader, 25);
        entities.ExistingInterstatePaymentAddress.FipsLocation =
          db.GetNullableString(reader, 26);
        entities.ExistingInterstatePaymentAddress.Populated = true;
        CheckValid<InterstatePaymentAddress>("LocationType",
          entities.ExistingInterstatePaymentAddress.LocationType);
      });
  }

  private bool ReadInterstateRequest()
  {
    entities.ExistingInterstateRequest.Populated = false;

    return Read("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prqGeneratedId",
          entities.ExistingWarrant.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingInterstateRequest.IntHGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingInterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.ExistingInterstateRequest.OtherStateFips =
          db.GetInt32(reader, 2);
        entities.ExistingInterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.ExistingInterstateRequest.KsCaseInd = db.GetString(reader, 4);
        entities.ExistingInterstateRequest.CasINumber =
          db.GetNullableString(reader, 5);
        entities.ExistingInterstateRequest.Country =
          db.GetNullableString(reader, 6);
        entities.ExistingInterstateRequest.Populated = true;
      });
  }

  private bool ReadPaymentStatus()
  {
    entities.ExistingKpc.Populated = false;

    return Read("ReadPaymentStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "paymentStatusId",
          local.HardcodedKpc.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingKpc.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.ExistingKpc.Populated = true;
      });
  }

  private bool ReadPaymentStatusHistory()
  {
    entities.ExistingPaymentStatusHistory.Populated = false;

    return Read("ReadPaymentStatusHistory",
      (db, command) =>
      {
        db.SetInt32(
          command, "prqGeneratedId",
          entities.ExistingWarrant.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "pstGeneratedId",
          local.HardcodedRequested.SystemGeneratedIdentifier);
        db.SetNullableDate(
          command, "discontinueDate",
          local.MaxDiscontinue.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingPaymentStatusHistory.PstGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingPaymentStatusHistory.PrqGeneratedId =
          db.GetInt32(reader, 1);
        entities.ExistingPaymentStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingPaymentStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.ExistingPaymentStatusHistory.Populated = true;
      });
  }

  private bool ReadProgram1()
  {
    entities.Program.Populated = false;

    return Read("ReadProgram1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Supported.Number);
        db.SetDate(
          command, "effectiveDate", local.Process.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Program.Code = db.GetString(reader, 1);
        entities.Program.Populated = true;
      });
  }

  private bool ReadProgram2()
  {
    entities.Program.Populated = false;

    return Read("ReadProgram2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Supported.Number);
        db.SetDate(
          command, "effectiveDate", local.Process.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Program.Code = db.GetString(reader, 1);
        entities.Program.Populated = true;
      });
  }

  private bool ReadReceiptRefund()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingWarrant.Populated);
    entities.ExistingReceiptRefund.Populated = false;

    return Read("ReadReceiptRefund",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          entities.ExistingWarrant.RctRTstamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingReceiptRefund.CreatedTimestamp =
          db.GetDateTime(reader, 0);
        entities.ExistingReceiptRefund.ReasonCode = db.GetString(reader, 1);
        entities.ExistingReceiptRefund.PayeeName =
          db.GetNullableString(reader, 2);
        entities.ExistingReceiptRefund.Amount = db.GetDecimal(reader, 3);
        entities.ExistingReceiptRefund.RequestDate = db.GetDate(reader, 4);
        entities.ExistingReceiptRefund.CdaIdentifier =
          db.GetNullableDateTime(reader, 5);
        entities.ExistingReceiptRefund.ReasonText =
          db.GetNullableString(reader, 6);
        entities.ExistingReceiptRefund.Populated = true;
      });
  }

  private IEnumerable<bool> ReadWarrant()
  {
    entities.ExistingWarrant.Populated = false;

    return ReadEach("ReadWarrant",
      (db, command) =>
      {
        db.SetDate(
          command, "processDate",
          local.NullDateWorkArea.Date.GetValueOrDefault());
        db.SetInt32(
          command, "systemGeneratedIdentifier1",
          local.Start.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier2",
          local.End.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingWarrant.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingWarrant.ProcessDate = db.GetDate(reader, 1);
        entities.ExistingWarrant.Amount = db.GetDecimal(reader, 2);
        entities.ExistingWarrant.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.ExistingWarrant.DesignatedPayeeCsePersonNo =
          db.GetNullableString(reader, 4);
        entities.ExistingWarrant.CsePersonNumber =
          db.GetNullableString(reader, 5);
        entities.ExistingWarrant.Classification = db.GetString(reader, 6);
        entities.ExistingWarrant.Type1 = db.GetString(reader, 7);
        entities.ExistingWarrant.RctRTstamp = db.GetNullableDateTime(reader, 8);
        entities.ExistingWarrant.PtpProcessDate = db.GetNullableDate(reader, 9);
        entities.ExistingWarrant.PrqRGeneratedId =
          db.GetNullableInt32(reader, 10);
        entities.ExistingWarrant.InterstateInd =
          db.GetNullableString(reader, 11);
        entities.ExistingWarrant.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.ExistingWarrant.Type1);

        return true;
      });
  }

  private void UpdatePaymentStatusHistory()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingPaymentStatusHistory.Populated);

    var discontinueDate = local.Process.Date;

    entities.ExistingPaymentStatusHistory.Populated = false;
    Update("UpdatePaymentStatusHistory",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetInt32(
          command, "pstGeneratedId",
          entities.ExistingPaymentStatusHistory.PstGeneratedId);
        db.SetInt32(
          command, "prqGeneratedId",
          entities.ExistingPaymentStatusHistory.PrqGeneratedId);
        db.SetInt32(
          command, "pymntStatHistId",
          entities.ExistingPaymentStatusHistory.SystemGeneratedIdentifier);
      });

    entities.ExistingPaymentStatusHistory.DiscontinueDate = discontinueDate;
    entities.ExistingPaymentStatusHistory.Populated = true;
  }

  private void UpdateWarrant()
  {
    var processDate = local.Process.Date;

    entities.ExistingWarrant.Populated = false;
    Update("UpdateWarrant",
      (db, command) =>
      {
        db.SetDate(command, "processDate", processDate);
        db.SetInt32(
          command, "paymentRequestId",
          entities.ExistingWarrant.SystemGeneratedIdentifier);
      });

    entities.ExistingWarrant.ProcessDate = processDate;
    entities.ExistingWarrant.Populated = true;
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
    /// <summary>A SuppPersonsGroup group.</summary>
    [Serializable]
    public class SuppPersonsGroup
    {
      /// <summary>
      /// A value of SuppPerson.
      /// </summary>
      [JsonPropertyName("suppPerson")]
      public CsePerson SuppPerson
      {
        get => suppPerson ??= new();
        set => suppPerson = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private CsePerson suppPerson;
    }

    /// <summary>A DtlCrdGroup group.</summary>
    [Serializable]
    public class DtlCrdGroup
    {
      /// <summary>
      /// A value of DtlCrdDisbursementTransaction.
      /// </summary>
      [JsonPropertyName("dtlCrdDisbursementTransaction")]
      public DisbursementTransaction DtlCrdDisbursementTransaction
      {
        get => dtlCrdDisbursementTransaction ??= new();
        set => dtlCrdDisbursementTransaction = value;
      }

      /// <summary>
      /// A value of DtlCrdPayor.
      /// </summary>
      [JsonPropertyName("dtlCrdPayor")]
      public CsePersonsWorkSet DtlCrdPayor
      {
        get => dtlCrdPayor ??= new();
        set => dtlCrdPayor = value;
      }

      /// <summary>
      /// A value of DtlCrdCollection.
      /// </summary>
      [JsonPropertyName("dtlCrdCollection")]
      public Collection DtlCrdCollection
      {
        get => dtlCrdCollection ??= new();
        set => dtlCrdCollection = value;
      }

      /// <summary>
      /// A value of DtlCrdCollectionType.
      /// </summary>
      [JsonPropertyName("dtlCrdCollectionType")]
      public CollectionType DtlCrdCollectionType
      {
        get => dtlCrdCollectionType ??= new();
        set => dtlCrdCollectionType = value;
      }

      /// <summary>
      /// Gets a value of DtlDbt.
      /// </summary>
      [JsonIgnore]
      public Array<DtlDbtGroup> DtlDbt =>
        dtlDbt ??= new(DtlDbtGroup.Capacity, 0);

      /// <summary>
      /// Gets a value of DtlDbt for json serialization.
      /// </summary>
      [JsonPropertyName("dtlDbt")]
      [Computed]
      public IList<DtlDbtGroup> DtlDbt_Json
      {
        get => dtlDbt;
        set => DtlDbt.Assign(value);
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4000;

      private DisbursementTransaction dtlCrdDisbursementTransaction;
      private CsePersonsWorkSet dtlCrdPayor;
      private Collection dtlCrdCollection;
      private CollectionType dtlCrdCollectionType;
      private Array<DtlDbtGroup> dtlDbt;
    }

    /// <summary>A DtlDbtGroup group.</summary>
    [Serializable]
    public class DtlDbtGroup
    {
      /// <summary>
      /// A value of DtlDbtDisbursementTransaction.
      /// </summary>
      [JsonPropertyName("dtlDbtDisbursementTransaction")]
      public DisbursementTransaction DtlDbtDisbursementTransaction
      {
        get => dtlDbtDisbursementTransaction ??= new();
        set => dtlDbtDisbursementTransaction = value;
      }

      /// <summary>
      /// A value of DtlDbtDisbursementType.
      /// </summary>
      [JsonPropertyName("dtlDbtDisbursementType")]
      public DisbursementType DtlDbtDisbursementType
      {
        get => dtlDbtDisbursementType ??= new();
        set => dtlDbtDisbursementType = value;
      }

      /// <summary>
      /// A value of DtlDbtPayoutInd.
      /// </summary>
      [JsonPropertyName("dtlDbtPayoutInd")]
      public Common DtlDbtPayoutInd
      {
        get => dtlDbtPayoutInd ??= new();
        set => dtlDbtPayoutInd = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private DisbursementTransaction dtlDbtDisbursementTransaction;
      private DisbursementType dtlDbtDisbursementType;
      private Common dtlDbtPayoutInd;
    }

    /// <summary>A SumCrdGroup group.</summary>
    [Serializable]
    public class SumCrdGroup
    {
      /// <summary>
      /// A value of SumCrdDisbursementTransaction.
      /// </summary>
      [JsonPropertyName("sumCrdDisbursementTransaction")]
      public DisbursementTransaction SumCrdDisbursementTransaction
      {
        get => sumCrdDisbursementTransaction ??= new();
        set => sumCrdDisbursementTransaction = value;
      }

      /// <summary>
      /// A value of SumCrdPayor.
      /// </summary>
      [JsonPropertyName("sumCrdPayor")]
      public CsePersonsWorkSet SumCrdPayor
      {
        get => sumCrdPayor ??= new();
        set => sumCrdPayor = value;
      }

      /// <summary>
      /// A value of SumCrdCollection.
      /// </summary>
      [JsonPropertyName("sumCrdCollection")]
      public Collection SumCrdCollection
      {
        get => sumCrdCollection ??= new();
        set => sumCrdCollection = value;
      }

      /// <summary>
      /// A value of SumCrdCollectionType.
      /// </summary>
      [JsonPropertyName("sumCrdCollectionType")]
      public CollectionType SumCrdCollectionType
      {
        get => sumCrdCollectionType ??= new();
        set => sumCrdCollectionType = value;
      }

      /// <summary>
      /// Gets a value of SumDbt.
      /// </summary>
      [JsonIgnore]
      public Array<SumDbtGroup> SumDbt =>
        sumDbt ??= new(SumDbtGroup.Capacity, 0);

      /// <summary>
      /// Gets a value of SumDbt for json serialization.
      /// </summary>
      [JsonPropertyName("sumDbt")]
      [Computed]
      public IList<SumDbtGroup> SumDbt_Json
      {
        get => sumDbt;
        set => SumDbt.Assign(value);
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4000;

      private DisbursementTransaction sumCrdDisbursementTransaction;
      private CsePersonsWorkSet sumCrdPayor;
      private Collection sumCrdCollection;
      private CollectionType sumCrdCollectionType;
      private Array<SumDbtGroup> sumDbt;
    }

    /// <summary>A SumDbtGroup group.</summary>
    [Serializable]
    public class SumDbtGroup
    {
      /// <summary>
      /// A value of SumDbtDisbursementTransaction.
      /// </summary>
      [JsonPropertyName("sumDbtDisbursementTransaction")]
      public DisbursementTransaction SumDbtDisbursementTransaction
      {
        get => sumDbtDisbursementTransaction ??= new();
        set => sumDbtDisbursementTransaction = value;
      }

      /// <summary>
      /// A value of SumDbtDisbursementType.
      /// </summary>
      [JsonPropertyName("sumDbtDisbursementType")]
      public DisbursementType SumDbtDisbursementType
      {
        get => sumDbtDisbursementType ??= new();
        set => sumDbtDisbursementType = value;
      }

      /// <summary>
      /// A value of SumDbtPayoutInd.
      /// </summary>
      [JsonPropertyName("sumDbtPayoutInd")]
      public Common SumDbtPayoutInd
      {
        get => sumDbtPayoutInd ??= new();
        set => sumDbtPayoutInd = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private DisbursementTransaction sumDbtDisbursementTransaction;
      private DisbursementType sumDbtDisbursementType;
      private Common sumDbtPayoutInd;
    }

    /// <summary>A FdsoMsgTextGroup group.</summary>
    [Serializable]
    public class FdsoMsgTextGroup
    {
      /// <summary>
      /// A value of FdsoMsgText1.
      /// </summary>
      [JsonPropertyName("fdsoMsgText1")]
      public MessageTextArea FdsoMsgText1
      {
        get => fdsoMsgText1 ??= new();
        set => fdsoMsgText1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private MessageTextArea fdsoMsgText1;
    }

    /// <summary>
    /// A value of NewWarrant.
    /// </summary>
    [JsonPropertyName("newWarrant")]
    public Common NewWarrant
    {
      get => newWarrant ??= new();
      set => newWarrant = value;
    }

    /// <summary>
    /// A value of DailyErrorReportCutoff.
    /// </summary>
    [JsonPropertyName("dailyErrorReportCutoff")]
    public DateWorkArea DailyErrorReportCutoff
    {
      get => dailyErrorReportCutoff ??= new();
      set => dailyErrorReportCutoff = value;
    }

    /// <summary>
    /// A value of HardcodedKpc.
    /// </summary>
    [JsonPropertyName("hardcodedKpc")]
    public PaymentStatus HardcodedKpc
    {
      get => hardcodedKpc ??= new();
      set => hardcodedKpc = value;
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
    /// A value of NbrOfMembers.
    /// </summary>
    [JsonPropertyName("nbrOfMembers")]
    public Common NbrOfMembers
    {
      get => nbrOfMembers ??= new();
      set => nbrOfMembers = value;
    }

    /// <summary>
    /// A value of MemberShareOfCurrSupp.
    /// </summary>
    [JsonPropertyName("memberShareOfCurrSupp")]
    public Common MemberShareOfCurrSupp
    {
      get => memberShareOfCurrSupp ??= new();
      set => memberShareOfCurrSupp = value;
    }

    /// <summary>
    /// A value of RemainingCurrentSupp.
    /// </summary>
    [JsonPropertyName("remainingCurrentSupp")]
    public Common RemainingCurrentSupp
    {
      get => remainingCurrentSupp ??= new();
      set => remainingCurrentSupp = value;
    }

    /// <summary>
    /// A value of NaCurrentSuppForWarr.
    /// </summary>
    [JsonPropertyName("naCurrentSuppForWarr")]
    public Common NaCurrentSuppForWarr
    {
      get => naCurrentSuppForWarr ??= new();
      set => naCurrentSuppForWarr = value;
    }

    /// <summary>
    /// A value of LatestCollectionDate.
    /// </summary>
    [JsonPropertyName("latestCollectionDate")]
    public DateWorkArea LatestCollectionDate
    {
      get => latestCollectionDate ??= new();
      set => latestCollectionDate = value;
    }

    /// <summary>
    /// A value of PersonExistsInGrp.
    /// </summary>
    [JsonPropertyName("personExistsInGrp")]
    public Common PersonExistsInGrp
    {
      get => personExistsInGrp ??= new();
      set => personExistsInGrp = value;
    }

    /// <summary>
    /// Gets a value of SuppPersons.
    /// </summary>
    [JsonIgnore]
    public Array<SuppPersonsGroup> SuppPersons => suppPersons ??= new(
      SuppPersonsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of SuppPersons for json serialization.
    /// </summary>
    [JsonPropertyName("suppPersons")]
    [Computed]
    public IList<SuppPersonsGroup> SuppPersons_Json
    {
      get => suppPersons;
      set => SuppPersons.Assign(value);
    }

    /// <summary>
    /// A value of NoOfAeRecordsCreated.
    /// </summary>
    [JsonPropertyName("noOfAeRecordsCreated")]
    public Common NoOfAeRecordsCreated
    {
      get => noOfAeRecordsCreated ??= new();
      set => noOfAeRecordsCreated = value;
    }

    /// <summary>
    /// A value of InterfaceIncomeNotification.
    /// </summary>
    [JsonPropertyName("interfaceIncomeNotification")]
    public InterfaceIncomeNotification InterfaceIncomeNotification
    {
      get => interfaceIncomeNotification ??= new();
      set => interfaceIncomeNotification = value;
    }

    /// <summary>
    /// A value of HardcodedNaCsp.
    /// </summary>
    [JsonPropertyName("hardcodedNaCsp")]
    public DisbursementType HardcodedNaCsp
    {
      get => hardcodedNaCsp ??= new();
      set => hardcodedNaCsp = value;
    }

    /// <summary>
    /// A value of HardcodedNaCcs.
    /// </summary>
    [JsonPropertyName("hardcodedNaCcs")]
    public DisbursementType HardcodedNaCcs
    {
      get => hardcodedNaCcs ??= new();
      set => hardcodedNaCcs = value;
    }

    /// <summary>
    /// A value of Process.
    /// </summary>
    [JsonPropertyName("process")]
    public DateWorkArea Process
    {
      get => process ??= new();
      set => process = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of PrintFdsoMsgInd.
    /// </summary>
    [JsonPropertyName("printFdsoMsgInd")]
    public Common PrintFdsoMsgInd
    {
      get => printFdsoMsgInd ??= new();
      set => printFdsoMsgInd = value;
    }

    /// <summary>
    /// A value of PrintOverflowMessageInd.
    /// </summary>
    [JsonPropertyName("printOverflowMessageInd")]
    public Common PrintOverflowMessageInd
    {
      get => printOverflowMessageInd ??= new();
      set => printOverflowMessageInd = value;
    }

    /// <summary>
    /// A value of StubLinesPrinted.
    /// </summary>
    [JsonPropertyName("stubLinesPrinted")]
    public Common StubLinesPrinted
    {
      get => stubLinesPrinted ??= new();
      set => stubLinesPrinted = value;
    }

    /// <summary>
    /// A value of UserId.
    /// </summary>
    [JsonPropertyName("userId")]
    public TextWorkArea UserId
    {
      get => userId ??= new();
      set => userId = value;
    }

    /// <summary>
    /// A value of RunInTestMode.
    /// </summary>
    [JsonPropertyName("runInTestMode")]
    public Common RunInTestMode
    {
      get => runInTestMode ??= new();
      set => runInTestMode = value;
    }

    /// <summary>
    /// A value of End.
    /// </summary>
    [JsonPropertyName("end")]
    public PaymentRequest End
    {
      get => end ??= new();
      set => end = value;
    }

    /// <summary>
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    public PaymentRequest Start
    {
      get => start ??= new();
      set => start = value;
    }

    /// <summary>
    /// A value of PayTape.
    /// </summary>
    [JsonPropertyName("payTape")]
    public External PayTape
    {
      get => payTape ??= new();
      set => payTape = value;
    }

    /// <summary>
    /// A value of RecordType.
    /// </summary>
    [JsonPropertyName("recordType")]
    public TextWorkArea RecordType
    {
      get => recordType ??= new();
      set => recordType = value;
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
    /// A value of TempCommon.
    /// </summary>
    [JsonPropertyName("tempCommon")]
    public Common TempCommon
    {
      get => tempCommon ??= new();
      set => tempCommon = value;
    }

    /// <summary>
    /// A value of WarrantPayeeCsePerson.
    /// </summary>
    [JsonPropertyName("warrantPayeeCsePerson")]
    public CsePerson WarrantPayeeCsePerson
    {
      get => warrantPayeeCsePerson ??= new();
      set => warrantPayeeCsePerson = value;
    }

    /// <summary>
    /// A value of WarrantPayeeCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("warrantPayeeCsePersonsWorkSet")]
    public CsePersonsWorkSet WarrantPayeeCsePersonsWorkSet
    {
      get => warrantPayeeCsePersonsWorkSet ??= new();
      set => warrantPayeeCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of WarrantStubForCsePerson.
    /// </summary>
    [JsonPropertyName("warrantStubForCsePerson")]
    public CsePerson WarrantStubForCsePerson
    {
      get => warrantStubForCsePerson ??= new();
      set => warrantStubForCsePerson = value;
    }

    /// <summary>
    /// A value of WarrantStubForCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("warrantStubForCsePersonsWorkSet")]
    public CsePersonsWorkSet WarrantStubForCsePersonsWorkSet
    {
      get => warrantStubForCsePersonsWorkSet ??= new();
      set => warrantStubForCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of NoOfPymntReqRead.
    /// </summary>
    [JsonPropertyName("noOfPymntReqRead")]
    public Common NoOfPymntReqRead
    {
      get => noOfPymntReqRead ??= new();
      set => noOfPymntReqRead = value;
    }

    /// <summary>
    /// A value of AmtOfPymntReqRead.
    /// </summary>
    [JsonPropertyName("amtOfPymntReqRead")]
    public PaymentRequest AmtOfPymntReqRead
    {
      get => amtOfPymntReqRead ??= new();
      set => amtOfPymntReqRead = value;
    }

    /// <summary>
    /// A value of NoOfPymntReqProcessed.
    /// </summary>
    [JsonPropertyName("noOfPymntReqProcessed")]
    public Common NoOfPymntReqProcessed
    {
      get => noOfPymntReqProcessed ??= new();
      set => noOfPymntReqProcessed = value;
    }

    /// <summary>
    /// A value of AmtOfPymntReqProcessed.
    /// </summary>
    [JsonPropertyName("amtOfPymntReqProcessed")]
    public PaymentRequest AmtOfPymntReqProcessed
    {
      get => amtOfPymntReqProcessed ??= new();
      set => amtOfPymntReqProcessed = value;
    }

    /// <summary>
    /// Gets a value of DtlCrd.
    /// </summary>
    [JsonIgnore]
    public Array<DtlCrdGroup> DtlCrd => dtlCrd ??= new(DtlCrdGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of DtlCrd for json serialization.
    /// </summary>
    [JsonPropertyName("dtlCrd")]
    [Computed]
    public IList<DtlCrdGroup> DtlCrd_Json
    {
      get => dtlCrd;
      set => DtlCrd.Assign(value);
    }

    /// <summary>
    /// Gets a value of SumCrd.
    /// </summary>
    [JsonIgnore]
    public Array<SumCrdGroup> SumCrd => sumCrd ??= new(SumCrdGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of SumCrd for json serialization.
    /// </summary>
    [JsonPropertyName("sumCrd")]
    [Computed]
    public IList<SumCrdGroup> SumCrd_Json
    {
      get => sumCrd;
      set => SumCrd.Assign(value);
    }

    /// <summary>
    /// A value of Payor.
    /// </summary>
    [JsonPropertyName("payor")]
    public CsePersonsWorkSet Payor
    {
      get => payor ??= new();
      set => payor = value;
    }

    /// <summary>
    /// A value of CloseInd.
    /// </summary>
    [JsonPropertyName("closeInd")]
    public Common CloseInd
    {
      get => closeInd ??= new();
      set => closeInd = value;
    }

    /// <summary>
    /// A value of InterstatWarrant.
    /// </summary>
    [JsonPropertyName("interstatWarrant")]
    public InterstatWarrant InterstatWarrant
    {
      get => interstatWarrant ??= new();
      set => interstatWarrant = value;
    }

    /// <summary>
    /// A value of ReceiptRefund.
    /// </summary>
    [JsonPropertyName("receiptRefund")]
    public ReceiptRefund ReceiptRefund
    {
      get => receiptRefund ??= new();
      set => receiptRefund = value;
    }

    /// <summary>
    /// A value of CreateCount.
    /// </summary>
    [JsonPropertyName("createCount")]
    public Common CreateCount
    {
      get => createCount ??= new();
      set => createCount = value;
    }

    /// <summary>
    /// A value of UseDesignatedPayeeInd.
    /// </summary>
    [JsonPropertyName("useDesignatedPayeeInd")]
    public Common UseDesignatedPayeeInd
    {
      get => useDesignatedPayeeInd ??= new();
      set => useDesignatedPayeeInd = value;
    }

    /// <summary>
    /// A value of PaytapeAlreadyCreated.
    /// </summary>
    [JsonPropertyName("paytapeAlreadyCreated")]
    public Common PaytapeAlreadyCreated
    {
      get => paytapeAlreadyCreated ??= new();
      set => paytapeAlreadyCreated = value;
    }

    /// <summary>
    /// A value of WarrantPayeePrint.
    /// </summary>
    [JsonPropertyName("warrantPayeePrint")]
    public CsePersonsWorkSet WarrantPayeePrint
    {
      get => warrantPayeePrint ??= new();
      set => warrantPayeePrint = value;
    }

    /// <summary>
    /// A value of WarrantMailingAddr.
    /// </summary>
    [JsonPropertyName("warrantMailingAddr")]
    public CsePersonAddress WarrantMailingAddr
    {
      get => warrantMailingAddr ??= new();
      set => warrantMailingAddr = value;
    }

    /// <summary>
    /// A value of VoucherNo.
    /// </summary>
    [JsonPropertyName("voucherNo")]
    public PayTape VoucherNo
    {
      get => voucherNo ??= new();
      set => voucherNo = value;
    }

    /// <summary>
    /// A value of MaxPrintLinesPerStub.
    /// </summary>
    [JsonPropertyName("maxPrintLinesPerStub")]
    public Common MaxPrintLinesPerStub
    {
      get => maxPrintLinesPerStub ??= new();
      set => maxPrintLinesPerStub = value;
    }

    /// <summary>
    /// A value of AgencyNumberDeleteMe.
    /// </summary>
    [JsonPropertyName("agencyNumberDeleteMe")]
    public TextWorkArea AgencyNumberDeleteMe
    {
      get => agencyNumberDeleteMe ??= new();
      set => agencyNumberDeleteMe = value;
    }

    /// <summary>
    /// A value of DeleteMe.
    /// </summary>
    [JsonPropertyName("deleteMe")]
    public Fund DeleteMe
    {
      get => deleteMe ??= new();
      set => deleteMe = value;
    }

    /// <summary>
    /// A value of MaxDiscontinue.
    /// </summary>
    [JsonPropertyName("maxDiscontinue")]
    public DateWorkArea MaxDiscontinue
    {
      get => maxDiscontinue ??= new();
      set => maxDiscontinue = value;
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
    /// A value of NullDisbursementType.
    /// </summary>
    [JsonPropertyName("nullDisbursementType")]
    public DisbursementType NullDisbursementType
    {
      get => nullDisbursementType ??= new();
      set => nullDisbursementType = value;
    }

    /// <summary>
    /// A value of NullCsePerson.
    /// </summary>
    [JsonPropertyName("nullCsePerson")]
    public CsePerson NullCsePerson
    {
      get => nullCsePerson ??= new();
      set => nullCsePerson = value;
    }

    /// <summary>
    /// A value of NullCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("nullCsePersonsWorkSet")]
    public CsePersonsWorkSet NullCsePersonsWorkSet
    {
      get => nullCsePersonsWorkSet ??= new();
      set => nullCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of NullWarrantMailingAddr.
    /// </summary>
    [JsonPropertyName("nullWarrantMailingAddr")]
    public CsePersonAddress NullWarrantMailingAddr
    {
      get => nullWarrantMailingAddr ??= new();
      set => nullWarrantMailingAddr = value;
    }

    /// <summary>
    /// A value of NullCollectionType.
    /// </summary>
    [JsonPropertyName("nullCollectionType")]
    public CollectionType NullCollectionType
    {
      get => nullCollectionType ??= new();
      set => nullCollectionType = value;
    }

    /// <summary>
    /// A value of NullCollection.
    /// </summary>
    [JsonPropertyName("nullCollection")]
    public Collection NullCollection
    {
      get => nullCollection ??= new();
      set => nullCollection = value;
    }

    /// <summary>
    /// A value of NullInterstatWarrant.
    /// </summary>
    [JsonPropertyName("nullInterstatWarrant")]
    public InterstatWarrant NullInterstatWarrant
    {
      get => nullInterstatWarrant ??= new();
      set => nullInterstatWarrant = value;
    }

    /// <summary>
    /// A value of OverflowMsgText1.
    /// </summary>
    [JsonPropertyName("overflowMsgText1")]
    public MessageTextArea OverflowMsgText1
    {
      get => overflowMsgText1 ??= new();
      set => overflowMsgText1 = value;
    }

    /// <summary>
    /// A value of OverflowMsgText2.
    /// </summary>
    [JsonPropertyName("overflowMsgText2")]
    public MessageTextArea OverflowMsgText2
    {
      get => overflowMsgText2 ??= new();
      set => overflowMsgText2 = value;
    }

    /// <summary>
    /// Gets a value of FdsoMsgText.
    /// </summary>
    [JsonIgnore]
    public Array<FdsoMsgTextGroup> FdsoMsgText => fdsoMsgText ??= new(
      FdsoMsgTextGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of FdsoMsgText for json serialization.
    /// </summary>
    [JsonPropertyName("fdsoMsgText")]
    [Computed]
    public IList<FdsoMsgTextGroup> FdsoMsgText_Json
    {
      get => fdsoMsgText;
      set => FdsoMsgText.Assign(value);
    }

    /// <summary>
    /// A value of HardcodedCoagfee.
    /// </summary>
    [JsonPropertyName("hardcodedCoagfee")]
    public DisbursementType HardcodedCoagfee
    {
      get => hardcodedCoagfee ??= new();
      set => hardcodedCoagfee = value;
    }

    /// <summary>
    /// A value of HardcodedCrfee.
    /// </summary>
    [JsonPropertyName("hardcodedCrfee")]
    public DisbursementType HardcodedCrfee
    {
      get => hardcodedCrfee ??= new();
      set => hardcodedCrfee = value;
    }

    /// <summary>
    /// A value of HardcodedDoa.
    /// </summary>
    [JsonPropertyName("hardcodedDoa")]
    public PaymentStatus HardcodedDoa
    {
      get => hardcodedDoa ??= new();
      set => hardcodedDoa = value;
    }

    /// <summary>
    /// A value of HardcodedRequested.
    /// </summary>
    [JsonPropertyName("hardcodedRequested")]
    public PaymentStatus HardcodedRequested
    {
      get => hardcodedRequested ??= new();
      set => hardcodedRequested = value;
    }

    /// <summary>
    /// A value of HardcodedZType.
    /// </summary>
    [JsonPropertyName("hardcodedZType")]
    public CollectionType HardcodedZType
    {
      get => hardcodedZType ??= new();
      set => hardcodedZType = value;
    }

    /// <summary>
    /// A value of HardcodedYType.
    /// </summary>
    [JsonPropertyName("hardcodedYType")]
    public CollectionType HardcodedYType
    {
      get => hardcodedYType ??= new();
      set => hardcodedYType = value;
    }

    /// <summary>
    /// A value of HardcodedRType.
    /// </summary>
    [JsonPropertyName("hardcodedRType")]
    public CollectionType HardcodedRType
    {
      get => hardcodedRType ??= new();
      set => hardcodedRType = value;
    }

    /// <summary>
    /// A value of HardcodedSType.
    /// </summary>
    [JsonPropertyName("hardcodedSType")]
    public CollectionType HardcodedSType
    {
      get => hardcodedSType ??= new();
      set => hardcodedSType = value;
    }

    /// <summary>
    /// A value of HardcodedKType.
    /// </summary>
    [JsonPropertyName("hardcodedKType")]
    public CollectionType HardcodedKType
    {
      get => hardcodedKType ??= new();
      set => hardcodedKType = value;
    }

    /// <summary>
    /// A value of HardcodedTType.
    /// </summary>
    [JsonPropertyName("hardcodedTType")]
    public CollectionType HardcodedTType
    {
      get => hardcodedTType ??= new();
      set => hardcodedTType = value;
    }

    /// <summary>
    /// A value of HardcodedUType.
    /// </summary>
    [JsonPropertyName("hardcodedUType")]
    public CollectionType HardcodedUType
    {
      get => hardcodedUType ??= new();
      set => hardcodedUType = value;
    }

    /// <summary>
    /// A value of HardcodedFType.
    /// </summary>
    [JsonPropertyName("hardcodedFType")]
    public CollectionType HardcodedFType
    {
      get => hardcodedFType ??= new();
      set => hardcodedFType = value;
    }

    /// <summary>
    /// A value of HardcodedPt.
    /// </summary>
    [JsonPropertyName("hardcodedPt")]
    public DisbursementType HardcodedPt
    {
      get => hardcodedPt ??= new();
      set => hardcodedPt = value;
    }

    /// <summary>
    /// A value of HardcodedExUra.
    /// </summary>
    [JsonPropertyName("hardcodedExUra")]
    public DisbursementType HardcodedExUra
    {
      get => hardcodedExUra ??= new();
      set => hardcodedExUra = value;
    }

    /// <summary>
    /// A value of Close.
    /// </summary>
    [JsonPropertyName("close")]
    public CsePersonsWorkSet Close
    {
      get => close ??= new();
      set => close = value;
    }

    /// <summary>
    /// A value of FipsRetrievedInd.
    /// </summary>
    [JsonPropertyName("fipsRetrievedInd")]
    public Common FipsRetrievedInd
    {
      get => fipsRetrievedInd ??= new();
      set => fipsRetrievedInd = value;
    }

    /// <summary>
    /// A value of TmpFips.
    /// </summary>
    [JsonPropertyName("tmpFips")]
    public Common TmpFips
    {
      get => tmpFips ??= new();
      set => tmpFips = value;
    }

    /// <summary>
    /// A value of TotalDisbTrans.
    /// </summary>
    [JsonPropertyName("totalDisbTrans")]
    public Common TotalDisbTrans
    {
      get => totalDisbTrans ??= new();
      set => totalDisbTrans = value;
    }

    /// <summary>
    /// A value of TextMsg.
    /// </summary>
    [JsonPropertyName("textMsg")]
    public TextWorkArea TextMsg
    {
      get => textMsg ??= new();
      set => textMsg = value;
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

    private Common newWarrant;
    private DateWorkArea dailyErrorReportCutoff;
    private PaymentStatus hardcodedKpc;
    private AbendData abendData;
    private Common nbrOfMembers;
    private Common memberShareOfCurrSupp;
    private Common remainingCurrentSupp;
    private Common naCurrentSuppForWarr;
    private DateWorkArea latestCollectionDate;
    private Common personExistsInGrp;
    private Array<SuppPersonsGroup> suppPersons;
    private Common noOfAeRecordsCreated;
    private InterfaceIncomeNotification interfaceIncomeNotification;
    private DisbursementType hardcodedNaCsp;
    private DisbursementType hardcodedNaCcs;
    private DateWorkArea process;
    private DateWorkArea current;
    private Common printFdsoMsgInd;
    private Common printOverflowMessageInd;
    private Common stubLinesPrinted;
    private TextWorkArea userId;
    private Common runInTestMode;
    private PaymentRequest end;
    private PaymentRequest start;
    private External payTape;
    private TextWorkArea recordType;
    private EabReportSend tempEabReportSend;
    private Common tempCommon;
    private CsePerson warrantPayeeCsePerson;
    private CsePersonsWorkSet warrantPayeeCsePersonsWorkSet;
    private CsePerson warrantStubForCsePerson;
    private CsePersonsWorkSet warrantStubForCsePersonsWorkSet;
    private Common noOfPymntReqRead;
    private PaymentRequest amtOfPymntReqRead;
    private Common noOfPymntReqProcessed;
    private PaymentRequest amtOfPymntReqProcessed;
    private Array<DtlCrdGroup> dtlCrd;
    private Array<SumCrdGroup> sumCrd;
    private CsePersonsWorkSet payor;
    private Common closeInd;
    private InterstatWarrant interstatWarrant;
    private ReceiptRefund receiptRefund;
    private Common createCount;
    private Common useDesignatedPayeeInd;
    private Common paytapeAlreadyCreated;
    private CsePersonsWorkSet warrantPayeePrint;
    private CsePersonAddress warrantMailingAddr;
    private PayTape voucherNo;
    private Common maxPrintLinesPerStub;
    private TextWorkArea agencyNumberDeleteMe;
    private Fund deleteMe;
    private DateWorkArea maxDiscontinue;
    private DateWorkArea nullDateWorkArea;
    private DisbursementType nullDisbursementType;
    private CsePerson nullCsePerson;
    private CsePersonsWorkSet nullCsePersonsWorkSet;
    private CsePersonAddress nullWarrantMailingAddr;
    private CollectionType nullCollectionType;
    private Collection nullCollection;
    private InterstatWarrant nullInterstatWarrant;
    private MessageTextArea overflowMsgText1;
    private MessageTextArea overflowMsgText2;
    private Array<FdsoMsgTextGroup> fdsoMsgText;
    private DisbursementType hardcodedCoagfee;
    private DisbursementType hardcodedCrfee;
    private PaymentStatus hardcodedDoa;
    private PaymentStatus hardcodedRequested;
    private CollectionType hardcodedZType;
    private CollectionType hardcodedYType;
    private CollectionType hardcodedRType;
    private CollectionType hardcodedSType;
    private CollectionType hardcodedKType;
    private CollectionType hardcodedTType;
    private CollectionType hardcodedUType;
    private CollectionType hardcodedFType;
    private DisbursementType hardcodedPt;
    private DisbursementType hardcodedExUra;
    private CsePersonsWorkSet close;
    private Common fipsRetrievedInd;
    private Common tmpFips;
    private Common totalDisbTrans;
    private TextWorkArea textMsg;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ObligeeCsePersonAccount.
    /// </summary>
    [JsonPropertyName("obligeeCsePersonAccount")]
    public CsePersonAccount ObligeeCsePersonAccount
    {
      get => obligeeCsePersonAccount ??= new();
      set => obligeeCsePersonAccount = value;
    }

    /// <summary>
    /// A value of ObligeeCsePerson.
    /// </summary>
    [JsonPropertyName("obligeeCsePerson")]
    public CsePerson ObligeeCsePerson
    {
      get => obligeeCsePerson ??= new();
      set => obligeeCsePerson = value;
    }

    /// <summary>
    /// A value of ExistingKpc.
    /// </summary>
    [JsonPropertyName("existingKpc")]
    public PaymentStatus ExistingKpc
    {
      get => existingKpc ??= new();
      set => existingKpc = value;
    }

    /// <summary>
    /// A value of PersonProgram.
    /// </summary>
    [JsonPropertyName("personProgram")]
    public PersonProgram PersonProgram
    {
      get => personProgram ??= new();
      set => personProgram = value;
    }

    /// <summary>
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    /// <summary>
    /// A value of Debt.
    /// </summary>
    [JsonPropertyName("debt")]
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
    }

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
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePerson Supported
    {
      get => supported ??= new();
      set => supported = value;
    }

    /// <summary>
    /// A value of ExistingWarrant.
    /// </summary>
    [JsonPropertyName("existingWarrant")]
    public PaymentRequest ExistingWarrant
    {
      get => existingWarrant ??= new();
      set => existingWarrant = value;
    }

    /// <summary>
    /// A value of ExistingPaymentStatusHistory.
    /// </summary>
    [JsonPropertyName("existingPaymentStatusHistory")]
    public PaymentStatusHistory ExistingPaymentStatusHistory
    {
      get => existingPaymentStatusHistory ??= new();
      set => existingPaymentStatusHistory = value;
    }

    /// <summary>
    /// A value of ExistingPaymentStatus.
    /// </summary>
    [JsonPropertyName("existingPaymentStatus")]
    public PaymentStatus ExistingPaymentStatus
    {
      get => existingPaymentStatus ??= new();
      set => existingPaymentStatus = value;
    }

    /// <summary>
    /// A value of NewPaymentStatusHistory.
    /// </summary>
    [JsonPropertyName("newPaymentStatusHistory")]
    public PaymentStatusHistory NewPaymentStatusHistory
    {
      get => newPaymentStatusHistory ??= new();
      set => newPaymentStatusHistory = value;
    }

    /// <summary>
    /// A value of ExistingDebit.
    /// </summary>
    [JsonPropertyName("existingDebit")]
    public DisbursementTransaction ExistingDebit
    {
      get => existingDebit ??= new();
      set => existingDebit = value;
    }

    /// <summary>
    /// A value of ExistingCredit.
    /// </summary>
    [JsonPropertyName("existingCredit")]
    public DisbursementTransaction ExistingCredit
    {
      get => existingCredit ??= new();
      set => existingCredit = value;
    }

    /// <summary>
    /// A value of ExistingDebitDtl.
    /// </summary>
    [JsonPropertyName("existingDebitDtl")]
    public DisbursementTransaction ExistingDebitDtl
    {
      get => existingDebitDtl ??= new();
      set => existingDebitDtl = value;
    }

    /// <summary>
    /// A value of ExistingCreditToDebit.
    /// </summary>
    [JsonPropertyName("existingCreditToDebit")]
    public DisbursementTransactionRln ExistingCreditToDebit
    {
      get => existingCreditToDebit ??= new();
      set => existingCreditToDebit = value;
    }

    /// <summary>
    /// A value of ExistinbDebitToCredit.
    /// </summary>
    [JsonPropertyName("existinbDebitToCredit")]
    public DisbursementTransactionRln ExistinbDebitToCredit
    {
      get => existinbDebitToCredit ??= new();
      set => existinbDebitToCredit = value;
    }

    /// <summary>
    /// A value of ExistingDisbursementType.
    /// </summary>
    [JsonPropertyName("existingDisbursementType")]
    public DisbursementType ExistingDisbursementType
    {
      get => existingDisbursementType ??= new();
      set => existingDisbursementType = value;
    }

    /// <summary>
    /// A value of NewWarrantRemailAddress.
    /// </summary>
    [JsonPropertyName("newWarrantRemailAddress")]
    public WarrantRemailAddress NewWarrantRemailAddress
    {
      get => newWarrantRemailAddress ??= new();
      set => newWarrantRemailAddress = value;
    }

    /// <summary>
    /// A value of NewPayTape.
    /// </summary>
    [JsonPropertyName("newPayTape")]
    public PayTape NewPayTape
    {
      get => newPayTape ??= new();
      set => newPayTape = value;
    }

    /// <summary>
    /// A value of ExistingFips.
    /// </summary>
    [JsonPropertyName("existingFips")]
    public Fips ExistingFips
    {
      get => existingFips ??= new();
      set => existingFips = value;
    }

    /// <summary>
    /// A value of ExistingInterstateRequest.
    /// </summary>
    [JsonPropertyName("existingInterstateRequest")]
    public InterstateRequest ExistingInterstateRequest
    {
      get => existingInterstateRequest ??= new();
      set => existingInterstateRequest = value;
    }

    /// <summary>
    /// A value of ExistingInterstatePaymentAddress.
    /// </summary>
    [JsonPropertyName("existingInterstatePaymentAddress")]
    public InterstatePaymentAddress ExistingInterstatePaymentAddress
    {
      get => existingInterstatePaymentAddress ??= new();
      set => existingInterstatePaymentAddress = value;
    }

    /// <summary>
    /// A value of ExistingReceiptRefund.
    /// </summary>
    [JsonPropertyName("existingReceiptRefund")]
    public ReceiptRefund ExistingReceiptRefund
    {
      get => existingReceiptRefund ??= new();
      set => existingReceiptRefund = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptDetailAddress.
    /// </summary>
    [JsonPropertyName("existingCashReceiptDetailAddress")]
    public CashReceiptDetailAddress ExistingCashReceiptDetailAddress
    {
      get => existingCashReceiptDetailAddress ??= new();
      set => existingCashReceiptDetailAddress = value;
    }

    /// <summary>
    /// A value of ExistingDoa.
    /// </summary>
    [JsonPropertyName("existingDoa")]
    public PaymentStatus ExistingDoa
    {
      get => existingDoa ??= new();
      set => existingDoa = value;
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
    /// A value of ExistingCaseRole.
    /// </summary>
    [JsonPropertyName("existingCaseRole")]
    public CaseRole ExistingCaseRole
    {
      get => existingCaseRole ??= new();
      set => existingCaseRole = value;
    }

    /// <summary>
    /// A value of ExistingKeyOnly.
    /// </summary>
    [JsonPropertyName("existingKeyOnly")]
    public DisbursementTransaction ExistingKeyOnly
    {
      get => existingKeyOnly ??= new();
      set => existingKeyOnly = value;
    }

    /// <summary>
    /// A value of ExistingPayor.
    /// </summary>
    [JsonPropertyName("existingPayor")]
    public CsePerson ExistingPayor
    {
      get => existingPayor ??= new();
      set => existingPayor = value;
    }

    /// <summary>
    /// A value of ExistingObligor.
    /// </summary>
    [JsonPropertyName("existingObligor")]
    public CsePersonAccount ExistingObligor
    {
      get => existingObligor ??= new();
      set => existingObligor = value;
    }

    /// <summary>
    /// A value of ExistingObligation.
    /// </summary>
    [JsonPropertyName("existingObligation")]
    public Obligation ExistingObligation
    {
      get => existingObligation ??= new();
      set => existingObligation = value;
    }

    /// <summary>
    /// A value of ExistingDebt.
    /// </summary>
    [JsonPropertyName("existingDebt")]
    public ObligationTransaction ExistingDebt
    {
      get => existingDebt ??= new();
      set => existingDebt = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("existingCashReceiptSourceType")]
    public CashReceiptSourceType ExistingCashReceiptSourceType
    {
      get => existingCashReceiptSourceType ??= new();
      set => existingCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("existingCashReceiptEvent")]
    public CashReceiptEvent ExistingCashReceiptEvent
    {
      get => existingCashReceiptEvent ??= new();
      set => existingCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of ExistingCashReceipt.
    /// </summary>
    [JsonPropertyName("existingCashReceipt")]
    public CashReceipt ExistingCashReceipt
    {
      get => existingCashReceipt ??= new();
      set => existingCashReceipt = value;
    }

    /// <summary>
    /// A value of ExistingCollectionType.
    /// </summary>
    [JsonPropertyName("existingCollectionType")]
    public CollectionType ExistingCollectionType
    {
      get => existingCollectionType ??= new();
      set => existingCollectionType = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("existingCashReceiptDetail")]
    public CashReceiptDetail ExistingCashReceiptDetail
    {
      get => existingCashReceiptDetail ??= new();
      set => existingCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of ExistingCollection.
    /// </summary>
    [JsonPropertyName("existingCollection")]
    public Collection ExistingCollection
    {
      get => existingCollection ??= new();
      set => existingCollection = value;
    }

    private CsePersonAccount obligeeCsePersonAccount;
    private CsePerson obligeeCsePerson;
    private PaymentStatus existingKpc;
    private PersonProgram personProgram;
    private Program program;
    private ObligationTransaction debt;
    private CsePersonAccount csePersonAccount;
    private CsePerson supported;
    private PaymentRequest existingWarrant;
    private PaymentStatusHistory existingPaymentStatusHistory;
    private PaymentStatus existingPaymentStatus;
    private PaymentStatusHistory newPaymentStatusHistory;
    private DisbursementTransaction existingDebit;
    private DisbursementTransaction existingCredit;
    private DisbursementTransaction existingDebitDtl;
    private DisbursementTransactionRln existingCreditToDebit;
    private DisbursementTransactionRln existinbDebitToCredit;
    private DisbursementType existingDisbursementType;
    private WarrantRemailAddress newWarrantRemailAddress;
    private PayTape newPayTape;
    private Fips existingFips;
    private InterstateRequest existingInterstateRequest;
    private InterstatePaymentAddress existingInterstatePaymentAddress;
    private ReceiptRefund existingReceiptRefund;
    private CashReceiptDetailAddress existingCashReceiptDetailAddress;
    private PaymentStatus existingDoa;
    private Case1 existingCase;
    private CaseRole existingCaseRole;
    private DisbursementTransaction existingKeyOnly;
    private CsePerson existingPayor;
    private CsePersonAccount existingObligor;
    private Obligation existingObligation;
    private ObligationTransaction existingDebt;
    private CashReceiptSourceType existingCashReceiptSourceType;
    private CashReceiptEvent existingCashReceiptEvent;
    private CashReceipt existingCashReceipt;
    private CollectionType existingCollectionType;
    private CashReceiptDetail existingCashReceiptDetail;
    private Collection existingCollection;
  }
#endregion
}
