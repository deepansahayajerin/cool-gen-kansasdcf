// Program: LE_AUTOMATIC_IWO_GENERATION, ID: 371064619, model: 746.
// Short name: SWE02042
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_AUTOMATIC_IWO_GENERATION.
/// </summary>
[Serializable]
public partial class LeAutomaticIwoGeneration: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_AUTOMATIC_IWO_GENERATION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeAutomaticIwoGeneration(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeAutomaticIwoGeneration.
  /// </summary>
  public LeAutomaticIwoGeneration(IContext context, Import import, Export export)
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
    // ---------------------------------------------------------------------------------------------------------
    // Date	  Author    Request	Description
    // 02/21/01  GVandy    WR187	Initial Development
    // 05/16/01  GVandy    PR 120142	Modify arrearage calculations for WA legal 
    // detail on the NOI.
    // 05/16/01  GVandy    PR 120182	Create LOPS records on the NOI.
    // 05/16/01  GVandy    PR 120160	Set previous IWGL end date to yesterday.
    // 06/01/01  GVandy    PR 120163	Only check for assignment reason code 'RSP'
    // when
    // 				determining if most recent legal action is assigned.
    // 				(The call to the create document cab fails if there
    // 				is no current RSP assignment.)
    // 06/01/01  GVandy    PR 120164	If we use an existing ORDIWO2, insure that 
    // there is
    // 				an RSP assignment to that legal action.  (The call to
    // 				the create document cab fails if there is no current
    // 				RSP assignment.)
    // 06/13/01  GVandy    PR 121482	Pass the related case # to the document 
    // trigger cab.
    // 06/13/01  GVandy    PR 121229	Add partitioning attribute to subtype reads
    // to solve
    // 				code gen problem.
    // 06/28/01  GVandy    WR 10509	1. Only use legal action details that have 
    // been
    // 				   obligated when creating the WC amount for the
    // 				   NOIIWONA.
    // 				2. Instead of creating an ORDIWO2, generate an alert
    // 				   if the WA amount of the previous IWO exceeds the
    // 				   current arrears debt amount.
    // 				3. Always create a new legal action for each
    // 				   automatic ORDIWO2A created.
    // 06/29/01  GVandy    PR 122678	Add detail messages to alerts.
    // 05/20/02  GVandy    PR 146228	Do not create LOPS records to inactive case
    // roles on
    // 				the NOIIWON.
    // 11/01/02  GVandy    WR010492	Set system_gen_ind on new legal actions.
    // 12/18/02  GVandy    PR163914	Copy forward withholding percent (limit) for
    // auto
    // 				ORDIWO2 legal details.
    // 04/25/03  GVandy    PR174900	Initialize case number to spaces before 
    // reading for
    // 				associated cases.
    // 09/26/07  MFan      WR296917 	Part C  Commented out (disabled) the codes 
    // that check
    // 				if AP is in bankruptcy
    // 08/29/08  JHuss     CQ 105	Added support for MWO.
    // 		    CQ 609	Added check for * in position 1 of Employer address
    // 				note line.
    // 09/04/09  JHuss     CQ 12951	Reset stop all processing flag. Give flag 
    // more
    // 				descriptive name.
    // 05/31/11  TPierce   CQ 25585	Modify to use new code table entries to 
    // control
    // 				processing of IWO and NMSN generation.
    // 05/22/12  GVandy    CQ 33628	The following changes were made to support 
    // IWOs for
    // 				Department of Labor Unemployment Compensation
    // 				Witthholding.
    // 				1) Generate ORDIWO2B document for KDOL UI IWOs.
    // 				2) Add filter to generate auto IWO for specified
    // 				   standard number only.
    // 				3) Add "special" interstate caseload for which auto
    // 				   IWOs should be allowed to generate for non Kansas
    // 				   orders.
    // 7/03/13  LSS       CQ 39236	Change the process for when an auto IWO is 
    // generated.
    // 				Change the READ for i-class legal actions to 3
    // 				separate READs
    // 					1) READ for IWO and IWO modification for most
    // 					   recently filed.
    // 					2) READ for Termination with a created date
    // 					   greater than the filed date of IWO or IWO
    // 					   modification - if so send alert - if not
    // 					   and filed date is not a null date create
    // 					   an ORDIWO2
    // 					3) READ for NOIIWON when no IWOs or
    // 					   Termination - if have one send alert - if
    // 					   no NOI create one.
    // 2/17/17  AHockman   cq47796	changes made to skip / ignore most recent 
    // IWOMODO
    // 				when WA and WC = zero.
    // 08/02/18  GVandy    CQ61457	Update SVES and 'O' type employer to work 
    // with eIWO
    // 				for SSA.
    // ---------------------------------------------------------------------------------------------------------
    // *************************************************************************************
    // CQ25585 T. Pierce       05/2011 Set code table constants for automatic 
    // MWO and IWO
    // 			generation.
    // *************************************************************************************
    local.AutoMwoCheckCode.CodeName = "AUTOMATIC MWO GENERATION";
    local.AutoMwoCheckCodeValue.Cdvalue = "Y";
    local.AutoIwoCheckCode.CodeName = "AUTOMATIC IWO GENERATION";
    local.AutoIwoCheckCodeValue.Cdvalue = "Y";
    local.Max.Date = new DateTime(2099, 12, 31);

    // *************************************************************************************
    // CQ25585 T. Pierce       05/2011 End changes.
    // *************************************************************************************
    local.Today.Date = Now().Date;

    if (ReadCsePerson1())
    {
      local.Eiwo.Number = entities.CsePerson.Number;

      if (!IsEmpty(Substring(global.TranCode, 5, 1)))
      {
        UseSiReadCsePersonBatch();

        switch(AsChar(export.AbendData.Type1))
        {
          case ' ':
            // ************************************************
            // *Successfull Adabas Read Occurred.             *
            // ************************************************
            goto Read1;
          case 'A':
            // ************************************************
            // *Unsuccessfull ADABAS Read Occurred.           *
            // ************************************************
            if (Equal(export.AbendData.AdabasResponseCd, "0148"))
            {
              export.CsePersonsWorkSet.LastName = "*ADABAS UNAVAIL*";
              export.CsePersonsWorkSet.FormattedName =
                "** ADABAS UNAVAILABLE **";
              ExitState = "ADABAS_UNAVAILABLE_RB";
            }
            else
            {
              ExitState = "ADABAS_READ_UNSUCCESSFUL";
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

            break;
          default:
            ExitState = "ADABAS_INVALID_RETURN_CODE";

            break;
        }

        return;
      }
      else
      {
        UseSiReadCsePerson();

        switch(AsChar(export.AbendData.Type1))
        {
          case ' ':
            // ---------------------------------------------
            // Matches found
            // ---------------------------------------------
            break;
          case 'A':
            switch(TrimEnd(export.AbendData.AdabasFileNumber))
            {
              case "0000":
                ExitState = "ACO_ADABAS_UNAVAILABLE";

                break;
              case "0149":
                switch(TrimEnd(export.AbendData.AdabasFileAction))
                {
                  case "AVF":
                    ExitState = "ACO_ADABAS_NO_EXACT_MATCHES";

                    break;
                  case "BVF":
                    ExitState = "ACO_ADABAS_NO_PHONETIC_MATCH";

                    break;
                  case "CVF":
                    ExitState = "ACO_ADABAS_NO_SSN_MATCH";

                    break;
                  default:
                    break;
                }

                break;
              default:
                break;
            }

            break;
          case 'C':
            ExitState = "ACO_RE0000_CICS_UNAVAILABLE_RB";

            break;
          default:
            break;
        }
      }
    }
    else
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

Read1:

    // --08/02/18 GVandy CQ61457  Update SVES and 'O' type employer to work with
    // eIWO for
    //   SSA.  Always read for the income source because the CODE attribute is 
    // not in the
    //   import income source view.
    if (ReadIncomeSource())
    {
      local.IncomeSource.Assign(entities.IncomeSource);
    }
    else
    {
      ExitState = "INCOME_SOURCE_NF";

      return;
    }

    // *************************************************************************************
    // If the income source type is 'E'mployment, the income source is valid for
    // both the
    // IWO and MWO only when the return code is 'E'mployed.
    // The Military accepts IWOs but does not accept MWOs.
    // Do not send the MWO if the income source type is 'M'ilitary,
    // Only send the IWO if the income source return code is 'A'ctive or 'R'
    // etired.
    // *************************************************************************************
    local.ValidIwoIncomeSource.Flag = "N";
    local.ValidMwoIncomeSource.Flag = "N";

    if (AsChar(local.IncomeSource.Type1) == 'E')
    {
      if (AsChar(local.IncomeSource.ReturnCd) == 'E')
      {
        local.ValidIwoIncomeSource.Flag = "Y";
        local.ValidMwoIncomeSource.Flag = "Y";
      }
      else if (AsChar(local.IncomeSource.ReturnCd) == 'U')
      {
        local.ValidIwoIncomeSource.Flag = "Y";
      }
    }
    else if (AsChar(local.IncomeSource.Type1) == 'M')
    {
      if (AsChar(local.IncomeSource.ReturnCd) == 'A' || AsChar
        (local.IncomeSource.ReturnCd) == 'R')
      {
        local.ValidIwoIncomeSource.Flag = "Y";
      }
    }
    else if (AsChar(local.IncomeSource.Type1) == 'O')
    {
      // --08/02/18 GVandy CQ61457  Update SVES and 'O' type employer to work 
      // with eIWO for
      //   SSA.  WC is now a valid IWO income source code.
      if (Equal(local.IncomeSource.Code, "SA") || Equal
        (local.IncomeSource.Code, "WC"))
      {
        if (AsChar(local.IncomeSource.ReturnCd) == 'V')
        {
          local.ValidIwoIncomeSource.Flag = "Y";
        }
      }
    }

    if (AsChar(local.ValidIwoIncomeSource.Flag) == 'N' && AsChar
      (local.ValidMwoIncomeSource.Flag) == 'N')
    {
      return;
    }

    local.EiwoEmployer.Flag = "";

    if (ReadEmployer2())
    {
      if (!Lt(Now().Date, entities.Employer.EiwoStartDate) && !
        Lt(entities.Employer.EiwoEndDate, Now().Date))
      {
        local.EiwoEmployer.Flag = "Y";
      }
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

      // *************************************************************************************
      // Skip this court case if a standard number was specified in the 
      // import_filter
      // legal_action view and the standard number on this court order does not 
      // match.
      // *************************************************************************************
      if (!IsEmpty(import.Filter.StandardNumber))
      {
        if (ReadLegalAction1())
        {
          // -- This court case matches the import filter standard number.  
          // Continue with the auto IWO process.
        }
        else
        {
          // -- Skip this court case since it does not match the import filter 
          // standard number.
          continue;
        }
      }

      // *************************************************************************************
      // Skip this court case if an associated AR has claimed good cause.
      // *************************************************************************************
      foreach(var item1 in ReadCase2())
      {
        local.Case1.Number = entities.Case1.Number;

        foreach(var item2 in ReadGoodCause())
        {
          if (Equal(entities.GoodCause.Code, "GC"))
          {
            goto ReadEach2;
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
            goto ReadEach2;
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
          if (Equal(global.UserId, "SWELB578") || Equal
            (global.UserId, "SWELB579"))
          {
            if (ReadCaseAssignment())
            {
              // -- This court order is assigned to the "special" interstate 
              // caseload.  Allow the auto
              // -- IWO to proceed.  This allows incoming interstate cases not 
              // registered in Kansas
              // -- and currently assigned to Mike Cruse in the interstate 
              // office to generate auto
              // -- IWOs for DOL UI.
              goto Test1;
            }
          }

          continue;
        }

Test1:

        if (entities.Fips.Location >= 20 && entities.Fips.Location <= 99)
        {
          // -- Also skip Tribal tribunals in the state of Kansas.
          if (Equal(global.UserId, "SWELB578") || Equal
            (global.UserId, "SWELB579"))
          {
            if (ReadCaseAssignment())
            {
              // -- This court order is assigned to the "special" interstate 
              // caseload.  Allow the auto
              // -- IWO to proceed.  This allows incoming interstate cases not 
              // registered in Kansas
              // -- and currently assigned to Mike Cruse in the interstate 
              // office to generate auto
              // -- IWOs for DOL UI.
              goto Read2;
            }
          }

          continue;
        }
      }
      else
      {
        continue;
      }

Read2:

      local.ContinueIwoProcessing.Flag = "N";
      local.ContinueMwoProcessing.Flag = "N";

      // *************************************************************************************
      // Skip this court case if there is no J or O class legal action with a 
      // filed date.
      // *************************************************************************************
      foreach(var item1 in ReadLegalAction5())
      {
        if (AsChar(entities.LegalAction.Classification) == 'J')
        {
          local.ContinueIwoProcessing.Flag = "Y";
          local.Jclass.Assign(entities.LegalAction);
        }

        if (ReadLegalActionDetail1())
        {
          local.ContinueMwoProcessing.Flag = "Y";
          local.JorOClass.Assign(entities.LegalActionDetail);
          MoveLegalAction3(entities.LegalAction, local.Mwo);
        }

        if (AsChar(local.ContinueMwoProcessing.Flag) == 'Y' && AsChar
          (local.ContinueIwoProcessing.Flag) == 'Y')
        {
          break;
        }
      }

      // *************************************************************************************
      // Skip this court case if there is no J or O class legal action with a 
      // filed date.
      // *************************************************************************************
      local.ToBeCreated.Command = "";

      if (AsChar(local.ContinueIwoProcessing.Flag) == 'Y' || AsChar
        (local.ContinueMwoProcessing.Flag) == 'Y')
      {
        // -- Check for a legal service provider assigned to the most recent non
        // "N" and non "U" legal action for this court case.
        foreach(var item1 in ReadLegalAction9())
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

          // Change the following code to reassign legal action to k county of 
          // the tribunal for cq47223
          if (local.LegalActionAssignment.Count == 0)
          {
            // -- No legal service provider is assigned to the court case.  
            // Generate an alert to the caseworker.
            if (Equal(entities.Fips.StateAbbreviation, "KS"))
            {
              if (ReadServiceProvider1())
              {
                if (ReadOfficeServiceProviderOffice2())
                {
                  MoveOfficeServiceProvider(entities.OfficeServiceProvider,
                    local.OfficeServiceProvider);
                  local.Office.SystemGeneratedId =
                    entities.Office.SystemGeneratedId;
                  local.LegalActionAssigment.ReasonCode = "RSP";
                  local.LegalActionAssigment.EffectiveDate = local.Today.Date;
                  local.LegalActionAssigment.DiscontinueDate = local.Max.Date;
                  local.LegalActionAssigment.LastUpdatedTimestamp = Now();
                  local.LegalActionAssigment.LastUpdatedBy = global.UserId;
                  UseSpCabCreateLegalActionAssgn();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    UseEabExtractExitStateMessage();
                    local.ToBeCreated.Command = "ALERT";
                    local.Infrastructure.Detail = "Service Provider #" + entities
                      .ServiceProvider.UserId + " errored because: " + local
                      .ExitStateWorkArea.Message;
                    local.StopProcessingThisCase.Flag = "Y";
                    ExitState = "ACO_NN0000_ALL_OK";

                    goto Test2;
                  }
                }
                else
                {
                  local.ToBeCreated.Command = "ALERT";
                  local.Infrastructure.Detail =
                    "Office Service Provider is not assigned for the K county.";
                    
                  local.StopProcessingThisCase.Flag = "Y";

                  goto Test2;
                }
              }
              else
              {
                local.ToBeCreated.Command = "ALERT";
                local.Infrastructure.Detail =
                  "K County is not assigned for the county.";
                local.StopProcessingThisCase.Flag = "Y";

                goto Test2;
              }
            }
            else
            {
              // this should only be for DOL
              if (ReadServiceProvider2())
              {
                if (ReadOfficeServiceProviderOffice1())
                {
                  MoveOfficeServiceProvider(entities.OfficeServiceProvider,
                    local.OfficeServiceProvider);
                  local.Office.SystemGeneratedId =
                    entities.Office.SystemGeneratedId;
                  local.LegalActionAssigment.ReasonCode = "RSP";
                  local.LegalActionAssigment.EffectiveDate = local.Today.Date;
                  local.LegalActionAssigment.DiscontinueDate = local.Max.Date;
                  local.LegalActionAssigment.LastUpdatedTimestamp = Now();
                  local.LegalActionAssigment.LastUpdatedBy = global.UserId;
                  UseSpCabCreateLegalActionAssgn();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    UseEabExtractExitStateMessage();
                    local.ToBeCreated.Command = "ALERT";
                    local.Infrastructure.Detail = "Service Provider #" + entities
                      .ServiceProvider.UserId + " errored because: " + local
                      .ExitStateWorkArea.Message;
                    local.StopProcessingThisCase.Flag = "Y";
                    ExitState = "ACO_NN0000_ALL_OK";

                    goto Test2;
                  }
                }
                else
                {
                  local.ToBeCreated.Command = "ALERT";
                  local.Infrastructure.Detail =
                    "Office Service Provider is not assigned for the default attorney";
                    
                  local.StopProcessingThisCase.Flag = "Y";

                  goto Test2;
                }
              }
              else
              {
                local.ToBeCreated.Command = "ALERT";
                local.Infrastructure.Detail =
                  "Default CSS Administration attorney not found.";
                local.StopProcessingThisCase.Flag = "Y";

                goto Test2;
              }
            }
          }

          break;
        }

        // Check to make sure there is not a * in pos 1 of the note line for the
        // employer
        if (ReadEmployerAddress())
        {
          local.ToBeCreated.Command = "ALERT";
          local.Infrastructure.Detail =
            "Withholding orders not accepted at this address, associate another address.";
            
          local.StopProcessingThisCase.Flag = "Y";
        }
      }

Test2:

      // *********************************************************************
      // Start IWO Processing
      // *********************************************************************
      // *********************************************************************
      // CQ25585 T. Pierce       5/2011  Check code table designed to "turn off"
      // automatic IWO generation.  If the code value "Y" is found on the code
      // value table, then the local_continue_iwo_processing value will return
      // as "Y".  Otherwise, the value will be returned "N" and processing will
      // not continue further based on "IF" statements.
      // *********************************************************************
      if (AsChar(local.ContinueIwoProcessing.Flag) == 'Y')
      {
        UseCabValidateCodeValue1();

        // *************************************************************************************
        // CQ25585 T. Pierce       05/2011 End changes.
        // *************************************************************************************
      }

      // We will escape from inside the IF once we determine what action to 
      // take.
      if (IsEmpty(local.ToBeCreated.Command) && AsChar
        (local.ContinueIwoProcessing.Flag) == 'Y' && AsChar
        (local.ValidIwoIncomeSource.Flag) == 'Y' && AsChar
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
            goto Test3;
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
            goto Test3;
          }

          if (AsChar(local.Group.Item.ObligationType.Classification) != 'A' && AsChar
            (local.Group.Item.ObligationType.Classification) != 'M' && AsChar
            (local.Group.Item.ObligationType.Classification) != 'N')
          {
            // -- Skip the obligation if it is not Accruing ("A"), Medical ("M"
            // ), or Non-Accruing ("N").
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
          goto Test3;
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
        // of three things:
        //   1. Generate an alert indicating that a verified income source was 
        // added but an IWO was not sent.
        //   2. Generate an Intent to Request Income Withholding (NOIIWON) and e
        // -mail to the legal service provider.
        //   3. Generate an Order for Income Withholding (ORDIWO2) and mail to 
        // the employer.
        // *************************************************************************************
        // Added new code for CQ39236 begins here **************
        local.CompareDateFiled.Assign(local.NullDateWorkArea);

        foreach(var item1 in ReadLegalAction6())
        {
          if (AsChar(local.MultiPayor.Flag) == 'Y')
          {
            // -- This is a multipayor case.  Check if the IWO was issued for 
            // our Obligor.  If not, then skip this I class legal action.
            if (!ReadLegalActionPerson3())
            {
              // -- Our person is not an obligor on the legal action.  Check if 
              // anyone else is an obligor for the legal action.
              if (ReadLegalActionPerson5())
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
                  

                goto Test3;
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
              

            goto Test3;
          }

          // ** CQ47796 AHOCKMAN   ignore this IWOMODO if the WA and WC amounts 
          // are set to zeros.
          if (Equal(entities.IclassLegalAction.ActionTaken, "IWOMODO"))
          {
            if (ReadLegalActionDetail3())
            {
              if (!ReadLegalActionDetail4())
              {
                continue;
              }
            }
          }

          local.CompareDateFiled.Timestamp =
            entities.IclassLegalAction.CreatedTstamp;

          break;
        }

        foreach(var item1 in ReadLegalAction8())
        {
          if (AsChar(local.MultiPayor.Flag) == 'Y')
          {
            // -- This is a multipayor case.  Check if the IWO was issued for 
            // our Obligor.  If not, then skip this I class legal action.
            if (!ReadLegalActionPerson4())
            {
              // -- Our person is not an obligor on the legal action.  Check if 
              // anyone else is an obligor for the legal action.
              if (ReadLegalActionPerson6())
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
                  

                goto Test3;
              }
            }
          }

          local.ToBeCreated.Command = "ALERT";
          local.Infrastructure.Detail =
            "Termination created after create date of most recent IWO/IWO Modification.";
            

          goto Test3;
        }

        if (entities.IclassLegalAction.Populated)
        {
          local.ToBeCreated.Command = "ORDIWO2";

          goto Test3;
        }

        if (!entities.IclassLegalAction.Populated && !
          entities.IclassTerm.Populated)
        {
          foreach(var item1 in ReadLegalAction7())
          {
            if (AsChar(local.MultiPayor.Flag) == 'Y')
            {
              // -- This is a multipayor case.  Check if the IWO was issued for 
              // our Obligor.  If not, then skip this I class legal action.
              if (!ReadLegalActionPerson3())
              {
                // -- Our person is not an obligor on the legal action.  Check 
                // if anyone else is an obligor for the legal action.
                if (ReadLegalActionPerson5())
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
                    

                  goto Test3;
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

              goto Test3;
            }

            local.ToBeCreated.Command = "ALERT";
            local.Infrastructure.Detail =
              "An NOIIWON legal action is already in process.";

            goto Test3;
          }

          // -- No I class actions existed.  Generate the NOI
          local.ToBeCreated.Command = "NOIIWON";
        }

        // Added new code for CQ39236 ends here **************
        // CQ39236 disabled original code
      }

Test3:

      // CQ39236 END disabled
      // -- The following check is no longer necessary as we will now always 
      // create a new ORDIWO2 legal action, copying the service provider
      // assignment from the most recent non-N and non-U class legal action (WR
      // 10509)
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

        if (ReadLegalActionDetail2())
        {
          // comment out the following code for cq47223
        }
      }

      if (AsChar(local.ContinueIwoProcessing.Flag) == 'Y' && AsChar
        (local.ValidIwoIncomeSource.Flag) == 'Y')
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

            foreach(var item1 in ReadCase4())
            {
              local.Infrastructure.CaseNumber = entities.Case1.Number;
              UseSpCabCreateInfrastructure();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }
            }

            if (AsChar(local.StopProcessingThisCase.Flag) == 'N' && AsChar
              (local.ContinueMwoProcessing.Flag) == 'Y' && AsChar
              (local.ValidMwoIncomeSource.Flag) == 'Y')
            {
              local.ToBeCreated.Command = "";
            }

            break;
          case "NOIIWON":
            // *************************************************************************************
            // Generate the Notice of Intent to Request Income Withholding (
            // NOIIWON).  The document
            // will be e-mailed to the legal service provider.
            // *************************************************************************************
            local.LegalAction.ActionTaken = "NOIIWON";
            local.LegalAction.Classification = "I";
            local.LegalAction.CourtCaseNumber =
              entities.Distinct.CourtCaseNumber;
            local.LegalAction.EstablishmentCode = "CS";
            local.LegalAction.OrderAuthority = "";
            local.LegalAction.PaymentLocation = "";
            local.LegalAction.StandardNumber = local.Jclass.StandardNumber ?? ""
              ;
            local.LegalAction.Type1 = "";
            local.LegalAction.SystemGenInd = "Y";
            local.FirstLegalAction.Flag = "N";

            // -- This creates the legal action, legal action assignment, and 
            // legal roles.
            UseAddLegalAction();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            // -- This creates the infrastructure record.
            UseLeLactRaiseInfrastrEvents();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            // -- This creates the court caption.
            UseLeCreateDuplicatCourtCaption();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            local.LegalActionDetail.Assign(local.NullLegalActionDetail);
            MoveLegalActionDetail3(local.NullLegalActionDetail,
              local.SameFrequency);

            // *************************************************************************************
            // Create the Withholding Current (WC) Legal Action Detail using the
            // amounts from the
            // Accruing Legal Action Details on the J class legal actions.
            // *************************************************************************************
            local.LegalActionDetail.EffectiveDate = local.NullDateWorkArea.Date;
            local.DifferentPeriods.Flag = "";

            foreach(var item1 in ReadLaDetFinancialLegalAction())
            {
              if (AsChar(local.MultiPayor.Flag) == 'Y')
              {
                if (!ReadLegalActionPerson1())
                {
                  // -- Our person is not the obligor for this detail.  Skip 
                  // this detail.
                  continue;
                }
              }

              // -- 6/28/01 GVandy WR 10509 Determine if the legal action detail
              // has an obligation which displays on OCTO.
              //   Only detail amounts which have an OCTO obligation amount will
              // be used in the WC calculation.
              local.DetailObligated.Flag = "N";

              foreach(var item2 in ReadObligationObligationType())
              {
                for(local.Group.Index = 0; local.Group.Index < local
                  .Group.Count; ++local.Group.Index)
                {
                  if (Equal(local.Group.Item.CsePerson.Number,
                    import.CsePerson.Number) && local
                    .Group.Item.ObligationType.SystemGeneratedIdentifier == entities
                    .ObligationType.SystemGeneratedIdentifier && local
                    .Group.Item.Obligation.SystemGeneratedIdentifier == entities
                    .Obligation.SystemGeneratedIdentifier)
                  {
                    local.DetailObligated.Flag = "Y";

                    goto ReadEach1;
                  }
                }
              }

ReadEach1:

              if (AsChar(local.DetailObligated.Flag) == 'N')
              {
                // -- The legal action detail has not been obligated.  Skip this
                // detail.
                continue;
              }

              if (!Equal(entities.LaDetFinancial.FreqPeriodCode,
                local.Previous.FreqPeriodCode) && !
                IsEmpty(local.Previous.FreqPeriodCode) && !
                IsEmpty(entities.LaDetFinancial.FreqPeriodCode))
              {
                local.DifferentPeriods.Flag = "Y";
              }

              MoveLegalActionDetail4(entities.LaDetFinancial, local.Previous);
              local.SameFrequency.CurrentAmount =
                local.SameFrequency.CurrentAmount.GetValueOrDefault() + entities
                .LaDetFinancial.CurrentAmount.GetValueOrDefault();

              // -- Convert to a yearly amount.
              switch(TrimEnd(entities.LaDetFinancial.FreqPeriodCode))
              {
                case "":
                  break;
                case "BW":
                  // -- Bi-weekly (every other week)
                  local.LegalActionDetail.CurrentAmount =
                    local.LegalActionDetail.CurrentAmount.GetValueOrDefault() +
                    entities
                    .LaDetFinancial.CurrentAmount.GetValueOrDefault() * 26;

                  break;
                case "M":
                  // -- Monthly
                  local.LegalActionDetail.CurrentAmount =
                    local.LegalActionDetail.CurrentAmount.GetValueOrDefault() +
                    entities
                    .LaDetFinancial.CurrentAmount.GetValueOrDefault() * 12;

                  break;
                case "SM":
                  // -- Semi-Monthly (twice per month)
                  local.LegalActionDetail.CurrentAmount =
                    local.LegalActionDetail.CurrentAmount.GetValueOrDefault() +
                    entities
                    .LaDetFinancial.CurrentAmount.GetValueOrDefault() * 24;

                  break;
                case "W":
                  // -- Weekly
                  local.LegalActionDetail.CurrentAmount =
                    local.LegalActionDetail.CurrentAmount.GetValueOrDefault() +
                    entities
                    .LaDetFinancial.CurrentAmount.GetValueOrDefault() * 52;

                  break;
                default:
                  break;
              }

              if ((Lt(
                entities.LaDetFinancial.EffectiveDate,
                local.LegalActionDetail.EffectiveDate) || Equal
                (local.LegalActionDetail.EffectiveDate,
                local.NullDateWorkArea.Date)) && !
                Equal(entities.LaDetFinancial.EffectiveDate,
                local.NullDateWorkArea.Date))
              {
                local.LegalActionDetail.EffectiveDate =
                  entities.LaDetFinancial.EffectiveDate;
              }
            }

            // -- Convert back to a monthly amount.
            local.LegalActionDetail.CurrentAmount =
              Math.Round(
                local.LegalActionDetail.CurrentAmount.GetValueOrDefault() /
              12, 2, MidpointRounding.AwayFromZero);

            if (local.LegalActionDetail.CurrentAmount.GetValueOrDefault() > 0)
            {
              // -- If all the accruing legal action details have the same 
              // frequency then create the WC detail using that frequency.
              // Otherwise, create the WC detail using a monthly frequency.
              if (AsChar(local.DifferentPeriods.Flag) == 'Y')
              {
                local.LegalActionDetail.DayOfMonth1 = 1;
                local.LegalActionDetail.FreqPeriodCode = "M";
              }
              else
              {
                local.LegalActionDetail.DayOfMonth1 =
                  local.Previous.DayOfMonth1.GetValueOrDefault();
                local.LegalActionDetail.DayOfMonth2 =
                  local.Previous.DayOfMonth2.GetValueOrDefault();
                local.LegalActionDetail.DayOfWeek =
                  local.Previous.DayOfWeek.GetValueOrDefault();
                local.LegalActionDetail.PeriodInd =
                  local.Previous.PeriodInd ?? "";
                local.LegalActionDetail.FreqPeriodCode =
                  local.Previous.FreqPeriodCode ?? "";
                local.LegalActionDetail.CurrentAmount =
                  local.SameFrequency.CurrentAmount.GetValueOrDefault();
              }

              local.ObligationType.Code = "WC";
              local.LegalActionDetail.DetailType = "F";
              local.LegalActionDetail.Description =
                "Created by automatic IWO process.";

              if (Equal(local.LegalActionDetail.EffectiveDate,
                local.NullDateWorkArea.Date))
              {
                local.LegalActionDetail.EffectiveDate = local.Jclass.FiledDate;
              }

              UseLeCreateLegalActionDetail();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }

              local.LegalActionPerson.Assign(local.NullLegalActionPerson);
              local.LegalActionPerson.AccountType = "R";
              local.LegalActionPerson.EffectiveDate =
                local.LegalActionDetail.EffectiveDate;
              local.LegalActionPerson.EndDate = local.LegalActionDetail.EndDate;
              UseLeLopsCreateLegActPers2();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }

              if (!ReadLegalActionPerson2())
              {
                ExitState = "LEGAL_ACTION_PERSON_NF";

                return;
              }

              foreach(var item1 in ReadLegalActionCaseRole2())
              {
                try
                {
                  CreateLaPersonLaCaseRole2();
                }
                catch(Exception e)
                {
                  switch(GetErrorCode(e))
                  {
                    case ErrorCode.AlreadyExists:
                      ExitState = "LA_PERSON_LA_CASE_ROLE_AE";

                      return;
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

            MoveLegalActionDetail2(local.LegalActionDetail, local.Wc);
            local.LegalActionDetail.Assign(local.NullLegalActionDetail);

            // *************************************************************************************
            // Create the Withholding Arrears (WA) Legal Action Detail using the
            // current
            // debt arrears amounts from the Non-Accruing and Medical 
            // Obligations.
            // *************************************************************************************
            local.LegalActionDetail.EffectiveDate = local.Wc.EffectiveDate;

            for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
              local.Group.Index)
            {
              if (!Equal(local.Group.Item.CsePerson.Number,
                import.CsePerson.Number))
              {
                continue;
              }

              if (AsChar(local.Group.Item.ObligationType.Classification) != 'M'
                && AsChar(local.Group.Item.ObligationType.Classification) != 'N'
                && AsChar(local.Group.Item.ObligationType.Classification) != 'A'
                )
              {
                continue;
              }

              local.LegalActionDetail.ArrearsAmount =
                local.LegalActionDetail.ArrearsAmount.GetValueOrDefault() + local
                .Group.Item.ScreenOwedAmounts.ArrearsAmountOwed;
            }

            if (local.LegalActionDetail.ArrearsAmount.GetValueOrDefault() >= 25)
            {
              if (local.Wc.CurrentAmount.GetValueOrDefault() * 0.3M > local
                .LegalActionDetail.ArrearsAmount.GetValueOrDefault() / 60)
              {
                local.LegalActionDetail.ArrearsAmount =
                  Math.Round(
                    local.Wc.CurrentAmount.GetValueOrDefault() *
                  0.3M, 2, MidpointRounding.AwayFromZero);
              }
              else
              {
                local.LegalActionDetail.ArrearsAmount =
                  Math.Round(
                    local.LegalActionDetail.ArrearsAmount.GetValueOrDefault() /
                  60, 2, MidpointRounding.AwayFromZero);
              }

              if (local.LegalActionDetail.ArrearsAmount.GetValueOrDefault() < 25
                )
              {
                local.LegalActionDetail.ArrearsAmount = 25M;
              }

              local.ObligationType.Code = "WA";

              if (Equal(local.LegalActionDetail.EffectiveDate,
                local.NullDateWorkArea.Date))
              {
                local.LegalActionDetail.EffectiveDate = local.Jclass.FiledDate;
              }

              local.LegalActionDetail.DayOfMonth1 = 1;
              local.LegalActionDetail.DetailType = "F";
              local.LegalActionDetail.FreqPeriodCode = "M";
              local.LegalActionDetail.Description =
                "Created by automatic IWO process.";
              UseLeCreateLegalActionDetail();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }

              local.LegalActionPerson.Assign(local.NullLegalActionPerson);
              local.LegalActionPerson.AccountType = "R";
              local.LegalActionPerson.EffectiveDate =
                local.LegalActionDetail.EffectiveDate;
              local.LegalActionPerson.EndDate = local.LegalActionDetail.EndDate;
              UseLeLopsCreateLegActPers2();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }

              if (!ReadLegalActionPerson2())
              {
                ExitState = "LEGAL_ACTION_PERSON_NF";

                return;
              }

              foreach(var item1 in ReadLegalActionCaseRole2())
              {
                try
                {
                  CreateLaPersonLaCaseRole2();
                }
                catch(Exception e)
                {
                  switch(GetErrorCode(e))
                  {
                    case ErrorCode.AlreadyExists:
                      ExitState = "LA_PERSON_LA_CASE_ROLE_AE";

                      return;
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

            // *************************************************************************************
            // Create the NOIIWON document trigger (for e-mailing).
            // *************************************************************************************
            local.SpDocKey.KeyCase = "";

            if (ReadCase1())
            {
              // 06/13/01  GVandy    PR 121482	Pass the related case # to the 
              // document trigger cab.
              local.SpDocKey.KeyCase = entities.Case1.Number;
            }

            local.SpDocKey.KeyAp = import.CsePerson.Number;
            local.SpDocKey.KeyLegalAction = local.LegalAction.Identifier;
            local.SpDocKey.KeyIncomeSource = import.IncomeSource.Identifier;
            local.Document.Name = "NOIIWONA";
            UseSpCreateDocumentInfrastruct2();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              if (IsExitState("OFFICE_NF"))
              {
                // -- This cab set exit state office_nf if an RSP legal action 
                // assignmnent is not found.
                // Reset the exit state to a more meaningful message.
                ExitState = "LEGAL_ACTION_ASSIGNMENT_NF";
              }

              return;
            }

            local.ToBeCreated.Command = "";

            break;
          case "ORDIWO2":
            // *************************************************************************************
            // Generate the Order for Income Withholding.  The document will be 
            // mailed to the employer.
            // *************************************************************************************
            // -- 06/28/01  GVandy  WR 10509 Always create a new legal action 
            // for each automatic ORDIWO2A created.
            local.LegalAction.ActionTaken = "ORDIWO2";
            local.LegalAction.Classification = "I";
            local.LegalAction.CourtCaseNumber =
              entities.Distinct.CourtCaseNumber;
            local.LegalAction.EstablishmentCode = "CS";
            local.LegalAction.OrderAuthority = "J";
            local.LegalAction.PaymentLocation = "PAYCTR";
            local.LegalAction.StandardNumber = local.Jclass.StandardNumber ?? ""
              ;
            local.LegalAction.Type1 = "P";
            local.LegalAction.SystemGenInd = "Y";
            local.FirstLegalAction.Flag = "N";

            // -- This creates the legal action, legal action assignment, and 
            // legal roles.
            UseAddLegalAction();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            if (!ReadLegalAction4())
            {
              ExitState = "LEGAL_ACTION_NF";

              return;
            }

            // -- This creates the infrastructure record.
            UseLeLactRaiseInfrastrEvents();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            // -- This creates the court caption.
            UseLeCreateDuplicatCourtCaption();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            // *************************************************************************************
            // Copy the Withholding Current (WC) Legal Action Detail, 
            // Withholding Arrears (WA) Legal
            // Action Detail, and associated LOPS information from the most 
            // current IWO, IWOMODO,
            // IWONOTKS or IWONOTKM legal action.
            // *************************************************************************************
            foreach(var item1 in ReadLegalActionDetailObligationType())
            {
              local.LegalActionDetail.Assign(local.NullLegalActionDetail);
              local.LegalActionDetail.Assign(entities.IclassLegalActionDetail);
              local.ObligationType.Code = entities.ObligationType.Code;
              local.LegalActionDetail.DayOfMonth1 = 1;
              local.LegalActionDetail.DayOfMonth2 = 0;
              local.LegalActionDetail.DayOfWeek = 0;
              local.LegalActionDetail.PeriodInd = "";
              local.LegalActionDetail.FreqPeriodCode = "M";

              // -- Convert to a monthly amount.
              switch(TrimEnd(entities.IclassLegalActionDetail.FreqPeriodCode))
              {
                case "":
                  break;
                case "BW":
                  // -- Bi-weekly (every other week)
                  local.LegalActionDetail.CurrentAmount =
                    Math.Round(
                      entities.IclassLegalActionDetail.CurrentAmount.
                      GetValueOrDefault() * 26
                    / 12, 2, MidpointRounding.AwayFromZero);
                  local.LegalActionDetail.ArrearsAmount =
                    Math.Round(
                      entities.IclassLegalActionDetail.ArrearsAmount.
                      GetValueOrDefault() * 26
                    / 12, 2, MidpointRounding.AwayFromZero);

                  break;
                case "M":
                  // -- Monthly
                  local.LegalActionDetail.CurrentAmount =
                    entities.IclassLegalActionDetail.CurrentAmount;
                  local.LegalActionDetail.ArrearsAmount =
                    entities.IclassLegalActionDetail.ArrearsAmount;

                  break;
                case "SM":
                  // -- Semi-Monthly (twice per month)
                  local.LegalActionDetail.CurrentAmount =
                    entities.IclassLegalActionDetail.CurrentAmount.
                      GetValueOrDefault() * 2;
                  local.LegalActionDetail.ArrearsAmount =
                    entities.IclassLegalActionDetail.ArrearsAmount.
                      GetValueOrDefault() * 2;

                  break;
                case "W":
                  // -- Weekly
                  local.LegalActionDetail.CurrentAmount =
                    Math.Round(
                      entities.IclassLegalActionDetail.CurrentAmount.
                      GetValueOrDefault() * 52
                    / 12, 2, MidpointRounding.AwayFromZero);
                  local.LegalActionDetail.ArrearsAmount =
                    Math.Round(
                      entities.IclassLegalActionDetail.ArrearsAmount.
                      GetValueOrDefault() * 52
                    / 12, 2, MidpointRounding.AwayFromZero);

                  break;
                default:
                  break;
              }

              local.LegalActionDetail.Description =
                "Created by automatic IWO process.";
              UseLeCreateLegalActionDetail();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }

              foreach(var item2 in ReadLegalActionPersonCsePerson())
              {
                local.LegalActionPerson.Assign(
                  entities.CopyThisLegalActionPerson);

                switch(TrimEnd(entities.IclassLegalActionDetail.FreqPeriodCode))
                {
                  case "":
                    break;
                  case "BW":
                    // -- Bi-weekly (every other week)
                    local.LegalActionPerson.CurrentAmount =
                      Math.Round(
                        entities.CopyThisLegalActionPerson.CurrentAmount.
                        GetValueOrDefault() * 26
                      / 12, 2, MidpointRounding.AwayFromZero);
                    local.LegalActionPerson.ArrearsAmount =
                      Math.Round(
                        entities.CopyThisLegalActionPerson.ArrearsAmount.
                        GetValueOrDefault() * 26
                      / 12, 2, MidpointRounding.AwayFromZero);

                    break;
                  case "M":
                    // -- Monthly
                    local.LegalActionPerson.CurrentAmount =
                      entities.CopyThisLegalActionPerson.CurrentAmount;
                    local.LegalActionPerson.ArrearsAmount =
                      entities.CopyThisLegalActionPerson.ArrearsAmount;

                    break;
                  case "SM":
                    // -- Semi-Monthly (twice per month)
                    local.LegalActionPerson.CurrentAmount =
                      entities.CopyThisLegalActionPerson.CurrentAmount.
                        GetValueOrDefault() * 2;
                    local.LegalActionPerson.ArrearsAmount =
                      entities.CopyThisLegalActionPerson.ArrearsAmount.
                        GetValueOrDefault() * 2;

                    break;
                  case "W":
                    // -- Weekly
                    local.LegalActionPerson.CurrentAmount =
                      Math.Round(
                        entities.CopyThisLegalActionPerson.CurrentAmount.
                        GetValueOrDefault() * 52
                      / 12, 2, MidpointRounding.AwayFromZero);
                    local.LegalActionPerson.ArrearsAmount =
                      Math.Round(
                        entities.CopyThisLegalActionPerson.ArrearsAmount.
                        GetValueOrDefault() * 52
                      / 12, 2, MidpointRounding.AwayFromZero);

                    break;
                  default:
                    break;
                }

                UseLeLopsCreateLegActPers1();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  return;
                }

                if (!ReadLegalActionPerson2())
                {
                  ExitState = "LEGAL_ACTION_PERSON_NF";

                  return;
                }

                foreach(var item3 in ReadLaPersonLaCaseRoleLegalActionCaseRoleCaseRole())
                  
                {
                  // Read for a legal_action_case_role associating the new 
                  // ORDIWO2 legal action to the current case_role.  If not
                  // found then create one.
                  if (!ReadLegalActionCaseRole1())
                  {
                    try
                    {
                      CreateLegalActionCaseRole();
                    }
                    catch(Exception e)
                    {
                      switch(GetErrorCode(e))
                      {
                        case ErrorCode.AlreadyExists:
                          ExitState = "LE0000_LEGAL_ACTION_CASE_ROLE_AE";

                          return;
                        case ErrorCode.PermittedValueViolation:
                          break;
                        case ErrorCode.DatabaseError:
                          break;
                        default:
                          throw;
                      }
                    }
                  }

                  try
                  {
                    CreateLaPersonLaCaseRole1();
                  }
                  catch(Exception e)
                  {
                    switch(GetErrorCode(e))
                    {
                      case ErrorCode.AlreadyExists:
                        ExitState = "LA_PERSON_LA_CASE_ROLE_AE";

                        return;
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
            }

            // *************************************************************************************
            // Close any active IWGLs for the court case number and employer.
            // *************************************************************************************
            // --08/02/18 GVandy CQ61457  Update SVES and 'O' type employer to 
            // work with eIWO for
            //   SSA.  SA is now employer based.  WC is not.
            if (AsChar(local.IncomeSource.Type1) == 'O' && Equal
              (local.IncomeSource.Code, "WC"))
            {
              // *************************************************************************************
              // Skip employer record retrieval for Type 'O'ther and Code is '
              // WC' (i.e. Workers comp).
              // *************************************************************************************
            }
            else if (ReadEmployer1())
            {
              foreach(var item1 in ReadLegalActionIncomeSource())
              {
                try
                {
                  UpdateLegalActionIncomeSource();
                }
                catch(Exception e)
                {
                  switch(GetErrorCode(e))
                  {
                    case ErrorCode.AlreadyExists:
                      ExitState = "LEGAL_ACTION_PERSON_RESOURCE_NU";

                      return;
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
              ExitState = "EMPLOYER_NF";

              return;
            }

            // *************************************************************************************
            // Create the new IWGL (Legal_action_income_source) record.
            // *************************************************************************************
            local.LegalActionIncomeSource.EffectiveDate = local.Today.Date;
            local.LegalActionIncomeSource.OrderType = "";
            local.LegalActionIncomeSource.WithholdingType = "W";
            local.IwglInd.Text1 = "I";
            local.CsePersonsWorkSet.Number = import.CsePerson.Number;
            UseEstabLegalActionIncSource();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            // *************************************************************************************
            // Create the ORDIWO2 document trigger (for batch mailing).
            // *************************************************************************************
            local.SpDocKey.KeyCase = "";

            if (ReadCase1())
            {
              // 06/13/01  GVandy    PR 121482	Pass the related case # to the 
              // document trigger cab.
              local.SpDocKey.KeyCase = entities.Case1.Number;
            }

            local.SpDocKey.KeyAp = import.CsePerson.Number;
            local.SpDocKey.KeyLegalAction = local.LegalAction.Identifier;
            local.SpDocKey.KeyIncomeSource = import.IncomeSource.Identifier;

            if (Equal(global.UserId, "SWELB578") || Equal
              (global.UserId, "SWELB579"))
            {
              // 05/22/12 GVandy CQ 33628  Generate ORDIWO2B document for 
              // Department of Labor Unemployment Compensation IWOs.
              local.Document.Name = "ORDIWO2B";
            }
            else
            {
              local.Document.Name = "ORDIWO2A";
            }

            UseSpCreateDocumentInfrastruct1();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              if (IsExitState("OFFICE_NF"))
              {
                // -- This cab set exit state office_nf if an RSP legal action 
                // assignmnent is not found.
                // Reset the exit state to a more meaningful message.
                ExitState = "LEGAL_ACTION_ASSIGNMENT_NF";
              }

              return;
            }

            local.ToBeCreated.Command = "";

            if (AsChar(local.EiwoEmployer.Flag) == 'Y')
            {
              if (!IsEmpty(local.Eiwo.Ssn) && !
                Equal(local.Eiwo.Ssn, "000000000"))
              {
                local.Create.CreatedBy = global.UserId;
                local.Field.Name = "LAEIWO2EMP";
                local.FieldValue.Value = "Y";
                UseSpCabCreateUpdateFieldValue();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  return;
                }

                local.IwoAction.ActionType = "E-IWO";
                local.IwoAction.StatusCd = "S";
                UseLeCreateIwoTransaction();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  return;
                }
              }
              else
              {
                local.Create.CreatedBy = global.UserId;
                local.Field.Name = "LAEIWO2EMP";
                local.FieldValue.Value = "N";
                UseSpCabCreateUpdateFieldValue();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  return;
                }

                local.IwoAction.ActionType = "PRINT";
                local.IwoAction.StatusCd = "P";
                UseLeCreateIwoTransaction();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  return;
                }

                local.Infrastructure.ReasonCode = "EIWONOTCOMPLETE";

                if (ReadEmployer1())
                {
                  local.Infrastructure.Detail =
                    "No NCP SSN, paper copy triggered to: " + TrimEnd
                    (entities.Employer.Name);
                }
                else
                {
                  local.Infrastructure.Detail =
                    "E-IWO WAS NOT COMPLETED DUE TO NO SS# IN THE SYSTEM.  PAPER COPY TRIGGERED.";
                    
                }

                local.Infrastructure.EventId = 51;
                local.Infrastructure.CsenetInOutCode = "";
                local.Infrastructure.ProcessStatus = "Q";
                local.Infrastructure.UserId = "INCS";
                local.Infrastructure.BusinessObjectCd = "LEA";
                local.Infrastructure.ReferenceDate = local.Today.Date;
                local.Infrastructure.CreatedBy = global.UserId;
                local.Infrastructure.CreatedTimestamp = Now();
                local.Infrastructure.DenormNumeric12 =
                  entities.LegalAction.Identifier;
                local.Infrastructure.DenormText12 =
                  entities.LegalAction.CourtCaseNumber;

                if (!IsEmpty(entities.LegalAction.InitiatingState) && !
                  Equal(entities.LegalAction.InitiatingState, "KS"))
                {
                  local.Infrastructure.InitiatingStateCode = "OS";
                }
                else
                {
                  local.Infrastructure.InitiatingStateCode = "KS";
                }

                local.Infrastructure.CsePersonNumber = import.CsePerson.Number;

                // dqd
                foreach(var item1 in ReadCase3())
                {
                  local.Infrastructure.CaseNumber = entities.Case1.Number;
                  UseSpCabCreateInfrastructure();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    return;
                  }
                }
              }
            }
            else
            {
              // EMPLOYER IS NOT E-IWO
              local.Create.CreatedBy = global.UserId;
              local.Field.Name = "LAEIWO2EMP";
              local.FieldValue.Value = "N";
              UseSpCabCreateUpdateFieldValue();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }

              local.IwoAction.ActionType = "PRINT";
              local.IwoAction.StatusCd = "P";
              UseLeCreateIwoTransaction();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }
            }

            break;
          default:
            break;
        }
      }

      // *********************************************************************
      // Start MWO Processing
      // *********************************************************************
      if (AsChar(local.ContinueMwoProcessing.Flag) == 'Y')
      {
        // *********************************************************************
        // CQ25585 T. Pierce       5/2011  Check code table designed to "turn 
        // off"
        // automatic MWO generation.  If the code value "Y" is found on the code
        // value table, then the local_continue_mwo_processing value will return
        // as "Y".  Otherwise, the value will be returned "N" and processing 
        // will
        // not continue further based on "IF" statements.
        // *********************************************************************
        UseCabValidateCodeValue2();

        // *************************************************************************************
        // CQ25585 T. Pierce       05/2011 End changes.
        // *************************************************************************************
      }

      // We will escape from inside the IF once we determine what action to 
      // take.
      if (IsEmpty(local.ToBeCreated.Command) && AsChar
        (local.ContinueMwoProcessing.Flag) == 'Y' && AsChar
        (local.ValidMwoIncomeSource.Flag) == 'Y' && AsChar
        (local.StopProcessingThisCase.Flag) == 'N')
      {
        // Set command to ALERT and Escape if a problem is found, otherwise set 
        // to MWONOTHC
        local.IclassLegalAction.FiledDate = local.NullDateWorkArea.Date;

        // Get most recent active MWO.
        if (ReadLegalAction3())
        {
          local.IclassLegalAction.FiledDate =
            entities.IclassLegalAction.FiledDate;
        }

        if (Equal(local.IclassLegalAction.FiledDate, local.NullDateWorkArea.Date))
          
        {
          local.ToBeCreated.Command = "";

          goto Test4;
        }

        // Check to see if a TERMMWOO exists.
        if (ReadLegalAction2())
        {
          local.ToBeCreated.Command = "";

          goto Test4;
        }

        // ********************************************************************************************************
        // Read each supported person on the HIC legal detail and check:
        // 1.  Child has active health insurance coverage.
        // 2.  Child is emancipated.
        // 3.  Health insurance viability is set to N.
        // If ANY of these things are true for ALL supported persons, then the 
        // document will not be generated.
        // ********************************************************************************************************
        local.MwoHicFoundForAllSp.Flag = "Y";
        local.MwoAllSpAreEmancipated.Flag = "Y";
        local.MwoHiViabIndIsN.Flag = "Y";

        foreach(var item1 in ReadCaseRole())
        {
          // Check to see if child has active health insurance coverage.
          if (AsChar(local.MwoHicFoundForAllSp.Flag) == 'Y')
          {
            if (ReadPersonalHealthInsurance())
            {
              // Child has an active health insurance policy.
            }
            else
            {
              // Child does not have an active health insurance policy.
              local.MwoHicFoundForAllSp.Flag = "N";
            }
          }

          // Check to see if child is emancipated.
          if (AsChar(local.MwoAllSpAreEmancipated.Flag) == 'Y')
          {
            if (Lt(local.NullDateWorkArea.Date,
              entities.CaseRole.DateOfEmancipation) && !
              Lt(local.Today.Date, entities.CaseRole.DateOfEmancipation))
            {
              // Child is emancipated.
            }
            else
            {
              // Child is not emancipated.
              local.MwoAllSpAreEmancipated.Flag = "N";
            }
          }

          // Check to see if health insurance viability indicator is set to N 
          // for this child.
          if (AsChar(local.MwoHiViabIndIsN.Flag) == 'Y')
          {
            if (ReadHealthInsuranceViability())
            {
              // Health insurance viability indicator is set to N.
            }
            else
            {
              // Health insurance viability indicator is not set to N.
              local.MwoHiViabIndIsN.Flag = "N";
            }
          }

          if (AsChar(local.MwoHicFoundForAllSp.Flag) == 'N' && AsChar
            (local.MwoAllSpAreEmancipated.Flag) == 'N' && AsChar
            (local.MwoHiViabIndIsN.Flag) == 'N')
          {
            break;
          }
        }

        // If all supported persons have active HIC, skip to next record.
        if (AsChar(local.MwoHicFoundForAllSp.Flag) == 'Y')
        {
          goto Test4;
        }

        // All supported persons are emancipated
        if (AsChar(local.MwoAllSpAreEmancipated.Flag) == 'Y')
        {
          goto Test4;
        }

        // Health insurance viability indicator is set to N for all supported 
        // persons.
        if (AsChar(local.MwoHiViabIndIsN.Flag) == 'Y')
        {
          goto Test4;
        }

        local.ToBeCreated.Command = "MWONOTHC";
      }

