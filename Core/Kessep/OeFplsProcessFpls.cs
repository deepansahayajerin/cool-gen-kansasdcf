// Program: OE_FPLS_PROCESS_FPLS, ID: 372354494, model: 746.
// Short name: SWEFPLSP
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_FPLS_PROCESS_FPLS.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This is the FPLS Driver Procedure for On-line screen driven transactions.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OeFplsProcessFpls: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_FPLS_PROCESS_FPLS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeFplsProcessFpls(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeFplsProcessFpls.
  /// </summary>
  public OeFplsProcessFpls(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ******** MAINTENANCE LOG ********************
    // AUTHOR    	 DATE  	  CHG REQ# DESCRIPTION
    // unknown   	MM/DD/YY	Initial Code
    // T.O.Redmond	02/02/96	Modify for External
    // T.O.Redmond	02/08/96	Retrofit
    // T.O.Redmond	02/22/96	Allow Response Delete
    // 				Add Prompt for CSE Person
    // G.Lofton	04/10/96	Add scrolling for FPLS Request
    // T.O.Redmond	04/24/96	Modify scrolling logic to use single Prev and Next 
    // Keys. Add modifications based upon May 1994 new FPLS Format. Add new
    // codes to interact with FPLS code tables.
    // T.O.Redmond	05/29/96 ID-130 Add New FPLS attributes.
    // 				Remove Response Delete.
    // T.O.Redmond	08/13/96	Prevent access to Page 2 if no data.
    // T.O.Redmond	09/04/96	Add Pad Zeroes
    // R. Marchman	11/19/96	Add new security and next tran
    // MK              09/13/98
    // Corrected consistency check error ICCGV09W by removing
    // TARGETING export_addr_group FROM THE BEGINNING UNTIL FULL
    // from each of the three FOR EACH statements.
    // Added validation of CSE Person Number before ADD or UPDATE cabs are USED.
    // Added validation and modified MAKE statements per SME
    // request.
    // It is noted that there is a great deal of duplicated code in this PrAD 
    // and also in the OE_FPLS_DISPLAY_REQUEST CAB.  As substantial future
    // enhancements to FPLS are required it was decided not to implement any but
    // necessary changes at this time.
    // E. Lyman   02/02/99   Added new security action block.
    // E. Lyman   08/03/00   Fixed next tran.
    // V. Madhira           04/20/2001                 PR #117898
    // Case# not carried from ALOM to FPLS.
    // ********* END MAINTENANCE LOG ****************
    // ******** MAINTENANCE LOG CONTINUE ********************
    // G. Pan     10/25/07     CQ610 and CQ311
    // CQ610 & CQ350 (same issue as CQ610) - the codes added to fix FPLS flows 
    // to ADDR.  The changes for these PRs is to continue finished the changes
    // left by the WR010500.
    // FPLS screen needs to flow accuratly to ADDR when a person is play two 
    // different roles on different cases.
    // Business Rules -
    // 41.	If a case number is displaying on FPLS, flow to ADDR and display that
    // case number.
    // 42.	If a person is an AR on one case and an AP on another case and no 
    // case number is displayed on FPLS, flow to ADDR to the case where the
    // person is playing the AP role.
    // 43.	If a person is an AP on more then one case, will flow to ADDR with 
    // the lowest case number that the AP is assigned to for that RSP SP.
    // 44.	If a case number is entered and the person number but the person does
    // not play an AP role on that case, get an error message 'This person is
    // not currently playing
    // this role on this case.'
    // CQ311 - G. Pan 2/22/08 Changes was made by previous contructor for FPLS 
    // page 2 (CQ311), G. Pan finished up coding and testing. - If there is more
    // data to be seen on page 2 display
    //     "See Page 2" on the bottom of FPLS screen in yellow font.
    // 45. If the Source Agency code is VA, NPRC, SSA, MBR or VA and data is 
    // received from the FPLS batch, then display See Page 2 at the bottom of
    // the FPLS Screen and allow
    //     the PF Page 2 key to follow to the FPL2 screen and display data.
    // 46. If the Source Agency code is not VA, NPRC, SSA, MBR or VA, then no 
    // See Page 2 message on the FPLS screen and do not allow the PF Page 2 key
    // to flow to the FPL2 screen.
    // 47. Make sure all the dates show MMDDYYYY.
    // 48. Make sure all the VA Codes display the code value and code narrative 
    // (description of the code) on FPL2.
    // Fixed prod & Acc existing problem - when pressed PF9 return key data 
    // disappeared from screen - Added CASE "RETURN" at the bottom of program.
    // 10/30/2008 G. Pan CQ610 - made changes to fix acceptance test problem.
    // ********* END MAINTENANCE LOG ****************
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
    // * 12/03/2010  Raj S              CQ5577      Modified to raise FPLS 
    // request to AR/CH  *
    // *
    // 
    // role persons.                            *
    // *
    // 
    // *
    // * 09/16/2011  Raj S              CQ5577      Modified to remove the 
    // person/case based *
    // *
    // 
    // security validation.  Now the program    *
    // *
    // 
    // will check only the logged in user has   *
    // *
    // 
    // access to the screen and command.        *
    // *
    // 
    // *
    // ***************************************************************************************
    // * 12/13/13    LSS                CQ42137     Change security access to 
    // the FPLS screen
    // *
    // 
    // back to the original security to allow only
    // *
    // 
    // the assigned case worker and their
    // supervisor
    // *
    // 
    // access to view FPLS data per business
    // *
    // 
    // requirement dated 11/27/13 to be in
    // *                                            compliance with the FPLS 
    // Security Agreement.
    // ****************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);
    export.StartingCase.Number = import.StartingCase.Number;
    export.StartingCsePerson.Number = import.StartingCsePerson.Number;
    export.StartingCsePersonsWorkSet.Assign(import.StartingCsePersonsWorkSet);
    export.HiddenCsePerson.Number = import.HiddenCsePerson.Number;

    // ************************************************
    // *Put leading zeroes on Case number             *
    // ************************************************
    if (IsEmpty(import.StartingCase.Number))
    {
      export.StartingCase.Number = import.StartingCase.Number;
    }
    else
    {
      local.TextWorkArea.Text10 = import.StartingCase.Number;
      UseEabPadLeftWithZeros();
      export.StartingCase.Number = local.TextWorkArea.Text10;
    }

    // ************************************************
    // *Put leading zeroes on CSE Person              *
    // ************************************************
    if (IsEmpty(import.StartingCsePerson.Number))
    {
      export.PersonName.FormattedName = "";
      export.StartingCsePerson.Number = "";
    }
    else
    {
      local.TextWorkArea.Text10 = import.StartingCsePerson.Number;
      UseEabPadLeftWithZeros();
      export.StartingCsePerson.Number = local.TextWorkArea.Text10;
    }

    if (Equal(global.Command, "RETNAME") || Equal(global.Command, "RETCOMP"))
    {
      export.StartingCsePerson.Number = import.StartingCsePersonsWorkSet.Number;
      export.HiddenCsePerson.Number = export.StartingCsePersonsWorkSet.Number;
    }

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    if (Equal(global.Command, "SIGNOFF"))
    {
      UseScCabSignoff();

      return;
    }

    // --------------------------------------------------------------------------------
    // 3/05/08  G. Pan   CQ311
    // Moved following codes to bottom of this program and commented out here.
    // It caused data not retained on the screen.
    // -------------------------------------------------------------------------------
    MoveFplsLocateRequest2(import.FplsLocateRequest, export.FplsLocateRequest);
    export.FplsLocateResponse.Assign(import.FplsLocateResponse);
    MoveScrollingAttributes(import.ScrollingAttributes,
      export.ScrollingAttributes);
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.FplsResp.Description = import.FplsResp.Description;
    export.FplsAgency.Description = import.FplsAgency.Description;
    export.DodEligDesc.Description = import.DodEligDesc.Description;
    export.DodStatusDesc.Description = import.DodStatusDesc.Description;
    export.DodServiceDesc.Description = import.DodServiceDesc.Description;
    export.DodPayGradeDesc.Description = import.DodPayGradeDesc.Description;
    export.ErrorCodeDesc.Description = import.ErrorCodeDesc.Description;
    export.UpdatedBy.CreatedBy = import.UpdatedBy.CreatedBy;
    export.DateUpdated.Date = import.DateUpdated.Date;
    export.AddressFormatFlag.Flag = import.AddressFormatFlag.Flag;
    export.Page2Message.Text10 = import.Page2Message.Text10;
    export.SesaRespondingState.Description =
      import.SesaRespondingState.Description;

    if (!Equal(global.Command, "LIST"))
    {
      export.CsePersonPrompt.SelectChar = "";
    }
    else
    {
      export.CsePersonPrompt.SelectChar = import.CsePersonPrompt.SelectChar;
    }

    // -----------------------------------------------------------------
    // 4.10.100
    // Beginning Of Change
    // Since we are not using SESA State, all statements using SESA group are 
    // commented through-out this procedure.
    // ------------------------------------------------------------------
    // -----------------------------------------------------------------
    // 4.10.100
    // End Of Change
    // Since we are not using SESA State, all statements using SESA group are 
    // commented through-out this procedure.
    // ------------------------------------------------------------------
    // -----------------------------------------------------------------
    // PR#79011
    // Beginning Of Change
    // ------------------------------------------------------------------
    export.FplsAddr.Index = 0;
    export.FplsAddr.Clear();

    for(import.FplsAddr.Index = 0; import.FplsAddr.Index < import
      .FplsAddr.Count; ++import.FplsAddr.Index)
    {
      if (export.FplsAddr.IsFull)
      {
        break;
      }

      export.FplsAddr.Update.FplsAddrGroupDet.Text39 =
        import.FplsAddr.Item.FplsAddrGroupDet.Text39;
      export.FplsAddr.Next();
    }

    // -----------------------------------------------------------------
    // PR#79011
    // End Of Change
    // ------------------------------------------------------------------
    if (IsEmpty(import.Standard.NextTransaction))
    {
      // ************************************************
      // *If the next tran info is not equal to spaces, *
      // *this implies the user requested a next tran   *
      // *action. Now validate that action.             *
      // ************************************************
    }
    else
    {
      // ************************************************
      // *Flowing from here to Next Tran.               *
      // ************************************************
      export.HiddenNextTranInfo.CsePersonNumber =
        export.StartingCsePerson.Number;
      export.HiddenNextTranInfo.CaseNumber = export.StartingCase.Number;
      UseScCabNextTranPut();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();

      if (!IsEmpty(export.HiddenNextTranInfo.CsePersonNumber))
      {
        export.StartingCsePerson.Number =
          export.HiddenNextTranInfo.CsePersonNumber ?? Spaces(10);
      }

      export.StartingCase.Number = export.HiddenNextTranInfo.CaseNumber ?? Spaces
        (10);
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
    {
      // ************************************************
      // Added validation to test CSE Person number for spaces
      // before USING add or update cabs.
      // MK 9/14/98
      // ************************************************
      if (IsEmpty(export.StartingCsePerson.Number))
      {
        var field = GetField(export.StartingCsePerson, "number");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

        return;
      }

      local.SesaError.Count = 0;

      // -----------------------------------------------------------------
      // 4.10.100
      // Beginning Of Change
      // Since we are not using SESA State, all statements using SESA group are 
      // commented through-out this procedure.
      // ------------------------------------------------------------------
      // -----------------------------------------------------------------
      // 4.10.100
      // End Of Change
      // Since we are not using SESA State, all statements using SESA group are 
      // commented through-out this procedure.
      // ------------------------------------------------------------------
      if (local.SesaError.Count > 0)
      {
        ExitState = "ZD_ACO_NE0000_INVALID_CODE";

        return;
      }
    }

    if (Equal(global.Command, "RETFPL2"))
    {
      ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";

      return;
    }

    if (Equal(global.Command, "RETNAME") || Equal(global.Command, "RETCOMP"))
    {
      export.CsePersonPrompt.SelectChar = "";
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "LIST") || Equal(global.Command, "RETCOMP") || Equal
      (global.Command, "RETNAME") || Equal(global.Command, "RETINCS") || Equal
      (global.Command, "ADDR") || Equal(global.Command, "INCS") || Equal
      (global.Command, "PAGE_2"))
    {
      // ************************************************
      // *Security is not required on a return from Link*
      // ************************************************
    }
    else
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    if (Equal(global.Command, "RETCOMP") || Equal
      (global.Command, "RETNAME") || Equal(global.Command, "RETINCS"))
    {
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "ADD") || Equal
      (global.Command, "UPDATE") || Equal(global.Command, "NEXT") || Equal
      (global.Command, "PREV"))
    {
    }

    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "PREV") || Equal
      (global.Command, "NEXT"))
    {
      if (!Equal(export.StartingCsePerson.Number, export.HiddenCsePerson.Number))
        
      {
        export.HiddenCsePerson.Number = export.StartingCsePerson.Number;
        MoveFplsLocateRequest3(local.FplsLocateRequest, export.FplsLocateRequest);
          
        export.FplsLocateResponse.Assign(local.FplsLocateResponse);
        MoveScrollingAttributes(local.ScrollingAttributes,
          export.ScrollingAttributes);

        // -----------------------------------------------------------------
        // Per PR# 117897, the following code commented. The Case# must be 
        // displayed on the screen when user come from ALOM by entering CASE#
        // and CSE Person#.
        //                                              
        // ---Vithal Madhira(04/20/2001)
        // ------------------------------------------------------------------
        // -----------------------------------------------------------------
        // 4.10.100
        // Beginning Of Change
        // Since we are not using SESA State, all statements using SESA group 
        // are commented through-out this procedure.
        // ------------------------------------------------------------------
        // -----------------------------------------------------------------
        // 4.10.100
        // End Of Change
        // Since we are not using SESA State, all statements using SESA group 
        // are commented through-out this procedure.
        // ------------------------------------------------------------------
        // -----------------------------------------------------------------
        // PR#79011
        // Beginning Of Change
        // ------------------------------------------------------------------
        export.FplsAddr.Index = 0;
        export.FplsAddr.Clear();

        for(local.FplsAddr.Index = 0; local.FplsAddr.Index < local
          .FplsAddr.Count; ++local.FplsAddr.Index)
        {
          if (export.FplsAddr.IsFull)
          {
            break;
          }

          export.FplsAddr.Update.FplsAddrGroupDet.Text39 =
            local.FplsAddr.Item.FplsAddrGroupDet.Text39;
          export.FplsAddr.Next();
        }

        // -----------------------------------------------------------------
        // PR#79011
        // End Of Change
        // ------------------------------------------------------------------
        global.Command = "DISPLAY";
      }

      if (IsEmpty(export.StartingCsePerson.Number))
      {
        var field = GetField(export.StartingCsePerson, "number");

        field.Error = true;

        export.StartingCsePersonsWorkSet.FormattedName = "";
        export.DateUpdated.Date = null;
        export.UpdatedBy.CreatedBy = "";
        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

        return;
      }

      local.UserAction.Command = global.Command;

      // -------------------------------------------------------------------------------
      // 10/25/07  G. Pan   CQ610
      // Added READ to implement the business rules #44 for  FPLS screen - If a 
      // case number and the person number is entered
      // but the person is not play an AP role on that case, sent an error 
      // message.
      // -------------------------------------------------------------------------------
      // ****************************************************************************************
      // ** CQ5577:  The following set of code commented out in order to process
      // all the roles **
      // ** currently accepting only AP role, With this changes will accept 
      // other roles(AR,CH).**
      // ****************************************************************************************
      UseOeFplsDisplayRequest();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
      }
      else if (IsExitState("CSE_PERSON_NF"))
      {
        var field = GetField(export.StartingCsePerson, "number");

        field.Error = true;
      }

      // ************************************************
      // *Move Returned Local Views to Export.          *
      // ************************************************
      export.FplsLocateResponse.Assign(local.FplsLocateResponse);
      MoveScrollingAttributes(local.ScrollingAttributes,
        export.ScrollingAttributes);

      // ************************************************
      // *Show the Worker that last updated this Request*
      // *and the Date last Updated.                    *
      // ************************************************
      if (IsEmpty(local.FplsLocateRequest.LastUpdatedBy))
      {
        local.UpdatedBy.CreatedBy = local.FplsLocateRequest.CreatedBy;
        local.DateUpdated.Date = Date(local.FplsLocateRequest.CreatedTimestamp);
      }
      else
      {
        local.UpdatedBy.CreatedBy = local.FplsLocateRequest.LastUpdatedBy;
        local.DateUpdated.Date =
          Date(local.FplsLocateRequest.LastUpdatedTimestamp);
      }

      export.DateUpdated.Date = local.DateUpdated.Date;
      export.UpdatedBy.CreatedBy = local.UpdatedBy.CreatedBy;

      if (local.FplsLocateRequest.Identifier <= 0)
      {
        return;
      }
      else
      {
        MoveFplsLocateRequest3(local.FplsLocateRequest, export.FplsLocateRequest);
          

        // -----------------------------------------------------------------
        // 4.10.100
        // Beginning Of Change
        // Since we are not using SESA State, all statements using SESA group 
        // are commented through-out this procedure.
        // ------------------------------------------------------------------
        // -----------------------------------------------------------------
        // 4.10.100
        // End Of Change
        // Since we are not using SESA State, all statements using SESA group 
        // are commented through-out this procedure.
        // ------------------------------------------------------------------
      }

      // ***************************************************
      // Corrected consistency check error ICCGV09W by removing
      // TARGETING export_addr_group FROM THE BEGINNING
      // UNTIL FULL from the FOR EACH statement.
      // MK 9/13/98
      // ***************************************************
      // -----------------------------------------------------------------
      // PR#79011
      // Beginning Of Change
      // ------------------------------------------------------------------
      for(export.FplsAddr.Index = 0; export.FplsAddr.Index < export
        .FplsAddr.Count; ++export.FplsAddr.Index)
      {
        export.FplsAddr.Update.FplsAddrGroupDet.Text39 = "";
      }

      // -----------------------------------------------------------------
      // PR#79011
      // End Of Change
      // ------------------------------------------------------------------
      if (local.FplsLocateResponse.Identifier <= 0)
      {
        return;
      }
      else
      {
        // -----------------------------------------------------------------
        // PR#79011
        // Beginning Of Change
        // ------------------------------------------------------------------
        export.FplsAddr.Index = 0;
        export.FplsAddr.Clear();

        for(local.FplsAddr.Index = 0; local.FplsAddr.Index < local
          .FplsAddr.Count; ++local.FplsAddr.Index)
        {
          if (export.FplsAddr.IsFull)
          {
            break;
          }

          export.FplsAddr.Update.FplsAddrGroupDet.Text39 =
            local.FplsAddr.Item.FplsAddrGroupDet.Text39;
          export.FplsAddr.Next();
        }

        // -----------------------------------------------------------------
        // PR#79011
        // End Of Change
        // ------------------------------------------------------------------
      }

      // -------------------------------------------------------------------------------
      // 2/22/08  G. Pan   CQ311
      // Commented out above logic and added below new logic. The program checks
      // five agency codes (NPRC (D01), SSA (E01, E02), MBR (E03)
      // and VA (F01) and its related data.  If it's true then program displays
      // "See Page 2" at the bottom FPLS screen.
      // -------------------------------------------------------------------------------
      export.Page2Message.Text10 = "";
      local.ProcessPage2.Flag = "N";

      if (Equal(export.FplsLocateResponse.AgencyCode, "D01") && !
        IsEmpty(export.FplsLocateResponse.NprcEmpdOrSepd))
      {
        export.Page2Message.Text10 = "SEE PAGE 2";
        local.ProcessPage2.Flag = "Y";
      }

      if (Equal(export.FplsLocateResponse.AgencyCode, "E01") || Equal
        (export.FplsLocateResponse.AgencyCode, "E02") && (
          !IsEmpty(export.FplsLocateResponse.SsaCorpDivision) || !
        IsEmpty(export.FplsLocateResponse.SsaFederalOrMilitary)))
      {
        export.Page2Message.Text10 = "SEE PAGE 2";
        local.ProcessPage2.Flag = "Y";
      }

      if (Equal(export.FplsLocateResponse.AgencyCode, "E03") && (
        export.FplsLocateResponse.MbrBenefitAmount.GetValueOrDefault() != 0 || !
        Equal(export.FplsLocateResponse.MbrDateOfDeath, null)))
      {
        export.Page2Message.Text10 = "SEE PAGE 2";
        local.ProcessPage2.Flag = "Y";
      }

      if (Equal(export.FplsLocateResponse.AgencyCode, "F01") && (
        !IsEmpty(export.FplsLocateResponse.VaBenefitCode) || !
        Equal(export.FplsLocateResponse.VaDateOfDeath, null) || !
        Equal(export.FplsLocateResponse.VaAmtOfAwardEffectiveDate, null) || export
        .FplsLocateResponse.VaAmountOfAward.GetValueOrDefault() != 0 || !
        IsEmpty(export.FplsLocateResponse.VaSuspenseCode) || !
        IsEmpty(export.FplsLocateResponse.VaIncarcerationCode) || !
        IsEmpty(export.FplsLocateResponse.VaRetirementPayCode)))
      {
        export.Page2Message.Text10 = "SEE PAGE 2";
        local.ProcessPage2.Flag = "Y";
      }
    }

    if (!Equal(global.Command, "ADDR"))
    {
      if (AsChar(export.AddressFormatFlag.Flag) == 'Y')
      {
        export.AddressFormatFlag.Flag = "";
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "LIST":
        if (AsChar(export.CsePersonPrompt.SelectChar) != 'S')
        {
          var field = GetField(export.CsePersonPrompt, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          return;
        }

        export.FplsLocateRequest.RequestSentDate =
          import.LocalEmptyFplsLocateRequest.RequestSentDate;
        export.FplsLocateResponse.DateUsed =
          import.LocalEmptyFplsLocateResponse.DateUsed;

        if (!IsEmpty(export.StartingCase.Number))
        {
          ExitState = "ECO_LNK_TO_CASE_COMPOSITION";
        }
        else
        {
          ExitState = "ECO_LNK_TO_CSE_NAME_LIST";
        }

        break;
      case "PREV":
        if (IsEmpty(import.ScrollingAttributes.MinusFlag))
        {
          ExitState = "OE0097_NO_MORE_REQUEST";
        }

        break;
      case "NEXT":
        if (IsEmpty(import.ScrollingAttributes.PlusFlag))
        {
          ExitState = "OE0097_NO_MORE_REQUEST";
        }

        break;
      case "PAGE_2":
        // ************************************************
        // *If there is no data to be seen on Page 2 then *
        // *Just don't go there -- OK!                    *
        // ************************************************
        // -------------------------------------------------------------------------------
        // 2/28/08  G. Pan   CQ311
        // If page2 flag is not set to Y, program continue to process.
        // -------------------------------------------------------------------------------
        if (IsEmpty(export.Page2Message.Text10) && AsChar
          (local.ProcessPage2.Flag) != 'Y')
        {
          ExitState = "OE0000_FPLS_PAGE2_EMPTY";

          return;
        }

        ExitState = "ECO_LNK_TO_PROCESS_FPLS_PAGE2";

        break;
      case "ADDR":
        // ----------------------------------------
        // WR292
        // Beginning of code
        // Flow to ADDR screen with Returned Address.
        // ------------------------------------------
        if (IsEmpty(export.FplsLocateResponse.ReturnedAddress) && IsEmpty
          (export.StartingCase.Number))
        {
          ExitState = "NEED_CASE_NUM_FLOW_TO_ADDR";

          return;
        }

        if (export.FplsLocateRequest.Identifier > 0 && !
          IsEmpty(export.FplsLocateResponse.AgencyCode) && export
          .FplsLocateResponse.Identifier > 0)
        {
          // -------------------------------------------------------------------------------
          // 10/25/07  G. Pan   CQ610
          // Added qualifier statements to reference in case_assignment, 
          // office_service_provider and service_provider in both
          // READ case & case_role.  This is to implement the business rules 42 
          // & 43 for  FPLS screen - 42. If a person is an
          // AR on one case and an AP on another case and no case number is 
          // displayed on FPLS, flow to ADDR to the case where
          // the person is playing the AP role. 43. If a person is an AP on more
          // then one case, will flow to ADDR with the
          // lowest case number that the AP is assigned to for that RSP SP.
          // -------------------------------------------------------------------------------
          if (IsEmpty(export.FplsLocateResponse.ReturnedAddress) && !
            IsEmpty(export.StartingCase.Number))
          {
          }
          else
          {
            local.ApCount.Count = 0;
            local.ArCount.Count = 0;
            local.ApCaseFound.Flag = "N";
            local.ArCaseFound.Flag = "N";

            if (ReadCsePerson())
            {
              // ----------------------------------------
              // WR010500
              // Beginning of code
              // If the person is playing 2 different roles on different
              // cases then select lower case number where person plays AP
              // role.
              // ------------------------------------------
              if (!IsEmpty(export.StartingCase.Number))
              {
                if (ReadCaseCaseRole1())
                {
                  goto Test;
                }

                ExitState = "CASE_NOT_BELONG_SP";

                return;
              }

Test:

              // 10/30/2008 G. Pan CQ610 - added below two statements to fix 
              // acceptance test problem.
              local.ApCaseNumber.Number = "";
              local.ArCaseNumber.Number = "";

              foreach(var item in ReadCaseCaseRole2())
              {
                if (Equal(entities.ExistingCaseRole.Type1, "AR"))
                {
                  // 10/30/2008 G. Pan CQ610 - added following IF statement for 
                  // acceptance test problem.
                  if (IsEmpty(local.ArCaseNumber.Number))
                  {
                    local.ArCaseNumber.Number = entities.ExistingCase.Number;
                  }

                  local.ArCaseFound.Flag = "Y";
                  ++local.ArCount.Count;

                  continue;
                }
                else
                {
                  // 10/30/2008 G. Pan CQ610 - added following IF statement to 
                  // fix acceptance test problem.
                  if (IsEmpty(local.ApCaseNumber.Number))
                  {
                    local.ApCaseNumber.Number = entities.ExistingCase.Number;
                  }

                  local.ApCaseFound.Flag = "Y";
                  ++local.ApCount.Count;

                  continue;
                }
              }

              if (AsChar(local.ApCaseFound.Flag) != 'Y' && AsChar
                (local.ArCaseFound.Flag) != 'Y')
              {
                var field = GetField(export.StartingCsePerson, "number");

                field.Error = true;

                ExitState = "ACO_NE0000_CASE_NF_FOR_PERSON";

                return;
              }

              if (local.ApCount.Count == 1 && local.ArCount.Count == 1)
              {
                // 10/30/2008 G. Pan CQ610 - added following SET statement to 
                // fix acceptance test problem.
                export.StartingCase.Number = local.ApCaseNumber.Number;
              }

              if (local.ApCount.Count > 0)
              {
                export.StartingCase.Number = local.ApCaseNumber.Number;
              }
              else
              {
                if (local.ArCount.Count > 0)
                {
                  export.StartingCase.Number = local.ArCaseNumber.Number;
                }

                if (local.ApCount.Count == 0 && local.ArCount.Count == 0)
                {
                  ExitState = "CO0000_AP_CSE_PERSON_NF";

                  var field = GetField(export.StartingCsePerson, "number");

                  field.Error = true;
                }
              }

              export.Addr1.Number = entities.ExistingCsePerson.Number;
            }
            else
            {
              ExitState = "CSE_PERSON_NF";

              var field = GetField(export.StartingCsePerson, "number");

              field.Error = true;

              return;
            }

            if (Equal(export.FplsLocateResponse.AgencyCode, "H97") || Equal
              (export.FplsLocateResponse.AgencyCode, "H98") || Equal
              (export.FplsLocateResponse.AgencyCode, "H99"))
            {
              if (Equal(export.FplsLocateResponse.AgencyCode, "H97"))
              {
                export.CsePersonAddress.Street1 =
                  Substring(export.FplsLocateResponse.ReturnedAddress, 1, 25);

                if (!IsEmpty(Substring(
                  export.FplsLocateResponse.ReturnedAddress, 41, 40)))
                {
                  export.CsePersonAddress.Street2 =
                    Substring(export.FplsLocateResponse.ReturnedAddress, 41, 23);
                    
                }
                else
                {
                  export.CsePersonAddress.Street2 =
                    Substring(export.FplsLocateResponse.ReturnedAddress, 26, 23);
                    
                }

                export.CsePersonAddress.City =
                  Substring(export.FplsLocateResponse.ReturnedAddress, 161, 15);
                  
                export.CsePersonAddress.State =
                  Substring(export.FplsLocateResponse.ReturnedAddress, 191, 2);
                export.CsePersonAddress.ZipCode =
                  Substring(export.FplsLocateResponse.ReturnedAddress, 193, 5);
                export.CsePersonAddress.Zip4 =
                  Substring(export.FplsLocateResponse.ReturnedAddress, 198, 4);
              }

              if (Equal(export.FplsLocateResponse.AgencyCode, "H98"))
              {
                // ----------------------------------------
                // This Agency provides only Employer address
                // which we can not display on ADDR screen.
                // Set exit state message.
                // ------------------------------------------
                ExitState = "ACO_NE0000_EMPLOYER_ADDRESS";

                return;
              }

              if (Equal(export.FplsLocateResponse.AgencyCode, "H99"))
              {
                if (AsChar(export.FplsLocateResponse.AddressIndType) == '1' || AsChar
                  (export.FplsLocateResponse.AddressIndType) == '3')
                {
                  // ----------------------------------------
                  // This is Employer address which we can not
                  // display on ADDR screen. Set exit state message.
                  // ------------------------------------------
                  ExitState = "ACO_NE0000_EMPLOYER_ADDRESS";

                  return;
                }
                else
                {
                  export.CsePersonAddress.Street1 =
                    Substring(export.FplsLocateResponse.ReturnedAddress, 1, 25);
                    

                  if (!IsEmpty(Substring(
                    export.FplsLocateResponse.ReturnedAddress, 41, 40)))
                  {
                    export.CsePersonAddress.Street2 =
                      Substring(export.FplsLocateResponse.ReturnedAddress, 41,
                      23);
                  }
                  else
                  {
                    export.CsePersonAddress.Street2 =
                      Substring(export.FplsLocateResponse.ReturnedAddress, 26,
                      23);
                  }

                  export.CsePersonAddress.City =
                    Substring(export.FplsLocateResponse.ReturnedAddress, 161, 15);
                    
                  export.CsePersonAddress.State =
                    Substring(export.FplsLocateResponse.ReturnedAddress, 191, 2);
                    
                  export.CsePersonAddress.ZipCode =
                    Substring(export.FplsLocateResponse.ReturnedAddress, 193, 5);
                    
                  export.CsePersonAddress.Zip4 =
                    Substring(export.FplsLocateResponse.ReturnedAddress, 198, 4);
                    
                }
              }
            }
            else if (Equal(export.FplsLocateResponse.AgencyCode, "A01") || Equal
              (export.FplsLocateResponse.AgencyCode, "A02") || Equal
              (export.FplsLocateResponse.AgencyCode, "B01") || Equal
              (export.FplsLocateResponse.AgencyCode, "C01") || Equal
              (export.FplsLocateResponse.AgencyCode, "E01") || Equal
              (export.FplsLocateResponse.AgencyCode, "E03") || Equal
              (export.FplsLocateResponse.AgencyCode, "F01"))
            {
              if (Equal(export.FplsLocateResponse.AgencyCode, "A02"))
              {
                if (AsChar(export.FplsLocateResponse.AddressIndType) == '1')
                {
                  // ----------------------------------------
                  // This is Employer address which we can not
                  // display on ADDR screen. Set exit state message.
                  // ------------------------------------------
                  ExitState = "ACO_NE0000_EMPLOYER_ADDRESS";

                  return;
                }
              }

              if (Equal(export.FplsLocateResponse.AgencyCode, "B01"))
              {
                if (AsChar(export.FplsLocateResponse.AddressIndType) == '1')
                {
                  // ----------------------------------------
                  // This is Employer address which we can not
                  // display on ADDR screen. Set exit state message.
                  // ------------------------------------------
                  ExitState = "ACO_NE0000_EMPLOYER_ADDRESS";

                  return;
                }
              }

              if (Equal(export.FplsLocateResponse.AgencyCode, "C01"))
              {
                // ----------------------------------------
                // This is IRS Agency which we can not
                // display on ADDR screen. Set exit state message.
                // ------------------------------------------
                ExitState = "ACO_NE0000_IRS_ADDRESS";

                return;
              }

              if (AsChar(export.FplsLocateResponse.AddressFormatInd) == 'C' || AsChar
                (export.FplsLocateResponse.AddressFormatInd) == 'X')
              {
                export.CsePersonAddress.City =
                  Substring(export.FplsLocateResponse.ReturnedAddress, 161, 15);
                  
                export.CsePersonAddress.State =
                  Substring(export.FplsLocateResponse.ReturnedAddress, 191, 2);
                export.CsePersonAddress.ZipCode =
                  Substring(export.FplsLocateResponse.ReturnedAddress, 193, 5);
                export.CsePersonAddress.Zip4 =
                  Substring(export.FplsLocateResponse.ReturnedAddress, 198, 4);

                if (AsChar(export.FplsLocateResponse.AddressFormatInd) == 'X')
                {
                  export.CsePersonAddress.Street1 =
                    Substring(export.FplsLocateResponse.ReturnedAddress, 1, 25);
                    

                  if (!IsEmpty(Substring(
                    export.FplsLocateResponse.ReturnedAddress, 41, 40)))
                  {
                    export.CsePersonAddress.Street2 =
                      Substring(export.FplsLocateResponse.ReturnedAddress, 41,
                      23);
                  }
                  else
                  {
                    export.CsePersonAddress.Street2 =
                      Substring(export.FplsLocateResponse.ReturnedAddress, 26,
                      23);
                  }
                }

                if (AsChar(export.FplsLocateResponse.AddressFormatInd) == 'C')
                {
                  local.ReturnedAddress.ReturnedAddress =
                    Substring(export.FplsLocateResponse.ReturnedAddress, 1, 160);
                    

                  do
                  {
                    local.EndPointer.Count =
                      Find(local.ReturnedAddress.ReturnedAddress, "\\");

                    if (local.EndPointer.Count == 0)
                    {
                      break;
                    }
                    else
                    {
                      ++local.TotalCount.Count;
                    }

                    if (local.TotalCount.Count == 1)
                    {
                      local.Street1.ReturnedAddress =
                        Substring(local.ReturnedAddress.ReturnedAddress, 1,
                        local.EndPointer.Count - 1);
                    }

                    if (local.TotalCount.Count == 2)
                    {
                      local.Street2.ReturnedAddress =
                        Substring(local.ReturnedAddress.ReturnedAddress, 1,
                        local.EndPointer.Count - 1);

                      break;
                    }

                    local.ReturnedAddress.ReturnedAddress =
                      Substring(local.ReturnedAddress.ReturnedAddress,
                      local.EndPointer.Count +
                      1, FplsLocateResponse.ReturnedAddress_MaxLength -
                      local.EndPointer.Count);
                  }
                  while(!Equal(global.Command, "COMMAND"));

                  if (local.TotalCount.Count == 0 && IsEmpty
                    (local.Street1.ReturnedAddress) && !
                    IsEmpty(local.ReturnedAddress.ReturnedAddress))
                  {
                    export.CsePersonAddress.Street1 =
                      TrimEnd(local.ReturnedAddress.ReturnedAddress);
                  }
                  else
                  {
                    if (!IsEmpty(local.Street1.ReturnedAddress))
                    {
                      export.CsePersonAddress.Street1 =
                        TrimEnd(local.Street1.ReturnedAddress);
                    }

                    if (!IsEmpty(local.Street2.ReturnedAddress))
                    {
                      export.CsePersonAddress.Street2 =
                        TrimEnd(local.Street2.ReturnedAddress);
                    }
                  }
                }
              }

              if (AsChar(export.FplsLocateResponse.AddressFormatInd) == 'F')
              {
                export.CsePersonAddress.ZipCode =
                  Substring(export.FplsLocateResponse.ReturnedAddress, 193, 5);
                export.CsePersonAddress.Zip4 =
                  Substring(export.FplsLocateResponse.ReturnedAddress, 198, 4);

                if (AsChar(export.AddressFormatFlag.Flag) == 'Y')
                {
                  export.AddressFormatFlag.Flag = "";
                }
                else
                {
                  export.AddressFormatFlag.Flag = "Y";
                  ExitState = "ACO_NE0000_MANUAL_ADDRESS_ENTRY";

                  return;
                }
              }
            }
          }
        }

        // ----------------------------------------
        // WR292
        // End of code.
        // ------------------------------------------
        ExitState = "ECO_LNK_TO_ADDRESS_MAINTENANCE";

        break;
      case "INCS":
        export.StartingCsePersonsWorkSet.Number =
          export.StartingCsePerson.Number;
        ExitState = "ECO_LNK_TO_INCOME_SOURCE_DETAILS";

        break;
      case "ADD":
        // ************************************************
        // Added validation above to test CSE Person number for spaces
        // before USING add or update cabs.
        // MK 9/14/98
        // ************************************************
        UseCreateFplsLocateRequest();

        if (!IsExitState("OE0000_FPLS_REQUEST_CREATE_OK"))
        {
          if (IsExitState("OE0142_KS_IS_NOT_ENTERABLE"))
          {
            // -----------------------------------------------------------------
            // 4.10.100
            // Beginning Of Change
            // Since we are not using SESA State, all statements using SESA 
            // group are commented through-out this procedure.
            // ------------------------------------------------------------------
            // -----------------------------------------------------------------
            // 4.10.100
            // End Of Change
            // Since we are not using SESA State, all statements using SESA 
            // group are commented through-out this procedure.
            // ------------------------------------------------------------------
            // ************************************************
            // Added validation above to test CSE Person NF
            // with specific MAKE ERROR on CSE Person Number.
            // Removed generic MAKE ERROR on
            // export FPLS_LOCATE_REQUEST IDENTIFIER
            // MK 9/14/98
            // ************************************************
          }
          else if (IsExitState("CSE_PERSON_NF"))
          {
            var field = GetField(export.StartingCsePerson, "number");

            field.Error = true;
          }
          else
          {
            // *** FALL THRU***
          }

          return;
        }

        global.Command = "DISPLAY";
        local.UserAction.Command = global.Command;
        UseOeFplsDisplayRequest();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }
        else if (IsExitState("CSE_PERSON_NF"))
        {
          var field = GetField(export.StartingCsePerson, "number");

          field.Error = true;
        }

        // ************************************************
        // *Move Returned Local Views to Export.          *
        // ************************************************
        export.FplsLocateResponse.Assign(local.FplsLocateResponse);
        MoveScrollingAttributes(local.ScrollingAttributes,
          export.ScrollingAttributes);

        // ************************************************
        // *Show the Worker that last updated this Request*
        // *and the Date last Updated.                    *
        // ************************************************
        if (IsEmpty(local.FplsLocateRequest.LastUpdatedBy))
        {
          local.UpdatedBy.CreatedBy = local.FplsLocateRequest.CreatedBy;
          local.DateUpdated.Date =
            Date(local.FplsLocateRequest.CreatedTimestamp);
        }
        else
        {
          local.UpdatedBy.CreatedBy = local.FplsLocateRequest.LastUpdatedBy;
          local.DateUpdated.Date =
            Date(local.FplsLocateRequest.LastUpdatedTimestamp);
        }

        export.DateUpdated.Date = local.DateUpdated.Date;
        export.UpdatedBy.CreatedBy = local.UpdatedBy.CreatedBy;

        if (local.FplsLocateRequest.Identifier <= 0)
        {
          return;
        }
        else
        {
          MoveFplsLocateRequest3(local.FplsLocateRequest,
            export.FplsLocateRequest);

          // -----------------------------------------------------------------
          // 4.10.100
          // Beginning Of Change
          // Since we are not using SESA State, all statements using SESA group 
          // are commented through-out this procedure.
          // ------------------------------------------------------------------
          // -----------------------------------------------------------------
          // 4.10.100
          // End Of Change
          // Since we are not using SESA State, all statements using SESA group 
          // are commented through-out this procedure.
          // ------------------------------------------------------------------
        }

        // ***************************************************
        // Corrected consistency check error ICCGV09W by removing
        // TARGETING export_addr_group FROM THE BEGINNING
        // UNTIL FULL from the FOR EACH statement.
        // MK 9/13/98
        // ***************************************************
        // -----------------------------------------------------------------
        // PR#79011
        // Beginning Of Change
        // ------------------------------------------------------------------
        for(export.FplsAddr.Index = 0; export.FplsAddr.Index < export
          .FplsAddr.Count; ++export.FplsAddr.Index)
        {
          export.FplsAddr.Update.FplsAddrGroupDet.Text39 = "";
        }

        // -----------------------------------------------------------------
        // PR#79011
        // End Of Change
        // ------------------------------------------------------------------
        if (local.FplsLocateResponse.Identifier <= 0)
        {
        }
        else
        {
          // -----------------------------------------------------------------
          // PR#79011
          // Beginning Of Change
          // ------------------------------------------------------------------
          export.FplsAddr.Index = 0;
          export.FplsAddr.Clear();

          for(local.FplsAddr.Index = 0; local.FplsAddr.Index < local
            .FplsAddr.Count; ++local.FplsAddr.Index)
          {
            if (export.FplsAddr.IsFull)
            {
              break;
            }

            export.FplsAddr.Update.FplsAddrGroupDet.Text39 =
              local.FplsAddr.Item.FplsAddrGroupDet.Text39;
            export.FplsAddr.Next();
          }

          // -----------------------------------------------------------------
          // PR#79011
          // End Of Change
          // ------------------------------------------------------------------
        }

        break;
      case "UPDATE":
        // ************************************************
        // Added validation above to test CSE Person number for spaces
        // before USING add or update cabs.
        // MK 9/14/98
        // ************************************************
        if (!Equal(import.StartingCsePerson.Number,
          import.HiddenCsePerson.Number))
        {
          var field = GetField(export.StartingCsePerson, "number");

          field.Error = true;

          ExitState = "ZD_ACO_NE0000_MUST_DISPLAY_FIRST";

          return;
        }

        // ************************************************
        // *Updates are only alowed if the request has not*
        // *already been sent to the FPLS agency.         *
        // ************************************************
        if (Lt(local.NullDate.Date, import.FplsLocateRequest.RequestSentDate))
        {
          var field = GetField(export.FplsLocateRequest, "transactionStatus");

          field.Error = true;

          ExitState = "OE0000_FPLS_TRAN_STATUS_NOT_C";

          return;
        }

        UseOeFplsUpdateFplsRequest();

        if (!IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE"))
        {
          if (IsExitState("OE0142_KS_IS_NOT_ENTERABLE"))
          {
            // -----------------------------------------------------------------
            // 4.10.100
            // Beginning Of Change
            // Since we are not using SESA State, all statements using SESA 
            // group are commented through-out this procedure.
            // ------------------------------------------------------------------
            // -----------------------------------------------------------------
            // 4.10.100
            // End Of Change
            // Since we are not using SESA State, all statements using SESA 
            // group are commented through-out this procedure.
            // ------------------------------------------------------------------
            // ************************************************
            // Added validation above to test CSE Person NF
            // with specific MAKE ERROR on CSE Person Number.
            // Removed generic MAKE ERROR on
            // export FPLS_LOCATE_REQUEST IDENTIFIER
            // MK 9/14/98
            // ************************************************
          }
          else if (IsExitState("CSE_PERSON_NF"))
          {
            var field = GetField(export.StartingCsePerson, "number");

            field.Error = true;
          }
          else
          {
            // *** FALL THRU***
          }

          return;
        }

        global.Command = "DISPLAY";
        local.UserAction.Command = global.Command;
        UseOeFplsDisplayRequest();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }
        else if (IsExitState("CSE_PERSON_NF"))
        {
          var field = GetField(export.StartingCsePerson, "number");

          field.Error = true;

          return;
        }

        // ************************************************
        // *Move Returned Local Views to Export.          *
        // ************************************************
        export.FplsLocateResponse.Assign(local.FplsLocateResponse);
        MoveScrollingAttributes(local.ScrollingAttributes,
          export.ScrollingAttributes);

        // ************************************************
        // *Show the Worker that last updated this Request*
        // *and the Date last Updated.                    *
        // ************************************************
        if (IsEmpty(local.FplsLocateRequest.LastUpdatedBy))
        {
          local.UpdatedBy.CreatedBy = local.FplsLocateRequest.CreatedBy;
          local.DateUpdated.Date =
            Date(local.FplsLocateRequest.CreatedTimestamp);
        }
        else
        {
          local.UpdatedBy.CreatedBy = local.FplsLocateRequest.LastUpdatedBy;
          local.DateUpdated.Date =
            Date(local.FplsLocateRequest.LastUpdatedTimestamp);
        }

        export.DateUpdated.Date = local.DateUpdated.Date;
        export.UpdatedBy.CreatedBy = local.UpdatedBy.CreatedBy;

        if (local.FplsLocateRequest.Identifier <= 0)
        {
          return;
        }
        else
        {
          MoveFplsLocateRequest3(local.FplsLocateRequest,
            export.FplsLocateRequest);

          // -----------------------------------------------------------------
          // 4.10.100
          // Beginning Of Change
          // Since we are not using SESA State, all statements using SESA group 
          // are commented through-out this procedure.
          // ------------------------------------------------------------------
          // -----------------------------------------------------------------
          // 4.10.100
          // End Of Change
          // Since we are not using SESA State, all statements using SESA group 
          // are commented through-out this procedure.
          // ------------------------------------------------------------------
        }

        // ***************************************************
        // Corrected consistency check error ICCGV09W by removing
        // TARGETING export_addr_group FROM THE BEGINNING
        // UNTIL FULL from the FOR EACH statement.
        // MK 9/13/98
        // ***************************************************
        // -----------------------------------------------------------------
        // PR#79011
        // Beginning Of Change
        // ------------------------------------------------------------------
        for(export.FplsAddr.Index = 0; export.FplsAddr.Index < export
          .FplsAddr.Count; ++export.FplsAddr.Index)
        {
          export.FplsAddr.Update.FplsAddrGroupDet.Text39 = "";
        }

        // -----------------------------------------------------------------
        // PR#79011
        // End Of Change
        // ------------------------------------------------------------------
        if (local.FplsLocateResponse.Identifier <= 0)
        {
        }
        else
        {
          // -----------------------------------------------------------------
          // PR#79011
          // Beginning Of Change
          // ------------------------------------------------------------------
          export.FplsAddr.Index = 0;
          export.FplsAddr.Clear();

          for(local.FplsAddr.Index = 0; local.FplsAddr.Index < local
            .FplsAddr.Count; ++local.FplsAddr.Index)
          {
            if (export.FplsAddr.IsFull)
            {
              break;
            }

            export.FplsAddr.Update.FplsAddrGroupDet.Text39 =
              local.FplsAddr.Item.FplsAddrGroupDet.Text39;
            export.FplsAddr.Next();
          }

          // -----------------------------------------------------------------
          // PR#79011
          // End Of Change
          // ------------------------------------------------------------------
        }

        break;
      case "DISPLAY":
        // --------------------------------------------------------------------------------
        // 3/05/08  G. Pan   CQ311
        // Added CASE RETURN
        // -------------------------------------------------------------------------------
        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveFplsAddr(OeFplsDisplayRequest.Export.
    FplsAddrGroup source, Local.FplsAddrGroup target)
  {
    target.FplsAddrGroupDet.Text39 = source.FplsAddrGroupDet.Text39;
  }

  private static void MoveFplsLocateRequest1(FplsLocateRequest source,
    FplsLocateRequest target)
  {
    target.Identifier = source.Identifier;
    target.TransactionStatus = source.TransactionStatus;
  }

  private static void MoveFplsLocateRequest2(FplsLocateRequest source,
    FplsLocateRequest target)
  {
    target.Identifier = source.Identifier;
    target.TransactionStatus = source.TransactionStatus;
    target.ZdelReqCreatDt = source.ZdelReqCreatDt;
    target.ZdelCreatUserId = source.ZdelCreatUserId;
    target.RequestSentDate = source.RequestSentDate;
    target.StationNumber = source.StationNumber;
    target.TransactionType = source.TransactionType;
    target.Ssn = source.Ssn;
    target.CaseId = source.CaseId;
    target.LocalCode = source.LocalCode;
    target.UsersField = source.UsersField;
    target.TypeOfCase = source.TypeOfCase;
    target.TransactionError = source.TransactionError;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.SendRequestTo = source.SendRequestTo;
  }

  private static void MoveFplsLocateRequest3(FplsLocateRequest source,
    FplsLocateRequest target)
  {
    target.Identifier = source.Identifier;
    target.TransactionStatus = source.TransactionStatus;
    target.RequestSentDate = source.RequestSentDate;
    target.StationNumber = source.StationNumber;
    target.TransactionType = source.TransactionType;
    target.Ssn = source.Ssn;
    target.CaseId = source.CaseId;
    target.LocalCode = source.LocalCode;
    target.UsersField = source.UsersField;
    target.TypeOfCase = source.TypeOfCase;
    target.TransactionError = source.TransactionError;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.SendRequestTo = source.SendRequestTo;
  }

  private static void MoveFplsLocateRequest4(FplsLocateRequest source,
    FplsLocateRequest target)
  {
    target.Identifier = source.Identifier;
    target.TransactionStatus = source.TransactionStatus;
    target.UsersField = source.UsersField;
  }

  private static void MoveFplsLocateResponse(FplsLocateResponse source,
    FplsLocateResponse target)
  {
    target.Identifier = source.Identifier;
    target.DateReceived = source.DateReceived;
    target.UsageStatus = source.UsageStatus;
    target.DateUsed = source.DateUsed;
    target.AgencyCode = source.AgencyCode;
    target.NameSentInd = source.NameSentInd;
    target.ApNameReturned = source.ApNameReturned;
    target.AddrDateFormatInd = source.AddrDateFormatInd;
    target.DateOfAddress = source.DateOfAddress;
    target.ResponseCode = source.ResponseCode;
    target.AddressFormatInd = source.AddressFormatInd;
    target.ReturnedAddress = source.ReturnedAddress;
    target.DodStatus = source.DodStatus;
    target.DodServiceCode = source.DodServiceCode;
    target.DodPayGradeCode = source.DodPayGradeCode;
    target.SesaRespondingState = source.SesaRespondingState;
    target.SesaWageClaimInd = source.SesaWageClaimInd;
    target.SesaWageAmount = source.SesaWageAmount;
    target.IrsNameControl = source.IrsNameControl;
    target.IrsTaxYear = source.IrsTaxYear;
    target.NprcEmpdOrSepd = source.NprcEmpdOrSepd;
    target.SsaFederalOrMilitary = source.SsaFederalOrMilitary;
    target.SsaCorpDivision = source.SsaCorpDivision;
    target.MbrBenefitAmount = source.MbrBenefitAmount;
    target.MbrDateOfDeath = source.MbrDateOfDeath;
    target.VaBenefitCode = source.VaBenefitCode;
    target.VaDateOfDeath = source.VaDateOfDeath;
    target.VaAmtOfAwardEffectiveDate = source.VaAmtOfAwardEffectiveDate;
    target.VaAmountOfAward = source.VaAmountOfAward;
    target.VaSuspenseCode = source.VaSuspenseCode;
    target.VaIncarcerationCode = source.VaIncarcerationCode;
    target.VaRetirementPayCode = source.VaRetirementPayCode;
    target.SsnReturned = source.SsnReturned;
    target.DodAnnualSalary = source.DodAnnualSalary;
    target.DodDateOfBirth = source.DodDateOfBirth;
    target.SubmittingOffice = source.SubmittingOffice;
    target.AddressIndType = source.AddressIndType;
  }

  private static void MoveNextTranInfo(NextTranInfo source, NextTranInfo target)
  {
    target.LegalActionIdentifier = source.LegalActionIdentifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CsePersonNumberAp = source.CsePersonNumberAp;
    target.CsePersonNumberObligee = source.CsePersonNumberObligee;
    target.CsePersonNumberObligor = source.CsePersonNumberObligor;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.ObligationId = source.ObligationId;
    target.StandardCrtOrdNumber = source.StandardCrtOrdNumber;
    target.InfrastructureId = source.InfrastructureId;
    target.MiscText1 = source.MiscText1;
    target.MiscText2 = source.MiscText2;
    target.MiscNum1 = source.MiscNum1;
    target.MiscNum2 = source.MiscNum2;
    target.MiscNum1V2 = source.MiscNum1V2;
    target.MiscNum2V2 = source.MiscNum2V2;
  }

  private static void MoveScrollingAttributes(ScrollingAttributes source,
    ScrollingAttributes target)
  {
    target.PlusFlag = source.PlusFlag;
    target.MinusFlag = source.MinusFlag;
  }

  private static void MoveSesa(OeFplsDisplayRequest.Export.SesaGroup source,
    Local.SesaGroup target)
  {
    target.SesaGroupDet.State = source.SesaGroupDet.State;
  }

  private static void MoveSesaToGroup1(Import.SesaGroup source,
    CreateFplsLocateRequest.Import.GroupGroup target)
  {
    target.Det.State = source.SesaGroupDet.State;
  }

  private static void MoveSesaToGroup2(Import.SesaGroup source,
    OeFplsUpdateFplsRequest.Import.GroupGroup target)
  {
    target.Det.State = source.SesaGroupDet.State;
  }

  private void UseCreateFplsLocateRequest()
  {
    var useImport = new CreateFplsLocateRequest.Import();
    var useExport = new CreateFplsLocateRequest.Export();

    useImport.SendDirective.UsersField = export.FplsLocateRequest.UsersField;
    import.Sesa.CopyTo(useImport.Group, MoveSesaToGroup1);
    useImport.CsePerson.Number = import.StartingCsePerson.Number;

    Call(CreateFplsLocateRequest.Execute, useImport, useExport);

    export.FplsLocateRequest.Assign(useExport.FplsLocateRequest);
  }

  private void UseEabPadLeftWithZeros()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.TextWorkArea.Text10;
    useExport.TextWorkArea.Text10 = local.TextWorkArea.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.TextWorkArea.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseOeFplsDisplayRequest()
  {
    var useImport = new OeFplsDisplayRequest.Import();
    var useExport = new OeFplsDisplayRequest.Export();

    useImport.UserAction.Command = local.UserAction.Command;
    useImport.FplsLocateResponse.Assign(import.FplsLocateResponse);
    useImport.CsePerson.Number = export.StartingCsePerson.Number;
    useImport.Hidden.Number = export.HiddenCsePerson.Number;
    MoveFplsLocateRequest1(export.FplsLocateRequest, useImport.FplsLocateRequest);
      

    Call(OeFplsDisplayRequest.Execute, useImport, useExport);

    export.ErrorCodeDesc.Description = useExport.TransactionError.Description;
    export.SesaRespondingState.Description =
      useExport.SesaRespondingState.Description;
    MoveScrollingAttributes(useExport.ScrollingAttributes,
      local.ScrollingAttributes);
    export.StartingCsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    useExport.Sesa.CopyTo(local.Sesa, MoveSesa);
    local.FplsLocateRequest.Assign(useExport.FplsLocateRequest);
    MoveFplsLocateResponse(useExport.FplsLocateResponse,
      local.FplsLocateResponse);
    export.FplsAgency.Description = useExport.FplsAgency.Description;
    export.FplsResp.Description = useExport.FplsResp.Description;
    export.DodEligDesc.Description = useExport.DodEligDesc.Description;
    export.DodStatusDesc.Description = useExport.DodStatusDesc.Description;
    export.DodServiceDesc.Description = useExport.DodServiceDesc.Description;
    export.DodPayGradeDesc.Description = useExport.DodPayGradeDesc.Description;
    useExport.FplsAddr.CopyTo(local.FplsAddr, MoveFplsAddr);
  }

  private void UseOeFplsUpdateFplsRequest()
  {
    var useImport = new OeFplsUpdateFplsRequest.Import();
    var useExport = new OeFplsUpdateFplsRequest.Export();

    import.Sesa.CopyTo(useImport.Group, MoveSesaToGroup2);
    MoveFplsLocateRequest4(export.FplsLocateRequest, useImport.FplsLocateRequest);
      
    useImport.CsePerson.Number = export.StartingCsePerson.Number;

    Call(OeFplsUpdateFplsRequest.Execute, useImport, useExport);

    MoveFplsLocateRequest3(useExport.FplsLocateRequest, local.FplsLocateRequest);
      
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.HiddenNextTranInfo.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveNextTranInfo(export.HiddenNextTranInfo, useImport.NextTranInfo);

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabSignoff()
  {
    var useImport = new ScCabSignoff.Import();
    var useExport = new ScCabSignoff.Export();

    Call(ScCabSignoff.Execute, useImport, useExport);
  }

  private void UseScCabTestSecurity()
  {
    var useImport = new ScCabTestSecurity.Import();
    var useExport = new ScCabTestSecurity.Export();

    useImport.Case1.Number = export.StartingCase.Number;
    useImport.CsePerson.Number = export.StartingCsePerson.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private bool ReadCaseCaseRole1()
  {
    entities.ExistingCaseRole.Populated = false;
    entities.ExistingCase.Populated = false;

    return Read("ReadCaseCaseRole1",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
        db.SetNullableDate(command, "startDate", date);
        db.SetString(command, "numb", export.StartingCase.Number);
        db.SetString(command, "userId", global.UserId);
      },
      (db, reader) =>
      {
        entities.ExistingCase.Number = db.GetString(reader, 0);
        entities.ExistingCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ExistingCase.Status = db.GetNullableString(reader, 1);
        entities.ExistingCaseRole.CspNumber = db.GetString(reader, 2);
        entities.ExistingCaseRole.Type1 = db.GetString(reader, 3);
        entities.ExistingCaseRole.Identifier = db.GetInt32(reader, 4);
        entities.ExistingCaseRole.StartDate = db.GetNullableDate(reader, 5);
        entities.ExistingCaseRole.EndDate = db.GetNullableDate(reader, 6);
        entities.ExistingCaseRole.Populated = true;
        entities.ExistingCase.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingCaseRole.Type1);
      });
  }

  private IEnumerable<bool> ReadCaseCaseRole2()
  {
    entities.ExistingCaseRole.Populated = false;
    entities.ExistingCase.Populated = false;

    return ReadEach("ReadCaseCaseRole2",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
        db.SetNullableDate(command, "startDate", date);
        db.SetString(command, "userId", global.UserId);
      },
      (db, reader) =>
      {
        entities.ExistingCase.Number = db.GetString(reader, 0);
        entities.ExistingCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ExistingCase.Status = db.GetNullableString(reader, 1);
        entities.ExistingCaseRole.CspNumber = db.GetString(reader, 2);
        entities.ExistingCaseRole.Type1 = db.GetString(reader, 3);
        entities.ExistingCaseRole.Identifier = db.GetInt32(reader, 4);
        entities.ExistingCaseRole.StartDate = db.GetNullableDate(reader, 5);
        entities.ExistingCaseRole.EndDate = db.GetNullableDate(reader, 6);
        entities.ExistingCaseRole.Populated = true;
        entities.ExistingCase.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingCaseRole.Type1);

        return true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.ExistingCsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", export.StartingCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingCsePerson.Populated = true;
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
    /// <summary>A FplsAddrGroup group.</summary>
    [Serializable]
    public class FplsAddrGroup
    {
      /// <summary>
      /// A value of FplsAddrGroupDet.
      /// </summary>
      [JsonPropertyName("fplsAddrGroupDet")]
      public FplsWorkArea FplsAddrGroupDet
      {
        get => fplsAddrGroupDet ??= new();
        set => fplsAddrGroupDet = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private FplsWorkArea fplsAddrGroupDet;
    }

    /// <summary>A SesaGroup group.</summary>
    [Serializable]
    public class SesaGroup
    {
      /// <summary>
      /// A value of SesaGroupDet.
      /// </summary>
      [JsonPropertyName("sesaGroupDet")]
      public OblgWork SesaGroupDet
      {
        get => sesaGroupDet ??= new();
        set => sesaGroupDet = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private OblgWork sesaGroupDet;
    }

    /// <summary>A AddrGroup group.</summary>
    [Serializable]
    public class AddrGroup
    {
      /// <summary>
      /// A value of AddrGroupDetFplsWorkArea.
      /// </summary>
      [JsonPropertyName("addrGroupDetFplsWorkArea")]
      public FplsWorkArea AddrGroupDetFplsWorkArea
      {
        get => addrGroupDetFplsWorkArea ??= new();
        set => addrGroupDetFplsWorkArea = value;
      }

      /// <summary>
      /// A value of AddrGroupDetWorkArea.
      /// </summary>
      [JsonPropertyName("addrGroupDetWorkArea")]
      public WorkArea AddrGroupDetWorkArea
      {
        get => addrGroupDetWorkArea ??= new();
        set => addrGroupDetWorkArea = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private FplsWorkArea addrGroupDetFplsWorkArea;
      private WorkArea addrGroupDetWorkArea;
    }

    /// <summary>
    /// A value of Page2Message.
    /// </summary>
    [JsonPropertyName("page2Message")]
    public TextWorkArea Page2Message
    {
      get => page2Message ??= new();
      set => page2Message = value;
    }

    /// <summary>
    /// A value of AddressFormatFlag.
    /// </summary>
    [JsonPropertyName("addressFormatFlag")]
    public Common AddressFormatFlag
    {
      get => addressFormatFlag ??= new();
      set => addressFormatFlag = value;
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

    /// <summary>
    /// Gets a value of FplsAddr.
    /// </summary>
    [JsonIgnore]
    public Array<FplsAddrGroup> FplsAddr => fplsAddr ??= new(
      FplsAddrGroup.Capacity);

    /// <summary>
    /// Gets a value of FplsAddr for json serialization.
    /// </summary>
    [JsonPropertyName("fplsAddr")]
    [Computed]
    public IList<FplsAddrGroup> FplsAddr_Json
    {
      get => fplsAddr;
      set => FplsAddr.Assign(value);
    }

    /// <summary>
    /// A value of LocalEmptyFplsLocateResponse.
    /// </summary>
    [JsonPropertyName("localEmptyFplsLocateResponse")]
    public FplsLocateResponse LocalEmptyFplsLocateResponse
    {
      get => localEmptyFplsLocateResponse ??= new();
      set => localEmptyFplsLocateResponse = value;
    }

    /// <summary>
    /// A value of LocalEmptyFplsLocateRequest.
    /// </summary>
    [JsonPropertyName("localEmptyFplsLocateRequest")]
    public FplsLocateRequest LocalEmptyFplsLocateRequest
    {
      get => localEmptyFplsLocateRequest ??= new();
      set => localEmptyFplsLocateRequest = value;
    }

    /// <summary>
    /// A value of UpdatedBy.
    /// </summary>
    [JsonPropertyName("updatedBy")]
    public FplsLocateRequest UpdatedBy
    {
      get => updatedBy ??= new();
      set => updatedBy = value;
    }

    /// <summary>
    /// A value of FplsLocateResponse.
    /// </summary>
    [JsonPropertyName("fplsLocateResponse")]
    public FplsLocateResponse FplsLocateResponse
    {
      get => fplsLocateResponse ??= new();
      set => fplsLocateResponse = value;
    }

    /// <summary>
    /// A value of FplsLocateRequest.
    /// </summary>
    [JsonPropertyName("fplsLocateRequest")]
    public FplsLocateRequest FplsLocateRequest
    {
      get => fplsLocateRequest ??= new();
      set => fplsLocateRequest = value;
    }

    /// <summary>
    /// A value of DateUpdated.
    /// </summary>
    [JsonPropertyName("dateUpdated")]
    public DateWorkArea DateUpdated
    {
      get => dateUpdated ??= new();
      set => dateUpdated = value;
    }

    /// <summary>
    /// A value of SesaRespondingState.
    /// </summary>
    [JsonPropertyName("sesaRespondingState")]
    public CodeValue SesaRespondingState
    {
      get => sesaRespondingState ??= new();
      set => sesaRespondingState = value;
    }

    /// <summary>
    /// A value of ErrorCodeDesc.
    /// </summary>
    [JsonPropertyName("errorCodeDesc")]
    public CodeValue ErrorCodeDesc
    {
      get => errorCodeDesc ??= new();
      set => errorCodeDesc = value;
    }

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of StartingCase.
    /// </summary>
    [JsonPropertyName("startingCase")]
    public Case1 StartingCase
    {
      get => startingCase ??= new();
      set => startingCase = value;
    }

    /// <summary>
    /// A value of StartingCsePerson.
    /// </summary>
    [JsonPropertyName("startingCsePerson")]
    public CsePerson StartingCsePerson
    {
      get => startingCsePerson ??= new();
      set => startingCsePerson = value;
    }

    /// <summary>
    /// A value of HiddenCsePerson.
    /// </summary>
    [JsonPropertyName("hiddenCsePerson")]
    public CsePerson HiddenCsePerson
    {
      get => hiddenCsePerson ??= new();
      set => hiddenCsePerson = value;
    }

    /// <summary>
    /// A value of StartingCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("startingCsePersonsWorkSet")]
    public CsePersonsWorkSet StartingCsePersonsWorkSet
    {
      get => startingCsePersonsWorkSet ??= new();
      set => startingCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of CsePersonPrompt.
    /// </summary>
    [JsonPropertyName("csePersonPrompt")]
    public Common CsePersonPrompt
    {
      get => csePersonPrompt ??= new();
      set => csePersonPrompt = value;
    }

    /// <summary>
    /// Gets a value of Sesa.
    /// </summary>
    [JsonIgnore]
    public Array<SesaGroup> Sesa => sesa ??= new(SesaGroup.Capacity);

    /// <summary>
    /// Gets a value of Sesa for json serialization.
    /// </summary>
    [JsonPropertyName("sesa")]
    [Computed]
    public IList<SesaGroup> Sesa_Json
    {
      get => sesa;
      set => Sesa.Assign(value);
    }

    /// <summary>
    /// Gets a value of Addr.
    /// </summary>
    [JsonIgnore]
    public Array<AddrGroup> Addr => addr ??= new(AddrGroup.Capacity);

    /// <summary>
    /// Gets a value of Addr for json serialization.
    /// </summary>
    [JsonPropertyName("addr")]
    [Computed]
    public IList<AddrGroup> Addr_Json
    {
      get => addr;
      set => Addr.Assign(value);
    }

    /// <summary>
    /// A value of DodServiceDesc.
    /// </summary>
    [JsonPropertyName("dodServiceDesc")]
    public CodeValue DodServiceDesc
    {
      get => dodServiceDesc ??= new();
      set => dodServiceDesc = value;
    }

    /// <summary>
    /// A value of DodPayGradeDesc.
    /// </summary>
    [JsonPropertyName("dodPayGradeDesc")]
    public CodeValue DodPayGradeDesc
    {
      get => dodPayGradeDesc ??= new();
      set => dodPayGradeDesc = value;
    }

    /// <summary>
    /// A value of DodStatusDesc.
    /// </summary>
    [JsonPropertyName("dodStatusDesc")]
    public CodeValue DodStatusDesc
    {
      get => dodStatusDesc ??= new();
      set => dodStatusDesc = value;
    }

    /// <summary>
    /// A value of DodEligDesc.
    /// </summary>
    [JsonPropertyName("dodEligDesc")]
    public CodeValue DodEligDesc
    {
      get => dodEligDesc ??= new();
      set => dodEligDesc = value;
    }

    /// <summary>
    /// A value of FplsAgency.
    /// </summary>
    [JsonPropertyName("fplsAgency")]
    public CodeValue FplsAgency
    {
      get => fplsAgency ??= new();
      set => fplsAgency = value;
    }

    /// <summary>
    /// A value of FplsResp.
    /// </summary>
    [JsonPropertyName("fplsResp")]
    public CodeValue FplsResp
    {
      get => fplsResp ??= new();
      set => fplsResp = value;
    }

    /// <summary>
    /// A value of ScrollingAttributes.
    /// </summary>
    [JsonPropertyName("scrollingAttributes")]
    public ScrollingAttributes ScrollingAttributes
    {
      get => scrollingAttributes ??= new();
      set => scrollingAttributes = value;
    }

    /// <summary>
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    private TextWorkArea page2Message;
    private Common addressFormatFlag;
    private CsePersonAddress csePersonAddress;
    private Array<FplsAddrGroup> fplsAddr;
    private FplsLocateResponse localEmptyFplsLocateResponse;
    private FplsLocateRequest localEmptyFplsLocateRequest;
    private FplsLocateRequest updatedBy;
    private FplsLocateResponse fplsLocateResponse;
    private FplsLocateRequest fplsLocateRequest;
    private DateWorkArea dateUpdated;
    private CodeValue sesaRespondingState;
    private CodeValue errorCodeDesc;
    private Standard standard;
    private Case1 startingCase;
    private CsePerson startingCsePerson;
    private CsePerson hiddenCsePerson;
    private CsePersonsWorkSet startingCsePersonsWorkSet;
    private Common csePersonPrompt;
    private Array<SesaGroup> sesa;
    private Array<AddrGroup> addr;
    private CodeValue dodServiceDesc;
    private CodeValue dodPayGradeDesc;
    private CodeValue dodStatusDesc;
    private CodeValue dodEligDesc;
    private CodeValue fplsAgency;
    private CodeValue fplsResp;
    private ScrollingAttributes scrollingAttributes;
    private NextTranInfo hiddenNextTranInfo;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A FplsAddrGroup group.</summary>
    [Serializable]
    public class FplsAddrGroup
    {
      /// <summary>
      /// A value of FplsAddrGroupDet.
      /// </summary>
      [JsonPropertyName("fplsAddrGroupDet")]
      public FplsWorkArea FplsAddrGroupDet
      {
        get => fplsAddrGroupDet ??= new();
        set => fplsAddrGroupDet = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private FplsWorkArea fplsAddrGroupDet;
    }

    /// <summary>A SesaGroup group.</summary>
    [Serializable]
    public class SesaGroup
    {
      /// <summary>
      /// A value of SesaGroupDet.
      /// </summary>
      [JsonPropertyName("sesaGroupDet")]
      public OblgWork SesaGroupDet
      {
        get => sesaGroupDet ??= new();
        set => sesaGroupDet = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private OblgWork sesaGroupDet;
    }

    /// <summary>A AddrGroup group.</summary>
    [Serializable]
    public class AddrGroup
    {
      /// <summary>
      /// A value of AddrGroupDetFplsWorkArea.
      /// </summary>
      [JsonPropertyName("addrGroupDetFplsWorkArea")]
      public FplsWorkArea AddrGroupDetFplsWorkArea
      {
        get => addrGroupDetFplsWorkArea ??= new();
        set => addrGroupDetFplsWorkArea = value;
      }

      /// <summary>
      /// A value of AddrGroupDetWorkArea.
      /// </summary>
      [JsonPropertyName("addrGroupDetWorkArea")]
      public WorkArea AddrGroupDetWorkArea
      {
        get => addrGroupDetWorkArea ??= new();
        set => addrGroupDetWorkArea = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private FplsWorkArea addrGroupDetFplsWorkArea;
      private WorkArea addrGroupDetWorkArea;
    }

    /// <summary>
    /// A value of Page2Message.
    /// </summary>
    [JsonPropertyName("page2Message")]
    public TextWorkArea Page2Message
    {
      get => page2Message ??= new();
      set => page2Message = value;
    }

    /// <summary>
    /// A value of AddressFormatFlag.
    /// </summary>
    [JsonPropertyName("addressFormatFlag")]
    public Common AddressFormatFlag
    {
      get => addressFormatFlag ??= new();
      set => addressFormatFlag = value;
    }

    /// <summary>
    /// A value of Addr1.
    /// </summary>
    [JsonPropertyName("addr1")]
    public CsePersonsWorkSet Addr1
    {
      get => addr1 ??= new();
      set => addr1 = value;
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

    /// <summary>
    /// Gets a value of FplsAddr.
    /// </summary>
    [JsonIgnore]
    public Array<FplsAddrGroup> FplsAddr => fplsAddr ??= new(
      FplsAddrGroup.Capacity);

    /// <summary>
    /// Gets a value of FplsAddr for json serialization.
    /// </summary>
    [JsonPropertyName("fplsAddr")]
    [Computed]
    public IList<FplsAddrGroup> FplsAddr_Json
    {
      get => fplsAddr;
      set => FplsAddr.Assign(value);
    }

    /// <summary>
    /// A value of UpdatedBy.
    /// </summary>
    [JsonPropertyName("updatedBy")]
    public FplsLocateRequest UpdatedBy
    {
      get => updatedBy ??= new();
      set => updatedBy = value;
    }

    /// <summary>
    /// A value of DateUpdated.
    /// </summary>
    [JsonPropertyName("dateUpdated")]
    public DateWorkArea DateUpdated
    {
      get => dateUpdated ??= new();
      set => dateUpdated = value;
    }

    /// <summary>
    /// A value of FplsLocateRequest.
    /// </summary>
    [JsonPropertyName("fplsLocateRequest")]
    public FplsLocateRequest FplsLocateRequest
    {
      get => fplsLocateRequest ??= new();
      set => fplsLocateRequest = value;
    }

    /// <summary>
    /// A value of SesaRespondingState.
    /// </summary>
    [JsonPropertyName("sesaRespondingState")]
    public CodeValue SesaRespondingState
    {
      get => sesaRespondingState ??= new();
      set => sesaRespondingState = value;
    }

    /// <summary>
    /// A value of ErrorCodeDesc.
    /// </summary>
    [JsonPropertyName("errorCodeDesc")]
    public CodeValue ErrorCodeDesc
    {
      get => errorCodeDesc ??= new();
      set => errorCodeDesc = value;
    }

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of StartingCase.
    /// </summary>
    [JsonPropertyName("startingCase")]
    public Case1 StartingCase
    {
      get => startingCase ??= new();
      set => startingCase = value;
    }

    /// <summary>
    /// A value of StartingCsePerson.
    /// </summary>
    [JsonPropertyName("startingCsePerson")]
    public CsePerson StartingCsePerson
    {
      get => startingCsePerson ??= new();
      set => startingCsePerson = value;
    }

    /// <summary>
    /// A value of HiddenCsePerson.
    /// </summary>
    [JsonPropertyName("hiddenCsePerson")]
    public CsePerson HiddenCsePerson
    {
      get => hiddenCsePerson ??= new();
      set => hiddenCsePerson = value;
    }

    /// <summary>
    /// A value of StartingCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("startingCsePersonsWorkSet")]
    public CsePersonsWorkSet StartingCsePersonsWorkSet
    {
      get => startingCsePersonsWorkSet ??= new();
      set => startingCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of CsePersonPrompt.
    /// </summary>
    [JsonPropertyName("csePersonPrompt")]
    public Common CsePersonPrompt
    {
      get => csePersonPrompt ??= new();
      set => csePersonPrompt = value;
    }

    /// <summary>
    /// Gets a value of Sesa.
    /// </summary>
    [JsonIgnore]
    public Array<SesaGroup> Sesa => sesa ??= new(SesaGroup.Capacity);

    /// <summary>
    /// Gets a value of Sesa for json serialization.
    /// </summary>
    [JsonPropertyName("sesa")]
    [Computed]
    public IList<SesaGroup> Sesa_Json
    {
      get => sesa;
      set => Sesa.Assign(value);
    }

    /// <summary>
    /// A value of FplsLocateResponse.
    /// </summary>
    [JsonPropertyName("fplsLocateResponse")]
    public FplsLocateResponse FplsLocateResponse
    {
      get => fplsLocateResponse ??= new();
      set => fplsLocateResponse = value;
    }

    /// <summary>
    /// Gets a value of Addr.
    /// </summary>
    [JsonIgnore]
    public Array<AddrGroup> Addr => addr ??= new(AddrGroup.Capacity);

    /// <summary>
    /// Gets a value of Addr for json serialization.
    /// </summary>
    [JsonPropertyName("addr")]
    [Computed]
    public IList<AddrGroup> Addr_Json
    {
      get => addr;
      set => Addr.Assign(value);
    }

    /// <summary>
    /// A value of DodServiceDesc.
    /// </summary>
    [JsonPropertyName("dodServiceDesc")]
    public CodeValue DodServiceDesc
    {
      get => dodServiceDesc ??= new();
      set => dodServiceDesc = value;
    }

    /// <summary>
    /// A value of DodPayGradeDesc.
    /// </summary>
    [JsonPropertyName("dodPayGradeDesc")]
    public CodeValue DodPayGradeDesc
    {
      get => dodPayGradeDesc ??= new();
      set => dodPayGradeDesc = value;
    }

    /// <summary>
    /// A value of DodStatusDesc.
    /// </summary>
    [JsonPropertyName("dodStatusDesc")]
    public CodeValue DodStatusDesc
    {
      get => dodStatusDesc ??= new();
      set => dodStatusDesc = value;
    }

    /// <summary>
    /// A value of DodEligDesc.
    /// </summary>
    [JsonPropertyName("dodEligDesc")]
    public CodeValue DodEligDesc
    {
      get => dodEligDesc ??= new();
      set => dodEligDesc = value;
    }

    /// <summary>
    /// A value of FplsAgency.
    /// </summary>
    [JsonPropertyName("fplsAgency")]
    public CodeValue FplsAgency
    {
      get => fplsAgency ??= new();
      set => fplsAgency = value;
    }

    /// <summary>
    /// A value of FplsResp.
    /// </summary>
    [JsonPropertyName("fplsResp")]
    public CodeValue FplsResp
    {
      get => fplsResp ??= new();
      set => fplsResp = value;
    }

    /// <summary>
    /// A value of ScrollingAttributes.
    /// </summary>
    [JsonPropertyName("scrollingAttributes")]
    public ScrollingAttributes ScrollingAttributes
    {
      get => scrollingAttributes ??= new();
      set => scrollingAttributes = value;
    }

    /// <summary>
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    /// <summary>
    /// A value of PersonName.
    /// </summary>
    [JsonPropertyName("personName")]
    public CsePersonsWorkSet PersonName
    {
      get => personName ??= new();
      set => personName = value;
    }

    /// <summary>
    /// A value of Data1099LocateRequest.
    /// </summary>
    [JsonPropertyName("data1099LocateRequest")]
    public Data1099LocateRequest Data1099LocateRequest
    {
      get => data1099LocateRequest ??= new();
      set => data1099LocateRequest = value;
    }

    /// <summary>
    /// A value of Data1099LocateResponse.
    /// </summary>
    [JsonPropertyName("data1099LocateResponse")]
    public Data1099LocateResponse Data1099LocateResponse
    {
      get => data1099LocateResponse ??= new();
      set => data1099LocateResponse = value;
    }

    private TextWorkArea page2Message;
    private Common addressFormatFlag;
    private CsePersonsWorkSet addr1;
    private CsePersonAddress csePersonAddress;
    private Array<FplsAddrGroup> fplsAddr;
    private FplsLocateRequest updatedBy;
    private DateWorkArea dateUpdated;
    private FplsLocateRequest fplsLocateRequest;
    private CodeValue sesaRespondingState;
    private CodeValue errorCodeDesc;
    private Standard standard;
    private Case1 startingCase;
    private CsePerson startingCsePerson;
    private CsePerson hiddenCsePerson;
    private CsePersonsWorkSet startingCsePersonsWorkSet;
    private Common csePersonPrompt;
    private Array<SesaGroup> sesa;
    private FplsLocateResponse fplsLocateResponse;
    private Array<AddrGroup> addr;
    private CodeValue dodServiceDesc;
    private CodeValue dodPayGradeDesc;
    private CodeValue dodStatusDesc;
    private CodeValue dodEligDesc;
    private CodeValue fplsAgency;
    private CodeValue fplsResp;
    private ScrollingAttributes scrollingAttributes;
    private NextTranInfo hiddenNextTranInfo;
    private CsePersonsWorkSet personName;
    private Data1099LocateRequest data1099LocateRequest;
    private Data1099LocateResponse data1099LocateResponse;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A FplsAddrGroup group.</summary>
    [Serializable]
    public class FplsAddrGroup
    {
      /// <summary>
      /// A value of FplsAddrGroupDet.
      /// </summary>
      [JsonPropertyName("fplsAddrGroupDet")]
      public FplsWorkArea FplsAddrGroupDet
      {
        get => fplsAddrGroupDet ??= new();
        set => fplsAddrGroupDet = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private FplsWorkArea fplsAddrGroupDet;
    }

    /// <summary>A SesaGroup group.</summary>
    [Serializable]
    public class SesaGroup
    {
      /// <summary>
      /// A value of SesaGroupDet.
      /// </summary>
      [JsonPropertyName("sesaGroupDet")]
      public OblgWork SesaGroupDet
      {
        get => sesaGroupDet ??= new();
        set => sesaGroupDet = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private OblgWork sesaGroupDet;
    }

    /// <summary>A AddrGroup group.</summary>
    [Serializable]
    public class AddrGroup
    {
      /// <summary>
      /// A value of AddrGroupDetFplsWorkArea.
      /// </summary>
      [JsonPropertyName("addrGroupDetFplsWorkArea")]
      public FplsWorkArea AddrGroupDetFplsWorkArea
      {
        get => addrGroupDetFplsWorkArea ??= new();
        set => addrGroupDetFplsWorkArea = value;
      }

      /// <summary>
      /// A value of AddrGroupDetWorkArea.
      /// </summary>
      [JsonPropertyName("addrGroupDetWorkArea")]
      public WorkArea AddrGroupDetWorkArea
      {
        get => addrGroupDetWorkArea ??= new();
        set => addrGroupDetWorkArea = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private FplsWorkArea addrGroupDetFplsWorkArea;
      private WorkArea addrGroupDetWorkArea;
    }

    /// <summary>
    /// A value of ArCaseNumber.
    /// </summary>
    [JsonPropertyName("arCaseNumber")]
    public Case1 ArCaseNumber
    {
      get => arCaseNumber ??= new();
      set => arCaseNumber = value;
    }

    /// <summary>
    /// A value of ApCaseNumber.
    /// </summary>
    [JsonPropertyName("apCaseNumber")]
    public Case1 ApCaseNumber
    {
      get => apCaseNumber ??= new();
      set => apCaseNumber = value;
    }

    /// <summary>
    /// A value of ProcessPage2.
    /// </summary>
    [JsonPropertyName("processPage2")]
    public Common ProcessPage2
    {
      get => processPage2 ??= new();
      set => processPage2 = value;
    }

    /// <summary>
    /// A value of ArCount.
    /// </summary>
    [JsonPropertyName("arCount")]
    public Common ArCount
    {
      get => arCount ??= new();
      set => arCount = value;
    }

    /// <summary>
    /// A value of ApCount.
    /// </summary>
    [JsonPropertyName("apCount")]
    public Common ApCount
    {
      get => apCount ??= new();
      set => apCount = value;
    }

    /// <summary>
    /// A value of ArCaseFound.
    /// </summary>
    [JsonPropertyName("arCaseFound")]
    public Common ArCaseFound
    {
      get => arCaseFound ??= new();
      set => arCaseFound = value;
    }

    /// <summary>
    /// A value of ApCaseFound.
    /// </summary>
    [JsonPropertyName("apCaseFound")]
    public Common ApCaseFound
    {
      get => apCaseFound ??= new();
      set => apCaseFound = value;
    }

    /// <summary>
    /// A value of Street2.
    /// </summary>
    [JsonPropertyName("street2")]
    public FplsLocateResponse Street2
    {
      get => street2 ??= new();
      set => street2 = value;
    }

    /// <summary>
    /// A value of Street1.
    /// </summary>
    [JsonPropertyName("street1")]
    public FplsLocateResponse Street1
    {
      get => street1 ??= new();
      set => street1 = value;
    }

    /// <summary>
    /// A value of TotalCount.
    /// </summary>
    [JsonPropertyName("totalCount")]
    public Common TotalCount
    {
      get => totalCount ??= new();
      set => totalCount = value;
    }

    /// <summary>
    /// A value of EndPointer.
    /// </summary>
    [JsonPropertyName("endPointer")]
    public Common EndPointer
    {
      get => endPointer ??= new();
      set => endPointer = value;
    }

    /// <summary>
    /// A value of ReturnedAddress.
    /// </summary>
    [JsonPropertyName("returnedAddress")]
    public FplsLocateResponse ReturnedAddress
    {
      get => returnedAddress ??= new();
      set => returnedAddress = value;
    }

    /// <summary>
    /// A value of CaseFound.
    /// </summary>
    [JsonPropertyName("caseFound")]
    public Common CaseFound
    {
      get => caseFound ??= new();
      set => caseFound = value;
    }

    /// <summary>
    /// Gets a value of FplsAddr.
    /// </summary>
    [JsonIgnore]
    public Array<FplsAddrGroup> FplsAddr => fplsAddr ??= new(
      FplsAddrGroup.Capacity);

    /// <summary>
    /// Gets a value of FplsAddr for json serialization.
    /// </summary>
    [JsonPropertyName("fplsAddr")]
    [Computed]
    public IList<FplsAddrGroup> FplsAddr_Json
    {
      get => fplsAddr;
      set => FplsAddr.Assign(value);
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
    /// A value of UpdatedBy.
    /// </summary>
    [JsonPropertyName("updatedBy")]
    public FplsLocateRequest UpdatedBy
    {
      get => updatedBy ??= new();
      set => updatedBy = value;
    }

    /// <summary>
    /// A value of FplsLocateResponse.
    /// </summary>
    [JsonPropertyName("fplsLocateResponse")]
    public FplsLocateResponse FplsLocateResponse
    {
      get => fplsLocateResponse ??= new();
      set => fplsLocateResponse = value;
    }

    /// <summary>
    /// A value of DateUpdated.
    /// </summary>
    [JsonPropertyName("dateUpdated")]
    public DateWorkArea DateUpdated
    {
      get => dateUpdated ??= new();
      set => dateUpdated = value;
    }

    /// <summary>
    /// A value of FplsAgency.
    /// </summary>
    [JsonPropertyName("fplsAgency")]
    public CodeValue FplsAgency
    {
      get => fplsAgency ??= new();
      set => fplsAgency = value;
    }

    /// <summary>
    /// A value of FplsResp.
    /// </summary>
    [JsonPropertyName("fplsResp")]
    public CodeValue FplsResp
    {
      get => fplsResp ??= new();
      set => fplsResp = value;
    }

    /// <summary>
    /// A value of DodServiceDesc.
    /// </summary>
    [JsonPropertyName("dodServiceDesc")]
    public CodeValue DodServiceDesc
    {
      get => dodServiceDesc ??= new();
      set => dodServiceDesc = value;
    }

    /// <summary>
    /// A value of DodPayGradeDesc.
    /// </summary>
    [JsonPropertyName("dodPayGradeDesc")]
    public CodeValue DodPayGradeDesc
    {
      get => dodPayGradeDesc ??= new();
      set => dodPayGradeDesc = value;
    }

    /// <summary>
    /// A value of DodStatusDesc.
    /// </summary>
    [JsonPropertyName("dodStatusDesc")]
    public CodeValue DodStatusDesc
    {
      get => dodStatusDesc ??= new();
      set => dodStatusDesc = value;
    }

    /// <summary>
    /// A value of DodEligDesc.
    /// </summary>
    [JsonPropertyName("dodEligDesc")]
    public CodeValue DodEligDesc
    {
      get => dodEligDesc ??= new();
      set => dodEligDesc = value;
    }

    /// <summary>
    /// Gets a value of Sesa.
    /// </summary>
    [JsonIgnore]
    public Array<SesaGroup> Sesa => sesa ??= new(SesaGroup.Capacity);

    /// <summary>
    /// Gets a value of Sesa for json serialization.
    /// </summary>
    [JsonPropertyName("sesa")]
    [Computed]
    public IList<SesaGroup> Sesa_Json
    {
      get => sesa;
      set => Sesa.Assign(value);
    }

    /// <summary>
    /// Gets a value of Addr.
    /// </summary>
    [JsonIgnore]
    public Array<AddrGroup> Addr => addr ??= new(AddrGroup.Capacity);

    /// <summary>
    /// Gets a value of Addr for json serialization.
    /// </summary>
    [JsonPropertyName("addr")]
    [Computed]
    public IList<AddrGroup> Addr_Json
    {
      get => addr;
      set => Addr.Assign(value);
    }

    /// <summary>
    /// A value of ScrollingAttributes.
    /// </summary>
    [JsonPropertyName("scrollingAttributes")]
    public ScrollingAttributes ScrollingAttributes
    {
      get => scrollingAttributes ??= new();
      set => scrollingAttributes = value;
    }

    /// <summary>
    /// A value of FplsLocateRequest.
    /// </summary>
    [JsonPropertyName("fplsLocateRequest")]
    public FplsLocateRequest FplsLocateRequest
    {
      get => fplsLocateRequest ??= new();
      set => fplsLocateRequest = value;
    }

    /// <summary>
    /// A value of WorkError.
    /// </summary>
    [JsonPropertyName("workError")]
    public Common WorkError
    {
      get => workError ??= new();
      set => workError = value;
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
    /// A value of SesaError.
    /// </summary>
    [JsonPropertyName("sesaError")]
    public Common SesaError
    {
      get => sesaError ??= new();
      set => sesaError = value;
    }

    /// <summary>
    /// A value of UserAction.
    /// </summary>
    [JsonPropertyName("userAction")]
    public Common UserAction
    {
      get => userAction ??= new();
      set => userAction = value;
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
    /// A value of ValidCode.
    /// </summary>
    [JsonPropertyName("validCode")]
    public Common ValidCode
    {
      get => validCode ??= new();
      set => validCode = value;
    }

    /// <summary>
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    /// <summary>
    /// A value of StartCsePersonWithZero.
    /// </summary>
    [JsonPropertyName("startCsePersonWithZero")]
    public TextWorkArea StartCsePersonWithZero
    {
      get => startCsePersonWithZero ??= new();
      set => startCsePersonWithZero = value;
    }

    /// <summary>
    /// A value of StartCsePerson.
    /// </summary>
    [JsonPropertyName("startCsePerson")]
    public TextWorkArea StartCsePerson
    {
      get => startCsePerson ??= new();
      set => startCsePerson = value;
    }

    /// <summary>
    /// A value of OnTheCase.
    /// </summary>
    [JsonPropertyName("onTheCase")]
    public Common OnTheCase
    {
      get => onTheCase ??= new();
      set => onTheCase = value;
    }

    private Case1 arCaseNumber;
    private Case1 apCaseNumber;
    private Common processPage2;
    private Common arCount;
    private Common apCount;
    private Common arCaseFound;
    private Common apCaseFound;
    private FplsLocateResponse street2;
    private FplsLocateResponse street1;
    private Common totalCount;
    private Common endPointer;
    private FplsLocateResponse returnedAddress;
    private Common caseFound;
    private Array<FplsAddrGroup> fplsAddr;
    private DateWorkArea nullDate;
    private FplsLocateRequest updatedBy;
    private FplsLocateResponse fplsLocateResponse;
    private DateWorkArea dateUpdated;
    private CodeValue fplsAgency;
    private CodeValue fplsResp;
    private CodeValue dodServiceDesc;
    private CodeValue dodPayGradeDesc;
    private CodeValue dodStatusDesc;
    private CodeValue dodEligDesc;
    private Array<SesaGroup> sesa;
    private Array<AddrGroup> addr;
    private ScrollingAttributes scrollingAttributes;
    private FplsLocateRequest fplsLocateRequest;
    private Common workError;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common sesaError;
    private Common userAction;
    private Code code;
    private CodeValue codeValue;
    private Common validCode;
    private TextWorkArea textWorkArea;
    private TextWorkArea startCsePersonWithZero;
    private TextWorkArea startCsePerson;
    private Common onTheCase;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of ExistingCsePerson.
    /// </summary>
    [JsonPropertyName("existingCsePerson")]
    public CsePerson ExistingCsePerson
    {
      get => existingCsePerson ??= new();
      set => existingCsePerson = value;
    }

    /// <summary>
    /// A value of ExistingCaseRole.
    /// </summary>
    [JsonPropertyName("existingCaseRole")]
    public CaseRole ExistingCaseRole
    {
      get => existingCaseRole ??= new();
      set => existingCaseRole = value;
    }

    /// <summary>
    /// A value of ExistingCase.
    /// </summary>
    [JsonPropertyName("existingCase")]
    public Case1 ExistingCase
    {
      get => existingCase ??= new();
      set => existingCase = value;
    }

    private CaseAssignment caseAssignment;
    private OfficeServiceProvider officeServiceProvider;
    private Office office;
    private ServiceProvider serviceProvider;
    private CsePerson existingCsePerson;
    private CaseRole existingCaseRole;
    private Case1 existingCase;
  }
#endregion
}
