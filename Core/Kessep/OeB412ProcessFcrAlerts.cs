// Program: OE_B412_PROCESS_FCR_ALERTS, ID: 373542210, model: 746.
// Short name: SWEE412B
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B412_PROCESS_FCR_ALERTS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class OeB412ProcessFcrAlerts: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B412_PROCESS_FCR_ALERTS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB412ProcessFcrAlerts(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB412ProcessFcrAlerts.
  /// </summary>
  public OeB412ProcessFcrAlerts(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***********************************************************************************
    // When a case is added or removed from the Federal Case Registry (FCR) an 
    // event is
    // created but no alert is needed.
    // When a person being added or changed on the FCR and the FCR finds errors 
    // with the
    // associated information, error codes are sent back with the record 
    // acknowledgment.
    // If the error is one that the worker can fix, the error code will appear 
    // in the
    // code table.  The worker is alerted.
    // When the FCR checks with the SSA, sometimes they find or correct the 
    // person's SSN
    // or date of birth/death.  If this information is supplied, the worker is 
    // alerted.
    // ***********************************************************************************
    // *************************************************************************************
    // Maintenance Log:
    //    Date     Request  Name     Description
    // ---------   -------  ----     
    // -------------------------------------------------------
    // 04/01/2000   new     Sree     Original.
    // 08/15/2001  WR10371  Ed       Added code table and rewrote.
    // 10/15/2001  WR20132  Ed       Added new fields in EAB and logic for date 
    // of death.
    // 03/26/2004  PR200614 GVandy   Correct extraction of case numbers.
    // 11/08/2005  WR258947 DDupree  Added  code so that the new additional
    //                               
    // ssn validation codes could be
    // processed.
    // 03/16/2009  CQ114    Raj S    Modified to update ADABAS with SSN received
    // from FCR
    //                               
    // If the person belong to CSE,
    // otherwise generate alerts
    //                               
    // To worker to update manually
    // after verification.
    //                               
    // Whenever CSE receives
    // different SSNs from FCR, the
    // same
    //                               
    // Will be alerted to the worker.
    // *************************************************************************************
    // 06/05/2009   DDupree     Added check when processing the returning ssn to
    // see
    //  if it is a invalid ssn and person number combination. Part of CQ7189.
    // __________________________________________________________________________________
    // 07/26/2010  CQ20705  RMathews  Added call to ADABAS close routine
    // __________________________________________________________________________________
    ExitState = "ACO_NN0000_ALL_OK";
    local.Max.Date = new DateTime(2099, 12, 31);
    UseOeB412Housekeeping();
    local.CurrentDate.Date = local.ProgramProcessingInfo.ProcessDate;

    do
    {
      local.Eab.FileInstruction = "READ";
      UseOeEabReadFcrAckRecords();

      if (Equal(local.Eab.TextReturnCode, "EF"))
      {
        break;
      }

      if (!IsEmpty(local.Eab.TextReturnCode))
      {
        ExitState = "FILE_READ_ERROR_WITH_RB";

        break;
      }

      ++local.RecordsRead.Count;

      // ******************************************************************
      // Skip all the Non IV-D cases acknowledgement and errors.
      // ******************************************************************
      if (Equal(local.FcrCaseAckErrorRecord.CaseId, 1, 3, "KPC") || Equal
        (local.FcrPersonAckErrorRecord.CaseId, 1, 3, "KPC"))
      {
        ++local.KpcRecordsSkipped.Count;

        continue;
      }

      // ******************************************************************
      // *   Skip records other than Acknowledgments/Errors records       *
      // *        FD =  Case 
      // acknowledgment
      // 
      // *
      // *        FS =  Person acknowledgment                             *
      // ******************************************************************
      if (!Equal(local.FcrCaseAckErrorRecord.RecordIdentifier, "FD") && !
        Equal(local.FcrPersonAckErrorRecord.RecordIdentifier, "FS"))
      {
        ++local.RecordsSkipped.Count;

        continue;
      }

      if (Equal(local.FcrCaseAckErrorRecord.RecordIdentifier, "FD"))
      {
        ++local.AcksReceived.Count;

        if (Equal(local.FcrCaseAckErrorRecord.AcknowledgementCode, "AAAAA"))
        {
          switch(AsChar(local.FcrCaseAckErrorRecord.ActionTypeCode))
          {
            case 'A':
              local.Infrastructure.Detail =
                "FEDERAL CASE REGISTRY : CASE ADDED" + "";

              break;
            case 'C':
              continue;
            case 'D':
              local.Infrastructure.Detail =
                "FEDERAL CASE REGISTRY : CASE REMOVED" + "";

              break;
            default:
              break;
          }

          local.Infrastructure.CaseNumber =
            Substring(local.FcrCaseAckErrorRecord.CaseId, 1, 10);
          local.Infrastructure.CsePersonNumber = "";
          local.Infrastructure.ReasonCode = "FCRACKRECEIVED";
          local.Infrastructure.ProcessStatus = "H";
          UseOeB412CreateInfrastructure1();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            break;
          }

          ++local.Commit.Count;
        }
        else
        {
        }

        continue;
      }

      if (Equal(local.FcrPersonAckErrorRecord.RecordIdentifier, "FS"))
      {
        ++local.AcksReceived.Count;
        local.CsePersonsWorkSet.Number =
          Substring(local.FcrPersonAckErrorRecord.MemberId, 6, 10);
        local.ReadSsn.SsnNum9 =
          (int)StringToNumber(local.FcrPersonAckErrorRecord.
            ProvidedOrCorrectedSsn);

        // **********************************************************
        // 07/29/2009   DDupree   added this check as part of cq7189.
        // **********************************************************
        if (ReadInvalidSsn())
        {
          local.ConvertMessage.SsnTextPart1 =
            Substring(local.FcrPersonAckErrorRecord.ProvidedOrCorrectedSsn, 1, 3);
            
          local.ConvertMessage.SsnTextPart2 =
            Substring(local.FcrPersonAckErrorRecord.ProvidedOrCorrectedSsn, 4, 2);
            
          local.ConvertMessage.SsnTextPart3 =
            Substring(local.FcrPersonAckErrorRecord.ProvidedOrCorrectedSsn, 6, 4);
            
          local.Message2.Text11 = local.ConvertMessage.SsnTextPart1 + "-" + local
            .ConvertMessage.SsnTextPart2 + "-" + local
            .ConvertMessage.SsnTextPart3;
          local.Message1.Text8 = "Bad SSN";
          local.Message1.Text6 = ", Per";
          local.Message1.Text33 = ": Did not process incoming record";
          local.EabReportSend.RptDetail = local.Message1.Text8 + local
            .Message2.Text11 + local.Message1.Text6 + local
            .CsePersonsWorkSet.Number + local.Message1.Text33;
          local.EabFileHandling.Action = "WRITE";
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          ExitState = "ACO_NN0000_ALL_OK";
          local.Message1.Text8 = "";
          local.Message1.Text6 = "";
          local.Message1.Text33 = "";
          local.EabReportSend.RptDetail = "";

          continue;
        }
        else
        {
          // this is fine, there is not invalid ssn record for this combination 
          // of cse person number and ssn number
        }

        if (!IsEmpty(local.FcrPersonAckErrorRecord.ErrorCode1))
        {
          // ***********************************************************************
          // Children do not currently have aliases, so do not report errors 
          // until
          // this functionality is added to CAECSES sometime in the future.
          // ***********************************************************************
          if (Equal(local.FcrPersonAckErrorRecord.ParticipantType, "CH"))
          {
            goto Test1;
          }

          // *****************************************************************
          // An alert should be created only if the error code is in the
          // code value table.
          // *****************************************************************
          local.Sub.Subscript = 0;

          do
          {
            ++local.Sub.Subscript;

            switch(local.Sub.Subscript)
            {
              case 1:
                if (!IsEmpty(local.FcrPersonAckErrorRecord.ErrorCode1))
                {
                  local.LookUp.Cdvalue =
                    local.FcrPersonAckErrorRecord.ErrorCode1;
                }
                else
                {
                  goto AfterCycle1;
                }

                break;
              case 2:
                if (!IsEmpty(local.FcrPersonAckErrorRecord.ErrorCode2))
                {
                  local.LookUp.Cdvalue =
                    local.FcrPersonAckErrorRecord.ErrorCode2;
                }
                else
                {
                  goto AfterCycle1;
                }

                break;
              case 3:
                if (!IsEmpty(local.FcrPersonAckErrorRecord.ErrorCode3))
                {
                  local.LookUp.Cdvalue =
                    local.FcrPersonAckErrorRecord.ErrorCode3;
                }
                else
                {
                  goto AfterCycle1;
                }

                break;
              case 4:
                if (!IsEmpty(local.FcrPersonAckErrorRecord.ErrorCode4))
                {
                  local.LookUp.Cdvalue =
                    local.FcrPersonAckErrorRecord.ErrorCode4;
                }
                else
                {
                  goto AfterCycle1;
                }

                break;
              case 5:
                if (!IsEmpty(local.FcrPersonAckErrorRecord.ErrorCode5))
                {
                  local.LookUp.Cdvalue =
                    local.FcrPersonAckErrorRecord.ErrorCode5;
                }
                else
                {
                  goto AfterCycle1;
                }

                break;
              default:
                break;
            }

            if (ReadCodeValue())
            {
              // *******************************************
              // ***	Create Alerts for FCR	        ***
              // *******************************************
              local.Infrastructure.CaseNumber =
                Substring(local.FcrPersonAckErrorRecord.CaseId, 1, 10);
              local.Infrastructure.CsePersonNumber =
                Substring(local.FcrPersonAckErrorRecord.MemberId, 6, 10);
              local.Infrastructure.ReasonCode = "FCRERROR";
              local.Infrastructure.Detail = entities.CodeValue.Description;
              local.Infrastructure.ProcessStatus = "Q";
              UseOeB412CreateInfrastructure2();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                goto AfterCycle2;
              }

              ++local.Commit.Count;
            }
          }
          while(local.Sub.Subscript < 5);

AfterCycle1:
          ;
        }

Test1:

        if (AsChar(local.FcrPersonAckErrorRecord.SsaDateOfBirthIndicator) == 'Y'
          )
        {
          local.ConvertDateDateWorkArea.Date =
            local.FcrPersonAckErrorRecord.DateOfBirth;
          UseCabConvertDate2String();
          local.Infrastructure.Detail =
            "Corrected Date of Birth Received from FCR: " + local
            .ConvertDateTextWorkArea.Text8;
          local.Infrastructure.CaseNumber =
            Substring(local.FcrPersonAckErrorRecord.CaseId, 1, 10);
          local.Infrastructure.CsePersonNumber =
            Substring(local.FcrPersonAckErrorRecord.MemberId, 6, 10);
          local.Infrastructure.ReasonCode = "FPLSRCV";
          local.Infrastructure.ProcessStatus = "Q";
          UseOeB412CreateInfrastructure2();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            break;
          }

          ++local.Commit.Count;
        }

        // ***********************************************************************************
        // 04/01/2009
        // 
        // Raj S
        // CQ114-START
        // Skip person records with action type code 'D' (i.e. we have sent a 
        // delete request
        // To FCR to remove the person from FCR database).
        // ***********************************************************************************
        if (AsChar(local.FcrPersonAckErrorRecord.ActionTypeCode) == 'D')
        {
          continue;
        }

        // ***********************************************************************************
        // 04/01/2009                             Raj S
        // CQ114-END
        // ***********************************************************************************
        // ***********************************************************************************
        // 03/16/2009
        // 
        // Raj S
        // CQ114-START
        // The following new code are added to track the primary SSN change and 
        // FCR verifi-
        // Cation, based on the response the FCR required alerts or ADABAS 
        // person update will
        // Take place.
        // 1.  If the CSE Sent SSN is verified by FCR then history records will 
        // be generated.
        // 2.  FCR sends a Verified SSN for the blank SSN and the person NOT 
        // belongs to AE
        //     System then, the new SSN will be updated in ADABAS and History 
        // record will be
        //     Generated with the message "Received & Updated FCR Verified SSN".
        // 3.  FCR sends a verified SSN for the blank SSN and the person belongs
        // to AE system
        //     Then the program will create a Alert record to the worker stating
        // FCR sent a
        //     Verified SSN but protected by AE.
        // 4.  If FCR send a different SSN then program will generate a alert to
        // the worker
        //     Stating "FCR sent a different verified SSN".
        // ***********************************************************************************
        local.Ae.Flag = "";
        local.Kscares.Flag = "";
        UseEabReadCsePersonBatchAll();

        // ***********************************************************************************
        // *Interpret the error codes returned from ADABAS and set an 
        // appropriate exit state.*
        // ***********************************************************************************
        switch(AsChar(local.AbendData.Type1))
        {
          case ' ':
            break;
          case 'A':
            // ***********************************************************************************
            // * Unsuccessful ADABAS Read Occurred.
            // 
            // *
            // ***********************************************************************************
            switch(TrimEnd(local.AbendData.AdabasResponseCd))
            {
              case "":
                break;
              case "0113":
                ExitState = "ACO_ADABAS_PERSON_NF_113";
                local.EabReportSend.RptDetail =
                  Substring(local.FcrPersonAckErrorRecord.MemberId,
                  FcrPersonAckErrorRecord.MemberId_MaxLength, 6, 10) + "; Person not found in Adabas error 113 -- number = " +
                  local.AbendData.AdabasFileAction + local
                  .AbendData.AdabasFileNumber + local.AbendData.Type1 + "";

                break;
              case "0148":
                ExitState = "ACO_ADABAS_UNAVAILABLE";
                local.EabReportSend.RptDetail =
                  "Adabas unavailable fetching person number = " + local
                  .CsePersonsWorkSet.Number;

                break;
              default:
                ExitState = "ADABAS_READ_UNSUCCESSFUL";
                local.EabReportSend.RptDetail =
                  "Adabas read unsuccessful fetching person number = " + local
                  .CsePersonsWorkSet.Number;

                break;
            }

            break;
          case 'C':
            // ************************************************
            // *CICS action Failed. A reason code should be   *
            // *interpreted.
            // 
            // *
            // ************************************************
            if (IsEmpty(export.AbendData.CicsResponseCd))
            {
            }
            else
            {
              ExitState = "ACO_NE0000_CICS_UNAVAILABLE";
            }

            local.EabReportSend.RptDetail =
              "CICS error fetching person number = " + import.Obligor.Number;

            break;
          default:
            ExitState = "ADABAS_INVALID_RETURN_CODE";
            local.EabReportSend.RptDetail =
              "Unknown error fetching person number = " + import
              .Obligor.Number;

            break;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            break;
          }
        }

        if (IsExitState("ADABAS_READ_UNSUCCESSFUL"))
        {
          ExitState = "ACO_NN0000_ALL_OK";
        }
        else if (IsExitState("ACO_ADABAS_PERSON_NF_113"))
        {
          ExitState = "ACO_NN0000_ALL_OK";
        }
        else if (IsExitState("ACO_NE0000_CICS_UNAVAILABLE"))
        {
          ExitState = "ACO_NN0000_ALL_OK";
        }
        else if (IsExitState("ADABAS_INVALID_RETURN_CODE"))
        {
          ExitState = "ACO_NN0000_ALL_OK";
        }
        else if (IsExitState("ACO_ADABAS_UNAVAILABLE"))
        {
          break;
        }
        else if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          break;
        }

        // ***********************************************************************************
        // 03/16/2009
        // 
        // Raj S
        // 
        // CQ114-END
        // ***********************************************************************************
        if (AsChar(local.FcrPersonAckErrorRecord.SsnValidityCode) == 'C' || AsChar
          (local.FcrPersonAckErrorRecord.SsnValidityCode) == 'E' || AsChar
          (local.FcrPersonAckErrorRecord.SsnValidityCode) == 'P' || AsChar
          (local.FcrPersonAckErrorRecord.SsnValidityCode) == 'R' || AsChar
          (local.FcrPersonAckErrorRecord.SsnValidityCode) == 'S')
        {
          if (!IsEmpty(local.FcrPersonAckErrorRecord.ProvidedOrCorrectedSsn))
          {
            // ***********************************************************************************
            // 03/16/2009
            // 
            // Raj S
            // CQ114-START
            // As per Kenny (FCR Team), SSN verification code values 'N' and 'V'
            // are the same and
            // Both values should be treated as verified.
            // If the person records has above mentioned SSN verification codes 
            // then the SSN sent
            // By CSE has been verified by FCR, a history Record will be 
            // generated with the Reason
            // Code "FCRVERSSN".
            // ***********************************************************************************
            if (IsEmpty(local.CsePersonsWorkSet.Ssn) || Equal
              (local.CsePersonsWorkSet.Ssn, "000000000"))
            {
              if (AsChar(local.Ae.Flag) == 'O' || AsChar
                (local.Kscares.Flag) == 'O')
              {
                // ***********************************************************************************
                // * FCR sent back verified SSN but the person selected belongs 
                // to AE system, system *
                // * cannot update ADABAS with FCR SSN.   The process will 
                // create a alert to the     *
                // * worker stating verified SSN received from FCR with a reason
                // code "FCRAESSNCH"   *
                // ***********************************************************************************
                local.Infrastructure.ReasonCode = "FCRAESSNCH";
                local.Infrastructure.ProcessStatus = "Q";
                local.Infrastructure.Detail =
                  "Contact AE to add verified SSN " + Substring
                  (local.FcrPersonAckErrorRecord.ProvidedOrCorrectedSsn,
                  FcrPersonAckErrorRecord.ProvidedOrCorrectedSsn_MaxLength, 1,
                  3) + "-" + Substring
                  (local.FcrPersonAckErrorRecord.ProvidedOrCorrectedSsn,
                  FcrPersonAckErrorRecord.ProvidedOrCorrectedSsn_MaxLength, 4,
                  2) + "-" + Substring
                  (local.FcrPersonAckErrorRecord.ProvidedOrCorrectedSsn,
                  FcrPersonAckErrorRecord.ProvidedOrCorrectedSsn_MaxLength, 6,
                  4);
              }
              else
              {
                // ***********************************************************************************
                // * FCR sent back verified SSN and CSE SSN is blank and person 
                // belongs to CSE System*
                // * and process will update the ADABAS with FCR SSN value and 
                // create a history      *
                // * record with a reason code "FCRBLKSSNCH"
                // 
                // *
                // ***********************************************************************************
                local.CsePersonsWorkSet.Ssn =
                  local.FcrPersonAckErrorRecord.ProvidedOrCorrectedSsn;
                local.CsePersonsWorkSet.Number =
                  Substring(local.FcrPersonAckErrorRecord.MemberId, 6, 10);
                UseCabUpdateAdabasPersonSsnBat();

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  // ***********************************************************************************
                  // * The process successfully updated the verified SSN sent by
                  // FCR with ADABAS, now  *
                  // * the process will create a history record stating FCR sent
                  // verified SSN and will *
                  // * not generate worker alert.
                  // 
                  // *
                  // ***********************************************************************************
                  local.Infrastructure.ReasonCode = "FCRVERSSN";
                  local.Infrastructure.Detail = "SSN:" + Substring
                    (local.FcrPersonAckErrorRecord.ProvidedOrCorrectedSsn,
                    FcrPersonAckErrorRecord.ProvidedOrCorrectedSsn_MaxLength, 1,
                    3) + "-" + Substring
                    (local.FcrPersonAckErrorRecord.ProvidedOrCorrectedSsn,
                    FcrPersonAckErrorRecord.ProvidedOrCorrectedSsn_MaxLength, 4,
                    2) + "-" + Substring
                    (local.FcrPersonAckErrorRecord.ProvidedOrCorrectedSsn,
                    FcrPersonAckErrorRecord.ProvidedOrCorrectedSsn_MaxLength, 6,
                    4);
                  local.Infrastructure.Detail =
                    TrimEnd(local.Infrastructure.Detail) + " was verified at the FCR ";
                    
                  local.Infrastructure.ProcessStatus = "H";
                }
                else
                {
                  // ***********************************************************************************
                  // * The process tried to update the ADABAS but due to various
                  // reasons it could not  *
                  // * update the ADABAS.  Alter the worker about the new 
                  // verified SSN received from   *
                  // * and update manually to the AE system.  This kind of 
                  // scenario will be very rare  *
                  // * in production. As a pre-caution system will handle this 
                  // situation proactively to*
                  // * alert the worker.
                  // 
                  // *
                  // ***********************************************************************************
                  UseEabExtractExitStateMessage();
                  local.EabReportSend.RptDetail =
                    TrimEnd(local.ExitStateWorkArea.Message) + ";  Type:" + local
                    .AbendData.Type1 + "; File#:" + local
                    .AbendData.AdabasFileNumber + "; Action:" + local
                    .AbendData.AdabasFileAction + "; Resp:" + local
                    .AbendData.AdabasResponseCd + "; Person#:" + Substring
                    (local.FcrPersonAckErrorRecord.MemberId,
                    FcrPersonAckErrorRecord.MemberId_MaxLength, 6, 10);
                  local.EabFileHandling.Action = "WRITE";
                  UseCabErrorReport();

                  if (!Equal(local.EabFileHandling.Status, "OK"))
                  {
                    ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                    return;
                  }

                  ExitState = "ACO_NN0000_ALL_OK";
                  local.Infrastructure.ReasonCode = "FCRAESSNCH";
                  local.Infrastructure.ProcessStatus = "Q";
                  local.Infrastructure.Detail =
                    "Contact AE to add verified SSN " + Substring
                    (local.FcrPersonAckErrorRecord.ProvidedOrCorrectedSsn,
                    FcrPersonAckErrorRecord.ProvidedOrCorrectedSsn_MaxLength, 1,
                    3) + "-" + Substring
                    (local.FcrPersonAckErrorRecord.ProvidedOrCorrectedSsn,
                    FcrPersonAckErrorRecord.ProvidedOrCorrectedSsn_MaxLength, 4,
                    2) + "-" + Substring
                    (local.FcrPersonAckErrorRecord.ProvidedOrCorrectedSsn,
                    FcrPersonAckErrorRecord.ProvidedOrCorrectedSsn_MaxLength, 6,
                    4);
                }
              }
            }
            else
            {
              // ***********************************************************************************
              // * FCR sent back verified SSN but the CSE person SSN is not 
              // blank or zeros, this   *
              // * process cannot update ADABAS with the SSN returned by the 
              // FCR. The process will *
              // * alert worker stating SSN received from FCR with a reason code
              // "FCRSSNCHDR"      *
              // ***********************************************************************************
              local.Infrastructure.ReasonCode = "FCRSSNCHDR";
              local.Infrastructure.ProcessStatus = "Q";
              local.Infrastructure.Detail = "SSN " + Substring
                (local.CsePersonsWorkSet.Ssn, CsePersonsWorkSet.Ssn_MaxLength,
                1, 3) + "-" + Substring
                (local.CsePersonsWorkSet.Ssn, CsePersonsWorkSet.Ssn_MaxLength,
                4, 2) + "-" + Substring
                (local.CsePersonsWorkSet.Ssn, CsePersonsWorkSet.Ssn_MaxLength,
                6, 4);
              local.Infrastructure.Detail =
                (local.Infrastructure.Detail ?? "") + " but FCR verified SSN as ";
                
              local.Infrastructure.Detail =
                (local.Infrastructure.Detail ?? "") + Substring
                (local.FcrPersonAckErrorRecord.ProvidedOrCorrectedSsn,
                FcrPersonAckErrorRecord.ProvidedOrCorrectedSsn_MaxLength, 1,
                3) + "-" + Substring
                (local.FcrPersonAckErrorRecord.ProvidedOrCorrectedSsn,
                FcrPersonAckErrorRecord.ProvidedOrCorrectedSsn_MaxLength, 4,
                2) + "-" + Substring
                (local.FcrPersonAckErrorRecord.ProvidedOrCorrectedSsn,
                FcrPersonAckErrorRecord.ProvidedOrCorrectedSsn_MaxLength, 6, 4);
                
            }

            local.Infrastructure.CaseNumber =
              Substring(local.FcrPersonAckErrorRecord.CaseId, 1, 10);
            local.Infrastructure.CsePersonNumber =
              Substring(local.FcrPersonAckErrorRecord.MemberId, 6, 10);
            UseOeB412CreateInfrastructure2();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              break;
            }

            ++local.Commit.Count;
          }
        }
        else
        {
          // ***********************************************************************************
          // 03/16/2009
          // 
          // Raj S
          // CQ114-START
          // As per Kenny (FCR Team), SSN verification code values 'N' and 'V' 
          // are the same and
          // Both values should be treated as verified.
          // If the person records has above mentioned SSN verification codes 
          // then the SSN sent
          // By CSE has been verified by FCR, a history Record will be generated
          // with the Reason
          // Code "FCRVERSSN".
          // ***********************************************************************************
          if (AsChar(local.FcrPersonAckErrorRecord.SsnValidityCode) == 'N' || AsChar
            (local.FcrPersonAckErrorRecord.SsnValidityCode) == 'V')
          {
            local.Infrastructure.Detail = "SSN:" + Substring
              (local.FcrPersonAckErrorRecord.FcrPrimarySsn,
              FcrPersonAckErrorRecord.FcrPrimarySsn_MaxLength, 1, 3) + "-" + Substring
              (local.FcrPersonAckErrorRecord.FcrPrimarySsn,
              FcrPersonAckErrorRecord.FcrPrimarySsn_MaxLength, 4, 2) + "-" + Substring
              (local.FcrPersonAckErrorRecord.FcrPrimarySsn,
              FcrPersonAckErrorRecord.FcrPrimarySsn_MaxLength, 6, 4);
            local.Infrastructure.Detail =
              TrimEnd(local.Infrastructure.Detail) + " was verified at the FCR";
              
            local.Infrastructure.CaseNumber =
              Substring(local.FcrPersonAckErrorRecord.CaseId, 1, 10);
            local.Infrastructure.CsePersonNumber =
              Substring(local.FcrPersonAckErrorRecord.MemberId, 6, 10);
            local.Infrastructure.ReasonCode = "FCRVERSSN";
            local.Infrastructure.ProcessStatus = "H";
            UseOeB412CreateInfrastructure2();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              break;
            }

            ++local.Commit.Count;
          }
        }

        // ***********************************************************************************************
        // 11/08/2005               DDupree               WR00258947
        // Added  the following 'if multiple ssn# = additional ssn#' statements 
        // so that the
        // correct message could be attached the the ssn in question. FCR moves 
        // the
        // additional ssn that we sent  in, into one of the multiple ssn# 
        // fields. Also in the
        // multiple ssn fields are ssn's that fcr has obtain from other sources 
        // and is sending
        // it to us. So that we can write out the correct message we will 
        // compare the ssn
        // we sent in with the ssn in the multiple ssn fields. If the ssn is a 
        // ssn that we went
        // in earlier then we will wait and write a message later in the program
        // stating if ssa
        // has validated it. If it is a new additional ssn then we will write 
        // out a message
        // saying that it is a new ssn from fcr.
        // ***********************************************************************************************
        if (!IsEmpty(local.FcrPersonAckErrorRecord.MultipleSsn1))
        {
          local.CsePersonsWorkSet.Number =
            Substring(local.FcrPersonAckErrorRecord.MemberId, 6, 10);
          local.ReadSsn.SsnNum9 =
            (int)StringToNumber(local.FcrPersonAckErrorRecord.MultipleSsn1);

          // **********************************************************
          // 07/29/2009   DDupree   added this check as part of cq7189.
          // **********************************************************
          if (ReadInvalidSsn())
          {
            local.ConvertMessage.SsnTextPart1 =
              Substring(local.FcrPersonAckErrorRecord.MultipleSsn1, 1, 3);
            local.ConvertMessage.SsnTextPart2 =
              Substring(local.FcrPersonAckErrorRecord.MultipleSsn1, 4, 2);
            local.ConvertMessage.SsnTextPart3 =
              Substring(local.FcrPersonAckErrorRecord.MultipleSsn1, 6, 4);
            local.Message2.Text11 = local.ConvertMessage.SsnTextPart1 + "-" + local
              .ConvertMessage.SsnTextPart2 + "-" + local
              .ConvertMessage.SsnTextPart3;
            local.Message1.Text8 = "Bad SSN";
            local.Message1.Text6 = ", Per";
            local.Message1.Text22 = ": Did not process the";
            local.Message1.Text9 = "incoming";
            local.Message1.Text16 = "multiple ssn 1";
            local.Message2.Text6 = " field";
            local.EabReportSend.RptDetail = local.Message1.Text8 + local
              .Message2.Text11 + local.Message1.Text6 + local
              .CsePersonsWorkSet.Number + local.Message1.Text22 + local
              .Message1.Text9 + local.Message1.Text16 + local.Message2.Text6;
            local.EabFileHandling.Action = "WRITE";
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            ExitState = "ACO_NN0000_ALL_OK";
            local.Message1.Text22 = "";
            local.Message1.Text9 = "";
            local.Message1.Text16 = "";
            local.Message2.Text6 = "";
            local.Message2.Text11 = "";
            local.Message1.Text8 = "";
            local.Message1.Text6 = "";
            local.EabReportSend.RptDetail = "";

            goto Test2;
          }
          else
          {
            // this is fine, there is not invalid ssn record for this 
            // combination of cse person number and ssn number
          }

          if (Equal(local.FcrPersonAckErrorRecord.MultipleSsn1,
            local.FcrPersonAckErrorRecord.AdditionalSsn1) || Equal
            (local.FcrPersonAckErrorRecord.MultipleSsn1,
            local.FcrPersonAckErrorRecord.AdditionalSsn2))
          {
          }
          else
          {
            local.Infrastructure.Detail =
              "Additional SSN Received from FCR: " + Substring
              (local.FcrPersonAckErrorRecord.MultipleSsn1,
              FcrPersonAckErrorRecord.MultipleSsn1_MaxLength, 1, 3) + "-" + Substring
              (local.FcrPersonAckErrorRecord.MultipleSsn1,
              FcrPersonAckErrorRecord.MultipleSsn1_MaxLength, 4, 2) + "-" + Substring
              (local.FcrPersonAckErrorRecord.MultipleSsn1,
              FcrPersonAckErrorRecord.MultipleSsn1_MaxLength, 6, 4);
            local.Infrastructure.CaseNumber =
              Substring(local.FcrPersonAckErrorRecord.CaseId, 1, 10);
            local.Infrastructure.CsePersonNumber =
              Substring(local.FcrPersonAckErrorRecord.MemberId, 6, 10);
            local.Infrastructure.ReasonCode = "ALTSSNFCR";
            local.Infrastructure.ProcessStatus = "Q";
            UseOeB412CreateInfrastructure2();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              break;
            }

            ++local.Commit.Count;
          }
        }

