// Program: SI_CHDS_CHILD_DETAILS, ID: 371757606, model: 746.
// Short name: SWECHDSP
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
/// A program: SI_CHDS_CHILD_DETAILS.
/// </para>
/// <para>
/// RESP: SRVINIT	
/// This procedure updates any information about a CSE PERSON as a child on a 
/// CASE.  Any information held on the ADABAS files will not be updateable when
/// the CSE PERSON is owned by AE.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiChdsChildDetails: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CHDS_CHILD_DETAILS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiChdsChildDetails(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiChdsChildDetails.
  /// </summary>
  public SiChdsChildDetails(IContext context, Import import, Export export):
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
    //                   M A I N T E N A N C E   L O G
    // Date	  Developer		Description
    // 04-13-95  Helen Sharland - MTW	Initial Development
    // 02-15-96  Bruce Moore		Retrofit
    // 06/27/96  G. Lofton		Changed ssn to numeric fields.
    // 11/01/96  G. Lofton		Add new security and remove old.
    // 12/23/96  Raju			Event insertion
    // 01/02/97  Raju			added 2 more events
    //                                 
    // - ar waived insurance ,
    //                                 
    // - parental rights severed
    // 06/18/97  M. D. Wheaton         deleted datenum
    // 07/04/97  H. Kennedy            Added name fields to the import ap
    //                                 
    // field in order to pass this data
    //                                 
    // back to the perm screen.  This
    //                                 
    // data was getting lost after the
    //                                 
    // first pass as the export view
    // had
    //                                 
    // no import view, to which it was
    //                                 
    // mapped, on the screen.
    // 09/28/98 C Deghand              Added SET statements to make
    //                                 
    // prompts spaces on a display.
    // 11/2/98  C Deghand              Added code and modified ESCAPE's to
    //                                 
    // make sure protected code stays
    //                                 
    // protected on an error.
    // -------------------------------------------------------------------
    // 02/09/99 W.Campbell             Added code to USE
    //                                 
    // EAB_ROLLBACK_CICS
    //                                 
    // for correct DB/2 update
    //                                 
    // and rollback operation.
    // ---------------------------------------------
    // 02/09/99 W.Campbell             Moved the USE for
    //                                 
    // ADABAS update until after
    //                                 
    // all the DB/2 updates since
    //                                 
    // ADABAS does not provide
    //                                 
    // for rollback.
    // ---------------------------------------------
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
    // 05/26/99 W.Campbell             Replaced zd exit states.
    // -----------------------------------------------
    // 03/03/00  C. Ott                Modified for PRWORA Paternity
    //                                 
    // redesign.  WR # 160.
    // ---------------------------------------------------------------
    // 11/27/00 M.Lachowicz            Changed header line.
    //                                 
    // WR #298.
    // -----------------------------------------------
    // 22/02/01 M.Lachowicz           Do not allow for
    //                                
    // space sex code.
    //                                
    // Work done on PR#113332.
    // --------------------------------------------
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
    // -------------------------------------------------------------------
    // 05/20/2002  Vithal Madhira      WR# 020259
    // New flow added to ALTS from CHDS. New PF 15 (ALTS) added to the screen. 
    // Old PF15 (FCDS) changed to PF17(FCDS) to be consistant with APDS/ARDS. PF
    // key mapping also changed accordingly on CHDS screen.
    // -------------------------------------------------------------------
    // 01/27/03 GVandy PR160417	Validate first, last and middle name.
    // ---------------------------------------------------------------------------------
    // 09/09/05  GVandy  WR00256682  Add indicator to screen to indicate if 
    // individual was displaced by Hurricane Katrina.
    // 11/29/05  GVandy  PR00260353  Add effective and end dates to Hurricane 
    // Katrina displaced_person records.
    // ----------------------------------------------------------------------------
    // 10/29/07   LSS     PR# 180608 / CQ406
    // Added verify statement to error out if ssn contains a non-numeric 
    // character.
    // ----------------------------------------------------------------------------
    // -----------------------------------------------------
    // 11/01/07     MWF   PR# 166617 CQ367
    // Add alias indicator to the CHDS screen
    // -----------------------------------------------------
    // 06/05/2009   DDupree     Added check when updating or adding a ssn 
    // against the invalid ssn table. Part of CQ7189.
    // __________________________________________________________________________________
    // 09/13/2018 JHarden  CQ64095  Add Tribal Fields to APDS/ARDS/CHDS
    // ----------------------------------------------------------------------------------------------------------------
    //  Date		Developer	Request #      Description
    // ----------------------------------------------------------------------------------------------------------------
    // 07/08/2020	Raj		CQ66150        Modified to add additional validations to 
    // Over 18 &  In School
    //                                                
    // and Emancipaton Date.  When over 18 and in
    // school flag is changed
    //                                                
    // from Y to N Emancipation date will populated
    // based on Child's
    //                                                
    // data of birth value.
    // ----------------------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    local.Current.Date = Now().Date;

    // ---------------------------------------------
    // Move Imports to Exports
    // ---------------------------------------------
    export.Hidden.Assign(import.Hidden);
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.Next.Number = import.Next.Number;
    export.Case1.Number = import.Case1.Number;
    MoveCsePersonsWorkSet2(import.Ap, export.Ap);
    MoveCsePersonsWorkSet3(import.Ar, export.Ar);
    export.ChCaseRole.Assign(import.ChCaseRole);
    export.ChCsePerson.Assign(import.ChCsePerson);
    export.ChCsePersonsWorkSet.Assign(import.ChCsePersonsWorkSet);
    export.ChSsnWorkArea.Assign(import.ChSsnWorkArea);
    export.AbsencePrompt.SelectChar = import.AbsencePrompt.SelectChar;
    export.ApPrompt.SelectChar = import.ApPrompt.SelectChar;
    export.ArRelPrompt.SelectChar = import.ArRelPrompt.SelectChar;
    export.RacePrompt.SelectChar = import.RacePrompt.SelectChar;
    export.RightsPrompt.SelectChar = import.RightsPrompt.SelectChar;
    export.ChPrompt.SelectChar = import.ChPrompt.SelectChar;
    export.PobStPrompt.SelectChar = import.PobStPrompt.SelectChar;
    export.PobFcPrompt.SelectChar = import.PobFcPrompt.SelectChar;

    // CQ64095
    export.TribalPrompt.SelectChar = import.TribalPrompt.SelectChar;
    export.TribalFlag.Flag = import.TribalFlag.Flag;
    export.ServiceProvider.LastName = import.ServiceProvider.LastName;

    // 11/27/00 M.L Start
    export.HeaderLine.Text35 = import.HeaderLine.Text35;

    // 11/27/00 M.L End
    MoveOffice(import.Office, export.Office);
    export.CaseOpen.Flag = import.CaseOpen.Flag;
    export.ActiveChild.Flag = import.ActiveChild.Flag;

    // 11/06/07   MWF    PR# 166617  CQ367
    export.Alt.Text13 = import.Alt.Text13;

    if (!import.Child.IsEmpty)
    {
      for(import.Child.Index = 0; import.Child.Index < import.Child.Count; ++
        import.Child.Index)
      {
        if (!import.Child.CheckSize())
        {
          break;
        }

        export.Child.Index = import.Child.Index;
        export.Child.CheckSize();

        export.Child.Update.Detail.Code = import.Child.Item.Detail.Code;
      }

      import.Child.CheckIndex();
    }

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

    // ---------------------------------------------
    // Move Hidden Import Views to Hidden Export Views
    // ---------------------------------------------
    export.HiddenPreviousCase.Number = import.HiddenPreviousCase.Number;
    export.HiddenPreviousCsePersonsWorkSet.Number =
      import.HiddenPreviousCsePersonsWorkSet.Number;
    export.HiddenAe.Flag = import.HiddenAe.Flag;
    MoveCsePersonsWorkSet4(import.HiddenApSelected, export.HiddenApSelected);
    export.ChPrev.Ssn = import.ChCsePersonsWorkSet.Ssn;
    export.HiddenChSelected.Number = import.HiddenChSelected.Number;
    export.Over18InSchoolChgFl.Flag = import.Over18InSchoolChgFl.Flag;

    if (AsChar(export.Over18InSchoolChgFl.Flag) == 'Y' || AsChar
      (export.ChCaseRole.Over18AndInSchool) == 'Y')
    {
      var field = GetField(export.ChCaseRole, "dateOfEmancipation");

      field.Color = "green";
      field.Highlighting = Highlighting.Underscore;
      field.Protected = false;
    }
    else
    {
      var field = GetField(export.ChCaseRole, "dateOfEmancipation");

      field.Color = "cyan";
      field.Protected = true;
    }

    export.LastReadHiddenCh.Assign(import.LastReadHiddenCh);

    // ---------------------------------------------
    //         	N E X T   T R A N
    // Use the CAB to nexttran to another procedure.
    // ---------------------------------------------
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      export.Hidden.CaseNumber = export.Next.Number;
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
        export.Next.Number = export.Hidden.CaseNumber ?? Spaces(10);
        UseCabZeroFillNumber();
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Next, "number");

        field.Error = true;

        goto Test1;
      }

      // ---------------------------------------------
      // Start of Code (Raju 01/20/97:1035 hrs CST)
      // ---------------------------------------------
      if (Equal(export.Hidden.LastTran, "SRPT") || Equal
        (export.Hidden.LastTran, "SRPU"))
      {
        local.LastTran.SystemGeneratedIdentifier =
          export.Hidden.InfrastructureId.GetValueOrDefault();
        UseOeCabReadInfrastructure();
        export.Case1.Number = local.LastTran.CaseNumber ?? Spaces(10);
        export.Next.Number = local.LastTran.CaseNumber ?? Spaces(10);
        export.ChCsePersonsWorkSet.Number = local.LastTran.CsePersonNumber ?? Spaces
          (10);
      }

      // ---------------------------------------------
      // End  of Code
      // ---------------------------------------------
      global.Command = "DISPLAY";
    }

