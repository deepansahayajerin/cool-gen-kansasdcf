// Program: SI_COMP_CASE_COMPOSITION, ID: 371734829, model: 746.
// Short name: SWECOMPP
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
/// A program: SI_COMP_CASE_COMPOSITION.
/// </para>
/// <para>
/// RESP: SRVINIT
/// COMP - Case Composition
/// This procedure lists all of the APs and children (current and past), along 
/// with the current AR, Mother and Father for a specified CASE.
/// An AP may be selected for the NEXT transfer to another screen.
/// A Child may be selected for a transfer to the Child Details screen or to the
/// Display Genetic Test screen.
/// An AP, a child and a mother can be selected to transfer to the genetic test 
/// details screen.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiCompCaseComposition: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_COMP_CASE_COMPOSITION program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCompCaseComposition(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCompCaseComposition.
  /// </summary>
  public SiCompCaseComposition(IContext context, Import import, Export export):
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
    // 03-06-95  Helen Sharland	Initial Development
    // 07-05-96  Rod Grey		Add PF-24 Print
    // 08-13-96  Michael Ramirez	Completed Print
    // 11/01/96  G. Lofton		Add new security and removed
    // 				old.
    // 12/31/1998	M Ramirez	Revised print process.
    // 12/31/1998	M Ramirez	Changed security to check CRUD actions only.
    // 12/31/1998	M Ramirez	Removed Case Invalid from main case of
    // 				command.  Added Case Otherwise, which will
    // 				handle this command.
    // ------------------------------------------------------------
    // 01/22/99 W.Campbell             An IF statement was
    //                                 
    // inserted to check for an
    //                                 
    // empty group before setting the
    //                                 
    // subscript of group_export_ap
    //                                 
    // to one.
    // --------------------------------------------
    // 04/30/99 W.Campbell             Logic added to allow
    //                                 
    // for selection of scrollable
    // items
    //                                 
    // for both AP and CH, so that as
    //                                 
    // much data as needed can be
    //                                 
    // passed to LGRQ via the dialog
    //                                 
    // flow.  This involved several
    //                                 
    // changes to this PRAD.  Also,
    //                                 
    // fixed ZDEL exit states.
    //                                 
    // Additionally, scroll counts were
    //                                 
    // added to the screen.
    // --------------------------------------------
    // 05/03/99 W.Campbell             New code added
    //                                 
    // to modify the displayed msg
    //                                 
    // on a successful display, to
    //                                 
    // inform the user that a selection
    //                                 
    // is needed for the return dialog
    //                                 
    // flow.  i.e. the user came to
    // this
    //                                 
    // screen automatically via the
    //                                 
    // dialog flow because another
    //                                 
    // procedure determined that
    //                                 
    // either an AP or a CH needed
    //                                 
    // to be selected from this screen
    //                                 
    // and then carried back to the
    //                                 
    // original screen via the
    //                                 
    // dialog flow.
    // ---------------------------------------------
    // 06/29/99 M.Lachowicz  Change property of READ
    //                        (Select Only or Cursor Only)
    // ------------------------------------------------------------
    // 08/09/99 M.Lachowicz  Set export hidden next tran to space
    // ------------------------------------------------------------
    // 01/06/2000	M Ramirez	83300		NEXT TRAN needs to be cleared before print 
    // process is invoked
    // ------------------------------------------------------------
    // ----------------------------------------------------------
    // PR# 81845       03/30/2000            Vithal Madhira
    // The code is modified to allow user to select  an 'AP' and 'CHILD' ( 
    // Mother is optional) on COMP  to do "Motherless Comparisons" genetic tests
    // on GTSC.
    // ----------------------------------------------------------
    // 09/05/00 W.Campbell      Inserted an IF
    //                          stmt to populate the
    //                          export_hidden_next_tran
    //                          fields with the First selected
    //                          AP person number.
    //                          Work done on WR#00193-B.
    // ---------------------------------------------
    // 09/28/00 M.Lachowicz      Modified COMP to write
    //                          the selected person number when
    //                          next transaction is selected.
    //                          If selected person is AR, also populate
    //                          cse_person_number_obligee.
    //                          If selected person is AP, populate
    //                          cse_person_number_ap,
    //                          cse_person_number_obligor and
    //                          cse_person_number.
    //                          Work done on WR#00216.
    // ---------------------------------------------
    // 11/07/00 M.Lachowicz      Modified COMP to write
    //                           AR and AP to next tran if any other person
    //                           is not selected.
    // ---------------------------------------------
    // 11/07/00 M.Lachowicz      Modified COMP to write
    //                           AR and AP to next tran if any other person
    //                           is not selected.
    // ---------------------------------------------
    // -----------------------------------------------------------------------------------------------
    // Per PR# 117676, user must flow back to GTSC by pressing PF9 (RETURN) 
    // after selecting AP/Mother/Child. PF15 (GTSC) will no longer be used and
    // will be deleted. The dialog flow from GTSC to COMP is changed from
    // TRANSFER TO LINK.
    //                                                      
    // --- Vithal (04/10/2001)
    // -----------------------------------------------------------------------------------------------
    // 12/11/01 GVandy	PR 135191  Dialog flow from LGRQ to COMP changed to a 
    // LINK.  Modified COMP
    // 			  to return to LGRQ using either F9 Return or F16 LGRQ.
    // -----------------------------------------------------------------------------------------------
    // --------------------------------------------------------------------------------------
    // Per WR# 020259, a flag 'IMPORT_HIDDEN_FROM_ALTS' will be sent to COMP 
    // from ALTS if multiple AP/CH exist on a case. User can select AP and CH at
    // the same time. When user press PF9 to return to ALTS, both selected AP
    // and CH will be sent to ALTS.
    //                                                      
    // -----------Vithal (05/20/2002)
    // ------------------------------------------------------------------------------------
    // 10/05/07 MFan	PR 218047  Added codes to always display NEXT CASE number 
    // on COMP screen.
    // -----------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    UseSpDocSetLiterals();
    export.Document.Name = import.Document.Name;

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // --------------------------------------------
    // Header details
    // --------------------------------------------
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.Next.Number = import.Next.Number;
    export.Case1.Number = import.Case1.Number;

    // 10/05/07 MFan	PR 218047  Added the following IF statement.
    if (IsEmpty(export.Next.Number))
    {
      export.Next.Number = import.Case1.Number;
    }

    export.ServiceProvider.LastName = import.ServiceProvider.LastName;
    MoveOffice(import.Office, export.Office);

    // 11/07/00 M.L Start
    export.HeaderLine.Text35 = import.HeaderLine.Text35;

    // 11/07/00 M.L End
    export.CaseOpen.Flag = import.CaseOpen.Flag;

    // 05/20/2002   Vithal Madhira
    export.HiddenFromAlts.Flag = import.HiddenFromAlts.Flag;

    // --------------------------------------------
    // 04/30/99 W.Campbell - New code added.
    // --------------------------------------------
    export.SelectedCaseRole.Index = -1;

    // --------------------------------------------
    // 04/30/99 W.Campbell - Added AP Scroll selected items.
    // --------------------------------------------
    for(import.ScrollSelAp.Index = 0; import.ScrollSelAp.Index < import
      .ScrollSelAp.Count; ++import.ScrollSelAp.Index)
    {
      if (!import.ScrollSelAp.CheckSize())
      {
        break;
      }

      export.ScrollSelAp.Index = import.ScrollSelAp.Index;
      export.ScrollSelAp.CheckSize();

      export.ScrollSelAp.Update.ScrollSelApDetailCommon.SelectChar =
        import.ScrollSelAp.Item.ScrollSelApDetailCommon.SelectChar;
      export.ScrollSelAp.Update.ScrollSelApDetailCsePersonsWorkSet.Assign(
        import.ScrollSelAp.Item.ScrollSelApDetailCsePersonsWorkSet);
      export.ScrollSelAp.Update.ScrollSelApDetailWorkArea.Text1 =
        import.ScrollSelAp.Item.ScrollSelApDetailWorkArea.Text1;
    }

    import.ScrollSelAp.CheckIndex();

    // --------------------------------------------
    // 04/30/99 W.Campbell - Added CHild Scroll selected items.
    // --------------------------------------------
    for(import.ScrollSelCh.Index = 0; import.ScrollSelCh.Index < import
      .ScrollSelCh.Count; ++import.ScrollSelCh.Index)
    {
      if (!import.ScrollSelCh.CheckSize())
      {
        break;
      }

      export.ScrollSelCh.Index = import.ScrollSelCh.Index;
      export.ScrollSelCh.CheckSize();

      export.ScrollSelCh.Update.ScrollSelChDetailCommon.SelectChar =
        import.ScrollSelCh.Item.ScrollSelChDetailCommon.SelectChar;
      export.ScrollSelCh.Update.ScrollSelChDetailCsePersonsWorkSet.Assign(
        import.ScrollSelCh.Item.ScrollSelChDetailCsePersonsWorkSet);
      export.ScrollSelCh.Update.ScrollSelChDetailWorkArea.Text1 =
        import.ScrollSelCh.Item.ScrollSelChDetailWorkArea.Text1;
    }

    import.ScrollSelCh.CheckIndex();

    // --------------------------------------------
    // AP details
    // --------------------------------------------
    export.ApStandard.ScrollingMessage = import.Ap1.ScrollingMessage;

    for(import.Ap.Index = 0; import.Ap.Index < import.Ap.Count; ++
      import.Ap.Index)
    {
      if (!import.Ap.CheckSize())
      {
        break;
      }

      export.Ap.Index = import.Ap.Index;
      export.Ap.CheckSize();

      export.Ap.Update.ApDetailCommon.SelectChar =
        import.Ap.Item.ApDetailCommon.SelectChar;
      export.Ap.Update.ApDetailCsePersonsWorkSet.Assign(
        import.Ap.Item.ApDetailCsePersonsWorkSet);
      export.Ap.Update.ApDetailWorkArea.Text1 =
        import.Ap.Item.ApDetailWorkArea.Text1;
    }

    import.Ap.CheckIndex();

    // --------------------------------------------
    // AR details
    // --------------------------------------------
    export.ArCommon.SelectChar = import.ArCommon.SelectChar;
    export.ArCsePersonsWorkSet.Assign(import.ArCsePersonsWorkSet);

    // --------------------------------------------
    // Mother details
    // --------------------------------------------
    export.MotherCommon.SelectChar = import.MotherCommon.SelectChar;
    export.MotherCsePersonsWorkSet.Assign(import.MotherCsePersonsWorkSet);

    // --------------------------------------------
    // Father details
    // --------------------------------------------
    export.FatherCommon.SelectChar = import.FatherCommon.SelectChar;
    export.FatherCsePersonsWorkSet.Assign(import.FatherCsePersonsWorkSet);

    // --------------------------------------------
    // Child details
    // --------------------------------------------
    for(import.Child.Index = 0; import.Child.Index < import.Child.Count; ++
      import.Child.Index)
    {
      if (!import.Child.CheckSize())
      {
        break;
      }

      export.Child.Index = import.Child.Index;
      export.Child.CheckSize();

      export.Child.Update.ChildDetailCommon.SelectChar =
        import.Child.Item.ChildDetailCommon.SelectChar;
      export.Child.Update.ChildDetailCsePersonsWorkSet.Assign(
        import.Child.Item.ChildDetailCsePersonsWorkSet);
      export.Child.Update.ChildDetailWorkArea.Text1 =
        import.Child.Item.ChildDetailWorkArea.Text1;
    }

    import.Child.CheckIndex();
    export.ChildStandard.ScrollingMessage = import.Child1.ScrollingMessage;

    // --------------------------------------------
    // AP details
    // --------------------------------------------
    export.HiddenAp.PageNumber = import.HiddenAp.PageNumber;

    for(import.PageKeysAp.Index = 0; import.PageKeysAp.Index < import
      .PageKeysAp.Count; ++import.PageKeysAp.Index)
    {
      if (!import.PageKeysAp.CheckSize())
      {
        break;
      }

      export.PageKeysAp.Index = import.PageKeysAp.Index;
      export.PageKeysAp.CheckSize();

      export.PageKeysAp.Update.PageKeysApStatus.Text1 =
        import.PageKeysAp.Item.PageKeysApStatus.Text1;
      export.PageKeysAp.Update.PageKeysAp1.Number =
        import.PageKeysAp.Item.PageKeysAp1.Number;
    }

    import.PageKeysAp.CheckIndex();

    if (import.HiddenAp.PageNumber == 0 || Equal(global.Command, "DISPLAY"))
    {
      export.HiddenAp.PageNumber = 1;

      export.PageKeysAp.Index = 0;
      export.PageKeysAp.CheckSize();
    }

    // --------------------------------------------
    // Child details
    // --------------------------------------------
    export.HiddenChild.PageNumber = import.HiddenChild.PageNumber;
    export.SelectedChildCsePerson.Number = import.SelectedChild.Number;

    for(import.PageKeysChild.Index = 0; import.PageKeysChild.Index < import
      .PageKeysChild.Count; ++import.PageKeysChild.Index)
    {
      if (!import.PageKeysChild.CheckSize())
      {
        break;
      }

      export.PageKeysChild.Index = import.PageKeysChild.Index;
      export.PageKeysChild.CheckSize();

      export.PageKeysChild.Update.PageKysChildStatus.Text1 =
        import.PageKeysChild.Item.PageKysChildStatus.Text1;
      export.PageKeysChild.Update.PageKeysChild1.Number =
        import.PageKeysChild.Item.PageKeysChild1.Number;
    }

    import.PageKeysChild.CheckIndex();

    if (import.HiddenChild.PageNumber == 0 || Equal(global.Command, "DISPLAY"))
    {
      export.HiddenChild.PageNumber = 1;

      export.PageKeysChild.Index = 0;
      export.PageKeysChild.CheckSize();
    }

    MoveCsePersonsWorkSet(import.Selected, export.Selected);
    export.HiddenComp.Flag = import.HiddenComp.Flag;

    // ---------------------------------------------
    // 04/30/99 W.Campbell - Scrolling
    // changed to allow for multiple selections
    // with scrolling.  New code added here.
    // ---------------------------------------------
    if (Equal(global.Command, "DISPLAY"))
    {
      // ---------------------------------------------
      // 04/30/99 W.Campbell - On a command of
      // display, the group views will be rebuilt
      // with data from the data base.  So just
      // keep on going.  If command is not
      // display, them some work must be
      // done.  See the following else statement.
      // ---------------------------------------------
    }
    else
    {
      // ---------------------------------------------
      // 04/30/99 W.Campbell -
      // Group_export_ap and Group_export_child
      // are on the screen.  Grp_export_scroll_sel_ap and
      // Grp_export_scroll_sel_ch are hidden and is
      // where we accumulate the scrollable, selectable
      // items.  Must get the items from the groups on
      // the screen and accumulate them in the larger
      // group views so that we will have saved all the
      // possibly selected items.
      // ---------------------------------------------
      // ---------------------------------------------
      // 04/30/99 W.Campbell -
      // First, we will take care of the APs.
      // ---------------------------------------------
      for(export.Ap.Index = 0; export.Ap.Index < export.Ap.Count; ++
        export.Ap.Index)
      {
        if (!export.Ap.CheckSize())
        {
          break;
        }

        // ---------------------------------------------
        // 04/30/99 W.Campbell - First, Validate the
        // APs selection character.
        // ---------------------------------------------
        switch(AsChar(export.Ap.Item.ApDetailCommon.SelectChar))
        {
          case 'S':
            // ---------------------------------------------
            // 09/05/00 W.Campbell - Inserted following IF
            // stmt to populate the export_hidden_next_tran
            // fields with the First selected AP person number.
            // This is to support the NEXTTRAN feature.
            // Work done on WR#00193-B.
            // ---------------------------------------------
            if (IsEmpty(export.Hidden.CsePersonNumber))
            {
              export.Hidden.CsePersonNumber =
                export.Ap.Item.ApDetailCsePersonsWorkSet.Number;
              export.Hidden.CsePersonNumberAp =
                export.Ap.Item.ApDetailCsePersonsWorkSet.Number;
            }

            break;
          case ' ':
            break;
          default:
            var field = GetField(export.Ap.Item.ApDetailCommon, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            return;
        }

        // ---------------------------------------------
        // 04/30/99 W.Campbell - Find each of these in
        // grp_export_scroll_sel_ap and update them
        // in that group if found.  If not found, then
        // add it to that group.
        // ---------------------------------------------
        for(export.ScrollSelAp.Index = 0; export.ScrollSelAp.Index < export
          .ScrollSelAp.Count; ++export.ScrollSelAp.Index)
        {
          if (!export.ScrollSelAp.CheckSize())
          {
            break;
          }

          if (Equal(export.Ap.Item.ApDetailCsePersonsWorkSet.Number,
            export.ScrollSelAp.Item.ScrollSelApDetailCsePersonsWorkSet.Number))
          {
            // ---------------------------------------------
            // 04/30/99 W.Campbell - Found this item, now
            // update it and then get the next one.
            // ---------------------------------------------
            export.ScrollSelAp.Update.ScrollSelApDetailCommon.SelectChar =
              export.Ap.Item.ApDetailCommon.SelectChar;

            goto Next1;
          }
        }

        export.ScrollSelAp.CheckIndex();

        // ---------------------------------------------
        // 04/30/99 W.Campbell - Didn't find this item
        // in the other group, so we must add it to the
        // other group.
        // ---------------------------------------------
        export.ScrollSelAp.Index = export.ScrollSelAp.Count;
        export.ScrollSelAp.CheckSize();

        if (export.ScrollSelAp.Index >= Export.ScrollSelApGroup.Capacity)
        {
          // ---------------------------------------------
          // 04/30/99 W.Campbell - This is a system problem
          // and should not happen, but if it does then get
          // out and don't cause a subscript out of range
          // problem.
          // ---------------------------------------------
          break;
        }

        // ---------------------------------------------
        // 04/30/99 W.Campbell - Add this item to
        // the other group and then get the next one.
        // ---------------------------------------------
        export.ScrollSelAp.Update.ScrollSelApDetailCommon.SelectChar =
          export.Ap.Item.ApDetailCommon.SelectChar;
        export.ScrollSelAp.Update.ScrollSelApDetailCsePersonsWorkSet.Assign(
          export.Ap.Item.ApDetailCsePersonsWorkSet);
        export.ScrollSelAp.Update.ScrollSelApDetailWorkArea.Text1 =
          export.Ap.Item.ApDetailWorkArea.Text1;

Next1:
        ;
      }

      export.Ap.CheckIndex();

      // ---------------------------------------------
      // 04/30/99 W.Campbell -
      // Now count the number of selected APs.
      // ---------------------------------------------
      local.Ap1.Count = 0;

      for(export.ScrollSelAp.Index = 0; export.ScrollSelAp.Index < export
        .ScrollSelAp.Count; ++export.ScrollSelAp.Index)
      {
        if (!export.ScrollSelAp.CheckSize())
        {
          break;
        }

        if (AsChar(export.ScrollSelAp.Item.ScrollSelApDetailCommon.SelectChar) ==
          'S')
        {
          ++local.Ap1.Count;
        }
      }

      export.ScrollSelAp.CheckIndex();

      // ---------------------------------------------
      // 04/30/99 W.Campbell -
      // Move the AP count to the export view
      // for display on the screen.
      // ---------------------------------------------
      export.ApCommon.Count = local.Ap1.Count;

      // ---------------------------------------------
      // 04/30/99 W.Campbell -
      // Now we will take care of the CHildren.
      // ---------------------------------------------
      for(export.Child.Index = 0; export.Child.Index < export.Child.Count; ++
        export.Child.Index)
      {
        if (!export.Child.CheckSize())
        {
          break;
        }

        // ---------------------------------------------
        // 04/30/99 W.Campbell - Validate the
        // CHildren selection character.
        // ---------------------------------------------
        switch(AsChar(export.Child.Item.ChildDetailCommon.SelectChar))
        {
          case 'S':
            break;
          case ' ':
            break;
          default:
            var field =
              GetField(export.Child.Item.ChildDetailCommon, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            return;
        }

        // ---------------------------------------------
        // 04/30/99 W.Campbell - Find each of these in
        // grp_export_scroll_sel_ch and update them
        // in that group, if found.  If not found, then
        // add them to that group.
        // ---------------------------------------------
        for(export.ScrollSelCh.Index = 0; export.ScrollSelCh.Index < export
          .ScrollSelCh.Count; ++export.ScrollSelCh.Index)
        {
          if (!export.ScrollSelCh.CheckSize())
          {
            break;
          }

          if (Equal(export.Child.Item.ChildDetailCsePersonsWorkSet.Number,
            export.ScrollSelCh.Item.ScrollSelChDetailCsePersonsWorkSet.Number))
          {
            // ---------------------------------------------
            // 04/30/99 W.Campbell - Found this item, now
            // update it and then get the next one.
            // ---------------------------------------------
            export.ScrollSelCh.Update.ScrollSelChDetailCommon.SelectChar =
              export.Child.Item.ChildDetailCommon.SelectChar;

            goto Next2;
          }
        }

        export.ScrollSelCh.CheckIndex();

        // ---------------------------------------------
        // 04/30/99 W.Campbell - Didn't find this item
        // in the other group, so we must add it to the
        // other group.
        // ---------------------------------------------
        export.ScrollSelCh.Index = export.ScrollSelCh.Count;
        export.ScrollSelCh.CheckSize();

        if (export.ScrollSelCh.Index >= Export.ScrollSelChGroup.Capacity)
        {
          // ---------------------------------------------
          // 04/30/99 W.Campbell - This is a system problem
          // and should not happen, but if it does then get
          // out and don't cause a subscript out of range
          // problem.
          // ---------------------------------------------
          break;
        }

        // ---------------------------------------------
        // 04/30/99 W.Campbell - Add this item to
        // the other group and then get the next one.
        // ---------------------------------------------
        export.ScrollSelCh.Update.ScrollSelChDetailCommon.SelectChar =
          export.Child.Item.ChildDetailCommon.SelectChar;
        export.ScrollSelCh.Update.ScrollSelChDetailCsePersonsWorkSet.Assign(
          export.Child.Item.ChildDetailCsePersonsWorkSet);
        export.ScrollSelCh.Update.ScrollSelChDetailWorkArea.Text1 =
          export.Child.Item.ChildDetailWorkArea.Text1;

Next2:
        ;
      }

      export.Child.CheckIndex();

      // ---------------------------------------------
      // 04/30/99 W.Campbell -
      // Now count the number of selected CHildren.
      // ---------------------------------------------
      local.Child1.Count = 0;

      for(export.ScrollSelCh.Index = 0; export.ScrollSelCh.Index < export
        .ScrollSelCh.Count; ++export.ScrollSelCh.Index)
      {
        if (!export.ScrollSelCh.CheckSize())
        {
          break;
        }

        if (AsChar(export.ScrollSelCh.Item.ScrollSelChDetailCommon.SelectChar) ==
          'S')
        {
          ++local.Child1.Count;
        }
      }

      export.ScrollSelCh.CheckIndex();

      // ---------------------------------------------
      // 04/30/99 W.Campbell -
      // Move the CHildren count to the export view
      // for display on the screen.
      // ---------------------------------------------
      export.ChildCommon.Count = local.Child1.Count;
    }

    // ---------------------------------------------
    // 04/30/99 W.Campbell - Scrolling
    // changed to allow for multiple selections
    // with scrolling.  End of New code added here.
    // ---------------------------------------------
    UseCabZeroFillNumber();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      var field = GetField(export.Next, "number");

      field.Error = true;

      return;
    }

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // 09/28/00 M.L Start
      for(export.Ap.Index = 0; export.Ap.Index < export.Ap.Count; ++
        export.Ap.Index)
      {
        if (!export.Ap.CheckSize())
        {
          break;
        }

        switch(AsChar(export.Ap.Item.ApDetailCommon.SelectChar))
        {
          case 'S':
            ++local.CounterOfSelections.Count;

            var field1 = GetField(export.Ap.Item.ApDetailCommon, "selectChar");

            field1.Error = true;

            local.Null1.CsePersonNumber =
              export.Ap.Item.ApDetailCsePersonsWorkSet.Number;
            local.Null1.CsePersonNumberObligor =
              export.Ap.Item.ApDetailCsePersonsWorkSet.Number;
            local.Null1.CsePersonNumberAp =
              export.Ap.Item.ApDetailCsePersonsWorkSet.Number;

            break;
          case ' ':
            break;
          default:
            var field2 = GetField(export.Ap.Item.ApDetailCommon, "selectChar");

            field2.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            return;
        }
      }

      export.Ap.CheckIndex();

      for(export.Child.Index = 0; export.Child.Index < export.Child.Count; ++
        export.Child.Index)
      {
        if (!export.Child.CheckSize())
        {
          break;
        }

        switch(AsChar(export.Child.Item.ChildDetailCommon.SelectChar))
        {
          case 'S':
            ++local.CounterOfSelections.Count;

            var field1 =
              GetField(export.Child.Item.ChildDetailCommon, "selectChar");

            field1.Error = true;

            local.Null1.CsePersonNumber =
              export.Child.Item.ChildDetailCsePersonsWorkSet.Number;

            break;
          case ' ':
            break;
          default:
            var field2 =
              GetField(export.Child.Item.ChildDetailCommon, "selectChar");

            field2.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            return;
        }
      }

      export.Child.CheckIndex();

      switch(AsChar(export.ArCommon.SelectChar))
      {
        case 'S':
          ++local.CounterOfSelections.Count;

          var field1 = GetField(export.ArCommon, "selectChar");

          field1.Error = true;

          local.Null1.CsePersonNumber = export.ArCsePersonsWorkSet.Number;
          local.Null1.CsePersonNumberObligee =
            export.ArCsePersonsWorkSet.Number;

          break;
        case ' ':
          break;
        default:
          var field2 = GetField(export.ArCommon, "selectChar");

          field2.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

          return;
      }

      switch(AsChar(export.FatherCommon.SelectChar))
      {
        case 'S':
          ++local.CounterOfSelections.Count;

          var field1 = GetField(export.FatherCommon, "selectChar");

          field1.Error = true;

          local.Null1.CsePersonNumber = export.FatherCsePersonsWorkSet.Number;

          break;
        case ' ':
          break;
        default:
          var field2 = GetField(export.FatherCommon, "selectChar");

          field2.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

          return;
      }

      switch(AsChar(export.MotherCommon.SelectChar))
      {
        case 'S':
          ++local.CounterOfSelections.Count;

          var field1 = GetField(export.MotherCommon, "selectChar");

          field1.Error = true;

          local.Null1.CsePersonNumber = export.MotherCsePersonsWorkSet.Number;

          break;
        case ' ':
          break;
        default:
          var field2 = GetField(export.MotherCommon, "selectChar");

          field2.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

          return;
      }

      if (local.CounterOfSelections.Count > 1)
      {
        ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

        return;
      }

      // 11/07/00 M.L Start
      if (local.CounterOfSelections.Count == 0)
      {
        if (CharAt(export.ArCsePersonsWorkSet.Number, 10) != 'O')
        {
          local.Null1.CsePersonNumber = export.ArCsePersonsWorkSet.Number;
          local.Null1.CsePersonNumberObligee =
            export.ArCsePersonsWorkSet.Number;
        }

        for(export.Ap.Index = 0; export.Ap.Index < export.Ap.Count; ++
          export.Ap.Index)
        {
          if (!export.Ap.CheckSize())
          {
            break;
          }

          if (AsChar(export.Ap.Item.ApDetailWorkArea.Text1) == 'A')
          {
            local.Null1.CsePersonNumberObligor =
              export.Ap.Item.ApDetailCsePersonsWorkSet.Number;
            local.Null1.CsePersonNumberAp =
              export.Ap.Item.ApDetailCsePersonsWorkSet.Number;

            break;
          }
        }

        export.Ap.CheckIndex();
      }

      // 11/07/00 M.L End
      export.Hidden.CsePersonNumber = local.Null1.CsePersonNumber ?? "";
      export.Hidden.CsePersonNumberAp = local.Null1.CsePersonNumberAp ?? "";
      export.Hidden.CsePersonNumberObligee =
        local.Null1.CsePersonNumberObligee ?? "";
      export.Hidden.CsePersonNumberObligor =
        local.Null1.CsePersonNumberObligor ?? "";

      // 09/28/00 M.L End
      export.Hidden.CaseNumber = export.Next.Number;
      UseScCabNextTranPut1();

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
        export.Next.Number = export.Hidden.CaseNumber ?? Spaces(10);
        export.Case1.Number = export.Hidden.CaseNumber ?? Spaces(10);
        UseCabZeroFillNumber();
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Next, "number");

        field.Error = true;

        return;
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    // to validate action level security
    // mjr
    // -----------------------------------------------------
    // 12/31/1998
    // Changed security to check CRUD actions only
    // ------------------------------------------------------------------
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "PRINT"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    // ---------------------------------------------
    // 04/30/99 W.Campbell - Scrolling
    // changed to allow for multiple selections
    // with scrolling.  Following code disabled.
    // ---------------------------------------------
    // ---------------------------------------------
    // 04/30/99 W.Campbell - Scrolling
    // changed to allow for multiple selections
    // with scrolling.  End of disabled code.
    // ---------------------------------------------
    if (!IsEmpty(export.ArCommon.SelectChar))
    {
      if (AsChar(export.ArCommon.SelectChar) != 'S')
      {
        var field = GetField(export.ArCommon, "selectChar");

        field.Error = true;

        ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

        return;
      }
    }

    if (!IsEmpty(export.MotherCommon.SelectChar))
    {
      if (AsChar(export.MotherCommon.SelectChar) != 'S')
      {
        var field = GetField(export.MotherCommon, "selectChar");

        field.Error = true;

        ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

        return;
      }
    }

    if (!IsEmpty(export.FatherCommon.SelectChar))
    {
      if (AsChar(export.FatherCommon.SelectChar) != 'S')
      {
        var field = GetField(export.FatherCommon, "selectChar");

        field.Error = true;

        ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

        return;
      }
    }

    // ---------------------------------------------
    // 04/30/99 W.Campbell - Scrolling
    // changed to allow for multiple selections
    // with scrolling.  Following code disabled.
    // ---------------------------------------------
    // ---------------------------------------------
    // 04/30/99 W.Campbell - Scrolling
    // changed to allow for multiple selections
    // with scrolling.  End of disabled code.
    // ---------------------------------------------
    if (Equal(global.Command, "RETURN") && AsChar(export.HiddenComp.Flag) == 'Q'
      )
    {
      // -- Export_hidden_comp ief_supplied flag = "Q" signifies that we arrived
      // at COMP from LGRQ.
      // Reset the command to LGRQ which allows multiple selections to be 
      // returned to LGRQ.
      global.Command = "LGRQ";
    }

    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        break;
      case "ASIN":
        if (IsEmpty(export.Case1.Number))
        {
          var field = GetField(export.Case1, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }
        else
        {
          // 06/28/99 M.L
          //              Change property of the following READ to generate
          //              SELECT ONLY
          if (ReadCase())
          {
            ExitState = "ECO_LNK_TO_ASIN";
            export.AsinObject.Text20 = "CASE";
          }
          else
          {
            var field = GetField(export.Case1, "number");

            field.Error = true;

            ExitState = "CASE_NF";
          }
        }

        return;
      case "PRINT":
        if (IsEmpty(export.ArCsePersonsWorkSet.Number))
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_PRINT";
        }
        else
        {
          export.DocmProtectFilter.Flag = "Y";
          export.Document.Type1 = "COMP";
          ExitState = "ECO_LNK_TO_DOCUMENT_MAINT";
        }

        return;
      case "RETDOCM":
        if (IsEmpty(import.Document.Name))
        {
          ExitState = "SP0000_NO_DOC_SEL_FOR_PRINT";

          return;
        }

        // mjr
        // ------------------------------------------
        // 01/06/2000
        // NEXT TRAN needs to be cleared before print process is invoked
        // -------------------------------------------------------
        export.Hidden.Assign(local.Null1);
        export.Standard.NextTransaction = "DKEY";
        export.Hidden.MiscText2 = TrimEnd(local.SpDocLiteral.IdDocument) + import
          .Document.Name;

        // mjr
        // ----------------------------------------------------
        // Place identifiers into next tran
        // -------------------------------------------------------
        export.Hidden.CaseNumber = export.Case1.Number;

        // ---------------------------------------------
        // 04/30/99 W.Campbell - Scrolling
        // changed to allow for multiple selections
        // with scrolling.  Following code disabled.
        // The following code needs to be changed
        // to accomodate the new scrolling.
        // ---------------------------------------------
        // ---------------------------------------------
        // 04/30/99 W.Campbell - Scrolling
        // changed to allow for multiple selections
        // with scrolling.  The following code
        // added to accomodate the new scrolling.
        // ---------------------------------------------
        if (local.Ap1.Count == 1)
        {
          for(export.ScrollSelAp.Index = 0; export.ScrollSelAp.Index < export
            .ScrollSelAp.Count; ++export.ScrollSelAp.Index)
          {
            if (!export.ScrollSelAp.CheckSize())
            {
              break;
            }

            if (!IsEmpty(export.ScrollSelAp.Item.ScrollSelApDetailCommon.
              SelectChar))
            {
              export.Hidden.CsePersonNumberAp =
                export.ScrollSelAp.Item.ScrollSelApDetailCsePersonsWorkSet.
                  Number;

              goto Test1;
            }
          }

          export.ScrollSelAp.CheckIndex();
        }

