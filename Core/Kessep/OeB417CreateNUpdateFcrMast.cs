// Program: OE_B417_CREATE_N_UPDATE_FCR_MAST, ID: 374565616, model: 746.
// Short name: SWE00063
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_B417_CREATE_N_UPDATE_FCR_MAST.
/// </para>
/// <para>
/// This Action block will be updating the FCR Master and Member entities.   
/// This action block can be called by B417 &amp; B418 Procedures.
/// </para>
/// </summary>
[Serializable]
public partial class OeB417CreateNUpdateFcrMast: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B417_CREATE_N_UPDATE_FCR_MAST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB417CreateNUpdateFcrMast(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB417CreateNUpdateFcrMast.
  /// </summary>
  public OeB417CreateNUpdateFcrMast(IContext context, Import import,
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
    // **************************************************************************************
    // This Common Action Block maintain FCR Case & Member records and used by 
    // FCR Extract
    // and FCR Response Processes (SRRUN070 & SRRUN072).
    // **************************************************************************************
    // ***************************************************************************************
    // *                               
    // Maintenance Log
    // 
    // *
    // ***************************************************************************************
    // *    DATE       NAME             PR/SR #     DESCRIPTION OF THE CHANGE
    // *
    // * ----------  -----------------  ---------
    // 
    // -----------------------------------------*
    // * 07/31/2009  Raj S              CQ7190      ***** Initial Coding *****
    // *
    // *
    // 
    // *
    // * 08/26/2010  Raj S              CQ21517     Modified to fix FCR Case 
    // Sent date update*
    // *
    // 
    // problem. The vlaue will be saved and the *
    // *
    // 
    // same value used while adding the same    *
    // *
    // 
    // case to the DB.                          *
    // *
    // 
    // *
    // ***************************************************************************************
    local.FcrMasterCaseRecord.Assign(import.FcrMasterCaseRecord);
    local.FcrMasterMemberRecord.Assign(import.FcrMasterMemberRecord);
    export.CaseCreCount.Count = import.CaseCreCount.Count;
    export.CaseUpdCount.Count = import.CaseUpdCount.Count;
    export.CaseDelCount.Count = import.CaseDelCount.Count;
    export.CaseSkipCount.Count = import.CaseSkipCount.Count;
    export.PersonUpdCount.Count = import.PersonUpdCount.Count;
    export.PersonsCreCount.Count = import.PersonsCreCount.Count;
    export.PersonDelCount.Count = import.PersonDelCount.Count;
    export.PersonSkipCount.Count = import.PersonSkipCount.Count;
    export.TotalCreCount.Count = import.TotalCreCount.Count;
    export.TotalUpdCount.Count = import.TotalUpdCount.Count;
    export.TotalDelCount.Count = import.TotalDelCount.Count;
    export.TotalSkipCount.Count = import.TotalSkipCount.Count;
    export.Commit.Count = import.Commit.Count;

    // **************************************************************************************
    // Idnentify the Case Master Record by checking case master record case id 
    // is > 0.
    // **************************************************************************************
    if (!IsEmpty(local.FcrMasterCaseRecord.CaseId))
    {
      local.FcrCaseMaster.AcknowlegementCode =
        local.FcrMasterCaseRecord.AcknowlegementCode;
      local.FcrCaseMaster.ActionTypeCode =
        local.FcrMasterCaseRecord.ActionTypeCode;
      local.FcrCaseMaster.BatchNumber = local.FcrMasterCaseRecord.BatchNumber;
      local.FcrCaseMaster.CaseId = local.FcrMasterCaseRecord.CaseId;
      local.FcrCaseMaster.CreatedBy = local.FcrMasterCaseRecord.CreatedBy;
      local.FcrCaseMaster.ErrorCode1 = local.FcrMasterCaseRecord.ErrorCode1;
      local.FcrCaseMaster.ErrorCode2 = local.FcrMasterCaseRecord.ErrorCode2;
      local.FcrCaseMaster.ErrorCode3 = local.FcrMasterCaseRecord.ErrorCode3;
      local.FcrCaseMaster.ErrorCode4 = local.FcrMasterCaseRecord.ErrorCode4;
      local.FcrCaseMaster.ErrorCode5 = local.FcrMasterCaseRecord.ErrorCode5;
      local.FcrCaseMaster.FcrCaseComments =
        local.FcrMasterCaseRecord.FcrCaseComments;
      local.FcrCaseMaster.FipsCountyCode =
        local.FcrMasterCaseRecord.FipsCountyCode;
      local.FcrCaseMaster.OrderIndicator =
        local.FcrMasterCaseRecord.OrderIndicator;

      if (Equal(local.FcrMasterCaseRecord.CaseId,
        local.FcrMasterCaseRecord.PreviousCaseId))
      {
        local.FcrCaseMaster.PreviousCaseId = "";
      }
      else
      {
        local.FcrCaseMaster.PreviousCaseId =
          local.FcrMasterCaseRecord.PreviousCaseId;
      }

      local.FcrCaseMaster.RecordIdentifier =
        local.FcrMasterCaseRecord.RecordIdentifier;
      local.FcrCaseMaster.CaseSentDateToFcr =
        local.FcrMasterCaseRecord.CaseSentDateToFcr;
      local.FcrCaseMaster.FcrCaseResponseDate =
        local.FcrMasterCaseRecord.FcrCaseResponseDate;
      local.FcrCaseMaster.CreatedBy = local.FcrMasterCaseRecord.CreatedBy;
      local.FcrCaseMaster.CreatedTimestamp =
        local.FcrMasterCaseRecord.CreatedTimestamp;

      if (ReadFcrCaseMaster2())
      {
        // ****************************************************************************************
        // If the Action Block is called from FCR extract process and the record
        // is already exists
        // then we don't want replace the existing records, because this may be 
        // a scenario of SSN
        // change, we want retain the existing record and once the new Add 
        // record is accepted by
        // the FCR then the values will be replaced through FCR response 
        // process.
        // ****************************************************************************************
        if ((AsChar(import.ExtractNResponseFlag.Flag) == 'E' || AsChar
          (import.ExtractNResponseFlag.Flag) == 'M') && (
            AsChar(import.FcrMasterCaseRecord.ActionTypeCode) == 'A' || AsChar
          (import.FcrMasterCaseRecord.ActionTypeCode) == 'L'))
        {
          ++export.TotalSkipCount.Count;
          ++export.CaseSkipCount.Count;

          return;
        }

        // **************************************************************************************
        // The 'D'elete Case transaction sent by CSE was accepted by FCR, so we 
        // need to delete
        // the case from CSE DB2 Master and related member records also will be 
        // deleted from DB.
        // **************************************************************************************
        if (AsChar(local.FcrCaseMaster.ActionTypeCode) == 'D')
        {
          if (Equal(local.FcrCaseMaster.AcknowlegementCode, "AAAAA"))
          {
            export.CaseDelSendDate.CaseSentDateToFcr =
              entities.ExistingFcrCaseMaster.CaseSentDateToFcr;
            DeleteFcrCaseMaster();
            ++export.TotalDelCount.Count;
            ++export.CaseDelCount.Count;
          }
          else
          {
            ++export.TotalSkipCount.Count;
            ++export.CaseSkipCount.Count;
          }

          return;
        }

        // **************************************************************************************
        // Since the Master record is already exists and modify the FCR case 
        // master Record with
        // the FCR response information.
        // **************************************************************************************
        try
        {
          UpdateFcrCaseMaster2();
          ++export.TotalUpdCount.Count;
          ++export.CaseUpdCount.Count;
          export.CaseDelSendDate.CaseSentDateToFcr =
            local.NullDate.CaseSentDateToFcr;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FCR_MASTER_NU";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FCR_MASTER_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
      else
      {
        // **************************************************************************************
        // This creation of new Case Master will occur when the case is deleted 
        // and accepted by
        // FCR due to FCR limitation in modifying the case member SSN.  Due the 
        // business rule
        // CSE would have sent a Case delete function and followed by Case add 
        // and case member
        // adds with the new SSN numbers.  **** This is typical scenario of 
        // changing SSNs for
        // AP as well as AR at the same time.
        // **************************************************************************************
        if (AsChar(local.FcrCaseMaster.ActionTypeCode) == 'A' || AsChar
          (import.ExtractNResponseFlag.Flag) == 'M' || AsChar
          (import.ExtractNResponseFlag.Flag) == 'R' && AsChar
          (local.FcrCaseMaster.ActionTypeCode) != 'A')
        {
          if (AsChar(import.ExtractNResponseFlag.Flag) == 'R')
          {
            if (AsChar(local.FcrCaseMaster.ActionTypeCode) == 'A' && !
              Equal(import.CaseDelSendDate.CaseSentDateToFcr,
              local.NullDate.CaseSentDateToFcr))
            {
              local.FcrCaseMaster.CaseSentDateToFcr =
                import.CaseDelSendDate.CaseSentDateToFcr;

              goto Test1;
            }

            local.FcrCaseMaster.CaseSentDateToFcr =
              AddDays(local.FcrCaseMaster.FcrCaseResponseDate, -4);
          }

Test1:

          try
          {
            CreateFcrCaseMaster();
            ++export.TotalCreCount.Count;
            ++export.CaseCreCount.Count;
            export.CaseDelSendDate.CaseSentDateToFcr =
              local.NullDate.CaseSentDateToFcr;
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FCR_MASTER_NU";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FCR_MASTER_PV";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
        else
        {
          ++export.TotalSkipCount.Count;
          ++export.CaseSkipCount.Count;
        }
      }
    }

    if (!IsEmpty(local.FcrMasterMemberRecord.CaseId))
    {
      local.FcrCaseMembers.AcknowledgementCode =
        local.FcrMasterMemberRecord.AcknowledgementCode;
      local.FcrCaseMembers.ActionTypeCode =
        local.FcrMasterMemberRecord.ActionTypeCode;
      local.FcrCaseMembers.AdditionalFirstName1 =
        local.FcrMasterMemberRecord.AdditionalFirstName1;
      local.FcrCaseMembers.AdditionalFirstName2 =
        local.FcrMasterMemberRecord.AdditionalFirstName2;
      local.FcrCaseMembers.AdditionalFirstName3 =
        local.FcrMasterMemberRecord.AdditionalFirstName3;
      local.FcrCaseMembers.AdditionalFirstName4 =
        local.FcrMasterMemberRecord.AdditionalFirstName4;
      local.FcrCaseMembers.AdditionalLastName1 =
        local.FcrMasterMemberRecord.AdditionalLastName1;
      local.FcrCaseMembers.AdditionalLastName2 =
        local.FcrMasterMemberRecord.AdditionalLastName2;
      local.FcrCaseMembers.AdditionalLastName3 =
        local.FcrMasterMemberRecord.AdditionalLastName3;
      local.FcrCaseMembers.AdditionalLastName4 =
        local.FcrMasterMemberRecord.AdditionalLastName4;
      local.FcrCaseMembers.AdditionalMiddleName1 =
        local.FcrMasterMemberRecord.AdditionalMiddleName1;
      local.FcrCaseMembers.AdditionalMiddleName2 =
        local.FcrMasterMemberRecord.AdditionalMiddleName2;
      local.FcrCaseMembers.AdditionalMiddleName3 =
        local.FcrMasterMemberRecord.AdditionalMiddleName3;
      local.FcrCaseMembers.AdditionalMiddleName4 =
        local.FcrMasterMemberRecord.AdditionalMiddleName4;
      local.FcrCaseMembers.AdditionalSsn1ValidityCode =
        local.FcrMasterMemberRecord.AdditionalSsn1ValidityCode;
      local.FcrCaseMembers.AdditionalSsn2ValidityCode =
        local.FcrMasterMemberRecord.AdditionalSsn2ValidityCode;
      local.FcrCaseMembers.AdditionalSsn1 =
        local.FcrMasterMemberRecord.AdditionalSsn1;
      local.FcrCaseMembers.AdditionalSsn2 =
        local.FcrMasterMemberRecord.AdditionalSsn2;
      local.FcrCaseMembers.BatchNumber =
        local.FcrMasterMemberRecord.BatchNumber;
      local.FcrCaseMembers.BundleFplsLocateResults =
        local.FcrMasterMemberRecord.BundleFplsLocateResults;
      local.FcrCaseMembers.CityOfBirth =
        local.FcrMasterMemberRecord.CityOfBirth;
      local.FcrCaseMembers.ErrorCode1 = local.FcrMasterMemberRecord.ErrorCode1;
      local.FcrCaseMembers.ErrorCode2 = local.FcrMasterMemberRecord.ErrorCode2;
      local.FcrCaseMembers.ErrorCode3 = local.FcrMasterMemberRecord.ErrorCode3;
      local.FcrCaseMembers.ErrorCode4 = local.FcrMasterMemberRecord.ErrorCode4;
      local.FcrCaseMembers.ErrorCode5 = local.FcrMasterMemberRecord.ErrorCode5;
      local.FcrCaseMembers.FamilyViolence =
        local.FcrMasterMemberRecord.FamilyViolence;
      local.FcrCaseMembers.FathersFirstName =
        local.FcrMasterMemberRecord.FathersFirstName;
      local.FcrCaseMembers.FathersLastName =
        local.FcrMasterMemberRecord.FathersLastName;
      local.FcrCaseMembers.FathersMiddleInitial =
        local.FcrMasterMemberRecord.FathersMiddleInitial;
      local.FcrCaseMembers.FcrPrimaryFirstName =
        local.FcrMasterMemberRecord.FcrPrimaryFirstName;
      local.FcrCaseMembers.FcrPrimaryLastName =
        local.FcrMasterMemberRecord.FcrPrimaryLastName;
      local.FcrCaseMembers.FcrPrimaryMiddleName =
        local.FcrMasterMemberRecord.FcrPrimaryMiddleName;
      local.FcrCaseMembers.FcrPrimarySsn =
        local.FcrMasterMemberRecord.FcrPrimarySsn;
      local.FcrCaseMembers.FipsCountyCode =
        local.FcrMasterMemberRecord.FipsCountyCode;
      local.FcrCaseMembers.FirstName = local.FcrMasterMemberRecord.FirstName;
      local.FcrCaseMembers.Irs1099 = local.FcrMasterMemberRecord.Irs1099;
      local.FcrCaseMembers.IrsUSsn = local.FcrMasterMemberRecord.IrsUSsn;
      local.FcrCaseMembers.LastName = local.FcrMasterMemberRecord.LastName;
      local.FcrCaseMembers.LocateRequestType =
        local.FcrMasterMemberRecord.LocateRequestType;
      local.FcrCaseMembers.LocateSource1 =
        local.FcrMasterMemberRecord.LocateSource1;
      local.FcrCaseMembers.LocateSource2 =
        local.FcrMasterMemberRecord.LocateSource2;
      local.FcrCaseMembers.LocateSource3 =
        local.FcrMasterMemberRecord.LocateSource3;
      local.FcrCaseMembers.LocateSource4 =
        local.FcrMasterMemberRecord.LocateSource4;
      local.FcrCaseMembers.LocateSource5 =
        local.FcrMasterMemberRecord.LocateSource5;
      local.FcrCaseMembers.LocateSource6 =
        local.FcrMasterMemberRecord.LocateSource6;
      local.FcrCaseMembers.LocateSource7 =
        local.FcrMasterMemberRecord.LocateSource7;
      local.FcrCaseMembers.LocateSource8 =
        local.FcrMasterMemberRecord.LocateSource8;
      local.FcrCaseMembers.MemberId = local.FcrMasterMemberRecord.MemberId;
      local.FcrCaseMembers.MiddleName = local.FcrMasterMemberRecord.MiddleName;
      local.FcrCaseMembers.MothersFirstName =
        local.FcrMasterMemberRecord.MothersFirstName;
      local.FcrCaseMembers.MothersMaidenNm =
        local.FcrMasterMemberRecord.MothersMaidenNm;
      local.FcrCaseMembers.MothersMiddleInitial =
        local.FcrMasterMemberRecord.MothersMiddleInitial;
      local.FcrCaseMembers.MultipleSsn1 =
        local.FcrMasterMemberRecord.MultipleSsn1;
      local.FcrCaseMembers.MultipleSsn2 =
        local.FcrMasterMemberRecord.MultipleSsn2;
      local.FcrCaseMembers.MultipleSsn3 =
        local.FcrMasterMemberRecord.MultipleSsn2;
      local.FcrCaseMembers.NewMemberId =
        local.FcrMasterMemberRecord.NewMemberId;

      if (Equal(local.FcrMasterMemberRecord.ParticipantType, "NP"))
      {
        local.FcrCaseMembers.ParticipantType = "AP";
      }
      else if (Equal(local.FcrMasterMemberRecord.ParticipantType, "CP"))
      {
        local.FcrCaseMembers.ParticipantType = "AR";
      }
      else
      {
        local.FcrCaseMembers.ParticipantType =
          local.FcrMasterMemberRecord.ParticipantType;
      }

      local.FcrCaseMembers.PreviousSsn =
        local.FcrMasterMemberRecord.PreviousSsn;
      local.FcrCaseMembers.ProvidedOrCorrectedSsn =
        local.FcrMasterMemberRecord.ProvidedOrCorrectedSsn;
      local.FcrCaseMembers.RecordIdentifier =
        local.FcrMasterMemberRecord.RecordIdentifier;
      local.FcrCaseMembers.SexCode = local.FcrMasterMemberRecord.SexCode;
      local.FcrCaseMembers.SsaCityOfLastResidence =
        local.FcrMasterMemberRecord.SsaCityOfLastResidence;
      local.FcrCaseMembers.SsaCityOfLumpSumPayment =
        local.FcrMasterMemberRecord.SsaCityOfLumpSumPayment;
      local.FcrCaseMembers.SsaDateOfBirthIndicator =
        local.FcrMasterMemberRecord.SsaDateOfBirthIndicator;
      local.FcrCaseMembers.SsaStateOfLastResidence =
        local.FcrMasterMemberRecord.SsaStateOfLastResidence;
      local.FcrCaseMembers.SsaStateOfLumpSumPayment =
        local.FcrMasterMemberRecord.SsaStateOfLumpSumPayment;
      local.FcrCaseMembers.SsaZipCodeOfLastResidence =
        local.FcrMasterMemberRecord.SsaZipCodeOfLastResidence;
      local.FcrCaseMembers.SsaZipCodeOfLumpSumPayment =
        local.FcrMasterMemberRecord.SsaZipCodeOfLumpSumPayment;
      local.FcrCaseMembers.Ssn = local.FcrMasterMemberRecord.Ssn;
      local.FcrCaseMembers.SsnValidityCode =
        local.FcrMasterMemberRecord.SsnValidityCode;
      local.FcrCaseMembers.StateOrCountryOfBirth =
        local.FcrMasterMemberRecord.StateOrCountryOfBirth;
      local.FcrCaseMembers.DateOfBirth =
        local.FcrMasterMemberRecord.DateOfBirth;
      local.FcrCaseMembers.DateOfDeath =
        local.FcrMasterMemberRecord.DateOfDeath;

      if (ReadFcrCaseMaster1())
      {
        if (Equal(entities.ExistingFcrCaseMaster.BatchNumber, "999999") && !
          IsEmpty(local.FcrCaseMembers.BatchNumber))
        {
          try
          {
            UpdateFcrCaseMaster1();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FCR_MASTER_NU";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FCR_MASTER_PV";

                return;
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
        ExitState = "CASE_NF";

        return;
      }

      if (ReadFcrCaseMembers())
      {
        // ****************************************************************************************
        // If the Action Block is called from FCR extract process and the record
        // is already exists
        // then we don't want replace the existing records, because this may 
        // scenario of SSN change
        // we want retain the existing record values and once the new Add record
        // is accepted by the
        // FCR then will we will replaced through FCR response process.
        // ****************************************************************************************
        if ((AsChar(import.ExtractNResponseFlag.Flag) == 'E' || AsChar
          (import.ExtractNResponseFlag.Flag) == 'M') && (
            AsChar(local.FcrCaseMembers.ActionTypeCode) == 'A' || AsChar
          (local.FcrCaseMembers.ActionTypeCode) == 'L'))
        {
          ++export.TotalSkipCount.Count;
          ++export.PersonSkipCount.Count;

          return;
        }

        // **************************************************************************************
        // The 'D'elete FCR Case member transaction sent by CSE was accepted by 
        // FCR, so we need to
        // delete the case member from CSE DB2 Master and subsequent add 
        // transaction will be added
        // to the case.
        // **************************************************************************************
        if (AsChar(local.FcrCaseMembers.ActionTypeCode) == 'D')
        {
          if (Equal(local.FcrCaseMembers.AcknowledgementCode, "AAAAA"))
          {
            DeleteFcrCaseMembers();
            ++export.TotalDelCount.Count;
            ++export.PersonDelCount.Count;
          }
          else
          {
            ++export.TotalSkipCount.Count;
            ++export.PersonSkipCount.Count;
          }

          return;
        }

        if (Equal(local.FcrCaseMembers.AcknowledgementCode, "REJCT"))
        {
          if (Equal(entities.ExistingFcrCaseMembers.AcknowledgementCode, "REJCT"))
            
          {
            // **************************************************************************************
            // The previous FCR Member transaction sent to FCR has been rejected
            // and the latest one
            // sent to FCR also got rejected, so store the latest record 
            // rejected by FCR.
            // **************************************************************************************
            goto Test2;
          }
          else
          {
            // **************************************************************************************
            // The current FCR member record rejected by FCR but the previous 
            // transaction has differ-
            // ent acknowledgement Status ('AAAAA'(Accepted)/'HOLDS'(Pending), 
            // we need to keep the
            // old FCR member record because that is what available in FCR 
            // Database.
            // **************************************************************************************
            ++export.TotalSkipCount.Count;
            ++export.PersonSkipCount.Count;
          }

          return;
        }

Test2:

        try
        {
          UpdateFcrCaseMembers();
          ++export.TotalUpdCount.Count;
          ++export.PersonUpdCount.Count;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FCR_MEMBER_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FCR_MEMBER_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
      else if (AsChar(local.FcrCaseMembers.ActionTypeCode) == 'A' || AsChar
        (import.ExtractNResponseFlag.Flag) == 'M' || AsChar
        (import.ExtractNResponseFlag.Flag) == 'R' && (
          AsChar(local.FcrCaseMembers.ActionTypeCode) == 'C' || AsChar
        (local.FcrCaseMembers.ActionTypeCode) == 'D'))
      {
        try
        {
          CreateFcrCaseMembers();
          ++export.TotalCreCount.Count;
          ++export.PersonsCreCount.Count;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FCR_MEMBER_AE";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FCR_MEMBER_PV";

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
        ++export.TotalSkipCount.Count;
        ++export.PersonSkipCount.Count;
      }
    }
  }

  private void CreateFcrCaseMaster()
  {
    var caseId = local.FcrCaseMaster.CaseId;
    var orderIndicator = local.FcrCaseMaster.OrderIndicator ?? "";
    var actionTypeCode = local.FcrCaseMaster.ActionTypeCode ?? "";
    var batchNumber = local.FcrCaseMaster.BatchNumber ?? "";
    var fipsCountyCode = local.FcrCaseMaster.FipsCountyCode ?? "";
    var previousCaseId = local.FcrCaseMaster.PreviousCaseId ?? "";
    var caseSentDateToFcr = local.FcrCaseMaster.CaseSentDateToFcr;
    var fcrCaseResponseDate = local.FcrCaseMaster.FcrCaseResponseDate;
    var acknowlegementCode = local.FcrCaseMaster.AcknowlegementCode ?? "";
    var errorCode1 = local.FcrCaseMaster.ErrorCode1 ?? "";
    var errorCode2 = local.FcrCaseMaster.ErrorCode2 ?? "";
    var errorCode3 = local.FcrCaseMaster.ErrorCode3 ?? "";
    var errorCode4 = local.FcrCaseMaster.ErrorCode4 ?? "";
    var errorCode5 = local.FcrCaseMaster.ErrorCode5 ?? "";
    var createdBy = local.FcrCaseMaster.CreatedBy;
    var createdTimestamp = local.FcrCaseMaster.CreatedTimestamp;
    var recordIdentifier = local.FcrCaseMaster.RecordIdentifier ?? "";
    var fcrCaseComments = local.FcrCaseMaster.FcrCaseComments ?? "";

    entities.ExistingFcrCaseMaster.Populated = false;
    Update("CreateFcrCaseMaster",
      (db, command) =>
      {
        db.SetString(command, "caseId", caseId);
        db.SetNullableString(command, "orderIndicator", orderIndicator);
        db.SetNullableString(command, "actionTypeCd", actionTypeCode);
        db.SetNullableString(command, "batchNumber", batchNumber);
        db.SetNullableString(command, "fipsCountyCd", fipsCountyCode);
        db.SetNullableString(command, "previousCaseId", previousCaseId);
        db.SetNullableDate(command, "applSentDt", caseSentDateToFcr);
        db.SetNullableDate(command, "fcrResponseDt", fcrCaseResponseDate);
        db.SetNullableString(command, "ackmntCd", acknowlegementCode);
        db.SetNullableString(command, "errorCode1", errorCode1);
        db.SetNullableString(command, "errorCode2", errorCode2);
        db.SetNullableString(command, "errorCode3", errorCode3);
        db.SetNullableString(command, "errorCode4", errorCode4);
        db.SetNullableString(command, "errorCode5", errorCode5);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "recordId", recordIdentifier);
        db.SetNullableString(command, "fcrCaseComments", fcrCaseComments);
      });

    entities.ExistingFcrCaseMaster.CaseId = caseId;
    entities.ExistingFcrCaseMaster.OrderIndicator = orderIndicator;
    entities.ExistingFcrCaseMaster.ActionTypeCode = actionTypeCode;
    entities.ExistingFcrCaseMaster.BatchNumber = batchNumber;
    entities.ExistingFcrCaseMaster.FipsCountyCode = fipsCountyCode;
    entities.ExistingFcrCaseMaster.PreviousCaseId = previousCaseId;
    entities.ExistingFcrCaseMaster.CaseSentDateToFcr = caseSentDateToFcr;
    entities.ExistingFcrCaseMaster.FcrCaseResponseDate = fcrCaseResponseDate;
    entities.ExistingFcrCaseMaster.AcknowlegementCode = acknowlegementCode;
    entities.ExistingFcrCaseMaster.ErrorCode1 = errorCode1;
    entities.ExistingFcrCaseMaster.ErrorCode2 = errorCode2;
    entities.ExistingFcrCaseMaster.ErrorCode3 = errorCode3;
    entities.ExistingFcrCaseMaster.ErrorCode4 = errorCode4;
    entities.ExistingFcrCaseMaster.ErrorCode5 = errorCode5;
    entities.ExistingFcrCaseMaster.CreatedBy = createdBy;
    entities.ExistingFcrCaseMaster.CreatedTimestamp = createdTimestamp;
    entities.ExistingFcrCaseMaster.RecordIdentifier = recordIdentifier;
    entities.ExistingFcrCaseMaster.FcrCaseComments = fcrCaseComments;
    entities.ExistingFcrCaseMaster.Populated = true;
  }

  private void CreateFcrCaseMembers()
  {
    var fcmCaseId = entities.ExistingFcrCaseMaster.CaseId;
    var memberId = local.FcrCaseMembers.MemberId;
    var actionTypeCode = local.FcrCaseMembers.ActionTypeCode ?? "";
    var locateRequestType = local.FcrCaseMembers.LocateRequestType ?? "";
    var recordIdentifier = local.FcrCaseMembers.RecordIdentifier ?? "";
    var participantType = local.FcrCaseMembers.ParticipantType ?? "";
    var sexCode = local.FcrCaseMembers.SexCode ?? "";
    var dateOfBirth = local.FcrCaseMembers.DateOfBirth;
    var ssn = local.FcrCaseMembers.Ssn ?? "";
    var firstName = local.FcrCaseMembers.FirstName ?? "";
    var middleName = local.FcrCaseMembers.MiddleName ?? "";
    var lastName = local.FcrCaseMembers.LastName ?? "";
    var fipsCountyCode = local.FcrCaseMembers.FipsCountyCode ?? "";
    var familyViolence = local.FcrCaseMembers.FamilyViolence ?? "";
    var previousSsn = local.FcrCaseMembers.PreviousSsn ?? "";
    var cityOfBirth = local.FcrCaseMembers.CityOfBirth ?? "";
    var stateOrCountryOfBirth = local.FcrCaseMembers.StateOrCountryOfBirth ?? ""
      ;
    var fathersFirstName = local.FcrCaseMembers.FathersFirstName ?? "";
    var fathersMiddleInitial = local.FcrCaseMembers.FathersMiddleInitial ?? "";
    var fathersLastName = local.FcrCaseMembers.FathersLastName ?? "";
    var mothersFirstName = local.FcrCaseMembers.MothersFirstName ?? "";
    var mothersMiddleInitial = local.FcrCaseMembers.MothersMiddleInitial ?? "";
    var mothersMaidenNm = local.FcrCaseMembers.MothersMaidenNm ?? "";
    var irsUSsn = local.FcrCaseMembers.IrsUSsn ?? "";
    var additionalSsn1 = local.FcrCaseMembers.AdditionalSsn1 ?? "";
    var additionalSsn2 = local.FcrCaseMembers.AdditionalSsn2 ?? "";
    var additionalFirstName1 = local.FcrCaseMembers.AdditionalFirstName1 ?? "";
    var additionalMiddleName1 = local.FcrCaseMembers.AdditionalMiddleName1 ?? ""
      ;
    var additionalLastName1 = local.FcrCaseMembers.AdditionalLastName1 ?? "";
    var additionalFirstName2 = local.FcrCaseMembers.AdditionalFirstName2 ?? "";
    var additionalMiddleName2 = local.FcrCaseMembers.AdditionalMiddleName2 ?? ""
      ;
    var additionalLastName2 = local.FcrCaseMembers.AdditionalLastName2 ?? "";
    var additionalFirstName3 = local.FcrCaseMembers.AdditionalFirstName3 ?? "";
    var additionalMiddleName3 = local.FcrCaseMembers.AdditionalMiddleName3 ?? ""
      ;
    var additionalLastName3 = local.FcrCaseMembers.AdditionalLastName3 ?? "";
    var additionalFirstName4 = local.FcrCaseMembers.AdditionalFirstName4 ?? "";
    var additionalMiddleName4 = local.FcrCaseMembers.AdditionalMiddleName4 ?? ""
      ;
    var additionalLastName4 = local.FcrCaseMembers.AdditionalLastName4 ?? "";
    var newMemberId = local.FcrCaseMembers.NewMemberId ?? "";
    var irs1099 = local.FcrCaseMembers.Irs1099 ?? "";
    var locateSource1 = local.FcrCaseMembers.LocateSource1 ?? "";
    var locateSource2 = local.FcrCaseMembers.LocateSource2 ?? "";
    var locateSource3 = local.FcrCaseMembers.LocateSource3 ?? "";
    var locateSource4 = local.FcrCaseMembers.LocateSource4 ?? "";
    var locateSource5 = local.FcrCaseMembers.LocateSource5 ?? "";
    var locateSource6 = local.FcrCaseMembers.LocateSource6 ?? "";
    var locateSource7 = local.FcrCaseMembers.LocateSource7 ?? "";
    var locateSource8 = local.FcrCaseMembers.LocateSource8 ?? "";
    var ssnValidityCode = local.FcrCaseMembers.SsnValidityCode ?? "";
    var providedOrCorrectedSsn =
      local.FcrCaseMembers.ProvidedOrCorrectedSsn ?? "";
    var multipleSsn1 = local.FcrCaseMembers.MultipleSsn1 ?? "";
    var multipleSsn2 = local.FcrCaseMembers.MultipleSsn2 ?? "";
    var multipleSsn3 = local.FcrCaseMembers.MultipleSsn3 ?? "";
    var ssaDateOfBirthIndicator =
      local.FcrCaseMembers.SsaDateOfBirthIndicator ?? "";
    var batchNumber = local.FcrCaseMembers.BatchNumber ?? "";
    var dateOfDeath = local.FcrCaseMembers.DateOfDeath;
    var ssaZipCodeOfLastResidence =
      local.FcrCaseMembers.SsaZipCodeOfLastResidence ?? "";
    var ssaZipCodeOfLumpSumPayment =
      local.FcrCaseMembers.SsaZipCodeOfLumpSumPayment ?? "";
    var fcrPrimarySsn = local.FcrCaseMembers.FcrPrimarySsn ?? "";
    var fcrPrimaryFirstName = local.FcrCaseMembers.FcrPrimaryFirstName ?? "";
    var fcrPrimaryMiddleName = local.FcrCaseMembers.FcrPrimaryMiddleName ?? "";
    var fcrPrimaryLastName = local.FcrCaseMembers.FcrPrimaryLastName ?? "";
    var acknowledgementCode = local.FcrCaseMembers.AcknowledgementCode ?? "";
    var errorCode1 = local.FcrCaseMembers.ErrorCode1 ?? "";
    var errorCode2 = local.FcrCaseMembers.ErrorCode2 ?? "";
    var errorCode3 = local.FcrCaseMembers.ErrorCode3 ?? "";
    var errorCode4 = local.FcrCaseMembers.ErrorCode4 ?? "";
    var errorCode5 = local.FcrCaseMembers.ErrorCode5 ?? "";
    var additionalSsn1ValidityCode =
      local.FcrCaseMembers.AdditionalSsn1ValidityCode ?? "";
    var additionalSsn2ValidityCode =
      local.FcrCaseMembers.AdditionalSsn2ValidityCode ?? "";
    var bundleFplsLocateResults =
      local.FcrCaseMembers.BundleFplsLocateResults ?? "";
    var ssaCityOfLastResidence =
      local.FcrCaseMembers.SsaCityOfLastResidence ?? "";
    var ssaStateOfLastResidence =
      local.FcrCaseMembers.SsaStateOfLastResidence ?? "";
    var ssaCityOfLumpSumPayment =
      local.FcrCaseMembers.SsaCityOfLumpSumPayment ?? "";
    var ssaStateOfLumpSumPayment =
      local.FcrCaseMembers.SsaStateOfLumpSumPayment ?? "";

    entities.ExistingFcrCaseMembers.Populated = false;
    Update("CreateFcrCaseMembers",
      (db, command) =>
      {
        db.SetString(command, "fcmCaseId", fcmCaseId);
        db.SetString(command, "memberId", memberId);
        db.SetNullableString(command, "actionTypeCd", actionTypeCode);
        db.SetNullableString(command, "locateReqstType", locateRequestType);
        db.SetNullableString(command, "recordId", recordIdentifier);
        db.SetNullableString(command, "participantType", participantType);
        db.SetNullableString(command, "sexCode", sexCode);
        db.SetNullableDate(command, "dateOfBirth", dateOfBirth);
        db.SetNullableString(command, "ssn", ssn);
        db.SetNullableString(command, "firstName", firstName);
        db.SetNullableString(command, "middleName", middleName);
        db.SetNullableString(command, "lastName", lastName);
        db.SetNullableString(command, "fipsCountyCd", fipsCountyCode);
        db.SetNullableString(command, "familyViolence", familyViolence);
        db.SetNullableString(command, "previousSsn", previousSsn);
        db.SetNullableString(command, "cityOfBirth", cityOfBirth);
        db.SetNullableString(command, "stOrCtryOfBrth", stateOrCountryOfBirth);
        db.SetNullableString(command, "fathersFirstName", fathersFirstName);
        db.SetNullableString(command, "fathersMi", fathersMiddleInitial);
        db.SetNullableString(command, "fathersLastName", fathersLastName);
        db.SetNullableString(command, "mothersFirstNm", mothersFirstName);
        db.SetNullableString(command, "mothersMi", mothersMiddleInitial);
        db.SetNullableString(command, "mothersMaidenNm", mothersMaidenNm);
        db.SetNullableString(command, "irsUSsn", irsUSsn);
        db.SetNullableString(command, "additionalSsn1", additionalSsn1);
        db.SetNullableString(command, "additionalSsn2", additionalSsn2);
        db.SetNullableString(command, "addlFirstName1", additionalFirstName1);
        db.SetNullableString(command, "addlMiddleName1", additionalMiddleName1);
        db.SetNullableString(command, "addlLastName1", additionalLastName1);
        db.SetNullableString(command, "addlFirstName2", additionalFirstName2);
        db.SetNullableString(command, "addlMiddleName2", additionalMiddleName2);
        db.SetNullableString(command, "addlLastName2", additionalLastName2);
        db.SetNullableString(command, "addlFirstName3", additionalFirstName3);
        db.SetNullableString(command, "addlMiddleName3", additionalMiddleName3);
        db.SetNullableString(command, "addlLastName3", additionalLastName3);
        db.SetNullableString(command, "addlFirstName4", additionalFirstName4);
        db.SetNullableString(command, "addlMiddleName4", additionalMiddleName4);
        db.SetNullableString(command, "addlLastName4", additionalLastName4);
        db.SetNullableString(command, "newMemberId", newMemberId);
        db.SetNullableString(command, "irs1099", irs1099);
        db.SetNullableString(command, "locateSource1", locateSource1);
        db.SetNullableString(command, "locateSource2", locateSource2);
        db.SetNullableString(command, "locateSource3", locateSource3);
        db.SetNullableString(command, "locateSource4", locateSource4);
        db.SetNullableString(command, "locateSource5", locateSource5);
        db.SetNullableString(command, "locateSource6", locateSource6);
        db.SetNullableString(command, "locateSource7", locateSource7);
        db.SetNullableString(command, "locateSource8", locateSource8);
        db.SetNullableString(command, "ssnValidityCd", ssnValidityCode);
        db.
          SetNullableString(command, "prvdOrCorctdSsn", providedOrCorrectedSsn);
          
        db.SetNullableString(command, "multipleSsn1", multipleSsn1);
        db.SetNullableString(command, "multipleSsn2", multipleSsn2);
        db.SetNullableString(command, "multipleSsn3", multipleSsn3);
        db.
          SetNullableString(command, "ssaDobIndicator", ssaDateOfBirthIndicator);
          
        db.SetNullableString(command, "batchNumber", batchNumber);
        db.SetNullableDate(command, "dateOfDeath", dateOfDeath);
        db.SetNullableString(
          command, "ssaZipLastResi", ssaZipCodeOfLastResidence);
        db.SetNullableString(
          command, "ssaZipLsPaymnt", ssaZipCodeOfLumpSumPayment);
        db.SetNullableString(command, "fcrPrimarySsn", fcrPrimarySsn);
        db.SetNullableString(command, "fcrPriFirstName", fcrPrimaryFirstName);
        db.SetNullableString(command, "fcrPriMiddleNm", fcrPrimaryMiddleName);
        db.SetNullableString(command, "fcrPriLastName", fcrPrimaryLastName);
        db.SetNullableString(command, "ackmntCd", acknowledgementCode);
        db.SetNullableString(command, "errorCode1", errorCode1);
        db.SetNullableString(command, "errorCode2", errorCode2);
        db.SetNullableString(command, "errorCode3", errorCode3);
        db.SetNullableString(command, "errorCode4", errorCode4);
        db.SetNullableString(command, "errorCode5", errorCode5);
        db.SetNullableString(
          command, "addlSsn1ValCd", additionalSsn1ValidityCode);
        db.SetNullableString(
          command, "addlSsn2ValCd", additionalSsn2ValidityCode);
        db.
          SetNullableString(command, "bndlFplsLocRslt", bundleFplsLocateResults);
          
        db.
          SetNullableString(command, "ssaLastResiCity", ssaCityOfLastResidence);
          
        db.SetNullableString(command, "ssaLastResiSt", ssaStateOfLastResidence);
        db.
          SetNullableString(command, "ssaLsPaymntCity", ssaCityOfLumpSumPayment);
          
        db.
          SetNullableString(command, "ssaLsPaymntSt", ssaStateOfLumpSumPayment);
          
      });

    entities.ExistingFcrCaseMembers.FcmCaseId = fcmCaseId;
    entities.ExistingFcrCaseMembers.MemberId = memberId;
    entities.ExistingFcrCaseMembers.ActionTypeCode = actionTypeCode;
    entities.ExistingFcrCaseMembers.LocateRequestType = locateRequestType;
    entities.ExistingFcrCaseMembers.RecordIdentifier = recordIdentifier;
    entities.ExistingFcrCaseMembers.ParticipantType = participantType;
    entities.ExistingFcrCaseMembers.SexCode = sexCode;
    entities.ExistingFcrCaseMembers.DateOfBirth = dateOfBirth;
    entities.ExistingFcrCaseMembers.Ssn = ssn;
    entities.ExistingFcrCaseMembers.FirstName = firstName;
    entities.ExistingFcrCaseMembers.MiddleName = middleName;
    entities.ExistingFcrCaseMembers.LastName = lastName;
    entities.ExistingFcrCaseMembers.FipsCountyCode = fipsCountyCode;
    entities.ExistingFcrCaseMembers.FamilyViolence = familyViolence;
    entities.ExistingFcrCaseMembers.PreviousSsn = previousSsn;
    entities.ExistingFcrCaseMembers.CityOfBirth = cityOfBirth;
    entities.ExistingFcrCaseMembers.StateOrCountryOfBirth =
      stateOrCountryOfBirth;
    entities.ExistingFcrCaseMembers.FathersFirstName = fathersFirstName;
    entities.ExistingFcrCaseMembers.FathersMiddleInitial = fathersMiddleInitial;
    entities.ExistingFcrCaseMembers.FathersLastName = fathersLastName;
    entities.ExistingFcrCaseMembers.MothersFirstName = mothersFirstName;
    entities.ExistingFcrCaseMembers.MothersMiddleInitial = mothersMiddleInitial;
    entities.ExistingFcrCaseMembers.MothersMaidenNm = mothersMaidenNm;
    entities.ExistingFcrCaseMembers.IrsUSsn = irsUSsn;
    entities.ExistingFcrCaseMembers.AdditionalSsn1 = additionalSsn1;
    entities.ExistingFcrCaseMembers.AdditionalSsn2 = additionalSsn2;
    entities.ExistingFcrCaseMembers.AdditionalFirstName1 = additionalFirstName1;
    entities.ExistingFcrCaseMembers.AdditionalMiddleName1 =
      additionalMiddleName1;
    entities.ExistingFcrCaseMembers.AdditionalLastName1 = additionalLastName1;
    entities.ExistingFcrCaseMembers.AdditionalFirstName2 = additionalFirstName2;
    entities.ExistingFcrCaseMembers.AdditionalMiddleName2 =
      additionalMiddleName2;
    entities.ExistingFcrCaseMembers.AdditionalLastName2 = additionalLastName2;
    entities.ExistingFcrCaseMembers.AdditionalFirstName3 = additionalFirstName3;
    entities.ExistingFcrCaseMembers.AdditionalMiddleName3 =
      additionalMiddleName3;
    entities.ExistingFcrCaseMembers.AdditionalLastName3 = additionalLastName3;
    entities.ExistingFcrCaseMembers.AdditionalFirstName4 = additionalFirstName4;
    entities.ExistingFcrCaseMembers.AdditionalMiddleName4 =
      additionalMiddleName4;
    entities.ExistingFcrCaseMembers.AdditionalLastName4 = additionalLastName4;
    entities.ExistingFcrCaseMembers.NewMemberId = newMemberId;
    entities.ExistingFcrCaseMembers.Irs1099 = irs1099;
    entities.ExistingFcrCaseMembers.LocateSource1 = locateSource1;
    entities.ExistingFcrCaseMembers.LocateSource2 = locateSource2;
    entities.ExistingFcrCaseMembers.LocateSource3 = locateSource3;
    entities.ExistingFcrCaseMembers.LocateSource4 = locateSource4;
    entities.ExistingFcrCaseMembers.LocateSource5 = locateSource5;
    entities.ExistingFcrCaseMembers.LocateSource6 = locateSource6;
    entities.ExistingFcrCaseMembers.LocateSource7 = locateSource7;
    entities.ExistingFcrCaseMembers.LocateSource8 = locateSource8;
    entities.ExistingFcrCaseMembers.SsnValidityCode = ssnValidityCode;
    entities.ExistingFcrCaseMembers.ProvidedOrCorrectedSsn =
      providedOrCorrectedSsn;
    entities.ExistingFcrCaseMembers.MultipleSsn1 = multipleSsn1;
    entities.ExistingFcrCaseMembers.MultipleSsn2 = multipleSsn2;
    entities.ExistingFcrCaseMembers.MultipleSsn3 = multipleSsn3;
    entities.ExistingFcrCaseMembers.SsaDateOfBirthIndicator =
      ssaDateOfBirthIndicator;
    entities.ExistingFcrCaseMembers.BatchNumber = batchNumber;
    entities.ExistingFcrCaseMembers.DateOfDeath = dateOfDeath;
    entities.ExistingFcrCaseMembers.SsaZipCodeOfLastResidence =
      ssaZipCodeOfLastResidence;
    entities.ExistingFcrCaseMembers.SsaZipCodeOfLumpSumPayment =
      ssaZipCodeOfLumpSumPayment;
    entities.ExistingFcrCaseMembers.FcrPrimarySsn = fcrPrimarySsn;
    entities.ExistingFcrCaseMembers.FcrPrimaryFirstName = fcrPrimaryFirstName;
    entities.ExistingFcrCaseMembers.FcrPrimaryMiddleName = fcrPrimaryMiddleName;
    entities.ExistingFcrCaseMembers.FcrPrimaryLastName = fcrPrimaryLastName;
    entities.ExistingFcrCaseMembers.AcknowledgementCode = acknowledgementCode;
    entities.ExistingFcrCaseMembers.ErrorCode1 = errorCode1;
    entities.ExistingFcrCaseMembers.ErrorCode2 = errorCode2;
    entities.ExistingFcrCaseMembers.ErrorCode3 = errorCode3;
    entities.ExistingFcrCaseMembers.ErrorCode4 = errorCode4;
    entities.ExistingFcrCaseMembers.ErrorCode5 = errorCode5;
    entities.ExistingFcrCaseMembers.AdditionalSsn1ValidityCode =
      additionalSsn1ValidityCode;
    entities.ExistingFcrCaseMembers.AdditionalSsn2ValidityCode =
      additionalSsn2ValidityCode;
    entities.ExistingFcrCaseMembers.BundleFplsLocateResults =
      bundleFplsLocateResults;
    entities.ExistingFcrCaseMembers.SsaCityOfLastResidence =
      ssaCityOfLastResidence;
    entities.ExistingFcrCaseMembers.SsaStateOfLastResidence =
      ssaStateOfLastResidence;
    entities.ExistingFcrCaseMembers.SsaCityOfLumpSumPayment =
      ssaCityOfLumpSumPayment;
    entities.ExistingFcrCaseMembers.SsaStateOfLumpSumPayment =
      ssaStateOfLumpSumPayment;
    entities.ExistingFcrCaseMembers.Populated = true;
  }

  private void DeleteFcrCaseMaster()
  {
    Update("DeleteFcrCaseMaster",
      (db, command) =>
      {
        db.SetString(command, "caseId", entities.ExistingFcrCaseMaster.CaseId);
      });
  }

  private void DeleteFcrCaseMembers()
  {
    Update("DeleteFcrCaseMembers",
      (db, command) =>
      {
        db.SetString(
          command, "fcmCaseId", entities.ExistingFcrCaseMembers.FcmCaseId);
        db.SetString(
          command, "memberId", entities.ExistingFcrCaseMembers.MemberId);
      });
  }

  private bool ReadFcrCaseMaster1()
  {
    entities.ExistingFcrCaseMaster.Populated = false;

    return Read("ReadFcrCaseMaster1",
      (db, command) =>
      {
        db.SetString(command, "caseId", local.FcrMasterMemberRecord.CaseId);
      },
      (db, reader) =>
      {
        entities.ExistingFcrCaseMaster.CaseId = db.GetString(reader, 0);
        entities.ExistingFcrCaseMaster.OrderIndicator =
          db.GetNullableString(reader, 1);
        entities.ExistingFcrCaseMaster.ActionTypeCode =
          db.GetNullableString(reader, 2);
        entities.ExistingFcrCaseMaster.BatchNumber =
          db.GetNullableString(reader, 3);
        entities.ExistingFcrCaseMaster.FipsCountyCode =
          db.GetNullableString(reader, 4);
        entities.ExistingFcrCaseMaster.PreviousCaseId =
          db.GetNullableString(reader, 5);
        entities.ExistingFcrCaseMaster.CaseSentDateToFcr =
          db.GetNullableDate(reader, 6);
        entities.ExistingFcrCaseMaster.FcrCaseResponseDate =
          db.GetNullableDate(reader, 7);
        entities.ExistingFcrCaseMaster.AcknowlegementCode =
          db.GetNullableString(reader, 8);
        entities.ExistingFcrCaseMaster.ErrorCode1 =
          db.GetNullableString(reader, 9);
        entities.ExistingFcrCaseMaster.ErrorCode2 =
          db.GetNullableString(reader, 10);
        entities.ExistingFcrCaseMaster.ErrorCode3 =
          db.GetNullableString(reader, 11);
        entities.ExistingFcrCaseMaster.ErrorCode4 =
          db.GetNullableString(reader, 12);
        entities.ExistingFcrCaseMaster.ErrorCode5 =
          db.GetNullableString(reader, 13);
        entities.ExistingFcrCaseMaster.CreatedBy = db.GetString(reader, 14);
        entities.ExistingFcrCaseMaster.CreatedTimestamp =
          db.GetDateTime(reader, 15);
        entities.ExistingFcrCaseMaster.RecordIdentifier =
          db.GetNullableString(reader, 16);
        entities.ExistingFcrCaseMaster.FcrCaseComments =
          db.GetNullableString(reader, 17);
        entities.ExistingFcrCaseMaster.Populated = true;
      });
  }

  private bool ReadFcrCaseMaster2()
  {
    entities.ExistingFcrCaseMaster.Populated = false;

    return Read("ReadFcrCaseMaster2",
      (db, command) =>
      {
        db.SetString(command, "caseId", local.FcrCaseMaster.CaseId);
      },
      (db, reader) =>
      {
        entities.ExistingFcrCaseMaster.CaseId = db.GetString(reader, 0);
        entities.ExistingFcrCaseMaster.OrderIndicator =
          db.GetNullableString(reader, 1);
        entities.ExistingFcrCaseMaster.ActionTypeCode =
          db.GetNullableString(reader, 2);
        entities.ExistingFcrCaseMaster.BatchNumber =
          db.GetNullableString(reader, 3);
        entities.ExistingFcrCaseMaster.FipsCountyCode =
          db.GetNullableString(reader, 4);
        entities.ExistingFcrCaseMaster.PreviousCaseId =
          db.GetNullableString(reader, 5);
        entities.ExistingFcrCaseMaster.CaseSentDateToFcr =
          db.GetNullableDate(reader, 6);
        entities.ExistingFcrCaseMaster.FcrCaseResponseDate =
          db.GetNullableDate(reader, 7);
        entities.ExistingFcrCaseMaster.AcknowlegementCode =
          db.GetNullableString(reader, 8);
        entities.ExistingFcrCaseMaster.ErrorCode1 =
          db.GetNullableString(reader, 9);
        entities.ExistingFcrCaseMaster.ErrorCode2 =
          db.GetNullableString(reader, 10);
        entities.ExistingFcrCaseMaster.ErrorCode3 =
          db.GetNullableString(reader, 11);
        entities.ExistingFcrCaseMaster.ErrorCode4 =
          db.GetNullableString(reader, 12);
        entities.ExistingFcrCaseMaster.ErrorCode5 =
          db.GetNullableString(reader, 13);
        entities.ExistingFcrCaseMaster.CreatedBy = db.GetString(reader, 14);
        entities.ExistingFcrCaseMaster.CreatedTimestamp =
          db.GetDateTime(reader, 15);
        entities.ExistingFcrCaseMaster.RecordIdentifier =
          db.GetNullableString(reader, 16);
        entities.ExistingFcrCaseMaster.FcrCaseComments =
          db.GetNullableString(reader, 17);
        entities.ExistingFcrCaseMaster.Populated = true;
      });
  }

  private bool ReadFcrCaseMembers()
  {
    entities.ExistingFcrCaseMembers.Populated = false;

    return Read("ReadFcrCaseMembers",
      (db, command) =>
      {
        db.SetString(command, "memberId", local.FcrCaseMembers.MemberId);
        db.
          SetString(command, "fcmCaseId", entities.ExistingFcrCaseMaster.CaseId);
          
      },
      (db, reader) =>
      {
        entities.ExistingFcrCaseMembers.FcmCaseId = db.GetString(reader, 0);
        entities.ExistingFcrCaseMembers.MemberId = db.GetString(reader, 1);
        entities.ExistingFcrCaseMembers.ActionTypeCode =
          db.GetNullableString(reader, 2);
        entities.ExistingFcrCaseMembers.LocateRequestType =
          db.GetNullableString(reader, 3);
        entities.ExistingFcrCaseMembers.RecordIdentifier =
          db.GetNullableString(reader, 4);
        entities.ExistingFcrCaseMembers.ParticipantType =
          db.GetNullableString(reader, 5);
        entities.ExistingFcrCaseMembers.SexCode =
          db.GetNullableString(reader, 6);
        entities.ExistingFcrCaseMembers.DateOfBirth =
          db.GetNullableDate(reader, 7);
        entities.ExistingFcrCaseMembers.Ssn = db.GetNullableString(reader, 8);
        entities.ExistingFcrCaseMembers.FirstName =
          db.GetNullableString(reader, 9);
        entities.ExistingFcrCaseMembers.MiddleName =
          db.GetNullableString(reader, 10);
        entities.ExistingFcrCaseMembers.LastName =
          db.GetNullableString(reader, 11);
        entities.ExistingFcrCaseMembers.FipsCountyCode =
          db.GetNullableString(reader, 12);
        entities.ExistingFcrCaseMembers.FamilyViolence =
          db.GetNullableString(reader, 13);
        entities.ExistingFcrCaseMembers.PreviousSsn =
          db.GetNullableString(reader, 14);
        entities.ExistingFcrCaseMembers.CityOfBirth =
          db.GetNullableString(reader, 15);
        entities.ExistingFcrCaseMembers.StateOrCountryOfBirth =
          db.GetNullableString(reader, 16);
        entities.ExistingFcrCaseMembers.FathersFirstName =
          db.GetNullableString(reader, 17);
        entities.ExistingFcrCaseMembers.FathersMiddleInitial =
          db.GetNullableString(reader, 18);
        entities.ExistingFcrCaseMembers.FathersLastName =
          db.GetNullableString(reader, 19);
        entities.ExistingFcrCaseMembers.MothersFirstName =
          db.GetNullableString(reader, 20);
        entities.ExistingFcrCaseMembers.MothersMiddleInitial =
          db.GetNullableString(reader, 21);
        entities.ExistingFcrCaseMembers.MothersMaidenNm =
          db.GetNullableString(reader, 22);
        entities.ExistingFcrCaseMembers.IrsUSsn =
          db.GetNullableString(reader, 23);
        entities.ExistingFcrCaseMembers.AdditionalSsn1 =
          db.GetNullableString(reader, 24);
        entities.ExistingFcrCaseMembers.AdditionalSsn2 =
          db.GetNullableString(reader, 25);
        entities.ExistingFcrCaseMembers.AdditionalFirstName1 =
          db.GetNullableString(reader, 26);
        entities.ExistingFcrCaseMembers.AdditionalMiddleName1 =
          db.GetNullableString(reader, 27);
        entities.ExistingFcrCaseMembers.AdditionalLastName1 =
          db.GetNullableString(reader, 28);
        entities.ExistingFcrCaseMembers.AdditionalFirstName2 =
          db.GetNullableString(reader, 29);
        entities.ExistingFcrCaseMembers.AdditionalMiddleName2 =
          db.GetNullableString(reader, 30);
        entities.ExistingFcrCaseMembers.AdditionalLastName2 =
          db.GetNullableString(reader, 31);
        entities.ExistingFcrCaseMembers.AdditionalFirstName3 =
          db.GetNullableString(reader, 32);
        entities.ExistingFcrCaseMembers.AdditionalMiddleName3 =
          db.GetNullableString(reader, 33);
        entities.ExistingFcrCaseMembers.AdditionalLastName3 =
          db.GetNullableString(reader, 34);
        entities.ExistingFcrCaseMembers.AdditionalFirstName4 =
          db.GetNullableString(reader, 35);
        entities.ExistingFcrCaseMembers.AdditionalMiddleName4 =
          db.GetNullableString(reader, 36);
        entities.ExistingFcrCaseMembers.AdditionalLastName4 =
          db.GetNullableString(reader, 37);
        entities.ExistingFcrCaseMembers.NewMemberId =
          db.GetNullableString(reader, 38);
        entities.ExistingFcrCaseMembers.Irs1099 =
          db.GetNullableString(reader, 39);
        entities.ExistingFcrCaseMembers.LocateSource1 =
          db.GetNullableString(reader, 40);
        entities.ExistingFcrCaseMembers.LocateSource2 =
          db.GetNullableString(reader, 41);
        entities.ExistingFcrCaseMembers.LocateSource3 =
          db.GetNullableString(reader, 42);
        entities.ExistingFcrCaseMembers.LocateSource4 =
          db.GetNullableString(reader, 43);
        entities.ExistingFcrCaseMembers.LocateSource5 =
          db.GetNullableString(reader, 44);
        entities.ExistingFcrCaseMembers.LocateSource6 =
          db.GetNullableString(reader, 45);
        entities.ExistingFcrCaseMembers.LocateSource7 =
          db.GetNullableString(reader, 46);
        entities.ExistingFcrCaseMembers.LocateSource8 =
          db.GetNullableString(reader, 47);
        entities.ExistingFcrCaseMembers.SsnValidityCode =
          db.GetNullableString(reader, 48);
        entities.ExistingFcrCaseMembers.ProvidedOrCorrectedSsn =
          db.GetNullableString(reader, 49);
        entities.ExistingFcrCaseMembers.MultipleSsn1 =
          db.GetNullableString(reader, 50);
        entities.ExistingFcrCaseMembers.MultipleSsn2 =
          db.GetNullableString(reader, 51);
        entities.ExistingFcrCaseMembers.MultipleSsn3 =
          db.GetNullableString(reader, 52);
        entities.ExistingFcrCaseMembers.SsaDateOfBirthIndicator =
          db.GetNullableString(reader, 53);
        entities.ExistingFcrCaseMembers.BatchNumber =
          db.GetNullableString(reader, 54);
        entities.ExistingFcrCaseMembers.DateOfDeath =
          db.GetNullableDate(reader, 55);
        entities.ExistingFcrCaseMembers.SsaZipCodeOfLastResidence =
          db.GetNullableString(reader, 56);
        entities.ExistingFcrCaseMembers.SsaZipCodeOfLumpSumPayment =
          db.GetNullableString(reader, 57);
        entities.ExistingFcrCaseMembers.FcrPrimarySsn =
          db.GetNullableString(reader, 58);
        entities.ExistingFcrCaseMembers.FcrPrimaryFirstName =
          db.GetNullableString(reader, 59);
        entities.ExistingFcrCaseMembers.FcrPrimaryMiddleName =
          db.GetNullableString(reader, 60);
        entities.ExistingFcrCaseMembers.FcrPrimaryLastName =
          db.GetNullableString(reader, 61);
        entities.ExistingFcrCaseMembers.AcknowledgementCode =
          db.GetNullableString(reader, 62);
        entities.ExistingFcrCaseMembers.ErrorCode1 =
          db.GetNullableString(reader, 63);
        entities.ExistingFcrCaseMembers.ErrorCode2 =
          db.GetNullableString(reader, 64);
        entities.ExistingFcrCaseMembers.ErrorCode3 =
          db.GetNullableString(reader, 65);
        entities.ExistingFcrCaseMembers.ErrorCode4 =
          db.GetNullableString(reader, 66);
        entities.ExistingFcrCaseMembers.ErrorCode5 =
          db.GetNullableString(reader, 67);
        entities.ExistingFcrCaseMembers.AdditionalSsn1ValidityCode =
          db.GetNullableString(reader, 68);
        entities.ExistingFcrCaseMembers.AdditionalSsn2ValidityCode =
          db.GetNullableString(reader, 69);
        entities.ExistingFcrCaseMembers.BundleFplsLocateResults =
          db.GetNullableString(reader, 70);
        entities.ExistingFcrCaseMembers.SsaCityOfLastResidence =
          db.GetNullableString(reader, 71);
        entities.ExistingFcrCaseMembers.SsaStateOfLastResidence =
          db.GetNullableString(reader, 72);
        entities.ExistingFcrCaseMembers.SsaCityOfLumpSumPayment =
          db.GetNullableString(reader, 73);
        entities.ExistingFcrCaseMembers.SsaStateOfLumpSumPayment =
          db.GetNullableString(reader, 74);
        entities.ExistingFcrCaseMembers.Populated = true;
      });
  }

  private void UpdateFcrCaseMaster1()
  {
    var batchNumber = local.FcrCaseMembers.BatchNumber ?? "";

    entities.ExistingFcrCaseMaster.Populated = false;
    Update("UpdateFcrCaseMaster1",
      (db, command) =>
      {
        db.SetNullableString(command, "batchNumber", batchNumber);
        db.SetString(command, "caseId", entities.ExistingFcrCaseMaster.CaseId);
      });

    entities.ExistingFcrCaseMaster.BatchNumber = batchNumber;
    entities.ExistingFcrCaseMaster.Populated = true;
  }

  private void UpdateFcrCaseMaster2()
  {
    var orderIndicator = local.FcrCaseMaster.OrderIndicator ?? "";
    var actionTypeCode = local.FcrCaseMaster.ActionTypeCode ?? "";
    var batchNumber = local.FcrCaseMaster.BatchNumber ?? "";
    var fipsCountyCode = local.FcrCaseMaster.FipsCountyCode ?? "";
    var previousCaseId = local.FcrCaseMaster.PreviousCaseId ?? "";
    var fcrCaseResponseDate = local.FcrCaseMaster.FcrCaseResponseDate;
    var acknowlegementCode = local.FcrCaseMaster.AcknowlegementCode ?? "";
    var errorCode1 = local.FcrCaseMaster.ErrorCode1 ?? "";
    var errorCode2 = local.FcrCaseMaster.ErrorCode2 ?? "";
    var errorCode3 = local.FcrCaseMaster.ErrorCode3 ?? "";
    var errorCode4 = local.FcrCaseMaster.ErrorCode4 ?? "";
    var errorCode5 = local.FcrCaseMaster.ErrorCode5 ?? "";
    var recordIdentifier = local.FcrCaseMaster.RecordIdentifier ?? "";
    var fcrCaseComments = local.FcrCaseMaster.FcrCaseComments ?? "";

    entities.ExistingFcrCaseMaster.Populated = false;
    Update("UpdateFcrCaseMaster2",
      (db, command) =>
      {
        db.SetNullableString(command, "orderIndicator", orderIndicator);
        db.SetNullableString(command, "actionTypeCd", actionTypeCode);
        db.SetNullableString(command, "batchNumber", batchNumber);
        db.SetNullableString(command, "fipsCountyCd", fipsCountyCode);
        db.SetNullableString(command, "previousCaseId", previousCaseId);
        db.SetNullableDate(command, "fcrResponseDt", fcrCaseResponseDate);
        db.SetNullableString(command, "ackmntCd", acknowlegementCode);
        db.SetNullableString(command, "errorCode1", errorCode1);
        db.SetNullableString(command, "errorCode2", errorCode2);
        db.SetNullableString(command, "errorCode3", errorCode3);
        db.SetNullableString(command, "errorCode4", errorCode4);
        db.SetNullableString(command, "errorCode5", errorCode5);
        db.SetNullableString(command, "recordId", recordIdentifier);
        db.SetNullableString(command, "fcrCaseComments", fcrCaseComments);
        db.SetString(command, "caseId", entities.ExistingFcrCaseMaster.CaseId);
      });

    entities.ExistingFcrCaseMaster.OrderIndicator = orderIndicator;
    entities.ExistingFcrCaseMaster.ActionTypeCode = actionTypeCode;
    entities.ExistingFcrCaseMaster.BatchNumber = batchNumber;
    entities.ExistingFcrCaseMaster.FipsCountyCode = fipsCountyCode;
    entities.ExistingFcrCaseMaster.PreviousCaseId = previousCaseId;
    entities.ExistingFcrCaseMaster.FcrCaseResponseDate = fcrCaseResponseDate;
    entities.ExistingFcrCaseMaster.AcknowlegementCode = acknowlegementCode;
    entities.ExistingFcrCaseMaster.ErrorCode1 = errorCode1;
    entities.ExistingFcrCaseMaster.ErrorCode2 = errorCode2;
    entities.ExistingFcrCaseMaster.ErrorCode3 = errorCode3;
    entities.ExistingFcrCaseMaster.ErrorCode4 = errorCode4;
    entities.ExistingFcrCaseMaster.ErrorCode5 = errorCode5;
    entities.ExistingFcrCaseMaster.RecordIdentifier = recordIdentifier;
    entities.ExistingFcrCaseMaster.FcrCaseComments = fcrCaseComments;
    entities.ExistingFcrCaseMaster.Populated = true;
  }

  private void UpdateFcrCaseMembers()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingFcrCaseMembers.Populated);

    var actionTypeCode = local.FcrCaseMembers.ActionTypeCode ?? "";
    var locateRequestType = local.FcrCaseMembers.LocateRequestType ?? "";
    var recordIdentifier = local.FcrCaseMembers.RecordIdentifier ?? "";
    var participantType = local.FcrCaseMembers.ParticipantType ?? "";
    var sexCode = local.FcrCaseMembers.SexCode ?? "";
    var dateOfBirth = local.FcrCaseMembers.DateOfBirth;
    var ssn = local.FcrCaseMembers.Ssn ?? "";
    var firstName = local.FcrCaseMembers.FirstName ?? "";
    var middleName = local.FcrCaseMembers.MiddleName ?? "";
    var lastName = local.FcrCaseMembers.LastName ?? "";
    var fipsCountyCode = local.FcrCaseMembers.FipsCountyCode ?? "";
    var familyViolence = local.FcrCaseMembers.FamilyViolence ?? "";
    var previousSsn = local.FcrCaseMembers.PreviousSsn ?? "";
    var cityOfBirth = local.FcrCaseMembers.CityOfBirth ?? "";
    var stateOrCountryOfBirth = local.FcrCaseMembers.StateOrCountryOfBirth ?? ""
      ;
    var fathersFirstName = local.FcrCaseMembers.FathersFirstName ?? "";
    var fathersMiddleInitial = local.FcrCaseMembers.FathersMiddleInitial ?? "";
    var fathersLastName = local.FcrCaseMembers.FathersLastName ?? "";
    var mothersFirstName = local.FcrCaseMembers.MothersFirstName ?? "";
    var mothersMiddleInitial = local.FcrCaseMembers.MothersMiddleInitial ?? "";
    var mothersMaidenNm = local.FcrCaseMembers.MothersMaidenNm ?? "";
    var irsUSsn = local.FcrCaseMembers.IrsUSsn ?? "";
    var additionalSsn1 = local.FcrCaseMembers.AdditionalSsn1 ?? "";
    var additionalSsn2 = local.FcrCaseMembers.AdditionalSsn2 ?? "";
    var additionalFirstName1 = local.FcrCaseMembers.AdditionalFirstName1 ?? "";
    var additionalMiddleName1 = local.FcrCaseMembers.AdditionalMiddleName1 ?? ""
      ;
    var additionalLastName1 = local.FcrCaseMembers.AdditionalLastName1 ?? "";
    var additionalFirstName2 = local.FcrCaseMembers.AdditionalFirstName2 ?? "";
    var additionalMiddleName2 = local.FcrCaseMembers.AdditionalMiddleName2 ?? ""
      ;
    var additionalLastName2 = local.FcrCaseMembers.AdditionalLastName2 ?? "";
    var additionalFirstName3 = local.FcrCaseMembers.AdditionalFirstName3 ?? "";
    var additionalMiddleName3 = local.FcrCaseMembers.AdditionalMiddleName3 ?? ""
      ;
    var additionalLastName3 = local.FcrCaseMembers.AdditionalLastName3 ?? "";
    var additionalFirstName4 = local.FcrCaseMembers.AdditionalFirstName4 ?? "";
    var additionalMiddleName4 = local.FcrCaseMembers.AdditionalMiddleName4 ?? ""
      ;
    var additionalLastName4 = local.FcrCaseMembers.AdditionalLastName4 ?? "";
    var newMemberId = local.FcrCaseMembers.NewMemberId ?? "";
    var irs1099 = local.FcrCaseMembers.Irs1099 ?? "";
    var locateSource1 = local.FcrCaseMembers.LocateSource1 ?? "";
    var locateSource2 = local.FcrCaseMembers.LocateSource2 ?? "";
    var locateSource3 = local.FcrCaseMembers.LocateSource3 ?? "";
    var locateSource4 = local.FcrCaseMembers.LocateSource4 ?? "";
    var locateSource5 = local.FcrCaseMembers.LocateSource5 ?? "";
    var locateSource6 = local.FcrCaseMembers.LocateSource6 ?? "";
    var locateSource7 = local.FcrCaseMembers.LocateSource7 ?? "";
    var locateSource8 = local.FcrCaseMembers.LocateSource8 ?? "";
    var ssnValidityCode = local.FcrCaseMembers.SsnValidityCode ?? "";
    var providedOrCorrectedSsn =
      local.FcrCaseMembers.ProvidedOrCorrectedSsn ?? "";
    var multipleSsn1 = local.FcrCaseMembers.MultipleSsn1 ?? "";
    var multipleSsn2 = local.FcrCaseMembers.MultipleSsn2 ?? "";
    var multipleSsn3 = local.FcrCaseMembers.MultipleSsn3 ?? "";
    var ssaDateOfBirthIndicator =
      local.FcrCaseMembers.SsaDateOfBirthIndicator ?? "";
    var batchNumber = local.FcrCaseMembers.BatchNumber ?? "";
    var dateOfDeath = local.FcrCaseMembers.DateOfDeath;
    var ssaZipCodeOfLastResidence =
      local.FcrCaseMembers.SsaZipCodeOfLastResidence ?? "";
    var ssaZipCodeOfLumpSumPayment =
      local.FcrCaseMembers.SsaZipCodeOfLumpSumPayment ?? "";
    var fcrPrimarySsn = local.FcrCaseMembers.FcrPrimarySsn ?? "";
    var fcrPrimaryFirstName = local.FcrCaseMembers.FcrPrimaryFirstName ?? "";
    var fcrPrimaryMiddleName = local.FcrCaseMembers.FcrPrimaryMiddleName ?? "";
    var fcrPrimaryLastName = local.FcrCaseMembers.FcrPrimaryLastName ?? "";
    var acknowledgementCode = local.FcrCaseMembers.AcknowledgementCode ?? "";
    var errorCode1 = local.FcrCaseMembers.ErrorCode1 ?? "";
    var errorCode2 = local.FcrCaseMembers.ErrorCode2 ?? "";
    var errorCode3 = local.FcrCaseMembers.ErrorCode3 ?? "";
    var errorCode4 = local.FcrCaseMembers.ErrorCode4 ?? "";
    var errorCode5 = local.FcrCaseMembers.ErrorCode5 ?? "";
    var additionalSsn1ValidityCode =
      local.FcrCaseMembers.AdditionalSsn1ValidityCode ?? "";
    var additionalSsn2ValidityCode =
      local.FcrCaseMembers.AdditionalSsn2ValidityCode ?? "";
    var bundleFplsLocateResults =
      local.FcrCaseMembers.BundleFplsLocateResults ?? "";
    var ssaCityOfLastResidence =
      local.FcrCaseMembers.SsaCityOfLastResidence ?? "";
    var ssaStateOfLastResidence =
      local.FcrCaseMembers.SsaStateOfLastResidence ?? "";
    var ssaCityOfLumpSumPayment =
      local.FcrCaseMembers.SsaCityOfLumpSumPayment ?? "";
    var ssaStateOfLumpSumPayment =
      local.FcrCaseMembers.SsaStateOfLumpSumPayment ?? "";

    entities.ExistingFcrCaseMembers.Populated = false;
    Update("UpdateFcrCaseMembers",
      (db, command) =>
      {
        db.SetNullableString(command, "actionTypeCd", actionTypeCode);
        db.SetNullableString(command, "locateReqstType", locateRequestType);
        db.SetNullableString(command, "recordId", recordIdentifier);
        db.SetNullableString(command, "participantType", participantType);
        db.SetNullableString(command, "sexCode", sexCode);
        db.SetNullableDate(command, "dateOfBirth", dateOfBirth);
        db.SetNullableString(command, "ssn", ssn);
        db.SetNullableString(command, "firstName", firstName);
        db.SetNullableString(command, "middleName", middleName);
        db.SetNullableString(command, "lastName", lastName);
        db.SetNullableString(command, "fipsCountyCd", fipsCountyCode);
        db.SetNullableString(command, "familyViolence", familyViolence);
        db.SetNullableString(command, "previousSsn", previousSsn);
        db.SetNullableString(command, "cityOfBirth", cityOfBirth);
        db.SetNullableString(command, "stOrCtryOfBrth", stateOrCountryOfBirth);
        db.SetNullableString(command, "fathersFirstName", fathersFirstName);
        db.SetNullableString(command, "fathersMi", fathersMiddleInitial);
        db.SetNullableString(command, "fathersLastName", fathersLastName);
        db.SetNullableString(command, "mothersFirstNm", mothersFirstName);
        db.SetNullableString(command, "mothersMi", mothersMiddleInitial);
        db.SetNullableString(command, "mothersMaidenNm", mothersMaidenNm);
        db.SetNullableString(command, "irsUSsn", irsUSsn);
        db.SetNullableString(command, "additionalSsn1", additionalSsn1);
        db.SetNullableString(command, "additionalSsn2", additionalSsn2);
        db.SetNullableString(command, "addlFirstName1", additionalFirstName1);
        db.SetNullableString(command, "addlMiddleName1", additionalMiddleName1);
        db.SetNullableString(command, "addlLastName1", additionalLastName1);
        db.SetNullableString(command, "addlFirstName2", additionalFirstName2);
        db.SetNullableString(command, "addlMiddleName2", additionalMiddleName2);
        db.SetNullableString(command, "addlLastName2", additionalLastName2);
        db.SetNullableString(command, "addlFirstName3", additionalFirstName3);
        db.SetNullableString(command, "addlMiddleName3", additionalMiddleName3);
        db.SetNullableString(command, "addlLastName3", additionalLastName3);
        db.SetNullableString(command, "addlFirstName4", additionalFirstName4);
        db.SetNullableString(command, "addlMiddleName4", additionalMiddleName4);
        db.SetNullableString(command, "addlLastName4", additionalLastName4);
        db.SetNullableString(command, "newMemberId", newMemberId);
        db.SetNullableString(command, "irs1099", irs1099);
        db.SetNullableString(command, "locateSource1", locateSource1);
        db.SetNullableString(command, "locateSource2", locateSource2);
        db.SetNullableString(command, "locateSource3", locateSource3);
        db.SetNullableString(command, "locateSource4", locateSource4);
        db.SetNullableString(command, "locateSource5", locateSource5);
        db.SetNullableString(command, "locateSource6", locateSource6);
        db.SetNullableString(command, "locateSource7", locateSource7);
        db.SetNullableString(command, "locateSource8", locateSource8);
        db.SetNullableString(command, "ssnValidityCd", ssnValidityCode);
        db.
          SetNullableString(command, "prvdOrCorctdSsn", providedOrCorrectedSsn);
          
        db.SetNullableString(command, "multipleSsn1", multipleSsn1);
        db.SetNullableString(command, "multipleSsn2", multipleSsn2);
        db.SetNullableString(command, "multipleSsn3", multipleSsn3);
        db.
          SetNullableString(command, "ssaDobIndicator", ssaDateOfBirthIndicator);
          
        db.SetNullableString(command, "batchNumber", batchNumber);
        db.SetNullableDate(command, "dateOfDeath", dateOfDeath);
        db.SetNullableString(
          command, "ssaZipLastResi", ssaZipCodeOfLastResidence);
        db.SetNullableString(
          command, "ssaZipLsPaymnt", ssaZipCodeOfLumpSumPayment);
        db.SetNullableString(command, "fcrPrimarySsn", fcrPrimarySsn);
        db.SetNullableString(command, "fcrPriFirstName", fcrPrimaryFirstName);
        db.SetNullableString(command, "fcrPriMiddleNm", fcrPrimaryMiddleName);
        db.SetNullableString(command, "fcrPriLastName", fcrPrimaryLastName);
        db.SetNullableString(command, "ackmntCd", acknowledgementCode);
        db.SetNullableString(command, "errorCode1", errorCode1);
        db.SetNullableString(command, "errorCode2", errorCode2);
        db.SetNullableString(command, "errorCode3", errorCode3);
        db.SetNullableString(command, "errorCode4", errorCode4);
        db.SetNullableString(command, "errorCode5", errorCode5);
        db.SetNullableString(
          command, "addlSsn1ValCd", additionalSsn1ValidityCode);
        db.SetNullableString(
          command, "addlSsn2ValCd", additionalSsn2ValidityCode);
        db.
          SetNullableString(command, "bndlFplsLocRslt", bundleFplsLocateResults);
          
        db.
          SetNullableString(command, "ssaLastResiCity", ssaCityOfLastResidence);
          
        db.SetNullableString(command, "ssaLastResiSt", ssaStateOfLastResidence);
        db.
          SetNullableString(command, "ssaLsPaymntCity", ssaCityOfLumpSumPayment);
          
        db.
          SetNullableString(command, "ssaLsPaymntSt", ssaStateOfLumpSumPayment);
          
        db.SetString(
          command, "fcmCaseId", entities.ExistingFcrCaseMembers.FcmCaseId);
        db.SetString(
          command, "memberId", entities.ExistingFcrCaseMembers.MemberId);
      });

    entities.ExistingFcrCaseMembers.ActionTypeCode = actionTypeCode;
    entities.ExistingFcrCaseMembers.LocateRequestType = locateRequestType;
    entities.ExistingFcrCaseMembers.RecordIdentifier = recordIdentifier;
    entities.ExistingFcrCaseMembers.ParticipantType = participantType;
    entities.ExistingFcrCaseMembers.SexCode = sexCode;
    entities.ExistingFcrCaseMembers.DateOfBirth = dateOfBirth;
    entities.ExistingFcrCaseMembers.Ssn = ssn;
    entities.ExistingFcrCaseMembers.FirstName = firstName;
    entities.ExistingFcrCaseMembers.MiddleName = middleName;
    entities.ExistingFcrCaseMembers.LastName = lastName;
    entities.ExistingFcrCaseMembers.FipsCountyCode = fipsCountyCode;
    entities.ExistingFcrCaseMembers.FamilyViolence = familyViolence;
    entities.ExistingFcrCaseMembers.PreviousSsn = previousSsn;
    entities.ExistingFcrCaseMembers.CityOfBirth = cityOfBirth;
    entities.ExistingFcrCaseMembers.StateOrCountryOfBirth =
      stateOrCountryOfBirth;
    entities.ExistingFcrCaseMembers.FathersFirstName = fathersFirstName;
    entities.ExistingFcrCaseMembers.FathersMiddleInitial = fathersMiddleInitial;
    entities.ExistingFcrCaseMembers.FathersLastName = fathersLastName;
    entities.ExistingFcrCaseMembers.MothersFirstName = mothersFirstName;
    entities.ExistingFcrCaseMembers.MothersMiddleInitial = mothersMiddleInitial;
    entities.ExistingFcrCaseMembers.MothersMaidenNm = mothersMaidenNm;
    entities.ExistingFcrCaseMembers.IrsUSsn = irsUSsn;
    entities.ExistingFcrCaseMembers.AdditionalSsn1 = additionalSsn1;
    entities.ExistingFcrCaseMembers.AdditionalSsn2 = additionalSsn2;
    entities.ExistingFcrCaseMembers.AdditionalFirstName1 = additionalFirstName1;
    entities.ExistingFcrCaseMembers.AdditionalMiddleName1 =
      additionalMiddleName1;
    entities.ExistingFcrCaseMembers.AdditionalLastName1 = additionalLastName1;
    entities.ExistingFcrCaseMembers.AdditionalFirstName2 = additionalFirstName2;
    entities.ExistingFcrCaseMembers.AdditionalMiddleName2 =
      additionalMiddleName2;
    entities.ExistingFcrCaseMembers.AdditionalLastName2 = additionalLastName2;
    entities.ExistingFcrCaseMembers.AdditionalFirstName3 = additionalFirstName3;
    entities.ExistingFcrCaseMembers.AdditionalMiddleName3 =
      additionalMiddleName3;
    entities.ExistingFcrCaseMembers.AdditionalLastName3 = additionalLastName3;
    entities.ExistingFcrCaseMembers.AdditionalFirstName4 = additionalFirstName4;
    entities.ExistingFcrCaseMembers.AdditionalMiddleName4 =
      additionalMiddleName4;
    entities.ExistingFcrCaseMembers.AdditionalLastName4 = additionalLastName4;
    entities.ExistingFcrCaseMembers.NewMemberId = newMemberId;
    entities.ExistingFcrCaseMembers.Irs1099 = irs1099;
    entities.ExistingFcrCaseMembers.LocateSource1 = locateSource1;
    entities.ExistingFcrCaseMembers.LocateSource2 = locateSource2;
    entities.ExistingFcrCaseMembers.LocateSource3 = locateSource3;
    entities.ExistingFcrCaseMembers.LocateSource4 = locateSource4;
    entities.ExistingFcrCaseMembers.LocateSource5 = locateSource5;
    entities.ExistingFcrCaseMembers.LocateSource6 = locateSource6;
    entities.ExistingFcrCaseMembers.LocateSource7 = locateSource7;
    entities.ExistingFcrCaseMembers.LocateSource8 = locateSource8;
    entities.ExistingFcrCaseMembers.SsnValidityCode = ssnValidityCode;
    entities.ExistingFcrCaseMembers.ProvidedOrCorrectedSsn =
      providedOrCorrectedSsn;
    entities.ExistingFcrCaseMembers.MultipleSsn1 = multipleSsn1;
    entities.ExistingFcrCaseMembers.MultipleSsn2 = multipleSsn2;
    entities.ExistingFcrCaseMembers.MultipleSsn3 = multipleSsn3;
    entities.ExistingFcrCaseMembers.SsaDateOfBirthIndicator =
      ssaDateOfBirthIndicator;
    entities.ExistingFcrCaseMembers.BatchNumber = batchNumber;
    entities.ExistingFcrCaseMembers.DateOfDeath = dateOfDeath;
    entities.ExistingFcrCaseMembers.SsaZipCodeOfLastResidence =
      ssaZipCodeOfLastResidence;
    entities.ExistingFcrCaseMembers.SsaZipCodeOfLumpSumPayment =
      ssaZipCodeOfLumpSumPayment;
    entities.ExistingFcrCaseMembers.FcrPrimarySsn = fcrPrimarySsn;
    entities.ExistingFcrCaseMembers.FcrPrimaryFirstName = fcrPrimaryFirstName;
    entities.ExistingFcrCaseMembers.FcrPrimaryMiddleName = fcrPrimaryMiddleName;
    entities.ExistingFcrCaseMembers.FcrPrimaryLastName = fcrPrimaryLastName;
    entities.ExistingFcrCaseMembers.AcknowledgementCode = acknowledgementCode;
    entities.ExistingFcrCaseMembers.ErrorCode1 = errorCode1;
    entities.ExistingFcrCaseMembers.ErrorCode2 = errorCode2;
    entities.ExistingFcrCaseMembers.ErrorCode3 = errorCode3;
    entities.ExistingFcrCaseMembers.ErrorCode4 = errorCode4;
    entities.ExistingFcrCaseMembers.ErrorCode5 = errorCode5;
    entities.ExistingFcrCaseMembers.AdditionalSsn1ValidityCode =
      additionalSsn1ValidityCode;
    entities.ExistingFcrCaseMembers.AdditionalSsn2ValidityCode =
      additionalSsn2ValidityCode;
    entities.ExistingFcrCaseMembers.BundleFplsLocateResults =
      bundleFplsLocateResults;
    entities.ExistingFcrCaseMembers.SsaCityOfLastResidence =
      ssaCityOfLastResidence;
    entities.ExistingFcrCaseMembers.SsaStateOfLastResidence =
      ssaStateOfLastResidence;
    entities.ExistingFcrCaseMembers.SsaCityOfLumpSumPayment =
      ssaCityOfLumpSumPayment;
    entities.ExistingFcrCaseMembers.SsaStateOfLumpSumPayment =
      ssaStateOfLumpSumPayment;
    entities.ExistingFcrCaseMembers.Populated = true;
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
    /// A value of FcrMasterCaseRecord.
    /// </summary>
    [JsonPropertyName("fcrMasterCaseRecord")]
    public FcrMasterCaseRecord FcrMasterCaseRecord
    {
      get => fcrMasterCaseRecord ??= new();
      set => fcrMasterCaseRecord = value;
    }

    /// <summary>
    /// A value of FcrMasterMemberRecord.
    /// </summary>
    [JsonPropertyName("fcrMasterMemberRecord")]
    public FcrMasterMemberRecord FcrMasterMemberRecord
    {
      get => fcrMasterMemberRecord ??= new();
      set => fcrMasterMemberRecord = value;
    }

    /// <summary>
    /// A value of PersonsCreCount.
    /// </summary>
    [JsonPropertyName("personsCreCount")]
    public Common PersonsCreCount
    {
      get => personsCreCount ??= new();
      set => personsCreCount = value;
    }

    /// <summary>
    /// A value of PersonUpdCount.
    /// </summary>
    [JsonPropertyName("personUpdCount")]
    public Common PersonUpdCount
    {
      get => personUpdCount ??= new();
      set => personUpdCount = value;
    }

    /// <summary>
    /// A value of PersonDelCount.
    /// </summary>
    [JsonPropertyName("personDelCount")]
    public Common PersonDelCount
    {
      get => personDelCount ??= new();
      set => personDelCount = value;
    }

    /// <summary>
    /// A value of PersonSkipCount.
    /// </summary>
    [JsonPropertyName("personSkipCount")]
    public Common PersonSkipCount
    {
      get => personSkipCount ??= new();
      set => personSkipCount = value;
    }

    /// <summary>
    /// A value of CaseCreCount.
    /// </summary>
    [JsonPropertyName("caseCreCount")]
    public Common CaseCreCount
    {
      get => caseCreCount ??= new();
      set => caseCreCount = value;
    }

    /// <summary>
    /// A value of CaseUpdCount.
    /// </summary>
    [JsonPropertyName("caseUpdCount")]
    public Common CaseUpdCount
    {
      get => caseUpdCount ??= new();
      set => caseUpdCount = value;
    }

    /// <summary>
    /// A value of CaseDelCount.
    /// </summary>
    [JsonPropertyName("caseDelCount")]
    public Common CaseDelCount
    {
      get => caseDelCount ??= new();
      set => caseDelCount = value;
    }

    /// <summary>
    /// A value of CaseSkipCount.
    /// </summary>
    [JsonPropertyName("caseSkipCount")]
    public Common CaseSkipCount
    {
      get => caseSkipCount ??= new();
      set => caseSkipCount = value;
    }

    /// <summary>
    /// A value of TotalCreCount.
    /// </summary>
    [JsonPropertyName("totalCreCount")]
    public Common TotalCreCount
    {
      get => totalCreCount ??= new();
      set => totalCreCount = value;
    }

    /// <summary>
    /// A value of TotalUpdCount.
    /// </summary>
    [JsonPropertyName("totalUpdCount")]
    public Common TotalUpdCount
    {
      get => totalUpdCount ??= new();
      set => totalUpdCount = value;
    }

    /// <summary>
    /// A value of TotalDelCount.
    /// </summary>
    [JsonPropertyName("totalDelCount")]
    public Common TotalDelCount
    {
      get => totalDelCount ??= new();
      set => totalDelCount = value;
    }

    /// <summary>
    /// A value of TotalSkipCount.
    /// </summary>
    [JsonPropertyName("totalSkipCount")]
    public Common TotalSkipCount
    {
      get => totalSkipCount ??= new();
      set => totalSkipCount = value;
    }

    /// <summary>
    /// A value of Commit.
    /// </summary>
    [JsonPropertyName("commit")]
    public Common Commit
    {
      get => commit ??= new();
      set => commit = value;
    }

    /// <summary>
    /// A value of ExtractNResponseFlag.
    /// </summary>
    [JsonPropertyName("extractNResponseFlag")]
    public Common ExtractNResponseFlag
    {
      get => extractNResponseFlag ??= new();
      set => extractNResponseFlag = value;
    }

    /// <summary>
    /// A value of CaseDelSendDate.
    /// </summary>
    [JsonPropertyName("caseDelSendDate")]
    public FcrCaseMaster CaseDelSendDate
    {
      get => caseDelSendDate ??= new();
      set => caseDelSendDate = value;
    }

    private FcrMasterCaseRecord fcrMasterCaseRecord;
    private FcrMasterMemberRecord fcrMasterMemberRecord;
    private Common personsCreCount;
    private Common personUpdCount;
    private Common personDelCount;
    private Common personSkipCount;
    private Common caseCreCount;
    private Common caseUpdCount;
    private Common caseDelCount;
    private Common caseSkipCount;
    private Common totalCreCount;
    private Common totalUpdCount;
    private Common totalDelCount;
    private Common totalSkipCount;
    private Common commit;
    private Common extractNResponseFlag;
    private FcrCaseMaster caseDelSendDate;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of PersonsCreCount.
    /// </summary>
    [JsonPropertyName("personsCreCount")]
    public Common PersonsCreCount
    {
      get => personsCreCount ??= new();
      set => personsCreCount = value;
    }

    /// <summary>
    /// A value of PersonUpdCount.
    /// </summary>
    [JsonPropertyName("personUpdCount")]
    public Common PersonUpdCount
    {
      get => personUpdCount ??= new();
      set => personUpdCount = value;
    }

    /// <summary>
    /// A value of PersonDelCount.
    /// </summary>
    [JsonPropertyName("personDelCount")]
    public Common PersonDelCount
    {
      get => personDelCount ??= new();
      set => personDelCount = value;
    }

    /// <summary>
    /// A value of PersonSkipCount.
    /// </summary>
    [JsonPropertyName("personSkipCount")]
    public Common PersonSkipCount
    {
      get => personSkipCount ??= new();
      set => personSkipCount = value;
    }

    /// <summary>
    /// A value of CaseCreCount.
    /// </summary>
    [JsonPropertyName("caseCreCount")]
    public Common CaseCreCount
    {
      get => caseCreCount ??= new();
      set => caseCreCount = value;
    }

    /// <summary>
    /// A value of CaseUpdCount.
    /// </summary>
    [JsonPropertyName("caseUpdCount")]
    public Common CaseUpdCount
    {
      get => caseUpdCount ??= new();
      set => caseUpdCount = value;
    }

    /// <summary>
    /// A value of CaseDelCount.
    /// </summary>
    [JsonPropertyName("caseDelCount")]
    public Common CaseDelCount
    {
      get => caseDelCount ??= new();
      set => caseDelCount = value;
    }

    /// <summary>
    /// A value of CaseSkipCount.
    /// </summary>
    [JsonPropertyName("caseSkipCount")]
    public Common CaseSkipCount
    {
      get => caseSkipCount ??= new();
      set => caseSkipCount = value;
    }

    /// <summary>
    /// A value of TotalCreCount.
    /// </summary>
    [JsonPropertyName("totalCreCount")]
    public Common TotalCreCount
    {
      get => totalCreCount ??= new();
      set => totalCreCount = value;
    }

    /// <summary>
    /// A value of TotalUpdCount.
    /// </summary>
    [JsonPropertyName("totalUpdCount")]
    public Common TotalUpdCount
    {
      get => totalUpdCount ??= new();
      set => totalUpdCount = value;
    }

    /// <summary>
    /// A value of TotalDelCount.
    /// </summary>
    [JsonPropertyName("totalDelCount")]
    public Common TotalDelCount
    {
      get => totalDelCount ??= new();
      set => totalDelCount = value;
    }

    /// <summary>
    /// A value of TotalSkipCount.
    /// </summary>
    [JsonPropertyName("totalSkipCount")]
    public Common TotalSkipCount
    {
      get => totalSkipCount ??= new();
      set => totalSkipCount = value;
    }

    /// <summary>
    /// A value of Commit.
    /// </summary>
    [JsonPropertyName("commit")]
    public Common Commit
    {
      get => commit ??= new();
      set => commit = value;
    }

    /// <summary>
    /// A value of CaseDelSendDate.
    /// </summary>
    [JsonPropertyName("caseDelSendDate")]
    public FcrCaseMaster CaseDelSendDate
    {
      get => caseDelSendDate ??= new();
      set => caseDelSendDate = value;
    }

    private Common personsCreCount;
    private Common personUpdCount;
    private Common personDelCount;
    private Common personSkipCount;
    private Common caseCreCount;
    private Common caseUpdCount;
    private Common caseDelCount;
    private Common caseSkipCount;
    private Common totalCreCount;
    private Common totalUpdCount;
    private Common totalDelCount;
    private Common totalSkipCount;
    private Common commit;
    private FcrCaseMaster caseDelSendDate;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of FcrMasterCaseRecord.
    /// </summary>
    [JsonPropertyName("fcrMasterCaseRecord")]
    public FcrMasterCaseRecord FcrMasterCaseRecord
    {
      get => fcrMasterCaseRecord ??= new();
      set => fcrMasterCaseRecord = value;
    }

    /// <summary>
    /// A value of FcrMasterMemberRecord.
    /// </summary>
    [JsonPropertyName("fcrMasterMemberRecord")]
    public FcrMasterMemberRecord FcrMasterMemberRecord
    {
      get => fcrMasterMemberRecord ??= new();
      set => fcrMasterMemberRecord = value;
    }

    /// <summary>
    /// A value of FcrCaseMaster.
    /// </summary>
    [JsonPropertyName("fcrCaseMaster")]
    public FcrCaseMaster FcrCaseMaster
    {
      get => fcrCaseMaster ??= new();
      set => fcrCaseMaster = value;
    }

    /// <summary>
    /// A value of FcrCaseMembers.
    /// </summary>
    [JsonPropertyName("fcrCaseMembers")]
    public FcrCaseMembers FcrCaseMembers
    {
      get => fcrCaseMembers ??= new();
      set => fcrCaseMembers = value;
    }

    /// <summary>
    /// A value of NullDate.
    /// </summary>
    [JsonPropertyName("nullDate")]
    public FcrCaseMaster NullDate
    {
      get => nullDate ??= new();
      set => nullDate = value;
    }

    private FcrMasterCaseRecord fcrMasterCaseRecord;
    private FcrMasterMemberRecord fcrMasterMemberRecord;
    private FcrCaseMaster fcrCaseMaster;
    private FcrCaseMembers fcrCaseMembers;
    private FcrCaseMaster nullDate;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingFcrCaseMaster.
    /// </summary>
    [JsonPropertyName("existingFcrCaseMaster")]
    public FcrCaseMaster ExistingFcrCaseMaster
    {
      get => existingFcrCaseMaster ??= new();
      set => existingFcrCaseMaster = value;
    }

    /// <summary>
    /// A value of ExistingFcrCaseMembers.
    /// </summary>
    [JsonPropertyName("existingFcrCaseMembers")]
    public FcrCaseMembers ExistingFcrCaseMembers
    {
      get => existingFcrCaseMembers ??= new();
      set => existingFcrCaseMembers = value;
    }

    private FcrCaseMaster existingFcrCaseMaster;
    private FcrCaseMembers existingFcrCaseMembers;
  }
#endregion
}
