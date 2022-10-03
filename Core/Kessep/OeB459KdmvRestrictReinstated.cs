// Program: OE_B459_KDMV_RESTRICT_REINSTATED, ID: 371387319, model: 746.
// Short name: SWEE459B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B459_KDMV_RESTRICT_REINSTATED.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class OeB459KdmvRestrictReinstated: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B459_KDMV_RESTRICT_REINSTATED program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB459KdmvRestrictReinstated(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB459KdmvRestrictReinstated.
  /// </summary>
  public OeB459KdmvRestrictReinstated(IContext context, Import import,
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
    // ***********************************************************************************************
    // DATE		Developer	Description
    // 10/18/2007      DDupree   	Initial Creation - WR280420
    // 11/08/2008      DDupree        Revised the business rules caused us to 
    // take out
    // the logic to create a hist record for a reinstatemet record and put it in
    // the
    // confirmation program like the restriction logic.
    // ***********************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    UseOeB459Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.StartCommon.Count = 1;
    local.CurrentCommon.Count = 1;
    local.CurrentPosition.Count = 1;
    local.FieldNumber.Count = 0;
    local.IncludeArrearsOnly.Flag = "";

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
            if (local.CurrentCommon.Count == 1)
            {
              local.MinimumAmountOwed.TotalCurrency = 0;
            }
            else
            {
              local.WorkArea.Text15 =
                Substring(local.ProgramProcessingInfo.ParameterList,
                local.StartCommon.Count, local.CurrentCommon.Count - 1);
              local.MinimumAmountOwed.TotalCurrency =
                StringToNumber(local.WorkArea.Text15);
            }

            local.StartCommon.Count = local.CurrentPosition.Count + 1;
            local.CurrentCommon.Count = 0;

            break;
          case 2:
            if (local.CurrentCommon.Count == 1)
            {
              local.NumberOfDays.Count = 0;
            }
            else
            {
              local.WorkArea.Text15 =
                Substring(local.ProgramProcessingInfo.ParameterList,
                local.StartCommon.Count, local.CurrentCommon.Count - 1);
              local.NumberOfDays.Count =
                (int)StringToNumber(local.WorkArea.Text15);
            }

            local.StartCommon.Count = local.CurrentPosition.Count + 1;
            local.CurrentCommon.Count = 0;

            break;
          case 3:
            if (local.CurrentCommon.Count == 1)
            {
              local.ArrearsOnly.Flag = "N";
            }
            else
            {
              local.ArrearsOnly.Flag =
                Substring(local.ProgramProcessingInfo.ParameterList,
                local.StartCommon.Count, local.CurrentCommon.Count - 1);
            }

            local.StartCommon.Count = local.CurrentPosition.Count + 1;
            local.CurrentCommon.Count = 0;

            break;
          case 4:
            if (local.CurrentCommon.Count == 1)
            {
              local.NumDaysBetw30DayLtr.Count = 90;
            }
            else
            {
              local.WorkArea.Text15 =
                Substring(local.ProgramProcessingInfo.ParameterList,
                local.StartCommon.Count, local.CurrentCommon.Count - 1);
              local.NumDaysBetw30DayLtr.Count =
                (int)StringToNumber(local.WorkArea.Text15);
            }

            local.StartCommon.Count = local.CurrentPosition.Count + 1;
            local.CurrentCommon.Count = 0;

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
      ++local.CurrentCommon.Count;
    }
    while(!Equal(global.Command, "COMMAND"));

    local.StartDate.Date = local.ProgramProcessingInfo.ProcessDate;
    local.AdministrativeAction.Type1 = "KDMV";
    local.ValidPeriod30DayNotic.Date =
      AddDays(local.StartDate.Date, -local.NumDaysBetw30DayLtr.Count);
    local.RestrictReinstateProcess.Flag =
      Substring(local.ProgramCheckpointRestart.RestartInfo, 1, 1);
    local.NumRestictRead.Text15 =
      Substring(local.ProgramCheckpointRestart.RestartInfo, 2, 15);
    local.NumOfRestrictsRequest.Text15 =
      Substring(local.ProgramCheckpointRestart.RestartInfo, 17, 15);
    local.NumberOfFailedRestrictsWorkArea.Text15 =
      Substring(local.ProgramCheckpointRestart.RestartInfo, 32, 15);
    local.TotalNumberOfErrors.Text15 =
      Substring(local.ProgramCheckpointRestart.RestartInfo, 47, 15);
    local.NumReinstateRead.Text15 =
      Substring(local.ProgramCheckpointRestart.RestartInfo, 62, 15);
    local.NumOfReinstateRequestWorkArea.Text15 =
      Substring(local.ProgramCheckpointRestart.RestartInfo, 77, 15);
    local.ReadOnlyCsePerson.Number =
      Substring(local.ProgramCheckpointRestart.RestartInfo, 92, 10);
    local.NumErrorRecords.Count =
      (int)StringToNumber(local.TotalNumberOfErrors.Text15);
    local.NumOfRestrictRequest.Count =
      (int)StringToNumber(local.NumOfRestrictsRequest.Text15);
    local.NumOfRestrictReads.Count =
      (int)StringToNumber(local.NumRestictRead.Text15);
    local.NumOfReinstateRequestCommon.Count =
      (int)StringToNumber(local.NumOfReinstateRequestWorkArea.Text15);
    local.NumberOfReinstateReads.Count =
      (int)StringToNumber(local.NumReinstateRead.Text15);
    local.NumberOfFailedRestrictsCommon.Count =
      (int)StringToNumber(local.NumberOfFailedRestrictsWorkArea.Text15);
    local.LastUpdateFailedRequest.Count =
      local.NumberOfFailedRestrictsCommon.Count;
    local.LastUpdateReinstateRead.Count = local.NumberOfReinstateReads.Count;
    local.LastUpdateReinstateRequ.Count =
      local.NumOfReinstateRequestCommon.Count;
    local.LastUpdateRestrictRead.Count = local.NumOfRestrictReads.Count;
    local.LastUpdateRestrictRequ.Count = local.NumOfRestrictRequest.Count;
    local.LastUpdatedNumOfErrors.Count = local.NumErrorRecords.Count;
    local.SufValues.Index = -1;

    foreach(var item in ReadCodeValue())
    {
      if (local.SufValues.Index + 1 == Local.SufValuesGroup.Capacity)
      {
        ExitState = "FN0000_GROUP_VIEW_LIMIT_EXCEEDED";

        return;
      }

      ++local.SufValues.Index;
      local.SufValues.CheckSize();

      local.SufValues.Update.SufValues1.Cdvalue = entities.CodeValue.Cdvalue;
    }

    // sorted by validation date
    if (ReadKsDriversLicense3())
    {
      local.LastLicenseProcessedDt.Date =
        entities.KsDriversLicense.ValidationDate;
    }

    if (AsChar(local.RestrictReinstateProcess.Flag) == 'T' || IsEmpty
      (local.RestrictReinstateProcess.Flag))
    {
      local.RestrictReinstateProcess.Flag = "";

      foreach(var item in ReadKsDriversLicenseCsePerson2())
      {
        local.ReadOnlyKsDriversLicense.SequenceCounter =
          entities.ReadOnlyKsDriversLicense.SequenceCounter;

        if (Equal(entities.ReadOnlyCsePerson.Number,
          local.ReadOnlyCsePerson.Number))
        {
          // checking to make sure we have not already processed this AP before
          continue;
        }

        local.ReadOnlyCsePerson.Number = entities.ReadOnlyCsePerson.Number;

        // we still need to do this read just incase the program has been 
        // stopped and restarted, we do not want to process this same AP again,
        // we might end up processing older records, since we can't use the
        // driver's license validation date as a way of keeping track where are
        // in our processing
        if (ReadKsDriversLicense1())
        {
          continue;
        }
        else
        {
          // we have not processed this ap so we will continue processing it
        }

        ExitState = "ACO_NN0000_ALL_OK";

        if (ReadKsDriversLicenseCsePerson1())
        {
          try
          {
            UpdateKsDriversLicense4();
            ++local.NumOfRestrictReads.Count;
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "KS_DRIVERS_LICENSE_NU";
                local.Error.Number = entities.CsePerson.Number;
                ++local.NumErrorRecords.Count;

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "KS_DRIVERS_LICENSE_PV";
                local.Error.Number = entities.CsePerson.Number;
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
            foreach(var item1 in ReadKsDriversLicense4())
            {
              if (ReadLegalAction1())
              {
                local.LegalAction.StandardNumber =
                  entities.LegalAction.StandardNumber;

                // this will actually be read by the legal id stored on the new 
                // table to get the
                // standard order nmber, we did not want to store standard order
                // number because
                // it could get changed and then we would have a hard time  
                // finding the correct
                // legal actions to go with it
              }
              else
              {
                ExitState = "LEGAL_ACTION_NF";
                local.Error.Number = entities.CsePerson.Number;
                ++local.NumErrorRecords.Count;

                goto Test2;
              }

              local.PlaceHolder.Flag = "";

              if (!ReadCaseRole())
              {
                local.PlaceHolder.Flag = "N";
                local.FailureReturnCode.Flag = "K";
              }

              if (Lt(local.ZeroDate.Date, entities.CsePerson.DateOfDeath) && IsEmpty
                (local.PlaceHolder.Flag))
              {
                local.PlaceHolder.Flag = "N";
                local.FailureReturnCode.Flag = "J";
              }

              if (!Equal(entities.N2dRead.ValidationDate,
                local.LastLicenseProcessedDt.Date) && IsEmpty
                (local.PlaceHolder.Flag))
              {
                if (Equal(local.TestDate.ValidationDate, local.ZeroDate.Date))
                {
                  local.TestDate.ValidationDate =
                    entities.N2dRead.ValidationDate;
                }
                else if (!Equal(entities.N2dRead.ValidationDate,
                  local.TestDate.ValidationDate))
                {
                  local.TestDate.ValidationDate = local.ZeroDate.Date;

                  goto ReadEach1;
                }
                else
                {
                }

                local.PlaceHolder.Flag = "N";
                local.FailureReturnCode.Flag = "A";
              }

              // included in the read on the ks-driver's lincese table  is
              //  when the record closure reason is less than or  equal to 
              // spaces
              if (IsEmpty(local.PlaceHolder.Flag))
              {
                if (Equal(local.CsePerson.Number, entities.CsePerson.Number))
                {
                }
                else
                {
                  local.PreviousCsePerson.Number = local.CsePerson.Number;
                  local.CsePerson.Number = entities.CsePerson.Number;
                }

                // If the service complete flag not equal 'Y'
                //   or the service complete date <= null date
                //    <------Next record
                // the check on 'Y'might need to be moved to the read depending 
                // on performance
                // If sevice complete date > batch run date  - 45 days
                //      <--------Next record
                if (Lt(AddDays(local.StartDate.Date, -local.NumberOfDays.Count),
                  entities.N2dRead.ServiceCompleteDate))
                {
                  goto ReadEach1;
                }

                // if the appealed received date > null date and the appeal 
                // resolved flag equal 'O' then go to the next driver's license
                // record
                // if the appeal resolved flag is 'S', 'W' or 'N' then continue 
                // to process otherwise
                // <----------next cse person record
                if (Lt(local.ZeroDate.Date, entities.N2dRead.AppealReceivedDate))
                  
                {
                  if (AsChar(entities.N2dRead.AppealResolved) == 'O')
                  {
                    continue;
                  }

                  if (AsChar(entities.N2dRead.AppealResolved) == 'S' || AsChar
                    (entities.N2dRead.AppealResolved) == 'W' || AsChar
                    (entities.N2dRead.AppealResolved) == 'N')
                  {
                    // continue processing otherwise go to the next person
                  }
                  else
                  {
                    goto ReadEach1;
                  }
                }

                UseOeDetermineDmvCriteria();

                if (!IsEmpty(local.FailureReturnCode.Flag))
                {
                  goto Test1;
                }

                if (AsChar(local.NextPerson.Flag) == 'Y')
                {
                  goto ReadEach1;
                }

                if (AsChar(local.GoToNextCourtOrder.Flag) == 'Y')
                {
                  continue;
                }

                // if the payment agreement date > 30 dayn notice created date
                //    check cash details for collections typeof U, C or I. The 
                // collection date =>
                // payment agreement date and collection date <= 1st payment due
                // date
                local.Local30DayLetterSent.Date =
                  entities.N2dRead.Attribute30DayLetterCreatedDate;
                UseOeDetermineCurrentIwo();

                // we only need to check payments when there is a current wage 
                // withholding or a
                //  payment agreement otherwise they have to pay in full - all 
                // past debt must be paid
                //   - we just need to check for how much they owe if it is zero
                // then they are off the
                // hook otherwise they can still get their driver's license 
                // restricted
                if (AsChar(local.WageWithholdingFound.Flag) == 'Y')
                {
                  // if the wage withholdiing flag = Y then the payment due date
                  // is set to the batch
                  // run date.
                  local.PaymentDueDateFrom.Date = local.IwoCreatedDate.Date;
                  local.PaymentTo.Date = local.StartDate.Date;
                  UseOeDeterminePaymentsForKdmv3();

                  if (local.PaymentAmountMade.TotalCurrency > 0)
                  {
                    local.FailureReturnCode.Flag = "F";

                    goto Test1;
                  }
                }

                if (Lt(local.ZeroDate.Date,
                  entities.N2dRead.PaymentAgreementDate))
                {
                  // the payment due date is set to the first payment due date
                  local.PaymentDueDateFrom.Date =
                    entities.N2dRead.PaymentAgreementDate;
                  local.PaymentTo.Date = entities.N2dRead.FirstPaymentDueDate;
                  UseOeDeterminePaymentsForKdmv3();

                  if (!Lt(local.PaymentAmountMade.TotalCurrency,
                    entities.N2dRead.AmountDue))
                  {
                    // we are actually comparing it to the agreed amount due 
                    // with what was actaully
                    // paid in.
                    // this is stored on the new table
                    local.FailureReturnCode.Flag = "F";

                    goto Test1;
                  }

                  if (Lt(local.ZeroDate.Date,
                    entities.N2dRead.PaymentAgreementDate))
                  {
                    if (Lt(local.StartDate.Date,
                      entities.N2dRead.FirstPaymentDueDate))
                    {
                      goto ReadEach1;
                    }
                  }
                }

                UseFnComputeTotalsForCtOrder3();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  local.Error.Number = entities.CsePerson.Number;
                  ++local.NumErrorRecords.Count;

                  goto Test2;
                }

                local.OwedAmount.TotalCurrency =
                  local.ScreenOwedAmountsDtl.TotalArrearsOwed + local
                  .ScreenOwedAmountsDtl.TotalInterestOwed;

                if (local.OwedAmount.TotalCurrency >= local
                  .MinimumAmountOwed.TotalCurrency)
                {
                  // keep processing
                }
                else
                {
                  local.FailureReturnCode.Flag = "G";

                  goto Test1;

                  // go to the next court order
                }

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  if (local.OwedAmount.TotalCurrency >= local
                    .MinimumAmountOwed.TotalCurrency)
                  {
                    // To see if any payments have been made that are still in 
                    // cash receipt and have
                    // not made to collections yet. There has not been enough 
                    // money in the collection
                    // table to meet the minium payment
                    if (!Equal(entities.CsePerson.Number,
                      local.AlreadyCheckedSsn.Number))
                    {
                      if (!IsEmpty(local.CsePersonsWorkSet.Ssn))
                      {
                        local.PreviousProcessCsePersonsWorkSet.Assign(
                          local.CsePersonsWorkSet);
                        MoveLegalAction(local.LegalAction,
                          local.PreviousLegalAction);
                        local.PreviousPersonTotal.TotalCurrency =
                          local.PersonTotal.TotalCurrency;
                      }

                      local.PersonTotal.TotalCurrency = 0;
                      local.Search.Flag = "3";
                      local.Phonetic.Percentage = 100;
                      local.StartCsePersonsWorkSet.Number =
                        entities.CsePerson.Number;
                      local.AlreadyCheckedSsn.Number =
                        entities.CsePerson.Number;
                      MoveCsePersonsWorkSet1(local.ClearCsePersonsWorkSet,
                        local.CsePersonsWorkSet);
                      UseSiReadCsePersonBatch();

                      if (!IsExitState("ACO_NN0000_ALL_OK"))
                      {
                        local.Error.Number = entities.CsePerson.Number;
                        ++local.NumErrorRecords.Count;

                        goto Test2;
                      }

                      MoveLegalAction(entities.LegalAction, local.LegalAction);

                      // we will now try to scrape off any suffix the last name 
                      // might have.
                      UseOeScrubSuffixes();
                    }

                    if (!Lt(local.ZeroDate.Date, local.CsePersonsWorkSet.Dob) ||
                      IsEmpty(local.CsePersonsWorkSet.Ssn) || Equal
                      (local.CsePersonsWorkSet.Ssn, "000000000"))
                    {
                      if (!Lt(local.ZeroDate.Date, local.CsePersonsWorkSet.Dob))
                      {
                        local.FailureReturnCode.Flag = "H";

                        goto Test1;
                      }

                      if (IsEmpty(local.CsePersonsWorkSet.Ssn) || Equal
                        (local.CsePersonsWorkSet.Ssn, "000000000"))
                      {
                        local.FailureReturnCode.Flag = "I";

                        goto Test1;
                      }
                    }

                    if (!Equal(entities.CsePerson.Number,
                      local.LastCsePersonsWorkSet.Number))
                    {
                      local.PreviousProcessCsePersonsWorkSet.Assign(
                        local.LastCsePersonsWorkSet);
                      local.PreviousProcessKsDriversLicense.KsDvrLicense =
                        local.LastKsDriversLicense.KsDvrLicense;
                    }

                    if (!Equal(local.PreviousProcessCsePersonsWorkSet.Number,
                      local.AlreadyWritten.Number) && IsExitState
                      ("ACO_NN0000_ALL_OK") && !
                      IsEmpty(local.PreviousProcessCsePersonsWorkSet.Number))
                    {
                      // we will using this area to write the sancation request 
                      // to kdmv
                      // we only want to write one record for either one 
                      // reguardless of how many court
                      // orders the obligor has that qualify to be restriction
                      // we only want to write out one record per obligor
                      local.DateWorkArea.Year =
                        Year(local.PreviousProcessCsePersonsWorkSet.Dob);
                      local.WorkArea.Text15 =
                        NumberToString(local.DateWorkArea.Year, 15);
                      local.Year.Text4 =
                        Substring(local.WorkArea.Text15, 12, 4);
                      local.DateWorkArea.Month =
                        Month(local.PreviousProcessCsePersonsWorkSet.Dob);
                      local.WorkArea.Text15 =
                        NumberToString(local.DateWorkArea.Month, 15);
                      local.Month.Text2 =
                        Substring(local.WorkArea.Text15, 14, 2);
                      local.DateWorkArea.Day =
                        Day(local.PreviousProcessCsePersonsWorkSet.Dob);
                      local.WorkArea.Text15 =
                        NumberToString(local.DateWorkArea.Day, 15);
                      local.Day.Text2 = Substring(local.WorkArea.Text15, 14, 2);
                      local.DateWorkArea.TextDate = local.Year.Text4 + local
                        .Month.Text2 + local.Day.Text2;
                      local.KdmvFile.FileType = "1";
                      local.KdmvFile.Dob = local.DateWorkArea.TextDate;
                      local.KdmvFile.Ssn =
                        local.PreviousProcessCsePersonsWorkSet.Ssn;
                      local.KdmvFile.DriverLicenseNumber =
                        local.PreviousProcessKsDriversLicense.KsDvrLicense ?? Spaces
                        (9);
                      local.KdmvFile.CsePersonNumber =
                        local.PreviousProcessCsePersonsWorkSet.Number;
                      local.KdmvFile.LastName =
                        local.PreviousProcessCsePersonsWorkSet.LastName;
                      local.KdmvFile.FirstName =
                        local.PreviousProcessCsePersonsWorkSet.FirstName;
                      local.DateWorkArea.Year = Year(local.StartDate.Date);
                      local.WorkArea.Text15 =
                        NumberToString(local.DateWorkArea.Year, 15);
                      local.Year.Text4 =
                        Substring(local.WorkArea.Text15, 12, 4);
                      local.DateWorkArea.Month = Month(local.StartDate.Date);
                      local.WorkArea.Text15 =
                        NumberToString(local.DateWorkArea.Month, 15);
                      local.Month.Text2 =
                        Substring(local.WorkArea.Text15, 14, 2);
                      local.DateWorkArea.Day = Day(local.StartDate.Date);
                      local.WorkArea.Text15 =
                        NumberToString(local.DateWorkArea.Day, 15);
                      local.Day.Text2 = Substring(local.WorkArea.Text15, 14, 2);
                      local.DateWorkArea.TextDate = local.Year.Text4 + local
                        .Month.Text2 + local.Day.Text2;
                      local.KdmvFile.ProcessDate = local.DateWorkArea.TextDate;
                      local.PassArea.FileInstruction = "WRITE";

                      // write physical record to file
                      UseOeEabKdmvRestrictReinstate();

                      if (!Equal(local.PassArea.TextReturnCode, "OK"))
                      {
                        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
                        local.Error.Number = entities.CsePerson.Number;
                        ++local.NumErrorRecords.Count;

                        goto Test2;
                      }

                      local.PersonSend.Flag = "";
                      ++local.NumOfRestrictRequest.Count;
                      ++local.RecordCount.Count;

                      if (local.RecordCount.Count >= local
                        .ProgramCheckpointRestart.UpdateFrequencyCount.
                          GetValueOrDefault())
                      {
                        local.ProgramCheckpointRestart.RestartInd = "Y";
                        local.ProgramCheckpointRestart.ProgramName =
                          local.ProgramProcessingInfo.Name;
                        local.ProgramCheckpointRestart.CheckpointCount =
                          local.ReadOnlyKsDriversLicense.SequenceCounter - 1;
                        local.NumRestictRead.Text15 =
                          NumberToString(local.NumOfRestrictReads.Count, 15);
                        local.NumOfRestrictsRequest.Text15 =
                          NumberToString(local.NumOfRestrictRequest.Count, 15);
                        local.NumberOfFailedRestrictsWorkArea.Text15 =
                          NumberToString(local.NumberOfFailedRestrictsCommon.
                            Count, 15);
                        local.TotalNumberOfErrors.Text15 =
                          NumberToString(local.NumErrorRecords.Count, 15);
                        local.NumReinstateRead.Text15 =
                          NumberToString(local.NumberOfReinstateReads.Count, 15);
                          
                        local.NumOfReinstateRequestWorkArea.Text15 =
                          NumberToString(local.NumOfReinstateRequestCommon.
                            Count, 15);
                        local.RestrictReinstateProcess.Flag = "T";
                        local.ProgramCheckpointRestart.RestartInfo =
                          local.NumRestictRead.Text15 + local
                          .NumOfRestrictsRequest.Text15 + local
                          .NumberOfFailedRestrictsWorkArea.Text15;
                        local.ProgramCheckpointRestart.RestartInfo =
                          local.RestrictReinstateProcess.Flag + TrimEnd
                          (local.ProgramCheckpointRestart.RestartInfo) + local
                          .TotalNumberOfErrors.Text15 + local
                          .NumReinstateRead.Text15 + local
                          .NumOfReinstateRequestWorkArea.Text15;
                        local.ProgramCheckpointRestart.RestartInfo =
                          (local.ProgramCheckpointRestart.RestartInfo ?? "") + local
                          .PreviousProcessCsePersonsWorkSet.Number;
                        UseUpdatePgmCheckpointRestart();

                        if (!IsExitState("ACO_NN0000_ALL_OK"))
                        {
                          return;
                        }

                        local.LastUpdateFailedRequest.Count =
                          local.NumberOfFailedRestrictsCommon.Count;
                        local.LastUpdateReinstateRead.Count =
                          local.NumberOfReinstateReads.Count;
                        local.LastUpdateReinstateRequ.Count =
                          local.NumOfReinstateRequestCommon.Count;
                        local.LastUpdateRestrictRead.Count =
                          local.NumOfRestrictReads.Count;
                        local.LastUpdateRestrictRequ.Count =
                          local.NumOfRestrictRequest.Count;
                        local.LastUpdatedNumOfErrors.Count =
                          local.NumErrorRecords.Count;
                        UseExtToDoACommit();

                        if (local.PassArea.NumericReturnCode != 0)
                        {
                          ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

                          return;
                        }

                        local.RecordCount.Count = 0;
                        local.EabFileHandling.Action = "WRITE";
                        local.EabReportSend.RptDetail = "Commit Taken: " + NumberToString
                          (TimeToInt(Time(Now())), 15);
                        UseCabErrorReport();

                        if (!Equal(local.EabFileHandling.Status, "OK"))
                        {
                          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                          return;
                        }

                        local.RestrictReinstateProcess.Flag = "";
                      }

                      local.AlreadyWritten.Number =
                        local.PreviousProcessCsePersonsWorkSet.Number;
                      local.LastSuccessfulProcessed.Assign(
                        local.CsePersonsWorkSet);
                      MoveLegalAction(local.LegalAction,
                        local.LastSuccesfulProcessed);
                    }
                    else if (!Equal(entities.CsePerson.Number,
                      local.AlreadyWritten.Number) && IsExitState
                      ("ACO_NN0000_ALL_OK"))
                    {
                      local.LastSuccessfulProcessed.Assign(
                        local.CsePersonsWorkSet);
                      MoveLegalAction(local.LegalAction,
                        local.LastSuccesfulProcessed);
                    }

                    // we will now update the ks driver license record with the 
                    // amount this court order
                    // owes and the date of the request is being sent to dmv for
                    // restriction.
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
                          local.Error.Number = entities.CsePerson.Number;
                          ++local.NumErrorRecords.Count;

                          goto Test2;
                        case ErrorCode.PermittedValueViolation:
                          ExitState = "KS_DRIVERS_LICENSE_PV";
                          local.Error.Number = entities.CsePerson.Number;
                          ++local.NumErrorRecords.Count;

                          goto Test2;
                        case ErrorCode.DatabaseError:
                          break;
                        default:
                          throw;
                      }
                    }

                    if (!Equal(entities.CsePerson.Number,
                      local.LastCsePersonsWorkSet.Number))
                    {
                      local.LastCsePersonsWorkSet.
                        Assign(local.CsePersonsWorkSet);
                      local.LastKsDriversLicense.KsDvrLicense =
                        entities.N2dRead.KsDvrLicense;
                    }

                    local.PersonSend.Flag = "Y";

                    // we do not do a hist record since we are just asking them 
                    // to restrict the license,
                    // we will do a hist record and oblo record when we recieve 
                    // confrimation that dmv
                    // has restricted a license.
                  }
                  else
                  {
                    continue;
                  }
                }
              }

Test1:

              if (!IsEmpty(local.FailureReturnCode.Flag))
              {
                // WRITE RECORD CLOSE MESSAGE HERE, we will write only one close
                // message at a time,  we have to determing it for each court 
                // order then write out
                // the error message
                switch(AsChar(local.FailureReturnCode.Flag))
                {
                  case 'A':
                    local.RecordError.Text15 = "INVALID DRV LIC";
                    local.ErrorReason.Text30 = "INVALID DRIVER'S LICENSE";

                    break;
                  case 'B':
                    local.RecordError.Text15 = "INCARCERATED";
                    local.ErrorReason.Text30 = "AP IS CURRENTLY INCARCERATED";

                    break;
                  case 'C':
                    local.RecordError.Text15 = "EXEMPTION";
                    local.ErrorReason.Text30 = "AN EXEMPTION HAS BEEN SET";

                    break;
                  case 'D':
                    local.RecordError.Text15 = "GOOD CAUSE";
                    local.ErrorReason.Text30 = "GOOD CAUSE/PENDING STATUS";

                    break;
                  case 'E':
                    local.RecordError.Text15 = "INVALID ADDRESS";
                    local.ErrorReason.Text30 = "NO ACTIVE ADDRESS";

                    break;
                  case 'F':
                    local.RecordError.Text15 = "MADE PAYMENT";
                    local.ErrorReason.Text30 = "AP HAS MADE PAYMENT";

                    break;
                  case 'G':
                    local.RecordError.Text15 = "ARREARS BELOW";
                    local.ErrorReason.Text30 = "ARREARS BELOW MINIMUM AMOUNT";

                    break;
                  case 'H':
                    local.RecordError.Text15 = "NO DOB";
                    local.ErrorReason.Text30 = "AP HAS NO DOB";

                    break;
                  case 'I':
                    local.RecordError.Text15 = "NO SSN";
                    local.ErrorReason.Text30 = "AP HAS NO SSN";

                    break;
                  case 'J':
                    local.RecordError.Text15 = "AP DECEASED";
                    local.ErrorReason.Text30 = "AP IS DECEASED";

                    break;
                  case 'K':
                    local.RecordError.Text15 = "NO ACTIVE ROLE";
                    local.ErrorReason.Text30 = "NO ACTIVE AP ROLE";

                    break;
                  default:
                    break;
                }

                // UPDATE KS DRIVER'S LICENSE RECORD WITH THE ERROR MESSAGE
                // THAT STOP THE COURT ORDER FROM BEING RESTRICTED
                try
                {
                  UpdateKsDriversLicense2();
                  ++local.NumberOfFailedRestrictsCommon.Count;
                }
                catch(Exception e)
                {
                  switch(GetErrorCode(e))
                  {
                    case ErrorCode.AlreadyExists:
                      ExitState = "KS_DRIVERS_LICENSE_NU";
                      local.Error.Number = entities.CsePerson.Number;
                      ++local.NumErrorRecords.Count;

                      goto Test2;
                    case ErrorCode.PermittedValueViolation:
                      ExitState = "KS_DRIVERS_LICENSE_PV";
                      local.Error.Number = entities.CsePerson.Number;
                      ++local.NumErrorRecords.Count;

                      goto Test2;
                    case ErrorCode.DatabaseError:
                      break;
                    default:
                      throw;
                  }
                }

                local.Infrastructure.SituationNumber = 0;
                local.Infrastructure.ReasonCode = "UNPROCRESTRICT";
                local.Infrastructure.EventId = 1;
                local.Infrastructure.EventType = "ADMINACT";
                local.Infrastructure.ProcessStatus = "Q";
                local.Infrastructure.UserId = "KDMV";
                local.Infrastructure.BusinessObjectCd = "ENF";
                local.Infrastructure.ReferenceDate = local.StartDate.Date;
                local.Infrastructure.CreatedBy =
                  local.ProgramProcessingInfo.Name;
                local.Infrastructure.EventDetailName =
                  "UNPROCESSED RESTRICTION";
                local.Infrastructure.CsePersonNumber =
                  entities.CsePerson.Number;
                local.Infrastructure.DenormNumeric12 =
                  entities.LegalAction.Identifier;
                local.Infrastructure.DenormText12 =
                  entities.LegalAction.CourtCaseNumber;
                local.Detail.Text11 = ", Reason :";
                local.Infrastructure.Detail = "Court Order # " + TrimEnd
                  (entities.LegalAction.StandardNumber) + local
                  .Detail.Text11 + local.ErrorReason.Text30;

                // This is for history record, it is not needed unless we add a 
                // record but if a case
                // can not be found for the current obligor and the current 
                // court order then it should
                // be errored out now not after we add other records and alerts.
                local.Infrastructure.CaseNumber = "";
                local.Infrastructure.CaseUnitNumber = 0;

                foreach(var item2 in ReadCaseCaseUnitCaseRole())
                {
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

                  goto Test2;
                }

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
                  local.Error.Number = entities.CsePerson.Number;
                  ++local.NumErrorRecords.Count;

                  goto Test2;
                }

                local.FailureReturnCode.Flag = "";
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
          }
        }