Test1:

        // ---------------------------------------------
        // 04/30/99 W.Campbell - Scrolling
        // changed to allow for multiple selections
        // with scrolling.  End of new code
        // added to accomodate the new scrolling.
        // ---------------------------------------------
        export.Hidden.CsePersonNumber = export.ArCsePersonsWorkSet.Number;

        // ---------------------------------------------
        // 04/30/99 W.Campbell - Scrolling
        // changed to allow for multiple selections
        // with scrolling.  Following code disabled.
        // The following code needs to be changed
        // to accomodate the new scrolling.
        // ---------------------------------------------
        // ---------------------------------------------
        // 04/30/99 W.Campbell - Scrolling
        // changed to allow for multiple selections
        // with scrolling.  The following code
        // added to accomodate the new scrolling.
        // ---------------------------------------------
        if (local.Child1.Count == 1)
        {
          for(export.ScrollSelCh.Index = 0; export.ScrollSelCh.Index < export
            .ScrollSelCh.Count; ++export.ScrollSelCh.Index)
          {
            if (!export.ScrollSelCh.CheckSize())
            {
              break;
            }

            if (!IsEmpty(export.ScrollSelCh.Item.ScrollSelChDetailCommon.
              SelectChar))
            {
              export.Hidden.MiscText1 =
                TrimEnd(local.SpDocLiteral.IdChNumber) + export
                .ScrollSelCh.Item.ScrollSelChDetailCsePersonsWorkSet.Number;

              goto Test2;
            }
          }

          export.ScrollSelCh.CheckIndex();
        }