Test4:

      if (AsChar(local.ContinueMwoProcessing.Flag) == 'Y' && AsChar
        (local.ValidMwoIncomeSource.Flag) == 'Y')
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
              local.Infrastructure.ReasonCode = "NOAUTOMWOCASE";
            }
            else
            {
              // -- Send the alert to the legal service provider.
              local.Infrastructure.ReasonCode = "NOAUTOMWO";
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

            foreach(var item1 in ReadCase4())
            {
              local.Infrastructure.CaseNumber = entities.Case1.Number;
              UseSpCabCreateInfrastructure();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }
            }

            break;
          case "MWONOTHC":
            // Find additional information and create document trigger here.
            local.LegalAction.ActionTaken = "MWONOTHC";
            local.LegalAction.Classification = "I";
            local.LegalAction.CourtCaseNumber =
              entities.Distinct.CourtCaseNumber;
            local.LegalAction.EstablishmentCode = "CS";
            local.LegalAction.OrderAuthority = "";
            local.LegalAction.PaymentLocation = "";
            local.LegalAction.StandardNumber = local.Mwo.StandardNumber ?? "";
            local.LegalAction.Type1 = "";
            local.LegalAction.SystemGenInd = "Y";
            local.FirstLegalAction.Flag = "N";

            // -- This creates the legal action, legal action assignment, and 
            // legal roles.
            UseAddLegalAction();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            // -- This creates the infrastructure record.
            UseLeLactRaiseInfrastrEvents();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            // -- This creates the court caption.
            UseLeCreateDuplicatCourtCaption();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            // *************************************************************************************
            // Create the MWONOHCA document trigger (for batch mailing).
            // *************************************************************************************
            local.SpDocKey.KeyCase = "";

            if (ReadCase1())
            {
              // 06/13/01  GVandy    PR 121482	Pass the related case # to the 
              // document trigger cab.
              local.SpDocKey.KeyCase = entities.Case1.Number;
            }

            local.SpDocKey.KeyAp = import.CsePerson.Number;
            local.SpDocKey.KeyLegalAction = local.LegalAction.Identifier;
            local.SpDocKey.KeyIncomeSource = import.IncomeSource.Identifier;
            local.Document.Name = "MWONOHCA";
            UseSpCreateDocumentInfrastruct2();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              if (IsExitState("OFFICE_NF"))
              {
                // -- This cab set exit state office_nf if an RSP legal action 
                // assignmnent is not found.
                // Reset the exit state to a more meaningful message.
                ExitState = "LEGAL_ACTION_ASSIGNMENT_NF";
              }

              return;
            }

            break;
          default:
            break;
        }
      }