ReadEach1:
        ;
      }
    }

    if (AsChar(local.PersonSend.Flag) == 'Y' && IsExitState
      ("ACO_NN0000_ALL_OK"))
    {
      // we only want to write out one record per obligor
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
      local.KdmvFile.FileType = "1";
      local.KdmvFile.Dob = local.DateWorkArea.TextDate;
      local.KdmvFile.Ssn = local.LastCsePersonsWorkSet.Ssn;
      local.KdmvFile.DriverLicenseNumber =
        local.LastKsDriversLicense.KsDvrLicense ?? Spaces(9);
      local.KdmvFile.CsePersonNumber = local.LastCsePersonsWorkSet.Number;
      local.KdmvFile.LastName = local.LastCsePersonsWorkSet.LastName;
      local.KdmvFile.FirstName = local.LastCsePersonsWorkSet.FirstName;
      local.DateWorkArea.Year = Year(local.StartDate.Date);
      local.WorkArea.Text15 = NumberToString(local.DateWorkArea.Year, 15);
      local.Year.Text4 = Substring(local.WorkArea.Text15, 12, 4);
      local.DateWorkArea.Month = Month(local.StartDate.Date);
      local.WorkArea.Text15 = NumberToString(local.DateWorkArea.Month, 15);
      local.Month.Text2 = Substring(local.WorkArea.Text15, 14, 2);
      local.DateWorkArea.Day = Day(local.StartDate.Date);
      local.WorkArea.Text15 = NumberToString(local.DateWorkArea.Day, 15);
      local.Day.Text2 = Substring(local.WorkArea.Text15, 14, 2);
      local.DateWorkArea.TextDate = local.Year.Text4 + local.Month.Text2 + local
        .Day.Text2;
      local.KdmvFile.ProcessDate = local.DateWorkArea.TextDate;
      local.PassArea.FileInstruction = "WRITE";

      // write physical record to file
      UseOeEabKdmvRestrictReinstate();

      if (!Equal(local.PassArea.TextReturnCode, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
        local.Error.Number = entities.CsePerson.Number;
        ++local.NumErrorRecords.Count;

        goto Test3;
      }

      ++local.NumOfRestrictRequest.Count;
    }

Test3:

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      foreach(var item in ReadKsDriversLicense7())
      {
        ExitState = "ACO_NN0000_ALL_OK";

        // we need to clear the  processing ind now that we have successfully
        // completed the restriction part of the program, we will then begin the
        // reinstatement part.
        try
        {
          UpdateKsDriversLicense5();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              if (ReadCsePerson1())
              {
                ExitState = "KS_DRIVERS_LICENSE_NU";
                local.Error.Number = entities.CsePerson.Number;
                ++local.NumErrorRecords.Count;
              }

              goto Test5;
            case ErrorCode.PermittedValueViolation:
              if (ReadCsePerson1())
              {
                ExitState = "KS_DRIVERS_LICENSE_PV";
                local.Error.Number = entities.CsePerson.Number;
                ++local.NumErrorRecords.Count;
              }

              goto Test5;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

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
        }
      }

      if (AsChar(local.RestrictReinstateProcess.Flag) == 'N' || IsEmpty
        (local.RestrictReinstateProcess.Flag))
      {
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

        local.PreviousProcessCsePersonsWorkSet.Assign(
          local.ClearCsePersonsWorkSet);
        local.LastCsePersonsWorkSet.Assign(local.ClearCsePersonsWorkSet);

        if (AsChar(local.RestrictReinstateProcess.Flag) == 'N')
        {
          // this would be a restart in the reinstate part of the program so we 
          // would want to
          // retain where the program would restart otherwise we will set it to 
          // the beginning in
          // the first of the process
        }
        else
        {
          local.ReadOnlyKsDriversLicense.SequenceCounter = 0;
        }

        // we are finished with the restriction process
        // and now we start the reinstatement process
        foreach(var item in ReadKsDriversLicense6())
        {
          local.ReadOnlyKsDriversLicense.SequenceCounter =
            entities.ReadOnlyKsDriversLicense.SequenceCounter;

          if (ReadCsePerson2())
          {
            local.ReadOnlyCsePerson.Number = entities.CsePerson.Number;
          }

          // handle changing from one ap to another if that previous person was 
          // successfully  completed
          if (AsChar(local.CtOrderReinstate.Flag) == 'Y')
          {
            local.PreviousCsePerson.Number = local.CurrentCsePerson.Number;

            for(local.ReinstatedPrev.Index = 0; local.ReinstatedPrev.Index < local
              .ReinstatedPrev.Count; ++local.ReinstatedPrev.Index)
            {
              if (!local.ReinstatedPrev.CheckSize())
              {
                break;
              }

              local.ReinstatedPrev.Update.ReinstatedPrevCommon.TotalCurrency =
                local.ClearCommon.TotalCurrency;
              local.ReinstatedPrev.Update.ReinstatedPrevKsDriversLicense.
                CreatedTstamp = local.ClearKsDriversLicense.CreatedTstamp;
              MoveLegalAction(local.ClearLegalAction,
                local.ReinstatedPrev.Update.ReinstatedPrevLegalAction);
              local.ReinstatedPrev.Update.ReinstatedPrevKdmvFile.Assign(
                local.ClearKdmvFile);
              local.ReinstatedPrev.Update.ReinstatedCodePrev.Flag =
                local.ClearReinstateCode.Flag;
            }

            local.ReinstatedPrev.CheckIndex();
            local.ReinstatedPrev.Index = -1;
            local.ReinstatedPrev.Count = 0;

            for(local.Reinstated.Index = 0; local.Reinstated.Index < local
              .Reinstated.Count; ++local.Reinstated.Index)
            {
              if (!local.Reinstated.CheckSize())
              {
                break;
              }

              local.ReinstatedPrev.Index = local.Reinstated.Index;
              local.ReinstatedPrev.CheckSize();

              local.ReinstatedPrev.Update.ReinstatedPrevKdmvFile.Assign(
                local.Reinstated.Item.ReinstatedKdmvFile);
              MoveLegalAction(local.Reinstated.Item.ReinstatedLegalAction,
                local.ReinstatedPrev.Update.ReinstatedPrevLegalAction);
              local.ReinstatedPrev.Update.ReinstatedPrevCommon.TotalCurrency =
                local.Reinstated.Item.ReinstatedCommon.TotalCurrency;
              local.ReinstatedPrev.Update.ReinstatedPrevKsDriversLicense.
                CreatedTstamp =
                  local.Reinstated.Item.ReinstatedKsDriversLicense.
                  CreatedTstamp;
              local.ReinstatedPrev.Update.ReinstatedCodePrev.Flag =
                local.Reinstated.Item.ReinstatedCode.Flag;
            }

            local.Reinstated.CheckIndex();
          }

          for(local.Reinstated.Index = 0; local.Reinstated.Index < local
            .Reinstated.Count; ++local.Reinstated.Index)
          {
            if (!local.Reinstated.CheckSize())
            {
              break;
            }

            local.Reinstated.Update.ReinstatedCommon.TotalCurrency =
              local.ClearCommon.TotalCurrency;
            local.Reinstated.Update.ReinstatedKsDriversLicense.CreatedTstamp =
              local.ClearKsDriversLicense.CreatedTstamp;
            MoveLegalAction(local.ClearLegalAction,
              local.Reinstated.Update.ReinstatedLegalAction);
            local.Reinstated.Update.ReinstatedKdmvFile.Assign(
              local.ClearKdmvFile);
            local.Reinstated.Update.ReinstatedCode.Flag =
              local.ClearReinstateCode.Flag;
          }

          local.Reinstated.CheckIndex();
          local.Reinstated.Index = -1;
          local.Reinstated.Count = 0;
          local.CtOrderReinstate.Flag = "";

          // checking to make sure we have not already processed this record 
          // before
          if (ReadKsDriversLicense1())
          {
            continue;
          }
          else
          {
            // we have not processed this ap so we will continue processing it
          }

          ++local.NumberOfReinstateReads.Count;
          local.PrevApTotalCtOrder.Count = local.ApTotalCtOrder.Count;
          local.PrevNumCtOrdReinstated.Count =
            local.NumCtOrdersReinstated.Count;
          local.ApTotalCtOrder.Count = 0;
          local.NumCtOrdersReinstated.Count = 0;

          foreach(var item1 in ReadKsDriversLicense5())
          {
            try
            {
              UpdateKsDriversLicense4();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "KS_DRIVERS_LICENSE_NU";
                  local.Error.Number = entities.CsePerson.Number;
                  ++local.NumErrorRecords.Count;

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "KS_DRIVERS_LICENSE_PV";
                  local.Error.Number = entities.CsePerson.Number;
                  ++local.NumErrorRecords.Count;

                  break;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }

            local.ReinstateReasonCode.Flag = "";
            local.CtOrderReinstate.Flag = "";
            ++local.ApTotalCtOrder.Count;

            if (ReadLegalAction2())
            {
              local.LegalAction.StandardNumber =
                entities.LegalAction.StandardNumber;

              // this will actually be read by the legal id stored on the new 
              // table to get the
              // standard order nmber, we did not want to store standard order 
              // number because
              // it could get changed and then we would have a hard time  
              // finding the correct
              // legal actions to go with it
            }
            else
            {
              ExitState = "LEGAL_ACTION_NF";
              local.Error.Number = entities.CsePerson.Number;
              ++local.NumErrorRecords.Count;

              break;
            }

            if (ReadObligationAdmActionExemption())
            {
              local.CtOrderReinstate.Flag = "Y";
              local.ReinstateReasonCode.Flag = "A";

              // next legal action
            }

            if (AsChar(local.CtOrderReinstate.Flag) != 'Y')
            {
              local.Local30DayLetterSent.Date =
                entities.KsDriversLicense.Attribute30DayLetterCreatedDate;
              UseOeDetermineCurrentIwo();

              if (AsChar(local.WageWithholdingFound.Flag) == 'Y')
              {
                // if the wage withholdiing flag = Y then the payment due date 
                // is set to the batch
                // run date.
                local.PaymentDueDateFrom.Date = local.IwoCreatedDate.Date;
                local.PaymentTo.Date = local.StartDate.Date;
                local.ReinstateProcess.Flag = "Y";
                UseOeDeterminePaymentsForKdmv1();

                if (local.PaymentAmountMade.TotalCurrency > 0)
                {
                  local.CtOrderReinstate.Flag = "Y";
                  local.ReinstateReasonCode.Flag = "B";

                  goto Test4;
                }
              }

              if (Lt(local.ZeroDate.Date,
                entities.KsDriversLicense.PaymentAgreementDate))
              {
                // the payment due date is set to the first payment due date
                local.PaymentDueDateFrom.Date =
                  entities.KsDriversLicense.PaymentAgreementDate;
                local.PaymentTo.Date =
                  entities.KsDriversLicense.FirstPaymentDueDate;
                local.ReinstateProcess.Flag = "Y";
                UseOeDeterminePaymentsForKdmv1();

                if (!Lt(local.PaymentAmountMade.TotalCurrency,
                  entities.KsDriversLicense.AmountDue))
                {
                  // the paid in amount >= agreed amount and the agreed amount 
                  // is > 0
                  // this is stored on the new table
                  // we are actually comparing it to the agreed amount due with 
                  // what was actaully
                  // paid in.
                  // this is stored on the new table
                  local.CtOrderReinstate.Flag = "Y";
                  local.ReinstateReasonCode.Flag = "C";

                  goto Test4;
                }
              }

              UseFnComputeTotalsForCtOrder4();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                local.Error.Number = entities.CsePerson.Number;
                ++local.NumErrorRecords.Count;

                break;
              }

              local.OwedAmount.TotalCurrency =
                local.ScreenOwedAmountsDtl.TotalArrearsOwed + local
                .ScreenOwedAmountsDtl.TotalInterestOwed;

              if (local.OwedAmount.TotalCurrency == 0)
              {
                // keep processing
                local.CtOrderReinstate.Flag = "Y";
                local.ReinstateReasonCode.Flag = "D";
              }
            }

Test4:

            if (AsChar(local.CtOrderReinstate.Flag) == 'Y')
            {
              if (!Equal(entities.CsePerson.Number,
                local.AlreadyCheckedSsn.Number))
              {
                if (!IsEmpty(local.CsePersonsWorkSet.Ssn))
                {
                  MoveLegalAction(local.LegalAction, local.PreviousLegalAction);
                  local.PreviousPersonTotal.TotalCurrency =
                    local.PersonTotal.TotalCurrency;
                }

                local.PersonTotal.TotalCurrency = 0;
                local.Search.Flag = "3";
                local.Phonetic.Percentage = 100;
                local.StartCsePersonsWorkSet.Number = entities.CsePerson.Number;
                local.AlreadyCheckedSsn.Number = entities.CsePerson.Number;
                MoveCsePersonsWorkSet1(local.ClearCsePersonsWorkSet,
                  local.CsePersonsWorkSet);
                UseSiReadCsePersonBatch();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  local.Error.Number = entities.CsePerson.Number;
                  ++local.NumErrorRecords.Count;

                  break;
                }

                MoveLegalAction(entities.LegalAction, local.LegalAction);

                // we will now try to scrape off any suffix the last name might 
                // have.
                UseOeScrubSuffixes();
              }

              ++local.Reinstated.Index;
              local.Reinstated.CheckSize();

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
              local.Reinstated.Update.ReinstatedKdmvFile.Dob =
                local.DateWorkArea.TextDate;
              local.Reinstated.Update.ReinstatedKdmvFile.CsePersonNumber =
                entities.CsePerson.Number;
              local.Reinstated.Update.ReinstatedKdmvFile.Ssn =
                local.CsePersonsWorkSet.Ssn;
              local.Reinstated.Update.ReinstatedKdmvFile.DriverLicenseNumber =
                entities.KsDriversLicense.KsDvrLicense ?? Spaces(9);
              local.Reinstated.Update.ReinstatedKdmvFile.LastName =
                local.CsePersonsWorkSet.LastName;
              local.Reinstated.Update.ReinstatedKdmvFile.FirstName =
                local.CsePersonsWorkSet.FirstName;
              local.Reinstated.Update.ReinstatedLegalAction.Identifier =
                entities.LegalAction.Identifier;
              local.Reinstated.Update.ReinstatedLegalAction.StandardNumber =
                entities.LegalAction.StandardNumber;
              local.Reinstated.Update.ReinstatedCommon.TotalCurrency =
                local.OwedAmount.TotalCurrency;
              local.Reinstated.Update.ReinstatedKsDriversLicense.CreatedTstamp =
                entities.KsDriversLicense.CreatedTstamp;
              local.Reinstated.Update.ReinstatedCode.Flag =
                local.ReinstateReasonCode.Flag;
              ++local.NumCtOrdersReinstated.Count;
              local.ApReinstate.Flag = "Y";
            }

            if (local.ApTotalCtOrder.Count != local.NumCtOrdersReinstated.Count)
            {
              for(local.Reinstated.Index = 0; local.Reinstated.Index < local
                .Reinstated.Count; ++local.Reinstated.Index)
              {
                if (!local.Reinstated.CheckSize())
                {
                  break;
                }

                local.Reinstated.Update.ReinstatedCommon.TotalCurrency =
                  local.ClearCommon.TotalCurrency;
                local.Reinstated.Update.ReinstatedKsDriversLicense.
                  CreatedTstamp = local.ClearKsDriversLicense.CreatedTstamp;
                MoveLegalAction(local.ClearLegalAction,
                  local.Reinstated.Update.ReinstatedLegalAction);
                local.Reinstated.Update.ReinstatedKdmvFile.Assign(
                  local.ClearKdmvFile);
                local.Reinstated.Update.ReinstatedCode.Flag =
                  local.ClearReinstateCode.Flag;
              }

              local.Reinstated.CheckIndex();
              local.Reinstated.Index = -1;
              local.Reinstated.Count = 0;

              goto ReadEach3;
            }

            if (!Equal(entities.CsePerson.Number,
              local.LastCsePersonsWorkSet.Number))
            {
              local.PreviousProcessCsePersonsWorkSet.Number =
                local.LastCsePersonsWorkSet.Number;
              local.LastCsePersonsWorkSet.Number = entities.CsePerson.Number;
            }

            if (!Equal(local.PreviousProcessCsePersonsWorkSet.Number,
              local.AlreadyWritten.Number) && IsExitState
              ("ACO_NN0000_ALL_OK") && !
              IsEmpty(local.PreviousProcessCsePersonsWorkSet.Number))
            {
              // at this point the court order meets the critieria to have the 
              // driver's license reinstated
              for(local.ReinstatedPrev.Index = 0; local.ReinstatedPrev.Index < local
                .ReinstatedPrev.Count; ++local.ReinstatedPrev.Index)
              {
                if (!local.ReinstatedPrev.CheckSize())
                {
                  break;
                }

                // we are now just capturing why we are reinstating a record. 
                // when it is confrimed
                // in sweeb461 we will write a history record with the reason 
                // for the reinstatement
                local.Detail.Text32 = "";

                if (AsChar(local.ReinstatedPrev.Item.ReinstatedCodePrev.Flag) ==
                  'A')
                {
                  local.Detail.Text32 = " - Obligation Exempted";
                }
                else if (AsChar(local.ReinstatedPrev.Item.ReinstatedCodePrev.
                  Flag) == 'B')
                {
                  local.Detail.Text32 = " - Wage Withholding Established";
                }
                else if (AsChar(local.ReinstatedPrev.Item.ReinstatedCodePrev.
                  Flag) == 'C')
                {
                  local.Detail.Text32 = "- Pmt made per Payment Agreement";
                }
                else if (AsChar(local.ReinstatedPrev.Item.ReinstatedCodePrev.
                  Flag) == 'D')
                {
                  local.Detail.Text32 = " - Debt Amount Paid in Full";
                }

                // **********************************************************************************
                // Update the KDMV record with the reinstatement date
                // **********************************************************************************
                if (ReadKsDriversLicense2())
                {
                  try
                  {
                    UpdateKsDriversLicense3();
                  }
                  catch(Exception e)
                  {
                    switch(GetErrorCode(e))
                    {
                      case ErrorCode.AlreadyExists:
                        ExitState = "KS_DRIVERS_LICENSE_NU";
                        local.Error.Number =
                          local.ReinstatedPrev.Item.ReinstatedPrevKdmvFile.
                            CsePersonNumber;
                        ++local.NumErrorRecords.Count;

                        goto ReadEach2;
                      case ErrorCode.PermittedValueViolation:
                        ExitState = "KS_DRIVERS_LICENSE_PV";
                        local.Error.Number =
                          local.ReinstatedPrev.Item.ReinstatedPrevKdmvFile.
                            CsePersonNumber;
                        ++local.NumErrorRecords.Count;

                        goto ReadEach2;
                      case ErrorCode.DatabaseError:
                        break;
                      default:
                        throw;
                    }
                  }
                }
              }

              local.ReinstatedPrev.CheckIndex();

              // we  have to make sure that all the court orders pass before we 
              // send a request to to take the obligors off the list
              // we only want to write out one record per obligor
              for(local.ReinstatedPrev.Index = 0; local.ReinstatedPrev.Index < local
                .ReinstatedPrev.Count; ++local.ReinstatedPrev.Index)
              {
                if (!local.ReinstatedPrev.CheckSize())
                {
                  break;
                }

                local.KdmvFile.FileType = "2";
                local.KdmvFile.Dob =
                  local.ReinstatedPrev.Item.ReinstatedPrevKdmvFile.Dob;
                local.KdmvFile.Ssn =
                  local.ReinstatedPrev.Item.ReinstatedPrevKdmvFile.Ssn;
                local.KdmvFile.DriverLicenseNumber =
                  local.ReinstatedPrev.Item.ReinstatedPrevKdmvFile.
                    DriverLicenseNumber;
                local.KdmvFile.CsePersonNumber =
                  local.ReinstatedPrev.Item.ReinstatedPrevKdmvFile.
                    CsePersonNumber;
                local.KdmvFile.LastName =
                  local.ReinstatedPrev.Item.ReinstatedPrevKdmvFile.LastName;
                local.KdmvFile.FirstName =
                  local.ReinstatedPrev.Item.ReinstatedPrevKdmvFile.FirstName;
                local.DateWorkArea.Year = Year(local.StartDate.Date);
                local.WorkArea.Text15 =
                  NumberToString(local.DateWorkArea.Year, 15);
                local.Year.Text4 = Substring(local.WorkArea.Text15, 12, 4);
                local.DateWorkArea.Month = Month(local.StartDate.Date);
                local.WorkArea.Text15 =
                  NumberToString(local.DateWorkArea.Month, 15);
                local.Month.Text2 = Substring(local.WorkArea.Text15, 14, 2);
                local.DateWorkArea.Day = Day(local.StartDate.Date);
                local.WorkArea.Text15 =
                  NumberToString(local.DateWorkArea.Day, 15);
                local.Day.Text2 = Substring(local.WorkArea.Text15, 14, 2);
                local.DateWorkArea.TextDate = local.Year.Text4 + local
                  .Month.Text2 + local.Day.Text2;
                local.KdmvFile.ProcessDate = local.DateWorkArea.TextDate;
                local.PassArea.FileInstruction = "WRITE";
                UseOeEabKdmvRestrictReinstate();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                }

                ++local.NumOfReinstateRequestCommon.Count;

                break;
              }

              local.ReinstatedPrev.CheckIndex();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                break;
              }

              ++local.RecordCount.Count;

              if (local.RecordCount.Count >= local
                .ProgramCheckpointRestart.UpdateFrequencyCount.
                  GetValueOrDefault())
              {
                local.ProgramCheckpointRestart.RestartInd = "Y";
                local.ProgramCheckpointRestart.ProgramName =
                  local.ProgramProcessingInfo.Name;
                local.ProgramCheckpointRestart.CheckpointCount =
                  local.ReadOnlyKsDriversLicense.SequenceCounter - 1;
                local.NumRestictRead.Text15 =
                  NumberToString(local.NumOfRestrictReads.Count, 15);
                local.NumOfRestrictsRequest.Text15 =
                  NumberToString(local.NumOfRestrictRequest.Count, 15);
                local.NumberOfFailedRestrictsWorkArea.Text15 =
                  NumberToString(local.NumberOfFailedRestrictsCommon.Count, 15);
                  
                local.TotalNumberOfErrors.Text15 =
                  NumberToString(local.NumErrorRecords.Count, 15);
                local.NumReinstateRead.Text15 =
                  NumberToString(local.NumberOfReinstateReads.Count, 15);
                local.NumOfReinstateRequestWorkArea.Text15 =
                  NumberToString(local.NumOfReinstateRequestCommon.Count, 15);
                local.RestrictReinstateProcess.Flag = "N";
                local.ProgramCheckpointRestart.RestartInfo =
                  local.NumRestictRead.Text15 + local
                  .NumOfRestrictsRequest.Text15 + local
                  .NumberOfFailedRestrictsWorkArea.Text15;
                local.ProgramCheckpointRestart.RestartInfo =
                  local.RestrictReinstateProcess.Flag + TrimEnd
                  (local.ProgramCheckpointRestart.RestartInfo) + local
                  .TotalNumberOfErrors.Text15 + local
                  .NumReinstateRead.Text15 + local
                  .NumOfReinstateRequestWorkArea.Text15;
                UseUpdatePgmCheckpointRestart();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  ++local.NumErrorRecords.Count;

                  goto Test5;
                }

                local.LastUpdateFailedRequest.Count =
                  local.NumberOfFailedRestrictsCommon.Count;
                local.LastUpdateReinstateRead.Count =
                  local.NumberOfReinstateReads.Count;
                local.LastUpdateReinstateRequ.Count =
                  local.NumOfReinstateRequestCommon.Count;
                local.LastUpdateRestrictRead.Count =
                  local.NumOfRestrictReads.Count;
                local.LastUpdateRestrictRequ.Count =
                  local.NumOfRestrictRequest.Count;
                local.LastUpdatedNumOfErrors.Count =
                  local.NumErrorRecords.Count;
                UseExtToDoACommit();

                if (local.PassArea.NumericReturnCode != 0)
                {
                  ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

                  return;
                }

                local.RecordCount.Count = 0;
              }

              local.PreviousfindMe.Number = "";
              local.PreviousPersonTotal.TotalCurrency = 0;

              for(local.ReinstatedPrev.Index = 0; local.ReinstatedPrev.Index < local
                .ReinstatedPrev.Count; ++local.ReinstatedPrev.Index)
              {
                if (!local.ReinstatedPrev.CheckSize())
                {
                  break;
                }

                local.ReinstatedPrev.Update.ReinstatedPrevKsDriversLicense.
                  CreatedTstamp = local.ClearKsDriversLicense.CreatedTstamp;
                MoveLegalAction(local.ClearLegalAction,
                  local.ReinstatedPrev.Update.ReinstatedPrevLegalAction);
                local.ReinstatedPrev.Update.ReinstatedPrevKdmvFile.Assign(
                  local.ClearKdmvFile);
              }

              local.ReinstatedPrev.CheckIndex();
              local.ReinstatedPrev.Index = -1;
              local.ReinstatedPrev.Count = 0;
            }
          }

