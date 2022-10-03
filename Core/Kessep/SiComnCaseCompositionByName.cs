// Program: SI_COMN_CASE_COMPOSITION_BY_NAME, ID: 370982715, model: 746.
// Short name: SWECOMNP
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
/// A program: SI_COMN_CASE_COMPOSITION_BY_NAME.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This procedure lists all of the cases that a CSE PERSON is or has been 
/// involved in.  It also lists the other people involved in each of these
/// cases.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiComnCaseCompositionByName: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_COMN_CASE_COMPOSITION_BY_NAME program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiComnCaseCompositionByName(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiComnCaseCompositionByName.
  /// </summary>
  public SiComnCaseCompositionByName(IContext context, Import import,
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
    // ------------------------------------------------------------
    // 		M A I N T E N A N C E   L O G
    // Date	  Developer	Description
    // 09-11-95  Helen Sharland Initial Dev
    // 10/29/96  G. Lofton	Added flows to PAR1 and ICAS
    // 			and added new security.
    // 04/28/97  Sid		Complete Case Re-Open logic IDCR #257/258.
    // 07/16/97  Sid		Make logic fallthru to manage sel char.
    // 			Add logic to flow to PAR1 and INRD.
    // 09/23/98 W. Campbell    Added an IF statement to prevent
    //                         executing of DISPLAY, NEXT OR PREV
    //                         logic after the end of the main CASE
    //                         statement if the exit state was
    //                         not = aco_nn0000_all_ok.
    // 11/04/98 W. Campbell    Added code to properly
    //                         handle the COMMAND of ICAS
    //                         and cause the flow to ICAS.
    // 11/05/98 W. Campbell    In called CAB
    //                         SI_COMN_REOPEN_CASE_N_CASE_UNIT,
    //                         A SORTED BY clause was added
    //                         to a READ EACH SEARCH CASE_ROLE
    //                         statement to obtain the requested
    //                         data in an order which provided for
    //                         proper subsequent processing.  This
    //                         was to solve a problem whereby the
    //                         AP was not being found in a called
    //                         CAB.
    // 11/06/98 W. Campbell    In called CAB
    //                         SI_COMN_REOPEN_CASE_N_CASE_UNIT,
    //                         added a set stmt to UPDATE
    //                         statement to update the CASE
    //                         cse_open_date to current date
    //                         when REOPENing a CLOSED CASE.
    // 11/06/98 W. Campbell    In called CAB
    //                         SI_READ_CASE_COMPS_BY_NAME,
    //                         If statement added to
    //                         interpret the ADABASE returned
    //                         exit state and change it to a
    //                         more user friendly exit state,
    //                         as per the SME request.
    // --------------------------------------------------
    // 05/14/99 W. Campbell    Added a move
    //                         statement to move import
    //                         interstate_case to export
    //                         interstate_case.  The export
    //                         view was not being populated.
    // -------------------------------------------------
    // 05/25/99 M. Lachowicz      Replace zdel exit state by
    //                            by new exit state.
    // -------------------------------------------------
    // 05/25/99 M. Lachowicz      Add new function key to transfer
    //                            to QARM screen.
    // -------------------------------------------------
    // 07/01/99 M.Lachowicz    Change property of READ
    //                         (Select Only or Cursor Only)
    // ------------------------------------------------------------
    // 11/22/99 W. Campbell      Inserted a FOR
    //                           statement to protect group
    //                           export items so that cursor
    //                           will be placed in the reopen
    //                           office message ready for
    //                           user input.  Work done on
    //                           PR#79699 along with changes to
    //                           cab SI_REOPEN_CASE_N_CASE_UNITS.
    // -----------------------------------------------
    // 12/13/99 M.Lachowicz      Add link to PEPR screen
    //                           PR # 82507.
    // ------------------------------------------------------------
    // 12/16/99 M.Lachowicz      Add RETURN from COMN screen
    //                           to Name.
    //                           PR # 82849.
    // ------------------------------------------------------------
    // 03/07/00 W. Campbell      Added new views and logic
    //                           to add Family Violence indicator to
    //                           this screen and procedure step.
    //                           The Family Violence Indicator is to
    //                           be maintained from this Transaction.
    //                           Work done on WR# 00162 for
    //                           PRWORA - Family Violence.
    // ------------------------------------------------------
    // 03/20/00 W.Campbell       Inserted code to fix a
    //                           bug in the display logic whereby
    //                           the Select character was being left
    //                           unprotected.
    // ------------------------------------------------------------
    // 03/31/00 W.Campbell       Unmatched the view
    //                           for CASE in the USE for
    //                           sc_cab_test_security so that a
    //                           case number will not be passed
    //                           since COMN only works with
    //                           person number.  It was previously
    //                           matched with export_next case.
    //                           Work done on WR# 00162
    //                           for PRWORA - Family Violence.
    // ---------------------------------------------
    // 04/20/00 W.Campbell       Changed property of a READ
    //                           back to default condition to
    //                           generate both Select and Cursor.
    //                           This was in the logic for PFK 20
    //                           command = SOURCE.
    //                           This was done to fix a -811 sql
    //                           error due to the fact that there can
    //                           be more than 1 row with the same CSE
    //                           case number as a foreign key in the PA
    //                           Referral.  Work done on WR#000162
    //                           for PRWORA - Family Violence.
    // ------------------------------------------------------------
    // 06/14/00 W.Campbell       Made changes for  WR#173
    //                           Family Violence Enhancements.
    //                           1. Added new field on the Screen -
    //                              FV letter sent (date).
    //                           2. Changed definition of PFK20
    //                              from SOURCE to FVltr.
    //                           Logic was added/changed to
    //                           accommodate these enhancements
    // ------------------------------------------------------------
    // 08/03/00 W.Campbell       Added code for NEXTTRAN
    //                           flow to also take the AP or AR
    //                           person number from the selected
    //                           Case.  Work done on WR#00183-B
    // ---------------------------------------------
    // 08/03/00 W.Campbell       Added code to provide OFFICE and
    //                           Service Provider info in the second
    //                           line of output for each case
    //                           provided in the list of Cases.
    //                           Work done on WR#00182.
    // ---------------------------------------------
    // 03/08/2002	M Ramirez	PR138466
    // Don't send documents if there is incoming interstate involvement
    // -----------------------------------------------
    // Install changes to fix PR153041. These changes are being installed so 
    // that the AP # is carried when XXNEXTXX to a screen. The logic for WR#0018
    // -B had been commented out. After discussing the matter with Karen and
    // Jennifer and showing them the problem they felt the change should be
    // installed. LBachura 1-7-03.
    // PR156551. May 22, 2003. Installed the logic to check and see if the AR is
    // inactive. Old logic is below and has been disabled. LBachura
    // PR156551. 07/16/2003  GVandy Corrected logic checking if the AR is 
    // inactive.  Previous changes were causing an abend.
    // PR252687  9/27/2007  CLocke  Corrected logic to check if a person is on 
    // more than 1 case.  If one case is interstate while the other case(s) are
    // KS cases, go ahead and produce the fv letter.
    // CQ16830  5/3/2010  RMathews  Modified views for new interstate indicator 
    // field on COMN screen.
    // **************************************************************************************************
    // *                                      
    // Maintenance Log
    // 
    // *
    // **************************************************************************************************
    // *    DATE       NAME             PR/SR #       DESCRIPTION OF THE CHANGE
    // *
    // * ----------  -----------------  ---------     
    // --------------------------------------------------*
    // * 05/27/2011  Raj S              CQ9690        Set FVI_SET_DATE and 
    // FVI_UPDATED_BY when setting  *
    // *
    // 
    // FVI. Re-clicked the code.
    // *
    // **************************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);
    local.Current.Date = Now().Date;
    local.Current.Timestamp = Now();

    if (Equal(global.Command, "CLEAR"))
    {
      var field = GetField(export.Search, "number");

      field.Protected = false;
      field.Focused = true;

      return;
    }

    // ---------------------------------------------
    // Move Imports to Exports
    // ---------------------------------------------
    export.CsePerson.Assign(import.CsePerson);
    export.FvPrompt.SelectChar = import.FvPrompt.SelectChar;
    MoveStandard(import.Standard, export.Standard);
    export.Search.Assign(import.Search);
    export.Next.Number = import.Next.Number;
    MoveOffice(import.ReopenOffice, export.ReopenOffice);
    export.ReopenWorkArea.Text40 = import.ReopenWorkArea.Text40;
    export.PaReferral.Assign(import.PaReferral);
    export.GoToInrdReopen.Text1 = import.GoToInrdReopen.Text1;
    export.Ar.Number = import.Ar.Number;

    // -------------------------------------------------
    // 05/14/99 W. Campbell    Added a move
    // statement to move import
    // interstate_case to export
    // interstate_case.  The export
    // view was not being populated.
    // -------------------------------------------------
    MoveInterstateCase(import.InterstateCase, export.InterstateCase);
    export.InformationRequest.Number = import.InformationRequest.Number;
    export.NotFromReferral.Flag = import.NotFromReferral.Flag;

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (!import.Import1.CheckSize())
      {
        break;
      }

      export.Export1.Index = import.Import1.Index;
      export.Export1.CheckSize();

      MoveCsePerson(import.Import1.Item.DetCsePerson,
        export.Export1.Update.DetCsePerson);
      export.Export1.Update.DetCommon.SelectChar =
        import.Import1.Item.DetCommon.SelectChar;
      MoveCase1(import.Import1.Item.DetCase1, export.Export1.Update.DetCase1);
      export.Export1.Update.DetCsePersonsWorkSet.Assign(
        import.Import1.Item.DetCsePersonsWorkSet);
      export.Export1.Update.DetCase2.Type1 = import.Import1.Item.DetCase2.Type1;
      export.Export1.Update.DetFamily.Type1 =
        import.Import1.Item.DetFamily.Type1;
      export.Export1.Update.DetStatus.Text1 =
        import.Import1.Item.DetStatus.Text1;
      export.Export1.Update.DetInter.Text1 = import.Import1.Item.DetInter.Text1;

      // ------------------------------------------------------------
      // 03/20/00 W.Campbell - Inserted code to fix a
      // bug in the display logic whereby the Select
      // character was being left unprotected.
      // ------------------------------------------------------------
      // ------------------------------------------------------------
      // 08/03/00 W.Campbell - Added verify function
      // for numeric test to the following IF stmt so that
      // the lines where Case number contains non-
      // numeric and non-blank data will be treated
      // the same as lines with blank case numbers.
      // Work done on WR000182.
      // ------------------------------------------------------------
      if (!IsEmpty(export.Export1.Item.DetCase1.Number) && Verify
        (export.Export1.Item.DetCase1.Number, "0123456789") == 0)
      {
        var field1 = GetField(export.Export1.Item.DetCommon, "selectChar");

        field1.Color = "green";
        field1.Highlighting = Highlighting.Underscore;
        field1.Protected = false;
        field1.Focused = true;

        var field2 = GetField(export.Export1.Item.DetCase1, "number");

        field2.Color = "cyan";
        field2.Protected = true;
      }
      else
      {
        var field = GetField(export.Export1.Item.DetCommon, "selectChar");

        field.Color = "green";
        field.Intensity = Intensity.Dark;
        field.Protected = true;

        // ------------------------------------------------------------
        // 08/03/00 W.Campbell - Added code to check
        // the lines where Case number contains non-
        // numeric and non-blank data to set the case
        // number to color white so that the office and
        // service provider will display in white in those
        // lines.  Work done on WR000182.
        // ------------------------------------------------------------
        if (!IsEmpty(export.Export1.Item.DetCase1.Number) && Verify
          (export.Export1.Item.DetCase1.Number, "0123456789") > 0)
        {
          var field1 = GetField(export.Export1.Item.DetCase1, "number");

          field1.Color = "white";
          field1.Protected = true;
        }
      }
    }

    import.Import1.CheckIndex();

    // ------------------------------------------------------------
    // Move Hidden Import Views to Hidden Export Views
    // ------------------------------------------------------------
    export.HiddenStandard.PageNumber = import.HiddenStandard.PageNumber;
    export.Previous.Number = import.Previous.Number;
    export.HiddenPrev.Assign(import.HiddenPrev);
    export.HfromIapi.Flag = import.HfromIapi.Flag;
    export.HfromInrd.Flag = import.HfromInrd.Flag;
    export.HfromPar1.Flag = import.HfromPar1.Flag;
    export.GoToInrdReopen.Text1 = import.GoToInrdReopen.Text1;

    for(import.PageKeys.Index = 0; import.PageKeys.Index < import
      .PageKeys.Count; ++import.PageKeys.Index)
    {
      if (!import.PageKeys.CheckSize())
      {
        break;
      }

      export.PageKeys.Index = import.PageKeys.Index;
      export.PageKeys.CheckSize();

      export.PageKeys.Update.PageKeyCase.Number =
        import.PageKeys.Item.PageKeyCase.Number;
      export.PageKeys.Update.PageKeyCaseRole.Type1 =
        import.PageKeys.Item.PageKeyCaseRole.Type1;
      export.PageKeys.Update.PageKeyCsePerson.Number =
        import.PageKeys.Item.PageKeyCsePerson.Number;
      export.PageKeys.Update.PageKeyStatus.Text1 =
        import.PageKeys.Item.PageKeyStatus.Text1;
    }

    import.PageKeys.CheckIndex();

    if (import.HiddenStandard.PageNumber == 0)
    {
      export.HiddenStandard.PageNumber = 1;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.Next.Number = export.HiddenNextTranInfo.CaseNumber ?? Spaces(10);
        export.Search.Number = export.HiddenNextTranInfo.CsePersonNumber ?? Spaces
          (10);
        global.Command = "DISPLAY";
      }
      else
      {
      }
    }

    UseCabZeroFillNumber();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      var field = GetField(export.Next, "number");

      field.Error = true;
    }

    // ---------------------------------------------
    //         	N E X T   T R A N
    // ---------------------------------------------
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // 12/16/99 M.L Start
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (!export.Export1.CheckSize())
        {
          break;
        }

        switch(AsChar(export.Export1.Item.DetCommon.SelectChar))
        {
          case 'S':
            ++local.Common.Count;

            break;
          case ' ':
            break;
          default:
            var field = GetField(export.Export1.Item.DetCommon, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            goto Test1;
        }
      }

      export.Export1.CheckIndex();

      if (local.Common.Count > 1)
      {
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.DetCommon.SelectChar) == 'S')
          {
            var field = GetField(export.Export1.Item.DetCommon, "selectChar");

            field.Error = true;
          }
        }

        export.Export1.CheckIndex();
        ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

        goto Test1;
      }

      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (!export.Export1.CheckSize())
        {
          break;
        }

        if (AsChar(export.Export1.Item.DetCommon.SelectChar) == 'S')
        {
          // ---------------------------------------------
          // Move selected view to a single view that will
          // be mapped back to calling screen view.
          // ---------------------------------------------
          export.Next.Number = export.Export1.Item.DetCase1.Number;
        }
      }

      export.Export1.CheckIndex();

      // 12/16/99 M.L End
      export.HiddenNextTranInfo.CaseNumber = export.Next.Number;
      export.HiddenNextTranInfo.CsePersonNumber = export.Search.Number;

      // Following changes is for installation of PR153041. These changes are 
      // being installed so that the AP # is carried when XXNEXTXX to a screen.
      // The logic for WR#0018-B had been commented out. After discussing the
      // matter with Karen and Jennifer and showing them the problem they felt
      // the change should be installed. LBachura 1-7-03.
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (!export.Export1.CheckSize())
        {
          break;
        }

        if (AsChar(export.Export1.Item.DetCommon.SelectChar) == 'S')
        {
          if (Equal(export.Export1.Item.DetCase2.Type1, "AP"))
          {
            export.HiddenNextTranInfo.CsePersonNumberAp =
              export.Export1.Item.DetCsePersonsWorkSet.Number;
            export.HiddenNextTranInfo.CsePersonNumberObligor =
              export.Export1.Item.DetCsePersonsWorkSet.Number;

            break;
          }
        }
      }

      export.Export1.CheckIndex();

      // Above code is for installation of PR153041. 1-7-03 Lbachura
      UseScCabNextTranPut();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }
    }