Test2:

        // ---------------------------------------------
        // 04/30/99 W.Campbell - Scrolling
        // changed to allow for multiple selections
        // with scrolling.  End of new code
        // added to accomodate the new scrolling.
        // ---------------------------------------------
        UseScCabNextTranPut2();

        // mjr---> DKEY's trancode = SRPD
        //  Can change this to do a READ instead of hardcoding
        global.NextTran = "SRPD PRINT";

        return;
      case "PRINTRET":
        // mjr
        // -----------------------------------------------
        // 12/31/1998
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
        export.Case1.Number = export.Hidden.CaseNumber ?? Spaces(10);
        global.Command = "DISPLAY";

        break;
      case "APNX":
        // --------------------------------------------
        // Scroll forward through AP details
        // --------------------------------------------
        if (export.HiddenAp.PageNumber == Import.PageKeysApGroup.Capacity)
        {
          ExitState = "MAX_PAGES_REACHED_SYSTEM_ERROR";

          return;
        }

        // ---------------------------------------------
        // Ensure that there is another page of details
        // to retrieve.
        // ---------------------------------------------
        export.PageKeysAp.Index = export.HiddenAp.PageNumber;
        export.PageKeysAp.CheckSize();

        if (IsEmpty(export.PageKeysAp.Item.PageKeysAp1.Number))
        {
          ExitState = "ACO_NE0000_INVALID_FORWARD";

          return;
        }
        else
        {
          // ---------------------------------------------
          // Increase the page number
          // ---------------------------------------------
          ++export.HiddenAp.PageNumber;
        }

        break;
      case "APPV":
        if (export.HiddenAp.PageNumber == 1)
        {
          ExitState = "ACO_NE0000_INVALID_BACKWARD";

          return;
        }

        --export.HiddenAp.PageNumber;

        break;
      case "RETURN":
        // --------------------------------------------------------------------------------------
        // Per WR# 020259, a flag 'IMPORT_HIDDEN_FROM_ALTS' will be sent to COMP
        // from ALTS if multiple AP/CH exist on a case. User can select AP and
        // CH at the same time. When user press PF9 to return to ALTS, both
        // selected AP and CH will be sent to ALTS.
        //                                                      
        // -----------Vithal (05/20/2002)
        // ------------------------------------------------------------------------------------
        if (AsChar(export.HiddenFromAlts.Flag) == 'Y')
        {
          if (local.Ap1.Count == 0)
          {
            if (local.Child1.Count == 0)
            {
              ExitState = "SI0000_SELECT_AP_OR_CH";

              return;
            }
          }

          if (local.Child1.Count == 0)
          {
            if (local.Ap1.Count == 0)
            {
              ExitState = "SI0000_SELECT_AP_OR_CH";

              return;
            }
          }

          if (local.Ap1.Count > 1)
          {
            ExitState = "ACO_NE0000_ONLY_ONE_SELECTION";

            for(export.Ap.Index = 0; export.Ap.Index < export.Ap.Count; ++
              export.Ap.Index)
            {
              if (!export.Ap.CheckSize())
              {
                break;
              }

              if (AsChar(export.Ap.Item.ApDetailCommon.SelectChar) == 'S')
              {
                var field =
                  GetField(export.Ap.Item.ApDetailCommon, "selectChar");

                field.Error = true;
              }
            }

            export.Ap.CheckIndex();

            return;
          }

          if (local.Child1.Count > 1)
          {
            ExitState = "ACO_NE0000_ONLY_ONE_SELECTION";

            for(export.Child.Index = 0; export.Child.Index < export
              .Child.Count; ++export.Child.Index)
            {
              if (!export.Child.CheckSize())
              {
                break;
              }

              if (AsChar(export.Child.Item.ChildDetailCommon.SelectChar) == 'S')
              {
                var field =
                  GetField(export.Child.Item.ChildDetailCommon, "selectChar");

                field.Error = true;
              }
            }

            export.Child.CheckIndex();

            return;
          }

          for(export.ScrollSelAp.Index = 0; export.ScrollSelAp.Index < export
            .ScrollSelAp.Count; ++export.ScrollSelAp.Index)
          {
            if (!export.ScrollSelAp.CheckSize())
            {
              break;
            }

            if (AsChar(export.ScrollSelAp.Item.ScrollSelApDetailCommon.
              SelectChar) == 'S')
            {
              export.SelectedAp.Number =
                export.ScrollSelAp.Item.ScrollSelApDetailCsePersonsWorkSet.
                  Number;

              break;
            }
          }

          export.ScrollSelAp.CheckIndex();

          for(export.ScrollSelCh.Index = 0; export.ScrollSelCh.Index < export
            .ScrollSelCh.Count; ++export.ScrollSelCh.Index)
          {
            if (!export.ScrollSelCh.CheckSize())
            {
              break;
            }

            if (AsChar(export.ScrollSelCh.Item.ScrollSelChDetailCommon.
              SelectChar) == 'S')
            {
              export.SelectedChildCsePersonsWorkSet.Number =
                export.ScrollSelCh.Item.ScrollSelChDetailCsePersonsWorkSet.
                  Number;

              break;
            }
          }

          export.ScrollSelCh.CheckIndex();
          export.HiddenFromAlts.Flag = "";
          ExitState = "ACO_NE0000_RETURN";

          return;
        }

        // --------------------------------------------------------------------------------------
        // Per PR# 117676, user must flow back to GTSC by pressing PF9 (RETURN) 
        // after selecting AP/Mother/Child. PF15 (GTSC) will no longer be used
        // and will be deleted.
        // ------------------------------------------------------------------------------------
        if (AsChar(export.HiddenComp.Flag) == 'Y')
        {
          // --------------------------------------------
          // A mother, AP and a child must be selected to
          // perform a genetic test.
          // --------------------------------------------
          // ----------------------------------------------------------------
          // PR# 81845  :   It is decided by SME ( Denise Liard) that 'mother' 
          // optional for Genetic testing,  AP and CHILD must be selected, to
          // display on the GTSC screen. SO, if mother is not selected do not
          // pass the views ( ie. clear the Export Views so that no data will be
          // passed to GTSC screen via Dialog Flow.)
          //                                                 
          // Vithal (03/29/2000)
          // -----------------------------------------------------------------
          if (local.Ap1.Count == 0 || local.Child1.Count == 0)
          {
            ExitState = "SI0000_GENETIC_TEST_SEL_INCOMPLE";

            return;
          }

          if (local.Ap1.Count > 1)
          {
            ExitState = "ACO_NE0000_ONLY_ONE_SELECTION";

            for(export.Ap.Index = 0; export.Ap.Index < export.Ap.Count; ++
              export.Ap.Index)
            {
              if (!export.Ap.CheckSize())
              {
                break;
              }

              if (AsChar(export.Ap.Item.ApDetailCommon.SelectChar) == 'S')
              {
                var field =
                  GetField(export.Ap.Item.ApDetailCommon, "selectChar");

                field.Error = true;
              }
            }

            export.Ap.CheckIndex();

            return;
          }

          if (local.Child1.Count > 1)
          {
            ExitState = "ACO_NE0000_ONLY_ONE_SELECTION";

            for(export.Child.Index = 0; export.Child.Index < export
              .Child.Count; ++export.Child.Index)
            {
              if (!export.Child.CheckSize())
              {
                break;
              }

              if (AsChar(export.Child.Item.ChildDetailCommon.SelectChar) == 'S')
              {
                var field =
                  GetField(export.Child.Item.ChildDetailCommon, "selectChar");

                field.Error = true;
              }
            }

            export.Child.CheckIndex();

            return;
          }

          if (IsEmpty(export.MotherCommon.SelectChar))
          {
            export.MotherCsePersonsWorkSet.Assign(local.NullMother);
          }

          // ---------------------------------------------
          // 04/30/99 W.Campbell - Scrolling
          // changed to allow for multiple selections
          // with scrolling.  Following code disabled.
          // ---------------------------------------------
          // ---------------------------------------------
          // 04/30/99 W.Campbell - Scrolling
          // changed to allow for multiple selections
          // with scrolling.  New code added.
          // ---------------------------------------------
          for(export.ScrollSelAp.Index = 0; export.ScrollSelAp.Index < export
            .ScrollSelAp.Count; ++export.ScrollSelAp.Index)
          {
            if (!export.ScrollSelAp.CheckSize())
            {
              break;
            }

            if (AsChar(export.ScrollSelAp.Item.ScrollSelApDetailCommon.
              SelectChar) == 'S')
            {
              export.SelectedAp.Number =
                export.ScrollSelAp.Item.ScrollSelApDetailCsePersonsWorkSet.
                  Number;

              break;
            }
          }

          export.ScrollSelAp.CheckIndex();

          // ---------------------------------------------
          // 04/30/99 W.Campbell - Scrolling
          // changed to allow for multiple selections
          // with scrolling.  End of new code added.
          // ---------------------------------------------
          // ---------------------------------------------
          // 04/30/99 W.Campbell - Scrolling
          // changed to allow for multiple selections
          // with scrolling.  Following code disabled.
          // ---------------------------------------------
          // ---------------------------------------------
          // 04/30/99 W.Campbell - Scrolling
          // changed to allow for multiple selections
          // with scrolling.  New code added.
          // ---------------------------------------------
          for(export.ScrollSelCh.Index = 0; export.ScrollSelCh.Index < export
            .ScrollSelCh.Count; ++export.ScrollSelCh.Index)
          {
            if (!export.ScrollSelCh.CheckSize())
            {
              break;
            }

            if (AsChar(export.ScrollSelCh.Item.ScrollSelChDetailCommon.
              SelectChar) == 'S')
            {
              export.SelectedChildCsePersonsWorkSet.Number =
                export.ScrollSelCh.Item.ScrollSelChDetailCsePersonsWorkSet.
                  Number;

              break;
            }
          }

          export.ScrollSelCh.CheckIndex();

          // ---------------------------------------------
          // 04/30/99 W.Campbell - Scrolling
          // changed to allow for multiple selections
          // with scrolling.  End of new code added.
          // ---------------------------------------------
          ExitState = "ACO_NE0000_RETURN";

          return;
        }

        // ---------------------------------------------
        // Make sure that only one selection has been
        // made.
        // ---------------------------------------------
        local.Selected.Count = 0;
        local.Selected.Count = local.Ap1.Count;

        if (AsChar(export.FatherCommon.SelectChar) == 'S')
        {
          ++local.Selected.Count;
        }

        if (AsChar(export.MotherCommon.SelectChar) == 'S')
        {
          ++local.Selected.Count;
        }

        if (AsChar(export.ArCommon.SelectChar) == 'S')
        {
          ++local.Selected.Count;
        }

        if (local.Selected.Count > 1)
        {
          ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

          return;
        }
        else
        {
          // ---------------------------------------------
          // 04/30/99 W.Campbell - Scrolling
          // changed to allow for multiple selections
          // with scrolling.  Following code disabled.
          // ---------------------------------------------
          // ---------------------------------------------
          // 04/30/99 W.Campbell - Scrolling
          // changed to allow for multiple selections
          // with scrolling.  New code added.
          // ---------------------------------------------
          for(export.ScrollSelAp.Index = 0; export.ScrollSelAp.Index < export
            .ScrollSelAp.Count; ++export.ScrollSelAp.Index)
          {
            if (!export.ScrollSelAp.CheckSize())
            {
              break;
            }

            if (AsChar(export.ScrollSelAp.Item.ScrollSelApDetailCommon.
              SelectChar) == 'S')
            {
              MoveCsePersonsWorkSet(export.ScrollSelAp.Item.
                ScrollSelApDetailCsePersonsWorkSet, export.Selected);

              goto Test3;
            }
          }

          export.ScrollSelAp.CheckIndex();

          // ---------------------------------------------
          // 04/30/99 W.Campbell - Scrolling
          // changed to allow for multiple selections
          // with scrolling.  End of new code added.
          // ---------------------------------------------
          if (AsChar(export.ArCommon.SelectChar) == 'S')
          {
            MoveCsePersonsWorkSet(export.ArCsePersonsWorkSet, export.Selected);

            goto Test3;
          }

          if (AsChar(export.MotherCommon.SelectChar) == 'S')
          {
            MoveCsePersonsWorkSet(export.MotherCsePersonsWorkSet,
              export.Selected);

            goto Test3;
          }

          if (AsChar(export.FatherCommon.SelectChar) == 'S')
          {
            MoveCsePersonsWorkSet(export.FatherCsePersonsWorkSet,
              export.Selected);

            goto Test3;
          }

          // ---------------------------------------------
          // 04/30/99 W.Campbell - Scrolling
          // changed to allow for multiple selections
          // with scrolling.  Following code disabled.
          // ---------------------------------------------
          // ---------------------------------------------
          // 04/30/99 W.Campbell - Scrolling
          // changed to allow for multiple selections
          // with scrolling.  New code added.
          // ---------------------------------------------
          for(export.ScrollSelCh.Index = 0; export.ScrollSelCh.Index < export
            .ScrollSelCh.Count; ++export.ScrollSelCh.Index)
          {
            if (!export.ScrollSelCh.CheckSize())
            {
              break;
            }

            if (AsChar(export.ScrollSelCh.Item.ScrollSelChDetailCommon.
              SelectChar) == 'S')
            {
              MoveCsePersonsWorkSet(export.ScrollSelCh.Item.
                ScrollSelChDetailCsePersonsWorkSet, export.Selected);

              goto Test3;
            }
          }

          export.ScrollSelCh.CheckIndex();

          // ---------------------------------------------
          // 04/30/99 W.Campbell - Scrolling
          // changed to allow for multiple selections
          // with scrolling.  End of new code added.
          // ---------------------------------------------
        }

