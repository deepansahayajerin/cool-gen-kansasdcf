// Program: LE_AUTOMATIC_IWO_FOR_UI, ID: 945091912, model: 746.
// Short name: SWE03070
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_AUTOMATIC_IWO_FOR_UI.
/// </summary>
[Serializable]
public partial class LeAutomaticIwoForUi: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_AUTOMATIC_IWO_FOR_UI program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeAutomaticIwoForUi(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeAutomaticIwoForUi.
  /// </summary>
  public LeAutomaticIwoForUi(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------------------------------------------------------------------
    // Date      Developer	Request #	Description
    // --------  ------------	---------	
    // -----------------------------------------------------------------
    // 05/15/12  RMathews	CQ33628		Created revised version of 
    // LE_AUTOMATIC_IWO_GENERATION to be used for
    //                         		certifying obligors for KDOL UI match 
    // processing. The IWO logic is used to
    // 					identify AP's and court cases that have current & arrears amounts 
    // that we
    // 					want to be matched to the KDOL UI payments for potential 
    // withholding.
    // 					Income_Source is not used by this CAB and MWO's are not processed.
    // 07/03/13  LSS		CQ 39236	Change the process for when an auto IWO is 
    // generated.
    // 					Change the READ for i-class legal actions to 3 separate READs
    // 					1) READ for IWO and IWO modification for most recently filed.
    // 					2) READ for Termination with a created date greater than the
    // 					   filed date of IWO or IWO modification - if so send alert
    // 					   - if not and filed date is not a null date create an ORDIWO2
    // 					3) READ for NOIIWON when no IWOs or Termination - if
    // 					   have one send alert - if no NOI create one.
    // 04/17/15  DQD		CQ47223		Disable the code that would set an alert for when
    // there
    // 					was not a legal service provider tied to the legal action.
    // 					Also it will disable the code to stop sending an alert
    // 					when the WA greater than the current arrears balance.
    // 					Both of these will now go out.
    // 02/24/17  AHockman	CQ47796		Changes made to look for WA and WC = zero on 
    // most recent
    // 					IWOMODO and skip/ignore that legal action detail.
    // 03/11/21  GVandy	CQ69406		Exclude Spousal and Spousal Arrears Judgements
    // 					when looking for debts due.
    // ---------------------------------------------------------------------------------------------------------
    local.Today.Date = Now().Date;
    export.ErrorMessages.Index = -1;
    export.Extract.Index = -1;

    if (!ReadCsePerson1())
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    // *************************************************************************************
    // Do not create an IWO if the AP is in Bankruptcy.
    // *************************************************************************************
    // *************************************************************************************
    // mFan WR296917 - Part C 9/26/2007  Commented out(Disabled) following 
    // statements.
    // *************************************************************************************
    // *************************************************************************************
    // For each court case on which the person is an AP, check to see if an IWO 
    // should be generated.
    // *************************************************************************************
    foreach(var item in ReadLegalActionTribunal())
    {
      if (local.PreviousDistinctTribunal.Identifier == entities
        .Tribunal.Identifier && Equal
        (local.PreviousDistinctLegalAction.CourtCaseNumber,
        entities.Distinct.CourtCaseNumber))
      {
        continue;
      }
      else
      {
        local.PreviousDistinctLegalAction.CourtCaseNumber =
          entities.Distinct.CourtCaseNumber;
        local.PreviousDistinctTribunal.Identifier =
          entities.Tribunal.Identifier;
      }

      local.StopProcessingThisCase.Flag = "N";

      // ********************************************************************************
      // Separate read to establish currancy on legal action.  Standard number 
      // needed
      // for error report, but don't want to alter the main cursor logic to add 
      // it there.
      // *********************************************************************************
      if (!ReadLegalAction1())
      {
        // --- If not found, standard number not available for error report
      }

      // *************************************************************************************
      // Skip this court case if an associated AR has claimed good cause.
      // *************************************************************************************
      foreach(var item1 in ReadCase1())
      {
        local.Case1.Number = entities.Case1.Number;

        foreach(var item2 in ReadGoodCause())
        {
          if (Equal(entities.GoodCause.Code, "GC"))
          {
            ++export.ErrorMessages.Index;
            export.ErrorMessages.CheckSize();

            export.ErrorMessages.Update.ErrorType.Text50 =
              "Court case bypassed for good cause";
            export.ErrorMessages.Update.ErrorCourtCase.StandardNumber =
              entities.ErrorMessage.StandardNumber;

            goto ReadEach;
          }
          else
          {
            break;
          }
        }

        // *************************************************************************************
        // Skip this court case if the associated CSE case is an outgoing 
        // interstate case.
        // *************************************************************************************
        foreach(var item2 in ReadInterstateRequest())
        {
          // -- Count the 'LO1' and 'CSI' types
          ReadInterstateRequestHistory1();

          // -- Count the non 'LO1' and 'CSI' types
          ReadInterstateRequestHistory2();

          if (local.NonCsiLo1Total.Count == 0 && local.CsiLo1Total.Count > 0)
          {
            continue;
          }

          if (AsChar(entities.InterstateRequest.KsCaseInd) == 'Y')
          {
            // --- This is an outgoing interstate case.
            ++export.ErrorMessages.Index;
            export.ErrorMessages.CheckSize();

            export.ErrorMessages.Update.ErrorType.Text50 =
              "Court case is outgoing interstate";
            export.ErrorMessages.Update.ErrorCourtCase.StandardNumber =
              entities.ErrorMessage.StandardNumber;

            goto ReadEach;
          }
        }
      }

      // *************************************************************************************
      // Skip this court case if it is not for a Kansas tribunal.
      // *************************************************************************************
      if (ReadFips())
      {
        if (!Equal(entities.Fips.StateAbbreviation, "KS"))
        {
          // *******************
          // Add code to check for interstate caseworker user id assigned to 
          // case
          // and allow item to process
          // *******************
          if (ReadCaseAssignment())
          {
            goto Test1;
          }

          ++export.ErrorMessages.Index;
          export.ErrorMessages.CheckSize();

          export.ErrorMessages.Update.ErrorType.Text50 =
            "Court case not issued by a Kansas tribunal";
          export.ErrorMessages.Update.ErrorCourtCase.StandardNumber =
            entities.ErrorMessage.StandardNumber;

          continue;
        }

Test1:

        if (entities.Fips.Location >= 20 && entities.Fips.Location <= 99)
        {
          // -- Also skip Tribal tribunals in the state of Kansas.
          if (ReadCaseAssignment())
          {
            goto Read;
          }

          ++export.ErrorMessages.Index;
          export.ErrorMessages.CheckSize();

          export.ErrorMessages.Update.ErrorType.Text50 =
            "Court case issued by a Tribal tribunal";
          export.ErrorMessages.Update.ErrorCourtCase.StandardNumber =
            entities.ErrorMessage.StandardNumber;

          continue;
        }
      }
      else
      {
        continue;
      }

Read:

      local.ContinueIwoProcessing.Flag = "N";
      local.ContinueMwoProcessing.Flag = "N";

      // *************************************************************************************
      // Skip this court case if there is no J or O class legal action with a 
      // filed date.
      // *************************************************************************************
      foreach(var item1 in ReadLegalAction2())
      {
        if (AsChar(entities.LegalAction.Classification) == 'J')
        {
          local.ContinueIwoProcessing.Flag = "Y";
          local.Jclass.Assign(entities.LegalAction);

          break;
        }
      }

      local.ToBeCreated.Command = "";

      if (AsChar(local.ContinueIwoProcessing.Flag) == 'Y' || AsChar
        (local.ContinueMwoProcessing.Flag) == 'Y')
      {
        // -- Check for a legal service provider assigned to the most recent non
        // "N" and non "U" legal action for this court case.
        foreach(var item1 in ReadLegalAction6())
        {
          if (AsChar(entities.PreviousAssignment.Classification) == 'N' || AsChar
            (entities.PreviousAssignment.Classification) == 'U')
          {
            // -- Ignore assignments to "N" and "U" class legal actions.
            continue;
          }

          // 06/01/01  GVandy  PR 120163 - Only check for assignment reason code
          // 'RSP' when determining if
          // the most recent legal action is assigned.  (The call to the create 
          // document cab fails if there
          // is no current RSP assignment.)
          ReadLegalActionAssigment();

          // cr47223 - the following code has been disabled as part of the work 
          // effort. The process will no longer stop if there is not a current
          // legal service provider tide to the legal action. Later on in the
          // dol process one will be assign to the legal action if there is not
          // one currently assigned but for now we do want it to fail becuase of
          // this check.
          break;
        }
      }

      // *********************************************************************
      // Start IWO Processing
      // *********************************************************************
      // We will escape from inside the IF once we determine what action to 
      // take.
      if (IsEmpty(local.ToBeCreated.Command) && AsChar
        (local.ContinueIwoProcessing.Flag) == 'Y' && AsChar
        (local.StopProcessingThisCase.Flag) == 'N')
      {
        // *************************************************************************************
        // Skip this court case if there is no existing debt.
        // *************************************************************************************
        local.ScreenObligationStatus.ObligationStatus = "";
        UseFnDisplayObligByCourtOrder();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (IsExitState("LEGAL_ACTION_NF"))
          {
            // -- The fn_display_oblig_by_court_order cab returns exit state 
            // legal_action_nf if no obligations exist for the court order.
            ExitState = "ACO_NN0000_ALL_OK";
          }
          else
          {
            goto Test2;
          }
        }

        for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
          local.Group.Index)
        {
          if (!Equal(local.Group.Item.CsePerson.Number, import.CsePerson.Number))
            
          {
            continue;
          }

          if (AsChar(local.Group.Item.Obligation.PrimarySecondaryCode) == 'S')
          {
            // -- Skip the court case number if a seconday obligation exists.
            goto Test2;
          }

          if (AsChar(local.Group.Item.ObligationType.Classification) != 'A' && AsChar
            (local.Group.Item.ObligationType.Classification) != 'M' && AsChar
            (local.Group.Item.ObligationType.Classification) != 'N')
          {
            // -- Skip the obligation if it is not Accruing ("A"), Medical ("M"
            // ), or Non-Accruing ("N").
            continue;
          }

          // 03/11/21 GVandy  CQ69406  Exclude Spousal and Spousal Arrears 
          // Judgements when looking for debts due.
          if (Equal(TrimEnd(local.Group.Item.ObligationType.Code), "SP") || Equal
            (TrimEnd(local.Group.Item.ObligationType.Code), "SAJ"))
          {
            // -- Skip the obligation if it is Spousal ("SP") or Spousal Arrears
            // Judgement ("SAJ").
            continue;
          }

          if (AsChar(local.Group.Item.ScreenObligationStatus.ObligationStatus) ==
            'A' || local.Group.Item.ScreenOwedAmounts.ArrearsAmountOwed > 0)
          {
            local.ScreenObligationStatus.ObligationStatus = "A";

            break;
          }
        }

        if (AsChar(local.ScreenObligationStatus.ObligationStatus) != 'A')
        {
          // -- Skip the court case number if no active obligations exist.
          ++export.ErrorMessages.Index;
          export.ErrorMessages.CheckSize();

          export.ErrorMessages.Update.ErrorType.Text50 =
            "Court case has no active obligation";
          export.ErrorMessages.Update.ErrorCourtCase.StandardNumber =
            entities.ErrorMessage.StandardNumber;

          goto Test2;
        }

        // -- Check if this is a mulitpayor court case.
        local.MultiPayor.Flag = "";

        foreach(var item1 in ReadCsePerson2())
        {
          if (!Equal(entities.MultipayorCsePerson.Number,
            import.CsePerson.Number))
          {
            local.MultiPayor.Flag = "Y";

            break;
          }
        }

        // *************************************************************************************
        // At this point we know that the court case meets the criteria for 
        // automatically generating
        // an IWO.  However, depending upon additional criteria, we will do one 
        // of two things:
        //   1. Generate an alert indicating that an obligor/court case was not 
        // able to be certified
        //      for UI match.
        //   2. Gather data to create a record for KDOL to process the obligor/
        // court case for UI match.
        // *************************************************************************************
        // Added new code for CQ39236 begins here **************
        local.CompareDateFiled.Assign(local.NullDateWorkArea);

        foreach(var item1 in ReadLegalAction3())
        {
          if (AsChar(local.MultiPayor.Flag) == 'Y')
          {
            // -- This is a multipayor case.  Check if the IWO was issued for 
            // our Obligor.  If not, then skip this I class legal action.
            if (!ReadLegalActionPerson1())
            {
              // -- Our person is not an obligor on the legal action.  Check if 
              // anyone else is an obligor for the legal action.
              if (ReadLegalActionPerson3())
              {
                // -- The IWO was for a different obligor.  Skip the legal 
                // action.
                continue;
              }
              else
              {
                // -- No obligor is assigned to the legal action.  Thus we can't
                // tell which payor the IWO was for.  Set an alert.
                local.ToBeCreated.Command = "ALERT";
                local.Infrastructure.Detail =
                  "The case is multi-payor and an Obligor is not defined on LOPS.";
                  

                goto Test2;
              }
            }
          }

          if (!Lt(local.Today.Date, entities.IclassLegalAction.EndDate))
          {
            // -- If the most recent I class action is end dated then send an 
            // alert.
            local.ToBeCreated.Command = "ALERT";
            local.Infrastructure.Detail =
              "Most recently created timestamp IWO or IWO Modification is end dated.";
              

            goto Test2;
          }

          // ** CQ47796 AHOCKMAN  IGNORE THIS IWOMODO IF THE MODO WA AND WC AMTS
          // ARE SET TO ZERO
          if (Equal(entities.IclassLegalAction.ActionTaken, "IWOMODO"))
          {
            if (ReadLegalActionDetail2())
            {
              if (!ReadLegalActionDetail3())
              {
                continue;
              }
            }
          }

          local.CompareDateFiled.Timestamp =
            entities.IclassLegalAction.CreatedTstamp;

          break;
        }

        foreach(var item1 in ReadLegalAction5())
        {
          if (AsChar(local.MultiPayor.Flag) == 'Y')
          {
            // -- This is a multipayor case.  Check if the IWO was issued for 
            // our Obligor.  If not, then skip this I class legal action.
            if (!ReadLegalActionPerson2())
            {
              // -- Our person is not an obligor on the legal action.  Check if 
              // anyone else is an obligor for the legal action.
              if (ReadLegalActionPerson4())
              {
                // -- The IWO was for a different obligor.  Skip the legal 
                // action.
                continue;
              }
              else
              {
                // -- No obligor is assigned to the legal action.  Thus we can't
                // tell which payor the IWO was for.  Set an alert.
                local.ToBeCreated.Command = "ALERT";
                local.Infrastructure.Detail =
                  "The case is multi-payor and an Obligor is not defined on LOPS.";
                  

                goto Test2;
              }
            }
          }

          local.ToBeCreated.Command = "ALERT";
          local.Infrastructure.Detail =
            "Termination created after create date of most recent IWO/IWO Modification.";
            

          goto Test2;
        }

        if (entities.IclassLegalAction.Populated)
        {
          local.ToBeCreated.Command = "ORDIWO2";

          goto Test2;
        }

        if (!entities.IclassLegalAction.Populated && !
          entities.IclassTerm.Populated)
        {
          foreach(var item1 in ReadLegalAction4())
          {
            if (AsChar(local.MultiPayor.Flag) == 'Y')
            {
              // -- This is a multipayor case.  Check if the IWO was issued for 
              // our Obligor.  If not, then skip this I class legal action.
              if (!ReadLegalActionPerson1())
              {
                // -- Our person is not an obligor on the legal action.  Check 
                // if anyone else is an obligor for the legal action.
                if (ReadLegalActionPerson3())
                {
                  // -- The IWO was for a different obligor.  Skip the legal 
                  // action.
                  continue;
                }
                else
                {
                  // -- No obligor is assigned to the legal action.  Thus we 
                  // can't tell which payor the IWO was for.  Set an alert.
                  local.ToBeCreated.Command = "ALERT";
                  local.Infrastructure.Detail =
                    "The case is multi-payor and an Obligor is not defined on LOPS.";
                    

                  goto Test2;
                }
              }
            }

            if (!Lt(local.Today.Date, entities.IclassLegalAction.EndDate))
            {
              // -- If the most recent I class action is end dated then send an 
              // alert.
              local.ToBeCreated.Command = "ALERT";
              local.Infrastructure.Detail =
                "The most recent I class legal action is end dated.";

              goto Test2;
            }

            local.ToBeCreated.Command = "ALERT";
            local.Infrastructure.Detail =
              "An NOIIWON legal action is already in process.";

            goto Test2;
          }

          // -- No I class actions existed.  Generate the NOI
          local.ToBeCreated.Command = "NOIIWON";

          // ----------------------------------------------------------------------
          // Skip this court case if it is only going to trigger an NOIIWON 
          // notice.
          // ----------------------------------------------------------------------
          continue;
        }

        // Added new code for CQ39236 ends here **************
        // CQ39236 disabled original code
      }

Test2:

      // CQ39236 END disabled
      if (Equal(local.ToBeCreated.Command, "ORDIWO2"))
      {
        // -- 6/28/01 GVandy WR 10509 An alert will be sent instead of creating 
        // the ORDIWO2 if
        //    the WA amount on the previous I class action is greater than the 
        // current arrears amount.
        local.LegalActionDetail.ArrearsAmount = 0;

        for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
          local.Group.Index)
        {
          if (!Equal(local.Group.Item.CsePerson.Number, import.CsePerson.Number))
            
          {
            continue;
          }

          if (AsChar(local.Group.Item.ObligationType.Classification) != 'M' && AsChar
            (local.Group.Item.ObligationType.Classification) != 'N' && AsChar
            (local.Group.Item.ObligationType.Classification) != 'A')
          {
            continue;
          }

          local.LegalActionDetail.ArrearsAmount =
            local.LegalActionDetail.ArrearsAmount.GetValueOrDefault() + local
            .Group.Item.ScreenOwedAmounts.ArrearsAmountOwed;
        }

        if (ReadLegalActionDetail1())
        {
          // cr47223 - the other part of the work effort is to disable the check
          // on WA amount to look at the current arrears balance. These are no
          // to go out.
        }
      }

      if (AsChar(local.ContinueIwoProcessing.Flag) == 'Y')
      {
        switch(TrimEnd(local.ToBeCreated.Command))
        {
          case "ALERT":
            // *************************************************************************************
            // Generate an alert to the legal service provider (or the 
            // caseworker if no legal service
            // provider has been assigned).
            // *************************************************************************************
            if (local.LegalActionAssignment.Count == 0)
            {
              // -- Send to the alert to the caseworker(s).
              local.Infrastructure.ReasonCode = "NOAUTOIWOCASE";
            }
            else
            {
              // -- Send the alert to the legal service provider.
              local.Infrastructure.ReasonCode = "NOAUTOIWO";
            }

            local.Infrastructure.EventId = 50;
            local.Infrastructure.CsenetInOutCode = "";
            local.Infrastructure.ProcessStatus = "Q";
            local.Infrastructure.UserId = "INCS";
            local.Infrastructure.BusinessObjectCd = "LEA";
            local.Infrastructure.ReferenceDate = local.Today.Date;
            local.Infrastructure.CreatedBy = global.UserId;
            local.Infrastructure.CreatedTimestamp = Now();
            local.Infrastructure.DenormNumeric12 =
              entities.PreviousAssignment.Identifier;
            local.Infrastructure.DenormText12 =
              entities.PreviousAssignment.CourtCaseNumber;

            if (!IsEmpty(entities.PreviousAssignment.InitiatingState) && !
              Equal(entities.PreviousAssignment.InitiatingState, "KS"))
            {
              local.Infrastructure.InitiatingStateCode = "OS";
            }
            else
            {
              local.Infrastructure.InitiatingStateCode = "KS";
            }

            local.Infrastructure.CsePersonNumber = import.CsePerson.Number;

            foreach(var item1 in ReadCase2())
            {
              local.Infrastructure.CaseNumber = entities.Case1.Number;
              UseSpCabCreateInfrastructure();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }
            }

            if (AsChar(local.StopProcessingThisCase.Flag) == 'N')
            {
              local.ToBeCreated.Command = "";
            }

            break;
          case "NOIIWON":
            // --------------------------------------------------------------------------------
            // No processing required for NOIIWON action in this CAB
            // --------------------------------------------------------------------------------
            break;
          case "ORDIWO2":
            // *************************************************************************************
            // Gather the WA & WC amounts for obligors/court cases that have 
            // been certified and
            // populate group view to return the results.
            // *************************************************************************************
            // *************************************************************************************
            // Copy the Withholding Current (WC) Legal Action Detail, 
            // Withholding Arrears (WA) Legal
            // Action Detail, and associated LOPS information from the most 
            // current IWO, IWOMODO,
            // IWONOTKS or IWONOTKM legal action.
            // *************************************************************************************
            local.LegalActionDetail.CurrentAmount = 0;
            local.LegalActionDetail.ArrearsAmount = 0;

            foreach(var item1 in ReadLegalActionDetailObligationType())
            {
              // -- Convert to a weekly amount in order to match UI payment 
              // schedule.
              switch(TrimEnd(entities.IclassLegalActionDetail.FreqPeriodCode))
              {
                case "":
                  break;
                case "BW":
                  // -- Bi-weekly (every other week)
                  local.LegalActionDetail.CurrentAmount =
                    Math.Round(
                      local.LegalActionDetail.CurrentAmount.
                      GetValueOrDefault() +
                    entities.IclassLegalActionDetail.CurrentAmount.
                      GetValueOrDefault() * 26 / 52
                    , 2, MidpointRounding.AwayFromZero);
                  local.LegalActionDetail.ArrearsAmount =
                    Math.Round(
                      local.LegalActionDetail.ArrearsAmount.
                      GetValueOrDefault() +
                    entities.IclassLegalActionDetail.ArrearsAmount.
                      GetValueOrDefault() * 26 / 52
                    , 2, MidpointRounding.AwayFromZero);

                  break;
                case "M":
                  // -- Monthly
                  local.LegalActionDetail.CurrentAmount =
                    Math.Round(
                      local.LegalActionDetail.CurrentAmount.
                      GetValueOrDefault() +
                    entities.IclassLegalActionDetail.CurrentAmount.
                      GetValueOrDefault() * 12 / 52
                    , 2, MidpointRounding.AwayFromZero);
                  local.LegalActionDetail.ArrearsAmount =
                    Math.Round(
                      local.LegalActionDetail.ArrearsAmount.
                      GetValueOrDefault() +
                    entities.IclassLegalActionDetail.ArrearsAmount.
                      GetValueOrDefault() * 12 / 52
                    , 2, MidpointRounding.AwayFromZero);

                  break;
                case "SM":
                  // -- Semi-Monthly (twice per month)
                  local.LegalActionDetail.CurrentAmount =
                    Math.Round(
                      local.LegalActionDetail.CurrentAmount.
                      GetValueOrDefault() +
                    entities.IclassLegalActionDetail.CurrentAmount.
                      GetValueOrDefault() * 24 / 52
                    , 2, MidpointRounding.AwayFromZero);
                  local.LegalActionDetail.ArrearsAmount =
                    Math.Round(
                      local.LegalActionDetail.ArrearsAmount.
                      GetValueOrDefault() +
                    entities.IclassLegalActionDetail.ArrearsAmount.
                      GetValueOrDefault() * 24 / 52
                    , 2, MidpointRounding.AwayFromZero);

                  break;
                case "W":
                  // -- Weekly
                  local.LegalActionDetail.CurrentAmount =
                    local.LegalActionDetail.CurrentAmount.GetValueOrDefault() +
                    entities
                    .IclassLegalActionDetail.CurrentAmount.GetValueOrDefault();
                  local.LegalActionDetail.ArrearsAmount =
                    local.LegalActionDetail.ArrearsAmount.GetValueOrDefault() +
                    entities
                    .IclassLegalActionDetail.ArrearsAmount.GetValueOrDefault();

                  break;
                default:
                  break;
              }
            }

            // --------------------------------------------------------------------------------
            // Populate group view for client and court case information
            // --------------------------------------------------------------------------------
            ++export.Extract.Index;
            export.Extract.CheckSize();

            export.Extract.Update.Client.FirstName =
              import.CsePersonsWorkSet.FirstName;
            export.Extract.Update.Client.LastName =
              import.CsePersonsWorkSet.LastName;
            export.Extract.Update.Client.MiddleInitial = "";
            export.Extract.Update.Client.Number =
              import.CsePersonsWorkSet.Number;
            export.Extract.Update.Client.Ssn = import.CsePersonsWorkSet.Ssn;
            export.Extract.Update.CourtCase.StandardNumber =
              entities.LegalAction.StandardNumber;
            export.Extract.Update.Amounts.ArrearsAmountOwed =
              local.LegalActionDetail.ArrearsAmount.GetValueOrDefault();
            export.Extract.Update.Amounts.CurrentAmountOwed =
              local.LegalActionDetail.CurrentAmount.GetValueOrDefault();
            export.Extract.Update.MaxPct.MaxWithholdingPercent = 0;
            local.ToBeCreated.Command = "";

            break;
          default:
            break;
        }
      }

