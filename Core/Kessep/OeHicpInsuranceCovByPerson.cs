// Program: OE_HICP_INSURANCE_COV_BY_PERSON, ID: 371845772, model: 746.
// Short name: SWEHICPP
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
/// A program: OE_HICP_INSURANCE_COV_BY_PERSON.
/// </para>
/// <para>
/// Resp - OBLGESTB
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OeHicpInsuranceCovByPerson: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_HICP_INSURANCE_COV_BY_PERSON program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeHicpInsuranceCovByPerson(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeHicpInsuranceCovByPerson.
  /// </summary>
  public OeHicpInsuranceCovByPerson(IContext context, Import import,
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
    // -------------------------------------------------------------------
    // Date      	Author		Reason
    // Jan 1995  	Rebecca Grimes  Initial Development
    // 02/14/95  	Sid Chowdhary   Rework/ Completion
    // 02/16/96  	T.O.Redmond     Retrofit
    // 04/05/96  	G.Lofton	Unit test corrections
    // 08/17/96  	Sid Chowdhary	Corrections.
    // 11/14/96	R. Marchman	Add new security and next tran.
    // 12/19/1996	Raju		Event insertion
    // 01/08/97	Raju		event trigger coverage verified dt
    // 				- if user changes existing date
    // 				to null date then no event is
    // 				raised
    // 02/18/97	Sid Chowdhary	Add case specific header.
    // 01/29/1999	M Ramirez	Added creation of document trigger.
    // 
    // 02/15/99	S Johnson	Change edit on Coverage Verified
    // 02/22/199       S Johnson       Disabled an edit on Coverage End
    // 				Date in create and update case of.
    // 
    // 03/01/1999      S Johnson       Fixed next tran
    // 10/10/1999	D.Lowry		PRH75900.  Added the new exit states for a 				
    // create.
    // 03/02/2000            Vithal Madhira                       PR# 89931
    // The child ('CH') is the only valid case role to add as an '
    // insured person' on HICP screen
    // ------------------------------------------------------------------
    // 11/17/00 	M.Lachowicz     WR 298. Create header
    //                           	information for screens.
    // 12/17/01 	M.Ashworth      PR133485. Added cab to send document to other
    //                          	state if the case is interstate.
    // 08/19/08	J. Huss		CQ 518	Changed so case number is passed when document 
    // trigger is created.
    // 10/23/2008	J Huss		CQ# 399  Moved check for AR being the policy holder
    // 				into SP_CAB_DETERMINE_INTERSTATE_DOC
    // -----------------------------------------------
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
    // * 12/21/2010  Raj S              CQ24156     Modified to remove the 
    // business edit for *
    // *
    // 
    // monthly premium amount.  This field can  *
    // *
    // 
    // have zeros even when premium verified    *
    // *
    // 
    // date is entered by the user.             *
    // *
    // 
    // The Screen field Monthly Premium Amount  *
    // *
    // 
    // domain changed from Number to Character  *
    // *
    // 
    // in order to display spaces in case of    *
    // *
    // 
    // amount is zero and verified date if null.*
    // *
    // 
    // *
    // ***************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.CurrentDate.Date = Now().Date;
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      export.Case1.Number = import.Case1.Number;
      export.CsePerson.Number = import.CsePerson.Number;
      export.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);
      export.ServiceProvider.LastName = import.ServiceProvider.LastName;
      MoveOffice(import.Office, export.Office);
      export.Next.Number = import.Next.Number;

      return;
    }

    // Move Imports to Exports
    export.CsePersonPrompt.SelectChar = import.CsePersonPrompt.SelectChar;
    export.Case1.Number = import.Case1.Number;
    export.CsePerson.Number = import.CsePerson.Number;
    export.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);
    MoveHealthInsuranceCompany(import.HealthInsuranceCompany,
      export.HealthInsuranceCompany);
    export.HealthInsuranceCoverage.Assign(import.HealthInsuranceCoverage);
    MoveContact(import.Contact, export.Contact);
    MoveCommon(import.WorkPromptContact, export.WorkPromptContact);
    MoveCommon(import.WorkPromptCoverage, export.WorkPromptCoverage);
    export.WorkPromptInd.Flag = import.WorkPromptInd.Flag;
    export.ServiceProvider.LastName = import.ServiceProvider.LastName;
    MoveOffice(import.Office, export.Office);
    export.Next.Number = import.Next.Number;
    export.CaseOpen.Flag = import.CaseOpen.Flag;

    export.Export1.Index = 0;
    export.Export1.Clear();

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (export.Export1.IsFull)
      {
        break;
      }

      export.Export1.Update.DetailsCaseRole.Type1 =
        import.Import1.Item.DetailsCaseRole.Type1;
      export.Export1.Update.Detail.SelectChar =
        import.Import1.Item.Detail.SelectChar;
      export.Export1.Update.Insured.Number = import.Import1.Item.Insured.Number;
      export.Export1.Update.InsuredName.FormattedNameText =
        import.Import1.Item.InsuredName.FormattedNameText;
      export.Export1.Update.InsuredH.Number =
        import.Import1.Item.InsuredH.Number;
      export.Export1.Update.DetailsPersonalHealthInsurance.Assign(
        import.Import1.Item.DetailsPersonalHealthInsurance);
      export.Export1.Update.H.CoverageVerifiedDate =
        import.Import1.Item.H.CoverageVerifiedDate;
      export.Export1.Update.PersonPrompt.SelectChar =
        import.Import1.Item.PersonPrompt.SelectChar;
      export.Export1.Update.DetailsCovAmtTxt.Text7 =
        import.Import1.Item.DetailsCovAmtTxt.Text7;
      export.Export1.Update.HiddenLastRead.CoverageVerifiedDate =
        import.Import1.Item.HiddenLastRead.CoverageVerifiedDate;
      export.Export1.Next();
    }

    // Move Hidden Import Views to Hidden Export Views
    export.Hcase.Number = import.Hcase.Number;
    export.HcsePerson.Number = import.HcsePerson.Number;
    export.SelectedCase.Number = import.SelectedCase.Number;
    export.SelectedCsePerson.Number = import.SelectedCsePerson.Number;
    export.SelectedCsePersonsWorkSet.Assign(import.SelectedCsePersonsWorkSet);
    export.SelectedContact.Assign(import.SelectedContact);
    export.SelectedHealthInsuranceCompany.Assign(
      import.SelectedHealthInsuranceCompany);
    export.SelectedHealthInsuranceCoverage.Assign(
      import.SelectedHealthInsuranceCoverage);

    // 11/17/00 M.L Start
    export.HeaderLine.Text35 = import.HeaderLine.Text35;

    // 11/17/00 M.L End
    export.Standard.NextTransaction = import.Standard.NextTransaction;

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

    local.SearchCase.Number = export.Case1.Number;
    local.SearchCsePerson.Number = export.CsePerson.Number;

    for(export.Export1.Index = 0; export.Export1.Index < export.Export1.Count; ++
      export.Export1.Index)
    {
      if (!IsEmpty(export.Export1.Item.Insured.Number))
      {
        local.TextWorkArea.Text10 = export.Export1.Item.Insured.Number;
        UseEabPadLeftWithZeros();
        export.Export1.Update.Insured.Number = local.TextWorkArea.Text10;
      }
    }

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // ************************************************
      // *Flowing from here to Next Tran.               *
      // ************************************************
      export.Hidden.CaseNumber = export.Case1.Number;
      export.Hidden.CsePersonNumber = export.CsePerson.Number;
      UseScCabNextTranPut();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();

      if (!IsEmpty(export.Hidden.CaseNumber))
      {
        export.Case1.Number = export.Hidden.CaseNumber ?? Spaces(10);
      }

      if (!IsEmpty(export.Hidden.CsePersonNumber))
      {
        export.CsePerson.Number = export.Hidden.CsePersonNumber ?? Spaces(10);
      }

      // --------------------------------------------
      // Date 12/12/96:0800hrs CST
      // Code change effected by : Raju
      // Input from SID : escape removed after
      //    command is display
      // --------------------------------------------
      // ---------------------------------------------
      // Start of Code (Raju 01/20/97:1500 hrs CST)
      // ---------------------------------------------
      if (Equal(export.Hidden.LastTran, "SRPT") || Equal
        (export.Hidden.LastTran, "SRPU"))
      {
        local.LastTran.SystemGeneratedIdentifier =
          export.Hidden.InfrastructureId.GetValueOrDefault();
        UseOeCabReadInfrastructure();
        export.Case1.Number = local.LastTran.CaseNumber ?? Spaces(10);
        export.CsePerson.Number = local.LastTran.CsePersonNumber ?? Spaces(10);
        export.HealthInsuranceCoverage.Identifier =
          local.LastTran.DenormNumeric12.GetValueOrDefault();
      }

      // ---------------------------------------------
      // End  of Code
      // ---------------------------------------------
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    UseOeCabSetMnemonics();

    if (!Equal(global.Command, "LIST") && !Equal(global.Command, "RETCOMP") && !
      Equal(global.Command, "RETNAME") && !Equal(global.Command, "RETHICL") && !
      Equal(global.Command, "RETHIPL") && !Equal(global.Command, "RETPCOL") && !
      Equal(global.Command, "RETHIPH"))
    {
      export.CsePersonPrompt.SelectChar = "";
      export.WorkPromptContact.SelectChar = "";
      export.WorkPromptCoverage.SelectChar = "";

      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        export.Export1.Update.PersonPrompt.SelectChar = "";
      }
    }

    local.SelectCount.Count = 0;

    for(export.Export1.Index = 0; export.Export1.Index < export.Export1.Count; ++
      export.Export1.Index)
    {
      switch(AsChar(export.Export1.Item.Detail.SelectChar))
      {
        case 'S':
          ++local.SelectCount.Count;

          break;
        case '*':
          // ---------------------------------------------
          // No action is taken but * is a valid entry.
          // It specifies that the action taken on the row
          // was successful.
          // --------------------------------------------
          break;
        case ' ':
          // ---------------------------------------------
          // No action is taken but spaces is a valid entry..
          // --------------------------------------------
          break;
        default:
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

          var field = GetField(export.Export1.Item.Detail, "selectChar");

          field.Error = true;

          break;
      }
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (local.SelectCount.Count > 0 && (Equal(global.Command, "PREV") || Equal
      (global.Command, "NEXT")))
    {
      ExitState = "AA_PAGE001_W_NO_MULTI_SELECT";

      return;
    }
    else if (Equal(global.Command, "DISPLAY") && (
      !Equal(import.Hcase.Number, import.Case1.Number) || !
      Equal(import.HcsePerson.Number, import.CsePerson.Number)))
    {
      // --------------------------------------------
      // If a new CSE Person has been entered for
      // display, discard the previous Insurance
      // Coverage details.
      // --------------------------------------------
      export.HealthInsuranceCoverage.Identifier = 0;
    }

    if (Equal(global.Command, "RETCOMP") || Equal(global.Command, "RETNAME"))
    {
      if (!IsEmpty(export.SelectedCsePersonsWorkSet.Number))
      {
        if (!IsEmpty(export.CsePersonPrompt.SelectChar))
        {
          export.CsePerson.Number = export.SelectedCsePersonsWorkSet.Number;
          export.CsePersonsWorkSet.FormattedName =
            export.SelectedCsePersonsWorkSet.FormattedName;
          local.SearchCsePerson.Number =
            export.SelectedCsePersonsWorkSet.Number;
          export.CsePersonPrompt.SelectChar = "";
        }
        else
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (!IsEmpty(export.Export1.Item.PersonPrompt.SelectChar))
            {
              export.Export1.Update.Insured.Number =
                export.SelectedCsePersonsWorkSet.Number;
              export.Export1.Update.InsuredName.FormattedNameText =
                export.SelectedCsePersonsWorkSet.FormattedName;
              export.Export1.Update.InsuredH.Number =
                export.SelectedCsePersonsWorkSet.Number;
              export.Export1.Update.PersonPrompt.SelectChar = "";

              break;
            }
          }

          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (!IsEmpty(export.Export1.Item.PersonPrompt.SelectChar))
            {
              var field =
                GetField(export.Export1.Item.PersonPrompt, "selectChar");

              field.Error = true;
            }
          }

          return;
        }
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "RETHIPH"))
    {
      if (!IsEmpty(export.SelectedCsePerson.Number))
      {
        export.Case1.Number = export.SelectedCase.Number;
        export.CsePerson.Number = export.SelectedCsePerson.Number;
        export.CsePersonsWorkSet.FormattedName =
          export.SelectedCsePersonsWorkSet.FormattedName;
        MoveHealthInsuranceCoverage3(export.SelectedHealthInsuranceCoverage,
          export.HealthInsuranceCoverage);
        MoveContact(export.SelectedContact, export.Contact);
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "RETHIPL"))
    {
      if (!IsEmpty(export.SelectedCsePerson.Number))
      {
        export.Case1.Number = export.SelectedCase.Number;
        export.CsePerson.Number = export.SelectedCsePerson.Number;
        export.CsePersonsWorkSet.FormattedName =
          export.SelectedCsePersonsWorkSet.FormattedName;
      }

      export.WorkPromptCoverage.SelectChar = "";
      export.WorkPromptCoverage.SelectChar = "";
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "LIST") || Equal(global.Command, "RETHICL") || Equal
      (global.Command, "RETPCOL") || Equal(global.Command, "HIPH") || Equal
      (global.Command, "RETPCON"))
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

    if (Equal(global.Command, "CREATE") || Equal(global.Command, "UPDATE") || Equal
      (global.Command, "DELETE"))
    {
      if (AsChar(export.CaseOpen.Flag) == 'N')
      {
        ExitState = "CANNOT_MODIFY_CLOSED_CASE";

        return;
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "RETURN":
        if (Equal(export.Hidden.LastTran, "SRPT") || Equal
          (export.Hidden.LastTran, "SRPU"))
        {
          global.NextTran = (export.Hidden.LastTran ?? "") + " " + "XXNEXTXX";
        }
        else
        {
          ExitState = "ACO_NE0000_RETURN";

          return;
        }

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "HIPH":
        ExitState = "ECO_XFR_TO_HIPH";

        return;
      case "RETPCOL":
        if (export.SelectedContact.ContactNumber > 0)
        {
          MoveContact(export.SelectedContact, export.Contact);
        }

        export.WorkPromptContact.SelectChar = "";

        break;
      case "RETHICL":
        if (export.SelectedHealthInsuranceCompany.Identifier > 0)
        {
          MoveHealthInsuranceCompany(export.SelectedHealthInsuranceCompany,
            export.HealthInsuranceCompany);
        }

        break;
      case "DELETE":
        // ---------------------------------------------
        // If key value does not equal previous key value
        //      MAKE key value ERROR
        //      EXIT STATE is display before delete
        //      ESCAPE
        // ---------------------------------------------
        if (IsEmpty(export.HcsePerson.Number) || IsEmpty(export.Hcase.Number))
        {
          ExitState = "OE0012_DISP_REC_BEFORE_DELETE";

          return;
        }

        if (!Equal(export.Export1.Item.Insured.Number,
          export.Export1.Item.InsuredH.Number) || !
          Equal(export.Hcase.Number, export.Case1.Number) || !
          Equal(export.HcsePerson.Number, export.CsePerson.Number))
        {
          ExitState = "OE0012_DISP_REC_BEFORE_DELETE";

          return;
        }

        local.SelectCount.Count = 0;

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!IsEmpty(export.Export1.Item.Detail.SelectChar) && AsChar
            (export.Export1.Item.Detail.SelectChar) != 'S')
          {
            if (AsChar(export.Export1.Item.Detail.SelectChar) == '*')
            {
              // ----------------------------------------------------------------
              // No action is taken but * is a valid entry. It specifies that an
              // earlier action taken on the row was successful.
              // ---------------------------------------------------------------
              goto Test1;
            }

            var field = GetField(export.Export1.Item.Detail, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
          }
          else if (AsChar(export.Export1.Item.Detail.SelectChar) == 'S')
          {
            ++local.SelectCount.Count;
          }

Test1:
          ;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (local.SelectCount.Count == 0)
        {
          ExitState = "OE0176_CHILD_SUPPORT_DEL_NO_SEL";

          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (!IsEmpty(export.Export1.Item.Insured.Number) && IsEmpty
              (export.Export1.Item.Detail.SelectChar))
            {
              var field = GetField(export.Export1.Item.Detail, "selectChar");

              field.Error = true;
            }
          }

          return;
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.Detail.SelectChar) == 'S')
          {
            ++local.SelectCount.Count;

            // ---------------------------------------------
            // Verify that a display has been performed
            // before the delete can take place.
            // ---------------------------------------------
            ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";

            // ---------------------------------------------
            // Insert USE statement that calls "DELETE"
            // Action Block here.
            // ---------------------------------------------
            UseOeHicpDeletePersonalHins();

            // ---------------------------------------------
            // Remove confirmation message and confirmation
            // from the screen.
            // ---------------------------------------------
            if (!IsExitState("ACO_NI0000_SUCCESSFUL_DELETE"))
            {
              var field = GetField(export.Export1.Item.Detail, "selectChar");

              field.Error = true;

              export.Export1.Update.Detail.SelectChar = "S";

              return;
            }
            else
            {
              export.Export1.Update.Detail.SelectChar = "*";
              ExitState = "ACO_NN0000_ALL_OK";
            }
          }
        }

        ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";

        break;
      case "UPDATE":
        // ---------------------------------------------
        // If key value does not equal previous key value
        //      MAKE key value ERROR
        //      EXIT STATE IS display before update
        //      ESCAPE
        // ---------------------------------------------
        if (!Equal(export.Hcase.Number, export.Case1.Number) || IsEmpty
          (export.Hcase.Number) || !
          Equal(export.HcsePerson.Number, export.CsePerson.Number) || IsEmpty
          (export.HcsePerson.Number))
        {
          ExitState = "ACO_NE0000_MUST_DISPLAY_FIRST";

          return;
        }

        if (!Equal(export.Export1.Item.Insured.Number,
          export.Export1.Item.InsuredH.Number))
        {
          var field = GetField(export.Export1.Item.Insured, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_NEW_KEY_ON_UPDATE";

          return;
        }

        local.SelectCount.Count = 0;

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!IsEmpty(export.Export1.Item.Detail.SelectChar) && AsChar
            (export.Export1.Item.Detail.SelectChar) != 'S')
          {
            if (AsChar(export.Export1.Item.Detail.SelectChar) == '*')
            {
              // -------------------------------------------------------------------
              // No action is taken but * is a valid entry. It specifies that an
              // earlier action taken on the row was successful.
              // ---------------------------------------------------------------------
              goto Test2;
            }

            var field = GetField(export.Export1.Item.Detail, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
          }
          else if (AsChar(export.Export1.Item.Detail.SelectChar) == 'S')
          {
            ++local.SelectCount.Count;
          }

Test2:
          ;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (local.SelectCount.Count == 0)
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (!IsEmpty(export.Export1.Item.Insured.Number) && IsEmpty
              (export.Export1.Item.Detail.SelectChar))
            {
              var field = GetField(export.Export1.Item.Detail, "selectChar");

              field.Error = true;
            }
          }

          return;
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.Detail.SelectChar) == 'S')
          {
            // ----------------------------------------------------------------
            // Verify that a display has been performed before the update
            // can take place.
            // ----------------------------------------------------------------
            // ----------------------------------------------------------------
            // All Non_database validation should be done here.  Do all
            // validation as above.
            // ----------------------------------------------------------------
            if (Equal(export.Export1.Item.DetailsPersonalHealthInsurance.
              CoverageEndDate, null))
            {
              export.Export1.Update.DetailsPersonalHealthInsurance.
                CoverageEndDate = local.MaxDate.ExpirationDate;
            }

            // ***************************************************************************************
            // * CQ24156 - Removed the below mentioned exisint business edit 
            // related to monthly      *
            // *           premium amount and 
            // premium verified date.
            // 
            // *
            // *           New businenss edit is added, if amount field is 
            // spaces & premium verified *
            // *           date has value then error message will be displayed 
            // to the user.          *
            // ***************************************************************************************
            if (IsEmpty(export.Export1.Item.DetailsCovAmtTxt.Text7) && !
              Equal(export.Export1.Item.DetailsPersonalHealthInsurance.
                PremiumVerifiedDate, null))
            {
              var field1 =
                GetField(export.Export1.Item.DetailsCovAmtTxt, "text7");

              field1.Error = true;

              var field2 =
                GetField(export.Export1.Item.DetailsPersonalHealthInsurance,
                "premiumVerifiedDate");

              field2.Error = true;

              ExitState = "OE0000_INSURANCE_PREMIUM_REQD";
            }

            if (!Equal(export.Export1.Item.DetailsPersonalHealthInsurance.
              CoverageVerifiedDate,
              export.Export1.Item.H.CoverageVerifiedDate))
            {
              export.Export1.Update.DetailsPersonalHealthInsurance.
                AlertFlagInsuranceExistsInd = "Y";
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              goto Test8;
            }

            // ***************************************************************************************
            // * CQ24156 - Verify the Monthly Premium Amount (Text Field) for 
            // numeric vlaue and      *
            // *           Deimal values.
            // 
            // START   *
            // ***************************************************************************************
            if (Verify(export.Export1.Item.DetailsCovAmtTxt.Text7,
              " 0123456789.") != 0)
            {
              var field =
                GetField(export.Export1.Item.DetailsCovAmtTxt, "text7");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_DATA_ENTERED";
            }
            else
            {
              local.DotCount.Count = 0;
              local.NoOfDigitsAfterDot.Count = 0;
              local.FirstDotPosition.Count = 0;
              local.DigitStartPosition.Count = 0;
              local.StringLength.Count =
                Length(TrimEnd(export.Export1.Item.DetailsCovAmtTxt.Text7));
              local.Common.Count = 1;

              for(var limit = local.StringLength.Count; local.Common.Count <= limit
                ; ++local.Common.Count)
              {
                local.CurrentPositionValue.Text1 =
                  Substring(export.Export1.Item.DetailsCovAmtTxt.Text7,
                  local.Common.Count, 1);

                if (IsEmpty(local.CurrentPositionValue.Text1) && local
                  .DigitStartPosition.Count == 0 && local.DotCount.Count == 0)
                {
                  continue;
                }

                if (IsEmpty(local.CurrentPositionValue.Text1) && (
                  local.DigitStartPosition.Count > 0 || local.DotCount.Count > 0
                  ))
                {
                  if (Verify(Substring(
                    export.Export1.Item.DetailsCovAmtTxt.Text7,
                    WorkArea.Text7_MaxLength, local.Common.Count +
                    1, local.StringLength.Count -
                    local.Common.Count), "01234567890.") == 0)
                  {
                    var field =
                      GetField(export.Export1.Item.DetailsCovAmtTxt, "text7");

                    field.Error = true;

                    ExitState = "ACO_NE0000_INVALID_DATA_ENTERED";

                    goto Test3;
                  }
                }

                if (Verify(local.CurrentPositionValue.Text1, "0123456789") == 0
                  && local.DigitStartPosition.Count == 0)
                {
                  local.DigitStartPosition.Count = local.Common.Count;
                }

                if (AsChar(local.CurrentPositionValue.Text1) == '.')
                {
                  if (local.DotCount.Count == 0)
                  {
                    local.FirstDotPosition.Count = local.Common.Count;
                  }

                  ++local.DotCount.Count;
                }

                if (local.DotCount.Count > 0)
                {
                  if (local.DotCount.Count > 1)
                  {
                    var field =
                      GetField(export.Export1.Item.DetailsCovAmtTxt, "text7");

                    field.Error = true;

                    ExitState = "ACO_NE0000_INVALID_DATA_ENTERED";

                    goto Test3;
                  }

                  if (local.Common.Count > local.FirstDotPosition.Count)
                  {
                    if (Verify(local.CurrentPositionValue.Text1, "0123456789") ==
                      0)
                    {
                      ++local.NoOfDigitsAfterDot.Count;
                    }
                  }
                }

                if (local.NoOfDigitsAfterDot.Count > 2)
                {
                  var field =
                    GetField(export.Export1.Item.DetailsCovAmtTxt, "text7");

                  field.Error = true;

                  ExitState = "ACO_NE0000_INVALID_DATA_ENTERED";

                  goto Test3;
                }
              }

              if (local.NoOfDigitsAfterDot.Count > 2)
              {
                var field =
                  GetField(export.Export1.Item.DetailsCovAmtTxt, "text7");

                field.Error = true;

                ExitState = "ACO_NE0000_INVALID_DATA_ENTERED";
              }
            }

Test3:

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              goto Test8;
            }

            if (Verify(TrimEnd(export.Export1.Item.DetailsCovAmtTxt.Text7),
              " 0123456789.") != 0)
            {
              ExitState = "ACO_NE0000_INVALID_DATA_ENTERED";

              goto Test8;
            }

            if (IsEmpty(export.Export1.Item.DetailsCovAmtTxt.Text7))
            {
              local.BatchConvertNumToText.Currency = 0M;
            }
            else
            {
              local.BatchConvertNumToText.Currency =
                StringToNumber(export.Export1.Item.DetailsCovAmtTxt.Text7);

              if (local.BatchConvertNumToText.Currency == 0 && Equal
                (export.Export1.Item.DetailsPersonalHealthInsurance.
                  PremiumVerifiedDate, null))
              {
                export.Export1.Update.DetailsCovAmtTxt.Text7 = "";
                local.BatchConvertNumToText.Currency = 0;

                goto Test4;
              }

              // ***************************************************************************************
              // * When CA:GEN converts text to number ignores the decimal 
              // places and make it as whole *
              // * number, so, we need divide the value by 100 to get the 
              // decimal values.              *
              // ***************************************************************************************
              if (local.BatchConvertNumToText.Currency > 0 && local
                .FirstDotPosition.Count > 0)
              {
                local.BatchConvertNumToText.Currency =
                  local.BatchConvertNumToText.Currency / 100;
              }

              UseFnConvertCurrencyToText();
              export.Export1.Update.DetailsCovAmtTxt.Text7 =
                Substring(local.BatchConvertNumToText.TextNumber16, 1, 7);
            }