Test1:

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    // ---------------------------------------------
    //        S E C U R I T Y   L O G I C
    // ---------------------------------------------
    // to validate action level security
    if (Equal(global.Command, "NAME") || Equal(global.Command, "ICAS") || Equal
      (global.Command, "PAR1") || Equal(global.Command, "ROLE") || Equal
      (global.Command, "QARM") || Equal(global.Command, "PEPR") || Equal
      (global.Command, "RTLIST") || Equal(global.Command, "INRD"))
    {
      // ******************************************
      // 06/14/00 W.Campbell - Added new logic for
      // FV letter sent date -
      // Added new COMMAND fvltr for the sending of a
      // Family Violence removal notification letter.
      // This COMMAND is triggered by PFK20.
      // Previously PFK20 generated COMMAND
      // source which performed an entirely
      // different set of logic which was removed
      // (disabled) with this change.  The expression
      // OR COMMAND IS EQUAL TO source
      // was removed from this IF statement.
      // ******************************************
    }
    else
    {
      // ---------------------------------------------
      // 03/31/00 W.Campbell - Unmatched the view
      // for CASE in the following USE for
      // sc_cab_test_security so that a case number
      // will not be passed since COMN only works with
      // person number.  It was previously matched with
      // export_next case.  Work done on WR# 00162
      // for PRWORA - Family Violence.
      // ---------------------------------------------
      UseScCabTestSecurity();
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ---------------------------------------------
    //        P F K E Y   P R O C E S S I N G
    // ---------------------------------------------
    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    // ------------------------------------------------------------
    // Check how many selections have been made.
    // Do not allow scrolling when a selection has been made.
    // ------------------------------------------------------------
    local.Common.Count = 0;

    for(export.Export1.Index = 0; export.Export1.Index < export.Export1.Count; ++
      export.Export1.Index)
    {
      if (!export.Export1.CheckSize())
      {
        break;
      }

      switch(AsChar(export.Export1.Item.DetCommon.SelectChar))
      {
        case 'S':
          ++local.Common.Count;

          break;
        case ' ':
          break;
        default:
          var field = GetField(export.Export1.Item.DetCommon, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

          break;
      }
    }

    export.Export1.CheckIndex();

    if (local.Common.Count > 0 && (Equal(global.Command, "PREV") || Equal
      (global.Command, "NEXT")))
    {
      ExitState = "ACO_NE0000_SCROLL_INVALID_W_SEL";
    }

    if (local.Common.Count > 1)
    {
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (!export.Export1.CheckSize())
        {
          break;
        }

        if (AsChar(export.Export1.Item.DetCommon.SelectChar) == 'S')
        {
          var field = GetField(export.Export1.Item.DetCommon, "selectChar");

          field.Error = true;
        }
      }

      export.Export1.CheckIndex();

      // 05/25/99 M. Lachowicz      Replace zdel exit state by
      //                            by new exit state.
      ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
    }

    for(export.Export1.Index = 0; export.Export1.Index < export.Export1.Count; ++
      export.Export1.Index)
    {
      if (!export.Export1.CheckSize())
      {
        break;
      }

      // ------------------------------------------------------------
      // 08/03/00 W.Campbell - Added verify function
      // for numeric test to the following IF stmt so that
      // the lines where Case number contains non-
      // numeric and non-blank data will be treated
      // the same as lines with blank case numbers.
      // Work done on WR000182.
      // ------------------------------------------------------------
      if (!IsEmpty(export.Export1.Item.DetCase1.Number) && Verify
        (export.Export1.Item.DetCase1.Number, "0123456789") == 0)
      {
        var field1 = GetField(export.Export1.Item.DetCommon, "selectChar");

        field1.Color = "green";
        field1.Highlighting = Highlighting.Underscore;
        field1.Protected = false;
        field1.Focused = true;

        var field2 = GetField(export.Export1.Item.DetCase1, "number");

        field2.Color = "cyan";
        field2.Protected = true;
      }
      else
      {
        // ------------------------------------------------------------
        // 03/20/00 W.Campbell - Inserted code to fix a
        // bug in the display logic whereby the Select
        // character was being left unprotected.
        // ------------------------------------------------------------
        var field = GetField(export.Export1.Item.DetCommon, "selectChar");

        field.Color = "cyan";
        field.Intensity = Intensity.Dark;
        field.Protected = true;

        // ------------------------------------------------------------
        // 08/03/00 W.Campbell - Added code to check
        // the lines where Case number contains non-
        // numeric and non-blank data to set the case
        // number to color white so that the office and
        // service provider will display in white in those
        // lines.  Work done on WR000182.
        // ------------------------------------------------------------
        if (!IsEmpty(export.Export1.Item.DetCase1.Number) && Verify
          (export.Export1.Item.DetCase1.Number, "0123456789") > 0)
        {
          var field1 = GetField(export.Export1.Item.DetCase1, "number");

          field1.Color = "white";
          field1.Protected = true;
        }
      }
    }

    export.Export1.CheckIndex();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    switch(TrimEnd(global.Command))
    {
      case "FVLTR":
        // ******************************************
        // 06/14/00 W.Campbell - Added new logic for
        // FV letter sent date -
        // Added new COMMAND fvltr for the sending of a
        // Family Violence removal notification letter.
        // This COMMAND is triggered by PFK20.
        // Previously PFK20 generated COMMAND
        // source which performed an entirely
        // different set of logic which was removed
        // (disabled) with this change.
        // ******************************************
        // ******************************************
        // 06/14/00 W.Campbell - Added new logic for
        // FV letter sent date -
        // Added new logic to initiate the sending of a
        // Family Violence removal notification letter
        // and added a new FV letter send date to the
        // screen.  The FV letter send date will be
        // set to current date as part of the logic of
        // initiating the letter which will be produced
        // in a nightly batch run which produces
        // documents for mailing.
        // ******************************************
        // ------------------------------------------------------
        // Must display before update.
        // ------------------------------------------------------
        if (!Equal(export.Search.Number, export.Previous.Number) || IsEmpty
          (export.Search.Number))
        {
          var field = GetField(export.Search, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";

          return;
        }

        // mjr
        // ------------------------------------------
        // 03/19/2002
        // PR138466 - Don't send FV letters on incoming interstate
        // cases
        // Give an error if the user attempts to send a document
        // on a case that has incoming interstate involvement and
        // is not a duplicate case
        // -------------------------------------------------------
        local.NoInterstateReqForCase.Flag = "N";
        local.InterstateCaseNoLetter.Flag = "N";

        foreach(var item in ReadCsePersonCase())
        {
          if (ReadInterstateRequest1())
          {
            // --------------------	If interstate case, don't produce fv letter
            // 			
            // ----------------------------------------------------------------------
            // 												
            local.InterstateCaseNoLetter.Flag = "Y";
          }
          else
          {
            local.NoInterstateReqForCase.Flag = "Y";

            break;
          }
        }

        // ******************************************
        // 06/14/00 W.Campbell - Added new logic
        // for FV letter sent date -
        // The user cannot request a FV letter be
        // sent until 1 day or more after the
        // last letter was sent.
        // ******************************************
        if (Equal(export.CsePerson.FvLetterSentDate, local.DateWorkArea.Date))
        {
          // **********************************
          // No FV Letter has been sent.
          // Therefore, it's OK to send one.
          // Just keep on going.
          // **********************************
        }
        else
        {
          // **********************************
          // A FV Letter has been sent.
          // Check to see if it was sent today.
          // If today then, cannot send again today..
          // **********************************
          if (Lt(export.CsePerson.FvLetterSentDate, local.Current.Date))
          {
            // **********************************
            // A FV Letter has been sent
            // earlier than today - this is OK.
            // Keep on going.
            // **********************************
          }
          else
          {
            // **********************************
            // A FV Letter has already been sent
            // today, cannot send another one today.
            // **********************************
            var field = GetField(export.Search, "number");

            field.Error = true;

            ExitState = "SI0000_FV_LETTER_TOO_SOON_ERROR";

            return;
          }
        }

        // ******************************************
        // 06/14/00 W.Campbell - Added new logic
        // for FV letter sent date -
        // The user must select an Open Case from
        // the list when requesting a FV letter be
        // sent.  This is so that the worker assigned
        // to the case can be determined and used
        // in the letter sending logic.
        // ******************************************
        if (local.Common.Count < 1)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.DetCommon.SelectChar) == 'S')
          {
            // ******************************************
            // 06/14/00 W.Campbell - Added new logic
            // for FV letter sent date -
            // Read the Selected Case.
            // ******************************************
            if (ReadCase())
            {
              // ******************************************
              // 06/14/00 W.Campbell - Added new logic
              // for FV letter sent date -
              // Check to make sure the case is still 'O'pen.
              // ******************************************
              if (AsChar(entities.Case1.Status) != 'O')
              {
                var field = GetField(export.Export1.Item.DetCase1, "number");

                field.Color = "red";
                field.Highlighting = Highlighting.ReverseVideo;
                field.Protected = true;

                ExitState = "SI0000_FV_LTR_CASE_NOT_OPEN";

                return;
              }
            }
            else
            {
              var field = GetField(export.Export1.Item.DetCase1, "number");

              field.Color = "red";
              field.Highlighting = Highlighting.ReverseVideo;
              field.Protected = true;

              ExitState = "CASE_NF";

              return;
            }

            break;
          }
        }

        export.Export1.CheckIndex();

        // ******************************************
        // 06/14/00 W.Campbell - Added new logic
        // for FV letter sent date -
        // Logic to update the FV Letter sent date
        // with the current date
        // ******************************************
        if (ReadCsePerson())
        {
          // ******************************************
          // 06/14/00 W.Campbell - Added new logic for
          // FV letter sent date -
          // Make sure that FVI is turned on (i.e. -
          // not spaces) before allowing FV Letter
          // to be sent.
          // ******************************************
          if (IsEmpty(entities.CsePerson.FamilyViolenceIndicator))
          {
            ExitState = "SI0000_FVI_NOT_SET_ERROR";

            var field1 = GetField(export.CsePerson, "familyViolenceIndicator");

            field1.Error = true;

            var field2 = GetField(export.Search, "number");

            field2.Error = true;

            return;
          }

          // ******************************************
          // 06/14/00 W.Campbell - Added new logic for
          // FV letter sent date -
          // Check FV Security here.
          // ******************************************
          UseScSecurityValidAuthForFv1();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          // ******************************************
          // 06/14/00 W.Campbell - Added new logic for
          // FV letter sent date -
          // Determine the Role that this person plays
          // on the Selected case.
          // ******************************************
          // PR156551. 07/16/2003  GVandy  Add check that case role end date is 
          // greater or equal to current date.
          if (ReadCaseCaseRoleCsePerson1())
          {
            local.CaseRole.Type1 = entities.CaseRole.Type1;
          }

          switch(TrimEnd(local.CaseRole.Type1))
          {
            case "AP":
              local.Document.Name = "APFVLTR";
              local.SpDocKey.KeyAp = entities.CsePerson.Number;

              break;
            case "AR":
              local.Document.Name = "ARFVLTR";
              local.SpDocKey.KeyAr = entities.CsePerson.Number;

              break;
            case "CH":
              local.Document.Name = "CHFVLTR";
              local.SpDocKey.KeyChild = entities.CsePerson.Number;

              break;
            default:
              var field = GetField(export.Export1.Item.DetCase2, "type1");

              field.Error = true;

              ExitState = "CASE_ROLE_NF";

              return;
          }

          // **********************************
          // 06/14/00 W.Campbell - Added new logic for
          // FV letter sent date -
          // If the case role = AR then we must also
          // check all the children on this case to see
          // if they have had a FV Letter sent today.
          // If one of them has then the user must
          // either cancel that letter or wait until
          // tomorrow.
          // **********************************
          if (Equal(local.Document.Name, "ARFVLTR"))
          {
            foreach(var item in ReadCaseRoleCsePerson4())
            {
              if (Equal(entities.Ch.FvLetterSentDate, local.Current.Date))
              {
                // **********************************
                // 06/14/00 W.Campbell - Added new logic for
                // FV letter sent date -
                // If the case role = AR then check for any
                // CH on the case who has already had a
                // FV Letter send today.  If there is one then
                // cannot send a new FV Letter until tomorrow
                // or either the current document is cancelled.
                // **********************************
                var field = GetField(export.Search, "number");

                field.Error = true;

                ExitState = "SI0000_FV_LETTER_TOO_SOON_ERROR";

                // -------------------------------
                // Must rollback if this occurs.
                // -------------------------------
                goto Read;
              }
              else
              {
                // **********************************
                // 06/14/00 W.Campbell - Added new logic for
                // FV letter sent date -
                // If the case role = AR then we must also
                // set the FV Letter send date to current date
                // for all the children on the case who have
                // FV turned on (i.e. FVI not = spaces.)
                // **********************************
                try
                {
                  UpdateCsePerson3();
                }
                catch(Exception e)
                {
                  switch(GetErrorCode(e))
                  {
                    case ErrorCode.AlreadyExists:
                      var field1 = GetField(export.Search, "number");

                      field1.Error = true;

                      ExitState = "CSE_PERSON_NU";

                      // ------------------------------------------------------
                      // Error encountered.
                      // Must rollback any updates which
                      // may have occurred.
                      // ------------------------------------------------------
                      goto Read;
                    case ErrorCode.PermittedValueViolation:
                      var field2 = GetField(export.Search, "number");

                      field2.Error = true;

                      ExitState = "CSE_PERSON_PV";

                      // ------------------------------------------------------
                      // Error encountered.
                      // Must rollback any updates which
                      // may have occurred.
                      // ------------------------------------------------------
                      goto Read;
                    case ErrorCode.DatabaseError:
                      break;
                    default:
                      throw;
                  }
                }
              }
            }
          }

          // **********************************
          // 06/14/00 W.Campbell - Added new logic for
          // FV letter sent date -
          // Must check to see if the AR on the case is
          // an ORG.  If it is, then don't send FV Letter.
          // No letter will be sent when AR is an ORG.
          // **********************************
          local.ProduceFvLtr.Flag = "Y";

          if (Equal(local.Document.Name, "CHFVLTR"))
          {
            if (ReadCaseRoleCsePerson2())
            {
              if (CharAt(entities.Ar.Number, 10) == 'O')
              {
                local.ProduceFvLtr.Flag = "N";
              }
            }
            else
            {
              ExitState = "CASE_ROLE_AR_NF";

              return;
            }
          }

          // **********************************
          // 09/27/07 PR252687 CLocke - Added  logic to
          // check to see if need to produce FV letter  -
          // If case is interstate - do not produce letter - If person has
          // multiple case and 1 is not interstate - produce letter - if 
          // duplicate case and interstate case  - do not produce letter
          // **********************************
          if (AsChar(local.NoInterstateReqForCase.Flag) == 'Y' && AsChar
            (local.InterstateCaseNoLetter.Flag) == 'Y')
          {
            local.ProduceFvLtr.Flag = "Y";
          }
          else if (AsChar(local.InterstateCaseNoLetter.Flag) == 'Y' && AsChar
            (local.NoInterstateReqForCase.Flag) == 'N')
          {
            local.ProduceFvLtr.Flag = "N";
          }
          else if (AsChar(local.InterstateCaseNoLetter.Flag) == 'N' && AsChar
            (local.NoInterstateReqForCase.Flag) == 'Y')
          {
            local.ProduceFvLtr.Flag = "Y";
          }
          else if (AsChar(local.InterstateCaseNoLetter.Flag) == 'N' && AsChar
            (local.NoInterstateReqForCase.Flag) == 'N')
          {
            local.ProduceFvLtr.Flag = "N";
          }

          if (AsChar(local.ProduceFvLtr.Flag) == 'N')
          {
            ExitState = "SI0000_FV_LETTER_NOT_SENT_CSENET";

            return;
          }

          // **********************************
          // 06/14/00 W.Campbell - Added new logic for
          // FV letter sent date -
          // Check to see if we want to Produce the
          // FV letter or not.
          // **********************************
          if (AsChar(local.ProduceFvLtr.Flag) == 'Y')
          {
            // **********************************
            // 06/14/00 W.Campbell - Added new logic for
            // FV letter sent date -
            // Insert FV letter producing logic here.
            // **********************************
            local.SpDocKey.KeyCase = entities.Case1.Number;

            // mjr
            // ----------------------------------------------
            // 03/06/2002
            // PR138466 - Don't send documents if there is
            // incoming interstate involvement
            // -----------------------------------------------------------
            UseSpCabDetermineInterstateDoc();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              // -------------------------------
              // Must rollback if this occurs.
              // -------------------------------
              goto Read;
            }

            // **********************************
            // FV letter produced all OK.
            // **********************************
          }

          // ******************************************
          // 06/14/00 W.Campbell - Added new logic for
          // FV letter sent date -
          // Update the FV letter send date to CURRENT DATE
          // and populate the export view with that same date.
          // ******************************************
          try
          {
            UpdateCsePerson2();
            export.CsePerson.FvLetterSentDate = local.Current.Date;
            export.HiddenPrev.Assign(export.CsePerson);
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                var field1 = GetField(export.Search, "number");

                field1.Error = true;

                ExitState = "CSE_PERSON_NU";

                // ------------------------------------------------------
                // Error encountered.
                // Must rollback any updates which
                // may have occurred.
                // ------------------------------------------------------
                break;
              case ErrorCode.PermittedValueViolation:
                var field2 = GetField(export.Search, "number");

                field2.Error = true;

                ExitState = "CSE_PERSON_PV";

                // ------------------------------------------------------
                // Error encountered.
                // Must rollback any updates which
                // may have occurred.
                // ------------------------------------------------------
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
          var field = GetField(export.Search, "number");

          field.Error = true;

          ExitState = "CSE_PERSON_NF";

          // ------------------------------------------------------
          // Error encountered.
          // Must rollback any updates which
          // may have occurred.
          // ------------------------------------------------------
          return;
        }

