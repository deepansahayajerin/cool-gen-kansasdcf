// Program: SP_CRMD_CASE_REVIEW_MEDICAL, ID: 372638156, model: 746.
// Short name: SWECRMDP
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
/// A program: SP_CRMD_CASE_REVIEW_MEDICAL.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpCrmdCaseReviewMedical: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CRMD_CASE_REVIEW_MEDICAL program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCrmdCaseReviewMedical(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCrmdCaseReviewMedical.
  /// </summary>
  public SpCrmdCaseReviewMedical(IContext context, Import import, Export export):
    
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
    // 10/30/95 J Rookard		 Remove ZDEL views
    // 03/14/96 Alan Hackler            Retro fits
    // 01/03/97 R. Marchman		 Add new security/next tran.
    // 01/10/97 R. Welborn              Conversion from plan taks note
    // 				 to Narrative/Infrastructure
    // 04/30/97 R. Grey                 Change Current Date
    // 05/14/97 R. Grey		 IDCR #328 Exit and other Messages
    // 06/11/97 R. Grey		 Add If AR is Org logic and clean up
    // 07/22/97 R. Grey		 Add review closed case
    // 03/16/99 Sury                    Rearrange and add new Screen fields and
    //                                  
    // Delete link to IWGL and Add link
    // to HICP
    // 06/15/99 Sury                    'No Insurance message' to be displayed
    //                                   
    // on initial Page
    // 02/03/00 Vithal Madhira	PR 86247 Modified code to implement the case 
    // review
    // 				 for each AP on a multiple AP case.
    // 08/08/00 SWSRCHF	WR# 170  Replace the read for Narrative by a read for
    // 				 Narrative Detail
    // 08/02/10 JHuss		CQ# 473	 Added sort criteria to read for HIC details/
    // obligor
    // 04/01/2011	T Pierce	CQ# 23212	Removed references to obsolete Narrative 
    // entity type.
    // -----------------------------------------------------------------------
    // 06/16/11  RMathews   CQ27977  Added narrative detail to export list for 
    // call to SP_CRMD_DISPLAY_CLOSED_CASE
    // 07/25/11  RMathews   CQ28992  Modified HIC responsible party lookup to 
    // prevent escape after first read
    ExitState = "ACO_NN0000_ALL_OK";
    local.Max.Date = UseCabSetMaximumDiscontinueDate();
    export.Hidden.Assign(import.Hidden);
    local.Current.Date = Now().Date;
    export.HiddenPass.SystemGeneratedIdentifier =
      import.HiddenPass.SystemGeneratedIdentifier;
    export.HiddenPassedReviewType.ActionEntry =
      import.HiddenPassedReviewType.ActionEntry;
    export.ApSelected.Flag = import.ApSelected.Flag;
    MoveCsePersonsWorkSet2(import.SelectedAp, export.SelectedAp);
    export.MultiAp.Flag = import.MultiAp.Flag;

    for(import.HiddenPassed.Index = 0; import.HiddenPassed.Index < import
      .HiddenPassed.Count; ++import.HiddenPassed.Index)
    {
      if (!import.HiddenPassed.CheckSize())
      {
        break;
      }

      export.HiddenPassed.Index = import.HiddenPassed.Index;
      export.HiddenPassed.CheckSize();

      export.HiddenPassed.Update.GexportH.Text =
        import.HiddenPassed.Item.GimportH.Text;

    }

    import.HiddenPassed.CheckIndex();
    export.Case1.Number = import.Case1.Number;
    export.CaseClosedIndicator.Flag = import.CaseClosedIndicator.Flag;
    export.Pgm1.Code = import.Pgm1.Code;
    export.Pgm2.Code = import.Pgm2.Code;
    export.Pgm3.Code = import.Pgm3.Code;
    export.Pgm4.Code = import.Pgm4.Code;
    export.Pgm5.Code = import.Pgm5.Code;
    export.Pgm6.Code = import.Pgm6.Code;
    export.Pgm7.Code = import.Pgm7.Code;
    export.Pgm8.Code = import.Pgm8.Code;
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.MoreApsMsg.Text30 = import.MoreApsMsg.Text30;
    MoveCsePersonsWorkSet2(import.Ar, export.Ar);
    MoveCsePersonsWorkSet2(import.Ap1, export.Ap1);
    export.MedReview.Text = import.MedReview.Text;

    export.Export1.Index = 0;
    export.Export1.Clear();

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (export.Export1.IsFull)
      {
        break;
      }

      export.Export1.Update.InsuAvailInd.Flag =
        import.Import1.Item.InsuAvailInd.Flag;
      export.Export1.Update.Common.SelectChar =
        import.Import1.Item.Common.SelectChar;

      if (Equal(global.Command, "RETLINK"))
      {
        if (AsChar(export.Export1.Item.Common.SelectChar) == '*' || AsChar
          (export.Export1.Item.Common.SelectChar) == 'S')
        {
          export.Export1.Update.Common.SelectChar = "";
        }
      }

      export.Export1.Update.Child1.Assign(import.Import1.Item.Child1);
      export.Export1.Update.Child2.Assign(import.Import1.Item.Child2);
      export.Export1.Update.HealthInsuranceViability.HinsViableInd =
        import.Import1.Item.HealthInsuranceViability.HinsViableInd;
      export.Export1.Update.ViableCsePers.FormattedName =
        import.Import1.Item.ViableCsePers.FormattedName;
      export.Export1.Update.MoreChWNoCovMsg.Text80 =
        import.Import1.Item.NoCoverageChildMsg.Text80;
      MoveTextWorkArea(import.Import1.Item.InsByApOrArNone,
        export.Export1.Update.InsByApOrArNone);
      export.Export1.Update.ChAge.TotalInteger =
        import.Import1.Item.ChAge.TotalInteger;
      export.Export1.Update.AgeText.Text4 = import.Import1.Item.AgeText.Text4;
      export.Export1.Update.ApCourtOrdHic.Text4 =
        import.Import1.Item.CourtOrdHic.Text4;
      export.Export1.Update.ApCourtOrdMs.Text4 =
        import.Import1.Item.CourtOrdMs.Text4;
      export.Export1.Update.ApCourtOrdMc.Text4 =
        import.Import1.Item.CourtOrdMc.Text4;

      export.Export1.Item.MedHinsProvider.Index = 0;
      export.Export1.Item.MedHinsProvider.Clear();

      for(import.Import1.Item.MedHinsProvider.Index = 0; import
        .Import1.Item.MedHinsProvider.Index < import
        .Import1.Item.MedHinsProvider.Count; ++
        import.Import1.Item.MedHinsProvider.Index)
      {
        if (export.Export1.Item.MedHinsProvider.IsFull)
        {
          break;
        }

        MoveCsePersonsWorkSet2(import.Import1.Item.MedHinsProvider.Item.
          HinsProvider,
          export.Export1.Update.MedHinsProvider.Update.HinsProvider);
        export.Export1.Update.MedHinsProvider.Update.LocalHighlite.Flag =
          import.Import1.Item.MedHinsProvider.Item.LocalHighlite.Flag;

        if (AsChar(export.Export1.Item.MedHinsProvider.Item.LocalHighlite.Flag) ==
          'Y')
        {
          var field =
            GetField(export.Export1.Item.MedHinsProvider.Item.HinsProvider,
            "formattedName");

          field.Color = "red";
          field.Highlighting = Highlighting.ReverseVideo;
          field.Protected = true;
          field.Focused = true;
        }

        export.Export1.Item.MedHinsProvider.Next();
      }

      export.Export1.Next();
    }

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    export.MedReview.Text = import.MedReview.Text;

    if (Equal(import.HiddenPassedReviewType.ActionEntry, "R"))
    {
      var field = GetField(export.MedReview, "text");

      field.Color = "cyan";
      field.Protected = true;
    }
    else
    {
      var field = GetField(export.MedReview, "text");

      field.Color = "green";
      field.Highlighting = Highlighting.Underscore;
      field.Protected = false;
      field.Focused = true;
    }

    if (Equal(global.Command, "RETLINK"))
    {
      return;
    }

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

      // ************************************************
      // There should be no valid Next Tran into this procedure step - RCG
      // ************************************************
      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      // --------------
      // This is where you set your command to do whatever is necessary to do on
      // a flow from the menu, maybe just escape....
      // --------------
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
            entities.MedicalReview.NarrativeText;

          if (local.Work.Index == 3)
          {
            break;
          }
        }

        if (!local.Work.IsEmpty)
        {
          local.Work.Index = 0;
          local.Work.CheckSize();

          export.MedReview.Text =
            Substring(local.Work.Item.Work1.NarrativeText, 12, 57);

          local.Work.Index = 1;
          local.Work.CheckSize();

          if (!IsEmpty(local.Work.Item.Work1.NarrativeText))
          {
            export.MedReview.Text = TrimEnd(export.MedReview.Text) + Substring
              (local.Work.Item.Work1.NarrativeText, 68, 12, 57);
          }

          local.Work.Index = 2;
          local.Work.CheckSize();

          if (!IsEmpty(local.Work.Item.Work1.NarrativeText))
          {
            export.MedReview.Text = TrimEnd(export.MedReview.Text) + Substring
              (local.Work.Item.Work1.NarrativeText, 68, 12, 57);
          }

          local.Work.Index = 3;
          local.Work.CheckSize();

          if (!IsEmpty(local.Work.Item.Work1.NarrativeText))
          {
            export.MedReview.Text = TrimEnd(export.MedReview.Text) + Substring
              (local.Work.Item.Work1.NarrativeText, 68, 12, 57);
          }

          export.HiddenPassed.Index = 1;
          export.HiddenPassed.CheckSize();

          export.HiddenPassed.Update.GexportH.Text = export.MedReview.Text;
        }

        // *** end
        // *** 08/08/00 SWSRCHF
        // *** Work request 000170
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "DISPLAY"))
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

    if (Equal(global.Command, "HICV") || Equal(global.Command, "HICP"))
    {
      local.Count.Count = 0;

      // **********************************************************
      // *HICP:
      // * No selection required in case of HICP command,
      // * If AR resposible for Move AR person #
      // * else move AP person # to HICP Screen.
      // * HICV:
      // * Selection Required in case of HICV command
      // * Then validate selection.
      // **********************************************************
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        export.Export1.Update.Common.SelectChar = "";

        if (Equal(export.Export1.Item.InsByApOrArNone.Text4, "AR"))
        {
          export.Selected.Number = export.Ar.Number;
        }
        else
        {
          export.Selected.Number = export.Ap1.Number;
        }
      }

      if (Equal(global.Command, "HICP"))
      {
        ExitState = "ECO_LNK_TO_HICP";
      }
      else
      {
        ExitState = "ECO_LNK_TO_HICV";
      }

      return;
    }

    switch(TrimEnd(global.Command))
    {
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "RETURN":
        if (!Equal(export.HiddenPassedReviewType.ActionEntry, "R"))
        {
          export.HiddenPassed.Index = 1;
          export.HiddenPassed.CheckSize();

          export.HiddenPassed.Update.GexportH.Text = export.MedReview.Text;
        }

        ExitState = "ACO_NE0000_RETURN";

        break;
      case "DISPLAY":
        if (AsChar(export.CaseClosedIndicator.Flag) == 'Y')
        {
          // CQ27977  Added export_med_review narrative_work to export list
          UseSpCrmdDisplayClosedCase();

          break;
        }

        if (ReadCase())
        {
          local.NoOfChOnCase.Count = 0;
          local.ChHinsStatus.CurrChHinsStatus.ActionEntry = "";
          local.ChHinsStatus.PrevChHinsNf.Flag = "N";
        }
        else
        {
          ExitState = "CASE_NF";

          break;
        }

        if (ReadCsePerson1())
        {
          local.Ar.Assign(entities.CsePerson);
          local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
          UseSiReadCsePerson();

          if (!IsEmpty(local.AbendData.Type1))
          {
            return;
          }

          MoveCsePersonsWorkSet2(local.CsePersonsWorkSet, export.Ar);

          foreach(var item in ReadProgram())
          {
            ++local.ProgramCount.Count;

            switch(local.ProgramCount.Count)
            {
              case 1:
                export.Pgm1.Code = entities.Program.Code;

                break;
              case 2:
                export.Pgm2.Code = entities.Program.Code;

                break;
              case 3:
                export.Pgm3.Code = entities.Program.Code;

                break;
              case 4:
                export.Pgm4.Code = entities.Program.Code;

                break;
              case 5:
                export.Pgm5.Code = entities.Program.Code;

                break;
              case 6:
                export.Pgm6.Code = entities.Program.Code;

                break;
              case 7:
                export.Pgm7.Code = entities.Program.Code;

                break;
              case 8:
                export.Pgm8.Code = entities.Program.Code;

                goto ReadEach1;
              default:
                goto ReadEach1;
            }
          }

ReadEach1:
          ;
        }

        // --------------------------------------------------------------------
        // 02/03/2000          Vithal Madhira                   PR# 86247
        // Modified code to implement the case review for each AP on a
        // multiple AP case.
        // -----------------------------------------------------------------------
        if (!IsEmpty(export.SelectedAp.Number) && AsChar
          (export.ApSelected.Flag) == 'Y')
        {
          MoveCsePersonsWorkSet2(export.SelectedAp, export.Ap1);

          if (AsChar(export.MultiAp.Flag) == 'Y')
          {
            export.MoreApsMsg.Text30 = "More AP's exist for this case.";
          }
        }
        else
        {
          local.Count.Count = 0;
          export.Ap1.FormattedName = "";

          foreach(var item in ReadCsePersonAbsentParent())
          {
            ++local.Count.Count;
            local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
            UseSiReadCsePerson();

            if (!IsEmpty(local.AbendData.Type1))
            {
              return;
            }

            if (local.Count.Count > 0)
            {
              if (IsEmpty(export.Ap1.FormattedName))
              {
                MoveCsePerson(entities.CsePerson, local.Ap1);
                MoveCsePersonsWorkSet2(local.CsePersonsWorkSet, export.Ap1);
              }
              else
              {
                export.Ap2.FormattedName =
                  local.CsePersonsWorkSet.FormattedName;
                export.MoreApsMsg.Text30 = "More AP's exist for this case.";

                break;
              }
            }
          }

          if (local.Count.Count == 0)
          {
            export.MoreApsMsg.Text30 = "Case AP identity is unknown.";
            export.Ap1.FormattedName = "";
            export.Ap2.FormattedName = "";
          }
        }

        export.Export1.Index = 0;
        export.Export1.Clear();

        foreach(var item in ReadCsePerson2())
        {
          local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
          ++local.NoOfChOnCase.Count;
          UseSiReadCsePerson();

          if (!IsEmpty(local.AbendData.Type1))
          {
            export.Export1.Next();

            return;
          }

          export.Export1.Update.Child1.Assign(local.CsePersonsWorkSet);

          if (Equal(local.CsePersonsWorkSet.Dob, local.Initialized.Date) || Equal
            (local.CsePersonsWorkSet.Dob, local.Max.Date))
          {
            export.Export1.Update.ChAge.TotalInteger = 0;
          }
          else
          {
            local.Age.TotalInteger = (long)DaysFromAD(local.Current.Date) - DaysFromAD
              (local.CsePersonsWorkSet.Dob);

            if (local.Age.TotalInteger < 365)
            {
              local.Age.TotalInteger =
                (long)Math.Round(
                  local.Age.TotalInteger /
                30.6M, MidpointRounding.AwayFromZero);
              export.Export1.Update.AgeText.Text4 = "Mths";
            }
            else
            {
              UseCabCalcCurrentAgeFromDob();
              export.Export1.Update.AgeText.Text4 = "Yrs";
            }

            export.Export1.Update.ChAge.TotalInteger = local.Age.TotalInteger;
          }

          if (ReadChild())
          {
            export.Export1.Update.Child2.Assign(entities.Child1);

            // ********************************************
            // DETERMINE INSURANCE COVERAGE VIABILITY
            // ********************************************
            if (ReadHealthInsuranceViabilityCsePerson())
            {
              export.Export1.Update.HealthInsuranceViability.HinsViableInd =
                entities.HealthInsuranceViability.HinsViableInd;
            }

            // *********************************************
            // SET THE RESPONSIBLE PARTY FLAG - RETRIEVE THE
            // COURT ORDER FOR MEDICAL SUPPORT AND
            // DETERMINE THE OBLIGOR.
            // *********************************************
            // 	
            export.Export1.Update.ResponsibleAp1.Flag = "";
            export.Export1.Update.ResponsibleAr.Flag = "";

            // CQ28992 - modified to actually loop through data instead of 
            // escaping after first read
            foreach(var item1 in ReadLegalActionPersonLegalActionDetailLegalAction())
              
            {
              // *********************************************
              // Read all obligors ordered to pay for Health Insurance
              // *********************************************
              // 	
              foreach(var item2 in ReadLegalActionPersonCsePerson2())
              {
                if (Equal(entities.ObligorCsePerson.Number, export.Ap1.Number))
                {
                  // ******
                  // * To determine has an order to provide health insurance / 
                  // medical support coverage
                  // ******
                  export.Export1.Update.InsByApOrArNone.Text4 = "AP";
                  export.Export1.Update.ApCourtOrdHic.Text4 = "HIC";

                  goto ReadEach2;
                }
                else if (Equal(entities.ObligorCsePerson.Number, local.Ar.Number))
                  
                {
                  export.Export1.Update.ApCourtOrdHic.Text4 = "HIC";
                  export.Export1.Update.InsByApOrArNone.Text4 = "AR";

                  goto ReadEach2;
                }
              }
            }

ReadEach2:

            foreach(var item1 in ReadLegalActionDetailObligationType())
            {
              foreach(var item2 in ReadLegalActionPersonCsePerson1())
              {
                if (Equal(entities.ObligorCsePerson.Number, export.Ap1.Number))
                {
                  // ******
                  // * To determine has an order to provide health insurance / 
                  // medical support coverage
                  // ******
                  if (Equal(entities.ObligationType.Code, "MS"))
                  {
                    export.Export1.Update.ApCourtOrdMs.Text4 = "MS";
                  }
                  else
                  {
                    export.Export1.Update.ApCourtOrdMc.Text4 = "MC";
                  }

                  break;
                }
              }
            }
          }

          // ********************************************
          // DETERMINE IF CHILD HAS MEDICAL COVERAGE AND,
          // IF SO, WHO IS PROVIDING HEALTH INSURANCE.
          // ********************************************
          local.CountValidHinsCoverage.Count = 0;

          export.Export1.Item.MedHinsProvider.Index = 0;
          export.Export1.Item.MedHinsProvider.Clear();

          foreach(var item1 in ReadHealthInsuranceCoveragePersonalHealthInsurance())
            
          {
            ++local.CountValidHinsCoverage.Count;

            if (!IsEmpty(entities.Provider.Number))
            {
              local.CsePersonsWorkSet.Number = entities.Provider.Number;
              UseSiReadCsePerson();

              if (!IsEmpty(local.AbendData.Type1))
              {
                export.Export1.Next();
                export.Export1.Item.MedHinsProvider.Next();

                return;
              }

              MoveCsePersonsWorkSet2(local.CsePersonsWorkSet,
                export.Export1.Update.MedHinsProvider.Update.HinsProvider);
            }

            export.Export1.Item.MedHinsProvider.Next();
          }

          if (local.CountValidHinsCoverage.Count == 0)
          {
            export.Export1.Update.InsuAvailInd.Flag = "N";

            if (local.NoOfChOnCase.Count >= 1)
            {
              local.OneChNoIns.Flag = "Y";
            }
          }
          else
          {
            export.Export1.Update.InsuAvailInd.Flag = "Y";

            if (local.NoOfChOnCase.Count >= 1)
            {
              export.Export1.Update.MoreChWNoCovMsg.Text80 = "";
            }
          }

          export.Export1.Next();
        }

        if (AsChar(local.OneChNoIns.Flag) == 'Y')
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            export.Export1.Update.MoreChWNoCovMsg.Text80 =
              "At least one child is not covered by health insurance";
          }
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
              entities.MedicalReview.NarrativeText;

            if (local.Work.Index == 3)
            {
              break;
            }
          }

          if (!local.Work.IsEmpty)
          {
            local.Work.Index = 0;
            local.Work.CheckSize();

            export.MedReview.Text =
              Substring(local.Work.Item.Work1.NarrativeText, 12, 57);

            local.Work.Index = 1;
            local.Work.CheckSize();

            if (!IsEmpty(local.Work.Item.Work1.NarrativeText))
            {
              export.MedReview.Text = TrimEnd(export.MedReview.Text) + Substring
                (local.Work.Item.Work1.NarrativeText, 68, 12, 57);
            }

            local.Work.Index = 2;
            local.Work.CheckSize();

            if (!IsEmpty(local.Work.Item.Work1.NarrativeText))
            {
              export.MedReview.Text = TrimEnd(export.MedReview.Text) + Substring
                (local.Work.Item.Work1.NarrativeText, 68, 12, 57);
            }

            local.Work.Index = 3;
            local.Work.CheckSize();

            if (!IsEmpty(local.Work.Item.Work1.NarrativeText))
            {
              export.MedReview.Text = TrimEnd(export.MedReview.Text) + Substring
                (local.Work.Item.Work1.NarrativeText, 68, 12, 57);
            }

            var field = GetField(export.MedReview, "text");

            field.Color = "cyan";
            field.Protected = true;
          }

          // *** end
          // *** 08/08/00 SWSRCHF
          // *** Work request 000170
        }
        else
        {
          export.HiddenPassed.Index = 1;
          export.HiddenPassed.CheckSize();

          export.MedReview.Text = export.HiddenPassed.Item.GexportH.Text;
        }

        if (IsEmpty(export.MedReview.Text))
        {
          ExitState = "SP0000_FIRST_REVIEW_4_CASE";
        }
        else if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }

        break;
      case "ENTER":
        // *********************************************************
        // * Allow to flow to CRPA Screen irrespective of Child
        // * establised is set or Legal action details preesent.
        // *********************************************************
        if (AsChar(export.CaseClosedIndicator.Flag) == 'Y')
        {
          ExitState = "ECO_LNK_TO_CR_PATERNITY";

          return;
        }
        else
        {
          // *********************************************************
          // * Allow to flow to CRPA Screen irrespective of Child
          // * establised is set or Legal action details preesent.
          // *********************************************************
          if (Equal(import.HiddenPassedReviewType.ActionEntry, "M") || Equal
            (import.HiddenPassedReviewType.ActionEntry, "P"))
          {
            if (IsEmpty(export.MedReview.Text))
            {
              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

              var field = GetField(export.MedReview, "text");

              field.Error = true;

              break;
            }
            else
            {
              export.HiddenPassed.Index = 1;
              export.HiddenPassed.CheckSize();

              export.HiddenPassed.Update.GexportH.Text = export.MedReview.Text;
            }
          }

          ExitState = "ECO_LNK_TO_CR_PATERNITY";

          return;
        }

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
      var field = GetField(export.MedReview, "text");

      field.Color = "cyan";
      field.Protected = true;
    }
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
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

  private static void MoveExport1(SpCrmdDisplayClosedCase.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    target.ApCourtOrdHic.Text4 = source.Hic.Text4;
    target.ApCourtOrdMc.Text4 = source.Mc.Text4;
    target.ApCourtOrdMs.Text4 = source.Ms.Text4;
    target.ChAge.TotalInteger = source.ChAge.TotalInteger;
    target.AgeText.Text4 = source.AgeText.Text4;
    target.InsuAvailInd.Flag = source.InsuAvailInd.Flag;
    target.ViableCsePers.FormattedName = source.ViableCsePers.FormattedName;
    target.ResponsibleAr.Flag = source.ResponsibleAr.Flag;
    target.Common.SelectChar = source.Common.SelectChar;
    target.HealthInsuranceViability.HinsViableInd =
      source.HealthInsuranceViability.HinsViableInd;
    source.MedHinsProvider.CopyTo(target.MedHinsProvider, MoveMedHinsProvider);
    target.Child1.Assign(source.Child1);
    target.ResponsibleAp2.Flag = source.ResponsibleAp2.Flag;
    target.ResponsibleAp1.Flag = source.ResponsibleAp1.Flag;
    target.HealthInsurProvider.FormattedName =
      source.HealthInsurProvider.FormattedName;
    target.Child2.Assign(source.Child2);
    target.MoreChWNoCovMsg.Text80 = source.MoreChWNoCovMsg.Text80;
    MoveTextWorkArea(source.InsByApOrArNone, target.InsByApOrArNone);
  }

  private static void MoveHealthInsuranceCoverage(
    HealthInsuranceCoverage source, HealthInsuranceCoverage target)
  {
    target.Identifier = source.Identifier;
    target.PolicyPaidByCsePersonInd = source.PolicyPaidByCsePersonInd;
  }

  private static void MoveHiddenPassed(SpCrmdDisplayClosedCase.Export.
    HiddenPassedGroup source, Export.HiddenPassedGroup target)
  {
    System.Diagnostics.Trace.TraceWarning(
      "INFO: source and target of the move do not fit perfectly.");
    target.GexportH.Text = source.GexportH.Text;
  }

  private static void MoveMedHinsProvider(SpCrmdDisplayClosedCase.Export.
    MedHinsProviderGroup source, Export.MedHinsProviderGroup target)
  {
    MoveHealthInsuranceCoverage(source.HinsCoverage, target.HinsCoverage);
    MovePersonalHealthInsurance(source.PersHins, target.PersHins);
    target.ProviderPerson.Assign(source.ProviderPerson);
    MoveCsePersonsWorkSet2(source.HinsProvider, target.HinsProvider);
    target.LocalHighlite.Flag = source.LocalHighlite.Flag;
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

  private static void MovePersonalHealthInsurance(
    PersonalHealthInsurance source, PersonalHealthInsurance target)
  {
    target.CoverageBeginDate = source.CoverageBeginDate;
    target.CoverageEndDate = source.CoverageEndDate;
  }

  private static void MoveTextWorkArea(TextWorkArea source, TextWorkArea target)
  {
    target.Text4 = source.Text4;
    target.Text30 = source.Text30;
  }

  private void UseCabCalcCurrentAgeFromDob()
  {
    var useImport = new CabCalcCurrentAgeFromDob.Import();
    var useExport = new CabCalcCurrentAgeFromDob.Export();

    useImport.CsePersonsWorkSet.Dob = local.CsePersonsWorkSet.Dob;

    Call(CabCalcCurrentAgeFromDob.Execute, useImport, useExport);

    local.Age.TotalInteger = useExport.Common.TotalInteger;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.Max.Date;

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
    useImport.CsePerson.Number = export.Selected.Number;

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

  private void UseSpCrmdDisplayClosedCase()
  {
    var useImport = new SpCrmdDisplayClosedCase.Import();
    var useExport = new SpCrmdDisplayClosedCase.Export();

    useImport.Case1.Number = export.Case1.Number;
    useImport.HiddenPass.SystemGeneratedIdentifier =
      export.HiddenPass.SystemGeneratedIdentifier;
    useImport.HiddenPassedReviewType.ActionEntry =
      export.HiddenPassedReviewType.ActionEntry;

    Call(SpCrmdDisplayClosedCase.Execute, useImport, useExport);

    export.Pgm1.Code = useExport.Pgm1.Code;
    export.Pgm2.Code = useExport.Pgm2.Code;
    export.Pgm3.Code = useExport.Pgm3.Code;
    export.Pgm4.Code = useExport.Pgm4.Code;
    export.Pgm2.Code = useExport.Pgm5.Code;
    export.Pgm6.Code = useExport.Pgm6.Code;
    export.Pgm7.Code = useExport.Pgm7.Code;
    export.Pgm8.Code = useExport.Pgm8.Code;
    MoveCsePersonsWorkSet2(useExport.Ar, export.Ar);
    MoveCsePersonsWorkSet2(useExport.Ap1CsePersonsWorkSet, export.Ap1);
    useExport.Export1.CopyTo(export.Export1, MoveExport1);
    export.MoreApsMsg.Text30 = useExport.MoreApsMsg.Text30;
    useExport.HiddenPassed.CopyTo(export.HiddenPassed, MoveHiddenPassed);
    export.MedReview.Text = useExport.MedReview.Text;
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
        entities.Case1.StatusDate = db.GetNullableDate(reader, 1);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadChild()
  {
    entities.Child1.Populated = false;

    return Read("ReadChild",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
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
        entities.Child1.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Child1.Type1);
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
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private IEnumerable<bool> ReadCsePerson2()
  {
    return ReadEach("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePersonAbsentParent()
  {
    entities.AbsentParent.Populated = false;
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePersonAbsentParent",
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
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.AbsentParent.CasNumber = db.GetString(reader, 3);
        entities.AbsentParent.Type1 = db.GetString(reader, 4);
        entities.AbsentParent.Identifier = db.GetInt32(reader, 5);
        entities.AbsentParent.StartDate = db.GetNullableDate(reader, 6);
        entities.AbsentParent.EndDate = db.GetNullableDate(reader, 7);
        entities.AbsentParent.CreatedTimestamp = db.GetDateTime(reader, 8);
        entities.AbsentParent.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
        CheckValid<CaseRole>("Type1", entities.AbsentParent.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadHealthInsuranceCoveragePersonalHealthInsurance()
  {
    System.Diagnostics.Debug.Assert(entities.Child1.Populated);

    return ReadEach("ReadHealthInsuranceCoveragePersonalHealthInsurance",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", entities.Child1.CspNumber);
        db.SetNullableDate(
          command, "coverEndDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber2", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        if (export.Export1.Item.MedHinsProvider.IsFull)
        {
          return false;
        }

        entities.HealthInsuranceCoverage.Identifier = db.GetInt64(reader, 0);
        entities.PersonalHealthInsurance.HcvId = db.GetInt64(reader, 0);
        entities.HealthInsuranceCoverage.PolicyPaidByCsePersonInd =
          db.GetString(reader, 1);
        entities.HealthInsuranceCoverage.CspNumber =
          db.GetNullableString(reader, 2);
        entities.Provider.Number = db.GetString(reader, 2);
        entities.PersonalHealthInsurance.CspNumber = db.GetString(reader, 3);
        entities.PersonalHealthInsurance.CoverageBeginDate =
          db.GetNullableDate(reader, 4);
        entities.PersonalHealthInsurance.CoverageEndDate =
          db.GetNullableDate(reader, 5);
        entities.Provider.Type1 = db.GetString(reader, 6);
        entities.Provider.OrganizationName = db.GetNullableString(reader, 7);
        entities.Provider.Populated = true;
        entities.PersonalHealthInsurance.Populated = true;
        entities.HealthInsuranceCoverage.Populated = true;
        CheckValid<CsePerson>("Type1", entities.Provider.Type1);

        return true;
      });
  }

  private bool ReadHealthInsuranceViabilityCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.Child1.Populated);
    entities.HinsViable.Populated = false;
    entities.HealthInsuranceViability.Populated = false;

    return Read("ReadHealthInsuranceViabilityCsePerson",
      (db, command) =>
      {
        db.SetInt32(command, "croIdentifier", entities.Child1.Identifier);
        db.SetString(command, "croType", entities.Child1.Type1);
        db.SetString(command, "casNumber", entities.Child1.CasNumber);
        db.SetString(command, "cspNumber", entities.Child1.CspNumber);
        db.SetString(command, "numb", export.Ap1.Number);
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
        entities.HinsViable.Number = db.GetString(reader, 9);
        entities.HinsViable.Type1 = db.GetString(reader, 10);
        entities.HinsViable.OrganizationName = db.GetNullableString(reader, 11);
        entities.HinsViable.Populated = true;
        entities.HealthInsuranceViability.Populated = true;
        CheckValid<HealthInsuranceViability>("CroType",
          entities.HealthInsuranceViability.CroType);
        CheckValid<CsePerson>("Type1", entities.HinsViable.Type1);
      });
  }

  private IEnumerable<bool> ReadLegalActionDetailObligationType()
  {
    entities.ObligationType.Populated = false;
    entities.LegalActionDetail.Populated = false;

    return ReadEach("ReadLegalActionDetailObligationType",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNumber", entities.CsePerson.Number);
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 2);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 4);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 5);
        entities.LegalActionDetail.OtyId = db.GetNullableInt32(reader, 6);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 6);
        entities.ObligationType.Code = db.GetString(reader, 7);
        entities.ObligationType.Populated = true;
        entities.LegalActionDetail.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionPersonCsePerson1()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.ObligorCsePerson.Populated = false;
    entities.ObligorLegalActionPerson.Populated = false;

    return ReadEach("ReadLegalActionPersonCsePerson1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladRNumber", entities.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.LegalActionDetail.LgaIdentifier);
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligorLegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.ObligorLegalActionPerson.CspNumber =
          db.GetNullableString(reader, 1);
        entities.ObligorCsePerson.Number = db.GetString(reader, 1);
        entities.ObligorLegalActionPerson.EffectiveDate = db.GetDate(reader, 2);
        entities.ObligorLegalActionPerson.Role = db.GetString(reader, 3);
        entities.ObligorLegalActionPerson.EndDate =
          db.GetNullableDate(reader, 4);
        entities.ObligorLegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 5);
        entities.ObligorLegalActionPerson.LadRNumber =
          db.GetNullableInt32(reader, 6);
        entities.ObligorLegalActionPerson.AccountType =
          db.GetNullableString(reader, 7);
        entities.ObligorCsePerson.Populated = true;
        entities.ObligorLegalActionPerson.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionPersonCsePerson2()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.ObligorCsePerson.Populated = false;
    entities.ObligorLegalActionPerson.Populated = false;

    return ReadEach("ReadLegalActionPersonCsePerson2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladRNumber", entities.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.LegalActionDetail.LgaIdentifier);
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligorLegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.ObligorLegalActionPerson.CspNumber =
          db.GetNullableString(reader, 1);
        entities.ObligorCsePerson.Number = db.GetString(reader, 1);
        entities.ObligorLegalActionPerson.EffectiveDate = db.GetDate(reader, 2);
        entities.ObligorLegalActionPerson.Role = db.GetString(reader, 3);
        entities.ObligorLegalActionPerson.EndDate =
          db.GetNullableDate(reader, 4);
        entities.ObligorLegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 5);
        entities.ObligorLegalActionPerson.LadRNumber =
          db.GetNullableInt32(reader, 6);
        entities.ObligorLegalActionPerson.AccountType =
          db.GetNullableString(reader, 7);
        entities.ObligorCsePerson.Populated = true;
        entities.ObligorLegalActionPerson.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionPersonLegalActionDetailLegalAction()
  {
    entities.LegalActionPerson.Populated = false;
    entities.LegalActionDetail.Populated = false;
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalActionPersonLegalActionDetailLegalAction",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
        db.SetNullableString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 1);
        entities.LegalActionPerson.EffectiveDate = db.GetDate(reader, 2);
        entities.LegalActionPerson.Role = db.GetString(reader, 3);
        entities.LegalActionPerson.EndDate = db.GetNullableDate(reader, 4);
        entities.LegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 5);
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 5);
        entities.LegalAction.Identifier = db.GetInt32(reader, 5);
        entities.LegalActionPerson.LadRNumber = db.GetNullableInt32(reader, 6);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 6);
        entities.LegalActionPerson.AccountType =
          db.GetNullableString(reader, 7);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 8);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 9);
        entities.LegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 10);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 11);
        entities.LegalActionDetail.OtyId = db.GetNullableInt32(reader, 12);
        entities.LegalAction.Type1 = db.GetString(reader, 13);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 14);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 15);
        entities.LegalActionPerson.Populated = true;
        entities.LegalActionDetail.Populated = true;
        entities.LegalAction.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);

        return true;
      });
  }

  private IEnumerable<bool> ReadNarrativeDetail1()
  {
    entities.MedicalReview.Populated = false;

    return ReadEach("ReadNarrativeDetail1",
      (db, command) =>
      {
        db.SetInt32(
          command, "infrastructureId",
          export.HiddenPass.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.MedicalReview.InfrastructureId = db.GetInt32(reader, 0);
        entities.MedicalReview.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.MedicalReview.NarrativeText = db.GetNullableString(reader, 2);
        entities.MedicalReview.LineNumber = db.GetInt32(reader, 3);
        entities.MedicalReview.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadNarrativeDetail2()
  {
    entities.MedicalReview.Populated = false;

    return ReadEach("ReadNarrativeDetail2",
      (db, command) =>
      {
        db.SetInt32(
          command, "infrastructureId",
          export.HiddenPass.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.MedicalReview.InfrastructureId = db.GetInt32(reader, 0);
        entities.MedicalReview.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.MedicalReview.NarrativeText = db.GetNullableString(reader, 2);
        entities.MedicalReview.LineNumber = db.GetInt32(reader, 3);
        entities.MedicalReview.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadProgram()
  {
    entities.Program.Populated = false;

    return ReadEach("ReadProgram",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Program.Code = db.GetString(reader, 1);
        entities.Program.Populated = true;

        return true;
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
    /// <summary>A HiddenPassedGroup group.</summary>
    [Serializable]
    public class HiddenPassedGroup
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
      /// A value of CourtOrdHic.
      /// </summary>
      [JsonPropertyName("courtOrdHic")]
      public TextWorkArea CourtOrdHic
      {
        get => courtOrdHic ??= new();
        set => courtOrdHic = value;
      }

      /// <summary>
      /// A value of CourtOrdMc.
      /// </summary>
      [JsonPropertyName("courtOrdMc")]
      public TextWorkArea CourtOrdMc
      {
        get => courtOrdMc ??= new();
        set => courtOrdMc = value;
      }

      /// <summary>
      /// A value of CourtOrdMs.
      /// </summary>
      [JsonPropertyName("courtOrdMs")]
      public TextWorkArea CourtOrdMs
      {
        get => courtOrdMs ??= new();
        set => courtOrdMs = value;
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
      /// A value of AgeText.
      /// </summary>
      [JsonPropertyName("ageText")]
      public TextWorkArea AgeText
      {
        get => ageText ??= new();
        set => ageText = value;
      }

      /// <summary>
      /// A value of InsuAvailInd.
      /// </summary>
      [JsonPropertyName("insuAvailInd")]
      public Common InsuAvailInd
      {
        get => insuAvailInd ??= new();
        set => insuAvailInd = value;
      }

      /// <summary>
      /// A value of ViableCsePers.
      /// </summary>
      [JsonPropertyName("viableCsePers")]
      public CsePersonsWorkSet ViableCsePers
      {
        get => viableCsePers ??= new();
        set => viableCsePers = value;
      }

      /// <summary>
      /// A value of ResponsibleAr.
      /// </summary>
      [JsonPropertyName("responsibleAr")]
      public Common ResponsibleAr
      {
        get => responsibleAr ??= new();
        set => responsibleAr = value;
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
      /// Gets a value of MedHinsProvider.
      /// </summary>
      [JsonIgnore]
      public Array<MedHinsProviderGroup> MedHinsProvider =>
        medHinsProvider ??= new(MedHinsProviderGroup.Capacity);

      /// <summary>
      /// Gets a value of MedHinsProvider for json serialization.
      /// </summary>
      [JsonPropertyName("medHinsProvider")]
      [Computed]
      public IList<MedHinsProviderGroup> MedHinsProvider_Json
      {
        get => medHinsProvider;
        set => MedHinsProvider.Assign(value);
      }

      /// <summary>
      /// A value of Child1.
      /// </summary>
      [JsonPropertyName("child1")]
      public CsePersonsWorkSet Child1
      {
        get => child1 ??= new();
        set => child1 = value;
      }

      /// <summary>
      /// A value of ResponsibleAp2.
      /// </summary>
      [JsonPropertyName("responsibleAp2")]
      public Common ResponsibleAp2
      {
        get => responsibleAp2 ??= new();
        set => responsibleAp2 = value;
      }

      /// <summary>
      /// A value of ResponsibleAp1.
      /// </summary>
      [JsonPropertyName("responsibleAp1")]
      public Common ResponsibleAp1
      {
        get => responsibleAp1 ??= new();
        set => responsibleAp1 = value;
      }

      /// <summary>
      /// A value of HealthInsurProvided.
      /// </summary>
      [JsonPropertyName("healthInsurProvided")]
      public CsePersonsWorkSet HealthInsurProvided
      {
        get => healthInsurProvided ??= new();
        set => healthInsurProvided = value;
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
      /// A value of NoCoverageChildMsg.
      /// </summary>
      [JsonPropertyName("noCoverageChildMsg")]
      public SpTextWorkArea NoCoverageChildMsg
      {
        get => noCoverageChildMsg ??= new();
        set => noCoverageChildMsg = value;
      }

      /// <summary>
      /// A value of InsByApOrArNone.
      /// </summary>
      [JsonPropertyName("insByApOrArNone")]
      public TextWorkArea InsByApOrArNone
      {
        get => insByApOrArNone ??= new();
        set => insByApOrArNone = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 15;

      private TextWorkArea courtOrdHic;
      private TextWorkArea courtOrdMc;
      private TextWorkArea courtOrdMs;
      private Common chAge;
      private TextWorkArea ageText;
      private Common insuAvailInd;
      private CsePersonsWorkSet viableCsePers;
      private Common responsibleAr;
      private Common common;
      private HealthInsuranceViability healthInsuranceViability;
      private Array<MedHinsProviderGroup> medHinsProvider;
      private CsePersonsWorkSet child1;
      private Common responsibleAp2;
      private Common responsibleAp1;
      private CsePersonsWorkSet healthInsurProvided;
      private CaseRole child2;
      private SpTextWorkArea noCoverageChildMsg;
      private TextWorkArea insByApOrArNone;
    }

    /// <summary>A MedHinsProviderGroup group.</summary>
    [Serializable]
    public class MedHinsProviderGroup
    {
      /// <summary>
      /// A value of HinsCoverage.
      /// </summary>
      [JsonPropertyName("hinsCoverage")]
      public HealthInsuranceCoverage HinsCoverage
      {
        get => hinsCoverage ??= new();
        set => hinsCoverage = value;
      }

      /// <summary>
      /// A value of PersHins.
      /// </summary>
      [JsonPropertyName("persHins")]
      public PersonalHealthInsurance PersHins
      {
        get => persHins ??= new();
        set => persHins = value;
      }

      /// <summary>
      /// A value of ProviderPerson.
      /// </summary>
      [JsonPropertyName("providerPerson")]
      public CsePerson ProviderPerson
      {
        get => providerPerson ??= new();
        set => providerPerson = value;
      }

      /// <summary>
      /// A value of HinsProvider.
      /// </summary>
      [JsonPropertyName("hinsProvider")]
      public CsePersonsWorkSet HinsProvider
      {
        get => hinsProvider ??= new();
        set => hinsProvider = value;
      }

      /// <summary>
      /// A value of LocalHighlite.
      /// </summary>
      [JsonPropertyName("localHighlite")]
      public Common LocalHighlite
      {
        get => localHighlite ??= new();
        set => localHighlite = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 2;

      private HealthInsuranceCoverage hinsCoverage;
      private PersonalHealthInsurance persHins;
      private CsePerson providerPerson;
      private CsePersonsWorkSet hinsProvider;
      private Common localHighlite;
    }

    /// <summary>A ChildGroup group.</summary>
    [Serializable]
    public class ChildGroup
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1;

      private LegalReferral legalReferral;
      private Common referredToLegal;
      private Common fedCompliance;
      private Common childAge;
      private Common daysRemainInCompli;
      private CaseRole ap;
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
    /// A value of CaseClosedIndicator.
    /// </summary>
    [JsonPropertyName("caseClosedIndicator")]
    public Common CaseClosedIndicator
    {
      get => caseClosedIndicator ??= new();
      set => caseClosedIndicator = value;
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
    /// A value of HiddenPass.
    /// </summary>
    [JsonPropertyName("hiddenPass")]
    public Infrastructure HiddenPass
    {
      get => hiddenPass ??= new();
      set => hiddenPass = value;
    }

    /// <summary>
    /// A value of MedReview.
    /// </summary>
    [JsonPropertyName("medReview")]
    public NarrativeWork MedReview
    {
      get => medReview ??= new();
      set => medReview = value;
    }

    /// <summary>
    /// Gets a value of HiddenPassed.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenPassedGroup> HiddenPassed => hiddenPassed ??= new(
      HiddenPassedGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenPassed for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenPassed")]
    [Computed]
    public IList<HiddenPassedGroup> HiddenPassed_Json
    {
      get => hiddenPassed;
      set => HiddenPassed.Assign(value);
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
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
    /// A value of MultiAp.
    /// </summary>
    [JsonPropertyName("multiAp")]
    public Common MultiAp
    {
      get => multiAp ??= new();
      set => multiAp = value;
    }

    private Program pgm1;
    private Program pgm2;
    private Program pgm3;
    private Program pgm4;
    private Program pgm5;
    private Program pgm6;
    private Program pgm7;
    private Program pgm8;
    private Common caseClosedIndicator;
    private TextWorkArea moreApsMsg;
    private Infrastructure hiddenPass;
    private NarrativeWork medReview;
    private Array<HiddenPassedGroup> hiddenPassed;
    private Array<ImportGroup> import1;
    private CsePersonsWorkSet ap2;
    private CsePersonsWorkSet ap1;
    private CsePersonsWorkSet ar;
    private CaseRole caseRole;
    private Case1 case1;
    private Common hiddenPassedReviewType;
    private Array<ChildGroup> child;
    private NextTranInfo hidden;
    private Standard standard;
    private Common apSelected;
    private CsePersonsWorkSet selectedAp;
    private Common multiAp;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A HiddenPassedGroup group.</summary>
    [Serializable]
    public class HiddenPassedGroup
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
      /// A value of ApCourtOrdHic.
      /// </summary>
      [JsonPropertyName("apCourtOrdHic")]
      public TextWorkArea ApCourtOrdHic
      {
        get => apCourtOrdHic ??= new();
        set => apCourtOrdHic = value;
      }

      /// <summary>
      /// A value of ApCourtOrdMc.
      /// </summary>
      [JsonPropertyName("apCourtOrdMc")]
      public TextWorkArea ApCourtOrdMc
      {
        get => apCourtOrdMc ??= new();
        set => apCourtOrdMc = value;
      }

      /// <summary>
      /// A value of ApCourtOrdMs.
      /// </summary>
      [JsonPropertyName("apCourtOrdMs")]
      public TextWorkArea ApCourtOrdMs
      {
        get => apCourtOrdMs ??= new();
        set => apCourtOrdMs = value;
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
      /// A value of AgeText.
      /// </summary>
      [JsonPropertyName("ageText")]
      public TextWorkArea AgeText
      {
        get => ageText ??= new();
        set => ageText = value;
      }

      /// <summary>
      /// A value of InsuAvailInd.
      /// </summary>
      [JsonPropertyName("insuAvailInd")]
      public Common InsuAvailInd
      {
        get => insuAvailInd ??= new();
        set => insuAvailInd = value;
      }

      /// <summary>
      /// A value of ViableCsePers.
      /// </summary>
      [JsonPropertyName("viableCsePers")]
      public CsePersonsWorkSet ViableCsePers
      {
        get => viableCsePers ??= new();
        set => viableCsePers = value;
      }

      /// <summary>
      /// A value of ResponsibleAr.
      /// </summary>
      [JsonPropertyName("responsibleAr")]
      public Common ResponsibleAr
      {
        get => responsibleAr ??= new();
        set => responsibleAr = value;
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
      /// Gets a value of MedHinsProvider.
      /// </summary>
      [JsonIgnore]
      public Array<MedHinsProviderGroup> MedHinsProvider =>
        medHinsProvider ??= new(MedHinsProviderGroup.Capacity);

      /// <summary>
      /// Gets a value of MedHinsProvider for json serialization.
      /// </summary>
      [JsonPropertyName("medHinsProvider")]
      [Computed]
      public IList<MedHinsProviderGroup> MedHinsProvider_Json
      {
        get => medHinsProvider;
        set => MedHinsProvider.Assign(value);
      }

      /// <summary>
      /// A value of Child1.
      /// </summary>
      [JsonPropertyName("child1")]
      public CsePersonsWorkSet Child1
      {
        get => child1 ??= new();
        set => child1 = value;
      }

      /// <summary>
      /// A value of ResponsibleAp2.
      /// </summary>
      [JsonPropertyName("responsibleAp2")]
      public Common ResponsibleAp2
      {
        get => responsibleAp2 ??= new();
        set => responsibleAp2 = value;
      }

      /// <summary>
      /// A value of ResponsibleAp1.
      /// </summary>
      [JsonPropertyName("responsibleAp1")]
      public Common ResponsibleAp1
      {
        get => responsibleAp1 ??= new();
        set => responsibleAp1 = value;
      }

      /// <summary>
      /// A value of HealthInsurProvider.
      /// </summary>
      [JsonPropertyName("healthInsurProvider")]
      public CsePersonsWorkSet HealthInsurProvider
      {
        get => healthInsurProvider ??= new();
        set => healthInsurProvider = value;
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
      /// A value of MoreChWNoCovMsg.
      /// </summary>
      [JsonPropertyName("moreChWNoCovMsg")]
      public SpTextWorkArea MoreChWNoCovMsg
      {
        get => moreChWNoCovMsg ??= new();
        set => moreChWNoCovMsg = value;
      }

      /// <summary>
      /// A value of InsByApOrArNone.
      /// </summary>
      [JsonPropertyName("insByApOrArNone")]
      public TextWorkArea InsByApOrArNone
      {
        get => insByApOrArNone ??= new();
        set => insByApOrArNone = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 15;

      private TextWorkArea apCourtOrdHic;
      private TextWorkArea apCourtOrdMc;
      private TextWorkArea apCourtOrdMs;
      private Common chAge;
      private TextWorkArea ageText;
      private Common insuAvailInd;
      private CsePersonsWorkSet viableCsePers;
      private Common responsibleAr;
      private Common common;
      private HealthInsuranceViability healthInsuranceViability;
      private Array<MedHinsProviderGroup> medHinsProvider;
      private CsePersonsWorkSet child1;
      private Common responsibleAp2;
      private Common responsibleAp1;
      private CsePersonsWorkSet healthInsurProvider;
      private CaseRole child2;
      private SpTextWorkArea moreChWNoCovMsg;
      private TextWorkArea insByApOrArNone;
    }

    /// <summary>A MedHinsProviderGroup group.</summary>
    [Serializable]
    public class MedHinsProviderGroup
    {
      /// <summary>
      /// A value of HinsCoverage.
      /// </summary>
      [JsonPropertyName("hinsCoverage")]
      public HealthInsuranceCoverage HinsCoverage
      {
        get => hinsCoverage ??= new();
        set => hinsCoverage = value;
      }

      /// <summary>
      /// A value of PersHins.
      /// </summary>
      [JsonPropertyName("persHins")]
      public PersonalHealthInsurance PersHins
      {
        get => persHins ??= new();
        set => persHins = value;
      }

      /// <summary>
      /// A value of ProviderPerson.
      /// </summary>
      [JsonPropertyName("providerPerson")]
      public CsePerson ProviderPerson
      {
        get => providerPerson ??= new();
        set => providerPerson = value;
      }

      /// <summary>
      /// A value of HinsProvider.
      /// </summary>
      [JsonPropertyName("hinsProvider")]
      public CsePersonsWorkSet HinsProvider
      {
        get => hinsProvider ??= new();
        set => hinsProvider = value;
      }

      /// <summary>
      /// A value of LocalHighlite.
      /// </summary>
      [JsonPropertyName("localHighlite")]
      public Common LocalHighlite
      {
        get => localHighlite ??= new();
        set => localHighlite = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 2;

      private HealthInsuranceCoverage hinsCoverage;
      private PersonalHealthInsurance persHins;
      private CsePerson providerPerson;
      private CsePersonsWorkSet hinsProvider;
      private Common localHighlite;
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
    /// A value of CaseClosedIndicator.
    /// </summary>
    [JsonPropertyName("caseClosedIndicator")]
    public Common CaseClosedIndicator
    {
      get => caseClosedIndicator ??= new();
      set => caseClosedIndicator = value;
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
    /// A value of HiddenPass.
    /// </summary>
    [JsonPropertyName("hiddenPass")]
    public Infrastructure HiddenPass
    {
      get => hiddenPass ??= new();
      set => hiddenPass = value;
    }

    /// <summary>
    /// A value of MedReview.
    /// </summary>
    [JsonPropertyName("medReview")]
    public NarrativeWork MedReview
    {
      get => medReview ??= new();
      set => medReview = value;
    }

    /// <summary>
    /// Gets a value of HiddenPassed.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenPassedGroup> HiddenPassed => hiddenPassed ??= new(
      HiddenPassedGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenPassed for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenPassed")]
    [Computed]
    public IList<HiddenPassedGroup> HiddenPassed_Json
    {
      get => hiddenPassed;
      set => HiddenPassed.Assign(value);
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public CsePerson Selected
    {
      get => selected ??= new();
      set => selected = value;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
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
    /// A value of MultiAp.
    /// </summary>
    [JsonPropertyName("multiAp")]
    public Common MultiAp
    {
      get => multiAp ??= new();
      set => multiAp = value;
    }

    /// <summary>
    /// A value of HiddenPassed1.
    /// </summary>
    [JsonPropertyName("hiddenPassed1")]
    public Infrastructure HiddenPassed1
    {
      get => hiddenPassed1 ??= new();
      set => hiddenPassed1 = value;
    }

    private Program pgm1;
    private Program pgm2;
    private Program pgm3;
    private Program pgm4;
    private Program pgm5;
    private Program pgm6;
    private Program pgm7;
    private Program pgm8;
    private Common caseClosedIndicator;
    private TextWorkArea moreApsMsg;
    private Infrastructure hiddenPass;
    private NarrativeWork medReview;
    private Array<HiddenPassedGroup> hiddenPassed;
    private CsePerson selected;
    private Array<ExportGroup> export1;
    private CsePersonsWorkSet ap2;
    private CsePersonsWorkSet ap1;
    private CsePersonsWorkSet ar;
    private CaseRole caseRole;
    private Case1 case1;
    private Common hiddenPassedReviewType;
    private NextTranInfo hidden;
    private Standard standard;
    private Common apSelected;
    private CsePersonsWorkSet selectedAp;
    private Common multiAp;
    private Infrastructure hiddenPassed1;
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

    /// <summary>A ChHinsStatusGroup group.</summary>
    [Serializable]
    public class ChHinsStatusGroup
    {
      /// <summary>
      /// A value of CurrChHinsStatus.
      /// </summary>
      [JsonPropertyName("currChHinsStatus")]
      public Common CurrChHinsStatus
      {
        get => currChHinsStatus ??= new();
        set => currChHinsStatus = value;
      }

      /// <summary>
      /// A value of PrevChHinsVf.
      /// </summary>
      [JsonPropertyName("prevChHinsVf")]
      public Common PrevChHinsVf
      {
        get => prevChHinsVf ??= new();
        set => prevChHinsVf = value;
      }

      /// <summary>
      /// A value of PrevChHinsNv.
      /// </summary>
      [JsonPropertyName("prevChHinsNv")]
      public Common PrevChHinsNv
      {
        get => prevChHinsNv ??= new();
        set => prevChHinsNv = value;
      }

      /// <summary>
      /// A value of PrevChHinsNf.
      /// </summary>
      [JsonPropertyName("prevChHinsNf")]
      public Common PrevChHinsNf
      {
        get => prevChHinsNf ??= new();
        set => prevChHinsNf = value;
      }

      private Common currChHinsStatus;
      private Common prevChHinsVf;
      private Common prevChHinsNv;
      private Common prevChHinsNf;
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
    /// A value of OneChNoIns.
    /// </summary>
    [JsonPropertyName("oneChNoIns")]
    public Common OneChNoIns
    {
      get => oneChNoIns ??= new();
      set => oneChNoIns = value;
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
    /// A value of Age.
    /// </summary>
    [JsonPropertyName("age")]
    public Common Age
    {
      get => age ??= new();
      set => age = value;
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
    /// A value of CountValidHinsCoverage.
    /// </summary>
    [JsonPropertyName("countValidHinsCoverage")]
    public Common CountValidHinsCoverage
    {
      get => countValidHinsCoverage ??= new();
      set => countValidHinsCoverage = value;
    }

    /// <summary>
    /// A value of CaseClosedDate.
    /// </summary>
    [JsonPropertyName("caseClosedDate")]
    public DateWorkArea CaseClosedDate
    {
      get => caseClosedDate ??= new();
      set => caseClosedDate = value;
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
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DateWorkArea Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
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
    /// A value of NoOfChOnCase.
    /// </summary>
    [JsonPropertyName("noOfChOnCase")]
    public Common NoOfChOnCase
    {
      get => noOfChOnCase ??= new();
      set => noOfChOnCase = value;
    }

    /// <summary>
    /// Gets a value of ChHinsStatus.
    /// </summary>
    [JsonPropertyName("chHinsStatus")]
    public ChHinsStatusGroup ChHinsStatus
    {
      get => chHinsStatus ?? (chHinsStatus = new());
      set => chHinsStatus = value;
    }

    private Array<WorkGroup> work;
    private Common oneChNoIns;
    private DateWorkArea max;
    private Common age;
    private Common programCount;
    private Common countValidHinsCoverage;
    private DateWorkArea caseClosedDate;
    private Common count;
    private DateWorkArea current;
    private DateWorkArea initialized;
    private CsePerson ar;
    private AbendData abendData;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePerson ap2;
    private CsePerson ap1;
    private Common noOfChOnCase;
    private ChHinsStatusGroup chHinsStatus;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of MedicalReview.
    /// </summary>
    [JsonPropertyName("medicalReview")]
    public NarrativeDetail MedicalReview
    {
      get => medicalReview ??= new();
      set => medicalReview = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
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
    /// A value of HinsViable.
    /// </summary>
    [JsonPropertyName("hinsViable")]
    public CsePerson HinsViable
    {
      get => hinsViable ??= new();
      set => hinsViable = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of ObligorCsePerson.
    /// </summary>
    [JsonPropertyName("obligorCsePerson")]
    public CsePerson ObligorCsePerson
    {
      get => obligorCsePerson ??= new();
      set => obligorCsePerson = value;
    }

    /// <summary>
    /// A value of ObligorLegalActionPerson.
    /// </summary>
    [JsonPropertyName("obligorLegalActionPerson")]
    public LegalActionPerson ObligorLegalActionPerson
    {
      get => obligorLegalActionPerson ??= new();
      set => obligorLegalActionPerson = value;
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
    /// A value of Provider.
    /// </summary>
    [JsonPropertyName("provider")]
    public CsePerson Provider
    {
      get => provider ??= new();
      set => provider = value;
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
    /// A value of HealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("healthInsuranceCoverage")]
    public HealthInsuranceCoverage HealthInsuranceCoverage
    {
      get => healthInsuranceCoverage ??= new();
      set => healthInsuranceCoverage = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of CoveredPerson.
    /// </summary>
    [JsonPropertyName("coveredPerson")]
    public CaseRole CoveredPerson
    {
      get => coveredPerson ??= new();
      set => coveredPerson = value;
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
    /// A value of Child1.
    /// </summary>
    [JsonPropertyName("child1")]
    public CaseRole Child1
    {
      get => child1 ??= new();
      set => child1 = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    private NarrativeDetail medicalReview;
    private Obligation obligation;
    private PersonProgram personProgram;
    private Program program;
    private CsePerson hinsViable;
    private ObligationType obligationType;
    private Infrastructure infrastructure;
    private CsePerson obligorCsePerson;
    private LegalActionPerson obligorLegalActionPerson;
    private HealthInsuranceViability healthInsuranceViability;
    private CsePerson provider;
    private PersonalHealthInsurance personalHealthInsurance;
    private HealthInsuranceCoverage healthInsuranceCoverage;
    private LegalActionPerson legalActionPerson;
    private LegalActionDetail legalActionDetail;
    private LegalAction legalAction;
    private CaseRole caseRole;
    private CaseRole coveredPerson;
    private CaseRole absentParent;
    private CaseRole applicantRecipient;
    private CaseRole child1;
    private CsePerson csePerson;
    private Case1 case1;
  }
#endregion
}
