// Program: SI_B250_CREATE_PA_REFERRALS, ID: 371789360, model: 746.
// Short name: SWEI250B
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
/// A program: SI_B250_CREATE_PA_REFERRALS.
/// </para>
/// <para>
/// RESP:  SRVINIT
/// This nightly run reads the PA Referral Interface table populated by AE/KSC
/// (SWRUN917?) and creates the actual PA Referrals to CSE.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SiB250CreatePaReferrals: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B250_CREATE_PA_REFERRALS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB250CreatePaReferrals(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB250CreatePaReferrals.
  /// </summary>
  public SiB250CreatePaReferrals(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ************************************************************
    // Date      Developer      Description
    // 06/96	  J Howard       Initial development
    // 01/21/97  Sid Chowdhary	 Modification to the Assignment logic.
    // 			 No Changes has been made to the rest of                         			 
    // the PRAD.
    // 05/02/97  JeHoward       Remove required fields for pgm-code =
    // 			 'FC'.
    // 07/02/97  JeHoward       Remove requirement for APs First and Last
    //                          Name (allow "unkown" APs to exist).
    //                          PR#24280
    // 07/02/97  JeHoward       Check FROM to allow AE and KsCares
    //                          referrals with same case numbers.
    //                          PR#24382
    // 09/18/97  JeHoward       Pass AR Person Number to Create
    //                          PA Referral for Create Infrastructure
    //                          to use when creating Event History.
    // 10/17/97   JeHoward	Add more info to the Key Info on errors.
    // 12/17/97   JeHoward	Allow Absence Code to be blank.
    // 			Remove code that changed FC/GA to NF.
    // 10/21/98   C. Ott       Modify for DIR Standards.  Add CAB_WRITE_ERROR 
    // and
    //                         CAB_WRITE_CONTROL
    // ************************************************************
    // ***************************************************************
    // 7/22/1999   C. Ott    'CI' like 'FC' program code does not require an '
    // AR'.
    //                       Added condition to logic.
    // ***************************************************************
    // ***************************************************************
    // 8/9/99  C. Ott  Problem report # 69594, If the PI (AR) Person number is 
    // the same as the Person number of the child, do not create AR Referral
    // Participant.  If the Person numbers are different, create the AR Referral
    // Participant.
    // **************************************************************
    // ***************************************************************
    // 8/22/1999   C. Ott    Added sort by descending type
    // ***************************************************************
    // ***************************************************************
    // 10/05/2000  M. Lachowicz    Turn off validation for
    //                             type of refferal.
    // ***************************************************************
    // ***************************************************************
    // 02/02/2001  M. Lachowicz    Fixed error report
    //                             PR 112835.
    // ***************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    // ***** Get the run parameters for this program.
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // *****  OK.  Continue processing.
    }
    else
    {
      return;
    }

    // ***** Get the DB2 commit frequency counts.
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmCheckpointRestart();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // *****  OK.  Continue processing.
    }
    else
    {
      return;
    }

    // ***********************************************************
    // Before each commit, the following SET statement captures the latest 
    // accessed Interface PA Referral.
    // **********************************************************
    local.Restart.InterfaceIdentifier =
      local.ProgramCheckpointRestart.RestartInfo ?? Spaces(10);

    // **********************************************************
    // Process the selected records in groups based upon the commit frequencies.
    // Do a DB2 commit at the end of each group.
    // ************************************************************
    local.ProgramCheckpointRestart.CheckpointCount = 0;

    // ****************************************************************
    // OPEN OUTPUT ERROR REPORT 99
    // ****************************************************************
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    local.EabReportSend.ProgramName = local.ProgramProcessingInfo.Name;
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ****************************************************************
    // OPEN OUTPUT CONTROL REPORT 99
    // ****************************************************************
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // **************************************************************
    //      This is the edit and validation section.
    //      All checking will be done for the entire referral;
    //      if there are errors, the referral will be "suspended",
    //      the program errors will be created and processing will
    //      continue with the next PA Referral.  Otherwise, processing
    //      will continue and the PA Referral will be created.
    // **************************************************************
    foreach(var item in ReadInterfacePaReferral())
    {
      local.ErrorCode.Text1 = "";

      if (Equal(entities.InterfacePaReferral.LastUpdatedBy, global.UserId))
      {
        continue;
      }

      ++local.NumberOfReads.Count;

      // ****************************************************************
      // Accumulate control totals for Interface PA referrals read
      // ****************************************************************
      ++local.ReadReferralsCount.Count;

      switch(TrimEnd(entities.InterfacePaReferral.Type1))
      {
        case "":
          local.ErrorCode.Text1 = "Y";
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "REFERRAL FROM " + entities
            .InterfacePaReferral.From + ", REFERRAL NUMBER " + entities
            .InterfacePaReferral.Number + " IS MISSING 'TYPE'.";
          UseCabErrorReport1();

          if (Equal(local.EabFileHandling.Status, "OK"))
          {
            // *****  OK.  Continue processing.
          }
          else
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            goto ReadEach2;
          }

          break;
        case "NEW":
          break;
        case "CHANGE":
          break;
        case "REOPEN":
          break;
        default:
          // ****************************************************************
          //    Interface PA referral Type is invalid.
          // ****************************************************************
          local.ErrorCode.Text1 = "Y";
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "REFERRAL FROM " + entities
            .InterfacePaReferral.From + ", REFERRAL NUMBER " + entities
            .InterfacePaReferral.Number + " HAS INVALID 'TYPE' = " + entities
            .InterfacePaReferral.Type1;
          UseCabErrorReport1();

          if (Equal(local.EabFileHandling.Status, "OK"))
          {
            // *****  OK.  Continue processing.
          }
          else
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            goto ReadEach2;
          }

          break;
      }

      if (IsEmpty(entities.InterfacePaReferral.CaseNumber))
      {
        local.ErrorCode.Text1 = "Y";
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "REFERRAL FROM " + entities
          .InterfacePaReferral.From + ", REFERRAL NUMBER " + entities
          .InterfacePaReferral.Number + " IS MISSING 'CASE NUMBER'.";
        UseCabErrorReport1();

        if (Equal(local.EabFileHandling.Status, "OK"))
        {
          // *****  OK.  Continue processing.
        }
        else
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          break;
        }
      }

      switch(TrimEnd(entities.InterfacePaReferral.From))
      {
        case "":
          local.ErrorCode.Text1 = "Y";
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "INTERFACE PA REFERRAL NUMBER " + entities
            .InterfacePaReferral.Number + " IS MISSING 'FROM'.";
          UseCabErrorReport1();

          if (Equal(local.EabFileHandling.Status, "OK"))
          {
            // *****  OK.  Continue processing.
          }
          else
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            goto ReadEach2;
          }

          break;
        case "KAE":
          break;
        case "KSC":
          break;
        default:
          // ****************************************************************
          //    Interface PA referral From is invalid.
          // ****************************************************************
          local.ErrorCode.Text1 = "Y";
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "INTERFACE PA REFERRAL NUMBER " + entities
            .InterfacePaReferral.Number + " HAS INVALID 'FROM' = " + entities
            .InterfacePaReferral.From;
          UseCabErrorReport1();

          if (Equal(local.EabFileHandling.Status, "OK"))
          {
            // *****  OK.  Continue processing.
          }
          else
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            goto ReadEach2;
          }

          break;
      }

      if (Equal(entities.InterfacePaReferral.ApprovalDate, local.Null1.Date))
      {
        local.ErrorCode.Text1 = "Y";
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "INTERFACE PA REFERRAL NUMBER " + entities
          .InterfacePaReferral.Number + " HAS INVALID 'APPROVAL DATE' = " + NumberToString
          (DateToInt(entities.InterfacePaReferral.ApprovalDate), 15);
        UseCabErrorReport1();

        if (Equal(local.EabFileHandling.Status, "OK"))
        {
          // *****  OK.  Continue processing.
        }
        else
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          break;
        }
      }

      if (IsEmpty(entities.InterfacePaReferral.PgmCode))
      {
        local.ErrorCode.Text1 = "Y";
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "REFERRAL FROM " + entities
          .InterfacePaReferral.From + ", REFERRAL NUMBER " + entities
          .InterfacePaReferral.Number + " IS MISSING 'PGM CODE'.";
        UseCabErrorReport1();

        if (Equal(local.EabFileHandling.Status, "OK"))
        {
          // *****  OK.  Continue processing.
        }
        else
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          break;
        }
      }
      else
      {
        local.Code.CodeName = "PROGRAM";
        local.CodeValue.Cdvalue = entities.InterfacePaReferral.PgmCode ?? Spaces
          (10);
        UseCabValidateCodeValue();

        if (AsChar(local.ValidCode.Flag) == 'N')
        {
          local.ErrorCode.Text1 = "Y";

          // ****************************************************************
          //    Interface PA Program Code is invalid.
          // ****************************************************************
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "REFERRAL FROM " + entities
            .InterfacePaReferral.From + ", REFERRAL NUMBER " + " HAS INVALID 'PGM CODE' = " +
            entities.InterfacePaReferral.PgmCode + ".";
          UseCabErrorReport1();

          if (Equal(local.EabFileHandling.Status, "OK"))
          {
            // *****  OK.  Continue processing.
          }
          else
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            break;
          }
        }
      }

      if (IsEmpty(entities.InterfacePaReferral.KsCounty))
      {
        local.ErrorCode.Text1 = "Y";
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "REFERRAL FROM " + entities
          .InterfacePaReferral.From + ", REFERRAL NUMBER " + entities
          .InterfacePaReferral.Number + " IS MISSING 'KS COUNTY CODE'.";
        UseCabErrorReport1();

        if (Equal(local.EabFileHandling.Status, "OK"))
        {
          // *****  OK.  Continue processing.
        }
        else
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          break;
        }
      }
      else
      {
        local.Code.CodeName = "COUNTY CODE";
        local.CodeValue.Cdvalue = entities.InterfacePaReferral.KsCounty ?? Spaces
          (10);
        UseCabValidateCodeValue();

        if (AsChar(local.ValidCode.Flag) == 'N')
        {
          // ****************************************************************
          //    Interface PA County Code is invalid.
          // ****************************************************************
          local.ErrorCode.Text1 = "Y";
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "REFERRAL FROM " + entities
            .InterfacePaReferral.From + ", REFERRAL NUMBER " + " HAS INVALID 'KS COUNTY CODE'" +
            " = " + entities.InterfacePaReferral.KsCounty;
          UseCabErrorReport1();

          if (Equal(local.EabFileHandling.Status, "OK"))
          {
            // *****  OK.  Continue processing.
          }
          else
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            break;
          }
        }
      }

      if (IsEmpty(entities.InterfacePaReferral.Number))
      {
        local.ErrorCode.Text1 = "Y";
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "REFERRAL FROM " + entities
          .InterfacePaReferral.From + ", CASE NUMBER " + entities
          .InterfacePaReferral.CaseNumber + " IS MISSING 'REFERRAL NUMBER'.";
        UseCabErrorReport1();

        if (Equal(local.EabFileHandling.Status, "OK"))
        {
          // *****  OK.  Continue processing.
        }
        else
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          break;
        }
      }
      else
      {
        // 10/05/2000 M.L start
        // 10/05/2000 M.L End
      }

      if (ReadInterfacePaReferralParticipnt1())
      {
        // ****************************************************
        // Hold the AR person number to pass to Create PA
        // Referral so that when the infrastructure record
        // is created, it is related to the AR.
        // ****************************************************
        local.HoldArNumber.PersonNumber = entities.Ar.PersonNumber;
        ++local.NumberOfReads.Count;

        // ****************************************************************
        //  Accumulate control totals.
        // ****************************************************************
        ++local.ReadRefParticipantCount.Count;
        local.Ar.Assign(entities.Ar);

        if (IsEmpty(entities.Ar.FirstName))
        {
          local.ErrorCode.Text1 = "Y";
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "REFERRAL PATICIPANT NUMBER " + entities
            .Ar.PersonNumber + ", IS MISSING FIRST NAME FOR 'AR' PARTICIPANT.";
          UseCabErrorReport1();

          if (Equal(local.EabFileHandling.Status, "OK"))
          {
            // *****  OK.  Continue processing.
          }
          else
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            break;
          }
        }

        if (IsEmpty(entities.Ar.LastName))
        {
          local.ErrorCode.Text1 = "Y";
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "REFERRAL PATICIPANT NUMBER " + entities
            .Ar.PersonNumber + ", IS MISSING LAST NAME FOR 'AR' PARTICIPANT.";
          UseCabErrorReport1();

          if (Equal(local.EabFileHandling.Status, "OK"))
          {
            // *****  OK.  Continue processing.
          }
          else
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            break;
          }
        }
      }
      else
      {
        switch(TrimEnd(entities.InterfacePaReferral.PgmCode))
        {
          case "FC":
            // ***************************************************************
            //      For a Foster Care referral, we do not need
            //      an AR ( AE can't send us Orgs at this time).
            //      State of KS will be set up as AR if case is registered.
            // ***************************************************************
            break;
          case "CI":
            // ***************************************************************
            // 7/22/1999   C. Ott   'CI' program code should be treated the same
            // as 'FC'.
            // ***************************************************************
            break;
          default:
            // **************************************************************
            //      Program error code from code table for no AR
            //      on non-FC Referral.
            // **************************************************************
            local.ErrorCode.Text1 = "Y";
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = "PA REFERRAL " + entities
              .InterfacePaReferral.Number + ", HAS NO 'AR' ROLE AND PROGRAM CODE IS NOT 'FC'.";
              
            UseCabErrorReport1();

            if (Equal(local.EabFileHandling.Status, "OK"))
            {
              // *****  OK.  Continue processing.
            }
            else
            {
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              break;
            }

            break;
        }
      }

      local.ChCount.Count = 0;

      foreach(var item1 in ReadInterfacePaReferralParticipnt2())
      {
        ++local.NumberOfReads.Count;

        // ***************************************************************
        //   Accumulate control totals.
        // ***************************************************************
        ++local.ReadRefParticipantCount.Count;
        ++local.ChCount.Count;

        if (Equal(entities.InterfacePaReferral.From, "KAE") && !
          IsEmpty(entities.Ch.AbsenceCode))
        {
          local.Code.CodeName = "ABSENCE";
          local.CodeValue.Cdvalue = entities.Ch.AbsenceCode ?? Spaces(10);
          UseCabValidateCodeValue();

          if (AsChar(local.ValidCode.Flag) == 'N')
          {
            // ****************************************************************
            //      Program error code from code table for invalid
            //      Absence Code.
            // ****************************************************************
            // 02/02/2001 M.L Start
            local.EabReportSend.RptDetail = "REFERRAL PATICIPANT NUMBER " + entities
              .Ch.PersonNumber + ", HAS INVALID ABSENCE CODE = " + entities
              .Ch.AbsenceCode;

            // 02/02/2001 M.L End
            local.EabFileHandling.Action = "WRITE";
            UseCabErrorReport1();

            if (Equal(local.EabFileHandling.Status, "OK"))
            {
              // *****  OK.  Continue processing.
            }
            else
            {
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              goto ReadEach2;
            }
          }
        }

        if (IsEmpty(entities.Ch.FirstName))
        {
          local.ErrorCode.Text1 = "Y";
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "REFERRAL PATICIPANT NUMBER " + entities
            .Ch.PersonNumber + ", IS MISSING FIRST NAME FOR 'CH' PARTICIPANT.";
          UseCabErrorReport1();

          if (Equal(local.EabFileHandling.Status, "OK"))
          {
            // *****  OK.  Continue processing.
          }
          else
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            goto ReadEach2;
          }
        }

        if (IsEmpty(entities.Ch.LastName))
        {
          local.ErrorCode.Text1 = "Y";
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "REFERRAL PATICIPANT NUMBER " + entities
            .Ch.PersonNumber + ", IS MISSING LAST NAME FOR 'CH' PARTICIPANT.";
          UseCabErrorReport1();

          if (Equal(local.EabFileHandling.Status, "OK"))
          {
            // *****  OK.  Continue processing.
          }
          else
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            goto ReadEach2;
          }
        }

        // ***************************************************************
        // 8/9/99  C. Ott  Problem report # 69594, If the PI (AR) Person number 
        // is the same as the Person number of the child, do not create AR
        // Referral Participant.  If the Person numbers are different, create
        // the AR Referral Participant.
        // **************************************************************
        if (Equal(entities.InterfacePaReferral.PgmCode, "CI"))
        {
          if (Equal(entities.Ch.PersonNumber, local.HoldArNumber.PersonNumber))
          {
            local.SuppressArRelationship.Flag = "Y";
          }
        }
      }

      if (local.ChCount.Count < 1)
      {
        // ****************************************************************
        //    Program error code from code table for no CH on  Referral.
        // ****************************************************************
        local.ErrorCode.Text1 = "Y";
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "PA REFERRAL NUMBER " + entities
          .InterfacePaReferral.Number + ", HAS NO 'CH' ROLE PARTICIPANT.";
        UseCabErrorReport1();

        if (Equal(local.EabFileHandling.Status, "OK"))
        {
          // *****  OK.  Continue processing.
        }
        else
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          break;
        }
      }

      local.Ap.Count = 0;

      foreach(var item1 in ReadInterfacePaReferralParticipnt3())
      {
        ++local.NumberOfReads.Count;

        if (Equal(entities.Other.Role, "AP"))
        {
          // ****************************************************************
          //      Count the number of APs - to be used when assigning
          //      the referral.
          // ****************************************************************
          ++local.Ap.Count;
        }

        // ****************************************************************
        //   Accumulate control totals.
        // ****************************************************************
        ++local.ReadRefParticipantCount.Count;

        // *******************************************************
        // APs can be sent over with partial names or "Unknown"
        // in a name field.  If this happens, the AP Count must
        // be set to zero so that referral assignment will take
        // place using the AR's name.
        // All other names cannot be blank.
        // *******************************************************
        if (IsEmpty(entities.Other.FirstName) || IsEmpty
          (entities.Other.LastName))
        {
          if (Equal(entities.Other.Role, "AP"))
          {
            local.Ap.Count = 0;
          }
          else
          {
            if (IsEmpty(entities.Other.FirstName))
            {
              local.ErrorCode.Text1 = "Y";
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail = "REFERRAL PATICIPANT NUMBER " + entities
                .Other.PersonNumber + ", IS MISSING FIRST NAME.";
              UseCabErrorReport1();

              if (Equal(local.EabFileHandling.Status, "OK"))
              {
                // *****  OK.  Continue processing.
              }
              else
              {
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                goto ReadEach2;
              }
            }

            if (IsEmpty(entities.Other.LastName))
            {
              local.ErrorCode.Text1 = "Y";
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail = "REFERRAL PATICIPANT NUMBER " + entities
                .Other.PersonNumber + ", IS MISSING LAST NAME.";
              UseCabErrorReport1();

              if (Equal(local.EabFileHandling.Status, "OK"))
              {
                // *****  OK.  Continue processing.
              }
              else
              {
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                goto ReadEach2;
              }
            }
          }
        }

        if (!Equal(entities.Other.Role, "AP") && !
          Equal(entities.Other.Role, "CP"))
        {
          // ***************************************************************
          //      Program error code for invalid ROLE.
          // ***************************************************************
          local.ErrorCode.Text1 = "Y";
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "REFERRAL PATICIPANT NUMBER " + entities
            .Other.PersonNumber + ", HAS INVALID ROLE = " + entities
            .Other.Role;
          UseCabErrorReport1();

          if (Equal(local.EabFileHandling.Status, "OK"))
          {
            // *****  OK.  Continue processing.
          }
          else
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            goto ReadEach2;
          }
        }
      }

      foreach(var item1 in ReadInterfacePaParticipantAddress2())
      {
        ++local.NumberOfReads.Count;

        // ****************************************************************
        //  Accumulate control totals.
        // ****************************************************************
        ++local.ReadRefPartAddrCount.Count;

        switch(AsChar(entities.InterfacePaParticipantAddress.Type1))
        {
          case 'R':
            break;
          case 'M':
            break;
          default:
            // ***************************************************************
            //      Program error code for invalid ADDRESS TYPE.
            // ***************************************************************
            local.ErrorCode.Text1 = "Y";
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = "PA REFERRAL NUMBER " + entities
              .InterfacePaReferral.Number + ", PARTICIPANT NUMBER " + NumberToString
              (entities.InterfacePaParticipantAddress.ParticipantIdentifier, 13,
              3) + " HAS AN INVALID ADDRESS TYPE = " + entities
              .InterfacePaParticipantAddress.Type1;
            UseCabErrorReport1();

            if (Equal(local.EabFileHandling.Status, "OK"))
            {
              // *****  OK.  Continue processing.
            }
            else
            {
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              goto ReadEach2;
            }

            break;
        }
      }

      if (!IsEmpty(local.ErrorCode.Text1))
      {
        // ****************************************************************
        // Error(s) found and written to error report.  Process next record.
        // ****************************************************************
        continue;
      }

      // ***************************************************************
      //      Process the PA Referral.
      //      Create the PA Referral and at least one Participant
      //      Set the PA Referral attributes
      // ***************************************************************
      local.PaReferral.CseInvolvementInd =
        entities.InterfacePaReferral.CseInvolvementInd;
      local.PaReferral.ApAreaCode = entities.InterfacePaReferral.ApAreaCode;
      local.PaReferral.ApEmployerName =
        entities.InterfacePaReferral.ApEmployerName;
      local.PaReferral.ApEmployerPhone =
        entities.InterfacePaReferral.ApEmployerPhone;
      local.PaReferral.ApPhoneNumber =
        entities.InterfacePaReferral.ApPhoneNumber;
      local.PaReferral.AssignmentDate =
        entities.InterfacePaReferral.ApprovalDate;
      local.PaReferral.ArEmployerName =
        entities.InterfacePaReferral.ArEmployerName;
      local.PaReferral.ArRetainedInd =
        entities.InterfacePaReferral.ArRetainedInd;
      local.PaReferral.AssignDeactivateDate =
        entities.InterfacePaReferral.AssignDeactivateDate;
      local.PaReferral.AssignDeactivateInd =
        entities.InterfacePaReferral.AssignDeactivateInd;
      local.PaReferral.CaseNumber = entities.InterfacePaReferral.CaseNumber;
      local.PaReferral.CaseWorker = entities.InterfacePaReferral.CaseWorker;
      local.PaReferral.CcStartDate = entities.InterfacePaReferral.CcStartDate;
      local.PaReferral.CreatedBy = global.UserId;
      local.PaReferral.CreatedTimestamp = Now();
      local.PaReferral.CsArrearageAmt =
        entities.InterfacePaReferral.CsArrearageAmt;
      local.PaReferral.CsFreq = entities.InterfacePaReferral.CsFreq;
      local.PaReferral.CsLastPaymentAmt =
        entities.InterfacePaReferral.CsLastPaymentAmt;
      local.PaReferral.CsOrderPlace = entities.InterfacePaReferral.CsOrderPlace;
      local.PaReferral.CsOrderState = entities.InterfacePaReferral.CsOrderState;
      local.PaReferral.CsPaymentAmount =
        entities.InterfacePaReferral.CsPaymentAmount;
      local.PaReferral.CseReferralRecDate =
        entities.InterfacePaReferral.CseReferralRecDate;
      local.PaReferral.FcAdoptionDisruptionInd =
        entities.InterfacePaReferral.FcAdoptionDisruptionInd;
      local.PaReferral.FcApNotified = entities.InterfacePaReferral.FcApNotified;
      local.PaReferral.FcCincInd = entities.InterfacePaReferral.FcCincInd;
      local.PaReferral.FcCostOfCare = entities.InterfacePaReferral.FcCostOfCare;
      local.PaReferral.FcCostOfCareFreq =
        entities.InterfacePaReferral.FcCostOfCareFreq;
      local.PaReferral.FcCountyChildRemovedFrom =
        entities.InterfacePaReferral.FcCountyChildRemovedFrom;
      local.PaReferral.FcDateOfInitialCustody =
        entities.InterfacePaReferral.FcDateOfInitialCustody;
      local.PaReferral.FcIvECaseNumber =
        entities.InterfacePaReferral.FcIvECaseNumber;
      local.PaReferral.FcJuvenileCourtOrder =
        entities.InterfacePaReferral.FcJuvenileCourtOrder;
      local.PaReferral.FcJuvenileCourtOrder =
        entities.InterfacePaReferral.FcJuvenileCourtOrder;
      local.PaReferral.FcJuvenileOffenderInd =
        entities.InterfacePaReferral.FcJuvenileOffenderInd;
      local.PaReferral.FcNextJuvenileCtDt =
        entities.InterfacePaReferral.FcNextJuvenileCtDt;
      local.PaReferral.FcOrderEstBy = entities.InterfacePaReferral.FcOrderEstBy;
      local.PaReferral.FcOtherBenefitInd =
        entities.InterfacePaReferral.FcOtherBenefitInd;
      local.PaReferral.FcPlacementDate =
        entities.InterfacePaReferral.FcPlacementDate;
      local.PaReferral.FcPlacementName =
        entities.InterfacePaReferral.FcPlacementName;
      local.PaReferral.FcPlacementType =
        entities.InterfacePaReferral.FcPlacementType;
      local.PaReferral.FcPreviousPa = entities.InterfacePaReferral.FcPreviousPa;
      local.PaReferral.FcRightsSevered =
        entities.InterfacePaReferral.FcRightsSevered;
      local.PaReferral.FcSourceOfFunding =
        entities.InterfacePaReferral.FcSourceOfFunding;
      local.PaReferral.FcSrsPayee = entities.InterfacePaReferral.FcSrsPayee;
      local.PaReferral.FcSsa = entities.InterfacePaReferral.FcSsa;
      local.PaReferral.FcSsi = entities.InterfacePaReferral.FcSsi;
      local.PaReferral.FcVaInd = entities.InterfacePaReferral.FcVaInd;
      local.PaReferral.FcWardsAccount =
        entities.InterfacePaReferral.FcWardsAccount;
      local.PaReferral.FcZebInd = entities.InterfacePaReferral.FcZebInd;
      local.PaReferral.From = entities.InterfacePaReferral.From;
      local.PaReferral.GoodCauseCode =
        entities.InterfacePaReferral.GoodCauseCode;
      local.PaReferral.GoodCauseDate =
        entities.InterfacePaReferral.GoodCauseDate;
      local.PaReferral.KsCounty = entities.InterfacePaReferral.KsCounty;
      local.PaReferral.LastApContactDate =
        entities.InterfacePaReferral.LastApContactDate;
      local.PaReferral.LastPaymentDate =
        entities.InterfacePaReferral.LastPaymentDate;
      local.PaReferral.LastUpdatedBy = global.UserId;
      local.PaReferral.LastUpdatedTimestamp = Now();
      local.PaReferral.MedicalAmt = entities.InterfacePaReferral.MedicalAmt;
      local.PaReferral.MedicalArrearage =
        entities.InterfacePaReferral.MedicalArrearage;
      local.PaReferral.MedicalFreq = entities.InterfacePaReferral.MedicalFreq;
      local.PaReferral.MedicalInsuranceCo =
        entities.InterfacePaReferral.MedicalInsuranceCo;
      local.PaReferral.MedicalLastPayment =
        entities.InterfacePaReferral.MedicalLastPayment;
      local.PaReferral.MedicalLastPaymentDate =
        entities.InterfacePaReferral.MedicalLastPaymentDate;
      local.PaReferral.MedicalOrderEffectiveDate =
        entities.InterfacePaReferral.MedicalOrderEffectiveDate;
      local.PaReferral.MedicalOrderInd =
        entities.InterfacePaReferral.MedicalOrderInd;
      local.PaReferral.MedicalOrderNumber =
        entities.InterfacePaReferral.MedicalOrderNumber;
      local.PaReferral.MedicalOrderPlace =
        entities.InterfacePaReferral.MedicalOrderPlace;
      local.PaReferral.MedicalOrderState =
        entities.InterfacePaReferral.MedicalOrderState;
      local.PaReferral.MedicalPaidTo =
        entities.InterfacePaReferral.MedicalPaidTo;
      local.PaReferral.MedicalPaymentDueDate =
        entities.InterfacePaReferral.MedicalPaymentDueDate;
      local.PaReferral.MedicalPaymentType =
        entities.InterfacePaReferral.MedicalPaymentType;
      local.PaReferral.MedicalPolicyNumber =
        entities.InterfacePaReferral.MedicalPolicyNumber;
      local.PaReferral.Note = entities.InterfacePaReferral.Note;
      local.PaReferral.Number = entities.InterfacePaReferral.Number;
      local.PaReferral.OrderEffectiveDate =
        entities.InterfacePaReferral.OrderEffectiveDate;
      local.PaReferral.PaymentDueDate =
        entities.InterfacePaReferral.PaymentDueDate;
      local.PaReferral.PaymentMadeTo =
        entities.InterfacePaReferral.PaymentMadeTo;
      local.PaReferral.PgmCode = entities.InterfacePaReferral.PgmCode;
      local.PaReferral.ReceivedDate = entities.InterfacePaReferral.ReceivedDate;
      local.PaReferral.SupportOrderFreq =
        entities.InterfacePaReferral.SupportOrderFreq;
      local.PaReferral.SupportOrderId =
        entities.InterfacePaReferral.SupportOrderId;
      local.PaReferral.Type1 = entities.InterfacePaReferral.Type1;
      local.PaReferral.VoluntarySupportInd =
        entities.InterfacePaReferral.VoluntarySupportInd;
      local.Participant.Count = 0;

      foreach(var item1 in ReadInterfacePaReferralParticipnt4())
      {
        ++local.Participant.Count;
        local.PaReferralParticipant.AbsenceCode = entities.Other.AbsenceCode;

        // 02/02/01 M.L Start
        if (Equal(entities.InterfacePaReferral.From, "KAE") && Equal
          (entities.Other.Role, "CH") && !IsEmpty(entities.Other.AbsenceCode))
        {
          local.Code.CodeName = "ABSENCE";
          local.CodeValue.Cdvalue = entities.Other.AbsenceCode ?? Spaces(10);
          UseCabValidateCodeValue();

          if (AsChar(local.ValidCode.Flag) == 'N')
          {
            local.PaReferralParticipant.AbsenceCode = "";
          }
        }

        // 02/02/01 M.L End
        local.PaReferralParticipant.BeneInd = entities.Other.BeneInd;
        local.PaReferralParticipant.CreatedBy = global.UserId;
        local.PaReferralParticipant.CreatedTimestamp = Now();
        local.PaReferralParticipant.Dob = entities.Other.Dob;
        local.PaReferralParticipant.FirstName = entities.Other.FirstName;
        local.PaReferralParticipant.GoodCauseStatus =
          entities.Other.GoodCauseStatus;
        local.PaReferralParticipant.Identifier = entities.Other.Identifier;
        local.PaReferralParticipant.InsurInd = entities.Other.InsurInd;
        local.PaReferralParticipant.LastName = entities.Other.LastName;
        local.PaReferralParticipant.LastUpdatedBy = "";
        local.PaReferralParticipant.LastUpdatedTimestamp = Now();
        local.PaReferralParticipant.Mi = entities.Other.Mi;
        local.PaReferralParticipant.PatEstInd = entities.Other.PatEstInd;
        local.PaReferralParticipant.PersonNumber = entities.Other.PersonNumber;
        local.PaReferralParticipant.Relationship = entities.Other.Relationship;
        local.PaReferralParticipant.Sex = entities.Other.Sex;
        local.PaReferralParticipant.Ssn = entities.Other.Ssn;
        local.PaReferralParticipant.Role = entities.Other.Role;

        if (local.Participant.Count == 1)
        {
          UseSiCreatePaReferral();

          if (IsExitState("PA_REFERRAL_NU") || IsExitState("PA_REFERRAL_PV"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = "PA REFERRAL FROM " + entities
              .InterfacePaReferral.From + ", REFERRAL NUMBER " + entities
              .InterfacePaReferral.Number + " ENCOUNTERD AN ERROR AND DID NOT CREATE OF PA REFERRAL";
              
            UseCabErrorReport1();

            if (Equal(local.EabFileHandling.Status, "OK"))
            {
              // *****  OK.  Continue processing.
            }
            else
            {
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              goto ReadEach2;
            }

            goto ReadEach1;
          }

          if (IsExitState("PA_REFERRAL_PARTICIPANT_NU") || IsExitState
            ("PA_REFERRAL_PARTICIPANT_PV"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = "PA REFERRAL FROM " + entities
              .InterfacePaReferral.From + ", REFERRAL NUMBER " + entities
              .InterfacePaReferral.Number + ", PARTICIPANT NUMBER " + " ENCOUNTERD AN ERROR AND DID NOT CREATE OF PA REFERRAL PARTICIPANT.  PROGRAM TERMINATED, MUST RESTART.";
              
            UseCabErrorReport1();

            if (Equal(local.EabFileHandling.Status, "OK"))
            {
              // *****  OK.  Continue processing.
            }
            else
            {
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              goto ReadEach2;
            }

            goto ReadEach1;
          }

          ++local.CreateReferralCount.Count;
          ++local.CreateReferralPartCount.Count;
        }
        else
        {
          ExitState = "ACO_NN0000_ALL_OK";
          UseSiCreatePaReferralParticipnt();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ++local.CreateReferralPartCount.Count;
          }
          else
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = "PA REFERRAL FROM " + entities
              .InterfacePaReferral.From + ", REFERRAL NUMBER " + entities
              .InterfacePaReferral.Number + ", PARTICIPANT NUMBER " + " ENCOUNTERD AN ERROR AND DID NOT CREATE OF PA REFERRAL PARTICIPANT.";
              
            UseCabErrorReport1();

            if (Equal(local.EabFileHandling.Status, "OK"))
            {
              // *****  OK.  Continue processing.
            }
            else
            {
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              goto ReadEach2;
            }

            goto ReadEach1;
          }
        }

        if (ReadInterfacePaParticipantAddress1())
        {
          local.AddrExists.Flag = "Y";
          local.PaParticipantAddress.City =
            entities.InterfacePaParticipantAddress.City;
          local.PaParticipantAddress.CreatedBy = global.UserId;
          local.PaParticipantAddress.CreatedTimestamp = Now();
          local.PaParticipantAddress.Identifier =
            entities.InterfacePaParticipantAddress.Identifier;
          local.PaParticipantAddress.LastUpdatedBy = global.UserId;
          local.PaParticipantAddress.LastUpdatedTimestamp = Now();
          local.PaParticipantAddress.State =
            entities.InterfacePaParticipantAddress.State;
          local.PaParticipantAddress.Street1 =
            entities.InterfacePaParticipantAddress.Street1;
          local.PaParticipantAddress.Street2 =
            entities.InterfacePaParticipantAddress.Street2;
          local.PaParticipantAddress.Type1 =
            entities.InterfacePaParticipantAddress.Type1;
          local.PaParticipantAddress.Zip =
            entities.InterfacePaParticipantAddress.Zip;
          local.PaParticipantAddress.Zip3 =
            entities.InterfacePaParticipantAddress.Zip3;
          local.PaParticipantAddress.Zip4 =
            entities.InterfacePaParticipantAddress.Zip4;
          ExitState = "ACO_NN0000_ALL_OK";
          UseSiCreatePaParticipantAddress();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ++local.CreateRefPartAddrCount.Count;
          }
          else
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = "PA REFERRAL NUMBER " + entities
              .InterfacePaReferral.Number + ", PARTICIPANT NUMBER " + NumberToString
              (entities.InterfacePaParticipantAddress.ParticipantIdentifier, 15) +
              ", ENCOUNTERED ERROR AND DID NOT CREATE ADDRESS FOR TYPE = " + entities
              .InterfacePaParticipantAddress.Type1;
            UseCabErrorReport1();

            if (Equal(local.EabFileHandling.Status, "OK"))
            {
              // *****  OK.  Continue processing.
            }
            else
            {
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              goto ReadEach2;
            }
          }
        }
        else
        {
          local.AddrExists.Flag = "N";
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // *****  OK.  Continue processing.
          // ***************************************************************
          //      The PA Referral was created on the first Participant.
          // ***************************************************************
          if (local.Participant.Count == 1)
          {
            local.NumberOfUpdates.Count += 2;
          }
          else
          {
            ++local.NumberOfUpdates.Count;
          }

          if (AsChar(local.AddrExists.Flag) == 'Y')
          {
            ++local.NumberOfUpdates.Count;
          }
        }
        else
        {
          goto ReadEach2;
        }
      }

      // *********************************************
      // Call the CAB to assign the newly created PA
      // Referral to the desired OSP.
      // *********************************************
      ExitState = "ACO_NN0000_ALL_OK";
      UseSiAssignPaReferralToOsp();

      if (IsExitState("ACO_NN0000_ALL_OK") && IsEmpty(local.ErrorCode.Text8))
      {
        ++local.AssignPaRefToOspCount.Count;
        UpdateInterfacePaReferral();
      }
      else
      {
        // **************************************************************
        // The following are error codes that may be returned from the 
        // Assignment CAB.  This section formats the Error Report message.
        // **************************************************************
        switch(TrimEnd(local.ErrorCode.Text8))
        {
          case "NOOFFICE":
            local.EabReportSend.RptDetail = "REFERRAL FROM " + entities
              .InterfacePaReferral.From + ", REFERRAL NUMBER " + entities
              .InterfacePaReferral.Number + " COULD NOT ASSIGN PA REFERRAL TO OSP; NO OFFICE FOUND FOR ASSIGNMENT";
              

            break;
          case "NOPGMALP":
            local.EabReportSend.RptDetail = "REFERRAL FROM " + entities
              .InterfacePaReferral.From + ", REFERRAL NUMBER " + entities
              .InterfacePaReferral.Number + " COULD NOT ASSIGN PA REFERRAL TO OSP; NO SERVICE FOUND FOR REFERRAL PROGRAM CODE.";
              

            break;
          case "NOCSEORG":
            local.EabReportSend.RptDetail = "REFERRAL FROM " + entities
              .InterfacePaReferral.From + ", REFERRAL NUMBER " + entities
              .InterfacePaReferral.Number + " COULD NOT ASSIGN PA REFERRAL TO OSP; NO CSE ORGANIZATION FOUND FOR PA REFERRAL KS COUNTY CODE.";
              

            break;
          case "PAREF_NF":
            local.EabReportSend.RptDetail = "REFERRAL FROM " + entities
              .InterfacePaReferral.From + ", REFERRAL NUMBER " + entities
              .InterfacePaReferral.Number + " COULD NOT ASSIGN PA REFERRAL TO OSP; NO PA REFERRAL FOUND.";
              

            break;
          case "APPARTNF":
            local.EabReportSend.RptDetail = "REFERRAL FROM " + entities
              .InterfacePaReferral.From + ", REFERRAL NUMBER " + entities
              .InterfacePaReferral.Number + " COULD NOT ASSIGN PA REFERRAL TO OSP; NO PA REFERRAL 'AP' PARTICIPANT FOUND.";
              

            break;
          case "CHPARTNF":
            local.EabReportSend.RptDetail = "REFERRAL FROM " + entities
              .InterfacePaReferral.From + ", REFERRAL NUMBER " + entities
              .InterfacePaReferral.Number + " COULD NOT ASSIGN PA REFERRAL TO OSP; NO PA REFERRAL 'CH' PARTICIPANT FOUND.";
              

            break;
          case "NOOFSRVP":
            local.EabReportSend.RptDetail = "REFERRAL FROM " + entities
              .InterfacePaReferral.From + ", REFERRAL NUMBER " + entities
              .InterfacePaReferral.Number + " COULD NOT ASSIGN PA REFERRAL TO OSP; NO OFFICE SERVICE PROVIDER FOUND.";
              

            break;
          default:
            local.EabReportSend.RptDetail = "REFERRAL FROM " + entities
              .InterfacePaReferral.From + ", REFERRAL NUMBER " + entities
              .InterfacePaReferral.Number + " COULD NOT ASSIGN PA REFERRAL TO OSP.";
              

            break;
        }

        local.EabFileHandling.Action = "WRITE";
        UseCabErrorReport1();

        if (Equal(local.EabFileHandling.Status, "OK"))
        {
          // *****  OK.  Continue processing.
        }
        else
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          break;
        }
      }

      // ***************************************************************
      //   Evaluate the commit count and determine if a commit should be taken.
      // **************************************************************
      if (local.NumberOfReads.Count >= local
        .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault() || local
        .NumberOfUpdates.Count >= local
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        // ****************************************************************
        //  Record the number of checkpoints and the last checkpoint time and 
        // set the restart indicator to yes.  Also return the checkpoint
        // frequency counts in case they been changed since the last read.
        // ***************************************************************
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.RestartInfo =
          entities.InterfacePaReferral.InterfaceIdentifier;
        local.ProgramCheckpointRestart.CheckpointCount =
          local.ProgramCheckpointRestart.CheckpointCount.GetValueOrDefault() + 1
          ;
        UseUpdatePgmCheckpointRestart2();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // *****  OK.  Continue processing.
        }
        else
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          break;
        }

        // **************************************************************
        //  Call an external that does a DB2 commit using a Cobol program.
        // **************************************************************
        UseExtToDoACommit();

        if (local.PassArea.NumericReturnCode != 0)
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "ERROR ENCOUNTERED DURING DATA BASE COMMIT.  PROGRAM TERMINATED.";
          UseCabErrorReport1();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          break;
        }

        // ****************************************************************
        //   Reinitialize the commit counters after every commit.
        // ****************************************************************
        local.NumberOfReads.Count = 0;
        local.NumberOfUpdates.Count = 0;
      }

