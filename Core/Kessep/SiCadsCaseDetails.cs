// Program: SI_CADS_CASE_DETAILS, ID: 371731756, model: 746.
// Short name: SWECADSP
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
/// A program: SI_CADS_CASE_DETAILS.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiCadsCaseDetails: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CADS_CASE_DETAILS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCadsCaseDetails(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCadsCaseDetails.
  /// </summary>
  public SiCadsCaseDetails(IContext context, Import import, Export export):
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
    //           M A I N T E N A N C E   L O G
    // Date	  Developer		Description
    // 12-20-95  Ken Evans		Initial Development
    // 01-26-96  P.Elie
    // 05/02/96  G.Lofton		Rework
    // 07/08/96  Rao Mulpuri		Rework
    // 09/12/96  G. Lofton		Corrected problems with
    // 				non-coop fields.
    // 11/02/96  G. Lofton		Add new security and removed old.
    // 12/07/96  G. Lofton - MTW	Add logic for case closure
    // 				and case function.
    // 12/27/96  G. Lofton - MTW	Add event logic.
    // 01/09/97  G. Lofton - MTW	Add external alerts logic.
    // 05/02/97  Sid			IDCR # 257/258
    // 06/13/97  Sid			View for closed cases.
    // 09/10/97  Sid			IDCR#383. DM changes for GC and NC.
    // 10/14/98  C Deghand             Added logic to check for changes on
    //                                 
    // service type.
    //                                 
    // Added IF statements to make the
    //                                 
    // select field error if no
    // selection
    //                                 
    // made.
    // 10/26/98  C Deghand             Added code to check for correct
    //                                 
    // indicator and to make sure
    //                                 
    // indicators are mutually
    // exclusive.
    // 10/28/98  C Deghand             Modified the good cause and
    //                                 
    // non coop fields to be protected
    //                                 
    // after being added.
    // 12/31/1998	M Ramirez	Revised the print process.
    // 				(Added to update.)
    // 12/31/1998	M Ramirez	Changed security to check CRUD actions only.
    // 12/31/1998	M Ramirez	Removed Case of Enter from main case of command.
    // 				Case Otherwise will handle this command.
    // -------------------------------------------------------------------
    // 03/04/99 W.Campbell             Added code to validate
    //                                 
    // that one service type is
    //                                 
    // designated with 'Y'.
    // -------------------------------------------------------------------
    // 04/06/1999	M Ramirez	Modified end of UPDATE to
    // 				accommodate Print of ARCLOS60 and
    // 				NOTCCLOS.
    // ------------------------------------------------------------
    // 04/07/99 W.Campbell             Added logic to insure
    //                                 
    // that if one or more APs exist
    //                                 
    // on the case, then one of them
    //                                 
    // must be entered for GC.  If no
    //                                 
    // APs exist on the case then,
    //                                 
    // the user must not enter one
    //                                 
    // for good cause (GC).
    // -----------------------------------------------
    // 04/08/99 W.Campbell             Replaced ZDEL exit states.
    // -----------------------------------------------
    // 04/14/99 M Ramirez	        Changed documents
    //                                 
    // from Online to Batch
    //                                 
    // (create a document trigger)
    // -----------------------------------------------------------
    // 05/02/99 W.Campbell             Added code to send
    //                                 
    // selection needed msg to COMP.
    //                                 
    // BE SURE TO MATCH
    //                                 
    // next_tran_info ON THE DIALOG
    //                                 
    // FLOW.
    // -----------------------------------------------
    // 05/19/99 W.Campbell             Added code to process
    //                                 
    // interstate case events as
    // needed.
    // -----------------------------------------------
    // 05/20/99 W.Campbell             Disabled code to process
    //                                 
    // interstate case events.
    // -------------------------------------------------------------------
    // 05/25/99 W.Campbell             Logic added to not
    //                                 
    // produce a document if the
    //                                 
    // case closure reason = RO
    //                                 
    // (Recovery Obligation).
    // -----------------------------------------------------------
    // 06/03/99 W.Campbell             Logic added to
    //                                 
    // handle case closure reason
    //                                 
    // = EM & 4D the same as NP.
    // -----------------------------------------------------------
    // 06/23/99 W.Campbell             Removed disabled
    //                                 
    // code to process interstate case
    //                                 
    // events as needed.
    // -------------------------------------------------------------------
    // 07/09/99 W.Campbell             Code disabled for
    //                                 
    // ZDEL views.  ZDEL views for
    //                                 
    // Interstate were replace with
    //                                 
    // new views and corresponding
    //                                 
    // changes were made to these
    //                                 
    // on the Screen definition.
    // ----------------------------------------------------------
    // 07/20/99 W.Campbell             Code disabled to
    //                                 
    // keep the person number
    //                                 
    // field from being populated
    //                                 
    // on a NEXT TRAN to this screen.
    // ----------------------------------------------------------
    // 07/20/99 W.Campbell             Changed screen field
    //                                 
    // properties for CASE #
    //                                 
    // to have the cursor put here.
    // ----------------------------------------------------------
    // 08/16/99 M.Lachowicz            Allow to update good cause
    //                                 
    // effective date to current date
    //                                 
    // only.
    // ----------------------------------------------------------
    // 09/16/1999 M Ramirez            Added check that the AR is a client
    // ----------------------------------------------------------
    // 09/28/99 M Lachowicz            Protect database againt
    //                                 
    // update status of CASE to blank
    // and
    //                                 
    // status date of CASE to
    //                                 
    // '0001-01-01'. PR #74990.
    // ----------------------------------------------------------
    // 10/15/99 W.Campbell             Logic added to not
    //                                 
    // require closure letter date
    //                                 
    // if the AR is an Organization
    //                                 
    // ( last char of CSE PERSON
    //                                 
    // number = 'O' (alpha O)).
    //                                 
    // Work done on PR#H00077426.
    // -----------------------------------------------------------
    // 11/04/99 M Lachowicz            Protect database againt
    //                                 
    // update status of CASE to blank.
    //                                 
    // PR #78207.
    // ----------------------------------------------------------
    // 11/09/99 M Lachowicz            Protect database againt
    //                                 
    // update status_date of CASE to
    //                                 
    // 0001-01-01.
    //                                 
    // PR #78205.
    // ----------------------------------------------------------
    // 11/17/99 M Lachowicz            Protect PA Med field if there is
    //                                 
    // any other program than MA, MK,
    // MS or
    //                                 
    // MP.
    //                                 
    // Al these programs can occur
    // together
    //                                 
    // with FS program.
    //                                 
    // PR # 79970.
    // ----------------------------------------------------------
    // 03/04/00 W.Campbell             Added local view for expedited
    //                                 
    // paternity and logic to use that
    // to
    //                                 
    // determine if Expedited Paternity
    // is
    //                                 
    // being updated to a value of "Y".
    // If it
    //                                 
    // is, then display a modified
    // Update
    //                                 
    // successful exit state msg to the
    // user
    //                                 
    // which reminds them to validate
    // the
    //                                 
    // other paternity information on
    // this case.
    //                                 
    // This work was done on WR# 000160
    //                                 
    // for PRWORA - Paternity
    // modifications.
    // -----------------------------------------------------
    // 06/05/00 M Lachowicz            Added logic to not
    //                                 
    // produce batch letter when
    //                                 
    // CSE Closure letter is updated to
    // blanks.
    //                                 
    // PR 96598.
    // -----------------------------------------------------
    // 07/07/00 W.Campbell             Logic added to not
    //                                 
    // allow the Closure date to be the
    //                                 
    // same as the Closure letter date.
    //                                 
    // The Closure date must be > the
    //                                 
    // Closure letter date.
    //                                 
    // Work done on PR#98503.
    // -----------------------------------------------------------
    // 07/07/00 W.Campbell             Code added to fix
    //                                 
    // problem with not allowing
    //                                 
    // update of closed case.
    //                                 
    // Work done on PR#98438.
    // -------------------------------------------------------------------
    // 07/18/00 W.Campbell             Modified Code dealing with
    //                                 
    // Duplicate Case Indicator to fix
    //                                 
    // problems reported on PR#98232.
    // -------------------------------------------------------------------
    // 11/27/00 M.Lachowicz            Changed header line.
    //                                 
    // WR #298.
    // -----------------------------------------------
    // 03/26/01 swsrchf    I00115283  Removed the disabled code.
    //                                
    // ARCLOS60 and NOTCCLOS no longer
    // triggered for
    //                                
    // Organizations
    // 03/26/01 swsrchf    WR 000240  Case cannot be CLOSED, if it has active 
    // Debts.
    //                                
    // Commented out Exit State
    // CASE_SUCCESSFULLY_CLOSED
    // --------------------------------------------------------------------------------
    // 12/18/01 M.Lachowicz            Fixed  PR 134213.
    // -----------------------------------------------
    // 01/25/02 M.Lachowicz            Allow worker to send closure
    //                                 
    // letter without closing case.  PR
    // 137179.
    // -----------------------------------------------
    // 02/04/02 M.Lachowicz            This is another change for WR20131.
    //                                 
    // Send closure letter even if
    // closure reason
    //                                 
    // code is 'AR' or 'RO'.  This
    // change was
    //                                 
    // formally part of the above PR
    // 137179.
    // -----------------------------------------------
    // 02/05/02 M.Lachowicz            This is another change for WR20131.
    //                                 
    // Do not allow to change closure
    // reason if closure letter
    //                                 
    // date is not current.  This
    // change was
    //                                 
    // formally part of the above PR
    // 137179.
    // -----------------------------------------------
    // 02/13/02 M.Lachowicz            Allow to close case if closure
    //                                 
    // letter was sent and closure
    // reason was
    //                                 
    // blank. PR 138851.
    // -----------------------------------------------
    // 02/15/02 M.Lachowicz            Allow to close case if AR is
    //                                 
    // organization without sending a
    // closure
    //                                 
    // letter. PR 138876.
    // -----------------------------------------------
    // 03/08/2002	M Ramirez	PR133505
    // Don't send documents if there is incoming interstate involvement
    // -----------------------------------------------
    // 03/19/2002 M.Lachowicz          Allow to remove Closure_Reason
    //                                 
    // and Closure_Letter_Date.
    //                                 
    // PR140864 and PR141615.
    // -----------------------------------------------
    // 05/20/2002 T.Bobb  PR00146072   Added statement to set the AP
    //                                 
    // when exiting procedure with a
    // NEXTTRAN.
    // ------------------------------------------------
    // 05/04/2006  GVandy  WR230751    Add No Jurisdiction code.
    // 				Change interstate IV-D agency to 5 characters in length.
    // ------------------------------------------------
    // 07/15/2011   AHockman  CQ00024980    make duplicate case indicator 
    // display only.
    //  Removed prompt so field is protected display only.  Data for this field 
    // is entered on IIMC screen.
    // ------------------------------------------------
    // -----------------------------------------------------------------------------------------------------------
    //  Date		Developer	Request #      	Description
    // -----------------------------------------------------------------------------------------------------------
    // 08/31/2020      Raj S           CQ67478		Modified to Add new Case closure
    // codes (SA, AM, IH, SS, LS,
    //                                                 
    // BG, TR), End Date IW Reason code and changes to
    // closure
    //                                                 
    // letter transmission on certain closure reason
    // codes.
    // 01/06/2021      Raj S           CQ68909         Modfided to remove 
    // requirement of Intent to Close Letter
    //                                                 
    // and stop generating Case Closure Letter for Case
    // Clousure
    //                                                 
    // Reason Code BG.
    // -----------------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    MoveNextTranInfo(import.HiddenNextTranInfo, export.HiddenNextTranInfo);
    local.Current.Date = Now().Date;

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // ---------------------------------------------
    // Move Imports to Exports
    // ---------------------------------------------
    export.Next.Number = import.Next.Number;
    export.Program.Code = import.Program.Code;
    export.Case1.Assign(import.Case1);
    export.HiddenCase.Assign(import.HiddenCase);
    export.ArCaseRole.Assign(import.ArCaseRole);
    export.Ap.Assign(import.Ap);
    MoveCsePersonsWorkSet(import.ArCsePersonsWorkSet, export.ArCsePersonsWorkSet);
      
    MoveCsePersonsWorkSet(import.ArCsePersonsWorkSet, export.ArCsePersonsWorkSet);
      
    MoveCaseFuncWorkSet(import.CaseFuncWorkSet, export.CaseFuncWorkSet);
    export.HiddenRedisplay.Flag = import.HiddenRedisplay.Flag;
    export.DesignatedPayeeFnd.Flag = import.DesignatedPayeeFnd.Flag;
    export.ApPrompt.SelectChar = import.ApPrompt.SelectChar;
    export.ArPrompt.SelectChar = import.ArPrompt.SelectChar;
    export.PaMedCdPrompt.SelectChar = import.PaMedCdPrompt.SelectChar;
    export.TermCodePrompt.SelectChar = import.TermCodePrompt.SelectChar;
    export.CaseClosureRsnPrompt.SelectChar =
      import.CaseClosureRsnPrompt.SelectChar;
    export.DuplicateIndPrompt.SelectChar = import.DuplicateIndPrompt.SelectChar;
    export.NoJurisdictionPrompt.SelectChar =
      import.NoJurisdictionPrompt.SelectChar;
    MoveOffice(import.Office, export.Office);
    export.FromCads.Flag = import.FromCads.Flag;

    // 11/27/00 M.L Start
    export.HeaderLine.Text35 = import.HeaderLine.Text35;

    // 11/27/00 M.L End
    export.ServiceProvider.LastName = import.ServiceProvider.LastName;
    export.CaseOpen.Flag = import.CaseOpen.Flag;
    export.CaseSuccessfullyClosed.Flag = import.CaseSuccessfullyClosed.Flag;
    export.InterstateRequest.KsCaseInd = import.InterstateRequest.KsCaseInd;

    // 11/09/99 M.L Start
    export.Original.StatusDate = import.Original.StatusDate;

    // 11/09/99 M.L End
    // 11/04/99 M.L  Start
    export.SuccessfullyDisplayed.Flag = import.SuccessfullyDisplayed.Flag;

    // 11/04/99 M.L  End
    if (!IsEmpty(export.HiddenRedisplay.Flag))
    {
      if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "EXIT") || Equal
        (global.Command, "SIGNOFF") || Equal(global.Command, "ENTER") || Equal
        (global.Command, "ADD"))
      {
      }
      else
      {
        ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";

        return;
      }
    }

    export.Interstate.Index = 0;
    export.Interstate.Clear();

    for(import.Interstate.Index = 0; import.Interstate.Index < import
      .Interstate.Count; ++import.Interstate.Index)
    {
      if (export.Interstate.IsFull)
      {
        break;
      }

      export.Interstate.Update.Interstate1.Assign(
        import.Interstate.Item.Interstate1);

      // @@@
      if (IsEmpty(export.Interstate.Item.Interstate1.Text10))
      {
        // Interstate involvement is to a domestic IV-D agency.
        var field = GetField(export.Interstate.Item.Interstate1, "text8");

        field.Protected = true;
      }
      else
      {
        // Interstate involvement is to a foreign or tribal IV-D agency.  
        // Highlight the interstate agency code in reverse video.
        var field = GetField(export.Interstate.Item.Interstate1, "text8");

        field.Highlighting = Highlighting.ReverseVideo;
        field.Protected = true;
      }

      export.Interstate.Next();
    }

    // initialize Good Cause Group  export view
    for(import.Gc.Index = 0; import.Gc.Index < import.Gc.Count; ++
      import.Gc.Index)
    {
      if (!import.Gc.CheckSize())
      {
        break;
      }

      export.Gc.Index = import.Gc.Index;
      export.Gc.CheckSize();

      export.Gc.Update.GcDetCommon.SelectChar =
        import.Gc.Item.GcDetCommon.SelectChar;
      export.Gc.Update.GcDetApCsePersonsWorkSet.Number =
        import.Gc.Item.GcDetApCsePersonsWorkSet.Number;
      MoveCaseRole(import.Gc.Item.GcDetApCaseRole,
        export.Gc.Update.GcDetApCaseRole);
      export.Gc.Update.GcCodePrompt.SelectChar =
        import.Gc.Item.GcCodePrompt.SelectChar;
      export.Gc.Update.GcDetGoodCause.Assign(import.Gc.Item.GcDetGoodCause);
    }

    import.Gc.CheckIndex();

    // initialize Non-Coop Group  export view
    for(import.Nc.Index = 0; import.Nc.Index < import.Nc.Count; ++
      import.Nc.Index)
    {
      if (!import.Nc.CheckSize())
      {
        break;
      }

      export.Nc.Index = import.Nc.Index;
      export.Nc.CheckSize();

      export.Nc.Update.NcDetCommon.SelectChar =
        import.Nc.Item.NcDetCommon.SelectChar;
      export.Nc.Update.NcDetApCsePersonsWorkSet.Number =
        import.Nc.Item.NcDetApCsePersonsWorkSet.Number;
      MoveCaseRole(import.Nc.Item.NcDetApCaseRole,
        export.Nc.Update.NcDetApCaseRole);
      export.Nc.Update.NcCodePrmpt.SelectChar =
        import.Nc.Item.NcCodePrompt.SelectChar;
      export.Nc.Update.NcRsnPrompt.SelectChar =
        import.Nc.Item.NcRsnPrompt.SelectChar;
      export.Nc.Update.NcDetNonCooperation.Assign(
        import.Nc.Item.NcDetNonCooperation);
    }

    import.Nc.CheckIndex();
    export.GcMinus.OneChar = import.GcMinus.OneChar;
    export.GcPlus.OneChar = import.GcPlus.OneChar;
    export.NcMinus.OneChar = import.NcMinus.OneChar;
    export.NcPlus.OneChar = import.NcPlus.OneChar;
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // ---------------------------------------------
    //         	Move hidden views
    // ---------------------------------------------
    export.Gc1.PageNumber = import.Gc1.PageNumber;
    export.Nc1.PageNumber = import.Nc1.PageNumber;
    export.CurrItmNoGc.Count = import.CurrItmNoGc.Count;
    export.CurrItmNoNc.Count = import.CurrItmNoNc.Count;
    export.HidNoItemsFndGc.Count = import.HidNoItemsFndGc.Count;
    export.HidNoItemsFndNc.Count = import.HidNoItemsFndNc.Count;
    export.HiddenPrev.Number = import.HiddenPrev.Number;
    export.ArPrev.Assign(import.ArPrev);
    export.Prev.Assign(import.Prev);
    export.MedProgExists.Flag = import.MedProgExists.Flag;
    export.NoItemsGc.Count = import.NoItemsGc.Count;
    export.NoItemsNc.Count = import.NoItemsNc.Count;
    export.MaxPagesGc.Count = import.MaxPagesGc.Count;
    export.MaxPagesNc.Count = import.MaxPagesNc.Count;

    if (Equal(global.Command, "DISPLAY"))
    {
      if (AsChar(export.ApPrompt.SelectChar) == 'S')
      {
        export.ApPrompt.SelectChar = "";
      }
    }
    else
    {
      if (!import.HiddenGrpGc.IsEmpty)
      {
        for(import.HiddenGrpGc.Index = 0; import.HiddenGrpGc.Index < import
          .HiddenGrpGc.Count; ++import.HiddenGrpGc.Index)
        {
          if (!import.HiddenGrpGc.CheckSize())
          {
            break;
          }

          export.HiddenGrpGc.Index = import.HiddenGrpGc.Index;
          export.HiddenGrpGc.CheckSize();

          export.HiddenGrpGc.Update.HiddenGrpGcDetCommon.SelectChar =
            import.HiddenGrpGc.Item.HiddenGrpGcDetCommon.SelectChar;
          export.HiddenGrpGc.Update.HiddenGrpGcDetCsePersonsWorkSet.Number =
            import.HiddenGrpGc.Item.HiddenGrpGcDetApCsePersonsWorkSet.Number;
          MoveCaseRole(import.HiddenGrpGc.Item.HiddenGrpGcDetApCaseRole,
            export.HiddenGrpGc.Update.HiddenGrpGcDetAp);
          export.HiddenGrpGc.Update.HiddenGrpGcDetGoodCause.Assign(
            import.HiddenGrpGc.Item.HiddenGrpGcDetGoodCause);
        }

        import.HiddenGrpGc.CheckIndex();
      }

      if (!import.HiddenGrpNc.IsEmpty)
      {
        for(import.HiddenGrpNc.Index = 0; import.HiddenGrpNc.Index < import
          .HiddenGrpNc.Count; ++import.HiddenGrpNc.Index)
        {
          if (!import.HiddenGrpNc.CheckSize())
          {
            break;
          }

          export.HiddenGrpNc.Index = import.HiddenGrpNc.Index;
          export.HiddenGrpNc.CheckSize();

          export.HiddenGrpNc.Update.HiddenGrpNcDetCommon.SelectChar =
            import.HiddenGrpNc.Item.HiddenGrpNcDetCommon.SelectChar;
          export.HiddenGrpNc.Update.HiddenGrpNcDetApCsePersonsWorkSet.Number =
            import.HiddenGrpNc.Item.HiddenGrpNcDetApCsePersonsWorkSet.Number;
          MoveCaseRole(import.HiddenGrpNc.Item.HiddenGrpNcDetApCaseRole,
            export.HiddenGrpNc.Update.HiddenGrpNcDetApCaseRole);
          export.HiddenGrpNc.Update.HiddenGrpNcDetNonCooperation.Assign(
            import.HiddenGrpNc.Item.HiddenGrpNcDetNonCooperation);
        }

        import.HiddenGrpNc.CheckIndex();
      }
    }

    // ============================================================
    // NEXTTRAN AND SECURITY  LOGIC
    // ============================================================
    // If the next tran info is not equal to spaces, this implies
    // the user requested a next tran action. now validate
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      export.HiddenNextTranInfo.CaseNumber = export.Case1.Number;
      export.HiddenNextTranInfo.CsePersonNumber = export.Ap.Number;

      // >>
      // 05/22/02 TB start
      export.HiddenNextTranInfo.CsePersonNumberAp = export.Ap.Number;

      // >>
      // 05/22/02 TB end
      UseScCabNextTranPut();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // this is where you set your export value to the export hidden
      // next tran values if the user is comming into this procedure
      // on a next tran action.
      UseScCabNextTranGet();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        if (Equal(export.HiddenNextTranInfo.LastTran, "SRPT") || Equal
          (export.HiddenNextTranInfo.LastTran, "SRPU"))
        {
          local.LastTran.SystemGeneratedIdentifier =
            export.HiddenNextTranInfo.InfrastructureId.GetValueOrDefault();
          UseOeCabReadInfrastructure();
          export.Ap.Number = local.LastTran.CsePersonNumber ?? Spaces(10);
          export.Next.Number = local.LastTran.CaseNumber ?? Spaces(10);
        }
        else
        {
          export.Next.Number = export.HiddenNextTranInfo.CaseNumber ?? Spaces
            (10);
          export.Case1.Number = export.HiddenNextTranInfo.CaseNumber ?? Spaces
            (10);
        }

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

    if (Equal(global.Command, "UPDATE"))
    {
      if (AsChar(export.CaseOpen.Flag) == 'N')
      {
        ExitState = "CANNOT_MODIFY_CLOSED_CASE";

        return;
      }
    }

    // IF SELCTION IS MADE NO SCROLLING WILL BE ALLOWED
    if (local.GrpGcCnt.Count > 0 || local.GrpNcCnt.Count > 0)
    {
      switch(TrimEnd(global.Command))
      {
        case "GCPV":
          break;
        case "GCNX":
          break;
        case "NCPV":
          break;
        case "NCNX":
          break;
        default:
          goto Test1;
      }

      ExitState = "ACO_NE0000_SCROLL_INVALID_W_SEL";
    }