Test1:

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    // ------------------------------------------------------------
    // Protect fields if required
    // ------------------------------------------------------------
    if (Equal(export.Next.Number, export.HiddenPreviousCase.Number))
    {
      if (AsChar(export.HiddenAe.Flag) == 'O')
      {
        // ---------------------------------------------
        // This CSE Person is owned by the AE system and may not be changed by 
        // the CSE system.
        // ---------------------------------------------
        var field1 = GetField(export.ChCsePersonsWorkSet, "dob");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.ChCsePersonsWorkSet, "firstName");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.ChCsePersonsWorkSet, "lastName");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.ChCsePersonsWorkSet, "middleInitial");

        field4.Color = "cyan";
        field4.Protected = true;

        var field5 = GetField(export.ChCsePersonsWorkSet, "sex");

        field5.Color = "cyan";
        field5.Protected = true;

        var field6 = GetField(export.ChSsnWorkArea, "ssnNumPart1");

        field6.Color = "cyan";
        field6.Protected = true;

        var field7 = GetField(export.ChSsnWorkArea, "ssnNumPart2");

        field7.Color = "cyan";
        field7.Protected = true;

        var field8 = GetField(export.ChSsnWorkArea, "ssnNumPart3");

        field8.Color = "cyan";
        field8.Protected = true;

        var field9 = GetField(export.ChSsnWorkArea, "ssnTextPart1");

        field9.Color = "cyan";
        field9.Protected = true;

        var field10 = GetField(export.ChSsnWorkArea, "ssnTextPart2");

        field10.Color = "cyan";
        field10.Protected = true;

        var field11 = GetField(export.ChSsnWorkArea, "ssnTextPart3");

        field11.Color = "cyan";
        field11.Protected = true;
      }
    }

    if (Equal(global.Command, "RETCOMP"))
    {
      if (AsChar(export.ApPrompt.SelectChar) == 'S')
      {
        if (!IsEmpty(import.HiddenApSelected.Number))
        {
          export.Ap.Number = import.HiddenApSelected.Number;
        }

        export.ApPrompt.SelectChar = "";

        var field = GetField(export.ApPrompt, "selectChar");

        field.Protected = false;
        field.Focused = true;
      }
      else if (!IsEmpty(import.HiddenApSelected.Number))
      {
        if (!Equal(import.HiddenApSelected.Number,
          import.HiddenChSelected.Number))
        {
          export.Ap.Number = import.HiddenApSelected.Number;
        }
      }

      if (AsChar(export.ChPrompt.SelectChar) == 'S')
      {
        if (!IsEmpty(import.HiddenChSelected.Number))
        {
          export.ChCsePersonsWorkSet.Number = import.HiddenChSelected.Number;
        }

        export.ChPrompt.SelectChar = "";

        var field = GetField(export.ChPrompt, "selectChar");

        field.Protected = false;
        field.Focused = true;
      }
      else if (!IsEmpty(import.HiddenChSelected.Number))
      {
        export.ChCsePersonsWorkSet.Number = import.HiddenChSelected.Number;
      }

      global.Command = "DISPLAY";
    }

    if (!Equal(global.Command, "DISPLAY"))
    {
      if (export.ChSsnWorkArea.SsnNumPart1 == 0 && export
        .ChSsnWorkArea.SsnNumPart2 == 0 && export.ChSsnWorkArea.SsnNumPart3 == 0
        )
      {
        export.ChCsePersonsWorkSet.Ssn = "";
      }
      else
      {
        MoveSsnWorkArea2(export.ChSsnWorkArea, local.SsnWorkArea);
        local.SsnWorkArea.ConvertOption = "2";
        UseCabSsnConvertNumToText();
        export.ChCsePersonsWorkSet.Ssn = export.ChSsnWorkArea.SsnText9;
      }

      // PR160844. Changes to highlight display on the screen if there is a 
      // blank in the ssn.
      local.SsnPart.Count = 0;

      if (IsEmpty(Substring(export.ChSsnWorkArea.SsnTextPart1, 1, 1)))
      {
        local.SsnPart.Count = (int)((long)local.SsnPart.Count + 1);
      }

      if (IsEmpty(Substring(export.ChSsnWorkArea.SsnTextPart1, 2, 1)))
      {
        local.SsnPart.Count = (int)((long)local.SsnPart.Count + 1);
      }

      if (IsEmpty(Substring(export.ChSsnWorkArea.SsnTextPart1, 3, 1)))
      {
        local.SsnPart.Count = (int)((long)local.SsnPart.Count + 1);
      }

      if (IsEmpty(Substring(export.ChSsnWorkArea.SsnTextPart2, 1, 1)))
      {
        local.SsnPart.Count = (int)((long)local.SsnPart.Count + 1);
      }

      if (IsEmpty(Substring(export.ChSsnWorkArea.SsnTextPart2, 2, 1)))
      {
        local.SsnPart.Count = (int)((long)local.SsnPart.Count + 1);
      }

      if (IsEmpty(Substring(export.ChSsnWorkArea.SsnTextPart3, 1, 1)))
      {
        local.SsnPart.Count = (int)((long)local.SsnPart.Count + 1);
      }

      if (IsEmpty(Substring(export.ChSsnWorkArea.SsnTextPart3, 2, 1)))
      {
        local.SsnPart.Count = (int)((long)local.SsnPart.Count + 1);
      }

      if (IsEmpty(Substring(export.ChSsnWorkArea.SsnTextPart3, 3, 1)))
      {
        local.SsnPart.Count = (int)((long)local.SsnPart.Count + 1);
      }

      if (IsEmpty(Substring(export.ChSsnWorkArea.SsnTextPart3, 4, 1)))
      {
        local.SsnPart.Count = (int)((long)local.SsnPart.Count + 1);
      }

      if (local.SsnPart.Count > 8)
      {
        goto Test2;
      }

      if (local.SsnPart.Count > 0 && local.SsnPart.Count < 9)
      {
        var field1 = GetField(export.ChSsnWorkArea, "ssnTextPart1");

        field1.Error = true;

        var field2 = GetField(export.ChSsnWorkArea, "ssnTextPart2");

        field2.Error = true;

        var field3 = GetField(export.ChSsnWorkArea, "ssnTextPart3");

        field3.Error = true;

        ExitState = "LE0000_SSN_CONTAINS_NONNUM";

        return;
      }

      // PR160844. End changes to highlight display on the screen if there is a 
      // blank in the ssn.
    }

