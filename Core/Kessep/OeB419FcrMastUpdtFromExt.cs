// Program: OE_B419_FCR_MAST_UPDT_FROM_EXT, ID: 374569214, model: 746.
// Short name: SWEE419B
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B419_FCR_MAST_UPDT_FROM_EXT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class OeB419FcrMastUpdtFromExt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B419_FCR_MAST_UPDT_FROM_EXT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB419FcrMastUpdtFromExt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB419FcrMastUpdtFromExt.
  /// </summary>
  public OeB419FcrMastUpdtFromExt(IContext context, Import import, Export export)
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
    // **************************************************************************************
    // This Batch Procedure updates the CSE FCR Master information tables (
    // Master & Members).
    // This procedure will read the CSE Master File/CSE FCR transmission file/
    // FCR Response
    // file and update CSE FCR Tables based on the information sent and received
    // from FCR.
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
    // * 08/27/2009  Raj S              CQ7190      ***** Initial Coding *****
    // *
    // *
    // 
    // *
    // ***************************************************************************************
    // * 11/09/2010  LSS                CQ23059     Set action type code of "L" 
    // to "A" to    *
    // *
    // 
    // keep from having missing FCR Member due  *
    // *
    // 
    // to Locate Request                        *
    // *
    // 
    // *
    // ***************************************************************************************
    // This needs to be removed to check the check-in with V76
    ExitState = "ACO_NN0000_ALL_OK";
    UseOeB419Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      // Extract restart info here, if needed
      local.RestartCaseId.CaseId =
        Substring(local.ProgramCheckpointRestart.RestartInfo, 1, 10);
      local.CaseCreCount.Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 11, 7));
      local.CaseDelCount.Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 18, 7));
      local.CaseSkipCount.Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 25, 7));
      local.CaseUpdCount.Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 32, 7));
      local.PersonsCreCount.Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 39, 7));
      local.PersonDelCount.Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 46, 7));
      local.PersonSkipCount.Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 53, 7));
      local.PersonUpdCount.Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 60, 7));
      local.TotalCreCount.Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 67, 7));
      local.TotalDelCount.Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 74, 7));
      local.TotalSkipCount.Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 81, 7));
      local.TotalUpdCount.Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 88, 7));
    }

    // ********************************************************************************************
    // To set the Extract flag to 'Y', in order to identify from where the 
    // actions block FCR Create
    // and update action is called.   If the action block is called through 
    // extract process, the AB
    // need to perfrom additional edits.
    // ********************************************************************************************
    local.ExtractNResponseFlag.Flag = "E";

    do
    {
      local.Eab.FileInstruction = "READ";

      // ***************************************************************************************
      // The below mentioned External Action Block (EAB) will read the input 
      // records from the
      // FCR OUTPUT EXTRACT Dataset to load the data to FCR DB2 master tables(
      // Case & Case Member
      // Table).
      // ***************************************************************************************
      UseOeEabReadFcrExtractRecord();

      if (Equal(local.Eab.TextReturnCode, "EF"))
      {
        break;
      }

      if (!IsEmpty(local.Eab.TextReturnCode))
      {
        local.EabReportSend.RptDetail = local.Eab.FileInstruction + local
          .Eab.TextLine8 + local.Eab.TextReturnCode + local.Eab.TextLine80 + local
          .FcrMasterCaseRecord.ErrorCode4 + local
          .FcrMasterCaseRecord.ErrorCode5;
        local.EabFileHandling.Action = "WRITE";
        UseCabErrorReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          break;
        }

        ExitState = "FILE_READ_ERROR_WITH_RB";

        break;
      }

      ++local.FcrMasterReadCount.Count;

      if (!IsEmpty(local.FcrMasterCaseRecord.CaseId))
      {
        local.FcrCaseMaster.CaseId = local.OutputExtFcrOutputCaseRecord.CaseId;
      }
      else
      {
        local.FcrCaseMaster.CaseId =
          local.OutputExtFcrOutputMemberRecord.CaseId;
      }

      if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y' && !
        IsEmpty(local.RestartCaseId.CaseId) && !
        Lt(local.RestartCaseId.CaseId, local.FcrCaseMaster.CaseId))
      {
        continue;
      }

      if (local.Commit.Count >= local
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault() && !
        Equal(local.FcrCaseMaster.CaseId, local.PreviousFcrCaseId.CaseId))
      {
        UseExtToDoACommit();

        if (local.Eab.NumericReturnCode != 0)
        {
          ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

          return;
        }

        local.EabReportSend.RptDetail =
          "Commit Taken after Commit Count reached: " + NumberToString
          (local.Commit.Count, 15);
        local.EabFileHandling.Action = "WRITE";
        UseCabErrorReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          break;
        }

        local.Commit.Count = 0;
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.ProgramName =
          local.ProgramProcessingInfo.Name;
        local.ProgramCheckpointRestart.RestartInfo =
          local.FcrCaseMaster.CaseId + NumberToString
          (local.CaseCreCount.Count, 9, 7) + NumberToString
          (local.CaseDelCount.Count, 9, 7) + NumberToString
          (local.CaseSkipCount.Count, 9, 7) + NumberToString
          (local.CaseUpdCount.Count, 9, 7) + NumberToString
          (local.PersonsCreCount.Count, 9, 7) + NumberToString
          (local.PersonDelCount.Count, 9, 7) + NumberToString
          (local.PersonSkipCount.Count, 9, 7) + NumberToString
          (local.PersonUpdCount.Count, 9, 7) + NumberToString
          (local.TotalCreCount.Count, 9, 7) + NumberToString
          (local.TotalDelCount.Count, 9, 7) + NumberToString
          (local.TotalSkipCount.Count, 9, 7) + NumberToString
          (local.TotalUpdCount.Count, 9, 7);
        UseUpdatePgmCheckpointRestart();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }

      local.PreviousFcrCaseId.CaseId = local.FcrCaseMaster.CaseId;

      // ****************************************************************************************
      // Initialize all FCR response record layouts(920) both Case & Case 
      // Member.
      // ****************************************************************************************
      local.FcrMasterCaseRecord.Assign(local.NullFcrMasterCaseRecord);
      local.FcrMasterMemberRecord.Assign(local.NullFcrMasterMemberRecord);

      // ***************************************************************************************
      // While processing FCR Output FCR Extract records, this process will 
      // select the records
      // which has action type as 'A'add or 'L'ocate because 'D'elete/'C'hange 
      // transactions will
      // be processed only when the response is received from FCR.
      // If the 'A'dd/'L'ocate person is already exists in CSE FCR Master then, 
      // this will be
      // the case where person's primary SSN is change, so this record will be 
      // updated only when
      // we get the response from FCR otherwise will keep the old value because 
      // that is the what
      // with FCR.
      // ***************************************************************************************
      // ****************************************************************************************
      // Move all FCR Output Extract record layout (640 bytes) to FCR response 
      // record layout(920)
      // ****************************************************************************************
      if (!IsEmpty(local.OutputExtFcrOutputCaseRecord.CaseId))
      {
        local.FcrCaseMaster.CaseId = local.OutputExtFcrOutputCaseRecord.CaseId;

        if (AsChar(local.OutputExtFcrOutputCaseRecord.ActionTypeCode) == 'A')
        {
          // ***************************************************************************************
          // Process "A"  action type case records.
          // ***************************************************************************************
        }
        else
        {
          // ***************************************************************************************
          // Skip input records other than A action type and these record types 
          // will be processed
          // at the time of FCR response/acknowledgement process.
          // ***************************************************************************************
          ++local.CaseSkipCount.Count;
          ++local.TotalSkipCount.Count;

          continue;
        }

        local.FcrMasterCaseRecord.AcknowlegementCode = "";
        local.FcrMasterCaseRecord.ActionTypeCode =
          local.OutputExtFcrOutputCaseRecord.ActionTypeCode;
        local.FcrMasterCaseRecord.BatchNumber = "";
        local.FcrMasterCaseRecord.CaseId =
          local.OutputExtFcrOutputCaseRecord.CaseId;
        local.FcrMasterCaseRecord.CreatedBy = "SWEEB419";
        local.FcrMasterCaseRecord.ErrorCode1 = "";
        local.FcrMasterCaseRecord.ErrorCode2 = "";
        local.FcrMasterCaseRecord.ErrorCode3 = "";
        local.FcrMasterCaseRecord.ErrorCode4 = "";
        local.FcrMasterCaseRecord.ErrorCode5 = "";
        local.FcrMasterCaseRecord.FcrCaseComments = "";
        local.FcrMasterCaseRecord.FipsCountyCode =
          local.OutputExtFcrOutputCaseRecord.FipsCountyCode;
        local.FcrMasterCaseRecord.OrderIndicator =
          local.OutputExtFcrOutputCaseRecord.OrderIndicator;
        local.FcrMasterCaseRecord.PreviousCaseId =
          local.OutputExtFcrOutputCaseRecord.PreviousCaseId;
        local.FcrMasterCaseRecord.RecordIdentifier =
          local.OutputExtFcrOutputCaseRecord.RecordId;
        local.FcrMasterCaseRecord.CaseSentDateToFcr =
          local.ProgramProcessingInfo.ProcessDate;
        local.FcrMasterCaseRecord.FcrCaseResponseDate = new DateTime(1, 1, 1);
        local.FcrMasterCaseRecord.CreatedTimestamp = Now();
      }
      else
      {
        local.FcrCaseMaster.CaseId =
          local.OutputExtFcrOutputMemberRecord.CaseId;

        // CQ23059
        // Don't want to skip "L" action types so change them to "A" to be 
        // processed
        // in the action block appropriately.
        if (AsChar(local.OutputExtFcrOutputMemberRecord.ActionTypeCode) == 'L')
        {
          local.OutputExtFcrOutputMemberRecord.ActionTypeCode = "A";
        }

        if (AsChar(local.OutputExtFcrOutputMemberRecord.ActionTypeCode) == 'A')
        {
          // ***************************************************************************************
          // Process "A" action type case records. Other types 'C'hange and 'D'
          // elete record types
          // will be processed at the time of FCR response/acknowledgement 
          // process.
          // ***************************************************************************************
          // ****************************************************************************************
          // Move all FCR Output Extract record layout (640 bytes) to FCR 
          // response record layout(920)
          // ****************************************************************************************
        }
        else
        {
          // ***************************************************************************************
          // Skip input records other than A action type and these record types 
          // will be processed
          // at the time of FCR response/acknowledgement process.
          // ***************************************************************************************
          ++local.PersonSkipCount.Count;
          ++local.TotalSkipCount.Count;

          continue;
        }

        local.FcrMasterMemberRecord.AcknowledgementCode = "";
        local.FcrMasterMemberRecord.ActionTypeCode =
          local.OutputExtFcrOutputMemberRecord.ActionTypeCode;
        local.FcrMasterMemberRecord.AdditionalFirstName1 =
          local.OutputExtFcrOutputMemberRecord.AdditionalFnam1;
        local.FcrMasterMemberRecord.AdditionalFirstName2 =
          local.OutputExtFcrOutputMemberRecord.AdditionalFnam2;
        local.FcrMasterMemberRecord.AdditionalFirstName3 =
          local.OutputExtFcrOutputMemberRecord.AdditionalFnam3;
        local.FcrMasterMemberRecord.AdditionalFirstName4 =
          local.OutputExtFcrOutputMemberRecord.AdditionalFnam4;
        local.FcrMasterMemberRecord.AdditionalLastName1 =
          local.OutputExtFcrOutputMemberRecord.AdditionalLnam1;
        local.FcrMasterMemberRecord.AdditionalLastName2 =
          local.OutputExtFcrOutputMemberRecord.AdditionalLnam2;
        local.FcrMasterMemberRecord.AdditionalLastName3 =
          local.OutputExtFcrOutputMemberRecord.AdditionalLnam3;
        local.FcrMasterMemberRecord.AdditionalLastName4 =
          local.OutputExtFcrOutputMemberRecord.AdditionalLnam4;
        local.FcrMasterMemberRecord.AdditionalMiddleName1 =
          local.OutputExtFcrOutputMemberRecord.AdditionalMnam1;
        local.FcrMasterMemberRecord.AdditionalMiddleName2 =
          local.OutputExtFcrOutputMemberRecord.AdditionalMnam2;
        local.FcrMasterMemberRecord.AdditionalMiddleName3 =
          local.OutputExtFcrOutputMemberRecord.AdditionalMnam3;
        local.FcrMasterMemberRecord.AdditionalMiddleName4 =
          local.OutputExtFcrOutputMemberRecord.AdditionalMnam4;
        local.FcrMasterMemberRecord.AdditionalSsn1ValidityCode = "";
        local.FcrMasterMemberRecord.AdditionalSsn2ValidityCode = "";
        local.FcrMasterMemberRecord.AdditionalSsn1 =
          local.OutputExtFcrOutputMemberRecord.AdditionalSsn1;
        local.FcrMasterMemberRecord.AdditionalSsn2 =
          local.OutputExtFcrOutputMemberRecord.AdditionalSsn2;
        local.FcrMasterMemberRecord.BatchNumber = "";
        local.FcrMasterMemberRecord.BundleFplsLocateResults =
          local.OutputExtFcrOutputMemberRecord.BundleResults;
        local.FcrMasterMemberRecord.CaseId =
          local.OutputExtFcrOutputMemberRecord.CaseId;
        local.FcrMasterMemberRecord.CityOfBirth =
          local.OutputExtFcrOutputMemberRecord.CityOfBirth;
        local.FcrMasterMemberRecord.ErrorCode1 = "";
        local.FcrMasterMemberRecord.ErrorCode2 = "";
        local.FcrMasterMemberRecord.ErrorCode3 = "";
        local.FcrMasterMemberRecord.ErrorCode4 = "";
        local.FcrMasterMemberRecord.ErrorCode5 = "";
        local.FcrMasterMemberRecord.FamilyViolence =
          local.OutputExtFcrOutputMemberRecord.FamilyViolence;
        local.FcrMasterMemberRecord.FathersFirstName =
          local.OutputExtFcrOutputMemberRecord.FathersFirstName;
        local.FcrMasterMemberRecord.FathersLastName =
          local.OutputExtFcrOutputMemberRecord.FathersLastName;
        local.FcrMasterMemberRecord.FathersMiddleInitial =
          local.OutputExtFcrOutputMemberRecord.FathersMidName;
        local.FcrMasterMemberRecord.FcrPrimaryFirstName = "";
        local.FcrMasterMemberRecord.FcrPrimaryLastName = "";
        local.FcrMasterMemberRecord.FcrPrimaryMiddleName = "";
        local.FcrMasterMemberRecord.FcrPrimarySsn = "";
        local.FcrMasterMemberRecord.FipsCountyCode =
          local.OutputExtFcrOutputMemberRecord.FipsCountyCode;
        local.FcrMasterMemberRecord.FirstName =
          local.OutputExtFcrOutputMemberRecord.FirstName;
        local.FcrMasterMemberRecord.Irs1099 =
          local.OutputExtFcrOutputMemberRecord.Irs1099;
        local.FcrMasterMemberRecord.IrsUSsn =
          local.OutputExtFcrOutputMemberRecord.IrsUSsn;
        local.FcrMasterMemberRecord.LastName =
          local.OutputExtFcrOutputMemberRecord.LastName;
        local.FcrMasterMemberRecord.LocateRequestType =
          local.OutputExtFcrOutputMemberRecord.LocateReqType;
        local.FcrMasterMemberRecord.LocateSource1 =
          local.OutputExtFcrOutputMemberRecord.LocateSource1;
        local.FcrMasterMemberRecord.LocateSource2 =
          local.OutputExtFcrOutputMemberRecord.LocateSource2;
        local.FcrMasterMemberRecord.LocateSource3 =
          local.OutputExtFcrOutputMemberRecord.LocateSource3;
        local.FcrMasterMemberRecord.LocateSource4 =
          local.OutputExtFcrOutputMemberRecord.LocateSource4;
        local.FcrMasterMemberRecord.LocateSource5 =
          local.OutputExtFcrOutputMemberRecord.LocateSource5;
        local.FcrMasterMemberRecord.LocateSource6 =
          local.OutputExtFcrOutputMemberRecord.LocateSource6;
        local.FcrMasterMemberRecord.LocateSource7 =
          local.OutputExtFcrOutputMemberRecord.LocateSource7;
        local.FcrMasterMemberRecord.LocateSource8 =
          local.OutputExtFcrOutputMemberRecord.LocateSource8;
        local.FcrMasterMemberRecord.MemberId =
          local.OutputExtFcrOutputMemberRecord.MemberId;
        local.FcrMasterMemberRecord.MiddleName =
          local.OutputExtFcrOutputMemberRecord.MiddleName;
        local.FcrMasterMemberRecord.MothersFirstName =
          local.OutputExtFcrOutputMemberRecord.MothersFirstName;
        local.FcrMasterMemberRecord.MothersMaidenNm =
          local.OutputExtFcrOutputMemberRecord.MothersLastName;
        local.FcrMasterMemberRecord.MothersMiddleInitial =
          local.OutputExtFcrOutputMemberRecord.MothersMidName;
        local.FcrMasterMemberRecord.MultipleSsn1 = "";
        local.FcrMasterMemberRecord.MultipleSsn2 = "";
        local.FcrMasterMemberRecord.MultipleSsn3 = "";
        local.FcrMasterMemberRecord.NewMemberId =
          local.OutputExtFcrOutputMemberRecord.NewMemberId;
        local.FcrMasterMemberRecord.ParticipantType =
          local.OutputExtFcrOutputMemberRecord.ParticipantType;
        local.FcrMasterMemberRecord.PreviousSsn =
          local.OutputExtFcrOutputMemberRecord.PreviousSsn;
        local.FcrMasterMemberRecord.ProvidedOrCorrectedSsn = "";
        local.FcrMasterMemberRecord.RecordIdentifier =
          local.OutputExtFcrOutputMemberRecord.RecordId;
        local.FcrMasterMemberRecord.SexCode =
          local.OutputExtFcrOutputMemberRecord.SexCode;
        local.FcrMasterMemberRecord.SsaCityOfLastResidence = "";
        local.FcrMasterMemberRecord.SsaCityOfLumpSumPayment = "";
        local.FcrMasterMemberRecord.SsaDateOfBirthIndicator = "";
        local.FcrMasterMemberRecord.SsaStateOfLastResidence = "";
        local.FcrMasterMemberRecord.SsaStateOfLumpSumPayment = "";
        local.FcrMasterMemberRecord.SsaZipCodeOfLastResidence = "";
        local.FcrMasterMemberRecord.SsaZipCodeOfLumpSumPayment = "";
        local.FcrMasterMemberRecord.Ssn =
          local.OutputExtFcrOutputMemberRecord.Ssn;
        local.FcrMasterMemberRecord.SsnValidityCode = "";
        local.FcrMasterMemberRecord.StateOrCountryOfBirth =
          local.OutputExtFcrOutputMemberRecord.StateCntyBirth;
        local.FcrMasterMemberRecord.DateOfBirth =
          StringToDate(Substring(
            local.OutputExtFcrOutputMemberRecord.DateOfBirth,
          FcrOutputMemberRecord.DateOfBirth_MaxLength, 1, 4) + "-" + Substring
          (local.OutputExtFcrOutputMemberRecord.DateOfBirth,
          FcrOutputMemberRecord.DateOfBirth_MaxLength, 5, 2) + "-"
          +
          Substring(local.OutputExtFcrOutputMemberRecord.DateOfBirth,
          FcrOutputMemberRecord.DateOfBirth_MaxLength, 7, 2));
        local.FcrMasterMemberRecord.DateOfDeath = new DateTime(1, 1, 1);
        local.FcrMasterMemberRecord.LastReceivedDtFromFcr =
          new DateTime(1, 1, 1);
        local.FcrMasterMemberRecord.LastSentDtToFcr = local.ProcessingDate.Date;
      }

      // ****************************************************************************************
      // Call the FCR record create/update action block to create Case Master/
      // Case Member records
      // ****************************************************************************************
      UseOeB417CreateNUpdateFcrMast();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        // **************************************************************************************
        // The following condition will not happen, if it happend we don't want 
        // the program abort
        // instead we will report this record in the error report and continue 
        // our FCR Case and
        // Member load process. After reporting reset the exit state to 
        // Processing OK.
        // **************************************************************************************
        if (IsExitState("CASE_NF"))
        {
          local.EabReportSend.RptDetail = local.FcrMasterMemberRecord.CaseId + "    " +
            local.FcrMasterMemberRecord.MemberId + "    " + local
            .FcrMasterMemberRecord.ActionTypeCode + "  *******  FCR Case Not found in FCR Case Master while adding the Part Record *******";
            
          local.EabFileHandling.Action = "WRITE";
          UseCabErrorReport1();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          ExitState = "ACO_NN0000_ALL_OK";

          continue;
        }

        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        break;
      }

      ++local.Commit.Count;
    }
    while(!Equal(local.Eab.TextReturnCode, "EF"));

    // **************************************************************************************
    // The program reached the end process either Normal or abnormal based on 
    // the termination
    // process will call the appropritate action blocks and write error message 
    // to sysout.
    // **************************************************************************************
    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      // **************************************************************************************
      // The program encountered an exception, the message needs to retrieved 
      // from exit state
      // and same needs to be printed in sysout for reference.
      // **************************************************************************************
      UseEabExtractExitStateMessage();
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = local.ExitStateWorkArea.Message;
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ALL_OK";
      UseOeB419Close();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
    else
    {
      UseOeB419Close();

      // **************************************************************************************
      // The processing completed successfully, time to update the program 
      // checkpoint restart
      // record for program SWEEB417 with NO restart information.
      // **************************************************************************************
      local.ProgramCheckpointRestart.ProgramName = global.UserId;
      local.ProgramCheckpointRestart.RestartInfo = "";
      local.ProgramCheckpointRestart.RestartInd = "N";
      local.ProgramCheckpointRestart.CheckpointCount = 0;
      UseUpdatePgmCheckpointRestart();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        UseEabExtractExitStateMessage();
        local.NeededToWrite.RptDetail =
          "Successful End of job, but error in update checkpoint restart.  Exitstate msg is: " +
          local.ExitStateWorkArea.Message;
        UseCabErrorReport2();
        ExitState = "ACO_AE0000_BATCH_ABEND";
      }
    }
  }

  private static void MoveExternal(External source, External target)
  {
    target.FileInstruction = source.FileInstruction;
    target.NumericReturnCode = source.NumericReturnCode;
    target.TextReturnCode = source.TextReturnCode;
    target.TextLine80 = source.TextLine80;
    target.TextLine8 = source.TextLine8;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

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

    useExport.External.NumericReturnCode = local.Eab.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.Eab.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseOeB417CreateNUpdateFcrMast()
  {
    var useImport = new OeB417CreateNUpdateFcrMast.Import();
    var useExport = new OeB417CreateNUpdateFcrMast.Export();

    useImport.FcrMasterCaseRecord.Assign(local.FcrMasterCaseRecord);
    useImport.FcrMasterMemberRecord.Assign(local.FcrMasterMemberRecord);
    useImport.Commit.Count = local.Commit.Count;
    useImport.TotalCreCount.Count = local.TotalCreCount.Count;
    useImport.TotalUpdCount.Count = local.TotalUpdCount.Count;
    useImport.PersonsCreCount.Count = local.PersonsCreCount.Count;
    useImport.CaseCreCount.Count = local.CaseCreCount.Count;
    useImport.PersonUpdCount.Count = local.PersonUpdCount.Count;
    useImport.CaseUpdCount.Count = local.CaseUpdCount.Count;
    useImport.TotalSkipCount.Count = local.TotalSkipCount.Count;
    useImport.TotalDelCount.Count = local.TotalDelCount.Count;
    useImport.CaseSkipCount.Count = local.CaseSkipCount.Count;
    useImport.CaseDelCount.Count = local.CaseDelCount.Count;
    useImport.PersonSkipCount.Count = local.PersonSkipCount.Count;
    useImport.PersonDelCount.Count = local.PersonDelCount.Count;
    useImport.ExtractNResponseFlag.Flag = local.ExtractNResponseFlag.Flag;

    Call(OeB417CreateNUpdateFcrMast.Execute, useImport, useExport);

    local.Commit.Count = useExport.Commit.Count;
    local.TotalCreCount.Count = useExport.TotalCreCount.Count;
    local.TotalUpdCount.Count = useExport.TotalUpdCount.Count;
    local.PersonsCreCount.Count = useExport.PersonsCreCount.Count;
    local.CaseCreCount.Count = useExport.CaseCreCount.Count;
    local.PersonUpdCount.Count = useExport.PersonUpdCount.Count;
    local.CaseUpdCount.Count = useExport.CaseUpdCount.Count;
    local.TotalSkipCount.Count = useExport.TotalSkipCount.Count;
    local.TotalDelCount.Count = useExport.TotalDelCount.Count;
    local.CaseSkipCount.Count = useExport.CaseSkipCount.Count;
    local.CaseDelCount.Count = useExport.CaseDelCount.Count;
    local.PersonSkipCount.Count = useExport.PersonSkipCount.Count;
    local.PersonDelCount.Count = useExport.PersonDelCount.Count;
  }

  private void UseOeB419Close()
  {
    var useImport = new OeB419Close.Import();
    var useExport = new OeB419Close.Export();

    useImport.ProcessingDate.Date = local.ProcessingDate.Date;
    useImport.TotalFcrMasterCount.Count = local.FcrMasterReadCount.Count;
    useImport.TotalCreCount.Count = local.TotalCreCount.Count;
    useImport.TotalUpdCount.Count = local.TotalUpdCount.Count;
    useImport.CaseCreCount.Count = local.CaseCreCount.Count;
    useImport.CaseUpdCount.Count = local.CaseUpdCount.Count;
    useImport.PersonsCreCount.Count = local.PersonsCreCount.Count;
    useImport.PersonsUpdCount.Count = local.PersonUpdCount.Count;
    useImport.TotalDelCount.Count = local.TotalDelCount.Count;
    useImport.CaseDelCount.Count = local.CaseDelCount.Count;
    useImport.PersonsDelCount.Count = local.PersonDelCount.Count;
    useImport.TotalSkipCount.Count = local.TotalSkipCount.Count;
    useImport.CaseSkipCount.Count = local.CaseSkipCount.Count;
    useImport.PersonsSkipCount.Count = local.PersonSkipCount.Count;

    Call(OeB419Close.Execute, useImport, useExport);
  }

  private void UseOeB419Housekeeping()
  {
    var useImport = new OeB419Housekeeping.Import();
    var useExport = new OeB419Housekeeping.Export();

    Call(OeB419Housekeeping.Execute, useImport, useExport);

    local.ProcessingDate.Date = useExport.ProcessingDate.Date;
    local.CurrentDateLessFive.Date = useExport.CurrentDateLessFive.Date;
    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }

  private void UseOeEabReadFcrExtractRecord()
  {
    var useImport = new OeEabReadFcrExtractRecord.Import();
    var useExport = new OeEabReadFcrExtractRecord.Export();

    MoveExternal(local.Eab, useImport.ExternalFileStatus);
    MoveExternal(local.Eab, useExport.ExternalFileStatus);
    useExport.Ext.Assign(local.OutputExtFcrOutputCaseRecord);
    useExport.FcrOutputExt.Assign(local.OutputExtFcrOutputMemberRecord);

    Call(OeEabReadFcrExtractRecord.Execute, useImport, useExport);

    local.Eab.Assign(useExport.ExternalFileStatus);
    local.OutputExtFcrOutputCaseRecord.Assign(useExport.Ext);
    local.OutputExtFcrOutputMemberRecord.Assign(useExport.FcrOutputExt);
  }

  private void UseUpdatePgmCheckpointRestart()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
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
    /// <summary>
    /// A value of OutputExtFcrOutputMemberRecord.
    /// </summary>
    [JsonPropertyName("outputExtFcrOutputMemberRecord")]
    public FcrOutputMemberRecord OutputExtFcrOutputMemberRecord
    {
      get => outputExtFcrOutputMemberRecord ??= new();
      set => outputExtFcrOutputMemberRecord = value;
    }

    /// <summary>
    /// A value of OutputExtFcrOutputCaseRecord.
    /// </summary>
    [JsonPropertyName("outputExtFcrOutputCaseRecord")]
    public FcrOutputCaseRecord OutputExtFcrOutputCaseRecord
    {
      get => outputExtFcrOutputCaseRecord ??= new();
      set => outputExtFcrOutputCaseRecord = value;
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
    /// A value of NullFcrMasterCaseRecord.
    /// </summary>
    [JsonPropertyName("nullFcrMasterCaseRecord")]
    public FcrMasterCaseRecord NullFcrMasterCaseRecord
    {
      get => nullFcrMasterCaseRecord ??= new();
      set => nullFcrMasterCaseRecord = value;
    }

    /// <summary>
    /// A value of NullFcrMasterMemberRecord.
    /// </summary>
    [JsonPropertyName("nullFcrMasterMemberRecord")]
    public FcrMasterMemberRecord NullFcrMasterMemberRecord
    {
      get => nullFcrMasterMemberRecord ??= new();
      set => nullFcrMasterMemberRecord = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
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
    /// A value of CurrentDateLessFive.
    /// </summary>
    [JsonPropertyName("currentDateLessFive")]
    public DateWorkArea CurrentDateLessFive
    {
      get => currentDateLessFive ??= new();
      set => currentDateLessFive = value;
    }

    /// <summary>
    /// A value of Eab.
    /// </summary>
    [JsonPropertyName("eab")]
    public External Eab
    {
      get => eab ??= new();
      set => eab = value;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    /// <summary>
    /// A value of RestartCaseId.
    /// </summary>
    [JsonPropertyName("restartCaseId")]
    public FcrCaseMaster RestartCaseId
    {
      get => restartCaseId ??= new();
      set => restartCaseId = value;
    }

    /// <summary>
    /// A value of PreviousFcrCaseId.
    /// </summary>
    [JsonPropertyName("previousFcrCaseId")]
    public FcrCaseMaster PreviousFcrCaseId
    {
      get => previousFcrCaseId ??= new();
      set => previousFcrCaseId = value;
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
    /// A value of FcrMasterReadCount.
    /// </summary>
    [JsonPropertyName("fcrMasterReadCount")]
    public Common FcrMasterReadCount
    {
      get => fcrMasterReadCount ??= new();
      set => fcrMasterReadCount = value;
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
    /// A value of PersonsCreCount.
    /// </summary>
    [JsonPropertyName("personsCreCount")]
    public Common PersonsCreCount
    {
      get => personsCreCount ??= new();
      set => personsCreCount = value;
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
    /// A value of PersonUpdCount.
    /// </summary>
    [JsonPropertyName("personUpdCount")]
    public Common PersonUpdCount
    {
      get => personUpdCount ??= new();
      set => personUpdCount = value;
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
    /// A value of TotalSkipCount.
    /// </summary>
    [JsonPropertyName("totalSkipCount")]
    public Common TotalSkipCount
    {
      get => totalSkipCount ??= new();
      set => totalSkipCount = value;
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
    /// A value of CaseSkipCount.
    /// </summary>
    [JsonPropertyName("caseSkipCount")]
    public Common CaseSkipCount
    {
      get => caseSkipCount ??= new();
      set => caseSkipCount = value;
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
    /// A value of PersonSkipCount.
    /// </summary>
    [JsonPropertyName("personSkipCount")]
    public Common PersonSkipCount
    {
      get => personSkipCount ??= new();
      set => personSkipCount = value;
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
    /// A value of ExtractNResponseFlag.
    /// </summary>
    [JsonPropertyName("extractNResponseFlag")]
    public Common ExtractNResponseFlag
    {
      get => extractNResponseFlag ??= new();
      set => extractNResponseFlag = value;
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
    /// A value of ProcessingDate.
    /// </summary>
    [JsonPropertyName("processingDate")]
    public DateWorkArea ProcessingDate
    {
      get => processingDate ??= new();
      set => processingDate = value;
    }

    private FcrOutputMemberRecord outputExtFcrOutputMemberRecord;
    private FcrOutputCaseRecord outputExtFcrOutputCaseRecord;
    private FcrCaseMaster fcrCaseMaster;
    private FcrMasterCaseRecord fcrMasterCaseRecord;
    private FcrMasterMemberRecord fcrMasterMemberRecord;
    private FcrMasterCaseRecord nullFcrMasterCaseRecord;
    private FcrMasterMemberRecord nullFcrMasterMemberRecord;
    private EabFileHandling eabFileHandling;
    private ProgramProcessingInfo programProcessingInfo;
    private EabReportSend eabReportSend;
    private DateWorkArea currentDateLessFive;
    private External eab;
    private Common commit;
    private ProgramCheckpointRestart programCheckpointRestart;
    private FcrCaseMaster restartCaseId;
    private FcrCaseMaster previousFcrCaseId;
    private EabReportSend neededToWrite;
    private Common fcrMasterReadCount;
    private Common totalCreCount;
    private Common totalUpdCount;
    private Common personsCreCount;
    private Common caseCreCount;
    private Common personUpdCount;
    private Common caseUpdCount;
    private Common totalSkipCount;
    private Common totalDelCount;
    private Common caseSkipCount;
    private Common caseDelCount;
    private Common personSkipCount;
    private Common personDelCount;
    private Common extractNResponseFlag;
    private ExitStateWorkArea exitStateWorkArea;
    private DateWorkArea processingDate;
  }
#endregion
}
