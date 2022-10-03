// Program: OE_B457_KDMV_COURTESY_LETTER, ID: 371380776, model: 746.
// Short name: SWEE457B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B457_KDMV_COURTESY_LETTER.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class OeB457KdmvCourtesyLetter: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B457_KDMV_COURTESY_LETTER program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB457KdmvCourtesyLetter(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB457KdmvCourtesyLetter.
  /// </summary>
  public OeB457KdmvCourtesyLetter(IContext context, Import import, Export export)
    :
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
    UseOeB457Housekeeping();

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
    local.TotalNumRecsAdded.Count = local.LastUpdatedTotalAdded.Count;
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
              local.StartDate.Date = Now().Date;
              local.MaximumTarget.TotalCurrency = 99999999;
            }
            else
            {
              local.WorkArea.Text15 =
                Substring(local.ProgramProcessingInfo.ParameterList,
                local.StartCommon.Count, local.Current.Count - 1);
              local.MaximumTarget.TotalCurrency =
                StringToNumber(local.WorkArea.Text15);
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
          case 6:
            if (local.Current.Count == 1)
            {
              local.NumMonthsBetwCurtsyLtr.Count = 36;
            }
            else
            {
              local.WorkArea.Text15 =
                Substring(local.ProgramProcessingInfo.ParameterList,
                local.StartCommon.Count, local.Current.Count - 1);
              local.NumMonthsBetwCurtsyLtr.Count =
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

    local.BatchTimestampWorkArea.IefTimestamp = new DateTime(1, 1, 1);
    local.StartDate.Date = local.ProgramProcessingInfo.ProcessDate;
    local.AdministrativeAction.Type1 = "KDMV";
    local.ValidPeriodCourtesyLtr.Date =
      AddMonths(local.StartDate.Date, -local.NumMonthsBetwCurtsyLtr.Count);
    local.ValidPeriod30DayNotic.Date =
      AddDays(local.StartDate.Date, -local.NumDaysBetw30DayLtr.Count);

    if (ReadKsDriversLicense4())
    {
      local.LastLicenseProcessedDt.Date =
        entities.ReadOnlyKsDriversLicense.ValidationDate;
    }

    ReadKsDriversLicense1();
    local.SequenceCount.Count = local.ReadCounter.Count;

    foreach(var item in ReadKsDriversLicense5())
    {
      ExitState = "ACO_NN0000_ALL_OK";
      local.ReadKsDriversLicense.SequenceCounter =
        entities.ReadOnlyKsDriversLicense.SequenceCounter;

      if (ReadCsePerson())
      {
        local.ReadCsePerson.Number = entities.ReadOnlyCsePerson.Number;
      }

      if (ReadKsDriversLicense3())
      {
        continue;
      }
      else
      {
        // we have not processed this ap so we will continue processing it
      }

      // checking to make sure we have not already processed this record before
      ++local.NumberOfRecordsRead.Count;
      local.FirstRecordProcessed.Flag = "";

      if (ReadKsDriversLicenseCsePerson())
      {
        try
        {
          UpdateKsDriversLicense2();
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
          local.PreviousCsePerson.Number = entities.CsePerson.Number;
          MoveKsDriversLicense(entities.KsDriversLicense, local.KsDriversLicense);
            
          local.AlreadyProcessed.StandardNumber = "";

          foreach(var item1 in ReadLegalAction())
          {
            ExitState = "ACO_NN0000_ALL_OK";

            if (Equal(entities.LegalAction.StandardNumber,
              local.AlreadyProcessed.StandardNumber))
            {
              // we only want to go through this once per court order number (
              // stand number field)
              continue;
            }

            local.AlreadyProcessed.StandardNumber =
              entities.LegalAction.StandardNumber;

            if (ReadKsDriversLicense2())
            {
              continue;

              // for some reason (possibly a restart) we have picked a record 
              // that has already
              // been processed, we do not want to reprocess this record so we 
              // will move on to
              // the next record
            }

            UseOeDetermineDmvCriteria();

            if (AsChar(local.LtrWithinTimeFrame.Flag) == 'Y')
            {
              goto ReadEach1;

              // aleady have a courtesy letter that is within the time period so
              // we will go to the next AP
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

              goto Test;
            }

            local.OwedAmount.TotalCurrency =
              local.ScreenOwedAmountsDtl.TotalArrearsOwed + local
              .ScreenOwedAmountsDtl.TotalInterestOwed - (
                local.ScreenOwedAmountsDtl.TotalArrearsColl + local
              .ScreenOwedAmountsDtl.TotalInterestColl + local
              .ScreenOwedAmountsDtl.TotalVoluntaryColl + local
              .ScreenOwedAmountsDtl.UndistributedAmt);

            if (local.OwedAmount.TotalCurrency >= local
              .MinimumTarget.TotalCurrency && local
              .OwedAmount.TotalCurrency <= local.MaximumTarget.TotalCurrency)
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
                // To see if any payments have been made that are still in cash 
                // receipt and have
                // not made to collections yet. There has not been enough money 
                // in the collection
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

                    goto Test;
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
                  // to do a checkpoint so we will not just partly save changes 
                  // to the current AP like
                  // we would have if we would wait until after we have done the
                  // updates to the
                  // current record
                  // we only want to write out one record per obligor
                  local.SpDocKey.KeyPerson = local.PreviousProcess.Number;
                  local.Document.Name = "DLCURTSY";
                  UseSpCreateDocumentInfrastruct();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    ++local.NumErrorRecords.Count;
                    local.Error.Number = local.PreviousProcess.Number;

                    goto ReadEach2;
                  }

                  ++local.TotalNumRecsAdded.Count;
                  local.AlreadyWritten.Number = local.PreviousProcess.Number;
                  local.LastSuccessfulProcessed.Assign(local.CsePersonsWorkSet);
                  MoveLegalAction(local.LegalAction,
                    local.LastSuccesfulProcessed);
                  local.PersonSend.Flag = "";

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

                      // we have to take off one from the restart point since it
                      // will add one in the update
                      // cab, if we did not take one off then it would let the 
                      // miss processing some records
                      local.ProgramCheckpointRestart.CheckpointCount =
                        local.NumberOfRecordsRead.Count - 1;
                      local.TotalAdded.Text15 =
                        NumberToString(local.TotalNumRecsAdded.Count, 15);
                      local.TotalErrors.Text15 =
                        NumberToString(local.NumErrorRecords.Count, 15);
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
                        local.TotalNumRecsAdded.Count;
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

                  if (local.TotalNumRecsAdded.Count >= local
                    .MaxNumberObligors.Count)
                  {
                    // THE PROCESS HAS CREATED THE MAXMIUM NUMBER OF COURTESY 
                    // LETTERS IT IS ALLOWED TO CREATE
                    local.PreviousProcess.Number = "";

                    goto ReadEach2;
                  }
                }
                else if (!Equal(entities.CsePerson.Number,
                  local.AlreadyWritten.Number) && IsExitState
                  ("ACO_NN0000_ALL_OK"))
                {
                  local.LastSuccessfulProcessed.Assign(local.CsePersonsWorkSet);
                  MoveLegalAction(local.LegalAction,
                    local.LastSuccesfulProcessed);
                }

                local.Infrastructure.CaseNumber = "";
                local.Infrastructure.CaseUnitNumber = 0;

                foreach(var item2 in ReadCaseCaseUnitCaseRole())
                {
                  // It really does not matter about wheater the case role is 
                  // active or not we are just
                  // concern with the highest cae nunber associated with the 
                  // cout order that is being
                  // worked - money is still owed therefore it is still in play.
                  // So we are not going to
                  // check start and end dates
                  if (Lt(local.Infrastructure.CaseNumber, entities.Case1.Number))
                    
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

                  goto Test;
                }

                local.FinanceWorkAttributes.NumericalDollarValue =
                  local.OwedAmount.TotalCurrency;
                UseFnCabReturnTextDollars();
                local.Infrastructure.SituationNumber = 0;
                local.Infrastructure.ReasonCode = "DMVCOURTESYLTR";
                local.Infrastructure.EventId = 1;
                local.Infrastructure.EventType = "ADMIN";
                local.Infrastructure.ProcessStatus = "Q";
                local.Infrastructure.UserId = "KDMV";
                local.Infrastructure.BusinessObjectCd = "ENF";
                local.Infrastructure.ReferenceDate = local.StartDate.Date;
                local.Infrastructure.CreatedBy =
                  local.ProgramProcessingInfo.Name;
                local.Infrastructure.EventDetailName = "DMV Courtesy Letter";
                local.Infrastructure.CsePersonNumber =
                  entities.CsePerson.Number;
                local.Infrastructure.DenormNumeric12 =
                  entities.LegalAction.Identifier;
                local.Infrastructure.DenormText12 =
                  entities.LegalAction.CourtCaseNumber;
                local.Detail.Text11 = ", Arrears $";
                local.Infrastructure.Detail =
                  "Courtesy Letter sent, Ct Order # " + TrimEnd
                  (entities.LegalAction.StandardNumber) + local
                  .Detail.Text11 + local.FinanceWorkAttributes.TextDollarValue;

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
                // CASEUNIT 	CASE UNIT (LIFECYCLE, OBLIGATIONS PAID/ACTIVATED/
                // REACTIVATED)
                // COMPLIANCE	OBLIGATIONS/FINANCE ACTIVITIES, PARENTAL RIGHTS/
                // EMANCIPATION
                // CSENET   	CSENET, QUICK LOCATE
                // DATEMON  	DATE MONITOR (FUTURE DATES - EMANCIPATION/DISCHARGE
                // /RELEASE)
                // DOCUMENT 	DOCUMENT (GENTEST, LOCATE, INSURANCE, MODIFICATION,
                // JE)
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

                  goto Test;
                }

                export.TotalAmtDebtOwed.TotalCurrency += local.OwedAmount.
                  TotalCurrency;
                local.PersonTotal.TotalCurrency += local.OwedAmount.
                  TotalCurrency;
                local.PersonSend.Flag = "Y";

                // **********************************************************************************
                // If we have gotten this far then we need to create a courtesy 
                // record for the
                // current court order and current obligor
                // ********************************************************************************
                if (IsEmpty(local.FirstRecordProcessed.Flag))
                {
                  local.FirstRecordProcessed.Flag = "Y";

                  if (!Lt(local.ZeroDate.Date,
                    entities.KsDriversLicense.CourtesyLetterSentDate))
                  {
                    // this is for  record stub, that is not attached to any 
                    // court order - was just holding
                    // the ap's driver's license info
                    // this will be the only time we update a courtesy letter 
                    // record, the rest will be created
                    try
                    {
                      UpdateKsDriversLicense1();
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

                          goto Test;
                        case ErrorCode.PermittedValueViolation:
                          ExitState = "KS_DRIVERS_LICENSE_PV";
                          ++local.NumErrorRecords.Count;
                          local.Error.Number = entities.CsePerson.Number;

                          goto Test;
                        case ErrorCode.DatabaseError:
                          break;
                        default:
                          throw;
                      }
                    }
                  }
                  else
                  {
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

                          goto Test;
                        case ErrorCode.PermittedValueViolation:
                          ExitState = "KS_DRIVERS_LICENSE_PV";
                          ++local.NumErrorRecords.Count;
                          local.Error.Number = entities.CsePerson.Number;

                          goto Test;
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

                        goto Test;
                      case ErrorCode.PermittedValueViolation:
                        ExitState = "KS_DRIVERS_LICENSE_PV";
                        ++local.NumErrorRecords.Count;
                        local.Error.Number = entities.CsePerson.Number;

                        goto Test;
                      case ErrorCode.DatabaseError:
                        break;
                      default:
                        throw;
                    }
                  }
                }

                if (!Equal(entities.CsePerson.Number,
                  local.LastCsePersonsWorkSet.Number))
                {
                  local.LastCsePersonsWorkSet.Number =
                    entities.CsePerson.Number;
                }
              }
              else
              {
                continue;
              }
            }
          }
        }

