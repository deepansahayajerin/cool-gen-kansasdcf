// Program: SI_NAME_NAME_LIST, ID: 371090456, model: 746.
// Short name: SWENAMEP
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
/// A program: SI_NAME_NAME_LIST.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This procedure lists all of the matches found on ADABAS based on either the 
/// first name and the last name or the SSN.
/// The search criteria can be narrowed to decrease the size of the list.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiNameNameList: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_NAME_NAME_LIST program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiNameNameList(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiNameNameList.
  /// </summary>
  public SiNameNameList(IContext context, Import import, Export export):
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
    // 		M A I N T E N A N C E   L O G
    // Date	  Developer		Description
    // 05-01-95  Helen Sharland - MTW	Initial Dev
    // 02-22-96  Lewis			Complete Development
    // 05-06-96  Rao Mulpuri		Changes to Link Organization
    // 06/11/96  G. Lofton		Changed input ssn to numeric
    // 				fields.
    // 09/12/96  Sid Chowdhary		Prevent flowing to REGI if
    // 				coming from IAPI.
    // 03/08/97  G. Lofton - MTW	Add Information Request
    // 				number to pass to REGI.
    // 06/23/97  Sid Chowdhary		H23960. Must return back to REGI
    // 				using F9.
    // 08/28/97  Sid Chowdhary         Change Alias flag to sel_char
    // 				(to remove permitted value violation)
    // 12/17/97  Sid Chowdhary		For Phonetic search "N", set to 100%
    // 09/09/98  W. Campbell           Added logic to give an error if
    //                                 
    // the first character of the Last
    //                                 
    // Name is not alphabetic on a name
    //                                 
    // search.
    // 09/10/98  W. Campbell           #H00048042 Disabled a statement in
    //                                 
    // the code which was not allowing
    // the
    //                                 
    // DOB to be effective in a name
    //                                 
    // search.  The DOB was being set
    // to
    //                                 
    // datenum(0).
    // 09/11/98  W. Campbell           Added a statement to set an exit
    //                                 
    // state before the escape out of
    // the
    //                                 
    // program when a type code
    //                                 
    // not = spaces is returned from
    // the
    //                                 
    // ADABAS call.  This is to provide
    // an
    //                                 
    // error msg to the user.
    // 09/14/98  W. Campbell           Code was inserted into
    //                                 
    // this action diagram in order to
    //                                 
    // output an error msg if either
    // the
    //                                 
    // DOB or middle initial are input
    //                                 
    // as part of a phonetic name
    // search.
    // 11/05/98  W. Campbell           A statement was
    //                                 
    // disabled in order to avoid the
    //                                 
    // check for only the first 3 chars
    //                                 
    // of the first name being input
    //                                 
    // on a phonetic name search.
    //                                 
    // Also, all the other statements
    //                                 
    // in the same else statement were
    //                                 
    // disabled.
    // 11/06/98 W. Campbell            Change made to only
    //                                 
    // use the first 3 chars of first
    //                                 
    // name in phonetic name search.
    // ------------------------------------------------------------
    // 03/04/99 W. Campbell            Added a set statment
    //                                 
    // to fix a problem caused by not
    //                                 
    // reinitializing the unique key
    //                                 
    // (100 byte area) to SPACES when
    //                                 
    // a search produced more than a
    //                                 
    // group full of data.  Also had to
    //                                 
    // change view matching to
    //                                 
    // export_hidden instead of
    //                                 
    // import_hidden
    // cse_person_work_set.
    // ------------------------------------------------------------
    // 03/30/99 W.Campbell             zdel exit state clean-up.
    // ------------------------------------------------------------
    // 05/14/99 M.Lachowicz            Disabled block of statments
    //                                 
    // to fix a problem which occurs
    // for
    //                                 
    // name search (when user clears
    // SSN,
    //                                 
    // SSN  is displaed for previous
    //                                 
    // search).
    // ----------------------------------------------------------
    // 02/01/00 M.Lachowicz            Populate name field in NEXT
    //                                 
    // TRAN. PR #86390
    // ----------------------------------------------------------
    // 12/20/2000 M.Ramirez            WR# 242 - Add office to NAME screen,
    // which includes:
    // 	1)  Changing detail line to include office
    // 	2)  New filter for Office number
    // 	3)  New filter for Person number
    // 	4)  Remove PF21 Name Search
    // 	5)  Remove PF22 SSN Search
    // 	6)  Add PF2 Display
    // ----------------------------------------------------------
    // 02/05/2001	M Ramirez	PR# 113224 - Name is being passed to
    // other screens with the office(s) tacked on at the end
    // ----------------------------------------------------------
    // 02/06/2001	M Ramirez	PR# 113331 - SSN doesn't display on
    // a NextTran
    // ----------------------------------------------------------
    // 02/06/2001	M Ramirez	PR# 113346 - Office doesn't highlight
    // when it is used in search
    // ----------------------------------------------------------
    // 02/21/2001	M Ramirez	PR# 114365 - Clear doesn't reset phonetic ind and 
    // percent
    // ----------------------------------------------------------
    // 04/05/2001	M Ramirez	PR# 115424 - On Person Number search, person is not 
    // found if only known to AE
    // ----------------------------------------------------------
    // 04/05/2001	M Ramirez	PR# 115426 - On flow from PAR1 SSN is not displayed 
    // immediately
    // ----------------------------------------------------------
    // 05/11/2001	M Ramirez	PR# 118211 - Set SSN = '000000000' to spaces on 
    // return from SI_READ_CSE_PERSON
    // ----------------------------------------------------------
    // 05/11/2001	M Ramirez	PR# 113346 - Office highlights inappropriately
    // ----------------------------------------------------------
    // 06/12/2001	M Ramirez	PR# 118553 - Search by Person number should show 
    // aliases
    // ----------------------------------------------------------
    // 09/05/2001	M Ashworth	PR# 126235 - Only display active assignment office 
    // numbers unless there are none.  If there are none, show the latest
    // discontinued.
    // ----------------------------------------------------------
    // 11/08/2001	M Ashworth	PR# 131012 - Added exit state to inform user to hit
    // F8 for more occurences when the list is full.
    // ----------------------------------------------------------
    // 02/19/2002	M Ashworth	PR 137923 The cab read adabas person calls an 
    // external that checks if there is an open program in the AE system.  If
    // there is, it sets the flag to an "O" so certain screens will not be able
    // to update the information. (Only ae people can update if there is an open
    // program).  Updating is not an issue in this program so it will be set
    // back to Y. This fixes the codes that display what system a person is
    // known to.
    // ----------------------------------------------------------
    // 06/24/2002  T Bobb  WR020259 Added display of active
    // person programs. Also rewrote the read part of the prad.
    // ----------------------------------------------------------
    // 11/13/2002  T.BOBB PR00161860  Change exit state to
    // flow back to REGI from ORGZ.
    // ----------------------------------------------------------
    // 12/04/2002  GVandy PR00163947  Backout changes made for PR00161860 which 
    // broke the link return from ORGZ to about 80 other screens.
    // ----------------------------------------------------------
    local.Current.Date = Now().Date;
    ExitState = "ACO_NN0000_ALL_OK";

    // ---------------------------------------------
    // Move Imports to Exports
    // ---------------------------------------------
    export.FromIapi.Flag = import.FromIapi.Flag;
    MoveInterstateCase(import.FromInterstateCase, export.FromInterstateCase);
    export.FromInrdCommon.Flag = import.FromInrdCommon.Flag;
    MoveInformationRequest(import.FromInrdInformationRequest,
      export.FromInrdInformationRequest);
    export.FromPar1.Flag = import.FromPar1.Flag;
    export.FromPaReferral.Assign(import.FromPaReferral);
    export.FromRegi.Flag = import.FromRegi.Flag;

    // mjr
    // ------------------------------------------
    // 02/21/2001
    // PR# 114265 - Clear doesn't reset phonetic ind and percent
    // Commented following line
    // -------------------------------------------------------
    // mjr
    // ------------------------------------------
    // 02/21/2001
    // PR# 114265 - Clear doesn't reset phonetic ind and percent
    // Added following line
    // -------------------------------------------------------
    export.InitialExecution.Flag = import.InitialExecution.Flag;

    for(import.ToRegi.Index = 0; import.ToRegi.Index < import.ToRegi.Count; ++
      import.ToRegi.Index)
    {
      if (!import.ToRegi.CheckSize())
      {
        break;
      }

      export.ToRegi.Index = import.ToRegi.Index;
      export.ToRegi.CheckSize();

      export.ToRegi.Update.DetailToRegi.Assign(import.ToRegi.Item.DetailToRegi);
      export.ToRegi.Update.DetailToRegi.Flag = "X";
    }

    import.ToRegi.CheckIndex();

    for(import.Searched.Index = 0; import.Searched.Index < import
      .Searched.Count; ++import.Searched.Index)
    {
      if (!import.Searched.CheckSize())
      {
        break;
      }

      export.Searched.Index = import.Searched.Index;
      export.Searched.CheckSize();

      MoveCsePersonsWorkSet3(import.Searched.Item.Searched1,
        export.Searched.Update.Searched1);
    }

    import.Searched.CheckIndex();

    if (Equal(global.Command, "CLEAR"))
    {
      // mjr
      // ------------------------------------------
      // 02/21/2001
      // PR# 114265 - Clear doesn't reset phonetic ind and percent
      // Added following 2 lines
      // -------------------------------------------------------
      export.Phonetic.Flag = "Y";
      export.Phonetic.Percentage = 35;

      var field = GetField(export.SearchCsePersonsWorkSet, "lastName");

      field.Protected = false;
      field.Focused = true;

      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.Next.Number = import.Next.Number;
    export.SearchSsnWorkArea.Assign(import.SearchSsnWorkArea);
    export.SearchCsePersonsWorkSet.Assign(import.SearchCsePersonsWorkSet);
    export.SearchPrev.Assign(import.SearchPrev);
    export.HiddenCsePersonsWorkSet.UniqueKey =
      import.HiddenCsePersonsWorkSet.UniqueKey;

    // mjr
    // ------------------------------------------
    // 02/21/2001
    // PR# 114265 - Clear doesn't reset phonetic ind and percent
    // Added following line
    // -------------------------------------------------------
    MoveCommon1(import.Phonetic, export.Phonetic);

    // mjr
    // ------------------------------------------
    // 02/21/2001
    // PR# 114265 - Clear doesn't reset phonetic ind and percent
    // Commented following line
    // -------------------------------------------------------
    // mjr
    // ------------------------------------------
    // 12/20/2000
    // WR# 242 - Add office to NAME screen
    // Added move import search office to export search office
    // -------------------------------------------------------
    export.SearchOffice.SystemGeneratedId =
      import.SearchOffice.SystemGeneratedId;

    // mjr
    // ------------------------------------------
    // 12/20/2000
    // WR# 242 - Add office to NAME screen
    // Zero fill Person number
    // -------------------------------------------------------
    if (!IsEmpty(export.SearchCsePersonsWorkSet.Number))
    {
      UseCabZeroFillNumber2();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.SearchCsePersonsWorkSet, "number");

        field.Error = true;

        return;
      }
    }

    if (!IsEmpty(export.Next.Number))
    {
      UseCabZeroFillNumber1();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Next, "number");

        field.Error = true;

        return;
      }
    }

    if (export.SearchSsnWorkArea.SsnNumPart1 != export
      .SearchPrev.SsnNumPart1 || export.SearchSsnWorkArea.SsnNumPart2 != export
      .SearchPrev.SsnNumPart2 || export.SearchSsnWorkArea.SsnNumPart3 != export
      .SearchPrev.SsnNumPart3)
    {
      export.SearchPrev.Assign(export.SearchSsnWorkArea);
      MoveSsnWorkArea2(export.SearchSsnWorkArea, local.SsnWorkArea);
      local.SsnWorkArea.ConvertOption = "2";
      UseCabSsnConvertNumToText();
      export.SearchCsePersonsWorkSet.Ssn = local.SsnWorkArea.SsnText9;
    }
    else
    {
      // mjr
      // --------------------------------------------------
      // 04/05/2001
      // PR# 115426 - On flow from PAR1 SSN is not displayed immediately
      // ---------------------------------------------------------------
      if (!IsEmpty(export.FromPar1.Flag) || !IsEmpty(export.FromIapi.Flag) || !
        IsEmpty(export.FromRegi.Flag))
      {
        if (!IsEmpty(export.SearchCsePersonsWorkSet.Ssn))
        {
          local.SsnWorkArea.SsnText9 = export.SearchCsePersonsWorkSet.Ssn;
          UseCabSsnConvertTextToNum();
          export.SearchPrev.Assign(export.SearchSsnWorkArea);
        }
      }
    }

    if (AsChar(export.InitialExecution.Flag) != 'N')
    {
      if (AsChar(import.FromInrdCommon.Flag) == 'Y' && AsChar
        (import.FromInrdInformationRequest.Type1) != 'P')
      {
        export.InitialExecution.Flag = "N";
      }
      else
      {
        export.InitialExecution.Flag = "N";
        export.Phonetic.Flag = "Y";
        export.Phonetic.Percentage = 35;
      }
    }

    export.Export1.Index = 0;
    export.Export1.Clear();

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (export.Export1.IsFull)
      {
        break;
      }

      export.Export1.Update.Gcommon.SelectChar =
        import.Import1.Item.Gcommon.SelectChar;
      export.Export1.Update.GcsePersonsWorkSet.Assign(
        import.Import1.Item.GcsePersonsWorkSet);
      MoveCommon2(import.Import1.Item.GimportAlt,
        export.Export1.Update.GexportAlt);
      export.Export1.Update.GexportAliasAe.Flag =
        import.Import1.Item.GimportAliasAe.Flag;
      export.Export1.Update.GexportAliasCse.Flag =
        import.Import1.Item.GimportAliasCse.Flag;
      export.Export1.Update.GexportAliasKanpay.Flag =
        import.Import1.Item.GimportAliasKanpay.Flag;
      export.Export1.Update.GexportAliasKscares.Flag =
        import.Import1.Item.GimportAliasKscares.Flag;
      export.Export1.Update.GexportAliasFacts.Flag =
        import.Import1.Item.GimportAliasFacts.Flag;
      export.Export1.Next();
    }

    // ---------------------------------------------
    //         	N E X T   T R A N
    // ---------------------------------------------
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      export.HiddenNextTranInfo.CaseNumber = export.Next.Number;

      // 02/01/00 M.L Start
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        switch(AsChar(export.Export1.Item.Gcommon.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            ++local.Common.Count;

            break;
          default:
            var field = GetField(export.Export1.Item.Gcommon, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            return;
        }
      }

      switch(local.Common.Count)
      {
        case 0:
          break;
        case 1:
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (AsChar(export.Export1.Item.Gcommon.SelectChar) == 'S')
            {
              export.HiddenNextTranInfo.CsePersonNumber =
                export.Export1.Item.GcsePersonsWorkSet.Number;

              break;
            }
          }

          break;
        default:
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (AsChar(export.Export1.Item.Gcommon.SelectChar) == 'S')
            {
              var field = GetField(export.Export1.Item.Gcommon, "selectChar");

              field.Error = true;
            }
          }

          ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

          return;
      }

      // 02/01/00 M.L End
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
        export.Next.Number = export.HiddenNextTranInfo.CaseNumber ?? Spaces(10);
        UseCabZeroFillNumber1();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NN0000_ALL_OK";
        }

        if (!IsEmpty(export.HiddenNextTranInfo.CsePersonNumberAp))
        {
          export.SearchCsePersonsWorkSet.Number =
            export.HiddenNextTranInfo.CsePersonNumberAp ?? Spaces(10);
          UseCabZeroFillNumber2();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            goto Test1;
          }

          ExitState = "ACO_NN0000_ALL_OK";
        }

        if (!IsEmpty(export.HiddenNextTranInfo.CsePersonNumberObligor))
        {
          export.SearchCsePersonsWorkSet.Number =
            export.HiddenNextTranInfo.CsePersonNumberObligor ?? Spaces(10);
          UseCabZeroFillNumber2();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            goto Test1;
          }

          ExitState = "ACO_NN0000_ALL_OK";
        }

        if (!IsEmpty(export.HiddenNextTranInfo.CsePersonNumber))
        {
          export.SearchCsePersonsWorkSet.Number =
            export.HiddenNextTranInfo.CsePersonNumber ?? Spaces(10);
          UseCabZeroFillNumber2();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            goto Test1;
          }

          ExitState = "ACO_NN0000_ALL_OK";
        }

        if (!IsEmpty(export.HiddenNextTranInfo.CsePersonNumberObligee))
        {
          export.SearchCsePersonsWorkSet.Number =
            export.HiddenNextTranInfo.CsePersonNumberObligee ?? Spaces(10);
          UseCabZeroFillNumber2();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            goto Test1;
          }

          ExitState = "ACO_NN0000_ALL_OK";
        }
      }
      else
      {
        ExitState = "ACO_NN0000_ALL_OK";
      }