Test2:

        if (!IsEmpty(local.FcrPersonAckErrorRecord.MultipleSsn2))
        {
          local.CsePersonsWorkSet.Number =
            Substring(local.FcrPersonAckErrorRecord.MemberId, 6, 10);
          local.ReadSsn.SsnNum9 =
            (int)StringToNumber(local.FcrPersonAckErrorRecord.MultipleSsn2);

          // **********************************************************
          // 07/29/2009   DDupree   added this check as part of cq7189.
          // **********************************************************
          if (ReadInvalidSsn())
          {
            local.ConvertMessage.SsnTextPart1 =
              Substring(local.FcrPersonAckErrorRecord.MultipleSsn2, 1, 3);
            local.ConvertMessage.SsnTextPart2 =
              Substring(local.FcrPersonAckErrorRecord.MultipleSsn2, 4, 2);
            local.ConvertMessage.SsnTextPart3 =
              Substring(local.FcrPersonAckErrorRecord.MultipleSsn2, 6, 4);
            local.Message2.Text11 = local.ConvertMessage.SsnTextPart1 + "-" + local
              .ConvertMessage.SsnTextPart2 + "-" + local
              .ConvertMessage.SsnTextPart3;
            local.Message1.Text8 = "Bad SSN";
            local.Message1.Text6 = ", Per";
            local.Message1.Text22 = ": Did not process the";
            local.Message1.Text9 = "incoming";
            local.Message1.Text16 = "multiple ssn 2";
            local.Message2.Text6 = " field";
            local.EabReportSend.RptDetail = local.Message1.Text8 + local
              .Message2.Text11 + local.Message1.Text6 + local
              .CsePersonsWorkSet.Number + local.Message1.Text22 + local
              .Message1.Text9 + local.Message1.Text16 + local.Message2.Text6;
            local.EabFileHandling.Action = "WRITE";
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            ExitState = "ACO_NN0000_ALL_OK";
            local.Message1.Text22 = "";
            local.Message1.Text9 = "";
            local.Message1.Text16 = "";
            local.Message2.Text6 = "";
            local.Message2.Text11 = "";
            local.Message1.Text8 = "";
            local.Message1.Text6 = "";
            local.EabReportSend.RptDetail = "";

            goto Test3;
          }
          else
          {
            // this is fine, there is not invalid ssn record for this 
            // combination of cse person number and ssn number
          }

          if (Equal(local.FcrPersonAckErrorRecord.MultipleSsn2,
            local.FcrPersonAckErrorRecord.AdditionalSsn1) || Equal
            (local.FcrPersonAckErrorRecord.MultipleSsn2,
            local.FcrPersonAckErrorRecord.AdditionalSsn2))
          {
          }
          else
          {
            local.Infrastructure.Detail =
              "Additional SSN Received from FCR: " + Substring
              (local.FcrPersonAckErrorRecord.MultipleSsn2,
              FcrPersonAckErrorRecord.MultipleSsn2_MaxLength, 1, 3) + "-" + Substring
              (local.FcrPersonAckErrorRecord.MultipleSsn2,
              FcrPersonAckErrorRecord.MultipleSsn2_MaxLength, 4, 2) + "-" + Substring
              (local.FcrPersonAckErrorRecord.MultipleSsn2,
              FcrPersonAckErrorRecord.MultipleSsn2_MaxLength, 6, 4);
            local.Infrastructure.CaseNumber =
              Substring(local.FcrPersonAckErrorRecord.CaseId, 1, 10);
            local.Infrastructure.CsePersonNumber =
              Substring(local.FcrPersonAckErrorRecord.MemberId, 6, 10);
            local.Infrastructure.ReasonCode = "ALTSSNFCR";
            local.Infrastructure.ProcessStatus = "Q";
            UseOeB412CreateInfrastructure2();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              break;
            }

            ++local.Commit.Count;
          }
        }

