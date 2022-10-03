// Program: OE_B458_KDMV_30_DAY_NOTICE, ID: 371384451, model: 746.
// Short name: SWEE458B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B458_KDMV_30_DAY_NOTICE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class OeB458Kdmv30DayNotice: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B458_KDMV_30_DAY_NOTICE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB458Kdmv30DayNotice(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB458Kdmv30DayNotice.
  /// </summary>
  public OeB458Kdmv30DayNotice(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***********************************************************************************************
    // DATE		Developer	Description
    // 04/14/2008      DDupree   	Initial Creation - WR280420
    // ***********************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    UseOeB458Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.StartCommon.Count = 1;
    local.Current.Count = 1;
    local.CurrentPosition.Count = 1;
    local.FieldNumber.Count = 0;
    local.IncludeArrearsOnly.Flag = "";
    export.TotalNumRecsAdded.Count = 0;
    local.TotalAdded.Text15 =
      Substring(local.ProgramCheckpointRestart.RestartInfo, 1, 15);
    local.LastUpdatedTotalAdded.Count =
      (int)StringToNumber(local.TotalAdded.Text15);
    local.TotalNumLettersSent.Count = local.LastUpdatedTotalAdded.Count;
    local.TotalErrors.Text15 =
      Substring(local.ProgramCheckpointRestart.RestartInfo, 16, 15);
    local.NumErrorRecords.Count = (int)StringToNumber(local.TotalErrors.Text15);
    local.LastUpdatedTotalError.Count = local.NumErrorRecords.Count;
    local.TotalRecordsUpdated.Text15 =
      Substring(local.ProgramCheckpointRestart.RestartInfo, 31, 15);
    local.NumOfRecordsUpdated.Count =
      (int)StringToNumber(local.TotalRecordsUpdated.Text15);
    local.LastUpdatedNumRecsUpd.Count = local.NumOfRecordsUpdated.Count;
    local.TotalRecordsRead.Text15 =
      Substring(local.ProgramCheckpointRestart.RestartInfo, 46, 15);
    local.NumberOfRecordsRead.Count =
      (int)StringToNumber(local.TotalRecordsRead.Text15);
    local.LastUpdatedNumRecsRead.Count = local.NumberOfRecordsRead.Count;
    local.TotalRecordsRead.Text15 =
      Substring(local.ProgramCheckpointRestart.RestartInfo, 61, 15);
    local.NumberOfRecordsRead.Count =
      (int)StringToNumber(local.TotalRecordsRead.Text15);
    local.ReadKsDriversLicense.SequenceCounter =
      local.ProgramCheckpointRestart.CheckpointCount.GetValueOrDefault();

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
              local.MinimumTarget.TotalCurrency = 0;
            }
            else
            {
              local.WorkArea.Text15 =
                Substring(local.ProgramProcessingInfo.ParameterList,
                local.StartCommon.Count, local.Current.Count - 1);
              local.MinimumTarget.TotalCurrency =
                StringToNumber(local.WorkArea.Text15);
            }

            local.StartCommon.Count = local.CurrentPosition.Count + 1;
            local.Current.Count = 0;

            break;
          case 2:
            if (local.Current.Count == 1)
            {
              local.PaymentPeriodDays.Count = 0;
            }
            else
            {
              local.WorkArea.Text15 =
                Substring(local.ProgramProcessingInfo.ParameterList,
                local.StartCommon.Count, local.Current.Count - 1);
              local.PaymentPeriodDays.Count =
                (int)StringToNumber(local.WorkArea.Text15);
            }

            local.StartCommon.Count = local.CurrentPosition.Count + 1;
            local.Current.Count = 0;

            break;
          case 3:
            if (local.Current.Count == 1)
            {
              local.MinimumPayment.TotalCurrency = 0;
            }
            else
            {
              local.WorkArea.Text15 =
                Substring(local.ProgramProcessingInfo.ParameterList,
                local.StartCommon.Count, local.Current.Count - 1);
              local.MinimumPayment.TotalCurrency =
                StringToNumber(local.WorkArea.Text15);
            }

            local.StartCommon.Count = local.CurrentPosition.Count + 1;
            local.Current.Count = 0;

            break;
          case 4:
            if (local.Current.Count == 1)
            {
              local.ArrearsOnly.Flag = "N";
            }
            else
            {
              local.ArrearsOnly.Flag =
                Substring(local.ProgramProcessingInfo.ParameterList,
                local.StartCommon.Count, local.Current.Count - 1);
            }

            local.StartCommon.Count = local.CurrentPosition.Count + 1;
            local.Current.Count = 0;

            break;
          case 5:
            if (local.Current.Count == 1)
            {
              local.NumDaysSinceCurtsyLtr.Count = 45;
            }
            else
            {
              local.WorkArea.Text15 =
                Substring(local.ProgramProcessingInfo.ParameterList,
                local.StartCommon.Count, local.Current.Count - 1);
              local.NumDaysSinceCurtsyLtr.Count =
                (int)StringToNumber(local.WorkArea.Text15);
            }

            local.StartCommon.Count = local.CurrentPosition.Count + 1;
            local.Current.Count = 0;

            break;
          case 6:
            if (local.Current.Count == 1)
            {
              local.NumMnthBtw30DayLtr.Count = 24;
            }
            else
            {
              local.WorkArea.Text15 =
                Substring(local.ProgramProcessingInfo.ParameterList,
                local.StartCommon.Count, local.Current.Count - 1);
              local.NumMnthBtw30DayLtr.Count =
                (int)StringToNumber(local.WorkArea.Text15);
            }

            local.StartCommon.Count = local.CurrentPosition.Count + 1;
            local.Current.Count = 0;

            break;
          case 7:
            if (local.Current.Count == 1)
            {
              local.MaxNumberObligors.Count = 1000;
            }
            else
            {
              local.WorkArea.Text15 =
                Substring(local.ProgramProcessingInfo.ParameterList,
                local.StartCommon.Count, local.Current.Count - 1);
              local.MaxNumberObligors.Count =
                (int)StringToNumber(local.WorkArea.Text15);
            }

            local.StartCommon.Count = local.CurrentPosition.Count + 1;
            local.Current.Count = 0;

            break;
          case 8:
            if (local.Current.Count == 1)
            {
              local.NumDaysBetw30DayLtr.Count = 90;
            }
            else
            {
              local.WorkArea.Text15 =
                Substring(local.ProgramProcessingInfo.ParameterList,
                local.StartCommon.Count, local.Current.Count - 1);
              local.NumDaysBetw30DayLtr.Count =
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

    local.CurrentDate.Date = Now().Date;
    local.BatchTimestampWorkArea.IefTimestamp = new DateTime(1, 1, 1);
    local.StartDate.Date = local.ProgramProcessingInfo.ProcessDate;
    local.AdministrativeAction.Type1 = "KDMV";
    local.Local30DayLetterTimeFrame.Date =
      AddMonths(local.StartDate.Date, -local.NumMnthBtw30DayLtr.Count);
    local.ValidPeriod30DayNotice.Date =
      AddMonths(local.StartDate.Date, -local.NumMnthBtw30DayLtr.Count);
    local.CourtesyLetterCompare.Date =
      AddDays(local.StartDate.Date, -local.NumDaysSinceCurtsyLtr.Count);
    local.ValidPeriod30DayNotic.Date =
      AddDays(local.StartDate.Date, -local.NumDaysBetw30DayLtr.Count);

    // the cse person license table will be replaced with the new table (cse ks 
    // drivers
    // license) when it is created, for right now it will be used as a standin
    if (ReadKsDriversLicense4())
    {
      local.LastLicenseProcessedDt.Date =
        entities.ProcessedCheck.ValidationDate;
    }

    ReadKsDriversLicense1();
    local.SequenceCount.Count = local.ReadCounter.Count;
    local.Group30DayNotice.Index = -1;
    local.Group30DayNotice.Count = 0;

    foreach(var item in ReadKsDriversLicense5())
    {
      local.ReadKsDriversLicense.SequenceCounter =
        entities.ReadOnlyKsDriversLicense.SequenceCounter;
      ExitState = "ACO_NN0000_ALL_OK";

      if (ReadCsePerson())
      {
        local.ReadCsePerson.Number = entities.ReadOnlyCsePerson2.Number;
      }

      // checking to make sure we have not already processed this record before
      if (ReadKsDriversLicense3())
      {
        continue;
      }
      else
      {
        // we have not processed this ap so we will continue processing it
      }

      ++local.NumberOfRecordsRead.Count;
      local.FirstRecordProcessed.Flag = "";
      local.RecordProcessed.Flag = "";

      if (ReadKsDriversLicenseCsePerson())
      {
        try
        {
          UpdateKsDriversLicense1();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "KS_DRIVERS_LICENSE_NU";
              ++local.NumErrorRecords.Count;

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "KS_DRIVERS_LICENSE_PV";
              ++local.NumErrorRecords.Count;

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          foreach(var item1 in ReadKsDriversLicense6())
          {
            if (Equal(entities.SecondRead.Attribute30DayLetterCreatedDate,
              local.StartDate.Date))
            {
              goto ReadEach1;

              // for some reason (possibly a restart) we have picked a record 
              // that has already
              // been processed, we do not want to reprocess this record so we 
              // will move on to
              // the next record
            }

            MoveKsDriversLicense(entities.KsDriversLicense,
              local.KsDriversLicense);
            local.AlreadyProcessed.StandardNumber = "";
            ExitState = "ACO_NN0000_ALL_OK";

            if (ReadLegalAction())
            {
              UseOeDetermineDmvCriteria();

              if (!Lt(local.ZeroDate.Date, local.CourtesyLetterSentDt.Date))
              {
                goto ReadEach1;
              }

              if (AsChar(local.NextPerson.Flag) == 'Y')
              {
                goto ReadEach1;
              }

              if (AsChar(local.GoToNextCourtOrder.Flag) == 'Y')
              {
                continue;
              }

              UseOeDeterminePaymentsForKdmvP();

              if (AsChar(local.GoToNextCourtOrder.Flag) == 'Y')
              {
                continue;
              }

              UseFnComputeTotalsForCtOrder3();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                ++local.NumErrorRecords.Count;
                local.Error.Number = entities.CsePerson.Number;

                goto Test2;
              }

              local.OwedAmount.TotalCurrency =
                local.ScreenOwedAmountsDtl.TotalArrearsOwed + local
                .ScreenOwedAmountsDtl.TotalInterestOwed - (
                  local.ScreenOwedAmountsDtl.TotalArrearsColl + local
                .ScreenOwedAmountsDtl.TotalInterestColl + local
                .ScreenOwedAmountsDtl.TotalVoluntaryColl + local
                .ScreenOwedAmountsDtl.UndistributedAmt);

              if (local.OwedAmount.TotalCurrency >= local
                .MinimumTarget.TotalCurrency)
              {
                // keep processing
              }
              else
              {
                // go to the next court order
                continue;
              }

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                if (local.OwedAmount.TotalCurrency >= local
                  .MinimumTarget.TotalCurrency)
                {
                  // To see if any payments have been made that are still in 
                  // cash receipt and have
                  // not made to collections yet. There has not been enough 
                  // money in the collection
                  // table to meet the minium payment
                  if (!Equal(entities.CsePerson.Number,
                    local.AlreadyCheckedSsn.Number))
                  {
                    local.PersonTotal.TotalCurrency = 0;
                    local.Search.Flag = "3";
                    local.Phonetic.Percentage = 100;
                    local.StartCsePersonsWorkSet.Number =
                      entities.CsePerson.Number;
                    local.AlreadyCheckedSsn.Number = entities.CsePerson.Number;
                    MoveCsePersonsWorkSet1(local.Clear, local.CsePersonsWorkSet);
                      
                    UseSiReadCsePersonBatch();

                    if (!IsExitState("ACO_NN0000_ALL_OK"))
                    {
                      ++local.NumErrorRecords.Count;
                      local.Error.Number = local.StartCsePersonsWorkSet.Number;

                      goto Test2;
                    }

                    if (IsEmpty(local.CsePersonsWorkSet.Ssn) || Equal
                      (local.CsePersonsWorkSet.Ssn, "000000000") || !
                      Lt(new DateTime(1, 1, 1), local.CsePersonsWorkSet.Dob))
                    {
                      goto ReadEach1;
                    }

                    MoveLegalAction(entities.LegalAction, local.LegalAction);
                  }

                  if (!Equal(entities.CsePerson.Number,
                    local.LastCsePersonsWorkSet.Number))
                  {
                    local.PreviousProcess.Number =
                      local.LastCsePersonsWorkSet.Number;
                  }

                  if (!Equal(local.PreviousProcess.Number,
                    local.AlreadyWritten.Number) && IsExitState
                    ("ACO_NN0000_ALL_OK") && !
                    IsEmpty(local.PreviousProcess.Number))
                  {
                    // we are doing a checkpoint here because we know we have 
                    // completed an AP
                    // and now we are ready to start processing another AP but 
                    // before we do we want
                    // to do a checkpoint so we will not just partly save 
                    // changes to the current AP like
                    // we would have if we would wait until after we have done 
                    // the updates to the
                    // current record
                    if (local.Group30DayNotice.Count <= 0)
                    {
                      goto Test1;
                    }

                    // we will only be using this area to count obligors, when 
                    // we finish processing all
                    // court orders for a obligor and at at least one court 
                    // order is qualifying for a 30 day
                    // letter then we will count it here. once the limit of 
                    // obligor has been reached then
                    // we will stop processing and close out the program.
                    for(local.Group30DayNotice.Index = 0; local
                      .Group30DayNotice.Index < local.Group30DayNotice.Count; ++
                      local.Group30DayNotice.Index)
                    {
                      if (!local.Group30DayNotice.CheckSize())
                      {
                        break;
                      }

                      local.Group30DayNoticeSorted.Index =
                        local.Group30DayNotice.Index;
                      local.Group30DayNoticeSorted.CheckSize();

                      local.Group30DayNoticeSorted.Update.Grp30DayNoticeSorted.
                        StandardNumber =
                          local.Group30DayNotice.Item.Grp30DayNoticeLegalAction.
                          StandardNumber;
                    }

                    local.Group30DayNotice.CheckIndex();

                    // we have to sort the the standard number by ascending
                    if (local.Group30DayNoticeSorted.Count > 1)
                    {
                      local.Change.Flag = "T";

                      while(AsChar(local.Change.Flag) == 'T')
                      {
                        local.Change.Flag = "F";

                        local.Group30DayNoticeSorted.Index = 0;
                        local.Group30DayNoticeSorted.CheckSize();

                        while(local.Group30DayNoticeSorted.Index + 1 < local
                          .Group30DayNoticeSorted.Count)
                        {
                          local.Temp1.StandardNumber =
                            local.Group30DayNoticeSorted.Item.
                              Grp30DayNoticeSorted.StandardNumber;

                          ++local.Group30DayNoticeSorted.Index;
                          local.Group30DayNoticeSorted.CheckSize();

                          local.Temp2.StandardNumber =
                            local.Group30DayNoticeSorted.Item.
                              Grp30DayNoticeSorted.StandardNumber;

                          if (Lt(local.Temp2.StandardNumber,
                            local.Temp1.StandardNumber) && !
                            IsEmpty(local.Temp2.StandardNumber))
                          {
                            local.Change.Flag = "T";

                            --local.Group30DayNoticeSorted.Index;
                            local.Group30DayNoticeSorted.CheckSize();

                            local.Group30DayNoticeSorted.Update.
                              Grp30DayNoticeSorted.StandardNumber =
                                local.Temp2.StandardNumber ?? "";

                            ++local.Group30DayNoticeSorted.Index;
                            local.Group30DayNoticeSorted.CheckSize();

                            local.Group30DayNoticeSorted.Update.
                              Grp30DayNoticeSorted.StandardNumber =
                                local.Temp1.StandardNumber ?? "";
                          }
                        }
                      }
                    }

                    local.Highest.Number = "";

                    for(local.Group30DayNotice.Index = 0; local
                      .Group30DayNotice.Index < local.Group30DayNotice.Count; ++
                      local.Group30DayNotice.Index)
                    {
                      if (!local.Group30DayNotice.CheckSize())
                      {
                        break;
                      }

                      if (Lt(local.Highest.Number,
                        local.Group30DayNotice.Item.Grp30DayNoticeCase.Number))
                      {
                        local.Highest.Number =
                          local.Group30DayNotice.Item.Grp30DayNoticeCase.Number;
                          
                      }
                    }

                    local.Group30DayNotice.CheckIndex();

                    // do once per ap
                    // we only want to write out one record per obligor
                    // now we will write out the trigger so that the document 
                    // email sever (i.e. formerly
                    // known as the auto IWO server) will create the 30 day 
                    // notice letter and a email
                    // the collection officer.
                    // currently the key is the current obligor and the case 
                    // number in the local highest
                    // case number field
                    local.Document.Name = "DMV30DAY";
                    local.SpDocKey.KeyCase = local.Highest.Number;
                    local.SpDocKey.KeyAp = local.PreviousProcess.Number;
                    UseSpCreateDocumentInfrastruct();

                    if (!IsExitState("ACO_NN0000_ALL_OK"))
                    {
                      ++local.NumErrorRecords.Count;
                      local.Error.Number = local.PreviousProcess.Number;

                      goto ReadEach2;
                    }

                    for(local.Group30DayNoticeSorted.Index = 0; local
                      .Group30DayNoticeSorted.Index < local
                      .Group30DayNoticeSorted.Count; ++
                      local.Group30DayNoticeSorted.Index)
                    {
                      if (!local.Group30DayNoticeSorted.CheckSize())
                      {
                        break;
                      }

                      // do this for each cour order, the following
                      switch(local.Group30DayNoticeSorted.Index + 1)
                      {
                        case 1:
                          local.Field.Name = "DMVSTDNUM0";

                          break;
                        case 2:
                          local.Field.Name = "DMVSTDNUM1";

                          break;
                        case 3:
                          local.Field.Name = "DMVSTDNUM2";

                          break;
                        case 4:
                          local.Field.Name = "DMVSTDNUM3";

                          break;
                        case 5:
                          local.Field.Name = "DMVSTDNUM4";

                          break;
                        case 6:
                          local.Field.Name = "DMVSTDNUM5";

                          break;
                        case 7:
                          local.Field.Name = "DMVSTDNUM6";

                          break;
                        case 8:
                          local.Field.Name = "DMVSTDNUM7";

                          break;
                        case 9:
                          local.Field.Name = "DMVSTDNUM8";

                          break;
                        case 10:
                          local.Field.Name = "DMVSTDNUM9";

                          break;
                        default:
                          goto AfterCycle1;
                      }

                      local.FieldValue.Value =
                        local.Group30DayNoticeSorted.Item.Grp30DayNoticeSorted.
                          StandardNumber ?? "";
                      UseSpCabCreateUpdateFieldValue();

                      if (!IsExitState("ACO_NN0000_ALL_OK"))
                      {
                        ++local.NumErrorRecords.Count;
                        local.Error.Number = local.PreviousProcess.Number;

                        goto ReadEach2;
                      }
                    }

AfterCycle1:

                    local.Group30DayNoticeSorted.CheckIndex();
                    local.PersonSend.Flag = "";
                    local.AlreadyWritten.Number = local.PreviousProcess.Number;
                    local.LastSuccessfulProcessed.
                      Assign(local.CsePersonsWorkSet);
                    MoveLegalAction(local.LegalAction,
                      local.LastSuccesfulProcessed);
                    local.Group30DayNotice.Count = 0;
                    local.Group30DayNotice.Index = -1;
                    local.Highest.Number = "";
                    local.Temp1.StandardNumber = "";
                    local.Temp2.StandardNumber = "";
                    local.Group30DayNoticeSorted.Count = 0;
                    local.Group30DayNoticeSorted.Index = -1;

                    if (IsExitState("ACO_NN0000_ALL_OK"))
                    {
                      // ************************************************
                      // *Check the number of reads, and updates that   *
                      // *have occurred since the last checkpoint.      *
                      // ************************************************
                      ++local.ProgramRestart.Count;

                      if (local.ProgramRestart.Count >= local
                        .ProgramCheckpointRestart.UpdateFrequencyCount.
                          GetValueOrDefault())
                      {
                        local.ProgramCheckpointRestart.RestartInd = "Y";
                        local.ProgramCheckpointRestart.ProgramName =
                          local.ProgramProcessingInfo.Name;

                        // we have to take off one from the restart point since 
                        // it will add one in the update
                        // cab, if we did not take one off then it would let the
                        // miss processing some records
                        local.ProgramCheckpointRestart.CheckpointCount =
                          local.NumberOfRecordsRead.Count - 1;
                        local.TotalAdded.Text15 =
                          NumberToString(local.TotalNumLettersSent.Count, 15);
                        local.TotalErrors.Text15 =
                          NumberToString(local.TotalErrorRecords.Count, 15);
                        local.TotalRecordsUpdated.Text15 =
                          NumberToString(local.NumOfRecordsUpdated.Count, 15);
                        local.TotalRecordsRead.Text15 =
                          NumberToString(local.NumberOfRecordsRead.Count, 15);
                        local.SequenceCounter.Text15 =
                          NumberToString(local.ReadKsDriversLicense.
                            SequenceCounter, 15);
                        local.ProgramCheckpointRestart.RestartInfo =
                          local.TotalAdded.Text15 + local.TotalErrors.Text15 + local
                          .TotalRecordsUpdated.Text15 + local
                          .TotalRecordsRead.Text15 + local
                          .SequenceCounter.Text15;
                        UseUpdatePgmCheckpointRestart();
                        local.LastUpdatedTotalError.Count =
                          local.NumErrorRecords.Count;
                        local.LastUpdatedTotalAdded.Count =
                          local.TotalNumLettersSent.Count;
                        local.LastUpdatedNumRecsUpd.Count =
                          local.NumOfRecordsUpdated.Count;
                        local.LastUpdatedNumRecsRead.Count =
                          local.NumberOfRecordsRead.Count;
                        local.ProgramRestart.Count = 0;

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

                        local.EabFileHandling.Action = "WRITE";
                        local.EabReportSend.RptDetail = "Commit Taken: " + NumberToString
                          (TimeToInt(Time(Now())), 15);
                        UseCabErrorReport();

                        if (!Equal(local.EabFileHandling.Status, "OK"))
                        {
                          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                          return;
                        }
                      }
                    }

                    ++local.TotalNumLettersSent.Count;

                    if (local.TotalNumLettersSent.Count >= local
                      .MaxNumberObligors.Count)
                    {
                      // THE PROCESS HAS CREATED THE MAXMIUM NUMBER OF 30 DAY 
                      // NOTICES IT IS ALLOWED TO CREATE
                      local.PreviousProcess.Number = "";

                      goto ReadEach2;
                    }
                  }
                  else if (!Equal(entities.CsePerson.Number,
                    local.AlreadyWritten.Number) && IsExitState
                    ("ACO_NN0000_ALL_OK"))
                  {
                    local.LastSuccessfulProcessed.
                      Assign(local.CsePersonsWorkSet);
                    MoveLegalAction(local.LegalAction,
                      local.LastSuccesfulProcessed);
                  }

Test1:

                  local.Infrastructure.CaseNumber = "";
                  local.Infrastructure.CaseUnitNumber = 0;

                  foreach(var item2 in ReadCaseCaseUnitCaseRole())
                  {
                    // It really does not matter about wheater the case role is 
                    // active or not we are just
                    // concern with the highest cae nunber associated with the 
                    // cout order that is being
                    // worked - money is still owed therefore it is still in 
                    // play. So we are not going to
                    // check start and end dates
                    if (Lt(local.Infrastructure.CaseNumber,
                      entities.Case1.Number))
                    {
                      local.Infrastructure.CaseNumber = entities.Case1.Number;
                      local.Infrastructure.CaseUnitNumber =
                        entities.CaseUnit.CuNumber;
                    }
                  }

                  if (IsEmpty(local.Infrastructure.CaseNumber))
                  {
                    ExitState = "CASE_NF";
                    ++local.NumErrorRecords.Count;
                    local.Error.Number = entities.CsePerson.Number;

                    goto Test2;
                  }

                  // **********************************************************************************
                  // If we have gotten this far then we need to create a 
                  // courtesy record for the
                  // current court order and current obligor
                  // ********************************************************************************
                  if (Lt(local.ZeroDate.Date,
                    entities.SecondRead.Attribute30DayLetterCreatedDate))
                  {
                    // if 30 day letter create date > null date
                    if (Lt(entities.SecondRead.Attribute30DayLetterCreatedDate,
                      AddMonths(local.StartDate.Date, -
                      local.NumMnthBtw30DayLtr.Count)))
                    {
                      // if the 30 day letter create date < batch run date - 30 
                      // day letter time frame (2 years is default)
                      if (Lt(entities.SecondRead.CourtesyLetterSentDate,
                        AddDays(local.StartDate.Date, -
                        local.NumDaysSinceCurtsyLtr.Count)))
                      {
                        // since last 30 day letter was sent over the specified 
                        // time period for a 30 day letter and the courtesy
                        // letter was sent over 45 days ago we will send a new
                        // 30 day letter.
                        ++local.SequenceCount.Count;

                        try
                        {
                          CreateKsDriversLicense();
                          ++local.NumOfRecordsUpdated.Count;
                        }
                        catch(Exception e)
                        {
                          switch(GetErrorCode(e))
                          {
                            case ErrorCode.AlreadyExists:
                              ExitState = "KS_DRIVERS_LICENSE_AE";
                              ++local.NumErrorRecords.Count;
                              local.Error.Number = entities.CsePerson.Number;

                              goto Test2;
                            case ErrorCode.PermittedValueViolation:
                              ExitState = "KS_DRIVERS_LICENSE_PV";
                              ++local.NumErrorRecords.Count;
                              local.Error.Number = entities.CsePerson.Number;

                              goto Test2;
                            case ErrorCode.DatabaseError:
                              break;
                            default:
                              throw;
                          }
                        }

                        local.RecordProcessed.Flag = "Y";

                        if (!Equal(entities.CsePerson.Number,
                          local.LastCsePersonsWorkSet.Number))
                        {
                          local.LastCsePersonsWorkSet.Number =
                            entities.CsePerson.Number;
                        }

                        ++local.Group30DayNotice.Index;
                        local.Group30DayNotice.CheckSize();

                        local.Group30DayNotice.Update.Grp30DayNoticeCase.
                          Number = local.Infrastructure.CaseNumber ?? Spaces
                          (10);
                        local.Group30DayNotice.Update.Grp30DayNoticeCsePerson.
                          Number = entities.CsePerson.Number;
                        local.Group30DayNotice.Update.Grp30DayNoticeLegalAction.
                          StandardNumber = entities.LegalAction.StandardNumber;

                        // we will actually add a new record on the new table, 
                        // we do not want to over write
                        //  the old 30 day letter create date with so creating a
                        // record is a most, we will
                        // transfer over driver's license info and courtesy 
                        // letter info, everything else that
                        // follows after the courtesy letter will not be 
                        // transfered to the new record.
                      }
                      else
                      {
                        // the courtesy letter was sent less than 45 days ago so
                        // a 30 day letter can not be sent yet
                        goto ReadEach1;
                      }
                    }
                    else
                    {
                      // since the date is not past the minimum time period 
                      // between 30 day letters, we will
                      //  not send one but move on to the next person
                      goto ReadEach1;
                    }
                  }
                  else if (!Lt(AddDays(
                    local.StartDate.Date, -
                    local.NumDaysSinceCurtsyLtr.Count),
                    entities.SecondRead.CourtesyLetterSentDate))
                  {
                    // since no 30 day letter has been sent and the courtesy 
                    // letter was sent over 45 days ago we will send a 30 day
                    // letter.
                    // we will update the current new table record
                    if (ReadKsDriversLicense2())
                    {
                      try
                      {
                        UpdateKsDriversLicense3();
                        ++local.NumOfRecordsUpdated.Count;
                      }
                      catch(Exception e)
                      {
                        switch(GetErrorCode(e))
                        {
                          case ErrorCode.AlreadyExists:
                            ExitState = "KS_DRIVERS_LICENSE_NU";
                            ++local.NumErrorRecords.Count;
                            local.Error.Number = entities.CsePerson.Number;

                            goto Test2;
                          case ErrorCode.PermittedValueViolation:
                            ExitState = "KS_DRIVERS_LICENSE_PV";
                            ++local.NumErrorRecords.Count;
                            local.Error.Number = entities.CsePerson.Number;

                            goto Test2;
                          case ErrorCode.DatabaseError:
                            break;
                          default:
                            throw;
                        }
                      }
                    }
                    else
                    {
                      ExitState = "KS_DRIVERS_LICENSE_NF";
                      ++local.NumErrorRecords.Count;
                      local.Error.Number = entities.CsePerson.Number;

                      goto Test2;
                    }

                    local.RecordProcessed.Flag = "Y";

                    if (!Equal(entities.CsePerson.Number,
                      local.LastCsePersonsWorkSet.Number))
                    {
                      local.LastCsePersonsWorkSet.Number =
                        entities.CsePerson.Number;
                    }

                    ++local.Group30DayNotice.Index;
                    local.Group30DayNotice.CheckSize();

                    local.Group30DayNotice.Update.Grp30DayNoticeCase.Number =
                      local.Infrastructure.CaseNumber ?? Spaces(10);
                    local.Group30DayNotice.Update.Grp30DayNoticeCsePerson.
                      Number = entities.CsePerson.Number;
                    local.Group30DayNotice.Update.Grp30DayNoticeLegalAction.
                      StandardNumber = entities.LegalAction.StandardNumber;
                  }
                  else
                  {
                    // the courtesy letter was sent less than 45 days ago so a 
                    // 30 day letter can not be sent yet
                    goto ReadEach1;
                  }

                  if (AsChar(local.RecordProcessed.Flag) == 'Y')
                  {
                    local.RecordProcessed.Flag = "";
                    local.FinanceWorkAttributes.NumericalDollarValue =
                      local.OwedAmount.TotalCurrency;
                    UseFnCabReturnTextDollars();
                    local.Infrastructure.SituationNumber = 0;
                    local.Infrastructure.ReasonCode = "DMV30DAYNOTICE";
                    local.Infrastructure.EventId = 1;
                    local.Infrastructure.EventType = "ADMINACT";
                    local.Infrastructure.ProcessStatus = "Q";
                    local.Infrastructure.UserId = "KDMV";
                    local.Infrastructure.BusinessObjectCd = "ENF";
                    local.Infrastructure.ReferenceDate = local.StartDate.Date;
                    local.Infrastructure.CreatedBy =
                      local.ProgramProcessingInfo.Name;
                    local.Infrastructure.EventDetailName = "DMV 30 Day Notice";
                    local.Infrastructure.CsePersonNumber =
                      entities.CsePerson.Number;
                    local.Infrastructure.DenormNumeric12 =
                      entities.LegalAction.Identifier;
                    local.Infrastructure.DenormText12 =
                      entities.LegalAction.CourtCaseNumber;
                    local.Detail.Text11 = ", Arrears $";
                    local.Infrastructure.Detail =
                      "30 Day notice sent, Ct Order # " + TrimEnd
                      (entities.LegalAction.StandardNumber) + local
                      .Detail.Text11 + local
                      .FinanceWorkAttributes.TextDollarValue;

                    // CDVALUE  	DESCRIPTION
                    // ---+------	---+---------+---------+---------+---------+
                    // ---------+---------+---
                    // ADMIN    	ADMINISTRATIVE/EXEMPTIONS
                    // ADMINACT 	ADMINISTRATIVE ACTION
                    // ADMINAPL 	ADMINSTRATIVE APPEAL
                    // APDTL    	ABSENT PARENT DETAILS
                    // APSTMT   	AP STATEMENT
                    // ARCHIVE  	ARCHIVE/RETRIEVAL (DOCUMENTS)
                    // BKRP     	BANKRUPTCY ACTIVITIES
                    // CASE     	CASE (AE ACTIVITIES, OPENINGS/CLOSINGS, 
                    // APPOINTMENTS)
                    // CASEROLE 	CASE ROLE (FAMILY VIOLENCE, ROLE CHANGES, GOOD 
                    // CAUSE, SSN, PAT)
                    // CASEUNIT 	CASE UNIT (LIFECYCLE, OBLIGATIONS PAID/
                    // ACTIVATED/REACTIVATED)
                    // COMPLIANCE	OBLIGATIONS/FINANCE ACTIVITIES, PARENTAL 
                    // RIGHTS/EMANCIPATION
                    // CSENET   	CSENET, QUICK LOCATE
                    // DATEMON  	DATE MONITOR (FUTURE DATES - EMANCIPATION/
                    // DISCHARGE/RELEASE)
                    // DOCUMENT 	DOCUMENT (GENTEST, LOCATE, INSURANCE, 
                    // MODIFICATION, JE)
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
                    UseSpCabCreateInfrastructure();

                    if (!IsExitState("ACO_NN0000_ALL_OK"))
                    {
                      ++local.NumErrorRecords.Count;
                      local.Error.Number = entities.CsePerson.Number;

                      goto Test2;
                    }

                    export.TotalAmtDebtOwed.TotalCurrency += local.OwedAmount.
                      TotalCurrency;
                    local.PersonTotal.TotalCurrency += local.OwedAmount.
                      TotalCurrency;
                    local.PersonSend.Flag = "Y";
                  }
                }
                else
                {
                  continue;
                }
              }
            }
          }
        }

Test2:

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabExtractExitStateMessage();
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "Record failed because: " + TrimEnd
            (local.ExitStateWorkArea.Message) + " CSE Person # " + local
            .Error.Number;
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          ++local.TotalErrorRecords.Count;
        }
      }
      else
      {
        continue;
      }

