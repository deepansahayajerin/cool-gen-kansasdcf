// Program: SP_ASIN_OSP_ASSIGNMENT, ID: 372317573, model: 746.
// Short name: SWEASINP
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
/// A program: SP_ASIN_OSP_ASSIGNMENT.
/// </para>
/// <para>
/// This PrAD is used to display, add, update the following business object 
/// assignments:
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpAsinOspAssignment: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_ASIN_OSP_ASSIGNMENT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpAsinOspAssignment(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpAsinOspAssignment.
  /// </summary>
  public SpAsinOspAssignment(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------------------------
    // Date	  Developer	Request #	Description
    // 10/24/96 Rick Delgado               Initial Development
    // 01/22/97 Govindaraj		    Major clean up of the procedure
    // 02/13/97 Raju - MTW		    LGRQ , ASIN
    // 				    made 1 transaction
    // 02/24/97  Siraj Konkader            Fixes to case reassign
    //                                     
    // Removed logic for LGRQ as it was
    // handled differently. Cleanup.
    // 03/10/97  Siraj Konkader            Added logic for LGRQ back.
    // 04/16/97  Siraj Konkader            Attorney codes are more than AT. Use 
    // Code Value.
    // 05/15/97  Siraj Konkader            Provide for Obligation assignment.
    // 11/13/97  Jack Rookard              Remove code supporting assignability 
    // for
    // Obligation Administrative Action occurrences.  Approved by Jan Brigham, 
    // Loren Benoit, and Tricia Barker.
    // 12/29/98  Anita Massey             changes per screen assess form
    // 03/30/99	PMcElderry
    // Logic for Event 95 - monitored activity assignments
    // associated to the creation of the first legal action on a case.
    // 05/18/99	PMcElderry
    // Added CSEnet functionality - create RS code GSWKR.
    // ---------------------------------------------------------------
    // -------------------------------------------------------------------
    // Date		Developer	Request #	Description
    // -------------------------------------------------------------------
    // 08/25/1999	M Ramirez	516		Pass in infrastructure reference_date
    // 						to sp_create_document_infrastruct for
    // 						CASETXFR document so that it will be
    // 						printed when then assignment is
    // 						effective.
    // 09/17/1999	M Ramirez	H00073450	Don't send the document if
    // 						the AR is not a client
    // 10/28/99		R. Jean
    // PR#H78286,78782 - Test for P, F, M, O, E, U when building infrastructure 
    // records.
    // -------------------------------------------------------------------
    // ***********************************************************************************************
    //   Date   Developer   Request #   Description
    // -----------------------------------------------------------------------------------------------
    // 02/10/00 SWSRCHF     H00082885   MONA(s) are not being transferred
    //                                  
    // with the Legal Action or the
    //                                  
    // Legal Referral
    // 08/06/00 SWDPARM    H00099763    abending when trying to reassign legal 
    // req
    // 09/14/00 GVandy     H00103139    Allow U class legal actions to be 
    // assigned
    // 				 to non-attorney service providers.
    // 11/21/00 SWSRCHF     000299      Don't force pick list for the Service 
    // Provider
    // 01/30/01 SWSRCHF    I00111815    Only assign cases to workers who have a
    //                                  
    // role code of "CO", 'SS" and "CH"
    // 04/26/01 GVandy     WR251   	 Legal Referral Changes - initial assignment
    // is now done on LGRQ.
    // 				 On an add, end date the previous legal referral assignment;
    // 				 default values for effective date, reason code, and override 
    // indicator;
    // 				 override indicator is now the only updatable field.
    // 05/22/01 GVandy     PR 120354	 Correct WR251 changes which caused 
    // effective and end date edits for
    // 				 business objects other than legal referrals to no longer work 
    // correctly.
    // 07/27/01 M.Lachowicz PR 124228	 Improve performance.
    // 12/20/01 GVandy     PR 125378    Allow N class legal actions to be 
    // assigned
    // 				 to non-attorney service providers.
    // 03/01/02 Mashworth  PR 138467    Added use of cab sp determine interstate
    // doc. PR 138467 - Do not send casetxfr letter if incoming interstate case
    // 07/18/2002 VMadhira PR 152294    ASIN should raise event 95 if the 
    // assignment is the first assignment to the legal action, instead of
    // raising event 95 only if the legal action is the only action for the
    // court case.
    // 09/25/02 GVandy     PR 158263    Do not raise event 95 on an update of a 
    // legal action assignment.
    // 02/17/06 GVandy     PR205434	Remove edit requiring service provider be an
    // attorney on UPDATEs.
    // 05/05/08  G. Pan	CQ4749 	Save subscript when the list is full. Without 
    // this statement it caused ASIN screen getting ASRA Abend.
    // 03/03/09 Arun Mathias CQ#9424 	Display Error message when adding same 
    // timeframe.
    // 12/03/10 GVandy      CQ109      Correct error causing legal action events
    // to not be raised for all case units.
    // 11/17/11 GVandy    CQ30161      Do not trigger CASETXFR letters.  The 
    // letters will now be
    // 				triggered by new batch program SP_B703_CASETXFR_GENERATION.
    // 02/10/14 GVandy    CQ42566      Performance fix when updating Legal 
    // Action assignments..
    // 02/20/14 LSS       CQ41892      Validate discontinue date - cannot be 
    // greater than current date + 1 day
    // ***************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "SIGNOFF"))
    {
      UseScCabSignoff();
    }
    else
    {
    }

    local.MaxDate.Date = UseCabSetMaximumDiscontinueDate();
    local.AddCommand.Flag = "N";

    // ************************************************************
    // * Use this view in place of CURRENT DATE.
    // * CURRENT_TIMESTAMP added 4/14/00
    // ************************************************************
    local.CurrentDate.Date = Now().Date;
    local.CurrentDate.Timestamp = Now();

    // ****  HouseKeeping - roll import to export  ****
    export.HeaderObjTitle1.Text80 = import.HeaderObjTitle1.Text80;
    export.HeaderObjTitle2.Text80 = import.HeaderObjTitle2.Text80;
    export.HeaderObject.Text20 = import.HeaderObject.Text20;
    export.HeaderStart.Date = import.HeaderStart.Date;
    export.FirstTime.Flag = import.FirstTime.Flag;
    export.Pointer.Count = import.Pointer.Count;
    export.HiddenOffice.SystemGeneratedId =
      import.HiddenOffice.SystemGeneratedId;
    MoveOfficeServiceProvider(import.HiddenOfficeServiceProvider,
      export.HiddenOfficeServiceProvider);
    MoveServiceProvider(import.HiddenServiceProvider,
      export.HiddenServiceProvider);
    export.HiddenCode.CodeName = import.HiddenCode.CodeName;
    export.HiddenCodeValue.Cdvalue = import.HiddenCodeValue.Cdvalue;
    export.HadministrativeAction.Type1 = import.HadministrativeAction.Type1;
    export.HadministrativeAppeal.Identifier =
      import.HadministrativeAppeal.Identifier;
    export.Hcase.Number = import.Hcase.Number;
    export.HcaseUnit.CuNumber = import.HcaseUnit.CuNumber;
    export.HcsePerson.Number = import.HcsePerson.Number;
    export.HcsePersonAccount.Type1 = import.HcsePersonAccount.Type1;
    export.HlegalAction.Assign(import.HlegalAction);
    export.Htribunal.Identifier = import.Htribunal.Identifier;
    export.HlegalReferral.Identifier = import.HlegalReferral.Identifier;
    MoveMonitoredActivity(import.HmonitoredActivity, export.HmonitoredActivity);
    export.Hobligation.SystemGeneratedIdentifier =
      import.Hobligation.SystemGeneratedIdentifier;
    MoveObligationType(import.HobligationType, export.HobligationType);
    export.HpaReferral.Assign(import.HpaReferral);
    export.HcsePersonsWorkSet.FormattedName =
      import.HcsePersonsWorkSet.FormattedName;

    // *** Problem report H00082885
    // *** 02/10/00 SWSRCHF
    // *** start
    export.OldOffice.SystemGeneratedId = import.OldOffice.SystemGeneratedId;
    export.OldServiceProvider.SystemGeneratedId =
      import.OldServiceProvider.SystemGeneratedId;
    MoveOfficeServiceProvider(import.OldOfficeServiceProvider,
      export.OldOfficeServiceProvider);
    export.OldLegalReferralAssignment.DiscontinueDate =
      import.OldLegalReferralAssignment.DiscontinueDate;
    export.OldLegalActionAssigment.DiscontinueDate =
      import.OldLegalActionAssigment.DiscontinueDate;
    export.Detail.Text32 = import.Detail.Text32;
    export.Compare.FiledDate = import.Compare.FiledDate;

    // *** end
    // *** 02/10/00 SWSRCHF
    // *** Problem report H00082885
    export.Group.Index = -1;

    for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
      import.Group.Index)
    {
      ++export.Group.Index;
      export.Group.CheckSize();

      export.Group.Update.Common.SelectChar =
        import.Group.Item.Common.SelectChar;
      export.Group.Update.PromptFunc.PromptField =
        import.Group.Item.PromptFunc.PromptField;
      export.Group.Update.PromptRsn.PromptField =
        import.Group.Item.PromptRsn.PromptField;
      export.Group.Update.PromptSp.PromptField =
        import.Group.Item.PromptSp.PromptField;
      export.Group.Update.CaseUnitFunctionAssignmt.Assign(
        import.Group.Item.CaseUnitFunctionAssignmt);

      if (Equal(export.Group.Item.CaseUnitFunctionAssignmt.DiscontinueDate,
        local.NullDate.Date))
      {
        export.Group.Update.CaseUnitFunctionAssignmt.DiscontinueDate =
          local.MaxDate.Date;
      }

      export.Group.Update.Office.SystemGeneratedId =
        import.Group.Item.Office.SystemGeneratedId;
      MoveOfficeServiceProvider(import.Group.Item.OfficeServiceProvider,
        export.Group.Update.OfficeServiceProvider);
      MoveServiceProvider(import.Group.Item.ServiceProvider,
        export.Group.Update.ServiceProvider);
      export.Group.Update.HcaseUnitFunctionAssignmt.Assign(
        import.Group.Item.HcaseUnitFunctionAssignmt);

      if (Equal(export.Group.Item.HcaseUnitFunctionAssignmt.DiscontinueDate,
        local.NullDate.Date))
      {
        export.Group.Update.HcaseUnitFunctionAssignmt.DiscontinueDate =
          local.MaxDate.Date;
      }

      export.Group.Update.Hoffice.SystemGeneratedId =
        import.Group.Item.Hoffice.SystemGeneratedId;
      export.Group.Update.HofficeServiceProvider.RoleCode =
        import.Group.Item.HofficeServiceProvider.RoleCode;
      export.Group.Update.HserviceProvider.UserId =
        import.Group.Item.HserviceProvider.UserId;
      export.Group.Update.Protect.Flag = import.Group.Item.Protect.Flag;
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);
    local.NextTranInfo.Assign(import.HiddenNextTranInfo);

    // **** End of HouseKeeping   ****
    // ****  Validate All Commands  ****
    switch(TrimEnd(global.Command))
    {
      case "ADD":
        // -------------------
        // continue processing
        // -------------------
        break;
      case "DISPLAY":
        // -------------------
        // continue processing
        // -------------------
        break;
      case "ENTER":
        if (IsEmpty(import.Standard.NextTransaction))
        {
          ExitState = "ACO_NE0000_INVALID_COMMAND";
        }
        else
        {
          // ---------------------------------------------
          // User is going out of this screen to another
          // ---------------------------------------------
          UseScCabNextTranPut();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            var field = GetField(export.Standard, "nextTransaction");

            field.Error = true;

            break;
          }

          return;
        }

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      case "LIST":
        // -------------------
        // continue processing
        // -------------------
        break;
      case "RETURN":
        if (Equal(export.HeaderObject.Text20, "LEGAL REFERRAL"))
        {
          // ************************************************************
          // * Per IDCR 276, ASIN should not return to LGRQ until a valid
          // * assignment exists
          // ************************************************************
          // ===============================================
          // 4/14/00 - bud adams  -  PR# K00302: Performance.
          //   Replaced (effective_dt <= current OR effective_dt =
          //   current + 1).
          // ===============================================
          local.SelectDateWorkArea.Date = AddDays(local.CurrentDate.Date, 1);

          if (ReadLegalReferralAssignment1())
          {
            // Should this change? Carl Galka 01/06/2000. The read each includes
            // DISCONTINUE DATE. Should we only check discontinue date if the
            // REFERRAL is in Sent Status?
            // ************************************************************
            // * Found a valid assignment, continue......
            // ************************************************************
            goto Test1;
          }

          ExitState = "OE0143_ASSIGN_ATTORNEY_4_LGRQ";

          break;
        }

Test1:

        ExitState = "ACO_NE0000_RETURN";

        return;
      case "RETSVPO":
        export.Group.Index = export.Pointer.Count - 1;
        export.Group.CheckSize();

        export.Group.Update.PromptSp.PromptField = "";

        var field1 = GetField(export.Group.Item.PromptSp, "promptField");

        field1.Protected = false;
        field1.Focused = true;

        if (export.HiddenOffice.SystemGeneratedId != 0)
        {
          // *** Work request 000299
          // *** 11/21/00 SWSRCHF
          // *** start
          if (IsEmpty(export.HiddenServiceProvider.UserId) && IsEmpty
            (export.HiddenOfficeServiceProvider.RoleCode))
          {
            export.HiddenOffice.SystemGeneratedId = 0;

            break;
          }

          export.Group.Update.Hoffice.SystemGeneratedId =
            export.HiddenOffice.SystemGeneratedId;
          export.Group.Update.HserviceProvider.UserId =
            export.HiddenServiceProvider.UserId;
          export.Group.Update.HofficeServiceProvider.RoleCode =
            export.HiddenOfficeServiceProvider.RoleCode;

          // *** end
          // *** 11/21/00 SWSRCHF
          // *** Work request 000299
          export.Group.Update.Office.SystemGeneratedId =
            export.HiddenOffice.SystemGeneratedId;
          MoveServiceProvider(export.HiddenServiceProvider,
            export.Group.Update.ServiceProvider);
          MoveOfficeServiceProvider(export.HiddenOfficeServiceProvider,
            export.Group.Update.OfficeServiceProvider);

          // *** Problem report I00111815
          // *** 01/30/01 SWSRCHF
          // *** start
          if (Equal(export.HeaderObject.Text20, "CASE"))
          {
            switch(TrimEnd(export.HiddenOfficeServiceProvider.RoleCode))
            {
              case "CH":
                break;
              case "CO":
                break;
              case "SS":
                break;
              default:
                var field3 =
                  GetField(export.Group.Item.PromptSp, "promptField");

                field3.Color = "red";
                field3.Protected = false;
                field3.Focused = true;

                var field4 =
                  GetField(export.Group.Item.OfficeServiceProvider, "roleCode");
                  

                field4.Error = true;

                ExitState = "SP0000_OSP_MUST_BE_A_CH_CO_SS";

                break;
            }
          }
          else
          {
          }

          // *** end
          // *** 01/30/01 SWSRCHF
          // *** Problem report I00111815
        }

        break;
      case "RETCDVL":
        export.Group.Index = export.Pointer.Count - 1;
        export.Group.CheckSize();

        var field2 = GetField(export.Group.Item.Common, "selectChar");

        field2.Protected = false;

        if (AsChar(export.Group.Item.PromptFunc.PromptField) == 'S')
        {
          export.Group.Update.PromptFunc.PromptField = "";

          var field = GetField(export.Group.Item.PromptFunc, "promptField");

          field.Protected = false;
          field.Focused = true;

          if (!IsEmpty(import.HiddenCodeValue.Cdvalue))
          {
            export.Group.Update.CaseUnitFunctionAssignmt.Function =
              import.HiddenCodeValue.Cdvalue;
          }
        }
        else if (AsChar(export.Group.Item.PromptRsn.PromptField) == 'S')
        {
          export.Group.Update.PromptRsn.PromptField = "";

          var field = GetField(export.Group.Item.PromptRsn, "promptField");

          field.Protected = false;
          field.Focused = true;

          if (!IsEmpty(import.HiddenCodeValue.Cdvalue))
          {
            export.Group.Update.CaseUnitFunctionAssignmt.ReasonCode =
              import.HiddenCodeValue.Cdvalue;
          }
        }

        break;
      case "UPDATE":
        break;
      case "NEXT":
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "FROMLGRQ":
        // ************************************************************
        // * Per IDCR 276, ASIN should not return to LGRQ until a valid
        // * assignment exists
        // ************************************************************
        global.Command = "DISPLAY";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";
        global.Command = "INVALID";

        break;
    }

    // ---------------------------------------------
    // Security & Selection Validation
    // ---------------------------------------------
    if (Equal(global.Command, "ADD") || Equal(global.Command, "DISPLAY") || Equal
      (global.Command, "DELETE") || Equal(global.Command, "LIST") || Equal
      (global.Command, "UPDATE"))
    {
      if (!Equal(global.Command, "LIST"))
      {
        UseScCabTestSecurity();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          goto Test2;
        }
      }

      if (Equal(global.Command, "DISPLAY"))
      {
        goto Test2;
      }

      // *** Validate that one Selection or one selection and one prompt 
      // selected  *****
      // *** One Selection is valid for Add and Update
      // *****
      // *** One Selection and one prompt is valid for LIST
      // *****
      local.SelCount.Count = 0;
      local.ReasonCount.Count = 0;
      local.SpCount.Count = 0;
      local.FunctionCount.Count = 0;
      local.RowCounter.Count = 0;

      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        if (!export.Group.CheckSize())
        {
          break;
        }

        if (!IsEmpty(export.Group.Item.Common.SelectChar))
        {
          var field = GetField(export.Group.Item.Common, "selectChar");

          field.Error = true;

          ++local.SelCount.Count;
          export.Pointer.Count = export.Group.Index + 1;
        }

        if (!IsEmpty(export.Group.Item.PromptSp.PromptField))
        {
          ++local.SpCount.Count;

          var field = GetField(export.Group.Item.PromptSp, "promptField");

          field.Error = true;
        }

        if (!IsEmpty(export.Group.Item.PromptRsn.PromptField))
        {
          ++local.ReasonCount.Count;

          var field = GetField(export.Group.Item.PromptRsn, "promptField");

          field.Error = true;
        }

        if (!IsEmpty(export.Group.Item.PromptFunc.PromptField))
        {
          ++local.FunctionCount.Count;

          var field = GetField(export.Group.Item.PromptFunc, "promptField");

          field.Error = true;
        }
      }

      export.Group.CheckIndex();

      // ***  Select Character ***
      switch(local.SelCount.Count)
      {
        case 0:
          break;
        case 1:
          local.SelResSpFuncInd.Count = 1000;

          break;
        default:
          local.SelResSpFuncInd.Count = 2000;

          break;
      }

      // ***  Reason Code Prompt ***
      switch(local.ReasonCount.Count)
      {
        case 0:
          break;
        case 1:
          local.SelResSpFuncInd.Count += 100;

          break;
        default:
          local.SelResSpFuncInd.Count += 200;

          break;
      }

      // *** Service Provider Prompt ***
      switch(local.SpCount.Count)
      {
        case 0:
          break;
        case 1:
          local.SelResSpFuncInd.Count += 10;

          break;
        default:
          local.SelResSpFuncInd.Count += 20;

          break;
      }

      // *** Function Prompt ***
      switch(local.FunctionCount.Count)
      {
        case 0:
          break;
        case 1:
          ++local.SelResSpFuncInd.Count;

          break;
        default:
          local.SelResSpFuncInd.Count += 2;

          break;
      }

      export.Group.Index = export.Pointer.Count - 1;
      export.Group.CheckSize();

      switch(local.SelResSpFuncInd.Count)
      {
        case 0:
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          break;
        case 1:
          if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
          {
            ExitState = "ACO_NE0000_INVALID_PROMPT_SELTD";
          }
          else
          {
            // *** Invalid Command ***
            ExitState = "ACO_NE0000_SEL_REQD_W_FUNCTION";
          }

          break;
        case 10:
          if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
          {
            ExitState = "ACO_NE0000_INVALID_PROMPT_SELTD";
          }
          else
          {
            // *** Invalid Command ***
            ExitState = "ACO_NE0000_SEL_REQD_W_FUNCTION";
          }

          break;
        case 100:
          if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
          {
            ExitState = "ACO_NE0000_INVALID_PROMPT_SELTD";
          }
          else
          {
            // *** Invalid Command ***
            ExitState = "ACO_NE0000_SEL_REQD_W_FUNCTION";
          }

          break;
        case 1000:
          if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
          {
            if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
            {
            }
            else
            {
              ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
            }
          }
          else
          {
            // *** Invalid Command ***
            ExitState = "ACO_NE0000_INVALID_COMMAND";
          }

          break;
        case 1001:
          if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
          {
            ExitState = "ACO_NE0000_INVALID_PROMPT_SELTD";
          }
          else if (AsChar(export.Group.Item.PromptFunc.PromptField) == 'S' && AsChar
            (export.Group.Item.Common.SelectChar) == 'S')
          {
          }
          else
          {
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
          }

          break;
        case 1010:
          if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
          {
            ExitState = "ACO_NE0000_INVALID_PROMPT_SELTD";
          }
          else if (AsChar(export.Group.Item.PromptSp.PromptField) == 'S' && AsChar
            (export.Group.Item.Common.SelectChar) == 'S')
          {
          }
          else
          {
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
          }

          break;
        case 1100:
          if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
          {
            ExitState = "ACO_NE0000_INVALID_PROMPT_SELTD";
          }
          else if (AsChar(export.Group.Item.PromptRsn.PromptField) == 'S' && AsChar
            (export.Group.Item.Common.SelectChar) == 'S')
          {
          }
          else
          {
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
          }

          break;
        default:
          // @@@  should be "greater than 1111" instead of "1100".
          if (local.SelResSpFuncInd.Count > 1111)
          {
            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
          }
          else
          {
            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";
          }

          break;
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        // *** Set the command to INVALID to bypass the rest of the           **
        // *
        // *** code except the 'set field protection' para at bottom of PrAD  **
        // *
        global.Command = "INVALID";
      }
    }

