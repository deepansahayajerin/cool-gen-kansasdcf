// Program: SP_CRIN_CASE_REVIEW_INITIAL, ID: 373539544, model: 746.
// Short name: SWECRINP
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
/// A program: SP_CRIN_CASE_REVIEW_INITIAL.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpCrinCaseReviewInitial: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CRIN_CASE_REVIEW_INITIAL program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCrinCaseReviewInitial(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCrinCaseReviewInitial.
  /// </summary>
  public SpCrinCaseReviewInitial(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -----------------------------------------------------------------------------
    //        M A I N T E N A N C E   L O G
    // Date    Developer       req #   Description
    // 03/14/96 Alan Hackler            Retro fits
    // 1/10/97  R. Welborn		 Conversion 				 from Plan Task Notes to Narrative.
    // 04/30/97 R. Grey		 Change Current Date
    // 05/15/97 R. Grey		 Clean up - More AP msgs
    // 07/22/97 R. Grey		 Add review closed case
    // 03/04/99 N.Engoor                Added two new PF KEYS and new  fields on
    // the screen.
    // 03/04/99 N.Engoor                Fixed code regarding display of 
    // narrative info.
    // 03/10/99 N.Engoor                Deleted  redundant READs.
    // 01/24/2000 V.Madhira      PR# 86247    Added new pickup indicator for AP
    // on CRIN screen. New code is added to implement this functionality.
    // -------------------------------------------------------------------------------
    // ----------------------------------------------------------------------------------
    // Date       Developer       req #    Description
    // 01-25-2000 V. Madhira  PR# 86247  : The screen design and functionality 
    // changed according to this PR. Now a new pickup indicator will be
    // displayed adjacent to AP info. on the screen and will be protected. If
    // there are multiple AP's on the case, the indicator will be unprotected
    // and user can enter an 'S' in the indicator field and will press PF4 to
    // flow to COMP. On COMP user can select an AP and will return to this
    // screen with the selected AP.
    //                      We must now create/modify/display the data based on 
    // the 'case_number' and 'cse_person_number'.
    //                                            
    // ----- Vithal (01-25-2000)
    // 08/08/2000 SWSRCHF WR# 000170  Replace the read for Narrative by a read 
    // for
    //                                
    // Narrative Detail
    // ---------------------------------------------------------------------------------------
    // ----------------------------------------------------------------------------
    // 11/07/2001 K Doshi PR131584
    // Fix screen help Id problem.
    // 02/25/2002	M Ramirez	PR139864
    // Added CREN mod order ind.  After a modification review is completed the 
    // user is asked whether they think this review will result in a
    // modification to the order.  If they answer N then we add special text to
    // the infrastructure detail line.  Also, the indicator is displayed on this
    // screen
    // 04/01/2011	T Pierce	CQ# 23212	Removed references to obsolete Narrative 
    // entity type.
    // 06/22/2011      RMathews        CQ28359         Display most recent case 
    // review when there are
    //                                                 
    // no active AP's on the case
    // -----------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.Case1.Number = import.Case1.Number;
    export.ClosedCaseIndicator.Flag = import.ClosedCaseIndicator.Flag;
    export.Hidden.Assign(import.Hidden);
    export.ModOrPer.Assign(import.ModOrPer);
    local.Current.Date = Now().Date;
    local.Current.Timestamp = Now();
    export.Flag.Flag = import.Flag.Flag;
    export.HiddenReviewType.ActionEntry = import.HiddenReviewType.ActionEntry;
    MoveInfrastructure2(import.HiddenOrigAndPass, export.HiddenOrigAndPass);
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.CommandPassedFromEnfor.Flag = import.CommandPassedFromEnfor.Flag;
    MoveCommon(import.MultiAp, export.MultiAp);
    export.ApSelected.Flag = import.ApSelected.Flag;
    MoveCsePersonsWorkSet2(import.SelectedAp, export.SelectedAp);

    for(import.PassReviewNotes.Index = 0; import.PassReviewNotes.Index < import
      .PassReviewNotes.Count; ++import.PassReviewNotes.Index)
    {
      if (!import.PassReviewNotes.CheckSize())
      {
        break;
      }

      export.PassReviewNote.Index = import.PassReviewNotes.Index;
      export.PassReviewNote.CheckSize();

      export.PassReviewNote.Update.GexportHidden.Assign(
        import.PassReviewNotes.Item.GimportHidden);
      export.PassReviewNote.Update.GexportHiddenPassedFlag.Flag =
        import.PassReviewNotes.Item.GimportHiddenPassedFlag.Flag;
    }

    import.PassReviewNotes.CheckIndex();

    if (!Equal(global.Command, "DISPLAY") && !Equal(global.Command, "XXFMMENU"))
    {
      export.CaseCoordinator.Assign(import.CaseCoordinator);
      MoveOffice(import.Case2, export.Case2);
      MoveCsePersonsWorkSet2(import.Ar, export.Ar);
      MoveCsePersonsWorkSet2(import.Ap1, export.Ap1);
      export.ServiceProvider.Assign(import.ServiceProvider);

      // ------------------------
      // N.Engoor  -  03/03/99
      // Added two new fields on the screen. Corresponding move stmnts added.
      // ------------------------
      MoveServiceProvider(import.Import2, export.Export2);
      MoveDateWorkArea(import.Import2LastReview, export.Export2LastReview);

      // mjr
      // --------------------------------------------
      // 02/25/2002
      // PR139864 - Added review resulted in modification
      // ---------------------------------------------------------
      export.LastRvwResultedInMod.Flag = import.LastRvwResultedInMod.Flag;
      MoveOffice(import.Case2, export.Case2);
      export.FunctionDesc.Text4 = import.FunctionDesc.Text4;
      export.MoreApsMsg.Text30 = import.MoreApsMsg.Text30;
      MoveDateWorkArea(import.LastReview, export.LastReview);
      export.Pgm1.Code = import.Pgm1.Code;
      export.Pgm2.Code = import.Pgm2.Code;
      export.Pgm3.Code = import.Pgm3.Code;
      export.Pgm4.Code = import.Pgm4.Code;
      export.Pgm5.Code = import.Pgm5.Code;
      export.Pgm6.Code = import.Pgm6.Code;
      export.Pgm7.Code = import.Pgm7.Code;
      export.Pgm8.Code = import.Pgm8.Code;

      export.Program.Index = 0;
      export.Program.Clear();

      for(import.Program.Index = 0; import.Program.Index < import
        .Program.Count; ++import.Program.Index)
      {
        if (export.Program.IsFull)
        {
          break;
        }

        export.Program.Update.Program1.Code = import.Program.Item.Program1.Code;
        export.Program.Next();
      }

      export.DisplayReviewNote.Index = 0;
      export.DisplayReviewNote.Clear();

      for(import.DisplayReviewNote.Index = 0; import.DisplayReviewNote.Index < import
        .DisplayReviewNote.Count; ++import.DisplayReviewNote.Index)
      {
        if (export.DisplayReviewNote.IsFull)
        {
          break;
        }

        export.DisplayReviewNote.Update.G.Text =
          import.DisplayReviewNote.Item.G.Text;
        export.DisplayReviewNote.Next();
      }
    }

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      // -----------
      // N.Engoor - 03/05/99
      // If Nexttran field is populated and any valid PF KEY is pressed ignore 
      // the data in the Nexttran field and do the function of the PF key
      // entered.
      // -----------
      if (Equal(global.Command, "INVALID"))
      {
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        return;
      }
      else
      {
        if (!Equal(global.Command, "ENTER"))
        {
          goto Test1;
        }

        // -----------
        // This is where you would set the local next_tran_info attributes to 
        // the import view attributes for the data to be passed to the next
        // transaction.
        // -----------
        export.Hidden.Assign(import.Hidden);
        export.Hidden.CaseNumber = export.Case1.Number;
        UseScCabNextTranPut();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          var field = GetField(export.Standard, "nextTransaction");

          field.Error = true;
        }
      }

      return;
    }