Test3:

        if (!IsEmpty(local.FcrPersonAckErrorRecord.MultipleSsn3))
        {
          local.CsePersonsWorkSet.Number =
            Substring(local.FcrPersonAckErrorRecord.MemberId, 6, 10);
          local.ReadSsn.SsnNum9 =
            (int)StringToNumber(local.FcrPersonAckErrorRecord.MultipleSsn3);

          // **********************************************************
          // 07/29/2009   DDupree   added this check as part of cq7189.
          // **********************************************************
          if (ReadInvalidSsn())
          {
            local.ConvertMessage.SsnTextPart1 =
              Substring(local.FcrPersonAckErrorRecord.MultipleSsn3, 1, 3);
            local.ConvertMessage.SsnTextPart2 =
              Substring(local.FcrPersonAckErrorRecord.MultipleSsn3, 4, 2);
            local.ConvertMessage.SsnTextPart3 =
              Substring(local.FcrPersonAckErrorRecord.MultipleSsn3, 6, 4);
            local.Message2.Text11 = local.ConvertMessage.SsnTextPart1 + "-" + local
              .ConvertMessage.SsnTextPart2 + "-" + local
              .ConvertMessage.SsnTextPart3;
            local.Message1.Text8 = "Bad SSN";
            local.Message1.Text6 = ", Per";
            local.Message1.Text22 = ": Did not process the";
            local.Message1.Text9 = "incoming";
            local.Message1.Text16 = "multiple ssn 3";
            local.Message2.Text6 = " field";
            local.EabReportSend.RptDetail = local.Message1.Text8 + local
              .Message2.Text11 + local.Message1.Text6 + local
              .CsePersonsWorkSet.Number + local.Message1.Text22 + local
              .Message1.Text9 + local.Message1.Text16 + local.Message2.Text6;
            local.EabFileHandling.Action = "WRITE";
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            ExitState = "ACO_NN0000_ALL_OK";
            local.Message1.Text22 = "";
            local.Message1.Text9 = "";
            local.Message1.Text16 = "";
            local.Message2.Text6 = "";
            local.Message2.Text11 = "";
            local.Message1.Text8 = "";
            local.Message1.Text6 = "";
            local.EabReportSend.RptDetail = "";

            goto Test4;
          }
          else
          {
            // this is fine, there is not invalid ssn record for this 
            // combination of cse person number and ssn number
          }

          if (Equal(local.FcrPersonAckErrorRecord.MultipleSsn3,
            local.FcrPersonAckErrorRecord.AdditionalSsn1) || Equal
            (local.FcrPersonAckErrorRecord.MultipleSsn3,
            local.FcrPersonAckErrorRecord.AdditionalSsn2))
          {
          }
          else
          {
            local.Infrastructure.Detail =
              "Additional SSN Received from FCR: " + Substring
              (local.FcrPersonAckErrorRecord.MultipleSsn3,
              FcrPersonAckErrorRecord.MultipleSsn3_MaxLength, 1, 3) + "-" + Substring
              (local.FcrPersonAckErrorRecord.MultipleSsn3,
              FcrPersonAckErrorRecord.MultipleSsn3_MaxLength, 4, 2) + "-" + Substring
              (local.FcrPersonAckErrorRecord.MultipleSsn3,
              FcrPersonAckErrorRecord.MultipleSsn3_MaxLength, 6, 4);
            local.Infrastructure.CaseNumber =
              Substring(local.FcrPersonAckErrorRecord.CaseId, 1, 10);
            local.Infrastructure.CsePersonNumber =
              Substring(local.FcrPersonAckErrorRecord.MemberId, 6, 10);
            local.Infrastructure.ReasonCode = "ALTSSNFCR";
            local.Infrastructure.ProcessStatus = "Q";
            UseOeB412CreateInfrastructure2();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              break;
            }

            ++local.Commit.Count;
          }
        }