Test2:

    export.Group.Index = export.Pointer.Count - 1;
    export.Group.CheckSize();

    if (Equal(global.Command, "LIST"))
    {
      switch(local.SelResSpFuncInd.Count)
      {
        case 1001:
          // ***  Function Selected         *****
          // ***  Can only be used with CASE_Unit ****
          if (AsChar(export.Group.Item.PromptFunc.PromptField) == 'S' && AsChar
            (export.Group.Item.Common.SelectChar) == 'S')
          {
            if (!Equal(export.HeaderObject.Text20, "CASE UNIT FUNCTION"))
            {
              var field =
                GetField(export.Group.Item.CaseUnitFunctionAssignmt, "function");
                

              field.Error = true;

              ExitState = "SP0000_FUNCTION_FOR_CASU_ONLY";

              goto Test3;
            }

            export.HiddenCode.CodeName = "CASE UNIT FUNCTION";
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            return;
          }
          else
          {
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
          }

          break;
        case 1010:
          // *** SP Selected  *****
          if (AsChar(export.Group.Item.PromptSp.PromptField) == 'S' && AsChar
            (export.Group.Item.Common.SelectChar) == 'S')
          {
            ExitState = "ECO_LNK_TO_LIST_SERVICE_PROVIDER";

            return;
          }
          else
          {
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
          }

          break;
        case 1100:
          // *** Reason Selected  *****
          if (AsChar(export.Group.Item.PromptRsn.PromptField) == 'S' && AsChar
            (export.Group.Item.Common.SelectChar) == 'S')
          {
            switch(TrimEnd(export.HeaderObject.Text20))
            {
              case "LEGAL ACTION":
                export.HiddenCode.CodeName = "LEGAL ASSIGNMENT REASON CODE";

                break;
              case "MONITORED ACTIVITY":
                export.HiddenCode.CodeName = "MONA ASSIGNMENT REASON CODE";

                break;
              case "OBLIGATION":
                export.HiddenCode.CodeName = "OBLIGATION ASSIGN REASON CODE";

                break;
              default:
                export.HiddenCode.CodeName = "ASSIGNMENT REASON CODE";

                break;
            }

            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            return;
          }
          else
          {
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
          }

          break;
        default:
          ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

          break;
      }
    }
    else if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
    {
      if (Equal(global.Command, "ADD"))
      {
        // **** Add has to be on a previous empty row ****
        if (!IsEmpty(export.Group.Item.Protect.Flag))
        {
          ExitState = "SP0000_ADD_FROM_BLANK_ONLY";

          var field = GetField(export.Group.Item.Common, "selectChar");

          field.Error = true;

          goto Test3;
        }

        if (Equal(export.HeaderObject.Text20, "LEGAL REFERRAL"))
        {
          // ***  Validate Effective Date  *****
          if (Equal(export.Group.Item.CaseUnitFunctionAssignmt.EffectiveDate,
            local.NullDate.Date))
          {
            export.Group.Update.CaseUnitFunctionAssignmt.EffectiveDate =
              AddDays(local.CurrentDate.Date, 1);
          }
          else if (!Equal(export.Group.Item.CaseUnitFunctionAssignmt.
            EffectiveDate, AddDays(local.CurrentDate.Date, 1)))
          {
            ExitState = "SP0000_DATE_MUST_BE_TOMORROW";

            var field =
              GetField(export.Group.Item.CaseUnitFunctionAssignmt,
              "effectiveDate");

            field.Error = true;

            goto Test3;
          }

          // -------------------------
          // Validate Discontinue Date
          // -------------------------
          if (!Equal(export.Group.Item.CaseUnitFunctionAssignmt.DiscontinueDate,
            local.NullDate.Date) && !
            Equal(export.Group.Item.CaseUnitFunctionAssignmt.DiscontinueDate,
            local.MaxDate.Date))
          {
            ExitState = "SP0000_DISC_DATE_MUST_BE_BLANK";

            var field =
              GetField(export.Group.Item.CaseUnitFunctionAssignmt,
              "discontinueDate");

            field.Error = true;

            goto Test3;
          }
        }
        else
        {
          // ***  Validate Effective Date  *****
          if (Equal(export.Group.Item.CaseUnitFunctionAssignmt.EffectiveDate,
            local.NullDate.Date))
          {
            export.Group.Update.CaseUnitFunctionAssignmt.EffectiveDate =
              local.CurrentDate.Date;
          }
          else if (Lt(export.Group.Item.CaseUnitFunctionAssignmt.EffectiveDate,
            local.CurrentDate.Date))
          {
            ExitState = "SP0000_DATE_LESS_THAN_CURRENT";

            var field =
              GetField(export.Group.Item.CaseUnitFunctionAssignmt,
              "effectiveDate");

            field.Error = true;

            goto Test3;
          }
          else if (Lt(AddDays(local.CurrentDate.Date, 1),
            export.Group.Item.CaseUnitFunctionAssignmt.EffectiveDate))
          {
            ExitState = "SP0000_FUT_ASSGNMT_DATE_INVALID";

            var field =
              GetField(export.Group.Item.CaseUnitFunctionAssignmt,
              "effectiveDate");

            field.Error = true;

            goto Test3;
          }

          // -------------------------
          // Validate Discontinue Date
          // -------------------------
          if (Lt(export.Group.Item.CaseUnitFunctionAssignmt.DiscontinueDate,
            export.Group.Item.CaseUnitFunctionAssignmt.EffectiveDate))
          {
            ExitState = "FN0000_DISC_DATE_BEFORE_EFF";

            var field =
              GetField(export.Group.Item.CaseUnitFunctionAssignmt,
              "discontinueDate");

            field.Error = true;

            goto Test3;
          }
          else if (Lt(export.Group.Item.CaseUnitFunctionAssignmt.
            DiscontinueDate, local.CurrentDate.Date))
          {
            ExitState = "SP0000_INVALID_DISC_DATE";

            var field =
              GetField(export.Group.Item.CaseUnitFunctionAssignmt,
              "discontinueDate");

            field.Error = true;

            goto Test3;

            // CQ41892 Validate for discontinue date to only allow current date
            // or current date + 1 day.
          }
          else if (Lt(AddDays(local.CurrentDate.Date, 1),
            export.Group.Item.CaseUnitFunctionAssignmt.DiscontinueDate) && !
            Equal(export.Group.Item.CaseUnitFunctionAssignmt.DiscontinueDate,
            local.MaxDate.Date))
          {
            var field =
              GetField(export.Group.Item.CaseUnitFunctionAssignmt,
              "discontinueDate");

            field.Error = true;

            ExitState = "SP0000_FUTURE_DISC_DATE_INVALID";

            goto Test3;
          }
        }

        // *** Work request 000299
        // *** 11/21/00 SWSRCHF
        // *** start
        // ---------------------------------
        // Validate Service Provider User ID
        // ---------------------------------
        if (IsEmpty(export.Group.Item.ServiceProvider.UserId))
        {
          var field = GetField(export.Group.Item.ServiceProvider, "userId");

          field.Error = true;

          ExitState = "SP0000_SERVICE_PROVIDER_REQUIRED";

          goto Test3;
        }

        if (!Equal(export.Group.Item.ServiceProvider.UserId,
          export.Group.Item.HserviceProvider.UserId))
        {
          // 07/27/2001 M.L Start
          local.Work.Count = 0;

          foreach(var item in ReadServiceProviderOfficeServiceProvider())
          {
            ++local.Work.Count;

            if (local.Work.Count > 1)
            {
              break;
            }

            if (ReadOffice())
            {
              local.ExistingOffice.SystemGeneratedId =
                entities.ExistingOffice.SystemGeneratedId;
              local.ExistingOfficeServiceProvider.Assign(
                entities.ExistingOfficeServiceProvider);
              local.ExistingServiceProvider.Assign(
                entities.ExistingServiceProvider);
            }
            else
            {
              var field1 = GetField(export.Group.Item.Common, "selectChar");

              field1.Color = "green";
              field1.Protected = false;

              var field2 =
                GetField(export.Group.Item.ServiceProvider, "userId");

              field2.Color = "red";
              field2.Protected = false;
              field2.Focused = true;

              ExitState = "SP0000_INVLD_OFFC_SRV_PRV_RL_CD";

              goto Test3;
            }
          }

          // 07/27/2001 M.L End
          switch(local.Work.Count)
          {
            case 0:
              var field1 = GetField(export.Group.Item.Common, "selectChar");

              field1.Color = "green";
              field1.Protected = false;

              var field2 =
                GetField(export.Group.Item.ServiceProvider, "userId");

              field2.Color = "red";
              field2.Protected = false;
              field2.Focused = true;

              ExitState = "SERVICE_PROVIDER_NF";

              goto Test3;
            case 1:
              // ------------------------------------------------------------------------------------
              // Validate Service Provider User ID, Office and Service Provider 
              // Role Code combination
              // ------------------------------------------------------------------------------------
              // 07/27/2001 M.L Start
              export.Group.Update.Office.SystemGeneratedId =
                local.ExistingOffice.SystemGeneratedId;
              export.Group.Update.Hoffice.SystemGeneratedId =
                local.ExistingOffice.SystemGeneratedId;
              export.HiddenOffice.SystemGeneratedId =
                local.ExistingOffice.SystemGeneratedId;
              export.Group.Update.ServiceProvider.SystemGeneratedId =
                local.ExistingServiceProvider.SystemGeneratedId;
              export.HiddenServiceProvider.SystemGeneratedId =
                local.ExistingServiceProvider.SystemGeneratedId;
              export.Group.Update.HserviceProvider.UserId =
                local.ExistingServiceProvider.UserId;
              export.HiddenServiceProvider.UserId =
                local.ExistingServiceProvider.UserId;
              export.Group.Update.OfficeServiceProvider.RoleCode =
                local.ExistingOfficeServiceProvider.RoleCode;
              export.Group.Update.HofficeServiceProvider.RoleCode =
                local.ExistingOfficeServiceProvider.RoleCode;
              export.HiddenOfficeServiceProvider.RoleCode =
                local.ExistingOfficeServiceProvider.RoleCode;
              export.Group.Update.OfficeServiceProvider.EffectiveDate =
                local.ExistingOfficeServiceProvider.EffectiveDate;
              export.HiddenOfficeServiceProvider.EffectiveDate =
                local.ExistingOfficeServiceProvider.EffectiveDate;

              // 07/27/2001 M.L End
              break;
            default:
              var field3 = GetField(export.Group.Item.PromptSp, "promptField");

              field3.Color = "red";
              field3.Protected = false;
              field3.Focused = true;

              var field4 = GetField(export.Group.Item.Common, "selectChar");

              field4.Color = "green";
              field4.Protected = false;

              var field5 =
                GetField(export.Group.Item.ServiceProvider, "userId");

              field5.Error = true;

              ExitState = "SP0000_SVPO_PICKLIST";

              goto Test3;
          }
        }

        // *** end
        // *** 11/21/00 SWSRCHF
        // *** Work request 000299
        // *** Problem report I00111815
        // *** 01/30/01 SWSRCHF
        // *** start
        if (Equal(export.HeaderObject.Text20, "CASE"))
        {
          switch(TrimEnd(export.HiddenOfficeServiceProvider.RoleCode))
          {
            case "CH":
              break;
            case "CO":
              break;
            case "SS":
              break;
            default:
              var field =
                GetField(export.Group.Item.OfficeServiceProvider, "roleCode");

              field.Error = true;

              ExitState = "SP0000_OSP_MUST_BE_A_CH_CO_SS";

              goto Test3;
          }
        }
        else
        {
        }

        // *** end
        // *** 01/30/01 SWSRCHF
        // *** Problem report I00111815
      }
      else if (Equal(global.Command, "UPDATE"))
      {
        // ----------------------------------
        // Update has to be on a existing row
        // ----------------------------------
        if (IsEmpty(export.Group.Item.Protect.Flag))
        {
          ExitState = "SP0000_UPDATE_ON_EMPTY_ROW";

          var field = GetField(export.Group.Item.Common, "selectChar");

          field.Error = true;

          goto Test3;
        }

        // **** Check to see if data changed ****
        if (Equal(export.Group.Item.CaseUnitFunctionAssignmt.EffectiveDate,
          export.Group.Item.HcaseUnitFunctionAssignmt.EffectiveDate) && Equal
          (export.Group.Item.CaseUnitFunctionAssignmt.DiscontinueDate,
          export.Group.Item.HcaseUnitFunctionAssignmt.DiscontinueDate) && Equal
          (export.Group.Item.CaseUnitFunctionAssignmt.ReasonCode,
          export.Group.Item.HcaseUnitFunctionAssignmt.ReasonCode) && Equal
          (export.Group.Item.CaseUnitFunctionAssignmt.Function,
          export.Group.Item.HcaseUnitFunctionAssignmt.Function) && Equal
          (export.Group.Item.CaseUnitFunctionAssignmt.CreatedBy,
          export.Group.Item.HcaseUnitFunctionAssignmt.CreatedBy) && AsChar
          (export.Group.Item.CaseUnitFunctionAssignmt.OverrideInd) == AsChar
          (export.Group.Item.HcaseUnitFunctionAssignmt.OverrideInd) && Equal
          (export.Group.Item.ServiceProvider.UserId,
          export.Group.Item.HserviceProvider.UserId) && export
          .Group.Item.Office.SystemGeneratedId == export
          .Group.Item.Hoffice.SystemGeneratedId && Equal
          (export.Group.Item.OfficeServiceProvider.RoleCode,
          export.Group.Item.HofficeServiceProvider.RoleCode))
        {
          ExitState = "SP0000_DATA_NOT_CHANGED";

          var field = GetField(export.Group.Item.Common, "selectChar");

          field.Error = true;

          goto Test3;
        }

        // ***  Validate Discontinue Date  *****
        if (Lt(export.Group.Item.CaseUnitFunctionAssignmt.DiscontinueDate,
          export.Group.Item.CaseUnitFunctionAssignmt.EffectiveDate))
        {
          ExitState = "FN0000_DISC_DATE_BEFORE_EFF";

          var field =
            GetField(export.Group.Item.CaseUnitFunctionAssignmt,
            "discontinueDate");

          field.Error = true;

          goto Test3;
        }
        else if (Lt(export.Group.Item.CaseUnitFunctionAssignmt.DiscontinueDate,
          local.CurrentDate.Date))
        {
          ExitState = "SP0000_DATE_LESS_THAN_CURRENT";

          var field =
            GetField(export.Group.Item.CaseUnitFunctionAssignmt,
            "discontinueDate");

          field.Error = true;

          goto Test3;

          // CQ41892 Validate for discontinue date to only allow current date  
          // or current date + 1 day.
        }
        else if (Lt(AddDays(local.CurrentDate.Date, 1),
          export.Group.Item.CaseUnitFunctionAssignmt.DiscontinueDate) && !
          Equal(export.Group.Item.CaseUnitFunctionAssignmt.DiscontinueDate,
          local.MaxDate.Date))
        {
          var field =
            GetField(export.Group.Item.CaseUnitFunctionAssignmt,
            "discontinueDate");

          field.Error = true;

          ExitState = "SP0000_FUTURE_DISC_DATE_INVALID";

          goto Test3;
        }
      }

      // ***  SP role cannot be spaces  *****
      if (IsEmpty(export.Group.Item.OfficeServiceProvider.RoleCode))
      {
        ExitState = "SP0000_OSP_REQD";

        var field = GetField(export.Group.Item.PromptRsn, "promptField");

        field.Error = true;

        goto Test3;
      }

      // ***  Validate SP Role   *****
      switch(TrimEnd(export.HeaderObject.Text20))
      {
        case "LEGAL ACTION":
          if (Equal(global.Command, "ADD"))
          {
            // 09/14/00 GVandy     H00103139    Allow U class legal actions to 
            // be assigned
            // 				 to non-attorney service providers.
            // 12/20/01 GVandy     PR 125378    Allow N class legal actions to 
            // be assigned
            // 				 to non-attorney service providers.
            if (IsEmpty(export.HlegalAction.Classification))
            {
              if (ReadLegalAction1())
              {
                local.LegalAction.Assign(entities.LegalAction);
              }
              else
              {
                // -- Continue.  An exit state will be set below.
              }
            }
            else
            {
              local.LegalAction.Assign(export.HlegalAction);
            }

            if (AsChar(local.LegalAction.Classification) == 'U' || AsChar
              (local.LegalAction.Classification) == 'N')
            {
              local.Code.CodeName = "OFFICE SERVICE PROVIDER ROLE";
            }
            else
            {
              local.Code.CodeName = "ATTORNEY CODES";
            }
          }
          else
          {
            // 02/17/06  GVandy	PR205434	Remove edit requiring service provider 
            // be an attorney on UPDATEs.
            // -- This is done so that erroneous assigments to a wrong service 
            // provider type can be end dated.
            // Example:  A J class legal action assigned to a non attorney could
            // not be end dated because the
            // edit was requiring the service provider be an attorney.
            local.Code.CodeName = "OFFICE SERVICE PROVIDER ROLE";
          }

          break;
        case "LEGAL REFERRAL":
          local.Code.CodeName = "ATTORNEY CODES";

          break;
        default:
          local.Code.CodeName = "OFFICE SERVICE PROVIDER ROLE";

          break;
      }

      local.CodeValue.Cdvalue =
        export.Group.Item.OfficeServiceProvider.RoleCode;
      UseCabValidateCodeValue();

      if (AsChar(local.ValidCode.Flag) == 'N')
      {
        ExitState = "SP0000_OSP_MUST_BE_AN_ATTY";

        var field1 = GetField(export.Group.Item.PromptRsn, "promptField");

        field1.Error = true;

        var field2 =
          GetField(export.Group.Item.OfficeServiceProvider, "roleCode");

        field2.Error = true;

        goto Test3;
      }

      // --------------------
      // Validate Reason Code
      // --------------------
      if (IsEmpty(export.Group.Item.CaseUnitFunctionAssignmt.ReasonCode))
      {
        if (Equal(export.HeaderObject.Text20, "LEGAL REFERRAL"))
        {
          export.Group.Update.CaseUnitFunctionAssignmt.ReasonCode = "RSP";
        }
        else
        {
          ExitState = "SP0000_ASSIGNMENT_REASON_REQD";

          var field =
            GetField(export.Group.Item.CaseUnitFunctionAssignmt, "reasonCode");

          field.Error = true;

          goto Test3;
        }
      }

      // --------------------
      // Validate Reason Code
      // --------------------
      switch(TrimEnd(export.HeaderObject.Text20))
      {
        case "LEGAL ACTION":
          local.Code.CodeName = "LEGAL ASSIGNMENT REASON CODE";

          break;
        case "MONITORED ACTIVITY":
          local.Code.CodeName = "MONA ASSIGNMENT REASON CODE";

          break;
        default:
          local.Code.CodeName = "ASSIGNMENT REASON CODE";

          break;
      }

      local.CodeValue.Cdvalue =
        export.Group.Item.CaseUnitFunctionAssignmt.ReasonCode;
      UseCabValidateCodeValue();

      if (AsChar(local.ValidCode.Flag) != 'Y')
      {
        ExitState = "SP0000_INVALID_ASSIGNMENT_REASON";

        var field =
          GetField(export.Group.Item.CaseUnitFunctionAssignmt, "reasonCode");

        field.Error = true;

        goto Test3;
      }

      // ***  Validate Override Indicator   *****
      switch(AsChar(export.Group.Item.CaseUnitFunctionAssignmt.OverrideInd))
      {
        case ' ':
          export.Group.Update.CaseUnitFunctionAssignmt.OverrideInd = "N";

          break;
        case 'Y':
          break;
        case 'N':
          break;
        default:
          ExitState = "SP0000_INVALID_OVERRIDE_IND";

          var field =
            GetField(export.Group.Item.CaseUnitFunctionAssignmt, "overrideInd");
            

          field.Error = true;

          goto Test3;
      }

      switch(TrimEnd(export.HeaderObject.Text20))
      {
        case "CASE":
          local.CaseAssignment.ReasonCode =
            export.Group.Item.CaseUnitFunctionAssignmt.ReasonCode;
          local.CaseAssignment.OverrideInd =
            export.Group.Item.CaseUnitFunctionAssignmt.OverrideInd;
          local.CaseAssignment.EffectiveDate =
            export.Group.Item.CaseUnitFunctionAssignmt.EffectiveDate;
          local.CaseAssignment.DiscontinueDate =
            export.Group.Item.CaseUnitFunctionAssignmt.DiscontinueDate;

          if (Equal(global.Command, "ADD"))
          {
            if (!Equal(export.Group.Item.CaseUnitFunctionAssignmt.
              DiscontinueDate, local.MaxDate.Date))
            {
              var field1 =
                GetField(export.Group.Item.CaseUnitFunctionAssignmt,
                "discontinueDate");

              field1.Error = true;

              ExitState = "SP0000_DISC_DATE_NOT_ALLOWED";

              goto Test3;
            }

            UseSpCabReassignCase();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              // 11/17/11 GVandy	CQ30161 Do not trigger CASETXFR letters.  The 
              // letters will now be triggered by new batch program
              // SP_B703_CASETXFR_GENERATION.
            }
            else
            {
              // CQ#9424 Added the Else part to Rollback if there are any errors
              break;
            }

            // ------------------------------------------------------------
            // CSEnet functionality - if caseworker changes notify other
            // state(s) if case responsibility changes
            // ------------------------------------------------------------
            if (Equal(local.CaseAssignment.ReasonCode, "RSP"))
            {
              local.ScreenIdentification.Command = "ASIN";
              UseSiCreateAutoCsenetTrans();

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                var field1 = GetField(export.Group.Item.Common, "selectChar");

                field1.Protected = false;

                ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
              }
            }
          }
          else if (Equal(global.Command, "UPDATE"))
          {
            // ****************************************************************
            // *Only reason code and Override Indicator can be updated.       *
            // *Discontinue Date is protected on screen and cannot be changed *
            // ****************************************************************
            local.CaseAssignment.CreatedTimestamp =
              export.Group.Item.CaseUnitFunctionAssignmt.CreatedTimestamp;
            local.CaseAssignment.CreatedBy =
              export.Group.Item.CaseUnitFunctionAssignmt.CreatedBy;
            UseSpCabUpdateCaseAssgn();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              var field1 = GetField(export.Group.Item.Common, "selectChar");

              field1.Protected = false;

              ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
            }
          }

          break;
        case "CASE UNIT FUNCTION":
          // ***  Establish Currency on required Entities  ***
          if (!ReadCaseCaseUnit())
          {
            if (ReadCase())
            {
              ExitState = "CASE_UNIT_NF";

              goto Test3;
            }
            else
            {
              ExitState = "CASE_NF";

              goto Test3;
            }
          }

          if (IsEmpty(export.Group.Item.CaseUnitFunctionAssignmt.Function))
          {
            ExitState = "SP0000_CASE_UNIT_FUNCTION_REQD";

            var field1 =
              GetField(export.Group.Item.CaseUnitFunctionAssignmt, "function");

            field1.Error = true;

            goto Test3;
          }

          local.Code.CodeName = "CASE UNIT FUNCTION";
          local.CodeValue.Cdvalue =
            export.Group.Item.CaseUnitFunctionAssignmt.Function;
          UseCabValidateCodeValue();

          if (AsChar(local.ValidCode.Flag) != 'Y')
          {
            ExitState = "SP0000_INVALID_FUNCTION";

            var field1 =
              GetField(export.Group.Item.CaseUnitFunctionAssignmt, "function");

            field1.Error = true;

            goto Test3;
          }

          // *** Check for Overlapping Assignments ***
          if (ReadCaseUnitFunctionAssignmt1())
          {
            var field1 =
              GetField(export.Group.Item.CaseUnitFunctionAssignmt,
              "effectiveDate");

            field1.Error = true;

            ExitState = "SP0000_OVERLAPPG_CU_ASSGMT_AE";

            goto Test3;
          }

          if (Equal(global.Command, "ADD"))
          {
            UseSpCabCreateCuFunctionAssign();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              var field1 = GetField(export.Group.Item.Common, "selectChar");

              field1.Protected = false;

              ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
            }
          }
          else if (Equal(global.Command, "UPDATE"))
          {
            UseSpCabUpdateCuFuncAssgn();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              var field1 = GetField(export.Group.Item.Common, "selectChar");

              field1.Protected = false;

              ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
            }
          }

          break;
        case "INTERSTATE REFERRAL":
          // **** Get currency on interstate-case ****
          // **** Decode Interstate case referral number out of the title line 1
          // ****
          local.Icase.TransSerialNumber =
            StringToNumber(Substring(
              export.HeaderObjTitle1.Text80, SpTextWorkArea.Text80_MaxLength,
            17, 12));
          local.Icase.TransactionDate =
            IntToDate(export.HiddenNextTranInfo.LegalActionIdentifier.
              GetValueOrDefault());

          // **** read Interstate case referral using icase number out of the 
          // title ****
          // **** line 1 & eff date from export_hidden OSP
          // ****
          if (!ReadInterstateCase())
          {
            ExitState = "INTERSTATE_CASE_NF";

            break;
          }

          // *** Check for Overlapping Assignments ***
          if (ReadInterstateCaseAssignment1())
          {
            ExitState = "SP0000_ASSIGNMENT_OVERLAP";

            var field1 =
              GetField(export.Group.Item.CaseUnitFunctionAssignmt,
              "effectiveDate");

            field1.Error = true;

            goto Test3;
          }

          // **** roll generic data into specific assignment data ****
          local.InterstateCaseAssignment.ReasonCode =
            export.Group.Item.CaseUnitFunctionAssignmt.ReasonCode;
          local.InterstateCaseAssignment.OverrideInd =
            export.Group.Item.CaseUnitFunctionAssignmt.OverrideInd;
          local.InterstateCaseAssignment.EffectiveDate =
            export.Group.Item.CaseUnitFunctionAssignmt.EffectiveDate;
          local.InterstateCaseAssignment.DiscontinueDate =
            export.Group.Item.CaseUnitFunctionAssignmt.DiscontinueDate;

          // **** Perform the request function - command ****
          if (Equal(global.Command, "ADD"))
          {
            // **** Pass data into ADD AB ****
            UseSpCabCreateIcaseAssignment();

            // **** Check Exitstate - for sucessful completion ****
            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              var field1 = GetField(export.Group.Item.Common, "selectChar");

              field1.Protected = false;

              ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
            }
          }
          else if (Equal(global.Command, "UPDATE"))
          {
            // **** Pass data into UPDATE AB ****
            local.InterstateCaseAssignment.CreatedTimestamp =
              export.Group.Item.CaseUnitFunctionAssignmt.CreatedTimestamp;
            local.InterstateCaseAssignment.CreatedBy =
              export.Group.Item.CaseUnitFunctionAssignmt.CreatedBy;
            UseSpCabUpdateIcaseAssignment();

            // **** Check Exitstate - for sucessful completion ****
            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              var field1 = GetField(export.Group.Item.Common, "selectChar");

              field1.Protected = false;

              ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
            }
          }

          break;
        case "MONITORED ACTIVITY":
          if (ReadMonitoredActivity2())
          {
            if (!ReadInfrastructure())
            {
              goto Test3;
            }
          }
          else
          {
            ExitState = "SP0000_MONITORED_ACTIVITY_NF";

            goto Test3;
          }

          // *** Check for Overlapping Assignments ***
          if (ReadMonitoredActivityAssignment1())
          {
            ExitState = "SP0000_OVERLAPG_MONA_ASSGMT_AE";

            var field1 =
              GetField(export.Group.Item.CaseUnitFunctionAssignmt,
              "effectiveDate");

            field1.Error = true;

            goto Test3;
          }

          local.MonitoredActivityAssignment.ReasonCode =
            export.Group.Item.CaseUnitFunctionAssignmt.ReasonCode;
          local.MonitoredActivityAssignment.OverrideInd =
            export.Group.Item.CaseUnitFunctionAssignmt.OverrideInd;
          local.MonitoredActivityAssignment.EffectiveDate =
            export.Group.Item.CaseUnitFunctionAssignmt.EffectiveDate;
          local.MonitoredActivityAssignment.DiscontinueDate =
            export.Group.Item.CaseUnitFunctionAssignmt.DiscontinueDate;
          local.MonitoredActivityAssignment.CreatedTimestamp =
            export.Group.Item.CaseUnitFunctionAssignmt.CreatedTimestamp;

          if (Equal(global.Command, "ADD"))
          {
            UseSpCabCreateMonActAssignment2();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              var field1 = GetField(export.Group.Item.Common, "selectChar");

              field1.Protected = false;

              ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
            }
          }
          else if (Equal(global.Command, "UPDATE"))
          {
            UseSpCabUpdateMonaAssignment();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              var field1 = GetField(export.Group.Item.Common, "selectChar");

              field1.Protected = false;

              ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
            }
          }

          break;
        case "PA REFERRAL":
          if (!ReadPaReferral())
          {
            ExitState = "PA_REFERRAL_NF";

            goto Test3;
          }

          // *** Check for Overlapping Assignments ***
          if (ReadPaReferralAssignment1())
          {
            ExitState = "SP0000_OVERLAPPING_ASSIGNMENT";

            var field1 =
              GetField(export.Group.Item.CaseUnitFunctionAssignmt,
              "effectiveDate");

            field1.Error = true;

            goto Test3;
          }

          local.PaReferralAssignment.ReasonCode =
            export.Group.Item.CaseUnitFunctionAssignmt.ReasonCode;
          local.PaReferralAssignment.OverrideInd =
            export.Group.Item.CaseUnitFunctionAssignmt.OverrideInd;
          local.PaReferralAssignment.EffectiveDate =
            export.Group.Item.CaseUnitFunctionAssignmt.EffectiveDate;
          local.PaReferralAssignment.DiscontinueDate =
            export.Group.Item.CaseUnitFunctionAssignmt.DiscontinueDate;
          local.PaReferralAssignment.CreatedTimestamp =
            export.Group.Item.CaseUnitFunctionAssignmt.CreatedTimestamp;

          if (Equal(global.Command, "ADD"))
          {
            UseSpCabCreatePaReferralAssign();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              var field1 = GetField(export.Group.Item.Common, "selectChar");

              field1.Protected = false;

              ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
            }
          }
          else if (Equal(global.Command, "UPDATE"))
          {
            UseSpCabUpdatePaRefAssgmt();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              var field1 = GetField(export.Group.Item.Common, "selectChar");

              field1.Protected = false;

              ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
            }
          }

          break;
        case "LEGAL REFERRAL":
          if (ReadCase())
          {
            if (!ReadLegalReferral())
            {
              ExitState = "LEGAL_REFERRAL_NF";

              goto Test3;
            }
          }
          else
          {
            ExitState = "CASE_NF";

            goto Test3;
          }

          // -- 04/26/01 GVandy  WR251 - Removed check for overlapping 
          // assignments.  We will now end date any overlapping assignments in
          // the create cab.
          local.LegalReferralAssignment.ReasonCode =
            export.Group.Item.CaseUnitFunctionAssignmt.ReasonCode;
          local.LegalReferralAssignment.OverrideInd =
            export.Group.Item.CaseUnitFunctionAssignmt.OverrideInd;
          local.LegalReferralAssignment.EffectiveDate =
            export.Group.Item.CaseUnitFunctionAssignmt.EffectiveDate;
          local.LegalReferralAssignment.DiscontinueDate =
            export.Group.Item.CaseUnitFunctionAssignmt.DiscontinueDate;
          local.LegalReferralAssignment.CreatedTimestamp =
            export.Group.Item.CaseUnitFunctionAssignmt.CreatedTimestamp;

          if (Equal(global.Command, "ADD"))
          {
            UseSpCabCreateLegalRefAssign();

            // *** Problem report H00082885
            // *** 02/10/00 SWSRCHF
            // *** start
            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              if (export.OldOffice.SystemGeneratedId != 0)
              {
                // ***
                // *** get the "OLD" Office Service Provider
                // ***
                if (!ReadOfficeServiceProvider2())
                {
                  ExitState = "SP0000_OFFICE_SERVICE_PROVIDR_NF";

                  goto Test3;
                }

                // ***
                // *** get the "NEW" Office Service Provider
                // ***
                if (ReadOfficeServiceProvider4())
                {
                  local.Lrf.BusinessObjectCd = "LRF";
                }
                else
                {
                  ExitState = "SP0000_OFFICE_SERVICE_PROVIDR_NF";

                  goto Test3;
                }

                // ***
                // *** OBTAIN all "OLD" Monitored Activity Assignment
                // ***
                // ===============================================
                // 4/14/00 - bud adams  -  PR# K00302: Performance;
                // ===============================================
                local.SelectInfrastructure.DenormNumeric12 =
                  entities.LegalReferral.Identifier;

                foreach(var item in ReadMonitoredActivityAssignmentMonitoredActivity2())
                  
                {
                  // ***
                  // *** CREATE a "NEW" Monitored Activity Assignment
                  // ***
                  try
                  {
                    CreateMonitoredActivityAssignment2();
                  }
                  catch(Exception e)
                  {
                    switch(GetErrorCode(e))
                    {
                      case ErrorCode.AlreadyExists:
                        ExitState = "SP0000_MONITORED_ACT_ASSGN_AE";

                        goto Test3;
                      case ErrorCode.PermittedValueViolation:
                        ExitState = "SP0000_MONITORED_ACT_ASSGN_PV";

                        goto Test3;
                      case ErrorCode.DatabaseError:
                        break;
                      default:
                        throw;
                    }
                  }
                }

                export.OldOffice.SystemGeneratedId =
                  local.InitOffice.SystemGeneratedId;
                export.OldServiceProvider.SystemGeneratedId =
                  local.InitServiceProvider.SystemGeneratedId;
                MoveOfficeServiceProvider(local.InitOfficeServiceProvider,
                  export.OldOfficeServiceProvider);
                export.OldLegalReferralAssignment.DiscontinueDate =
                  local.InitLegalReferralAssignment.DiscontinueDate;

                break;
              }
            }

            // *** end
            // *** 02/10/00 SWSRCHF
            // *** Problem report H00082885
            // *** December 15, 1999  David Lowry
            // Added this new cab to create an infrastructure record when a new 
            // legal referral has been created and assigned.
            if (IsExitState("ACO_NN0000_ALL_OK") && AsChar
              (entities.LegalReferral.Status) == 'S')
            {
              if (!ReadServiceProviderOfficeOfficeServiceProvider())
              {
                var field1 = GetField(export.Group.Item.Common, "selectChar");

                field1.Error = true;

                var field2 =
                  GetField(export.Group.Item.OfficeServiceProvider, "roleCode");
                  

                field2.Error = true;

                var field3 =
                  GetField(export.Group.Item.Office, "systemGeneratedId");

                field3.Error = true;

                var field4 =
                  GetField(export.Group.Item.ServiceProvider, "userId");

                field4.Error = true;

                ExitState = "SP0000_OFFICE_SERVICE_PROVIDR_NF";

                goto Test3;
              }

              if (export.Group.Index == 0)
              {
                // Code added by Carl Galka on 01/05/2000.
                // If we are adding the first assignment, we must trigger a 
                // monitored activity by creating an infrastrcture record.
                UseOeLgrqPrepareEvent();
              }
              else
              {
                local.Infrastructure.DenormNumeric12 =
                  export.HlegalReferral.Identifier;

                if (ReadOfficeServiceProvider1())
                {
                  // This will find one office service provider to allow us to 
                  // determine the monitored activities that are available.
                }

                // *** 7/28/00  Anita Massey    Added a check to make sure that 
                // a key only osp is populated.   When this view is not
                // populated in the new version of cool gen it abends rather
                // than dropping on through like it did before.
                if (entities.AkeyOnlyOfficeServiceProvider.Populated)
                {
                  foreach(var item in ReadMonitoredActivityMonitoredActivityAssignment())
                    
                  {
                    // If it is a changed to an existing referral, we must 
                    // create a new monitored activity assignment and relate it
                    // to the same monitored activity.
                    local.MonitoredActivityAssignment.Assign(entities.Old);
                    local.MonitoredActivityAssignment.DiscontinueDate =
                      local.MaxDate.Date;
                    local.MonitoredActivityAssignment.LastUpdatedBy = "";
                    local.MonitoredActivityAssignment.LastUpdatedTimestamp =
                      local.NullDate.Timestamp;
                    UseSpCabCreateMonActAssignment1();
                  }
                }
              }
            }

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              // This was changed by Carl Galka on 01/06/2000 per problem report
              // 81816. We will automatically return to LGRQ when we
              // successfully assign an attorney.
              var field1 = GetField(export.Group.Item.Common, "selectChar");

              field1.Protected = false;

              ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
            }
          }
          else if (Equal(global.Command, "UPDATE"))
          {
            UseSpCabUpdateLegRefAssgmt();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              // -- 04/26/01 GVandy WR251 - The only updatable attribute is now 
              // the override indicator.  Since the discontinue date cannot be
              // changed there is no reason to update the monitored activity
              // assignments.
              var field1 = GetField(export.Group.Item.Common, "selectChar");

              field1.Protected = false;

              ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
            }
          }

          break;
        case "LEGAL ACTION":
          if (!entities.LegalAction.Populated)
          {
            if (!ReadLegalAction1())
            {
              ExitState = "LEGAL_ACTION_NF";

              goto Test3;
            }
          }

          if (ReadTribunal())
          {
            local.Tribunal.Assign(entities.Tribunal);

            if (!ReadFips())
            {
              if (!ReadFipsTribAddress())
              {
                goto Test3;
              }
            }
          }
          else
          {
            goto Test3;
          }

          // *** Check for Overlapping Assignments ***
          if (ReadLegalActionAssigment1())
          {
            ExitState = "SP0000_OVERLAPPING_ASSIGNMENT";

            var field1 =
              GetField(export.Group.Item.CaseUnitFunctionAssignmt,
              "effectiveDate");

            field1.Error = true;

            goto Test3;
          }

          local.LegalActionAssigment.ReasonCode =
            export.Group.Item.CaseUnitFunctionAssignmt.ReasonCode;
          local.LegalActionAssigment.OverrideInd =
            export.Group.Item.CaseUnitFunctionAssignmt.OverrideInd;
          local.LegalActionAssigment.EffectiveDate =
            export.Group.Item.CaseUnitFunctionAssignmt.EffectiveDate;
          local.LegalActionAssigment.DiscontinueDate =
            export.Group.Item.CaseUnitFunctionAssignmt.DiscontinueDate;
          local.LegalActionAssigment.CreatedTimestamp =
            export.Group.Item.CaseUnitFunctionAssignmt.CreatedTimestamp;
          local.LegalActionAssigment.CreatedTimestamp =
            export.Group.Item.CaseUnitFunctionAssignmt.CreatedTimestamp;

          if (Equal(global.Command, "ADD"))
          {
            UseSpCabCreateLegalActionAssgn();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              break;
            }

            // *** Problem report H00082885
            // *** 02/10/00 SWSRCHF
            // *** start
            if (export.OldOffice.SystemGeneratedId != 0)
            {
              // ***
              // *** Obtain "OLD" Office Service Provider
              // ***
              if (!ReadOfficeServiceProvider2())
              {
                ExitState = "SP0000_OFFICE_SERVICE_PROVIDR_NF";

                goto Test3;
              }

              // ***
              // *** Obtain "NEW" Office Service Provider
              // ***
              if (ReadOfficeServiceProvider4())
              {
                local.Lea.BusinessObjectCd = "LEA";
                local.Saved.DenormText12 =
                  Substring(export.HlegalAction.CourtCaseNumber, 1, 12);
              }
              else
              {
                ExitState = "SP0000_OFFICE_SERVICE_PROVIDR_NF";

                goto Test3;
              }

              local.Compare.Detail = export.Detail.Text32;

              // ***
              // *** Obtain all  "OLD" Monitored Activity Assignment(s)
              // ***
              foreach(var item in ReadMonitoredActivityAssignmentMonitoredActivity1())
                
              {
                // ***
                // *** CREATE a  "NEW" Monitored Activity Assignment
                // ***
                try
                {
                  CreateMonitoredActivityAssignment1();
                }
                catch(Exception e)
                {
                  switch(GetErrorCode(e))
                  {
                    case ErrorCode.AlreadyExists:
                      ExitState = "SP0000_MONITORED_ACT_ASSGN_AE";

                      goto Test3;
                    case ErrorCode.PermittedValueViolation:
                      ExitState = "SP0000_MONITORED_ACT_ASSGN_PV";

                      goto Test3;
                    case ErrorCode.DatabaseError:
                      break;
                    default:
                      throw;
                  }
                }
              }

              export.OldOffice.SystemGeneratedId =
                local.InitOffice.SystemGeneratedId;
              export.OldServiceProvider.SystemGeneratedId =
                local.InitServiceProvider.SystemGeneratedId;
              MoveOfficeServiceProvider(local.InitOfficeServiceProvider,
                export.OldOfficeServiceProvider);
              export.OldLegalActionAssigment.DiscontinueDate =
                local.InitLegalActionAssigment.DiscontinueDate;
              export.Detail.Text32 = local.InitWorkArea.Text32;
              export.Compare.FiledDate = local.InitLegalAction.FiledDate;
            }

            // *** end
            // *** 02/10/00 SWSRCHF
            // *** Problem report H00082885
            var field1 = GetField(export.Group.Item.Common, "selectChar");

            field1.Protected = false;

            // ------------------------------------------------------------
            // Event 95 will be created when the first legal action is added.
            // Without the service provider attached to the process, the
            // monitored activity assignment cannot be determined after
            // event processor batch runs.
            // -------------------------------------------------------------
            export.HlegalAction.Assign(entities.LegalAction);

            // ---------------------------------------------------------------------------
            // The following code is added as part of the PR# 152294. When the 
            // first assignment is done this code will be executed and event 95
            // will be raised.
            //                                              
            // Vithal (07/18/2002)
            // ----------------------------------------------------------------------------
            local.Event95.Count = 0;

            foreach(var item in ReadLegalActionAssigment2())
            {
              ++local.Event95.Count;
            }

            if (local.Event95.Count == 1 && Equal
              (local.LegalActionAssigment.ReasonCode, "RSP"))
            {
              local.Event95.Flag = "Y";
              UseLeLactRaiseInfrastrEvents();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                break;
              }
            }

            ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
          }
          else if (Equal(global.Command, "UPDATE"))
          {
            UseSpCabUpdateLactAssignment();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              // *** Problem report H00082885
              // *** 02/10/00 SWSRCHF
              // *** start
              // ***
              // *** Obtain "OLD" Office Service Provider
              // ***
              if (ReadOfficeServiceProvider3())
              {
                export.OldOffice.SystemGeneratedId =
                  export.Group.Item.Office.SystemGeneratedId;
                export.OldServiceProvider.SystemGeneratedId =
                  export.Group.Item.ServiceProvider.SystemGeneratedId;
                export.OldOfficeServiceProvider.EffectiveDate =
                  export.Group.Item.OfficeServiceProvider.EffectiveDate;
                export.OldOfficeServiceProvider.RoleCode =
                  export.Group.Item.OfficeServiceProvider.RoleCode;
                local.Lea.BusinessObjectCd = "LEA";
                local.Saved.DenormText12 =
                  Substring(export.HlegalAction.CourtCaseNumber, 1, 12);
              }
              else
              {
                ExitState = "SP0000_OFFICE_SERVICE_PROVIDR_NF";

                goto Test3;
              }

              export.OldLegalActionAssigment.DiscontinueDate =
                local.LegalActionAssigment.DiscontinueDate;
              local.Compare.Detail = export.Detail.Text32;

              // ***
              // *** Obtain all "OLD" Monitored Activity Assignment(s)
              // ***
              // 02/10/14 GVandy CQ42566  Performance fix when updating Legal 
              // Action assignments.
              if (IsEmpty(entities.LegalAction.StandardNumber))
              {
                foreach(var item in ReadLegalAction2())
                {
                  local.Other.DenormNumeric12 = entities.Other.Identifier;

                  foreach(var item1 in ReadInfrastructureMonitoredActivityAssignment())
                    
                  {
                    // ***
                    // *** UPDATE the "OLD" Monitored Activity Assignment
                    // ***
                    try
                    {
                      UpdateMonitoredActivityAssignment();
                    }
                    catch(Exception e)
                    {
                      switch(GetErrorCode(e))
                      {
                        case ErrorCode.AlreadyExists:
                          ExitState = "SP0000_MONITORED_ACT_ASSGN_NU";

                          goto Test3;
                        case ErrorCode.PermittedValueViolation:
                          ExitState = "SP0000_MONITORED_ACT_ASSGN_PV";

                          goto Test3;
                        case ErrorCode.DatabaseError:
                          break;
                        default:
                          throw;
                      }
                    }
                  }
                }
              }
              else
              {
                foreach(var item in ReadLegalAction3())
                {
                  local.Other.DenormNumeric12 = entities.Other.Identifier;

                  foreach(var item1 in ReadInfrastructureMonitoredActivityAssignment())
                    
                  {
                    // ***
                    // *** UPDATE the "OLD" Monitored Activity Assignment
                    // ***
                    try
                    {
                      UpdateMonitoredActivityAssignment();
                    }
                    catch(Exception e)
                    {
                      switch(GetErrorCode(e))
                      {
                        case ErrorCode.AlreadyExists:
                          ExitState = "SP0000_MONITORED_ACT_ASSGN_NU";

                          goto Test3;
                        case ErrorCode.PermittedValueViolation:
                          ExitState = "SP0000_MONITORED_ACT_ASSGN_PV";

                          goto Test3;
                        case ErrorCode.DatabaseError:
                          break;
                        default:
                          throw;
                      }
                    }
                  }
                }
              }

              // *** end
              // *** 02/10/00 SWSRCHF
              // *** Problem report H00082885
              var field1 = GetField(export.Group.Item.Common, "selectChar");

              field1.Protected = false;

              ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
            }
          }

          break;
        case "OBLIGATION":
          if (!ReadObligation())
          {
            ExitState = "OBLIGATION_NF";

            goto Test3;
          }

          // *** Check for Overlapping Assignments ***
          if (ReadObligationAssignment1())
          {
            ExitState = "SP0000_OVERLAPPING_ASSIGNMENT";

            var field1 =
              GetField(export.Group.Item.CaseUnitFunctionAssignmt,
              "effectiveDate");

            field1.Error = true;

            goto Test3;
          }

          local.ObligationAssignment.ReasonCode =
            export.Group.Item.CaseUnitFunctionAssignmt.ReasonCode;
          local.ObligationAssignment.OverrideInd =
            export.Group.Item.CaseUnitFunctionAssignmt.OverrideInd;
          local.ObligationAssignment.EffectiveDate =
            export.Group.Item.CaseUnitFunctionAssignmt.EffectiveDate;
          local.ObligationAssignment.DiscontinueDate =
            export.Group.Item.CaseUnitFunctionAssignmt.DiscontinueDate;
          local.ObligationAssignment.CreatedTimestamp =
            export.Group.Item.CaseUnitFunctionAssignmt.CreatedTimestamp;

          if (Equal(global.Command, "ADD"))
          {
            UseSpCabCreateOblgAssignment();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              var field1 = GetField(export.Group.Item.Common, "selectChar");

              field1.Protected = false;

              ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
            }
          }
          else if (Equal(global.Command, "UPDATE"))
          {
            UseSpCabUpdateObligAssignment();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              var field1 = GetField(export.Group.Item.Common, "selectChar");

              field1.Protected = false;

              ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
            }
          }

          break;
        case "ADMIN APPEAL":
          if (ReadAdministrativeAppeal())
          {
            ReadCsePerson();
          }
          else
          {
            ExitState = "LE0000_ADMINISTRATIVE_APPEAL_NF";

            goto Test3;
          }

          // *** Check for Overlapping Assignments ***
          if (ReadAdministrativeAppealAssignment1())
          {
            ExitState = "SP0000_OVERLAPPING_ASSIGNMENT";

            var field1 =
              GetField(export.Group.Item.CaseUnitFunctionAssignmt,
              "effectiveDate");

            field1.Error = true;

            goto Test3;
          }

          local.AdministrativeAppealAssignment.ReasonCode =
            export.Group.Item.CaseUnitFunctionAssignmt.ReasonCode;
          local.AdministrativeAppealAssignment.OverrideInd =
            export.Group.Item.CaseUnitFunctionAssignmt.OverrideInd;
          local.AdministrativeAppealAssignment.EffectiveDate =
            export.Group.Item.CaseUnitFunctionAssignmt.EffectiveDate;
          local.AdministrativeAppealAssignment.DiscontinueDate =
            export.Group.Item.CaseUnitFunctionAssignmt.DiscontinueDate;
          local.AdministrativeAppealAssignment.CreatedTimestamp =
            export.Group.Item.CaseUnitFunctionAssignmt.CreatedTimestamp;

          if (Equal(global.Command, "ADD"))
          {
            UseSpCabCreateAdminAppealAssgn();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              var field1 = GetField(export.Group.Item.Common, "selectChar");

              field1.Protected = false;

              ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
            }
          }
          else if (Equal(global.Command, "UPDATE"))
          {
            UseSpCabUpdateAdmAppAssignment();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              var field1 = GetField(export.Group.Item.Common, "selectChar");

              field1.Protected = false;

              ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
            }
          }

          break;
        default:
          ExitState = "ACO_NE0000_OPTION_UNAVAILABLE";

          var field = GetField(export.HeaderObject, "text20");

          field.Error = true;

          break;
      }

      if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
        ("ACO_NI0000_SUCCESSFUL_ADD") || IsExitState
        ("ACO_NI0000_SUCCESSFUL_UPDATE"))
      {
        global.Command = "DISPLAY";
      }
      else
      {
        // **** Need to have a roll back if an error is encountered ****
        UseEabRollbackCics();
      }
    }