Test1:

    if (Equal(global.Command, "RETROLE"))
    {
      export.FromCads.Flag = "Y";

      if (AsChar(export.ArPrompt.SelectChar) == 'S')
      {
        export.ArPrompt.SelectChar = "";
      }

      if (!IsEmpty(import.SelectedAr.Number))
      {
        MoveCsePersonsWorkSet(import.SelectedAr, export.ArCsePersonsWorkSet);
      }
      else
      {
        export.FromCads.Flag = "Y";
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "RETCOMP"))
    {
      if (AsChar(export.ApPrompt.SelectChar) == 'S')
      {
        export.ApPrompt.SelectChar = "";
      }

      if (!IsEmpty(import.SelectedAp.Number))
      {
        MoveCsePersonsWorkSet(import.SelectedAp, export.Ap);
      }

      global.Command = "DISPLAY";
    }

    if (!Equal(global.Command, "DISPLAY"))
    {
      if (AsChar(export.MedProgExists.Flag) != 'Y')
      {
        var field1 = GetField(export.Case1, "paMedicalService");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.PaMedCdPrompt, "selectChar");

        field2.Color = "cyan";
        field2.Protected = true;
      }
    }

    // mjr
    // -----------------------------------------------------
    // 12/31/1998
    // Changed security to check on CRUD actions only.
    // ------------------------------------------------------------------
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "ADD") || Equal
      (global.Command, "UPDATE"))
    {
      // to validate action level security
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        // mjr---> Changed this escape to exit the PrAD.
        return;
      }
    }

    // ---------------------------------------------
    //        P F K E Y   P R O C E S S I N G
    // ---------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "RTLIST":
        // ----------------------------------------------------------
        // On return from list(code table etc), populate the retreive
        // data to field on the screen.
        // ----------------------------------------------------------
        if (AsChar(export.MedProgExists.Flag) == 'Y')
        {
          if (AsChar(export.PaMedCdPrompt.SelectChar) == 'S')
          {
            if (!IsEmpty(import.Selected.Cdvalue))
            {
              export.Case1.PaMedicalService = import.Selected.Cdvalue;
            }

            export.PaMedCdPrompt.SelectChar = "+";

            var field1 = GetField(export.PaMedCdPrompt, "selectChar");

            field1.Color = "green";
            field1.Highlighting = Highlighting.Underscore;
            field1.Protected = false;
            field1.Focused = true;
          }
        }

        if (AsChar(export.TermCodePrompt.SelectChar) == 'S')
        {
          if (!IsEmpty(import.Selected.Cdvalue))
          {
            export.ArCaseRole.AssignmentTerminationCode =
              import.Selected.Cdvalue;
          }

          export.TermCodePrompt.SelectChar = "";

          var field1 = GetField(export.TermCodePrompt, "selectChar");

          field1.Color = "green";
          field1.Highlighting = Highlighting.Underscore;
          field1.Protected = false;
          field1.Focused = true;
        }

        if (AsChar(export.CaseClosureRsnPrompt.SelectChar) == 'S')
        {
          if (!IsEmpty(import.Selected.Cdvalue))
          {
            export.Case1.ClosureReason = import.Selected.Cdvalue;
          }

          export.CaseClosureRsnPrompt.SelectChar = "";
        }

        // @@@
        if (AsChar(export.NoJurisdictionPrompt.SelectChar) == 'S')
        {
          if (!IsEmpty(import.Selected.Cdvalue))
          {
            export.Case1.NoJurisdictionCd = import.Selected.Cdvalue;
          }

          export.NoJurisdictionPrompt.SelectChar = "";
        }

        if (AsChar(export.DuplicateIndPrompt.SelectChar) == 'S')
        {
          // -------------------------------------------------------------------
          // 07/18/00 W.Campbell - Added the following
          // code to replaced the above code with different logic.
          // Work done on PR#98232.
          // -------------------------------------------------------------------
          if (!IsEmpty(import.Selected.Description))
          {
            export.Case1.DuplicateCaseIndicator = import.Selected.Cdvalue;

            if (AsChar(export.Case1.DuplicateCaseIndicator) == 'S')
            {
              // -------------------------------------------------------------------
              // 07/18/00 W.Campbell - 'S' for 'S'PACE
              // from the code table.
              // -------------------------------------------------------------------
              export.Case1.DuplicateCaseIndicator = "";
            }
          }

          // -------------------------------------------------------------------
          // 07/18/00 W.Campbell - End of Added code
          // to replaced the above code with different logic.
          // Work done on PR#98232.
          // -------------------------------------------------------------------
          export.DuplicateIndPrompt.SelectChar = "";
        }

        if (!export.Gc.IsEmpty)
        {
          // Validate Good Cause Prompt
          for(export.Gc.Index = 0; export.Gc.Index < export.Gc.Count; ++
            export.Gc.Index)
          {
            if (!export.Gc.CheckSize())
            {
              break;
            }

            if (AsChar(export.Gc.Item.GcCodePrompt.SelectChar) == 'S')
            {
              if (!IsEmpty(import.Selected.Cdvalue))
              {
                export.Gc.Update.GcDetGoodCause.Code = import.Selected.Cdvalue;
              }

              export.Gc.Update.GcCodePrompt.SelectChar = "+";

              var field1 = GetField(export.Gc.Item.GcCodePrompt, "selectChar");

              field1.Color = "green";
              field1.Highlighting = Highlighting.Underscore;
              field1.Protected = false;
              field1.Focused = true;

              goto Test2;
            }
          }

          export.Gc.CheckIndex();
        }

Test2:

        if (!export.Nc.IsEmpty)
        {
          // Validate Non-Coop Prompt
          for(export.Nc.Index = 0; export.Nc.Index < export.Nc.Count; ++
            export.Nc.Index)
          {
            if (!export.Nc.CheckSize())
            {
              break;
            }

            if (AsChar(export.Nc.Item.NcCodePrmpt.SelectChar) == 'S')
            {
              if (!IsEmpty(import.Selected.Cdvalue))
              {
                export.Nc.Update.NcDetNonCooperation.Code =
                  import.Selected.Cdvalue;
              }

              export.Nc.Update.NcCodePrmpt.SelectChar = "+";

              var field1 = GetField(export.Nc.Item.NcCodePrmpt, "selectChar");

              field1.Color = "green";
              field1.Highlighting = Highlighting.Underscore;
              field1.Protected = false;
              field1.Focused = true;

              goto Test16;
            }

            if (AsChar(export.Nc.Item.NcRsnPrompt.SelectChar) == 'S')
            {
              if (!IsEmpty(import.Selected.Cdvalue))
              {
                export.Nc.Update.NcDetNonCooperation.Reason =
                  import.Selected.Cdvalue;
              }

              export.Nc.Update.NcRsnPrompt.SelectChar = "+";

              var field1 = GetField(export.Nc.Item.NcRsnPrompt, "selectChar");

              field1.Color = "green";
              field1.Highlighting = Highlighting.Underscore;
              field1.Protected = false;
              field1.Focused = true;

              goto Test16;
            }
          }

          export.Nc.CheckIndex();
        }

        break;
      case "ADD":
        // 11/04/99 M.L Start
        if (AsChar(export.SuccessfullyDisplayed.Flag) != 'Y')
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_ADD";

          break;
        }

        // 11/04/99 M.L End
        if (!Equal(export.Case1.ClosureLetterDate, export.Prev.ClosureLetterDate))
          
        {
          var field1 = GetField(export.Case1, "closureLetterDate");

          field1.Error = true;

          ExitState = "SI0000_CANT_ADD_CLOSURE_LTR";

          break;
        }

        if (!Equal(export.Case1.StatusDate, export.Prev.StatusDate) || !
          Equal(export.Case1.ClosureReason, export.Prev.ClosureReason))
        {
          var field1 = GetField(export.Case1, "closureReason");

          field1.Error = true;

          var field2 = GetField(export.Case1, "statusDate");

          field2.Error = true;

          ExitState = "SI0000_CANT_ADD_CLOSURE_DT_N_RSN";

          break;
        }

        // @@@
        if (!Equal(export.Case1.NoJurisdictionCd, export.Prev.NoJurisdictionCd))
        {
          var field1 = GetField(export.Case1, "noJurisdictionCd");

          field1.Error = true;

          ExitState = "SI0000_CANT_ADD_NO_JURISDICTION";

          break;
        }

        local.Row1AddFuncGc.Flag = "";
        local.Row1AddFuncNc.Flag = "";

        // --------------------------------------------------------------
        // 10/14/98  Added the IF statements in the
        // case of SPACES to set the error and
        // place the cursor.
        // --------------------------------------------------------------
        for(export.Gc.Index = 0; export.Gc.Index < export.Gc.Count; ++
          export.Gc.Index)
        {
          if (!export.Gc.CheckSize())
          {
            break;
          }

          if (export.Gc.Index == 0)
          {
            switch(AsChar(export.Gc.Item.GcDetCommon.SelectChar))
            {
              case 'S':
                ++local.GrpGcCnt.Count;
                local.Row1AddFuncGc.Flag = "Y";

                break;
              case ' ':
                if (!IsEmpty(export.Gc.Item.GcDetGoodCause.Code))
                {
                  var field2 =
                    GetField(export.Gc.Item.GcDetCommon, "selectChar");

                  field2.Error = true;
                }

                break;
              default:
                var field1 = GetField(export.Gc.Item.GcDetCommon, "selectChar");

                field1.Error = true;

                ++local.Invalid.Count;
                ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

                break;
            }
          }
          else if (IsEmpty(export.Gc.Item.GcDetCommon.SelectChar))
          {
          }
          else
          {
            ++local.GrpGcCnt.Count;
            ++local.Invalid.Count;

            var field1 = GetField(export.Gc.Item.GcDetCommon, "selectChar");

            field1.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
          }
        }

        export.Gc.CheckIndex();

        for(export.Nc.Index = 0; export.Nc.Index < export.Nc.Count; ++
          export.Nc.Index)
        {
          if (!export.Nc.CheckSize())
          {
            break;
          }

          if (export.Nc.Index == 0)
          {
            switch(AsChar(export.Nc.Item.NcDetCommon.SelectChar))
            {
              case 'S':
                ++local.GrpNcCnt.Count;
                local.Row1AddFuncNc.Flag = "Y";

                break;
              case ' ':
                if (!IsEmpty(export.Nc.Item.NcDetNonCooperation.Code))
                {
                  var field2 =
                    GetField(export.Nc.Item.NcDetCommon, "selectChar");

                  field2.Error = true;
                }

                break;
              default:
                var field1 = GetField(export.Nc.Item.NcDetCommon, "selectChar");

                field1.Error = true;

                ++local.Invalid.Count;
                ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

                break;
            }
          }
          else if (IsEmpty(export.Nc.Item.NcDetCommon.SelectChar))
          {
          }
          else
          {
            ++local.GrpNcCnt.Count;
            ++local.Invalid.Count;

            var field1 = GetField(export.Nc.Item.NcDetCommon, "selectChar");

            field1.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
          }
        }

        export.Nc.CheckIndex();

        if (IsEmpty(local.Row1AddFuncGc.Flag) && IsEmpty
          (local.Row1AddFuncNc.Flag))
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        // Update ap data if selection was made
        if (AsChar(local.Row1AddFuncGc.Flag) == 'Y')
        {
          export.Gc.Index = 0;
          export.Gc.CheckSize();

          if (AsChar(export.Gc.Item.GcDetCommon.SelectChar) == 'S')
          {
            if (IsEmpty(export.Gc.Item.GcDetGoodCause.Code))
            {
              ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

              var field1 = GetField(export.Gc.Item.GcDetGoodCause, "code");

              field1.Error = true;
            }

            if (IsEmpty(export.Gc.Item.GcDetApCsePersonsWorkSet.Number))
            {
            }
            else
            {
              UseCabZeroFillNumber3();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                if (IsExitState("PERSON_NUMBER_NOT_NUMERIC"))
                {
                  var field1 =
                    GetField(export.Gc.Item.GcDetApCsePersonsWorkSet, "number");
                    

                  field1.Error = true;

                  break;
                }
              }
            }

            if (Lt(local.Blank.Date, export.Gc.Item.GcDetGoodCause.EffectiveDate))
              
            {
              // 08/16/99 M.L Start
              if (!Equal(export.Gc.Item.GcDetGoodCause.EffectiveDate,
                local.Current.Date))
              {
                var field1 =
                  GetField(export.Gc.Item.GcDetGoodCause, "effectiveDate");

                field1.Error = true;

                ExitState = "ACO_NE0000_DT_MUST_B_CURRENT_DT";

                break;
              }

              // 08/16/99 M.L End
            }
            else
            {
              export.Gc.Update.GcDetGoodCause.EffectiveDate =
                local.Current.Date;
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              break;
            }

            // VALIDATE good cause CODES
            // 58691
            local.Code.CodeName = "CADS GOOD CAUSE";
            local.CodeValue.Cdvalue = export.Gc.Item.GcDetGoodCause.Code ?? Spaces
              (10);
            UseCabValidateCodeValue();

            if (AsChar(local.Common.Flag) == 'N')
            {
              var field1 = GetField(export.Gc.Item.GcDetGoodCause, "code");

              field1.Error = true;

              ExitState = "ACO_NE0000_INVALID_CODE";

              break;
            }

            // -----------------------------------------------
            // 04/07/99 W.Campbell - Added logic to insure
            // that if one or more APs exist on the case,
            // then one of them must be entered for GC.
            // If no APs exist on the case then, the user
            // must not enter one for good cause (GC).
            // -----------------------------------------------
            local.Grp.Count = 0;

            if (AsChar(import.Case1.Status) == 'C')
            {
              UseSiGetApsForCase();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                break;
              }

              if (IsEmpty(export.Gc.Item.GcDetApCsePersonsWorkSet.Number))
              {
                if (!local.Grp.IsEmpty)
                {
                  var field1 =
                    GetField(export.Gc.Item.GcDetApCsePersonsWorkSet, "number");
                    

                  field1.Error = true;

                  ExitState = "AP_REQUIRED_FOR_GOOD_CAUSE";

                  break;
                }
              }
              else if (local.Grp.IsEmpty)
              {
                var field1 =
                  GetField(export.Gc.Item.GcDetApCsePersonsWorkSet, "number");

                field1.Error = true;

                ExitState = "INVALID_GC_AP_SPECIFICATION";

                break;
              }
              else
              {
                for(local.Grp.Index = 0; local.Grp.Index < local.Grp.Count; ++
                  local.Grp.Index)
                {
                  if (!local.Grp.CheckSize())
                  {
                    break;
                  }

                  if (Equal(export.Gc.Item.GcDetApCsePersonsWorkSet.Number,
                    local.Grp.Item.Ap.Number))
                  {
                    // -----------------------------------------------
                    // The Good Cause AP is one of the AP(S)
                    // on the Case.  All is OK.
                    // -----------------------------------------------
                    goto Test3;
                  }
                }

                local.Grp.CheckIndex();
                ExitState = "GC_AP_MUST_BE_AP_ON_CASE";

                break;
              }
            }
            else
            {
              UseSiGetActiveApsForCase();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                break;
              }

              if (IsEmpty(export.Gc.Item.GcDetApCsePersonsWorkSet.Number))
              {
                if (!local.Grp.IsEmpty)
                {
                  var field1 =
                    GetField(export.Gc.Item.GcDetApCsePersonsWorkSet, "number");
                    

                  field1.Error = true;

                  ExitState = "AP_REQUIRED_FOR_GOOD_CAUSE";

                  break;
                }
              }
              else if (local.Grp.IsEmpty)
              {
                var field1 =
                  GetField(export.Gc.Item.GcDetApCsePersonsWorkSet, "number");

                field1.Error = true;

                ExitState = "INVALID_GC_AP_SPECIFICATION";

                break;
              }
              else
              {
                local.Grp.Index = 0;
                local.Grp.CheckSize();

                if (Equal(export.Gc.Item.GcDetApCsePersonsWorkSet.Number,
                  local.Grp.Item.Ap.Number))
                {
                  // -----------------------------------------------
                  // The Good Cause AP is one of the AP(S)
                  // on the Case.  All is OK.
                  // -----------------------------------------------
                }
                else if (Lt(export.Gc.Item.GcDetApCsePersonsWorkSet.Number,
                  local.Grp.Item.Ap.Number))
                {
                  var field1 =
                    GetField(export.Gc.Item.GcDetApCsePersonsWorkSet, "number");
                    

                  field1.Error = true;

                  ExitState = "GC_AP_MUST_BE_AP_ON_CASE";

                  return;
                }
                else
                {
                  while(Lt(local.Grp.Item.Ap.Number,
                    export.Gc.Item.GcDetApCsePersonsWorkSet.Number) && local
                    .Grp.Index < local.Grp.Count)
                  {
                    // -----------------------------------------------
                    // Search the group view of APs
                    // on this case looking for the
                    // Good Cause AP.  The APs in
                    // the group view MUST be in
                    // ASCENDING order.
                    // -----------------------------------------------
                    ++local.Grp.Index;
                    local.Grp.CheckSize();
                  }

                  if (Equal(export.Gc.Item.GcDetApCsePersonsWorkSet.Number,
                    local.Grp.Item.Ap.Number))
                  {
                    // -----------------------------------------------
                    // The Good Cause AP is one of the AP(S)
                    // on the Case.  All is OK.
                    // -----------------------------------------------
                  }
                  else
                  {
                    var field1 =
                      GetField(export.Gc.Item.GcDetApCsePersonsWorkSet, "number");
                      

                    field1.Error = true;

                    ExitState = "GC_AP_MUST_BE_AP_ON_CASE";

                    return;
                  }
                }
              }
            }