Read:

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.Export1.Update.DetCommon.SelectChar = "";
          ExitState = "ACO_NI0000_UPDATE_SUCCESSFUL";
        }
        else
        {
          // ------------------------------------------------------
          // Some kind of error encountered.
          // Must rollback any updates which
          // may have occurred.
          // ------------------------------------------------------
          UseEabRollbackCics();
        }

        return;
      case "RTLIST":
        // ------------------------------------------------------
        // 03/07/00 W. Campbell -  Added new logic
        // for return from list for selection of value
        // for the Family Violence Indicator value
        // lookup in the code table.
        // ------------------------------------------------------
        // ------------------------------------------------------
        // On return from list(code table etc),
        // populate the retreived data to field
        // on the screen.
        // ------------------------------------------------------
        if (AsChar(export.FvPrompt.SelectChar) == 'S')
        {
          if (!IsEmpty(import.Selected.Description))
          {
            export.CsePerson.FamilyViolenceIndicator = import.Selected.Cdvalue;
          }

          export.FvPrompt.SelectChar = "";
        }

        break;
      case "DISPLAY":
        // ------------------------------------------------------
        // 03/07/00 W. Campbell -  Added new logic
        // to add this Case DISPLAY - The display
        // logic is handled after this Case of
        // Command.  Just fall thru to that logic.
        // ------------------------------------------------------
        break;
      case "LIST":
        // ------------------------------------------------------
        // 03/07/00 W. Campbell -  Added new logic
        // to add this Case LIST - The Prompt
        // logic was added for Family Violence
        // Indicator value lookup in the code table.
        // ------------------------------------------------------
        if (AsChar(import.FvPrompt.SelectChar) == 'S')
        {
          ++local.Invalid.Count;
          export.Prompt.CodeName = "FAMILY VIOLENCE CODES";
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";
        }
        else
        {
          ++local.Invalid.Count;

          var field = GetField(export.FvPrompt, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
        }

        switch(local.Invalid.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

            break;
          case 1:
            break;
          default:
            if (AsChar(export.FvPrompt.SelectChar) == 'S')
            {
              var field = GetField(export.FvPrompt, "selectChar");

              field.Error = true;
            }

            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            break;
        }

        return;
      case "UPDATE":
        // ------------------------------------------------------
        // 03/07/00 W. Campbell -  Added new
        // CASE of update and new logic for
        // update of the Family Violence Indicator.
        // Work done on WR# 00162 for
        // PRWORA - Family Violence.
        // ------------------------------------------------------
        // ------------------------------------------------------
        // 03/07/00 W. Campbell -  Added code to
        // call new security cab to enforce the rules
        // about which workers can change the
        // FV indicator on a person.
        // ------------------------------------------------------
        local.CsePerson.Number = export.Search.Number;
        UseScSecurityValidAuthForFv2();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // ------------------------------------------------------
        // Must display before update.
        // ------------------------------------------------------
        if (!Equal(export.Search.Number, export.Previous.Number) || IsEmpty
          (export.Search.Number))
        {
          var field = GetField(export.Search, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";

          return;
        }

        // ------------------------------------------------------
        // Must validate that the Export Person number
        // is for a Client and not for an Organization.
        // ------------------------------------------------------
        if (CharAt(export.Search.Number, 10) == 'O')
        {
          var field = GetField(export.Search, "number");

          field.Error = true;

          ExitState = "SI0000_NO_FV_FOR_ORGANIZATION";

          return;
        }

        // ------------------------------------------------------
        // Must validate that the value which the user
        // is wanting to update the Family Value
        // Indicator with is in the code tables.
        // ------------------------------------------------------
        if (AsChar(export.CsePerson.FamilyViolenceIndicator) != AsChar
          (export.HiddenPrev.FamilyViolenceIndicator))
        {
          if (!IsEmpty(export.CsePerson.FamilyViolenceIndicator))
          {
            local.Code.CodeName = "FAMILY VIOLENCE CODES";
            local.CodeValue.Cdvalue =
              export.CsePerson.FamilyViolenceIndicator ?? Spaces(10);

            // ------------------------------------------------------
            // USE cab_validate_code_value
            // ------------------------------------------------------
            UseCabValidateCodeValue();

            if (AsChar(local.Common.Flag) == 'N')
            {
              var field = GetField(export.CsePerson, "familyViolenceIndicator");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_CODE";

              return;
            }
          }
        }
        else
        {
          // ------------------------------------------------------
          // No need to update, data did not change.
          // ------------------------------------------------------
          var field = GetField(export.Search, "number");

          field.Error = true;

          ExitState = "INVALID_UPDATE";

          return;
        }

        // ------------------------------------------------------
        // Check to see if the FV indicator is being
        // set to a non-blank value(turned on).
        // ------------------------------------------------------
        if (!IsEmpty(export.CsePerson.FamilyViolenceIndicator))
        {
          // ------------------------------------------------------
          // Must validate that if the Person who is
          // getting their FV indicator turned on is
          // an active CH on a case then the active
          // AR on that case must also have its
          // FV indicator turned on.
          // ------------------------------------------------------
          foreach(var item in ReadCsePersonCaseRoleCase2())
          {
            // ------------------------------------------------------
            // This person is an active CH on a case
            // Now, look to see if the active AR on
            // that case has its FV indicator turned on.
            // ------------------------------------------------------
            if (ReadCaseRoleCsePerson3())
            {
              // ------------------------------------------------------
              // The active AR on the case does not
              // have its FV indicator turned on.
              // This is not allowed.  Get out with an
              // error msg.
              // ------------------------------------------------------
              ExitState = "SI0000_ACT_CH_WITH_ACT_AR_WO_FV";

              var field = GetField(export.Search, "number");

              field.Error = true;

              return;
            }
            else
            {
              // ------------------------------------------------------
              // The active AR on the case has
              // its FV indicator turned on.
              // Therefore, keep on looking for
              // other cases.
              // ------------------------------------------------------
            }
          }
        }
        else
        {
          // ------------------------------------------------------
          // The FV indicator is being changed
          // to a blank value(turned off).
          // Therefore, must now see if it is being
          // turned off on an Active AR, and if so,
          // then don't allow it, if there is an
          // active CH on the same case with its
          // FV indicator turned on.
          // ------------------------------------------------------
          local.TurningOffFvi.Flag = "Y";

          foreach(var item in ReadCsePersonCaseRoleCase1())
          {
            // ------------------------------------------------------
            // This person is an active AR on a case
            // Now, look to see if the active CH on
            // that case has its FV indicator turned on.
            // ------------------------------------------------------
            if (ReadCaseRoleCsePerson1())
            {
              // ------------------------------------------------------
              // The active CH on the case has
              // its FV indicator turned on.
              // This is not allowed.  Get out with an
              // error msg.
              // ------------------------------------------------------
              ExitState = "SI0000_ACT_AR_WITH_ACT_CH_W_FV";

              var field = GetField(export.Search, "number");

              field.Error = true;

              return;
            }
            else
            {
              // ------------------------------------------------------
              // This active CH on the case has
              // its FV indicator turned off.
              // Therefore, keep on looking for
              // other active children with their
              // FV indicator turned on.
              // ------------------------------------------------------
            }
          }

          // pr 252687  1/24/2008   CLocke    -  Need to determine if person/
          // case is interstate only or if person/case is non interstate also in
          // order to know whether to edit the removal of FVI.
          local.InterstateCase.Flag = "N";
          local.NonInterstateCase.Flag = "N";

          foreach(var item in ReadCsePersonCase())
          {
            if (ReadInterstateRequest1())
            {
              local.InterstateCase.Flag = "Y";
            }
            else
            {
              local.NonInterstateCase.Flag = "Y";

              break;
            }
          }

          // mjr
          // ------------------------------------------
          // 03/19/2002
          // PR138466 - Don't send FV letters on incoming interstate
          // cases
          // We don't need to perform any validations on the
          // person's fv_letter_sent_date value
          // -------------------------------------------------------
          // 1/24/08  CLocke  CQ495    -    When person has an interstate case 
          // only, no FV letter will be sent so no validations needs to be done
          // on date sent.  But if person has either both an interstate case and
          // Kansas case or Kansas case(s) only, need to validate FV letter
          // date sent.
          if (AsChar(local.NonInterstateCase.Flag) == 'N' && AsChar
            (local.InterstateCase.Flag) == 'Y' || AsChar
            (local.NonInterstateCase.Flag) == 'N' && AsChar
            (local.InterstateCase.Flag) == 'N')
          {
            export.CsePerson.FvLetterSentDate = local.DateWorkArea.Date;

            goto Test2;
          }

          if (AsChar(local.NonInterstateCase.Flag) == 'Y' && AsChar
            (local.InterstateCase.Flag) == 'Y' || AsChar
            (local.InterstateCase.Flag) == 'N' && AsChar
            (local.NonInterstateCase.Flag) == 'Y')
          {
            // ******************************************
            // 06/14/00 W.Campbell -
            // Added new logic for FV letter sent date -
            // FVI cannot be turned off If the letter sent
            // date = 0001-01-01.  i.e. A letter must have
            // been sent and not been cancelled.
            // ******************************************
            if (Equal(export.CsePerson.FvLetterSentDate, local.DateWorkArea.Date))
              
            {
              ExitState = "SI0000_FV_LETTER_NOT_BEEN_SENT";

              var field = GetField(export.CsePerson, "familyViolenceIndicator");

              field.Error = true;

              return;
            }

            // ******************************************
            // 06/14/00 W.Campbell - Added new logic for
            // FV letter sent date -
            // FVI can only be turned off If current date
            // is > the FV letter sent date + 1 day AND if
            // current date is < FV letter sent date +30 days.
            // i.e. A letter must have been RECENTLY
            // sent and not been cancelled.
            // ******************************************
            if (Lt(local.Current.Date,
              AddDays(export.CsePerson.FvLetterSentDate, 1)))
            {
              ExitState = "SI0000_FV_LTR_DATE_GT_1_DAY_ERR";

              var field = GetField(export.CsePerson, "familyViolenceIndicator");

              field.Error = true;

              return;
            }

            if (Lt(AddDays(export.CsePerson.FvLetterSentDate, 30),
              local.Current.Date))
            {
              ExitState = "SI0000_FV_LTR_DATE_LT_30_DAY_ERR";

              var field = GetField(export.CsePerson, "familyViolenceIndicator");

              field.Error = true;

              return;
            }

            // 01/14/2008   C Locke    FVI cannot be removed on same day that 
            // letter was sent
            if (Equal(local.Current.Date, export.CsePerson.FvLetterSentDate))
            {
              ExitState = "SI0000_FV_LETTER_NOT_BEEN_SENT";

              var field = GetField(export.CsePerson, "familyViolenceIndicator");

              field.Error = true;

              return;
            }

            // ******************************************
            // 06/14/00 W.Campbell - Added new logic for
            // FV letter sent date -
            // OK to set FVI to spaces and set the
            // FV letter sent date = 0001-01-01.
            // ******************************************
            export.CsePerson.FvLetterSentDate = local.DateWorkArea.Date;
          }
        }

Test2:

        if (ReadCsePerson())
        {
          // ******************************************
          // 06/14/00 W.Campbell - Added new logic for
          // FV letter sent date -
          // Included FV letter sent date in the following
          // update statement.
          // ******************************************
          try
          {
            UpdateCsePerson1();
            export.HiddenPrev.Assign(export.CsePerson);
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                var field1 = GetField(export.Search, "number");

                field1.Error = true;

                ExitState = "CSE_PERSON_NU";

                return;
              case ErrorCode.PermittedValueViolation:
                var field2 = GetField(export.Search, "number");

                field2.Error = true;

                ExitState = "CSE_PERSON_PV";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
        else
        {
          var field = GetField(export.Search, "number");

          field.Error = true;

          ExitState = "CSE_PERSON_NF";

          return;
        }

        // -----------------------------------------------
        // Put the logic here to write an infrastructure
        // record.
        // -----------------------------------------------
        // *****************************************
        // Begin event insertion.
        // *****************************************
        // -----------------------------------------------
        // Initialize a local previous case number
        // to spaces.
        // -----------------------------------------------
        local.Previous.Number = "";

        // -----------------------------------------------
        // Read each case which this person is
        // involved in as a AP, AR or CH.  However,
        // we only want it once even if the person
        // has played more than one role at various
        // times.
        // -----------------------------------------------
        foreach(var item in ReadCaseCaseRoleCsePerson2())
        {
          if (Equal(entities.Case1.Number, local.Previous.Number))
          {
            continue;
          }

          local.Previous.Number = entities.Case1.Number;

          // mjr
          // ------------------------------------------
          // 03/19/2002
          // Interstate request should be open to be considered
          // interstate
          // Added "AND other_state_case_status IS EQUAL TO 'O'"
          // -------------------------------------------------------
          if (ReadInterstateRequest2())
          {
            if (AsChar(entities.InterstateRequest.KsCaseInd) == 'Y')
            {
              local.Infrastructure.InitiatingStateCode = "KS";
            }
            else
            {
              local.Infrastructure.InitiatingStateCode = "OS";
            }
          }
          else
          {
            local.Infrastructure.InitiatingStateCode = "KS";
          }

          local.Infrastructure.ProcessStatus = "Q";
          local.Infrastructure.EventId = 11;
          local.Infrastructure.BusinessObjectCd = "CAS";
          local.Infrastructure.CaseNumber = entities.Case1.Number;
          local.Infrastructure.UserId = global.UserId;
          local.Infrastructure.ReferenceDate = local.Current.Date;
          local.Infrastructure.CaseUnitNumber = 0;
          local.Infrastructure.CsePersonNumber = entities.CsePerson.Number;

          if (!IsEmpty(entities.CsePerson.FamilyViolenceIndicator))
          {
            local.Infrastructure.ReasonCode = "FAMVIOLENCESET";
            local.Infrastructure.Detail = "FV turned on with value(";
            local.Infrastructure.Detail =
              TrimEnd(local.Infrastructure.Detail) + (
                export.CsePerson.FamilyViolenceIndicator ?? "") + ") for Person #:";
              
          }
          else
          {
            local.Infrastructure.ReasonCode = "FAMVIOLENCERMVD";
            local.Infrastructure.Detail = "FV turned off for Person #:";
          }

          local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + " " +
            export.Search.Number;
          local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + " on Case #: " +
            entities.Case1.Number;

          // -----------------------------------------------
          // Write out the infrastructure record.
          // -----------------------------------------------
          UseSpCabCreateInfrastructure();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else
          {
            UseEabRollbackCics();
            export.NotFromReferral.Flag = "";

            var field1 = GetField(export.CsePerson, "familyViolenceIndicator");

            field1.Error = true;

            var field2 = GetField(export.Search, "number");

            field2.Error = true;

            break;
          }
        }

        // *****************************************
        // End event insertion.
        // *****************************************
        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // -----------------------------------------------
          // Must also update the FV indicators in the export
          // Repeating Group view so that the displayed
          // information will reflect the new FV value.
          // -----------------------------------------------
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (!export.Export1.CheckSize())
            {
              break;
            }

            if (Equal(export.Export1.Item.DetCsePersonsWorkSet.Number,
              export.Search.Number))
            {
              MoveCsePerson(export.CsePerson, export.Export1.Update.DetCsePerson);
                
            }
          }

          export.Export1.CheckIndex();

          // -----------------------------------------------
          // Set the export hidden previous view to current values.
          // -----------------------------------------------
          export.HiddenPrev.Assign(entities.CsePerson);

          if (AsChar(local.TurningOffFvi.Flag) == 'Y')
          {
            local.TurningOffFvi.Flag = "";
            ExitState = "AC0_NI0000_UPDATE_SUCCES_FVI_OFF";
          }
          else
          {
            ExitState = "ACO_NI0000_UPDATE_SUCCESSFUL";
          }
        }

        return;
      case "PEPR":
        // 12/13/99 M.L Start
        if (local.Common.Count < 1)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          break;
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.DetCommon.SelectChar) == 'S')
          {
            // ---------------------------------------------
            // Move selected view to a single view that will
            // be mapped back to calling screen view.
            // ---------------------------------------------
            export.Next.Number = export.Export1.Item.DetCase1.Number;
            ExitState = "ECO_LNK_TO_PEPR";

            goto Test3;
          }
        }

        export.Export1.CheckIndex();

        // 12/13/99 M.L End
        break;
      case "QARM":
        // 05/25/99 M. Lachowicz      Add new function key to transfer
        //                            to QARM screen.
        // -------------------------------------------------
        if (local.Common.Count == 1)
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (!export.Export1.CheckSize())
            {
              break;
            }

            if (AsChar(export.Export1.Item.DetCommon.SelectChar) == 'S')
            {
              // ---------------------------------------------
              // Move selected view to a single view that will
              // be mapped back to calling screen view.
              // ---------------------------------------------
              export.HiddenSelectedCase.Number =
                export.Export1.Item.DetCase1.Number;
              export.HiddenSelectedCsePersonsWorkSet.Number =
                export.Export1.Item.DetCsePersonsWorkSet.Number;
              ExitState = "ECO_LNK_TO_QARM";

              return;
            }
          }

          export.Export1.CheckIndex();
        }

        ExitState = "ECO_LNK_TO_QARM";

        return;
      case "PAR1":
        if (local.Common.Count < 1)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          break;
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.DetCommon.SelectChar) == 'S')
          {
            if (AsChar(export.Export1.Item.DetCase1.Status) != 'O')
            {
              ExitState = "SI0000_CASE_HAS_TO_BE_OPEN";

              var field = GetField(export.Export1.Item.DetCase1, "number");

              field.Color = "red";
              field.Highlighting = Highlighting.ReverseVideo;
              field.Protected = true;
            }
            else if (!IsEmpty(export.PaReferral.Number))
            {
              ExitState = "ECO_XFR_TO_PA_REFERRAL_PG1";
              export.Par1FromComn.Flag = "Y";
              export.HiddenSelectedCase.Number =
                export.Export1.Item.DetCase1.Number;

              return;
            }
            else
            {
              ExitState = "SI0000_DID_NOT_COME_FROM_PAR1";

              var field = GetField(export.Export1.Item.DetCase1, "number");

              field.Color = "red";
              field.Highlighting = Highlighting.ReverseVideo;
              field.Protected = true;
            }
          }
        }

        export.Export1.CheckIndex();

        break;
      case "NEXT":
        if (export.HiddenStandard.PageNumber == Import.PageKeysGroup.Capacity)
        {
          ExitState = "MAX_PAGES_REACHED_SYSTEM_ERROR";

          break;
        }

        // ------------------------------------------------------------
        // Ensure that there is another page of details to retrieve.
        // ------------------------------------------------------------
        export.PageKeys.Index = export.HiddenStandard.PageNumber;
        export.PageKeys.CheckSize();

        if (IsEmpty(export.PageKeys.Item.PageKeyCase.Number))
        {
          ExitState = "ACO_NE0000_INVALID_FORWARD";

          break;
        }

        ++export.HiddenStandard.PageNumber;

        break;
      case "PREV":
        if (export.HiddenStandard.PageNumber == 1)
        {
          ExitState = "ACO_NE0000_INVALID_BACKWARD";

          break;
        }

        --export.HiddenStandard.PageNumber;

        break;
      case "NAME":
        ExitState = "ECO_XFR_TO_NAME_LIST";

        return;
      case "ICAS":
        // ---------------------------------------------
        // 11/04/98 W. Campbell - Added code to
        // properly handle the COMMAND of ICAS
        // and cause the flow to ICAS.
        // ---------------------------------------------
        if (local.Common.Count < 1)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          break;
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.DetCommon.SelectChar) == 'S')
          {
            if (AsChar(export.Export1.Item.DetCase1.Status) != 'O')
            {
              ExitState = "SI0000_CASE_HAS_TO_BE_OPEN";

              var field = GetField(export.Export1.Item.DetCase1, "number");

              field.Color = "red";
              field.Highlighting = Highlighting.ReverseVideo;
              field.Protected = true;
            }
            else if (export.InterstateCase.TransSerialNumber > 0)
            {
              ExitState = "ECO_XFR_TO_ICAS";
              export.HiddenSelectedCase.Number =
                export.Export1.Item.DetCase1.Number;

              return;
            }
            else
            {
              ExitState = "SI0000_DID_NOT_COME_FROM_ICAS";

              var field = GetField(export.Export1.Item.DetCase1, "number");

              field.Color = "red";
              field.Highlighting = Highlighting.ReverseVideo;
              field.Protected = true;
            }
          }
        }

        export.Export1.CheckIndex();

        // ---------------------------------------------
        // 11/04/98 W. Campbell -  End of code added
        // to properly handle the COMMAND of ICAS
        // and cause the flow to ICAS.
        // ---------------------------------------------
        break;
      case "SOURCE":
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
      case "REOPEN":
        if (local.Common.Count < 1)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          break;
        }

        if (export.ReopenOffice.SystemGeneratedId <= 0)
        {
          export.ReopenWorkArea.Text40 =
            "Enter Case Reopen Office # and press F18";

          var field = GetField(export.ReopenOffice, "systemGeneratedId");

          field.Color = "green";
          field.Highlighting = Highlighting.Underscore;
          field.Protected = false;
          field.Focused = true;

          export.ReopenOffice.Name = "";

          // -------------------------------------------------
          // 11/22/99 W. Campbell - Inserted following
          // FOR statement to protect group export
          // items so that cursor will be placed in the
          // reopen office message ready for user input.
          // Work done on PR#79699 along with changes
          // to cab SI_REOPEN_CASE_N_CASE_UNITS.
          // -------------------------------------------------
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (!export.Export1.CheckSize())
            {
              break;
            }

            // ------------------------------------------------------------
            // 08/03/00 W.Campbell - Added verify function
            // for numeric test to the following IF stmt so that
            // the lines where Case number contains non-
            // numeric and non-blank data will be treated
            // the same as lines with blank case numbers.
            // Work done on WR000182.
            // ------------------------------------------------------------
            if (!IsEmpty(export.Export1.Item.DetCase1.Number) && Verify
              (export.Export1.Item.DetCase1.Number, "0123456789") == 0)
            {
              var field1 =
                GetField(export.Export1.Item.DetCommon, "selectChar");

              field1.Color = "cyan";
              field1.Highlighting = Highlighting.Underscore;
              field1.Protected = true;
              field1.Focused = false;

              var field2 = GetField(export.Export1.Item.DetCase1, "number");

              field2.Color = "cyan";
              field2.Protected = true;
            }
            else
            {
              // ------------------------------------------------------------
              // 03/20/00 W.Campbell - Inserted code to fix a
              // bug in the display logic whereby the Select
              // character was being left unprotected.
              // ------------------------------------------------------------
              var field1 =
                GetField(export.Export1.Item.DetCommon, "selectChar");

              field1.Color = "green";
              field1.Intensity = Intensity.Dark;
              field1.Protected = true;

              // ------------------------------------------------------------
              // 08/03/00 W.Campbell - Added code to check
              // the lines where Case number contains non-
              // numeric and non-blank data to set the case
              // number to color white so that the office and
              // service provider will display in white in those
              // lines.  Work done on WR000182.
              // ------------------------------------------------------------
              if (!IsEmpty(export.Export1.Item.DetCase1.Number) && Verify
                (export.Export1.Item.DetCase1.Number, "0123456789") > 0)
              {
                var field2 = GetField(export.Export1.Item.DetCase1, "number");

                field2.Color = "white";
                field2.Protected = true;
              }
            }
          }

          export.Export1.CheckIndex();

          return;
        }
        else
        {
          // -----------------------------------------------
          // Check to if the Office Number entered is valid,
          // And if the User is an OSP in that ofice.
          // -----------------------------------------------
          // 07/01/99 M.L         Change property of READ to generate
          //                      Select Only
          // ------------------------------------------------------------
          if (ReadOffice())
          {
            MoveOffice(entities.ReopenOffice, export.ReopenOffice);

            if (ReadOfficeServiceProvider())
            {
              // ---------------------------------------------
              // Valid information entered, continue..
              // ---------------------------------------------
              MoveOffice(export.ReopenOffice, local.Reopen);
            }
            else
            {
              export.ReopenWorkArea.Text40 =
                "Enter Case Reopen Office # and press F18";

              var field = GetField(export.ReopenOffice, "systemGeneratedId");

              field.Error = true;

              ExitState = "CASE_REOPEN_OFFICE_ERR_FOR_OSP";

              break;
            }
          }
          else
          {
            export.ReopenWorkArea.Text40 =
              "Enter Case Reopen Office # and press F18";

            var field = GetField(export.ReopenOffice, "systemGeneratedId");

            field.Error = true;

            ExitState = "OFFICE_NF";

            break;
          }
        }

        if (local.Common.Count == 1)
        {
          export.Export1.Index = 0;

          for(var limit = export.Export1.Count; export.Export1.Index < limit; ++
            export.Export1.Index)
          {
            if (!export.Export1.CheckSize())
            {
              break;
            }

            if (AsChar(export.Export1.Item.DetCommon.SelectChar) == 'S')
            {
              // ---------------------------------------------
              // Check if Case is being opened by a Referral.
              // ---------------------------------------------
              if (IsEmpty(export.NotFromReferral.Flag))
              {
                if (export.InformationRequest.Number <= 0 && IsEmpty
                  (export.PaReferral.Number) && export
                  .InterstateCase.TransSerialNumber <= 0)
                {
                  ExitState = "SI0000_CASE_REOPED_FROM_OTHER";
                  export.NotFromReferral.Flag = "Y";

                  goto Test3;
                }
              }

              // ---------------------------------------------
              // Move selected view to a single view that will
              // be mapped back to calling screen view.
              // ---------------------------------------------
              export.HiddenSelectedCase.Number =
                export.Export1.Item.DetCase1.Number;
              UseSiComnReopenCaseNCaseUnit();

              if (AsChar(export.GoToInrdReopen.Text1) == 'Y')
              {
                UseEabRollbackCics();
                ExitState = "COMPLETE_INRD_FOR_REOPENING_CASE";
                export.ReopenWorkArea.Text40 = "";
                export.ReopenOffice.Name = "";
                export.ReopenOffice.SystemGeneratedId = 0;

                return;
              }

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                export.Export1.Update.DetCommon.SelectChar = "";
                local.CaseReopened.Flag = "Y";
                global.Command = "DISPLAY";
              }
              else
              {
                UseEabRollbackCics();
                export.NotFromReferral.Flag = "";

                var field = GetField(export.Export1.Item.DetCase1, "number");

                field.Error = true;

                goto Test3;
              }
            }
          }

          export.Export1.CheckIndex();
        }

        break;
      case "ROLE":
        if (local.Common.Count < 1)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          break;
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.DetCommon.SelectChar) == 'S')
          {
            // ---------------------------------------------
            // Move selected view to a single view that will
            // be mapped back to calling screen view.
            // ---------------------------------------------
            export.Next.Number = export.Export1.Item.DetCase1.Number;
            ExitState = "ECO_LNK_TO_ROLE";

            goto Test3;
          }
        }

        export.Export1.CheckIndex();

        break;
      case "RETURN":
        if (local.Common.Count == 1)
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (!export.Export1.CheckSize())
            {
              break;
            }

            if (AsChar(export.Export1.Item.DetCommon.SelectChar) == 'S')
            {
              // ---------------------------------------------
              // Move selected view to a single view that will
              // be mapped back to calling screen view.
              // ---------------------------------------------
              export.HiddenSelectedCase.Number =
                export.Export1.Item.DetCase1.Number;
              export.HiddenSelectedCsePersonsWorkSet.Number =
                export.Export1.Item.DetCsePersonsWorkSet.Number;
              ExitState = "ACO_NE0000_RETURN";

              return;
            }
          }

          export.Export1.CheckIndex();
        }

        ExitState = "ACO_NE0000_RETURN";

        return;
      case "INVALID":
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "INRD":
        if (AsChar(import.GoToInrdReopen.Text1) == 'Y')
        {
          if (local.Common.Count == 1)
          {
            for(export.Export1.Index = 0; export.Export1.Index < export
              .Export1.Count; ++export.Export1.Index)
            {
              if (!export.Export1.CheckSize())
              {
                break;
              }

              if (AsChar(export.Export1.Item.DetCommon.SelectChar) == 'S')
              {
                // ---------------------------------------------
                // Move selected view to a single view that will
                // be mapped back to calling screen view.
                // ---------------------------------------------
                export.HiddenSelectedCase.Number =
                  export.Export1.Item.DetCase1.Number;
                export.HiddenSelectedCsePersonsWorkSet.Number =
                  export.Search.Number;
                ExitState = "ECO_LNK_TO_INRD";

                return;
              }
            }

            export.Export1.CheckIndex();
          }
        }
        else
        {
          ExitState = "ACO_NE0000_INVALID_COMMAND";
        }

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

