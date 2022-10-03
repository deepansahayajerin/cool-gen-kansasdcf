// Program: SI_IIMC_IS_MANUAL_CONVERSION, ID: 372505053, model: 746.
// Short name: SWEIIMCP
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_IIMC_IS_MANUAL_CONVERSION.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiIimcIsManualConversion: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_IIMC_IS_MANUAL_CONVERSION program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiIimcIsManualConversion(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiIimcIsManualConversion.
  /// </summary>
  public SiIimcIsManualConversion(IContext context, Import import, Export export)
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
    // ------------------------------------------------------------
    // Date	  	Developer Name		Description
    // 10/20/1997 	Sid Chowdhary		Initial development
    // 01/07/1998	Sid Chowdhary		IDCR# 409
    // 12/15/98        C Deghand               Added the move statement
    //                                         
    // for contact country.
    // 12/17/98        C Deghand               Added an IF statement in
    //                                         
    // the procedure after the
    //                                         
    // Read Action Block to check
    //                                         
    // for the Exit State "Case is
    //                                         
    // not an interstate case".
    // 12/22/98        C Deghand               Added set statements in the
    //                                         
    // check for case number.
    // 1/26/99         C Deghand               Removed OE Cab Check Case
    //                                         
    // Member and replaced it with
    //                                         
    // SI Read Case Header
    //                                         
    // Information.
    // 2/4/99          C Deghand               Moved the IF for clear to
    //                                         
    // the top of the procedure.
    // 2/11/99         C Deghand               Added the code for case
    //                                         
    // IIFI.
    // 3/12/99         C Deghand               Added code to check for
    //                                         
    // multiple prompt selections.
    // 05/14/1999	M Ramirez		Added creation of document trigger
    // 05/26/1999	PMcElderry	        CSEnet functionality
    // 07/19/1999      CScroggins              Added flow to PEPR upon case 
    // closure
    // ------------------------------------------------------------
    // ------------------------------------------------------------
    // 07/19/1999      CScroggins              Added flow to PEPR upon case 
    // closure
    // 07/31/1999      CScroggins              Removed exit state on flow from 
    // CDVL
    //                                         
    // Added automaic flow to PEPR upon
    //                                         
    // successful ADD
    //                                         
    // Commented out code to unprotect
    //                                         
    // Duplicate Case Indicator
    // 11/08/2000      CScroggins              Added changes for PR#106722
    // ------------------------------------------------------------
    // 11/17/00 M.Lachowicz      WR 298. Create header
    //                           information for screens.
    // -----------------------------------------------
    // *******************************************************************
    //   03/12/2001        Madhu Kumar
    //   Edit check on zip code  for 4 or 5  digits .
    // *******************************************************************
    // 05/14/01 C Fairley I00119237 Added automatic flow to IREQ, when multiple 
    // Interstate
    //                              Requests exist.
    //                              Removed OLD commented out code.
    // -------------------------------------------------------------------------------------
    // 07/25/2001   Tom Bobb WR10501
    // Do not allow creation of incoming if open outgoing
    // exists
    // -------------------------------------------------------------------------------------
    // ---------------------------------------------------------------------------------------
    // 03/04/02 T.Bobb PR00138552 Moved the CSENet code
    // that used to be at the end of the Prad to after the Update
    // logic so that when a Interstate Request is closed, a
    // CSENet transaction will be sent to the initiating state.
    // ---------------------------------------------------------------------------------------
    // 06/27/03 GVandy 	 PR180512	Clear out FIPS info if the user selected a 
    // country.  Otherwise,
    // 					previous FIPS state info is being stored in the interstate_request.
    // 05/10/06 GVandy 	 WR230751	Add support for Tribal IV-D agencies.
    // 03/19/08 Arun Mathias    CQ#318         Do not allow user to re-open the 
    // interstate case if the KS-case is closed and Also,
    //                                         
    // Added edit checks for closure reason.
    // 03/20/09    Anita Hockman   cq531       changes made to keep fields blank
    // if the chase is not an active incoming case.
    // 11/29/10 RMathews        CQ23444        Additional change to keep fields 
    // blank for previous incoming interstate cases.
    // 6/12/18  JHarden         CQ62215        Add a field for FAX number on 
    // IIMC
    // 02/12/19 GVandy 	 CQ65253	Allow domestic payment addresses for
    // 					interstate cases from foreign countries.
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;
    local.Max.Date = new DateTime(2099, 12, 31);
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    MoveInterstateRequest4(import.HiddenInterstateRequest,
      export.HiddenInterstateRequest);
    MoveCase5(import.DisplayOnly, export.DisplayOnly);
    export.Hduplicate.DuplicateCaseIndicator =
      import.Hduplicate.DuplicateCaseIndicator;
    export.Next.Number = import.Next.Number;
    export.OspServiceProvider.LastName = import.OspServiceProvider.LastName;
    MoveOffice(import.OspOffice, export.OspOffice);
    MoveCsePersonsWorkSet(import.Ar, export.Ar);
    MoveCsePersonsWorkSet(import.Ap, export.ApCsePersonsWorkSet);
    export.InterstateCase.Assign(import.InterstateCase);
    export.Prev.OtherStateCaseStatus = import.Prev.OtherStateCaseStatus;

    // *** 11/17/00 M.L Start
    export.HeaderLine.Text35 = import.HeaderLine.Text35;

    // *** 11/17/00 M.L End
    export.ApCsePerson.Number = export.ApCsePersonsWorkSet.Number;

    // ---------------------------------------------
    // Move Imports to Exports
    // ---------------------------------------------
    export.InterstateRequest.Assign(import.InterstateRequest);
    export.H.Assign(import.H);
    export.InterstateContact.Assign(import.InterstateContact);
    MoveInterstateWorkArea(import.ContactPhone, export.ContactPhone);
    export.InterstateContactAddress.Assign(import.InterstateContactAddress);
    export.InterstatePaymentAddress.Assign(import.InterstatePaymentAddress);
    export.OtherState.Assign(import.OtherState);
    export.HotherState.StateAbbreviation = import.HotherState.StateAbbreviation;
    export.Agency.Description = import.Agency.Description;
    export.Note.Assign(import.Note);
    export.PersonPrompt.SelectChar = import.PersonPrompt.SelectChar;
    export.IreqCaseClosurePrompt.SelectChar =
      import.IreqCaseClosurePrompt.SelectChar;
    export.IreqCaseProgramPrompt.SelectChar =
      import.IreqCaseProgramPrompt.SelectChar;
    export.IreqCountryPrompt.SelectChar = import.IreqCountryPrompt.SelectChar;
    export.IreqTribalPrompt.SelectChar = import.IreqTribalPrompt.SelectChar;
    export.IreqStatePrompt.SelectChar = import.IreqStatePrompt.SelectChar;
    export.PaymentStatePrompt.SelectChar = import.PaymentStatePrompt.SelectChar;
    export.PaymentCountryPrompt.SelectChar =
      import.PaymentCountryPrompt.SelectChar;
    export.ContactStatePrompt.SelectChar = import.ContactStatePrompt.SelectChar;
    export.ContactCountryPrompt.SelectChar =
      import.ContactCountryPrompt.SelectChar;
    export.IreqCreatedDate.Date = import.IreqCreatedDate.Date;
    export.IreqUpdatedDate.Date = import.IreqUpdatedDate.Date;
    export.SelectedInterstateRequest.Assign(import.SelectedInterstateRequest);
    export.SelectedFips.StateAbbreviation =
      import.SelectedFips.StateAbbreviation;
    export.DupCaseIndicatorPrompt.SelectChar =
      import.DupCaseIndicatorPrompt.SelectChar;
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    local.Max.Date = UseCabSetMaximumDiscontinueDate();
    UseOeCabSetMnemonics();

    if (Equal(global.Command, "UPDATE") || Equal(global.Command, "REOPEN"))
    {
      if (!Equal(export.Next.Number, export.DisplayOnly.Number))
      {
        var field = GetField(export.Next, "number");

        field.Error = true;

        ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";
      }

      if (!Equal(export.OtherState.StateAbbreviation,
        export.HotherState.StateAbbreviation))
      {
        var field = GetField(export.OtherState, "stateAbbreviation");

        field.Error = true;

        ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";
      }

      if (!Equal(export.InterstateRequest.Country, export.H.Country))
      {
        var field = GetField(export.InterstateRequest, "country");

        field.Error = true;

        ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";
      }

      if (!Equal(export.InterstateRequest.TribalAgency, export.H.TribalAgency))
      {
        var field = GetField(export.InterstateRequest, "tribalAgency");

        field.Error = true;

        ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";
      }

      // *** CQ#318 Changes Begin Here ***
      if (Equal(global.Command, "REOPEN"))
      {
        if (AsChar(export.DisplayOnly.Status) == 'C')
        {
          var field = GetField(export.Next, "number");

          field.Error = true;

          ExitState = "SI0000_MUST_REOPEN_FROM_COMN";
        }
      }

      // *** CQ#318 Changes End   Here ***
      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    if (!Equal(export.Next.Number, export.DisplayOnly.Number) && !
      IsEmpty(export.DisplayOnly.Number))
    {
      export.OtherState.StateAbbreviation = "";
      export.OtherState.State = 0;
      export.InterstateRequest.Country = "";
      export.InterstateRequest.TribalAgency = "";
      export.InterstateRequest.OtherStateCaseId = "";
      export.InterstateRequest.CaseType = "";
      export.ApCsePersonsWorkSet.Number = "";
      export.ApCsePersonsWorkSet.FormattedName = "";
    }

    if (!IsEmpty(export.Next.Number))
    {
      local.ZeroFill.Text10 = export.Next.Number;
      UseEabPadLeftWithZeros();
      export.DisplayOnly.Number = local.ZeroFill.Text10;
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.RetcompInd.Flag = "N";

    if (Equal(global.Command, "RETCOMP"))
    {
      if (!IsEmpty(import.SelectedCsePersonsWorkSet.Number))
      {
        export.ApCsePerson.Number = import.SelectedCsePersonsWorkSet.Number;
        MoveCsePersonsWorkSet(import.SelectedCsePersonsWorkSet,
          export.ApCsePersonsWorkSet);
      }

      export.PersonPrompt.SelectChar = "";
      local.RetcompInd.Flag = "Y";
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "RETIREQ"))
    {
      if (!IsEmpty(import.SelectedFips.StateAbbreviation) || !
        IsEmpty(import.SelectedInterstateRequest.Country) || !
        IsEmpty(import.SelectedInterstateRequest.TribalAgency))
      {
        export.OtherState.StateAbbreviation =
          import.SelectedFips.StateAbbreviation;
        MoveInterstateRequest2(import.SelectedInterstateRequest,
          export.InterstateRequest);
        export.IreqStatePrompt.SelectChar = "";
        export.IreqCountryPrompt.SelectChar = "";
        export.IreqTribalPrompt.SelectChar = "";
      }

      global.Command = "DISPLAY";
    }

    // ---------------------------------------------
    //         	N E X T   T R A N
    // Use the CAB to nexttran to another procedure.
    // ---------------------------------------------
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      export.HiddenNextTranInfo.CaseNumber = export.Next.Number;
      export.HiddenNextTranInfo.CsePersonNumber =
        export.ApCsePersonsWorkSet.Number;
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

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.ApCsePersonsWorkSet.Number =
          export.HiddenNextTranInfo.CsePersonNumber ?? Spaces(10);
        export.Next.Number = export.HiddenNextTranInfo.CaseNumber ?? Spaces(10);
        global.Command = "DISPLAY";
      }
      else
      {
        return;
      }
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "RETCDVL"))
    {
      if (!IsEmpty(export.IreqStatePrompt.SelectChar))
      {
        export.OtherState.StateAbbreviation = import.SelectedCodeValue.Cdvalue;
        export.Agency.Description = import.SelectedCodeValue.Description;

        var field = GetField(export.OtherState, "stateAbbreviation");

        field.Protected = false;
        field.Focused = true;

        export.IreqStatePrompt.SelectChar = "";

        if (!IsEmpty(export.OtherState.StateAbbreviation))
        {
          local.StateCommon.State = export.OtherState.StateAbbreviation;
          UseSiValidateStateFips();

          if (AsChar(local.FipsError.Flag) == 'Y')
          {
            var field1 = GetField(export.OtherState, "stateAbbreviation");

            field1.Error = true;

            return;
          }
          else
          {
            export.InterstateRequest.OtherStateFips = export.OtherState.State;
            export.OtherStateFips.CountyFips =
              NumberToString(export.OtherState.County, 3);
            export.OtherStateFips.LocationFips =
              NumberToString(export.OtherState.Location, 2);
          }
        }
      }
      else if (!IsEmpty(export.IreqCountryPrompt.SelectChar))
      {
        export.InterstateRequest.Country = import.SelectedCodeValue.Cdvalue;

        var field = GetField(export.InterstateRequest, "country");

        field.Protected = false;
        field.Focused = true;

        export.Agency.Description = import.SelectedCodeValue.Description;
      }
      else if (!IsEmpty(export.IreqTribalPrompt.SelectChar))
      {
        export.InterstateRequest.TribalAgency =
          import.SelectedCodeValue.Cdvalue;

        var field = GetField(export.InterstateRequest, "tribalAgency");

        field.Protected = false;
        field.Focused = true;

        export.Agency.Description = import.SelectedCodeValue.Description;
      }
      else if (!IsEmpty(export.IreqCaseClosurePrompt.SelectChar))
      {
        export.InterstateRequest.OtherStateCaseClosureReason =
          import.SelectedCodeValue.Cdvalue;

        var field =
          GetField(export.InterstateRequest, "otherStateCaseClosureReason");

        field.Protected = false;
        field.Focused = true;
      }
      else if (!IsEmpty(export.IreqCaseProgramPrompt.SelectChar))
      {
        export.InterstateRequest.CaseType = import.SelectedCodeValue.Cdvalue;

        var field = GetField(export.InterstateRequest, "caseType");

        field.Protected = false;
        field.Focused = true;
      }
      else if (!IsEmpty(export.ContactStatePrompt.SelectChar))
      {
        export.InterstateContactAddress.State =
          import.SelectedCodeValue.Cdvalue;

        var field = GetField(export.InterstateContactAddress, "state");

        field.Protected = false;
        field.Focused = true;
      }
      else if (!IsEmpty(export.ContactCountryPrompt.SelectChar))
      {
        export.InterstateContactAddress.Country =
          import.SelectedCodeValue.Cdvalue;

        var field = GetField(export.InterstateContactAddress, "country");

        field.Protected = false;
        field.Focused = true;
      }
      else if (!IsEmpty(export.PaymentStatePrompt.SelectChar))
      {
        export.InterstatePaymentAddress.State =
          import.SelectedCodeValue.Cdvalue;

        var field = GetField(export.InterstatePaymentAddress, "state");

        field.Protected = false;
        field.Focused = true;
      }
      else if (!IsEmpty(export.PaymentCountryPrompt.SelectChar))
      {
        export.InterstatePaymentAddress.Country =
          import.SelectedCodeValue.Cdvalue;

        var field = GetField(export.InterstatePaymentAddress, "country");

        field.Protected = false;
        field.Focused = true;
      }
      else if (!IsEmpty(export.DupCaseIndicatorPrompt.SelectChar))
      {
        if (!IsEmpty(import.SelectedCodeValue.Cdvalue))
        {
          export.DisplayOnly.DuplicateCaseIndicator =
            import.SelectedCodeValue.Cdvalue;
        }

        if (AsChar(export.DisplayOnly.DuplicateCaseIndicator) == 'S')
        {
          export.DisplayOnly.DuplicateCaseIndicator = "";
        }
      }

      export.IreqStatePrompt.SelectChar = "";
      export.IreqCountryPrompt.SelectChar = "";
      export.IreqTribalPrompt.SelectChar = "";
      export.IreqCaseClosurePrompt.SelectChar = "";
      export.IreqCaseProgramPrompt.SelectChar = "";
      export.ContactStatePrompt.SelectChar = "";
      export.ContactCountryPrompt.SelectChar = "";
      export.PaymentStatePrompt.SelectChar = "";
      export.PaymentCountryPrompt.SelectChar = "";
      export.DupCaseIndicatorPrompt.SelectChar = "";

      return;
    }

    // ---------------------------------------------
    //        E D I T   C H E C K
    // ---------------------------------------------
    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
    {
      // **************************************************************
      //      Validations for 5 digits and 4 digit zip codes.
      // **************************************************************
      if (Length(TrimEnd(export.InterstateContactAddress.ZipCode)) > 0 && Length
        (TrimEnd(export.InterstateContactAddress.ZipCode)) < 5)
      {
        var field = GetField(export.InterstateContactAddress, "zipCode");

        field.Error = true;

        ExitState = "OE0000_ZIP_CODE_MUST_BE_5_DIGITS";

        return;
      }

      if (Length(TrimEnd(export.InterstateContactAddress.ZipCode)) > 0 && Verify
        (export.InterstateContactAddress.ZipCode, "0123456789") != 0)
      {
        var field = GetField(export.InterstateContactAddress, "zipCode");

        field.Error = true;

        ExitState = "SI0000_ZIP_CODE_MUST_BE_NUMERIC";

        return;
      }

      if (Length(TrimEnd(export.InterstateContactAddress.ZipCode)) == 0 && Length
        (TrimEnd(export.InterstateContactAddress.Zip4)) > 0)
      {
        var field = GetField(export.InterstateContactAddress, "zipCode");

        field.Error = true;

        ExitState = "SI0000_ZIP_5_DGT_REQ_4_DGTS_REQ";

        return;
      }

      if (Length(TrimEnd(export.InterstateContactAddress.ZipCode)) > 0 && Length
        (TrimEnd(export.InterstateContactAddress.Zip4)) > 0)
      {
        if (Length(TrimEnd(export.InterstateContactAddress.Zip4)) < 4)
        {
          var field = GetField(export.InterstateContactAddress, "zip4");

          field.Error = true;

          ExitState = "SI0000_ZIP_PLS_4_REQ_4_DGTS_OR_0";

          return;
        }
        else if (Verify(export.InterstateContactAddress.Zip4, "0123456789") != 0
          )
        {
          var field = GetField(export.InterstateContactAddress, "zip4");

          field.Error = true;

          ExitState = "SI0000_ZIP_CODE_MUST_BE_NUMERIC";

          return;
        }
      }

      // **************************************************************
      //      Validations for 5 digits and 4 digit zip codes.
      // **************************************************************
      if (Length(TrimEnd(export.InterstatePaymentAddress.ZipCode)) > 0 && Length
        (TrimEnd(export.InterstatePaymentAddress.ZipCode)) < 5)
      {
        var field = GetField(export.InterstatePaymentAddress, "zipCode");

        field.Error = true;

        ExitState = "OE0000_ZIP_CODE_MUST_BE_5_DIGITS";

        return;
      }

      if (Length(TrimEnd(export.InterstatePaymentAddress.ZipCode)) > 0 && Verify
        (export.InterstatePaymentAddress.ZipCode, "0123456789") != 0)
      {
        var field = GetField(export.InterstatePaymentAddress, "zipCode");

        field.Error = true;

        ExitState = "SI0000_ZIP_CODE_MUST_BE_NUMERIC";

        return;
      }

      if (Length(TrimEnd(export.InterstatePaymentAddress.ZipCode)) == 0 && Length
        (TrimEnd(export.InterstatePaymentAddress.Zip4)) > 0)
      {
        var field = GetField(export.InterstatePaymentAddress, "zipCode");

        field.Error = true;

        ExitState = "SI0000_ZIP_5_DGT_REQ_4_DGTS_REQ";

        return;
      }

      if (Length(TrimEnd(export.InterstatePaymentAddress.ZipCode)) > 0 && Length
        (TrimEnd(export.InterstatePaymentAddress.Zip4)) > 0)
      {
        if (Length(TrimEnd(export.InterstatePaymentAddress.Zip4)) < 4)
        {
          var field = GetField(export.InterstatePaymentAddress, "zip4");

          field.Error = true;

          ExitState = "SI0000_ZIP_PLS_4_REQ_4_DGTS_OR_0";

          return;
        }
        else if (Verify(export.InterstatePaymentAddress.Zip4, "0123456789") != 0
          )
        {
          var field = GetField(export.InterstatePaymentAddress, "zip4");

          field.Error = true;

          ExitState = "SI0000_ZIP_CODE_MUST_BE_NUMERIC";

          return;
        }
      }

      if (IsEmpty(export.DisplayOnly.Number) || !
        Equal(export.DisplayOnly.Number, export.Next.Number))
      {
        var field = GetField(export.Next, "number");

        field.Error = true;

        ExitState = "ACO_NE0000_DISPLAY_FIRST";

        return;
      }

      if (AsChar(export.DisplayOnly.DuplicateCaseIndicator) != 'Y' && AsChar
        (export.DisplayOnly.DuplicateCaseIndicator) != 'N' && !
        IsEmpty(export.DisplayOnly.DuplicateCaseIndicator))
      {
        var field = GetField(export.DisplayOnly, "duplicateCaseIndicator");

        field.Error = true;

        ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";
      }

      if (AsChar(export.DisplayOnly.DuplicateCaseIndicator) == 'Y' && AsChar
        (export.Hduplicate.DuplicateCaseIndicator) != 'Y')
      {
        local.CaseMarkedDuplicate.Flag = "Y";
      }

      // --  Validate State, Country, and Tribal Agency
      // Determine how many State, Country, and Tribal agency were entered.
      local.Common1.Count = 0;

      if (!IsEmpty(export.OtherState.StateAbbreviation))
      {
        ++local.Common1.Count;
      }

      if (!IsEmpty(export.InterstateRequest.Country))
      {
        ++local.Common1.Count;
      }

      if (!IsEmpty(export.InterstateRequest.TribalAgency))
      {
        ++local.Common1.Count;
      }

      if (local.Common1.Count == 1)
      {
        // Only one of State, Country, and Tribal agency were entered.  Validate
        // the entry.
        if (!IsEmpty(export.OtherState.StateAbbreviation))
        {
          if (Equal(export.OtherState.StateAbbreviation, "KS"))
          {
            // KS is not allowed as a State agency in this situation.
            var field = GetField(export.OtherState, "stateAbbreviation");

            field.Error = true;

            ExitState = "SI0000_CANT_USE_KS_4_STATE_CODE";
          }
          else
          {
            // Validate the entered value against the State agency code table.
            local.Validation.Cdvalue = export.OtherState.StateAbbreviation;
            UseCabValidateCodeValue6();

            if (AsChar(local.Error.Flag) != 'Y')
            {
              var field = GetField(export.OtherState, "stateAbbreviation");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_STATE_CODE";
            }
          }
        }

        if (!IsEmpty(export.InterstateRequest.Country))
        {
          if (Equal(export.InterstateRequest.Country, "US"))
          {
            // US is not allowed as a Foreign agency in this situation.
            var field = GetField(export.InterstateRequest, "country");

            field.Error = true;

            ExitState = "SI0000_CANT_USE_US_FOR_COUNTRY";
          }
          else
          {
            // Validate the entered value against the Country code table.
            local.Validation.Cdvalue = export.InterstateRequest.Country ?? Spaces
              (10);
            UseCabValidateCodeValue4();

            if (AsChar(local.Error.Flag) != 'Y')
            {
              var field = GetField(export.InterstateRequest, "country");

              field.Error = true;

              ExitState = "LE0000_INVALID_COUNTRY_CODE";
            }
          }
        }

        if (!IsEmpty(export.InterstateRequest.TribalAgency))
        {
          // Validate the entered value against the Tribal agency code table.
          local.Validation.Cdvalue = export.InterstateRequest.TribalAgency ?? Spaces
            (10);
          UseCabValidateCodeValue1();

          if (AsChar(local.Error.Flag) != 'Y')
          {
            var field = GetField(export.InterstateRequest, "tribalAgency");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_TRIBAL_AGENCY";
          }
        }
      }
      else
      {
        // -- Either none or more than one of State, Foreign, and Tribal agency 
        // were entered.
        var field1 = GetField(export.InterstateRequest, "tribalAgency");

        field1.Error = true;

        var field2 = GetField(export.InterstateRequest, "country");

        field2.Error = true;

        var field3 = GetField(export.OtherState, "stateAbbreviation");

        field3.Error = true;

        export.Agency.Description = Spaces(CodeValue.Description_MaxLength);
        ExitState = "SI0000_IVD_AGENCY_REQUIRED";
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      if (IsEmpty(export.InterstateRequest.OtherStateCaseId))
      {
        var field = GetField(export.InterstateRequest, "otherStateCaseId");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
      }

      if (IsEmpty(export.InterstateRequest.OtherStateCaseStatus))
      {
      }
      else if (AsChar(export.InterstateRequest.OtherStateCaseStatus) != 'O' && AsChar
        (export.InterstateRequest.OtherStateCaseStatus) != 'C')
      {
        var field = GetField(export.InterstateRequest, "otherStateCaseStatus");

        field.Error = true;

        ExitState = "SI0000_INVALID_CASE_STATUS";
      }

      if (!IsEmpty(export.InterstateRequest.OtherStateCaseClosureReason))
      {
        local.Validation.Cdvalue =
          export.InterstateRequest.OtherStateCaseClosureReason ?? Spaces(10);
        UseCabValidateCodeValue3();

        if (AsChar(local.Error.Flag) != 'Y')
        {
          var field =
            GetField(export.InterstateRequest, "otherStateCaseClosureReason");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";
        }
      }

      // >>
      // Closing a case should only happen when doing an update.
      if (Equal(global.Command, "UPDATE"))
      {
        if (AsChar(export.InterstateRequest.OtherStateCaseStatus) == 'C')
        {
          if (Equal(export.InterstateRequest.OtherStateCaseClosureDate,
            local.Zero.Date))
          {
            var field1 =
              GetField(export.InterstateRequest, "otherStateCaseClosureDate");

            field1.Error = true;

            var field2 =
              GetField(export.InterstateRequest, "otherStateCaseStatus");

            field2.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }

          if (IsEmpty(export.InterstateRequest.OtherStateCaseClosureReason))
          {
            var field1 =
              GetField(export.InterstateRequest, "otherStateCaseClosureReason");
              

            field1.Error = true;

            var field2 =
              GetField(export.InterstateRequest, "otherStateCaseStatus");

            field2.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }
        }

        // *** CQ#318 Changes Begin Here ***
        if (!IsEmpty(export.InterstateRequest.OtherStateCaseClosureReason))
        {
          if (AsChar(export.InterstateRequest.OtherStateCaseStatus) != 'C')
          {
            var field1 =
              GetField(export.InterstateRequest, "otherStateCaseClosureReason");
              

            field1.Error = true;

            var field2 =
              GetField(export.InterstateRequest, "otherStateCaseStatus");

            field2.Error = true;

            ExitState = "SI0000_CASE_STAT_MUST_BE_CLOSED";
          }
        }

        // *** CQ#318 Changes End   Here ***
      }

      if (IsEmpty(export.InterstateRequest.CaseType))
      {
        var field = GetField(export.InterstateRequest, "caseType");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
      }
      else
      {
        local.Validation.Cdvalue = export.InterstateRequest.CaseType ?? Spaces
          (10);
        UseCabValidateCodeValue2();

        if (AsChar(local.Error.Flag) != 'Y')
        {
          var field = GetField(export.InterstateRequest, "caseType");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";
        }
      }

      // CQ62215
      if (export.InterstateContact.ContactFaxAreaCode.GetValueOrDefault() > 0
        || export.InterstateContact.ContactFaxNumber.GetValueOrDefault() > 0)
      {
        if (export.InterstateContact.ContactFaxNumber.GetValueOrDefault() == 0)
        {
          var field = GetField(export.InterstateContact, "contactFaxNumber");

          field.Error = true;

          ExitState = "OE0000_FAX_NO_REQD";
        }

        if (export.InterstateContact.ContactFaxAreaCode.GetValueOrDefault() == 0
          )
        {
          var field = GetField(export.InterstateContact, "contactFaxAreaCode");

          field.Error = true;

          ExitState = "OE0000_FAX_AREA_CODE_REQD";
        }
      }

      if (!IsEmpty(export.InterstateContact.ContactPhoneExtension))
      {
        if (export.InterstateContact.ContactPhoneNum.GetValueOrDefault() == 0)
        {
          var field =
            GetField(export.InterstateContact, "contactPhoneExtension");

          field.Error = true;

          ExitState = "ACO_NE0000_EXTENSION_WO_PHONE_NO";
        }
      }

      if (export.InterstateContact.AreaCode.GetValueOrDefault() > 0 || export
        .InterstateContact.ContactPhoneNum.GetValueOrDefault() > 0)
      {
        if (export.InterstateContact.ContactPhoneNum.GetValueOrDefault() == 0)
        {
          var field = GetField(export.InterstateContact, "contactPhoneNum");

          field.Error = true;

          ExitState = "ACO_NE0000_PHONE_NO_REQD";
        }

        if (export.InterstateContact.AreaCode.GetValueOrDefault() == 0)
        {
          var field = GetField(export.InterstateContact, "areaCode");

          field.Error = true;

          ExitState = "CO0000_PHONE_AREA_CODE_REQD";
        }
      }

      if (IsEmpty(export.InterstateContactAddress.Street1) && IsEmpty
        (export.InterstateContactAddress.Street3))
      {
        var field1 = GetField(export.InterstateContactAddress, "street1");

        field1.Error = true;

        var field2 = GetField(export.InterstateContactAddress, "street3");

        field2.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
      }

      if (IsEmpty(export.InterstateContactAddress.City) && IsEmpty
        (export.InterstateContactAddress.Province))
      {
        var field1 = GetField(export.InterstateContactAddress, "city");

        field1.Error = true;

        var field2 = GetField(export.InterstateContactAddress, "province");

        field2.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
      }

      if (IsEmpty(export.InterstateContactAddress.State) && IsEmpty
        (export.InterstateContactAddress.Country))
      {
        var field1 = GetField(export.InterstateContactAddress, "state");

        field1.Error = true;

        var field2 = GetField(export.InterstateContactAddress, "country");

        field2.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
      }

      if (IsEmpty(export.InterstateContactAddress.ZipCode) && IsEmpty
        (export.InterstateContactAddress.PostalCode))
      {
        var field1 = GetField(export.InterstateContactAddress, "zipCode");

        field1.Error = true;

        var field2 = GetField(export.InterstateContactAddress, "postalCode");

        field2.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
      }

      // --02/12/19 GVandy  CQ65253  Allow domestic payment addresses for 
      // interstate cases
      //   from foreign countries.  Insure that the payment address is either 
      // domestic or
      //   foreign, not a combination of both.
      if ((!IsEmpty(export.InterstatePaymentAddress.Street1) || !
        IsEmpty(export.InterstatePaymentAddress.Street2) || !
        IsEmpty(export.InterstatePaymentAddress.City) || !
        IsEmpty(export.InterstatePaymentAddress.State) || !
        IsEmpty(export.InterstatePaymentAddress.ZipCode) || !
        IsEmpty(export.InterstatePaymentAddress.Zip4)) && (
          !IsEmpty(export.InterstatePaymentAddress.Street3) || !
        IsEmpty(export.InterstatePaymentAddress.Street4) || !
        IsEmpty(export.InterstatePaymentAddress.Province) || !
        IsEmpty(export.InterstatePaymentAddress.PostalCode) || !
        IsEmpty(export.InterstatePaymentAddress.Country)))
      {
        var field1 = GetField(export.InterstatePaymentAddress, "street1");

        field1.Error = true;

        var field2 = GetField(export.InterstatePaymentAddress, "street2");

        field2.Error = true;

        var field3 = GetField(export.InterstatePaymentAddress, "city");

        field3.Error = true;

        var field4 = GetField(export.InterstatePaymentAddress, "state");

        field4.Error = true;

        var field5 = GetField(export.InterstatePaymentAddress, "zipCode");

        field5.Error = true;

        var field6 = GetField(export.InterstatePaymentAddress, "zip4");

        field6.Error = true;

        var field7 = GetField(export.InterstatePaymentAddress, "street3");

        field7.Error = true;

        var field8 = GetField(export.InterstatePaymentAddress, "street4");

        field8.Error = true;

        var field9 = GetField(export.InterstatePaymentAddress, "province");

        field9.Error = true;

        var field10 = GetField(export.InterstatePaymentAddress, "postalCode");

        field10.Error = true;

        var field11 = GetField(export.InterstatePaymentAddress, "country");

        field11.Error = true;

        ExitState = "SI0000_DOMESTIC_OR_FOREIGN";
      }

      if (IsEmpty(export.InterstatePaymentAddress.Street1) && IsEmpty
        (export.InterstatePaymentAddress.Street3))
      {
        var field1 = GetField(export.InterstatePaymentAddress, "street1");

        field1.Error = true;

        var field2 = GetField(export.InterstatePaymentAddress, "street3");

        field2.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
      }

      if (IsEmpty(export.InterstatePaymentAddress.City) && IsEmpty
        (export.InterstatePaymentAddress.Province))
      {
        var field1 = GetField(export.InterstatePaymentAddress, "city");

        field1.Error = true;

        var field2 = GetField(export.InterstatePaymentAddress, "province");

        field2.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
      }

      if (IsEmpty(export.InterstatePaymentAddress.State) && IsEmpty
        (export.InterstatePaymentAddress.Country))
      {
        var field1 = GetField(export.InterstatePaymentAddress, "state");

        field1.Error = true;

        var field2 = GetField(export.InterstatePaymentAddress, "country");

        field2.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
      }

      if (IsEmpty(export.InterstatePaymentAddress.ZipCode) && IsEmpty
        (export.InterstatePaymentAddress.PostalCode))
      {
        var field1 = GetField(export.InterstatePaymentAddress, "zipCode");

        field1.Error = true;

        var field2 = GetField(export.InterstatePaymentAddress, "postalCode");

        field2.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
      }

      if (!IsEmpty(export.InterstateContactAddress.Street1) || !
        IsEmpty(export.InterstateContactAddress.Street3) || !
        IsEmpty(export.InterstateContactAddress.City) || !
        IsEmpty(export.InterstateContactAddress.State) || !
        IsEmpty(export.InterstateContactAddress.Country))
      {
        if (!IsEmpty(export.InterstateContactAddress.Street1) && IsEmpty
          (export.InterstateContactAddress.City))
        {
          var field = GetField(export.InterstateContactAddress, "city");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }

        if (!IsEmpty(export.InterstateContactAddress.Street3) && IsEmpty
          (export.InterstateContactAddress.Province))
        {
          var field = GetField(export.InterstateContactAddress, "province");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }

        if (!IsEmpty(export.OtherState.StateAbbreviation))
        {
          if (!IsEmpty(export.InterstateContactAddress.Street1) && IsEmpty
            (export.InterstateContactAddress.State))
          {
            var field = GetField(export.InterstateContactAddress, "state");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }
          else
          {
            local.Validation.Cdvalue =
              export.InterstateContactAddress.State ?? Spaces(10);
            UseCabValidateCodeValue6();

            if (AsChar(local.Error.Flag) != 'Y' && !
              IsEmpty(export.InterstateContactAddress.Street1) && IsEmpty
              (export.InterstateContactAddress.State))
            {
              var field = GetField(export.InterstateContactAddress, "state");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_STATE_CODE";
            }
          }

          if (!IsEmpty(export.InterstateContactAddress.Street1) && IsEmpty
            (export.InterstateContactAddress.ZipCode))
          {
            var field = GetField(export.InterstateContactAddress, "zipCode");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }
        }

        if (!IsEmpty(export.InterstateRequest.Country))
        {
          if (IsEmpty(export.InterstateContactAddress.Country))
          {
            var field = GetField(export.InterstateContactAddress, "country");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }
          else
          {
            local.Validation.Cdvalue =
              export.InterstateContactAddress.Country ?? Spaces(10);
            UseCabValidateCodeValue4();

            if (AsChar(local.Error.Flag) != 'Y')
            {
              var field = GetField(export.InterstateContactAddress, "country");

              field.Error = true;

              ExitState = "LE0000_INVALID_COUNTRY_CODE";
            }
          }

          if (IsEmpty(export.InterstateContactAddress.PostalCode))
          {
            var field = GetField(export.InterstateContactAddress, "postalCode");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }
        }
      }

      if (!IsEmpty(export.OtherState.StateAbbreviation))
      {
        if (!IsEmpty(export.InterstatePaymentAddress.State))
        {
          local.Validation.Cdvalue = export.InterstatePaymentAddress.State ?? Spaces
            (10);
          UseCabValidateCodeValue6();

          if (AsChar(local.Error.Flag) != 'Y')
          {
            var field = GetField(export.InterstatePaymentAddress, "state");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_STATE_CODE";
          }
        }
      }

      if (!IsEmpty(export.InterstateRequest.Country))
      {
        if (IsEmpty(export.InterstatePaymentAddress.Country))
        {
          // --02/12/19 GVandy  CQ65253  Allow domestic payment addresses for 
          // interstate cases
          //   from foreign countries.
        }
        else
        {
          local.Validation.Cdvalue =
            export.InterstatePaymentAddress.Country ?? Spaces(10);
          UseCabValidateCodeValue4();

          if (AsChar(local.Error.Flag) != 'Y')
          {
            var field = GetField(export.InterstatePaymentAddress, "country");

            field.Error = true;

            ExitState = "LE0000_INVALID_COUNTRY_CODE";
          }
        }
      }

      if (!IsEmpty(export.InterstateContactAddress.Country))
      {
        local.CodeValue.Cdvalue = export.InterstateContactAddress.Country ?? Spaces
          (10);
        local.Code.CodeName = "COUNTRY CODE";
        UseCabValidateCodeValue8();

        if (AsChar(local.Common2.Flag) == 'N')
        {
          var field = GetField(export.InterstateContactAddress, "country");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_CODE";
        }
      }

      if (!IsEmpty(export.InterstatePaymentAddress.Country))
      {
        local.CodeValue.Cdvalue = export.InterstatePaymentAddress.Country ?? Spaces
          (10);
        local.Code.CodeName = "COUNTRY CODE";
        UseCabValidateCodeValue8();

        if (AsChar(local.Common2.Flag) == 'N')
        {
          var field = GetField(export.InterstatePaymentAddress, "country");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_CODE";
        }
      }

      if (IsEmpty(export.InterstatePaymentAddress.PayableToName))
      {
        var field = GetField(export.InterstatePaymentAddress, "payableToName");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

        return;
      }

      if (!IsEmpty(export.InterstatePaymentAddress.Street3) && IsEmpty
        (import.InterstatePaymentAddress.Province))
      {
        var field = GetField(export.InterstatePaymentAddress, "province");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

        return;
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    if (!IsEmpty(export.OtherState.StateAbbreviation))
    {
      local.StateCommon.State = export.OtherState.StateAbbreviation;
      UseSiValidateStateFips();

      if (AsChar(local.FipsError.Flag) == 'Y')
      {
        var field = GetField(export.OtherState, "stateAbbreviation");

        field.Error = true;

        return;
      }
      else
      {
        export.InterstateRequest.OtherStateFips = export.OtherState.State;
        export.OtherStateFips.CountyFips =
          NumberToString(export.OtherState.County, 3);
        export.OtherStateFips.LocationFips =
          NumberToString(export.OtherState.Location, 2);
      }

      local.Validation.Cdvalue = export.OtherState.StateAbbreviation;
      UseCabValidateCodeValue7();

      if (AsChar(local.Error.Flag) != 'Y')
      {
        var field = GetField(export.OtherState, "stateAbbreviation");

        field.Error = true;

        ExitState = "INVALID_STATE_ABBREVIATION";

        return;
      }
    }
    else
    {
      // -- 6/27/03 GVandy PR180512  Clear out FIPS info if the user selected a 
      // country.  Otherwise, previous FIPS
      //    state info is being stored in the interstate_request.
      export.OtherState.Assign(local.RefreshFips);
      export.InterstateRequest.OtherStateFips = local.RefreshFips.State;
    }

    if (!IsEmpty(export.InterstateRequest.Country))
    {
      local.Validation.Cdvalue = export.InterstateRequest.Country ?? Spaces(10);
      UseCabValidateCodeValue5();

      if (AsChar(local.Error.Flag) != 'Y')
      {
        var field = GetField(export.InterstateRequest, "country");

        field.Error = true;

        ExitState = "LE0000_INVALID_COUNTRY_CODE";

        return;
      }
    }

    if (!IsEmpty(export.InterstateRequest.TribalAgency))
    {
      local.Validation.Cdvalue = export.InterstateRequest.TribalAgency ?? Spaces
        (10);
      UseCabValidateCodeValue1();

      if (AsChar(local.Error.Flag) != 'Y')
      {
        var field = GetField(export.InterstateRequest, "tribalAgency");

        field.Error = true;

        ExitState = "ACO_NE0000_INVALID_TRIBAL_AGENCY";

        return;
      }
    }

    // ---------------------------------------------
    //        S E C U R I T Y   L O G I C
    // ---------------------------------------------
    // to validate action level security
    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE") || Equal
      (global.Command, "DISPLAY") || Equal(global.Command, "REOPEN"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // ---------------------------------------------
    //        P F K E Y   P R O C E S S I N G
    // ---------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "IIOI":
        if (IsEmpty(export.DisplayOnly.Number) || !
          Equal(export.DisplayOnly.Number, export.Next.Number))
        {
          var field = GetField(export.Next, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_DISPLAY_FIRST";

          return;
        }

        if (!IsEmpty(export.InterstateRequest.Country))
        {
          var field = GetField(export.InterstateRequest, "country");

          field.Error = true;

          ExitState = "ACO_NE0000_NOT_DOMESTIC_CASE";

          return;
        }

        if (!IsEmpty(export.InterstateRequest.TribalAgency))
        {
          var field = GetField(export.InterstateRequest, "tribalAgency");

          field.Error = true;

          ExitState = "ACO_NE0000_NOT_DOMESTIC_CASE";

          return;
        }

        ExitState = "ECO_LNK_TO_IIOI";

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        return;
      case "UPDATE":
        if (AsChar(export.Prev.OtherStateCaseStatus) == 'C')
        {
          ExitState = "SI0000_UPDATE_NOT_ALLOWED_CASE_C";

          return;
        }
        else
        {
          export.InterstateRequest.OtherStateCaseClosureDate =
            local.Current.Date;
        }

        if (!Equal(export.H.CaseType, export.InterstateRequest.CaseType))
        {
          local.ChangeProgram.Flag = "Y";
        }

        if (AsChar(export.InterstateRequest.KsCaseInd) == 'Y' && AsChar
          (export.InterstateRequest.OtherStateCaseStatus) == 'O')
        {
          ExitState = "CASE_NOT_IC_INTERSTATE";

          break;
        }

        if (IsEmpty(export.InterstateRequest.KsCaseInd) && IsEmpty
          (export.InterstateRequest.OtherStateCaseStatus))
        {
          ExitState = "CASE_NOT_IC_INTERSTATE";

          break;
        }

        if (IsEmpty(export.InterstateRequest.KsCaseInd) && AsChar
          (export.InterstateRequest.OtherStateCaseStatus) == 'O')
        {
          ExitState = "CASE_NOT_IC_INTERSTATE";

          break;
        }

        if (AsChar(export.H.OtherStateCaseStatus) == 'O' && AsChar
          (export.InterstateRequest.OtherStateCaseStatus) == 'C')
        {
          local.CaseClosed.Flag = "Y";
        }

        // -- Give a warning message if the address state/country does not match
        // the IV-D agency location.
        if (IsEmpty(import.AddressMismatch.Flag))
        {
          if (!IsEmpty(export.OtherState.StateAbbreviation))
          {
            if (!Equal(export.OtherState.StateAbbreviation,
              export.InterstateContactAddress.State))
            {
              var field1 = GetField(export.OtherState, "stateAbbreviation");

              field1.Color = "yellow";
              field1.Protected = false;
              field1.Focused = true;

              var field2 = GetField(export.InterstateContactAddress, "state");

              field2.Color = "yellow";
              field2.Protected = false;

              export.AddressMismatch.Flag = "Y";
            }

            if (!Equal(export.OtherState.StateAbbreviation,
              export.InterstatePaymentAddress.State))
            {
              var field1 = GetField(export.OtherState, "stateAbbreviation");

              field1.Color = "yellow";
              field1.Protected = false;
              field1.Focused = true;

              var field2 = GetField(export.InterstatePaymentAddress, "state");

              field2.Color = "yellow";
              field2.Protected = false;

              export.AddressMismatch.Flag = "Y";
            }

            if (!IsEmpty(export.InterstateContactAddress.Country))
            {
              var field = GetField(export.InterstateContactAddress, "country");

              field.Color = "yellow";
              field.Protected = false;

              export.AddressMismatch.Flag = "Y";
            }

            if (!IsEmpty(export.InterstatePaymentAddress.Country))
            {
              var field = GetField(export.InterstatePaymentAddress, "country");

              field.Color = "yellow";
              field.Protected = false;

              export.AddressMismatch.Flag = "Y";
            }
          }
          else if (!IsEmpty(export.InterstateRequest.Country))
          {
            if (!Equal(export.InterstateRequest.Country,
              export.InterstateContactAddress.Country))
            {
              var field1 = GetField(export.InterstateRequest, "country");

              field1.Color = "yellow";
              field1.Protected = false;
              field1.Focused = true;

              var field2 = GetField(export.InterstateContactAddress, "country");

              field2.Color = "yellow";
              field2.Protected = false;

              export.AddressMismatch.Flag = "Y";
            }

            if (!Equal(export.InterstateRequest.Country,
              export.InterstatePaymentAddress.Country))
            {
              var field1 = GetField(export.InterstateRequest, "country");

              field1.Color = "yellow";
              field1.Protected = false;
              field1.Focused = true;

              var field2 = GetField(export.InterstatePaymentAddress, "country");

              field2.Color = "yellow";
              field2.Protected = false;

              export.AddressMismatch.Flag = "Y";
            }

            if (!IsEmpty(export.InterstateContactAddress.State))
            {
              var field = GetField(export.InterstateContactAddress, "state");

              field.Color = "yellow";
              field.Protected = false;

              export.AddressMismatch.Flag = "Y";
            }

            if (!IsEmpty(export.InterstatePaymentAddress.State))
            {
              var field = GetField(export.InterstatePaymentAddress, "state");

              field.Color = "yellow";
              field.Protected = false;

              export.AddressMismatch.Flag = "Y";
            }
          }
          else if (!IsEmpty(export.InterstateRequest.TribalAgency))
          {
            local.TribalAgencyCommon.State =
              Substring(export.Agency.Description, 1, 2);

            if (!Equal(local.TribalAgencyCommon.State,
              export.InterstateContactAddress.State))
            {
              var field1 = GetField(export.InterstateRequest, "tribalAgency");

              field1.Color = "yellow";
              field1.Protected = false;
              field1.Focused = true;

              var field2 = GetField(export.InterstateContactAddress, "state");

              field2.Color = "yellow";
              field2.Protected = false;

              export.AddressMismatch.Flag = "Y";
            }

            if (!Equal(local.TribalAgencyCommon.State,
              export.InterstatePaymentAddress.State))
            {
              var field1 = GetField(export.InterstateRequest, "tribalAgency");

              field1.Color = "yellow";
              field1.Protected = false;
              field1.Focused = true;

              var field2 = GetField(export.InterstatePaymentAddress, "state");

              field2.Color = "yellow";
              field2.Protected = false;

              export.AddressMismatch.Flag = "Y";
            }

            if (!IsEmpty(export.InterstateContactAddress.Country))
            {
              var field = GetField(export.InterstateContactAddress, "country");

              field.Color = "yellow";
              field.Protected = false;

              export.AddressMismatch.Flag = "Y";
            }

            if (!IsEmpty(export.InterstatePaymentAddress.Country))
            {
              var field = GetField(export.InterstatePaymentAddress, "country");

              field.Color = "yellow";
              field.Protected = false;

              export.AddressMismatch.Flag = "Y";
            }
          }

          if (AsChar(export.AddressMismatch.Flag) == 'Y')
          {
            ExitState = "SI0000_IVD_ADDRESS_MISMATCH_UPDT";

            return;
          }
        }

        UseSiIimcUpdateIncomingIsCase();
        export.Prev.OtherStateCaseStatus =
          export.InterstateRequest.OtherStateCaseStatus;

        if (IsExitState("ACO_NE0000_INVALID_CODE") || IsExitState
          ("SI0000_INVALID_CODE_CASE_NEVER_D"))
        {
          var field = GetField(export.DisplayOnly, "duplicateCaseIndicator");

          field.Error = true;
        }

        if (!IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE"))
        {
          UseEabRollbackCics();

          return;
        }

        export.Hduplicate.DuplicateCaseIndicator =
          export.DisplayOnly.DuplicateCaseIndicator ?? "";

        if (AsChar(export.InterstateRequest.OtherStateCaseStatus) == 'C' && Equal
          (export.InterstateRequest.OtherStateCaseClosureDate, Now().Date))
        {
          if (IsEmpty(export.DisplayOnly.Number) || !
            Equal(export.DisplayOnly.Number, export.Next.Number))
          {
            var field = GetField(export.Next, "number");

            field.Error = true;

            ExitState = "ACO_NE0000_DISPLAY_FIRST";

            return;
          }

          if (AsChar(local.CaseStatusChangedInd.Flag) == 'Y')
          {
            // -------------------------------------------------------------------
            // Add check to see if state is CSENet ready and if we are 
            // exchanging information with them for this specific transaction
            // type.
            // -------------------------------------------------------------------
            if (export.InterstateRequest.OtherStateFips == 0)
            {
              ExitState = "SI0000_CLOSED_SEND_MANUAL_NOTICE";

              break;
            }

            if (IsEmpty(export.OtherState.StateAbbreviation))
            {
              ExitState = "SI0000_CLOSED_SEND_MANUAL_NOTICE";

              break;
            }

            local.CsenetStateTable.StateCode =
              export.OtherState.StateAbbreviation;
            UseSiReadCsenetStateTable();

            if (IsExitState("CO0000_CSENET_STATE_NF"))
            {
              var field = GetField(export.OtherState, "stateAbbreviation");

              field.Error = true;
            }
            else if (AsChar(local.CsenetStateTable.CsenetReadyInd) != 'Y')
            {
              ExitState = "SI0000_CLOSED_SND_MAN_NOTC_PF20";
            }
            else
            {
              // --------------------
              // CSEnet functionality
              // --------------------
              // *******************************************************************
              // 03/04/02 T.bobb PR00138552 Moved the CSENet code
              // from the end of the prad to here so that anytime a case is
              // closed, a CSENet transaction will be generated.
              // *******************************************************************
              if (Equal(global.Command, "UPDATE") && (
                IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE") || IsExitState
                ("ACO_NN0000_ALL_OK")) && !
                IsEmpty(export.InterstateRequest.OtherStateCaseClosureReason) &&
                AsChar(export.HiddenInterstateRequest.OtherStateCaseStatus) == 'O'
                )
              {
                local.ScreenIndentification.Command = "IIMC";
                export.DisplayOnly.ClosureReason =
                  export.InterstateRequest.OtherStateCaseClosureReason ?? "";
                UseSiCreateAutoCsenetTrans();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  UseEabRollbackCics();

                  return;
                }
                else
                {
                  export.HiddenInterstateRequest.OtherStateCaseStatus = "C";
                  ExitState = "ECO_LNK_TO_PEPR";

                  return;
                }
              }
            }
          }
        }

        break;
      case "ADD":
        if (AsChar(export.InterstateRequest.OtherStateCaseStatus) == 'O' && AsChar
          (export.InterstateRequest.KsCaseInd) == 'N')
        {
          ExitState = "ACO_NE0000_OPEN_IN_INTER_REQ_AE";

          return;
        }

        if (IsEmpty(export.InterstatePaymentAddress.PayableToName) || IsEmpty
          (export.InterstatePaymentAddress.Street1) && IsEmpty
          (export.InterstatePaymentAddress.Street3) || IsEmpty
          (export.InterstatePaymentAddress.State) && IsEmpty
          (export.InterstatePaymentAddress.Country) || IsEmpty
          (export.InterstatePaymentAddress.ZipCode) && IsEmpty
          (export.InterstatePaymentAddress.PostalCode) || IsEmpty
          (export.InterstatePaymentAddress.City))
        {
          if (IsEmpty(export.InterstatePaymentAddress.PayableToName))
          {
            var field =
              GetField(export.InterstatePaymentAddress, "payableToName");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

            return;
          }

          if (IsEmpty(export.InterstatePaymentAddress.Street1) && IsEmpty
            (export.InterstatePaymentAddress.Street3))
          {
            var field1 = GetField(export.InterstatePaymentAddress, "street1");

            field1.Error = true;

            var field2 = GetField(export.InterstatePaymentAddress, "street3");

            field2.Error = true;

            ExitState = "FN0000_ENTER_ONE_OF_THESE_FIELDS";

            return;
          }

          if (IsEmpty(export.InterstatePaymentAddress.State) && IsEmpty
            (export.InterstatePaymentAddress.Country))
          {
            var field1 = GetField(export.InterstatePaymentAddress, "state");

            field1.Error = true;

            var field2 = GetField(export.InterstatePaymentAddress, "country");

            field2.Error = true;

            ExitState = "FN0000_ENTER_ONE_OF_THESE_FIELDS";

            return;
          }

          if (IsEmpty(export.InterstatePaymentAddress.ZipCode) && IsEmpty
            (export.InterstatePaymentAddress.PostalCode))
          {
            var field1 = GetField(export.InterstatePaymentAddress, "zipCode");

            field1.Error = true;

            var field2 =
              GetField(export.InterstatePaymentAddress, "postalCode");

            field2.Error = true;

            ExitState = "FN0000_ENTER_ONE_OF_THESE_FIELDS";

            return;
          }

          if (IsEmpty(export.InterstatePaymentAddress.City) && IsEmpty
            (import.InterstatePaymentAddress.Province))
          {
            var field1 = GetField(export.InterstatePaymentAddress, "city");

            field1.Error = true;

            var field2 = GetField(export.InterstatePaymentAddress, "province");

            field2.Error = true;

            ExitState = "FN0000_ENTER_ONE_OF_THESE_FIELDS";

            return;
          }

          if (!IsEmpty(export.InterstatePaymentAddress.Street3) && IsEmpty
            (import.InterstatePaymentAddress.Province))
          {
            var field = GetField(export.InterstatePaymentAddress, "province");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

            return;
          }
        }

        if (!IsEmpty(export.DisplayOnly.DuplicateCaseIndicator))
        {
          var field = GetField(export.DisplayOnly, "duplicateCaseIndicator");

          field.Error = true;

          ExitState = "SP0000_INVALID_VALUE_ENTERED";

          return;
        }

        // >>
        // Closure reason code should be blank on an add.
        if (!IsEmpty(export.InterstateRequest.OtherStateCaseClosureReason))
        {
          ExitState = "FN0000_FIELD_MUST_BE_SPACES";

          var field =
            GetField(export.InterstateRequest, "otherStateCaseClosureReason");

          field.Error = true;

          return;
        }

        // -- Give a warning message if the address state/country does not match
        // the IV-D agency location.
        if (IsEmpty(import.AddressMismatch.Flag))
        {
          if (!IsEmpty(export.OtherState.StateAbbreviation))
          {
            if (!Equal(export.OtherState.StateAbbreviation,
              export.InterstateContactAddress.State))
            {
              var field1 = GetField(export.OtherState, "stateAbbreviation");

              field1.Color = "yellow";
              field1.Protected = false;
              field1.Focused = true;

              var field2 = GetField(export.InterstateContactAddress, "state");

              field2.Color = "yellow";
              field2.Protected = false;

              export.AddressMismatch.Flag = "Y";
            }

            if (!Equal(export.OtherState.StateAbbreviation,
              export.InterstatePaymentAddress.State))
            {
              var field1 = GetField(export.OtherState, "stateAbbreviation");

              field1.Color = "yellow";
              field1.Protected = false;
              field1.Focused = true;

              var field2 = GetField(export.InterstatePaymentAddress, "state");

              field2.Color = "yellow";
              field2.Protected = false;

              export.AddressMismatch.Flag = "Y";
            }

            if (!IsEmpty(export.InterstateContactAddress.Country))
            {
              var field = GetField(export.InterstateContactAddress, "country");

              field.Color = "yellow";
              field.Protected = false;

              export.AddressMismatch.Flag = "Y";
            }

            if (!IsEmpty(export.InterstatePaymentAddress.Country))
            {
              var field = GetField(export.InterstatePaymentAddress, "country");

              field.Color = "yellow";
              field.Protected = false;

              export.AddressMismatch.Flag = "Y";
            }
          }
          else if (!IsEmpty(export.InterstateRequest.Country))
          {
            if (!Equal(export.InterstateRequest.Country,
              export.InterstateContactAddress.Country))
            {
              var field1 = GetField(export.InterstateRequest, "country");

              field1.Color = "yellow";
              field1.Protected = false;
              field1.Focused = true;

              var field2 = GetField(export.InterstateContactAddress, "country");

              field2.Color = "yellow";
              field2.Protected = false;

              export.AddressMismatch.Flag = "Y";
            }

            if (!Equal(export.InterstateRequest.Country,
              export.InterstatePaymentAddress.Country))
            {
              var field1 = GetField(export.InterstateRequest, "country");

              field1.Color = "yellow";
              field1.Protected = false;
              field1.Focused = true;

              var field2 = GetField(export.InterstatePaymentAddress, "country");

              field2.Color = "yellow";
              field2.Protected = false;

              export.AddressMismatch.Flag = "Y";
            }

            if (!IsEmpty(export.InterstateContactAddress.State))
            {
              var field = GetField(export.InterstateContactAddress, "state");

              field.Color = "yellow";
              field.Protected = false;

              export.AddressMismatch.Flag = "Y";
            }

            if (!IsEmpty(export.InterstatePaymentAddress.State))
            {
              var field = GetField(export.InterstatePaymentAddress, "state");

              field.Color = "yellow";
              field.Protected = false;

              export.AddressMismatch.Flag = "Y";
            }
          }
          else if (!IsEmpty(export.InterstateRequest.TribalAgency))
          {
            local.TribalAgencyCommon.State =
              Substring(export.Agency.Description, 1, 2);

            if (!Equal(local.TribalAgencyCommon.State,
              export.InterstateContactAddress.State))
            {
              var field1 = GetField(export.InterstateRequest, "tribalAgency");

              field1.Color = "yellow";
              field1.Protected = false;
              field1.Focused = true;

              var field2 = GetField(export.InterstateContactAddress, "state");

              field2.Color = "yellow";
              field2.Protected = false;

              export.AddressMismatch.Flag = "Y";
            }

            if (!Equal(local.TribalAgencyCommon.State,
              export.InterstatePaymentAddress.State))
            {
              var field1 = GetField(export.InterstateRequest, "tribalAgency");

              field1.Color = "yellow";
              field1.Protected = false;
              field1.Focused = true;

              var field2 = GetField(export.InterstatePaymentAddress, "state");

              field2.Color = "yellow";
              field2.Protected = false;

              export.AddressMismatch.Flag = "Y";
            }

            if (!IsEmpty(export.InterstateContactAddress.Country))
            {
              var field = GetField(export.InterstateContactAddress, "country");

              field.Color = "yellow";
              field.Protected = false;

              export.AddressMismatch.Flag = "Y";
            }

            if (!IsEmpty(export.InterstatePaymentAddress.Country))
            {
              var field = GetField(export.InterstatePaymentAddress, "country");

              field.Color = "yellow";
              field.Protected = false;

              export.AddressMismatch.Flag = "Y";
            }
          }

          if (AsChar(export.AddressMismatch.Flag) == 'Y')
          {
            ExitState = "SI0000_IVD_ADDRESS_MISMATCH_ADD";

            return;
          }
        }

        export.Next.DuplicateCaseIndicator =
          export.DisplayOnly.DuplicateCaseIndicator ?? "";
        UseSiIimcCreateIncomingIsCase();

        if (!IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
        {
          UseEabRollbackCics();

          return;
        }

        export.Hduplicate.DuplicateCaseIndicator =
          export.DisplayOnly.DuplicateCaseIndicator ?? "";

        // mjr
        // -----------------------------------------------------
        // 05/14/1999
        // Added creation of document trigger
        // ------------------------------------------------------------------
        ExitState = "ACO_NN0000_ALL_OK";
        ExitState = "ECO_LNK_TO_PEPR";

        return;
      case "PEPR":
        if (IsEmpty(export.DisplayOnly.Number) || !
          Equal(export.DisplayOnly.Number, export.Next.Number))
        {
          var field = GetField(export.Next, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_DISPLAY_FIRST";

          return;
        }

        ExitState = "ECO_LNK_TO_PEPR";

        break;
      case "IREQ":
        if (IsEmpty(export.DisplayOnly.Number) || !
          Equal(export.DisplayOnly.Number, export.Next.Number))
        {
          var field = GetField(export.Next, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_DISPLAY_FIRST";

          return;
        }

        ExitState = "ECO_XFR_TO_IREQ";

        break;
      case "LIST":
        if (!IsEmpty(export.PersonPrompt.SelectChar))
        {
          if (AsChar(export.PersonPrompt.SelectChar) == 'S')
          {
            ++local.Invalid.Count;

            if (IsEmpty(export.Next.Number))
            {
              var field = GetField(export.Next, "number");

              field.Error = true;

              ExitState = "OE0000_CASE_NUMBER_REQD_FOR_COMP";
            }
            else
            {
              ExitState = "ECO_LNK_TO_CASE_COMPOSITION";
            }

            return;
          }
          else
          {
            var field = GetField(export.PersonPrompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_NO_SELECTION_MADE";
          }
        }
        else
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";
        }

        if (!IsEmpty(export.IreqStatePrompt.SelectChar))
        {
          if (AsChar(export.IreqStatePrompt.SelectChar) == 'S')
          {
            ++local.Invalid.Count;
            export.Prompt.CodeName = local.StateCode.CodeName;
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";
          }
          else
          {
            var field = GetField(export.IreqStatePrompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_NO_SELECTION_MADE";
          }
        }

        if (!IsEmpty(export.IreqCountryPrompt.SelectChar))
        {
          if (AsChar(export.IreqCountryPrompt.SelectChar) == 'S')
          {
            ++local.Invalid.Count;
            export.Prompt.CodeName = local.Country.CodeName;
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";
          }
          else
          {
            var field = GetField(export.IreqCountryPrompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_NO_SELECTION_MADE";
          }
        }

        if (!IsEmpty(export.IreqTribalPrompt.SelectChar))
        {
          if (AsChar(export.IreqTribalPrompt.SelectChar) == 'S')
          {
            ++local.Invalid.Count;
            export.Prompt.CodeName = local.TribalAgencyCode.CodeName;
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";
          }
          else
          {
            var field = GetField(export.IreqTribalPrompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_NO_SELECTION_MADE";
          }
        }

        if (!IsEmpty(export.IreqCaseClosurePrompt.SelectChar))
        {
          if (AsChar(export.IreqCaseClosurePrompt.SelectChar) == 'S')
          {
            ++local.Invalid.Count;
            export.Prompt.CodeName = local.CsenetCaseClosure.CodeName;
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";
          }
          else
          {
            var field = GetField(export.IreqCaseClosurePrompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_NO_SELECTION_MADE";
          }
        }

        if (!IsEmpty(export.IreqCaseProgramPrompt.SelectChar))
        {
          if (AsChar(export.IreqCaseProgramPrompt.SelectChar) == 'S')
          {
            ++local.Invalid.Count;
            export.Prompt.CodeName = local.CsenetProgramType.CodeName;
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";
          }
          else
          {
            var field = GetField(export.IreqCaseProgramPrompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_NO_SELECTION_MADE";
          }
        }

        if (!IsEmpty(export.ContactStatePrompt.SelectChar))
        {
          if (AsChar(export.ContactStatePrompt.SelectChar) == 'S')
          {
            ++local.Invalid.Count;
            export.Prompt.CodeName = local.StateCode.CodeName;
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";
          }
          else
          {
            var field = GetField(export.ContactStatePrompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_NO_SELECTION_MADE";
          }
        }

        if (!IsEmpty(export.ContactCountryPrompt.SelectChar))
        {
          if (AsChar(export.ContactCountryPrompt.SelectChar) == 'S')
          {
            ++local.Invalid.Count;
            export.Prompt.CodeName = local.Country.CodeName;
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";
          }
          else
          {
            var field = GetField(export.ContactCountryPrompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_NO_SELECTION_MADE";
          }
        }

        if (!IsEmpty(export.PaymentStatePrompt.SelectChar))
        {
          if (AsChar(export.PaymentStatePrompt.SelectChar) == 'S')
          {
            ++local.Invalid.Count;
            export.Prompt.CodeName = local.StateCode.CodeName;
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";
          }
          else
          {
            var field = GetField(export.PaymentStatePrompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_NO_SELECTION_MADE";
          }
        }

        if (!IsEmpty(export.PaymentCountryPrompt.SelectChar))
        {
          if (AsChar(export.PaymentCountryPrompt.SelectChar) == 'S')
          {
            ++local.Invalid.Count;
            export.Prompt.CodeName = local.Country.CodeName;
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";
          }
          else
          {
            var field = GetField(export.PaymentCountryPrompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_NO_SELECTION_MADE";
          }
        }

        if (!IsEmpty(export.DupCaseIndicatorPrompt.SelectChar))
        {
          if (AsChar(export.DupCaseIndicatorPrompt.SelectChar) == 'S')
          {
            ++local.Invalid.Count;
            export.Prompt.CodeName = "DUPLICATE CASE";
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";
          }
          else
          {
            var field = GetField(export.DupCaseIndicatorPrompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_NO_SELECTION_MADE";
          }
        }

        switch(local.Invalid.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

            break;
          case 1:
            break;
          default:
            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            if (AsChar(export.PersonPrompt.SelectChar) == 'S')
            {
              var field = GetField(export.PersonPrompt, "selectChar");

              field.Error = true;
            }

            if (AsChar(export.IreqStatePrompt.SelectChar) == 'S')
            {
              var field = GetField(export.IreqStatePrompt, "selectChar");

              field.Error = true;
            }

            if (AsChar(export.IreqCountryPrompt.SelectChar) == 'S')
            {
              var field = GetField(export.IreqCountryPrompt, "selectChar");

              field.Error = true;
            }

            if (AsChar(export.IreqTribalPrompt.SelectChar) == 'S')
            {
              var field = GetField(export.IreqTribalPrompt, "selectChar");

              field.Error = true;
            }

            if (AsChar(export.IreqCaseClosurePrompt.SelectChar) == 'S')
            {
              var field = GetField(export.IreqCaseClosurePrompt, "selectChar");

              field.Error = true;
            }

            if (AsChar(export.IreqCaseProgramPrompt.SelectChar) == 'S')
            {
              var field = GetField(export.IreqCaseProgramPrompt, "selectChar");

              field.Error = true;
            }

            if (AsChar(export.ContactStatePrompt.SelectChar) == 'S')
            {
              var field = GetField(export.ContactStatePrompt, "selectChar");

              field.Error = true;
            }

            if (AsChar(export.ContactCountryPrompt.SelectChar) == 'S')
            {
              var field = GetField(export.ContactCountryPrompt, "selectChar");

              field.Error = true;
            }

            if (AsChar(export.PaymentStatePrompt.SelectChar) == 'S')
            {
              var field = GetField(export.PaymentStatePrompt, "selectChar");

              field.Error = true;
            }

            if (AsChar(export.PaymentCountryPrompt.SelectChar) == 'S')
            {
              var field = GetField(export.PaymentCountryPrompt, "selectChar");

              field.Error = true;
            }

            if (AsChar(export.PaymentCountryPrompt.SelectChar) == 'S')
            {
              var field = GetField(export.PaymentCountryPrompt, "selectChar");

              field.Error = true;
            }

            if (AsChar(export.DupCaseIndicatorPrompt.SelectChar) == 'S')
            {
              var field = GetField(export.DupCaseIndicatorPrompt, "selectChar");

              field.Error = true;
            }

            break;
        }

        break;
      case "DISPLAY":
        if (IsEmpty(export.Next.Number))
        {
          var field = GetField(export.Next, "number");

          field.Error = true;

          export.DisplayOnly.Assign(local.RefreshCase);
          export.Hduplicate.DuplicateCaseIndicator =
            local.RefreshCase.DuplicateCaseIndicator;
          MoveCsePersonsWorkSet(local.RefreshCsePersonsWorkSet,
            export.ApCsePersonsWorkSet);
          MoveCsePersonsWorkSet(local.RefreshCsePersonsWorkSet, export.Ar);
          export.InterstateRequest.Assign(local.RefreshInterstateRequest);
          MoveInterstateRequest3(local.RefreshInterstateRequest, export.H);
          export.InterstateContact.Assign(local.RefreshInterstateContact);
          export.InterstateContactAddress.Assign(
            local.RefreshInterstateContactAddress);
          MoveInterstatePaymentAddress1(local.RefreshInterstatePaymentAddress,
            export.InterstatePaymentAddress);
          MoveInterstateRequestHistory(local.RefreshInterstateRequestHistory,
            export.Note);
          export.OtherState.Assign(local.RefreshFips);
          export.HotherState.StateAbbreviation =
            local.RefreshFips.StateAbbreviation;
          MoveOeWorkGroup(local.RefreshOeWorkGroup, export.OtherStateFips);
          export.OtherState.StateAbbreviation = "";
          export.InterstateRequest.Country = "";
          export.Agency.Description = Spaces(CodeValue.Description_MaxLength);
          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          return;
        }
        else
        {
          local.ZeroFill.Text10 = export.Next.Number;
          UseEabPadLeftWithZeros();
          export.Next.Number = local.ZeroFill.Text10;

          // *** 1/25/99  C Deghand  Removed OE CAB CHECK CASE MEMBER and 
          // replaced it with SI READ CASE HEADER INFORMATION.
          local.Ap.FormattedName = "";
          local.Ar.FormattedName = "";
          UseSiReadCaseHeaderInformation();
          local.Ap.FormattedName = export.ApCsePersonsWorkSet.FormattedName;
          local.Ar.FormattedName = export.Ar.FormattedName;

          if (AsChar(local.RetcompInd.Flag) == 'Y' && IsEmpty
            (export.ApCsePersonsWorkSet.FormattedName) && !
            IsEmpty(import.SelectedCsePersonsWorkSet.Number))
          {
            MoveCsePersonsWorkSet(import.SelectedCsePersonsWorkSet,
              export.ApCsePersonsWorkSet);
          }
          else if (AsChar(local.RetcompInd.Flag) == 'Y')
          {
            MoveCsePersonsWorkSet(import.Ap, export.ApCsePersonsWorkSet);
          }
          else if (AsChar(local.RetcompInd.Flag) == 'N' && Equal
            (import.DisplayOnly.Number, export.DisplayOnly.Number) && !
            IsEmpty(import.Ap.Number) && !
            Equal(export.ApCsePersonsWorkSet.Number, import.Ap.Number))
          {
            MoveCsePersonsWorkSet(import.Ap, export.ApCsePersonsWorkSet);
          }

          if (AsChar(local.CaseOpen.Flag) == 'N')
          {
            MoveCase5(export.Next, export.DisplayOnly);
            ExitState = "DISPLAY_OK_FOR_CLOSED_CASE";
          }
        }

        UseSiReadOfficeOspHeader();

        if (IsExitState("DISPLAY_OK_FOR_CLOSED_CASE"))
        {
        }
        else if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // 12/22/98   C Deghand   Added statements to Set exports to SPACES.
        if (!Equal(export.Next.Number, export.DisplayOnly.Number) && !
          IsEmpty(export.DisplayOnly.Number))
        {
          export.ApCsePersonsWorkSet.Number = "";
          export.ApCsePersonsWorkSet.FormattedName = "";
          export.InterstateRequest.Country = "";
          export.InterstateRequest.TribalAgency = "";
          export.OtherState.StateAbbreviation = "";
        }

        // --  Only one of State, Country, or Tribal Agency may be entered.
        local.Common1.Count = 0;

        if (!IsEmpty(export.OtherState.StateAbbreviation))
        {
          ++local.Common1.Count;
        }

        if (!IsEmpty(export.InterstateRequest.Country))
        {
          ++local.Common1.Count;
        }

        if (!IsEmpty(export.InterstateRequest.TribalAgency))
        {
          ++local.Common1.Count;
        }

        if (local.Common1.Count > 1)
        {
          if (!IsEmpty(export.OtherState.StateAbbreviation))
          {
            var field = GetField(export.OtherState, "stateAbbreviation");

            field.Error = true;
          }

          if (!IsEmpty(export.InterstateRequest.Country))
          {
            var field = GetField(export.InterstateRequest, "country");

            field.Error = true;
          }

          if (!IsEmpty(export.InterstateRequest.TribalAgency))
          {
            var field = GetField(export.InterstateRequest, "tribalAgency");

            field.Error = true;
          }

          export.Agency.Description = Spaces(CodeValue.Description_MaxLength);
          ExitState = "SI0000_SELECT_STATE_COUNTRY_TRIB";

          return;
        }

        UseSiIimcReadIncomingIsInfo();
        export.Prev.OtherStateCaseStatus =
          export.InterstateRequest.OtherStateCaseStatus;

        // >>
        // If displaying a closed or open  outgoing interstate request, blank 
        // out the closure reason and protect status.
        if (AsChar(export.InterstateRequest.KsCaseInd) == 'Y' && AsChar
          (export.InterstateRequest.OtherStateCaseStatus) == 'C')
        {
          export.InterstateRequest.OtherStateCaseClosureReason = "";
        }

        if (AsChar(export.InterstateRequest.KsCaseInd) == 'Y' && AsChar
          (export.InterstateRequest.OtherStateCaseStatus) == 'O')
        {
          export.InterstateRequest.OtherStateCaseClosureReason = "";

          var field1 =
            GetField(export.InterstateRequest, "otherStateCaseClosureReason");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 =
            GetField(export.InterstateRequest, "otherStateCaseStatus");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 =
            GetField(export.InterstateRequest, "otherStateCaseClosureDate");

          field3.Color = "cyan";
          field3.Protected = true;
        }

        if (IsEmpty(export.OtherState.StateAbbreviation))
        {
          export.OtherState.State = 0;
          export.DisplayOnly.DuplicateCaseIndicator = "";
        }

        if (!Lt(export.InterstateRequest.OtherStateCaseClosureDate,
          local.Max.Date))
        {
          export.InterstateRequest.OtherStateCaseClosureDate = local.Null1.Date;
        }

        if (!Lt(local.Null1.Date,
          export.InterstateRequest.OtherStateCaseClosureDate))
        {
          export.InterstateRequest.OtherStateCaseClosureDate =
            export.DisplayOnly.CseOpenDate;
        }

        if (AsChar(export.InterstateRequest.OtherStateCaseStatus) == 'C' && AsChar
          (export.InterstateRequest.KsCaseInd) == 'N')
        {
          var field1 =
            GetField(export.InterstateRequest, "otherStateCaseStatus");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 =
            GetField(export.InterstateRequest, "otherStateCaseClosureDate");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 =
            GetField(export.InterstateRequest, "otherStateCaseClosureReason");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.IreqCaseClosurePrompt, "selectChar");

          field4.Color = "cyan";
          field4.Protected = true;
        }
        else if (AsChar(export.InterstateRequest.OtherStateCaseStatus) == 'O'
          && AsChar(export.InterstateRequest.KsCaseInd) == 'N')
        {
          var field1 =
            GetField(export.InterstateRequest, "otherStateCaseStatus");

          field1.Protected = false;

          var field2 =
            GetField(export.InterstateRequest, "otherStateCaseClosureDate");

          field2.Protected = false;

          var field3 =
            GetField(export.InterstateRequest, "otherStateCaseClosureReason");

          field3.Protected = false;

          var field4 = GetField(export.IreqCaseClosurePrompt, "selectChar");

          field4.Protected = false;
        }

        if (IsExitState("SI0000_MULTIPLE_IR_EXISTS_FOR_AP") || IsExitState
          ("SI0000_MULTIPLE_IR_FOR_CLOSED_CA"))
        {
          // *** Problem report I00119237
          // *** 05/14/01 swsrchf
          // *** start
          export.AutoFlow.Flag = "Y";

          // ***
          // *** Flow to IREQ, when multiple Interstate Requests exist for a 
          // case
          // ***
          ExitState = "ECO_XFR_TO_IREQ";

          // *** end
          // *** 05/14/01 swsrchf
          // *** Problem report I00119237
          return;
        }

        if (!IsExitState("ACO_NI0000_SUCCESSFUL_DISPLAY"))
        {
          // *** 12/17/98  C Deghand  Added this IF statement.
          // *** Anita Hockman    cq531  changes made to keep data from 
          // displaying
          // when case is not incoming.    So I set fields to spaces.
          if (IsExitState("CASE_NOT_INTERSTATE"))
          {
            export.InterstateRequest.OtherStateCaseId = "";
            export.InterstateRequest.OtherStateCaseClosureDate =
              local.Zero.Date;
            export.InterstateRequest.OtherStateCaseStatus = "";
            export.InterstateRequest.CaseType = "";
            export.DisplayOnly.CseOpenDate = local.Zero.Date;
            export.DisplayOnly.Status = "";

            goto Test;
          }

          // *** RMathews  CQ23444   Blank out fields for cases that were 
          // previously incoming interstate.
          if (IsExitState("ACO_NE0000_PREV_IN_INTRSTAT_CSE"))
          {
            export.InterstateRequest.OtherStateCaseId = "";
            export.InterstateRequest.OtherStateCaseClosureDate =
              local.Zero.Date;
            export.InterstateRequest.OtherStateCaseStatus = "";
            export.InterstateRequest.CaseType = "";
            export.DisplayOnly.CseOpenDate = local.Zero.Date;
            export.DisplayOnly.Status = "";

            break;
          }

          // *** Anita Hockman    cq531  changes made to keep data from 
          // displaying
          // when case is not incoming.    So I set fields to spaces.
          if (IsExitState("CASE_NOT_IC_INTERSTATE"))
          {
            export.InterstateRequest.OtherStateCaseId = "";
            export.InterstateRequest.OtherStateCaseClosureDate =
              local.Zero.Date;
            export.InterstateRequest.OtherStateCaseStatus = "";
            export.InterstateRequest.CaseType = "";
            export.DisplayOnly.CseOpenDate = local.Zero.Date;
            export.DisplayOnly.Status = "";

            break;
          }

          if (IsExitState("FIPS_NF"))
          {
            var field = GetField(export.OtherState, "stateAbbreviation");

            field.Error = true;
          }

          if (IsEmpty(export.OtherState.StateAbbreviation))
          {
            export.OtherState.Assign(local.RefreshFips);
            export.HotherState.StateAbbreviation =
              local.RefreshFips.StateAbbreviation;
            export.OtherStateFips.CountyFips = "";
            export.OtherStateFips.LocationFips = "";
          }

          // ------------------------------------------------------
          // If there is a country code entered, display the code.
          // ------------------------------------------------------
          MoveInterstateRequest3(export.InterstateRequest, export.H);
          export.InterstateRequest.Assign(local.RefreshInterstateRequest);

          if (!IsEmpty(export.H.Country) || !IsEmpty(export.H.TribalAgency))
          {
            export.InterstateRequest.Country = export.H.Country ?? "";
            export.InterstateRequest.TribalAgency = export.H.TribalAgency ?? "";
          }
          else
          {
          }

          MoveInterstateRequest3(local.RefreshInterstateRequest, export.H);
          export.InterstateContact.Assign(local.RefreshInterstateContact);
          export.InterstateContactAddress.Assign(
            local.RefreshInterstateContactAddress);
          MoveInterstatePaymentAddress1(local.RefreshInterstatePaymentAddress,
            export.InterstatePaymentAddress);
          MoveInterstateWorkArea(local.RefreshPhone, export.ContactPhone);
        }
        else if (IsEmpty(export.InterstateRequest.Country))
        {
          export.OtherStateFips.CountyFips =
            NumberToString(export.OtherState.County, 3);
          export.OtherStateFips.LocationFips =
            NumberToString(export.OtherState.Location, 2);
          export.HotherState.StateAbbreviation =
            export.OtherState.StateAbbreviation;
          MoveInterstateRequest3(export.InterstateRequest, export.H);
          MoveInterstateRequest4(export.InterstateRequest,
            export.HiddenInterstateRequest);

          if (IsEmpty(export.OtherState.StateAbbreviation))
          {
            export.OtherState.Assign(local.RefreshFips);
            export.HotherState.StateAbbreviation =
              local.RefreshFips.StateAbbreviation;
            export.OtherStateFips.CountyFips = "";
            export.OtherStateFips.LocationFips = "";
          }

          if (IsEmpty(export.InterstateRequest.Country))
          {
          }
        }
        else
        {
          export.OtherState.StateAbbreviation = "";

          if (IsEmpty(export.OtherState.StateAbbreviation))
          {
            export.OtherState.Assign(local.RefreshFips);
            export.HotherState.StateAbbreviation =
              local.RefreshFips.StateAbbreviation;
            export.OtherStateFips.CountyFips = "";
            export.OtherStateFips.LocationFips = "";
          }

          export.HotherState.StateAbbreviation = "";
          MoveInterstateRequest3(export.InterstateRequest, export.H);
          MoveInterstateRequest4(export.InterstateRequest,
            export.HiddenInterstateRequest);

          if (IsEmpty(export.InterstateRequest.Country))
          {
          }
        }

Test:

        export.Hduplicate.DuplicateCaseIndicator =
          export.DisplayOnly.DuplicateCaseIndicator ?? "";

        if (AsChar(local.CaseOpen.Flag) == 'N')
        {
          export.ApCsePersonsWorkSet.FormattedName = local.Ap.FormattedName;
          export.Ar.FormattedName = local.Ar.FormattedName;

          switch(local.InterstateRequestCount.Count)
          {
            case 0:
              ExitState = "DISPLAY_OK_FOR_CLOSED_CASE";

              break;
            case 1:
              ExitState = "DISPLAY_OK_FOR_CLOSED_CASE";

              break;
            default:
              ExitState = "SI0000_DISP_OK_CLOSED_INTERSTATE";

              break;
          }
        }

        if (AsChar(local.RetFromCdvl.Flag) == 'Y')
        {
          export.InterstateRequest.Assign(import.InterstateRequest);

          if (AsChar(export.IreqCountryPrompt.SelectChar) == 'S')
          {
            export.InterstateRequest.Country = import.SelectedCodeValue.Cdvalue;
            export.Agency.Description = import.SelectedCodeValue.Description;
            export.IreqCountryPrompt.SelectChar = "";

            var field = GetField(export.InterstateRequest, "country");

            field.Protected = false;
            field.Focused = true;
          }

          ExitState = "ACO_NN0000_ALL_OK";

          return;
        }

        break;
      case "REOPEN":
        UseSiIimcReopenIncomingIsCase();

        if (IsExitState("SI0000_INT_REQ_REOPEN_FAILED"))
        {
          var field =
            GetField(export.InterstateRequest, "otherStateCaseStatus");

          field.Error = true;

          return;
        }

        // tbb
        if (IsExitState("ACO_NE0000_OPEN_OUT_INTER_REQ_AE"))
        {
          export.InterstateRequest.OtherStateCaseStatus = "";
          export.InterstateRequest.OtherStateCaseId = "";
          export.InterstateRequest.CaseType = "";
          export.InterstateRequest.OtherStateCaseClosureReason = "";
          export.InterstateRequest.OtherStateCaseClosureDate = null;

          return;
        }

        if (!IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE"))
        {
          return;
        }

        ExitState = "ECO_LNK_TO_PEPR";

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

    if (Equal(export.InterstateRequest.OtherStateCaseClosureDate, local.Max.Date))
      
    {
      export.InterstateRequest.OtherStateCaseClosureDate = local.Zero.Date;
    }
  }

  private static void MoveCase2(Case1 source, Case1 target)
  {
    target.Number = source.Number;
    target.ClosureReason = source.ClosureReason;
  }

  private static void MoveCase3(Case1 source, Case1 target)
  {
    target.Number = source.Number;
    target.ClosureReason = source.ClosureReason;
    target.Status = source.Status;
  }

  private static void MoveCase4(Case1 source, Case1 target)
  {
    target.Number = source.Number;
    target.Status = source.Status;
    target.StatusDate = source.StatusDate;
    target.CseOpenDate = source.CseOpenDate;
    target.DuplicateCaseIndicator = source.DuplicateCaseIndicator;
  }

  private static void MoveCase5(Case1 source, Case1 target)
  {
    target.Number = source.Number;
    target.Status = source.Status;
    target.CseOpenDate = source.CseOpenDate;
    target.DuplicateCaseIndicator = source.DuplicateCaseIndicator;
  }

  private static void MoveCase6(Case1 source, Case1 target)
  {
    target.Number = source.Number;
    target.CseOpenDate = source.CseOpenDate;
  }

  private static void MoveCase7(Case1 source, Case1 target)
  {
    target.Number = source.Number;
    target.DuplicateCaseIndicator = source.DuplicateCaseIndicator;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveCsenetStateTable(CsenetStateTable source,
    CsenetStateTable target)
  {
    target.StateCode = source.StateCode;
    target.CsenetReadyInd = source.CsenetReadyInd;
  }

  private static void MoveInterstateCase1(InterstateCase source,
    InterstateCase target)
  {
    target.TransSerialNumber = source.TransSerialNumber;
    target.TransactionDate = source.TransactionDate;
  }

  private static void MoveInterstateCase2(InterstateCase source,
    InterstateCase target)
  {
    target.SendPaymentsBankAccount = source.SendPaymentsBankAccount;
    target.SendPaymentsRoutingCode = source.SendPaymentsRoutingCode;
  }

  private static void MoveInterstateContact(InterstateContact source,
    InterstateContact target)
  {
    target.ContactPhoneNum = source.ContactPhoneNum;
    target.StartDate = source.StartDate;
    target.EndDate = source.EndDate;
    target.NameLast = source.NameLast;
    target.NameFirst = source.NameFirst;
    target.NameMiddle = source.NameMiddle;
    target.AreaCode = source.AreaCode;
    target.ContactPhoneExtension = source.ContactPhoneExtension;
    target.ContactFaxNumber = source.ContactFaxNumber;
    target.ContactFaxAreaCode = source.ContactFaxAreaCode;
    target.ContactInternetAddress = source.ContactInternetAddress;
  }

  private static void MoveInterstateContactAddress(
    InterstateContactAddress source, InterstateContactAddress target)
  {
    target.LocationType = source.LocationType;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.Type1 = source.Type1;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.StartDate = source.StartDate;
    target.EndDate = source.EndDate;
    target.Street3 = source.Street3;
    target.Street4 = source.Street4;
    target.Province = source.Province;
    target.PostalCode = source.PostalCode;
    target.Country = source.Country;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
    target.Zip3 = source.Zip3;
  }

  private static void MoveInterstatePaymentAddress1(
    InterstatePaymentAddress source, InterstatePaymentAddress target)
  {
    target.LocationType = source.LocationType;
    target.PayableToName = source.PayableToName;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.Type1 = source.Type1;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.Zip5 = source.Zip5;
    target.AddressStartDate = source.AddressStartDate;
    target.AddressEndDate = source.AddressEndDate;
    target.FipsCounty = source.FipsCounty;
    target.FipsState = source.FipsState;
    target.FipsLocation = source.FipsLocation;
    target.Street3 = source.Street3;
    target.Street4 = source.Street4;
    target.Province = source.Province;
    target.PostalCode = source.PostalCode;
    target.Country = source.Country;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
    target.Zip3 = source.Zip3;
    target.County = source.County;
  }

  private static void MoveInterstatePaymentAddress2(
    InterstatePaymentAddress source, InterstatePaymentAddress target)
  {
    target.LocationType = source.LocationType;
    target.PayableToName = source.PayableToName;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.Type1 = source.Type1;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.Zip5 = source.Zip5;
    target.AddressStartDate = source.AddressStartDate;
    target.AddressEndDate = source.AddressEndDate;
    target.FipsCounty = source.FipsCounty;
    target.FipsState = source.FipsState;
    target.FipsLocation = source.FipsLocation;
    target.RoutingNumberAba = source.RoutingNumberAba;
    target.AccountNumberDfi = source.AccountNumberDfi;
    target.Street3 = source.Street3;
    target.Street4 = source.Street4;
    target.Province = source.Province;
    target.PostalCode = source.PostalCode;
    target.Country = source.Country;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
    target.Zip3 = source.Zip3;
    target.County = source.County;
  }

  private static void MoveInterstatePaymentAddress3(
    InterstatePaymentAddress source, InterstatePaymentAddress target)
  {
    target.LocationType = source.LocationType;
    target.PayableToName = source.PayableToName;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.Type1 = source.Type1;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.AddressStartDate = source.AddressStartDate;
    target.AddressEndDate = source.AddressEndDate;
    target.FipsCounty = source.FipsCounty;
    target.FipsState = source.FipsState;
    target.FipsLocation = source.FipsLocation;
    target.RoutingNumberAba = source.RoutingNumberAba;
    target.AccountNumberDfi = source.AccountNumberDfi;
    target.Street3 = source.Street3;
    target.Street4 = source.Street4;
    target.Province = source.Province;
    target.PostalCode = source.PostalCode;
    target.Country = source.Country;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
    target.Zip3 = source.Zip3;
  }

  private static void MoveInterstateRequest1(InterstateRequest source,
    InterstateRequest target)
  {
    target.IntHGeneratedId = source.IntHGeneratedId;
    target.OtherStateFips = source.OtherStateFips;
    target.OtherStateCaseId = source.OtherStateCaseId;
    target.CaseType = source.CaseType;
    target.Country = source.Country;
    target.TribalAgency = source.TribalAgency;
  }

  private static void MoveInterstateRequest2(InterstateRequest source,
    InterstateRequest target)
  {
    target.OtherStateFips = source.OtherStateFips;
    target.OtherStateCaseId = source.OtherStateCaseId;
    target.Country = source.Country;
    target.TribalAgency = source.TribalAgency;
  }

  private static void MoveInterstateRequest3(InterstateRequest source,
    InterstateRequest target)
  {
    target.OtherStateCaseStatus = source.OtherStateCaseStatus;
    target.CaseType = source.CaseType;
    target.Country = source.Country;
    target.TribalAgency = source.TribalAgency;
  }

  private static void MoveInterstateRequest4(InterstateRequest source,
    InterstateRequest target)
  {
    target.OtherStateCaseStatus = source.OtherStateCaseStatus;
    target.OtherStateCaseClosureReason = source.OtherStateCaseClosureReason;
  }

  private static void MoveInterstateRequestHistory(
    InterstateRequestHistory source, InterstateRequestHistory target)
  {
    target.TransactionDate = source.TransactionDate;
    target.ActionReasonCode = source.ActionReasonCode;
    target.Note = source.Note;
  }

  private static void MoveInterstateWorkArea(InterstateWorkArea source,
    InterstateWorkArea target)
  {
    target.PhoneNumber = source.PhoneNumber;
    target.PhoneAreaCode = source.PhoneAreaCode;
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

  private static void MoveOeWorkGroup(OeWorkGroup source, OeWorkGroup target)
  {
    target.LocationFips = source.LocationFips;
    target.CountyFips = source.CountyFips;
  }

  private static void MoveOffice(Office source, Office target)
  {
    target.TypeCode = source.TypeCode;
    target.Name = source.Name;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseCabValidateCodeValue1()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.TribalAgencyCode.CodeName;
    useImport.CodeValue.Cdvalue = local.Validation.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.Error.Flag = useExport.ValidCode.Flag;
    export.Agency.Description = useExport.CodeValue.Description;
  }

  private void UseCabValidateCodeValue2()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.CsenetProgramType.CodeName;
    useImport.CodeValue.Cdvalue = local.Validation.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.Error.Flag = useExport.ValidCode.Flag;
  }

  private void UseCabValidateCodeValue3()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.CsenetCaseClosure.CodeName;
    useImport.CodeValue.Cdvalue = local.Validation.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.Error.Flag = useExport.ValidCode.Flag;
  }

  private void UseCabValidateCodeValue4()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Country.CodeName;
    useImport.CodeValue.Cdvalue = local.Validation.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.Error.Flag = useExport.ValidCode.Flag;
  }

  private void UseCabValidateCodeValue5()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Country.CodeName;
    useImport.CodeValue.Cdvalue = local.Validation.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.Error.Flag = useExport.ValidCode.Flag;
    export.Agency.Description = useExport.CodeValue.Description;
  }

  private void UseCabValidateCodeValue6()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.StateCode.CodeName;
    useImport.CodeValue.Cdvalue = local.Validation.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.Error.Flag = useExport.ValidCode.Flag;
  }

  private void UseCabValidateCodeValue7()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.StateCode.CodeName;
    useImport.CodeValue.Cdvalue = local.Validation.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.Error.Flag = useExport.ValidCode.Flag;
    export.Agency.Description = useExport.CodeValue.Description;
  }

  private void UseCabValidateCodeValue8()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.Common2.Flag = useExport.ValidCode.Flag;
  }

  private void UseEabPadLeftWithZeros()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.ZeroFill.Text10;
    useExport.TextWorkArea.Text10 = local.ZeroFill.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.ZeroFill.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseOeCabSetMnemonics()
  {
    var useImport = new OeCabSetMnemonics.Import();
    var useExport = new OeCabSetMnemonics.Export();

    Call(OeCabSetMnemonics.Execute, useImport, useExport);

    local.TribalAgencyCode.CodeName = useExport.TribalAgency.CodeName;
    local.CsenetProgramType.CodeName = useExport.CsenetProgramType.CodeName;
    local.CsenetCaseClosure.CodeName = useExport.CsenetCaseClosure.CodeName;
    local.Country.CodeName = useExport.Country.CodeName;
    local.StateCode.CodeName = useExport.State.CodeName;
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

    useImport.Case1.Number = export.Next.Number;
    useImport.CsePersonsWorkSet.Number = export.ApCsePersonsWorkSet.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiCreateAutoCsenetTrans()
  {
    var useImport = new SiCreateAutoCsenetTrans.Import();
    var useExport = new SiCreateAutoCsenetTrans.Export();

    useImport.Specific.ActionReasonCode = local.Specific.ActionReasonCode;
    useImport.ScreenIdentification.Command =
      local.ScreenIndentification.Command;
    MoveCase3(export.DisplayOnly, useImport.Case1);
    useImport.InterstateRequest.IntHGeneratedId =
      export.InterstateRequest.IntHGeneratedId;

    Call(SiCreateAutoCsenetTrans.Execute, useImport, useExport);
  }

  private void UseSiIimcCreateIncomingIsCase()
  {
    var useImport = new SiIimcCreateIncomingIsCase.Import();
    var useExport = new SiIimcCreateIncomingIsCase.Export();

    MoveCase7(export.Next, useImport.Case1);
    useImport.OtherState.StateAbbreviation =
      export.OtherState.StateAbbreviation;
    useImport.InterstateRequest.Assign(export.InterstateRequest);
    MoveInterstatePaymentAddress3(export.InterstatePaymentAddress,
      useImport.InterstatePaymentAddress);
    useImport.InterstateContactAddress.Assign(export.InterstateContactAddress);
    useImport.Ap.Number = export.ApCsePerson.Number;
    MoveInterstateCase2(export.InterstateCase, useImport.InterstateCase);
    useImport.InterstateRequestHistory.Note = export.Note.Note;
    MoveInterstateContact(export.InterstateContact, useImport.InterstateContact);
      

    Call(SiIimcCreateIncomingIsCase.Execute, useImport, useExport);

    export.Case1.Number = useExport.Case1.Number;
    export.IreqUpdatedDate.Date = useExport.IreqUpdated.Date;
    export.IreqCreatedDate.Date = useExport.IreqCreated.Date;
    export.InterstateContact.Assign(useExport.InterstateContact);
    export.InterstateRequest.Assign(useExport.InterstateRequest);
    MoveInterstatePaymentAddress3(useExport.InterstatePaymentAddress,
      export.InterstatePaymentAddress);
    MoveInterstateContactAddress(useExport.InterstateContactAddress,
      export.InterstateContactAddress);
    MoveInterstateCase2(useExport.InterstateCase, export.InterstateCase);
    MoveInterstateRequestHistory(useExport.InterstateRequestHistory, export.Note);
      
  }

  private void UseSiIimcReadIncomingIsInfo()
  {
    var useImport = new SiIimcReadIncomingIsInfo.Import();
    var useExport = new SiIimcReadIncomingIsInfo.Export();

    useImport.RetcompInd.Flag = local.RetcompInd.Flag;
    MoveCsePersonsWorkSet(export.Ar, useImport.Ar);
    MoveInterstateRequest1(export.InterstateRequest, useImport.InterstateRequest);
      
    useImport.OtherState.Assign(export.OtherState);
    MoveCase6(export.Next, useImport.Case1);
    MoveCsePersonsWorkSet(export.ApCsePersonsWorkSet, useImport.Ap);

    Call(SiIimcReadIncomingIsInfo.Execute, useImport, useExport);

    local.InterstateRequestCount.Count = useExport.InterstateRequestCount.Count;
    export.Agency.Description = useExport.Agency.Description;
    export.IreqUpdatedDate.Date = useExport.IreqUpdated.Date;
    export.IreqCreatedDate.Date = useExport.IreqCreated.Date;
    export.Note.Assign(useExport.Note);
    MoveInterstateWorkArea(useExport.ContactPhone, export.ContactPhone);
    MoveCase4(useExport.Case1, export.DisplayOnly);
    export.OtherState.Assign(useExport.OtherState);
    export.InterstateContact.Assign(useExport.InterstateContact);
    export.InterstateRequest.Assign(useExport.InterstateRequest);
    MoveCsePersonsWorkSet(useExport.Ar, export.Ar);
    MoveCsePersonsWorkSet(useExport.Ap, export.ApCsePersonsWorkSet);
    export.InterstatePaymentAddress.Assign(useExport.InterstatePaymentAddress);
    export.InterstateContactAddress.Assign(useExport.InterstateContactAddress);
    export.InterstateCase.Assign(useExport.InterstateCase);
  }

  private void UseSiIimcReopenIncomingIsCase()
  {
    var useImport = new SiIimcReopenIncomingIsCase.Import();
    var useExport = new SiIimcReopenIncomingIsCase.Export();

    MoveInterstateCase1(export.InterstateCase, useImport.InterstateCase);
    useImport.Ap.Number = export.ApCsePerson.Number;
    useImport.InterstateRequest.Assign(export.InterstateRequest);
    useImport.Case1.Number = export.Next.Number;
    useImport.OtherState.Assign(export.OtherState);

    Call(SiIimcReopenIncomingIsCase.Execute, useImport, useExport);

    export.InterstateRequest.Assign(useExport.InterstateRequest);
    export.ApCsePerson.Number = useExport.Ap.Number;
    MoveCase2(useExport.Case1, export.Case1);
  }

  private void UseSiIimcUpdateIncomingIsCase()
  {
    var useImport = new SiIimcUpdateIncomingIsCase.Import();
    var useExport = new SiIimcUpdateIncomingIsCase.Export();

    useImport.CaseMarkedDuplicate.Flag = local.CaseMarkedDuplicate.Flag;
    useImport.CaseClosed.Flag = local.CaseClosed.Flag;
    useImport.ChangeProgram.Flag = local.ChangeProgram.Flag;
    useImport.OtherState.Assign(export.OtherState);
    MoveInterstatePaymentAddress2(export.InterstatePaymentAddress,
      useImport.InterstatePaymentAddress);
    useImport.InterstateContactAddress.Assign(export.InterstateContactAddress);
    MoveInterstateContact(export.InterstateContact, useImport.InterstateContact);
      
    useImport.InterstateRequestHistory.Note = export.Note.Note;
    useImport.Ap.Number = export.ApCsePerson.Number;
    MoveCase7(export.DisplayOnly, useImport.Case1);
    useImport.InterstateRequest.Assign(export.InterstateRequest);
    MoveInterstateCase2(export.InterstateCase, useImport.InterstateCase);

    Call(SiIimcUpdateIncomingIsCase.Execute, useImport, useExport);

    MoveCase5(useExport.Case1, export.DisplayOnly);
    export.InterstateRequest.Assign(useExport.InterstateRequest);
    export.IreqUpdatedDate.Date = useExport.IreqUpdated.Date;
    export.InterstateCase.Assign(useExport.InterstateCase);
    local.CaseStatusChangedInd.Flag = useExport.CaseStatusChanged.Flag;
    export.IreqCreatedDate.Date = useExport.IreqCreated.Date;
    MoveInterstateRequestHistory(useExport.InterstateRequestHistory, export.Note);
      
    export.InterstateContact.Assign(useExport.InterstateContact);
    export.InterstatePaymentAddress.Assign(useExport.InterstatePaymentAddress);
    export.InterstateContactAddress.Assign(useExport.InterstateContactAddress);
  }

  private void UseSiReadCaseHeaderInformation()
  {
    var useImport = new SiReadCaseHeaderInformation.Import();
    var useExport = new SiReadCaseHeaderInformation.Export();

    useImport.Case1.Number = export.Next.Number;

    Call(SiReadCaseHeaderInformation.Execute, useImport, useExport);

    local.MultipleAps.Flag = useExport.MultipleAps.Flag;
    local.CaseOpen.Flag = useExport.CaseOpen.Flag;
    MoveCsePersonsWorkSet(useExport.Ar, export.Ar);
    MoveCsePersonsWorkSet(useExport.Ap, export.ApCsePersonsWorkSet);
  }

  private void UseSiReadCsenetStateTable()
  {
    var useImport = new SiReadCsenetStateTable.Import();
    var useExport = new SiReadCsenetStateTable.Export();

    useImport.CsenetStateTable.StateCode = local.CsenetStateTable.StateCode;

    Call(SiReadCsenetStateTable.Execute, useImport, useExport);

    MoveCsenetStateTable(useExport.CsenetStateTable, local.CsenetStateTable);
  }

  private void UseSiReadOfficeOspHeader()
  {
    var useImport = new SiReadOfficeOspHeader.Import();
    var useExport = new SiReadOfficeOspHeader.Export();

    useImport.Case1.Number = export.Next.Number;

    Call(SiReadOfficeOspHeader.Execute, useImport, useExport);

    export.HeaderLine.Text35 = useExport.HeaderLine.Text35;
    export.OspServiceProvider.LastName = useExport.ServiceProvider.LastName;
    MoveOffice(useExport.Office, export.OspOffice);
  }

  private void UseSiValidateStateFips()
  {
    var useImport = new SiValidateStateFips.Import();
    var useExport = new SiValidateStateFips.Export();

    useImport.Common.State = local.StateCommon.State;

    Call(SiValidateStateFips.Execute, useImport, useExport);

    local.FipsError.Flag = useExport.Error.Flag;
    export.OtherState.Assign(useExport.Fips);
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
    /// A value of AddressMismatch.
    /// </summary>
    [JsonPropertyName("addressMismatch")]
    public Common AddressMismatch
    {
      get => addressMismatch ??= new();
      set => addressMismatch = value;
    }

    /// <summary>
    /// A value of IreqTribalPrompt.
    /// </summary>
    [JsonPropertyName("ireqTribalPrompt")]
    public Common IreqTribalPrompt
    {
      get => ireqTribalPrompt ??= new();
      set => ireqTribalPrompt = value;
    }

    /// <summary>
    /// A value of SelectedFips.
    /// </summary>
    [JsonPropertyName("selectedFips")]
    public Fips SelectedFips
    {
      get => selectedFips ??= new();
      set => selectedFips = value;
    }

    /// <summary>
    /// A value of SelectedInterstateRequest.
    /// </summary>
    [JsonPropertyName("selectedInterstateRequest")]
    public InterstateRequest SelectedInterstateRequest
    {
      get => selectedInterstateRequest ??= new();
      set => selectedInterstateRequest = value;
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
    /// A value of Hduplicate.
    /// </summary>
    [JsonPropertyName("hduplicate")]
    public Case1 Hduplicate
    {
      get => hduplicate ??= new();
      set => hduplicate = value;
    }

    /// <summary>
    /// A value of Agency.
    /// </summary>
    [JsonPropertyName("agency")]
    public CodeValue Agency
    {
      get => agency ??= new();
      set => agency = value;
    }

    /// <summary>
    /// A value of H.
    /// </summary>
    [JsonPropertyName("h")]
    public InterstateRequest H
    {
      get => h ??= new();
      set => h = value;
    }

    /// <summary>
    /// A value of HotherState.
    /// </summary>
    [JsonPropertyName("hotherState")]
    public Fips HotherState
    {
      get => hotherState ??= new();
      set => hotherState = value;
    }

    /// <summary>
    /// A value of OtherStateFips.
    /// </summary>
    [JsonPropertyName("otherStateFips")]
    public OeWorkGroup OtherStateFips
    {
      get => otherStateFips ??= new();
      set => otherStateFips = value;
    }

    /// <summary>
    /// A value of SelectedCodeValue.
    /// </summary>
    [JsonPropertyName("selectedCodeValue")]
    public CodeValue SelectedCodeValue
    {
      get => selectedCodeValue ??= new();
      set => selectedCodeValue = value;
    }

    /// <summary>
    /// A value of PaymentCountryPrompt.
    /// </summary>
    [JsonPropertyName("paymentCountryPrompt")]
    public Common PaymentCountryPrompt
    {
      get => paymentCountryPrompt ??= new();
      set => paymentCountryPrompt = value;
    }

    /// <summary>
    /// A value of ContactCountryPrompt.
    /// </summary>
    [JsonPropertyName("contactCountryPrompt")]
    public Common ContactCountryPrompt
    {
      get => contactCountryPrompt ??= new();
      set => contactCountryPrompt = value;
    }

    /// <summary>
    /// A value of IreqUpdatedDate.
    /// </summary>
    [JsonPropertyName("ireqUpdatedDate")]
    public DateWorkArea IreqUpdatedDate
    {
      get => ireqUpdatedDate ??= new();
      set => ireqUpdatedDate = value;
    }

    /// <summary>
    /// A value of IreqCreatedDate.
    /// </summary>
    [JsonPropertyName("ireqCreatedDate")]
    public DateWorkArea IreqCreatedDate
    {
      get => ireqCreatedDate ??= new();
      set => ireqCreatedDate = value;
    }

    /// <summary>
    /// A value of ContactPhone.
    /// </summary>
    [JsonPropertyName("contactPhone")]
    public InterstateWorkArea ContactPhone
    {
      get => contactPhone ??= new();
      set => contactPhone = value;
    }

    /// <summary>
    /// A value of IreqCaseProgramPrompt.
    /// </summary>
    [JsonPropertyName("ireqCaseProgramPrompt")]
    public Common IreqCaseProgramPrompt
    {
      get => ireqCaseProgramPrompt ??= new();
      set => ireqCaseProgramPrompt = value;
    }

    /// <summary>
    /// A value of IreqCaseClosurePrompt.
    /// </summary>
    [JsonPropertyName("ireqCaseClosurePrompt")]
    public Common IreqCaseClosurePrompt
    {
      get => ireqCaseClosurePrompt ??= new();
      set => ireqCaseClosurePrompt = value;
    }

    /// <summary>
    /// A value of PaymentStatePrompt.
    /// </summary>
    [JsonPropertyName("paymentStatePrompt")]
    public Common PaymentStatePrompt
    {
      get => paymentStatePrompt ??= new();
      set => paymentStatePrompt = value;
    }

    /// <summary>
    /// A value of ContactStatePrompt.
    /// </summary>
    [JsonPropertyName("contactStatePrompt")]
    public Common ContactStatePrompt
    {
      get => contactStatePrompt ??= new();
      set => contactStatePrompt = value;
    }

    /// <summary>
    /// A value of IreqCountryPrompt.
    /// </summary>
    [JsonPropertyName("ireqCountryPrompt")]
    public Common IreqCountryPrompt
    {
      get => ireqCountryPrompt ??= new();
      set => ireqCountryPrompt = value;
    }

    /// <summary>
    /// A value of IreqStatePrompt.
    /// </summary>
    [JsonPropertyName("ireqStatePrompt")]
    public Common IreqStatePrompt
    {
      get => ireqStatePrompt ??= new();
      set => ireqStatePrompt = value;
    }

    /// <summary>
    /// A value of Note.
    /// </summary>
    [JsonPropertyName("note")]
    public InterstateRequestHistory Note
    {
      get => note ??= new();
      set => note = value;
    }

    /// <summary>
    /// A value of OtherState.
    /// </summary>
    [JsonPropertyName("otherState")]
    public Fips OtherState
    {
      get => otherState ??= new();
      set => otherState = value;
    }

    /// <summary>
    /// A value of DisplayOnly.
    /// </summary>
    [JsonPropertyName("displayOnly")]
    public Case1 DisplayOnly
    {
      get => displayOnly ??= new();
      set => displayOnly = value;
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
    /// A value of InterstateContact.
    /// </summary>
    [JsonPropertyName("interstateContact")]
    public InterstateContact InterstateContact
    {
      get => interstateContact ??= new();
      set => interstateContact = value;
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
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePersonsWorkSet Ar
    {
      get => ar ??= new();
      set => ar = value;
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
    /// A value of InterstatePaymentAddress.
    /// </summary>
    [JsonPropertyName("interstatePaymentAddress")]
    public InterstatePaymentAddress InterstatePaymentAddress
    {
      get => interstatePaymentAddress ??= new();
      set => interstatePaymentAddress = value;
    }

    /// <summary>
    /// A value of InterstateContactAddress.
    /// </summary>
    [JsonPropertyName("interstateContactAddress")]
    public InterstateContactAddress InterstateContactAddress
    {
      get => interstateContactAddress ??= new();
      set => interstateContactAddress = value;
    }

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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    /// <summary>
    /// A value of OspServiceProvider.
    /// </summary>
    [JsonPropertyName("ospServiceProvider")]
    public ServiceProvider OspServiceProvider
    {
      get => ospServiceProvider ??= new();
      set => ospServiceProvider = value;
    }

    /// <summary>
    /// A value of OspOffice.
    /// </summary>
    [JsonPropertyName("ospOffice")]
    public Office OspOffice
    {
      get => ospOffice ??= new();
      set => ospOffice = value;
    }

    /// <summary>
    /// A value of HiddenInterstateRequest.
    /// </summary>
    [JsonPropertyName("hiddenInterstateRequest")]
    public InterstateRequest HiddenInterstateRequest
    {
      get => hiddenInterstateRequest ??= new();
      set => hiddenInterstateRequest = value;
    }

    /// <summary>
    /// A value of DupCaseIndicatorPrompt.
    /// </summary>
    [JsonPropertyName("dupCaseIndicatorPrompt")]
    public Common DupCaseIndicatorPrompt
    {
      get => dupCaseIndicatorPrompt ??= new();
      set => dupCaseIndicatorPrompt = value;
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
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public InterstateRequest Prev
    {
      get => prev ??= new();
      set => prev = value;
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

    private Common addressMismatch;
    private Common ireqTribalPrompt;
    private Fips selectedFips;
    private InterstateRequest selectedInterstateRequest;
    private Case1 case1;
    private Case1 hduplicate;
    private CodeValue agency;
    private InterstateRequest h;
    private Fips hotherState;
    private OeWorkGroup otherStateFips;
    private CodeValue selectedCodeValue;
    private Common paymentCountryPrompt;
    private Common contactCountryPrompt;
    private DateWorkArea ireqUpdatedDate;
    private DateWorkArea ireqCreatedDate;
    private InterstateWorkArea contactPhone;
    private Common ireqCaseProgramPrompt;
    private Common ireqCaseClosurePrompt;
    private Common paymentStatePrompt;
    private Common contactStatePrompt;
    private Common ireqCountryPrompt;
    private Common ireqStatePrompt;
    private InterstateRequestHistory note;
    private Fips otherState;
    private Case1 displayOnly;
    private Case1 next;
    private InterstateContact interstateContact;
    private InterstateRequest interstateRequest;
    private CsePersonsWorkSet ar;
    private CsePersonsWorkSet ap;
    private InterstatePaymentAddress interstatePaymentAddress;
    private InterstateContactAddress interstateContactAddress;
    private Common personPrompt;
    private Standard standard;
    private CsePersonsWorkSet selectedCsePersonsWorkSet;
    private NextTranInfo hiddenNextTranInfo;
    private ServiceProvider ospServiceProvider;
    private Office ospOffice;
    private InterstateRequest hiddenInterstateRequest;
    private Common dupCaseIndicatorPrompt;
    private InterstateCase interstateCase;
    private InterstateRequest prev;
    private WorkArea headerLine;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of AddressMismatch.
    /// </summary>
    [JsonPropertyName("addressMismatch")]
    public Common AddressMismatch
    {
      get => addressMismatch ??= new();
      set => addressMismatch = value;
    }

    /// <summary>
    /// A value of IreqTribalPrompt.
    /// </summary>
    [JsonPropertyName("ireqTribalPrompt")]
    public Common IreqTribalPrompt
    {
      get => ireqTribalPrompt ??= new();
      set => ireqTribalPrompt = value;
    }

    /// <summary>
    /// A value of SelectedFips.
    /// </summary>
    [JsonPropertyName("selectedFips")]
    public Fips SelectedFips
    {
      get => selectedFips ??= new();
      set => selectedFips = value;
    }

    /// <summary>
    /// A value of SelectedInterstateRequest.
    /// </summary>
    [JsonPropertyName("selectedInterstateRequest")]
    public InterstateRequest SelectedInterstateRequest
    {
      get => selectedInterstateRequest ??= new();
      set => selectedInterstateRequest = value;
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
    /// A value of Hduplicate.
    /// </summary>
    [JsonPropertyName("hduplicate")]
    public Case1 Hduplicate
    {
      get => hduplicate ??= new();
      set => hduplicate = value;
    }

    /// <summary>
    /// A value of Agency.
    /// </summary>
    [JsonPropertyName("agency")]
    public CodeValue Agency
    {
      get => agency ??= new();
      set => agency = value;
    }

    /// <summary>
    /// A value of H.
    /// </summary>
    [JsonPropertyName("h")]
    public InterstateRequest H
    {
      get => h ??= new();
      set => h = value;
    }

    /// <summary>
    /// A value of HotherState.
    /// </summary>
    [JsonPropertyName("hotherState")]
    public Fips HotherState
    {
      get => hotherState ??= new();
      set => hotherState = value;
    }

    /// <summary>
    /// A value of OtherStateFips.
    /// </summary>
    [JsonPropertyName("otherStateFips")]
    public OeWorkGroup OtherStateFips
    {
      get => otherStateFips ??= new();
      set => otherStateFips = value;
    }

    /// <summary>
    /// A value of PaymentCountryPrompt.
    /// </summary>
    [JsonPropertyName("paymentCountryPrompt")]
    public Common PaymentCountryPrompt
    {
      get => paymentCountryPrompt ??= new();
      set => paymentCountryPrompt = value;
    }

    /// <summary>
    /// A value of ContactCountryPrompt.
    /// </summary>
    [JsonPropertyName("contactCountryPrompt")]
    public Common ContactCountryPrompt
    {
      get => contactCountryPrompt ??= new();
      set => contactCountryPrompt = value;
    }

    /// <summary>
    /// A value of IreqUpdatedDate.
    /// </summary>
    [JsonPropertyName("ireqUpdatedDate")]
    public DateWorkArea IreqUpdatedDate
    {
      get => ireqUpdatedDate ??= new();
      set => ireqUpdatedDate = value;
    }

    /// <summary>
    /// A value of IreqCreatedDate.
    /// </summary>
    [JsonPropertyName("ireqCreatedDate")]
    public DateWorkArea IreqCreatedDate
    {
      get => ireqCreatedDate ??= new();
      set => ireqCreatedDate = value;
    }

    /// <summary>
    /// A value of ContactPhone.
    /// </summary>
    [JsonPropertyName("contactPhone")]
    public InterstateWorkArea ContactPhone
    {
      get => contactPhone ??= new();
      set => contactPhone = value;
    }

    /// <summary>
    /// A value of IreqCaseProgramPrompt.
    /// </summary>
    [JsonPropertyName("ireqCaseProgramPrompt")]
    public Common IreqCaseProgramPrompt
    {
      get => ireqCaseProgramPrompt ??= new();
      set => ireqCaseProgramPrompt = value;
    }

    /// <summary>
    /// A value of IreqCaseClosurePrompt.
    /// </summary>
    [JsonPropertyName("ireqCaseClosurePrompt")]
    public Common IreqCaseClosurePrompt
    {
      get => ireqCaseClosurePrompt ??= new();
      set => ireqCaseClosurePrompt = value;
    }

    /// <summary>
    /// A value of PaymentStatePrompt.
    /// </summary>
    [JsonPropertyName("paymentStatePrompt")]
    public Common PaymentStatePrompt
    {
      get => paymentStatePrompt ??= new();
      set => paymentStatePrompt = value;
    }

    /// <summary>
    /// A value of ContactStatePrompt.
    /// </summary>
    [JsonPropertyName("contactStatePrompt")]
    public Common ContactStatePrompt
    {
      get => contactStatePrompt ??= new();
      set => contactStatePrompt = value;
    }

    /// <summary>
    /// A value of IreqCountryPrompt.
    /// </summary>
    [JsonPropertyName("ireqCountryPrompt")]
    public Common IreqCountryPrompt
    {
      get => ireqCountryPrompt ??= new();
      set => ireqCountryPrompt = value;
    }

    /// <summary>
    /// A value of IreqStatePrompt.
    /// </summary>
    [JsonPropertyName("ireqStatePrompt")]
    public Common IreqStatePrompt
    {
      get => ireqStatePrompt ??= new();
      set => ireqStatePrompt = value;
    }

    /// <summary>
    /// A value of Note.
    /// </summary>
    [JsonPropertyName("note")]
    public InterstateRequestHistory Note
    {
      get => note ??= new();
      set => note = value;
    }

    /// <summary>
    /// A value of DisplayOnly.
    /// </summary>
    [JsonPropertyName("displayOnly")]
    public Case1 DisplayOnly
    {
      get => displayOnly ??= new();
      set => displayOnly = value;
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
    /// A value of OtherState.
    /// </summary>
    [JsonPropertyName("otherState")]
    public Fips OtherState
    {
      get => otherState ??= new();
      set => otherState = value;
    }

    /// <summary>
    /// A value of InterstateContact.
    /// </summary>
    [JsonPropertyName("interstateContact")]
    public InterstateContact InterstateContact
    {
      get => interstateContact ??= new();
      set => interstateContact = value;
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
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePersonsWorkSet Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of ApCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("apCsePersonsWorkSet")]
    public CsePersonsWorkSet ApCsePersonsWorkSet
    {
      get => apCsePersonsWorkSet ??= new();
      set => apCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of InterstatePaymentAddress.
    /// </summary>
    [JsonPropertyName("interstatePaymentAddress")]
    public InterstatePaymentAddress InterstatePaymentAddress
    {
      get => interstatePaymentAddress ??= new();
      set => interstatePaymentAddress = value;
    }

    /// <summary>
    /// A value of InterstateContactAddress.
    /// </summary>
    [JsonPropertyName("interstateContactAddress")]
    public InterstateContactAddress InterstateContactAddress
    {
      get => interstateContactAddress ??= new();
      set => interstateContactAddress = value;
    }

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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
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
    /// A value of OspServiceProvider.
    /// </summary>
    [JsonPropertyName("ospServiceProvider")]
    public ServiceProvider OspServiceProvider
    {
      get => ospServiceProvider ??= new();
      set => ospServiceProvider = value;
    }

    /// <summary>
    /// A value of OspOffice.
    /// </summary>
    [JsonPropertyName("ospOffice")]
    public Office OspOffice
    {
      get => ospOffice ??= new();
      set => ospOffice = value;
    }

    /// <summary>
    /// A value of Prompt.
    /// </summary>
    [JsonPropertyName("prompt")]
    public Code Prompt
    {
      get => prompt ??= new();
      set => prompt = value;
    }

    /// <summary>
    /// A value of HiddenInterstateRequest.
    /// </summary>
    [JsonPropertyName("hiddenInterstateRequest")]
    public InterstateRequest HiddenInterstateRequest
    {
      get => hiddenInterstateRequest ??= new();
      set => hiddenInterstateRequest = value;
    }

    /// <summary>
    /// A value of DupCaseIndicatorPrompt.
    /// </summary>
    [JsonPropertyName("dupCaseIndicatorPrompt")]
    public Common DupCaseIndicatorPrompt
    {
      get => dupCaseIndicatorPrompt ??= new();
      set => dupCaseIndicatorPrompt = value;
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
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public InterstateRequest Prev
    {
      get => prev ??= new();
      set => prev = value;
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

    /// <summary>
    /// A value of AutoFlow.
    /// </summary>
    [JsonPropertyName("autoFlow")]
    public Common AutoFlow
    {
      get => autoFlow ??= new();
      set => autoFlow = value;
    }

    private Common addressMismatch;
    private Common ireqTribalPrompt;
    private Fips selectedFips;
    private InterstateRequest selectedInterstateRequest;
    private Case1 case1;
    private Case1 hduplicate;
    private CodeValue agency;
    private InterstateRequest h;
    private Fips hotherState;
    private OeWorkGroup otherStateFips;
    private Common paymentCountryPrompt;
    private Common contactCountryPrompt;
    private DateWorkArea ireqUpdatedDate;
    private DateWorkArea ireqCreatedDate;
    private InterstateWorkArea contactPhone;
    private Common ireqCaseProgramPrompt;
    private Common ireqCaseClosurePrompt;
    private Common paymentStatePrompt;
    private Common contactStatePrompt;
    private Common ireqCountryPrompt;
    private Common ireqStatePrompt;
    private InterstateRequestHistory note;
    private Case1 displayOnly;
    private Case1 next;
    private Fips otherState;
    private InterstateContact interstateContact;
    private InterstateRequest interstateRequest;
    private CsePersonsWorkSet ar;
    private CsePersonsWorkSet apCsePersonsWorkSet;
    private InterstatePaymentAddress interstatePaymentAddress;
    private InterstateContactAddress interstateContactAddress;
    private Common personPrompt;
    private Standard standard;
    private CsePerson apCsePerson;
    private NextTranInfo hiddenNextTranInfo;
    private ServiceProvider ospServiceProvider;
    private Office ospOffice;
    private Code prompt;
    private InterstateRequest hiddenInterstateRequest;
    private Common dupCaseIndicatorPrompt;
    private InterstateCase interstateCase;
    private InterstateRequest prev;
    private WorkArea headerLine;
    private Common autoFlow;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of TribalAgencyCommon.
    /// </summary>
    [JsonPropertyName("tribalAgencyCommon")]
    public Common TribalAgencyCommon
    {
      get => tribalAgencyCommon ??= new();
      set => tribalAgencyCommon = value;
    }

    /// <summary>
    /// A value of Common1.
    /// </summary>
    [JsonPropertyName("common1")]
    public Common Common1
    {
      get => common1 ??= new();
      set => common1 = value;
    }

    /// <summary>
    /// A value of TribalAgencyCode.
    /// </summary>
    [JsonPropertyName("tribalAgencyCode")]
    public Code TribalAgencyCode
    {
      get => tribalAgencyCode ??= new();
      set => tribalAgencyCode = value;
    }

    /// <summary>
    /// A value of CsenetStateTable.
    /// </summary>
    [JsonPropertyName("csenetStateTable")]
    public CsenetStateTable CsenetStateTable
    {
      get => csenetStateTable ??= new();
      set => csenetStateTable = value;
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
    /// A value of RetFromCdvl.
    /// </summary>
    [JsonPropertyName("retFromCdvl")]
    public Common RetFromCdvl
    {
      get => retFromCdvl ??= new();
      set => retFromCdvl = value;
    }

    /// <summary>
    /// A value of CaseStatusChangedInd.
    /// </summary>
    [JsonPropertyName("caseStatusChangedInd")]
    public Common CaseStatusChangedInd
    {
      get => caseStatusChangedInd ??= new();
      set => caseStatusChangedInd = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
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
    /// A value of UpdateStatus.
    /// </summary>
    [JsonPropertyName("updateStatus")]
    public InterstateRequest UpdateStatus
    {
      get => updateStatus ??= new();
      set => updateStatus = value;
    }

    /// <summary>
    /// A value of Specific.
    /// </summary>
    [JsonPropertyName("specific")]
    public InterstateRequestHistory Specific
    {
      get => specific ??= new();
      set => specific = value;
    }

    /// <summary>
    /// A value of ScreenIndentification.
    /// </summary>
    [JsonPropertyName("screenIndentification")]
    public Common ScreenIndentification
    {
      get => screenIndentification ??= new();
      set => screenIndentification = value;
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
    /// A value of Common2.
    /// </summary>
    [JsonPropertyName("common2")]
    public Common Common2
    {
      get => common2 ??= new();
      set => common2 = value;
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePersonsWorkSet Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of InterstateRequestCount.
    /// </summary>
    [JsonPropertyName("interstateRequestCount")]
    public Common InterstateRequestCount
    {
      get => interstateRequestCount ??= new();
      set => interstateRequestCount = value;
    }

    /// <summary>
    /// A value of MultipleAps.
    /// </summary>
    [JsonPropertyName("multipleAps")]
    public Common MultipleAps
    {
      get => multipleAps ??= new();
      set => multipleAps = value;
    }

    /// <summary>
    /// A value of Invalid.
    /// </summary>
    [JsonPropertyName("invalid")]
    public Common Invalid
    {
      get => invalid ??= new();
      set => invalid = value;
    }

    /// <summary>
    /// A value of RetcompInd.
    /// </summary>
    [JsonPropertyName("retcompInd")]
    public Common RetcompInd
    {
      get => retcompInd ??= new();
      set => retcompInd = value;
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
    /// A value of CaseMarkedDuplicate.
    /// </summary>
    [JsonPropertyName("caseMarkedDuplicate")]
    public Common CaseMarkedDuplicate
    {
      get => caseMarkedDuplicate ??= new();
      set => caseMarkedDuplicate = value;
    }

    /// <summary>
    /// A value of CaseClosed.
    /// </summary>
    [JsonPropertyName("caseClosed")]
    public Common CaseClosed
    {
      get => caseClosed ??= new();
      set => caseClosed = value;
    }

    /// <summary>
    /// A value of ChangeProgram.
    /// </summary>
    [JsonPropertyName("changeProgram")]
    public Common ChangeProgram
    {
      get => changeProgram ??= new();
      set => changeProgram = value;
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
    /// A value of RefreshPhone.
    /// </summary>
    [JsonPropertyName("refreshPhone")]
    public InterstateWorkArea RefreshPhone
    {
      get => refreshPhone ??= new();
      set => refreshPhone = value;
    }

    /// <summary>
    /// A value of RefreshFips.
    /// </summary>
    [JsonPropertyName("refreshFips")]
    public Fips RefreshFips
    {
      get => refreshFips ??= new();
      set => refreshFips = value;
    }

    /// <summary>
    /// A value of StateCommon.
    /// </summary>
    [JsonPropertyName("stateCommon")]
    public Common StateCommon
    {
      get => stateCommon ??= new();
      set => stateCommon = value;
    }

    /// <summary>
    /// A value of FipsError.
    /// </summary>
    [JsonPropertyName("fipsError")]
    public Common FipsError
    {
      get => fipsError ??= new();
      set => fipsError = value;
    }

    /// <summary>
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public DateWorkArea Zero
    {
      get => zero ??= new();
      set => zero = value;
    }

    /// <summary>
    /// A value of RefreshInterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("refreshInterstateRequestHistory")]
    public InterstateRequestHistory RefreshInterstateRequestHistory
    {
      get => refreshInterstateRequestHistory ??= new();
      set => refreshInterstateRequestHistory = value;
    }

    /// <summary>
    /// A value of CsenetProgramType.
    /// </summary>
    [JsonPropertyName("csenetProgramType")]
    public Code CsenetProgramType
    {
      get => csenetProgramType ??= new();
      set => csenetProgramType = value;
    }

    /// <summary>
    /// A value of CsenetCaseClosure.
    /// </summary>
    [JsonPropertyName("csenetCaseClosure")]
    public Code CsenetCaseClosure
    {
      get => csenetCaseClosure ??= new();
      set => csenetCaseClosure = value;
    }

    /// <summary>
    /// A value of Country.
    /// </summary>
    [JsonPropertyName("country")]
    public Code Country
    {
      get => country ??= new();
      set => country = value;
    }

    /// <summary>
    /// A value of RefreshInterstateContact.
    /// </summary>
    [JsonPropertyName("refreshInterstateContact")]
    public InterstateContact RefreshInterstateContact
    {
      get => refreshInterstateContact ??= new();
      set => refreshInterstateContact = value;
    }

    /// <summary>
    /// A value of RefreshInterstatePaymentAddress.
    /// </summary>
    [JsonPropertyName("refreshInterstatePaymentAddress")]
    public InterstatePaymentAddress RefreshInterstatePaymentAddress
    {
      get => refreshInterstatePaymentAddress ??= new();
      set => refreshInterstatePaymentAddress = value;
    }

    /// <summary>
    /// A value of RefreshInterstateContactAddress.
    /// </summary>
    [JsonPropertyName("refreshInterstateContactAddress")]
    public InterstateContactAddress RefreshInterstateContactAddress
    {
      get => refreshInterstateContactAddress ??= new();
      set => refreshInterstateContactAddress = value;
    }

    /// <summary>
    /// A value of RefreshCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("refreshCsePersonsWorkSet")]
    public CsePersonsWorkSet RefreshCsePersonsWorkSet
    {
      get => refreshCsePersonsWorkSet ??= new();
      set => refreshCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of RefreshInterstateRequest.
    /// </summary>
    [JsonPropertyName("refreshInterstateRequest")]
    public InterstateRequest RefreshInterstateRequest
    {
      get => refreshInterstateRequest ??= new();
      set => refreshInterstateRequest = value;
    }

    /// <summary>
    /// A value of RefreshCase.
    /// </summary>
    [JsonPropertyName("refreshCase")]
    public Case1 RefreshCase
    {
      get => refreshCase ??= new();
      set => refreshCase = value;
    }

    /// <summary>
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public Common Error
    {
      get => error ??= new();
      set => error = value;
    }

    /// <summary>
    /// A value of ZeroFill.
    /// </summary>
    [JsonPropertyName("zeroFill")]
    public TextWorkArea ZeroFill
    {
      get => zeroFill ??= new();
      set => zeroFill = value;
    }

    /// <summary>
    /// A value of StateCode.
    /// </summary>
    [JsonPropertyName("stateCode")]
    public Code StateCode
    {
      get => stateCode ??= new();
      set => stateCode = value;
    }

    /// <summary>
    /// A value of Validation.
    /// </summary>
    [JsonPropertyName("validation")]
    public CodeValue Validation
    {
      get => validation ??= new();
      set => validation = value;
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

    private Common tribalAgencyCommon;
    private Common common1;
    private Code tribalAgencyCode;
    private CsenetStateTable csenetStateTable;
    private DateWorkArea null1;
    private Common retFromCdvl;
    private Common caseStatusChangedInd;
    private DateWorkArea current;
    private InterstateRequestHistory interstateRequestHistory;
    private InterstateRequest updateStatus;
    private InterstateRequestHistory specific;
    private Common screenIndentification;
    private Infrastructure infrastructure;
    private OutgoingDocument outgoingDocument;
    private Document document;
    private SpDocKey spDocKey;
    private Common common2;
    private Code code;
    private CodeValue codeValue;
    private CsePersonsWorkSet ap;
    private CsePersonsWorkSet ar;
    private Common interstateRequestCount;
    private Common multipleAps;
    private Common invalid;
    private Common retcompInd;
    private Common caseOpen;
    private Common caseMarkedDuplicate;
    private Common caseClosed;
    private Common changeProgram;
    private OeWorkGroup refreshOeWorkGroup;
    private InterstateWorkArea refreshPhone;
    private Fips refreshFips;
    private Common stateCommon;
    private Common fipsError;
    private DateWorkArea zero;
    private InterstateRequestHistory refreshInterstateRequestHistory;
    private Code csenetProgramType;
    private Code csenetCaseClosure;
    private Code country;
    private InterstateContact refreshInterstateContact;
    private InterstatePaymentAddress refreshInterstatePaymentAddress;
    private InterstateContactAddress refreshInterstateContactAddress;
    private CsePersonsWorkSet refreshCsePersonsWorkSet;
    private InterstateRequest refreshInterstateRequest;
    private Case1 refreshCase;
    private Common error;
    private TextWorkArea zeroFill;
    private Code stateCode;
    private CodeValue validation;
    private DateWorkArea max;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
    }

    private Case1 case1;
    private CaseUnit caseUnit;
  }
#endregion
}
