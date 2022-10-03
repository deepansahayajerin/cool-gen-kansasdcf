// Program: SP_CRES_CASE_RVIEW_ESTABLISHMENT, ID: 372651489, model: 746.
// Short name: SWECRESP
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
/// A program: SP_CRES_CASE_RVIEW_ESTABLISHMENT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpCresCaseRviewEstablishment: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CRES_CASE_RVIEW_ESTABLISHMENT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCresCaseRviewEstablishment(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCresCaseRviewEstablishment.
  /// </summary>
  public SpCresCaseRviewEstablishment(IContext context, Import import,
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
    // 03/14/96 Alan Hackler            Retro fits
    // 01/03/97 R. Marchman		 Add new security/next tran.
    // 01/10/97 R. Welborn		 Conversion from Plan Task Note
    // 				 to Infrastructure/Narrative.
    // 04/30/97 R. Grey		 Change Current Date
    // 07/22/97 R. Grey		 Add review closed case
    // 04/16/99 N.Engoor                Removed redundant READ stmnts. Added  
    // new fields on the screen. Changed the way in which the notes were being
    // displayed.
    // ---------------------------------------------
    // -----------------------------------------------------------------------------------
    // 02/03/2000          Vithal Madhira                   PR# 86247        
    // Modified code to implement the case review for each AP on a multiple AP
    // case.
    // 08/08/2000 SWSRCHF WR# 000170  Replace the read for Narrative by a read 
    // for
    //                                
    // Narrative Detail
    // 08/13/10 J Huss		CQ# 16256	Corrected AP number passed when flowing to 
    // CREN.
    // 04/01/2011	T Pierce	CQ# 23212	Removed references to obsolete Narrative 
    // entity type.
    // ------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);
    local.Current.Date = Now().Date;
    local.MaxDate.Date = UseCabSetMaximumDiscontinueDate();
    export.Case1.Number = import.Case1.Number;
    export.HiddenPass1.SystemGeneratedIdentifier =
      import.HiddenPass1.SystemGeneratedIdentifier;
    export.HiddenPassedReviewType.ActionEntry =
      import.HiddenPassedReviewType.ActionEntry;
    export.ActivityLiteral.Text16 = import.ActivityLiteral.Text16;
    export.LegalReferral.ReferralDate = import.LegalReferral.ReferralDate;
    export.CaseClosedIndicator.Flag = import.CaseClosedIndicator.Flag;
    export.MoreApsMsg.Text30 = import.MoreApsMsg.Text30;
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.MultiAp.Flag = import.MultiAp.Flag;
    export.ApSelected.Flag = import.ApSelected.Flag;
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

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_XFR_TO_MENU";

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ------------------
      // This is where you set your export value to the export hidden next tran 
      // values if the user is comming into this procedure on a next tran
      // action.
      // ------------------
      export.Case1.Number = export.Hidden.CaseNumber ?? Spaces(10);
      UseScCabNextTranGet();
      global.Command = "DISPLAY";

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      // ------------------
      // This is where you set your command to do whatever is necessary to do on
      // a flow from the menu, maybe just escape....
      // ------------------
      // You should get this information from the Dialog Flow Diagram.  It is 
      // the SEND CMD on the propertis for a Transfer from one  procedure to
      // another.
      // *** the statement would read COMMAND IS display   *****
      global.Command = "DISPLAY";

      return;
    }

    if (!Equal(global.Command, "DISPLAY"))
    {
      export.ApServed.ServiceDate = import.ApServed.ServiceDate;
      export.Ar.Assign(import.Ar);
      MoveCsePersonsWorkSet2(import.Ap1, export.Ap1);
      export.CurrentSupportAp1.Flag = import.CurrentSupportAp1.Flag;
      export.JudgementAp1.Flag = import.JudgementAp1.Flag;
      export.MedicalSupportAp1.Flag = import.ImporMedicalSupportAp1.Flag;
      export.EstReview.Text = import.EstReview.Text;

      export.Export1.Index = 0;
      export.Export1.Clear();

      for(import.Import1.Index = 0; import.Import1.Index < import
        .Import1.Count; ++import.Import1.Index)
      {
        if (export.Export1.IsFull)
        {
          break;
        }

        export.Export1.Update.LegalReferral.ReferralDate =
          import.Import1.Item.LegalReferral.ReferralDate;
        export.Export1.Update.Compliance.Date =
          import.Import1.Item.Compliance.Date;
        export.Export1.Update.Start.Date = import.Import1.Item.Start.Date;
        export.Export1.Update.Common.SelectChar =
          import.Import1.Item.Common.SelectChar;

        if (Equal(global.Command, "RETLINK"))
        {
          if (AsChar(export.Export1.Item.Common.SelectChar) == '*')
          {
            export.Export1.Update.Common.SelectChar = "";
            global.Command = "DISPLAY";
          }
        }

        MoveCaseRole(import.Import1.Item.Child1, export.Export1.Update.Child1);
        export.Export1.Update.Ap1ResponsibleParty.Flag =
          import.Import1.Item.Ap1ResponsibleParty.Flag;
        export.Export1.Update.ChildCsePersonsWorkSet.Assign(
          import.Import1.Item.ChildCsePersonsWorkSet);
        export.Export1.Update.ChildAge.TotalInteger =
          import.Import1.Item.ChildAge.TotalInteger;
        export.Export1.Update.FedCompliance.Flag =
          import.Import1.Item.FedCompliance.Flag;
        export.Export1.Update.DaysRemainInCompli.Count =
          import.Import1.Item.DaysRemainInCompli.Count;
        MoveCommon(import.Import1.Item.Ap1RightsSevered,
          export.Export1.Update.Ap1RightsSevered);
        MoveHealthInsuranceViability(import.Import1.Item.
          HealthInsuranceViability,
          export.Export1.Update.HealthInsuranceViability);
        export.Export1.Update.MedicalSupportAr.Flag =
          import.Import1.Item.MedicalSupportAr.Flag;
        export.Export1.Update.ChildHlthInsuAvail.Flag =
          import.Import1.Item.ChildHlthInsuAvail.Flag;
        export.Export1.Update.Judgement.Flag =
          import.Import1.Item.Judgement.Flag;
        export.Export1.Update.CurrentSupport.Flag =
          import.Import1.Item.CurrentSupport.Flag;
        export.Export1.Update.MedicalSupport.Flag =
          import.Import1.Item.MedicalSupport.Flag;

        export.Export1.Item.ProgramPerChild.Index = 0;
        export.Export1.Item.ProgramPerChild.Clear();

        for(import.Import1.Item.ProgramPerChild.Index = 0; import
          .Import1.Item.ProgramPerChild.Index < import
          .Import1.Item.ProgramPerChild.Count; ++
          import.Import1.Item.ProgramPerChild.Index)
        {
          if (export.Export1.Item.ProgramPerChild.IsFull)
          {
            break;
          }

          export.Export1.Update.ProgramPerChild.Update.Program.Code =
            import.Import1.Item.ProgramPerChild.Item.Program.Code;
          export.Export1.Item.ProgramPerChild.Next();
        }

        export.Export1.Next();
      }

      if (Equal(global.Command, "RETLINK"))
      {
        if (AsChar(export.Export1.Item.Common.SelectChar) == '*')
        {
          export.Export1.Update.Common.SelectChar = "";
        }
      }
    }

    if (Equal(import.HiddenPassedReviewType.ActionEntry, "R"))
    {
      var field = GetField(export.EstReview, "text");

      field.Color = "cyan";
      field.Protected = true;
    }
    else
    {
      var field = GetField(export.EstReview, "text");

      field.Color = "green";
      field.Highlighting = Highlighting.Underscore;
      field.Protected = false;
      field.Focused = true;
    }

    // ------------------
    // If the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. Now validate.
    // ------------------
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      if (Equal(global.Command, "INVALID"))
      {
        export.Standard.NextTransaction = "";
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        return;
      }

      if (!Equal(global.Command, "ENTER"))
      {
        export.Standard.NextTransaction = "";

        goto Test;
      }

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