ReadEach:
      ;
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveGroup(FnDisplayObligByCourtOrder.Export.
    GroupGroup source, Local.GroupGroup target)
  {
    target.DetailConcatInd.Text8 = source.DetailConcatInds.Text8;
    target.LegalAction.Identifier = source.LegalAction.Identifier;
    target.Common.SelectChar = source.Common.SelectChar;
    target.CsePerson.Number = source.CsePerson.Number;
    MoveCsePersonsWorkSet(source.CsePersonsWorkSet, target.CsePersonsWorkSet);
    target.ObligationType.Assign(source.ObligationType);
    target.Obligation.Assign(source.Obligation);
    target.ScreenObligationStatus.ObligationStatus =
      source.ScreenObligationStatus.ObligationStatus;
    target.ObligationPaymentSchedule.FrequencyCode =
      source.ObligationPaymentSchedule.FrequencyCode;
    target.ServiceProvider.UserId = source.ServiceProvider.UserId;
    target.ScreenObMutliSvcPrvdr.MultiServiceProviderInd =
      source.ScreenObMutliSvcPrvdr.MultiServiceProviderInd;
    target.ScreenOwedAmounts.Assign(source.ScreenOwedAmounts);
    target.HiddenAmtOwed.Flag = source.HiddenAmtOwedUnavl.Flag;
    target.ScreenDueAmounts.TotalAmountDue =
      source.ScreenDueAmounts.TotalAmountDue;
  }

  private void UseFnDisplayObligByCourtOrder()
  {
    var useImport = new FnDisplayObligByCourtOrder.Import();
    var useExport = new FnDisplayObligByCourtOrder.Export();

    useImport.Search.Assign(local.Jclass);

    Call(FnDisplayObligByCourtOrder.Execute, useImport, useExport);

    useExport.Group.CopyTo(local.Group, MoveGroup);
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    local.Infrastructure.Assign(useExport.Infrastructure);
  }

  private IEnumerable<bool> ReadCase1()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Today.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableString(
          command, "courtCaseNo", entities.Distinct.CourtCaseNumber ?? "");
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCase2()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Today.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetInt32(command, "lgaId", entities.PreviousAssignment.Identifier);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private bool ReadCaseAssignment()
  {
    entities.CaseAssignment.Populated = false;

    return Read("ReadCaseAssignment",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Today.Date.GetValueOrDefault());
        db.SetString(command, "ospCode", import.OfficeServiceProvider.RoleCode);
        db.SetInt32(command, "offId", import.Office.SystemGeneratedId);
        db.SetInt32(command, "spdId", import.ServiceProvider.SystemGeneratedId);
        db.SetNullableString(
          command, "courtCaseNo", entities.Distinct.CourtCaseNumber ?? "");
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 0);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 1);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 3);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 4);
        entities.CaseAssignment.OspCode = db.GetString(reader, 5);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 6);
        entities.CaseAssignment.CasNo = db.GetString(reader, 7);
        entities.CaseAssignment.Populated = true;
      });
  }

  private bool ReadCsePerson1()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCsePerson2()
  {
    entities.MultipayorCsePerson.Populated = false;

    return ReadEach("ReadCsePerson2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", entities.Distinct.CourtCaseNumber ?? "");
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.MultipayorCsePerson.Number = db.GetString(reader, 0);
        entities.MultipayorCsePerson.Populated = true;

        return true;
      });
  }

  private bool ReadFips()
  {
    System.Diagnostics.Debug.Assert(entities.Tribunal.Populated);
    entities.Fips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.SetInt32(
          command, "location",
          entities.Tribunal.FipLocation.GetValueOrDefault());
        db.SetInt32(
          command, "county", entities.Tribunal.FipCounty.GetValueOrDefault());
        db.SetInt32(
          command, "state", entities.Tribunal.FipState.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.StateAbbreviation = db.GetString(reader, 3);
        entities.Fips.Populated = true;
      });
  }

  private IEnumerable<bool> ReadGoodCause()
  {
    entities.GoodCause.Populated = false;

    return ReadEach("ReadGoodCause",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "effectiveDate", local.Today.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableString(command, "cspNumber1", entities.CsePerson.Number);
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

        return true;
      });
  }

  private IEnumerable<bool> ReadInterstateRequest()
  {
    entities.InterstateRequest.Populated = false;

    return ReadEach("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 1);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 2);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 3);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 4);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 5);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 6);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 7);
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);

        return true;
      });
  }

  private bool ReadInterstateRequestHistory1()
  {
    return Read("ReadInterstateRequestHistory1",
      (db, command) =>
      {
        db.SetInt32(
          command, "intGeneratedId",
          entities.InterstateRequest.IntHGeneratedId);
      },
      (db, reader) =>
      {
        local.CsiLo1Total.Count = db.GetInt32(reader, 0);
      });
  }

  private bool ReadInterstateRequestHistory2()
  {
    return Read("ReadInterstateRequestHistory2",
      (db, command) =>
      {
        db.SetInt32(
          command, "intGeneratedId",
          entities.InterstateRequest.IntHGeneratedId);
      },
      (db, reader) =>
      {
        local.NonCsiLo1Total.Count = db.GetInt32(reader, 0);
      });
  }

  private bool ReadLegalAction1()
  {
    entities.ErrorMessage.Populated = false;

    return Read("ReadLegalAction1",
      (db, command) =>
      {
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
        db.SetNullableString(
          command, "courtCaseNo", entities.Distinct.CourtCaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.ErrorMessage.Identifier = db.GetInt32(reader, 0);
        entities.ErrorMessage.CourtCaseNumber = db.GetNullableString(reader, 1);
        entities.ErrorMessage.StandardNumber = db.GetNullableString(reader, 2);
        entities.ErrorMessage.TrbId = db.GetNullableInt32(reader, 3);
        entities.ErrorMessage.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalAction2()
  {
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalAction2",
      (db, command) =>
      {
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
        db.SetNullableString(
          command, "courtCaseNo", entities.Distinct.CourtCaseNumber ?? "");
        db.SetNullableDate(
          command, "filedDt1", local.NullDateWorkArea.Date.GetValueOrDefault());
          
        db.SetNullableDate(
          command, "filedDt2", local.Today.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.Type1 = db.GetString(reader, 3);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 4);
        entities.LegalAction.InitiatingState = db.GetNullableString(reader, 5);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 6);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 7);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 8);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 9);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 10);
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalAction3()
  {
    entities.IclassLegalAction.Populated = false;

    return ReadEach("ReadLegalAction3",
      (db, command) =>
      {
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
        db.SetNullableString(
          command, "courtCaseNo", entities.Distinct.CourtCaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.IclassLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.IclassLegalAction.Classification = db.GetString(reader, 1);
        entities.IclassLegalAction.ActionTaken = db.GetString(reader, 2);
        entities.IclassLegalAction.Type1 = db.GetString(reader, 3);
        entities.IclassLegalAction.FiledDate = db.GetNullableDate(reader, 4);
        entities.IclassLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 5);
        entities.IclassLegalAction.EndDate = db.GetNullableDate(reader, 6);
        entities.IclassLegalAction.CreatedTstamp = db.GetDateTime(reader, 7);
        entities.IclassLegalAction.TrbId = db.GetNullableInt32(reader, 8);
        entities.IclassLegalAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalAction4()
  {
    entities.IclassLegalAction.Populated = false;

    return ReadEach("ReadLegalAction4",
      (db, command) =>
      {
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
        db.SetNullableString(
          command, "courtCaseNo", entities.Distinct.CourtCaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.IclassLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.IclassLegalAction.Classification = db.GetString(reader, 1);
        entities.IclassLegalAction.ActionTaken = db.GetString(reader, 2);
        entities.IclassLegalAction.Type1 = db.GetString(reader, 3);
        entities.IclassLegalAction.FiledDate = db.GetNullableDate(reader, 4);
        entities.IclassLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 5);
        entities.IclassLegalAction.EndDate = db.GetNullableDate(reader, 6);
        entities.IclassLegalAction.CreatedTstamp = db.GetDateTime(reader, 7);
        entities.IclassLegalAction.TrbId = db.GetNullableInt32(reader, 8);
        entities.IclassLegalAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalAction5()
  {
    entities.IclassTerm.Populated = false;

    return ReadEach("ReadLegalAction5",
      (db, command) =>
      {
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
        db.SetNullableString(
          command, "courtCaseNo", entities.Distinct.CourtCaseNumber ?? "");
        db.SetDateTime(
          command, "createdTstamp",
          local.CompareDateFiled.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.IclassTerm.Identifier = db.GetInt32(reader, 0);
        entities.IclassTerm.Classification = db.GetString(reader, 1);
        entities.IclassTerm.ActionTaken = db.GetString(reader, 2);
        entities.IclassTerm.Type1 = db.GetString(reader, 3);
        entities.IclassTerm.FiledDate = db.GetNullableDate(reader, 4);
        entities.IclassTerm.CourtCaseNumber = db.GetNullableString(reader, 5);
        entities.IclassTerm.EndDate = db.GetNullableDate(reader, 6);
        entities.IclassTerm.CreatedTstamp = db.GetDateTime(reader, 7);
        entities.IclassTerm.TrbId = db.GetNullableInt32(reader, 8);
        entities.IclassTerm.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalAction6()
  {
    entities.PreviousAssignment.Populated = false;

    return ReadEach("ReadLegalAction6",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", entities.Distinct.CourtCaseNumber ?? "");
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.PreviousAssignment.Identifier = db.GetInt32(reader, 0);
        entities.PreviousAssignment.Classification = db.GetString(reader, 1);
        entities.PreviousAssignment.InitiatingState =
          db.GetNullableString(reader, 2);
        entities.PreviousAssignment.CourtCaseNumber =
          db.GetNullableString(reader, 3);
        entities.PreviousAssignment.CreatedTstamp = db.GetDateTime(reader, 4);
        entities.PreviousAssignment.TrbId = db.GetNullableInt32(reader, 5);
        entities.PreviousAssignment.Populated = true;

        return true;
      });
  }

  private bool ReadLegalActionAssigment()
  {
    return Read("ReadLegalActionAssigment",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.PreviousAssignment.Identifier);
        db.SetNullableDate(
          command, "endDt", local.Today.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        local.LegalActionAssignment.Count = db.GetInt32(reader, 0);
      });
  }

  private bool ReadLegalActionDetail1()
  {
    entities.IclassLegalActionDetail.Populated = false;

    return Read("ReadLegalActionDetail1",
      (db, command) =>
      {
        db.SetInt32(
          command, "lgaIdentifier", entities.IclassLegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.IclassLegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.IclassLegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.IclassLegalActionDetail.EndDate =
          db.GetNullableDate(reader, 2);
        entities.IclassLegalActionDetail.EffectiveDate = db.GetDate(reader, 3);
        entities.IclassLegalActionDetail.BondAmount =
          db.GetNullableDecimal(reader, 4);
        entities.IclassLegalActionDetail.CreatedBy = db.GetString(reader, 5);
        entities.IclassLegalActionDetail.CreatedTstamp =
          db.GetDateTime(reader, 6);
        entities.IclassLegalActionDetail.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.IclassLegalActionDetail.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 8);
        entities.IclassLegalActionDetail.ArrearsAmount =
          db.GetNullableDecimal(reader, 9);
        entities.IclassLegalActionDetail.CurrentAmount =
          db.GetNullableDecimal(reader, 10);
        entities.IclassLegalActionDetail.JudgementAmount =
          db.GetNullableDecimal(reader, 11);
        entities.IclassLegalActionDetail.Limit =
          db.GetNullableInt32(reader, 12);
        entities.IclassLegalActionDetail.DetailType = db.GetString(reader, 13);
        entities.IclassLegalActionDetail.FreqPeriodCode =
          db.GetNullableString(reader, 14);
        entities.IclassLegalActionDetail.DayOfWeek =
          db.GetNullableInt32(reader, 15);
        entities.IclassLegalActionDetail.DayOfMonth1 =
          db.GetNullableInt32(reader, 16);
        entities.IclassLegalActionDetail.DayOfMonth2 =
          db.GetNullableInt32(reader, 17);
        entities.IclassLegalActionDetail.PeriodInd =
          db.GetNullableString(reader, 18);
        entities.IclassLegalActionDetail.Description = db.GetString(reader, 19);
        entities.IclassLegalActionDetail.OtyId =
          db.GetNullableInt32(reader, 20);
        entities.IclassLegalActionDetail.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.IclassLegalActionDetail.DetailType);
      });
  }

  private bool ReadLegalActionDetail2()
  {
    entities.IclassLegalActionDetail.Populated = false;

    return Read("ReadLegalActionDetail2",
      (db, command) =>
      {
        db.SetInt32(
          command, "lgaIdentifier", entities.IclassLegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.IclassLegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.IclassLegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.IclassLegalActionDetail.EndDate =
          db.GetNullableDate(reader, 2);
        entities.IclassLegalActionDetail.EffectiveDate = db.GetDate(reader, 3);
        entities.IclassLegalActionDetail.BondAmount =
          db.GetNullableDecimal(reader, 4);
        entities.IclassLegalActionDetail.CreatedBy = db.GetString(reader, 5);
        entities.IclassLegalActionDetail.CreatedTstamp =
          db.GetDateTime(reader, 6);
        entities.IclassLegalActionDetail.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.IclassLegalActionDetail.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 8);
        entities.IclassLegalActionDetail.ArrearsAmount =
          db.GetNullableDecimal(reader, 9);
        entities.IclassLegalActionDetail.CurrentAmount =
          db.GetNullableDecimal(reader, 10);
        entities.IclassLegalActionDetail.JudgementAmount =
          db.GetNullableDecimal(reader, 11);
        entities.IclassLegalActionDetail.Limit =
          db.GetNullableInt32(reader, 12);
        entities.IclassLegalActionDetail.DetailType = db.GetString(reader, 13);
        entities.IclassLegalActionDetail.FreqPeriodCode =
          db.GetNullableString(reader, 14);
        entities.IclassLegalActionDetail.DayOfWeek =
          db.GetNullableInt32(reader, 15);
        entities.IclassLegalActionDetail.DayOfMonth1 =
          db.GetNullableInt32(reader, 16);
        entities.IclassLegalActionDetail.DayOfMonth2 =
          db.GetNullableInt32(reader, 17);
        entities.IclassLegalActionDetail.PeriodInd =
          db.GetNullableString(reader, 18);
        entities.IclassLegalActionDetail.Description = db.GetString(reader, 19);
        entities.IclassLegalActionDetail.OtyId =
          db.GetNullableInt32(reader, 20);
        entities.IclassLegalActionDetail.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.IclassLegalActionDetail.DetailType);
      });
  }

  private bool ReadLegalActionDetail3()
  {
    entities.IclassLegalActionDetail.Populated = false;

    return Read("ReadLegalActionDetail3",
      (db, command) =>
      {
        db.SetInt32(
          command, "lgaIdentifier", entities.IclassLegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.IclassLegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.IclassLegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.IclassLegalActionDetail.EndDate =
          db.GetNullableDate(reader, 2);
        entities.IclassLegalActionDetail.EffectiveDate = db.GetDate(reader, 3);
        entities.IclassLegalActionDetail.BondAmount =
          db.GetNullableDecimal(reader, 4);
        entities.IclassLegalActionDetail.CreatedBy = db.GetString(reader, 5);
        entities.IclassLegalActionDetail.CreatedTstamp =
          db.GetDateTime(reader, 6);
        entities.IclassLegalActionDetail.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.IclassLegalActionDetail.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 8);
        entities.IclassLegalActionDetail.ArrearsAmount =
          db.GetNullableDecimal(reader, 9);
        entities.IclassLegalActionDetail.CurrentAmount =
          db.GetNullableDecimal(reader, 10);
        entities.IclassLegalActionDetail.JudgementAmount =
          db.GetNullableDecimal(reader, 11);
        entities.IclassLegalActionDetail.Limit =
          db.GetNullableInt32(reader, 12);
        entities.IclassLegalActionDetail.DetailType = db.GetString(reader, 13);
        entities.IclassLegalActionDetail.FreqPeriodCode =
          db.GetNullableString(reader, 14);
        entities.IclassLegalActionDetail.DayOfWeek =
          db.GetNullableInt32(reader, 15);
        entities.IclassLegalActionDetail.DayOfMonth1 =
          db.GetNullableInt32(reader, 16);
        entities.IclassLegalActionDetail.DayOfMonth2 =
          db.GetNullableInt32(reader, 17);
        entities.IclassLegalActionDetail.PeriodInd =
          db.GetNullableString(reader, 18);
        entities.IclassLegalActionDetail.Description = db.GetString(reader, 19);
        entities.IclassLegalActionDetail.OtyId =
          db.GetNullableInt32(reader, 20);
        entities.IclassLegalActionDetail.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.IclassLegalActionDetail.DetailType);
      });
  }

  private IEnumerable<bool> ReadLegalActionDetailObligationType()
  {
    entities.IclassLegalActionDetail.Populated = false;
    entities.ObligationType.Populated = false;

    return ReadEach("ReadLegalActionDetailObligationType",
      (db, command) =>
      {
        db.SetInt32(
          command, "lgaIdentifier", entities.IclassLegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.IclassLegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.IclassLegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.IclassLegalActionDetail.EndDate =
          db.GetNullableDate(reader, 2);
        entities.IclassLegalActionDetail.EffectiveDate = db.GetDate(reader, 3);
        entities.IclassLegalActionDetail.BondAmount =
          db.GetNullableDecimal(reader, 4);
        entities.IclassLegalActionDetail.CreatedBy = db.GetString(reader, 5);
        entities.IclassLegalActionDetail.CreatedTstamp =
          db.GetDateTime(reader, 6);
        entities.IclassLegalActionDetail.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.IclassLegalActionDetail.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 8);
        entities.IclassLegalActionDetail.ArrearsAmount =
          db.GetNullableDecimal(reader, 9);
        entities.IclassLegalActionDetail.CurrentAmount =
          db.GetNullableDecimal(reader, 10);
        entities.IclassLegalActionDetail.JudgementAmount =
          db.GetNullableDecimal(reader, 11);
        entities.IclassLegalActionDetail.Limit =
          db.GetNullableInt32(reader, 12);
        entities.IclassLegalActionDetail.DetailType = db.GetString(reader, 13);
        entities.IclassLegalActionDetail.FreqPeriodCode =
          db.GetNullableString(reader, 14);
        entities.IclassLegalActionDetail.DayOfWeek =
          db.GetNullableInt32(reader, 15);
        entities.IclassLegalActionDetail.DayOfMonth1 =
          db.GetNullableInt32(reader, 16);
        entities.IclassLegalActionDetail.DayOfMonth2 =
          db.GetNullableInt32(reader, 17);
        entities.IclassLegalActionDetail.PeriodInd =
          db.GetNullableString(reader, 18);
        entities.IclassLegalActionDetail.Description = db.GetString(reader, 19);
        entities.IclassLegalActionDetail.OtyId =
          db.GetNullableInt32(reader, 20);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 20);
        entities.ObligationType.Code = db.GetString(reader, 21);
        entities.ObligationType.Classification = db.GetString(reader, 22);
        entities.IclassLegalActionDetail.Populated = true;
        entities.ObligationType.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.IclassLegalActionDetail.DetailType);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);

        return true;
      });
  }

  private bool ReadLegalActionPerson1()
  {
    entities.MultipayorLegalActionPerson.Populated = false;

    return Read("ReadLegalActionPerson1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.IclassLegalAction.Identifier);
        db.SetNullableString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.MultipayorLegalActionPerson.Identifier =
          db.GetInt32(reader, 0);
        entities.MultipayorLegalActionPerson.CspNumber =
          db.GetNullableString(reader, 1);
        entities.MultipayorLegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.MultipayorLegalActionPerson.LadRNumber =
          db.GetNullableInt32(reader, 3);
        entities.MultipayorLegalActionPerson.AccountType =
          db.GetNullableString(reader, 4);
        entities.MultipayorLegalActionPerson.Populated = true;
      });
  }

  private bool ReadLegalActionPerson2()
  {
    entities.MultipayorLegalActionPerson.Populated = false;

    return Read("ReadLegalActionPerson2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.IclassTerm.Identifier);
        db.SetNullableString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.MultipayorLegalActionPerson.Identifier =
          db.GetInt32(reader, 0);
        entities.MultipayorLegalActionPerson.CspNumber =
          db.GetNullableString(reader, 1);
        entities.MultipayorLegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.MultipayorLegalActionPerson.LadRNumber =
          db.GetNullableInt32(reader, 3);
        entities.MultipayorLegalActionPerson.AccountType =
          db.GetNullableString(reader, 4);
        entities.MultipayorLegalActionPerson.Populated = true;
      });
  }

  private bool ReadLegalActionPerson3()
  {
    entities.MultipayorLegalActionPerson.Populated = false;

    return Read("ReadLegalActionPerson3",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.IclassLegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.MultipayorLegalActionPerson.Identifier =
          db.GetInt32(reader, 0);
        entities.MultipayorLegalActionPerson.CspNumber =
          db.GetNullableString(reader, 1);
        entities.MultipayorLegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.MultipayorLegalActionPerson.LadRNumber =
          db.GetNullableInt32(reader, 3);
        entities.MultipayorLegalActionPerson.AccountType =
          db.GetNullableString(reader, 4);
        entities.MultipayorLegalActionPerson.Populated = true;
      });
  }

  private bool ReadLegalActionPerson4()
  {
    entities.MultipayorLegalActionPerson.Populated = false;

    return Read("ReadLegalActionPerson4",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.IclassTerm.Identifier);
      },
      (db, reader) =>
      {
        entities.MultipayorLegalActionPerson.Identifier =
          db.GetInt32(reader, 0);
        entities.MultipayorLegalActionPerson.CspNumber =
          db.GetNullableString(reader, 1);
        entities.MultipayorLegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.MultipayorLegalActionPerson.LadRNumber =
          db.GetNullableInt32(reader, 3);
        entities.MultipayorLegalActionPerson.AccountType =
          db.GetNullableString(reader, 4);
        entities.MultipayorLegalActionPerson.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalActionTribunal()
  {
    entities.Distinct.Populated = false;
    entities.Tribunal.Populated = false;

    return ReadEach("ReadLegalActionTribunal",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "endDate", local.Today.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Distinct.Identifier = db.GetInt32(reader, 0);
        entities.Distinct.CourtCaseNumber = db.GetNullableString(reader, 1);
        entities.Distinct.TrbId = db.GetNullableInt32(reader, 2);
        entities.Tribunal.Identifier = db.GetInt32(reader, 2);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 3);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 4);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 5);
        entities.Distinct.Populated = true;
        entities.Tribunal.Populated = true;

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
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
    }

    private OfficeServiceProvider officeServiceProvider;
    private ServiceProvider serviceProvider;
    private Office office;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePerson csePerson;
    private IncomeSource incomeSource;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ErrorMessagesGroup group.</summary>
    [Serializable]
    public class ErrorMessagesGroup
    {
      /// <summary>
      /// A value of ErrorCourtCase.
      /// </summary>
      [JsonPropertyName("errorCourtCase")]
      public LegalAction ErrorCourtCase
      {
        get => errorCourtCase ??= new();
        set => errorCourtCase = value;
      }

      /// <summary>
      /// A value of ErrorType.
      /// </summary>
      [JsonPropertyName("errorType")]
      public WorkArea ErrorType
      {
        get => errorType ??= new();
        set => errorType = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private LegalAction errorCourtCase;
      private WorkArea errorType;
    }

    /// <summary>A ExtractGroup group.</summary>
    [Serializable]
    public class ExtractGroup
    {
      /// <summary>
      /// A value of MaxPct.
      /// </summary>
      [JsonPropertyName("maxPct")]
      public DolUiWithholding MaxPct
      {
        get => maxPct ??= new();
        set => maxPct = value;
      }

      /// <summary>
      /// A value of CourtCase.
      /// </summary>
      [JsonPropertyName("courtCase")]
      public LegalAction CourtCase
      {
        get => courtCase ??= new();
        set => courtCase = value;
      }

      /// <summary>
      /// A value of Amounts.
      /// </summary>
      [JsonPropertyName("amounts")]
      public ScreenOwedAmounts Amounts
      {
        get => amounts ??= new();
        set => amounts = value;
      }

      /// <summary>
      /// A value of Client.
      /// </summary>
      [JsonPropertyName("client")]
      public CsePersonsWorkSet Client
      {
        get => client ??= new();
        set => client = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private DolUiWithholding maxPct;
      private LegalAction courtCase;
      private ScreenOwedAmounts amounts;
      private CsePersonsWorkSet client;
    }

    /// <summary>
    /// Gets a value of ErrorMessages.
    /// </summary>
    [JsonIgnore]
    public Array<ErrorMessagesGroup> ErrorMessages => errorMessages ??= new(
      ErrorMessagesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ErrorMessages for json serialization.
    /// </summary>
    [JsonPropertyName("errorMessages")]
    [Computed]
    public IList<ErrorMessagesGroup> ErrorMessages_Json
    {
      get => errorMessages;
      set => ErrorMessages.Assign(value);
    }

    /// <summary>
    /// Gets a value of Extract.
    /// </summary>
    [JsonIgnore]
    public Array<ExtractGroup> Extract => extract ??= new(
      ExtractGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Extract for json serialization.
    /// </summary>
    [JsonPropertyName("extract")]
    [Computed]
    public IList<ExtractGroup> Extract_Json
    {
      get => extract;
      set => Extract.Assign(value);
    }

    private Array<ErrorMessagesGroup> errorMessages;
    private Array<ExtractGroup> extract;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of DetailConcatInd.
      /// </summary>
      [JsonPropertyName("detailConcatInd")]
      public TextWorkArea DetailConcatInd
      {
        get => detailConcatInd ??= new();
        set => detailConcatInd = value;
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
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
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
      /// A value of CsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("csePersonsWorkSet")]
      public CsePersonsWorkSet CsePersonsWorkSet
      {
        get => csePersonsWorkSet ??= new();
        set => csePersonsWorkSet = value;
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
      /// A value of ScreenObligationStatus.
      /// </summary>
      [JsonPropertyName("screenObligationStatus")]
      public ScreenObligationStatus ScreenObligationStatus
      {
        get => screenObligationStatus ??= new();
        set => screenObligationStatus = value;
      }

      /// <summary>
      /// A value of ObligationPaymentSchedule.
      /// </summary>
      [JsonPropertyName("obligationPaymentSchedule")]
      public ObligationPaymentSchedule ObligationPaymentSchedule
      {
        get => obligationPaymentSchedule ??= new();
        set => obligationPaymentSchedule = value;
      }

      /// <summary>
      /// A value of ServiceProvider.
      /// </summary>
      [JsonPropertyName("serviceProvider")]
      public ServiceProvider ServiceProvider
      {
        get => serviceProvider ??= new();
        set => serviceProvider = value;
      }

      /// <summary>
      /// A value of ScreenObMutliSvcPrvdr.
      /// </summary>
      [JsonPropertyName("screenObMutliSvcPrvdr")]
      public ScreenObMutliSvcPrvdr ScreenObMutliSvcPrvdr
      {
        get => screenObMutliSvcPrvdr ??= new();
        set => screenObMutliSvcPrvdr = value;
      }

      /// <summary>
      /// A value of ScreenOwedAmounts.
      /// </summary>
      [JsonPropertyName("screenOwedAmounts")]
      public ScreenOwedAmounts ScreenOwedAmounts
      {
        get => screenOwedAmounts ??= new();
        set => screenOwedAmounts = value;
      }

      /// <summary>
      /// A value of HiddenAmtOwed.
      /// </summary>
      [JsonPropertyName("hiddenAmtOwed")]
      public Common HiddenAmtOwed
      {
        get => hiddenAmtOwed ??= new();
        set => hiddenAmtOwed = value;
      }

      /// <summary>
      /// A value of ScreenDueAmounts.
      /// </summary>
      [JsonPropertyName("screenDueAmounts")]
      public ScreenDueAmounts ScreenDueAmounts
      {
        get => screenDueAmounts ??= new();
        set => screenDueAmounts = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 80;

      private TextWorkArea detailConcatInd;
      private LegalAction legalAction;
      private Common common;
      private CsePerson csePerson;
      private CsePersonsWorkSet csePersonsWorkSet;
      private ObligationType obligationType;
      private Obligation obligation;
      private ScreenObligationStatus screenObligationStatus;
      private ObligationPaymentSchedule obligationPaymentSchedule;
      private ServiceProvider serviceProvider;
      private ScreenObMutliSvcPrvdr screenObMutliSvcPrvdr;
      private ScreenOwedAmounts screenOwedAmounts;
      private Common hiddenAmtOwed;
      private ScreenDueAmounts screenDueAmounts;
    }

    /// <summary>
    /// A value of AutoIwoCheckCodeValue.
    /// </summary>
    [JsonPropertyName("autoIwoCheckCodeValue")]
    public CodeValue AutoIwoCheckCodeValue
    {
      get => autoIwoCheckCodeValue ??= new();
      set => autoIwoCheckCodeValue = value;
    }

    /// <summary>
    /// A value of AutoIwoCheckCode.
    /// </summary>
    [JsonPropertyName("autoIwoCheckCode")]
    public Code AutoIwoCheckCode
    {
      get => autoIwoCheckCode ??= new();
      set => autoIwoCheckCode = value;
    }

    /// <summary>
    /// A value of AutoMwoCheckCodeValue.
    /// </summary>
    [JsonPropertyName("autoMwoCheckCodeValue")]
    public CodeValue AutoMwoCheckCodeValue
    {
      get => autoMwoCheckCodeValue ??= new();
      set => autoMwoCheckCodeValue = value;
    }

    /// <summary>
    /// A value of AutoMwoCheckCode.
    /// </summary>
    [JsonPropertyName("autoMwoCheckCode")]
    public Code AutoMwoCheckCode
    {
      get => autoMwoCheckCode ??= new();
      set => autoMwoCheckCode = value;
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
    /// A value of ContinueIwoProcessing.
    /// </summary>
    [JsonPropertyName("continueIwoProcessing")]
    public Common ContinueIwoProcessing
    {
      get => continueIwoProcessing ??= new();
      set => continueIwoProcessing = value;
    }

    /// <summary>
    /// A value of ContinueMwoProcessing.
    /// </summary>
    [JsonPropertyName("continueMwoProcessing")]
    public Common ContinueMwoProcessing
    {
      get => continueMwoProcessing ??= new();
      set => continueMwoProcessing = value;
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
    /// A value of CsiLo1Total.
    /// </summary>
    [JsonPropertyName("csiLo1Total")]
    public Common CsiLo1Total
    {
      get => csiLo1Total ??= new();
      set => csiLo1Total = value;
    }

    /// <summary>
    /// A value of DetailObligated.
    /// </summary>
    [JsonPropertyName("detailObligated")]
    public Common DetailObligated
    {
      get => detailObligated ??= new();
      set => detailObligated = value;
    }

    /// <summary>
    /// A value of DifferentPeriods.
    /// </summary>
    [JsonPropertyName("differentPeriods")]
    public Common DifferentPeriods
    {
      get => differentPeriods ??= new();
      set => differentPeriods = value;
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
    /// A value of EmployerAddressNote.
    /// </summary>
    [JsonPropertyName("employerAddressNote")]
    public WorkArea EmployerAddressNote
    {
      get => employerAddressNote ??= new();
      set => employerAddressNote = value;
    }

    /// <summary>
    /// A value of FirstLegalAction.
    /// </summary>
    [JsonPropertyName("firstLegalAction")]
    public Common FirstLegalAction
    {
      get => firstLegalAction ??= new();
      set => firstLegalAction = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    /// <summary>
    /// A value of HcOtCAccruing.
    /// </summary>
    [JsonPropertyName("hcOtCAccruing")]
    public ObligationType HcOtCAccruing
    {
      get => hcOtCAccruing ??= new();
      set => hcOtCAccruing = value;
    }

    /// <summary>
    /// A value of HcOtCVoluntary.
    /// </summary>
    [JsonPropertyName("hcOtCVoluntary")]
    public ObligationType HcOtCVoluntary
    {
      get => hcOtCVoluntary ??= new();
      set => hcOtCVoluntary = value;
    }

    /// <summary>
    /// A value of IclassCommon.
    /// </summary>
    [JsonPropertyName("iclassCommon")]
    public Common IclassCommon
    {
      get => iclassCommon ??= new();
      set => iclassCommon = value;
    }

    /// <summary>
    /// A value of IclassLegalAction.
    /// </summary>
    [JsonPropertyName("iclassLegalAction")]
    public LegalAction IclassLegalAction
    {
      get => iclassLegalAction ??= new();
      set => iclassLegalAction = value;
    }

    /// <summary>
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
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
    /// A value of IwglInd.
    /// </summary>
    [JsonPropertyName("iwglInd")]
    public WorkArea IwglInd
    {
      get => iwglInd ??= new();
      set => iwglInd = value;
    }

    /// <summary>
    /// A value of Jclass.
    /// </summary>
    [JsonPropertyName("jclass")]
    public LegalAction Jclass
    {
      get => jclass ??= new();
      set => jclass = value;
    }

    /// <summary>
    /// A value of ZdelLocalJClassForIwoFound.
    /// </summary>
    [JsonPropertyName("zdelLocalJClassForIwoFound")]
    public TextWorkArea ZdelLocalJClassForIwoFound
    {
      get => zdelLocalJClassForIwoFound ??= new();
      set => zdelLocalJClassForIwoFound = value;
    }

    /// <summary>
    /// A value of JorOClass.
    /// </summary>
    [JsonPropertyName("jorOClass")]
    public LegalActionDetail JorOClass
    {
      get => jorOClass ??= new();
      set => jorOClass = value;
    }

    /// <summary>
    /// A value of ZdelLocalJOrOClassForMwo.
    /// </summary>
    [JsonPropertyName("zdelLocalJOrOClassForMwo")]
    public TextWorkArea ZdelLocalJOrOClassForMwo
    {
      get => zdelLocalJOrOClassForMwo ??= new();
      set => zdelLocalJOrOClassForMwo = value;
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
    /// A value of LegalActionAssignment.
    /// </summary>
    [JsonPropertyName("legalActionAssignment")]
    public Common LegalActionAssignment
    {
      get => legalActionAssignment ??= new();
      set => legalActionAssignment = value;
    }

    /// <summary>
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    /// <summary>
    /// A value of LegalActionIncomeSource.
    /// </summary>
    [JsonPropertyName("legalActionIncomeSource")]
    public LegalActionIncomeSource LegalActionIncomeSource
    {
      get => legalActionIncomeSource ??= new();
      set => legalActionIncomeSource = value;
    }

    /// <summary>
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
    }

    /// <summary>
    /// A value of Mwo.
    /// </summary>
    [JsonPropertyName("mwo")]
    public LegalAction Mwo
    {
      get => mwo ??= new();
      set => mwo = value;
    }

    /// <summary>
    /// A value of MwoAllSpAreEmancipated.
    /// </summary>
    [JsonPropertyName("mwoAllSpAreEmancipated")]
    public Common MwoAllSpAreEmancipated
    {
      get => mwoAllSpAreEmancipated ??= new();
      set => mwoAllSpAreEmancipated = value;
    }

    /// <summary>
    /// A value of MwoHiViabIndIsN.
    /// </summary>
    [JsonPropertyName("mwoHiViabIndIsN")]
    public Common MwoHiViabIndIsN
    {
      get => mwoHiViabIndIsN ??= new();
      set => mwoHiViabIndIsN = value;
    }

    /// <summary>
    /// A value of MwoHicFoundForAllSp.
    /// </summary>
    [JsonPropertyName("mwoHicFoundForAllSp")]
    public Common MwoHicFoundForAllSp
    {
      get => mwoHicFoundForAllSp ??= new();
      set => mwoHicFoundForAllSp = value;
    }

    /// <summary>
    /// A value of MwonothcIsRequired.
    /// </summary>
    [JsonPropertyName("mwonothcIsRequired")]
    public TextWorkArea MwonothcIsRequired
    {
      get => mwonothcIsRequired ??= new();
      set => mwonothcIsRequired = value;
    }

    /// <summary>
    /// A value of MultiPayor.
    /// </summary>
    [JsonPropertyName("multiPayor")]
    public Common MultiPayor
    {
      get => multiPayor ??= new();
      set => multiPayor = value;
    }

    /// <summary>
    /// A value of NonCsiLo1Total.
    /// </summary>
    [JsonPropertyName("nonCsiLo1Total")]
    public Common NonCsiLo1Total
    {
      get => nonCsiLo1Total ??= new();
      set => nonCsiLo1Total = value;
    }

    /// <summary>
    /// A value of NullDateWorkArea.
    /// </summary>
    [JsonPropertyName("nullDateWorkArea")]
    public DateWorkArea NullDateWorkArea
    {
      get => nullDateWorkArea ??= new();
      set => nullDateWorkArea = value;
    }

    /// <summary>
    /// A value of NullLegalActionDetail.
    /// </summary>
    [JsonPropertyName("nullLegalActionDetail")]
    public LegalActionDetail NullLegalActionDetail
    {
      get => nullLegalActionDetail ??= new();
      set => nullLegalActionDetail = value;
    }

    /// <summary>
    /// A value of NullLegalActionPerson.
    /// </summary>
    [JsonPropertyName("nullLegalActionPerson")]
    public LegalActionPerson NullLegalActionPerson
    {
      get => nullLegalActionPerson ??= new();
      set => nullLegalActionPerson = value;
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
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public LegalActionDetail Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of PreviousDistinctLegalAction.
    /// </summary>
    [JsonPropertyName("previousDistinctLegalAction")]
    public LegalAction PreviousDistinctLegalAction
    {
      get => previousDistinctLegalAction ??= new();
      set => previousDistinctLegalAction = value;
    }

    /// <summary>
    /// A value of PreviousDistinctTribunal.
    /// </summary>
    [JsonPropertyName("previousDistinctTribunal")]
    public Tribunal PreviousDistinctTribunal
    {
      get => previousDistinctTribunal ??= new();
      set => previousDistinctTribunal = value;
    }

    /// <summary>
    /// A value of SameFrequency.
    /// </summary>
    [JsonPropertyName("sameFrequency")]
    public LegalActionDetail SameFrequency
    {
      get => sameFrequency ??= new();
      set => sameFrequency = value;
    }

    /// <summary>
    /// A value of ScreenObligationStatus.
    /// </summary>
    [JsonPropertyName("screenObligationStatus")]
    public ScreenObligationStatus ScreenObligationStatus
    {
      get => screenObligationStatus ??= new();
      set => screenObligationStatus = value;
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
    /// A value of StopProcessingThisCase.
    /// </summary>
    [JsonPropertyName("stopProcessingThisCase")]
    public Common StopProcessingThisCase
    {
      get => stopProcessingThisCase ??= new();
      set => stopProcessingThisCase = value;
    }

    /// <summary>
    /// A value of ToBeCreated.
    /// </summary>
    [JsonPropertyName("toBeCreated")]
    public Common ToBeCreated
    {
      get => toBeCreated ??= new();
      set => toBeCreated = value;
    }

    /// <summary>
    /// A value of Today.
    /// </summary>
    [JsonPropertyName("today")]
    public DateWorkArea Today
    {
      get => today ??= new();
      set => today = value;
    }

    /// <summary>
    /// A value of ValidIwoIncomeSource.
    /// </summary>
    [JsonPropertyName("validIwoIncomeSource")]
    public Common ValidIwoIncomeSource
    {
      get => validIwoIncomeSource ??= new();
      set => validIwoIncomeSource = value;
    }

    /// <summary>
    /// A value of ValidMwoIncomeSource.
    /// </summary>
    [JsonPropertyName("validMwoIncomeSource")]
    public Common ValidMwoIncomeSource
    {
      get => validMwoIncomeSource ??= new();
      set => validMwoIncomeSource = value;
    }

    /// <summary>
    /// A value of Wc.
    /// </summary>
    [JsonPropertyName("wc")]
    public LegalActionDetail Wc
    {
      get => wc ??= new();
      set => wc = value;
    }

    /// <summary>
    /// A value of CompareDateFiled.
    /// </summary>
    [JsonPropertyName("compareDateFiled")]
    public DateWorkArea CompareDateFiled
    {
      get => compareDateFiled ??= new();
      set => compareDateFiled = value;
    }

    private CodeValue autoIwoCheckCodeValue;
    private Code autoIwoCheckCode;
    private CodeValue autoMwoCheckCodeValue;
    private Code autoMwoCheckCode;
    private Case1 case1;
    private Common continueIwoProcessing;
    private Common continueMwoProcessing;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common csiLo1Total;
    private Common detailObligated;
    private Common differentPeriods;
    private Document document;
    private WorkArea employerAddressNote;
    private Common firstLegalAction;
    private Array<GroupGroup> group;
    private ObligationType hcOtCAccruing;
    private ObligationType hcOtCVoluntary;
    private Common iclassCommon;
    private LegalAction iclassLegalAction;
    private IncomeSource incomeSource;
    private Infrastructure infrastructure;
    private WorkArea iwglInd;
    private LegalAction jclass;
    private TextWorkArea zdelLocalJClassForIwoFound;
    private LegalActionDetail jorOClass;
    private TextWorkArea zdelLocalJOrOClassForMwo;
    private LegalAction legalAction;
    private Common legalActionAssignment;
    private LegalActionDetail legalActionDetail;
    private LegalActionIncomeSource legalActionIncomeSource;
    private LegalActionPerson legalActionPerson;
    private LegalAction mwo;
    private Common mwoAllSpAreEmancipated;
    private Common mwoHiViabIndIsN;
    private Common mwoHicFoundForAllSp;
    private TextWorkArea mwonothcIsRequired;
    private Common multiPayor;
    private Common nonCsiLo1Total;
    private DateWorkArea nullDateWorkArea;
    private LegalActionDetail nullLegalActionDetail;
    private LegalActionPerson nullLegalActionPerson;
    private ObligationType obligationType;
    private LegalActionDetail previous;
    private LegalAction previousDistinctLegalAction;
    private Tribunal previousDistinctTribunal;
    private LegalActionDetail sameFrequency;
    private ScreenObligationStatus screenObligationStatus;
    private SpDocKey spDocKey;
    private Common stopProcessingThisCase;
    private Common toBeCreated;
    private DateWorkArea today;
    private Common validIwoIncomeSource;
    private Common validMwoIncomeSource;
    private LegalActionDetail wc;
    private DateWorkArea compareDateFiled;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ErrorMessage.
    /// </summary>
    [JsonPropertyName("errorMessage")]
    public LegalAction ErrorMessage
    {
      get => errorMessage ??= new();
      set => errorMessage = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
    }

    /// <summary>
    /// A value of AbsentParent.
    /// </summary>
    [JsonPropertyName("absentParent")]
    public CaseRole AbsentParent
    {
      get => absentParent ??= new();
      set => absentParent = value;
    }

    /// <summary>
    /// A value of ApplicantRecipient.
    /// </summary>
    [JsonPropertyName("applicantRecipient")]
    public CaseRole ApplicantRecipient
    {
      get => applicantRecipient ??= new();
      set => applicantRecipient = value;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    /// <summary>
    /// A value of CopyThisCsePerson.
    /// </summary>
    [JsonPropertyName("copyThisCsePerson")]
    public CsePerson CopyThisCsePerson
    {
      get => copyThisCsePerson ??= new();
      set => copyThisCsePerson = value;
    }

    /// <summary>
    /// A value of CopyThisLaPersonLaCaseRole.
    /// </summary>
    [JsonPropertyName("copyThisLaPersonLaCaseRole")]
    public LaPersonLaCaseRole CopyThisLaPersonLaCaseRole
    {
      get => copyThisLaPersonLaCaseRole ??= new();
      set => copyThisLaPersonLaCaseRole = value;
    }

    /// <summary>
    /// A value of CopyThisLegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("copyThisLegalActionCaseRole")]
    public LegalActionCaseRole CopyThisLegalActionCaseRole
    {
      get => copyThisLegalActionCaseRole ??= new();
      set => copyThisLegalActionCaseRole = value;
    }

    /// <summary>
    /// A value of CopyThisLegalActionPerson.
    /// </summary>
    [JsonPropertyName("copyThisLegalActionPerson")]
    public LegalActionPerson CopyThisLegalActionPerson
    {
      get => copyThisLegalActionPerson ??= new();
      set => copyThisLegalActionPerson = value;
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
    /// A value of Distinct.
    /// </summary>
    [JsonPropertyName("distinct")]
    public LegalAction Distinct
    {
      get => distinct ??= new();
      set => distinct = value;
    }

    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    /// <summary>
    /// A value of EmployerAddress.
    /// </summary>
    [JsonPropertyName("employerAddress")]
    public EmployerAddress EmployerAddress
    {
      get => employerAddress ??= new();
      set => employerAddress = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
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
    /// A value of HealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("healthInsuranceCoverage")]
    public HealthInsuranceCoverage HealthInsuranceCoverage
    {
      get => healthInsuranceCoverage ??= new();
      set => healthInsuranceCoverage = value;
    }

    /// <summary>
    /// A value of HealthInsuranceViability.
    /// </summary>
    [JsonPropertyName("healthInsuranceViability")]
    public HealthInsuranceViability HealthInsuranceViability
    {
      get => healthInsuranceViability ??= new();
      set => healthInsuranceViability = value;
    }

    /// <summary>
    /// A value of IclassLegalAction.
    /// </summary>
    [JsonPropertyName("iclassLegalAction")]
    public LegalAction IclassLegalAction
    {
      get => iclassLegalAction ??= new();
      set => iclassLegalAction = value;
    }

    /// <summary>
    /// A value of IclassLegalActionDetail.
    /// </summary>
    [JsonPropertyName("iclassLegalActionDetail")]
    public LegalActionDetail IclassLegalActionDetail
    {
      get => iclassLegalActionDetail ??= new();
      set => iclassLegalActionDetail = value;
    }

    /// <summary>
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
    }

    /// <summary>
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    /// <summary>
    /// A value of InterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("interstateRequestHistory")]
    public InterstateRequestHistory InterstateRequestHistory
    {
      get => interstateRequestHistory ??= new();
      set => interstateRequestHistory = value;
    }

    /// <summary>
    /// A value of IwoLaPersonLaCaseRole.
    /// </summary>
    [JsonPropertyName("iwoLaPersonLaCaseRole")]
    public LaPersonLaCaseRole IwoLaPersonLaCaseRole
    {
      get => iwoLaPersonLaCaseRole ??= new();
      set => iwoLaPersonLaCaseRole = value;
    }

    /// <summary>
    /// A value of IwoLegalActionPerson.
    /// </summary>
    [JsonPropertyName("iwoLegalActionPerson")]
    public LegalActionPerson IwoLegalActionPerson
    {
      get => iwoLegalActionPerson ??= new();
      set => iwoLegalActionPerson = value;
    }

    /// <summary>
    /// A value of IwoLegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("iwoLegalActionCaseRole")]
    public LegalActionCaseRole IwoLegalActionCaseRole
    {
      get => iwoLegalActionCaseRole ??= new();
      set => iwoLegalActionCaseRole = value;
    }

    /// <summary>
    /// A value of Jclass.
    /// </summary>
    [JsonPropertyName("jclass")]
    public Obligation Jclass
    {
      get => jclass ??= new();
      set => jclass = value;
    }

    /// <summary>
    /// A value of JorOClass.
    /// </summary>
    [JsonPropertyName("jorOClass")]
    public LegalAction JorOClass
    {
      get => jorOClass ??= new();
      set => jorOClass = value;
    }

    /// <summary>
    /// A value of LaDetFinancial.
    /// </summary>
    [JsonPropertyName("laDetFinancial")]
    public LegalActionDetail LaDetFinancial
    {
      get => laDetFinancial ??= new();
      set => laDetFinancial = value;
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
    /// A value of LegalActionAssigment.
    /// </summary>
    [JsonPropertyName("legalActionAssigment")]
    public LegalActionAssigment LegalActionAssigment
    {
      get => legalActionAssigment ??= new();
      set => legalActionAssigment = value;
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
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    /// <summary>
    /// A value of LegalActionIncomeSource.
    /// </summary>
    [JsonPropertyName("legalActionIncomeSource")]
    public LegalActionIncomeSource LegalActionIncomeSource
    {
      get => legalActionIncomeSource ??= new();
      set => legalActionIncomeSource = value;
    }

    /// <summary>
    /// A value of MultipayorCsePerson.
    /// </summary>
    [JsonPropertyName("multipayorCsePerson")]
    public CsePerson MultipayorCsePerson
    {
      get => multipayorCsePerson ??= new();
      set => multipayorCsePerson = value;
    }

    /// <summary>
    /// A value of MultipayorLegalActionDetail.
    /// </summary>
    [JsonPropertyName("multipayorLegalActionDetail")]
    public LegalActionDetail MultipayorLegalActionDetail
    {
      get => multipayorLegalActionDetail ??= new();
      set => multipayorLegalActionDetail = value;
    }

    /// <summary>
    /// A value of MultipayorLegalActionPerson.
    /// </summary>
    [JsonPropertyName("multipayorLegalActionPerson")]
    public LegalActionPerson MultipayorLegalActionPerson
    {
      get => multipayorLegalActionPerson ??= new();
      set => multipayorLegalActionPerson = value;
    }

    /// <summary>
    /// A value of Mwo.
    /// </summary>
    [JsonPropertyName("mwo")]
    public LegalActionPerson Mwo
    {
      get => mwo ??= new();
      set => mwo = value;
    }

    /// <summary>
    /// A value of MwoSupportedPerson.
    /// </summary>
    [JsonPropertyName("mwoSupportedPerson")]
    public CsePerson MwoSupportedPerson
    {
      get => mwoSupportedPerson ??= new();
      set => mwoSupportedPerson = value;
    }

    /// <summary>
    /// A value of NonNOrUClass.
    /// </summary>
    [JsonPropertyName("nonNOrUClass")]
    public LegalAction NonNOrUClass
    {
      get => nonNOrUClass ??= new();
      set => nonNOrUClass = value;
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
    /// A value of Obligor1.
    /// </summary>
    [JsonPropertyName("obligor1")]
    public CsePersonAccount Obligor1
    {
      get => obligor1 ??= new();
      set => obligor1 = value;
    }

    /// <summary>
    /// A value of Obligor2.
    /// </summary>
    [JsonPropertyName("obligor2")]
    public CsePerson Obligor2
    {
      get => obligor2 ??= new();
      set => obligor2 = value;
    }

    /// <summary>
    /// A value of Ordiwo2.
    /// </summary>
    [JsonPropertyName("ordiwo2")]
    public LegalAction Ordiwo2
    {
      get => ordiwo2 ??= new();
      set => ordiwo2 = value;
    }

    /// <summary>
    /// A value of PersonalHealthInsurance.
    /// </summary>
    [JsonPropertyName("personalHealthInsurance")]
    public PersonalHealthInsurance PersonalHealthInsurance
    {
      get => personalHealthInsurance ??= new();
      set => personalHealthInsurance = value;
    }

    /// <summary>
    /// A value of PreviousAssignment.
    /// </summary>
    [JsonPropertyName("previousAssignment")]
    public LegalAction PreviousAssignment
    {
      get => previousAssignment ??= new();
      set => previousAssignment = value;
    }

    /// <summary>
    /// A value of Termmwoo.
    /// </summary>
    [JsonPropertyName("termmwoo")]
    public LegalAction Termmwoo
    {
      get => termmwoo ??= new();
      set => termmwoo = value;
    }

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of ZdelNew.
    /// </summary>
    [JsonPropertyName("zdelNew")]
    public LegalAction ZdelNew
    {
      get => zdelNew ??= new();
      set => zdelNew = value;
    }

    /// <summary>
    /// A value of IclassTerm.
    /// </summary>
    [JsonPropertyName("iclassTerm")]
    public LegalAction IclassTerm
    {
      get => iclassTerm ??= new();
      set => iclassTerm = value;
    }

    private LegalAction errorMessage;
    private ServiceProvider serviceProvider;
    private Office office;
    private OfficeServiceProvider officeServiceProvider;
    private CaseAssignment caseAssignment;
    private CaseRole absentParent;
    private CaseRole applicantRecipient;
    private Bankruptcy bankruptcy;
    private Case1 case1;
    private CaseRole caseRole;
    private CsePerson copyThisCsePerson;
    private LaPersonLaCaseRole copyThisLaPersonLaCaseRole;
    private LegalActionCaseRole copyThisLegalActionCaseRole;
    private LegalActionPerson copyThisLegalActionPerson;
    private CsePerson csePerson;
    private LegalAction distinct;
    private Employer employer;
    private EmployerAddress employerAddress;
    private Fips fips;
    private GoodCause goodCause;
    private HealthInsuranceCoverage healthInsuranceCoverage;
    private HealthInsuranceViability healthInsuranceViability;
    private LegalAction iclassLegalAction;
    private LegalActionDetail iclassLegalActionDetail;
    private IncomeSource incomeSource;
    private InterstateRequest interstateRequest;
    private InterstateRequestHistory interstateRequestHistory;
    private LaPersonLaCaseRole iwoLaPersonLaCaseRole;
    private LegalActionPerson iwoLegalActionPerson;
    private LegalActionCaseRole iwoLegalActionCaseRole;
    private Obligation jclass;
    private LegalAction jorOClass;
    private LegalActionDetail laDetFinancial;
    private LegalAction legalAction;
    private LegalActionAssigment legalActionAssigment;
    private LegalActionCaseRole legalActionCaseRole;
    private LegalActionDetail legalActionDetail;
    private LegalActionIncomeSource legalActionIncomeSource;
    private CsePerson multipayorCsePerson;
    private LegalActionDetail multipayorLegalActionDetail;
    private LegalActionPerson multipayorLegalActionPerson;
    private LegalActionPerson mwo;
    private CsePerson mwoSupportedPerson;
    private LegalAction nonNOrUClass;
    private Obligation obligation;
    private ObligationType obligationType;
    private CsePersonAccount obligor1;
    private CsePerson obligor2;
    private LegalAction ordiwo2;
    private PersonalHealthInsurance personalHealthInsurance;
    private LegalAction previousAssignment;
    private LegalAction termmwoo;
    private Tribunal tribunal;
    private LegalAction zdelNew;
    private LegalAction iclassTerm;
  }
#endregion
}