Test3:

        // Add section to return child seperately, before removing  code in 
        // prior ELSE logic for passing CHILD in cse-person-work-set, verify
        // data expected by other PRAD using CASE Comp.   ...Paul Elie  23/2/96.
        // ---------------------------------------------
        // 04/30/99 W.Campbell - Scrolling
        // changed to allow for multiple selections
        // with scrolling.  Following code disabled.
        // ---------------------------------------------
        // ---------------------------------------------
        // 04/30/99 W.Campbell - Scrolling
        // changed to allow for multiple selections
        // with scrolling.  New code added.
        // ---------------------------------------------
        for(export.ScrollSelCh.Index = 0; export.ScrollSelCh.Index < export
          .ScrollSelCh.Count; ++export.ScrollSelCh.Index)
        {
          if (!export.ScrollSelCh.CheckSize())
          {
            break;
          }

          if (AsChar(export.ScrollSelCh.Item.ScrollSelChDetailCommon.SelectChar) ==
            'S')
          {
            export.SelectedChildCsePersonsWorkSet.Number =
              export.ScrollSelCh.Item.ScrollSelChDetailCsePersonsWorkSet.Number;
              

            break;
          }
        }

        export.ScrollSelCh.CheckIndex();

        // ---------------------------------------------
        // 04/30/99 W.Campbell - Scrolling
        // changed to allow for multiple selections
        // with scrolling.  End of new code added.
        // ---------------------------------------------
        ExitState = "ACO_NE0000_RETURN";

        return;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "CHDS":
        // ---------------------------------------------
        // 04/30/99 W.Campbell - Scrolling
        // changed to allow for multiple selections
        // with scrolling.  Following code disabled.
        // ---------------------------------------------
        // ---------------------------------------------
        // 04/30/99 W.Campbell - Scrolling
        // changed to allow for multiple selections
        // with scrolling.  New code added.
        // ---------------------------------------------
        for(export.ScrollSelCh.Index = 0; export.ScrollSelCh.Index < export
          .ScrollSelCh.Count; ++export.ScrollSelCh.Index)
        {
          if (!export.ScrollSelCh.CheckSize())
          {
            break;
          }

          if (AsChar(export.ScrollSelCh.Item.ScrollSelChDetailCommon.SelectChar) ==
            'S')
          {
            MoveCsePersonsWorkSet(export.ScrollSelCh.Item.
              ScrollSelChDetailCsePersonsWorkSet, export.Selected);

            break;
          }
        }

        export.ScrollSelCh.CheckIndex();

        // ---------------------------------------------
        // 04/30/99 W.Campbell - Scrolling
        // changed to allow for multiple selections
        // with scrolling.  End of new code added.
        // ---------------------------------------------
        ExitState = "ECO_XFR_TO_CHILD_DETAILS";

        return;
      case "GTDS":
        // ---------------------------------------------
        // Transfer to display genetic test screen with
        // selected child.
        // ---------------------------------------------
        if (local.Child1.Count == 0)
        {
          ExitState = "GENETIC_TEST_REQUIRES_A_CHILD";

          return;
        }

        // ---------------------------------------------
        // 04/30/99 W.Campbell - Scrolling
        // changed to allow for multiple selections
        // with scrolling.  Following code disabled.
        // ---------------------------------------------
        // ---------------------------------------------
        // 04/30/99 W.Campbell - Scrolling
        // changed to allow for multiple selections
        // with scrolling.  New code added.
        // ---------------------------------------------
        for(export.ScrollSelCh.Index = 0; export.ScrollSelCh.Index < export
          .ScrollSelCh.Count; ++export.ScrollSelCh.Index)
        {
          if (!export.ScrollSelCh.CheckSize())
          {
            break;
          }

          if (AsChar(export.ScrollSelCh.Item.ScrollSelChDetailCommon.SelectChar) ==
            'S')
          {
            export.SelectedChildCsePersonsWorkSet.Number =
              export.ScrollSelCh.Item.ScrollSelChDetailCsePersonsWorkSet.Number;
              

            break;
          }
        }

        export.ScrollSelCh.CheckIndex();

        // ---------------------------------------------
        // 04/30/99 W.Campbell - Scrolling
        // changed to allow for multiple selections
        // with scrolling.  End of new code added.
        // ---------------------------------------------
        ExitState = "ECO_XFR_TO_DISPLAY_GENETIC_TEST";

        return;
      case "LGRQ":
        export.SelectedCaseRole.Index = -1;

        // ---------------------------------------------
        // 04/30/99 W.Campbell - Scrolling
        // changed to allow for multiple selections
        // with scrolling.  Following code disabled.
        // ---------------------------------------------
        // ---------------------------------------------
        // 04/30/99 W.Campbell - Scrolling
        // changed to allow for multiple selections
        // with scrolling.  New code added.
        // ---------------------------------------------
        if (export.SelectedCaseRole.Index + 1 < Export
          .SelectedCaseRoleGroup.Capacity)
        {
          for(export.ScrollSelAp.Index = 0; export.ScrollSelAp.Index < import
            .ScrollSelAp.Count; ++export.ScrollSelAp.Index)
          {
            if (!export.ScrollSelAp.CheckSize())
            {
              break;
            }

            if (AsChar(export.ScrollSelAp.Item.ScrollSelApDetailCommon.
              SelectChar) == 'S')
            {
              ++export.SelectedCaseRole.Index;
              export.SelectedCaseRole.CheckSize();

              if (export.SelectedCaseRole.Index >= Export
                .SelectedCaseRoleGroup.Capacity)
              {
                goto Test4;
              }

              export.SelectedCaseRole.Update.SelectedCsePerson.Number =
                export.ScrollSelAp.Item.ScrollSelApDetailCsePersonsWorkSet.
                  Number;
              export.SelectedCaseRole.Update.SelectedCsePersonsWorkSet.
                FormattedName =
                  export.ScrollSelAp.Item.ScrollSelApDetailCsePersonsWorkSet.
                  FormattedName;
              export.SelectedCaseRole.Update.SelectedCaseRole1.Type1 = "AP";
            }
          }

          export.ScrollSelAp.CheckIndex();

          if (AsChar(export.ArCommon.SelectChar) == 'S')
          {
            ++export.SelectedCaseRole.Index;
            export.SelectedCaseRole.CheckSize();

            if (export.SelectedCaseRole.Index >= Export
              .SelectedCaseRoleGroup.Capacity)
            {
              goto Test4;
            }

            export.SelectedCaseRole.Update.SelectedCsePerson.Number =
              export.ArCsePersonsWorkSet.Number;
            export.SelectedCaseRole.Update.SelectedCsePersonsWorkSet.
              FormattedName = export.ArCsePersonsWorkSet.FormattedName;
            export.SelectedCaseRole.Update.SelectedCaseRole1.Type1 = "AR";
          }

          if (AsChar(export.MotherCommon.SelectChar) == 'S')
          {
            ++export.SelectedCaseRole.Index;
            export.SelectedCaseRole.CheckSize();

            if (export.SelectedCaseRole.Index >= Export
              .SelectedCaseRoleGroup.Capacity)
            {
              goto Test4;
            }

            export.SelectedCaseRole.Update.SelectedCsePerson.Number =
              export.MotherCsePersonsWorkSet.Number;
            export.SelectedCaseRole.Update.SelectedCsePersonsWorkSet.
              FormattedName = export.MotherCsePersonsWorkSet.FormattedName;
            export.SelectedCaseRole.Update.SelectedCaseRole1.Type1 = "MO";
          }

          if (AsChar(export.FatherCommon.SelectChar) == 'S')
          {
            ++export.SelectedCaseRole.Index;
            export.SelectedCaseRole.CheckSize();

            if (export.SelectedCaseRole.Index >= Export
              .SelectedCaseRoleGroup.Capacity)
            {
              goto Test4;
            }

            export.SelectedCaseRole.Update.SelectedCsePerson.Number =
              export.FatherCsePersonsWorkSet.Number;
            export.SelectedCaseRole.Update.SelectedCsePersonsWorkSet.
              FormattedName = export.FatherCsePersonsWorkSet.FormattedName;
            export.SelectedCaseRole.Update.SelectedCaseRole1.Type1 = "FA";
          }

          for(export.ScrollSelCh.Index = 0; export.ScrollSelCh.Index < export
            .ScrollSelCh.Count; ++export.ScrollSelCh.Index)
          {
            if (!export.ScrollSelCh.CheckSize())
            {
              break;
            }

            if (AsChar(export.ScrollSelCh.Item.ScrollSelChDetailCommon.
              SelectChar) == 'S')
            {
              ++export.SelectedCaseRole.Index;
              export.SelectedCaseRole.CheckSize();

              if (export.SelectedCaseRole.Index >= Export
                .SelectedCaseRoleGroup.Capacity)
              {
                goto Test4;
              }

              export.SelectedCaseRole.Update.SelectedCsePerson.Number =
                export.ScrollSelCh.Item.ScrollSelChDetailCsePersonsWorkSet.
                  Number;
              export.SelectedCaseRole.Update.SelectedCsePersonsWorkSet.
                FormattedName =
                  export.ScrollSelCh.Item.ScrollSelChDetailCsePersonsWorkSet.
                  FormattedName;
              export.SelectedCaseRole.Update.SelectedCaseRole1.Type1 = "CH";
            }
          }

          export.ScrollSelCh.CheckIndex();
        }

