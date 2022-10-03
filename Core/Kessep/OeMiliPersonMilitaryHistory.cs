// Program: OE_MILI_PERSON_MILITARY_HISTORY, ID: 371919565, model: 746.
// Short name: SWEMILIP
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
/// A program: OE_MILI_PERSON_MILITARY_HISTORY.
/// </para>
/// <para>
/// Resp - OBLGESTB
/// This Procedure(PRAD) reads MILITARY_SERVICE and returns a group View.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OeMiliPersonMilitaryHistory: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_MILI_PERSON_MILITARY_HISTORY program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeMiliPersonMilitaryHistory(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeMiliPersonMilitaryHistory.
  /// </summary>
  public OeMiliPersonMilitaryHistory(IContext context, Import import,
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
    // ---------------------------------------------
    //   Date 		Developer	Description
    // Jan 1995	Rebecca Grimes 	Initial Development
    // 02/18/95  	Sid     	Rework and Completion
    // 01/11/96  	T.O.Redmond	Add Monthly Pay and BAQ Allotment
    // 				from Income Source and
    // 				add Country/State Prompts
    // 02/06/96	T.O.Redmond	Retrofit for Security
    // 02/23/96	T.O.Redmond	Add Prompt for CSE Person
    // 04/02/96	T.O.Redmond	Add Monthly Pay and BAQ from
    // 				Personal Income Source
    // 04/06/96	T.O.Redmond	Only print BAQ or Monthly Pay for
    // 				current Military SErvice.
    // 06/07/96        Konkader        Print functionality
    // 06/26/96        Welborn         Left Pad EAB Call.
    // 7/3/96		Sid C		String Test fixes.
    // 11/13/96    	R. Marchman	Add new security and next tran.
    // 12/12/96	Raju		event insertion
    // 01/18/97        Raju            on return from HIST, display rec
    // 				which created event
    // 04/28/97	Sid		Problem Fixes.
    // 03/23/98	Siraj Konkader	ZDEL cleanup
    // 12/15/1998	M Ramirez	Revised print process.
    // 12/15/1998	M Ramirez	Changed security to check on CRUD actions only.
    // 01/06/2000	M Ramirez	83300	NEXT TRAN needs to be cleared
    // 					before invoking print process
    // ---------------------------------------------
    // 04/04/00   W.Campbell      Disabled existing call to
    //                            Security Cab and added a
    //                            new call with view matching
    //                            changed to match the export
    //                            views of case and cse_person.
    //                            Work done on WR#000162
    //                            for PRWORA - Family Violence.
    // --------------------------------------------------------
    // ---------------------------------------------
    // This program DISPLAYS the Last Military
    // Service record when enter is hit and displays
    // the previous or the next records on the
    // command NEXT or PREV. The user can add,
    // update or delete the record displayed on the
    // screen, without a select. To support this
    // functionality, the view explicit GROUP PAGES
    // is used which stores the Military Service
    // start date (part identifier with CSE Person).
    // ---------------------------------------------
    // ======================================================================================
    // 03/08/2001                 Vithal Madhira                  WR# 000261
    // When adding or changing the MILI address automatically display that 
    // address on ADDR screen. Also if user DELETE the record on military screen
    // before entering end_date, end_date the address on ADDR screen.
    // 02/20/2002                  Vithal Madhira                  WR# 000261
    // 1. Deleted the 'APO' field from screen.
    // 2. Changed the label from 'City' to 'City/APO/FPO'.
    // 3. Added new edit to check if 'APO' or 'FPO' entered in 'City/APO/FPO' 
    // field, user must enter 'AA' or 'AP' or 'AE' in 'State' field.
    // ======================================================================================
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);
    export.Case1.Number = import.Case1.Number;
    export.CsePerson.Number = import.CsePerson.Number;
    export.Name.Assign(import.Name);
    MoveIncomeSource(import.HincomeSource, export.HincomeSource);

    if (!IsEmpty(import.CsePerson.Number))
    {
      local.TextWorkArea.Text10 = import.CsePerson.Number;
      UseEabPadLeftWithZeros();
      export.CsePerson.Number = local.TextWorkArea.Text10;
    }

    if (!IsEmpty(import.Case1.Number))
    {
      local.TextWorkArea.Text10 = import.Case1.Number;
      UseEabPadLeftWithZeros();
      export.Case1.Number = local.TextWorkArea.Text10;
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

    UseOeCabSetMnemonics();
    UseSpDocSetLiterals();

    // ---------------------------------------------
    // Move Imports to Exports
    // ---------------------------------------------
    export.CsePersonPrompt.SelectChar = import.CsePersonPrompt.SelectChar;
    export.PersonIncomeHistory.Assign(import.PersonIncomeHistory);
    export.HiddenListval.Flag = import.HiddenListval.Flag;

    if (Equal(global.Command, "RETNAME"))
    {
      export.CsePerson.Number = import.FromSelection.Number;
      export.Name.Number = import.FromSelection.Number;
      export.Name.FormattedName = import.FromSelection.FormattedName;
      export.Name.FirstName = import.FromSelection.FirstName;
      export.Name.LastName = import.FromSelection.LastName;
      export.Name.MiddleInitial = import.FromSelection.MiddleInitial;
      export.Name.Ssn = import.FromSelection.Ssn;
      export.CsePersonPrompt.SelectChar = "";
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "RETCOMP"))
    {
      export.CsePerson.Number = import.FromSelection.Number;
      export.Name.Number = import.FromSelection.Number;
      export.Name.FormattedName = import.FromSelection.FormattedName;
      export.Name.FirstName = import.FromSelection.FirstName;
      export.Name.LastName = import.FromSelection.LastName;
      export.Name.MiddleInitial = import.FromSelection.MiddleInitial;
      export.Name.Ssn = import.FromSelection.Ssn;
      export.CsePersonPrompt.SelectChar = "";
      global.Command = "DISPLAY";
    }

    // ************************************************
    // *Move Import views to Export views             *
    // ************************************************
    export.AlreadyDisplayed.Flag = import.AlreadyDisplayed.Flag;
    export.LstCountryCodePrompt.SelectChar =
      import.LstCountryCodePrompt.SelectChar;
    export.LstLocAddrStatePrompt.SelectChar =
      import.LstLocAddrStatePrompt.SelectChar;
    export.MilitaryService.Assign(import.MilitaryService);
    export.WorkH.Assign(import.WorkH);
    MoveStandard(import.Work, export.Work);
    export.MilitaryBranch.Description = import.MilitaryBranch.Description;
    export.MilitaryDutyStatus.Description =
      import.MilitaryDutyStatus.Description;
    export.MilitaryRank.Description = import.MilitaryRank.Description;
    export.WorkListBranch.SelectChar = import.WorkListBranch.SelectChar;
    export.WorkListDutyStatus.SelectChar =
      import.WorkListDutyStation.SelectChar;
    export.WorkListRank.SelectChar = import.WorkListRank.SelectChar;
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (IsEmpty(import.Standard.NextTransaction))
    {
      // If the next tran info is not equal to spaces, this implies that the 
      // user requested a next tran action. Now validate.
    }
    else
    {
      // ************************************************
      // *Flowing from here to Next Tran.               *
      // ************************************************
      export.Hidden.CaseNumber = export.Case1.Number;
      export.Hidden.CsePersonNumber = export.CsePerson.Number;
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

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();
      export.CsePerson.Number = export.Hidden.CsePersonNumber ?? Spaces(10);
      export.Case1.Number = export.Hidden.CaseNumber ?? Spaces(10);

      // --------------------------------------------
      // Date 12/12/96:0900hrs CST
      // Code change effected by : Raju
      // Input from SID : escape removed after
      //    command is display
      // --------------------------------------------
      // ---------------------------------------------
      // Start of Code (Raju 01/20/97:1520 hrs CST)
      // ---------------------------------------------
      if (Equal(export.Hidden.LastTran, "SRPT") || Equal
        (export.Hidden.LastTran, "SRPU"))
      {
        local.LastTran.SystemGeneratedIdentifier =
          export.Hidden.InfrastructureId.GetValueOrDefault();
        UseOeCabReadInfrastructure();
        export.Case1.Number = export.Hidden.CaseNumber ?? Spaces(10);
        export.CsePerson.Number = local.LastTran.CsePersonNumber ?? Spaces(10);
        export.MilitaryService.EffectiveDate = local.LastTran.DenormDate;
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

    // ************************************************
    // *Move Imports to Locals                        *
    // ************************************************
    // ---------------------------------------------
    // Move Hidden Import Views to Hidden Export Views
    // ---------------------------------------------
    export.HcsePerson.Number = import.HcsePerson.Number;
    MoveMilitaryService3(import.HmilitaryService, export.HmilitaryService);
    MoveIncomeSource(import.HincomeSource, export.HincomeSource);

    // ---------------------------------------------
    // Code added by Raju  Dec 12, 1996:1100 hrs CST
    // ---------------------------------------------
    // ---------------------------------------------
    // Start of code
    // ---------------------------------------------
    export.HiddenLastRead.EffectiveDate = import.HiddenLastRead.EffectiveDate;

    // ---------------------------------------------
    // End of code
    // ---------------------------------------------
    // ---------------------------------------------
    // Loop through the group pages moving the group
    // import details to the group export details.
    // ---------------------------------------------
    export.Pages.Index = 0;
    export.Pages.CheckSize();

    for(import.Pages.Index = 0; import.Pages.Index < import.Pages.Count; ++
      import.Pages.Index)
    {
      if (!import.Pages.CheckSize())
      {
        break;
      }

      export.Pages.Update.PagesDetail.EffectiveDate =
        import.Pages.Item.PagesDetail.EffectiveDate;

      ++export.Pages.Index;
      export.Pages.CheckSize();
    }

    import.Pages.CheckIndex();

    if (Equal(global.Command, "RETINCS"))
    {
      export.CsePerson.Number = export.Name.Number;
      local.FromIncs.Flag = "Y";
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "RETCDVL"))
    {
      switch(AsChar(export.HiddenListval.Flag))
      {
        case '1':
          if (AsChar(export.WorkListDutyStatus.SelectChar) == 'S')
          {
            export.MilitaryService.DutyStatusCode = import.List.Cdvalue;
            export.MilitaryDutyStatus.Description = import.List.Description;

            var field3 = GetField(export.MilitaryService, "dutyStatusCode");

            field3.Protected = false;
            field3.Focused = true;

            var field4 = GetField(export.WorkListDutyStatus, "selectChar");

            field4.Protected = false;

            export.WorkListDutyStatus.SelectChar = "";
          }

          break;
        case '2':
          export.MilitaryService.BranchCode = import.List.Cdvalue;
          export.MilitaryBranch.Description = import.List.Description;

          var field1 = GetField(export.MilitaryService, "branchCode");

          field1.Protected = false;
          field1.Focused = true;

          var field2 = GetField(export.WorkListBranch, "selectChar");

          field2.Protected = false;

          export.WorkListBranch.SelectChar = "";

          break;
        case '3':
          if (AsChar(export.WorkListRank.SelectChar) == 'S')
          {
            export.WorkListRank.SelectChar = "";
            export.MilitaryService.Rank = import.List.Cdvalue;
            export.MilitaryRank.Description = import.List.Description;

            var field3 = GetField(export.MilitaryService, "rank");

            field3.Protected = false;
            field3.Focused = true;

            var field4 = GetField(export.WorkListRank, "selectChar");

            field4.Protected = false;
          }

          break;
        case '4':
          if (AsChar(export.LstLocAddrStatePrompt.SelectChar) == 'S')
          {
            export.LstLocAddrStatePrompt.SelectChar = "";
            export.MilitaryService.State = import.List.Cdvalue;

            var field3 = GetField(export.MilitaryService, "state");

            field3.Protected = false;
            field3.Focused = true;

            var field4 = GetField(export.LstLocAddrStatePrompt, "selectChar");

            field4.Protected = false;
          }

          break;
        case '5':
          if (AsChar(export.LstCountryCodePrompt.SelectChar) == 'S')
          {
            export.LstCountryCodePrompt.SelectChar = "";
            export.MilitaryService.Country = import.List.Cdvalue;

            var field3 = GetField(export.MilitaryService, "country");

            field3.Protected = false;
            field3.Focused = true;

            var field4 = GetField(export.LstCountryCodePrompt, "selectChar");

            field4.Protected = false;
          }

          break;
        default:
          break;
      }

      export.HiddenListval.Flag = "";

      if (!IsEmpty(export.CsePersonPrompt.SelectChar))
      {
        var field = GetField(export.CsePersonPrompt, "selectChar");

        field.Error = true;
      }

      if (!IsEmpty(export.WorkListDutyStatus.SelectChar))
      {
        var field = GetField(export.WorkListDutyStatus, "selectChar");

        field.Error = true;
      }

      if (!IsEmpty(export.WorkListBranch.SelectChar))
      {
        var field = GetField(export.WorkListBranch, "selectChar");

        field.Error = true;
      }

      if (!IsEmpty(export.WorkListRank.SelectChar))
      {
        var field = GetField(export.WorkListRank, "selectChar");

        field.Error = true;
      }

      if (!IsEmpty(export.LstLocAddrStatePrompt.SelectChar))
      {
        var field = GetField(export.LstLocAddrStatePrompt, "selectChar");

        field.Error = true;
      }

      if (!IsEmpty(export.LstCountryCodePrompt.SelectChar))
      {
        var field = GetField(export.LstCountryCodePrompt, "selectChar");

        field.Error = true;
      }

      return;
    }

    // mjr---> Changed security to check on CRUD actions only
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "CREATE") || Equal
      (global.Command, "UPDATE") || Equal(global.Command, "DELETE") || Equal
      (global.Command, "PRINT"))
    {
      // --------------------------------------------------------
      // 04/04/00 W.Campbell - Disabled existing call to
      // Security Cab and added a new call with view
      // matching changed to match the export views
      // of case and cse_person.  Work done on
      // WR#000162 for PRWORA - Family Violence.
      // --------------------------------------------------------
      UseScCabTestSecurity();

      // --------------------------------------------------------
      // 04/04/00 W.Campbell - End of change to
      // disable existing call to Security Cab and
      // added a new call with view matching
      // changed to match the export views
      // of case and cse_person.  Work done on
      // WR#000162 for PRWORA - Family Violence.
      // --------------------------------------------------------
      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
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
      case "LIST":
        // ------------------------------------------------
        // CSE PERSON PROMPT
        // ------------------------------------------------
        if (AsChar(export.CsePersonPrompt.SelectChar) != 'S' && !
          IsEmpty(export.CsePersonPrompt.SelectChar))
        {
          var field = GetField(export.CsePersonPrompt, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
        }

        // ------------------------------------------------
        // DUTY STATION PROMPT
        // ------------------------------------------------
        if (!IsEmpty(import.WorkListDutyStation.SelectChar) && AsChar
          (import.WorkListDutyStation.SelectChar) != 'S')
        {
          var field = GetField(export.WorkListDutyStatus, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
        }

        // ------------------------------------------------
        // BRANCH CODE PROMPT
        // ------------------------------------------------
        if (!IsEmpty(import.WorkListBranch.SelectChar) && AsChar
          (import.WorkListBranch.SelectChar) != 'S')
        {
          var field = GetField(export.WorkListBranch, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
        }

        // ------------------------------------------------
        // RANK PROMPT
        // ------------------------------------------------
        if (!IsEmpty(import.WorkListRank.SelectChar) && AsChar
          (import.WorkListRank.SelectChar) != 'S')
        {
          var field = GetField(export.WorkListRank, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
        }

        // ------------------------------------------------
        // STATE PROMPT
        // ------------------------------------------------
        if (!IsEmpty(import.LstLocAddrStatePrompt.SelectChar) && AsChar
          (import.LstLocAddrStatePrompt.SelectChar) != 'S')
        {
          var field = GetField(export.LstLocAddrStatePrompt, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
        }

        // ------------------------------------------------
        // COUNTRY PROMPT
        // ------------------------------------------------
        if (!IsEmpty(import.LstCountryCodePrompt.SelectChar) && AsChar
          (import.LstCountryCodePrompt.SelectChar) != 'S')
        {
          var field = GetField(export.LstCountryCodePrompt, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
        }

        if (IsExitState("ACO_NE0000_INVALID_SELECT_CODE"))
        {
          return;
        }

        if (IsEmpty(import.CsePersonPrompt.SelectChar) && IsEmpty
          (import.WorkListDutyStation.SelectChar) && IsEmpty
          (import.WorkListBranch.SelectChar) && IsEmpty
          (import.WorkListRank.SelectChar) && IsEmpty
          (import.LstLocAddrStatePrompt.SelectChar) && IsEmpty
          (import.LstCountryCodePrompt.SelectChar))
        {
          if (IsEmpty(export.CsePerson.Number))
          {
            var field = GetField(export.CsePersonPrompt, "selectChar");

            field.Error = true;
          }

          if (IsEmpty(export.MilitaryService.DutyStatusCode))
          {
            var field = GetField(export.WorkListDutyStatus, "selectChar");

            field.Error = true;
          }

          if (IsEmpty(export.MilitaryService.BranchCode))
          {
            var field = GetField(export.WorkListBranch, "selectChar");

            field.Error = true;
          }

          if (IsEmpty(export.MilitaryService.Rank))
          {
            var field = GetField(export.WorkListRank, "selectChar");

            field.Error = true;
          }

          if (IsEmpty(export.MilitaryService.State))
          {
            var field = GetField(export.LstLocAddrStatePrompt, "selectChar");

            field.Error = true;
          }

          if (IsEmpty(export.MilitaryService.Country))
          {
            var field = GetField(export.LstCountryCodePrompt, "selectChar");

            field.Error = true;
          }

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          return;
        }

        if (AsChar(import.CsePersonPrompt.SelectChar) == 'S')
        {
          if (IsEmpty(export.Case1.Number))
          {
            ExitState = "ECO_LNK_TO_CSE_NAME_LIST";

            return;
          }
          else
          {
            ExitState = "ECO_LNK_TO_SI_COMP_CASE_COMP";

            return;
          }
        }

        if (AsChar(import.WorkListDutyStation.SelectChar) == 'S')
        {
          export.List.CodeName = local.MilitaryDutyStatus.CodeName;
          export.HiddenListval.Flag = "1";
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

          return;
        }

        if (AsChar(import.WorkListBranch.SelectChar) == 'S')
        {
          export.List.CodeName = local.MilitaryBranch.CodeName;
          export.HiddenListval.Flag = "2";
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

          return;
        }

        if (AsChar(import.WorkListRank.SelectChar) == 'S')
        {
          export.List.CodeName = local.MilitaryRank.CodeName;
          export.HiddenListval.Flag = "3";
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

          return;
        }

        if (AsChar(import.LstLocAddrStatePrompt.SelectChar) == 'S')
        {
          export.List.CodeName = local.State.CodeName;
          export.HiddenListval.Flag = "4";
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

          return;
        }

        if (AsChar(import.LstCountryCodePrompt.SelectChar) == 'S')
        {
          export.List.CodeName = local.Country.CodeName;
          export.HiddenListval.Flag = "5";
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

          return;
        }

        return;
      case "PRINT":
        if (!Equal(export.CsePerson.Number, import.HcsePerson.Number) || IsEmpty
          (import.HcsePerson.Number) || !
          Equal(export.HmilitaryService.City, export.MilitaryService.City) || !
          Equal(export.HmilitaryService.CommandingOfficerFirstName,
          export.MilitaryService.CommandingOfficerFirstName) || !
          Equal(export.HmilitaryService.CommandingOfficerLastName,
          export.MilitaryService.CommandingOfficerLastName) || AsChar
          (export.HmilitaryService.CommandingOfficerMi) != AsChar
          (export.MilitaryService.CommandingOfficerMi) || !
          Equal(export.HmilitaryService.Country, export.MilitaryService.Country) ||
          !
          Equal(export.HmilitaryService.CurrentUsDutyStation,
          export.MilitaryService.CurrentUsDutyStation) || !
          Equal(export.HmilitaryService.EffectiveDate,
          export.MilitaryService.EffectiveDate) || !
          Equal(export.HmilitaryService.State, export.MilitaryService.State) ||
          !
          Equal(export.HmilitaryService.Street1, export.MilitaryService.Street1) ||
          !
          Equal(export.HmilitaryService.Street2, export.MilitaryService.Street2) ||
          !Equal(export.HmilitaryService.Zip3, export.MilitaryService.Zip3) || !
          Equal(export.HmilitaryService.ZipCode4,
          export.MilitaryService.ZipCode4) || !
          Equal(export.HmilitaryService.ZipCode5,
          export.MilitaryService.ZipCode5))
        {
          var field = GetField(export.CsePerson, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_DISPLAY_BEFORE_PRINT";
        }
        else
        {
          local.Document.Name = "MILICLOC";

          // mjr
          // -------------------------------------------
          // 01/06/2000
          // NEXT TRAN needs to be cleared before invoking print process
          // -------------------------------------------------------
          export.Hidden.Assign(local.Null1);
          export.Standard.NextTransaction = "DKEY";
          export.Hidden.MiscText2 = TrimEnd(local.SpDocLiteral.IdDocument) + local
            .Document.Name;

          // mjr
          // ----------------------------------------------------
          // Place identifiers into next tran
          // -------------------------------------------------------
          export.Hidden.MiscText1 = TrimEnd(local.SpDocLiteral.IdPrNumber) + export
            .CsePerson.Number;
          local.WorkArea.Text50 =
            NumberToString(DateToInt(export.MilitaryService.EffectiveDate), 8, 8);
            
          local.WorkArea.Text50 = TrimEnd(local.SpDocLiteral.IdMilitary) + local
            .WorkArea.Text50;
          export.Hidden.MiscText1 = TrimEnd(export.Hidden.MiscText1) + ";" + local
            .WorkArea.Text50;
          UseScCabNextTranPut2();

          // mjr---> DKEY's trancode = SRPD
          //  Can change this to do a READ instead of hardcoding
          global.NextTran = "SRPD PRINT";
        }

        return;
      case "PRINTRET":
        // mjr
        // -----------------------------------------------
        // 12/15/1998
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
          Find(
            String(export.Hidden.MiscText1, NextTranInfo.MiscText1_MaxLength),
          TrimEnd(local.SpDocLiteral.IdPrNumber));

        if (local.Position.Count <= 0)
        {
          break;
        }

        export.CsePerson.Number =
          Substring(export.Hidden.MiscText1, local.Position.Count + 7, 10);
        local.Position.Count =
          Find(
            String(export.Hidden.MiscText1, NextTranInfo.MiscText1_MaxLength),
          TrimEnd(local.SpDocLiteral.IdMilitary));

        if (local.Position.Count <= 0)
        {
          break;
        }

        local.Start.EffectiveDate =
          IntToDate((int)StringToNumber(Substring(
            export.Hidden.MiscText1, 50, local.Position.Count + 9, 8)));
        export.WorkH.PageNumber = 1;
        global.Command = "DISPLAY";

        break;
      case "INCS":
        // ------------------------------------------------
        // FLOW TO INCOME SOURCE DETAILS
        // ------------------------------------------------
        ExitState = "ECO_LNK_TO_INCOME_SOURCE_DETAILS";

        return;
      case "PREV":
        // ---------------------------------------------
        // Scrolling is not allowed without a DISPLAY
        // ---------------------------------------------
        if (!Equal(export.CsePerson.Number, import.HcsePerson.Number) || IsEmpty
          (import.HcsePerson.Number))
        {
          var field = GetField(export.CsePerson, "number");

          field.Error = true;

          ExitState = "OE0169_MILI_DISPLAY";

          return;
        }

        // ---------------------------------------------
        // CHECK the page number for a PREV. SET the
        // start date of the previous page to the local
        // view for a DISPLAY.
        // ---------------------------------------------
        if (import.WorkH.PageNumber <= 1)
        {
          ExitState = "ACO_NE0000_INVALID_BACKWARD";

          return;
        }

        export.WorkH.PageNumber = import.WorkH.PageNumber - 1;

        import.Pages.Index = export.WorkH.PageNumber - 1;
        import.Pages.CheckSize();

        local.Start.EffectiveDate = import.Pages.Item.PagesDetail.EffectiveDate;
        global.Command = "DISPLAY";

        break;
      case "NEXT":
        // ---------------------------------------------
        // Scrolling is not allowed without a DISPLAY
        // ---------------------------------------------
        if (!Equal(export.CsePerson.Number, import.HcsePerson.Number) || IsEmpty
          (import.HcsePerson.Number))
        {
          var field = GetField(export.CsePerson, "number");

          field.Error = true;

          ExitState = "OE0169_MILI_DISPLAY";

          return;
        }

        // ---------------------------------------------
        // CHECK the more sign for a NEXT. SET the
        // start date of the next page to the local
        // view for a DISPLAY.
        // ---------------------------------------------
        if (IsEmpty(import.WorkH.PlusFlag))
        {
          ExitState = "ACO_NE0000_INVALID_FORWARD";

          return;
        }

        if (import.WorkH.PageNumber == Import.PagesGroup.Capacity)
        {
          ExitState = "ACO_NI0000_SEARCH_CRITERIA_REQD";

          return;
        }
        else
        {
          export.WorkH.PageNumber = import.WorkH.PageNumber + 1;

          import.Pages.Index = export.WorkH.PageNumber - 1;
          import.Pages.CheckSize();
        }

        local.Start.EffectiveDate = import.Pages.Item.PagesDetail.EffectiveDate;
        global.Command = "DISPLAY";

        break;
      case "DISPLAY":
        // ************************************************************
        // User wants to keep the case number even after the display is done.
        // ************************************************************
        if (IsEmpty(export.CsePerson.Number))
        {
          var field = GetField(export.CsePerson, "number");

          field.Error = true;

          ExitState = "CSE_PERSON_NO_REQUIRED";

          return;
        }

        if (!IsEmpty(export.Case1.Number))
        {
          UseOeCabCheckCaseMember3();

          switch(AsChar(local.Work.Flag))
          {
            case 'C':
              var field1 = GetField(export.Case1, "number");

              field1.Error = true;

              ExitState = "CASE_NF";

              break;
            case 'P':
              var field2 = GetField(export.CsePerson, "number");

              field2.Error = true;

              ExitState = "CSE_PERSON_NF";

              break;
            case 'R':
              var field3 = GetField(export.Case1, "number");

              field3.Error = true;

              var field4 = GetField(export.CsePerson, "number");

              field4.Error = true;

              ExitState = "OE0000_CASE_MEMBER_NE";

              break;
            default:
              break;
          }

          if (!IsEmpty(local.Work.Flag))
          {
            export.MilitaryService.Assign(local.RefreshMilitaryService);
            MoveMilitaryService2(local.RefreshMilitaryService,
              export.HmilitaryService);
            export.PersonIncomeHistory.Assign(local.RefreshPersonIncomeHistory);
            export.MilitaryBranch.Description =
              Spaces(CodeValue.Description_MaxLength);
            export.MilitaryDutyStatus.Description =
              Spaces(CodeValue.Description_MaxLength);
            export.MilitaryRank.Description =
              Spaces(CodeValue.Description_MaxLength);
            export.WorkH.PlusFlag = "";
            export.WorkH.MinusFlag = "";
            export.Pages.Count = 0;

            return;
          }
        }

        // ---------------------------------------------
        // If the start date is not entered, all the
        // records are to be displayed. POPULATE the
        // group pages with the military service start
        // date for scrolling purposes.
        // If the start date is entered, then move it to
        // the local view for a single record DISPLAY.
        // ---------------------------------------------
        if (Equal(import.MilitaryService.EffectiveDate,
          local.RefreshMilitaryService.EffectiveDate) || !
          Equal(export.HcsePerson.Number, export.CsePerson.Number))
        {
          export.Pages.Count = 0;
          export.WorkH.MinusFlag = "";
          export.WorkH.PlusFlag = "";
          export.WorkH.PageNumber = 1;
          UseOeMiliListMilitaryService();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            var field = GetField(export.CsePerson, "number");

            field.Error = true;

            export.MilitaryService.Assign(local.RefreshMilitaryService);
            MoveMilitaryService2(local.RefreshMilitaryService,
              export.HmilitaryService);
            export.PersonIncomeHistory.Assign(local.RefreshPersonIncomeHistory);
            export.MilitaryBranch.Description =
              Spaces(CodeValue.Description_MaxLength);
            export.MilitaryDutyStatus.Description =
              Spaces(CodeValue.Description_MaxLength);
            export.MilitaryRank.Description =
              Spaces(CodeValue.Description_MaxLength);
            export.WorkH.PlusFlag = "";
            export.WorkH.MinusFlag = "";

            return;
          }
        }
        else
        {
          local.Start.EffectiveDate = import.MilitaryService.EffectiveDate;
        }

        export.WorkH.PageNumber = 1;

        export.Pages.Index = export.WorkH.PageNumber - 1;
        export.Pages.CheckSize();

        if (Lt(local.RefreshMilitaryService.EffectiveDate,
          export.Pages.Item.PagesDetail.EffectiveDate))
        {
          local.Start.EffectiveDate =
            export.Pages.Item.PagesDetail.EffectiveDate;
        }

        break;
      default:
        break;
    }

    // =================================================================
    // Code added to implement new business rules.
    //                                                 
    // Vithal (02/20/2002).
    // =================================================================
    if (Equal(global.Command, "CREATE") || Equal(global.Command, "UPDATE"))
    {
      if (Equal(export.MilitaryService.City, "APO") || Equal
        (export.MilitaryService.City, "FPO"))
      {
        if (Equal(export.MilitaryService.State, "AA") || Equal
          (export.MilitaryService.State, "AE") || Equal
          (export.MilitaryService.State, "AP"))
        {
        }
        else
        {
          var field = GetField(export.MilitaryService, "state");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_STATE_FOR_APO";

          return;
        }
      }

      if (Equal(export.MilitaryService.State, "AA") || Equal
        (export.MilitaryService.State, "AE") || Equal
        (export.MilitaryService.State, "AP"))
      {
        if (Equal(export.MilitaryService.City, "APO") || Equal
          (export.MilitaryService.City, "FPO"))
        {
        }
        else
        {
          var field = GetField(export.MilitaryService, "city");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_CITY_FOR_STAT";

          return;
        }
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "CREATE":
        // ------------------------------------------------
        // Check for OPEN military service record.
        // ------------------------------------------------
        UseOeCheckOpenMilitaryService();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field = GetField(export.CsePerson, "number");

          field.Error = true;

          return;
        }

        // ---------------------------------------------
        // The CSE Person Number must be entered.
        // ---------------------------------------------
        if (IsEmpty(export.CsePerson.Number))
        {
          var field = GetField(export.CsePerson, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          return;
        }
        else
        {
          // *********************************************
          // *Make sure that CSE_PERSON exists on a CASE.*
          // *********************************************
          UseOeCabCheckCaseMember1();

          if (!IsEmpty(local.WorkError.Flag))
          {
            var field = GetField(export.CsePerson, "number");

            field.Error = true;

            return;
          }
        }

        // ---------------------------------------------
        // The expected return to states date cannot be
        // in the past.
        // ---------------------------------------------
        if (Lt(import.MilitaryService.ExpectedReturnDateToStates, Now().Date) &&
          !Equal(import.MilitaryService.ExpectedReturnDateToStates, null))
        {
          var field =
            GetField(export.MilitaryService, "expectedReturnDateToStates");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_DATE";
        }

        // ---------------------------------------------
        // The date the CSE Person leaves the military
        // service cannot be in the future.
        // ---------------------------------------------
        if (!Equal(import.MilitaryService.EndDate, null) && Lt
          (Now().Date, import.MilitaryService.EndDate))
        {
          var field = GetField(export.MilitaryService, "endDate");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_DATE";
        }

        // ---------------------------------------------
        // The start date into the military service is
        // required and cannot be greater than the
        // current date.
        // ---------------------------------------------
        if (Equal(import.MilitaryService.StartDate, local.Initialized.StartDate))
          
        {
          if (Lt(local.Initialized.EndDate, import.MilitaryService.EndDate))
          {
            var field = GetField(export.MilitaryService, "startDate");

            field.Error = true;

            ExitState = "OE0170_START_DATE_REQUIRED";
          }
        }
        else if (Lt(Now().Date, import.MilitaryService.StartDate))
        {
          var field = GetField(export.MilitaryService, "startDate");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_DATE";
        }

        // ---------------------------------------------
        // The start date into the military service
        // cannot be after the date the CSE Person left
        // the military service.
        // ---------------------------------------------
        if (!Equal(import.MilitaryService.EndDate, null) && !
          Lt(import.MilitaryService.StartDate, import.MilitaryService.EndDate))
        {
          var field1 = GetField(export.MilitaryService, "endDate");

          field1.Error = true;

          var field2 = GetField(export.MilitaryService, "startDate");

          field2.Error = true;

          ExitState = "ACO_NE0000_END_LESS_THAN_START";
        }

        if (!IsEmpty(import.MilitaryService.CommandingOfficerFirstName) || !
          IsEmpty(import.MilitaryService.CommandingOfficerLastName) || !
          IsEmpty(import.MilitaryService.CommandingOfficerMi))
        {
          if (IsEmpty(import.MilitaryService.CommandingOfficerFirstName))
          {
            var field =
              GetField(export.MilitaryService, "commandingOfficerFirstName");

            field.Error = true;

            ExitState = "OE0000_COMMANDER_1ST_NAME_REQD";
          }

          if (IsEmpty(import.MilitaryService.CommandingOfficerLastName))
          {
            var field =
              GetField(export.MilitaryService, "commandingOfficerLastName");

            field.Error = true;

            ExitState = "OE0000_COMMANDER_LAST_NAME_REQD";
          }
        }

        if (import.MilitaryService.PhoneAreaCode.GetValueOrDefault() != 0 || import
          .MilitaryService.Phone.GetValueOrDefault() != 0 || !
          IsEmpty(import.MilitaryService.PhoneExt))
        {
          if (import.MilitaryService.PhoneAreaCode.GetValueOrDefault() == 0)
          {
            var field = GetField(export.MilitaryService, "phoneAreaCode");

            field.Error = true;

            ExitState = "CO0000_PHONE_AREA_CODE_REQD";
          }

          if (import.MilitaryService.Phone.GetValueOrDefault() == 0)
          {
            var field = GetField(export.MilitaryService, "phone");

            field.Error = true;

            ExitState = "ACO_NE0000_PHONE_NO_REQD";
          }
        }

        // ---------------------------------------------
        // The address is not mandatory. It can be added
        // at a later time. Check here if address is
        // entered.
        // ---------------------------------------------
        if (!IsEmpty(export.MilitaryService.Country))
        {
          local.CodeValue.Cdvalue = export.MilitaryService.Country ?? Spaces
            (10);
          UseCabValidateCodeValue1();

          if (local.ReturnCode.Count != 0)
          {
            var field = GetField(export.MilitaryService, "country");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_CODE";
          }
        }

        if (!IsEmpty(export.MilitaryService.State))
        {
          local.CodeValue.Cdvalue = export.MilitaryService.State ?? Spaces(10);
          UseCabValidateCodeValue2();

          if (local.ReturnCode.Count != 0)
          {
            var field = GetField(export.MilitaryService, "state");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_STATE_CODE";
          }
        }

        if (!IsEmpty(import.MilitaryService.Street1) || !
          IsEmpty(import.MilitaryService.Street2) || !
          IsEmpty(import.MilitaryService.City) || !
          IsEmpty(import.MilitaryService.State) || !
          IsEmpty(import.MilitaryService.ZipCode5) || !
          IsEmpty(import.MilitaryService.Country))
        {
          // ---------------------------------------------
          // Some data has been keyed in the address
          // fields. Do the edit check for address here.
          // ---------------------------------------------
          if (IsEmpty(import.MilitaryService.Street1))
          {
            var field = GetField(export.MilitaryService, "street1");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }

          if (IsEmpty(import.MilitaryService.City))
          {
            var field = GetField(export.MilitaryService, "city");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }

          if (IsEmpty(import.MilitaryService.State))
          {
            var field = GetField(export.MilitaryService, "state");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }

          if (IsEmpty(import.MilitaryService.Country))
          {
            export.MilitaryService.Country = "US";
          }

          if (IsEmpty(import.MilitaryService.ZipCode5))
          {
            var field = GetField(export.MilitaryService, "zipCode5");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }
          else
          {
            for(local.CheckZip.Count = 1; local.CheckZip.Count <= 5; ++
              local.CheckZip.Count)
            {
              local.CheckZip.SelectChar =
                Substring(import.MilitaryService.ZipCode5, local.CheckZip.Count,
                1);

              if (AsChar(local.CheckZip.SelectChar) < '0' || AsChar
                (local.CheckZip.SelectChar) > '9')
              {
                var field = GetField(export.MilitaryService, "zipCode5");

                field.Error = true;

                ExitState = "FN0000_ZIPCODE_MUST_BE_5_DIGITS";
              }
            }
          }
        }

        if (IsEmpty(import.MilitaryService.ZipCode4))
        {
        }
        else
        {
          if (IsEmpty(import.MilitaryService.ZipCode5))
          {
            var field = GetField(export.MilitaryService, "zipCode5");

            field.Error = true;

            ExitState = "FN0000_ZIPCODE_MUST_BE_5_DIGITS";
          }

          for(local.CheckZip.Count = 1; local.CheckZip.Count <= 4; ++
            local.CheckZip.Count)
          {
            local.CheckZip.SelectChar =
              Substring(import.MilitaryService.ZipCode4, local.CheckZip.Count, 1);
              

            if (AsChar(local.CheckZip.SelectChar) < '0' || AsChar
              (local.CheckZip.SelectChar) > '9')
            {
              var field = GetField(export.MilitaryService, "zipCode4");

              field.Error = true;

              ExitState = "FN0000_ZIP4_MUST_BE_4_NUMERIC";
            }
          }
        }

        // ---------------------------------------------
        // The CSE Person's rank needs to be a valid code.
        // ---------------------------------------------
        if (!IsEmpty(import.MilitaryService.Rank))
        {
          local.CodeValue.Cdvalue = import.MilitaryService.Rank ?? Spaces(10);
          UseCabValidateCodeValue4();

          if (local.WorkError.Count != 0)
          {
            ExitState = "ACO_NE0000_INVALID_CODE";

            var field = GetField(export.MilitaryService, "rank");

            field.Error = true;
          }
        }

        // ---------------------------------------------
        // The branch is required.
        // ---------------------------------------------
        if (IsEmpty(import.MilitaryService.BranchCode))
        {
          var field = GetField(export.MilitaryService, "branchCode");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }
        else
        {
          local.CodeValue.Cdvalue = import.MilitaryService.BranchCode ?? Spaces
            (10);
          UseCabValidateCodeValue3();

          if (local.WorkError.Count != 0)
          {
            ExitState = "ACO_NE0000_INVALID_CODE";

            var field = GetField(export.MilitaryService, "branchCode");

            field.Error = true;
          }
        }

        // ---------------------------------------------
        //     The Effective Date is required.
        // ---------------------------------------------
        if (Equal(import.MilitaryService.EffectiveDate, null))
        {
          export.MilitaryService.EffectiveDate = Now().Date;
          local.Start.EffectiveDate = Now().Date;
        }
        else if (Lt(Now().Date, import.MilitaryService.EffectiveDate))
        {
          var field = GetField(export.MilitaryService, "effectiveDate");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_DATE";
        }

        // ---------------------------------------------
        // The expected discharge date cannot be in the past.
        // ---------------------------------------------
        if (Lt(import.MilitaryService.ExpectedDischargeDate, Now().Date) && !
          Equal(import.MilitaryService.ExpectedDischargeDate, null))
        {
          var field = GetField(export.MilitaryService, "expectedDischargeDate");

          field.Error = true;

          ExitState = "ZD_DATE_LESS_THAN_CURRENT_DATE_1";
        }
        else if (import.MilitaryService.EffectiveDate != null && import
          .MilitaryService.ExpectedDischargeDate != null && !
          Lt(import.MilitaryService.EffectiveDate,
          import.MilitaryService.ExpectedDischargeDate))
        {
          var field = GetField(export.MilitaryService, "expectedDischargeDate");

          field.Error = true;

          ExitState = "OE0172_MILI_EXPECTED_DISCHARGED";
        }

        // ---------------------------------------------
        // The CSE Person's duty status needs to be a
        // valid code.
        // ---------------------------------------------
        if (!IsEmpty(import.MilitaryService.Rank))
        {
          local.CodeValue.Cdvalue = import.MilitaryService.DutyStatusCode ?? Spaces
            (10);
          UseCabValidateCodeValue5();

          if (local.WorkError.Count != 0)
          {
            ExitState = "ACO_NE0000_INVALID_CODE";

            var field = GetField(export.MilitaryService, "dutyStatusCode");

            field.Error = true;
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        UseOeCreateMilitaryService();

        // ---------------------------------------------
        // Code added by Raju  Dec 12, 1996:1100 hrs CST
        // ---------------------------------------------
        // ---------------------------------------------
        // Start of code
        // ---------------------------------------------
        export.HiddenLastRead.EffectiveDate = null;

        // ---------------------------------------------
        // End of code
        // ---------------------------------------------
        if (IsExitState("CSE_PERSON_NF"))
        {
          export.Name.FormattedName = "";

          var field = GetField(export.CsePerson, "number");

          field.Error = true;

          return;
        }
        else if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
        {
          export.HcsePerson.Number = "";

          if (!IsEmpty(export.MilitaryService.BranchCode) && !
            IsEmpty(export.MilitaryService.CurrentUsDutyStation))
          {
            if (!IsEmpty(export.MilitaryService.Street1) || !
              IsEmpty(export.MilitaryService.Street2) || !
              IsEmpty(export.MilitaryService.City) || !
              IsEmpty(export.MilitaryService.State) || !
              IsEmpty(export.MilitaryService.ZipCode5) || !
              IsEmpty(export.MilitaryService.ZipCode4))
            {
              local.FlowData.Command = "CREATE";
              local.FlowData.Flag = "C";
              UseOeFlowAddressFrmMiliToAddr();
            }
          }
        }
        else if (IsExitState("INCOME_SOURCE_NF"))
        {
          ExitState = "OE0000_INCS_MUST_EXIST_FOR_MILI";

          var field = GetField(export.CsePerson, "number");

          field.Error = true;

          return;
        }
        else
        {
          var field1 = GetField(export.MilitaryService, "effectiveDate");

          field1.Error = true;

          var field2 = GetField(export.CsePerson, "number");

          field2.Error = true;

          return;
        }

        break;
      case "DISPLAY":
        // ---------------------------------------------
        // Verify that all mandatory fields for a
        // display have been entered.
        // ---------------------------------------------
        if (IsEmpty(export.CsePerson.Number))
        {
          var field = GetField(export.CsePerson, "number");

          field.Error = true;

          export.MilitaryService.Assign(local.RefreshMilitaryService);
          MoveMilitaryService2(local.RefreshMilitaryService,
            export.HmilitaryService);
          MoveIncomeSource(local.RefreshIncomeSource, export.HincomeSource);
          export.PersonIncomeHistory.Assign(local.RefreshPersonIncomeHistory);
          export.Name.FormattedName = "";
          export.MilitaryBranch.Description =
            Spaces(CodeValue.Description_MaxLength);
          export.MilitaryDutyStatus.Description =
            Spaces(CodeValue.Description_MaxLength);
          export.MilitaryRank.Description =
            Spaces(CodeValue.Description_MaxLength);
          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          return;
        }
        else
        {
          UseOeCabCheckCaseMember2();

          if (!IsEmpty(local.WorkError.Flag))
          {
            var field = GetField(export.CsePerson, "number");

            field.Error = true;

            ExitState = "CSE_PERSON_NF";
            export.MilitaryService.Assign(local.RefreshMilitaryService);
            MoveMilitaryService2(local.RefreshMilitaryService,
              export.HmilitaryService);
            MoveIncomeSource(local.RefreshIncomeSource, export.HincomeSource);
            export.PersonIncomeHistory.Assign(local.RefreshPersonIncomeHistory);
            export.Name.FormattedName = "";
            export.MilitaryBranch.Description =
              Spaces(CodeValue.Description_MaxLength);
            export.MilitaryDutyStatus.Description =
              Spaces(CodeValue.Description_MaxLength);
            export.MilitaryRank.Description =
              Spaces(CodeValue.Description_MaxLength);

            return;
          }
        }

        // ---------------------------------------------
        // SET the subscript of the group export pages
        // and populate it with the Local View.
        // ---------------------------------------------
        export.Pages.Index = export.WorkH.PageNumber - 1;
        export.Pages.CheckSize();

        export.Pages.Update.PagesDetail.EffectiveDate =
          local.Start.EffectiveDate;
        UseOeReadMilitaryService();
        export.AlreadyDisplayed.Flag = "Y";

        if (export.Pages.Index >= 1)
        {
          export.PersonIncomeHistory.MilitaryBaqAllotment = 0;
          export.PersonIncomeHistory.IncomeAmt = 0;
        }

        if (IsExitState("ACO_NI0000_SUCCESSFUL_DISPLAY"))
        {
          export.HcsePerson.Number = export.CsePerson.Number;
          MoveMilitaryService5(export.MilitaryService, export.HmilitaryService);

          // ---------------------------------------------
          // Code added by Raju  Dec 12, 1996:1100 hrs CST
          // ---------------------------------------------
          // ---------------------------------------------
          // Start of code
          // ---------------------------------------------
          export.HiddenLastRead.EffectiveDate =
            export.MilitaryService.EffectiveDate;

          // ---------------------------------------------
          // End of code
          // ---------------------------------------------
          // mjr
          // -----------------------------------------------
          // 12/20/1998
          // Added check for an exitstate returned from Print
          // ------------------------------------------------------------
          local.Position.Count =
            Find(String(
              export.Hidden.MiscText2, NextTranInfo.MiscText2_MaxLength),
            TrimEnd(local.SpDocLiteral.IdDocument));

          if (local.Position.Count <= 0)
          {
            ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
          }
          else
          {
            // mjr---> Determines the appropriate exitstate for the Print 
            // process
            local.WorkArea.Text50 = export.Hidden.MiscText2 ?? Spaces(50);
            UseSpPrintDecodeReturnCode();
            export.Hidden.MiscText2 = local.WorkArea.Text50;
          }
        }
        else
        {
          // ---------------------------------------------
          // Code added by Raju  Dec 12, 1996:1100 hrs CST
          // ---------------------------------------------
          // ---------------------------------------------
          // Start of code
          // ---------------------------------------------
          export.HiddenLastRead.EffectiveDate = null;

          // ---------------------------------------------
          // End of code
          // ---------------------------------------------
          export.HcsePerson.Number = "";

          var field = GetField(export.CsePerson, "number");

          field.Error = true;

          return;
        }

        // ---------------------------------------------
        // SET the  MORE - +  flag here depending in the
        // availability of additional records.
        // ---------------------------------------------
        if (export.WorkH.PageNumber == 1)
        {
          export.WorkH.MinusFlag = "";
        }
        else
        {
          export.WorkH.MinusFlag = "-";
        }

        export.Pages.Index = export.WorkH.PageNumber;
        export.Pages.CheckSize();

        if (export.Pages.Index < Export.PagesGroup.Capacity)
        {
          if (export.Pages.Item.PagesDetail.EffectiveDate != null)
          {
            export.WorkH.PlusFlag = "+";
          }
          else if (Equal(export.Pages.Item.PagesDetail.EffectiveDate, null))
          {
            export.WorkH.PlusFlag = "";
          }
        }

        break;
      case "DELETE":
        // ---------------------------------------------
        // Verify that a display has been performed
        // before the delete can take place.
        // ---------------------------------------------
        if (IsEmpty(import.HcsePerson.Number))
        {
          var field = GetField(export.CsePerson, "number");

          field.Error = true;

          ExitState = "OE0012_DISP_REC_BEFORE_DELETE";

          return;
        }

        // ---------------------------------------------
        // If key value does not equal previous key value, do not allow delete.
        // ---------------------------------------------
        if (!Equal(import.MilitaryService.EffectiveDate,
          import.HmilitaryService.EffectiveDate))
        {
          var field = GetField(export.MilitaryService, "effectiveDate");

          field.Error = true;

          ExitState = "OE0000_KEY_CHANGE_NA";
        }

        if (!Equal(export.CsePerson.Number, import.HcsePerson.Number))
        {
          var field = GetField(export.CsePerson, "number");

          field.Error = true;

          ExitState = "OE0012_DISP_REC_BEFORE_DELETE";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // ---------------------------------------------
        //     CALL Action Block for DELETE.
        // ---------------------------------------------
        UseOeDeleteMilitaryService();

        if (!IsExitState("ACO_NI0000_SUCCESSFUL_DELETE"))
        {
          var field1 = GetField(export.CsePerson, "number");

          field1.Error = true;

          var field2 = GetField(export.MilitaryService, "startDate");

          field2.Error = true;

          return;
        }
        else
        {
          // ----------------------------------------------------------------------------------
          // Also end_date the address on the ADDR screen.
          // ---------------------------------------------------------------------------------
          local.FlowData.Command = "UPDATE";
          local.FlowData.Flag = "D";
          UseOeFlowAddressFrmMiliToAddr();

          // ***************************************************************
          // After a delete, user must use PF2 to refresh the list
          // ***************************************************************
          export.Pages.Count = 0;
          export.WorkH.MinusFlag = "";
          export.WorkH.PlusFlag = "";
          export.WorkH.PageNumber = 1;
          export.HcsePerson.Number = "";
          export.MilitaryService.Assign(local.RefreshMilitaryService);
          MoveMilitaryService2(local.RefreshMilitaryService,
            export.HmilitaryService);
          MoveIncomeSource(local.RefreshIncomeSource, export.HincomeSource);
          export.PersonIncomeHistory.Assign(local.RefreshPersonIncomeHistory);
          export.MilitaryBranch.Description =
            Spaces(CodeValue.Description_MaxLength);
          export.MilitaryDutyStatus.Description =
            Spaces(CodeValue.Description_MaxLength);
          export.MilitaryRank.Description =
            Spaces(CodeValue.Description_MaxLength);

          // ---------------------------------------------
          // Code added by Raju  Dec 12, 1996:1100 hrs CST
          // ---------------------------------------------
          // ---------------------------------------------
          // Start of code
          // ---------------------------------------------
          export.HiddenLastRead.EffectiveDate = null;

          // ---------------------------------------------
          // End of code
          // ---------------------------------------------
        }

        break;
      case "UPDATE":
        // ---------------------------------------------
        // Verify that a display has been performed
        // before the update can take place.
        // ---------------------------------------------
        if (IsEmpty(import.HcsePerson.Number))
        {
          var field = GetField(export.CsePerson, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_MUST_DISPLAY_FIRST";

          return;
        }

        // ---------------------------------------------
        // If key value does not equal previous key
        // value, do not allow update.
        // ---------------------------------------------
        if (!Equal(export.CsePerson.Number, import.HcsePerson.Number))
        {
          var field = GetField(export.CsePerson, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_NEW_KEY_ON_UPDATE";
        }

        if (!Equal(import.MilitaryService.EffectiveDate,
          import.HmilitaryService.EffectiveDate))
        {
          var field = GetField(export.MilitaryService, "effectiveDate");

          field.Error = true;

          ExitState = "ACO_NE0000_NEW_KEY_ON_UPDATE";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // ---------------------------------------------
        // All Non_database validation is done here.
        // ---------------------------------------------
        // ---------------------------------------------
        // The expected return to states date cannot be
        // in the past.
        // ---------------------------------------------
        if (!Equal(import.MilitaryService.ExpectedReturnDateToStates,
          import.HmilitaryService.ExpectedReturnDateToStates))
        {
          if (Lt(import.MilitaryService.ExpectedReturnDateToStates, Now().Date) &&
            !Equal(import.MilitaryService.ExpectedReturnDateToStates, null))
          {
            var field =
              GetField(export.MilitaryService, "expectedReturnDateToStates");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_DATE";
          }
        }

        // ---------------------------------------------
        // The start date into the military service
        // cannot be after the date the CSE Person left
        // the military service.
        // ---------------------------------------------
        // ---------------------------------------------
        // The date the CSE Person leaves the military
        // service cannot be in the future.
        // ---------------------------------------------
        if (!Equal(import.MilitaryService.EndDate, null) && Lt
          (Now().Date, import.MilitaryService.EndDate))
        {
          var field = GetField(export.MilitaryService, "endDate");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_DATE";
        }

        // ---------------------------------------------
        // The start date into the military service is
        // required and cannot be greater than the
        // current date.
        // ---------------------------------------------
        if (Equal(import.MilitaryService.StartDate, local.Initialized.StartDate))
          
        {
          if (Lt(local.Initialized.EndDate, import.MilitaryService.EndDate))
          {
            var field = GetField(export.MilitaryService, "startDate");

            field.Error = true;

            ExitState = "OE0170_START_DATE_REQUIRED";
          }
        }
        else if (Lt(Now().Date, import.MilitaryService.StartDate))
        {
          var field = GetField(export.MilitaryService, "startDate");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_DATE";
        }

        if (!Equal(import.MilitaryService.EndDate, null) && !
          Lt(import.MilitaryService.StartDate, import.MilitaryService.EndDate))
        {
          var field1 = GetField(export.MilitaryService, "endDate");

          field1.Error = true;

          var field2 = GetField(export.MilitaryService, "startDate");

          field2.Error = true;

          ExitState = "ACO_NE0000_END_LESS_THAN_START";
        }

        if (!IsEmpty(import.MilitaryService.CommandingOfficerFirstName) || !
          IsEmpty(import.MilitaryService.CommandingOfficerLastName) || !
          IsEmpty(import.MilitaryService.CommandingOfficerMi))
        {
          if (IsEmpty(import.MilitaryService.CommandingOfficerFirstName))
          {
            var field =
              GetField(export.MilitaryService, "commandingOfficerFirstName");

            field.Error = true;

            ExitState = "OE0000_COMMANDER_1ST_NAME_REQD";
          }

          if (IsEmpty(import.MilitaryService.CommandingOfficerLastName))
          {
            var field =
              GetField(export.MilitaryService, "commandingOfficerLastName");

            field.Error = true;

            ExitState = "OE0000_COMMANDER_LAST_NAME_REQD";
          }
        }

        if (import.MilitaryService.PhoneAreaCode.GetValueOrDefault() != 0 || import
          .MilitaryService.Phone.GetValueOrDefault() != 0 || !
          IsEmpty(import.MilitaryService.PhoneExt))
        {
          if (import.MilitaryService.PhoneAreaCode.GetValueOrDefault() == 0)
          {
            var field = GetField(export.MilitaryService, "phoneAreaCode");

            field.Error = true;

            ExitState = "CO0000_PHONE_AREA_CODE_REQD";
          }

          if (import.MilitaryService.Phone.GetValueOrDefault() == 0)
          {
            var field = GetField(export.MilitaryService, "phone");

            field.Error = true;

            ExitState = "ACO_NE0000_PHONE_NO_REQD";
          }
        }

        // ---------------------------------------------
        // The address is not mandatory. It can be added
        // at a later time. Check here if address is
        // entered.
        // ---------------------------------------------
        if (!IsEmpty(export.MilitaryService.State))
        {
          local.CodeValue.Cdvalue = export.MilitaryService.State ?? Spaces(10);
          UseCabValidateCodeValue2();

          if (local.ReturnCode.Count != 0)
          {
            var field = GetField(export.MilitaryService, "state");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_STATE_CODE";
          }
        }

        if (!IsEmpty(export.MilitaryService.Country))
        {
          local.CodeValue.Cdvalue = export.MilitaryService.Country ?? Spaces
            (10);
          UseCabValidateCodeValue1();

          if (local.ReturnCode.Count != 0)
          {
            var field = GetField(export.MilitaryService, "country");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_CODE";
          }
        }

        if (!IsEmpty(import.MilitaryService.Street1) || !
          IsEmpty(import.MilitaryService.Street2) || !
          IsEmpty(import.MilitaryService.City) || !
          IsEmpty(import.MilitaryService.State) || !
          IsEmpty(import.MilitaryService.ZipCode5) || !
          IsEmpty(import.MilitaryService.Country))
        {
          // ---------------------------------------------
          // Some data has been keyed in the address
          // fields. Do the edit check for address here.
          // ---------------------------------------------
          if (IsEmpty(import.MilitaryService.Street1))
          {
            var field = GetField(export.MilitaryService, "street1");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }

          if (IsEmpty(import.MilitaryService.City))
          {
            var field = GetField(export.MilitaryService, "city");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }

          if (IsEmpty(import.MilitaryService.State))
          {
            var field = GetField(export.MilitaryService, "state");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }

          if (IsEmpty(import.MilitaryService.Country))
          {
            export.MilitaryService.Country = "US";
          }

          if (IsEmpty(import.MilitaryService.ZipCode5))
          {
            var field = GetField(export.MilitaryService, "zipCode5");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }
          else
          {
            for(local.CheckZip.Count = 1; local.CheckZip.Count <= 5; ++
              local.CheckZip.Count)
            {
              local.CheckZip.SelectChar =
                Substring(import.MilitaryService.ZipCode5, local.CheckZip.Count,
                1);

              if (AsChar(local.CheckZip.SelectChar) < '0' || AsChar
                (local.CheckZip.SelectChar) > '9')
              {
                var field = GetField(export.MilitaryService, "zipCode5");

                field.Error = true;

                ExitState = "FN0000_ZIPCODE_MUST_BE_5_DIGITS";
              }
            }
          }
        }

        if (IsEmpty(import.MilitaryService.ZipCode4))
        {
        }
        else
        {
          if (IsEmpty(import.MilitaryService.ZipCode5))
          {
            var field = GetField(export.MilitaryService, "zipCode5");

            field.Error = true;

            ExitState = "FN0000_ZIPCODE_MUST_BE_5_DIGITS";
          }

          local.CheckZip.Count = 0;
          local.CheckZip.SelectChar = "";

          for(local.CheckZip.Count = 1; local.CheckZip.Count <= 4; ++
            local.CheckZip.Count)
          {
            local.CheckZip.SelectChar =
              Substring(import.MilitaryService.ZipCode4, local.CheckZip.Count, 1);
              

            if (AsChar(local.CheckZip.SelectChar) < '0' || AsChar
              (local.CheckZip.SelectChar) > '9')
            {
              var field = GetField(export.MilitaryService, "zipCode4");

              field.Error = true;

              ExitState = "FN0000_ZIP4_MUST_BE_4_NUMERIC";
            }
          }
        }

        // ---------------------------------------------
        // The CSE Person's rank needs to be a valid code.
        // ---------------------------------------------
        if (!IsEmpty(import.MilitaryService.Rank))
        {
          local.CodeValue.Cdvalue = import.MilitaryService.Rank ?? Spaces(10);
          UseCabValidateCodeValue4();

          if (local.WorkError.Count != 0)
          {
            ExitState = "ACO_NE0000_INVALID_CODE";

            var field = GetField(export.MilitaryService, "rank");

            field.Error = true;
          }
        }

        // ---------------------------------------------
        // The branch is required.
        // ---------------------------------------------
        if (IsEmpty(import.MilitaryService.BranchCode))
        {
          var field = GetField(export.MilitaryService, "branchCode");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }
        else
        {
          local.CodeValue.Cdvalue = import.MilitaryService.BranchCode ?? Spaces
            (10);
          UseCabValidateCodeValue3();

          if (local.WorkError.Count != 0)
          {
            ExitState = "ACO_NE0000_INVALID_CODE";

            var field = GetField(export.MilitaryService, "branchCode");

            field.Error = true;
          }
        }

        if (!Equal(import.MilitaryService.ExpectedDischargeDate,
          import.HmilitaryService.ExpectedDischargeDate))
        {
          if (Lt(import.MilitaryService.ExpectedDischargeDate, Now().Date) && !
            Equal(import.MilitaryService.ExpectedDischargeDate, null))
          {
            var field =
              GetField(export.MilitaryService, "expectedDischargeDate");

            field.Error = true;

            ExitState = "ZD_DATE_LESS_THAN_CURRENT_DATE_1";
          }
          else if (!Equal(import.MilitaryService.ExpectedDischargeDate, null) &&
            !
            Lt(import.MilitaryService.EffectiveDate,
            import.MilitaryService.ExpectedDischargeDate))
          {
            var field =
              GetField(export.MilitaryService, "expectedDischargeDate");

            field.Error = true;

            ExitState = "OE0172_MILI_EXPECTED_DISCHARGED";
          }
        }

        // ---------------------------------------------
        // The CSE Person's duty status needs to be a
        // valid code.
        // ---------------------------------------------
        if (!IsEmpty(import.MilitaryService.DutyStatusCode))
        {
          local.CodeValue.Cdvalue = import.MilitaryService.DutyStatusCode ?? Spaces
            (10);
          UseCabValidateCodeValue5();

          if (local.WorkError.Count != 0)
          {
            ExitState = "ACO_NE0000_INVALID_CODE";

            var field = GetField(export.MilitaryService, "dutyStatusCode");

            field.Error = true;
          }
        }

        // ---------------------------------------------
        // The CSE Person Number must be entered.
        // ---------------------------------------------
        if (IsEmpty(export.CsePerson.Number))
        {
          var field = GetField(export.CsePerson, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        UseOeUpdateMilitaryService();

        if (IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE"))
        {
          if (Lt(local.Blank.Date, export.MilitaryService.EndDate) && Equal
            (export.HmilitaryService.EndDate, local.Blank.Date))
          {
            // -------------------------------------------------------------------------
            // The user is updating the existing military address with an  end 
            // date. Update  the corresponding  'cse_person_address' and end
            // date that address with an end_code of 'ME' and source of 'MS'.
            // ------------------------------------------------------------------------
            local.FlowData.Command = "UPDATE";
            local.FlowData.Flag = "U";
          }
          else if (Equal(export.MilitaryService.EndDate, local.Blank.Date) && !
            Equal(export.MilitaryService.CurrentUsDutyStation,
            export.HmilitaryService.CurrentUsDutyStation))
          {
            // -------------------------------------------------------------------------
            // The user is adding the military address now, not end dating the 
            // address . Create the 'cse_person_address' also.
            // ------------------------------------------------------------------------
            if (IsEmpty(export.HmilitaryService.CurrentUsDutyStation) || Lt
              (local.Blank.Date, export.HmilitaryService.EndDate))
            {
              local.FlowData.Command = "CREATE";
              local.FlowData.Flag = "C";
            }
          }
          else if (Equal(export.MilitaryService.EndDate, local.Blank.Date) && (
            !IsEmpty(export.HmilitaryService.CurrentUsDutyStation) && !
            Equal(export.MilitaryService.CurrentUsDutyStation,
            export.HmilitaryService.CurrentUsDutyStation) || !
            Equal(export.MilitaryService.Street1,
            export.HmilitaryService.Street1) || !
            Equal(export.MilitaryService.Street2,
            export.HmilitaryService.Street2) || !
            Equal(export.MilitaryService.City, export.HmilitaryService.City) ||
            !
            Equal(export.MilitaryService.State, export.HmilitaryService.State) ||
            !
            Equal(export.MilitaryService.ZipCode5,
            export.HmilitaryService.ZipCode5) || !
            Equal(export.MilitaryService.ZipCode4,
            export.HmilitaryService.ZipCode4)))
          {
            // -------------------------------------------------------------------------
            // The user is updating the existing military address and not end 
            // dating the address. Update  the corresponding  '
            // cse_person_address' also.
            // ------------------------------------------------------------------------
            local.FlowData.Command = "UPDATE";
            local.FlowData.Flag = "U";
          }

          if (!IsEmpty(local.FlowData.Command))
          {
            UseOeFlowAddressFrmMiliToAddr();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
            }
          }

          MoveMilitaryService5(export.MilitaryService, export.HmilitaryService);
          export.HcsePerson.Number = export.CsePerson.Number;
        }

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }

    // ---------------------------------------------
    // Code added by Raju  Dec 12, 1996:1000 hrs CST
    // The oe cab raise event will be called from
    //   here case of add / update
    // ---------------------------------------------
    // ---------------------------------------------
    // Start of code
    // ---------------------------------------------
    // ------------------------------------------
    // added
    // . local view infrastructure
    //   - event id
    //   - reason code
    // . import , export hidden last read
    //           military service
    //   - start date
    // . local raise event flag work area
    //   - text1
    // This will be set/assigned for each event
    //   raised
    // ------------------------------------------
    if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
    {
      local.Infrastructure.SituationNumber = 0;
      local.Infrastructure.UserId = "MILI";
      local.Infrastructure.EventId = 10;
      local.Infrastructure.ReferenceDate = export.MilitaryService.EffectiveDate;
      local.Infrastructure.BusinessObjectCd = "MIL";
      local.Infrastructure.DenormDate = export.MilitaryService.EffectiveDate;

      // ---------------------------------------------
      // forming the detail line
      // Military Service Effective Date : MMDDCCYY
      // ---------------------------------------------
      local.Infrastructure.Detail = Spaces(Infrastructure.Detail_MaxLength);
      local.Text40.Text40 = "Branch Code : " + (
        export.MilitaryService.BranchCode ?? "");
      local.Infrastructure.Detail = local.Text40.Text40;
      local.Text40.Text40 = " , Military Service Effective Date :";
      local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + local
        .Text40.Text40;
      local.Date.Date = export.MilitaryService.EffectiveDate;
      local.DetailText10.Text10 = UseCabConvertDate2String();
      local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + local
        .DetailText10.Text10;

      // ---------------------------------------------
      // bkrp has the standard code template for
      //   event insertion
      // The plan is to retain the foll views
      //    local number of events
      //    local raise event flag
      // to meet future needs if more than 1 event
      //   is to be raised.
      // Please refer BKRP prad for 'how to do this ?'
      // ---------------------------------------------
      local.RaiseEventFlag.Text1 = "Y";

      if (AsChar(local.RaiseEventFlag.Text1) == 'Y')
      {
        UseOeCabRaiseMiliEvents();

        if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
        {
        }
        else
        {
          UseEabRollbackCics();

          return;
        }
      }

      export.HiddenLastRead.EffectiveDate =
        export.MilitaryService.EffectiveDate;
    }

    // ---------------------------------------------
    // End of code
    // ---------------------------------------------
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Flag = source.Flag;
    target.Command = source.Command;
  }

  private static void MoveExport1ToPages(OeMiliListMilitaryService.Export.
    ExportGroup source, Export.PagesGroup target)
  {
    target.PagesDetail.EffectiveDate = source.PageDetail.EffectiveDate;
  }

  private static void MoveIncomeSource(IncomeSource source, IncomeSource target)
  {
    target.Identifier = source.Identifier;
    target.Type1 = source.Type1;
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.SituationNumber = source.SituationNumber;
    target.EventId = source.EventId;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormDate = source.DenormDate;
    target.UserId = source.UserId;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private static void MoveMilitaryService1(MilitaryService source,
    MilitaryService target)
  {
    target.PhoneAreaCode = source.PhoneAreaCode;
    target.PhoneCountryCode = source.PhoneCountryCode;
    target.PhoneExt = source.PhoneExt;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.State = source.State;
    target.ZipCode5 = source.ZipCode5;
    target.ZipCode4 = source.ZipCode4;
    target.Zip3 = source.Zip3;
    target.Country = source.Country;
    target.Apo = source.Apo;
    target.ExpectedReturnDateToStates = source.ExpectedReturnDateToStates;
    target.OverseasDutyStation = source.OverseasDutyStation;
    target.ExpectedDischargeDate = source.ExpectedDischargeDate;
    target.Phone = source.Phone;
    target.BranchCode = source.BranchCode;
    target.CommandingOfficerLastName = source.CommandingOfficerLastName;
    target.CommandingOfficerFirstName = source.CommandingOfficerFirstName;
    target.CommandingOfficerMi = source.CommandingOfficerMi;
    target.CurrentUsDutyStation = source.CurrentUsDutyStation;
    target.DutyStatusCode = source.DutyStatusCode;
    target.Rank = source.Rank;
    target.EndDate = source.EndDate;
    target.StartDate = source.StartDate;
    target.EffectiveDate = source.EffectiveDate;
  }

  private static void MoveMilitaryService2(MilitaryService source,
    MilitaryService target)
  {
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.State = source.State;
    target.Province = source.Province;
    target.PostalCode = source.PostalCode;
    target.ZipCode5 = source.ZipCode5;
    target.ZipCode4 = source.ZipCode4;
    target.Zip3 = source.Zip3;
    target.Country = source.Country;
    target.ExpectedReturnDateToStates = source.ExpectedReturnDateToStates;
    target.OverseasDutyStation = source.OverseasDutyStation;
    target.ExpectedDischargeDate = source.ExpectedDischargeDate;
    target.BranchCode = source.BranchCode;
    target.CommandingOfficerLastName = source.CommandingOfficerLastName;
    target.CommandingOfficerFirstName = source.CommandingOfficerFirstName;
    target.CommandingOfficerMi = source.CommandingOfficerMi;
    target.CurrentUsDutyStation = source.CurrentUsDutyStation;
    target.DutyStatusCode = source.DutyStatusCode;
    target.Rank = source.Rank;
    target.EndDate = source.EndDate;
    target.StartDate = source.StartDate;
    target.EffectiveDate = source.EffectiveDate;
  }

  private static void MoveMilitaryService3(MilitaryService source,
    MilitaryService target)
  {
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.State = source.State;
    target.Province = source.Province;
    target.PostalCode = source.PostalCode;
    target.ZipCode5 = source.ZipCode5;
    target.ZipCode4 = source.ZipCode4;
    target.Zip3 = source.Zip3;
    target.Country = source.Country;
    target.ExpectedReturnDateToStates = source.ExpectedReturnDateToStates;
    target.OverseasDutyStation = source.OverseasDutyStation;
    target.ExpectedDischargeDate = source.ExpectedDischargeDate;
    target.CommandingOfficerLastName = source.CommandingOfficerLastName;
    target.CommandingOfficerFirstName = source.CommandingOfficerFirstName;
    target.CommandingOfficerMi = source.CommandingOfficerMi;
    target.CurrentUsDutyStation = source.CurrentUsDutyStation;
    target.DutyStatusCode = source.DutyStatusCode;
    target.Rank = source.Rank;
    target.EndDate = source.EndDate;
    target.StartDate = source.StartDate;
    target.EffectiveDate = source.EffectiveDate;
  }

  private static void MoveMilitaryService4(MilitaryService source,
    MilitaryService target)
  {
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.State = source.State;
    target.ZipCode5 = source.ZipCode5;
    target.ZipCode4 = source.ZipCode4;
    target.Zip3 = source.Zip3;
    target.Country = source.Country;
    target.Apo = source.Apo;
    target.ExpectedReturnDateToStates = source.ExpectedReturnDateToStates;
    target.OverseasDutyStation = source.OverseasDutyStation;
    target.ExpectedDischargeDate = source.ExpectedDischargeDate;
    target.Phone = source.Phone;
    target.BranchCode = source.BranchCode;
    target.CommandingOfficerLastName = source.CommandingOfficerLastName;
    target.CommandingOfficerFirstName = source.CommandingOfficerFirstName;
    target.CommandingOfficerMi = source.CommandingOfficerMi;
    target.CurrentUsDutyStation = source.CurrentUsDutyStation;
    target.DutyStatusCode = source.DutyStatusCode;
    target.Rank = source.Rank;
    target.EndDate = source.EndDate;
    target.StartDate = source.StartDate;
    target.EffectiveDate = source.EffectiveDate;
  }

  private static void MoveMilitaryService5(MilitaryService source,
    MilitaryService target)
  {
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.State = source.State;
    target.ZipCode5 = source.ZipCode5;
    target.ZipCode4 = source.ZipCode4;
    target.Zip3 = source.Zip3;
    target.Country = source.Country;
    target.ExpectedReturnDateToStates = source.ExpectedReturnDateToStates;
    target.OverseasDutyStation = source.OverseasDutyStation;
    target.ExpectedDischargeDate = source.ExpectedDischargeDate;
    target.BranchCode = source.BranchCode;
    target.CommandingOfficerLastName = source.CommandingOfficerLastName;
    target.CommandingOfficerFirstName = source.CommandingOfficerFirstName;
    target.CommandingOfficerMi = source.CommandingOfficerMi;
    target.CurrentUsDutyStation = source.CurrentUsDutyStation;
    target.DutyStatusCode = source.DutyStatusCode;
    target.Rank = source.Rank;
    target.EndDate = source.EndDate;
    target.StartDate = source.StartDate;
    target.EffectiveDate = source.EffectiveDate;
  }

  private static void MoveSpDocLiteral(SpDocLiteral source, SpDocLiteral target)
  {
    target.IdDocument = source.IdDocument;
    target.IdMilitary = source.IdMilitary;
    target.IdPrNumber = source.IdPrNumber;
    target.IdTribunal = source.IdTribunal;
  }

  private static void MoveStandard(Standard source, Standard target)
  {
    target.Command = source.Command;
    target.DeleteConfirmation = source.DeleteConfirmation;
  }

  private string UseCabConvertDate2String()
  {
    var useImport = new CabConvertDate2String.Import();
    var useExport = new CabConvertDate2String.Export();

    useImport.DateWorkArea.Date = local.Date.Date;

    Call(CabConvertDate2String.Execute, useImport, useExport);

    return useExport.TextWorkArea.Text8;
  }

  private void UseCabValidateCodeValue1()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Country.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ReturnCode.Count = useExport.ReturnCode.Count;
  }

  private void UseCabValidateCodeValue2()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.State.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ReturnCode.Count = useExport.ReturnCode.Count;
  }

  private void UseCabValidateCodeValue3()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.MilitaryBranch.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.WorkError.Count = useExport.ReturnCode.Count;
    export.MilitaryBranch.Description = useExport.CodeValue.Description;
  }

  private void UseCabValidateCodeValue4()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.MilitaryRank.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.WorkError.Count = useExport.ReturnCode.Count;
    export.MilitaryRank.Description = useExport.CodeValue.Description;
  }

  private void UseCabValidateCodeValue5()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.MilitaryDutyStatus.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.WorkError.Count = useExport.ReturnCode.Count;
    export.MilitaryDutyStatus.Description = useExport.CodeValue.Description;
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

  private void UseOeCabCheckCaseMember1()
  {
    var useImport = new OeCabCheckCaseMember.Import();
    var useExport = new OeCabCheckCaseMember.Export();

    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(OeCabCheckCaseMember.Execute, useImport, useExport);

    local.WorkError.Flag = useExport.Work.Flag;
    export.Name.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseOeCabCheckCaseMember2()
  {
    var useImport = new OeCabCheckCaseMember.Import();
    var useExport = new OeCabCheckCaseMember.Export();

    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(OeCabCheckCaseMember.Execute, useImport, useExport);

    local.WorkError.Flag = useExport.Work.Flag;
    export.Name.Assign(useExport.CsePersonsWorkSet);
    export.CsePerson.Number = useExport.CsePerson.Number;
  }

  private void UseOeCabCheckCaseMember3()
  {
    var useImport = new OeCabCheckCaseMember.Import();
    var useExport = new OeCabCheckCaseMember.Export();

    useImport.Case1.Number = export.Case1.Number;
    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(OeCabCheckCaseMember.Execute, useImport, useExport);

    local.Work.Flag = useExport.Work.Flag;
  }

  private void UseOeCabRaiseMiliEvents()
  {
    var useImport = new OeCabRaiseMiliEvents.Import();
    var useExport = new OeCabRaiseMiliEvents.Export();

    MoveInfrastructure(local.Infrastructure, useImport.Infrastructure);
    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(OeCabRaiseMiliEvents.Execute, useImport, useExport);

    MoveInfrastructure(useExport.Infrastructure, local.Infrastructure);
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

    local.Country.CodeName = useExport.Country.CodeName;
    local.State.CodeName = useExport.State.CodeName;
    local.MilitaryBranch.CodeName = useExport.MilitaryBranch.CodeName;
    local.MilitaryRank.CodeName = useExport.MilitaryRank.CodeName;
    local.MilitaryDutyStatus.CodeName = useExport.MilitaryDutyStatus.CodeName;
  }

  private void UseOeCheckOpenMilitaryService()
  {
    var useImport = new OeCheckOpenMilitaryService.Import();
    var useExport = new OeCheckOpenMilitaryService.Export();

    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(OeCheckOpenMilitaryService.Execute, useImport, useExport);
  }

  private void UseOeCreateMilitaryService()
  {
    var useImport = new OeCreateMilitaryService.Import();
    var useExport = new OeCreateMilitaryService.Export();

    MoveIncomeSource(import.HincomeSource, useImport.IncomeSource);
    MoveMilitaryService1(export.MilitaryService, useImport.MilitaryService);
    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(OeCreateMilitaryService.Execute, useImport, useExport);

    export.MilitaryService.Assign(useExport.MilitaryService);
    export.CsePerson.Number = useExport.CsePerson.Number;
    export.PersonIncomeHistory.Assign(useExport.PersonIncomeHistory);
  }

  private void UseOeDeleteMilitaryService()
  {
    var useImport = new OeDeleteMilitaryService.Import();
    var useExport = new OeDeleteMilitaryService.Export();

    useImport.CsePerson.Number = export.CsePerson.Number;
    useImport.MilitaryService.EffectiveDate =
      import.MilitaryService.EffectiveDate;

    Call(OeDeleteMilitaryService.Execute, useImport, useExport);

    MoveMilitaryService4(useExport.MilitaryService, export.MilitaryService);
  }

  private void UseOeFlowAddressFrmMiliToAddr()
  {
    var useImport = new OeFlowAddressFrmMiliToAddr.Import();
    var useExport = new OeFlowAddressFrmMiliToAddr.Export();

    useImport.CsePerson.Number = export.CsePerson.Number;
    MoveMilitaryService1(export.MilitaryService, useImport.MilitaryService);
    MoveCommon(local.FlowData, useImport.FlowData);

    Call(OeFlowAddressFrmMiliToAddr.Execute, useImport, useExport);
  }

  private void UseOeMiliListMilitaryService()
  {
    var useImport = new OeMiliListMilitaryService.Import();
    var useExport = new OeMiliListMilitaryService.Export();

    useImport.FromIncs.Flag = local.FromIncs.Flag;
    useImport.IncomeSource.Identifier = export.HincomeSource.Identifier;
    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(OeMiliListMilitaryService.Execute, useImport, useExport);

    export.Name.Assign(useExport.Name);
    useExport.Export1.CopyTo(export.Pages, MoveExport1ToPages);
    export.CsePerson.Number = useExport.Group.Number;
  }

  private void UseOeReadMilitaryService()
  {
    var useImport = new OeReadMilitaryService.Import();
    var useExport = new OeReadMilitaryService.Export();

    useImport.MilitaryService.EffectiveDate = local.Start.EffectiveDate;
    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(OeReadMilitaryService.Execute, useImport, useExport);

    export.PersonIncomeHistory.Assign(useExport.PersonIncomeHistory);
    MoveIncomeSource(useExport.IncomeSource, export.HincomeSource);
    export.MilitaryDutyStatus.Description =
      useExport.MilitaryDutyStatus.Description;
    export.MilitaryBranch.Description = useExport.MilitaryBranch.Description;
    export.MilitaryRank.Description = useExport.MilitaryRank.Description;
    local.Next.EffectiveDate = useExport.Next.EffectiveDate;
    export.WorkH.PlusFlag = useExport.WorkH.PlusFlag;
    export.MilitaryService.Assign(useExport.MilitaryService);
    export.CsePerson.Number = useExport.CsePerson.Number;
  }

  private void UseOeUpdateMilitaryService()
  {
    var useImport = new OeUpdateMilitaryService.Import();
    var useExport = new OeUpdateMilitaryService.Export();

    MoveMilitaryService1(export.MilitaryService, useImport.MilitaryService);
    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(OeUpdateMilitaryService.Execute, useImport, useExport);

    export.MilitaryService.Assign(useExport.MilitaryService);
    export.CsePerson.Number = useExport.CsePerson.Number;
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.Hidden.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut1()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(export.Hidden);

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabNextTranPut2()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.NextTranInfo.Assign(export.Hidden);
    useImport.Standard.NextTransaction = export.Standard.NextTransaction;

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

    useImport.Case1.Number = export.Case1.Number;
    useImport.CsePerson.Number = export.CsePerson.Number;

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

    useImport.WorkArea.Text50 = local.WorkArea.Text50;

    Call(SpPrintDecodeReturnCode.Execute, useImport, useExport);

    local.WorkArea.Text50 = useExport.WorkArea.Text50;
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
    /// <summary>A PagesGroup group.</summary>
    [Serializable]
    public class PagesGroup
    {
      /// <summary>
      /// A value of PagesDetail.
      /// </summary>
      [JsonPropertyName("pagesDetail")]
      public MilitaryService PagesDetail
      {
        get => pagesDetail ??= new();
        set => pagesDetail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 15;

      private MilitaryService pagesDetail;
    }

    /// <summary>
    /// A value of HiddenListval.
    /// </summary>
    [JsonPropertyName("hiddenListval")]
    public Common HiddenListval
    {
      get => hiddenListval ??= new();
      set => hiddenListval = value;
    }

    /// <summary>
    /// A value of HiddenLastRead.
    /// </summary>
    [JsonPropertyName("hiddenLastRead")]
    public MilitaryService HiddenLastRead
    {
      get => hiddenLastRead ??= new();
      set => hiddenLastRead = value;
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
    /// A value of HincomeSource.
    /// </summary>
    [JsonPropertyName("hincomeSource")]
    public IncomeSource HincomeSource
    {
      get => hincomeSource ??= new();
      set => hincomeSource = value;
    }

    /// <summary>
    /// A value of Name.
    /// </summary>
    [JsonPropertyName("name")]
    public CsePersonsWorkSet Name
    {
      get => name ??= new();
      set => name = value;
    }

    /// <summary>
    /// A value of AlreadyDisplayed.
    /// </summary>
    [JsonPropertyName("alreadyDisplayed")]
    public Common AlreadyDisplayed
    {
      get => alreadyDisplayed ??= new();
      set => alreadyDisplayed = value;
    }

    /// <summary>
    /// A value of PersonIncomeHistory.
    /// </summary>
    [JsonPropertyName("personIncomeHistory")]
    public PersonIncomeHistory PersonIncomeHistory
    {
      get => personIncomeHistory ??= new();
      set => personIncomeHistory = value;
    }

    /// <summary>
    /// A value of FromSelection.
    /// </summary>
    [JsonPropertyName("fromSelection")]
    public CsePersonsWorkSet FromSelection
    {
      get => fromSelection ??= new();
      set => fromSelection = value;
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
    /// A value of LstLocAddrStatePrompt.
    /// </summary>
    [JsonPropertyName("lstLocAddrStatePrompt")]
    public Common LstLocAddrStatePrompt
    {
      get => lstLocAddrStatePrompt ??= new();
      set => lstLocAddrStatePrompt = value;
    }

    /// <summary>
    /// A value of LstCountryCodePrompt.
    /// </summary>
    [JsonPropertyName("lstCountryCodePrompt")]
    public Common LstCountryCodePrompt
    {
      get => lstCountryCodePrompt ??= new();
      set => lstCountryCodePrompt = value;
    }

    /// <summary>
    /// A value of List.
    /// </summary>
    [JsonPropertyName("list")]
    public CodeValue List
    {
      get => list ??= new();
      set => list = value;
    }

    /// <summary>
    /// A value of WorkListDutyStation.
    /// </summary>
    [JsonPropertyName("workListDutyStation")]
    public Common WorkListDutyStation
    {
      get => workListDutyStation ??= new();
      set => workListDutyStation = value;
    }

    /// <summary>
    /// A value of WorkListBranch.
    /// </summary>
    [JsonPropertyName("workListBranch")]
    public Common WorkListBranch
    {
      get => workListBranch ??= new();
      set => workListBranch = value;
    }

    /// <summary>
    /// A value of WorkListRank.
    /// </summary>
    [JsonPropertyName("workListRank")]
    public Common WorkListRank
    {
      get => workListRank ??= new();
      set => workListRank = value;
    }

    /// <summary>
    /// A value of MilitaryBranch.
    /// </summary>
    [JsonPropertyName("militaryBranch")]
    public CodeValue MilitaryBranch
    {
      get => militaryBranch ??= new();
      set => militaryBranch = value;
    }

    /// <summary>
    /// A value of MilitaryDutyStatus.
    /// </summary>
    [JsonPropertyName("militaryDutyStatus")]
    public CodeValue MilitaryDutyStatus
    {
      get => militaryDutyStatus ??= new();
      set => militaryDutyStatus = value;
    }

    /// <summary>
    /// A value of MilitaryRank.
    /// </summary>
    [JsonPropertyName("militaryRank")]
    public CodeValue MilitaryRank
    {
      get => militaryRank ??= new();
      set => militaryRank = value;
    }

    /// <summary>
    /// A value of Work.
    /// </summary>
    [JsonPropertyName("work")]
    public Standard Work
    {
      get => work ??= new();
      set => work = value;
    }

    /// <summary>
    /// A value of WorkH.
    /// </summary>
    [JsonPropertyName("workH")]
    public ScrollingAttributes WorkH
    {
      get => workH ??= new();
      set => workH = value;
    }

    /// <summary>
    /// Gets a value of Pages.
    /// </summary>
    [JsonIgnore]
    public Array<PagesGroup> Pages => pages ??= new(PagesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Pages for json serialization.
    /// </summary>
    [JsonPropertyName("pages")]
    [Computed]
    public IList<PagesGroup> Pages_Json
    {
      get => pages;
      set => Pages.Assign(value);
    }

    /// <summary>
    /// A value of HmilitaryService.
    /// </summary>
    [JsonPropertyName("hmilitaryService")]
    public MilitaryService HmilitaryService
    {
      get => hmilitaryService ??= new();
      set => hmilitaryService = value;
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
    /// A value of MilitaryService.
    /// </summary>
    [JsonPropertyName("militaryService")]
    public MilitaryService MilitaryService
    {
      get => militaryService ??= new();
      set => militaryService = value;
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
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of DlgflwSelectedPf24Lett.
    /// </summary>
    [JsonPropertyName("dlgflwSelectedPf24Lett")]
    public CodeValue DlgflwSelectedPf24Lett
    {
      get => dlgflwSelectedPf24Lett ??= new();
      set => dlgflwSelectedPf24Lett = value;
    }

    private Common hiddenListval;
    private MilitaryService hiddenLastRead;
    private Case1 case1;
    private IncomeSource hincomeSource;
    private CsePersonsWorkSet name;
    private Common alreadyDisplayed;
    private PersonIncomeHistory personIncomeHistory;
    private CsePersonsWorkSet fromSelection;
    private Common csePersonPrompt;
    private Common lstLocAddrStatePrompt;
    private Common lstCountryCodePrompt;
    private CodeValue list;
    private Common workListDutyStation;
    private Common workListBranch;
    private Common workListRank;
    private CodeValue militaryBranch;
    private CodeValue militaryDutyStatus;
    private CodeValue militaryRank;
    private Standard work;
    private ScrollingAttributes workH;
    private Array<PagesGroup> pages;
    private MilitaryService hmilitaryService;
    private CsePerson hcsePerson;
    private MilitaryService militaryService;
    private CsePerson csePerson;
    private CodeValue codeValue;
    private NextTranInfo hidden;
    private Standard standard;
    private CodeValue dlgflwSelectedPf24Lett;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A PagesGroup group.</summary>
    [Serializable]
    public class PagesGroup
    {
      /// <summary>
      /// A value of PagesDetail.
      /// </summary>
      [JsonPropertyName("pagesDetail")]
      public MilitaryService PagesDetail
      {
        get => pagesDetail ??= new();
        set => pagesDetail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 15;

      private MilitaryService pagesDetail;
    }

    /// <summary>
    /// A value of HiddenListval.
    /// </summary>
    [JsonPropertyName("hiddenListval")]
    public Common HiddenListval
    {
      get => hiddenListval ??= new();
      set => hiddenListval = value;
    }

    /// <summary>
    /// A value of HiddenLastRead.
    /// </summary>
    [JsonPropertyName("hiddenLastRead")]
    public MilitaryService HiddenLastRead
    {
      get => hiddenLastRead ??= new();
      set => hiddenLastRead = value;
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
    /// A value of HincomeSource.
    /// </summary>
    [JsonPropertyName("hincomeSource")]
    public IncomeSource HincomeSource
    {
      get => hincomeSource ??= new();
      set => hincomeSource = value;
    }

    /// <summary>
    /// A value of Name.
    /// </summary>
    [JsonPropertyName("name")]
    public CsePersonsWorkSet Name
    {
      get => name ??= new();
      set => name = value;
    }

    /// <summary>
    /// A value of AlreadyDisplayed.
    /// </summary>
    [JsonPropertyName("alreadyDisplayed")]
    public Common AlreadyDisplayed
    {
      get => alreadyDisplayed ??= new();
      set => alreadyDisplayed = value;
    }

    /// <summary>
    /// A value of PersonIncomeHistory.
    /// </summary>
    [JsonPropertyName("personIncomeHistory")]
    public PersonIncomeHistory PersonIncomeHistory
    {
      get => personIncomeHistory ??= new();
      set => personIncomeHistory = value;
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
    /// A value of LstLocAddrStatePrompt.
    /// </summary>
    [JsonPropertyName("lstLocAddrStatePrompt")]
    public Common LstLocAddrStatePrompt
    {
      get => lstLocAddrStatePrompt ??= new();
      set => lstLocAddrStatePrompt = value;
    }

    /// <summary>
    /// A value of LstCountryCodePrompt.
    /// </summary>
    [JsonPropertyName("lstCountryCodePrompt")]
    public Common LstCountryCodePrompt
    {
      get => lstCountryCodePrompt ??= new();
      set => lstCountryCodePrompt = value;
    }

    /// <summary>
    /// A value of ListCountryCodePrompt.
    /// </summary>
    [JsonPropertyName("listCountryCodePrompt")]
    public Standard ListCountryCodePrompt
    {
      get => listCountryCodePrompt ??= new();
      set => listCountryCodePrompt = value;
    }

    /// <summary>
    /// A value of ListResLocnAddrStates.
    /// </summary>
    [JsonPropertyName("listResLocnAddrStates")]
    public Standard ListResLocnAddrStates
    {
      get => listResLocnAddrStates ??= new();
      set => listResLocnAddrStates = value;
    }

    /// <summary>
    /// A value of List.
    /// </summary>
    [JsonPropertyName("list")]
    public Code List
    {
      get => list ??= new();
      set => list = value;
    }

    /// <summary>
    /// A value of WorkListDutyStatus.
    /// </summary>
    [JsonPropertyName("workListDutyStatus")]
    public Common WorkListDutyStatus
    {
      get => workListDutyStatus ??= new();
      set => workListDutyStatus = value;
    }

    /// <summary>
    /// A value of WorkListBranch.
    /// </summary>
    [JsonPropertyName("workListBranch")]
    public Common WorkListBranch
    {
      get => workListBranch ??= new();
      set => workListBranch = value;
    }

    /// <summary>
    /// A value of WorkListRank.
    /// </summary>
    [JsonPropertyName("workListRank")]
    public Common WorkListRank
    {
      get => workListRank ??= new();
      set => workListRank = value;
    }

    /// <summary>
    /// A value of MilitaryBranch.
    /// </summary>
    [JsonPropertyName("militaryBranch")]
    public CodeValue MilitaryBranch
    {
      get => militaryBranch ??= new();
      set => militaryBranch = value;
    }

    /// <summary>
    /// A value of MilitaryDutyStatus.
    /// </summary>
    [JsonPropertyName("militaryDutyStatus")]
    public CodeValue MilitaryDutyStatus
    {
      get => militaryDutyStatus ??= new();
      set => militaryDutyStatus = value;
    }

    /// <summary>
    /// A value of MilitaryRank.
    /// </summary>
    [JsonPropertyName("militaryRank")]
    public CodeValue MilitaryRank
    {
      get => militaryRank ??= new();
      set => militaryRank = value;
    }

    /// <summary>
    /// A value of Work.
    /// </summary>
    [JsonPropertyName("work")]
    public Standard Work
    {
      get => work ??= new();
      set => work = value;
    }

    /// <summary>
    /// A value of WorkH.
    /// </summary>
    [JsonPropertyName("workH")]
    public ScrollingAttributes WorkH
    {
      get => workH ??= new();
      set => workH = value;
    }

    /// <summary>
    /// Gets a value of Pages.
    /// </summary>
    [JsonIgnore]
    public Array<PagesGroup> Pages => pages ??= new(PagesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Pages for json serialization.
    /// </summary>
    [JsonPropertyName("pages")]
    [Computed]
    public IList<PagesGroup> Pages_Json
    {
      get => pages;
      set => Pages.Assign(value);
    }

    /// <summary>
    /// A value of HmilitaryService.
    /// </summary>
    [JsonPropertyName("hmilitaryService")]
    public MilitaryService HmilitaryService
    {
      get => hmilitaryService ??= new();
      set => hmilitaryService = value;
    }

    /// <summary>
    /// A value of MilitaryService.
    /// </summary>
    [JsonPropertyName("militaryService")]
    public MilitaryService MilitaryService
    {
      get => militaryService ??= new();
      set => militaryService = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
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
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    /// <summary>
    /// A value of DocumentResponse.
    /// </summary>
    [JsonPropertyName("documentResponse")]
    public Common DocumentResponse
    {
      get => documentResponse ??= new();
      set => documentResponse = value;
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
    /// A value of Pf24Source.
    /// </summary>
    [JsonPropertyName("pf24Source")]
    public Code Pf24Source
    {
      get => pf24Source ??= new();
      set => pf24Source = value;
    }

    private Common hiddenListval;
    private MilitaryService hiddenLastRead;
    private Case1 case1;
    private IncomeSource hincomeSource;
    private CsePersonsWorkSet name;
    private Common alreadyDisplayed;
    private PersonIncomeHistory personIncomeHistory;
    private Common csePersonPrompt;
    private Common lstLocAddrStatePrompt;
    private Common lstCountryCodePrompt;
    private Standard listCountryCodePrompt;
    private Standard listResLocnAddrStates;
    private Code list;
    private Common workListDutyStatus;
    private Common workListBranch;
    private Common workListRank;
    private CodeValue militaryBranch;
    private CodeValue militaryDutyStatus;
    private CodeValue militaryRank;
    private Standard work;
    private ScrollingAttributes workH;
    private Array<PagesGroup> pages;
    private MilitaryService hmilitaryService;
    private MilitaryService militaryService;
    private CsePerson hcsePerson;
    private CsePerson csePerson;
    private Code code;
    private NextTranInfo hidden;
    private Standard standard;
    private AbendData abendData;
    private Common documentResponse;
    private Common hiddenDisplayPerformed;
    private Code pf24Source;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of Work.
    /// </summary>
    [JsonPropertyName("work")]
    public Common Work
    {
      get => work ??= new();
      set => work = value;
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
    /// A value of Position.
    /// </summary>
    [JsonPropertyName("position")]
    public Common Position
    {
      get => position ??= new();
      set => position = value;
    }

    /// <summary>
    /// A value of WorkArea.
    /// </summary>
    [JsonPropertyName("workArea")]
    public WorkArea WorkArea
    {
      get => workArea ??= new();
      set => workArea = value;
    }

    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public MilitaryService Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    /// <summary>
    /// A value of FromIncs.
    /// </summary>
    [JsonPropertyName("fromIncs")]
    public Common FromIncs
    {
      get => fromIncs ??= new();
      set => fromIncs = value;
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
    /// A value of Text40.
    /// </summary>
    [JsonPropertyName("text40")]
    public WorkArea Text40
    {
      get => text40 ??= new();
      set => text40 = value;
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
    /// A value of RefreshPersonIncomeHistory.
    /// </summary>
    [JsonPropertyName("refreshPersonIncomeHistory")]
    public PersonIncomeHistory RefreshPersonIncomeHistory
    {
      get => refreshPersonIncomeHistory ??= new();
      set => refreshPersonIncomeHistory = value;
    }

    /// <summary>
    /// A value of RefreshIncomeSource.
    /// </summary>
    [JsonPropertyName("refreshIncomeSource")]
    public IncomeSource RefreshIncomeSource
    {
      get => refreshIncomeSource ??= new();
      set => refreshIncomeSource = value;
    }

    /// <summary>
    /// A value of CheckZip.
    /// </summary>
    [JsonPropertyName("checkZip")]
    public Common CheckZip
    {
      get => checkZip ??= new();
      set => checkZip = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of State.
    /// </summary>
    [JsonPropertyName("state")]
    public Code State
    {
      get => state ??= new();
      set => state = value;
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
    /// A value of MilitaryBranch.
    /// </summary>
    [JsonPropertyName("militaryBranch")]
    public Code MilitaryBranch
    {
      get => militaryBranch ??= new();
      set => militaryBranch = value;
    }

    /// <summary>
    /// A value of MilitaryRank.
    /// </summary>
    [JsonPropertyName("militaryRank")]
    public Code MilitaryRank
    {
      get => militaryRank ??= new();
      set => militaryRank = value;
    }

    /// <summary>
    /// A value of MilitaryDutyStatus.
    /// </summary>
    [JsonPropertyName("militaryDutyStatus")]
    public Code MilitaryDutyStatus
    {
      get => militaryDutyStatus ??= new();
      set => militaryDutyStatus = value;
    }

    /// <summary>
    /// A value of RefreshMilitaryService.
    /// </summary>
    [JsonPropertyName("refreshMilitaryService")]
    public MilitaryService RefreshMilitaryService
    {
      get => refreshMilitaryService ??= new();
      set => refreshMilitaryService = value;
    }

    /// <summary>
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public MilitaryService Next
    {
      get => next ??= new();
      set => next = value;
    }

    /// <summary>
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    public MilitaryService Start
    {
      get => start ??= new();
      set => start = value;
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
    /// A value of ReturnCode.
    /// </summary>
    [JsonPropertyName("returnCode")]
    public Common ReturnCode
    {
      get => returnCode ??= new();
      set => returnCode = value;
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
    /// A value of ValidCode.
    /// </summary>
    [JsonPropertyName("validCode")]
    public Common ValidCode
    {
      get => validCode ??= new();
      set => validCode = value;
    }

    /// <summary>
    /// A value of PrintableDocument.
    /// </summary>
    [JsonPropertyName("printableDocument")]
    public CodeValue PrintableDocument
    {
      get => printableDocument ??= new();
      set => printableDocument = value;
    }

    /// <summary>
    /// A value of MultDocsPossFromScrn.
    /// </summary>
    [JsonPropertyName("multDocsPossFromScrn")]
    public Standard MultDocsPossFromScrn
    {
      get => multDocsPossFromScrn ??= new();
      set => multDocsPossFromScrn = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
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
    /// A value of DetailText10.
    /// </summary>
    [JsonPropertyName("detailText10")]
    public TextWorkArea DetailText10
    {
      get => detailText10 ??= new();
      set => detailText10 = value;
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
    /// A value of FlowData.
    /// </summary>
    [JsonPropertyName("flowData")]
    public Common FlowData
    {
      get => flowData ??= new();
      set => flowData = value;
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

    private NextTranInfo null1;
    private Common work;
    private SpDocLiteral spDocLiteral;
    private Common position;
    private WorkArea workArea;
    private MilitaryService initialized;
    private Common fromIncs;
    private DateWorkArea date;
    private WorkArea text40;
    private TextWorkArea textWorkArea;
    private Document document;
    private PersonIncomeHistory refreshPersonIncomeHistory;
    private IncomeSource refreshIncomeSource;
    private Common checkZip;
    private Code country;
    private CsePerson csePerson;
    private Code state;
    private CodeValue codeValue;
    private Code militaryBranch;
    private Code militaryRank;
    private Code militaryDutyStatus;
    private MilitaryService refreshMilitaryService;
    private MilitaryService next;
    private MilitaryService start;
    private Common workError;
    private Common returnCode;
    private Code code;
    private Common validCode;
    private CodeValue printableDocument;
    private Standard multDocsPossFromScrn;
    private WorkArea raiseEventFlag;
    private Infrastructure infrastructure;
    private Common numberOfEvents;
    private TextWorkArea detailText10;
    private Infrastructure lastTran;
    private Common flowData;
    private DateWorkArea blank;
  }
#endregion
}
