// Program: SP_CRPA_CASE_REVIEW_PATERNITY, ID: 372641087, model: 746.
// Short name: SWECRPAP
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
/// A program: SP_CRPA_CASE_REVIEW_PATERNITY.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpCrpaCaseReviewPaternity: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CRPA_CASE_REVIEW_PATERNITY program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCrpaCaseReviewPaternity(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCrpaCaseReviewPaternity.
  /// </summary>
  public SpCrpaCaseReviewPaternity(IContext context, Import import,
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
    //        M A I N T E N A N C E   L O G
    //  Date    Developer       req #   Description
    // 10/30/95 J Rookard		 Zdelete views
    // 03/14/96 Alan Hackler            Retro fits
    // 01/03/97 R. Marchman		 Add new security/next tran.
    // 01/10/97 R. Welborn		 Conversion from Plan Task Note
    // 				 to Infrastructure/Conversion.
    // 04/30/97 R. Grey                 Change Current Date
    // 06/01/97 R. Grey		 Modify dialog flow logic to display
    // 				 selected CSE Person
    // 07/16/97 R. Grey		 Display Children over 18 with msg
    // 07/22/97 R. Grey		 Add review closed case
    // 03/19/99 Sury G                  Changes done:
    //                                   
    // 1. Display AP's name and #No,
    // Message
    //                                     
    // to display if more than one AP
    // exist.
    //                                   
    // 2. Display Child's age in months(
    // if less than year)
    //                                   
    // 3. Father's Signature on birth
    // cert Ind.
    //                                   
    // 4. To display  error message, if
    // invalid PF key
    //                                    
    // pressed to flow to next tran and
    // neglect
    //                                   
    // next tran if valid PF key (other
    // than ENTER) pressed
    // ---------------------------------------------
    // ------------------------------------------------------------------
    // 02/03/2000          Vithal Madhira                      PR# 86247        
    // Modified code to implement the case review for each AP on a multiple AP
    // case.
    // 03/09/2000          Vithal Madhira                    WR# 000160
    // Paternity Indicator: Zdeleted all the related attributes and added new 
    // views/attributes and code to populate the Birth Fathers Lname, Fname,
    // Paternity_Estbl_Indicator, Birth_Certificate_Signature.
    // 08/08/2000 SWSRCHF WR# 000170  Replace the read for Narrative by a read 
    // for
    //                                
    // Narrative Detail
    // 05/08/2008 Arun Mathias  CQ#421 Automatic flow from CRPA, if the born out
    // of wedlock
    //                          or the CSE to establish indicators are set to "
    // U".
    // 06/02/2008 Arun Mathias CQ#421 If AP is female then Do not flow to CPAT.
    // 04/01/2011	T Pierce	CR# 23212	Removed references to obsolete Narrative 
    // entity type.
    // --------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);
    local.Current.Date = Now().Date;
    local.MaxDate.Date = UseCabSetMaximumDiscontinueDate();
    export.MoreApsMsg.Text30 = import.MoreApsMsg.Text30;
    export.HiddenPass1.SystemGeneratedIdentifier =
      import.HiddenPass1.SystemGeneratedIdentifier;
    export.HiddenPassedReviewType.ActionEntry =
      import.HiddenPassedReviewType.ActionEntry;
    export.CaseClosedIndicator.Flag = import.CaseClosedIndicator.Flag;
    export.ActivityLiteral.Text16 = import.ActivityLiteral.Text16;
    export.ServiceProcess.ServiceDate = import.ServiceProcess.ServiceDate;

    // ***CQ#421 Changes Begin Here ***
    export.FromCrpa.Flag = import.FromCrpa.Flag;
    export.UnknownPaternity.Flag = import.UnknownPaternity.Flag;

    // ***CQ#421 Changes End   Here ***
    export.ApSelected.Flag = import.ApSelected.Flag;
    export.MultiAp.Flag = import.MultiAp.Flag;
    MoveCsePersonsWorkSet3(import.SelectedAp, export.SelectedAp);

    for(import.HiddenPass.Index = 0; import.HiddenPass.Index < Import
      .HiddenPassGroup.Capacity; ++import.HiddenPass.Index)
    {
      if (!import.HiddenPass.CheckSize())
      {
        break;
      }

      export.HiddenPass.Index = import.HiddenPass.Index;
      export.HiddenPass.CheckSize();

      export.HiddenPass.Update.GexportH.Text =
        import.HiddenPass.Item.GimportH.Text;
      export.HiddenPass.Update.GexportHiddenPassed.Flag =
        import.HiddenPass.Item.GimportHiddenPassed.Flag;
    }

    import.HiddenPass.CheckIndex();
    export.Case1.Number = import.Case1.Number;
    export.Mo.Assign(import.Mo);
    MoveCsePersonsWorkSet3(import.Ap, export.Ap);
    MoveCsePersonsWorkSet3(import.Ar, export.Ar);
    export.LegalReferral.ReferralDate = import.LegalReferral.ReferralDate;
    export.MoreApsMsg.Text30 = import.MoreApsMsg.Text30;
    export.PatReview.Text = import.PatReview.Text;

    export.Child.Index = 0;
    export.Child.Clear();

    for(import.Child.Index = 0; import.Child.Index < import.Child.Count; ++
      import.Child.Index)
    {
      if (export.Child.IsFull)
      {
        break;
      }

      export.Child.Update.Compliance.Date = import.Child.Item.Compliance.Date;
      export.Child.Update.StartDate.Date = import.Child.Item.StartDate.Date;
      export.Child.Update.Common.SelectChar =
        import.Child.Item.Common.SelectChar;

      if (Equal(global.Command, "RETLINK") || Equal(global.Command, "DISPLAY"))
      {
        if (AsChar(export.Child.Item.Common.SelectChar) == '*' || AsChar
          (export.Child.Item.Common.SelectChar) == 'S')
        {
          export.Child.Update.Common.SelectChar = "";
        }
      }

      export.Child.Update.LegalReferral.ReferralDate =
        import.Child.Item.LegalReferral.ReferralDate;
      export.Child.Update.ChildCaseRole.Type1 =
        import.Child.Item.ChildCaseRole.Type1;
      export.Child.Update.ChildCsePersonsWorkSet.Assign(
        import.Child.Item.ChildCsePersonsWorkSet);
      export.Child.Update.ChildAge.Count = import.Child.Item.ChildAge.Count;
      MoveSpTextWorkArea(import.Child.Item.ChAgeMsgTxt,
        export.Child.Update.ChAgeMsgTxt);
      export.Child.Update.DaysRemainInCompli.Count =
        import.Child.Item.DaysRemainInCompli.Count;
      export.Child.Update.Child1.RelToAr = import.Child.Item.Child1.RelToAr;
      export.Child.Update.GeneticTest.Assign(import.Child.Item.GeneticTest);
      export.Child.Update.GeneticTestSchedule.Flag =
        import.Child.Item.GeneticTestSchedule.Flag;
      export.Child.Update.FedCompliance.Flag =
        import.Child.Item.FedCompliance.Flag;
      export.Child.Next();
    }

    if (Equal(import.HiddenPassedReviewType.ActionEntry, "R"))
    {
      var field = GetField(export.PatReview, "text");

      field.Color = "cyan";
      field.Protected = true;
    }
    else
    {
      var field = GetField(export.PatReview, "text");

      field.Color = "green";
      field.Highlighting = Highlighting.Underscore;
      field.Protected = false;
      field.Focused = true;
    }

    if (Equal(global.Command, "CLEAR") || Equal(global.Command, "RETLINK"))
    {
      return;
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else if (Equal(global.Command, "ENTER"))
    {
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

      return;
    }
    else if (Equal(global.Command, "INVALID"))
    {
      var field = GetField(export.Standard, "nextTransaction");

      field.Error = true;

      ExitState = "ACO_NE0000_INVALID_PF_KEY";

      return;
    }
    else
    {
      export.Standard.NextTransaction = "";
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ****
      // this is where you set your export value to the export hidden next tran 
      // values if the user is comming into this procedure on a next tran
      // action.
      // ****
      export.Case1.Number = export.Hidden.CaseNumber ?? Spaces(10);
      UseScCabNextTranGet();
      global.Command = "DISPLAY";

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      // ****
      // this is where you set your command to do whatever is necessary to do on
      // a flow from the menu, maybe just escape....
      // ****
      // You should get this information from the Dialog Flow Diagram.  It is 
      // the SEND CMD on the propertis for a Transfer from one  procedure to
      // another.
      // *** the statement would read COMMAND IS display   *****
      global.Command = "DISPLAY";

      return;
    }

    if (Equal(global.Command, "REDISP"))
    {
      if (!Equal(import.HiddenPassedReviewType.ActionEntry, "R"))
      {
        // *** Work request 000170
        // *** 08/08/00 SWSRCHF
        // *** start
        local.Work.Index = -1;

        foreach(var item in ReadNarrativeDetail1())
        {
          ++local.Work.Index;
          local.Work.CheckSize();

          local.Work.Update.Work1.NarrativeText =
            entities.PaternityReview.NarrativeText;

          if (local.Work.Index == 3)
          {
            break;
          }
        }

        if (!local.Work.IsEmpty)
        {
          local.Work.Index = 0;
          local.Work.CheckSize();

          export.PatReview.Text =
            Substring(local.Work.Item.Work1.NarrativeText, 14, 55);

          local.Work.Index = 1;
          local.Work.CheckSize();

          if (!IsEmpty(local.Work.Item.Work1.NarrativeText))
          {
            export.PatReview.Text = TrimEnd(export.PatReview.Text) + Substring
              (local.Work.Item.Work1.NarrativeText, 68, 14, 55);
          }

          local.Work.Index = 2;
          local.Work.CheckSize();

          if (!IsEmpty(local.Work.Item.Work1.NarrativeText))
          {
            export.PatReview.Text = TrimEnd(export.PatReview.Text) + Substring
              (local.Work.Item.Work1.NarrativeText, 68, 14, 55);
          }

          local.Work.Index = 3;
          local.Work.CheckSize();

          if (!IsEmpty(local.Work.Item.Work1.NarrativeText))
          {
            export.PatReview.Text = TrimEnd(export.PatReview.Text) + Substring
              (local.Work.Item.Work1.NarrativeText, 68, 14, 55);
          }

          export.HiddenPass.Index = 2;
          export.HiddenPass.CheckSize();

          export.HiddenPass.Update.GexportH.Text = export.PatReview.Text;
        }

        // *** end
        // *** 08/08/00 SWSRCHF
        // *** Work request 000170
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "CHDS") || Equal(global.Command, "GTDS") || Equal
      (global.Command, "GTSC") || Equal(global.Command, "LGRQ"))
    {
    }
    else
    {
      // to validate action level security
      if (Equal(global.Command, "DISPLAY"))
      {
        UseScCabTestSecurity();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          return;
        }
      }
    }

    if (Equal(global.Command, "CHDS") || Equal(global.Command, "GTDS") || Equal
      (global.Command, "GTSC") || Equal(global.Command, "LGRQ") || Equal
      (global.Command, "CPAT"))
    {
      if (Equal(global.Command, "CHDS") || Equal(global.Command, "GTSC") || Equal
        (global.Command, "GTDS"))
      {
        for(export.Child.Index = 0; export.Child.Index < export.Child.Count; ++
          export.Child.Index)
        {
          if (!IsEmpty(export.Child.Item.Common.SelectChar))
          {
            if (AsChar(export.Child.Item.Common.SelectChar) == 'S')
            {
              export.SelectedChildCsePerson.Number =
                export.Child.Item.ChildCsePersonsWorkSet.Number;
              export.SelectedChildCsePersonsWorkSet.Number =
                export.Child.Item.ChildCsePersonsWorkSet.Number;
              export.Child.Update.Common.SelectChar = "";

              if (Equal(global.Command, "GTSC") || Equal
                (global.Command, "GTDS"))
              {
                if (ReadGeneticTest1())
                {
                  if (ReadAbsentParentMother())
                  {
                    if (ReadCsePerson3())
                    {
                      export.SelectedAllegFather.Number =
                        entities.CsePerson.Number;
                    }

                    if (ReadCsePerson1())
                    {
                      export.SelectedMotherCsePersonsWorkSet.Number =
                        entities.CsePerson.Number;
                      export.SelectedMotherCsePerson.Number =
                        entities.CsePerson.Number;
                    }
                  }
                  else if (ReadFatherMotherChild())
                  {
                    if (ReadCsePerson2())
                    {
                      export.SelectedAllegFather.Number =
                        entities.CsePerson.Number;
                    }

                    if (ReadCsePerson1())
                    {
                      export.SelectedMotherCsePersonsWorkSet.Number =
                        entities.CsePerson.Number;
                      export.SelectedMotherCsePerson.Number =
                        entities.CsePerson.Number;
                    }
                  }
                }
                else
                {
                  var field = GetField(export.Child.Item.Common, "selectChar");

                  field.Error = true;

                  ExitState = "SP0000_NO_GENETIC_TEST_FOR_CHILD";

                  return;
                }
              }

              goto Test1;
            }
            else
            {
              ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

              var field = GetField(export.Child.Item.Common, "selectChar");

              field.Error = true;

              return;
            }
          }
        }

        ExitState = "ACO_NE0000_NO_SELECTION_MADE";

        return;
      }
      else if (Equal(global.Command, "CPAT"))
      {
        ExitState = "ECO_LNK_TO_CPAT";
      }
      else
      {
        ExitState = "ECO_LNK_TO_LEGAL_REQUEST";
      }

Test1:

      if (Equal(global.Command, "CHDS"))
      {
        ExitState = "ECO_LNK_TO_CHDS";
      }
      else if (Equal(global.Command, "GTDS"))
      {
        ExitState = "ECO_LNK_TO_GENETIC_TEST_DETAIL";
      }
      else if (Equal(global.Command, "GTSC"))
      {
        ExitState = "ECO_LNK_TO_GENETIC_TEST_SCHEDULE";
      }

      return;
    }

    switch(TrimEnd(global.Command))
    {
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "RETURN":
        if (!Equal(import.HiddenPassedReviewType.ActionEntry, "R"))
        {
          export.HiddenPass.Index = 2;
          export.HiddenPass.CheckSize();

          export.HiddenPass.Update.GexportH.Text = export.PatReview.Text;
        }

        ExitState = "ACO_NE0000_RETURN";

        break;
      case "DISPLAY":
        if (AsChar(export.CaseClosedIndicator.Flag) == 'Y')
        {
          UseSpCrpaDisplayClosedCase();

          break;
        }

        if (!ReadCase())
        {
          ExitState = "CASE_NF";

          break;
        }

        // ***************************************************************
        // * Display Mother Details
        // ***************************************************************
        if (ReadCsePersonMother())
        {
          local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
          UseSiReadCsePerson();

          if (!IsEmpty(local.AbendData.Type1))
          {
            return;
          }

          MoveCsePersonsWorkSet2(local.CsePersonsWorkSet, export.Mo);
        }

        // ***************************************************************
        // * Display AR Details
        // ***************************************************************
        if (ReadCsePersonApplicantRecipient())
        {
          local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
          UseSiReadCsePerson();

          if (!IsEmpty(local.AbendData.Type1))
          {
            return;
          }

          MoveCsePersonsWorkSet3(local.CsePersonsWorkSet, export.Ar);
        }

        // ***************************************************************
        // * Determine more than more than one AP exist and Display
        // * AP Details
        // ***************************************************************
        // ------------------------------------------------------------------
        // 02/03/2000          Vithal Madhira                   PR# 86247
        // Modified code to implement the case review for each AP on a
        // multiple AP case.
        // --------------------------------------------------------------------
        if (!IsEmpty(export.SelectedAp.Number) && AsChar
          (export.ApSelected.Flag) == 'Y')
        {
          MoveCsePersonsWorkSet3(export.SelectedAp, export.Ap);

          if (AsChar(export.MultiAp.Flag) == 'Y')
          {
            export.MoreApsMsg.Text30 = "More AP's exist for this case";
          }
        }
        else
        {
          local.Count.Count = 0;
          export.Ap.FormattedName = "";

          foreach(var item in ReadCsePersonAbsentParent2())
          {
            ++local.Count.Count;

            if (local.Count.Count > 0)
            {
              if (IsEmpty(export.Ap.FormattedName))
              {
                local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
                UseSiReadCsePerson();

                if (!IsEmpty(local.AbendData.Type1))
                {
                  return;
                }

                MoveCsePersonsWorkSet3(local.CsePersonsWorkSet, export.Ap);
              }
              else
              {
                export.MoreApsMsg.Text30 = "More AP's exist for this case";

                break;
              }
            }
          }
        }

        // *** CQ#421 If female is an AP then do not flow to CPAT ***
        // *** Changes Begin Here ***
        if (Equal(import.HiddenPassedReviewType.ActionEntry, "P"))
        {
          local.FemaleAp.Flag = "N";

          foreach(var item in ReadCsePersonAbsentParent1())
          {
            local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
            UseSiReadCsePerson();

            if (!IsEmpty(local.AbendData.Type1))
            {
              return;
            }

            if (AsChar(local.CsePersonsWorkSet.Sex) == 'F')
            {
              local.FemaleAp.Flag = "Y";

              break;
            }
          }
        }

        // *** Changes End Here ***
        if (ReadLegalReferralLegalReferralCaseRoleCaseRole())
        {
          export.LegalReferral.ReferralDate =
            entities.LegalReferral.ReferralDate;
        }

        if (IsEmpty(export.MoreApsMsg.Text30))
        {
          if (ReadServiceProcessLegalActionLegalActionPerson2())
          {
            export.ServiceProcess.ServiceDate =
              entities.ServiceProcess.ServiceDate;
          }
        }
        else if (ReadServiceProcessLegalActionLegalActionPerson1())
        {
          export.ServiceProcess.ServiceDate =
            entities.ServiceProcess.ServiceDate;
        }

        // ***CQ#421 Check only for annual review process whether Born out of 
        // wedlock or CSE to Establish Paternity indicator is unknown
        // ***CQ#421 Changes Start Here ***
        if (Equal(import.HiddenPassedReviewType.ActionEntry, "P"))
        {
          export.UnknownPaternity.Flag = "";

          if (AsChar(local.FemaleAp.Flag) == 'N')
          {
            if (ReadCsePerson4())
            {
              export.UnknownPaternity.Flag = "Y";
            }
          }
        }

        // ***CQ#421 Changes End Here   ***
        export.Child.Index = 0;
        export.Child.Clear();

        foreach(var item in ReadChild())
        {
          export.Child.Update.Child1.RelToAr = entities.Child1.RelToAr;

          if (ReadCsePerson5())
          {
            export.Child.Update.ChildCsePerson.Assign(entities.Child2);
            local.CsePersonsWorkSet.Number = entities.Child2.Number;
            UseSiReadCsePerson();

            if (!IsEmpty(local.AbendData.Type1))
            {
              export.Child.Next();

              return;
            }

            if (Equal(local.CsePersonsWorkSet.Dob, local.Initialized.Date) || Equal
              (local.CsePersonsWorkSet.Dob, local.MaxDate.Date))
            {
              export.Child.Update.ChildAge.Count = 0;
              export.Child.Update.ChAgeMsgTxt.Text80 =
                "Date of birth not identified.";
            }
            else
            {
              // *********************************************************
              // * Determine Child's Age
              // *********************************************************
              local.ChAge.TotalInteger =
                (long)DaysFromAD(local.Current.Date) - DaysFromAD
                (local.CsePersonsWorkSet.Dob);

              if (local.ChAge.TotalInteger < 365)
              {
                export.Child.Update.ChildAge.Count =
                  (int)Math.Round(
                    local.ChAge.TotalInteger /
                  30.6M, MidpointRounding.AwayFromZero);
                export.Child.Update.ChAgeMsgTxt.Text4 = "Mths";
              }
              else
              {
                UseCabCalcCurrentAgeFromDob();
                export.Child.Update.ChAgeMsgTxt.Text4 = "Yrs";
                export.Child.Update.ChildAge.Count =
                  (int)export.LocalReturnedAge.TotalInteger;
              }
            }

            export.Child.Update.ChildCsePersonsWorkSet.Assign(
              local.CsePersonsWorkSet);
            export.ActivityLiteral.Text16 = "Required Cmpt Dt";

            if (ReadMonitoredActivity())
            {
              export.Child.Update.Compliance.Date =
                entities.MonitoredActivity.FedNonComplianceDate;
              export.Child.Update.StartDate.Date =
                entities.MonitoredActivity.StartDate;

              if (Equal(entities.MonitoredActivity.ClosureDate,
                local.Initialized.Date) || Equal
                (entities.MonitoredActivity.ClosureDate, local.MaxDate.Date))
              {
                // Indicates that the monitored activity has not been closed,
                // therefore AP has not been located.
                if (!Lt(local.Current.Date,
                  entities.MonitoredActivity.FedNonComplianceDate))
                {
                  export.Child.Update.FedCompliance.Flag = "N";
                  export.ActivityLiteral.Text16 = "Required Cmpt Dt";
                  export.Child.Update.DaysRemainInCompli.Count = 0;
                }
                else if (Lt(local.Current.Date,
                  entities.MonitoredActivity.FedNonComplianceDate))
                {
                  export.Child.Update.FedCompliance.Flag = "";
                  export.ActivityLiteral.Text16 = "Required Cmpt Dt";
                  export.Child.Update.DaysRemainInCompli.Count =
                    DaysFromAD(entities.MonitoredActivity.FedNonComplianceDate) -
                    DaysFromAD(local.Current.Date);
                }
              }
              else
              {
                // Indicates monitored activity has been closed, and other 
                // fields populated
                //  (ap address, etc.) indicate whether or not he's been found.
                // Therefore,
                //  compliance indicator is set based on whether the monitored 
                // activity was
                //  closed on or before compliance date.
                if (!Lt(entities.MonitoredActivity.FedNonComplianceDate,
                  entities.MonitoredActivity.ClosureDate))
                {
                  export.Child.Update.FedCompliance.Flag = "Y";
                  export.ActivityLiteral.Text16 = " Activity Closed";
                  export.Child.Update.Compliance.Date =
                    entities.MonitoredActivity.ClosureDate;
                }
                else
                {
                  export.Child.Update.FedCompliance.Flag = "N";
                  export.ActivityLiteral.Text16 = " Activity Closed";
                  export.Child.Update.Compliance.Date =
                    entities.MonitoredActivity.ClosureDate;
                }

                export.Child.Update.DaysRemainInCompli.Count = 0;
              }
            }
          }

          export.Child.Update.GeneticTestSchedule.Flag = "";

          foreach(var item1 in ReadGeneticTest2())
          {
            export.Child.Update.GeneticTestSchedule.Flag = "Y";

            if (ReadMother())
            {
              if (ReadAbsentParent())
              {
                MoveGeneticTest(entities.GeneticTest,
                  export.Child.Update.GeneticTest);

                break;
              }
              else if (ReadFather())
              {
                MoveGeneticTest(entities.GeneticTest,
                  export.Child.Update.GeneticTest);

                break;
              }
            }
          }

          if (!Equal(export.Child.Item.GeneticTest.ActualTestDate, null) || !
            Equal(export.Child.Item.GeneticTest.TestResultReceivedDate, null))
          {
            export.Child.Update.GeneticTestSchedule.Flag = "";
          }

          export.Child.Next();
        }

        if (Equal(import.HiddenPassedReviewType.ActionEntry, "R"))
        {
          // *** Work request 000170
          // *** 08/08/00 SWSRCHF
          // *** start
          local.Work.Index = -1;

          foreach(var item in ReadNarrativeDetail2())
          {
            ++local.Work.Index;
            local.Work.CheckSize();

            local.Work.Update.Work1.NarrativeText =
              entities.PaternityReview.NarrativeText;

            if (local.Work.Index == 3)
            {
              break;
            }
          }

          if (!local.Work.IsEmpty)
          {
            local.Work.Index = 0;
            local.Work.CheckSize();

            export.PatReview.Text =
              Substring(local.Work.Item.Work1.NarrativeText, 14, 55);

            local.Work.Index = 1;
            local.Work.CheckSize();

            if (!IsEmpty(local.Work.Item.Work1.NarrativeText))
            {
              export.PatReview.Text = TrimEnd(export.PatReview.Text) + Substring
                (local.Work.Item.Work1.NarrativeText, 68, 14, 55);
            }

            local.Work.Index = 2;
            local.Work.CheckSize();

            if (!IsEmpty(local.Work.Item.Work1.NarrativeText))
            {
              export.PatReview.Text = TrimEnd(export.PatReview.Text) + Substring
                (local.Work.Item.Work1.NarrativeText, 68, 14, 55);
            }

            local.Work.Index = 3;
            local.Work.CheckSize();

            if (!IsEmpty(local.Work.Item.Work1.NarrativeText))
            {
              export.PatReview.Text = TrimEnd(export.PatReview.Text) + Substring
                (local.Work.Item.Work1.NarrativeText, 68, 14, 55);
            }

            var field = GetField(export.PatReview, "text");

            field.Color = "cyan";
            field.Protected = true;
          }

          // *** end
          // *** 08/08/00 SWSRCHF
          // *** Work request 000170
        }
        else
        {
          export.HiddenPass.Index = 2;
          export.HiddenPass.CheckSize();

          export.PatReview.Text = export.HiddenPass.Item.GexportH.Text;
        }

        if (IsEmpty(export.PatReview.Text))
        {
          ExitState = "SP0000_FIRST_REVIEW_4_CASE";
        }
        else if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }

        break;
      case "ENTER":
        // **CQ#421 Below code is commented out and moved below just to avoid 
        // duplication of code
        // ***CQ#421 Changes Begin Here ***
        // ***CQ#421 Changes End  Here ***
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

    // ***CQ#421 The below main IF condition was just to avoid duplication of 
    // code in command ENTER and DISPLAY
    // *** CQ#421 Changes Begin Here ***
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "ENTER"))
    {
      if (Equal(global.Command, "DISPLAY"))
      {
        // ***It had already been to CPAT because of unknown paternity, so now 
        // send it to the CRES screen.
        // ***This flag is set only if it is annual case review and has unknown 
        // paternity.
        if (AsChar(export.FromCrpa.Flag) == 'Y')
        {
          goto Test2;
        }

        goto Test3;
      }

Test2:

      // *** Moved the below IF condition from command ENTER just to avoid 
      // duplication of code
      if (Equal(import.HiddenPassedReviewType.ActionEntry, "M") || Equal
        (import.HiddenPassedReviewType.ActionEntry, "P"))
      {
        if (IsEmpty(export.PatReview.Text))
        {
          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          var field = GetField(export.PatReview, "text");

          field.Error = true;

          goto Test3;
        }
        else
        {
          export.HiddenPass.Index = 2;
          export.HiddenPass.CheckSize();

          export.HiddenPass.Update.GexportH.Text = export.PatReview.Text;
        }
      }

      if (Equal(global.Command, "ENTER"))
      {
        // *** Automatic flow to CPAT only if it is annual review and has 
        // unknown paternity
        if (Equal(import.HiddenPassedReviewType.ActionEntry, "P"))
        {
          if (AsChar(export.UnknownPaternity.Flag) == 'Y')
          {
            export.FromCrpa.Flag = "Y";
            ExitState = "ECO_LNK_TO_CPAT";

            return;
          }
        }
      }

      // ***Send it to the CRES screen and Initialize the flag so that we know 
      // that if the user goes back
      // ***then he has to go through the CPAT screen again, if the paternity is
      // unknown
      export.FromCrpa.Flag = "";
      ExitState = "ECO_LNK_TO_CR_ESTABLISHMENT";

      return;
    }