ReadEach1:
      ;
    }

ReadEach2:

    if (AsChar(local.PersonSend.Flag) == 'Y' && IsExitState
      ("ACO_NN0000_ALL_OK"))
    {
      // this will pick up the last record since it did not have a document 
      // generated for it
      if (local.Group30DayNotice.Count <= 0)
      {
        goto Test3;
      }

      for(local.Group30DayNotice.Index = 0; local.Group30DayNotice.Index < local
        .Group30DayNotice.Count; ++local.Group30DayNotice.Index)
      {
        if (!local.Group30DayNotice.CheckSize())
        {
          break;
        }

        local.Group30DayNoticeSorted.Index = local.Group30DayNotice.Index;
        local.Group30DayNoticeSorted.CheckSize();

        local.Group30DayNoticeSorted.Update.Grp30DayNoticeSorted.
          StandardNumber =
            local.Group30DayNotice.Item.Grp30DayNoticeLegalAction.
            StandardNumber;
      }

      local.Group30DayNotice.CheckIndex();

      // we have to sort the the standard number by ascending
      if (local.Group30DayNoticeSorted.Count > 1)
      {
        local.Change.Flag = "T";

        while(AsChar(local.Change.Flag) == 'T')
        {
          local.Change.Flag = "F";

          local.Group30DayNoticeSorted.Index = 0;
          local.Group30DayNoticeSorted.CheckSize();

          while(local.Group30DayNoticeSorted.Index + 1 < local
            .Group30DayNoticeSorted.Count)
          {
            local.Temp1.StandardNumber =
              local.Group30DayNoticeSorted.Item.Grp30DayNoticeSorted.
                StandardNumber;

            ++local.Group30DayNoticeSorted.Index;
            local.Group30DayNoticeSorted.CheckSize();

            local.Temp2.StandardNumber =
              local.Group30DayNoticeSorted.Item.Grp30DayNoticeSorted.
                StandardNumber;

            if (Lt(local.Temp2.StandardNumber, local.Temp1.StandardNumber) && !
              IsEmpty(local.Temp2.StandardNumber))
            {
              local.Change.Flag = "T";

              --local.Group30DayNoticeSorted.Index;
              local.Group30DayNoticeSorted.CheckSize();

              local.Group30DayNoticeSorted.Update.Grp30DayNoticeSorted.
                StandardNumber = local.Temp2.StandardNumber ?? "";

              ++local.Group30DayNoticeSorted.Index;
              local.Group30DayNoticeSorted.CheckSize();

              local.Group30DayNoticeSorted.Update.Grp30DayNoticeSorted.
                StandardNumber = local.Temp1.StandardNumber ?? "";
            }
          }
        }
      }

      for(local.Group30DayNotice.Index = 0; local.Group30DayNotice.Index < local
        .Group30DayNotice.Count; ++local.Group30DayNotice.Index)
      {
        if (!local.Group30DayNotice.CheckSize())
        {
          break;
        }

        if (Lt(local.Highest.Number,
          local.Group30DayNotice.Item.Grp30DayNoticeCase.Number))
        {
          local.Highest.Number =
            local.Group30DayNotice.Item.Grp30DayNoticeCase.Number;
          local.SpDocKey.KeyAp =
            local.Group30DayNotice.Item.Grp30DayNoticeCsePerson.Number;
        }
      }

      local.Group30DayNotice.CheckIndex();
      ++local.TotalNumLettersSent.Count;

      // now we will write out the trigger so that the document email sever (i.
      // e. formerly
      // known as the auto IWO server) will create the 30 day notice letter and 
      // a email
      // the collection officer.
      // currently the key is the current obligor and the case number in the 
      // local highest
      // case number field
      for(local.Group30DayNotice.Index = 0; local.Group30DayNotice.Index < local
        .Group30DayNotice.Count; ++local.Group30DayNotice.Index)
      {
        if (!local.Group30DayNotice.CheckSize())
        {
          break;
        }

        local.Temp.Number =
          local.Group30DayNotice.Item.Grp30DayNoticeCsePerson.Number;

        break;
      }

      local.Group30DayNotice.CheckIndex();
      local.Document.Name = "DMV30DAY";
      local.SpDocKey.KeyCase = local.Highest.Number;
      local.SpDocKey.KeyAp = local.Temp.Number;
      UseSpCreateDocumentInfrastruct();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        ++local.NumErrorRecords.Count;

        goto Test3;
      }

      for(local.Group30DayNoticeSorted.Index = 0; local
        .Group30DayNoticeSorted.Index < local.Group30DayNoticeSorted.Count; ++
        local.Group30DayNoticeSorted.Index)
      {
        if (!local.Group30DayNoticeSorted.CheckSize())
        {
          break;
        }

        // do this for each cour order, the following
        switch(local.Group30DayNoticeSorted.Index + 1)
        {
          case 1:
            local.Field.Name = "DMVSTDNUM0";

            break;
          case 2:
            local.Field.Name = "DMVSTDNUM1";

            break;
          case 3:
            local.Field.Name = "DMVSTDNUM2";

            break;
          case 4:
            local.Field.Name = "DMVSTDNUM3";

            break;
          case 5:
            local.Field.Name = "DMVSTDNUM4";

            break;
          case 6:
            local.Field.Name = "DMVSTDNUM5";

            break;
          case 7:
            local.Field.Name = "DMVSTDNUM6";

            break;
          case 8:
            local.Field.Name = "DMVSTDNUM7";

            break;
          case 9:
            local.Field.Name = "DMVSTDNUM8";

            break;
          case 10:
            local.Field.Name = "DMVSTDNUM9";

            break;
          default:
            goto AfterCycle2;
        }

        local.FieldValue.Value =
          local.Group30DayNoticeSorted.Item.Grp30DayNoticeSorted.
            StandardNumber ?? "";
        UseSpCabCreateUpdateFieldValue();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          ++local.NumErrorRecords.Count;

          goto Test3;
        }
      }