Test4:

            export.Export1.Update.DetailsPersonalHealthInsurance.
              CoverageCostAmount = local.BatchConvertNumToText.Currency;

            // ***************************************************************************************
            // * CQ24156 - Verify the Monthly Premium Amount (Text Field) for 
            // numeric vlaue and      *
            // *           Deimal values.
            // 
            // END     *
            // ***************************************************************************************
            // ----------------------------------------------------------------
            // Verify all the date values entered.  Either the insurance
            // policy number or the group policy number must be entered.
            // ----------------------------------------------------------------
            if (!Equal(export.Export1.Item.DetailsPersonalHealthInsurance.
              CoverageVerifiedDate, null) && Lt
              (Now().Date,
              export.Export1.Item.DetailsPersonalHealthInsurance.
                CoverageVerifiedDate))
            {
              var field =
                GetField(export.Export1.Item.DetailsPersonalHealthInsurance,
                "coverageVerifiedDate");

              field.Error = true;

              ExitState = "OE0182_INVALID_VERIFIED_DATE";
            }

            if (Lt(export.Export1.Item.DetailsPersonalHealthInsurance.
              CoverageEndDate,
              export.Export1.Item.DetailsPersonalHealthInsurance.
                CoverageBeginDate))
            {
              var field1 =
                GetField(export.Export1.Item.DetailsPersonalHealthInsurance,
                "coverageEndDate");

              field1.Error = true;

              var field2 =
                GetField(export.Export1.Item.DetailsPersonalHealthInsurance,
                "coverageBeginDate");

              field2.Error = true;

              ExitState = "CO0000_INVALID_EFF_OR_EXP_DATE";
            }

            if (Equal(export.Export1.Item.DetailsPersonalHealthInsurance.
              CoverageEndDate, null))
            {
              export.Export1.Update.DetailsPersonalHealthInsurance.
                CoverageEndDate = local.MaxDate.ExpirationDate;
            }
            else if (Lt(export.Export1.Item.DetailsPersonalHealthInsurance.
              CoverageEndDate, Now().Date))
            {
            }
            else if (Lt(export.Export1.Item.DetailsPersonalHealthInsurance.
              CoverageVerifiedDate,
              export.Export1.Item.DetailsPersonalHealthInsurance.
                CoverageBeginDate))
            {
              var field =
                GetField(export.Export1.Item.DetailsPersonalHealthInsurance,
                "coverageVerifiedDate");

              field.Error = true;

              ExitState = "OE0182_INVALID_VERIFIED_DATE";
            }

            if (Equal(export.Export1.Item.DetailsPersonalHealthInsurance.
              CoverageBeginDate, null))
            {
              var field =
                GetField(export.Export1.Item.DetailsPersonalHealthInsurance,
                "coverageBeginDate");

              field.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
            }

            if (!Equal(export.Export1.Item.DetailsPersonalHealthInsurance.
              PremiumVerifiedDate, null) && (
                Lt(Now().Date,
              export.Export1.Item.DetailsPersonalHealthInsurance.
                PremiumVerifiedDate) || Lt
              (export.Export1.Item.DetailsPersonalHealthInsurance.
                PremiumVerifiedDate,
              export.Export1.Item.DetailsPersonalHealthInsurance.
                CoverageBeginDate)))
            {
              var field =
                GetField(export.Export1.Item.DetailsPersonalHealthInsurance,
                "premiumVerifiedDate");

              field.Error = true;

              ExitState = "OE0183_PREMIUM_VERIFIED_DATE";
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              goto Test8;
            }

            // ---------------------------------------------
            // Insert the USE statement here to call the
            // UPDATE action block.
            // ---------------------------------------------
            UseOeHicpUpdatePersonalHins();

            if (IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE"))
            {
              export.Export1.Update.Detail.SelectChar = "*";
              ExitState = "ACO_NN0000_ALL_OK";

              // 10/23/2008	J Huss	Moved check for AR being the policy holder 
              // into SP_CAB_DETERMINE_INTERSTATE_DOC
              if (ReadCaseRole2())
              {
                local.IsClient.Flag = "Y";
              }
              else
              {
                local.IsClient.Flag = "N";
              }

              // mjr
              // ----------------------------------------------
              // 01/29/1999
              // Added creation of document trigger.
              // -----------------------------------------------------------
              // 08/19/08	J.Huss	Changed so case number is passed when document 
              // trigger is created.
              if (AsChar(local.IsClient.Flag) == 'Y')
              {
                local.Document.Name = "INSUINFO";
                local.SpDocKey.KeyChild = export.Export1.Item.Insured.Number;
                local.SpDocKey.KeyHealthInsCoverage =
                  export.HealthInsuranceCoverage.Identifier;
                local.SpDocKey.KeyCase = import.Case1.Number;
                local.FindDoc.Flag = "Y";

                // mca
                // ----------------------------------------------
                // 12/17/2001
                // 12/17/01 M.Ashworth       PR133485. Added cab to send 
                // document to other
                //                           state if the case is interstate.
                // -----------------------------------------------------------
                UseSpCabDetermineInterstateDoc();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  return;
                }
              }
            }
            else
            {
              // *** October 12, 1999  David Lowry
              // Added this case of to set the correct error message.
              // PRH75900.
              switch(local.Common.Count)
              {
                case 1:
                  var field1 =
                    GetField(export.Export1.Item.DetailsPersonalHealthInsurance,
                    "coverageBeginDate");

                  field1.Error = true;

                  ExitState = "OE0000_COVERAGE_EFF_DATE_ERROR";

                  break;
                case 2:
                  var field2 =
                    GetField(export.Export1.Item.DetailsPersonalHealthInsurance,
                    "coverageEndDate");

                  field2.Error = true;

                  ExitState = "OE0000_COVERAGE_END_DATE_ERROR";

                  break;
                case 3:
                  var field3 =
                    GetField(export.Export1.Item.DetailsPersonalHealthInsurance,
                    "coverageEndDate");

                  field3.Error = true;

                  var field4 =
                    GetField(export.Export1.Item.DetailsPersonalHealthInsurance,
                    "coverageBeginDate");

                  field4.Error = true;

                  ExitState = "OE0000_COVERAGE_DATES_ERROR";

                  break;
                default:
                  break;
              }

              export.Export1.Update.Detail.SelectChar = "S";

              var field = GetField(export.Export1.Item.Detail, "selectChar");

              field.Error = true;

              return;
            }
          }
        }

        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

        break;
      case "CREATE":
        // -------------------------------------------------------------
        // Verify that a display has been performed before the update
        // can take place.
        // -------------------------------------------------------------
        if (export.SelectedHealthInsuranceCoverage.Identifier == 0)
        {
          ExitState = "OE0000_DISPLAY_COVERAGE_FIRST";

          return;
        }

        if (!Equal(export.Hcase.Number, export.Case1.Number) || !
          Equal(export.HcsePerson.Number, export.CsePerson.Number))
        {
          ExitState = "ACO_NE0000_DISPLAY_REQD_NEW_SRCH";

          return;
        }

        local.SelectCount.Count = 0;

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!IsEmpty(export.Export1.Item.Detail.SelectChar) && AsChar
            (export.Export1.Item.Detail.SelectChar) != 'S')
          {
            if (AsChar(export.Export1.Item.Detail.SelectChar) == '*')
            {
              // ---------------------------------------------
              // No action is taken but * is a valid entry.
              // It specifies that an earlier action taken on
              // the row was successful.
              // --------------------------------------------
              goto Test5;
            }

            var field = GetField(export.Export1.Item.Detail, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
          }
          else if (AsChar(export.Export1.Item.Detail.SelectChar) == 'S')
          {
            ++local.SelectCount.Count;
          }

Test5:
          ;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (local.SelectCount.Count == 0)
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (!IsEmpty(export.Export1.Item.Insured.Number) && IsEmpty
              (export.Export1.Item.Detail.SelectChar))
            {
              var field = GetField(export.Export1.Item.Detail, "selectChar");

              field.Error = true;
            }
          }

          return;
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.Detail.SelectChar) == 'S')
          {
            // ---------------------------------------------
            // All Non-Database validation should be done
            // here.  Do all validation as above.
            // ---------------------------------------------
            // ---------------------------------------------
            // The Case Number is required.
            // ---------------------------------------------
            if (IsEmpty(import.Case1.Number))
            {
              var field = GetField(export.Case1, "number");

              field.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
            }
            else
            {
              UseOeCabCheckCaseMember1();

              if (!IsEmpty(local.WorkError.Flag))
              {
                var field = GetField(export.Case1, "number");

                field.Error = true;

                ExitState = "CASE_NF";
              }
            }

            // ---------------------------------------------
            // The CSE Person Number is required.
            // ---------------------------------------------
            if (IsEmpty(export.Export1.Item.Insured.Number))
            {
              var field = GetField(export.Export1.Item.Insured, "number");

              field.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
            }
            else
            {
              UseOeCabCheckCaseMember6();

              if (!IsEmpty(local.WorkError.Flag))
              {
                var field = GetField(export.Export1.Item.Insured, "number");

                field.Error = true;

                export.Export1.Update.InsuredName.FormattedNameText = "";
                ExitState = "CSE_PERSON_NF";
              }
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            if (!IsEmpty(import.Case1.Number) && !
              IsEmpty(export.Export1.Item.Insured.Number))
            {
              UseOeCabCheckCaseMember2();

              if (!IsEmpty(local.WorkError.Flag))
              {
                var field1 = GetField(export.Case1, "number");

                field1.Error = true;

                var field2 = GetField(export.Export1.Item.Insured, "number");

                field2.Error = true;

                ExitState = "OE0000_CASE_MEMBER_NE";
              }
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            // -----------------------------------------------------------------------------
            // PR# 89931 :    Only a child (Case_Role=CH) can be added as an '
            // insured_person' on HICP screen. Give an error message if user try
            // to add an 'AP' or 'AR'.
            // ------------------------------------------------------------------------------
            if (!IsEmpty(export.Export1.Item.Insured.Number) && !
              IsEmpty(import.Case1.Number))
            {
              if (ReadCaseRole1())
              {
                if (Equal(entities.CaseRole.Type1, "AP") || Equal
                  (entities.CaseRole.Type1, "AR"))
                {
                  var field1 =
                    GetField(export.Export1.Item.Detail, "selectChar");

                  field1.Error = true;

                  var field2 = GetField(export.Export1.Item.Insured, "number");

                  field2.Error = true;

                  if (IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    ExitState = "ACO_NE0000_ADD_CHILD_ROLE_ONLY";
                  }
                }
              }
              else
              {
                var field1 = GetField(export.Export1.Item.Detail, "selectChar");

                field1.Error = true;

                var field2 = GetField(export.Export1.Item.Insured, "number");

                field2.Error = true;

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  ExitState = "CASE_ROLE_NF";
                }
              }
            }

            // ---------------------------------------------
            // Do all edit Checks for the record to be added.
            // ---------------------------------------------
            if (Equal(export.Export1.Item.DetailsPersonalHealthInsurance.
              CoverageEndDate, null))
            {
              export.Export1.Update.DetailsPersonalHealthInsurance.
                CoverageEndDate = local.MaxDate.ExpirationDate;
            }

            // ***************************************************************************************
            // * CQ24156 - Removed the below mentioned exisint business edit 
            // related to monthly      *
            // *           premium amount and 
            // premium verified date.
            // 
            // *
            // *           New businenss edit is added, if amount field is 
            // spaces & premium verified *
            // *           date has value then error message will be displayed 
            // to the user.          *
            // ***************************************************************************************
            if (IsEmpty(export.Export1.Item.DetailsCovAmtTxt.Text7) && !
              Equal(export.Export1.Item.DetailsPersonalHealthInsurance.
                PremiumVerifiedDate, null))
            {
              var field1 =
                GetField(export.Export1.Item.DetailsCovAmtTxt, "text7");

              field1.Error = true;

              var field2 =
                GetField(export.Export1.Item.DetailsPersonalHealthInsurance,
                "premiumVerifiedDate");

              field2.Error = true;

              ExitState = "OE0000_INSURANCE_PREMIUM_REQD";
            }

            if (!Equal(export.Export1.Item.DetailsPersonalHealthInsurance.
              CoverageVerifiedDate, null))
            {
              if (Lt(Now().Date,
                export.Export1.Item.DetailsPersonalHealthInsurance.
                  CoverageVerifiedDate))
              {
                ExitState = "OE0182_INVALID_VERIFIED_DATE";
              }
              else
              {
                // *********************************************
                // The coverage verified date has been added.
                // Call CAB to trigger letter to MEDI and AR
                // that insurance exists.
                // *********************************************
                export.Export1.Update.DetailsPersonalHealthInsurance.
                  AlertFlagInsuranceExistsInd = "Y";
              }
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              goto Test8;
            }

            // ***************************************************************************************
            // * CQ24156 - Verify the Monthly Premium Amount (Text Field) for 
            // numeric vlaue and      *
            // *           Deimal values.
            // 
            // START   *
            // ***************************************************************************************
            if (Verify(export.Export1.Item.DetailsCovAmtTxt.Text7,
              " 0123456789.") != 0)
            {
              var field =
                GetField(export.Export1.Item.DetailsCovAmtTxt, "text7");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_DATA_ENTERED";
            }
            else
            {
              local.DotCount.Count = 0;
              local.NoOfDigitsAfterDot.Count = 0;
              local.FirstDotPosition.Count = 0;
              local.DigitStartPosition.Count = 0;
              local.StringLength.Count =
                Length(TrimEnd(export.Export1.Item.DetailsCovAmtTxt.Text7));
              local.Common.Count = 1;

              for(var limit = local.StringLength.Count; local.Common.Count <= limit
                ; ++local.Common.Count)
              {
                local.CurrentPositionValue.Text1 =
                  Substring(export.Export1.Item.DetailsCovAmtTxt.Text7,
                  local.Common.Count, 1);

                if (IsEmpty(local.CurrentPositionValue.Text1) && local
                  .DigitStartPosition.Count == 0 && local.DotCount.Count == 0)
                {
                  continue;
                }

                if (IsEmpty(local.CurrentPositionValue.Text1) && (
                  local.DigitStartPosition.Count > 0 || local.DotCount.Count > 0
                  ))
                {
                  if (Verify(Substring(
                    export.Export1.Item.DetailsCovAmtTxt.Text7,
                    WorkArea.Text7_MaxLength, local.Common.Count +
                    1, local.StringLength.Count -
                    local.Common.Count), "01234567890.") == 0)
                  {
                    var field =
                      GetField(export.Export1.Item.DetailsCovAmtTxt, "text7");

                    field.Error = true;

                    ExitState = "ACO_NE0000_INVALID_DATA_ENTERED";

                    goto Test6;
                  }
                }

                if (Verify(local.CurrentPositionValue.Text1, "0123456789") == 0
                  && local.DigitStartPosition.Count == 0)
                {
                  local.DigitStartPosition.Count = local.Common.Count;
                }

                if (AsChar(local.CurrentPositionValue.Text1) == '.')
                {
                  if (local.DotCount.Count == 0)
                  {
                    local.FirstDotPosition.Count = local.Common.Count;
                  }

                  ++local.DotCount.Count;
                }

                if (local.DotCount.Count > 0)
                {
                  if (local.DotCount.Count > 1)
                  {
                    var field =
                      GetField(export.Export1.Item.DetailsCovAmtTxt, "text7");

                    field.Error = true;

                    ExitState = "ACO_NE0000_INVALID_DATA_ENTERED";

                    goto Test6;
                  }

                  if (local.Common.Count > local.FirstDotPosition.Count)
                  {
                    if (Verify(local.CurrentPositionValue.Text1, "0123456789") ==
                      0)
                    {
                      ++local.NoOfDigitsAfterDot.Count;
                    }
                  }
                }

                if (local.NoOfDigitsAfterDot.Count > 2)
                {
                  var field =
                    GetField(export.Export1.Item.DetailsCovAmtTxt, "text7");

                  field.Error = true;

                  ExitState = "ACO_NE0000_INVALID_DATA_ENTERED";

                  goto Test6;
                }
              }

              if (local.NoOfDigitsAfterDot.Count > 2)
              {
                var field =
                  GetField(export.Export1.Item.DetailsCovAmtTxt, "text7");

                field.Error = true;

                ExitState = "ACO_NE0000_INVALID_DATA_ENTERED";
              }
            }

Test6:

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              goto Test8;
            }

            if (Verify(TrimEnd(export.Export1.Item.DetailsCovAmtTxt.Text7),
              " 0123456789.") != 0)
            {
              ExitState = "ACO_NE0000_INVALID_DATA_ENTERED";

              goto Test8;
            }

            if (IsEmpty(export.Export1.Item.DetailsCovAmtTxt.Text7))
            {
              local.BatchConvertNumToText.Currency = 0M;
            }
            else
            {
              local.BatchConvertNumToText.Currency =
                StringToNumber(export.Export1.Item.DetailsCovAmtTxt.Text7);

              if (local.BatchConvertNumToText.Currency == 0 && Equal
                (export.Export1.Item.DetailsPersonalHealthInsurance.
                  PremiumVerifiedDate, null))
              {
                export.Export1.Update.DetailsCovAmtTxt.Text7 = "";
                local.BatchConvertNumToText.Currency = 0;

                goto Test7;
              }

              // ***************************************************************************************
              // * When CA:GEN converts text to number ignores the decimal 
              // places and make it as whole *
              // * number, so, we need divide the value by 100 to get the 
              // decimal values.              *
              // ***************************************************************************************
              if (local.BatchConvertNumToText.Currency > 0 && local
                .FirstDotPosition.Count > 0)
              {
                local.BatchConvertNumToText.Currency =
                  local.BatchConvertNumToText.Currency / 100;
              }

              UseFnConvertCurrencyToText();
              export.Export1.Update.DetailsCovAmtTxt.Text7 =
                Substring(local.BatchConvertNumToText.TextNumber16, 1, 7);
            }

Test7:

            export.Export1.Update.DetailsPersonalHealthInsurance.
              CoverageCostAmount = local.BatchConvertNumToText.Currency;

            // ***************************************************************************************
            // * CQ24156 - Verify the Monthly Premium Amount (Text Field) for 
            // numeric vlaue and      *
            // *           Deimal values.
            // 
            // END     *
            // ***************************************************************************************
            // ---------------------------------------------
            // Verify all the date values entered.
            // Either the insurance policy number or the
            // group policy number must be entered.
            // ---------------------------------------------
            if (!Equal(export.Export1.Item.DetailsPersonalHealthInsurance.
              CoverageVerifiedDate, null) && Lt
              (Now().Date,
              export.Export1.Item.DetailsPersonalHealthInsurance.
                CoverageVerifiedDate))
            {
              var field =
                GetField(export.Export1.Item.DetailsPersonalHealthInsurance,
                "coverageVerifiedDate");

              field.Error = true;

              ExitState = "OE0182_INVALID_VERIFIED_DATE";
            }

            if (Lt(export.Export1.Item.DetailsPersonalHealthInsurance.
              CoverageEndDate,
              export.Export1.Item.DetailsPersonalHealthInsurance.
                CoverageBeginDate))
            {
              var field1 =
                GetField(export.Export1.Item.DetailsPersonalHealthInsurance,
                "coverageEndDate");

              field1.Error = true;

              var field2 =
                GetField(export.Export1.Item.DetailsPersonalHealthInsurance,
                "coverageBeginDate");

              field2.Error = true;

              ExitState = "CO0000_INVALID_EFF_OR_EXP_DATE";
            }

            if (Equal(export.Export1.Item.DetailsPersonalHealthInsurance.
              CoverageEndDate, null))
            {
              export.Export1.Update.DetailsPersonalHealthInsurance.
                CoverageEndDate = local.MaxDate.ExpirationDate;
            }
            else if (Lt(export.Export1.Item.DetailsPersonalHealthInsurance.
              CoverageEndDate, Now().Date))
            {
            }
            else if (Lt(export.Export1.Item.DetailsPersonalHealthInsurance.
              CoverageVerifiedDate,
              export.Export1.Item.DetailsPersonalHealthInsurance.
                CoverageBeginDate))
            {
              var field =
                GetField(export.Export1.Item.DetailsPersonalHealthInsurance,
                "coverageVerifiedDate");

              field.Error = true;

              ExitState = "OE0182_INVALID_VERIFIED_DATE";
            }

            if (Equal(export.Export1.Item.DetailsPersonalHealthInsurance.
              CoverageBeginDate, null))
            {
              var field =
                GetField(export.Export1.Item.DetailsPersonalHealthInsurance,
                "coverageBeginDate");

              field.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
            }

            if (!Equal(export.Export1.Item.DetailsPersonalHealthInsurance.
              PremiumVerifiedDate, null) && (
                Lt(Now().Date,
              export.Export1.Item.DetailsPersonalHealthInsurance.
                PremiumVerifiedDate) || Lt
              (export.Export1.Item.DetailsPersonalHealthInsurance.
                PremiumVerifiedDate,
              export.Export1.Item.DetailsPersonalHealthInsurance.
                CoverageBeginDate)))
            {
              var field =
                GetField(export.Export1.Item.DetailsPersonalHealthInsurance,
                "premiumVerifiedDate");

              field.Error = true;

              ExitState = "OE0183_PREMIUM_VERIFIED_DATE";
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              goto Test8;
            }

            UseOeHicpCreatePersonalHins();

            // ---------------------------------------------
            // Begin code - Raju : 12/19/1996 0335 CST
            // ---------------------------------------------
            export.Export1.Update.HiddenLastRead.CoverageVerifiedDate = null;

            // ---------------------------------------------
            // End   code - Raju : 12/19/1996 0335 CST
            // ---------------------------------------------
            if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
            {
              export.Export1.Update.Detail.SelectChar = "*";
              ExitState = "ACO_NN0000_ALL_OK";

              // 10/23/2008	J Huss	Moved check for AR being the policy holder 
              // into SP_CAB_DETERMINE_INTERSTATE_DOC
              // -------------------------------------------------------------------
              // Dont create document if the AR is an organization.
              // -------------------------------------------------------------------
              if (ReadCaseRole2())
              {
                local.IsClient.Flag = "Y";
              }
              else
              {
                local.IsClient.Flag = "N";
              }

              // mjr
              // ----------------------------------------------
              // 01/29/1999
              // Added creation of document trigger.
              // -----------------------------------------------------------
              // 08/19/08	J.Huss	Changed so case number is passed when document 
              // trigger is created.
              if (AsChar(local.IsClient.Flag) == 'Y')
              {
                local.Document.Name = "INSUINFO";
                local.FindDoc.Flag = "N";
                local.SpDocKey.KeyChild = export.Export1.Item.Insured.Number;
                local.SpDocKey.KeyHealthInsCoverage =
                  export.HealthInsuranceCoverage.Identifier;
                local.SpDocKey.KeyCase = import.Case1.Number;

                // mca
                // ----------------------------------------------
                // 12/17/2001
                // 12/17/01 M.Ashworth       PR133485. Added cab to send 
                // document to other
                //                           state if the case is interstate.  
                // Calling this cab will check if document already created where
                // it was not before.  This will be helpfull if the user
                // decides to delete an entry and re-add it.
                // -----------------------------------------------------------
                UseSpCabDetermineInterstateDoc();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  return;
                }
              }
            }
            else
            {
              // *** October 10, 1999  David Lowry
              // Added this case of to set the correct error message.
              switch(local.Common.Count)
              {
                case 1:
                  var field1 =
                    GetField(export.Export1.Item.DetailsPersonalHealthInsurance,
                    "coverageBeginDate");

                  field1.Error = true;

                  ExitState = "OE0000_COVERAGE_EFF_DATE_ERROR";

                  break;
                case 2:
                  var field2 =
                    GetField(export.Export1.Item.DetailsPersonalHealthInsurance,
                    "coverageEndDate");

                  field2.Error = true;

                  ExitState = "OE0000_COVERAGE_END_DATE_ERROR";

                  break;
                case 3:
                  var field3 =
                    GetField(export.Export1.Item.DetailsPersonalHealthInsurance,
                    "coverageEndDate");

                  field3.Error = true;

                  var field4 =
                    GetField(export.Export1.Item.DetailsPersonalHealthInsurance,
                    "coverageBeginDate");

                  field4.Error = true;

                  ExitState = "OE0000_COVERAGE_DATES_ERROR";

                  break;
                default:
                  break;
              }

              export.Export1.Update.Detail.SelectChar = "S";

              var field = GetField(export.Export1.Item.Detail, "selectChar");

              field.Error = true;

              return;
            }
          }
        }

        ExitState = "ACO_NI0000_SUCCESSFUL_ADD";

        break;
      case "NEXT":
        ExitState = "ACO_NE0000_INVALID_FORWARD";

        return;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        return;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      case "LIST":
        local.SelectCount.Count = 0;

        if (!IsEmpty(export.CsePersonPrompt.SelectChar) && AsChar
          (export.CsePersonPrompt.SelectChar) != 'S')
        {
          var field = GetField(export.CsePersonPrompt, "selectChar");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
          }
        }
        else if (AsChar(export.CsePersonPrompt.SelectChar) == 'S')
        {
          ++local.SelectCount.Count;
        }

        if (!IsEmpty(export.WorkPromptContact.SelectChar) && AsChar
          (export.WorkPromptContact.SelectChar) != 'S')
        {
          var field = GetField(export.WorkPromptContact, "selectChar");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
          }
        }
        else if (AsChar(export.WorkPromptContact.SelectChar) == 'S')
        {
          ++local.SelectCount.Count;
        }

        if (!IsEmpty(export.WorkPromptCoverage.SelectChar) && AsChar
          (export.WorkPromptCoverage.SelectChar) != 'S')
        {
          var field = GetField(export.WorkPromptCoverage, "selectChar");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
          }
        }
        else if (AsChar(export.WorkPromptCoverage.SelectChar) == 'S')
        {
          ++local.SelectCount.Count;
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!IsEmpty(export.Export1.Item.PersonPrompt.SelectChar) && AsChar
            (export.Export1.Item.PersonPrompt.SelectChar) != 'S')
          {
            var field =
              GetField(export.Export1.Item.PersonPrompt, "selectChar");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            }
          }
          else if (AsChar(export.Export1.Item.PersonPrompt.SelectChar) == 'S')
          {
            ++local.SelectCount.Count;
          }
        }

        if (local.SelectCount.Count == 0 && IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field1 = GetField(export.CsePersonPrompt, "selectChar");

          field1.Error = true;

          var field2 = GetField(export.WorkPromptContact, "selectChar");

          field2.Error = true;

          var field3 = GetField(export.WorkPromptCoverage, "selectChar");

          field3.Error = true;

          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            var field =
              GetField(export.Export1.Item.PersonPrompt, "selectChar");

            field.Error = true;
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (AsChar(import.CsePersonPrompt.SelectChar) == 'S')
        {
          if (!IsEmpty(export.Case1.Number))
          {
            ExitState = "ECO_LNK_TO_CASE_COMPOSITION";
          }
          else
          {
            ExitState = "ECO_LNK_TO_CSE_NAME_LIST";
          }

          return;
        }

        if (AsChar(import.WorkPromptContact.SelectChar) == 'S')
        {
          if (IsEmpty(export.CsePerson.Number) || export
            .Contact.ContactNumber == 0)
          {
            if (IsEmpty(export.Case1.Number))
            {
              var field = GetField(export.CsePerson, "number");

              field.Error = true;
            }

            ExitState = "OE0184_PERSON_NO_REQD_FOR_PCON";
          }
          else
          {
            export.WorkPromptInd.Flag = "Y";
            export.WorkPromptContact.SelectChar = "";
            ExitState = "ECO_LNK_TO_PCON";
          }

          return;
        }

        if (AsChar(import.WorkPromptCoverage.SelectChar) == 'S')
        {
          export.WorkPromptInd.Flag = "Y";
          ExitState = "ECO_LNK_TO_LIST_INSURANCE_COVERA";

          return;
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.PersonPrompt.SelectChar) == 'S')
          {
            if (!IsEmpty(export.Case1.Number))
            {
              ExitState = "ECO_LNK_TO_CASE_COMPOSITION";
            }
            else
            {
              var field = GetField(export.Case1, "number");

              field.Error = true;

              ExitState = "OE0000_CASE_NUMBER_REQD_FOR_COMP";
            }

            return;
          }
        }

        break;
      case "DISPLAY":
        // ---------------------------------------------
        // Verify that all mandatory fields for a
        // display have been entered.
        // ---------------------------------------------
        // -------------------------------------------------------------
        // On a display the user enteres the Person Number and Case
        // ID.  If the person is a policy holder for more than one
        // insurance policy, the user will automatically be taken
        // to a list screen so that he can select the insurance record
        // that he wants to display.
        // ---------------------------------------------------------------
        // ---------------------------------------------
        // The CSE Person Number is required.
        // ---------------------------------------------
        if (IsEmpty(export.CsePerson.Number))
        {
          var field = GetField(export.CsePerson, "number");

          field.Error = true;

          export.CsePersonsWorkSet.FormattedName = "";
          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }

        // ---------------------------------------------
        // The Case Number is required.
        // ---------------------------------------------
        if (IsEmpty(export.Case1.Number))
        {
          var field = GetField(export.Case1, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // ---------------------------------------------
        // CSE_Person Number
        // ---------------------------------------------
        UseOeCabCheckCaseMember5();

        if (!IsEmpty(local.WorkError.Flag))
        {
          export.CsePersonsWorkSet.FormattedName = "";

          var field = GetField(export.CsePerson, "number");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "CSE_PERSON_NF";
          }
        }

        // ---------------------------------------------
        // Validate Case Number
        // ---------------------------------------------
        UseOeCabCheckCaseMember4();

        if (!IsEmpty(local.WorkError.Flag))
        {
          var field = GetField(export.Case1, "number");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "CASE_NF";
          }
        }
        else
        {
          export.Next.Number = export.Case1.Number;
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (!IsEmpty(export.Case1.Number) && !
            IsEmpty(export.CsePerson.Number))
          {
            UseOeCabCheckCaseMember3();

            if (!IsEmpty(local.WorkError.Flag))
            {
              var field1 = GetField(export.Case1, "number");

              field1.Error = true;

              var field2 = GetField(export.CsePerson, "number");

              field2.Error = true;

              ExitState = "OE0000_CASE_MEMBER_NE";
            }
          }
        }

        UseSiReadOfficeOspHeader();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.SelectedHealthInsuranceCompany.Assign(
            local.RefreshHealthInsuranceCompany);
          export.HealthInsuranceCompany.Assign(
            local.RefreshHealthInsuranceCompany);
          MoveHealthInsuranceCoverage3(local.RefreshHealthInsuranceCoverage,
            export.HealthInsuranceCoverage);
          export.SelectedHealthInsuranceCoverage.Assign(
            local.RefreshHealthInsuranceCoverage);
          MoveContact(local.RefreshContact, export.Contact);

          if (!export.Export1.IsEmpty)
          {
            for(export.Export1.Index = 0; export.Export1.Index < export
              .Export1.Count; ++export.Export1.Index)
            {
              export.Export1.Update.Detail.SelectChar =
                local.RefreshCommon.SelectChar;
              export.Export1.Update.DetailsCaseRole.Type1 =
                local.RefreshCaseRole.Type1;
              export.Export1.Update.Insured.Number =
                local.RefreshCsePerson.Number;
              export.Export1.Update.InsuredH.Number =
                local.RefreshCsePerson.Number;
              export.Export1.Update.DetailsPersonalHealthInsurance.Assign(
                local.RefreshPersonalHealthInsurance);
              export.Export1.Update.InsuredName.FormattedNameText =
                local.RefreshOeWorkGroup.FormattedNameText;
              export.Export1.Update.H.CoverageVerifiedDate =
                local.RefreshPersonalHealthInsurance.CoverageVerifiedDate;
              export.Export1.Update.HiddenLastRead.CoverageVerifiedDate =
                local.RefreshPersonalHealthInsurance.CoverageVerifiedDate;
              export.Export1.Update.PersonPrompt.SelectChar =
                local.RefreshCommon.SelectChar;
              export.Export1.Update.DetailsCovAmtTxt.Text7 =
                local.RefreshCovAmtTxt.Text7;
            }
          }

          return;
        }

        // -------------------------------------------------------------
        // If the Contact person details for the CSE Person are entered,
        // validate the contact details.
        // -------------------------------------------------------------
        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // -------------------------------------------------------------
        // Insert the USE statement here that calls the READ action
        // block.
        // -------------------------------------------------------------
        UseOeCabCheckInsuranceCoverage();

        if (IsExitState("HEALTH_INSURANCE_COVERAGE_NF_RB") || IsExitState
          ("MORE_THAN_ONE_INSURANCE_COVERAGE") || IsExitState
          ("OE0000_MULTIPLE_INSURANCE_EXIST"))
        {
          MoveContact(local.RefreshContact, export.Contact);
          export.SelectedHealthInsuranceCompany.Assign(
            local.RefreshHealthInsuranceCompany);
          export.HealthInsuranceCompany.Assign(
            local.RefreshHealthInsuranceCompany);
          MoveHealthInsuranceCoverage3(local.RefreshHealthInsuranceCoverage,
            export.HealthInsuranceCoverage);
          export.SelectedHealthInsuranceCoverage.Assign(
            local.RefreshHealthInsuranceCoverage);

          if (!export.Export1.IsEmpty)
          {
            for(export.Export1.Index = 0; export.Export1.Index < export
              .Export1.Count; ++export.Export1.Index)
            {
              export.Export1.Update.Detail.SelectChar =
                local.RefreshCommon.SelectChar;
              export.Export1.Update.DetailsCaseRole.Type1 =
                local.RefreshCaseRole.Type1;
              export.Export1.Update.Insured.Number =
                local.RefreshCsePerson.Number;
              export.Export1.Update.InsuredH.Number =
                local.RefreshCsePerson.Number;
              export.Export1.Update.DetailsPersonalHealthInsurance.Assign(
                local.RefreshPersonalHealthInsurance);
              export.Export1.Update.InsuredName.FormattedNameText =
                local.RefreshOeWorkGroup.FormattedNameText;
              export.Export1.Update.H.CoverageVerifiedDate =
                local.RefreshPersonalHealthInsurance.CoverageVerifiedDate;
              export.Export1.Update.HiddenLastRead.CoverageVerifiedDate =
                local.RefreshPersonalHealthInsurance.CoverageVerifiedDate;
              export.Export1.Update.PersonPrompt.SelectChar =
                local.RefreshCommon.SelectChar;
              export.Export1.Update.DetailsCovAmtTxt.Text7 =
                local.RefreshCovAmtTxt.Text7;
            }
          }

          if (IsExitState("MORE_THAN_ONE_INSURANCE_COVERAGE") || IsExitState
            ("OE0000_MULTIPLE_INSURANCE_EXIST"))
          {
            ExitState = "OE0000_MULTIPLE_INSURANCE_EXIST";

            var field = GetField(export.WorkPromptCoverage, "selectChar");

            field.Error = true;
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // ---------------------------------------------
        // Insert the USE statement here that calls the
        // READ action block.
        // ---------------------------------------------
        UseOeHicpListAllPersonalHins();

        // ---------------------------------------------
        // Begin code - Raju : 12/19/1996 0335 CST
        // ---------------------------------------------
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          export.Export1.Update.HiddenLastRead.CoverageVerifiedDate =
            export.Export1.Item.DetailsPersonalHealthInsurance.
              CoverageVerifiedDate;

          if (!IsEmpty(export.Export1.Item.Insured.Number))
          {
            var field1 = GetField(export.Export1.Item.Insured, "number");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 =
              GetField(export.Export1.Item.PersonPrompt, "selectChar");

            field2.Color = "cyan";
            field2.Protected = true;

            // ***************************************************************************************
            // * CQ24156 - Convert the Personal Health Insurance Coverage Cost 
            // amount from numeric to*
            // *           Text to display on the 
            // screen.
            // 
            // START*
            // ***************************************************************************************
            if (export.Export1.Item.DetailsPersonalHealthInsurance.
              PremiumVerifiedDate != null || export
              .Export1.Item.DetailsPersonalHealthInsurance.CoverageCostAmount.
                GetValueOrDefault() > 0)
            {
              local.BatchConvertNumToText.Currency =
                export.Export1.Item.DetailsPersonalHealthInsurance.
                  CoverageCostAmount.GetValueOrDefault();
              UseFnConvertCurrencyToText();
              export.Export1.Update.DetailsCovAmtTxt.Text7 =
                Substring(local.BatchConvertNumToText.TextNumber16, 1, 7);
            }

            // ***************************************************************************************
            // * CQ24156 - Convert the Personal Health Insurance Coverage Cost 
            // amount from numeric to*
            // *           Text to display on the 
            // screen.
            // 
            // END*
            // ***************************************************************************************
          }
        }

        // ---------------------------------------------
        // End   code - Raju : 12/19/1996 0335 CST
        // ---------------------------------------------
        if (IsExitState("HEALTH_INSURANCE_COVERAGE_NF_RB"))
        {
          export.SelectedHealthInsuranceCompany.Assign(
            local.RefreshHealthInsuranceCompany);
          export.HealthInsuranceCompany.Assign(
            local.RefreshHealthInsuranceCompany);
          MoveHealthInsuranceCoverage3(local.RefreshHealthInsuranceCoverage,
            export.HealthInsuranceCoverage);
          export.SelectedHealthInsuranceCoverage.Assign(
            local.RefreshHealthInsuranceCoverage);

          if (!export.Export1.IsEmpty)
          {
            for(export.Export1.Index = 0; export.Export1.Index < export
              .Export1.Count; ++export.Export1.Index)
            {
              export.Export1.Update.Detail.SelectChar =
                local.RefreshCommon.SelectChar;
              export.Export1.Update.DetailsCaseRole.Type1 =
                local.RefreshCaseRole.Type1;
              export.Export1.Update.Insured.Number =
                local.RefreshCsePerson.Number;
              export.Export1.Update.InsuredH.Number =
                local.RefreshCsePerson.Number;
              export.Export1.Update.DetailsPersonalHealthInsurance.Assign(
                local.RefreshPersonalHealthInsurance);
              export.Export1.Update.InsuredName.FormattedNameText =
                local.RefreshOeWorkGroup.FormattedNameText;
              export.Export1.Update.H.CoverageVerifiedDate =
                local.RefreshPersonalHealthInsurance.CoverageVerifiedDate;
              export.Export1.Update.HiddenLastRead.CoverageVerifiedDate =
                local.RefreshPersonalHealthInsurance.CoverageVerifiedDate;
              export.Export1.Update.PersonPrompt.SelectChar =
                local.RefreshCommon.SelectChar;

              // ---------------------------------------------
              // Begin code - Raju : 12/19/1996 0335 CST
              // ---------------------------------------------
              export.Export1.Update.HiddenLastRead.CoverageVerifiedDate = null;

              // ---------------------------------------------
              // End   code - Raju : 12/19/1996 0335 CST
              // ---------------------------------------------
            }
          }
        }

        if (IsExitState("ACO_NI0000_SUCCESSFUL_DISPLAY"))
        {
          if (AsChar(export.CaseOpen.Flag) == 'N')
          {
            ExitState = "DISPLAY_OK_FOR_CLOSED_CASE";
          }
        }

        break;
      case "RETPCON":
        break;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        return;
    }