ReadEach2:

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

            local.Error.Number = "";
          }

ReadEach3:
          ;
        }

        if (IsExitState("ACO_NN0000_ALL_OK") && (local.Reinstated.Count > 0 || local
          .ReinstatedPrev.Count > 0))
        {
          // this is just to catch the last record, it did not have a chance to 
          // process
          if (local.ReinstatedPrev.Count > 0)
          {
          }
          else
          {
            if (local.ApTotalCtOrder.Count != local
              .NumCtOrdersReinstated.Count && local
              .NumCtOrdersReinstated.Count > 0)
            {
              goto Test5;
            }

            local.ReinstatedPrev.Index = -1;
            local.ReinstatedPrev.Count = 0;

            for(local.Reinstated.Index = 0; local.Reinstated.Index < local
              .Reinstated.Count; ++local.Reinstated.Index)
            {
              if (!local.Reinstated.CheckSize())
              {
                break;
              }

              local.ReinstatedPrev.Index = local.Reinstated.Index;
              local.ReinstatedPrev.CheckSize();

              local.ReinstatedPrev.Update.ReinstatedPrevKdmvFile.Assign(
                local.Reinstated.Item.ReinstatedKdmvFile);
              MoveLegalAction(local.Reinstated.Item.ReinstatedLegalAction,
                local.ReinstatedPrev.Update.ReinstatedPrevLegalAction);
              local.ReinstatedPrev.Update.ReinstatedPrevCommon.TotalCurrency =
                local.Reinstated.Item.ReinstatedCommon.TotalCurrency;
              local.ReinstatedPrev.Update.ReinstatedPrevKsDriversLicense.
                CreatedTstamp =
                  local.Reinstated.Item.ReinstatedKsDriversLicense.
                  CreatedTstamp;
              local.ReinstatedPrev.Update.ReinstatedCodePrev.Flag =
                local.Reinstated.Item.ReinstatedCode.Flag;
            }

            local.Reinstated.CheckIndex();
          }

          // we  have to make sure that all the court orders pass before we send
          // a request to to take the obligors off the list
          // we only want to write out one record per obligor
          for(local.ReinstatedPrev.Index = 0; local.ReinstatedPrev.Index < local
            .ReinstatedPrev.Count; ++local.ReinstatedPrev.Index)
          {
            if (!local.ReinstatedPrev.CheckSize())
            {
              break;
            }

            local.KdmvFile.FileType = "2";
            local.KdmvFile.Dob =
              local.ReinstatedPrev.Item.ReinstatedPrevKdmvFile.Dob;
            local.KdmvFile.Ssn =
              local.ReinstatedPrev.Item.ReinstatedPrevKdmvFile.Ssn;
            local.KdmvFile.DriverLicenseNumber =
              local.ReinstatedPrev.Item.ReinstatedPrevKdmvFile.
                DriverLicenseNumber;
            local.KdmvFile.CsePersonNumber =
              local.ReinstatedPrev.Item.ReinstatedPrevKdmvFile.CsePersonNumber;
            local.KdmvFile.LastName =
              local.ReinstatedPrev.Item.ReinstatedPrevKdmvFile.LastName;
            local.KdmvFile.FirstName =
              local.ReinstatedPrev.Item.ReinstatedPrevKdmvFile.FirstName;
            local.DateWorkArea.Year = Year(local.StartDate.Date);
            local.WorkArea.Text15 = NumberToString(local.DateWorkArea.Year, 15);
            local.Year.Text4 = Substring(local.WorkArea.Text15, 12, 4);
            local.DateWorkArea.Month = Month(local.StartDate.Date);
            local.WorkArea.Text15 =
              NumberToString(local.DateWorkArea.Month, 15);
            local.Month.Text2 = Substring(local.WorkArea.Text15, 14, 2);
            local.DateWorkArea.Day = Day(local.StartDate.Date);
            local.WorkArea.Text15 = NumberToString(local.DateWorkArea.Day, 15);
            local.Day.Text2 = Substring(local.WorkArea.Text15, 14, 2);
            local.DateWorkArea.TextDate = local.Year.Text4 + local
              .Month.Text2 + local.Day.Text2;
            local.KdmvFile.ProcessDate = local.DateWorkArea.TextDate;
            local.PassArea.FileInstruction = "WRITE";
            UseOeEabKdmvRestrictReinstate();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            ++local.NumOfReinstateRequestCommon.Count;

            break;
          }

          local.ReinstatedPrev.CheckIndex();

          // at this point the court order meets the critieria to have the 
          // driver's license reinstated
          for(local.ReinstatedPrev.Index = 0; local.ReinstatedPrev.Index < local
            .ReinstatedPrev.Count; ++local.ReinstatedPrev.Index)
          {
            if (!local.ReinstatedPrev.CheckSize())
            {
              break;
            }

            // **********************************************************************************
            // Update the KDMV record with the reinstatement date
            // **********************************************************************************
            if (ReadKsDriversLicense2())
            {
              try
              {
                UpdateKsDriversLicense3();
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "KS_DRIVERS_LICENSE_NU";
                    local.Error.Number =
                      local.ReinstatedPrev.Item.ReinstatedPrevKdmvFile.
                        CsePersonNumber;
                    ++local.NumErrorRecords.Count;

                    goto Test5;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "KS_DRIVERS_LICENSE_PV";
                    local.Error.Number =
                      local.ReinstatedPrev.Item.ReinstatedPrevKdmvFile.
                        CsePersonNumber;
                    ++local.NumErrorRecords.Count;

                    goto Test5;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }
            }
          }

          local.ReinstatedPrev.CheckIndex();
        }
      }
    }

