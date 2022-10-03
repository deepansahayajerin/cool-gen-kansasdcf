// Program: LE_CREATE_CRED_CERTIFICATION, ID: 372737281, model: 746.
// Short name: SWE00737
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_CREATE_CRED_CERTIFICATION.
/// </summary>
[Serializable]
public partial class LeCreateCredCertification: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_CREATE_CRED_CERTIFICATION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeCreateCredCertification(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeCreateCredCertification.
  /// </summary>
  public LeCreateCredCertification(IContext context, Import import,
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
    // ---------------------------------------------------------------------------------------------
    //                          M A I N T E N A N C E      L O G
    // ---------------------------------------------------------------------------------------------
    // Date	  Developer	Request #	Description
    // --------  ----------	------------	
    // -----------------------------------------------------
    // 04/23/96  H.Hooks			Initial Code
    // 07/19/96  M Ramirez			Added Print Function
    // 03/18/98  R Grey			Modify to skip abort when SSN invalid.
    // 02/05/99  PMcElderry			Moved SSN validation to PRAD, removed unnecessary
    // 					READs, added qualifier to READ of OBLIGATION for
    // 					arrearage balance > 0.
    // 02/24/99  M Ramirez			Added creation of document trigger
    // 03/24/99  M Brown			Modifications to remove usage of obligation
    // 					as_of_dt_tot_bal_curr_arr - attribute being deleted
    // 					from the model.
    // 04/12/99  PMcElderry			Added logic to ensure debt is court ordered.
    // 					Repositioned document creation w/in CREATE of CREDIT
    // 					REPORTING - formerly in UPDATE
    // 04/29/99  PMcElderry			Changed concurrent obl logic.
    // 05/13/99  PMcElderry			Added logic to prevent mulitple CRA XGC/XBR/XAD
    // 					records from being created if no change has been
    // 					made on the case; logic for date stayed and released.
    // 05/27/99  PMcElderry			Added AB FN_DETERM_PGM_FOR_DEBT_DTL to prevent
    // 					interstate debts from being included in certification.
    // 					Added READ to make sure cases are Open.
    // 08/20/99  PMcElderry			Logic for type "ACT" CSE Actions - current, 
    // original,
    // 					and highest amounts reset for CREDIT REPORTING.
    // 12/16/99  PMcElderry	PR # 81488	Regardless of case status, determine if 
    // any
    // 					debts are outstanding.
    // 08/31/00  PMcElderry 	PR # 101274	Changes to speed program; removed 
    // system
    // 	  GVandy			generated CURRENT TIMESTAMP, CURRENT DATE and
    // 					USER_ID from CREATEs and UPDATEs; altered READS to
    // 					prevent 'union with index (b/t
    // 					primary_secondary_obligation_rln_rsn and
    // 					obligation_rln
    // 08/31/00  PMcElderry	PR # 102336	Logic to remove usege of 
    // admin_action_cert_obligation
    // 					and adm_act_cert_debt_detail; deleted CREATE
    // 					and DELETE logic for aforementioned entities
    // 10/26/00  E.Shirk	PR# 105810   	Made changes to correct groupview 
    // overflow abend.
    // 					General overhaul.  Removed dead and duplicate code,
    // 					fixed read against supported person, fixed read
    // 					against obligation rln, fixed view matching for
    // 					le_get_highr_of_prim_sec_cc_obls calls.
    // 11/20/00  E.Shirk	PR# 108126	Made changes to shut off the consideration 
    // of secondary
    // 					obligations for debt amounts.
    // 01/11/01  E.Shirk	PR# 110952	Altered the read against bankruptcy to a 
    // read each.
    // 					Examined the discharge and dismiss dates for a
    // 					date greater than the ppi date.
    // 01/11/01  E.Shirk	PR# 110339	Added logic to bypass temporary obligations 
    // from
    // 					the certification amounts.
    // 02/21/01  E.Shirk	WO# 000269	Added read each against ap case role to 
    // determine
    // 					if the AP has any good cause actions taken by any AR.
    // 					This later eliminates the calling of
    // 					LE_CHECK_GOOD_CAUSE_FOR_DEBT when no good cause
    // 					was found for AP.
    // 05/16/02  E.Shirk	PR#122851	Altered Read Each against Case, Case Role and
    // legal
    // 					action to evaluate the case role end date when
    // 					building the array of valid cases for a given person.
    // 05/21/02  E.Shirk	PR#144138	Removed good cause (XGC) processing logic.
    // 					This logic became obsolete with WR269.
    // 05/23/02  E.Shirk	PR#134048	Altered process to begin evaluating 
    // interstate
    // 					at the debt program level, versus the previous
    // 					obligation determination.
    // 06/12/02  E.Shirk	PR#120826	Altered process to delete CRED certification 
    // when
    // 					an obligors arrears balance falls below $1.00.
    // 04/14/04  CMJ		PR#204813	Request to add obligation MJ for cred check.
    // 05/06/04  CMJ		pr 180774,	People not getting on the certification list
    // 			186616.197320,	- rejecting on error list.  These individuals
    // 			181773,188696,	have a admin_act_cert but do not have a
    // 			169161,192578,	cred_rpt_act - code program to delete the
    // 			and others		admin if cred does not exist .
    // 05/06/04  CMJ				Added switch to help determine when you had a cred.
    // 07/21/04  CMJ		PR#211254	ISS not updating and sending to credit bureau.
    // 08/11/04  CMJ				Changed legal read  want legal actions with J
    // 					classification can have debts.
    // 03/08/05  CMJ				Put in code to  do an ISS after a Cancel.
    // 03/30/05  CMJ		PR#237371	Take off check for program code at the debt 
    // level,
    // 					and put back on the obligation level for not letting
    // 					any interstate person go to the credit bureau.
    // 08/31/06  MLB		PR00280354	This is to fix the problem of
    // 					debts/disbursements not being exempted when they
    // 					should. This is due to a flag being set and not
    // 					reset at the correct time.
    // 02/26/07  Raj S		PR 00300253	Modified to fix the  problem in selecting 
    // legal action
    // 			00263585	classification, currently the process checks all case
    // 			00299089	role should have 'J' classification legal action but
    // 					as per the business any one of the case role in the
    // 					case role has 'J' classification is valid.
    // 08/23/07  MWF		WR296917	Commented out the codes that exclude AP's in
    // 			(Part D)	bankruptcy to comply the new federal bankruptcy
    // 					law effective 10/17/2005. Note that the codes exist
    // 					in the process for handling APs previously reported
    // 					to credit agency and in the process for handling
    // 					APs never reported to credit agency before.
    // 09/24/10  DQD		CQ19411		We are now using a parameter to make sure that
    // 					a obligation was created a certian number of days ago.
    // 08/04/17  GVandy	CQ56369		Changes for Metro2 file/record layouts.
    // 					Restructured code to cleanup formatting,
    // 					simplify logic, and changes to certification
    // 					criteria.
    // ---------------------------------------------------------------------------------------------
    export.CredRecordCreated.Flag = "N";
    export.ErrorWritten.Flag = "N";

    // -------------------------------------------------------------------------------------
    // Read NCP person and obligor record.
    // -------------------------------------------------------------------------------------
    if (!ReadCsePersonObligor())
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    // ---------------------------------------------------------------------------------------------
    // Find the Credit Reporting (administrative_act_certification) record for 
    // the NCP.
    // ---------------------------------------------------------------------------------------------
    if (ReadCreditReporting())
    {
      MoveAdministrativeActCertification(entities.CreditReporting,
        local.CreditReporting);
    }
    else
    {
      // --This is OK.  The NCP may not have previously been certified for 
      // credit reporting.
    }

    // ---------------------------------------------------------------------------------------------
    // Step 0:  Find the most recent CRA (Credit Reporting Agency) status...
    // ---------------------------------------------------------------------------------------------
    local.NcpCurrentlySentToCra.Flag = "N";

    if (ReadCreditReportingAction2())
    {
      // --This is needed for the DEL case in Step 1 and also in Steps 3 & 4.
      local.MostRecentCraStatus.Assign(entities.MostRecentCraStatus);

      switch(TrimEnd(Substring(local.MostRecentCraStatus.CraTransCode, 1, 1)))
      {
        case "":
          local.NcpCurrentlySentToCra.Flag = "N";

          break;
        case "A":
          local.NcpCurrentlySentToCra.Flag = "Y";

          break;
        case "U":
          if (Equal(entities.MostRecentCraStatus.CraTransCode, "U13") || Equal
            (entities.MostRecentCraStatus.CraTransCode, "U64"))
          {
            // --In the scenario where all case involvement ends for a 
            // previously certified NCP
            //   whose debts were either paid off (U13) or written off (U64) 
            // then any further
            //   reporting requires the NCP to again meet the initial 
            // certification requirements.
            //   So treat him as if he is not currently certified.
            local.NcpCurrentlySentToCra.Flag = "N";
          }
          else
          {
            local.NcpCurrentlySentToCra.Flag = "Y";
          }

          break;
        case "D":
          local.NcpCurrentlySentToCra.Flag = "N";

          break;
        default:
          local.NcpCurrentlySentToCra.Flag = "N";

          break;
      }
    }

    // ---------------------------------------------------------------------------------------------
    // Step 1: Find the most recent CSE credit status for the NCP.
    // ---------------------------------------------------------------------------------------------
    // --Sort by the CRA Trans Date.  In the new scheme the ISS won't have a 
    // Date Sent to CRA but it will have the CRA Trans Date.
    foreach(var item in ReadCreditReportingAction3())
    {
      // ---------------------------------------------------------------------------------------------
      // Determine if we need to skip the NCP based on the most recent status.
      // ---------------------------------------------------------------------------------------------
      switch(TrimEnd(entities.MostRecentCseStatus.CseActionCode))
      {
        case "ISS":
          if (Lt(import.ProgramProcessingInfo.ProcessDate,
            AddDays(entities.CreditReporting.NotificationDate, 15)))
          {
            // --ISS (Notice Sent - Pending Notification Date)
            // --Skip the NCP.  The notification period during which the NCP can
            // protest has not expired.
            if (AsChar(import.InformationLogging.Flag) == 'Y')
            {
              export.InfoLogging.RptDetail = "NCP " + import
                .CsePerson.Number + " Skipped  -  Pending ISS notification period expiration.";
                
            }

            return;
          }
          else
          {
            // --ISS (Notification Date Reached)
            // --Continue.
          }

          break;
        case "RAP":
          // --RAP (AP Response)
          // --Ignore RAP.  Use the prior status.
          continue;
        case "STA":
          // --STA (Stayed)
          // --Skip the NCP until the Stay is released.
          if (AsChar(import.InformationLogging.Flag) == 'Y')
          {
            export.InfoLogging.RptDetail = "NCP " + import.CsePerson.Number + " Skipped  -  Currently STA (Stayed).";
              
          }

          return;
        case "RST":
          // --RST (Release Stay)
          // --Continue.
          break;
        case "CAN":
          // --CAN (Cancelled)
          //   CAN is no longer valid but included here due to historical 
          // records that may be encountered.
          // --Continue.
          break;
        case "DEL":
          if (AsChar(local.NcpCurrentlySentToCra.Flag) == 'Y' && Equal
            (entities.MostRecentCseStatus.DateSentToCra, local.Null1.Date))
          {
            // --DEL (Delete Pending - Not yet sent to credit reporting agency)
            //   This is a worker initiated DELETE from the CRED screen.
            // --The NCP has previously been sent to the CRA and the most recent
            // status sent to the
            //   CRA is not a delete (i.e. they are currently active at the CRA
            // ). We will send a
            //   DDA status to the CRA.
            if (AsChar(import.InformationLogging.Flag) == 'Y')
            {
              export.InfoLogging.RptDetail = "NCP " + import
                .CsePerson.Number + " Skipped  -  DEL (Delete) is pending.";
            }

            // --Update the credit reporting action.  Set the date sent to CRA 
            // to the processing
            //   date and the CRA Trans Code to DDA.
            try
            {
              UpdateCreditReportingAction();
              export.SendToCra.RptDetail = "Y";
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "CREDIT_REPORTING_ACTION_NU";

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "CREDIT_REPORTING_ACTION_PV";

                  break;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }

            return;
          }
          else
          {
            // --DEL (Delete)
            // --Continue.
          }

          break;
        case "ACT":
          // --ACT (Activate)
          //   ACT is no longer valid but included here due to historical 
          // records that may be encountered.
          // --Continue.
          break;
        case "SND":
          // --SND (Send)  (This is the first submission to the credit reporting
          // agency)
          // --Continue.
          break;
        case "UPD":
          // --UPD (Update)
          // --Continue.
          break;
        case "XBR":
          // --XBR (Excluded Due to Bankruptcy)   NCPs are no longer excluded 
          // due to bankruptcy.
          //   It is included here due to historical records that may be 
          // encountered.
          // --Continue.
          break;
        case "XGC":
          // --XGC (Excluded Due to Good Cause)
          // --Continue.
          break;
        case "XAD":
          // --XAD (Excluded Due to Administrative Exemptions)
          // --Continue.
          break;
        default:
          // --Unrecognized status.  SKip this record and use the prior status.
          continue;
      }

      local.MostRecentCseStatus.Assign(entities.MostRecentCseStatus);

      break;
    }

    // ---------------------------------------------------------------------------------------------
    // Determine if the NCP meets the criteria to be certified for credit 
    // reporting today.
    // ---------------------------------------------------------------------------------------------
    UseLeCheckIfNcpCredCertifiable();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ---------------------------------------------------------------------------------------------
    // Processing proceeds based on whether the NCP should be excluded from 
    // credit reporting.
    // ---------------------------------------------------------------------------------------------
    switch(AsChar(local.ExcludeNcpFromCredRpt.Flag))
    {
      case 'Y':
        // --NCP is to be excluded crom credit reporting.
        switch(AsChar(local.NcpCurrentlySentToCra.Flag))
        {
          case 'Y':
            // --NCP is currently reported to the Credit Agencies but should now
            // be excluded from
            //   credit reporting.  Create a DEL action and send DDA trans code 
            // to the credit
            //   agencies.
            local.New1.CseActionCode = "DEL";
            local.New1.CraTransCode = "DDA";
            local.New1.CurrentAmount = 0;
            local.New1.HighestAmount = 0;
            local.New1.OriginalAmount = 0;
            local.CreditReporting.DecertifiedDate =
              import.ProgramProcessingInfo.ProcessDate;
            local.CreditReporting.OriginalAmount = 0;
            local.CreditReporting.CurrentAmount = 0;
            local.CreditReporting.HighestAmount = 0;
            local.CreditReporting.CurrentAmountDate = local.Null1.Date;

            switch(TrimEnd(local.ExcludedReason.Text9))
            {
              case "UNDER18":
                local.CreditReporting.DecertificationReason =
                  "Under 18 Years of Age";

                if (AsChar(import.InformationLogging.Flag) == 'Y')
                {
                  export.InfoLogging.RptDetail = "NCP " + import
                    .CsePerson.Number + " DEL/DDA  -  NCP is under 18 years of age. ";
                    
                }

                break;
              case "NOSSNDOB":
                local.CreditReporting.DecertificationReason =
                  "No SSN or Date of Birth";

                if (AsChar(import.InformationLogging.Flag) == 'Y')
                {
                  export.InfoLogging.RptDetail = "NCP " + import
                    .CsePerson.Number + " DEL/DDA  -  NCP has neither an SSN or Date of Birth.";
                    
                }

                break;
              default:
                break;
            }

            break;
          case 'N':
            // --NCP is not currently reported to the Credit Agencies and should
            // remain excluded
            //   from credit reporting. No action required.
            if (AsChar(import.InformationLogging.Flag) == 'Y')
            {
              switch(TrimEnd(local.ExcludedReason.Text9))
              {
                case "UNDER18":
                  export.InfoLogging.RptDetail = "NCP " + import
                    .CsePerson.Number + " Skipped  -  NCP is under 18 years of age.";
                    

                  break;
                case "NOSSNDOB":
                  export.InfoLogging.RptDetail = "NCP " + import
                    .CsePerson.Number + " Skipped  -  NCP has neither an SSN or Date of Birth.";
                    

                  break;
                case "NOADDRESS":
                  export.InfoLogging.RptDetail = "NCP " + import
                    .CsePerson.Number + " Skipped  -  NCP has no address.";

                  break;
                default:
                  break;
              }
            }

            return;
          default:
            break;
        }

        break;
      case 'N':
        // --NCP is not to be excluded from credit reporting.
        switch(AsChar(local.NcpCurrentlySentToCra.Flag))
        {
          case 'Y':
            // --NCP is currently reported to the Credit Agencies and should not
            // be excluded from
            //   credit reporting. Create an UPD action and send a Uxx trans 
            // code to the credit
            //   agencies.
            local.New1.CseActionCode = "UPD";
            local.CreditReporting.OriginalAmount =
              local.New1.OriginalAmount.GetValueOrDefault();
            local.CreditReporting.CurrentAmount =
              local.New1.CurrentAmount.GetValueOrDefault();
            local.CreditReporting.HighestAmount =
              local.New1.HighestAmount.GetValueOrDefault();
            local.CreditReporting.CurrentAmountDate =
              import.ProgramProcessingInfo.ProcessDate;

            if (AsChar(import.InformationLogging.Flag) == 'Y')
            {
              export.InfoLogging.RptDetail = "NCP " + import
                .CsePerson.Number + " " + (local.New1.CseActionCode ?? "") + "/"
                + local.New1.CraTransCode + "  -  Meets certification criteria.";
                
            }

            break;
          case 'N':
            // --NCP is not currently reported to the Credit Agencies and should
            // not be excluded
            //   from credit reporting.
            if (local.New1.CurrentAmount.GetValueOrDefault() >= 1000)
            {
              // --NCP meets the $1000.00 threshold for certification.
              switch(TrimEnd(local.MostRecentCseStatus.CseActionCode ?? ""))
              {
                case "ISS":
                  // --Most recent action is ISS (Notification Date Reached).  
                  // Create a SND action to Add
                  //   the NCP.
                  local.New1.CseActionCode = "SND";
                  local.CreditReporting.OriginalAmount =
                    local.New1.OriginalAmount.GetValueOrDefault();
                  local.CreditReporting.CurrentAmount =
                    local.New1.CurrentAmount.GetValueOrDefault();
                  local.CreditReporting.HighestAmount =
                    local.New1.HighestAmount.GetValueOrDefault();
                  local.CreditReporting.CurrentAmountDate =
                    import.ProgramProcessingInfo.ProcessDate;

                  break;
                case "RST":
                  // --Most recent action is RST (Release Stay).  Create a SND 
                  // action to Add the NCP.
                  local.New1.CseActionCode = "SND";
                  local.CreditReporting.OriginalAmount =
                    local.New1.OriginalAmount.GetValueOrDefault();
                  local.CreditReporting.CurrentAmount =
                    local.New1.CurrentAmount.GetValueOrDefault();
                  local.CreditReporting.HighestAmount =
                    local.New1.HighestAmount.GetValueOrDefault();
                  local.CreditReporting.CurrentAmountDate =
                    import.ProgramProcessingInfo.ProcessDate;

                  break;
                default:
                  // --All other statuses will create a ISS (Issuance) to send a
                  // notice to the NCP.
                  local.New1.CseActionCode = "ISS";
                  local.New1.CraTransCode = "";
                  local.New1.OriginalAmount = 0;
                  local.New1.HighestAmount = 0;
                  local.CreditReporting.NotificationDate =
                    import.ProgramProcessingInfo.ProcessDate;
                  local.CreditReporting.NotifiedBy = global.UserId;
                  local.CreditReporting.DateStayed = local.Null1.Date;
                  local.CreditReporting.DateStayReleased = local.Null1.Date;
                  local.CreditReporting.CurrentAmount =
                    local.New1.CurrentAmount.GetValueOrDefault();
                  local.CreditReporting.CurrentAmountDate =
                    import.ProgramProcessingInfo.ProcessDate;

                  // --Send notice to the NCP.  Trigger the CRDTNOTF document.
                  local.Document.Name = "CRDTNOTF";
                  local.SpDocKey.KeyPerson = import.CsePerson.Number;
                  local.SpDocKey.KeyPersonAccount = "R";

                  if (entities.CreditReporting.Populated)
                  {
                    local.SpDocKey.KeyAdminActionCert =
                      entities.CreditReporting.TakenDate;
                  }
                  else
                  {
                    local.SpDocKey.KeyAdminActionCert =
                      import.ProgramProcessingInfo.ProcessDate;
                  }

                  local.SpDocKey.KeyCase = local.Issuance.Number;
                  local.Infrastructure.ReferenceDate =
                    import.ProgramProcessingInfo.ProcessDate;
                  UseSpCreateDocumentInfrastruct();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    return;
                  }

                  break;
              }

              if (AsChar(import.InformationLogging.Flag) == 'Y')
              {
                export.InfoLogging.RptDetail = "NCP " + import
                  .CsePerson.Number + " " + (local.New1.CseActionCode ?? "") + "/"
                  + local.New1.CraTransCode + "  -  Meets certification criteria.";
                  
              }
            }
            else
            {
              // --NCP is not currently reported to the Credit Agencies. No 
              // action required.
              if (AsChar(import.InformationLogging.Flag) == 'Y')
              {
                export.InfoLogging.RptDetail = "NCP " + import
                  .CsePerson.Number + " Skipped  -  Less than $1000 in arrears.";
                  
              }

              return;
            }

            break;
          default:
            break;
        }

        break;
      default:
        break;
    }

    // ---------------------------------------------------------------------------------------------
    // Step 4: Create the new credit reporting action record.
    // ---------------------------------------------------------------------------------------------
    local.New1.CraTransDate = import.ProgramProcessingInfo.ProcessDate;

    if (IsEmpty(local.New1.CraTransCode))
    {
      local.New1.DateSentToCra = local.Null1.Date;
      local.CreditReporting.DateSent = local.Null1.Date;
    }
    else
    {
      local.New1.DateSentToCra = import.ProgramProcessingInfo.ProcessDate;
      local.CreditReporting.DateSent = import.ProgramProcessingInfo.ProcessDate;
    }

    if (entities.CreditReporting.Populated)
    {
      // --Update the credit reporting record.
      try
      {
        UpdateCreditReporting();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "CREDIT_REPORTING_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "CREDIT_REPORTING_PV";

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
      // --NCP has not previously been certified for CRED reporting.  Create a 
      // new credit
      //   reporting record.
      if (!ReadAdministrativeAction())
      {
        ExitState = "ADMINISTRATIVE_ACTION_NF";

        return;
      }

      try
      {
        CreateCreditReporting();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "CREDIT_REPORTING_AE";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "CREDIT_REPORTING_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // --Create a new credit reporting action record.  These are the entries 
    // that display on
    //   the CRED screen.
    ReadCreditReportingAction1();

    try
    {
      CreateCreditReportingAction();
      export.CredRecordCreated.Flag = "Y";

      if (Lt(local.Null1.Date, entities.CreditReportingAction.DateSentToCra))
      {
        export.SendToCra.RptDetail = "Y";
      }
      else
      {
        export.SendToCra.RptDetail = "N";
      }
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "CREDIT_REPORTING_ACTION_AE";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "CREDIT_REPORTING_ACTION_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private static void MoveAdministrativeActCertification(
    AdministrativeActCertification source,
    AdministrativeActCertification target)
  {
    target.DateSent = source.DateSent;
    target.OriginalAmount = source.OriginalAmount;
    target.CurrentAmount = source.CurrentAmount;
    target.CurrentAmountDate = source.CurrentAmountDate;
    target.DecertifiedDate = source.DecertifiedDate;
    target.NotificationDate = source.NotificationDate;
    target.NotifiedBy = source.NotifiedBy;
    target.DecertificationReason = source.DecertificationReason;
    target.DateStayed = source.DateStayed;
    target.DateStayReleased = source.DateStayReleased;
    target.HighestAmount = source.HighestAmount;
  }

  private static void MoveCreditReportingAction(CreditReportingAction source,
    CreditReportingAction target)
  {
    target.CraTransCode = source.CraTransCode;
    target.OriginalAmount = source.OriginalAmount;
    target.CurrentAmount = source.CurrentAmount;
    target.HighestAmount = source.HighestAmount;
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.ReferenceDate = source.ReferenceDate;
  }

  private static void MoveSpDocKey(SpDocKey source, SpDocKey target)
  {
    target.KeyAdminActionCert = source.KeyAdminActionCert;
    target.KeyCase = source.KeyCase;
    target.KeyPerson = source.KeyPerson;
    target.KeyPersonAccount = source.KeyPersonAccount;
  }

  private void UseLeCheckIfNcpCredCertifiable()
  {
    var useImport = new LeCheckIfNcpCredCertifiable.Import();
    var useExport = new LeCheckIfNcpCredCertifiable.Export();

    useImport.CsePerson.Number = import.CsePerson.Number;
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    useImport.ExcludeFvi.Flag = import.ExcludeFvi.Flag;
    useImport.ObligationCreatedCutoff.Timestamp =
      import.ObligationCreatedCutoff.Timestamp;
    useImport.NcpCurrentlySentToCra.Flag = local.NcpCurrentlySentToCra.Flag;
    useImport.CreditReportingAction.Assign(local.MostRecentCseStatus);

    Call(LeCheckIfNcpCredCertifiable.Execute, useImport, useExport);

    local.ExcludeNcpFromCredRpt.Flag = useExport.ExcludeNcpFromCredRpt.Flag;
    MoveCreditReportingAction(useExport.CreditReportingAction, local.New1);
    local.Issuance.Number = useExport.Issuance.Number;
    local.ExcludedReason.Text9 = useExport.ExcludedReason.Text9;
  }

  private void UseSpCreateDocumentInfrastruct()
  {
    var useImport = new SpCreateDocumentInfrastruct.Import();
    var useExport = new SpCreateDocumentInfrastruct.Export();

    useImport.Document.Name = local.Document.Name;
    MoveSpDocKey(local.SpDocKey, useImport.SpDocKey);
    MoveInfrastructure(local.Infrastructure, useImport.Infrastructure);

    Call(SpCreateDocumentInfrastruct.Execute, useImport, useExport);
  }

  private void CreateCreditReporting()
  {
    System.Diagnostics.Debug.Assert(entities.Obligor.Populated);

    var cpaType = entities.Obligor.Type1;
    var cspNumber = entities.Obligor.CspNumber;
    var type1 = "CRED";
    var takenDate = import.ProgramProcessingInfo.ProcessDate;
    var aatType = entities.AdministrativeAction.Type1;
    var originalAmount = 0M;
    var currentAmount = local.CreditReporting.CurrentAmount.GetValueOrDefault();
    var currentAmountDate = local.CreditReporting.CurrentAmountDate;
    var decertifiedDate = local.Null1.Date;
    var notificationDate = local.CreditReporting.NotificationDate;
    var createdBy = global.UserId;
    var createdTstamp = Now();
    var notifiedBy = local.CreditReporting.NotifiedBy ?? "";
    var dateSent = local.CreditReporting.DateSent;
    var dateStayed = local.CreditReporting.DateStayed;
    var dateStayReleased = local.CreditReporting.DateStayReleased;
    var decertificationReason = Spaces(240);

    CheckValid<AdministrativeActCertification>("CpaType", cpaType);
    CheckValid<AdministrativeActCertification>("Type1", type1);
    entities.CreditReporting.Populated = false;
    Update("CreateCreditReporting",
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
        db.SetNullableDate(command, "decertifiedDt", decertifiedDate);
        db.SetNullableDate(command, "notificationDt", notificationDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdTstamp", null);
        db.SetNullableDate(command, "referralDt", default(DateTime));
        db.SetNullableDecimal(command, "recoveryAmt", originalAmount);
        db.SetString(command, "witness", "");
        db.SetNullableString(command, "notifiedBy", notifiedBy);
        db.SetNullableString(command, "reasonWithdraw", "");
        db.SetNullableString(command, "denialReason", "");
        db.SetNullableDate(command, "dateSent", dateSent);
        db.SetNullableDate(command, "apRespRecdDate", null);
        db.SetNullableDate(command, "dateStayed", dateStayed);
        db.SetNullableDate(command, "dateStayReleased", dateStayReleased);
        db.SetNullableDecimal(command, "highestAmount", originalAmount);
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
        db.SetNullableString(command, "decertifyReason", decertificationReason);
        db.SetNullableString(command, "addressStreet1", "");
        db.SetNullableString(command, "addressCity", "");
        db.SetNullableString(command, "addressZip", "");
      });

    entities.CreditReporting.CpaType = cpaType;
    entities.CreditReporting.CspNumber = cspNumber;
    entities.CreditReporting.Type1 = type1;
    entities.CreditReporting.TakenDate = takenDate;
    entities.CreditReporting.AatType = aatType;
    entities.CreditReporting.OriginalAmount = originalAmount;
    entities.CreditReporting.CurrentAmount = currentAmount;
    entities.CreditReporting.CurrentAmountDate = currentAmountDate;
    entities.CreditReporting.DecertifiedDate = decertifiedDate;
    entities.CreditReporting.NotificationDate = notificationDate;
    entities.CreditReporting.CreatedBy = createdBy;
    entities.CreditReporting.CreatedTstamp = createdTstamp;
    entities.CreditReporting.LastUpdatedBy = "";
    entities.CreditReporting.LastUpdatedTstamp = null;
    entities.CreditReporting.NotifiedBy = notifiedBy;
    entities.CreditReporting.DateSent = dateSent;
    entities.CreditReporting.ApRespReceivedData = null;
    entities.CreditReporting.DateStayed = dateStayed;
    entities.CreditReporting.DateStayReleased = dateStayReleased;
    entities.CreditReporting.HighestAmount = originalAmount;
    entities.CreditReporting.TanfCode = "";
    entities.CreditReporting.DecertificationReason = decertificationReason;
    entities.CreditReporting.Populated = true;
  }

  private void CreateCreditReportingAction()
  {
    System.Diagnostics.Debug.Assert(entities.CreditReporting.Populated);

    var identifier = local.New1.Identifier;
    var cseActionCode = local.New1.CseActionCode ?? "";
    var craTransCode = local.New1.CraTransCode;
    var craTransDate = local.New1.CraTransDate;
    var dateSentToCra = local.New1.DateSentToCra;
    var originalAmount = local.New1.OriginalAmount.GetValueOrDefault();
    var currentAmount = local.New1.CurrentAmount.GetValueOrDefault();
    var highestAmount = local.New1.HighestAmount.GetValueOrDefault();
    var cpaType = entities.CreditReporting.CpaType;
    var cspNumber = entities.CreditReporting.CspNumber;
    var aacType = entities.CreditReporting.Type1;
    var aacTakenDate = entities.CreditReporting.TakenDate;
    var aacTanfCode = entities.CreditReporting.TanfCode;

    CheckValid<CreditReportingAction>("CpaType", cpaType);
    CheckValid<CreditReportingAction>("AacType", aacType);
    entities.CreditReportingAction.Populated = false;
    Update("CreateCreditReportingAction",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", identifier);
        db.SetNullableString(command, "cseActionCode", cseActionCode);
        db.SetString(command, "craTransCode", craTransCode);
        db.SetNullableDate(command, "craTransDate", craTransDate);
        db.SetNullableDate(command, "dateSentToCra", dateSentToCra);
        db.SetNullableDecimal(command, "originalAmount", originalAmount);
        db.SetNullableDecimal(command, "currentAmount", currentAmount);
        db.SetNullableDecimal(command, "highestAmount", highestAmount);
        db.SetString(command, "cpaType", cpaType);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "aacType", aacType);
        db.SetDate(command, "aacTakenDate", aacTakenDate);
        db.SetString(command, "aacTanfCode", aacTanfCode);
      });

    entities.CreditReportingAction.Identifier = identifier;
    entities.CreditReportingAction.CseActionCode = cseActionCode;
    entities.CreditReportingAction.CraTransCode = craTransCode;
    entities.CreditReportingAction.CraTransDate = craTransDate;
    entities.CreditReportingAction.DateSentToCra = dateSentToCra;
    entities.CreditReportingAction.OriginalAmount = originalAmount;
    entities.CreditReportingAction.CurrentAmount = currentAmount;
    entities.CreditReportingAction.HighestAmount = highestAmount;
    entities.CreditReportingAction.CpaType = cpaType;
    entities.CreditReportingAction.CspNumber = cspNumber;
    entities.CreditReportingAction.AacType = aacType;
    entities.CreditReportingAction.AacTakenDate = aacTakenDate;
    entities.CreditReportingAction.AacTanfCode = aacTanfCode;
    entities.CreditReportingAction.Populated = true;
  }

  private bool ReadAdministrativeAction()
  {
    entities.AdministrativeAction.Populated = false;

    return Read("ReadAdministrativeAction",
      null,
      (db, reader) =>
      {
        entities.AdministrativeAction.Type1 = db.GetString(reader, 0);
        entities.AdministrativeAction.Populated = true;
      });
  }

  private bool ReadCreditReporting()
  {
    System.Diagnostics.Debug.Assert(entities.Obligor.Populated);
    entities.CreditReporting.Populated = false;

    return Read("ReadCreditReporting",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.Obligor.Type1);
        db.SetString(command, "cspNumber", entities.Obligor.CspNumber);
      },
      (db, reader) =>
      {
        entities.CreditReporting.CpaType = db.GetString(reader, 0);
        entities.CreditReporting.CspNumber = db.GetString(reader, 1);
        entities.CreditReporting.Type1 = db.GetString(reader, 2);
        entities.CreditReporting.TakenDate = db.GetDate(reader, 3);
        entities.CreditReporting.AatType = db.GetNullableString(reader, 4);
        entities.CreditReporting.OriginalAmount =
          db.GetNullableDecimal(reader, 5);
        entities.CreditReporting.CurrentAmount =
          db.GetNullableDecimal(reader, 6);
        entities.CreditReporting.CurrentAmountDate =
          db.GetNullableDate(reader, 7);
        entities.CreditReporting.DecertifiedDate =
          db.GetNullableDate(reader, 8);
        entities.CreditReporting.NotificationDate =
          db.GetNullableDate(reader, 9);
        entities.CreditReporting.CreatedBy = db.GetString(reader, 10);
        entities.CreditReporting.CreatedTstamp = db.GetDateTime(reader, 11);
        entities.CreditReporting.LastUpdatedBy =
          db.GetNullableString(reader, 12);
        entities.CreditReporting.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 13);
        entities.CreditReporting.NotifiedBy = db.GetNullableString(reader, 14);
        entities.CreditReporting.DateSent = db.GetNullableDate(reader, 15);
        entities.CreditReporting.ApRespReceivedData =
          db.GetNullableDate(reader, 16);
        entities.CreditReporting.DateStayed = db.GetNullableDate(reader, 17);
        entities.CreditReporting.DateStayReleased =
          db.GetNullableDate(reader, 18);
        entities.CreditReporting.HighestAmount =
          db.GetNullableDecimal(reader, 19);
        entities.CreditReporting.TanfCode = db.GetString(reader, 20);
        entities.CreditReporting.DecertificationReason =
          db.GetNullableString(reader, 21);
        entities.CreditReporting.Populated = true;
        CheckValid<AdministrativeActCertification>("CpaType",
          entities.CreditReporting.CpaType);
        CheckValid<AdministrativeActCertification>("Type1",
          entities.CreditReporting.Type1);
      });
  }

  private bool ReadCreditReportingAction1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligor.Populated);
    System.Diagnostics.Debug.Assert(entities.CreditReporting.Populated);
    local.New1.Populated = false;

    return Read("ReadCreditReportingAction1",
      (db, command) =>
      {
        db.SetDate(
          command, "aacTakenDate",
          entities.CreditReporting.TakenDate.GetValueOrDefault());
        db.SetString(command, "aacType", entities.CreditReporting.Type1);
        db.SetString(command, "aacTanfCode", entities.CreditReporting.TanfCode);
        db.SetString(command, "cspNumber1", entities.CreditReporting.CspNumber);
        db.SetString(command, "cpaType1", entities.CreditReporting.CpaType);
        db.SetString(command, "cpaType2", entities.Obligor.Type1);
        db.SetString(command, "cspNumber2", entities.Obligor.CspNumber);
      },
      (db, reader) =>
      {
        local.New1.Identifier = db.GetInt32(reader, 0);
        local.New1.Populated = true;
      });
  }

  private bool ReadCreditReportingAction2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligor.Populated);
    entities.MostRecentCraStatus.Populated = false;

    return Read("ReadCreditReportingAction2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "dateSentToCra", local.Null1.Date.GetValueOrDefault());
        db.SetString(command, "cpaType", entities.Obligor.Type1);
        db.SetString(command, "cspNumber", entities.Obligor.CspNumber);
      },
      (db, reader) =>
      {
        entities.MostRecentCraStatus.Identifier = db.GetInt32(reader, 0);
        entities.MostRecentCraStatus.CseActionCode =
          db.GetNullableString(reader, 1);
        entities.MostRecentCraStatus.CraTransCode = db.GetString(reader, 2);
        entities.MostRecentCraStatus.CraTransDate =
          db.GetNullableDate(reader, 3);
        entities.MostRecentCraStatus.DateSentToCra =
          db.GetNullableDate(reader, 4);
        entities.MostRecentCraStatus.OriginalAmount =
          db.GetNullableDecimal(reader, 5);
        entities.MostRecentCraStatus.CurrentAmount =
          db.GetNullableDecimal(reader, 6);
        entities.MostRecentCraStatus.HighestAmount =
          db.GetNullableDecimal(reader, 7);
        entities.MostRecentCraStatus.CpaType = db.GetString(reader, 8);
        entities.MostRecentCraStatus.CspNumber = db.GetString(reader, 9);
        entities.MostRecentCraStatus.AacType = db.GetString(reader, 10);
        entities.MostRecentCraStatus.AacTakenDate = db.GetDate(reader, 11);
        entities.MostRecentCraStatus.AacTanfCode = db.GetString(reader, 12);
        entities.MostRecentCraStatus.Populated = true;
        CheckValid<CreditReportingAction>("CpaType",
          entities.MostRecentCraStatus.CpaType);
        CheckValid<CreditReportingAction>("AacType",
          entities.MostRecentCraStatus.AacType);
      });
  }

  private IEnumerable<bool> ReadCreditReportingAction3()
  {
    System.Diagnostics.Debug.Assert(entities.Obligor.Populated);
    entities.MostRecentCseStatus.Populated = false;

    return ReadEach("ReadCreditReportingAction3",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.Obligor.Type1);
        db.SetString(command, "cspNumber", entities.Obligor.CspNumber);
      },
      (db, reader) =>
      {
        entities.MostRecentCseStatus.Identifier = db.GetInt32(reader, 0);
        entities.MostRecentCseStatus.CseActionCode =
          db.GetNullableString(reader, 1);
        entities.MostRecentCseStatus.CraTransCode = db.GetString(reader, 2);
        entities.MostRecentCseStatus.CraTransDate =
          db.GetNullableDate(reader, 3);
        entities.MostRecentCseStatus.DateSentToCra =
          db.GetNullableDate(reader, 4);
        entities.MostRecentCseStatus.OriginalAmount =
          db.GetNullableDecimal(reader, 5);
        entities.MostRecentCseStatus.CurrentAmount =
          db.GetNullableDecimal(reader, 6);
        entities.MostRecentCseStatus.HighestAmount =
          db.GetNullableDecimal(reader, 7);
        entities.MostRecentCseStatus.CpaType = db.GetString(reader, 8);
        entities.MostRecentCseStatus.CspNumber = db.GetString(reader, 9);
        entities.MostRecentCseStatus.AacType = db.GetString(reader, 10);
        entities.MostRecentCseStatus.AacTakenDate = db.GetDate(reader, 11);
        entities.MostRecentCseStatus.AacTanfCode = db.GetString(reader, 12);
        entities.MostRecentCseStatus.Populated = true;
        CheckValid<CreditReportingAction>("CpaType",
          entities.MostRecentCseStatus.CpaType);
        CheckValid<CreditReportingAction>("AacType",
          entities.MostRecentCseStatus.AacType);

        return true;
      });
  }

  private bool ReadCsePersonObligor()
  {
    entities.CsePerson.Populated = false;
    entities.Obligor.Populated = false;

    return Read("ReadCsePersonObligor",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.Obligor.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.Obligor.Type1 = db.GetString(reader, 2);
        entities.CsePerson.Populated = true;
        entities.Obligor.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
        CheckValid<CsePersonAccount>("Type1", entities.Obligor.Type1);
      });
  }

  private void UpdateCreditReporting()
  {
    System.Diagnostics.Debug.Assert(entities.CreditReporting.Populated);

    var originalAmount =
      local.CreditReporting.OriginalAmount.GetValueOrDefault();
    var currentAmount = local.CreditReporting.CurrentAmount.GetValueOrDefault();
    var currentAmountDate = local.CreditReporting.CurrentAmountDate;
    var decertifiedDate = local.CreditReporting.DecertifiedDate;
    var notificationDate = local.CreditReporting.NotificationDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTstamp = Now();
    var notifiedBy = local.CreditReporting.NotifiedBy ?? "";
    var dateSent = local.CreditReporting.DateSent;
    var dateStayed = local.CreditReporting.DateStayed;
    var dateStayReleased = local.CreditReporting.DateStayReleased;
    var highestAmount = local.CreditReporting.HighestAmount.GetValueOrDefault();
    var decertificationReason = local.CreditReporting.DecertificationReason ?? ""
      ;

    entities.CreditReporting.Populated = false;
    Update("UpdateCreditReporting",
      (db, command) =>
      {
        db.SetNullableDecimal(command, "originalAmt", originalAmount);
        db.SetNullableDecimal(command, "currentAmt", currentAmount);
        db.SetNullableDate(command, "currentAmtDt", currentAmountDate);
        db.SetNullableDate(command, "decertifiedDt", decertifiedDate);
        db.SetNullableDate(command, "notificationDt", notificationDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetNullableString(command, "notifiedBy", notifiedBy);
        db.SetNullableDate(command, "dateSent", dateSent);
        db.SetNullableDate(command, "dateStayed", dateStayed);
        db.SetNullableDate(command, "dateStayReleased", dateStayReleased);
        db.SetNullableDecimal(command, "highestAmount", highestAmount);
        db.SetNullableString(command, "decertifyReason", decertificationReason);
        db.SetString(command, "cpaType", entities.CreditReporting.CpaType);
        db.SetString(command, "cspNumber", entities.CreditReporting.CspNumber);
        db.SetString(command, "type", entities.CreditReporting.Type1);
        db.SetDate(
          command, "takenDt",
          entities.CreditReporting.TakenDate.GetValueOrDefault());
        db.SetString(command, "tanfCode", entities.CreditReporting.TanfCode);
      });

    entities.CreditReporting.OriginalAmount = originalAmount;
    entities.CreditReporting.CurrentAmount = currentAmount;
    entities.CreditReporting.CurrentAmountDate = currentAmountDate;
    entities.CreditReporting.DecertifiedDate = decertifiedDate;
    entities.CreditReporting.NotificationDate = notificationDate;
    entities.CreditReporting.LastUpdatedBy = lastUpdatedBy;
    entities.CreditReporting.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.CreditReporting.NotifiedBy = notifiedBy;
    entities.CreditReporting.DateSent = dateSent;
    entities.CreditReporting.DateStayed = dateStayed;
    entities.CreditReporting.DateStayReleased = dateStayReleased;
    entities.CreditReporting.HighestAmount = highestAmount;
    entities.CreditReporting.DecertificationReason = decertificationReason;
    entities.CreditReporting.Populated = true;
  }

  private void UpdateCreditReportingAction()
  {
    System.Diagnostics.Debug.Assert(entities.MostRecentCseStatus.Populated);

    var craTransCode = "DDA";
    var dateSentToCra = import.ProgramProcessingInfo.ProcessDate;
    var originalAmount = 0M;

    entities.MostRecentCseStatus.Populated = false;
    Update("UpdateCreditReportingAction",
      (db, command) =>
      {
        db.SetString(command, "craTransCode", craTransCode);
        db.SetNullableDate(command, "dateSentToCra", dateSentToCra);
        db.SetNullableDecimal(command, "originalAmount", originalAmount);
        db.SetNullableDecimal(command, "currentAmount", originalAmount);
        db.SetNullableDecimal(command, "highestAmount", originalAmount);
        db.SetInt32(
          command, "identifier", entities.MostRecentCseStatus.Identifier);
        db.SetString(command, "cpaType", entities.MostRecentCseStatus.CpaType);
        db.SetString(
          command, "cspNumber", entities.MostRecentCseStatus.CspNumber);
        db.SetString(command, "aacType", entities.MostRecentCseStatus.AacType);
        db.SetDate(
          command, "aacTakenDate",
          entities.MostRecentCseStatus.AacTakenDate.GetValueOrDefault());
        db.SetString(
          command, "aacTanfCode", entities.MostRecentCseStatus.AacTanfCode);
      });

    entities.MostRecentCseStatus.CraTransCode = craTransCode;
    entities.MostRecentCseStatus.DateSentToCra = dateSentToCra;
    entities.MostRecentCseStatus.OriginalAmount = originalAmount;
    entities.MostRecentCseStatus.CurrentAmount = originalAmount;
    entities.MostRecentCseStatus.HighestAmount = originalAmount;
    entities.MostRecentCseStatus.Populated = true;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of InformationLogging.
    /// </summary>
    [JsonPropertyName("informationLogging")]
    public Common InformationLogging
    {
      get => informationLogging ??= new();
      set => informationLogging = value;
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
    /// A value of ExcludeFvi.
    /// </summary>
    [JsonPropertyName("excludeFvi")]
    public Common ExcludeFvi
    {
      get => excludeFvi ??= new();
      set => excludeFvi = value;
    }

    /// <summary>
    /// A value of ObligationCreatedCutoff.
    /// </summary>
    [JsonPropertyName("obligationCreatedCutoff")]
    public DateWorkArea ObligationCreatedCutoff
    {
      get => obligationCreatedCutoff ??= new();
      set => obligationCreatedCutoff = value;
    }

    private CsePerson csePerson;
    private Common informationLogging;
    private ProgramProcessingInfo programProcessingInfo;
    private Common excludeFvi;
    private DateWorkArea obligationCreatedCutoff;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ErrorWritten.
    /// </summary>
    [JsonPropertyName("errorWritten")]
    public Common ErrorWritten
    {
      get => errorWritten ??= new();
      set => errorWritten = value;
    }

    /// <summary>
    /// A value of CredRecordCreated.
    /// </summary>
    [JsonPropertyName("credRecordCreated")]
    public Common CredRecordCreated
    {
      get => credRecordCreated ??= new();
      set => credRecordCreated = value;
    }

    /// <summary>
    /// A value of SendToCra.
    /// </summary>
    [JsonPropertyName("sendToCra")]
    public EabReportSend SendToCra
    {
      get => sendToCra ??= new();
      set => sendToCra = value;
    }

    /// <summary>
    /// A value of InfoLogging.
    /// </summary>
    [JsonPropertyName("infoLogging")]
    public EabReportSend InfoLogging
    {
      get => infoLogging ??= new();
      set => infoLogging = value;
    }

    private Common errorWritten;
    private Common credRecordCreated;
    private EabReportSend sendToCra;
    private EabReportSend infoLogging;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ExcludeNcpFromCredRpt.
    /// </summary>
    [JsonPropertyName("excludeNcpFromCredRpt")]
    public Common ExcludeNcpFromCredRpt
    {
      get => excludeNcpFromCredRpt ??= new();
      set => excludeNcpFromCredRpt = value;
    }

    /// <summary>
    /// A value of NcpCurrentlySentToCra.
    /// </summary>
    [JsonPropertyName("ncpCurrentlySentToCra")]
    public Common NcpCurrentlySentToCra
    {
      get => ncpCurrentlySentToCra ??= new();
      set => ncpCurrentlySentToCra = value;
    }

    /// <summary>
    /// A value of MostRecentCraStatus.
    /// </summary>
    [JsonPropertyName("mostRecentCraStatus")]
    public CreditReportingAction MostRecentCraStatus
    {
      get => mostRecentCraStatus ??= new();
      set => mostRecentCraStatus = value;
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
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
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
    /// A value of MostRecentCseStatus.
    /// </summary>
    [JsonPropertyName("mostRecentCseStatus")]
    public CreditReportingAction MostRecentCseStatus
    {
      get => mostRecentCseStatus ??= new();
      set => mostRecentCseStatus = value;
    }

    /// <summary>
    /// A value of TbdLocalNcpMeetsCredCriter.
    /// </summary>
    [JsonPropertyName("tbdLocalNcpMeetsCredCriter")]
    public Common TbdLocalNcpMeetsCredCriter
    {
      get => tbdLocalNcpMeetsCredCriter ??= new();
      set => tbdLocalNcpMeetsCredCriter = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public CreditReportingAction New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of Issuance.
    /// </summary>
    [JsonPropertyName("issuance")]
    public Case1 Issuance
    {
      get => issuance ??= new();
      set => issuance = value;
    }

    /// <summary>
    /// A value of ExcludedReason.
    /// </summary>
    [JsonPropertyName("excludedReason")]
    public WorkArea ExcludedReason
    {
      get => excludedReason ??= new();
      set => excludedReason = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of CreditReporting.
    /// </summary>
    [JsonPropertyName("creditReporting")]
    public AdministrativeActCertification CreditReporting
    {
      get => creditReporting ??= new();
      set => creditReporting = value;
    }

    /// <summary>
    /// A value of TbdLocalAll.
    /// </summary>
    [JsonPropertyName("tbdLocalAll")]
    public AdministrativeActCertification TbdLocalAll
    {
      get => tbdLocalAll ??= new();
      set => tbdLocalAll = value;
    }

    private Common excludeNcpFromCredRpt;
    private Common ncpCurrentlySentToCra;
    private CreditReportingAction mostRecentCraStatus;
    private DateWorkArea null1;
    private ExitStateWorkArea exitStateWorkArea;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private CreditReportingAction mostRecentCseStatus;
    private Common tbdLocalNcpMeetsCredCriter;
    private CreditReportingAction new1;
    private Case1 issuance;
    private WorkArea excludedReason;
    private Document document;
    private SpDocKey spDocKey;
    private Infrastructure infrastructure;
    private AdministrativeActCertification creditReporting;
    private AdministrativeActCertification tbdLocalAll;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of CreditReporting.
    /// </summary>
    [JsonPropertyName("creditReporting")]
    public AdministrativeActCertification CreditReporting
    {
      get => creditReporting ??= new();
      set => creditReporting = value;
    }

    /// <summary>
    /// A value of MostRecentCraStatus.
    /// </summary>
    [JsonPropertyName("mostRecentCraStatus")]
    public CreditReportingAction MostRecentCraStatus
    {
      get => mostRecentCraStatus ??= new();
      set => mostRecentCraStatus = value;
    }

    /// <summary>
    /// A value of MostRecentCseStatus.
    /// </summary>
    [JsonPropertyName("mostRecentCseStatus")]
    public CreditReportingAction MostRecentCseStatus
    {
      get => mostRecentCseStatus ??= new();
      set => mostRecentCseStatus = value;
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
    /// A value of CreditReportingAction.
    /// </summary>
    [JsonPropertyName("creditReportingAction")]
    public CreditReportingAction CreditReportingAction
    {
      get => creditReportingAction ??= new();
      set => creditReportingAction = value;
    }

    private CsePerson csePerson;
    private CsePersonAccount obligor;
    private AdministrativeActCertification creditReporting;
    private CreditReportingAction mostRecentCraStatus;
    private CreditReportingAction mostRecentCseStatus;
    private AdministrativeAction administrativeAction;
    private CreditReportingAction creditReportingAction;
  }
#endregion
}
