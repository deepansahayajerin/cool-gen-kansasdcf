// Program: SI_ARDS_AR_DETAILS, ID: 371756946, model: 746.
// Short name: SWEARDSP
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
/// A program: SI_ARDS_AR_DETAILS.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiArdsArDetails: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_ARDS_AR_DETAILS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiArdsArDetails(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiArdsArDetails.
  /// </summary>
  public SiArdsArDetails(IContext context, Import import, Export export):
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
    // Date	  Developer		Description
    // 05/11/95  JeHoward		KESSEP
    // 08/02/95  H Sharland- MTW
    // 02/25/96  Paul Elie		Retrofit Security and Nexttran etc
    // 09/24/96  G. Lofton		Add county code
    // 11/01/96  G. Lofton		Add new security and remove old.
    // 05/09/97  Sid Chowdhary		Add ADC Open and Closed dates.
    // 07/12/97  Sid			Display AE Mailing address.
    // 09/25/98  C Deghand             Added IF's to set the correct
    //                                 
    // prompts to error.  Added SET's
    // to
    //                                 
    // change the prompts to SPACES on
    // a
    //                                 
    // display.
    // 11/2/98   C Deghand             Added code to make sure protected
    //                                 
    // fields stay protected.
    // ------------------------------------------------------------
    // 01/14/99 W.Campbell             Some code disabled and
    //                                 
    // some new code inserted in order
    //                                 
    // to use new logic to obtain
    //                                 
    // the CSE_PERSON_ADDRESS.
    // ------------------------------------------------------
    // 02/03/99 W.Campbell             Inserted a statement
    //                                 
    // to set local_ar cse_person
    //                                 
    // number to the export_ar
    //                                 
    // cse_person_work_set number,
    //                                 
    // so that the correct data will
    //                                 
    // get passed.
    // ------------------------------------------------------
    // 02/04/99 W.Campbell             Code added to create
    //                                 
    // infrastructure record(s)
    //                                 
    // for change(s) to SSN.  This is
    //                                 
    // to take care of the situation
    //                                 
    // where this AR is also an AP
    //                                 
    // on some other CASE(s).
    // -------------------------------------------------------
    // 02/08/99 W.Campbell             The logic for update of
    //                                 
    // ADABAS was rearranged so that
    //                                 
    // it would occur after all other
    //                                 
    // DB/2 updates in case a
    //                                 
    // ROLLBACK was needed for DB/2
    //                                 
    // since ADABAS does not have
    //                                 
    // rollback capability.
    // ------------------------------------------------------------
    // 02/08/99 W.Campbell             Added logic to USE
    //                                 
    // EAB_ROLLBACK_CICS to
    //                                 
    // help ensure correct rollback
    //                                 
    // of DB/2 updates.
    // ------------------------------------------------------------
    // 02/11/99 W.Campbell             IF stmt copied and
    //                                 
    // disabled just to keep it in
    //                                 
    // case it needs to be re-enabled
    //                                 
    // again.  The original IF stmt
    //                                 
    // was modified so that it will be
    //                                 
    // true when a SSN is changed
    //                                 
    // from some non-blank (zero)
    //                                 
    // value to a blank(zero) value.
    //                                 
    // This is in the logic which
    //                                 
    // creates a row on the
    // infrastructure
    //                                 
    // table(to log an event).
    // ---------------------------------------------
    // 02/11/99 W.Campbell             IF stmt inserted to
    //                                 
    // log 'Unknown' when the SSN
    //                                 
    // is changed from non-blanks
    //                                 
    // (zeros) to blanks (zeros).
    //                                 
    // This is in the logic which
    //                                 
    // creates a row on the
    // infrastructure
    //                                 
    // table(to log an event).
    // ---------------------------------------------
    // 02/23/99 W.Campbell             Deleted a disabled USE
    //                                 
    // statement for
    //                                 
    // ZDEL_SI_READ_CSE_PERSON_ADDRESS
    // -------------------------------------------------
    // 05/03/99 W.Campbell             Added code to send
    //                                 
    // selection needed msg to COMP.
    //                                 
    // BE SURE TO MATCH
    //                                 
    // next_tran_info ON THE
    //                                 
    // DIALOG FLOW.
    // -----------------------------------------------
    // 07/26/99 W.Campbell             Changed the screen
    //                                 
    // properties for the next CASE
    // field
    //                                 
    // to put the cursor here so that
    //                                 
    // the cursor will be in that field
    // on
    //                                 
    // a successful dispaly.
    // -----------------------------------------------
    // 07/03/99 M.Lachowicz            Added validation for sex.
    // -----------------------------------------------
    // 02/07/00 M.Lachowicz            Fixed code to process
    //                                 
    // NEXT_TRAN.
    // -----------------------------------------------
    // 03/30/00 W.Campbell             Modified an ESCAPE
    //                                 
    // statement after check for bad
    //                                 
    // return from SECURITY so that
    //                                 
    // it completely leaves the
    //                                 
    // Pstep.  Work done
    //                                 
    // under WR#000162 for
    //                                 
    // Family Violence.
    // ---------------------------------------------
    // 03/30/00 W.Campbell             Changed view matching
    //                                 
    // for the USE of
    //                                 
    // SC_CAB_TEST_SECURITY.
    //                                 
    // Changed view matching for the
    //                                 
    // cab's inport case to the Pstep's
    //                                 
    // export_next case.  It previously
    //                                 
    // was to the Pstep's inport_next
    // case.
    //                                 
    // Work done on WR#000162 for
    //                                 
    // PRWORA Family Violence
    // Indicator.
    // ---------------------------------------------
    // 09/29/00 M.Ramirez             WR# 217  Added AR person number to 
    // next_tran details
    // ---------------------------------------------
    // 11/22/00 M.Lachowicz            Changed header line.
    //                                 
    // Work done on WR#00298..
    // ---------------------------------------------
    // 02/22/01 M.Lachowicz            Sex code is mandatory.
    //                                 
    // Work done on PR 113332.
    // ---------------------------------------------
    // 03/09/01 M.Lachowicz            Made the following changes
    //                                 
    // 1. Allow to update even case is
    // closed.
    //                                 
    // 2. Address should be most
    // recently
    //                                     
    // verified.
    //                                 
    // Work done on WR 241.
    // ---------------------------------------------
    // ----------------------------------------------------------------------------------
    // 08/27/2001    Vithal Madhira        PR# 121249, 124583, 124584
    // Fixed the code for family violence indicator. The screen is not 
    // displaying data even  if the family violence indicator is not on the AP.
    // It must display data if the family violence indicator is not on AP.
    // Changed code in SWE01082(SC_CAB_TEST_SECURITY)  and SWE00301(
    // SC_SECURITY_VALID_AUTH_FOR_FV) CABs and ARDS PSTEP.
    // ---------------------------------------------------------------------------------
    // PR141874 on March 20, 2002 by L. Bachura. Removed TAF dates from the ARDS
    // screen per PR 141874.
    // ---------------------------------------------
    // 07/03/02 P.Phinney    WR020205  Add Place Of Birth Fields to Screen.
    // ---------------------------------------------
    // -------------------------------------------------------------
    // 11/04/02 M. Lachowicz - Validate first, last and middle name.
    //                         Work made on PR160147.
    // -------------------------------------------------------------
    // 09/09/05  GVandy  WR00256682  Add indicator to screen to indicate if 
    // individual was displaced by Hurricane Katrina.
    // 11/29/05  GVandy  PR00260353  Add effective and end dates to Hurricane 
    // Katrina displaced_person records.
    // 10/29/07     LSS     PR# 180608 / CQ406
    // Added verify statement to error out if ssn contains a non-numeric 
    // character.
    // 03/11/08  GVandy  CQ296	Add display and maintenance of CSE_Person 
    // Prior_TAF_Ind on the screen.
    // 06/05/2009   DDupree     Added check when updating or adding a ssn 
    // against the invalid ssn table. Part of CQ7189.
    // __________________________________________________________________________________
    // 06/13/11  RMathews  CQ21791 Added edit for start and end dates when 
    // reading for MO and FA roles.
    // 5/01/2018  JHarden  CQ61889  Ability to select, view and update ARDS for 
    // inactive AR'S by going to ROLE screen and selecting.
    // 09/13/2018  JHarden  CQ64095  add Tribal fields to APDS/ARDS/CHDS
    // 2/27/2019   JHarden   CQ65304  End date CP address when date of death is 
    // entered.
    // 08/27/19  JHarden  CQ66290  Add an indicator for threats made on staff.
    // 12/23/2020  GVandy  CQ68785  Add customer service code.
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;
    local.MaxDate.Date = UseCabSetMaximumDiscontinueDate();
    MoveNextTranInfo(import.HiddenNextTranInfo, export.HiddenNextTranInfo);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // ---------------------------------------------
    // Move Imports to Exports
    // ---------------------------------------------
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.Next.Number = import.Next.Number;
    MoveCase3(import.Case1, export.Case1);
    export.ArCsePerson.Assign(import.ArCsePerson);
    MoveCsePersonAddress4(import.CsePersonAddress, export.CsePersonAddress);
    export.ArCaseRole.Assign(import.ArCaseRole);
    export.Alt.Text13 = import.Alt.Text13;
    MoveCsePersonsWorkSet1(import.ApCsePersonsWorkSet,
      export.ApCsePersonsWorkSet);
    export.ArCsePersonsWorkSet.Assign(import.ArCsePersonsWorkSet);
    export.ArSsnWorkArea.Assign(import.ArSsnWorkArea);
    export.ApPrompt.SelectChar = import.ApPrompt.SelectChar;
    export.RacePrompt.SelectChar = import.RacePrompt.SelectChar;
    MoveCsePersonsWorkSet2(import.SelectedAp, export.SelectedAp);
    export.PhoneTypePrompt.SelectChar = import.PhoneTypePrompt.SelectChar;
    export.ServiceProvider.LastName = import.ServiceProvider.LastName;
    MoveOffice(import.Office, export.Office);
    export.CaseOpen.Flag = import.CaseOpen.Flag;
    export.CsePersonEmailAddress.EmailAddress =
      import.CsePersonEmailAddress.EmailAddress;

    // CQ64095
    export.TribalFlag.Flag = import.TribalFlag.Flag;
    export.TribalPrompt.SelectChar = import.TribalPrompt.SelectChar;

    // CQ61889
    export.ArPrompt.SelectChar = import.ArPrompt.SelectChar;

    // 09/09/05  GVandy  WR00256682  Add indicator to screen to indicate if 
    // individual was displaced by Hurricane Katrina.
    MoveDisplacedPerson(import.DisplacedPerson, export.DisplacedPerson);

    if (AsChar(export.DisplacedPerson.DisplacedInterfaceInd) != 'Y')
    {
      var field = GetField(export.DisplacedPerson, "displacedInd");

      field.Color = "green";
      field.Highlighting = Highlighting.Underscore;
      field.Protected = false;
    }

    export.CustomerServicePrompt.SelectChar =
      import.CustomerServicePrompt.SelectChar;

    // 07/03/02 P.Phinney    WR020205  Add Place Of Birth Fields to Screen.
    // ---------------------------------------------
    export.PobStPrompt.SelectChar = import.PobStPrompt.SelectChar;
    export.PobFcPrompt.SelectChar = import.PobFcPrompt.SelectChar;
    export.WorkForeignCountryDesc.Text40 = import.WorkForeignCountryDesc.Text40;

    // 11/22/00 M.L Start
    export.HeaderLine.Text35 = import.HeaderLine.Text35;

    // 11/22/00 M.L End
    // ---------------------------------------------
    // Move Hidden Import Views to Hidden Export Views
    // ---------------------------------------------
    export.HiddenPrevCase.Number = import.HiddenPrevCase.Number;
    export.HiddenPrevCsePersonsWorkSet.Number =
      import.HiddenPrevCsePersonsWorkSet.Number;
    export.HiddenAe.Flag = import.HiddenAe.Flag;
    MoveCsePerson1(import.HiddenCsePerson, export.HiddenCsePerson);

    // ---------------------------------------------
    // 02/04/99 W.Campbell - Code added to create
    // infrastructure record(s) for change(s) to SSN.
    // ---------------------------------------------
    export.LastReadHidden.Ssn = import.LastReadHidden.Ssn;

    // 07/30/99 Start
    export.HiddenArSex.Sex = import.HiddenArSex.Sex;

    // 07/30/99 End
    // 02/07/00 M.L Start
    // ---------------------------------------------
    //         	N E X T   T R A N
    // ---------------------------------------------
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      export.HiddenNextTranInfo.CaseNumber = export.Case1.Number;
      export.HiddenNextTranInfo.CsePersonNumberAp =
        export.ApCsePersonsWorkSet.Number;

      // mjr
      // --------------------------------------------
      // 09/29/2000
      // WR# 217 - Added AR person number to next_tran details
      // ---------------------------------------------------------
      export.HiddenNextTranInfo.CsePersonNumber =
        export.ArCsePersonsWorkSet.Number;
      UseScCabNextTranPut();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        // mjr
        // --------------------------------------------
        // 09/29/2000
        // WR# 217 - Added check for AP person number in
        // cse_person_number_ap and cse_person_number_obligor
        // of next_tran
        // ---------------------------------------------------------
        if (!IsEmpty(export.HiddenNextTranInfo.CsePersonNumberAp))
        {
          export.ApCsePersonsWorkSet.Number =
            export.HiddenNextTranInfo.CsePersonNumberAp ?? Spaces(10);
        }
        else if (!IsEmpty(export.HiddenNextTranInfo.CsePersonNumberObligor))
        {
          export.ApCsePersonsWorkSet.Number =
            export.HiddenNextTranInfo.CsePersonNumberObligor ?? Spaces(10);
        }
        else
        {
        }

        export.Case1.Number = export.HiddenNextTranInfo.CaseNumber ?? Spaces
          (10);
        export.Next.Number = export.HiddenNextTranInfo.CaseNumber ?? Spaces(10);
        global.Command = "DISPLAY";
      }
      else
      {
      }
    }

    // 02/07/00 M.L End
    // ---------------------------------------------
    // 02/04/99 W.Campbell - End of Code added to create
    // infrastructure record(s) for change(s) to SSN.
    // ---------------------------------------------
    // ---------------------------------------------
    // 10/19/99 JF.Caillouet -  Moved Left Fill of Next Case Number so it can be
    // used sooner than later  - (ex.  the Security CAB.)
    // ---------------------------------------------
    if (!IsEmpty(export.Next.Number))
    {
      UseCabZeroFillNumber();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        if (IsExitState("CASE_NUMBER_NOT_NUMERIC"))
        {
          var field = GetField(export.Next, "number");

          field.Error = true;

          return;
        }
      }
    }
    else
    {
      var field = GetField(export.Next, "number");

      field.Error = true;

      ExitState = "CASE_NUMBER_REQUIRED";

      return;
    }

    if (IsEmpty(export.Case1.Number))
    {
      export.Case1.Number = export.Next.Number;
    }

    if (!Equal(export.Next.Number, export.Case1.Number))
    {
      export.Case1.Number = export.Next.Number;
      export.ApCsePersonsWorkSet.Number = "";
    }

    if (!IsEmpty(export.ApCsePersonsWorkSet.Number))
    {
      local.TextWorkArea.Text10 = export.ApCsePersonsWorkSet.Number;
      UseEabPadLeftWithZeros();
      export.ApCsePersonsWorkSet.Number = local.TextWorkArea.Text10;
    }

    // 02/07/00 M.L Start
    // 02/07/00 M.L End
    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "UPDATE"))
    {
      // 03/09/01 M.L Start
      // 03/09/01 M.L End
    }

    // ------------------------------------------------------------
    // Protect fields if needed
    // ------------------------------------------------------------
    if (Equal(export.Next.Number, export.HiddenPrevCase.Number))
    {
      if (AsChar(export.HiddenAe.Flag) == 'O')
      {
        var field1 = GetField(export.ArCsePersonsWorkSet, "number");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.ArCsePersonsWorkSet, "dob");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.ArCsePersonsWorkSet, "firstName");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.ArCsePersonsWorkSet, "lastName");

        field4.Color = "cyan";
        field4.Protected = true;

        var field5 = GetField(export.ArCsePersonsWorkSet, "middleInitial");

        field5.Color = "cyan";
        field5.Protected = true;

        var field6 = GetField(export.ArCsePersonsWorkSet, "sex");

        field6.Color = "cyan";
        field6.Protected = true;

        var field7 = GetField(export.ArSsnWorkArea, "ssnNumPart1");

        field7.Color = "cyan";
        field7.Protected = true;

        var field8 = GetField(export.ArSsnWorkArea, "ssnNumPart2");

        field8.Color = "cyan";
        field8.Protected = true;

        var field9 = GetField(export.ArSsnWorkArea, "ssnNumPart3");

        field9.Color = "cyan";
        field9.Protected = true;

        var field10 = GetField(export.ArSsnWorkArea, "ssnTextPart1");

        field10.Color = "cyan";
        field10.Protected = true;

        var field11 = GetField(export.ArSsnWorkArea, "ssnTextPart2");

        field11.Color = "cyan";
        field11.Protected = true;

        var field12 = GetField(export.ArSsnWorkArea, "ssnTextPart3");

        field12.Color = "cyan";
        field12.Protected = true;
      }

      if (CharAt(export.ArCsePersonsWorkSet.Number, 10) == 'O')
      {
        var field1 = GetField(export.ArCsePersonsWorkSet, "number");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.ArCsePersonsWorkSet, "dob");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.ArCsePersonsWorkSet, "firstName");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.ArCsePersonsWorkSet, "lastName");

        field4.Color = "cyan";
        field4.Protected = true;

        var field5 = GetField(export.ArCsePersonsWorkSet, "middleInitial");

        field5.Color = "cyan";
        field5.Protected = true;

        var field6 = GetField(export.ArCsePersonsWorkSet, "sex");

        field6.Color = "cyan";
        field6.Protected = true;

        var field7 = GetField(export.ArSsnWorkArea, "ssnNumPart1");

        field7.Color = "cyan";
        field7.Protected = true;

        var field8 = GetField(export.ArSsnWorkArea, "ssnNumPart2");

        field8.Color = "cyan";
        field8.Protected = true;

        var field9 = GetField(export.ArSsnWorkArea, "ssnNumPart3");

        field9.Color = "cyan";
        field9.Protected = true;

        var field10 = GetField(export.ArSsnWorkArea, "ssnTextPart1");

        field10.Color = "cyan";
        field10.Protected = true;

        var field11 = GetField(export.ArSsnWorkArea, "ssnTextPart2");

        field11.Color = "cyan";
        field11.Protected = true;

        var field12 = GetField(export.ArSsnWorkArea, "ssnTextPart3");

        field12.Color = "cyan";
        field12.Protected = true;

        var field13 = GetField(export.ArCsePerson, "race");

        field13.Color = "cyan";
        field13.Protected = true;

        var field14 = GetField(export.RacePrompt, "selectChar");

        field14.Color = "cyan";
        field14.Protected = true;

        var field15 = GetField(export.ArCsePerson, "currentSpouseFirstName");

        field15.Color = "cyan";
        field15.Protected = true;

        var field16 = GetField(export.ArCsePerson, "currentSpouseLastName");

        field16.Color = "cyan";
        field16.Protected = true;

        var field17 = GetField(export.ArCsePerson, "currentSpouseMi");

        field17.Color = "cyan";
        field17.Protected = true;

        var field18 = GetField(export.ArCsePerson, "emergencyAreaCode");

        field18.Color = "cyan";
        field18.Protected = true;

        var field19 = GetField(export.ArCsePerson, "emergencyPhone");

        field19.Color = "cyan";
        field19.Protected = true;

        var field20 = GetField(export.ArCsePerson, "homePhone");

        field20.Color = "cyan";
        field20.Protected = true;

        var field21 = GetField(export.ArCsePerson, "homePhoneAreaCode");

        field21.Color = "cyan";
        field21.Protected = true;

        var field22 = GetField(export.ArCsePerson, "otherAreaCode");

        field22.Color = "cyan";
        field22.Protected = true;

        var field23 = GetField(export.ArCsePerson, "otherNumber");

        field23.Color = "cyan";
        field23.Protected = true;

        var field24 = GetField(export.ArCsePerson, "nameMaiden");

        field24.Color = "cyan";
        field24.Protected = true;

        var field25 = GetField(export.ArCsePerson, "priorTafInd");

        field25.Color = "cyan";
        field25.Protected = true;
      }
    }

    // ---------------------------------------------
    // When the control is returned from a LIST screen
    // Populate the appropriate promt fields.
    // ---------------------------------------------
    if (Equal(global.Command, "RETCOMP"))
    {
      if (AsChar(import.ApPrompt.SelectChar) == 'S')
      {
        export.ApPrompt.SelectChar = "";

        var field = GetField(export.ApPrompt, "selectChar");

        field.Protected = false;
        field.Focused = true;
      }

      if (!IsEmpty(import.SelectedAp.Number))
      {
        MoveCsePersonsWorkSet2(import.SelectedAp, export.ApCsePersonsWorkSet);
      }

      global.Command = "DISPLAY";
    }

    // CQ61889
    if (Equal(global.Command, "RETROLE"))
    {
      if (AsChar(import.ArPrompt.SelectChar) == 'S')
      {
        export.ArPrompt.SelectChar = "";

        var field = GetField(export.ArPrompt, "selectChar");

        field.Protected = false;
        field.Focused = true;
      }

      if (!IsEmpty(import.SelectedAr.Number))
      {
        export.ArCsePersonsWorkSet.Number = import.SelectedAr.Number;
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "RTLIST"))
    {
      // 07/03/02 P.Phinney    WR020205  Add Place Of Birth Fields to Screen.
      // ---------------------------------------------
      if (AsChar(export.PobStPrompt.SelectChar) == 'S')
      {
        export.ArCsePerson.BirthPlaceState = import.Selected.Cdvalue;
        export.PobStPrompt.SelectChar = "";

        var field = GetField(export.PobStPrompt, "selectChar");

        field.Protected = false;
        field.Focused = true;
      }

      if (AsChar(export.PobFcPrompt.SelectChar) == 'S')
      {
        if (!IsEmpty(import.Selected.Cdvalue))
        {
          export.ArCsePerson.BirthplaceCountry = import.Selected.Cdvalue;
          export.WorkForeignCountryDesc.Text40 = import.Selected.Description;
        }
        else
        {
          export.WorkForeignCountryDesc.Text40 = "";
        }

        export.PobFcPrompt.SelectChar = "";

        var field = GetField(export.PobFcPrompt, "selectChar");

        field.Protected = false;
        field.Focused = true;
      }

      if (AsChar(import.PhoneTypePrompt.SelectChar) == 'S')
      {
        if (!IsEmpty(import.Selected.Cdvalue))
        {
          export.ArCsePerson.OtherPhoneType = import.Selected.Cdvalue;
        }

        export.PhoneTypePrompt.SelectChar = "";

        var field = GetField(export.PhoneTypePrompt, "selectChar");

        field.Protected = false;
        field.Focused = true;
      }

      if (AsChar(import.RacePrompt.SelectChar) == 'S')
      {
        if (!IsEmpty(import.Selected.Cdvalue))
        {
          export.ArCsePerson.Race = import.Selected.Cdvalue;
        }

        export.RacePrompt.SelectChar = "";

        var field = GetField(export.RacePrompt, "selectChar");

        field.Protected = false;
        field.Focused = true;
      }

      // 12/23/2020  GVandy  CQ68785  Add customer service code.
      if (AsChar(import.CustomerServicePrompt.SelectChar) == 'S')
      {
        if (!IsEmpty(import.Selected.Cdvalue))
        {
          export.ArCsePerson.CustomerServiceCode = import.Selected.Cdvalue;
        }

        export.CustomerServicePrompt.SelectChar = "";

        var field = GetField(export.CustomerServicePrompt, "selectChar");

        field.Protected = false;
        field.Focused = true;
      }

      // CQ64095
      if (AsChar(import.TribalPrompt.SelectChar) == 'S')
      {
        if (!IsEmpty(import.Selected.Cdvalue))
        {
          export.ArCsePerson.TribalCode = import.Selected.Cdvalue;
        }

        export.TribalPrompt.SelectChar = "";

        var field = GetField(export.TribalPrompt, "selectChar");

        field.Protected = false;
        field.Focused = true;
      }

      if (!IsEmpty(export.ArCsePerson.TribalCode))
      {
        export.TribalFlag.Flag = "Y";
      }
      else
      {
        export.TribalFlag.Flag = "N";
      }

      return;
    }

    if (!Equal(global.Command, "DISPLAY"))
    {
      if (export.ArSsnWorkArea.SsnNumPart1 == 0 && export
        .ArSsnWorkArea.SsnNumPart2 == 0 && export.ArSsnWorkArea.SsnNumPart3 == 0
        )
      {
        export.ArCsePersonsWorkSet.Ssn = "";
      }
      else
      {
        MoveSsnWorkArea(export.ArSsnWorkArea, local.SsnWorkArea);
        local.SsnWorkArea.ConvertOption = "2";
        UseCabSsnConvertNumToText();
        export.ArCsePersonsWorkSet.Ssn = export.ArSsnWorkArea.SsnText9;
      }

      // PR# 180608 / CQ406   10/29/07   LSS   Commented out the SET statements 
      // and moved them to inside the UPDATE.
      // PR# 180608 / CQ406   10/29/07   LSS   Commented out the SET statement 
      // and moved it to inside the UPDATE after the validations.
      // PR160844. Changes to highlight the screen is a blank is entered in the 
      // ssn.
      local.SsnPart.Count = 0;

      if (IsEmpty(Substring(export.ArSsnWorkArea.SsnTextPart1, 1, 1)))
      {
        local.SsnPart.Count = (int)((long)local.SsnPart.Count + 1);
      }

      if (IsEmpty(Substring(export.ArSsnWorkArea.SsnTextPart1, 2, 1)))
      {
        local.SsnPart.Count = (int)((long)local.SsnPart.Count + 1);
      }

      if (IsEmpty(Substring(export.ArSsnWorkArea.SsnTextPart1, 3, 1)))
      {
        local.SsnPart.Count = (int)((long)local.SsnPart.Count + 1);
      }

      if (IsEmpty(Substring(export.ArSsnWorkArea.SsnTextPart2, 1, 1)))
      {
        local.SsnPart.Count = (int)((long)local.SsnPart.Count + 1);
      }

      if (IsEmpty(Substring(export.ArSsnWorkArea.SsnTextPart2, 2, 1)))
      {
        local.SsnPart.Count = (int)((long)local.SsnPart.Count + 1);
      }

      if (IsEmpty(Substring(export.ArSsnWorkArea.SsnTextPart3, 1, 1)))
      {
        local.SsnPart.Count = (int)((long)local.SsnPart.Count + 1);
      }

      if (IsEmpty(Substring(export.ArSsnWorkArea.SsnTextPart3, 2, 1)))
      {
        local.SsnPart.Count = (int)((long)local.SsnPart.Count + 1);
      }

      if (IsEmpty(Substring(export.ArSsnWorkArea.SsnTextPart3, 3, 1)))
      {
        local.SsnPart.Count = (int)((long)local.SsnPart.Count + 1);
      }

      if (IsEmpty(Substring(export.ArSsnWorkArea.SsnTextPart3, 4, 1)))
      {
        local.SsnPart.Count = (int)((long)local.SsnPart.Count + 1);
      }

      if (local.SsnPart.Count > 8)
      {
        goto Test1;
      }

      if (local.SsnPart.Count > 0 && local.SsnPart.Count < 9)
      {
        var field1 = GetField(export.ArSsnWorkArea, "ssnTextPart1");

        field1.Error = true;

        var field2 = GetField(export.ArSsnWorkArea, "ssnTextPart2");

        field2.Error = true;

        var field3 = GetField(export.ArSsnWorkArea, "ssnTextPart3");

        field3.Error = true;

        ExitState = "LE0000_SSN_CONTAINS_NONNUM";

        return;
      }

      // PR160844. End of changes to highlight the screen is a blank is entered 
      // in the ssn.
    }