Test3:

    // ---------------------------------------------
    // If a display is required, call the action
    // block that reads the next group of data based
    // on the page number.
    // ---------------------------------------------
    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "NEXT") || Equal
        (global.Command, "PREV"))
      {
        // ------------------------------------------------------------
        // Initialise tables if the person number has been changed
        // -----------------------------------------------------------
        if (IsEmpty(export.Search.Number))
        {
          var field = GetField(export.Search, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          goto Test4;
        }

        if (!Equal(export.Previous.Number, export.Search.Number) && !
          IsEmpty(import.Previous.Number))
        {
          export.HiddenStandard.PageNumber = 1;
          export.PageKeys.Count = 0;
        }

        export.ReopenWorkArea.Text40 = "";
        export.ReopenOffice.Name = "";
        export.ReopenOffice.SystemGeneratedId = 0;
        local.TextWorkArea.Text10 = export.Search.Number;
        UseEabPadLeftWithZeros();
        export.Search.Number = local.TextWorkArea.Text10;

        export.PageKeys.Index = export.HiddenStandard.PageNumber - 1;
        export.PageKeys.CheckSize();

        local.PageKeyCsePersonsWorkSet.Number =
          export.PageKeys.Item.PageKeyCsePerson.Number;
        UseSiReadCaseCompsByName();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          goto Test4;
        }

        ++export.PageKeys.Index;
        export.PageKeys.CheckSize();

        export.PageKeys.Update.PageKeyCase.Number = local.PageKeyCase.Number;
        export.PageKeys.Update.PageKeyStatus.Text1 = local.PageKeyStatus.Text1;
        export.PageKeys.Update.PageKeyCaseRole.Type1 =
          local.PageKeyCaseRole.Type1;
        export.PageKeys.Update.PageKeyCsePerson.Number =
          local.PageKeyCsePersonsWorkSet.Number;
        export.Previous.Number = export.Search.Number;
        export.HiddenPrev.Assign(export.CsePerson);

        if (export.Export1.IsEmpty)
        {
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
        }
        else
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";

          if (AsChar(local.CaseReopened.Flag) == 'Y')
          {
            ExitState = "CASE_SUCCESSFULLY_REOPENED";
          }
        }
      }
    }