Test3:

    if (Equal(global.Command, "DISPLAY"))
    {
      // *** Initialize group view in case of Re-Displays ***
      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        if (!export.Group.CheckSize())
        {
          break;
        }

        export.Group.Update.CaseUnitFunctionAssignmt.Assign(
          local.CaseUnitFunctionAssignmt);
        export.Group.Update.HcaseUnitFunctionAssignmt.Assign(
          local.CaseUnitFunctionAssignmt);
        MoveServiceProvider(local.ServiceProvider,
          export.Group.Update.ServiceProvider);
        export.Group.Update.HserviceProvider.UserId =
          local.ServiceProvider.UserId;
        export.Group.Update.Office.SystemGeneratedId =
          local.Office.SystemGeneratedId;
        export.Group.Update.Hoffice.SystemGeneratedId =
          local.Office.SystemGeneratedId;
        MoveOfficeServiceProvider(local.OfficeServiceProvider,
          export.Group.Update.OfficeServiceProvider);
        export.Group.Update.HofficeServiceProvider.RoleCode =
          local.OfficeServiceProvider.RoleCode;
        export.Group.Update.Common.SelectChar = "";
        export.Group.Update.PromptFunc.PromptField = "";
        export.Group.Update.PromptSp.PromptField = "";
        export.Group.Update.PromptRsn.PromptField = "";
        export.Group.Update.Protect.Flag = "";
      }

      export.Group.CheckIndex();
      export.Group.Count = 0;
      export.Group.Index = -1;

      switch(TrimEnd(export.HeaderObject.Text20))
      {
        case "CASE":
          if (IsEmpty(export.FirstTime.Flag))
          {
            export.FirstTime.Flag = "N";

            // ****  Set Display Headers on first past thru ****
            local.ConcatDelimLength.Count = 1;
            local.ConcatDelimiter.Text5 = "";

            local.Concat.Index = 0;
            local.Concat.CheckSize();

            local.Concat.Update.GrpoLocString.Text32 = "Case:";

            local.Concat.Index = 1;
            local.Concat.CheckSize();

            local.Concat.Update.GrpoLocString.Text32 = import.Hcase.Number;
            UseCoEabConcat10Delimit();
            export.HeaderObjTitle1.Text80 = local.ConcatResult.Text80;
          }

          export.Group.Index = -1;

          foreach(var item in ReadCaseAssignment())
          {
            ++export.Group.Index;
            export.Group.CheckSize();

            if (export.Group.Index >= Export.GroupGroup.Capacity)
            {
              goto Test4;
            }

            export.Group.Update.CaseUnitFunctionAssignmt.ReasonCode =
              entities.CaseAssignment.ReasonCode;
            export.Group.Update.CaseUnitFunctionAssignmt.OverrideInd =
              entities.CaseAssignment.OverrideInd;
            export.Group.Update.CaseUnitFunctionAssignmt.EffectiveDate =
              entities.CaseAssignment.EffectiveDate;
            export.Group.Update.CaseUnitFunctionAssignmt.DiscontinueDate =
              entities.CaseAssignment.DiscontinueDate;
            export.Group.Update.CaseUnitFunctionAssignmt.CreatedBy =
              entities.CaseAssignment.CreatedBy;
            export.Group.Update.CaseUnitFunctionAssignmt.CreatedTimestamp =
              entities.CaseAssignment.CreatedTimestamp;
            export.Group.Update.HcaseUnitFunctionAssignmt.Assign(
              export.Group.Item.CaseUnitFunctionAssignmt);
            export.Group.Update.Protect.Flag = "Y";

            if (ReadServiceProviderOfficeServiceProviderOffice3())
            {
              export.Group.Update.Office.SystemGeneratedId =
                entities.ExistingOffice.SystemGeneratedId;
              export.Group.Update.Hoffice.SystemGeneratedId =
                entities.ExistingOffice.SystemGeneratedId;
              MoveOfficeServiceProvider(entities.ExistingOfficeServiceProvider,
                export.Group.Update.OfficeServiceProvider);
              export.Group.Update.HofficeServiceProvider.RoleCode =
                entities.ExistingOfficeServiceProvider.RoleCode;
              MoveServiceProvider(entities.ExistingServiceProvider,
                export.Group.Update.ServiceProvider);
              export.Group.Update.HserviceProvider.UserId =
                entities.ExistingServiceProvider.UserId;
            }
            else
            {
              // ****   Don't think you want the program to bail when a case is 
              // not assigned  ****
              ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";

              goto Test4;
            }
          }

          break;
        case "CASE UNIT FUNCTION":
          if (IsEmpty(export.FirstTime.Flag))
          {
            export.FirstTime.Flag = "N";

            // ****  Set Display Headers on first past thru ****
            // Format Business Object Identifiers and Text
            local.ConcatDelimLength.Count = 1;
            local.ConcatDelimiter.Text5 = "";

            local.Concat.Index = 0;
            local.Concat.CheckSize();

            local.Concat.Update.GrpoLocString.Text32 = "Case:";

            local.Concat.Index = 1;
            local.Concat.CheckSize();

            local.Concat.Update.GrpoLocString.Text32 = export.Hcase.Number;

            local.Concat.Index = 2;
            local.Concat.CheckSize();

            local.Concat.Update.GrpoLocString.Text32 = "Case Unit:";

            local.Concat.Index = 3;
            local.Concat.CheckSize();

            local.Concat.Update.GrpoLocString.Text32 =
              NumberToString(export.HcaseUnit.CuNumber, 13, 3);
            UseCoEabConcat10Delimit();
            export.HeaderObjTitle1.Text80 = local.ConcatResult.Text80;
          }

          foreach(var item in ReadCaseUnitFunctionAssignmt2())
          {
            ++export.Group.Index;
            export.Group.CheckSize();

            if (export.Group.Index >= Export.GroupGroup.Capacity)
            {
              goto Test4;
            }

            MoveCaseUnitFunctionAssignmt1(entities.CaseUnitFunctionAssignmt,
              export.Group.Update.CaseUnitFunctionAssignmt);
            MoveCaseUnitFunctionAssignmt2(entities.CaseUnitFunctionAssignmt,
              export.Group.Update.HcaseUnitFunctionAssignmt);
            export.Group.Update.Protect.Flag = "Y";

            if (ReadServiceProviderOfficeServiceProviderOffice4())
            {
              export.Group.Update.Office.SystemGeneratedId =
                entities.ExistingOffice.SystemGeneratedId;
              export.Group.Update.Hoffice.SystemGeneratedId =
                entities.ExistingOffice.SystemGeneratedId;
              MoveOfficeServiceProvider(entities.ExistingOfficeServiceProvider,
                export.Group.Update.OfficeServiceProvider);
              export.Group.Update.HofficeServiceProvider.RoleCode =
                entities.ExistingOfficeServiceProvider.RoleCode;
              MoveServiceProvider(entities.ExistingServiceProvider,
                export.Group.Update.ServiceProvider);
              export.Group.Update.HserviceProvider.UserId =
                entities.ExistingServiceProvider.UserId;
            }
            else
            {
              // ****   Don't think you want the program to bail when a case is 
              // not assigned  ****
            }
          }

          break;
        case "INTERSTATE REFERRAL":
          // **** The following is required due to PM didn't want to changed 
          // import views - time is of essences ****
          // **** Decode Interstate case referral number out of the title line 1
          // ****
          local.Icase.TransSerialNumber =
            StringToNumber(Substring(
              export.HeaderObjTitle1.Text80, SpTextWorkArea.Text80_MaxLength,
            17, 12));

          if (IsEmpty(export.FirstTime.Flag))
          {
            local.Icase.TransactionDate =
              export.HiddenOfficeServiceProvider.EffectiveDate;
            export.HiddenNextTranInfo.LegalActionIdentifier =
              DateToInt(local.Icase.TransactionDate);
          }
          else
          {
            local.Icase.TransactionDate =
              IntToDate(export.HiddenNextTranInfo.LegalActionIdentifier.
                GetValueOrDefault());
          }

          // **** read Interstate case referral using icase number out of the 
          // title ****
          // **** line 1 & eff date from export_hidden OSP
          // ****
          if (!ReadInterstateCase())
          {
            ExitState = "INTERSTATE_CASE_NF";

            break;
          }

          export.FirstTime.Flag = "N";

          // **** ICASE screen should be passing the desired title ****
          // **** line 1 & 2 - no action required on ASIN part     ****
          // ****Read Each assignment for this Interstate Case ****
          foreach(var item in ReadInterstateCaseAssignment2())
          {
            ++export.Group.Index;
            export.Group.CheckSize();

            if (export.Group.Index >= Export.GroupGroup.Capacity)
            {
              break;
            }

            export.Group.Update.CaseUnitFunctionAssignmt.ReasonCode =
              entities.InterstateCaseAssignment.ReasonCode;
            export.Group.Update.CaseUnitFunctionAssignmt.OverrideInd =
              entities.InterstateCaseAssignment.OverrideInd;
            export.Group.Update.CaseUnitFunctionAssignmt.EffectiveDate =
              entities.InterstateCaseAssignment.EffectiveDate;
            export.Group.Update.CaseUnitFunctionAssignmt.DiscontinueDate =
              entities.InterstateCaseAssignment.DiscontinueDate;
            export.Group.Update.CaseUnitFunctionAssignmt.CreatedBy =
              entities.InterstateCaseAssignment.CreatedBy;
            export.Group.Update.CaseUnitFunctionAssignmt.CreatedTimestamp =
              entities.InterstateCaseAssignment.CreatedTimestamp;
            export.Group.Update.HcaseUnitFunctionAssignmt.Assign(
              export.Group.Item.CaseUnitFunctionAssignmt);
            export.Group.Update.Protect.Flag = "Y";

            if (ReadServiceProviderOfficeServiceProviderOffice5())
            {
              export.Group.Update.Office.SystemGeneratedId =
                entities.ExistingOffice.SystemGeneratedId;
              export.Group.Update.Hoffice.SystemGeneratedId =
                entities.ExistingOffice.SystemGeneratedId;
              MoveOfficeServiceProvider(entities.ExistingOfficeServiceProvider,
                export.Group.Update.OfficeServiceProvider);
              export.Group.Update.HofficeServiceProvider.RoleCode =
                entities.ExistingOfficeServiceProvider.RoleCode;
              MoveServiceProvider(entities.ExistingServiceProvider,
                export.Group.Update.ServiceProvider);
              export.Group.Update.HserviceProvider.UserId =
                entities.ExistingServiceProvider.UserId;
            }
          }

          break;
        case "MONITORED ACTIVITY":
          if (IsEmpty(export.FirstTime.Flag))
          {
            export.FirstTime.Flag = "N";

            // ****  Set Display Headers on first past thru ****
            if (!ReadMonitoredActivity1())
            {
              ExitState = "SP0000_MONITORED_ACTIVITY_NF";

              break;
            }

            // Format Business Object Identifiers and Text
            local.ConcatDelimLength.Count = 1;
            local.ConcatDelimiter.Text5 = "";
            local.Concat.Index = -1;

            if (ReadInfrastructure())
            {
              ++local.Concat.Index;
              local.Concat.CheckSize();

              local.Concat.Update.GrpoLocString.Text32 = "Case #:";
              local.Concat.Update.GrpoLocString.Text32 =
                TrimEnd(local.Concat.Item.GrpoLocString.Text32) + entities
                .Infrastructure.CaseNumber;

              ++local.Concat.Index;
              local.Concat.CheckSize();

              local.Concat.Update.GrpoLocString.Text32 = "Case Unit #:";
              local.Concat.Update.GrpoLocString.Text32 =
                TrimEnd(local.Concat.Item.GrpoLocString.Text32) + NumberToString
                (entities.Infrastructure.CaseUnitNumber.GetValueOrDefault(), 13,
                3);

              ++local.Concat.Index;
              local.Concat.CheckSize();

              local.Concat.Update.GrpoLocString.Text32 = "CSE Person #:";
              local.Concat.Update.GrpoLocString.Text32 =
                TrimEnd(local.Concat.Item.GrpoLocString.Text32) + entities
                .Infrastructure.CsePersonNumber;
            }

            UseCoEabConcat10Delimit();
            export.HeaderObjTitle1.Text80 = local.ConcatResult.Text80;
            local.ConcatResult.Text80 = "";

            for(local.Concat.Index = 0; local.Concat.Index < Local
              .ConcatGroup.Capacity; ++local.Concat.Index)
            {
              if (!local.Concat.CheckSize())
              {
                break;
              }

              local.Concat.Update.GrpoLocString.Text32 = "";
            }

            local.Concat.CheckIndex();

            local.Concat.Index = 0;
            local.Concat.CheckSize();

            local.Concat.Update.GrpoLocString.Text32 =
              entities.MonitoredActivity.Name;

            ++local.Concat.Index;
            local.Concat.CheckSize();

            local.DateWorkArea.Date = entities.MonitoredActivity.StartDate;
            UseCabConvertDate2String();
            local.Concat.Update.GrpoLocString.Text32 = "Start Date:";
            local.Concat.Update.GrpoLocString.Text32 =
              TrimEnd(local.Concat.Item.GrpoLocString.Text32) + local
              .TextWorkArea.Text8;
            UseCoEabConcat10Delimit();
            export.HeaderObjTitle2.Text80 = local.ConcatResult.Text80;
          }

          foreach(var item in ReadMonitoredActivityAssignment2())
          {
            ++export.Group.Index;
            export.Group.CheckSize();

            if (export.Group.Index >= Export.GroupGroup.Capacity)
            {
              goto Test4;
            }

            export.Group.Update.CaseUnitFunctionAssignmt.ReasonCode =
              entities.Old.ReasonCode;
            export.Group.Update.CaseUnitFunctionAssignmt.OverrideInd =
              entities.Old.OverrideInd;
            export.Group.Update.CaseUnitFunctionAssignmt.EffectiveDate =
              entities.Old.EffectiveDate;
            export.Group.Update.CaseUnitFunctionAssignmt.DiscontinueDate =
              entities.Old.DiscontinueDate;
            export.Group.Update.CaseUnitFunctionAssignmt.CreatedBy =
              entities.Old.CreatedBy;
            export.Group.Update.CaseUnitFunctionAssignmt.CreatedTimestamp =
              entities.Old.CreatedTimestamp;
            export.Group.Update.HcaseUnitFunctionAssignmt.Assign(
              export.Group.Item.CaseUnitFunctionAssignmt);
            export.Group.Update.Common.SelectChar = "";
            export.Group.Update.PromptFunc.PromptField = "";
            export.Group.Update.PromptSp.PromptField = "";
            export.Group.Update.PromptRsn.PromptField = "";
            export.Group.Update.Protect.Flag = "Y";

            if (ReadServiceProviderOfficeServiceProviderOffice7())
            {
              export.Group.Update.Office.SystemGeneratedId =
                entities.ExistingOffice.SystemGeneratedId;
              export.Group.Update.Hoffice.SystemGeneratedId =
                entities.ExistingOffice.SystemGeneratedId;
              MoveOfficeServiceProvider(entities.ExistingOfficeServiceProvider,
                export.Group.Update.OfficeServiceProvider);
              export.Group.Update.HofficeServiceProvider.RoleCode =
                entities.ExistingOfficeServiceProvider.RoleCode;
              MoveServiceProvider(entities.ExistingServiceProvider,
                export.Group.Update.ServiceProvider);
              export.Group.Update.HserviceProvider.UserId =
                entities.ExistingServiceProvider.UserId;
            }
            else
            {
              ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";

              goto Test4;
            }
          }

          break;
        case "PA REFERRAL":
          if (IsEmpty(export.FirstTime.Flag))
          {
            export.FirstTime.Flag = "N";

            // ****  Set Display Headers on first past thru ****
            // Format Business Object Identifiers and Text
            local.ConcatDelimLength.Count = 1;
            local.ConcatDelimiter.Text5 = "";

            local.Concat.Index = 0;
            local.Concat.CheckSize();

            local.Concat.Update.GrpoLocString.Text32 = "Type:";

            local.Concat.Index = 1;
            local.Concat.CheckSize();

            local.Concat.Update.GrpoLocString.Text32 = export.HpaReferral.Type1;

            local.Concat.Index = 2;
            local.Concat.CheckSize();

            local.Concat.Update.GrpoLocString.Text32 = "Number";

            local.Concat.Index = 3;
            local.Concat.CheckSize();

            local.Concat.Update.GrpoLocString.Text32 =
              export.HpaReferral.Number;
            UseCoEabConcat10Delimit();
            export.HeaderObjTitle1.Text80 = local.ConcatResult.Text80;
          }

          foreach(var item in ReadPaReferralAssignment2())
          {
            ++export.Group.Index;
            export.Group.CheckSize();

            if (export.Group.Index >= Export.GroupGroup.Capacity)
            {
              goto Test4;
            }

            export.Group.Update.CaseUnitFunctionAssignmt.ReasonCode =
              entities.PaReferralAssignment.ReasonCode;
            export.Group.Update.CaseUnitFunctionAssignmt.OverrideInd =
              entities.PaReferralAssignment.OverrideInd;
            export.Group.Update.CaseUnitFunctionAssignmt.EffectiveDate =
              entities.PaReferralAssignment.EffectiveDate;
            export.Group.Update.CaseUnitFunctionAssignmt.DiscontinueDate =
              entities.PaReferralAssignment.DiscontinueDate;
            export.Group.Update.CaseUnitFunctionAssignmt.CreatedBy =
              entities.PaReferralAssignment.CreatedBy;
            export.Group.Update.CaseUnitFunctionAssignmt.CreatedTimestamp =
              entities.PaReferralAssignment.CreatedTimestamp;
            export.Group.Update.HcaseUnitFunctionAssignmt.Assign(
              export.Group.Item.CaseUnitFunctionAssignmt);
            export.Group.Update.Common.SelectChar = "";
            export.Group.Update.PromptFunc.PromptField = "";
            export.Group.Update.PromptSp.PromptField = "";
            export.Group.Update.PromptRsn.PromptField = "";
            export.Group.Update.Protect.Flag = "Y";

            if (ReadServiceProviderOfficeServiceProviderOffice9())
            {
              export.Group.Update.Office.SystemGeneratedId =
                entities.ExistingOffice.SystemGeneratedId;
              export.Group.Update.Hoffice.SystemGeneratedId =
                entities.ExistingOffice.SystemGeneratedId;
              MoveOfficeServiceProvider(entities.ExistingOfficeServiceProvider,
                export.Group.Update.OfficeServiceProvider);
              export.Group.Update.HofficeServiceProvider.RoleCode =
                entities.ExistingOfficeServiceProvider.RoleCode;
              MoveServiceProvider(entities.ExistingServiceProvider,
                export.Group.Update.ServiceProvider);
              export.Group.Update.HserviceProvider.UserId =
                entities.ExistingServiceProvider.UserId;
            }
          }

          break;
        case "LEGAL REFERRAL":
          if (IsEmpty(export.FirstTime.Flag))
          {
            export.FirstTime.Flag = "N";

            // ****  Set Display Headers on first past thru ****
            local.ConcatDelimLength.Count = 1;
            local.ConcatDelimiter.Text5 = "";

            local.Concat.Index = 0;
            local.Concat.CheckSize();

            local.Concat.Update.GrpoLocString.Text32 = "Case:";

            local.Concat.Index = 1;
            local.Concat.CheckSize();

            local.Concat.Update.GrpoLocString.Text32 = export.Hcase.Number;

            local.Concat.Index = 2;
            local.Concat.CheckSize();

            local.Concat.Update.GrpoLocString.Text32 = "Legal Request ID:";

            local.Concat.Index = 3;
            local.Concat.CheckSize();

            local.Concat.Update.GrpoLocString.Text32 =
              NumberToString(export.HlegalReferral.Identifier, 13, 3);
            UseCoEabConcat10Delimit();
            export.HeaderObjTitle1.Text80 = local.ConcatResult.Text80;
          }

          local.ActiveAssignmentFound.Flag = "N";

          foreach(var item in ReadLegalReferralAssignment2())
          {
            ++export.Group.Index;
            export.Group.CheckSize();

            if (export.Group.Index >= Export.GroupGroup.Capacity)
            {
              break;
            }

            export.Group.Update.CaseUnitFunctionAssignmt.ReasonCode =
              entities.LegalReferralAssignment.ReasonCode;
            export.Group.Update.CaseUnitFunctionAssignmt.OverrideInd =
              entities.LegalReferralAssignment.OverrideInd;
            export.Group.Update.CaseUnitFunctionAssignmt.EffectiveDate =
              entities.LegalReferralAssignment.EffectiveDate;
            export.Group.Update.CaseUnitFunctionAssignmt.DiscontinueDate =
              entities.LegalReferralAssignment.DiscontinueDate;
            export.Group.Update.CaseUnitFunctionAssignmt.CreatedBy =
              entities.LegalReferralAssignment.CreatedBy;
            export.Group.Update.CaseUnitFunctionAssignmt.CreatedTimestamp =
              entities.LegalReferralAssignment.CreatedTimestamp;
            export.Group.Update.HcaseUnitFunctionAssignmt.Assign(
              export.Group.Item.CaseUnitFunctionAssignmt);
            export.Group.Update.Common.SelectChar = "";
            export.Group.Update.PromptFunc.PromptField = "";
            export.Group.Update.PromptSp.PromptField = "";
            export.Group.Update.PromptRsn.PromptField = "";
            export.Group.Update.Protect.Flag = "Y";

            if (ReadServiceProviderOfficeServiceProviderOffice6())
            {
              export.Group.Update.Office.SystemGeneratedId =
                entities.ExistingOffice.SystemGeneratedId;
              export.Group.Update.Hoffice.SystemGeneratedId =
                entities.ExistingOffice.SystemGeneratedId;
              MoveOfficeServiceProvider(entities.ExistingOfficeServiceProvider,
                export.Group.Update.OfficeServiceProvider);
              export.Group.Update.HofficeServiceProvider.RoleCode =
                entities.ExistingOfficeServiceProvider.RoleCode;
              MoveServiceProvider(entities.ExistingServiceProvider,
                export.Group.Update.ServiceProvider);
              export.Group.Update.HserviceProvider.UserId =
                entities.ExistingServiceProvider.UserId;
            }

            if (!Lt(local.CurrentDate.Date,
              entities.LegalReferralAssignment.EffectiveDate) && !
              Lt(entities.LegalReferralAssignment.DiscontinueDate,
              local.CurrentDate.Date))
            {
              local.ActiveAssignmentFound.Flag = "Y";
            }
          }

          break;
        case "LEGAL ACTION":
          if (IsEmpty(export.FirstTime.Flag))
          {
            export.FirstTime.Flag = "N";

            // ****  Set Display Headers on first past thru ****
            // ****  Make a single compound read out of the next four reads to 
            // eliminate swap outs  ****
            if (ReadLegalAction1())
            {
              if (ReadTribunal())
              {
                local.Tribunal.Assign(entities.Tribunal);

                if (!ReadFips())
                {
                  ReadFipsTribAddress();
                }
              }
            }
            else
            {
              ExitState = "LEGAL_ACTION_NF";

              goto Test5;
            }

            // Format Business Object Identifiers and Text
            local.ConcatDelimLength.Count = 1;
            local.ConcatDelimiter.Text5 = "";

            local.Concat.Index = 0;
            local.Concat.CheckSize();

            local.Concat.Update.GrpoLocString.Text32 =
              entities.LegalAction.CourtCaseNumber ?? Spaces(32);
            local.Concat.Update.GrpoLocString.Text32 =
              TrimEnd(local.Concat.Item.GrpoLocString.Text32) + "/";
            local.Concat.Update.GrpoLocString.Text32 =
              TrimEnd(local.Concat.Item.GrpoLocString.Text32) + entities
              .Fips.StateAbbreviation;
            local.Concat.Update.GrpoLocString.Text32 =
              TrimEnd(local.Concat.Item.GrpoLocString.Text32) + "/";
            local.Concat.Update.GrpoLocString.Text32 =
              TrimEnd(local.Concat.Item.GrpoLocString.Text32) + entities
              .Fips.CountyAbbreviation;
            local.Concat.Update.GrpoLocString.Text32 =
              TrimEnd(local.Concat.Item.GrpoLocString.Text32) + "/";
            local.Concat.Update.GrpoLocString.Text32 =
              TrimEnd(local.Concat.Item.GrpoLocString.Text32) + entities
              .Foreign.Country;

            local.Concat.Index = 1;
            local.Concat.CheckSize();

            local.Code.CodeName = "ACTION TAKEN";
            local.CodeValue.Cdvalue = entities.LegalAction.ActionTaken;
            UseCabGetCodeValueDescription();

            if (IsEmpty(local.CodeValue.Description))
            {
              local.Concat.Update.GrpoLocString.Text32 =
                entities.LegalAction.ActionTaken;

              // *** Problem report H00082885
              // *** 02/10/00 SWSRCHF
              // *** start
              export.Detail.Text32 = "";

              // *** end
              // *** 02/10/00 SWSRCHF
              // *** Problem report H00082885
            }
            else
            {
              local.Concat.Update.GrpoLocString.Text32 =
                local.CodeValue.Description;

              // *** Problem report H00082885
              // *** 02/10/00 SWSRCHF
              // *** start
              export.Detail.Text32 = local.Concat.Item.GrpoLocString.Text32;

              // *** end
              // *** 02/10/00 SWSRCHF
              // *** Problem report H00082885
            }

            UseCoEabConcat10Delimit();
            export.HeaderObjTitle1.Text80 = local.ConcatResult.Text80;
            local.ConcatResult.Text80 = "";

            for(local.Concat.Index = 0; local.Concat.Index < Local
              .ConcatGroup.Capacity; ++local.Concat.Index)
            {
              if (!local.Concat.CheckSize())
              {
                break;
              }

              local.Concat.Update.GrpoLocString.Text32 = "";
            }

            local.Concat.CheckIndex();

            local.Concat.Index = 0;
            local.Concat.CheckSize();

            local.DateWorkArea.Date = entities.LegalAction.FiledDate;

            // *** Problem report H00082885
            // *** 02/10/00 SWSRCHF
            // *** start
            export.Compare.FiledDate = entities.LegalAction.FiledDate;

            // *** end
            // *** 02/10/00 SWSRCHF
            // *** Problem report H00082885
            UseCabConvertDate2String();
            local.Concat.Update.GrpoLocString.Text32 = "Filed Date:";
            local.Concat.Update.GrpoLocString.Text32 =
              TrimEnd(local.Concat.Item.GrpoLocString.Text32) + local
              .TextWorkArea.Text8;
            UseCoEabConcat10Delimit();
            export.HeaderObjTitle2.Text80 = local.ConcatResult.Text80;
          }

          foreach(var item in ReadLegalActionAssigment3())
          {
            ++export.Group.Index;
            export.Group.CheckSize();

            if (export.Group.Index >= Export.GroupGroup.Capacity)
            {
              goto Test4;
            }

            export.Group.Update.CaseUnitFunctionAssignmt.ReasonCode =
              entities.LegalActionAssigment.ReasonCode;
            export.Group.Update.CaseUnitFunctionAssignmt.OverrideInd =
              entities.LegalActionAssigment.OverrideInd;
            export.Group.Update.CaseUnitFunctionAssignmt.EffectiveDate =
              entities.LegalActionAssigment.EffectiveDate;
            export.Group.Update.CaseUnitFunctionAssignmt.DiscontinueDate =
              entities.LegalActionAssigment.DiscontinueDate;
            export.Group.Update.CaseUnitFunctionAssignmt.CreatedBy =
              entities.LegalActionAssigment.CreatedBy;
            export.Group.Update.CaseUnitFunctionAssignmt.CreatedTimestamp =
              entities.LegalActionAssigment.CreatedTimestamp;
            export.Group.Update.HcaseUnitFunctionAssignmt.Assign(
              export.Group.Item.CaseUnitFunctionAssignmt);
            export.Group.Update.Common.SelectChar = "";
            export.Group.Update.PromptFunc.PromptField = "";
            export.Group.Update.PromptSp.PromptField = "";
            export.Group.Update.PromptRsn.PromptField = "";
            export.Group.Update.Protect.Flag = "Y";

            if (ReadServiceProviderOfficeServiceProviderOffice1())
            {
              export.Group.Update.Office.SystemGeneratedId =
                entities.ExistingOffice.SystemGeneratedId;
              export.Group.Update.Hoffice.SystemGeneratedId =
                entities.ExistingOffice.SystemGeneratedId;
              MoveOfficeServiceProvider(entities.ExistingOfficeServiceProvider,
                export.Group.Update.OfficeServiceProvider);
              export.Group.Update.HofficeServiceProvider.RoleCode =
                entities.ExistingOfficeServiceProvider.RoleCode;
              MoveServiceProvider(entities.ExistingServiceProvider,
                export.Group.Update.ServiceProvider);
              export.Group.Update.HserviceProvider.UserId =
                entities.ExistingServiceProvider.UserId;
            }
            else
            {
              ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";

              goto Test4;
            }
          }

          break;
        case "OBLIGATION":
          if (IsEmpty(export.FirstTime.Flag))
          {
            export.FirstTime.Flag = "N";

            // ****  Set Display Headers on first past thru ****
            // Format Business Object Identifiers and Text
            local.ConcatDelimLength.Count = 1;
            local.ConcatDelimiter.Text5 = "";

            local.Concat.Index = 0;
            local.Concat.CheckSize();

            local.Concat.Update.GrpoLocString.Text32 = "Obligor #:";

            local.Concat.Index = 1;
            local.Concat.CheckSize();

            local.Concat.Update.GrpoLocString.Text32 = export.HcsePerson.Number;

            local.Concat.Index = 2;
            local.Concat.CheckSize();

            local.Concat.Update.GrpoLocString.Text32 =
              export.HcsePersonsWorkSet.FormattedName;
            UseCoEabConcat10Delimit();
            export.HeaderObjTitle1.Text80 = local.ConcatResult.Text80;

            local.Concat.Index = 0;
            local.Concat.CheckSize();

            local.Concat.Update.GrpoLocString.Text32 = "Obligation Type :";

            local.Concat.Index = 1;
            local.Concat.CheckSize();

            local.Concat.Update.GrpoLocString.Text32 =
              export.HobligationType.Code;

            local.Concat.Index = 2;
            local.Concat.CheckSize();

            local.Concat.Update.GrpoLocString.Text32 = "Court Order #:";

            local.Concat.Index = 3;
            local.Concat.CheckSize();

            local.Concat.Update.GrpoLocString.Text32 =
              export.HlegalAction.StandardNumber ?? Spaces(32);
            UseCoEabConcat10Delimit();
            export.HeaderObjTitle2.Text80 = local.ConcatResult.Text80;
          }

          foreach(var item in ReadObligationAssignment2())
          {
            ++export.Group.Index;
            export.Group.CheckSize();

            if (export.Group.Index >= Export.GroupGroup.Capacity)
            {
              goto Test4;
            }

            export.Group.Update.CaseUnitFunctionAssignmt.ReasonCode =
              entities.ObligationAssignment.ReasonCode;
            export.Group.Update.CaseUnitFunctionAssignmt.OverrideInd =
              entities.ObligationAssignment.OverrideInd;
            export.Group.Update.CaseUnitFunctionAssignmt.EffectiveDate =
              entities.ObligationAssignment.EffectiveDate;
            export.Group.Update.CaseUnitFunctionAssignmt.DiscontinueDate =
              entities.ObligationAssignment.DiscontinueDate;
            export.Group.Update.CaseUnitFunctionAssignmt.CreatedBy =
              entities.ObligationAssignment.CreatedBy;
            export.Group.Update.CaseUnitFunctionAssignmt.CreatedTimestamp =
              entities.ObligationAssignment.CreatedTimestamp;
            export.Group.Update.HcaseUnitFunctionAssignmt.Assign(
              export.Group.Item.CaseUnitFunctionAssignmt);
            export.Group.Update.Common.SelectChar = "";
            export.Group.Update.PromptFunc.PromptField = "";
            export.Group.Update.PromptSp.PromptField = "";
            export.Group.Update.PromptRsn.PromptField = "";
            export.Group.Update.Protect.Flag = "Y";

            if (ReadServiceProviderOfficeServiceProviderOffice8())
            {
              export.Group.Update.Office.SystemGeneratedId =
                entities.ExistingOffice.SystemGeneratedId;
              export.Group.Update.Hoffice.SystemGeneratedId =
                entities.ExistingOffice.SystemGeneratedId;
              MoveOfficeServiceProvider(entities.ExistingOfficeServiceProvider,
                export.Group.Update.OfficeServiceProvider);
              export.Group.Update.HofficeServiceProvider.RoleCode =
                entities.ExistingOfficeServiceProvider.RoleCode;
              MoveServiceProvider(entities.ExistingServiceProvider,
                export.Group.Update.ServiceProvider);
              export.Group.Update.HserviceProvider.UserId =
                entities.ExistingServiceProvider.UserId;
            }
            else
            {
              ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";

              goto Test4;
            }
          }

          break;
        case "ADMIN APPEAL":
          if (IsEmpty(export.FirstTime.Flag))
          {
            export.FirstTime.Flag = "N";

            // ****  Set Display Headers on first past thru ****
            // ****  See if you can combine these two reads ****
            if (ReadAdministrativeAppeal())
            {
              // Format Business Object Identifiers and Text
              local.ConcatDelimLength.Count = 1;
              local.ConcatDelimiter.Text5 = "";
              local.Concat.Index = -1;

              if (ReadCsePerson())
              {
                ++local.Concat.Index;
                local.Concat.CheckSize();

                local.Concat.Update.GrpoLocString.Text32 = "CSE Person:";
                local.Concat.Update.GrpoLocString.Text32 =
                  TrimEnd(local.Concat.Item.GrpoLocString.Text32) + entities
                  .CsePerson.Number;
              }

              ++local.Concat.Index;
              local.Concat.CheckSize();

              local.Concat.Update.GrpoLocString.Text32 = "Adm Appeal No:";
              local.Concat.Update.GrpoLocString.Text32 =
                TrimEnd(local.Concat.Item.GrpoLocString.Text32) + entities
                .AdministrativeAppeal.Number;

              ++local.Concat.Index;
              local.Concat.CheckSize();

              local.Concat.Update.GrpoLocString.Text32 = "Req Date:";
              local.DateWorkArea.Date =
                entities.AdministrativeAppeal.RequestDate;
              UseCabConvertDate2String();
              local.Concat.Update.GrpoLocString.Text32 =
                TrimEnd(local.Concat.Item.GrpoLocString.Text32) + local
                .TextWorkArea.Text8;
              UseCoEabConcat10Delimit();
              export.HeaderObjTitle1.Text80 = local.ConcatResult.Text80;
            }
            else
            {
              ExitState = "LE0000_ADMINISTRATIVE_APPEAL_NF";
            }
          }

          foreach(var item in ReadAdministrativeAppealAssignment2())
          {
            ++export.Group.Index;
            export.Group.CheckSize();

            if (export.Group.Index >= Export.GroupGroup.Capacity)
            {
              goto Test4;
            }

            export.Group.Update.CaseUnitFunctionAssignmt.ReasonCode =
              entities.AdministrativeAppealAssignment.ReasonCode;
            export.Group.Update.CaseUnitFunctionAssignmt.OverrideInd =
              entities.AdministrativeAppealAssignment.OverrideInd;
            export.Group.Update.CaseUnitFunctionAssignmt.EffectiveDate =
              entities.AdministrativeAppealAssignment.EffectiveDate;
            export.Group.Update.CaseUnitFunctionAssignmt.DiscontinueDate =
              entities.AdministrativeAppealAssignment.DiscontinueDate;
            export.Group.Update.CaseUnitFunctionAssignmt.CreatedBy =
              entities.AdministrativeAppealAssignment.CreatedBy;
            export.Group.Update.CaseUnitFunctionAssignmt.CreatedTimestamp =
              entities.AdministrativeAppealAssignment.CreatedTimestamp;
            export.Group.Update.HcaseUnitFunctionAssignmt.Assign(
              export.Group.Item.CaseUnitFunctionAssignmt);
            export.Group.Update.Common.SelectChar = "";
            export.Group.Update.PromptFunc.PromptField = "";
            export.Group.Update.PromptSp.PromptField = "";
            export.Group.Update.PromptRsn.PromptField = "";
            export.Group.Update.Protect.Flag = "Y";

            if (ReadServiceProviderOfficeServiceProviderOffice2())
            {
              export.Group.Update.Office.SystemGeneratedId =
                entities.ExistingOffice.SystemGeneratedId;
              export.Group.Update.Hoffice.SystemGeneratedId =
                entities.ExistingOffice.SystemGeneratedId;
              MoveOfficeServiceProvider(entities.ExistingOfficeServiceProvider,
                export.Group.Update.OfficeServiceProvider);
              export.Group.Update.HofficeServiceProvider.RoleCode =
                entities.ExistingOfficeServiceProvider.RoleCode;
              MoveServiceProvider(entities.ExistingServiceProvider,
                export.Group.Update.ServiceProvider);
              export.Group.Update.HserviceProvider.UserId =
                entities.ExistingServiceProvider.UserId;
            }
          }

          break;
        default:
          break;
      }