Test4:

        // ---------------------------------------------
        // 04/30/99 W.Campbell - Scrolling
        // changed to allow for multiple selections
        // with scrolling.  End of new code added.
        // ---------------------------------------------
        if (AsChar(export.HiddenComp.Flag) == 'Q')
        {
          ExitState = "ACO_NE0000_RETURN";
        }
        else
        {
          ExitState = "ECO_XFR_TO_LEGAL_REQUEST";
        }

        return;
      case "CHPV":
        // --------------------------------------------
        // Scroll back through child details
        // --------------------------------------------
        if (export.HiddenChild.PageNumber == 1)
        {
          ExitState = "ACO_NE0000_INVALID_BACKWARD";

          return;
        }

        --export.HiddenChild.PageNumber;

        break;
      case "CHNX":
        // --------------------------------------------
        // Scroll forward through child details
        // --------------------------------------------
        if (export.HiddenChild.PageNumber == Import.PageKeysChildGroup.Capacity)
        {
          ExitState = "MAX_PAGES_REACHED_SYSTEM_ERROR";

          return;
        }

        // ---------------------------------------------
        // Ensure that there is another page of details
        // to retrieve.
        // ---------------------------------------------
        export.PageKeysChild.Index = export.HiddenChild.PageNumber;
        export.PageKeysChild.CheckSize();

        if (IsEmpty(export.PageKeysChild.Item.PageKeysChild1.Number))
        {
          ExitState = "ACO_NE0000_INVALID_FORWARD";

          return;
        }
        else
        {
          // ---------------------------------------------
          // Increase the page number
          // ---------------------------------------------
          ++export.HiddenChild.PageNumber;
        }

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
    }

    // ---------------------------------------------
    // 04/30/99 W.Campbell - Scrolling
    // changed to allow for multiple selections
    // with scrolling.  New code added here.
    // ---------------------------------------------
    if (Equal(global.Command, "DISPLAY"))
    {
      // ---------------------------------------------
      // 04/30/99 W.Campbell - Initialize the
      // scrolled selected items to indicate
      // that none have been displayed or selected.
      // ---------------------------------------------
      export.ScrollSelAp.Index = -1;
      export.ScrollSelAp.Count = 0;
      export.ScrollSelCh.Index = -1;
      export.ScrollSelCh.Count = 0;
      local.Ap1.Count = 0;
      local.Child1.Count = 0;
      export.ApCommon.Count = 0;
      export.ChildCommon.Count = 0;
    }

    // ---------------------------------------------
    // 04/30/99 W.Campbell - Scrolling
    // changed to allow for multiple selections
    // with scrolling.  End of New code added here.
    // ---------------------------------------------
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "CHNX") || Equal
      (global.Command, "CHPV") || Equal(global.Command, "APPV") || Equal
      (global.Command, "APNX"))
    {
      if (IsEmpty(export.Case1.Number))
      {
        if (IsEmpty(export.Next.Number))
        {
          var field = GetField(export.Next, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          return;
        }
        else
        {
          export.Case1.Number = export.Next.Number;
        }
      }
      else if (!IsEmpty(export.Next.Number))
      {
        if (!Equal(export.Case1.Number, export.Next.Number))
        {
          export.Case1.Number = export.Next.Number;
        }
      }

      export.Ap.Count = 0;
      export.Child.Count = 0;

      // ---------------------------------------------
      // 04/30/99 W.Campbell - New code added here
      // just to get rid of consistency check warnings.
      // ---------------------------------------------
      export.Ap.Index = -1;
      export.Child.Index = -1;
      export.ArCsePersonsWorkSet.Assign(local.Blank);
      export.FatherCsePersonsWorkSet.Assign(local.Blank);
      export.MotherCsePersonsWorkSet.Assign(local.Blank);

      // 06/29/99 M.L         Change property of READ to generate
      //                      Select Only
      // ------------------------------------------------------------
      if (ReadCase())
      {
        if (AsChar(entities.Case1.Status) == 'O')
        {
          export.CaseOpen.Flag = "Y";
        }
        else
        {
          export.CaseOpen.Flag = "N";
        }
      }
      else
      {
        ExitState = "CASE_NF";

        return;
      }

      UseSiReadOfficeOspHeader();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      // ---------------------------------------------
      // AP details retrieval
      // ---------------------------------------------
      export.PageKeysAp.Index = export.HiddenAp.PageNumber - 1;
      export.PageKeysAp.CheckSize();

      local.CaseRole.Type1 = "AP";
      UseSiReadSpecificCaseRoles1();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      // ---------------------------------------------
      // If a full group is retrieved, set up the page
      // keys for the next page of data
      // --------------------------------------------
      if (export.Ap.IsFull)
      {
        export.PageKeysAp.Index = export.HiddenAp.PageNumber;
        export.PageKeysAp.CheckSize();

        export.PageKeysAp.Update.PageKeysApStatus.Text1 =
          local.NextPageStatus.Text1;
        export.PageKeysAp.Update.PageKeysAp1.Number = local.NextPage.Number;
      }

      // ---------------------------------------------
      // AR, Mother and Father details retrieval
      // ---------------------------------------------
      UseSiReadParentAndArDetails();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      // ---------------------------------------------
      // Child details retrieval
      // ---------------------------------------------
      export.PageKeysChild.Index = export.HiddenChild.PageNumber - 1;
      export.PageKeysChild.CheckSize();

      local.CaseRole.Type1 = "CH";
      UseSiReadSpecificCaseRoles2();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      // Blank out ssn's that are all zeroes.
      if (Equal(export.ArCsePersonsWorkSet.Ssn, "000000000"))
      {
        export.ArCsePersonsWorkSet.Ssn = "";
      }

      if (Equal(export.FatherCsePersonsWorkSet.Ssn, "000000000"))
      {
        export.FatherCsePersonsWorkSet.Ssn = "";
      }

      if (Equal(export.MotherCsePersonsWorkSet.Ssn, "000000000"))
      {
        export.MotherCsePersonsWorkSet.Ssn = "";
      }

      for(export.Ap.Index = 0; export.Ap.Index < export.Ap.Count; ++
        export.Ap.Index)
      {
        if (!export.Ap.CheckSize())
        {
          break;
        }

        if (Equal(export.Ap.Item.ApDetailCsePersonsWorkSet.Ssn, "000000000"))
        {
          export.Ap.Update.ApDetailCsePersonsWorkSet.Ssn = "";
        }
      }

      export.Ap.CheckIndex();

      for(export.Child.Index = 0; export.Child.Index < export.Child.Count; ++
        export.Child.Index)
      {
        if (!export.Child.CheckSize())
        {
          break;
        }

        if (Equal(export.Child.Item.ChildDetailCsePersonsWorkSet.Ssn,
          "000000000"))
        {
          export.Child.Update.ChildDetailCsePersonsWorkSet.Ssn = "";
        }
      }

      export.Child.CheckIndex();

      // ---------------------------------------------
      // If a full group is retrieved, set up the page
      // keys for the next page of data
      // --------------------------------------------
      if (export.Child.IsFull)
      {
        export.PageKeysChild.Index = export.HiddenChild.PageNumber;
        export.PageKeysChild.CheckSize();

        export.PageKeysChild.Update.PageKysChildStatus.Text1 =
          local.NextPageStatus.Text1;
        export.PageKeysChild.Update.PageKeysChild1.Number =
          local.NextPage.Number;
      }

      if (export.Ap.IsEmpty && export.Child.IsEmpty && IsEmpty
        (export.ArCsePersonsWorkSet.Number) && IsEmpty
        (export.FatherCsePersonsWorkSet.Number) && IsEmpty
        (export.MotherCsePersonsWorkSet.Number))
      {
        var field = GetField(export.ArCommon, "selectChar");

        field.Protected = false;
        field.Focused = true;

        ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
      }
      else
      {
        // ---------------------------------------------
        // 01/22/99 W.Campbell - The following IF stmt
        // was inserted to check for an empty group
        // before setting the subscript of group_export_ap
        // to one.
        // --------------------------------------------
        if (!export.Ap.IsEmpty)
        {
          export.Ap.Index = 0;
          export.Ap.CheckSize();

          var field = GetField(export.Ap.Item.ApDetailCommon, "selectChar");

          field.Protected = false;
          field.Focused = true;
        }

        if (AsChar(export.CaseOpen.Flag) == 'N')
        {
          ExitState = "DISPLAY_OK_FOR_CLOSED_CASE";
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // mjr
          // -----------------------------------------------
          // 12/31/1998
          // Added check for an exitstate returned from Print
          // ------------------------------------------------------------
          local.Position.Count =
            Find(String(
              export.Hidden.MiscText2, NextTranInfo.MiscText2_MaxLength),
            TrimEnd(local.SpDocLiteral.IdDocument));

          if (local.Position.Count <= 0)
          {
            ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
          }
          else
          {
            // mjr---> Determines the appropriate exitstate for the Print 
            // process
            local.Print.Text50 = export.Hidden.MiscText2 ?? Spaces(50);
            UseSpPrintDecodeReturnCode();
            export.Hidden.MiscText2 = local.Print.Text50;
          }
        }
      }

      // ---------------------------------------------
      // 04/30/99 W.Campbell - New code added
      // to blank out the selection character for
      // AR, mother and father on a command
      // of display.
      // ---------------------------------------------
      if (Equal(global.Command, "DISPLAY"))
      {
        export.ArCommon.SelectChar = "";
        export.MotherCommon.SelectChar = "";
        export.FatherCommon.SelectChar = "";
      }

      // ---------------------------------------------
      // 04/30/99 W.Campbell - Scrolling
      // changed to allow for multiple selections
      // with scrolling.  New code added here.
      // ---------------------------------------------
      if (Equal(global.Command, "APNX") || Equal(global.Command, "APPV") || Equal
        (global.Command, "CHNX") || Equal(global.Command, "CHPV") || Equal
        (global.Command, "DISPLAY"))
      {
        // ---------------------------------------------
        // 04/30/99 W.Campbell -
        // Group_export_ap and Group_export_child
        // are on the screen.  Grp_export_scroll_sel_ap and
        // Grp_export_scroll_sel_ch are hidden and is
        // where we accumulate the scrollable, selectable
        // items.  Now we must take the items from the
        // hidden groups and update the scroll selection
        // character for the items on the screen if it has
        // been previously selected.
        // ---------------------------------------------
        // ---------------------------------------------
        // 04/30/99 W.Campbell -
        // First, we will take care of the APs.
        // ---------------------------------------------
        for(export.Ap.Index = 0; export.Ap.Index < export.Ap.Count; ++
          export.Ap.Index)
        {
          if (!export.Ap.CheckSize())
          {
            break;
          }

          // ---------------------------------------------
          // 04/30/99 W.Campbell - Find each of these in
          // grp_export_scroll_sel_ap and update their
          // screen selection character.  If not found,
          // then add them to that group.
          // ---------------------------------------------
          for(export.ScrollSelAp.Index = 0; export.ScrollSelAp.Index < export
            .ScrollSelAp.Count; ++export.ScrollSelAp.Index)
          {
            if (!export.ScrollSelAp.CheckSize())
            {
              break;
            }

            if (Equal(export.Ap.Item.ApDetailCsePersonsWorkSet.Number,
              export.ScrollSelAp.Item.ScrollSelApDetailCsePersonsWorkSet.
                Number))
            {
              // ---------------------------------------------
              // 04/30/99 W.Campbell - Found this item, now
              // update it and then get the next one.
              // ---------------------------------------------
              export.Ap.Update.ApDetailCommon.SelectChar =
                export.ScrollSelAp.Item.ScrollSelApDetailCommon.SelectChar;

              goto Next3;
            }
          }

          export.ScrollSelAp.CheckIndex();

          // ---------------------------------------------
          // 04/30/99 W.Campbell - Didn't find this item
          // in the other group, so we must add it to the
          // other group.
          // ---------------------------------------------
          export.ScrollSelAp.Index = export.ScrollSelAp.Count;
          export.ScrollSelAp.CheckSize();

          if (export.ScrollSelAp.Index >= Export.ScrollSelApGroup.Capacity)
          {
            // ---------------------------------------------
            // 04/30/99 W.Campbell - This is a system problem
            // and should not happen, but if it does then get
            // out and don't cause a subscript out of range
            // problem.
            // ---------------------------------------------
            break;
          }

          // ---------------------------------------------
          // 04/30/99 W.Campbell - Add this item to
          // the other group and then get the next one.
          // ---------------------------------------------
          export.ScrollSelAp.Update.ScrollSelApDetailCommon.SelectChar =
            export.Ap.Item.ApDetailCommon.SelectChar;
          export.ScrollSelAp.Update.ScrollSelApDetailCsePersonsWorkSet.Assign(
            export.Ap.Item.ApDetailCsePersonsWorkSet);
          export.ScrollSelAp.Update.ScrollSelApDetailWorkArea.Text1 =
            export.Ap.Item.ApDetailWorkArea.Text1;

Next3:
          ;
        }

        export.Ap.CheckIndex();

        // ---------------------------------------------
        // 04/30/99 W.Campbell -
        // Now we will take care of the CHildren.
        // ---------------------------------------------
        for(export.Child.Index = 0; export.Child.Index < export.Child.Count; ++
          export.Child.Index)
        {
          if (!export.Child.CheckSize())
          {
            break;
          }

          // ---------------------------------------------
          // 04/30/99 W.Campbell - Find each of these in
          // grp_export_scroll_sel_ch and update their
          // screen selection character.  If not found,
          // then add them to the other group.
          // ---------------------------------------------
          for(export.ScrollSelCh.Index = 0; export.ScrollSelCh.Index < export
            .ScrollSelCh.Count; ++export.ScrollSelCh.Index)
          {
            if (!export.ScrollSelCh.CheckSize())
            {
              break;
            }

            if (Equal(export.Child.Item.ChildDetailCsePersonsWorkSet.Number,
              export.ScrollSelCh.Item.ScrollSelChDetailCsePersonsWorkSet.
                Number))
            {
              // ---------------------------------------------
              // 04/30/99 W.Campbell - Found this item, now
              // update it and then get the next one.
              // ---------------------------------------------
              export.Child.Update.ChildDetailCommon.SelectChar =
                export.ScrollSelCh.Item.ScrollSelChDetailCommon.SelectChar;

              goto Next4;
            }
          }

          export.ScrollSelCh.CheckIndex();

          // ---------------------------------------------
          // 04/30/99 W.Campbell - Didn't find this item
          // in the other group, so we must add it to the
          // other group.
          // ---------------------------------------------
          export.ScrollSelCh.Index = export.ScrollSelCh.Count;
          export.ScrollSelCh.CheckSize();

          if (export.ScrollSelCh.Index >= Export.ScrollSelChGroup.Capacity)
          {
            // ---------------------------------------------
            // 04/30/99 W.Campbell - This is a system problem
            // and should not happen, but if it does then get
            // out and don't cause a subscript out of range
            // problem.
            // ---------------------------------------------
            break;
          }

          // ---------------------------------------------
          // 04/30/99 W.Campbell - Add this item to
          // the other group and then get the next one.
          // ---------------------------------------------
          export.ScrollSelCh.Update.ScrollSelChDetailCommon.SelectChar =
            export.Child.Item.ChildDetailCommon.SelectChar;
          export.ScrollSelCh.Update.ScrollSelChDetailCsePersonsWorkSet.Assign(
            export.Child.Item.ChildDetailCsePersonsWorkSet);
          export.ScrollSelCh.Update.ScrollSelChDetailWorkArea.Text1 =
            export.Child.Item.ChildDetailWorkArea.Text1;

Next4:
          ;
        }

        export.Child.CheckIndex();
      }

      // ---------------------------------------------
      // 04/30/99 W.Campbell - Scrolling
      // changed to allow for multiple selections
      // with scrolling.  End of New code added here.
      // ---------------------------------------------
      // ---------------------------------------------
      // 05/03/99 W.Campbell - New code added
      // to modify the displayed msg on a successful
      // display, to inform the user that a selection is
      // needed for the return dialog flow.  i.e. the
      // user came to this screen automatically via
      // the dialog flow because another procedure
      // determined that either an AP or a CH needed
      // to be selected from this screen and then
      // carried back to the original screen via the
      // dialog flow.
      // ---------------------------------------------
      if (Equal(import.Hidden.MiscText1, "SELECTION NEEDED"))
      {
        // M.L  08/09/1999   Start
        export.Hidden.MiscText1 = "";

        // M.L  08/09/1999   End
        if (IsExitState("ACO_NI0000_SUCCESSFUL_DISPLAY") || IsExitState
          ("DISPLAY_OK_FOR_CLOSED_CASE"))
        {
          ExitState = "SUCCESSFUL_DISPLAY_MULT_AP_OR_CH";
        }
      }

      // ---------------------------------------------
      // 05/03/99 W.Campbell - End of new code added
      // to modify the displayed msg on a successful
      // display, to inform the user that a selection is
      // needed for the return dialog flow.  i.e. the
      // user came to this screen automatically via
      // the dialog flow because another procedure
      // determined that either an AP or a CH needed
      // to be selected from this screen and then
      // carried back to the original screen via the
      // dialog flow.
      // ---------------------------------------------
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveExport1ToAp(SiReadSpecificCaseRoles.Export.
    ExportGroup source, Export.ApGroup target)
  {
    target.ApDetailCommon.SelectChar = source.DetailCommon.SelectChar;
    target.ApDetailWorkArea.Text1 = source.DetailWorkArea.Text1;
    target.ApDetailCsePersonsWorkSet.Assign(source.DetailCsePersonsWorkSet);
  }

  private static void MoveExport1ToChild(SiReadSpecificCaseRoles.Export.
    ExportGroup source, Export.ChildGroup target)
  {
    target.ChildDetailCommon.SelectChar = source.DetailCommon.SelectChar;
    target.ChildDetailWorkArea.Text1 = source.DetailWorkArea.Text1;
    target.ChildDetailCsePersonsWorkSet.Assign(source.DetailCsePersonsWorkSet);
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

  private static void MoveSpDocLiteral(SpDocLiteral source, SpDocLiteral target)
  {
    target.IdChNumber = source.IdChNumber;
    target.IdDocument = source.IdDocument;
  }

  private void UseCabZeroFillNumber()
  {
    var useImport = new CabZeroFillNumber.Import();
    var useExport = new CabZeroFillNumber.Export();

    useImport.Case1.Number = export.Next.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    export.Next.Number = useImport.Case1.Number;
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
    MoveNextTranInfo(export.Hidden, useImport.NextTranInfo);

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabNextTranPut2()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = export.Standard.NextTransaction;
    MoveNextTranInfo(export.Hidden, useImport.NextTranInfo);

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

    useImport.Case1.Number = import.Next.Number;

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

  private void UseSiReadParentAndArDetails()
  {
    var useImport = new SiReadParentAndArDetails.Import();
    var useExport = new SiReadParentAndArDetails.Export();

    useImport.Case1.Number = export.Case1.Number;
    useImport.CaseOpen.Flag = export.CaseOpen.Flag;

    Call(SiReadParentAndArDetails.Execute, useImport, useExport);

    export.ArCsePersonsWorkSet.Assign(useExport.Ar);
    export.MotherCsePersonsWorkSet.Assign(useExport.Mother);
    export.FatherCsePersonsWorkSet.Assign(useExport.Father);
  }

  private void UseSiReadSpecificCaseRoles1()
  {
    var useImport = new SiReadSpecificCaseRoles.Import();
    var useExport = new SiReadSpecificCaseRoles.Export();

    useImport.CaseRole.Type1 = local.CaseRole.Type1;
    useImport.Case1.Number = export.Case1.Number;
    useImport.Standard.PageNumber = export.HiddenAp.PageNumber;
    useImport.PageKeyStatus.Text1 =
      export.PageKeysAp.Item.PageKeysApStatus.Text1;
    useImport.PageKey.Number = export.PageKeysAp.Item.PageKeysAp1.Number;

    Call(SiReadSpecificCaseRoles.Execute, useImport, useExport);

    local.NextPageStatus.Text1 = useExport.NextPageStatus.Text1;
    local.NextPage.Number = useExport.NextPage.Number;
    export.ApStandard.ScrollingMessage = useExport.Standard.ScrollingMessage;
    useExport.Export1.CopyTo(export.Ap, MoveExport1ToAp);
  }

  private void UseSiReadSpecificCaseRoles2()
  {
    var useImport = new SiReadSpecificCaseRoles.Import();
    var useExport = new SiReadSpecificCaseRoles.Export();

    useImport.CaseRole.Type1 = local.CaseRole.Type1;
    useImport.Case1.Number = export.Case1.Number;
    useImport.Standard.PageNumber = export.HiddenChild.PageNumber;
    useImport.PageKeyStatus.Text1 =
      export.PageKeysChild.Item.PageKysChildStatus.Text1;
    useImport.PageKey.Number = export.PageKeysChild.Item.PageKeysChild1.Number;

    Call(SiReadSpecificCaseRoles.Execute, useImport, useExport);

    local.NextPageStatus.Text1 = useExport.NextPageStatus.Text1;
    local.NextPage.Number = useExport.NextPage.Number;
    export.ChildStandard.ScrollingMessage = useExport.Standard.ScrollingMessage;
    useExport.Export1.CopyTo(export.Child, MoveExport1ToChild);
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

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", export.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.Populated = true;
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
    /// <summary>A ScrollSelApGroup group.</summary>
    [Serializable]
    public class ScrollSelApGroup
    {
      /// <summary>
      /// A value of ScrollSelApDetailCommon.
      /// </summary>
      [JsonPropertyName("scrollSelApDetailCommon")]
      public Common ScrollSelApDetailCommon
      {
        get => scrollSelApDetailCommon ??= new();
        set => scrollSelApDetailCommon = value;
      }

      /// <summary>
      /// A value of ScrollSelApDetailWorkArea.
      /// </summary>
      [JsonPropertyName("scrollSelApDetailWorkArea")]
      public WorkArea ScrollSelApDetailWorkArea
      {
        get => scrollSelApDetailWorkArea ??= new();
        set => scrollSelApDetailWorkArea = value;
      }

      /// <summary>
      /// A value of ScrollSelApDetailCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("scrollSelApDetailCsePersonsWorkSet")]
      public CsePersonsWorkSet ScrollSelApDetailCsePersonsWorkSet
      {
        get => scrollSelApDetailCsePersonsWorkSet ??= new();
        set => scrollSelApDetailCsePersonsWorkSet = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 40;

      private Common scrollSelApDetailCommon;
      private WorkArea scrollSelApDetailWorkArea;
      private CsePersonsWorkSet scrollSelApDetailCsePersonsWorkSet;
    }

    /// <summary>A ApGroup group.</summary>
    [Serializable]
    public class ApGroup
    {
      /// <summary>
      /// A value of ApDetailCommon.
      /// </summary>
      [JsonPropertyName("apDetailCommon")]
      public Common ApDetailCommon
      {
        get => apDetailCommon ??= new();
        set => apDetailCommon = value;
      }

      /// <summary>
      /// A value of ApDetailWorkArea.
      /// </summary>
      [JsonPropertyName("apDetailWorkArea")]
      public WorkArea ApDetailWorkArea
      {
        get => apDetailWorkArea ??= new();
        set => apDetailWorkArea = value;
      }

      /// <summary>
      /// A value of ApDetailCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("apDetailCsePersonsWorkSet")]
      public CsePersonsWorkSet ApDetailCsePersonsWorkSet
      {
        get => apDetailCsePersonsWorkSet ??= new();
        set => apDetailCsePersonsWorkSet = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private Common apDetailCommon;
      private WorkArea apDetailWorkArea;
      private CsePersonsWorkSet apDetailCsePersonsWorkSet;
    }

    /// <summary>A PageKeysApGroup group.</summary>
    [Serializable]
    public class PageKeysApGroup
    {
      /// <summary>
      /// A value of PageKeysApStatus.
      /// </summary>
      [JsonPropertyName("pageKeysApStatus")]
      public WorkArea PageKeysApStatus
      {
        get => pageKeysApStatus ??= new();
        set => pageKeysApStatus = value;
      }

      /// <summary>
      /// A value of PageKeysAp1.
      /// </summary>
      [JsonPropertyName("pageKeysAp1")]
      public CsePersonsWorkSet PageKeysAp1
      {
        get => pageKeysAp1 ??= new();
        set => pageKeysAp1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private WorkArea pageKeysApStatus;
      private CsePersonsWorkSet pageKeysAp1;
    }

    /// <summary>A ScrollSelChGroup group.</summary>
    [Serializable]
    public class ScrollSelChGroup
    {
      /// <summary>
      /// A value of ScrollSelChDetailCommon.
      /// </summary>
      [JsonPropertyName("scrollSelChDetailCommon")]
      public Common ScrollSelChDetailCommon
      {
        get => scrollSelChDetailCommon ??= new();
        set => scrollSelChDetailCommon = value;
      }

      /// <summary>
      /// A value of ScrollSelChDetailWorkArea.
      /// </summary>
      [JsonPropertyName("scrollSelChDetailWorkArea")]
      public WorkArea ScrollSelChDetailWorkArea
      {
        get => scrollSelChDetailWorkArea ??= new();
        set => scrollSelChDetailWorkArea = value;
      }

      /// <summary>
      /// A value of ScrollSelChDetailCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("scrollSelChDetailCsePersonsWorkSet")]
      public CsePersonsWorkSet ScrollSelChDetailCsePersonsWorkSet
      {
        get => scrollSelChDetailCsePersonsWorkSet ??= new();
        set => scrollSelChDetailCsePersonsWorkSet = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 40;

      private Common scrollSelChDetailCommon;
      private WorkArea scrollSelChDetailWorkArea;
      private CsePersonsWorkSet scrollSelChDetailCsePersonsWorkSet;
    }

    /// <summary>A ChildGroup group.</summary>
    [Serializable]
    public class ChildGroup
    {
      /// <summary>
      /// A value of ChildDetailCommon.
      /// </summary>
      [JsonPropertyName("childDetailCommon")]
      public Common ChildDetailCommon
      {
        get => childDetailCommon ??= new();
        set => childDetailCommon = value;
      }

      /// <summary>
      /// A value of ChildDetailWorkArea.
      /// </summary>
      [JsonPropertyName("childDetailWorkArea")]
      public WorkArea ChildDetailWorkArea
      {
        get => childDetailWorkArea ??= new();
        set => childDetailWorkArea = value;
      }

      /// <summary>
      /// A value of ChildDetailCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("childDetailCsePersonsWorkSet")]
      public CsePersonsWorkSet ChildDetailCsePersonsWorkSet
      {
        get => childDetailCsePersonsWorkSet ??= new();
        set => childDetailCsePersonsWorkSet = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private Common childDetailCommon;
      private WorkArea childDetailWorkArea;
      private CsePersonsWorkSet childDetailCsePersonsWorkSet;
    }

    /// <summary>A PageKeysChildGroup group.</summary>
    [Serializable]
    public class PageKeysChildGroup
    {
      /// <summary>
      /// A value of PageKysChildStatus.
      /// </summary>
      [JsonPropertyName("pageKysChildStatus")]
      public WorkArea PageKysChildStatus
      {
        get => pageKysChildStatus ??= new();
        set => pageKysChildStatus = value;
      }

      /// <summary>
      /// A value of PageKeysChild1.
      /// </summary>
      [JsonPropertyName("pageKeysChild1")]
      public CsePersonsWorkSet PageKeysChild1
      {
        get => pageKeysChild1 ??= new();
        set => pageKeysChild1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private WorkArea pageKysChildStatus;
      private CsePersonsWorkSet pageKeysChild1;
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
    /// A value of DocSelection.
    /// </summary>
    [JsonPropertyName("docSelection")]
    public Common DocSelection
    {
      get => docSelection ??= new();
      set => docSelection = value;
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
    /// A value of SelectedChild.
    /// </summary>
    [JsonPropertyName("selectedChild")]
    public CsePerson SelectedChild
    {
      get => selectedChild ??= new();
      set => selectedChild = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of Ap1.
    /// </summary>
    [JsonPropertyName("ap1")]
    public Standard Ap1
    {
      get => ap1 ??= new();
      set => ap1 = value;
    }

    /// <summary>
    /// A value of HiddenAp.
    /// </summary>
    [JsonPropertyName("hiddenAp")]
    public Standard HiddenAp
    {
      get => hiddenAp ??= new();
      set => hiddenAp = value;
    }

    /// <summary>
    /// A value of ArCommon.
    /// </summary>
    [JsonPropertyName("arCommon")]
    public Common ArCommon
    {
      get => arCommon ??= new();
      set => arCommon = value;
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
    /// A value of MotherCommon.
    /// </summary>
    [JsonPropertyName("motherCommon")]
    public Common MotherCommon
    {
      get => motherCommon ??= new();
      set => motherCommon = value;
    }

    /// <summary>
    /// A value of MotherCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("motherCsePersonsWorkSet")]
    public CsePersonsWorkSet MotherCsePersonsWorkSet
    {
      get => motherCsePersonsWorkSet ??= new();
      set => motherCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of FatherCommon.
    /// </summary>
    [JsonPropertyName("fatherCommon")]
    public Common FatherCommon
    {
      get => fatherCommon ??= new();
      set => fatherCommon = value;
    }

    /// <summary>
    /// A value of FatherCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("fatherCsePersonsWorkSet")]
    public CsePersonsWorkSet FatherCsePersonsWorkSet
    {
      get => fatherCsePersonsWorkSet ??= new();
      set => fatherCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Child1.
    /// </summary>
    [JsonPropertyName("child1")]
    public Standard Child1
    {
      get => child1 ??= new();
      set => child1 = value;
    }

    /// <summary>
    /// A value of HiddenChild.
    /// </summary>
    [JsonPropertyName("hiddenChild")]
    public Standard HiddenChild
    {
      get => hiddenChild ??= new();
      set => hiddenChild = value;
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
    /// Gets a value of ScrollSelAp.
    /// </summary>
    [JsonIgnore]
    public Array<ScrollSelApGroup> ScrollSelAp => scrollSelAp ??= new(
      ScrollSelApGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ScrollSelAp for json serialization.
    /// </summary>
    [JsonPropertyName("scrollSelAp")]
    [Computed]
    public IList<ScrollSelApGroup> ScrollSelAp_Json
    {
      get => scrollSelAp;
      set => ScrollSelAp.Assign(value);
    }

    /// <summary>
    /// Gets a value of Ap.
    /// </summary>
    [JsonIgnore]
    public Array<ApGroup> Ap => ap ??= new(ApGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Ap for json serialization.
    /// </summary>
    [JsonPropertyName("ap")]
    [Computed]
    public IList<ApGroup> Ap_Json
    {
      get => ap;
      set => Ap.Assign(value);
    }

    /// <summary>
    /// Gets a value of PageKeysAp.
    /// </summary>
    [JsonIgnore]
    public Array<PageKeysApGroup> PageKeysAp => pageKeysAp ??= new(
      PageKeysApGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of PageKeysAp for json serialization.
    /// </summary>
    [JsonPropertyName("pageKeysAp")]
    [Computed]
    public IList<PageKeysApGroup> PageKeysAp_Json
    {
      get => pageKeysAp;
      set => PageKeysAp.Assign(value);
    }

    /// <summary>
    /// Gets a value of ScrollSelCh.
    /// </summary>
    [JsonIgnore]
    public Array<ScrollSelChGroup> ScrollSelCh => scrollSelCh ??= new(
      ScrollSelChGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ScrollSelCh for json serialization.
    /// </summary>
    [JsonPropertyName("scrollSelCh")]
    [Computed]
    public IList<ScrollSelChGroup> ScrollSelCh_Json
    {
      get => scrollSelCh;
      set => ScrollSelCh.Assign(value);
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
    /// Gets a value of PageKeysChild.
    /// </summary>
    [JsonIgnore]
    public Array<PageKeysChildGroup> PageKeysChild => pageKeysChild ??= new(
      PageKeysChildGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of PageKeysChild for json serialization.
    /// </summary>
    [JsonPropertyName("pageKeysChild")]
    [Computed]
    public IList<PageKeysChildGroup> PageKeysChild_Json
    {
      get => pageKeysChild;
      set => PageKeysChild.Assign(value);
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
    /// A value of HiddenComp.
    /// </summary>
    [JsonPropertyName("hiddenComp")]
    public Common HiddenComp
    {
      get => hiddenComp ??= new();
      set => hiddenComp = value;
    }

    /// <summary>
    /// A value of HiddenFromAlts.
    /// </summary>
    [JsonPropertyName("hiddenFromAlts")]
    public Common HiddenFromAlts
    {
      get => hiddenFromAlts ??= new();
      set => hiddenFromAlts = value;
    }

    private WorkArea headerLine;
    private ServiceProvider serviceProvider;
    private Office office;
    private Common docSelection;
    private Document document;
    private CsePerson selectedChild;
    private CsePersonsWorkSet selected;
    private Standard standard;
    private Case1 next;
    private Case1 case1;
    private Standard ap1;
    private Standard hiddenAp;
    private Common arCommon;
    private CsePersonsWorkSet arCsePersonsWorkSet;
    private Common motherCommon;
    private CsePersonsWorkSet motherCsePersonsWorkSet;
    private Common fatherCommon;
    private CsePersonsWorkSet fatherCsePersonsWorkSet;
    private Standard child1;
    private Standard hiddenChild;
    private NextTranInfo hidden;
    private Array<ScrollSelApGroup> scrollSelAp;
    private Array<ApGroup> ap;
    private Array<PageKeysApGroup> pageKeysAp;
    private Array<ScrollSelChGroup> scrollSelCh;
    private Array<ChildGroup> child;
    private Array<PageKeysChildGroup> pageKeysChild;
    private Common caseOpen;
    private Common hiddenComp;
    private Common hiddenFromAlts;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ScrollSelApGroup group.</summary>
    [Serializable]
    public class ScrollSelApGroup
    {
      /// <summary>
      /// A value of ScrollSelApDetailCommon.
      /// </summary>
      [JsonPropertyName("scrollSelApDetailCommon")]
      public Common ScrollSelApDetailCommon
      {
        get => scrollSelApDetailCommon ??= new();
        set => scrollSelApDetailCommon = value;
      }

      /// <summary>
      /// A value of ScrollSelApDetailWorkArea.
      /// </summary>
      [JsonPropertyName("scrollSelApDetailWorkArea")]
      public WorkArea ScrollSelApDetailWorkArea
      {
        get => scrollSelApDetailWorkArea ??= new();
        set => scrollSelApDetailWorkArea = value;
      }

      /// <summary>
      /// A value of ScrollSelApDetailCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("scrollSelApDetailCsePersonsWorkSet")]
      public CsePersonsWorkSet ScrollSelApDetailCsePersonsWorkSet
      {
        get => scrollSelApDetailCsePersonsWorkSet ??= new();
        set => scrollSelApDetailCsePersonsWorkSet = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 40;

      private Common scrollSelApDetailCommon;
      private WorkArea scrollSelApDetailWorkArea;
      private CsePersonsWorkSet scrollSelApDetailCsePersonsWorkSet;
    }

    /// <summary>A ApGroup group.</summary>
    [Serializable]
    public class ApGroup
    {
      /// <summary>
      /// A value of ApDetailCommon.
      /// </summary>
      [JsonPropertyName("apDetailCommon")]
      public Common ApDetailCommon
      {
        get => apDetailCommon ??= new();
        set => apDetailCommon = value;
      }

      /// <summary>
      /// A value of ApDetailWorkArea.
      /// </summary>
      [JsonPropertyName("apDetailWorkArea")]
      public WorkArea ApDetailWorkArea
      {
        get => apDetailWorkArea ??= new();
        set => apDetailWorkArea = value;
      }

      /// <summary>
      /// A value of ApDetailCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("apDetailCsePersonsWorkSet")]
      public CsePersonsWorkSet ApDetailCsePersonsWorkSet
      {
        get => apDetailCsePersonsWorkSet ??= new();
        set => apDetailCsePersonsWorkSet = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private Common apDetailCommon;
      private WorkArea apDetailWorkArea;
      private CsePersonsWorkSet apDetailCsePersonsWorkSet;
    }

    /// <summary>A PageKeysApGroup group.</summary>
    [Serializable]
    public class PageKeysApGroup
    {
      /// <summary>
      /// A value of PageKeysApStatus.
      /// </summary>
      [JsonPropertyName("pageKeysApStatus")]
      public WorkArea PageKeysApStatus
      {
        get => pageKeysApStatus ??= new();
        set => pageKeysApStatus = value;
      }

      /// <summary>
      /// A value of PageKeysAp1.
      /// </summary>
      [JsonPropertyName("pageKeysAp1")]
      public CsePersonsWorkSet PageKeysAp1
      {
        get => pageKeysAp1 ??= new();
        set => pageKeysAp1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private WorkArea pageKeysApStatus;
      private CsePersonsWorkSet pageKeysAp1;
    }

    /// <summary>A ScrollSelChGroup group.</summary>
    [Serializable]
    public class ScrollSelChGroup
    {
      /// <summary>
      /// A value of ScrollSelChDetailCommon.
      /// </summary>
      [JsonPropertyName("scrollSelChDetailCommon")]
      public Common ScrollSelChDetailCommon
      {
        get => scrollSelChDetailCommon ??= new();
        set => scrollSelChDetailCommon = value;
      }

      /// <summary>
      /// A value of ScrollSelChDetailWorkArea.
      /// </summary>
      [JsonPropertyName("scrollSelChDetailWorkArea")]
      public WorkArea ScrollSelChDetailWorkArea
      {
        get => scrollSelChDetailWorkArea ??= new();
        set => scrollSelChDetailWorkArea = value;
      }

      /// <summary>
      /// A value of ScrollSelChDetailCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("scrollSelChDetailCsePersonsWorkSet")]
      public CsePersonsWorkSet ScrollSelChDetailCsePersonsWorkSet
      {
        get => scrollSelChDetailCsePersonsWorkSet ??= new();
        set => scrollSelChDetailCsePersonsWorkSet = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 40;

      private Common scrollSelChDetailCommon;
      private WorkArea scrollSelChDetailWorkArea;
      private CsePersonsWorkSet scrollSelChDetailCsePersonsWorkSet;
    }

    /// <summary>A ChildGroup group.</summary>
    [Serializable]
    public class ChildGroup
    {
      /// <summary>
      /// A value of ChildDetailCommon.
      /// </summary>
      [JsonPropertyName("childDetailCommon")]
      public Common ChildDetailCommon
      {
        get => childDetailCommon ??= new();
        set => childDetailCommon = value;
      }

      /// <summary>
      /// A value of ChildDetailWorkArea.
      /// </summary>
      [JsonPropertyName("childDetailWorkArea")]
      public WorkArea ChildDetailWorkArea
      {
        get => childDetailWorkArea ??= new();
        set => childDetailWorkArea = value;
      }

      /// <summary>
      /// A value of ChildDetailCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("childDetailCsePersonsWorkSet")]
      public CsePersonsWorkSet ChildDetailCsePersonsWorkSet
      {
        get => childDetailCsePersonsWorkSet ??= new();
        set => childDetailCsePersonsWorkSet = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private Common childDetailCommon;
      private WorkArea childDetailWorkArea;
      private CsePersonsWorkSet childDetailCsePersonsWorkSet;
    }

    /// <summary>A PageKeysChildGroup group.</summary>
    [Serializable]
    public class PageKeysChildGroup
    {
      /// <summary>
      /// A value of PageKysChildStatus.
      /// </summary>
      [JsonPropertyName("pageKysChildStatus")]
      public WorkArea PageKysChildStatus
      {
        get => pageKysChildStatus ??= new();
        set => pageKysChildStatus = value;
      }

      /// <summary>
      /// A value of PageKeysChild1.
      /// </summary>
      [JsonPropertyName("pageKeysChild1")]
      public CsePersonsWorkSet PageKeysChild1
      {
        get => pageKeysChild1 ??= new();
        set => pageKeysChild1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private WorkArea pageKysChildStatus;
      private CsePersonsWorkSet pageKeysChild1;
    }

    /// <summary>A SelectedCaseRoleGroup group.</summary>
    [Serializable]
    public class SelectedCaseRoleGroup
    {
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
      /// A value of SelectedCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("selectedCsePersonsWorkSet")]
      public CsePersonsWorkSet SelectedCsePersonsWorkSet
      {
        get => selectedCsePersonsWorkSet ??= new();
        set => selectedCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of SelectedCaseRole1.
      /// </summary>
      [JsonPropertyName("selectedCaseRole1")]
      public CaseRole SelectedCaseRole1
      {
        get => selectedCaseRole1 ??= new();
        set => selectedCaseRole1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private CsePerson selectedCsePerson;
      private CsePersonsWorkSet selectedCsePersonsWorkSet;
      private CaseRole selectedCaseRole1;
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
    /// A value of ApCommon.
    /// </summary>
    [JsonPropertyName("apCommon")]
    public Common ApCommon
    {
      get => apCommon ??= new();
      set => apCommon = value;
    }

    /// <summary>
    /// A value of ChildCommon.
    /// </summary>
    [JsonPropertyName("childCommon")]
    public Common ChildCommon
    {
      get => childCommon ??= new();
      set => childCommon = value;
    }

    /// <summary>
    /// A value of DocmProtectFilter.
    /// </summary>
    [JsonPropertyName("docmProtectFilter")]
    public Common DocmProtectFilter
    {
      get => docmProtectFilter ??= new();
      set => docmProtectFilter = value;
    }

    /// <summary>
    /// A value of AsinObject.
    /// </summary>
    [JsonPropertyName("asinObject")]
    public SpTextWorkArea AsinObject
    {
      get => asinObject ??= new();
      set => asinObject = value;
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
    /// A value of DocSelection.
    /// </summary>
    [JsonPropertyName("docSelection")]
    public Common DocSelection
    {
      get => docSelection ??= new();
      set => docSelection = value;
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
    /// A value of SelectedChildCsePerson.
    /// </summary>
    [JsonPropertyName("selectedChildCsePerson")]
    public CsePerson SelectedChildCsePerson
    {
      get => selectedChildCsePerson ??= new();
      set => selectedChildCsePerson = value;
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
    /// A value of SelectedChildCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("selectedChildCsePersonsWorkSet")]
    public CsePersonsWorkSet SelectedChildCsePersonsWorkSet
    {
      get => selectedChildCsePersonsWorkSet ??= new();
      set => selectedChildCsePersonsWorkSet = value;
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
    /// A value of ApStandard.
    /// </summary>
    [JsonPropertyName("apStandard")]
    public Standard ApStandard
    {
      get => apStandard ??= new();
      set => apStandard = value;
    }

    /// <summary>
    /// A value of HiddenAp.
    /// </summary>
    [JsonPropertyName("hiddenAp")]
    public Standard HiddenAp
    {
      get => hiddenAp ??= new();
      set => hiddenAp = value;
    }

    /// <summary>
    /// A value of ArCommon.
    /// </summary>
    [JsonPropertyName("arCommon")]
    public Common ArCommon
    {
      get => arCommon ??= new();
      set => arCommon = value;
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
    /// A value of MotherCommon.
    /// </summary>
    [JsonPropertyName("motherCommon")]
    public Common MotherCommon
    {
      get => motherCommon ??= new();
      set => motherCommon = value;
    }

    /// <summary>
    /// A value of MotherCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("motherCsePersonsWorkSet")]
    public CsePersonsWorkSet MotherCsePersonsWorkSet
    {
      get => motherCsePersonsWorkSet ??= new();
      set => motherCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of FatherCommon.
    /// </summary>
    [JsonPropertyName("fatherCommon")]
    public Common FatherCommon
    {
      get => fatherCommon ??= new();
      set => fatherCommon = value;
    }

    /// <summary>
    /// A value of FatherCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("fatherCsePersonsWorkSet")]
    public CsePersonsWorkSet FatherCsePersonsWorkSet
    {
      get => fatherCsePersonsWorkSet ??= new();
      set => fatherCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of HiddenChild.
    /// </summary>
    [JsonPropertyName("hiddenChild")]
    public Standard HiddenChild
    {
      get => hiddenChild ??= new();
      set => hiddenChild = value;
    }

    /// <summary>
    /// A value of ChildStandard.
    /// </summary>
    [JsonPropertyName("childStandard")]
    public Standard ChildStandard
    {
      get => childStandard ??= new();
      set => childStandard = value;
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
    /// Gets a value of ScrollSelAp.
    /// </summary>
    [JsonIgnore]
    public Array<ScrollSelApGroup> ScrollSelAp => scrollSelAp ??= new(
      ScrollSelApGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ScrollSelAp for json serialization.
    /// </summary>
    [JsonPropertyName("scrollSelAp")]
    [Computed]
    public IList<ScrollSelApGroup> ScrollSelAp_Json
    {
      get => scrollSelAp;
      set => ScrollSelAp.Assign(value);
    }

    /// <summary>
    /// Gets a value of Ap.
    /// </summary>
    [JsonIgnore]
    public Array<ApGroup> Ap => ap ??= new(ApGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Ap for json serialization.
    /// </summary>
    [JsonPropertyName("ap")]
    [Computed]
    public IList<ApGroup> Ap_Json
    {
      get => ap;
      set => Ap.Assign(value);
    }

    /// <summary>
    /// Gets a value of PageKeysAp.
    /// </summary>
    [JsonIgnore]
    public Array<PageKeysApGroup> PageKeysAp => pageKeysAp ??= new(
      PageKeysApGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of PageKeysAp for json serialization.
    /// </summary>
    [JsonPropertyName("pageKeysAp")]
    [Computed]
    public IList<PageKeysApGroup> PageKeysAp_Json
    {
      get => pageKeysAp;
      set => PageKeysAp.Assign(value);
    }

    /// <summary>
    /// Gets a value of ScrollSelCh.
    /// </summary>
    [JsonIgnore]
    public Array<ScrollSelChGroup> ScrollSelCh => scrollSelCh ??= new(
      ScrollSelChGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ScrollSelCh for json serialization.
    /// </summary>
    [JsonPropertyName("scrollSelCh")]
    [Computed]
    public IList<ScrollSelChGroup> ScrollSelCh_Json
    {
      get => scrollSelCh;
      set => ScrollSelCh.Assign(value);
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
    /// Gets a value of PageKeysChild.
    /// </summary>
    [JsonIgnore]
    public Array<PageKeysChildGroup> PageKeysChild => pageKeysChild ??= new(
      PageKeysChildGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of PageKeysChild for json serialization.
    /// </summary>
    [JsonPropertyName("pageKeysChild")]
    [Computed]
    public IList<PageKeysChildGroup> PageKeysChild_Json
    {
      get => pageKeysChild;
      set => PageKeysChild.Assign(value);
    }

    /// <summary>
    /// Gets a value of SelectedCaseRole.
    /// </summary>
    [JsonIgnore]
    public Array<SelectedCaseRoleGroup> SelectedCaseRole =>
      selectedCaseRole ??= new(SelectedCaseRoleGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of SelectedCaseRole for json serialization.
    /// </summary>
    [JsonPropertyName("selectedCaseRole")]
    [Computed]
    public IList<SelectedCaseRoleGroup> SelectedCaseRole_Json
    {
      get => selectedCaseRole;
      set => SelectedCaseRole.Assign(value);
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
    /// A value of HiddenComp.
    /// </summary>
    [JsonPropertyName("hiddenComp")]
    public Common HiddenComp
    {
      get => hiddenComp ??= new();
      set => hiddenComp = value;
    }

    /// <summary>
    /// A value of HiddenFromAlts.
    /// </summary>
    [JsonPropertyName("hiddenFromAlts")]
    public Common HiddenFromAlts
    {
      get => hiddenFromAlts ??= new();
      set => hiddenFromAlts = value;
    }

    private WorkArea headerLine;
    private Common apCommon;
    private Common childCommon;
    private Common docmProtectFilter;
    private SpTextWorkArea asinObject;
    private ServiceProvider serviceProvider;
    private Office office;
    private Common docSelection;
    private Document document;
    private CsePerson selectedChildCsePerson;
    private CsePersonsWorkSet selected;
    private CsePersonsWorkSet selectedChildCsePersonsWorkSet;
    private CsePersonsWorkSet selectedAp;
    private Standard standard;
    private Case1 next;
    private Case1 case1;
    private Standard apStandard;
    private Standard hiddenAp;
    private Common arCommon;
    private CsePersonsWorkSet arCsePersonsWorkSet;
    private Common motherCommon;
    private CsePersonsWorkSet motherCsePersonsWorkSet;
    private Common fatherCommon;
    private CsePersonsWorkSet fatherCsePersonsWorkSet;
    private Standard hiddenChild;
    private Standard childStandard;
    private NextTranInfo hidden;
    private Array<ScrollSelApGroup> scrollSelAp;
    private Array<ApGroup> ap;
    private Array<PageKeysApGroup> pageKeysAp;
    private Array<ScrollSelChGroup> scrollSelCh;
    private Array<ChildGroup> child;
    private Array<PageKeysChildGroup> pageKeysChild;
    private Array<SelectedCaseRoleGroup> selectedCaseRole;
    private Common caseOpen;
    private Common hiddenComp;
    private Common hiddenFromAlts;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of CounterOfSelections.
    /// </summary>
    [JsonPropertyName("counterOfSelections")]
    public Common CounterOfSelections
    {
      get => counterOfSelections ??= new();
      set => counterOfSelections = value;
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
    /// A value of Print.
    /// </summary>
    [JsonPropertyName("print")]
    public WorkArea Print
    {
      get => print ??= new();
      set => print = value;
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
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public CsePersonsWorkSet Blank
    {
      get => blank ??= new();
      set => blank = value;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public Common Selected
    {
      get => selected ??= new();
      set => selected = value;
    }

    /// <summary>
    /// A value of NextPageStatus.
    /// </summary>
    [JsonPropertyName("nextPageStatus")]
    public WorkArea NextPageStatus
    {
      get => nextPageStatus ??= new();
      set => nextPageStatus = value;
    }

    /// <summary>
    /// A value of NextPage.
    /// </summary>
    [JsonPropertyName("nextPage")]
    public CsePersonsWorkSet NextPage
    {
      get => nextPage ??= new();
      set => nextPage = value;
    }

    /// <summary>
    /// A value of Format.
    /// </summary>
    [JsonPropertyName("format")]
    public CsePersonsWorkSet Format
    {
      get => format ??= new();
      set => format = value;
    }

    /// <summary>
    /// A value of Mother.
    /// </summary>
    [JsonPropertyName("mother")]
    public CsePersonsWorkSet Mother
    {
      get => mother ??= new();
      set => mother = value;
    }

    /// <summary>
    /// A value of Father.
    /// </summary>
    [JsonPropertyName("father")]
    public CsePersonsWorkSet Father
    {
      get => father ??= new();
      set => father = value;
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
    /// A value of Child1.
    /// </summary>
    [JsonPropertyName("child1")]
    public Common Child1
    {
      get => child1 ??= new();
      set => child1 = value;
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
    /// A value of Ap1.
    /// </summary>
    [JsonPropertyName("ap1")]
    public Common Ap1
    {
      get => ap1 ??= new();
      set => ap1 = value;
    }

    /// <summary>
    /// A value of NullMother.
    /// </summary>
    [JsonPropertyName("nullMother")]
    public CsePersonsWorkSet NullMother
    {
      get => nullMother ??= new();
      set => nullMother = value;
    }

    private Common counterOfSelections;
    private NextTranInfo null1;
    private WorkArea print;
    private SpDocLiteral spDocLiteral;
    private Common position;
    private CsePersonsWorkSet blank;
    private Common selected;
    private WorkArea nextPageStatus;
    private CsePersonsWorkSet nextPage;
    private CsePersonsWorkSet format;
    private CsePersonsWorkSet mother;
    private CsePersonsWorkSet father;
    private CsePersonsWorkSet ar;
    private Common child1;
    private CaseRole caseRole;
    private Common ap1;
    private CsePersonsWorkSet nullMother;
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

    private Case1 case1;
  }
#endregion
}