Test4:

    for(export.Export1.Index = 0; export.Export1.Index < export.Export1.Count; ++
      export.Export1.Index)
    {
      if (!export.Export1.CheckSize())
      {
        break;
      }

      // ------------------------------------------------------------
      // 08/03/00 W.Campbell - Added verify function
      // for numeric test to the following IF stmt so that
      // the lines where Case number contains non-
      // numeric and non-blank data will be treated
      // the same as lines with blank case numbers.
      // Work done on WR000182.
      // ------------------------------------------------------------
      if (!IsEmpty(export.Export1.Item.DetCase1.Number) && Verify
        (export.Export1.Item.DetCase1.Number, "0123456789") == 0)
      {
        var field1 = GetField(export.Export1.Item.DetCommon, "selectChar");

        field1.Color = "green";
        field1.Highlighting = Highlighting.Underscore;
        field1.Protected = false;
        field1.Focused = true;

        var field2 = GetField(export.Export1.Item.DetCase1, "number");

        field2.Color = "cyan";
        field2.Protected = true;
      }
      else
      {
        // ------------------------------------------------------------
        // 03/20/00 W.Campbell - Inserted code to fix a
        // bug in the display logic whereby the Select
        // character was being left unprotected.
        // ------------------------------------------------------------
        var field = GetField(export.Export1.Item.DetCommon, "selectChar");

        field.Color = "green";
        field.Intensity = Intensity.Dark;
        field.Protected = true;

        // ------------------------------------------------------------
        // 08/03/00 W.Campbell - Added code to check
        // the lines where Case number contains non-
        // numeric and non-blank data to set the case
        // number to color white so that the office and
        // service provider will display in white in those
        // lines.  Work done on WR000182.
        // ------------------------------------------------------------
        if (!IsEmpty(export.Export1.Item.DetCase1.Number) && Verify
          (export.Export1.Item.DetCase1.Number, "0123456789") > 0)
        {
          var field1 = GetField(export.Export1.Item.DetCase1, "number");

          field1.Color = "white";
          field1.Protected = true;
        }
      }
    }

    export.Export1.CheckIndex();
  }

  private static void MoveCase1(Case1 source, Case1 target)
  {
    target.Number = source.Number;
    target.Status = source.Status;
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.FamilyViolenceIndicator = source.FamilyViolenceIndicator;
  }

  private static void MoveDocument(Document source, Document target)
  {
    target.Name = source.Name;
    target.Type1 = source.Type1;
  }

  private static void MoveExport1(SiReadCaseCompsByName.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    target.DetInter.Text1 = source.DetInter.Text1;
    target.DetCommon.SelectChar = source.DetCommon.SelectChar;
    MoveCase1(source.DetCase1, target.DetCase1);
    target.DetCsePersonsWorkSet.Assign(source.DetCsePersonsWorkSet);
    MoveCsePerson(source.DetCsePerson, target.DetCsePerson);
    target.DetCase2.Type1 = source.DetCase2.Type1;
    target.DetFamily.Type1 = source.DetFamily.Type1;
    target.DetStatus.Text1 = source.DetRoleStatus.Text1;
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

  private static void MoveOffice(Office source, Office target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.Name = source.Name;
  }

  private static void MovePaReferral(PaReferral source, PaReferral target)
  {
    target.Number = source.Number;
    target.Type1 = source.Type1;
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

  private static void MoveStandard(Standard source, Standard target)
  {
    target.NextTransaction = source.NextTransaction;
    target.ScrollingMessage = source.ScrollingMessage;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.Code.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.Common.Flag = useExport.ValidCode.Flag;
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

    useImport.CsePersonsWorkSet.Number = export.Search.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseScSecurityValidAuthForFv1()
  {
    var useImport = new ScSecurityValidAuthForFv.Import();
    var useExport = new ScSecurityValidAuthForFv.Export();

    useImport.Case1.Number = entities.Case1.Number;

    Call(ScSecurityValidAuthForFv.Execute, useImport, useExport);
  }

  private void UseScSecurityValidAuthForFv2()
  {
    var useImport = new ScSecurityValidAuthForFv.Import();
    var useExport = new ScSecurityValidAuthForFv.Export();

    useImport.CsePerson.Number = local.CsePerson.Number;

    Call(ScSecurityValidAuthForFv.Execute, useImport, useExport);
  }

  private void UseSiComnReopenCaseNCaseUnit()
  {
    var useImport = new SiComnReopenCaseNCaseUnit.Import();
    var useExport = new SiComnReopenCaseNCaseUnit.Export();

    useImport.InformationRequest.Number = export.InformationRequest.Number;
    useImport.Reopen.SystemGeneratedId = export.ReopenOffice.SystemGeneratedId;
    useImport.Case1.Number = export.HiddenSelectedCase.Number;
    MoveInterstateCase(export.InterstateCase, useImport.InterstateCase);
    MovePaReferral(export.PaReferral, useImport.PaReferral);

    Call(SiComnReopenCaseNCaseUnit.Execute, useImport, useExport);

    export.Ar.Number = useExport.Ar.Number;
    export.GoToInrdReopen.Text1 = useExport.GoToInrdReopen.Text1;
  }

  private void UseSiReadCaseCompsByName()
  {
    var useImport = new SiReadCaseCompsByName.Import();
    var useExport = new SiReadCaseCompsByName.Export();

    useImport.PageKeyCsePersonsWorkSet.Number =
      local.PageKeyCsePersonsWorkSet.Number;
    useImport.CsePersonsWorkSet.Number = export.Search.Number;
    useImport.Standard.PageNumber = export.HiddenStandard.PageNumber;
    useImport.PageKeyCase.Number = export.PageKeys.Item.PageKeyCase.Number;
    useImport.PageKeyStatus.Text1 = export.PageKeys.Item.PageKeyStatus.Text1;
    useImport.PageKeyCaseRole.Type1 =
      export.PageKeys.Item.PageKeyCaseRole.Type1;

    Call(SiReadCaseCompsByName.Execute, useImport, useExport);

    local.PageKeyCaseRole.Type1 = useExport.PageKeyCaseRole.Type1;
    local.PageKeyCsePersonsWorkSet.Number =
      useExport.PageKeyCsePersonsWorkSet.Number;
    local.PageKeyCase.Number = useExport.PageKeyCase.Number;
    local.PageKeyStatus.Text1 = useExport.PageKeyStatus.Text1;
    export.CsePerson.Assign(useExport.CsePerson);
    export.Search.Assign(useExport.CsePersonsWorkSet);
    useExport.Export1.CopyTo(export.Export1, MoveExport1);
    export.Standard.ScrollingMessage = useExport.Standard.ScrollingMessage;
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    local.Infrastructure.Assign(useExport.Infrastructure);
  }

  private void UseSpCabDetermineInterstateDoc()
  {
    var useImport = new SpCabDetermineInterstateDoc.Import();
    var useExport = new SpCabDetermineInterstateDoc.Export();

    MoveDocument(local.Document, useImport.Document);
    MoveSpDocKey(local.SpDocKey, useImport.SpDocKey);

    Call(SpCabDetermineInterstateDoc.Execute, useImport, useExport);
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", export.Export1.Item.DetCase1.Number);
      },
      (db, reader) =>
      {
        entities.Case1.ClosureReason = db.GetNullableString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 1);
        entities.Case1.Status = db.GetNullableString(reader, 2);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 3);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 4);
        entities.Case1.OfficeIdentifier = db.GetNullableInt32(reader, 5);
        entities.Case1.LastUpdatedTimestamp = db.GetNullableDateTime(reader, 6);
        entities.Case1.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.Case1.ClosureLetterDate = db.GetNullableDate(reader, 8);
        entities.Case1.DuplicateCaseIndicator = db.GetNullableString(reader, 9);
        entities.Case1.Note = db.GetNullableString(reader, 10);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseCaseRoleCsePerson1()
  {
    entities.CaseRole.Populated = false;
    entities.CsePerson.Populated = false;
    entities.Case1.Populated = false;

    return Read("ReadCaseCaseRoleCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb1", export.Export1.Item.DetCase1.Number);
        db.SetNullableDate(
          command, "endDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "numb2", export.Search.Number);
      },
      (db, reader) =>
      {
        entities.Case1.ClosureReason = db.GetNullableString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 1);
        entities.CaseRole.CasNumber = db.GetString(reader, 1);
        entities.Case1.Status = db.GetNullableString(reader, 2);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 3);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 4);
        entities.Case1.OfficeIdentifier = db.GetNullableInt32(reader, 5);
        entities.Case1.LastUpdatedTimestamp = db.GetNullableDateTime(reader, 6);
        entities.Case1.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.Case1.ClosureLetterDate = db.GetNullableDate(reader, 8);
        entities.Case1.DuplicateCaseIndicator = db.GetNullableString(reader, 9);
        entities.Case1.Note = db.GetNullableString(reader, 10);
        entities.CaseRole.CspNumber = db.GetString(reader, 11);
        entities.CsePerson.Number = db.GetString(reader, 11);
        entities.CaseRole.Type1 = db.GetString(reader, 12);
        entities.CaseRole.Identifier = db.GetInt32(reader, 13);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 14);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 15);
        entities.CsePerson.Type1 = db.GetString(reader, 16);
        entities.CsePerson.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 17);
        entities.CsePerson.LastUpdatedBy = db.GetNullableString(reader, 18);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 19);
        entities.CsePerson.FvLetterSentDate = db.GetNullableDate(reader, 20);
        entities.CsePerson.FviSetDate = db.GetNullableDate(reader, 21);
        entities.CsePerson.FviUpdatedBy = db.GetNullableString(reader, 22);
        entities.CaseRole.Populated = true;
        entities.CsePerson.Populated = true;
        entities.Case1.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private IEnumerable<bool> ReadCaseCaseRoleCsePerson2()
  {
    entities.CaseRole.Populated = false;
    entities.CsePerson.Populated = false;
    entities.Case1.Populated = false;

    return ReadEach("ReadCaseCaseRoleCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", export.Search.Number);
      },
      (db, reader) =>
      {
        entities.Case1.ClosureReason = db.GetNullableString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 1);
        entities.CaseRole.CasNumber = db.GetString(reader, 1);
        entities.Case1.Status = db.GetNullableString(reader, 2);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 3);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 4);
        entities.Case1.OfficeIdentifier = db.GetNullableInt32(reader, 5);
        entities.Case1.LastUpdatedTimestamp = db.GetNullableDateTime(reader, 6);
        entities.Case1.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.Case1.ClosureLetterDate = db.GetNullableDate(reader, 8);
        entities.Case1.DuplicateCaseIndicator = db.GetNullableString(reader, 9);
        entities.Case1.Note = db.GetNullableString(reader, 10);
        entities.CaseRole.CspNumber = db.GetString(reader, 11);
        entities.CsePerson.Number = db.GetString(reader, 11);
        entities.CaseRole.Type1 = db.GetString(reader, 12);
        entities.CaseRole.Identifier = db.GetInt32(reader, 13);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 14);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 15);
        entities.CsePerson.Type1 = db.GetString(reader, 16);
        entities.CsePerson.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 17);
        entities.CsePerson.LastUpdatedBy = db.GetNullableString(reader, 18);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 19);
        entities.CsePerson.FvLetterSentDate = db.GetNullableDate(reader, 20);
        entities.CsePerson.FviSetDate = db.GetNullableDate(reader, 21);
        entities.CsePerson.FviUpdatedBy = db.GetNullableString(reader, 22);
        entities.CaseRole.Populated = true;
        entities.CsePerson.Populated = true;
        entities.Case1.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private bool ReadCaseRoleCsePerson1()
  {
    entities.CaseRole.Populated = false;
    entities.CsePerson.Populated = false;

    return Read("ReadCaseRoleCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CsePerson.Type1 = db.GetString(reader, 6);
        entities.CsePerson.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 7);
        entities.CsePerson.LastUpdatedBy = db.GetNullableString(reader, 8);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 9);
        entities.CsePerson.FvLetterSentDate = db.GetNullableDate(reader, 10);
        entities.CsePerson.FviSetDate = db.GetNullableDate(reader, 11);
        entities.CsePerson.FviUpdatedBy = db.GetNullableString(reader, 12);
        entities.CaseRole.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCaseRoleCsePerson2()
  {
    entities.Ar.Populated = false;
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRoleCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.Ar.Number = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.Ar.Type1 = db.GetString(reader, 6);
        entities.Ar.LastUpdatedTimestamp = db.GetNullableDateTime(reader, 7);
        entities.Ar.LastUpdatedBy = db.GetNullableString(reader, 8);
        entities.Ar.FamilyViolenceIndicator = db.GetNullableString(reader, 9);
        entities.Ar.FvLetterSentDate = db.GetNullableDate(reader, 10);
        entities.Ar.Populated = true;
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.Ar.Type1);
      });
  }

  private bool ReadCaseRoleCsePerson3()
  {
    entities.CaseRole.Populated = false;
    entities.CsePerson.Populated = false;

    return Read("ReadCaseRoleCsePerson3",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CsePerson.Type1 = db.GetString(reader, 6);
        entities.CsePerson.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 7);
        entities.CsePerson.LastUpdatedBy = db.GetNullableString(reader, 8);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 9);
        entities.CsePerson.FvLetterSentDate = db.GetNullableDate(reader, 10);
        entities.CsePerson.FviSetDate = db.GetNullableDate(reader, 11);
        entities.CsePerson.FviUpdatedBy = db.GetNullableString(reader, 12);
        entities.CaseRole.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private IEnumerable<bool> ReadCaseRoleCsePerson4()
  {
    entities.Ch.Populated = false;
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCaseRoleCsePerson4",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.Ch.Number = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.Ch.Type1 = db.GetString(reader, 6);
        entities.Ch.LastUpdatedTimestamp = db.GetNullableDateTime(reader, 7);
        entities.Ch.LastUpdatedBy = db.GetNullableString(reader, 8);
        entities.Ch.FamilyViolenceIndicator = db.GetNullableString(reader, 9);
        entities.Ch.FvLetterSentDate = db.GetNullableDate(reader, 10);
        entities.Ch.Populated = true;
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.Ch.Type1);

        return true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", export.Search.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 2);
        entities.CsePerson.LastUpdatedBy = db.GetNullableString(reader, 3);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 4);
        entities.CsePerson.FvLetterSentDate = db.GetNullableDate(reader, 5);
        entities.CsePerson.FviSetDate = db.GetNullableDate(reader, 6);
        entities.CsePerson.FviUpdatedBy = db.GetNullableString(reader, 7);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private IEnumerable<bool> ReadCsePersonCase()
  {
    entities.CsePerson.Populated = false;
    entities.Case1.Populated = false;

    return ReadEach("ReadCsePersonCase",
      (db, command) =>
      {
        db.SetString(command, "numb", export.Search.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 2);
        entities.CsePerson.LastUpdatedBy = db.GetNullableString(reader, 3);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 4);
        entities.CsePerson.FvLetterSentDate = db.GetNullableDate(reader, 5);
        entities.CsePerson.FviSetDate = db.GetNullableDate(reader, 6);
        entities.CsePerson.FviUpdatedBy = db.GetNullableString(reader, 7);
        entities.Case1.ClosureReason = db.GetNullableString(reader, 8);
        entities.Case1.Number = db.GetString(reader, 9);
        entities.Case1.Status = db.GetNullableString(reader, 10);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 11);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 12);
        entities.Case1.OfficeIdentifier = db.GetNullableInt32(reader, 13);
        entities.Case1.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 14);
        entities.Case1.LastUpdatedBy = db.GetNullableString(reader, 15);
        entities.Case1.ClosureLetterDate = db.GetNullableDate(reader, 16);
        entities.Case1.DuplicateCaseIndicator =
          db.GetNullableString(reader, 17);
        entities.Case1.Note = db.GetNullableString(reader, 18);
        entities.CsePerson.Populated = true;
        entities.Case1.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePersonCaseRoleCase1()
  {
    entities.CaseRole.Populated = false;
    entities.CsePerson.Populated = false;
    entities.Case1.Populated = false;

    return ReadEach("ReadCsePersonCaseRoleCase1",
      (db, command) =>
      {
        db.SetString(command, "numb", export.Search.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 2);
        entities.CsePerson.LastUpdatedBy = db.GetNullableString(reader, 3);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 4);
        entities.CsePerson.FvLetterSentDate = db.GetNullableDate(reader, 5);
        entities.CsePerson.FviSetDate = db.GetNullableDate(reader, 6);
        entities.CsePerson.FviUpdatedBy = db.GetNullableString(reader, 7);
        entities.CaseRole.CasNumber = db.GetString(reader, 8);
        entities.Case1.Number = db.GetString(reader, 8);
        entities.CaseRole.Type1 = db.GetString(reader, 9);
        entities.CaseRole.Identifier = db.GetInt32(reader, 10);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 11);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 12);
        entities.Case1.ClosureReason = db.GetNullableString(reader, 13);
        entities.Case1.Status = db.GetNullableString(reader, 14);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 15);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 16);
        entities.Case1.OfficeIdentifier = db.GetNullableInt32(reader, 17);
        entities.Case1.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 18);
        entities.Case1.LastUpdatedBy = db.GetNullableString(reader, 19);
        entities.Case1.ClosureLetterDate = db.GetNullableDate(reader, 20);
        entities.Case1.DuplicateCaseIndicator =
          db.GetNullableString(reader, 21);
        entities.Case1.Note = db.GetNullableString(reader, 22);
        entities.CaseRole.Populated = true;
        entities.CsePerson.Populated = true;
        entities.Case1.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePersonCaseRoleCase2()
  {
    entities.CaseRole.Populated = false;
    entities.CsePerson.Populated = false;
    entities.Case1.Populated = false;

    return ReadEach("ReadCsePersonCaseRoleCase2",
      (db, command) =>
      {
        db.SetString(command, "numb", export.Search.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 2);
        entities.CsePerson.LastUpdatedBy = db.GetNullableString(reader, 3);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 4);
        entities.CsePerson.FvLetterSentDate = db.GetNullableDate(reader, 5);
        entities.CsePerson.FviSetDate = db.GetNullableDate(reader, 6);
        entities.CsePerson.FviUpdatedBy = db.GetNullableString(reader, 7);
        entities.CaseRole.CasNumber = db.GetString(reader, 8);
        entities.Case1.Number = db.GetString(reader, 8);
        entities.CaseRole.Type1 = db.GetString(reader, 9);
        entities.CaseRole.Identifier = db.GetInt32(reader, 10);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 11);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 12);
        entities.Case1.ClosureReason = db.GetNullableString(reader, 13);
        entities.Case1.Status = db.GetNullableString(reader, 14);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 15);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 16);
        entities.Case1.OfficeIdentifier = db.GetNullableInt32(reader, 17);
        entities.Case1.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 18);
        entities.Case1.LastUpdatedBy = db.GetNullableString(reader, 19);
        entities.Case1.ClosureLetterDate = db.GetNullableDate(reader, 20);
        entities.Case1.DuplicateCaseIndicator =
          db.GetNullableString(reader, 21);
        entities.Case1.Note = db.GetNullableString(reader, 22);
        entities.CaseRole.Populated = true;
        entities.CsePerson.Populated = true;
        entities.Case1.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);

        return true;
      });
  }

  private bool ReadInterstateRequest1()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest1",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 1);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 2);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 3);
        entities.InterstateRequest.Populated = true;
      });
  }

  private bool ReadInterstateRequest2()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest2",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 1);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 2);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 3);
        entities.InterstateRequest.Populated = true;
      });
  }

  private bool ReadOffice()
  {
    entities.ReopenOffice.Populated = false;

    return Read("ReadOffice",
      (db, command) =>
      {
        db.SetInt32(command, "officeId", export.ReopenOffice.SystemGeneratedId);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ReopenOffice.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ReopenOffice.Name = db.GetString(reader, 1);
        entities.ReopenOffice.EffectiveDate = db.GetDate(reader, 2);
        entities.ReopenOffice.DiscontinueDate = db.GetNullableDate(reader, 3);
        entities.ReopenOffice.OffOffice = db.GetNullableInt32(reader, 4);
        entities.ReopenOffice.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvider()
  {
    entities.ReopenOfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider",
      (db, command) =>
      {
        db.SetInt32(
          command, "offGeneratedId", entities.ReopenOffice.SystemGeneratedId);
        db.SetString(command, "userId", global.UserId);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ReopenOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.ReopenOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 1);
        entities.ReopenOfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.ReopenOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 3);
        entities.ReopenOfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.ReopenOfficeServiceProvider.Populated = true;
      });
  }

  private void UpdateCsePerson1()
  {
    var lastUpdatedTimestamp = local.Current.Timestamp;
    var lastUpdatedBy = global.UserId;
    var familyViolenceIndicator = export.CsePerson.FamilyViolenceIndicator ?? ""
      ;
    var fvLetterSentDate = export.CsePerson.FvLetterSentDate;
    var fviSetDate = local.Current.Date;

    entities.CsePerson.Populated = false;
    Update("UpdateCsePerson1",
      (db, command) =>
      {
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableString(command, "familyViolInd", familyViolenceIndicator);
        db.SetNullableDate(command, "fvLtrSentDt", fvLetterSentDate);
        db.SetNullableDate(command, "fviSetDate", fviSetDate);
        db.SetNullableString(command, "fviUpdatedBy", lastUpdatedBy);
        db.SetString(command, "numb", entities.CsePerson.Number);
      });

    entities.CsePerson.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CsePerson.LastUpdatedBy = lastUpdatedBy;
    entities.CsePerson.FamilyViolenceIndicator = familyViolenceIndicator;
    entities.CsePerson.FvLetterSentDate = fvLetterSentDate;
    entities.CsePerson.FviSetDate = fviSetDate;
    entities.CsePerson.FviUpdatedBy = lastUpdatedBy;
    entities.CsePerson.Populated = true;
  }

  private void UpdateCsePerson2()
  {
    var lastUpdatedTimestamp = local.Current.Timestamp;
    var lastUpdatedBy = global.UserId;
    var fvLetterSentDate = local.Current.Date;

    entities.CsePerson.Populated = false;
    Update("UpdateCsePerson2",
      (db, command) =>
      {
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDate(command, "fvLtrSentDt", fvLetterSentDate);
        db.SetString(command, "numb", entities.CsePerson.Number);
      });

    entities.CsePerson.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CsePerson.LastUpdatedBy = lastUpdatedBy;
    entities.CsePerson.FvLetterSentDate = fvLetterSentDate;
    entities.CsePerson.Populated = true;
  }

  private void UpdateCsePerson3()
  {
    var lastUpdatedTimestamp = local.Current.Timestamp;
    var lastUpdatedBy = global.UserId;
    var fvLetterSentDate = local.Current.Date;

    entities.Ch.Populated = false;
    Update("UpdateCsePerson3",
      (db, command) =>
      {
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDate(command, "fvLtrSentDt", fvLetterSentDate);
        db.SetString(command, "numb", entities.Ch.Number);
      });

    entities.Ch.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.Ch.LastUpdatedBy = lastUpdatedBy;
    entities.Ch.FvLetterSentDate = fvLetterSentDate;
    entities.Ch.Populated = true;
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
      /// A value of DetInter.
      /// </summary>
      [JsonPropertyName("detInter")]
      public WorkArea DetInter
      {
        get => detInter ??= new();
        set => detInter = value;
      }

      /// <summary>
      /// A value of DetCommon.
      /// </summary>
      [JsonPropertyName("detCommon")]
      public Common DetCommon
      {
        get => detCommon ??= new();
        set => detCommon = value;
      }

      /// <summary>
      /// A value of DetCase1.
      /// </summary>
      [JsonPropertyName("detCase1")]
      public Case1 DetCase1
      {
        get => detCase1 ??= new();
        set => detCase1 = value;
      }

      /// <summary>
      /// A value of DetCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("detCsePersonsWorkSet")]
      public CsePersonsWorkSet DetCsePersonsWorkSet
      {
        get => detCsePersonsWorkSet ??= new();
        set => detCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of DetCsePerson.
      /// </summary>
      [JsonPropertyName("detCsePerson")]
      public CsePerson DetCsePerson
      {
        get => detCsePerson ??= new();
        set => detCsePerson = value;
      }

      /// <summary>
      /// A value of DetCase2.
      /// </summary>
      [JsonPropertyName("detCase2")]
      public CaseRole DetCase2
      {
        get => detCase2 ??= new();
        set => detCase2 = value;
      }

      /// <summary>
      /// A value of DetFamily.
      /// </summary>
      [JsonPropertyName("detFamily")]
      public CaseRole DetFamily
      {
        get => detFamily ??= new();
        set => detFamily = value;
      }

      /// <summary>
      /// A value of DetStatus.
      /// </summary>
      [JsonPropertyName("detStatus")]
      public WorkArea DetStatus
      {
        get => detStatus ??= new();
        set => detStatus = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 13;

      private WorkArea detInter;
      private Common detCommon;
      private Case1 detCase1;
      private CsePersonsWorkSet detCsePersonsWorkSet;
      private CsePerson detCsePerson;
      private CaseRole detCase2;
      private CaseRole detFamily;
      private WorkArea detStatus;
    }

    /// <summary>A PageKeysGroup group.</summary>
    [Serializable]
    public class PageKeysGroup
    {
      /// <summary>
      /// A value of PageKeyCase.
      /// </summary>
      [JsonPropertyName("pageKeyCase")]
      public Case1 PageKeyCase
      {
        get => pageKeyCase ??= new();
        set => pageKeyCase = value;
      }

      /// <summary>
      /// A value of PageKeyStatus.
      /// </summary>
      [JsonPropertyName("pageKeyStatus")]
      public WorkArea PageKeyStatus
      {
        get => pageKeyStatus ??= new();
        set => pageKeyStatus = value;
      }

      /// <summary>
      /// A value of PageKeyCaseRole.
      /// </summary>
      [JsonPropertyName("pageKeyCaseRole")]
      public CaseRole PageKeyCaseRole
      {
        get => pageKeyCaseRole ??= new();
        set => pageKeyCaseRole = value;
      }

      /// <summary>
      /// A value of PageKeyCsePerson.
      /// </summary>
      [JsonPropertyName("pageKeyCsePerson")]
      public CsePerson PageKeyCsePerson
      {
        get => pageKeyCsePerson ??= new();
        set => pageKeyCsePerson = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 25;

      private Case1 pageKeyCase;
      private WorkArea pageKeyStatus;
      private CaseRole pageKeyCaseRole;
      private CsePerson pageKeyCsePerson;
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
    /// A value of GoToInrdReopen.
    /// </summary>
    [JsonPropertyName("goToInrdReopen")]
    public WorkArea GoToInrdReopen
    {
      get => goToInrdReopen ??= new();
      set => goToInrdReopen = value;
    }

    /// <summary>
    /// A value of HiddenPrev.
    /// </summary>
    [JsonPropertyName("hiddenPrev")]
    public CsePerson HiddenPrev
    {
      get => hiddenPrev ??= new();
      set => hiddenPrev = value;
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
    /// A value of FvPrompt.
    /// </summary>
    [JsonPropertyName("fvPrompt")]
    public Common FvPrompt
    {
      get => fvPrompt ??= new();
      set => fvPrompt = value;
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
    /// A value of HfromIapi.
    /// </summary>
    [JsonPropertyName("hfromIapi")]
    public Common HfromIapi
    {
      get => hfromIapi ??= new();
      set => hfromIapi = value;
    }

    /// <summary>
    /// A value of HfromPar1.
    /// </summary>
    [JsonPropertyName("hfromPar1")]
    public Common HfromPar1
    {
      get => hfromPar1 ??= new();
      set => hfromPar1 = value;
    }

    /// <summary>
    /// A value of HfromInrd.
    /// </summary>
    [JsonPropertyName("hfromInrd")]
    public Common HfromInrd
    {
      get => hfromInrd ??= new();
      set => hfromInrd = value;
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
    /// A value of InformationRequest.
    /// </summary>
    [JsonPropertyName("informationRequest")]
    public InformationRequest InformationRequest
    {
      get => informationRequest ??= new();
      set => informationRequest = value;
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
    /// A value of ReopenWorkArea.
    /// </summary>
    [JsonPropertyName("reopenWorkArea")]
    public WorkArea ReopenWorkArea
    {
      get => reopenWorkArea ??= new();
      set => reopenWorkArea = value;
    }

    /// <summary>
    /// A value of ReopenOffice.
    /// </summary>
    [JsonPropertyName("reopenOffice")]
    public Office ReopenOffice
    {
      get => reopenOffice ??= new();
      set => reopenOffice = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public CsePersonsWorkSet Previous
    {
      get => previous ??= new();
      set => previous = value;
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
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Case1 Next
    {
      get => next ??= new();
      set => next = value;
    }

    /// <summary>
    /// Gets a value of Import1.
    /// </summary>
    [JsonIgnore]
    public Array<ImportGroup> Import1 =>
      import1 ??= new(ImportGroup.Capacity, 0);

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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// Gets a value of PageKeys.
    /// </summary>
    [JsonIgnore]
    public Array<PageKeysGroup> PageKeys => pageKeys ??= new(
      PageKeysGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of PageKeys for json serialization.
    /// </summary>
    [JsonPropertyName("pageKeys")]
    [Computed]
    public IList<PageKeysGroup> PageKeys_Json
    {
      get => pageKeys;
      set => PageKeys.Assign(value);
    }

    /// <summary>
    /// A value of HiddenStandard.
    /// </summary>
    [JsonPropertyName("hiddenStandard")]
    public Standard HiddenStandard
    {
      get => hiddenStandard ??= new();
      set => hiddenStandard = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of NotFromReferral.
    /// </summary>
    [JsonPropertyName("notFromReferral")]
    public Common NotFromReferral
    {
      get => notFromReferral ??= new();
      set => notFromReferral = value;
    }

    private CsePersonsWorkSet ar;
    private WorkArea goToInrdReopen;
    private CsePerson hiddenPrev;
    private CodeValue selected;
    private Common fvPrompt;
    private CsePerson csePerson;
    private Common hfromIapi;
    private Common hfromPar1;
    private Common hfromInrd;
    private InterstateCase interstateCase;
    private InformationRequest informationRequest;
    private PaReferral paReferral;
    private WorkArea reopenWorkArea;
    private Office reopenOffice;
    private CsePersonsWorkSet previous;
    private CsePersonsWorkSet search;
    private Case1 next;
    private Array<ImportGroup> import1;
    private Standard standard;
    private Array<PageKeysGroup> pageKeys;
    private Standard hiddenStandard;
    private NextTranInfo hiddenNextTranInfo;
    private Case1 case1;
    private Common notFromReferral;
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
      /// A value of DetInter.
      /// </summary>
      [JsonPropertyName("detInter")]
      public WorkArea DetInter
      {
        get => detInter ??= new();
        set => detInter = value;
      }

      /// <summary>
      /// A value of DetCommon.
      /// </summary>
      [JsonPropertyName("detCommon")]
      public Common DetCommon
      {
        get => detCommon ??= new();
        set => detCommon = value;
      }

      /// <summary>
      /// A value of DetCase1.
      /// </summary>
      [JsonPropertyName("detCase1")]
      public Case1 DetCase1
      {
        get => detCase1 ??= new();
        set => detCase1 = value;
      }

      /// <summary>
      /// A value of DetCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("detCsePersonsWorkSet")]
      public CsePersonsWorkSet DetCsePersonsWorkSet
      {
        get => detCsePersonsWorkSet ??= new();
        set => detCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of DetCsePerson.
      /// </summary>
      [JsonPropertyName("detCsePerson")]
      public CsePerson DetCsePerson
      {
        get => detCsePerson ??= new();
        set => detCsePerson = value;
      }

      /// <summary>
      /// A value of DetCase2.
      /// </summary>
      [JsonPropertyName("detCase2")]
      public CaseRole DetCase2
      {
        get => detCase2 ??= new();
        set => detCase2 = value;
      }

      /// <summary>
      /// A value of DetFamily.
      /// </summary>
      [JsonPropertyName("detFamily")]
      public CaseRole DetFamily
      {
        get => detFamily ??= new();
        set => detFamily = value;
      }

      /// <summary>
      /// A value of DetStatus.
      /// </summary>
      [JsonPropertyName("detStatus")]
      public WorkArea DetStatus
      {
        get => detStatus ??= new();
        set => detStatus = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 13;

      private WorkArea detInter;
      private Common detCommon;
      private Case1 detCase1;
      private CsePersonsWorkSet detCsePersonsWorkSet;
      private CsePerson detCsePerson;
      private CaseRole detCase2;
      private CaseRole detFamily;
      private WorkArea detStatus;
    }

    /// <summary>A PageKeysGroup group.</summary>
    [Serializable]
    public class PageKeysGroup
    {
      /// <summary>
      /// A value of PageKeyCase.
      /// </summary>
      [JsonPropertyName("pageKeyCase")]
      public Case1 PageKeyCase
      {
        get => pageKeyCase ??= new();
        set => pageKeyCase = value;
      }

      /// <summary>
      /// A value of PageKeyStatus.
      /// </summary>
      [JsonPropertyName("pageKeyStatus")]
      public WorkArea PageKeyStatus
      {
        get => pageKeyStatus ??= new();
        set => pageKeyStatus = value;
      }

      /// <summary>
      /// A value of PageKeyCaseRole.
      /// </summary>
      [JsonPropertyName("pageKeyCaseRole")]
      public CaseRole PageKeyCaseRole
      {
        get => pageKeyCaseRole ??= new();
        set => pageKeyCaseRole = value;
      }

      /// <summary>
      /// A value of PageKeyCsePerson.
      /// </summary>
      [JsonPropertyName("pageKeyCsePerson")]
      public CsePerson PageKeyCsePerson
      {
        get => pageKeyCsePerson ??= new();
        set => pageKeyCsePerson = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 25;

      private Case1 pageKeyCase;
      private WorkArea pageKeyStatus;
      private CaseRole pageKeyCaseRole;
      private CsePerson pageKeyCsePerson;
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
    /// A value of GoToInrdReopen.
    /// </summary>
    [JsonPropertyName("goToInrdReopen")]
    public WorkArea GoToInrdReopen
    {
      get => goToInrdReopen ??= new();
      set => goToInrdReopen = value;
    }

    /// <summary>
    /// A value of HiddenPrev.
    /// </summary>
    [JsonPropertyName("hiddenPrev")]
    public CsePerson HiddenPrev
    {
      get => hiddenPrev ??= new();
      set => hiddenPrev = value;
    }

    /// <summary>
    /// A value of FvPrompt.
    /// </summary>
    [JsonPropertyName("fvPrompt")]
    public Common FvPrompt
    {
      get => fvPrompt ??= new();
      set => fvPrompt = value;
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
    /// A value of HfromIapi.
    /// </summary>
    [JsonPropertyName("hfromIapi")]
    public Common HfromIapi
    {
      get => hfromIapi ??= new();
      set => hfromIapi = value;
    }

    /// <summary>
    /// A value of HfromPar1.
    /// </summary>
    [JsonPropertyName("hfromPar1")]
    public Common HfromPar1
    {
      get => hfromPar1 ??= new();
      set => hfromPar1 = value;
    }

    /// <summary>
    /// A value of HfromInrd.
    /// </summary>
    [JsonPropertyName("hfromInrd")]
    public Common HfromInrd
    {
      get => hfromInrd ??= new();
      set => hfromInrd = value;
    }

    /// <summary>
    /// A value of Par1FromComn.
    /// </summary>
    [JsonPropertyName("par1FromComn")]
    public Common Par1FromComn
    {
      get => par1FromComn ??= new();
      set => par1FromComn = value;
    }

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
    /// A value of ReopenWorkArea.
    /// </summary>
    [JsonPropertyName("reopenWorkArea")]
    public WorkArea ReopenWorkArea
    {
      get => reopenWorkArea ??= new();
      set => reopenWorkArea = value;
    }

    /// <summary>
    /// A value of ReopenOffice.
    /// </summary>
    [JsonPropertyName("reopenOffice")]
    public Office ReopenOffice
    {
      get => reopenOffice ??= new();
      set => reopenOffice = value;
    }

    /// <summary>
    /// A value of HiddenSelectedCase.
    /// </summary>
    [JsonPropertyName("hiddenSelectedCase")]
    public Case1 HiddenSelectedCase
    {
      get => hiddenSelectedCase ??= new();
      set => hiddenSelectedCase = value;
    }

    /// <summary>
    /// A value of HiddenSelectedCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("hiddenSelectedCsePersonsWorkSet")]
    public CsePersonsWorkSet HiddenSelectedCsePersonsWorkSet
    {
      get => hiddenSelectedCsePersonsWorkSet ??= new();
      set => hiddenSelectedCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public CsePersonsWorkSet Previous
    {
      get => previous ??= new();
      set => previous = value;
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
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Case1 Next
    {
      get => next ??= new();
      set => next = value;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 =>
      export1 ??= new(ExportGroup.Capacity, 0);

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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// Gets a value of PageKeys.
    /// </summary>
    [JsonIgnore]
    public Array<PageKeysGroup> PageKeys => pageKeys ??= new(
      PageKeysGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of PageKeys for json serialization.
    /// </summary>
    [JsonPropertyName("pageKeys")]
    [Computed]
    public IList<PageKeysGroup> PageKeys_Json
    {
      get => pageKeys;
      set => PageKeys.Assign(value);
    }

    /// <summary>
    /// A value of HiddenStandard.
    /// </summary>
    [JsonPropertyName("hiddenStandard")]
    public Standard HiddenStandard
    {
      get => hiddenStandard ??= new();
      set => hiddenStandard = value;
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
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
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
    /// A value of NotFromReferral.
    /// </summary>
    [JsonPropertyName("notFromReferral")]
    public Common NotFromReferral
    {
      get => notFromReferral ??= new();
      set => notFromReferral = value;
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

    private CsePersonsWorkSet ar;
    private WorkArea goToInrdReopen;
    private CsePerson hiddenPrev;
    private Common fvPrompt;
    private CsePerson csePerson;
    private Common hfromIapi;
    private Common hfromPar1;
    private Common hfromInrd;
    private Common par1FromComn;
    private InformationRequest informationRequest;
    private WorkArea reopenWorkArea;
    private Office reopenOffice;
    private Case1 hiddenSelectedCase;
    private CsePersonsWorkSet hiddenSelectedCsePersonsWorkSet;
    private CsePersonsWorkSet previous;
    private CsePersonsWorkSet search;
    private Case1 next;
    private Array<ExportGroup> export1;
    private Standard standard;
    private Array<PageKeysGroup> pageKeys;
    private Standard hiddenStandard;
    private NextTranInfo hiddenNextTranInfo;
    private InterstateCase interstateCase;
    private PaReferral paReferral;
    private Common notFromReferral;
    private Code prompt;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of GoToInrdReopen.
    /// </summary>
    [JsonPropertyName("goToInrdReopen")]
    public WorkArea GoToInrdReopen
    {
      get => goToInrdReopen ??= new();
      set => goToInrdReopen = value;
    }

    /// <summary>
    /// A value of NonInterstateCase.
    /// </summary>
    [JsonPropertyName("nonInterstateCase")]
    public Common NonInterstateCase
    {
      get => nonInterstateCase ??= new();
      set => nonInterstateCase = value;
    }

    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public Common InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    /// <summary>
    /// A value of InterstateCaseNoLetter.
    /// </summary>
    [JsonPropertyName("interstateCaseNoLetter")]
    public Common InterstateCaseNoLetter
    {
      get => interstateCaseNoLetter ??= new();
      set => interstateCaseNoLetter = value;
    }

    /// <summary>
    /// A value of CaseInvolvement.
    /// </summary>
    [JsonPropertyName("caseInvolvement")]
    public Common CaseInvolvement
    {
      get => caseInvolvement ??= new();
      set => caseInvolvement = value;
    }

    /// <summary>
    /// A value of NoInterstateReqForCase.
    /// </summary>
    [JsonPropertyName("noInterstateReqForCase")]
    public Common NoInterstateReqForCase
    {
      get => noInterstateReqForCase ??= new();
      set => noInterstateReqForCase = value;
    }

    /// <summary>
    /// A value of ProduceFvLtr.
    /// </summary>
    [JsonPropertyName("produceFvLtr")]
    public Common ProduceFvLtr
    {
      get => produceFvLtr ??= new();
      set => produceFvLtr = value;
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
    /// A value of TurningOffFvi.
    /// </summary>
    [JsonPropertyName("turningOffFvi")]
    public Common TurningOffFvi
    {
      get => turningOffFvi ??= new();
      set => turningOffFvi = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public Case1 Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of Zdel.
    /// </summary>
    [JsonPropertyName("zdel")]
    public TextWorkArea Zdel
    {
      get => zdel ??= new();
      set => zdel = value;
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
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
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
    /// A value of CaseReopened.
    /// </summary>
    [JsonPropertyName("caseReopened")]
    public Common CaseReopened
    {
      get => caseReopened ??= new();
      set => caseReopened = value;
    }

    /// <summary>
    /// A value of Reopen.
    /// </summary>
    [JsonPropertyName("reopen")]
    public Office Reopen
    {
      get => reopen ??= new();
      set => reopen = value;
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

    /// <summary>
    /// A value of PageKeyCaseRole.
    /// </summary>
    [JsonPropertyName("pageKeyCaseRole")]
    public CaseRole PageKeyCaseRole
    {
      get => pageKeyCaseRole ??= new();
      set => pageKeyCaseRole = value;
    }

    /// <summary>
    /// A value of PageKeyCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("pageKeyCsePersonsWorkSet")]
    public CsePersonsWorkSet PageKeyCsePersonsWorkSet
    {
      get => pageKeyCsePersonsWorkSet ??= new();
      set => pageKeyCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of PageKeyCase.
    /// </summary>
    [JsonPropertyName("pageKeyCase")]
    public Case1 PageKeyCase
    {
      get => pageKeyCase ??= new();
      set => pageKeyCase = value;
    }

    /// <summary>
    /// A value of PageKeyStatus.
    /// </summary>
    [JsonPropertyName("pageKeyStatus")]
    public WorkArea PageKeyStatus
    {
      get => pageKeyStatus ??= new();
      set => pageKeyStatus = value;
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
    /// A value of Closure.
    /// </summary>
    [JsonPropertyName("closure")]
    public DateWorkArea Closure
    {
      get => closure ??= new();
      set => closure = value;
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

    private WorkArea goToInrdReopen;
    private Common nonInterstateCase;
    private Common interstateCase;
    private Common interstateCaseNoLetter;
    private Common caseInvolvement;
    private Common noInterstateReqForCase;
    private Common produceFvLtr;
    private CaseRole caseRole;
    private Document document;
    private SpDocKey spDocKey;
    private Common turningOffFvi;
    private Case1 previous;
    private TextWorkArea zdel;
    private CsePerson csePerson;
    private CodeValue codeValue;
    private Code code;
    private Common invalid;
    private Common caseReopened;
    private Office reopen;
    private DateWorkArea current;
    private CsePersonsWorkSet csePersonsWorkSet;
    private AbendData abendData;
    private CaseRole pageKeyCaseRole;
    private CsePersonsWorkSet pageKeyCsePersonsWorkSet;
    private Case1 pageKeyCase;
    private WorkArea pageKeyStatus;
    private Common common;
    private TextWorkArea textWorkArea;
    private Infrastructure infrastructure;
    private DateWorkArea closure;
    private DateWorkArea dateWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Ch.
    /// </summary>
    [JsonPropertyName("ch")]
    public CsePerson Ch
    {
      get => ch ??= new();
      set => ch = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

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
    /// A value of PaReferral.
    /// </summary>
    [JsonPropertyName("paReferral")]
    public PaReferral PaReferral
    {
      get => paReferral ??= new();
      set => paReferral = value;
    }

    /// <summary>
    /// A value of ReopenOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("reopenOfficeServiceProvider")]
    public OfficeServiceProvider ReopenOfficeServiceProvider
    {
      get => reopenOfficeServiceProvider ??= new();
      set => reopenOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of ReopenServiceProvider.
    /// </summary>
    [JsonPropertyName("reopenServiceProvider")]
    public ServiceProvider ReopenServiceProvider
    {
      get => reopenServiceProvider ??= new();
      set => reopenServiceProvider = value;
    }

    /// <summary>
    /// A value of ReopenOffice.
    /// </summary>
    [JsonPropertyName("reopenOffice")]
    public Office ReopenOffice
    {
      get => reopenOffice ??= new();
      set => reopenOffice = value;
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
    /// A value of Zdel.
    /// </summary>
    [JsonPropertyName("zdel")]
    public CaseUnit Zdel
    {
      get => zdel ??= new();
      set => zdel = value;
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

    private CsePerson ar;
    private CsePerson ch;
    private CaseRole caseRole;
    private CsePerson csePerson;
    private InformationRequest informationRequest;
    private PaReferral paReferral;
    private OfficeServiceProvider reopenOfficeServiceProvider;
    private ServiceProvider reopenServiceProvider;
    private Office reopenOffice;
    private Case1 case1;
    private CaseUnit zdel;
    private InterstateRequest interstateRequest;
  }
#endregion
}