Test1:

      if (!IsEmpty(export.SearchCsePersonsWorkSet.Number))
      {
        UseSiReadCsePerson();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NN0000_ALL_OK";
        }
        else
        {
          // mjr
          // ------------------------------------------
          // 05/11/2001
          // PR# 118211 - Set SSN = '000000000' to spaces on return
          // from SI_READ_CSE_PERSON
          // -------------------------------------------------------
          if (Equal(export.SearchCsePersonsWorkSet.Ssn, "000000000"))
          {
            export.SearchCsePersonsWorkSet.Ssn = "";
          }
          else
          {
            // mjr
            // ------------------------------------------
            // 02/06/2001
            // PR# 113331 - SSN doesn't display on a NextTran
            // -------------------------------------------------------
            local.SsnWorkArea.SsnText9 = export.SearchCsePersonsWorkSet.Ssn;
            UseCabSsnConvertTextToNum();
            export.SearchPrev.Assign(export.SearchSsnWorkArea);
          }
        }
      }

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      var field = GetField(export.SearchCsePersonsWorkSet, "lastName");

      field.Protected = false;
      field.Focused = true;

      return;
    }

    if (Equal(global.Command, "RETCOMN"))
    {
      var field = GetField(export.SearchCsePersonsWorkSet, "lastName");

      field.Protected = false;
      field.Focused = true;

      return;
    }

    // ---------------------------------------------
    //        S E C U R I T Y   L O G I C
    // ---------------------------------------------
    // to validate action level security
    if (Equal(global.Command, "REGI") || Equal(global.Command, "PART") || Equal
      (global.Command, "COMN") || Equal(global.Command, "SAVE") || Equal
      (global.Command, "ORGZ") || Equal(global.Command, "RTNORG") || Equal
      (global.Command, "QARM"))
    {
    }
    else
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
    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    // ---------------------------------------------------
    // Check that a valid selection code has been entered.
    // ---------------------------------------------------
    local.Common.Count = 0;

    for(export.Export1.Index = 0; export.Export1.Index < export.Export1.Count; ++
      export.Export1.Index)
    {
      switch(AsChar(export.Export1.Item.Gcommon.SelectChar))
      {
        case ' ':
          break;
        case 'S':
          ++local.Common.Count;
          export.Selected.Assign(export.Export1.Item.GcsePersonsWorkSet);

          // mjr
          // --------------------------------------------
          // 02/05/2001
          // PR# 113224 - Name is being passed to other screens
          // with the office(s) tacked on at the end
          // ---------------------------------------------------------
          UseSiFormatCsePersonName1();

          // mjr
          // --------------------------------------------
          // 02/05/2001
          // PR# 113224 - end change
          // ---------------------------------------------------------
          break;
        default:
          var field = GetField(export.Export1.Item.Gcommon, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

          break;
      }
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // mjr
    // ------------------------------------------
    // 12/20/2000
    // WR# 242 - Add office to NAME screen
    // Added CASE display
    // Removed CASE nmsrch
    // Removed CASE ssnsrch
    // Modified CASE next
    // -------------------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        export.HiddenCsePersonsWorkSet.UniqueKey = "";

        break;
      case "RTNORG":
        // --------------------------------------------
        // This Command is for returning to the calling
        // Screen without stopping at the Name screen.
        // This is triggered after returning from Organization.
        // --------------------------------------------
        export.Selected.Assign(import.Selected);

        // 12/04/02 GVandy PR00163947  Backout changes made for PR00161860.
        ExitState = "ACO_NE0000_RETURN";

        return;
      case "ORGZ":
        ExitState = "ECO_LNK_TO_ORG_MAINTENANCE";

        return;
      case "PART":
        switch(local.Common.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_NO_SELECTION_MADE";

            break;
          case 1:
            ExitState = "ECO_XFR_TO_CASE_PARTICIPATION";

            break;
          default:
            for(export.Export1.Index = 0; export.Export1.Index < export
              .Export1.Count; ++export.Export1.Index)
            {
              if (AsChar(export.Export1.Item.Gcommon.SelectChar) == 'S')
              {
                var field1 =
                  GetField(export.Export1.Item.Gcommon, "selectChar");

                field1.Error = true;
              }
            }

            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

            break;
        }

        return;
      case "COMN":
        switch(local.Common.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_NO_SELECTION_MADE";

            break;
          case 1:
            ExitState = "ECO_XFR_TO_LIST_CASES_BY_PERSON";

            break;
          default:
            for(export.Export1.Index = 0; export.Export1.Index < export
              .Export1.Count; ++export.Export1.Index)
            {
              if (AsChar(export.Export1.Item.Gcommon.SelectChar) == 'S')
              {
                var field1 =
                  GetField(export.Export1.Item.Gcommon, "selectChar");

                field1.Error = true;
              }
            }

            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

            break;
        }

        return;
      case "NEXT":
        if (IsEmpty(export.HiddenCsePersonsWorkSet.UniqueKey))
        {
          ExitState = "CO0000_SCROLLED_BEYOND_LAST_PG";

          return;
        }

        // mjr
        // ------------------------------------------
        // 12/20/2000
        // WR# 242 - Add office to NAME screen
        // Modified CASE next - removed retrieval logic.
        // It'll occur later in the if COMMAND = Display
        // -------------------------------------------------------
        break;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        return;
      case "RETURN":
        switch(local.Common.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_RETURN";

            break;
          case 1:
            ExitState = "ACO_NE0000_RETURN";

            break;
          default:
            for(export.Export1.Index = 0; export.Export1.Index < export
              .Export1.Count; ++export.Export1.Index)
            {
              if (AsChar(export.Export1.Item.Gcommon.SelectChar) == 'S')
              {
                var field1 =
                  GetField(export.Export1.Item.Gcommon, "selectChar");

                field1.Error = true;
              }
            }

            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

            break;
        }

        return;
      case "SAVE":
        if (!IsEmpty(import.FromPaReferral.Number) || import
          .FromInterstateCase.TransSerialNumber > 0)
        {
        }
        else if (import.FromInrdInformationRequest.Number <= 0)
        {
          ExitState = "MUST_FLOW_FROM_INRD";

          return;
        }
        else if (ReadInformationRequest())
        {
          if (AsChar(entities.InformationRequest.ApplicationProcessedInd) != 'Y'
            )
          {
            ExitState = "INFORMATION_REQUEST_MUST_BE_COMP";

            return;
          }
        }
        else
        {
          ExitState = "INFORMATION_REQUEST_NF";

          return;
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.Gcommon.SelectChar) == 'S')
          {
            // ------------------------------------------------------------
            // Make sure the person has not already been saved.
            // ------------------------------------------------------------
            for(export.ToRegi.Index = 0; export.ToRegi.Index < export
              .ToRegi.Count; ++export.ToRegi.Index)
            {
              if (!export.ToRegi.CheckSize())
              {
                break;
              }

              if (Equal(export.Export1.Item.GcsePersonsWorkSet.Number,
                export.ToRegi.Item.DetailToRegi.Number))
              {
                var field1 =
                  GetField(export.Export1.Item.GcsePersonsWorkSet,
                  "formattedName");

                field1.Error = true;

                ExitState = "SI0000_ALREADY_SAVED";

                goto Next;
              }
            }

            export.ToRegi.CheckIndex();

            export.ToRegi.Index = export.ToRegi.Count;
            export.ToRegi.CheckSize();

            MoveCsePersonsWorkSet1(export.Export1.Item.GcsePersonsWorkSet,
              export.ToRegi.Update.DetailToRegi);

            // mjr
            // --------------------------------------------
            // 02/05/2001
            // PR# 113224 - Name is being passed to other screens
            // with the office(s) tacked on at the end
            // ---------------------------------------------------------
            UseSiFormatCsePersonName2();

            // mjr
            // --------------------------------------------
            // 02/05/2001
            // PR# 113224 - end change
            // ---------------------------------------------------------
            export.Export1.Update.Gcommon.SelectChar = "";
            ExitState = "ACO_NI0000_SUCCESSFULLY_SAVED";
          }

Next:
          ;
        }

        if (!IsExitState("ACO_NI0000_SUCCESSFULLY_SAVED") && !
          IsExitState("SI0000_ALREADY_SAVED"))
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";
        }

        return;
      case "REGI":
        if (AsChar(export.FromRegi.Flag) == 'Y')
        {
          ExitState = "SI0000_MUST_RETURN_TO_REGI";

          return;
        }

        if (AsChar(export.FromPar1.Flag) == 'Y')
        {
          ExitState = "SI0000_MUST_FLOW_BACK_TO_PAR1";

          return;
        }

        if (AsChar(export.FromIapi.Flag) == 'Y')
        {
          ExitState = "SI0000_MUST_FLOW_BACK_TO_IAPI";

          return;
        }

        if (import.FromInrdInformationRequest.Number <= 0)
        {
          ExitState = "MUST_FLOW_FROM_INRD";

          return;
        }
        else if (ReadInformationRequest())
        {
          if (AsChar(entities.InformationRequest.ApplicationProcessedInd) != 'Y'
            )
          {
            ExitState = "INFORMATION_REQUEST_MUST_BE_COMP";

            return;
          }

          if (AsChar(entities.InformationRequest.NameSearchComplete) == 'Y')
          {
            goto Test2;
          }

          if (AsChar(entities.InformationRequest.Type1) == 'J' || AsChar
            (entities.InformationRequest.Type1) == 'F')
          {
            // for enrollment types that the AR is the state it is possible that
            // there is only 1
            //  person on the case. AR can not be search for on NAME, it has to 
            // be on ORGZ
            if (export.Searched.Count >= 1)
            {
              local.InformationRequest.Assign(entities.InformationRequest);

              if (IsEmpty(local.InformationRequest.NameSearchComplete))
              {
                local.InformationRequest.NameSearchComplete = "Y";
              }

              UseSiInrdUpdateInformationReq();

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
              }
              else
              {
                UseEabRollbackCics();

                return;
              }
            }
            else
            {
              ExitState = "NAME_SEARCH_REQUIRED";

              return;
            }
          }
          else if (export.Searched.Count > 1)
          {
            local.InformationRequest.Assign(entities.InformationRequest);

            if (IsEmpty(local.InformationRequest.NameSearchComplete))
            {
              local.InformationRequest.NameSearchComplete = "Y";
            }

            UseSiInrdUpdateInformationReq();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
            }
            else
            {
              UseEabRollbackCics();

              return;
            }
          }
          else
          {
            ExitState = "NAME_SEARCH_REQUIRED";

            return;
          }
        }
        else
        {
          ExitState = "INFORMATION_REQUEST_NF";

          return;
        }