Test1:

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ----------------
      // This is where you set your export value to the export hidden next tran 
      // values if the user is comming into this procedure on a next tran
      // action.
      // ----------------
      UseScCabNextTranGet();

      if (!IsEmpty(export.Hidden.CaseNumber))
      {
        export.Case1.Number = export.Hidden.CaseNumber ?? Spaces(10);
        global.Command = "DISPLAY";
      }
      else
      {
      }
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      // ----------------
      // This is where you set your command to do whatever is necessary to do on
      // a flow from the menu, maybe just escape....
      // ----------------
      // You should get this information from the Dialog Flow Diagram.  It is 
      // the SEND CMD on the propertis for a Transfer from one  procedure to
      // another.
      // *** the statement would read COMMAND IS display   *****
      // *** if the dialog flow property was display first, just add an escape 
      // completely out of the procedure  ****
      global.Command = "DISPLAY";
    }

    switch(TrimEnd(global.Command))
    {
      case "EXIT":
        ExitState = "ECO_XFR_TO_MENU";

        return;
      case "CADS":
        ExitState = "ECO_LNK_TO_LST_CASE_DETAILS";

        return;
      case "PEPR":
        ExitState = "ECO_LNK_TO_PEPR";

        return;
      case "NATE":
        if (export.DisplayReviewNote.IsEmpty)
        {
          ExitState = "SP0000_FIRST_REVIEW_4_CASE";

          return;
        }

        if (Equal(import.HiddenReviewType.ActionEntry, "R"))
        {
          ExitState = "SI0000_CANT_FLOW_TO_NATE_IF_REVW";

          return;
        }

        export.ModOrPer.UserId = "CRIN";
        ExitState = "ECO_LNK_TO_NATE";

        return;
      case "COMP":
        ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

        return;
      case "RETCOMP":
        // -----------------------------------------------------------------------
        // Check if user selected an AP or not.
        // ----------------------------------------------------------------------
        if (ReadCsePersonCaseRole1())
        {
          var field = GetField(export.MultiAp, "selectChar");

          field.Color = "green";
          field.Protected = false;

          export.MultiAp.SelectChar = "+";

          // -----------------------------------------------------------------------
          // Per PR# 86247: Clear the review notes and read review notes 
          // associated to the AP_Number selected on the COMP screen. Here first
          // clear the group view.
          //                                            
          // --- Vithal (01-25-2000)
          // -----------------------------------------------------------------------
          for(export.DisplayReviewNote.Index = 0; export
            .DisplayReviewNote.Index < export.DisplayReviewNote.Count; ++
            export.DisplayReviewNote.Index)
          {
            export.DisplayReviewNote.Update.G.Text = local.NarrativeWork.Text;
          }

          for(export.PassReviewNote.Index = 0; export.PassReviewNote.Index < export
            .PassReviewNote.Count; ++export.PassReviewNote.Index)
          {
            if (!export.PassReviewNote.CheckSize())
            {
              break;
            }

            export.HiddenExportModified.Update.HiddenPassedFlag.Flag =
              local.ExportHiddenPassed.Flag;
            export.PassReviewNote.Update.GexportHidden.Assign(
              local.ExpModifiedNarrative);
          }

          export.PassReviewNote.CheckIndex();
          global.Command = "DISPLAY";
        }
        else
        {
          export.Ap1.Number = "";
          export.Ap1.FormattedName = "";
          export.MultiAp.SelectChar = "+";

          var field1 = GetField(export.MultiAp, "selectChar");

          field1.Color = "green";
          field1.Protected = false;

          var field2 = GetField(export.Ap1, "number");

          field2.Error = true;

          var field3 = GetField(export.Ap1, "formattedName");

          field3.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

          return;
        }

        break;
      default:
        break;
    }

    local.Save.Command = global.Command;

    if (Equal(export.HiddenReviewType.ActionEntry, "M"))
    {
      global.Command = "REVMOD";
    }

    if (Equal(export.HiddenReviewType.ActionEntry, "P"))
    {
      global.Command = "REVPER";
    }

    if (Equal(export.HiddenReviewType.ActionEntry, "R"))
    {
      global.Command = "REVIEW";
    }

    // to validate action level security
    if (Equal(global.Command, "REVMOD") || Equal(global.Command, "REVPER") || Equal
      (global.Command, "ENTER") || Equal(global.Command, "REVIEW"))
    {
      UseScCabTestSecurity();
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    global.Command = local.Save.Command;

    if (AsChar(import.CommandPassedFromEnfor.Flag) == 'A')
    {
      global.Command = "ADD";
    }

    if (Equal(global.Command, "ADD"))
    {
      global.Command = "DISPLAY";

      if (Equal(import.HiddenReviewType.ActionEntry, "P"))
      {
        export.PassReviewNote.Index = 0;
        export.PassReviewNote.CheckSize();

        export.PassReviewNote.Update.GexportHidden.Text = "LOCATE -- " + TrimEnd
          (export.PassReviewNote.Item.GexportHidden.Text);

        export.PassReviewNote.Index = 1;
        export.PassReviewNote.CheckSize();

        export.PassReviewNote.Update.GexportHidden.Text = "MEDICAL -- " + TrimEnd
          (export.PassReviewNote.Item.GexportHidden.Text);

        export.PassReviewNote.Index = 2;
        export.PassReviewNote.CheckSize();

        export.PassReviewNote.Update.GexportHidden.Text = "PATERNITY -- " + TrimEnd
          (export.PassReviewNote.Item.GexportHidden.Text);

        export.PassReviewNote.Index = 3;
        export.PassReviewNote.CheckSize();

        export.PassReviewNote.Update.GexportHidden.Text =
          "ESTABLISHMENT -- " + TrimEnd
          (export.PassReviewNote.Item.GexportHidden.Text);

        export.PassReviewNote.Index = 4;
        export.PassReviewNote.CheckSize();

        export.PassReviewNote.Update.GexportHidden.Text = "ENFORCEMENT -- " + TrimEnd
          (export.PassReviewNote.Item.GexportHidden.Text);
      }

      if (Equal(import.HiddenReviewType.ActionEntry, "M"))
      {
        export.PassReviewNote.Index = 0;
        export.PassReviewNote.CheckSize();

        export.PassReviewNote.Update.GexportHidden.Text =
          "MOD REVIEW LOCATE -- " + TrimEnd
          (export.PassReviewNote.Item.GexportHidden.Text);

        export.PassReviewNote.Index = 1;
        export.PassReviewNote.CheckSize();

        export.PassReviewNote.Update.GexportHidden.Text =
          "MOD REVIEW MEDICAL -- " + TrimEnd
          (export.PassReviewNote.Item.GexportHidden.Text);

        export.PassReviewNote.Index = 2;
        export.PassReviewNote.CheckSize();

        export.PassReviewNote.Update.GexportHidden.Text =
          "MOD REVIEW PATERNITY -- " + TrimEnd
          (export.PassReviewNote.Item.GexportHidden.Text);

        export.PassReviewNote.Index = 3;
        export.PassReviewNote.CheckSize();

        export.PassReviewNote.Update.GexportHidden.Text =
          "MOD REVIEW ESTABLISHMENT -- " + TrimEnd
          (export.PassReviewNote.Item.GexportHidden.Text);

        export.PassReviewNote.Index = 4;
        export.PassReviewNote.CheckSize();

        export.PassReviewNote.Update.GexportHidden.Text =
          "MOD REVIEW ENFORCEMENT -- " + TrimEnd
          (export.PassReviewNote.Item.GexportHidden.Text);
      }

      if (Equal(import.HiddenReviewType.ActionEntry, "R"))
      {
      }
      else
      {
        // ------------------
        // Create infrastructure record for case, date, and create associated 
        // narratives.
        // ------------------
        local.ReviewType.InitiatingStateCode = "KS";
        local.ReviewType.SituationNumber = 0;
        local.GetDateFromTimestamp.Timestamp = local.Current.Timestamp;
        local.GetDateFromTimestamp.Date = local.Current.Date;
        UseCabConvertDate2String();
      }

      if (Equal(import.HiddenReviewType.ActionEntry, "P"))
      {
        // *******************************************
        // PERIODIC REVIEW TASK
        // *******************************************
        local.ReviewType.EventId = 5;
        local.ReviewType.EventType = "CASE";
        local.ReviewType.ReasonCode = "OSPRVWS";
        local.ReviewType.Detail = "Annual review completed by ";
        local.ReviewType.Detail = (local.ReviewType.Detail ?? "") + global
          .UserId;
        local.ReviewType.Detail = TrimEnd((local.ReviewType.Detail ?? "") + " on " +
          local.Date.Text8);
      }
      else if (Equal(import.HiddenReviewType.ActionEntry, "M"))
      {
        // *******************************************
        // MODIFICATION REVIEW TASK
        // *******************************************
        local.ReviewType.EventId = 8;
        local.ReviewType.EventType = "MODFN";
        local.ReviewType.ReasonCode = "MODFNRVWDT";

        // mjr
        // ---------------------------------------------
        // 02/25/2002
        // PR139864 - Added review resulted in modification
        // Changed detail line to reflect whether a modification will
        // be done as a result of this review
        // ----------------------------------------------------------
        local.ReviewType.DenormText12 = import.CrenModOrderInd.Flag;

        if (AsChar(import.CrenModOrderInd.Flag) == 'N')
        {
          local.ReviewType.Detail =
            "CS Mod Review completed - will not pursue modification.";
        }
        else
        {
          local.ReviewType.Detail = "CS Mod Review completed. ";
        }
      }

      if (Equal(import.HiddenReviewType.ActionEntry, "P") || Equal
        (import.HiddenReviewType.ActionEntry, "M"))
      {
        local.ReviewType.BusinessObjectCd = "CAS";
        local.ReviewType.CreatedBy = global.UserId;
        local.ReviewType.CaseNumber = export.Case1.Number;
        local.ReviewType.UserId = "CRIN";
        local.ReviewType.ReferenceDate = local.Current.Date;
        local.ReviewType.ProcessStatus = "Q";

        // -------------------------------------------------------------------------
        // Per PR# 86247  the 'cse_person_number' field in 'Infrastructure' 
        // record will created with the 'export_ap1_cse_persons_workset_number'
        // displayed on the screen. The user may select this 'AP_Number'  by
        // flowing to COMP screen.
        //                                                 
        // -------Vithal (01-25-2000)
        // -------------------------------------------------------------------------------
        local.ReviewType.CsePersonNumber = export.Ap1.Number;

        if (AsChar(export.ApSelected.Flag) == 'Y')
        {
          local.ReviewType.CsePersonNumber = export.SelectedAp.Number;
        }
        else if (ReadCase())
        {
          if (ReadCsePersonCaseRole2())
          {
            local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
            UseSiReadCsePerson();

            if (!IsEmpty(local.AbendData.Type1))
            {
              return;
            }

            local.ReviewType.CsePersonNumber = local.CsePersonsWorkSet.Number;
          }
        }
        else
        {
          var field = GetField(export.Case1, "number");

          field.Error = true;

          ExitState = "CASE_NF";

          return;
        }

        UseSpCabCreateInfrastructure();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        local.ForUpdateAndCreate.Timestamp = local.Current.Timestamp;

        // *** Work request 000170
        // *** 08/08/2000 SWSRCHF
        local.GetDateFromTimestamp.Timestamp = Now();

        if (ReadInfrastructure1())
        {
          local.ProgramCount.Count = 1;

          for(export.PassReviewNote.Index = 0; export.PassReviewNote.Index < export
            .PassReviewNote.Count; ++export.PassReviewNote.Index)
          {
            if (!export.PassReviewNote.CheckSize())
            {
              break;
            }

            // *** Work request 000170
            // *** 08/08/2000 SWSRCHF
            // *** start
            if (Equal(export.PassReviewNote.Item.GexportHidden.Text, 1, 10,
              "LOCATE -- "))
            {
              local.StartingPosition.Count = 1;
              local.Length.Count = 68;
              local.Work.LineNumber = 1;
              local.Work.NarrativeText =
                Substring(export.PassReviewNote.Item.GexportHidden.Text,
                local.StartingPosition.Count, local.Length.Count);

              while(!IsEmpty(local.Work.NarrativeText))
              {
                try
                {
                  CreateNarrativeDetail();
                  local.CaseReviewCompleted.Flag = "Y";
                  local.StartingPosition.Count += local.Length.Count;

                  if (local.StartingPosition.Count >= 232)
                  {
                    goto Next;
                  }

                  local.Length.Count = 58;

                  if (IsEmpty(Substring(
                    export.PassReviewNote.Item.GexportHidden.Text,
                    local.StartingPosition.Count, local.Length.Count)))
                  {
                    goto Next;
                  }

                  local.Work.NarrativeText = "LOCATE -- " + Substring
                    (export.PassReviewNote.Item.GexportHidden.Text,
                    NarrativeWork.Text_MaxLength, local.StartingPosition.Count,
                    local.Length.Count);
                  ++local.Work.LineNumber;
                }
                catch(Exception e)
                {
                  switch(GetErrorCode(e))
                  {
                    case ErrorCode.AlreadyExists:
                      ExitState = "SP0000_NARRATIVE_DETAIL_AE";

                      return;
                    case ErrorCode.PermittedValueViolation:
                      ExitState = "SP0000_NARRATIVE_DETAIL_PV";

                      return;
                    case ErrorCode.DatabaseError:
                      break;
                    default:
                      throw;
                  }
                }
              }
            }

            if (Equal(export.PassReviewNote.Item.GexportHidden.Text, 1, 11,
              "MEDICAL -- "))
            {
              local.StartingPosition.Count = 1;
              local.Length.Count = 68;
              ++local.Work.LineNumber;
              local.Work.NarrativeText =
                Substring(export.PassReviewNote.Item.GexportHidden.Text,
                local.StartingPosition.Count, local.Length.Count);

              while(!IsEmpty(local.Work.NarrativeText))
              {
                try
                {
                  CreateNarrativeDetail();
                  local.CaseReviewCompleted.Flag = "Y";
                  local.StartingPosition.Count += local.Length.Count;

                  if (local.StartingPosition.Count >= 228)
                  {
                    goto Next;
                  }

                  local.Length.Count = 57;

                  if (IsEmpty(Substring(
                    export.PassReviewNote.Item.GexportHidden.Text,
                    local.StartingPosition.Count, local.Length.Count)))
                  {
                    goto Next;
                  }

                  local.Work.NarrativeText = "MEDICAL -- " + Substring
                    (export.PassReviewNote.Item.GexportHidden.Text,
                    NarrativeWork.Text_MaxLength, local.StartingPosition.Count,
                    local.Length.Count);
                  ++local.Work.LineNumber;
                }
                catch(Exception e)
                {
                  switch(GetErrorCode(e))
                  {
                    case ErrorCode.AlreadyExists:
                      ExitState = "SP0000_NARRATIVE_DETAIL_AE";

                      return;
                    case ErrorCode.PermittedValueViolation:
                      ExitState = "SP0000_NARRATIVE_DETAIL_PV";

                      return;
                    case ErrorCode.DatabaseError:
                      break;
                    default:
                      throw;
                  }
                }
              }
            }

            if (Equal(export.PassReviewNote.Item.GexportHidden.Text, 1, 13,
              "PATERNITY -- "))
            {
              local.StartingPosition.Count = 1;
              local.Length.Count = 68;
              ++local.Work.LineNumber;
              local.Work.NarrativeText =
                Substring(export.PassReviewNote.Item.GexportHidden.Text,
                local.StartingPosition.Count, local.Length.Count);

              while(!IsEmpty(local.Work.NarrativeText))
              {
                try
                {
                  CreateNarrativeDetail();
                  local.CaseReviewCompleted.Flag = "Y";
                  local.StartingPosition.Count += local.Length.Count;

                  if (local.StartingPosition.Count >= 220)
                  {
                    goto Next;
                  }

                  local.Length.Count = 55;

                  if (IsEmpty(Substring(
                    export.PassReviewNote.Item.GexportHidden.Text,
                    local.StartingPosition.Count, local.Length.Count)))
                  {
                    goto Next;
                  }

                  local.Work.NarrativeText = "PATERNITY -- " + Substring
                    (export.PassReviewNote.Item.GexportHidden.Text,
                    NarrativeWork.Text_MaxLength, local.StartingPosition.Count,
                    local.Length.Count);
                  ++local.Work.LineNumber;
                }
                catch(Exception e)
                {
                  switch(GetErrorCode(e))
                  {
                    case ErrorCode.AlreadyExists:
                      ExitState = "SP0000_NARRATIVE_DETAIL_AE";

                      return;
                    case ErrorCode.PermittedValueViolation:
                      ExitState = "SP0000_NARRATIVE_DETAIL_PV";

                      return;
                    case ErrorCode.DatabaseError:
                      break;
                    default:
                      throw;
                  }
                }
              }
            }

            if (Equal(export.PassReviewNote.Item.GexportHidden.Text, 1, 17,
              "ESTABLISHMENT -- "))
            {
              local.StartingPosition.Count = 1;
              local.Length.Count = 68;
              ++local.Work.LineNumber;
              local.Work.NarrativeText =
                Substring(export.PassReviewNote.Item.GexportHidden.Text,
                local.StartingPosition.Count, local.Length.Count);

              while(!IsEmpty(local.Work.NarrativeText))
              {
                try
                {
                  CreateNarrativeDetail();
                  local.CaseReviewCompleted.Flag = "Y";
                  local.StartingPosition.Count += local.Length.Count;

                  if (local.StartingPosition.Count >= 204)
                  {
                    goto Next;
                  }

                  local.Length.Count = 51;

                  if (IsEmpty(Substring(
                    export.PassReviewNote.Item.GexportHidden.Text,
                    local.StartingPosition.Count, local.Length.Count)))
                  {
                    goto Next;
                  }

                  local.Work.NarrativeText = "ESTABLISHMENT -- " + Substring
                    (export.PassReviewNote.Item.GexportHidden.Text,
                    NarrativeWork.Text_MaxLength, local.StartingPosition.Count,
                    local.Length.Count);
                  ++local.Work.LineNumber;
                }
                catch(Exception e)
                {
                  switch(GetErrorCode(e))
                  {
                    case ErrorCode.AlreadyExists:
                      ExitState = "SP0000_NARRATIVE_DETAIL_AE";

                      return;
                    case ErrorCode.PermittedValueViolation:
                      ExitState = "SP0000_NARRATIVE_DETAIL_PV";

                      return;
                    case ErrorCode.DatabaseError:
                      break;
                    default:
                      throw;
                  }
                }
              }
            }

            if (Equal(export.PassReviewNote.Item.GexportHidden.Text, 1, 15,
              "ENFORCEMENT -- "))
            {
              local.StartingPosition.Count = 1;
              local.Length.Count = 68;
              ++local.Work.LineNumber;
              local.Work.NarrativeText =
                Substring(export.PassReviewNote.Item.GexportHidden.Text,
                local.StartingPosition.Count, local.Length.Count);

              while(!IsEmpty(local.Work.NarrativeText))
              {
                try
                {
                  CreateNarrativeDetail();
                  local.CaseReviewCompleted.Flag = "Y";
                  local.StartingPosition.Count += local.Length.Count;

                  if (local.StartingPosition.Count >= 212)
                  {
                    goto Next;
                  }

                  local.Length.Count = 53;

                  if (IsEmpty(Substring(
                    export.PassReviewNote.Item.GexportHidden.Text,
                    local.StartingPosition.Count, local.Length.Count)))
                  {
                    goto Next;
                  }

                  local.Work.NarrativeText = "ENFORCEMENT -- " + Substring
                    (export.PassReviewNote.Item.GexportHidden.Text,
                    NarrativeWork.Text_MaxLength, local.StartingPosition.Count,
                    local.Length.Count);
                  ++local.Work.LineNumber;
                }
                catch(Exception e)
                {
                  switch(GetErrorCode(e))
                  {
                    case ErrorCode.AlreadyExists:
                      ExitState = "SP0000_NARRATIVE_DETAIL_AE";

                      return;
                    case ErrorCode.PermittedValueViolation:
                      ExitState = "SP0000_NARRATIVE_DETAIL_PV";

                      return;
                    case ErrorCode.DatabaseError:
                      break;
                    default:
                      throw;
                  }
                }
              }
            }

            if (Equal(export.PassReviewNote.Item.GexportHidden.Text, 1, 21,
              "MOD REVIEW LOCATE -- "))
            {
              local.StartingPosition.Count = 1;
              local.Length.Count = 68;
              local.Work.LineNumber = 1;
              local.Work.NarrativeText =
                Substring(export.PassReviewNote.Item.GexportHidden.Text,
                local.StartingPosition.Count, local.Length.Count);

              while(!IsEmpty(local.Work.NarrativeText))
              {
                try
                {
                  CreateNarrativeDetail();
                  local.CaseReviewCompleted.Flag = "Y";
                  local.StartingPosition.Count += local.Length.Count;

                  if (local.StartingPosition.Count >= 232)
                  {
                    goto Next;
                  }

                  local.Length.Count = 47;

                  if (IsEmpty(Substring(
                    export.PassReviewNote.Item.GexportHidden.Text,
                    local.StartingPosition.Count, local.Length.Count)))
                  {
                    goto Next;
                  }

                  local.Work.NarrativeText = "MOD REVIEW LOCATE -- " + Substring
                    (export.PassReviewNote.Item.GexportHidden.Text,
                    NarrativeWork.Text_MaxLength, local.StartingPosition.Count,
                    local.Length.Count);
                  ++local.Work.LineNumber;
                }
                catch(Exception e)
                {
                  switch(GetErrorCode(e))
                  {
                    case ErrorCode.AlreadyExists:
                      ExitState = "SP0000_NARRATIVE_DETAIL_AE";

                      return;
                    case ErrorCode.PermittedValueViolation:
                      ExitState = "SP0000_NARRATIVE_DETAIL_PV";

                      return;
                    case ErrorCode.DatabaseError:
                      break;
                    default:
                      throw;
                  }
                }
              }
            }

            if (Equal(export.PassReviewNote.Item.GexportHidden.Text, 1, 22,
              "MOD REVIEW MEDICAL -- "))
            {
              local.StartingPosition.Count = 1;
              local.Length.Count = 68;
              ++local.Work.LineNumber;
              local.Work.NarrativeText =
                Substring(export.PassReviewNote.Item.GexportHidden.Text,
                local.StartingPosition.Count, local.Length.Count);

              while(!IsEmpty(local.Work.NarrativeText))
              {
                try
                {
                  CreateNarrativeDetail();
                  local.CaseReviewCompleted.Flag = "Y";
                  local.StartingPosition.Count += local.Length.Count;

                  if (local.StartingPosition.Count >= 228)
                  {
                    goto Next;
                  }

                  local.Length.Count = 46;

                  if (IsEmpty(Substring(
                    export.PassReviewNote.Item.GexportHidden.Text,
                    local.StartingPosition.Count, local.Length.Count)))
                  {
                    goto Next;
                  }

                  local.Work.NarrativeText = "MOD REVIEW MEDICAL -- " + Substring
                    (export.PassReviewNote.Item.GexportHidden.Text,
                    NarrativeWork.Text_MaxLength, local.StartingPosition.Count,
                    local.Length.Count);
                  ++local.Work.LineNumber;
                }
                catch(Exception e)
                {
                  switch(GetErrorCode(e))
                  {
                    case ErrorCode.AlreadyExists:
                      ExitState = "SP0000_NARRATIVE_DETAIL_AE";

                      return;
                    case ErrorCode.PermittedValueViolation:
                      ExitState = "SP0000_NARRATIVE_DETAIL_PV";

                      return;
                    case ErrorCode.DatabaseError:
                      break;
                    default:
                      throw;
                  }
                }
              }
            }

            if (Equal(export.PassReviewNote.Item.GexportHidden.Text, 1, 24,
              "MOD REVIEW PATERNITY -- "))
            {
              local.StartingPosition.Count = 1;
              local.Length.Count = 68;
              ++local.Work.LineNumber;
              local.Work.NarrativeText =
                Substring(export.PassReviewNote.Item.GexportHidden.Text,
                local.StartingPosition.Count, local.Length.Count);

              while(!IsEmpty(local.Work.NarrativeText))
              {
                try
                {
                  CreateNarrativeDetail();
                  local.CaseReviewCompleted.Flag = "Y";
                  local.StartingPosition.Count += local.Length.Count;

                  if (local.StartingPosition.Count >= 220)
                  {
                    goto Next;
                  }

                  local.Length.Count = 44;

                  if (IsEmpty(Substring(
                    export.PassReviewNote.Item.GexportHidden.Text,
                    local.StartingPosition.Count, local.Length.Count)))
                  {
                    goto Next;
                  }

                  local.Work.NarrativeText = "MOD REVIEW PATERNITY -- " + Substring
                    (export.PassReviewNote.Item.GexportHidden.Text,
                    NarrativeWork.Text_MaxLength, local.StartingPosition.Count,
                    local.Length.Count);
                  ++local.Work.LineNumber;
                }
                catch(Exception e)
                {
                  switch(GetErrorCode(e))
                  {
                    case ErrorCode.AlreadyExists:
                      ExitState = "SP0000_NARRATIVE_DETAIL_AE";

                      return;
                    case ErrorCode.PermittedValueViolation:
                      ExitState = "SP0000_NARRATIVE_DETAIL_PV";

                      return;
                    case ErrorCode.DatabaseError:
                      break;
                    default:
                      throw;
                  }
                }
              }
            }

            if (Equal(export.PassReviewNote.Item.GexportHidden.Text, 1, 28,
              "MOD REVIEW ESTABLISHMENT -- "))
            {
              local.StartingPosition.Count = 1;
              local.Length.Count = 68;
              ++local.Work.LineNumber;
              local.Work.NarrativeText =
                Substring(export.PassReviewNote.Item.GexportHidden.Text,
                local.StartingPosition.Count, local.Length.Count);

              while(!IsEmpty(local.Work.NarrativeText))
              {
                try
                {
                  CreateNarrativeDetail();
                  local.CaseReviewCompleted.Flag = "Y";
                  local.StartingPosition.Count += local.Length.Count;

                  if (local.StartingPosition.Count >= 204)
                  {
                    goto Next;
                  }

                  local.Length.Count = 40;

                  if (IsEmpty(Substring(
                    export.PassReviewNote.Item.GexportHidden.Text,
                    local.StartingPosition.Count, local.Length.Count)))
                  {
                    goto Next;
                  }

                  local.Work.NarrativeText = "MOD REVIEW ESTABLISHMENT -- " + Substring
                    (export.PassReviewNote.Item.GexportHidden.Text,
                    NarrativeWork.Text_MaxLength, local.StartingPosition.Count,
                    local.Length.Count);
                  ++local.Work.LineNumber;
                }
                catch(Exception e)
                {
                  switch(GetErrorCode(e))
                  {
                    case ErrorCode.AlreadyExists:
                      ExitState = "SP0000_NARRATIVE_DETAIL_AE";

                      return;
                    case ErrorCode.PermittedValueViolation:
                      ExitState = "SP0000_NARRATIVE_DETAIL_PV";

                      return;
                    case ErrorCode.DatabaseError:
                      break;
                    default:
                      throw;
                  }
                }
              }
            }

            if (Equal(export.PassReviewNote.Item.GexportHidden.Text, 1, 26,
              "MOD REVIEW ENFORCEMENT -- "))
            {
              local.StartingPosition.Count = 1;
              local.Length.Count = 68;
              ++local.Work.LineNumber;
              local.Work.NarrativeText =
                Substring(export.PassReviewNote.Item.GexportHidden.Text,
                local.StartingPosition.Count, local.Length.Count);

              while(!IsEmpty(local.Work.NarrativeText))
              {
                try
                {
                  CreateNarrativeDetail();
                  local.CaseReviewCompleted.Flag = "Y";
                  local.StartingPosition.Count += local.Length.Count;

                  if (local.StartingPosition.Count >= 212)
                  {
                    goto Next;
                  }

                  local.Length.Count = 42;

                  if (IsEmpty(Substring(
                    export.PassReviewNote.Item.GexportHidden.Text,
                    local.StartingPosition.Count, local.Length.Count)))
                  {
                    goto Next;
                  }

                  local.Work.NarrativeText = "MOD REVIEW ENFORCEMENT -- " + Substring
                    (export.PassReviewNote.Item.GexportHidden.Text,
                    NarrativeWork.Text_MaxLength, local.StartingPosition.Count,
                    local.Length.Count);
                  ++local.Work.LineNumber;
                }
                catch(Exception e)
                {
                  switch(GetErrorCode(e))
                  {
                    case ErrorCode.AlreadyExists:
                      ExitState = "SP0000_NARRATIVE_DETAIL_AE";

                      return;
                    case ErrorCode.PermittedValueViolation:
                      ExitState = "SP0000_NARRATIVE_DETAIL_PV";

                      return;
                    case ErrorCode.DatabaseError:
                      break;
                    default:
                      throw;
                  }
                }
              }
            }

            // *** end
            // *** 08/08/2000 SWSRCHF
            // *** Work request 000170
Next:
            ;
          }

          export.PassReviewNote.CheckIndex();
        }
        else
        {
          ExitState = "SP0000_INFRASTRUCTURE_CASE_NF";
        }
      }

      // mjr
      // ---------------------------------------
      // 02/27/2002
      // PR139864 - Add batch documents notifying AP and AR
      // that no change will be sought in there support order
      // ----------------------------------------------------
      if (Equal(import.HiddenReviewType.ActionEntry, "M"))
      {
        if (AsChar(import.CrenModOrderInd.Flag) == 'N')
        {
          local.SpDocKey.KeyAp = local.ReviewType.CsePersonNumber ?? Spaces(10);
          local.SpDocKey.KeyCase = local.ReviewType.CaseNumber ?? Spaces(10);
          local.Document.Type1 = "BTCH";
          local.Document.Name = "APMODDEN";
          UseSpCreateDocumentInfrastruct();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          if (ReadCsePerson2())
          {
            local.Document.Name = "ARMODDEN";
            UseSpCabDetermineInterstateDoc();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }
          }
        }
      }

      // ***********************************************************
      //   REVIEW COMPLETE. RE-DISPLAY.
      // ***********************************************************
    }

    switch(TrimEnd(global.Command))
    {
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "LIST":
        // -----------------------------------------------------------------
        // Per PR# 86247  a new pickup indicator is added for AP. The following 
        // code is for implementing the functionality.
        //                                              
        // ----- Vithal Madhira (01-24-2000)
        // -----------------------------------------------------------------
        switch(AsChar(export.MultiAp.SelectChar))
        {
          case ' ':
            break;
          case '+':
            break;
          case 'S':
            ++local.Invalid.Count;
            export.ApSelected.Flag = "Y";
            ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

            break;
          default:
            var field = GetField(export.MultiAp, "selectChar");

            field.Error = true;

            ++local.Invalid.Count;
            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            break;
        }

        switch(local.Invalid.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

            break;
          case 1:
            break;
          default:
            break;
        }

        break;
      case "DISPLAY":
        if (AsChar(export.ClosedCaseIndicator.Flag) == 'Y')
        {
          UseSpCrinDisplayClosedCase();

          return;
        }

        if (ReadCase())
        {
          export.Case1.Number = entities.Case1.Number;

          if (ReadCaseAssignment())
          {
            if (ReadOfficeServiceProvider())
            {
              if (ReadOffice())
              {
                MoveOffice(entities.Office, export.Case2);
              }
              else
              {
                ExitState = "FN0000_OFFICE_NF";

                return;
              }

              if (ReadServiceProvider2())
              {
                export.CaseCoordinator.Assign(entities.ServiceProvider);
              }
              else
              {
                ExitState = "SERVICE_PROVIDER_NF";

                return;
              }
            }
            else
            {
              ExitState = "OFFICE_SERVICE_PROVIDER_NF";

              return;
            }
          }
          else
          {
            ExitState = "SP0000_FIRST_REVIEW_4_CLOSD_CASE";

            return;
          }

          // -----------------------
          // Redesign Note:
          // Per Jack R. after discussion of functionality in presently approved
          // design, while there may be more than 2 "alleged" ap's for a case,
          // this only happens in the case where paternity has not yet been
          // established.  Regardless, there is always a flow into the Paternity
          // screen, and a flow from there to COMP will list all alleged AP's
          // for the case.  Since this is the current design and it was
          // approved, no modification to allow for displaying multiple
          // potential ap's will be inserted at this time.
          // RVW 01/06/97.
          // ------------------------
          // ----------------------------------------------------------------------------------
          // PR#  86247  : The screen design changed according to this PR. Now a
          // new pickup indicator will be displayed adjacent to AP info. on the
          // screen and will be protected. If there are multiple AP's on the
          // case, the indicator will be unprotected and user can enter an 'S'
          // in the indicator field and will press PF4 to flow to COMP. On COMP
          // user can select an AP and will return to this screen with the
          // selected AP.
          //                      We must now create/modify/display the data 
          // based on the 'case_number' and 'cse_person_number'.
          //                                            
          // ----- Vithal (01-25-2000)
          // ---------------------------------------------------------------------------------------
          if (IsEmpty(export.ApSelected.Flag))
          {
            local.NoOfApOnCase.Count = 0;

            foreach(var item in ReadCsePersonCaseRole3())
            {
              ++local.NoOfApOnCase.Count;

              if (local.NoOfApOnCase.Count == 1)
              {
                local.Ap1.Number = entities.CsePerson.Number;
                local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
                UseSiReadCsePerson();

                if (!IsEmpty(local.AbendData.Type1))
                {
                  return;
                }

                MoveCsePersonsWorkSet2(local.CsePersonsWorkSet, export.Ap1);

                var field = GetField(export.MultiAp, "selectChar");

                field.Color = "";
                field.Protected = true;

                export.MoreApsMsg.Text30 = "";
              }
              else if (local.NoOfApOnCase.Count > 1)
              {
                export.MoreApsMsg.Text30 = "More AP's exist for this case.";

                var field = GetField(export.MultiAp, "selectChar");

                field.Color = "green";
                field.Protected = false;

                export.MultiAp.Flag = "Y";

                goto Test2;
              }
            }

            if (local.NoOfApOnCase.Count == 0)
            {
              export.MoreApsMsg.Text30 = "Case AP identity is unknown.";
              export.Ap1.Number = "";
              export.Ap1.FormattedName = "";

              var field = GetField(export.MultiAp, "selectChar");

              field.Protected = true;
            }
          }
          else
          {
            if (!IsEmpty(export.Ap1.Number))
            {
              MoveCsePersonsWorkSet2(export.Ap1, export.SelectedAp);
            }
            else
            {
              MoveCsePersonsWorkSet2(export.SelectedAp, export.Ap1);
            }

            if (AsChar(export.MultiAp.Flag) == 'Y')
            {
              export.MoreApsMsg.Text30 = "More AP's exist for this case.";

              var field = GetField(export.MultiAp, "selectChar");

              field.Color = "green";
              field.Protected = false;
            }
          }

Test2:

          if (ReadCsePerson1())
          {
            local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
            UseSiReadCsePerson();

            if (!IsEmpty(local.AbendData.Type1))
            {
              return;
            }

            MoveCsePersonsWorkSet2(local.CsePersonsWorkSet, export.Ar);
          }

          local.ProgramCount.Count = 1;

          export.Program.Index = 0;
          export.Program.Clear();

          foreach(var item in ReadProgram())
          {
            export.Program.Update.Program1.Code = entities.Program1.Code;

            switch(local.ProgramCount.Count)
            {
              case 1:
                export.Pgm1.Code = entities.Program1.Code;

                break;
              case 2:
                export.Pgm2.Code = entities.Program1.Code;

                break;
              case 3:
                export.Pgm3.Code = entities.Program1.Code;

                break;
              case 4:
                export.Pgm4.Code = entities.Program1.Code;

                break;
              case 5:
                export.Pgm5.Code = entities.Program1.Code;

                break;
              case 6:
                export.Pgm6.Code = entities.Program1.Code;

                break;
              case 7:
                export.Pgm7.Code = entities.Program1.Code;

                break;
              case 8:
                export.Pgm8.Code = entities.Program1.Code;
                export.Program.Next();

                goto ReadEach;
              default:
                export.Program.Next();

                goto ReadEach;
            }

            ++local.ProgramCount.Count;
            export.Program.Next();
          }

ReadEach:

          if (IsEmpty(export.Ap1.Number))
          {
            export.FunctionDesc.Text4 = "LOC";
          }
          else
          {
            UseSpCabGetCurrentCaseFunction();
          }

          // ************************************************
          // Set up for creation of Infrastructure records and Narrative
          // for active review.
          // ************************************************
          if (Equal(import.HiddenReviewType.ActionEntry, "P"))
          {
            // *******************************************
            // PERIODIC REVIEW TASK
            // *******************************************
            local.ReviewType.EventId = 5;
            local.ReviewType.EventType = "CASE";
            local.ReviewType.ReasonCode = "OSPRVWS";
            local.ReviewOther.EventId = 8;
            local.ReviewOther.EventType = "MODFN";
            local.ReviewOther.ReasonCode = "MODFNRVWDT";
          }
          else if (Equal(import.HiddenReviewType.ActionEntry, "M"))
          {
            // *******************************************
            // MODIFICATION REVIEW TASK
            // *******************************************
            local.ReviewType.EventId = 8;
            local.ReviewType.EventType = "MODFN";
            local.ReviewType.ReasonCode = "MODFNRVWDT";
            local.ReviewOther.EventId = 5;
            local.ReviewOther.EventType = "CASE";
            local.ReviewOther.ReasonCode = "OSPRVWS";
          }

          if (Equal(import.HiddenReviewType.ActionEntry, "R"))
          {
            // CQ28359 Only include person number in read if one is available
            local.InfrastructureFound.Flag = "N";

            if (!IsEmpty(export.Ap1.Number))
            {
              if (ReadInfrastructure5())
              {
                MoveInfrastructure2(entities.Infrastructure,
                  export.HiddenOrigAndPass);
                local.InfrastructureFound.Flag = "Y";
              }
            }
            else if (ReadInfrastructure9())
            {
              MoveInfrastructure2(entities.Infrastructure,
                export.HiddenOrigAndPass);
              local.InfrastructureFound.Flag = "Y";
            }

            if (AsChar(local.InfrastructureFound.Flag) == 'Y')
            {
              export.Export2LastReview.Date =
                Date(export.HiddenOrigAndPass.CreatedTimestamp);

              // mjr
              // --------------------------------------
              // 02/25/2002
              // PR139864 - Added 'last rvw resulted in mod' flag
              // ---------------------------------------------------
              export.LastRvwResultedInMod.Flag =
                entities.Infrastructure.DenormText12 ?? Spaces(1);

              if (ReadServiceProvider1())
              {
                MoveServiceProvider(entities.ServiceProvider, export.Export2);
              }
              else
              {
                export.ServiceProvider.UserId =
                  entities.Infrastructure.CreatedBy;

                var field1 = GetField(export.ServiceProvider, "firstName");

                field1.Error = true;

                var field2 = GetField(export.ServiceProvider, "lastName");

                field2.Error = true;

                ExitState = "SP0000_LAST_RVWING_SRV_PRVDR_NF";
              }
            }

            // CQ28359 Only include person number in read if one is available
            local.InfrastructureFound.Flag = "N";

            if (!IsEmpty(export.Ap1.Number))
            {
              if (ReadInfrastructure4())
              {
                export.ModOrPer.Assign(entities.Infrastructure);
                MoveInfrastructure2(entities.Infrastructure,
                  export.HiddenOrigAndPass);
                local.InfrastructureFound.Flag = "Y";
              }
            }
            else if (ReadInfrastructure8())
            {
              export.ModOrPer.Assign(entities.Infrastructure);
              MoveInfrastructure2(entities.Infrastructure,
                export.HiddenOrigAndPass);
              local.InfrastructureFound.Flag = "Y";
            }

            if (AsChar(local.InfrastructureFound.Flag) == 'Y')
            {
              export.LastReview.Date =
                Date(export.HiddenOrigAndPass.CreatedTimestamp);

              // *** Work request 000170
              // *** 08/08/2000 SWSRCHF
              // *** start
              local.Work2.Index = -1;

              foreach(var item in ReadNarrativeDetail11())
              {
                ++local.Work2.Index;
                local.Work2.CheckSize();

                if (local.Work2.Index == 0)
                {
                  local.Work2.Update.Work.NarrativeText =
                    entities.Existing.NarrativeText;
                }
                else
                {
                  local.Work2.Update.Work.NarrativeText =
                    Substring(entities.Existing.NarrativeText, 11, 58);
                }

                if (local.Work2.Index == 3)
                {
                  break;
                }
              }

              local.WorkGroupMed.Index = -1;

              foreach(var item in ReadNarrativeDetail1())
              {
                ++local.WorkGroupMed.Index;
                local.WorkGroupMed.CheckSize();

                if (local.WorkGroupMed.Index == 0)
                {
                  local.WorkGroupMed.Update.WorkGrpMed.NarrativeText =
                    entities.Existing.NarrativeText;
                }
                else
                {
                  local.WorkGroupMed.Update.WorkGrpMed.NarrativeText =
                    Substring(entities.Existing.NarrativeText, 12, 57);
                }

                if (local.WorkGroupMed.Index == 3)
                {
                  break;
                }
              }

              local.WorkGroupPat.Index = -1;

              foreach(var item in ReadNarrativeDetail2())
              {
                ++local.WorkGroupPat.Index;
                local.WorkGroupPat.CheckSize();

                if (local.WorkGroupPat.Index == 0)
                {
                  local.WorkGroupPat.Update.WorkGrpPat.NarrativeText =
                    entities.Existing.NarrativeText;
                }
                else
                {
                  local.WorkGroupPat.Update.WorkGrpPat.NarrativeText =
                    Substring(entities.Existing.NarrativeText, 14, 55);
                }

                if (local.WorkGroupPat.Index == 3)
                {
                  break;
                }
              }

              local.WorkGroupEst.Index = -1;

              foreach(var item in ReadNarrativeDetail7())
              {
                ++local.WorkGroupEst.Index;
                local.WorkGroupEst.CheckSize();

                if (local.WorkGroupEst.Index == 0)
                {
                  local.WorkGroupEst.Update.WorkGrpEst.NarrativeText =
                    entities.Existing.NarrativeText;
                }
                else
                {
                  local.WorkGroupEst.Update.WorkGrpEst.NarrativeText =
                    Substring(entities.Existing.NarrativeText, 18, 51);
                }

                if (local.WorkGroupEst.Index == 3)
                {
                  break;
                }
              }

              local.WorkGroupEnf.Index = -1;

              foreach(var item in ReadNarrativeDetail6())
              {
                ++local.WorkGroupEnf.Index;
                local.WorkGroupEnf.CheckSize();

                if (local.WorkGroupEnf.Index == 0)
                {
                  local.WorkGroupEnf.Update.WorkGrpEnf.NarrativeText =
                    entities.Existing.NarrativeText;
                }
                else
                {
                  local.WorkGroupEnf.Update.WorkGrpEnf.NarrativeText =
                    Substring(entities.Existing.NarrativeText, 16, 53);
                }

                if (local.WorkGroupEnf.Index == 3)
                {
                  break;
                }
              }

              local.Work1.Index = 0;
              local.Work1.CheckSize();

              for(local.Work2.Index = 0; local.Work2.Index < local.Work2.Count; ++
                local.Work2.Index)
              {
                if (!local.Work2.CheckSize())
                {
                  break;
                }

                local.Work1.Update.Work.Text =
                  TrimEnd(local.Work1.Item.Work.Text) + (
                    local.Work2.Item.Work.NarrativeText ?? "");
              }

              local.Work2.CheckIndex();

              local.Work1.Index = 1;
              local.Work1.CheckSize();

              for(local.WorkGroupMed.Index = 0; local.WorkGroupMed.Index < local
                .WorkGroupMed.Count; ++local.WorkGroupMed.Index)
              {
                if (!local.WorkGroupMed.CheckSize())
                {
                  break;
                }

                local.Work1.Update.Work.Text =
                  TrimEnd(local.Work1.Item.Work.Text) + (
                    local.WorkGroupMed.Item.WorkGrpMed.NarrativeText ?? "");
              }

              local.WorkGroupMed.CheckIndex();

              local.Work1.Index = 2;
              local.Work1.CheckSize();

              for(local.WorkGroupPat.Index = 0; local.WorkGroupPat.Index < local
                .WorkGroupPat.Count; ++local.WorkGroupPat.Index)
              {
                if (!local.WorkGroupPat.CheckSize())
                {
                  break;
                }

                local.Work1.Update.Work.Text =
                  TrimEnd(local.Work1.Item.Work.Text) + (
                    local.WorkGroupPat.Item.WorkGrpPat.NarrativeText ?? "");
              }

              local.WorkGroupPat.CheckIndex();

              local.Work1.Index = 3;
              local.Work1.CheckSize();

              for(local.WorkGroupEst.Index = 0; local.WorkGroupEst.Index < local
                .WorkGroupEst.Count; ++local.WorkGroupEst.Index)
              {
                if (!local.WorkGroupEst.CheckSize())
                {
                  break;
                }

                local.Work1.Update.Work.Text =
                  TrimEnd(local.Work1.Item.Work.Text) + (
                    local.WorkGroupEst.Item.WorkGrpEst.NarrativeText ?? "");
              }

              local.WorkGroupEst.CheckIndex();

              local.Work1.Index = 4;
              local.Work1.CheckSize();

              for(local.WorkGroupEnf.Index = 0; local.WorkGroupEnf.Index < local
                .WorkGroupEnf.Count; ++local.WorkGroupEnf.Index)
              {
                if (!local.WorkGroupEnf.CheckSize())
                {
                  break;
                }

                local.Work1.Update.Work.Text =
                  TrimEnd(local.Work1.Item.Work.Text) + (
                    local.WorkGroupEnf.Item.WorkGrpEnf.NarrativeText ?? "");
              }

              local.WorkGroupEnf.CheckIndex();

              if (!local.Work1.IsEmpty)
              {
                for(local.Work1.Index = 0; local.Work1.Index < local
                  .Work1.Count; ++local.Work1.Index)
                {
                  if (!local.Work1.CheckSize())
                  {
                    break;
                  }

                  export.PassReviewNote.Index = local.Work1.Index;
                  export.PassReviewNote.CheckSize();

                  export.PassReviewNote.Update.GexportHidden.Text =
                    local.Work1.Item.Work.Text;
                }

                local.Work1.CheckIndex();
                local.Work1.Index = -1;

                export.DisplayReviewNote.Index = 0;
                export.DisplayReviewNote.Clear();

                while(local.Work1.Index + 1 < local.Work1.Count)
                {
                  if (export.DisplayReviewNote.IsFull)
                  {
                    break;
                  }

                  ++local.Work1.Index;
                  local.Work1.CheckSize();

                  export.DisplayReviewNote.Update.G.Text =
                    local.Work1.Item.Work.Text;
                  export.DisplayReviewNote.Next();
                }
              }

              // *** end
              // *** 08/08/2000 SWSRCHF
              // *** Work request 000170
              if (ReadServiceProvider1())
              {
                export.ServiceProvider.Assign(entities.ServiceProvider);
              }
              else
              {
                export.ServiceProvider.UserId =
                  entities.Infrastructure.CreatedBy;

                var field1 = GetField(export.ServiceProvider, "firstName");

                field1.Error = true;

                var field2 = GetField(export.ServiceProvider, "lastName");

                field2.Error = true;

                ExitState = "SP0000_LAST_RVWING_SRV_PRVDR_NF";
              }
            }
          }
          else
          {
            // CQ28359 Only include person number in read if one is available
            local.InfrastructureFound.Flag = "N";

            if (!IsEmpty(export.Ap1.Number))
            {
              if (ReadInfrastructure3())
              {
                MoveInfrastructure2(entities.Infrastructure,
                  export.HiddenOrigAndPass);
                export.ModOrPer.Assign(entities.Infrastructure);
                local.InfrastructureFound.Flag = "Y";
              }
            }
            else if (ReadInfrastructure7())
            {
              MoveInfrastructure2(entities.Infrastructure,
                export.HiddenOrigAndPass);
              export.ModOrPer.Assign(entities.Infrastructure);
              local.InfrastructureFound.Flag = "Y";
            }

            if (AsChar(local.InfrastructureFound.Flag) == 'Y')
            {
              if (Equal(import.HiddenReviewType.ActionEntry, "P"))
              {
                export.LastReview.Date =
                  Date(export.HiddenOrigAndPass.CreatedTimestamp);
              }
              else
              {
                export.Export2LastReview.Date =
                  Date(export.HiddenOrigAndPass.CreatedTimestamp);

                // mjr
                // --------------------------------------
                // 02/25/2002
                // PR139864 - Added 'last rvw resulted in mod' flag
                // ---------------------------------------------------
                export.LastRvwResultedInMod.Flag =
                  entities.Infrastructure.DenormText12 ?? Spaces(1);
              }

              export.PassReviewNote.Index = -1;

              if (AsChar(export.Flag.Flag) == 'Y')
              {
                if (Equal(import.HiddenReviewType.ActionEntry, "P"))
                {
                  // *** Work request 000170
                  // *** 08/08/2000 SWSRCHF
                  // *** start
                  local.Work2.Index = -1;

                  foreach(var item in ReadNarrativeDetail3())
                  {
                    ++local.Work2.Index;
                    local.Work2.CheckSize();

                    local.Work2.Update.Work.NarrativeText =
                      entities.Existing.NarrativeText;

                    if (local.Work2.Index == 3)
                    {
                      break;
                    }
                  }

                  local.WorkGroupMed.Index = -1;

                  foreach(var item in ReadNarrativeDetail5())
                  {
                    ++local.WorkGroupMed.Index;
                    local.WorkGroupMed.CheckSize();

                    local.WorkGroupMed.Update.WorkGrpMed.NarrativeText =
                      entities.Existing.NarrativeText;

                    if (local.WorkGroupMed.Index == 3)
                    {
                      break;
                    }
                  }

                  local.WorkGroupPat.Index = -1;

                  foreach(var item in ReadNarrativeDetail4())
                  {
                    ++local.WorkGroupPat.Index;
                    local.WorkGroupPat.CheckSize();

                    local.WorkGroupPat.Update.WorkGrpPat.NarrativeText =
                      entities.Existing.NarrativeText;

                    if (local.WorkGroupPat.Index == 3)
                    {
                      break;
                    }
                  }

                  local.WorkGroupEst.Index = -1;

                  foreach(var item in ReadNarrativeDetail12())
                  {
                    ++local.WorkGroupEst.Index;
                    local.WorkGroupEst.CheckSize();

                    local.WorkGroupEst.Update.WorkGrpEst.NarrativeText =
                      entities.Existing.NarrativeText;

                    if (local.WorkGroupEst.Index == 3)
                    {
                      break;
                    }
                  }

                  local.WorkGroupEnf.Index = -1;

                  foreach(var item in ReadNarrativeDetail6())
                  {
                    ++local.WorkGroupEnf.Index;
                    local.WorkGroupEnf.CheckSize();

                    local.WorkGroupEnf.Update.WorkGrpEnf.NarrativeText =
                      entities.Existing.NarrativeText;

                    if (local.WorkGroupEnf.Index == 3)
                    {
                      break;
                    }
                  }

                  if (!local.Work2.IsEmpty)
                  {
                    local.Work1.Index = 0;
                    local.Work1.CheckSize();

                    for(local.Work2.Index = 0; local.Work2.Index < local
                      .Work2.Count; ++local.Work2.Index)
                    {
                      if (!local.Work2.CheckSize())
                      {
                        break;
                      }

                      local.Work1.Update.Work.Text =
                        TrimEnd(local.Work1.Item.Work.Text) + Substring
                        (local.Work2.Item.Work.NarrativeText, 68, 11, 58);
                    }

                    local.Work2.CheckIndex();
                  }

                  if (!local.WorkGroupMed.IsEmpty)
                  {
                    local.Work1.Index = 1;
                    local.Work1.CheckSize();

                    for(local.WorkGroupMed.Index = 0; local
                      .WorkGroupMed.Index < local.WorkGroupMed.Count; ++
                      local.WorkGroupMed.Index)
                    {
                      if (!local.WorkGroupMed.CheckSize())
                      {
                        break;
                      }

                      local.Work1.Update.Work.Text =
                        TrimEnd(local.Work1.Item.Work.Text) + Substring
                        (local.WorkGroupMed.Item.WorkGrpMed.NarrativeText, 68,
                        12, 57);
                    }

                    local.WorkGroupMed.CheckIndex();
                  }

                  if (!local.WorkGroupPat.IsEmpty)
                  {
                    local.Work1.Index = 2;
                    local.Work1.CheckSize();

                    for(local.WorkGroupPat.Index = 0; local
                      .WorkGroupPat.Index < local.WorkGroupPat.Count; ++
                      local.WorkGroupPat.Index)
                    {
                      if (!local.WorkGroupPat.CheckSize())
                      {
                        break;
                      }

                      local.Work1.Update.Work.Text =
                        TrimEnd(local.Work1.Item.Work.Text) + Substring
                        (local.WorkGroupPat.Item.WorkGrpPat.NarrativeText, 68,
                        14, 55);
                    }

                    local.WorkGroupPat.CheckIndex();
                  }

                  if (!local.WorkGroupEst.IsEmpty)
                  {
                    local.Work1.Index = 3;
                    local.Work1.CheckSize();

                    for(local.WorkGroupEst.Index = 0; local
                      .WorkGroupEst.Index < local.WorkGroupEst.Count; ++
                      local.WorkGroupEst.Index)
                    {
                      if (!local.WorkGroupEst.CheckSize())
                      {
                        break;
                      }

                      local.Work1.Update.Work.Text =
                        TrimEnd(local.Work1.Item.Work.Text) + Substring
                        (local.WorkGroupEst.Item.WorkGrpEst.NarrativeText, 68,
                        18, 51);
                    }

                    local.WorkGroupEst.CheckIndex();
                  }

                  if (!local.WorkGroupEnf.IsEmpty)
                  {
                    local.Work1.Index = 4;
                    local.Work1.CheckSize();

                    for(local.WorkGroupEnf.Index = 0; local
                      .WorkGroupEnf.Index < local.WorkGroupEnf.Count; ++
                      local.WorkGroupEnf.Index)
                    {
                      if (!local.WorkGroupEnf.CheckSize())
                      {
                        break;
                      }

                      local.Work1.Update.Work.Text =
                        TrimEnd(local.Work1.Item.Work.Text) + Substring
                        (local.WorkGroupEnf.Item.WorkGrpEnf.NarrativeText, 68,
                        16, 53);
                    }

                    local.WorkGroupEnf.CheckIndex();
                  }

                  if (!local.Work1.IsEmpty)
                  {
                    for(local.Work1.Index = 0; local.Work1.Index < Local
                      .WorkGroup1.Capacity; ++local.Work1.Index)
                    {
                      if (!local.Work1.CheckSize())
                      {
                        break;
                      }

                      export.PassReviewNote.Index = local.Work1.Index;
                      export.PassReviewNote.CheckSize();

                      export.PassReviewNote.Update.GexportHidden.Text =
                        local.Work1.Item.Work.Text;
                    }

                    local.Work1.CheckIndex();
                    local.Work1.Index = -1;

                    export.DisplayReviewNote.Index = 0;
                    export.DisplayReviewNote.Clear();

                    while(local.Work1.Index + 1 < Local.WorkGroup1.Capacity)
                    {
                      if (export.DisplayReviewNote.IsFull)
                      {
                        break;
                      }

                      ++local.Work1.Index;
                      local.Work1.CheckSize();

                      if (local.Work1.Index == 0)
                      {
                        export.DisplayReviewNote.Update.G.Text =
                          "LOCATE -- " + local.Work1.Item.Work.Text;
                      }

                      if (local.Work1.Index == 1)
                      {
                        export.DisplayReviewNote.Update.G.Text =
                          "MEDICAL -- " + local.Work1.Item.Work.Text;
                      }

                      if (local.Work1.Index == 2)
                      {
                        export.DisplayReviewNote.Update.G.Text =
                          "PATERNITY -- " + local.Work1.Item.Work.Text;
                      }

                      if (local.Work1.Index == 3)
                      {
                        export.DisplayReviewNote.Update.G.Text =
                          "ESTABLISHMENT -- " + local.Work1.Item.Work.Text;
                      }

                      if (local.Work1.Index == 4)
                      {
                        export.DisplayReviewNote.Update.G.Text =
                          "ENFORCEMENT -- " + local.Work1.Item.Work.Text;
                      }

                      export.DisplayReviewNote.Next();
                    }
                  }

                  // *** end
                  // *** 08/08/2000 SWSRCHF
                  // *** Work request 000170
                }
                else
                {
                  // *** Work request 000170
                  // *** 08/08/2000 SWSRCHF
                  // *** start
                  local.Work2.Index = -1;

                  foreach(var item in ReadNarrativeDetail13())
                  {
                    ++local.Work2.Index;
                    local.Work2.CheckSize();

                    local.Work2.Update.Work.NarrativeText =
                      entities.Existing.NarrativeText;

                    if (local.Work2.Index == 4)
                    {
                      break;
                    }
                  }

                  local.WorkGroupMed.Index = -1;

                  foreach(var item in ReadNarrativeDetail9())
                  {
                    ++local.WorkGroupMed.Index;
                    local.WorkGroupMed.CheckSize();

                    local.WorkGroupMed.Update.WorkGrpMed.NarrativeText =
                      entities.Existing.NarrativeText;

                    if (local.WorkGroupMed.Index == 4)
                    {
                      break;
                    }
                  }

                  local.WorkGroupPat.Index = -1;

                  foreach(var item in ReadNarrativeDetail10())
                  {
                    ++local.WorkGroupPat.Index;
                    local.WorkGroupPat.CheckSize();

                    local.WorkGroupPat.Update.WorkGrpPat.NarrativeText =
                      entities.Existing.NarrativeText;

                    if (local.WorkGroupPat.Index == 4)
                    {
                      break;
                    }
                  }

                  local.WorkGroupEst.Index = -1;

                  foreach(var item in ReadNarrativeDetail14())
                  {
                    ++local.WorkGroupEst.Index;
                    local.WorkGroupEst.CheckSize();

                    local.WorkGroupEst.Update.WorkGrpEst.NarrativeText =
                      entities.Existing.NarrativeText;

                    if (local.WorkGroupEst.Index == 4)
                    {
                      break;
                    }
                  }

                  local.WorkGroupEnf.Index = -1;

                  foreach(var item in ReadNarrativeDetail8())
                  {
                    ++local.WorkGroupEnf.Index;
                    local.WorkGroupEnf.CheckSize();

                    local.WorkGroupEnf.Update.WorkGrpEnf.NarrativeText =
                      entities.Existing.NarrativeText;

                    if (local.WorkGroupEnf.Index == 4)
                    {
                      break;
                    }
                  }

                  if (!local.Work2.IsEmpty)
                  {
                    local.Work1.Index = 0;
                    local.Work1.CheckSize();

                    for(local.Work2.Index = 0; local.Work2.Index < local
                      .Work2.Count; ++local.Work2.Index)
                    {
                      if (!local.Work2.CheckSize())
                      {
                        break;
                      }

                      local.Work1.Update.Work.Text =
                        TrimEnd(local.Work1.Item.Work.Text) + Substring
                        (local.Work2.Item.Work.NarrativeText, 68, 22, 47);
                    }

                    local.Work2.CheckIndex();
                  }

                  if (!local.WorkGroupMed.IsEmpty)
                  {
                    local.Work1.Index = 1;
                    local.Work1.CheckSize();

                    for(local.WorkGroupMed.Index = 0; local
                      .WorkGroupMed.Index < local.WorkGroupMed.Count; ++
                      local.WorkGroupMed.Index)
                    {
                      if (!local.WorkGroupMed.CheckSize())
                      {
                        break;
                      }

                      local.Work1.Update.Work.Text =
                        TrimEnd(local.Work1.Item.Work.Text) + Substring
                        (local.WorkGroupMed.Item.WorkGrpMed.NarrativeText, 68,
                        23, 46);
                    }

                    local.WorkGroupMed.CheckIndex();
                  }

                  if (!local.WorkGroupPat.IsEmpty)
                  {
                    local.Work1.Index = 2;
                    local.Work1.CheckSize();

                    for(local.WorkGroupPat.Index = 0; local
                      .WorkGroupPat.Index < local.WorkGroupPat.Count; ++
                      local.WorkGroupPat.Index)
                    {
                      if (!local.WorkGroupPat.CheckSize())
                      {
                        break;
                      }

                      local.Work1.Update.Work.Text =
                        TrimEnd(local.Work1.Item.Work.Text) + Substring
                        (local.WorkGroupPat.Item.WorkGrpPat.NarrativeText, 68,
                        25, 44);
                    }

                    local.WorkGroupPat.CheckIndex();
                  }

                  if (!local.WorkGroupEst.IsEmpty)
                  {
                    local.Work1.Index = 3;
                    local.Work1.CheckSize();

                    for(local.WorkGroupEst.Index = 0; local
                      .WorkGroupEst.Index < local.WorkGroupEst.Count; ++
                      local.WorkGroupEst.Index)
                    {
                      if (!local.WorkGroupEst.CheckSize())
                      {
                        break;
                      }

                      local.Work1.Update.Work.Text =
                        TrimEnd(local.Work1.Item.Work.Text) + Substring
                        (local.WorkGroupEst.Item.WorkGrpEst.NarrativeText, 68,
                        29, 40);
                    }

                    local.WorkGroupEst.CheckIndex();
                  }

                  if (!local.WorkGroupEnf.IsEmpty)
                  {
                    local.Work1.Index = 4;
                    local.Work1.CheckSize();

                    for(local.WorkGroupEnf.Index = 0; local
                      .WorkGroupEnf.Index < local.WorkGroupEnf.Count; ++
                      local.WorkGroupEnf.Index)
                    {
                      if (!local.WorkGroupEnf.CheckSize())
                      {
                        break;
                      }

                      local.Work1.Update.Work.Text =
                        TrimEnd(local.Work1.Item.Work.Text) + Substring
                        (local.WorkGroupEnf.Item.WorkGrpEnf.NarrativeText, 68,
                        27, 42);
                    }

                    local.WorkGroupEnf.CheckIndex();
                  }

                  if (!local.Work1.IsEmpty)
                  {
                    for(local.Work1.Index = 0; local.Work1.Index < Local
                      .WorkGroup1.Capacity; ++local.Work1.Index)
                    {
                      if (!local.Work1.CheckSize())
                      {
                        break;
                      }

                      export.PassReviewNote.Index = local.Work1.Index;
                      export.PassReviewNote.CheckSize();

                      export.PassReviewNote.Update.GexportHidden.Text =
                        local.Work1.Item.Work.Text;
                    }

                    local.Work1.CheckIndex();
                    local.Work1.Index = -1;

                    export.DisplayReviewNote.Index = 0;
                    export.DisplayReviewNote.Clear();

                    while(local.Work1.Index + 1 < Local.WorkGroup1.Capacity)
                    {
                      if (export.DisplayReviewNote.IsFull)
                      {
                        break;
                      }

                      ++local.Work1.Index;
                      local.Work1.CheckSize();

                      if (local.Work1.Index == 0)
                      {
                        export.DisplayReviewNote.Update.G.Text =
                          "MOD REVIEW LOCATE -- " + local.Work1.Item.Work.Text;
                      }

                      if (local.Work1.Index == 1)
                      {
                        export.DisplayReviewNote.Update.G.Text =
                          "MOD REVIEW MEDICAL -- " + local
                          .Work1.Item.Work.Text;
                      }

                      if (local.Work1.Index == 2)
                      {
                        export.DisplayReviewNote.Update.G.Text =
                          "MOD REVIEW PATERNITY -- " + local
                          .Work1.Item.Work.Text;
                      }

                      if (local.Work1.Index == 3)
                      {
                        export.DisplayReviewNote.Update.G.Text =
                          "MOD REVIEW ESTABLISHMENT -- " + local
                          .Work1.Item.Work.Text;
                      }

                      if (local.Work1.Index == 4)
                      {
                        export.DisplayReviewNote.Update.G.Text =
                          "MOD REVIEW ENFORCEMENT -- " + local
                          .Work1.Item.Work.Text;
                      }

                      export.DisplayReviewNote.Next();
                    }
                  }

                  // *** end
                  // *** 08/08/2000 SWSRCHF
                  // *** Work request 000170
                }
              }
              else
              {
                export.DisplayReviewNote.Index = 0;
                export.DisplayReviewNote.Clear();

                for(import.DisplayReviewNote.Index = 0; import
                  .DisplayReviewNote.Index < import.DisplayReviewNote.Count; ++
                  import.DisplayReviewNote.Index)
                {
                  if (export.DisplayReviewNote.IsFull)
                  {
                    break;
                  }

                  export.DisplayReviewNote.Update.G.Text =
                    import.DisplayReviewNote.Item.G.Text;
                  export.DisplayReviewNote.Next();
                }
              }

              if (ReadServiceProvider1())
              {
                if (Equal(import.HiddenReviewType.ActionEntry, "P"))
                {
                  export.ServiceProvider.Assign(entities.ServiceProvider);
                }
                else
                {
                  MoveServiceProvider(entities.ServiceProvider, export.Export2);
                }
              }
              else
              {
                export.ServiceProvider.UserId =
                  entities.Infrastructure.CreatedBy;

                var field1 = GetField(export.ServiceProvider, "firstName");

                field1.Error = true;

                var field2 = GetField(export.ServiceProvider, "lastName");

                field2.Error = true;

                ExitState = "SP0000_LAST_RVWING_SRV_PRVDR_NF";
              }
            }

            // CQ28359 Only include person number in read if one is available
            local.InfrastructureFound.Flag = "N";

            if (!IsEmpty(export.Ap1.Number))
            {
              if (ReadInfrastructure2())
              {
                MoveInfrastructure2(entities.Infrastructure,
                  export.HiddenOrigAndPass);
                local.InfrastructureFound.Flag = "Y";
              }
            }
            else if (ReadInfrastructure6())
            {
              MoveInfrastructure2(entities.Infrastructure,
                export.HiddenOrigAndPass);
              local.InfrastructureFound.Flag = "Y";
            }

            if (AsChar(local.InfrastructureFound.Flag) == 'Y')
            {
              if (Equal(import.HiddenReviewType.ActionEntry, "P"))
              {
                export.Export2LastReview.Date =
                  Date(export.HiddenOrigAndPass.CreatedTimestamp);

                // mjr
                // --------------------------------------
                // 02/25/2002
                // PR139864 - Added 'last rvw resulted in mod' flag
                // ---------------------------------------------------
                export.LastRvwResultedInMod.Flag =
                  entities.Infrastructure.DenormText12 ?? Spaces(1);
              }
              else
              {
                export.LastReview.Date =
                  Date(export.HiddenOrigAndPass.CreatedTimestamp);
              }

              if (ReadServiceProvider1())
              {
                if (Equal(import.HiddenReviewType.ActionEntry, "P"))
                {
                  MoveServiceProvider(entities.ServiceProvider, export.Export2);
                }
                else
                {
                  export.ServiceProvider.Assign(entities.ServiceProvider);
                }
              }
            }
          }
        }
        else
        {
          var field = GetField(export.Case1, "number");

          field.Error = true;

          ExitState = "CASE_NF";

          return;
        }

        if (export.DisplayReviewNote.IsEmpty)
        {
          if (AsChar(export.ClosedCaseIndicator.Flag) == 'Y')
          {
            ExitState = "SP0000_FIRST_REVIEW_4_CLOSD_CASE";
          }
          else
          {
            ExitState = "SP0000_FIRST_REVIEW_4_CASE";
          }
        }
        else if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (AsChar(export.CommandPassedFromEnfor.Flag) == 'A')
          {
            export.CommandPassedFromEnfor.Flag = "";

            if (AsChar(export.ClosedCaseIndicator.Flag) == 'Y')
            {
              ExitState = "SP0000_CLOSED_CASE_RVW_COMPLT";
            }
            else
            {
              ExitState = "SP0000_CASE_REVIEW_COMPLETED";
            }
          }
          else if (AsChar(export.CommandPassedFromEnfor.Flag) == 'D')
          {
            export.CommandPassedFromEnfor.Flag = "";

            if (AsChar(export.ClosedCaseIndicator.Flag) == 'Y')
            {
              ExitState = "DISPLAY_OK_FOR_CLOSED_CASE";
            }
            else
            {
              ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
            }
          }

          if (AsChar(local.CaseReviewCompleted.Flag) == 'Y')
          {
            ExitState = "SP0000_CASE_REVIEW_COMPLETED";

            return;
          }

          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }

        break;
      case "ENTER":
        export.Flag.Flag = "";
        ExitState = "ECO_LNK_TO_CR_LOCATE";

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      case "NEXT":
        ExitState = "SP0000_SCROLLED_BEYOND_1ST_PG";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }
  }

  private static void MoveCommon(Common source, Common target)
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
    target.FormattedName = source.FormattedName;
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
  }

  private static void MoveDisplayReviewNote(SpCrinDisplayClosedCase.Export.
    DisplayReviewNoteGroup source, Export.DisplayReviewNoteGroup target)
  {
    target.G.Text = source.G.Text;
  }

  private static void MoveDocument(Document source, Document target)
  {
    target.Name = source.Name;
    target.Type1 = source.Type1;
  }

  private static void MoveInfrastructure1(Infrastructure source,
    Infrastructure target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.EventType = source.EventType;
    target.EventDetailName = source.EventDetailName;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormText12 = source.DenormText12;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.CsenetInOutCode = source.CsenetInOutCode;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.UserId = source.UserId;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private static void MoveInfrastructure2(Infrastructure source,
    Infrastructure target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveOffice(Office source, Office target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.Name = source.Name;
  }

  private static void MovePassReviewNote(SpCrinDisplayClosedCase.Export.
    PassReviewNoteGroup source, Export.PassReviewNoteGroup target)
  {
    System.Diagnostics.Trace.TraceWarning(
      "INFO: source and target of the move do not fit perfectly.");
    target.GexportHidden.Text = source.GexportHidden.Text;
  }

  private static void MoveProgram(SpCrinDisplayClosedCase.Export.
    ProgramGroup source, Export.ProgramGroup target)
  {
    target.Program1.Code = source.Program1.Code;
  }

  private static void MoveServiceProvider(ServiceProvider source,
    ServiceProvider target)
  {
    target.LastName = source.LastName;
    target.FirstName = source.FirstName;
  }

  private static void MoveSpDocKey(SpDocKey source, SpDocKey target)
  {
    target.KeyAp = source.KeyAp;
    target.KeyCase = source.KeyCase;
  }

  private void UseCabConvertDate2String()
  {
    var useImport = new CabConvertDate2String.Import();
    var useExport = new CabConvertDate2String.Export();

    useImport.DateWorkArea.Date = local.GetDateFromTimestamp.Date;

    Call(CabConvertDate2String.Execute, useImport, useExport);

    local.Date.Text8 = useExport.TextWorkArea.Text8;
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

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet1(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
      
    local.AbendData.Assign(useExport.AbendData);
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    MoveInfrastructure1(local.ReviewType, useImport.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    MoveInfrastructure1(useExport.Infrastructure, local.ReviewType);
  }

  private void UseSpCabDetermineInterstateDoc()
  {
    var useImport = new SpCabDetermineInterstateDoc.Import();
    var useExport = new SpCabDetermineInterstateDoc.Export();

    MoveDocument(local.Document, useImport.Document);
    MoveSpDocKey(local.SpDocKey, useImport.SpDocKey);

    Call(SpCabDetermineInterstateDoc.Execute, useImport, useExport);
  }

  private void UseSpCabGetCurrentCaseFunction()
  {
    var useImport = new SpCabGetCurrentCaseFunction.Import();
    var useExport = new SpCabGetCurrentCaseFunction.Export();

    useImport.Case1.Number = export.Case1.Number;

    Call(SpCabGetCurrentCaseFunction.Execute, useImport, useExport);

    export.FunctionDesc.Text4 = useExport.FunctionDesc.Text4;
  }

  private void UseSpCreateDocumentInfrastruct()
  {
    var useImport = new SpCreateDocumentInfrastruct.Import();
    var useExport = new SpCreateDocumentInfrastruct.Export();

    MoveDocument(local.Document, useImport.Document);
    MoveSpDocKey(local.SpDocKey, useImport.SpDocKey);

    Call(SpCreateDocumentInfrastruct.Execute, useImport, useExport);
  }

  private void UseSpCrinDisplayClosedCase()
  {
    var useImport = new SpCrinDisplayClosedCase.Import();
    var useExport = new SpCrinDisplayClosedCase.Export();

    useImport.Crme.Number = import.Case1.Number;
    useImport.HiddenReviewType.ActionEntry =
      export.HiddenReviewType.ActionEntry;
    useImport.CommandPassedFromEnfor.Flag = export.CommandPassedFromEnfor.Flag;

    Call(SpCrinDisplayClosedCase.Execute, useImport, useExport);

    export.LastRvwResultedInMod.Flag = useExport.LastRvwResultedInMod.Flag;
    export.Export2LastReview.Date = useExport.LastReview1.Date;
    MoveServiceProvider(useExport.Export1, export.Export2);
    export.HiddenOrigAndPass.SystemGeneratedIdentifier =
      useExport.HiddenOrigAndPass.SystemGeneratedIdentifier;
    export.Case1.Number = useExport.Case2.Number;
    useExport.PassReviewNote.CopyTo(export.PassReviewNote, MovePassReviewNote);
    export.CaseCoordinator.Assign(useExport.CaseCoordinator);
    MoveOffice(useExport.Case1, export.Case2);
    MoveCsePersonsWorkSet2(useExport.Ar, export.Ar);
    MoveCsePersonsWorkSet2(useExport.Ap1, export.Ap1);
    export.ServiceProvider.Assign(useExport.ServiceProvider);
    export.Pgm1.Code = useExport.Pgm1.Code;
    export.Pgm2.Code = useExport.Pgm2.Code;
    export.Pgm3.Code = useExport.Pgm3.Code;
    export.Pgm4.Code = useExport.Pgm4.Code;
    export.Pgm5.Code = useExport.Pgm5.Code;
    export.Pgm6.Code = useExport.Pgm6.Code;
    export.Pgm7.Code = useExport.Pgm7.Code;
    export.Pgm8.Code = useExport.Pgm8.Code;
    useExport.Program.CopyTo(export.Program, MoveProgram);
    useExport.DisplayReviewNote.CopyTo(
      export.DisplayReviewNote, MoveDisplayReviewNote);
    export.FunctionDesc.Text4 = useExport.FunctionDesc.Text4;
    MoveDateWorkArea(useExport.LastReview, export.LastReview);
    export.MoreApsMsg.Text30 = useExport.MoreApsMsg.Text30;
  }

  private void CreateNarrativeDetail()
  {
    var infrastructureId = entities.Infrastructure.SystemGeneratedIdentifier;
    var createdTimestamp = local.GetDateFromTimestamp.Timestamp;
    var createdBy = global.UserId;
    var caseNumber = export.Case1.Number;
    var narrativeText = local.Work.NarrativeText ?? "";
    var lineNumber = local.Work.LineNumber;

    entities.New1.Populated = false;
    Update("CreateNarrativeDetail",
      (db, command) =>
      {
        db.SetInt32(command, "infrastructureId", infrastructureId);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "createdBy", createdBy);
        db.SetNullableString(command, "caseNumber", caseNumber);
        db.SetNullableString(command, "narrativeText", narrativeText);
        db.SetInt32(command, "lineNumber", lineNumber);
      });

    entities.New1.InfrastructureId = infrastructureId;
    entities.New1.CreatedTimestamp = createdTimestamp;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CaseNumber = caseNumber;
    entities.New1.NarrativeText = narrativeText;
    entities.New1.LineNumber = lineNumber;
    entities.New1.Populated = true;
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseAssignment()
  {
    entities.CaseAssignment.Populated = false;

    return Read("ReadCaseAssignment",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseAssignment.ReasonCode = db.GetString(reader, 0);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 1);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 4);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 5);
        entities.CaseAssignment.OspCode = db.GetString(reader, 6);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 7);
        entities.CaseAssignment.CasNo = db.GetString(reader, 8);
        entities.CaseAssignment.Populated = true;
      });
  }

  private bool ReadCsePerson1()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePerson2()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", local.ReviewType.CaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePersonCaseRole1()
  {
    entities.CsePerson.Populated = false;
    entities.CaseRole.Populated = false;

    return Read("ReadCsePersonCaseRole1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", export.Case1.Number);
        db.SetString(command, "numb", import.Ap1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CaseRole.CasNumber = db.GetString(reader, 2);
        entities.CaseRole.Type1 = db.GetString(reader, 3);
        entities.CaseRole.Identifier = db.GetInt32(reader, 4);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 6);
        entities.CaseRole.CreatedTimestamp = db.GetDateTime(reader, 7);
        entities.CsePerson.Populated = true;
        entities.CaseRole.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private bool ReadCsePersonCaseRole2()
  {
    entities.CsePerson.Populated = false;
    entities.CaseRole.Populated = false;

    return Read("ReadCsePersonCaseRole2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CaseRole.CasNumber = db.GetString(reader, 2);
        entities.CaseRole.Type1 = db.GetString(reader, 3);
        entities.CaseRole.Identifier = db.GetInt32(reader, 4);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 6);
        entities.CaseRole.CreatedTimestamp = db.GetDateTime(reader, 7);
        entities.CsePerson.Populated = true;
        entities.CaseRole.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private IEnumerable<bool> ReadCsePersonCaseRole3()
  {
    entities.CsePerson.Populated = false;
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCsePersonCaseRole3",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CaseRole.CasNumber = db.GetString(reader, 2);
        entities.CaseRole.Type1 = db.GetString(reader, 3);
        entities.CaseRole.Identifier = db.GetInt32(reader, 4);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 6);
        entities.CaseRole.CreatedTimestamp = db.GetDateTime(reader, 7);
        entities.CsePerson.Populated = true;
        entities.CaseRole.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);

        return true;
      });
  }

  private bool ReadInfrastructure1()
  {
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure1",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          local.ReviewType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.SituationNumber = db.GetInt32(reader, 1);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 2);
        entities.Infrastructure.EventId = db.GetInt32(reader, 3);
        entities.Infrastructure.EventType = db.GetString(reader, 4);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 5);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 6);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 7);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 8);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 9);
        entities.Infrastructure.DenormDate = db.GetNullableDate(reader, 10);
        entities.Infrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.Infrastructure.InitiatingStateCode = db.GetString(reader, 12);
        entities.Infrastructure.CsenetInOutCode = db.GetString(reader, 13);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 14);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 15);
        entities.Infrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 16);
        entities.Infrastructure.UserId = db.GetString(reader, 17);
        entities.Infrastructure.CreatedBy = db.GetString(reader, 18);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.Infrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 20);
        entities.Infrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.Infrastructure.Function = db.GetNullableString(reader, 23);
        entities.Infrastructure.CaseUnitState =
          db.GetNullableString(reader, 24);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 25);
        entities.Infrastructure.Populated = true;
      });
  }

  private bool ReadInfrastructure2()
  {
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure2",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", export.Case1.Number);
        db.SetNullableString(command, "csePersonNum", export.Ap1.Number);
        db.SetInt32(command, "eventId", local.ReviewOther.EventId);
        db.SetString(command, "eventType", local.ReviewOther.EventType);
        db.SetString(command, "reasonCode", local.ReviewOther.ReasonCode);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.SituationNumber = db.GetInt32(reader, 1);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 2);
        entities.Infrastructure.EventId = db.GetInt32(reader, 3);
        entities.Infrastructure.EventType = db.GetString(reader, 4);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 5);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 6);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 7);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 8);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 9);
        entities.Infrastructure.DenormDate = db.GetNullableDate(reader, 10);
        entities.Infrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.Infrastructure.InitiatingStateCode = db.GetString(reader, 12);
        entities.Infrastructure.CsenetInOutCode = db.GetString(reader, 13);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 14);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 15);
        entities.Infrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 16);
        entities.Infrastructure.UserId = db.GetString(reader, 17);
        entities.Infrastructure.CreatedBy = db.GetString(reader, 18);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.Infrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 20);
        entities.Infrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.Infrastructure.Function = db.GetNullableString(reader, 23);
        entities.Infrastructure.CaseUnitState =
          db.GetNullableString(reader, 24);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 25);
        entities.Infrastructure.Populated = true;
      });
  }

  private bool ReadInfrastructure3()
  {
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure3",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", export.Case1.Number);
        db.SetNullableString(command, "csePersonNum", export.Ap1.Number);
        db.SetInt32(command, "eventId", local.ReviewType.EventId);
        db.SetString(command, "eventType", local.ReviewType.EventType);
        db.SetString(command, "reasonCode", local.ReviewType.ReasonCode);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.SituationNumber = db.GetInt32(reader, 1);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 2);
        entities.Infrastructure.EventId = db.GetInt32(reader, 3);
        entities.Infrastructure.EventType = db.GetString(reader, 4);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 5);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 6);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 7);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 8);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 9);
        entities.Infrastructure.DenormDate = db.GetNullableDate(reader, 10);
        entities.Infrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.Infrastructure.InitiatingStateCode = db.GetString(reader, 12);
        entities.Infrastructure.CsenetInOutCode = db.GetString(reader, 13);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 14);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 15);
        entities.Infrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 16);
        entities.Infrastructure.UserId = db.GetString(reader, 17);
        entities.Infrastructure.CreatedBy = db.GetString(reader, 18);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.Infrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 20);
        entities.Infrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.Infrastructure.Function = db.GetNullableString(reader, 23);
        entities.Infrastructure.CaseUnitState =
          db.GetNullableString(reader, 24);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 25);
        entities.Infrastructure.Populated = true;
      });
  }

  private bool ReadInfrastructure4()
  {
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure4",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", export.Case1.Number);
        db.SetNullableString(command, "csePersonNum", export.Ap1.Number);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.SituationNumber = db.GetInt32(reader, 1);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 2);
        entities.Infrastructure.EventId = db.GetInt32(reader, 3);
        entities.Infrastructure.EventType = db.GetString(reader, 4);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 5);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 6);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 7);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 8);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 9);
        entities.Infrastructure.DenormDate = db.GetNullableDate(reader, 10);
        entities.Infrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.Infrastructure.InitiatingStateCode = db.GetString(reader, 12);
        entities.Infrastructure.CsenetInOutCode = db.GetString(reader, 13);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 14);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 15);
        entities.Infrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 16);
        entities.Infrastructure.UserId = db.GetString(reader, 17);
        entities.Infrastructure.CreatedBy = db.GetString(reader, 18);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.Infrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 20);
        entities.Infrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.Infrastructure.Function = db.GetNullableString(reader, 23);
        entities.Infrastructure.CaseUnitState =
          db.GetNullableString(reader, 24);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 25);
        entities.Infrastructure.Populated = true;
      });
  }

  private bool ReadInfrastructure5()
  {
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure5",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", export.Case1.Number);
        db.SetNullableString(command, "csePersonNum", export.Ap1.Number);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.SituationNumber = db.GetInt32(reader, 1);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 2);
        entities.Infrastructure.EventId = db.GetInt32(reader, 3);
        entities.Infrastructure.EventType = db.GetString(reader, 4);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 5);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 6);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 7);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 8);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 9);
        entities.Infrastructure.DenormDate = db.GetNullableDate(reader, 10);
        entities.Infrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.Infrastructure.InitiatingStateCode = db.GetString(reader, 12);
        entities.Infrastructure.CsenetInOutCode = db.GetString(reader, 13);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 14);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 15);
        entities.Infrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 16);
        entities.Infrastructure.UserId = db.GetString(reader, 17);
        entities.Infrastructure.CreatedBy = db.GetString(reader, 18);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.Infrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 20);
        entities.Infrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.Infrastructure.Function = db.GetNullableString(reader, 23);
        entities.Infrastructure.CaseUnitState =
          db.GetNullableString(reader, 24);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 25);
        entities.Infrastructure.Populated = true;
      });
  }

  private bool ReadInfrastructure6()
  {
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure6",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", export.Case1.Number);
        db.SetInt32(command, "eventId", local.ReviewOther.EventId);
        db.SetString(command, "eventType", local.ReviewOther.EventType);
        db.SetString(command, "reasonCode", local.ReviewOther.ReasonCode);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.SituationNumber = db.GetInt32(reader, 1);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 2);
        entities.Infrastructure.EventId = db.GetInt32(reader, 3);
        entities.Infrastructure.EventType = db.GetString(reader, 4);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 5);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 6);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 7);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 8);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 9);
        entities.Infrastructure.DenormDate = db.GetNullableDate(reader, 10);
        entities.Infrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.Infrastructure.InitiatingStateCode = db.GetString(reader, 12);
        entities.Infrastructure.CsenetInOutCode = db.GetString(reader, 13);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 14);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 15);
        entities.Infrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 16);
        entities.Infrastructure.UserId = db.GetString(reader, 17);
        entities.Infrastructure.CreatedBy = db.GetString(reader, 18);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.Infrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 20);
        entities.Infrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.Infrastructure.Function = db.GetNullableString(reader, 23);
        entities.Infrastructure.CaseUnitState =
          db.GetNullableString(reader, 24);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 25);
        entities.Infrastructure.Populated = true;
      });
  }

  private bool ReadInfrastructure7()
  {
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure7",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", export.Case1.Number);
        db.SetInt32(command, "eventId", local.ReviewType.EventId);
        db.SetString(command, "eventType", local.ReviewType.EventType);
        db.SetString(command, "reasonCode", local.ReviewType.ReasonCode);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.SituationNumber = db.GetInt32(reader, 1);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 2);
        entities.Infrastructure.EventId = db.GetInt32(reader, 3);
        entities.Infrastructure.EventType = db.GetString(reader, 4);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 5);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 6);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 7);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 8);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 9);
        entities.Infrastructure.DenormDate = db.GetNullableDate(reader, 10);
        entities.Infrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.Infrastructure.InitiatingStateCode = db.GetString(reader, 12);
        entities.Infrastructure.CsenetInOutCode = db.GetString(reader, 13);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 14);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 15);
        entities.Infrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 16);
        entities.Infrastructure.UserId = db.GetString(reader, 17);
        entities.Infrastructure.CreatedBy = db.GetString(reader, 18);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.Infrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 20);
        entities.Infrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.Infrastructure.Function = db.GetNullableString(reader, 23);
        entities.Infrastructure.CaseUnitState =
          db.GetNullableString(reader, 24);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 25);
        entities.Infrastructure.Populated = true;
      });
  }

  private bool ReadInfrastructure8()
  {
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure8",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", export.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.SituationNumber = db.GetInt32(reader, 1);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 2);
        entities.Infrastructure.EventId = db.GetInt32(reader, 3);
        entities.Infrastructure.EventType = db.GetString(reader, 4);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 5);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 6);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 7);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 8);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 9);
        entities.Infrastructure.DenormDate = db.GetNullableDate(reader, 10);
        entities.Infrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.Infrastructure.InitiatingStateCode = db.GetString(reader, 12);
        entities.Infrastructure.CsenetInOutCode = db.GetString(reader, 13);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 14);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 15);
        entities.Infrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 16);
        entities.Infrastructure.UserId = db.GetString(reader, 17);
        entities.Infrastructure.CreatedBy = db.GetString(reader, 18);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.Infrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 20);
        entities.Infrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.Infrastructure.Function = db.GetNullableString(reader, 23);
        entities.Infrastructure.CaseUnitState =
          db.GetNullableString(reader, 24);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 25);
        entities.Infrastructure.Populated = true;
      });
  }

  private bool ReadInfrastructure9()
  {
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure9",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", export.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.SituationNumber = db.GetInt32(reader, 1);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 2);
        entities.Infrastructure.EventId = db.GetInt32(reader, 3);
        entities.Infrastructure.EventType = db.GetString(reader, 4);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 5);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 6);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 7);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 8);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 9);
        entities.Infrastructure.DenormDate = db.GetNullableDate(reader, 10);
        entities.Infrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.Infrastructure.InitiatingStateCode = db.GetString(reader, 12);
        entities.Infrastructure.CsenetInOutCode = db.GetString(reader, 13);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 14);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 15);
        entities.Infrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 16);
        entities.Infrastructure.UserId = db.GetString(reader, 17);
        entities.Infrastructure.CreatedBy = db.GetString(reader, 18);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.Infrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 20);
        entities.Infrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.Infrastructure.Function = db.GetNullableString(reader, 23);
        entities.Infrastructure.CaseUnitState =
          db.GetNullableString(reader, 24);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 25);
        entities.Infrastructure.Populated = true;
      });
  }

  private IEnumerable<bool> ReadNarrativeDetail1()
  {
    entities.Existing.Populated = false;

    return ReadEach("ReadNarrativeDetail1",
      (db, command) =>
      {
        db.SetInt32(
          command, "infrastructureId",
          entities.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Existing.InfrastructureId = db.GetInt32(reader, 0);
        entities.Existing.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.Existing.NarrativeText = db.GetNullableString(reader, 2);
        entities.Existing.LineNumber = db.GetInt32(reader, 3);
        entities.Existing.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadNarrativeDetail10()
  {
    entities.Existing.Populated = false;

    return ReadEach("ReadNarrativeDetail10",
      (db, command) =>
      {
        db.SetInt32(
          command, "infrastructureId",
          entities.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Existing.InfrastructureId = db.GetInt32(reader, 0);
        entities.Existing.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.Existing.NarrativeText = db.GetNullableString(reader, 2);
        entities.Existing.LineNumber = db.GetInt32(reader, 3);
        entities.Existing.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadNarrativeDetail11()
  {
    entities.Existing.Populated = false;

    return ReadEach("ReadNarrativeDetail11",
      (db, command) =>
      {
        db.SetInt32(
          command, "infrastructureId",
          entities.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Existing.InfrastructureId = db.GetInt32(reader, 0);
        entities.Existing.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.Existing.NarrativeText = db.GetNullableString(reader, 2);
        entities.Existing.LineNumber = db.GetInt32(reader, 3);
        entities.Existing.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadNarrativeDetail12()
  {
    entities.Existing.Populated = false;

    return ReadEach("ReadNarrativeDetail12",
      (db, command) =>
      {
        db.SetInt32(
          command, "infrastructureId",
          entities.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Existing.InfrastructureId = db.GetInt32(reader, 0);
        entities.Existing.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.Existing.NarrativeText = db.GetNullableString(reader, 2);
        entities.Existing.LineNumber = db.GetInt32(reader, 3);
        entities.Existing.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadNarrativeDetail13()
  {
    entities.Existing.Populated = false;

    return ReadEach("ReadNarrativeDetail13",
      (db, command) =>
      {
        db.SetInt32(
          command, "infrastructureId",
          entities.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Existing.InfrastructureId = db.GetInt32(reader, 0);
        entities.Existing.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.Existing.NarrativeText = db.GetNullableString(reader, 2);
        entities.Existing.LineNumber = db.GetInt32(reader, 3);
        entities.Existing.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadNarrativeDetail14()
  {
    entities.Existing.Populated = false;

    return ReadEach("ReadNarrativeDetail14",
      (db, command) =>
      {
        db.SetInt32(
          command, "infrastructureId",
          entities.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Existing.InfrastructureId = db.GetInt32(reader, 0);
        entities.Existing.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.Existing.NarrativeText = db.GetNullableString(reader, 2);
        entities.Existing.LineNumber = db.GetInt32(reader, 3);
        entities.Existing.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadNarrativeDetail2()
  {
    entities.Existing.Populated = false;

    return ReadEach("ReadNarrativeDetail2",
      (db, command) =>
      {
        db.SetInt32(
          command, "infrastructureId",
          entities.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Existing.InfrastructureId = db.GetInt32(reader, 0);
        entities.Existing.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.Existing.NarrativeText = db.GetNullableString(reader, 2);
        entities.Existing.LineNumber = db.GetInt32(reader, 3);
        entities.Existing.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadNarrativeDetail3()
  {
    entities.Existing.Populated = false;

    return ReadEach("ReadNarrativeDetail3",
      (db, command) =>
      {
        db.SetInt32(
          command, "infrastructureId",
          entities.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Existing.InfrastructureId = db.GetInt32(reader, 0);
        entities.Existing.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.Existing.NarrativeText = db.GetNullableString(reader, 2);
        entities.Existing.LineNumber = db.GetInt32(reader, 3);
        entities.Existing.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadNarrativeDetail4()
  {
    entities.Existing.Populated = false;

    return ReadEach("ReadNarrativeDetail4",
      (db, command) =>
      {
        db.SetInt32(
          command, "infrastructureId",
          entities.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Existing.InfrastructureId = db.GetInt32(reader, 0);
        entities.Existing.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.Existing.NarrativeText = db.GetNullableString(reader, 2);
        entities.Existing.LineNumber = db.GetInt32(reader, 3);
        entities.Existing.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadNarrativeDetail5()
  {
    entities.Existing.Populated = false;

    return ReadEach("ReadNarrativeDetail5",
      (db, command) =>
      {
        db.SetInt32(
          command, "infrastructureId",
          entities.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Existing.InfrastructureId = db.GetInt32(reader, 0);
        entities.Existing.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.Existing.NarrativeText = db.GetNullableString(reader, 2);
        entities.Existing.LineNumber = db.GetInt32(reader, 3);
        entities.Existing.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadNarrativeDetail6()
  {
    entities.Existing.Populated = false;

    return ReadEach("ReadNarrativeDetail6",
      (db, command) =>
      {
        db.SetInt32(
          command, "infrastructureId",
          entities.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Existing.InfrastructureId = db.GetInt32(reader, 0);
        entities.Existing.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.Existing.NarrativeText = db.GetNullableString(reader, 2);
        entities.Existing.LineNumber = db.GetInt32(reader, 3);
        entities.Existing.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadNarrativeDetail7()
  {
    entities.Existing.Populated = false;

    return ReadEach("ReadNarrativeDetail7",
      (db, command) =>
      {
        db.SetInt32(
          command, "infrastructureId",
          entities.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Existing.InfrastructureId = db.GetInt32(reader, 0);
        entities.Existing.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.Existing.NarrativeText = db.GetNullableString(reader, 2);
        entities.Existing.LineNumber = db.GetInt32(reader, 3);
        entities.Existing.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadNarrativeDetail8()
  {
    entities.Existing.Populated = false;

    return ReadEach("ReadNarrativeDetail8",
      (db, command) =>
      {
        db.SetInt32(
          command, "infrastructureId",
          entities.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Existing.InfrastructureId = db.GetInt32(reader, 0);
        entities.Existing.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.Existing.NarrativeText = db.GetNullableString(reader, 2);
        entities.Existing.LineNumber = db.GetInt32(reader, 3);
        entities.Existing.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadNarrativeDetail9()
  {
    entities.Existing.Populated = false;

    return ReadEach("ReadNarrativeDetail9",
      (db, command) =>
      {
        db.SetInt32(
          command, "infrastructureId",
          entities.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Existing.InfrastructureId = db.GetInt32(reader, 0);
        entities.Existing.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.Existing.NarrativeText = db.GetNullableString(reader, 2);
        entities.Existing.LineNumber = db.GetInt32(reader, 3);
        entities.Existing.Populated = true;

        return true;
      });
  }

  private bool ReadOffice()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.Office.Populated = false;

    return Read("ReadOffice",
      (db, command) =>
      {
        db.SetInt32(
          command, "officeId", entities.OfficeServiceProvider.OffGeneratedId);
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.Name = db.GetString(reader, 1);
        entities.Office.EffectiveDate = db.GetDate(reader, 2);
        entities.Office.DiscontinueDate = db.GetNullableDate(reader, 3);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 4);
        entities.Office.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvider()
  {
    System.Diagnostics.Debug.Assert(entities.CaseAssignment.Populated);
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          entities.CaseAssignment.OspDate.GetValueOrDefault());
        db.SetString(command, "roleCode", entities.CaseAssignment.OspCode);
        db.SetInt32(command, "offGeneratedId", entities.CaseAssignment.OffId);
        db.SetInt32(command, "spdGeneratedId", entities.CaseAssignment.SpdId);
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.OfficeServiceProvider.Populated = true;
      });
  }

  private IEnumerable<bool> ReadProgram()
  {
    return ReadEach("ReadProgram",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        if (export.Program.IsFull)
        {
          return false;
        }

        entities.Program1.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Program1.Code = db.GetString(reader, 1);
        entities.Program1.Populated = true;

        return true;
      });
  }

  private bool ReadServiceProvider1()
  {
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider1",
      (db, command) =>
      {
        db.SetString(command, "userId", entities.Infrastructure.CreatedBy);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.UserId = db.GetString(reader, 1);
        entities.ServiceProvider.LastName = db.GetString(reader, 2);
        entities.ServiceProvider.FirstName = db.GetString(reader, 3);
        entities.ServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProvider2()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider2",
      (db, command) =>
      {
        db.SetInt32(
          command, "servicePrvderId",
          entities.OfficeServiceProvider.SpdGeneratedId);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.UserId = db.GetString(reader, 1);
        entities.ServiceProvider.LastName = db.GetString(reader, 2);
        entities.ServiceProvider.FirstName = db.GetString(reader, 3);
        entities.ServiceProvider.Populated = true;
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
    /// <summary>A HiddenImportModifiedGroup group.</summary>
    [Serializable]
    public class HiddenImportModifiedGroup
    {
      /// <summary>
      /// A value of HiddenPassedFlag.
      /// </summary>
      [JsonPropertyName("hiddenPassedFlag")]
      public Common HiddenPassedFlag
      {
        get => hiddenPassedFlag ??= new();
        set => hiddenPassedFlag = value;
      }

      /// <summary>
      /// A value of ModifiedNarrative.
      /// </summary>
      [JsonPropertyName("modifiedNarrative")]
      public NarrativeWork ModifiedNarrative
      {
        get => modifiedNarrative ??= new();
        set => modifiedNarrative = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private Common hiddenPassedFlag;
      private NarrativeWork modifiedNarrative;
    }

    /// <summary>A PasForUpdtGroup group.</summary>
    [Serializable]
    public class PasForUpdtGroup
    {
      /// <summary>
      /// A value of GimportH.
      /// </summary>
      [JsonPropertyName("gimportH")]
      public LegalAction GimportH
      {
        get => gimportH ??= new();
        set => gimportH = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private LegalAction gimportH;
    }

    /// <summary>A PassReviewNotesGroup group.</summary>
    [Serializable]
    public class PassReviewNotesGroup
    {
      /// <summary>
      /// A value of GimportHiddenPassedFlag.
      /// </summary>
      [JsonPropertyName("gimportHiddenPassedFlag")]
      public Common GimportHiddenPassedFlag
      {
        get => gimportHiddenPassedFlag ??= new();
        set => gimportHiddenPassedFlag = value;
      }

      /// <summary>
      /// A value of GimportHidden.
      /// </summary>
      [JsonPropertyName("gimportHidden")]
      public NarrativeWork GimportHidden
      {
        get => gimportHidden ??= new();
        set => gimportHidden = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private Common gimportHiddenPassedFlag;
      private NarrativeWork gimportHidden;
    }

    /// <summary>A ProgramGroup group.</summary>
    [Serializable]
    public class ProgramGroup
    {
      /// <summary>
      /// A value of Program1.
      /// </summary>
      [JsonPropertyName("program1")]
      public Program Program1
      {
        get => program1 ??= new();
        set => program1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 8;

      private Program program1;
    }

    /// <summary>A DisplayReviewNoteGroup group.</summary>
    [Serializable]
    public class DisplayReviewNoteGroup
    {
      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public NarrativeWork G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private NarrativeWork g;
    }

    /// <summary>
    /// A value of Flag.
    /// </summary>
    [JsonPropertyName("flag")]
    public Common Flag
    {
      get => flag ??= new();
      set => flag = value;
    }

    /// <summary>
    /// Gets a value of HiddenImportModified.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenImportModifiedGroup> HiddenImportModified =>
      hiddenImportModified ??= new(HiddenImportModifiedGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenImportModified for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenImportModified")]
    [Computed]
    public IList<HiddenImportModifiedGroup> HiddenImportModified_Json
    {
      get => hiddenImportModified;
      set => HiddenImportModified.Assign(value);
    }

    /// <summary>
    /// A value of ModOrPer.
    /// </summary>
    [JsonPropertyName("modOrPer")]
    public Infrastructure ModOrPer
    {
      get => modOrPer ??= new();
      set => modOrPer = value;
    }

    /// <summary>
    /// A value of Import2LastReview.
    /// </summary>
    [JsonPropertyName("import2LastReview")]
    public DateWorkArea Import2LastReview
    {
      get => import2LastReview ??= new();
      set => import2LastReview = value;
    }

    /// <summary>
    /// A value of Import2.
    /// </summary>
    [JsonPropertyName("import2")]
    public ServiceProvider Import2
    {
      get => import2 ??= new();
      set => import2 = value;
    }

    /// <summary>
    /// A value of ClosedCaseIndicator.
    /// </summary>
    [JsonPropertyName("closedCaseIndicator")]
    public Common ClosedCaseIndicator
    {
      get => closedCaseIndicator ??= new();
      set => closedCaseIndicator = value;
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
    /// A value of HiddenReviewType.
    /// </summary>
    [JsonPropertyName("hiddenReviewType")]
    public Common HiddenReviewType
    {
      get => hiddenReviewType ??= new();
      set => hiddenReviewType = value;
    }

    /// <summary>
    /// A value of HiddenOrigAndPass.
    /// </summary>
    [JsonPropertyName("hiddenOrigAndPass")]
    public Infrastructure HiddenOrigAndPass
    {
      get => hiddenOrigAndPass ??= new();
      set => hiddenOrigAndPass = value;
    }

    /// <summary>
    /// Gets a value of PasForUpdt.
    /// </summary>
    [JsonIgnore]
    public Array<PasForUpdtGroup> PasForUpdt => pasForUpdt ??= new(
      PasForUpdtGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of PasForUpdt for json serialization.
    /// </summary>
    [JsonPropertyName("pasForUpdt")]
    [Computed]
    public IList<PasForUpdtGroup> PasForUpdt_Json
    {
      get => pasForUpdt;
      set => PasForUpdt.Assign(value);
    }

    /// <summary>
    /// Gets a value of PassReviewNotes.
    /// </summary>
    [JsonIgnore]
    public Array<PassReviewNotesGroup> PassReviewNotes =>
      passReviewNotes ??= new(PassReviewNotesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of PassReviewNotes for json serialization.
    /// </summary>
    [JsonPropertyName("passReviewNotes")]
    [Computed]
    public IList<PassReviewNotesGroup> PassReviewNotes_Json
    {
      get => passReviewNotes;
      set => PassReviewNotes.Assign(value);
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of CaseCoordinator.
    /// </summary>
    [JsonPropertyName("caseCoordinator")]
    public ServiceProvider CaseCoordinator
    {
      get => caseCoordinator ??= new();
      set => caseCoordinator = value;
    }

    /// <summary>
    /// A value of Case2.
    /// </summary>
    [JsonPropertyName("case2")]
    public Office Case2
    {
      get => case2 ??= new();
      set => case2 = value;
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
    /// A value of Ap1.
    /// </summary>
    [JsonPropertyName("ap1")]
    public CsePersonsWorkSet Ap1
    {
      get => ap1 ??= new();
      set => ap1 = value;
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
    /// A value of Pgm1.
    /// </summary>
    [JsonPropertyName("pgm1")]
    public Program Pgm1
    {
      get => pgm1 ??= new();
      set => pgm1 = value;
    }

    /// <summary>
    /// A value of Pgm2.
    /// </summary>
    [JsonPropertyName("pgm2")]
    public Program Pgm2
    {
      get => pgm2 ??= new();
      set => pgm2 = value;
    }

    /// <summary>
    /// A value of Pgm3.
    /// </summary>
    [JsonPropertyName("pgm3")]
    public Program Pgm3
    {
      get => pgm3 ??= new();
      set => pgm3 = value;
    }

    /// <summary>
    /// A value of Pgm4.
    /// </summary>
    [JsonPropertyName("pgm4")]
    public Program Pgm4
    {
      get => pgm4 ??= new();
      set => pgm4 = value;
    }

    /// <summary>
    /// A value of Pgm5.
    /// </summary>
    [JsonPropertyName("pgm5")]
    public Program Pgm5
    {
      get => pgm5 ??= new();
      set => pgm5 = value;
    }

    /// <summary>
    /// A value of Pgm6.
    /// </summary>
    [JsonPropertyName("pgm6")]
    public Program Pgm6
    {
      get => pgm6 ??= new();
      set => pgm6 = value;
    }

    /// <summary>
    /// A value of Pgm7.
    /// </summary>
    [JsonPropertyName("pgm7")]
    public Program Pgm7
    {
      get => pgm7 ??= new();
      set => pgm7 = value;
    }

    /// <summary>
    /// A value of Pgm8.
    /// </summary>
    [JsonPropertyName("pgm8")]
    public Program Pgm8
    {
      get => pgm8 ??= new();
      set => pgm8 = value;
    }

    /// <summary>
    /// Gets a value of Program.
    /// </summary>
    [JsonIgnore]
    public Array<ProgramGroup> Program =>
      program ??= new(ProgramGroup.Capacity);

    /// <summary>
    /// Gets a value of Program for json serialization.
    /// </summary>
    [JsonPropertyName("program")]
    [Computed]
    public IList<ProgramGroup> Program_Json
    {
      get => program;
      set => Program.Assign(value);
    }

    /// <summary>
    /// Gets a value of DisplayReviewNote.
    /// </summary>
    [JsonIgnore]
    public Array<DisplayReviewNoteGroup> DisplayReviewNote =>
      displayReviewNote ??= new(DisplayReviewNoteGroup.Capacity);

    /// <summary>
    /// Gets a value of DisplayReviewNote for json serialization.
    /// </summary>
    [JsonPropertyName("displayReviewNote")]
    [Computed]
    public IList<DisplayReviewNoteGroup> DisplayReviewNote_Json
    {
      get => displayReviewNote;
      set => DisplayReviewNote.Assign(value);
    }

    /// <summary>
    /// A value of FunctionDesc.
    /// </summary>
    [JsonPropertyName("functionDesc")]
    public TextWorkArea FunctionDesc
    {
      get => functionDesc ??= new();
      set => functionDesc = value;
    }

    /// <summary>
    /// A value of LastReview.
    /// </summary>
    [JsonPropertyName("lastReview")]
    public DateWorkArea LastReview
    {
      get => lastReview ??= new();
      set => lastReview = value;
    }

    /// <summary>
    /// A value of MoreApsMsg.
    /// </summary>
    [JsonPropertyName("moreApsMsg")]
    public TextWorkArea MoreApsMsg
    {
      get => moreApsMsg ??= new();
      set => moreApsMsg = value;
    }

    /// <summary>
    /// A value of CommandPassedFromEnfor.
    /// </summary>
    [JsonPropertyName("commandPassedFromEnfor")]
    public Common CommandPassedFromEnfor
    {
      get => commandPassedFromEnfor ??= new();
      set => commandPassedFromEnfor = value;
    }

    /// <summary>
    /// A value of MultiAp.
    /// </summary>
    [JsonPropertyName("multiAp")]
    public Common MultiAp
    {
      get => multiAp ??= new();
      set => multiAp = value;
    }

    /// <summary>
    /// A value of ApSelected.
    /// </summary>
    [JsonPropertyName("apSelected")]
    public Common ApSelected
    {
      get => apSelected ??= new();
      set => apSelected = value;
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
    /// A value of CrenModOrderInd.
    /// </summary>
    [JsonPropertyName("crenModOrderInd")]
    public Common CrenModOrderInd
    {
      get => crenModOrderInd ??= new();
      set => crenModOrderInd = value;
    }

    /// <summary>
    /// A value of LastRvwResultedInMod.
    /// </summary>
    [JsonPropertyName("lastRvwResultedInMod")]
    public Common LastRvwResultedInMod
    {
      get => lastRvwResultedInMod ??= new();
      set => lastRvwResultedInMod = value;
    }

    private Common flag;
    private Array<HiddenImportModifiedGroup> hiddenImportModified;
    private Infrastructure modOrPer;
    private DateWorkArea import2LastReview;
    private ServiceProvider import2;
    private Common closedCaseIndicator;
    private NextTranInfo hidden;
    private Common hiddenReviewType;
    private Infrastructure hiddenOrigAndPass;
    private Array<PasForUpdtGroup> pasForUpdt;
    private Array<PassReviewNotesGroup> passReviewNotes;
    private Case1 case1;
    private Standard standard;
    private ServiceProvider caseCoordinator;
    private Office case2;
    private CsePersonsWorkSet ar;
    private CsePersonsWorkSet ap1;
    private ServiceProvider serviceProvider;
    private Program pgm1;
    private Program pgm2;
    private Program pgm3;
    private Program pgm4;
    private Program pgm5;
    private Program pgm6;
    private Program pgm7;
    private Program pgm8;
    private Array<ProgramGroup> program;
    private Array<DisplayReviewNoteGroup> displayReviewNote;
    private TextWorkArea functionDesc;
    private DateWorkArea lastReview;
    private TextWorkArea moreApsMsg;
    private Common commandPassedFromEnfor;
    private Common multiAp;
    private Common apSelected;
    private CsePersonsWorkSet selectedAp;
    private Common crenModOrderInd;
    private Common lastRvwResultedInMod;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A HiddenExportModifiedGroup group.</summary>
    [Serializable]
    public class HiddenExportModifiedGroup
    {
      /// <summary>
      /// A value of HiddenPassedFlag.
      /// </summary>
      [JsonPropertyName("hiddenPassedFlag")]
      public Common HiddenPassedFlag
      {
        get => hiddenPassedFlag ??= new();
        set => hiddenPassedFlag = value;
      }

      /// <summary>
      /// A value of ModifiedNarrative.
      /// </summary>
      [JsonPropertyName("modifiedNarrative")]
      public NarrativeWork ModifiedNarrative
      {
        get => modifiedNarrative ??= new();
        set => modifiedNarrative = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private Common hiddenPassedFlag;
      private NarrativeWork modifiedNarrative;
    }

    /// <summary>A PasForUpdtGroup group.</summary>
    [Serializable]
    public class PasForUpdtGroup
    {
      /// <summary>
      /// A value of GexportH.
      /// </summary>
      [JsonPropertyName("gexportH")]
      public LegalAction GexportH
      {
        get => gexportH ??= new();
        set => gexportH = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private LegalAction gexportH;
    }

    /// <summary>A PassReviewNoteGroup group.</summary>
    [Serializable]
    public class PassReviewNoteGroup
    {
      /// <summary>
      /// A value of GexportHiddenPassedFlag.
      /// </summary>
      [JsonPropertyName("gexportHiddenPassedFlag")]
      public Common GexportHiddenPassedFlag
      {
        get => gexportHiddenPassedFlag ??= new();
        set => gexportHiddenPassedFlag = value;
      }

      /// <summary>
      /// A value of GexportHidden.
      /// </summary>
      [JsonPropertyName("gexportHidden")]
      public NarrativeWork GexportHidden
      {
        get => gexportHidden ??= new();
        set => gexportHidden = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private Common gexportHiddenPassedFlag;
      private NarrativeWork gexportHidden;
    }

    /// <summary>A ProgramGroup group.</summary>
    [Serializable]
    public class ProgramGroup
    {
      /// <summary>
      /// A value of Program1.
      /// </summary>
      [JsonPropertyName("program1")]
      public Program Program1
      {
        get => program1 ??= new();
        set => program1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 8;

      private Program program1;
    }

    /// <summary>A DisplayReviewNoteGroup group.</summary>
    [Serializable]
    public class DisplayReviewNoteGroup
    {
      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public NarrativeWork G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private NarrativeWork g;
    }

    /// <summary>
    /// A value of Flag.
    /// </summary>
    [JsonPropertyName("flag")]
    public Common Flag
    {
      get => flag ??= new();
      set => flag = value;
    }

    /// <summary>
    /// Gets a value of HiddenExportModified.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenExportModifiedGroup> HiddenExportModified =>
      hiddenExportModified ??= new(HiddenExportModifiedGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenExportModified for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenExportModified")]
    [Computed]
    public IList<HiddenExportModifiedGroup> HiddenExportModified_Json
    {
      get => hiddenExportModified;
      set => HiddenExportModified.Assign(value);
    }

    /// <summary>
    /// A value of ModOrPer.
    /// </summary>
    [JsonPropertyName("modOrPer")]
    public Infrastructure ModOrPer
    {
      get => modOrPer ??= new();
      set => modOrPer = value;
    }

    /// <summary>
    /// A value of Export2LastReview.
    /// </summary>
    [JsonPropertyName("export2LastReview")]
    public DateWorkArea Export2LastReview
    {
      get => export2LastReview ??= new();
      set => export2LastReview = value;
    }

    /// <summary>
    /// A value of Export2.
    /// </summary>
    [JsonPropertyName("export2")]
    public ServiceProvider Export2
    {
      get => export2 ??= new();
      set => export2 = value;
    }

    /// <summary>
    /// A value of ClosedCaseIndicator.
    /// </summary>
    [JsonPropertyName("closedCaseIndicator")]
    public Common ClosedCaseIndicator
    {
      get => closedCaseIndicator ??= new();
      set => closedCaseIndicator = value;
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
    /// A value of HiddenReviewType.
    /// </summary>
    [JsonPropertyName("hiddenReviewType")]
    public Common HiddenReviewType
    {
      get => hiddenReviewType ??= new();
      set => hiddenReviewType = value;
    }

    /// <summary>
    /// A value of HiddenOrigAndPass.
    /// </summary>
    [JsonPropertyName("hiddenOrigAndPass")]
    public Infrastructure HiddenOrigAndPass
    {
      get => hiddenOrigAndPass ??= new();
      set => hiddenOrigAndPass = value;
    }

    /// <summary>
    /// Gets a value of PasForUpdt.
    /// </summary>
    [JsonIgnore]
    public Array<PasForUpdtGroup> PasForUpdt => pasForUpdt ??= new(
      PasForUpdtGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of PasForUpdt for json serialization.
    /// </summary>
    [JsonPropertyName("pasForUpdt")]
    [Computed]
    public IList<PasForUpdtGroup> PasForUpdt_Json
    {
      get => pasForUpdt;
      set => PasForUpdt.Assign(value);
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
    /// Gets a value of PassReviewNote.
    /// </summary>
    [JsonIgnore]
    public Array<PassReviewNoteGroup> PassReviewNote => passReviewNote ??= new(
      PassReviewNoteGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of PassReviewNote for json serialization.
    /// </summary>
    [JsonPropertyName("passReviewNote")]
    [Computed]
    public IList<PassReviewNoteGroup> PassReviewNote_Json
    {
      get => passReviewNote;
      set => PassReviewNote.Assign(value);
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
    /// A value of CaseCoordinator.
    /// </summary>
    [JsonPropertyName("caseCoordinator")]
    public ServiceProvider CaseCoordinator
    {
      get => caseCoordinator ??= new();
      set => caseCoordinator = value;
    }

    /// <summary>
    /// A value of Case2.
    /// </summary>
    [JsonPropertyName("case2")]
    public Office Case2
    {
      get => case2 ??= new();
      set => case2 = value;
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
    /// A value of Ap1.
    /// </summary>
    [JsonPropertyName("ap1")]
    public CsePersonsWorkSet Ap1
    {
      get => ap1 ??= new();
      set => ap1 = value;
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
    /// A value of Pgm1.
    /// </summary>
    [JsonPropertyName("pgm1")]
    public Program Pgm1
    {
      get => pgm1 ??= new();
      set => pgm1 = value;
    }

    /// <summary>
    /// A value of Pgm2.
    /// </summary>
    [JsonPropertyName("pgm2")]
    public Program Pgm2
    {
      get => pgm2 ??= new();
      set => pgm2 = value;
    }

    /// <summary>
    /// A value of Pgm3.
    /// </summary>
    [JsonPropertyName("pgm3")]
    public Program Pgm3
    {
      get => pgm3 ??= new();
      set => pgm3 = value;
    }

    /// <summary>
    /// A value of Pgm4.
    /// </summary>
    [JsonPropertyName("pgm4")]
    public Program Pgm4
    {
      get => pgm4 ??= new();
      set => pgm4 = value;
    }

    /// <summary>
    /// A value of Pgm5.
    /// </summary>
    [JsonPropertyName("pgm5")]
    public Program Pgm5
    {
      get => pgm5 ??= new();
      set => pgm5 = value;
    }

    /// <summary>
    /// A value of Pgm6.
    /// </summary>
    [JsonPropertyName("pgm6")]
    public Program Pgm6
    {
      get => pgm6 ??= new();
      set => pgm6 = value;
    }

    /// <summary>
    /// A value of Pgm7.
    /// </summary>
    [JsonPropertyName("pgm7")]
    public Program Pgm7
    {
      get => pgm7 ??= new();
      set => pgm7 = value;
    }

    /// <summary>
    /// A value of Pgm8.
    /// </summary>
    [JsonPropertyName("pgm8")]
    public Program Pgm8
    {
      get => pgm8 ??= new();
      set => pgm8 = value;
    }

    /// <summary>
    /// Gets a value of Program.
    /// </summary>
    [JsonIgnore]
    public Array<ProgramGroup> Program =>
      program ??= new(ProgramGroup.Capacity);

    /// <summary>
    /// Gets a value of Program for json serialization.
    /// </summary>
    [JsonPropertyName("program")]
    [Computed]
    public IList<ProgramGroup> Program_Json
    {
      get => program;
      set => Program.Assign(value);
    }

    /// <summary>
    /// Gets a value of DisplayReviewNote.
    /// </summary>
    [JsonIgnore]
    public Array<DisplayReviewNoteGroup> DisplayReviewNote =>
      displayReviewNote ??= new(DisplayReviewNoteGroup.Capacity);

    /// <summary>
    /// Gets a value of DisplayReviewNote for json serialization.
    /// </summary>
    [JsonPropertyName("displayReviewNote")]
    [Computed]
    public IList<DisplayReviewNoteGroup> DisplayReviewNote_Json
    {
      get => displayReviewNote;
      set => DisplayReviewNote.Assign(value);
    }

    /// <summary>
    /// A value of FunctionDesc.
    /// </summary>
    [JsonPropertyName("functionDesc")]
    public TextWorkArea FunctionDesc
    {
      get => functionDesc ??= new();
      set => functionDesc = value;
    }

    /// <summary>
    /// A value of LastReview.
    /// </summary>
    [JsonPropertyName("lastReview")]
    public DateWorkArea LastReview
    {
      get => lastReview ??= new();
      set => lastReview = value;
    }

    /// <summary>
    /// A value of MoreApsMsg.
    /// </summary>
    [JsonPropertyName("moreApsMsg")]
    public TextWorkArea MoreApsMsg
    {
      get => moreApsMsg ??= new();
      set => moreApsMsg = value;
    }

    /// <summary>
    /// A value of CommandPassedFromEnfor.
    /// </summary>
    [JsonPropertyName("commandPassedFromEnfor")]
    public Common CommandPassedFromEnfor
    {
      get => commandPassedFromEnfor ??= new();
      set => commandPassedFromEnfor = value;
    }

    /// <summary>
    /// A value of MultiAp.
    /// </summary>
    [JsonPropertyName("multiAp")]
    public Common MultiAp
    {
      get => multiAp ??= new();
      set => multiAp = value;
    }

    /// <summary>
    /// A value of ApSelected.
    /// </summary>
    [JsonPropertyName("apSelected")]
    public Common ApSelected
    {
      get => apSelected ??= new();
      set => apSelected = value;
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
    /// A value of LastRvwResultedInMod.
    /// </summary>
    [JsonPropertyName("lastRvwResultedInMod")]
    public Common LastRvwResultedInMod
    {
      get => lastRvwResultedInMod ??= new();
      set => lastRvwResultedInMod = value;
    }

    private Common flag;
    private Array<HiddenExportModifiedGroup> hiddenExportModified;
    private Infrastructure modOrPer;
    private DateWorkArea export2LastReview;
    private ServiceProvider export2;
    private Common closedCaseIndicator;
    private NextTranInfo hidden;
    private Common hiddenReviewType;
    private Infrastructure hiddenOrigAndPass;
    private Array<PasForUpdtGroup> pasForUpdt;
    private Case1 case1;
    private Array<PassReviewNoteGroup> passReviewNote;
    private Standard standard;
    private ServiceProvider caseCoordinator;
    private Office case2;
    private CsePersonsWorkSet ar;
    private CsePersonsWorkSet ap1;
    private ServiceProvider serviceProvider;
    private Program pgm1;
    private Program pgm2;
    private Program pgm3;
    private Program pgm4;
    private Program pgm5;
    private Program pgm6;
    private Program pgm7;
    private Program pgm8;
    private Array<ProgramGroup> program;
    private Array<DisplayReviewNoteGroup> displayReviewNote;
    private TextWorkArea functionDesc;
    private DateWorkArea lastReview;
    private TextWorkArea moreApsMsg;
    private Common commandPassedFromEnfor;
    private Common multiAp;
    private Common apSelected;
    private CsePersonsWorkSet selectedAp;
    private Common lastRvwResultedInMod;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A WorkGroup1 group.</summary>
    [Serializable]
    public class WorkGroup1
    {
      /// <summary>
      /// A value of Work.
      /// </summary>
      [JsonPropertyName("work")]
      public NarrativeWork Work
      {
        get => work ??= new();
        set => work = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private NarrativeWork work;
    }

    /// <summary>A WorkGroup2 group.</summary>
    [Serializable]
    public class WorkGroup2
    {
      /// <summary>
      /// A value of Work.
      /// </summary>
      [JsonPropertyName("work")]
      public NarrativeDetail Work
      {
        get => work ??= new();
        set => work = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private NarrativeDetail work;
    }

    /// <summary>A WorkGroupMedGroup group.</summary>
    [Serializable]
    public class WorkGroupMedGroup
    {
      /// <summary>
      /// A value of WorkGrpMed.
      /// </summary>
      [JsonPropertyName("workGrpMed")]
      public NarrativeDetail WorkGrpMed
      {
        get => workGrpMed ??= new();
        set => workGrpMed = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private NarrativeDetail workGrpMed;
    }

    /// <summary>A WorkGroupPatGroup group.</summary>
    [Serializable]
    public class WorkGroupPatGroup
    {
      /// <summary>
      /// A value of WorkGrpPat.
      /// </summary>
      [JsonPropertyName("workGrpPat")]
      public NarrativeDetail WorkGrpPat
      {
        get => workGrpPat ??= new();
        set => workGrpPat = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private NarrativeDetail workGrpPat;
    }

    /// <summary>A WorkGroupEstGroup group.</summary>
    [Serializable]
    public class WorkGroupEstGroup
    {
      /// <summary>
      /// A value of WorkGrpEst.
      /// </summary>
      [JsonPropertyName("workGrpEst")]
      public NarrativeDetail WorkGrpEst
      {
        get => workGrpEst ??= new();
        set => workGrpEst = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private NarrativeDetail workGrpEst;
    }

    /// <summary>A WorkGroupEnfGroup group.</summary>
    [Serializable]
    public class WorkGroupEnfGroup
    {
      /// <summary>
      /// A value of WorkGrpEnf.
      /// </summary>
      [JsonPropertyName("workGrpEnf")]
      public NarrativeDetail WorkGrpEnf
      {
        get => workGrpEnf ??= new();
        set => workGrpEnf = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private NarrativeDetail workGrpEnf;
    }

    /// <summary>
    /// A value of InfrastructureFound.
    /// </summary>
    [JsonPropertyName("infrastructureFound")]
    public Common InfrastructureFound
    {
      get => infrastructureFound ??= new();
      set => infrastructureFound = value;
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
    /// A value of StartingPosition.
    /// </summary>
    [JsonPropertyName("startingPosition")]
    public Common StartingPosition
    {
      get => startingPosition ??= new();
      set => startingPosition = value;
    }

    /// <summary>
    /// A value of Length.
    /// </summary>
    [JsonPropertyName("length")]
    public Common Length
    {
      get => length ??= new();
      set => length = value;
    }

    /// <summary>
    /// Gets a value of Work1.
    /// </summary>
    [JsonIgnore]
    public Array<WorkGroup1> Work1 => work1 ??= new(WorkGroup1.Capacity, 0);

    /// <summary>
    /// Gets a value of Work1 for json serialization.
    /// </summary>
    [JsonPropertyName("work1")]
    [Computed]
    public IList<WorkGroup1> Work1_Json
    {
      get => work1;
      set => Work1.Assign(value);
    }

    /// <summary>
    /// Gets a value of Work2.
    /// </summary>
    [JsonIgnore]
    public Array<WorkGroup2> Work2 => work2 ??= new(WorkGroup2.Capacity, 0);

    /// <summary>
    /// Gets a value of Work2 for json serialization.
    /// </summary>
    [JsonPropertyName("work2")]
    [Computed]
    public IList<WorkGroup2> Work2_Json
    {
      get => work2;
      set => Work2.Assign(value);
    }

    /// <summary>
    /// Gets a value of WorkGroupMed.
    /// </summary>
    [JsonIgnore]
    public Array<WorkGroupMedGroup> WorkGroupMed => workGroupMed ??= new(
      WorkGroupMedGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of WorkGroupMed for json serialization.
    /// </summary>
    [JsonPropertyName("workGroupMed")]
    [Computed]
    public IList<WorkGroupMedGroup> WorkGroupMed_Json
    {
      get => workGroupMed;
      set => WorkGroupMed.Assign(value);
    }

    /// <summary>
    /// Gets a value of WorkGroupPat.
    /// </summary>
    [JsonIgnore]
    public Array<WorkGroupPatGroup> WorkGroupPat => workGroupPat ??= new(
      WorkGroupPatGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of WorkGroupPat for json serialization.
    /// </summary>
    [JsonPropertyName("workGroupPat")]
    [Computed]
    public IList<WorkGroupPatGroup> WorkGroupPat_Json
    {
      get => workGroupPat;
      set => WorkGroupPat.Assign(value);
    }

    /// <summary>
    /// Gets a value of WorkGroupEst.
    /// </summary>
    [JsonIgnore]
    public Array<WorkGroupEstGroup> WorkGroupEst => workGroupEst ??= new(
      WorkGroupEstGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of WorkGroupEst for json serialization.
    /// </summary>
    [JsonPropertyName("workGroupEst")]
    [Computed]
    public IList<WorkGroupEstGroup> WorkGroupEst_Json
    {
      get => workGroupEst;
      set => WorkGroupEst.Assign(value);
    }

    /// <summary>
    /// Gets a value of WorkGroupEnf.
    /// </summary>
    [JsonIgnore]
    public Array<WorkGroupEnfGroup> WorkGroupEnf => workGroupEnf ??= new(
      WorkGroupEnfGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of WorkGroupEnf for json serialization.
    /// </summary>
    [JsonPropertyName("workGroupEnf")]
    [Computed]
    public IList<WorkGroupEnfGroup> WorkGroupEnf_Json
    {
      get => workGroupEnf;
      set => WorkGroupEnf.Assign(value);
    }

    /// <summary>
    /// A value of Work.
    /// </summary>
    [JsonPropertyName("work")]
    public NarrativeDetail Work
    {
      get => work ??= new();
      set => work = value;
    }

    /// <summary>
    /// A value of Text.
    /// </summary>
    [JsonPropertyName("text")]
    public WorkArea Text
    {
      get => text ??= new();
      set => text = value;
    }

    /// <summary>
    /// A value of ForUpdateAndCreate.
    /// </summary>
    [JsonPropertyName("forUpdateAndCreate")]
    public DateWorkArea ForUpdateAndCreate
    {
      get => forUpdateAndCreate ??= new();
      set => forUpdateAndCreate = value;
    }

    /// <summary>
    /// A value of Check.
    /// </summary>
    [JsonPropertyName("check")]
    public Common Check
    {
      get => check ??= new();
      set => check = value;
    }

    /// <summary>
    /// A value of ReviewOther.
    /// </summary>
    [JsonPropertyName("reviewOther")]
    public Infrastructure ReviewOther
    {
      get => reviewOther ??= new();
      set => reviewOther = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of Save.
    /// </summary>
    [JsonPropertyName("save")]
    public Common Save
    {
      get => save ??= new();
      set => save = value;
    }

    /// <summary>
    /// A value of ReviewType.
    /// </summary>
    [JsonPropertyName("reviewType")]
    public Infrastructure ReviewType
    {
      get => reviewType ??= new();
      set => reviewType = value;
    }

    /// <summary>
    /// A value of GetDateFromTimestamp.
    /// </summary>
    [JsonPropertyName("getDateFromTimestamp")]
    public DateWorkArea GetDateFromTimestamp
    {
      get => getDateFromTimestamp ??= new();
      set => getDateFromTimestamp = value;
    }

    /// <summary>
    /// A value of Date.
    /// </summary>
    [JsonPropertyName("date")]
    public TextWorkArea Date
    {
      get => date ??= new();
      set => date = value;
    }

    /// <summary>
    /// A value of ProgramCount.
    /// </summary>
    [JsonPropertyName("programCount")]
    public Common ProgramCount
    {
      get => programCount ??= new();
      set => programCount = value;
    }

    /// <summary>
    /// A value of Ap1.
    /// </summary>
    [JsonPropertyName("ap1")]
    public CsePerson Ap1
    {
      get => ap1 ??= new();
      set => ap1 = value;
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
    /// A value of CaseReviewCompleted.
    /// </summary>
    [JsonPropertyName("caseReviewCompleted")]
    public CsePersonsWorkSet CaseReviewCompleted
    {
      get => caseReviewCompleted ??= new();
      set => caseReviewCompleted = value;
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
    /// A value of Ap2.
    /// </summary>
    [JsonPropertyName("ap2")]
    public CsePerson Ap2
    {
      get => ap2 ??= new();
      set => ap2 = value;
    }

    /// <summary>
    /// A value of NoOfApOnCase.
    /// </summary>
    [JsonPropertyName("noOfApOnCase")]
    public Common NoOfApOnCase
    {
      get => noOfApOnCase ??= new();
      set => noOfApOnCase = value;
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
    /// A value of ExportHiddenPassed.
    /// </summary>
    [JsonPropertyName("exportHiddenPassed")]
    public Common ExportHiddenPassed
    {
      get => exportHiddenPassed ??= new();
      set => exportHiddenPassed = value;
    }

    /// <summary>
    /// A value of ExpModifiedNarrative.
    /// </summary>
    [JsonPropertyName("expModifiedNarrative")]
    public NarrativeWork ExpModifiedNarrative
    {
      get => expModifiedNarrative ??= new();
      set => expModifiedNarrative = value;
    }

    /// <summary>
    /// A value of NarrativeWork.
    /// </summary>
    [JsonPropertyName("narrativeWork")]
    public NarrativeWork NarrativeWork
    {
      get => narrativeWork ??= new();
      set => narrativeWork = value;
    }

    private Common infrastructureFound;
    private Document document;
    private SpDocKey spDocKey;
    private Common startingPosition;
    private Common length;
    private Array<WorkGroup1> work1;
    private Array<WorkGroup2> work2;
    private Array<WorkGroupMedGroup> workGroupMed;
    private Array<WorkGroupPatGroup> workGroupPat;
    private Array<WorkGroupEstGroup> workGroupEst;
    private Array<WorkGroupEnfGroup> workGroupEnf;
    private NarrativeDetail work;
    private WorkArea text;
    private DateWorkArea forUpdateAndCreate;
    private Common check;
    private Infrastructure reviewOther;
    private Common count;
    private DateWorkArea current;
    private Common save;
    private Infrastructure reviewType;
    private DateWorkArea getDateFromTimestamp;
    private TextWorkArea date;
    private Common programCount;
    private CsePerson ap1;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePersonsWorkSet caseReviewCompleted;
    private AbendData abendData;
    private CsePerson ap2;
    private Common noOfApOnCase;
    private Common invalid;
    private Common exportHiddenPassed;
    private NarrativeWork expModifiedNarrative;
    private NarrativeWork narrativeWork;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public NarrativeDetail New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public NarrativeDetail Existing
    {
      get => existing ??= new();
      set => existing = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    /// <summary>
    /// A value of Program1.
    /// </summary>
    [JsonPropertyName("program1")]
    public Program Program1
    {
      get => program1 ??= new();
      set => program1 = value;
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    private NarrativeDetail new1;
    private NarrativeDetail existing;
    private OfficeServiceProvider officeServiceProvider;
    private CaseAssignment caseAssignment;
    private Infrastructure infrastructure;
    private LegalAction legalAction;
    private Case1 case1;
    private Office office;
    private CsePerson csePerson;
    private CaseRole caseRole;
    private Program program1;
    private PersonProgram personProgram;
    private ServiceProvider serviceProvider;
  }
#endregion
}