Test5:

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      foreach(var item in ReadKsDriversLicense7())
      {
        ExitState = "ACO_NN0000_ALL_OK";

        // we need to clear the  processing ind now that we have successfully
        // completed both parts of the program
        try
        {
          UpdateKsDriversLicense5();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              if (ReadCsePerson1())
              {
                ExitState = "KS_DRIVERS_LICENSE_NU";
                local.Error.Number = entities.CsePerson.Number;
                ++local.NumErrorRecords.Count;
              }

              break;
            case ErrorCode.PermittedValueViolation:
              if (ReadCsePerson1())
              {
                ExitState = "KS_DRIVERS_LICENSE_PV";
                local.Error.Number = entities.CsePerson.Number;
                ++local.NumErrorRecords.Count;
              }

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

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
        }
      }

      UseOeB459Close2();
      local.PassArea.FileInstruction = "CLOSE";
      UseOeEabKdmvRestrictReinstate();
      local.ProgramCheckpointRestart.RestartInd = "N";
      local.ProgramCheckpointRestart.ProgramName =
        local.ProgramProcessingInfo.Name;
      local.ProgramCheckpointRestart.CheckpointCount = -1;
      local.ProgramCheckpointRestart.RestartInfo = "";
      UseUpdatePgmCheckpointRestart();
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

      UseOeB459Close1();
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
    target.ReadFrequencyCount = source.ReadFrequencyCount;
    target.CheckpointCount = source.CheckpointCount;
    target.RestartInd = source.RestartInd;
    target.RestartInfo = source.RestartInfo;
  }

  private static void MoveScreenOwedAmountsDtl(ScreenOwedAmountsDtl source,
    ScreenOwedAmountsDtl target)
  {
    target.TotalArrearsOwed = source.TotalArrearsOwed;
    target.TotalInterestOwed = source.TotalInterestOwed;
  }

  private static void MoveSufValuesToGroup(Local.SufValuesGroup source,
    OeScrubSuffixes.Import.GroupGroup target)
  {
    target.CodeValue.Cdvalue = source.SufValues1.Cdvalue;
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

  private void UseFnComputeTotalsForCtOrder3()
  {
    var useImport = new FnComputeTotalsForCtOrder3.Import();
    var useExport = new FnComputeTotalsForCtOrder3.Export();

    useImport.Obligor.Number = entities.CsePerson.Number;
    useImport.IncludeArrearsOnly.Flag = local.ArrearsOnly.Flag;
    useImport.StartDate.Date = local.StartDate.Date;
    useImport.FilterByStdNo.StandardNumber = local.LegalAction.StandardNumber;

    Call(FnComputeTotalsForCtOrder3.Execute, useImport, useExport);

    MoveScreenOwedAmountsDtl(useExport.ScreenOwedAmountsDtl,
      local.ScreenOwedAmountsDtl);
  }

  private void UseFnComputeTotalsForCtOrder4()
  {
    var useImport = new FnComputeTotalsForCtOrder4.Import();
    var useExport = new FnComputeTotalsForCtOrder4.Export();

    useImport.Obligor.Number = entities.CsePerson.Number;
    useImport.FilterByStdNo.StandardNumber =
      entities.LegalAction.StandardNumber;
    useImport.StartDate.Date = local.StartDate.Date;

    Call(FnComputeTotalsForCtOrder4.Execute, useImport, useExport);

    MoveScreenOwedAmountsDtl(useExport.ScreenOwedAmountsDtl,
      local.ScreenOwedAmountsDtl);
  }

  private void UseOeB459Close1()
  {
    var useImport = new OeB459Close.Import();
    var useExport = new OeB459Close.Export();

    useImport.NumRestrictRecordsRead.Count = local.LastUpdateRestrictRead.Count;
    useImport.NumOfRestrictRequest.Count = local.LastUpdateRestrictRequ.Count;
    useImport.NumFailedRestrictReqst.Count =
      local.LastUpdateFailedRequest.Count;
    useImport.NumReinstateRecsRead.Count = local.LastUpdateRestrictRead.Count;
    useImport.NumOfReinstateRequest.Count = local.LastUpdateReinstateRequ.Count;
    useImport.NumErrorRecords.Count = local.LastUpdatedNumOfErrors.Count;

    Call(OeB459Close.Execute, useImport, useExport);
  }

  private void UseOeB459Close2()
  {
    var useImport = new OeB459Close.Import();
    var useExport = new OeB459Close.Export();

    useImport.NumRestrictRecordsRead.Count = local.NumOfRestrictReads.Count;
    useImport.NumOfRestrictRequest.Count = local.NumOfRestrictRequest.Count;
    useImport.NumFailedRestrictReqst.Count =
      local.NumberOfFailedRestrictsCommon.Count;
    useImport.NumReinstateRecsRead.Count = local.NumberOfReinstateReads.Count;
    useImport.NumOfReinstateRequest.Count =
      local.NumOfReinstateRequestCommon.Count;
    useImport.NumErrorRecords.Count = local.NumErrorRecords.Count;

    Call(OeB459Close.Execute, useImport, useExport);
  }

  private void UseOeB459Housekeeping()
  {
    var useImport = new OeB459Housekeeping.Import();
    var useExport = new OeB459Housekeeping.Export();

    Call(OeB459Housekeeping.Execute, useImport, useExport);

    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseOeDetermineCurrentIwo()
  {
    var useImport = new OeDetermineCurrentIwo.Import();
    var useExport = new OeDetermineCurrentIwo.Export();

    useImport.Import30DayLetterSentDate.Date = local.Local30DayLetterSent.Date;
    useImport.LegalAction.StandardNumber = local.LegalAction.StandardNumber;
    useImport.CsePerson.Number = entities.CsePerson.Number;

    Call(OeDetermineCurrentIwo.Execute, useImport, useExport);

    local.IwoCreatedDate.Date = useExport.CreatedDate.Date;
    local.WageWithholdingFound.Flag = useExport.WageWithholdingFound.Flag;
  }

  private void UseOeDetermineDmvCriteria()
  {
    var useImport = new OeDetermineDmvCriteria.Import();
    var useExport = new OeDetermineDmvCriteria.Export();

    useImport.CsePerson.Number = entities.CsePerson.Number;
    useImport.ValidPeriodCourtesyLtr.Date = local.SpecifiedTimeFrameDate.Date;
    useImport.AdministrativeAction.Type1 = local.AdministrativeAction.Type1;
    useImport.StartDate.Date = local.StartDate.Date;
    useImport.LegalAction.StandardNumber = local.LegalAction.StandardNumber;
    useImport.ValidPeriod30DayNotic.Date = local.ValidPeriod30DayNotic.Date;

    Call(OeDetermineDmvCriteria.Execute, useImport, useExport);

    local.FailureReturnCode.Flag = useExport.FailureReturnCode.Flag;
    local.CourtesyLetterSentDt.Date = useExport.CrtLetterSentDate.Date;
    local.LtrWithinTimeFrame.Flag = useExport.LtrWithinTimeFrame.Flag;
    local.GoToNextCourtOrder.Flag = useExport.GoToNextCourtOrder.Flag;
    local.NextPerson.Flag = useExport.NextPerson.Flag;
  }

  private void UseOeDeterminePaymentsForKdmv1()
  {
    var useImport = new OeDeterminePaymentsForKdmv2.Import();
    var useExport = new OeDeterminePaymentsForKdmv2.Export();

    useImport.ReinstateProcess.Flag = local.ReinstateProcess.Flag;
    useImport.PaymentDueToDate.Date = local.PaymentTo.Date;
    useImport.ReadFromDate.Date = local.PaymentDueDateFrom.Date;
    useImport.LegalAction.StandardNumber = local.LegalAction.StandardNumber;
    useImport.CsePerson.Number = entities.CsePerson.Number;

    Call(OeDeterminePaymentsForKdmv2.Execute, useImport, useExport);

    local.PaymentAmountMade.TotalCurrency = useExport.AmountPaid.TotalCurrency;
  }

  private void UseOeDeterminePaymentsForKdmv3()
  {
    var useImport = new OeDeterminePaymentsForKdmv2.Import();
    var useExport = new OeDeterminePaymentsForKdmv2.Export();

    useImport.PaymentDueToDate.Date = local.PaymentTo.Date;
    useImport.ReadFromDate.Date = local.PaymentDueDateFrom.Date;
    useImport.LegalAction.StandardNumber = local.LegalAction.StandardNumber;
    useImport.CsePerson.Number = local.CsePerson.Number;

    Call(OeDeterminePaymentsForKdmv2.Execute, useImport, useExport);

    local.PaymentAmountMade.TotalCurrency = useExport.AmountPaid.TotalCurrency;
  }

  private void UseOeEabKdmvRestrictReinstate()
  {
    var useImport = new OeEabKdmvRestrictReinstate.Import();
    var useExport = new OeEabKdmvRestrictReinstate.Export();

    useImport.External.FileInstruction = local.PassArea.FileInstruction;
    useImport.KdmvFile.Assign(local.KdmvFile);
    useExport.External.Assign(local.PassArea);

    Call(OeEabKdmvRestrictReinstate.Execute, useImport, useExport);

    local.PassArea.Assign(useExport.External);
  }

  private void UseOeScrubSuffixes()
  {
    var useImport = new OeScrubSuffixes.Import();
    var useExport = new OeScrubSuffixes.Export();

    useImport.CsePersonsWorkSet.LastName = local.CsePersonsWorkSet.LastName;
    local.SufValues.CopyTo(useImport.Group, MoveSufValuesToGroup);

    Call(OeScrubSuffixes.Execute, useImport, useExport);

    local.CsePersonsWorkSet.LastName = useExport.CsePersonsWorkSet.LastName;
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

  private void UseUpdatePgmCheckpointRestart()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
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
        entities.Case1.StatusDate = db.GetNullableDate(reader, 2);
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 3);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 4);
        entities.CaseRole.CspNumber = db.GetString(reader, 5);
        entities.CaseRole.Type1 = db.GetString(reader, 6);
        entities.CaseRole.Identifier = db.GetInt32(reader, 7);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 8);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 9);
        entities.CaseRole.Populated = true;
        entities.Case1.Populated = true;
        entities.CaseUnit.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);

        return true;
      });
  }

  private bool ReadCaseRole()
  {
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRole",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "startDate", local.StartDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private IEnumerable<bool> ReadCodeValue()
  {
    entities.CodeValue.Populated = false;

    return ReadEach("ReadCodeValue",
      null,
      (db, reader) =>
      {
        entities.CodeValue.Id = db.GetInt32(reader, 0);
        entities.CodeValue.CodId = db.GetNullableInt32(reader, 1);
        entities.CodeValue.Cdvalue = db.GetString(reader, 2);
        entities.CodeValue.Populated = true;

        return true;
      });
  }

  private bool ReadCsePerson1()
  {
    System.Diagnostics.Debug.Assert(entities.KsDriversLicense.Populated);
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.KsDriversLicense.CspNum);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.DateOfDeath = db.GetNullableDate(reader, 2);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 3);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePerson2()
  {
    System.Diagnostics.Debug.
      Assert(entities.ReadOnlyKsDriversLicense.Populated);
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.ReadOnlyKsDriversLicense.CspNum);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.DateOfDeath = db.GetNullableDate(reader, 2);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 3);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadKsDriversLicense1()
  {
    entities.N2dRead.Populated = false;

    return Read("ReadKsDriversLicense1",
      (db, command) =>
      {
        db.SetString(command, "cspNum", local.ReadOnlyCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.N2dRead.CreatedTstamp = db.GetDateTime(reader, 0);
        entities.N2dRead.CspNum = db.GetString(reader, 1);
        entities.N2dRead.LgaIdentifier = db.GetNullableInt32(reader, 2);
        entities.N2dRead.KsDvrLicense = db.GetNullableString(reader, 3);
        entities.N2dRead.ValidationDate = db.GetNullableDate(reader, 4);
        entities.N2dRead.CourtesyLetterSentDate = db.GetNullableDate(reader, 5);
        entities.N2dRead.Attribute30DayLetterCreatedDate =
          db.GetNullableDate(reader, 6);
        entities.N2dRead.ServiceCompleteInd = db.GetNullableString(reader, 7);
        entities.N2dRead.ServiceCompleteDate = db.GetNullableDate(reader, 8);
        entities.N2dRead.RestrictedDate = db.GetNullableDate(reader, 9);
        entities.N2dRead.ReinstatedDate = db.GetNullableDate(reader, 10);
        entities.N2dRead.AppealReceivedDate = db.GetNullableDate(reader, 11);
        entities.N2dRead.AppealResolved = db.GetNullableString(reader, 12);
        entities.N2dRead.PaymentAgreementDate = db.GetNullableDate(reader, 13);
        entities.N2dRead.FirstPaymentDueDate = db.GetNullableDate(reader, 14);
        entities.N2dRead.ManualInd = db.GetNullableString(reader, 15);
        entities.N2dRead.ManualDate = db.GetNullableDate(reader, 16);
        entities.N2dRead.AppealResolvedDate = db.GetNullableDate(reader, 17);
        entities.N2dRead.LicenseRestrictionSentDate =
          db.GetNullableDate(reader, 18);
        entities.N2dRead.RestrictionStatus = db.GetNullableString(reader, 19);
        entities.N2dRead.AmountOwed = db.GetNullableDecimal(reader, 20);
        entities.N2dRead.AmountDue = db.GetNullableDecimal(reader, 21);
        entities.N2dRead.RecordClosureReason = db.GetNullableString(reader, 22);
        entities.N2dRead.RecordClosureDate = db.GetNullableDate(reader, 23);
        entities.N2dRead.LastUpdatedBy = db.GetNullableString(reader, 24);
        entities.N2dRead.LastUpdatedTstamp = db.GetNullableDateTime(reader, 25);
        entities.N2dRead.ProcessedInd = db.GetNullableString(reader, 26);
        entities.N2dRead.SequenceCounter = db.GetInt32(reader, 27);
        entities.N2dRead.Populated = true;
      });
  }

  private bool ReadKsDriversLicense2()
  {
    entities.N2dRead.Populated = false;

    return Read("ReadKsDriversLicense2",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTstamp",
          local.ReinstatedPrev.Item.ReinstatedPrevKsDriversLicense.
            CreatedTstamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.N2dRead.CreatedTstamp = db.GetDateTime(reader, 0);
        entities.N2dRead.CspNum = db.GetString(reader, 1);
        entities.N2dRead.LgaIdentifier = db.GetNullableInt32(reader, 2);
        entities.N2dRead.KsDvrLicense = db.GetNullableString(reader, 3);
        entities.N2dRead.ValidationDate = db.GetNullableDate(reader, 4);
        entities.N2dRead.CourtesyLetterSentDate = db.GetNullableDate(reader, 5);
        entities.N2dRead.Attribute30DayLetterCreatedDate =
          db.GetNullableDate(reader, 6);
        entities.N2dRead.ServiceCompleteInd = db.GetNullableString(reader, 7);
        entities.N2dRead.ServiceCompleteDate = db.GetNullableDate(reader, 8);
        entities.N2dRead.RestrictedDate = db.GetNullableDate(reader, 9);
        entities.N2dRead.ReinstatedDate = db.GetNullableDate(reader, 10);
        entities.N2dRead.AppealReceivedDate = db.GetNullableDate(reader, 11);
        entities.N2dRead.AppealResolved = db.GetNullableString(reader, 12);
        entities.N2dRead.PaymentAgreementDate = db.GetNullableDate(reader, 13);
        entities.N2dRead.FirstPaymentDueDate = db.GetNullableDate(reader, 14);
        entities.N2dRead.ManualInd = db.GetNullableString(reader, 15);
        entities.N2dRead.ManualDate = db.GetNullableDate(reader, 16);
        entities.N2dRead.AppealResolvedDate = db.GetNullableDate(reader, 17);
        entities.N2dRead.LicenseRestrictionSentDate =
          db.GetNullableDate(reader, 18);
        entities.N2dRead.RestrictionStatus = db.GetNullableString(reader, 19);
        entities.N2dRead.AmountOwed = db.GetNullableDecimal(reader, 20);
        entities.N2dRead.AmountDue = db.GetNullableDecimal(reader, 21);
        entities.N2dRead.RecordClosureReason = db.GetNullableString(reader, 22);
        entities.N2dRead.RecordClosureDate = db.GetNullableDate(reader, 23);
        entities.N2dRead.LastUpdatedBy = db.GetNullableString(reader, 24);
        entities.N2dRead.LastUpdatedTstamp = db.GetNullableDateTime(reader, 25);
        entities.N2dRead.ProcessedInd = db.GetNullableString(reader, 26);
        entities.N2dRead.SequenceCounter = db.GetInt32(reader, 27);
        entities.N2dRead.Populated = true;
      });
  }

  private bool ReadKsDriversLicense3()
  {
    entities.KsDriversLicense.Populated = false;

    return Read("ReadKsDriversLicense3",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "validationDate", local.ZeroDate.Date.GetValueOrDefault());
      },
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
        entities.KsDriversLicense.Attribute30DayLetterCreatedDate =
          db.GetNullableDate(reader, 7);
        entities.KsDriversLicense.ServiceCompleteInd =
          db.GetNullableString(reader, 8);
        entities.KsDriversLicense.ServiceCompleteDate =
          db.GetNullableDate(reader, 9);
        entities.KsDriversLicense.RestrictedDate =
          db.GetNullableDate(reader, 10);
        entities.KsDriversLicense.ReinstatedDate =
          db.GetNullableDate(reader, 11);
        entities.KsDriversLicense.AppealReceivedDate =
          db.GetNullableDate(reader, 12);
        entities.KsDriversLicense.AppealResolved =
          db.GetNullableString(reader, 13);
        entities.KsDriversLicense.PaymentAgreementDate =
          db.GetNullableDate(reader, 14);
        entities.KsDriversLicense.FirstPaymentDueDate =
          db.GetNullableDate(reader, 15);
        entities.KsDriversLicense.ManualDate = db.GetNullableDate(reader, 16);
        entities.KsDriversLicense.LicenseRestrictionSentDate =
          db.GetNullableDate(reader, 17);
        entities.KsDriversLicense.RestrictionStatus =
          db.GetNullableString(reader, 18);
        entities.KsDriversLicense.AmountOwed =
          db.GetNullableDecimal(reader, 19);
        entities.KsDriversLicense.AmountDue = db.GetNullableDecimal(reader, 20);
        entities.KsDriversLicense.RecordClosureReason =
          db.GetNullableString(reader, 21);
        entities.KsDriversLicense.RecordClosureDate =
          db.GetNullableDate(reader, 22);
        entities.KsDriversLicense.LastUpdatedBy =
          db.GetNullableString(reader, 23);
        entities.KsDriversLicense.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 24);
        entities.KsDriversLicense.ProcessedInd =
          db.GetNullableString(reader, 25);
        entities.KsDriversLicense.SequenceCounter = db.GetInt32(reader, 26);
        entities.KsDriversLicense.Populated = true;
      });
  }

  private IEnumerable<bool> ReadKsDriversLicense4()
  {
    entities.N2dRead.Populated = false;

    return ReadEach("ReadKsDriversLicense4",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "ltr30DayDate", local.ZeroDate.Date.GetValueOrDefault());
        db.SetString(command, "cspNum", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.N2dRead.CreatedTstamp = db.GetDateTime(reader, 0);
        entities.N2dRead.CspNum = db.GetString(reader, 1);
        entities.N2dRead.LgaIdentifier = db.GetNullableInt32(reader, 2);
        entities.N2dRead.KsDvrLicense = db.GetNullableString(reader, 3);
        entities.N2dRead.ValidationDate = db.GetNullableDate(reader, 4);
        entities.N2dRead.CourtesyLetterSentDate = db.GetNullableDate(reader, 5);
        entities.N2dRead.Attribute30DayLetterCreatedDate =
          db.GetNullableDate(reader, 6);
        entities.N2dRead.ServiceCompleteInd = db.GetNullableString(reader, 7);
        entities.N2dRead.ServiceCompleteDate = db.GetNullableDate(reader, 8);
        entities.N2dRead.RestrictedDate = db.GetNullableDate(reader, 9);
        entities.N2dRead.ReinstatedDate = db.GetNullableDate(reader, 10);
        entities.N2dRead.AppealReceivedDate = db.GetNullableDate(reader, 11);
        entities.N2dRead.AppealResolved = db.GetNullableString(reader, 12);
        entities.N2dRead.PaymentAgreementDate = db.GetNullableDate(reader, 13);
        entities.N2dRead.FirstPaymentDueDate = db.GetNullableDate(reader, 14);
        entities.N2dRead.ManualInd = db.GetNullableString(reader, 15);
        entities.N2dRead.ManualDate = db.GetNullableDate(reader, 16);
        entities.N2dRead.AppealResolvedDate = db.GetNullableDate(reader, 17);
        entities.N2dRead.LicenseRestrictionSentDate =
          db.GetNullableDate(reader, 18);
        entities.N2dRead.RestrictionStatus = db.GetNullableString(reader, 19);
        entities.N2dRead.AmountOwed = db.GetNullableDecimal(reader, 20);
        entities.N2dRead.AmountDue = db.GetNullableDecimal(reader, 21);
        entities.N2dRead.RecordClosureReason = db.GetNullableString(reader, 22);
        entities.N2dRead.RecordClosureDate = db.GetNullableDate(reader, 23);
        entities.N2dRead.LastUpdatedBy = db.GetNullableString(reader, 24);
        entities.N2dRead.LastUpdatedTstamp = db.GetNullableDateTime(reader, 25);
        entities.N2dRead.ProcessedInd = db.GetNullableString(reader, 26);
        entities.N2dRead.SequenceCounter = db.GetInt32(reader, 27);
        entities.N2dRead.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadKsDriversLicense5()
  {
    entities.KsDriversLicense.Populated = false;

    return ReadEach("ReadKsDriversLicense5",
      (db, command) =>
      {
        db.SetString(command, "cspNum", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "restrictSentDt1", local.ZeroDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "restrictSentDt2", local.StartDate.Date.GetValueOrDefault());
          
      },
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
        entities.KsDriversLicense.Attribute30DayLetterCreatedDate =
          db.GetNullableDate(reader, 7);
        entities.KsDriversLicense.ServiceCompleteInd =
          db.GetNullableString(reader, 8);
        entities.KsDriversLicense.ServiceCompleteDate =
          db.GetNullableDate(reader, 9);
        entities.KsDriversLicense.RestrictedDate =
          db.GetNullableDate(reader, 10);
        entities.KsDriversLicense.ReinstatedDate =
          db.GetNullableDate(reader, 11);
        entities.KsDriversLicense.AppealReceivedDate =
          db.GetNullableDate(reader, 12);
        entities.KsDriversLicense.AppealResolved =
          db.GetNullableString(reader, 13);
        entities.KsDriversLicense.PaymentAgreementDate =
          db.GetNullableDate(reader, 14);
        entities.KsDriversLicense.FirstPaymentDueDate =
          db.GetNullableDate(reader, 15);
        entities.KsDriversLicense.ManualDate = db.GetNullableDate(reader, 16);
        entities.KsDriversLicense.LicenseRestrictionSentDate =
          db.GetNullableDate(reader, 17);
        entities.KsDriversLicense.RestrictionStatus =
          db.GetNullableString(reader, 18);
        entities.KsDriversLicense.AmountOwed =
          db.GetNullableDecimal(reader, 19);
        entities.KsDriversLicense.AmountDue = db.GetNullableDecimal(reader, 20);
        entities.KsDriversLicense.RecordClosureReason =
          db.GetNullableString(reader, 21);
        entities.KsDriversLicense.RecordClosureDate =
          db.GetNullableDate(reader, 22);
        entities.KsDriversLicense.LastUpdatedBy =
          db.GetNullableString(reader, 23);
        entities.KsDriversLicense.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 24);
        entities.KsDriversLicense.ProcessedInd =
          db.GetNullableString(reader, 25);
        entities.KsDriversLicense.SequenceCounter = db.GetInt32(reader, 26);
        entities.KsDriversLicense.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadKsDriversLicense6()
  {
    entities.ReadOnlyKsDriversLicense.Populated = false;

    return ReadEach("ReadKsDriversLicense6",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "restrictSentDt1", local.StartDate.Date.GetValueOrDefault());
          
        db.SetNullableDate(
          command, "restrictSentDt2", local.ZeroDate.Date.GetValueOrDefault());
        db.SetInt32(
          command, "sequenceCounter",
          local.ReadOnlyKsDriversLicense.SequenceCounter);
      },
      (db, reader) =>
      {
        entities.ReadOnlyKsDriversLicense.CspNum = db.GetString(reader, 0);
        entities.ReadOnlyKsDriversLicense.ValidationDate =
          db.GetNullableDate(reader, 1);
        entities.ReadOnlyKsDriversLicense.CourtesyLetterSentDate =
          db.GetNullableDate(reader, 2);
        entities.ReadOnlyKsDriversLicense.Attribute30DayLetterCreatedDate =
          db.GetNullableDate(reader, 3);
        entities.ReadOnlyKsDriversLicense.ServiceCompleteInd =
          db.GetNullableString(reader, 4);
        entities.ReadOnlyKsDriversLicense.ServiceCompleteDate =
          db.GetNullableDate(reader, 5);
        entities.ReadOnlyKsDriversLicense.RestrictedDate =
          db.GetNullableDate(reader, 6);
        entities.ReadOnlyKsDriversLicense.ReinstatedDate =
          db.GetNullableDate(reader, 7);
        entities.ReadOnlyKsDriversLicense.ManualDate =
          db.GetNullableDate(reader, 8);
        entities.ReadOnlyKsDriversLicense.LicenseRestrictionSentDate =
          db.GetNullableDate(reader, 9);
        entities.ReadOnlyKsDriversLicense.RestrictionStatus =
          db.GetNullableString(reader, 10);
        entities.ReadOnlyKsDriversLicense.RecordClosureReason =
          db.GetNullableString(reader, 11);
        entities.ReadOnlyKsDriversLicense.SequenceCounter =
          db.GetInt32(reader, 12);
        entities.ReadOnlyKsDriversLicense.Populated = true;

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
        entities.KsDriversLicense.LgaIdentifier =
          db.GetNullableInt32(reader, 3);
        entities.KsDriversLicense.KsDvrLicense =
          db.GetNullableString(reader, 4);
        entities.KsDriversLicense.ValidationDate =
          db.GetNullableDate(reader, 5);
        entities.KsDriversLicense.CourtesyLetterSentDate =
          db.GetNullableDate(reader, 6);
        entities.KsDriversLicense.Attribute30DayLetterCreatedDate =
          db.GetNullableDate(reader, 7);
        entities.KsDriversLicense.ServiceCompleteInd =
          db.GetNullableString(reader, 8);
        entities.KsDriversLicense.ServiceCompleteDate =
          db.GetNullableDate(reader, 9);
        entities.KsDriversLicense.RestrictedDate =
          db.GetNullableDate(reader, 10);
        entities.KsDriversLicense.ReinstatedDate =
          db.GetNullableDate(reader, 11);
        entities.KsDriversLicense.AppealReceivedDate =
          db.GetNullableDate(reader, 12);
        entities.KsDriversLicense.AppealResolved =
          db.GetNullableString(reader, 13);
        entities.KsDriversLicense.PaymentAgreementDate =
          db.GetNullableDate(reader, 14);
        entities.KsDriversLicense.FirstPaymentDueDate =
          db.GetNullableDate(reader, 15);
        entities.KsDriversLicense.ManualDate = db.GetNullableDate(reader, 16);
        entities.KsDriversLicense.LicenseRestrictionSentDate =
          db.GetNullableDate(reader, 17);
        entities.KsDriversLicense.RestrictionStatus =
          db.GetNullableString(reader, 18);
        entities.KsDriversLicense.AmountOwed =
          db.GetNullableDecimal(reader, 19);
        entities.KsDriversLicense.AmountDue = db.GetNullableDecimal(reader, 20);
        entities.KsDriversLicense.RecordClosureReason =
          db.GetNullableString(reader, 21);
        entities.KsDriversLicense.RecordClosureDate =
          db.GetNullableDate(reader, 22);
        entities.KsDriversLicense.LastUpdatedBy =
          db.GetNullableString(reader, 23);
        entities.KsDriversLicense.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 24);
        entities.KsDriversLicense.ProcessedInd =
          db.GetNullableString(reader, 25);
        entities.KsDriversLicense.SequenceCounter = db.GetInt32(reader, 26);
        entities.KsDriversLicense.Populated = true;

        return true;
      });
  }

  private bool ReadKsDriversLicenseCsePerson1()
  {
    entities.KsDriversLicense.Populated = false;
    entities.CsePerson.Populated = false;

    return Read("ReadKsDriversLicenseCsePerson1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "ltr30DayDate", local.ZeroDate.Date.GetValueOrDefault());
        db.SetString(command, "numb", local.ReadOnlyCsePerson.Number);
        db.SetInt32(
          command, "sequenceCounter",
          local.ReadOnlyKsDriversLicense.SequenceCounter);
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
        entities.KsDriversLicense.Attribute30DayLetterCreatedDate =
          db.GetNullableDate(reader, 7);
        entities.KsDriversLicense.ServiceCompleteInd =
          db.GetNullableString(reader, 8);
        entities.KsDriversLicense.ServiceCompleteDate =
          db.GetNullableDate(reader, 9);
        entities.KsDriversLicense.RestrictedDate =
          db.GetNullableDate(reader, 10);
        entities.KsDriversLicense.ReinstatedDate =
          db.GetNullableDate(reader, 11);
        entities.KsDriversLicense.AppealReceivedDate =
          db.GetNullableDate(reader, 12);
        entities.KsDriversLicense.AppealResolved =
          db.GetNullableString(reader, 13);
        entities.KsDriversLicense.PaymentAgreementDate =
          db.GetNullableDate(reader, 14);
        entities.KsDriversLicense.FirstPaymentDueDate =
          db.GetNullableDate(reader, 15);
        entities.KsDriversLicense.ManualDate = db.GetNullableDate(reader, 16);
        entities.KsDriversLicense.LicenseRestrictionSentDate =
          db.GetNullableDate(reader, 17);
        entities.KsDriversLicense.RestrictionStatus =
          db.GetNullableString(reader, 18);
        entities.KsDriversLicense.AmountOwed =
          db.GetNullableDecimal(reader, 19);
        entities.KsDriversLicense.AmountDue = db.GetNullableDecimal(reader, 20);
        entities.KsDriversLicense.RecordClosureReason =
          db.GetNullableString(reader, 21);
        entities.KsDriversLicense.RecordClosureDate =
          db.GetNullableDate(reader, 22);
        entities.KsDriversLicense.LastUpdatedBy =
          db.GetNullableString(reader, 23);
        entities.KsDriversLicense.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 24);
        entities.KsDriversLicense.ProcessedInd =
          db.GetNullableString(reader, 25);
        entities.KsDriversLicense.SequenceCounter = db.GetInt32(reader, 26);
        entities.CsePerson.Type1 = db.GetString(reader, 27);
        entities.CsePerson.DateOfDeath = db.GetNullableDate(reader, 28);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 29);
        entities.KsDriversLicense.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private IEnumerable<bool> ReadKsDriversLicenseCsePerson2()
  {
    entities.ReadOnlyCsePerson.Populated = false;
    entities.ReadOnlyKsDriversLicense.Populated = false;

    return ReadEach("ReadKsDriversLicenseCsePerson2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "courtesyLtrDate", local.ZeroDate.Date.GetValueOrDefault());
        db.SetString(command, "cspNum", local.ReadOnlyCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ReadOnlyKsDriversLicense.CspNum = db.GetString(reader, 0);
        entities.ReadOnlyCsePerson.Number = db.GetString(reader, 0);
        entities.ReadOnlyKsDriversLicense.ValidationDate =
          db.GetNullableDate(reader, 1);
        entities.ReadOnlyKsDriversLicense.CourtesyLetterSentDate =
          db.GetNullableDate(reader, 2);
        entities.ReadOnlyKsDriversLicense.Attribute30DayLetterCreatedDate =
          db.GetNullableDate(reader, 3);
        entities.ReadOnlyKsDriversLicense.ServiceCompleteInd =
          db.GetNullableString(reader, 4);
        entities.ReadOnlyKsDriversLicense.ServiceCompleteDate =
          db.GetNullableDate(reader, 5);
        entities.ReadOnlyKsDriversLicense.RestrictedDate =
          db.GetNullableDate(reader, 6);
        entities.ReadOnlyKsDriversLicense.ReinstatedDate =
          db.GetNullableDate(reader, 7);
        entities.ReadOnlyKsDriversLicense.ManualDate =
          db.GetNullableDate(reader, 8);
        entities.ReadOnlyKsDriversLicense.LicenseRestrictionSentDate =
          db.GetNullableDate(reader, 9);
        entities.ReadOnlyKsDriversLicense.RestrictionStatus =
          db.GetNullableString(reader, 10);
        entities.ReadOnlyKsDriversLicense.RecordClosureReason =
          db.GetNullableString(reader, 11);
        entities.ReadOnlyKsDriversLicense.SequenceCounter =
          db.GetInt32(reader, 12);
        entities.ReadOnlyCsePerson.Populated = true;
        entities.ReadOnlyKsDriversLicense.Populated = true;

        return true;
      });
  }

  private bool ReadLegalAction1()
  {
    System.Diagnostics.Debug.Assert(entities.N2dRead.Populated);
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction1",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          entities.N2dRead.LgaIdentifier.GetValueOrDefault());
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

  private bool ReadLegalAction2()
  {
    System.Diagnostics.Debug.Assert(entities.KsDriversLicense.Populated);
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction2",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          entities.KsDriversLicense.LgaIdentifier.GetValueOrDefault());
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

  private bool ReadObligationAdmActionExemption()
  {
    entities.ObligationAdmActionExemption.Populated = false;

    return Read("ReadObligationAdmActionExemption",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableString(
          command, "standardNo", entities.LegalAction.StandardNumber ?? "");
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
      });
  }

  private void UpdateKsDriversLicense1()
  {
    System.Diagnostics.Debug.Assert(entities.N2dRead.Populated);

    var licenseRestrictionSentDate = local.StartDate.Date;
    var restrictionStatus = "REQUEST SENT";
    var amountOwed = local.OwedAmount.TotalCurrency;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTstamp = Now();

    entities.N2dRead.Populated = false;
    Update("UpdateKsDriversLicense1",
      (db, command) =>
      {
        db.
          SetNullableDate(command, "restrictSentDt", licenseRestrictionSentDate);
          
        db.SetNullableString(command, "restrictionStatus", restrictionStatus);
        db.SetNullableDecimal(command, "amountOwed", amountOwed);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetString(command, "cspNum", entities.N2dRead.CspNum);
        db.
          SetInt32(command, "sequenceCounter", entities.N2dRead.SequenceCounter);
          
      });

    entities.N2dRead.LicenseRestrictionSentDate = licenseRestrictionSentDate;
    entities.N2dRead.RestrictionStatus = restrictionStatus;
    entities.N2dRead.AmountOwed = amountOwed;
    entities.N2dRead.LastUpdatedBy = lastUpdatedBy;
    entities.N2dRead.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.N2dRead.Populated = true;
  }

  private void UpdateKsDriversLicense2()
  {
    System.Diagnostics.Debug.Assert(entities.N2dRead.Populated);

    var recordClosureReason = local.RecordError.Text15;
    var recordClosureDate = local.StartDate.Date;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTstamp = Now();

    entities.N2dRead.Populated = false;
    Update("UpdateKsDriversLicense2",
      (db, command) =>
      {
        db.SetNullableString(command, "recClosureReason", recordClosureReason);
        db.SetNullableDate(command, "recordClosureDt", recordClosureDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetString(command, "cspNum", entities.N2dRead.CspNum);
        db.
          SetInt32(command, "sequenceCounter", entities.N2dRead.SequenceCounter);
          
      });

    entities.N2dRead.RecordClosureReason = recordClosureReason;
    entities.N2dRead.RecordClosureDate = recordClosureDate;
    entities.N2dRead.LastUpdatedBy = lastUpdatedBy;
    entities.N2dRead.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.N2dRead.Populated = true;
  }

  private void UpdateKsDriversLicense3()
  {
    System.Diagnostics.Debug.Assert(entities.N2dRead.Populated);

    var manualInd = local.ReinstatedPrev.Item.ReinstatedCodePrev.Flag;
    var manualDate = local.StartDate.Date;
    var restrictionStatus = "REINSTATEMENT SENT";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTstamp = Now();

    entities.N2dRead.Populated = false;
    Update("UpdateKsDriversLicense3",
      (db, command) =>
      {
        db.SetNullableString(command, "manualInd", manualInd);
        db.SetNullableDate(command, "manualDate", manualDate);
        db.SetNullableString(command, "restrictionStatus", restrictionStatus);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetString(command, "cspNum", entities.N2dRead.CspNum);
        db.
          SetInt32(command, "sequenceCounter", entities.N2dRead.SequenceCounter);
          
      });

    entities.N2dRead.ManualInd = manualInd;
    entities.N2dRead.ManualDate = manualDate;
    entities.N2dRead.RestrictionStatus = restrictionStatus;
    entities.N2dRead.LastUpdatedBy = lastUpdatedBy;
    entities.N2dRead.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.N2dRead.Populated = true;
  }

  private void UpdateKsDriversLicense4()
  {
    System.Diagnostics.Debug.Assert(entities.KsDriversLicense.Populated);

    var processedInd = "Y";

    entities.KsDriversLicense.Populated = false;
    Update("UpdateKsDriversLicense4",
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

  private void UpdateKsDriversLicense5()
  {
    System.Diagnostics.Debug.Assert(entities.KsDriversLicense.Populated);
    entities.KsDriversLicense.Populated = false;
    Update("UpdateKsDriversLicense5",
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
    /// A value of TotalNumErrorRecords.
    /// </summary>
    [JsonPropertyName("totalNumErrorRecords")]
    public Common TotalNumErrorRecords
    {
      get => totalNumErrorRecords ??= new();
      set => totalNumErrorRecords = value;
    }

    private Common totalAmtDebtOwed;
    private Common totalNumRecsAdded;
    private Common numberOfRecordsRead;
    private Common totalNumErrorRecords;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A ReinstatedPrevGroup group.</summary>
    [Serializable]
    public class ReinstatedPrevGroup
    {
      /// <summary>
      /// A value of ReinstatedCodePrev.
      /// </summary>
      [JsonPropertyName("reinstatedCodePrev")]
      public Common ReinstatedCodePrev
      {
        get => reinstatedCodePrev ??= new();
        set => reinstatedCodePrev = value;
      }

      /// <summary>
      /// A value of ReinstatedPrevKsDriversLicense.
      /// </summary>
      [JsonPropertyName("reinstatedPrevKsDriversLicense")]
      public KsDriversLicense ReinstatedPrevKsDriversLicense
      {
        get => reinstatedPrevKsDriversLicense ??= new();
        set => reinstatedPrevKsDriversLicense = value;
      }

      /// <summary>
      /// A value of ReinstatedPrevCommon.
      /// </summary>
      [JsonPropertyName("reinstatedPrevCommon")]
      public Common ReinstatedPrevCommon
      {
        get => reinstatedPrevCommon ??= new();
        set => reinstatedPrevCommon = value;
      }

      /// <summary>
      /// A value of ReinstatedPrevLegalAction.
      /// </summary>
      [JsonPropertyName("reinstatedPrevLegalAction")]
      public LegalAction ReinstatedPrevLegalAction
      {
        get => reinstatedPrevLegalAction ??= new();
        set => reinstatedPrevLegalAction = value;
      }

      /// <summary>
      /// A value of ReinstatedPrevKdmvFile.
      /// </summary>
      [JsonPropertyName("reinstatedPrevKdmvFile")]
      public KdmvFile ReinstatedPrevKdmvFile
      {
        get => reinstatedPrevKdmvFile ??= new();
        set => reinstatedPrevKdmvFile = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 25;

      private Common reinstatedCodePrev;
      private KsDriversLicense reinstatedPrevKsDriversLicense;
      private Common reinstatedPrevCommon;
      private LegalAction reinstatedPrevLegalAction;
      private KdmvFile reinstatedPrevKdmvFile;
    }

    /// <summary>A ClearGroup group.</summary>
    [Serializable]
    public class ClearGroup
    {
      /// <summary>
      /// A value of ReasonCodeClear.
      /// </summary>
      [JsonPropertyName("reasonCodeClear")]
      public Common ReasonCodeClear
      {
        get => reasonCodeClear ??= new();
        set => reasonCodeClear = value;
      }

      /// <summary>
      /// A value of ClearKsDriversLicense.
      /// </summary>
      [JsonPropertyName("clearKsDriversLicense")]
      public KsDriversLicense ClearKsDriversLicense
      {
        get => clearKsDriversLicense ??= new();
        set => clearKsDriversLicense = value;
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
      /// A value of ClearKdmvFile.
      /// </summary>
      [JsonPropertyName("clearKdmvFile")]
      public KdmvFile ClearKdmvFile
      {
        get => clearKdmvFile ??= new();
        set => clearKdmvFile = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 25;

      private Common reasonCodeClear;
      private KsDriversLicense clearKsDriversLicense;
      private LegalAction clearLegalAction;
      private KdmvFile clearKdmvFile;
    }

    /// <summary>A ReinstatedGroup group.</summary>
    [Serializable]
    public class ReinstatedGroup
    {
      /// <summary>
      /// A value of ReinstatedCode.
      /// </summary>
      [JsonPropertyName("reinstatedCode")]
      public Common ReinstatedCode
      {
        get => reinstatedCode ??= new();
        set => reinstatedCode = value;
      }

      /// <summary>
      /// A value of ReinstatedKsDriversLicense.
      /// </summary>
      [JsonPropertyName("reinstatedKsDriversLicense")]
      public KsDriversLicense ReinstatedKsDriversLicense
      {
        get => reinstatedKsDriversLicense ??= new();
        set => reinstatedKsDriversLicense = value;
      }

      /// <summary>
      /// A value of ReinstatedCommon.
      /// </summary>
      [JsonPropertyName("reinstatedCommon")]
      public Common ReinstatedCommon
      {
        get => reinstatedCommon ??= new();
        set => reinstatedCommon = value;
      }

      /// <summary>
      /// A value of ReinstatedLegalAction.
      /// </summary>
      [JsonPropertyName("reinstatedLegalAction")]
      public LegalAction ReinstatedLegalAction
      {
        get => reinstatedLegalAction ??= new();
        set => reinstatedLegalAction = value;
      }

      /// <summary>
      /// A value of ReinstatedKdmvFile.
      /// </summary>
      [JsonPropertyName("reinstatedKdmvFile")]
      public KdmvFile ReinstatedKdmvFile
      {
        get => reinstatedKdmvFile ??= new();
        set => reinstatedKdmvFile = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 25;

      private Common reinstatedCode;
      private KsDriversLicense reinstatedKsDriversLicense;
      private Common reinstatedCommon;
      private LegalAction reinstatedLegalAction;
      private KdmvFile reinstatedKdmvFile;
    }

    /// <summary>A SufValuesGroup group.</summary>
    [Serializable]
    public class SufValuesGroup
    {
      /// <summary>
      /// A value of SufValues1.
      /// </summary>
      [JsonPropertyName("sufValues1")]
      public CodeValue SufValues1
      {
        get => sufValues1 ??= new();
        set => sufValues1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 250;

      private CodeValue sufValues1;
    }

    /// <summary>
    /// A value of ClearReinstateCode.
    /// </summary>
    [JsonPropertyName("clearReinstateCode")]
    public Common ClearReinstateCode
    {
      get => clearReinstateCode ??= new();
      set => clearReinstateCode = value;
    }

    /// <summary>
    /// A value of ClearKsDriversLicense.
    /// </summary>
    [JsonPropertyName("clearKsDriversLicense")]
    public KsDriversLicense ClearKsDriversLicense
    {
      get => clearKsDriversLicense ??= new();
      set => clearKsDriversLicense = value;
    }

    /// <summary>
    /// A value of ClearCommon.
    /// </summary>
    [JsonPropertyName("clearCommon")]
    public Common ClearCommon
    {
      get => clearCommon ??= new();
      set => clearCommon = value;
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
    /// A value of ClearKdmvFile.
    /// </summary>
    [JsonPropertyName("clearKdmvFile")]
    public KdmvFile ClearKdmvFile
    {
      get => clearKdmvFile ??= new();
      set => clearKdmvFile = value;
    }

    /// <summary>
    /// A value of TestDate.
    /// </summary>
    [JsonPropertyName("testDate")]
    public KsDriversLicense TestDate
    {
      get => testDate ??= new();
      set => testDate = value;
    }

    /// <summary>
    /// A value of ReinstateReasonCode.
    /// </summary>
    [JsonPropertyName("reinstateReasonCode")]
    public Common ReinstateReasonCode
    {
      get => reinstateReasonCode ??= new();
      set => reinstateReasonCode = value;
    }

    /// <summary>
    /// A value of PreviousProcessKsDriversLicense.
    /// </summary>
    [JsonPropertyName("previousProcessKsDriversLicense")]
    public KsDriversLicense PreviousProcessKsDriversLicense
    {
      get => previousProcessKsDriversLicense ??= new();
      set => previousProcessKsDriversLicense = value;
    }

    /// <summary>
    /// A value of LastKsDriversLicense.
    /// </summary>
    [JsonPropertyName("lastKsDriversLicense")]
    public KsDriversLicense LastKsDriversLicense
    {
      get => lastKsDriversLicense ??= new();
      set => lastKsDriversLicense = value;
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
    /// A value of ReadOnlyCsePerson.
    /// </summary>
    [JsonPropertyName("readOnlyCsePerson")]
    public CsePerson ReadOnlyCsePerson
    {
      get => readOnlyCsePerson ??= new();
      set => readOnlyCsePerson = value;
    }

    /// <summary>
    /// A value of LastUpdateFailedRequest.
    /// </summary>
    [JsonPropertyName("lastUpdateFailedRequest")]
    public Common LastUpdateFailedRequest
    {
      get => lastUpdateFailedRequest ??= new();
      set => lastUpdateFailedRequest = value;
    }

    /// <summary>
    /// A value of LastUpdateRestrictRequ.
    /// </summary>
    [JsonPropertyName("lastUpdateRestrictRequ")]
    public Common LastUpdateRestrictRequ
    {
      get => lastUpdateRestrictRequ ??= new();
      set => lastUpdateRestrictRequ = value;
    }

    /// <summary>
    /// A value of LastUpdateReinstateRequ.
    /// </summary>
    [JsonPropertyName("lastUpdateReinstateRequ")]
    public Common LastUpdateReinstateRequ
    {
      get => lastUpdateReinstateRequ ??= new();
      set => lastUpdateReinstateRequ = value;
    }

    /// <summary>
    /// A value of LastUpdateRestrictRead.
    /// </summary>
    [JsonPropertyName("lastUpdateRestrictRead")]
    public Common LastUpdateRestrictRead
    {
      get => lastUpdateRestrictRead ??= new();
      set => lastUpdateRestrictRead = value;
    }

    /// <summary>
    /// A value of LastUpdateReinstateRead.
    /// </summary>
    [JsonPropertyName("lastUpdateReinstateRead")]
    public Common LastUpdateReinstateRead
    {
      get => lastUpdateReinstateRead ??= new();
      set => lastUpdateReinstateRead = value;
    }

    /// <summary>
    /// A value of LastUpdatedNumOfErrors.
    /// </summary>
    [JsonPropertyName("lastUpdatedNumOfErrors")]
    public Common LastUpdatedNumOfErrors
    {
      get => lastUpdatedNumOfErrors ??= new();
      set => lastUpdatedNumOfErrors = value;
    }

    /// <summary>
    /// A value of NumberOfFailedRestrictsWorkArea.
    /// </summary>
    [JsonPropertyName("numberOfFailedRestrictsWorkArea")]
    public WorkArea NumberOfFailedRestrictsWorkArea
    {
      get => numberOfFailedRestrictsWorkArea ??= new();
      set => numberOfFailedRestrictsWorkArea = value;
    }

    /// <summary>
    /// A value of NumReinstateRead.
    /// </summary>
    [JsonPropertyName("numReinstateRead")]
    public WorkArea NumReinstateRead
    {
      get => numReinstateRead ??= new();
      set => numReinstateRead = value;
    }

    /// <summary>
    /// A value of NumOfReinstateRequestWorkArea.
    /// </summary>
    [JsonPropertyName("numOfReinstateRequestWorkArea")]
    public WorkArea NumOfReinstateRequestWorkArea
    {
      get => numOfReinstateRequestWorkArea ??= new();
      set => numOfReinstateRequestWorkArea = value;
    }

    /// <summary>
    /// A value of NumberOfFailedRestrictsCommon.
    /// </summary>
    [JsonPropertyName("numberOfFailedRestrictsCommon")]
    public Common NumberOfFailedRestrictsCommon
    {
      get => numberOfFailedRestrictsCommon ??= new();
      set => numberOfFailedRestrictsCommon = value;
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
    /// A value of NumberOfReinstateReads.
    /// </summary>
    [JsonPropertyName("numberOfReinstateReads")]
    public Common NumberOfReinstateReads
    {
      get => numberOfReinstateReads ??= new();
      set => numberOfReinstateReads = value;
    }

    /// <summary>
    /// A value of NumOfRestrictReads.
    /// </summary>
    [JsonPropertyName("numOfRestrictReads")]
    public Common NumOfRestrictReads
    {
      get => numOfRestrictReads ??= new();
      set => numOfRestrictReads = value;
    }

    /// <summary>
    /// A value of NumOfRestrictRequest.
    /// </summary>
    [JsonPropertyName("numOfRestrictRequest")]
    public Common NumOfRestrictRequest
    {
      get => numOfRestrictRequest ??= new();
      set => numOfRestrictRequest = value;
    }

    /// <summary>
    /// A value of NumOfReinstateRequestCommon.
    /// </summary>
    [JsonPropertyName("numOfReinstateRequestCommon")]
    public Common NumOfReinstateRequestCommon
    {
      get => numOfReinstateRequestCommon ??= new();
      set => numOfReinstateRequestCommon = value;
    }

    /// <summary>
    /// A value of PaymentTo.
    /// </summary>
    [JsonPropertyName("paymentTo")]
    public DateWorkArea PaymentTo
    {
      get => paymentTo ??= new();
      set => paymentTo = value;
    }

    /// <summary>
    /// A value of RecordError.
    /// </summary>
    [JsonPropertyName("recordError")]
    public WorkArea RecordError
    {
      get => recordError ??= new();
      set => recordError = value;
    }

    /// <summary>
    /// A value of ErrorReason.
    /// </summary>
    [JsonPropertyName("errorReason")]
    public TextWorkArea ErrorReason
    {
      get => errorReason ??= new();
      set => errorReason = value;
    }

    /// <summary>
    /// A value of FailureReturnCode.
    /// </summary>
    [JsonPropertyName("failureReturnCode")]
    public Common FailureReturnCode
    {
      get => failureReturnCode ??= new();
      set => failureReturnCode = value;
    }

    /// <summary>
    /// A value of PlaceHolder.
    /// </summary>
    [JsonPropertyName("placeHolder")]
    public Common PlaceHolder
    {
      get => placeHolder ??= new();
      set => placeHolder = value;
    }

    /// <summary>
    /// A value of CloseAllApRecords.
    /// </summary>
    [JsonPropertyName("closeAllApRecords")]
    public Common CloseAllApRecords
    {
      get => closeAllApRecords ??= new();
      set => closeAllApRecords = value;
    }

    /// <summary>
    /// A value of FromDate.
    /// </summary>
    [JsonPropertyName("fromDate")]
    public DateWorkArea FromDate
    {
      get => fromDate ??= new();
      set => fromDate = value;
    }

    /// <summary>
    /// A value of IwoCreatedDate.
    /// </summary>
    [JsonPropertyName("iwoCreatedDate")]
    public DateWorkArea IwoCreatedDate
    {
      get => iwoCreatedDate ??= new();
      set => iwoCreatedDate = value;
    }

    /// <summary>
    /// A value of RestrictReinstateProcess.
    /// </summary>
    [JsonPropertyName("restrictReinstateProcess")]
    public Common RestrictReinstateProcess
    {
      get => restrictReinstateProcess ??= new();
      set => restrictReinstateProcess = value;
    }

    /// <summary>
    /// Gets a value of ReinstatedPrev.
    /// </summary>
    [JsonIgnore]
    public Array<ReinstatedPrevGroup> ReinstatedPrev => reinstatedPrev ??= new(
      ReinstatedPrevGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ReinstatedPrev for json serialization.
    /// </summary>
    [JsonPropertyName("reinstatedPrev")]
    [Computed]
    public IList<ReinstatedPrevGroup> ReinstatedPrev_Json
    {
      get => reinstatedPrev;
      set => ReinstatedPrev.Assign(value);
    }

    /// <summary>
    /// A value of ReinstateProcess.
    /// </summary>
    [JsonPropertyName("reinstateProcess")]
    public Common ReinstateProcess
    {
      get => reinstateProcess ??= new();
      set => reinstateProcess = value;
    }

    /// <summary>
    /// A value of PrevApTotalCtOrder.
    /// </summary>
    [JsonPropertyName("prevApTotalCtOrder")]
    public Common PrevApTotalCtOrder
    {
      get => prevApTotalCtOrder ??= new();
      set => prevApTotalCtOrder = value;
    }

    /// <summary>
    /// A value of PrevNumCtOrdReinstated.
    /// </summary>
    [JsonPropertyName("prevNumCtOrdReinstated")]
    public Common PrevNumCtOrdReinstated
    {
      get => prevNumCtOrdReinstated ??= new();
      set => prevNumCtOrdReinstated = value;
    }

    /// <summary>
    /// A value of ApReinstate.
    /// </summary>
    [JsonPropertyName("apReinstate")]
    public Common ApReinstate
    {
      get => apReinstate ??= new();
      set => apReinstate = value;
    }

    /// <summary>
    /// A value of CtOrderReinstate.
    /// </summary>
    [JsonPropertyName("ctOrderReinstate")]
    public Common CtOrderReinstate
    {
      get => ctOrderReinstate ??= new();
      set => ctOrderReinstate = value;
    }

    /// <summary>
    /// Gets a value of Clear.
    /// </summary>
    [JsonIgnore]
    public Array<ClearGroup> Clear => clear ??= new(ClearGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Clear for json serialization.
    /// </summary>
    [JsonPropertyName("clear")]
    [Computed]
    public IList<ClearGroup> Clear_Json
    {
      get => clear;
      set => Clear.Assign(value);
    }

    /// <summary>
    /// Gets a value of Reinstated.
    /// </summary>
    [JsonIgnore]
    public Array<ReinstatedGroup> Reinstated => reinstated ??= new(
      ReinstatedGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Reinstated for json serialization.
    /// </summary>
    [JsonPropertyName("reinstated")]
    [Computed]
    public IList<ReinstatedGroup> Reinstated_Json
    {
      get => reinstated;
      set => Reinstated.Assign(value);
    }

    /// <summary>
    /// A value of NumCtOrdersReinstated.
    /// </summary>
    [JsonPropertyName("numCtOrdersReinstated")]
    public Common NumCtOrdersReinstated
    {
      get => numCtOrdersReinstated ??= new();
      set => numCtOrdersReinstated = value;
    }

    /// <summary>
    /// A value of ApTotalCtOrder.
    /// </summary>
    [JsonPropertyName("apTotalCtOrder")]
    public Common ApTotalCtOrder
    {
      get => apTotalCtOrder ??= new();
      set => apTotalCtOrder = value;
    }

    /// <summary>
    /// A value of CurrentCsePerson.
    /// </summary>
    [JsonPropertyName("currentCsePerson")]
    public CsePerson CurrentCsePerson
    {
      get => currentCsePerson ??= new();
      set => currentCsePerson = value;
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
    /// A value of PaymentAmountMade.
    /// </summary>
    [JsonPropertyName("paymentAmountMade")]
    public Common PaymentAmountMade
    {
      get => paymentAmountMade ??= new();
      set => paymentAmountMade = value;
    }

    /// <summary>
    /// A value of WageWithholdingFound.
    /// </summary>
    [JsonPropertyName("wageWithholdingFound")]
    public Common WageWithholdingFound
    {
      get => wageWithholdingFound ??= new();
      set => wageWithholdingFound = value;
    }

    /// <summary>
    /// A value of NumObligorReninstated.
    /// </summary>
    [JsonPropertyName("numObligorReninstated")]
    public Common NumObligorReninstated
    {
      get => numObligorReninstated ??= new();
      set => numObligorReninstated = value;
    }

    /// <summary>
    /// A value of KdmvFile.
    /// </summary>
    [JsonPropertyName("kdmvFile")]
    public KdmvFile KdmvFile
    {
      get => kdmvFile ??= new();
      set => kdmvFile = value;
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
    /// A value of SendA30DayLetter.
    /// </summary>
    [JsonPropertyName("sendA30DayLetter")]
    public Common SendA30DayLetter
    {
      get => sendA30DayLetter ??= new();
      set => sendA30DayLetter = value;
    }

    /// <summary>
    /// A value of NumMonthsOf30DayLtr.
    /// </summary>
    [JsonPropertyName("numMonthsOf30DayLtr")]
    public Common NumMonthsOf30DayLtr
    {
      get => numMonthsOf30DayLtr ??= new();
      set => numMonthsOf30DayLtr = value;
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
    /// A value of Local30DayLetterSent.
    /// </summary>
    [JsonPropertyName("local30DayLetterSent")]
    public DateWorkArea Local30DayLetterSent
    {
      get => local30DayLetterSent ??= new();
      set => local30DayLetterSent = value;
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
    /// A value of PaymentDueDateFrom.
    /// </summary>
    [JsonPropertyName("paymentDueDateFrom")]
    public DateWorkArea PaymentDueDateFrom
    {
      get => paymentDueDateFrom ??= new();
      set => paymentDueDateFrom = value;
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
    /// A value of SpecifiedTimeFrameDate.
    /// </summary>
    [JsonPropertyName("specifiedTimeFrameDate")]
    public DateWorkArea SpecifiedTimeFrameDate
    {
      get => specifiedTimeFrameDate ??= new();
      set => specifiedTimeFrameDate = value;
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
    /// A value of NumberOfDays.
    /// </summary>
    [JsonPropertyName("numberOfDays")]
    public Common NumberOfDays
    {
      get => numberOfDays ??= new();
      set => numberOfDays = value;
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
    /// A value of MinimumAmountOwed.
    /// </summary>
    [JsonPropertyName("minimumAmountOwed")]
    public Common MinimumAmountOwed
    {
      get => minimumAmountOwed ??= new();
      set => minimumAmountOwed = value;
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
    /// A value of CurrentCommon.
    /// </summary>
    [JsonPropertyName("currentCommon")]
    public Common CurrentCommon
    {
      get => currentCommon ??= new();
      set => currentCommon = value;
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
    /// A value of Read.
    /// </summary>
    [JsonPropertyName("read")]
    public LegalAction Read
    {
      get => read ??= new();
      set => read = value;
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
    /// A value of PreviousProcessCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("previousProcessCsePersonsWorkSet")]
    public CsePersonsWorkSet PreviousProcessCsePersonsWorkSet
    {
      get => previousProcessCsePersonsWorkSet ??= new();
      set => previousProcessCsePersonsWorkSet = value;
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
    /// A value of ClearCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("clearCsePersonsWorkSet")]
    public CsePersonsWorkSet ClearCsePersonsWorkSet
    {
      get => clearCsePersonsWorkSet ??= new();
      set => clearCsePersonsWorkSet = value;
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
    /// A value of RecordCount.
    /// </summary>
    [JsonPropertyName("recordCount")]
    public Common RecordCount
    {
      get => recordCount ??= new();
      set => recordCount = value;
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
    /// A value of RestartAmount.
    /// </summary>
    [JsonPropertyName("restartAmount")]
    public WorkArea RestartAmount
    {
      get => restartAmount ??= new();
      set => restartAmount = value;
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
    /// A value of NumRestictRead.
    /// </summary>
    [JsonPropertyName("numRestictRead")]
    public WorkArea NumRestictRead
    {
      get => numRestictRead ??= new();
      set => numRestictRead = value;
    }

    /// <summary>
    /// A value of NumOfRestrictsRequest.
    /// </summary>
    [JsonPropertyName("numOfRestrictsRequest")]
    public WorkArea NumOfRestrictsRequest
    {
      get => numOfRestrictsRequest ??= new();
      set => numOfRestrictsRequest = value;
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
    /// Gets a value of SufValues.
    /// </summary>
    [JsonIgnore]
    public Array<SufValuesGroup> SufValues => sufValues ??= new(
      SufValuesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of SufValues for json serialization.
    /// </summary>
    [JsonPropertyName("sufValues")]
    [Computed]
    public IList<SufValuesGroup> SufValues_Json
    {
      get => sufValues;
      set => SufValues.Assign(value);
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
    /// A value of TotalErrorRecords.
    /// </summary>
    [JsonPropertyName("totalErrorRecords")]
    public Common TotalErrorRecords
    {
      get => totalErrorRecords ??= new();
      set => totalErrorRecords = value;
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

    private Common clearReinstateCode;
    private KsDriversLicense clearKsDriversLicense;
    private Common clearCommon;
    private LegalAction clearLegalAction;
    private KdmvFile clearKdmvFile;
    private KsDriversLicense testDate;
    private Common reinstateReasonCode;
    private KsDriversLicense previousProcessKsDriversLicense;
    private KsDriversLicense lastKsDriversLicense;
    private KsDriversLicense readOnlyKsDriversLicense;
    private CsePerson readOnlyCsePerson;
    private Common lastUpdateFailedRequest;
    private Common lastUpdateRestrictRequ;
    private Common lastUpdateReinstateRequ;
    private Common lastUpdateRestrictRead;
    private Common lastUpdateReinstateRead;
    private Common lastUpdatedNumOfErrors;
    private WorkArea numberOfFailedRestrictsWorkArea;
    private WorkArea numReinstateRead;
    private WorkArea numOfReinstateRequestWorkArea;
    private Common numberOfFailedRestrictsCommon;
    private Common numberOfErrors;
    private Common numberOfReinstateReads;
    private Common numOfRestrictReads;
    private Common numOfRestrictRequest;
    private Common numOfReinstateRequestCommon;
    private DateWorkArea paymentTo;
    private WorkArea recordError;
    private TextWorkArea errorReason;
    private Common failureReturnCode;
    private Common placeHolder;
    private Common closeAllApRecords;
    private DateWorkArea fromDate;
    private DateWorkArea iwoCreatedDate;
    private Common restrictReinstateProcess;
    private Array<ReinstatedPrevGroup> reinstatedPrev;
    private Common reinstateProcess;
    private Common prevApTotalCtOrder;
    private Common prevNumCtOrdReinstated;
    private Common apReinstate;
    private Common ctOrderReinstate;
    private Array<ClearGroup> clear;
    private Array<ReinstatedGroup> reinstated;
    private Common numCtOrdersReinstated;
    private Common apTotalCtOrder;
    private CsePerson currentCsePerson;
    private CsePerson previousCsePerson;
    private Common paymentAmountMade;
    private Common wageWithholdingFound;
    private Common numObligorReninstated;
    private KdmvFile kdmvFile;
    private Common numOfObligorsProcessed;
    private Common sendA30DayLetter;
    private Common numMonthsOf30DayLtr;
    private DateWorkArea local30DayLetterTimeFrame;
    private DateWorkArea local30DayLetterSent;
    private DateWorkArea courtesyLetterSentDt;
    private DateWorkArea paymentDueDateFrom;
    private Common ltrWithinTimeFrame;
    private Document document;
    private DateWorkArea specifiedTimeFrameDate;
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
    private Common numberOfDays;
    private WorkArea workArea;
    private Common minimumAmountOwed;
    private ExitStateWorkArea exitStateWorkArea;
    private External passArea;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private TextWorkArea postion;
    private Common currentPosition;
    private Common fieldNumber;
    private Common currentCommon;
    private Common startCommon;
    private Common includeArrearsOnly;
    private CsePerson csePerson;
    private DateWorkArea zeroDate;
    private LegalAction read;
    private Common owedAmount;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePersonsWorkSet previousProcessCsePersonsWorkSet;
    private LegalAction legalAction;
    private LegalAction previousLegalAction;
    private Common previousPersonTotal;
    private Common personTotal;
    private Common search;
    private Common phonetic;
    private CsePersonsWorkSet startCsePersonsWorkSet;
    private CsePerson alreadyCheckedSsn;
    private CsePersonsWorkSet clearCsePersonsWorkSet;
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
    private Common recordCount;
    private WorkArea restartLegalId;
    private WorkArea restartAmount;
    private WorkArea totalAmount;
    private WorkArea numRestictRead;
    private WorkArea numOfRestrictsRequest;
    private WorkArea totalNumberOfErrors;
    private Array<SufValuesGroup> sufValues;
    private CsePerson error;
    private Common totalErrorRecords;
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
    /// A value of ReadOnlyCsePerson.
    /// </summary>
    [JsonPropertyName("readOnlyCsePerson")]
    public CsePerson ReadOnlyCsePerson
    {
      get => readOnlyCsePerson ??= new();
      set => readOnlyCsePerson = value;
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
    /// A value of N2dRead.
    /// </summary>
    [JsonPropertyName("n2dRead")]
    public KsDriversLicense N2dRead
    {
      get => n2dRead ??= new();
      set => n2dRead = value;
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
    /// A value of CsePersonLicense.
    /// </summary>
    [JsonPropertyName("csePersonLicense")]
    public CsePersonLicense CsePersonLicense
    {
      get => csePersonLicense ??= new();
      set => csePersonLicense = value;
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
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public CsePersonAccount Previous
    {
      get => previous ??= new();
      set => previous = value;
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
    /// A value of AdministrativeAction.
    /// </summary>
    [JsonPropertyName("administrativeAction")]
    public AdministrativeAction AdministrativeAction
    {
      get => administrativeAction ??= new();
      set => administrativeAction = value;
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
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
    }

    /// <summary>
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

    private CsePerson readOnlyCsePerson;
    private KsDriversLicense readOnlyKsDriversLicense;
    private KsDriversLicense n2dRead;
    private KsDriversLicense ksDriversLicense;
    private CsePersonLicense csePersonLicense;
    private CsePerson csePerson;
    private LegalAction legalAction;
    private CaseRole caseRole;
    private CsePersonAccount obligor;
    private Obligation obligation;
    private ObligationType obligationType;
    private Case1 case1;
    private CaseUnit caseUnit;
    private LegalActionCaseRole legalActionCaseRole;
    private CsePersonAccount previous;
    private Infrastructure infrastructure;
    private AdministrativeAction administrativeAction;
    private ObligationAdmActionExemption obligationAdmActionExemption;
    private CodeValue codeValue;
    private Code code;
  }
#endregion
}
