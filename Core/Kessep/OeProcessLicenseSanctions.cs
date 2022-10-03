// Program: OE_PROCESS_LICENSE_SANCTIONS, ID: 371320727, model: 746.
// Short name: SWE03598
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_PROCESS_LICENSE_SANCTIONS.
/// </summary>
[Serializable]
public partial class OeProcessLicenseSanctions: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_PROCESS_LICENSE_SANCTIONS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeProcessLicenseSanctions(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeProcessLicenseSanctions.
  /// </summary>
  public OeProcessLicenseSanctions(IContext context, Import import,
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
    // **********************************************************************************************
    // Initial Code       Dwayne Dupree        04/13/2007
    // This is  main process code for the kdwp process,  it is very similar to 
    // the code for
    // the kdmv process.
    // 08/23/2010      GVandy		CQ19415 - Add parameters signifying 1) the number
    // of days in the past
    // 				that an obligation must have been created before its debts are
    // 				included in the arrears calculations  2) the number of days in the
    // 				past that the obligation began accruing (or the debt due
    // 				date for non accruing obligations).
    // **********************************************************************************************
    local.AdministrativeAction.Type1 = "KDWP";
    local.StartCommon.Count = 1;
    local.Current.Count = 1;
    local.CurrentPosition.Count = 1;
    local.FieldNumber.Count = 0;
    local.IncludeArrearsOnly.Flag = "";
    export.RecordProcessed.Count = import.RecordProcessed.Count + 1;
    local.ProgramProcessingInfo.Assign(import.ProgramProcessingInfo);
    MoveProgramCheckpointRestart(import.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
    local.IncludeArrearsOnly.Flag = "N";

    do
    {
      local.Postion.Text1 =
        Substring(local.ProgramProcessingInfo.ParameterList,
        local.CurrentPosition.Count, 1);

      if (AsChar(local.Postion.Text1) == ',')
      {
        ++local.FieldNumber.Count;
        local.WorkArea.Text15 = "";

        switch(local.FieldNumber.Count)
        {
          case 1:
            if (local.Current.Count == 1)
            {
              local.MiniumTarget.TotalCurrency = 0;
            }
            else
            {
              local.WorkArea.Text15 =
                Substring(local.ProgramProcessingInfo.ParameterList,
                local.StartCommon.Count, local.Current.Count - 1);
              local.MiniumTarget.TotalCurrency =
                StringToNumber(local.WorkArea.Text15);
            }

            local.StartCommon.Count = local.CurrentPosition.Count + 1;
            local.Current.Count = 0;

            break;
          case 2:
            if (local.Current.Count == 1)
            {
              local.NumberOfDays.Count = 0;
            }
            else
            {
              local.WorkArea.Text15 =
                Substring(local.ProgramProcessingInfo.ParameterList,
                local.StartCommon.Count, local.Current.Count - 1);
              local.NumberOfDays.Count =
                (int)StringToNumber(local.WorkArea.Text15);
            }

            local.StartCommon.Count = local.CurrentPosition.Count + 1;
            local.Current.Count = 0;

            break;
          case 3:
            if (local.Current.Count == 1)
            {
              local.StartDate.Date = Now().Date;
            }
            else
            {
              local.WorkArea.Text15 =
                Substring(local.ProgramProcessingInfo.ParameterList,
                local.StartCommon.Count, local.Current.Count - 1);
              local.StartDate.Date = StringToDate(local.WorkArea.Text15);
            }

            local.StartCommon.Count = local.CurrentPosition.Count + 1;
            local.Current.Count = 0;

            break;
          case 4:
            if (local.Current.Count == 1)
            {
              local.MiniumPayment.TotalCurrency = 0;
            }
            else
            {
              local.WorkArea.Text15 =
                Substring(local.ProgramProcessingInfo.ParameterList,
                local.StartCommon.Count, local.Current.Count - 1);
              local.MiniumPayment.TotalCurrency =
                StringToNumber(local.WorkArea.Text15);
            }

            local.StartCommon.Count = local.CurrentPosition.Count + 1;
            local.Current.Count = 0;

            break;
          case 5:
            if (local.Current.Count == 1)
            {
              local.IncludeArrearsOnly.Flag = "N";
            }
            else
            {
              local.IncludeArrearsOnly.Flag =
                Substring(local.ProgramProcessingInfo.ParameterList,
                local.StartCommon.Count, local.Current.Count - 1);
            }

            local.StartCommon.Count = local.CurrentPosition.Count + 1;
            local.Current.Count = 0;

            break;
          case 6:
            // -- 08/23/2010 CQ19415 - Add support for parameter indicating the 
            // number of days in the past an obligation must have been created.
            if (local.Current.Count == 1)
            {
              local.NumDaysSinceObCreated.Count = 0;
            }
            else
            {
              local.WorkArea.Text15 =
                Substring(local.ProgramProcessingInfo.ParameterList,
                local.StartCommon.Count, local.Current.Count - 1);
              local.NumDaysSinceObCreated.Count =
                (int)StringToNumber(local.WorkArea.Text15);
            }

            local.StartCommon.Count = local.CurrentPosition.Count + 1;
            local.Current.Count = 0;

            break;
          case 7:
            // -- 08/23/2010 CQ19415 - Add support for parameter indicating the 
            // number of days in the past that the obligation began accruing (or
            // the debt due date for non accruing obligations).
            if (local.Current.Count == 1)
            {
              local.NumDaysAccruingOrDue.Count = 0;
            }
            else
            {
              local.WorkArea.Text15 =
                Substring(local.ProgramProcessingInfo.ParameterList,
                local.StartCommon.Count, local.Current.Count - 1);
              local.NumDaysAccruingOrDue.Count =
                (int)StringToNumber(local.WorkArea.Text15);
            }

            local.StartCommon.Count = local.CurrentPosition.Count + 1;
            local.Current.Count = 0;

            break;
          case 8:
            if (local.Current.Count == 1)
            {
              local.NumMonthsIwoPeriod.Count = 12;
            }
            else
            {
              local.WorkArea.Text15 =
                Substring(local.ProgramProcessingInfo.ParameterList,
                local.StartCommon.Count, local.Current.Count - 1);
              local.NumMonthsIwoPeriod.Count =
                (int)StringToNumber(local.WorkArea.Text15);
            }

            local.StartCommon.Count = local.CurrentPosition.Count + 1;
            local.Current.Count = 0;

            break;
          default:
            break;
        }
      }
      else if (IsEmpty(local.Postion.Text1))
      {
        break;
      }

      ++local.CurrentPosition.Count;
      ++local.Current.Count;
    }
    while(!Equal(global.Command, "COMMAND"));

    local.StartBatchTimestampWorkArea.IefTimestamp = Now();
    local.CsePerson.Number =
      Substring(local.ProgramCheckpointRestart.RestartInfo, 1, 10);
    local.RestartLegalId.Text15 =
      Substring(local.ProgramCheckpointRestart.RestartInfo, 11, 15);
    local.RestartAmount.Text15 =
      Substring(local.ProgramCheckpointRestart.RestartInfo, 26, 15);
    local.PersonTotal.TotalCurrency =
      StringToNumber(local.RestartAmount.Text15) / (decimal)100;
    local.LegalAction.Identifier =
      (int)StringToNumber(local.RestartLegalId.Text15);
    local.TotalAmount.Text15 =
      Substring(local.ProgramCheckpointRestart.RestartInfo, 41, 15);
    local.TotalNumberRecordsRead.Text15 =
      Substring(local.ProgramCheckpointRestart.RestartInfo, 56, 15);
    local.NumOfRecordsProcessed.Text15 =
      Substring(local.ProgramCheckpointRestart.RestartInfo, 71, 15);
    local.TotalNumberOfErrors.Text15 =
      Substring(local.ProgramCheckpointRestart.RestartInfo, 86, 15);
    export.TotalAmtDebtOwed.TotalCurrency =
      StringToNumber(local.TotalAmount.Text15) / (decimal)100;
    export.TotalNumRecsAdded.Count =
      (int)StringToNumber(local.NumOfRecordsProcessed.Text15);
    export.NumberOfRecordsRead.Count =
      (int)StringToNumber(local.TotalNumberRecordsRead.Text15);
    export.TotalNumErrorRecords.Count =
      (int)StringToNumber(local.TotalNumberOfErrors.Text15);

    if (local.MiniumPayment.TotalCurrency <= 0)
    {
      local.MiniumPayment.TotalCurrency = 0;
    }

    local.From.Date = AddDays(local.StartDate.Date, -local.NumberOfDays.Count);
    local.To.Date = local.StartDate.Date;
    local.ZeroDate.Date = new DateTime(1, 1, 1);
    local.EndDate.Date = new DateTime(2099, 12, 31);
    local.TotalNumberOfErrorsRec.Count = 0;

    if (export.TotalNumErrorRecords.Count <= 0)
    {
      export.TotalNumErrorRecords.Count = 0;
    }

    if (export.NumberOfRecordsRead.Count <= 0)
    {
      export.NumberOfRecordsRead.Count = 0;
    }

    if (export.TotalNumRecsAdded.Count <= 0)
    {
      export.TotalNumRecsAdded.Count = 0;
    }

    local.PrepareDate.NumericalMonth = Month(local.StartDate.Date);
    local.PrepareDate.NumericalYear = Year(local.StartDate.Date);
    local.PrepareDate.TextYear =
      NumberToString(local.PrepareDate.NumericalYear, 12, 4);
    local.PrepareDate.TextMonth =
      NumberToString(local.PrepareDate.NumericalMonth, 14, 2);
    local.Date.Text10 = local.PrepareDate.TextYear + "-" + local
      .PrepareDate.TextMonth + "-01";
    local.Base1.Date = StringToDate(local.Date.Text10);
    local.IwoPeriod1.Count = 0;
    local.IwoPeriod.Index = -1;
    local.IwoPeriod.Count = 0;

    while(local.IwoPeriod1.Count < 100)
    {
      ++local.IwoPeriod1.Count;

      if (local.IwoPeriod1.Count > local.NumMonthsIwoPeriod.Count)
      {
        break;
      }

      ++local.IwoPeriod.Index;
      local.IwoPeriod.CheckSize();

      local.IwoPeriod.Update.BeginDate.Date =
        AddMonths(local.Base1.Date, -local.IwoPeriod1.Count);
      local.IwoPeriod.Update.EndDate.Date =
        AddMonths(local.IwoPeriod.Item.BeginDate.Date, 1);
      local.IwoPeriod.Update.EndDate.Date =
        AddDays(local.IwoPeriod.Item.EndDate.Date, -1);
    }

    if (!IsEmpty(local.CsePerson.Number))
    {
      local.Search.Flag = "3";
      local.Phonetic.Percentage = 100;
      local.StartCsePersonsWorkSet.Number = local.CsePerson.Number;

      // Since this program has been restarted, we have to set up the previous 
      // process
      // fields so they can finish processing.
    }

    ExitState = "ACO_NN0000_ALL_OK";

    foreach(var item in ReadCsePersonLegalAction())
    {
      if (AsChar(import.SinglePass.Flag) == 'Y')
      {
        if (!Equal(local.CsePerson.Number, entities.CsePerson.Number))
        {
          return;
        }
      }

      ++export.NumberOfRecordsRead.Count;

      if (Equal(local.SkipObligor.Number, entities.CsePerson.Number))
      {
        // bad ssn or dob for obligor, so do not try to process
        continue;
      }
      else
      {
        local.SkipObligor.Number = "";
      }

      if (AsChar(local.AlreadyDone.Flag) == 'Y')
      {
        // this court order number has already been checked, so there is no need
        // to process it again
        if (Equal(entities.LegalAction.StandardNumber, local.Hold.StandardNumber))
          
        {
          if (Equal(local.CsePerson.Number, entities.CsePerson.Number))
          {
            continue;
          }
          else
          {
            local.Hold.StandardNumber = entities.LegalAction.StandardNumber;
            local.AlreadyDone.Flag = "";
            local.TotalCollection.TotalCurrency = 0;
          }
        }
        else
        {
          local.Hold.StandardNumber = entities.LegalAction.StandardNumber;
          local.AlreadyDone.Flag = "";
          local.TotalCollection.TotalCurrency = 0;
        }
      }
      else
      {
        local.Hold.StandardNumber = entities.LegalAction.StandardNumber;
        local.TotalCollection.TotalCurrency = 0;
      }

      if (Equal(local.CsePerson.Number, entities.CsePerson.Number))
      {
        if (AsChar(local.ContinueProcess.Flag) != 'Y')
        {
          continue;
        }
      }
      else
      {
        local.PreviousfindMe.Number = local.CsePerson.Number;
        local.CsePerson.Number = entities.CsePerson.Number;
        local.ContinueProcess.Flag = "";
      }

      if (AsChar(local.ContinueProcess.Flag) != 'Y')
      {
        // a quick check to see if this obligor is still active and owes 
        // anything
        local.AmountOwed.TotalCurrency = 0;

        foreach(var item1 in ReadDebtDetail())
        {
          local.AmountOwed.TotalCurrency += entities.DebtDetail.BalanceDueAmt;

          if (local.AmountOwed.TotalCurrency > 0)
          {
            local.ContinueProcess.Flag = "Y";

            goto Test1;
          }
        }
      }

Test1:

      if (AsChar(local.ContinueProcess.Flag) != 'Y')
      {
        continue;
      }

      // DEBT_TYP_ID  DEBT_TYP_CD  
      // DEBT_TYP_NM
      // 
      // DEBT_TYP_CLA
      // ---------+---------+---------+---------+---------+---------+---------+
      // ---------+
      //           1  CS           CHILD SUPPORT                             A
      //           2  SP           SPOUSAL SUPPORT                           A
      //           3  MS           MEDICAL SUPPORT                           A
      //           4  IVD RC       IV-D RECOVERY                             R
      //           5  IRS NEG      IRS NEGATIVE RECOVERY                     R
      //           6  MIS AR       AR MISDIRECTED PAYMENT                    R
      //           7  MIS AP       AP MISDIRECTED PAYMENT                    R
      //           8  MIS NON      NON-CASE PERSON MISDIRECTED PAYMENT       R
      //           9  BDCK RC      BAD 
      // CHECK
      // 
      // R
      //          10  MJ           MEDICAL JUDGEMENT                         M
      //          11  %UME         PERCENT UNINSURED MEDICAL EXP JUDGEMENT   M
      //          12  IJ           INTEREST JUDGEMENT                        N
      //          13  AJ           ARREARS JUDGEMENT                         N
      //          14  CRCH         COST OF RAISING CHILD                     N
      //          15  FEE          GENETIC FEE TEST                          F
      //          16  VOL          
      // VOLUNTARY
      // 
      // V
      //          17  SAJ          SPOUSAL ARREARS JUDGEMENT                 N
      //          18  718B         718B JUDGEMENT                            N
      //          19  MC           MEDICAL COSTS                             A
      //          20  WA           WITHHOLDING ARREARS                       N
      //          21  WC           WITHHOLDING CURRENT                       A
      //          22  GIFT         GIFT
      // 
      // V
      local.ReadExemp.StandardNumber = entities.LegalAction.StandardNumber;

      foreach(var item1 in ReadObligationAdmActionExemption())
      {
        goto ReadEach2;

        // next legal action
      }

      local.TotalCollection.TotalCurrency = 0;

      foreach(var item1 in ReadCashReceiptDetail())
      {
        // CRDETAIL_STAT_ID  CODE        NAME
        // ---------+---------+---------+---------+---------+---------
        //                1  REC         RECORDED
        //                2  ADJ         ADJUSTED
        //                3  SUSP        SUSPENDED
        //                4  DIST        DISTRIBUTED
        //                5  REF         REFUNDED
        //                6  REL         RELEASED FOR DISTRIBUTION
        //                7  PEND        PENDED
        //                8  REIPDELETE  REIP DELETE
        foreach(var item2 in ReadCollectionType())
        {
          if (entities.CollectionType.SequentialIdentifier == 16 || entities
            .CollectionType.SequentialIdentifier == 2 || entities
            .CollectionType.SequentialIdentifier == 17 || entities
            .CollectionType.SequentialIdentifier == 18 || entities
            .CollectionType.SequentialIdentifier == 11 || entities
            .CollectionType.SequentialIdentifier == 14 || entities
            .CollectionType.SequentialIdentifier == 20 || entities
            .CollectionType.SequentialIdentifier == 21)
          {
            goto ReadEach1;

            // next CASH RECEIPT
          }

          // COLLECTION_TYPE_ID  PRINT_NAME            CODE        NAME
          // ---------+---------+---------+---------+---------+---------+
          // ---------+---------
          //                  1  REGULAR COLLECTION    C           REGULAR 
          // COLLECTION
          //                  2  IV-D RECOVERY         D           IV-D RECOVERY
          //                  3  FEDERAL OFFSET        F           FEDERAL 
          // OFFSET
          //                  4  STATE OFFSET          S           STATE OFFSET-
          // MISC
          //                  5  STATE OFFSET          U           UNEMPLOYMENT 
          // OFFSET
          //                  6  INCOME WITHHOLDING    I           INCOME 
          // WITHHOLDING
          //                  9  FEE PAYMENT           P           FEE PAYMENT
          //                 10  STATE OFFSET          K           KPERS
          //                 11  STATE OFFSET          R           STATE 
          // RECOVERY
          //                 14  DIR PMT AP            4           DIRECT 
          // PAYMENT - AP
          //                 15  COLLECTION AGENCY     A           COLLECTION 
          // AGENCY
          //                 16  BAD CK REC            B           BAD CHECK 
          // RECOVERY
          //                 17  MIS DIR REC           M           MISDIRECTED 
          // AR, AP, NON
          //                 18  1040X                 N           1040X 
          // RECOVERY
          //                 19  FEDERAL OFFSET        T           TREASURY 
          // OFFSET
          //                 20  DIR PMT CT            5           DIRECT 
          // PAYMENT - COURT
          //               21  DIR PMT CRU           6           DIRECT PAYMENT 
          // - CRU
          //               23  VOLUNTARY PAYMENT     V           VOLUNTARY 
          // PAYMENT
          //               25  TREASURY OFFSET RET   Y           TREASURY OFFSET
          // - RETIREME
          //               26  TREASURY OFFSET VEN   Z           TREASURY OFFSET
          // - VENDOR
          //               27  CSENET/IRS TAX INTCP  7           CSENET/IRS TAX 
          // INTERCEPT
          //               28  CSENET/ST TAX INTCP   8           CSENET/STATE 
          // TAX INTERCEPT
          //               29  CSENET/UI TAX INTCP   9           CSENET/UI
        }

        if (ReadCashReceiptType())
        {
          if (entities.CashReceiptType.SystemGeneratedIdentifier == 2 || entities
            .CashReceiptType.SystemGeneratedIdentifier == 7 || entities
            .CashReceiptType.SystemGeneratedIdentifier == 11)
          {
            continue;

            // CRTYPE_ID  CODE        CATEGORY_IND  NAME
            // ---------+---------+---------+---------+---------+---------+
            // ---------+--------
            //         1  CHECK       C             CHECK
            //         2  FCRT REC    N             COURT RECORD - ENTERED BY 
            // FIELD
            //         3  MNY ORD     C             MONEY ORDER
            //         4  CURRENCY    C             CURRENCY
            //         5  CRDT CRD    C             CREDIT CARD
            //         6  EFT         C             ELECTRONIC FUNDS TRANSFER
            //         7  FDIR PMT    N             DIRECT PAY FROM AP TO AR - 
            // REC BY FIELD
            //         8  CSENET      N             CSENET NOTIFIED
            //         9  CRT REC     C             COURT RECORD
            //        10  INTERFUND   C             INTERFUND VOUCHER
            //        11  RDIR PMT    N             DIRECT PAY FROM AP TO AR - 
            // REC BY CRU
            //        12  MANINT      C             MANUAL INTERFACE
            // do not want fcrt rec, fdir pmt, rdir pmt
          }
        }

        local.TotalCollection.TotalCurrency += entities.CashReceiptDetail.
          CollectionAmount;

        if (local.TotalCollection.TotalCurrency > local
          .MiniumPayment.TotalCurrency)
        {
          local.AlreadyDone.Flag = "Y";

          goto ReadEach2;

          // get new legal action, no need to check any more obligations for 
          // this court order number because the minium payment has been met
        }

ReadEach1:
        ;
      }

      if (local.TotalCollection.TotalCurrency > local
        .MiniumPayment.TotalCurrency)
      {
        local.AlreadyDone.Flag = "Y";

        continue;

        // get new legal action
      }

      foreach(var item1 in ReadCase())
      {
        if (ReadGoodCause())
        {
          // co=cooperting
          // gc=good cause
          // pd=pending
          if (Equal(entities.GoodCause.Code, "GC") || Equal
            (entities.GoodCause.Code, "PD"))
          {
            // next legal action
            goto ReadEach2;
          }

          break;
        }
      }

      local.OwedAmount.TotalCurrency = 0;
      local.ScreenOwedAmountsDtl.Assign(local.ClearScreenOwedAmountsDtl);

      // to see how much is owed by court order
      if (!IsEmpty(entities.LegalAction.StandardNumber))
      {
        if (Equal(entities.LegalAction.StandardNumber,
          local.AlreadyCheckedCtNumLegalAction.StandardNumber) && Equal
          (entities.CsePerson.Number, local.AlreadyCheckedCtNumCsePerson.Number))
          
        {
          continue;
        }

        UseFnComputeTotalsForCtOrder2();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          goto Test2;
        }

        local.OwedAmount.TotalCurrency =
          local.ScreenOwedAmountsDtl.TotalArrearsOwed + local
          .ScreenOwedAmountsDtl.TotalInterestOwed - (
            local.ScreenOwedAmountsDtl.TotalArrearsColl + local
          .ScreenOwedAmountsDtl.TotalInterestColl + local
          .ScreenOwedAmountsDtl.TotalVoluntaryColl + local
          .ScreenOwedAmountsDtl.UndistributedAmt);
        local.AlreadyCheckedCtNumLegalAction.StandardNumber =
          entities.LegalAction.StandardNumber;
        local.AlreadyCheckedCtNumCsePerson.Number = entities.CsePerson.Number;
      }

Test2:

      local.ProcessLastRecord.Flag = "";

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        if (local.OwedAmount.TotalCurrency > local.MiniumTarget.TotalCurrency)
        {
          // To see if any payments have been made that are still in cash 
          // receipt and have
          // not made to collections yet. There has not been enough money in the
          // collection
          // table to meet the minium payment
          local.MeetsCriteria.Flag = "";

          if (ReadLegalAction())
          {
            local.IwoPayment.TotalCurrency = 0;

            if (Equal(entities.Iwo.ActionTaken, "IWOMODO") || Equal
              (entities.Iwo.ActionTaken, "IWONOTKM") || Equal
              (entities.Iwo.ActionTaken, "IWONOTKS") || Equal
              (entities.Iwo.ActionTaken, "IWO"))
            {
              foreach(var item1 in ReadLegalActionDetail())
              {
                local.IwoPayment.TotalCurrency =
                  entities.LegalActionDetail.CurrentAmount.GetValueOrDefault() +
                  entities
                  .LegalActionDetail.ArrearsAmount.GetValueOrDefault() + local
                  .IwoPayment.TotalCurrency;
              }

              UseOeDeterminePaymentsKdwp();

              if (AsChar(local.MeetsCriteria.Flag) == 'N')
              {
              }
              else
              {
                continue;
              }
            }
            else
            {
            }
          }

          if (!Equal(entities.CsePerson.Number, local.AlreadyCheckedSsn.Number))
          {
            if (!IsEmpty(local.CsePersonsWorkSet.Ssn))
            {
              local.PreviousProcess.Assign(local.CsePersonsWorkSet);
              MoveLegalAction(local.LegalAction, local.Previous);
              local.PreviousPersonTotal.TotalCurrency =
                local.PersonTotal.TotalCurrency;
            }

            local.PersonTotal.TotalCurrency = 0;
            local.Search.Flag = "3";
            local.Phonetic.Percentage = 100;
            local.StartCsePersonsWorkSet.Number = entities.CsePerson.Number;
            local.AlreadyCheckedSsn.Number = entities.CsePerson.Number;
            local.CsePersonsWorkSet.Assign(local.ClearCsePersonsWorkSet);
            UseSiReadCsePersonBatch();
            MoveLegalAction(entities.LegalAction, local.LegalAction);

            if (IsEmpty(local.CsePersonsWorkSet.Ssn) || Equal
              (local.CsePersonsWorkSet.Ssn, "000000000") || !
              Lt(new DateTime(1, 1, 1), local.CsePersonsWorkSet.Dob))
            {
              if (!Lt(new DateTime(1, 1, 1), local.CsePersonsWorkSet.Dob))
              {
                local.DateWorkArea.TextDate = "";
              }
              else
              {
                local.DateWorkArea.Year = Year(local.CsePersonsWorkSet.Dob);
                local.WorkArea.Text15 =
                  NumberToString(local.DateWorkArea.Year, 15);
                local.Year.Text4 = Substring(local.WorkArea.Text15, 12, 4);
                local.DateWorkArea.Month = Month(local.CsePersonsWorkSet.Dob);
                local.WorkArea.Text15 =
                  NumberToString(local.DateWorkArea.Month, 15);
                local.Month.Text2 = Substring(local.WorkArea.Text15, 14, 2);
                local.DateWorkArea.Day = Day(local.CsePersonsWorkSet.Dob);
                local.WorkArea.Text15 =
                  NumberToString(local.DateWorkArea.Day, 15);
                local.Day.Text2 = Substring(local.WorkArea.Text15, 14, 2);
                local.DateWorkArea.TextDate = local.Year.Text4 + local
                  .Month.Text2 + local.Day.Text2;
              }

              if (Equal(local.CsePersonsWorkSet.Ssn, "000000000"))
              {
                local.MissingSsnAndDodRpt.Ssn = "";
              }
              else
              {
                local.MissingSsnAndDodRpt.Ssn = local.CsePersonsWorkSet.Ssn;
              }

              local.MissingSsnAndDodRpt.CsePersonNumber =
                entities.CsePerson.Number;
              local.MissingSsnAndDodRpt.Dob = local.DateWorkArea.TextDate;
              local.MissingSsnAndDodRpt.LastName =
                local.CsePersonsWorkSet.LastName;
              local.MissingSsnAndDodRpt.FirstName =
                local.CsePersonsWorkSet.FirstName;
              local.MissingSsnAndDodRpt.MiddleInt =
                local.CsePersonsWorkSet.MiddleInitial;
              local.MissingSsnAndDodRpt.MissingAttribute = "";

              // A = MISSING SSN
              // B = MISSING DOB
              // C = MISSING BOTH
              if (IsEmpty(local.MissingSsnAndDodRpt.Ssn) && !
                IsEmpty(local.MissingSsnAndDodRpt.Dob))
              {
                local.MissingSsnAndDodRpt.MissingAttribute = "A";
              }

              if (!IsEmpty(local.MissingSsnAndDodRpt.Ssn) && IsEmpty
                (local.MissingSsnAndDodRpt.Dob))
              {
                local.MissingSsnAndDodRpt.MissingAttribute = "B";
              }

              if (IsEmpty(local.MissingSsnAndDodRpt.Ssn) && IsEmpty
                (local.MissingSsnAndDodRpt.Dob))
              {
                local.MissingSsnAndDodRpt.MissingAttribute = "C";
              }

              local.PassArea.FileInstruction = "WRITE";
              UseOeEabSsnAndDobMissRpt();

              if (!Equal(local.PassArea.TextReturnCode, "OK"))
              {
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }

              local.CsePersonsWorkSet.Assign(local.ClearCsePersonsWorkSet);
              local.LegalAction.Identifier = 0;
              local.SkipObligor.Number = entities.CsePerson.Number;
              local.ProcessLastRecord.Flag = "";

              continue;

              // next person
            }
          }

          local.FinanceWorkAttributes.NumericalDollarValue =
            local.OwedAmount.TotalCurrency;
          UseFnCabReturnTextDollars();
          local.Infrastructure.SituationNumber = 0;
          local.Infrastructure.ReasonCode = "KDWPLICDEN";
          local.Infrastructure.EventId = 1;
          local.Infrastructure.EventType = "ADMINACT";
          local.Infrastructure.ProcessStatus = "Q";
          local.Infrastructure.UserId = "KDWP";
          local.Infrastructure.BusinessObjectCd = "ENF";
          local.Infrastructure.ReferenceDate = local.StartDate.Date;
          local.Infrastructure.CreatedBy = local.ProgramProcessingInfo.Name;
          local.Infrastructure.EventDetailName = "KDWP License Sanction";
          local.Infrastructure.CsePersonNumber = entities.CsePerson.Number;
          local.Infrastructure.DenormNumeric12 =
            entities.LegalAction.Identifier;
          local.Infrastructure.DenormText12 =
            entities.LegalAction.CourtCaseNumber;
          local.Detail.Text11 = ", Arrears $";
          local.Infrastructure.Detail = "KDWP license sanction, Ct Order # " + TrimEnd
            (entities.LegalAction.StandardNumber) + local.Detail.Text11 + local
            .FinanceWorkAttributes.TextDollarValue;

          // CDVALUE  	DESCRIPTION
          // ---+------	---+---------+---------+---------+---------+---------+
          // ---------+---
          // ADMIN    	ADMINISTRATIVE/EXEMPTIONS
          // ADMINACT 	ADMINISTRATIVE ACTION
          // ADMINAPL 	ADMINSTRATIVE APPEAL
          // APDTL    	ABSENT PARENT DETAILS
          // APSTMT   	AP STATEMENT
          // ARCHIVE  	ARCHIVE/RETRIEVAL (DOCUMENTS)
          // BKRP     	BANKRUPTCY ACTIVITIES
          // CASE     	CASE (AE ACTIVITIES, OPENINGS/CLOSINGS, APPOINTMENTS)
          // CASEROLE 	CASE ROLE (FAMILY VIOLENCE, ROLE CHANGES, GOOD CAUSE, 
          // SSN, PAT)
          // CASEUNIT 	CASE UNIT (LIFECYCLE, OBLIGATIONS PAID/ACTIVATED/
          // REACTIVATED)
          // COMPLIANCE	OBLIGATIONS/FINANCE ACTIVITIES, PARENTAL RIGHTS/
          // EMANCIPATION
          // CSENET   	CSENET, QUICK LOCATE
          // DATEMON  	DATE MONITOR (FUTURE DATES - EMANCIPATION/DISCHARGE/
          // RELEASE)
          // DOCUMENT 	DOCUMENT (GENTEST, LOCATE, INSURANCE, MODIFICATION, JE)
          // EXTERNAL 	EXTERNAL (MANUALLY CREATED)
          // GENTEST   	GENETIC TEST ACTIVITIES
          // HEALTHINS 	HEALTH INSURANCE
          // LEACTION  	LEGAL ACTIONS, NOTICE OF HEARINGS
          // LEREFRL   	LEGAL REFERRALS
          // LOC       	LOCATE EVENTS (DHR, FPLS, 1099, ADDRESSES)
          // MODFN     	SUPPORT MODIFICATION REVIEW
          // PAT       	PERSON PATERNITY TYPE EVENT
          // PERSDTL   	CSE PERSON DETAIL TYPE EVENT Z
          // SERV      	SERVICE REQUESTED, COMPLETED, ANSWERS
          if (ReadCaseCaseRoleCaseUnit2())
          {
            local.Infrastructure.CaseNumber = entities.Case1.Number;
            local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
          }
          else
          {
            ExitState = "CASE_NF";

            goto Test3;
          }

          UseSpCabCreateInfrastructure();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            goto Test3;
          }

          export.TotalAmtDebtOwed.TotalCurrency += local.OwedAmount.
            TotalCurrency;
          local.PersonTotal.TotalCurrency += local.OwedAmount.TotalCurrency;
          local.PersonSend.Flag = "Y";
          local.LastPersonTotal.TotalCurrency = local.PersonTotal.TotalCurrency;
          local.LastCsePersonsWorkSet.Assign(local.CsePersonsWorkSet);
          MoveLegalAction(local.LegalAction, local.LastLegalAction);

          // need to only check this once, for this run
        }
        else
        {
          continue;
        }
      }

Test3:

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        if (AsChar(local.PersonSend.Flag) == 'Y')
        {
        }
        else
        {
          continue;
        }
      }

      if (!Equal(entities.CsePerson.Number, local.AlreadyWritten.Number) && IsExitState
        ("ACO_NN0000_ALL_OK") && !IsEmpty(local.PreviousProcess.Number))
      {
        // we only want to write out one record per obligor
        local.AlreadyWritten.Number = entities.CsePerson.Number;
        local.LastSuccessfulProcessed.Assign(local.CsePersonsWorkSet);
        MoveLegalAction(local.LegalAction, local.LastSuccesfulProcessed);

        if (local.PreviousPersonTotal.TotalCurrency > 0 && !
          IsEmpty(local.PreviousfindMe.Number))
        {
          local.KsWildlifeParks.CurrentAmount =
            local.PreviousPersonTotal.TotalCurrency;
          local.KsWildlifeParks.OriginalAmount =
            local.PreviousPersonTotal.TotalCurrency;
          local.KsWildlifeParks.CurrentAmountDate = local.StartDate.Date;

          // **********************************************************************************
          // Find the records that was sent last time
          // ********************************************************************************
          if (ReadKsWildlifeParks2())
          {
            local.KsWildlifeParks.OriginalAmount =
              entities.KsWildlifeParks.OriginalAmount;
          }

          if (!ReadObligor2())
          {
            foreach(var item1 in ReadInfrastructure2())
            {
              DeleteInfrastructure();
            }

            ExitState = "FN0000_OBLIGOR_NF";

            goto Test4;
          }

          // **********************************************************************************
          // Create a KDWP record
          // **********************************************************************************
          if (ReadAdministrativeAction())
          {
            try
            {
              CreateKsWildlifeParks();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  foreach(var item1 in ReadInfrastructure2())
                  {
                    DeleteInfrastructure();
                  }

                  ExitState = "LE0000_ADMIN_ACT_CERT_AE";

                  goto Test4;
                case ErrorCode.PermittedValueViolation:
                  foreach(var item1 in ReadInfrastructure2())
                  {
                    DeleteInfrastructure();
                  }

                  ExitState = "ADMIN_ACT_CERTIFICATION_PV";

                  goto Test4;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }
          else
          {
            ExitState = "ADMINISTRATIVE_ACTION_NF";

            goto Test4;
          }

          local.PreviousfindMe.Number = "";
          local.PreviousPersonTotal.TotalCurrency = 0;
        }

        local.DateWorkArea.Year = Year(local.PreviousProcess.Dob);
        local.WorkArea.Text15 = NumberToString(local.DateWorkArea.Year, 15);
        local.Year.Text4 = Substring(local.WorkArea.Text15, 12, 4);
        local.DateWorkArea.Month = Month(local.PreviousProcess.Dob);
        local.WorkArea.Text15 = NumberToString(local.DateWorkArea.Month, 15);
        local.Month.Text2 = Substring(local.WorkArea.Text15, 14, 2);
        local.DateWorkArea.Day = Day(local.PreviousProcess.Dob);
        local.WorkArea.Text15 = NumberToString(local.DateWorkArea.Day, 15);
        local.Day.Text2 = Substring(local.WorkArea.Text15, 14, 2);
        local.DateWorkArea.TextDate = local.Year.Text4 + local.Month.Text2 + local
          .Day.Text2;
        ++export.TotalNumRecsAdded.Count;
        local.PassArea.FileInstruction = "WRITE";
        local.KdwpOutput.CsePersonNumber = local.PreviousProcess.Number;
        local.KdwpOutput.Ssn = local.PreviousProcess.Ssn;
        local.KdwpOutput.AliasInd = "";
        local.KdwpOutput.Dob = local.DateWorkArea.TextDate;
        local.KdwpOutput.LastName = local.PreviousProcess.LastName;
        local.KdwpOutput.FirstName = local.PreviousProcess.FirstName;
        local.KdwpOutput.MiddleName = local.PreviousProcess.MiddleInitial;
        UseOeEabRequestToKdwp();

        if (!Equal(local.PassArea.TextReturnCode, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        local.StartCsePersonsWorkSet.Number = local.PreviousProcess.Number;
        UseEabReadAliasBatch();

        if (!Equal(local.PassArea.TextReturnCode, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        if (!local.Alias.IsEmpty)
        {
          for(local.Alias.Index = 0; local.Alias.Index < local.Alias.Count; ++
            local.Alias.Index)
          {
            if (Equal(local.PreviousProcess.Ssn, local.Alias.Item.Grps.Ssn))
            {
              continue;
            }

            if (Equal(local.Alias.Item.Grps.Ssn, "000000000"))
            {
              continue;
            }

            local.KdwpOutput.Ssn = local.Alias.Item.Grps.Ssn;
            local.KdwpOutput.AliasInd = "Y";
            local.PassArea.FileInstruction = "WRITE";
            UseOeEabRequestToKdwp();

            if (!Equal(local.PassArea.TextReturnCode, "OK"))
            {
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            ++export.TotalNumRecsAdded.Count;
          }
        }

        local.Alias.Index = 0;
        local.Alias.Clear();

        while(Equal(entities.CsePerson.Number, "XXXXXXXXXX"))
        {
          if (local.Alias.IsFull)
          {
            break;
          }

          local.Alias.Next();
        }

        ++local.RecordCount.Count;

        if (local.RecordCount.Count >= local
          .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
        {
          local.ProgramCheckpointRestart.RestartInd = "Y";
          local.ProgramCheckpointRestart.ProgramName =
            local.ProgramProcessingInfo.Name;
          local.RestartLegalId.Text15 =
            NumberToString(entities.LegalAction.Identifier, 15);
          local.RestartAmount.Text15 =
            NumberToString((long)(local.PersonTotal.TotalCurrency * 100), 15);
          local.ProgramCheckpointRestart.RestartInfo =
            local.CsePersonsWorkSet.Number + local.RestartLegalId.Text15 + local
            .RestartAmount.Text15;
          local.TotalAmount.Text15 =
            NumberToString((long)(export.TotalAmtDebtOwed.TotalCurrency * 100),
            15);
          local.TotalNumberRecordsRead.Text15 =
            NumberToString(export.NumberOfRecordsRead.Count, 15);
          local.NumOfRecordsProcessed.Text15 =
            NumberToString(export.TotalNumRecsAdded.Count, 15);
          local.TotalNumberOfErrors.Text15 =
            NumberToString(export.TotalNumErrorRecords.Count, 15);
          local.ProgramCheckpointRestart.RestartInfo =
            TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + local
            .TotalAmount.Text15 + local.TotalNumberRecordsRead.Text15 + local
            .NumOfRecordsProcessed.Text15 + local.TotalNumberOfErrors.Text15;
          UseUpdatePgmCheckpointRestart();
          UseExtToDoACommit();

          if (local.PassArea.NumericReturnCode != 0)
          {
            ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

            return;
          }

          local.RecordCount.Count = 0;
        }

        local.PersonSend.Flag = "";
        local.KdwpOutput.Assign(local.ClearKdwpOutput);
      }
      else if (!Equal(entities.CsePerson.Number, local.AlreadyWritten.Number) &&
        IsExitState("ACO_NN0000_ALL_OK"))
      {
        local.LastSuccessfulProcessed.Assign(local.CsePersonsWorkSet);
        MoveLegalAction(local.LegalAction, local.LastSuccesfulProcessed);
      }

Test4:

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        // NOW WE WILL write out any errors and then reset the exitstate to all 
        // is ok
        UseEabExtractExitStateMessage();
        local.EabFileHandling.Action = "WRITE";
        local.Error.StandardNumber = "";

        if (!Equal(entities.LegalAction.StandardNumber,
          local.LegalAction.StandardNumber))
        {
          local.Error.StandardNumber = entities.LegalAction.StandardNumber;
          local.LastCsePersonsWorkSet.Assign(local.LastSuccessfulProcessed);
          local.CsePersonsWorkSet.Assign(local.LastSuccessfulProcessed);
          MoveLegalAction(local.LastSuccesfulProcessed, local.LegalAction);
          MoveLegalAction(local.LastSuccesfulProcessed, local.LastLegalAction);
        }
        else if (!Equal(local.LegalAction.StandardNumber,
          local.LastLegalAction.StandardNumber))
        {
          local.Error.StandardNumber = entities.LegalAction.StandardNumber;
          local.LastCsePersonsWorkSet.Assign(local.PreviousProcess);
          local.CsePersonsWorkSet.Assign(local.PreviousProcess);
          MoveLegalAction(local.Previous, local.LegalAction);
          MoveLegalAction(local.Previous, local.LastLegalAction);
        }
        else if (Equal(local.LegalAction.StandardNumber,
          local.LastLegalAction.StandardNumber))
        {
          local.Error.StandardNumber = local.Previous.StandardNumber ?? "";
          MoveLegalAction(local.ClearLegalAction, local.Previous);
          local.PreviousProcess.Assign(local.ClearCsePersonsWorkSet);
          local.LastSuccessfulProcessed.Assign(local.CsePersonsWorkSet);
          MoveLegalAction(local.LegalAction, local.LastSuccesfulProcessed);
        }

        local.EabReportSend.RptDetail = "Error found in program:  " + TrimEnd
          (local.ExitStateWorkArea.Message) + "  Court Order # :  " + (
            local.Error.StandardNumber ?? "");
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        ExitState = "ACO_NN0000_ALL_OK";
        ++export.TotalNumErrorRecords.Count;
      }

ReadEach2:
      ;
    }

    if (local.OwedAmount.TotalCurrency > local.MiniumTarget.TotalCurrency && IsExitState
      ("ACO_NN0000_ALL_OK"))
    {
      // This section is only to pick up the last record if it was valid and 
      // should be written out.
      local.KsWildlifeParks.CurrentAmount = local.LastPersonTotal.TotalCurrency;
      local.KsWildlifeParks.OriginalAmount =
        local.LastPersonTotal.TotalCurrency;
      local.KsWildlifeParks.CurrentAmountDate = local.StartDate.Date;

      // **********************************************************************************
      // Find the records that was sent last time
      // ********************************************************************************
      if (ReadKsWildlifeParks1())
      {
        local.KsWildlifeParks.OriginalAmount =
          entities.KsWildlifeParks.OriginalAmount;
      }

      if (!ReadObligor1())
      {
        foreach(var item in ReadInfrastructure1())
        {
          DeleteInfrastructure();
        }

        ExitState = "FN0000_OBLIGOR_NF";

        goto Test5;
      }

      // **********************************************************************************
      // Create a KDWP record
      // **********************************************************************************
      if (ReadAdministrativeAction())
      {
        try
        {
          CreateKsWildlifeParks();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              foreach(var item in ReadInfrastructure1())
              {
                DeleteInfrastructure();
              }

              ExitState = "LE0000_ADMIN_ACT_CERT_AE";

              goto Test5;
            case ErrorCode.PermittedValueViolation:
              foreach(var item in ReadInfrastructure1())
              {
                DeleteInfrastructure();
              }

              ExitState = "ADMIN_ACT_CERTIFICATION_PV";

              goto Test5;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
      else
      {
        ExitState = "ADMINISTRATIVE_ACTION_NF";

        goto Test5;
      }

      if (ReadCaseCaseRoleCaseUnit1())
      {
        local.Infrastructure.CaseNumber = entities.Case1.Number;
        local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
      }
      else
      {
        foreach(var item in ReadKsWildlifeParks3())
        {
          DeleteKsWildlifeParks();
        }

        foreach(var item in ReadInfrastructure2())
        {
          DeleteInfrastructure();
        }

        ExitState = "CASE_NF";

        goto Test5;
      }

      local.DateWorkArea.Year = Year(local.LastCsePersonsWorkSet.Dob);
      local.WorkArea.Text15 = NumberToString(local.DateWorkArea.Year, 15);
      local.Year.Text4 = Substring(local.WorkArea.Text15, 12, 4);
      local.DateWorkArea.Month = Month(local.LastCsePersonsWorkSet.Dob);
      local.WorkArea.Text15 = NumberToString(local.DateWorkArea.Month, 15);
      local.Month.Text2 = Substring(local.WorkArea.Text15, 14, 2);
      local.DateWorkArea.Day = Day(local.LastCsePersonsWorkSet.Dob);
      local.WorkArea.Text15 = NumberToString(local.DateWorkArea.Day, 15);
      local.Day.Text2 = Substring(local.WorkArea.Text15, 14, 2);
      local.DateWorkArea.TextDate = local.Year.Text4 + local.Month.Text2 + local
        .Day.Text2;
      ++export.TotalNumRecsAdded.Count;
      local.PassArea.FileInstruction = "WRITE";
      local.KdwpOutput.CsePersonNumber = local.LastCsePersonsWorkSet.Number;
      local.KdwpOutput.Ssn = local.LastCsePersonsWorkSet.Ssn;
      local.KdwpOutput.AliasInd = "";
      local.KdwpOutput.Dob = local.DateWorkArea.TextDate;
      local.KdwpOutput.LastName = local.LastCsePersonsWorkSet.LastName;
      local.KdwpOutput.FirstName = local.LastCsePersonsWorkSet.FirstName;
      local.KdwpOutput.MiddleName = local.LastCsePersonsWorkSet.MiddleInitial;
      UseOeEabRequestToKdwp();

      if (!Equal(local.PassArea.TextReturnCode, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      local.StartCsePersonsWorkSet.Number = local.LastCsePersonsWorkSet.Number;
      UseEabReadAliasBatch();

      if (!Equal(local.PassArea.TextReturnCode, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      if (!local.Alias.IsEmpty)
      {
        for(local.Alias.Index = 0; local.Alias.Index < local.Alias.Count; ++
          local.Alias.Index)
        {
          if (Equal(local.LastCsePersonsWorkSet.Ssn, local.Alias.Item.Grps.Ssn))
          {
            continue;
          }

          if (Equal(local.Alias.Item.Grps.Ssn, "000000000"))
          {
            continue;
          }

          local.KdwpOutput.Ssn = local.Alias.Item.Grps.Ssn;
          local.KdwpOutput.AliasInd = "Y";
          local.PassArea.FileInstruction = "WRITE";
          UseOeEabRequestToKdwp();

          if (!Equal(local.PassArea.TextReturnCode, "OK"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          ++export.TotalNumRecsAdded.Count;
        }
      }
    }

Test5:

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      // NOW WE WILL write out any errors and then reset the exitstate to all is
      // ok
      UseEabExtractExitStateMessage();
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error found in program:  " + TrimEnd
        (local.ExitStateWorkArea.Message) + "  Court Order # :  " + (
          local.LastLegalAction.StandardNumber ?? "");
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      ExitState = "ACO_NN0000_ALL_OK";
      ++export.TotalNumErrorRecords.Count;
    }
  }

  private static void MoveAlias(Local.AliasGroup source,
    EabReadAliasBatch.Export.AliasesGroup target)
  {
    System.Diagnostics.Trace.TraceWarning(
      "INFO: source and target of the move fit weakly.");
    target.G.Assign(source.Grps);
  }

  private static void MoveAliases(EabReadAliasBatch.Export.AliasesGroup source,
    Local.AliasGroup target)
  {
    System.Diagnostics.Trace.TraceWarning(
      "INFO: source and target of the move do not fit perfectly but only head.");
      
    target.Grps.Assign(source.G);
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
    target.LastName = source.LastName;
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

  private static void MoveIwoPeriod(Local.IwoPeriodGroup source,
    OeDeterminePaymentsKdwp.Import.IwoPeriodGroup target)
  {
    target.EndDate.Date = source.EndDate.Date;
    target.BeginDate.Date = source.BeginDate.Date;
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.ProgramName = source.ProgramName;
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
    target.CheckpointCount = source.CheckpointCount;
    target.LastCheckpointTimestamp = source.LastCheckpointTimestamp;
    target.RestartInd = source.RestartInd;
    target.RestartInfo = source.RestartInfo;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

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

  private void UseEabReadAliasBatch()
  {
    var useImport = new EabReadAliasBatch.Import();
    var useExport = new EabReadAliasBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.StartCsePersonsWorkSet.Number;
    local.Alias.CopyTo(useExport.Aliases, MoveAlias);
    useExport.AbendData.Assign(local.AbendData);

    Call(EabReadAliasBatch.Execute, useImport, useExport);

    useExport.Aliases.CopyTo(local.Alias, MoveAliases);
    local.AbendData.Assign(useExport.AbendData);
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.PassArea.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.PassArea.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseFnCabReturnTextDollars()
  {
    var useImport = new FnCabReturnTextDollars.Import();
    var useExport = new FnCabReturnTextDollars.Export();

    useImport.FinanceWorkAttributes.NumericalDollarValue =
      local.FinanceWorkAttributes.NumericalDollarValue;

    Call(FnCabReturnTextDollars.Execute, useImport, useExport);

    local.FinanceWorkAttributes.TextDollarValue =
      useExport.FinanceWorkAttributes.TextDollarValue;
  }

  private void UseFnComputeTotalsForCtOrder2()
  {
    var useImport = new FnComputeTotalsForCtOrder2.Import();
    var useExport = new FnComputeTotalsForCtOrder2.Export();

    useImport.Obligor.Number = entities.CsePerson.Number;
    useImport.FilterByStdNo.StandardNumber =
      entities.LegalAction.StandardNumber;
    useImport.MiniumAmount.TotalCurrency = local.MiniumTarget.TotalCurrency;
    useImport.StartDate.Date = local.StartDate.Date;
    useImport.NumDaysAccruingOrDue.Count = local.NumDaysAccruingOrDue.Count;
    useImport.NumDaysSinceObCreated.Count = local.NumDaysSinceObCreated.Count;

    Call(FnComputeTotalsForCtOrder2.Execute, useImport, useExport);

    local.ScreenOwedAmountsDtl.Assign(useExport.ScreenOwedAmountsDtl);
  }

  private void UseOeDeterminePaymentsKdwp()
  {
    var useImport = new OeDeterminePaymentsKdwp.Import();
    var useExport = new OeDeterminePaymentsKdwp.Export();

    useImport.IwoAmount.TotalCurrency = local.IwoPayment.TotalCurrency;
    useImport.LegalAction.StandardNumber = entities.LegalAction.StandardNumber;
    useImport.CsePerson.Number = entities.CsePerson.Number;
    local.IwoPeriod.CopyTo(useImport.IwoPeriod, MoveIwoPeriod);

    Call(OeDeterminePaymentsKdwp.Execute, useImport, useExport);

    local.MeetsCriteria.Flag = useExport.MeetsCriteria.Flag;
  }

  private void UseOeEabRequestToKdwp()
  {
    var useImport = new OeEabRequestToKdwp.Import();
    var useExport = new OeEabRequestToKdwp.Export();

    useImport.KdwpOutput.Assign(local.KdwpOutput);
    useImport.External.FileInstruction = local.PassArea.FileInstruction;
    useExport.External.Assign(local.PassArea);

    Call(OeEabRequestToKdwp.Execute, useImport, useExport);

    local.PassArea.Assign(useExport.External);
  }

  private void UseOeEabSsnAndDobMissRpt()
  {
    var useImport = new OeEabSsnAndDobMissRpt.Import();
    var useExport = new OeEabSsnAndDobMissRpt.Export();

    useImport.External.FileInstruction = local.PassArea.FileInstruction;
    useImport.MissingSsnAndDodRpt.Assign(local.MissingSsnAndDodRpt);
    useExport.External.Assign(local.PassArea);

    Call(OeEabSsnAndDobMissRpt.Execute, useImport, useExport);

    local.PassArea.Assign(useExport.External);
  }

  private void UseSiReadCsePersonBatch()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.StartCsePersonsWorkSet.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    MoveInfrastructure(useExport.Infrastructure, local.Infrastructure);
  }

  private void UseUpdatePgmCheckpointRestart()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }

  private void CreateKsWildlifeParks()
  {
    System.Diagnostics.Debug.Assert(entities.Previous.Populated);

    var cpaType = entities.Previous.Type1;
    var cspNumber = entities.Previous.CspNumber;
    var type1 = local.AdministrativeAction.Type1;
    var takenDate = local.StartDate.Date;
    var aatType = entities.AdministrativeAction.Type1;
    var originalAmount =
      local.KsWildlifeParks.OriginalAmount.GetValueOrDefault();
    var currentAmount = local.KsWildlifeParks.CurrentAmount.GetValueOrDefault();
    var currentAmountDate = local.KsWildlifeParks.CurrentAmountDate;
    var createdBy = global.UserId;
    var createdTstamp = Now();
    var param = 0M;

    CheckValid<AdministrativeActCertification>("CpaType", cpaType);
    CheckValid<AdministrativeActCertification>("Type1", type1);
    entities.KsWildlifeParks.Populated = false;
    Update("CreateKsWildlifeParks",
      (db, command) =>
      {
        db.SetString(command, "cpaType", cpaType);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "type", type1);
        db.SetDate(command, "takenDt", takenDate);
        db.SetNullableString(command, "aatType", aatType);
        db.SetNullableDecimal(command, "originalAmt", originalAmount);
        db.SetNullableDecimal(command, "currentAmt", currentAmount);
        db.SetNullableDate(command, "currentAmtDt", currentAmountDate);
        db.SetNullableDate(command, "decertifiedDt", null);
        db.SetNullableDate(command, "notificationDt", null);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetNullableString(command, "lastUpdatedBy", createdBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", createdTstamp);
        db.SetNullableDate(command, "referralDt", default(DateTime));
        db.SetNullableDecimal(command, "recoveryAmt", param);
        db.SetString(command, "witness", "");
        db.SetNullableString(command, "notifiedBy", "");
        db.SetNullableString(command, "reasonWithdraw", "");
        db.SetNullableString(command, "denialReason", "");
        db.SetNullableDate(command, "dateSent", takenDate);
        db.SetNullableString(command, "etypeAdminOffset", "");
        db.SetNullableString(command, "localCode", "");
        db.SetInt32(command, "ssn", 0);
        db.SetString(command, "caseNumber", "");
        db.SetString(command, "lastName", "");
        db.SetInt32(command, "amountOwed", 0);
        db.SetNullableString(command, "transferState", "");
        db.SetNullableInt32(command, "localForTransfer", 0);
        db.SetNullableInt32(command, "processYear", 0);
        db.SetString(command, "tanfCode", "");
        db.SetNullableString(command, "decertifyReason", "");
        db.SetNullableDecimal(command, "previousAmount", param);
        db.SetNullableDecimal(command, "minimumAmount", param);
        db.SetNullableString(command, "addressStreet1", "");
        db.SetNullableString(command, "addressCity", "");
        db.SetNullableString(command, "addressZip", "");
      });

    entities.KsWildlifeParks.CpaType = cpaType;
    entities.KsWildlifeParks.CspNumber = cspNumber;
    entities.KsWildlifeParks.Type1 = type1;
    entities.KsWildlifeParks.TakenDate = takenDate;
    entities.KsWildlifeParks.AatType = aatType;
    entities.KsWildlifeParks.OriginalAmount = originalAmount;
    entities.KsWildlifeParks.CurrentAmount = currentAmount;
    entities.KsWildlifeParks.CurrentAmountDate = currentAmountDate;
    entities.KsWildlifeParks.DecertifiedDate = null;
    entities.KsWildlifeParks.NotificationDate = null;
    entities.KsWildlifeParks.CreatedBy = createdBy;
    entities.KsWildlifeParks.CreatedTstamp = createdTstamp;
    entities.KsWildlifeParks.LastUpdatedBy = createdBy;
    entities.KsWildlifeParks.LastUpdatedTstamp = createdTstamp;
    entities.KsWildlifeParks.NotifiedBy = "";
    entities.KsWildlifeParks.DateSent = takenDate;
    entities.KsWildlifeParks.TanfCode = "";
    entities.KsWildlifeParks.PreviousAmount = param;
    entities.KsWildlifeParks.MinimumAmount = param;
    entities.KsWildlifeParks.Populated = true;
  }

  private void DeleteInfrastructure()
  {
    Update("DeleteInfrastructure#1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "infId", entities.Infrastructure.SystemGeneratedIdentifier);
      });

    Update("DeleteInfrastructure#2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "infId", entities.Infrastructure.SystemGeneratedIdentifier);
      });

    Update("DeleteInfrastructure#3",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "infId", entities.Infrastructure.SystemGeneratedIdentifier);
      });

    Update("DeleteInfrastructure#4",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "infId", entities.Infrastructure.SystemGeneratedIdentifier);
      });

    Update("DeleteInfrastructure#5",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "infId", entities.Infrastructure.SystemGeneratedIdentifier);
      });

    Update("DeleteInfrastructure#6",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "infId", entities.Infrastructure.SystemGeneratedIdentifier);
      });

    Update("DeleteInfrastructure#7",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "infId", entities.Infrastructure.SystemGeneratedIdentifier);
      });

    Update("DeleteInfrastructure#8",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "infId", entities.Infrastructure.SystemGeneratedIdentifier);
      });

    Update("DeleteInfrastructure#9",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "infId", entities.Infrastructure.SystemGeneratedIdentifier);
      });
  }

  private void DeleteKsWildlifeParks()
  {
    bool exists;

    exists = Read("DeleteKsWildlifeParks#1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cpaRType", entities.KsWildlifeParks.CpaType);
        db.SetNullableString(
          command, "cspRNumber", entities.KsWildlifeParks.CspNumber);
        db.
          SetNullableString(command, "aacRType", entities.KsWildlifeParks.Type1);
          
        db.SetNullableDate(
          command, "aacRTakenDate",
          entities.KsWildlifeParks.TakenDate.GetValueOrDefault());
        db.SetNullableString(
          command, "aacTanfCode", entities.KsWildlifeParks.TanfCode);
      },
      null);

    if (exists)
    {
      throw DataError("Restrict violation on table \"CKT_ADMIN_APPEAL\".",
        "50001");
    }

    Update("DeleteKsWildlifeParks#2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cpaRType", entities.KsWildlifeParks.CpaType);
        db.SetNullableString(
          command, "cspRNumber", entities.KsWildlifeParks.CspNumber);
        db.
          SetNullableString(command, "aacRType", entities.KsWildlifeParks.Type1);
          
        db.SetNullableDate(
          command, "aacRTakenDate",
          entities.KsWildlifeParks.TakenDate.GetValueOrDefault());
        db.SetNullableString(
          command, "aacTanfCode", entities.KsWildlifeParks.TanfCode);
      });
  }

  private bool ReadAdministrativeAction()
  {
    entities.AdministrativeAction.Populated = false;

    return Read("ReadAdministrativeAction",
      (db, command) =>
      {
        db.SetString(command, "type", local.AdministrativeAction.Type1);
      },
      (db, reader) =>
      {
        entities.AdministrativeAction.Type1 = db.GetString(reader, 0);
        entities.AdministrativeAction.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCase()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", entities.LegalAction.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private bool ReadCaseCaseRoleCaseUnit1()
  {
    entities.CaseRole.Populated = false;
    entities.Case1.Populated = false;
    entities.CaseUnit.Populated = false;

    return Read("ReadCaseCaseRoleCaseUnit1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", local.LastCsePersonsWorkSet.Number);
        db.SetInt32(command, "lgaId", local.LastLegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 6);
        entities.CaseUnit.CasNo = db.GetString(reader, 7);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 8);
        entities.CaseRole.Populated = true;
        entities.Case1.Populated = true;
        entities.CaseUnit.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private bool ReadCaseCaseRoleCaseUnit2()
  {
    entities.CaseRole.Populated = false;
    entities.Case1.Populated = false;
    entities.CaseUnit.Populated = false;

    return Read("ReadCaseCaseRoleCaseUnit2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetInt32(command, "lgaId", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 6);
        entities.CaseUnit.CasNo = db.GetString(reader, 7);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 8);
        entities.CaseRole.Populated = true;
        entities.Case1.Populated = true;
        entities.CaseUnit.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private IEnumerable<bool> ReadCashReceiptDetail()
  {
    entities.CashReceiptDetail.Populated = false;

    return ReadEach("ReadCashReceiptDetail",
      (db, command) =>
      {
        db.SetDate(command, "date1", local.From.Date.GetValueOrDefault());
        db.SetDate(command, "date2", local.To.Date.GetValueOrDefault());
        db.
          SetNullableString(command, "oblgorPrsnNbr", entities.CsePerson.Number);
          
        db.SetNullableString(
          command, "courtOrderNumber", entities.LegalAction.StandardNumber ?? ""
          );
        db.SetNullableDate(
          command, "discontinueDate", local.EndDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 5);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 6);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 7);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 8);
        entities.CashReceiptDetail.Populated = true;

        return true;
      });
  }

  private bool ReadCashReceiptType()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CashReceiptType.Populated = false;

    return Read("ReadCashReceiptType",
      (db, command) =>
      {
        db.SetInt32(
          command, "crtypeId", entities.CashReceiptDetail.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptType.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCollectionType()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CollectionType.Populated = false;

    return ReadEach("ReadCollectionType",
      (db, command) =>
      {
        db.SetInt32(
          command, "collectionTypeId",
          entities.CashReceiptDetail.CltIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 0);
        entities.CollectionType.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePersonLegalAction()
  {
    entities.CsePerson.Populated = false;
    entities.LegalAction.Populated = false;

    return ReadEach("ReadCsePersonLegalAction",
      (db, command) =>
      {
        db.SetString(command, "numb", local.CsePerson.Number);
        db.SetNullableDate(
          command, "dateOfDeath", local.ZeroDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "startDate", local.StartDate.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "standardNo", local.Read.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.DateOfDeath = db.GetNullableDate(reader, 2);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 3);
        entities.LegalAction.Identifier = db.GetInt32(reader, 4);
        entities.LegalAction.Classification = db.GetString(reader, 5);
        entities.LegalAction.ActionTaken = db.GetString(reader, 6);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 7);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 8);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 9);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 10);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 11);
        entities.CsePerson.Populated = true;
        entities.LegalAction.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadDebtDetail()
  {
    entities.DebtDetail.Populated = false;

    return ReadEach("ReadDebtDetail",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "retiredDt", local.ZeroDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 7);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 8);
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);

        return true;
      });
  }

  private bool ReadGoodCause()
  {
    entities.GoodCause.Populated = false;

    return Read("ReadGoodCause",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "effectiveDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.GoodCause.Code = db.GetNullableString(reader, 0);
        entities.GoodCause.EffectiveDate = db.GetNullableDate(reader, 1);
        entities.GoodCause.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.GoodCause.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.GoodCause.CasNumber = db.GetString(reader, 4);
        entities.GoodCause.CspNumber = db.GetString(reader, 5);
        entities.GoodCause.CroType = db.GetString(reader, 6);
        entities.GoodCause.CroIdentifier = db.GetInt32(reader, 7);
        entities.GoodCause.CasNumber1 = db.GetNullableString(reader, 8);
        entities.GoodCause.CspNumber1 = db.GetNullableString(reader, 9);
        entities.GoodCause.CroType1 = db.GetNullableString(reader, 10);
        entities.GoodCause.CroIdentifier1 = db.GetNullableInt32(reader, 11);
        entities.GoodCause.Populated = true;
        CheckValid<GoodCause>("CroType", entities.GoodCause.CroType);
        CheckValid<GoodCause>("CroType1", entities.GoodCause.CroType1);
      });
  }

  private IEnumerable<bool> ReadInfrastructure1()
  {
    entities.Infrastructure.Populated = false;

    return ReadEach("ReadInfrastructure1",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          local.StartBatchTimestampWorkArea.IefTimestamp.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum", local.LastCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.EventId = db.GetInt32(reader, 1);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 2);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 3);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.Infrastructure.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadInfrastructure2()
  {
    entities.Infrastructure.Populated = false;

    return ReadEach("ReadInfrastructure2",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          local.StartBatchTimestampWorkArea.IefTimestamp.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum", local.PreviousProcess.Number);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.EventId = db.GetInt32(reader, 1);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 2);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 3);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.Infrastructure.Populated = true;

        return true;
      });
  }

  private bool ReadKsWildlifeParks1()
  {
    entities.KsWildlifeParks.Populated = false;

    return Read("ReadKsWildlifeParks1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", local.LastCsePersonsWorkSet.Number);
        db.SetDate(command, "date", local.StartDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.KsWildlifeParks.CpaType = db.GetString(reader, 0);
        entities.KsWildlifeParks.CspNumber = db.GetString(reader, 1);
        entities.KsWildlifeParks.Type1 = db.GetString(reader, 2);
        entities.KsWildlifeParks.TakenDate = db.GetDate(reader, 3);
        entities.KsWildlifeParks.AatType = db.GetNullableString(reader, 4);
        entities.KsWildlifeParks.OriginalAmount =
          db.GetNullableDecimal(reader, 5);
        entities.KsWildlifeParks.CurrentAmount =
          db.GetNullableDecimal(reader, 6);
        entities.KsWildlifeParks.CurrentAmountDate =
          db.GetNullableDate(reader, 7);
        entities.KsWildlifeParks.DecertifiedDate =
          db.GetNullableDate(reader, 8);
        entities.KsWildlifeParks.NotificationDate =
          db.GetNullableDate(reader, 9);
        entities.KsWildlifeParks.CreatedBy = db.GetString(reader, 10);
        entities.KsWildlifeParks.CreatedTstamp = db.GetDateTime(reader, 11);
        entities.KsWildlifeParks.LastUpdatedBy =
          db.GetNullableString(reader, 12);
        entities.KsWildlifeParks.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 13);
        entities.KsWildlifeParks.NotifiedBy = db.GetNullableString(reader, 14);
        entities.KsWildlifeParks.DateSent = db.GetNullableDate(reader, 15);
        entities.KsWildlifeParks.TanfCode = db.GetString(reader, 16);
        entities.KsWildlifeParks.PreviousAmount =
          db.GetNullableDecimal(reader, 17);
        entities.KsWildlifeParks.MinimumAmount =
          db.GetNullableDecimal(reader, 18);
        entities.KsWildlifeParks.Populated = true;
        CheckValid<AdministrativeActCertification>("CpaType",
          entities.KsWildlifeParks.CpaType);
        CheckValid<AdministrativeActCertification>("Type1",
          entities.KsWildlifeParks.Type1);
      });
  }

  private bool ReadKsWildlifeParks2()
  {
    entities.KsWildlifeParks.Populated = false;

    return Read("ReadKsWildlifeParks2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", local.PreviousProcess.Number);
        db.SetDate(command, "date", local.StartDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.KsWildlifeParks.CpaType = db.GetString(reader, 0);
        entities.KsWildlifeParks.CspNumber = db.GetString(reader, 1);
        entities.KsWildlifeParks.Type1 = db.GetString(reader, 2);
        entities.KsWildlifeParks.TakenDate = db.GetDate(reader, 3);
        entities.KsWildlifeParks.AatType = db.GetNullableString(reader, 4);
        entities.KsWildlifeParks.OriginalAmount =
          db.GetNullableDecimal(reader, 5);
        entities.KsWildlifeParks.CurrentAmount =
          db.GetNullableDecimal(reader, 6);
        entities.KsWildlifeParks.CurrentAmountDate =
          db.GetNullableDate(reader, 7);
        entities.KsWildlifeParks.DecertifiedDate =
          db.GetNullableDate(reader, 8);
        entities.KsWildlifeParks.NotificationDate =
          db.GetNullableDate(reader, 9);
        entities.KsWildlifeParks.CreatedBy = db.GetString(reader, 10);
        entities.KsWildlifeParks.CreatedTstamp = db.GetDateTime(reader, 11);
        entities.KsWildlifeParks.LastUpdatedBy =
          db.GetNullableString(reader, 12);
        entities.KsWildlifeParks.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 13);
        entities.KsWildlifeParks.NotifiedBy = db.GetNullableString(reader, 14);
        entities.KsWildlifeParks.DateSent = db.GetNullableDate(reader, 15);
        entities.KsWildlifeParks.TanfCode = db.GetString(reader, 16);
        entities.KsWildlifeParks.PreviousAmount =
          db.GetNullableDecimal(reader, 17);
        entities.KsWildlifeParks.MinimumAmount =
          db.GetNullableDecimal(reader, 18);
        entities.KsWildlifeParks.Populated = true;
        CheckValid<AdministrativeActCertification>("CpaType",
          entities.KsWildlifeParks.CpaType);
        CheckValid<AdministrativeActCertification>("Type1",
          entities.KsWildlifeParks.Type1);
      });
  }

  private IEnumerable<bool> ReadKsWildlifeParks3()
  {
    System.Diagnostics.Debug.Assert(entities.Previous.Populated);
    entities.KsWildlifeParks.Populated = false;

    return ReadEach("ReadKsWildlifeParks3",
      (db, command) =>
      {
        db.
          SetDate(command, "takenDt", local.StartDate.Date.GetValueOrDefault());
          
        db.SetNullableString(
          command, "aatType", entities.AdministrativeAction.Type1);
        db.SetDateTime(
          command, "createdTstamp",
          local.StartBatchTimestampWorkArea.IefTimestamp.GetValueOrDefault());
        db.SetString(command, "cpaType", entities.Previous.Type1);
        db.SetString(command, "cspNumber", entities.Previous.CspNumber);
      },
      (db, reader) =>
      {
        entities.KsWildlifeParks.CpaType = db.GetString(reader, 0);
        entities.KsWildlifeParks.CspNumber = db.GetString(reader, 1);
        entities.KsWildlifeParks.Type1 = db.GetString(reader, 2);
        entities.KsWildlifeParks.TakenDate = db.GetDate(reader, 3);
        entities.KsWildlifeParks.AatType = db.GetNullableString(reader, 4);
        entities.KsWildlifeParks.OriginalAmount =
          db.GetNullableDecimal(reader, 5);
        entities.KsWildlifeParks.CurrentAmount =
          db.GetNullableDecimal(reader, 6);
        entities.KsWildlifeParks.CurrentAmountDate =
          db.GetNullableDate(reader, 7);
        entities.KsWildlifeParks.DecertifiedDate =
          db.GetNullableDate(reader, 8);
        entities.KsWildlifeParks.NotificationDate =
          db.GetNullableDate(reader, 9);
        entities.KsWildlifeParks.CreatedBy = db.GetString(reader, 10);
        entities.KsWildlifeParks.CreatedTstamp = db.GetDateTime(reader, 11);
        entities.KsWildlifeParks.LastUpdatedBy =
          db.GetNullableString(reader, 12);
        entities.KsWildlifeParks.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 13);
        entities.KsWildlifeParks.NotifiedBy = db.GetNullableString(reader, 14);
        entities.KsWildlifeParks.DateSent = db.GetNullableDate(reader, 15);
        entities.KsWildlifeParks.TanfCode = db.GetString(reader, 16);
        entities.KsWildlifeParks.PreviousAmount =
          db.GetNullableDecimal(reader, 17);
        entities.KsWildlifeParks.MinimumAmount =
          db.GetNullableDecimal(reader, 18);
        entities.KsWildlifeParks.Populated = true;
        CheckValid<AdministrativeActCertification>("CpaType",
          entities.KsWildlifeParks.CpaType);
        CheckValid<AdministrativeActCertification>("Type1",
          entities.KsWildlifeParks.Type1);

        return true;
      });
  }

  private bool ReadLegalAction()
  {
    entities.Iwo.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", entities.LegalAction.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Iwo.Identifier = db.GetInt32(reader, 0);
        entities.Iwo.Classification = db.GetString(reader, 1);
        entities.Iwo.ActionTaken = db.GetString(reader, 2);
        entities.Iwo.Type1 = db.GetString(reader, 3);
        entities.Iwo.EndDate = db.GetNullableDate(reader, 4);
        entities.Iwo.StandardNumber = db.GetNullableString(reader, 5);
        entities.Iwo.CreatedTstamp = db.GetDateTime(reader, 6);
        entities.Iwo.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalActionDetail()
  {
    entities.LegalActionDetail.Populated = false;

    return ReadEach("ReadLegalActionDetail",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.Iwo.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 2);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionDetail.ArrearsAmount =
          db.GetNullableDecimal(reader, 4);
        entities.LegalActionDetail.CurrentAmount =
          db.GetNullableDecimal(reader, 5);
        entities.LegalActionDetail.JudgementAmount =
          db.GetNullableDecimal(reader, 6);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 7);
        entities.LegalActionDetail.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);

        return true;
      });
  }

  private IEnumerable<bool> ReadObligationAdmActionExemption()
  {
    entities.ObligationAdmActionExemption.Populated = false;

    return ReadEach("ReadObligationAdmActionExemption",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableString(
          command, "standardNo", local.ReadExemp.StandardNumber ?? "");
        db.SetString(command, "type", local.AdministrativeAction.Type1);
        db.SetDate(
          command, "effectiveDt", local.StartDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligationAdmActionExemption.OtyType = db.GetInt32(reader, 0);
        entities.ObligationAdmActionExemption.AatType = db.GetString(reader, 1);
        entities.ObligationAdmActionExemption.ObgGeneratedId =
          db.GetInt32(reader, 2);
        entities.ObligationAdmActionExemption.CspNumber =
          db.GetString(reader, 3);
        entities.ObligationAdmActionExemption.CpaType = db.GetString(reader, 4);
        entities.ObligationAdmActionExemption.EffectiveDate =
          db.GetDate(reader, 5);
        entities.ObligationAdmActionExemption.EndDate = db.GetDate(reader, 6);
        entities.ObligationAdmActionExemption.Populated = true;
        CheckValid<ObligationAdmActionExemption>("CpaType",
          entities.ObligationAdmActionExemption.CpaType);

        return true;
      });
  }

  private bool ReadObligor1()
  {
    entities.Previous.Populated = false;

    return Read("ReadObligor1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", local.LastCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.Previous.CspNumber = db.GetString(reader, 0);
        entities.Previous.Type1 = db.GetString(reader, 1);
        entities.Previous.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.Previous.Type1);
      });
  }

  private bool ReadObligor2()
  {
    entities.Previous.Populated = false;

    return Read("ReadObligor2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", local.PreviousProcess.Number);
      },
      (db, reader) =>
      {
        entities.Previous.CspNumber = db.GetString(reader, 0);
        entities.Previous.Type1 = db.GetString(reader, 1);
        entities.Previous.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.Previous.Type1);
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
    /// A value of SinglePass.
    /// </summary>
    [JsonPropertyName("singlePass")]
    public Common SinglePass
    {
      get => singlePass ??= new();
      set => singlePass = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of RecordProcessed.
    /// </summary>
    [JsonPropertyName("recordProcessed")]
    public Common RecordProcessed
    {
      get => recordProcessed ??= new();
      set => recordProcessed = value;
    }

    private Common singlePass;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private LegalAction legalAction;
    private CsePerson csePerson;
    private Common recordProcessed;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of TotalAmtDebtOwed.
    /// </summary>
    [JsonPropertyName("totalAmtDebtOwed")]
    public Common TotalAmtDebtOwed
    {
      get => totalAmtDebtOwed ??= new();
      set => totalAmtDebtOwed = value;
    }

    /// <summary>
    /// A value of TotalNumErrorRecords.
    /// </summary>
    [JsonPropertyName("totalNumErrorRecords")]
    public Common TotalNumErrorRecords
    {
      get => totalNumErrorRecords ??= new();
      set => totalNumErrorRecords = value;
    }

    /// <summary>
    /// A value of TotalNumRecsAdded.
    /// </summary>
    [JsonPropertyName("totalNumRecsAdded")]
    public Common TotalNumRecsAdded
    {
      get => totalNumRecsAdded ??= new();
      set => totalNumRecsAdded = value;
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
    /// A value of RecordProcessed.
    /// </summary>
    [JsonPropertyName("recordProcessed")]
    public Common RecordProcessed
    {
      get => recordProcessed ??= new();
      set => recordProcessed = value;
    }

    private Common totalAmtDebtOwed;
    private Common totalNumErrorRecords;
    private Common totalNumRecsAdded;
    private Common numberOfRecordsRead;
    private Common recordProcessed;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A IwoPeriodGroup group.</summary>
    [Serializable]
    public class IwoPeriodGroup
    {
      /// <summary>
      /// A value of EndDate.
      /// </summary>
      [JsonPropertyName("endDate")]
      public DateWorkArea EndDate
      {
        get => endDate ??= new();
        set => endDate = value;
      }

      /// <summary>
      /// A value of BeginDate.
      /// </summary>
      [JsonPropertyName("beginDate")]
      public DateWorkArea BeginDate
      {
        get => beginDate ??= new();
        set => beginDate = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 36;

      private DateWorkArea endDate;
      private DateWorkArea beginDate;
    }

    /// <summary>A AliasGroup group.</summary>
    [Serializable]
    public class AliasGroup
    {
      /// <summary>
      /// A value of Grps.
      /// </summary>
      [JsonPropertyName("grps")]
      public CsePersonsWorkSet Grps
      {
        get => grps ??= new();
        set => grps = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private CsePersonsWorkSet grps;
    }

    /// <summary>
    /// A value of HasIwo.
    /// </summary>
    [JsonPropertyName("hasIwo")]
    public Common HasIwo
    {
      get => hasIwo ??= new();
      set => hasIwo = value;
    }

    /// <summary>
    /// A value of Date.
    /// </summary>
    [JsonPropertyName("date")]
    public TextWorkArea Date
    {
      get => date ??= new();
      set => date = value;
    }

    /// <summary>
    /// A value of MeetsCriteria.
    /// </summary>
    [JsonPropertyName("meetsCriteria")]
    public Common MeetsCriteria
    {
      get => meetsCriteria ??= new();
      set => meetsCriteria = value;
    }

    /// <summary>
    /// A value of IwoPayment.
    /// </summary>
    [JsonPropertyName("iwoPayment")]
    public Common IwoPayment
    {
      get => iwoPayment ??= new();
      set => iwoPayment = value;
    }

    /// <summary>
    /// A value of IwoPeriod1.
    /// </summary>
    [JsonPropertyName("iwoPeriod1")]
    public Common IwoPeriod1
    {
      get => iwoPeriod1 ??= new();
      set => iwoPeriod1 = value;
    }

    /// <summary>
    /// A value of Base1.
    /// </summary>
    [JsonPropertyName("base1")]
    public DateWorkArea Base1
    {
      get => base1 ??= new();
      set => base1 = value;
    }

    /// <summary>
    /// A value of PrepareDate.
    /// </summary>
    [JsonPropertyName("prepareDate")]
    public DateWorkAttributes PrepareDate
    {
      get => prepareDate ??= new();
      set => prepareDate = value;
    }

    /// <summary>
    /// Gets a value of IwoPeriod.
    /// </summary>
    [JsonIgnore]
    public Array<IwoPeriodGroup> IwoPeriod => iwoPeriod ??= new(
      IwoPeriodGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of IwoPeriod for json serialization.
    /// </summary>
    [JsonPropertyName("iwoPeriod")]
    [Computed]
    public IList<IwoPeriodGroup> IwoPeriod_Json
    {
      get => iwoPeriod;
      set => IwoPeriod.Assign(value);
    }

    /// <summary>
    /// A value of NumMonthsIwoPeriod.
    /// </summary>
    [JsonPropertyName("numMonthsIwoPeriod")]
    public Common NumMonthsIwoPeriod
    {
      get => numMonthsIwoPeriod ??= new();
      set => numMonthsIwoPeriod = value;
    }

    /// <summary>
    /// A value of NumDaysAccruingOrDue.
    /// </summary>
    [JsonPropertyName("numDaysAccruingOrDue")]
    public Common NumDaysAccruingOrDue
    {
      get => numDaysAccruingOrDue ??= new();
      set => numDaysAccruingOrDue = value;
    }

    /// <summary>
    /// A value of NumDaysSinceObCreated.
    /// </summary>
    [JsonPropertyName("numDaysSinceObCreated")]
    public Common NumDaysSinceObCreated
    {
      get => numDaysSinceObCreated ??= new();
      set => numDaysSinceObCreated = value;
    }

    /// <summary>
    /// A value of ReadExemp.
    /// </summary>
    [JsonPropertyName("readExemp")]
    public LegalAction ReadExemp
    {
      get => readExemp ??= new();
      set => readExemp = value;
    }

    /// <summary>
    /// A value of ColAlreadyAccountedFor.
    /// </summary>
    [JsonPropertyName("colAlreadyAccountedFor")]
    public Common ColAlreadyAccountedFor
    {
      get => colAlreadyAccountedFor ??= new();
      set => colAlreadyAccountedFor = value;
    }

    /// <summary>
    /// A value of LastSuccessfulProcessed.
    /// </summary>
    [JsonPropertyName("lastSuccessfulProcessed")]
    public CsePersonsWorkSet LastSuccessfulProcessed
    {
      get => lastSuccessfulProcessed ??= new();
      set => lastSuccessfulProcessed = value;
    }

    /// <summary>
    /// A value of LastSuccesfulProcessed.
    /// </summary>
    [JsonPropertyName("lastSuccesfulProcessed")]
    public LegalAction LastSuccesfulProcessed
    {
      get => lastSuccesfulProcessed ??= new();
      set => lastSuccesfulProcessed = value;
    }

    /// <summary>
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public LegalAction Error
    {
      get => error ??= new();
      set => error = value;
    }

    /// <summary>
    /// A value of LastPersonTotal.
    /// </summary>
    [JsonPropertyName("lastPersonTotal")]
    public Common LastPersonTotal
    {
      get => lastPersonTotal ??= new();
      set => lastPersonTotal = value;
    }

    /// <summary>
    /// A value of StartBatchTimestampWorkArea.
    /// </summary>
    [JsonPropertyName("startBatchTimestampWorkArea")]
    public BatchTimestampWorkArea StartBatchTimestampWorkArea
    {
      get => startBatchTimestampWorkArea ??= new();
      set => startBatchTimestampWorkArea = value;
    }

    /// <summary>
    /// A value of KsWildlifeParks.
    /// </summary>
    [JsonPropertyName("ksWildlifeParks")]
    public AdministrativeActCertification KsWildlifeParks
    {
      get => ksWildlifeParks ??= new();
      set => ksWildlifeParks = value;
    }

    /// <summary>
    /// A value of ClearLegalAction.
    /// </summary>
    [JsonPropertyName("clearLegalAction")]
    public LegalAction ClearLegalAction
    {
      get => clearLegalAction ??= new();
      set => clearLegalAction = value;
    }

    /// <summary>
    /// A value of LastCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("lastCsePersonsWorkSet")]
    public CsePersonsWorkSet LastCsePersonsWorkSet
    {
      get => lastCsePersonsWorkSet ??= new();
      set => lastCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of LastLegalAction.
    /// </summary>
    [JsonPropertyName("lastLegalAction")]
    public LegalAction LastLegalAction
    {
      get => lastLegalAction ??= new();
      set => lastLegalAction = value;
    }

    /// <summary>
    /// A value of NumOfRecordsProcessed.
    /// </summary>
    [JsonPropertyName("numOfRecordsProcessed")]
    public WorkArea NumOfRecordsProcessed
    {
      get => numOfRecordsProcessed ??= new();
      set => numOfRecordsProcessed = value;
    }

    /// <summary>
    /// A value of TotalNumberRecordsRead.
    /// </summary>
    [JsonPropertyName("totalNumberRecordsRead")]
    public WorkArea TotalNumberRecordsRead
    {
      get => totalNumberRecordsRead ??= new();
      set => totalNumberRecordsRead = value;
    }

    /// <summary>
    /// A value of TotalAmount.
    /// </summary>
    [JsonPropertyName("totalAmount")]
    public WorkArea TotalAmount
    {
      get => totalAmount ??= new();
      set => totalAmount = value;
    }

    /// <summary>
    /// A value of TotalNumberOfErrors.
    /// </summary>
    [JsonPropertyName("totalNumberOfErrors")]
    public WorkArea TotalNumberOfErrors
    {
      get => totalNumberOfErrors ??= new();
      set => totalNumberOfErrors = value;
    }

    /// <summary>
    /// A value of RestartAmount.
    /// </summary>
    [JsonPropertyName("restartAmount")]
    public WorkArea RestartAmount
    {
      get => restartAmount ??= new();
      set => restartAmount = value;
    }

    /// <summary>
    /// A value of RestartLegalId.
    /// </summary>
    [JsonPropertyName("restartLegalId")]
    public WorkArea RestartLegalId
    {
      get => restartLegalId ??= new();
      set => restartLegalId = value;
    }

    /// <summary>
    /// A value of ProcessLastRecord.
    /// </summary>
    [JsonPropertyName("processLastRecord")]
    public Common ProcessLastRecord
    {
      get => processLastRecord ??= new();
      set => processLastRecord = value;
    }

    /// <summary>
    /// A value of AlreadyCheckedCtNumCsePerson.
    /// </summary>
    [JsonPropertyName("alreadyCheckedCtNumCsePerson")]
    public CsePerson AlreadyCheckedCtNumCsePerson
    {
      get => alreadyCheckedCtNumCsePerson ??= new();
      set => alreadyCheckedCtNumCsePerson = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public LegalAction Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of PreviousProcess.
    /// </summary>
    [JsonPropertyName("previousProcess")]
    public CsePersonsWorkSet PreviousProcess
    {
      get => previousProcess ??= new();
      set => previousProcess = value;
    }

    /// <summary>
    /// A value of ClearKdwpOutput.
    /// </summary>
    [JsonPropertyName("clearKdwpOutput")]
    public KdwpOutput ClearKdwpOutput
    {
      get => clearKdwpOutput ??= new();
      set => clearKdwpOutput = value;
    }

    /// <summary>
    /// Gets a value of Alias.
    /// </summary>
    [JsonIgnore]
    public Array<AliasGroup> Alias => alias ??= new(AliasGroup.Capacity);

    /// <summary>
    /// Gets a value of Alias for json serialization.
    /// </summary>
    [JsonPropertyName("alias")]
    [Computed]
    public IList<AliasGroup> Alias_Json
    {
      get => alias;
      set => Alias.Assign(value);
    }

    /// <summary>
    /// A value of PreviousPersonTotal.
    /// </summary>
    [JsonPropertyName("previousPersonTotal")]
    public Common PreviousPersonTotal
    {
      get => previousPersonTotal ??= new();
      set => previousPersonTotal = value;
    }

    /// <summary>
    /// A value of AdministrativeAction.
    /// </summary>
    [JsonPropertyName("administrativeAction")]
    public AdministrativeAction AdministrativeAction
    {
      get => administrativeAction ??= new();
      set => administrativeAction = value;
    }

    /// <summary>
    /// A value of KdwpOutput.
    /// </summary>
    [JsonPropertyName("kdwpOutput")]
    public KdwpOutput KdwpOutput
    {
      get => kdwpOutput ??= new();
      set => kdwpOutput = value;
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
    /// A value of PreviousfindMe.
    /// </summary>
    [JsonPropertyName("previousfindMe")]
    public CsePerson PreviousfindMe
    {
      get => previousfindMe ??= new();
      set => previousfindMe = value;
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
    /// A value of SkipObligor.
    /// </summary>
    [JsonPropertyName("skipObligor")]
    public CsePerson SkipObligor
    {
      get => skipObligor ??= new();
      set => skipObligor = value;
    }

    /// <summary>
    /// A value of Hold.
    /// </summary>
    [JsonPropertyName("hold")]
    public LegalAction Hold
    {
      get => hold ??= new();
      set => hold = value;
    }

    /// <summary>
    /// A value of AlreadyDone.
    /// </summary>
    [JsonPropertyName("alreadyDone")]
    public Common AlreadyDone
    {
      get => alreadyDone ??= new();
      set => alreadyDone = value;
    }

    /// <summary>
    /// A value of TotalCollection.
    /// </summary>
    [JsonPropertyName("totalCollection")]
    public Common TotalCollection
    {
      get => totalCollection ??= new();
      set => totalCollection = value;
    }

    /// <summary>
    /// A value of ContinueProcess.
    /// </summary>
    [JsonPropertyName("continueProcess")]
    public Common ContinueProcess
    {
      get => continueProcess ??= new();
      set => continueProcess = value;
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
    /// A value of AmountOwed.
    /// </summary>
    [JsonPropertyName("amountOwed")]
    public Common AmountOwed
    {
      get => amountOwed ??= new();
      set => amountOwed = value;
    }

    /// <summary>
    /// A value of ZeroDate.
    /// </summary>
    [JsonPropertyName("zeroDate")]
    public DateWorkArea ZeroDate
    {
      get => zeroDate ??= new();
      set => zeroDate = value;
    }

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of StartDate.
    /// </summary>
    [JsonPropertyName("startDate")]
    public DateWorkArea StartDate
    {
      get => startDate ??= new();
      set => startDate = value;
    }

    /// <summary>
    /// A value of IncludeArrearsOnly.
    /// </summary>
    [JsonPropertyName("includeArrearsOnly")]
    public Common IncludeArrearsOnly
    {
      get => includeArrearsOnly ??= new();
      set => includeArrearsOnly = value;
    }

    /// <summary>
    /// A value of MiniumPayment.
    /// </summary>
    [JsonPropertyName("miniumPayment")]
    public Common MiniumPayment
    {
      get => miniumPayment ??= new();
      set => miniumPayment = value;
    }

    /// <summary>
    /// A value of From.
    /// </summary>
    [JsonPropertyName("from")]
    public DateWorkArea From
    {
      get => from ??= new();
      set => from = value;
    }

    /// <summary>
    /// A value of To.
    /// </summary>
    [JsonPropertyName("to")]
    public DateWorkArea To
    {
      get => to ??= new();
      set => to = value;
    }

    /// <summary>
    /// A value of EndDate.
    /// </summary>
    [JsonPropertyName("endDate")]
    public DateWorkArea EndDate
    {
      get => endDate ??= new();
      set => endDate = value;
    }

    /// <summary>
    /// A value of OwedAmount.
    /// </summary>
    [JsonPropertyName("owedAmount")]
    public Common OwedAmount
    {
      get => owedAmount ??= new();
      set => owedAmount = value;
    }

    /// <summary>
    /// A value of ClearScreenOwedAmountsDtl.
    /// </summary>
    [JsonPropertyName("clearScreenOwedAmountsDtl")]
    public ScreenOwedAmountsDtl ClearScreenOwedAmountsDtl
    {
      get => clearScreenOwedAmountsDtl ??= new();
      set => clearScreenOwedAmountsDtl = value;
    }

    /// <summary>
    /// A value of ScreenOwedAmountsDtl.
    /// </summary>
    [JsonPropertyName("screenOwedAmountsDtl")]
    public ScreenOwedAmountsDtl ScreenOwedAmountsDtl
    {
      get => screenOwedAmountsDtl ??= new();
      set => screenOwedAmountsDtl = value;
    }

    /// <summary>
    /// A value of AlreadyCheckedCtNumLegalAction.
    /// </summary>
    [JsonPropertyName("alreadyCheckedCtNumLegalAction")]
    public LegalAction AlreadyCheckedCtNumLegalAction
    {
      get => alreadyCheckedCtNumLegalAction ??= new();
      set => alreadyCheckedCtNumLegalAction = value;
    }

    /// <summary>
    /// A value of ClearBatchTimestampWorkArea.
    /// </summary>
    [JsonPropertyName("clearBatchTimestampWorkArea")]
    public BatchTimestampWorkArea ClearBatchTimestampWorkArea
    {
      get => clearBatchTimestampWorkArea ??= new();
      set => clearBatchTimestampWorkArea = value;
    }

    /// <summary>
    /// A value of TestTime.
    /// </summary>
    [JsonPropertyName("testTime")]
    public BatchTimestampWorkArea TestTime
    {
      get => testTime ??= new();
      set => testTime = value;
    }

    /// <summary>
    /// A value of NumberOfProcessedRecord.
    /// </summary>
    [JsonPropertyName("numberOfProcessedRecord")]
    public Common NumberOfProcessedRecord
    {
      get => numberOfProcessedRecord ??= new();
      set => numberOfProcessedRecord = value;
    }

    /// <summary>
    /// A value of TestCall.
    /// </summary>
    [JsonPropertyName("testCall")]
    public WorkArea TestCall
    {
      get => testCall ??= new();
      set => testCall = value;
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
    /// A value of PersonTotal.
    /// </summary>
    [JsonPropertyName("personTotal")]
    public Common PersonTotal
    {
      get => personTotal ??= new();
      set => personTotal = value;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public Common Search
    {
      get => search ??= new();
      set => search = value;
    }

    /// <summary>
    /// A value of Phonetic.
    /// </summary>
    [JsonPropertyName("phonetic")]
    public Common Phonetic
    {
      get => phonetic ??= new();
      set => phonetic = value;
    }

    /// <summary>
    /// A value of StartCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("startCsePersonsWorkSet")]
    public CsePersonsWorkSet StartCsePersonsWorkSet
    {
      get => startCsePersonsWorkSet ??= new();
      set => startCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of AlreadyCheckedSsn.
    /// </summary>
    [JsonPropertyName("alreadyCheckedSsn")]
    public CsePerson AlreadyCheckedSsn
    {
      get => alreadyCheckedSsn ??= new();
      set => alreadyCheckedSsn = value;
    }

    /// <summary>
    /// A value of ClearCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("clearCsePersonsWorkSet")]
    public CsePersonsWorkSet ClearCsePersonsWorkSet
    {
      get => clearCsePersonsWorkSet ??= new();
      set => clearCsePersonsWorkSet = value;
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
    /// A value of WorkArea.
    /// </summary>
    [JsonPropertyName("workArea")]
    public WorkArea WorkArea
    {
      get => workArea ??= new();
      set => workArea = value;
    }

    /// <summary>
    /// A value of Year.
    /// </summary>
    [JsonPropertyName("year")]
    public TextWorkArea Year
    {
      get => year ??= new();
      set => year = value;
    }

    /// <summary>
    /// A value of Month.
    /// </summary>
    [JsonPropertyName("month")]
    public TextWorkArea Month
    {
      get => month ??= new();
      set => month = value;
    }

    /// <summary>
    /// A value of Day.
    /// </summary>
    [JsonPropertyName("day")]
    public TextWorkArea Day
    {
      get => day ??= new();
      set => day = value;
    }

    /// <summary>
    /// A value of MissingSsnAndDodRpt.
    /// </summary>
    [JsonPropertyName("missingSsnAndDodRpt")]
    public MissingSsnAndDodRpt MissingSsnAndDodRpt
    {
      get => missingSsnAndDodRpt ??= new();
      set => missingSsnAndDodRpt = value;
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
    /// A value of FinanceWorkAttributes.
    /// </summary>
    [JsonPropertyName("financeWorkAttributes")]
    public FinanceWorkAttributes FinanceWorkAttributes
    {
      get => financeWorkAttributes ??= new();
      set => financeWorkAttributes = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of Detail.
    /// </summary>
    [JsonPropertyName("detail")]
    public WorkArea Detail
    {
      get => detail ??= new();
      set => detail = value;
    }

    /// <summary>
    /// A value of PersonSend.
    /// </summary>
    [JsonPropertyName("personSend")]
    public Common PersonSend
    {
      get => personSend ??= new();
      set => personSend = value;
    }

    /// <summary>
    /// A value of MiniumTarget.
    /// </summary>
    [JsonPropertyName("miniumTarget")]
    public Common MiniumTarget
    {
      get => miniumTarget ??= new();
      set => miniumTarget = value;
    }

    /// <summary>
    /// A value of Kdwp.
    /// </summary>
    [JsonPropertyName("kdwp")]
    public AdministrativeActCertification Kdwp
    {
      get => kdwp ??= new();
      set => kdwp = value;
    }

    /// <summary>
    /// A value of SkipRecord.
    /// </summary>
    [JsonPropertyName("skipRecord")]
    public Common SkipRecord
    {
      get => skipRecord ??= new();
      set => skipRecord = value;
    }

    /// <summary>
    /// A value of PriorKdwp.
    /// </summary>
    [JsonPropertyName("priorKdwp")]
    public Common PriorKdwp
    {
      get => priorKdwp ??= new();
      set => priorKdwp = value;
    }

    /// <summary>
    /// A value of AlreadyWritten.
    /// </summary>
    [JsonPropertyName("alreadyWritten")]
    public CsePerson AlreadyWritten
    {
      get => alreadyWritten ??= new();
      set => alreadyWritten = value;
    }

    /// <summary>
    /// A value of ValidAddress.
    /// </summary>
    [JsonPropertyName("validAddress")]
    public Common ValidAddress
    {
      get => validAddress ??= new();
      set => validAddress = value;
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
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
    }

    /// <summary>
    /// A value of TotalNumberOfErrorsRec.
    /// </summary>
    [JsonPropertyName("totalNumberOfErrorsRec")]
    public Common TotalNumberOfErrorsRec
    {
      get => totalNumberOfErrorsRec ??= new();
      set => totalNumberOfErrorsRec = value;
    }

    /// <summary>
    /// A value of Postion.
    /// </summary>
    [JsonPropertyName("postion")]
    public TextWorkArea Postion
    {
      get => postion ??= new();
      set => postion = value;
    }

    /// <summary>
    /// A value of CurrentPosition.
    /// </summary>
    [JsonPropertyName("currentPosition")]
    public Common CurrentPosition
    {
      get => currentPosition ??= new();
      set => currentPosition = value;
    }

    /// <summary>
    /// A value of FieldNumber.
    /// </summary>
    [JsonPropertyName("fieldNumber")]
    public Common FieldNumber
    {
      get => fieldNumber ??= new();
      set => fieldNumber = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public Common Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of StartCommon.
    /// </summary>
    [JsonPropertyName("startCommon")]
    public Common StartCommon
    {
      get => startCommon ??= new();
      set => startCommon = value;
    }

    /// <summary>
    /// A value of NumberOfDays.
    /// </summary>
    [JsonPropertyName("numberOfDays")]
    public Common NumberOfDays
    {
      get => numberOfDays ??= new();
      set => numberOfDays = value;
    }

    /// <summary>
    /// A value of Read.
    /// </summary>
    [JsonPropertyName("read")]
    public LegalAction Read
    {
      get => read ??= new();
      set => read = value;
    }

    /// <summary>
    /// A value of RecordCount.
    /// </summary>
    [JsonPropertyName("recordCount")]
    public Common RecordCount
    {
      get => recordCount ??= new();
      set => recordCount = value;
    }

    private Common hasIwo;
    private TextWorkArea date;
    private Common meetsCriteria;
    private Common iwoPayment;
    private Common iwoPeriod1;
    private DateWorkArea base1;
    private DateWorkAttributes prepareDate;
    private Array<IwoPeriodGroup> iwoPeriod;
    private Common numMonthsIwoPeriod;
    private Common numDaysAccruingOrDue;
    private Common numDaysSinceObCreated;
    private LegalAction readExemp;
    private Common colAlreadyAccountedFor;
    private CsePersonsWorkSet lastSuccessfulProcessed;
    private LegalAction lastSuccesfulProcessed;
    private LegalAction error;
    private Common lastPersonTotal;
    private BatchTimestampWorkArea startBatchTimestampWorkArea;
    private AdministrativeActCertification ksWildlifeParks;
    private LegalAction clearLegalAction;
    private CsePersonsWorkSet lastCsePersonsWorkSet;
    private LegalAction lastLegalAction;
    private WorkArea numOfRecordsProcessed;
    private WorkArea totalNumberRecordsRead;
    private WorkArea totalAmount;
    private WorkArea totalNumberOfErrors;
    private WorkArea restartAmount;
    private WorkArea restartLegalId;
    private Common processLastRecord;
    private CsePerson alreadyCheckedCtNumCsePerson;
    private LegalAction legalAction;
    private LegalAction previous;
    private CsePersonsWorkSet previousProcess;
    private KdwpOutput clearKdwpOutput;
    private Array<AliasGroup> alias;
    private Common previousPersonTotal;
    private AdministrativeAction administrativeAction;
    private KdwpOutput kdwpOutput;
    private ProgramCheckpointRestart programCheckpointRestart;
    private CsePerson previousfindMe;
    private Common numberOfRecordsRead;
    private CsePerson skipObligor;
    private LegalAction hold;
    private Common alreadyDone;
    private Common totalCollection;
    private Common continueProcess;
    private CsePerson csePerson;
    private Common amountOwed;
    private DateWorkArea zeroDate;
    private DateWorkArea dateWorkArea;
    private DateWorkArea startDate;
    private Common includeArrearsOnly;
    private Common miniumPayment;
    private DateWorkArea from;
    private DateWorkArea to;
    private DateWorkArea endDate;
    private Common owedAmount;
    private ScreenOwedAmountsDtl clearScreenOwedAmountsDtl;
    private ScreenOwedAmountsDtl screenOwedAmountsDtl;
    private LegalAction alreadyCheckedCtNumLegalAction;
    private BatchTimestampWorkArea clearBatchTimestampWorkArea;
    private BatchTimestampWorkArea testTime;
    private Common numberOfProcessedRecord;
    private WorkArea testCall;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private Common personTotal;
    private Common search;
    private Common phonetic;
    private CsePersonsWorkSet startCsePersonsWorkSet;
    private CsePerson alreadyCheckedSsn;
    private CsePersonsWorkSet clearCsePersonsWorkSet;
    private CsePersonsWorkSet csePersonsWorkSet;
    private WorkArea workArea;
    private TextWorkArea year;
    private TextWorkArea month;
    private TextWorkArea day;
    private MissingSsnAndDodRpt missingSsnAndDodRpt;
    private External passArea;
    private FinanceWorkAttributes financeWorkAttributes;
    private Infrastructure infrastructure;
    private ProgramProcessingInfo programProcessingInfo;
    private WorkArea detail;
    private Common personSend;
    private Common miniumTarget;
    private AdministrativeActCertification kdwp;
    private Common skipRecord;
    private Common priorKdwp;
    private CsePerson alreadyWritten;
    private Common validAddress;
    private AbendData abendData;
    private ExitStateWorkArea exitStateWorkArea;
    private Common totalNumberOfErrorsRec;
    private TextWorkArea postion;
    private Common currentPosition;
    private Common fieldNumber;
    private Common current;
    private Common startCommon;
    private Common numberOfDays;
    private LegalAction read;
    private Common recordCount;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    /// <summary>
    /// A value of Iwo.
    /// </summary>
    [JsonPropertyName("iwo")]
    public LegalAction Iwo
    {
      get => iwo ??= new();
      set => iwo = value;
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
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public CsePersonAccount Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of KsWildlifeParks.
    /// </summary>
    [JsonPropertyName("ksWildlifeParks")]
    public AdministrativeActCertification KsWildlifeParks
    {
      get => ksWildlifeParks ??= new();
      set => ksWildlifeParks = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
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
    /// A value of Debt.
    /// </summary>
    [JsonPropertyName("debt")]
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of AccrualInstructions.
    /// </summary>
    [JsonPropertyName("accrualInstructions")]
    public AccrualInstructions AccrualInstructions
    {
      get => accrualInstructions ??= new();
      set => accrualInstructions = value;
    }

    /// <summary>
    /// A value of ObligationAdmActionExemption.
    /// </summary>
    [JsonPropertyName("obligationAdmActionExemption")]
    public ObligationAdmActionExemption ObligationAdmActionExemption
    {
      get => obligationAdmActionExemption ??= new();
      set => obligationAdmActionExemption = value;
    }

    /// <summary>
    /// A value of AdministrativeAction.
    /// </summary>
    [JsonPropertyName("administrativeAction")]
    public AdministrativeAction AdministrativeAction
    {
      get => administrativeAction ??= new();
      set => administrativeAction = value;
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
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
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
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailStatHistory.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatHistory")]
    public CashReceiptDetailStatHistory CashReceiptDetailStatHistory
    {
      get => cashReceiptDetailStatHistory ??= new();
      set => cashReceiptDetailStatHistory = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatus")]
    public CashReceiptDetailStatus CashReceiptDetailStatus
    {
      get => cashReceiptDetailStatus ??= new();
      set => cashReceiptDetailStatus = value;
    }

    /// <summary>
    /// A value of GoodCause.
    /// </summary>
    [JsonPropertyName("goodCause")]
    public GoodCause GoodCause
    {
      get => goodCause ??= new();
      set => goodCause = value;
    }

    /// <summary>
    /// A value of OthCaseRole.
    /// </summary>
    [JsonPropertyName("othCaseRole")]
    public CaseRole OthCaseRole
    {
      get => othCaseRole ??= new();
      set => othCaseRole = value;
    }

    /// <summary>
    /// A value of OthCsePerson.
    /// </summary>
    [JsonPropertyName("othCsePerson")]
    public CsePerson OthCsePerson
    {
      get => othCsePerson ??= new();
      set => othCsePerson = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
    }

    /// <summary>
    /// A value of N2dView.
    /// </summary>
    [JsonPropertyName("n2dView")]
    public LegalAction N2dView
    {
      get => n2dView ??= new();
      set => n2dView = value;
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
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
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
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
    }

    private LegalActionDetail legalActionDetail;
    private LegalAction iwo;
    private Infrastructure infrastructure;
    private CsePersonAccount previous;
    private AdministrativeActCertification ksWildlifeParks;
    private CsePerson csePerson;
    private LegalAction legalAction;
    private DebtDetail debtDetail;
    private Obligation obligation;
    private CsePersonAccount obligor;
    private ObligationTransaction debt;
    private ObligationType obligationType;
    private AccrualInstructions accrualInstructions;
    private ObligationAdmActionExemption obligationAdmActionExemption;
    private AdministrativeAction administrativeAction;
    private CashReceiptDetail cashReceiptDetail;
    private CollectionType collectionType;
    private CashReceipt cashReceipt;
    private CashReceiptType cashReceiptType;
    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private GoodCause goodCause;
    private CaseRole othCaseRole;
    private CsePerson othCsePerson;
    private CaseRole caseRole;
    private Case1 case1;
    private LegalActionPerson legalActionPerson;
    private LegalAction n2dView;
    private CaseUnit caseUnit;
    private LegalActionCaseRole legalActionCaseRole;
    private AdministrativeActCertification administrativeActCertification;
    private CsePersonAddress csePersonAddress;
  }
#endregion
}