Test3:

            // -----------------------------------------------
            // 04/07/99 W.Campbell - End of logic added to insure
            // that if one or more APs exist on the case,
            // then one of them must be entered for GC.
            // If no APs exist on the case then, the user
            // must not enter one for good cause (GC).
            // -----------------------------------------------
            UseSiCadsAddCaseRoleDetail2();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.Gc.Update.GcDetGoodCause.EffectiveDate = local.Blank.Date;
              export.Gc.Update.GcDetGoodCause.Code = "";
              export.Gc.Update.GcDetApCsePersonsWorkSet.Number = "";
              export.Gc.Update.GcDetCommon.SelectChar = "";
            }
            else
            {
              UseEabRollbackCics();
            }
          }
          else
          {
          }
        }

        if (AsChar(local.Row1AddFuncNc.Flag) == 'Y')
        {
          export.Nc.Index = 0;
          export.Nc.CheckSize();

          if (AsChar(export.Nc.Item.NcDetCommon.SelectChar) == 'S')
          {
            if (IsEmpty(export.Nc.Item.NcDetNonCooperation.Code))
            {
              var field1 = GetField(export.Nc.Item.NcDetNonCooperation, "code");

              field1.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
            }

            if (IsEmpty(export.Nc.Item.NcDetNonCooperation.Reason) && AsChar
              (export.Nc.Item.NcDetNonCooperation.Code) == 'Y')
            {
              var field1 =
                GetField(export.Nc.Item.NcDetNonCooperation, "reason");

              field1.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
            }

            if (IsEmpty(export.Nc.Item.NcDetApCsePersonsWorkSet.Number))
            {
            }
            else
            {
              UseCabZeroFillNumber2();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                if (IsExitState("PERSON_NUMBER_NOT_NUMERIC"))
                {
                  var field1 =
                    GetField(export.Nc.Item.NcDetApCsePersonsWorkSet, "number");
                    

                  field1.Error = true;

                  break;
                }
              }
            }

            if (Lt(local.Blank.Date,
              export.Nc.Item.NcDetNonCooperation.EffectiveDate))
            {
              if (Lt(local.Current.Date,
                export.Nc.Item.NcDetNonCooperation.EffectiveDate))
              {
                var field1 =
                  GetField(export.Nc.Item.NcDetNonCooperation, "effectiveDate");
                  

                field1.Error = true;

                ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";

                break;
              }
            }
            else
            {
              export.Nc.Update.NcDetNonCooperation.EffectiveDate =
                local.Current.Date;
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              break;
            }

            // VALIDATE non coop CODES
            // 58691
            local.Code.CodeName = "CADS NON COOPERATION";
            local.CodeValue.Cdvalue =
              export.Nc.Item.NcDetNonCooperation.Code ?? Spaces(10);
            UseCabValidateCodeValue();

            if (AsChar(local.Common.Flag) == 'N')
            {
              var field1 = GetField(export.Nc.Item.NcDetNonCooperation, "code");

              field1.Error = true;

              ExitState = "ACO_NE0000_INVALID_CODE";

              break;
            }

            if (!IsEmpty(export.Nc.Item.NcDetNonCooperation.Reason))
            {
              local.Code.CodeName = "NON COOP REASON";
              local.CodeValue.Cdvalue =
                export.Nc.Item.NcDetNonCooperation.Reason ?? Spaces(10);
              UseCabValidateCodeValue();

              if (AsChar(local.Common.Flag) == 'N')
              {
                var field1 =
                  GetField(export.Nc.Item.NcDetNonCooperation, "reason");

                field1.Error = true;

                ExitState = "ACO_NE0000_INVALID_CODE";

                break;
              }
            }

            UseSiCadsAddCaseRoleDetail1();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.Nc.Update.NcDetCommon.SelectChar = "";
              export.Nc.Update.NcDetNonCooperation.EffectiveDate =
                local.Blank.Date;
              export.Nc.Update.NcDetNonCooperation.Code = "";
              export.Nc.Update.NcDetNonCooperation.Reason = "";
              export.Nc.Update.NcDetApCsePersonsWorkSet.Number = "";
              export.Nc.Update.NcDetCommon.SelectChar = "";
            }
            else
            {
              UseEabRollbackCics();

              break;
            }
          }
          else
          {
          }
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.HiddenRedisplay.Flag = "A";
          global.Command = "DISPLAY";
        }

        break;
      case "DISPLAY":
        // Display statements are in a seperate case at the end of the prad.
        break;
      case "GCPV":
        if (IsEmpty(export.GcMinus.OneChar) || export.Gc1.PageNumber == 1)
        {
          ExitState = "ACO_NE0000_INVALID_BACKWARD";

          break;
        }

        if (export.CurrItmNoGc.Count - (export.NoItemsGc.Count + 3) >= 0)
        {
          export.CurrItmNoGc.Count -= export.NoItemsGc.Count + 3;
        }
        else
        {
          ExitState = "ACO_NE0000_INVALID_BACKWARD";

          break;
        }

        --export.Gc1.PageNumber;
        UseSiCadsMoveNextGoodCause2();

        break;
      case "GCNX":
        if (IsEmpty(import.GcPlus.OneChar) || export.CurrItmNoGc.Count >= export
          .HidNoItemsFndGc.Count)
        {
          ExitState = "ACO_NE0000_INVALID_FORWARD";

          break;
        }

        for(export.Gc.Index = 0; export.Gc.Index < export.Gc.Count; ++
          export.Gc.Index)
        {
          if (!export.Gc.CheckSize())
          {
            break;
          }

          export.Gc.Update.GcDetApCsePersonsWorkSet.Number = "";
          MoveCaseRole(local.NullCaseRole, export.Gc.Update.GcDetApCaseRole);
          export.Gc.Update.GcDetGoodCause.Assign(local.NullGoodCause);
        }

        export.Gc.CheckIndex();
        ++export.Gc1.PageNumber;
        UseSiCadsMoveNextGoodCause1();

        break;
      case "NCPV":
        if (IsEmpty(export.NcMinus.OneChar) || export.Nc1.PageNumber == 1)
        {
          ExitState = "ACO_NE0000_INVALID_BACKWARD";

          break;
        }

        if (export.CurrItmNoNc.Count - (export.NoItemsNc.Count + 3) >= 0)
        {
          export.CurrItmNoNc.Count -= export.NoItemsNc.Count + 3;
        }
        else
        {
          ExitState = "ACO_NE0000_INVALID_BACKWARD";

          break;
        }

        --export.Nc1.PageNumber;
        UseSiCadsMoveNextNonCoops();

        break;
      case "NCNX":
        if (IsEmpty(export.NcPlus.OneChar) || export.CurrItmNoNc.Count >= export
          .HidNoItemsFndNc.Count)
        {
          ExitState = "ACO_NE0000_INVALID_FORWARD";

          break;
        }

        ++export.Nc1.PageNumber;

        for(export.Nc.Index = 0; export.Nc.Index < export.Nc.Count; ++
          export.Nc.Index)
        {
          if (!export.Nc.CheckSize())
          {
            break;
          }

          export.Nc.Update.NcDetApCsePersonsWorkSet.Number = "";
          MoveCaseRole(local.NullCaseRole, export.Nc.Update.NcDetApCaseRole);
          export.Nc.Update.NcDetNonCooperation.Assign(local.NullNonCooperation);
        }

        export.Nc.CheckIndex();
        UseSiCadsMoveNextNonCoops();

        break;
      case "UPDATE":
        // 11/04/99 M.L Start
        if (AsChar(export.SuccessfullyDisplayed.Flag) != 'Y')
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";

          break;
        }

        // 11/04/99 M.L End
        local.DataChanged.Flag = "N";

        if (!Equal(import.Ap.Number, import.HiddenPrev.Number))
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";

          break;
        }

        export.Gc.Index = 0;
        export.Gc.CheckSize();

        export.Nc.Index = 0;
        export.Nc.CheckSize();

        if (!IsEmpty(export.Gc.Item.GcDetCommon.SelectChar))
        {
          var field1 = GetField(export.Gc.Item.GcDetCommon, "selectChar");

          field1.Error = true;

          ExitState = "REQUEST_CANNOT_BE_EXECUTED";
        }

        if (!IsEmpty(export.Nc.Item.NcDetCommon.SelectChar))
        {
          var field1 = GetField(export.Nc.Item.NcDetCommon, "selectChar");

          field1.Error = true;

          ExitState = "REQUEST_CANNOT_BE_EXECUTED";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        // ---------------------------------------------
        // Non-data base validation
        // If no input field has change no data base update required.
        // ---------------------------------------------
        // Update Case Entity data if data changed
        if (!IsEmpty(export.Case1.ExpeditedPaternityInd))
        {
          if (AsChar(export.Case1.ExpeditedPaternityInd) != 'Y' && AsChar
            (export.Case1.ExpeditedPaternityInd) != 'N')
          {
            var field1 = GetField(export.Case1, "expeditedPaternityInd");

            field1.Error = true;

            ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";
          }
        }

        // -------------------------------------------------------------------
        // 07/18/00 W.Campbell - Added the following
        // code to replaced the above code with different logic.
        // Work done on PR#98232.
        // -------------------------------------------------------------------
        if (AsChar(export.Case1.DuplicateCaseIndicator) == AsChar
          (export.Prev.DuplicateCaseIndicator))
        {
          // -------------------------------------------------------------------
          // 07/18/00 W.Campbell - Duplicate case Indicator.
          // has not changed.  Just keep on going.
          // -------------------------------------------------------------------
        }
        else
        {
          // -------------------------------------------------------------------
          // 07/18/00 W.Campbell - Duplicate case Indicator
          // has changed.  Must Check to see if the new value is 'Y'. 
          // -------------------------------------------------------------------
          if (AsChar(export.Case1.DuplicateCaseIndicator) == 'Y')
          {
            // -------------------------------------------------------------------
            // 07/18/00 W.Campbell - Duplicate case Indicator
            // is being changed and the new value is'Y'.
            // Must insure that the Case is an Incomming
            // Interstate Case.
            // -------------------------------------------------------------------
            local.IncommingInterstateFound.Flag = "N";

            for(export.Interstate.Index = 0; export.Interstate.Index < export
              .Interstate.Count; ++export.Interstate.Index)
            {
              // @@@
              if (CharAt(export.Interstate.Item.Interstate1.Text8,
                Length(TrimEnd(export.Interstate.Item.Interstate1.Text8))) == 'I'
                )
              {
                local.IncommingInterstateFound.Flag = "Y";

                break;
              }
            }

            if (AsChar(local.IncommingInterstateFound.Flag) != 'Y')
            {
              var field1 = GetField(export.Case1, "duplicateCaseIndicator");

              field1.Error = true;

              ExitState = "INVALID_INTERSTATE_INDICATOR";

              goto Test4;
            }
          }

          local.DataChanged.Flag = "C";
        }

Test4:

        // -------------------------------------------------------------------
        // 07/18/00 W.Campbell - End of Added code
        // to replaced the above code with different logic.
        // Work done on PR#98232.
        // -------------------------------------------------------------------
        // ---------------------------------------------------------------
        // 10/26/98  Added code to check that indicators are Y or spaces.
        // ---------------------------------------------------------------
        if (!IsEmpty(export.Case1.LocateInd))
        {
          if (AsChar(export.Case1.LocateInd) != 'Y')
          {
            var field1 = GetField(export.Case1, "locateInd");

            field1.Error = true;

            ExitState = "INVALID_INDICATOR_Y_OR_SPACE";
          }
        }

        if (!IsEmpty(export.Case1.FullServiceWithMedInd))
        {
          if (AsChar(export.Case1.FullServiceWithMedInd) != 'Y')
          {
            var field1 = GetField(export.Case1, "fullServiceWithMedInd");

            field1.Error = true;

            ExitState = "INVALID_INDICATOR_Y_OR_SPACE";
          }
        }

        if (!IsEmpty(export.Case1.FullServiceWithoutMedInd))
        {
          if (AsChar(export.Case1.FullServiceWithoutMedInd) != 'Y')
          {
            var field1 = GetField(export.Case1, "fullServiceWithoutMedInd");

            field1.Error = true;

            ExitState = "INVALID_INDICATOR_Y_OR_SPACE";
          }
        }

        // -------------------------------------------------------------------
        // 03/04/99 W.Campbell -  Added code to validate
        // that one service type is designated with 'Y'.
        // -------------------------------------------------------------------
        if (IsEmpty(import.Case1.LocateInd) && IsEmpty
          (import.Case1.FullServiceWithMedInd) && IsEmpty
          (import.Case1.FullServiceWithoutMedInd))
        {
          var field1 = GetField(export.Case1, "locateInd");

          field1.Error = true;

          var field2 = GetField(export.Case1, "fullServiceWithMedInd");

          field2.Error = true;

          var field3 = GetField(export.Case1, "fullServiceWithoutMedInd");

          field3.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "CASE_MUST_HAVE_SERV_TYPE";
          }
        }

        // -------------------------------------------------------------------
        // 10/26/98  Added code to check that indicators are mutually exclusive.
        // -------------------------------------------------------------------
        if (AsChar(import.Case1.LocateInd) == 'Y' && (
          AsChar(import.Case1.FullServiceWithMedInd) == 'Y' || AsChar
          (import.Case1.FullServiceWithoutMedInd) == 'Y'))
        {
          var field1 = GetField(export.Case1, "locateInd");

          field1.Error = true;

          var field2 = GetField(export.Case1, "fullServiceWithMedInd");

          field2.Error = true;

          var field3 = GetField(export.Case1, "fullServiceWithoutMedInd");

          field3.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "INVALID_MUTUALLY_EXCLUSIVE_FIELD";
          }
        }
        else if (AsChar(import.Case1.FullServiceWithMedInd) == 'Y' && AsChar
          (import.Case1.FullServiceWithoutMedInd) == 'Y')
        {
          var field1 = GetField(export.Case1, "fullServiceWithMedInd");

          field1.Error = true;

          var field2 = GetField(export.Case1, "fullServiceWithoutMedInd");

          field2.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "INVALID_MUTUALLY_EXCLUSIVE_FIELD";
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // ---------------------------------------------
        // Access tables for validations
        // --------------------------------------------
        if (!Equal(export.Case1.PaMedicalService, export.Prev.PaMedicalService))
        {
          if (!IsEmpty(export.Case1.PaMedicalService))
          {
            // *******************************************
            // VALIDATE PA MEDICAL SERVICE CODE
            // *******************************************
            local.Code.CodeName = "PA MED SERVICE";
            local.CodeValue.Cdvalue = export.Case1.PaMedicalService ?? Spaces
              (10);
            UseCabValidateCodeValue();

            if (AsChar(local.Common.Flag) == 'N')
            {
              var field1 = GetField(export.Case1, "paMedicalService");

              field1.Error = true;

              ExitState = "ACO_NE0000_INVALID_CODE";

              goto Test5;
            }
          }

          local.DataChanged.Flag = "C";
        }

Test5:

        // *mxk
        // *******************************************************
        //     Do we need the check below  ...
        //     With the enhancement we can change the  closure letter
        //     date and error reason code hence the lines below have
        //     been commented out .
        // ***********************************************************
        if (!Equal(export.Case1.ClosureLetterDate, export.Prev.ClosureLetterDate))
          
        {
          if (Lt(local.Blank.Date, export.Case1.ClosureLetterDate))
          {
            // -----------------------------------------------------------
            // 10/15/99 W.Campbell - Logic added to not
            // allow closure letter date if the AR is an
            // Organization ( last char of CSE PERSON
            // number = 'O' (alpha O)).  Work done
            // on PR#H00077426.
            // -----------------------------------------------------------
            if (CharAt(export.ArCsePersonsWorkSet.Number, 10) == 'O')
            {
              // ----------------------------------
              // IF the AR is an Organization, then
              // closure letter date not needed.
              // ----------------------------------
              var field1 = GetField(export.Case1, "closureLetterDate");

              field1.Error = true;

              ExitState = "NO_CLOSE_LTR_DATE_FOR_AR_IS_ORG";

              break;
            }

            // --------------------------------------------------------------------------------------------------------
            // CQ67478: Added code to closure date is not required for closure 
            // reason codes IC,IL,LO & GC.
            // --------------------------------------------------------------------------------------------------------
            // --------------------------------------------------------------------------------------------------------
            // CQ68909: Closure Letter Date is not required for closure reason 
            // code BG.
            // --------------------------------------------------------------------------------------------------------
            if (Equal(export.Case1.ClosureReason, "GC") || Equal
              (export.Case1.ClosureReason, "IC") || Equal
              (export.Case1.ClosureReason, "IN") || Equal
              (export.Case1.ClosureReason, "LO") || Equal
              (export.Case1.ClosureReason, "TR") || Equal
              (export.Case1.ClosureReason, "BG"))
            {
              var field1 = GetField(export.Case1, "closureLetterDate");

              field1.Error = true;

              ExitState = "SI0000_CLOSURE_LTR_NR_SEL_REASON";

              break;
            }

            // *****mxk
            // **********************************************
            //   The date entered in the 60 day letter field
            //   must be current date as required in the enhancement.
            // ******************************************************
            if (!Equal(export.Case1.ClosureLetterDate, local.Current.Date))
            {
              var field1 = GetField(export.Case1, "closureLetterDate");

              field1.Error = true;

              ExitState = "ACO_NE0000_DT_MUST_B_CURRENT_DT";

              break;
            }

            // *****mxk
            // **********************************************
            //      A closure reason will be required when a date is
            // entered in the 60 day closure letter date field .
            // ******************************************************
            if (IsEmpty(export.Case1.ClosureReason))
            {
              var field1 = GetField(export.Case1, "closureReason");

              field1.Error = true;

              ExitState = "SI0000_CASE_CLOSURE_RSN_REQUIRED";

              break;
            }

            // *** Problem report I00115283
            // *** 03/26/01 swsrchf
            // *** start
            if (ReadCsePerson())
            {
              if (AsChar(entities.KeyOnly.Type1) == 'C')
              {
                // --------------------------------------------------------------------------------------------------------
                // CQ67478: Added code to skip closure letter not requried for 
                // closure reason codes GC,IC,IL,LO & TR.
                // --------------------------------------------------------------------------------------------------------
                // --------------------------------------------------------------------------------------------------------
                // CQ68909: Intent to Close Letter requirement is removed for 
                // closure reason code BG. BG Reason code added
                //          to the List.
                // --------------------------------------------------------------------------------------------------------
                if (Equal(export.Case1.ClosureReason, "GC") || Equal
                  (export.Case1.ClosureReason, "IC") || Equal
                  (export.Case1.ClosureReason, "IN") || Equal
                  (export.Case1.ClosureReason, "LO") || Equal
                  (export.Case1.ClosureReason, "TR"))
                {
                  goto Test6;
                }

                local.Document.Name = "ARCLOS60";
              }
            }
            else
            {
              ExitState = "CSE_PERSON_NF";

              break;
            }

            // *** end
            // *** 03/26/01 swsrchf
            // *** Problem request I00115283
          }

Test6:

          local.DataChanged.Flag = "C";
        }

        if (!Equal(export.Case1.StatusDate, export.Prev.StatusDate))
        {
          if (Lt(local.Blank.Date, export.Case1.StatusDate))
          {
            if (Lt(export.Case1.StatusDate, export.Case1.CseOpenDate))
            {
              var field1 = GetField(export.Case1, "statusDate");

              field1.Error = true;

              var field2 = GetField(export.Case1, "cseOpenDate");

              field2.Error = true;

              ExitState = "SI0000_CASE_CLOSE_BEFORE_OPEN";

              break;
            }

            if (Lt(local.Current.Date, export.Case1.StatusDate))
            {
              var field1 = GetField(export.Case1, "statusDate");

              field1.Error = true;

              ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";

              break;
            }

            if (Lt(export.Case1.StatusDate, local.Current.Date))
            {
              var field1 = GetField(export.Case1, "statusDate");

              field1.Error = true;

              ExitState = "ACO_NE0000_DATE_LESS_CURRENT_DT";

              break;
            }

            if (IsEmpty(export.Case1.ClosureReason))
            {
              var field1 = GetField(export.Case1, "closureReason");

              field1.Error = true;

              ExitState = "SI0000_CASE_CLOSURE_RSN_REQUIRED";

              break;
            }

            // -----------------------------------------------------------
            // 07/07/00 W.Campbell - Logic added to not
            // allow the Closure date (status date) to be the
            // same as the Closure letter date.
            // The Closure date must be > the
            // Closure letter date.
            // Work done on PR#98503.
            // -----------------------------------------------------------
            // 01/25/02 M.L start
            if (!Lt(export.Case1.ClosureLetterDate, export.Case1.StatusDate))
            {
              var field1 = GetField(export.Case1, "statusDate");

              field1.Error = true;

              var field2 = GetField(export.Case1, "closureLetterDate");

              field2.Error = true;

              ExitState = "SI0000_LTR_DT_MUST_BE_LT_CLS_DT";

              break;
            }

            if (Equal(export.Case1.StatusDate, export.Case1.ClosureLetterDate) &&
              Equal(export.Prev.ClosureLetterDate, local.Current.Date))
            {
              var field1 = GetField(export.Case1, "statusDate");

              field1.Error = true;

              var field2 = GetField(export.Case1, "closureLetterDate");

              field2.Error = true;

              ExitState = "SI0000_LTR_DT_MUST_BE_LT_CLS_DT";

              break;
            }

            // 01/25/02 M.L end
            // 01/25/02 M.L Start
            if (Lt(local.Blank.Date, export.Case1.ClosureLetterDate))
            {
              // --------------------------------------------------------------------------------------------------------
              // CQ68909: Closure Letter Date is not required for closure reason
              // code BG.
              // --------------------------------------------------------------------------------------------------------
              if (Equal(export.Case1.ClosureReason, "GC") || Equal
                (export.Case1.ClosureReason, "IC") || Equal
                (export.Case1.ClosureReason, "IN") || Equal
                (export.Case1.ClosureReason, "LO") || Equal
                (export.Case1.ClosureReason, "TR") || Equal
                (export.Case1.ClosureReason, "BG"))
              {
                local.Lt60Days.Flag = "N";

                goto Test7;
              }

              local.DateWorkArea.Date =
                AddDays(export.Case1.ClosureLetterDate, 60);

              if (Lt(export.Case1.StatusDate, local.DateWorkArea.Date))
              {
                local.Lt60Days.Flag = "Y";
              }
              else
              {
                local.Lt60Days.Flag = "N";
              }
            }

Test7:

            // 01/25/02 M.L End
            local.DataChanged.Flag = "C";
          }
        }

        // 01/26/02 M.L Start
        // Modified the next IF from
        // IF export case closure_reason <> export_prev case closure_reason
        // to
        // IF (export case closure_reason <> export_prev case closure_reason) OR
        // (export case status_date <> export_prev case status_date)
        // This change was made on Saturday Jan 26, 2002.
        // 01/26/02 M.L End
        if (!Equal(export.Case1.ClosureReason, export.Prev.ClosureReason) || !
          Equal(export.Case1.StatusDate, export.Prev.StatusDate))
        {
          // 03/19/2002 M.Lachowicz Start
          if (IsEmpty(export.Case1.ClosureReason) && Equal
            (export.Case1.ClosureLetterDate, local.Blank.Date) && Equal
            (export.Case1.StatusDate, local.Blank.Date))
          {
            local.Document.Name = "";
            local.Lt60Days.Flag = "N";
            local.DataChanged.Flag = "C";

            goto Test11;
          }

          // 03/19/2002 M.Lachowicz End
          if (!IsEmpty(export.Case1.ClosureReason))
          {
            // *******************************************
            // VALIDATE CASE CLOSURE REASON CODE
            // *******************************************
            local.Code.CodeName = "CASE CLOSURE REASON";
            local.CodeValue.Cdvalue = export.Case1.ClosureReason ?? Spaces(10);
            UseCabValidateCodeValue();

            if (AsChar(local.Common.Flag) == 'N')
            {
              var field1 = GetField(export.Case1, "closureReason");

              field1.Error = true;

              ExitState = "ACO_NE0000_INVALID_CODE";

              goto Test11;
            }
          }

          // 01/25/02 M.L Start
          if (Equal(export.Prev.ClosureLetterDate, local.Current.Date))
          {
            // --------------------------------------------------------------------------------------------------------
            // CQ67478: Added code to skip the below listed validation for 
            // reason code GC.
            // --------------------------------------------------------------------------------------------------------
            // --------------------------------------------------------------------------------------------------------
            // CQ68909: Closure Letter Date is not required for closure reason 
            // code BG.
            // --------------------------------------------------------------------------------------------------------
            if (Equal(export.Case1.ClosureReason, "GC") || Equal
              (export.Case1.ClosureReason, "IC") || Equal
              (export.Case1.ClosureReason, "IN") || Equal
              (export.Case1.ClosureReason, "LO") || Equal
              (export.Case1.ClosureReason, "TR") || Equal
              (export.Case1.ClosureReason, "BG"))
            {
              goto Test8;
            }

            var field1 = GetField(export.Case1, "closureReason");

            field1.Error = true;

            ExitState = "SI0000_ONE_CLOSURE_LETTER_ON_DAY";

            break;
          }

Test8:

          // 01/25/02 M.L End
          // 02/05/02 M.L Start
          if (!Equal(export.Case1.ClosureReason, export.Prev.ClosureReason))
          {
            if (!Equal(export.Case1.ClosureLetterDate, local.Current.Date))
            {
              // 02/13/2002 M. Lachowicz Start
              if (!Equal(export.Prev.ClosureLetterDate, local.Blank.Date) && IsEmpty
                (export.Prev.ClosureReason) && Lt
                (local.Blank.Date, export.Case1.StatusDate))
              {
                goto Test9;
              }

              // 02/13/2002 M. Lachowicz End
              // --------------------------------------------------------------------------------------------------------
              // CQ67478: Added Reason codes GC,IC,IN,LO & TR in addition to AR 
              // & RO
              // --------------------------------------------------------------------------------------------------------
              // --------------------------------------------------------------------------------------------------------
              // CQ68909: Closure Letter Date is not required for closure reason
              // code BG.
              // --------------------------------------------------------------------------------------------------------
              if (Equal(export.Case1.ClosureReason, "AR") || Equal
                (export.Case1.ClosureReason, "GC") || Equal
                (export.Case1.ClosureReason, "IC") || Equal
                (export.Case1.ClosureReason, "IN") || Equal
                (export.Case1.ClosureReason, "LO") || Equal
                (export.Case1.ClosureReason, "RO") || Equal
                (export.Case1.ClosureReason, "TR") || Equal
                (export.Case1.ClosureReason, "BG"))
              {
                if (Lt(local.Blank.Date, export.Case1.ClosureLetterDate))
                {
                  local.ClosureDatePlus60Days.Date =
                    AddDays(export.Case1.ClosureLetterDate, 60);

                  if (Lt(local.ClosureDatePlus60Days.Date,
                    export.Case1.StatusDate))
                  {
                    goto Test9;
                  }
                }
                else
                {
                  // 02/15/2002 M. Lachowicz Start
                  if (Equal(export.Case1.StatusDate, local.Blank.Date))
                  {
                    var field2 = GetField(export.Case1, "statusDate");

                    field2.Error = true;

                    ExitState = "SI0000_CLOSURE_DATE_REQUIRED";

                    break;
                  }

                  // 02/15/2002 M. Lachowicz End
                  goto Test9;
                }
              }

              if (Equal(export.Case1.ClosureLetterDate, local.Blank.Date))
              {
                // 02/15/2002 M. Lachowicz Start
                // --------------------------------------------------------------------------------------------------------
                // CQ67478: 60 day closure/intent to close letter will not be 
                // sent for reason codes GC, IC,IN,LO,TR & BG..
                // --------------------------------------------------------------------------------------------------------
                // --------------------------------------------------------------------------------------------------------
                // CQ68909: Closure Letter Date & Letter is not required for 
                // closure reason code BG. Added in addition to
                //          Organization check.
                // --------------------------------------------------------------------------------------------------------
                if (CharAt(export.ArCsePersonsWorkSet.Number, 10) == 'O' || Equal
                  (export.Case1.ClosureReason, "BG"))
                {
                  // ----------------------------------
                  // IF the AR is an Organization, then
                  // closure letter date not needed.
                  // ----------------------------------
                  if (Equal(export.Case1.StatusDate, local.Blank.Date))
                  {
                    var field3 = GetField(export.Case1, "statusDate");

                    field3.Error = true;

                    ExitState = "SI0000_CLOSURE_DATE_REQUIRED";

                    break;
                  }

                  local.SendClosureLtrReqd.Flag = "N";

                  goto Test9;
                }

                // 02/15/2002 M. Lachowicz End
                var field2 = GetField(export.Case1, "closureLetterDate");

                field2.Error = true;

                ExitState = "SI0000_CLOSURE_LTR_NOTICE_REQD";

                break;
              }

              var field1 = GetField(export.Case1, "closureLetterDate");

              field1.Error = true;

              ExitState = "SI0000_RSN_CHGD_ENTR_CURRENT_DT";

              break;
            }
          }

Test9:

          // 02/05/02 M.L End
          // -----------------------------------------------------------
          // 06/03/99 W.Campbell - Logic added to
          // handle case closure reason = EM & 4D the
          // same as NP in the IF statement below.
          // -----------------------------------------------------------
          // -------------------------------------------------------------------------------------------------------
          // If the closure reason field is  AR or RO then we do not send a 60 
          // day closure letter.
          // CQ67478: Added Reason codes GC, IC,IN, LO & TR to the list.
          // CQ68909: Closure reason code BG added to the list.
          // --------------------------------------------------------------------------------------------------------
          if (Equal(export.Case1.ClosureReason, "AR") || Equal
            (export.Case1.ClosureReason, "GC") || Equal
            (export.Case1.ClosureReason, "IC") || Equal
            (export.Case1.ClosureReason, "IN") || Equal
            (export.Case1.ClosureReason, "LO") || Equal
            (export.Case1.ClosureReason, "RO") || Equal
            (export.Case1.ClosureReason, "TR") || Equal
            (export.Case1.ClosureReason, "BG"))
          {
            local.Lt60Days.Flag = "N";
            local.SendClosureLtrReqd.Flag = "Y";

            // --------------------------------------------------------------------------------------------------------
            // CQ68909: Closure Letter NOT required for closure reason code BG. 
            // Added to the below list.
            // --------------------------------------------------------------------------------------------------------
            if (Equal(export.Case1.ClosureReason, "AR") || Equal
              (export.Case1.ClosureReason, "RO") || Equal
              (export.Case1.ClosureReason, "BG"))
            {
              local.SendClosureLtrReqd.Flag = "N";
            }
          }
          else
          {
            if (Lt(local.Blank.Date, export.Case1.ClosureLetterDate))
            {
              // 01/25/02 M.L Start
              // 01/25/02 M.L End
            }
            else
            {
              // -----------------------------------------------------------
              // 10/15/99 W.Campbell - Logic added to not
              // require closure letter date if the AR is an
              // Organization ( last char of CSE PERSON
              // number = 'O' (alpha O)).  Work done
              // on PR#H00077426.
              // -----------------------------------------------------------
              if (CharAt(export.ArCsePersonsWorkSet.Number, 10) == 'O')
              {
                // ----------------------------------
                // IF the AR is an Organization, then
                // closure letter date not needed.
                // ----------------------------------
                local.SendClosureLtrReqd.Flag = "N";

                goto Test10;
              }
              else
              {
                var field1 = GetField(export.Case1, "statusDate");

                field1.Error = true;

                var field2 = GetField(export.Case1, "closureReason");

                field2.Error = true;

                ExitState = "SI0000_CLOSURE_LTR_NOTICE_REQD";

                break;
              }
            }

            local.SendClosureLtrReqd.Flag = "Y";
          }

Test10:

          // 01/25/02 M.L Start
          if (Equal(export.Case1.StatusDate, local.Blank.Date))
          {
            local.DataChanged.Flag = "C";
            export.Case1.Status = "O";

            goto Test11;
          }

          // 01/25/02 M.L End
          // 12/18/01 M.Lachowicz Start
          if (IsEmpty(export.Ap.Number))
          {
            export.Case1.Status = "C";
            local.DataChanged.Flag = "C";

            goto Test11;
          }

          // 12/18/01 M.Lachowicz End
          // *** Work request 000240
          // *** 03/26/01 swsrchf
          // *** start
          UseSiCadsDetermineCaseClosure();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            break;
          }

          // *** end
          // *** 03/26/01 swsrchf
          // *** Work request 000240
          export.Case1.Status = "C";
          local.DataChanged.Flag = "C";
        }