Test2:

    if (Equal(global.Command, "UPDATE"))
    {
      // 03/09/01 M.L Start
      // 03/09/01 M.L End
    }

    // ---------------------------------------------
    // When the control is returned from a LIST screen
    // Populate the appropriate prompt fields.
    // ---------------------------------------------
    if (Equal(global.Command, "RTLIST"))
    {
      if (AsChar(export.RacePrompt.SelectChar) == 'S')
      {
        if (!IsEmpty(import.Selected.Cdvalue))
        {
          export.ChCsePerson.Race = import.Selected.Cdvalue;
        }

        export.RacePrompt.SelectChar = "";

        var field = GetField(export.RacePrompt, "selectChar");

        field.Protected = false;
        field.Focused = true;
      }

      if (AsChar(export.PobStPrompt.SelectChar) == 'S')
      {
        if (!IsEmpty(import.Selected.Cdvalue))
        {
          export.ChCsePerson.BirthPlaceState = import.Selected.Cdvalue;
        }

        export.PobStPrompt.SelectChar = "";

        var field = GetField(export.PobStPrompt, "selectChar");

        field.Protected = false;
        field.Focused = true;
      }

      if (AsChar(export.PobFcPrompt.SelectChar) == 'S')
      {
        if (!IsEmpty(import.Selected.Cdvalue))
        {
          export.ChCsePerson.BirthplaceCountry = import.Selected.Cdvalue;
          export.WorkForeignCountryDesc.Text40 = import.Selected.Description;
        }

        export.PobFcPrompt.SelectChar = "";

        var field = GetField(export.PobFcPrompt, "selectChar");

        field.Protected = false;
        field.Focused = true;
      }

      if (AsChar(export.ArRelPrompt.SelectChar) == 'S')
      {
        if (!IsEmpty(import.Selected.Cdvalue))
        {
          export.ChCaseRole.RelToAr = import.Selected.Cdvalue;
        }

        export.ArRelPrompt.SelectChar = "";

        var field = GetField(export.ArRelPrompt, "selectChar");

        field.Protected = false;
        field.Focused = true;
      }

      if (AsChar(export.AbsencePrompt.SelectChar) == 'S')
      {
        if (!IsEmpty(import.Selected.Cdvalue))
        {
          export.ChCaseRole.AbsenceReasonCode = import.Selected.Cdvalue;
        }

        export.AbsencePrompt.SelectChar = "";

        var field = GetField(export.AbsencePrompt, "selectChar");

        field.Protected = false;
        field.Focused = true;
      }

      if (AsChar(export.RightsPrompt.SelectChar) == 'S')
      {
        if (!IsEmpty(import.Selected.Cdvalue))
        {
          export.ChCaseRole.FcParentalRights = import.Selected.Cdvalue;
        }

        export.RightsPrompt.SelectChar = "";

        var field = GetField(export.RightsPrompt, "selectChar");

        field.Protected = false;
        field.Focused = true;
      }

      // CQ64095
      if (AsChar(export.TribalPrompt.SelectChar) == 'S')
      {
        if (!IsEmpty(import.Selected.Cdvalue))
        {
          export.ChCsePerson.TribalCode = import.Selected.Cdvalue;
        }

        export.TribalPrompt.SelectChar = "";

        var field = GetField(export.TribalPrompt, "selectChar");

        field.Protected = false;
        field.Focused = true;
      }

      if (!IsEmpty(export.ChCsePerson.TribalCode))
      {
        export.TribalFlag.Flag = "Y";
      }
      else
      {
        export.TribalFlag.Flag = "N";
      }

      return;
    }

    if (Equal(global.Command, "FCDS") || Equal(global.Command, "CPAT") || Equal
      (global.Command, "ALTS"))
    {
    }
    else
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
    }

    // ---------------------------------------------
    //        P F K E Y   P R O C E S S I N G
    // ---------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
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
            ++local.Invalid.Count;
            ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

            break;
          default:
            var field1 = GetField(export.ApPrompt, "selectChar");

            field1.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            ++local.Invalid.Count;

            break;
        }

        switch(AsChar(import.ChPrompt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            ++local.Invalid.Count;
            ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

            break;
          default:
            var field1 = GetField(export.ChPrompt, "selectChar");

            field1.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            ++local.Invalid.Count;

            break;
        }

        switch(AsChar(import.RacePrompt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            export.Prompt.CodeName = "RACE";
            ++local.Invalid.Count;
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            break;
          default:
            var field1 = GetField(export.RacePrompt, "selectChar");

            field1.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            ++local.Invalid.Count;

            break;
        }

        switch(AsChar(import.PobStPrompt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            export.Prompt.CodeName = "STATE CODE";
            ++local.Invalid.Count;
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            break;
          default:
            var field1 = GetField(export.PobStPrompt, "selectChar");

            field1.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            ++local.Invalid.Count;

            break;
        }

        switch(AsChar(import.PobFcPrompt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            export.Prompt.CodeName = "FIPS COUNTRY CODE";
            ++local.Invalid.Count;
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            break;
          default:
            var field1 = GetField(export.PobFcPrompt, "selectChar");

            field1.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            ++local.Invalid.Count;

            break;
        }

        switch(AsChar(import.ArRelPrompt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            export.Prompt.CodeName = "RELATIONSHIP TO AR";
            ++local.Invalid.Count;
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            break;
          default:
            var field1 = GetField(export.ArRelPrompt, "selectChar");

            field1.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            ++local.Invalid.Count;

            break;
        }

        switch(AsChar(import.AbsencePrompt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            export.Prompt.CodeName = "ABSENCE";
            ++local.Invalid.Count;
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            break;
          default:
            var field1 = GetField(export.AbsencePrompt, "selectChar");

            field1.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            ++local.Invalid.Count;

            break;
        }

        switch(AsChar(import.RightsPrompt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            export.Prompt.CodeName = "RIGHTS SEVERED";
            ++local.Invalid.Count;
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            break;
          default:
            var field1 = GetField(export.RightsPrompt, "selectChar");

            field1.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            ++local.Invalid.Count;

            break;
        }

        // CQ64095
        switch(AsChar(import.TribalPrompt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            export.Prompt.CodeName = "TRIBAL NAME";
            ++local.Invalid.Count;
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            break;
          default:
            var field1 = GetField(export.TribalPrompt, "selectChar");

            field1.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            ++local.Invalid.Count;

            break;
        }

        if (!IsEmpty(export.ChCsePerson.TribalCode))
        {
          export.TribalFlag.Flag = "Y";
        }
        else
        {
          export.TribalFlag.Flag = "N";
        }

        switch(local.Invalid.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

            break;
          case 1:
            break;
          default:
            if (AsChar(export.ApPrompt.SelectChar) == 'S')
            {
              var field1 = GetField(export.ApPrompt, "selectChar");

              field1.Error = true;
            }

            if (AsChar(export.ChPrompt.SelectChar) == 'S')
            {
              var field1 = GetField(export.ChPrompt, "selectChar");

              field1.Error = true;
            }

            if (AsChar(export.RacePrompt.SelectChar) == 'S')
            {
              var field1 = GetField(export.RacePrompt, "selectChar");

              field1.Error = true;
            }

            if (AsChar(export.PobStPrompt.SelectChar) == 'S')
            {
              var field1 = GetField(export.PobStPrompt, "selectChar");

              field1.Error = true;
            }

            if (AsChar(export.PobFcPrompt.SelectChar) == 'S')
            {
              var field1 = GetField(export.PobFcPrompt, "selectChar");

              field1.Error = true;
            }

            if (AsChar(export.ArRelPrompt.SelectChar) == 'S')
            {
              var field1 = GetField(export.ArRelPrompt, "selectChar");

              field1.Error = true;
            }

            if (AsChar(export.AbsencePrompt.SelectChar) == 'S')
            {
              var field1 = GetField(export.AbsencePrompt, "selectChar");

              field1.Error = true;
            }

            if (AsChar(export.RightsPrompt.SelectChar) == 'S')
            {
              var field1 = GetField(export.RightsPrompt, "selectChar");

              field1.Error = true;
            }

            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            break;
        }

        break;
      case "DISPLAY":
        export.ApPrompt.SelectChar = "";
        export.ChPrompt.SelectChar = "";
        export.RacePrompt.SelectChar = "";
        export.PobStPrompt.SelectChar = "";
        export.ArRelPrompt.SelectChar = "";
        export.AbsencePrompt.SelectChar = "";
        export.RightsPrompt.SelectChar = "";

        // ----------------------------------------------------------------------------------------------------------------
        // CQ66150:  Reset the in school and over 18 change flag, this flag 
        // value set during the update process. It is
        //           required to reset the value.
        // ----------------------------------------------------------------------------------------------------------------
        export.Over18InSchoolChgFl.Flag = "";

        // cq64095
        export.TribalPrompt.SelectChar = "";
        export.ChSsnWorkArea.SsnTextPart1 = "";
        export.ChSsnWorkArea.SsnTextPart2 = "";
        export.ChSsnWorkArea.SsnTextPart3 = "";

        // 11/01/07     MWF    PR# 166617 CQ367   Added Set statement
        export.Alt.Text13 = "";

        if (!IsEmpty(export.Next.Number))
        {
          UseCabZeroFillNumber();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            if (IsExitState("CASE_NUMBER_NOT_NUMERIC"))
            {
              var field1 = GetField(export.Next, "number");

              field1.Error = true;

              break;
            }
          }
        }
        else
        {
          var field1 = GetField(export.Next, "number");

          field1.Error = true;

          ExitState = "CASE_NUMBER_REQUIRED";

          break;
        }

        if (IsEmpty(export.Case1.Number))
        {
          export.Case1.Number = export.Next.Number;
        }

        if (!Equal(export.Next.Number, export.Case1.Number))
        {
          export.Case1.Number = export.Next.Number;
          export.Ap.Number = "";
          export.ChCsePersonsWorkSet.Number = "";
        }

        if (!IsEmpty(export.Ap.Number))
        {
          local.TextWorkArea.Text10 = export.Ap.Number;
          UseEabPadLeftWithZeros();
          export.Ap.Number = local.TextWorkArea.Text10;
        }

        if (!IsEmpty(export.ChCsePersonsWorkSet.Number))
        {
          local.TextWorkArea.Text10 = export.ChCsePersonsWorkSet.Number;
          UseEabPadLeftWithZeros();
          export.ChCsePersonsWorkSet.Number = local.TextWorkArea.Text10;
        }

        // ---------------------------------------------
        // Call the action block that reads the data required for this screen.
        // --------------------------------------------
        UseSiReadCaseHeaderInformation();

        switch(AsChar(local.AbendData.Type1))
        {
          case ' ':
            break;
          case 'A':
            return;
          case 'C':
            return;
          default:
            break;
        }

        if (AsChar(local.MultipleAps.Flag) == 'Y')
        {
          // -----------------------------------------------
          // 05/03/99 W.Campbell - Added code to send
          // selection needed msg to COMP.  BE SURE
          // TO MATCH next_tran_info ON THE DIALOG
          // FLOW.
          // -----------------------------------------------
          export.Hidden.MiscText1 = "SELECTION NEEDED";
          ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

          return;
        }

        if (IsExitState("NO_APS_ON_A_CASE"))
        {
          local.NoAp.Flag = "Y";
          ExitState = "ACO_NN0000_ALL_OK";
        }
        else
        {
          local.NoAp.Flag = "N";
        }

        UseSiReadOfficeOspHeader();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        if (IsEmpty(export.ChCsePersonsWorkSet.Number))
        {
          UseSiRetrieveChildForCase();

          if (AsChar(local.MultipleAps.Flag) == 'Y')
          {
            ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

            return;
          }

          if (IsEmpty(export.ChCsePersonsWorkSet.Number))
          {
            var field1 = GetField(export.ChCsePersonsWorkSet, "number");

            field1.Error = true;

            ExitState = "NO_ACTIVE_CHILD";
          }
        }

        // ---------------------------------------------
        // Code added by Raju  Dec 23, 1996:0400 hrs CST
        // ---------------------------------------------
        // ---------------------------------------------
        // Start of code
        // ---------------------------------------------
        export.LastReadHiddenCh.DateOfEmancipation = local.Zero.Date;
        export.LastReadHiddenCh.ArWaivedInsurance = "";
        export.LastReadHiddenCh.FcParentalRights = "";
        export.LastReadHiddenCh.Over18AndInSchool = "";

        // ---------------------------------------------
        // End of code
        // ---------------------------------------------
        UseSiReadChildDetails();
        export.ChPrev.Ssn = export.ChCsePersonsWorkSet.Ssn;

        if (AsChar(export.HiddenAe.Flag) == 'O')
        {
          // ---------------------------------------------
          // This CSE Person is owned by the AE / KSCares
          // and may not be changed by the CSE system.
          // ---------------------------------------------
          var field1 = GetField(export.ChCsePersonsWorkSet, "dob");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.ChCsePersonsWorkSet, "firstName");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.ChCsePersonsWorkSet, "lastName");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.ChCsePersonsWorkSet, "middleInitial");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 = GetField(export.ChCsePersonsWorkSet, "sex");

          field5.Color = "cyan";
          field5.Protected = true;

          var field6 = GetField(export.ChSsnWorkArea, "ssnNumPart1");

          field6.Color = "cyan";
          field6.Protected = true;

          var field7 = GetField(export.ChSsnWorkArea, "ssnNumPart2");

          field7.Color = "cyan";
          field7.Protected = true;

          var field8 = GetField(export.ChSsnWorkArea, "ssnNumPart3");

          field8.Color = "cyan";
          field8.Protected = true;

          var field9 = GetField(export.ChSsnWorkArea, "ssnTextPart1");

          field9.Color = "cyan";
          field9.Protected = true;

          var field10 = GetField(export.ChSsnWorkArea, "ssnTextPart2");

          field10.Color = "cyan";
          field10.Protected = true;

          var field11 = GetField(export.ChSsnWorkArea, "ssnTextPart3");

          field11.Color = "cyan";
          field11.Protected = true;

          var field12 = GetField(export.ChCsePersonsWorkSet, "number");

          field12.Color = "cyan";
          field12.Protected = true;

          // cq64095
          var field13 = GetField(export.ChCsePerson, "tribalCode");

          field13.Color = "cyan";
          field13.Protected = true;
        }

        var field = GetField(export.ChCsePersonsWorkSet, "sex");

        field.Color = "cyan";
        field.Protected = true;

        // ----------------------------------------------------------------------------------------------------------------
        // CQ66150:  Protect Emancipation Date vlaue when child is NOT over 18 
        // and in school.
        // ----------------------------------------------------------------------------------------------------------------
        if (AsChar(export.ChCaseRole.Over18AndInSchool) == 'Y')
        {
          var field1 = GetField(export.ChCaseRole, "dateOfEmancipation");

          field1.Color = "green";
          field1.Highlighting = Highlighting.Underscore;
          field1.Protected = false;
        }
        else
        {
          var field1 = GetField(export.ChCaseRole, "dateOfEmancipation");

          field1.Color = "cyan";
          field1.Protected = true;
        }

        switch(AsChar(local.AbendData.Type1))
        {
          case ' ':
            if (!IsEmpty(export.ChCsePersonsWorkSet.Ssn))
            {
              local.SsnWorkArea.SsnText9 = export.ChCsePersonsWorkSet.Ssn;
              UseCabSsnConvertTextToNum();

              // LSS 08/08/07  - PR312605   changed exit state
              if (IsExitState("INVALID_UNIT_MUST_BE_NUMERIC"))
              {
                export.ChSsnWorkArea.SsnNumPart1 = 0;
                export.ChSsnWorkArea.SsnNumPart2 = 0;
                export.ChSsnWorkArea.SsnNumPart3 = 0;

                var field1 = GetField(export.ChSsnWorkArea, "ssnNumPart3");

                field1.Error = true;

                var field2 = GetField(export.ChSsnWorkArea, "ssnNumPart2");

                field2.Error = true;

                var field3 = GetField(export.ChSsnWorkArea, "ssnNumPart1");

                field3.Error = true;

                var field4 = GetField(export.ChSsnWorkArea, "ssnTextPart3");

                field4.Error = true;

                var field5 = GetField(export.ChSsnWorkArea, "ssnTextPart2");

                field5.Error = true;

                var field6 = GetField(export.ChSsnWorkArea, "ssnTextPart1");

                field6.Error = true;
              }
            }

            // PR160844. Display ssn in the text field.
            if (export.ChSsnWorkArea.SsnNumPart1 == 0 && export
              .ChSsnWorkArea.SsnNumPart2 == 0 && export
              .ChSsnWorkArea.SsnNumPart3 == 0)
            {
              export.ChSsnWorkArea.SsnTextPart1 = "";
              export.ChSsnWorkArea.SsnTextPart2 = "";
              export.ChSsnWorkArea.SsnTextPart3 = "";
            }

            if (IsEmpty(export.ChSsnWorkArea.SsnTextPart1) && IsEmpty
              (export.ChSsnWorkArea.SsnTextPart2) && IsEmpty
              (export.ChSsnWorkArea.SsnTextPart3))
            {
              MoveSsnWorkArea2(export.ChSsnWorkArea, local.SsnWorkArea);
              local.SsnWorkArea.ConvertOption = "2";
              UseCabSsnConvertNumToText();
              export.ChSsnWorkArea.SsnText9 = export.ChCsePersonsWorkSet.Ssn;

              if (export.ChSsnWorkArea.SsnNumPart1 == 0)
              {
                export.ChSsnWorkArea.SsnTextPart1 = "";
              }
              else
              {
                export.ChSsnWorkArea.SsnTextPart1 =
                  Substring(export.ChSsnWorkArea.SsnText9, 1, 3);
              }

              if (export.ChSsnWorkArea.SsnNumPart2 == 0)
              {
                export.ChSsnWorkArea.SsnTextPart2 = "";
              }
              else
              {
                export.ChSsnWorkArea.SsnTextPart2 =
                  Substring(export.ChSsnWorkArea.SsnText9, 4, 2);
              }

              if (export.ChSsnWorkArea.SsnNumPart3 == 0)
              {
                export.ChSsnWorkArea.SsnTextPart3 = "";
              }
              else
              {
                export.ChSsnWorkArea.SsnTextPart3 =
                  Substring(export.ChSsnWorkArea.SsnText9, 6, 4);
              }
            }

            // PR160844. Display ssn in the text field.
            break;
          case 'A':
            return;
          case 'C':
            return;
          default:
            break;
        }

        // CQ64095
        if (!IsEmpty(export.ChCsePerson.TribalCode))
        {
          export.TribalFlag.Flag = "Y";
        }
        else
        {
          export.TribalFlag.Flag = "N";
        }

        // 11/01/07     MWF   PR# 166617 CQ367    Added USE statement and IF 
        // statement for Flag check.
        UseSiAltsBuildAliasAndSsn2();

        if (AsChar(local.ChOccur.Flag) == 'Y')
        {
          export.Alt.Text13 = "Alt SSN/Alias";
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
          var field1 = GetField(export.DisplacedPerson, "displacedInd");

          field1.Color = "cyan";
          field1.Protected = true;
        }
        else
        {
          var field1 = GetField(export.DisplacedPerson, "displacedInd");

          field1.Color = "green";
          field1.Highlighting = Highlighting.Underscore;
          field1.Protected = false;
        }

        // ---------------------------------------------
        // Code added by Raju  Dec 23, 1996:0400 hrs CST
        // ---------------------------------------------
        // ---------------------------------------------
        // Start of code
        // ---------------------------------------------
        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.LastReadHiddenCh.DateOfEmancipation =
            export.ChCaseRole.DateOfEmancipation;
          export.LastReadHiddenCh.ArWaivedInsurance =
            export.ChCaseRole.ArWaivedInsurance ?? "";
          export.LastReadHiddenCh.FcParentalRights =
            export.ChCaseRole.FcParentalRights ?? "";
          export.LastReadHiddenCh.Over18AndInSchool =
            export.ChCaseRole.Over18AndInSchool ?? "";
        }

        // ---------------------------------------------
        // End of code
        // ---------------------------------------------
        // ------------------------------------------------------------------------------------
        // The following code is added to populate 'Foreign Country' 
        // description.
        // ------------------------------------------------------------------------------------
        if (!IsEmpty(export.ChCsePerson.BirthplaceCountry))
        {
          local.Code.CodeName = "FIPS COUNTRY CODE";
          local.CodeValue.Cdvalue = export.ChCsePerson.BirthplaceCountry ?? Spaces
            (10);

          // ---------------------------------------------
          // Call CAB to validate against the 'FIPS COUNTRY CODE' table
          // --------------------------------------------
          UseCabValidateCodeValue3();
          export.WorkForeignCountryDesc.Text40 =
            local.DisplayForeignCountry.Description;
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";

          if (AsChar(local.NoAp.Flag) == 'Y')
          {
            ExitState = "NO_APS_ON_A_CASE";
          }

          if (AsChar(export.ActiveChild.Flag) == 'N')
          {
            ExitState = "DISPLAY_OK_FOR_INACTIVE_CHILD";
          }

          if (AsChar(export.CaseOpen.Flag) == 'N')
          {
            ExitState = "DISPLAY_OK_FOR_CLOSED_CASE";
          }
        }

        break;
      case "UPDATE":
        // ---------------------------------------------
        // Verify that a display has been performed
        // before the update can take place.
        // ---------------------------------------------
        if (!Equal(import.HiddenPreviousCsePersonsWorkSet.Number,
          import.ChCsePersonsWorkSet.Number) || !
          Equal(import.HiddenPreviousCase.Number, import.Case1.Number))
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";

          return;
        }

        // ---------------------------------------------
        // Validate Name
        // ---------------------------------------------
        UseSiCheckName();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field1 = GetField(export.ChCsePersonsWorkSet, "firstName");

          field1.Error = true;

          var field2 = GetField(export.ChCsePersonsWorkSet, "lastName");

          field2.Error = true;

          var field3 = GetField(export.ChCsePersonsWorkSet, "middleInitial");

          field3.Error = true;

          ExitState = "SI0001_INVALID_NAME";
        }

        // ------------------------------------------------------------------------------
        // PR# 180608 / CQ406   10/29/07   LSS
        // Moved the SET statements from the end of the validations
        // to place it with the SSN validation.
        // ------------------------------------------------------------------------------
        local.SsnConcat.Text8 = export.ChSsnWorkArea.SsnTextPart2 + export
          .ChSsnWorkArea.SsnTextPart3;
        export.ChCsePersonsWorkSet.Ssn = export.ChSsnWorkArea.SsnTextPart1 + local
          .SsnConcat.Text8;

        // PR# 180608 / CQ406   10/29/07   LSS   Added verify / ERROR statements
        // for SSN.
        if (Verify(export.ChCsePersonsWorkSet.Ssn, "0123456789") != 0 && !
          IsEmpty(export.ChCsePersonsWorkSet.Ssn))
        {
          var field1 = GetField(export.ChSsnWorkArea, "ssnTextPart1");

          field1.Error = true;

          var field2 = GetField(export.ChSsnWorkArea, "ssnTextPart2");

          field2.Error = true;

          var field3 = GetField(export.ChSsnWorkArea, "ssnTextPart3");

          field3.Error = true;

          ExitState = "LE0000_SSN_CONTAINS_NONNUM";
        }

        if (!IsEmpty(export.ChCsePersonsWorkSet.Ssn) && !
          Equal(export.ChCsePersonsWorkSet.Ssn, export.ChPrev.Ssn))
        {
          // added this check as part of cq7189.
          local.Convert.SsnNum9 =
            (int)StringToNumber(export.ChCsePersonsWorkSet.Ssn);

          if (ReadInvalidSsn())
          {
            var field1 = GetField(export.ChCsePersonsWorkSet, "number");

            field1.Error = true;

            var field2 = GetField(export.ChSsnWorkArea, "ssnTextPart1");

            field2.Error = true;

            var field3 = GetField(export.ChSsnWorkArea, "ssnTextPart2");

            field3.Error = true;

            var field4 = GetField(export.ChSsnWorkArea, "ssnTextPart3");

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
        // Validate Sex
        // ---------------------------------------------
        // 02/22/01 M.L Do not allow space for sex code.
        if (AsChar(export.ChCsePersonsWorkSet.Sex) != 'F' && AsChar
          (export.ChCsePersonsWorkSet.Sex) != 'M')
        {
          var field1 = GetField(export.ChCsePersonsWorkSet, "sex");

          field1.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "INVALID_SEX";
          }
        }

        // ---------------------------------------------
        // Validate Race
        // ---------------------------------------------
        if (!IsEmpty(export.ChCsePerson.Race))
        {
          local.Code.CodeName = "RACE";
          local.CodeValue.Cdvalue = export.ChCsePerson.Race ?? Spaces(10);
          UseCabValidateCodeValue1();

          if (AsChar(local.Invalid.Flag) == 'N')
          {
            var field1 = GetField(export.ChCsePerson, "race");

            field1.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "INVALID_RACE";
            }
          }
        }

        // ---------------------------------------------
        // Validate DOB
        // ---------------------------------------------
        if (!Lt(import.ChCsePersonsWorkSet.Dob, Now().Date))
        {
          var field1 = GetField(export.ChCsePersonsWorkSet, "dob");

          field1.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "INVALID_DATE_OF_BIRTH";
          }
        }

        // 03/09/01 M.L Start
        if (Lt(Now().Date, export.ChCsePerson.DateOfDeath))
        {
          var field1 = GetField(export.ChCsePerson, "dateOfDeath");

          field1.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "INVALID_DATE_OF_DEATH";
          }
        }

        if (Lt(export.ChCsePerson.DateOfDeath, export.ChCsePersonsWorkSet.Dob) &&
          Lt(local.Zero.Date, export.ChCsePerson.DateOfDeath))
        {
          var field1 = GetField(export.ChCsePerson, "dateOfDeath");

          field1.Error = true;

          var field2 = GetField(export.ChCsePersonsWorkSet, "dob");

          field2.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "DOD_LESS_THAN_DOB";
          }
        }

        // CQ64095
        if (!IsEmpty(export.ChCsePerson.TribalCode))
        {
          local.Code.CodeName = "TRIBAL NAME";
          local.CodeValue.Cdvalue = export.ChCsePerson.TribalCode ?? Spaces(10);
          UseCabValidateCodeValue1();

          if (AsChar(local.Invalid.Flag) == 'N')
          {
            var field1 = GetField(export.ChCsePerson, "tribalCode");

            field1.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_INVALID_TRIBAL_NAME";
            }
          }
        }

        if (!IsEmpty(export.ChCsePerson.TribalCode))
        {
          export.TribalFlag.Flag = "Y";
        }
        else
        {
          export.TribalFlag.Flag = "N";
        }

        // 03/09/01 M.L End
        // ---------------------------------------------
        // Emancipation Date
        // ---------------------------------------------
        if (Lt(local.Blank.Date, export.ChCaseRole.DateOfEmancipation))
        {
          if (!Lt(export.ChCsePersonsWorkSet.Dob,
            export.ChCaseRole.DateOfEmancipation))
          {
            var field1 = GetField(export.ChCaseRole, "dateOfEmancipation");

            field1.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "SI0000_EMANCIPATION_DT_LT_DOB";
            }
          }
        }

        // ---------------------------------------------
        // Validate POB State
        // ---------------------------------------------
        if (!IsEmpty(export.ChCsePerson.BirthPlaceState))
        {
          local.Code.CodeName = "STATE CODE";
          local.CodeValue.Cdvalue = export.ChCsePerson.BirthPlaceState ?? Spaces
            (10);
          UseCabValidateCodeValue1();

          if (AsChar(local.Invalid.Flag) == 'N')
          {
            var field1 = GetField(export.ChCsePerson, "birthPlaceState");

            field1.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_INVALID_STATE_CODE";
            }
          }
        }

        // --------------------------------------------
        // Validate the POB Foreign Country.
        // --------------------------------------------
        if (!IsEmpty(import.ChCsePerson.BirthplaceCountry))
        {
          local.Code.CodeName = "FIPS COUNTRY CODE";
          local.CodeValue.Cdvalue = import.ChCsePerson.BirthplaceCountry ?? Spaces
            (10);

          // ---------------------------------------------
          // Call CAB to validate against the 'FIPS COUNTRY CODE' table
          // --------------------------------------------
          UseCabValidateCodeValue2();

          if (AsChar(local.Common.Flag) == 'N')
          {
            var field1 = GetField(export.ChCsePerson, "birthplaceCountry");

            field1.Error = true;

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

        // -------------------------------------------------------------------------------
        //        User can not enter both 'State'  and  'Foreign Country'.
        // --------------------------------------------------------------------------------
        if (!IsEmpty(export.ChCsePerson.BirthPlaceState) && !
          IsEmpty(export.ChCsePerson.BirthplaceCountry))
        {
          var field1 = GetField(export.ChCsePerson, "birthPlaceState");

          field1.Error = true;

          var field2 = GetField(export.ChCsePerson, "birthplaceCountry");

          field2.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_STATE_COUNTRY_ERROR";
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
              var field1 = GetField(export.DisplacedPerson, "displacedInd");

              field1.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "INVALID_INDICATOR_Y_OR_SPACE";
              }

              break;
          }
        }

        // ---------------------------------------------
        // Validate relationship with AR.
        // ---------------------------------------------
        if (!IsEmpty(export.ChCaseRole.RelToAr))
        {
          local.Code.CodeName = "RELATIONSHIP TO AR";
          local.CodeValue.Cdvalue = export.ChCaseRole.RelToAr ?? Spaces(10);
          UseCabValidateCodeValue1();

          if (AsChar(local.Invalid.Flag) == 'N')
          {
            var field1 = GetField(export.ChCaseRole, "relToAr");

            field1.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_INVALID_CODE";
            }
          }
        }

        // ---------------------------------------------
        // Validate Over 18 and In school
        // ---------------------------------------------
        // ----------------------------------------------------------------------------------------------
        // 1. Validate Over 18 and In school valid values "Y", "N" & <SPACE>
        // CQ66150 - Added below listed validation and Emancipation date value 
        // updates
        // 2. Child is Over 18 and In School is changed from "N"/<SPACE? to "Y" 
        // and Emancipation date is
        //    is NOT entered then message will be displayed to the user to enter
        // emancipation date.
        // 3. When Over 18 and In shcool changed from "Y" to N/SPACE then 
        // Emancipation date will be
        //    calculated from date of birth (child date of birth + 18 Years).
        // ----------------------------------------------------------------------------------------------
        if (AsChar(import.ChCaseRole.Over18AndInSchool) != 'Y' && AsChar
          (import.ChCaseRole.Over18AndInSchool) != 'N' && !
          IsEmpty(import.ChCaseRole.Over18AndInSchool))
        {
          var field1 = GetField(export.ChCaseRole, "over18AndInSchool");

          field1.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            // -----------------------------------------------
            // 05/26/99 W.Campbell -  Replaced zd exit states.
            // -----------------------------------------------
            ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";
          }
        }
        else
        {
          // ----------------------------------------------------------------------------------------------------------------
          // Over 18 and in school changed from N/SPACE to Y, enable 
          // emancipation date and set over 18 and in school change
          // flag value.
          // ----------------------------------------------------------------------------------------------------------------
          if (AsChar(import.ChCaseRole.Over18AndInSchool) == 'Y' && AsChar
            (export.LastReadHiddenCh.Over18AndInSchool) != AsChar
            (import.ChCaseRole.Over18AndInSchool) && AsChar
            (export.Over18InSchoolChgFl.Flag) != 'Y')
          {
            export.Over18InSchoolChgFl.Flag = "Y";

            var field1 = GetField(export.ChCaseRole, "dateOfEmancipation");

            field1.Color = "green";
            field1.Highlighting = Highlighting.Underscore;
            field1.Protected = false;
            field1.Focused = true;

            if (Equal(export.ChCaseRole.DateOfEmancipation, local.Blank.Date))
            {
              ExitState = "SI0000_EMANCIPATION_DT_REQUIRED";
            }
            else if (!Lt(AddYears(export.ChCsePersonsWorkSet.Dob, 18),
              export.ChCaseRole.DateOfEmancipation))
            {
              ExitState = "SI0000_EMANCIPATION_DT_LT_DOB_18";
            }
            else
            {
              ExitState = "SI0000_EMANCIPATION_DT_REQUIRED";
            }

            goto Test3;
          }

          // ----------------------------------------------------------------------------------------------------------------
          // When Child is Over 18 and in school then Emancipation Date should 
          // have value.
          // ----------------------------------------------------------------------------------------------------------------
          if (AsChar(import.ChCaseRole.Over18AndInSchool) == 'Y' && Equal
            (export.ChCaseRole.DateOfEmancipation, local.Blank.Date))
          {
            ExitState = "SI0000_EMANCIPATION_DT_REQUIRED";

            goto Test3;
          }

          // ----------------------------------------------------------------------------------------------------------------
          // When Child is in school and over 18, Entered Emancipation date must
          // be greater than DOB plus 18 years
          // ----------------------------------------------------------------------------------------------------------------
          if (AsChar(import.ChCaseRole.Over18AndInSchool) == 'Y' && !
            Equal(import.ChCaseRole.DateOfEmancipation, local.Blank.Date) && !
            Lt(AddYears(export.ChCsePersonsWorkSet.Dob, 18),
            import.ChCaseRole.DateOfEmancipation))
          {
            ExitState = "SI0000_EMANCIPATION_DT_LT_DOB_18";

            goto Test3;
          }

          // ----------------------------------------------------------------------------------------------------------------
          // Over 18 and in school changed from Y to N/SPACE, derive 
          // emancipation date from child date of birth.
          // ----------------------------------------------------------------------------------------------------------------
          if (AsChar(import.ChCaseRole.Over18AndInSchool) != 'Y')
          {
            if (!Equal(export.ChCsePersonsWorkSet.Dob, local.Blank.Date))
            {
              export.ChCaseRole.DateOfEmancipation =
                AddYears(export.ChCsePersonsWorkSet.Dob, 18);
            }
            else
            {
              export.ChCaseRole.DateOfEmancipation = local.Blank.Date;
            }

            var field1 = GetField(export.ChCaseRole, "dateOfEmancipation");

            field1.Color = "cyan";
            field1.Protected = true;
          }

          export.Over18InSchoolChgFl.Flag = "";
        }