Test:

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabExtractExitStateMessage();
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "Record failed because: " + TrimEnd
            (local.ExitStateWorkArea.Message) + " CSE Person # " + local
            .Error.Number;
          UseCabErrorReport();
          local.Error.Number = "";

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }
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
      local.Document.Name = "DLCURTSY";
      local.SpDocKey.KeyPerson = local.LastCsePersonsWorkSet.Number;
      ++local.TotalNumRecsAdded.Count;
      UseSpCreateDocumentInfrastruct();
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseOeB457Close1();
      local.ProgramCheckpointRestart.CheckpointCount = -1;
      local.ProgramCheckpointRestart.RestartInd = "N";
      local.ProgramCheckpointRestart.ProgramName =
        local.ProgramProcessingInfo.Name;
      local.ProgramCheckpointRestart.RestartInfo = "";
      UseUpdatePgmCheckpointRestart();

      foreach(var item in ReadKsDriversLicense6())
      {
        // we need to clear the  processing ind for next time, since the 
        // indicator only refers to the current run.
        try
        {
          UpdateKsDriversLicense3();
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

      UseOeB457Close2();
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

    useImport.IncludeArrearsOnly.Flag = local.ArrearsOnly.Flag;
    useImport.Obligor.Number = entities.CsePerson.Number;
    useImport.FilterByStdNo.StandardNumber =
      entities.LegalAction.StandardNumber;
    useImport.StartDate.Date = local.StartDate.Date;

    Call(FnComputeTotalsForCtOrder3.Execute, useImport, useExport);

    MoveScreenOwedAmountsDtl(useExport.ScreenOwedAmountsDtl,
      local.ScreenOwedAmountsDtl);
  }

  private void UseOeB457Close1()
  {
    var useImport = new OeB457Close.Import();
    var useExport = new OeB457Close.Export();

    useImport.NumOfRecordsUpdated.Count = local.NumOfRecordsUpdated.Count;
    useImport.NumberOfRecordsRead.Count = local.NumberOfRecordsRead.Count;
    useImport.TotalNumRecsAdded.Count = local.TotalNumRecsAdded.Count;
    useImport.NumErrorRecords.Count = local.NumErrorRecords.Count;

    Call(OeB457Close.Execute, useImport, useExport);
  }

  private void UseOeB457Close2()
  {
    var useImport = new OeB457Close.Import();
    var useExport = new OeB457Close.Export();

    useImport.NumOfRecordsUpdated.Count = local.LastUpdatedNumRecsUpd.Count;
    useImport.NumberOfRecordsRead.Count = local.LastUpdatedNumRecsRead.Count;
    useImport.TotalNumRecsAdded.Count = local.LastUpdatedTotalAdded.Count;
    useImport.NumErrorRecords.Count = local.LastUpdatedTotalError.Count;

    Call(OeB457Close.Execute, useImport, useExport);
  }

  private void UseOeB457Housekeeping()
  {
    var useImport = new OeB457Housekeeping.Import();
    var useExport = new OeB457Housekeeping.Export();

    Call(OeB457Housekeeping.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseOeDetermineDmvCriteria()
  {
    var useImport = new OeDetermineDmvCriteria.Import();
    var useExport = new OeDetermineDmvCriteria.Export();

    useImport.StartDate.Date = local.StartDate.Date;
    useImport.AdministrativeAction.Type1 = local.AdministrativeAction.Type1;
    useImport.ValidPeriodCourtesyLtr.Date = local.ValidPeriodCourtesyLtr.Date;
    useImport.LegalAction.StandardNumber = entities.LegalAction.StandardNumber;
    useImport.CsePerson.Number = entities.CsePerson.Number;
    useImport.ValidPeriod30DayNotic.Date = local.ValidPeriod30DayNotic.Date;

    Call(OeDetermineDmvCriteria.Execute, useImport, useExport);

    local.NextPerson.Flag = useExport.NextPerson.Flag;
    local.GoToNextCourtOrder.Flag = useExport.GoToNextCourtOrder.Flag;
    local.LtrWithinTimeFrame.Flag = useExport.LtrWithinTimeFrame.Flag;
    local.CrtLetterSentDate.Date = useExport.CrtLetterSentDate.Date;
  }

  private void UseOeDeterminePaymentsForKdmvP()
  {
    var useImport = new OeDeterminePaymentsForKdmvP.Import();
    var useExport = new OeDeterminePaymentsForKdmvP.Export();

    useImport.LocalMinumPayment.TotalCurrency =
      local.MiniumPayment.TotalCurrency;
    useImport.NumberOfDays.Count = local.PaymentPeriodDays.Count;
    useImport.Start.Date = local.StartDate.Date;
    useImport.LegalAction.StandardNumber = entities.LegalAction.StandardNumber;
    useImport.CsePerson.Number = entities.CsePerson.Number;

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

  private void UseSpCreateDocumentInfrastruct()
  {
    var useImport = new SpCreateDocumentInfrastruct.Import();
    var useExport = new SpCreateDocumentInfrastruct.Export();

    useImport.Document.Name = local.Document.Name;
    useImport.SpDocKey.KeyPerson = local.SpDocKey.KeyPerson;

    Call(SpCreateDocumentInfrastruct.Execute, useImport, useExport);
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
    var ksDvrLicense = local.KsDriversLicense.KsDvrLicense ?? "";
    var validationDate = local.KsDriversLicense.ValidationDate;
    var courtesyLetterSentDate = local.StartDate.Date;
    var sequenceCounter = local.SequenceCount.Count;

    entities.KsDriversLicense.Populated = false;
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
        db.SetNullableDate(command, "ltr30DayDate", default(DateTime));
        db.SetNullableString(command, "servCompleteInd", "");
        db.SetNullableString(command, "restrictionStatus", "");
        db.SetNullableDecimal(command, "amountOwed", 0M);
        db.SetNullableString(command, "lastUpdatedBy", createdBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", createdTstamp);
        db.SetNullableString(command, "note1", "");
        db.SetNullableString(command, "processedInd", "");
        db.SetInt32(command, "sequenceCounter", sequenceCounter);
      });

    entities.KsDriversLicense.CreatedBy = createdBy;
    entities.KsDriversLicense.CreatedTstamp = createdTstamp;
    entities.KsDriversLicense.CspNum = cspNum;
    entities.KsDriversLicense.LgaIdentifier = lgaIdentifier;
    entities.KsDriversLicense.KsDvrLicense = ksDvrLicense;
    entities.KsDriversLicense.ValidationDate = validationDate;
    entities.KsDriversLicense.CourtesyLetterSentDate = courtesyLetterSentDate;
    entities.KsDriversLicense.LastUpdatedBy = createdBy;
    entities.KsDriversLicense.LastUpdatedTstamp = createdTstamp;
    entities.KsDriversLicense.ProcessedInd = "";
    entities.KsDriversLicense.SequenceCounter = sequenceCounter;
    entities.KsDriversLicense.Populated = true;
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
    entities.ReadOnlyCsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.ReadOnlyKsDriversLicense.CspNum);
      },
      (db, reader) =>
      {
        entities.ReadOnlyCsePerson.Number = db.GetString(reader, 0);
        entities.ReadOnlyCsePerson.Populated = true;
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
    entities.ProcessedCheck.Populated = false;

    return Read("ReadKsDriversLicense2",
      (db, command) =>
      {
        db.SetString(command, "cspNum", entities.CsePerson.Number);
        db.SetNullableString(
          command, "standardNo", entities.LegalAction.StandardNumber ?? "");
        db.SetNullableDate(
          command, "courtesyLtrDate", local.StartDate.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.ProcessedCheck.CreatedTstamp = db.GetDateTime(reader, 0);
        entities.ProcessedCheck.CspNum = db.GetString(reader, 1);
        entities.ProcessedCheck.LgaIdentifier = db.GetNullableInt32(reader, 2);
        entities.ProcessedCheck.ValidationDate = db.GetNullableDate(reader, 3);
        entities.ProcessedCheck.CourtesyLetterSentDate =
          db.GetNullableDate(reader, 4);
        entities.ProcessedCheck.ProcessedInd = db.GetNullableString(reader, 5);
        entities.ProcessedCheck.SequenceCounter = db.GetInt32(reader, 6);
        entities.ProcessedCheck.Populated = true;
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
        entities.ProcessedCheck.CreatedTstamp = db.GetDateTime(reader, 0);
        entities.ProcessedCheck.CspNum = db.GetString(reader, 1);
        entities.ProcessedCheck.LgaIdentifier = db.GetNullableInt32(reader, 2);
        entities.ProcessedCheck.ValidationDate = db.GetNullableDate(reader, 3);
        entities.ProcessedCheck.CourtesyLetterSentDate =
          db.GetNullableDate(reader, 4);
        entities.ProcessedCheck.ProcessedInd = db.GetNullableString(reader, 5);
        entities.ProcessedCheck.SequenceCounter = db.GetInt32(reader, 6);
        entities.ProcessedCheck.Populated = true;
      });
  }

  private bool ReadKsDriversLicense4()
  {
    entities.ReadOnlyKsDriversLicense.Populated = false;

    return Read("ReadKsDriversLicense4",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTstamp",
          local.BatchTimestampWorkArea.IefTimestamp.GetValueOrDefault());
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
        entities.ReadOnlyKsDriversLicense.ProcessedInd =
          db.GetNullableString(reader, 4);
        entities.ReadOnlyKsDriversLicense.SequenceCounter =
          db.GetInt32(reader, 5);
        entities.ReadOnlyKsDriversLicense.Populated = true;
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
        entities.ReadOnlyKsDriversLicense.ProcessedInd =
          db.GetNullableString(reader, 4);
        entities.ReadOnlyKsDriversLicense.SequenceCounter =
          db.GetInt32(reader, 5);
        entities.ReadOnlyKsDriversLicense.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadKsDriversLicense6()
  {
    entities.KsDriversLicense.Populated = false;

    return ReadEach("ReadKsDriversLicense6",
      null,
      (db, reader) =>
      {
        entities.KsDriversLicense.CreatedBy = db.GetString(reader, 0);
        entities.KsDriversLicense.CreatedTstamp = db.GetDateTime(reader, 1);
        entities.KsDriversLicense.CspNum = db.GetString(reader, 2);
        entities.KsDriversLicense.LgaIdentifier =
          db.GetNullableInt32(reader, 3);
        entities.KsDriversLicense.KsDvrLicense =
          db.GetNullableString(reader, 4);
        entities.KsDriversLicense.ValidationDate =
          db.GetNullableDate(reader, 5);
        entities.KsDriversLicense.CourtesyLetterSentDate =
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
        entities.KsDriversLicense.LgaIdentifier =
          db.GetNullableInt32(reader, 3);
        entities.KsDriversLicense.KsDvrLicense =
          db.GetNullableString(reader, 4);
        entities.KsDriversLicense.ValidationDate =
          db.GetNullableDate(reader, 5);
        entities.KsDriversLicense.CourtesyLetterSentDate =
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

  private IEnumerable<bool> ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalAction",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 1);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private void UpdateKsDriversLicense1()
  {
    System.Diagnostics.Debug.Assert(entities.KsDriversLicense.Populated);

    var lgaIdentifier = entities.LegalAction.Identifier;
    var courtesyLetterSentDate = local.StartDate.Date;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTstamp = Now();

    entities.KsDriversLicense.Populated = false;
    Update("UpdateKsDriversLicense1",
      (db, command) =>
      {
        db.SetNullableInt32(command, "lgaIdentifier", lgaIdentifier);
        db.SetNullableDate(command, "courtesyLtrDate", courtesyLetterSentDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetString(command, "cspNum", entities.KsDriversLicense.CspNum);
        db.SetInt32(
          command, "sequenceCounter",
          entities.KsDriversLicense.SequenceCounter);
      });

    entities.KsDriversLicense.LgaIdentifier = lgaIdentifier;
    entities.KsDriversLicense.CourtesyLetterSentDate = courtesyLetterSentDate;
    entities.KsDriversLicense.LastUpdatedBy = lastUpdatedBy;
    entities.KsDriversLicense.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.KsDriversLicense.Populated = true;
  }

  private void UpdateKsDriversLicense2()
  {
    System.Diagnostics.Debug.Assert(entities.KsDriversLicense.Populated);

    var processedInd = "Y";

    entities.KsDriversLicense.Populated = false;
    Update("UpdateKsDriversLicense2",
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

  private void UpdateKsDriversLicense3()
  {
    System.Diagnostics.Debug.Assert(entities.KsDriversLicense.Populated);
    entities.KsDriversLicense.Populated = false;
    Update("UpdateKsDriversLicense3",
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
    /// <summary>A ApGroup group.</summary>
    [Serializable]
    public class ApGroup
    {
      /// <summary>
      /// A value of Ap1.
      /// </summary>
      [JsonPropertyName("ap1")]
      public CsePerson Ap1
      {
        get => ap1 ??= new();
        set => ap1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1500;

      private CsePerson ap1;
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
    /// A value of ReadKsDriversLicense.
    /// </summary>
    [JsonPropertyName("readKsDriversLicense")]
    public KsDriversLicense ReadKsDriversLicense
    {
      get => readKsDriversLicense ??= new();
      set => readKsDriversLicense = value;
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
    /// Gets a value of Ap.
    /// </summary>
    [JsonIgnore]
    public Array<ApGroup> Ap => ap ??= new(ApGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Ap for json serialization.
    /// </summary>
    [JsonPropertyName("ap")]
    [Computed]
    public IList<ApGroup> Ap_Json
    {
      get => ap;
      set => Ap.Assign(value);
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
    /// A value of LtrWithinTimeFrame.
    /// </summary>
    [JsonPropertyName("ltrWithinTimeFrame")]
    public Common LtrWithinTimeFrame
    {
      get => ltrWithinTimeFrame ??= new();
      set => ltrWithinTimeFrame = value;
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
    /// A value of ValidPeriodCourtesyLtr.
    /// </summary>
    [JsonPropertyName("validPeriodCourtesyLtr")]
    public DateWorkArea ValidPeriodCourtesyLtr
    {
      get => validPeriodCourtesyLtr ??= new();
      set => validPeriodCourtesyLtr = value;
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
    /// A value of NumMonthsBetwCurtsyLtr.
    /// </summary>
    [JsonPropertyName("numMonthsBetwCurtsyLtr")]
    public Common NumMonthsBetwCurtsyLtr
    {
      get => numMonthsBetwCurtsyLtr ??= new();
      set => numMonthsBetwCurtsyLtr = value;
    }

    /// <summary>
    /// A value of MaximumTarget.
    /// </summary>
    [JsonPropertyName("maximumTarget")]
    public Common MaximumTarget
    {
      get => maximumTarget ??= new();
      set => maximumTarget = value;
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
    /// A value of MiniumPayment.
    /// </summary>
    [JsonPropertyName("miniumPayment")]
    public Common MiniumPayment
    {
      get => miniumPayment ??= new();
      set => miniumPayment = value;
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

    private Common numDaysBetw30DayLtr;
    private DateWorkArea validPeriod30DayNotic;
    private CsePerson error;
    private WorkArea sequenceCounter;
    private KsDriversLicense readKsDriversLicense;
    private Common sequenceCount;
    private WorkArea totalRecordsRead;
    private WorkArea totalRecordsUpdated;
    private Common totalCount;
    private Array<ApGroup> ap;
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
    private Common ltrWithinTimeFrame;
    private Document document;
    private SpDocKey spDocKey;
    private DateWorkArea validPeriodCourtesyLtr;
    private ScreenOwedAmountsDtl screenOwedAmountsDtl;
    private AdministrativeAction administrativeAction;
    private Common paymentsFound;
    private Common goToNextCourtOrder;
    private Common nextPerson;
    private DateWorkArea lastLicenseProcessedDt;
    private Common maxNumberObligors;
    private Common numMonthsBetwCurtsyLtr;
    private Common maximumTarget;
    private Common numErrorRecords;
    private Common dollarAmtDebtsOwed;
    private Common totalNumRecsAdded;
    private Common numberOfRecordsRead;
    private WorkArea minPayment;
    private WorkArea minTarget;
    private WorkArea numOfDays;
    private TextWorkArea updateProgrProces;
    private Common arrearsOnly;
    private Common miniumPayment;
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
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ReadOnlyCsePerson.
    /// </summary>
    [JsonPropertyName("readOnlyCsePerson")]
    public CsePerson ReadOnlyCsePerson
    {
      get => readOnlyCsePerson ??= new();
      set => readOnlyCsePerson = value;
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
    /// A value of ReadOnlyKsDriversLicense.
    /// </summary>
    [JsonPropertyName("readOnlyKsDriversLicense")]
    public KsDriversLicense ReadOnlyKsDriversLicense
    {
      get => readOnlyKsDriversLicense ??= new();
      set => readOnlyKsDriversLicense = value;
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

    private KsDriversLicense processedCheck;
    private CsePerson readOnlyCsePerson;
    private LegalAction readOnlyLegalAction;
    private KsDriversLicense readOnlyKsDriversLicense;
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
  }
#endregion
}