Test11:

        // @@@
        if (!Equal(export.Case1.NoJurisdictionCd, export.Prev.NoJurisdictionCd))
        {
          if (!IsEmpty(export.Case1.NoJurisdictionCd))
          {
            // *******************************************
            // VALIDATE NO JURISDICTION CODE
            // *******************************************
            local.Code.CodeName = "NO JURISDICTION";
            local.CodeValue.Cdvalue = export.Case1.NoJurisdictionCd ?? Spaces
              (10);
            UseCabValidateCodeValue();

            if (AsChar(local.Common.Flag) == 'N')
            {
              var field1 = GetField(export.Case1, "noJurisdictionCd");

              field1.Error = true;

              ExitState = "ACO_NE0000_INVALID_CODE";

              goto Test12;
            }
          }

          local.DataChanged.Flag = "C";
        }

Test12:

        if (AsChar(export.Case1.ExpeditedPaternityInd) != AsChar
          (export.Prev.ExpeditedPaternityInd) || !
          Equal(export.Case1.Note, export.Prev.Note))
        {
          local.DataChanged.Flag = "C";
        }

        // -------------------------------------------------------------------
        // 10/14/98  Added this IF statement to see if any updates have been
        //  made to the service type.
        // -------------------------------------------------------------------
        if (AsChar(export.Case1.LocateInd) != AsChar(export.Prev.LocateInd) || AsChar
          (export.Case1.FullServiceWithMedInd) != AsChar
          (export.Prev.FullServiceWithMedInd) || AsChar
          (export.Case1.FullServiceWithoutMedInd) != AsChar
          (export.Prev.FullServiceWithoutMedInd))
        {
          local.DataChanged.Flag = "C";
        }

        // ***	Update Case Role data if data changed
        if (!Equal(export.ArCaseRole.AssignmentTerminatedDt,
          export.ArPrev.AssignmentTerminatedDt) || !
          Equal(export.ArCaseRole.AssignmentTerminationCode,
          export.ArPrev.AssignmentTerminationCode))
        {
          if (!IsEmpty(export.ArCaseRole.AssignmentTerminationCode))
          {
            // VALIDATE TERMINATION REASON FOR CASE
            local.Code.CodeName = "CASE TERMINATION REASON";
            local.CodeValue.Cdvalue =
              export.ArCaseRole.AssignmentTerminationCode ?? Spaces(10);
            UseCabValidateCodeValue();

            if (AsChar(local.Common.Flag) == 'N')
            {
              var field1 =
                GetField(export.ArCaseRole, "assignmentTerminationCode");

              field1.Error = true;

              ExitState = "ACO_NE0000_INVALID_CODE";
            }
            else if (!Lt(local.Blank.Date,
              export.ArCaseRole.AssignmentTerminatedDt))
            {
              export.ArCaseRole.AssignmentTerminatedDt = local.Current.Date;
            }
          }

          // If flag is C  "CASE" data is to be updated.
          // If flag is R  "CASE ROLE" AR data is to be updated.
          // If Flag is B  both entities to be updated.
          if (AsChar(local.DataChanged.Flag) == 'C')
          {
            local.DataChanged.Flag = "B";
          }
          else
          {
            local.DataChanged.Flag = "R";
          }
        }

        // ***	Validate ap data if selection was made
        local.GrpGcCnt.Count = 0;
        export.Gc.Index = 1;

        for(var limit = export.Gc.Count; export.Gc.Index < limit; ++
          export.Gc.Index)
        {
          if (!export.Gc.CheckSize())
          {
            break;
          }

          if (AsChar(export.Gc.Item.GcDetCommon.SelectChar) == 'S')
          {
            if (IsEmpty(export.Gc.Item.GcDetGoodCause.Code))
            {
              export.HiddenRedisplay.Flag = "Y";
              export.Gc.Update.GcDetGoodCause.EffectiveDate = local.Blank.Date;
            }
            else
            {
              // VALIDATE good cause CODES
              // 58691
              local.Code.CodeName = "CADS GOOD CAUSE";
              local.CodeValue.Cdvalue = export.Gc.Item.GcDetGoodCause.Code ?? Spaces
                (10);
              UseCabValidateCodeValue();
            }

            if (AsChar(local.Common.Flag) == 'N')
            {
              var field1 = GetField(export.Gc.Item.GcDetGoodCause, "code");

              field1.Error = true;

              ExitState = "ACO_NE0000_INVALID_CODE";
            }
            else
            {
              if (Lt(local.Blank.Date,
                export.Gc.Item.GcDetGoodCause.EffectiveDate))
              {
                // 08/16/99 M.L Start
                if (!Equal(export.Gc.Item.GcDetGoodCause.EffectiveDate,
                  local.Current.Date))
                {
                  var field1 =
                    GetField(export.Gc.Item.GcDetGoodCause, "effectiveDate");

                  field1.Error = true;

                  ExitState = "ACO_NE0000_DT_MUST_B_CURRENT_DT";

                  goto Test16;
                }

                // 08/16/99 M.L End
              }
              else
              {
                export.Gc.Update.GcDetGoodCause.EffectiveDate =
                  local.Current.Date;
              }

              ++local.GrpGcCnt.Count;
            }
          }
          else
          {
          }
        }

        export.Gc.CheckIndex();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        local.GrpNcCnt.Count = 0;
        export.Nc.Index = 1;

        for(var limit = export.Nc.Count; export.Nc.Index < limit; ++
          export.Nc.Index)
        {
          if (!export.Nc.CheckSize())
          {
            break;
          }

          if (AsChar(export.Nc.Item.NcDetCommon.SelectChar) == 'S')
          {
            // VALIDATE non coop cause CODES
            if (IsEmpty(export.Nc.Item.NcDetNonCooperation.Code))
            {
              var field1 = GetField(export.Nc.Item.NcDetNonCooperation, "code");

              field1.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
            }
            else
            {
              // 58691
              local.Code.CodeName = "CADS NON COOPERATION";
              local.CodeValue.Cdvalue =
                export.Nc.Item.NcDetNonCooperation.Code ?? Spaces(10);
              UseCabValidateCodeValue();
            }

            if (AsChar(local.Common.Flag) == 'N')
            {
              var field1 = GetField(export.Nc.Item.NcDetNonCooperation, "code");

              field1.Error = true;

              ExitState = "ACO_NE0000_INVALID_CODE";
            }
            else
            {
              if (IsEmpty(export.Nc.Item.NcDetNonCooperation.Reason))
              {
                export.Nc.Update.NcDetNonCooperation.EffectiveDate =
                  local.Blank.Date;
                export.HiddenRedisplay.Flag = "Y";
              }
              else
              {
                local.Code.CodeName = "NON COOP REASON";
                local.CodeValue.Cdvalue =
                  export.Nc.Item.NcDetNonCooperation.Reason ?? Spaces(10);
                UseCabValidateCodeValue();
              }

              if (AsChar(local.Common.Flag) == 'N')
              {
                var field1 =
                  GetField(export.Nc.Item.NcDetNonCooperation, "reason");

                field1.Error = true;

                ExitState = "ACO_NE0000_INVALID_CODE";

                goto Test13;
              }

              if (Lt(local.Blank.Date,
                export.Nc.Item.NcDetNonCooperation.EffectiveDate))
              {
                if (Lt(local.Current.Date,
                  export.Nc.Item.NcDetNonCooperation.EffectiveDate))
                {
                  var field1 =
                    GetField(export.Nc.Item.NcDetNonCooperation, "effectiveDate");
                    

                  field1.Error = true;

                  ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";

                  goto Test16;
                }
              }
              else
              {
                export.Nc.Update.NcDetNonCooperation.EffectiveDate =
                  local.Current.Date;
              }

              // this was disabled for cq58691
              ++local.GrpNcCnt.Count;
            }
          }
          else
          {
          }

Test13:
          ;
        }

        export.Nc.CheckIndex();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        if (AsChar(local.DataChanged.Flag) == 'N' && local.GrpGcCnt.Count <= 0
          && local.GrpNcCnt.Count <= 0)
        {
          ExitState = "ACO_NI0000_DATA_WAS_NOT_CHANGED";

          break;
        }

        if (!Equal(export.Case1.ClosureReason, export.Prev.ClosureReason) || !
          Equal(export.Case1.StatusDate, export.Prev.StatusDate))
        {
          if (!Equal(export.Case1.ClosureReason, export.HiddenCase.ClosureReason)
            || !Equal(export.Case1.StatusDate, export.HiddenCase.StatusDate))
          {
            if (AsChar(local.Lt60Days.Flag) == 'Y')
            {
              MoveCase3(export.Case1, export.HiddenCase);
              ExitState = "SI0000_CLOSURE_LTR_SENT_LT_60_DY";

              break;
            }
          }
        }

        // -----------------------------------------------------
        // 03/04/00 W.Campbell - Added local view for
        // expedited paternity and logic to use that to
        // determine if Expedited Paternity is being
        // updated to a value of "Y".  If it is, then
        // display a modified Update successful exit
        // state msg to the user which reminds them
        // to validate the other paternity information
        // on this case.  This work was done on WR# 000160
        // for PRWORA - Paternity modifications.
        // -----------------------------------------------------
        if (AsChar(export.Case1.ExpeditedPaternityInd) != AsChar
          (export.Prev.ExpeditedPaternityInd))
        {
          if (AsChar(export.Case1.ExpeditedPaternityInd) == 'Y')
          {
            local.Case1.ExpeditedPaternityInd = "Y";
          }
        }

        // -----------------------------------------------------
        // 03/04/00 W.Campbell - End of code added.
        // -----------------------------------------------------
        // 11/09/99 M.L Start
        if (Equal(export.Case1.StatusDate, local.Blank.Date))
        {
          // 12/21/01 M.L  WR240 Start
          if ((Equal(export.Case1.ClosureReason, "AR") || Equal
            (export.Case1.ClosureReason, "RO") || Equal
            (export.Case1.ClosureReason, "BG") || Equal
            (export.Case1.ClosureReason, "GC")) && AsChar
            (export.Case1.Status) == 'C')
          {
            export.Case1.StatusDate = local.Current.Date;

            goto Test14;
          }

          // 12/21/01 M.L  WR240 End
          // 12/21/01 M.L  WR240 Start
          if (AsChar(export.Case1.Status) == 'C')
          {
            export.Case1.StatusDate = local.Current.Date;

            goto Test14;
          }

          // 12/21/01 M.L  WR240 End
          export.Case1.StatusDate = export.Original.StatusDate;
        }

Test14:

        // 11/09/99 M.L End
        UseSiCadsUpdateCaseDetails();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field1 = GetField(export.Case1, "number");

          field1.Error = true;

          UseEabRollbackCics();

          break;
        }

        // 11/09/99 M.L Start
        if (!Equal(export.Case1.StatusDate, local.Blank.Date) && AsChar
          (export.Case1.Status) == 'O')
        {
          export.Case1.StatusDate = local.Blank.Date;
        }

        // 11/09/99 M.L End
        // 01/26/02  M. L Start
        // Modified the next IF statment from
        // IF export case closure_reason <> export_prev case closure_reason
        // to
        // IF export case status = 'C'
        // 01/26/02 M.L End
        if (AsChar(export.Case1.Status) == 'C')
        {
          // 01/26/02 M.L start
          // 01/26/02 M.L end
          UseSiCadsCaseClosureProcessing();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            var field1 = GetField(export.Case1, "number");

            field1.Error = true;

            UseEabRollbackCics();

            break;
          }

          // *** Problem report I00115283
          // *** 03/26/01 swsrchf
          // *** start
          if (ReadCsePerson())
          {
            if (AsChar(entities.KeyOnly.Type1) == 'C')
            {
              local.Document.Name = "NOTCCLOS";
            }
          }
          else
          {
            ExitState = "CSE_PERSON_NF";

            break;
          }

          // *** end
          // *** 03/26/01 swsrchf
          // *** Problem request I00115283
          export.CaseSuccessfullyClosed.Flag = "Y";
        }

        // 01/25/02  M.L Start
        export.Prev.Assign(export.Case1);

        // 01/25/02  M.L End
        var field = GetField(export.Next, "number");

        field.Color = "green";
        field.Highlighting = Highlighting.Underscore;
        field.Protected = false;
        field.Focused = true;

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();
          export.HiddenRedisplay.Flag = "";

          break;
        }

        export.HiddenCase.ClosureReason = "";
        export.HiddenCase.StatusDate = local.Blank.Date;

        // mjr
        // -----------------------------------------------
        // 04/06/1999
        // Modified end of UPDATE to accommodate Print of ARCLOS60 and NOTCCLOS.
        // (By checking for a document BEFORE setting exitstates)
        // ------------------------------------------------------------
        if (!IsEmpty(local.Document.Name))
        {
          // mjr
          // ----------------------------------------------
          // 04/14/1999
          // Batch documents don't flow to DKEY
          // -----------------------------------------------------------
          // 02/04/02 M.L Start
          if (Equal(local.Document.Name, "ARCLOS60") && (
            Equal(export.Case1.ClosureReason, "AR") || Equal
            (export.Case1.ClosureReason, "BG") || Equal
            (export.Case1.ClosureReason, "RO")))
          {
            goto Test15;
          }

          // 02/04/02 M.L End
          // -----------------------------------------------------------
          // 10/15/99 W.Campbell - Logic added to not
          // produce a document if the AR is an
          // Organization ( last char of CSE PERSON
          // number = 'O' (alpha O)).  Work done
          // on PR#H00077426.
          // -----------------------------------------------------------
          if (CharAt(export.ArCsePersonsWorkSet.Number, 10) == 'O' || Equal
            (export.Case1.ClosureReason, "BG"))
          {
            // ----------------------------------
            // IF the AR is an Organization, then
            // don't produce a document.
            // ----------------------------------
            goto Test15;
          }

          // mjr
          // ----------------------------------------------
          // 03/06/2002
          // PR133505 - Don't send documents if there is
          // incoming interstate involvement
          // -----------------------------------------------------------
          // 02/14/01 M.L Start
          local.SpDocKey.KeyCase = export.Case1.Number;
          local.SpDocKey.KeyAp = export.Ap.Number;
          UseSpCabDetermineInterstateDoc();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabRollbackCics();
            export.HiddenRedisplay.Flag = "";

            break;
          }
        }