Test8:

    for(export.Export1.Index = 0; export.Export1.Index < export.Export1.Count; ++
      export.Export1.Index)
    {
      if (Equal(export.Export1.Item.DetailsPersonalHealthInsurance.
        CoverageEndDate, local.MaxDate.ExpirationDate))
      {
        export.Export1.Update.DetailsPersonalHealthInsurance.CoverageEndDate =
          null;
      }
    }

    // -------------------------------------------------------------
    // Code added by Raju  Dec 19, 1996:1130 hrs CST
    // The oe cab raise event will be called from here case of add /
    // update when the coverage verified date is added/changed
    // -------------------------------------------------------------
    // ------------------------------------------
    // added
    // . local view infrastructure
    // . import , export hidden last read
    //           personal health insurance
    // . local raise event flag work area
    //   - text1
    //   - this will be set/assigned for each event
    //     raised
    // ------------------------------------------
    // ---------------------------------------------
    // Start of code
    // ---------------------------------------------
    if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD") || IsExitState
      ("ACO_NI0000_SUCCESSFUL_UPDATE"))
    {
      local.Infrastructure.UserId = "HICP";
      local.Infrastructure.EventId = 45;
      local.Infrastructure.BusinessObjectCd = "PHI";
      local.Infrastructure.ReasonCode = "HINSSETUP";
      local.Infrastructure.SituationNumber = 0;

      // -------------------------------------------------------------
      // format the detail line (75c) as follows
      // 'Insured Person # :XXXXXXXXXX ,
      // Coverage Verified Date : MMDDCCYY'
      // -------------------------------------------------------------
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (AsChar(export.Export1.Item.Detail.SelectChar) == '*')
        {
          local.Infrastructure.Detail = Spaces(Infrastructure.Detail_MaxLength);
          local.DetailText30.Text30 = "Insured Person # :";
          local.DetailText10.Text10 = export.Export1.Item.Insured.Number;
          local.Infrastructure.Detail = TrimEnd(local.DetailText30.Text30) + local
            .DetailText10.Text10;
          local.DetailText30.Text30 = " , Role : " + export
            .Export1.Item.DetailsCaseRole.Type1;
          local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + local
            .DetailText30.Text30;
          local.DetailText30.Text30 = " , Coverage Verified Dt :";
          local.Infrastructure.ReferenceDate =
            export.Export1.Item.DetailsPersonalHealthInsurance.
              CoverageVerifiedDate;
          local.Date.Date = local.Infrastructure.ReferenceDate;
          local.DetailText10.Text10 = UseCabConvertDate2String();
          local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + TrimEnd
            (local.DetailText30.Text30);
          local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + TrimEnd
            (local.DetailText10.Text10);
          local.RaiseEventFlag.Text1 = "N";

          if (!Equal(export.Export1.Item.DetailsPersonalHealthInsurance.
            CoverageVerifiedDate, null))
          {
            if (!Equal(export.Export1.Item.DetailsPersonalHealthInsurance.
              CoverageVerifiedDate,
              export.Export1.Item.HiddenLastRead.CoverageVerifiedDate))
            {
              local.RaiseEventFlag.Text1 = "Y";
            }
          }

          if (AsChar(local.RaiseEventFlag.Text1) == 'Y')
          {
            // ------------------------------------------------------------
            // This is to aid the event processor to gather events from a
            // single situation
            // This is an extremely important piece of code
            // ------------------------------------------------------------
            local.Insured.Number = export.Export1.Item.Insured.Number;
            UseOeHicpRaiseEvent();

            if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD") || IsExitState
              ("ACO_NI0000_SUCCESSFUL_UPDATE"))
            {
            }
            else
            {
              return;
            }
          }
        }
      }

      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        export.Export1.Update.HiddenLastRead.CoverageVerifiedDate =
          export.Export1.Item.DetailsPersonalHealthInsurance.
            CoverageVerifiedDate;
      }
    }

    // ------------------------------------------
    // added
    // . local view infrastructure
    // . import , export hidden last read
    //           personal health insurance
    // . local raise event flag work area
    //   - text1
    //   - this will be set/assigned for each event
    //     raised
    // ------------------------------------------
    // ---------------------------------------------
    // End of code
    // ---------------------------------------------
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Flag = source.Flag;
    target.SelectChar = source.SelectChar;
  }

  private static void MoveContact(Contact source, Contact target)
  {
    target.ContactNumber = source.ContactNumber;
    target.RelationshipToCsePerson = source.RelationshipToCsePerson;
    target.NameLast = source.NameLast;
    target.NameFirst = source.NameFirst;
    target.MiddleInitial = source.MiddleInitial;
  }

  private static void MoveExport1(OeHicpListAllPersonalHins.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    System.Diagnostics.Trace.TraceWarning(
      "INFO: source and target of the move fit weakly.");
    target.PersonPrompt.SelectChar = source.Prompt.SelectChar;
    target.H.CoverageVerifiedDate = source.H.CoverageVerifiedDate;
    target.InsuredH.Number = source.InsuredH.Number;
    target.DetailsCaseRole.Type1 = source.DetailsCaseRole.Type1;
    target.Detail.SelectChar = source.Detail.SelectChar;
    target.Insured.Number = source.Insured.Number;
    target.InsuredName.FormattedNameText = source.InsuredName.FormattedNameText;
    target.DetailsPersonalHealthInsurance.Assign(
      source.DetailsPersonalHealthInsurance);
  }

  private static void MoveHealthInsuranceCompany(HealthInsuranceCompany source,
    HealthInsuranceCompany target)
  {
    target.Identifier = source.Identifier;
    target.CarrierCode = source.CarrierCode;
    target.InsurancePolicyCarrier = source.InsurancePolicyCarrier;
  }

  private static void MoveHealthInsuranceCoverage1(
    HealthInsuranceCoverage source, HealthInsuranceCoverage target)
  {
    target.Identifier = source.Identifier;
    target.InsuranceGroupNumber = source.InsuranceGroupNumber;
    target.InsurancePolicyNumber = source.InsurancePolicyNumber;
  }

  private static void MoveHealthInsuranceCoverage2(
    HealthInsuranceCoverage source, HealthInsuranceCoverage target)
  {
    target.Identifier = source.Identifier;
    target.InsuranceGroupNumber = source.InsuranceGroupNumber;
    target.InsurancePolicyNumber = source.InsurancePolicyNumber;
    target.PolicyExpirationDate = source.PolicyExpirationDate;
  }

  private static void MoveHealthInsuranceCoverage3(
    HealthInsuranceCoverage source, HealthInsuranceCoverage target)
  {
    target.Identifier = source.Identifier;
    target.InsuranceGroupNumber = source.InsuranceGroupNumber;
    target.InsurancePolicyNumber = source.InsurancePolicyNumber;
    target.CoverageCode1 = source.CoverageCode1;
    target.CoverageCode2 = source.CoverageCode2;
    target.CoverageCode3 = source.CoverageCode3;
    target.CoverageCode4 = source.CoverageCode4;
    target.CoverageCode5 = source.CoverageCode5;
    target.CoverageCode6 = source.CoverageCode6;
    target.CoverageCode7 = source.CoverageCode7;
  }

  private static void MoveHealthInsuranceCoverage4(
    HealthInsuranceCoverage source, HealthInsuranceCoverage target)
  {
    target.Identifier = source.Identifier;
    target.PolicyExpirationDate = source.PolicyExpirationDate;
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

  private static void MoveOffice(Office source, Office target)
  {
    target.TypeCode = source.TypeCode;
    target.Name = source.Name;
  }

  private static void MovePersonalHealthInsurance(
    PersonalHealthInsurance source, PersonalHealthInsurance target)
  {
    target.CoverageVerifiedDate = source.CoverageVerifiedDate;
    target.PremiumVerifiedDate = source.PremiumVerifiedDate;
    target.AlertFlagInsuranceExistsInd = source.AlertFlagInsuranceExistsInd;
    target.CoverageCostAmount = source.CoverageCostAmount;
    target.CoverageBeginDate = source.CoverageBeginDate;
    target.CoverageEndDate = source.CoverageEndDate;
  }

  private static void MoveSpDocKey(SpDocKey source, SpDocKey target)
  {
    target.KeyCase = source.KeyCase;
    target.KeyChild = source.KeyChild;
    target.KeyHealthInsCoverage = source.KeyHealthInsCoverage;
    target.KeyPerson = source.KeyPerson;
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

  private void UseFnConvertCurrencyToText()
  {
    var useImport = new FnConvertCurrencyToText.Import();
    var useExport = new FnConvertCurrencyToText.Export();

    useImport.BatchConvertNumToText.Currency =
      local.BatchConvertNumToText.Currency;

    Call(FnConvertCurrencyToText.Execute, useImport, useExport);

    local.BatchConvertNumToText.TextNumber16 =
      useExport.BatchConvertNumToText.TextNumber16;
  }

  private void UseOeCabCheckCaseMember1()
  {
    var useImport = new OeCabCheckCaseMember.Import();
    var useExport = new OeCabCheckCaseMember.Export();

    useImport.Case1.Number = import.Case1.Number;

    Call(OeCabCheckCaseMember.Execute, useImport, useExport);

    local.WorkError.Flag = useExport.Work.Flag;
  }

  private void UseOeCabCheckCaseMember2()
  {
    var useImport = new OeCabCheckCaseMember.Import();
    var useExport = new OeCabCheckCaseMember.Export();

    useImport.Case1.Number = import.Case1.Number;
    useImport.CsePerson.Number = export.Export1.Item.Insured.Number;

    Call(OeCabCheckCaseMember.Execute, useImport, useExport);

    local.WorkError.Flag = useExport.Work.Flag;
  }

  private void UseOeCabCheckCaseMember3()
  {
    var useImport = new OeCabCheckCaseMember.Import();
    var useExport = new OeCabCheckCaseMember.Export();

    useImport.Case1.Number = export.Case1.Number;
    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(OeCabCheckCaseMember.Execute, useImport, useExport);

    local.WorkError.Flag = useExport.Work.Flag;
  }

  private void UseOeCabCheckCaseMember4()
  {
    var useImport = new OeCabCheckCaseMember.Import();
    var useExport = new OeCabCheckCaseMember.Export();

    useImport.Case1.Number = export.Case1.Number;

    Call(OeCabCheckCaseMember.Execute, useImport, useExport);

    local.WorkError.Flag = useExport.Work.Flag;
    export.CaseOpen.Flag = useExport.CaseOpen.Flag;
  }

  private void UseOeCabCheckCaseMember5()
  {
    var useImport = new OeCabCheckCaseMember.Import();
    var useExport = new OeCabCheckCaseMember.Export();

    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(OeCabCheckCaseMember.Execute, useImport, useExport);

    local.WorkError.Flag = useExport.Work.Flag;
    export.CsePerson.Number = useExport.CsePerson.Number;
    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseOeCabCheckCaseMember6()
  {
    var useImport = new OeCabCheckCaseMember.Import();
    var useExport = new OeCabCheckCaseMember.Export();

    useImport.CsePerson.Number = export.Export1.Item.Insured.Number;

    Call(OeCabCheckCaseMember.Execute, useImport, useExport);

    local.WorkError.Flag = useExport.Work.Flag;
    export.Export1.Update.Insured.Number = useExport.CsePerson.Number;
    export.Export1.Update.InsuredName.FormattedNameText =
      useExport.WorkName.FormattedNameText;
  }

  private void UseOeCabCheckInsuranceCoverage()
  {
    var useImport = new OeCabCheckInsuranceCoverage.Import();
    var useExport = new OeCabCheckInsuranceCoverage.Export();

    useImport.Contact.Assign(export.Contact);
    useImport.Case1.Number = export.Case1.Number;
    useImport.HealthInsuranceCoverage.Identifier =
      export.HealthInsuranceCoverage.Identifier;
    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(OeCabCheckInsuranceCoverage.Execute, useImport, useExport);

    export.HealthInsuranceCoverage.Assign(useExport.HealthInsuranceCoverage);
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

  private void UseOeCabSetMnemonics()
  {
    var useImport = new OeCabSetMnemonics.Import();
    var useExport = new OeCabSetMnemonics.Export();

    Call(OeCabSetMnemonics.Execute, useImport, useExport);

    local.MaxDate.ExpirationDate = useExport.MaxDate.ExpirationDate;
  }

  private void UseOeHicpCreatePersonalHins()
  {
    var useImport = new OeHicpCreatePersonalHins.Import();
    var useExport = new OeHicpCreatePersonalHins.Export();

    useImport.Case1.Number = export.Case1.Number;
    useImport.Holder.Number = export.CsePerson.Number;
    MovePersonalHealthInsurance(export.Export1.Item.
      DetailsPersonalHealthInsurance, useImport.PersonalHealthInsurance);
    useImport.Covered.Number = export.Export1.Item.Insured.Number;
    MoveHealthInsuranceCoverage1(export.HealthInsuranceCoverage,
      useImport.HealthInsuranceCoverage);

    Call(OeHicpCreatePersonalHins.Execute, useImport, useExport);

    export.Export1.Update.DetailsPersonalHealthInsurance.Assign(
      useExport.PersonalHealthInsurance);
    export.Export1.Update.Insured.Number = useExport.Covered.Number;
    MoveHealthInsuranceCoverage2(useExport.HealthInsuranceCoverage,
      export.HealthInsuranceCoverage);
    export.Export1.Update.DetailsCaseRole.Type1 = useExport.CaseRole.Type1;
    local.Common.Count = useExport.Common.Count;
  }

  private void UseOeHicpDeletePersonalHins()
  {
    var useImport = new OeHicpDeletePersonalHins.Import();
    var useExport = new OeHicpDeletePersonalHins.Export();

    useImport.Insured.Number = export.Export1.Item.Insured.Number;
    useImport.HealthInsuranceCoverage.Identifier =
      export.HealthInsuranceCoverage.Identifier;
    useImport.Holder.Number = export.CsePerson.Number;

    Call(OeHicpDeletePersonalHins.Execute, useImport, useExport);
  }

  private void UseOeHicpListAllPersonalHins()
  {
    var useImport = new OeHicpListAllPersonalHins.Import();
    var useExport = new OeHicpListAllPersonalHins.Export();

    useImport.HolderCase.Number = export.Case1.Number;
    useImport.HolderCsePerson.Number = export.CsePerson.Number;
    useImport.HolderHealthInsuranceCompany.
      Assign(export.HealthInsuranceCompany);
    useImport.HolderHealthInsuranceCoverage.Assign(
      export.HealthInsuranceCoverage);
    useImport.HolderContact.Assign(export.Contact);

    Call(OeHicpListAllPersonalHins.Execute, useImport, useExport);

    export.HcsePerson.Number = useExport.HolderHCsePerson.Number;
    export.Hcase.Number = useExport.HolderHCase.Number;
    export.SelectedHealthInsuranceCoverage.Assign(
      useExport.HolderHHealthInsuranceCoverage);
    useExport.Export1.CopyTo(export.Export1, MoveExport1);
    export.Case1.Number = useExport.HolderCase.Number;
    export.CsePerson.Number = useExport.HolderCsePerson.Number;
    export.HealthInsuranceCompany.
      Assign(useExport.HolderHealthInsuranceCompany);
    MoveHealthInsuranceCoverage1(useExport.HolderHealthInsuranceCoverage,
      export.HealthInsuranceCoverage);
    export.Contact.Assign(useExport.HolderContact);
  }

  private void UseOeHicpRaiseEvent()
  {
    var useImport = new OeHicpRaiseEvent.Import();
    var useExport = new OeHicpRaiseEvent.Export();

    MoveHealthInsuranceCoverage4(export.HealthInsuranceCoverage,
      useImport.HolderHealthInsuranceCoverage);
    useImport.Insured.Number = local.Insured.Number;
    useImport.Case1.Number = export.Case1.Number;
    useImport.HolderCsePerson.Number = export.CsePerson.Number;
    MoveInfrastructure(local.Infrastructure, useImport.Infrastructure);

    Call(OeHicpRaiseEvent.Execute, useImport, useExport);

    MoveInfrastructure(useExport.Infrastructure, local.Infrastructure);
  }

  private void UseOeHicpUpdatePersonalHins()
  {
    var useImport = new OeHicpUpdatePersonalHins.Import();
    var useExport = new OeHicpUpdatePersonalHins.Export();

    useImport.Case1.Number = export.Case1.Number;
    useImport.Holder.Number = export.CsePerson.Number;
    MovePersonalHealthInsurance(export.Export1.Item.
      DetailsPersonalHealthInsurance, useImport.PersonalHealthInsurance);
    useImport.Covered.Number = export.Export1.Item.Insured.Number;
    MoveHealthInsuranceCoverage1(export.HealthInsuranceCoverage,
      useImport.HealthInsuranceCoverage);

    Call(OeHicpUpdatePersonalHins.Execute, useImport, useExport);

    export.Export1.Update.DetailsPersonalHealthInsurance.Assign(
      useExport.PersonalHealthInsurance);
    local.Common.Count = useExport.Common.Count;
    MoveHealthInsuranceCoverage2(useExport.HealthInsuranceCoverage,
      export.HealthInsuranceCoverage);
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.Hidden.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(export.Hidden);

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

  private void UseSiReadOfficeOspHeader()
  {
    var useImport = new SiReadOfficeOspHeader.Import();
    var useExport = new SiReadOfficeOspHeader.Export();

    useImport.Case1.Number = export.Case1.Number;

    Call(SiReadOfficeOspHeader.Execute, useImport, useExport);

    export.HeaderLine.Text35 = useExport.HeaderLine.Text35;
    export.ServiceProvider.LastName = useExport.ServiceProvider.LastName;
    MoveOffice(useExport.Office, export.Office);
  }

  private void UseSpCabDetermineInterstateDoc()
  {
    var useImport = new SpCabDetermineInterstateDoc.Import();
    var useExport = new SpCabDetermineInterstateDoc.Export();

    useImport.FindDoc.Flag = local.FindDoc.Flag;
    useImport.Document.Name = local.Document.Name;
    MoveSpDocKey(local.SpDocKey, useImport.SpDocKey);

    Call(SpCabDetermineInterstateDoc.Execute, useImport, useExport);
  }

  private bool ReadCaseRole1()
  {
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRole1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", import.Case1.Number);
        db.SetString(command, "cspNumber", export.Export1.Item.Insured.Number);
        db.SetNullableDate(
          command, "startDate", local.CurrentDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private bool ReadCaseRole2()
  {
    entities.IsArOrgCaseRole.Populated = false;

    return Read("ReadCaseRole2",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetString(command, "casNumber", export.Case1.Number);
        db.SetNullableDate(command, "startDate", date);
      },
      (db, reader) =>
      {
        entities.IsArOrgCaseRole.CasNumber = db.GetString(reader, 0);
        entities.IsArOrgCaseRole.CspNumber = db.GetString(reader, 1);
        entities.IsArOrgCaseRole.Type1 = db.GetString(reader, 2);
        entities.IsArOrgCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.IsArOrgCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.IsArOrgCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.IsArOrgCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.IsArOrgCaseRole.Type1);
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
    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
      /// <summary>
      /// A value of PersonPrompt.
      /// </summary>
      [JsonPropertyName("personPrompt")]
      public Common PersonPrompt
      {
        get => personPrompt ??= new();
        set => personPrompt = value;
      }

      /// <summary>
      /// A value of H.
      /// </summary>
      [JsonPropertyName("h")]
      public PersonalHealthInsurance H
      {
        get => h ??= new();
        set => h = value;
      }

      /// <summary>
      /// A value of InsuredH.
      /// </summary>
      [JsonPropertyName("insuredH")]
      public CsePerson InsuredH
      {
        get => insuredH ??= new();
        set => insuredH = value;
      }

      /// <summary>
      /// A value of DetailsCaseRole.
      /// </summary>
      [JsonPropertyName("detailsCaseRole")]
      public CaseRole DetailsCaseRole
      {
        get => detailsCaseRole ??= new();
        set => detailsCaseRole = value;
      }

      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public Common Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>
      /// A value of Insured.
      /// </summary>
      [JsonPropertyName("insured")]
      public CsePerson Insured
      {
        get => insured ??= new();
        set => insured = value;
      }

      /// <summary>
      /// A value of InsuredName.
      /// </summary>
      [JsonPropertyName("insuredName")]
      public OeWorkGroup InsuredName
      {
        get => insuredName ??= new();
        set => insuredName = value;
      }

      /// <summary>
      /// A value of DetailsPersonalHealthInsurance.
      /// </summary>
      [JsonPropertyName("detailsPersonalHealthInsurance")]
      public PersonalHealthInsurance DetailsPersonalHealthInsurance
      {
        get => detailsPersonalHealthInsurance ??= new();
        set => detailsPersonalHealthInsurance = value;
      }

      /// <summary>
      /// A value of HiddenLastRead.
      /// </summary>
      [JsonPropertyName("hiddenLastRead")]
      public PersonalHealthInsurance HiddenLastRead
      {
        get => hiddenLastRead ??= new();
        set => hiddenLastRead = value;
      }

      /// <summary>
      /// A value of DetailsCovAmtTxt.
      /// </summary>
      [JsonPropertyName("detailsCovAmtTxt")]
      public WorkArea DetailsCovAmtTxt
      {
        get => detailsCovAmtTxt ??= new();
        set => detailsCovAmtTxt = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private Common personPrompt;
      private PersonalHealthInsurance h;
      private CsePerson insuredH;
      private CaseRole detailsCaseRole;
      private Common detail;
      private CsePerson insured;
      private OeWorkGroup insuredName;
      private PersonalHealthInsurance detailsPersonalHealthInsurance;
      private PersonalHealthInsurance hiddenLastRead;
      private WorkArea detailsCovAmtTxt;
    }

    /// <summary>
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Case1 Next
    {
      get => next ??= new();
      set => next = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of SelectedCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("selectedCsePersonsWorkSet")]
    public CsePersonsWorkSet SelectedCsePersonsWorkSet
    {
      get => selectedCsePersonsWorkSet ??= new();
      set => selectedCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Contact.
    /// </summary>
    [JsonPropertyName("contact")]
    public Contact Contact
    {
      get => contact ??= new();
      set => contact = value;
    }

    /// <summary>
    /// A value of SelectedContact.
    /// </summary>
    [JsonPropertyName("selectedContact")]
    public Contact SelectedContact
    {
      get => selectedContact ??= new();
      set => selectedContact = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCompany.
    /// </summary>
    [JsonPropertyName("healthInsuranceCompany")]
    public HealthInsuranceCompany HealthInsuranceCompany
    {
      get => healthInsuranceCompany ??= new();
      set => healthInsuranceCompany = value;
    }

    /// <summary>
    /// A value of SelectedHealthInsuranceCompany.
    /// </summary>
    [JsonPropertyName("selectedHealthInsuranceCompany")]
    public HealthInsuranceCompany SelectedHealthInsuranceCompany
    {
      get => selectedHealthInsuranceCompany ??= new();
      set => selectedHealthInsuranceCompany = value;
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
    /// A value of SelectedHealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("selectedHealthInsuranceCoverage")]
    public HealthInsuranceCoverage SelectedHealthInsuranceCoverage
    {
      get => selectedHealthInsuranceCoverage ??= new();
      set => selectedHealthInsuranceCoverage = value;
    }

    /// <summary>
    /// Gets a value of Import1.
    /// </summary>
    [JsonIgnore]
    public Array<ImportGroup> Import1 => import1 ??= new(ImportGroup.Capacity);

    /// <summary>
    /// Gets a value of Import1 for json serialization.
    /// </summary>
    [JsonPropertyName("import1")]
    [Computed]
    public IList<ImportGroup> Import1_Json
    {
      get => import1;
      set => Import1.Assign(value);
    }

    /// <summary>
    /// A value of OeWorkGroup.
    /// </summary>
    [JsonPropertyName("oeWorkGroup")]
    public OeWorkGroup OeWorkGroup
    {
      get => oeWorkGroup ??= new();
      set => oeWorkGroup = value;
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
    /// A value of WorkPromptContact.
    /// </summary>
    [JsonPropertyName("workPromptContact")]
    public Common WorkPromptContact
    {
      get => workPromptContact ??= new();
      set => workPromptContact = value;
    }

    /// <summary>
    /// A value of WorkPromptCoverage.
    /// </summary>
    [JsonPropertyName("workPromptCoverage")]
    public Common WorkPromptCoverage
    {
      get => workPromptCoverage ??= new();
      set => workPromptCoverage = value;
    }

    /// <summary>
    /// A value of WorkPromptInd.
    /// </summary>
    [JsonPropertyName("workPromptInd")]
    public Common WorkPromptInd
    {
      get => workPromptInd ??= new();
      set => workPromptInd = value;
    }

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    /// <summary>
    /// A value of SelectedCase.
    /// </summary>
    [JsonPropertyName("selectedCase")]
    public Case1 SelectedCase
    {
      get => selectedCase ??= new();
      set => selectedCase = value;
    }

    /// <summary>
    /// A value of SelectedCsePerson.
    /// </summary>
    [JsonPropertyName("selectedCsePerson")]
    public CsePerson SelectedCsePerson
    {
      get => selectedCsePerson ??= new();
      set => selectedCsePerson = value;
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
    /// A value of CaseOpen.
    /// </summary>
    [JsonPropertyName("caseOpen")]
    public Common CaseOpen
    {
      get => caseOpen ??= new();
      set => caseOpen = value;
    }

    /// <summary>
    /// A value of HeaderLine.
    /// </summary>
    [JsonPropertyName("headerLine")]
    public WorkArea HeaderLine
    {
      get => headerLine ??= new();
      set => headerLine = value;
    }

    private Case1 next;
    private Standard standard;
    private Case1 case1;
    private Case1 hcase;
    private CsePerson csePerson;
    private CsePerson hcsePerson;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePersonsWorkSet selectedCsePersonsWorkSet;
    private Contact contact;
    private Contact selectedContact;
    private HealthInsuranceCompany healthInsuranceCompany;
    private HealthInsuranceCompany selectedHealthInsuranceCompany;
    private HealthInsuranceCoverage healthInsuranceCoverage;
    private HealthInsuranceCoverage selectedHealthInsuranceCoverage;
    private Array<ImportGroup> import1;
    private OeWorkGroup oeWorkGroup;
    private Common csePersonPrompt;
    private Common workPromptContact;
    private Common workPromptCoverage;
    private Common workPromptInd;
    private NextTranInfo hidden;
    private Case1 selectedCase;
    private CsePerson selectedCsePerson;
    private ServiceProvider serviceProvider;
    private Office office;
    private Common caseOpen;
    private WorkArea headerLine;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
      /// <summary>
      /// A value of PersonPrompt.
      /// </summary>
      [JsonPropertyName("personPrompt")]
      public Common PersonPrompt
      {
        get => personPrompt ??= new();
        set => personPrompt = value;
      }

      /// <summary>
      /// A value of H.
      /// </summary>
      [JsonPropertyName("h")]
      public PersonalHealthInsurance H
      {
        get => h ??= new();
        set => h = value;
      }

      /// <summary>
      /// A value of InsuredH.
      /// </summary>
      [JsonPropertyName("insuredH")]
      public CsePerson InsuredH
      {
        get => insuredH ??= new();
        set => insuredH = value;
      }

      /// <summary>
      /// A value of DetailsCaseRole.
      /// </summary>
      [JsonPropertyName("detailsCaseRole")]
      public CaseRole DetailsCaseRole
      {
        get => detailsCaseRole ??= new();
        set => detailsCaseRole = value;
      }

      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public Common Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>
      /// A value of Insured.
      /// </summary>
      [JsonPropertyName("insured")]
      public CsePerson Insured
      {
        get => insured ??= new();
        set => insured = value;
      }

      /// <summary>
      /// A value of InsuredName.
      /// </summary>
      [JsonPropertyName("insuredName")]
      public OeWorkGroup InsuredName
      {
        get => insuredName ??= new();
        set => insuredName = value;
      }

      /// <summary>
      /// A value of DetailsPersonalHealthInsurance.
      /// </summary>
      [JsonPropertyName("detailsPersonalHealthInsurance")]
      public PersonalHealthInsurance DetailsPersonalHealthInsurance
      {
        get => detailsPersonalHealthInsurance ??= new();
        set => detailsPersonalHealthInsurance = value;
      }

      /// <summary>
      /// A value of HiddenLastRead.
      /// </summary>
      [JsonPropertyName("hiddenLastRead")]
      public PersonalHealthInsurance HiddenLastRead
      {
        get => hiddenLastRead ??= new();
        set => hiddenLastRead = value;
      }

      /// <summary>
      /// A value of DetailsCovAmtTxt.
      /// </summary>
      [JsonPropertyName("detailsCovAmtTxt")]
      public WorkArea DetailsCovAmtTxt
      {
        get => detailsCovAmtTxt ??= new();
        set => detailsCovAmtTxt = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private Common personPrompt;
      private PersonalHealthInsurance h;
      private CsePerson insuredH;
      private CaseRole detailsCaseRole;
      private Common detail;
      private CsePerson insured;
      private OeWorkGroup insuredName;
      private PersonalHealthInsurance detailsPersonalHealthInsurance;
      private PersonalHealthInsurance hiddenLastRead;
      private WorkArea detailsCovAmtTxt;
    }

    /// <summary>
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Case1 Next
    {
      get => next ??= new();
      set => next = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of SelectedCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("selectedCsePersonsWorkSet")]
    public CsePersonsWorkSet SelectedCsePersonsWorkSet
    {
      get => selectedCsePersonsWorkSet ??= new();
      set => selectedCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Contact.
    /// </summary>
    [JsonPropertyName("contact")]
    public Contact Contact
    {
      get => contact ??= new();
      set => contact = value;
    }

    /// <summary>
    /// A value of SelectedContact.
    /// </summary>
    [JsonPropertyName("selectedContact")]
    public Contact SelectedContact
    {
      get => selectedContact ??= new();
      set => selectedContact = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCompany.
    /// </summary>
    [JsonPropertyName("healthInsuranceCompany")]
    public HealthInsuranceCompany HealthInsuranceCompany
    {
      get => healthInsuranceCompany ??= new();
      set => healthInsuranceCompany = value;
    }

    /// <summary>
    /// A value of SelectedHealthInsuranceCompany.
    /// </summary>
    [JsonPropertyName("selectedHealthInsuranceCompany")]
    public HealthInsuranceCompany SelectedHealthInsuranceCompany
    {
      get => selectedHealthInsuranceCompany ??= new();
      set => selectedHealthInsuranceCompany = value;
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
    /// A value of SelectedHealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("selectedHealthInsuranceCoverage")]
    public HealthInsuranceCoverage SelectedHealthInsuranceCoverage
    {
      get => selectedHealthInsuranceCoverage ??= new();
      set => selectedHealthInsuranceCoverage = value;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 => export1 ??= new(ExportGroup.Capacity);

    /// <summary>
    /// Gets a value of Export1 for json serialization.
    /// </summary>
    [JsonPropertyName("export1")]
    [Computed]
    public IList<ExportGroup> Export1_Json
    {
      get => export1;
      set => Export1.Assign(value);
    }

    /// <summary>
    /// A value of OeWorkGroup.
    /// </summary>
    [JsonPropertyName("oeWorkGroup")]
    public OeWorkGroup OeWorkGroup
    {
      get => oeWorkGroup ??= new();
      set => oeWorkGroup = value;
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
    /// A value of WorkPromptContact.
    /// </summary>
    [JsonPropertyName("workPromptContact")]
    public Common WorkPromptContact
    {
      get => workPromptContact ??= new();
      set => workPromptContact = value;
    }

    /// <summary>
    /// A value of WorkPromptCoverage.
    /// </summary>
    [JsonPropertyName("workPromptCoverage")]
    public Common WorkPromptCoverage
    {
      get => workPromptCoverage ??= new();
      set => workPromptCoverage = value;
    }

    /// <summary>
    /// A value of WorkPromptInd.
    /// </summary>
    [JsonPropertyName("workPromptInd")]
    public Common WorkPromptInd
    {
      get => workPromptInd ??= new();
      set => workPromptInd = value;
    }

    /// <summary>
    /// A value of WorkContactExist.
    /// </summary>
    [JsonPropertyName("workContactExist")]
    public Common WorkContactExist
    {
      get => workContactExist ??= new();
      set => workContactExist = value;
    }

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    /// <summary>
    /// A value of SelectedCase.
    /// </summary>
    [JsonPropertyName("selectedCase")]
    public Case1 SelectedCase
    {
      get => selectedCase ??= new();
      set => selectedCase = value;
    }

    /// <summary>
    /// A value of SelectedCsePerson.
    /// </summary>
    [JsonPropertyName("selectedCsePerson")]
    public CsePerson SelectedCsePerson
    {
      get => selectedCsePerson ??= new();
      set => selectedCsePerson = value;
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
    /// A value of CaseOpen.
    /// </summary>
    [JsonPropertyName("caseOpen")]
    public Common CaseOpen
    {
      get => caseOpen ??= new();
      set => caseOpen = value;
    }

    /// <summary>
    /// A value of HeaderLine.
    /// </summary>
    [JsonPropertyName("headerLine")]
    public WorkArea HeaderLine
    {
      get => headerLine ??= new();
      set => headerLine = value;
    }

    private Case1 next;
    private Standard standard;
    private Case1 case1;
    private Case1 hcase;
    private CsePerson csePerson;
    private CsePerson hcsePerson;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePersonsWorkSet selectedCsePersonsWorkSet;
    private Contact contact;
    private Contact selectedContact;
    private HealthInsuranceCompany healthInsuranceCompany;
    private HealthInsuranceCompany selectedHealthInsuranceCompany;
    private HealthInsuranceCoverage healthInsuranceCoverage;
    private HealthInsuranceCoverage selectedHealthInsuranceCoverage;
    private Array<ExportGroup> export1;
    private OeWorkGroup oeWorkGroup;
    private Common csePersonPrompt;
    private Common workPromptContact;
    private Common workPromptCoverage;
    private Common workPromptInd;
    private Common workContactExist;
    private NextTranInfo hidden;
    private Case1 selectedCase;
    private CsePerson selectedCsePerson;
    private ServiceProvider serviceProvider;
    private Office office;
    private Common caseOpen;
    private WorkArea headerLine;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of IsClient.
    /// </summary>
    [JsonPropertyName("isClient")]
    public Common IsClient
    {
      get => isClient ??= new();
      set => isClient = value;
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
    /// A value of OutgoingDocument.
    /// </summary>
    [JsonPropertyName("outgoingDocument")]
    public OutgoingDocument OutgoingDocument
    {
      get => outgoingDocument ??= new();
      set => outgoingDocument = value;
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
    /// A value of Insured.
    /// </summary>
    [JsonPropertyName("insured")]
    public CsePerson Insured
    {
      get => insured ??= new();
      set => insured = value;
    }

    /// <summary>
    /// A value of RefreshContact.
    /// </summary>
    [JsonPropertyName("refreshContact")]
    public Contact RefreshContact
    {
      get => refreshContact ??= new();
      set => refreshContact = value;
    }

    /// <summary>
    /// A value of SearchCase.
    /// </summary>
    [JsonPropertyName("searchCase")]
    public Case1 SearchCase
    {
      get => searchCase ??= new();
      set => searchCase = value;
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
    /// A value of RefreshHealthInsuranceCompany.
    /// </summary>
    [JsonPropertyName("refreshHealthInsuranceCompany")]
    public HealthInsuranceCompany RefreshHealthInsuranceCompany
    {
      get => refreshHealthInsuranceCompany ??= new();
      set => refreshHealthInsuranceCompany = value;
    }

    /// <summary>
    /// A value of RefreshHealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("refreshHealthInsuranceCoverage")]
    public HealthInsuranceCoverage RefreshHealthInsuranceCoverage
    {
      get => refreshHealthInsuranceCoverage ??= new();
      set => refreshHealthInsuranceCoverage = value;
    }

    /// <summary>
    /// A value of RefreshCaseRole.
    /// </summary>
    [JsonPropertyName("refreshCaseRole")]
    public CaseRole RefreshCaseRole
    {
      get => refreshCaseRole ??= new();
      set => refreshCaseRole = value;
    }

    /// <summary>
    /// A value of RefreshCommon.
    /// </summary>
    [JsonPropertyName("refreshCommon")]
    public Common RefreshCommon
    {
      get => refreshCommon ??= new();
      set => refreshCommon = value;
    }

    /// <summary>
    /// A value of RefreshCsePerson.
    /// </summary>
    [JsonPropertyName("refreshCsePerson")]
    public CsePerson RefreshCsePerson
    {
      get => refreshCsePerson ??= new();
      set => refreshCsePerson = value;
    }

    /// <summary>
    /// A value of RefreshOeWorkGroup.
    /// </summary>
    [JsonPropertyName("refreshOeWorkGroup")]
    public OeWorkGroup RefreshOeWorkGroup
    {
      get => refreshOeWorkGroup ??= new();
      set => refreshOeWorkGroup = value;
    }

    /// <summary>
    /// A value of RefreshPersonalHealthInsurance.
    /// </summary>
    [JsonPropertyName("refreshPersonalHealthInsurance")]
    public PersonalHealthInsurance RefreshPersonalHealthInsurance
    {
      get => refreshPersonalHealthInsurance ??= new();
      set => refreshPersonalHealthInsurance = value;
    }

    /// <summary>
    /// A value of SelectCount.
    /// </summary>
    [JsonPropertyName("selectCount")]
    public Common SelectCount
    {
      get => selectCount ??= new();
      set => selectCount = value;
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
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public Code MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

    /// <summary>
    /// A value of SearchCsePerson.
    /// </summary>
    [JsonPropertyName("searchCsePerson")]
    public CsePerson SearchCsePerson
    {
      get => searchCsePerson ??= new();
      set => searchCsePerson = value;
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
    /// A value of RaiseEventFlag.
    /// </summary>
    [JsonPropertyName("raiseEventFlag")]
    public WorkArea RaiseEventFlag
    {
      get => raiseEventFlag ??= new();
      set => raiseEventFlag = value;
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
    /// A value of DetailText10.
    /// </summary>
    [JsonPropertyName("detailText10")]
    public TextWorkArea DetailText10
    {
      get => detailText10 ??= new();
      set => detailText10 = value;
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
    /// A value of LastTran.
    /// </summary>
    [JsonPropertyName("lastTran")]
    public Infrastructure LastTran
    {
      get => lastTran ??= new();
      set => lastTran = value;
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
    /// A value of DigitStartPosition.
    /// </summary>
    [JsonPropertyName("digitStartPosition")]
    public Common DigitStartPosition
    {
      get => digitStartPosition ??= new();
      set => digitStartPosition = value;
    }

    /// <summary>
    /// A value of FirstDotPosition.
    /// </summary>
    [JsonPropertyName("firstDotPosition")]
    public Common FirstDotPosition
    {
      get => firstDotPosition ??= new();
      set => firstDotPosition = value;
    }

    /// <summary>
    /// A value of DotCount.
    /// </summary>
    [JsonPropertyName("dotCount")]
    public Common DotCount
    {
      get => dotCount ??= new();
      set => dotCount = value;
    }

    /// <summary>
    /// A value of NoOfDigitsAfterDot.
    /// </summary>
    [JsonPropertyName("noOfDigitsAfterDot")]
    public Common NoOfDigitsAfterDot
    {
      get => noOfDigitsAfterDot ??= new();
      set => noOfDigitsAfterDot = value;
    }

    /// <summary>
    /// A value of StringLength.
    /// </summary>
    [JsonPropertyName("stringLength")]
    public Common StringLength
    {
      get => stringLength ??= new();
      set => stringLength = value;
    }

    /// <summary>
    /// A value of CurrentPositionValue.
    /// </summary>
    [JsonPropertyName("currentPositionValue")]
    public WorkArea CurrentPositionValue
    {
      get => currentPositionValue ??= new();
      set => currentPositionValue = value;
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
    /// A value of RefreshCovAmtTxt.
    /// </summary>
    [JsonPropertyName("refreshCovAmtTxt")]
    public WorkArea RefreshCovAmtTxt
    {
      get => refreshCovAmtTxt ??= new();
      set => refreshCovAmtTxt = value;
    }

    /// <summary>
    /// A value of CovAmtTxt.
    /// </summary>
    [JsonPropertyName("covAmtTxt")]
    public WorkArea CovAmtTxt
    {
      get => covAmtTxt ??= new();
      set => covAmtTxt = value;
    }

    private Common findDoc;
    private Common isClient;
    private Common common;
    private OutgoingDocument outgoingDocument;
    private Document document;
    private SpDocKey spDocKey;
    private CsePerson insured;
    private Contact refreshContact;
    private Case1 searchCase;
    private TextWorkArea textWorkArea;
    private HealthInsuranceCompany refreshHealthInsuranceCompany;
    private HealthInsuranceCoverage refreshHealthInsuranceCoverage;
    private CaseRole refreshCaseRole;
    private Common refreshCommon;
    private CsePerson refreshCsePerson;
    private OeWorkGroup refreshOeWorkGroup;
    private PersonalHealthInsurance refreshPersonalHealthInsurance;
    private Common selectCount;
    private Common workError;
    private Code maxDate;
    private CsePerson searchCsePerson;
    private Infrastructure infrastructure;
    private WorkArea raiseEventFlag;
    private TextWorkArea detailText30;
    private TextWorkArea detailText10;
    private DateWorkArea date;
    private Infrastructure lastTran;
    private DateWorkArea currentDate;
    private Common digitStartPosition;
    private Common firstDotPosition;
    private Common dotCount;
    private Common noOfDigitsAfterDot;
    private Common stringLength;
    private WorkArea currentPositionValue;
    private BatchConvertNumToText batchConvertNumToText;
    private WorkArea refreshCovAmtTxt;
    private WorkArea covAmtTxt;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of IsArOrgCaseRole.
    /// </summary>
    [JsonPropertyName("isArOrgCaseRole")]
    public CaseRole IsArOrgCaseRole
    {
      get => isArOrgCaseRole ??= new();
      set => isArOrgCaseRole = value;
    }

    /// <summary>
    /// A value of IsArOrgCsePerson.
    /// </summary>
    [JsonPropertyName("isArOrgCsePerson")]
    public CsePerson IsArOrgCsePerson
    {
      get => isArOrgCsePerson ??= new();
      set => isArOrgCsePerson = value;
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

    private CaseRole isArOrgCaseRole;
    private CsePerson isArOrgCsePerson;
    private CsePerson csePerson;
    private Case1 case1;
    private CaseRole caseRole;
  }
#endregion
}