Test4:

        // ***********************************************************************************************
        // 11/08/2005               DDupree               WR00258947
        // Added  the following 'if' statements so that the new additional ssn 
        // validation
        // codes could be processed.
        // ***********************************************************************************************
        if (!IsEmpty(local.FcrPersonAckErrorRecord.AdditionalSsn1) && !
          IsEmpty(local.FcrPersonAckErrorRecord.AdditionalSsn1ValidityCode))
        {
          local.CsePersonsWorkSet.Number =
            Substring(local.FcrPersonAckErrorRecord.MemberId, 6, 10);
          local.ReadSsn.SsnNum9 =
            (int)StringToNumber(local.FcrPersonAckErrorRecord.AdditionalSsn1);

          // **********************************************************
          // 07/29/2009   DDupree   added this check as part of cq7189.
          // **********************************************************
          if (ReadInvalidSsn())
          {
            local.ConvertMessage.SsnTextPart1 =
              Substring(local.FcrPersonAckErrorRecord.AdditionalSsn1, 1, 3);
            local.ConvertMessage.SsnTextPart2 =
              Substring(local.FcrPersonAckErrorRecord.AdditionalSsn1, 4, 2);
            local.ConvertMessage.SsnTextPart3 =
              Substring(local.FcrPersonAckErrorRecord.AdditionalSsn1, 6, 4);
            local.Message2.Text11 = local.ConvertMessage.SsnTextPart1 + "-" + local
              .ConvertMessage.SsnTextPart2 + "-" + local
              .ConvertMessage.SsnTextPart3;
            local.Message1.Text8 = "Bad SSN";
            local.Message1.Text6 = ", Per";
            local.Message1.Text22 = ": Did not process the";
            local.Message1.Text9 = "incoming";
            local.Message1.Text16 = "additional ssn 1";
            local.Message2.Text6 = " field";
            local.EabReportSend.RptDetail = local.Message1.Text8 + local
              .Message2.Text11 + local.Message1.Text6 + local
              .CsePersonsWorkSet.Number + local.Message1.Text22 + local
              .Message1.Text9 + local.Message1.Text16 + local.Message2.Text6;
            local.EabFileHandling.Action = "WRITE";
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            ExitState = "ACO_NN0000_ALL_OK";
            local.Message1.Text22 = "";
            local.Message1.Text9 = "";
            local.Message1.Text16 = "";
            local.Message2.Text6 = "";
            local.Message2.Text11 = "";
            local.Message1.Text8 = "";
            local.Message1.Text6 = "";
            local.EabReportSend.RptDetail = "";

            goto Test5;
          }
          else
          {
            // this is fine, there is not invalid ssn record for this 
            // combination of cse person number and ssn number
          }

          if (AsChar(local.FcrPersonAckErrorRecord.AdditionalSsn1ValidityCode) ==
            'V')
          {
            local.Infrastructure.Detail =
              "Additional SSN received from the FCR was verified by SSA: " + Substring
              (local.FcrPersonAckErrorRecord.AdditionalSsn1,
              FcrPersonAckErrorRecord.AdditionalSsn1_MaxLength, 1, 3) + "-" + Substring
              (local.FcrPersonAckErrorRecord.AdditionalSsn1,
              FcrPersonAckErrorRecord.AdditionalSsn1_MaxLength, 4, 2) + "-" + Substring
              (local.FcrPersonAckErrorRecord.AdditionalSsn1,
              FcrPersonAckErrorRecord.AdditionalSsn1_MaxLength, 6, 4);
          }
          else
          {
            local.Infrastructure.Detail =
              "Additional SSN received from the FCR was not verified by SSA: " +
              Substring
              (local.FcrPersonAckErrorRecord.AdditionalSsn1,
              FcrPersonAckErrorRecord.AdditionalSsn1_MaxLength, 1, 3) + "-" + Substring
              (local.FcrPersonAckErrorRecord.AdditionalSsn1,
              FcrPersonAckErrorRecord.AdditionalSsn1_MaxLength, 4, 2) + "-" + Substring
              (local.FcrPersonAckErrorRecord.AdditionalSsn1,
              FcrPersonAckErrorRecord.AdditionalSsn1_MaxLength, 6, 4);
          }

          local.Infrastructure.CaseNumber =
            Substring(local.FcrPersonAckErrorRecord.CaseId, 1, 10);
          local.Infrastructure.CsePersonNumber =
            Substring(local.FcrPersonAckErrorRecord.MemberId, 6, 10);
          local.Infrastructure.ReasonCode = "ALTSSNFCR";
          local.Infrastructure.ProcessStatus = "Q";
          UseOeB412CreateInfrastructure2();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            break;
          }

          ++local.Commit.Count;
        }