Test15:

        if (AsChar(export.CaseSuccessfullyClosed.Flag) != 'Y')
        {
          // -----------------------------------------------------
          // 03/04/00 W.Campbell - Added local view for
          // expedited paternity and logic to use that to
          // determine if Expedited Paternity is being
          // updated to a value of "Y".  If it is, then
          // display a modified Update successful exit
          // state msg to the user which reminds them
          // to validate the other paternity information
          // on this case.  This work was done on WR# 000160
          // for PRWORA - Paternity modifications.
          // -----------------------------------------------------
          if (AsChar(local.Case1.ExpeditedPaternityInd) == 'Y')
          {
            ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE_EP";
          }
          else
          {
            ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
          }

          // -----------------------------------------------------
          // 03/04/00 W.Campbell - End of code added.
          // -----------------------------------------------------
        }
        else
        {
          // -------------------------------------------------------------------
          // 06/23/99 W.Campbell - Removed disabled
          // code to process interstate case
          // events as needed.
          // -------------------------------------------------------------------
          // -------------------------------------------------------------------
          // 05/20/99 W.Campbell - Disabled code to process
          // interstate case events as needed.
          // -------------------------------------------------------------------
          // -------------------------------------------------------------------
          // 05/19/99 W.Campbell - Added code to process
          // interstate case events as needed.
          // -------------------------------------------------------------------
          // -------------------------------------------------------------------
          // 05/19/99 W.Campbell - End of code added to
          // process interstate case events as needed.
          // -------------------------------------------------------------------
          // -------------------------------------------------------------------
          // 07/07/00 W.Campbell -  Code added to
          // fix problem with not allowing update of closed
          // case.  Work done on PR#98438.
          // -------------------------------------------------------------------
          export.CaseOpen.Flag = "N";

          // -------------------------------------------------------------------
          // 07/07/00 W.Campbell - End of code added to
          // fix problem with not allowing update of closed
          // case.  Work done on PR#98438.
          // -------------------------------------------------------------------
          export.CaseSuccessfullyClosed.Flag = "";

          // *** Work request 000240
          // *** 03/26/01 swsrchf
          // *** start
          ExitState = "SI0000_CASE_SUCESSFULLY_CLOSED";

          // *** end
          // *** 03/26/01 swsrchf
          // *** Work request 000240
        }

        break;
      case "PRINTRET":
        // mjr
        // ----------------------------------------
        // 04/14/1999
        // PrintRet is not used for batch documents.
        // -----------------------------------------------------
        break;
      case "LIST":
        switch(AsChar(export.ArPrompt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            ++local.Invalid.Count;
            export.FromCads.Flag = "Y";
            ExitState = "ECO_LNK_TO_ROLE";

            break;
          default:
            var field1 = GetField(export.ArPrompt, "selectChar");

            field1.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            ++local.Invalid.Count;

            break;
        }

        switch(AsChar(export.ApPrompt.SelectChar))
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

        if (AsChar(export.MedProgExists.Flag) == 'Y')
        {
          switch(AsChar(export.PaMedCdPrompt.SelectChar))
          {
            case '+':
              break;
            case ' ':
              break;
            case 'S':
              ++local.Invalid.Count;
              export.Prompt.CodeName = "PA MED SERVICE";
              ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

              break;
            default:
              var field1 = GetField(export.PaMedCdPrompt, "selectChar");

              field1.Error = true;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
              ++local.Invalid.Count;

              break;
          }
        }

        switch(AsChar(export.CaseClosureRsnPrompt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            ++local.Invalid.Count;
            export.Prompt.CodeName = "CASE CLOSURE REASON";
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            break;
          default:
            var field1 = GetField(export.CaseClosureRsnPrompt, "selectChar");

            field1.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            ++local.Invalid.Count;

            break;
        }

        // @@@
        switch(AsChar(export.NoJurisdictionPrompt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            ++local.Invalid.Count;
            export.Prompt.CodeName = "NO JURISDICTION";
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            break;
          default:
            var field1 = GetField(export.NoJurisdictionPrompt, "selectChar");

            field1.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            ++local.Invalid.Count;

            break;
        }

        switch(AsChar(export.DuplicateIndPrompt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            ++local.Invalid.Count;
            export.Prompt.CodeName = "DUPLICATE CASE";
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            break;
          default:
            var field1 = GetField(export.DuplicateIndPrompt, "selectChar");

            field1.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            ++local.Invalid.Count;

            break;
        }

        switch(AsChar(export.TermCodePrompt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            ++local.Invalid.Count;
            export.Prompt.CodeName = "CASE TERMINATION REASON";
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            break;
          default:
            var field1 = GetField(export.TermCodePrompt, "selectChar");

            field1.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            ++local.Invalid.Count;

            break;
        }

        // CHECK FOR PROMPT ON Good Cause
        if (!export.Gc.IsEmpty)
        {
          for(export.Gc.Index = 0; export.Gc.Index < export.Gc.Count; ++
            export.Gc.Index)
          {
            if (!export.Gc.CheckSize())
            {
              break;
            }

            switch(AsChar(export.Gc.Item.GcCodePrompt.SelectChar))
            {
              case ' ':
                break;
              case '+':
                break;
              case 'S':
                ++local.Invalid.Count;

                // 58691
                export.Prompt.CodeName = "CADS GOOD CAUSE";
                ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

                break;
              default:
                var field1 =
                  GetField(export.Gc.Item.GcCodePrompt, "selectChar");

                field1.Error = true;

                ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";
                ++local.Invalid.Count;

                break;
            }
          }

          export.Gc.CheckIndex();
        }

        // Non-Coop Prompt
        if (!export.Nc.IsEmpty)
        {
          for(export.Nc.Index = 0; export.Nc.Index < export.Nc.Count; ++
            export.Nc.Index)
          {
            if (!export.Nc.CheckSize())
            {
              break;
            }

            switch(AsChar(export.Nc.Item.NcCodePrmpt.SelectChar))
            {
              case ' ':
                break;
              case '+':
                break;
              case 'S':
                ++local.Invalid.Count;

                // 58691
                export.Prompt.CodeName = "CADS NON COOPERATION";
                ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

                break;
              default:
                var field1 = GetField(export.Nc.Item.NcCodePrmpt, "selectChar");

                field1.Error = true;

                ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";
                ++local.Invalid.Count;

                break;
            }

            switch(AsChar(export.Nc.Item.NcRsnPrompt.SelectChar))
            {
              case ' ':
                break;
              case '+':
                break;
              case 'S':
                ++local.Invalid.Count;
                export.Prompt.CodeName = "NON COOP REASON";
                ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

                break;
              default:
                var field1 = GetField(export.Nc.Item.NcRsnPrompt, "selectChar");

                field1.Error = true;

                ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";
                ++local.Invalid.Count;

                break;
            }
          }

          export.Nc.CheckIndex();
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

            if (AsChar(export.ApPrompt.SelectChar) == 'S')
            {
              var field1 = GetField(export.ApPrompt, "selectChar");

              field1.Error = true;
            }

            if (AsChar(export.MedProgExists.Flag) == 'Y')
            {
              if (AsChar(export.PaMedCdPrompt.SelectChar) == 'S')
              {
                var field1 = GetField(export.PaMedCdPrompt, "selectChar");

                field1.Error = true;
              }
            }

            if (AsChar(export.CaseClosureRsnPrompt.SelectChar) == 'S')
            {
              var field1 = GetField(export.CaseClosureRsnPrompt, "selectChar");

              field1.Error = true;
            }

            // @@@
            if (AsChar(export.NoJurisdictionPrompt.SelectChar) == 'S')
            {
              var field1 = GetField(export.NoJurisdictionPrompt, "selectChar");

              field1.Error = true;
            }

            if (AsChar(export.DuplicateIndPrompt.SelectChar) == 'S')
            {
              var field1 = GetField(export.DuplicateIndPrompt, "selectChar");

              field1.Error = true;
            }

            if (AsChar(export.TermCodePrompt.SelectChar) == 'S')
            {
              var field1 = GetField(export.TermCodePrompt, "selectChar");

              field1.Error = true;
            }

            if (!export.Gc.IsEmpty)
            {
              for(export.Gc.Index = 0; export.Gc.Index < export.Gc.Count; ++
                export.Gc.Index)
              {
                if (!export.Gc.CheckSize())
                {
                  break;
                }

                if (AsChar(export.Gc.Item.GcCodePrompt.SelectChar) == 'S')
                {
                  var field1 =
                    GetField(export.Gc.Item.GcCodePrompt, "selectChar");

                  field1.Error = true;
                }
              }

              export.Gc.CheckIndex();
            }

            if (!export.Nc.IsEmpty)
            {
              for(export.Nc.Index = 0; export.Nc.Index < export.Nc.Count; ++
                export.Nc.Index)
              {
                if (!export.Nc.CheckSize())
                {
                  break;
                }

                if (AsChar(export.Nc.Item.NcCodePrmpt.SelectChar) == 'S')
                {
                  var field1 =
                    GetField(export.Nc.Item.NcCodePrmpt, "selectChar");

                  field1.Error = true;
                }

                if (AsChar(export.Nc.Item.NcRsnPrompt.SelectChar) == 'S')
                {
                  var field1 =
                    GetField(export.Nc.Item.NcRsnPrompt, "selectChar");

                  field1.Error = true;
                }
              }

              export.Nc.CheckIndex();
            }

            break;
        }

        break;
      case "HELP":
        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
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

        return;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

Test16:

    if (Equal(global.Command, "DISPLAY"))
    {
      export.PaMedCdPrompt.SelectChar = "";
      export.TermCodePrompt.SelectChar = "";
      export.DuplicateIndPrompt.SelectChar = "";
      export.CaseClosureRsnPrompt.SelectChar = "";
      export.NoJurisdictionPrompt.SelectChar = "";
      export.Program.Code = "";
      export.Case1.PaMedicalService = "";
      export.Case1.ClosureReason = "";
      export.Case1.ExpeditedPaternityInd = "";
      export.Case1.FullServiceWithMedInd = "";
      export.Case1.FullServiceWithoutMedInd = "";
      export.Case1.LocateInd = "";
      export.Case1.Note = Spaces(Case1.Note_MaxLength);
      export.Case1.CseOpenDate = local.Blank.Date;
      export.Case1.ClosureLetterDate = local.Blank.Date;
      export.Case1.StatusDate = local.Blank.Date;
      export.Case1.Status = "";
      export.Case1.NoJurisdictionCd = "";
      export.DesignatedPayeeFnd.Flag = "";
      export.InterstateRequest.KsCaseInd = "";
      export.ArCaseRole.AssignmentOfRights = "";
      export.ArCaseRole.AssignmentTerminationCode = "";
      export.ArCaseRole.AssignmentDate = local.Blank.Date;
      export.ArCaseRole.AssignmentTerminatedDt = local.Blank.Date;
      export.HiddenCase.ClosureReason = "";
      export.HiddenCase.StatusDate = local.Blank.Date;

      // 11/04/99 M.L Start
      export.SuccessfullyDisplayed.Flag = "";

      // 11/04/99 M.L End
      export.CaseFuncWorkSet.FuncDate = local.Blank.Date;
      export.CaseFuncWorkSet.FuncText3 = "";

      // ------------------------------------------------------------
      // Clear export group views
      // ------------------------------------------------------------
      for(export.Gc.Index = 0; export.Gc.Index < Export.GcGroup.Capacity; ++
        export.Gc.Index)
      {
        if (!export.Gc.CheckSize())
        {
          break;
        }

        export.Gc.Update.GcDetApCsePersonsWorkSet.Number = "";
        export.Gc.Update.GcDetCommon.SelectChar = "";
        export.Gc.Update.GcCodePrompt.SelectChar = "";
        export.Gc.Update.GcDetGoodCause.Code = "";
        export.Gc.Update.GcDetGoodCause.EffectiveDate = local.Blank.Date;
      }

      export.Gc.CheckIndex();

      for(export.Nc.Index = 0; export.Nc.Index < Export.NcGroup.Capacity; ++
        export.Nc.Index)
      {
        if (!export.Nc.CheckSize())
        {
          break;
        }

        export.Nc.Update.NcDetNonCooperation.EffectiveDate = local.Blank.Date;
        export.Nc.Update.NcDetNonCooperation.Reason = "";
        export.Nc.Update.NcDetApCsePersonsWorkSet.Number = "";
        export.Nc.Update.NcDetNonCooperation.Code = "";
        export.Nc.Update.NcDetCommon.SelectChar = "";
        export.Nc.Update.NcCodePrmpt.SelectChar = "";
        export.Nc.Update.NcRsnPrompt.SelectChar = "";
      }

      export.Nc.CheckIndex();

      for(export.Interstate.Index = 0; export.Interstate.Index < export
        .Interstate.Count; ++export.Interstate.Index)
      {
        export.Interstate.Update.Interstate1.Text8 = "";
        export.Interstate.Update.Interstate1.Text10 = "";
      }

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        // 11/04/99 M.L Start
        export.SuccessfullyDisplayed.Flag = "Y";

        // 11/04/99 M.L End
        if (!IsEmpty(export.Next.Number))
        {
          UseCabZeroFillNumber1();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            if (IsExitState("CASE_NUMBER_NOT_NUMERIC"))
            {
              var field = GetField(export.Next, "number");

              field.Error = true;

              // 11/04/99 M.L Start
              export.SuccessfullyDisplayed.Flag = "N";

              // 11/04/99 M.L End
              goto Test21;
            }
          }
        }
        else
        {
          var field = GetField(export.Next, "number");

          field.Error = true;

          ExitState = "CASE_NUMBER_REQUIRED";

          // 11/04/99 M.L Start
          export.SuccessfullyDisplayed.Flag = "N";

          // 11/04/99 M.L End
          goto Test20;
        }

        if (IsEmpty(export.Case1.Number))
        {
          export.Case1.Number = export.Next.Number;
        }

        if (!Equal(export.Next.Number, export.Case1.Number))
        {
          // Check AP number and Person number to determine if they are
          // valid and associated to the new case number.
          UseSiCheckCaseToApAndChild();
          export.Case1.Number = export.Next.Number;
        }

        export.NcPlus.OneChar = "";
        export.NcMinus.OneChar = "";
        export.GcPlus.OneChar = "";
        export.GcMinus.OneChar = "";

        // ---------------------------------------------
        // Call the action block that reads
        // the data required for this screen.
        // --------------------------------------------
        if (AsChar(export.FromCads.Flag) == 'Y')
        {
          UseSiReadCaseHeaderInfo2();
        }
        else
        {
          UseSiReadCaseHeaderInformation();
        }

        if (!IsEmpty(local.AbendData.Type1))
        {
          // M.L 09/28/99 If EXIT STATE is OK for Abend of Natural then changed 
          // exit state to error.
          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_ADABAS_UNAVAILABLE";
          }

          // M.L 09/28/99 End.
          // 11/04/99 M.L Start
          export.SuccessfullyDisplayed.Flag = "N";

          // 11/04/99 M.L End
          goto Test20;
        }

        if (AsChar(local.MultipleAps.Flag) == 'Y')
        {
          // -----------------------------------------------
          // 05/02/99 W.Campbell - Added code to send
          // selection needed msg to COMP.  BE SURE
          // TO MATCH next_tran_info ON THE DIALOG
          // FLOW.
          // -----------------------------------------------
          export.HiddenNextTranInfo.MiscText1 = "SELECTION NEEDED";
          ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

          goto Test20;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (IsExitState("NO_APS_ON_A_CASE"))
          {
            local.NoAps.Flag = "Y";
            ExitState = "ACO_NN0000_ALL_OK";

            goto Test17;
          }

          // 11/04/99 M.L Start
          export.SuccessfullyDisplayed.Flag = "N";

          // 11/04/99 M.L End
          goto Test20;
        }

Test17:

        UseSiReadOfficeOspHeader();
        UseSiCadsReadCaseDetails();

        // 11/09/99 M.L Start
        export.Original.StatusDate = export.Case1.StatusDate;

        // 11/09/99 M.L End
        if (AsChar(export.Case1.Status) == 'O')
        {
          export.Case1.StatusDate = local.Blank.Date;
        }

        // @@@
        for(export.Interstate.Index = 0; export.Interstate.Index < export
          .Interstate.Count; ++export.Interstate.Index)
        {
          if (IsEmpty(export.Interstate.Item.Interstate1.Text10))
          {
            // Interstate involvement is to a domestic IV-D agency.
            var field = GetField(export.Interstate.Item.Interstate1, "text8");

            field.Highlighting = Highlighting.Normal;
            field.Protected = true;
          }
          else
          {
            // Interstate involvement is to a foreign or tribal IV-D agency.  
            // Highlight the interstate agency code in reverse video.
            var field = GetField(export.Interstate.Item.Interstate1, "text8");

            field.Highlighting = Highlighting.ReverseVideo;
            field.Protected = true;
          }
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.Prev.Assign(export.Case1);
        }
        else
        {
          if (IsExitState("SI0000_PERSON_PROGRAM_CASE_NF") || IsExitState
            ("CASE_NF"))
          {
            var field = GetField(export.Next, "number");

            field.Error = true;
          }
          else if (IsExitState("CASE_ROLE_AR_NF"))
          {
            var field = GetField(export.ArCsePersonsWorkSet, "number");

            field.Error = true;
          }

          // 11/04/99 M.L Start
          export.SuccessfullyDisplayed.Flag = "N";

          // 11/04/99 M.L End
          goto Test20;
        }

        // 11/17/99 M.L Start
        if (Equal(export.Program.Code, "NA"))
        {
          if (AsChar(export.MedProgExists.Flag) == 'Y')
          {
            if (ReadPersonProgram())
            {
              export.MedProgExists.Flag = "";
            }
          }
        }
        else
        {
          export.MedProgExists.Flag = "";
        }

        // 11/17/99 M.L End
        if (AsChar(export.MedProgExists.Flag) == 'Y')
        {
          export.PaMedCdPrompt.SelectChar = "+";
        }
        else
        {
          export.PaMedCdPrompt.SelectChar = "";

          var field1 = GetField(export.Case1, "paMedicalService");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.PaMedCdPrompt, "selectChar");

          field2.Color = "cyan";
          field2.Protected = true;
        }

        export.HidNoItemsFndGc.Count = 0;
        export.CurrItmNoNc.Count = 0;
        UseSiCadsGetGoodCauseNonCoop();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          // 11/04/99 M.L Start
          export.SuccessfullyDisplayed.Flag = "N";

          // 11/04/99 M.L End
          goto Test20;
        }

        if (export.HiddenGrpGc.IsEmpty && export.HiddenGrpNc.IsEmpty)
        {
          var field = GetField(export.Next, "number");

          field.Color = "green";
          field.Highlighting = Highlighting.Underscore;
          field.Protected = false;
          field.Focused = true;

          goto Test20;
        }

        if (export.HiddenGrpGc.IsFull || export.HiddenGrpNc.IsFull)
        {
          ExitState = "ACO_NE0000_MAX_PAGES_REACHED";

          goto Test20;
        }

        export.Gc.Index = 0;
        export.Gc.CheckSize();

        export.NoItemsGc.Count = 0;
        export.CurrItmNoGc.Count = 0;

        if (!export.HiddenGrpGc.IsEmpty)
        {
          for(export.HiddenGrpGc.Index = 0; export.HiddenGrpGc.Index < export
            .HiddenGrpGc.Count; ++export.HiddenGrpGc.Index)
          {
            if (!export.HiddenGrpGc.CheckSize())
            {
              break;
            }

            ++export.Gc.Index;
            export.Gc.CheckSize();

            export.Gc.Update.GcDetApCsePersonsWorkSet.Number =
              export.HiddenGrpGc.Item.HiddenGrpGcDetCsePersonsWorkSet.Number;
            MoveCaseRole(export.HiddenGrpGc.Item.HiddenGrpGcDetAp,
              export.Gc.Update.GcDetApCaseRole);
            export.Gc.Update.GcDetCommon.SelectChar =
              export.HiddenGrpGc.Item.HiddenGrpGcDetCommon.SelectChar;
            export.Gc.Update.GcDetGoodCause.Assign(
              export.HiddenGrpGc.Item.HiddenGrpGcDetGoodCause);
            ++export.NoItemsGc.Count;
            ++export.CurrItmNoGc.Count;

            if (export.NoItemsGc.Count >= Export.GcGroup.Capacity - 1)
            {
              goto Test18;
            }
          }

          export.HiddenGrpGc.CheckIndex();
        }

Test18:

        export.Gc1.PageNumber = 1;
        export.MaxPagesGc.Count = export.HidNoItemsFndGc.Count / 3;
        local.Count.AverageReal = (decimal)export.HidNoItemsFndGc.Count / 3;

        if (local.Count.AverageReal > export.MaxPagesGc.Count)
        {
          ++export.MaxPagesGc.Count;
        }

        export.Nc.Index = 0;
        export.Nc.CheckSize();

        export.NoItemsNc.Count = 0;
        export.CurrItmNoNc.Count = 0;

        if (!export.HiddenGrpNc.IsEmpty)
        {
          for(export.HiddenGrpNc.Index = 0; export.HiddenGrpNc.Index < export
            .HiddenGrpNc.Count; ++export.HiddenGrpNc.Index)
          {
            if (!export.HiddenGrpNc.CheckSize())
            {
              break;
            }

            ++export.Nc.Index;
            export.Nc.CheckSize();

            export.Nc.Update.NcDetApCsePersonsWorkSet.Number =
              export.HiddenGrpNc.Item.HiddenGrpNcDetApCsePersonsWorkSet.Number;
            MoveCaseRole(export.HiddenGrpNc.Item.HiddenGrpNcDetApCaseRole,
              export.Nc.Update.NcDetApCaseRole);
            export.Nc.Update.NcDetCommon.SelectChar =
              export.HiddenGrpNc.Item.HiddenGrpNcDetCommon.SelectChar;
            export.Nc.Update.NcDetNonCooperation.Assign(
              export.HiddenGrpNc.Item.HiddenGrpNcDetNonCooperation);
            ++export.NoItemsNc.Count;
            ++export.CurrItmNoNc.Count;

            if (export.NoItemsNc.Count >= Export.NcGroup.Capacity - 1)
            {
              goto Test19;
            }
          }

          export.HiddenGrpNc.CheckIndex();
        }

Test19:

        export.Nc1.PageNumber = 1;
        export.MaxPagesNc.Count = export.HidNoItemsFndNc.Count / 3;
        local.Count.AverageReal = (decimal)export.HidNoItemsFndNc.Count / 3;

        if (local.Count.AverageReal > export.MaxPagesNc.Count)
        {
          ++export.MaxPagesNc.Count;
        }
      }

Test20:

      if (export.MaxPagesGc.Count > export.Gc1.PageNumber)
      {
        export.GcPlus.OneChar = "+";
      }

      if (export.MaxPagesNc.Count > export.Nc1.PageNumber)
      {
        export.NcPlus.OneChar = "+";
      }

      if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
        ("ACO_NE0000_MAX_PAGES_REACHED"))
      {
        export.ArPrev.Assign(export.ArCaseRole);
        export.Prev.Assign(export.Case1);
        export.HiddenPrev.Number = export.Ap.Number;

        if (!IsExitState("ACO_NE0000_MAX_PAGES_REACHED"))
        {
          if (AsChar(export.CaseOpen.Flag) == 'N')
          {
            ExitState = "DISPLAY_OK_FOR_CLOSED_CASE";
          }
          else
          {
            // mjr
            // -----------------------------------------------
            // 04/14/1999
            // Removed check for an exitstate returned from Print
            // ------------------------------------------------------------
            ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";

            // mjr
            // -----------------------------------------
            // 12/31/1998
            // Added check for successful close.
            // ------------------------------------------------------
            if (AsChar(export.CaseSuccessfullyClosed.Flag) == 'Y')
            {
              export.CaseSuccessfullyClosed.Flag = "";
            }

            if (AsChar(local.NoAps.Flag) == 'Y')
            {
              ExitState = "NO_APS_ON_A_CASE";
            }

            if (AsChar(local.NoActiveProgramInd.Flag) == 'Y')
            {
              var field1 = GetField(export.Case1, "number");

              field1.Error = true;

              ExitState = "SI0000_PERSON_PROGRAM_CASE_NF";
            }
          }
        }
        else
        {
          // 11/04/99 M.L Start
          export.SuccessfullyDisplayed.Flag = "N";

          // 11/04/99 M.L End
        }

        if (AsChar(export.HiddenRedisplay.Flag) == 'A')
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
        }

        export.HiddenRedisplay.Flag = "";

        var field = GetField(export.Next, "number");

        field.Color = "green";
        field.Highlighting = Highlighting.Underscore;
        field.Protected = false;
        field.Focused = true;
      }

      for(export.Gc.Index = 1; export.Gc.Index < Export.GcGroup.Capacity; ++
        export.Gc.Index)
      {
        if (!export.Gc.CheckSize())
        {
          break;
        }

        export.Gc.Update.GcCodePrompt.SelectChar = "+";

        var field1 =
          GetField(export.Gc.Item.GcDetApCsePersonsWorkSet, "number");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.Gc.Item.GcDetCommon, "selectChar");

        field2.Color = "cyan";
        field2.Intensity = Intensity.Dark;
        field2.Protected = true;

        var field3 = GetField(export.Gc.Item.GcDetGoodCause, "code");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.Gc.Item.GcCodePrompt, "selectChar");

        field4.Color = "cyan";
        field4.Protected = true;

        var field5 = GetField(export.Gc.Item.GcDetGoodCause, "effectiveDate");

        field5.Color = "cyan";
        field5.Protected = true;
      }

      export.Gc.CheckIndex();

      for(export.Nc.Index = 1; export.Nc.Index < Export.NcGroup.Capacity; ++
        export.Nc.Index)
      {
        if (!export.Nc.CheckSize())
        {
          break;
        }

        export.Nc.Update.NcCodePrmpt.SelectChar = "+";
        export.Nc.Update.NcRsnPrompt.SelectChar = "+";

        var field1 = GetField(export.Nc.Item.NcDetCommon, "selectChar");

        field1.Color = "cyan";
        field1.Intensity = Intensity.Dark;
        field1.Protected = true;

        var field2 = GetField(export.Nc.Item.NcDetNonCooperation, "code");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.Nc.Item.NcCodePrmpt, "selectChar");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.Nc.Item.NcRsnPrompt, "selectChar");

        field4.Color = "cyan";
        field4.Protected = true;

        var field5 = GetField(export.Nc.Item.NcDetNonCooperation, "reason");

        field5.Color = "cyan";
        field5.Protected = true;

        var field6 =
          GetField(export.Nc.Item.NcDetNonCooperation, "effectiveDate");

        field6.Color = "cyan";
        field6.Protected = true;

        var field7 =
          GetField(export.Nc.Item.NcDetApCsePersonsWorkSet, "number");

        field7.Color = "cyan";
        field7.Protected = true;
      }

      export.Nc.CheckIndex();
    }
    else
    {
      if (AsChar(export.MedProgExists.Flag) != 'Y')
      {
        var field1 = GetField(export.Case1, "paMedicalService");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.PaMedCdPrompt, "selectChar");

        field2.Color = "cyan";
        field2.Protected = true;
      }

      export.Gc.Index = 0;
      export.Gc.CheckSize();

      if (IsEmpty(export.Gc.Item.GcCodePrompt.SelectChar))
      {
        export.Gc.Update.GcCodePrompt.SelectChar = "+";
      }

      export.Nc.Index = 0;
      export.Nc.CheckSize();

      if (IsEmpty(export.Nc.Item.NcCodePrmpt.SelectChar))
      {
        export.Nc.Update.NcCodePrmpt.SelectChar = "+";
      }

      if (IsEmpty(export.Nc.Item.NcRsnPrompt.SelectChar))
      {
        export.Nc.Update.NcRsnPrompt.SelectChar = "+";
      }
    }