AfterCycle2:

      local.Group30DayNoticeSorted.CheckIndex();
    }

Test3:

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseOeB458Close1();
      local.ProgramCheckpointRestart.CheckpointCount = -1;
      local.ProgramCheckpointRestart.RestartInd = "N";
      local.ProgramCheckpointRestart.ProgramName =
        local.ProgramProcessingInfo.Name;
      local.ProgramCheckpointRestart.RestartInfo = "";
      UseUpdatePgmCheckpointRestart();

      foreach(var item in ReadKsDriversLicense7())
      {
        // we need to clear the  processing ind for next time, since the 
        // indicator only refers to the current run.
        try
        {
          UpdateKsDriversLicense2();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              break;
            case ErrorCode.PermittedValueViolation:
              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
    }
    else
    {
      UseEabExtractExitStateMessage();
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Record failed because: " + TrimEnd
        (local.ExitStateWorkArea.Message) + " CSE Person # " + local
        .Error.Number;
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      UseOeB458Close2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveCsePersonsWorkSet1(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.ReplicationIndicator = source.ReplicationIndicator;
    target.Number = source.Number;
    target.Sex = source.Sex;
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.LastName = source.LastName;
  }

  private static void MoveCsePersonsWorkSet2(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Sex = source.Sex;
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.FormattedName = source.FormattedName;
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

  private static void MoveKsDriversLicense(KsDriversLicense source,
    KsDriversLicense target)
  {
    target.KsDvrLicense = source.KsDvrLicense;
    target.ValidationDate = source.ValidationDate;
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveScreenOwedAmountsDtl(ScreenOwedAmountsDtl source,
    ScreenOwedAmountsDtl target)
  {
    target.TotalArrearsOwed = source.TotalArrearsOwed;
    target.TotalInterestOwed = source.TotalInterestOwed;
  }

  private static void MoveSpDocKey(SpDocKey source, SpDocKey target)
  {
    target.KeyAp = source.KeyAp;
    target.KeyCase = source.KeyCase;
    target.KeyPerson = source.KeyPerson;
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

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
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

  private void UseFnComputeTotalsForCtOrder3()
  {
    var useImport = new FnComputeTotalsForCtOrder3.Import();
    var useExport = new FnComputeTotalsForCtOrder3.Export();

    useImport.Obligor.Number = entities.CsePerson.Number;
    useImport.FilterByStdNo.StandardNumber =
      entities.LegalAction.StandardNumber;
    useImport.IncludeArrearsOnly.Flag = local.ArrearsOnly.Flag;
    useImport.StartDate.Date = local.StartDate.Date;

    Call(FnComputeTotalsForCtOrder3.Execute, useImport, useExport);

    MoveScreenOwedAmountsDtl(useExport.ScreenOwedAmountsDtl,
      local.ScreenOwedAmountsDtl);
  }

  private void UseOeB458Close1()
  {
    var useImport = new OeB458Close.Import();
    var useExport = new OeB458Close.Export();

    useImport.NumOfRecordsUpdated.Count = local.NumOfRecordsUpdated.Count;
    useImport.MaxNumberOfAps.Count = local.MaxNumberObligors.Count;
    useImport.NumberOfRecordsRead.Count = local.NumberOfRecordsRead.Count;
    useImport.TotalNumLettesSent.Count = local.TotalNumLettersSent.Count;
    useImport.NumErrorRecords.Count = local.NumErrorRecords.Count;

    Call(OeB458Close.Execute, useImport, useExport);
  }

  private void UseOeB458Close2()
  {
    var useImport = new OeB458Close.Import();
    var useExport = new OeB458Close.Export();

    useImport.NumOfRecordsUpdated.Count = local.LastUpdatedNumRecsUpd.Count;
    useImport.MaxNumberOfAps.Count = local.MaxNumberObligors.Count;
    useImport.NumberOfRecordsRead.Count = local.LastUpdatedNumRecsRead.Count;
    useImport.TotalNumLettesSent.Count = local.LastUpdatedTotalAdded.Count;
    useImport.NumErrorRecords.Count = local.LastUpdatedTotalError.Count;

    Call(OeB458Close.Execute, useImport, useExport);
  }

  private void UseOeB458Housekeeping()
  {
    var useImport = new OeB458Housekeeping.Import();
    var useExport = new OeB458Housekeeping.Export();

    Call(OeB458Housekeeping.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseOeDetermineDmvCriteria()
  {
    var useImport = new OeDetermineDmvCriteria.Import();
    var useExport = new OeDetermineDmvCriteria.Export();

    useImport.CsePerson.Number = entities.CsePerson.Number;
    useImport.LegalAction.StandardNumber = entities.LegalAction.StandardNumber;
    useImport.AdministrativeAction.Type1 = local.AdministrativeAction.Type1;
    useImport.StartDate.Date = local.StartDate.Date;
    useImport.ValidPeriodCourtesyLtr.Date = local.CourtesyLetterCompare.Date;
    useImport.ValidPeriod30DayNotic.Date = local.ValidPeriod30DayNotice.Date;

    Call(OeDetermineDmvCriteria.Execute, useImport, useExport);

    local.GoToNextCourtOrder.Flag = useExport.GoToNextCourtOrder.Flag;
    local.NextPerson.Flag = useExport.NextPerson.Flag;
    local.CourtesyLetterSentDt.Date = useExport.CrtLetterSentDate.Date;
  }

  private void UseOeDeterminePaymentsForKdmvP()
  {
    var useImport = new OeDeterminePaymentsForKdmvP.Import();
    var useExport = new OeDeterminePaymentsForKdmvP.Export();

    useImport.CsePerson.Number = entities.CsePerson.Number;
    useImport.LegalAction.StandardNumber = entities.LegalAction.StandardNumber;
    useImport.LocalMinumPayment.TotalCurrency =
      local.MinimumPayment.TotalCurrency;
    useImport.Start.Date = local.StartDate.Date;
    useImport.NumberOfDays.Count = local.PaymentPeriodDays.Count;

    Call(OeDeterminePaymentsForKdmvP.Execute, useImport, useExport);

    local.GoToNextCourtOrder.Flag = useExport.NextCourtOrder.Flag;
  }

  private void UseSiReadCsePersonBatch()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.StartCsePersonsWorkSet.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

    MoveCsePersonsWorkSet2(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
      
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    MoveInfrastructure(useExport.Infrastructure, local.Infrastructure);
  }

  private void UseSpCabCreateUpdateFieldValue()
  {
    var useImport = new SpCabCreateUpdateFieldValue.Import();
    var useExport = new SpCabCreateUpdateFieldValue.Export();

    useImport.Infrastructure.SystemGeneratedIdentifier =
      local.DocNew.SystemGeneratedIdentifier;
    useImport.Field.Name = local.Field.Name;
    useImport.FieldValue.Assign(local.FieldValue);

    Call(SpCabCreateUpdateFieldValue.Execute, useImport, useExport);
  }

  private void UseSpCreateDocumentInfrastruct()
  {
    var useImport = new SpCreateDocumentInfrastruct.Import();
    var useExport = new SpCreateDocumentInfrastruct.Export();

    useImport.Document.Assign(local.Document);
    MoveSpDocKey(local.SpDocKey, useImport.SpDocKey);

    Call(SpCreateDocumentInfrastruct.Execute, useImport, useExport);

    local.DocNew.SystemGeneratedIdentifier =
      useExport.Infrastructure.SystemGeneratedIdentifier;
  }

  private void UseUpdatePgmCheckpointRestart()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }

  private void CreateKsDriversLicense()
  {
    var createdBy = global.UserId;
    var createdTstamp = Now();
    var cspNum = entities.CsePerson.Number;
    var lgaIdentifier = entities.LegalAction.Identifier;
    var ksDvrLicense = entities.SecondRead.KsDvrLicense;
    var validationDate = entities.SecondRead.ValidationDate;
    var courtesyLetterSentDate = entities.SecondRead.CourtesyLetterSentDate;
    var attribute30DayLetterCreatedDate = local.StartDate.Date;
    var sequenceCounter = local.SequenceCount.Count;

    entities.Create.Populated = false;
    Update("CreateKsDriversLicense",
      (db, command) =>
      {
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetString(command, "cspNum", cspNum);
        db.SetNullableInt32(command, "lgaIdentifier", lgaIdentifier);
        db.SetNullableString(command, "ksDvrLicense", ksDvrLicense);
        db.SetNullableDate(command, "validationDate", validationDate);
        db.SetNullableDate(command, "courtesyLtrDate", courtesyLetterSentDate);
        db.SetNullableDate(
          command, "ltr30DayDate", attribute30DayLetterCreatedDate);
        db.SetNullableString(command, "servCompleteInd", "");
        db.SetNullableDate(command, "servCompleteDt", default(DateTime));
        db.SetNullableString(command, "restrictionStatus", "");
        db.SetNullableDecimal(command, "amountOwed", 0M);
        db.SetNullableString(command, "lastUpdatedBy", createdBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", createdTstamp);
        db.SetNullableString(command, "note1", "");
        db.SetNullableString(command, "processedInd", "");
        db.SetInt32(command, "sequenceCounter", sequenceCounter);
      });

    entities.Create.CreatedBy = createdBy;
    entities.Create.CreatedTstamp = createdTstamp;
    entities.Create.CspNum = cspNum;
    entities.Create.LgaIdentifier = lgaIdentifier;
    entities.Create.KsDvrLicense = ksDvrLicense;
    entities.Create.ValidationDate = validationDate;
    entities.Create.CourtesyLetterSentDate = courtesyLetterSentDate;
    entities.Create.Attribute30DayLetterCreatedDate =
      attribute30DayLetterCreatedDate;
    entities.Create.LastUpdatedBy = createdBy;
    entities.Create.LastUpdatedTstamp = createdTstamp;
    entities.Create.ProcessedInd = "";
    entities.Create.SequenceCounter = sequenceCounter;
    entities.Create.Populated = true;
  }

  private IEnumerable<bool> ReadCaseCaseUnitCaseRole()
  {
    entities.CaseRole.Populated = false;
    entities.Case1.Populated = false;
    entities.CaseUnit.Populated = false;

    return ReadEach("ReadCaseCaseUnitCaseRole",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetInt32(command, "lgaId", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseUnit.CasNo = db.GetString(reader, 0);
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 2);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 3);
        entities.CaseRole.CspNumber = db.GetString(reader, 4);
        entities.CaseRole.Type1 = db.GetString(reader, 5);
        entities.CaseRole.Identifier = db.GetInt32(reader, 6);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 7);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 8);
        entities.CaseRole.Populated = true;
        entities.Case1.Populated = true;
        entities.CaseUnit.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);

        return true;
      });
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.
      Assert(entities.ReadOnlyKsDriversLicense.Populated);
    entities.ReadOnlyCsePerson2.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.ReadOnlyKsDriversLicense.CspNum);
      },
      (db, reader) =>
      {
        entities.ReadOnlyCsePerson2.Number = db.GetString(reader, 0);
        entities.ReadOnlyCsePerson2.Populated = true;
      });
  }

  private bool ReadKsDriversLicense1()
  {
    return Read("ReadKsDriversLicense1",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTstamp",
          local.BatchTimestampWorkArea.IefTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        local.ReadCounter.Count = db.GetInt32(reader, 0);
      });
  }

  private bool ReadKsDriversLicense2()
  {
    entities.Create.Populated = false;

    return Read("ReadKsDriversLicense2",
      (db, command) =>
      {
        db.SetInt32(
          command, "sequenceCounter", entities.SecondRead.SequenceCounter);
        db.SetString(command, "cspNum", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Create.CreatedBy = db.GetString(reader, 0);
        entities.Create.CreatedTstamp = db.GetDateTime(reader, 1);
        entities.Create.CspNum = db.GetString(reader, 2);
        entities.Create.LgaIdentifier = db.GetNullableInt32(reader, 3);
        entities.Create.KsDvrLicense = db.GetNullableString(reader, 4);
        entities.Create.ValidationDate = db.GetNullableDate(reader, 5);
        entities.Create.CourtesyLetterSentDate = db.GetNullableDate(reader, 6);
        entities.Create.Attribute30DayLetterCreatedDate =
          db.GetNullableDate(reader, 7);
        entities.Create.LastUpdatedBy = db.GetNullableString(reader, 8);
        entities.Create.LastUpdatedTstamp = db.GetNullableDateTime(reader, 9);
        entities.Create.ProcessedInd = db.GetNullableString(reader, 10);
        entities.Create.SequenceCounter = db.GetInt32(reader, 11);
        entities.Create.Populated = true;
      });
  }

  private bool ReadKsDriversLicense3()
  {
    entities.ProcessedCheck.Populated = false;

    return Read("ReadKsDriversLicense3",
      (db, command) =>
      {
        db.SetString(command, "cspNum", local.ReadCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ProcessedCheck.CreatedBy = db.GetString(reader, 0);
        entities.ProcessedCheck.CreatedTstamp = db.GetDateTime(reader, 1);
        entities.ProcessedCheck.CspNum = db.GetString(reader, 2);
        entities.ProcessedCheck.KsDvrLicense = db.GetNullableString(reader, 3);
        entities.ProcessedCheck.ValidationDate = db.GetNullableDate(reader, 4);
        entities.ProcessedCheck.CourtesyLetterSentDate =
          db.GetNullableDate(reader, 5);
        entities.ProcessedCheck.Attribute30DayLetterCreatedDate =
          db.GetNullableDate(reader, 6);
        entities.ProcessedCheck.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.ProcessedCheck.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 8);
        entities.ProcessedCheck.ProcessedInd = db.GetNullableString(reader, 9);
        entities.ProcessedCheck.SequenceCounter = db.GetInt32(reader, 10);
        entities.ProcessedCheck.Populated = true;
      });
  }

  private bool ReadKsDriversLicense4()
  {
    entities.ProcessedCheck.Populated = false;

    return Read("ReadKsDriversLicense4",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTstamp",
          local.BatchTimestampWorkArea.IefTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ProcessedCheck.CreatedBy = db.GetString(reader, 0);
        entities.ProcessedCheck.CreatedTstamp = db.GetDateTime(reader, 1);
        entities.ProcessedCheck.CspNum = db.GetString(reader, 2);
        entities.ProcessedCheck.KsDvrLicense = db.GetNullableString(reader, 3);
        entities.ProcessedCheck.ValidationDate = db.GetNullableDate(reader, 4);
        entities.ProcessedCheck.CourtesyLetterSentDate =
          db.GetNullableDate(reader, 5);
        entities.ProcessedCheck.Attribute30DayLetterCreatedDate =
          db.GetNullableDate(reader, 6);
        entities.ProcessedCheck.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.ProcessedCheck.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 8);
        entities.ProcessedCheck.ProcessedInd = db.GetNullableString(reader, 9);
        entities.ProcessedCheck.SequenceCounter = db.GetInt32(reader, 10);
        entities.ProcessedCheck.Populated = true;
      });
  }

  private IEnumerable<bool> ReadKsDriversLicense5()
  {
    entities.ReadOnlyKsDriversLicense.Populated = false;

    return ReadEach("ReadKsDriversLicense5",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "validationDate",
          local.LastLicenseProcessedDt.Date.GetValueOrDefault());
        db.SetInt32(
          command, "sequenceCounter",
          local.ReadKsDriversLicense.SequenceCounter);
      },
      (db, reader) =>
      {
        entities.ReadOnlyKsDriversLicense.CreatedTstamp =
          db.GetDateTime(reader, 0);
        entities.ReadOnlyKsDriversLicense.CspNum = db.GetString(reader, 1);
        entities.ReadOnlyKsDriversLicense.ValidationDate =
          db.GetNullableDate(reader, 2);
        entities.ReadOnlyKsDriversLicense.CourtesyLetterSentDate =
          db.GetNullableDate(reader, 3);
        entities.ReadOnlyKsDriversLicense.Attribute30DayLetterCreatedDate =
          db.GetNullableDate(reader, 4);
        entities.ReadOnlyKsDriversLicense.ProcessedInd =
          db.GetNullableString(reader, 5);
        entities.ReadOnlyKsDriversLicense.SequenceCounter =
          db.GetInt32(reader, 6);
        entities.ReadOnlyKsDriversLicense.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadKsDriversLicense6()
  {
    entities.SecondRead.Populated = false;

    return ReadEach("ReadKsDriversLicense6",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "courtesyLtrDate", local.ZeroDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "validationDate",
          local.LastLicenseProcessedDt.Date.GetValueOrDefault());
        db.SetString(command, "cspNum", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.SecondRead.CreatedTstamp = db.GetDateTime(reader, 0);
        entities.SecondRead.CspNum = db.GetString(reader, 1);
        entities.SecondRead.LgaIdentifier = db.GetNullableInt32(reader, 2);
        entities.SecondRead.KsDvrLicense = db.GetNullableString(reader, 3);
        entities.SecondRead.ValidationDate = db.GetNullableDate(reader, 4);
        entities.SecondRead.CourtesyLetterSentDate =
          db.GetNullableDate(reader, 5);
        entities.SecondRead.Attribute30DayLetterCreatedDate =
          db.GetNullableDate(reader, 6);
        entities.SecondRead.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.SecondRead.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 8);
        entities.SecondRead.SequenceCounter = db.GetInt32(reader, 9);
        entities.SecondRead.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadKsDriversLicense7()
  {
    entities.KsDriversLicense.Populated = false;

    return ReadEach("ReadKsDriversLicense7",
      null,
      (db, reader) =>
      {
        entities.KsDriversLicense.CreatedBy = db.GetString(reader, 0);
        entities.KsDriversLicense.CreatedTstamp = db.GetDateTime(reader, 1);
        entities.KsDriversLicense.CspNum = db.GetString(reader, 2);
        entities.KsDriversLicense.KsDvrLicense =
          db.GetNullableString(reader, 3);
        entities.KsDriversLicense.ValidationDate =
          db.GetNullableDate(reader, 4);
        entities.KsDriversLicense.CourtesyLetterSentDate =
          db.GetNullableDate(reader, 5);
        entities.KsDriversLicense.Attribute30DayLetterCreatedDate =
          db.GetNullableDate(reader, 6);
        entities.KsDriversLicense.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.KsDriversLicense.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 8);
        entities.KsDriversLicense.ProcessedInd =
          db.GetNullableString(reader, 9);
        entities.KsDriversLicense.SequenceCounter = db.GetInt32(reader, 10);
        entities.KsDriversLicense.Populated = true;

        return true;
      });
  }

  private bool ReadKsDriversLicenseCsePerson()
  {
    entities.KsDriversLicense.Populated = false;
    entities.CsePerson.Populated = false;

    return Read("ReadKsDriversLicenseCsePerson",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "validationDate",
          local.LastLicenseProcessedDt.Date.GetValueOrDefault());
        db.SetString(command, "numb", local.ReadCsePerson.Number);
        db.SetNullableDate(
          command, "dateOfDeath", local.ZeroDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "startDate", local.StartDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.KsDriversLicense.CreatedBy = db.GetString(reader, 0);
        entities.KsDriversLicense.CreatedTstamp = db.GetDateTime(reader, 1);
        entities.KsDriversLicense.CspNum = db.GetString(reader, 2);
        entities.CsePerson.Number = db.GetString(reader, 2);
        entities.KsDriversLicense.KsDvrLicense =
          db.GetNullableString(reader, 3);
        entities.KsDriversLicense.ValidationDate =
          db.GetNullableDate(reader, 4);
        entities.KsDriversLicense.CourtesyLetterSentDate =
          db.GetNullableDate(reader, 5);
        entities.KsDriversLicense.Attribute30DayLetterCreatedDate =
          db.GetNullableDate(reader, 6);
        entities.KsDriversLicense.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.KsDriversLicense.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 8);
        entities.KsDriversLicense.ProcessedInd =
          db.GetNullableString(reader, 9);
        entities.KsDriversLicense.SequenceCounter = db.GetInt32(reader, 10);
        entities.CsePerson.Type1 = db.GetString(reader, 11);
        entities.CsePerson.DateOfDeath = db.GetNullableDate(reader, 12);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 13);
        entities.KsDriversLicense.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadLegalAction()
  {
    System.Diagnostics.Debug.Assert(entities.SecondRead.Populated);
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          entities.SecondRead.LgaIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 1);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.Populated = true;
      });
  }

  private void UpdateKsDriversLicense1()
  {
    System.Diagnostics.Debug.Assert(entities.KsDriversLicense.Populated);

    var processedInd = "Y";

    entities.KsDriversLicense.Populated = false;
    Update("UpdateKsDriversLicense1",
      (db, command) =>
      {
        db.SetNullableString(command, "processedInd", processedInd);
        db.SetString(command, "cspNum", entities.KsDriversLicense.CspNum);
        db.SetInt32(
          command, "sequenceCounter",
          entities.KsDriversLicense.SequenceCounter);
      });

    entities.KsDriversLicense.ProcessedInd = processedInd;
    entities.KsDriversLicense.Populated = true;
  }

  private void UpdateKsDriversLicense2()
  {
    System.Diagnostics.Debug.Assert(entities.KsDriversLicense.Populated);
    entities.KsDriversLicense.Populated = false;
    Update("UpdateKsDriversLicense2",
      (db, command) =>
      {
        db.SetNullableString(command, "processedInd", "");
        db.SetString(command, "cspNum", entities.KsDriversLicense.CspNum);
        db.SetInt32(
          command, "sequenceCounter",
          entities.KsDriversLicense.SequenceCounter);
      });

    entities.KsDriversLicense.ProcessedInd = "";
    entities.KsDriversLicense.Populated = true;
  }

  private void UpdateKsDriversLicense3()
  {
    System.Diagnostics.Debug.Assert(entities.Create.Populated);

    var attribute30DayLetterCreatedDate = local.StartDate.Date;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTstamp = Now();

    entities.Create.Populated = false;
    Update("UpdateKsDriversLicense3",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "ltr30DayDate", attribute30DayLetterCreatedDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetString(command, "cspNum", entities.Create.CspNum);
        db.
          SetInt32(command, "sequenceCounter", entities.Create.SequenceCounter);
          
      });

    entities.Create.Attribute30DayLetterCreatedDate =
      attribute30DayLetterCreatedDate;
    entities.Create.LastUpdatedBy = lastUpdatedBy;
    entities.Create.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.Create.Populated = true;
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
    /// A value of RecordProcessed.
    /// </summary>
    [JsonPropertyName("recordProcessed")]
    public Common RecordProcessed
    {
      get => recordProcessed ??= new();
      set => recordProcessed = value;
    }

    private Common recordProcessed;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of RecordProcessed.
    /// </summary>
    [JsonPropertyName("recordProcessed")]
    public Common RecordProcessed
    {
      get => recordProcessed ??= new();
      set => recordProcessed = value;
    }

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
    /// A value of TotalNumRecsAdded.
    /// </summary>
    [JsonPropertyName("totalNumRecsAdded")]
    public Common TotalNumRecsAdded
    {
      get => totalNumRecsAdded ??= new();
      set => totalNumRecsAdded = value;
    }

    private Common recordProcessed;
    private Common totalAmtDebtOwed;
    private Common totalNumRecsAdded;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A Group30DayNoticeSortedGroup group.</summary>
    [Serializable]
    public class Group30DayNoticeSortedGroup
    {
      /// <summary>
      /// A value of Grp30DayNoticeSorted.
      /// </summary>
      [JsonPropertyName("grp30DayNoticeSorted")]
      public LegalAction Grp30DayNoticeSorted
      {
        get => grp30DayNoticeSorted ??= new();
        set => grp30DayNoticeSorted = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private LegalAction grp30DayNoticeSorted;
    }

    /// <summary>A Group30DayNoticeGroup group.</summary>
    [Serializable]
    public class Group30DayNoticeGroup
    {
      /// <summary>
      /// A value of Grp30DayNoticeLegalAction.
      /// </summary>
      [JsonPropertyName("grp30DayNoticeLegalAction")]
      public LegalAction Grp30DayNoticeLegalAction
      {
        get => grp30DayNoticeLegalAction ??= new();
        set => grp30DayNoticeLegalAction = value;
      }

      /// <summary>
      /// A value of Grp30DayNoticeCsePerson.
      /// </summary>
      [JsonPropertyName("grp30DayNoticeCsePerson")]
      public CsePerson Grp30DayNoticeCsePerson
      {
        get => grp30DayNoticeCsePerson ??= new();
        set => grp30DayNoticeCsePerson = value;
      }

      /// <summary>
      /// A value of Grp30DayNoticeCase.
      /// </summary>
      [JsonPropertyName("grp30DayNoticeCase")]
      public Case1 Grp30DayNoticeCase
      {
        get => grp30DayNoticeCase ??= new();
        set => grp30DayNoticeCase = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private LegalAction grp30DayNoticeLegalAction;
      private CsePerson grp30DayNoticeCsePerson;
      private Case1 grp30DayNoticeCase;
    }

    /// <summary>
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public CsePerson Temp
    {
      get => temp ??= new();
      set => temp = value;
    }

    /// <summary>
    /// A value of Change.
    /// </summary>
    [JsonPropertyName("change")]
    public Common Change
    {
      get => change ??= new();
      set => change = value;
    }

    /// <summary>
    /// A value of Temp2.
    /// </summary>
    [JsonPropertyName("temp2")]
    public LegalAction Temp2
    {
      get => temp2 ??= new();
      set => temp2 = value;
    }

    /// <summary>
    /// A value of Temp1.
    /// </summary>
    [JsonPropertyName("temp1")]
    public LegalAction Temp1
    {
      get => temp1 ??= new();
      set => temp1 = value;
    }

    /// <summary>
    /// Gets a value of Group30DayNoticeSorted.
    /// </summary>
    [JsonIgnore]
    public Array<Group30DayNoticeSortedGroup> Group30DayNoticeSorted =>
      group30DayNoticeSorted ??= new(Group30DayNoticeSortedGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Group30DayNoticeSorted for json serialization.
    /// </summary>
    [JsonPropertyName("group30DayNoticeSorted")]
    [Computed]
    public IList<Group30DayNoticeSortedGroup> Group30DayNoticeSorted_Json
    {
      get => group30DayNoticeSorted;
      set => Group30DayNoticeSorted.Assign(value);
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
    /// A value of SequenceCount.
    /// </summary>
    [JsonPropertyName("sequenceCount")]
    public Common SequenceCount
    {
      get => sequenceCount ??= new();
      set => sequenceCount = value;
    }

    /// <summary>
    /// A value of TotalRecordsRead.
    /// </summary>
    [JsonPropertyName("totalRecordsRead")]
    public WorkArea TotalRecordsRead
    {
      get => totalRecordsRead ??= new();
      set => totalRecordsRead = value;
    }

    /// <summary>
    /// A value of TotalRecordsUpdated.
    /// </summary>
    [JsonPropertyName("totalRecordsUpdated")]
    public WorkArea TotalRecordsUpdated
    {
      get => totalRecordsUpdated ??= new();
      set => totalRecordsUpdated = value;
    }

    /// <summary>
    /// A value of TotalCount.
    /// </summary>
    [JsonPropertyName("totalCount")]
    public Common TotalCount
    {
      get => totalCount ??= new();
      set => totalCount = value;
    }

    /// <summary>
    /// A value of NumOfRecordsUpdated.
    /// </summary>
    [JsonPropertyName("numOfRecordsUpdated")]
    public Common NumOfRecordsUpdated
    {
      get => numOfRecordsUpdated ??= new();
      set => numOfRecordsUpdated = value;
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

    /// <summary>
    /// A value of ReadCounter.
    /// </summary>
    [JsonPropertyName("readCounter")]
    public Common ReadCounter
    {
      get => readCounter ??= new();
      set => readCounter = value;
    }

    /// <summary>
    /// A value of CourtesyLetter.
    /// </summary>
    [JsonPropertyName("courtesyLetter")]
    public DateWorkArea CourtesyLetter
    {
      get => courtesyLetter ??= new();
      set => courtesyLetter = value;
    }

    /// <summary>
    /// A value of AlreadyProcessed.
    /// </summary>
    [JsonPropertyName("alreadyProcessed")]
    public LegalAction AlreadyProcessed
    {
      get => alreadyProcessed ??= new();
      set => alreadyProcessed = value;
    }

    /// <summary>
    /// A value of ReadCsePerson.
    /// </summary>
    [JsonPropertyName("readCsePerson")]
    public CsePerson ReadCsePerson
    {
      get => readCsePerson ??= new();
      set => readCsePerson = value;
    }

    /// <summary>
    /// A value of KsDriversLicense.
    /// </summary>
    [JsonPropertyName("ksDriversLicense")]
    public KsDriversLicense KsDriversLicense
    {
      get => ksDriversLicense ??= new();
      set => ksDriversLicense = value;
    }

    /// <summary>
    /// A value of CrtLetterSentDate.
    /// </summary>
    [JsonPropertyName("crtLetterSentDate")]
    public DateWorkArea CrtLetterSentDate
    {
      get => crtLetterSentDate ??= new();
      set => crtLetterSentDate = value;
    }

    /// <summary>
    /// A value of FirstRecordProcessed.
    /// </summary>
    [JsonPropertyName("firstRecordProcessed")]
    public Common FirstRecordProcessed
    {
      get => firstRecordProcessed ??= new();
      set => firstRecordProcessed = value;
    }

    /// <summary>
    /// A value of PreviousCsePerson.
    /// </summary>
    [JsonPropertyName("previousCsePerson")]
    public CsePerson PreviousCsePerson
    {
      get => previousCsePerson ??= new();
      set => previousCsePerson = value;
    }

    /// <summary>
    /// A value of BatchTimestampWorkArea.
    /// </summary>
    [JsonPropertyName("batchTimestampWorkArea")]
    public BatchTimestampWorkArea BatchTimestampWorkArea
    {
      get => batchTimestampWorkArea ??= new();
      set => batchTimestampWorkArea = value;
    }

    /// <summary>
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
    }

    /// <summary>
    /// A value of SpDocKey.
    /// </summary>
    [JsonPropertyName("spDocKey")]
    public SpDocKey SpDocKey
    {
      get => spDocKey ??= new();
      set => spDocKey = value;
    }

    /// <summary>
    /// A value of ValidPeriod30DayNotice.
    /// </summary>
    [JsonPropertyName("validPeriod30DayNotice")]
    public DateWorkArea ValidPeriod30DayNotice
    {
      get => validPeriod30DayNotice ??= new();
      set => validPeriod30DayNotice = value;
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
    /// A value of AdministrativeAction.
    /// </summary>
    [JsonPropertyName("administrativeAction")]
    public AdministrativeAction AdministrativeAction
    {
      get => administrativeAction ??= new();
      set => administrativeAction = value;
    }

    /// <summary>
    /// A value of PaymentsFound.
    /// </summary>
    [JsonPropertyName("paymentsFound")]
    public Common PaymentsFound
    {
      get => paymentsFound ??= new();
      set => paymentsFound = value;
    }

    /// <summary>
    /// A value of GoToNextCourtOrder.
    /// </summary>
    [JsonPropertyName("goToNextCourtOrder")]
    public Common GoToNextCourtOrder
    {
      get => goToNextCourtOrder ??= new();
      set => goToNextCourtOrder = value;
    }

    /// <summary>
    /// A value of NextPerson.
    /// </summary>
    [JsonPropertyName("nextPerson")]
    public Common NextPerson
    {
      get => nextPerson ??= new();
      set => nextPerson = value;
    }

    /// <summary>
    /// A value of LastLicenseProcessedDt.
    /// </summary>
    [JsonPropertyName("lastLicenseProcessedDt")]
    public DateWorkArea LastLicenseProcessedDt
    {
      get => lastLicenseProcessedDt ??= new();
      set => lastLicenseProcessedDt = value;
    }

    /// <summary>
    /// A value of MaxNumberObligors.
    /// </summary>
    [JsonPropertyName("maxNumberObligors")]
    public Common MaxNumberObligors
    {
      get => maxNumberObligors ??= new();
      set => maxNumberObligors = value;
    }

    /// <summary>
    /// A value of NumMnthBtw30DayLtr.
    /// </summary>
    [JsonPropertyName("numMnthBtw30DayLtr")]
    public Common NumMnthBtw30DayLtr
    {
      get => numMnthBtw30DayLtr ??= new();
      set => numMnthBtw30DayLtr = value;
    }

    /// <summary>
    /// A value of NumErrorRecords.
    /// </summary>
    [JsonPropertyName("numErrorRecords")]
    public Common NumErrorRecords
    {
      get => numErrorRecords ??= new();
      set => numErrorRecords = value;
    }

    /// <summary>
    /// A value of DollarAmtDebtsOwed.
    /// </summary>
    [JsonPropertyName("dollarAmtDebtsOwed")]
    public Common DollarAmtDebtsOwed
    {
      get => dollarAmtDebtsOwed ??= new();
      set => dollarAmtDebtsOwed = value;
    }

    /// <summary>
    /// A value of TotalNumLettersSent.
    /// </summary>
    [JsonPropertyName("totalNumLettersSent")]
    public Common TotalNumLettersSent
    {
      get => totalNumLettersSent ??= new();
      set => totalNumLettersSent = value;
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
    /// A value of MinPayment.
    /// </summary>
    [JsonPropertyName("minPayment")]
    public WorkArea MinPayment
    {
      get => minPayment ??= new();
      set => minPayment = value;
    }

    /// <summary>
    /// A value of MinTarget.
    /// </summary>
    [JsonPropertyName("minTarget")]
    public WorkArea MinTarget
    {
      get => minTarget ??= new();
      set => minTarget = value;
    }

    /// <summary>
    /// A value of NumOfDays.
    /// </summary>
    [JsonPropertyName("numOfDays")]
    public WorkArea NumOfDays
    {
      get => numOfDays ??= new();
      set => numOfDays = value;
    }

    /// <summary>
    /// A value of UpdateProgrProces.
    /// </summary>
    [JsonPropertyName("updateProgrProces")]
    public TextWorkArea UpdateProgrProces
    {
      get => updateProgrProces ??= new();
      set => updateProgrProces = value;
    }

    /// <summary>
    /// A value of ArrearsOnly.
    /// </summary>
    [JsonPropertyName("arrearsOnly")]
    public Common ArrearsOnly
    {
      get => arrearsOnly ??= new();
      set => arrearsOnly = value;
    }

    /// <summary>
    /// A value of MinimumPayment.
    /// </summary>
    [JsonPropertyName("minimumPayment")]
    public Common MinimumPayment
    {
      get => minimumPayment ??= new();
      set => minimumPayment = value;
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
    /// A value of PaymentPeriodDays.
    /// </summary>
    [JsonPropertyName("paymentPeriodDays")]
    public Common PaymentPeriodDays
    {
      get => paymentPeriodDays ??= new();
      set => paymentPeriodDays = value;
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
    /// A value of MinimumTarget.
    /// </summary>
    [JsonPropertyName("minimumTarget")]
    public Common MinimumTarget
    {
      get => minimumTarget ??= new();
      set => minimumTarget = value;
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
    /// A value of PassArea.
    /// </summary>
    [JsonPropertyName("passArea")]
    public External PassArea
    {
      get => passArea ??= new();
      set => passArea = value;
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
    /// A value of IncludeArrearsOnly.
    /// </summary>
    [JsonPropertyName("includeArrearsOnly")]
    public Common IncludeArrearsOnly
    {
      get => includeArrearsOnly ??= new();
      set => includeArrearsOnly = value;
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
    /// A value of ZeroDate.
    /// </summary>
    [JsonPropertyName("zeroDate")]
    public DateWorkArea ZeroDate
    {
      get => zeroDate ??= new();
      set => zeroDate = value;
    }

    /// <summary>
    /// A value of ReadLegalAction.
    /// </summary>
    [JsonPropertyName("readLegalAction")]
    public LegalAction ReadLegalAction
    {
      get => readLegalAction ??= new();
      set => readLegalAction = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of PreviousLegalAction.
    /// </summary>
    [JsonPropertyName("previousLegalAction")]
    public LegalAction PreviousLegalAction
    {
      get => previousLegalAction ??= new();
      set => previousLegalAction = value;
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
    /// A value of Clear.
    /// </summary>
    [JsonPropertyName("clear")]
    public CsePersonsWorkSet Clear
    {
      get => clear ??= new();
      set => clear = value;
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
    /// A value of LastPersonTotal.
    /// </summary>
    [JsonPropertyName("lastPersonTotal")]
    public Common LastPersonTotal
    {
      get => lastPersonTotal ??= new();
      set => lastPersonTotal = value;
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
    /// A value of AlreadyWritten.
    /// </summary>
    [JsonPropertyName("alreadyWritten")]
    public CsePerson AlreadyWritten
    {
      get => alreadyWritten ??= new();
      set => alreadyWritten = value;
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
    /// A value of StartBatchTimestampWorkArea.
    /// </summary>
    [JsonPropertyName("startBatchTimestampWorkArea")]
    public BatchTimestampWorkArea StartBatchTimestampWorkArea
    {
      get => startBatchTimestampWorkArea ??= new();
      set => startBatchTimestampWorkArea = value;
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
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
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
    /// A value of TotalRecordsProcessed.
    /// </summary>
    [JsonPropertyName("totalRecordsProcessed")]
    public Common TotalRecordsProcessed
    {
      get => totalRecordsProcessed ??= new();
      set => totalRecordsProcessed = value;
    }

    /// <summary>
    /// A value of TotalAdded.
    /// </summary>
    [JsonPropertyName("totalAdded")]
    public WorkArea TotalAdded
    {
      get => totalAdded ??= new();
      set => totalAdded = value;
    }

    /// <summary>
    /// A value of TotalCseLicense.
    /// </summary>
    [JsonPropertyName("totalCseLicense")]
    public WorkArea TotalCseLicense
    {
      get => totalCseLicense ??= new();
      set => totalCseLicense = value;
    }

    /// <summary>
    /// A value of CsePersonLicenseUpdated.
    /// </summary>
    [JsonPropertyName("csePersonLicenseUpdated")]
    public Common CsePersonLicenseUpdated
    {
      get => csePersonLicenseUpdated ??= new();
      set => csePersonLicenseUpdated = value;
    }

    /// <summary>
    /// A value of TotalKsDriversLicense.
    /// </summary>
    [JsonPropertyName("totalKsDriversLicense")]
    public WorkArea TotalKsDriversLicense
    {
      get => totalKsDriversLicense ??= new();
      set => totalKsDriversLicense = value;
    }

    /// <summary>
    /// A value of KdmvDriverLicenseUpdate.
    /// </summary>
    [JsonPropertyName("kdmvDriverLicenseUpdate")]
    public Common KdmvDriverLicenseUpdate
    {
      get => kdmvDriverLicenseUpdate ??= new();
      set => kdmvDriverLicenseUpdate = value;
    }

    /// <summary>
    /// A value of TotalErrors.
    /// </summary>
    [JsonPropertyName("totalErrors")]
    public WorkArea TotalErrors
    {
      get => totalErrors ??= new();
      set => totalErrors = value;
    }

    /// <summary>
    /// A value of TotalErrorRecords.
    /// </summary>
    [JsonPropertyName("totalErrorRecords")]
    public Common TotalErrorRecords
    {
      get => totalErrorRecords ??= new();
      set => totalErrorRecords = value;
    }

    /// <summary>
    /// A value of LastUpdatedNumRecsRead.
    /// </summary>
    [JsonPropertyName("lastUpdatedNumRecsRead")]
    public Common LastUpdatedNumRecsRead
    {
      get => lastUpdatedNumRecsRead ??= new();
      set => lastUpdatedNumRecsRead = value;
    }

    /// <summary>
    /// A value of LastUpdatedNumRecsUpd.
    /// </summary>
    [JsonPropertyName("lastUpdatedNumRecsUpd")]
    public Common LastUpdatedNumRecsUpd
    {
      get => lastUpdatedNumRecsUpd ??= new();
      set => lastUpdatedNumRecsUpd = value;
    }

    /// <summary>
    /// A value of LastUpdatedTotalError.
    /// </summary>
    [JsonPropertyName("lastUpdatedTotalError")]
    public Common LastUpdatedTotalError
    {
      get => lastUpdatedTotalError ??= new();
      set => lastUpdatedTotalError = value;
    }

    /// <summary>
    /// A value of LastUpdatedTotalAdded.
    /// </summary>
    [JsonPropertyName("lastUpdatedTotalAdded")]
    public Common LastUpdatedTotalAdded
    {
      get => lastUpdatedTotalAdded ??= new();
      set => lastUpdatedTotalAdded = value;
    }

    /// <summary>
    /// A value of ProgramRestart.
    /// </summary>
    [JsonPropertyName("programRestart")]
    public Common ProgramRestart
    {
      get => programRestart ??= new();
      set => programRestart = value;
    }

    /// <summary>
    /// A value of CourtesyLetterSentDt.
    /// </summary>
    [JsonPropertyName("courtesyLetterSentDt")]
    public DateWorkArea CourtesyLetterSentDt
    {
      get => courtesyLetterSentDt ??= new();
      set => courtesyLetterSentDt = value;
    }

    /// <summary>
    /// A value of CourtesyLetterCompare.
    /// </summary>
    [JsonPropertyName("courtesyLetterCompare")]
    public DateWorkArea CourtesyLetterCompare
    {
      get => courtesyLetterCompare ??= new();
      set => courtesyLetterCompare = value;
    }

    /// <summary>
    /// A value of NumOfObligorsProcessed.
    /// </summary>
    [JsonPropertyName("numOfObligorsProcessed")]
    public Common NumOfObligorsProcessed
    {
      get => numOfObligorsProcessed ??= new();
      set => numOfObligorsProcessed = value;
    }

    /// <summary>
    /// A value of Highest.
    /// </summary>
    [JsonPropertyName("highest")]
    public Case1 Highest
    {
      get => highest ??= new();
      set => highest = value;
    }

    /// <summary>
    /// Gets a value of Group30DayNotice.
    /// </summary>
    [JsonIgnore]
    public Array<Group30DayNoticeGroup> Group30DayNotice =>
      group30DayNotice ??= new(Group30DayNoticeGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Group30DayNotice for json serialization.
    /// </summary>
    [JsonPropertyName("group30DayNotice")]
    [Computed]
    public IList<Group30DayNoticeGroup> Group30DayNotice_Json
    {
      get => group30DayNotice;
      set => Group30DayNotice.Assign(value);
    }

    /// <summary>
    /// A value of DocNew.
    /// </summary>
    [JsonPropertyName("docNew")]
    public Infrastructure DocNew
    {
      get => docNew ??= new();
      set => docNew = value;
    }

    /// <summary>
    /// A value of Field.
    /// </summary>
    [JsonPropertyName("field")]
    public Field Field
    {
      get => field ??= new();
      set => field = value;
    }

    /// <summary>
    /// A value of FieldValue.
    /// </summary>
    [JsonPropertyName("fieldValue")]
    public FieldValue FieldValue
    {
      get => fieldValue ??= new();
      set => fieldValue = value;
    }

    /// <summary>
    /// A value of NumDaysSinceCurtsyLtr.
    /// </summary>
    [JsonPropertyName("numDaysSinceCurtsyLtr")]
    public Common NumDaysSinceCurtsyLtr
    {
      get => numDaysSinceCurtsyLtr ??= new();
      set => numDaysSinceCurtsyLtr = value;
    }

    /// <summary>
    /// A value of Local30DayLetterTimeFrame.
    /// </summary>
    [JsonPropertyName("local30DayLetterTimeFrame")]
    public DateWorkArea Local30DayLetterTimeFrame
    {
      get => local30DayLetterTimeFrame ??= new();
      set => local30DayLetterTimeFrame = value;
    }

    /// <summary>
    /// A value of ReadKsDriversLicense.
    /// </summary>
    [JsonPropertyName("readKsDriversLicense")]
    public KsDriversLicense ReadKsDriversLicense
    {
      get => readKsDriversLicense ??= new();
      set => readKsDriversLicense = value;
    }

    /// <summary>
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public CsePerson Error
    {
      get => error ??= new();
      set => error = value;
    }

    /// <summary>
    /// A value of SequenceCounter.
    /// </summary>
    [JsonPropertyName("sequenceCounter")]
    public WorkArea SequenceCounter
    {
      get => sequenceCounter ??= new();
      set => sequenceCounter = value;
    }

    /// <summary>
    /// A value of NumDaysBetw30DayLtr.
    /// </summary>
    [JsonPropertyName("numDaysBetw30DayLtr")]
    public Common NumDaysBetw30DayLtr
    {
      get => numDaysBetw30DayLtr ??= new();
      set => numDaysBetw30DayLtr = value;
    }

    /// <summary>
    /// A value of ValidPeriod30DayNotic.
    /// </summary>
    [JsonPropertyName("validPeriod30DayNotic")]
    public DateWorkArea ValidPeriod30DayNotic
    {
      get => validPeriod30DayNotic ??= new();
      set => validPeriod30DayNotic = value;
    }

    private CsePerson temp;
    private Common change;
    private LegalAction temp2;
    private LegalAction temp1;
    private Array<Group30DayNoticeSortedGroup> group30DayNoticeSorted;
    private DateWorkArea currentDate;
    private Common sequenceCount;
    private WorkArea totalRecordsRead;
    private WorkArea totalRecordsUpdated;
    private Common totalCount;
    private Common numOfRecordsUpdated;
    private Common recordProcessed;
    private Common readCounter;
    private DateWorkArea courtesyLetter;
    private LegalAction alreadyProcessed;
    private CsePerson readCsePerson;
    private KsDriversLicense ksDriversLicense;
    private DateWorkArea crtLetterSentDate;
    private Common firstRecordProcessed;
    private CsePerson previousCsePerson;
    private BatchTimestampWorkArea batchTimestampWorkArea;
    private Document document;
    private SpDocKey spDocKey;
    private DateWorkArea validPeriod30DayNotice;
    private ScreenOwedAmountsDtl screenOwedAmountsDtl;
    private AdministrativeAction administrativeAction;
    private Common paymentsFound;
    private Common goToNextCourtOrder;
    private Common nextPerson;
    private DateWorkArea lastLicenseProcessedDt;
    private Common maxNumberObligors;
    private Common numMnthBtw30DayLtr;
    private Common numErrorRecords;
    private Common dollarAmtDebtsOwed;
    private Common totalNumLettersSent;
    private Common numberOfRecordsRead;
    private WorkArea minPayment;
    private WorkArea minTarget;
    private WorkArea numOfDays;
    private TextWorkArea updateProgrProces;
    private Common arrearsOnly;
    private Common minimumPayment;
    private DateWorkArea startDate;
    private Common paymentPeriodDays;
    private WorkArea workArea;
    private Common minimumTarget;
    private ExitStateWorkArea exitStateWorkArea;
    private External passArea;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private TextWorkArea postion;
    private Common currentPosition;
    private Common fieldNumber;
    private Common current;
    private Common startCommon;
    private Common includeArrearsOnly;
    private CsePerson csePerson;
    private DateWorkArea zeroDate;
    private LegalAction readLegalAction;
    private Common owedAmount;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePersonsWorkSet previousProcess;
    private LegalAction legalAction;
    private LegalAction previousLegalAction;
    private Common previousPersonTotal;
    private Common personTotal;
    private Common search;
    private Common phonetic;
    private CsePersonsWorkSet startCsePersonsWorkSet;
    private CsePerson alreadyCheckedSsn;
    private CsePersonsWorkSet clear;
    private FinanceWorkAttributes financeWorkAttributes;
    private Infrastructure infrastructure;
    private WorkArea detail;
    private Common personSend;
    private Common lastPersonTotal;
    private CsePersonsWorkSet lastCsePersonsWorkSet;
    private LegalAction lastLegalAction;
    private CsePerson alreadyWritten;
    private CsePersonsWorkSet lastSuccessfulProcessed;
    private LegalAction lastSuccesfulProcessed;
    private BatchTimestampWorkArea startBatchTimestampWorkArea;
    private CsePerson previousfindMe;
    private DateWorkArea dateWorkArea;
    private TextWorkArea year;
    private TextWorkArea month;
    private TextWorkArea day;
    private Common totalRecordsProcessed;
    private WorkArea totalAdded;
    private WorkArea totalCseLicense;
    private Common csePersonLicenseUpdated;
    private WorkArea totalKsDriversLicense;
    private Common kdmvDriverLicenseUpdate;
    private WorkArea totalErrors;
    private Common totalErrorRecords;
    private Common lastUpdatedNumRecsRead;
    private Common lastUpdatedNumRecsUpd;
    private Common lastUpdatedTotalError;
    private Common lastUpdatedTotalAdded;
    private Common programRestart;
    private DateWorkArea courtesyLetterSentDt;
    private DateWorkArea courtesyLetterCompare;
    private Common numOfObligorsProcessed;
    private Case1 highest;
    private Array<Group30DayNoticeGroup> group30DayNotice;
    private Infrastructure docNew;
    private Field field;
    private FieldValue fieldValue;
    private Common numDaysSinceCurtsyLtr;
    private DateWorkArea local30DayLetterTimeFrame;
    private KsDriversLicense readKsDriversLicense;
    private CsePerson error;
    private WorkArea sequenceCounter;
    private Common numDaysBetw30DayLtr;
    private DateWorkArea validPeriod30DayNotic;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ReadOnlyKsDriversLicense.
    /// </summary>
    [JsonPropertyName("readOnlyKsDriversLicense")]
    public KsDriversLicense ReadOnlyKsDriversLicense
    {
      get => readOnlyKsDriversLicense ??= new();
      set => readOnlyKsDriversLicense = value;
    }

    /// <summary>
    /// A value of ReadOnlyCsePerson1.
    /// </summary>
    [JsonPropertyName("readOnlyCsePerson1")]
    public CsePerson ReadOnlyCsePerson1
    {
      get => readOnlyCsePerson1 ??= new();
      set => readOnlyCsePerson1 = value;
    }

    /// <summary>
    /// A value of ReadOnlyLegalAction.
    /// </summary>
    [JsonPropertyName("readOnlyLegalAction")]
    public LegalAction ReadOnlyLegalAction
    {
      get => readOnlyLegalAction ??= new();
      set => readOnlyLegalAction = value;
    }

    /// <summary>
    /// A value of ProcessedCheck.
    /// </summary>
    [JsonPropertyName("processedCheck")]
    public KsDriversLicense ProcessedCheck
    {
      get => processedCheck ??= new();
      set => processedCheck = value;
    }

    /// <summary>
    /// A value of KsDriversLicense.
    /// </summary>
    [JsonPropertyName("ksDriversLicense")]
    public KsDriversLicense KsDriversLicense
    {
      get => ksDriversLicense ??= new();
      set => ksDriversLicense = value;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of SecondRead.
    /// </summary>
    [JsonPropertyName("secondRead")]
    public KsDriversLicense SecondRead
    {
      get => secondRead ??= new();
      set => secondRead = value;
    }

    /// <summary>
    /// A value of Create.
    /// </summary>
    [JsonPropertyName("create")]
    public KsDriversLicense Create
    {
      get => create ??= new();
      set => create = value;
    }

    /// <summary>
    /// A value of ReadOnlyCsePerson2.
    /// </summary>
    [JsonPropertyName("readOnlyCsePerson2")]
    public CsePerson ReadOnlyCsePerson2
    {
      get => readOnlyCsePerson2 ??= new();
      set => readOnlyCsePerson2 = value;
    }

    private KsDriversLicense readOnlyKsDriversLicense;
    private CsePerson readOnlyCsePerson1;
    private LegalAction readOnlyLegalAction;
    private KsDriversLicense processedCheck;
    private KsDriversLicense ksDriversLicense;
    private CsePerson csePerson;
    private LegalAction legalAction;
    private CaseRole caseRole;
    private CsePersonAccount obligor;
    private Obligation obligation;
    private ObligationType obligationType;
    private Case1 case1;
    private CaseUnit caseUnit;
    private LegalActionCaseRole legalActionCaseRole;
    private KsDriversLicense secondRead;
    private KsDriversLicense create;
    private CsePerson readOnlyCsePerson2;
  }
#endregion
}
