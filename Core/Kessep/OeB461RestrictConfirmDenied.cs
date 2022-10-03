// Program: OE_B461_RESTRICT_CONFIRM_DENIED, ID: 371393303, model: 746.
// Short name: SWEE461B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B461_RESTRICT_CONFIRM_DENIED.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class OeB461RestrictConfirmDenied: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B461_RESTRICT_CONFIRM_DENIED program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB461RestrictConfirmDenied(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB461RestrictConfirmDenied.
  /// </summary>
  public OeB461RestrictConfirmDenied(IContext context, Import import,
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
    // 07/28/2008      DDupree   	Initial Creation - WR280420
    // ***********************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    UseOeB461Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.StartCommon.Count = 1;
    local.Current.Count = 1;
    local.CurrentPosition.Count = 1;
    local.FieldNumber.Count = 0;
    local.TotalEmailCharacterCnt.Count = 0;

    do
    {
      local.Postion.Text1 =
        Substring(local.ProgramProcessingInfo.ParameterList,
        local.CurrentPosition.Count, 1);

      if (AsChar(local.Postion.Text1) == ';')
      {
        ++local.FieldNumber.Count;
        local.WorkArea.Text15 = "";

        if (local.FieldNumber.Count == 1)
        {
          local.ToEmailAddress.Text80 = "";

          if (local.Current.Count == 1)
          {
            local.ToEmailAddress.Text80 = "";
          }
          else
          {
            local.ToEmailAddress.Text80 =
              Substring(local.ProgramProcessingInfo.ParameterList,
              local.StartCommon.Count, local.Current.Count - 1);
          }

          local.TotalEmailCharacterCnt.Count += local.Current.Count;
          local.StartCommon.Count = local.CurrentPosition.Count + 1;
          local.Current.Count = 0;

          break;
        }
        else
        {
        }
      }

      ++local.CurrentPosition.Count;
      ++local.Current.Count;

      if (local.CurrentPosition.Count >= 240)
      {
        break;

        // we do not want to get into an endless loop, if for some reason there 
        // is no
        // delimiter in the parameter then when we will get to the last 
        // character we will escape.
      }
    }
    while(!Equal(global.Command, "COMMAND"));

    local.NullDate.Date = new DateTime(1, 1, 1);
    local.StartDate.Date = local.ProgramProcessingInfo.ProcessDate;
    local.StartBatchTimestampWorkArea.IefTimestamp = Now();
    local.NumLicenseRestrictedWorkArea.Text15 =
      Substring(local.ProgramCheckpointRestart.RestartInfo, 16, 15);
    local.NumRestrictDeniedRecsWorkArea.Text15 =
      Substring(local.ProgramCheckpointRestart.RestartInfo, 31, 15);
    local.NumErrorRecordsWorkArea.Text15 =
      Substring(local.ProgramCheckpointRestart.RestartInfo, 46, 15);
    local.NumReinstateDeniedRecsWorkArea.Text15 =
      Substring(local.ProgramCheckpointRestart.RestartInfo, 61, 15);
    local.NumLicenseReinstated.Text15 =
      Substring(local.ProgramCheckpointRestart.RestartInfo, 76, 15);
    local.RecordType4Found.Flag =
      Substring(local.ProgramCheckpointRestart.RestartInfo, 91, 1);
    local.RestartCount.Count =
      local.ProgramCheckpointRestart.CheckpointCount.GetValueOrDefault();
    local.NumLicenseRestrictedCommon.Count =
      (int)StringToNumber(local.NumLicenseRestrictedWorkArea.Text15);
    local.NumRestrictDeniedRecsCommon.Count =
      (int)StringToNumber(local.NumRestrictDeniedRecsWorkArea.Text15);
    local.NumErrorRecordsCommon.Count =
      (int)StringToNumber(local.NumErrorRecordsWorkArea.Text15);
    local.NumReinstateDeniedRecsCommon.Count =
      (int)StringToNumber(local.NumReinstateDeniedRecsWorkArea.Text15);
    local.NumLicenseReinstate.Count =
      (int)StringToNumber(local.NumLicenseReinstated.Text15);
    local.RecordType4Found.Flag = "";
    local.AdministrativeAction.Type1 = "KDMV";

    if (!ReadAdministrativeAction())
    {
      ExitState = "ADMINISTRATIVE_ACTION_NF";

      return;
    }

    do
    {
      local.PassArea.FileInstruction = "READ";
      UseOeEabRestrictConfirmDenied();

      if (Equal(local.PassArea.TextReturnCode, "EF"))
      {
        break;
      }

      if (!Equal(local.PassArea.TextReturnCode, "00"))
      {
        ExitState = "FILE_READ_ERROR_WITH_RB";

        break;
      }

      local.Date.Text10 =
        Substring(local.KdmvFile.ProcessDate, KdmvFile.ProcessDate_MaxLength, 1,
        4) + "-" + Substring
        (local.KdmvFile.ProcessDate, KdmvFile.ProcessDate_MaxLength, 5, 2) + "-"
        + Substring
        (local.KdmvFile.ProcessDate, KdmvFile.ProcessDate_MaxLength, 7, 2);
      local.ProcessDate.Date = StringToDate(local.Date.Text10);

      if (AsChar(local.KdmvFile.FileType) == '1' || AsChar
        (local.KdmvFile.FileType) == '2')
      {
        local.Test.FileType = "A";

        // restrict type
      }
      else
      {
        local.Test.FileType = "B";

        // reinstate type
      }

      local.DateFound.Flag = "";

      for(local.ProcessDates.Index = 0; local.ProcessDates.Index < local
        .ProcessDates.Count; ++local.ProcessDates.Index)
      {
        if (!local.ProcessDates.CheckSize())
        {
          break;
        }

        if (Equal(local.ProcessDate.Date,
          local.ProcessDates.Item.ProcessDate.Date) && AsChar
          (local.Test.FileType) == AsChar
          (local.ProcessDates.Item.RecGroupType.FileType))
        {
          local.DateFound.Flag = "Y";

          break;
        }
      }

      local.ProcessDates.CheckIndex();

      if (AsChar(local.DateFound.Flag) != 'Y')
      {
        local.ProcessDates.Index = local.ProcessDates.Count;
        local.ProcessDates.CheckSize();

        local.ProcessDates.Update.ProcessDate.Date = local.ProcessDate.Date;
        local.ProcessDates.Update.ProcessDate.TextDate =
          local.KdmvFile.ProcessDate;
        local.ProcessDates.Update.RecType.FileType = local.KdmvFile.FileType;
        local.ProcessDates.Update.RecGroupType.FileType = local.Test.FileType;
      }

      ++local.TotalNumberProcessed.Count;

      if (local.TotalNumberProcessed.Count <= local.RestartCount.Count && local
        .RestartCount.Count > 0)
      {
        continue;
      }

      ExitState = "ACO_NN0000_ALL_OK";

      if (AsChar(local.KdmvFile.FileType) == '1')
      {
        // the ap driver's license has been restricted
        // we will read the ks driver's license table by the incoming cse person
        // number and
        // where request restrict date is greater than null and restrict date is
        // less than or
        // equal to null date
        foreach(var item in ReadCsePersonKsDriversLicense1())
        {
          // need to make sure that the record has not already been processed, 
          // check the
          // restrict date field, if is less than or equal to a null date then 
          // it has not been
          // processed before otherwise do not process it.
          if (Equal(local.CsePerson.Number, entities.CsePerson.Number))
          {
          }
          else
          {
            local.CsePerson.Number = entities.CsePerson.Number;
            local.HighestCtOrdAmt.TotalCurrency = 0;
            local.LowestCtOrdAmt.TotalCurrency = 0;
            local.PersonTotal.TotalCurrency = 0;
            local.NumberOfCtOrders.Count = 0;
          }

          if (ReadLegalAction())
          {
            local.LegalAction.StandardNumber =
              entities.LegalAction.StandardNumber;
          }
          else
          {
            ExitState = "LEGAL_ACTION_NF";
            local.Error.Number = entities.CsePerson.Number;
            ++local.NumErrorRecordsCommon.Count;

            goto Test;
          }

          // the local owed amount field is gotten from the  current ks driver's
          // license record - it was calucated when we requested the
          // restriction
          local.OwedAmount.TotalCurrency =
            entities.KsDriversLicense.AmountOwed.GetValueOrDefault();
          local.Infrastructure.Assign(local.Clear);

          // This is for history record, it is not needed unless we add a record
          // but if a case
          // can not be found for the current obligor and the current court 
          // order then it should
          // be errored out now not after we add other records and alerts.
          foreach(var item1 in ReadCaseCaseUnitCaseRole2())
          {
            // It really does not matter about wheater the case role is active 
            // or not we are just
            // concern with the highest cae nunber associated with the cout 
            // order that is being
            // worked - money is still owed therefore it is still in play. So we
            // are not going to
            // check start and end dates
            if (Lt(local.Infrastructure.CaseNumber, entities.Case1.Number))
            {
              local.Infrastructure.CaseNumber = entities.Case1.Number;
              local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
            }
          }

          if (IsEmpty(local.Infrastructure.CaseNumber))
          {
            // If a case is not found it is probablybecause the case has been 
            // close, it it has
            // been close that means the debt has been paid off and the the 
            // driver's license
            // has been reinstated. At this point we will just let this record 
            // error out.
            ExitState = "CASE_NF";
            ++local.NumErrorRecordsCommon.Count;
            local.Error.Number = entities.CsePerson.Number;

            goto Test;
          }

          local.FinanceWorkAttributes.NumericalDollarValue =
            local.OwedAmount.TotalCurrency;
          UseFnCabReturnTextDollars();
          local.Infrastructure.SituationNumber = 0;
          local.Infrastructure.ReasonCode = "LICENSERESTRICT";
          local.Infrastructure.EventId = 1;
          local.Infrastructure.EventType = "ADMINACT";
          local.Infrastructure.ProcessStatus = "Q";
          local.Infrastructure.UserId = "KDMV";
          local.Infrastructure.BusinessObjectCd = "ENF";
          local.Infrastructure.ReferenceDate = local.StartDate.Date;
          local.Infrastructure.CreatedBy = local.ProgramProcessingInfo.Name;
          local.Infrastructure.EventDetailName = "KDMV License Restricted";
          local.Infrastructure.CsePersonNumber = entities.CsePerson.Number;
          local.Infrastructure.DenormNumeric12 =
            entities.LegalAction.Identifier;
          local.Infrastructure.DenormText12 =
            entities.LegalAction.CourtCaseNumber;
          local.Detail.Text11 = ", Arrears $";
          local.Infrastructure.Detail =
            "KDMV license restriction, Ct Order # " + TrimEnd
            (entities.LegalAction.StandardNumber) + local.Detail.Text11 + local
            .FinanceWorkAttributes.TextDollarValue;
          local.Infrastructure.CaseNumber = entities.Case1.Number;
          local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;

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
          UseSpCabCreateInfrastructure();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            ++local.NumErrorRecordsCommon.Count;
            local.Error.Number = entities.CsePerson.Number;

            goto Test;
          }

          // we will now update the current ks driver's license  record
          // we will be setting the restricted date to batch run date
          //       If the restricted status is not reinstated
          //                setting the restircted status to license restricted
          //        else
          //              the restricted status will be left as reinstated
          try
          {
            UpdateKsDriversLicense2();

            // set the lowest and highest court order amounts per AP, this will 
            // be for the oblo
            // record that we will create when the process finishes the current 
            // AP
            if (local.LowestCtOrdAmt.TotalCurrency <= 0)
            {
              local.HighestCtOrdAmt.TotalCurrency =
                local.OwedAmount.TotalCurrency;
              local.LowestCtOrdAmt.TotalCurrency =
                local.OwedAmount.TotalCurrency;
            }
            else
            {
              if (local.OwedAmount.TotalCurrency < local
                .LowestCtOrdAmt.TotalCurrency)
              {
                local.LowestCtOrdAmt.TotalCurrency =
                  local.OwedAmount.TotalCurrency;
              }

              if (local.OwedAmount.TotalCurrency > local
                .HighestCtOrdAmt.TotalCurrency)
              {
                local.HighestCtOrdAmt.TotalCurrency =
                  local.OwedAmount.TotalCurrency;
              }
            }
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "KS_DRIVERS_LICENSE_NU";
                ++local.NumErrorRecordsCommon.Count;
                local.Error.Number = entities.CsePerson.Number;

                goto Test;
              case ErrorCode.PermittedValueViolation:
                ExitState = "KS_DRIVERS_LICENSE_PV";
                ++local.NumErrorRecordsCommon.Count;
                local.Error.Number = entities.CsePerson.Number;

                goto Test;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          ++local.NumberOfCtOrders.Count;
          local.PersonTotal.TotalCurrency += local.OwedAmount.TotalCurrency;
          local.RecordsSuccesfullyProces.Flag = "Y";
        }

        if (AsChar(local.RecordsSuccesfullyProces.Flag) == 'Y')
        {
          // we only want to write out one record per obligor
          // **********************************************************************************
          // Create a KDMV record for the oblo screen
          // **********************************************************************************
          if (!ReadObligor())
          {
            ++local.NumErrorRecordsCommon.Count;
            local.Error.Number = entities.CsePerson.Number;
            ExitState = "FN0000_OBLIGOR_NF_RB";

            return;
          }

          try
          {
            CreateKsDeptMotorVeh();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ++local.NumErrorRecordsCommon.Count;
                local.Error.Number = entities.CsePerson.Number;
                ExitState = "ADMIN_ACT_CERT_AR_RB";

                return;
              case ErrorCode.PermittedValueViolation:
                ++local.NumErrorRecordsCommon.Count;
                local.Error.Number = entities.CsePerson.Number;
                ExitState = "ADMIN_ACT_CERTIFICATION_PV_RB";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          ++local.NumLicenseRestrictedCommon.Count;
        }
        else
        {
          ExitState = "KS_DRIVERS_LICENSE_NF";
          ++local.NumErrorRecordsCommon.Count;
          local.Error.Number = local.KdmvFile.CsePersonNumber;
        }
      }
      else if (AsChar(local.KdmvFile.FileType) == '2')
      {
        // the ap's driver's license was not restricted for some reason
        // we will read the ks driver's license table by the incoming cse person
        // number and
        // where request restrict date is greater than null
        ExitState = "ACO_NN0000_ALL_OK";

        foreach(var item in ReadCsePersonKsDriversLicense1())
        {
          // need to make sure that the record has not already been processed, 
          // check the
          // restrict date field, if is less than or equal to a null date then 
          // it has not been
          // processed before otherwise do not process it.
          if (Equal(local.CsePerson.Number, entities.CsePerson.Number))
          {
          }
          else
          {
            local.CsePerson.Number = entities.CsePerson.Number;
          }

          if (ReadLegalAction())
          {
            local.LegalAction.StandardNumber =
              entities.LegalAction.StandardNumber;
          }
          else
          {
            ExitState = "LEGAL_ACTION_NF";
            local.Error.Number = entities.CsePerson.Number;
            ++local.NumErrorRecordsCommon.Count;

            goto Test;
          }

          local.Infrastructure.Assign(local.Clear);

          foreach(var item1 in ReadCaseCaseUnitCaseRole1())
          {
            if (Lt(local.Infrastructure.CaseNumber, entities.Case1.Number))
            {
              local.Infrastructure.CaseNumber = entities.Case1.Number;
              local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
            }
          }

          if (IsEmpty(local.Infrastructure.CaseNumber))
          {
            // If a case is not found it is probablybecause the case has been 
            // close, it it has
            // been close that means the debt has been paid off and the the 
            // driver's license
            // has been reinstated. At this point we will just let this record 
            // error out.
            ExitState = "CASE_NF";
            ++local.NumErrorRecordsCommon.Count;
            local.Error.Number = entities.CsePerson.Number;

            goto Test;
          }

          local.Infrastructure.SituationNumber = 0;
          local.Infrastructure.ReasonCode = "RESTRICTDENIED";
          local.Infrastructure.EventId = 1;
          local.Infrastructure.EventType = "ADMINACT";
          local.Infrastructure.ProcessStatus = "Q";
          local.Infrastructure.UserId = "KDMV";
          local.Infrastructure.BusinessObjectCd = "ENF";
          local.Infrastructure.ReferenceDate = local.StartDate.Date;
          local.Infrastructure.CreatedBy = local.ProgramProcessingInfo.Name;
          local.Infrastructure.EventDetailName =
            "DMV License Restriction Denied";
          local.Infrastructure.CsePersonNumber = entities.CsePerson.Number;
          local.Infrastructure.DenormNumeric12 =
            entities.LegalAction.Identifier;
          local.Infrastructure.DenormText12 =
            entities.LegalAction.CourtCaseNumber;
          local.Infrastructure.Detail = "Ct Odr # " + TrimEnd
            (entities.LegalAction.StandardNumber) + " Reason: " + local
            .KdmvFile.DmvProblemText;
          local.Infrastructure.CaseNumber = entities.Case1.Number;
          local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;

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
          UseSpCabCreateInfrastructure();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            ++local.NumErrorRecordsCommon.Count;
            local.Error.Number = entities.CsePerson.Number;

            goto Test;
          }

          // we will now update the current ks driver's license  record
          // we will be setting the restricted date to batch run date
          //       If the restricted status is not reinstated
          //                setting the restircted status to denied
          //        else
          //              the restricted status will be left as reinstated
          // we will also being setting the record status with the status code 
          // past back from dmv
          // will also be setting the error explaniation with what is past back 
          // from dmv
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
                ++local.NumErrorRecordsCommon.Count;
                local.Error.Number = entities.CsePerson.Number;

                goto Test;
              case ErrorCode.PermittedValueViolation:
                ExitState = "KS_DRIVERS_LICENSE_PV";
                ++local.NumErrorRecordsCommon.Count;
                local.Error.Number = entities.CsePerson.Number;

                goto Test;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          local.RecordsSuccesfullyProces.Flag = "Y";
        }

        if (AsChar(local.RecordsSuccesfullyProces.Flag) != 'Y')
        {
          // no record was found the ces person number on the ks driver's 
          // license table, we will write out a error record
          ExitState = "KS_DRIVERS_LICENSE_NF";
          ++local.NumErrorRecordsCommon.Count;
          local.Error.Number = local.KdmvFile.CsePersonNumber;

          goto Test;
        }

        ++local.NumRestrictDeniedRecsCommon.Count;
        local.RecordsSuccesfullyProces.Flag = "Y";
      }
      else if (AsChar(local.KdmvFile.FileType) == '3')
      {
        // the ap driver's license has been reinstated
        // we will read the ks driver's license table by the incoming cse person
        // number and
        // where request reinstatement date (stored in the field manul date) is 
        // greater than
        // null and reinstate date is less than or equal to null date
        foreach(var item in ReadCsePersonKsDriversLicense2())
        {
          // need to make sure that the record has not already been processed, 
          // check the
          // reinstated date field, if is less than or equal to a null date then
          // it has not been
          // processed before otherwise do not process it.
          if (Equal(local.CsePerson.Number, entities.CsePerson.Number))
          {
          }
          else
          {
            local.CsePerson.Number = entities.CsePerson.Number;
          }

          if (ReadLegalAction())
          {
            local.LegalAction.StandardNumber =
              entities.LegalAction.StandardNumber;
          }
          else
          {
            ExitState = "LEGAL_ACTION_NF";
            local.Error.Number = entities.CsePerson.Number;
            ++local.NumErrorRecordsCommon.Count;

            goto Test;
          }

          // This is for history record, it is not needed unless we add a record
          // but if a case
          // can not be found for the current obligor and the current court 
          // order then it should
          // be errored out now not after we add other records and alerts.
          local.Infrastructure.Assign(local.Clear);

          foreach(var item1 in ReadCaseCaseUnitCaseRole2())
          {
            // It really does not matter about wheater the case role is active 
            // or not we are just
            // concern with the highest cae nunber associated with the cout 
            // order that is being
            // worked - money is still owed therefore it is still in play. So we
            // are not going to
            // check start and end dates
            if (Lt(local.Infrastructure.CaseNumber, entities.Case1.Number))
            {
              local.Infrastructure.CaseNumber = entities.Case1.Number;
              local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
            }
          }

          if (IsEmpty(local.Infrastructure.CaseNumber))
          {
            foreach(var item1 in ReadCaseCaseUnitCaseRole3())
            {
              // It really does not matter about wheater the case role is active
              // or not we are just
              // concern with the highest cae nunber associated with the cout 
              // order that is being
              // worked - money is still owed therefore it is still in play. So 
              // we are not going to
              // check start and end dates
              if (Lt(local.Infrastructure.CaseNumber, entities.Case1.Number))
              {
                local.Infrastructure.CaseNumber = entities.Case1.Number;
                local.Infrastructure.CaseUnitNumber =
                  entities.CaseUnit.CuNumber;
              }
            }
          }

          if (IsEmpty(local.Infrastructure.CaseNumber))
          {
            // If a case is not found it is probably because the case has been 
            // close, it it has
            // been close that means the debt has been paid off and the the 
            // driver's license
            // has been reinstated. At this point we will just let this record 
            // error out.
            ExitState = "CASE_NF";
            ++local.NumErrorRecordsCommon.Count;
            local.Error.Number = entities.CsePerson.Number;

            goto Test;
          }

          local.Infrastructure.SituationNumber = 0;
          local.Infrastructure.ReasonCode = "LICENSEREINSTAT";
          local.Infrastructure.EventId = 1;
          local.Infrastructure.EventType = "ADMINACT";
          local.Infrastructure.ProcessStatus = "Q";
          local.Infrastructure.UserId = "KDMV";
          local.Infrastructure.BusinessObjectCd = "ENF";
          local.Infrastructure.ReferenceDate = local.StartDate.Date;
          local.Infrastructure.CreatedBy = local.ProgramProcessingInfo.Name;
          local.Infrastructure.EventDetailName = "KDMV License Reinstatement";
          local.Infrastructure.CsePersonNumber = entities.CsePerson.Number;
          local.Infrastructure.DenormNumeric12 =
            entities.LegalAction.Identifier;
          local.Infrastructure.DenormText12 =
            entities.LegalAction.CourtCaseNumber;

          switch(AsChar(entities.KsDriversLicense.ManualInd))
          {
            case 'A':
              local.Detail.Text32 = " - Obligation Exempted";

              break;
            case 'B':
              local.Detail.Text32 = " - Wage Withholding Established";

              break;
            case 'C':
              local.Detail.Text32 = "- Pmt made per Payment Agreement";

              break;
            case 'D':
              local.Detail.Text32 = " - Debt Amount Paid in Full";

              break;
            default:
              break;
          }

          local.Infrastructure.Detail = "Ct Order # " + TrimEnd
            (entities.LegalAction.StandardNumber) + local.Detail.Text32;
          local.Infrastructure.CaseNumber = entities.Case1.Number;
          local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;

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
          UseSpCabCreateInfrastructure();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            ++local.NumErrorRecordsCommon.Count;
            local.Error.Number = entities.CsePerson.Number;

            goto Test;
          }

          // we will now update the current ks driver's license  record
          // we will be setting the restricted date to batch run date
          //       If the restricted status is not reinstated
          //                setting the restircted status to license restricted
          //        else
          //              the restricted status will be left as reinstated
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
                ++local.NumErrorRecordsCommon.Count;
                local.Error.Number = entities.CsePerson.Number;

                goto Test;
              case ErrorCode.PermittedValueViolation:
                ExitState = "KS_DRIVERS_LICENSE_PV";
                ++local.NumErrorRecordsCommon.Count;
                local.Error.Number = entities.CsePerson.Number;

                goto Test;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          local.RecordsSuccesfullyProces.Flag = "Y";
        }

        if (AsChar(local.RecordsSuccesfullyProces.Flag) != 'Y')
        {
          // no record was found the ces person number on the ks driver's 
          // license table, we will write out a error record
          ExitState = "KS_DRIVERS_LICENSE_NF";
          ++local.NumErrorRecordsCommon.Count;
          local.Error.Number = local.KdmvFile.CsePersonNumber;

          goto Test;
        }

        ++local.NumLicenseReinstate.Count;
      }
      else if (AsChar(local.KdmvFile.FileType) == '4')
      {
        // the ap's driver's license was not restricted for some reason
        // we will read the ks driver's license table by the incoming cse person
        // number and
        // where request restrict date is greater than null
        ExitState = "ACO_NN0000_ALL_OK";

        foreach(var item in ReadCsePersonKsDriversLicense2())
        {
          // need to make sure that the record has not already been processed, 
          // check the
          // restrict date field, if is less than or equal to a null date then 
          // it has not been
          // processed before otherwise do not process it.
          if (Equal(local.CsePerson.Number, entities.CsePerson.Number))
          {
          }
          else
          {
            local.CsePerson.Number = entities.CsePerson.Number;
          }

          if (ReadLegalAction())
          {
            local.LegalAction.StandardNumber =
              entities.LegalAction.StandardNumber;
          }
          else
          {
            ExitState = "LEGAL_ACTION_NF";
            local.Error.Number = entities.CsePerson.Number;
            ++local.NumErrorRecordsCommon.Count;

            goto Test;
          }

          local.Infrastructure.Assign(local.Clear);

          foreach(var item1 in ReadCaseCaseUnitCaseRole1())
          {
            if (Lt(local.Infrastructure.CaseNumber, entities.Case1.Number))
            {
              local.Infrastructure.CaseNumber = entities.Case1.Number;
              local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
            }
          }

          if (IsEmpty(local.Infrastructure.CaseNumber))
          {
            // If a case is not found it is probablybecause the case has been 
            // close, it it has
            // been close that means the debt has been paid off and the the 
            // driver's license
            // has been reinstated. At this point we will just let this record 
            // error out.
            ExitState = "CASE_NF";
            ++local.NumErrorRecordsCommon.Count;
            local.Error.Number = entities.CsePerson.Number;

            goto Test;
          }

          local.Infrastructure.SituationNumber = 0;
          local.Infrastructure.ReasonCode = "REINSTATEMANUAL";
          local.Infrastructure.EventId = 1;
          local.Infrastructure.EventType = "ADMINACT";
          local.Infrastructure.ProcessStatus = "Q";
          local.Infrastructure.UserId = "KDMV";
          local.Infrastructure.BusinessObjectCd = "ENF";
          local.Infrastructure.ReferenceDate = local.StartDate.Date;
          local.Infrastructure.CreatedBy = local.ProgramProcessingInfo.Name;
          local.Infrastructure.EventDetailName = "DMV License Reinstate Manual";
          local.Infrastructure.CsePersonNumber = entities.CsePerson.Number;
          local.Infrastructure.DenormNumeric12 =
            entities.LegalAction.Identifier;
          local.Infrastructure.DenormText12 =
            entities.LegalAction.CourtCaseNumber;
          local.Infrastructure.Detail = "Ct Odr # " + TrimEnd
            (entities.LegalAction.StandardNumber) + " Reason: " + local
            .KdmvFile.DmvProblemText;
          local.Infrastructure.CaseNumber = entities.Case1.Number;
          local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;

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
          UseSpCabCreateInfrastructure();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            ++local.NumErrorRecordsCommon.Count;
            local.Error.Number = entities.CsePerson.Number;

            goto Test;
          }

          // we will now update the current ks driver's license  record
          // we will be setting the reinstated date to batch run date
          // the restricted status will be set to reinstated manually
          // will also be setting the error explaniation with what is past back 
          // from dmv
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
                ++local.NumErrorRecordsCommon.Count;
                local.Error.Number = entities.CsePerson.Number;

                goto Test;
              case ErrorCode.PermittedValueViolation:
                ExitState = "KS_DRIVERS_LICENSE_PV";
                ++local.NumErrorRecordsCommon.Count;
                local.Error.Number = entities.CsePerson.Number;

                goto Test;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          local.RecordType4Found.Flag = "Y";
          local.RecordsSuccesfullyProces.Flag = "Y";
        }

        if (AsChar(local.RecordsSuccesfullyProces.Flag) != 'Y')
        {
          // no record was found the ces person number on the ks driver's 
          // license table, we will write out a error record
          ExitState = "KS_DRIVERS_LICENSE_NF";
          ++local.NumErrorRecordsCommon.Count;
          local.Error.Number = local.KdmvFile.CsePersonNumber;

          goto Test;
        }

        ++local.NumReinstateDeniedRecsCommon.Count;
        local.Name.Text33 = local.KdmvFile.LastName + "   " + local
          .KdmvFile.FirstName;
        local.SsnDob.Text23 = "   " + local.KdmvFile.Ssn + "   " + local
          .KdmvFile.Dob;
        local.PersonNumbDrLicense.Text25 = "   " + local
          .KdmvFile.CsePersonNumber + "   " + local
          .KdmvFile.DriverLicenseNumber;
        local.Problem.Text40 = "    " + local.KdmvFile.DmvProblemText;
        local.MismatchReportSend.RptDetail = local.Name.Text33 + local
          .SsnDob.Text23 + local.PersonNumbDrLicense.Text25 + local
          .Problem.Text40;
        local.EabFileHandling.Action = "WRITE";
        UseCabBusinessReport01();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "FILE_READ_ERROR_WITH_RB";

          break;
        }

        local.RecordsSuccesfullyProces.Flag = "Y";
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

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        local.Error.Number = "";
        ExitState = "ACO_NN0000_ALL_OK";
      }

      local.RestartCount.Count = local.TotalNumberProcessed.Count;
      ++local.RecordCount.Count;

      if (local.RecordCount.Count >= local
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.ProgramName =
          local.ProgramProcessingInfo.Name;
        local.ProgramCheckpointRestart.CheckpointCount =
          local.TotalNumberProcessed.Count - 1;
        local.TotalNumberRecords.Text15 =
          NumberToString(local.TotalNumberProcessed.Count, 15);
        local.NumLicenseRestrictedWorkArea.Text15 =
          NumberToString(local.NumLicenseRestrictedCommon.Count, 15);
        local.NumRestrictDeniedRecsWorkArea.Text15 =
          NumberToString(local.NumRestrictDeniedRecsCommon.Count, 15);
        local.NumErrorRecordsWorkArea.Text15 =
          NumberToString(local.NumErrorRecordsCommon.Count, 15);
        local.NumReinstateDeniedRecsWorkArea.Text15 =
          NumberToString(local.NumReinstateDeniedRecsCommon.Count, 15);
        local.NumLicenseReinstated.Text15 =
          NumberToString(local.NumLicenseReinstate.Count, 15);
        local.ProgramCheckpointRestart.RestartInfo =
          local.TotalNumberRecords.Text15 + local
          .NumLicenseRestrictedWorkArea.Text15 + local
          .NumRestrictDeniedRecsWorkArea.Text15 + local
          .NumErrorRecordsWorkArea.Text15;
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + local
          .NumReinstateDeniedRecsWorkArea.Text15 + local
          .NumLicenseReinstated.Text15;
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + local
          .RecordType4Found.Flag;
        UseUpdatePgmCheckpointRestart();
        UseExtToDoACommit();

        if (local.PassArea.NumericReturnCode != 0)
        {
          ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

          return;
        }

        local.RecordCount.Count = 0;
      }

      // check the ks driver's license entity view, if it is not populated then 
      // this is an error, write a error to the error report
      local.RecordsSuccesfullyProces.Flag = "";
    }
    while(!Equal(local.PassArea.TextReturnCode, "EF"));

    if (local.ProcessDates.Count >= 1)
    {
      // If we receieved any records to be processed then we will check to make 
      // sure
      // we processed all of them for that day.
      local.CsePerson.Number = "";
      local.MissingPerson.Index = -1;
      local.MissingPerson.Count = 0;

      for(local.ProcessDates.Index = 0; local.ProcessDates.Index < local
        .ProcessDates.Count; ++local.ProcessDates.Index)
      {
        if (!local.ProcessDates.CheckSize())
        {
          break;
        }

        local.NumberOfMissingAps.Count = 0;

        if (AsChar(local.ProcessDates.Item.RecGroupType.FileType) == 'A')
        {
          // we are checking for restricted records here
          foreach(var item in ReadKsDriversLicenseCsePerson1())
          {
            if (Equal(entities.CsePerson.Number, local.CsePerson.Number))
            {
              // we do not want duplicates, we only want one record per AP
              continue;
            }

            local.CsePerson.Number = entities.CsePerson.Number;
            ++local.NumberOfMissingAps.Count;

            ++local.MissingPerson.Index;
            local.MissingPerson.CheckSize();

            local.MissingPerson.Update.MissngPerson.Number =
              entities.CsePerson.Number;
            local.MissingPerson.Update.MissingPerson1.TextDate =
              local.ProcessDates.Item.ProcessDate.TextDate;
            local.MissingPerson.Update.MissingRecType.FileType =
              local.ProcessDates.Item.RecType.FileType;
          }

          local.ProcessDates.Update.NumMissngRestrictAp.Count =
            local.NumberOfMissingAps.Count;
        }
        else
        {
          // we are checking for reinstated records here
          foreach(var item in ReadKsDriversLicenseCsePerson2())
          {
            if (Equal(entities.CsePerson.Number, local.CsePerson.Number))
            {
              // we do not want duplicates, we only want one record per AP
              continue;
            }

            local.CsePerson.Number = entities.CsePerson.Number;
            ++local.NumberOfMissingAps.Count;

            ++local.MissingPerson.Index;
            local.MissingPerson.CheckSize();

            local.MissingPerson.Update.MissngPerson.Number =
              entities.CsePerson.Number;
            local.MissingPerson.Update.MissingPerson1.TextDate =
              local.ProcessDates.Item.ProcessDate.TextDate;
            local.MissingPerson.Update.MissingRecType.FileType =
              local.ProcessDates.Item.RecType.FileType;
          }

          local.ProcessDates.Update.NumMissingReinstAp.Count =
            local.NumberOfMissingAps.Count;
        }
      }

      local.ProcessDates.CheckIndex();

      if (local.MissingPerson.Count >= 1)
      {
        for(local.MissingPerson.Index = 0; local.MissingPerson.Index < local
          .MissingPerson.Count; ++local.MissingPerson.Index)
        {
          if (!local.MissingPerson.CheckSize())
          {
            break;
          }

          local.EabFileHandling.Action = "WRITE";

          if (AsChar(local.MissingPerson.Item.MissingRecType.FileType) == '1'
            || AsChar(local.MissingPerson.Item.MissingRecType.FileType) == '2')
          {
            local.EabReportSend.RptDetail =
              "License Sanction request not done for process date : " + TrimEnd
              (local.MissingPerson.Item.MissingPerson1.TextDate) + ", CSE Person # " +
              local.MissingPerson.Item.MissngPerson.Number;
            ++local.NumMissingRestrictedAps.Count;
          }
          else
          {
            local.EabReportSend.RptDetail =
              "License Reinstatement request not done for process date : " + TrimEnd
              (local.MissingPerson.Item.MissingPerson1.TextDate) + ", CSE Person # " +
              local.MissingPerson.Item.MissngPerson.Number;
            ++local.NumMissingReinstateAps.Count;
          }

          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }
        }

        local.MissingPerson.CheckIndex();
      }
    }

    if (AsChar(local.RecordType4Found.Flag) == 'Y')
    {
      if (ReadJob())
      {
        try
        {
          CreateJobRun();
          local.ReportData.Type1 = "H";
          ++local.ReportData.SequenceNumber;
          local.ReportData.LineText =
            "Reinstatement errors have occurred with DMV, check SAR report SRRUN148.";
            

          try
          {
            CreateReportData();
          }
          catch(Exception e1)
          {
            switch(GetErrorCode(e1))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "CO0000_REPORT_DATA_AE_RB";
                ++local.NumErrorRecordsCommon.Count;
                local.Error.Number = "";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "CO0000_REPORT_DATA_PV_RB";
                ++local.NumErrorRecordsCommon.Count;
                local.Error.Number = "";

                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "CO0000_JOB_RUN_AE_AB";
              ++local.NumErrorRecordsCommon.Count;
              local.Error.Number = "";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "CO0000_JOB_RUN_PV_RB";
              ++local.NumErrorRecordsCommon.Count;
              local.Error.Number = "";

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
        ExitState = "CO0000_JOB_NF_AB";
        ++local.NumErrorRecordsCommon.Count;
        local.Error.Number = "";
      }
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // Now wrap things up,
      UseOeB461Close();
      local.PassArea.FileInstruction = "CLOSE";
      UseOeEabRestrictConfirmDenied();
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

      UseOeB461Close();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.TextDate = source.TextDate;
    target.Date = source.Date;
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

  private static void MoveProcessDates(Local.ProcessDatesGroup source,
    OeB461Close.Import.ProcessDatesGroup target)
  {
    System.Diagnostics.Trace.TraceWarning(
      "INFO: source and target of the move do not fit perfectly.");
    MoveDateWorkArea(source.ProcessDate, target.ProcessDate);
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.ProgramName = source.ProgramName;
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
    target.CheckpointCount = source.CheckpointCount;
    target.RestartInfo = source.RestartInfo;
  }

  private void UseCabBusinessReport01()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.MismatchReportSend.RptDetail;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
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

  private int UseGenerate9DigitRandomNumber()
  {
    var useImport = new Generate9DigitRandomNumber.Import();
    var useExport = new Generate9DigitRandomNumber.Export();

    Call(Generate9DigitRandomNumber.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute9DigitRandomNumber;
  }

  private void UseOeB461Close()
  {
    var useImport = new OeB461Close.Import();
    var useExport = new OeB461Close.Export();

    useImport.TotalNumProcessed.Count = local.TotalNumberProcessed.Count;
    useImport.NumLicenseRestricted.Count =
      local.NumLicenseRestrictedCommon.Count;
    useImport.NumRestrictDeniedRecs.Count =
      local.NumRestrictDeniedRecsCommon.Count;
    useImport.NumberOfErrorRecords.Count = local.NumErrorRecordsCommon.Count;
    useImport.NumMissingRestrictAps.Count = local.NumMissingRestrictedAps.Count;
    useImport.NumLicenseReinstated.Count = local.NumLicenseReinstate.Count;
    useImport.NumReinstateDeniedRecs.Count =
      local.NumReinstateDeniedRecsCommon.Count;
    useImport.NumMissingReinstateAps.Count = local.NumMissingReinstateAps.Count;
    local.ProcessDates.CopyTo(useImport.ProcessDates, MoveProcessDates);

    Call(OeB461Close.Execute, useImport, useExport);
  }

  private void UseOeB461Housekeeping()
  {
    var useImport = new OeB461Housekeeping.Import();
    var useExport = new OeB461Housekeeping.Export();

    Call(OeB461Housekeeping.Execute, useImport, useExport);

    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseOeEabRestrictConfirmDenied()
  {
    var useImport = new OeEabRestrictConfirmDenied.Import();
    var useExport = new OeEabRestrictConfirmDenied.Export();

    useImport.External.FileInstruction = local.PassArea.FileInstruction;
    useExport.External.Assign(local.PassArea);
    useExport.KdmvFile.Assign(local.KdmvFile);

    Call(OeEabRestrictConfirmDenied.Execute, useImport, useExport);

    local.PassArea.Assign(useExport.External);
    local.KdmvFile.Assign(useExport.KdmvFile);
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

  private void CreateJobRun()
  {
    var startTimestamp = Now();
    var status = "WAIT";
    var outputType = "WORDPROC-P";
    var emailAddress = Substring(local.ToEmailAddress.Text80, 1, 50);
    var jobName = entities.Job.Name;
    var systemGenId = UseGenerate9DigitRandomNumber();

    entities.JobRun.Populated = false;
    Update("CreateJobRun",
      (db, command) =>
      {
        db.SetDateTime(command, "startTimestamp", startTimestamp);
        db.SetNullableDateTime(command, "endTimestamp", null);
        db.SetNullableString(command, "zdelUserId", "");
        db.SetNullableString(command, "zdelPersonNumber", "");
        db.SetNullableInt32(command, "zdelLegActionId", 0);
        db.SetString(command, "status", status);
        db.SetNullableString(command, "printerId", "");
        db.SetString(command, "outputType", outputType);
        db.SetNullableString(command, "errorMsg", "");
        db.SetNullableString(command, "emailAddress", emailAddress);
        db.SetNullableString(command, "parmInfo", "");
        db.SetString(command, "jobName", jobName);
        db.SetInt32(command, "systemGenId", systemGenId);
      });

    entities.JobRun.StartTimestamp = startTimestamp;
    entities.JobRun.EndTimestamp = null;
    entities.JobRun.Status = status;
    entities.JobRun.PrinterId = "";
    entities.JobRun.OutputType = outputType;
    entities.JobRun.ErrorMsg = "";
    entities.JobRun.EmailAddress = emailAddress;
    entities.JobRun.ParmInfo = "";
    entities.JobRun.JobName = jobName;
    entities.JobRun.SystemGenId = systemGenId;
    entities.JobRun.Populated = true;
  }

  private void CreateKsDeptMotorVeh()
  {
    System.Diagnostics.Debug.Assert(entities.Obligor.Populated);

    var cpaType = entities.Obligor.Type1;
    var cspNumber = entities.Obligor.CspNumber;
    var type1 = entities.AdministrativeAction.Type1;
    var takenDate = local.StartDate.Date;
    var originalAmount = local.PersonTotal.TotalCurrency;
    var createdBy = global.UserId;
    var createdTstamp = Now();
    var numberOfCourtOrders = local.NumberOfCtOrders.Count;
    var lowestCourtOrderAmount = local.LowestCtOrdAmt.TotalCurrency;
    var highestCourtOrderAmount = local.HighestCtOrdAmt.TotalCurrency;

    CheckValid<AdministrativeActCertification>("CpaType", cpaType);
    CheckValid<AdministrativeActCertification>("Type1", type1);
    entities.KsDeptMotorVeh.Populated = false;
    Update("CreateKsDeptMotorVeh",
      (db, command) =>
      {
        db.SetString(command, "cpaType", cpaType);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "type", type1);
        db.SetDate(command, "takenDt", takenDate);
        db.SetNullableString(command, "aatType", type1);
        db.SetNullableDecimal(command, "originalAmt", originalAmount);
        db.SetNullableDecimal(command, "currentAmt", originalAmount);
        db.SetNullableDate(command, "currentAmtDt", takenDate);
        db.SetNullableDate(command, "decertifiedDt", null);
        db.SetNullableDate(command, "notificationDt", null);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetNullableString(command, "lastUpdatedBy", createdBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", createdTstamp);
        db.SetNullableDate(command, "referralDt", default(DateTime));
        db.SetNullableDecimal(command, "recoveryAmt", 0M);
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
        db.SetNullableString(command, "addressStreet1", "");
        db.SetNullableString(command, "addressCity", "");
        db.SetNullableString(command, "addressZip", "");
        db.SetNullableInt32(command, "numCourtOrders", numberOfCourtOrders);
        db.
          SetNullableDecimal(command, "lowestCtOrdAmt", lowestCourtOrderAmount);
          
        db.SetNullableDecimal(
          command, "highestCtOrdAmt", highestCourtOrderAmount);
      });

    entities.KsDeptMotorVeh.CpaType = cpaType;
    entities.KsDeptMotorVeh.CspNumber = cspNumber;
    entities.KsDeptMotorVeh.Type1 = type1;
    entities.KsDeptMotorVeh.TakenDate = takenDate;
    entities.KsDeptMotorVeh.AatType = type1;
    entities.KsDeptMotorVeh.OriginalAmount = originalAmount;
    entities.KsDeptMotorVeh.CurrentAmount = originalAmount;
    entities.KsDeptMotorVeh.CurrentAmountDate = takenDate;
    entities.KsDeptMotorVeh.DecertifiedDate = null;
    entities.KsDeptMotorVeh.NotificationDate = null;
    entities.KsDeptMotorVeh.CreatedBy = createdBy;
    entities.KsDeptMotorVeh.CreatedTstamp = createdTstamp;
    entities.KsDeptMotorVeh.LastUpdatedBy = createdBy;
    entities.KsDeptMotorVeh.LastUpdatedTstamp = createdTstamp;
    entities.KsDeptMotorVeh.NotifiedBy = "";
    entities.KsDeptMotorVeh.DateSent = takenDate;
    entities.KsDeptMotorVeh.TanfCode = "";
    entities.KsDeptMotorVeh.DecertificationReason = "";
    entities.KsDeptMotorVeh.NumberOfCourtOrders = numberOfCourtOrders;
    entities.KsDeptMotorVeh.LowestCourtOrderAmount = lowestCourtOrderAmount;
    entities.KsDeptMotorVeh.HighestCourtOrderAmount = highestCourtOrderAmount;
    entities.KsDeptMotorVeh.Populated = true;
  }

  private void CreateReportData()
  {
    System.Diagnostics.Debug.Assert(entities.JobRun.Populated);

    var type1 = local.ReportData.Type1;
    var sequenceNumber = local.ReportData.SequenceNumber;
    var firstPageOnlyInd = "Y";
    var lineText = local.ReportData.LineText;
    var jobName = entities.JobRun.JobName;
    var jruSystemGenId = entities.JobRun.SystemGenId;

    entities.ReportData.Populated = false;
    Update("CreateReportData",
      (db, command) =>
      {
        db.SetString(command, "type", type1);
        db.SetInt32(command, "sequenceNumber", sequenceNumber);
        db.SetNullableString(command, "firstPageOnlyIn", firstPageOnlyInd);
        db.SetString(command, "lineControl", "");
        db.SetString(command, "lineText", lineText);
        db.SetString(command, "jobName", jobName);
        db.SetInt32(command, "jruSystemGenId", jruSystemGenId);
      });

    entities.ReportData.Type1 = type1;
    entities.ReportData.SequenceNumber = sequenceNumber;
    entities.ReportData.FirstPageOnlyInd = firstPageOnlyInd;
    entities.ReportData.LineControl = "";
    entities.ReportData.LineText = lineText;
    entities.ReportData.JobName = jobName;
    entities.ReportData.JruSystemGenId = jruSystemGenId;
    entities.ReportData.Populated = true;
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

  private IEnumerable<bool> ReadCaseCaseUnitCaseRole1()
  {
    entities.CaseRole.Populated = false;
    entities.Case1.Populated = false;
    entities.CaseUnit.Populated = false;

    return ReadEach("ReadCaseCaseUnitCaseRole1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "startDate", local.StartDate.Date.GetValueOrDefault());
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

  private IEnumerable<bool> ReadCaseCaseUnitCaseRole2()
  {
    entities.CaseRole.Populated = false;
    entities.Case1.Populated = false;
    entities.CaseUnit.Populated = false;

    return ReadEach("ReadCaseCaseUnitCaseRole2",
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

  private IEnumerable<bool> ReadCaseCaseUnitCaseRole3()
  {
    entities.CaseRole.Populated = false;
    entities.Case1.Populated = false;
    entities.CaseUnit.Populated = false;

    return ReadEach("ReadCaseCaseUnitCaseRole3",
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

  private IEnumerable<bool> ReadCsePersonKsDriversLicense1()
  {
    entities.KsDriversLicense.Populated = false;
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePersonKsDriversLicense1",
      (db, command) =>
      {
        db.SetString(command, "numb", local.KdmvFile.CsePersonNumber);
        db.SetNullableDate(
          command, "restrictSentDt",
          local.ProcessDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "restrictedDate", local.ZeroDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.KsDriversLicense.CspNum = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.DateOfDeath = db.GetNullableDate(reader, 2);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 3);
        entities.KsDriversLicense.LgaIdentifier =
          db.GetNullableInt32(reader, 4);
        entities.KsDriversLicense.KsDvrLicense =
          db.GetNullableString(reader, 5);
        entities.KsDriversLicense.ValidationDate =
          db.GetNullableDate(reader, 6);
        entities.KsDriversLicense.RestrictedDate =
          db.GetNullableDate(reader, 7);
        entities.KsDriversLicense.ReinstatedDate =
          db.GetNullableDate(reader, 8);
        entities.KsDriversLicense.ManualInd = db.GetNullableString(reader, 9);
        entities.KsDriversLicense.ManualDate = db.GetNullableDate(reader, 10);
        entities.KsDriversLicense.LicenseRestrictionSentDate =
          db.GetNullableDate(reader, 11);
        entities.KsDriversLicense.RestrictionStatus =
          db.GetNullableString(reader, 12);
        entities.KsDriversLicense.AmountOwed =
          db.GetNullableDecimal(reader, 13);
        entities.KsDriversLicense.RecordClosureReason =
          db.GetNullableString(reader, 14);
        entities.KsDriversLicense.RecordClosureDate =
          db.GetNullableDate(reader, 15);
        entities.KsDriversLicense.LastUpdatedBy =
          db.GetNullableString(reader, 16);
        entities.KsDriversLicense.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 17);
        entities.KsDriversLicense.SequenceCounter = db.GetInt32(reader, 18);
        entities.KsDriversLicense.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePersonKsDriversLicense2()
  {
    entities.KsDriversLicense.Populated = false;
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePersonKsDriversLicense2",
      (db, command) =>
      {
        db.SetString(command, "numb", local.KdmvFile.CsePersonNumber);
        db.SetNullableDate(
          command, "manualDate", local.ProcessDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "reinstatedDate", local.ZeroDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.KsDriversLicense.CspNum = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.DateOfDeath = db.GetNullableDate(reader, 2);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 3);
        entities.KsDriversLicense.LgaIdentifier =
          db.GetNullableInt32(reader, 4);
        entities.KsDriversLicense.KsDvrLicense =
          db.GetNullableString(reader, 5);
        entities.KsDriversLicense.ValidationDate =
          db.GetNullableDate(reader, 6);
        entities.KsDriversLicense.RestrictedDate =
          db.GetNullableDate(reader, 7);
        entities.KsDriversLicense.ReinstatedDate =
          db.GetNullableDate(reader, 8);
        entities.KsDriversLicense.ManualInd = db.GetNullableString(reader, 9);
        entities.KsDriversLicense.ManualDate = db.GetNullableDate(reader, 10);
        entities.KsDriversLicense.LicenseRestrictionSentDate =
          db.GetNullableDate(reader, 11);
        entities.KsDriversLicense.RestrictionStatus =
          db.GetNullableString(reader, 12);
        entities.KsDriversLicense.AmountOwed =
          db.GetNullableDecimal(reader, 13);
        entities.KsDriversLicense.RecordClosureReason =
          db.GetNullableString(reader, 14);
        entities.KsDriversLicense.RecordClosureDate =
          db.GetNullableDate(reader, 15);
        entities.KsDriversLicense.LastUpdatedBy =
          db.GetNullableString(reader, 16);
        entities.KsDriversLicense.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 17);
        entities.KsDriversLicense.SequenceCounter = db.GetInt32(reader, 18);
        entities.KsDriversLicense.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private bool ReadJob()
  {
    entities.Job.Populated = false;

    return Read("ReadJob",
      null,
      (db, reader) =>
      {
        entities.Job.Name = db.GetString(reader, 0);
        entities.Job.Populated = true;
      });
  }

  private IEnumerable<bool> ReadKsDriversLicenseCsePerson1()
  {
    entities.KsDriversLicense.Populated = false;
    entities.CsePerson.Populated = false;

    return ReadEach("ReadKsDriversLicenseCsePerson1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "restrictSentDt",
          local.ProcessDates.Item.ProcessDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "restrictedDate", local.NullDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.KsDriversLicense.CspNum = db.GetString(reader, 0);
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.KsDriversLicense.LgaIdentifier =
          db.GetNullableInt32(reader, 1);
        entities.KsDriversLicense.KsDvrLicense =
          db.GetNullableString(reader, 2);
        entities.KsDriversLicense.ValidationDate =
          db.GetNullableDate(reader, 3);
        entities.KsDriversLicense.RestrictedDate =
          db.GetNullableDate(reader, 4);
        entities.KsDriversLicense.ReinstatedDate =
          db.GetNullableDate(reader, 5);
        entities.KsDriversLicense.ManualInd = db.GetNullableString(reader, 6);
        entities.KsDriversLicense.ManualDate = db.GetNullableDate(reader, 7);
        entities.KsDriversLicense.LicenseRestrictionSentDate =
          db.GetNullableDate(reader, 8);
        entities.KsDriversLicense.RestrictionStatus =
          db.GetNullableString(reader, 9);
        entities.KsDriversLicense.AmountOwed =
          db.GetNullableDecimal(reader, 10);
        entities.KsDriversLicense.RecordClosureReason =
          db.GetNullableString(reader, 11);
        entities.KsDriversLicense.RecordClosureDate =
          db.GetNullableDate(reader, 12);
        entities.KsDriversLicense.LastUpdatedBy =
          db.GetNullableString(reader, 13);
        entities.KsDriversLicense.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 14);
        entities.KsDriversLicense.SequenceCounter = db.GetInt32(reader, 15);
        entities.CsePerson.Type1 = db.GetString(reader, 16);
        entities.CsePerson.DateOfDeath = db.GetNullableDate(reader, 17);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 18);
        entities.KsDriversLicense.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadKsDriversLicenseCsePerson2()
  {
    entities.KsDriversLicense.Populated = false;
    entities.CsePerson.Populated = false;

    return ReadEach("ReadKsDriversLicenseCsePerson2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "manualDate",
          local.ProcessDates.Item.ProcessDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "recordClosureDt", local.NullDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.KsDriversLicense.CspNum = db.GetString(reader, 0);
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.KsDriversLicense.LgaIdentifier =
          db.GetNullableInt32(reader, 1);
        entities.KsDriversLicense.KsDvrLicense =
          db.GetNullableString(reader, 2);
        entities.KsDriversLicense.ValidationDate =
          db.GetNullableDate(reader, 3);
        entities.KsDriversLicense.RestrictedDate =
          db.GetNullableDate(reader, 4);
        entities.KsDriversLicense.ReinstatedDate =
          db.GetNullableDate(reader, 5);
        entities.KsDriversLicense.ManualInd = db.GetNullableString(reader, 6);
        entities.KsDriversLicense.ManualDate = db.GetNullableDate(reader, 7);
        entities.KsDriversLicense.LicenseRestrictionSentDate =
          db.GetNullableDate(reader, 8);
        entities.KsDriversLicense.RestrictionStatus =
          db.GetNullableString(reader, 9);
        entities.KsDriversLicense.AmountOwed =
          db.GetNullableDecimal(reader, 10);
        entities.KsDriversLicense.RecordClosureReason =
          db.GetNullableString(reader, 11);
        entities.KsDriversLicense.RecordClosureDate =
          db.GetNullableDate(reader, 12);
        entities.KsDriversLicense.LastUpdatedBy =
          db.GetNullableString(reader, 13);
        entities.KsDriversLicense.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 14);
        entities.KsDriversLicense.SequenceCounter = db.GetInt32(reader, 15);
        entities.CsePerson.Type1 = db.GetString(reader, 16);
        entities.CsePerson.DateOfDeath = db.GetNullableDate(reader, 17);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 18);
        entities.KsDriversLicense.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private bool ReadLegalAction()
  {
    System.Diagnostics.Debug.Assert(entities.KsDriversLicense.Populated);
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
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

  private bool ReadObligor()
  {
    entities.Obligor.Populated = false;

    return Read("ReadObligor",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", local.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Obligor.CspNumber = db.GetString(reader, 0);
        entities.Obligor.Type1 = db.GetString(reader, 1);
        entities.Obligor.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.Obligor.Type1);
      });
  }

  private void UpdateKsDriversLicense1()
  {
    System.Diagnostics.Debug.Assert(entities.KsDriversLicense.Populated);

    var reinstatedDate = local.StartDate.Date;
    var restrictionStatus = "REINSTATED";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTstamp = Now();

    entities.KsDriversLicense.Populated = false;
    Update("UpdateKsDriversLicense1",
      (db, command) =>
      {
        db.SetNullableDate(command, "reinstatedDate", reinstatedDate);
        db.SetNullableString(command, "restrictionStatus", restrictionStatus);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetString(command, "cspNum", entities.KsDriversLicense.CspNum);
        db.SetInt32(
          command, "sequenceCounter",
          entities.KsDriversLicense.SequenceCounter);
      });

    entities.KsDriversLicense.ReinstatedDate = reinstatedDate;
    entities.KsDriversLicense.RestrictionStatus = restrictionStatus;
    entities.KsDriversLicense.LastUpdatedBy = lastUpdatedBy;
    entities.KsDriversLicense.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.KsDriversLicense.Populated = true;
  }

  private void UpdateKsDriversLicense2()
  {
    System.Diagnostics.Debug.Assert(entities.KsDriversLicense.Populated);

    var restrictedDate = local.StartDate.Date;
    var restrictionStatus = "RESTRICTED";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTstamp = Now();

    entities.KsDriversLicense.Populated = false;
    Update("UpdateKsDriversLicense2",
      (db, command) =>
      {
        db.SetNullableDate(command, "restrictedDate", restrictedDate);
        db.SetNullableString(command, "restrictionStatus", restrictionStatus);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetString(command, "cspNum", entities.KsDriversLicense.CspNum);
        db.SetInt32(
          command, "sequenceCounter",
          entities.KsDriversLicense.SequenceCounter);
      });

    entities.KsDriversLicense.RestrictedDate = restrictedDate;
    entities.KsDriversLicense.RestrictionStatus = restrictionStatus;
    entities.KsDriversLicense.LastUpdatedBy = lastUpdatedBy;
    entities.KsDriversLicense.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.KsDriversLicense.Populated = true;
  }

  private void UpdateKsDriversLicense3()
  {
    System.Diagnostics.Debug.Assert(entities.KsDriversLicense.Populated);

    var reinstatedDate = local.StartDate.Date;
    var restrictionStatus = "REINSTATE MANUALLY";
    var recordClosureReason = Substring(local.KdmvFile.DmvProblemText, 1, 18);
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTstamp = Now();

    entities.KsDriversLicense.Populated = false;
    Update("UpdateKsDriversLicense3",
      (db, command) =>
      {
        db.SetNullableDate(command, "reinstatedDate", reinstatedDate);
        db.SetNullableString(command, "restrictionStatus", restrictionStatus);
        db.SetNullableString(command, "recClosureReason", recordClosureReason);
        db.SetNullableDate(command, "recordClosureDt", reinstatedDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetString(command, "cspNum", entities.KsDriversLicense.CspNum);
        db.SetInt32(
          command, "sequenceCounter",
          entities.KsDriversLicense.SequenceCounter);
      });

    entities.KsDriversLicense.ReinstatedDate = reinstatedDate;
    entities.KsDriversLicense.RestrictionStatus = restrictionStatus;
    entities.KsDriversLicense.RecordClosureReason = recordClosureReason;
    entities.KsDriversLicense.RecordClosureDate = reinstatedDate;
    entities.KsDriversLicense.LastUpdatedBy = lastUpdatedBy;
    entities.KsDriversLicense.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.KsDriversLicense.Populated = true;
  }

  private void UpdateKsDriversLicense4()
  {
    System.Diagnostics.Debug.Assert(entities.KsDriversLicense.Populated);

    var restrictionStatus = "RESTRICTION DENIED";
    var recordClosureReason = Substring(local.KdmvFile.DmvProblemText, 1, 18);
    var recordClosureDate = local.StartDate.Date;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTstamp = Now();

    entities.KsDriversLicense.Populated = false;
    Update("UpdateKsDriversLicense4",
      (db, command) =>
      {
        db.SetNullableString(command, "restrictionStatus", restrictionStatus);
        db.SetNullableString(command, "recClosureReason", recordClosureReason);
        db.SetNullableDate(command, "recordClosureDt", recordClosureDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetString(command, "cspNum", entities.KsDriversLicense.CspNum);
        db.SetInt32(
          command, "sequenceCounter",
          entities.KsDriversLicense.SequenceCounter);
      });

    entities.KsDriversLicense.RestrictionStatus = restrictionStatus;
    entities.KsDriversLicense.RecordClosureReason = recordClosureReason;
    entities.KsDriversLicense.RecordClosureDate = recordClosureDate;
    entities.KsDriversLicense.LastUpdatedBy = lastUpdatedBy;
    entities.KsDriversLicense.LastUpdatedTstamp = lastUpdatedTstamp;
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
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A MissingPersonGroup group.</summary>
    [Serializable]
    public class MissingPersonGroup
    {
      /// <summary>
      /// A value of MissingPerson1.
      /// </summary>
      [JsonPropertyName("missingPerson1")]
      public DateWorkArea MissingPerson1
      {
        get => missingPerson1 ??= new();
        set => missingPerson1 = value;
      }

      /// <summary>
      /// A value of MissngPerson.
      /// </summary>
      [JsonPropertyName("missngPerson")]
      public CsePerson MissngPerson
      {
        get => missngPerson ??= new();
        set => missngPerson = value;
      }

      /// <summary>
      /// A value of MissingRecType.
      /// </summary>
      [JsonPropertyName("missingRecType")]
      public KdmvFile MissingRecType
      {
        get => missingRecType ??= new();
        set => missingRecType = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private DateWorkArea missingPerson1;
      private CsePerson missngPerson;
      private KdmvFile missingRecType;
    }

    /// <summary>A ProcessDatesGroup group.</summary>
    [Serializable]
    public class ProcessDatesGroup
    {
      /// <summary>
      /// A value of RecGroupType.
      /// </summary>
      [JsonPropertyName("recGroupType")]
      public KdmvFile RecGroupType
      {
        get => recGroupType ??= new();
        set => recGroupType = value;
      }

      /// <summary>
      /// A value of NumMissingReinstAp.
      /// </summary>
      [JsonPropertyName("numMissingReinstAp")]
      public Common NumMissingReinstAp
      {
        get => numMissingReinstAp ??= new();
        set => numMissingReinstAp = value;
      }

      /// <summary>
      /// A value of NumMissngRestrictAp.
      /// </summary>
      [JsonPropertyName("numMissngRestrictAp")]
      public Common NumMissngRestrictAp
      {
        get => numMissngRestrictAp ??= new();
        set => numMissngRestrictAp = value;
      }

      /// <summary>
      /// A value of ProcessDate.
      /// </summary>
      [JsonPropertyName("processDate")]
      public DateWorkArea ProcessDate
      {
        get => processDate ??= new();
        set => processDate = value;
      }

      /// <summary>
      /// A value of RecType.
      /// </summary>
      [JsonPropertyName("recType")]
      public KdmvFile RecType
      {
        get => recType ??= new();
        set => recType = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 25;

      private KdmvFile recGroupType;
      private Common numMissingReinstAp;
      private Common numMissngRestrictAp;
      private DateWorkArea processDate;
      private KdmvFile recType;
    }

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

    /// <summary>
    /// A value of LoopCount.
    /// </summary>
    [JsonPropertyName("loopCount")]
    public Common LoopCount
    {
      get => loopCount ??= new();
      set => loopCount = value;
    }

    /// <summary>
    /// A value of ReportData.
    /// </summary>
    [JsonPropertyName("reportData")]
    public ReportData ReportData
    {
      get => reportData ??= new();
      set => reportData = value;
    }

    /// <summary>
    /// A value of TotalEmailCharacterCnt.
    /// </summary>
    [JsonPropertyName("totalEmailCharacterCnt")]
    public Common TotalEmailCharacterCnt
    {
      get => totalEmailCharacterCnt ??= new();
      set => totalEmailCharacterCnt = value;
    }

    /// <summary>
    /// A value of EmailMessage.
    /// </summary>
    [JsonPropertyName("emailMessage")]
    public WorkArea EmailMessage
    {
      get => emailMessage ??= new();
      set => emailMessage = value;
    }

    /// <summary>
    /// A value of FromEmailAddress.
    /// </summary>
    [JsonPropertyName("fromEmailAddress")]
    public WorkArea FromEmailAddress
    {
      get => fromEmailAddress ??= new();
      set => fromEmailAddress = value;
    }

    /// <summary>
    /// A value of Test.
    /// </summary>
    [JsonPropertyName("test")]
    public KdmvFile Test
    {
      get => test ??= new();
      set => test = value;
    }

    /// <summary>
    /// A value of NumMissingReinstateAps.
    /// </summary>
    [JsonPropertyName("numMissingReinstateAps")]
    public Common NumMissingReinstateAps
    {
      get => numMissingReinstateAps ??= new();
      set => numMissingReinstateAps = value;
    }

    /// <summary>
    /// A value of NumReinstateDeniedRecsCommon.
    /// </summary>
    [JsonPropertyName("numReinstateDeniedRecsCommon")]
    public Common NumReinstateDeniedRecsCommon
    {
      get => numReinstateDeniedRecsCommon ??= new();
      set => numReinstateDeniedRecsCommon = value;
    }

    /// <summary>
    /// A value of NumLicenseReinstate.
    /// </summary>
    [JsonPropertyName("numLicenseReinstate")]
    public Common NumLicenseReinstate
    {
      get => numLicenseReinstate ??= new();
      set => numLicenseReinstate = value;
    }

    /// <summary>
    /// A value of NumLicenseReinstated.
    /// </summary>
    [JsonPropertyName("numLicenseReinstated")]
    public WorkArea NumLicenseReinstated
    {
      get => numLicenseReinstated ??= new();
      set => numLicenseReinstated = value;
    }

    /// <summary>
    /// A value of NumReinstateDeniedRecsWorkArea.
    /// </summary>
    [JsonPropertyName("numReinstateDeniedRecsWorkArea")]
    public WorkArea NumReinstateDeniedRecsWorkArea
    {
      get => numReinstateDeniedRecsWorkArea ??= new();
      set => numReinstateDeniedRecsWorkArea = value;
    }

    /// <summary>
    /// A value of RecordType4Found.
    /// </summary>
    [JsonPropertyName("recordType4Found")]
    public Common RecordType4Found
    {
      get => recordType4Found ??= new();
      set => recordType4Found = value;
    }

    /// <summary>
    /// A value of ToEmailAddress.
    /// </summary>
    [JsonPropertyName("toEmailAddress")]
    public WorkArea ToEmailAddress
    {
      get => toEmailAddress ??= new();
      set => toEmailAddress = value;
    }

    /// <summary>
    /// A value of RecordsSuccesfullyProces.
    /// </summary>
    [JsonPropertyName("recordsSuccesfullyProces")]
    public Common RecordsSuccesfullyProces
    {
      get => recordsSuccesfullyProces ??= new();
      set => recordsSuccesfullyProces = value;
    }

    /// <summary>
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public Common Restart
    {
      get => restart ??= new();
      set => restart = value;
    }

    /// <summary>
    /// A value of NumberOfMissingAps.
    /// </summary>
    [JsonPropertyName("numberOfMissingAps")]
    public Common NumberOfMissingAps
    {
      get => numberOfMissingAps ??= new();
      set => numberOfMissingAps = value;
    }

    /// <summary>
    /// A value of NullDate.
    /// </summary>
    [JsonPropertyName("nullDate")]
    public DateWorkArea NullDate
    {
      get => nullDate ??= new();
      set => nullDate = value;
    }

    /// <summary>
    /// Gets a value of MissingPerson.
    /// </summary>
    [JsonIgnore]
    public Array<MissingPersonGroup> MissingPerson => missingPerson ??= new(
      MissingPersonGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of MissingPerson for json serialization.
    /// </summary>
    [JsonPropertyName("missingPerson")]
    [Computed]
    public IList<MissingPersonGroup> MissingPerson_Json
    {
      get => missingPerson;
      set => MissingPerson.Assign(value);
    }

    /// <summary>
    /// A value of DateFound.
    /// </summary>
    [JsonPropertyName("dateFound")]
    public Common DateFound
    {
      get => dateFound ??= new();
      set => dateFound = value;
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
    /// A value of ProcessDate.
    /// </summary>
    [JsonPropertyName("processDate")]
    public DateWorkArea ProcessDate
    {
      get => processDate ??= new();
      set => processDate = value;
    }

    /// <summary>
    /// A value of TestAmount.
    /// </summary>
    [JsonPropertyName("testAmount")]
    public Common TestAmount
    {
      get => testAmount ??= new();
      set => testAmount = value;
    }

    /// <summary>
    /// Gets a value of ProcessDates.
    /// </summary>
    [JsonIgnore]
    public Array<ProcessDatesGroup> ProcessDates => processDates ??= new(
      ProcessDatesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ProcessDates for json serialization.
    /// </summary>
    [JsonPropertyName("processDates")]
    [Computed]
    public IList<ProcessDatesGroup> ProcessDates_Json
    {
      get => processDates;
      set => ProcessDates.Assign(value);
    }

    /// <summary>
    /// A value of NumberOfCtOrders.
    /// </summary>
    [JsonPropertyName("numberOfCtOrders")]
    public Common NumberOfCtOrders
    {
      get => numberOfCtOrders ??= new();
      set => numberOfCtOrders = value;
    }

    /// <summary>
    /// A value of LowestCtOrdAmt.
    /// </summary>
    [JsonPropertyName("lowestCtOrdAmt")]
    public Common LowestCtOrdAmt
    {
      get => lowestCtOrdAmt ??= new();
      set => lowestCtOrdAmt = value;
    }

    /// <summary>
    /// A value of HighestCtOrdAmt.
    /// </summary>
    [JsonPropertyName("highestCtOrdAmt")]
    public Common HighestCtOrdAmt
    {
      get => highestCtOrdAmt ??= new();
      set => highestCtOrdAmt = value;
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
    /// A value of AdministrativeAction.
    /// </summary>
    [JsonPropertyName("administrativeAction")]
    public AdministrativeAction AdministrativeAction
    {
      get => administrativeAction ??= new();
      set => administrativeAction = value;
    }

    /// <summary>
    /// A value of NumRestrictDeniedRecsCommon.
    /// </summary>
    [JsonPropertyName("numRestrictDeniedRecsCommon")]
    public Common NumRestrictDeniedRecsCommon
    {
      get => numRestrictDeniedRecsCommon ??= new();
      set => numRestrictDeniedRecsCommon = value;
    }

    /// <summary>
    /// A value of NumLicenseRestrictedCommon.
    /// </summary>
    [JsonPropertyName("numLicenseRestrictedCommon")]
    public Common NumLicenseRestrictedCommon
    {
      get => numLicenseRestrictedCommon ??= new();
      set => numLicenseRestrictedCommon = value;
    }

    /// <summary>
    /// A value of TotalNumberProcessed.
    /// </summary>
    [JsonPropertyName("totalNumberProcessed")]
    public Common TotalNumberProcessed
    {
      get => totalNumberProcessed ??= new();
      set => totalNumberProcessed = value;
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
    /// A value of OwedAmount.
    /// </summary>
    [JsonPropertyName("owedAmount")]
    public Common OwedAmount
    {
      get => owedAmount ??= new();
      set => owedAmount = value;
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
    /// A value of PersonTotal.
    /// </summary>
    [JsonPropertyName("personTotal")]
    public Common PersonTotal
    {
      get => personTotal ??= new();
      set => personTotal = value;
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
    /// A value of StartBatchTimestampWorkArea.
    /// </summary>
    [JsonPropertyName("startBatchTimestampWorkArea")]
    public BatchTimestampWorkArea StartBatchTimestampWorkArea
    {
      get => startBatchTimestampWorkArea ??= new();
      set => startBatchTimestampWorkArea = value;
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
    /// A value of RecordCount.
    /// </summary>
    [JsonPropertyName("recordCount")]
    public Common RecordCount
    {
      get => recordCount ??= new();
      set => recordCount = value;
    }

    /// <summary>
    /// A value of NumErrorRecordsWorkArea.
    /// </summary>
    [JsonPropertyName("numErrorRecordsWorkArea")]
    public WorkArea NumErrorRecordsWorkArea
    {
      get => numErrorRecordsWorkArea ??= new();
      set => numErrorRecordsWorkArea = value;
    }

    /// <summary>
    /// A value of TotalNumberRecords.
    /// </summary>
    [JsonPropertyName("totalNumberRecords")]
    public WorkArea TotalNumberRecords
    {
      get => totalNumberRecords ??= new();
      set => totalNumberRecords = value;
    }

    /// <summary>
    /// A value of NumLicenseRestrictedWorkArea.
    /// </summary>
    [JsonPropertyName("numLicenseRestrictedWorkArea")]
    public WorkArea NumLicenseRestrictedWorkArea
    {
      get => numLicenseRestrictedWorkArea ??= new();
      set => numLicenseRestrictedWorkArea = value;
    }

    /// <summary>
    /// A value of NumRestrictDeniedRecsWorkArea.
    /// </summary>
    [JsonPropertyName("numRestrictDeniedRecsWorkArea")]
    public WorkArea NumRestrictDeniedRecsWorkArea
    {
      get => numRestrictDeniedRecsWorkArea ??= new();
      set => numRestrictDeniedRecsWorkArea = value;
    }

    /// <summary>
    /// A value of NumErrorRecordsCommon.
    /// </summary>
    [JsonPropertyName("numErrorRecordsCommon")]
    public Common NumErrorRecordsCommon
    {
      get => numErrorRecordsCommon ??= new();
      set => numErrorRecordsCommon = value;
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
    /// A value of NumMissingRestrictedAps.
    /// </summary>
    [JsonPropertyName("numMissingRestrictedAps")]
    public Common NumMissingRestrictedAps
    {
      get => numMissingRestrictedAps ??= new();
      set => numMissingRestrictedAps = value;
    }

    /// <summary>
    /// A value of RestartCount.
    /// </summary>
    [JsonPropertyName("restartCount")]
    public Common RestartCount
    {
      get => restartCount ??= new();
      set => restartCount = value;
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
    /// A value of Name.
    /// </summary>
    [JsonPropertyName("name")]
    public WorkArea Name
    {
      get => name ??= new();
      set => name = value;
    }

    /// <summary>
    /// A value of Missmatch.
    /// </summary>
    [JsonPropertyName("missmatch")]
    public KdmvFile Missmatch
    {
      get => missmatch ??= new();
      set => missmatch = value;
    }

    /// <summary>
    /// A value of SsnDob.
    /// </summary>
    [JsonPropertyName("ssnDob")]
    public WorkArea SsnDob
    {
      get => ssnDob ??= new();
      set => ssnDob = value;
    }

    /// <summary>
    /// A value of PersonNumbDrLicense.
    /// </summary>
    [JsonPropertyName("personNumbDrLicense")]
    public WorkArea PersonNumbDrLicense
    {
      get => personNumbDrLicense ??= new();
      set => personNumbDrLicense = value;
    }

    /// <summary>
    /// A value of Problem.
    /// </summary>
    [JsonPropertyName("problem")]
    public WorkArea Problem
    {
      get => problem ??= new();
      set => problem = value;
    }

    /// <summary>
    /// A value of MismatchReportSend.
    /// </summary>
    [JsonPropertyName("mismatchReportSend")]
    public EabReportSend MismatchReportSend
    {
      get => mismatchReportSend ??= new();
      set => mismatchReportSend = value;
    }

    /// <summary>
    /// A value of NonMatchRecsFromKdmv.
    /// </summary>
    [JsonPropertyName("nonMatchRecsFromKdmv")]
    public Common NonMatchRecsFromKdmv
    {
      get => nonMatchRecsFromKdmv ??= new();
      set => nonMatchRecsFromKdmv = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public Common Current
    {
      get => current ??= new();
      set => current = value;
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
    /// A value of Postion.
    /// </summary>
    [JsonPropertyName("postion")]
    public TextWorkArea Postion
    {
      get => postion ??= new();
      set => postion = value;
    }

    /// <summary>
    /// A value of Clear.
    /// </summary>
    [JsonPropertyName("clear")]
    public Infrastructure Clear
    {
      get => clear ??= new();
      set => clear = value;
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
    /// A value of IncludeArrearsOnly.
    /// </summary>
    [JsonPropertyName("includeArrearsOnly")]
    public Common IncludeArrearsOnly
    {
      get => includeArrearsOnly ??= new();
      set => includeArrearsOnly = value;
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
    /// A value of NumberOfDays.
    /// </summary>
    [JsonPropertyName("numberOfDays")]
    public Common NumberOfDays
    {
      get => numberOfDays ??= new();
      set => numberOfDays = value;
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

    private Common loopCount;
    private ReportData reportData;
    private Common totalEmailCharacterCnt;
    private WorkArea emailMessage;
    private WorkArea fromEmailAddress;
    private KdmvFile test;
    private Common numMissingReinstateAps;
    private Common numReinstateDeniedRecsCommon;
    private Common numLicenseReinstate;
    private WorkArea numLicenseReinstated;
    private WorkArea numReinstateDeniedRecsWorkArea;
    private Common recordType4Found;
    private WorkArea toEmailAddress;
    private Common recordsSuccesfullyProces;
    private Common restart;
    private Common numberOfMissingAps;
    private DateWorkArea nullDate;
    private Array<MissingPersonGroup> missingPerson;
    private Common dateFound;
    private TextWorkArea date;
    private DateWorkArea processDate;
    private Common testAmount;
    private Array<ProcessDatesGroup> processDates;
    private Common numberOfCtOrders;
    private Common lowestCtOrdAmt;
    private Common highestCtOrdAmt;
    private KdmvFile kdmvFile;
    private AdministrativeAction administrativeAction;
    private Common numRestrictDeniedRecsCommon;
    private Common numLicenseRestrictedCommon;
    private Common totalNumberProcessed;
    private DateWorkArea startDate;
    private ExitStateWorkArea exitStateWorkArea;
    private External passArea;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private CsePerson csePerson;
    private DateWorkArea zeroDate;
    private Common owedAmount;
    private LegalAction legalAction;
    private Common personTotal;
    private FinanceWorkAttributes financeWorkAttributes;
    private Infrastructure infrastructure;
    private WorkArea detail;
    private BatchTimestampWorkArea startBatchTimestampWorkArea;
    private DateWorkArea dateWorkArea;
    private Common recordCount;
    private WorkArea numErrorRecordsWorkArea;
    private WorkArea totalNumberRecords;
    private WorkArea numLicenseRestrictedWorkArea;
    private WorkArea numRestrictDeniedRecsWorkArea;
    private Common numErrorRecordsCommon;
    private CsePerson error;
    private Common numMissingRestrictedAps;
    private Common restartCount;
    private Array<ReinstatedPrevGroup> reinstatedPrev;
    private WorkArea name;
    private KdmvFile missmatch;
    private WorkArea ssnDob;
    private WorkArea personNumbDrLicense;
    private WorkArea problem;
    private EabReportSend mismatchReportSend;
    private Common nonMatchRecsFromKdmv;
    private Common startCommon;
    private Common current;
    private Common currentPosition;
    private TextWorkArea postion;
    private Infrastructure clear;
    private Common fieldNumber;
    private Common includeArrearsOnly;
    private WorkArea workArea;
    private Common minimumAmountOwed;
    private Common numberOfDays;
    private Common arrearsOnly;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Job.
    /// </summary>
    [JsonPropertyName("job")]
    public Job Job
    {
      get => job ??= new();
      set => job = value;
    }

    /// <summary>
    /// A value of ReportData.
    /// </summary>
    [JsonPropertyName("reportData")]
    public ReportData ReportData
    {
      get => reportData ??= new();
      set => reportData = value;
    }

    /// <summary>
    /// A value of JobRun.
    /// </summary>
    [JsonPropertyName("jobRun")]
    public JobRun JobRun
    {
      get => jobRun ??= new();
      set => jobRun = value;
    }

    /// <summary>
    /// A value of Reset.
    /// </summary>
    [JsonPropertyName("reset")]
    public KsDriversLicense Reset
    {
      get => reset ??= new();
      set => reset = value;
    }

    /// <summary>
    /// A value of KsDeptMotorVeh.
    /// </summary>
    [JsonPropertyName("ksDeptMotorVeh")]
    public AdministrativeActCertification KsDeptMotorVeh
    {
      get => ksDeptMotorVeh ??= new();
      set => ksDeptMotorVeh = value;
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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
    /// A value of ProcessedCheck.
    /// </summary>
    [JsonPropertyName("processedCheck")]
    public KsDriversLicense ProcessedCheck
    {
      get => processedCheck ??= new();
      set => processedCheck = value;
    }

    private Job job;
    private ReportData reportData;
    private JobRun jobRun;
    private KsDriversLicense reset;
    private AdministrativeActCertification ksDeptMotorVeh;
    private KsDriversLicense ksDriversLicense;
    private CsePerson csePerson;
    private LegalAction legalAction;
    private CaseRole caseRole;
    private Case1 case1;
    private CaseUnit caseUnit;
    private LegalActionCaseRole legalActionCaseRole;
    private CsePersonAccount obligor;
    private Infrastructure infrastructure;
    private AdministrativeAction administrativeAction;
    private ObligationAdmActionExemption obligationAdmActionExemption;
    private KsDriversLicense processedCheck;
  }
#endregion
}
