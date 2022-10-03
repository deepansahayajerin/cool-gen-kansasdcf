// Program: OE_BKRP_BANKRUPTCY, ID: 372033622, model: 746.
// Short name: SWEBKRPP
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
/// A program: OE_BKRP_BANKRUPTCY.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This procedure step facilitates maintenance of Bankruptcy details of a CSE 
/// Person.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OeBkrpBankruptcy: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_BKRP_BANKRUPTCY program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeBkrpBankruptcy(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeBkrpBankruptcy.
  /// </summary>
  public OeBkrpBankruptcy(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------------
    // M A I N T E N A N C E   L O G
    //  Date    Developer       request #    Description
    // 03/22/95 Govindraj Kad	Development
    // 02/01/96 Alan Hackler   retro fits
    // 02/12/96 Sid Chowdhary	testing and rework
    // 06/05/96 Jack Rookard	add print functionality
    // 06/11/96 Konkader       complete print functionality
    // 06/26/96 Welborn	Add Left Pad EAB
    // 11/08/96 R. Marchman    Add new security and next tran
    // 12/11/96 Raju		raise events
    // 1/19/97  Konkader       modified Next Tran for HIST/MONA flow
    // 01/20/97 Raju	        put in code for NEXT TRAN added F9 return key
    // 03/25/97 Sid Chowdhary	IDCR # 324 changes for expected discharged date.
    // 05/27/97 M D Wheaton    Changed zero date methodology
    // 12/11/97 Jack Rookard   Remove control table access from Infrastructure 
    // processing.
    // 12/17/98 R.Jean         Correct exit states; add error exit states; 
    // retain bankruptcy after delete; don't allow key changes during scrolling;
    // edit for multiple prompts entered; display new attribute Dismiss/
    // Withdrawn date; allow deletes to occur only on day of creation; move
    // clear logic before move imports to exports to completely clear screen
    // 12/30/1998	M Ramirez	Changed security to check CRUD actions only.
    // 12/30/1998	M Ramirez	Revised print process.
    // 01/06/2000	M Ramirez	83300	NEXT TRAN needs to be cleared
    // 					before invoking print process
    // 04/24/2001      Madhu Kumar        Added edit checks for zip code both 4 
    // and 5 digits .
    // 02/25/2002	SWSRPRM		PR # 138709
    // Infrastructure detail record posting incorrect date for a
    // withdraw/dismissal.
    // Deleted commented code.
    // 05/25/2011 T. Pierce  CQ#27198  Do not allow specification of expected
    // discharge date and either a dis/with date or discharge date 
    // simultaneously.
    // -------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    UseSpDocSetLiterals();

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    // =============================================
    // The following three MOVE statements are
    // specifically moved to this place for this
    // screen (BKRP) since we dont want to clear
    // person number
    // =============================================
    export.Case1.Number = import.Case1.Number;
    export.CsePerson.Number = import.CsePerson.Number;
    export.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);

    if (!IsEmpty(export.Case1.Number))
    {
      local.TextWorkArea.Text10 = export.Case1.Number;
      UseEabPadLeftWithZeros();
      export.Case1.Number = local.TextWorkArea.Text10;
    }

    if (!IsEmpty(export.CsePerson.Number))
    {
      local.TextWorkArea.Text10 = export.CsePerson.Number;
      UseEabPadLeftWithZeros();
      export.CsePerson.Number = local.TextWorkArea.Text10;
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // ---------------------------------------------------------------
    // if the next tran info is not equal to spaces, this implies the
    // user requested a next tran action. now validate
    // ---------------------------------------------------------------
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      export.HiddenNextTranInfo.CsePersonNumber = export.CsePerson.Number;
      export.HiddenNextTranInfo.CsePersonNumberAp = export.CsePerson.Number;
      UseScCabNextTranPut1();

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

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    // =============================================
    // Next Tran and Security code ends here
    // =============================================
    local.HighlightError.Flag = "N";

    // ---------------------------------------------
    // Move the import views to export views.
    // ---------------------------------------------
    export.Bankruptcy.Assign(import.Bankruptcy);
    MoveBankruptcy2(import.UpdateStamp, export.UpdateStamp);
    MoveScrollingAttributes(import.ScrollingAttributes,
      export.ScrollingAttributes);
    export.ListAttorneyStates.PromptField =
      import.ListAttorneyStates.PromptField;
    export.ListBkrpDistCtStates.PromptField =
      import.ListBkrpDistCtStates.PromptField;
    export.ListBkrpTypes.PromptField = import.ListBkrpTypes.PromptField;
    export.ListCsePersons.PromptField = import.ListCsePersons.PromptField;
    export.ListTrusteeOffrStates.PromptField =
      import.ListTrusteeOffrStates.PromptField;
    export.HiddenCase.Number = import.HiddenCase.Number;
    export.HiddenPrevCsePerson.Number = import.HiddenPrevCsePerson.Number;
    export.HiddenPrevBankruptcy.Identifier =
      import.HiddenPrevBankruptcy.Identifier;
    export.HiddenPrevUserAction.Command = import.HiddenPrevUserAction.Command;
    export.HiddenDisplayPerformed.Flag = import.HiddenDisplaySuccessful.Flag;
    local.UserAction.Command = global.Command;

    // ---------------------------------------------
    // Code added by Raju  Dec 12, 1996:1100 hrs CST
    // Start of code
    // ---------------------------------------------
    export.HiddenLastRead.Assign(import.HiddenLastRead);

    // ---------------------------------------------
    // End of code
    // ---------------------------------------------
    if (!Equal(global.Command, "LIST") && !Equal(global.Command, "RETCOMP") && !
      Equal(global.Command, "RETNAME") && !Equal(global.Command, "RETCDVL"))
    {
      export.ListAttorneyStates.PromptField = "";
      export.ListBkrpDistCtStates.PromptField = "";
      export.ListBkrpTypes.PromptField = "";
      export.ListCsePersons.PromptField = "";
      export.ListTrusteeOffrStates.PromptField = "";
    }

    if (Equal(global.Command, "RETCOMP") || Equal(global.Command, "RETNAME"))
    {
      export.CsePerson.Number = import.CsePersonsWorkSet.Number;
      export.ListCsePersons.PromptField = "";
      global.Command = "DISPLAY";
      local.UserAction.Command = global.Command;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();

      if (!IsEmpty(export.HiddenNextTranInfo.CaseNumber))
      {
        export.Case1.Number = export.HiddenNextTranInfo.CaseNumber ?? Spaces
          (10);
      }

      if (!IsEmpty(export.HiddenNextTranInfo.CsePersonNumber))
      {
        export.CsePerson.Number = export.HiddenNextTranInfo.CsePersonNumber ?? Spaces
          (10);
      }
      else if (!IsEmpty(export.HiddenNextTranInfo.CsePersonNumberAp))
      {
        export.CsePerson.Number =
          export.HiddenNextTranInfo.CsePersonNumberAp ?? Spaces(10);
      }
      else if (!IsEmpty(export.HiddenNextTranInfo.CsePersonNumberObligor))
      {
        export.CsePerson.Number =
          export.HiddenNextTranInfo.CsePersonNumberObligor ?? Spaces(10);
      }

      // ---------------------------------------------
      // Start of Code (Raju 01/20/97:1035 hrs CST)
      // ---------------------------------------------
      if (Equal(export.HiddenNextTranInfo.LastTran, "SRPT") || Equal
        (export.HiddenNextTranInfo.LastTran, "SRPU"))
      {
        local.LastTran.SystemGeneratedIdentifier =
          export.HiddenNextTranInfo.InfrastructureId.GetValueOrDefault();
        UseOeCabReadInfrastructure();
        export.Case1.Number = local.LastTran.CaseNumber ?? Spaces(10);
        export.CsePerson.Number = local.LastTran.CsePersonNumber ?? Spaces(10);
        export.Bankruptcy.Identifier =
          (int)local.LastTran.DenormNumeric12.GetValueOrDefault();
      }

      // ------------
      // End  of Code
      // ------------
      export.CsePersonsWorkSet.Number = export.CsePerson.Number;
      global.Command = "DISPLAY";
      local.UserAction.Command = global.Command;
    }

    // mjr
    // ---------------------------------------------
    // 12/30/1998
    // Changed security to check CRUD actions only
    // ----------------------------------------------------------
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "CREATE") || Equal
      (global.Command, "UPDATE") || Equal(global.Command, "DELETE") || Equal
      (global.Command, "PRINT"))
    {
      // to validate action level security
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "RETURN":
        if (Equal(export.HiddenNextTranInfo.LastTran, "SRPT") || Equal
          (export.HiddenNextTranInfo.LastTran, "SRPU"))
        {
          global.NextTran = (export.HiddenNextTranInfo.LastTran ?? "") + " " + "XXNEXTXX"
            ;
        }
        else
        {
          ExitState = "ACO_NE0000_RETURN";
        }

        break;
      case "LIST":
        // ****************************************************************
        // * Test for prompt values being S or space.
        // ****************************************************************
        if (AsChar(export.ListCsePersons.PromptField) != 'S' && !
          IsEmpty(export.ListCsePersons.PromptField))
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.ListCsePersons, "promptField");

          field.Error = true;

          break;
        }

        if (AsChar(export.ListBkrpTypes.PromptField) != 'S' && !
          IsEmpty(export.ListBkrpTypes.PromptField))
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.ListBkrpTypes, "promptField");

          field.Error = true;

          break;
        }

        if (AsChar(export.ListBkrpDistCtStates.PromptField) != 'S' && !
          IsEmpty(export.ListBkrpDistCtStates.PromptField))
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.ListBkrpDistCtStates, "promptField");

          field.Error = true;

          break;
        }

        if (AsChar(export.ListTrusteeOffrStates.PromptField) != 'S' && !
          IsEmpty(export.ListTrusteeOffrStates.PromptField))
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.ListTrusteeOffrStates, "promptField");

          field.Error = true;

          break;
        }

        if (AsChar(export.ListAttorneyStates.PromptField) != 'S' && !
          IsEmpty(export.ListAttorneyStates.PromptField))
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.ListAttorneyStates, "promptField");

          field.Error = true;

          break;
        }

        // ****************************************************************
        // * Test for multiple prompts, must be 1 prompt at a time.
        // ****************************************************************
        if (AsChar(export.ListCsePersons.PromptField) == 'S')
        {
          ++local.Prompt.Count;
        }

        if (AsChar(export.ListBkrpTypes.PromptField) == 'S')
        {
          ++local.Prompt.Count;
        }

        if (AsChar(export.ListBkrpDistCtStates.PromptField) == 'S')
        {
          ++local.Prompt.Count;
        }

        if (AsChar(export.ListTrusteeOffrStates.PromptField) == 'S')
        {
          ++local.Prompt.Count;
        }

        if (AsChar(export.ListAttorneyStates.PromptField) == 'S')
        {
          ++local.Prompt.Count;
        }

        // ****************************************************************
        // * Determine the number of prompts; must be 1.
        // ****************************************************************
        if (local.Prompt.Count == 0)
        {
          var field1 = GetField(export.ListCsePersons, "promptField");

          field1.Error = true;

          var field2 = GetField(export.ListBkrpTypes, "promptField");

          field2.Error = true;

          var field3 = GetField(export.ListBkrpDistCtStates, "promptField");

          field3.Error = true;

          var field4 = GetField(export.ListTrusteeOffrStates, "promptField");

          field4.Error = true;

          var field5 = GetField(export.ListAttorneyStates, "promptField");

          field5.Error = true;

          ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";
        }
        else if (local.Prompt.Count > 1)
        {
          if (AsChar(export.ListCsePersons.PromptField) == 'S')
          {
            var field = GetField(export.ListCsePersons, "promptField");

            field.Error = true;
          }

          if (AsChar(export.ListBkrpTypes.PromptField) == 'S')
          {
            var field = GetField(export.ListBkrpTypes, "promptField");

            field.Error = true;
          }

          if (AsChar(export.ListBkrpDistCtStates.PromptField) == 'S')
          {
            var field = GetField(export.ListBkrpDistCtStates, "promptField");

            field.Error = true;
          }

          if (AsChar(export.ListTrusteeOffrStates.PromptField) == 'S')
          {
            var field = GetField(export.ListTrusteeOffrStates, "promptField");

            field.Error = true;
          }

          if (AsChar(export.ListAttorneyStates.PromptField) == 'S')
          {
            var field = GetField(export.ListAttorneyStates, "promptField");

            field.Error = true;
          }

          ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";
        }
        else
        {
          if (AsChar(export.ListCsePersons.PromptField) == 'S')
          {
            if (!IsEmpty(export.Case1.Number))
            {
              ExitState = "ECO_LNK_TO_CASE_COMPOSITION";
            }
            else
            {
              ExitState = "ECO_LNK_TO_CSE_NAME_LIST";
            }
          }

          if (AsChar(export.ListBkrpTypes.PromptField) == 'S')
          {
            export.Required.CodeName = "BANKRUPTCY TYPE";
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";
          }

          if (AsChar(export.ListBkrpDistCtStates.PromptField) == 'S')
          {
            export.Required.CodeName = "STATE CODE";
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";
          }

          if (AsChar(export.ListTrusteeOffrStates.PromptField) == 'S')
          {
            export.Required.CodeName = "STATE CODE";
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";
          }

          if (AsChar(export.ListAttorneyStates.PromptField) == 'S')
          {
            export.Required.CodeName = "STATE CODE";
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";
          }
        }

        break;
      case "RETCDVL":
        global.Command = "DISPLAY";

        if (AsChar(export.ListBkrpTypes.PromptField) == 'S')
        {
          export.ListBkrpTypes.PromptField = "";

          if (IsEmpty(import.Selected.Cdvalue))
          {
            var field = GetField(export.Bankruptcy, "bankruptcyType");

            field.Protected = false;
            field.Focused = true;
          }
          else
          {
            export.Bankruptcy.BankruptcyType = import.Selected.Cdvalue;

            var field = GetField(export.Bankruptcy, "bankruptcyCourtActionNo");

            field.Protected = false;
            field.Focused = true;
          }

          break;
        }

        if (AsChar(export.ListBkrpDistCtStates.PromptField) == 'S')
        {
          export.ListBkrpDistCtStates.PromptField = "";

          if (IsEmpty(import.Selected.Cdvalue))
          {
            var field = GetField(export.Bankruptcy, "bdcAddrState");

            field.Protected = false;
            field.Focused = true;
          }
          else
          {
            export.Bankruptcy.BdcAddrState = import.Selected.Cdvalue;

            var field = GetField(export.Bankruptcy, "bdcAddrZip5");

            field.Protected = false;
            field.Focused = true;
          }

          break;
        }

        if (AsChar(export.ListTrusteeOffrStates.PromptField) == 'S')
        {
          export.ListTrusteeOffrStates.PromptField = "";

          if (IsEmpty(import.Selected.Cdvalue))
          {
            var field = GetField(export.Bankruptcy, "btoAddrState");

            field.Protected = false;
            field.Focused = true;
          }
          else
          {
            export.Bankruptcy.BtoAddrState = import.Selected.Cdvalue;

            var field = GetField(export.Bankruptcy, "btoAddrZip5");

            field.Protected = false;
            field.Focused = true;
          }

          break;
        }

        if (AsChar(export.ListAttorneyStates.PromptField) == 'S')
        {
          export.ListAttorneyStates.PromptField = "";

          if (IsEmpty(import.Selected.Cdvalue))
          {
            var field = GetField(export.Bankruptcy, "apAttrAddrState");

            field.Protected = false;
            field.Focused = true;
          }
          else
          {
            export.Bankruptcy.ApAttrAddrState = import.Selected.Cdvalue;

            var field = GetField(export.Bankruptcy, "apAttrAddrZip5");

            field.Protected = false;
            field.Focused = true;
          }
        }

        break;
      case "DISPLAY":
        // mjr---> Command is process below
        break;
      case "NEXT":
        // mjr---> Command is process below
        break;
      case "PREV":
        // mjr---> Command is process below
        break;
      case "CREATE":
        // *****************************************************************
        // The five digit zip code entered must be validated to see that
        // it is not less than 5 digits.
        // *****************************************************************
        if (Length(TrimEnd(export.Bankruptcy.BdcAddrZip5)) > 0 && Length
          (TrimEnd(export.Bankruptcy.BdcAddrZip5)) < 5)
        {
          var field = GetField(export.Bankruptcy, "bdcAddrZip5");

          field.Error = true;

          ExitState = "SI_ZIP_CODE_MUST_HAVE_5_DIGITS";

          return;
        }

        if (Length(TrimEnd(export.Bankruptcy.BdcAddrZip5)) > 0 && Verify
          (export.Bankruptcy.BdcAddrZip5, "0123456789") != 0)
        {
          var field = GetField(export.Bankruptcy, "bdcAddrZip5");

          field.Error = true;

          ExitState = "SI0000_ZIP_CODE_MUST_BE_NUMERIC";

          return;
        }

        if (Length(TrimEnd(export.Bankruptcy.BdcAddrZip5)) == 0 && Length
          (TrimEnd(export.Bankruptcy.BdcAddrZip4)) > 0)
        {
          var field = GetField(export.Bankruptcy, "bdcAddrZip5");

          field.Error = true;

          ExitState = "SI0000_ZIP_5_DGT_REQ_4_DGTS_REQ";

          return;
        }

        if (Length(TrimEnd(export.Bankruptcy.BdcAddrZip5)) > 0 && Length
          (TrimEnd(export.Bankruptcy.BdcAddrZip4)) > 0)
        {
          if (Length(TrimEnd(export.Bankruptcy.BdcAddrZip4)) < 4)
          {
            var field = GetField(export.Bankruptcy, "bdcAddrZip4");

            field.Error = true;

            ExitState = "SI0000_ZIP_PLS_4_REQ_4_DGTS_OR_0";

            return;
          }
          else if (Verify(export.Bankruptcy.BdcAddrZip4, "0123456789") != 0)
          {
            var field = GetField(export.Bankruptcy, "bdcAddrZip4");

            field.Error = true;

            ExitState = "SI0000_ZIP_CODE_MUST_BE_NUMERIC";

            return;
          }
        }

        if (Length(TrimEnd(export.Bankruptcy.BtoAddrZip5)) > 0 && Length
          (TrimEnd(export.Bankruptcy.BtoAddrZip5)) < 5)
        {
          var field = GetField(export.Bankruptcy, "btoAddrZip5");

          field.Error = true;

          ExitState = "SI_ZIP_CODE_MUST_HAVE_5_DIGITS";

          return;
        }

        if (Length(TrimEnd(export.Bankruptcy.BtoAddrZip5)) > 0 && Verify
          (export.Bankruptcy.BtoAddrZip5, "0123456789") != 0)
        {
          var field = GetField(export.Bankruptcy, "btoAddrZip5");

          field.Error = true;

          ExitState = "SI0000_ZIP_CODE_MUST_BE_NUMERIC";

          return;
        }

        if (Length(TrimEnd(export.Bankruptcy.BtoAddrZip5)) == 0 && Length
          (TrimEnd(export.Bankruptcy.BtoAddrZip4)) > 0)
        {
          var field = GetField(export.Bankruptcy, "btoAddrZip5");

          field.Error = true;

          ExitState = "SI0000_ZIP_5_DGT_REQ_4_DGTS_REQ";

          return;
        }

        if (Length(TrimEnd(export.Bankruptcy.BtoAddrZip5)) > 0 && Length
          (TrimEnd(export.Bankruptcy.BtoAddrZip4)) > 0)
        {
          if (Length(TrimEnd(export.Bankruptcy.BtoAddrZip4)) < 4)
          {
            var field = GetField(export.Bankruptcy, "btoAddrZip4");

            field.Error = true;

            ExitState = "SI0000_ZIP_PLS_4_REQ_4_DGTS_OR_0";

            return;
          }
          else if (Verify(export.Bankruptcy.BtoAddrZip4, "0123456789") != 0)
          {
            var field = GetField(export.Bankruptcy, "btoAddrZip4");

            field.Error = true;

            ExitState = "SI0000_ZIP_CODE_MUST_BE_NUMERIC";

            return;
          }
        }

        if (Length(TrimEnd(export.Bankruptcy.ApAttrAddrZip5)) > 0 && Length
          (TrimEnd(export.Bankruptcy.ApAttrAddrZip5)) < 5)
        {
          var field = GetField(export.Bankruptcy, "apAttrAddrZip5");

          field.Error = true;

          ExitState = "SI_ZIP_CODE_MUST_HAVE_5_DIGITS";

          return;
        }

        if (Length(TrimEnd(export.Bankruptcy.ApAttrAddrZip5)) > 0 && Verify
          (export.Bankruptcy.ApAttrAddrZip5, "0123456789") != 0)
        {
          var field = GetField(export.Bankruptcy, "apAttrAddrZip5");

          field.Error = true;

          ExitState = "SI0000_ZIP_CODE_MUST_BE_NUMERIC";

          return;
        }

        if (Length(TrimEnd(export.Bankruptcy.ApAttrAddrZip5)) == 0 && Length
          (TrimEnd(export.Bankruptcy.ApAttrAddrZip4)) > 0)
        {
          var field = GetField(export.Bankruptcy, "apAttrAddrZip5");

          field.Error = true;

          ExitState = "SI0000_ZIP_5_DGT_REQ_4_DGTS_REQ";

          return;
        }

        if (Length(TrimEnd(export.Bankruptcy.ApAttrAddrZip5)) > 0 && Length
          (TrimEnd(export.Bankruptcy.ApAttrAddrZip4)) > 0)
        {
          if (Length(TrimEnd(export.Bankruptcy.ApAttrAddrZip4)) < 4)
          {
            var field = GetField(export.Bankruptcy, "apAttrAddrZip4");

            field.Error = true;

            ExitState = "SI0000_ZIP_PLS_4_REQ_4_DGTS_OR_0";

            return;
          }
          else if (Verify(export.Bankruptcy.ApAttrAddrZip4, "0123456789") != 0)
          {
            var field = GetField(export.Bankruptcy, "apAttrAddrZip4");

            field.Error = true;

            ExitState = "SI0000_ZIP_CODE_MUST_BE_NUMERIC";

            return;
          }
        }

        export.HiddenDisplayPerformed.Flag = "N";
        UseOeBkrpValidateBankruptcy();

        if (local.LastErrorEntryNo.Count == 0)
        {
          // ------------------------------------------------------------
          // The following exit state statement is required to clear any
          // adabas error exit state set by adabas external.
          // ------------------------------------------------------------
          ExitState = "ACO_NN0000_ALL_OK";
          UseOeBkrpCreateBankruptcy();
          export.HiddenLastRead.BankruptcyConfirmationDate =
            local.ZeroDate.Date;
          export.HiddenLastRead.BankruptcyDischargeDate = local.ZeroDate.Date;
          export.HiddenLastRead.BankruptcyFilingDate = local.ZeroDate.Date;
          export.HiddenDisplayPerformed.Flag = "Y";

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
          }
        }
        else
        {
          // ---------------------------------------------------------
          // One or more errors encountered.  Errors highlighted below
          // ---------------------------------------------------------
          local.HighlightError.Flag = "Y";
        }

        break;
      case "UPDATE":
        // -----------------------------------------------------
        // Check that the user has displayed the record before
        // attempting to update and has not changed the key data.
        // -----------------------------------------------------
        // *****************************************************************
        // The five digit zip code entered must be validated to see that
        // it is not less than 5 digits.
        // *****************************************************************
        if (Length(TrimEnd(export.Bankruptcy.BdcAddrZip5)) > 0 && Length
          (TrimEnd(export.Bankruptcy.BdcAddrZip5)) < 5)
        {
          var field = GetField(export.Bankruptcy, "bdcAddrZip5");

          field.Error = true;

          ExitState = "SI_ZIP_CODE_MUST_HAVE_5_DIGITS";

          return;
        }

        if (Length(TrimEnd(export.Bankruptcy.BdcAddrZip5)) > 0 && Verify
          (export.Bankruptcy.BdcAddrZip5, "0123456789") != 0)
        {
          var field = GetField(export.Bankruptcy, "bdcAddrZip5");

          field.Error = true;

          ExitState = "SI0000_ZIP_CODE_MUST_BE_NUMERIC";

          return;
        }

        if (Length(TrimEnd(export.Bankruptcy.BdcAddrZip5)) == 0 && Length
          (TrimEnd(export.Bankruptcy.BdcAddrZip4)) > 0)
        {
          var field = GetField(export.Bankruptcy, "bdcAddrZip5");

          field.Error = true;

          ExitState = "SI0000_ZIP_5_DGT_REQ_4_DGTS_REQ";

          return;
        }

        if (Length(TrimEnd(export.Bankruptcy.BdcAddrZip5)) > 0 && Length
          (TrimEnd(export.Bankruptcy.BdcAddrZip4)) > 0)
        {
          if (Length(TrimEnd(export.Bankruptcy.BdcAddrZip4)) < 4)
          {
            var field = GetField(export.Bankruptcy, "bdcAddrZip4");

            field.Error = true;

            ExitState = "SI0000_ZIP_PLS_4_REQ_4_DGTS_OR_0";

            return;
          }
          else if (Verify(export.Bankruptcy.BdcAddrZip4, "0123456789") != 0)
          {
            var field = GetField(export.Bankruptcy, "bdcAddrZip4");

            field.Error = true;

            ExitState = "SI0000_ZIP_CODE_MUST_BE_NUMERIC";

            return;
          }
        }

        if (Length(TrimEnd(export.Bankruptcy.BtoAddrZip5)) > 0 && Length
          (TrimEnd(export.Bankruptcy.BtoAddrZip5)) < 5)
        {
          var field = GetField(export.Bankruptcy, "btoAddrZip5");

          field.Error = true;

          ExitState = "SI_ZIP_CODE_MUST_HAVE_5_DIGITS";

          return;
        }

        if (Length(TrimEnd(export.Bankruptcy.BtoAddrZip5)) > 0 && Verify
          (export.Bankruptcy.BtoAddrZip5, "0123456789") != 0)
        {
          var field = GetField(export.Bankruptcy, "btoAddrZip5");

          field.Error = true;

          ExitState = "SI0000_ZIP_CODE_MUST_BE_NUMERIC";

          return;
        }

        if (Length(TrimEnd(export.Bankruptcy.BtoAddrZip5)) == 0 && Length
          (TrimEnd(export.Bankruptcy.BtoAddrZip4)) > 0)
        {
          var field = GetField(export.Bankruptcy, "btoAddrZip5");

          field.Error = true;

          ExitState = "SI0000_ZIP_5_DGT_REQ_4_DGTS_REQ";

          return;
        }

        if (Length(TrimEnd(export.Bankruptcy.BtoAddrZip5)) > 0 && Length
          (TrimEnd(export.Bankruptcy.BtoAddrZip4)) > 0)
        {
          if (Length(TrimEnd(export.Bankruptcy.BtoAddrZip4)) < 4)
          {
            var field = GetField(export.Bankruptcy, "btoAddrZip4");

            field.Error = true;

            ExitState = "SI0000_ZIP_PLS_4_REQ_4_DGTS_OR_0";

            return;
          }
          else if (Verify(export.Bankruptcy.BtoAddrZip4, "0123456789") != 0)
          {
            var field = GetField(export.Bankruptcy, "btoAddrZip4");

            field.Error = true;

            ExitState = "SI0000_ZIP_CODE_MUST_BE_NUMERIC";

            return;
          }
        }

        if (Length(TrimEnd(export.Bankruptcy.ApAttrAddrZip5)) > 0 && Length
          (TrimEnd(export.Bankruptcy.ApAttrAddrZip5)) < 5)
        {
          var field = GetField(export.Bankruptcy, "apAttrAddrZip5");

          field.Error = true;

          ExitState = "SI_ZIP_CODE_MUST_HAVE_5_DIGITS";

          return;
        }

        if (Length(TrimEnd(export.Bankruptcy.ApAttrAddrZip5)) > 0 && Verify
          (export.Bankruptcy.ApAttrAddrZip5, "0123456789") != 0)
        {
          var field = GetField(export.Bankruptcy, "apAttrAddrZip5");

          field.Error = true;

          ExitState = "SI0000_ZIP_CODE_MUST_BE_NUMERIC";

          return;
        }

        if (Length(TrimEnd(export.Bankruptcy.ApAttrAddrZip5)) == 0 && Length
          (TrimEnd(export.Bankruptcy.ApAttrAddrZip4)) > 0)
        {
          var field = GetField(export.Bankruptcy, "apAttrAddrZip5");

          field.Error = true;

          ExitState = "SI0000_ZIP_5_DGT_REQ_4_DGTS_REQ";

          return;
        }

        if (Length(TrimEnd(export.Bankruptcy.ApAttrAddrZip5)) > 0 && Length
          (TrimEnd(export.Bankruptcy.ApAttrAddrZip4)) > 0)
        {
          if (Length(TrimEnd(export.Bankruptcy.ApAttrAddrZip4)) < 4)
          {
            var field = GetField(export.Bankruptcy, "apAttrAddrZip4");

            field.Error = true;

            ExitState = "SI0000_ZIP_PLS_4_REQ_4_DGTS_OR_0";

            return;
          }
          else if (Verify(export.Bankruptcy.ApAttrAddrZip4, "0123456789") != 0)
          {
            var field = GetField(export.Bankruptcy, "apAttrAddrZip4");

            field.Error = true;

            ExitState = "SI0000_ZIP_CODE_MUST_BE_NUMERIC";

            return;
          }
        }

        if (!Equal(export.CsePerson.Number, export.HiddenPrevCsePerson.Number) ||
          export.Bankruptcy.Identifier != export
          .HiddenPrevBankruptcy.Identifier || !
          Equal(export.Case1.Number, export.HiddenCase.Number) || !
          Equal(import.HiddenPrevUserAction.Command, "DISPLAY") && AsChar
          (import.HiddenDisplaySuccessful.Flag) != 'Y' && !
          Equal(import.HiddenPrevUserAction.Command, "UPDATE") && !
          Equal(import.HiddenPrevUserAction.Command, "CREATE") && !
          Equal(import.HiddenPrevUserAction.Command, "PREV") && !
          Equal(import.HiddenPrevUserAction.Command, "NEXT"))
        {
          ExitState = "CO0000_DISPLAY_BEF_UPD_OR_DEL";

          break;
        }

        UseOeBkrpValidateBankruptcy();

        if (local.LastErrorEntryNo.Count == 0)
        {
          // ------------------------------------------------------------
          // The following exit state statement is required to clear any
          // adabas error exit state set by adabas external.
          // ------------------------------------------------------------
          ExitState = "ACO_NN0000_ALL_OK";
          UseOeBkrpUpdateBankruptcy();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
          }
        }
        else
        {
          // ---------------------------------------------------------
          // One or more errors encountered.  Errors highlighted below.
          // ---------------------------------------------------------
          local.HighlightError.Flag = "Y";

          // ------------------------------------------------------
          // local_transaction_failed ief_supplied flag is not set
          // deliberately so that UPDATE can be initiated without
          // DISPLAY again.
          // ------------------------------------------------------
        }

        break;
      case "DELETE":
        // -----------------------------------------------------
        // Check that the user has displayed the record before
        // attempting to delete and has not changed the key data.
        // -----------------------------------------------------
        if (!Equal(export.CsePerson.Number, export.HiddenPrevCsePerson.Number) ||
          export.Bankruptcy.Identifier != export
          .HiddenPrevBankruptcy.Identifier || !
          Equal(export.Case1.Number, export.HiddenCase.Number) || !
          Equal(import.HiddenPrevUserAction.Command, "DISPLAY") && AsChar
          (import.HiddenDisplaySuccessful.Flag) != 'Y' && !
          Equal(import.HiddenPrevUserAction.Command, "PREV") && !
          Equal(import.HiddenPrevUserAction.Command, "NEXT"))
        {
          ExitState = "CO0000_DISPLAY_BEF_UPD_OR_DEL";
          export.HiddenDisplayPerformed.Flag = "N";

          break;
        }

        export.HiddenDisplayPerformed.Flag = "N";
        UseOeBkrpValidateBankruptcy();

        if (local.LastErrorEntryNo.Count == 0)
        {
          // -----------------------------------------------------------
          // The following exit state statement is required to clear any
          // adabas error exit state set by adabas external.
          // -----------------------------------------------------------
          ExitState = "ACO_NN0000_ALL_OK";
          UseOeBkrpDeleteBankruptcy();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.Bankruptcy.Assign(local.InitialisedToBlanks);
            MoveBankruptcy2(local.InitialisedToBlanks, export.UpdateStamp);
            export.HiddenLastRead.BankruptcyConfirmationDate =
              local.ZeroDate.Date;
            export.HiddenLastRead.BankruptcyDischargeDate = local.ZeroDate.Date;
            export.HiddenLastRead.BankruptcyFilingDate = local.ZeroDate.Date;
            ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";
          }
        }
        else
        {
          // ----------------------------------------------------------
          // One or more errors encountered.  Errors highlighted below.
          // ----------------------------------------------------------
          local.HighlightError.Flag = "Y";
        }

        break;
      case "PRINT":
        if (AsChar(export.HiddenDisplayPerformed.Flag) != 'Y')
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_PRINT";

          return;
        }

        local.Document.Name = "BANKRPTC";

        // mjr
        // ------------------------------------------
        // 01/06/2000
        // NEXT TRAN needs to be cleared before invoking print process
        // -------------------------------------------------------
        export.HiddenNextTranInfo.Assign(local.Null1);
        export.Standard.NextTransaction = "DKEY";
        export.HiddenNextTranInfo.MiscText2 =
          TrimEnd(local.SpDocLiteral.IdDocument) + local.Document.Name;

        // mjr
        // ----------------------------------------------------
        // Place identifiers into next tran
        // -------------------------------------------------------
        export.HiddenNextTranInfo.MiscText1 =
          TrimEnd(local.SpDocLiteral.IdPrNumber) + export.CsePerson.Number;
        local.Print.Text50 = TrimEnd(local.SpDocLiteral.IdBankruptcy) + NumberToString
          (export.Bankruptcy.Identifier, 13, 3);
        export.HiddenNextTranInfo.MiscText1 =
          TrimEnd(export.HiddenNextTranInfo.MiscText1) + ";" + local
          .Print.Text50;
        UseScCabNextTranPut2();

        // --------------------------------------------------
        // mjr---> DKEY's trancode = SRPD
        // Can change this to do a read instead of hardcoding
        // --------------------------------------------------
        global.NextTran = "SRPD PRINT";

        return;
      case "PRINTRET":
        // mjr
        // -----------------------------------------------
        // 12/30/1998
        // After the document is Printed (the user may still be looking
        // at WordPerfect), control is returned here.  Any cleanup
        // processing which is necessary after a print, should be done
        // now.
        // ------------------------------------------------------------
        UseScCabNextTranGet();

        // mjr
        // ----------------------------------------------------
        // Extract identifiers from next tran
        // -------------------------------------------------------
        local.Position.Count =
          Find(String(
            export.HiddenNextTranInfo.MiscText1,
          NextTranInfo.MiscText1_MaxLength),
          TrimEnd(local.SpDocLiteral.IdPrNumber));

        if (local.Position.Count <= 0)
        {
          break;
        }

        export.CsePerson.Number =
          Substring(export.HiddenNextTranInfo.MiscText1, local.Position.Count +
          7, 10);
        local.Position.Count =
          Find(String(
            export.HiddenNextTranInfo.MiscText1,
          NextTranInfo.MiscText1_MaxLength),
          TrimEnd(local.SpDocLiteral.IdBankruptcy));

        if (local.Position.Count <= 0)
        {
          break;
        }

        local.BatchConvertNumToText.Number15 =
          StringToNumber(Substring(
            export.HiddenNextTranInfo.MiscText1, 50, local.Position.Count +
          5, 3));
        export.Bankruptcy.Identifier =
          (int)local.BatchConvertNumToText.Number15;
        global.Command = "DISPLAY";
        local.UserAction.Command = global.Command;

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

    // mjr
    // -----------------------------------------------
    // 12/30/1998
    // Moved Display, Prev and Next here for return from Print
    // ------------------------------------------------------------
    // ---------------------------------------------
    // Command DISPLAY displays given Bankruptcy rec
    // Command PREV displays previous bankruptcy rec
    // Command NEXT displays next bankruptcy rec
    // ---------------------------------------------
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "PREV") || Equal
      (global.Command, "NEXT"))
    {
      // -----------------------------------------------------
      // Check that the user has displayed the record before
      // attempting to update and has not changed the key data.
      // -----------------------------------------------------
      if (Equal(global.Command, "NEXT") || Equal(global.Command, "PREV"))
      {
        if (!Equal(export.CsePerson.Number, export.HiddenPrevCsePerson.Number) ||
          export.Bankruptcy.Identifier != export
          .HiddenPrevBankruptcy.Identifier || !
          Equal(export.Case1.Number, export.HiddenCase.Number))
        {
          ExitState = "OE0000_KEY_CHANGE_NA";

          goto Test;
        }
      }

      UseOeBkrpDisplayBankruptcy();

      if (AsChar(local.BankruptcyDisplayed.Flag) == 'Y')
      {
        // mjr
        // -----------------------------------------------
        // 12/30/1998
        // Added check for an exitstate returned from Print
        // ------------------------------------------------------------
        local.Position.Count =
          Find(String(
            export.HiddenNextTranInfo.MiscText2,
          NextTranInfo.MiscText2_MaxLength),
          TrimEnd(local.SpDocLiteral.IdDocument));

        if (local.Position.Count <= 0)
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }
        else
        {
          // mjr---> Determines the appropriate exitstate for the Print process
          local.Print.Text50 = export.HiddenNextTranInfo.MiscText2 ?? Spaces
            (50);
          UseSpPrintDecodeReturnCode();
          export.HiddenNextTranInfo.MiscText2 = local.Print.Text50;
        }

        export.HiddenDisplayPerformed.Flag = "Y";

        // ---------------------------------------------
        // Code added by Raju  Dec 12, 1996:1100 hrs CST
        // Start of code
        // ---------------------------------------------
        export.HiddenLastRead.BankruptcyDischargeDate =
          export.Bankruptcy.BankruptcyDischargeDate;
        export.HiddenLastRead.BankruptcyFilingDate =
          export.Bankruptcy.BankruptcyFilingDate;
        export.HiddenLastRead.BankruptcyConfirmationDate =
          export.Bankruptcy.BankruptcyConfirmationDate;

        // ---------------------------------------------
        // End of code
        // ---------------------------------------------
      }
      else
      {
        export.HiddenDisplayPerformed.Flag = "N";

        if (IsExitState("CASE_NF"))
        {
          var field = GetField(export.Case1, "number");

          field.Error = true;

          // CQ#27198 05/25/2011  T. Pierce  Clear screen when this error 
          // occurs.
          MoveBankruptcy1(local.EmptyBankruptcy, export.Bankruptcy);
          MoveBankruptcy2(local.EmptyUpdateStamp, export.UpdateStamp);
        }
        else if (IsExitState("CSE_PERSON_NO_REQUIRED"))
        {
          export.Bankruptcy.Identifier = 0;

          var field = GetField(export.CsePerson, "number");

          field.Error = true;

          // CQ#27198 05/25/2011  T. Pierce  Clear screen when this error 
          // occurs.
          export.Case1.Number = local.EmptyCase.Number;
          MoveBankruptcy1(local.EmptyBankruptcy, export.Bankruptcy);
          MoveBankruptcy2(local.EmptyUpdateStamp, export.UpdateStamp);
        }
        else if (IsExitState("CSE_PERSON_NF"))
        {
          export.Bankruptcy.Identifier = 0;

          var field = GetField(export.CsePerson, "number");

          field.Error = true;

          // CQ#27198 05/25/2011  T. Pierce  Clear screen when this error 
          // occurs.
          export.Case1.Number = local.EmptyCase.Number;
          MoveBankruptcy1(local.EmptyBankruptcy, export.Bankruptcy);
          MoveBankruptcy2(local.EmptyUpdateStamp, export.UpdateStamp);
        }
        else if (IsExitState("BANKRUPTCY_NF"))
        {
          var field1 = GetField(export.CsePerson, "number");

          field1.Error = true;

          var field2 = GetField(export.Bankruptcy, "identifier");

          field2.Error = true;

          // CQ#27198 05/25/2011  T. Pierce  Clear screen when this error 
          // occurs.
          export.Case1.Number = local.EmptyCase.Number;
          MoveBankruptcy1(local.EmptyBankruptcy, export.Bankruptcy);
          MoveBankruptcy2(local.EmptyUpdateStamp, export.UpdateStamp);
        }
        else if (IsExitState("CO0000_NO_BANKRUPTCY_TO_DISPLAY"))
        {
        }
        else if (IsExitState("ACO_NE0000_INVALID_BACKWARD"))
        {
          // CQ#27198 05/25/2011  T. Pierce Change exit state when scrolling 
          // backward beyond first record.
        }
        else if (IsExitState("ACO_NE0000_INVALID_FORWARD"))
        {
          // CQ#27198 05/25/2011  T. Pierce Change exit state when scrolling 
          // forward beyond last record.
        }
        else if (IsExitState("OE0000_CASE_CSE_PERSON_NF"))
        {
          // CQ#27198 05/25/2011  T. Pierce  Clear screen when this error 
          // occurs.
          MoveBankruptcy1(local.EmptyBankruptcy, export.Bankruptcy);
          MoveBankruptcy2(local.EmptyUpdateStamp, export.UpdateStamp);

          var field = GetField(export.CsePerson, "number");

          field.Error = true;
        }
        else
        {
          var field = GetField(export.CsePerson, "number");

          field.Error = true;
        }
      }
    }