Test4:

      if (export.Group.IsEmpty)
      {
        ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
      }
      else if (export.Group.IsFull)
      {
        ExitState = "ACO_NI0000_LIST_IS_FULL";

        // ----------------------------------------------------------------------------------------
        // G. Pan     5/05/2008   CQ4749
        // 	Added following statement.
        // ----------------------------------------------------------------------------------------
        export.Group.Index = Export.GroupGroup.Capacity - 1;
        export.Group.CheckSize();

        export.HeaderStart.Date =
          export.Group.Item.CaseUnitFunctionAssignmt.EffectiveDate;
      }
      else
      {
        export.HeaderStart.Date =
          export.Group.Item.CaseUnitFunctionAssignmt.EffectiveDate;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }
      }
    }

Test5:

    // ***  Set Screen Protection ***
    for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
      export.Group.Index)
    {
      if (!export.Group.CheckSize())
      {
        break;
      }

      if (AsChar(export.Group.Item.Protect.Flag) == 'Y')
      {
        var field1 =
          GetField(export.Group.Item.CaseUnitFunctionAssignmt, "effectiveDate");
          

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 =
          GetField(export.Group.Item.CaseUnitFunctionAssignmt, "function");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.Group.Item.PromptSp, "promptField");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.Group.Item.PromptFunc, "promptField");

        field4.Color = "cyan";
        field4.Protected = true;

        // *** Work request 000299
        // *** 11/21/00 SWSRCHF
        var field5 = GetField(export.Group.Item.ServiceProvider, "userId");

        field5.Color = "cyan";
        field5.Highlighting = Highlighting.Normal;
        field5.Protected = true;

        switch(TrimEnd(export.HeaderObject.Text20))
        {
          case "CASE":
            var field6 =
              GetField(export.Group.Item.CaseUnitFunctionAssignmt,
              "discontinueDate");

            field6.Color = "cyan";
            field6.Protected = true;

            break;
          case "CASE UNIT FUNCTION":
            var field7 =
              GetField(export.Group.Item.CaseUnitFunctionAssignmt, "function");

            field7.Color = "cyan";
            field7.Protected = true;

            var field8 = GetField(export.Group.Item.PromptFunc, "promptField");

            field8.Color = "cyan";
            field8.Protected = true;

            break;
          case "MONITORED ACTIVITY":
            break;
          case "PA REFERRAL":
            break;
          case "LEGAL REFERRAL":
            // -- 04/26/01 GVandy WR251 - Override indicator is now the only 
            // updatable field.
            var field9 =
              GetField(export.Group.Item.CaseUnitFunctionAssignmt,
              "discontinueDate");

            field9.Color = "cyan";
            field9.Protected = true;

            var field10 =
              GetField(export.Group.Item.CaseUnitFunctionAssignmt, "reasonCode");
              

            field10.Color = "cyan";
            field10.Protected = true;

            var field11 = GetField(export.Group.Item.PromptRsn, "promptField");

            field11.Color = "cyan";
            field11.Protected = true;

            break;
          case "LEGAL ACTION":
            break;
          case "OBLIGATION":
            break;
          case "ADMIN APPEAL":
            break;
          default:
            break;
        }

        if (Lt(export.Group.Item.CaseUnitFunctionAssignmt.DiscontinueDate,
          local.CurrentDate.Date) && Equal
          (export.Group.Item.CaseUnitFunctionAssignmt.DiscontinueDate,
          export.Group.Item.HcaseUnitFunctionAssignmt.DiscontinueDate))
        {
          var field6 = GetField(export.Group.Item.Common, "selectChar");

          field6.Color = "cyan";
          field6.Protected = true;

          var field7 =
            GetField(export.Group.Item.CaseUnitFunctionAssignmt, "reasonCode");

          field7.Color = "cyan";
          field7.Protected = true;

          var field8 =
            GetField(export.Group.Item.CaseUnitFunctionAssignmt, "overrideInd");
            

          field8.Color = "cyan";
          field8.Protected = true;

          var field9 =
            GetField(export.Group.Item.CaseUnitFunctionAssignmt, "effectiveDate");
            

          field9.Color = "cyan";
          field9.Protected = true;

          var field10 =
            GetField(export.Group.Item.CaseUnitFunctionAssignmt,
            "discontinueDate");

          field10.Color = "cyan";
          field10.Protected = true;

          var field11 =
            GetField(export.Group.Item.CaseUnitFunctionAssignmt, "function");

          field11.Color = "cyan";
          field11.Protected = true;

          var field12 = GetField(export.Group.Item.PromptSp, "promptField");

          field12.Color = "cyan";
          field12.Protected = true;

          var field13 = GetField(export.Group.Item.PromptRsn, "promptField");

          field13.Color = "cyan";
          field13.Protected = true;
        }
      }

      // *** Reset the Discontinue date back to null date for display ***
      if (Equal(export.Group.Item.CaseUnitFunctionAssignmt.DiscontinueDate,
        local.MaxDate.Date))
      {
        export.Group.Update.CaseUnitFunctionAssignmt.DiscontinueDate =
          local.NullDate.Date;
      }
    }

    export.Group.CheckIndex();
  }

  private static void MoveAdministrativeAppealAssignment(
    AdministrativeAppealAssignment source,
    AdministrativeAppealAssignment target)
  {
    target.ReasonCode = source.ReasonCode;
    target.OverrideInd = source.OverrideInd;
    target.DiscontinueDate = source.DiscontinueDate;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
  }

  private static void MoveCaseUnitFunctionAssignmt1(
    CaseUnitFunctionAssignmt source, CaseUnitFunctionAssignmt target)
  {
    target.ReasonCode = source.ReasonCode;
    target.OverrideInd = source.OverrideInd;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.Function = source.Function;
  }

  private static void MoveCaseUnitFunctionAssignmt2(
    CaseUnitFunctionAssignmt source, CaseUnitFunctionAssignmt target)
  {
    target.ReasonCode = source.ReasonCode;
    target.OverrideInd = source.OverrideInd;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
    target.CreatedBy = source.CreatedBy;
    target.Function = source.Function;
  }

  private static void MoveCodeValue(CodeValue source, CodeValue target)
  {
    target.Cdvalue = source.Cdvalue;
    target.Description = source.Description;
  }

  private static void MoveConcatToStrings(Local.ConcatGroup source,
    CoEabConcat10Delimit.Import.StringsGroup target)
  {
    target.String1.Text32 = source.GrpoLocString.Text32;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
  }

  private static void MoveInterstateCase(InterstateCase source,
    InterstateCase target)
  {
    target.TransSerialNumber = source.TransSerialNumber;
    target.TransactionDate = source.TransactionDate;
  }

  private static void MoveLegalActionAssigment(LegalActionAssigment source,
    LegalActionAssigment target)
  {
    target.ReasonCode = source.ReasonCode;
    target.OverrideInd = source.OverrideInd;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.DiscontinueDate = source.DiscontinueDate;
  }

  private static void MoveLegalReferralAssignment1(
    LegalReferralAssignment source, LegalReferralAssignment target)
  {
    target.OverrideInd = source.OverrideInd;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveLegalReferralAssignment2(
    LegalReferralAssignment source, LegalReferralAssignment target)
  {
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
  }

  private static void MoveMonitoredActivity(MonitoredActivity source,
    MonitoredActivity target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.TypeCode = source.TypeCode;
  }

  private static void MoveMonitoredActivityAssignment(
    MonitoredActivityAssignment source, MonitoredActivityAssignment target)
  {
    target.ReasonCode = source.ReasonCode;
    target.EffectiveDate = source.EffectiveDate;
    target.OverrideInd = source.OverrideInd;
    target.DiscontinueDate = source.DiscontinueDate;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveOfficeServiceProvider(OfficeServiceProvider source,
    OfficeServiceProvider target)
  {
    target.RoleCode = source.RoleCode;
    target.EffectiveDate = source.EffectiveDate;
  }

  private static void MovePaReferralAssignment(PaReferralAssignment source,
    PaReferralAssignment target)
  {
    target.ReasonCode = source.ReasonCode;
    target.OverrideInd = source.OverrideInd;
    target.DiscontinueDate = source.DiscontinueDate;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
  }

  private static void MoveServiceProvider(ServiceProvider source,
    ServiceProvider target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.UserId = source.UserId;
  }

  private void UseCabConvertDate2String()
  {
    var useImport = new CabConvertDate2String.Import();
    var useExport = new CabConvertDate2String.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(CabConvertDate2String.Execute, useImport, useExport);

    local.TextWorkArea.Text8 = useExport.TextWorkArea.Text8;
  }

  private void UseCabGetCodeValueDescription()
  {
    var useImport = new CabGetCodeValueDescription.Import();
    var useExport = new CabGetCodeValueDescription.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.Code.CodeName;

    Call(CabGetCodeValueDescription.Execute, useImport, useExport);

    MoveCodeValue(useExport.CodeValue, local.CodeValue);
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.Code.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Flag = useExport.ValidCode.Flag;
  }

  private void UseCoEabConcat10Delimit()
  {
    var useImport = new CoEabConcat10Delimit.Import();
    var useExport = new CoEabConcat10Delimit.Export();

    useImport.DelimiterLength.Count = local.ConcatDelimLength.Count;
    useImport.Delimiter.Text5 = local.ConcatDelimiter.Text5;
    local.Concat.CopyTo(useImport.Strings, MoveConcatToStrings);
    useExport.ResultString.Text80 = local.ConcatResult.Text80;

    Call(CoEabConcat10Delimit.Execute, useImport, useExport);

    local.ConcatResult.Text80 = useExport.ResultString.Text80;
  }

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseLeLactRaiseInfrastrEvents()
  {
    var useImport = new LeLactRaiseInfrastrEvents.Import();
    var useExport = new LeLactRaiseInfrastrEvents.Export();

    useImport.Event95ForNewLegActn.Flag = local.Event95.Flag;
    useImport.LegalAction.Identifier = export.HlegalAction.Identifier;

    Call(LeLactRaiseInfrastrEvents.Execute, useImport, useExport);
  }

  private void UseOeLgrqPrepareEvent()
  {
    var useImport = new OeLgrqPrepareEvent.Import();
    var useExport = new OeLgrqPrepareEvent.Export();

    useImport.LegalReferral.Assign(entities.LegalReferral);
    useImport.Case1.Number = entities.Case1.Number;
    useImport.ServiceProvider.Assign(entities.ExistingServiceProvider);
    MoveCsePersonsWorkSet(import.Ap, useImport.Ap);

    Call(OeLgrqPrepareEvent.Execute, useImport, useExport);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.NextTranInfo.Assign(import.HiddenNextTranInfo);
    useImport.Standard.NextTransaction = import.Standard.NextTransaction;

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

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiCreateAutoCsenetTrans()
  {
    var useImport = new SiCreateAutoCsenetTrans.Import();
    var useExport = new SiCreateAutoCsenetTrans.Export();

    useImport.ScreenIdentification.Command = local.ScreenIdentification.Command;
    useImport.Case1.Number = export.Hcase.Number;

    Call(SiCreateAutoCsenetTrans.Execute, useImport, useExport);
  }

  private void UseSpCabCreateAdminAppealAssgn()
  {
    var useImport = new SpCabCreateAdminAppealAssgn.Import();
    var useExport = new SpCabCreateAdminAppealAssgn.Export();

    useImport.ServiceProvider.SystemGeneratedId =
      export.Group.Item.ServiceProvider.SystemGeneratedId;
    useImport.Office.SystemGeneratedId =
      export.Group.Item.Office.SystemGeneratedId;
    MoveOfficeServiceProvider(export.Group.Item.OfficeServiceProvider,
      useImport.OfficeServiceProvider);
    useImport.AdministrativeAppeal.Identifier =
      export.HadministrativeAppeal.Identifier;
    useImport.AdministrativeAppealAssignment.Assign(
      local.AdministrativeAppealAssignment);

    Call(SpCabCreateAdminAppealAssgn.Execute, useImport, useExport);
  }

  private void UseSpCabCreateCuFunctionAssign()
  {
    var useImport = new SpCabCreateCuFunctionAssign.Import();
    var useExport = new SpCabCreateCuFunctionAssign.Export();

    useImport.Case1.Number = export.Hcase.Number;
    useImport.ServiceProvider.SystemGeneratedId =
      export.Group.Item.ServiceProvider.SystemGeneratedId;
    useImport.Office.SystemGeneratedId =
      export.Group.Item.Office.SystemGeneratedId;
    useImport.CaseUnit.CuNumber = export.HcaseUnit.CuNumber;
    MoveOfficeServiceProvider(export.Group.Item.OfficeServiceProvider,
      useImport.OfficeServiceProvider);
    MoveCaseUnitFunctionAssignmt1(export.Group.Item.CaseUnitFunctionAssignmt,
      useImport.CaseUnitFunctionAssignmt);

    Call(SpCabCreateCuFunctionAssign.Execute, useImport, useExport);
  }

  private void UseSpCabCreateIcaseAssignment()
  {
    var useImport = new SpCabCreateIcaseAssignment.Import();
    var useExport = new SpCabCreateIcaseAssignment.Export();

    MoveInterstateCase(local.Icase, useImport.InterstateCase);
    useImport.ServiceProvider.SystemGeneratedId =
      export.Group.Item.ServiceProvider.SystemGeneratedId;
    useImport.Office.SystemGeneratedId =
      export.Group.Item.Office.SystemGeneratedId;
    MoveOfficeServiceProvider(export.Group.Item.OfficeServiceProvider,
      useImport.OfficeServiceProvider);
    useImport.InterstateCaseAssignment.Assign(local.InterstateCaseAssignment);

    Call(SpCabCreateIcaseAssignment.Execute, useImport, useExport);

    local.InterstateCaseAssignment.Assign(useExport.InterstateCaseAssignment);
  }

  private void UseSpCabCreateLegalActionAssgn()
  {
    var useImport = new SpCabCreateLegalActionAssgn.Import();
    var useExport = new SpCabCreateLegalActionAssgn.Export();

    useImport.ServiceProvider.SystemGeneratedId =
      export.Group.Item.ServiceProvider.SystemGeneratedId;
    useImport.Office.SystemGeneratedId =
      export.Group.Item.Office.SystemGeneratedId;
    MoveOfficeServiceProvider(export.Group.Item.OfficeServiceProvider,
      useImport.OfficeServiceProvider);
    useImport.LegalAction.Identifier = export.HlegalAction.Identifier;
    useImport.LegalActionAssigment.Assign(local.LegalActionAssigment);

    Call(SpCabCreateLegalActionAssgn.Execute, useImport, useExport);
  }

  private void UseSpCabCreateLegalRefAssign()
  {
    var useImport = new SpCabCreateLegalRefAssign.Import();
    var useExport = new SpCabCreateLegalRefAssign.Export();

    useImport.Case1.Number = export.Hcase.Number;
    useImport.LegalReferral.Identifier = export.HlegalReferral.Identifier;
    useImport.LegalReferralAssignment.Assign(local.LegalReferralAssignment);
    useImport.ServiceProvider.SystemGeneratedId =
      export.Group.Item.ServiceProvider.SystemGeneratedId;
    useImport.Office.SystemGeneratedId =
      export.Group.Item.Office.SystemGeneratedId;
    MoveOfficeServiceProvider(export.Group.Item.OfficeServiceProvider,
      useImport.OfficeServiceProvider);

    Call(SpCabCreateLegalRefAssign.Execute, useImport, useExport);
  }

  private void UseSpCabCreateMonActAssignment1()
  {
    var useImport = new SpCabCreateMonActAssignment.Import();
    var useExport = new SpCabCreateMonActAssignment.Export();

    useImport.MonitoredActivity.Assign(entities.MonitoredActivity);
    useImport.ServiceProvider.SystemGeneratedId =
      entities.ExistingServiceProvider.SystemGeneratedId;
    MoveOfficeServiceProvider(entities.ExistingOfficeServiceProvider,
      useImport.OfficeServiceProvider);
    useImport.Office.SystemGeneratedId =
      entities.ExistingOffice.SystemGeneratedId;
    useImport.MonitoredActivityAssignment.Assign(
      local.MonitoredActivityAssignment);

    Call(SpCabCreateMonActAssignment.Execute, useImport, useExport);
  }

  private void UseSpCabCreateMonActAssignment2()
  {
    var useImport = new SpCabCreateMonActAssignment.Import();
    var useExport = new SpCabCreateMonActAssignment.Export();

    useImport.MonitoredActivityAssignment.Assign(
      local.MonitoredActivityAssignment);
    MoveMonitoredActivity(export.HmonitoredActivity, useImport.MonitoredActivity);
      
    useImport.ServiceProvider.SystemGeneratedId =
      export.Group.Item.ServiceProvider.SystemGeneratedId;
    useImport.Office.SystemGeneratedId =
      export.Group.Item.Office.SystemGeneratedId;
    MoveOfficeServiceProvider(export.Group.Item.OfficeServiceProvider,
      useImport.OfficeServiceProvider);

    Call(SpCabCreateMonActAssignment.Execute, useImport, useExport);

    local.MonitoredActivityAssignment.Assign(
      useExport.MonitoredActivityAssignment);
  }

  private void UseSpCabCreateOblgAssignment()
  {
    var useImport = new SpCabCreateOblgAssignment.Import();
    var useExport = new SpCabCreateOblgAssignment.Export();

    useImport.Obligation.SystemGeneratedIdentifier =
      entities.Obligation.SystemGeneratedIdentifier;
    useImport.ObligationAssignment.Assign(local.ObligationAssignment);
    useImport.CsePersonAccount.Type1 = export.HcsePersonAccount.Type1;
    useImport.CsePerson.Number = export.HcsePerson.Number;
    useImport.ServiceProvider.SystemGeneratedId =
      export.Group.Item.ServiceProvider.SystemGeneratedId;
    useImport.Office.SystemGeneratedId =
      export.Group.Item.Office.SystemGeneratedId;
    MoveOfficeServiceProvider(export.Group.Item.OfficeServiceProvider,
      useImport.OfficeServiceProvider);

    Call(SpCabCreateOblgAssignment.Execute, useImport, useExport);
  }

  private void UseSpCabCreatePaReferralAssign()
  {
    var useImport = new SpCabCreatePaReferralAssign.Import();
    var useExport = new SpCabCreatePaReferralAssign.Export();

    useImport.PaReferral.Assign(entities.PaReferral);
    useImport.PaReferralAssignment.Assign(local.PaReferralAssignment);
    useImport.ServiceProvider.SystemGeneratedId =
      export.Group.Item.ServiceProvider.SystemGeneratedId;
    useImport.Office.SystemGeneratedId =
      export.Group.Item.Office.SystemGeneratedId;
    MoveOfficeServiceProvider(export.Group.Item.OfficeServiceProvider,
      useImport.OfficeServiceProvider);

    Call(SpCabCreatePaReferralAssign.Execute, useImport, useExport);

    entities.PaReferral.Assign(useImport.PaReferral);
  }

  private void UseSpCabReassignCase()
  {
    var useImport = new SpCabReassignCase.Import();
    var useExport = new SpCabReassignCase.Export();

    MoveServiceProvider(export.Group.Item.ServiceProvider,
      useImport.ServiceProvider);
    useImport.Office.SystemGeneratedId =
      export.Group.Item.Office.SystemGeneratedId;
    MoveOfficeServiceProvider(export.Group.Item.OfficeServiceProvider,
      useImport.OfficeServiceProvider);
    useImport.Case1.Number = export.Hcase.Number;
    useImport.New1.Assign(local.CaseAssignment);
    MoveDateWorkArea(local.CurrentDate, useImport.Current);

    Call(SpCabReassignCase.Execute, useImport, useExport);
  }

  private void UseSpCabUpdateAdmAppAssignment()
  {
    var useImport = new SpCabUpdateAdmAppAssignment.Import();
    var useExport = new SpCabUpdateAdmAppAssignment.Export();

    useImport.AdministrativeAppealAssignment.Assign(
      local.AdministrativeAppealAssignment);
    useImport.AdministrativeAppeal.Identifier =
      export.HadministrativeAppeal.Identifier;
    useImport.Office.SystemGeneratedId =
      export.Group.Item.Office.SystemGeneratedId;
    useImport.ServiceProvider.SystemGeneratedId =
      export.Group.Item.ServiceProvider.SystemGeneratedId;
    MoveOfficeServiceProvider(export.Group.Item.OfficeServiceProvider,
      useImport.OfficeServiceProvider);

    Call(SpCabUpdateAdmAppAssignment.Execute, useImport, useExport);

    MoveAdministrativeAppealAssignment(useExport.AdministrativeAppealAssignment,
      local.AdministrativeAppealAssignment);
  }

  private void UseSpCabUpdateCaseAssgn()
  {
    var useImport = new SpCabUpdateCaseAssgn.Import();
    var useExport = new SpCabUpdateCaseAssgn.Export();

    useImport.Office.SystemGeneratedId =
      export.Group.Item.Office.SystemGeneratedId;
    useImport.ServiceProvider.SystemGeneratedId =
      export.Group.Item.ServiceProvider.SystemGeneratedId;
    MoveOfficeServiceProvider(export.Group.Item.OfficeServiceProvider,
      useImport.OfficeServiceProvider);
    useImport.Case1.Number = export.Hcase.Number;
    useImport.CaseAssignment.Assign(local.CaseAssignment);

    Call(SpCabUpdateCaseAssgn.Execute, useImport, useExport);
  }

  private void UseSpCabUpdateCuFuncAssgn()
  {
    var useImport = new SpCabUpdateCuFuncAssgn.Import();
    var useExport = new SpCabUpdateCuFuncAssgn.Export();

    useImport.CaseUnit.CuNumber = export.HcaseUnit.CuNumber;
    useImport.Case1.Number = export.Hcase.Number;
    useImport.Office.SystemGeneratedId =
      export.Group.Item.Office.SystemGeneratedId;
    useImport.ServiceProvider.SystemGeneratedId =
      export.Group.Item.ServiceProvider.SystemGeneratedId;
    MoveOfficeServiceProvider(export.Group.Item.OfficeServiceProvider,
      useImport.OfficeServiceProvider);
    MoveCaseUnitFunctionAssignmt1(export.Group.Item.CaseUnitFunctionAssignmt,
      useImport.CaseUnitFunctionAssignmt);

    Call(SpCabUpdateCuFuncAssgn.Execute, useImport, useExport);
  }

  private void UseSpCabUpdateIcaseAssignment()
  {
    var useImport = new SpCabUpdateIcaseAssignment.Import();
    var useExport = new SpCabUpdateIcaseAssignment.Export();

    useImport.InterstateCaseAssignment.Assign(local.InterstateCaseAssignment);
    MoveInterstateCase(local.Icase, useImport.InterstateCase);
    useImport.Office.SystemGeneratedId =
      export.Group.Item.Office.SystemGeneratedId;
    useImport.ServiceProvider.SystemGeneratedId =
      export.Group.Item.ServiceProvider.SystemGeneratedId;
    MoveOfficeServiceProvider(export.Group.Item.OfficeServiceProvider,
      useImport.OfficeServiceProvider);

    Call(SpCabUpdateIcaseAssignment.Execute, useImport, useExport);

    local.InterstateCaseAssignment.Assign(useExport.InterstateCaseAssignment);
  }

  private void UseSpCabUpdateLactAssignment()
  {
    var useImport = new SpCabUpdateLactAssignment.Import();
    var useExport = new SpCabUpdateLactAssignment.Export();

    useImport.LegalAction.Identifier = export.HlegalAction.Identifier;
    useImport.LegalActionAssigment.Assign(local.LegalActionAssigment);
    useImport.Office.SystemGeneratedId =
      export.Group.Item.Office.SystemGeneratedId;
    useImport.ServiceProvider.SystemGeneratedId =
      export.Group.Item.ServiceProvider.SystemGeneratedId;
    MoveOfficeServiceProvider(export.Group.Item.OfficeServiceProvider,
      useImport.OfficeServiceProvider);

    Call(SpCabUpdateLactAssignment.Execute, useImport, useExport);

    MoveLegalActionAssigment(useExport.LegalActionAssigment,
      local.LegalActionAssigment);
  }

  private void UseSpCabUpdateLegRefAssgmt()
  {
    var useImport = new SpCabUpdateLegRefAssgmt.Import();
    var useExport = new SpCabUpdateLegRefAssgmt.Export();

    useImport.Case1.Number = export.Hcase.Number;
    useImport.LegalReferral.Identifier = export.HlegalReferral.Identifier;
    MoveLegalReferralAssignment1(local.LegalReferralAssignment,
      useImport.LegalReferralAssignment);
    useImport.Office.SystemGeneratedId =
      export.Group.Item.Office.SystemGeneratedId;
    useImport.ServiceProvider.SystemGeneratedId =
      export.Group.Item.ServiceProvider.SystemGeneratedId;
    MoveOfficeServiceProvider(export.Group.Item.OfficeServiceProvider,
      useImport.OfficeServiceProvider);

    Call(SpCabUpdateLegRefAssgmt.Execute, useImport, useExport);

    MoveLegalReferralAssignment2(useExport.LegalReferralAssignment,
      local.LegalReferralAssignment);
  }

  private void UseSpCabUpdateMonaAssignment()
  {
    var useImport = new SpCabUpdateMonaAssignment.Import();
    var useExport = new SpCabUpdateMonaAssignment.Export();

    useImport.MonitoredActivityAssignment.Assign(
      local.MonitoredActivityAssignment);
    useImport.MonitoredActivity.SystemGeneratedIdentifier =
      export.HmonitoredActivity.SystemGeneratedIdentifier;
    useImport.Office.SystemGeneratedId =
      export.Group.Item.Office.SystemGeneratedId;
    useImport.ServiceProvider.SystemGeneratedId =
      export.Group.Item.ServiceProvider.SystemGeneratedId;
    MoveOfficeServiceProvider(export.Group.Item.OfficeServiceProvider,
      useImport.OfficeServiceProvider);

    Call(SpCabUpdateMonaAssignment.Execute, useImport, useExport);

    MoveMonitoredActivityAssignment(useExport.MonitoredActivityAssignment,
      local.MonitoredActivityAssignment);
  }

  private void UseSpCabUpdateObligAssignment()
  {
    var useImport = new SpCabUpdateObligAssignment.Import();
    var useExport = new SpCabUpdateObligAssignment.Export();

    useImport.Office.SystemGeneratedId =
      export.Group.Item.Office.SystemGeneratedId;
    useImport.ServiceProvider.SystemGeneratedId =
      export.Group.Item.ServiceProvider.SystemGeneratedId;
    MoveOfficeServiceProvider(export.Group.Item.OfficeServiceProvider,
      useImport.OfficeServiceProvider);
    useImport.ObligationType.SystemGeneratedIdentifier =
      export.HobligationType.SystemGeneratedIdentifier;
    useImport.CsePerson.Number = export.HcsePerson.Number;
    useImport.CsePersonAccount.Type1 = export.HcsePersonAccount.Type1;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Hobligation.SystemGeneratedIdentifier;
    useImport.ObligationAssignment.Assign(local.ObligationAssignment);

    Call(SpCabUpdateObligAssignment.Execute, useImport, useExport);

    local.ObligationAssignment.Assign(useExport.ObligationAssignment);
  }

  private void UseSpCabUpdatePaRefAssgmt()
  {
    var useImport = new SpCabUpdatePaRefAssgmt.Import();
    var useExport = new SpCabUpdatePaRefAssgmt.Export();

    useImport.PaReferralAssignment.Assign(local.PaReferralAssignment);
    useImport.PaReferral.Assign(export.HpaReferral);
    useImport.Office.SystemGeneratedId =
      export.Group.Item.Office.SystemGeneratedId;
    useImport.ServiceProvider.SystemGeneratedId =
      export.Group.Item.ServiceProvider.SystemGeneratedId;
    MoveOfficeServiceProvider(export.Group.Item.OfficeServiceProvider,
      useImport.OfficeServiceProvider);

    Call(SpCabUpdatePaRefAssgmt.Execute, useImport, useExport);

    MovePaReferralAssignment(useExport.PaReferralAssignment,
      local.PaReferralAssignment);
  }

  private void CreateMonitoredActivityAssignment1()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingOfficeServiceProvider.Populated);

    var systemGeneratedIdentifier = entities.Old.SystemGeneratedIdentifier;
    var reasonCode = entities.Old.ReasonCode;
    var responsibilityCode = entities.Old.ResponsibilityCode;
    var effectiveDate = local.LegalActionAssigment.EffectiveDate;
    var overrideInd = entities.Old.OverrideInd;
    var discontinueDate = local.MaxDate.Date;
    var createdBy = global.UserId;
    var createdTimestamp = local.CurrentDate.Timestamp;
    var spdId = entities.ExistingOfficeServiceProvider.SpdGeneratedId;
    var offId = entities.ExistingOfficeServiceProvider.OffGeneratedId;
    var ospCode = entities.ExistingOfficeServiceProvider.RoleCode;
    var ospDate = entities.ExistingOfficeServiceProvider.EffectiveDate;
    var macId = entities.MonitoredActivity.SystemGeneratedIdentifier;

    entities.New1.Populated = false;
    Update("CreateMonitoredActivityAssignment1",
      (db, command) =>
      {
        db.SetInt32(command, "systemGeneratedI", systemGeneratedIdentifier);
        db.SetString(command, "reasonCode", reasonCode);
        db.SetString(command, "responsibilityCod", responsibilityCode);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetString(command, "overrideInd", overrideInd);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", createdBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetInt32(command, "spdId", spdId);
        db.SetInt32(command, "offId", offId);
        db.SetString(command, "ospCode", ospCode);
        db.SetDate(command, "ospDate", ospDate);
        db.SetInt32(command, "macId", macId);
      });

    entities.New1.SystemGeneratedIdentifier = systemGeneratedIdentifier;
    entities.New1.ReasonCode = reasonCode;
    entities.New1.ResponsibilityCode = responsibilityCode;
    entities.New1.EffectiveDate = effectiveDate;
    entities.New1.OverrideInd = overrideInd;
    entities.New1.DiscontinueDate = discontinueDate;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CreatedTimestamp = createdTimestamp;
    entities.New1.LastUpdatedBy = createdBy;
    entities.New1.LastUpdatedTimestamp = createdTimestamp;
    entities.New1.SpdId = spdId;
    entities.New1.OffId = offId;
    entities.New1.OspCode = ospCode;
    entities.New1.OspDate = ospDate;
    entities.New1.MacId = macId;
    entities.New1.Populated = true;
  }

  private void CreateMonitoredActivityAssignment2()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingOfficeServiceProvider.Populated);

    var systemGeneratedIdentifier = entities.Old.SystemGeneratedIdentifier;
    var reasonCode = entities.Old.ReasonCode;
    var responsibilityCode = entities.Old.ResponsibilityCode;
    var effectiveDate = local.LegalReferralAssignment.EffectiveDate;
    var overrideInd = entities.Old.OverrideInd;
    var discontinueDate = local.MaxDate.Date;
    var createdBy = global.UserId;
    var createdTimestamp = local.CurrentDate.Timestamp;
    var spdId = entities.ExistingOfficeServiceProvider.SpdGeneratedId;
    var offId = entities.ExistingOfficeServiceProvider.OffGeneratedId;
    var ospCode = entities.ExistingOfficeServiceProvider.RoleCode;
    var ospDate = entities.ExistingOfficeServiceProvider.EffectiveDate;
    var macId = entities.MonitoredActivity.SystemGeneratedIdentifier;

    entities.New1.Populated = false;
    Update("CreateMonitoredActivityAssignment2",
      (db, command) =>
      {
        db.SetInt32(command, "systemGeneratedI", systemGeneratedIdentifier);
        db.SetString(command, "reasonCode", reasonCode);
        db.SetString(command, "responsibilityCod", responsibilityCode);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetString(command, "overrideInd", overrideInd);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", createdBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetInt32(command, "spdId", spdId);
        db.SetInt32(command, "offId", offId);
        db.SetString(command, "ospCode", ospCode);
        db.SetDate(command, "ospDate", ospDate);
        db.SetInt32(command, "macId", macId);
      });

    entities.New1.SystemGeneratedIdentifier = systemGeneratedIdentifier;
    entities.New1.ReasonCode = reasonCode;
    entities.New1.ResponsibilityCode = responsibilityCode;
    entities.New1.EffectiveDate = effectiveDate;
    entities.New1.OverrideInd = overrideInd;
    entities.New1.DiscontinueDate = discontinueDate;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CreatedTimestamp = createdTimestamp;
    entities.New1.LastUpdatedBy = createdBy;
    entities.New1.LastUpdatedTimestamp = createdTimestamp;
    entities.New1.SpdId = spdId;
    entities.New1.OffId = offId;
    entities.New1.OspCode = ospCode;
    entities.New1.OspDate = ospDate;
    entities.New1.MacId = macId;
    entities.New1.Populated = true;
  }

  private bool ReadAdministrativeAppeal()
  {
    entities.AdministrativeAppeal.Populated = false;

    return Read("ReadAdministrativeAppeal",
      (db, command) =>
      {
        db.SetInt32(
          command, "adminAppealId", export.HadministrativeAppeal.Identifier);
      },
      (db, reader) =>
      {
        entities.AdministrativeAppeal.Identifier = db.GetInt32(reader, 0);
        entities.AdministrativeAppeal.Number = db.GetNullableString(reader, 1);
        entities.AdministrativeAppeal.RequestDate = db.GetDate(reader, 2);
        entities.AdministrativeAppeal.CspQNumber =
          db.GetNullableString(reader, 3);
        entities.AdministrativeAppeal.Populated = true;
      });
  }

  private bool ReadAdministrativeAppealAssignment1()
  {
    entities.AdministrativeAppealAssignment.Populated = false;

    return Read("ReadAdministrativeAppealAssignment1",
      (db, command) =>
      {
        db.SetInt32(command, "aapId", entities.AdministrativeAppeal.Identifier);
        db.SetDate(
          command, "effectiveDate",
          export.Group.Item.CaseUnitFunctionAssignmt.DiscontinueDate.
            GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate",
          export.Group.Item.CaseUnitFunctionAssignmt.EffectiveDate.
            GetValueOrDefault());
        db.SetString(
          command, "reasonCode",
          export.Group.Item.CaseUnitFunctionAssignmt.ReasonCode);
        db.SetDateTime(
          command, "createdTimestamp",
          export.Group.Item.CaseUnitFunctionAssignmt.CreatedTimestamp.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.AdministrativeAppealAssignment.ReasonCode =
          db.GetString(reader, 0);
        entities.AdministrativeAppealAssignment.OverrideInd =
          db.GetString(reader, 1);
        entities.AdministrativeAppealAssignment.EffectiveDate =
          db.GetDate(reader, 2);
        entities.AdministrativeAppealAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.AdministrativeAppealAssignment.CreatedBy =
          db.GetString(reader, 4);
        entities.AdministrativeAppealAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.AdministrativeAppealAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.AdministrativeAppealAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 7);
        entities.AdministrativeAppealAssignment.SpdId = db.GetInt32(reader, 8);
        entities.AdministrativeAppealAssignment.OffId = db.GetInt32(reader, 9);
        entities.AdministrativeAppealAssignment.OspCode =
          db.GetString(reader, 10);
        entities.AdministrativeAppealAssignment.OspDate =
          db.GetDate(reader, 11);
        entities.AdministrativeAppealAssignment.AapId = db.GetInt32(reader, 12);
        entities.AdministrativeAppealAssignment.Populated = true;
      });
  }

  private IEnumerable<bool> ReadAdministrativeAppealAssignment2()
  {
    entities.AdministrativeAppealAssignment.Populated = false;

    return ReadEach("ReadAdministrativeAppealAssignment2",
      (db, command) =>
      {
        db.SetInt32(command, "aapId", export.HadministrativeAppeal.Identifier);
        db.SetDate(
          command, "effectiveDate",
          export.HeaderStart.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.AdministrativeAppealAssignment.ReasonCode =
          db.GetString(reader, 0);
        entities.AdministrativeAppealAssignment.OverrideInd =
          db.GetString(reader, 1);
        entities.AdministrativeAppealAssignment.EffectiveDate =
          db.GetDate(reader, 2);
        entities.AdministrativeAppealAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.AdministrativeAppealAssignment.CreatedBy =
          db.GetString(reader, 4);
        entities.AdministrativeAppealAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.AdministrativeAppealAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.AdministrativeAppealAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 7);
        entities.AdministrativeAppealAssignment.SpdId = db.GetInt32(reader, 8);
        entities.AdministrativeAppealAssignment.OffId = db.GetInt32(reader, 9);
        entities.AdministrativeAppealAssignment.OspCode =
          db.GetString(reader, 10);
        entities.AdministrativeAppealAssignment.OspDate =
          db.GetDate(reader, 11);
        entities.AdministrativeAppealAssignment.AapId = db.GetInt32(reader, 12);
        entities.AdministrativeAppealAssignment.Populated = true;

        return true;
      });
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", export.Hcase.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCaseAssignment()
  {
    entities.CaseAssignment.Populated = false;

    return ReadEach("ReadCaseAssignment",
      (db, command) =>
      {
        db.SetString(command, "casNo", import.Hcase.Number);
        db.SetDate(
          command, "effectiveDate",
          export.HeaderStart.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseAssignment.ReasonCode = db.GetString(reader, 0);
        entities.CaseAssignment.OverrideInd = db.GetString(reader, 1);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 2);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 3);
        entities.CaseAssignment.CreatedBy = db.GetString(reader, 4);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.CaseAssignment.LastUpdatedBy = db.GetNullableString(reader, 6);
        entities.CaseAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 7);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 8);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 9);
        entities.CaseAssignment.OspCode = db.GetString(reader, 10);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 11);
        entities.CaseAssignment.CasNo = db.GetString(reader, 12);
        entities.CaseAssignment.Populated = true;

        return true;
      });
  }

  private bool ReadCaseCaseUnit()
  {
    entities.CaseUnit.Populated = false;
    entities.Case1.Populated = false;

    return Read("ReadCaseCaseUnit",
      (db, command) =>
      {
        db.SetString(command, "casNo", export.Hcase.Number);
        db.SetInt32(command, "cuNumber", export.HcaseUnit.CuNumber);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseUnit.CasNo = db.GetString(reader, 0);
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 1);
        entities.CaseUnit.Populated = true;
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseUnitFunctionAssignmt1()
  {
    System.Diagnostics.Debug.Assert(entities.CaseUnit.Populated);
    entities.CaseUnitFunctionAssignmt.Populated = false;

    return Read("ReadCaseUnitFunctionAssignmt1",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          export.Group.Item.CaseUnitFunctionAssignmt.DiscontinueDate.
            GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate",
          export.Group.Item.CaseUnitFunctionAssignmt.EffectiveDate.
            GetValueOrDefault());
        db.SetString(
          command, "function",
          export.Group.Item.CaseUnitFunctionAssignmt.Function);
        db.SetString(
          command, "reasonCode",
          export.Group.Item.CaseUnitFunctionAssignmt.ReasonCode);
        db.SetDateTime(
          command, "createdTimestamp",
          export.Group.Item.CaseUnitFunctionAssignmt.CreatedTimestamp.
            GetValueOrDefault());
        db.SetString(command, "casNo", entities.CaseUnit.CasNo);
        db.SetInt32(command, "csuNo", entities.CaseUnit.CuNumber);
      },
      (db, reader) =>
      {
        entities.CaseUnitFunctionAssignmt.ReasonCode = db.GetString(reader, 0);
        entities.CaseUnitFunctionAssignmt.OverrideInd = db.GetString(reader, 1);
        entities.CaseUnitFunctionAssignmt.EffectiveDate = db.GetDate(reader, 2);
        entities.CaseUnitFunctionAssignmt.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.CaseUnitFunctionAssignmt.CreatedBy = db.GetString(reader, 4);
        entities.CaseUnitFunctionAssignmt.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.CaseUnitFunctionAssignmt.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.CaseUnitFunctionAssignmt.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 7);
        entities.CaseUnitFunctionAssignmt.SpdId = db.GetInt32(reader, 8);
        entities.CaseUnitFunctionAssignmt.OffId = db.GetInt32(reader, 9);
        entities.CaseUnitFunctionAssignmt.OspCode = db.GetString(reader, 10);
        entities.CaseUnitFunctionAssignmt.OspDate = db.GetDate(reader, 11);
        entities.CaseUnitFunctionAssignmt.CsuNo = db.GetInt32(reader, 12);
        entities.CaseUnitFunctionAssignmt.CasNo = db.GetString(reader, 13);
        entities.CaseUnitFunctionAssignmt.Function = db.GetString(reader, 14);
        entities.CaseUnitFunctionAssignmt.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCaseUnitFunctionAssignmt2()
  {
    entities.CaseUnitFunctionAssignmt.Populated = false;

    return ReadEach("ReadCaseUnitFunctionAssignmt2",
      (db, command) =>
      {
        db.SetInt32(command, "csuNo", export.HcaseUnit.CuNumber);
        db.SetString(command, "casNo", export.Hcase.Number);
        db.SetDate(
          command, "effectiveDate",
          export.HeaderStart.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseUnitFunctionAssignmt.ReasonCode = db.GetString(reader, 0);
        entities.CaseUnitFunctionAssignmt.OverrideInd = db.GetString(reader, 1);
        entities.CaseUnitFunctionAssignmt.EffectiveDate = db.GetDate(reader, 2);
        entities.CaseUnitFunctionAssignmt.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.CaseUnitFunctionAssignmt.CreatedBy = db.GetString(reader, 4);
        entities.CaseUnitFunctionAssignmt.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.CaseUnitFunctionAssignmt.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.CaseUnitFunctionAssignmt.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 7);
        entities.CaseUnitFunctionAssignmt.SpdId = db.GetInt32(reader, 8);
        entities.CaseUnitFunctionAssignmt.OffId = db.GetInt32(reader, 9);
        entities.CaseUnitFunctionAssignmt.OspCode = db.GetString(reader, 10);
        entities.CaseUnitFunctionAssignmt.OspDate = db.GetDate(reader, 11);
        entities.CaseUnitFunctionAssignmt.CsuNo = db.GetInt32(reader, 12);
        entities.CaseUnitFunctionAssignmt.CasNo = db.GetString(reader, 13);
        entities.CaseUnitFunctionAssignmt.Function = db.GetString(reader, 14);
        entities.CaseUnitFunctionAssignmt.Populated = true;

        return true;
      });
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.AdministrativeAppeal.Populated);
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(
          command, "numb", entities.AdministrativeAppeal.CspQNumber ?? "");
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
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
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 4);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadFipsTribAddress()
  {
    entities.Foreign.Populated = false;

    return Read("ReadFipsTribAddress",
      (db, command) =>
      {
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.Foreign.Identifier = db.GetInt32(reader, 0);
        entities.Foreign.Country = db.GetNullableString(reader, 1);
        entities.Foreign.TrbId = db.GetNullableInt32(reader, 2);
        entities.Foreign.Populated = true;
      });
  }

  private bool ReadInfrastructure()
  {
    System.Diagnostics.Debug.Assert(entities.MonitoredActivity.Populated);
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          entities.MonitoredActivity.InfSysGenId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.EventId = db.GetInt32(reader, 1);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 2);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 3);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 4);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 5);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 6);
        entities.Infrastructure.CaseUnitNumber = db.GetNullableInt32(reader, 7);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 8);
        entities.Infrastructure.Function = db.GetNullableString(reader, 9);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 10);
        entities.Infrastructure.Populated = true;
      });
  }

  private IEnumerable<bool> ReadInfrastructureMonitoredActivityAssignment()
  {
    System.Diagnostics.Debug.Assert(
      entities.AkeyOnlyOfficeServiceProvider.Populated);
    entities.Infrastructure.Populated = false;
    entities.Old.Populated = false;

    return ReadEach("ReadInfrastructureMonitoredActivityAssignment",
      (db, command) =>
      {
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Other.DenormNumeric12.GetValueOrDefault());
        db.SetString(command, "businessObjectCd", local.Lea.BusinessObjectCd);
        db.SetNullableString(
          command, "denormText12", local.Saved.DenormText12 ?? "");
        db.SetNullableDate(
          command, "referenceDate",
          export.Compare.FiledDate.GetValueOrDefault());
        db.SetNullableString(command, "detail", local.Compare.Detail ?? "");
        db.SetDate(
          command, "effectiveDate", local.CurrentDate.Date.GetValueOrDefault());
          
        db.SetDate(
          command, "ospDate",
          entities.AkeyOnlyOfficeServiceProvider.EffectiveDate.
            GetValueOrDefault());
        db.SetString(
          command, "ospCode", entities.AkeyOnlyOfficeServiceProvider.RoleCode);
        db.SetInt32(
          command, "offId",
          entities.AkeyOnlyOfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdId",
          entities.AkeyOnlyOfficeServiceProvider.SpdGeneratedId);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.EventId = db.GetInt32(reader, 1);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 2);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 3);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 4);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 5);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 6);
        entities.Infrastructure.CaseUnitNumber = db.GetNullableInt32(reader, 7);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 8);
        entities.Infrastructure.Function = db.GetNullableString(reader, 9);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 10);
        entities.Old.SystemGeneratedIdentifier = db.GetInt32(reader, 11);
        entities.Old.ReasonCode = db.GetString(reader, 12);
        entities.Old.ResponsibilityCode = db.GetString(reader, 13);
        entities.Old.EffectiveDate = db.GetDate(reader, 14);
        entities.Old.OverrideInd = db.GetString(reader, 15);
        entities.Old.DiscontinueDate = db.GetNullableDate(reader, 16);
        entities.Old.CreatedBy = db.GetString(reader, 17);
        entities.Old.CreatedTimestamp = db.GetDateTime(reader, 18);
        entities.Old.LastUpdatedBy = db.GetNullableString(reader, 19);
        entities.Old.LastUpdatedTimestamp = db.GetNullableDateTime(reader, 20);
        entities.Old.SpdId = db.GetInt32(reader, 21);
        entities.Old.OffId = db.GetInt32(reader, 22);
        entities.Old.OspCode = db.GetString(reader, 23);
        entities.Old.OspDate = db.GetDate(reader, 24);
        entities.Old.MacId = db.GetInt32(reader, 25);
        entities.Infrastructure.Populated = true;
        entities.Old.Populated = true;

        return true;
      });
  }

  private bool ReadInterstateCase()
  {
    entities.InterstateCase.Populated = false;

    return Read("ReadInterstateCase",
      (db, command) =>
      {
        db.SetDate(
          command, "transactionDate",
          local.Icase.TransactionDate.GetValueOrDefault());
        db.SetInt64(command, "transSerialNbr", local.Icase.TransSerialNumber);
      },
      (db, reader) =>
      {
        entities.InterstateCase.TransSerialNumber = db.GetInt64(reader, 0);
        entities.InterstateCase.TransactionDate = db.GetDate(reader, 1);
        entities.InterstateCase.Populated = true;
      });
  }

  private bool ReadInterstateCaseAssignment1()
  {
    entities.InterstateCaseAssignment.Populated = false;

    return Read("ReadInterstateCaseAssignment1",
      (db, command) =>
      {
        db.
          SetInt64(command, "icsNo", entities.InterstateCase.TransSerialNumber);
          
        db.SetDate(
          command, "icsDate",
          entities.InterstateCase.TransactionDate.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate",
          export.Group.Item.CaseUnitFunctionAssignmt.DiscontinueDate.
            GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate",
          export.Group.Item.CaseUnitFunctionAssignmt.EffectiveDate.
            GetValueOrDefault());
        db.SetString(
          command, "reasonCode",
          export.Group.Item.CaseUnitFunctionAssignmt.ReasonCode);
        db.SetDateTime(
          command, "createdTimestamp",
          export.Group.Item.CaseUnitFunctionAssignmt.CreatedTimestamp.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateCaseAssignment.ReasonCode = db.GetString(reader, 0);
        entities.InterstateCaseAssignment.OverrideInd = db.GetString(reader, 1);
        entities.InterstateCaseAssignment.EffectiveDate = db.GetDate(reader, 2);
        entities.InterstateCaseAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.InterstateCaseAssignment.CreatedBy = db.GetString(reader, 4);
        entities.InterstateCaseAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.InterstateCaseAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.InterstateCaseAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 7);
        entities.InterstateCaseAssignment.SpdId = db.GetInt32(reader, 8);
        entities.InterstateCaseAssignment.OffId = db.GetInt32(reader, 9);
        entities.InterstateCaseAssignment.OspCode = db.GetString(reader, 10);
        entities.InterstateCaseAssignment.OspDate = db.GetDate(reader, 11);
        entities.InterstateCaseAssignment.IcsDate = db.GetDate(reader, 12);
        entities.InterstateCaseAssignment.IcsNo = db.GetInt64(reader, 13);
        entities.InterstateCaseAssignment.Populated = true;
      });
  }

  private IEnumerable<bool> ReadInterstateCaseAssignment2()
  {
    entities.InterstateCaseAssignment.Populated = false;

    return ReadEach("ReadInterstateCaseAssignment2",
      (db, command) =>
      {
        db.
          SetInt64(command, "icsNo", entities.InterstateCase.TransSerialNumber);
          
        db.SetDate(
          command, "icsDate",
          entities.InterstateCase.TransactionDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateCaseAssignment.ReasonCode = db.GetString(reader, 0);
        entities.InterstateCaseAssignment.OverrideInd = db.GetString(reader, 1);
        entities.InterstateCaseAssignment.EffectiveDate = db.GetDate(reader, 2);
        entities.InterstateCaseAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.InterstateCaseAssignment.CreatedBy = db.GetString(reader, 4);
        entities.InterstateCaseAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.InterstateCaseAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.InterstateCaseAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 7);
        entities.InterstateCaseAssignment.SpdId = db.GetInt32(reader, 8);
        entities.InterstateCaseAssignment.OffId = db.GetInt32(reader, 9);
        entities.InterstateCaseAssignment.OspCode = db.GetString(reader, 10);
        entities.InterstateCaseAssignment.OspDate = db.GetDate(reader, 11);
        entities.InterstateCaseAssignment.IcsDate = db.GetDate(reader, 12);
        entities.InterstateCaseAssignment.IcsNo = db.GetInt64(reader, 13);
        entities.InterstateCaseAssignment.Populated = true;

        return true;
      });
  }

  private bool ReadLegalAction1()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction1",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", export.HlegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 5);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 6);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 7);
        entities.LegalAction.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalAction2()
  {
    System.Diagnostics.Debug.Assert(
      entities.AkeyOnlyOfficeServiceProvider.Populated);
    entities.Other.Populated = false;

    return ReadEach("ReadLegalAction2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", entities.LegalAction.StandardNumber ?? "");
        db.SetNullableDate(
          command, "endDt",
          local.LegalActionAssigment.DiscontinueDate.GetValueOrDefault());
        db.SetNullableString(
          command, "ospRoleCode",
          entities.AkeyOnlyOfficeServiceProvider.RoleCode);
        db.SetNullableDate(
          command, "ospEffectiveDate",
          entities.AkeyOnlyOfficeServiceProvider.EffectiveDate.
            GetValueOrDefault());
        db.SetNullableInt32(
          command, "offGeneratedId",
          entities.AkeyOnlyOfficeServiceProvider.OffGeneratedId);
        db.SetNullableInt32(
          command, "spdGeneratedId",
          entities.AkeyOnlyOfficeServiceProvider.SpdGeneratedId);
      },
      (db, reader) =>
      {
        entities.Other.Identifier = db.GetInt32(reader, 0);
        entities.Other.StandardNumber = db.GetNullableString(reader, 1);
        entities.Other.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalAction3()
  {
    entities.Other.Populated = false;

    return ReadEach("ReadLegalAction3",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", entities.LegalAction.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Other.Identifier = db.GetInt32(reader, 0);
        entities.Other.StandardNumber = db.GetNullableString(reader, 1);
        entities.Other.Populated = true;

        return true;
      });
  }

  private bool ReadLegalActionAssigment1()
  {
    entities.LegalActionAssigment.Populated = false;

    return Read("ReadLegalActionAssigment1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.LegalAction.Identifier);
        db.SetDate(
          command, "effectiveDt",
          export.Group.Item.CaseUnitFunctionAssignmt.DiscontinueDate.
            GetValueOrDefault());
        db.SetNullableDate(
          command, "endDt",
          export.Group.Item.CaseUnitFunctionAssignmt.EffectiveDate.
            GetValueOrDefault());
        db.SetString(
          command, "reasonCode",
          export.Group.Item.CaseUnitFunctionAssignmt.ReasonCode);
        db.SetDateTime(
          command, "createdTimestamp",
          export.Group.Item.CaseUnitFunctionAssignmt.CreatedTimestamp.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalActionAssigment.LgaIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.LegalActionAssigment.OspEffectiveDate =
          db.GetNullableDate(reader, 1);
        entities.LegalActionAssigment.OspRoleCode =
          db.GetNullableString(reader, 2);
        entities.LegalActionAssigment.OffGeneratedId =
          db.GetNullableInt32(reader, 3);
        entities.LegalActionAssigment.SpdGeneratedId =
          db.GetNullableInt32(reader, 4);
        entities.LegalActionAssigment.EffectiveDate = db.GetDate(reader, 5);
        entities.LegalActionAssigment.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.LegalActionAssigment.ReasonCode = db.GetString(reader, 7);
        entities.LegalActionAssigment.OverrideInd = db.GetString(reader, 8);
        entities.LegalActionAssigment.CreatedBy = db.GetString(reader, 9);
        entities.LegalActionAssigment.CreatedTimestamp =
          db.GetDateTime(reader, 10);
        entities.LegalActionAssigment.LastUpdatedBy =
          db.GetNullableString(reader, 11);
        entities.LegalActionAssigment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 12);
        entities.LegalActionAssigment.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalActionAssigment2()
  {
    entities.LegalActionAssigment.Populated = false;

    return ReadEach("ReadLegalActionAssigment2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", export.HlegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalActionAssigment.LgaIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.LegalActionAssigment.OspEffectiveDate =
          db.GetNullableDate(reader, 1);
        entities.LegalActionAssigment.OspRoleCode =
          db.GetNullableString(reader, 2);
        entities.LegalActionAssigment.OffGeneratedId =
          db.GetNullableInt32(reader, 3);
        entities.LegalActionAssigment.SpdGeneratedId =
          db.GetNullableInt32(reader, 4);
        entities.LegalActionAssigment.EffectiveDate = db.GetDate(reader, 5);
        entities.LegalActionAssigment.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.LegalActionAssigment.ReasonCode = db.GetString(reader, 7);
        entities.LegalActionAssigment.OverrideInd = db.GetString(reader, 8);
        entities.LegalActionAssigment.CreatedBy = db.GetString(reader, 9);
        entities.LegalActionAssigment.CreatedTimestamp =
          db.GetDateTime(reader, 10);
        entities.LegalActionAssigment.LastUpdatedBy =
          db.GetNullableString(reader, 11);
        entities.LegalActionAssigment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 12);
        entities.LegalActionAssigment.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionAssigment3()
  {
    entities.LegalActionAssigment.Populated = false;

    return ReadEach("ReadLegalActionAssigment3",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", export.HlegalAction.Identifier);
        db.SetDate(
          command, "effectiveDt", export.HeaderStart.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalActionAssigment.LgaIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.LegalActionAssigment.OspEffectiveDate =
          db.GetNullableDate(reader, 1);
        entities.LegalActionAssigment.OspRoleCode =
          db.GetNullableString(reader, 2);
        entities.LegalActionAssigment.OffGeneratedId =
          db.GetNullableInt32(reader, 3);
        entities.LegalActionAssigment.SpdGeneratedId =
          db.GetNullableInt32(reader, 4);
        entities.LegalActionAssigment.EffectiveDate = db.GetDate(reader, 5);
        entities.LegalActionAssigment.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.LegalActionAssigment.ReasonCode = db.GetString(reader, 7);
        entities.LegalActionAssigment.OverrideInd = db.GetString(reader, 8);
        entities.LegalActionAssigment.CreatedBy = db.GetString(reader, 9);
        entities.LegalActionAssigment.CreatedTimestamp =
          db.GetDateTime(reader, 10);
        entities.LegalActionAssigment.LastUpdatedBy =
          db.GetNullableString(reader, 11);
        entities.LegalActionAssigment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 12);
        entities.LegalActionAssigment.Populated = true;

        return true;
      });
  }

  private bool ReadLegalReferral()
  {
    entities.LegalReferral.Populated = false;

    return Read("ReadLegalReferral",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", export.HlegalReferral.Identifier);
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.LegalReferral.CasNumber = db.GetString(reader, 0);
        entities.LegalReferral.Identifier = db.GetInt32(reader, 1);
        entities.LegalReferral.ReferredByUserId = db.GetString(reader, 2);
        entities.LegalReferral.StatusDate = db.GetNullableDate(reader, 3);
        entities.LegalReferral.Status = db.GetNullableString(reader, 4);
        entities.LegalReferral.ReferralDate = db.GetDate(reader, 5);
        entities.LegalReferral.ReferralReason1 = db.GetString(reader, 6);
        entities.LegalReferral.ReferralReason2 = db.GetString(reader, 7);
        entities.LegalReferral.ReferralReason3 = db.GetString(reader, 8);
        entities.LegalReferral.ReferralReason4 = db.GetString(reader, 9);
        entities.LegalReferral.ReferralReason5 = db.GetString(reader, 10);
        entities.LegalReferral.Populated = true;
      });
  }

  private bool ReadLegalReferralAssignment1()
  {
    entities.LegalReferralAssignment.Populated = false;

    return Read("ReadLegalReferralAssignment1",
      (db, command) =>
      {
        db.SetInt32(command, "lgrId", export.HlegalReferral.Identifier);
        db.SetString(command, "casNo", export.Hcase.Number);
        db.SetDate(
          command, "effectiveDate",
          local.SelectDateWorkArea.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate",
          local.CurrentDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalReferralAssignment.ReasonCode = db.GetString(reader, 0);
        entities.LegalReferralAssignment.OverrideInd = db.GetString(reader, 1);
        entities.LegalReferralAssignment.EffectiveDate = db.GetDate(reader, 2);
        entities.LegalReferralAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.LegalReferralAssignment.CreatedBy = db.GetString(reader, 4);
        entities.LegalReferralAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.LegalReferralAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.LegalReferralAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 7);
        entities.LegalReferralAssignment.SpdId = db.GetInt32(reader, 8);
        entities.LegalReferralAssignment.OffId = db.GetInt32(reader, 9);
        entities.LegalReferralAssignment.OspCode = db.GetString(reader, 10);
        entities.LegalReferralAssignment.OspDate = db.GetDate(reader, 11);
        entities.LegalReferralAssignment.CasNo = db.GetString(reader, 12);
        entities.LegalReferralAssignment.LgrId = db.GetInt32(reader, 13);
        entities.LegalReferralAssignment.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalReferralAssignment2()
  {
    entities.LegalReferralAssignment.Populated = false;

    return ReadEach("ReadLegalReferralAssignment2",
      (db, command) =>
      {
        db.SetInt32(command, "lgrId", export.HlegalReferral.Identifier);
        db.SetString(command, "casNo", export.Hcase.Number);
        db.SetDate(
          command, "effectiveDate",
          export.HeaderStart.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalReferralAssignment.ReasonCode = db.GetString(reader, 0);
        entities.LegalReferralAssignment.OverrideInd = db.GetString(reader, 1);
        entities.LegalReferralAssignment.EffectiveDate = db.GetDate(reader, 2);
        entities.LegalReferralAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.LegalReferralAssignment.CreatedBy = db.GetString(reader, 4);
        entities.LegalReferralAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.LegalReferralAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.LegalReferralAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 7);
        entities.LegalReferralAssignment.SpdId = db.GetInt32(reader, 8);
        entities.LegalReferralAssignment.OffId = db.GetInt32(reader, 9);
        entities.LegalReferralAssignment.OspCode = db.GetString(reader, 10);
        entities.LegalReferralAssignment.OspDate = db.GetDate(reader, 11);
        entities.LegalReferralAssignment.CasNo = db.GetString(reader, 12);
        entities.LegalReferralAssignment.LgrId = db.GetInt32(reader, 13);
        entities.LegalReferralAssignment.Populated = true;

        return true;
      });
  }

  private bool ReadMonitoredActivity1()
  {
    entities.MonitoredActivity.Populated = false;

    return Read("ReadMonitoredActivity1",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          export.HmonitoredActivity.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.MonitoredActivity.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.MonitoredActivity.Name = db.GetString(reader, 1);
        entities.MonitoredActivity.TypeCode = db.GetNullableString(reader, 2);
        entities.MonitoredActivity.StartDate = db.GetDate(reader, 3);
        entities.MonitoredActivity.ClosureDate = db.GetNullableDate(reader, 4);
        entities.MonitoredActivity.ClosureReasonCode =
          db.GetNullableString(reader, 5);
        entities.MonitoredActivity.InfSysGenId = db.GetNullableInt32(reader, 6);
        entities.MonitoredActivity.Populated = true;
      });
  }

  private bool ReadMonitoredActivity2()
  {
    entities.MonitoredActivity.Populated = false;

    return Read("ReadMonitoredActivity2",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          export.HmonitoredActivity.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.MonitoredActivity.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.MonitoredActivity.Name = db.GetString(reader, 1);
        entities.MonitoredActivity.TypeCode = db.GetNullableString(reader, 2);
        entities.MonitoredActivity.StartDate = db.GetDate(reader, 3);
        entities.MonitoredActivity.ClosureDate = db.GetNullableDate(reader, 4);
        entities.MonitoredActivity.ClosureReasonCode =
          db.GetNullableString(reader, 5);
        entities.MonitoredActivity.InfSysGenId = db.GetNullableInt32(reader, 6);
        entities.MonitoredActivity.Populated = true;
      });
  }

  private bool ReadMonitoredActivityAssignment1()
  {
    entities.Old.Populated = false;

    return Read("ReadMonitoredActivityAssignment1",
      (db, command) =>
      {
        db.SetInt32(
          command, "macId",
          entities.MonitoredActivity.SystemGeneratedIdentifier);
        db.SetDate(
          command, "effectiveDate",
          export.Group.Item.CaseUnitFunctionAssignmt.DiscontinueDate.
            GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate",
          export.Group.Item.CaseUnitFunctionAssignmt.EffectiveDate.
            GetValueOrDefault());
        db.SetString(
          command, "reasonCode",
          export.Group.Item.CaseUnitFunctionAssignmt.ReasonCode);
        db.SetDateTime(
          command, "createdTimestamp",
          export.Group.Item.CaseUnitFunctionAssignmt.CreatedTimestamp.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Old.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Old.ReasonCode = db.GetString(reader, 1);
        entities.Old.ResponsibilityCode = db.GetString(reader, 2);
        entities.Old.EffectiveDate = db.GetDate(reader, 3);
        entities.Old.OverrideInd = db.GetString(reader, 4);
        entities.Old.DiscontinueDate = db.GetNullableDate(reader, 5);
        entities.Old.CreatedBy = db.GetString(reader, 6);
        entities.Old.CreatedTimestamp = db.GetDateTime(reader, 7);
        entities.Old.LastUpdatedBy = db.GetNullableString(reader, 8);
        entities.Old.LastUpdatedTimestamp = db.GetNullableDateTime(reader, 9);
        entities.Old.SpdId = db.GetInt32(reader, 10);
        entities.Old.OffId = db.GetInt32(reader, 11);
        entities.Old.OspCode = db.GetString(reader, 12);
        entities.Old.OspDate = db.GetDate(reader, 13);
        entities.Old.MacId = db.GetInt32(reader, 14);
        entities.Old.Populated = true;
      });
  }

  private IEnumerable<bool> ReadMonitoredActivityAssignment2()
  {
    entities.Old.Populated = false;

    return ReadEach("ReadMonitoredActivityAssignment2",
      (db, command) =>
      {
        db.SetInt32(
          command, "macId",
          export.HmonitoredActivity.SystemGeneratedIdentifier);
        db.SetDate(
          command, "effectiveDate",
          export.HeaderStart.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Old.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Old.ReasonCode = db.GetString(reader, 1);
        entities.Old.ResponsibilityCode = db.GetString(reader, 2);
        entities.Old.EffectiveDate = db.GetDate(reader, 3);
        entities.Old.OverrideInd = db.GetString(reader, 4);
        entities.Old.DiscontinueDate = db.GetNullableDate(reader, 5);
        entities.Old.CreatedBy = db.GetString(reader, 6);
        entities.Old.CreatedTimestamp = db.GetDateTime(reader, 7);
        entities.Old.LastUpdatedBy = db.GetNullableString(reader, 8);
        entities.Old.LastUpdatedTimestamp = db.GetNullableDateTime(reader, 9);
        entities.Old.SpdId = db.GetInt32(reader, 10);
        entities.Old.OffId = db.GetInt32(reader, 11);
        entities.Old.OspCode = db.GetString(reader, 12);
        entities.Old.OspDate = db.GetDate(reader, 13);
        entities.Old.MacId = db.GetInt32(reader, 14);
        entities.Old.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadMonitoredActivityAssignmentMonitoredActivity1()
  {
    System.Diagnostics.Debug.Assert(
      entities.AkeyOnlyOfficeServiceProvider.Populated);
    entities.MonitoredActivity.Populated = false;
    entities.Old.Populated = false;

    return ReadEach("ReadMonitoredActivityAssignmentMonitoredActivity1",
      (db, command) =>
      {
        db.SetDate(
          command, "ospDate",
          entities.AkeyOnlyOfficeServiceProvider.EffectiveDate.
            GetValueOrDefault());
        db.SetString(
          command, "ospCode", entities.AkeyOnlyOfficeServiceProvider.RoleCode);
        db.SetInt32(
          command, "offId",
          entities.AkeyOnlyOfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdId",
          entities.AkeyOnlyOfficeServiceProvider.SpdGeneratedId);
        db.SetNullableDate(
          command, "discontinueDate",
          export.OldLegalActionAssigment.DiscontinueDate.GetValueOrDefault());
        db.SetNullableString(command, "lastUpdatedBy", global.UserId);
        db.SetNullableString(
          command, "denormText12", local.Saved.DenormText12 ?? "");
        db.SetString(command, "businessObjectCd", local.Lea.BusinessObjectCd);
        db.SetNullableDate(
          command, "referenceDate",
          export.Compare.FiledDate.GetValueOrDefault());
        db.SetNullableString(command, "detail", local.Compare.Detail ?? "");
      },
      (db, reader) =>
      {
        entities.Old.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Old.ReasonCode = db.GetString(reader, 1);
        entities.Old.ResponsibilityCode = db.GetString(reader, 2);
        entities.Old.EffectiveDate = db.GetDate(reader, 3);
        entities.Old.OverrideInd = db.GetString(reader, 4);
        entities.Old.DiscontinueDate = db.GetNullableDate(reader, 5);
        entities.Old.CreatedBy = db.GetString(reader, 6);
        entities.Old.CreatedTimestamp = db.GetDateTime(reader, 7);
        entities.Old.LastUpdatedBy = db.GetNullableString(reader, 8);
        entities.Old.LastUpdatedTimestamp = db.GetNullableDateTime(reader, 9);
        entities.Old.SpdId = db.GetInt32(reader, 10);
        entities.Old.OffId = db.GetInt32(reader, 11);
        entities.Old.OspCode = db.GetString(reader, 12);
        entities.Old.OspDate = db.GetDate(reader, 13);
        entities.Old.MacId = db.GetInt32(reader, 14);
        entities.MonitoredActivity.SystemGeneratedIdentifier =
          db.GetInt32(reader, 14);
        entities.MonitoredActivity.Name = db.GetString(reader, 15);
        entities.MonitoredActivity.TypeCode = db.GetNullableString(reader, 16);
        entities.MonitoredActivity.StartDate = db.GetDate(reader, 17);
        entities.MonitoredActivity.ClosureDate = db.GetNullableDate(reader, 18);
        entities.MonitoredActivity.ClosureReasonCode =
          db.GetNullableString(reader, 19);
        entities.MonitoredActivity.InfSysGenId =
          db.GetNullableInt32(reader, 20);
        entities.MonitoredActivity.Populated = true;
        entities.Old.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadMonitoredActivityAssignmentMonitoredActivity2()
  {
    System.Diagnostics.Debug.Assert(
      entities.AkeyOnlyOfficeServiceProvider.Populated);
    entities.MonitoredActivity.Populated = false;
    entities.Old.Populated = false;

    return ReadEach("ReadMonitoredActivityAssignmentMonitoredActivity2",
      (db, command) =>
      {
        db.SetDate(
          command, "ospDate",
          entities.AkeyOnlyOfficeServiceProvider.EffectiveDate.
            GetValueOrDefault());
        db.SetString(
          command, "ospCode", entities.AkeyOnlyOfficeServiceProvider.RoleCode);
        db.SetInt32(
          command, "offId",
          entities.AkeyOnlyOfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdId",
          entities.AkeyOnlyOfficeServiceProvider.SpdGeneratedId);
        db.SetNullableDate(
          command, "discontinueDate",
          export.OldLegalReferralAssignment.DiscontinueDate.
            GetValueOrDefault());
        db.SetNullableString(command, "lastUpdatedBy", global.UserId);
        db.SetNullableString(command, "caseNumber", entities.Case1.Number);
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.SelectInfrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetString(command, "businessObjectCd", local.Lrf.BusinessObjectCd);
      },
      (db, reader) =>
      {
        entities.Old.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Old.ReasonCode = db.GetString(reader, 1);
        entities.Old.ResponsibilityCode = db.GetString(reader, 2);
        entities.Old.EffectiveDate = db.GetDate(reader, 3);
        entities.Old.OverrideInd = db.GetString(reader, 4);
        entities.Old.DiscontinueDate = db.GetNullableDate(reader, 5);
        entities.Old.CreatedBy = db.GetString(reader, 6);
        entities.Old.CreatedTimestamp = db.GetDateTime(reader, 7);
        entities.Old.LastUpdatedBy = db.GetNullableString(reader, 8);
        entities.Old.LastUpdatedTimestamp = db.GetNullableDateTime(reader, 9);
        entities.Old.SpdId = db.GetInt32(reader, 10);
        entities.Old.OffId = db.GetInt32(reader, 11);
        entities.Old.OspCode = db.GetString(reader, 12);
        entities.Old.OspDate = db.GetDate(reader, 13);
        entities.Old.MacId = db.GetInt32(reader, 14);
        entities.MonitoredActivity.SystemGeneratedIdentifier =
          db.GetInt32(reader, 14);
        entities.MonitoredActivity.Name = db.GetString(reader, 15);
        entities.MonitoredActivity.TypeCode = db.GetNullableString(reader, 16);
        entities.MonitoredActivity.StartDate = db.GetDate(reader, 17);
        entities.MonitoredActivity.ClosureDate = db.GetNullableDate(reader, 18);
        entities.MonitoredActivity.ClosureReasonCode =
          db.GetNullableString(reader, 19);
        entities.MonitoredActivity.InfSysGenId =
          db.GetNullableInt32(reader, 20);
        entities.MonitoredActivity.Populated = true;
        entities.Old.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadMonitoredActivityMonitoredActivityAssignment()
  {
    System.Diagnostics.Debug.Assert(
      entities.AkeyOnlyOfficeServiceProvider.Populated);
    entities.MonitoredActivity.Populated = false;
    entities.Old.Populated = false;

    return ReadEach("ReadMonitoredActivityMonitoredActivityAssignment",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "closureDate", local.MaxDate.Date.GetValueOrDefault());
        db.SetNullableString(command, "caseNumber", import.Hcase.Number);
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetDate(
          command, "ospDate",
          entities.AkeyOnlyOfficeServiceProvider.EffectiveDate.
            GetValueOrDefault());
        db.SetString(
          command, "ospCode", entities.AkeyOnlyOfficeServiceProvider.RoleCode);
        db.SetInt32(
          command, "offId",
          entities.AkeyOnlyOfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdId",
          entities.AkeyOnlyOfficeServiceProvider.SpdGeneratedId);
      },
      (db, reader) =>
      {
        entities.MonitoredActivity.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Old.MacId = db.GetInt32(reader, 0);
        entities.MonitoredActivity.Name = db.GetString(reader, 1);
        entities.MonitoredActivity.TypeCode = db.GetNullableString(reader, 2);
        entities.MonitoredActivity.StartDate = db.GetDate(reader, 3);
        entities.MonitoredActivity.ClosureDate = db.GetNullableDate(reader, 4);
        entities.MonitoredActivity.ClosureReasonCode =
          db.GetNullableString(reader, 5);
        entities.MonitoredActivity.InfSysGenId = db.GetNullableInt32(reader, 6);
        entities.Old.SystemGeneratedIdentifier = db.GetInt32(reader, 7);
        entities.Old.ReasonCode = db.GetString(reader, 8);
        entities.Old.ResponsibilityCode = db.GetString(reader, 9);
        entities.Old.EffectiveDate = db.GetDate(reader, 10);
        entities.Old.OverrideInd = db.GetString(reader, 11);
        entities.Old.DiscontinueDate = db.GetNullableDate(reader, 12);
        entities.Old.CreatedBy = db.GetString(reader, 13);
        entities.Old.CreatedTimestamp = db.GetDateTime(reader, 14);
        entities.Old.LastUpdatedBy = db.GetNullableString(reader, 15);
        entities.Old.LastUpdatedTimestamp = db.GetNullableDateTime(reader, 16);
        entities.Old.SpdId = db.GetInt32(reader, 17);
        entities.Old.OffId = db.GetInt32(reader, 18);
        entities.Old.OspCode = db.GetString(reader, 19);
        entities.Old.OspDate = db.GetDate(reader, 20);
        entities.MonitoredActivity.Populated = true;
        entities.Old.Populated = true;

        return true;
      });
  }

  private bool ReadObligation()
  {
    entities.Obligation.Populated = false;

    return Read("ReadObligation",
      (db, command) =>
      {
        db.SetInt32(
          command, "obId", export.Hobligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "dtyGeneratedId",
          export.HobligationType.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", export.HcsePersonAccount.Type1);
        db.SetString(command, "cspNumber", export.HcsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
      });
  }

  private bool ReadObligationAssignment1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationAssignment.Populated = false;

    return Read("ReadObligationAssignment1",
      (db, command) =>
      {
        db.SetInt32(
          command, "obgId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNo", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetInt32(command, "otyId", entities.Obligation.DtyGeneratedId);
        db.SetDate(
          command, "effectiveDate",
          export.Group.Item.CaseUnitFunctionAssignmt.DiscontinueDate.
            GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate",
          export.Group.Item.CaseUnitFunctionAssignmt.EffectiveDate.
            GetValueOrDefault());
        db.SetString(
          command, "reasonCode",
          export.Group.Item.CaseUnitFunctionAssignmt.ReasonCode);
        db.SetDateTime(
          command, "createdTimestamp",
          export.Group.Item.CaseUnitFunctionAssignmt.CreatedTimestamp.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligationAssignment.ReasonCode = db.GetString(reader, 0);
        entities.ObligationAssignment.OverrideInd = db.GetString(reader, 1);
        entities.ObligationAssignment.EffectiveDate = db.GetDate(reader, 2);
        entities.ObligationAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.ObligationAssignment.CreatedBy = db.GetString(reader, 4);
        entities.ObligationAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.ObligationAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.ObligationAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 7);
        entities.ObligationAssignment.SpdId = db.GetInt32(reader, 8);
        entities.ObligationAssignment.OffId = db.GetInt32(reader, 9);
        entities.ObligationAssignment.OspCode = db.GetString(reader, 10);
        entities.ObligationAssignment.OspDate = db.GetDate(reader, 11);
        entities.ObligationAssignment.OtyId = db.GetInt32(reader, 12);
        entities.ObligationAssignment.CpaType = db.GetString(reader, 13);
        entities.ObligationAssignment.CspNo = db.GetString(reader, 14);
        entities.ObligationAssignment.ObgId = db.GetInt32(reader, 15);
        entities.ObligationAssignment.Populated = true;
        CheckValid<ObligationAssignment>("CpaType",
          entities.ObligationAssignment.CpaType);
      });
  }

  private IEnumerable<bool> ReadObligationAssignment2()
  {
    entities.ObligationAssignment.Populated = false;

    return ReadEach("ReadObligationAssignment2",
      (db, command) =>
      {
        db.SetInt32(
          command, "obgId", export.Hobligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otyId", export.HobligationType.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", export.HcsePersonAccount.Type1);
        db.SetString(command, "cspNo", export.HcsePerson.Number);
        db.SetDate(
          command, "effectiveDate",
          export.HeaderStart.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligationAssignment.ReasonCode = db.GetString(reader, 0);
        entities.ObligationAssignment.OverrideInd = db.GetString(reader, 1);
        entities.ObligationAssignment.EffectiveDate = db.GetDate(reader, 2);
        entities.ObligationAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.ObligationAssignment.CreatedBy = db.GetString(reader, 4);
        entities.ObligationAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.ObligationAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.ObligationAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 7);
        entities.ObligationAssignment.SpdId = db.GetInt32(reader, 8);
        entities.ObligationAssignment.OffId = db.GetInt32(reader, 9);
        entities.ObligationAssignment.OspCode = db.GetString(reader, 10);
        entities.ObligationAssignment.OspDate = db.GetDate(reader, 11);
        entities.ObligationAssignment.OtyId = db.GetInt32(reader, 12);
        entities.ObligationAssignment.CpaType = db.GetString(reader, 13);
        entities.ObligationAssignment.CspNo = db.GetString(reader, 14);
        entities.ObligationAssignment.ObgId = db.GetInt32(reader, 15);
        entities.ObligationAssignment.Populated = true;
        CheckValid<ObligationAssignment>("CpaType",
          entities.ObligationAssignment.CpaType);

        return true;
      });
  }

  private bool ReadOffice()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingOfficeServiceProvider.Populated);
    entities.ExistingOffice.Populated = false;

    return Read("ReadOffice",
      (db, command) =>
      {
        db.SetInt32(
          command, "officeId",
          entities.ExistingOfficeServiceProvider.OffGeneratedId);
      },
      (db, reader) =>
      {
        entities.ExistingOffice.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingOffice.OffOffice = db.GetNullableInt32(reader, 1);
        entities.ExistingOffice.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvider1()
  {
    entities.AkeyOnlyOfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "closureDate", local.MaxDate.Date.GetValueOrDefault());
        db.SetNullableString(command, "caseNumber", import.Hcase.Number);
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Infrastructure.DenormNumeric12.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.AkeyOnlyOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.AkeyOnlyOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 1);
        entities.AkeyOnlyOfficeServiceProvider.RoleCode =
          db.GetString(reader, 2);
        entities.AkeyOnlyOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 3);
        entities.AkeyOnlyOfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvider2()
  {
    entities.AkeyOnlyOfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider2",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          export.OldOfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.SetString(
          command, "roleCode", export.OldOfficeServiceProvider.RoleCode);
        db.SetInt32(
          command, "offGeneratedId", export.OldOffice.SystemGeneratedId);
        db.SetInt32(
          command, "spdGeneratedId",
          export.OldServiceProvider.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.AkeyOnlyOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.AkeyOnlyOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 1);
        entities.AkeyOnlyOfficeServiceProvider.RoleCode =
          db.GetString(reader, 2);
        entities.AkeyOnlyOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 3);
        entities.AkeyOnlyOfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvider3()
  {
    entities.AkeyOnlyOfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider3",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          export.Group.Item.OfficeServiceProvider.EffectiveDate.
            GetValueOrDefault());
        db.SetString(
          command, "roleCode",
          export.Group.Item.OfficeServiceProvider.RoleCode);
        db.SetInt32(
          command, "offGeneratedId",
          export.Group.Item.Office.SystemGeneratedId);
        db.SetInt32(
          command, "spdGeneratedId",
          export.Group.Item.ServiceProvider.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.AkeyOnlyOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.AkeyOnlyOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 1);
        entities.AkeyOnlyOfficeServiceProvider.RoleCode =
          db.GetString(reader, 2);
        entities.AkeyOnlyOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 3);
        entities.AkeyOnlyOfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvider4()
  {
    entities.ExistingOfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider4",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          export.Group.Item.OfficeServiceProvider.EffectiveDate.
            GetValueOrDefault());
        db.SetString(
          command, "roleCode",
          export.Group.Item.OfficeServiceProvider.RoleCode);
        db.SetInt32(
          command, "offGeneratedId",
          export.Group.Item.Office.SystemGeneratedId);
        db.SetInt32(
          command, "spdGeneratedId",
          export.Group.Item.ServiceProvider.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.ExistingOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 1);
        entities.ExistingOfficeServiceProvider.RoleCode =
          db.GetString(reader, 2);
        entities.ExistingOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 3);
        entities.ExistingOfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.ExistingOfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadPaReferral()
  {
    entities.PaReferral.Populated = false;

    return Read("ReadPaReferral",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTstamp",
          export.HpaReferral.CreatedTimestamp.GetValueOrDefault());
        db.SetString(command, "numb", export.HpaReferral.Number);
        db.SetString(command, "type", export.HpaReferral.Type1);
      },
      (db, reader) =>
      {
        entities.PaReferral.Number = db.GetString(reader, 0);
        entities.PaReferral.Type1 = db.GetString(reader, 1);
        entities.PaReferral.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.PaReferral.Populated = true;
      });
  }

  private bool ReadPaReferralAssignment1()
  {
    entities.PaReferralAssignment.Populated = false;

    return Read("ReadPaReferralAssignment1",
      (db, command) =>
      {
        db.SetDateTime(
          command, "pafTstamp",
          entities.PaReferral.CreatedTimestamp.GetValueOrDefault());
        db.SetString(command, "pafType", entities.PaReferral.Type1);
        db.SetString(command, "pafNo", entities.PaReferral.Number);
        db.SetDate(
          command, "effectiveDate",
          export.Group.Item.CaseUnitFunctionAssignmt.DiscontinueDate.
            GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate",
          export.Group.Item.CaseUnitFunctionAssignmt.EffectiveDate.
            GetValueOrDefault());
        db.SetString(
          command, "reasonCode",
          export.Group.Item.CaseUnitFunctionAssignmt.ReasonCode);
        db.SetDateTime(
          command, "createdTimestamp",
          export.Group.Item.CaseUnitFunctionAssignmt.CreatedTimestamp.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PaReferralAssignment.ReasonCode = db.GetString(reader, 0);
        entities.PaReferralAssignment.OverrideInd = db.GetString(reader, 1);
        entities.PaReferralAssignment.EffectiveDate = db.GetDate(reader, 2);
        entities.PaReferralAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.PaReferralAssignment.CreatedBy = db.GetString(reader, 4);
        entities.PaReferralAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.PaReferralAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.PaReferralAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 7);
        entities.PaReferralAssignment.SpdId = db.GetInt32(reader, 8);
        entities.PaReferralAssignment.OffId = db.GetInt32(reader, 9);
        entities.PaReferralAssignment.OspCode = db.GetString(reader, 10);
        entities.PaReferralAssignment.OspDate = db.GetDate(reader, 11);
        entities.PaReferralAssignment.PafNo = db.GetString(reader, 12);
        entities.PaReferralAssignment.PafType = db.GetString(reader, 13);
        entities.PaReferralAssignment.PafTstamp = db.GetDateTime(reader, 14);
        entities.PaReferralAssignment.Populated = true;
      });
  }

  private IEnumerable<bool> ReadPaReferralAssignment2()
  {
    entities.PaReferralAssignment.Populated = false;

    return ReadEach("ReadPaReferralAssignment2",
      (db, command) =>
      {
        db.SetDateTime(
          command, "pafTstamp",
          export.HpaReferral.CreatedTimestamp.GetValueOrDefault());
        db.SetString(command, "pafNo", export.HpaReferral.Number);
        db.SetString(command, "pafType", export.HpaReferral.Type1);
        db.SetDate(
          command, "effectiveDate",
          export.HeaderStart.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PaReferralAssignment.ReasonCode = db.GetString(reader, 0);
        entities.PaReferralAssignment.OverrideInd = db.GetString(reader, 1);
        entities.PaReferralAssignment.EffectiveDate = db.GetDate(reader, 2);
        entities.PaReferralAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.PaReferralAssignment.CreatedBy = db.GetString(reader, 4);
        entities.PaReferralAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.PaReferralAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.PaReferralAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 7);
        entities.PaReferralAssignment.SpdId = db.GetInt32(reader, 8);
        entities.PaReferralAssignment.OffId = db.GetInt32(reader, 9);
        entities.PaReferralAssignment.OspCode = db.GetString(reader, 10);
        entities.PaReferralAssignment.OspDate = db.GetDate(reader, 11);
        entities.PaReferralAssignment.PafNo = db.GetString(reader, 12);
        entities.PaReferralAssignment.PafType = db.GetString(reader, 13);
        entities.PaReferralAssignment.PafTstamp = db.GetDateTime(reader, 14);
        entities.PaReferralAssignment.Populated = true;

        return true;
      });
  }

  private bool ReadServiceProviderOfficeOfficeServiceProvider()
  {
    entities.ExistingServiceProvider.Populated = false;
    entities.ExistingOfficeServiceProvider.Populated = false;
    entities.ExistingOffice.Populated = false;

    return Read("ReadServiceProviderOfficeOfficeServiceProvider",
      (db, command) =>
      {
        db.SetInt32(
          command, "servicePrvderId",
          export.Group.Item.ServiceProvider.SystemGeneratedId);
        db.SetString(
          command, "roleCode",
          export.Group.Item.OfficeServiceProvider.RoleCode);
        db.SetDate(
          command, "effectiveDate",
          export.Group.Item.OfficeServiceProvider.EffectiveDate.
            GetValueOrDefault());
        db.SetInt32(
          command, "officeId", export.Group.Item.Office.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.ExistingServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingServiceProvider.UserId = db.GetString(reader, 1);
        entities.ExistingServiceProvider.LastName = db.GetString(reader, 2);
        entities.ExistingServiceProvider.FirstName = db.GetString(reader, 3);
        entities.ExistingServiceProvider.MiddleInitial =
          db.GetString(reader, 4);
        entities.ExistingOffice.SystemGeneratedId = db.GetInt32(reader, 5);
        entities.ExistingOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 5);
        entities.ExistingOffice.OffOffice = db.GetNullableInt32(reader, 6);
        entities.ExistingOfficeServiceProvider.RoleCode =
          db.GetString(reader, 7);
        entities.ExistingOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 8);
        entities.ExistingOfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 9);
        entities.ExistingServiceProvider.Populated = true;
        entities.ExistingOfficeServiceProvider.Populated = true;
        entities.ExistingOffice.Populated = true;
      });
  }

  private IEnumerable<bool> ReadServiceProviderOfficeServiceProvider()
  {
    entities.ExistingServiceProvider.Populated = false;
    entities.ExistingOfficeServiceProvider.Populated = false;

    return ReadEach("ReadServiceProviderOfficeServiceProvider",
      (db, command) =>
      {
        db.
          SetString(command, "userId", export.Group.Item.ServiceProvider.UserId);
          
        db.SetNullableDate(
          command, "discontinueDate", local.MaxDate.Date.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate", local.CurrentDate.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.ExistingServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingServiceProvider.UserId = db.GetString(reader, 1);
        entities.ExistingServiceProvider.LastName = db.GetString(reader, 2);
        entities.ExistingServiceProvider.FirstName = db.GetString(reader, 3);
        entities.ExistingServiceProvider.MiddleInitial =
          db.GetString(reader, 4);
        entities.ExistingOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 5);
        entities.ExistingOfficeServiceProvider.RoleCode =
          db.GetString(reader, 6);
        entities.ExistingOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 7);
        entities.ExistingOfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 8);
        entities.ExistingServiceProvider.Populated = true;
        entities.ExistingOfficeServiceProvider.Populated = true;

        return true;
      });
  }

  private bool ReadServiceProviderOfficeServiceProviderOffice1()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionAssigment.Populated);
    entities.ExistingServiceProvider.Populated = false;
    entities.ExistingOfficeServiceProvider.Populated = false;
    entities.ExistingOffice.Populated = false;

    return Read("ReadServiceProviderOfficeServiceProviderOffice1",
      (db, command) =>
      {
        db.SetString(
          command, "roleCode", entities.LegalActionAssigment.OspRoleCode ?? ""
          );
        db.SetDate(
          command, "effectiveDate",
          entities.LegalActionAssigment.OspEffectiveDate.GetValueOrDefault());
        db.SetInt32(
          command, "offGeneratedId",
          entities.LegalActionAssigment.OffGeneratedId.GetValueOrDefault());
        db.SetInt32(
          command, "spdGeneratedId",
          entities.LegalActionAssigment.SpdGeneratedId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingServiceProvider.UserId = db.GetString(reader, 1);
        entities.ExistingServiceProvider.LastName = db.GetString(reader, 2);
        entities.ExistingServiceProvider.FirstName = db.GetString(reader, 3);
        entities.ExistingServiceProvider.MiddleInitial =
          db.GetString(reader, 4);
        entities.ExistingOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 5);
        entities.ExistingOffice.SystemGeneratedId = db.GetInt32(reader, 5);
        entities.ExistingOfficeServiceProvider.RoleCode =
          db.GetString(reader, 6);
        entities.ExistingOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 7);
        entities.ExistingOfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 8);
        entities.ExistingOffice.OffOffice = db.GetNullableInt32(reader, 9);
        entities.ExistingServiceProvider.Populated = true;
        entities.ExistingOfficeServiceProvider.Populated = true;
        entities.ExistingOffice.Populated = true;
      });
  }

  private bool ReadServiceProviderOfficeServiceProviderOffice2()
  {
    System.Diagnostics.Debug.Assert(
      entities.AdministrativeAppealAssignment.Populated);
    entities.ExistingServiceProvider.Populated = false;
    entities.ExistingOfficeServiceProvider.Populated = false;
    entities.ExistingOffice.Populated = false;

    return Read("ReadServiceProviderOfficeServiceProviderOffice2",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          entities.AdministrativeAppealAssignment.OspDate.GetValueOrDefault());
        db.SetString(
          command, "roleCode", entities.AdministrativeAppealAssignment.OspCode);
          
        db.SetInt32(
          command, "offGeneratedId",
          entities.AdministrativeAppealAssignment.OffId);
        db.SetInt32(
          command, "spdGeneratedId",
          entities.AdministrativeAppealAssignment.SpdId);
      },
      (db, reader) =>
      {
        entities.ExistingServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingServiceProvider.UserId = db.GetString(reader, 1);
        entities.ExistingServiceProvider.LastName = db.GetString(reader, 2);
        entities.ExistingServiceProvider.FirstName = db.GetString(reader, 3);
        entities.ExistingServiceProvider.MiddleInitial =
          db.GetString(reader, 4);
        entities.ExistingOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 5);
        entities.ExistingOffice.SystemGeneratedId = db.GetInt32(reader, 5);
        entities.ExistingOfficeServiceProvider.RoleCode =
          db.GetString(reader, 6);
        entities.ExistingOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 7);
        entities.ExistingOfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 8);
        entities.ExistingOffice.OffOffice = db.GetNullableInt32(reader, 9);
        entities.ExistingServiceProvider.Populated = true;
        entities.ExistingOfficeServiceProvider.Populated = true;
        entities.ExistingOffice.Populated = true;
      });
  }

  private bool ReadServiceProviderOfficeServiceProviderOffice3()
  {
    System.Diagnostics.Debug.Assert(entities.CaseAssignment.Populated);
    entities.ExistingServiceProvider.Populated = false;
    entities.ExistingOfficeServiceProvider.Populated = false;
    entities.ExistingOffice.Populated = false;

    return Read("ReadServiceProviderOfficeServiceProviderOffice3",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          entities.CaseAssignment.OspDate.GetValueOrDefault());
        db.SetString(command, "roleCode", entities.CaseAssignment.OspCode);
        db.SetInt32(command, "offGeneratedId", entities.CaseAssignment.OffId);
        db.SetInt32(command, "spdGeneratedId", entities.CaseAssignment.SpdId);
      },
      (db, reader) =>
      {
        entities.ExistingServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingServiceProvider.UserId = db.GetString(reader, 1);
        entities.ExistingServiceProvider.LastName = db.GetString(reader, 2);
        entities.ExistingServiceProvider.FirstName = db.GetString(reader, 3);
        entities.ExistingServiceProvider.MiddleInitial =
          db.GetString(reader, 4);
        entities.ExistingOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 5);
        entities.ExistingOffice.SystemGeneratedId = db.GetInt32(reader, 5);
        entities.ExistingOfficeServiceProvider.RoleCode =
          db.GetString(reader, 6);
        entities.ExistingOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 7);
        entities.ExistingOfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 8);
        entities.ExistingOffice.OffOffice = db.GetNullableInt32(reader, 9);
        entities.ExistingServiceProvider.Populated = true;
        entities.ExistingOfficeServiceProvider.Populated = true;
        entities.ExistingOffice.Populated = true;
      });
  }

  private bool ReadServiceProviderOfficeServiceProviderOffice4()
  {
    System.Diagnostics.Debug.
      Assert(entities.CaseUnitFunctionAssignmt.Populated);
    entities.ExistingServiceProvider.Populated = false;
    entities.ExistingOfficeServiceProvider.Populated = false;
    entities.ExistingOffice.Populated = false;

    return Read("ReadServiceProviderOfficeServiceProviderOffice4",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          entities.CaseUnitFunctionAssignmt.OspDate.GetValueOrDefault());
        db.SetString(
          command, "roleCode", entities.CaseUnitFunctionAssignmt.OspCode);
        db.SetInt32(
          command, "offGeneratedId", entities.CaseUnitFunctionAssignmt.OffId);
        db.SetInt32(
          command, "spdGeneratedId", entities.CaseUnitFunctionAssignmt.SpdId);
      },
      (db, reader) =>
      {
        entities.ExistingServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingServiceProvider.UserId = db.GetString(reader, 1);
        entities.ExistingServiceProvider.LastName = db.GetString(reader, 2);
        entities.ExistingServiceProvider.FirstName = db.GetString(reader, 3);
        entities.ExistingServiceProvider.MiddleInitial =
          db.GetString(reader, 4);
        entities.ExistingOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 5);
        entities.ExistingOffice.SystemGeneratedId = db.GetInt32(reader, 5);
        entities.ExistingOfficeServiceProvider.RoleCode =
          db.GetString(reader, 6);
        entities.ExistingOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 7);
        entities.ExistingOfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 8);
        entities.ExistingOffice.OffOffice = db.GetNullableInt32(reader, 9);
        entities.ExistingServiceProvider.Populated = true;
        entities.ExistingOfficeServiceProvider.Populated = true;
        entities.ExistingOffice.Populated = true;
      });
  }

  private bool ReadServiceProviderOfficeServiceProviderOffice5()
  {
    System.Diagnostics.Debug.
      Assert(entities.InterstateCaseAssignment.Populated);
    entities.ExistingServiceProvider.Populated = false;
    entities.ExistingOfficeServiceProvider.Populated = false;
    entities.ExistingOffice.Populated = false;

    return Read("ReadServiceProviderOfficeServiceProviderOffice5",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          entities.InterstateCaseAssignment.OspDate.GetValueOrDefault());
        db.SetString(
          command, "roleCode", entities.InterstateCaseAssignment.OspCode);
        db.SetInt32(
          command, "offGeneratedId", entities.InterstateCaseAssignment.OffId);
        db.SetInt32(
          command, "spdGeneratedId", entities.InterstateCaseAssignment.SpdId);
      },
      (db, reader) =>
      {
        entities.ExistingServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingServiceProvider.UserId = db.GetString(reader, 1);
        entities.ExistingServiceProvider.LastName = db.GetString(reader, 2);
        entities.ExistingServiceProvider.FirstName = db.GetString(reader, 3);
        entities.ExistingServiceProvider.MiddleInitial =
          db.GetString(reader, 4);
        entities.ExistingOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 5);
        entities.ExistingOffice.SystemGeneratedId = db.GetInt32(reader, 5);
        entities.ExistingOfficeServiceProvider.RoleCode =
          db.GetString(reader, 6);
        entities.ExistingOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 7);
        entities.ExistingOfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 8);
        entities.ExistingOffice.OffOffice = db.GetNullableInt32(reader, 9);
        entities.ExistingServiceProvider.Populated = true;
        entities.ExistingOfficeServiceProvider.Populated = true;
        entities.ExistingOffice.Populated = true;
      });
  }

  private bool ReadServiceProviderOfficeServiceProviderOffice6()
  {
    System.Diagnostics.Debug.Assert(entities.LegalReferralAssignment.Populated);
    entities.ExistingServiceProvider.Populated = false;
    entities.ExistingOfficeServiceProvider.Populated = false;
    entities.ExistingOffice.Populated = false;

    return Read("ReadServiceProviderOfficeServiceProviderOffice6",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          entities.LegalReferralAssignment.OspDate.GetValueOrDefault());
        db.SetString(
          command, "roleCode", entities.LegalReferralAssignment.OspCode);
        db.SetInt32(
          command, "offGeneratedId", entities.LegalReferralAssignment.OffId);
        db.SetInt32(
          command, "spdGeneratedId", entities.LegalReferralAssignment.SpdId);
      },
      (db, reader) =>
      {
        entities.ExistingServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingServiceProvider.UserId = db.GetString(reader, 1);
        entities.ExistingServiceProvider.LastName = db.GetString(reader, 2);
        entities.ExistingServiceProvider.FirstName = db.GetString(reader, 3);
        entities.ExistingServiceProvider.MiddleInitial =
          db.GetString(reader, 4);
        entities.ExistingOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 5);
        entities.ExistingOffice.SystemGeneratedId = db.GetInt32(reader, 5);
        entities.ExistingOfficeServiceProvider.RoleCode =
          db.GetString(reader, 6);
        entities.ExistingOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 7);
        entities.ExistingOfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 8);
        entities.ExistingOffice.OffOffice = db.GetNullableInt32(reader, 9);
        entities.ExistingServiceProvider.Populated = true;
        entities.ExistingOfficeServiceProvider.Populated = true;
        entities.ExistingOffice.Populated = true;
      });
  }

  private bool ReadServiceProviderOfficeServiceProviderOffice7()
  {
    System.Diagnostics.Debug.Assert(entities.Old.Populated);
    entities.ExistingServiceProvider.Populated = false;
    entities.ExistingOfficeServiceProvider.Populated = false;
    entities.ExistingOffice.Populated = false;

    return Read("ReadServiceProviderOfficeServiceProviderOffice7",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", entities.Old.OspDate.GetValueOrDefault());
        db.SetString(command, "roleCode", entities.Old.OspCode);
        db.SetInt32(command, "offGeneratedId", entities.Old.OffId);
        db.SetInt32(command, "spdGeneratedId", entities.Old.SpdId);
      },
      (db, reader) =>
      {
        entities.ExistingServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingServiceProvider.UserId = db.GetString(reader, 1);
        entities.ExistingServiceProvider.LastName = db.GetString(reader, 2);
        entities.ExistingServiceProvider.FirstName = db.GetString(reader, 3);
        entities.ExistingServiceProvider.MiddleInitial =
          db.GetString(reader, 4);
        entities.ExistingOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 5);
        entities.ExistingOffice.SystemGeneratedId = db.GetInt32(reader, 5);
        entities.ExistingOfficeServiceProvider.RoleCode =
          db.GetString(reader, 6);
        entities.ExistingOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 7);
        entities.ExistingOfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 8);
        entities.ExistingOffice.OffOffice = db.GetNullableInt32(reader, 9);
        entities.ExistingServiceProvider.Populated = true;
        entities.ExistingOfficeServiceProvider.Populated = true;
        entities.ExistingOffice.Populated = true;
      });
  }

  private bool ReadServiceProviderOfficeServiceProviderOffice8()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationAssignment.Populated);
    entities.ExistingServiceProvider.Populated = false;
    entities.ExistingOfficeServiceProvider.Populated = false;
    entities.ExistingOffice.Populated = false;

    return Read("ReadServiceProviderOfficeServiceProviderOffice8",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          entities.ObligationAssignment.OspDate.GetValueOrDefault());
        db.
          SetString(command, "roleCode", entities.ObligationAssignment.OspCode);
          
        db.SetInt32(
          command, "offGeneratedId", entities.ObligationAssignment.OffId);
        db.SetInt32(
          command, "spdGeneratedId", entities.ObligationAssignment.SpdId);
      },
      (db, reader) =>
      {
        entities.ExistingServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingServiceProvider.UserId = db.GetString(reader, 1);
        entities.ExistingServiceProvider.LastName = db.GetString(reader, 2);
        entities.ExistingServiceProvider.FirstName = db.GetString(reader, 3);
        entities.ExistingServiceProvider.MiddleInitial =
          db.GetString(reader, 4);
        entities.ExistingOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 5);
        entities.ExistingOffice.SystemGeneratedId = db.GetInt32(reader, 5);
        entities.ExistingOfficeServiceProvider.RoleCode =
          db.GetString(reader, 6);
        entities.ExistingOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 7);
        entities.ExistingOfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 8);
        entities.ExistingOffice.OffOffice = db.GetNullableInt32(reader, 9);
        entities.ExistingServiceProvider.Populated = true;
        entities.ExistingOfficeServiceProvider.Populated = true;
        entities.ExistingOffice.Populated = true;
      });
  }

  private bool ReadServiceProviderOfficeServiceProviderOffice9()
  {
    System.Diagnostics.Debug.Assert(entities.PaReferralAssignment.Populated);
    entities.ExistingServiceProvider.Populated = false;
    entities.ExistingOfficeServiceProvider.Populated = false;
    entities.ExistingOffice.Populated = false;

    return Read("ReadServiceProviderOfficeServiceProviderOffice9",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          entities.PaReferralAssignment.OspDate.GetValueOrDefault());
        db.
          SetString(command, "roleCode", entities.PaReferralAssignment.OspCode);
          
        db.SetInt32(
          command, "offGeneratedId", entities.PaReferralAssignment.OffId);
        db.SetInt32(
          command, "spdGeneratedId", entities.PaReferralAssignment.SpdId);
      },
      (db, reader) =>
      {
        entities.ExistingServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingServiceProvider.UserId = db.GetString(reader, 1);
        entities.ExistingServiceProvider.LastName = db.GetString(reader, 2);
        entities.ExistingServiceProvider.FirstName = db.GetString(reader, 3);
        entities.ExistingServiceProvider.MiddleInitial =
          db.GetString(reader, 4);
        entities.ExistingOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 5);
        entities.ExistingOffice.SystemGeneratedId = db.GetInt32(reader, 5);
        entities.ExistingOfficeServiceProvider.RoleCode =
          db.GetString(reader, 6);
        entities.ExistingOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 7);
        entities.ExistingOfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 8);
        entities.ExistingOffice.OffOffice = db.GetNullableInt32(reader, 9);
        entities.ExistingServiceProvider.Populated = true;
        entities.ExistingOfficeServiceProvider.Populated = true;
        entities.ExistingOffice.Populated = true;
      });
  }

  private bool ReadTribunal()
  {
    System.Diagnostics.Debug.Assert(entities.LegalAction.Populated);
    entities.Tribunal.Populated = false;

    return Read("ReadTribunal",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.LegalAction.TrbId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Tribunal.JudicialDivision = db.GetNullableString(reader, 0);
        entities.Tribunal.Name = db.GetString(reader, 1);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 2);
        entities.Tribunal.JudicialDistrict = db.GetString(reader, 3);
        entities.Tribunal.Identifier = db.GetInt32(reader, 4);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 5);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 6);
        entities.Tribunal.Populated = true;
      });
  }

  private void UpdateMonitoredActivityAssignment()
  {
    System.Diagnostics.Debug.Assert(entities.Old.Populated);

    var discontinueDate = local.LegalActionAssigment.DiscontinueDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = local.CurrentDate.Timestamp;

    entities.Old.Populated = false;
    Update("UpdateMonitoredActivityAssignment",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetDateTime(
          command, "createdTimestamp",
          entities.Old.CreatedTimestamp.GetValueOrDefault());
        db.SetInt32(command, "spdId", entities.Old.SpdId);
        db.SetInt32(command, "offId", entities.Old.OffId);
        db.SetString(command, "ospCode", entities.Old.OspCode);
        db.
          SetDate(command, "ospDate", entities.Old.OspDate.GetValueOrDefault());
          
        db.SetInt32(command, "macId", entities.Old.MacId);
      });

    entities.Old.DiscontinueDate = discontinueDate;
    entities.Old.LastUpdatedBy = lastUpdatedBy;
    entities.Old.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.Old.Populated = true;
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
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of Protect.
      /// </summary>
      [JsonPropertyName("protect")]
      public Common Protect
      {
        get => protect ??= new();
        set => protect = value;
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
      /// A value of CaseUnitFunctionAssignmt.
      /// </summary>
      [JsonPropertyName("caseUnitFunctionAssignmt")]
      public CaseUnitFunctionAssignmt CaseUnitFunctionAssignmt
      {
        get => caseUnitFunctionAssignmt ??= new();
        set => caseUnitFunctionAssignmt = value;
      }

      /// <summary>
      /// A value of HcaseUnitFunctionAssignmt.
      /// </summary>
      [JsonPropertyName("hcaseUnitFunctionAssignmt")]
      public CaseUnitFunctionAssignmt HcaseUnitFunctionAssignmt
      {
        get => hcaseUnitFunctionAssignmt ??= new();
        set => hcaseUnitFunctionAssignmt = value;
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
      /// A value of HserviceProvider.
      /// </summary>
      [JsonPropertyName("hserviceProvider")]
      public ServiceProvider HserviceProvider
      {
        get => hserviceProvider ??= new();
        set => hserviceProvider = value;
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
      /// A value of Hoffice.
      /// </summary>
      [JsonPropertyName("hoffice")]
      public Office Hoffice
      {
        get => hoffice ??= new();
        set => hoffice = value;
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
      /// A value of HofficeServiceProvider.
      /// </summary>
      [JsonPropertyName("hofficeServiceProvider")]
      public OfficeServiceProvider HofficeServiceProvider
      {
        get => hofficeServiceProvider ??= new();
        set => hofficeServiceProvider = value;
      }

      /// <summary>
      /// A value of PromptRsn.
      /// </summary>
      [JsonPropertyName("promptRsn")]
      public Standard PromptRsn
      {
        get => promptRsn ??= new();
        set => promptRsn = value;
      }

      /// <summary>
      /// A value of PromptSp.
      /// </summary>
      [JsonPropertyName("promptSp")]
      public Standard PromptSp
      {
        get => promptSp ??= new();
        set => promptSp = value;
      }

      /// <summary>
      /// A value of PromptFunc.
      /// </summary>
      [JsonPropertyName("promptFunc")]
      public Standard PromptFunc
      {
        get => promptFunc ??= new();
        set => promptFunc = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 75;

      private Common protect;
      private Common common;
      private CaseUnitFunctionAssignmt caseUnitFunctionAssignmt;
      private CaseUnitFunctionAssignmt hcaseUnitFunctionAssignmt;
      private ServiceProvider serviceProvider;
      private ServiceProvider hserviceProvider;
      private Office office;
      private Office hoffice;
      private OfficeServiceProvider officeServiceProvider;
      private OfficeServiceProvider hofficeServiceProvider;
      private Standard promptRsn;
      private Standard promptSp;
      private Standard promptFunc;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of Pointer.
    /// </summary>
    [JsonPropertyName("pointer")]
    public Common Pointer
    {
      get => pointer ??= new();
      set => pointer = value;
    }

    /// <summary>
    /// A value of FirstTime.
    /// </summary>
    [JsonPropertyName("firstTime")]
    public Common FirstTime
    {
      get => firstTime ??= new();
      set => firstTime = value;
    }

    /// <summary>
    /// A value of HcsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("hcsePersonsWorkSet")]
    public CsePersonsWorkSet HcsePersonsWorkSet
    {
      get => hcsePersonsWorkSet ??= new();
      set => hcsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Htribunal.
    /// </summary>
    [JsonPropertyName("htribunal")]
    public Tribunal Htribunal
    {
      get => htribunal ??= new();
      set => htribunal = value;
    }

    /// <summary>
    /// A value of HmonitoredActivity.
    /// </summary>
    [JsonPropertyName("hmonitoredActivity")]
    public MonitoredActivity HmonitoredActivity
    {
      get => hmonitoredActivity ??= new();
      set => hmonitoredActivity = value;
    }

    /// <summary>
    /// A value of HadministrativeAppeal.
    /// </summary>
    [JsonPropertyName("hadministrativeAppeal")]
    public AdministrativeAppeal HadministrativeAppeal
    {
      get => hadministrativeAppeal ??= new();
      set => hadministrativeAppeal = value;
    }

    /// <summary>
    /// A value of HadministrativeAction.
    /// </summary>
    [JsonPropertyName("hadministrativeAction")]
    public AdministrativeAction HadministrativeAction
    {
      get => hadministrativeAction ??= new();
      set => hadministrativeAction = value;
    }

    /// <summary>
    /// A value of HcsePersonAccount.
    /// </summary>
    [JsonPropertyName("hcsePersonAccount")]
    public CsePersonAccount HcsePersonAccount
    {
      get => hcsePersonAccount ??= new();
      set => hcsePersonAccount = value;
    }

    /// <summary>
    /// A value of HobligationType.
    /// </summary>
    [JsonPropertyName("hobligationType")]
    public ObligationType HobligationType
    {
      get => hobligationType ??= new();
      set => hobligationType = value;
    }

    /// <summary>
    /// A value of Hobligation.
    /// </summary>
    [JsonPropertyName("hobligation")]
    public Obligation Hobligation
    {
      get => hobligation ??= new();
      set => hobligation = value;
    }

    /// <summary>
    /// A value of HpaReferral.
    /// </summary>
    [JsonPropertyName("hpaReferral")]
    public PaReferral HpaReferral
    {
      get => hpaReferral ??= new();
      set => hpaReferral = value;
    }

    /// <summary>
    /// A value of HlegalReferral.
    /// </summary>
    [JsonPropertyName("hlegalReferral")]
    public LegalReferral HlegalReferral
    {
      get => hlegalReferral ??= new();
      set => hlegalReferral = value;
    }

    /// <summary>
    /// A value of HlegalAction.
    /// </summary>
    [JsonPropertyName("hlegalAction")]
    public LegalAction HlegalAction
    {
      get => hlegalAction ??= new();
      set => hlegalAction = value;
    }

    /// <summary>
    /// A value of HcsePerson.
    /// </summary>
    [JsonPropertyName("hcsePerson")]
    public CsePerson HcsePerson
    {
      get => hcsePerson ??= new();
      set => hcsePerson = value;
    }

    /// <summary>
    /// A value of HcaseUnit.
    /// </summary>
    [JsonPropertyName("hcaseUnit")]
    public CaseUnit HcaseUnit
    {
      get => hcaseUnit ??= new();
      set => hcaseUnit = value;
    }

    /// <summary>
    /// A value of Hcase.
    /// </summary>
    [JsonPropertyName("hcase")]
    public Case1 Hcase
    {
      get => hcase ??= new();
      set => hcase = value;
    }

    /// <summary>
    /// A value of HiddenOffice.
    /// </summary>
    [JsonPropertyName("hiddenOffice")]
    public Office HiddenOffice
    {
      get => hiddenOffice ??= new();
      set => hiddenOffice = value;
    }

    /// <summary>
    /// A value of HiddenServiceProvider.
    /// </summary>
    [JsonPropertyName("hiddenServiceProvider")]
    public ServiceProvider HiddenServiceProvider
    {
      get => hiddenServiceProvider ??= new();
      set => hiddenServiceProvider = value;
    }

    /// <summary>
    /// A value of HiddenOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("hiddenOfficeServiceProvider")]
    public OfficeServiceProvider HiddenOfficeServiceProvider
    {
      get => hiddenOfficeServiceProvider ??= new();
      set => hiddenOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of HiddenCode.
    /// </summary>
    [JsonPropertyName("hiddenCode")]
    public Code HiddenCode
    {
      get => hiddenCode ??= new();
      set => hiddenCode = value;
    }

    /// <summary>
    /// A value of HiddenCodeValue.
    /// </summary>
    [JsonPropertyName("hiddenCodeValue")]
    public CodeValue HiddenCodeValue
    {
      get => hiddenCodeValue ??= new();
      set => hiddenCodeValue = value;
    }

    /// <summary>
    /// A value of HeaderObjTitle2.
    /// </summary>
    [JsonPropertyName("headerObjTitle2")]
    public SpTextWorkArea HeaderObjTitle2
    {
      get => headerObjTitle2 ??= new();
      set => headerObjTitle2 = value;
    }

    /// <summary>
    /// A value of HeaderObjTitle1.
    /// </summary>
    [JsonPropertyName("headerObjTitle1")]
    public SpTextWorkArea HeaderObjTitle1
    {
      get => headerObjTitle1 ??= new();
      set => headerObjTitle1 = value;
    }

    /// <summary>
    /// A value of HeaderStart.
    /// </summary>
    [JsonPropertyName("headerStart")]
    public DateWorkArea HeaderStart
    {
      get => headerStart ??= new();
      set => headerStart = value;
    }

    /// <summary>
    /// A value of HeaderObject.
    /// </summary>
    [JsonPropertyName("headerObject")]
    public SpTextWorkArea HeaderObject
    {
      get => headerObject ??= new();
      set => headerObject = value;
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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
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
    /// A value of OldServiceProvider.
    /// </summary>
    [JsonPropertyName("oldServiceProvider")]
    public ServiceProvider OldServiceProvider
    {
      get => oldServiceProvider ??= new();
      set => oldServiceProvider = value;
    }

    /// <summary>
    /// A value of OldOffice.
    /// </summary>
    [JsonPropertyName("oldOffice")]
    public Office OldOffice
    {
      get => oldOffice ??= new();
      set => oldOffice = value;
    }

    /// <summary>
    /// A value of OldOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("oldOfficeServiceProvider")]
    public OfficeServiceProvider OldOfficeServiceProvider
    {
      get => oldOfficeServiceProvider ??= new();
      set => oldOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of OldLegalReferralAssignment.
    /// </summary>
    [JsonPropertyName("oldLegalReferralAssignment")]
    public LegalReferralAssignment OldLegalReferralAssignment
    {
      get => oldLegalReferralAssignment ??= new();
      set => oldLegalReferralAssignment = value;
    }

    /// <summary>
    /// A value of OldLegalActionAssigment.
    /// </summary>
    [JsonPropertyName("oldLegalActionAssigment")]
    public LegalActionAssigment OldLegalActionAssigment
    {
      get => oldLegalActionAssigment ??= new();
      set => oldLegalActionAssigment = value;
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
    /// A value of Compare.
    /// </summary>
    [JsonPropertyName("compare")]
    public LegalAction Compare
    {
      get => compare ??= new();
      set => compare = value;
    }

    private CsePersonsWorkSet ap;
    private Common pointer;
    private Common firstTime;
    private CsePersonsWorkSet hcsePersonsWorkSet;
    private Tribunal htribunal;
    private MonitoredActivity hmonitoredActivity;
    private AdministrativeAppeal hadministrativeAppeal;
    private AdministrativeAction hadministrativeAction;
    private CsePersonAccount hcsePersonAccount;
    private ObligationType hobligationType;
    private Obligation hobligation;
    private PaReferral hpaReferral;
    private LegalReferral hlegalReferral;
    private LegalAction hlegalAction;
    private CsePerson hcsePerson;
    private CaseUnit hcaseUnit;
    private Case1 hcase;
    private Office hiddenOffice;
    private ServiceProvider hiddenServiceProvider;
    private OfficeServiceProvider hiddenOfficeServiceProvider;
    private Code hiddenCode;
    private CodeValue hiddenCodeValue;
    private SpTextWorkArea headerObjTitle2;
    private SpTextWorkArea headerObjTitle1;
    private DateWorkArea headerStart;
    private SpTextWorkArea headerObject;
    private Array<GroupGroup> group;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
    private ServiceProvider oldServiceProvider;
    private Office oldOffice;
    private OfficeServiceProvider oldOfficeServiceProvider;
    private LegalReferralAssignment oldLegalReferralAssignment;
    private LegalActionAssigment oldLegalActionAssigment;
    private WorkArea detail;
    private LegalAction compare;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of Protect.
      /// </summary>
      [JsonPropertyName("protect")]
      public Common Protect
      {
        get => protect ??= new();
        set => protect = value;
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
      /// A value of CaseUnitFunctionAssignmt.
      /// </summary>
      [JsonPropertyName("caseUnitFunctionAssignmt")]
      public CaseUnitFunctionAssignmt CaseUnitFunctionAssignmt
      {
        get => caseUnitFunctionAssignmt ??= new();
        set => caseUnitFunctionAssignmt = value;
      }

      /// <summary>
      /// A value of HcaseUnitFunctionAssignmt.
      /// </summary>
      [JsonPropertyName("hcaseUnitFunctionAssignmt")]
      public CaseUnitFunctionAssignmt HcaseUnitFunctionAssignmt
      {
        get => hcaseUnitFunctionAssignmt ??= new();
        set => hcaseUnitFunctionAssignmt = value;
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
      /// A value of HserviceProvider.
      /// </summary>
      [JsonPropertyName("hserviceProvider")]
      public ServiceProvider HserviceProvider
      {
        get => hserviceProvider ??= new();
        set => hserviceProvider = value;
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
      /// A value of Hoffice.
      /// </summary>
      [JsonPropertyName("hoffice")]
      public Office Hoffice
      {
        get => hoffice ??= new();
        set => hoffice = value;
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
      /// A value of HofficeServiceProvider.
      /// </summary>
      [JsonPropertyName("hofficeServiceProvider")]
      public OfficeServiceProvider HofficeServiceProvider
      {
        get => hofficeServiceProvider ??= new();
        set => hofficeServiceProvider = value;
      }

      /// <summary>
      /// A value of PromptSp.
      /// </summary>
      [JsonPropertyName("promptSp")]
      public Standard PromptSp
      {
        get => promptSp ??= new();
        set => promptSp = value;
      }

      /// <summary>
      /// A value of PromptRsn.
      /// </summary>
      [JsonPropertyName("promptRsn")]
      public Standard PromptRsn
      {
        get => promptRsn ??= new();
        set => promptRsn = value;
      }

      /// <summary>
      /// A value of PromptFunc.
      /// </summary>
      [JsonPropertyName("promptFunc")]
      public Standard PromptFunc
      {
        get => promptFunc ??= new();
        set => promptFunc = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 75;

      private Common protect;
      private Common common;
      private CaseUnitFunctionAssignmt caseUnitFunctionAssignmt;
      private CaseUnitFunctionAssignmt hcaseUnitFunctionAssignmt;
      private ServiceProvider serviceProvider;
      private ServiceProvider hserviceProvider;
      private Office office;
      private Office hoffice;
      private OfficeServiceProvider officeServiceProvider;
      private OfficeServiceProvider hofficeServiceProvider;
      private Standard promptSp;
      private Standard promptRsn;
      private Standard promptFunc;
    }

    /// <summary>
    /// A value of Pointer.
    /// </summary>
    [JsonPropertyName("pointer")]
    public Common Pointer
    {
      get => pointer ??= new();
      set => pointer = value;
    }

    /// <summary>
    /// A value of FirstTime.
    /// </summary>
    [JsonPropertyName("firstTime")]
    public Common FirstTime
    {
      get => firstTime ??= new();
      set => firstTime = value;
    }

    /// <summary>
    /// A value of HiddenCaseUnitFunctionAssignmt.
    /// </summary>
    [JsonPropertyName("hiddenCaseUnitFunctionAssignmt")]
    public CaseUnitFunctionAssignmt HiddenCaseUnitFunctionAssignmt
    {
      get => hiddenCaseUnitFunctionAssignmt ??= new();
      set => hiddenCaseUnitFunctionAssignmt = value;
    }

    /// <summary>
    /// A value of HcsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("hcsePersonsWorkSet")]
    public CsePersonsWorkSet HcsePersonsWorkSet
    {
      get => hcsePersonsWorkSet ??= new();
      set => hcsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Htribunal.
    /// </summary>
    [JsonPropertyName("htribunal")]
    public Tribunal Htribunal
    {
      get => htribunal ??= new();
      set => htribunal = value;
    }

    /// <summary>
    /// A value of HmonitoredActivity.
    /// </summary>
    [JsonPropertyName("hmonitoredActivity")]
    public MonitoredActivity HmonitoredActivity
    {
      get => hmonitoredActivity ??= new();
      set => hmonitoredActivity = value;
    }

    /// <summary>
    /// A value of HadministrativeAppeal.
    /// </summary>
    [JsonPropertyName("hadministrativeAppeal")]
    public AdministrativeAppeal HadministrativeAppeal
    {
      get => hadministrativeAppeal ??= new();
      set => hadministrativeAppeal = value;
    }

    /// <summary>
    /// A value of HadministrativeAction.
    /// </summary>
    [JsonPropertyName("hadministrativeAction")]
    public AdministrativeAction HadministrativeAction
    {
      get => hadministrativeAction ??= new();
      set => hadministrativeAction = value;
    }

    /// <summary>
    /// A value of HcsePersonAccount.
    /// </summary>
    [JsonPropertyName("hcsePersonAccount")]
    public CsePersonAccount HcsePersonAccount
    {
      get => hcsePersonAccount ??= new();
      set => hcsePersonAccount = value;
    }

    /// <summary>
    /// A value of HobligationType.
    /// </summary>
    [JsonPropertyName("hobligationType")]
    public ObligationType HobligationType
    {
      get => hobligationType ??= new();
      set => hobligationType = value;
    }

    /// <summary>
    /// A value of Hobligation.
    /// </summary>
    [JsonPropertyName("hobligation")]
    public Obligation Hobligation
    {
      get => hobligation ??= new();
      set => hobligation = value;
    }

    /// <summary>
    /// A value of HpaReferral.
    /// </summary>
    [JsonPropertyName("hpaReferral")]
    public PaReferral HpaReferral
    {
      get => hpaReferral ??= new();
      set => hpaReferral = value;
    }

    /// <summary>
    /// A value of HlegalReferral.
    /// </summary>
    [JsonPropertyName("hlegalReferral")]
    public LegalReferral HlegalReferral
    {
      get => hlegalReferral ??= new();
      set => hlegalReferral = value;
    }

    /// <summary>
    /// A value of HlegalAction.
    /// </summary>
    [JsonPropertyName("hlegalAction")]
    public LegalAction HlegalAction
    {
      get => hlegalAction ??= new();
      set => hlegalAction = value;
    }

    /// <summary>
    /// A value of HcsePerson.
    /// </summary>
    [JsonPropertyName("hcsePerson")]
    public CsePerson HcsePerson
    {
      get => hcsePerson ??= new();
      set => hcsePerson = value;
    }

    /// <summary>
    /// A value of HcaseUnit.
    /// </summary>
    [JsonPropertyName("hcaseUnit")]
    public CaseUnit HcaseUnit
    {
      get => hcaseUnit ??= new();
      set => hcaseUnit = value;
    }

    /// <summary>
    /// A value of Hcase.
    /// </summary>
    [JsonPropertyName("hcase")]
    public Case1 Hcase
    {
      get => hcase ??= new();
      set => hcase = value;
    }

    /// <summary>
    /// A value of HiddenOffice.
    /// </summary>
    [JsonPropertyName("hiddenOffice")]
    public Office HiddenOffice
    {
      get => hiddenOffice ??= new();
      set => hiddenOffice = value;
    }

    /// <summary>
    /// A value of HiddenServiceProvider.
    /// </summary>
    [JsonPropertyName("hiddenServiceProvider")]
    public ServiceProvider HiddenServiceProvider
    {
      get => hiddenServiceProvider ??= new();
      set => hiddenServiceProvider = value;
    }

    /// <summary>
    /// A value of HiddenOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("hiddenOfficeServiceProvider")]
    public OfficeServiceProvider HiddenOfficeServiceProvider
    {
      get => hiddenOfficeServiceProvider ??= new();
      set => hiddenOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of HiddenCode.
    /// </summary>
    [JsonPropertyName("hiddenCode")]
    public Code HiddenCode
    {
      get => hiddenCode ??= new();
      set => hiddenCode = value;
    }

    /// <summary>
    /// A value of HiddenCodeValue.
    /// </summary>
    [JsonPropertyName("hiddenCodeValue")]
    public CodeValue HiddenCodeValue
    {
      get => hiddenCodeValue ??= new();
      set => hiddenCodeValue = value;
    }

    /// <summary>
    /// A value of HeaderObjTitle2.
    /// </summary>
    [JsonPropertyName("headerObjTitle2")]
    public SpTextWorkArea HeaderObjTitle2
    {
      get => headerObjTitle2 ??= new();
      set => headerObjTitle2 = value;
    }

    /// <summary>
    /// A value of HeaderObjTitle1.
    /// </summary>
    [JsonPropertyName("headerObjTitle1")]
    public SpTextWorkArea HeaderObjTitle1
    {
      get => headerObjTitle1 ??= new();
      set => headerObjTitle1 = value;
    }

    /// <summary>
    /// A value of HeaderStart.
    /// </summary>
    [JsonPropertyName("headerStart")]
    public DateWorkArea HeaderStart
    {
      get => headerStart ??= new();
      set => headerStart = value;
    }

    /// <summary>
    /// A value of HeaderObject.
    /// </summary>
    [JsonPropertyName("headerObject")]
    public SpTextWorkArea HeaderObject
    {
      get => headerObject ??= new();
      set => headerObject = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
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
    /// A value of HeaderPromptType.
    /// </summary>
    [JsonPropertyName("headerPromptType")]
    public Standard HeaderPromptType
    {
      get => headerPromptType ??= new();
      set => headerPromptType = value;
    }

    /// <summary>
    /// A value of AddupdOkAsin.
    /// </summary>
    [JsonPropertyName("addupdOkAsin")]
    public WorkArea AddupdOkAsin
    {
      get => addupdOkAsin ??= new();
      set => addupdOkAsin = value;
    }

    /// <summary>
    /// A value of OldServiceProvider.
    /// </summary>
    [JsonPropertyName("oldServiceProvider")]
    public ServiceProvider OldServiceProvider
    {
      get => oldServiceProvider ??= new();
      set => oldServiceProvider = value;
    }

    /// <summary>
    /// A value of OldOffice.
    /// </summary>
    [JsonPropertyName("oldOffice")]
    public Office OldOffice
    {
      get => oldOffice ??= new();
      set => oldOffice = value;
    }

    /// <summary>
    /// A value of OldOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("oldOfficeServiceProvider")]
    public OfficeServiceProvider OldOfficeServiceProvider
    {
      get => oldOfficeServiceProvider ??= new();
      set => oldOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of OldLegalReferralAssignment.
    /// </summary>
    [JsonPropertyName("oldLegalReferralAssignment")]
    public LegalReferralAssignment OldLegalReferralAssignment
    {
      get => oldLegalReferralAssignment ??= new();
      set => oldLegalReferralAssignment = value;
    }

    /// <summary>
    /// A value of OldLegalActionAssigment.
    /// </summary>
    [JsonPropertyName("oldLegalActionAssigment")]
    public LegalActionAssigment OldLegalActionAssigment
    {
      get => oldLegalActionAssigment ??= new();
      set => oldLegalActionAssigment = value;
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
    /// A value of Compare.
    /// </summary>
    [JsonPropertyName("compare")]
    public LegalAction Compare
    {
      get => compare ??= new();
      set => compare = value;
    }

    private Common pointer;
    private Common firstTime;
    private CaseUnitFunctionAssignmt hiddenCaseUnitFunctionAssignmt;
    private CsePersonsWorkSet hcsePersonsWorkSet;
    private Tribunal htribunal;
    private MonitoredActivity hmonitoredActivity;
    private AdministrativeAppeal hadministrativeAppeal;
    private AdministrativeAction hadministrativeAction;
    private CsePersonAccount hcsePersonAccount;
    private ObligationType hobligationType;
    private Obligation hobligation;
    private PaReferral hpaReferral;
    private LegalReferral hlegalReferral;
    private LegalAction hlegalAction;
    private CsePerson hcsePerson;
    private CaseUnit hcaseUnit;
    private Case1 hcase;
    private Office hiddenOffice;
    private ServiceProvider hiddenServiceProvider;
    private OfficeServiceProvider hiddenOfficeServiceProvider;
    private Code hiddenCode;
    private CodeValue hiddenCodeValue;
    private SpTextWorkArea headerObjTitle2;
    private SpTextWorkArea headerObjTitle1;
    private DateWorkArea headerStart;
    private SpTextWorkArea headerObject;
    private Array<GroupGroup> group;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
    private Standard headerPromptType;
    private WorkArea addupdOkAsin;
    private ServiceProvider oldServiceProvider;
    private Office oldOffice;
    private OfficeServiceProvider oldOfficeServiceProvider;
    private LegalReferralAssignment oldLegalReferralAssignment;
    private LegalActionAssigment oldLegalActionAssigment;
    private WorkArea detail;
    private LegalAction compare;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A ConcatGroup group.</summary>
    [Serializable]
    public class ConcatGroup
    {
      /// <summary>
      /// A value of GrpoLocString.
      /// </summary>
      [JsonPropertyName("grpoLocString")]
      public WorkArea GrpoLocString
      {
        get => grpoLocString ??= new();
        set => grpoLocString = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private WorkArea grpoLocString;
    }

    /// <summary>A DelGroup group.</summary>
    [Serializable]
    public class DelGroup
    {
      /// <summary>
      /// A value of CaseUnitFunctionAssignmt.
      /// </summary>
      [JsonPropertyName("caseUnitFunctionAssignmt")]
      public CaseUnitFunctionAssignmt CaseUnitFunctionAssignmt
      {
        get => caseUnitFunctionAssignmt ??= new();
        set => caseUnitFunctionAssignmt = value;
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
      /// A value of Office.
      /// </summary>
      [JsonPropertyName("office")]
      public Office Office
      {
        get => office ??= new();
        set => office = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private CaseUnitFunctionAssignmt caseUnitFunctionAssignmt;
      private OfficeServiceProvider officeServiceProvider;
      private ServiceProvider serviceProvider;
      private Office office;
    }

    /// <summary>
    /// A value of Other.
    /// </summary>
    [JsonPropertyName("other")]
    public Infrastructure Other
    {
      get => other ??= new();
      set => other = value;
    }

    /// <summary>
    /// A value of FindDoc.
    /// </summary>
    [JsonPropertyName("findDoc")]
    public Common FindDoc
    {
      get => findDoc ??= new();
      set => findDoc = value;
    }

    /// <summary>
    /// A value of PrevDocsPrinted.
    /// </summary>
    [JsonPropertyName("prevDocsPrinted")]
    public Common PrevDocsPrinted
    {
      get => prevDocsPrinted ??= new();
      set => prevDocsPrinted = value;
    }

    /// <summary>
    /// A value of ExistingServiceProvider.
    /// </summary>
    [JsonPropertyName("existingServiceProvider")]
    public ServiceProvider ExistingServiceProvider
    {
      get => existingServiceProvider ??= new();
      set => existingServiceProvider = value;
    }

    /// <summary>
    /// A value of ExistingOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("existingOfficeServiceProvider")]
    public OfficeServiceProvider ExistingOfficeServiceProvider
    {
      get => existingOfficeServiceProvider ??= new();
      set => existingOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of ExistingOffice.
    /// </summary>
    [JsonPropertyName("existingOffice")]
    public Office ExistingOffice
    {
      get => existingOffice ??= new();
      set => existingOffice = value;
    }

    /// <summary>
    /// A value of Work.
    /// </summary>
    [JsonPropertyName("work")]
    public Common Work
    {
      get => work ??= new();
      set => work = value;
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
    /// A value of SelectInfrastructure.
    /// </summary>
    [JsonPropertyName("selectInfrastructure")]
    public Infrastructure SelectInfrastructure
    {
      get => selectInfrastructure ??= new();
      set => selectInfrastructure = value;
    }

    /// <summary>
    /// A value of InitLegalAction.
    /// </summary>
    [JsonPropertyName("initLegalAction")]
    public LegalAction InitLegalAction
    {
      get => initLegalAction ??= new();
      set => initLegalAction = value;
    }

    /// <summary>
    /// A value of Compare.
    /// </summary>
    [JsonPropertyName("compare")]
    public Infrastructure Compare
    {
      get => compare ??= new();
      set => compare = value;
    }

    /// <summary>
    /// A value of InitWorkArea.
    /// </summary>
    [JsonPropertyName("initWorkArea")]
    public WorkArea InitWorkArea
    {
      get => initWorkArea ??= new();
      set => initWorkArea = value;
    }

    /// <summary>
    /// A value of InitLegalReferralAssignment.
    /// </summary>
    [JsonPropertyName("initLegalReferralAssignment")]
    public LegalReferralAssignment InitLegalReferralAssignment
    {
      get => initLegalReferralAssignment ??= new();
      set => initLegalReferralAssignment = value;
    }

    /// <summary>
    /// A value of InitLegalActionAssigment.
    /// </summary>
    [JsonPropertyName("initLegalActionAssigment")]
    public LegalActionAssigment InitLegalActionAssigment
    {
      get => initLegalActionAssigment ??= new();
      set => initLegalActionAssigment = value;
    }

    /// <summary>
    /// A value of InitServiceProvider.
    /// </summary>
    [JsonPropertyName("initServiceProvider")]
    public ServiceProvider InitServiceProvider
    {
      get => initServiceProvider ??= new();
      set => initServiceProvider = value;
    }

    /// <summary>
    /// A value of InitOffice.
    /// </summary>
    [JsonPropertyName("initOffice")]
    public Office InitOffice
    {
      get => initOffice ??= new();
      set => initOffice = value;
    }

    /// <summary>
    /// A value of InitOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("initOfficeServiceProvider")]
    public OfficeServiceProvider InitOfficeServiceProvider
    {
      get => initOfficeServiceProvider ??= new();
      set => initOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of Saved.
    /// </summary>
    [JsonPropertyName("saved")]
    public Infrastructure Saved
    {
      get => saved ??= new();
      set => saved = value;
    }

    /// <summary>
    /// A value of Lea.
    /// </summary>
    [JsonPropertyName("lea")]
    public Infrastructure Lea
    {
      get => lea ??= new();
      set => lea = value;
    }

    /// <summary>
    /// A value of Lrf.
    /// </summary>
    [JsonPropertyName("lrf")]
    public Infrastructure Lrf
    {
      get => lrf ??= new();
      set => lrf = value;
    }

    /// <summary>
    /// A value of LegalReferral.
    /// </summary>
    [JsonPropertyName("legalReferral")]
    public MonitoredActivity LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
    }

    /// <summary>
    /// A value of Icase.
    /// </summary>
    [JsonPropertyName("icase")]
    public InterstateCase Icase
    {
      get => icase ??= new();
      set => icase = value;
    }

    /// <summary>
    /// A value of InterstateCaseAssignment.
    /// </summary>
    [JsonPropertyName("interstateCaseAssignment")]
    public InterstateCaseAssignment InterstateCaseAssignment
    {
      get => interstateCaseAssignment ??= new();
      set => interstateCaseAssignment = value;
    }

    /// <summary>
    /// A value of ScreenIdentification.
    /// </summary>
    [JsonPropertyName("screenIdentification")]
    public Common ScreenIdentification
    {
      get => screenIdentification ??= new();
      set => screenIdentification = value;
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
    /// A value of FunctionCount.
    /// </summary>
    [JsonPropertyName("functionCount")]
    public Common FunctionCount
    {
      get => functionCount ??= new();
      set => functionCount = value;
    }

    /// <summary>
    /// A value of SpCount.
    /// </summary>
    [JsonPropertyName("spCount")]
    public Common SpCount
    {
      get => spCount ??= new();
      set => spCount = value;
    }

    /// <summary>
    /// A value of ReasonCount.
    /// </summary>
    [JsonPropertyName("reasonCount")]
    public Common ReasonCount
    {
      get => reasonCount ??= new();
      set => reasonCount = value;
    }

    /// <summary>
    /// A value of SelResSpFuncInd.
    /// </summary>
    [JsonPropertyName("selResSpFuncInd")]
    public Common SelResSpFuncInd
    {
      get => selResSpFuncInd ??= new();
      set => selResSpFuncInd = value;
    }

    /// <summary>
    /// A value of OutgoingDocument.
    /// </summary>
    [JsonPropertyName("outgoingDocument")]
    public OutgoingDocument OutgoingDocument
    {
      get => outgoingDocument ??= new();
      set => outgoingDocument = value;
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
    /// A value of DelLocalLast.
    /// </summary>
    [JsonPropertyName("delLocalLast")]
    public Common DelLocalLast
    {
      get => delLocalLast ??= new();
      set => delLocalLast = value;
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
    /// A value of RowCounter.
    /// </summary>
    [JsonPropertyName("rowCounter")]
    public Common RowCounter
    {
      get => rowCounter ??= new();
      set => rowCounter = value;
    }

    /// <summary>
    /// A value of SelCount.
    /// </summary>
    [JsonPropertyName("selCount")]
    public Common SelCount
    {
      get => selCount ??= new();
      set => selCount = value;
    }

    /// <summary>
    /// A value of SelectDateWorkArea.
    /// </summary>
    [JsonPropertyName("selectDateWorkArea")]
    public DateWorkArea SelectDateWorkArea
    {
      get => selectDateWorkArea ??= new();
      set => selectDateWorkArea = value;
    }

    /// <summary>
    /// A value of ObligationAssignment.
    /// </summary>
    [JsonPropertyName("obligationAssignment")]
    public ObligationAssignment ObligationAssignment
    {
      get => obligationAssignment ??= new();
      set => obligationAssignment = value;
    }

    /// <summary>
    /// A value of ActiveAssignmentFound.
    /// </summary>
    [JsonPropertyName("activeAssignmentFound")]
    public Common ActiveAssignmentFound
    {
      get => activeAssignmentFound ??= new();
      set => activeAssignmentFound = value;
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
    /// A value of DelLocalQuery.
    /// </summary>
    [JsonPropertyName("delLocalQuery")]
    public DateWorkArea DelLocalQuery
    {
      get => delLocalQuery ??= new();
      set => delLocalQuery = value;
    }

    /// <summary>
    /// A value of AddCommand.
    /// </summary>
    [JsonPropertyName("addCommand")]
    public Common AddCommand
    {
      get => addCommand ??= new();
      set => addCommand = value;
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
    /// A value of NoOfEntriesSelected.
    /// </summary>
    [JsonPropertyName("noOfEntriesSelected")]
    public Common NoOfEntriesSelected
    {
      get => noOfEntriesSelected ??= new();
      set => noOfEntriesSelected = value;
    }

    /// <summary>
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public DateWorkArea MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
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
    /// A value of InitializedFipsTribAddress.
    /// </summary>
    [JsonPropertyName("initializedFipsTribAddress")]
    public FipsTribAddress InitializedFipsTribAddress
    {
      get => initializedFipsTribAddress ??= new();
      set => initializedFipsTribAddress = value;
    }

    /// <summary>
    /// A value of InitializedFips.
    /// </summary>
    [JsonPropertyName("initializedFips")]
    public Fips InitializedFips
    {
      get => initializedFips ??= new();
      set => initializedFips = value;
    }

    /// <summary>
    /// A value of InitializedTribunal.
    /// </summary>
    [JsonPropertyName("initializedTribunal")]
    public Tribunal InitializedTribunal
    {
      get => initializedTribunal ??= new();
      set => initializedTribunal = value;
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
    /// A value of ConcatResult.
    /// </summary>
    [JsonPropertyName("concatResult")]
    public WorkArea ConcatResult
    {
      get => concatResult ??= new();
      set => concatResult = value;
    }

    /// <summary>
    /// A value of ConcatDelimiter.
    /// </summary>
    [JsonPropertyName("concatDelimiter")]
    public WorkArea ConcatDelimiter
    {
      get => concatDelimiter ??= new();
      set => concatDelimiter = value;
    }

    /// <summary>
    /// A value of ConcatDelimLength.
    /// </summary>
    [JsonPropertyName("concatDelimLength")]
    public Common ConcatDelimLength
    {
      get => concatDelimLength ??= new();
      set => concatDelimLength = value;
    }

    /// <summary>
    /// Gets a value of Concat.
    /// </summary>
    [JsonIgnore]
    public Array<ConcatGroup> Concat => concat ??= new(ConcatGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Concat for json serialization.
    /// </summary>
    [JsonPropertyName("concat")]
    [Computed]
    public IList<ConcatGroup> Concat_Json
    {
      get => concat;
      set => Concat.Assign(value);
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
    /// A value of PaReferralAssignment.
    /// </summary>
    [JsonPropertyName("paReferralAssignment")]
    public PaReferralAssignment PaReferralAssignment
    {
      get => paReferralAssignment ??= new();
      set => paReferralAssignment = value;
    }

    /// <summary>
    /// A value of LegalReferralAssignment.
    /// </summary>
    [JsonPropertyName("legalReferralAssignment")]
    public LegalReferralAssignment LegalReferralAssignment
    {
      get => legalReferralAssignment ??= new();
      set => legalReferralAssignment = value;
    }

    /// <summary>
    /// A value of AdministrativeAppealAssignment.
    /// </summary>
    [JsonPropertyName("administrativeAppealAssignment")]
    public AdministrativeAppealAssignment AdministrativeAppealAssignment
    {
      get => administrativeAppealAssignment ??= new();
      set => administrativeAppealAssignment = value;
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
    /// Gets a value of Del.
    /// </summary>
    [JsonIgnore]
    public Array<DelGroup> Del => del ??= new(DelGroup.Capacity);

    /// <summary>
    /// Gets a value of Del for json serialization.
    /// </summary>
    [JsonPropertyName("del")]
    [Computed]
    public IList<DelGroup> Del_Json
    {
      get => del;
      set => Del.Assign(value);
    }

    /// <summary>
    /// A value of CaseUnitFunctionAssignmt.
    /// </summary>
    [JsonPropertyName("caseUnitFunctionAssignmt")]
    public CaseUnitFunctionAssignmt CaseUnitFunctionAssignmt
    {
      get => caseUnitFunctionAssignmt ??= new();
      set => caseUnitFunctionAssignmt = value;
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
    /// A value of MonitoredActivityAssignment.
    /// </summary>
    [JsonPropertyName("monitoredActivityAssignment")]
    public MonitoredActivityAssignment MonitoredActivityAssignment
    {
      get => monitoredActivityAssignment ??= new();
      set => monitoredActivityAssignment = value;
    }

    /// <summary>
    /// A value of NextTranInfo.
    /// </summary>
    [JsonPropertyName("nextTranInfo")]
    public NextTranInfo NextTranInfo
    {
      get => nextTranInfo ??= new();
      set => nextTranInfo = value;
    }

    /// <summary>
    /// A value of DelLocalSelectionMade.
    /// </summary>
    [JsonPropertyName("delLocalSelectionMade")]
    public Common DelLocalSelectionMade
    {
      get => delLocalSelectionMade ??= new();
      set => delLocalSelectionMade = value;
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
    /// A value of Event95.
    /// </summary>
    [JsonPropertyName("event95")]
    public Common Event95
    {
      get => event95 ??= new();
      set => event95 = value;
    }

    /// <summary>
    /// A value of ZdelFipsTribAddress.
    /// </summary>
    [JsonPropertyName("zdelFipsTribAddress")]
    public FipsTribAddress ZdelFipsTribAddress
    {
      get => zdelFipsTribAddress ??= new();
      set => zdelFipsTribAddress = value;
    }

    /// <summary>
    /// A value of ZdelFips.
    /// </summary>
    [JsonPropertyName("zdelFips")]
    public Fips ZdelFips
    {
      get => zdelFips ??= new();
      set => zdelFips = value;
    }

    private Infrastructure other;
    private Common findDoc;
    private Common prevDocsPrinted;
    private ServiceProvider existingServiceProvider;
    private OfficeServiceProvider existingOfficeServiceProvider;
    private Office existingOffice;
    private Common work;
    private LegalAction legalAction;
    private Infrastructure selectInfrastructure;
    private LegalAction initLegalAction;
    private Infrastructure compare;
    private WorkArea initWorkArea;
    private LegalReferralAssignment initLegalReferralAssignment;
    private LegalActionAssigment initLegalActionAssigment;
    private ServiceProvider initServiceProvider;
    private Office initOffice;
    private OfficeServiceProvider initOfficeServiceProvider;
    private Infrastructure saved;
    private Infrastructure lea;
    private Infrastructure lrf;
    private MonitoredActivity legalReferral;
    private InterstateCase icase;
    private InterstateCaseAssignment interstateCaseAssignment;
    private Common screenIdentification;
    private ServiceProvider serviceProvider;
    private Office office;
    private OfficeServiceProvider officeServiceProvider;
    private Common functionCount;
    private Common spCount;
    private Common reasonCount;
    private Common selResSpFuncInd;
    private OutgoingDocument outgoingDocument;
    private Infrastructure infrastructure;
    private Document document;
    private SpDocKey spDocKey;
    private Common delLocalLast;
    private Common sub;
    private Common rowCounter;
    private Common selCount;
    private DateWorkArea selectDateWorkArea;
    private ObligationAssignment obligationAssignment;
    private Common activeAssignmentFound;
    private DateWorkArea currentDate;
    private DateWorkArea delLocalQuery;
    private Common addCommand;
    private Common validCode;
    private Common noOfEntriesSelected;
    private DateWorkArea maxDate;
    private CodeValue codeValue;
    private Code code;
    private TextWorkArea textWorkArea;
    private FipsTribAddress initializedFipsTribAddress;
    private Fips initializedFips;
    private Tribunal initializedTribunal;
    private Tribunal tribunal;
    private WorkArea concatResult;
    private WorkArea concatDelimiter;
    private Common concatDelimLength;
    private Array<ConcatGroup> concat;
    private DateWorkArea nullDate;
    private PaReferralAssignment paReferralAssignment;
    private LegalReferralAssignment legalReferralAssignment;
    private AdministrativeAppealAssignment administrativeAppealAssignment;
    private LegalActionAssigment legalActionAssigment;
    private Array<DelGroup> del;
    private CaseUnitFunctionAssignmt caseUnitFunctionAssignmt;
    private CaseAssignment caseAssignment;
    private MonitoredActivityAssignment monitoredActivityAssignment;
    private NextTranInfo nextTranInfo;
    private Common delLocalSelectionMade;
    private DateWorkArea dateWorkArea;
    private Common event95;
    private FipsTribAddress zdelFipsTribAddress;
    private Fips zdelFips;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Other.
    /// </summary>
    [JsonPropertyName("other")]
    public LegalAction Other
    {
      get => other ??= new();
      set => other = value;
    }

    /// <summary>
    /// A value of AkeyOnlyMonitoredActivityAssignment.
    /// </summary>
    [JsonPropertyName("akeyOnlyMonitoredActivityAssignment")]
    public MonitoredActivityAssignment AkeyOnlyMonitoredActivityAssignment
    {
      get => akeyOnlyMonitoredActivityAssignment ??= new();
      set => akeyOnlyMonitoredActivityAssignment = value;
    }

    /// <summary>
    /// A value of AkeyOnlyMonitoredActivity.
    /// </summary>
    [JsonPropertyName("akeyOnlyMonitoredActivity")]
    public MonitoredActivity AkeyOnlyMonitoredActivity
    {
      get => akeyOnlyMonitoredActivity ??= new();
      set => akeyOnlyMonitoredActivity = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public MonitoredActivityAssignment New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of AkeyOnlyOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("akeyOnlyOfficeServiceProvider")]
    public OfficeServiceProvider AkeyOnlyOfficeServiceProvider
    {
      get => akeyOnlyOfficeServiceProvider ??= new();
      set => akeyOnlyOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of AkeyOnlyServiceProvider.
    /// </summary>
    [JsonPropertyName("akeyOnlyServiceProvider")]
    public ServiceProvider AkeyOnlyServiceProvider
    {
      get => akeyOnlyServiceProvider ??= new();
      set => akeyOnlyServiceProvider = value;
    }

    /// <summary>
    /// A value of AkeyOnlyOffice.
    /// </summary>
    [JsonPropertyName("akeyOnlyOffice")]
    public Office AkeyOnlyOffice
    {
      get => akeyOnlyOffice ??= new();
      set => akeyOnlyOffice = value;
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
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    /// <summary>
    /// A value of Foreign.
    /// </summary>
    [JsonPropertyName("foreign")]
    public FipsTribAddress Foreign
    {
      get => foreign ??= new();
      set => foreign = value;
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
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
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
    /// A value of AdministrativeAction.
    /// </summary>
    [JsonPropertyName("administrativeAction")]
    public AdministrativeAction AdministrativeAction
    {
      get => administrativeAction ??= new();
      set => administrativeAction = value;
    }

    /// <summary>
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
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
    /// A value of AdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("administrativeAppeal")]
    public AdministrativeAppeal AdministrativeAppeal
    {
      get => administrativeAppeal ??= new();
      set => administrativeAppeal = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of LegalReferral.
    /// </summary>
    [JsonPropertyName("legalReferral")]
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
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
    /// A value of MonitoredActivity.
    /// </summary>
    [JsonPropertyName("monitoredActivity")]
    public MonitoredActivity MonitoredActivity
    {
      get => monitoredActivity ??= new();
      set => monitoredActivity = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of CaseUnitFunctionAssignmt.
    /// </summary>
    [JsonPropertyName("caseUnitFunctionAssignmt")]
    public CaseUnitFunctionAssignmt CaseUnitFunctionAssignmt
    {
      get => caseUnitFunctionAssignmt ??= new();
      set => caseUnitFunctionAssignmt = value;
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
    /// A value of AdministrativeAppealAssignment.
    /// </summary>
    [JsonPropertyName("administrativeAppealAssignment")]
    public AdministrativeAppealAssignment AdministrativeAppealAssignment
    {
      get => administrativeAppealAssignment ??= new();
      set => administrativeAppealAssignment = value;
    }

    /// <summary>
    /// A value of ObligationAssignment.
    /// </summary>
    [JsonPropertyName("obligationAssignment")]
    public ObligationAssignment ObligationAssignment
    {
      get => obligationAssignment ??= new();
      set => obligationAssignment = value;
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
    /// A value of LegalReferralAssignment.
    /// </summary>
    [JsonPropertyName("legalReferralAssignment")]
    public LegalReferralAssignment LegalReferralAssignment
    {
      get => legalReferralAssignment ??= new();
      set => legalReferralAssignment = value;
    }

    /// <summary>
    /// A value of PaReferralAssignment.
    /// </summary>
    [JsonPropertyName("paReferralAssignment")]
    public PaReferralAssignment PaReferralAssignment
    {
      get => paReferralAssignment ??= new();
      set => paReferralAssignment = value;
    }

    /// <summary>
    /// A value of Old.
    /// </summary>
    [JsonPropertyName("old")]
    public MonitoredActivityAssignment Old
    {
      get => old ??= new();
      set => old = value;
    }

    /// <summary>
    /// A value of InterstateCaseAssignment.
    /// </summary>
    [JsonPropertyName("interstateCaseAssignment")]
    public InterstateCaseAssignment InterstateCaseAssignment
    {
      get => interstateCaseAssignment ??= new();
      set => interstateCaseAssignment = value;
    }

    /// <summary>
    /// A value of ExistingServiceProvider.
    /// </summary>
    [JsonPropertyName("existingServiceProvider")]
    public ServiceProvider ExistingServiceProvider
    {
      get => existingServiceProvider ??= new();
      set => existingServiceProvider = value;
    }

    /// <summary>
    /// A value of ExistingOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("existingOfficeServiceProvider")]
    public OfficeServiceProvider ExistingOfficeServiceProvider
    {
      get => existingOfficeServiceProvider ??= new();
      set => existingOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of ExistingOffice.
    /// </summary>
    [JsonPropertyName("existingOffice")]
    public Office ExistingOffice
    {
      get => existingOffice ??= new();
      set => existingOffice = value;
    }

    /// <summary>
    /// A value of Zdel.
    /// </summary>
    [JsonPropertyName("zdel")]
    public CsePerson Zdel
    {
      get => zdel ??= new();
      set => zdel = value;
    }

    private LegalAction other;
    private MonitoredActivityAssignment akeyOnlyMonitoredActivityAssignment;
    private MonitoredActivity akeyOnlyMonitoredActivity;
    private MonitoredActivityAssignment new1;
    private OfficeServiceProvider akeyOnlyOfficeServiceProvider;
    private ServiceProvider akeyOnlyServiceProvider;
    private Office akeyOnlyOffice;
    private CaseRole caseRole;
    private InterstateCase interstateCase;
    private FipsTribAddress foreign;
    private Fips fips;
    private Tribunal tribunal;
    private CsePerson csePerson;
    private AdministrativeAction administrativeAction;
    private CsePersonAccount csePersonAccount;
    private ObligationType obligationType;
    private AdministrativeAppeal administrativeAppeal;
    private Obligation obligation;
    private LegalAction legalAction;
    private LegalReferral legalReferral;
    private PaReferral paReferral;
    private MonitoredActivity monitoredActivity;
    private CaseUnit caseUnit;
    private Case1 case1;
    private Infrastructure infrastructure;
    private CaseUnitFunctionAssignmt caseUnitFunctionAssignmt;
    private CaseAssignment caseAssignment;
    private AdministrativeAppealAssignment administrativeAppealAssignment;
    private ObligationAssignment obligationAssignment;
    private LegalActionAssigment legalActionAssigment;
    private LegalReferralAssignment legalReferralAssignment;
    private PaReferralAssignment paReferralAssignment;
    private MonitoredActivityAssignment old;
    private InterstateCaseAssignment interstateCaseAssignment;
    private ServiceProvider existingServiceProvider;
    private OfficeServiceProvider existingOfficeServiceProvider;
    private Office existingOffice;
    private CsePerson zdel;
  }
#endregion
}