Test3:

    // *** CQ#421 Changes End   Here ***
    if (AsChar(export.CaseClosedIndicator.Flag) == 'Y')
    {
      var field = GetField(export.PatReview, "text");

      field.Color = "cyan";
      field.Protected = true;
    }
  }

  private static void MoveCaseRole(CaseRole source, CaseRole target)
  {
    target.Type1 = source.Type1;
    target.ParentType = source.ParentType;
  }

  private static void MoveChild(SpCrpaDisplayClosedCase.Export.
    ChildGroup source, Export.ChildGroup target)
  {
    target.Compliance.Date = source.Compliance.Date;
    target.Common.SelectChar = source.Common.SelectChar;
    target.Child1.RelToAr = source.Child1.RelToAr;
    target.GeneticTest.Assign(source.GeneticTest);
    target.LegalReferral.ReferralDate = source.LegalReferral.ReferralDate;
    target.StartDate.Date = source.StartDate.Date;
    target.ReferredToLegal.Flag = source.ReferredToLegal.Flag;
    target.FedCompliance.Flag = source.FedCompliance.Flag;
    target.ChildAge.Count = source.ChildAge.Count;
    target.DaysRemainInCompli.Count = source.DaysRemainInCompli.Count;
    MoveCaseRole(source.Ap, target.Ap);
    target.ChildCaseRole.Type1 = source.ChildCaseRole.Type1;
    target.ChildCsePersonsWorkSet.Assign(source.ChildCsePersonsWorkSet);
    MoveSpTextWorkArea(source.ChAgeMsgTxt, target.ChAgeMsgTxt);
    target.GeneticTestSchedule.Flag = source.GenTestFlag.Flag;
    target.ChildCsePerson.Assign(source.ChildCsePerson);
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
    target.Dob = source.Dob;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveCsePersonsWorkSet3(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveGeneticTest(GeneticTest source, GeneticTest target)
  {
    target.TestNumber = source.TestNumber;
    target.ActualTestDate = source.ActualTestDate;
    target.TestResultReceivedDate = source.TestResultReceivedDate;
    target.LastUpdatedBy = source.LastUpdatedBy;
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

  private static void MoveSpTextWorkArea(SpTextWorkArea source,
    SpTextWorkArea target)
  {
    target.Text80 = source.Text80;
    target.Text4 = source.Text4;
  }

  private void UseCabCalcCurrentAgeFromDob()
  {
    var useImport = new CabCalcCurrentAgeFromDob.Import();
    var useExport = new CabCalcCurrentAgeFromDob.Export();

    useImport.CsePersonsWorkSet.Dob = local.CsePersonsWorkSet.Dob;

    Call(CabCalcCurrentAgeFromDob.Execute, useImport, useExport);

    export.LocalReturnedAge.TotalInteger = useExport.Common.TotalInteger;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.MaxDate.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
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

    useImport.CsePersonsWorkSet.Number = import.Mo.Number;
    useImport.Case1.Number = import.Case1.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    MoveCsePersonsWorkSet1(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
      
  }

  private void UseSpCrpaDisplayClosedCase()
  {
    var useImport = new SpCrpaDisplayClosedCase.Import();
    var useExport = new SpCrpaDisplayClosedCase.Export();

    useImport.Case1.Number = export.Case1.Number;
    useImport.HiddenPassedReviewType.ActionEntry =
      export.HiddenPassedReviewType.ActionEntry;
    useImport.HiddenPass1.SystemGeneratedIdentifier =
      export.HiddenPass1.SystemGeneratedIdentifier;

    Call(SpCrpaDisplayClosedCase.Execute, useImport, useExport);

    useExport.Child.CopyTo(export.Child, MoveChild);
    export.Mo.Assign(useExport.Mo);
    MoveCsePersonsWorkSet3(useExport.Ar, export.Ar);
    MoveCsePersonsWorkSet3(useExport.Ap, export.Ap);
    export.LegalReferral.ReferralDate = useExport.LegalReferral.ReferralDate;
    export.ServiceProcess.ServiceDate = useExport.ServiceProcess.ServiceDate;
    export.ActivityLiteral.Text16 = useExport.ActivityLiteral.Text16;
    export.MoreApsMsg.Text30 = useExport.MoreApsMsg.Text30;
    export.PatReview.Text = useExport.PatReview.Text;
  }

  private bool ReadAbsentParent()
  {
    System.Diagnostics.Debug.Assert(entities.GeneticTest.Populated);
    entities.AllegedFather.Populated = false;

    return Read("ReadAbsentParent",
      (db, command) =>
      {
        db.SetString(command, "casNumber1", entities.Case1.Number);
        db.SetString(command, "cspNumber1", export.Ap.Number);
        db.SetInt32(
          command, "caseRoleId",
          entities.GeneticTest.CroAIdentifier.GetValueOrDefault());
        db.SetString(command, "type", entities.GeneticTest.CroAType ?? "");
        db.SetString(
          command, "casNumber2", entities.GeneticTest.CasANumber ?? "");
        db.SetString(
          command, "cspNumber2", entities.GeneticTest.CspANumber ?? "");
      },
      (db, reader) =>
      {
        entities.AllegedFather.CasNumber = db.GetString(reader, 0);
        entities.AllegedFather.CspNumber = db.GetString(reader, 1);
        entities.AllegedFather.Type1 = db.GetString(reader, 2);
        entities.AllegedFather.Identifier = db.GetInt32(reader, 3);
        entities.AllegedFather.EndDate = db.GetNullableDate(reader, 4);
        entities.AllegedFather.Populated = true;
        CheckValid<CaseRole>("Type1", entities.AllegedFather.Type1);
      });
  }

  private bool ReadAbsentParentMother()
  {
    System.Diagnostics.Debug.Assert(entities.GeneticTest.Populated);
    entities.AllegedFather.Populated = false;
    entities.Mother.Populated = false;

    return Read("ReadAbsentParentMother",
      (db, command) =>
      {
        db.SetInt32(
          command, "caseRoleId1",
          entities.GeneticTest.CroMIdentifier.GetValueOrDefault());
        db.SetString(command, "type1", entities.GeneticTest.CroMType ?? "");
        db.SetString(
          command, "casNumber1", entities.GeneticTest.CasMNumber ?? "");
        db.SetString(
          command, "cspNumber1", entities.GeneticTest.CspMNumber ?? "");
        db.SetInt32(
          command, "caseRoleId2",
          entities.GeneticTest.CroAIdentifier.GetValueOrDefault());
        db.SetString(command, "type2", entities.GeneticTest.CroAType ?? "");
        db.SetString(
          command, "casNumber2", entities.GeneticTest.CasANumber ?? "");
        db.SetString(
          command, "cspNumber2", entities.GeneticTest.CspANumber ?? "");
      },
      (db, reader) =>
      {
        entities.AllegedFather.CasNumber = db.GetString(reader, 0);
        entities.AllegedFather.CspNumber = db.GetString(reader, 1);
        entities.AllegedFather.Type1 = db.GetString(reader, 2);
        entities.AllegedFather.Identifier = db.GetInt32(reader, 3);
        entities.AllegedFather.EndDate = db.GetNullableDate(reader, 4);
        entities.Mother.CasNumber = db.GetString(reader, 5);
        entities.Mother.CspNumber = db.GetString(reader, 6);
        entities.Mother.Type1 = db.GetString(reader, 7);
        entities.Mother.Identifier = db.GetInt32(reader, 8);
        entities.Mother.StartDate = db.GetNullableDate(reader, 9);
        entities.Mother.EndDate = db.GetNullableDate(reader, 10);
        entities.AllegedFather.Populated = true;
        entities.Mother.Populated = true;
        CheckValid<CaseRole>("Type1", entities.AllegedFather.Type1);
        CheckValid<CaseRole>("Type1", entities.Mother.Type1);
      });
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
        entities.Case1.Populated = true;
      });
  }

  private IEnumerable<bool> ReadChild()
  {
    return ReadEach("ReadChild",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Child.IsFull)
        {
          return false;
        }

        entities.Child1.CasNumber = db.GetString(reader, 0);
        entities.Child1.CspNumber = db.GetString(reader, 1);
        entities.Child1.Type1 = db.GetString(reader, 2);
        entities.Child1.Identifier = db.GetInt32(reader, 3);
        entities.Child1.StartDate = db.GetNullableDate(reader, 4);
        entities.Child1.EndDate = db.GetNullableDate(reader, 5);
        entities.Child1.HealthInsuranceIndicator =
          db.GetNullableString(reader, 6);
        entities.Child1.MedicalSupportIndicator =
          db.GetNullableString(reader, 7);
        entities.Child1.ArWaivedInsurance = db.GetNullableString(reader, 8);
        entities.Child1.RelToAr = db.GetNullableString(reader, 9);
        entities.Child1.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Child1.Type1);

        return true;
      });
  }

  private bool ReadCsePerson1()
  {
    System.Diagnostics.Debug.Assert(entities.Mother.Populated);
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb1", entities.Mother.CspNumber);
        db.SetString(command, "numb2", export.Mo.Number);
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
    System.Diagnostics.Debug.Assert(entities.Father.Populated);
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb1", entities.Father.CspNumber);
        db.SetString(command, "numb2", export.Ap.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePerson3()
  {
    System.Diagnostics.Debug.Assert(entities.AllegedFather.Populated);
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson3",
      (db, command) =>
      {
        db.SetString(command, "numb1", entities.AllegedFather.CspNumber);
        db.SetString(command, "numb2", export.Ap.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePerson4()
  {
    entities.Child2.Populated = false;

    return Read("ReadCsePerson4",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Child2.Number = db.GetString(reader, 0);
        entities.Child2.Type1 = db.GetString(reader, 1);
        entities.Child2.BornOutOfWedlock = db.GetNullableString(reader, 2);
        entities.Child2.CseToEstblPaternity = db.GetNullableString(reader, 3);
        entities.Child2.PaternityEstablishedIndicator =
          db.GetNullableString(reader, 4);
        entities.Child2.BirthCertFathersLastName =
          db.GetNullableString(reader, 5);
        entities.Child2.BirthCertFathersFirstName =
          db.GetNullableString(reader, 6);
        entities.Child2.BirthCertificateSignature =
          db.GetNullableString(reader, 7);
        entities.Child2.Populated = true;
        CheckValid<CsePerson>("Type1", entities.Child2.Type1);
      });
  }

  private bool ReadCsePerson5()
  {
    System.Diagnostics.Debug.Assert(entities.Child1.Populated);
    entities.Child2.Populated = false;

    return Read("ReadCsePerson5",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.Child1.CspNumber);
      },
      (db, reader) =>
      {
        entities.Child2.Number = db.GetString(reader, 0);
        entities.Child2.Type1 = db.GetString(reader, 1);
        entities.Child2.BornOutOfWedlock = db.GetNullableString(reader, 2);
        entities.Child2.CseToEstblPaternity = db.GetNullableString(reader, 3);
        entities.Child2.PaternityEstablishedIndicator =
          db.GetNullableString(reader, 4);
        entities.Child2.BirthCertFathersLastName =
          db.GetNullableString(reader, 5);
        entities.Child2.BirthCertFathersFirstName =
          db.GetNullableString(reader, 6);
        entities.Child2.BirthCertificateSignature =
          db.GetNullableString(reader, 7);
        entities.Child2.Populated = true;
        CheckValid<CsePerson>("Type1", entities.Child2.Type1);
      });
  }

  private IEnumerable<bool> ReadCsePersonAbsentParent1()
  {
    entities.AbsentParent.Populated = false;
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePersonAbsentParent1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.AbsentParent.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.AbsentParent.CasNumber = db.GetString(reader, 2);
        entities.AbsentParent.Type1 = db.GetString(reader, 3);
        entities.AbsentParent.Identifier = db.GetInt32(reader, 4);
        entities.AbsentParent.StartDate = db.GetNullableDate(reader, 5);
        entities.AbsentParent.EndDate = db.GetNullableDate(reader, 6);
        entities.AbsentParent.CreatedTimestamp = db.GetDateTime(reader, 7);
        entities.AbsentParent.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
        CheckValid<CaseRole>("Type1", entities.AbsentParent.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePersonAbsentParent2()
  {
    entities.AbsentParent.Populated = false;
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePersonAbsentParent2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.AbsentParent.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.AbsentParent.CasNumber = db.GetString(reader, 2);
        entities.AbsentParent.Type1 = db.GetString(reader, 3);
        entities.AbsentParent.Identifier = db.GetInt32(reader, 4);
        entities.AbsentParent.StartDate = db.GetNullableDate(reader, 5);
        entities.AbsentParent.EndDate = db.GetNullableDate(reader, 6);
        entities.AbsentParent.CreatedTimestamp = db.GetDateTime(reader, 7);
        entities.AbsentParent.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
        CheckValid<CaseRole>("Type1", entities.AbsentParent.Type1);

        return true;
      });
  }

  private bool ReadCsePersonApplicantRecipient()
  {
    entities.CsePerson.Populated = false;
    entities.ApplicantRecipient.Populated = false;

    return Read("ReadCsePersonApplicantRecipient",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.ApplicantRecipient.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.ApplicantRecipient.CasNumber = db.GetString(reader, 2);
        entities.ApplicantRecipient.Type1 = db.GetString(reader, 3);
        entities.ApplicantRecipient.Identifier = db.GetInt32(reader, 4);
        entities.ApplicantRecipient.StartDate = db.GetNullableDate(reader, 5);
        entities.ApplicantRecipient.EndDate = db.GetNullableDate(reader, 6);
        entities.ApplicantRecipient.AssignmentDate =
          db.GetNullableDate(reader, 7);
        entities.CsePerson.Populated = true;
        entities.ApplicantRecipient.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
        CheckValid<CaseRole>("Type1", entities.ApplicantRecipient.Type1);
      });
  }

  private bool ReadCsePersonMother()
  {
    entities.Mother.Populated = false;
    entities.CsePerson.Populated = false;

    return Read("ReadCsePersonMother",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.Mother.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.Mother.CasNumber = db.GetString(reader, 2);
        entities.Mother.Type1 = db.GetString(reader, 3);
        entities.Mother.Identifier = db.GetInt32(reader, 4);
        entities.Mother.StartDate = db.GetNullableDate(reader, 5);
        entities.Mother.EndDate = db.GetNullableDate(reader, 6);
        entities.Mother.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
        CheckValid<CaseRole>("Type1", entities.Mother.Type1);
      });
  }

  private bool ReadFather()
  {
    entities.Father.Populated = false;

    return Read("ReadFather",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "cspNumber", export.Ap.Number);
      },
      (db, reader) =>
      {
        entities.Father.CasNumber = db.GetString(reader, 0);
        entities.Father.CspNumber = db.GetString(reader, 1);
        entities.Father.Type1 = db.GetString(reader, 2);
        entities.Father.Identifier = db.GetInt32(reader, 3);
        entities.Father.EndDate = db.GetNullableDate(reader, 4);
        entities.Father.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Father.Type1);
      });
  }

  private bool ReadFatherMotherChild()
  {
    System.Diagnostics.Debug.Assert(entities.GeneticTest.Populated);
    entities.Mother.Populated = false;
    entities.Father.Populated = false;
    entities.Child1.Populated = false;

    return Read("ReadFatherMotherChild",
      (db, command) =>
      {
        db.SetInt32(
          command, "caseRoleId1",
          entities.GeneticTest.CroIdentifier.GetValueOrDefault());
        db.SetString(command, "type1", entities.GeneticTest.CroType ?? "");
        db.
          SetString(command, "casNumber1", entities.GeneticTest.CasNumber ?? "");
          
        db.
          SetString(command, "cspNumber1", entities.GeneticTest.CspNumber ?? "");
          
        db.SetInt32(
          command, "caseRoleId2",
          entities.GeneticTest.CroMIdentifier.GetValueOrDefault());
        db.SetString(command, "type2", entities.GeneticTest.CroMType ?? "");
        db.SetString(
          command, "casNumber2", entities.GeneticTest.CasMNumber ?? "");
        db.SetString(
          command, "cspNumber2", entities.GeneticTest.CspMNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Father.CasNumber = db.GetString(reader, 0);
        entities.Father.CspNumber = db.GetString(reader, 1);
        entities.Father.Type1 = db.GetString(reader, 2);
        entities.Father.Identifier = db.GetInt32(reader, 3);
        entities.Father.EndDate = db.GetNullableDate(reader, 4);
        entities.Mother.CasNumber = db.GetString(reader, 5);
        entities.Mother.CspNumber = db.GetString(reader, 6);
        entities.Mother.Type1 = db.GetString(reader, 7);
        entities.Mother.Identifier = db.GetInt32(reader, 8);
        entities.Mother.StartDate = db.GetNullableDate(reader, 9);
        entities.Mother.EndDate = db.GetNullableDate(reader, 10);
        entities.Child1.CasNumber = db.GetString(reader, 11);
        entities.Child1.CspNumber = db.GetString(reader, 12);
        entities.Child1.Type1 = db.GetString(reader, 13);
        entities.Child1.Identifier = db.GetInt32(reader, 14);
        entities.Child1.StartDate = db.GetNullableDate(reader, 15);
        entities.Child1.EndDate = db.GetNullableDate(reader, 16);
        entities.Child1.HealthInsuranceIndicator =
          db.GetNullableString(reader, 17);
        entities.Child1.MedicalSupportIndicator =
          db.GetNullableString(reader, 18);
        entities.Child1.ArWaivedInsurance = db.GetNullableString(reader, 19);
        entities.Child1.RelToAr = db.GetNullableString(reader, 20);
        entities.Mother.Populated = true;
        entities.Father.Populated = true;
        entities.Child1.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Father.Type1);
        CheckValid<CaseRole>("Type1", entities.Mother.Type1);
        CheckValid<CaseRole>("Type1", entities.Child1.Type1);
      });
  }

  private bool ReadGeneticTest1()
  {
    entities.GeneticTest.Populated = false;

    return Read("ReadGeneticTest1",
      (db, command) =>
      {
        db.SetInt32(
          command, "testNumber", export.Child.Item.GeneticTest.TestNumber);
      },
      (db, reader) =>
      {
        entities.GeneticTest.TestNumber = db.GetInt32(reader, 0);
        entities.GeneticTest.ActualTestDate = db.GetNullableDate(reader, 1);
        entities.GeneticTest.TestResultReceivedDate =
          db.GetNullableDate(reader, 2);
        entities.GeneticTest.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.GeneticTest.LastUpdatedBy = db.GetString(reader, 4);
        entities.GeneticTest.LastUpdatedTimestamp = db.GetDateTime(reader, 5);
        entities.GeneticTest.CasNumber = db.GetNullableString(reader, 6);
        entities.GeneticTest.CspNumber = db.GetNullableString(reader, 7);
        entities.GeneticTest.CroType = db.GetNullableString(reader, 8);
        entities.GeneticTest.CroIdentifier = db.GetNullableInt32(reader, 9);
        entities.GeneticTest.CasMNumber = db.GetNullableString(reader, 10);
        entities.GeneticTest.CspMNumber = db.GetNullableString(reader, 11);
        entities.GeneticTest.CroMType = db.GetNullableString(reader, 12);
        entities.GeneticTest.CroMIdentifier = db.GetNullableInt32(reader, 13);
        entities.GeneticTest.CasANumber = db.GetNullableString(reader, 14);
        entities.GeneticTest.CspANumber = db.GetNullableString(reader, 15);
        entities.GeneticTest.CroAType = db.GetNullableString(reader, 16);
        entities.GeneticTest.CroAIdentifier = db.GetNullableInt32(reader, 17);
        entities.GeneticTest.Populated = true;
        CheckValid<GeneticTest>("CroType", entities.GeneticTest.CroType);
        CheckValid<GeneticTest>("CroMType", entities.GeneticTest.CroMType);
        CheckValid<GeneticTest>("CroAType", entities.GeneticTest.CroAType);
      });
  }

  private IEnumerable<bool> ReadGeneticTest2()
  {
    System.Diagnostics.Debug.Assert(entities.Child1.Populated);
    entities.GeneticTest.Populated = false;

    return ReadEach("ReadGeneticTest2",
      (db, command) =>
      {
        db.
          SetNullableInt32(command, "croIdentifier", entities.Child1.Identifier);
          
        db.SetNullableString(command, "croType", entities.Child1.Type1);
        db.SetNullableString(command, "casNumber", entities.Child1.CasNumber);
        db.SetNullableString(command, "cspNumber", entities.Child1.CspNumber);
      },
      (db, reader) =>
      {
        entities.GeneticTest.TestNumber = db.GetInt32(reader, 0);
        entities.GeneticTest.ActualTestDate = db.GetNullableDate(reader, 1);
        entities.GeneticTest.TestResultReceivedDate =
          db.GetNullableDate(reader, 2);
        entities.GeneticTest.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.GeneticTest.LastUpdatedBy = db.GetString(reader, 4);
        entities.GeneticTest.LastUpdatedTimestamp = db.GetDateTime(reader, 5);
        entities.GeneticTest.CasNumber = db.GetNullableString(reader, 6);
        entities.GeneticTest.CspNumber = db.GetNullableString(reader, 7);
        entities.GeneticTest.CroType = db.GetNullableString(reader, 8);
        entities.GeneticTest.CroIdentifier = db.GetNullableInt32(reader, 9);
        entities.GeneticTest.CasMNumber = db.GetNullableString(reader, 10);
        entities.GeneticTest.CspMNumber = db.GetNullableString(reader, 11);
        entities.GeneticTest.CroMType = db.GetNullableString(reader, 12);
        entities.GeneticTest.CroMIdentifier = db.GetNullableInt32(reader, 13);
        entities.GeneticTest.CasANumber = db.GetNullableString(reader, 14);
        entities.GeneticTest.CspANumber = db.GetNullableString(reader, 15);
        entities.GeneticTest.CroAType = db.GetNullableString(reader, 16);
        entities.GeneticTest.CroAIdentifier = db.GetNullableInt32(reader, 17);
        entities.GeneticTest.Populated = true;
        CheckValid<GeneticTest>("CroType", entities.GeneticTest.CroType);
        CheckValid<GeneticTest>("CroMType", entities.GeneticTest.CroMType);
        CheckValid<GeneticTest>("CroAType", entities.GeneticTest.CroAType);

        return true;
      });
  }

  private bool ReadLegalReferralLegalReferralCaseRoleCaseRole()
  {
    entities.CaseRole.Populated = false;
    entities.LegalReferralCaseRole.Populated = false;
    entities.LegalReferral.Populated = false;
    entities.CsePerson.Populated = false;

    return Read("ReadLegalReferralLegalReferralCaseRoleCaseRole",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "numb", export.Ap.Number);
      },
      (db, reader) =>
      {
        entities.LegalReferral.CasNumber = db.GetString(reader, 0);
        entities.LegalReferralCaseRole.CasNumber = db.GetString(reader, 0);
        entities.LegalReferral.Identifier = db.GetInt32(reader, 1);
        entities.LegalReferralCaseRole.LgrId = db.GetInt32(reader, 1);
        entities.LegalReferral.ReferralDate = db.GetDate(reader, 2);
        entities.LegalReferral.ReferralReason1 = db.GetString(reader, 3);
        entities.LegalReferral.ReferralReason2 = db.GetString(reader, 4);
        entities.LegalReferral.ReferralReason3 = db.GetString(reader, 5);
        entities.LegalReferral.ReferralReason4 = db.GetString(reader, 6);
        entities.LegalReferral.ReferralReason5 = db.GetString(reader, 7);
        entities.LegalReferralCaseRole.CreatedBy = db.GetString(reader, 8);
        entities.LegalReferralCaseRole.CreatedTimestamp =
          db.GetDateTime(reader, 9);
        entities.LegalReferralCaseRole.CasNumberRole = db.GetString(reader, 10);
        entities.CaseRole.CasNumber = db.GetString(reader, 10);
        entities.LegalReferralCaseRole.CspNumber = db.GetString(reader, 11);
        entities.CaseRole.CspNumber = db.GetString(reader, 11);
        entities.CsePerson.Number = db.GetString(reader, 11);
        entities.LegalReferralCaseRole.CroType = db.GetString(reader, 12);
        entities.CaseRole.Type1 = db.GetString(reader, 12);
        entities.LegalReferralCaseRole.CroId = db.GetInt32(reader, 13);
        entities.CaseRole.Identifier = db.GetInt32(reader, 13);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 14);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 15);
        entities.CsePerson.Type1 = db.GetString(reader, 16);
        entities.CaseRole.Populated = true;
        entities.LegalReferralCaseRole.Populated = true;
        entities.LegalReferral.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<LegalReferralCaseRole>("CroType",
          entities.LegalReferralCaseRole.CroType);
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadMonitoredActivity()
  {
    entities.MonitoredActivity.Populated = false;

    return Read("ReadMonitoredActivity",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", export.Case1.Number);
        db.SetNullableString(command, "csePersonNum", export.Ap.Number);
      },
      (db, reader) =>
      {
        entities.MonitoredActivity.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.MonitoredActivity.ActivityControlNumber =
          db.GetInt32(reader, 1);
        entities.MonitoredActivity.FedNonComplianceDate =
          db.GetNullableDate(reader, 2);
        entities.MonitoredActivity.StartDate = db.GetDate(reader, 3);
        entities.MonitoredActivity.ClosureDate = db.GetNullableDate(reader, 4);
        entities.MonitoredActivity.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.MonitoredActivity.InfSysGenId = db.GetNullableInt32(reader, 6);
        entities.MonitoredActivity.Populated = true;
      });
  }

  private bool ReadMother()
  {
    System.Diagnostics.Debug.Assert(entities.GeneticTest.Populated);
    entities.Mother.Populated = false;

    return Read("ReadMother",
      (db, command) =>
      {
        db.SetString(command, "casNumber1", entities.Case1.Number);
        db.SetInt32(
          command, "caseRoleId",
          entities.GeneticTest.CroMIdentifier.GetValueOrDefault());
        db.SetString(command, "type", entities.GeneticTest.CroMType ?? "");
        db.SetString(
          command, "casNumber2", entities.GeneticTest.CasMNumber ?? "");
        db.SetString(
          command, "cspNumber1", entities.GeneticTest.CspMNumber ?? "");
        db.SetString(command, "cspNumber2", export.Mo.Number);
      },
      (db, reader) =>
      {
        entities.Mother.CasNumber = db.GetString(reader, 0);
        entities.Mother.CspNumber = db.GetString(reader, 1);
        entities.Mother.Type1 = db.GetString(reader, 2);
        entities.Mother.Identifier = db.GetInt32(reader, 3);
        entities.Mother.StartDate = db.GetNullableDate(reader, 4);
        entities.Mother.EndDate = db.GetNullableDate(reader, 5);
        entities.Mother.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Mother.Type1);
      });
  }

  private IEnumerable<bool> ReadNarrativeDetail1()
  {
    entities.PaternityReview.Populated = false;

    return ReadEach("ReadNarrativeDetail1",
      (db, command) =>
      {
        db.SetInt32(
          command, "infrastructureId",
          export.HiddenPass1.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.PaternityReview.InfrastructureId = db.GetInt32(reader, 0);
        entities.PaternityReview.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.PaternityReview.NarrativeText =
          db.GetNullableString(reader, 2);
        entities.PaternityReview.LineNumber = db.GetInt32(reader, 3);
        entities.PaternityReview.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadNarrativeDetail2()
  {
    entities.PaternityReview.Populated = false;

    return ReadEach("ReadNarrativeDetail2",
      (db, command) =>
      {
        db.SetInt32(
          command, "infrastructureId",
          export.HiddenPass1.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.PaternityReview.InfrastructureId = db.GetInt32(reader, 0);
        entities.PaternityReview.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.PaternityReview.NarrativeText =
          db.GetNullableString(reader, 2);
        entities.PaternityReview.LineNumber = db.GetInt32(reader, 3);
        entities.PaternityReview.Populated = true;

        return true;
      });
  }

  private bool ReadServiceProcessLegalActionLegalActionPerson1()
  {
    entities.ServiceProcess.Populated = false;
    entities.LegalAction.Populated = false;
    entities.LegalActionPerson.Populated = false;

    return Read("ReadServiceProcessLegalActionLegalActionPerson1",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNumber", export.Ap.Number);
      },
      (db, reader) =>
      {
        entities.ServiceProcess.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.LgaIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.ServiceProcess.ServiceDate = db.GetDate(reader, 1);
        entities.ServiceProcess.Identifier = db.GetInt32(reader, 2);
        entities.LegalAction.Classification = db.GetString(reader, 3);
        entities.LegalAction.ActionTaken = db.GetString(reader, 4);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 5);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 6);
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 7);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 8);
        entities.LegalActionPerson.EffectiveDate = db.GetDate(reader, 9);
        entities.LegalActionPerson.Role = db.GetString(reader, 10);
        entities.LegalActionPerson.EndDate = db.GetNullableDate(reader, 11);
        entities.LegalActionPerson.AccountType =
          db.GetNullableString(reader, 12);
        entities.ServiceProcess.Populated = true;
        entities.LegalAction.Populated = true;
        entities.LegalActionPerson.Populated = true;
      });
  }

  private bool ReadServiceProcessLegalActionLegalActionPerson2()
  {
    entities.ServiceProcess.Populated = false;
    entities.LegalAction.Populated = false;
    entities.LegalActionPerson.Populated = false;

    return Read("ReadServiceProcessLegalActionLegalActionPerson2",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNumber", export.Ap.Number);
      },
      (db, reader) =>
      {
        entities.ServiceProcess.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.LgaIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.ServiceProcess.ServiceDate = db.GetDate(reader, 1);
        entities.ServiceProcess.Identifier = db.GetInt32(reader, 2);
        entities.LegalAction.Classification = db.GetString(reader, 3);
        entities.LegalAction.ActionTaken = db.GetString(reader, 4);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 5);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 6);
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 7);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 8);
        entities.LegalActionPerson.EffectiveDate = db.GetDate(reader, 9);
        entities.LegalActionPerson.Role = db.GetString(reader, 10);
        entities.LegalActionPerson.EndDate = db.GetNullableDate(reader, 11);
        entities.LegalActionPerson.AccountType =
          db.GetNullableString(reader, 12);
        entities.ServiceProcess.Populated = true;
        entities.LegalAction.Populated = true;
        entities.LegalActionPerson.Populated = true;
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
    /// <summary>A HiddenPassGroup group.</summary>
    [Serializable]
    public class HiddenPassGroup
    {
      /// <summary>
      /// A value of GimportHiddenPassed.
      /// </summary>
      [JsonPropertyName("gimportHiddenPassed")]
      public Common GimportHiddenPassed
      {
        get => gimportHiddenPassed ??= new();
        set => gimportHiddenPassed = value;
      }

      /// <summary>
      /// A value of GimportH.
      /// </summary>
      [JsonPropertyName("gimportH")]
      public NarrativeWork GimportH
      {
        get => gimportH ??= new();
        set => gimportH = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private Common gimportHiddenPassed;
      private NarrativeWork gimportH;
    }

    /// <summary>A ChildGroup group.</summary>
    [Serializable]
    public class ChildGroup
    {
      /// <summary>
      /// A value of Compliance.
      /// </summary>
      [JsonPropertyName("compliance")]
      public DateWorkArea Compliance
      {
        get => compliance ??= new();
        set => compliance = value;
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
      /// A value of Child1.
      /// </summary>
      [JsonPropertyName("child1")]
      public CaseRole Child1
      {
        get => child1 ??= new();
        set => child1 = value;
      }

      /// <summary>
      /// A value of GeneticTest.
      /// </summary>
      [JsonPropertyName("geneticTest")]
      public GeneticTest GeneticTest
      {
        get => geneticTest ??= new();
        set => geneticTest = value;
      }

      /// <summary>
      /// A value of LegalReferral.
      /// </summary>
      [JsonPropertyName("legalReferral")]
      public LegalReferral LegalReferral
      {
        get => legalReferral ??= new();
        set => legalReferral = value;
      }

      /// <summary>
      /// A value of ReferredToLegal.
      /// </summary>
      [JsonPropertyName("referredToLegal")]
      public Common ReferredToLegal
      {
        get => referredToLegal ??= new();
        set => referredToLegal = value;
      }

      /// <summary>
      /// A value of StartDate.
      /// </summary>
      [JsonPropertyName("startDate")]
      public DateWorkArea StartDate
      {
        get => startDate ??= new();
        set => startDate = value;
      }

      /// <summary>
      /// A value of FedCompliance.
      /// </summary>
      [JsonPropertyName("fedCompliance")]
      public Common FedCompliance
      {
        get => fedCompliance ??= new();
        set => fedCompliance = value;
      }

      /// <summary>
      /// A value of ChildAge.
      /// </summary>
      [JsonPropertyName("childAge")]
      public Common ChildAge
      {
        get => childAge ??= new();
        set => childAge = value;
      }

      /// <summary>
      /// A value of DaysRemainInCompli.
      /// </summary>
      [JsonPropertyName("daysRemainInCompli")]
      public Common DaysRemainInCompli
      {
        get => daysRemainInCompli ??= new();
        set => daysRemainInCompli = value;
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
      /// A value of ChildCaseRole.
      /// </summary>
      [JsonPropertyName("childCaseRole")]
      public CaseRole ChildCaseRole
      {
        get => childCaseRole ??= new();
        set => childCaseRole = value;
      }

      /// <summary>
      /// A value of ChildCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("childCsePersonsWorkSet")]
      public CsePersonsWorkSet ChildCsePersonsWorkSet
      {
        get => childCsePersonsWorkSet ??= new();
        set => childCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of ChAgeMsgTxt.
      /// </summary>
      [JsonPropertyName("chAgeMsgTxt")]
      public SpTextWorkArea ChAgeMsgTxt
      {
        get => chAgeMsgTxt ??= new();
        set => chAgeMsgTxt = value;
      }

      /// <summary>
      /// A value of GeneticTestSchedule.
      /// </summary>
      [JsonPropertyName("geneticTestSchedule")]
      public Common GeneticTestSchedule
      {
        get => geneticTestSchedule ??= new();
        set => geneticTestSchedule = value;
      }

      /// <summary>
      /// A value of ChildCsePerson.
      /// </summary>
      [JsonPropertyName("childCsePerson")]
      public CsePerson ChildCsePerson
      {
        get => childCsePerson ??= new();
        set => childCsePerson = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 15;

      private DateWorkArea compliance;
      private Common common;
      private CaseRole child1;
      private GeneticTest geneticTest;
      private LegalReferral legalReferral;
      private Common referredToLegal;
      private DateWorkArea startDate;
      private Common fedCompliance;
      private Common childAge;
      private Common daysRemainInCompli;
      private CaseRole ap;
      private CaseRole childCaseRole;
      private CsePersonsWorkSet childCsePersonsWorkSet;
      private SpTextWorkArea chAgeMsgTxt;
      private Common geneticTestSchedule;
      private CsePerson childCsePerson;
    }

    /// <summary>
    /// A value of CaseClosedIndicator.
    /// </summary>
    [JsonPropertyName("caseClosedIndicator")]
    public Common CaseClosedIndicator
    {
      get => caseClosedIndicator ??= new();
      set => caseClosedIndicator = value;
    }

    /// <summary>
    /// A value of HiddenPass1.
    /// </summary>
    [JsonPropertyName("hiddenPass1")]
    public Infrastructure HiddenPass1
    {
      get => hiddenPass1 ??= new();
      set => hiddenPass1 = value;
    }

    /// <summary>
    /// A value of PatReview.
    /// </summary>
    [JsonPropertyName("patReview")]
    public NarrativeWork PatReview
    {
      get => patReview ??= new();
      set => patReview = value;
    }

    /// <summary>
    /// Gets a value of HiddenPass.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenPassGroup> HiddenPass => hiddenPass ??= new(
      HiddenPassGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenPass for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenPass")]
    [Computed]
    public IList<HiddenPassGroup> HiddenPass_Json
    {
      get => hiddenPass;
      set => HiddenPass.Assign(value);
    }

    /// <summary>
    /// A value of LegalReferral.
    /// </summary>
    [JsonPropertyName("legalReferral")]
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
    }

    /// <summary>
    /// A value of HiddenPassedReviewType.
    /// </summary>
    [JsonPropertyName("hiddenPassedReviewType")]
    public Common HiddenPassedReviewType
    {
      get => hiddenPassedReviewType ??= new();
      set => hiddenPassedReviewType = value;
    }

    /// <summary>
    /// A value of ServiceProcess.
    /// </summary>
    [JsonPropertyName("serviceProcess")]
    public ServiceProcess ServiceProcess
    {
      get => serviceProcess ??= new();
      set => serviceProcess = value;
    }

    /// <summary>
    /// Gets a value of Child.
    /// </summary>
    [JsonIgnore]
    public Array<ChildGroup> Child => child ??= new(ChildGroup.Capacity);

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
    /// A value of Mo.
    /// </summary>
    [JsonPropertyName("mo")]
    public CsePersonsWorkSet Mo
    {
      get => mo ??= new();
      set => mo = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of ActivityLiteral.
    /// </summary>
    [JsonPropertyName("activityLiteral")]
    public WorkArea ActivityLiteral
    {
      get => activityLiteral ??= new();
      set => activityLiteral = value;
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
    /// A value of UnknownPaternity.
    /// </summary>
    [JsonPropertyName("unknownPaternity")]
    public Common UnknownPaternity
    {
      get => unknownPaternity ??= new();
      set => unknownPaternity = value;
    }

    /// <summary>
    /// A value of FromCrpa.
    /// </summary>
    [JsonPropertyName("fromCrpa")]
    public Common FromCrpa
    {
      get => fromCrpa ??= new();
      set => fromCrpa = value;
    }

    private Common caseClosedIndicator;
    private Infrastructure hiddenPass1;
    private NarrativeWork patReview;
    private Array<HiddenPassGroup> hiddenPass;
    private LegalReferral legalReferral;
    private Common hiddenPassedReviewType;
    private ServiceProcess serviceProcess;
    private Array<ChildGroup> child;
    private CsePersonsWorkSet mo;
    private CsePersonsWorkSet ar;
    private CsePersonsWorkSet ap;
    private Case1 case1;
    private NextTranInfo hidden;
    private Standard standard;
    private WorkArea activityLiteral;
    private TextWorkArea moreApsMsg;
    private Common multiAp;
    private Common apSelected;
    private CsePersonsWorkSet selectedAp;
    private Common unknownPaternity;
    private Common fromCrpa;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A HiddenPassGroup group.</summary>
    [Serializable]
    public class HiddenPassGroup
    {
      /// <summary>
      /// A value of GexportHiddenPassed.
      /// </summary>
      [JsonPropertyName("gexportHiddenPassed")]
      public Common GexportHiddenPassed
      {
        get => gexportHiddenPassed ??= new();
        set => gexportHiddenPassed = value;
      }

      /// <summary>
      /// A value of GexportH.
      /// </summary>
      [JsonPropertyName("gexportH")]
      public NarrativeWork GexportH
      {
        get => gexportH ??= new();
        set => gexportH = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private Common gexportHiddenPassed;
      private NarrativeWork gexportH;
    }

    /// <summary>A ChildGroup group.</summary>
    [Serializable]
    public class ChildGroup
    {
      /// <summary>
      /// A value of Compliance.
      /// </summary>
      [JsonPropertyName("compliance")]
      public DateWorkArea Compliance
      {
        get => compliance ??= new();
        set => compliance = value;
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
      /// A value of Child1.
      /// </summary>
      [JsonPropertyName("child1")]
      public CaseRole Child1
      {
        get => child1 ??= new();
        set => child1 = value;
      }

      /// <summary>
      /// A value of GeneticTest.
      /// </summary>
      [JsonPropertyName("geneticTest")]
      public GeneticTest GeneticTest
      {
        get => geneticTest ??= new();
        set => geneticTest = value;
      }

      /// <summary>
      /// A value of LegalReferral.
      /// </summary>
      [JsonPropertyName("legalReferral")]
      public LegalReferral LegalReferral
      {
        get => legalReferral ??= new();
        set => legalReferral = value;
      }

      /// <summary>
      /// A value of StartDate.
      /// </summary>
      [JsonPropertyName("startDate")]
      public DateWorkArea StartDate
      {
        get => startDate ??= new();
        set => startDate = value;
      }

      /// <summary>
      /// A value of ReferredToLegal.
      /// </summary>
      [JsonPropertyName("referredToLegal")]
      public Common ReferredToLegal
      {
        get => referredToLegal ??= new();
        set => referredToLegal = value;
      }

      /// <summary>
      /// A value of FedCompliance.
      /// </summary>
      [JsonPropertyName("fedCompliance")]
      public Common FedCompliance
      {
        get => fedCompliance ??= new();
        set => fedCompliance = value;
      }

      /// <summary>
      /// A value of ChildAge.
      /// </summary>
      [JsonPropertyName("childAge")]
      public Common ChildAge
      {
        get => childAge ??= new();
        set => childAge = value;
      }

      /// <summary>
      /// A value of DaysRemainInCompli.
      /// </summary>
      [JsonPropertyName("daysRemainInCompli")]
      public Common DaysRemainInCompli
      {
        get => daysRemainInCompli ??= new();
        set => daysRemainInCompli = value;
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
      /// A value of ChildCaseRole.
      /// </summary>
      [JsonPropertyName("childCaseRole")]
      public CaseRole ChildCaseRole
      {
        get => childCaseRole ??= new();
        set => childCaseRole = value;
      }

      /// <summary>
      /// A value of ChildCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("childCsePersonsWorkSet")]
      public CsePersonsWorkSet ChildCsePersonsWorkSet
      {
        get => childCsePersonsWorkSet ??= new();
        set => childCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of ChAgeMsgTxt.
      /// </summary>
      [JsonPropertyName("chAgeMsgTxt")]
      public SpTextWorkArea ChAgeMsgTxt
      {
        get => chAgeMsgTxt ??= new();
        set => chAgeMsgTxt = value;
      }

      /// <summary>
      /// A value of GeneticTestSchedule.
      /// </summary>
      [JsonPropertyName("geneticTestSchedule")]
      public Common GeneticTestSchedule
      {
        get => geneticTestSchedule ??= new();
        set => geneticTestSchedule = value;
      }

      /// <summary>
      /// A value of ChildCsePerson.
      /// </summary>
      [JsonPropertyName("childCsePerson")]
      public CsePerson ChildCsePerson
      {
        get => childCsePerson ??= new();
        set => childCsePerson = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 15;

      private DateWorkArea compliance;
      private Common common;
      private CaseRole child1;
      private GeneticTest geneticTest;
      private LegalReferral legalReferral;
      private DateWorkArea startDate;
      private Common referredToLegal;
      private Common fedCompliance;
      private Common childAge;
      private Common daysRemainInCompli;
      private CaseRole ap;
      private CaseRole childCaseRole;
      private CsePersonsWorkSet childCsePersonsWorkSet;
      private SpTextWorkArea chAgeMsgTxt;
      private Common geneticTestSchedule;
      private CsePerson childCsePerson;
    }

    /// <summary>
    /// A value of CaseClosedIndicator.
    /// </summary>
    [JsonPropertyName("caseClosedIndicator")]
    public Common CaseClosedIndicator
    {
      get => caseClosedIndicator ??= new();
      set => caseClosedIndicator = value;
    }

    /// <summary>
    /// A value of LocalReturnedAge.
    /// </summary>
    [JsonPropertyName("localReturnedAge")]
    public Common LocalReturnedAge
    {
      get => localReturnedAge ??= new();
      set => localReturnedAge = value;
    }

    /// <summary>
    /// A value of SelectedMotherCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("selectedMotherCsePersonsWorkSet")]
    public CsePersonsWorkSet SelectedMotherCsePersonsWorkSet
    {
      get => selectedMotherCsePersonsWorkSet ??= new();
      set => selectedMotherCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of SelectedMotherCsePerson.
    /// </summary>
    [JsonPropertyName("selectedMotherCsePerson")]
    public CsePerson SelectedMotherCsePerson
    {
      get => selectedMotherCsePerson ??= new();
      set => selectedMotherCsePerson = value;
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
    /// A value of SelectedAllegFather.
    /// </summary>
    [JsonPropertyName("selectedAllegFather")]
    public CsePersonsWorkSet SelectedAllegFather
    {
      get => selectedAllegFather ??= new();
      set => selectedAllegFather = value;
    }

    /// <summary>
    /// A value of HiddenPass1.
    /// </summary>
    [JsonPropertyName("hiddenPass1")]
    public Infrastructure HiddenPass1
    {
      get => hiddenPass1 ??= new();
      set => hiddenPass1 = value;
    }

    /// <summary>
    /// A value of PatReview.
    /// </summary>
    [JsonPropertyName("patReview")]
    public NarrativeWork PatReview
    {
      get => patReview ??= new();
      set => patReview = value;
    }

    /// <summary>
    /// A value of ServiceProcess.
    /// </summary>
    [JsonPropertyName("serviceProcess")]
    public ServiceProcess ServiceProcess
    {
      get => serviceProcess ??= new();
      set => serviceProcess = value;
    }

    /// <summary>
    /// Gets a value of HiddenPass.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenPassGroup> HiddenPass => hiddenPass ??= new(
      HiddenPassGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenPass for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenPass")]
    [Computed]
    public IList<HiddenPassGroup> HiddenPass_Json
    {
      get => hiddenPass;
      set => HiddenPass.Assign(value);
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
    /// A value of LegalReferral.
    /// </summary>
    [JsonPropertyName("legalReferral")]
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
    }

    /// <summary>
    /// A value of HiddenPassedReviewType.
    /// </summary>
    [JsonPropertyName("hiddenPassedReviewType")]
    public Common HiddenPassedReviewType
    {
      get => hiddenPassedReviewType ??= new();
      set => hiddenPassedReviewType = value;
    }

    /// <summary>
    /// Gets a value of Child.
    /// </summary>
    [JsonIgnore]
    public Array<ChildGroup> Child => child ??= new(ChildGroup.Capacity);

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
    /// A value of Mo.
    /// </summary>
    [JsonPropertyName("mo")]
    public CsePersonsWorkSet Mo
    {
      get => mo ??= new();
      set => mo = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of ActivityLiteral.
    /// </summary>
    [JsonPropertyName("activityLiteral")]
    public WorkArea ActivityLiteral
    {
      get => activityLiteral ??= new();
      set => activityLiteral = value;
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
    /// A value of HiddenPassed.
    /// </summary>
    [JsonPropertyName("hiddenPassed")]
    public Infrastructure HiddenPassed
    {
      get => hiddenPassed ??= new();
      set => hiddenPassed = value;
    }

    /// <summary>
    /// A value of UnknownPaternity.
    /// </summary>
    [JsonPropertyName("unknownPaternity")]
    public Common UnknownPaternity
    {
      get => unknownPaternity ??= new();
      set => unknownPaternity = value;
    }

    /// <summary>
    /// A value of FromCrpa.
    /// </summary>
    [JsonPropertyName("fromCrpa")]
    public Common FromCrpa
    {
      get => fromCrpa ??= new();
      set => fromCrpa = value;
    }

    private Common caseClosedIndicator;
    private Common localReturnedAge;
    private CsePersonsWorkSet selectedMotherCsePersonsWorkSet;
    private CsePerson selectedMotherCsePerson;
    private CsePerson selectedChildCsePerson;
    private CsePersonsWorkSet selectedAllegFather;
    private Infrastructure hiddenPass1;
    private NarrativeWork patReview;
    private ServiceProcess serviceProcess;
    private Array<HiddenPassGroup> hiddenPass;
    private CsePersonsWorkSet selectedChildCsePersonsWorkSet;
    private LegalReferral legalReferral;
    private Common hiddenPassedReviewType;
    private Array<ChildGroup> child;
    private CsePersonsWorkSet mo;
    private CsePersonsWorkSet ar;
    private CsePersonsWorkSet ap;
    private Case1 case1;
    private NextTranInfo hidden;
    private Standard standard;
    private WorkArea activityLiteral;
    private TextWorkArea moreApsMsg;
    private Common multiAp;
    private Common apSelected;
    private CsePersonsWorkSet selectedAp;
    private Infrastructure hiddenPassed;
    private Common unknownPaternity;
    private Common fromCrpa;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A WorkGroup group.</summary>
    [Serializable]
    public class WorkGroup
    {
      /// <summary>
      /// A value of Work1.
      /// </summary>
      [JsonPropertyName("work1")]
      public NarrativeDetail Work1
      {
        get => work1 ??= new();
        set => work1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private NarrativeDetail work1;
    }

    /// <summary>
    /// Gets a value of Work.
    /// </summary>
    [JsonIgnore]
    public Array<WorkGroup> Work => work ??= new(WorkGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Work for json serialization.
    /// </summary>
    [JsonPropertyName("work")]
    [Computed]
    public IList<WorkGroup> Work_Json
    {
      get => work;
      set => Work.Assign(value);
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
    /// A value of ChAge.
    /// </summary>
    [JsonPropertyName("chAge")]
    public Common ChAge
    {
      get => chAge ??= new();
      set => chAge = value;
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
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DateWorkArea Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
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
    /// A value of FemaleAp.
    /// </summary>
    [JsonPropertyName("femaleAp")]
    public Common FemaleAp
    {
      get => femaleAp ??= new();
      set => femaleAp = value;
    }

    private Array<WorkGroup> work;
    private Common count;
    private Common chAge;
    private DateWorkArea current;
    private DateWorkArea maxDate;
    private DateWorkArea initialized;
    private AbendData abendData;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common femaleAp;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of PaternityReview.
    /// </summary>
    [JsonPropertyName("paternityReview")]
    public NarrativeDetail PaternityReview
    {
      get => paternityReview ??= new();
      set => paternityReview = value;
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
    /// A value of LegalReferralCaseRole.
    /// </summary>
    [JsonPropertyName("legalReferralCaseRole")]
    public LegalReferralCaseRole LegalReferralCaseRole
    {
      get => legalReferralCaseRole ??= new();
      set => legalReferralCaseRole = value;
    }

    /// <summary>
    /// A value of ServiceProcess.
    /// </summary>
    [JsonPropertyName("serviceProcess")]
    public ServiceProcess ServiceProcess
    {
      get => serviceProcess ??= new();
      set => serviceProcess = value;
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
    /// A value of AbsentParent.
    /// </summary>
    [JsonPropertyName("absentParent")]
    public CaseRole AbsentParent
    {
      get => absentParent ??= new();
      set => absentParent = value;
    }

    /// <summary>
    /// A value of AllegedFather.
    /// </summary>
    [JsonPropertyName("allegedFather")]
    public CaseRole AllegedFather
    {
      get => allegedFather ??= new();
      set => allegedFather = value;
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
    /// A value of MonitoredActivity.
    /// </summary>
    [JsonPropertyName("monitoredActivity")]
    public MonitoredActivity MonitoredActivity
    {
      get => monitoredActivity ??= new();
      set => monitoredActivity = value;
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
    /// A value of GeneticTest.
    /// </summary>
    [JsonPropertyName("geneticTest")]
    public GeneticTest GeneticTest
    {
      get => geneticTest ??= new();
      set => geneticTest = value;
    }

    /// <summary>
    /// A value of LegalReferral.
    /// </summary>
    [JsonPropertyName("legalReferral")]
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
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

    /// <summary>
    /// A value of ApplicantRecipient.
    /// </summary>
    [JsonPropertyName("applicantRecipient")]
    public CaseRole ApplicantRecipient
    {
      get => applicantRecipient ??= new();
      set => applicantRecipient = value;
    }

    /// <summary>
    /// A value of Child1.
    /// </summary>
    [JsonPropertyName("child1")]
    public CaseRole Child1
    {
      get => child1 ??= new();
      set => child1 = value;
    }

    /// <summary>
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    /// <summary>
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
    }

    /// <summary>
    /// A value of Child2.
    /// </summary>
    [JsonPropertyName("child2")]
    public CsePerson Child2
    {
      get => child2 ??= new();
      set => child2 = value;
    }

    private NarrativeDetail paternityReview;
    private CaseRole caseRole;
    private LegalReferralCaseRole legalReferralCaseRole;
    private ServiceProcess serviceProcess;
    private LegalAction legalAction;
    private CaseRole absentParent;
    private CaseRole allegedFather;
    private CaseRole mother;
    private CaseRole father;
    private MonitoredActivity monitoredActivity;
    private Infrastructure infrastructure;
    private GeneticTest geneticTest;
    private LegalReferral legalReferral;
    private Case1 case1;
    private CsePerson csePerson;
    private CaseRole applicantRecipient;
    private CaseRole child1;
    private LegalActionDetail legalActionDetail;
    private LegalActionPerson legalActionPerson;
    private CsePerson child2;
  }
#endregion
}