Test2:

        ExitState = "ECO_XFR_TO_CASE_REGISTRATION";

        return;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "":
        var field = GetField(export.SearchCsePersonsWorkSet, "lastName");

        field.Protected = false;
        field.Focused = true;

        return;
      default:
        // mjr
        // ------------------------------------------
        // 12/20/2000
        // Moved logic from CASE invalid and placed it here
        // Removed CASE invalid
        // -------------------------------------------------------
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
    }

    // mjr
    // ------------------------------------------
    // 12/20/2000
    // WR# 242 - Add office to NAME screen
    // Added if COMMAND = display or COMMAND = next
    // -------------------------------------------------------
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "NEXT"))
    {
      // ---------------------------------------------
      // Validate the office
      // ---------------------------------------------
      if (export.SearchOffice.SystemGeneratedId != 0)
      {
        if (!ReadOffice())
        {
          var field = GetField(export.SearchOffice, "systemGeneratedId");

          field.Error = true;

          ExitState = "OFFICE_NF";
        }
      }

      // ---------------------------------------------
      // Validate the DOB
      // ---------------------------------------------
      if (!Lt(export.SearchCsePersonsWorkSet.Dob, local.Current.Date))
      {
        var field = GetField(export.SearchCsePersonsWorkSet, "dob");

        field.Error = true;

        ExitState = "INVALID_DATE_OF_BIRTH";
      }

      // ---------------------------------------------
      // Validate the Phonetic flag
      // ---------------------------------------------
      switch(AsChar(export.Phonetic.Flag))
      {
        case 'Y':
          if (!IsEmpty(export.SearchCsePersonsWorkSet.LastName) && IsEmpty
            (export.SearchCsePersonsWorkSet.FirstName))
          {
            var field1 = GetField(export.SearchCsePersonsWorkSet, "firstName");

            field1.Error = true;

            ExitState = "ACO_NE0000_FIRST_NAME_REQUIRED";

            return;
          }

          if (export.Phonetic.Percentage == 100)
          {
            export.Phonetic.Percentage = 35;
          }

          break;
        case 'N':
          export.Phonetic.Percentage = 100;

          break;
        case ' ':
          export.Phonetic.Flag = "N";
          export.Phonetic.Percentage = 100;

          break;
        default:
          var field = GetField(export.Phonetic, "flag");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";

          break;
      }

      // ---------------------------------------------
      // Validate first name
      // ---------------------------------------------
      // ---------------------------------------------
      // Validate the Percentage
      // ---------------------------------------------
      if (export.Phonetic.Percentage < 0 || export.Phonetic.Percentage > 100)
      {
        var field = GetField(export.Phonetic, "percentage");

        field.Error = true;

        ExitState = "INVALID_VALUE";
      }
      else if (export.Phonetic.Percentage == 100)
      {
        export.Phonetic.Flag = "N";
      }

      // ---------------------------------------------
      // Validate the sex
      // ---------------------------------------------
      switch(AsChar(export.SearchCsePersonsWorkSet.Sex))
      {
        case 'F':
          break;
        case 'M':
          break;
        case ' ':
          break;
        default:
          var field = GetField(export.SearchCsePersonsWorkSet, "sex");

          field.Error = true;

          ExitState = "INVALID_SEX";

          break;
      }

      if (!IsEmpty(export.SearchCsePersonsWorkSet.Ssn))
      {
        local.SsnWorkArea.SsnText9 = export.SearchCsePersonsWorkSet.Ssn;
        UseCabSsnConvertTextToNum();
        export.SearchPrev.Assign(export.SearchSsnWorkArea);
      }

      if (!IsEmpty(export.SearchCsePersonsWorkSet.LastName))
      {
        // ------------------------------------------------------------
        // Validate that the first character of the Last Name is alphabetic.
        // ------------------------------------------------------------
        local.TextWorkArea.Text1 =
          Substring(export.SearchCsePersonsWorkSet.LastName, 1, 1);

        if (AsChar(local.TextWorkArea.Text1) < 'A' || AsChar
          (local.TextWorkArea.Text1) > 'Z')
        {
          var field = GetField(export.SearchCsePersonsWorkSet, "lastName");

          field.Error = true;

          ExitState = "LAST_NAME_MUST_START_WITH_ALPHA";
        }
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      if (IsEmpty(export.SearchCsePersonsWorkSet.Number) && IsEmpty
        (export.SearchCsePersonsWorkSet.LastName) && IsEmpty
        (export.SearchCsePersonsWorkSet.Ssn))
      {
        var field1 = GetField(export.SearchCsePersonsWorkSet, "number");

        field1.Error = true;

        var field2 = GetField(export.SearchCsePersonsWorkSet, "lastName");

        field2.Error = true;

        var field3 = GetField(export.SearchSsnWorkArea, "ssnNumPart1");

        field3.Error = true;

        var field4 = GetField(export.SearchSsnWorkArea, "ssnNumPart2");

        field4.Error = true;

        var field5 = GetField(export.SearchSsnWorkArea, "ssnNumPart3");

        field5.Error = true;

        ExitState = "ACO_NE0000_SEARCH_CRITERIA_REQD";

        return;
      }

      // >>
      // WR020259B  new code starts here
      // ------------------------------------------------------------
      // Search by Person number
      // ------------------------------------------------------------
      if (!IsEmpty(export.SearchCsePersonsWorkSet.Number))
      {
        if (ReadCsePerson())
        {
          if (AsChar(entities.CsePerson.Type1) == 'O')
          {
            ExitState = "SI0000_ORG_CANNOT_BE_DISPLAYED";

            return;
          }
        }
        else
        {
          // mjr
          // -----------------------------------------------
          // 2001/06/12
          // Not an error as the person may only be known to AE
          // ------------------------------------------------------------
        }

        var field = GetField(export.SearchCsePersonsWorkSet, "number");

        field.Color = "yellow";
        field.Intensity = Intensity.High;
        field.Highlighting = Highlighting.ReverseVideo;
        field.Protected = false;

        local.Search.Assign(export.SearchCsePersonsWorkSet);
        local.Common.Flag = "3";
      }
      else if (!IsEmpty(export.SearchCsePersonsWorkSet.Ssn))
      {
        // ------------------------------------------------------------
        // Search on SSN only
        // ------------------------------------------------------------
        local.Search.Assign(local.NullCsePersonsWorkSet);

        var field1 = GetField(export.SearchSsnWorkArea, "ssnNumPart1");

        field1.Color = "yellow";
        field1.Intensity = Intensity.High;
        field1.Highlighting = Highlighting.ReverseVideo;
        field1.Protected = false;

        var field2 = GetField(export.SearchSsnWorkArea, "ssnNumPart2");

        field2.Color = "yellow";
        field2.Intensity = Intensity.High;
        field2.Highlighting = Highlighting.ReverseVideo;
        field2.Protected = false;

        var field3 = GetField(export.SearchSsnWorkArea, "ssnNumPart3");

        field3.Color = "yellow";
        field3.Intensity = Intensity.High;
        field3.Highlighting = Highlighting.ReverseVideo;
        field3.Protected = false;

        local.Search.Assign(export.SearchCsePersonsWorkSet);
        local.Common.Flag = "1";
      }
      else if (!IsEmpty(export.SearchCsePersonsWorkSet.LastName))
      {
        // ------------------------------------------------------------
        // Search on name and sex
        // ------------------------------------------------------------
        local.Search.Assign(export.SearchCsePersonsWorkSet);
        local.Search.Ssn = "";

        // ------------------------------------------------------------
        // 11/06/98 W. Campbell - Only use the first
        // 3 chars of first name in phonetic name search.
        // ------------------------------------------------------------
        if (AsChar(export.Phonetic.Flag) == 'Y')
        {
          local.Search.FirstName = Substring(local.Search.FirstName, 1, 3);
        }

        var field = GetField(export.SearchCsePersonsWorkSet, "lastName");

        field.Color = "yellow";
        field.Intensity = Intensity.High;
        field.Highlighting = Highlighting.ReverseVideo;
        field.Protected = false;

        if (!IsEmpty(export.SearchCsePersonsWorkSet.FirstName))
        {
          var field1 = GetField(export.SearchCsePersonsWorkSet, "firstName");

          field1.Color = "yellow";
          field1.Intensity = Intensity.High;
          field1.Highlighting = Highlighting.ReverseVideo;
          field1.Protected = false;
        }

        if (AsChar(export.Phonetic.Flag) == 'N')
        {
          if (!IsEmpty(export.SearchCsePersonsWorkSet.MiddleInitial))
          {
            var field1 =
              GetField(export.SearchCsePersonsWorkSet, "middleInitial");

            field1.Color = "yellow";
            field1.Intensity = Intensity.High;
            field1.Highlighting = Highlighting.ReverseVideo;
            field1.Protected = false;
          }
        }

        if (AsChar(export.Phonetic.Flag) == 'N')
        {
          if (Lt(local.NullDateWorkArea.Date, export.SearchCsePersonsWorkSet.Dob))
            
          {
            var field1 = GetField(export.SearchCsePersonsWorkSet, "dob");

            field1.Color = "yellow";
            field1.Intensity = Intensity.High;
            field1.Highlighting = Highlighting.ReverseVideo;
            field1.Protected = false;
          }
        }

        if (!IsEmpty(export.SearchCsePersonsWorkSet.Sex))
        {
          var field1 = GetField(export.SearchCsePersonsWorkSet, "sex");

          field1.Color = "yellow";
          field1.Intensity = Intensity.High;
          field1.Highlighting = Highlighting.ReverseVideo;
          field1.Protected = false;
        }

        if (export.SearchOffice.SystemGeneratedId > 0)
        {
          var field1 = GetField(export.SearchOffice, "systemGeneratedId");

          field1.Color = "yellow";
          field1.Intensity = Intensity.High;
          field1.Highlighting = Highlighting.ReverseVideo;
          field1.Protected = false;
        }

        local.Common.Flag = "2";
      }
      else
      {
        // >>
        // Invalid
      }

      if (export.Searched.IsEmpty)
      {
        export.Searched.Count = 0;
        export.Searched.Index = -1;

        ++export.Searched.Index;
        export.Searched.CheckSize();

        MoveCsePersonsWorkSet3(export.SearchCsePersonsWorkSet,
          export.Searched.Update.Searched1);
      }

      export.Searched.Index = 0;

      for(var limit = export.Searched.Count; export.Searched.Index < limit; ++
        export.Searched.Index)
      {
        if (!export.Searched.CheckSize())
        {
          break;
        }

        if (export.Searched.Count == Export.SearchedGroup.Capacity)
        {
          break;
        }

        if (Equal(export.SearchCsePersonsWorkSet.LastName,
          export.Searched.Item.Searched1.LastName) && Equal
          (export.SearchCsePersonsWorkSet.FirstName,
          export.Searched.Item.Searched1.FirstName))
        {
          MoveCsePersonsWorkSet3(export.SearchCsePersonsWorkSet,
            export.Searched.Update.Searched1);

          break;
        }
        else if (export.Searched.Count == export.Searched.Index + 1)
        {
          ++export.Searched.Index;
          export.Searched.CheckSize();

          MoveCsePersonsWorkSet3(export.SearchCsePersonsWorkSet,
            export.Searched.Update.Searched1);

          break;
        }
      }

      export.Searched.CheckIndex();
      ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
      UseCabSearchClient();

      if (!IsExitState("ACO_NI0000_SUCCESSFUL_DISPLAY"))
      {
        return;
      }

      if (export.Export1.IsEmpty)
      {
        ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";

        return;
      }

      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (Equal(export.Export1.Item.GcsePersonsWorkSet.Ssn, "000000000"))
        {
          export.Export1.Update.GcsePersonsWorkSet.Ssn = "";
        }

        var field = GetField(export.Export1.Item.Gcommon, "selectChar");

        field.Protected = false;
        field.Focused = true;
      }

      if (IsExitState("ACO_NI0000_SUCCESSFUL_DISPLAY"))
      {
        if (export.Export1.IsFull)
        {
          ExitState = "SI0000_LIST_IS_FULL_PRESS_F8";
        }
        else
        {
          ExitState = "SI0000_SUCCESSFUL_DISPLAY_HIGHLT";
        }
      }
    }
  }

  private static void MoveCommon1(Common source, Common target)
  {
    target.Percentage = source.Percentage;
    target.Flag = source.Flag;
  }

  private static void MoveCommon2(Common source, Common target)
  {
    target.Flag = source.Flag;
    target.SelectChar = source.SelectChar;
  }

  private static void MoveCsePersonsWorkSet1(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Sex = source.Sex;
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.FormattedName = source.FormattedName;
    target.LastName = source.LastName;
  }

  private static void MoveCsePersonsWorkSet2(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Sex = source.Sex;
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private static void MoveCsePersonsWorkSet3(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.FirstName = source.FirstName;
    target.LastName = source.LastName;
  }

  private static void MoveExport1(CabSearchClient.Export.ExportGroup source,
    Export.ExportGroup target)
  {
    target.Gcommon.SelectChar = source.G.SelectChar;
    target.GcsePersonsWorkSet.Assign(source.Detail);
    MoveCommon2(source.Alt, target.GexportAlt);
    target.GexportAliasKscares.Flag = source.Kscares.Flag;
    target.GexportAliasKanpay.Flag = source.Kanpay.Flag;
    target.GexportAliasCse.Flag = source.Cse.Flag;
    target.GexportAliasAe.Flag = source.Ae.Flag;
    target.GexportAliasFacts.Flag = source.Facts.Flag;
  }

  private static void MoveInformationRequest(InformationRequest source,
    InformationRequest target)
  {
    target.Number = source.Number;
    target.Type1 = source.Type1;
  }

  private static void MoveInterstateCase(InterstateCase source,
    InterstateCase target)
  {
    target.TransSerialNumber = source.TransSerialNumber;
    target.TransactionDate = source.TransactionDate;
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

  private void UseCabSearchClient()
  {
    var useImport = new CabSearchClient.Import();
    var useExport = new CabSearchClient.Export();

    useImport.Start.UniqueKey = export.HiddenCsePersonsWorkSet.UniqueKey;
    useImport.Phonetic.Percentage = export.Phonetic.Percentage;
    MoveCsePersonsWorkSet2(local.Search, useImport.CsePersonsWorkSet);
    useImport.Search.Flag = local.Common.Flag;
    useImport.Office.SystemGeneratedId = export.SearchOffice.SystemGeneratedId;

    Call(CabSearchClient.Execute, useImport, useExport);

    export.HiddenCsePersonsWorkSet.UniqueKey = useExport.Next.UniqueKey;
    local.AbendData.Assign(useExport.AbendData);
    useExport.Export1.CopyTo(export.Export1, MoveExport1);
  }

  private void UseCabSsnConvertNumToText()
  {
    var useImport = new CabSsnConvertNumToText.Import();
    var useExport = new CabSsnConvertNumToText.Export();

    MoveSsnWorkArea1(local.SsnWorkArea, useImport.SsnWorkArea);

    Call(CabSsnConvertNumToText.Execute, useImport, useExport);

    MoveSsnWorkArea2(useExport.SsnWorkArea, local.SsnWorkArea);
  }

  private void UseCabSsnConvertTextToNum()
  {
    var useImport = new CabSsnConvertTextToNum.Import();
    var useExport = new CabSsnConvertTextToNum.Export();

    useImport.SsnWorkArea.SsnText9 = local.SsnWorkArea.SsnText9;

    Call(CabSsnConvertTextToNum.Execute, useImport, useExport);

    export.SearchSsnWorkArea.Assign(useExport.SsnWorkArea);
  }

  private void UseCabZeroFillNumber1()
  {
    var useImport = new CabZeroFillNumber.Import();
    var useExport = new CabZeroFillNumber.Export();

    useImport.Case1.Number = export.Next.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    export.Next.Number = useImport.Case1.Number;
  }

  private void UseCabZeroFillNumber2()
  {
    var useImport = new CabZeroFillNumber.Import();
    var useExport = new CabZeroFillNumber.Export();

    useImport.CsePersonsWorkSet.Number = export.SearchCsePersonsWorkSet.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    export.SearchCsePersonsWorkSet.Number = useImport.CsePersonsWorkSet.Number;
  }

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
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

    useImport.CsePersonsWorkSet.Number = export.SearchCsePersonsWorkSet.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiFormatCsePersonName1()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    useImport.CsePersonsWorkSet.Assign(export.Export1.Item.GcsePersonsWorkSet);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    export.Selected.FormattedName = useExport.CsePersonsWorkSet.FormattedName;
  }

  private void UseSiFormatCsePersonName2()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    useImport.CsePersonsWorkSet.Assign(export.Export1.Item.GcsePersonsWorkSet);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    export.ToRegi.Update.DetailToRegi.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private void UseSiInrdUpdateInformationReq()
  {
    var useImport = new SiInrdUpdateInformationReq.Import();
    var useExport = new SiInrdUpdateInformationReq.Export();

    useImport.InformationRequest.Assign(local.InformationRequest);

    Call(SiInrdUpdateInformationReq.Execute, useImport, useExport);
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.SearchCsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    export.SearchCsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", export.SearchCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadInformationRequest()
  {
    entities.InformationRequest.Populated = false;

    return Read("ReadInformationRequest",
      (db, command) =>
      {
        db.SetInt64(command, "numb", import.FromInrdInformationRequest.Number);
      },
      (db, reader) =>
      {
        entities.InformationRequest.Number = db.GetInt64(reader, 0);
        entities.InformationRequest.NonparentQuestionnaireSent =
          db.GetNullableString(reader, 1);
        entities.InformationRequest.ParentQuestionnaireSent =
          db.GetNullableString(reader, 2);
        entities.InformationRequest.PaternityQuestionnaireSent =
          db.GetNullableString(reader, 3);
        entities.InformationRequest.ApplicationSentIndicator =
          db.GetString(reader, 4);
        entities.InformationRequest.QuestionnaireTypeIndicator =
          db.GetString(reader, 5);
        entities.InformationRequest.DateReceivedByCseComplete =
          db.GetNullableDate(reader, 6);
        entities.InformationRequest.DateReceivedByCseIncomplete =
          db.GetNullableDate(reader, 7);
        entities.InformationRequest.DateApplicationRequested =
          db.GetDate(reader, 8);
        entities.InformationRequest.CallerLastName =
          db.GetNullableString(reader, 9);
        entities.InformationRequest.CallerFirstName = db.GetString(reader, 10);
        entities.InformationRequest.CallerMiddleInitial =
          db.GetString(reader, 11);
        entities.InformationRequest.InquirerNameSuffix =
          db.GetNullableString(reader, 12);
        entities.InformationRequest.ApplicantLastName =
          db.GetNullableString(reader, 13);
        entities.InformationRequest.ApplicantFirstName =
          db.GetNullableString(reader, 14);
        entities.InformationRequest.ApplicantMiddleInitial =
          db.GetNullableString(reader, 15);
        entities.InformationRequest.ApplicantNameSuffix =
          db.GetString(reader, 16);
        entities.InformationRequest.ApplicantStreet1 =
          db.GetNullableString(reader, 17);
        entities.InformationRequest.ApplicantStreet2 =
          db.GetNullableString(reader, 18);
        entities.InformationRequest.ApplicantCity =
          db.GetNullableString(reader, 19);
        entities.InformationRequest.ApplicantState =
          db.GetNullableString(reader, 20);
        entities.InformationRequest.ApplicantZip5 =
          db.GetNullableString(reader, 21);
        entities.InformationRequest.ApplicantZip4 =
          db.GetNullableString(reader, 22);
        entities.InformationRequest.ApplicantZip3 =
          db.GetNullableString(reader, 23);
        entities.InformationRequest.ApplicantPhone =
          db.GetNullableInt32(reader, 24);
        entities.InformationRequest.DateApplicationSent =
          db.GetNullableDate(reader, 25);
        entities.InformationRequest.Type1 = db.GetString(reader, 26);
        entities.InformationRequest.ServiceCode =
          db.GetNullableString(reader, 27);
        entities.InformationRequest.ReasonIncomplete =
          db.GetNullableString(reader, 28);
        entities.InformationRequest.Note = db.GetNullableString(reader, 29);
        entities.InformationRequest.CreatedBy = db.GetString(reader, 30);
        entities.InformationRequest.CreatedTimestamp =
          db.GetDateTime(reader, 31);
        entities.InformationRequest.LastUpdatedBy =
          db.GetNullableString(reader, 32);
        entities.InformationRequest.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 33);
        entities.InformationRequest.ReasonDenied =
          db.GetNullableString(reader, 34);
        entities.InformationRequest.DateDenied = db.GetNullableDate(reader, 35);
        entities.InformationRequest.ApplicantAreaCode =
          db.GetNullableInt32(reader, 36);
        entities.InformationRequest.ApplicationProcessedInd =
          db.GetNullableString(reader, 37);
        entities.InformationRequest.NameSearchComplete =
          db.GetNullableString(reader, 38);
        entities.InformationRequest.ReopenReasonType =
          db.GetNullableString(reader, 39);
        entities.InformationRequest.MiscellaneousReason =
          db.GetNullableString(reader, 40);
        entities.InformationRequest.Populated = true;
      });
  }

  private bool ReadOffice()
  {
    entities.Office.Populated = false;

    return Read("ReadOffice",
      (db, command) =>
      {
        db.SetInt32(command, "officeId", export.SearchOffice.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.DiscontinueDate = db.GetNullableDate(reader, 1);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 2);
        entities.Office.Populated = true;
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
      /// A value of Gcommon.
      /// </summary>
      [JsonPropertyName("gcommon")]
      public Common Gcommon
      {
        get => gcommon ??= new();
        set => gcommon = value;
      }

      /// <summary>
      /// A value of GcsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("gcsePersonsWorkSet")]
      public CsePersonsWorkSet GcsePersonsWorkSet
      {
        get => gcsePersonsWorkSet ??= new();
        set => gcsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of GimportAlt.
      /// </summary>
      [JsonPropertyName("gimportAlt")]
      public Common GimportAlt
      {
        get => gimportAlt ??= new();
        set => gimportAlt = value;
      }

      /// <summary>
      /// A value of GimportAliasKscares.
      /// </summary>
      [JsonPropertyName("gimportAliasKscares")]
      public Common GimportAliasKscares
      {
        get => gimportAliasKscares ??= new();
        set => gimportAliasKscares = value;
      }

      /// <summary>
      /// A value of GimportAliasKanpay.
      /// </summary>
      [JsonPropertyName("gimportAliasKanpay")]
      public Common GimportAliasKanpay
      {
        get => gimportAliasKanpay ??= new();
        set => gimportAliasKanpay = value;
      }

      /// <summary>
      /// A value of GimportAliasCse.
      /// </summary>
      [JsonPropertyName("gimportAliasCse")]
      public Common GimportAliasCse
      {
        get => gimportAliasCse ??= new();
        set => gimportAliasCse = value;
      }

      /// <summary>
      /// A value of GimportAliasAe.
      /// </summary>
      [JsonPropertyName("gimportAliasAe")]
      public Common GimportAliasAe
      {
        get => gimportAliasAe ??= new();
        set => gimportAliasAe = value;
      }

      /// <summary>
      /// A value of GimportAliasFacts.
      /// </summary>
      [JsonPropertyName("gimportAliasFacts")]
      public Common GimportAliasFacts
      {
        get => gimportAliasFacts ??= new();
        set => gimportAliasFacts = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 117;

      private Common gcommon;
      private CsePersonsWorkSet gcsePersonsWorkSet;
      private Common gimportAlt;
      private Common gimportAliasKscares;
      private Common gimportAliasKanpay;
      private Common gimportAliasCse;
      private Common gimportAliasAe;
      private Common gimportAliasFacts;
    }

    /// <summary>A ToRegiGroup group.</summary>
    [Serializable]
    public class ToRegiGroup
    {
      /// <summary>
      /// A value of DetailToRegi.
      /// </summary>
      [JsonPropertyName("detailToRegi")]
      public CsePersonsWorkSet DetailToRegi
      {
        get => detailToRegi ??= new();
        set => detailToRegi = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 30;

      private CsePersonsWorkSet detailToRegi;
    }

    /// <summary>A SearchedGroup group.</summary>
    [Serializable]
    public class SearchedGroup
    {
      /// <summary>
      /// A value of Searched1.
      /// </summary>
      [JsonPropertyName("searched1")]
      public CsePersonsWorkSet Searched1
      {
        get => searched1 ??= new();
        set => searched1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private CsePersonsWorkSet searched1;
    }

    /// <summary>
    /// A value of FromRegi.
    /// </summary>
    [JsonPropertyName("fromRegi")]
    public Common FromRegi
    {
      get => fromRegi ??= new();
      set => fromRegi = value;
    }

    /// <summary>
    /// A value of FromInterstateCase.
    /// </summary>
    [JsonPropertyName("fromInterstateCase")]
    public InterstateCase FromInterstateCase
    {
      get => fromInterstateCase ??= new();
      set => fromInterstateCase = value;
    }

    /// <summary>
    /// A value of FromPaReferral.
    /// </summary>
    [JsonPropertyName("fromPaReferral")]
    public PaReferral FromPaReferral
    {
      get => fromPaReferral ??= new();
      set => fromPaReferral = value;
    }

    /// <summary>
    /// A value of FromInrdCommon.
    /// </summary>
    [JsonPropertyName("fromInrdCommon")]
    public Common FromInrdCommon
    {
      get => fromInrdCommon ??= new();
      set => fromInrdCommon = value;
    }

    /// <summary>
    /// A value of FromPar1.
    /// </summary>
    [JsonPropertyName("fromPar1")]
    public Common FromPar1
    {
      get => fromPar1 ??= new();
      set => fromPar1 = value;
    }

    /// <summary>
    /// A value of FromIapi.
    /// </summary>
    [JsonPropertyName("fromIapi")]
    public Common FromIapi
    {
      get => fromIapi ??= new();
      set => fromIapi = value;
    }

    /// <summary>
    /// A value of HiddenCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("hiddenCsePersonsWorkSet")]
    public CsePersonsWorkSet HiddenCsePersonsWorkSet
    {
      get => hiddenCsePersonsWorkSet ??= new();
      set => hiddenCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public CsePersonsWorkSet Selected
    {
      get => selected ??= new();
      set => selected = value;
    }

    /// <summary>
    /// A value of Phonetic.
    /// </summary>
    [JsonPropertyName("phonetic")]
    public Common Phonetic
    {
      get => phonetic ??= new();
      set => phonetic = value;
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
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Case1 Next
    {
      get => next ??= new();
      set => next = value;
    }

    /// <summary>
    /// A value of SearchCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("searchCsePersonsWorkSet")]
    public CsePersonsWorkSet SearchCsePersonsWorkSet
    {
      get => searchCsePersonsWorkSet ??= new();
      set => searchCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of SearchSsnWorkArea.
    /// </summary>
    [JsonPropertyName("searchSsnWorkArea")]
    public SsnWorkArea SearchSsnWorkArea
    {
      get => searchSsnWorkArea ??= new();
      set => searchSsnWorkArea = value;
    }

    /// <summary>
    /// A value of InitialExecution.
    /// </summary>
    [JsonPropertyName("initialExecution")]
    public Common InitialExecution
    {
      get => initialExecution ??= new();
      set => initialExecution = value;
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
    /// Gets a value of ToRegi.
    /// </summary>
    [JsonIgnore]
    public Array<ToRegiGroup> ToRegi => toRegi ??= new(ToRegiGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ToRegi for json serialization.
    /// </summary>
    [JsonPropertyName("toRegi")]
    [Computed]
    public IList<ToRegiGroup> ToRegi_Json
    {
      get => toRegi;
      set => ToRegi.Assign(value);
    }

    /// <summary>
    /// A value of FromInrdInformationRequest.
    /// </summary>
    [JsonPropertyName("fromInrdInformationRequest")]
    public InformationRequest FromInrdInformationRequest
    {
      get => fromInrdInformationRequest ??= new();
      set => fromInrdInformationRequest = value;
    }

    /// <summary>
    /// A value of SearchOffice.
    /// </summary>
    [JsonPropertyName("searchOffice")]
    public Office SearchOffice
    {
      get => searchOffice ??= new();
      set => searchOffice = value;
    }

    /// <summary>
    /// A value of SearchPrev.
    /// </summary>
    [JsonPropertyName("searchPrev")]
    public SsnWorkArea SearchPrev
    {
      get => searchPrev ??= new();
      set => searchPrev = value;
    }

    /// <summary>
    /// Gets a value of Searched.
    /// </summary>
    [JsonIgnore]
    public Array<SearchedGroup> Searched => searched ??= new(
      SearchedGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Searched for json serialization.
    /// </summary>
    [JsonPropertyName("searched")]
    [Computed]
    public IList<SearchedGroup> Searched_Json
    {
      get => searched;
      set => Searched.Assign(value);
    }

    private Common fromRegi;
    private InterstateCase fromInterstateCase;
    private PaReferral fromPaReferral;
    private Common fromInrdCommon;
    private Common fromPar1;
    private Common fromIapi;
    private CsePersonsWorkSet hiddenCsePersonsWorkSet;
    private CsePersonsWorkSet selected;
    private Common phonetic;
    private Standard standard;
    private Case1 next;
    private CsePersonsWorkSet searchCsePersonsWorkSet;
    private SsnWorkArea searchSsnWorkArea;
    private Common initialExecution;
    private NextTranInfo hiddenNextTranInfo;
    private Array<ImportGroup> import1;
    private Array<ToRegiGroup> toRegi;
    private InformationRequest fromInrdInformationRequest;
    private Office searchOffice;
    private SsnWorkArea searchPrev;
    private Array<SearchedGroup> searched;
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
      /// A value of Gcommon.
      /// </summary>
      [JsonPropertyName("gcommon")]
      public Common Gcommon
      {
        get => gcommon ??= new();
        set => gcommon = value;
      }

      /// <summary>
      /// A value of GcsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("gcsePersonsWorkSet")]
      public CsePersonsWorkSet GcsePersonsWorkSet
      {
        get => gcsePersonsWorkSet ??= new();
        set => gcsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of GexportAlt.
      /// </summary>
      [JsonPropertyName("gexportAlt")]
      public Common GexportAlt
      {
        get => gexportAlt ??= new();
        set => gexportAlt = value;
      }

      /// <summary>
      /// A value of GexportAliasKscares.
      /// </summary>
      [JsonPropertyName("gexportAliasKscares")]
      public Common GexportAliasKscares
      {
        get => gexportAliasKscares ??= new();
        set => gexportAliasKscares = value;
      }

      /// <summary>
      /// A value of GexportAliasKanpay.
      /// </summary>
      [JsonPropertyName("gexportAliasKanpay")]
      public Common GexportAliasKanpay
      {
        get => gexportAliasKanpay ??= new();
        set => gexportAliasKanpay = value;
      }

      /// <summary>
      /// A value of GexportAliasCse.
      /// </summary>
      [JsonPropertyName("gexportAliasCse")]
      public Common GexportAliasCse
      {
        get => gexportAliasCse ??= new();
        set => gexportAliasCse = value;
      }

      /// <summary>
      /// A value of GexportAliasAe.
      /// </summary>
      [JsonPropertyName("gexportAliasAe")]
      public Common GexportAliasAe
      {
        get => gexportAliasAe ??= new();
        set => gexportAliasAe = value;
      }

      /// <summary>
      /// A value of GexportAliasFacts.
      /// </summary>
      [JsonPropertyName("gexportAliasFacts")]
      public Common GexportAliasFacts
      {
        get => gexportAliasFacts ??= new();
        set => gexportAliasFacts = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 117;

      private Common gcommon;
      private CsePersonsWorkSet gcsePersonsWorkSet;
      private Common gexportAlt;
      private Common gexportAliasKscares;
      private Common gexportAliasKanpay;
      private Common gexportAliasCse;
      private Common gexportAliasAe;
      private Common gexportAliasFacts;
    }

    /// <summary>A ToRegiGroup group.</summary>
    [Serializable]
    public class ToRegiGroup
    {
      /// <summary>
      /// A value of DetailToRegi.
      /// </summary>
      [JsonPropertyName("detailToRegi")]
      public CsePersonsWorkSet DetailToRegi
      {
        get => detailToRegi ??= new();
        set => detailToRegi = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 30;

      private CsePersonsWorkSet detailToRegi;
    }

    /// <summary>A SearchedGroup group.</summary>
    [Serializable]
    public class SearchedGroup
    {
      /// <summary>
      /// A value of Searched1.
      /// </summary>
      [JsonPropertyName("searched1")]
      public CsePersonsWorkSet Searched1
      {
        get => searched1 ??= new();
        set => searched1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private CsePersonsWorkSet searched1;
    }

    /// <summary>
    /// A value of FromRegi.
    /// </summary>
    [JsonPropertyName("fromRegi")]
    public Common FromRegi
    {
      get => fromRegi ??= new();
      set => fromRegi = value;
    }

    /// <summary>
    /// A value of FromInterstateCase.
    /// </summary>
    [JsonPropertyName("fromInterstateCase")]
    public InterstateCase FromInterstateCase
    {
      get => fromInterstateCase ??= new();
      set => fromInterstateCase = value;
    }

    /// <summary>
    /// A value of FromPaReferral.
    /// </summary>
    [JsonPropertyName("fromPaReferral")]
    public PaReferral FromPaReferral
    {
      get => fromPaReferral ??= new();
      set => fromPaReferral = value;
    }

    /// <summary>
    /// A value of FromInrdCommon.
    /// </summary>
    [JsonPropertyName("fromInrdCommon")]
    public Common FromInrdCommon
    {
      get => fromInrdCommon ??= new();
      set => fromInrdCommon = value;
    }

    /// <summary>
    /// A value of FromPar1.
    /// </summary>
    [JsonPropertyName("fromPar1")]
    public Common FromPar1
    {
      get => fromPar1 ??= new();
      set => fromPar1 = value;
    }

    /// <summary>
    /// A value of FromIapi.
    /// </summary>
    [JsonPropertyName("fromIapi")]
    public Common FromIapi
    {
      get => fromIapi ??= new();
      set => fromIapi = value;
    }

    /// <summary>
    /// A value of HiddenCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("hiddenCsePersonsWorkSet")]
    public CsePersonsWorkSet HiddenCsePersonsWorkSet
    {
      get => hiddenCsePersonsWorkSet ??= new();
      set => hiddenCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public CsePersonsWorkSet Selected
    {
      get => selected ??= new();
      set => selected = value;
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
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Case1 Next
    {
      get => next ??= new();
      set => next = value;
    }

    /// <summary>
    /// A value of SearchCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("searchCsePersonsWorkSet")]
    public CsePersonsWorkSet SearchCsePersonsWorkSet
    {
      get => searchCsePersonsWorkSet ??= new();
      set => searchCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of SearchSsnWorkArea.
    /// </summary>
    [JsonPropertyName("searchSsnWorkArea")]
    public SsnWorkArea SearchSsnWorkArea
    {
      get => searchSsnWorkArea ??= new();
      set => searchSsnWorkArea = value;
    }

    /// <summary>
    /// A value of Phonetic.
    /// </summary>
    [JsonPropertyName("phonetic")]
    public Common Phonetic
    {
      get => phonetic ??= new();
      set => phonetic = value;
    }

    /// <summary>
    /// A value of InitialExecution.
    /// </summary>
    [JsonPropertyName("initialExecution")]
    public Common InitialExecution
    {
      get => initialExecution ??= new();
      set => initialExecution = value;
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
    /// Gets a value of ToRegi.
    /// </summary>
    [JsonIgnore]
    public Array<ToRegiGroup> ToRegi => toRegi ??= new(ToRegiGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ToRegi for json serialization.
    /// </summary>
    [JsonPropertyName("toRegi")]
    [Computed]
    public IList<ToRegiGroup> ToRegi_Json
    {
      get => toRegi;
      set => ToRegi.Assign(value);
    }

    /// <summary>
    /// A value of FromInrdInformationRequest.
    /// </summary>
    [JsonPropertyName("fromInrdInformationRequest")]
    public InformationRequest FromInrdInformationRequest
    {
      get => fromInrdInformationRequest ??= new();
      set => fromInrdInformationRequest = value;
    }

    /// <summary>
    /// A value of SearchOffice.
    /// </summary>
    [JsonPropertyName("searchOffice")]
    public Office SearchOffice
    {
      get => searchOffice ??= new();
      set => searchOffice = value;
    }

    /// <summary>
    /// A value of SearchPrev.
    /// </summary>
    [JsonPropertyName("searchPrev")]
    public SsnWorkArea SearchPrev
    {
      get => searchPrev ??= new();
      set => searchPrev = value;
    }

    /// <summary>
    /// A value of ZdelExportDeleteLater.
    /// </summary>
    [JsonPropertyName("zdelExportDeleteLater")]
    public AbendData ZdelExportDeleteLater
    {
      get => zdelExportDeleteLater ??= new();
      set => zdelExportDeleteLater = value;
    }

    /// <summary>
    /// Gets a value of Searched.
    /// </summary>
    [JsonIgnore]
    public Array<SearchedGroup> Searched => searched ??= new(
      SearchedGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Searched for json serialization.
    /// </summary>
    [JsonPropertyName("searched")]
    [Computed]
    public IList<SearchedGroup> Searched_Json
    {
      get => searched;
      set => Searched.Assign(value);
    }

    private Common fromRegi;
    private InterstateCase fromInterstateCase;
    private PaReferral fromPaReferral;
    private Common fromInrdCommon;
    private Common fromPar1;
    private Common fromIapi;
    private CsePersonsWorkSet hiddenCsePersonsWorkSet;
    private CsePersonsWorkSet selected;
    private Standard standard;
    private Case1 next;
    private CsePersonsWorkSet searchCsePersonsWorkSet;
    private SsnWorkArea searchSsnWorkArea;
    private Common phonetic;
    private Common initialExecution;
    private NextTranInfo hiddenNextTranInfo;
    private Array<ExportGroup> export1;
    private Array<ToRegiGroup> toRegi;
    private InformationRequest fromInrdInformationRequest;
    private Office searchOffice;
    private SsnWorkArea searchPrev;
    private AbendData zdelExportDeleteLater;
    private Array<SearchedGroup> searched;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of InformationRequest.
    /// </summary>
    [JsonPropertyName("informationRequest")]
    public InformationRequest InformationRequest
    {
      get => informationRequest ??= new();
      set => informationRequest = value;
    }

    /// <summary>
    /// A value of Formatted.
    /// </summary>
    [JsonPropertyName("formatted")]
    public CsePersonsWorkSet Formatted
    {
      get => formatted ??= new();
      set => formatted = value;
    }

    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public Common Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of NextAlias.
    /// </summary>
    [JsonPropertyName("nextAlias")]
    public CsePersonsWorkSet NextAlias
    {
      get => nextAlias ??= new();
      set => nextAlias = value;
    }

    /// <summary>
    /// A value of Phonetic.
    /// </summary>
    [JsonPropertyName("phonetic")]
    public Common Phonetic
    {
      get => phonetic ??= new();
      set => phonetic = value;
    }

    /// <summary>
    /// A value of Offices.
    /// </summary>
    [JsonPropertyName("offices")]
    public Common Offices
    {
      get => offices ??= new();
      set => offices = value;
    }

    /// <summary>
    /// A value of ZdelLocalRmainingSpceFrOffc.
    /// </summary>
    [JsonPropertyName("zdelLocalRmainingSpceFrOffc")]
    public Common ZdelLocalRmainingSpceFrOffc
    {
      get => zdelLocalRmainingSpceFrOffc ??= new();
      set => zdelLocalRmainingSpceFrOffc = value;
    }

    /// <summary>
    /// A value of ZdelLocalPosition.
    /// </summary>
    [JsonPropertyName("zdelLocalPosition")]
    public Common ZdelLocalPosition
    {
      get => zdelLocalPosition ??= new();
      set => zdelLocalPosition = value;
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
    /// A value of NullCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("nullCsePersonsWorkSet")]
    public CsePersonsWorkSet NullCsePersonsWorkSet
    {
      get => nullCsePersonsWorkSet ??= new();
      set => nullCsePersonsWorkSet = value;
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
    /// A value of ZdelLocalTextnum.
    /// </summary>
    [JsonPropertyName("zdelLocalTextnum")]
    public WorkArea ZdelLocalTextnum
    {
      get => zdelLocalTextnum ??= new();
      set => zdelLocalTextnum = value;
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
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public CsePersonsWorkSet Search
    {
      get => search ??= new();
      set => search = value;
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
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    private InformationRequest informationRequest;
    private CsePersonsWorkSet formatted;
    private Common max;
    private CsePersonsWorkSet nextAlias;
    private Common phonetic;
    private Common offices;
    private Common zdelLocalRmainingSpceFrOffc;
    private Common zdelLocalPosition;
    private DateWorkArea nullDateWorkArea;
    private CsePersonsWorkSet nullCsePersonsWorkSet;
    private DateWorkArea current;
    private WorkArea zdelLocalTextnum;
    private TextWorkArea textWorkArea;
    private CsePersonsWorkSet search;
    private SsnWorkArea ssnWorkArea;
    private AbendData abendData;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common common;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of InformationRequest.
    /// </summary>
    [JsonPropertyName("informationRequest")]
    public InformationRequest InformationRequest
    {
      get => informationRequest ??= new();
      set => informationRequest = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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

    private InformationRequest informationRequest;
    private CaseRole caseRole;
    private Office office;
    private OfficeServiceProvider officeServiceProvider;
    private CaseAssignment caseAssignment;
    private Case1 case1;
    private CsePerson csePerson;
  }
#endregion
}