Test5:

        if (!IsEmpty(local.FcrPersonAckErrorRecord.AdditionalSsn2) && !
          IsEmpty(local.FcrPersonAckErrorRecord.AdditionalSsn2ValidityCode))
        {
          local.CsePersonsWorkSet.Number =
            Substring(local.FcrPersonAckErrorRecord.MemberId, 6, 10);
          local.ReadSsn.SsnNum9 =
            (int)StringToNumber(local.FcrPersonAckErrorRecord.AdditionalSsn2);

          // **********************************************************
          // 07/29/2009   DDupree   added this check as part of cq7189.
          // **********************************************************
          if (ReadInvalidSsn())
          {
            local.ConvertMessage.SsnTextPart1 =
              Substring(local.FcrPersonAckErrorRecord.AdditionalSsn2, 1, 3);
            local.ConvertMessage.SsnTextPart2 =
              Substring(local.FcrPersonAckErrorRecord.AdditionalSsn2, 4, 2);
            local.ConvertMessage.SsnTextPart3 =
              Substring(local.FcrPersonAckErrorRecord.AdditionalSsn2, 6, 4);
            local.Message2.Text11 = local.ConvertMessage.SsnTextPart1 + "-" + local
              .ConvertMessage.SsnTextPart2 + "-" + local
              .ConvertMessage.SsnTextPart3;
            local.Message1.Text8 = "Bad SSN";
            local.Message1.Text6 = ", Per";
            local.Message1.Text22 = ": Did not process the";
            local.Message1.Text9 = "incoming";
            local.Message1.Text16 = "additional ssn 2";
            local.Message2.Text6 = " field";
            local.EabReportSend.RptDetail = local.Message1.Text8 + local
              .Message2.Text11 + local.Message1.Text6 + local
              .CsePersonsWorkSet.Number + local.Message1.Text22 + local
              .Message1.Text9 + local.Message1.Text16 + local.Message2.Text6;
            local.EabFileHandling.Action = "WRITE";
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            ExitState = "ACO_NN0000_ALL_OK";
            local.Message1.Text22 = "";
            local.Message1.Text9 = "";
            local.Message1.Text16 = "";
            local.Message2.Text6 = "";
            local.Message2.Text11 = "";
            local.Message1.Text8 = "";
            local.Message1.Text6 = "";
            local.EabReportSend.RptDetail = "";

            goto Test6;
          }
          else
          {
            // this is fine, there is not invalid ssn record for this 
            // combination of cse person number and ssn number
          }

          if (AsChar(local.FcrPersonAckErrorRecord.AdditionalSsn2ValidityCode) ==
            'V')
          {
            local.Infrastructure.Detail =
              "Additional SSN received from the FCR was verified by SSA: " + Substring
              (local.FcrPersonAckErrorRecord.AdditionalSsn2,
              FcrPersonAckErrorRecord.AdditionalSsn2_MaxLength, 1, 3) + "-" + Substring
              (local.FcrPersonAckErrorRecord.AdditionalSsn2,
              FcrPersonAckErrorRecord.AdditionalSsn2_MaxLength, 4, 2) + "-" + Substring
              (local.FcrPersonAckErrorRecord.AdditionalSsn2,
              FcrPersonAckErrorRecord.AdditionalSsn2_MaxLength, 6, 4);
          }
          else
          {
            local.Infrastructure.Detail =
              "Additional SSN received from the FCR was not verified by SSA: " +
              Substring
              (local.FcrPersonAckErrorRecord.AdditionalSsn2,
              FcrPersonAckErrorRecord.AdditionalSsn2_MaxLength, 1, 3) + "-" + Substring
              (local.FcrPersonAckErrorRecord.AdditionalSsn2,
              FcrPersonAckErrorRecord.AdditionalSsn2_MaxLength, 4, 2) + "-" + Substring
              (local.FcrPersonAckErrorRecord.AdditionalSsn2,
              FcrPersonAckErrorRecord.AdditionalSsn2_MaxLength, 6, 4);
          }

          local.Infrastructure.CaseNumber =
            Substring(local.FcrPersonAckErrorRecord.CaseId, 1, 10);
          local.Infrastructure.CsePersonNumber =
            Substring(local.FcrPersonAckErrorRecord.MemberId, 6, 10);
          local.Infrastructure.ReasonCode = "ALTSSNFCR";
          local.Infrastructure.ProcessStatus = "Q";
          UseOeB412CreateInfrastructure2();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            break;
          }

          ++local.Commit.Count;
        }

Test6:

        switch(AsChar(local.DateOfDeathIndicator.Flag))
        {
          case 'Y':
            // ********************************************************************
            // Date of Death has been reported by Social Security Administration
            // ********************************************************************
            local.FcrCsePerson.Number =
              Substring(local.FcrPersonAckErrorRecord.MemberId, 6, 10);
            local.ConvertDateDateWorkArea.Date =
              local.FcrPersonAckErrorRecord.DateOfDeath;
            UseCabConvertDate2String();
            UseCabProcessDateOfDeath();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              break;
            }

            ++local.Commit.Count;

            break;
          case 'R':
            // ********************************************************************
            // Date of Death reported erroneously by Social Security 
            // Administration
            // ********************************************************************
            local.FcrCsePerson.Number =
              Substring(local.FcrPersonAckErrorRecord.MemberId, 6, 10);
            UseCabReverseDateOfDeath();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              break;
            }

            ++local.Commit.Count;

            break;
          case 'I':
            // ********************************************************************
            // Date of Death reported by Social Security Administration is 
            // invalid
            // (date of death is less than or equal to date of birth)
            // ********************************************************************
            local.FcrCsePerson.Number =
              Substring(local.FcrPersonAckErrorRecord.MemberId, 6, 10);
            local.ConvertDateDateWorkArea.Date =
              local.FcrPersonAckErrorRecord.DateOfDeath;
            UseCabConvertDate2String();
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Invalid date of death for person: " + (
                local.Infrastructure.CsePersonNumber ?? "") + " date reported as: " +
              local.ConvertDateTextWorkArea.Text8;
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              break;
            }

            break;
          default:
            break;
        }
      }

      if (local.Commit.Count >= local
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        UseExtToDoACommit();

        if (local.Eab.NumericReturnCode != 0)
        {
          ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

          break;
        }

        local.EabReportSend.RptDetail =
          "Commit Taken after Commit Count reached: " + NumberToString
          (local.Commit.Count, 15);
        local.EabFileHandling.Action = "WRITE";
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          break;
        }

        local.Commit.Count = 0;
      }
    }
    while(!Equal(local.Eab.TextReturnCode, "EF"));