Test:

    // ------------------------------------------------------------
    // Code added by Raju  Dec 12, 1996:1000 hrs CST
    // The oe cab raise event will be called from here case of add /
    // update
    // Start of code
    // ------------------------------------------------------------
    // -------------------------------------------------------
    // added
    // . local view infrastructure
    //   - event id
    //   - reason code
    // . import , export hidden last read
    //           bankruptcy
    //   - filing date
    //   - discharge date
    // . local raise event flag work area
    //   - text1
    // This will be set/assigned for each event raised
    // -------------------------------------------------------
    if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD") || IsExitState
      ("ACO_NI0000_SUCCESSFUL_UPDATE"))
    {
      local.Infrastructure.UserId = "BKRP";
      local.Infrastructure.EventId = 4;
      local.Infrastructure.BusinessObjectCd = "BKR";
      local.Infrastructure.DenormNumeric12 = export.Bankruptcy.Identifier;
      local.Infrastructure.SituationNumber = 0;

      // -------------------------
      // formation of detail line
      // -------------------------
      // ----------------------------------------------------------
      // a detail line of 75c has to be formatted
      //   as follows
      // 'Bankruptcy Identifier:999, Confirmation Date : MMDDCCYY'
      // 'Bankruptcy Identifier:999, Discharged Date : MMDDCCYY'
      // 'Bankruptcy Identifier:999, Filed Date : MMDDCCYY'
      // ----------------------------------------------------------
      for(local.NumberOfEvents.TotalInteger = 1; local
        .NumberOfEvents.TotalInteger <= 4; ++local.NumberOfEvents.TotalInteger)
      {
        local.Infrastructure.Detail = Spaces(Infrastructure.Detail_MaxLength);
        local.DetailText30.Text30 = "Bankruptcy Identifier:";
        local.DetailText10.Text10 =
          NumberToString(export.Bankruptcy.Identifier, 13, 3);
        local.Infrastructure.Detail = TrimEnd(local.DetailText30.Text30) + local
          .DetailText10.Text10;
        local.RaiseEventFlag.Text1 = "N";

        if (local.NumberOfEvents.TotalInteger == 1)
        {
          if (!Equal(export.Bankruptcy.BankruptcyDischargeDate,
            local.ZeroDate.Date))
          {
            if (!Equal(export.HiddenLastRead.BankruptcyDischargeDate,
              export.Bankruptcy.BankruptcyDischargeDate))
            {
              local.RaiseEventFlag.Text1 = "Y";
              local.Infrastructure.ReasonCode = "BKRPDISCHRG";
              local.Infrastructure.ReferenceDate =
                export.Bankruptcy.BankruptcyDischargeDate;
              local.Date.Date = local.Infrastructure.ReferenceDate;
              local.DetailText25.Text25 = ", Discharged Date:";
              local.Infrastructure.Detail =
                TrimEnd(local.Infrastructure.Detail) + local
                .DetailText25.Text25;
              local.DetailText10.Text10 = UseCabConvertDate2String();
              local.Infrastructure.Detail =
                TrimEnd(local.Infrastructure.Detail) + local
                .DetailText10.Text10;
            }
          }
        }
        else if (local.NumberOfEvents.TotalInteger == 2)
        {
          if (!Equal(export.Bankruptcy.BankruptcyFilingDate, local.ZeroDate.Date))
            
          {
            if (!Equal(export.HiddenLastRead.BankruptcyFilingDate,
              export.Bankruptcy.BankruptcyFilingDate))
            {
              if (Equal(export.Bankruptcy.BankruptcyType, "13"))
              {
                local.Infrastructure.ReasonCode = "BKRPCHAPTER13";
              }
              else if (Equal(export.Bankruptcy.BankruptcyType, "07"))
              {
                local.Infrastructure.ReasonCode = "BKRPCHAPTER7";
              }

              local.RaiseEventFlag.Text1 = "Y";
              local.Infrastructure.ReferenceDate =
                export.Bankruptcy.BankruptcyFilingDate;
              local.Date.Date = local.Infrastructure.ReferenceDate;
              local.DetailText25.Text25 = ", Filed Date:";
              local.Infrastructure.Detail =
                TrimEnd(local.Infrastructure.Detail) + local
                .DetailText25.Text25;
              local.DetailText10.Text10 = UseCabConvertDate2String();
              local.Infrastructure.Detail =
                TrimEnd(local.Infrastructure.Detail) + local
                .DetailText10.Text10;
            }
          }
        }
        else if (local.NumberOfEvents.TotalInteger == 3)
        {
          if (!Equal(export.Bankruptcy.BankruptcyConfirmationDate,
            local.ZeroDate.Date))
          {
            if (!Equal(export.HiddenLastRead.BankruptcyConfirmationDate,
              export.Bankruptcy.BankruptcyConfirmationDate))
            {
              local.RaiseEventFlag.Text1 = "Y";
              local.Infrastructure.ReasonCode = "BKRPCONFIRM";
              local.Infrastructure.ReferenceDate =
                export.Bankruptcy.BankruptcyConfirmationDate;
              local.Date.Date = local.Infrastructure.ReferenceDate;
              local.DetailText25.Text25 = ", Confirmation Date:";
              local.Infrastructure.Detail =
                TrimEnd(local.Infrastructure.Detail) + local
                .DetailText25.Text25;
              local.DetailText10.Text10 = UseCabConvertDate2String();
              local.Infrastructure.Detail =
                TrimEnd(local.Infrastructure.Detail) + local
                .DetailText10.Text10;
            }
          }
        }
        else if (local.NumberOfEvents.TotalInteger == 4)
        {
          if (!Equal(export.Bankruptcy.BankruptcyDismissWithdrawDate,
            local.ZeroDate.Date))
          {
            if (!Equal(export.HiddenLastRead.BankruptcyDismissWithdrawDate,
              export.Bankruptcy.BankruptcyDismissWithdrawDate))
            {
              if (Equal(export.Bankruptcy.BankruptcyType, "13"))
              {
                local.Infrastructure.ReasonCode = "BKRPDISM/WITH13";
              }
              else if (Equal(export.Bankruptcy.BankruptcyType, "07"))
              {
                local.Infrastructure.ReasonCode = "BKRPDISM/WITH7";
              }

              local.RaiseEventFlag.Text1 = "Y";
              local.Infrastructure.ReferenceDate =
                export.Bankruptcy.BankruptcyDismissWithdrawDate;
              local.Date.Date = local.Infrastructure.ReferenceDate;
              local.DetailText25.Text25 = ", Dismiss/Withdraw Date:";
              local.Infrastructure.Detail =
                TrimEnd(local.Infrastructure.Detail) + local
                .DetailText25.Text25;
              local.DetailText10.Text10 = UseCabConvertDate2String();
              local.Infrastructure.Detail =
                TrimEnd(local.Infrastructure.Detail) + local
                .DetailText10.Text10;
            }
          }
        }
        else
        {
          local.RaiseEventFlag.Text1 = "N";
        }

        if (AsChar(local.RaiseEventFlag.Text1) == 'Y')
        {
          UseOeCabRaiseEvent();

          if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD") || IsExitState
            ("ACO_NI0000_SUCCESSFUL_UPDATE"))
          {
          }
          else
          {
            UseEabRollbackCics();

            return;
          }
        }
      }

      export.HiddenLastRead.BankruptcyDischargeDate =
        export.Bankruptcy.BankruptcyDischargeDate;
      export.HiddenLastRead.BankruptcyFilingDate =
        export.Bankruptcy.BankruptcyFilingDate;
      export.HiddenLastRead.BankruptcyConfirmationDate =
        export.Bankruptcy.BankruptcyConfirmationDate;
    }

    // ---------------------------------------------
    // End of code
    // ---------------------------------------------
    // ----------------------------------------------------------
    // Save the current views and command in hidden views for
    // checking in the next pass
    // ----------------------------------------------------------
    export.HiddenCase.Number = export.Case1.Number;
    export.HiddenPrevCsePerson.Number = export.CsePerson.Number;
    export.HiddenPrevBankruptcy.Identifier = export.Bankruptcy.Identifier;

    if (AsChar(export.HiddenDisplayPerformed.Flag) == 'N')
    {
      export.HiddenPrevUserAction.Command = "INVALID";
    }
    else
    {
      export.HiddenPrevUserAction.Command = global.Command;
    }

    // ---------------------------------------------
    // If any validation error encountered, highlight the errors encountered.
    // ---------------------------------------------
    if (AsChar(local.HighlightError.Flag) == 'Y')
    {
      local.Errors.Index = local.LastErrorEntryNo.Count - 1;
      local.Errors.CheckSize();

      while(local.Errors.Index >= 0)
      {
        switch(local.Errors.Item.DetailErrorCode.Count)
        {
          case 1:
            var field1 = GetField(export.CsePerson, "number");

            field1.Error = true;

            ExitState = "CSE_PERSON_NF";

            break;
          case 2:
            var field2 = GetField(export.CsePerson, "number");

            field2.Error = true;

            ExitState = "BANKRUPTCY_NF";

            break;
          case 3:
            var field3 = GetField(export.Bankruptcy, "bankruptcyType");

            field3.Error = true;

            ExitState = "OE0081_INVALID_BANKRUPTCY_TYPE";

            break;
          case 4:
            var field4 = GetField(export.Bankruptcy, "bankruptcyCourtActionNo");

            field4.Error = true;

            ExitState = "OE0082_INVALID_BNKRTCY_CASE_NO";

            break;
          case 5:
            var field5 = GetField(export.Bankruptcy, "bankruptcyDistrictCourt");

            field5.Error = true;

            ExitState = "OE0083_INVALID_BNKRPTCY_COURT";

            break;
          case 6:
            var field6 = GetField(export.Bankruptcy, "bdcAddrStreet1");

            field6.Error = true;

            ExitState = "OE0084_BDC_ADDR_REQUIRED";

            break;
          case 7:
            var field7 = GetField(export.Bankruptcy, "bdcAddrCity");

            field7.Error = true;

            ExitState = "OE0085_BDC_CITY_REQD";

            break;
          case 8:
            var field8 = GetField(export.Bankruptcy, "bdcAddrState");

            field8.Error = true;

            ExitState = "OE0000_INVALID_BKRP_DIST_CT_ST";

            break;
          case 9:
            var field9 = GetField(export.Bankruptcy, "trusteeLastName");

            field9.Error = true;

            ExitState = "OE0087_TRUSTEE_LAST_REQD";

            break;
          case 10:
            var field10 = GetField(export.Bankruptcy, "trusteeFirstName");

            field10.Error = true;

            ExitState = "OE0088_TRUSTEE_FIRST_NAME_REQD";

            break;
          case 11:
            var field11 = GetField(export.Bankruptcy, "btoAddrStreet1");

            field11.Error = true;

            ExitState = "OE0089_TRUSTEE_ADDR_REQD";

            break;
          case 12:
            var field12 = GetField(export.Bankruptcy, "btoAddrCity");

            field12.Error = true;

            ExitState = "OE0090_TRUSTEE_CITY_REQD";

            break;
          case 13:
            var field13 = GetField(export.Bankruptcy, "btoAddrState");

            field13.Error = true;

            ExitState = "OE0091_INVALID_TRUSTEE_STATE";

            break;
          case 14:
            var field14 = GetField(export.Bankruptcy, "apAttorneyLastName");

            field14.Error = true;

            ExitState = "OE0092_ATTORNEY_LAST_REQD";

            break;
          case 15:
            var field15 = GetField(export.Bankruptcy, "apAttorneyFirstName");

            field15.Error = true;

            ExitState = "OE0093_ATTORNEY_FIRST_NAME_REQ";

            break;
          case 16:
            var field16 = GetField(export.Bankruptcy, "apAttrAddrStreet1");

            field16.Error = true;

            ExitState = "OE0094_ATTORNEY_ADDR_ST_REQD";

            break;
          case 17:
            var field17 = GetField(export.Bankruptcy, "apAttrAddrCity");

            field17.Error = true;

            ExitState = "OE0095_ATTR_ADDR_CITY_REQD";

            break;
          case 18:
            var field18 = GetField(export.Bankruptcy, "apAttrAddrState");

            field18.Error = true;

            ExitState = "OE0096_INVALID_ATTORNEY_STATE";

            break;
          case 19:
            var field19 = GetField(export.Bankruptcy, "bankruptcyFilingDate");

            field19.Error = true;

            ExitState = "OE0124_INVALID_BKRP_FILING_DT";

            break;
          case 20:
            var field20 =
              GetField(export.Bankruptcy, "dateRequestedMotionToLift");

            field20.Error = true;

            ExitState = "OE0125_INVALID_REQ_LIFT_MOTION";

            break;
          case 21:
            var field21 = GetField(export.Bankruptcy, "proofOfClaimFiledDate");

            field21.Error = true;

            ExitState = "OE0126_INV_PRF_OF_CLM_FILED_DT";

            break;
          case 22:
            var field22 =
              GetField(export.Bankruptcy, "bankruptcyConfirmationDate");

            field22.Error = true;

            ExitState = "OE0127_INVALID_BKRP_CONFIRM_DT";

            break;
          case 23:
            var field23 =
              GetField(export.Bankruptcy, "bankruptcyDischargeDate");

            field23.Error = true;

            ExitState = "OE0128_INVALID_BKRP_DISCH_DT";

            break;
          case 24:
            var field24 = GetField(export.Bankruptcy, "bdcAddrZip5");

            field24.Error = true;

            ExitState = "OE0129_INV_BKRP_DT_CT_ADDR_ZIP";

            break;
          case 25:
            var field25 = GetField(export.Bankruptcy, "btoAddrZip5");

            field25.Error = true;

            ExitState = "OE0130_INV_TRUST_OFF_ADDR_ZIP";

            break;
          case 26:
            var field26 = GetField(export.Bankruptcy, "apAttrAddrZip5");

            field26.Error = true;

            ExitState = "OE0131_INV_BKRP_ATTR_ADDR_ZIP";

            break;
          case 27:
            var field27 = GetField(export.CsePerson, "number");

            field27.Error = true;

            ExitState = "OE0132_CURR_BKRP_NOT_YET_DISCH";

            break;
          case 28:
            var field28 =
              GetField(export.Bankruptcy, "dateMotionToLiftGranted");

            field28.Error = true;

            ExitState = "OE0133_INV_MOTION_GRANTED_DT";

            break;
          case 29:
            var field29 = GetField(export.Bankruptcy, "bdcPhoneAreaCode");

            field29.Error = true;

            ExitState = "OE0000_PHONE_AREA_REQD";

            break;
          case 30:
            var field30 = GetField(export.Bankruptcy, "bdcPhoneNo");

            field30.Error = true;

            ExitState = "OE0000_PHONE_NBR_REQD";

            break;
          case 31:
            var field31 = GetField(export.Bankruptcy, "btoPhoneAreaCode");

            field31.Error = true;

            ExitState = "OE0000_PHONE_AREA_REQD";

            break;
          case 32:
            var field32 = GetField(export.Bankruptcy, "btoPhoneNo");

            field32.Error = true;

            ExitState = "OE0000_PHONE_NBR_REQD";

            break;
          case 33:
            var field33 = GetField(export.Bankruptcy, "btoFaxAreaCode");

            field33.Error = true;

            ExitState = "OE0000_FAX_AREA_CODE_REQD";

            break;
          case 34:
            var field34 = GetField(export.Bankruptcy, "btoFax");

            field34.Error = true;

            ExitState = "OE0000_FAX_NO_REQD";

            break;
          case 35:
            var field35 =
              GetField(export.Bankruptcy, "apAttorneyPhoneAreaCode");

            field35.Error = true;

            ExitState = "OE0000_PHONE_AREA_REQD";

            break;
          case 36:
            var field36 = GetField(export.Bankruptcy, "apAttorneyPhoneNo");

            field36.Error = true;

            ExitState = "OE0000_PHONE_NBR_REQD";

            break;
          case 37:
            var field37 = GetField(export.Bankruptcy, "apAttorneyFaxAreaCode");

            field37.Error = true;

            ExitState = "OE0000_FAX_AREA_CODE_REQD";

            break;
          case 38:
            var field38 = GetField(export.Bankruptcy, "apAttorneyFax");

            field38.Error = true;

            ExitState = "OE0000_FAX_NO_REQD";

            break;
          case 39:
            var field39 = GetField(export.Bankruptcy, "bdcAddrZip5");

            field39.Error = true;

            ExitState = "ACO_NE0000_ZIP_CODE_NOT_NUMERIC";

            break;
          case 40:
            var field40 = GetField(export.Bankruptcy, "btoAddrZip5");

            field40.Error = true;

            ExitState = "ACO_NE0000_ZIP_CODE_NOT_NUMERIC";

            break;
          case 41:
            var field41 = GetField(export.Bankruptcy, "apAttrAddrZip5");

            field41.Error = true;

            ExitState = "ACO_NE0000_ZIP_CODE_NOT_NUMERIC";

            break;
          case 42:
            // CQ#27198 05/25/2011  T. Pierce  This error is now used for Dis/
            // With
            // as well as Discharge overlaps.  Highlight correct field.
            if (!Equal(export.Bankruptcy.BankruptcyDismissWithdrawDate,
              local.ZeroDate.Date))
            {
              var field =
                GetField(export.Bankruptcy, "bankruptcyDismissWithdrawDate");

              field.Error = true;

              ExitState = "BANKRUPTCY_DIS_WITH_DATE_OVERLA";
            }

            if (!Equal(export.Bankruptcy.BankruptcyDischargeDate,
              local.ZeroDate.Date))
            {
              var field =
                GetField(export.Bankruptcy, "bankruptcyDischargeDate");

              field.Error = true;

              ExitState = "BANKRUPTCY_OVERLAP";
            }

            break;
          case 43:
            var field42 =
              GetField(export.Bankruptcy, "expectedBkrpDischargeDate");

            field42.Error = true;

            ExitState = "OE0000_INVALID_BKRP_EXPT_DSCH_DT";

            break;
          case 44:
            var field43 =
              GetField(export.Bankruptcy, "expectedBkrpDischargeDate");

            field43.Error = true;

            var field44 =
              GetField(export.Bankruptcy, "bankruptcyDischargeDate");

            field44.Error = true;

            ExitState = "OE0000_BKRP_DISCH_EXPT_DISCH_REQ";

            break;
          case 45:
            ExitState = "OE0000_BKRP_DELETE_NOT_ALLOWED";

            break;
          case 46:
            var field45 = GetField(export.Bankruptcy, "bankruptcyType");

            field45.Error = true;

            ExitState = "OE0000_MUST_CHANGE_BKRP_TYPE";

            break;
          case 47:
            var field46 = GetField(export.Bankruptcy, "bankruptcyFilingDate");

            field46.Error = true;

            ExitState = "OE0000_FILED_DT_CAN_NOT_GT_CURR";

            break;
          case 48:
            var field47 =
              GetField(export.Bankruptcy, "bankruptcyDischargeDate");

            field47.Error = true;

            ExitState = "OE0000_DISCH_DT_CANT_GT_CURR_DT";

            break;
          case 49:
            var field48 =
              GetField(export.Bankruptcy, "bankruptcyDischargeDate");

            field48.Error = true;

            ExitState = "OE0000_DISCH_DT_CANT_LT_FILE_DT";

            break;
          case 50:
            var field49 = GetField(export.Bankruptcy, "bankruptcyFilingDate");

            field49.Error = true;

            ExitState = "OE0000_FILED_DT_IS_REQUIRED";

            break;
          case 51:
            var field50 =
              GetField(export.Bankruptcy, "bankruptcyDismissWithdrawDate");

            field50.Error = true;

            ExitState = "OE0000_DSMWTH_DT_CANT_GT_CURR_DT";

            break;
          case 52:
            var field51 =
              GetField(export.Bankruptcy, "bankruptcyDismissWithdrawDate");

            field51.Error = true;

            ExitState = "OE0000_DSMWTH_DT_CANT_LT_FILE_DT";

            break;
          case 53:
            if (export.Bankruptcy.BdcPhoneAreaCode.GetValueOrDefault() == 0)
            {
              var field = GetField(export.Bankruptcy, "bdcPhoneAreaCode");

              field.Error = true;
            }

            if (export.Bankruptcy.BdcPhoneNo.GetValueOrDefault() == 0)
            {
              var field = GetField(export.Bankruptcy, "bdcPhoneNo");

              field.Error = true;
            }

            ExitState = "OE0000_AREA_PHONE_REQUIRED";

            break;
          case 54:
            if (export.Bankruptcy.BtoPhoneAreaCode.GetValueOrDefault() == 0)
            {
              var field = GetField(export.Bankruptcy, "btoPhoneAreaCode");

              field.Error = true;
            }

            if (export.Bankruptcy.BtoPhoneNo.GetValueOrDefault() == 0)
            {
              var field = GetField(export.Bankruptcy, "btoPhoneNo");

              field.Error = true;
            }

            ExitState = "OE0000_AREA_PHONE_REQUIRED";

            break;
          case 55:
            if (export.Bankruptcy.ApAttorneyPhoneAreaCode.GetValueOrDefault() ==
              0)
            {
              var field =
                GetField(export.Bankruptcy, "apAttorneyPhoneAreaCode");

              field.Error = true;
            }

            if (export.Bankruptcy.ApAttorneyPhoneNo.GetValueOrDefault() == 0)
            {
              var field = GetField(export.Bankruptcy, "apAttorneyPhoneNo");

              field.Error = true;
            }

            ExitState = "OE0000_AREA_PHONE_REQUIRED";

            break;
          case 56:
            var field52 =
              GetField(export.Bankruptcy, "bankruptcyDischargeDate");

            field52.Error = true;

            var field53 =
              GetField(export.Bankruptcy, "bankruptcyDismissWithdrawDate");

            field53.Error = true;

            ExitState = "OE0000_BKRP_DISMISS_DISCHRG_NA";

            break;
          case 57:
            var field54 =
              GetField(export.Bankruptcy, "expectedBkrpDischargeDate");

            field54.Error = true;

            var field55 =
              GetField(export.Bankruptcy, "bankruptcyDischargeDate");

            field55.Error = true;

            var field56 =
              GetField(export.Bankruptcy, "bankruptcyDismissWithdrawDate");

            field56.Error = true;

            ExitState = "OE0000_EXPDSCH_DISCH_DSWTH_REQD";

            break;
          case 58:
            var field57 =
              GetField(export.Bankruptcy, "bankruptcyConfirmationDate");

            field57.Error = true;

            ExitState = "OE0000_CONF_DT_CANT_GT_DISCH_DT";

            break;
          case 59:
            var field58 =
              GetField(export.Bankruptcy, "bankruptcyConfirmationDate");

            field58.Error = true;

            ExitState = "OE0000_CONF_DT_CANT_GT_DISWTH_DT";

            break;
          case 60:
            var field59 =
              GetField(export.Bankruptcy, "bankruptcyDischargeDate");

            field59.Error = true;

            ExitState = "OE0000_DISCHARGE_CANT_BE_CHANGED";

            break;
          case 61:
            var field60 =
              GetField(export.Bankruptcy, "bankruptcyDismissWithdrawDate");

            field60.Error = true;

            ExitState = "OE0000_DISMSWTHDRW_DT_CANT_CHG";

            break;
          case 62:
            var field61 =
              GetField(export.Bankruptcy, "bankruptcyConfirmationDate");

            field61.Error = true;

            ExitState = "OE0000_CONF_DT_NOT_ALLWD_W_TYP_7";

            break;
          case 63:
            var field62 =
              GetField(export.Bankruptcy, "bankruptcyDischargeDate");

            field62.Error = true;

            ExitState = "OE0000_DSCH_DT_NOT_ALLWD_W_TYP13";

            break;
          case 64:
            // CQ#27198 05/25/2011  T. Pierce  Added new error code evaluation.
            // This is the case when an expected discharge date was specified 
            // along
            // with either a discharge date or a dis/with date on Update or 
            // Create.
            if (!Equal(export.Bankruptcy.BankruptcyDischargeDate,
              local.LowValue.Date))
            {
              var field =
                GetField(export.Bankruptcy, "bankruptcyDischargeDate");

              field.Error = true;
            }

            if (!Equal(export.Bankruptcy.BankruptcyDismissWithdrawDate,
              local.LowValue.Date))
            {
              var field =
                GetField(export.Bankruptcy, "bankruptcyDismissWithdrawDate");

              field.Error = true;
            }

            var field63 =
              GetField(export.Bankruptcy, "expectedBkrpDischargeDate");

            field63.Error = true;

            ExitState = "OE0000_DIS_WITH_AND_DISCH_ERROR";

            break;
          default:
            var field64 = GetField(export.CsePerson, "number");

            field64.Error = true;

            ExitState = "OE0004_UNKNOWN_ERROR_CODE";

            break;
        }

        --local.Errors.Index;
        local.Errors.CheckSize();
      }
    }
  }

  private static void MoveBankruptcy1(Bankruptcy source, Bankruptcy target)
  {
    target.BtoFaxAreaCode = source.BtoFaxAreaCode;
    target.BtoPhoneAreaCode = source.BtoPhoneAreaCode;
    target.BdcPhoneAreaCode = source.BdcPhoneAreaCode;
    target.ApAttorneyFaxAreaCode = source.ApAttorneyFaxAreaCode;
    target.ApAttorneyPhoneAreaCode = source.ApAttorneyPhoneAreaCode;
    target.BtoPhoneExt = source.BtoPhoneExt;
    target.BtoFaxExt = source.BtoFaxExt;
    target.BdcPhoneExt = source.BdcPhoneExt;
    target.ApAttorneyFaxExt = source.ApAttorneyFaxExt;
    target.ApAttorneyPhoneExt = source.ApAttorneyPhoneExt;
    target.BankruptcyCourtActionNo = source.BankruptcyCourtActionNo;
    target.BankruptcyType = source.BankruptcyType;
    target.BankruptcyFilingDate = source.BankruptcyFilingDate;
    target.BankruptcyDischargeDate = source.BankruptcyDischargeDate;
    target.BankruptcyConfirmationDate = source.BankruptcyConfirmationDate;
    target.ExpectedBkrpDischargeDate = source.ExpectedBkrpDischargeDate;
    target.ProofOfClaimFiledDate = source.ProofOfClaimFiledDate;
    target.DateRequestedMotionToLift = source.DateRequestedMotionToLift;
    target.DateMotionToLiftGranted = source.DateMotionToLiftGranted;
    target.TrusteeLastName = source.TrusteeLastName;
    target.TrusteeFirstName = source.TrusteeFirstName;
    target.TrusteeMiddleInt = source.TrusteeMiddleInt;
    target.TrusteeSuffix = source.TrusteeSuffix;
    target.BtoPhoneNo = source.BtoPhoneNo;
    target.BtoFax = source.BtoFax;
    target.BtoAddrStreet1 = source.BtoAddrStreet1;
    target.BtoAddrStreet2 = source.BtoAddrStreet2;
    target.BtoAddrCity = source.BtoAddrCity;
    target.BtoAddrState = source.BtoAddrState;
    target.BtoAddrZip5 = source.BtoAddrZip5;
    target.BtoAddrZip4 = source.BtoAddrZip4;
    target.BtoAddrZip3 = source.BtoAddrZip3;
    target.BankruptcyDistrictCourt = source.BankruptcyDistrictCourt;
    target.BdcPhoneNo = source.BdcPhoneNo;
    target.BdcAddrStreet1 = source.BdcAddrStreet1;
    target.BdcAddrStreet2 = source.BdcAddrStreet2;
    target.BdcAddrCity = source.BdcAddrCity;
    target.BdcAddrState = source.BdcAddrState;
    target.BdcAddrZip5 = source.BdcAddrZip5;
    target.BdcAddrZip4 = source.BdcAddrZip4;
    target.BdcAddrZip3 = source.BdcAddrZip3;
    target.ApAttorneyFirmName = source.ApAttorneyFirmName;
    target.ApAttorneyLastName = source.ApAttorneyLastName;
    target.ApAttorneyFirstName = source.ApAttorneyFirstName;
    target.ApAttorneyMi = source.ApAttorneyMi;
    target.ApAttorneySuffix = source.ApAttorneySuffix;
    target.ApAttorneyPhoneNo = source.ApAttorneyPhoneNo;
    target.ApAttorneyFax = source.ApAttorneyFax;
    target.ApAttrAddrStreet1 = source.ApAttrAddrStreet1;
    target.ApAttrAddrStreet2 = source.ApAttrAddrStreet2;
    target.ApAttrAddrCity = source.ApAttrAddrCity;
    target.ApAttrAddrState = source.ApAttrAddrState;
    target.ApAttrAddrZip5 = source.ApAttrAddrZip5;
    target.ApAttrAddrZip4 = source.ApAttrAddrZip4;
    target.ApAttrAddrZip3 = source.ApAttrAddrZip3;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.Narrative = source.Narrative;
    target.BankruptcyDismissWithdrawDate = source.BankruptcyDismissWithdrawDate;
  }

  private static void MoveBankruptcy2(Bankruptcy source, Bankruptcy target)
  {
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
  }

  private static void MoveErrors(OeBkrpValidateBankruptcy.Export.
    ErrorsGroup source, Local.ErrorsGroup target)
  {
    target.DetailErrorCode.Count = source.DetailErrorCode.Count;
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.SituationNumber = source.SituationNumber;
    target.EventId = source.EventId;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.UserId = source.UserId;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private static void MoveScrollingAttributes(ScrollingAttributes source,
    ScrollingAttributes target)
  {
    target.PlusFlag = source.PlusFlag;
    target.MinusFlag = source.MinusFlag;
  }

  private static void MoveSpDocLiteral(SpDocLiteral source, SpDocLiteral target)
  {
    target.IdBankruptcy = source.IdBankruptcy;
    target.IdDocument = source.IdDocument;
    target.IdPrNumber = source.IdPrNumber;
  }

  private string UseCabConvertDate2String()
  {
    var useImport = new CabConvertDate2String.Import();
    var useExport = new CabConvertDate2String.Export();

    useImport.DateWorkArea.Date = local.Date.Date;

    Call(CabConvertDate2String.Execute, useImport, useExport);

    return useExport.TextWorkArea.Text8;
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

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseOeBkrpCreateBankruptcy()
  {
    var useImport = new OeBkrpCreateBankruptcy.Import();
    var useExport = new OeBkrpCreateBankruptcy.Export();

    useImport.Bankruptcy.Assign(export.Bankruptcy);
    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(OeBkrpCreateBankruptcy.Execute, useImport, useExport);

    MoveBankruptcy2(useExport.UpdateStamp, export.UpdateStamp);
    export.Bankruptcy.Assign(useExport.Bankruptcy);
  }

  private void UseOeBkrpDeleteBankruptcy()
  {
    var useImport = new OeBkrpDeleteBankruptcy.Import();
    var useExport = new OeBkrpDeleteBankruptcy.Export();

    useImport.Bankruptcy.Identifier = export.Bankruptcy.Identifier;
    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(OeBkrpDeleteBankruptcy.Execute, useImport, useExport);
  }

  private void UseOeBkrpDisplayBankruptcy()
  {
    var useImport = new OeBkrpDisplayBankruptcy.Import();
    var useExport = new OeBkrpDisplayBankruptcy.Export();

    useImport.Case1.Number = export.Case1.Number;
    useImport.UserAction.Command = local.UserAction.Command;
    useImport.Bankruptcy.Assign(export.Bankruptcy);
    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(OeBkrpDisplayBankruptcy.Execute, useImport, useExport);

    local.BankruptcyDisplayed.Flag = useExport.BankruptcyDisplayed.Flag;
    MoveBankruptcy2(useExport.UpdateStamp, export.UpdateStamp);
    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    MoveScrollingAttributes(useExport.ScrollingAttributes,
      export.ScrollingAttributes);
    export.Bankruptcy.Assign(useExport.Bankruptcy);
  }

  private void UseOeBkrpUpdateBankruptcy()
  {
    var useImport = new OeBkrpUpdateBankruptcy.Import();
    var useExport = new OeBkrpUpdateBankruptcy.Export();

    useImport.Bankruptcy.Assign(export.Bankruptcy);
    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(OeBkrpUpdateBankruptcy.Execute, useImport, useExport);

    MoveBankruptcy2(useExport.UpdateStamp, export.UpdateStamp);
    export.Bankruptcy.Assign(useExport.Bankruptcy);
  }

  private void UseOeBkrpValidateBankruptcy()
  {
    var useImport = new OeBkrpValidateBankruptcy.Import();
    var useExport = new OeBkrpValidateBankruptcy.Export();

    useImport.Bankruptcy.Assign(export.Bankruptcy);
    useImport.CsePerson.Number = export.CsePerson.Number;
    useImport.UserAction.Command = local.UserAction.Command;

    Call(OeBkrpValidateBankruptcy.Execute, useImport, useExport);

    local.LastErrorEntryNo.Count = useExport.LastErrorEntryNo.Count;
    useExport.Errors.CopyTo(local.Errors, MoveErrors);
  }

  private void UseOeCabRaiseEvent()
  {
    var useImport = new OeCabRaiseEvent.Import();
    var useExport = new OeCabRaiseEvent.Export();

    MoveInfrastructure(local.Infrastructure, useImport.Infrastructure);
    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(OeCabRaiseEvent.Execute, useImport, useExport);

    local.Infrastructure.Assign(useExport.Infrastructure);
  }

  private void UseOeCabReadInfrastructure()
  {
    var useImport = new OeCabReadInfrastructure.Import();
    var useExport = new OeCabReadInfrastructure.Export();

    useImport.Infrastructure.SystemGeneratedIdentifier =
      local.LastTran.SystemGeneratedIdentifier;

    Call(OeCabReadInfrastructure.Execute, useImport, useExport);

    local.LastTran.Assign(useExport.Infrastructure);
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.HiddenNextTranInfo.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut1()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(export.HiddenNextTranInfo);

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabNextTranPut2()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = export.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(export.HiddenNextTranInfo);

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

    useImport.Case1.Number = import.Case1.Number;
    useImport.CsePerson.Number = import.CsePerson.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSpDocSetLiterals()
  {
    var useImport = new SpDocSetLiterals.Import();
    var useExport = new SpDocSetLiterals.Export();

    Call(SpDocSetLiterals.Execute, useImport, useExport);

    MoveSpDocLiteral(useExport.SpDocLiteral, local.SpDocLiteral);
  }

  private void UseSpPrintDecodeReturnCode()
  {
    var useImport = new SpPrintDecodeReturnCode.Import();
    var useExport = new SpPrintDecodeReturnCode.Export();

    useImport.WorkArea.Text50 = local.Print.Text50;

    Call(SpPrintDecodeReturnCode.Execute, useImport, useExport);

    local.Print.Text50 = useExport.WorkArea.Text50;
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
    /// <summary>
    /// A value of HiddenLastRead.
    /// </summary>
    [JsonPropertyName("hiddenLastRead")]
    public Bankruptcy HiddenLastRead
    {
      get => hiddenLastRead ??= new();
      set => hiddenLastRead = value;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public CodeValue Selected
    {
      get => selected ??= new();
      set => selected = value;
    }

    /// <summary>
    /// A value of UpdateStamp.
    /// </summary>
    [JsonPropertyName("updateStamp")]
    public Bankruptcy UpdateStamp
    {
      get => updateStamp ??= new();
      set => updateStamp = value;
    }

    /// <summary>
    /// A value of ListBkrpTypes.
    /// </summary>
    [JsonPropertyName("listBkrpTypes")]
    public Standard ListBkrpTypes
    {
      get => listBkrpTypes ??= new();
      set => listBkrpTypes = value;
    }

    /// <summary>
    /// A value of ListBkrpDistCtStates.
    /// </summary>
    [JsonPropertyName("listBkrpDistCtStates")]
    public Standard ListBkrpDistCtStates
    {
      get => listBkrpDistCtStates ??= new();
      set => listBkrpDistCtStates = value;
    }

    /// <summary>
    /// A value of ListTrusteeOffrStates.
    /// </summary>
    [JsonPropertyName("listTrusteeOffrStates")]
    public Standard ListTrusteeOffrStates
    {
      get => listTrusteeOffrStates ??= new();
      set => listTrusteeOffrStates = value;
    }

    /// <summary>
    /// A value of ListAttorneyStates.
    /// </summary>
    [JsonPropertyName("listAttorneyStates")]
    public Standard ListAttorneyStates
    {
      get => listAttorneyStates ??= new();
      set => listAttorneyStates = value;
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
    /// A value of ListCsePersons.
    /// </summary>
    [JsonPropertyName("listCsePersons")]
    public Standard ListCsePersons
    {
      get => listCsePersons ??= new();
      set => listCsePersons = value;
    }

    /// <summary>
    /// A value of HiddenDisplaySuccessful.
    /// </summary>
    [JsonPropertyName("hiddenDisplaySuccessful")]
    public Common HiddenDisplaySuccessful
    {
      get => hiddenDisplaySuccessful ??= new();
      set => hiddenDisplaySuccessful = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of HiddenPrevUserAction.
    /// </summary>
    [JsonPropertyName("hiddenPrevUserAction")]
    public Common HiddenPrevUserAction
    {
      get => hiddenPrevUserAction ??= new();
      set => hiddenPrevUserAction = value;
    }

    /// <summary>
    /// A value of HiddenPrevBankruptcy.
    /// </summary>
    [JsonPropertyName("hiddenPrevBankruptcy")]
    public Bankruptcy HiddenPrevBankruptcy
    {
      get => hiddenPrevBankruptcy ??= new();
      set => hiddenPrevBankruptcy = value;
    }

    /// <summary>
    /// A value of HiddenPrevCsePerson.
    /// </summary>
    [JsonPropertyName("hiddenPrevCsePerson")]
    public CsePerson HiddenPrevCsePerson
    {
      get => hiddenPrevCsePerson ??= new();
      set => hiddenPrevCsePerson = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
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
    /// A value of HiddenCase.
    /// </summary>
    [JsonPropertyName("hiddenCase")]
    public Case1 HiddenCase
    {
      get => hiddenCase ??= new();
      set => hiddenCase = value;
    }

    private Bankruptcy hiddenLastRead;
    private CodeValue selected;
    private Bankruptcy updateStamp;
    private Standard listBkrpTypes;
    private Standard listBkrpDistCtStates;
    private Standard listTrusteeOffrStates;
    private Standard listAttorneyStates;
    private Case1 case1;
    private Standard listCsePersons;
    private Common hiddenDisplaySuccessful;
    private ScrollingAttributes scrollingAttributes;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common hiddenPrevUserAction;
    private Bankruptcy hiddenPrevBankruptcy;
    private CsePerson hiddenPrevCsePerson;
    private CsePerson csePerson;
    private Bankruptcy bankruptcy;
    private Standard standard;
    private NextTranInfo hiddenNextTranInfo;
    private Case1 hiddenCase;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of HiddenLastRead.
    /// </summary>
    [JsonPropertyName("hiddenLastRead")]
    public Bankruptcy HiddenLastRead
    {
      get => hiddenLastRead ??= new();
      set => hiddenLastRead = value;
    }

    /// <summary>
    /// A value of Required.
    /// </summary>
    [JsonPropertyName("required")]
    public Code Required
    {
      get => required ??= new();
      set => required = value;
    }

    /// <summary>
    /// A value of UpdateStamp.
    /// </summary>
    [JsonPropertyName("updateStamp")]
    public Bankruptcy UpdateStamp
    {
      get => updateStamp ??= new();
      set => updateStamp = value;
    }

    /// <summary>
    /// A value of ListBkrpTypes.
    /// </summary>
    [JsonPropertyName("listBkrpTypes")]
    public Standard ListBkrpTypes
    {
      get => listBkrpTypes ??= new();
      set => listBkrpTypes = value;
    }

    /// <summary>
    /// A value of ListBkrpDistCtStates.
    /// </summary>
    [JsonPropertyName("listBkrpDistCtStates")]
    public Standard ListBkrpDistCtStates
    {
      get => listBkrpDistCtStates ??= new();
      set => listBkrpDistCtStates = value;
    }

    /// <summary>
    /// A value of ListTrusteeOffrStates.
    /// </summary>
    [JsonPropertyName("listTrusteeOffrStates")]
    public Standard ListTrusteeOffrStates
    {
      get => listTrusteeOffrStates ??= new();
      set => listTrusteeOffrStates = value;
    }

    /// <summary>
    /// A value of ListAttorneyStates.
    /// </summary>
    [JsonPropertyName("listAttorneyStates")]
    public Standard ListAttorneyStates
    {
      get => listAttorneyStates ??= new();
      set => listAttorneyStates = value;
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
    /// A value of ListCsePersons.
    /// </summary>
    [JsonPropertyName("listCsePersons")]
    public Standard ListCsePersons
    {
      get => listCsePersons ??= new();
      set => listCsePersons = value;
    }

    /// <summary>
    /// A value of HiddenDisplayPerformed.
    /// </summary>
    [JsonPropertyName("hiddenDisplayPerformed")]
    public Common HiddenDisplayPerformed
    {
      get => hiddenDisplayPerformed ??= new();
      set => hiddenDisplayPerformed = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of HiddenPrevUserAction.
    /// </summary>
    [JsonPropertyName("hiddenPrevUserAction")]
    public Common HiddenPrevUserAction
    {
      get => hiddenPrevUserAction ??= new();
      set => hiddenPrevUserAction = value;
    }

    /// <summary>
    /// A value of HiddenPrevBankruptcy.
    /// </summary>
    [JsonPropertyName("hiddenPrevBankruptcy")]
    public Bankruptcy HiddenPrevBankruptcy
    {
      get => hiddenPrevBankruptcy ??= new();
      set => hiddenPrevBankruptcy = value;
    }

    /// <summary>
    /// A value of HiddenPrevCsePerson.
    /// </summary>
    [JsonPropertyName("hiddenPrevCsePerson")]
    public CsePerson HiddenPrevCsePerson
    {
      get => hiddenPrevCsePerson ??= new();
      set => hiddenPrevCsePerson = value;
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
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    /// <summary>
    /// A value of HiddenCase.
    /// </summary>
    [JsonPropertyName("hiddenCase")]
    public Case1 HiddenCase
    {
      get => hiddenCase ??= new();
      set => hiddenCase = value;
    }

    private Bankruptcy hiddenLastRead;
    private Code required;
    private Bankruptcy updateStamp;
    private Standard listBkrpTypes;
    private Standard listBkrpDistCtStates;
    private Standard listTrusteeOffrStates;
    private Standard listAttorneyStates;
    private Case1 case1;
    private Standard listCsePersons;
    private Common hiddenDisplayPerformed;
    private ScrollingAttributes scrollingAttributes;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common hiddenPrevUserAction;
    private Bankruptcy hiddenPrevBankruptcy;
    private CsePerson hiddenPrevCsePerson;
    private CsePerson csePerson;
    private Bankruptcy bankruptcy;
    private AbendData abendData;
    private Standard standard;
    private NextTranInfo hiddenNextTranInfo;
    private Case1 hiddenCase;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A ErrorsGroup group.</summary>
    [Serializable]
    public class ErrorsGroup
    {
      /// <summary>
      /// A value of DetailErrorCode.
      /// </summary>
      [JsonPropertyName("detailErrorCode")]
      public Common DetailErrorCode
      {
        get => detailErrorCode ??= new();
        set => detailErrorCode = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common detailErrorCode;
    }

    /// <summary>
    /// A value of EmptyUpdateStamp.
    /// </summary>
    [JsonPropertyName("emptyUpdateStamp")]
    public Bankruptcy EmptyUpdateStamp
    {
      get => emptyUpdateStamp ??= new();
      set => emptyUpdateStamp = value;
    }

    /// <summary>
    /// A value of EmptyCase.
    /// </summary>
    [JsonPropertyName("emptyCase")]
    public Case1 EmptyCase
    {
      get => emptyCase ??= new();
      set => emptyCase = value;
    }

    /// <summary>
    /// A value of EmptyBankruptcy.
    /// </summary>
    [JsonPropertyName("emptyBankruptcy")]
    public Bankruptcy EmptyBankruptcy
    {
      get => emptyBankruptcy ??= new();
      set => emptyBankruptcy = value;
    }

    /// <summary>
    /// A value of LowValue.
    /// </summary>
    [JsonPropertyName("lowValue")]
    public DateWorkArea LowValue
    {
      get => lowValue ??= new();
      set => lowValue = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public NextTranInfo Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of BatchConvertNumToText.
    /// </summary>
    [JsonPropertyName("batchConvertNumToText")]
    public BatchConvertNumToText BatchConvertNumToText
    {
      get => batchConvertNumToText ??= new();
      set => batchConvertNumToText = value;
    }

    /// <summary>
    /// A value of Print.
    /// </summary>
    [JsonPropertyName("print")]
    public WorkArea Print
    {
      get => print ??= new();
      set => print = value;
    }

    /// <summary>
    /// A value of Position.
    /// </summary>
    [JsonPropertyName("position")]
    public Common Position
    {
      get => position ??= new();
      set => position = value;
    }

    /// <summary>
    /// A value of SpDocLiteral.
    /// </summary>
    [JsonPropertyName("spDocLiteral")]
    public SpDocLiteral SpDocLiteral
    {
      get => spDocLiteral ??= new();
      set => spDocLiteral = value;
    }

    /// <summary>
    /// A value of Prompt.
    /// </summary>
    [JsonPropertyName("prompt")]
    public Common Prompt
    {
      get => prompt ??= new();
      set => prompt = value;
    }

    /// <summary>
    /// A value of ZeroDate.
    /// </summary>
    [JsonPropertyName("zeroDate")]
    public DateWorkArea ZeroDate
    {
      get => zeroDate ??= new();
      set => zeroDate = value;
    }

    /// <summary>
    /// A value of Date.
    /// </summary>
    [JsonPropertyName("date")]
    public DateWorkArea Date
    {
      get => date ??= new();
      set => date = value;
    }

    /// <summary>
    /// A value of DetailText30.
    /// </summary>
    [JsonPropertyName("detailText30")]
    public TextWorkArea DetailText30
    {
      get => detailText30 ??= new();
      set => detailText30 = value;
    }

    /// <summary>
    /// A value of DetailText25.
    /// </summary>
    [JsonPropertyName("detailText25")]
    public WorkArea DetailText25
    {
      get => detailText25 ??= new();
      set => detailText25 = value;
    }

    /// <summary>
    /// A value of DetailText10.
    /// </summary>
    [JsonPropertyName("detailText10")]
    public TextWorkArea DetailText10
    {
      get => detailText10 ??= new();
      set => detailText10 = value;
    }

    /// <summary>
    /// A value of RaiseEventFlag.
    /// </summary>
    [JsonPropertyName("raiseEventFlag")]
    public WorkArea RaiseEventFlag
    {
      get => raiseEventFlag ??= new();
      set => raiseEventFlag = value;
    }

    /// <summary>
    /// A value of NumberOfEvents.
    /// </summary>
    [JsonPropertyName("numberOfEvents")]
    public Common NumberOfEvents
    {
      get => numberOfEvents ??= new();
      set => numberOfEvents = value;
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
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
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
    /// A value of UndischargedBkrpDeleted.
    /// </summary>
    [JsonPropertyName("undischargedBkrpDeleted")]
    public Common UndischargedBkrpDeleted
    {
      get => undischargedBkrpDeleted ??= new();
      set => undischargedBkrpDeleted = value;
    }

    /// <summary>
    /// A value of BkrpDischargedNow.
    /// </summary>
    [JsonPropertyName("bkrpDischargedNow")]
    public Common BkrpDischargedNow
    {
      get => bkrpDischargedNow ??= new();
      set => bkrpDischargedNow = value;
    }

    /// <summary>
    /// A value of BankruptcyCreated.
    /// </summary>
    [JsonPropertyName("bankruptcyCreated")]
    public Common BankruptcyCreated
    {
      get => bankruptcyCreated ??= new();
      set => bankruptcyCreated = value;
    }

    /// <summary>
    /// A value of BankruptcyDisplayed.
    /// </summary>
    [JsonPropertyName("bankruptcyDisplayed")]
    public Common BankruptcyDisplayed
    {
      get => bankruptcyDisplayed ??= new();
      set => bankruptcyDisplayed = value;
    }

    /// <summary>
    /// A value of InitialisedToBlanks.
    /// </summary>
    [JsonPropertyName("initialisedToBlanks")]
    public Bankruptcy InitialisedToBlanks
    {
      get => initialisedToBlanks ??= new();
      set => initialisedToBlanks = value;
    }

    /// <summary>
    /// A value of DisplayNotSuccessful.
    /// </summary>
    [JsonPropertyName("displayNotSuccessful")]
    public Common DisplayNotSuccessful
    {
      get => displayNotSuccessful ??= new();
      set => displayNotSuccessful = value;
    }

    /// <summary>
    /// A value of LastErrorEntryNo.
    /// </summary>
    [JsonPropertyName("lastErrorEntryNo")]
    public Common LastErrorEntryNo
    {
      get => lastErrorEntryNo ??= new();
      set => lastErrorEntryNo = value;
    }

    /// <summary>
    /// Gets a value of Errors.
    /// </summary>
    [JsonIgnore]
    public Array<ErrorsGroup> Errors => errors ??= new(ErrorsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Errors for json serialization.
    /// </summary>
    [JsonPropertyName("errors")]
    [Computed]
    public IList<ErrorsGroup> Errors_Json
    {
      get => errors;
      set => Errors.Assign(value);
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
    /// A value of HighlightError.
    /// </summary>
    [JsonPropertyName("highlightError")]
    public Common HighlightError
    {
      get => highlightError ??= new();
      set => highlightError = value;
    }

    /// <summary>
    /// A value of LastTran.
    /// </summary>
    [JsonPropertyName("lastTran")]
    public Infrastructure LastTran
    {
      get => lastTran ??= new();
      set => lastTran = value;
    }

    private Bankruptcy emptyUpdateStamp;
    private Case1 emptyCase;
    private Bankruptcy emptyBankruptcy;
    private DateWorkArea lowValue;
    private NextTranInfo null1;
    private BatchConvertNumToText batchConvertNumToText;
    private WorkArea print;
    private Common position;
    private SpDocLiteral spDocLiteral;
    private Common prompt;
    private DateWorkArea zeroDate;
    private DateWorkArea date;
    private TextWorkArea detailText30;
    private WorkArea detailText25;
    private TextWorkArea detailText10;
    private WorkArea raiseEventFlag;
    private Common numberOfEvents;
    private Infrastructure infrastructure;
    private TextWorkArea textWorkArea;
    private Document document;
    private Common undischargedBkrpDeleted;
    private Common bkrpDischargedNow;
    private Common bankruptcyCreated;
    private Common bankruptcyDisplayed;
    private Bankruptcy initialisedToBlanks;
    private Common displayNotSuccessful;
    private Common lastErrorEntryNo;
    private Array<ErrorsGroup> errors;
    private Common userAction;
    private Common highlightError;
    private Infrastructure lastTran;
  }
#endregion
}