Test21:

    if (!IsExitState("ACO_NN0000_ALL_OK") || !
      IsExitState("CANNOT_MODIFY_CLOSED_CASE"))
    {
      for(export.Gc.Index = 1; export.Gc.Index < Export.GcGroup.Capacity; ++
        export.Gc.Index)
      {
        if (!export.Gc.CheckSize())
        {
          break;
        }

        export.Gc.Update.GcCodePrompt.SelectChar = "+";

        var field1 =
          GetField(export.Gc.Item.GcDetApCsePersonsWorkSet, "number");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.Gc.Item.GcDetCommon, "selectChar");

        field2.Color = "cyan";
        field2.Intensity = Intensity.Dark;
        field2.Protected = true;

        var field3 = GetField(export.Gc.Item.GcDetGoodCause, "code");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.Gc.Item.GcCodePrompt, "selectChar");

        field4.Color = "cyan";
        field4.Protected = true;

        var field5 = GetField(export.Gc.Item.GcDetGoodCause, "effectiveDate");

        field5.Color = "cyan";
        field5.Protected = true;
      }

      export.Gc.CheckIndex();

      for(export.Nc.Index = 1; export.Nc.Index < Export.NcGroup.Capacity; ++
        export.Nc.Index)
      {
        if (!export.Nc.CheckSize())
        {
          break;
        }

        export.Nc.Update.NcCodePrmpt.SelectChar = "+";
        export.Nc.Update.NcRsnPrompt.SelectChar = "+";

        var field1 = GetField(export.Nc.Item.NcDetCommon, "selectChar");

        field1.Color = "cyan";
        field1.Intensity = Intensity.Dark;
        field1.Protected = true;

        var field2 = GetField(export.Nc.Item.NcDetNonCooperation, "code");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.Nc.Item.NcCodePrmpt, "selectChar");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.Nc.Item.NcRsnPrompt, "selectChar");

        field4.Color = "cyan";
        field4.Protected = true;

        var field5 = GetField(export.Nc.Item.NcDetNonCooperation, "reason");

        field5.Color = "cyan";
        field5.Protected = true;

        var field6 =
          GetField(export.Nc.Item.NcDetNonCooperation, "effectiveDate");

        field6.Color = "cyan";
        field6.Protected = true;

        var field7 =
          GetField(export.Nc.Item.NcDetApCsePersonsWorkSet, "number");

        field7.Color = "cyan";
        field7.Protected = true;
      }

      export.Nc.CheckIndex();
    }
  }

  private static void MoveCase2(Case1 source, Case1 target)
  {
    target.Number = source.Number;
    target.ClosureReason = source.ClosureReason;
    target.Status = source.Status;
    target.StatusDate = source.StatusDate;
    target.Note = source.Note;
  }

  private static void MoveCase3(Case1 source, Case1 target)
  {
    target.Number = source.Number;
    target.ClosureReason = source.ClosureReason;
    target.StatusDate = source.StatusDate;
    target.DuplicateCaseIndicator = source.DuplicateCaseIndicator;
  }

  private static void MoveCaseFuncWorkSet(CaseFuncWorkSet source,
    CaseFuncWorkSet target)
  {
    target.FuncText3 = source.FuncText3;
    target.FuncDate = source.FuncDate;
  }

  private static void MoveCaseRole(CaseRole source, CaseRole target)
  {
    target.Identifier = source.Identifier;
    target.Type1 = source.Type1;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveGc1(Export.GcGroup source,
    SiCadsUpdateCaseDetails.Import.GcGroup target)
  {
    target.GgcCdPrmpt.SelectChar = source.GcCodePrompt.SelectChar;
    target.GgcCommon.SelectChar = source.GcDetCommon.SelectChar;
    MoveCaseRole(source.GcDetApCaseRole, target.GgcApCaseRole);
    target.GgcApCsePersonsWorkSet.Number =
      source.GcDetApCsePersonsWorkSet.Number;
    target.GgcGoodCause.Assign(source.GcDetGoodCause);
  }

  private static void MoveGc2(SiCadsUpdateCaseDetails.Export.GcGroup source,
    Export.GcGroup target)
  {
    target.GcCodePrompt.SelectChar = source.GgcPromptSel.SelectChar;
    target.GcDetCommon.SelectChar = source.GgcCommon.SelectChar;
    MoveCaseRole(source.GgcApCaseRole, target.GcDetApCaseRole);
    target.GcDetApCsePersonsWorkSet.Number =
      source.GgcApCsePersonsWorkSet.Number;
    target.GcDetGoodCause.Assign(source.GgcGoodCause);
  }

  private static void MoveGc3(SiCadsMoveNextGoodCause.Export.GcGroup source,
    Export.GcGroup target)
  {
    target.GcCodePrompt.SelectChar = source.GgcCdPrmpt.SelectChar;
    target.GcDetCommon.SelectChar = source.GgcCommon.SelectChar;
    MoveCaseRole(source.GgcApCaseRole, target.GcDetApCaseRole);
    target.GcDetApCsePersonsWorkSet.Number =
      source.GgcApCsePersonsWorkSet.Number;
    target.GcDetGoodCause.Assign(source.GgcGoodCause);
  }

  private static void MoveGc4(SiCadsGetGoodCauseNonCoop.Export.GcGroup source,
    Export.HiddenGrpGcGroup target)
  {
    target.HiddenGrpGcDetGoodCause.Assign(source.GcDetGoodCause);
    target.HiddenGrpGcDetCommon.SelectChar = source.GcDetCommon.SelectChar;
    MoveCaseRole(source.GcDetApCaseRole, target.HiddenGrpGcDetAp);
    target.HiddenGrpGcDetCsePersonsWorkSet.Number =
      source.GcDetApCsePersonsWorkSet.Number;
  }

  private static void MoveGrp1(SiGetApsForCase.Export.GrpGroup source,
    Local.GrpGroup target)
  {
    target.Ap.Number = source.Ap.Number;
  }

  private static void MoveGrp2(SiGetActiveApsForCase.Export.GrpGroup source,
    Local.GrpGroup target)
  {
    target.Ap.Number = source.Ap.Number;
  }

  private static void MoveHiddenGrpGc(Import.HiddenGrpGcGroup source,
    SiCadsMoveNextGoodCause.Import.HiddenGrpGcGroup target)
  {
    target.HiddenGGcGoodCause.Assign(source.HiddenGrpGcDetGoodCause);
    target.HiddenGGcCommon.SelectChar = source.HiddenGrpGcDetCommon.SelectChar;
    MoveCaseRole(source.HiddenGrpGcDetApCaseRole, target.HiddenGGcApCaseRole);
    target.HiddenGGcApCsePersonsWorkSet.Number =
      source.HiddenGrpGcDetApCsePersonsWorkSet.Number;
  }

  private static void MoveHiddenGrpNc(Import.HiddenGrpNcGroup source,
    SiCadsMoveNextNonCoops.Import.HiddenGrpNcGroup target)
  {
    target.HiddenGNcNonCooperation.Assign(source.HiddenGrpNcDetNonCooperation);
    target.HiddenGNcCommon.SelectChar = source.HiddenGrpNcDetCommon.SelectChar;
    MoveCaseRole(source.HiddenGrpNcDetApCaseRole, target.HiddenGNcApCaseRole);
    target.HiddenGNcApCsePersonsWorkSet.Number =
      source.HiddenGrpNcDetApCsePersonsWorkSet.Number;
  }

  private static void MoveInterstate(SiCadsReadCaseDetails.Export.
    InterstateGroup source, Export.InterstateGroup target)
  {
    target.Interstate1.Assign(source.Interstate1);
  }

  private static void MoveNc1(Export.NcGroup source,
    SiCadsUpdateCaseDetails.Import.NcGroup target)
  {
    target.GncRsnPrompt.SelectChar = source.NcRsnPrompt.SelectChar;
    target.GncCdPrmpt.SelectChar = source.NcCodePrmpt.SelectChar;
    target.GncCommon.SelectChar = source.NcDetCommon.SelectChar;
    MoveCaseRole(source.NcDetApCaseRole, target.GncApCaseRole);
    target.GncApCsePersonsWorkSet.Number =
      source.NcDetApCsePersonsWorkSet.Number;
    target.GncNonCooperation.Assign(source.NcDetNonCooperation);
  }

  private static void MoveNc2(SiCadsUpdateCaseDetails.Export.NcGroup source,
    Export.NcGroup target)
  {
    target.NcRsnPrompt.SelectChar = source.GncRsnPrompt.SelectChar;
    target.NcCodePrmpt.SelectChar = source.GncPromptSel.SelectChar;
    target.NcDetCommon.SelectChar = source.GncCommon.SelectChar;
    MoveCaseRole(source.GncApCaseRole, target.NcDetApCaseRole);
    target.NcDetApCsePersonsWorkSet.Number =
      source.GncApCsePersonsWorkSet.Number;
    target.NcDetNonCooperation.Assign(source.GncNonCooperation);
  }

  private static void MoveNc3(SiCadsMoveNextNonCoops.Export.NcGroup source,
    Export.NcGroup target)
  {
    target.NcRsnPrompt.SelectChar = source.NonCoopRsnPrmpt.SelectChar;
    target.NcCodePrmpt.SelectChar = source.NonCoopCdPrmpt.SelectChar;
    target.NcDetCommon.SelectChar = source.GncCommon.SelectChar;
    MoveCaseRole(source.GncApCaseRole, target.NcDetApCaseRole);
    target.NcDetApCsePersonsWorkSet.Number =
      source.GncApCsePersonsWorkSet.Number;
    target.NcDetNonCooperation.Assign(source.GncNonCooperation);
  }

  private static void MoveNc4(SiCadsGetGoodCauseNonCoop.Export.NcGroup source,
    Export.HiddenGrpNcGroup target)
  {
    target.HiddenGrpNcDetNonCooperation.Assign(source.NcDetNonCooperation);
    target.HiddenGrpNcDetCommon.SelectChar = source.NcDetCommon.SelectChar;
    MoveCaseRole(source.NcDetApCaseRole, target.HiddenGrpNcDetApCaseRole);
    target.HiddenGrpNcDetApCsePersonsWorkSet.Number =
      source.NcDetApCsePersonsWorkSet.Number;
  }

  private static void MoveNextTranInfo(NextTranInfo source, NextTranInfo target)
  {
    target.LastTran = source.LastTran;
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

  private static void MoveSpDocKey(SpDocKey source, SpDocKey target)
  {
    target.KeyAp = source.KeyAp;
    target.KeyCase = source.KeyCase;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.Common.Flag = useExport.ValidCode.Flag;
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

    useImport.CsePersonsWorkSet.Number =
      export.Nc.Item.NcDetApCsePersonsWorkSet.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    export.Nc.Update.NcDetApCsePersonsWorkSet.Number =
      useImport.CsePersonsWorkSet.Number;
  }

  private void UseCabZeroFillNumber3()
  {
    var useImport = new CabZeroFillNumber.Import();
    var useExport = new CabZeroFillNumber.Export();

    useImport.CsePersonsWorkSet.Number =
      export.Gc.Item.GcDetApCsePersonsWorkSet.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    export.Gc.Update.GcDetApCsePersonsWorkSet.Number =
      useImport.CsePersonsWorkSet.Number;
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

    useImport.CsePersonsWorkSet.Number = export.Ap.Number;
    useImport.Case1.Number = export.Next.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiCadsAddCaseRoleDetail1()
  {
    var useImport = new SiCadsAddCaseRoleDetail.Import();
    var useExport = new SiCadsAddCaseRoleDetail.Export();

    useImport.Row1AddFuncNc.Flag = local.Row1AddFuncNc.Flag;
    MoveCase2(export.Case1, useImport.Case1);
    useImport.Ap.Number = export.Nc.Item.NcDetApCsePersonsWorkSet.Number;
    useImport.NonCooperation.Assign(export.Nc.Item.NcDetNonCooperation);
    useImport.Ar.Number = export.ArCsePersonsWorkSet.Number;

    Call(SiCadsAddCaseRoleDetail.Execute, useImport, useExport);

    MoveCaseRole(useExport.CaseRole, export.Nc.Update.NcDetApCaseRole);
  }

  private void UseSiCadsAddCaseRoleDetail2()
  {
    var useImport = new SiCadsAddCaseRoleDetail.Import();
    var useExport = new SiCadsAddCaseRoleDetail.Export();

    useImport.Row1AddFuncGc.Flag = local.Row1AddFuncGc.Flag;
    MoveCase2(export.Case1, useImport.Case1);
    useImport.Ap.Number = export.Gc.Item.GcDetApCsePersonsWorkSet.Number;
    useImport.GoodCause.Assign(export.Gc.Item.GcDetGoodCause);
    useImport.Ar.Number = export.ArCsePersonsWorkSet.Number;

    Call(SiCadsAddCaseRoleDetail.Execute, useImport, useExport);

    MoveCaseRole(useExport.CaseRole, export.Gc.Update.GcDetApCaseRole);
  }

  private void UseSiCadsCaseClosureProcessing()
  {
    var useImport = new SiCadsCaseClosureProcessing.Import();
    var useExport = new SiCadsCaseClosureProcessing.Export();

    MoveCase2(export.Case1, useImport.Case1);

    Call(SiCadsCaseClosureProcessing.Execute, useImport, useExport);
  }

  private void UseSiCadsDetermineCaseClosure()
  {
    var useImport = new SiCadsDetermineCaseClosure.Import();
    var useExport = new SiCadsDetermineCaseClosure.Export();

    useImport.Current.Date = local.Current.Date;
    useImport.Ap.Number = export.Ap.Number;
    useImport.Case1.Number = export.Case1.Number;

    Call(SiCadsDetermineCaseClosure.Execute, useImport, useExport);
  }

  private void UseSiCadsGetGoodCauseNonCoop()
  {
    var useImport = new SiCadsGetGoodCauseNonCoop.Import();
    var useExport = new SiCadsGetGoodCauseNonCoop.Export();

    useImport.Ar.Number = export.ArCsePersonsWorkSet.Number;
    useImport.Case1.Number = export.Case1.Number;

    Call(SiCadsGetGoodCauseNonCoop.Execute, useImport, useExport);

    export.HidNoItemsFndNc.Count = useExport.NoNcFnd.Count;
    export.HidNoItemsFndGc.Count = useExport.NoGcFnd.Count;
    useExport.Nc.CopyTo(export.HiddenGrpNc, MoveNc4);
    useExport.Gc.CopyTo(export.HiddenGrpGc, MoveGc4);
  }

  private void UseSiCadsMoveNextGoodCause1()
  {
    var useImport = new SiCadsMoveNextGoodCause.Import();
    var useExport = new SiCadsMoveNextGoodCause.Export();

    useImport.HidNoItemsFndGc.Count = import.HidNoItemsFndGc.Count;
    useImport.GcMinus.OneChar = import.GcMinus.OneChar;
    useImport.GcPlus.OneChar = import.GcPlus.OneChar;
    import.HiddenGrpGc.CopyTo(useImport.HiddenGrpGc, MoveHiddenGrpGc);
    useImport.CurrItmNoGc.Count = export.CurrItmNoGc.Count;
    useImport.Gc1.PageNumber = export.Gc1.PageNumber;

    Call(SiCadsMoveNextGoodCause.Execute, useImport, useExport);

    export.NoItemsGc.Count = useExport.NoItemsGc.Count;
    export.CurrItmNoGc.Count = useExport.CurrItmNoGc.Count;
    export.HidNoItemsFndGc.Count = useExport.HidNoItemsFndGc.Count;
    export.Gc1.PageNumber = useExport.Gc1.PageNumber;
    export.GcMinus.OneChar = useExport.GcMinus.OneChar;
    export.GcPlus.OneChar = useExport.GcPlus.OneChar;
    useExport.Gc.CopyTo(export.Gc, MoveGc3);
  }

  private void UseSiCadsMoveNextGoodCause2()
  {
    var useImport = new SiCadsMoveNextGoodCause.Import();
    var useExport = new SiCadsMoveNextGoodCause.Export();

    useImport.GcMinus.OneChar = import.GcMinus.OneChar;
    useImport.GcPlus.OneChar = import.GcPlus.OneChar;
    import.HiddenGrpGc.CopyTo(useImport.HiddenGrpGc, MoveHiddenGrpGc);
    useImport.CurrItmNoGc.Count = export.CurrItmNoGc.Count;
    useImport.HidNoItemsFndGc.Count = export.HidNoItemsFndGc.Count;
    useImport.Gc1.PageNumber = export.Gc1.PageNumber;

    Call(SiCadsMoveNextGoodCause.Execute, useImport, useExport);

    export.NoItemsGc.Count = useExport.NoItemsGc.Count;
    export.CurrItmNoGc.Count = useExport.CurrItmNoGc.Count;
    export.HidNoItemsFndGc.Count = useExport.HidNoItemsFndGc.Count;
    export.Gc1.PageNumber = useExport.Gc1.PageNumber;
    export.GcMinus.OneChar = useExport.GcMinus.OneChar;
    export.GcPlus.OneChar = useExport.GcPlus.OneChar;
    useExport.Gc.CopyTo(export.Gc, MoveGc3);
  }

  private void UseSiCadsMoveNextNonCoops()
  {
    var useImport = new SiCadsMoveNextNonCoops.Import();
    var useExport = new SiCadsMoveNextNonCoops.Export();

    useImport.HidNoItemsFndNc.Count = import.HidNoItemsFndNc.Count;
    useImport.NcMinus.OneChar = import.NcMinus.OneChar;
    useImport.NcPlus.OneChar = import.NcPlus.OneChar;
    import.HiddenGrpNc.CopyTo(useImport.HiddenGrpNc, MoveHiddenGrpNc);
    useImport.CurrItmNoNc.Count = export.CurrItmNoNc.Count;
    useImport.Nc1.PageNumber = export.Nc1.PageNumber;

    Call(SiCadsMoveNextNonCoops.Execute, useImport, useExport);

    export.NoItemsNc.Count = useExport.NoItemsNc.Count;
    export.CurrItmNoNc.Count = useExport.CurrItmNoNc.Count;
    export.HidNoItemsFndNc.Count = useExport.HidNoItemsFndNc.Count;
    export.Nc1.PageNumber = useExport.Nc1.PageNumber;
    export.NcMinus.OneChar = useExport.NcMinus.OneChar;
    export.NcPlus.OneChar = useExport.NcPlus.OneChar;
    useExport.Nc.CopyTo(export.Nc, MoveNc3);
  }

  private void UseSiCadsReadCaseDetails()
  {
    var useImport = new SiCadsReadCaseDetails.Import();
    var useExport = new SiCadsReadCaseDetails.Export();

    useImport.Ar.Number = export.ArCsePersonsWorkSet.Number;
    useImport.Ap.Number = export.Ap.Number;
    useImport.Case1.Number = export.Case1.Number;

    Call(SiCadsReadCaseDetails.Execute, useImport, useExport);

    local.NoActiveProgramInd.Flag = useExport.NoActiveProgramInd.Flag;
    export.DesignatedPayeeFnd.Flag = useExport.DesignatedPayeeInd.Flag;
    export.Program.Code = useExport.Program.Code;
    export.ArCaseRole.Assign(useExport.Assign1);
    export.Case1.Assign(useExport.Case1);
    export.MedProgExists.Flag = useExport.MedProgExists.Flag;
    MoveCaseFuncWorkSet(useExport.CaseFuncWorkSet, export.CaseFuncWorkSet);
    useExport.Interstate.CopyTo(export.Interstate, MoveInterstate);
  }

  private void UseSiCadsUpdateCaseDetails()
  {
    var useImport = new SiCadsUpdateCaseDetails.Import();
    var useExport = new SiCadsUpdateCaseDetails.Export();

    useImport.DataChanged.Flag = local.DataChanged.Flag;
    useImport.ArCaseRole.Assign(export.ArCaseRole);
    useImport.ArCsePersonsWorkSet.Number = export.ArCsePersonsWorkSet.Number;
    useImport.Ap.Number = export.Ap.Number;
    useImport.Case1.Assign(export.Case1);
    export.Nc.CopyTo(useImport.Nc, MoveNc1);
    export.Gc.CopyTo(useImport.Gc, MoveGc1);

    Call(SiCadsUpdateCaseDetails.Execute, useImport, useExport);

    useExport.Nc.CopyTo(export.Nc, MoveNc2);
    useExport.Gc.CopyTo(export.Gc, MoveGc2);
  }

  private void UseSiCheckCaseToApAndChild()
  {
    var useImport = new SiCheckCaseToApAndChild.Import();
    var useExport = new SiCheckCaseToApAndChild.Export();

    useImport.Ap.Number = export.Ap.Number;
    useImport.HiddenAp.Number = export.HiddenPrev.Number;
    useImport.Case1.Number = export.Next.Number;

    Call(SiCheckCaseToApAndChild.Execute, useImport, useExport);

    export.Ap.Number = useExport.Ap.Number;
  }

  private void UseSiGetActiveApsForCase()
  {
    var useImport = new SiGetActiveApsForCase.Import();
    var useExport = new SiGetActiveApsForCase.Export();

    useImport.Case1.Number = import.Case1.Number;

    Call(SiGetActiveApsForCase.Execute, useImport, useExport);

    useExport.Grp.CopyTo(local.Grp, MoveGrp2);
  }

  private void UseSiGetApsForCase()
  {
    var useImport = new SiGetApsForCase.Import();
    var useExport = new SiGetApsForCase.Export();

    useImport.Case1.Number = import.Case1.Number;

    Call(SiGetApsForCase.Execute, useImport, useExport);

    useExport.Grp.CopyTo(local.Grp, MoveGrp1);
  }

  private void UseSiReadCaseHeaderInfo2()
  {
    var useImport = new SiReadCaseHeaderInfo2.Import();
    var useExport = new SiReadCaseHeaderInfo2.Export();

    useImport.Ap.Number = import.Ap.Number;
    useImport.Case1.Number = import.Case1.Number;
    useImport.Ar.Number = export.ArCsePersonsWorkSet.Number;

    Call(SiReadCaseHeaderInfo2.Execute, useImport, useExport);

    local.MultipleAps.Flag = useExport.MultipleAps.Flag;
    local.AbendData.Assign(useExport.AbendData);
    export.CaseOpen.Flag = useExport.CaseOpen.Flag;
    export.HiddenAe.Flag = useExport.Ae.Flag;
    MoveCsePersonsWorkSet(useExport.Ar, export.ArCsePersonsWorkSet);
    export.Ap.Assign(useExport.Ap);
  }

  private void UseSiReadCaseHeaderInformation()
  {
    var useImport = new SiReadCaseHeaderInformation.Import();
    var useExport = new SiReadCaseHeaderInformation.Export();

    useImport.Ap.Number = export.Ap.Number;
    useImport.Case1.Number = export.Case1.Number;

    Call(SiReadCaseHeaderInformation.Execute, useImport, useExport);

    local.MultipleAps.Flag = useExport.MultipleAps.Flag;
    local.AbendData.Assign(useExport.AbendData);
    export.CaseOpen.Flag = useExport.CaseOpen.Flag;
    export.HiddenAe.Flag = useExport.Ae.Flag;
    MoveCsePersonsWorkSet(useExport.Ar, export.ArCsePersonsWorkSet);
    export.Ap.Assign(useExport.Ap);
  }

  private void UseSiReadOfficeOspHeader()
  {
    var useImport = new SiReadOfficeOspHeader.Import();
    var useExport = new SiReadOfficeOspHeader.Export();

    useImport.Case1.Number = export.Case1.Number;

    Call(SiReadOfficeOspHeader.Execute, useImport, useExport);

    export.HeaderLine.Text35 = useExport.HeaderLine.Text35;
    MoveOffice(useExport.Office, export.Office);
    export.ServiceProvider.LastName = useExport.ServiceProvider.LastName;
  }

  private void UseSpCabDetermineInterstateDoc()
  {
    var useImport = new SpCabDetermineInterstateDoc.Import();
    var useExport = new SpCabDetermineInterstateDoc.Export();

    useImport.FindDoc.Flag = local.FindDoc.Flag;
    MoveSpDocKey(local.SpDocKey, useImport.SpDocKey);
    useImport.Document.Name = local.Document.Name;

    Call(SpCabDetermineInterstateDoc.Execute, useImport, useExport);
  }

  private bool ReadCsePerson()
  {
    entities.KeyOnly.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", export.ArCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.KeyOnly.Number = db.GetString(reader, 0);
        entities.KeyOnly.Type1 = db.GetString(reader, 1);
        entities.KeyOnly.Populated = true;
        CheckValid<CsePerson>("Type1", entities.KeyOnly.Type1);
      });
  }

  private bool ReadPersonProgram()
  {
    entities.PersonProgram.Populated = false;

    return Read("ReadPersonProgram",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", export.Case1.Number);
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.PersonProgram.Populated = true;
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
    /// <summary>A InterstateGroup group.</summary>
    [Serializable]
    public class InterstateGroup
    {
      /// <summary>
      /// A value of Interstate1.
      /// </summary>
      [JsonPropertyName("interstate1")]
      public TextWorkArea Interstate1
      {
        get => interstate1 ??= new();
        set => interstate1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private TextWorkArea interstate1;
    }

    /// <summary>A ZdelImportGroupIsGroup group.</summary>
    [Serializable]
    public class ZdelImportGroupIsGroup
    {
      /// <summary>
      /// A value of ZdelImportGrpIsState.
      /// </summary>
      [JsonPropertyName("zdelImportGrpIsState")]
      public Fips ZdelImportGrpIsState
      {
        get => zdelImportGrpIsState ??= new();
        set => zdelImportGrpIsState = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private Fips zdelImportGrpIsState;
    }

    /// <summary>A HiddenGrpNcGroup group.</summary>
    [Serializable]
    public class HiddenGrpNcGroup
    {
      /// <summary>
      /// A value of HiddenGrpNcDetNonCooperation.
      /// </summary>
      [JsonPropertyName("hiddenGrpNcDetNonCooperation")]
      public NonCooperation HiddenGrpNcDetNonCooperation
      {
        get => hiddenGrpNcDetNonCooperation ??= new();
        set => hiddenGrpNcDetNonCooperation = value;
      }

      /// <summary>
      /// A value of HiddenGrpNcDetCommon.
      /// </summary>
      [JsonPropertyName("hiddenGrpNcDetCommon")]
      public Common HiddenGrpNcDetCommon
      {
        get => hiddenGrpNcDetCommon ??= new();
        set => hiddenGrpNcDetCommon = value;
      }

      /// <summary>
      /// A value of HiddenGrpNcDetApCaseRole.
      /// </summary>
      [JsonPropertyName("hiddenGrpNcDetApCaseRole")]
      public CaseRole HiddenGrpNcDetApCaseRole
      {
        get => hiddenGrpNcDetApCaseRole ??= new();
        set => hiddenGrpNcDetApCaseRole = value;
      }

      /// <summary>
      /// A value of HiddenGrpNcDetApCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("hiddenGrpNcDetApCsePersonsWorkSet")]
      public CsePersonsWorkSet HiddenGrpNcDetApCsePersonsWorkSet
      {
        get => hiddenGrpNcDetApCsePersonsWorkSet ??= new();
        set => hiddenGrpNcDetApCsePersonsWorkSet = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 45;

      private NonCooperation hiddenGrpNcDetNonCooperation;
      private Common hiddenGrpNcDetCommon;
      private CaseRole hiddenGrpNcDetApCaseRole;
      private CsePersonsWorkSet hiddenGrpNcDetApCsePersonsWorkSet;
    }

    /// <summary>A HiddenGrpGcGroup group.</summary>
    [Serializable]
    public class HiddenGrpGcGroup
    {
      /// <summary>
      /// A value of HiddenGrpGcDetGoodCause.
      /// </summary>
      [JsonPropertyName("hiddenGrpGcDetGoodCause")]
      public GoodCause HiddenGrpGcDetGoodCause
      {
        get => hiddenGrpGcDetGoodCause ??= new();
        set => hiddenGrpGcDetGoodCause = value;
      }

      /// <summary>
      /// A value of HiddenGrpGcDetCommon.
      /// </summary>
      [JsonPropertyName("hiddenGrpGcDetCommon")]
      public Common HiddenGrpGcDetCommon
      {
        get => hiddenGrpGcDetCommon ??= new();
        set => hiddenGrpGcDetCommon = value;
      }

      /// <summary>
      /// A value of HiddenGrpGcDetApCaseRole.
      /// </summary>
      [JsonPropertyName("hiddenGrpGcDetApCaseRole")]
      public CaseRole HiddenGrpGcDetApCaseRole
      {
        get => hiddenGrpGcDetApCaseRole ??= new();
        set => hiddenGrpGcDetApCaseRole = value;
      }

      /// <summary>
      /// A value of HiddenGrpGcDetApCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("hiddenGrpGcDetApCsePersonsWorkSet")]
      public CsePersonsWorkSet HiddenGrpGcDetApCsePersonsWorkSet
      {
        get => hiddenGrpGcDetApCsePersonsWorkSet ??= new();
        set => hiddenGrpGcDetApCsePersonsWorkSet = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 45;

      private GoodCause hiddenGrpGcDetGoodCause;
      private Common hiddenGrpGcDetCommon;
      private CaseRole hiddenGrpGcDetApCaseRole;
      private CsePersonsWorkSet hiddenGrpGcDetApCsePersonsWorkSet;
    }

    /// <summary>A NcGroup group.</summary>
    [Serializable]
    public class NcGroup
    {
      /// <summary>
      /// A value of NcRsnPrompt.
      /// </summary>
      [JsonPropertyName("ncRsnPrompt")]
      public Common NcRsnPrompt
      {
        get => ncRsnPrompt ??= new();
        set => ncRsnPrompt = value;
      }

      /// <summary>
      /// A value of NcCodePrompt.
      /// </summary>
      [JsonPropertyName("ncCodePrompt")]
      public Common NcCodePrompt
      {
        get => ncCodePrompt ??= new();
        set => ncCodePrompt = value;
      }

      /// <summary>
      /// A value of NcDetCommon.
      /// </summary>
      [JsonPropertyName("ncDetCommon")]
      public Common NcDetCommon
      {
        get => ncDetCommon ??= new();
        set => ncDetCommon = value;
      }

      /// <summary>
      /// A value of NcDetApCaseRole.
      /// </summary>
      [JsonPropertyName("ncDetApCaseRole")]
      public CaseRole NcDetApCaseRole
      {
        get => ncDetApCaseRole ??= new();
        set => ncDetApCaseRole = value;
      }

      /// <summary>
      /// A value of NcDetApCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("ncDetApCsePersonsWorkSet")]
      public CsePersonsWorkSet NcDetApCsePersonsWorkSet
      {
        get => ncDetApCsePersonsWorkSet ??= new();
        set => ncDetApCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of NcDetNonCooperation.
      /// </summary>
      [JsonPropertyName("ncDetNonCooperation")]
      public NonCooperation NcDetNonCooperation
      {
        get => ncDetNonCooperation ??= new();
        set => ncDetNonCooperation = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private Common ncRsnPrompt;
      private Common ncCodePrompt;
      private Common ncDetCommon;
      private CaseRole ncDetApCaseRole;
      private CsePersonsWorkSet ncDetApCsePersonsWorkSet;
      private NonCooperation ncDetNonCooperation;
    }

    /// <summary>A GcGroup group.</summary>
    [Serializable]
    public class GcGroup
    {
      /// <summary>
      /// A value of GcCodePrompt.
      /// </summary>
      [JsonPropertyName("gcCodePrompt")]
      public Common GcCodePrompt
      {
        get => gcCodePrompt ??= new();
        set => gcCodePrompt = value;
      }

      /// <summary>
      /// A value of GcDetCommon.
      /// </summary>
      [JsonPropertyName("gcDetCommon")]
      public Common GcDetCommon
      {
        get => gcDetCommon ??= new();
        set => gcDetCommon = value;
      }

      /// <summary>
      /// A value of GcDetApCaseRole.
      /// </summary>
      [JsonPropertyName("gcDetApCaseRole")]
      public CaseRole GcDetApCaseRole
      {
        get => gcDetApCaseRole ??= new();
        set => gcDetApCaseRole = value;
      }

      /// <summary>
      /// A value of GcDetApCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("gcDetApCsePersonsWorkSet")]
      public CsePersonsWorkSet GcDetApCsePersonsWorkSet
      {
        get => gcDetApCsePersonsWorkSet ??= new();
        set => gcDetApCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of GcDetGoodCause.
      /// </summary>
      [JsonPropertyName("gcDetGoodCause")]
      public GoodCause GcDetGoodCause
      {
        get => gcDetGoodCause ??= new();
        set => gcDetGoodCause = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private Common gcCodePrompt;
      private Common gcDetCommon;
      private CaseRole gcDetApCaseRole;
      private CsePersonsWorkSet gcDetApCsePersonsWorkSet;
      private GoodCause gcDetGoodCause;
    }

    /// <summary>
    /// A value of NoJurisdictionPrompt.
    /// </summary>
    [JsonPropertyName("noJurisdictionPrompt")]
    public Common NoJurisdictionPrompt
    {
      get => noJurisdictionPrompt ??= new();
      set => noJurisdictionPrompt = value;
    }

    /// <summary>
    /// A value of Original.
    /// </summary>
    [JsonPropertyName("original")]
    public Case1 Original
    {
      get => original ??= new();
      set => original = value;
    }

    /// <summary>
    /// A value of SuccessfullyDisplayed.
    /// </summary>
    [JsonPropertyName("successfullyDisplayed")]
    public Common SuccessfullyDisplayed
    {
      get => successfullyDisplayed ??= new();
      set => successfullyDisplayed = value;
    }

    /// <summary>
    /// Gets a value of Interstate.
    /// </summary>
    [JsonIgnore]
    public Array<InterstateGroup> Interstate => interstate ??= new(
      InterstateGroup.Capacity);

    /// <summary>
    /// Gets a value of Interstate for json serialization.
    /// </summary>
    [JsonPropertyName("interstate")]
    [Computed]
    public IList<InterstateGroup> Interstate_Json
    {
      get => interstate;
      set => Interstate.Assign(value);
    }

    /// <summary>
    /// A value of DuplicateIndPrompt.
    /// </summary>
    [JsonPropertyName("duplicateIndPrompt")]
    public Common DuplicateIndPrompt
    {
      get => duplicateIndPrompt ??= new();
      set => duplicateIndPrompt = value;
    }

    /// <summary>
    /// Gets a value of ZdelImportGroupIs.
    /// </summary>
    [JsonIgnore]
    public Array<ZdelImportGroupIsGroup> ZdelImportGroupIs =>
      zdelImportGroupIs ??= new(ZdelImportGroupIsGroup.Capacity);

    /// <summary>
    /// Gets a value of ZdelImportGroupIs for json serialization.
    /// </summary>
    [JsonPropertyName("zdelImportGroupIs")]
    [Computed]
    public IList<ZdelImportGroupIsGroup> ZdelImportGroupIs_Json
    {
      get => zdelImportGroupIs;
      set => ZdelImportGroupIs.Assign(value);
    }

    /// <summary>
    /// A value of CaseSuccessfullyClosed.
    /// </summary>
    [JsonPropertyName("caseSuccessfullyClosed")]
    public Common CaseSuccessfullyClosed
    {
      get => caseSuccessfullyClosed ??= new();
      set => caseSuccessfullyClosed = value;
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
    /// A value of CaseClosureRsnPrompt.
    /// </summary>
    [JsonPropertyName("caseClosureRsnPrompt")]
    public Common CaseClosureRsnPrompt
    {
      get => caseClosureRsnPrompt ??= new();
      set => caseClosureRsnPrompt = value;
    }

    /// <summary>
    /// A value of HiddenRedisplay.
    /// </summary>
    [JsonPropertyName("hiddenRedisplay")]
    public Common HiddenRedisplay
    {
      get => hiddenRedisplay ??= new();
      set => hiddenRedisplay = value;
    }

    /// <summary>
    /// A value of PaMedCdPrompt.
    /// </summary>
    [JsonPropertyName("paMedCdPrompt")]
    public Common PaMedCdPrompt
    {
      get => paMedCdPrompt ??= new();
      set => paMedCdPrompt = value;
    }

    /// <summary>
    /// A value of TermCodePrompt.
    /// </summary>
    [JsonPropertyName("termCodePrompt")]
    public Common TermCodePrompt
    {
      get => termCodePrompt ??= new();
      set => termCodePrompt = value;
    }

    /// <summary>
    /// A value of DesignatedPayeeFnd.
    /// </summary>
    [JsonPropertyName("designatedPayeeFnd")]
    public Common DesignatedPayeeFnd
    {
      get => designatedPayeeFnd ??= new();
      set => designatedPayeeFnd = value;
    }

    /// <summary>
    /// A value of Gc1.
    /// </summary>
    [JsonPropertyName("gc1")]
    public Standard Gc1
    {
      get => gc1 ??= new();
      set => gc1 = value;
    }

    /// <summary>
    /// A value of Nc1.
    /// </summary>
    [JsonPropertyName("nc1")]
    public Standard Nc1
    {
      get => nc1 ??= new();
      set => nc1 = value;
    }

    /// <summary>
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public Case1 Prev
    {
      get => prev ??= new();
      set => prev = value;
    }

    /// <summary>
    /// A value of ArPrev.
    /// </summary>
    [JsonPropertyName("arPrev")]
    public CaseRole ArPrev
    {
      get => arPrev ??= new();
      set => arPrev = value;
    }

    /// <summary>
    /// A value of NoItemsNc.
    /// </summary>
    [JsonPropertyName("noItemsNc")]
    public Common NoItemsNc
    {
      get => noItemsNc ??= new();
      set => noItemsNc = value;
    }

    /// <summary>
    /// A value of NoItemsGc.
    /// </summary>
    [JsonPropertyName("noItemsGc")]
    public Common NoItemsGc
    {
      get => noItemsGc ??= new();
      set => noItemsGc = value;
    }

    /// <summary>
    /// A value of CurrItmNoGc.
    /// </summary>
    [JsonPropertyName("currItmNoGc")]
    public Common CurrItmNoGc
    {
      get => currItmNoGc ??= new();
      set => currItmNoGc = value;
    }

    /// <summary>
    /// A value of CurrItmNoNc.
    /// </summary>
    [JsonPropertyName("currItmNoNc")]
    public Common CurrItmNoNc
    {
      get => currItmNoNc ??= new();
      set => currItmNoNc = value;
    }

    /// <summary>
    /// A value of HidNoItemsFndNc.
    /// </summary>
    [JsonPropertyName("hidNoItemsFndNc")]
    public Common HidNoItemsFndNc
    {
      get => hidNoItemsFndNc ??= new();
      set => hidNoItemsFndNc = value;
    }

    /// <summary>
    /// A value of HidNoItemsFndGc.
    /// </summary>
    [JsonPropertyName("hidNoItemsFndGc")]
    public Common HidNoItemsFndGc
    {
      get => hidNoItemsFndGc ??= new();
      set => hidNoItemsFndGc = value;
    }

    /// <summary>
    /// A value of MaxPagesGc.
    /// </summary>
    [JsonPropertyName("maxPagesGc")]
    public Common MaxPagesGc
    {
      get => maxPagesGc ??= new();
      set => maxPagesGc = value;
    }

    /// <summary>
    /// A value of MaxPagesNc.
    /// </summary>
    [JsonPropertyName("maxPagesNc")]
    public Common MaxPagesNc
    {
      get => maxPagesNc ??= new();
      set => maxPagesNc = value;
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
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
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
    /// A value of NcMinus.
    /// </summary>
    [JsonPropertyName("ncMinus")]
    public Standard NcMinus
    {
      get => ncMinus ??= new();
      set => ncMinus = value;
    }

    /// <summary>
    /// A value of NcPlus.
    /// </summary>
    [JsonPropertyName("ncPlus")]
    public Standard NcPlus
    {
      get => ncPlus ??= new();
      set => ncPlus = value;
    }

    /// <summary>
    /// A value of GcMinus.
    /// </summary>
    [JsonPropertyName("gcMinus")]
    public Standard GcMinus
    {
      get => gcMinus ??= new();
      set => gcMinus = value;
    }

    /// <summary>
    /// A value of GcPlus.
    /// </summary>
    [JsonPropertyName("gcPlus")]
    public Standard GcPlus
    {
      get => gcPlus ??= new();
      set => gcPlus = value;
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
    /// A value of ArCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("arCsePersonsWorkSet")]
    public CsePersonsWorkSet ArCsePersonsWorkSet
    {
      get => arCsePersonsWorkSet ??= new();
      set => arCsePersonsWorkSet = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of HiddenPrev.
    /// </summary>
    [JsonPropertyName("hiddenPrev")]
    public CsePersonsWorkSet HiddenPrev
    {
      get => hiddenPrev ??= new();
      set => hiddenPrev = value;
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
    /// A value of MedProgExists.
    /// </summary>
    [JsonPropertyName("medProgExists")]
    public Common MedProgExists
    {
      get => medProgExists ??= new();
      set => medProgExists = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
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
    /// Gets a value of HiddenGrpNc.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenGrpNcGroup> HiddenGrpNc => hiddenGrpNc ??= new(
      HiddenGrpNcGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenGrpNc for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenGrpNc")]
    [Computed]
    public IList<HiddenGrpNcGroup> HiddenGrpNc_Json
    {
      get => hiddenGrpNc;
      set => HiddenGrpNc.Assign(value);
    }

    /// <summary>
    /// Gets a value of HiddenGrpGc.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenGrpGcGroup> HiddenGrpGc => hiddenGrpGc ??= new(
      HiddenGrpGcGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenGrpGc for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenGrpGc")]
    [Computed]
    public IList<HiddenGrpGcGroup> HiddenGrpGc_Json
    {
      get => hiddenGrpGc;
      set => HiddenGrpGc.Assign(value);
    }

    /// <summary>
    /// Gets a value of Nc.
    /// </summary>
    [JsonIgnore]
    public Array<NcGroup> Nc => nc ??= new(NcGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Nc for json serialization.
    /// </summary>
    [JsonPropertyName("nc")]
    [Computed]
    public IList<NcGroup> Nc_Json
    {
      get => nc;
      set => Nc.Assign(value);
    }

    /// <summary>
    /// Gets a value of Gc.
    /// </summary>
    [JsonIgnore]
    public Array<GcGroup> Gc => gc ??= new(GcGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Gc for json serialization.
    /// </summary>
    [JsonPropertyName("gc")]
    [Computed]
    public IList<GcGroup> Gc_Json
    {
      get => gc;
      set => Gc.Assign(value);
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

    /// <summary>
    /// A value of CaseFuncWorkSet.
    /// </summary>
    [JsonPropertyName("caseFuncWorkSet")]
    public CaseFuncWorkSet CaseFuncWorkSet
    {
      get => caseFuncWorkSet ??= new();
      set => caseFuncWorkSet = value;
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
    /// A value of ArPrompt.
    /// </summary>
    [JsonPropertyName("arPrompt")]
    public Common ArPrompt
    {
      get => arPrompt ??= new();
      set => arPrompt = value;
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
    /// A value of FromCads.
    /// </summary>
    [JsonPropertyName("fromCads")]
    public Common FromCads
    {
      get => fromCads ??= new();
      set => fromCads = value;
    }

    private Common noJurisdictionPrompt;
    private Case1 original;
    private Common successfullyDisplayed;
    private Array<InterstateGroup> interstate;
    private Common duplicateIndPrompt;
    private Array<ZdelImportGroupIsGroup> zdelImportGroupIs;
    private Common caseSuccessfullyClosed;
    private Common caseOpen;
    private Common caseClosureRsnPrompt;
    private Common hiddenRedisplay;
    private Common paMedCdPrompt;
    private Common termCodePrompt;
    private Common designatedPayeeFnd;
    private Standard gc1;
    private Standard nc1;
    private Case1 prev;
    private CaseRole arPrev;
    private Common noItemsNc;
    private Common noItemsGc;
    private Common currItmNoGc;
    private Common currItmNoNc;
    private Common hidNoItemsFndNc;
    private Common hidNoItemsFndGc;
    private Common maxPagesGc;
    private Common maxPagesNc;
    private InterstateRequest interstateRequest;
    private Program program;
    private CaseRole arCaseRole;
    private Standard ncMinus;
    private Standard ncPlus;
    private Standard gcMinus;
    private Standard gcPlus;
    private CodeValue selected;
    private Common apPrompt;
    private Common hiddenAe;
    private CsePersonsWorkSet arCsePersonsWorkSet;
    private CsePersonsWorkSet ap;
    private Case1 case1;
    private CsePersonsWorkSet hiddenPrev;
    private Case1 next;
    private Common medProgExists;
    private CsePersonsWorkSet selectedAp;
    private Standard standard;
    private Office office;
    private ServiceProvider serviceProvider;
    private Array<HiddenGrpNcGroup> hiddenGrpNc;
    private Array<HiddenGrpGcGroup> hiddenGrpGc;
    private Array<NcGroup> nc;
    private Array<GcGroup> gc;
    private NextTranInfo hiddenNextTranInfo;
    private Case1 hiddenCase;
    private CaseFuncWorkSet caseFuncWorkSet;
    private WorkArea headerLine;
    private Common arPrompt;
    private CsePersonsWorkSet selectedAr;
    private Common fromCads;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A InterstateGroup group.</summary>
    [Serializable]
    public class InterstateGroup
    {
      /// <summary>
      /// A value of Interstate1.
      /// </summary>
      [JsonPropertyName("interstate1")]
      public TextWorkArea Interstate1
      {
        get => interstate1 ??= new();
        set => interstate1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private TextWorkArea interstate1;
    }

    /// <summary>A ZdelExportGroupIsGroup group.</summary>
    [Serializable]
    public class ZdelExportGroupIsGroup
    {
      /// <summary>
      /// A value of ZdelExportGrpIsState.
      /// </summary>
      [JsonPropertyName("zdelExportGrpIsState")]
      public Fips ZdelExportGrpIsState
      {
        get => zdelExportGrpIsState ??= new();
        set => zdelExportGrpIsState = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private Fips zdelExportGrpIsState;
    }

    /// <summary>A HiddenGrpNcGroup group.</summary>
    [Serializable]
    public class HiddenGrpNcGroup
    {
      /// <summary>
      /// A value of HiddenGrpNcDetNonCooperation.
      /// </summary>
      [JsonPropertyName("hiddenGrpNcDetNonCooperation")]
      public NonCooperation HiddenGrpNcDetNonCooperation
      {
        get => hiddenGrpNcDetNonCooperation ??= new();
        set => hiddenGrpNcDetNonCooperation = value;
      }

      /// <summary>
      /// A value of HiddenGrpNcDetCommon.
      /// </summary>
      [JsonPropertyName("hiddenGrpNcDetCommon")]
      public Common HiddenGrpNcDetCommon
      {
        get => hiddenGrpNcDetCommon ??= new();
        set => hiddenGrpNcDetCommon = value;
      }

      /// <summary>
      /// A value of HiddenGrpNcDetApCaseRole.
      /// </summary>
      [JsonPropertyName("hiddenGrpNcDetApCaseRole")]
      public CaseRole HiddenGrpNcDetApCaseRole
      {
        get => hiddenGrpNcDetApCaseRole ??= new();
        set => hiddenGrpNcDetApCaseRole = value;
      }

      /// <summary>
      /// A value of HiddenGrpNcDetApCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("hiddenGrpNcDetApCsePersonsWorkSet")]
      public CsePersonsWorkSet HiddenGrpNcDetApCsePersonsWorkSet
      {
        get => hiddenGrpNcDetApCsePersonsWorkSet ??= new();
        set => hiddenGrpNcDetApCsePersonsWorkSet = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 45;

      private NonCooperation hiddenGrpNcDetNonCooperation;
      private Common hiddenGrpNcDetCommon;
      private CaseRole hiddenGrpNcDetApCaseRole;
      private CsePersonsWorkSet hiddenGrpNcDetApCsePersonsWorkSet;
    }

    /// <summary>A HiddenGrpGcGroup group.</summary>
    [Serializable]
    public class HiddenGrpGcGroup
    {
      /// <summary>
      /// A value of HiddenGrpGcDetGoodCause.
      /// </summary>
      [JsonPropertyName("hiddenGrpGcDetGoodCause")]
      public GoodCause HiddenGrpGcDetGoodCause
      {
        get => hiddenGrpGcDetGoodCause ??= new();
        set => hiddenGrpGcDetGoodCause = value;
      }

      /// <summary>
      /// A value of HiddenGrpGcDetCommon.
      /// </summary>
      [JsonPropertyName("hiddenGrpGcDetCommon")]
      public Common HiddenGrpGcDetCommon
      {
        get => hiddenGrpGcDetCommon ??= new();
        set => hiddenGrpGcDetCommon = value;
      }

      /// <summary>
      /// A value of HiddenGrpGcDetAp.
      /// </summary>
      [JsonPropertyName("hiddenGrpGcDetAp")]
      public CaseRole HiddenGrpGcDetAp
      {
        get => hiddenGrpGcDetAp ??= new();
        set => hiddenGrpGcDetAp = value;
      }

      /// <summary>
      /// A value of HiddenGrpGcDetCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("hiddenGrpGcDetCsePersonsWorkSet")]
      public CsePersonsWorkSet HiddenGrpGcDetCsePersonsWorkSet
      {
        get => hiddenGrpGcDetCsePersonsWorkSet ??= new();
        set => hiddenGrpGcDetCsePersonsWorkSet = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 45;

      private GoodCause hiddenGrpGcDetGoodCause;
      private Common hiddenGrpGcDetCommon;
      private CaseRole hiddenGrpGcDetAp;
      private CsePersonsWorkSet hiddenGrpGcDetCsePersonsWorkSet;
    }

    /// <summary>A NcGroup group.</summary>
    [Serializable]
    public class NcGroup
    {
      /// <summary>
      /// A value of NcRsnPrompt.
      /// </summary>
      [JsonPropertyName("ncRsnPrompt")]
      public Common NcRsnPrompt
      {
        get => ncRsnPrompt ??= new();
        set => ncRsnPrompt = value;
      }

      /// <summary>
      /// A value of NcCodePrmpt.
      /// </summary>
      [JsonPropertyName("ncCodePrmpt")]
      public Common NcCodePrmpt
      {
        get => ncCodePrmpt ??= new();
        set => ncCodePrmpt = value;
      }

      /// <summary>
      /// A value of NcDetCommon.
      /// </summary>
      [JsonPropertyName("ncDetCommon")]
      public Common NcDetCommon
      {
        get => ncDetCommon ??= new();
        set => ncDetCommon = value;
      }

      /// <summary>
      /// A value of NcDetApCaseRole.
      /// </summary>
      [JsonPropertyName("ncDetApCaseRole")]
      public CaseRole NcDetApCaseRole
      {
        get => ncDetApCaseRole ??= new();
        set => ncDetApCaseRole = value;
      }

      /// <summary>
      /// A value of NcDetApCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("ncDetApCsePersonsWorkSet")]
      public CsePersonsWorkSet NcDetApCsePersonsWorkSet
      {
        get => ncDetApCsePersonsWorkSet ??= new();
        set => ncDetApCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of NcDetNonCooperation.
      /// </summary>
      [JsonPropertyName("ncDetNonCooperation")]
      public NonCooperation NcDetNonCooperation
      {
        get => ncDetNonCooperation ??= new();
        set => ncDetNonCooperation = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private Common ncRsnPrompt;
      private Common ncCodePrmpt;
      private Common ncDetCommon;
      private CaseRole ncDetApCaseRole;
      private CsePersonsWorkSet ncDetApCsePersonsWorkSet;
      private NonCooperation ncDetNonCooperation;
    }

    /// <summary>A GcGroup group.</summary>
    [Serializable]
    public class GcGroup
    {
      /// <summary>
      /// A value of GcCodePrompt.
      /// </summary>
      [JsonPropertyName("gcCodePrompt")]
      public Common GcCodePrompt
      {
        get => gcCodePrompt ??= new();
        set => gcCodePrompt = value;
      }

      /// <summary>
      /// A value of GcDetCommon.
      /// </summary>
      [JsonPropertyName("gcDetCommon")]
      public Common GcDetCommon
      {
        get => gcDetCommon ??= new();
        set => gcDetCommon = value;
      }

      /// <summary>
      /// A value of GcDetApCaseRole.
      /// </summary>
      [JsonPropertyName("gcDetApCaseRole")]
      public CaseRole GcDetApCaseRole
      {
        get => gcDetApCaseRole ??= new();
        set => gcDetApCaseRole = value;
      }

      /// <summary>
      /// A value of GcDetApCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("gcDetApCsePersonsWorkSet")]
      public CsePersonsWorkSet GcDetApCsePersonsWorkSet
      {
        get => gcDetApCsePersonsWorkSet ??= new();
        set => gcDetApCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of GcDetGoodCause.
      /// </summary>
      [JsonPropertyName("gcDetGoodCause")]
      public GoodCause GcDetGoodCause
      {
        get => gcDetGoodCause ??= new();
        set => gcDetGoodCause = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private Common gcCodePrompt;
      private Common gcDetCommon;
      private CaseRole gcDetApCaseRole;
      private CsePersonsWorkSet gcDetApCsePersonsWorkSet;
      private GoodCause gcDetGoodCause;
    }

    /// <summary>
    /// A value of NoJurisdictionPrompt.
    /// </summary>
    [JsonPropertyName("noJurisdictionPrompt")]
    public Common NoJurisdictionPrompt
    {
      get => noJurisdictionPrompt ??= new();
      set => noJurisdictionPrompt = value;
    }

    /// <summary>
    /// A value of Original.
    /// </summary>
    [JsonPropertyName("original")]
    public Case1 Original
    {
      get => original ??= new();
      set => original = value;
    }

    /// <summary>
    /// A value of SuccessfullyDisplayed.
    /// </summary>
    [JsonPropertyName("successfullyDisplayed")]
    public Common SuccessfullyDisplayed
    {
      get => successfullyDisplayed ??= new();
      set => successfullyDisplayed = value;
    }

    /// <summary>
    /// Gets a value of Interstate.
    /// </summary>
    [JsonIgnore]
    public Array<InterstateGroup> Interstate => interstate ??= new(
      InterstateGroup.Capacity);

    /// <summary>
    /// Gets a value of Interstate for json serialization.
    /// </summary>
    [JsonPropertyName("interstate")]
    [Computed]
    public IList<InterstateGroup> Interstate_Json
    {
      get => interstate;
      set => Interstate.Assign(value);
    }

    /// <summary>
    /// A value of DuplicateIndPrompt.
    /// </summary>
    [JsonPropertyName("duplicateIndPrompt")]
    public Common DuplicateIndPrompt
    {
      get => duplicateIndPrompt ??= new();
      set => duplicateIndPrompt = value;
    }

    /// <summary>
    /// Gets a value of ZdelExportGroupIs.
    /// </summary>
    [JsonIgnore]
    public Array<ZdelExportGroupIsGroup> ZdelExportGroupIs =>
      zdelExportGroupIs ??= new(ZdelExportGroupIsGroup.Capacity);

    /// <summary>
    /// Gets a value of ZdelExportGroupIs for json serialization.
    /// </summary>
    [JsonPropertyName("zdelExportGroupIs")]
    [Computed]
    public IList<ZdelExportGroupIsGroup> ZdelExportGroupIs_Json
    {
      get => zdelExportGroupIs;
      set => ZdelExportGroupIs.Assign(value);
    }

    /// <summary>
    /// A value of CaseSuccessfullyClosed.
    /// </summary>
    [JsonPropertyName("caseSuccessfullyClosed")]
    public Common CaseSuccessfullyClosed
    {
      get => caseSuccessfullyClosed ??= new();
      set => caseSuccessfullyClosed = value;
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
    /// A value of CaseClosureRsnPrompt.
    /// </summary>
    [JsonPropertyName("caseClosureRsnPrompt")]
    public Common CaseClosureRsnPrompt
    {
      get => caseClosureRsnPrompt ??= new();
      set => caseClosureRsnPrompt = value;
    }

    /// <summary>
    /// A value of HiddenRedisplay.
    /// </summary>
    [JsonPropertyName("hiddenRedisplay")]
    public Common HiddenRedisplay
    {
      get => hiddenRedisplay ??= new();
      set => hiddenRedisplay = value;
    }

    /// <summary>
    /// A value of PaMedCdPrompt.
    /// </summary>
    [JsonPropertyName("paMedCdPrompt")]
    public Common PaMedCdPrompt
    {
      get => paMedCdPrompt ??= new();
      set => paMedCdPrompt = value;
    }

    /// <summary>
    /// A value of TermCodePrompt.
    /// </summary>
    [JsonPropertyName("termCodePrompt")]
    public Common TermCodePrompt
    {
      get => termCodePrompt ??= new();
      set => termCodePrompt = value;
    }

    /// <summary>
    /// A value of DesignatedPayeeFnd.
    /// </summary>
    [JsonPropertyName("designatedPayeeFnd")]
    public Common DesignatedPayeeFnd
    {
      get => designatedPayeeFnd ??= new();
      set => designatedPayeeFnd = value;
    }

    /// <summary>
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public Case1 Prev
    {
      get => prev ??= new();
      set => prev = value;
    }

    /// <summary>
    /// A value of ArPrev.
    /// </summary>
    [JsonPropertyName("arPrev")]
    public CaseRole ArPrev
    {
      get => arPrev ??= new();
      set => arPrev = value;
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
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    /// <summary>
    /// A value of NoItemsNc.
    /// </summary>
    [JsonPropertyName("noItemsNc")]
    public Common NoItemsNc
    {
      get => noItemsNc ??= new();
      set => noItemsNc = value;
    }

    /// <summary>
    /// A value of NoItemsGc.
    /// </summary>
    [JsonPropertyName("noItemsGc")]
    public Common NoItemsGc
    {
      get => noItemsGc ??= new();
      set => noItemsGc = value;
    }

    /// <summary>
    /// A value of CurrItmNoGc.
    /// </summary>
    [JsonPropertyName("currItmNoGc")]
    public Common CurrItmNoGc
    {
      get => currItmNoGc ??= new();
      set => currItmNoGc = value;
    }

    /// <summary>
    /// A value of CurrItmNoNc.
    /// </summary>
    [JsonPropertyName("currItmNoNc")]
    public Common CurrItmNoNc
    {
      get => currItmNoNc ??= new();
      set => currItmNoNc = value;
    }

    /// <summary>
    /// A value of HidNoItemsFndNc.
    /// </summary>
    [JsonPropertyName("hidNoItemsFndNc")]
    public Common HidNoItemsFndNc
    {
      get => hidNoItemsFndNc ??= new();
      set => hidNoItemsFndNc = value;
    }

    /// <summary>
    /// A value of HidNoItemsFndGc.
    /// </summary>
    [JsonPropertyName("hidNoItemsFndGc")]
    public Common HidNoItemsFndGc
    {
      get => hidNoItemsFndGc ??= new();
      set => hidNoItemsFndGc = value;
    }

    /// <summary>
    /// A value of MaxPagesGc.
    /// </summary>
    [JsonPropertyName("maxPagesGc")]
    public Common MaxPagesGc
    {
      get => maxPagesGc ??= new();
      set => maxPagesGc = value;
    }

    /// <summary>
    /// A value of MaxPagesNc.
    /// </summary>
    [JsonPropertyName("maxPagesNc")]
    public Common MaxPagesNc
    {
      get => maxPagesNc ??= new();
      set => maxPagesNc = value;
    }

    /// <summary>
    /// A value of Gc1.
    /// </summary>
    [JsonPropertyName("gc1")]
    public Standard Gc1
    {
      get => gc1 ??= new();
      set => gc1 = value;
    }

    /// <summary>
    /// A value of Nc1.
    /// </summary>
    [JsonPropertyName("nc1")]
    public Standard Nc1
    {
      get => nc1 ??= new();
      set => nc1 = value;
    }

    /// <summary>
    /// A value of SaveGc.
    /// </summary>
    [JsonPropertyName("saveGc")]
    public CsePerson SaveGc
    {
      get => saveGc ??= new();
      set => saveGc = value;
    }

    /// <summary>
    /// A value of SaveNc.
    /// </summary>
    [JsonPropertyName("saveNc")]
    public CsePerson SaveNc
    {
      get => saveNc ??= new();
      set => saveNc = value;
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
    /// A value of NcMinus.
    /// </summary>
    [JsonPropertyName("ncMinus")]
    public Standard NcMinus
    {
      get => ncMinus ??= new();
      set => ncMinus = value;
    }

    /// <summary>
    /// A value of NcPlus.
    /// </summary>
    [JsonPropertyName("ncPlus")]
    public Standard NcPlus
    {
      get => ncPlus ??= new();
      set => ncPlus = value;
    }

    /// <summary>
    /// A value of GcMinus.
    /// </summary>
    [JsonPropertyName("gcMinus")]
    public Standard GcMinus
    {
      get => gcMinus ??= new();
      set => gcMinus = value;
    }

    /// <summary>
    /// A value of GcPlus.
    /// </summary>
    [JsonPropertyName("gcPlus")]
    public Standard GcPlus
    {
      get => gcPlus ??= new();
      set => gcPlus = value;
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
    /// A value of ArCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("arCsePersonsWorkSet")]
    public CsePersonsWorkSet ArCsePersonsWorkSet
    {
      get => arCsePersonsWorkSet ??= new();
      set => arCsePersonsWorkSet = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of HiddenPrev.
    /// </summary>
    [JsonPropertyName("hiddenPrev")]
    public CsePersonsWorkSet HiddenPrev
    {
      get => hiddenPrev ??= new();
      set => hiddenPrev = value;
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
    /// A value of MedProgExists.
    /// </summary>
    [JsonPropertyName("medProgExists")]
    public Common MedProgExists
    {
      get => medProgExists ??= new();
      set => medProgExists = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
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
    /// Gets a value of HiddenGrpNc.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenGrpNcGroup> HiddenGrpNc => hiddenGrpNc ??= new(
      HiddenGrpNcGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenGrpNc for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenGrpNc")]
    [Computed]
    public IList<HiddenGrpNcGroup> HiddenGrpNc_Json
    {
      get => hiddenGrpNc;
      set => HiddenGrpNc.Assign(value);
    }

    /// <summary>
    /// Gets a value of HiddenGrpGc.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenGrpGcGroup> HiddenGrpGc => hiddenGrpGc ??= new(
      HiddenGrpGcGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenGrpGc for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenGrpGc")]
    [Computed]
    public IList<HiddenGrpGcGroup> HiddenGrpGc_Json
    {
      get => hiddenGrpGc;
      set => HiddenGrpGc.Assign(value);
    }

    /// <summary>
    /// Gets a value of Nc.
    /// </summary>
    [JsonIgnore]
    public Array<NcGroup> Nc => nc ??= new(NcGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Nc for json serialization.
    /// </summary>
    [JsonPropertyName("nc")]
    [Computed]
    public IList<NcGroup> Nc_Json
    {
      get => nc;
      set => Nc.Assign(value);
    }

    /// <summary>
    /// Gets a value of Gc.
    /// </summary>
    [JsonIgnore]
    public Array<GcGroup> Gc => gc ??= new(GcGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Gc for json serialization.
    /// </summary>
    [JsonPropertyName("gc")]
    [Computed]
    public IList<GcGroup> Gc_Json
    {
      get => gc;
      set => Gc.Assign(value);
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

    /// <summary>
    /// A value of CaseFuncWorkSet.
    /// </summary>
    [JsonPropertyName("caseFuncWorkSet")]
    public CaseFuncWorkSet CaseFuncWorkSet
    {
      get => caseFuncWorkSet ??= new();
      set => caseFuncWorkSet = value;
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
    /// A value of ArPrompt.
    /// </summary>
    [JsonPropertyName("arPrompt")]
    public Common ArPrompt
    {
      get => arPrompt ??= new();
      set => arPrompt = value;
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
    /// A value of FromCads.
    /// </summary>
    [JsonPropertyName("fromCads")]
    public Common FromCads
    {
      get => fromCads ??= new();
      set => fromCads = value;
    }

    private Common noJurisdictionPrompt;
    private Case1 original;
    private Common successfullyDisplayed;
    private Array<InterstateGroup> interstate;
    private Common duplicateIndPrompt;
    private Array<ZdelExportGroupIsGroup> zdelExportGroupIs;
    private Common caseSuccessfullyClosed;
    private Common caseOpen;
    private Common caseClosureRsnPrompt;
    private Common hiddenRedisplay;
    private Common paMedCdPrompt;
    private Common termCodePrompt;
    private Common designatedPayeeFnd;
    private Case1 prev;
    private CaseRole arPrev;
    private InterstateRequest interstateRequest;
    private Program program;
    private Common noItemsNc;
    private Common noItemsGc;
    private Common currItmNoGc;
    private Common currItmNoNc;
    private Common hidNoItemsFndNc;
    private Common hidNoItemsFndGc;
    private Common maxPagesGc;
    private Common maxPagesNc;
    private Standard gc1;
    private Standard nc1;
    private CsePerson saveGc;
    private CsePerson saveNc;
    private CaseRole arCaseRole;
    private Standard ncMinus;
    private Standard ncPlus;
    private Standard gcMinus;
    private Standard gcPlus;
    private Code prompt;
    private Common apPrompt;
    private Common hiddenAe;
    private CsePersonsWorkSet arCsePersonsWorkSet;
    private CsePersonsWorkSet ap;
    private Case1 case1;
    private CsePersonsWorkSet hiddenPrev;
    private Case1 next;
    private Common medProgExists;
    private CsePersonsWorkSet selectedAp;
    private Standard standard;
    private Office office;
    private ServiceProvider serviceProvider;
    private Array<HiddenGrpNcGroup> hiddenGrpNc;
    private Array<HiddenGrpGcGroup> hiddenGrpGc;
    private Array<NcGroup> nc;
    private Array<GcGroup> gc;
    private NextTranInfo hiddenNextTranInfo;
    private Case1 hiddenCase;
    private CaseFuncWorkSet caseFuncWorkSet;
    private WorkArea headerLine;
    private Common arPrompt;
    private CsePersonsWorkSet selectedAr;
    private Common fromCads;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A GrpGroup group.</summary>
    [Serializable]
    public class GrpGroup
    {
      /// <summary>
      /// A value of Ap.
      /// </summary>
      [JsonPropertyName("ap")]
      public CsePerson Ap
      {
        get => ap ??= new();
        set => ap = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 30;

      private CsePerson ap;
    }

    /// <summary>
    /// A value of NullNonCooperation.
    /// </summary>
    [JsonPropertyName("nullNonCooperation")]
    public NonCooperation NullNonCooperation
    {
      get => nullNonCooperation ??= new();
      set => nullNonCooperation = value;
    }

    /// <summary>
    /// A value of NullGoodCause.
    /// </summary>
    [JsonPropertyName("nullGoodCause")]
    public GoodCause NullGoodCause
    {
      get => nullGoodCause ??= new();
      set => nullGoodCause = value;
    }

    /// <summary>
    /// A value of NullCaseRole.
    /// </summary>
    [JsonPropertyName("nullCaseRole")]
    public CaseRole NullCaseRole
    {
      get => nullCaseRole ??= new();
      set => nullCaseRole = value;
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
    /// A value of ClosureDatePlus60Days.
    /// </summary>
    [JsonPropertyName("closureDatePlus60Days")]
    public DateWorkArea ClosureDatePlus60Days
    {
      get => closureDatePlus60Days ??= new();
      set => closureDatePlus60Days = value;
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
    /// A value of IncommingInterstateFound.
    /// </summary>
    [JsonPropertyName("incommingInterstateFound")]
    public Common IncommingInterstateFound
    {
      get => incommingInterstateFound ??= new();
      set => incommingInterstateFound = value;
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
    /// A value of ScreenIdentification.
    /// </summary>
    [JsonPropertyName("screenIdentification")]
    public Common ScreenIdentification
    {
      get => screenIdentification ??= new();
      set => screenIdentification = value;
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
    /// Gets a value of Grp.
    /// </summary>
    [JsonIgnore]
    public Array<GrpGroup> Grp => grp ??= new(GrpGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Grp for json serialization.
    /// </summary>
    [JsonPropertyName("grp")]
    [Computed]
    public IList<GrpGroup> Grp_Json
    {
      get => grp;
      set => Grp.Assign(value);
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
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
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
    /// A value of NoActiveProgramInd.
    /// </summary>
    [JsonPropertyName("noActiveProgramInd")]
    public Common NoActiveProgramInd
    {
      get => noActiveProgramInd ??= new();
      set => noActiveProgramInd = value;
    }

    /// <summary>
    /// A value of NoAps.
    /// </summary>
    [JsonPropertyName("noAps")]
    public Common NoAps
    {
      get => noAps ??= new();
      set => noAps = value;
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
    /// A value of ClearCommon.
    /// </summary>
    [JsonPropertyName("clearCommon")]
    public Common ClearCommon
    {
      get => clearCommon ??= new();
      set => clearCommon = value;
    }

    /// <summary>
    /// A value of ClearCaseRole.
    /// </summary>
    [JsonPropertyName("clearCaseRole")]
    public CaseRole ClearCaseRole
    {
      get => clearCaseRole ??= new();
      set => clearCaseRole = value;
    }

    /// <summary>
    /// A value of ClearCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("clearCsePersonsWorkSet")]
    public CsePersonsWorkSet ClearCsePersonsWorkSet
    {
      get => clearCsePersonsWorkSet ??= new();
      set => clearCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Row1AddFuncNc.
    /// </summary>
    [JsonPropertyName("row1AddFuncNc")]
    public Common Row1AddFuncNc
    {
      get => row1AddFuncNc ??= new();
      set => row1AddFuncNc = value;
    }

    /// <summary>
    /// A value of Row1AddFuncGc.
    /// </summary>
    [JsonPropertyName("row1AddFuncGc")]
    public Common Row1AddFuncGc
    {
      get => row1AddFuncGc ??= new();
      set => row1AddFuncGc = value;
    }

    /// <summary>
    /// A value of Count.
    /// </summary>
    [JsonPropertyName("count")]
    public Common Count
    {
      get => count ??= new();
      set => count = value;
    }

    /// <summary>
    /// A value of DataChanged.
    /// </summary>
    [JsonPropertyName("dataChanged")]
    public Common DataChanged
    {
      get => dataChanged ??= new();
      set => dataChanged = value;
    }

    /// <summary>
    /// A value of ItemsFndNc.
    /// </summary>
    [JsonPropertyName("itemsFndNc")]
    public Common ItemsFndNc
    {
      get => itemsFndNc ??= new();
      set => itemsFndNc = value;
    }

    /// <summary>
    /// A value of ItemsFndGc.
    /// </summary>
    [JsonPropertyName("itemsFndGc")]
    public Common ItemsFndGc
    {
      get => itemsFndGc ??= new();
      set => itemsFndGc = value;
    }

    /// <summary>
    /// A value of GrpGcCnt.
    /// </summary>
    [JsonPropertyName("grpGcCnt")]
    public Common GrpGcCnt
    {
      get => grpGcCnt ??= new();
      set => grpGcCnt = value;
    }

    /// <summary>
    /// A value of GrpNcCnt.
    /// </summary>
    [JsonPropertyName("grpNcCnt")]
    public Common GrpNcCnt
    {
      get => grpNcCnt ??= new();
      set => grpNcCnt = value;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
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
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public DateWorkArea Blank
    {
      get => blank ??= new();
      set => blank = value;
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
    /// A value of SendClosureLtrReqd.
    /// </summary>
    [JsonPropertyName("sendClosureLtrReqd")]
    public Common SendClosureLtrReqd
    {
      get => sendClosureLtrReqd ??= new();
      set => sendClosureLtrReqd = value;
    }

    /// <summary>
    /// A value of Lt60Days.
    /// </summary>
    [JsonPropertyName("lt60Days")]
    public Common Lt60Days
    {
      get => lt60Days ??= new();
      set => lt60Days = value;
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

    private NonCooperation nullNonCooperation;
    private GoodCause nullGoodCause;
    private CaseRole nullCaseRole;
    private Common findDoc;
    private DateWorkArea closureDatePlus60Days;
    private Common work;
    private Infrastructure infrastructure;
    private OutgoingDocument outgoingDocument;
    private Common incommingInterstateFound;
    private Case1 case1;
    private Common screenIdentification;
    private SpDocKey spDocKey;
    private Array<GrpGroup> grp;
    private WorkArea print;
    private Document document;
    private SpDocLiteral spDocLiteral;
    private Common position;
    private Common noActiveProgramInd;
    private Common noAps;
    private DateWorkArea current;
    private Common clearCommon;
    private CaseRole clearCaseRole;
    private CsePersonsWorkSet clearCsePersonsWorkSet;
    private Common row1AddFuncNc;
    private Common row1AddFuncGc;
    private Common count;
    private Common dataChanged;
    private Common itemsFndNc;
    private Common itemsFndGc;
    private Common grpGcCnt;
    private Common grpNcCnt;
    private Common multipleAps;
    private AbendData abendData;
    private Code code;
    private CodeValue codeValue;
    private Common common;
    private Common invalid;
    private DateWorkArea blank;
    private DateWorkArea dateWorkArea;
    private Common sendClosureLtrReqd;
    private Common lt60Days;
    private Infrastructure lastTran;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingObligationType.
    /// </summary>
    [JsonPropertyName("existingObligationType")]
    public ObligationType ExistingObligationType
    {
      get => existingObligationType ??= new();
      set => existingObligationType = value;
    }

    /// <summary>
    /// A value of ExistingObligation.
    /// </summary>
    [JsonPropertyName("existingObligation")]
    public Obligation ExistingObligation
    {
      get => existingObligation ??= new();
      set => existingObligation = value;
    }

    /// <summary>
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    /// <summary>
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
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
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
    }

    /// <summary>
    /// A value of PersonProgram.
    /// </summary>
    [JsonPropertyName("personProgram")]
    public PersonProgram PersonProgram
    {
      get => personProgram ??= new();
      set => personProgram = value;
    }

    /// <summary>
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CaseRole Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of KeyOnly.
    /// </summary>
    [JsonPropertyName("keyOnly")]
    public CsePerson KeyOnly
    {
      get => keyOnly ??= new();
      set => keyOnly = value;
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

    private ObligationType existingObligationType;
    private Obligation existingObligation;
    private DebtDetail debtDetail;
    private ObligationTransaction obligationTransaction;
    private CsePersonAccount csePersonAccount;
    private CaseUnit caseUnit;
    private PersonProgram personProgram;
    private Program program;
    private CaseRole ap;
    private CsePerson keyOnly;
    private Case1 case1;
  }
#endregion
}