Test:

    if (Equal(global.Command, "HICV") || Equal(global.Command, "CURA") || Equal
      (global.Command, "INCL") || Equal(global.Command, "LGRQ") || Equal
      (global.Command, "RETLINK"))
    {
    }
    else
    {
      // to validate action level security
      UseScCabTestSecurity();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }

    if (Equal(global.Command, "HICV") || Equal(global.Command, "CURA") || Equal
      (global.Command, "INCL") || Equal(global.Command, "LGRQ") || Equal
      (global.Command, "RETURN"))
    {
      if (!export.HiddenPass.IsEmpty)
      {
        export.HiddenPass.Index = 3;
        export.HiddenPass.CheckSize();

        if (!Equal(export.EstReview.Text, import.HiddenPass.Item.GimportH.Text))
        {
          export.HiddenPass.Update.GexportH.Text = export.EstReview.Text;
        }
      }
    }

    if (Equal(global.Command, "RETLINK"))
    {
      global.Command = "DISPLAY";
    }

    switch(TrimEnd(global.Command))
    {
      case "HICV":
        ExitState = "ECO_LNK_TO_HICV";

        return;
      case "CURA":
        if (export.Export1.IsEmpty)
        {
          ExitState = "SP0000_INVALID_ACTION_NO_CHILD";

          return;
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!IsEmpty(export.Export1.Item.Common.SelectChar))
          {
            if (AsChar(export.Export1.Item.Common.SelectChar) == 'S')
            {
              export.Export1.Update.Common.SelectChar = "*";
              export.SelectedChildCsePerson.Number =
                export.Export1.Item.ChildCsePersonsWorkSet.Number;
              MoveCsePersonsWorkSet3(export.Export1.Item.ChildCsePersonsWorkSet,
                export.SelectedChildCsePersonsWorkSet);
              ExitState = "ECO_LNK_TO_CURA";

              return;
            }
            else
            {
              var field = GetField(export.Export1.Item.Common, "selectChar");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

              return;
            }
          }
        }

        ExitState = "ACO_NE0000_NO_SELECTION_MADE";

        return;
      case "INCL":
        ExitState = "ECO_LNK_TO_INCL_INC_SOURCE_LIST";

        return;
      case "LGRQ":
        ExitState = "ECO_LNK_TO_LEGAL_REQUEST";

        return;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "DISPLAY":
        if (ReadCase())
        {
          if (AsChar(export.CaseClosedIndicator.Flag) == 'Y')
          {
            local.OpenOrClosed.Date = entities.Case1.StatusDate;
          }
          else
          {
            local.OpenOrClosed.Date = local.Current.Date;
          }
        }
        else
        {
          ExitState = "CASE_NF";

          return;
        }

        if (ReadCsePerson1())
        {
          local.CsePersonsWorkSet.Number = entities.Ar.Number;
          UseSiReadCsePerson();

          if (!IsEmpty(local.AbendData.Type1))
          {
            return;
          }

          export.Ar.Assign(local.CsePersonsWorkSet);
        }

        // -----------------------------------------------------------------------------------
        // 02/03/2000          Vithal Madhira                   PR# 86247
        // Modified code to implement the case review for each AP on a
        // multiple AP case.
        // ------------------------------------------------------------------------------
        if (!IsEmpty(export.SelectedAp.Number) && AsChar
          (export.ApSelected.Flag) == 'Y')
        {
          MoveCsePersonsWorkSet3(export.SelectedAp, export.Ap1);

          if (AsChar(export.MultiAp.Flag) == 'Y')
          {
            export.MoreApsMsg.Text30 = "More AP's exist for case.";
          }

          if (ReadServiceProcess())
          {
            export.ApServed.ServiceDate = entities.ServiceProcess.ServiceDate;
          }
        }
        else
        {
          foreach(var item in ReadCsePerson2())
          {
            if (IsEmpty(local.Ap1.Number))
            {
              local.Ap1.Number = entities.Ap.Number;
              local.CsePersonsWorkSet.Number = entities.Ap.Number;
              UseSiReadCsePerson();

              if (!IsEmpty(local.AbendData.Type1))
              {
                return;
              }

              export.Ap1.Assign(local.CsePersonsWorkSet);
            }
            else
            {
              export.Ap2.Assign(local.CsePersonsWorkSet);
              export.MoreApsMsg.Text30 = "More AP's exist for case.";

              break;
            }
          }

          if (ReadServiceProcess())
          {
            export.ApServed.ServiceDate = entities.ServiceProcess.ServiceDate;
          }
        }

        // *******************************************
        // LOOK FOR A LEGAL REFERRAL FOR ESTABLISHING A COURT ORDER
        // *******************************************
        foreach(var item in ReadLegalReferral())
        {
          if (AsChar(export.CaseClosedIndicator.Flag) != 'Y' && AsChar
            (entities.LegalReferral.Status) == 'C')
          {
            continue;
          }

          export.LegalReferral.ReferralDate =
            entities.LegalReferral.ReferralDate;

          break;
        }

        export.Export1.Index = 0;
        export.Export1.Clear();

        foreach(var item in ReadCsePersonChild())
        {
          local.CsePersonsWorkSet.Number = entities.Child1.Number;
          UseSiReadCsePerson();

          if (!IsEmpty(local.AbendData.Type1))
          {
            export.Export1.Next();

            return;
          }

          if (Equal(local.CsePersonsWorkSet.Dob, local.Initialized.Date) || Equal
            (local.CsePersonsWorkSet.Dob, local.MaxDate.Date))
          {
            export.Export1.Update.ChildAge.TotalInteger = 0;
          }
          else
          {
            UseCabCalcCurrentAgeFromDob();
            export.Export1.Update.ChildAge.TotalInteger =
              export.LocalReturnedAge.TotalInteger;
          }

          UseSpCabIndSupportAndJugmnt();
          export.Export1.Update.MedicalSupport.Flag =
            local.ChMedicalSupport.Flag;
          export.Export1.Update.Judgement.Flag = local.ChJudgement.Flag;
          export.Export1.Update.CurrentSupport.Flag =
            local.ChCurrentSupport.Flag;
          export.Export1.Update.ChildCsePersonsWorkSet.Assign(
            local.CsePersonsWorkSet);

          if (ReadPersonalHealthInsurance())
          {
            if (ReadHealthInsuranceCoverage())
            {
              export.Export1.Update.ChildHlthInsuAvail.Flag = "Y";
            }
            else
            {
              export.Export1.Update.ChildHlthInsuAvail.Flag = "N";
            }
          }

          if (ReadHealthInsuranceViability())
          {
            MoveHealthInsuranceViability(entities.HealthInsuranceViability,
              export.Export1.Update.HealthInsuranceViability);
          }

          export.Export1.Update.Ap1RightsSevered.SelectChar =
            entities.Child2.FcParentalRights ?? Spaces(1);

          export.Export1.Item.ProgramPerChild.Index = 0;
          export.Export1.Item.ProgramPerChild.Clear();

          foreach(var item1 in ReadProgram())
          {
            export.Export1.Update.ProgramPerChild.Update.Program.Code =
              entities.Program.Code;
            export.Export1.Item.ProgramPerChild.Next();
          }

          if (ReadMonitoredActivity())
          {
            export.Export1.Update.Start.Date =
              entities.MonitoredActivity.StartDate;

            if (Equal(entities.MonitoredActivity.ClosureDate,
              local.Initialized.Date) || Equal
              (entities.MonitoredActivity.ClosureDate, local.MaxDate.Date))
            {
              // Indicates that the monitored activity has not been closed,
              // therefore AP has not been located.
              export.ActivityLiteral.Text16 = "Required Cmpt Dt";
              export.Export1.Update.Compliance.Date =
                entities.MonitoredActivity.FedNonComplianceDate;

              if (!Lt(local.Current.Date,
                entities.MonitoredActivity.FedNonComplianceDate))
              {
                export.Export1.Update.FedCompliance.Flag = "N";
                export.Export1.Update.DaysRemainInCompli.Count = 0;
              }
              else
              {
                export.Export1.Update.FedCompliance.Flag = "";
                export.Export1.Update.DaysRemainInCompli.Count =
                  DaysFromAD(entities.MonitoredActivity.FedNonComplianceDate) -
                  DaysFromAD(local.Current.Date);
              }
            }
            else
            {
              // --------------
              // Indicates monitored activity has been closed, and other fields 
              // populated (ap address, etc.) indicate whether or not he's been
              // found.   Therefore, compliance indicator is set based on
              // whether the monitored activity was closed on or before
              // compliance date.
              // --------------
              export.ActivityLiteral.Text16 = " Activity Closed";
              export.Export1.Update.Compliance.Date =
                entities.MonitoredActivity.ClosureDate;

              if (Lt(entities.MonitoredActivity.FedNonComplianceDate,
                entities.MonitoredActivity.ClosureDate))
              {
                export.Export1.Update.FedCompliance.Flag = "N";
              }
              else
              {
                export.Export1.Update.FedCompliance.Flag = "Y";
              }

              export.Export1.Update.DaysRemainInCompli.Count = 0;
            }
          }

          export.Export1.Next();
        }

        if (Equal(import.HiddenPassedReviewType.ActionEntry, "R"))
        {
          // *** Work request 000170
          // *** 08/08/00 SWSRCHF
          // *** start
          local.Work.Index = -1;

          foreach(var item in ReadNarrativeDetail())
          {
            ++local.Work.Index;
            local.Work.CheckSize();

            local.Work.Update.Work1.NarrativeText =
              entities.EstablishmentReview.NarrativeText;

            if (local.Work.Index == 3)
            {
              break;
            }
          }

          if (!local.Work.IsEmpty)
          {
            local.Work.Index = 0;
            local.Work.CheckSize();

            export.EstReview.Text =
              Substring(local.Work.Item.Work1.NarrativeText, 18, 51);

            local.Work.Index = 1;
            local.Work.CheckSize();

            if (!IsEmpty(local.Work.Item.Work1.NarrativeText))
            {
              export.EstReview.Text = TrimEnd(export.EstReview.Text) + Substring
                (local.Work.Item.Work1.NarrativeText, 68, 18, 51);
            }

            local.Work.Index = 2;
            local.Work.CheckSize();

            if (!IsEmpty(local.Work.Item.Work1.NarrativeText))
            {
              export.EstReview.Text = TrimEnd(export.EstReview.Text) + Substring
                (local.Work.Item.Work1.NarrativeText, 68, 18, 51);
            }

            local.Work.Index = 3;
            local.Work.CheckSize();

            if (!IsEmpty(local.Work.Item.Work1.NarrativeText))
            {
              export.EstReview.Text = TrimEnd(export.EstReview.Text) + Substring
                (local.Work.Item.Work1.NarrativeText, 68, 18, 51);
            }

            var field = GetField(export.EstReview, "text");

            field.Color = "cyan";
            field.Protected = true;
          }

          // *** end
          // *** 08/08/00 SWSRCHF
          // *** Work request 000170
        }
        else
        {
          export.HiddenPass.Index = 3;
          export.HiddenPass.CheckSize();

          export.EstReview.Text = export.HiddenPass.Item.GexportH.Text;
        }

        if (IsEmpty(export.EstReview.Text))
        {
          ExitState = "SP0000_FIRST_REVIEW_4_CASE";
        }
        else if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }

        break;
      case "ENTER":
        if (Equal(import.HiddenPassedReviewType.ActionEntry, "M") || Equal
          (import.HiddenPassedReviewType.ActionEntry, "P"))
        {
          if (IsEmpty(export.EstReview.Text))
          {
            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

            var field = GetField(export.EstReview, "text");

            field.Error = true;

            return;
          }
          else
          {
            export.HiddenPass.Index = 3;
            export.HiddenPass.CheckSize();

            export.HiddenPass.Update.GexportH.Text = export.EstReview.Text;
          }
        }

        ExitState = "ECO_LNK_TO_CR_ENFORCEMENT";

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

    if (AsChar(export.CaseClosedIndicator.Flag) == 'Y')
    {
      var field = GetField(export.EstReview, "text");

      field.Color = "cyan";
      field.Protected = true;
    }
  }

  private static void MoveCaseRole(CaseRole source, CaseRole target)
  {
    target.ArWaivedInsurance = source.ArWaivedInsurance;
    target.FcParentalRights = source.FcParentalRights;
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
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveCsePersonsWorkSet3(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveHealthInsuranceViability(
    HealthInsuranceViability source, HealthInsuranceViability target)
  {
    target.HinsViableInd = source.HinsViableInd;
    target.HinsViableIndUpdatedDate = source.HinsViableIndUpdatedDate;
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

    useImport.CsePersonsWorkSet.Number = import.Ar.Number;
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

  private void UseSpCabIndSupportAndJugmnt()
  {
    var useImport = new SpCabIndSupportAndJugmnt.Import();
    var useExport = new SpCabIndSupportAndJugmnt.Export();

    useImport.Child.Number = entities.Child1.Number;
    useImport.Ap.Number = export.Ap1.Number;

    Call(SpCabIndSupportAndJugmnt.Execute, useImport, useExport);

    local.ChJudgement.Flag = useExport.JudgementChild.Flag;
    local.ChMedicalSupport.Flag = useExport.MedicalSupportChild.Flag;
    local.ChCurrentSupport.Flag = useExport.CurrentSupportChild.Flag;
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
        entities.Case1.StatusDate = db.GetNullableDate(reader, 1);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCsePerson1()
  {
    entities.Ar.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.OpenOrClosed.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Ar.Number = db.GetString(reader, 0);
        entities.Ar.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCsePerson2()
  {
    entities.Ap.Populated = false;

    return ReadEach("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.OpenOrClosed.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Ap.Number = db.GetString(reader, 0);
        entities.Ap.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePersonChild()
  {
    return ReadEach("ReadCsePersonChild",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.OpenOrClosed.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.Child1.Number = db.GetString(reader, 0);
        entities.Child2.CspNumber = db.GetString(reader, 0);
        entities.Child2.CasNumber = db.GetString(reader, 1);
        entities.Child2.Type1 = db.GetString(reader, 2);
        entities.Child2.Identifier = db.GetInt32(reader, 3);
        entities.Child2.StartDate = db.GetNullableDate(reader, 4);
        entities.Child2.EndDate = db.GetNullableDate(reader, 5);
        entities.Child2.ArWaivedInsurance = db.GetNullableString(reader, 6);
        entities.Child2.FcParentalRights = db.GetNullableString(reader, 7);
        entities.Child1.Populated = true;
        entities.Child2.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Child2.Type1);

        return true;
      });
  }

  private bool ReadHealthInsuranceCoverage()
  {
    System.Diagnostics.Debug.Assert(entities.PersonalHealthInsurance.Populated);
    entities.HealthInsuranceCoverage.Populated = false;

    return Read("ReadHealthInsuranceCoverage",
      (db, command) =>
      {
        db.SetInt64(
          command, "identifier", entities.PersonalHealthInsurance.HcvId);
        db.SetNullableDate(
          command, "policyExpDate",
          local.OpenOrClosed.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.HealthInsuranceCoverage.Identifier = db.GetInt64(reader, 0);
        entities.HealthInsuranceCoverage.PolicyExpirationDate =
          db.GetNullableDate(reader, 1);
        entities.HealthInsuranceCoverage.Populated = true;
      });
  }

  private bool ReadHealthInsuranceViability()
  {
    System.Diagnostics.Debug.Assert(entities.Child2.Populated);
    entities.HealthInsuranceViability.Populated = false;

    return Read("ReadHealthInsuranceViability",
      (db, command) =>
      {
        db.SetInt32(command, "croIdentifier", entities.Child2.Identifier);
        db.SetString(command, "croType", entities.Child2.Type1);
        db.SetString(command, "casNumber", entities.Child2.CasNumber);
        db.SetString(command, "cspNumber", entities.Child2.CspNumber);
        db.SetNullableString(command, "cspNum", export.Ap1.Number);
      },
      (db, reader) =>
      {
        entities.HealthInsuranceViability.CroType = db.GetString(reader, 0);
        entities.HealthInsuranceViability.CspNumber = db.GetString(reader, 1);
        entities.HealthInsuranceViability.CasNumber = db.GetString(reader, 2);
        entities.HealthInsuranceViability.CroIdentifier =
          db.GetInt32(reader, 3);
        entities.HealthInsuranceViability.Identifier = db.GetInt32(reader, 4);
        entities.HealthInsuranceViability.HinsViableInd =
          db.GetNullableString(reader, 5);
        entities.HealthInsuranceViability.HinsViableIndUpdatedDate =
          db.GetNullableDate(reader, 6);
        entities.HealthInsuranceViability.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.HealthInsuranceViability.LastUpdatedTimestamp =
          db.GetDateTime(reader, 8);
        entities.HealthInsuranceViability.CspNum =
          db.GetNullableString(reader, 9);
        entities.HealthInsuranceViability.Populated = true;
        CheckValid<HealthInsuranceViability>("CroType",
          entities.HealthInsuranceViability.CroType);
      });
  }

  private IEnumerable<bool> ReadLegalReferral()
  {
    entities.LegalReferral.Populated = false;

    return ReadEach("ReadLegalReferral",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "cspNumber", export.Ap1.Number);
      },
      (db, reader) =>
      {
        entities.LegalReferral.CasNumber = db.GetString(reader, 0);
        entities.LegalReferral.Identifier = db.GetInt32(reader, 1);
        entities.LegalReferral.Status = db.GetNullableString(reader, 2);
        entities.LegalReferral.ReferralDate = db.GetDate(reader, 3);
        entities.LegalReferral.ReferralReason1 = db.GetString(reader, 4);
        entities.LegalReferral.ReferralReason2 = db.GetString(reader, 5);
        entities.LegalReferral.ReferralReason3 = db.GetString(reader, 6);
        entities.LegalReferral.ReferralReason4 = db.GetString(reader, 7);
        entities.LegalReferral.ReferralReason5 = db.GetString(reader, 8);
        entities.LegalReferral.Populated = true;

        return true;
      });
  }

  private bool ReadMonitoredActivity()
  {
    entities.MonitoredActivity.Populated = false;

    return Read("ReadMonitoredActivity",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", export.Case1.Number);
        db.SetNullableString(command, "csePersonNum", export.Ap1.Number);
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

  private IEnumerable<bool> ReadNarrativeDetail()
  {
    entities.EstablishmentReview.Populated = false;

    return ReadEach("ReadNarrativeDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "infrastructureId",
          export.HiddenPass1.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.EstablishmentReview.InfrastructureId = db.GetInt32(reader, 0);
        entities.EstablishmentReview.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.EstablishmentReview.NarrativeText =
          db.GetNullableString(reader, 2);
        entities.EstablishmentReview.LineNumber = db.GetInt32(reader, 3);
        entities.EstablishmentReview.Populated = true;

        return true;
      });
  }

  private bool ReadPersonalHealthInsurance()
  {
    entities.PersonalHealthInsurance.Populated = false;

    return Read("ReadPersonalHealthInsurance",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Child1.Number);
        db.SetNullableDate(
          command, "coverBeginDate",
          local.OpenOrClosed.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonalHealthInsurance.HcvId = db.GetInt64(reader, 0);
        entities.PersonalHealthInsurance.CspNumber = db.GetString(reader, 1);
        entities.PersonalHealthInsurance.VerifiedUserId =
          db.GetNullableString(reader, 2);
        entities.PersonalHealthInsurance.CoverageVerifiedDate =
          db.GetNullableDate(reader, 3);
        entities.PersonalHealthInsurance.CoverageBeginDate =
          db.GetNullableDate(reader, 4);
        entities.PersonalHealthInsurance.CoverageEndDate =
          db.GetNullableDate(reader, 5);
        entities.PersonalHealthInsurance.Populated = true;
      });
  }

  private IEnumerable<bool> ReadProgram()
  {
    return ReadEach("ReadProgram",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          local.OpenOrClosed.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        if (export.Export1.Item.ProgramPerChild.IsFull)
        {
          return false;
        }

        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Program.Code = db.GetString(reader, 1);
        entities.Program.Populated = true;

        return true;
      });
  }

  private bool ReadServiceProcess()
  {
    entities.ServiceProcess.Populated = false;

    return Read("ReadServiceProcess",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNumber", export.Ap1.Number);
      },
      (db, reader) =>
      {
        entities.ServiceProcess.LgaIdentifier = db.GetInt32(reader, 0);
        entities.ServiceProcess.MethodOfService = db.GetString(reader, 1);
        entities.ServiceProcess.ServiceDate = db.GetDate(reader, 2);
        entities.ServiceProcess.CreatedBy = db.GetString(reader, 3);
        entities.ServiceProcess.CreatedTstamp = db.GetDateTime(reader, 4);
        entities.ServiceProcess.Identifier = db.GetInt32(reader, 5);
        entities.ServiceProcess.Populated = true;
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

    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
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
      /// A value of ChildHlthInsuAvail.
      /// </summary>
      [JsonPropertyName("childHlthInsuAvail")]
      public Common ChildHlthInsuAvail
      {
        get => childHlthInsuAvail ??= new();
        set => childHlthInsuAvail = value;
      }

      /// <summary>
      /// A value of MedicalSupportAr.
      /// </summary>
      [JsonPropertyName("medicalSupportAr")]
      public Common MedicalSupportAr
      {
        get => medicalSupportAr ??= new();
        set => medicalSupportAr = value;
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
      /// A value of HealthInsuranceViability.
      /// </summary>
      [JsonPropertyName("healthInsuranceViability")]
      public HealthInsuranceViability HealthInsuranceViability
      {
        get => healthInsuranceViability ??= new();
        set => healthInsuranceViability = value;
      }

      /// <summary>
      /// A value of MedicalInsViable2.
      /// </summary>
      [JsonPropertyName("medicalInsViable2")]
      public Common MedicalInsViable2
      {
        get => medicalInsViable2 ??= new();
        set => medicalInsViable2 = value;
      }

      /// <summary>
      /// A value of MedicalInsViable1.
      /// </summary>
      [JsonPropertyName("medicalInsViable1")]
      public Common MedicalInsViable1
      {
        get => medicalInsViable1 ??= new();
        set => medicalInsViable1 = value;
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
      /// A value of Ap1ResponsibleParty.
      /// </summary>
      [JsonPropertyName("ap1ResponsibleParty")]
      public Common Ap1ResponsibleParty
      {
        get => ap1ResponsibleParty ??= new();
        set => ap1ResponsibleParty = value;
      }

      /// <summary>
      /// A value of Ap2ResponsibleParty.
      /// </summary>
      [JsonPropertyName("ap2ResponsibleParty")]
      public Common Ap2ResponsibleParty
      {
        get => ap2ResponsibleParty ??= new();
        set => ap2ResponsibleParty = value;
      }

      /// <summary>
      /// A value of Ap2RightsSevered.
      /// </summary>
      [JsonPropertyName("ap2RightsSevered")]
      public Common Ap2RightsSevered
      {
        get => ap2RightsSevered ??= new();
        set => ap2RightsSevered = value;
      }

      /// <summary>
      /// A value of Ap1RightsSevered.
      /// </summary>
      [JsonPropertyName("ap1RightsSevered")]
      public Common Ap1RightsSevered
      {
        get => ap1RightsSevered ??= new();
        set => ap1RightsSevered = value;
      }

      /// <summary>
      /// A value of ReferredToLegalAp2.
      /// </summary>
      [JsonPropertyName("referredToLegalAp2")]
      public Common ReferredToLegalAp2
      {
        get => referredToLegalAp2 ??= new();
        set => referredToLegalAp2 = value;
      }

      /// <summary>
      /// A value of MedicalCoverageAp2.
      /// </summary>
      [JsonPropertyName("medicalCoverageAp2")]
      public Common MedicalCoverageAp2
      {
        get => medicalCoverageAp2 ??= new();
        set => medicalCoverageAp2 = value;
      }

      /// <summary>
      /// A value of JudgementAp2.
      /// </summary>
      [JsonPropertyName("judgementAp2")]
      public Common JudgementAp2
      {
        get => judgementAp2 ??= new();
        set => judgementAp2 = value;
      }

      /// <summary>
      /// A value of MedicalSupportAp2.
      /// </summary>
      [JsonPropertyName("medicalSupportAp2")]
      public Common MedicalSupportAp2
      {
        get => medicalSupportAp2 ??= new();
        set => medicalSupportAp2 = value;
      }

      /// <summary>
      /// A value of CurrentSupportAp2.
      /// </summary>
      [JsonPropertyName("currentSupportAp2")]
      public Common CurrentSupportAp2
      {
        get => currentSupportAp2 ??= new();
        set => currentSupportAp2 = value;
      }

      /// <summary>
      /// A value of MedicalProvider.
      /// </summary>
      [JsonPropertyName("medicalProvider")]
      public CsePersonsWorkSet MedicalProvider
      {
        get => medicalProvider ??= new();
        set => medicalProvider = value;
      }

      /// <summary>
      /// Gets a value of ProgramPerChild.
      /// </summary>
      [JsonIgnore]
      public Array<ProgramPerChildGroup> ProgramPerChild =>
        programPerChild ??= new(ProgramPerChildGroup.Capacity);

      /// <summary>
      /// Gets a value of ProgramPerChild for json serialization.
      /// </summary>
      [JsonPropertyName("programPerChild")]
      [Computed]
      public IList<ProgramPerChildGroup> ProgramPerChild_Json
      {
        get => programPerChild;
        set => ProgramPerChild.Assign(value);
      }

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
      /// A value of Start.
      /// </summary>
      [JsonPropertyName("start")]
      public DateWorkArea Start
      {
        get => start ??= new();
        set => start = value;
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
      /// A value of MedicalCoverage.
      /// </summary>
      [JsonPropertyName("medicalCoverage")]
      public Common MedicalCoverage
      {
        get => medicalCoverage ??= new();
        set => medicalCoverage = value;
      }

      /// <summary>
      /// A value of Judgement.
      /// </summary>
      [JsonPropertyName("judgement")]
      public Common Judgement
      {
        get => judgement ??= new();
        set => judgement = value;
      }

      /// <summary>
      /// A value of MedicalSupport.
      /// </summary>
      [JsonPropertyName("medicalSupport")]
      public Common MedicalSupport
      {
        get => medicalSupport ??= new();
        set => medicalSupport = value;
      }

      /// <summary>
      /// A value of CurrentSupport.
      /// </summary>
      [JsonPropertyName("currentSupport")]
      public Common CurrentSupport
      {
        get => currentSupport ??= new();
        set => currentSupport = value;
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
      /// A value of FedCompliance.
      /// </summary>
      [JsonPropertyName("fedCompliance")]
      public Common FedCompliance
      {
        get => fedCompliance ??= new();
        set => fedCompliance = value;
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
      /// A value of Import4.
      /// </summary>
      [JsonPropertyName("import4")]
      public Program Import4
      {
        get => import4 ??= new();
        set => import4 = value;
      }

      /// <summary>
      /// A value of Import3.
      /// </summary>
      [JsonPropertyName("import3")]
      public Program Import3
      {
        get => import3 ??= new();
        set => import3 = value;
      }

      /// <summary>
      /// A value of Import2.
      /// </summary>
      [JsonPropertyName("import2")]
      public Program Import2
      {
        get => import2 ??= new();
        set => import2 = value;
      }

      /// <summary>
      /// A value of Import5.
      /// </summary>
      [JsonPropertyName("import5")]
      public Program Import5
      {
        get => import5 ??= new();
        set => import5 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 15;

      private LegalReferral legalReferral;
      private Common childHlthInsuAvail;
      private Common medicalSupportAr;
      private Common common;
      private HealthInsuranceViability healthInsuranceViability;
      private Common medicalInsViable2;
      private Common medicalInsViable1;
      private Common childAge;
      private Common ap1ResponsibleParty;
      private Common ap2ResponsibleParty;
      private Common ap2RightsSevered;
      private Common ap1RightsSevered;
      private Common referredToLegalAp2;
      private Common medicalCoverageAp2;
      private Common judgementAp2;
      private Common medicalSupportAp2;
      private Common currentSupportAp2;
      private CsePersonsWorkSet medicalProvider;
      private Array<ProgramPerChildGroup> programPerChild;
      private DateWorkArea compliance;
      private DateWorkArea start;
      private CaseRole child1;
      private Common medicalCoverage;
      private Common judgement;
      private Common medicalSupport;
      private Common currentSupport;
      private Common daysRemainInCompli;
      private Common fedCompliance;
      private Common referredToLegal;
      private CaseRole childCaseRole;
      private CsePersonsWorkSet childCsePersonsWorkSet;
      private Program import4;
      private Program import3;
      private Program import2;
      private Program import5;
    }

    /// <summary>A ProgramPerChildGroup group.</summary>
    [Serializable]
    public class ProgramPerChildGroup
    {
      /// <summary>
      /// A value of Program.
      /// </summary>
      [JsonPropertyName("program")]
      public Program Program
      {
        get => program ??= new();
        set => program = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 8;

      private Program program;
    }

    /// <summary>
    /// A value of ApServed.
    /// </summary>
    [JsonPropertyName("apServed")]
    public ServiceProcess ApServed
    {
      get => apServed ??= new();
      set => apServed = value;
    }

    /// <summary>
    /// A value of ServiceDate.
    /// </summary>
    [JsonPropertyName("serviceDate")]
    public DateWorkArea ServiceDate
    {
      get => serviceDate ??= new();
      set => serviceDate = value;
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
    /// A value of EstReview.
    /// </summary>
    [JsonPropertyName("estReview")]
    public NarrativeWork EstReview
    {
      get => estReview ??= new();
      set => estReview = value;
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
    /// A value of JudgementAp1.
    /// </summary>
    [JsonPropertyName("judgementAp1")]
    public Common JudgementAp1
    {
      get => judgementAp1 ??= new();
      set => judgementAp1 = value;
    }

    /// <summary>
    /// A value of ImporMedicalSupportAp1.
    /// </summary>
    [JsonPropertyName("imporMedicalSupportAp1")]
    public Common ImporMedicalSupportAp1
    {
      get => imporMedicalSupportAp1 ??= new();
      set => imporMedicalSupportAp1 = value;
    }

    /// <summary>
    /// A value of CurrentSupportAp1.
    /// </summary>
    [JsonPropertyName("currentSupportAp1")]
    public Common CurrentSupportAp1
    {
      get => currentSupportAp1 ??= new();
      set => currentSupportAp1 = value;
    }

    /// <summary>
    /// A value of JudgementAp2.
    /// </summary>
    [JsonPropertyName("judgementAp2")]
    public Common JudgementAp2
    {
      get => judgementAp2 ??= new();
      set => judgementAp2 = value;
    }

    /// <summary>
    /// A value of MedicalSupportAp2.
    /// </summary>
    [JsonPropertyName("medicalSupportAp2")]
    public Common MedicalSupportAp2
    {
      get => medicalSupportAp2 ??= new();
      set => medicalSupportAp2 = value;
    }

    /// <summary>
    /// A value of CurrentSupportAp2.
    /// </summary>
    [JsonPropertyName("currentSupportAp2")]
    public Common CurrentSupportAp2
    {
      get => currentSupportAp2 ??= new();
      set => currentSupportAp2 = value;
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
    /// A value of Ap2.
    /// </summary>
    [JsonPropertyName("ap2")]
    public CsePersonsWorkSet Ap2
    {
      get => ap2 ??= new();
      set => ap2 = value;
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
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePersonsWorkSet Ar
    {
      get => ar ??= new();
      set => ar = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of CaseClosedIndicator.
    /// </summary>
    [JsonPropertyName("caseClosedIndicator")]
    public Common CaseClosedIndicator
    {
      get => caseClosedIndicator ??= new();
      set => caseClosedIndicator = value;
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

    private ServiceProcess apServed;
    private DateWorkArea serviceDate;
    private TextWorkArea moreApsMsg;
    private NarrativeWork estReview;
    private Infrastructure hiddenPass1;
    private Array<HiddenPassGroup> hiddenPass;
    private Common judgementAp1;
    private Common imporMedicalSupportAp1;
    private Common currentSupportAp1;
    private Common judgementAp2;
    private Common medicalSupportAp2;
    private Common currentSupportAp2;
    private LegalReferral legalReferral;
    private CsePersonsWorkSet ap2;
    private CsePersonsWorkSet ap1;
    private CsePersonsWorkSet ar;
    private Array<ImportGroup> import1;
    private Case1 case1;
    private Common hiddenPassedReviewType;
    private NextTranInfo hidden;
    private Standard standard;
    private Common caseClosedIndicator;
    private WorkArea activityLiteral;
    private Common multiAp;
    private Common apSelected;
    private CsePersonsWorkSet selectedAp;
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

    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
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
      /// A value of ChildHlthInsuAvail.
      /// </summary>
      [JsonPropertyName("childHlthInsuAvail")]
      public Common ChildHlthInsuAvail
      {
        get => childHlthInsuAvail ??= new();
        set => childHlthInsuAvail = value;
      }

      /// <summary>
      /// A value of MedicalSupportAr.
      /// </summary>
      [JsonPropertyName("medicalSupportAr")]
      public Common MedicalSupportAr
      {
        get => medicalSupportAr ??= new();
        set => medicalSupportAr = value;
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
      /// A value of HealthInsuranceViability.
      /// </summary>
      [JsonPropertyName("healthInsuranceViability")]
      public HealthInsuranceViability HealthInsuranceViability
      {
        get => healthInsuranceViability ??= new();
        set => healthInsuranceViability = value;
      }

      /// <summary>
      /// A value of MedicalInsViable2.
      /// </summary>
      [JsonPropertyName("medicalInsViable2")]
      public Common MedicalInsViable2
      {
        get => medicalInsViable2 ??= new();
        set => medicalInsViable2 = value;
      }

      /// <summary>
      /// A value of MedicalInsViable1.
      /// </summary>
      [JsonPropertyName("medicalInsViable1")]
      public Common MedicalInsViable1
      {
        get => medicalInsViable1 ??= new();
        set => medicalInsViable1 = value;
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
      /// A value of Ap1ResponsibleParty.
      /// </summary>
      [JsonPropertyName("ap1ResponsibleParty")]
      public Common Ap1ResponsibleParty
      {
        get => ap1ResponsibleParty ??= new();
        set => ap1ResponsibleParty = value;
      }

      /// <summary>
      /// A value of Ap2ResponsibleParty.
      /// </summary>
      [JsonPropertyName("ap2ResponsibleParty")]
      public Common Ap2ResponsibleParty
      {
        get => ap2ResponsibleParty ??= new();
        set => ap2ResponsibleParty = value;
      }

      /// <summary>
      /// A value of Ap2RightsSevered.
      /// </summary>
      [JsonPropertyName("ap2RightsSevered")]
      public Common Ap2RightsSevered
      {
        get => ap2RightsSevered ??= new();
        set => ap2RightsSevered = value;
      }

      /// <summary>
      /// A value of Ap1RightsSevered.
      /// </summary>
      [JsonPropertyName("ap1RightsSevered")]
      public Common Ap1RightsSevered
      {
        get => ap1RightsSevered ??= new();
        set => ap1RightsSevered = value;
      }

      /// <summary>
      /// A value of ReferredToLegalAp2.
      /// </summary>
      [JsonPropertyName("referredToLegalAp2")]
      public Common ReferredToLegalAp2
      {
        get => referredToLegalAp2 ??= new();
        set => referredToLegalAp2 = value;
      }

      /// <summary>
      /// A value of MedicalCoverageAp2.
      /// </summary>
      [JsonPropertyName("medicalCoverageAp2")]
      public Common MedicalCoverageAp2
      {
        get => medicalCoverageAp2 ??= new();
        set => medicalCoverageAp2 = value;
      }

      /// <summary>
      /// A value of JudgementAp2.
      /// </summary>
      [JsonPropertyName("judgementAp2")]
      public Common JudgementAp2
      {
        get => judgementAp2 ??= new();
        set => judgementAp2 = value;
      }

      /// <summary>
      /// A value of MedicalSupportAp2.
      /// </summary>
      [JsonPropertyName("medicalSupportAp2")]
      public Common MedicalSupportAp2
      {
        get => medicalSupportAp2 ??= new();
        set => medicalSupportAp2 = value;
      }

      /// <summary>
      /// A value of CurrentSupportAp2.
      /// </summary>
      [JsonPropertyName("currentSupportAp2")]
      public Common CurrentSupportAp2
      {
        get => currentSupportAp2 ??= new();
        set => currentSupportAp2 = value;
      }

      /// <summary>
      /// A value of MedicalProvider.
      /// </summary>
      [JsonPropertyName("medicalProvider")]
      public CsePersonsWorkSet MedicalProvider
      {
        get => medicalProvider ??= new();
        set => medicalProvider = value;
      }

      /// <summary>
      /// Gets a value of ProgramPerChild.
      /// </summary>
      [JsonIgnore]
      public Array<ProgramPerChildGroup> ProgramPerChild =>
        programPerChild ??= new(ProgramPerChildGroup.Capacity);

      /// <summary>
      /// Gets a value of ProgramPerChild for json serialization.
      /// </summary>
      [JsonPropertyName("programPerChild")]
      [Computed]
      public IList<ProgramPerChildGroup> ProgramPerChild_Json
      {
        get => programPerChild;
        set => ProgramPerChild.Assign(value);
      }

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
      /// A value of Start.
      /// </summary>
      [JsonPropertyName("start")]
      public DateWorkArea Start
      {
        get => start ??= new();
        set => start = value;
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
      /// A value of MedicalCoverage.
      /// </summary>
      [JsonPropertyName("medicalCoverage")]
      public Common MedicalCoverage
      {
        get => medicalCoverage ??= new();
        set => medicalCoverage = value;
      }

      /// <summary>
      /// A value of Judgement.
      /// </summary>
      [JsonPropertyName("judgement")]
      public Common Judgement
      {
        get => judgement ??= new();
        set => judgement = value;
      }

      /// <summary>
      /// A value of MedicalSupport.
      /// </summary>
      [JsonPropertyName("medicalSupport")]
      public Common MedicalSupport
      {
        get => medicalSupport ??= new();
        set => medicalSupport = value;
      }

      /// <summary>
      /// A value of CurrentSupport.
      /// </summary>
      [JsonPropertyName("currentSupport")]
      public Common CurrentSupport
      {
        get => currentSupport ??= new();
        set => currentSupport = value;
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
      /// A value of FedCompliance.
      /// </summary>
      [JsonPropertyName("fedCompliance")]
      public Common FedCompliance
      {
        get => fedCompliance ??= new();
        set => fedCompliance = value;
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
      /// A value of Export4.
      /// </summary>
      [JsonPropertyName("export4")]
      public Program Export4
      {
        get => export4 ??= new();
        set => export4 = value;
      }

      /// <summary>
      /// A value of Export3.
      /// </summary>
      [JsonPropertyName("export3")]
      public Program Export3
      {
        get => export3 ??= new();
        set => export3 = value;
      }

      /// <summary>
      /// A value of Export2.
      /// </summary>
      [JsonPropertyName("export2")]
      public Program Export2
      {
        get => export2 ??= new();
        set => export2 = value;
      }

      /// <summary>
      /// A value of Export5.
      /// </summary>
      [JsonPropertyName("export5")]
      public Program Export5
      {
        get => export5 ??= new();
        set => export5 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 15;

      private LegalReferral legalReferral;
      private Common childHlthInsuAvail;
      private Common medicalSupportAr;
      private Common common;
      private HealthInsuranceViability healthInsuranceViability;
      private Common medicalInsViable2;
      private Common medicalInsViable1;
      private Common childAge;
      private Common ap1ResponsibleParty;
      private Common ap2ResponsibleParty;
      private Common ap2RightsSevered;
      private Common ap1RightsSevered;
      private Common referredToLegalAp2;
      private Common medicalCoverageAp2;
      private Common judgementAp2;
      private Common medicalSupportAp2;
      private Common currentSupportAp2;
      private CsePersonsWorkSet medicalProvider;
      private Array<ProgramPerChildGroup> programPerChild;
      private DateWorkArea compliance;
      private DateWorkArea start;
      private CaseRole child1;
      private Common medicalCoverage;
      private Common judgement;
      private Common medicalSupport;
      private Common currentSupport;
      private Common daysRemainInCompli;
      private Common fedCompliance;
      private Common referredToLegal;
      private CaseRole childCaseRole;
      private CsePersonsWorkSet childCsePersonsWorkSet;
      private Program export4;
      private Program export3;
      private Program export2;
      private Program export5;
    }

    /// <summary>A ProgramPerChildGroup group.</summary>
    [Serializable]
    public class ProgramPerChildGroup
    {
      /// <summary>
      /// A value of Program.
      /// </summary>
      [JsonPropertyName("program")]
      public Program Program
      {
        get => program ??= new();
        set => program = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 8;

      private Program program;
    }

    /// <summary>A ChildGroup group.</summary>
    [Serializable]
    public class ChildGroup
    {
      /// <summary>
      /// A value of Compliance1.
      /// </summary>
      [JsonPropertyName("compliance1")]
      public DateWorkArea Compliance1
      {
        get => compliance1 ??= new();
        set => compliance1 = value;
      }

      /// <summary>
      /// A value of Export1Common.
      /// </summary>
      [JsonPropertyName("export1Common")]
      public Common Export1Common
      {
        get => export1Common ??= new();
        set => export1Common = value;
      }

      /// <summary>
      /// A value of Export1Child.
      /// </summary>
      [JsonPropertyName("export1Child")]
      public CaseRole Export1Child
      {
        get => export1Child ??= new();
        set => export1Child = value;
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
      /// A value of Export1LegalReferral.
      /// </summary>
      [JsonPropertyName("export1LegalReferral")]
      public LegalReferral Export1LegalReferral
      {
        get => export1LegalReferral ??= new();
        set => export1LegalReferral = value;
      }

      /// <summary>
      /// A value of ReferredToLegal1.
      /// </summary>
      [JsonPropertyName("referredToLegal1")]
      public Common ReferredToLegal1
      {
        get => referredToLegal1 ??= new();
        set => referredToLegal1 = value;
      }

      /// <summary>
      /// A value of FedCompliance1.
      /// </summary>
      [JsonPropertyName("fedCompliance1")]
      public Common FedCompliance1
      {
        get => fedCompliance1 ??= new();
        set => fedCompliance1 = value;
      }

      /// <summary>
      /// A value of ChildAge1.
      /// </summary>
      [JsonPropertyName("childAge1")]
      public Common ChildAge1
      {
        get => childAge1 ??= new();
        set => childAge1 = value;
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
      /// A value of Child1CaseRole.
      /// </summary>
      [JsonPropertyName("child1CaseRole")]
      public CaseRole Child1CaseRole
      {
        get => child1CaseRole ??= new();
        set => child1CaseRole = value;
      }

      /// <summary>
      /// A value of Child1CsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("child1CsePersonsWorkSet")]
      public CsePersonsWorkSet Child1CsePersonsWorkSet
      {
        get => child1CsePersonsWorkSet ??= new();
        set => child1CsePersonsWorkSet = value;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 15;

      private DateWorkArea compliance1;
      private Common export1Common;
      private CaseRole export1Child;
      private GeneticTest geneticTest;
      private LegalReferral export1LegalReferral;
      private Common referredToLegal1;
      private Common fedCompliance1;
      private Common childAge1;
      private CaseRole ap;
      private CaseRole child1CaseRole;
      private CsePersonsWorkSet child1CsePersonsWorkSet;
      private SpTextWorkArea chAgeMsgTxt;
    }

    /// <summary>
    /// A value of ApServed.
    /// </summary>
    [JsonPropertyName("apServed")]
    public ServiceProcess ApServed
    {
      get => apServed ??= new();
      set => apServed = value;
    }

    /// <summary>
    /// A value of ServiceDate.
    /// </summary>
    [JsonPropertyName("serviceDate")]
    public DateWorkArea ServiceDate
    {
      get => serviceDate ??= new();
      set => serviceDate = value;
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
    /// A value of MoreApsMsg.
    /// </summary>
    [JsonPropertyName("moreApsMsg")]
    public TextWorkArea MoreApsMsg
    {
      get => moreApsMsg ??= new();
      set => moreApsMsg = value;
    }

    /// <summary>
    /// A value of EstReview.
    /// </summary>
    [JsonPropertyName("estReview")]
    public NarrativeWork EstReview
    {
      get => estReview ??= new();
      set => estReview = value;
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
    /// A value of JudgementAp1.
    /// </summary>
    [JsonPropertyName("judgementAp1")]
    public Common JudgementAp1
    {
      get => judgementAp1 ??= new();
      set => judgementAp1 = value;
    }

    /// <summary>
    /// A value of MedicalSupportAp1.
    /// </summary>
    [JsonPropertyName("medicalSupportAp1")]
    public Common MedicalSupportAp1
    {
      get => medicalSupportAp1 ??= new();
      set => medicalSupportAp1 = value;
    }

    /// <summary>
    /// A value of CurrentSupportAp1.
    /// </summary>
    [JsonPropertyName("currentSupportAp1")]
    public Common CurrentSupportAp1
    {
      get => currentSupportAp1 ??= new();
      set => currentSupportAp1 = value;
    }

    /// <summary>
    /// A value of JudgementAp2.
    /// </summary>
    [JsonPropertyName("judgementAp2")]
    public Common JudgementAp2
    {
      get => judgementAp2 ??= new();
      set => judgementAp2 = value;
    }

    /// <summary>
    /// A value of MedicalSupportAp2.
    /// </summary>
    [JsonPropertyName("medicalSupportAp2")]
    public Common MedicalSupportAp2
    {
      get => medicalSupportAp2 ??= new();
      set => medicalSupportAp2 = value;
    }

    /// <summary>
    /// A value of CurrentSupportAp2.
    /// </summary>
    [JsonPropertyName("currentSupportAp2")]
    public Common CurrentSupportAp2
    {
      get => currentSupportAp2 ??= new();
      set => currentSupportAp2 = value;
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
    /// A value of LegalReferral.
    /// </summary>
    [JsonPropertyName("legalReferral")]
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
    }

    /// <summary>
    /// A value of Ap2.
    /// </summary>
    [JsonPropertyName("ap2")]
    public CsePersonsWorkSet Ap2
    {
      get => ap2 ??= new();
      set => ap2 = value;
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
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePersonsWorkSet Ar
    {
      get => ar ??= new();
      set => ar = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of CaseClosedIndicator.
    /// </summary>
    [JsonPropertyName("caseClosedIndicator")]
    public Common CaseClosedIndicator
    {
      get => caseClosedIndicator ??= new();
      set => caseClosedIndicator = value;
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
    /// A value of LocalReturnedAge.
    /// </summary>
    [JsonPropertyName("localReturnedAge")]
    public Common LocalReturnedAge
    {
      get => localReturnedAge ??= new();
      set => localReturnedAge = value;
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

    private ServiceProcess apServed;
    private DateWorkArea serviceDate;
    private CsePersonsWorkSet selectedChildCsePersonsWorkSet;
    private TextWorkArea moreApsMsg;
    private NarrativeWork estReview;
    private Infrastructure hiddenPass1;
    private Array<HiddenPassGroup> hiddenPass;
    private Common judgementAp1;
    private Common medicalSupportAp1;
    private Common currentSupportAp1;
    private Common judgementAp2;
    private Common medicalSupportAp2;
    private Common currentSupportAp2;
    private CsePerson selectedChildCsePerson;
    private LegalReferral legalReferral;
    private CsePersonsWorkSet ap2;
    private CsePersonsWorkSet ap1;
    private CsePersonsWorkSet ar;
    private Array<ExportGroup> export1;
    private Case1 case1;
    private Common hiddenPassedReviewType;
    private NextTranInfo hidden;
    private Standard standard;
    private Common caseClosedIndicator;
    private Array<ChildGroup> child;
    private Common localReturnedAge;
    private WorkArea activityLiteral;
    private Common multiAp;
    private Common apSelected;
    private CsePersonsWorkSet selectedAp;
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
    /// A value of OpenOrClosed.
    /// </summary>
    [JsonPropertyName("openOrClosed")]
    public DateWorkArea OpenOrClosed
    {
      get => openOrClosed ??= new();
      set => openOrClosed = value;
    }

    /// <summary>
    /// A value of ChJudgement.
    /// </summary>
    [JsonPropertyName("chJudgement")]
    public Common ChJudgement
    {
      get => chJudgement ??= new();
      set => chJudgement = value;
    }

    /// <summary>
    /// A value of ChMedicalSupport.
    /// </summary>
    [JsonPropertyName("chMedicalSupport")]
    public Common ChMedicalSupport
    {
      get => chMedicalSupport ??= new();
      set => chMedicalSupport = value;
    }

    /// <summary>
    /// A value of ChCurrentSupport.
    /// </summary>
    [JsonPropertyName("chCurrentSupport")]
    public Common ChCurrentSupport
    {
      get => chCurrentSupport ??= new();
      set => chCurrentSupport = value;
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
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DateWorkArea Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    /// <summary>
    /// A value of LegalActionFound.
    /// </summary>
    [JsonPropertyName("legalActionFound")]
    public Common LegalActionFound
    {
      get => legalActionFound ??= new();
      set => legalActionFound = value;
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
    /// A value of Ap2.
    /// </summary>
    [JsonPropertyName("ap2")]
    public CsePerson Ap2
    {
      get => ap2 ??= new();
      set => ap2 = value;
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
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public DateWorkArea MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

    private Array<WorkGroup> work;
    private DateWorkArea openOrClosed;
    private Common chJudgement;
    private Common chMedicalSupport;
    private Common chCurrentSupport;
    private DateWorkArea current;
    private DateWorkArea initialized;
    private Common legalActionFound;
    private CsePersonsWorkSet csePersonsWorkSet;
    private AbendData abendData;
    private CsePerson ap2;
    private CsePerson ap1;
    private DateWorkArea maxDate;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of EstablishmentReview.
    /// </summary>
    [JsonPropertyName("establishmentReview")]
    public NarrativeDetail EstablishmentReview
    {
      get => establishmentReview ??= new();
      set => establishmentReview = value;
    }

    /// <summary>
    /// A value of Child1.
    /// </summary>
    [JsonPropertyName("child1")]
    public CsePerson Child1
    {
      get => child1 ??= new();
      set => child1 = value;
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public LegalActionPerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("healthInsuranceCoverage")]
    public HealthInsuranceCoverage HealthInsuranceCoverage
    {
      get => healthInsuranceCoverage ??= new();
      set => healthInsuranceCoverage = value;
    }

    /// <summary>
    /// A value of PersonalHealthInsurance.
    /// </summary>
    [JsonPropertyName("personalHealthInsurance")]
    public PersonalHealthInsurance PersonalHealthInsurance
    {
      get => personalHealthInsurance ??= new();
      set => personalHealthInsurance = value;
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
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
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
    /// A value of AbsentParent.
    /// </summary>
    [JsonPropertyName("absentParent")]
    public CaseRole AbsentParent
    {
      get => absentParent ??= new();
      set => absentParent = value;
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
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePerson Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of HealthInsuranceViability.
    /// </summary>
    [JsonPropertyName("healthInsuranceViability")]
    public HealthInsuranceViability HealthInsuranceViability
    {
      get => healthInsuranceViability ??= new();
      set => healthInsuranceViability = value;
    }

    /// <summary>
    /// A value of Child2.
    /// </summary>
    [JsonPropertyName("child2")]
    public CaseRole Child2
    {
      get => child2 ??= new();
      set => child2 = value;
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
    /// A value of LegalReferral.
    /// </summary>
    [JsonPropertyName("legalReferral")]
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
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

    private NarrativeDetail establishmentReview;
    private CsePerson child1;
    private ServiceProcess serviceProcess;
    private CsePerson ap;
    private LegalActionPerson obligor;
    private LegalReferralCaseRole legalReferralCaseRole;
    private LegalAction legalAction;
    private ObligationType obligationType;
    private LegalActionCaseRole legalActionCaseRole;
    private HealthInsuranceCoverage healthInsuranceCoverage;
    private PersonalHealthInsurance personalHealthInsurance;
    private MonitoredActivity monitoredActivity;
    private Infrastructure infrastructure;
    private LegalActionPerson legalActionPerson;
    private LegalActionDetail legalActionDetail;
    private PersonProgram personProgram;
    private Program program;
    private CaseRole absentParent;
    private CaseRole applicantRecipient;
    private CsePerson ar;
    private HealthInsuranceViability healthInsuranceViability;
    private CaseRole child2;
    private Case1 case1;
    private LegalReferral legalReferral;
    private CsePerson csePerson;
    private CaseRole caseRole;
  }
#endregion
}