ReadEach1:
      ;
    }

ReadEach2:

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // ****************************************************************
      //  Set restart indicator to "N" because we successfully finished this 
      // program.
      // ****************************************************************
      local.ProgramCheckpointRestart.RestartInfo = "";
      local.ProgramCheckpointRestart.RestartInd = "N";
      UseUpdatePgmCheckpointRestart1();
    }

    // ****************************************************************
    // WRITE OUTPUT CONTROL REPORT 98
    // ****************************************************************
    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail =
      "INTERFACE PA REFERRALS READ                       " + NumberToString
      (local.ReadReferralsCount.Count, 15);
    UseCabControlReport1();

    if (Equal(local.EabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }

    local.EabReportSend.RptDetail =
      "INTERFACE PA REFERRAL PARTICIPANTS READ           " + NumberToString
      (local.ReadRefParticipantCount.Count, 15);
    UseCabControlReport1();

    if (Equal(local.EabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }

    local.EabReportSend.RptDetail =
      "INTERFACE PA REFERRAL PARTICIPANT ADDRESSES READ  " + NumberToString
      (local.ReadRefPartAddrCount.Count, 15);
    UseCabControlReport1();

    if (Equal(local.EabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }

    local.EabReportSend.RptDetail =
      "PA REFERRALS CREATED                              " + NumberToString
      (local.CreateReferralCount.Count, 15);
    UseCabControlReport1();

    if (Equal(local.EabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }

    local.EabReportSend.RptDetail =
      "PA REFERRAL PARTICIPANTS CREATED                  " + NumberToString
      (local.CreateReferralPartCount.Count, 15);
    UseCabControlReport1();

    if (Equal(local.EabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }

    local.EabReportSend.RptDetail =
      "PA REFERRAL PARTICIPANT ADDRESSES CREATED         " + NumberToString
      (local.CreateRefPartAddrCount.Count, 15);
    UseCabControlReport1();

    if (Equal(local.EabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }

    local.EabReportSend.RptDetail =
      "PA REFERRALS ASSIGNED TO OFFICE SERVICE PROVIDER  " + NumberToString
      (local.AssignPaRefToOspCount.Count, 15);
    UseCabControlReport1();

    if (Equal(local.EabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }

    // ****************************************************************
    // CLOSE OUTPUT CONTROL REPORT 98
    // ****************************************************************
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport1();

    if (Equal(local.EabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "ERROR WRITING TO CONTROL REPORT.";
      UseCabErrorReport1();

      if (Equal(local.EabFileHandling.Status, "OK"))
      {
      }
      else
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      }
    }

    // ****************************************************************
    // CLOSE OUTPUT ERROR REPORT 99
    // ****************************************************************
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport1();

    if (Equal(local.EabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
    }
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MovePaReferral1(PaReferral source, PaReferral target)
  {
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.Number = source.Number;
    target.Type1 = source.Type1;
  }

  private static void MovePaReferral2(PaReferral source, PaReferral target)
  {
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.Number = source.Number;
    target.Type1 = source.Type1;
    target.PgmCode = source.PgmCode;
    target.KsCounty = source.KsCounty;
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

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Flag = useExport.ValidCode.Flag;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.PassArea.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.PassArea.NumericReturnCode = useExport.External.NumericReturnCode;
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

  private void UseSiAssignPaReferralToOsp()
  {
    var useImport = new SiAssignPaReferralToOsp.Import();
    var useExport = new SiAssignPaReferralToOsp.Export();

    useImport.Ap.Count = local.Ap.Count;
    MovePaReferral2(local.PaReferral, useImport.PaReferral);
    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;

    Call(SiAssignPaReferralToOsp.Execute, useImport, useExport);

    local.ErrorCode.Text8 = useExport.ErrorCode.Text8;
  }

  private void UseSiCreatePaParticipantAddress()
  {
    var useImport = new SiCreatePaParticipantAddress.Import();
    var useExport = new SiCreatePaParticipantAddress.Export();

    useImport.PaReferralParticipant.Identifier =
      local.PaReferralParticipant.Identifier;
    MovePaReferral1(local.PaReferral, useImport.PaReferral);
    useImport.PaParticipantAddress.Assign(local.PaParticipantAddress);

    Call(SiCreatePaParticipantAddress.Execute, useImport, useExport);
  }

  private void UseSiCreatePaReferral()
  {
    var useImport = new SiCreatePaReferral.Import();
    var useExport = new SiCreatePaReferral.Export();

    useImport.Ar.PersonNumber = local.HoldArNumber.PersonNumber;
    useImport.PaReferralParticipant.Assign(local.PaReferralParticipant);
    useImport.PaReferral.Assign(local.PaReferral);
    useImport.SuppressArRelationship.Flag = local.SuppressArRelationship.Flag;

    Call(SiCreatePaReferral.Execute, useImport, useExport);

    local.PaReferral.Assign(useExport.PaReferral);
  }

  private void UseSiCreatePaReferralParticipnt()
  {
    var useImport = new SiCreatePaReferralParticipnt.Import();
    var useExport = new SiCreatePaReferralParticipnt.Export();

    useImport.PaReferralParticipant.Assign(local.PaReferralParticipant);
    MovePaReferral1(local.PaReferral, useImport.PaReferral);

    Call(SiCreatePaReferralParticipnt.Execute, useImport, useExport);
  }

  private void UseUpdatePgmCheckpointRestart1()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);
  }

  private void UseUpdatePgmCheckpointRestart2()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }

  private bool ReadInterfacePaParticipantAddress1()
  {
    entities.InterfacePaParticipantAddress.Populated = false;

    return Read("ReadInterfacePaParticipantAddress1",
      (db, command) =>
      {
        db.SetString(
          command, "interfaceId",
          entities.InterfacePaReferral.InterfaceIdentifier);
        db.SetInt32(
          command, "participantId", local.PaReferralParticipant.Identifier);
      },
      (db, reader) =>
      {
        entities.InterfacePaParticipantAddress.InterfaceIdentifier =
          db.GetString(reader, 0);
        entities.InterfacePaParticipantAddress.ParticipantIdentifier =
          db.GetInt32(reader, 1);
        entities.InterfacePaParticipantAddress.Identifier =
          db.GetInt32(reader, 2);
        entities.InterfacePaParticipantAddress.CreatedBy =
          db.GetNullableString(reader, 3);
        entities.InterfacePaParticipantAddress.CreatedTimestamp =
          db.GetNullableDateTime(reader, 4);
        entities.InterfacePaParticipantAddress.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.InterfacePaParticipantAddress.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.InterfacePaParticipantAddress.Type1 =
          db.GetNullableString(reader, 7);
        entities.InterfacePaParticipantAddress.Street1 =
          db.GetNullableString(reader, 8);
        entities.InterfacePaParticipantAddress.Street2 =
          db.GetNullableString(reader, 9);
        entities.InterfacePaParticipantAddress.City =
          db.GetNullableString(reader, 10);
        entities.InterfacePaParticipantAddress.State =
          db.GetNullableString(reader, 11);
        entities.InterfacePaParticipantAddress.Zip =
          db.GetNullableString(reader, 12);
        entities.InterfacePaParticipantAddress.Zip4 =
          db.GetNullableString(reader, 13);
        entities.InterfacePaParticipantAddress.Zip3 =
          db.GetNullableString(reader, 14);
        entities.InterfacePaParticipantAddress.Populated = true;
      });
  }

  private IEnumerable<bool> ReadInterfacePaParticipantAddress2()
  {
    entities.InterfacePaParticipantAddress.Populated = false;

    return ReadEach("ReadInterfacePaParticipantAddress2",
      (db, command) =>
      {
        db.SetString(
          command, "interfaceId",
          entities.InterfacePaReferral.InterfaceIdentifier);
      },
      (db, reader) =>
      {
        entities.InterfacePaParticipantAddress.InterfaceIdentifier =
          db.GetString(reader, 0);
        entities.InterfacePaParticipantAddress.ParticipantIdentifier =
          db.GetInt32(reader, 1);
        entities.InterfacePaParticipantAddress.Identifier =
          db.GetInt32(reader, 2);
        entities.InterfacePaParticipantAddress.CreatedBy =
          db.GetNullableString(reader, 3);
        entities.InterfacePaParticipantAddress.CreatedTimestamp =
          db.GetNullableDateTime(reader, 4);
        entities.InterfacePaParticipantAddress.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.InterfacePaParticipantAddress.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.InterfacePaParticipantAddress.Type1 =
          db.GetNullableString(reader, 7);
        entities.InterfacePaParticipantAddress.Street1 =
          db.GetNullableString(reader, 8);
        entities.InterfacePaParticipantAddress.Street2 =
          db.GetNullableString(reader, 9);
        entities.InterfacePaParticipantAddress.City =
          db.GetNullableString(reader, 10);
        entities.InterfacePaParticipantAddress.State =
          db.GetNullableString(reader, 11);
        entities.InterfacePaParticipantAddress.Zip =
          db.GetNullableString(reader, 12);
        entities.InterfacePaParticipantAddress.Zip4 =
          db.GetNullableString(reader, 13);
        entities.InterfacePaParticipantAddress.Zip3 =
          db.GetNullableString(reader, 14);
        entities.InterfacePaParticipantAddress.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadInterfacePaReferral()
  {
    entities.InterfacePaReferral.Populated = false;

    return ReadEach("ReadInterfacePaReferral",
      (db, command) =>
      {
        db.SetString(command, "interfaceId", local.Restart.InterfaceIdentifier);
      },
      (db, reader) =>
      {
        entities.InterfacePaReferral.InterfaceIdentifier =
          db.GetString(reader, 0);
        entities.InterfacePaReferral.CsOrderPlace =
          db.GetNullableString(reader, 1);
        entities.InterfacePaReferral.CsOrderState =
          db.GetNullableString(reader, 2);
        entities.InterfacePaReferral.CsFreq = db.GetNullableString(reader, 3);
        entities.InterfacePaReferral.From = db.GetNullableString(reader, 4);
        entities.InterfacePaReferral.ApPhoneNumber =
          db.GetNullableInt32(reader, 5);
        entities.InterfacePaReferral.ApAreaCode =
          db.GetNullableInt32(reader, 6);
        entities.InterfacePaReferral.CcStartDate =
          db.GetNullableDate(reader, 7);
        entities.InterfacePaReferral.ArEmployerName =
          db.GetNullableString(reader, 8);
        entities.InterfacePaReferral.SupportOrderFreq =
          db.GetNullableString(reader, 9);
        entities.InterfacePaReferral.CreatedBy =
          db.GetNullableString(reader, 10);
        entities.InterfacePaReferral.CreatedTimestamp =
          db.GetDateTime(reader, 11);
        entities.InterfacePaReferral.LastUpdatedBy =
          db.GetNullableString(reader, 12);
        entities.InterfacePaReferral.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 13);
        entities.InterfacePaReferral.Note = db.GetNullableString(reader, 14);
        entities.InterfacePaReferral.ReceivedDate =
          db.GetNullableDate(reader, 15);
        entities.InterfacePaReferral.AssignDeactivateInd =
          db.GetNullableString(reader, 16);
        entities.InterfacePaReferral.AssignDeactivateDate =
          db.GetNullableDate(reader, 17);
        entities.InterfacePaReferral.CaseNumber =
          db.GetNullableString(reader, 18);
        entities.InterfacePaReferral.Number = db.GetString(reader, 19);
        entities.InterfacePaReferral.Type1 = db.GetString(reader, 20);
        entities.InterfacePaReferral.MedicalPaymentDueDate =
          db.GetNullableDate(reader, 21);
        entities.InterfacePaReferral.MedicalAmt =
          db.GetNullableDecimal(reader, 22);
        entities.InterfacePaReferral.MedicalFreq =
          db.GetNullableString(reader, 23);
        entities.InterfacePaReferral.MedicalLastPayment =
          db.GetNullableDecimal(reader, 24);
        entities.InterfacePaReferral.MedicalLastPaymentDate =
          db.GetNullableDate(reader, 25);
        entities.InterfacePaReferral.MedicalOrderEffectiveDate =
          db.GetNullableDate(reader, 26);
        entities.InterfacePaReferral.MedicalOrderState =
          db.GetNullableString(reader, 27);
        entities.InterfacePaReferral.MedicalOrderPlace =
          db.GetNullableString(reader, 28);
        entities.InterfacePaReferral.MedicalArrearage =
          db.GetNullableDecimal(reader, 29);
        entities.InterfacePaReferral.MedicalPaidTo =
          db.GetNullableString(reader, 30);
        entities.InterfacePaReferral.MedicalPaymentType =
          db.GetNullableString(reader, 31);
        entities.InterfacePaReferral.MedicalInsuranceCo =
          db.GetNullableString(reader, 32);
        entities.InterfacePaReferral.MedicalPolicyNumber =
          db.GetNullableString(reader, 33);
        entities.InterfacePaReferral.MedicalOrderNumber =
          db.GetNullableString(reader, 34);
        entities.InterfacePaReferral.MedicalOrderInd =
          db.GetNullableString(reader, 35);
        entities.InterfacePaReferral.ApprovalDate =
          db.GetNullableDate(reader, 36);
        entities.InterfacePaReferral.CseRegion =
          db.GetNullableString(reader, 37);
        entities.InterfacePaReferral.CseReferralRecDate =
          db.GetNullableDate(reader, 38);
        entities.InterfacePaReferral.ArRetainedInd =
          db.GetNullableString(reader, 39);
        entities.InterfacePaReferral.PgmCode = db.GetNullableString(reader, 40);
        entities.InterfacePaReferral.CaseWorker =
          db.GetNullableString(reader, 41);
        entities.InterfacePaReferral.PaymentMadeTo =
          db.GetNullableString(reader, 42);
        entities.InterfacePaReferral.CsArrearageAmt =
          db.GetNullableDecimal(reader, 43);
        entities.InterfacePaReferral.CsLastPaymentAmt =
          db.GetNullableDecimal(reader, 44);
        entities.InterfacePaReferral.LastPaymentDate =
          db.GetNullableDate(reader, 45);
        entities.InterfacePaReferral.GoodCauseCode =
          db.GetNullableString(reader, 46);
        entities.InterfacePaReferral.GoodCauseDate =
          db.GetNullableDate(reader, 47);
        entities.InterfacePaReferral.CsPaymentAmount =
          db.GetNullableDecimal(reader, 48);
        entities.InterfacePaReferral.OrderEffectiveDate =
          db.GetNullableDate(reader, 49);
        entities.InterfacePaReferral.PaymentDueDate =
          db.GetNullableDate(reader, 50);
        entities.InterfacePaReferral.SupportOrderId =
          db.GetNullableString(reader, 51);
        entities.InterfacePaReferral.LastApContactDate =
          db.GetNullableDate(reader, 52);
        entities.InterfacePaReferral.VoluntarySupportInd =
          db.GetNullableString(reader, 53);
        entities.InterfacePaReferral.ApEmployerPhone =
          db.GetNullableInt64(reader, 54);
        entities.InterfacePaReferral.ApEmployerName =
          db.GetNullableString(reader, 55);
        entities.InterfacePaReferral.FcNextJuvenileCtDt =
          db.GetNullableDate(reader, 56);
        entities.InterfacePaReferral.FcOrderEstBy =
          db.GetNullableString(reader, 57);
        entities.InterfacePaReferral.FcJuvenileCourtOrder =
          db.GetNullableString(reader, 58);
        entities.InterfacePaReferral.FcJuvenileOffenderInd =
          db.GetNullableString(reader, 59);
        entities.InterfacePaReferral.FcCincInd =
          db.GetNullableString(reader, 60);
        entities.InterfacePaReferral.FcPlacementDate =
          db.GetNullableDate(reader, 61);
        entities.InterfacePaReferral.FcSrsPayee =
          db.GetNullableString(reader, 62);
        entities.InterfacePaReferral.FcCostOfCareFreq =
          db.GetNullableString(reader, 63);
        entities.InterfacePaReferral.FcCostOfCare =
          db.GetNullableDecimal(reader, 64);
        entities.InterfacePaReferral.FcAdoptionDisruptionInd =
          db.GetNullableString(reader, 65);
        entities.InterfacePaReferral.FcPlacementType =
          db.GetNullableString(reader, 66);
        entities.InterfacePaReferral.FcPreviousPa =
          db.GetNullableString(reader, 67);
        entities.InterfacePaReferral.FcDateOfInitialCustody =
          db.GetNullableDate(reader, 68);
        entities.InterfacePaReferral.FcRightsSevered =
          db.GetNullableString(reader, 69);
        entities.InterfacePaReferral.FcIvECaseNumber =
          db.GetNullableString(reader, 70);
        entities.InterfacePaReferral.FcPlacementName =
          db.GetNullableString(reader, 71);
        entities.InterfacePaReferral.FcSourceOfFunding =
          db.GetNullableString(reader, 72);
        entities.InterfacePaReferral.FcOtherBenefitInd =
          db.GetNullableString(reader, 73);
        entities.InterfacePaReferral.FcZebInd =
          db.GetNullableString(reader, 74);
        entities.InterfacePaReferral.FcVaInd = db.GetNullableString(reader, 75);
        entities.InterfacePaReferral.FcSsi = db.GetNullableString(reader, 76);
        entities.InterfacePaReferral.FcSsa = db.GetNullableString(reader, 77);
        entities.InterfacePaReferral.FcWardsAccount =
          db.GetNullableString(reader, 78);
        entities.InterfacePaReferral.FcCountyChildRemovedFrom =
          db.GetNullableString(reader, 79);
        entities.InterfacePaReferral.FcApNotified =
          db.GetNullableString(reader, 80);
        entities.InterfacePaReferral.KsCounty =
          db.GetNullableString(reader, 81);
        entities.InterfacePaReferral.CseInvolvementInd =
          db.GetNullableString(reader, 82);
        entities.InterfacePaReferral.Populated = true;

        return true;
      });
  }

  private bool ReadInterfacePaReferralParticipnt1()
  {
    entities.Ar.Populated = false;

    return Read("ReadInterfacePaReferralParticipnt1",
      (db, command) =>
      {
        db.SetString(
          command, "interfaceId",
          entities.InterfacePaReferral.InterfaceIdentifier);
      },
      (db, reader) =>
      {
        entities.Ar.InterfaceIdentifier = db.GetString(reader, 0);
        entities.Ar.Identifier = db.GetInt32(reader, 1);
        entities.Ar.CreatedBy = db.GetNullableString(reader, 2);
        entities.Ar.CreatedTimestamp = db.GetNullableDateTime(reader, 3);
        entities.Ar.LastUpdatedBy = db.GetNullableString(reader, 4);
        entities.Ar.LastUpdatedTimestamp = db.GetNullableDateTime(reader, 5);
        entities.Ar.AbsenceCode = db.GetNullableString(reader, 6);
        entities.Ar.Relationship = db.GetNullableString(reader, 7);
        entities.Ar.Sex = db.GetNullableString(reader, 8);
        entities.Ar.Dob = db.GetNullableDate(reader, 9);
        entities.Ar.LastName = db.GetNullableString(reader, 10);
        entities.Ar.FirstName = db.GetNullableString(reader, 11);
        entities.Ar.Mi = db.GetNullableString(reader, 12);
        entities.Ar.Ssn = db.GetNullableString(reader, 13);
        entities.Ar.PersonNumber = db.GetNullableString(reader, 14);
        entities.Ar.GoodCauseStatus = db.GetNullableString(reader, 15);
        entities.Ar.InsurInd = db.GetNullableString(reader, 16);
        entities.Ar.PatEstInd = db.GetNullableString(reader, 17);
        entities.Ar.BeneInd = db.GetNullableString(reader, 18);
        entities.Ar.Role = db.GetNullableString(reader, 19);
        entities.Ar.Populated = true;
      });
  }

  private IEnumerable<bool> ReadInterfacePaReferralParticipnt2()
  {
    entities.Ch.Populated = false;

    return ReadEach("ReadInterfacePaReferralParticipnt2",
      (db, command) =>
      {
        db.SetString(
          command, "interfaceId",
          entities.InterfacePaReferral.InterfaceIdentifier);
      },
      (db, reader) =>
      {
        entities.Ch.InterfaceIdentifier = db.GetString(reader, 0);
        entities.Ch.Identifier = db.GetInt32(reader, 1);
        entities.Ch.CreatedBy = db.GetNullableString(reader, 2);
        entities.Ch.CreatedTimestamp = db.GetNullableDateTime(reader, 3);
        entities.Ch.LastUpdatedBy = db.GetNullableString(reader, 4);
        entities.Ch.LastUpdatedTimestamp = db.GetNullableDateTime(reader, 5);
        entities.Ch.AbsenceCode = db.GetNullableString(reader, 6);
        entities.Ch.Relationship = db.GetNullableString(reader, 7);
        entities.Ch.Sex = db.GetNullableString(reader, 8);
        entities.Ch.Dob = db.GetNullableDate(reader, 9);
        entities.Ch.LastName = db.GetNullableString(reader, 10);
        entities.Ch.FirstName = db.GetNullableString(reader, 11);
        entities.Ch.Mi = db.GetNullableString(reader, 12);
        entities.Ch.Ssn = db.GetNullableString(reader, 13);
        entities.Ch.PersonNumber = db.GetNullableString(reader, 14);
        entities.Ch.GoodCauseStatus = db.GetNullableString(reader, 15);
        entities.Ch.InsurInd = db.GetNullableString(reader, 16);
        entities.Ch.PatEstInd = db.GetNullableString(reader, 17);
        entities.Ch.BeneInd = db.GetNullableString(reader, 18);
        entities.Ch.Role = db.GetNullableString(reader, 19);
        entities.Ch.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadInterfacePaReferralParticipnt3()
  {
    entities.Other.Populated = false;

    return ReadEach("ReadInterfacePaReferralParticipnt3",
      (db, command) =>
      {
        db.SetString(
          command, "interfaceId",
          entities.InterfacePaReferral.InterfaceIdentifier);
      },
      (db, reader) =>
      {
        entities.Other.InterfaceIdentifier = db.GetString(reader, 0);
        entities.Other.Identifier = db.GetInt32(reader, 1);
        entities.Other.CreatedBy = db.GetNullableString(reader, 2);
        entities.Other.CreatedTimestamp = db.GetNullableDateTime(reader, 3);
        entities.Other.LastUpdatedBy = db.GetNullableString(reader, 4);
        entities.Other.LastUpdatedTimestamp = db.GetNullableDateTime(reader, 5);
        entities.Other.AbsenceCode = db.GetNullableString(reader, 6);
        entities.Other.Relationship = db.GetNullableString(reader, 7);
        entities.Other.Sex = db.GetNullableString(reader, 8);
        entities.Other.Dob = db.GetNullableDate(reader, 9);
        entities.Other.LastName = db.GetNullableString(reader, 10);
        entities.Other.FirstName = db.GetNullableString(reader, 11);
        entities.Other.Mi = db.GetNullableString(reader, 12);
        entities.Other.Ssn = db.GetNullableString(reader, 13);
        entities.Other.PersonNumber = db.GetNullableString(reader, 14);
        entities.Other.GoodCauseStatus = db.GetNullableString(reader, 15);
        entities.Other.InsurInd = db.GetNullableString(reader, 16);
        entities.Other.PatEstInd = db.GetNullableString(reader, 17);
        entities.Other.BeneInd = db.GetNullableString(reader, 18);
        entities.Other.Role = db.GetNullableString(reader, 19);
        entities.Other.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadInterfacePaReferralParticipnt4()
  {
    entities.Other.Populated = false;

    return ReadEach("ReadInterfacePaReferralParticipnt4",
      (db, command) =>
      {
        db.SetString(
          command, "interfaceId",
          entities.InterfacePaReferral.InterfaceIdentifier);
      },
      (db, reader) =>
      {
        entities.Other.InterfaceIdentifier = db.GetString(reader, 0);
        entities.Other.Identifier = db.GetInt32(reader, 1);
        entities.Other.CreatedBy = db.GetNullableString(reader, 2);
        entities.Other.CreatedTimestamp = db.GetNullableDateTime(reader, 3);
        entities.Other.LastUpdatedBy = db.GetNullableString(reader, 4);
        entities.Other.LastUpdatedTimestamp = db.GetNullableDateTime(reader, 5);
        entities.Other.AbsenceCode = db.GetNullableString(reader, 6);
        entities.Other.Relationship = db.GetNullableString(reader, 7);
        entities.Other.Sex = db.GetNullableString(reader, 8);
        entities.Other.Dob = db.GetNullableDate(reader, 9);
        entities.Other.LastName = db.GetNullableString(reader, 10);
        entities.Other.FirstName = db.GetNullableString(reader, 11);
        entities.Other.Mi = db.GetNullableString(reader, 12);
        entities.Other.Ssn = db.GetNullableString(reader, 13);
        entities.Other.PersonNumber = db.GetNullableString(reader, 14);
        entities.Other.GoodCauseStatus = db.GetNullableString(reader, 15);
        entities.Other.InsurInd = db.GetNullableString(reader, 16);
        entities.Other.PatEstInd = db.GetNullableString(reader, 17);
        entities.Other.BeneInd = db.GetNullableString(reader, 18);
        entities.Other.Role = db.GetNullableString(reader, 19);
        entities.Other.Populated = true;

        return true;
      });
  }

  private void UpdateInterfacePaReferral()
  {
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.InterfacePaReferral.Populated = false;
    Update("UpdateInterfacePaReferral",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetString(
          command, "interfaceId",
          entities.InterfacePaReferral.InterfaceIdentifier);
      });

    entities.InterfacePaReferral.LastUpdatedBy = lastUpdatedBy;
    entities.InterfacePaReferral.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.InterfacePaReferral.Populated = true;
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
    /// <summary>
    /// A value of SuppressArRelationship.
    /// </summary>
    [JsonPropertyName("suppressArRelationship")]
    public Common SuppressArRelationship
    {
      get => suppressArRelationship ??= new();
      set => suppressArRelationship = value;
    }

    /// <summary>
    /// A value of AssignPaRefToOspCount.
    /// </summary>
    [JsonPropertyName("assignPaRefToOspCount")]
    public Common AssignPaRefToOspCount
    {
      get => assignPaRefToOspCount ??= new();
      set => assignPaRefToOspCount = value;
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
    /// A value of ErrorCode.
    /// </summary>
    [JsonPropertyName("errorCode")]
    public TextWorkArea ErrorCode
    {
      get => errorCode ??= new();
      set => errorCode = value;
    }

    /// <summary>
    /// A value of CreateRefPartAddrCount.
    /// </summary>
    [JsonPropertyName("createRefPartAddrCount")]
    public Common CreateRefPartAddrCount
    {
      get => createRefPartAddrCount ??= new();
      set => createRefPartAddrCount = value;
    }

    /// <summary>
    /// A value of CreateReferralPartCount.
    /// </summary>
    [JsonPropertyName("createReferralPartCount")]
    public Common CreateReferralPartCount
    {
      get => createReferralPartCount ??= new();
      set => createReferralPartCount = value;
    }

    /// <summary>
    /// A value of CreateReferralCount.
    /// </summary>
    [JsonPropertyName("createReferralCount")]
    public Common CreateReferralCount
    {
      get => createReferralCount ??= new();
      set => createReferralCount = value;
    }

    /// <summary>
    /// A value of ReadRefPartAddrCount.
    /// </summary>
    [JsonPropertyName("readRefPartAddrCount")]
    public Common ReadRefPartAddrCount
    {
      get => readRefPartAddrCount ??= new();
      set => readRefPartAddrCount = value;
    }

    /// <summary>
    /// A value of ReadRefParticipantCount.
    /// </summary>
    [JsonPropertyName("readRefParticipantCount")]
    public Common ReadRefParticipantCount
    {
      get => readRefParticipantCount ??= new();
      set => readRefParticipantCount = value;
    }

    /// <summary>
    /// A value of ReadReferralsCount.
    /// </summary>
    [JsonPropertyName("readReferralsCount")]
    public Common ReadReferralsCount
    {
      get => readReferralsCount ??= new();
      set => readReferralsCount = value;
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
    /// A value of HoldArNumber.
    /// </summary>
    [JsonPropertyName("holdArNumber")]
    public PaReferralParticipant HoldArNumber
    {
      get => holdArNumber ??= new();
      set => holdArNumber = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public Common Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of ValidCode.
    /// </summary>
    [JsonPropertyName("validCode")]
    public Common ValidCode
    {
      get => validCode ??= new();
      set => validCode = value;
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
    /// A value of Participant.
    /// </summary>
    [JsonPropertyName("participant")]
    public Common Participant
    {
      get => participant ??= new();
      set => participant = value;
    }

    /// <summary>
    /// A value of PaReferralParticipant.
    /// </summary>
    [JsonPropertyName("paReferralParticipant")]
    public PaReferralParticipant PaReferralParticipant
    {
      get => paReferralParticipant ??= new();
      set => paReferralParticipant = value;
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
    /// A value of PaParticipantAddress.
    /// </summary>
    [JsonPropertyName("paParticipantAddress")]
    public PaParticipantAddress PaParticipantAddress
    {
      get => paParticipantAddress ??= new();
      set => paParticipantAddress = value;
    }

    /// <summary>
    /// A value of AddrExists.
    /// </summary>
    [JsonPropertyName("addrExists")]
    public Common AddrExists
    {
      get => addrExists ??= new();
      set => addrExists = value;
    }

    /// <summary>
    /// A value of ChCount.
    /// </summary>
    [JsonPropertyName("chCount")]
    public Common ChCount
    {
      get => chCount ??= new();
      set => chCount = value;
    }

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public InterfacePaReferralParticipnt Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public InterfacePaReferral Restart
    {
      get => restart ??= new();
      set => restart = value;
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
    /// A value of NumberOfReads.
    /// </summary>
    [JsonPropertyName("numberOfReads")]
    public Common NumberOfReads
    {
      get => numberOfReads ??= new();
      set => numberOfReads = value;
    }

    /// <summary>
    /// A value of NumberOfUpdates.
    /// </summary>
    [JsonPropertyName("numberOfUpdates")]
    public Common NumberOfUpdates
    {
      get => numberOfUpdates ??= new();
      set => numberOfUpdates = value;
    }

    private Common suppressArRelationship;
    private Common assignPaRefToOspCount;
    private DateWorkArea null1;
    private TextWorkArea errorCode;
    private Common createRefPartAddrCount;
    private Common createReferralPartCount;
    private Common createReferralCount;
    private Common readRefPartAddrCount;
    private Common readRefParticipantCount;
    private Common readReferralsCount;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private PaReferralParticipant holdArNumber;
    private Common ap;
    private Common validCode;
    private Code code;
    private CodeValue codeValue;
    private Common participant;
    private PaReferralParticipant paReferralParticipant;
    private PaReferral paReferral;
    private PaParticipantAddress paParticipantAddress;
    private Common addrExists;
    private Common chCount;
    private InterfacePaReferralParticipnt ar;
    private InterfacePaReferral restart;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ExitStateWorkArea exitStateWorkArea;
    private External passArea;
    private Common numberOfReads;
    private Common numberOfUpdates;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of InterfacePaParticipantAddress.
    /// </summary>
    [JsonPropertyName("interfacePaParticipantAddress")]
    public InterfacePaParticipantAddress InterfacePaParticipantAddress
    {
      get => interfacePaParticipantAddress ??= new();
      set => interfacePaParticipantAddress = value;
    }

    /// <summary>
    /// A value of Ch.
    /// </summary>
    [JsonPropertyName("ch")]
    public InterfacePaReferralParticipnt Ch
    {
      get => ch ??= new();
      set => ch = value;
    }

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public InterfacePaReferralParticipnt Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of Other.
    /// </summary>
    [JsonPropertyName("other")]
    public InterfacePaReferralParticipnt Other
    {
      get => other ??= new();
      set => other = value;
    }

    /// <summary>
    /// A value of InterfacePaReferral.
    /// </summary>
    [JsonPropertyName("interfacePaReferral")]
    public InterfacePaReferral InterfacePaReferral
    {
      get => interfacePaReferral ??= new();
      set => interfacePaReferral = value;
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

    private PaReferral paReferral;
    private InterfacePaParticipantAddress interfacePaParticipantAddress;
    private InterfacePaReferralParticipnt ch;
    private InterfacePaReferralParticipnt ar;
    private InterfacePaReferralParticipnt other;
    private InterfacePaReferral interfacePaReferral;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
  }
#endregion
}