Test3:

        // ---------------------------------------------
        // Validate Health Insurance
        // ---------------------------------------------
        if (AsChar(import.ChCaseRole.HealthInsuranceIndicator) != 'Y' && AsChar
          (import.ChCaseRole.HealthInsuranceIndicator) != 'N' && !
          IsEmpty(import.ChCaseRole.HealthInsuranceIndicator))
        {
          var field1 = GetField(export.ChCaseRole, "healthInsuranceIndicator");

          field1.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            // -----------------------------------------------
            // 05/26/99 W.Campbell -  Replaced zd exit states.
            // -----------------------------------------------
            ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";
          }
        }

        // ---------------------------------------------
        // Validate Absence Code
        // ---------------------------------------------
        if (!IsEmpty(import.ChCaseRole.AbsenceReasonCode))
        {
          local.Code.CodeName = "ABSENCE";
          local.CodeValue.Cdvalue = export.ChCaseRole.AbsenceReasonCode ?? Spaces
            (10);
          UseCabValidateCodeValue1();

          if (AsChar(local.Invalid.Flag) == 'N')
          {
            var field1 = GetField(export.ChCaseRole, "absenceReasonCode");

            field1.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "INVALID_ABSENCE_REASON_CODE";
            }
          }
        }

        // ---------------------------------------------
        // Validate AR Waived Insurance
        // ---------------------------------------------
        if (AsChar(import.ChCaseRole.ArWaivedInsurance) != 'Y' && AsChar
          (import.ChCaseRole.ArWaivedInsurance) != 'N' && !
          IsEmpty(import.ChCaseRole.ArWaivedInsurance))
        {
          var field1 = GetField(export.ChCaseRole, "arWaivedInsurance");

          field1.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            // -----------------------------------------------
            // 05/26/99 W.Campbell -  Replaced zd exit states.
            // -----------------------------------------------
            ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";
          }
        }

        // ---------------------------------------------
        // Validate Rights Severed
        // ---------------------------------------------
        if (AsChar(export.ChCaseRole.FcParentalRights) != 'F' && AsChar
          (export.ChCaseRole.FcParentalRights) != 'M' && AsChar
          (export.ChCaseRole.FcParentalRights) != 'B' && AsChar
          (export.ChCaseRole.FcParentalRights) != 'N' && !
          IsEmpty(export.ChCaseRole.FcParentalRights))
        {
          var field1 = GetField(export.ChCaseRole, "fcParentalRights");

          field1.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_INVALID_CODE";
          }
        }

        // ---------------------------------------------
        // Validate Medical Support
        // ---------------------------------------------
        if (AsChar(import.ChCaseRole.MedicalSupportIndicator) != 'Y' && AsChar
          (import.ChCaseRole.MedicalSupportIndicator) != 'N' && !
          IsEmpty(import.ChCaseRole.MedicalSupportIndicator))
        {
          var field1 = GetField(export.ChCaseRole, "medicalSupportIndicator");

          field1.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            // -----------------------------------------------
            // 05/26/99 W.Campbell -  Replaced zd exit states.
            // -----------------------------------------------
            ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        // ----------------------------------------------------------------------
        // 10/29/07     LSS     PR#180608 / CQ406
        // Commented out the SET statements and moved them to the SSN 
        // validation.
        // ----------------------------------------------------------------------
        UseSiUpdateCsePerson();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field1 = GetField(export.ChCsePersonsWorkSet, "number");

          field1.Error = true;

          // ---------------------------------------------
          // 02/09/99 W.Campbell - Added code to USE
          // EAB_ROLLBACK_CICS for correct DB/2
          // update and rollback operation.
          // ---------------------------------------------
          UseEabRollbackCics();

          break;
        }

        UseSiUpdateCaseRole();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          // ---------------------------------------------
          // 02/09/99 W.Campbell - Added code to USE
          // EAB_ROLLBACK_CICS for correct DB/2
          // update and rollback operation.
          // ---------------------------------------------
          UseEabRollbackCics();

          break;
        }

        // ---------------------------------------------
        // 02/09/99 W.Campbell - Moved the USE
        // for ADABAS update until after all the DB/2
        // updates since ADABAS does not
        // provide for rollback.
        // ---------------------------------------------
        if (AsChar(export.HiddenAe.Flag) != 'O')
        {
          UseCabUpdateAdabasPerson();

          if (!IsEmpty(local.AbendData.Type1))
          {
            // ---------------------------------------------
            // 02/09/99 W.Campbell - Added code to USE
            // EAB_ROLLBACK_CICS for correct DB/2
            // update and rollback operation.
            // ---------------------------------------------
            UseEabRollbackCics();

            return;
          }
        }

        // 09/09/05  GVandy  WR00256682  Add indicator to screen to indicate if 
        // individual was displaced by Hurricane Katrina.
        if (AsChar(export.DisplacedPerson.DisplacedInterfaceInd) != 'Y')
        {
          if (ReadDisplacedPerson2())
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
                  if (ReadDisplacedPerson3())
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

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabRollbackCics();

            break;
          }
        }

        export.ChPrev.Ssn = export.ChCsePersonsWorkSet.Ssn;
        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

        break;
      case "FCDS":
        ExitState = "ECO_LNK_2_FOSTER_CARE_CHILD_DETL";

        return;
      case "ALTS":
        // ----------------------------------------------------------------------------------
        //               Per WR# 020259, this code is added.
        //                                                     
        // --  Vithal (05/20/2002)
        // -----------------------------------------------------------------------------------
        ExitState = "ECO_XFR_TO_ALT_SSN_AND_ALIAS";

        return;
      case "CPAT":
        ExitState = "ECO_LNK_TO_CPAT";

        return;
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
      case "INVALID":
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

    // ---------------------------------------------
    // Code added by Raju  Dec 23, 1996:0200 hrs CST
    // The oe cab raise event will be called from
    //   here case of update.
    // ---------------------------------------------
    // ---------------------------------------------
    // Start of code
    // ---------------------------------------------
    if (IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE"))
    {
      local.Infrastructure.UserId = "CHDS";
      local.Infrastructure.EventId = 9;
      local.Infrastructure.BusinessObjectCd = "CAU";
      local.Ch.Number = export.ChCsePersonsWorkSet.Number;
      local.Infrastructure.SituationNumber = 0;

      for(local.NumberOfEvents.TotalInteger = 1; local
        .NumberOfEvents.TotalInteger <= 3; ++local.NumberOfEvents.TotalInteger)
      {
        local.RaiseEventFlag.Text1 = "N";

        if (local.NumberOfEvents.TotalInteger == 1)
        {
          if (!Equal(export.ChCaseRole.DateOfEmancipation, local.Zero.Date))
          {
            if (!Equal(export.LastReadHiddenCh.DateOfEmancipation,
              export.ChCaseRole.DateOfEmancipation))
            {
              local.RaiseEventFlag.Text1 = "Y";
              local.Infrastructure.ReasonCode = "EMANCIPAFUTR";
              local.Infrastructure.ReferenceDate =
                export.ChCaseRole.DateOfEmancipation;

              // -------------------------
              // formation of detail line
              // -------------------------
              local.DetailText30.Text30 = "Child's Emancipation Date :";
              local.Date.Date = local.Infrastructure.ReferenceDate;
              local.DetailText10.Text10 = UseCabConvertDate2String();
              local.Infrastructure.Detail =
                TrimEnd(local.DetailText30.Text30) + local.DetailText10.Text10;
            }
          }
        }
        else if (local.NumberOfEvents.TotalInteger == 2)
        {
          if (!IsEmpty(export.ChCaseRole.ArWaivedInsurance))
          {
            if (AsChar(export.LastReadHiddenCh.ArWaivedInsurance) != AsChar
              (export.ChCaseRole.ArWaivedInsurance) && AsChar
              (export.ChCaseRole.ArWaivedInsurance) == 'Y')
            {
              local.RaiseEventFlag.Text1 = "Y";
              local.Infrastructure.ReasonCode = "ARWAIVHINS";
              local.Infrastructure.Detail =
                "AR has waived Health Insurance for :";
              local.Infrastructure.Detail =
                TrimEnd(local.Infrastructure.Detail) + TrimEnd
                (export.ChCsePersonsWorkSet.FormattedName);
            }
          }
        }
        else if (local.NumberOfEvents.TotalInteger == 3)
        {
          if (!IsEmpty(export.ChCaseRole.FcParentalRights))
          {
            if (AsChar(export.LastReadHiddenCh.FcParentalRights) != AsChar
              (export.ChCaseRole.FcParentalRights) && AsChar
              (export.ChCaseRole.FcParentalRights) != 'N')
            {
              local.RaiseEventFlag.Text1 = "Y";
              local.Infrastructure.ReasonCode = "LEPRNTLRIGHTS";

              switch(AsChar(export.ChCaseRole.FcParentalRights))
              {
                case 'B':
                  local.DetailText30.Text30 = " on Both";

                  break;
                case 'F':
                  local.DetailText30.Text30 = " on Father";

                  break;
                case 'M':
                  local.DetailText30.Text30 = " on Mother";

                  break;
                default:
                  break;
              }

              local.Infrastructure.Detail = "Parental Rights severed for :";
              local.Infrastructure.Detail =
                TrimEnd(local.Infrastructure.Detail) + TrimEnd
                (export.ChCsePersonsWorkSet.FormattedName);
              local.Infrastructure.Detail =
                TrimEnd(local.Infrastructure.Detail) + TrimEnd
                (local.DetailText30.Text30);
            }
          }
        }
        else
        {
          local.RaiseEventFlag.Text1 = "N";
        }

        if (AsChar(local.RaiseEventFlag.Text1) == 'Y')
        {
          UseSiChdsRaiseEvent();

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

      export.LastReadHiddenCh.DateOfEmancipation =
        export.ChCaseRole.DateOfEmancipation;
      export.LastReadHiddenCh.ArWaivedInsurance =
        export.ChCaseRole.ArWaivedInsurance ?? "";
      export.LastReadHiddenCh.FcParentalRights =
        export.ChCaseRole.FcParentalRights ?? "";
      export.LastReadHiddenCh.Over18AndInSchool =
        export.ChCaseRole.Over18AndInSchool ?? "";

      if (AsChar(export.LastReadHiddenCh.Over18AndInSchool) != 'Y')
      {
        var field = GetField(export.ChCaseRole, "dateOfEmancipation");

        field.Color = "cyan";
        field.Protected = true;
      }
      else
      {
        var field = GetField(export.ChCaseRole, "dateOfEmancipation");

        field.Color = "";
        field.Protected = false;
      }
    }

    // ---------------------------------------------
    // End of code
    // ---------------------------------------------
    // ----------------------------------------------------------------
    // 11/2/98  Added code to make sure protected fields stay protected when 
    // there is an error.
    // ----------------------------------------------------------------
    if (!IsExitState("ACO_NN0000_ALL_OK") || !
      IsExitState("CANNOT_MODIFY_CLOSED_CASE") || !
      IsExitState("CANNOT_MODIFY_INACTIVE_CHILD") || !
      IsExitState("NO_APS_ON_A_CASE"))
    {
      // ------------------------------------------------------------
      // Protect fields if required
      // ------------------------------------------------------------
      if (AsChar(export.HiddenAe.Flag) == 'O')
      {
        // ---------------------------------------------
        // This CSE Person is owned by the AE system and
        // may not be changed by the CSE system.
        // ---------------------------------------------
        var field1 = GetField(export.ChCsePersonsWorkSet, "dob");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.ChCsePersonsWorkSet, "firstName");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.ChCsePersonsWorkSet, "lastName");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.ChCsePersonsWorkSet, "middleInitial");

        field4.Color = "cyan";
        field4.Protected = true;

        var field5 = GetField(export.ChCsePersonsWorkSet, "sex");

        field5.Color = "cyan";
        field5.Protected = true;

        var field6 = GetField(export.ChSsnWorkArea, "ssnNumPart1");

        field6.Color = "cyan";
        field6.Protected = true;

        var field7 = GetField(export.ChSsnWorkArea, "ssnNumPart2");

        field7.Color = "cyan";
        field7.Protected = true;

        var field8 = GetField(export.ChSsnWorkArea, "ssnNumPart3");

        field8.Color = "cyan";
        field8.Protected = true;

        var field9 = GetField(export.ChSsnWorkArea, "ssnTextPart1");

        field9.Color = "cyan";
        field9.Protected = true;

        var field10 = GetField(export.ChSsnWorkArea, "ssnTextPart2");

        field10.Color = "cyan";
        field10.Protected = true;

        var field11 = GetField(export.ChSsnWorkArea, "ssnTextPart3");

        field11.Color = "cyan";
        field11.Protected = true;

        var field12 = GetField(export.ChCsePersonsWorkSet, "number");

        field12.Color = "cyan";
        field12.Protected = true;
      }
    }

    // ---------------------------------------------
    // If all processing completed successfully,
    // move all imports to previous exports .
    // --------------------------------------------
    export.HiddenPreviousCase.Number = export.Case1.Number;
    export.HiddenPreviousCsePersonsWorkSet.Number =
      export.ChCsePersonsWorkSet.Number;
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
    target.AbsenceReasonCode = source.AbsenceReasonCode;
    target.PriorMedicalSupport = source.PriorMedicalSupport;
    target.ArWaivedInsurance = source.ArWaivedInsurance;
    target.DateOfEmancipation = source.DateOfEmancipation;
    target.FcAdoptionDisruptionInd = source.FcAdoptionDisruptionInd;
    target.FcApNotified = source.FcApNotified;
    target.FcCincInd = source.FcCincInd;
    target.FcCostOfCare = source.FcCostOfCare;
    target.FcCostOfCareFreq = source.FcCostOfCareFreq;
    target.FcCountyChildRemovedFrom = source.FcCountyChildRemovedFrom;
    target.FcDateOfInitialCustody = source.FcDateOfInitialCustody;
    target.FcInHomeServiceInd = source.FcInHomeServiceInd;
    target.FcIvECaseNumber = source.FcIvECaseNumber;
    target.FcJuvenileCourtOrder = source.FcJuvenileCourtOrder;
    target.FcJuvenileOffenderInd = source.FcJuvenileOffenderInd;
    target.FcLevelOfCare = source.FcLevelOfCare;
    target.FcNextJuvenileCtDt = source.FcNextJuvenileCtDt;
    target.FcOrderEstBy = source.FcOrderEstBy;
    target.FcOtherBenefitInd = source.FcOtherBenefitInd;
    target.FcParentalRights = source.FcParentalRights;
    target.FcPrevPayeeFirstName = source.FcPrevPayeeFirstName;
    target.FcPrevPayeeMiddleInitial = source.FcPrevPayeeMiddleInitial;
    target.FcPlacementDate = source.FcPlacementDate;
    target.FcPlacementName = source.FcPlacementName;
    target.FcPlacementReason = source.FcPlacementReason;
    target.FcPreviousPa = source.FcPreviousPa;
    target.FcPreviousPayeeLastName = source.FcPreviousPayeeLastName;
    target.FcSourceOfFunding = source.FcSourceOfFunding;
    target.FcSrsPayee = source.FcSrsPayee;
    target.FcSsa = source.FcSsa;
    target.FcSsi = source.FcSsi;
    target.FcVaInd = source.FcVaInd;
    target.FcWardsAccount = source.FcWardsAccount;
    target.FcZebInd = source.FcZebInd;
    target.Over18AndInSchool = source.Over18AndInSchool;
    target.ResidesWithArIndicator = source.ResidesWithArIndicator;
    target.SpecialtyArea = source.SpecialtyArea;
    target.RelToAr = source.RelToAr;
  }

  private static void MoveCsePerson1(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
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
    target.BirthplaceCountry = source.BirthplaceCountry;
    target.TextMessageIndicator = source.TextMessageIndicator;
    target.TribalCode = source.TribalCode;
    target.TaxId = source.TaxId;
  }

  private static void MoveCsePerson2(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
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
    target.BornOutOfWedlock = source.BornOutOfWedlock;
    target.CseToEstblPaternity = source.CseToEstblPaternity;
    target.PaternityEstablishedIndicator = source.PaternityEstablishedIndicator;
    target.DatePaternEstab = source.DatePaternEstab;
    target.BirthCertFathersLastName = source.BirthCertFathersLastName;
    target.BirthCertFathersFirstName = source.BirthCertFathersFirstName;
    target.BirthCertFathersMi = source.BirthCertFathersMi;
    target.BirthCertificateSignature = source.BirthCertificateSignature;
    target.BirthplaceCountry = source.BirthplaceCountry;
    target.TextMessageIndicator = source.TextMessageIndicator;
    target.TribalCode = source.TribalCode;
  }

  private static void MoveCsePersonsWorkSet1(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.ReplicationIndicator = source.ReplicationIndicator;
    target.Number = source.Number;
  }

  private static void MoveCsePersonsWorkSet2(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.ReplicationIndicator = source.ReplicationIndicator;
    target.Number = source.Number;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.FormattedName = source.FormattedName;
    target.LastName = source.LastName;
  }

  private static void MoveCsePersonsWorkSet3(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.ReplicationIndicator = source.ReplicationIndicator;
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveCsePersonsWorkSet4(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveDisplacedPerson(DisplacedPerson source,
    DisplacedPerson target)
  {
    target.DisplacedInd = source.DisplacedInd;
    target.DisplacedInterfaceInd = source.DisplacedInterfaceInd;
  }

  private static void MoveGroupToChild(SiReadChildDetails.Export.
    GroupGroup source, Export.ChildGroup target)
  {
    target.Detail.Code = source.Detail.Code;
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

  private static void MoveOffice(Office source, Office target)
  {
    target.TypeCode = source.TypeCode;
    target.Name = source.Name;
  }

  private static void MoveSsnWorkArea1(SsnWorkArea source, SsnWorkArea target)
  {
    target.ConvertOption = source.ConvertOption;
    target.SsnNumPart1 = source.SsnNumPart1;
    target.SsnNumPart2 = source.SsnNumPart2;
    target.SsnNumPart3 = source.SsnNumPart3;
  }

  private static void MoveSsnWorkArea2(SsnWorkArea source, SsnWorkArea target)
  {
    target.SsnText9 = source.SsnText9;
    target.SsnNumPart1 = source.SsnNumPart1;
    target.SsnNumPart2 = source.SsnNumPart2;
    target.SsnNumPart3 = source.SsnNumPart3;
  }

  private string UseCabConvertDate2String()
  {
    var useImport = new CabConvertDate2String.Import();
    var useExport = new CabConvertDate2String.Export();

    useImport.DateWorkArea.Date = local.Date.Date;

    Call(CabConvertDate2String.Execute, useImport, useExport);

    return useExport.TextWorkArea.Text8;
  }

  private void UseCabSsnConvertNumToText()
  {
    var useImport = new CabSsnConvertNumToText.Import();
    var useExport = new CabSsnConvertNumToText.Export();

    MoveSsnWorkArea1(local.SsnWorkArea, useImport.SsnWorkArea);

    Call(CabSsnConvertNumToText.Execute, useImport, useExport);

    MoveSsnWorkArea2(useExport.SsnWorkArea, export.ChSsnWorkArea);
  }

  private void UseCabSsnConvertTextToNum()
  {
    var useImport = new CabSsnConvertTextToNum.Import();
    var useExport = new CabSsnConvertTextToNum.Export();

    useImport.SsnWorkArea.SsnText9 = local.SsnWorkArea.SsnText9;

    Call(CabSsnConvertTextToNum.Execute, useImport, useExport);

    MoveSsnWorkArea2(useExport.SsnWorkArea, export.ChSsnWorkArea);
  }

  private void UseCabUpdateAdabasPerson()
  {
    var useImport = new CabUpdateAdabasPerson.Import();
    var useExport = new CabUpdateAdabasPerson.Export();

    useImport.CsePersonsWorkSet.Assign(export.ChCsePersonsWorkSet);

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

    local.Invalid.Flag = useExport.ValidCode.Flag;
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

    useImport.Case1.Number = export.Next.Number;
    useImport.CsePersonsWorkSet.Number = export.ChCsePersonsWorkSet.Number;
    useImport.CsePerson.Number = export.ChCsePerson.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiAltsBuildAliasAndSsn2()
  {
    var useImport = new SiAltsBuildAliasAndSsn2.Import();
    var useExport = new SiAltsBuildAliasAndSsn2.Export();

    useImport.Ch1.Number = export.ChCsePersonsWorkSet.Number;

    Call(SiAltsBuildAliasAndSsn2.Execute, useImport, useExport);

    local.ChOccur.Flag = useExport.ChOccur.Flag;
  }

  private void UseSiChdsRaiseEvent()
  {
    var useImport = new SiChdsRaiseEvent.Import();
    var useExport = new SiChdsRaiseEvent.Export();

    useImport.Ch.Number = local.Ch.Number;
    MoveInfrastructure1(local.Infrastructure, useImport.Infrastructure);

    Call(SiChdsRaiseEvent.Execute, useImport, useExport);

    MoveInfrastructure2(useExport.Infrastructure, local.Infrastructure);
  }

  private void UseSiCheckName()
  {
    var useImport = new SiCheckName.Import();
    var useExport = new SiCheckName.Export();

    useImport.CsePersonsWorkSet.Assign(export.ChCsePersonsWorkSet);

    Call(SiCheckName.Execute, useImport, useExport);
  }

  private void UseSiReadCaseHeaderInformation()
  {
    var useImport = new SiReadCaseHeaderInformation.Import();
    var useExport = new SiReadCaseHeaderInformation.Export();

    useImport.Case1.Number = export.Case1.Number;
    useImport.Ap.Number = export.Ap.Number;

    Call(SiReadCaseHeaderInformation.Execute, useImport, useExport);

    local.MultipleAps.Flag = useExport.MultipleAps.Flag;
    local.AbendData.Assign(useExport.AbendData);
    export.Ap.Assign(useExport.Ap);
    export.Ar.Assign(useExport.Ar);
    export.CaseOpen.Flag = useExport.CaseOpen.Flag;
  }

  private void UseSiReadChildDetails()
  {
    var useImport = new SiReadChildDetails.Import();
    var useExport = new SiReadChildDetails.Export();

    useImport.Case1.Number = export.Case1.Number;
    useImport.Child.Number = export.ChCsePersonsWorkSet.Number;

    Call(SiReadChildDetails.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    export.ActiveChild.Flag = useExport.ActiveChild.Flag;
    MoveCsePerson2(useExport.ChildCsePerson, export.ChCsePerson);
    export.ChCaseRole.Assign(useExport.ChildCaseRole);
    export.HiddenAe.Flag = useExport.Ae.Flag;
    useExport.Group.CopyTo(export.Child, MoveGroupToChild);
    export.ChCsePersonsWorkSet.Assign(useExport.ChildCsePersonsWorkSet);
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

  private void UseSiRetrieveChildForCase()
  {
    var useImport = new SiRetrieveChildForCase.Import();
    var useExport = new SiRetrieveChildForCase.Export();

    useImport.Case1.Number = export.Case1.Number;
    useImport.CaseOpen.Flag = export.CaseOpen.Flag;

    Call(SiRetrieveChildForCase.Execute, useImport, useExport);

    local.MultipleAps.Flag = useExport.MultipleChildren.Flag;
    MoveCsePersonsWorkSet1(useExport.Child, export.ChCsePersonsWorkSet);
  }

  private void UseSiUpdateCaseRole()
  {
    var useImport = new SiUpdateCaseRole.Import();
    var useExport = new SiUpdateCaseRole.Export();

    useImport.Case1.Number = import.Case1.Number;
    useImport.CsePerson.Number = import.ChCsePerson.Number;
    MoveCaseRole(export.ChCaseRole, useImport.CaseRole);

    Call(SiUpdateCaseRole.Execute, useImport, useExport);
  }

  private void UseSiUpdateCsePerson()
  {
    var useImport = new SiUpdateCsePerson.Import();
    var useExport = new SiUpdateCsePerson.Export();

    useImport.CsePersonsWorkSet.Assign(import.ChCsePersonsWorkSet);
    MoveCsePerson1(import.ChCsePerson, useImport.CsePerson);

    Call(SiUpdateCsePerson.Execute, useImport, useExport);
  }

  private void CreateDisplacedPerson()
  {
    var number = import.ChCsePerson.Number;
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

  private bool ReadDisplacedPerson1()
  {
    entities.DisplacedPerson.Populated = false;

    return Read("ReadDisplacedPerson1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.ChCsePersonsWorkSet.Number);
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
        db.SetString(command, "cspNumber", import.ChCsePerson.Number);
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

  private bool ReadDisplacedPerson3()
  {
    entities.DisplacedPerson.Populated = false;

    return Read("ReadDisplacedPerson3",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.ChCsePerson.Number);
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
        db.SetString(command, "cspNumber", export.ChCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.InvalidSsn.CspNumber = db.GetString(reader, 0);
        entities.InvalidSsn.Ssn = db.GetInt32(reader, 1);
        entities.InvalidSsn.Populated = true;
      });
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
    /// <summary>A ChildGroup group.</summary>
    [Serializable]
    public class ChildGroup
    {
      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public Program Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private Program detail;
    }

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
    /// A value of ChPrev.
    /// </summary>
    [JsonPropertyName("chPrev")]
    public CsePersonsWorkSet ChPrev
    {
      get => chPrev ??= new();
      set => chPrev = value;
    }

    /// <summary>
    /// A value of ActiveChild.
    /// </summary>
    [JsonPropertyName("activeChild")]
    public Common ActiveChild
    {
      get => activeChild ??= new();
      set => activeChild = value;
    }

    /// <summary>
    /// A value of ChSsnWorkArea.
    /// </summary>
    [JsonPropertyName("chSsnWorkArea")]
    public SsnWorkArea ChSsnWorkArea
    {
      get => chSsnWorkArea ??= new();
      set => chSsnWorkArea = value;
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
    /// A value of HiddenApSelected.
    /// </summary>
    [JsonPropertyName("hiddenApSelected")]
    public CsePersonsWorkSet HiddenApSelected
    {
      get => hiddenApSelected ??= new();
      set => hiddenApSelected = value;
    }

    /// <summary>
    /// A value of HiddenChSelected.
    /// </summary>
    [JsonPropertyName("hiddenChSelected")]
    public CsePersonsWorkSet HiddenChSelected
    {
      get => hiddenChSelected ??= new();
      set => hiddenChSelected = value;
    }

    /// <summary>
    /// A value of HiddenSelected.
    /// </summary>
    [JsonPropertyName("hiddenSelected")]
    public CsePersonsWorkSet HiddenSelected
    {
      get => hiddenSelected ??= new();
      set => hiddenSelected = value;
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
    /// A value of PobStPrompt.
    /// </summary>
    [JsonPropertyName("pobStPrompt")]
    public Common PobStPrompt
    {
      get => pobStPrompt ??= new();
      set => pobStPrompt = value;
    }

    /// <summary>
    /// A value of ChPrompt.
    /// </summary>
    [JsonPropertyName("chPrompt")]
    public Common ChPrompt
    {
      get => chPrompt ??= new();
      set => chPrompt = value;
    }

    /// <summary>
    /// A value of RightsPrompt.
    /// </summary>
    [JsonPropertyName("rightsPrompt")]
    public Common RightsPrompt
    {
      get => rightsPrompt ??= new();
      set => rightsPrompt = value;
    }

    /// <summary>
    /// A value of ArRelPrompt.
    /// </summary>
    [JsonPropertyName("arRelPrompt")]
    public Common ArRelPrompt
    {
      get => arRelPrompt ??= new();
      set => arRelPrompt = value;
    }

    /// <summary>
    /// A value of AbsencePrompt.
    /// </summary>
    [JsonPropertyName("absencePrompt")]
    public Common AbsencePrompt
    {
      get => absencePrompt ??= new();
      set => absencePrompt = value;
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
    /// A value of HiddenPreviousCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("hiddenPreviousCsePersonsWorkSet")]
    public CsePersonsWorkSet HiddenPreviousCsePersonsWorkSet
    {
      get => hiddenPreviousCsePersonsWorkSet ??= new();
      set => hiddenPreviousCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of HiddenPreviousCase.
    /// </summary>
    [JsonPropertyName("hiddenPreviousCase")]
    public Case1 HiddenPreviousCase
    {
      get => hiddenPreviousCase ??= new();
      set => hiddenPreviousCase = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of ChCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("chCsePersonsWorkSet")]
    public CsePersonsWorkSet ChCsePersonsWorkSet
    {
      get => chCsePersonsWorkSet ??= new();
      set => chCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ChCsePerson.
    /// </summary>
    [JsonPropertyName("chCsePerson")]
    public CsePerson ChCsePerson
    {
      get => chCsePerson ??= new();
      set => chCsePerson = value;
    }

    /// <summary>
    /// A value of ChCaseRole.
    /// </summary>
    [JsonPropertyName("chCaseRole")]
    public CaseRole ChCaseRole
    {
      get => chCaseRole ??= new();
      set => chCaseRole = value;
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
    /// Gets a value of Child.
    /// </summary>
    [JsonIgnore]
    public Array<ChildGroup> Child => child ??= new(ChildGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Child for json serialization.
    /// </summary>
    [JsonPropertyName("child")]
    [Computed]
    public IList<ChildGroup> Child_Json
    {
      get => child;
      set => Child.Assign(value);
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
    /// A value of LastReadHiddenCh.
    /// </summary>
    [JsonPropertyName("lastReadHiddenCh")]
    public CaseRole LastReadHiddenCh
    {
      get => lastReadHiddenCh ??= new();
      set => lastReadHiddenCh = value;
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
    /// A value of DisplacedPerson.
    /// </summary>
    [JsonPropertyName("displacedPerson")]
    public DisplacedPerson DisplacedPerson
    {
      get => displacedPerson ??= new();
      set => displacedPerson = value;
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
    /// A value of HiddenCh.
    /// </summary>
    [JsonPropertyName("hiddenCh")]
    public CaseRole HiddenCh
    {
      get => hiddenCh ??= new();
      set => hiddenCh = value;
    }

    /// <summary>
    /// A value of Over18InSchoolChgFl.
    /// </summary>
    [JsonPropertyName("over18InSchoolChgFl")]
    public Common Over18InSchoolChgFl
    {
      get => over18InSchoolChgFl ??= new();
      set => over18InSchoolChgFl = value;
    }

    private Common tribalFlag;
    private Common tribalPrompt;
    private CsePersonsWorkSet chPrev;
    private Common activeChild;
    private SsnWorkArea chSsnWorkArea;
    private ServiceProvider serviceProvider;
    private Office office;
    private CsePersonsWorkSet hiddenApSelected;
    private CsePersonsWorkSet hiddenChSelected;
    private CsePersonsWorkSet hiddenSelected;
    private CodeValue selected;
    private Common pobStPrompt;
    private Common chPrompt;
    private Common rightsPrompt;
    private Common arRelPrompt;
    private Common absencePrompt;
    private Common racePrompt;
    private Common apPrompt;
    private Common hiddenAe;
    private CsePersonsWorkSet hiddenPreviousCsePersonsWorkSet;
    private Case1 hiddenPreviousCase;
    private Case1 next;
    private Case1 case1;
    private CsePersonsWorkSet ap;
    private CsePersonsWorkSet ar;
    private CsePersonsWorkSet chCsePersonsWorkSet;
    private CsePerson chCsePerson;
    private CaseRole chCaseRole;
    private Standard standard;
    private Array<ChildGroup> child;
    private NextTranInfo hidden;
    private CaseRole lastReadHiddenCh;
    private Common caseOpen;
    private WorkArea headerLine;
    private Common pobFcPrompt;
    private DisplacedPerson displacedPerson;
    private WorkArea alt;
    private CaseRole hiddenCh;
    private Common over18InSchoolChgFl;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ChildGroup group.</summary>
    [Serializable]
    public class ChildGroup
    {
      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public Program Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private Program detail;
    }

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
    /// A value of ActiveChild.
    /// </summary>
    [JsonPropertyName("activeChild")]
    public Common ActiveChild
    {
      get => activeChild ??= new();
      set => activeChild = value;
    }

    /// <summary>
    /// A value of ChSsnWorkArea.
    /// </summary>
    [JsonPropertyName("chSsnWorkArea")]
    public SsnWorkArea ChSsnWorkArea
    {
      get => chSsnWorkArea ??= new();
      set => chSsnWorkArea = value;
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
    /// A value of HiddenApSelected.
    /// </summary>
    [JsonPropertyName("hiddenApSelected")]
    public CsePersonsWorkSet HiddenApSelected
    {
      get => hiddenApSelected ??= new();
      set => hiddenApSelected = value;
    }

    /// <summary>
    /// A value of HiddenChSelected.
    /// </summary>
    [JsonPropertyName("hiddenChSelected")]
    public CsePersonsWorkSet HiddenChSelected
    {
      get => hiddenChSelected ??= new();
      set => hiddenChSelected = value;
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
    /// A value of PobStPrompt.
    /// </summary>
    [JsonPropertyName("pobStPrompt")]
    public Common PobStPrompt
    {
      get => pobStPrompt ??= new();
      set => pobStPrompt = value;
    }

    /// <summary>
    /// A value of ChPrompt.
    /// </summary>
    [JsonPropertyName("chPrompt")]
    public Common ChPrompt
    {
      get => chPrompt ??= new();
      set => chPrompt = value;
    }

    /// <summary>
    /// A value of RightsPrompt.
    /// </summary>
    [JsonPropertyName("rightsPrompt")]
    public Common RightsPrompt
    {
      get => rightsPrompt ??= new();
      set => rightsPrompt = value;
    }

    /// <summary>
    /// A value of ArRelPrompt.
    /// </summary>
    [JsonPropertyName("arRelPrompt")]
    public Common ArRelPrompt
    {
      get => arRelPrompt ??= new();
      set => arRelPrompt = value;
    }

    /// <summary>
    /// A value of AbsencePrompt.
    /// </summary>
    [JsonPropertyName("absencePrompt")]
    public Common AbsencePrompt
    {
      get => absencePrompt ??= new();
      set => absencePrompt = value;
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
    /// A value of ApPrompt.
    /// </summary>
    [JsonPropertyName("apPrompt")]
    public Common ApPrompt
    {
      get => apPrompt ??= new();
      set => apPrompt = value;
    }

    /// <summary>
    /// A value of HiddenPreviousCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("hiddenPreviousCsePersonsWorkSet")]
    public CsePersonsWorkSet HiddenPreviousCsePersonsWorkSet
    {
      get => hiddenPreviousCsePersonsWorkSet ??= new();
      set => hiddenPreviousCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of HiddenPreviousCase.
    /// </summary>
    [JsonPropertyName("hiddenPreviousCase")]
    public Case1 HiddenPreviousCase
    {
      get => hiddenPreviousCase ??= new();
      set => hiddenPreviousCase = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of ChCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("chCsePersonsWorkSet")]
    public CsePersonsWorkSet ChCsePersonsWorkSet
    {
      get => chCsePersonsWorkSet ??= new();
      set => chCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ChCsePerson.
    /// </summary>
    [JsonPropertyName("chCsePerson")]
    public CsePerson ChCsePerson
    {
      get => chCsePerson ??= new();
      set => chCsePerson = value;
    }

    /// <summary>
    /// A value of ChCaseRole.
    /// </summary>
    [JsonPropertyName("chCaseRole")]
    public CaseRole ChCaseRole
    {
      get => chCaseRole ??= new();
      set => chCaseRole = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// Gets a value of Child.
    /// </summary>
    [JsonIgnore]
    public Array<ChildGroup> Child => child ??= new(ChildGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Child for json serialization.
    /// </summary>
    [JsonPropertyName("child")]
    [Computed]
    public IList<ChildGroup> Child_Json
    {
      get => child;
      set => Child.Assign(value);
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
    /// A value of LastReadHiddenCh.
    /// </summary>
    [JsonPropertyName("lastReadHiddenCh")]
    public CaseRole LastReadHiddenCh
    {
      get => lastReadHiddenCh ??= new();
      set => lastReadHiddenCh = value;
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
    /// A value of WorkForeignCountryDesc.
    /// </summary>
    [JsonPropertyName("workForeignCountryDesc")]
    public WorkArea WorkForeignCountryDesc
    {
      get => workForeignCountryDesc ??= new();
      set => workForeignCountryDesc = value;
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
    /// A value of Alt.
    /// </summary>
    [JsonPropertyName("alt")]
    public WorkArea Alt
    {
      get => alt ??= new();
      set => alt = value;
    }

    /// <summary>
    /// A value of ChPrev.
    /// </summary>
    [JsonPropertyName("chPrev")]
    public CsePersonsWorkSet ChPrev
    {
      get => chPrev ??= new();
      set => chPrev = value;
    }

    /// <summary>
    /// A value of HiddenCh.
    /// </summary>
    [JsonPropertyName("hiddenCh")]
    public CaseRole HiddenCh
    {
      get => hiddenCh ??= new();
      set => hiddenCh = value;
    }

    /// <summary>
    /// A value of Over18InSchoolChgFl.
    /// </summary>
    [JsonPropertyName("over18InSchoolChgFl")]
    public Common Over18InSchoolChgFl
    {
      get => over18InSchoolChgFl ??= new();
      set => over18InSchoolChgFl = value;
    }

    private Common tribalFlag;
    private Common tribalPrompt;
    private Common activeChild;
    private SsnWorkArea chSsnWorkArea;
    private ServiceProvider serviceProvider;
    private Office office;
    private CsePersonsWorkSet hiddenApSelected;
    private CsePersonsWorkSet hiddenChSelected;
    private Code prompt;
    private Common pobStPrompt;
    private Common chPrompt;
    private Common rightsPrompt;
    private Common arRelPrompt;
    private Common absencePrompt;
    private Common racePrompt;
    private Common apPrompt;
    private CsePersonsWorkSet hiddenPreviousCsePersonsWorkSet;
    private Case1 hiddenPreviousCase;
    private Case1 next;
    private Case1 case1;
    private CsePersonsWorkSet ap;
    private CsePersonsWorkSet ar;
    private CsePersonsWorkSet chCsePersonsWorkSet;
    private CsePerson chCsePerson;
    private CaseRole chCaseRole;
    private Common hiddenAe;
    private Standard standard;
    private Array<ChildGroup> child;
    private NextTranInfo hidden;
    private CaseRole lastReadHiddenCh;
    private Common caseOpen;
    private WorkArea headerLine;
    private Common pobFcPrompt;
    private WorkArea workForeignCountryDesc;
    private DisplacedPerson displacedPerson;
    private WorkArea alt;
    private CsePersonsWorkSet chPrev;
    private CaseRole hiddenCh;
    private Common over18InSchoolChgFl;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A ChildGroup group.</summary>
    [Serializable]
    public class ChildGroup
    {
      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public Program Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private Program detail;
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
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public DateWorkArea MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
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
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public DateWorkArea Zero
    {
      get => zero ??= new();
      set => zero = value;
    }

    /// <summary>
    /// A value of NoAp.
    /// </summary>
    [JsonPropertyName("noAp")]
    public Common NoAp
    {
      get => noAp ??= new();
      set => noAp = value;
    }

    /// <summary>
    /// A value of Ch.
    /// </summary>
    [JsonPropertyName("ch")]
    public CsePerson Ch
    {
      get => ch ??= new();
      set => ch = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
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
    /// Gets a value of Child.
    /// </summary>
    [JsonIgnore]
    public Array<ChildGroup> Child => child ??= new(ChildGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Child for json serialization.
    /// </summary>
    [JsonPropertyName("child")]
    [Computed]
    public IList<ChildGroup> Child_Json
    {
      get => child;
      set => Child.Assign(value);
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
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public DateWorkArea Blank
    {
      get => blank ??= new();
      set => blank = value;
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
    /// A value of LastTran.
    /// </summary>
    [JsonPropertyName("lastTran")]
    public Infrastructure LastTran
    {
      get => lastTran ??= new();
      set => lastTran = value;
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
    /// A value of ChOccur.
    /// </summary>
    [JsonPropertyName("chOccur")]
    public Common ChOccur
    {
      get => chOccur ??= new();
      set => chOccur = value;
    }

    private SsnWorkArea convert;
    private DateWorkArea current;
    private DateWorkArea maxDate;
    private TextWorkArea ssnConcat;
    private Common ssnPart;
    private DateWorkArea zero;
    private Common noAp;
    private CsePerson ch;
    private Common multipleAps;
    private AbendData abendData;
    private CsePerson csePerson;
    private Common invalid;
    private CodeValue codeValue;
    private Code code;
    private Common common;
    private SsnWorkArea ssnWorkArea;
    private TextWorkArea textWorkArea;
    private Array<ChildGroup> child;
    private Infrastructure infrastructure;
    private TextWorkArea detailText10;
    private DateWorkArea date;
    private DateWorkArea blank;
    private TextWorkArea detailText30;
    private WorkArea raiseEventFlag;
    private Common numberOfEvents;
    private Infrastructure lastTran;
    private CodeValue displayForeignCountry;
    private Common chOccur;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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

    private InvalidSsn invalidSsn;
    private CsePerson csePerson;
    private DisplacedPerson displacedPerson;
  }
#endregion
}
