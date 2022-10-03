// Program: LE_CHECK_IF_NCP_CRED_CERTIFIABLE, ID: 1902622953, model: 746.
// Short name: SWE00736
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_CHECK_IF_NCP_CRED_CERTIFIABLE.
/// </summary>
[Serializable]
public partial class LeCheckIfNcpCredCertifiable: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_CHECK_IF_NCP_CRED_CERTIFIABLE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeCheckIfNcpCredCertifiable(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeCheckIfNcpCredCertifiable.
  /// </summary>
  public LeCheckIfNcpCredCertifiable(IContext context, Import import,
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
    // 					Created this cab to contain existing and new
    // 					certification logic.  Some logic from
    // 					le_create_cred_certification was moved into
    // 					this new cab.
    // ---------------------------------------------------------------------------------------------
    MoveCreditReportingAction(import.CreditReportingAction,
      export.CreditReportingAction);
    export.ExcludeNcpFromCredRpt.Flag = "N";
    local.FirstDayOfMonth.Date =
      AddDays(import.ProgramProcessingInfo.ProcessDate,
      -(Day(import.ProgramProcessingInfo.ProcessDate) - 1));

    // -------------------------------------------------------------------------------------
    // Read NCP person record to get the Family Violence indicator.
    // -------------------------------------------------------------------------------------
    if (!ReadCsePersonObligor())
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    // -------------------------------------------------------------------------------------
    // Get SSN and Date of Birth.
    // -------------------------------------------------------------------------------------
    local.CsePersonsWorkSet.Number = import.CsePerson.Number;
    UseCabReadAdabasPersonBatch();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // -------------------------------------------------------------------------------------
    // NCP must have at least one of SSN or Date of Birth.
    // -------------------------------------------------------------------------------------
    if ((IsEmpty(local.CsePersonsWorkSet.Ssn) || Equal
      (local.CsePersonsWorkSet.Ssn, "000000000")) && (
        Equal(local.CsePersonsWorkSet.Dob, local.Null1.Date) || Equal
      (local.CsePersonsWorkSet.Dob, new DateTime(1, 1, 1))))
    {
      // --Metro 2 standards require that the NCP has at least one of SSN or 
      // Date of Birth.
      // --NCP cannot be certified for credit reporting.
      export.ExcludeNcpFromCredRpt.Flag = "Y";
      export.ExcludedReason.Text9 = "NOSSNDOB";

      return;
    }

    // -------------------------------------------------------------------------------------
    // If NCP has a Date of Birth they must be at least 18 years of age.
    // -------------------------------------------------------------------------------------
    if (Lt(local.Null1.Date, local.CsePersonsWorkSet.Dob) && Lt
      (AddYears(import.ProgramProcessingInfo.ProcessDate, -18),
      local.CsePersonsWorkSet.Dob))
    {
      // --NCP is under 18 years of age.
      // --NCP cannot be certified for credit reporting.
      export.ExcludeNcpFromCredRpt.Flag = "Y";
      export.ExcludedReason.Text9 = "UNDER18";

      return;
    }

    // -------------------------------------------------------------------------------------
    // NCP must have an address to be initially certified for credit reporting.
    // -------------------------------------------------------------------------------------
    if (AsChar(import.NcpCurrentlySentToCra.Flag) == 'N')
    {
      if (ReadCsePersonAddress())
      {
        // --continue processing
      }
      else
      {
        // --NCP does not have an address.
        // --NCP cannot be certified for credit reporting.
        export.ExcludeNcpFromCredRpt.Flag = "Y";
        export.ExcludedReason.Text9 = "NOADDRESS";

        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // Note that FVI is not enabled, but was coded for potential future use.
    // -------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------
    // Check for Family Violence (if import flag indicates FVI is turned on)
    // -------------------------------------------------------------------------------------
    if (AsChar(import.ExcludeFvi.Flag) == 'Y' && !
      IsEmpty(entities.CsePerson.FamilyViolenceIndicator))
    {
      // --Family Violence exclusions are turned on for Credit Reporting and the
      // NCP has
      //   Family Violence set.  Excluded the NCP from credit reporting.
      export.CreditReportingAction.CurrentAmount = 0;

      return;
    }

    // --Left this in case Bankruptcy exclusions ever need to be turned back on.
    local.TotalCredCertifyAmount.TotalCurrency = 0;
    local.TotalDebtMoreThan4Mth.BalanceDueAmt = 0;
    local.TotalDebtMoreThan4Mth.InterestBalanceDueAmt = 0;
    local.TotalDebtLessThan4Mth.BalanceDueAmt = 0;
    local.TotalDebtLessThan4Mth.InterestBalanceDueAmt = 0;
    local.AdministrativeActCertification.Type1 = "CRED";
    local.AllObligForAp.Index = -1;
    local.DebtTotByCase.Index = -1;

    if (AsChar(import.NcpCurrentlySentToCra.Flag) == 'N')
    {
      // -------------------------------------------------------------------------------------
      // Before we go to any more trouble verify that the NCP has a debt balance
      // > $1000.
      // Most NCPs who are not currently certified will not have arrears > $1000
      // so there
      // is no sense incurring any extra processing overhead for them.
      // -------------------------------------------------------------------------------------
      ReadDebtDetail();

      if (local.TotalNcpDebtOwed.TotalCurrency < 1000)
      {
        // --NCP has arrears < $1000.  They won't qualify for CRED certification
        // so skip the
        //   remainder of the processing.
        export.CreditReportingAction.CurrentAmount = 0;

        return;
      }
      else
      {
        // --NCP has arrears > $1000.  Continue.
      }

      // -------------------------------------------------------------------------------------
      // Build a group of all the NCPs cases and legal actions.  This group will
      // be used to
      // determine which case has the most arrears.  That case number is used 
      // for the issuance
      // document sent to the NCP.
      // -------------------------------------------------------------------------------------
      // -------------------------------------------------------------------------------------
      // Date:02/26/2007  Who: Raj S    Ref: PR#00300253
      // Change Description: Moved the legal action classification check outside
      // the loop.
      // -------------------------------------------------------------------------------------
      foreach(var item in ReadCaseRoleCase())
      {
        // 8/11/2004 check for legal action classification of J (only J will 
        // have debts)
        foreach(var item1 in ReadLegalAction2())
        {
          if (local.DebtTotByCase.Index + 1 == Local
            .DebtTotByCaseGroup.Capacity)
          {
            ExitState = "OE0000_MAX_NUMBER_OF_ENTRIES_EX";

            return;
          }

          local.DebtTotByCase.Index = local.DebtTotByCase.Count;
          local.DebtTotByCase.CheckSize();

          local.DebtTotByCase.Update.DebtTotByCaseCase.Number =
            entities.Case1.Number;
          local.DebtTotByCase.Update.DebtTotByCaseLegalAction.Identifier =
            entities.LegalAction.Identifier;
        }
      }
    }

    // -------------------------------------------------------------------------------------
    // The following read for good cause is for processing efficiency only.  
    // Determine if
    // the AP has been marked good cause in any case.   Once we have determined 
    // if he has
    // been marked good cause in any case, then we can check the individual 
    // debts later in
    // the process.
    // -------------------------------------------------------------------------------------
    // cmj 5/6/2004  -old cases will be out here so don't qualify read with only
    // open cases - new cases will be qualified.
    // cmj 1/20/2005  added case to read each to get the case number  because of
    // case number not there when reissue is done on a document from a cancel
    // later in program.   Using as a safe guard. for determining what is wrong
    // with program
    local.GoodCauseFoundForAp.Flag = "N";
    local.ActiveApCaseRoleFound.Flag = "N";

    foreach(var item in ReadCaseRole())
    {
      local.ActiveApCaseRoleFound.Flag = "Y";

      if (ReadGoodCause2())
      {
        local.GoodCauseFoundForAp.Flag = "Y";

        break;
      }
    }

    // -------------------------------------------------------------------------------------
    // Read all obligations tied to the obligor where:
    //   1) Obligation type is a support obligation (CS,SP,MS MJ,IJ,AJ,CRCH,SAJ,
    // 718B,MC).
    //   2) Obligation is not a secondary obligation.
    //   3) Obligation was created prior to the cutoff date.
    // -------------------------------------------------------------------------------------
    // --Now incoming interstate cases are reported (obligation order_type_code 
    // = 'I').
    //   However outgoing interstate case will not be reported. Outgoing will be
    // handled by
    //   exemptions. CQ26118 1/23/2014.
    // --CMJ 4/14/2004 added MJ (medical judgement per request of Brian 
    // Windmeyer - business   rules reflect chg  pr 204813
    foreach(var item in ReadObligationObligationType())
    {
      // -------------------------------------------------------------------------------------
      // Read legal action to verify that obligation is court ordered.  Must be 
      // court ordered
      // to report to credit reporting agencies.
      // -------------------------------------------------------------------------------------
      if (ReadLegalAction1())
      {
        // --Obligation is court ordered.  Continue Processing for AP
      }
      else
      {
        // -- Obligation is not court ordered.  Bypass obligation
        continue;
      }

      // -------------------------------------------------------------------------------------
      // Bypass temporary obligations.
      // -------------------------------------------------------------------------------------
      if (AsChar(entities.LegalAction.Type1) == 'T')
      {
        continue;
      }

      // -------------------------------------------------------------------------------------
      // Currently all obligation types are CRED certifiable, thus this call is 
      // commented.
      // -------------------------------------------------------------------------------------
      // -------------------------------------------------------------------------------------
      // This obligation is a keeper, store it in the local group
      // -------------------------------------------------------------------------------------
      if (entities.Obligation.SystemGeneratedIdentifier != local
        .Previous.SystemGeneratedIdentifier)
      {
        ++local.AllObligForAp.Index;
        local.AllObligForAp.CheckSize();

        local.AllObligForAp.Update.AllObligForAp1.SystemGeneratedIdentifier =
          entities.Obligation.SystemGeneratedIdentifier;
        local.Previous.SystemGeneratedIdentifier =
          entities.Obligation.SystemGeneratedIdentifier;
      }

      // -------------------------------------------------------------------------------------
      // Check for active exemptions on obligation.
      // -------------------------------------------------------------------------------------
      foreach(var item1 in ReadObligationAdmActionExemptionAdministrativeAction())
        
      {
        // -- Obligation has been administratively exempted.  Skip the 
        // obligation.
        goto ReadEach;
      }

      // -------------------------------------------------------------------------------------
      // Read the active debts associated to the current obligation.
      // -------------------------------------------------------------------------------------
      foreach(var item1 in ReadDebtDebtDetail())
      {
        // -------------------------------------------------------------------------------------
        // Accruing obligations are only in arrears when the debt due date is 
        // less than the
        // first of the month, versus less than the PPI process date of non-
        // accruing
        // obligations.  Omit accruing debts that are not in arrears.  Currently
        // accruing
        // obligations are CS,SP,MS and MC types.
        // -------------------------------------------------------------------------------------
        if (entities.ObligationType.SystemGeneratedIdentifier == 1 || entities
          .ObligationType.SystemGeneratedIdentifier == 2 || entities
          .ObligationType.SystemGeneratedIdentifier == 3 || entities
          .ObligationType.SystemGeneratedIdentifier == 19)
        {
          if (Lt(entities.DebtDetail.DueDt, local.FirstDayOfMonth.Date))
          {
            // -- Reportable arrears debt
          }
          else
          {
            continue;
          }
        }

        // -------------------------------------------------------------------------------------
        // Check for good cause on the debt.
        // -------------------------------------------------------------------------------------
        if (AsChar(local.GoodCauseFoundForAp.Flag) == 'Y')
        {
          UseLeCheckGoodCauseForDebt();

          if (AsChar(local.GoodCauseFoundForDebt.Flag) == 'Y')
          {
            // -- Skip the debt.
            continue;
          }
        }

        // -------------------------------------------------------------------------------------
        // Sum the debt detail balance due amount by case
        // -------------------------------------------------------------------------------------
        if (AsChar(import.NcpCurrentlySentToCra.Flag) == 'N')
        {
          for(local.DebtTotByCase.Index = 0; local.DebtTotByCase.Index < local
            .DebtTotByCase.Count; ++local.DebtTotByCase.Index)
          {
            if (!local.DebtTotByCase.CheckSize())
            {
              break;
            }

            if (entities.LegalAction.Identifier == local
              .DebtTotByCase.Item.DebtTotByCaseLegalAction.Identifier)
            {
              local.DebtTotByCase.Update.DebtTotByCaseDebtDetail.BalanceDueAmt =
                local.DebtTotByCase.Item.DebtTotByCaseDebtDetail.BalanceDueAmt +
                entities.DebtDetail.BalanceDueAmt;
            }
          }

          local.DebtTotByCase.CheckIndex();
        }

        // -------------------------------------------------------------------------------------
        // Sum the debt detail balance due amount by greater and less than 4 
        // months in arrears.
        // -------------------------------------------------------------------------------------
        if (entities.DebtDetail.BalanceDueAmt + entities
          .DebtDetail.InterestBalanceDueAmt.GetValueOrDefault() > 0)
        {
          local.TotalCredCertifyAmount.TotalCurrency =
            local.TotalCredCertifyAmount.TotalCurrency + entities
            .DebtDetail.BalanceDueAmt + entities
            .DebtDetail.InterestBalanceDueAmt.GetValueOrDefault();

          if (!Lt(import.ProgramProcessingInfo.ProcessDate,
            AddMonths(entities.DebtDetail.DueDt, 4)))
          {
            local.TotalDebtMoreThan4Mth.BalanceDueAmt += entities.DebtDetail.
              BalanceDueAmt;
            local.TotalDebtMoreThan4Mth.InterestBalanceDueAmt =
              local.TotalDebtMoreThan4Mth.InterestBalanceDueAmt.
                GetValueOrDefault() + entities
              .DebtDetail.InterestBalanceDueAmt.GetValueOrDefault();
          }
          else
          {
            local.TotalDebtLessThan4Mth.BalanceDueAmt += entities.DebtDetail.
              BalanceDueAmt;
            local.TotalDebtLessThan4Mth.InterestBalanceDueAmt =
              local.TotalDebtLessThan4Mth.InterestBalanceDueAmt.
                GetValueOrDefault() + entities
              .DebtDetail.InterestBalanceDueAmt.GetValueOrDefault();
          }
        }
      }

ReadEach:
      ;
    }

    switch(AsChar(import.NcpCurrentlySentToCra.Flag))
    {
      case 'Y':
        // NCP is currently certified with CRA.
        if (local.TotalCredCertifyAmount.TotalCurrency < 1)
        {
          export.CreditReportingAction.CurrentAmount = 0;

          // --Leave original and highest amounts set as they were.
          // -------------------------------------------------------------------------------------
          // Determine why the arrears dropped below $1.
          // -------------------------------------------------------------------------------------
          // --Further delineate why no arrears.  Paid off, written off, all 
          // cases closed, etc.
          if (AsChar(local.ActiveApCaseRoleFound.Flag) == 'Y')
          {
            // --NCPs certifiable debts are less than $1.00 and the NCP remains 
            // active on one or
            //   more case.  (The NCP is paid current)
            export.CreditReportingAction.CraTransCode = "U11";
          }
          else
          {
            // --The NCP is no longer active on any cases.  Determine if the 
            // debts were paid off or
            //   written off.
            // --Find most recent debt paid date.
            ReadCollection();

            // --Find most recent debt adjustment date for reason code CLOSECA 
            // or WO ALL.
            ReadDebtAdjustment();

            if (Lt(local.MostRecentCollection.CollectionDt,
              local.MostRecentDebtAdjustment.DebtAdjustmentDt))
            {
              // --NCPs debts were written off.
              export.CreditReportingAction.CraTransCode = "U64";
            }
            else
            {
              // --NCP debts were paid off.
              export.CreditReportingAction.CraTransCode = "U13";
            }
          }
        }
        else
        {
          // -------------------------------------------------------------------------------------
          // Certifiable debt balance remains $1.00 or more.  The NCP still 
          // meets credit
          // reporting criteria.
          // -------------------------------------------------------------------------------------
          export.CreditReportingAction.CurrentAmount =
            local.TotalCredCertifyAmount.TotalCurrency;

          if (local.TotalCredCertifyAmount.TotalCurrency > export
            .CreditReportingAction.HighestAmount.GetValueOrDefault())
          {
            export.CreditReportingAction.HighestAmount =
              local.TotalCredCertifyAmount.TotalCurrency;
          }

          // --Leave original amount set as is.
          if (local.TotalDebtMoreThan4Mth.BalanceDueAmt + local
            .TotalDebtMoreThan4Mth.InterestBalanceDueAmt.GetValueOrDefault() ==
              0)
          {
            // --NCPs debts are less than 4 months old.
            export.CreditReportingAction.CraTransCode = "U71";
          }
          else
          {
            // --NCP has debts more than 4 months old.
            export.CreditReportingAction.CraTransCode = "U93";
          }
        }

        break;
      case 'N':
        // NCP is NOT currently certified with CRA.
        if (local.TotalCredCertifyAmount.TotalCurrency >= 1000)
        {
          // -------------------------------------------------------------------------------------
          // Certifiable debt balance is at least $1000.  The NCP meets credit 
          // reporting threshold.
          // -------------------------------------------------------------------------------------
          if (local.TotalDebtMoreThan4Mth.BalanceDueAmt + local
            .TotalDebtMoreThan4Mth.InterestBalanceDueAmt.GetValueOrDefault() ==
              0)
          {
            // --NCPs debts are less than 4 months old.
            export.CreditReportingAction.CraTransCode = "A71";
          }
          else
          {
            // --NCP has debts more than 4 months old.
            export.CreditReportingAction.CraTransCode = "A93";
          }

          export.CreditReportingAction.OriginalAmount =
            local.TotalCredCertifyAmount.TotalCurrency;
          export.CreditReportingAction.CurrentAmount =
            local.TotalCredCertifyAmount.TotalCurrency;

          if (local.TotalCredCertifyAmount.TotalCurrency > export
            .CreditReportingAction.HighestAmount.GetValueOrDefault())
          {
            export.CreditReportingAction.HighestAmount =
              local.TotalCredCertifyAmount.TotalCurrency;
          }

          // -------------------------------------------------------------------------------------
          // Get case with highest debt balance.  On a obligor not previously 
          // reported, we pass
          // the case number to the document process.  This case number is then 
          // displayed on the
          // credit reporting notification document sent to the AP.
          // -------------------------------------------------------------------------------------
          local.HighAmtForCaseCase.Number = "";
          local.HighAmtForCaseDebtDetail.BalanceDueAmt = 0;

          for(local.DebtTotByCase.Index = 0; local.DebtTotByCase.Index < local
            .DebtTotByCase.Count; ++local.DebtTotByCase.Index)
          {
            if (!local.DebtTotByCase.CheckSize())
            {
              break;
            }

            if (local.DebtTotByCase.Item.DebtTotByCaseDebtDetail.
              BalanceDueAmt > local.HighAmtForCaseDebtDetail.BalanceDueAmt)
            {
              local.HighAmtForCaseCase.Number =
                local.DebtTotByCase.Item.DebtTotByCaseCase.Number;
              local.HighAmtForCaseDebtDetail.BalanceDueAmt =
                local.DebtTotByCase.Item.DebtTotByCaseDebtDetail.BalanceDueAmt;
            }
          }

          local.DebtTotByCase.CheckIndex();

          // ----------------------------------------------------------
          //  Determine Case Number for Issuance Document
          // ----------------------------------------------------------
          if (IsEmpty(local.HighAmtForCaseCase.Number))
          {
            // --This shouldn't be necessary, but just in case.
            // --Read the case on which the AP has most recently become active.
            foreach(var item in ReadCaseCaseRole())
            {
              if (ReadGoodCause1())
              {
                // --Skip this case if there is good cause.
                continue;
              }

              export.Issuance.Number = entities.Case1.Number;

              return;
            }
          }
          else
          {
            export.Issuance.Number = local.HighAmtForCaseCase.Number;
          }
        }

        break;
      default:
        break;
    }
  }

  private static void MoveCreditReportingAction(CreditReportingAction source,
    CreditReportingAction target)
  {
    target.OriginalAmount = source.OriginalAmount;
    target.CurrentAmount = source.CurrentAmount;
    target.HighestAmount = source.HighestAmount;
  }

  private void UseCabReadAdabasPersonBatch()
  {
    var useImport = new CabReadAdabasPersonBatch.Import();
    var useExport = new CabReadAdabasPersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(CabReadAdabasPersonBatch.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseLeCheckGoodCauseForDebt()
  {
    var useImport = new LeCheckGoodCauseForDebt.Import();
    var useExport = new LeCheckGoodCauseForDebt.Export();

    useImport.ObligationType.SystemGeneratedIdentifier =
      entities.ObligationType.SystemGeneratedIdentifier;
    useImport.Obligation.SystemGeneratedIdentifier =
      entities.Obligation.SystemGeneratedIdentifier;
    useImport.Debt.SystemGeneratedIdentifier =
      entities.Debt.SystemGeneratedIdentifier;
    useImport.Obligor.Number = import.CsePerson.Number;
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;

    Call(LeCheckGoodCauseForDebt.Execute, useImport, useExport);

    local.GoodCauseFoundForDebt.Flag = useExport.GoodCauseFoundForDebt.Flag;
  }

  private IEnumerable<bool> ReadCaseCaseRole()
  {
    entities.Case1.Populated = false;
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCaseCaseRole",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.CaseRole.CspNumber = db.GetString(reader, 2);
        entities.CaseRole.Type1 = db.GetString(reader, 3);
        entities.CaseRole.Identifier = db.GetInt32(reader, 4);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 6);
        entities.Case1.Populated = true;
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseRole()
  {
    entities.Ap.Populated = false;

    return ReadEach("ReadCaseRole",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "startDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Ap.CasNumber = db.GetString(reader, 0);
        entities.Ap.CspNumber = db.GetString(reader, 1);
        entities.Ap.Type1 = db.GetString(reader, 2);
        entities.Ap.Identifier = db.GetInt32(reader, 3);
        entities.Ap.StartDate = db.GetNullableDate(reader, 4);
        entities.Ap.EndDate = db.GetNullableDate(reader, 5);
        entities.Ap.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Ap.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseRoleCase()
  {
    entities.Case1.Populated = false;
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCaseRoleCase",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "startDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.Case1.Status = db.GetNullableString(reader, 6);
        entities.Case1.Populated = true;
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);

        return true;
      });
  }

  private bool ReadCollection()
  {
    System.Diagnostics.Debug.Assert(entities.Obligor.Populated);
    local.MostRecentCollection.Populated = false;

    return Read("ReadCollection",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.Obligor.Type1);
        db.SetString(command, "cspNumber", entities.Obligor.CspNumber);
      },
      (db, reader) =>
      {
        local.MostRecentCollection.CollectionDt = db.GetDate(reader, 0);
        local.MostRecentCollection.Populated = true;
      });
  }

  private bool ReadCsePersonAddress()
  {
    entities.CsePersonAddress.Populated = false;

    return Read("ReadCsePersonAddress",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePersonAddress.Identifier = db.GetDateTime(reader, 0);
        entities.CsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.CsePersonAddress.VerifiedDate = db.GetNullableDate(reader, 2);
        entities.CsePersonAddress.LocationType = db.GetString(reader, 3);
        entities.CsePersonAddress.Populated = true;
        CheckValid<CsePersonAddress>("LocationType",
          entities.CsePersonAddress.LocationType);
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
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 2);
        entities.Obligor.Type1 = db.GetString(reader, 3);
        entities.CsePerson.Populated = true;
        entities.Obligor.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
        CheckValid<CsePersonAccount>("Type1", entities.Obligor.Type1);
      });
  }

  private bool ReadDebtAdjustment()
  {
    System.Diagnostics.Debug.Assert(entities.Obligor.Populated);
    local.MostRecentDebtAdjustment.Populated = false;

    return Read("ReadDebtAdjustment",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.Obligor.Type1);
        db.SetString(command, "cspNumber", entities.Obligor.CspNumber);
      },
      (db, reader) =>
      {
        local.MostRecentDebtAdjustment.DebtAdjustmentDt = db.GetDate(reader, 0);
        local.MostRecentDebtAdjustment.Populated = true;
      });
  }

  private IEnumerable<bool> ReadDebtDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.DebtDetail.Populated = false;
    entities.Debt.Populated = false;

    return ReadEach("ReadDebtDebtDetail",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetDate(
          command, "dueDt",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "retiredDt", local.Null1.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.Debt.Type1 = db.GetString(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 4);
        entities.Debt.DebtType = db.GetString(reader, 5);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 6);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 7);
        entities.Debt.OtyType = db.GetInt32(reader, 8);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 8);
        entities.DebtDetail.DueDt = db.GetDate(reader, 9);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 10);
        entities.DebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 11);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 12);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 13);
        entities.DebtDetail.CoveredPrdEndDt = db.GetNullableDate(reader, 14);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 15);
        entities.DebtDetail.Populated = true;
        entities.Debt.Populated = true;
        CheckValid<ObligationTransaction>("CpaType", entities.Debt.CpaType);
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
        CheckValid<ObligationTransaction>("DebtType", entities.Debt.DebtType);
        CheckValid<ObligationTransaction>("CpaSupType", entities.Debt.CpaSupType);
          

        return true;
      });
  }

  private bool ReadDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.Obligor.Populated);

    return Read("ReadDebtDetail",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "retiredDt", local.Null1.Date.GetValueOrDefault());
        db.SetString(command, "cpaType", entities.Obligor.Type1);
        db.SetString(command, "cspNumber", entities.Obligor.CspNumber);
      },
      (db, reader) =>
      {
        local.TotalNcpDebtOwed.TotalCurrency = db.GetDecimal(reader, 0);
      });
  }

  private bool ReadGoodCause1()
  {
    System.Diagnostics.Debug.Assert(entities.CaseRole.Populated);
    entities.GoodCause.Populated = false;

    return Read("ReadGoodCause1",
      (db, command) =>
      {
        db.
          SetNullableString(command, "casNumber1", entities.CaseRole.CasNumber);
          
        db.SetNullableInt32(
          command, "croIdentifier1", entities.CaseRole.Identifier);
        db.SetNullableString(command, "croType1", entities.CaseRole.Type1);
        db.
          SetNullableString(command, "cspNumber1", entities.CaseRole.CspNumber);
          
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

  private bool ReadGoodCause2()
  {
    System.Diagnostics.Debug.Assert(entities.Ap.Populated);
    entities.GoodCause.Populated = false;

    return Read("ReadGoodCause2",
      (db, command) =>
      {
        db.SetNullableString(command, "casNumber1", entities.Ap.CasNumber);
        db.SetNullableInt32(command, "croIdentifier1", entities.Ap.Identifier);
        db.SetNullableString(command, "croType1", entities.Ap.Type1);
        db.SetNullableString(command, "cspNumber1", entities.Ap.CspNumber);
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

  private bool ReadLegalAction1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction1",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          entities.Obligation.LgaId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.Type1 = db.GetString(reader, 2);
        entities.LegalAction.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalAction2()
  {
    System.Diagnostics.Debug.Assert(entities.CaseRole.Populated);
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalAction2",
      (db, command) =>
      {
        db.SetInt32(command, "croIdentifier", entities.CaseRole.Identifier);
        db.SetString(command, "croType", entities.CaseRole.Type1);
        db.SetString(command, "cspNumber", entities.CaseRole.CspNumber);
        db.SetString(command, "casNumber", entities.CaseRole.CasNumber);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.Type1 = db.GetString(reader, 2);
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool>
    ReadObligationAdmActionExemptionAdministrativeAction()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationAdmActionExemption.Populated = false;
    entities.AdministrativeAction.Populated = false;

    return ReadEach("ReadObligationAdmActionExemptionAdministrativeAction",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetDate(
          command, "effectiveDt",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetDate(command, "endDt", local.Null1.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligationAdmActionExemption.OtyType = db.GetInt32(reader, 0);
        entities.ObligationAdmActionExemption.AatType = db.GetString(reader, 1);
        entities.AdministrativeAction.Type1 = db.GetString(reader, 1);
        entities.ObligationAdmActionExemption.ObgGeneratedId =
          db.GetInt32(reader, 2);
        entities.ObligationAdmActionExemption.CspNumber =
          db.GetString(reader, 3);
        entities.ObligationAdmActionExemption.CpaType = db.GetString(reader, 4);
        entities.ObligationAdmActionExemption.EffectiveDate =
          db.GetDate(reader, 5);
        entities.ObligationAdmActionExemption.EndDate = db.GetDate(reader, 6);
        entities.ObligationAdmActionExemption.Description =
          db.GetNullableString(reader, 7);
        entities.ObligationAdmActionExemption.Populated = true;
        entities.AdministrativeAction.Populated = true;
        CheckValid<ObligationAdmActionExemption>("CpaType",
          entities.ObligationAdmActionExemption.CpaType);

        return true;
      });
  }

  private IEnumerable<bool> ReadObligationObligationType()
  {
    System.Diagnostics.Debug.Assert(entities.Obligor.Populated);
    entities.ObligationType.Populated = false;
    entities.Obligation.Populated = false;

    return ReadEach("ReadObligationObligationType",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTmst",
          import.ObligationCreatedCutoff.Timestamp.GetValueOrDefault());
        db.SetString(command, "cpaType", entities.Obligor.Type1);
        db.SetString(command, "cspNumber", entities.Obligor.CspNumber);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 5);
        entities.Obligation.AsOfDtTotBalCurrArr =
          db.GetNullableDecimal(reader, 6);
        entities.Obligation.CreatedTmst = db.GetDateTime(reader, 7);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 8);
        entities.ObligationType.SupportedPersonReqInd = db.GetString(reader, 9);
        entities.ObligationType.Populated = true;
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.Obligation.OrderTypeCode);
        CheckValid<ObligationType>("SupportedPersonReqInd",
          entities.ObligationType.SupportedPersonReqInd);

        return true;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// A value of NcpCurrentlySentToCra.
    /// </summary>
    [JsonPropertyName("ncpCurrentlySentToCra")]
    public Common NcpCurrentlySentToCra
    {
      get => ncpCurrentlySentToCra ??= new();
      set => ncpCurrentlySentToCra = value;
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
    private CreditReportingAction creditReportingAction;
    private ProgramProcessingInfo programProcessingInfo;
    private Common excludeFvi;
    private Common ncpCurrentlySentToCra;
    private DateWorkArea obligationCreatedCutoff;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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
    /// A value of ExcludedReason.
    /// </summary>
    [JsonPropertyName("excludedReason")]
    public WorkArea ExcludedReason
    {
      get => excludedReason ??= new();
      set => excludedReason = value;
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

    /// <summary>
    /// A value of Issuance.
    /// </summary>
    [JsonPropertyName("issuance")]
    public Case1 Issuance
    {
      get => issuance ??= new();
      set => issuance = value;
    }

    private Common excludeNcpFromCredRpt;
    private WorkArea excludedReason;
    private CreditReportingAction creditReportingAction;
    private Case1 issuance;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A AllObligForApGroup group.</summary>
    [Serializable]
    public class AllObligForApGroup
    {
      /// <summary>
      /// A value of AllObligForAp1.
      /// </summary>
      [JsonPropertyName("allObligForAp1")]
      public Obligation AllObligForAp1
      {
        get => allObligForAp1 ??= new();
        set => allObligForAp1 = value;
      }

      /// <summary>
      /// A value of AllObligForApExmpt.
      /// </summary>
      [JsonPropertyName("allObligForApExmpt")]
      public Common AllObligForApExmpt
      {
        get => allObligForApExmpt ??= new();
        set => allObligForApExmpt = value;
      }

      /// <summary>
      /// A value of AllObligForApGc.
      /// </summary>
      [JsonPropertyName("allObligForApGc")]
      public Common AllObligForApGc
      {
        get => allObligForApGc ??= new();
        set => allObligForApGc = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Obligation allObligForAp1;
      private Common allObligForApExmpt;
      private Common allObligForApGc;
    }

    /// <summary>A DebtTotByCaseGroup group.</summary>
    [Serializable]
    public class DebtTotByCaseGroup
    {
      /// <summary>
      /// A value of DebtTotByCaseLegalAction.
      /// </summary>
      [JsonPropertyName("debtTotByCaseLegalAction")]
      public LegalAction DebtTotByCaseLegalAction
      {
        get => debtTotByCaseLegalAction ??= new();
        set => debtTotByCaseLegalAction = value;
      }

      /// <summary>
      /// A value of DebtTotByCaseCase.
      /// </summary>
      [JsonPropertyName("debtTotByCaseCase")]
      public Case1 DebtTotByCaseCase
      {
        get => debtTotByCaseCase ??= new();
        set => debtTotByCaseCase = value;
      }

      /// <summary>
      /// A value of DebtTotByCaseDebtDetail.
      /// </summary>
      [JsonPropertyName("debtTotByCaseDebtDetail")]
      public DebtDetail DebtTotByCaseDebtDetail
      {
        get => debtTotByCaseDebtDetail ??= new();
        set => debtTotByCaseDebtDetail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 300;

      private LegalAction debtTotByCaseLegalAction;
      private Case1 debtTotByCaseCase;
      private DebtDetail debtTotByCaseDebtDetail;
    }

    /// <summary>
    /// A value of MostRecentDebtAdjustment.
    /// </summary>
    [JsonPropertyName("mostRecentDebtAdjustment")]
    public ObligationTransaction MostRecentDebtAdjustment
    {
      get => mostRecentDebtAdjustment ??= new();
      set => mostRecentDebtAdjustment = value;
    }

    /// <summary>
    /// A value of MostRecentCollection.
    /// </summary>
    [JsonPropertyName("mostRecentCollection")]
    public Collection MostRecentCollection
    {
      get => mostRecentCollection ??= new();
      set => mostRecentCollection = value;
    }

    /// <summary>
    /// A value of ActiveApCaseRoleFound.
    /// </summary>
    [JsonPropertyName("activeApCaseRoleFound")]
    public Common ActiveApCaseRoleFound
    {
      get => activeApCaseRoleFound ??= new();
      set => activeApCaseRoleFound = value;
    }

    /// <summary>
    /// A value of TotalNcpDebtOwed.
    /// </summary>
    [JsonPropertyName("totalNcpDebtOwed")]
    public Common TotalNcpDebtOwed
    {
      get => totalNcpDebtOwed ??= new();
      set => totalNcpDebtOwed = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of TotalCredCertifyAmount.
    /// </summary>
    [JsonPropertyName("totalCredCertifyAmount")]
    public Common TotalCredCertifyAmount
    {
      get => totalCredCertifyAmount ??= new();
      set => totalCredCertifyAmount = value;
    }

    /// <summary>
    /// A value of TotalDebtMoreThan4Mth.
    /// </summary>
    [JsonPropertyName("totalDebtMoreThan4Mth")]
    public DebtDetail TotalDebtMoreThan4Mth
    {
      get => totalDebtMoreThan4Mth ??= new();
      set => totalDebtMoreThan4Mth = value;
    }

    /// <summary>
    /// A value of TotalDebtLessThan4Mth.
    /// </summary>
    [JsonPropertyName("totalDebtLessThan4Mth")]
    public DebtDetail TotalDebtLessThan4Mth
    {
      get => totalDebtLessThan4Mth ??= new();
      set => totalDebtLessThan4Mth = value;
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
    /// Gets a value of AllObligForAp.
    /// </summary>
    [JsonIgnore]
    public Array<AllObligForApGroup> AllObligForAp => allObligForAp ??= new(
      AllObligForApGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of AllObligForAp for json serialization.
    /// </summary>
    [JsonPropertyName("allObligForAp")]
    [Computed]
    public IList<AllObligForApGroup> AllObligForAp_Json
    {
      get => allObligForAp;
      set => AllObligForAp.Assign(value);
    }

    /// <summary>
    /// Gets a value of DebtTotByCase.
    /// </summary>
    [JsonIgnore]
    public Array<DebtTotByCaseGroup> DebtTotByCase => debtTotByCase ??= new(
      DebtTotByCaseGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of DebtTotByCase for json serialization.
    /// </summary>
    [JsonPropertyName("debtTotByCase")]
    [Computed]
    public IList<DebtTotByCaseGroup> DebtTotByCase_Json
    {
      get => debtTotByCase;
      set => DebtTotByCase.Assign(value);
    }

    /// <summary>
    /// A value of GoodCauseFoundForAp.
    /// </summary>
    [JsonPropertyName("goodCauseFoundForAp")]
    public Common GoodCauseFoundForAp
    {
      get => goodCauseFoundForAp ??= new();
      set => goodCauseFoundForAp = value;
    }

    /// <summary>
    /// A value of ObligTypeCertifiable.
    /// </summary>
    [JsonPropertyName("obligTypeCertifiable")]
    public Common ObligTypeCertifiable
    {
      get => obligTypeCertifiable ??= new();
      set => obligTypeCertifiable = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public Obligation Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of FirstDayOfMonth.
    /// </summary>
    [JsonPropertyName("firstDayOfMonth")]
    public DateWorkArea FirstDayOfMonth
    {
      get => firstDayOfMonth ??= new();
      set => firstDayOfMonth = value;
    }

    /// <summary>
    /// A value of GoodCauseFoundForDebt.
    /// </summary>
    [JsonPropertyName("goodCauseFoundForDebt")]
    public Common GoodCauseFoundForDebt
    {
      get => goodCauseFoundForDebt ??= new();
      set => goodCauseFoundForDebt = value;
    }

    /// <summary>
    /// A value of HighAmtForCaseCase.
    /// </summary>
    [JsonPropertyName("highAmtForCaseCase")]
    public Case1 HighAmtForCaseCase
    {
      get => highAmtForCaseCase ??= new();
      set => highAmtForCaseCase = value;
    }

    /// <summary>
    /// A value of HighAmtForCaseDebtDetail.
    /// </summary>
    [JsonPropertyName("highAmtForCaseDebtDetail")]
    public DebtDetail HighAmtForCaseDebtDetail
    {
      get => highAmtForCaseDebtDetail ??= new();
      set => highAmtForCaseDebtDetail = value;
    }

    private ObligationTransaction mostRecentDebtAdjustment;
    private Collection mostRecentCollection;
    private Common activeApCaseRoleFound;
    private Common totalNcpDebtOwed;
    private DateWorkArea null1;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common totalCredCertifyAmount;
    private DebtDetail totalDebtMoreThan4Mth;
    private DebtDetail totalDebtLessThan4Mth;
    private AdministrativeActCertification administrativeActCertification;
    private Array<AllObligForApGroup> allObligForAp;
    private Array<DebtTotByCaseGroup> debtTotByCase;
    private Common goodCauseFoundForAp;
    private Common obligTypeCertifiable;
    private Obligation previous;
    private DateWorkArea firstDayOfMonth;
    private Common goodCauseFoundForDebt;
    private Case1 highAmtForCaseCase;
    private DebtDetail highAmtForCaseDebtDetail;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of DebtAdjustment.
    /// </summary>
    [JsonPropertyName("debtAdjustment")]
    public ObligationTransaction DebtAdjustment
    {
      get => debtAdjustment ??= new();
      set => debtAdjustment = value;
    }

    /// <summary>
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
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
    /// A value of Bankruptcy.
    /// </summary>
    [JsonPropertyName("bankruptcy")]
    public Bankruptcy Bankruptcy
    {
      get => bankruptcy ??= new();
      set => bankruptcy = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CaseRole Ap
    {
      get => ap ??= new();
      set => ap = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
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
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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

    private ObligationTransaction debtAdjustment;
    private Collection collection;
    private CsePerson csePerson;
    private Bankruptcy bankruptcy;
    private Case1 case1;
    private LegalAction legalAction;
    private LegalActionCaseRole legalActionCaseRole;
    private CaseRole caseRole;
    private GoodCause goodCause;
    private CaseRole ap;
    private ObligationType obligationType;
    private Obligation obligation;
    private ObligationAdmActionExemption obligationAdmActionExemption;
    private AdministrativeAction administrativeAction;
    private DebtDetail debtDetail;
    private ObligationTransaction debt;
    private CsePersonAccount obligor;
    private CsePersonAddress csePersonAddress;
  }
#endregion
}