Test1:

    // ---------------------------------------------
    //        S E C U R I T Y   L O G I C
    // ---------------------------------------------
    // to validate action level security
    if (Equal(global.Command, "ALTS") || Equal(global.Command, "EMAL"))
    {
    }
    else
    {
      // ---------------------------------------------
      // 03/30/00 W.Campbell - Changed view matching
      // for the USE of SC_CAB_TEST_SECURITY.
      // Changed view matching for the cab's inport case
      // to the Pstep's export_next case.  It previously
      // was to the Pstep's inport_next case.  Work done on
      // WR#000162 for PRWORA Family Violence Indicator.
      // ---------------------------------------------
      UseScCabTestSecurity();

      // ---------------------------------------------
      // 03/30/00 W.Campbell - Modified the following
      // ESCAPE so that it completely leaves the Pstep.
      // Work done under WR#000162 for Family
      // Violence.
      // ---------------------------------------------
      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // ---------------------------------------------
    //        P F K E Y   P R O C E S S I N G
    // ---------------------------------------------
    // ---------------------------------------------
    // Any commands specific to your procedure
    // should be added to the following CASE OF
    // statement.
    // Make sure that all unused PF keys are set to
    // INVALID on the screen definition.
    // ---------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "EMAL":
        export.ToEmal.SelectChar = "S";
        ExitState = "ECO_LNK_TO_EMAL";

        return;
      case "ALTS":
        ExitState = "ECO_XFR_TO_ALT_SSN_AND_ALIAS";

        return;
      case "EXIT":
        // --------------------------------------------
        // Allows the user to flow back to the previous
        // screen.
        // --------------------------------------------
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      case "LIST":
        // ---------------------------------------------
        // This command allows the user to link to a
        // selection list and retrieve the appropriate
        // value, not losing any of the data already
        // entered.
        // ---------------------------------------------
        switch(AsChar(import.ApPrompt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            ++local.Common.Count;
            ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

            break;
          default:
            var field = GetField(export.ApPrompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            ++local.Common.Count;

            break;
        }

        // CQ61889
        switch(AsChar(import.ArPrompt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            ++local.Common.Count;
            ExitState = "ECO_LNK_TO_CASE_ROLE_MAINTENANCE";

            break;
          default:
            var field = GetField(export.ArPrompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            ++local.Common.Count;

            break;
        }

        // 07/03/02 P.Phinney    WR020205  Add Place Of Birth Fields to Screen.
        // ---------------------------------------------
        switch(AsChar(import.PobStPrompt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            ++local.Common.Count;
            export.Prompt.CodeName = "STATE CODE";
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            break;
          default:
            var field = GetField(export.PobStPrompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            ++local.Common.Count;

            break;
        }

        switch(AsChar(import.PobFcPrompt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            ++local.Common.Count;
            export.Prompt.CodeName = "FIPS COUNTRY CODE";
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            break;
          default:
            var field = GetField(export.PobFcPrompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            ++local.Common.Count;

            break;
        }

        switch(AsChar(import.PhoneTypePrompt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            ++local.Common.Count;
            export.Prompt.CodeName = "OTHER PHONE TYPE";
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            break;
          default:
            var field = GetField(export.PhoneTypePrompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            ++local.Common.Count;

            break;
        }

        switch(AsChar(import.RacePrompt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            ++local.Common.Count;
            export.Prompt.CodeName = "RACE";
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            break;
          default:
            var field = GetField(export.RacePrompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            ++local.Common.Count;

            break;
        }

        // 12/23/2020  GVandy  CQ68785  Add customer service code.
        switch(AsChar(import.CustomerServicePrompt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            ++local.Common.Count;
            export.Prompt.CodeName = "CUSTOMER SERVICE INQUIRIES";
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            break;
          default:
            var field = GetField(export.CustomerServicePrompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            ++local.Common.Count;

            break;
        }

        // CQ64095
        switch(AsChar(import.TribalPrompt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            ++local.Common.Count;
            export.Prompt.CodeName = "TRIBAL NAME";
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            break;
          default:
            var field = GetField(export.TribalPrompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            ++local.Common.Count;

            break;
        }

        switch(local.Common.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

            break;
          case 1:
            // ****
            // for the cases where you link from 1 procedure to another 
            // procedure, you must set the export_hidden security link_indicator
            // to "L".
            // this will tell the called procedure that we are on a link and not
            // a transfer.  Don't forget to do the view matching on the dialog
            // design screen.
            break;
          default:
            if (AsChar(import.ApPrompt.SelectChar) == 'S')
            {
              var field = GetField(export.ApPrompt, "selectChar");

              field.Error = true;
            }

            // CQ61889
            if (AsChar(import.ArPrompt.SelectChar) == 'S')
            {
              var field = GetField(export.ArPrompt, "selectChar");

              field.Error = true;
            }

            // 07/03/02 P.Phinney    WR020205  Add Place Of Birth Fields to 
            // Screen.
            // ---------------------------------------------
            if (AsChar(export.PobStPrompt.SelectChar) == 'S')
            {
              var field = GetField(export.PobStPrompt, "selectChar");

              field.Error = true;
            }

            if (AsChar(export.PobFcPrompt.SelectChar) == 'S')
            {
              var field = GetField(export.PobFcPrompt, "selectChar");

              field.Error = true;
            }

            if (AsChar(import.RacePrompt.SelectChar) == 'S')
            {
              var field = GetField(export.RacePrompt, "selectChar");

              field.Error = true;
            }

            if (AsChar(import.CustomerServicePrompt.SelectChar) == 'S')
            {
              var field = GetField(export.CustomerServicePrompt, "selectChar");

              field.Error = true;
            }

            if (AsChar(import.PhoneTypePrompt.SelectChar) == 'S')
            {
              var field = GetField(export.PhoneTypePrompt, "selectChar");

              field.Error = true;
            }

            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            break;
        }

        break;
      case "UPDATE":
        // ---------------------------------------------
        // Verify that a display has been performed before the update can take 
        // place.
        // ---------------------------------------------
        if (!Equal(import.ArCsePersonsWorkSet.Number,
          export.HiddenPrevCsePersonsWorkSet.Number))
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";

          break;
        }

        // 11/04/2002 M.Lachowicz Start
        if (CharAt(export.ArCsePersonsWorkSet.Number, 10) == 'O')
        {
        }
        else
        {
          UseSiCheckName();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            var field1 = GetField(export.ArCsePersonsWorkSet, "firstName");

            field1.Error = true;

            var field2 = GetField(export.ArCsePersonsWorkSet, "lastName");

            field2.Error = true;

            var field3 = GetField(export.ArCsePersonsWorkSet, "middleInitial");

            field3.Error = true;

            ExitState = "SI0001_INVALID_NAME";
          }
        }

        // 11/04/2002 M.Lachowicz End
        // -----------------------------------------------------------------------------
        // PR# 180608 / CQ406   10/29/07   LSS
        // Moved the SET statements from COMMAND not equal to Display to inside 
        // update.
        // -----------------------------------------------------------------------------
        local.SsnConcat.Text8 = export.ArSsnWorkArea.SsnTextPart2 + export
          .ArSsnWorkArea.SsnTextPart3;
        export.ArCsePersonsWorkSet.Ssn = export.ArSsnWorkArea.SsnTextPart1 + local
          .SsnConcat.Text8;

        // PR# 180608 / CQ406   10/29/07   LSS   Added verify / ERROR statements
        // for SSN.
        if (Verify(export.ArCsePersonsWorkSet.Ssn, "0123456789") != 0 && !
          IsEmpty(export.ArCsePersonsWorkSet.Ssn))
        {
          var field1 = GetField(export.ArSsnWorkArea, "ssnTextPart1");

          field1.Error = true;

          var field2 = GetField(export.ArSsnWorkArea, "ssnTextPart2");

          field2.Error = true;

          var field3 = GetField(export.ArSsnWorkArea, "ssnTextPart3");

          field3.Error = true;

          ExitState = "LE0000_SSN_CONTAINS_NONNUM";
        }

        if (!IsEmpty(export.ArCsePersonsWorkSet.Ssn) && !
          Equal(export.ArCsePersonsWorkSet.Ssn, export.LastReadHidden.Ssn))
        {
          // added this check as part of cq7189.
          local.Convert.SsnNum9 =
            (int)StringToNumber(export.ArCsePersonsWorkSet.Ssn);

          if (ReadInvalidSsn())
          {
            var field1 = GetField(export.ArCsePersonsWorkSet, "number");

            field1.Error = true;

            var field2 = GetField(export.ArSsnWorkArea, "ssnTextPart1");

            field2.Error = true;

            var field3 = GetField(export.ArSsnWorkArea, "ssnTextPart2");

            field3.Error = true;

            var field4 = GetField(export.ArSsnWorkArea, "ssnTextPart3");

            field4.Error = true;

            ExitState = "INVALID_SSN";

            break;
          }
          else
          {
            // this is fine, there is not invalid ssn record for this 
            // combination of cse person number and ssn number
          }
        }

        // ---------------------------------------------
        // Validate the DOB
        // ---------------------------------------------
        if (Lt(Now().Date, import.ArCsePersonsWorkSet.Dob))
        {
          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "INVALID_DATE_OF_BIRTH";
          }

          var field = GetField(export.ArCsePersonsWorkSet, "dob");

          field.Error = true;
        }

        // 03/09/01 M.L Start
        if (Lt(Now().Date, export.ArCsePerson.DateOfDeath))
        {
          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "INVALID_DATE_OF_DEATH";
          }

          var field = GetField(export.ArCsePerson, "dateOfDeath");

          field.Error = true;
        }

        if (Lt(export.ArCsePerson.DateOfDeath, export.ArCsePersonsWorkSet.Dob) &&
          Lt(local.Zero.Date, export.ArCsePerson.DateOfDeath))
        {
          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "DOD_LESS_THAN_DOB";
          }

          var field1 = GetField(export.ArCsePersonsWorkSet, "dob");

          field1.Error = true;

          var field2 = GetField(export.ArCsePerson, "dateOfDeath");

          field2.Error = true;
        }

        // CQ65304
        if (Lt(local.Zero.Date, export.ArCsePerson.DateOfDeath) && !
          Lt(local.Zero.Date, export.HiddenCsePerson.DateOfDeath))
        {
          foreach(var item in ReadCsePersonAddress())
          {
            try
            {
              UpdateCsePersonAddress();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "CSE_PERSON_ADDRESS_NF";

                  break;
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

        // 03/09/01 M.L End
        // WR020205
        // --------------------------------------------
        // User can not enter both 'State'  and  'Foreign Country'.
        // --------------------------------------------
        if (!IsEmpty(export.ArCsePerson.BirthPlaceState) && !
          IsEmpty(export.ArCsePerson.BirthplaceCountry))
        {
          var field1 = GetField(export.ArCsePerson, "birthPlaceState");

          field1.Error = true;

          var field2 = GetField(export.ArCsePerson, "birthplaceCountry");

          field2.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_STATE_COUNTRY_ERROR";
          }
        }

        export.WorkForeignCountryDesc.Text40 = "";

        // --------------------------------------------
        // Validate the POB State
        // --------------------------------------------
        if (!IsEmpty(import.ArCsePerson.BirthPlaceState))
        {
          local.Code.CodeName = "STATE CODE";
          local.CodeValue.Cdvalue = import.ArCsePerson.BirthPlaceState ?? Spaces
            (10);

          // ---------------------------------------------
          // Call CAB to validate against the state table
          // --------------------------------------------
          UseCabValidateCodeValue1();

          if (AsChar(local.Common.Flag) == 'N')
          {
            var field = GetField(export.ArCsePerson, "birthPlaceState");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_INVALID_STATE_CODE";
            }
          }
        }

        // --  Validate Hurricane Katrina Indicator.
        if (AsChar(export.DisplacedPerson.DisplacedInterfaceInd) != 'Y')
        {
          switch(AsChar(export.DisplacedPerson.DisplacedInd))
          {
            case ' ':
              break;
            case 'Y':
              break;
            default:
              var field = GetField(export.DisplacedPerson, "displacedInd");

              field.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "INVALID_INDICATOR_Y_OR_SPACE";
              }

              break;
          }
        }

        // --------------------------------------------
        // Validate the POB Foreign Country.
        // --------------------------------------------
        if (!IsEmpty(import.ArCsePerson.BirthplaceCountry))
        {
          local.Code.CodeName = "FIPS COUNTRY CODE";
          local.CodeValue.Cdvalue = import.ArCsePerson.BirthplaceCountry ?? Spaces
            (10);

          // ---------------------------------------------
          // Call CAB to validate against the 'FIPS COUNTRY CODE' table
          // --------------------------------------------
          UseCabValidateCodeValue2();

          if (AsChar(local.Common.Flag) == 'N')
          {
            var field = GetField(export.ArCsePerson, "birthplaceCountry");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_INVALID_FORGN_COUNTRY";
            }
          }
          else
          {
            export.WorkForeignCountryDesc.Text40 =
              local.DisplayForeignCountry.Description;
          }
        }

        // ---------------------------------------------
        // Validate Sex
        // ---------------------------------------------
        // 02/22/01 M.L Do not allow space for sex code.
        if (AsChar(import.ArCsePersonsWorkSet.Sex) != 'F' && AsChar
          (import.ArCsePersonsWorkSet.Sex) != 'M')
        {
          // 02/22/01 M.L Start
          if (CharAt(export.ArCsePersonsWorkSet.Number, 10) == 'O')
          {
            goto Test2;
          }

          // 02/22/01 M.L End
          var field = GetField(export.ArCsePersonsWorkSet, "sex");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "INVALID_SEX";
          }
        }

Test2:

        // 07/30/99  M.L.  Add extra validation for sex.
        // CQ21791  Added check for role start and end dates in following reads
        if (AsChar(import.ArCsePersonsWorkSet.Sex) != 'F' && AsChar
          (import.HiddenArSex.Sex) == 'F')
        {
          if (ReadCaseRole2())
          {
            ExitState = "SI0000_FEMALE_SEX_CHANGE_NOT";

            var field = GetField(export.ArCsePersonsWorkSet, "sex");

            field.Error = true;
          }
        }

        if (AsChar(import.ArCsePersonsWorkSet.Sex) != 'M' && AsChar
          (import.HiddenArSex.Sex) == 'M')
        {
          if (ReadCaseRole1())
          {
            ExitState = "SI0000_MALE_SEX_CHANGE_NOT_ALLOW";

            var field = GetField(export.ArCsePersonsWorkSet, "sex");

            field.Error = true;
          }
        }

        // 07/30/99  M.L.  End
        // -- Validate Prior TAF Indicator (Y or spaces).
        switch(AsChar(export.ArCsePerson.PriorTafInd))
        {
          case ' ':
            break;
          case 'Y':
            break;
          default:
            var field = GetField(export.ArCsePerson, "priorTafInd");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "INVALID_INDICATOR_Y_OR_SPACE";
            }

            break;
        }

        // 08/04/99 M.L Add validation for area code.
        if (export.ArCsePerson.HomePhone.GetValueOrDefault() != 0 && export
          .ArCsePerson.HomePhoneAreaCode.GetValueOrDefault() == 0)
        {
          var field = GetField(export.ArCsePerson, "homePhoneAreaCode");

          field.Error = true;

          ExitState = "OE0000_PHONE_AREA_REQD";
        }

        if (export.ArCsePerson.EmergencyPhone.GetValueOrDefault() != 0 && export
          .ArCsePerson.EmergencyAreaCode.GetValueOrDefault() == 0)
        {
          var field = GetField(export.ArCsePerson, "emergencyAreaCode");

          field.Error = true;

          ExitState = "OE0000_PHONE_AREA_REQD";
        }

        if (export.ArCsePerson.OtherNumber.GetValueOrDefault() != 0 && export
          .ArCsePerson.OtherAreaCode.GetValueOrDefault() == 0)
        {
          var field = GetField(export.ArCsePerson, "otherAreaCode");

          field.Error = true;

          ExitState = "OE0000_PHONE_AREA_REQD";
        }

        if (export.ArCsePerson.WorkPhone.GetValueOrDefault() != 0 && export
          .ArCsePerson.WorkPhoneAreaCode.GetValueOrDefault() == 0)
        {
          var field = GetField(export.ArCsePerson, "workPhoneAreaCode");

          field.Error = true;

          ExitState = "OE0000_PHONE_AREA_REQD";
        }

        // 08/04/99 M.L End
        if (!IsEmpty(import.ArCsePerson.TextMessageIndicator))
        {
          if (AsChar(import.ArCsePerson.TextMessageIndicator) != 'Y' && AsChar
            (import.ArCsePerson.TextMessageIndicator) != 'N')
          {
            var field = GetField(export.ArCsePerson, "textMessageIndicator");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "INVALID_TEXT_MESSAGE_IND";
            }
          }
        }

        if (IsEmpty(import.ArCsePerson.TextMessageIndicator))
        {
          if (!IsEmpty(import.HiddenCsePerson.TextMessageIndicator))
          {
            var field = GetField(export.ArCsePerson, "textMessageIndicator");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "INVALID_TEXT_MESSAGE_IND";
            }
          }
        }

        // CQ66290 Validate threat on staff
        if (!IsEmpty(import.ArCsePerson.ThreatOnStaff))
        {
          if (AsChar(import.ArCsePerson.ThreatOnStaff) != 'Y' && AsChar
            (import.ArCsePerson.ThreatOnStaff) != 'N')
          {
            var field = GetField(export.ArCsePerson, "threatOnStaff");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "INVALID_THREAT_IND";
            }
          }
        }

        // ---------------------------------------------
        // Validate Other Phone Type
        // ---------------------------------------------
        if (!IsEmpty(import.PhoneTypePrompt.SelectChar))
        {
          local.Code.CodeName = "OTHER PHONE TYPE";
          local.CodeValue.Cdvalue = import.ArCsePerson.OtherPhoneType ?? Spaces
            (10);

          // ---------------------------------------------
          // Call CAB to validate against the RACE table
          // ---------------------------------------------
          UseCabValidateCodeValue1();

          if (AsChar(local.Common.Flag) == 'N')
          {
            var field = GetField(export.ArCsePerson, "otherPhoneType");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "INVALID_OTHER_PHONE_TYPE";
            }
          }
        }

        // CQ64095 Validate tribal name
        if (!IsEmpty(import.ArCsePerson.TribalCode))
        {
          local.Code.CodeName = "TRIBAL NAME";
          local.CodeValue.Cdvalue = import.ArCsePerson.TribalCode ?? Spaces(10);

          // ---------------------------------------------
          // Call CAB to validate against the RACE table
          // ---------------------------------------------
          UseCabValidateCodeValue1();

          if (AsChar(local.Common.Flag) == 'N')
          {
            var field = GetField(export.ArCsePerson, "tribalCode");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_INVALID_TRIBAL_NAME";
            }
          }
        }

        if (!IsEmpty(export.ArCsePerson.TribalCode))
        {
          export.TribalFlag.Flag = "Y";
        }
        else
        {
          export.TribalFlag.Flag = "N";
        }

        // 12/23/2020  GVandy  CQ68785  Add customer service code.
        if (!IsEmpty(import.ArCsePerson.CustomerServiceCode))
        {
          local.Code.CodeName = "CUSTOMER SERVICE INQUIRIES";
          local.CodeValue.Cdvalue = import.ArCsePerson.CustomerServiceCode ?? Spaces
            (10);

          // ---------------------------------------------
          // Call CAB to validate against the RACE table
          // ---------------------------------------------
          UseCabValidateCodeValue1();

          if (AsChar(local.Common.Flag) == 'N')
          {
            var field = GetField(export.ArCsePerson, "customerServiceCode");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "SI0000_INVALID_CUST_SERV_CD";
            }
          }
        }

        switch(AsChar(import.ArCsePerson.CustomerServiceCode))
        {
          case 'E':
            if (IsEmpty(export.CsePersonEmailAddress.EmailAddress))
            {
              var field = GetField(export.ArCsePerson, "customerServiceCode");

              field.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "SI0000_MUST_HAVE_EMAIL_ADDRESS";
              }
            }

            break;
          case 'T':
            if (AsChar(export.ArCsePerson.OtherPhoneType) != 'C' || export
              .ArCsePerson.OtherNumber.GetValueOrDefault() == 0)
            {
              var field = GetField(export.ArCsePerson, "customerServiceCode");

              field.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                if (IsEmpty(import.HiddenCsePerson.CustomerServiceCode))
                {
                  ExitState = "SI0000_MUST_HAVE_CELL_PHONE";
                }
                else
                {
                  ExitState = "SI0000_CHANGE_CUSTOMER_SERVICE";
                }
              }
            }

            break;
          default:
            break;
        }

        // ---------------------------------------------
        // Validate Race
        // ---------------------------------------------
        if (!IsEmpty(import.ArCsePerson.Race))
        {
          local.Code.CodeName = "RACE";
          local.CodeValue.Cdvalue = import.ArCsePerson.Race ?? Spaces(10);

          // ---------------------------------------------
          // Call CAB to validate against the RACE table
          // ---------------------------------------------
          UseCabValidateCodeValue1();

          if (AsChar(local.Common.Flag) == 'N')
          {
            var field = GetField(export.ArCsePerson, "race");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "INVALID_RACE";
            }
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        local.ArCsePerson.Assign(import.ArCsePerson);
        local.ArCsePerson.Number = import.ArCsePersonsWorkSet.Number;

        // -----------------------------------------------------------------------------
        // PR# 180608 / CQ406   10/29/07   LSS
        // Moved this SET statement from COMMAND not equal to Display to inside 
        // update after the validations
        // -----------------------------------------------------------------------------
        local.ArCsePerson.TaxId = export.ArCsePersonsWorkSet.Ssn;
        UseSiUpdateCsePerson();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          // ------------------------------------------------------------
          // 02/08/99 W.Campbell - Added logic to USE
          // EAB_ROLLBACK_CICS to help ensure
          // correct rollback of DB/2 updates.
          // ------------------------------------------------------------
          UseEabRollbackCics();

          break;
        }

        // -- Update Prior TAF Indicator.
        // This update was not added to the si_update_cse_person cab because all
        // other psteps and cabs which call
        // that action block would have required modification to maintain this 
        // indicator as well.
        if (AsChar(export.ArCsePerson.Type1) == 'C')
        {
          if (ReadCsePerson())
          {
            if (AsChar(entities.Ar.PriorTafInd) != AsChar
              (export.ArCsePerson.PriorTafInd))
            {
              try
              {
                UpdateCsePerson();
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "CSE_PERSON_NU";

                    break;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "CSE_PERSON_PV";

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
            ExitState = "CSE_PERSON_NF";
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabRollbackCics();

            break;
          }
        }

        // ---------------------------------------------
        // Update details of the Case Role
        // ---------------------------------------------
        export.ArCaseRole.Type1 = "AR";
        UseSiUpdateCaseRole();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // ------------------------------------------------------------
          // 02/08/99 W.Campbell - The logic for
          // update of ADABAS was rearranged so that
          // it would occur after all other DB/2 updates
          // in case a ROLLBACK was needed for DB/2
          // since ADABAS does not have
          // rollback capability.
          // ------------------------------------------------------------
          // ---------------------------------------------
          // Update adabas details
          // --------------------------------------------
          if (AsChar(import.HiddenAe.Flag) != 'O')
          {
            UseCabUpdateAdabasPerson();

            if (!IsEmpty(local.AbendData.Type1))
            {
              // ------------------------------------------------------------
              // 02/08/99 W.Campbell - Added logic to USE
              // EAB_ROLLBACK_CICS to help ensure
              // correct rollback of DB/2 updates.
              // ------------------------------------------------------------
              UseEabRollbackCics();

              return;
            }
          }
        }

        // 09/09/05  GVandy  WR00256682  Add indicator to screen to indicate if 
        // individual was displaced by Hurricane Katrina.
        if (AsChar(export.DisplacedPerson.DisplacedInterfaceInd) != 'Y')
        {
          if (ReadDisplacedPerson1())
          {
            if (AsChar(export.DisplacedPerson.DisplacedInd) != 'Y')
            {
              try
              {
                UpdateDisplacedPerson1();
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "DISPLACED_PERSON_NU";

                    break;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "DISPLACED_PERSON_PV";

                    break;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }
            }
          }
          else if (AsChar(export.DisplacedPerson.DisplacedInd) == 'Y')
          {
            local.MaxDate.Date = new DateTime(2099, 12, 31);

            try
            {
              CreateDisplacedPerson();
              MoveDisplacedPerson(entities.DisplacedPerson,
                export.DisplacedPerson);
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  if (ReadDisplacedPerson2())
                  {
                    try
                    {
                      UpdateDisplacedPerson2();
                    }
                    catch(Exception e1)
                    {
                      switch(GetErrorCode(e1))
                      {
                        case ErrorCode.AlreadyExists:
                          ExitState = "DISPLACED_PERSON_NU";

                          break;
                        case ErrorCode.PermittedValueViolation:
                          ExitState = "DISPLACED_PERSON_PV";

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
                    ExitState = "DISPLACED_PERSON_NF";
                  }

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "DISPLACED_PERSON_PV";

                  break;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // 07/30/99 Start
          export.HiddenArSex.Sex = export.ArCsePersonsWorkSet.Sex;

          // 07/30/99 End
          export.HiddenCsePerson.CustomerServiceCode =
            export.ArCsePerson.CustomerServiceCode ?? "";
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

          break;
        }
        else
        {
          // ------------------------------------------------------------
          // 02/08/99 W.Campbell - Added logic to USE
          // EAB_ROLLBACK_CICS to help ensure
          // correct rollback of DB/2 updates.
          // ------------------------------------------------------------
          UseEabRollbackCics();
        }

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        return;
      case "SIGNOFF":
        // --------------------------------------------
        // Sign the user off the Kessep system
        // --------------------------------------------
        UseScCabSignoff();

        return;
      case "DISPLAY":
        // WR020205
        export.WorkForeignCountryDesc.Text40 = "";

        // ---------------------------------------------
        // 02/04/99 W.Campbell - Code added to create
        // infrastructure record(s) for change(s) to SSN.
        // ---------------------------------------------
        export.LastReadHidden.Ssn = "";
        export.ArSsnWorkArea.SsnTextPart1 = "";
        export.ArSsnWorkArea.SsnTextPart2 = "";
        export.ArSsnWorkArea.SsnTextPart3 = "";

        // ---------------------------------------------
        // 02/04/99 W.Campbell - End of Code added to create
        // infrastructure record(s) for change(s) to SSN.
        // ---------------------------------------------
        // CQ61889
        export.ArPrompt.SelectChar = "";
        export.ApPrompt.SelectChar = "";
        export.PhoneTypePrompt.SelectChar = "";
        export.RacePrompt.SelectChar = "";
        export.Alt.Text13 = "";
        export.CsePersonEmailAddress.EmailAddress =
          Spaces(CsePersonEmailAddress.EmailAddress_MaxLength);

        // CQ64095
        export.TribalPrompt.SelectChar = "";
        export.CustomerServicePrompt.SelectChar = "";

        // ---------------------------------------------
        // 10/19/99 JF.Caillouet - Moved this Left Zero Fill CAB  up higher so 
        // it can be used sooner (Ex. Security Cab.)
        // ---------------------------------------------
        // ---------------------------------------------
        // Call the action blocks that read the data required for this screen.
        // ---------------------------------------------
        UseSiReadCaseHeaderInformation();

        // CQ61889
        if (!IsEmpty(import.SelectedAr.Number) && !
          Equal(import.SelectedAr.Number, export.ArCsePersonsWorkSet.Number) &&
          Equal(import.HiddenPrevCase.Number, import.Next.Number))
        {
          UseCabReadAdabasPerson();
        }

        // 09/09/05  GVandy  WR00256682  Add indicator to screen to indicate if 
        // individual was displaced by Hurricane Katrina.
        if (ReadDisplacedPerson1())
        {
          MoveDisplacedPerson(entities.DisplacedPerson, export.DisplacedPerson);
        }
        else
        {
          export.DisplacedPerson.DisplacedInd = "";
          export.DisplacedPerson.DisplacedInterfaceInd = "";
        }

        if (AsChar(export.DisplacedPerson.DisplacedInterfaceInd) == 'Y')
        {
          var field = GetField(export.DisplacedPerson, "displacedInd");

          field.Color = "cyan";
          field.Highlighting = Highlighting.Normal;
          field.Protected = true;
        }
        else
        {
          var field = GetField(export.DisplacedPerson, "displacedInd");

          field.Color = "green";
          field.Highlighting = Highlighting.Underscore;
          field.Protected = false;
        }

        MoveSsnWorkArea(export.ArSsnWorkArea, local.SsnWorkArea);
        local.SsnWorkArea.ConvertOption = "2";
        UseCabSsnConvertNumToText();
        export.ArSsnWorkArea.SsnText9 = export.ArCsePersonsWorkSet.Ssn;

        if (export.ArSsnWorkArea.SsnNumPart1 == 0)
        {
        }
        else
        {
          export.ArSsnWorkArea.SsnTextPart1 =
            Substring(export.ArSsnWorkArea.SsnText9, 1, 3);
        }

        if (export.ArSsnWorkArea.SsnNumPart2 == 0)
        {
        }
        else
        {
          export.ArSsnWorkArea.SsnTextPart2 =
            Substring(export.ArSsnWorkArea.SsnText9, 4, 2);
        }

        if (export.ArSsnWorkArea.SsnNumPart3 == 0)
        {
        }
        else
        {
          export.ArSsnWorkArea.SsnTextPart3 =
            Substring(export.ArSsnWorkArea.SsnText9, 6, 4);
        }

        if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState("NO_APS_ON_A_CASE"))
        {
        }
        else
        {
          break;
        }

        if (IsEmpty(local.AbendData.Type1))
        {
          // *****
          // Read CSE person to determine whether to protect the ar data
          // *****
          UseSiReadCsePerson();

          if (AsChar(export.HiddenAe.Flag) == 'O')
          {
            var field1 = GetField(export.ArCsePersonsWorkSet, "number");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.ArCsePersonsWorkSet, "dob");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.ArCsePersonsWorkSet, "firstName");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.ArCsePersonsWorkSet, "lastName");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.ArCsePersonsWorkSet, "middleInitial");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 = GetField(export.ArCsePersonsWorkSet, "sex");

            field6.Color = "cyan";
            field6.Protected = true;

            var field7 = GetField(export.ArSsnWorkArea, "ssnNumPart1");

            field7.Color = "cyan";
            field7.Protected = true;

            var field8 = GetField(export.ArSsnWorkArea, "ssnNumPart2");

            field8.Color = "cyan";
            field8.Protected = true;

            var field9 = GetField(export.ArSsnWorkArea, "ssnNumPart3");

            field9.Color = "cyan";
            field9.Protected = true;

            var field10 = GetField(export.ArSsnWorkArea, "ssnTextPart1");

            field10.Color = "cyan";
            field10.Protected = true;

            var field11 = GetField(export.ArSsnWorkArea, "ssnTextPart2");

            field11.Color = "cyan";
            field11.Protected = true;

            var field12 = GetField(export.ArSsnWorkArea, "ssnTextPart3");

            field12.Color = "cyan";
            field12.Protected = true;

            // cq640195
            var field13 = GetField(export.ArCsePerson, "tribalCode");

            field13.Color = "cyan";
            field13.Protected = true;
          }

          if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
            ("NO_APS_ON_A_CASE"))
          {
            if (!IsEmpty(export.ArCsePersonsWorkSet.Ssn))
            {
              local.SsnWorkArea.SsnText9 = export.ArCsePersonsWorkSet.Ssn;
              UseCabSsnConvertTextToNum();

              if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
                ("NO_APS_ON_A_CASE"))
              {
              }
              else
              {
                export.ArSsnWorkArea.SsnNumPart1 = 0;
                export.ArSsnWorkArea.SsnNumPart2 = 0;
                export.ArSsnWorkArea.SsnNumPart3 = 0;

                var field1 = GetField(export.ArSsnWorkArea, "ssnNumPart3");

                field1.Error = true;

                var field2 = GetField(export.ArSsnWorkArea, "ssnNumPart2");

                field2.Error = true;

                var field3 = GetField(export.ArSsnWorkArea, "ssnNumPart1");

                field3.Error = true;

                var field4 = GetField(export.ArSsnWorkArea, "ssnTextPart3");

                field4.Error = true;

                var field5 = GetField(export.ArSsnWorkArea, "ssnTextPart2");

                field5.Error = true;

                var field6 = GetField(export.ArSsnWorkArea, "ssnTextPart1");

                field6.Error = true;
              }
            }
            else
            {
              export.ArSsnWorkArea.SsnText9 = "";
              export.ArSsnWorkArea.SsnNumPart1 = 0;
              export.ArSsnWorkArea.SsnNumPart2 = 0;
              export.ArSsnWorkArea.SsnNumPart3 = 0;
              export.ArSsnWorkArea.SsnTextPart1 = "";
              export.ArSsnWorkArea.SsnTextPart2 = "";
              export.ArSsnWorkArea.SsnTextPart3 = "";
            }

            // PR160844. Load the text portion of ssn parts so that ssn displays
            // on the screen.
            if (export.ArSsnWorkArea.SsnNumPart1 == 0 && export
              .ArSsnWorkArea.SsnNumPart2 == 0 && export
              .ArSsnWorkArea.SsnNumPart3 == 0)
            {
              export.ArSsnWorkArea.SsnTextPart1 = "";
              export.ArSsnWorkArea.SsnTextPart2 = "";
              export.ArSsnWorkArea.SsnTextPart3 = "";
            }

            if (IsEmpty(export.ArSsnWorkArea.SsnTextPart1) && IsEmpty
              (export.ArSsnWorkArea.SsnTextPart2) && IsEmpty
              (export.ArSsnWorkArea.SsnTextPart3))
            {
              MoveSsnWorkArea(export.ArSsnWorkArea, local.SsnWorkArea);
              local.SsnWorkArea.ConvertOption = "2";
              UseCabSsnConvertNumToText();
              export.ArCsePersonsWorkSet.Ssn = export.ArSsnWorkArea.SsnText9;
              export.ArSsnWorkArea.SsnText9 = export.ArCsePersonsWorkSet.Ssn;

              if (export.ArSsnWorkArea.SsnNumPart1 == 0)
              {
                export.ArSsnWorkArea.SsnTextPart1 = "";
              }
              else
              {
                export.ArSsnWorkArea.SsnTextPart1 =
                  Substring(export.ArSsnWorkArea.SsnText9, 1, 3);
              }

              if (export.ArSsnWorkArea.SsnNumPart2 == 0)
              {
                export.ArSsnWorkArea.SsnTextPart2 = "";
              }
              else
              {
                export.ArSsnWorkArea.SsnTextPart2 =
                  Substring(export.ArSsnWorkArea.SsnText9, 4, 2);
              }

              if (export.ArSsnWorkArea.SsnNumPart3 == 0)
              {
                export.ArSsnWorkArea.SsnTextPart3 = "";
              }
              else
              {
                export.ArSsnWorkArea.SsnTextPart3 =
                  Substring(export.ArSsnWorkArea.SsnText9, 6, 4);
              }
            }
          }
          else
          {
          }
        }
        else
        {
          return;
        }

        if (AsChar(local.MultipleAps.Flag) == 'Y')
        {
          // -----------------------------------------------
          // 05/03/99 W.Campbell - Added code to send
          // selection needed msg to COMP.  BE SURE
          // TO MATCH next_tran_info ON THE DIALOG
          // FLOW.
          // -----------------------------------------------
          export.HiddenNextTranInfo.MiscText1 = "SELECTION NEEDED";
          ExitState = "ECO_LNK_TO_CASE_COMPOSITION";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (IsExitState("NO_APS_ON_A_CASE"))
          {
            local.ApExists.Flag = "N";
            ExitState = "ACO_NN0000_ALL_OK";
          }
          else
          {
            break;
          }
        }

        UseSiReadOfficeOspHeader();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        local.AddressType.Flag = "M";
        UseCabReadAdabasAddress();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        if (!IsEmpty(local.AeMailing.Street1))
        {
          MoveCsePersonAddress3(local.AeMailing, export.CsePersonAddress);
        }
        else
        {
          // ------------------------------------------------------
          // 01/14/99 W.Campbell - Following code disabled in order
          // to use new logic to obtain the CSE_PERSON_ADDRESS.
          // ------------------------------------------------------
          // -------------------------------------------------
          // 02/23/99 W.Campbell - Deleted a disabled
          // USE statement for
          // ZDEL_SI_READ_CSE_PERSON_ADDRESS
          // -------------------------------------------------
          // ------------------------------------------------------
          // 01/14/99 W.Campbell - Following code inserted
          // to use new logic to obtain the CSE_PERSON_ADDRESS.
          // ------------------------------------------------------
          // ------------------------------------------------------
          // 02/03/99 W.Campbell - Inserted a
          // statement to set local_ar cse_person
          // number to the export_ar
          // cse_person_work_set number,
          // so that the correct data will get passed.
          // ------------------------------------------------------
          local.ArCsePerson.Number = export.ArCsePersonsWorkSet.Number;
          UseSiGetCsePersonMailingAddr();

          if (!IsEmpty(export.CsePersonAddress.LocationType))
          {
            // ------------------------------------------------------
            // An address was returned.  However, we
            // only want a 'D'omestic, 'M'ailing address.
            // Anything else ('F'oreign or 'R'esidential),
            // then blank out the export address.
            // ------------------------------------------------------
            if (AsChar(export.CsePersonAddress.LocationType) == 'D' && AsChar
              (export.CsePersonAddress.Type1) == 'M')
            {
              // ------------------------------------------------------
              // All is good, keep on going.
              // ------------------------------------------------------
            }
            else
            {
              // ------------------------------------------------------
              // Blank out the export address.
              // ------------------------------------------------------
              MoveCsePersonAddress2(local.Blank, export.CsePersonAddress);
            }
          }

          // ------------------------------------------------------
          // 01/14/99 W.Campbell - End of code inserted
          // to use new logic to obtain the CSE_PERSON_ADDRESS.
          // ------------------------------------------------------
        }

        UseSiArdsReadArCaseRoleDetail();

        if (AsChar(export.HiddenAe.Flag) == 'O')
        {
          var field1 = GetField(export.ArCsePersonsWorkSet, "number");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.ArCsePersonsWorkSet, "dob");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.ArCsePersonsWorkSet, "firstName");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.ArCsePersonsWorkSet, "lastName");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 = GetField(export.ArCsePersonsWorkSet, "middleInitial");

          field5.Color = "cyan";
          field5.Protected = true;

          var field6 = GetField(export.ArCsePersonsWorkSet, "sex");

          field6.Color = "cyan";
          field6.Protected = true;

          var field7 = GetField(export.ArSsnWorkArea, "ssnNumPart1");

          field7.Color = "cyan";
          field7.Protected = true;

          var field8 = GetField(export.ArSsnWorkArea, "ssnNumPart2");

          field8.Color = "cyan";
          field8.Protected = true;

          var field9 = GetField(export.ArSsnWorkArea, "ssnNumPart3");

          field9.Color = "cyan";
          field9.Protected = true;

          var field10 = GetField(export.ArSsnWorkArea, "ssnTextPart1");

          field10.Color = "cyan";
          field10.Protected = true;

          var field11 = GetField(export.ArSsnWorkArea, "ssnTextPart2");

          field11.Color = "cyan";
          field11.Protected = true;

          var field12 = GetField(export.ArSsnWorkArea, "ssnTextPart3");

          field12.Color = "cyan";
          field12.Protected = true;
        }

        if (IsEmpty(local.AbendData.Type1))
        {
          if (CharAt(export.ArCsePersonsWorkSet.Number, 10) == 'O')
          {
            var field1 = GetField(export.ArCsePersonsWorkSet, "number");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.ArCsePersonsWorkSet, "dob");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.ArCsePersonsWorkSet, "firstName");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.ArCsePersonsWorkSet, "lastName");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.ArCsePersonsWorkSet, "middleInitial");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 = GetField(export.ArCsePersonsWorkSet, "sex");

            field6.Color = "cyan";
            field6.Protected = true;

            var field7 = GetField(export.ArSsnWorkArea, "ssnNumPart1");

            field7.Color = "cyan";
            field7.Protected = true;

            var field8 = GetField(export.ArSsnWorkArea, "ssnNumPart2");

            field8.Color = "cyan";
            field8.Protected = true;

            var field9 = GetField(export.ArSsnWorkArea, "ssnNumPart3");

            field9.Color = "cyan";
            field9.Protected = true;

            var field10 = GetField(export.ArSsnWorkArea, "ssnTextPart1");

            field10.Color = "cyan";
            field10.Protected = true;

            var field11 = GetField(export.ArSsnWorkArea, "ssnTextPart2");

            field11.Color = "cyan";
            field11.Protected = true;

            var field12 = GetField(export.ArSsnWorkArea, "ssnTextPart3");

            field12.Color = "cyan";
            field12.Protected = true;

            var field13 = GetField(export.ArCsePerson, "race");

            field13.Color = "cyan";
            field13.Protected = true;

            var field14 = GetField(export.RacePrompt, "selectChar");

            field14.Color = "cyan";
            field14.Protected = true;

            var field15 =
              GetField(export.ArCsePerson, "currentSpouseFirstName");

            field15.Color = "cyan";
            field15.Protected = true;

            var field16 = GetField(export.ArCsePerson, "currentSpouseLastName");

            field16.Color = "cyan";
            field16.Protected = true;

            var field17 = GetField(export.ArCsePerson, "currentSpouseMi");

            field17.Color = "cyan";
            field17.Protected = true;

            var field18 = GetField(export.ArCsePerson, "emergencyAreaCode");

            field18.Color = "cyan";
            field18.Protected = true;

            var field19 = GetField(export.ArCsePerson, "emergencyPhone");

            field19.Color = "cyan";
            field19.Protected = true;

            var field20 = GetField(export.ArCsePerson, "homePhone");

            field20.Color = "cyan";
            field20.Protected = true;

            var field21 = GetField(export.ArCsePerson, "homePhoneAreaCode");

            field21.Color = "cyan";
            field21.Protected = true;

            var field22 = GetField(export.ArCsePerson, "otherAreaCode");

            field22.Color = "cyan";
            field22.Protected = true;

            var field23 = GetField(export.ArCsePerson, "otherNumber");

            field23.Color = "cyan";
            field23.Protected = true;

            var field24 = GetField(export.ArCsePerson, "nameMaiden");

            field24.Color = "cyan";
            field24.Protected = true;

            var field25 = GetField(export.ArCsePerson, "priorTafInd");

            field25.Color = "cyan";
            field25.Protected = true;

            var field26 = GetField(export.ArCsePerson, "textMessageIndicator");

            field26.Color = "cyan";
            field26.Protected = true;

            var field27 = GetField(export.ArCsePerson, "customerServiceCode");

            field27.Color = "cyan";
            field27.Protected = true;

            // CQ64095
            var field28 = GetField(export.ArCsePerson, "tribalCode");

            field28.Color = "cyan";
            field28.Protected = true;
          }

          if (ReadCsePersonEmailAddress())
          {
            export.CsePersonEmailAddress.EmailAddress =
              entities.CsePersonEmailAddress.EmailAddress;
          }
        }
        else
        {
          return;
        }

        if (CharAt(export.ArCsePersonsWorkSet.Number, 10) == 'O')
        {
        }
        else
        {
          UseSiAltsBuildAliasAndSsn();
        }

        if (AsChar(local.ArOccur.Flag) == 'Y')
        {
          export.Alt.Text13 = "Alt SSN/Alias";
        }

        // WR020205
        // WR020205
        if (!IsEmpty(export.ArCsePerson.BirthplaceCountry))
        {
          local.Code.CodeName = "FIPS COUNTRY CODE";
          local.CodeValue.Cdvalue = export.ArCsePerson.BirthplaceCountry ?? Spaces
            (10);

          // ---------------------------------------------
          // Call CAB to validate against the 'FIPS COUNTRY CODE' table
          // --------------------------------------------
          UseCabValidateCodeValue3();
          export.WorkForeignCountryDesc.Text40 =
            local.DisplayForeignCountry.Description;
        }

        // ---------------------------------------------
        // 02/04/99 W.Campbell - Code added to create
        // infrastructure record(s) for change(s) to SSN.
        // ---------------------------------------------
        export.LastReadHidden.Ssn = export.ArCsePersonsWorkSet.Ssn;

        // ---------------------------------------------
        // 02/04/99 W.Campbell - End of Code added to create
        // infrastructure record(s) for change(s) to SSN.
        // ---------------------------------------------
        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // ----------------------------------------------------------------------
          // Now call the Family Violence CAB and pass the data to the CAB to 
          // check if the AR has  family violence Flag set.
          //                                                
          // Vithal(08/27/2001)
          // ----------------------------------------------------------------------
          UseScSecurityCheckForFv();

          if (IsExitState("SC0000_DATA_NOT_DISPLAY_FOR_FV"))
          {
            MoveSsnWorkArea(local.ArSsnWorkArea, export.ArSsnWorkArea);
            export.ArCsePersonsWorkSet.Dob = local.Zero.Date;
            export.ArCsePerson.DateOfDeath = local.Zero.Date;
            export.ArCsePerson.NameMaiden = "";
            export.ArCsePersonsWorkSet.Sex = "";
            export.ArCsePerson.Race = "";
            export.ArCsePerson.CurrentSpouseLastName = "";
            export.ArCsePerson.CurrentSpouseFirstName = "";
            export.ArCsePerson.CurrentSpouseMi = "";
            export.ArCsePerson.AeCaseNumber = "";
            export.Case1.AdcOpenDate = local.Zero.Date;
            export.Case1.AdcCloseDate = local.Zero.Date;
            export.ArCsePerson.KscaresNumber = "";
            MoveCsePersonAddress2(local.Blank, export.CsePersonAddress);
            export.ArCsePerson.HomePhoneAreaCode = 0;
            export.ArCsePerson.HomePhone = 0;
            export.ArCsePerson.EmergencyAreaCode = 0;
            export.ArCsePerson.EmergencyPhone = 0;
            export.ArCsePerson.OtherAreaCode = 0;
            export.ArCsePerson.OtherNumber = 0;
            export.ArCsePerson.OtherPhoneType = "";
            export.ArCsePerson.WorkPhoneAreaCode = 0;
            export.ArCsePerson.WorkPhone = 0;
            export.ArCsePerson.WorkPhoneExt = "";
            export.ArCsePerson.OrganizationName = "";
            export.ArCsePerson.TaxId = "";
            export.CsePersonEmailAddress.EmailAddress =
              Spaces(CsePersonEmailAddress.EmailAddress_MaxLength);
            export.ArCaseRole.Note = Spaces(CaseRole.Note_MaxLength);

            // CQ64095
            export.ArCsePerson.TribalCode = "";
            export.ArCsePerson.CustomerServiceCode = "";

            break;
          }

          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";

          if (AsChar(local.ApExists.Flag) == 'N')
          {
            ExitState = "NO_APS_ON_A_CASE";
          }

          if (AsChar(export.CaseOpen.Flag) == 'N')
          {
            ExitState = "DISPLAY_OK_FOR_CLOSED_CASE";
          }

          if (IsExitState("ACO_NI0000_SUCCESSFUL_DISPLAY"))
          {
            export.HiddenCsePerson.TextMessageIndicator =
              export.ArCsePerson.TextMessageIndicator ?? "";
          }
        }

        // cq64095
        if (!IsEmpty(export.ArCsePerson.TribalCode))
        {
          export.TribalFlag.Flag = "Y";
        }
        else
        {
          export.TribalFlag.Flag = "N";
        }

        // 07/30/99 Start
        export.HiddenArSex.Sex = export.ArCsePersonsWorkSet.Sex;

        // 07/30/99 End
        // CQ65304
        export.HiddenCsePerson.DateOfDeath = export.ArCsePerson.DateOfDeath;

        // CQ66290
        export.HiddenCsePerson.ThreatOnStaff =
          export.ArCsePerson.ThreatOnStaff ?? "";
        export.HiddenCsePerson.CustomerServiceCode =
          export.ArCsePerson.CustomerServiceCode ?? "";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

    // ---------------------------------------------
    // 02/04/99 W.Campbell - Code added to create
    // infrastructure record(s) for change(s) to SSN.
    // The oe cab raise event will be called from
    // here as an extention of case of update.
    // ---------------------------------------------
    if (IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE"))
    {
      local.Infrastructure.UserId = "APDS";
      local.Infrastructure.BusinessObjectCd = "CAU";
      local.Infrastructure.ReferenceDate = local.Zero.Date;

      for(local.NumberOfEvents.TotalInteger = 1; local
        .NumberOfEvents.TotalInteger <= 3; ++local.NumberOfEvents.TotalInteger)
      {
        local.RaiseEventFlag.Text1 = "N";

        if (local.NumberOfEvents.TotalInteger == 1)
        {
          // ---------------------------------------------
          // 02/11/99 W.Campbell - IF stmt copied and
          // disabled just to keep it in case it needs to
          // be re-enabled again.
          // ---------------------------------------------
          if (Lt("000000000", import.LastReadHidden.Ssn) && !
            Equal(export.ArCsePersonsWorkSet.Ssn, export.LastReadHidden.Ssn))
          {
            local.RaiseEventFlag.Text1 = "Y";
            local.Infrastructure.EventId = 46;
            local.Infrastructure.ReasonCode = "ARSSNIDCHNG";
            local.DetailText30.Text30 = "AP's SSN Changed to :";
            local.Infrastructure.Detail = local.DetailText30.Text30;

            // ---------------------------------------------
            // 02/11/99 W.Campbell - IF stmt inserted to
            // log 'Unknown' when the SSN is changed
            // from non-blanks(zeros) to blanks (zeros).
            // ---------------------------------------------
            if (!Lt("000000000", export.ArCsePersonsWorkSet.Ssn))
            {
              local.DetailText30.Text30 = "Unknown";
            }
            else
            {
              local.DetailText10.Text10 =
                Substring(export.ArCsePersonsWorkSet.Ssn, 1, 3);
              local.DetailText10.Text10 =
                Substring(local.DetailText10.Text10,
                TextWorkArea.Text10_MaxLength, 1, 4) + Substring
                (export.ArCsePersonsWorkSet.Ssn,
                CsePersonsWorkSet.Ssn_MaxLength, 4, 2);
              local.DetailText30.Text30 =
                Substring(local.DetailText10.Text10,
                TextWorkArea.Text10_MaxLength, 1, 7) + Substring
                (export.ArCsePersonsWorkSet.Ssn,
                CsePersonsWorkSet.Ssn_MaxLength, 6, 4);
            }

            local.Infrastructure.Detail =
              TrimEnd(local.Infrastructure.Detail) + local.DetailText30.Text30;
            local.DetailText10.Text10 = " From :";
            local.Infrastructure.Detail =
              TrimEnd(local.Infrastructure.Detail) + local.DetailText10.Text10;
            local.DetailText10.Text10 =
              Substring(export.LastReadHidden.Ssn, 1, 3);
            local.DetailText10.Text10 =
              Substring(local.DetailText10.Text10,
              TextWorkArea.Text10_MaxLength, 1, 4) + Substring
              (export.LastReadHidden.Ssn, CsePersonsWorkSet.Ssn_MaxLength, 4, 2);
              
            local.DetailText30.Text30 =
              Substring(local.DetailText10.Text10,
              TextWorkArea.Text10_MaxLength, 1, 7) + Substring
              (export.LastReadHidden.Ssn, CsePersonsWorkSet.Ssn_MaxLength, 6, 4);
              
            local.Infrastructure.Detail =
              TrimEnd(local.Infrastructure.Detail) + local.DetailText30.Text30;
          }
        }
        else if (local.NumberOfEvents.TotalInteger == 2)
        {
          if ((IsEmpty(export.LastReadHidden.Ssn) || Equal
            (export.LastReadHidden.Ssn, "000000000")) && !
            IsEmpty(export.ArCsePersonsWorkSet.Ssn))
          {
            local.RaiseEventFlag.Text1 = "Y";
            local.Infrastructure.EventId = 46;
            local.Infrastructure.ReasonCode = "ARSSNID";
            local.DetailText30.Text30 = "AP's SSN Identified as :";
            local.Infrastructure.Detail = local.DetailText30.Text30;
            local.DetailText10.Text10 =
              Substring(export.ArCsePersonsWorkSet.Ssn, 1, 3);
            local.DetailText10.Text10 =
              Substring(local.DetailText10.Text10,
              TextWorkArea.Text10_MaxLength, 1, 4) + Substring
              (export.ArCsePersonsWorkSet.Ssn, CsePersonsWorkSet.Ssn_MaxLength,
              4, 2);
            local.DetailText30.Text30 =
              Substring(local.DetailText10.Text10,
              TextWorkArea.Text10_MaxLength, 1, 7) + Substring
              (export.ArCsePersonsWorkSet.Ssn, CsePersonsWorkSet.Ssn_MaxLength,
              6, 4);
            local.Infrastructure.Detail =
              TrimEnd(local.Infrastructure.Detail) + local.DetailText30.Text30;
          }
        }
        else if (local.NumberOfEvents.TotalInteger == 3)
        {
          if (AsChar(export.ArCsePerson.TextMessageIndicator) != AsChar
            (export.HiddenCsePerson.TextMessageIndicator))
          {
            local.RaiseEventFlag.Text1 = "Y";
            local.Infrastructure.EventId = 46;
            local.Infrastructure.ReasonCode = "TXTMSGUPDAR";
            local.Message.Text50 =
              "The text message indicator has been changed from:";
            local.Infrastructure.Detail = local.Message.Text50 + " " + (
              export.HiddenCsePerson.TextMessageIndicator ?? "") + " to " + (
                export.ArCsePerson.TextMessageIndicator ?? "");
            local.AparSelection.Text1 = "R";
            UseSiAddrRaiseEvent();

            if (IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE"))
            {
            }
            else
            {
              UseEabRollbackCics();

              return;
            }

            local.AparSelection.Text1 = "P";
            local.Infrastructure.ReasonCode = "TXTMSGUPDAP";
            UseSiAddrRaiseEvent();

            if (IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE"))
            {
              break;
            }
            else
            {
              UseEabRollbackCics();

              return;
            }
          }
        }
        else
        {
        }

        if (AsChar(local.RaiseEventFlag.Text1) == 'Y')
        {
          // --------------------------------------------
          // This is to aid the event processor to
          //    gather events from a single situation
          // This is an extremely important piece of code
          // --------------------------------------------
          local.ArForInfrastructure.Number = export.ArCsePersonsWorkSet.Number;
          UseOeCabRaiseEvent();

          if (IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE"))
          {
          }
          else
          {
            UseEabRollbackCics();

            return;
          }
        }
      }

      export.LastReadHidden.Ssn = export.ArCsePersonsWorkSet.Ssn;
      export.HiddenCsePerson.TextMessageIndicator =
        export.ArCsePerson.TextMessageIndicator ?? "";
    }

    // ---------------------------------------------
    // 02/04/99 W.Campbell - End of code added to create
    // infrastructure record(s) for change(s) to SSN.
    // The oe cab raise event will be called from
    // here as an extention of case of update.
    // ---------------------------------------------
    // ------------------------------------------------------------
    // If all processing completed successfully, move all imports
    // to previous exports .
    // ------------------------------------------------------------
    export.HiddenPrevCase.Number = export.Case1.Number;
    export.HiddenPrevCsePersonsWorkSet.Number =
      export.ArCsePersonsWorkSet.Number;

    // -----------------------------------------------------------------
    // 11/2/98   Added code to make sure protected fields stay protected when 
    // there is an error.
    // -----------------------------------------------------------------
    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      // ------------------------------------------------------------
      // Protect fields if needed
      // ------------------------------------------------------------
      if (Equal(export.Next.Number, export.HiddenPrevCase.Number))
      {
        if (AsChar(export.HiddenAe.Flag) == 'O')
        {
          var field1 = GetField(export.ArCsePersonsWorkSet, "number");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.ArCsePersonsWorkSet, "dob");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.ArCsePersonsWorkSet, "firstName");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.ArCsePersonsWorkSet, "lastName");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 = GetField(export.ArCsePersonsWorkSet, "middleInitial");

          field5.Color = "cyan";
          field5.Protected = true;

          var field6 = GetField(export.ArCsePersonsWorkSet, "sex");

          field6.Color = "cyan";
          field6.Protected = true;

          var field7 = GetField(export.ArSsnWorkArea, "ssnNumPart1");

          field7.Color = "cyan";
          field7.Protected = true;

          var field8 = GetField(export.ArSsnWorkArea, "ssnNumPart2");

          field8.Color = "cyan";
          field8.Protected = true;

          var field9 = GetField(export.ArSsnWorkArea, "ssnNumPart3");

          field9.Color = "cyan";
          field9.Protected = true;

          var field10 = GetField(export.ArSsnWorkArea, "ssnTextPart1");

          field10.Color = "cyan";
          field10.Protected = true;

          var field11 = GetField(export.ArSsnWorkArea, "ssnTextPart2");

          field11.Color = "cyan";
          field11.Protected = true;

          var field12 = GetField(export.ArSsnWorkArea, "ssnTextPart3");

          field12.Color = "cyan";
          field12.Protected = true;
        }

        if (CharAt(export.ArCsePersonsWorkSet.Number, 10) == 'O')
        {
          var field1 = GetField(export.ArCsePersonsWorkSet, "number");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.ArCsePersonsWorkSet, "dob");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.ArCsePersonsWorkSet, "firstName");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.ArCsePersonsWorkSet, "lastName");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 = GetField(export.ArCsePersonsWorkSet, "middleInitial");

          field5.Color = "cyan";
          field5.Protected = true;

          var field6 = GetField(export.ArCsePersonsWorkSet, "sex");

          field6.Color = "cyan";
          field6.Protected = true;

          var field7 = GetField(export.ArSsnWorkArea, "ssnNumPart1");

          field7.Color = "cyan";
          field7.Protected = true;

          var field8 = GetField(export.ArSsnWorkArea, "ssnNumPart2");

          field8.Color = "cyan";
          field8.Protected = true;

          var field9 = GetField(export.ArSsnWorkArea, "ssnNumPart3");

          field9.Color = "cyan";
          field9.Protected = true;

          var field10 = GetField(export.ArSsnWorkArea, "ssnTextPart1");

          field10.Color = "cyan";
          field10.Protected = true;

          var field11 = GetField(export.ArSsnWorkArea, "ssnTextPart2");

          field11.Color = "cyan";
          field11.Protected = true;

          var field12 = GetField(export.ArSsnWorkArea, "ssnTextPart3");

          field12.Color = "cyan";
          field12.Protected = true;

          var field13 = GetField(export.ArCsePerson, "race");

          field13.Color = "cyan";
          field13.Protected = true;

          var field14 = GetField(export.RacePrompt, "selectChar");

          field14.Color = "cyan";
          field14.Protected = true;

          var field15 = GetField(export.ArCsePerson, "currentSpouseFirstName");

          field15.Color = "cyan";
          field15.Protected = true;

          var field16 = GetField(export.ArCsePerson, "currentSpouseLastName");

          field16.Color = "cyan";
          field16.Protected = true;

          var field17 = GetField(export.ArCsePerson, "currentSpouseMi");

          field17.Color = "cyan";
          field17.Protected = true;

          var field18 = GetField(export.ArCsePerson, "emergencyAreaCode");

          field18.Color = "cyan";
          field18.Protected = true;

          var field19 = GetField(export.ArCsePerson, "emergencyPhone");

          field19.Color = "cyan";
          field19.Protected = true;

          var field20 = GetField(export.ArCsePerson, "homePhone");

          field20.Color = "cyan";
          field20.Protected = true;

          var field21 = GetField(export.ArCsePerson, "homePhoneAreaCode");

          field21.Color = "cyan";
          field21.Protected = true;

          var field22 = GetField(export.ArCsePerson, "otherAreaCode");

          field22.Color = "cyan";
          field22.Protected = true;

          var field23 = GetField(export.ArCsePerson, "otherNumber");

          field23.Color = "cyan";
          field23.Protected = true;

          var field24 = GetField(export.ArCsePerson, "nameMaiden");

          field24.Color = "cyan";
          field24.Protected = true;

          var field25 = GetField(export.ArCsePerson, "priorTafInd");

          field25.Color = "cyan";
          field25.Protected = true;

          var field26 = GetField(export.ArCsePerson, "textMessageIndicator");

          field26.Color = "cyan";
          field26.Protected = true;

          var field27 = GetField(export.ArCsePerson, "customerServiceCode");

          field27.Color = "cyan";
          field27.Protected = true;
        }
      }
    }
  }

  private static void MoveCase2(Case1 source, Case1 target)
  {
    target.Number = source.Number;
    target.ExpeditedPaternityInd = source.ExpeditedPaternityInd;
    target.FullServiceWithoutMedInd = source.FullServiceWithoutMedInd;
    target.FullServiceWithMedInd = source.FullServiceWithMedInd;
    target.LocateInd = source.LocateInd;
    target.ClosureReason = source.ClosureReason;
    target.Status = source.Status;
    target.StatusDate = source.StatusDate;
    target.CseOpenDate = source.CseOpenDate;
    target.PaMedicalService = source.PaMedicalService;
    target.AdcOpenDate = source.AdcOpenDate;
    target.AdcCloseDate = source.AdcCloseDate;
  }

  private static void MoveCase3(Case1 source, Case1 target)
  {
    target.Number = source.Number;
    target.AdcOpenDate = source.AdcOpenDate;
    target.AdcCloseDate = source.AdcCloseDate;
  }

  private static void MoveCaseRole(CaseRole source, CaseRole target)
  {
    target.Identifier = source.Identifier;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.CreatedBy = source.CreatedBy;
    target.Type1 = source.Type1;
    target.StartDate = source.StartDate;
    target.EndDate = source.EndDate;
    target.Note = source.Note;
    target.OnSsInd = source.OnSsInd;
    target.HealthInsuranceIndicator = source.HealthInsuranceIndicator;
    target.MedicalSupportIndicator = source.MedicalSupportIndicator;
    target.ContactFirstName = source.ContactFirstName;
    target.ContactMiddleInitial = source.ContactMiddleInitial;
    target.ContactPhone = source.ContactPhone;
    target.ContactLastName = source.ContactLastName;
    target.ChildCareExpenses = source.ChildCareExpenses;
    target.AssignmentDate = source.AssignmentDate;
    target.AssignmentTerminationCode = source.AssignmentTerminationCode;
    target.AssignmentOfRights = source.AssignmentOfRights;
    target.AssignmentTerminatedDt = source.AssignmentTerminatedDt;
    target.ArChgProcReqInd = source.ArChgProcReqInd;
    target.ArChgProcessedDate = source.ArChgProcessedDate;
    target.ArInvalidInd = source.ArInvalidInd;
  }

  private static void MoveCsePerson1(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
    target.TextMessageIndicator = source.TextMessageIndicator;
    target.ThreatOnStaff = source.ThreatOnStaff;
    target.CustomerServiceCode = source.CustomerServiceCode;
  }

  private static void MoveCsePerson2(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
    target.KscaresNumber = source.KscaresNumber;
    target.Occupation = source.Occupation;
    target.AeCaseNumber = source.AeCaseNumber;
    target.DateOfDeath = source.DateOfDeath;
    target.IllegalAlienIndicator = source.IllegalAlienIndicator;
    target.CurrentSpouseMi = source.CurrentSpouseMi;
    target.CurrentSpouseFirstName = source.CurrentSpouseFirstName;
    target.BirthPlaceState = source.BirthPlaceState;
    target.EmergencyPhone = source.EmergencyPhone;
    target.NameMiddle = source.NameMiddle;
    target.NameMaiden = source.NameMaiden;
    target.HomePhone = source.HomePhone;
    target.OtherNumber = source.OtherNumber;
    target.BirthPlaceCity = source.BirthPlaceCity;
    target.Weight = source.Weight;
    target.OtherIdInfo = source.OtherIdInfo;
    target.CurrentMaritalStatus = source.CurrentMaritalStatus;
    target.CurrentSpouseLastName = source.CurrentSpouseLastName;
    target.Race = source.Race;
    target.HeightFt = source.HeightFt;
    target.HeightIn = source.HeightIn;
    target.HairColor = source.HairColor;
    target.EyeColor = source.EyeColor;
    target.HomePhoneAreaCode = source.HomePhoneAreaCode;
    target.EmergencyAreaCode = source.EmergencyAreaCode;
    target.OtherAreaCode = source.OtherAreaCode;
    target.OtherPhoneType = source.OtherPhoneType;
    target.WorkPhoneAreaCode = source.WorkPhoneAreaCode;
    target.WorkPhone = source.WorkPhone;
    target.WorkPhoneExt = source.WorkPhoneExt;
    target.BirthplaceCountry = source.BirthplaceCountry;
    target.TextMessageIndicator = source.TextMessageIndicator;
    target.TribalCode = source.TribalCode;
    target.ThreatOnStaff = source.ThreatOnStaff;
    target.CustomerServiceCode = source.CustomerServiceCode;
    target.TaxId = source.TaxId;
    target.OrganizationName = source.OrganizationName;
  }

  private static void MoveCsePersonAddress1(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.Type1 = source.Type1;
    target.EndDate = source.EndDate;
    target.EndCode = source.EndCode;
    target.County = source.County;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
  }

  private static void MoveCsePersonAddress2(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.Type1 = source.Type1;
    target.EndDate = source.EndDate;
    target.County = source.County;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
  }

  private static void MoveCsePersonAddress3(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.Type1 = source.Type1;
    target.County = source.County;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
  }

  private static void MoveCsePersonAddress4(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.EndDate = source.EndDate;
    target.EndCode = source.EndCode;
    target.County = source.County;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
  }

  private static void MoveCsePersonsWorkSet1(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.ReplicationIndicator = source.ReplicationIndicator;
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveCsePersonsWorkSet2(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveCsePersonsWorkSet3(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Sex = source.Sex;
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private static void MoveCsePersonsWorkSet4(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private static void MoveDisplacedPerson(DisplacedPerson source,
    DisplacedPerson target)
  {
    target.DisplacedInd = source.DisplacedInd;
    target.DisplacedInterfaceInd = source.DisplacedInterfaceInd;
  }

  private static void MoveInfrastructure1(Infrastructure source,
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

  private static void MoveInfrastructure2(Infrastructure source,
    Infrastructure target)
  {
    target.SituationNumber = source.SituationNumber;
    target.EventId = source.EventId;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
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

  private static void MoveOffice(Office source, Office target)
  {
    target.TypeCode = source.TypeCode;
    target.Name = source.Name;
  }

  private static void MoveSsnWorkArea(SsnWorkArea source, SsnWorkArea target)
  {
    target.SsnText9 = source.SsnText9;
    target.SsnNumPart1 = source.SsnNumPart1;
    target.SsnNumPart2 = source.SsnNumPart2;
    target.SsnNumPart3 = source.SsnNumPart3;
  }

  private void UseCabReadAdabasAddress()
  {
    var useImport = new CabReadAdabasAddress.Import();
    var useExport = new CabReadAdabasAddress.Export();

    useImport.AddressType.Flag = local.AddressType.Flag;
    useImport.CsePersonsWorkSet.Number = export.ArCsePersonsWorkSet.Number;

    Call(CabReadAdabasAddress.Execute, useImport, useExport);

    local.AeMailing.Assign(useExport.Ae);
    local.AbendData.Assign(useExport.AbendData);
  }

  private void UseCabReadAdabasPerson()
  {
    var useImport = new CabReadAdabasPerson.Import();
    var useExport = new CabReadAdabasPerson.Export();

    useImport.CsePersonsWorkSet.Number = import.SelectedAr.Number;

    Call(CabReadAdabasPerson.Execute, useImport, useExport);

    export.ArCsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseCabSsnConvertNumToText()
  {
    var useImport = new CabSsnConvertNumToText.Import();
    var useExport = new CabSsnConvertNumToText.Export();

    useImport.SsnWorkArea.Assign(local.SsnWorkArea);

    Call(CabSsnConvertNumToText.Execute, useImport, useExport);

    MoveSsnWorkArea(useExport.SsnWorkArea, export.ArSsnWorkArea);
  }

  private void UseCabSsnConvertTextToNum()
  {
    var useImport = new CabSsnConvertTextToNum.Import();
    var useExport = new CabSsnConvertTextToNum.Export();

    useImport.SsnWorkArea.SsnText9 = local.SsnWorkArea.SsnText9;

    Call(CabSsnConvertTextToNum.Execute, useImport, useExport);

    MoveSsnWorkArea(useExport.SsnWorkArea, export.ArSsnWorkArea);
  }

  private void UseCabUpdateAdabasPerson()
  {
    var useImport = new CabUpdateAdabasPerson.Import();
    var useExport = new CabUpdateAdabasPerson.Export();

    useImport.CsePersonsWorkSet.Assign(export.ArCsePersonsWorkSet);

    Call(CabUpdateAdabasPerson.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
  }

  private void UseCabValidateCodeValue1()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.Code.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.Common.Flag = useExport.ValidCode.Flag;
  }

  private void UseCabValidateCodeValue2()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.Code.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.Common.Flag = useExport.ValidCode.Flag;
    local.DisplayForeignCountry.Description = useExport.CodeValue.Description;
  }

  private void UseCabValidateCodeValue3()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.Code.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.DisplayForeignCountry.Description = useExport.CodeValue.Description;
  }

  private void UseCabZeroFillNumber()
  {
    var useImport = new CabZeroFillNumber.Import();
    var useExport = new CabZeroFillNumber.Export();

    useImport.Case1.Number = export.Next.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    export.Next.Number = useImport.Case1.Number;
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

  private void UseOeCabRaiseEvent()
  {
    var useImport = new OeCabRaiseEvent.Import();
    var useExport = new OeCabRaiseEvent.Export();

    MoveInfrastructure1(local.Infrastructure, useImport.Infrastructure);
    useImport.CsePerson.Number = local.ArForInfrastructure.Number;

    Call(OeCabRaiseEvent.Execute, useImport, useExport);

    local.Infrastructure.Assign(useExport.Infrastructure);
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

    useImport.CsePersonsWorkSet.Number = import.ArCsePersonsWorkSet.Number;
    useImport.CsePerson.Number = import.ArCsePerson.Number;
    useImport.Case1.Number = export.Next.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseScSecurityCheckForFv()
  {
    var useImport = new ScSecurityCheckForFv.Import();
    var useExport = new ScSecurityCheckForFv.Export();

    useImport.CsePersonsWorkSet.Number = export.ArCsePersonsWorkSet.Number;
    useImport.CsePerson.Number = export.ArCsePerson.Number;
    useImport.Case1.Number = export.Next.Number;

    Call(ScSecurityCheckForFv.Execute, useImport, useExport);
  }

  private void UseSiAddrRaiseEvent()
  {
    var useImport = new SiAddrRaiseEvent.Import();
    var useExport = new SiAddrRaiseEvent.Export();

    useImport.AparSelection.Text1 = local.AparSelection.Text1;
    MoveInfrastructure1(local.Infrastructure, useImport.Infrastructure);
    useImport.CsePerson.Number = export.ArCsePerson.Number;

    Call(SiAddrRaiseEvent.Execute, useImport, useExport);

    MoveInfrastructure2(useExport.Infrastructure, local.Infrastructure);
  }

  private void UseSiAltsBuildAliasAndSsn()
  {
    var useImport = new SiAltsBuildAliasAndSsn.Import();
    var useExport = new SiAltsBuildAliasAndSsn.Export();

    useImport.Ar1.Number = export.ArCsePersonsWorkSet.Number;

    Call(SiAltsBuildAliasAndSsn.Execute, useImport, useExport);

    local.ArOccur.Flag = useExport.ArOccur.Flag;
  }

  private void UseSiArdsReadArCaseRoleDetail()
  {
    var useImport = new SiArdsReadArCaseRoleDetail.Import();
    var useExport = new SiArdsReadArCaseRoleDetail.Export();

    useImport.CsePersonsWorkSet.Number = export.ArCsePersonsWorkSet.Number;
    useImport.Case1.Number = export.Case1.Number;

    Call(SiArdsReadArCaseRoleDetail.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    export.HiddenAe.Flag = useExport.Ae.Flag;
    MoveCsePersonsWorkSet3(useExport.CsePersonsWorkSet,
      export.ArCsePersonsWorkSet);
    export.ArCaseRole.Assign(useExport.Ar);
    export.ArCsePerson.Assign(useExport.CsePerson);
    MoveCase2(useExport.Case1, export.Case1);
  }

  private void UseSiCheckName()
  {
    var useImport = new SiCheckName.Import();
    var useExport = new SiCheckName.Export();

    MoveCsePersonsWorkSet4(export.ArCsePersonsWorkSet,
      useImport.CsePersonsWorkSet);

    Call(SiCheckName.Execute, useImport, useExport);
  }

  private void UseSiGetCsePersonMailingAddr()
  {
    var useImport = new SiGetCsePersonMailingAddr.Import();
    var useExport = new SiGetCsePersonMailingAddr.Export();

    useImport.CsePerson.Number = local.ArCsePerson.Number;

    Call(SiGetCsePersonMailingAddr.Execute, useImport, useExport);

    MoveCsePersonAddress1(useExport.CsePersonAddress, export.CsePersonAddress);
  }

  private void UseSiReadCaseHeaderInformation()
  {
    var useImport = new SiReadCaseHeaderInformation.Import();
    var useExport = new SiReadCaseHeaderInformation.Export();

    useImport.Ap.Number = export.ApCsePersonsWorkSet.Number;
    useImport.Case1.Number = export.Case1.Number;

    Call(SiReadCaseHeaderInformation.Execute, useImport, useExport);

    local.MultipleAps.Flag = useExport.MultipleAps.Flag;
    local.AbendData.Assign(useExport.AbendData);
    export.HiddenAe.Flag = useExport.Ae.Flag;
    export.ApCsePersonsWorkSet.Assign(useExport.Ap);
    export.ArCsePersonsWorkSet.Assign(useExport.Ar);
    export.CaseOpen.Flag = useExport.CaseOpen.Flag;
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.ArCsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    export.HiddenAe.Flag = useExport.Ae.Flag;
  }

  private void UseSiReadOfficeOspHeader()
  {
    var useImport = new SiReadOfficeOspHeader.Import();
    var useExport = new SiReadOfficeOspHeader.Export();

    useImport.Case1.Number = export.Case1.Number;

    Call(SiReadOfficeOspHeader.Execute, useImport, useExport);

    export.ServiceProvider.LastName = useExport.ServiceProvider.LastName;
    MoveOffice(useExport.Office, export.Office);
    export.HeaderLine.Text35 = useExport.HeaderLine.Text35;
  }

  private void UseSiUpdateCaseRole()
  {
    var useImport = new SiUpdateCaseRole.Import();
    var useExport = new SiUpdateCaseRole.Export();

    useImport.Case1.Number = import.Case1.Number;
    useImport.CsePerson.Number = local.ArCsePerson.Number;
    MoveCaseRole(export.ArCaseRole, useImport.CaseRole);

    Call(SiUpdateCaseRole.Execute, useImport, useExport);
  }

  private void UseSiUpdateCsePerson()
  {
    var useImport = new SiUpdateCsePerson.Import();
    var useExport = new SiUpdateCsePerson.Export();

    MoveCsePerson2(local.ArCsePerson, useImport.CsePerson);

    Call(SiUpdateCsePerson.Execute, useImport, useExport);
  }

  private void CreateDisplacedPerson()
  {
    var number = export.ArCsePersonsWorkSet.Number;
    var effectiveDate = local.Current.Date;
    var endDate = local.MaxDate.Date;
    var displacedInd = export.DisplacedPerson.DisplacedInd ?? "";
    var createdBy = global.UserId;
    var createdTimestamp = Now();

    entities.DisplacedPerson.Populated = false;
    Update("CreateDisplacedPerson",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", number);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "endDate", endDate);
        db.SetNullableString(command, "displacedInd", displacedInd);
        db.SetNullableString(command, "displacedIntInd", "");
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableDateTime(command, "lastUpdatedTmst", null);
        db.SetNullableString(command, "lastUpdatedBy", "");
      });

    entities.DisplacedPerson.Number = number;
    entities.DisplacedPerson.EffectiveDate = effectiveDate;
    entities.DisplacedPerson.EndDate = endDate;
    entities.DisplacedPerson.DisplacedInd = displacedInd;
    entities.DisplacedPerson.DisplacedInterfaceInd = "";
    entities.DisplacedPerson.CreatedBy = createdBy;
    entities.DisplacedPerson.CreatedTimestamp = createdTimestamp;
    entities.DisplacedPerson.LastUpdatedTimestamp = null;
    entities.DisplacedPerson.LastUpdatedBy = "";
    entities.DisplacedPerson.Populated = true;
  }

  private bool ReadCaseRole1()
  {
    entities.Father.Populated = false;

    return Read("ReadCaseRole1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.ArCsePersonsWorkSet.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Father.CasNumber = db.GetString(reader, 0);
        entities.Father.CspNumber = db.GetString(reader, 1);
        entities.Father.Type1 = db.GetString(reader, 2);
        entities.Father.Identifier = db.GetInt32(reader, 3);
        entities.Father.StartDate = db.GetNullableDate(reader, 4);
        entities.Father.EndDate = db.GetNullableDate(reader, 5);
        entities.Father.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.Father.CreatedTimestamp = db.GetDateTime(reader, 7);
        entities.Father.ConfirmedType = db.GetNullableString(reader, 8);
        entities.Father.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Father.Type1);
      });
  }

  private bool ReadCaseRole2()
  {
    entities.Mother.Populated = false;

    return Read("ReadCaseRole2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.ArCsePersonsWorkSet.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Mother.CasNumber = db.GetString(reader, 0);
        entities.Mother.CspNumber = db.GetString(reader, 1);
        entities.Mother.Type1 = db.GetString(reader, 2);
        entities.Mother.Identifier = db.GetInt32(reader, 3);
        entities.Mother.StartDate = db.GetNullableDate(reader, 4);
        entities.Mother.EndDate = db.GetNullableDate(reader, 5);
        entities.Mother.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.Mother.CreatedTimestamp = db.GetDateTime(reader, 7);
        entities.Mother.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Mother.Type1);
      });
  }

  private bool ReadCsePerson()
  {
    entities.Ar.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", local.ArCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Ar.Number = db.GetString(reader, 0);
        entities.Ar.Type1 = db.GetString(reader, 1);
        entities.Ar.LastUpdatedTimestamp = db.GetNullableDateTime(reader, 2);
        entities.Ar.LastUpdatedBy = db.GetNullableString(reader, 3);
        entities.Ar.PriorTafInd = db.GetNullableString(reader, 4);
        entities.Ar.Populated = true;
        CheckValid<CsePerson>("Type1", entities.Ar.Type1);
      });
  }

  private IEnumerable<bool> ReadCsePersonAddress()
  {
    entities.CsePersonAddress.Populated = false;

    return ReadEach("ReadCsePersonAddress",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetNullableDate(command, "endDate", date);
        db.SetString(command, "cspNumber", export.ArCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CsePersonAddress.Identifier = db.GetDateTime(reader, 0);
        entities.CsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.CsePersonAddress.SendDate = db.GetNullableDate(reader, 2);
        entities.CsePersonAddress.Source = db.GetNullableString(reader, 3);
        entities.CsePersonAddress.Street1 = db.GetNullableString(reader, 4);
        entities.CsePersonAddress.Street2 = db.GetNullableString(reader, 5);
        entities.CsePersonAddress.City = db.GetNullableString(reader, 6);
        entities.CsePersonAddress.Type1 = db.GetNullableString(reader, 7);
        entities.CsePersonAddress.WorkerId = db.GetNullableString(reader, 8);
        entities.CsePersonAddress.VerifiedDate = db.GetNullableDate(reader, 9);
        entities.CsePersonAddress.EndDate = db.GetNullableDate(reader, 10);
        entities.CsePersonAddress.EndCode = db.GetNullableString(reader, 11);
        entities.CsePersonAddress.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 12);
        entities.CsePersonAddress.LocationType = db.GetString(reader, 13);
        entities.CsePersonAddress.Populated = true;
        CheckValid<CsePersonAddress>("LocationType",
          entities.CsePersonAddress.LocationType);

        return true;
      });
  }

  private bool ReadCsePersonEmailAddress()
  {
    entities.CsePersonEmailAddress.Populated = false;

    return Read("ReadCsePersonEmailAddress",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.ArCsePersonsWorkSet.Number);
        db.SetNullableDate(
          command, "endDate", local.MaxDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePersonEmailAddress.CspNumber = db.GetString(reader, 0);
        entities.CsePersonEmailAddress.EndDate = db.GetNullableDate(reader, 1);
        entities.CsePersonEmailAddress.CreatedTimestamp =
          db.GetDateTime(reader, 2);
        entities.CsePersonEmailAddress.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 3);
        entities.CsePersonEmailAddress.EmailAddress =
          db.GetNullableString(reader, 4);
        entities.CsePersonEmailAddress.Populated = true;
      });
  }

  private bool ReadDisplacedPerson1()
  {
    entities.DisplacedPerson.Populated = false;

    return Read("ReadDisplacedPerson1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.ArCsePersonsWorkSet.Number);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DisplacedPerson.Number = db.GetString(reader, 0);
        entities.DisplacedPerson.EffectiveDate = db.GetDate(reader, 1);
        entities.DisplacedPerson.EndDate = db.GetNullableDate(reader, 2);
        entities.DisplacedPerson.DisplacedInd = db.GetNullableString(reader, 3);
        entities.DisplacedPerson.DisplacedInterfaceInd =
          db.GetNullableString(reader, 4);
        entities.DisplacedPerson.CreatedBy = db.GetString(reader, 5);
        entities.DisplacedPerson.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.DisplacedPerson.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 7);
        entities.DisplacedPerson.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.DisplacedPerson.Populated = true;
      });
  }

  private bool ReadDisplacedPerson2()
  {
    entities.DisplacedPerson.Populated = false;

    return Read("ReadDisplacedPerson2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.ArCsePersonsWorkSet.Number);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DisplacedPerson.Number = db.GetString(reader, 0);
        entities.DisplacedPerson.EffectiveDate = db.GetDate(reader, 1);
        entities.DisplacedPerson.EndDate = db.GetNullableDate(reader, 2);
        entities.DisplacedPerson.DisplacedInd = db.GetNullableString(reader, 3);
        entities.DisplacedPerson.DisplacedInterfaceInd =
          db.GetNullableString(reader, 4);
        entities.DisplacedPerson.CreatedBy = db.GetString(reader, 5);
        entities.DisplacedPerson.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.DisplacedPerson.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 7);
        entities.DisplacedPerson.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.DisplacedPerson.Populated = true;
      });
  }

  private bool ReadInvalidSsn()
  {
    entities.InvalidSsn.Populated = false;

    return Read("ReadInvalidSsn",
      (db, command) =>
      {
        db.SetInt32(command, "ssn", local.Convert.SsnNum9);
        db.SetString(command, "cspNumber", export.ArCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.InvalidSsn.CspNumber = db.GetString(reader, 0);
        entities.InvalidSsn.Ssn = db.GetInt32(reader, 1);
        entities.InvalidSsn.Populated = true;
      });
  }

  private void UpdateCsePerson()
  {
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;
    var priorTafInd = export.ArCsePerson.PriorTafInd ?? "";

    entities.Ar.Populated = false;
    Update("UpdateCsePerson",
      (db, command) =>
      {
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableString(command, "priorTafInd", priorTafInd);
        db.SetString(command, "numb", entities.Ar.Number);
      });

    entities.Ar.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.Ar.LastUpdatedBy = lastUpdatedBy;
    entities.Ar.PriorTafInd = priorTafInd;
    entities.Ar.Populated = true;
  }

  private void UpdateCsePersonAddress()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAddress.Populated);

    var endDate = Now().Date;
    var endCode = "DC";

    entities.CsePersonAddress.Populated = false;
    Update("UpdateCsePersonAddress",
      (db, command) =>
      {
        db.SetNullableDate(command, "endDate", endDate);
        db.SetNullableString(command, "endCode", endCode);
        db.SetDateTime(
          command, "identifier",
          entities.CsePersonAddress.Identifier.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.CsePersonAddress.CspNumber);
      });

    entities.CsePersonAddress.EndDate = endDate;
    entities.CsePersonAddress.EndCode = endCode;
    entities.CsePersonAddress.Populated = true;
  }

  private void UpdateDisplacedPerson1()
  {
    var endDate = local.Current.Date;
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;

    entities.DisplacedPerson.Populated = false;
    Update("UpdateDisplacedPerson1",
      (db, command) =>
      {
        db.SetNullableDate(command, "endDate", endDate);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetString(command, "cspNumber", entities.DisplacedPerson.Number);
        db.SetDate(
          command, "effectiveDate",
          entities.DisplacedPerson.EffectiveDate.GetValueOrDefault());
      });

    entities.DisplacedPerson.EndDate = endDate;
    entities.DisplacedPerson.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.DisplacedPerson.LastUpdatedBy = lastUpdatedBy;
    entities.DisplacedPerson.Populated = true;
  }

  private void UpdateDisplacedPerson2()
  {
    var endDate = local.MaxDate.Date;
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;

    entities.DisplacedPerson.Populated = false;
    Update("UpdateDisplacedPerson2",
      (db, command) =>
      {
        db.SetNullableDate(command, "endDate", endDate);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetString(command, "cspNumber", entities.DisplacedPerson.Number);
        db.SetDate(
          command, "effectiveDate",
          entities.DisplacedPerson.EffectiveDate.GetValueOrDefault());
      });

    entities.DisplacedPerson.EndDate = endDate;
    entities.DisplacedPerson.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.DisplacedPerson.LastUpdatedBy = lastUpdatedBy;
    entities.DisplacedPerson.Populated = true;
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
    /// A value of TribalFlag.
    /// </summary>
    [JsonPropertyName("tribalFlag")]
    public Common TribalFlag
    {
      get => tribalFlag ??= new();
      set => tribalFlag = value;
    }

    /// <summary>
    /// A value of TribalPrompt.
    /// </summary>
    [JsonPropertyName("tribalPrompt")]
    public Common TribalPrompt
    {
      get => tribalPrompt ??= new();
      set => tribalPrompt = value;
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
    /// A value of CsePersonEmailAddress.
    /// </summary>
    [JsonPropertyName("csePersonEmailAddress")]
    public CsePersonEmailAddress CsePersonEmailAddress
    {
      get => csePersonEmailAddress ??= new();
      set => csePersonEmailAddress = value;
    }

    /// <summary>
    /// A value of TextMessageInd.
    /// </summary>
    [JsonPropertyName("textMessageInd")]
    public WorkArea TextMessageInd
    {
      get => textMessageInd ??= new();
      set => textMessageInd = value;
    }

    /// <summary>
    /// A value of DisplacedPerson.
    /// </summary>
    [JsonPropertyName("displacedPerson")]
    public DisplacedPerson DisplacedPerson
    {
      get => displacedPerson ??= new();
      set => displacedPerson = value;
    }

    /// <summary>
    /// A value of WorkForeignCountryDesc.
    /// </summary>
    [JsonPropertyName("workForeignCountryDesc")]
    public WorkArea WorkForeignCountryDesc
    {
      get => workForeignCountryDesc ??= new();
      set => workForeignCountryDesc = value;
    }

    /// <summary>
    /// A value of PhoneTypePrompt.
    /// </summary>
    [JsonPropertyName("phoneTypePrompt")]
    public Common PhoneTypePrompt
    {
      get => phoneTypePrompt ??= new();
      set => phoneTypePrompt = value;
    }

    /// <summary>
    /// A value of SelectedAp.
    /// </summary>
    [JsonPropertyName("selectedAp")]
    public CsePersonsWorkSet SelectedAp
    {
      get => selectedAp ??= new();
      set => selectedAp = value;
    }

    /// <summary>
    /// A value of SelectedAr.
    /// </summary>
    [JsonPropertyName("selectedAr")]
    public CsePersonsWorkSet SelectedAr
    {
      get => selectedAr ??= new();
      set => selectedAr = value;
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
    /// A value of RacePrompt.
    /// </summary>
    [JsonPropertyName("racePrompt")]
    public Common RacePrompt
    {
      get => racePrompt ??= new();
      set => racePrompt = value;
    }

    /// <summary>
    /// A value of ArPrompt.
    /// </summary>
    [JsonPropertyName("arPrompt")]
    public Common ArPrompt
    {
      get => arPrompt ??= new();
      set => arPrompt = value;
    }

    /// <summary>
    /// A value of ApPrompt.
    /// </summary>
    [JsonPropertyName("apPrompt")]
    public Common ApPrompt
    {
      get => apPrompt ??= new();
      set => apPrompt = value;
    }

    /// <summary>
    /// A value of HiddenAe.
    /// </summary>
    [JsonPropertyName("hiddenAe")]
    public Common HiddenAe
    {
      get => hiddenAe ??= new();
      set => hiddenAe = value;
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
    /// A value of HiddenPrevCase.
    /// </summary>
    [JsonPropertyName("hiddenPrevCase")]
    public Case1 HiddenPrevCase
    {
      get => hiddenPrevCase ??= new();
      set => hiddenPrevCase = value;
    }

    /// <summary>
    /// A value of HiddenPrevCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("hiddenPrevCsePersonsWorkSet")]
    public CsePersonsWorkSet HiddenPrevCsePersonsWorkSet
    {
      get => hiddenPrevCsePersonsWorkSet ??= new();
      set => hiddenPrevCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ArCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("arCsePersonsWorkSet")]
    public CsePersonsWorkSet ArCsePersonsWorkSet
    {
      get => arCsePersonsWorkSet ??= new();
      set => arCsePersonsWorkSet = value;
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
    /// A value of ArCaseRole.
    /// </summary>
    [JsonPropertyName("arCaseRole")]
    public CaseRole ArCaseRole
    {
      get => arCaseRole ??= new();
      set => arCaseRole = value;
    }

    /// <summary>
    /// A value of ArCsePerson.
    /// </summary>
    [JsonPropertyName("arCsePerson")]
    public CsePerson ArCsePerson
    {
      get => arCsePerson ??= new();
      set => arCsePerson = value;
    }

    /// <summary>
    /// A value of Alt.
    /// </summary>
    [JsonPropertyName("alt")]
    public WorkArea Alt
    {
      get => alt ??= new();
      set => alt = value;
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
    /// A value of ArSsnWorkArea.
    /// </summary>
    [JsonPropertyName("arSsnWorkArea")]
    public SsnWorkArea ArSsnWorkArea
    {
      get => arSsnWorkArea ??= new();
      set => arSsnWorkArea = value;
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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
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
    /// A value of LastReadHidden.
    /// </summary>
    [JsonPropertyName("lastReadHidden")]
    public CsePersonsWorkSet LastReadHidden
    {
      get => lastReadHidden ??= new();
      set => lastReadHidden = value;
    }

    /// <summary>
    /// A value of HiddenArSex.
    /// </summary>
    [JsonPropertyName("hiddenArSex")]
    public CsePersonsWorkSet HiddenArSex
    {
      get => hiddenArSex ??= new();
      set => hiddenArSex = value;
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
    /// A value of PobStPrompt.
    /// </summary>
    [JsonPropertyName("pobStPrompt")]
    public Common PobStPrompt
    {
      get => pobStPrompt ??= new();
      set => pobStPrompt = value;
    }

    /// <summary>
    /// A value of PobFcPrompt.
    /// </summary>
    [JsonPropertyName("pobFcPrompt")]
    public Common PobFcPrompt
    {
      get => pobFcPrompt ??= new();
      set => pobFcPrompt = value;
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
    /// A value of CustomerServicePrompt.
    /// </summary>
    [JsonPropertyName("customerServicePrompt")]
    public Common CustomerServicePrompt
    {
      get => customerServicePrompt ??= new();
      set => customerServicePrompt = value;
    }

    private Common tribalFlag;
    private Common tribalPrompt;
    private CsePerson hiddenCsePerson;
    private CsePersonEmailAddress csePersonEmailAddress;
    private WorkArea textMessageInd;
    private DisplacedPerson displacedPerson;
    private WorkArea workForeignCountryDesc;
    private Common phoneTypePrompt;
    private CsePersonsWorkSet selectedAp;
    private CsePersonsWorkSet selectedAr;
    private CodeValue selected;
    private Common racePrompt;
    private Common arPrompt;
    private Common apPrompt;
    private Common hiddenAe;
    private CsePersonsWorkSet apCsePersonsWorkSet;
    private Case1 hiddenPrevCase;
    private CsePersonsWorkSet hiddenPrevCsePersonsWorkSet;
    private CsePersonsWorkSet arCsePersonsWorkSet;
    private CsePersonAddress csePersonAddress;
    private CaseRole arCaseRole;
    private CsePerson arCsePerson;
    private WorkArea alt;
    private Case1 case1;
    private Case1 next;
    private Standard standard;
    private SsnWorkArea arSsnWorkArea;
    private ServiceProvider serviceProvider;
    private Office office;
    private NextTranInfo hiddenNextTranInfo;
    private Common caseOpen;
    private CsePersonsWorkSet lastReadHidden;
    private CsePersonsWorkSet hiddenArSex;
    private WorkArea headerLine;
    private Common pobStPrompt;
    private Common pobFcPrompt;
    private CsePerson apCsePerson;
    private Common customerServicePrompt;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of TribalFlag.
    /// </summary>
    [JsonPropertyName("tribalFlag")]
    public Common TribalFlag
    {
      get => tribalFlag ??= new();
      set => tribalFlag = value;
    }

    /// <summary>
    /// A value of TribalPrompt.
    /// </summary>
    [JsonPropertyName("tribalPrompt")]
    public Common TribalPrompt
    {
      get => tribalPrompt ??= new();
      set => tribalPrompt = value;
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
    /// A value of CsePersonEmailAddress.
    /// </summary>
    [JsonPropertyName("csePersonEmailAddress")]
    public CsePersonEmailAddress CsePersonEmailAddress
    {
      get => csePersonEmailAddress ??= new();
      set => csePersonEmailAddress = value;
    }

    /// <summary>
    /// A value of TextMessageInd.
    /// </summary>
    [JsonPropertyName("textMessageInd")]
    public WorkArea TextMessageInd
    {
      get => textMessageInd ??= new();
      set => textMessageInd = value;
    }

    /// <summary>
    /// A value of DisplacedPerson.
    /// </summary>
    [JsonPropertyName("displacedPerson")]
    public DisplacedPerson DisplacedPerson
    {
      get => displacedPerson ??= new();
      set => displacedPerson = value;
    }

    /// <summary>
    /// A value of PhoneTypePrompt.
    /// </summary>
    [JsonPropertyName("phoneTypePrompt")]
    public Common PhoneTypePrompt
    {
      get => phoneTypePrompt ??= new();
      set => phoneTypePrompt = value;
    }

    /// <summary>
    /// A value of SelectedAp.
    /// </summary>
    [JsonPropertyName("selectedAp")]
    public CsePersonsWorkSet SelectedAp
    {
      get => selectedAp ??= new();
      set => selectedAp = value;
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
    /// A value of RacePrompt.
    /// </summary>
    [JsonPropertyName("racePrompt")]
    public Common RacePrompt
    {
      get => racePrompt ??= new();
      set => racePrompt = value;
    }

    /// <summary>
    /// A value of ArPrompt.
    /// </summary>
    [JsonPropertyName("arPrompt")]
    public Common ArPrompt
    {
      get => arPrompt ??= new();
      set => arPrompt = value;
    }

    /// <summary>
    /// A value of ApPrompt.
    /// </summary>
    [JsonPropertyName("apPrompt")]
    public Common ApPrompt
    {
      get => apPrompt ??= new();
      set => apPrompt = value;
    }

    /// <summary>
    /// A value of HiddenAe.
    /// </summary>
    [JsonPropertyName("hiddenAe")]
    public Common HiddenAe
    {
      get => hiddenAe ??= new();
      set => hiddenAe = value;
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
    /// A value of HiddenPrevCase.
    /// </summary>
    [JsonPropertyName("hiddenPrevCase")]
    public Case1 HiddenPrevCase
    {
      get => hiddenPrevCase ??= new();
      set => hiddenPrevCase = value;
    }

    /// <summary>
    /// A value of HiddenPrevCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("hiddenPrevCsePersonsWorkSet")]
    public CsePersonsWorkSet HiddenPrevCsePersonsWorkSet
    {
      get => hiddenPrevCsePersonsWorkSet ??= new();
      set => hiddenPrevCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ArCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("arCsePersonsWorkSet")]
    public CsePersonsWorkSet ArCsePersonsWorkSet
    {
      get => arCsePersonsWorkSet ??= new();
      set => arCsePersonsWorkSet = value;
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
    /// A value of ArCaseRole.
    /// </summary>
    [JsonPropertyName("arCaseRole")]
    public CaseRole ArCaseRole
    {
      get => arCaseRole ??= new();
      set => arCaseRole = value;
    }

    /// <summary>
    /// A value of ArCsePerson.
    /// </summary>
    [JsonPropertyName("arCsePerson")]
    public CsePerson ArCsePerson
    {
      get => arCsePerson ??= new();
      set => arCsePerson = value;
    }

    /// <summary>
    /// A value of Alt.
    /// </summary>
    [JsonPropertyName("alt")]
    public WorkArea Alt
    {
      get => alt ??= new();
      set => alt = value;
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
    /// A value of ArSsnWorkArea.
    /// </summary>
    [JsonPropertyName("arSsnWorkArea")]
    public SsnWorkArea ArSsnWorkArea
    {
      get => arSsnWorkArea ??= new();
      set => arSsnWorkArea = value;
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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
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
    /// A value of LastReadHidden.
    /// </summary>
    [JsonPropertyName("lastReadHidden")]
    public CsePersonsWorkSet LastReadHidden
    {
      get => lastReadHidden ??= new();
      set => lastReadHidden = value;
    }

    /// <summary>
    /// A value of HiddenArSex.
    /// </summary>
    [JsonPropertyName("hiddenArSex")]
    public CsePersonsWorkSet HiddenArSex
    {
      get => hiddenArSex ??= new();
      set => hiddenArSex = value;
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
    /// A value of PobStPrompt.
    /// </summary>
    [JsonPropertyName("pobStPrompt")]
    public Common PobStPrompt
    {
      get => pobStPrompt ??= new();
      set => pobStPrompt = value;
    }

    /// <summary>
    /// A value of PobFcPrompt.
    /// </summary>
    [JsonPropertyName("pobFcPrompt")]
    public Common PobFcPrompt
    {
      get => pobFcPrompt ??= new();
      set => pobFcPrompt = value;
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
    /// A value of WorkForeignCountryDesc.
    /// </summary>
    [JsonPropertyName("workForeignCountryDesc")]
    public WorkArea WorkForeignCountryDesc
    {
      get => workForeignCountryDesc ??= new();
      set => workForeignCountryDesc = value;
    }

    /// <summary>
    /// A value of CustomerServicePrompt.
    /// </summary>
    [JsonPropertyName("customerServicePrompt")]
    public Common CustomerServicePrompt
    {
      get => customerServicePrompt ??= new();
      set => customerServicePrompt = value;
    }

    /// <summary>
    /// A value of ToEmal.
    /// </summary>
    [JsonPropertyName("toEmal")]
    public Common ToEmal
    {
      get => toEmal ??= new();
      set => toEmal = value;
    }

    private Common tribalFlag;
    private Common tribalPrompt;
    private CsePerson hiddenCsePerson;
    private CsePersonEmailAddress csePersonEmailAddress;
    private WorkArea textMessageInd;
    private DisplacedPerson displacedPerson;
    private Common phoneTypePrompt;
    private CsePersonsWorkSet selectedAp;
    private Code prompt;
    private Common racePrompt;
    private Common arPrompt;
    private Common apPrompt;
    private Common hiddenAe;
    private CsePersonsWorkSet apCsePersonsWorkSet;
    private Case1 hiddenPrevCase;
    private CsePersonsWorkSet hiddenPrevCsePersonsWorkSet;
    private CsePersonsWorkSet arCsePersonsWorkSet;
    private CsePersonAddress csePersonAddress;
    private CaseRole arCaseRole;
    private CsePerson arCsePerson;
    private WorkArea alt;
    private Case1 case1;
    private Case1 next;
    private Standard standard;
    private SsnWorkArea arSsnWorkArea;
    private ServiceProvider serviceProvider;
    private Office office;
    private NextTranInfo hiddenNextTranInfo;
    private Common caseOpen;
    private CsePersonsWorkSet lastReadHidden;
    private CsePersonsWorkSet hiddenArSex;
    private WorkArea headerLine;
    private Common pobStPrompt;
    private Common pobFcPrompt;
    private CsePerson apCsePerson;
    private WorkArea workForeignCountryDesc;
    private Common customerServicePrompt;
    private Common toEmal;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Update.
    /// </summary>
    [JsonPropertyName("update")]
    public CsePersonAddress Update
    {
      get => update ??= new();
      set => update = value;
    }

    /// <summary>
    /// A value of Message.
    /// </summary>
    [JsonPropertyName("message")]
    public WorkArea Message
    {
      get => message ??= new();
      set => message = value;
    }

    /// <summary>
    /// A value of AparSelection.
    /// </summary>
    [JsonPropertyName("aparSelection")]
    public WorkArea AparSelection
    {
      get => aparSelection ??= new();
      set => aparSelection = value;
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
    /// A value of Convert.
    /// </summary>
    [JsonPropertyName("convert")]
    public SsnWorkArea Convert
    {
      get => convert ??= new();
      set => convert = value;
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
    /// A value of SsnConcat.
    /// </summary>
    [JsonPropertyName("ssnConcat")]
    public TextWorkArea SsnConcat
    {
      get => ssnConcat ??= new();
      set => ssnConcat = value;
    }

    /// <summary>
    /// A value of SsnPart.
    /// </summary>
    [JsonPropertyName("ssnPart")]
    public Common SsnPart
    {
      get => ssnPart ??= new();
      set => ssnPart = value;
    }

    /// <summary>
    /// A value of ArSsnWorkArea.
    /// </summary>
    [JsonPropertyName("arSsnWorkArea")]
    public SsnWorkArea ArSsnWorkArea
    {
      get => arSsnWorkArea ??= new();
      set => arSsnWorkArea = value;
    }

    /// <summary>
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public CsePersonAddress Blank
    {
      get => blank ??= new();
      set => blank = value;
    }

    /// <summary>
    /// A value of ApExists.
    /// </summary>
    [JsonPropertyName("apExists")]
    public Common ApExists
    {
      get => apExists ??= new();
      set => apExists = value;
    }

    /// <summary>
    /// A value of AeMailing.
    /// </summary>
    [JsonPropertyName("aeMailing")]
    public CsePersonAddress AeMailing
    {
      get => aeMailing ??= new();
      set => aeMailing = value;
    }

    /// <summary>
    /// A value of AddressType.
    /// </summary>
    [JsonPropertyName("addressType")]
    public Common AddressType
    {
      get => addressType ??= new();
      set => addressType = value;
    }

    /// <summary>
    /// A value of ArOccur.
    /// </summary>
    [JsonPropertyName("arOccur")]
    public Common ArOccur
    {
      get => arOccur ??= new();
      set => arOccur = value;
    }

    /// <summary>
    /// A value of PhoneCharNum.
    /// </summary>
    [JsonPropertyName("phoneCharNum")]
    public WorkArea PhoneCharNum
    {
      get => phoneCharNum ??= new();
      set => phoneCharNum = value;
    }

    /// <summary>
    /// A value of PhoneNumeric.
    /// </summary>
    [JsonPropertyName("phoneNumeric")]
    public Common PhoneNumeric
    {
      get => phoneNumeric ??= new();
      set => phoneNumeric = value;
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
    /// A value of MultipleAps.
    /// </summary>
    [JsonPropertyName("multipleAps")]
    public Common MultipleAps
    {
      get => multipleAps ??= new();
      set => multipleAps = value;
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
    /// A value of ArCsePerson.
    /// </summary>
    [JsonPropertyName("arCsePerson")]
    public CsePerson ArCsePerson
    {
      get => arCsePerson ??= new();
      set => arCsePerson = value;
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
    /// A value of NextTranInfo.
    /// </summary>
    [JsonPropertyName("nextTranInfo")]
    public NextTranInfo NextTranInfo
    {
      get => nextTranInfo ??= new();
      set => nextTranInfo = value;
    }

    /// <summary>
    /// A value of SsnWorkArea.
    /// </summary>
    [JsonPropertyName("ssnWorkArea")]
    public SsnWorkArea SsnWorkArea
    {
      get => ssnWorkArea ??= new();
      set => ssnWorkArea = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
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
    /// A value of ArForInfrastructure.
    /// </summary>
    [JsonPropertyName("arForInfrastructure")]
    public CsePerson ArForInfrastructure
    {
      get => arForInfrastructure ??= new();
      set => arForInfrastructure = value;
    }

    /// <summary>
    /// A value of DisplayForeignCountry.
    /// </summary>
    [JsonPropertyName("displayForeignCountry")]
    public CodeValue DisplayForeignCountry
    {
      get => displayForeignCountry ??= new();
      set => displayForeignCountry = value;
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

    private CsePersonAddress update;
    private WorkArea message;
    private WorkArea aparSelection;
    private DateWorkArea max;
    private SsnWorkArea convert;
    private DateWorkArea current;
    private TextWorkArea ssnConcat;
    private Common ssnPart;
    private SsnWorkArea arSsnWorkArea;
    private CsePersonAddress blank;
    private Common apExists;
    private CsePersonAddress aeMailing;
    private Common addressType;
    private Common arOccur;
    private WorkArea phoneCharNum;
    private Common phoneNumeric;
    private CsePersonAddress csePersonAddress;
    private Common multipleAps;
    private AbendData abendData;
    private CsePerson arCsePerson;
    private Common common;
    private CodeValue codeValue;
    private Code code;
    private NextTranInfo nextTranInfo;
    private SsnWorkArea ssnWorkArea;
    private TextWorkArea textWorkArea;
    private Infrastructure infrastructure;
    private DateWorkArea zero;
    private WorkArea raiseEventFlag;
    private Common numberOfEvents;
    private TextWorkArea detailText30;
    private TextWorkArea detailText10;
    private CsePerson arForInfrastructure;
    private CodeValue displayForeignCountry;
    private DateWorkArea maxDate;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CsePersonEmailAddress.
    /// </summary>
    [JsonPropertyName("csePersonEmailAddress")]
    public CsePersonEmailAddress CsePersonEmailAddress
    {
      get => csePersonEmailAddress ??= new();
      set => csePersonEmailAddress = value;
    }

    /// <summary>
    /// A value of InvalidSsn.
    /// </summary>
    [JsonPropertyName("invalidSsn")]
    public InvalidSsn InvalidSsn
    {
      get => invalidSsn ??= new();
      set => invalidSsn = value;
    }

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePerson Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of DisplacedPerson.
    /// </summary>
    [JsonPropertyName("displacedPerson")]
    public DisplacedPerson DisplacedPerson
    {
      get => displacedPerson ??= new();
      set => displacedPerson = value;
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
    /// A value of Father.
    /// </summary>
    [JsonPropertyName("father")]
    public CaseRole Father
    {
      get => father ??= new();
      set => father = value;
    }

    /// <summary>
    /// A value of Mother.
    /// </summary>
    [JsonPropertyName("mother")]
    public CaseRole Mother
    {
      get => mother ??= new();
      set => mother = value;
    }

    private CsePersonAddress csePersonAddress;
    private CsePersonEmailAddress csePersonEmailAddress;
    private InvalidSsn invalidSsn;
    private CsePerson ar;
    private DisplacedPerson displacedPerson;
    private CsePerson csePerson;
    private CaseRole father;
    private CaseRole mother;
  }
#endregion
}