ReadEach2:
      ;
    }
  }

  private static void MoveCsePersonsWorkSet1(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Ssn = source.Ssn;
  }

  private static void MoveCsePersonsWorkSet2(CsePersonsWorkSet source,
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
    MoveCsePersonsWorkSet2(source.CsePersonsWorkSet, target.CsePersonsWorkSet);
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

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveIwoAction(IwoAction source, IwoAction target)
  {
    target.ActionType = source.ActionType;
    target.StatusCd = source.StatusCd;
  }

  private static void MoveLegalAction1(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.Classification = source.Classification;
    target.ActionTaken = source.ActionTaken;
    target.Type1 = source.Type1;
    target.OrderAuthority = source.OrderAuthority;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.StandardNumber = source.StandardNumber;
    target.PaymentLocation = source.PaymentLocation;
    target.EstablishmentCode = source.EstablishmentCode;
    target.CtOrdAltBillingAddrInd = source.CtOrdAltBillingAddrInd;
    target.SystemGenInd = source.SystemGenInd;
  }

  private static void MoveLegalAction2(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
  }

  private static void MoveLegalAction3(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveLegalActionDetail1(LegalActionDetail source,
    LegalActionDetail target)
  {
    target.FreqPeriodCode = source.FreqPeriodCode;
    target.DayOfWeek = source.DayOfWeek;
    target.DayOfMonth1 = source.DayOfMonth1;
    target.DayOfMonth2 = source.DayOfMonth2;
    target.PeriodInd = source.PeriodInd;
    target.Number = source.Number;
    target.DetailType = source.DetailType;
    target.EndDate = source.EndDate;
    target.EffectiveDate = source.EffectiveDate;
    target.Description = source.Description;
    target.BondAmount = source.BondAmount;
    target.Limit = source.Limit;
    target.ArrearsAmount = source.ArrearsAmount;
    target.CurrentAmount = source.CurrentAmount;
    target.JudgementAmount = source.JudgementAmount;
  }

  private static void MoveLegalActionDetail2(LegalActionDetail source,
    LegalActionDetail target)
  {
    target.DetailType = source.DetailType;
    target.EffectiveDate = source.EffectiveDate;
    target.CurrentAmount = source.CurrentAmount;
  }

  private static void MoveLegalActionDetail3(LegalActionDetail source,
    LegalActionDetail target)
  {
    target.DetailType = source.DetailType;
    target.CurrentAmount = source.CurrentAmount;
  }

  private static void MoveLegalActionDetail4(LegalActionDetail source,
    LegalActionDetail target)
  {
    target.FreqPeriodCode = source.FreqPeriodCode;
    target.DayOfWeek = source.DayOfWeek;
    target.DayOfMonth1 = source.DayOfMonth1;
    target.DayOfMonth2 = source.DayOfMonth2;
    target.PeriodInd = source.PeriodInd;
  }

  private static void MoveOfficeServiceProvider(OfficeServiceProvider source,
    OfficeServiceProvider target)
  {
    target.RoleCode = source.RoleCode;
    target.EffectiveDate = source.EffectiveDate;
  }

  private static void MoveSpDocKey(SpDocKey source, SpDocKey target)
  {
    target.KeyAdminAction = source.KeyAdminAction;
    target.KeyAdminActionCert = source.KeyAdminActionCert;
    target.KeyAdminAppeal = source.KeyAdminAppeal;
    target.KeyAp = source.KeyAp;
    target.KeyAppointment = source.KeyAppointment;
    target.KeyAr = source.KeyAr;
    target.KeyBankruptcy = source.KeyBankruptcy;
    target.KeyCase = source.KeyCase;
    target.KeyCashRcptDetail = source.KeyCashRcptDetail;
    target.KeyCashRcptEvent = source.KeyCashRcptEvent;
    target.KeyCashRcptSource = source.KeyCashRcptSource;
    target.KeyCashRcptType = source.KeyCashRcptType;
    target.KeyChild = source.KeyChild;
    target.KeyContact = source.KeyContact;
    target.KeyGeneticTest = source.KeyGeneticTest;
    target.KeyHealthInsCoverage = source.KeyHealthInsCoverage;
    target.KeyIncarceration = source.KeyIncarceration;
    target.KeyIncomeSource = source.KeyIncomeSource;
    target.KeyInfoRequest = source.KeyInfoRequest;
    target.KeyInterstateRequest = source.KeyInterstateRequest;
    target.KeyLegalAction = source.KeyLegalAction;
    target.KeyLegalActionDetail = source.KeyLegalActionDetail;
    target.KeyLegalReferral = source.KeyLegalReferral;
    target.KeyMilitaryService = source.KeyMilitaryService;
    target.KeyObligation = source.KeyObligation;
    target.KeyObligationAdminAction = source.KeyObligationAdminAction;
    target.KeyObligationType = source.KeyObligationType;
    target.KeyPerson = source.KeyPerson;
    target.KeyPersonAccount = source.KeyPersonAccount;
    target.KeyPersonAddress = source.KeyPersonAddress;
    target.KeyRecaptureRule = source.KeyRecaptureRule;
    target.KeyResource = source.KeyResource;
    target.KeyTribunal = source.KeyTribunal;
    target.KeyWorkerComp = source.KeyWorkerComp;
    target.KeyWorksheet = source.KeyWorksheet;
  }

  private void UseAddLegalAction()
  {
    var useImport = new AddLegalAction.Import();
    var useExport = new AddLegalAction.Export();

    useImport.Tribunal.Identifier = entities.Tribunal.Identifier;
    MoveLegalAction1(local.LegalAction, useImport.LegalAction);
    useImport.Import1StLegActCourtCaseNo.Flag = local.FirstLegalAction.Flag;

    Call(AddLegalAction.Execute, useImport, useExport);

    MoveLegalAction1(useExport.LegalAction, local.LegalAction);
  }

  private void UseCabValidateCodeValue1()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.AutoIwoCheckCodeValue.Cdvalue;
    useImport.Code.CodeName = local.AutoIwoCheckCode.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ContinueIwoProcessing.Flag = useExport.ValidCode.Flag;
  }

  private void UseCabValidateCodeValue2()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.AutoMwoCheckCodeValue.Cdvalue;
    useImport.Code.CodeName = local.AutoMwoCheckCode.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ContinueMwoProcessing.Flag = useExport.ValidCode.Flag;
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseEstabLegalActionIncSource()
  {
    var useImport = new EstabLegalActionIncSource.Import();
    var useExport = new EstabLegalActionIncSource.Export();

    useImport.IncomeSource.Identifier = import.IncomeSource.Identifier;
    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;
    useImport.IwglInd.Text1 = local.IwglInd.Text1;
    useImport.LegalAction.Identifier = local.LegalAction.Identifier;
    useImport.LegalActionIncomeSource.Assign(local.LegalActionIncomeSource);

    Call(EstabLegalActionIncSource.Execute, useImport, useExport);
  }

  private void UseFnDisplayObligByCourtOrder()
  {
    var useImport = new FnDisplayObligByCourtOrder.Import();
    var useExport = new FnDisplayObligByCourtOrder.Export();

    useImport.Search.Assign(local.Jclass);

    Call(FnDisplayObligByCourtOrder.Execute, useImport, useExport);

    useExport.Group.CopyTo(local.Group, MoveGroup);
  }

  private void UseLeCreateDuplicatCourtCaption()
  {
    var useImport = new LeCreateDuplicatCourtCaption.Import();
    var useExport = new LeCreateDuplicatCourtCaption.Export();

    MoveLegalAction2(local.LegalAction, useImport.Current);

    Call(LeCreateDuplicatCourtCaption.Execute, useImport, useExport);
  }

  private void UseLeCreateIwoTransaction()
  {
    var useImport = new LeCreateIwoTransaction.Import();
    var useExport = new LeCreateIwoTransaction.Export();

    useImport.CsePerson.Number = import.CsePerson.Number;
    useImport.IncomeSource.Identifier = import.IncomeSource.Identifier;
    MoveIwoAction(local.IwoAction, useImport.IwoAction);
    useImport.Infrastructure.SystemGeneratedIdentifier =
      local.Create.SystemGeneratedIdentifier;
    useImport.LegalAction.Identifier = local.LegalAction.Identifier;

    Call(LeCreateIwoTransaction.Execute, useImport, useExport);
  }

  private void UseLeCreateLegalActionDetail()
  {
    var useImport = new LeCreateLegalActionDetail.Import();
    var useExport = new LeCreateLegalActionDetail.Export();

    useImport.ObligationType.Code = local.ObligationType.Code;
    useImport.LegalAction.Identifier = local.LegalAction.Identifier;
    MoveLegalActionDetail1(local.LegalActionDetail, useImport.LegalActionDetail);
      

    Call(LeCreateLegalActionDetail.Execute, useImport, useExport);

    MoveLegalActionDetail1(useExport.LegalActionDetail, local.LegalActionDetail);
      
  }

  private void UseLeLactRaiseInfrastrEvents()
  {
    var useImport = new LeLactRaiseInfrastrEvents.Import();
    var useExport = new LeLactRaiseInfrastrEvents.Export();

    useImport.Event95ForNewLegActn.Flag = local.FirstLegalAction.Flag;
    useImport.LegalAction.Identifier = local.LegalAction.Identifier;

    Call(LeLactRaiseInfrastrEvents.Execute, useImport, useExport);
  }

  private void UseLeLopsCreateLegActPers1()
  {
    var useImport = new LeLopsCreateLegActPers.Import();
    var useExport = new LeLopsCreateLegActPers.Export();

    useImport.CsePerson.Number = entities.CopyThisCsePerson.Number;
    useImport.LegalActionPerson.Assign(local.LegalActionPerson);
    useImport.LegalActionDetail.Number = local.LegalActionDetail.Number;
    useImport.LegalAction.Identifier = local.LegalAction.Identifier;

    Call(LeLopsCreateLegActPers.Execute, useImport, useExport);

    local.LegalActionPerson.Assign(useExport.LegalActionPerson);
  }

  private void UseLeLopsCreateLegActPers2()
  {
    var useImport = new LeLopsCreateLegActPers.Import();
    var useExport = new LeLopsCreateLegActPers.Export();

    useImport.CsePerson.Number = import.CsePerson.Number;
    useImport.LegalActionPerson.Assign(local.LegalActionPerson);
    useImport.LegalActionDetail.Number = local.LegalActionDetail.Number;
    useImport.LegalAction.Identifier = local.LegalAction.Identifier;

    Call(LeLopsCreateLegActPers.Execute, useImport, useExport);

    local.LegalActionPerson.Assign(useExport.LegalActionPerson);
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.Eiwo.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet1(useExport.CsePersonsWorkSet, local.Eiwo);
    export.AbendData.Assign(useExport.AbendData);
  }

  private void UseSiReadCsePersonBatch()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.Eiwo.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

    MoveCsePersonsWorkSet1(useExport.CsePersonsWorkSet, local.Eiwo);
    export.AbendData.Assign(useExport.AbendData);
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    local.Infrastructure.Assign(useExport.Infrastructure);
  }

  private void UseSpCabCreateLegalActionAssgn()
  {
    var useImport = new SpCabCreateLegalActionAssgn.Import();
    var useExport = new SpCabCreateLegalActionAssgn.Export();

    useImport.ServiceProvider.SystemGeneratedId =
      entities.ServiceProvider.SystemGeneratedId;
    useImport.LegalAction.Identifier = entities.PreviousAssignment.Identifier;
    useImport.LegalActionAssigment.Assign(local.LegalActionAssigment);
    MoveOfficeServiceProvider(local.OfficeServiceProvider,
      useImport.OfficeServiceProvider);
    useImport.Office.SystemGeneratedId = local.Office.SystemGeneratedId;

    Call(SpCabCreateLegalActionAssgn.Execute, useImport, useExport);
  }

  private void UseSpCabCreateUpdateFieldValue()
  {
    var useImport = new SpCabCreateUpdateFieldValue.Import();
    var useExport = new SpCabCreateUpdateFieldValue.Export();

    useImport.Infrastructure.SystemGeneratedIdentifier =
      local.Create.SystemGeneratedIdentifier;
    useImport.FieldValue.Assign(local.FieldValue);
    useImport.Field.Name = local.Field.Name;

    Call(SpCabCreateUpdateFieldValue.Execute, useImport, useExport);
  }

  private void UseSpCreateDocumentInfrastruct1()
  {
    var useImport = new SpCreateDocumentInfrastruct.Import();
    var useExport = new SpCreateDocumentInfrastruct.Export();

    useImport.Document.Assign(local.Document);
    MoveSpDocKey(local.SpDocKey, useImport.SpDocKey);

    Call(SpCreateDocumentInfrastruct.Execute, useImport, useExport);

    MoveInfrastructure(useExport.Infrastructure, local.Create);
  }

  private void UseSpCreateDocumentInfrastruct2()
  {
    var useImport = new SpCreateDocumentInfrastruct.Import();
    var useExport = new SpCreateDocumentInfrastruct.Export();

    useImport.Document.Assign(local.Document);
    MoveSpDocKey(local.SpDocKey, useImport.SpDocKey);

    Call(SpCreateDocumentInfrastruct.Execute, useImport, useExport);
  }

  private void CreateLaPersonLaCaseRole1()
  {
    System.Diagnostics.Debug.Assert(entities.IwoLegalActionCaseRole.Populated);

    var identifier = entities.CopyThisLaPersonLaCaseRole.Identifier;
    var croId = entities.IwoLegalActionCaseRole.CroIdentifier;
    var croType = entities.IwoLegalActionCaseRole.CroType;
    var cspNum = entities.IwoLegalActionCaseRole.CspNumber;
    var casNum = entities.IwoLegalActionCaseRole.CasNumber;
    var lgaId = entities.IwoLegalActionCaseRole.LgaId;
    var lapId = entities.IwoLegalActionPerson.Identifier;

    CheckValid<LaPersonLaCaseRole>("CroType", croType);
    entities.IwoLaPersonLaCaseRole.Populated = false;
    Update("CreateLaPersonLaCaseRole1",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", identifier);
        db.SetInt32(command, "croId", croId);
        db.SetString(command, "croType", croType);
        db.SetString(command, "cspNum", cspNum);
        db.SetString(command, "casNum", casNum);
        db.SetInt32(command, "lgaId", lgaId);
        db.SetInt32(command, "lapId", lapId);
      });

    entities.IwoLaPersonLaCaseRole.Identifier = identifier;
    entities.IwoLaPersonLaCaseRole.CroId = croId;
    entities.IwoLaPersonLaCaseRole.CroType = croType;
    entities.IwoLaPersonLaCaseRole.CspNum = cspNum;
    entities.IwoLaPersonLaCaseRole.CasNum = casNum;
    entities.IwoLaPersonLaCaseRole.LgaId = lgaId;
    entities.IwoLaPersonLaCaseRole.LapId = lapId;
    entities.IwoLaPersonLaCaseRole.Populated = true;
  }

  private void CreateLaPersonLaCaseRole2()
  {
    System.Diagnostics.Debug.Assert(entities.IwoLegalActionCaseRole.Populated);

    var identifier = 1;
    var croId = entities.IwoLegalActionCaseRole.CroIdentifier;
    var croType = entities.IwoLegalActionCaseRole.CroType;
    var cspNum = entities.IwoLegalActionCaseRole.CspNumber;
    var casNum = entities.IwoLegalActionCaseRole.CasNumber;
    var lgaId = entities.IwoLegalActionCaseRole.LgaId;
    var lapId = entities.IwoLegalActionPerson.Identifier;

    CheckValid<LaPersonLaCaseRole>("CroType", croType);
    entities.IwoLaPersonLaCaseRole.Populated = false;
    Update("CreateLaPersonLaCaseRole2",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", identifier);
        db.SetInt32(command, "croId", croId);
        db.SetString(command, "croType", croType);
        db.SetString(command, "cspNum", cspNum);
        db.SetString(command, "casNum", casNum);
        db.SetInt32(command, "lgaId", lgaId);
        db.SetInt32(command, "lapId", lapId);
      });

    entities.IwoLaPersonLaCaseRole.Identifier = identifier;
    entities.IwoLaPersonLaCaseRole.CroId = croId;
    entities.IwoLaPersonLaCaseRole.CroType = croType;
    entities.IwoLaPersonLaCaseRole.CspNum = cspNum;
    entities.IwoLaPersonLaCaseRole.CasNum = casNum;
    entities.IwoLaPersonLaCaseRole.LgaId = lgaId;
    entities.IwoLaPersonLaCaseRole.LapId = lapId;
    entities.IwoLaPersonLaCaseRole.Populated = true;
  }

  private void CreateLegalActionCaseRole()
  {
    System.Diagnostics.Debug.Assert(entities.CaseRole.Populated);

    var casNumber = entities.CaseRole.CasNumber;
    var cspNumber = entities.CaseRole.CspNumber;
    var croType = entities.CaseRole.Type1;
    var croIdentifier = entities.CaseRole.Identifier;
    var lgaId = entities.Ordiwo2.Identifier;
    var createdBy = global.UserId;
    var createdTstamp = Now();
    var initialCreationInd = "Y";

    CheckValid<LegalActionCaseRole>("CroType", croType);
    entities.IwoLegalActionCaseRole.Populated = false;
    Update("CreateLegalActionCaseRole",
      (db, command) =>
      {
        db.SetString(command, "casNumber", casNumber);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "croType", croType);
        db.SetInt32(command, "croIdentifier", croIdentifier);
        db.SetInt32(command, "lgaId", lgaId);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetString(command, "initCrInd", initialCreationInd);
      });

    entities.IwoLegalActionCaseRole.CasNumber = casNumber;
    entities.IwoLegalActionCaseRole.CspNumber = cspNumber;
    entities.IwoLegalActionCaseRole.CroType = croType;
    entities.IwoLegalActionCaseRole.CroIdentifier = croIdentifier;
    entities.IwoLegalActionCaseRole.LgaId = lgaId;
    entities.IwoLegalActionCaseRole.CreatedBy = createdBy;
    entities.IwoLegalActionCaseRole.CreatedTstamp = createdTstamp;
    entities.IwoLegalActionCaseRole.InitialCreationInd = initialCreationInd;
    entities.IwoLegalActionCaseRole.Populated = true;
  }

  private bool ReadCase1()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Today.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetInt32(command, "lgaId", local.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;
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

  private IEnumerable<bool> ReadCase3()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase3",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Today.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetInt32(command, "lgaId", local.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCase4()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase4",
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
    entities.SpecialInterstateCaseloadCaseAssignment.Populated = false;

    return Read("ReadCaseAssignment",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Today.Date.GetValueOrDefault());
        db.SetString(
          command, "ospCode",
          import.InterstateCaseloadOfficeServiceProvider.RoleCode);
        db.SetInt32(
          command, "offId", import.InterstateCaseloadOffice.SystemGeneratedId);
        db.SetInt32(
          command, "spdId",
          import.InterstateCaseloadServiceProvider.SystemGeneratedId);
        db.SetNullableString(
          command, "courtCaseNo", entities.Distinct.CourtCaseNumber ?? "");
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.SpecialInterstateCaseloadCaseAssignment.EffectiveDate =
          db.GetDate(reader, 0);
        entities.SpecialInterstateCaseloadCaseAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 1);
        entities.SpecialInterstateCaseloadCaseAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 2);
        entities.SpecialInterstateCaseloadCaseAssignment.SpdId =
          db.GetInt32(reader, 3);
        entities.SpecialInterstateCaseloadCaseAssignment.OffId =
          db.GetInt32(reader, 4);
        entities.SpecialInterstateCaseloadCaseAssignment.OspCode =
          db.GetString(reader, 5);
        entities.SpecialInterstateCaseloadCaseAssignment.OspDate =
          db.GetDate(reader, 6);
        entities.SpecialInterstateCaseloadCaseAssignment.CasNo =
          db.GetString(reader, 7);
        entities.SpecialInterstateCaseloadCaseAssignment.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCaseRole()
  {
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCaseRole",
      (db, command) =>
      {
        db.SetNullableInt32(command, "lgaRIdentifier", local.Mwo.Identifier);
        db.SetNullableInt32(command, "ladRNumber", local.JorOClass.Number);
        db.SetNullableDate(
          command, "endDate", local.Today.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.DateOfEmancipation = db.GetNullableDate(reader, 6);
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);

        return true;
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

  private bool ReadEmployer1()
  {
    entities.Employer.Populated = false;

    return Read("ReadEmployer1",
      (db, command) =>
      {
        db.SetDateTime(
          command, "identifier",
          import.IncomeSource.Identifier.GetValueOrDefault());
        db.SetString(command, "cspINumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Employer.Identifier = db.GetInt32(reader, 0);
        entities.Employer.Name = db.GetNullableString(reader, 1);
        entities.Employer.EiwoEndDate = db.GetNullableDate(reader, 2);
        entities.Employer.EiwoStartDate = db.GetNullableDate(reader, 3);
        entities.Employer.Populated = true;
      });
  }

  private bool ReadEmployer2()
  {
    entities.Employer.Populated = false;

    return Read("ReadEmployer2",
      (db, command) =>
      {
        db.SetDateTime(
          command, "identifier",
          import.IncomeSource.Identifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Employer.Identifier = db.GetInt32(reader, 0);
        entities.Employer.Name = db.GetNullableString(reader, 1);
        entities.Employer.EiwoEndDate = db.GetNullableDate(reader, 2);
        entities.Employer.EiwoStartDate = db.GetNullableDate(reader, 3);
        entities.Employer.Populated = true;
      });
  }

  private bool ReadEmployerAddress()
  {
    entities.EmployerAddress.Populated = false;

    return Read("ReadEmployerAddress",
      (db, command) =>
      {
        db.SetString(command, "cspINumber", entities.CsePerson.Number);
        db.SetDateTime(
          command, "identifier",
          import.IncomeSource.Identifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.EmployerAddress.Identifier = db.GetDateTime(reader, 0);
        entities.EmployerAddress.EmpId = db.GetInt32(reader, 1);
        entities.EmployerAddress.Note = db.GetNullableString(reader, 2);
        entities.EmployerAddress.Populated = true;
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
        entities.Fips.CountyDescription = db.GetNullableString(reader, 3);
        entities.Fips.StateAbbreviation = db.GetString(reader, 4);
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

  private bool ReadHealthInsuranceViability()
  {
    System.Diagnostics.Debug.Assert(entities.CaseRole.Populated);
    entities.HealthInsuranceViability.Populated = false;

    return Read("ReadHealthInsuranceViability",
      (db, command) =>
      {
        db.SetInt32(command, "croIdentifier", entities.CaseRole.Identifier);
        db.SetString(command, "croType", entities.CaseRole.Type1);
        db.SetString(command, "casNumber", entities.CaseRole.CasNumber);
        db.SetString(command, "cspNumber", entities.CaseRole.CspNumber);
      },
      (db, reader) =>
      {
        entities.HealthInsuranceViability.CroType = db.GetString(reader, 0);
        entities.HealthInsuranceViability.CspNumber = db.GetString(reader, 1);
        entities.HealthInsuranceViability.CasNumber = db.GetString(reader, 2);
        entities.HealthInsuranceViability.CroIdentifier =
          db.GetInt32(reader, 3);
        entities.HealthInsuranceViability.Identifier = db.GetInt32(reader, 4);
        entities.HealthInsuranceViability.HinsViableInd =
          db.GetNullableString(reader, 5);
        entities.HealthInsuranceViability.Populated = true;
        CheckValid<HealthInsuranceViability>("CroType",
          entities.HealthInsuranceViability.CroType);
      });
  }

  private bool ReadIncomeSource()
  {
    entities.IncomeSource.Populated = false;

    return Read("ReadIncomeSource",
      (db, command) =>
      {
        db.SetString(command, "cspINumber", import.CsePerson.Number);
        db.SetDateTime(
          command, "identifier",
          import.IncomeSource.Identifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.IncomeSource.Type1 = db.GetString(reader, 1);
        entities.IncomeSource.ReturnCd = db.GetNullableString(reader, 2);
        entities.IncomeSource.Code = db.GetNullableString(reader, 3);
        entities.IncomeSource.CspINumber = db.GetString(reader, 4);
        entities.IncomeSource.EmpId = db.GetNullableInt32(reader, 5);
        entities.IncomeSource.EndDt = db.GetNullableDate(reader, 6);
        entities.IncomeSource.Populated = true;
        CheckValid<IncomeSource>("Type1", entities.IncomeSource.Type1);
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

  private IEnumerable<bool> ReadLaDetFinancialLegalAction()
  {
    entities.JorOClass.Populated = false;
    entities.LaDetFinancial.Populated = false;

    return ReadEach("ReadLaDetFinancialLegalAction",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", entities.Distinct.CourtCaseNumber ?? "");
        db.SetNullableDate(
          command, "endDt", local.Today.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "filedDt", local.NullDateWorkArea.Date.GetValueOrDefault());
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.LaDetFinancial.LgaIdentifier = db.GetInt32(reader, 0);
        entities.JorOClass.Identifier = db.GetInt32(reader, 0);
        entities.LaDetFinancial.Number = db.GetInt32(reader, 1);
        entities.LaDetFinancial.EndDate = db.GetNullableDate(reader, 2);
        entities.LaDetFinancial.EffectiveDate = db.GetDate(reader, 3);
        entities.LaDetFinancial.ArrearsAmount =
          db.GetNullableDecimal(reader, 4);
        entities.LaDetFinancial.CurrentAmount =
          db.GetNullableDecimal(reader, 5);
        entities.LaDetFinancial.JudgementAmount =
          db.GetNullableDecimal(reader, 6);
        entities.LaDetFinancial.DetailType = db.GetString(reader, 7);
        entities.LaDetFinancial.FreqPeriodCode =
          db.GetNullableString(reader, 8);
        entities.LaDetFinancial.DayOfWeek = db.GetNullableInt32(reader, 9);
        entities.LaDetFinancial.DayOfMonth1 = db.GetNullableInt32(reader, 10);
        entities.LaDetFinancial.DayOfMonth2 = db.GetNullableInt32(reader, 11);
        entities.LaDetFinancial.PeriodInd = db.GetNullableString(reader, 12);
        entities.LaDetFinancial.OtyId = db.GetNullableInt32(reader, 13);
        entities.JorOClass.Classification = db.GetString(reader, 14);
        entities.JorOClass.ActionTaken = db.GetString(reader, 15);
        entities.JorOClass.Type1 = db.GetString(reader, 16);
        entities.JorOClass.FiledDate = db.GetNullableDate(reader, 17);
        entities.JorOClass.InitiatingState = db.GetNullableString(reader, 18);
        entities.JorOClass.CourtCaseNumber = db.GetNullableString(reader, 19);
        entities.JorOClass.EndDate = db.GetNullableDate(reader, 20);
        entities.JorOClass.StandardNumber = db.GetNullableString(reader, 21);
        entities.JorOClass.CreatedTstamp = db.GetDateTime(reader, 22);
        entities.JorOClass.TrbId = db.GetNullableInt32(reader, 23);
        entities.JorOClass.Populated = true;
        entities.LaDetFinancial.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LaDetFinancial.DetailType);

        return true;
      });
  }

  private IEnumerable<bool> ReadLaPersonLaCaseRoleLegalActionCaseRoleCaseRole()
  {
    entities.Case1.Populated = false;
    entities.CaseRole.Populated = false;
    entities.CopyThisLaPersonLaCaseRole.Populated = false;
    entities.CopyThisLegalActionCaseRole.Populated = false;

    return ReadEach("ReadLaPersonLaCaseRoleLegalActionCaseRoleCaseRole",
      (db, command) =>
      {
        db.SetInt32(
          command, "lapId", entities.CopyThisLegalActionPerson.Identifier);
      },
      (db, reader) =>
      {
        entities.CopyThisLaPersonLaCaseRole.Identifier = db.GetInt32(reader, 0);
        entities.CopyThisLaPersonLaCaseRole.CroId = db.GetInt32(reader, 1);
        entities.CopyThisLegalActionCaseRole.CroIdentifier =
          db.GetInt32(reader, 1);
        entities.CaseRole.Identifier = db.GetInt32(reader, 1);
        entities.CopyThisLaPersonLaCaseRole.CroType = db.GetString(reader, 2);
        entities.CopyThisLegalActionCaseRole.CroType = db.GetString(reader, 2);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CopyThisLaPersonLaCaseRole.CspNum = db.GetString(reader, 3);
        entities.CopyThisLegalActionCaseRole.CspNumber =
          db.GetString(reader, 3);
        entities.CaseRole.CspNumber = db.GetString(reader, 3);
        entities.CopyThisLaPersonLaCaseRole.CasNum = db.GetString(reader, 4);
        entities.CopyThisLegalActionCaseRole.CasNumber =
          db.GetString(reader, 4);
        entities.CaseRole.CasNumber = db.GetString(reader, 4);
        entities.Case1.Number = db.GetString(reader, 4);
        entities.Case1.Number = db.GetString(reader, 4);
        entities.CopyThisLaPersonLaCaseRole.LgaId = db.GetInt32(reader, 5);
        entities.CopyThisLegalActionCaseRole.LgaId = db.GetInt32(reader, 5);
        entities.CopyThisLaPersonLaCaseRole.LapId = db.GetInt32(reader, 6);
        entities.CopyThisLegalActionCaseRole.InitialCreationInd =
          db.GetString(reader, 7);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 8);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 9);
        entities.CaseRole.DateOfEmancipation = db.GetNullableDate(reader, 10);
        entities.Case1.Populated = true;
        entities.CaseRole.Populated = true;
        entities.CopyThisLaPersonLaCaseRole.Populated = true;
        entities.CopyThisLegalActionCaseRole.Populated = true;
        CheckValid<LaPersonLaCaseRole>("CroType",
          entities.CopyThisLaPersonLaCaseRole.CroType);
        CheckValid<LegalActionCaseRole>("CroType",
          entities.CopyThisLegalActionCaseRole.CroType);
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);

        return true;
      });
  }

  private bool ReadLegalAction1()
  {
    entities.Filter.Populated = false;

    return Read("ReadLegalAction1",
      (db, command) =>
      {
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
        db.SetNullableString(
          command, "courtCaseNo", entities.Distinct.CourtCaseNumber ?? "");
        db.SetNullableString(
          command, "standardNo", import.Filter.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Filter.Identifier = db.GetInt32(reader, 0);
        entities.Filter.CourtCaseNumber = db.GetNullableString(reader, 1);
        entities.Filter.StandardNumber = db.GetNullableString(reader, 2);
        entities.Filter.TrbId = db.GetNullableInt32(reader, 3);
        entities.Filter.Populated = true;
      });
  }

  private bool ReadLegalAction2()
  {
    entities.IclassLegalAction.Populated = false;

    return Read("ReadLegalAction2",
      (db, command) =>
      {
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
        db.SetNullableString(
          command, "courtCaseNo", entities.Distinct.CourtCaseNumber ?? "");
        db.SetNullableDate(
          command, "filedDt",
          local.IclassLegalAction.FiledDate.GetValueOrDefault());
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
      });
  }

  private bool ReadLegalAction3()
  {
    entities.IclassLegalAction.Populated = false;

    return Read("ReadLegalAction3",
      (db, command) =>
      {
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
        db.SetNullableString(
          command, "courtCaseNo", entities.Distinct.CourtCaseNumber ?? "");
        db.SetNullableDate(
          command, "filedDt", local.Today.Date.GetValueOrDefault());
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
      });
  }

  private bool ReadLegalAction4()
  {
    entities.Ordiwo2.Populated = false;

    return Read("ReadLegalAction4",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", local.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.Ordiwo2.Identifier = db.GetInt32(reader, 0);
        entities.Ordiwo2.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalAction5()
  {
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalAction5",
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

  private IEnumerable<bool> ReadLegalAction6()
  {
    entities.IclassLegalAction.Populated = false;

    return ReadEach("ReadLegalAction6",
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

  private IEnumerable<bool> ReadLegalAction7()
  {
    entities.IclassLegalAction.Populated = false;

    return ReadEach("ReadLegalAction7",
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

  private IEnumerable<bool> ReadLegalAction8()
  {
    entities.IclassTerm.Populated = false;

    return ReadEach("ReadLegalAction8",
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

  private IEnumerable<bool> ReadLegalAction9()
  {
    entities.PreviousAssignment.Populated = false;

    return ReadEach("ReadLegalAction9",
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

  private bool ReadLegalActionCaseRole1()
  {
    System.Diagnostics.Debug.Assert(entities.CaseRole.Populated);
    entities.IwoLegalActionCaseRole.Populated = false;

    return Read("ReadLegalActionCaseRole1",
      (db, command) =>
      {
        db.SetInt32(command, "lgaId", entities.Ordiwo2.Identifier);
        db.SetInt32(command, "croIdentifier", entities.CaseRole.Identifier);
        db.SetString(command, "croType", entities.CaseRole.Type1);
        db.SetString(command, "cspNumber", entities.CaseRole.CspNumber);
        db.SetString(command, "casNumber", entities.CaseRole.CasNumber);
      },
      (db, reader) =>
      {
        entities.IwoLegalActionCaseRole.CasNumber = db.GetString(reader, 0);
        entities.IwoLegalActionCaseRole.CspNumber = db.GetString(reader, 1);
        entities.IwoLegalActionCaseRole.CroType = db.GetString(reader, 2);
        entities.IwoLegalActionCaseRole.CroIdentifier = db.GetInt32(reader, 3);
        entities.IwoLegalActionCaseRole.LgaId = db.GetInt32(reader, 4);
        entities.IwoLegalActionCaseRole.CreatedBy = db.GetString(reader, 5);
        entities.IwoLegalActionCaseRole.CreatedTstamp =
          db.GetDateTime(reader, 6);
        entities.IwoLegalActionCaseRole.InitialCreationInd =
          db.GetString(reader, 7);
        entities.IwoLegalActionCaseRole.Populated = true;
        CheckValid<LegalActionCaseRole>("CroType",
          entities.IwoLegalActionCaseRole.CroType);
      });
  }

  private IEnumerable<bool> ReadLegalActionCaseRole2()
  {
    entities.IwoLegalActionCaseRole.Populated = false;

    return ReadEach("ReadLegalActionCaseRole2",
      (db, command) =>
      {
        db.SetInt32(command, "lgaId", local.LegalAction.Identifier);
        db.SetNullableDate(
          command, "endDate", local.Today.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.IwoLegalActionCaseRole.CasNumber = db.GetString(reader, 0);
        entities.IwoLegalActionCaseRole.CspNumber = db.GetString(reader, 1);
        entities.IwoLegalActionCaseRole.CroType = db.GetString(reader, 2);
        entities.IwoLegalActionCaseRole.CroIdentifier = db.GetInt32(reader, 3);
        entities.IwoLegalActionCaseRole.LgaId = db.GetInt32(reader, 4);
        entities.IwoLegalActionCaseRole.CreatedBy = db.GetString(reader, 5);
        entities.IwoLegalActionCaseRole.CreatedTstamp =
          db.GetDateTime(reader, 6);
        entities.IwoLegalActionCaseRole.InitialCreationInd =
          db.GetString(reader, 7);
        entities.IwoLegalActionCaseRole.Populated = true;
        CheckValid<LegalActionCaseRole>("CroType",
          entities.IwoLegalActionCaseRole.CroType);

        return true;
      });
  }

  private bool ReadLegalActionDetail1()
  {
    entities.LegalActionDetail.Populated = false;

    return Read("ReadLegalActionDetail1",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
        db.SetNullableString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "endDt", local.Today.Date.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDt",
          local.NullDateWorkArea.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 2);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 4);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 5);
        entities.LegalActionDetail.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);
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

  private bool ReadLegalActionDetail4()
  {
    entities.IclassLegalActionDetail.Populated = false;

    return Read("ReadLegalActionDetail4",
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

  private IEnumerable<bool> ReadLegalActionIncomeSource()
  {
    entities.LegalActionIncomeSource.Populated = false;

    return ReadEach("ReadLegalActionIncomeSource",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "endDt", local.Today.Date.GetValueOrDefault());
        db.SetNullableInt32(command, "empId", entities.Employer.Identifier);
        db.SetNullableString(
          command, "courtCaseNo", entities.Distinct.CourtCaseNumber ?? "");
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalActionIncomeSource.CspNumber = db.GetString(reader, 0);
        entities.LegalActionIncomeSource.LgaIdentifier = db.GetInt32(reader, 1);
        entities.LegalActionIncomeSource.IsrIdentifier =
          db.GetDateTime(reader, 2);
        entities.LegalActionIncomeSource.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionIncomeSource.CreatedBy = db.GetString(reader, 4);
        entities.LegalActionIncomeSource.CreatedTstamp =
          db.GetDateTime(reader, 5);
        entities.LegalActionIncomeSource.WithholdingType =
          db.GetString(reader, 6);
        entities.LegalActionIncomeSource.EndDate =
          db.GetNullableDate(reader, 7);
        entities.LegalActionIncomeSource.WageOrNonWage =
          db.GetNullableString(reader, 8);
        entities.LegalActionIncomeSource.OrderType =
          db.GetNullableString(reader, 9);
        entities.LegalActionIncomeSource.Identifier = db.GetInt32(reader, 10);
        entities.LegalActionIncomeSource.Populated = true;

        return true;
      });
  }

  private bool ReadLegalActionPerson1()
  {
    System.Diagnostics.Debug.Assert(entities.LaDetFinancial.Populated);
    entities.CopyThisLegalActionPerson.Populated = false;

    return Read("ReadLegalActionPerson1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladRNumber", entities.LaDetFinancial.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.LaDetFinancial.LgaIdentifier);
        db.SetNullableString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CopyThisLegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.CopyThisLegalActionPerson.CspNumber =
          db.GetNullableString(reader, 1);
        entities.CopyThisLegalActionPerson.EffectiveDate =
          db.GetDate(reader, 2);
        entities.CopyThisLegalActionPerson.Role = db.GetString(reader, 3);
        entities.CopyThisLegalActionPerson.EndDate =
          db.GetNullableDate(reader, 4);
        entities.CopyThisLegalActionPerson.EndReason =
          db.GetNullableString(reader, 5);
        entities.CopyThisLegalActionPerson.CreatedTstamp =
          db.GetDateTime(reader, 6);
        entities.CopyThisLegalActionPerson.CreatedBy = db.GetString(reader, 7);
        entities.CopyThisLegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 8);
        entities.CopyThisLegalActionPerson.LadRNumber =
          db.GetNullableInt32(reader, 9);
        entities.CopyThisLegalActionPerson.AccountType =
          db.GetNullableString(reader, 10);
        entities.CopyThisLegalActionPerson.ArrearsAmount =
          db.GetNullableDecimal(reader, 11);
        entities.CopyThisLegalActionPerson.CurrentAmount =
          db.GetNullableDecimal(reader, 12);
        entities.CopyThisLegalActionPerson.JudgementAmount =
          db.GetNullableDecimal(reader, 13);
        entities.CopyThisLegalActionPerson.Populated = true;
      });
  }

  private bool ReadLegalActionPerson2()
  {
    entities.IwoLegalActionPerson.Populated = false;

    return Read("ReadLegalActionPerson2",
      (db, command) =>
      {
        db.SetInt32(command, "laPersonId", local.LegalActionPerson.Identifier);
      },
      (db, reader) =>
      {
        entities.IwoLegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.IwoLegalActionPerson.Populated = true;
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

  private bool ReadLegalActionPerson4()
  {
    entities.MultipayorLegalActionPerson.Populated = false;

    return Read("ReadLegalActionPerson4",
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

  private bool ReadLegalActionPerson5()
  {
    entities.MultipayorLegalActionPerson.Populated = false;

    return Read("ReadLegalActionPerson5",
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

  private bool ReadLegalActionPerson6()
  {
    entities.MultipayorLegalActionPerson.Populated = false;

    return Read("ReadLegalActionPerson6",
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

  private IEnumerable<bool> ReadLegalActionPersonCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.IclassLegalActionDetail.Populated);
    entities.CopyThisCsePerson.Populated = false;
    entities.CopyThisLegalActionPerson.Populated = false;

    return ReadEach("ReadLegalActionPersonCsePerson",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladRNumber", entities.IclassLegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier",
          entities.IclassLegalActionDetail.LgaIdentifier);
      },
      (db, reader) =>
      {
        entities.CopyThisLegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.CopyThisLegalActionPerson.CspNumber =
          db.GetNullableString(reader, 1);
        entities.CopyThisCsePerson.Number = db.GetString(reader, 1);
        entities.CopyThisLegalActionPerson.EffectiveDate =
          db.GetDate(reader, 2);
        entities.CopyThisLegalActionPerson.Role = db.GetString(reader, 3);
        entities.CopyThisLegalActionPerson.EndDate =
          db.GetNullableDate(reader, 4);
        entities.CopyThisLegalActionPerson.EndReason =
          db.GetNullableString(reader, 5);
        entities.CopyThisLegalActionPerson.CreatedTstamp =
          db.GetDateTime(reader, 6);
        entities.CopyThisLegalActionPerson.CreatedBy = db.GetString(reader, 7);
        entities.CopyThisLegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 8);
        entities.CopyThisLegalActionPerson.LadRNumber =
          db.GetNullableInt32(reader, 9);
        entities.CopyThisLegalActionPerson.AccountType =
          db.GetNullableString(reader, 10);
        entities.CopyThisLegalActionPerson.ArrearsAmount =
          db.GetNullableDecimal(reader, 11);
        entities.CopyThisLegalActionPerson.CurrentAmount =
          db.GetNullableDecimal(reader, 12);
        entities.CopyThisLegalActionPerson.JudgementAmount =
          db.GetNullableDecimal(reader, 13);
        entities.CopyThisCsePerson.Populated = true;
        entities.CopyThisLegalActionPerson.Populated = true;

        return true;
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

  private IEnumerable<bool> ReadObligationObligationType()
  {
    System.Diagnostics.Debug.Assert(entities.LaDetFinancial.Populated);
    entities.Obligation.Populated = false;
    entities.ObligationType.Populated = false;

    return ReadEach("ReadObligationObligationType",
      (db, command) =>
      {
        db.
          SetNullableInt32(command, "ladNumber", entities.LaDetFinancial.Number);
          
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.LaDetFinancial.LgaIdentifier);
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.Obligation.LgaIdentifier = db.GetNullableInt32(reader, 4);
        entities.Obligation.LadNumber = db.GetNullableInt32(reader, 5);
        entities.ObligationType.Code = db.GetString(reader, 6);
        entities.ObligationType.Classification = db.GetString(reader, 7);
        entities.Obligation.Populated = true;
        entities.ObligationType.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);

        return true;
      });
  }

  private bool ReadOfficeServiceProviderOffice1()
  {
    entities.Office.Populated = false;
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProviderOffice1",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetInt32(
          command, "spdGeneratedId",
          entities.ServiceProvider.SystemGeneratedId);
        db.SetString(
          command, "roleCode",
          import.CentralOffDefaultAttyOfficeServiceProvider.RoleCode);
        db.SetDate(command, "effectiveDate", date);
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
        db.SetInt32(
          command, "officeId",
          import.CentralOffDefaultAttyOffice.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 5);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 6);
        entities.Office.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadOfficeServiceProviderOffice2()
  {
    entities.Office.Populated = false;
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProviderOffice2",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetInt32(
          command, "spdGeneratedId",
          entities.ServiceProvider.SystemGeneratedId);
        db.SetDate(command, "effectiveDate", date);
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 5);
        entities.Office.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadPersonalHealthInsurance()
  {
    System.Diagnostics.Debug.Assert(entities.CaseRole.Populated);
    entities.PersonalHealthInsurance.Populated = false;

    return Read("ReadPersonalHealthInsurance",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CaseRole.CspNumber);
        db.SetNullableDate(
          command, "coverBeginDate", local.Today.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonalHealthInsurance.HcvId = db.GetInt64(reader, 0);
        entities.PersonalHealthInsurance.CspNumber = db.GetString(reader, 1);
        entities.PersonalHealthInsurance.CoverageBeginDate =
          db.GetNullableDate(reader, 2);
        entities.PersonalHealthInsurance.CoverageEndDate =
          db.GetNullableDate(reader, 3);
        entities.PersonalHealthInsurance.Populated = true;
      });
  }

  private bool ReadServiceProvider1()
  {
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "countyDescription", entities.Fips.CountyDescription ?? "");
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.UserId = db.GetString(reader, 1);
        entities.ServiceProvider.LastName = db.GetString(reader, 2);
        entities.ServiceProvider.FirstName = db.GetString(reader, 3);
        entities.ServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProvider2()
  {
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider2",
      (db, command) =>
      {
        db.SetInt32(
          command, "servicePrvderId",
          import.CentralOffDefaultAttyServiceProvider.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.UserId = db.GetString(reader, 1);
        entities.ServiceProvider.LastName = db.GetString(reader, 2);
        entities.ServiceProvider.FirstName = db.GetString(reader, 3);
        entities.ServiceProvider.Populated = true;
      });
  }

  private void UpdateLegalActionIncomeSource()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionIncomeSource.Populated);

    var endDate = AddDays(local.Today.Date, -1);

    entities.LegalActionIncomeSource.Populated = false;
    Update("UpdateLegalActionIncomeSource",
      (db, command) =>
      {
        db.SetNullableDate(command, "endDt", endDate);
        db.SetString(
          command, "cspNumber", entities.LegalActionIncomeSource.CspNumber);
        db.SetInt32(
          command, "lgaIdentifier",
          entities.LegalActionIncomeSource.LgaIdentifier);
        db.SetDateTime(
          command, "isrIdentifier",
          entities.LegalActionIncomeSource.IsrIdentifier.GetValueOrDefault());
        db.SetInt32(
          command, "identifier", entities.LegalActionIncomeSource.Identifier);
      });

    entities.LegalActionIncomeSource.EndDate = endDate;
    entities.LegalActionIncomeSource.Populated = true;
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
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
    }

    /// <summary>
    /// A value of Filter.
    /// </summary>
    [JsonPropertyName("filter")]
    public LegalAction Filter
    {
      get => filter ??= new();
      set => filter = value;
    }

    /// <summary>
    /// A value of InterstateCaseloadServiceProvider.
    /// </summary>
    [JsonPropertyName("interstateCaseloadServiceProvider")]
    public ServiceProvider InterstateCaseloadServiceProvider
    {
      get => interstateCaseloadServiceProvider ??= new();
      set => interstateCaseloadServiceProvider = value;
    }

    /// <summary>
    /// A value of InterstateCaseloadOffice.
    /// </summary>
    [JsonPropertyName("interstateCaseloadOffice")]
    public Office InterstateCaseloadOffice
    {
      get => interstateCaseloadOffice ??= new();
      set => interstateCaseloadOffice = value;
    }

    /// <summary>
    /// A value of InterstateCaseloadOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("interstateCaseloadOfficeServiceProvider")]
    public OfficeServiceProvider InterstateCaseloadOfficeServiceProvider
    {
      get => interstateCaseloadOfficeServiceProvider ??= new();
      set => interstateCaseloadOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of CentralOffDefaultAttyServiceProvider.
    /// </summary>
    [JsonPropertyName("centralOffDefaultAttyServiceProvider")]
    public ServiceProvider CentralOffDefaultAttyServiceProvider
    {
      get => centralOffDefaultAttyServiceProvider ??= new();
      set => centralOffDefaultAttyServiceProvider = value;
    }

    /// <summary>
    /// A value of CentralOffDefaultAttyOffice.
    /// </summary>
    [JsonPropertyName("centralOffDefaultAttyOffice")]
    public Office CentralOffDefaultAttyOffice
    {
      get => centralOffDefaultAttyOffice ??= new();
      set => centralOffDefaultAttyOffice = value;
    }

    /// <summary>
    /// A value of CentralOffDefaultAttyOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("centralOffDefaultAttyOfficeServiceProvider")]
    public OfficeServiceProvider CentralOffDefaultAttyOfficeServiceProvider
    {
      get => centralOffDefaultAttyOfficeServiceProvider ??= new();
      set => centralOffDefaultAttyOfficeServiceProvider = value;
    }

    private CsePerson csePerson;
    private IncomeSource incomeSource;
    private LegalAction filter;
    private ServiceProvider interstateCaseloadServiceProvider;
    private Office interstateCaseloadOffice;
    private OfficeServiceProvider interstateCaseloadOfficeServiceProvider;
    private ServiceProvider centralOffDefaultAttyServiceProvider;
    private Office centralOffDefaultAttyOffice;
    private OfficeServiceProvider centralOffDefaultAttyOfficeServiceProvider;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private AbendData abendData;
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
    /// A value of IwoAction.
    /// </summary>
    [JsonPropertyName("iwoAction")]
    public IwoAction IwoAction
    {
      get => iwoAction ??= new();
      set => iwoAction = value;
    }

    /// <summary>
    /// A value of Create.
    /// </summary>
    [JsonPropertyName("create")]
    public Infrastructure Create
    {
      get => create ??= new();
      set => create = value;
    }

    /// <summary>
    /// A value of FieldValue.
    /// </summary>
    [JsonPropertyName("fieldValue")]
    public FieldValue FieldValue
    {
      get => fieldValue ??= new();
      set => fieldValue = value;
    }

    /// <summary>
    /// A value of Field.
    /// </summary>
    [JsonPropertyName("field")]
    public Field Field
    {
      get => field ??= new();
      set => field = value;
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
    /// A value of Eiwo.
    /// </summary>
    [JsonPropertyName("eiwo")]
    public CsePersonsWorkSet Eiwo
    {
      get => eiwo ??= new();
      set => eiwo = value;
    }

    /// <summary>
    /// A value of EiwoEmployer.
    /// </summary>
    [JsonPropertyName("eiwoEmployer")]
    public Common EiwoEmployer
    {
      get => eiwoEmployer ??= new();
      set => eiwoEmployer = value;
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
    /// A value of LegalActionAssigment.
    /// </summary>
    [JsonPropertyName("legalActionAssigment")]
    public LegalActionAssigment LegalActionAssigment
    {
      get => legalActionAssigment ??= new();
      set => legalActionAssigment = value;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
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
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
    }

    private IwoAction iwoAction;
    private Infrastructure create;
    private FieldValue fieldValue;
    private Field field;
    private AbendData abendData;
    private CsePersonsWorkSet eiwo;
    private Common eiwoEmployer;
    private DateWorkArea max;
    private LegalActionAssigment legalActionAssigment;
    private OfficeServiceProvider officeServiceProvider;
    private Office office;
    private DateWorkArea compareDateFiled;
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
    private ExitStateWorkArea exitStateWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
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

    /// <summary>
    /// A value of SpecialInterstateCaseloadCaseAssignment.
    /// </summary>
    [JsonPropertyName("specialInterstateCaseloadCaseAssignment")]
    public CaseAssignment SpecialInterstateCaseloadCaseAssignment
    {
      get => specialInterstateCaseloadCaseAssignment ??= new();
      set => specialInterstateCaseloadCaseAssignment = value;
    }

    /// <summary>
    /// A value of SpecialInterateCaseload.
    /// </summary>
    [JsonPropertyName("specialInterateCaseload")]
    public OfficeServiceProvider SpecialInterateCaseload
    {
      get => specialInterateCaseload ??= new();
      set => specialInterateCaseload = value;
    }

    /// <summary>
    /// A value of SpecialInterstateCaseloadServiceProvider.
    /// </summary>
    [JsonPropertyName("specialInterstateCaseloadServiceProvider")]
    public ServiceProvider SpecialInterstateCaseloadServiceProvider
    {
      get => specialInterstateCaseloadServiceProvider ??= new();
      set => specialInterstateCaseloadServiceProvider = value;
    }

    /// <summary>
    /// A value of SpecialInterstateCaseloadOffice.
    /// </summary>
    [JsonPropertyName("specialInterstateCaseloadOffice")]
    public Office SpecialInterstateCaseloadOffice
    {
      get => specialInterstateCaseloadOffice ??= new();
      set => specialInterstateCaseloadOffice = value;
    }

    /// <summary>
    /// A value of Filter.
    /// </summary>
    [JsonPropertyName("filter")]
    public LegalAction Filter
    {
      get => filter ??= new();
      set => filter = value;
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

    private CaseUnit caseUnit;
    private Office office;
    private OfficeServiceProvider officeServiceProvider;
    private ServiceProvider serviceProvider;
    private LegalAction iclassTerm;
    private CaseAssignment specialInterstateCaseloadCaseAssignment;
    private OfficeServiceProvider specialInterateCaseload;
    private ServiceProvider specialInterstateCaseloadServiceProvider;
    private Office specialInterstateCaseloadOffice;
    private LegalAction filter;
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
  }
#endregion
}