AfterCycle2:

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseOeB412Close();
    }
    else
    {
      UseEabExtractExitStateMessage();
      local.EabReportSend.RptDetail = "Program abended because: " + local
        .ExitStateWorkArea.Message;
      local.EabFileHandling.Action = "WRITE";
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      ExitState = "ACO_NN0000_ALL_OK";
      UseOeB412Close();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }

    UseSiCloseAdabas();
  }

  private static void MoveAbendData(AbendData source, AbendData target)
  {
    target.Type1 = source.Type1;
    target.AdabasFileNumber = source.AdabasFileNumber;
    target.AdabasFileAction = source.AdabasFileAction;
    target.AdabasResponseCd = source.AdabasResponseCd;
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

  private static void MoveFcrCaseAckErrorRecord(FcrCaseAckErrorRecord source,
    FcrCaseAckErrorRecord target)
  {
    target.RecordIdentifier = source.RecordIdentifier;
    target.ActionTypeCode = source.ActionTypeCode;
    target.CaseId = source.CaseId;
    target.CaseType = source.CaseType;
    target.OrderIndicator = source.OrderIndicator;
    target.FipsCountyCode = source.FipsCountyCode;
    target.UserField = source.UserField;
    target.PreviousCaseId = source.PreviousCaseId;
    target.BatchNumber = source.BatchNumber;
    target.AcknowledgementCode = source.AcknowledgementCode;
    target.ErrorCode1 = source.ErrorCode1;
    target.ErrorCode2 = source.ErrorCode2;
    target.ErrorCode3 = source.ErrorCode3;
    target.ErrorCode4 = source.ErrorCode4;
    target.ErrorCode5 = source.ErrorCode5;
  }

  private static void MoveFcrPersonAckErrorRecord1(
    FcrPersonAckErrorRecord source, FcrPersonAckErrorRecord target)
  {
    target.RecordIdentifier = source.RecordIdentifier;
    target.ActionTypeCode = source.ActionTypeCode;
    target.CaseId = source.CaseId;
    target.UserField = source.UserField;
    target.FipsCountyCode = source.FipsCountyCode;
    target.LocateRequestType = source.LocateRequestType;
    target.BundleFplsLocateResults = source.BundleFplsLocateResults;
    target.ParticipantType = source.ParticipantType;
    target.FamilyViolence = source.FamilyViolence;
    target.MemberId = source.MemberId;
    target.SexCode = source.SexCode;
    target.DateOfBirth = source.DateOfBirth;
    target.Ssn = source.Ssn;
    target.PreviousSsn = source.PreviousSsn;
    target.FirstName = source.FirstName;
    target.MiddleName = source.MiddleName;
    target.LastName = source.LastName;
    target.CityOfBirth = source.CityOfBirth;
    target.StateOrCountryOfBirth = source.StateOrCountryOfBirth;
    target.FathersFirstName = source.FathersFirstName;
    target.FathersMiddleInitial = source.FathersMiddleInitial;
    target.FathersLastName = source.FathersLastName;
    target.MothersFirstName = source.MothersFirstName;
    target.MothersMiddleInitial = source.MothersMiddleInitial;
    target.MothersMaidenName = source.MothersMaidenName;
    target.IrsUSsn = source.IrsUSsn;
    target.AdditionalSsn1 = source.AdditionalSsn1;
    target.AdditionalSsn2 = source.AdditionalSsn2;
    target.AdditionalFirstName1 = source.AdditionalFirstName1;
    target.AdditionalMiddleName1 = source.AdditionalMiddleName1;
    target.AdditionalLastName1 = source.AdditionalLastName1;
    target.AdditionalFirstName2 = source.AdditionalFirstName2;
    target.AdditionalMiddleName2 = source.AdditionalMiddleName2;
    target.AdditionalLastName2 = source.AdditionalLastName2;
    target.AdditionalFirstName3 = source.AdditionalFirstName3;
    target.AdditionalMiddleName3 = source.AdditionalMiddleName3;
    target.AdditionalLastName3 = source.AdditionalLastName3;
    target.AdditionalFirstName4 = source.AdditionalFirstName4;
    target.AdditionalMiddleName4 = source.AdditionalMiddleName4;
    target.AdditionalLastName4 = source.AdditionalLastName4;
    target.NewMemberId = source.NewMemberId;
    target.Irs1099 = source.Irs1099;
    target.LocateSource1 = source.LocateSource1;
    target.LocateSource2 = source.LocateSource2;
    target.LocateSource3 = source.LocateSource3;
    target.LocateSource4 = source.LocateSource4;
    target.LocateSource5 = source.LocateSource5;
    target.LocateSource6 = source.LocateSource6;
    target.LocateSource7 = source.LocateSource7;
    target.LocateSource8 = source.LocateSource8;
    target.SsnValidityCode = source.SsnValidityCode;
    target.ProvidedOrCorrectedSsn = source.ProvidedOrCorrectedSsn;
    target.MultipleSsn1 = source.MultipleSsn1;
    target.MultipleSsn2 = source.MultipleSsn2;
    target.MultipleSsn3 = source.MultipleSsn3;
    target.SsaDateOfBirthIndicator = source.SsaDateOfBirthIndicator;
    target.BatchNumber = source.BatchNumber;
    target.DateOfDeath = source.DateOfDeath;
    target.SsaZipCodeOfLastResidence = source.SsaZipCodeOfLastResidence;
    target.SsaZipCodeOfLumpSumPayment = source.SsaZipCodeOfLumpSumPayment;
    target.FcrPrimarySsn = source.FcrPrimarySsn;
    target.FcrPrimaryFirstName = source.FcrPrimaryFirstName;
    target.FcrPrimaryMiddleName = source.FcrPrimaryMiddleName;
    target.FcrPrimaryLastName = source.FcrPrimaryLastName;
    target.AcknowledgementCode = source.AcknowledgementCode;
    target.ErrorCode1 = source.ErrorCode1;
    target.ErrorCode2 = source.ErrorCode2;
    target.ErrorCode3 = source.ErrorCode3;
    target.ErrorCode4 = source.ErrorCode4;
    target.ErrorCode5 = source.ErrorCode5;
    target.AdditionalSsn1ValidityCode = source.AdditionalSsn1ValidityCode;
    target.AdditionalSsn2ValidityCode = source.AdditionalSsn2ValidityCode;
  }

  private static void MoveFcrPersonAckErrorRecord2(
    FcrPersonAckErrorRecord source, FcrPersonAckErrorRecord target)
  {
    target.CaseId = source.CaseId;
    target.MemberId = source.MemberId;
    target.FirstName = source.FirstName;
    target.MiddleName = source.MiddleName;
    target.LastName = source.LastName;
    target.DateOfDeath = source.DateOfDeath;
    target.SsaZipCodeOfLastResidence = source.SsaZipCodeOfLastResidence;
  }

  private static void MoveFcrPersonAckErrorRecord3(
    FcrPersonAckErrorRecord source, FcrPersonAckErrorRecord target)
  {
    target.CaseId = source.CaseId;
    target.MemberId = source.MemberId;
    target.FirstName = source.FirstName;
    target.MiddleName = source.MiddleName;
    target.LastName = source.LastName;
    target.DateOfDeath = source.DateOfDeath;
    target.SsaZipCodeOfLastResidence = source.SsaZipCodeOfLastResidence;
    target.FcrPrimaryFirstName = source.FcrPrimaryFirstName;
    target.FcrPrimaryMiddleName = source.FcrPrimaryMiddleName;
    target.FcrPrimaryLastName = source.FcrPrimaryLastName;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private void UseCabConvertDate2String()
  {
    var useImport = new CabConvertDate2String.Import();
    var useExport = new CabConvertDate2String.Export();

    useImport.DateWorkArea.Date = local.ConvertDateDateWorkArea.Date;

    Call(CabConvertDate2String.Execute, useImport, useExport);

    local.ConvertDateTextWorkArea.Text8 = useExport.TextWorkArea.Text8;
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

  private void UseCabProcessDateOfDeath()
  {
    var useImport = new CabProcessDateOfDeath.Import();
    var useExport = new CabProcessDateOfDeath.Export();

    useImport.Max.Date = local.Max.Date;
    useImport.PersonsUpdated.Count = local.PersonsUpdatedWithDod.Count;
    useImport.DodAlertsCreated.Count = local.DodAlertsCreated.Count;
    useImport.DodEventsCreated.Count = local.DodEventsCreated.Count;
    MoveFcrPersonAckErrorRecord3(local.FcrPersonAckErrorRecord,
      useImport.FcrPersonAckErrorRecord);
    useImport.SsaCityLastResidence.Text15 = local.SsaCityLastResidence.Text15;
    useImport.SsaStateLastResidence.Text2 = local.SsaStateLastResidence.Text2;
    useImport.CsePerson.Number = local.FcrCsePerson.Number;
    useImport.ConvertDateOfDeath.Text8 = local.ConvertDateTextWorkArea.Text8;
    MoveProgramProcessingInfo(local.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);
    useExport.DodAlertsCreated.Count = local.DodAlertsCreated.Count;

    Call(CabProcessDateOfDeath.Execute, useImport, useExport);

    local.PersonsUpdatedWithDod.Count = useExport.PersonsUpdated.Count;
    local.DodAlertsCreated.Count = useExport.DodAlertsCreated.Count;
    local.DodEventsCreated.Count = useExport.DodEventsCreated.Count;
  }

  private void UseCabReverseDateOfDeath()
  {
    var useImport = new CabReverseDateOfDeath.Import();
    var useExport = new CabReverseDateOfDeath.Export();

    useImport.Max.Date = local.Max.Date;
    useImport.PersonsUpdated.Count = local.PersonsUpdatedWithDod.Count;
    useImport.DodAlertsCreated.Count = local.DodAlertsCreated.Count;
    useImport.DodEventsCreated.Count = local.DodEventsCreated.Count;
    MoveFcrPersonAckErrorRecord2(local.FcrPersonAckErrorRecord,
      useImport.FcrPersonAckErrorRecord);
    useImport.SsaCityLastResidence.Text15 = local.SsaCityLastResidence.Text15;
    useImport.SsaStateLastResidence.Text2 = local.SsaStateLastResidence.Text2;
    useImport.CsePerson.Number = local.FcrCsePerson.Number;
    MoveProgramProcessingInfo(local.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);

    Call(CabReverseDateOfDeath.Execute, useImport, useExport);

    local.PersonsUpdatedWithDod.Count = useExport.PersonsUpdated.Count;
    local.DodAlertsCreated.Count = useExport.DodAlertsCreated.Count;
    local.DodEventsCreated.Count = useExport.DodEventsCreated.Count;
  }

  private void UseCabUpdateAdabasPersonSsnBat()
  {
    var useImport = new CabUpdateAdabasPersonSsnBat.Import();
    var useExport = new CabUpdateAdabasPersonSsnBat.Export();

    useImport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);

    Call(CabUpdateAdabasPersonSsnBat.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseEabReadCsePersonBatchAll()
  {
    var useImport = new EabReadCsePersonBatchAll.Import();
    var useExport = new EabReadCsePersonBatchAll.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;
    useImport.Current.Date = local.CurrentDate.Date;
    MoveCsePersonsWorkSet(local.CsePersonsWorkSet, useExport.CsePersonsWorkSet);
    useExport.AbendData.Assign(local.AbendData);
    useExport.Ae.Flag = local.Ae.Flag;
    useExport.Kscares.Flag = local.Kscares.Flag;

    Call(EabReadCsePersonBatchAll.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    MoveAbendData(useExport.AbendData, local.AbendData);
    local.Ae.Flag = useExport.Ae.Flag;
    local.Kscares.Flag = useExport.Kscares.Flag;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.Eab.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.Eab.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseOeB412Close()
  {
    var useImport = new OeB412Close.Import();
    var useExport = new OeB412Close.Export();

    useImport.PersonsUpdated.Count = local.PersonsUpdatedWithDod.Count;
    useImport.DodAlertsCreated.Count = local.DodAlertsCreated.Count;
    useImport.DodEventsCreated.Count = local.DodEventsCreated.Count;
    useImport.RecordsSkipped.Count = local.RecordsSkipped.Count;
    useImport.EventsCreated.Count = local.EventsCreated.Count;
    useImport.RecordsRead.Count = local.RecordsRead.Count;
    useImport.KpcRecords.Count = local.KpcRecordsSkipped.Count;
    useImport.AlertsCreated.Count = local.AlertsCreated.Count;
    useImport.ErrorsReceived.Count = local.ErrorsReceived.Count;
    useImport.AcksReceived.Count = local.AcksReceived.Count;
    useImport.FcrError.Count = local.FcrError.Count;

    Call(OeB412Close.Execute, useImport, useExport);
  }

  private void UseOeB412CreateInfrastructure1()
  {
    var useImport = new OeB412CreateInfrastructure.Import();
    var useExport = new OeB412CreateInfrastructure.Export();

    useImport.ItemsCreated.Count = local.EventsCreated.Count;
    useImport.Infrastructure.Assign(local.Infrastructure);
    MoveProgramProcessingInfo(local.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);

    Call(OeB412CreateInfrastructure.Execute, useImport, useExport);

    local.EventsCreated.Count = useExport.ItemsCreated.Count;
  }

  private void UseOeB412CreateInfrastructure2()
  {
    var useImport = new OeB412CreateInfrastructure.Import();
    var useExport = new OeB412CreateInfrastructure.Export();

    useImport.ItemsCreated.Count = local.AlertsCreated.Count;
    useImport.Infrastructure.Assign(local.Infrastructure);
    MoveProgramProcessingInfo(local.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);

    Call(OeB412CreateInfrastructure.Execute, useImport, useExport);

    local.AlertsCreated.Count = useExport.ItemsCreated.Count;
  }

  private void UseOeB412Housekeeping()
  {
    var useImport = new OeB412Housekeeping.Import();
    var useExport = new OeB412Housekeeping.Export();

    Call(OeB412Housekeeping.Execute, useImport, useExport);

    local.FcrErrors.Id = useExport.FcrErrors.Id;
    local.ProgramCheckpointRestart.UpdateFrequencyCount =
      useExport.ProgramCheckpointRestart.UpdateFrequencyCount;
    MoveProgramProcessingInfo(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
  }

  private void UseOeEabReadFcrAckRecords()
  {
    var useImport = new OeEabReadFcrAckRecords.Import();
    var useExport = new OeEabReadFcrAckRecords.Export();

    useImport.External.Assign(local.Eab);
    useExport.DateOfDeathIndicator.Flag = local.DateOfDeathIndicator.Flag;
    useExport.SsaCityLastResidence.Text15 = local.SsaCityLastResidence.Text15;
    useExport.SsaStateLastResidence.Text2 = local.SsaStateLastResidence.Text2;
    useExport.SsaCityLumpSumPayment.Text15 = local.SsaCityLumpSumPayment.Text15;
    useExport.SsaStateLumpSumPaymnt.Text2 = local.SsaStateLumpSumPayment.Text2;
    useExport.External.Assign(local.Eab);
    useExport.FcrCaseAckErrorRecord.Assign(local.FcrCaseAckErrorRecord);
    useExport.FcrPersonAckErrorRecord.Assign(local.FcrPersonAckErrorRecord);

    Call(OeEabReadFcrAckRecords.Execute, useImport, useExport);

    local.DateOfDeathIndicator.Flag = useExport.DateOfDeathIndicator.Flag;
    local.SsaCityLastResidence.Text15 = useExport.SsaCityLastResidence.Text15;
    local.SsaStateLastResidence.Text2 = useExport.SsaStateLastResidence.Text2;
    local.SsaCityLumpSumPayment.Text15 = useExport.SsaCityLumpSumPayment.Text15;
    local.SsaStateLumpSumPayment.Text2 = useExport.SsaStateLumpSumPaymnt.Text2;
    local.Eab.Assign(useExport.External);
    MoveFcrCaseAckErrorRecord(useExport.FcrCaseAckErrorRecord,
      local.FcrCaseAckErrorRecord);
    MoveFcrPersonAckErrorRecord1(useExport.FcrPersonAckErrorRecord,
      local.FcrPersonAckErrorRecord);
  }

  private void UseSiCloseAdabas()
  {
    var useImport = new SiCloseAdabas.Import();
    var useExport = new SiCloseAdabas.Export();

    Call(SiCloseAdabas.Execute, useImport, useExport);
  }

  private bool ReadCodeValue()
  {
    entities.CodeValue.Populated = false;

    return Read("ReadCodeValue",
      (db, command) =>
      {
        db.SetNullableInt32(command, "codId", local.FcrErrors.Id);
        db.SetString(command, "cdvalue", local.LookUp.Cdvalue);
      },
      (db, reader) =>
      {
        entities.CodeValue.Id = db.GetInt32(reader, 0);
        entities.CodeValue.CodId = db.GetNullableInt32(reader, 1);
        entities.CodeValue.Cdvalue = db.GetString(reader, 2);
        entities.CodeValue.EffectiveDate = db.GetDate(reader, 3);
        entities.CodeValue.ExpirationDate = db.GetDate(reader, 4);
        entities.CodeValue.Description = db.GetString(reader, 5);
        entities.CodeValue.Populated = true;
      });
  }

  private bool ReadInvalidSsn()
  {
    entities.InvalidSsn.Populated = false;

    return Read("ReadInvalidSsn",
      (db, command) =>
      {
        db.SetInt32(command, "ssn", local.ReadSsn.SsnNum9);
        db.SetString(command, "cspNumber", local.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.InvalidSsn.CspNumber = db.GetString(reader, 0);
        entities.InvalidSsn.Ssn = db.GetInt32(reader, 1);
        entities.InvalidSsn.Populated = true;
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonsWorkSet Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    private CsePersonsWorkSet obligor;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    private AbendData abendData;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Kscares.
    /// </summary>
    [JsonPropertyName("kscares")]
    public Common Kscares
    {
      get => kscares ??= new();
      set => kscares = value;
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
    /// A value of ConvertMessage.
    /// </summary>
    [JsonPropertyName("convertMessage")]
    public SsnWorkArea ConvertMessage
    {
      get => convertMessage ??= new();
      set => convertMessage = value;
    }

    /// <summary>
    /// A value of Message2.
    /// </summary>
    [JsonPropertyName("message2")]
    public WorkArea Message2
    {
      get => message2 ??= new();
      set => message2 = value;
    }

    /// <summary>
    /// A value of Message1.
    /// </summary>
    [JsonPropertyName("message1")]
    public WorkArea Message1
    {
      get => message1 ??= new();
      set => message1 = value;
    }

    /// <summary>
    /// A value of ReadSsn.
    /// </summary>
    [JsonPropertyName("readSsn")]
    public SsnWorkArea ReadSsn
    {
      get => readSsn ??= new();
      set => readSsn = value;
    }

    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
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
    /// A value of PersonsUpdatedWithDod.
    /// </summary>
    [JsonPropertyName("personsUpdatedWithDod")]
    public Common PersonsUpdatedWithDod
    {
      get => personsUpdatedWithDod ??= new();
      set => personsUpdatedWithDod = value;
    }

    /// <summary>
    /// A value of DodAlertsCreated.
    /// </summary>
    [JsonPropertyName("dodAlertsCreated")]
    public Common DodAlertsCreated
    {
      get => dodAlertsCreated ??= new();
      set => dodAlertsCreated = value;
    }

    /// <summary>
    /// A value of DodEventsCreated.
    /// </summary>
    [JsonPropertyName("dodEventsCreated")]
    public Common DodEventsCreated
    {
      get => dodEventsCreated ??= new();
      set => dodEventsCreated = value;
    }

    /// <summary>
    /// A value of RecordsSkipped.
    /// </summary>
    [JsonPropertyName("recordsSkipped")]
    public Common RecordsSkipped
    {
      get => recordsSkipped ??= new();
      set => recordsSkipped = value;
    }

    /// <summary>
    /// A value of EventsCreated.
    /// </summary>
    [JsonPropertyName("eventsCreated")]
    public Common EventsCreated
    {
      get => eventsCreated ??= new();
      set => eventsCreated = value;
    }

    /// <summary>
    /// A value of RecordsRead.
    /// </summary>
    [JsonPropertyName("recordsRead")]
    public Common RecordsRead
    {
      get => recordsRead ??= new();
      set => recordsRead = value;
    }

    /// <summary>
    /// A value of FcrRead.
    /// </summary>
    [JsonPropertyName("fcrRead")]
    public Common FcrRead
    {
      get => fcrRead ??= new();
      set => fcrRead = value;
    }

    /// <summary>
    /// A value of KpcRecordsSkipped.
    /// </summary>
    [JsonPropertyName("kpcRecordsSkipped")]
    public Common KpcRecordsSkipped
    {
      get => kpcRecordsSkipped ??= new();
      set => kpcRecordsSkipped = value;
    }

    /// <summary>
    /// A value of AlertsCreated.
    /// </summary>
    [JsonPropertyName("alertsCreated")]
    public Common AlertsCreated
    {
      get => alertsCreated ??= new();
      set => alertsCreated = value;
    }

    /// <summary>
    /// A value of ErrorsReceived.
    /// </summary>
    [JsonPropertyName("errorsReceived")]
    public Common ErrorsReceived
    {
      get => errorsReceived ??= new();
      set => errorsReceived = value;
    }

    /// <summary>
    /// A value of AcksReceived.
    /// </summary>
    [JsonPropertyName("acksReceived")]
    public Common AcksReceived
    {
      get => acksReceived ??= new();
      set => acksReceived = value;
    }

    /// <summary>
    /// A value of FcrError.
    /// </summary>
    [JsonPropertyName("fcrError")]
    public Common FcrError
    {
      get => fcrError ??= new();
      set => fcrError = value;
    }

    /// <summary>
    /// A value of DateOfDeathIndicator.
    /// </summary>
    [JsonPropertyName("dateOfDeathIndicator")]
    public Common DateOfDeathIndicator
    {
      get => dateOfDeathIndicator ??= new();
      set => dateOfDeathIndicator = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of FcrCaseAckErrorRecord.
    /// </summary>
    [JsonPropertyName("fcrCaseAckErrorRecord")]
    public FcrCaseAckErrorRecord FcrCaseAckErrorRecord
    {
      get => fcrCaseAckErrorRecord ??= new();
      set => fcrCaseAckErrorRecord = value;
    }

    /// <summary>
    /// A value of FcrPersonAckErrorRecord.
    /// </summary>
    [JsonPropertyName("fcrPersonAckErrorRecord")]
    public FcrPersonAckErrorRecord FcrPersonAckErrorRecord
    {
      get => fcrPersonAckErrorRecord ??= new();
      set => fcrPersonAckErrorRecord = value;
    }

    /// <summary>
    /// A value of SsaCityLastResidence.
    /// </summary>
    [JsonPropertyName("ssaCityLastResidence")]
    public WorkArea SsaCityLastResidence
    {
      get => ssaCityLastResidence ??= new();
      set => ssaCityLastResidence = value;
    }

    /// <summary>
    /// A value of SsaStateLastResidence.
    /// </summary>
    [JsonPropertyName("ssaStateLastResidence")]
    public WorkArea SsaStateLastResidence
    {
      get => ssaStateLastResidence ??= new();
      set => ssaStateLastResidence = value;
    }

    /// <summary>
    /// A value of SsaCityLumpSumPayment.
    /// </summary>
    [JsonPropertyName("ssaCityLumpSumPayment")]
    public WorkArea SsaCityLumpSumPayment
    {
      get => ssaCityLumpSumPayment ??= new();
      set => ssaCityLumpSumPayment = value;
    }

    /// <summary>
    /// A value of SsaStateLumpSumPayment.
    /// </summary>
    [JsonPropertyName("ssaStateLumpSumPayment")]
    public WorkArea SsaStateLumpSumPayment
    {
      get => ssaStateLumpSumPayment ??= new();
      set => ssaStateLumpSumPayment = value;
    }

    /// <summary>
    /// A value of FcrErrors.
    /// </summary>
    [JsonPropertyName("fcrErrors")]
    public Code FcrErrors
    {
      get => fcrErrors ??= new();
      set => fcrErrors = value;
    }

    /// <summary>
    /// A value of Sub.
    /// </summary>
    [JsonPropertyName("sub")]
    public Common Sub
    {
      get => sub ??= new();
      set => sub = value;
    }

    /// <summary>
    /// A value of LookUp.
    /// </summary>
    [JsonPropertyName("lookUp")]
    public CodeValue LookUp
    {
      get => lookUp ??= new();
      set => lookUp = value;
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
    /// A value of CreateAlertOnError.
    /// </summary>
    [JsonPropertyName("createAlertOnError")]
    public Common CreateAlertOnError
    {
      get => createAlertOnError ??= new();
      set => createAlertOnError = value;
    }

    /// <summary>
    /// A value of ActionType.
    /// </summary>
    [JsonPropertyName("actionType")]
    public TextWorkArea ActionType
    {
      get => actionType ??= new();
      set => actionType = value;
    }

    /// <summary>
    /// A value of MoreThanOneSsnFound.
    /// </summary>
    [JsonPropertyName("moreThanOneSsnFound")]
    public Common MoreThanOneSsnFound
    {
      get => moreThanOneSsnFound ??= new();
      set => moreThanOneSsnFound = value;
    }

    /// <summary>
    /// A value of MoreThanOneErrorFound.
    /// </summary>
    [JsonPropertyName("moreThanOneErrorFound")]
    public Common MoreThanOneErrorFound
    {
      get => moreThanOneErrorFound ??= new();
      set => moreThanOneErrorFound = value;
    }

    /// <summary>
    /// A value of AdditionalSsnsFound.
    /// </summary>
    [JsonPropertyName("additionalSsnsFound")]
    public Common AdditionalSsnsFound
    {
      get => additionalSsnsFound ??= new();
      set => additionalSsnsFound = value;
    }

    /// <summary>
    /// A value of SsnCorrected.
    /// </summary>
    [JsonPropertyName("ssnCorrected")]
    public Common SsnCorrected
    {
      get => ssnCorrected ??= new();
      set => ssnCorrected = value;
    }

    /// <summary>
    /// A value of DobCorrected.
    /// </summary>
    [JsonPropertyName("dobCorrected")]
    public Common DobCorrected
    {
      get => dobCorrected ??= new();
      set => dobCorrected = value;
    }

    /// <summary>
    /// A value of ErrorCodes.
    /// </summary>
    [JsonPropertyName("errorCodes")]
    public Common ErrorCodes
    {
      get => errorCodes ??= new();
      set => errorCodes = value;
    }

    /// <summary>
    /// A value of Temp2.
    /// </summary>
    [JsonPropertyName("temp2")]
    public External Temp2
    {
      get => temp2 ??= new();
      set => temp2 = value;
    }

    /// <summary>
    /// A value of Temp1.
    /// </summary>
    [JsonPropertyName("temp1")]
    public External Temp1
    {
      get => temp1 ??= new();
      set => temp1 = value;
    }

    /// <summary>
    /// A value of ErrorCodeExternal.
    /// </summary>
    [JsonPropertyName("errorCodeExternal")]
    public External ErrorCodeExternal
    {
      get => errorCodeExternal ??= new();
      set => errorCodeExternal = value;
    }

    /// <summary>
    /// A value of AckCode.
    /// </summary>
    [JsonPropertyName("ackCode")]
    public TextWorkArea AckCode
    {
      get => ackCode ??= new();
      set => ackCode = value;
    }

    /// <summary>
    /// A value of ErrorCodeTextWorkArea.
    /// </summary>
    [JsonPropertyName("errorCodeTextWorkArea")]
    public TextWorkArea ErrorCodeTextWorkArea
    {
      get => errorCodeTextWorkArea ??= new();
      set => errorCodeTextWorkArea = value;
    }

    /// <summary>
    /// A value of FcrCsePerson.
    /// </summary>
    [JsonPropertyName("fcrCsePerson")]
    public CsePerson FcrCsePerson
    {
      get => fcrCsePerson ??= new();
      set => fcrCsePerson = value;
    }

    /// <summary>
    /// A value of FcrCase.
    /// </summary>
    [JsonPropertyName("fcrCase")]
    public Case1 FcrCase
    {
      get => fcrCase ??= new();
      set => fcrCase = value;
    }

    /// <summary>
    /// A value of FcrRecordType.
    /// </summary>
    [JsonPropertyName("fcrRecordType")]
    public External FcrRecordType
    {
      get => fcrRecordType ??= new();
      set => fcrRecordType = value;
    }

    /// <summary>
    /// A value of ConvertDateTextWorkArea.
    /// </summary>
    [JsonPropertyName("convertDateTextWorkArea")]
    public TextWorkArea ConvertDateTextWorkArea
    {
      get => convertDateTextWorkArea ??= new();
      set => convertDateTextWorkArea = value;
    }

    /// <summary>
    /// A value of ConvertDateDateWorkArea.
    /// </summary>
    [JsonPropertyName("convertDateDateWorkArea")]
    public DateWorkArea ConvertDateDateWorkArea
    {
      get => convertDateDateWorkArea ??= new();
      set => convertDateDateWorkArea = value;
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
    /// A value of Eab.
    /// </summary>
    [JsonPropertyName("eab")]
    public External Eab
    {
      get => eab ??= new();
      set => eab = value;
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
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Ae.
    /// </summary>
    [JsonPropertyName("ae")]
    public Common Ae
    {
      get => ae ??= new();
      set => ae = value;
    }

    private Common kscares;
    private DateWorkArea currentDate;
    private SsnWorkArea convertMessage;
    private WorkArea message2;
    private WorkArea message1;
    private SsnWorkArea readSsn;
    private DateWorkArea max;
    private Common commit;
    private Common personsUpdatedWithDod;
    private Common dodAlertsCreated;
    private Common dodEventsCreated;
    private Common recordsSkipped;
    private Common eventsCreated;
    private Common recordsRead;
    private Common fcrRead;
    private Common kpcRecordsSkipped;
    private Common alertsCreated;
    private Common errorsReceived;
    private Common acksReceived;
    private Common fcrError;
    private Common dateOfDeathIndicator;
    private DateWorkArea null1;
    private FcrCaseAckErrorRecord fcrCaseAckErrorRecord;
    private FcrPersonAckErrorRecord fcrPersonAckErrorRecord;
    private WorkArea ssaCityLastResidence;
    private WorkArea ssaStateLastResidence;
    private WorkArea ssaCityLumpSumPayment;
    private WorkArea ssaStateLumpSumPayment;
    private Code fcrErrors;
    private Common sub;
    private CodeValue lookUp;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Common createAlertOnError;
    private TextWorkArea actionType;
    private Common moreThanOneSsnFound;
    private Common moreThanOneErrorFound;
    private Common additionalSsnsFound;
    private Common ssnCorrected;
    private Common dobCorrected;
    private Common errorCodes;
    private External temp2;
    private External temp1;
    private External errorCodeExternal;
    private TextWorkArea ackCode;
    private TextWorkArea errorCodeTextWorkArea;
    private CsePerson fcrCsePerson;
    private Case1 fcrCase;
    private External fcrRecordType;
    private TextWorkArea convertDateTextWorkArea;
    private DateWorkArea convertDateDateWorkArea;
    private Infrastructure infrastructure;
    private External eab;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private ExitStateWorkArea exitStateWorkArea;
    private AbendData abendData;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common ae;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of InvalidSsn.
    /// </summary>
    [JsonPropertyName("invalidSsn")]
    public InvalidSsn InvalidSsn
    {
      get => invalidSsn ??= new();
      set => invalidSsn = value;
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

    private ProgramProcessingInfo programProcessingInfo;
    private InvalidSsn invalidSsn;
    private CsePerson csePerson;
    private CodeValue codeValue;
    private Code code;
  }
#endregion
}
