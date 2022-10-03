// Program: SP_CRLO_CASE_REVIEW_LOCATE, ID: 372653322, model: 746.
// Short name: SWECRLOP
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
/// A program: SP_CRLO_CASE_REVIEW_LOCATE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpCrloCaseReviewLocate: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CRLO_CASE_REVIEW_LOCATE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCrloCaseReviewLocate(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCrloCaseReviewLocate.
  /// </summary>
  public SpCrloCaseReviewLocate(IContext context, Import import, Export export):
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
    // 				 to Narrative/Infrastructure.
    // 04/30/97 Rod Grey 	         Change Current Date
    // 05\14\97 R. Grey		 IDCR #328 - Exit Msgs
    // 05\14\97 R. Grey		 IDCR #327 - Change dialog flow from
    // 				 INCS to INCL
    // 07/22/97 R. Grey		 Add review closed case
    // 07/28/97 R. Grey		 Change BKRP date from confirm to
    // 				 file date for display  H00025276.
    // 04/10/99 N.Engoor                Added new fields on the screen.
    // 06/16/99 N.Engoor                Changed the way in which the notes are 
    // being displayed.
    // ---------------------------------------------
    // -----------------------------------------------------------------------------------
    // 02/03/2000          Vithal Madhira                   PR# 86247        
    // Modified code to implement the case review for each AP on a multiple AP
    // case.
    // 08/08/2000 SWSRCHF WR# 000170  Replace the read for Narrative by a read 
    // for
    //                                
    // Narrative Detail
    // 04/01/2011	T Pierce	CQ# 23212	Removed references to obsolete Narrative 
    // entity type.
    // ------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);
    local.Current.Date = Now().Date;
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.Case1.Number = import.Case1.Number;
    export.HiddenPassed1.SystemGeneratedIdentifier =
      import.HiddenPassed1.SystemGeneratedIdentifier;
    export.HiddenPassedReviewType.ActionEntry =
      import.HiddenPassedReviewType.ActionEntry;
    export.CaseClosedIndicator.Flag = import.CaseClosedIndicator.Flag;
    export.ActivityLiteral.Text16 = import.ActivityLiteral.Text16;
    export.MoreAps.Text80 = import.MoreAps.Text80;
    MoveCsePersonsWorkSet3(import.Ap1, export.Ap1);
    export.MultiAp.Flag = import.MultiAp.Flag;
    export.ApSelected.Flag = import.ApSelected.Flag;
    MoveCsePersonsWorkSet3(import.SelectedAp, export.SelectedApCsePersonsWorkSet);
      

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
      export.HiddenPassed.Update.GexportHiddenPassed.Flag =
        import.HiddenPassed.Item.GimportHiddenPassed.Flag;
    }

    import.HiddenPassed.CheckIndex();

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

    if (!Equal(global.Command, "DISPLAY"))
    {
      export.LocateReview.Text = import.LocateReview.Text;
      export.ApAddrProbMsg.Text80 = import.ApAddrProbMsg.Text80;

      export.Ap.Index = 0;
      export.Ap.Clear();

      for(import.Ap.Index = 0; import.Ap.Index < import.Ap.Count; ++
        import.Ap.Index)
      {
        if (export.Ap.IsFull)
        {
          break;
        }

        export.Ap.Update.PersonIncomeHistory.Assign(
          import.Ap.Item.PersonIncomeHistory);
        export.Ap.Update.Common.SelectChar = import.Ap.Item.Common.SelectChar;

        if (Equal(global.Command, "RETLINK"))
        {
          if (AsChar(export.Ap.Item.Common.SelectChar) == '*')
          {
            export.Ap.Update.Common.SelectChar = "";
            global.Command = "DISPLAY";
          }
        }

        export.Ap.Update.LastVerifiedEmpDat.Assign(
          import.Ap.Item.LastVerifiedEmpDat);
        export.Ap.Update.ActivityLiteral.Text16 =
          import.Ap.Item.ActivityLiteral.Text16;
        MoveTextWorkArea(import.Ap.Item.AltAlias, export.Ap.Update.AltAlias);
        MoveBankruptcy(import.Ap.Item.Bankruptcy, export.Ap.Update.Bankruptcy);
        export.Ap.Update.DaysRemaining.Count =
          import.Ap.Item.DaysRemaining.Count;
        export.Ap.Update.Incarceration.Assign(import.Ap.Item.Incarceration);
        export.Ap.Update.ApCsePerson.Assign(import.Ap.Item.ApCsePerson);
        export.Ap.Update.ApCsePersonAddress.Assign(
          import.Ap.Item.ApCsePersonAddress);
        export.Ap.Update.ApCsePersonsWorkSet.Assign(
          import.Ap.Item.ApCsePersonsWorkSet);
        export.Ap.Update.FederalCompliance.Flag =
          import.Ap.Item.FederalCompliance.Flag;
        export.Ap.Update.MedicalInsuAvailabl.Flag =
          import.Ap.Item.MedicalInsuAvailabl.Flag;
        export.Ap.Update.ParolOfficer.Flag = import.Ap.Item.ParolOfficer.Flag;
        export.Ap.Update.Data1099LocateRequest.Assign(
          import.Ap.Item.Data1099LocateRequest);
        export.Ap.Update.FplsFplsLocateRequest.Assign(import.Ap.Item.Fpls);
        export.Ap.Update.SesaFplsLocateRequest.Assign(import.Ap.Item.Sesa);
        export.Ap.Update.EmpIncome.TotalCurrency =
          import.Ap.Item.EmpIncome.TotalCurrency;
        export.Ap.Update.OtherIncome.TotalCurrency =
          import.Ap.Item.OtherIncome.TotalCurrency;
        export.Ap.Update.Start.Date = import.Ap.Item.Start.Date;
        export.Ap.Update.Compliance.Date = import.Ap.Item.Compliance.Date;
        export.Ap.Update.AddressNotVerfMsg.Text20 =
          import.Ap.Item.AddressNotVerfMsg.Text20;
        export.Ap.Update.HiliteAddrField.Flag =
          import.Ap.Item.HiliteAddrField.Flag;
        export.Ap.Next();
      }
    }

    if (Equal(export.HiddenPassedReviewType.ActionEntry, "R"))
    {
      var field = GetField(export.LocateReview, "text");

      field.Color = "cyan";
      field.Protected = true;
    }
    else
    {
      var field = GetField(export.LocateReview, "text");

      field.Color = "green";
      field.Highlighting = Highlighting.Underscore;
      field.Protected = false;
      field.Focused = true;
    }

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else if (Equal(global.Command, "INVALID"))
    {
      ExitState = "ACO_NE0000_INVALID_PF_KEY";

      return;
    }
    else
    {
      if (!Equal(global.Command, "ENTER"))
      {
        goto Test;
      }

      export.Hidden.LastTran = "CRLO";
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

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ---------------
      // This is where you set your export value to the export hidden next tran 
      // values if the user is comming into this procedure on a next tran
      // action.
      // ---------------
      UseScCabNextTranGet();
      export.Case1.Number = export.Hidden.CaseNumber ?? Spaces(10);

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      // ---------------------
      // This is where you set your command to do whatever is necessary to do on
      // a flow from the menu, maybe just escape....
      // ---------------------
      // You should get this information from the Dialog Flow Diagram.  It is 
      // the SEND CMD on the propertis for a Transfer from one  procedure to
      // another.
      // *** the statement would read COMMAND IS display   *****
      global.Command = "DISPLAY";

      return;
    }

    if (Equal(global.Command, "APDS") || Equal(global.Command, "INCL") || Equal
      (global.Command, "FPLS") || Equal(global.Command, "JAIL") || Equal
      (global.Command, "ADDR"))
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

    if (Equal(global.Command, "APDS") || Equal(global.Command, "INCL") || Equal
      (global.Command, "FPLS") || Equal(global.Command, "JAIL") || Equal
      (global.Command, "ADDR") || Equal(global.Command, "RETURN"))
    {
      // -----------------------
      // N.Engoor - 04/18/99
      // Save any text that is entered new before flowing to any screen on a 
      // link.
      // -----------------------
      if (!export.HiddenPassed.IsEmpty)
      {
        export.HiddenPassed.Index = 0;
        export.HiddenPassed.CheckSize();

        if (!Equal(export.LocateReview.Text,
          export.HiddenPassed.Item.GexportH.Text))
        {
          export.HiddenPassed.Update.GexportH.Text = export.LocateReview.Text;
        }
      }
    }

    if (Equal(global.Command, "APDS") || Equal(global.Command, "INCL") || Equal
      (global.Command, "FPLS") || Equal(global.Command, "JAIL") || Equal
      (global.Command, "ADDR"))
    {
      local.Common.Count = 0;

      for(export.Ap.Index = 0; export.Ap.Index < export.Ap.Count; ++
        export.Ap.Index)
      {
        if (!IsEmpty(export.Ap.Item.Common.SelectChar))
        {
          ++local.Common.Count;

          if (AsChar(export.Ap.Item.Common.SelectChar) == 'S')
          {
            export.Ap2.Assign(export.Ap.Item.ApCsePersonsWorkSet);
            export.SelectedApCsePerson.Number = export.Ap2.Number;
            export.Ap.Update.Common.SelectChar = "*";

            if (export.Ap.Item.SesaFplsLocateRequest.Identifier == 0 && IsEmpty
              (export.Ap.Item.SesaFplsLocateRequest.TransactionType))
            {
              export.Selected.Assign(export.Ap.Item.FplsFplsLocateRequest);
            }
            else
            {
              export.Selected.Assign(export.Ap.Item.SesaFplsLocateRequest);
            }

            break;
          }
          else
          {
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            var field = GetField(export.Ap.Item.Common, "selectChar");

            field.Error = true;

            return;
          }
        }
      }

      if (local.Common.Count < 1)
      {
        ExitState = "ACO_NE0000_NO_SELECTION_MADE";

        return;
      }

      switch(TrimEnd(global.Command))
      {
        case "JAIL":
          ExitState = "ECO_LNK_TO_PERSON_INCARCERATION";

          break;
        case "INCL":
          ExitState = "ECO_LNK_TO_INCOME_SRC_LIST_BY_AP";

          break;
        case "ADDR":
          ExitState = "ECO_LNK_TO_ADDRESS_MAINTENANCE";

          break;
        case "FPLS":
          ExitState = "ECO_LNK_TO_PROCESS_FPLS_TRANSCTN";

          break;
        case "APDS":
          ExitState = "ECO_LNK_TO_AP_DETAILS";

          break;
        default:
          break;
      }
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    switch(TrimEnd(global.Command))
    {
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "DISPLAY":
        if (ReadCase())
        {
          local.NoOfApOnCase.Count = 0;
          local.ApAddrStatus.CurrApAddrStatus.ActionEntry = "";
          local.ApAddrStatus.PrevApAddrNv.Flag = "N";
          local.ApAddrStatus.PrevApAddrNf.Flag = "N";
          local.ApAddrStatus.PrevApAddrVf.Flag = "N";

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

          break;
        }

        // -----------------------------------------------------------------------------------
        // 02/03/2000          Vithal Madhira                   PR# 86247
        // Modified code to implement the case review for each AP on a
        // multiple AP case.
        // ------------------------------------------------------------------------------
        if (!IsEmpty(export.SelectedApCsePersonsWorkSet.Number) && AsChar
          (export.ApSelected.Flag) == 'Y')
        {
          // --------------------------------------------------------------------------
          // User selected an AP on CRIN screen and came to this screen.
          // ---------------------------------------------------------------------------
          if (ReadCsePerson1())
          {
            local.SelApCsePerson.Assign(entities.ApCsePerson);

            if (AsChar(entities.ApCsePerson.UnemploymentInd) != 'Y')
            {
              local.SelApCsePerson.UnemploymentInd = "N";
            }
            else
            {
              local.SelApCsePerson.UnemploymentInd = "Y";
            }

            local.CsePersonsWorkSet.Number = entities.ApCsePerson.Number;
            UseSiReadCsePerson();

            if (!IsEmpty(local.AbendData.Type1))
            {
              return;
            }

            MoveCsePersonsWorkSet2(local.CsePersonsWorkSet,
              local.SelApCsePersonsWorkSet);

            // --------------------------
            // N.Engoor - 03/11/99
            // Calling External that retrieves the Alias name if any for the 
            // corresponding AP.
            // --------------------------
            UseEabReadAlias2();

            if (!IsEmpty(local.AbendData.Type1))
            {
              return;
            }

            if (!Equal(local.Alias.Item.Galias.Dob, null))
            {
              local.SelAltAlias.Text10 = "SSN/Alias";
              local.SelAltAlias.Text4 = "Alt";
            }

            if (ReadCsePersonAddress())
            {
              local.SelApCsePersonAddress.Assign(entities.ApCsePersonAddress);
            }

            if (IsExitState("ACO_NN0000_ALL_OK") && !
              entities.ApCsePersonAddress.Populated)
            {
              export.ApAddrProbMsg.Text80 = "AP Address not found";
            }

            if (ReadIncarceration1())
            {
              local.SelIncarceration.Assign(entities.Incarceration);

              if (Equal(local.SelIncarceration.EndDate, local.Max.Date))
              {
                local.SelIncarceration.EndDate = local.Initialized.Date;
              }

              if (!Lt(local.Current.Date, entities.Incarceration.EndDate) && !
                Equal(entities.Incarceration.EndDate, local.Initialized.Date))
              {
                // ************************************************
                // If the AP is out of Jail/Prison, then determine if
                // the AP is on Parole/Probation.
                // ************************************************
                if (ReadIncarceration3())
                {
                  if (!Lt(local.OpenOrClosed.Date,
                    entities.ParoleOrProbation.VerifiedDate) && (
                      Equal(entities.ParoleOrProbation.EndDate,
                    local.Initialized.Date) || !
                    Lt(entities.ParoleOrProbation.EndDate,
                    local.OpenOrClosed.Date)))
                  {
                    local.SelParolOfficer.Flag = "Y";
                  }
                  else
                  {
                    local.SelParolOfficer.Flag = "";
                  }
                }
              }

              if (Equal(export.HiddenPassedReviewType.ActionEntry, "M") || Equal
                (export.HiddenPassedReviewType.ActionEntry, "P"))
              {
                // ---------------
                // Refit Note:
                // In the previous useage of this procedure, the concat 
                // statements inserted the text by including the present text in
                // an entity view.  However, that entity view was not populated
                // prior to doing the concat.  I have changed the logic
                // existant in the procedure by establishing currency and
                // ensuring that the current Locate Review narrative is inserted
                // after the "AP Ýfirst name¨" in the concat statements.
                // SHOULD THIS CAUSE A PROBLEM, removing the read to establish 
                // currency on the current Locate Review narrative WILL RETURN
                // the functionality of this procedure to it's prior state.
                // RVW 1/7/97
                // ----------------
                local.IncarceratedFlag.Flag = "Y";
              }
            }

            // *********************************************
            // QUALIFY THE INCOME SOURCE READ WITH START DATE AND END DATE.
            // *********************************************
            local.RollingMaxVerfDt.Date = local.Initialized.Date;
            local.SelPersonIncomeHistory.IncomeAmt = 0;
            local.SelOtherIncome.TotalCurrency = 0;

            foreach(var item in ReadIncomeSource())
            {
              if (Equal(entities.IncomeSource.EndDt, new DateTime(2099, 12, 31)))
                
              {
                if (AsChar(entities.IncomeSource.Type1) == 'E')
                {
                  if (AsChar(entities.IncomeSource.ReturnCd) == 'E' || AsChar
                    (entities.IncomeSource.ReturnCd) == 'L' || AsChar
                    (entities.IncomeSource.ReturnCd) == 'W' || AsChar
                    (entities.IncomeSource.ReturnCd) == 'O')
                  {
                    local.SelLastVerifiedEmpDate.ReturnDt =
                      entities.IncomeSource.ReturnDt;
                  }
                }

                if (AsChar(entities.IncomeSource.Type1) == 'M')
                {
                  if (AsChar(entities.IncomeSource.ReturnCd) == 'A' || AsChar
                    (entities.IncomeSource.ReturnCd) == 'R')
                  {
                    local.SelLastVerifiedEmpDate.ReturnDt =
                      entities.IncomeSource.ReturnDt;
                  }
                }

                if (AsChar(entities.IncomeSource.Type1) == 'O')
                {
                  if (AsChar(entities.IncomeSource.ReturnCd) == 'V')
                  {
                    local.SelLastVerifiedEmpDate.ReturnDt =
                      entities.IncomeSource.ReturnDt;
                  }
                }

                if (AsChar(entities.IncomeSource.Type1) == 'R')
                {
                  if (AsChar(entities.IncomeSource.ReturnCd) == 'V')
                  {
                    local.SelLastVerifiedEmpDate.ReturnDt =
                      entities.IncomeSource.ReturnDt;
                  }
                }
              }

              if (Equal(local.SelLastVerifiedEmpDate.ReturnDt,
                local.BlankDate.Date))
              {
                continue;
              }

              local.SelLastVerifiedEmpDate.LastQtrIncome =
                entities.IncomeSource.LastQtrIncome;
              local.SelLastVerifiedEmpDate.LastQtr =
                entities.IncomeSource.LastQtr;
              local.SelLastVerifiedEmpDate.LastQtrYr =
                entities.IncomeSource.LastQtrYr;

              break;
            }

            local.SelPersonIncomeHistory.VerifiedDt =
              local.RollingMaxVerfDt.Date;

            if (ReadFplsLocateRequest())
            {
              local.SelFplsFplsLocateRequest.Assign(entities.FplsLocateRequest);
            }

            if (Read1099LocateRequest())
            {
              local.Seldata1099LocateRequest.Assign(
                entities.Data1099LocateRequest);
            }
          }

          local.SelActivityLiteral.Text16 = "Required Cmpt Dt";

          if (ReadMonitoredActivityInfrastructure2())
          {
            local.SelStart.Date = entities.MonitoredActivity.StartDate;

            if (Equal(entities.MonitoredActivity.ClosureDate,
              local.Initialized.Date) || Equal
              (entities.MonitoredActivity.ClosureDate, local.Max.Date))
            {
              local.SelCompliance.Date =
                entities.MonitoredActivity.FedNonComplianceDate;
              local.SelActivityLiteral.Text16 = "Required Cmpt Dt";

              // Indicates that the monitored activity has not been closed,
              // therefore AP has not been located.
              if (!Lt(local.Current.Date,
                entities.MonitoredActivity.FedNonComplianceDate))
              {
                local.SelFederalCompliance.Flag = "N";
                local.SelDaysRemaining.Count = 0;
              }
              else if (Lt(local.Current.Date,
                entities.MonitoredActivity.FedNonComplianceDate))
              {
                local.SelFederalCompliance.Flag = "";
                local.SelDaysRemaining.Count =
                  DaysFromAD(entities.MonitoredActivity.FedNonComplianceDate) -
                  DaysFromAD(local.Current.Date);
              }
            }
            else
            {
              // Indicates monitored activity has been closed, and other fields 
              // populated (ap address, etc.) indicate whether or not he's been
              // found.   Therefore, compliance indicator is set based on
              // whether the monitored activity was closed on or before
              // compliance date.
              local.SelCompliance.Date = entities.MonitoredActivity.ClosureDate;
              local.SelActivityLiteral.Text16 = " Activity Closed";

              if (!Lt(entities.MonitoredActivity.FedNonComplianceDate,
                entities.MonitoredActivity.ClosureDate))
              {
                local.SelFederalCompliance.Flag = "Y";
              }
              else
              {
                local.SelFederalCompliance.Flag = "N";
              }

              export.Ap.Update.DaysRemaining.Count = 0;
            }
          }

          if (AsChar(export.MultiAp.Flag) == 'Y')
          {
            export.MoreAps.Text80 = "Multiple AP's exist on this case";
          }

          export.Ap.Index = 0;
          export.Ap.Clear();

          while(!export.Ap.IsFull)
          {
            if (export.Ap.IsFull)
            {
              break;
            }

            export.Ap.Update.ActivityLiteral.Text16 =
              local.SelActivityLiteral.Text16;
            export.Ap.Update.LastVerifiedEmpDat.Assign(
              local.SelLastVerifiedEmpDate);
            MoveTextWorkArea(local.SelAltAlias, export.Ap.Update.AltAlias);
            export.Ap.Update.AddressNotVerfMsg.Text20 =
              local.SelAddressNotVerfMsg.Text20;
            export.Ap.Update.EmpIncome.TotalCurrency =
              local.SelEmpIncome.TotalCurrency;
            export.Ap.Update.Compliance.Date = local.SelCompliance.Date;
            export.Ap.Update.Start.Date = local.SelStart.Date;
            export.Ap.Update.Common.SelectChar = local.SelCommon.SelectChar;
            export.Ap.Update.OtherIncome.TotalCurrency =
              local.SelOtherIncome.TotalCurrency;
            export.Ap.Update.SesaFplsLocateRequest.Assign(local.SelSesa);
            export.Ap.Update.FplsFplsLocateRequest.Assign(
              local.SelFplsFplsLocateRequest);
            export.Ap.Update.Data1099LocateRequest.Assign(
              local.Seldata1099LocateRequest);
            export.Ap.Update.ParolOfficer.Flag = local.SelParolOfficer.Flag;
            export.Ap.Update.FederalCompliance.Flag =
              local.SelFederalCompliance.Flag;
            export.Ap.Update.DaysRemaining.Count = local.SelDaysRemaining.Count;
            export.Ap.Update.SesaCommon.Flag = local.SelSeas.Flag;
            export.Ap.Update.FplsCommon.Flag = local.SelFplsCommon.Flag;
            export.Ap.Update.MedicalInsuAvailabl.Flag =
              local.SelMedicalInsuAvailable.Flag;
            export.Ap.Update.PersonIncomeHistory.Assign(
              local.SelPersonIncomeHistory);
            export.Ap.Update.ApCsePerson.Assign(local.SelApCsePerson);
            export.Ap.Update.ApCsePersonAddress.Assign(
              local.SelApCsePersonAddress);
            export.Ap.Update.TotalChildren.Count = local.SelTotalChildren.Count;
            export.Ap.Update.ApCsePersonsWorkSet.Assign(
              local.SelApCsePersonsWorkSet);
            export.Ap.Update.Incarceration.Assign(local.SelIncarceration);
            MoveBankruptcy(local.SelBankruptcy, export.Ap.Update.Bankruptcy);
            export.Ap.Update.HiliteAddrField.Flag =
              local.SelHiliteAddrField.Flag;
            export.Ap.Next();

            break;

            export.Ap.Next();
          }
        }
        else
        {
          // --------------------------------------------------------------------------
          // User HAS NOT  selected an AP on CRIN screen and came to this 
          // screen.
          // ---------------------------------------------------------------------------
          local.NoOfApOnCase.Count = 0;

          export.Ap.Index = 0;
          export.Ap.Clear();

          foreach(var item in ReadAbsentParent())
          {
            if (ReadCsePerson2())
            {
              ++local.NoOfApOnCase.Count;
              export.Ap.Update.ApCsePerson.Assign(entities.ApCsePerson);

              if (AsChar(entities.ApCsePerson.UnemploymentInd) != 'Y')
              {
                export.Ap.Update.ApCsePerson.UnemploymentInd = "N";
              }
              else
              {
                export.Ap.Update.ApCsePerson.UnemploymentInd = "Y";
              }

              local.CsePersonsWorkSet.Number = entities.ApCsePerson.Number;
              UseSiReadCsePerson();

              if (!IsEmpty(local.AbendData.Type1))
              {
                export.Ap.Next();

                return;
              }

              MoveCsePersonsWorkSet2(local.CsePersonsWorkSet,
                export.Ap.Update.ApCsePersonsWorkSet);

              // --------------------------
              // N.Engoor - 03/11/99
              // Calling External that retrieves the Alias name if any for the 
              // corresponding AP.
              // --------------------------
              UseEabReadAlias1();

              if (!IsEmpty(local.AbendData.Type1))
              {
                export.Ap.Next();

                return;
              }

              if (!Equal(local.Alias.Item.Galias.Dob, null))
              {
                export.Ap.Update.AltAlias.Text10 = "SSN/Alias";
                export.Ap.Update.AltAlias.Text4 = "Alt";
              }

              if (ReadCsePersonAddress())
              {
                export.Ap.Update.ApCsePersonAddress.Assign(
                  entities.ApCsePersonAddress);
              }

              if (IsExitState("ACO_NN0000_ALL_OK") && !
                entities.ApCsePersonAddress.Populated)
              {
                if (local.NoOfApOnCase.Count > 1)
                {
                  if (AsChar(local.ApAddrStatus.PrevApAddrNv.Flag) == 'N' && AsChar
                    (local.ApAddrStatus.PrevApAddrNf.Flag) == 'N')
                  {
                    export.ApAddrProbMsg.Text80 =
                      "More AP's exist for this case.";
                  }
                  else if (AsChar(local.ApAddrStatus.PrevApAddrNv.Flag) == 'Y'
                    || AsChar(local.ApAddrStatus.PrevApAddrNf.Flag) == 'Y')
                  {
                    export.ApAddrProbMsg.Text80 =
                      "One or more AP's addresses unknown.";
                  }
                }
                else
                {
                  export.ApAddrProbMsg.Text80 = "";
                }

                local.ApAddrStatus.PrevApAddrNf.Flag = "Y";
                export.Ap.Update.AddressNotVerfMsg.Text20 =
                  "AP Address not found";
                export.Ap.Update.HiliteAddrField.Flag = "Y";
              }
              else if (IsExitState("ACO_NN0000_ALL_OK") && entities
                .ApCsePersonAddress.Populated && Equal
                (export.Ap.Item.ApCsePersonAddress.VerifiedDate,
                local.Initialized.Date))
              {
                if (local.NoOfApOnCase.Count > 1)
                {
                  if (AsChar(local.ApAddrStatus.PrevApAddrNv.Flag) == 'Y' && AsChar
                    (local.ApAddrStatus.PrevApAddrNf.Flag) == 'Y')
                  {
                    export.ApAddrProbMsg.Text80 =
                      "One or more AP's addresses not verified.";
                  }
                  else if (AsChar(local.ApAddrStatus.PrevApAddrNf.Flag) == 'Y')
                  {
                    export.ApAddrProbMsg.Text80 =
                      "One or more AP's addresses unknown.";
                  }
                  else
                  {
                    export.ApAddrProbMsg.Text80 =
                      "One or more AP's addresses not verified.";
                  }
                }
                else
                {
                  export.ApAddrProbMsg.Text80 = "";
                }

                local.ApAddrStatus.PrevApAddrNv.Flag = "Y";
                export.Ap.Update.AddressNotVerfMsg.Text20 =
                  "Address not verified";
                export.Ap.Update.HiliteAddrField.Flag = "Y";
              }

              if (ReadIncarceration1())
              {
                export.Ap.Update.Incarceration.Assign(entities.Incarceration);

                if (Equal(export.Ap.Item.Incarceration.EndDate, local.Max.Date))
                {
                  export.Ap.Update.Incarceration.EndDate =
                    local.Initialized.Date;
                }

                if (!Lt(local.Current.Date, entities.Incarceration.EndDate) && !
                  Equal(entities.Incarceration.EndDate, local.Initialized.Date))
                {
                  // ************************************************
                  // If the AP is out of Jail/Prison, then determine if
                  // the AP is on Parole/Probation.
                  // ************************************************
                  if (ReadIncarceration2())
                  {
                    if (!Lt(local.OpenOrClosed.Date,
                      entities.ParoleOrProbation.VerifiedDate) && (
                        Equal(entities.ParoleOrProbation.EndDate,
                      local.Initialized.Date) || !
                      Lt(entities.ParoleOrProbation.EndDate,
                      local.OpenOrClosed.Date)))
                    {
                      export.Ap.Update.ParolOfficer.Flag = "Y";
                    }
                    else
                    {
                      export.Ap.Update.ParolOfficer.Flag = "";
                    }
                  }
                }

                if (Equal(export.HiddenPassedReviewType.ActionEntry, "M") || Equal
                  (export.HiddenPassedReviewType.ActionEntry, "P"))
                {
                  // ---------------
                  // Refit Note:
                  // In the previous useage of this procedure, the concat 
                  // statements inserted the text by including the present text
                  // in an entity view.  However, that entity view was not
                  // populated prior to doing the concat.  I have changed the
                  // logic existant in the procedure by establishing currency
                  // and ensuring that the current Locate Review narrative is
                  // inserted after the "AP Ýfirst name¨" in the concat
                  // statements.
                  // SHOULD THIS CAUSE A PROBLEM, removing the read to establish
                  // currency on the current Locate Review narrative WILL
                  // RETURN the functionality of this procedure to it's prior
                  // state.
                  // RVW 1/7/97
                  // ----------------
                  local.IncarceratedFlag.Flag = "Y";
                }
              }

              // *********************************************
              // QUALIFY THE INCOME SOURCE READ WITH START DATE AND END DATE.
              // *********************************************
              local.RollingMaxVerfDt.Date = local.Initialized.Date;
              export.Ap.Update.PersonIncomeHistory.IncomeAmt = 0;
              export.Ap.Update.OtherIncome.TotalCurrency = 0;

              foreach(var item1 in ReadIncomeSource())
              {
                if (Equal(entities.IncomeSource.EndDt,
                  new DateTime(2099, 12, 31)))
                {
                  if (AsChar(entities.IncomeSource.Type1) == 'E')
                  {
                    if (AsChar(entities.IncomeSource.ReturnCd) == 'E' || AsChar
                      (entities.IncomeSource.ReturnCd) == 'L' || AsChar
                      (entities.IncomeSource.ReturnCd) == 'W' || AsChar
                      (entities.IncomeSource.ReturnCd) == 'O')
                    {
                      export.Ap.Update.LastVerifiedEmpDat.ReturnDt =
                        entities.IncomeSource.ReturnDt;
                    }
                  }

                  if (AsChar(entities.IncomeSource.Type1) == 'M')
                  {
                    if (AsChar(entities.IncomeSource.ReturnCd) == 'A' || AsChar
                      (entities.IncomeSource.ReturnCd) == 'R')
                    {
                      export.Ap.Update.LastVerifiedEmpDat.ReturnDt =
                        entities.IncomeSource.ReturnDt;
                    }
                  }

                  if (AsChar(entities.IncomeSource.Type1) == 'O')
                  {
                    if (AsChar(entities.IncomeSource.ReturnCd) == 'V')
                    {
                      export.Ap.Update.LastVerifiedEmpDat.ReturnDt =
                        entities.IncomeSource.ReturnDt;
                    }
                  }

                  if (AsChar(entities.IncomeSource.Type1) == 'R')
                  {
                    if (AsChar(entities.IncomeSource.ReturnCd) == 'V')
                    {
                      export.Ap.Update.LastVerifiedEmpDat.ReturnDt =
                        entities.IncomeSource.ReturnDt;
                    }
                  }
                }

                if (Equal(export.Ap.Item.LastVerifiedEmpDat.ReturnDt,
                  local.BlankDate.Date))
                {
                  continue;
                }

                export.Ap.Update.LastVerifiedEmpDat.LastQtrIncome =
                  entities.IncomeSource.LastQtrIncome;
                export.Ap.Update.LastVerifiedEmpDat.LastQtr =
                  entities.IncomeSource.LastQtr;
                export.Ap.Update.LastVerifiedEmpDat.LastQtrYr =
                  entities.IncomeSource.LastQtrYr;

                break;
              }

              export.Ap.Update.PersonIncomeHistory.VerifiedDt =
                local.RollingMaxVerfDt.Date;

              if (ReadFplsLocateRequest())
              {
                export.Ap.Update.FplsFplsLocateRequest.Assign(
                  entities.FplsLocateRequest);
              }

              if (Read1099LocateRequest())
              {
                export.Ap.Update.Data1099LocateRequest.Assign(
                  entities.Data1099LocateRequest);
              }
            }

            export.Ap.Update.ActivityLiteral.Text16 = "Required Cmpt Dt";

            if (ReadMonitoredActivityInfrastructure1())
            {
              export.Ap.Update.Start.Date =
                entities.MonitoredActivity.StartDate;

              if (Equal(entities.MonitoredActivity.ClosureDate,
                local.Initialized.Date) || Equal
                (entities.MonitoredActivity.ClosureDate, local.Max.Date))
              {
                export.Ap.Update.Compliance.Date =
                  entities.MonitoredActivity.FedNonComplianceDate;
                export.Ap.Update.ActivityLiteral.Text16 = "Required Cmpt Dt";

                // Indicates that the monitored activity has not been closed,
                // therefore AP has not been located.
                if (!Lt(local.Current.Date,
                  entities.MonitoredActivity.FedNonComplianceDate))
                {
                  export.Ap.Update.FederalCompliance.Flag = "N";
                  export.Ap.Update.DaysRemaining.Count = 0;
                }
                else if (Lt(local.Current.Date,
                  entities.MonitoredActivity.FedNonComplianceDate))
                {
                  export.Ap.Update.FederalCompliance.Flag = "";
                  export.Ap.Update.DaysRemaining.Count =
                    DaysFromAD(entities.MonitoredActivity.FedNonComplianceDate) -
                    DaysFromAD(local.Current.Date);
                }
              }
              else
              {
                // Indicates monitored activity has been closed, and other 
                // fields populated (ap address, etc.) indicate whether or not
                // he's been found.   Therefore, compliance indicator is set
                // based on whether the monitored activity was closed on or
                // before compliance date.
                export.Ap.Update.Compliance.Date =
                  entities.MonitoredActivity.ClosureDate;
                export.Ap.Update.ActivityLiteral.Text16 = " Activity Closed";

                if (!Lt(entities.MonitoredActivity.FedNonComplianceDate,
                  entities.MonitoredActivity.ClosureDate))
                {
                  export.Ap.Update.FederalCompliance.Flag = "Y";
                }
                else
                {
                  export.Ap.Update.FederalCompliance.Flag = "N";
                }

                export.Ap.Update.DaysRemaining.Count = 0;
              }
            }

            export.Ap.Next();
          }

          switch(local.NoOfApOnCase.Count)
          {
            case 2:
              export.MoreAps.Text80 = "Multiple AP's exist on this case";

              break;
            case 1:
              break;
            default:
              break;
          }
        }

        if (Equal(export.HiddenPassedReviewType.ActionEntry, "R"))
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
              entities.LocateReview.NarrativeText;

            if (local.Work.Index == 3)
            {
              break;
            }
          }

          if (!local.Work.IsEmpty)
          {
            local.Work.Index = 0;
            local.Work.CheckSize();

            export.LocateReview.Text =
              Substring(local.Work.Item.Work1.NarrativeText, 11, 58);

            local.Work.Index = 1;
            local.Work.CheckSize();

            if (!IsEmpty(local.Work.Item.Work1.NarrativeText))
            {
              export.LocateReview.Text = TrimEnd(export.LocateReview.Text) + Substring
                (local.Work.Item.Work1.NarrativeText, 68, 11, 58);
            }

            local.Work.Index = 2;
            local.Work.CheckSize();

            if (!IsEmpty(local.Work.Item.Work1.NarrativeText))
            {
              export.LocateReview.Text = TrimEnd(export.LocateReview.Text) + Substring
                (local.Work.Item.Work1.NarrativeText, 68, 11, 58);
            }

            local.Work.Index = 3;
            local.Work.CheckSize();

            if (!IsEmpty(local.Work.Item.Work1.NarrativeText))
            {
              export.LocateReview.Text = TrimEnd(export.LocateReview.Text) + Substring
                (local.Work.Item.Work1.NarrativeText, 68, 11, 58);
            }
          }

          // *** end
          // *** 08/08/00 SWSRCHF
          // *** Work request 000170
          var field = GetField(export.LocateReview, "text");

          field.Color = "cyan";
          field.Protected = true;
        }
        else if (!export.HiddenPassed.IsEmpty)
        {
          export.HiddenPassed.Index = 0;
          export.HiddenPassed.CheckSize();

          export.LocateReview.Text = export.HiddenPassed.Item.GexportH.Text;
        }

        if (IsEmpty(export.LocateReview.Text))
        {
          ExitState = "SP0000_FIRST_REVIEW_4_CASE";
        }
        else if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }

        break;

        if (AsChar(local.SsnNotFoundFlag.Flag) == 'Y')
        {
          ExitState = "SP0000_SSN_NOT_RECORDED";
        }

        break;
      case "ENTER":
        if (IsEmpty(export.LocateReview.Text))
        {
          if (Equal(export.HiddenPassedReviewType.ActionEntry, "R"))
          {
          }
          else
          {
            var field = GetField(export.LocateReview, "text");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

            return;
          }
        }
        else
        {
          export.HiddenPassed.Index = 0;
          export.HiddenPassed.CheckSize();

          export.HiddenPassed.Update.GexportH.Text = export.LocateReview.Text;
        }

        ExitState = "ECO_LNK_TO_CR_MEDICAL";

        return;

        // --------------------
        // N.Engoor  -  03/25/99
        // Commented out code.
        // --------------------
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
      var field = GetField(export.LocateReview, "text");

      field.Color = "cyan";
      field.Protected = true;
    }
  }

  private static void MoveAlias(Local.AliasGroup source,
    EabReadAlias.Export.AliasesGroup target)
  {
    target.G.Assign(source.Galias);
    target.Gkscares.Flag = source.Gkscares.Flag;
    target.Gkanpay.Flag = source.Gkanpay.Flag;
    target.Gcse.Flag = source.Gcse.Flag;
    target.Gae.Flag = source.Gae.Flag;
  }

  private static void MoveAliases(EabReadAlias.Export.AliasesGroup source,
    Local.AliasGroup target)
  {
    target.Galias.Assign(source.G);
    target.Gkscares.Flag = source.Gkscares.Flag;
    target.Gkanpay.Flag = source.Gkanpay.Flag;
    target.Gcse.Flag = source.Gcse.Flag;
    target.Gae.Flag = source.Gae.Flag;
  }

  private static void MoveBankruptcy(Bankruptcy source, Bankruptcy target)
  {
    target.BankruptcyFilingDate = source.BankruptcyFilingDate;
    target.BankruptcyConfirmationDate = source.BankruptcyConfirmationDate;
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
    target.Ssn = source.Ssn;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveCsePersonsWorkSet3(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveTextWorkArea(TextWorkArea source, TextWorkArea target)
  {
    target.Text4 = source.Text4;
    target.Text10 = source.Text10;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.Max.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseEabReadAlias1()
  {
    var useImport = new EabReadAlias.Import();
    var useExport = new EabReadAlias.Export();

    useImport.CsePersonsWorkSet.Number =
      export.Ap.Item.ApCsePersonsWorkSet.Number;
    local.Alias.CopyTo(useExport.Aliases, MoveAlias);

    Call(EabReadAlias.Execute, useImport, useExport);

    useExport.Aliases.CopyTo(local.Alias, MoveAliases);
  }

  private void UseEabReadAlias2()
  {
    var useImport = new EabReadAlias.Import();
    var useExport = new EabReadAlias.Export();

    useImport.CsePersonsWorkSet.Number = local.SelApCsePersonsWorkSet.Number;
    local.Alias.CopyTo(useExport.Aliases, MoveAlias);

    Call(EabReadAlias.Execute, useImport, useExport);

    useExport.Aliases.CopyTo(local.Alias, MoveAliases);
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

    useImport.CsePersonsWorkSet.Number = import.ApCsePersonsWorkSet.Number;
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

  private bool Read1099LocateRequest()
  {
    entities.Data1099LocateRequest.Populated = false;

    return Read("Read1099LocateRequest",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Data1099LocateRequest.CspNumber = db.GetString(reader, 0);
        entities.Data1099LocateRequest.Identifier = db.GetInt32(reader, 1);
        entities.Data1099LocateRequest.CreatedTimestamp =
          db.GetDateTime(reader, 2);
        entities.Data1099LocateRequest.RequestSentDate =
          db.GetNullableDate(reader, 3);
        entities.Data1099LocateRequest.Populated = true;
      });
  }

  private IEnumerable<bool> ReadAbsentParent()
  {
    return ReadEach("ReadAbsentParent",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.OpenOrClosed.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Ap.IsFull)
        {
          return false;
        }

        entities.AbsentParent.CasNumber = db.GetString(reader, 0);
        entities.AbsentParent.CspNumber = db.GetString(reader, 1);
        entities.AbsentParent.Type1 = db.GetString(reader, 2);
        entities.AbsentParent.Identifier = db.GetInt32(reader, 3);
        entities.AbsentParent.StartDate = db.GetNullableDate(reader, 4);
        entities.AbsentParent.EndDate = db.GetNullableDate(reader, 5);
        entities.AbsentParent.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.AbsentParent.Populated = true;
        CheckValid<CaseRole>("Type1", entities.AbsentParent.Type1);

        return true;
      });
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

  private bool ReadCsePerson1()
  {
    entities.ApCsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.
          SetString(command, "numb", export.SelectedApCsePersonsWorkSet.Number);
          
      },
      (db, reader) =>
      {
        entities.ApCsePerson.Number = db.GetString(reader, 0);
        entities.ApCsePerson.Type1 = db.GetString(reader, 1);
        entities.ApCsePerson.AeCaseNumber = db.GetNullableString(reader, 2);
        entities.ApCsePerson.DateOfDeath = db.GetNullableDate(reader, 3);
        entities.ApCsePerson.CreatedBy = db.GetString(reader, 4);
        entities.ApCsePerson.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.ApCsePerson.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.ApCsePerson.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.ApCsePerson.UnemploymentInd = db.GetNullableString(reader, 8);
        entities.ApCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ApCsePerson.Type1);
      });
  }

  private bool ReadCsePerson2()
  {
    System.Diagnostics.Debug.Assert(entities.AbsentParent.Populated);
    entities.ApCsePerson.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.AbsentParent.CspNumber);
      },
      (db, reader) =>
      {
        entities.ApCsePerson.Number = db.GetString(reader, 0);
        entities.ApCsePerson.Type1 = db.GetString(reader, 1);
        entities.ApCsePerson.AeCaseNumber = db.GetNullableString(reader, 2);
        entities.ApCsePerson.DateOfDeath = db.GetNullableDate(reader, 3);
        entities.ApCsePerson.CreatedBy = db.GetString(reader, 4);
        entities.ApCsePerson.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.ApCsePerson.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.ApCsePerson.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.ApCsePerson.UnemploymentInd = db.GetNullableString(reader, 8);
        entities.ApCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ApCsePerson.Type1);
      });
  }

  private bool ReadCsePersonAddress()
  {
    entities.ApCsePersonAddress.Populated = false;

    return Read("ReadCsePersonAddress",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ApCsePersonAddress.Identifier = db.GetDateTime(reader, 0);
        entities.ApCsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.ApCsePersonAddress.VerifiedDate =
          db.GetNullableDate(reader, 2);
        entities.ApCsePersonAddress.EndDate = db.GetNullableDate(reader, 3);
        entities.ApCsePersonAddress.EndCode = db.GetNullableString(reader, 4);
        entities.ApCsePersonAddress.Populated = true;
      });
  }

  private bool ReadFplsLocateRequest()
  {
    entities.FplsLocateRequest.Populated = false;

    return Read("ReadFplsLocateRequest",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.FplsLocateRequest.CspNumber = db.GetString(reader, 0);
        entities.FplsLocateRequest.Identifier = db.GetInt32(reader, 1);
        entities.FplsLocateRequest.TransactionType =
          db.GetNullableString(reader, 2);
        entities.FplsLocateRequest.UsersField = db.GetNullableString(reader, 3);
        entities.FplsLocateRequest.RequestSentDate =
          db.GetNullableDate(reader, 4);
        entities.FplsLocateRequest.SendRequestTo =
          db.GetNullableString(reader, 5);
        entities.FplsLocateRequest.Populated = true;
      });
  }

  private bool ReadIncarceration1()
  {
    entities.Incarceration.Populated = false;

    return Read("ReadIncarceration1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
        db.
          SetDate(command, "date", local.OpenOrClosed.Date.GetValueOrDefault());
          
        db.SetNullableDate(
          command, "endDate", local.Initialized.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Incarceration.CspNumber = db.GetString(reader, 0);
        entities.Incarceration.Identifier = db.GetInt32(reader, 1);
        entities.Incarceration.VerifiedDate = db.GetNullableDate(reader, 2);
        entities.Incarceration.EndDate = db.GetNullableDate(reader, 3);
        entities.Incarceration.StartDate = db.GetNullableDate(reader, 4);
        entities.Incarceration.Type1 = db.GetNullableString(reader, 5);
        entities.Incarceration.ParoleOfficerLastName =
          db.GetNullableString(reader, 6);
        entities.Incarceration.Populated = true;
      });
  }

  private bool ReadIncarceration2()
  {
    entities.ParoleOrProbation.Populated = false;

    return Read("ReadIncarceration2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
        db.SetNullableDate(
          command, "verifiedDate",
          export.Ap.Item.Incarceration.EndDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDate", local.OpenOrClosed.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ParoleOrProbation.CspNumber = db.GetString(reader, 0);
        entities.ParoleOrProbation.Identifier = db.GetInt32(reader, 1);
        entities.ParoleOrProbation.VerifiedDate = db.GetNullableDate(reader, 2);
        entities.ParoleOrProbation.EndDate = db.GetNullableDate(reader, 3);
        entities.ParoleOrProbation.StartDate = db.GetNullableDate(reader, 4);
        entities.ParoleOrProbation.Type1 = db.GetNullableString(reader, 5);
        entities.ParoleOrProbation.Populated = true;
      });
  }

  private bool ReadIncarceration3()
  {
    entities.ParoleOrProbation.Populated = false;

    return Read("ReadIncarceration3",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
        db.SetNullableDate(
          command, "verifiedDate",
          local.SelIncarceration.EndDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDate", local.OpenOrClosed.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ParoleOrProbation.CspNumber = db.GetString(reader, 0);
        entities.ParoleOrProbation.Identifier = db.GetInt32(reader, 1);
        entities.ParoleOrProbation.VerifiedDate = db.GetNullableDate(reader, 2);
        entities.ParoleOrProbation.EndDate = db.GetNullableDate(reader, 3);
        entities.ParoleOrProbation.StartDate = db.GetNullableDate(reader, 4);
        entities.ParoleOrProbation.Type1 = db.GetNullableString(reader, 5);
        entities.ParoleOrProbation.Populated = true;
      });
  }

  private IEnumerable<bool> ReadIncomeSource()
  {
    entities.IncomeSource.Populated = false;

    return ReadEach("ReadIncomeSource",
      (db, command) =>
      {
        db.SetString(command, "cspINumber", entities.ApCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.IncomeSource.Type1 = db.GetString(reader, 1);
        entities.IncomeSource.LastQtrIncome = db.GetNullableDecimal(reader, 2);
        entities.IncomeSource.LastQtr = db.GetNullableString(reader, 3);
        entities.IncomeSource.LastQtrYr = db.GetNullableInt32(reader, 4);
        entities.IncomeSource.Attribute2NdQtrIncome =
          db.GetNullableDecimal(reader, 5);
        entities.IncomeSource.Attribute3RdQtrIncome =
          db.GetNullableDecimal(reader, 6);
        entities.IncomeSource.Attribute4ThQtrIncome =
          db.GetNullableDecimal(reader, 7);
        entities.IncomeSource.ReturnDt = db.GetNullableDate(reader, 8);
        entities.IncomeSource.ReturnCd = db.GetNullableString(reader, 9);
        entities.IncomeSource.CspINumber = db.GetString(reader, 10);
        entities.IncomeSource.StartDt = db.GetNullableDate(reader, 11);
        entities.IncomeSource.EndDt = db.GetNullableDate(reader, 12);
        entities.IncomeSource.Populated = true;
        CheckValid<IncomeSource>("Type1", entities.IncomeSource.Type1);

        return true;
      });
  }

  private bool ReadMonitoredActivityInfrastructure1()
  {
    entities.MonitoredActivity.Populated = false;
    entities.LastLocated.Populated = false;

    return Read("ReadMonitoredActivityInfrastructure1",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", import.Case1.Number);
        db.SetNullableString(
          command, "csePersonNum", export.Ap.Item.ApCsePersonsWorkSet.Number);
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
        entities.LastLocated.SystemGeneratedIdentifier = db.GetInt32(reader, 6);
        entities.LastLocated.EventId = db.GetInt32(reader, 7);
        entities.LastLocated.EventType = db.GetString(reader, 8);
        entities.LastLocated.ReasonCode = db.GetString(reader, 9);
        entities.LastLocated.BusinessObjectCd = db.GetString(reader, 10);
        entities.LastLocated.CaseNumber = db.GetNullableString(reader, 11);
        entities.LastLocated.CsePersonNumber = db.GetNullableString(reader, 12);
        entities.LastLocated.CreatedBy = db.GetString(reader, 13);
        entities.LastLocated.CreatedTimestamp = db.GetDateTime(reader, 14);
        entities.LastLocated.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 15);
        entities.LastLocated.Function = db.GetNullableString(reader, 16);
        entities.MonitoredActivity.Populated = true;
        entities.LastLocated.Populated = true;
      });
  }

  private bool ReadMonitoredActivityInfrastructure2()
  {
    entities.MonitoredActivity.Populated = false;
    entities.LastLocated.Populated = false;

    return Read("ReadMonitoredActivityInfrastructure2",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", import.Case1.Number);
        db.SetNullableString(
          command, "csePersonNum", local.SelApCsePersonsWorkSet.Number);
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
        entities.LastLocated.SystemGeneratedIdentifier = db.GetInt32(reader, 6);
        entities.LastLocated.EventId = db.GetInt32(reader, 7);
        entities.LastLocated.EventType = db.GetString(reader, 8);
        entities.LastLocated.ReasonCode = db.GetString(reader, 9);
        entities.LastLocated.BusinessObjectCd = db.GetString(reader, 10);
        entities.LastLocated.CaseNumber = db.GetNullableString(reader, 11);
        entities.LastLocated.CsePersonNumber = db.GetNullableString(reader, 12);
        entities.LastLocated.CreatedBy = db.GetString(reader, 13);
        entities.LastLocated.CreatedTimestamp = db.GetDateTime(reader, 14);
        entities.LastLocated.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 15);
        entities.LastLocated.Function = db.GetNullableString(reader, 16);
        entities.MonitoredActivity.Populated = true;
        entities.LastLocated.Populated = true;
      });
  }

  private IEnumerable<bool> ReadNarrativeDetail()
  {
    entities.LocateReview.Populated = false;

    return ReadEach("ReadNarrativeDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "infrastructureId",
          export.HiddenPassed1.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.LocateReview.InfrastructureId = db.GetInt32(reader, 0);
        entities.LocateReview.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.LocateReview.NarrativeText = db.GetNullableString(reader, 2);
        entities.LocateReview.LineNumber = db.GetInt32(reader, 3);
        entities.LocateReview.Populated = true;

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

    /// <summary>A ApGroup group.</summary>
    [Serializable]
    public class ApGroup
    {
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
      /// A value of LastVerifiedEmpDat.
      /// </summary>
      [JsonPropertyName("lastVerifiedEmpDat")]
      public IncomeSource LastVerifiedEmpDat
      {
        get => lastVerifiedEmpDat ??= new();
        set => lastVerifiedEmpDat = value;
      }

      /// <summary>
      /// A value of AltAlias.
      /// </summary>
      [JsonPropertyName("altAlias")]
      public TextWorkArea AltAlias
      {
        get => altAlias ??= new();
        set => altAlias = value;
      }

      /// <summary>
      /// A value of AddressNotVerfMsg.
      /// </summary>
      [JsonPropertyName("addressNotVerfMsg")]
      public SpTextWorkArea AddressNotVerfMsg
      {
        get => addressNotVerfMsg ??= new();
        set => addressNotVerfMsg = value;
      }

      /// <summary>
      /// A value of EmpIncome.
      /// </summary>
      [JsonPropertyName("empIncome")]
      public Common EmpIncome
      {
        get => empIncome ??= new();
        set => empIncome = value;
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
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>
      /// A value of OtherIncome.
      /// </summary>
      [JsonPropertyName("otherIncome")]
      public Common OtherIncome
      {
        get => otherIncome ??= new();
        set => otherIncome = value;
      }

      /// <summary>
      /// A value of Sesa.
      /// </summary>
      [JsonPropertyName("sesa")]
      public FplsLocateRequest Sesa
      {
        get => sesa ??= new();
        set => sesa = value;
      }

      /// <summary>
      /// A value of Fpls.
      /// </summary>
      [JsonPropertyName("fpls")]
      public FplsLocateRequest Fpls
      {
        get => fpls ??= new();
        set => fpls = value;
      }

      /// <summary>
      /// A value of Data1099LocateRequest.
      /// </summary>
      [JsonPropertyName("data1099LocateRequest")]
      public Data1099LocateRequest Data1099LocateRequest
      {
        get => data1099LocateRequest ??= new();
        set => data1099LocateRequest = value;
      }

      /// <summary>
      /// A value of ParolOfficer.
      /// </summary>
      [JsonPropertyName("parolOfficer")]
      public Common ParolOfficer
      {
        get => parolOfficer ??= new();
        set => parolOfficer = value;
      }

      /// <summary>
      /// A value of FederalCompliance.
      /// </summary>
      [JsonPropertyName("federalCompliance")]
      public Common FederalCompliance
      {
        get => federalCompliance ??= new();
        set => federalCompliance = value;
      }

      /// <summary>
      /// A value of DaysRemaining.
      /// </summary>
      [JsonPropertyName("daysRemaining")]
      public Common DaysRemaining
      {
        get => daysRemaining ??= new();
        set => daysRemaining = value;
      }

      /// <summary>
      /// A value of MedicalInsuAvailabl.
      /// </summary>
      [JsonPropertyName("medicalInsuAvailabl")]
      public Common MedicalInsuAvailabl
      {
        get => medicalInsuAvailabl ??= new();
        set => medicalInsuAvailabl = value;
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
      /// A value of ApCsePerson.
      /// </summary>
      [JsonPropertyName("apCsePerson")]
      public CsePerson ApCsePerson
      {
        get => apCsePerson ??= new();
        set => apCsePerson = value;
      }

      /// <summary>
      /// A value of ApCsePersonAddress.
      /// </summary>
      [JsonPropertyName("apCsePersonAddress")]
      public CsePersonAddress ApCsePersonAddress
      {
        get => apCsePersonAddress ??= new();
        set => apCsePersonAddress = value;
      }

      /// <summary>
      /// A value of TotalChildren.
      /// </summary>
      [JsonPropertyName("totalChildren")]
      public Common TotalChildren
      {
        get => totalChildren ??= new();
        set => totalChildren = value;
      }

      /// <summary>
      /// A value of ApCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("apCsePersonsWorkSet")]
      public CsePersonsWorkSet ApCsePersonsWorkSet
      {
        get => apCsePersonsWorkSet ??= new();
        set => apCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of Incarceration.
      /// </summary>
      [JsonPropertyName("incarceration")]
      public Incarceration Incarceration
      {
        get => incarceration ??= new();
        set => incarceration = value;
      }

      /// <summary>
      /// A value of Bankruptcy.
      /// </summary>
      [JsonPropertyName("bankruptcy")]
      public Bankruptcy Bankruptcy
      {
        get => bankruptcy ??= new();
        set => bankruptcy = value;
      }

      /// <summary>
      /// A value of HiliteAddrField.
      /// </summary>
      [JsonPropertyName("hiliteAddrField")]
      public Common HiliteAddrField
      {
        get => hiliteAddrField ??= new();
        set => hiliteAddrField = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 15;

      private WorkArea activityLiteral;
      private IncomeSource lastVerifiedEmpDat;
      private TextWorkArea altAlias;
      private SpTextWorkArea addressNotVerfMsg;
      private Common empIncome;
      private DateWorkArea compliance;
      private DateWorkArea start;
      private Common common;
      private Common otherIncome;
      private FplsLocateRequest sesa;
      private FplsLocateRequest fpls;
      private Data1099LocateRequest data1099LocateRequest;
      private Common parolOfficer;
      private Common federalCompliance;
      private Common daysRemaining;
      private Common medicalInsuAvailabl;
      private PersonIncomeHistory personIncomeHistory;
      private CsePerson apCsePerson;
      private CsePersonAddress apCsePersonAddress;
      private Common totalChildren;
      private CsePersonsWorkSet apCsePersonsWorkSet;
      private Incarceration incarceration;
      private Bankruptcy bankruptcy;
      private Common hiliteAddrField;
    }

    /// <summary>
    /// A value of MoreAps.
    /// </summary>
    [JsonPropertyName("moreAps")]
    public SpTextWorkArea MoreAps
    {
      get => moreAps ??= new();
      set => moreAps = value;
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
    /// A value of ApAddrProbMsg.
    /// </summary>
    [JsonPropertyName("apAddrProbMsg")]
    public SpTextWorkArea ApAddrProbMsg
    {
      get => apAddrProbMsg ??= new();
      set => apAddrProbMsg = value;
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

    /// <summary>
    /// A value of LocateReview.
    /// </summary>
    [JsonPropertyName("locateReview")]
    public NarrativeWork LocateReview
    {
      get => locateReview ??= new();
      set => locateReview = value;
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
    /// A value of HiddenPassedReviewType.
    /// </summary>
    [JsonPropertyName("hiddenPassedReviewType")]
    public Common HiddenPassedReviewType
    {
      get => hiddenPassedReviewType ??= new();
      set => hiddenPassedReviewType = value;
    }

    /// <summary>
    /// Gets a value of Ap.
    /// </summary>
    [JsonIgnore]
    public Array<ApGroup> Ap => ap ??= new(ApGroup.Capacity);

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
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
    }

    /// <summary>
    /// A value of TotalChildren.
    /// </summary>
    [JsonPropertyName("totalChildren")]
    public Common TotalChildren
    {
      get => totalChildren ??= new();
      set => totalChildren = value;
    }

    /// <summary>
    /// A value of ApCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("apCsePersonsWorkSet")]
    public CsePersonsWorkSet ApCsePersonsWorkSet
    {
      get => apCsePersonsWorkSet ??= new();
      set => apCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Incarceration.
    /// </summary>
    [JsonPropertyName("incarceration")]
    public Incarceration Incarceration
    {
      get => incarceration ??= new();
      set => incarceration = value;
    }

    /// <summary>
    /// A value of Bankruptcy.
    /// </summary>
    [JsonPropertyName("bankruptcy")]
    public Bankruptcy Bankruptcy
    {
      get => bankruptcy ??= new();
      set => bankruptcy = value;
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
    /// A value of MultiAp.
    /// </summary>
    [JsonPropertyName("multiAp")]
    public Common MultiAp
    {
      get => multiAp ??= new();
      set => multiAp = value;
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

    private SpTextWorkArea moreAps;
    private Common caseClosedIndicator;
    private SpTextWorkArea apAddrProbMsg;
    private Infrastructure hiddenPassed1;
    private NarrativeWork locateReview;
    private Array<HiddenPassedGroup> hiddenPassed;
    private Common hiddenPassedReviewType;
    private Array<ApGroup> ap;
    private CsePerson apCsePerson;
    private Common totalChildren;
    private CsePersonsWorkSet apCsePersonsWorkSet;
    private Incarceration incarceration;
    private Bankruptcy bankruptcy;
    private Case1 case1;
    private NextTranInfo hidden;
    private Standard standard;
    private WorkArea activityLiteral;
    private Common multiAp;
    private CsePersonsWorkSet ap1;
    private Common apSelected;
    private CsePersonsWorkSet selectedAp;
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

    /// <summary>A ApGroup group.</summary>
    [Serializable]
    public class ApGroup
    {
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
      /// A value of LastVerifiedEmpDat.
      /// </summary>
      [JsonPropertyName("lastVerifiedEmpDat")]
      public IncomeSource LastVerifiedEmpDat
      {
        get => lastVerifiedEmpDat ??= new();
        set => lastVerifiedEmpDat = value;
      }

      /// <summary>
      /// A value of AltAlias.
      /// </summary>
      [JsonPropertyName("altAlias")]
      public TextWorkArea AltAlias
      {
        get => altAlias ??= new();
        set => altAlias = value;
      }

      /// <summary>
      /// A value of AddressNotVerfMsg.
      /// </summary>
      [JsonPropertyName("addressNotVerfMsg")]
      public SpTextWorkArea AddressNotVerfMsg
      {
        get => addressNotVerfMsg ??= new();
        set => addressNotVerfMsg = value;
      }

      /// <summary>
      /// A value of EmpIncome.
      /// </summary>
      [JsonPropertyName("empIncome")]
      public Common EmpIncome
      {
        get => empIncome ??= new();
        set => empIncome = value;
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
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>
      /// A value of OtherIncome.
      /// </summary>
      [JsonPropertyName("otherIncome")]
      public Common OtherIncome
      {
        get => otherIncome ??= new();
        set => otherIncome = value;
      }

      /// <summary>
      /// A value of SesaFplsLocateRequest.
      /// </summary>
      [JsonPropertyName("sesaFplsLocateRequest")]
      public FplsLocateRequest SesaFplsLocateRequest
      {
        get => sesaFplsLocateRequest ??= new();
        set => sesaFplsLocateRequest = value;
      }

      /// <summary>
      /// A value of FplsFplsLocateRequest.
      /// </summary>
      [JsonPropertyName("fplsFplsLocateRequest")]
      public FplsLocateRequest FplsFplsLocateRequest
      {
        get => fplsFplsLocateRequest ??= new();
        set => fplsFplsLocateRequest = value;
      }

      /// <summary>
      /// A value of Data1099LocateRequest.
      /// </summary>
      [JsonPropertyName("data1099LocateRequest")]
      public Data1099LocateRequest Data1099LocateRequest
      {
        get => data1099LocateRequest ??= new();
        set => data1099LocateRequest = value;
      }

      /// <summary>
      /// A value of ParolOfficer.
      /// </summary>
      [JsonPropertyName("parolOfficer")]
      public Common ParolOfficer
      {
        get => parolOfficer ??= new();
        set => parolOfficer = value;
      }

      /// <summary>
      /// A value of FederalCompliance.
      /// </summary>
      [JsonPropertyName("federalCompliance")]
      public Common FederalCompliance
      {
        get => federalCompliance ??= new();
        set => federalCompliance = value;
      }

      /// <summary>
      /// A value of DaysRemaining.
      /// </summary>
      [JsonPropertyName("daysRemaining")]
      public Common DaysRemaining
      {
        get => daysRemaining ??= new();
        set => daysRemaining = value;
      }

      /// <summary>
      /// A value of SesaCommon.
      /// </summary>
      [JsonPropertyName("sesaCommon")]
      public Common SesaCommon
      {
        get => sesaCommon ??= new();
        set => sesaCommon = value;
      }

      /// <summary>
      /// A value of FplsCommon.
      /// </summary>
      [JsonPropertyName("fplsCommon")]
      public Common FplsCommon
      {
        get => fplsCommon ??= new();
        set => fplsCommon = value;
      }

      /// <summary>
      /// A value of MedicalInsuAvailabl.
      /// </summary>
      [JsonPropertyName("medicalInsuAvailabl")]
      public Common MedicalInsuAvailabl
      {
        get => medicalInsuAvailabl ??= new();
        set => medicalInsuAvailabl = value;
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
      /// A value of ApCsePerson.
      /// </summary>
      [JsonPropertyName("apCsePerson")]
      public CsePerson ApCsePerson
      {
        get => apCsePerson ??= new();
        set => apCsePerson = value;
      }

      /// <summary>
      /// A value of ApCsePersonAddress.
      /// </summary>
      [JsonPropertyName("apCsePersonAddress")]
      public CsePersonAddress ApCsePersonAddress
      {
        get => apCsePersonAddress ??= new();
        set => apCsePersonAddress = value;
      }

      /// <summary>
      /// A value of TotalChildren.
      /// </summary>
      [JsonPropertyName("totalChildren")]
      public Common TotalChildren
      {
        get => totalChildren ??= new();
        set => totalChildren = value;
      }

      /// <summary>
      /// A value of ApCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("apCsePersonsWorkSet")]
      public CsePersonsWorkSet ApCsePersonsWorkSet
      {
        get => apCsePersonsWorkSet ??= new();
        set => apCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of Incarceration.
      /// </summary>
      [JsonPropertyName("incarceration")]
      public Incarceration Incarceration
      {
        get => incarceration ??= new();
        set => incarceration = value;
      }

      /// <summary>
      /// A value of Bankruptcy.
      /// </summary>
      [JsonPropertyName("bankruptcy")]
      public Bankruptcy Bankruptcy
      {
        get => bankruptcy ??= new();
        set => bankruptcy = value;
      }

      /// <summary>
      /// A value of HiliteAddrField.
      /// </summary>
      [JsonPropertyName("hiliteAddrField")]
      public Common HiliteAddrField
      {
        get => hiliteAddrField ??= new();
        set => hiliteAddrField = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 15;

      private WorkArea activityLiteral;
      private IncomeSource lastVerifiedEmpDat;
      private TextWorkArea altAlias;
      private SpTextWorkArea addressNotVerfMsg;
      private Common empIncome;
      private DateWorkArea compliance;
      private DateWorkArea start;
      private Common common;
      private Common otherIncome;
      private FplsLocateRequest sesaFplsLocateRequest;
      private FplsLocateRequest fplsFplsLocateRequest;
      private Data1099LocateRequest data1099LocateRequest;
      private Common parolOfficer;
      private Common federalCompliance;
      private Common daysRemaining;
      private Common sesaCommon;
      private Common fplsCommon;
      private Common medicalInsuAvailabl;
      private PersonIncomeHistory personIncomeHistory;
      private CsePerson apCsePerson;
      private CsePersonAddress apCsePersonAddress;
      private Common totalChildren;
      private CsePersonsWorkSet apCsePersonsWorkSet;
      private Incarceration incarceration;
      private Bankruptcy bankruptcy;
      private Common hiliteAddrField;
    }

    /// <summary>
    /// A value of MoreAps.
    /// </summary>
    [JsonPropertyName("moreAps")]
    public SpTextWorkArea MoreAps
    {
      get => moreAps ??= new();
      set => moreAps = value;
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
    /// A value of ApAddrProbMsg.
    /// </summary>
    [JsonPropertyName("apAddrProbMsg")]
    public SpTextWorkArea ApAddrProbMsg
    {
      get => apAddrProbMsg ??= new();
      set => apAddrProbMsg = value;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public FplsLocateRequest Selected
    {
      get => selected ??= new();
      set => selected = value;
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
    /// A value of LocateReview.
    /// </summary>
    [JsonPropertyName("locateReview")]
    public NarrativeWork LocateReview
    {
      get => locateReview ??= new();
      set => locateReview = value;
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
    /// Gets a value of Ap.
    /// </summary>
    [JsonIgnore]
    public Array<ApGroup> Ap => ap ??= new(ApGroup.Capacity);

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
    /// A value of SelectedApCsePerson.
    /// </summary>
    [JsonPropertyName("selectedApCsePerson")]
    public CsePerson SelectedApCsePerson
    {
      get => selectedApCsePerson ??= new();
      set => selectedApCsePerson = value;
    }

    /// <summary>
    /// A value of TotalChildren.
    /// </summary>
    [JsonPropertyName("totalChildren")]
    public Common TotalChildren
    {
      get => totalChildren ??= new();
      set => totalChildren = value;
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
    /// A value of Incarceration.
    /// </summary>
    [JsonPropertyName("incarceration")]
    public Incarceration Incarceration
    {
      get => incarceration ??= new();
      set => incarceration = value;
    }

    /// <summary>
    /// A value of Bankruptcy.
    /// </summary>
    [JsonPropertyName("bankruptcy")]
    public Bankruptcy Bankruptcy
    {
      get => bankruptcy ??= new();
      set => bankruptcy = value;
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
    /// A value of MultiAp.
    /// </summary>
    [JsonPropertyName("multiAp")]
    public Common MultiAp
    {
      get => multiAp ??= new();
      set => multiAp = value;
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
    /// A value of ApSelected.
    /// </summary>
    [JsonPropertyName("apSelected")]
    public Common ApSelected
    {
      get => apSelected ??= new();
      set => apSelected = value;
    }

    /// <summary>
    /// A value of SelectedApCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("selectedApCsePersonsWorkSet")]
    public CsePersonsWorkSet SelectedApCsePersonsWorkSet
    {
      get => selectedApCsePersonsWorkSet ??= new();
      set => selectedApCsePersonsWorkSet = value;
    }

    private SpTextWorkArea moreAps;
    private Common caseClosedIndicator;
    private SpTextWorkArea apAddrProbMsg;
    private FplsLocateRequest selected;
    private Infrastructure hiddenPassed1;
    private Array<HiddenPassedGroup> hiddenPassed;
    private NarrativeWork locateReview;
    private Common hiddenPassedReviewType;
    private Array<ApGroup> ap;
    private CsePerson selectedApCsePerson;
    private Common totalChildren;
    private CsePersonsWorkSet ap2;
    private Incarceration incarceration;
    private Bankruptcy bankruptcy;
    private Case1 case1;
    private NextTranInfo hidden;
    private Standard standard;
    private WorkArea activityLiteral;
    private Common multiAp;
    private CsePersonsWorkSet ap1;
    private Common apSelected;
    private CsePersonsWorkSet selectedApCsePersonsWorkSet;
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

    /// <summary>A AliasGroup group.</summary>
    [Serializable]
    public class AliasGroup
    {
      /// <summary>
      /// A value of Galias.
      /// </summary>
      [JsonPropertyName("galias")]
      public CsePersonsWorkSet Galias
      {
        get => galias ??= new();
        set => galias = value;
      }

      /// <summary>
      /// A value of Gkscares.
      /// </summary>
      [JsonPropertyName("gkscares")]
      public Common Gkscares
      {
        get => gkscares ??= new();
        set => gkscares = value;
      }

      /// <summary>
      /// A value of Gkanpay.
      /// </summary>
      [JsonPropertyName("gkanpay")]
      public Common Gkanpay
      {
        get => gkanpay ??= new();
        set => gkanpay = value;
      }

      /// <summary>
      /// A value of Gcse.
      /// </summary>
      [JsonPropertyName("gcse")]
      public Common Gcse
      {
        get => gcse ??= new();
        set => gcse = value;
      }

      /// <summary>
      /// A value of Gae.
      /// </summary>
      [JsonPropertyName("gae")]
      public Common Gae
      {
        get => gae ??= new();
        set => gae = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private CsePersonsWorkSet galias;
      private Common gkscares;
      private Common gkanpay;
      private Common gcse;
      private Common gae;
    }

    /// <summary>A ApAddrStatusGroup group.</summary>
    [Serializable]
    public class ApAddrStatusGroup
    {
      /// <summary>
      /// A value of CurrApAddrStatus.
      /// </summary>
      [JsonPropertyName("currApAddrStatus")]
      public Common CurrApAddrStatus
      {
        get => currApAddrStatus ??= new();
        set => currApAddrStatus = value;
      }

      /// <summary>
      /// A value of PrevApAddrVf.
      /// </summary>
      [JsonPropertyName("prevApAddrVf")]
      public Common PrevApAddrVf
      {
        get => prevApAddrVf ??= new();
        set => prevApAddrVf = value;
      }

      /// <summary>
      /// A value of PrevApAddrNv.
      /// </summary>
      [JsonPropertyName("prevApAddrNv")]
      public Common PrevApAddrNv
      {
        get => prevApAddrNv ??= new();
        set => prevApAddrNv = value;
      }

      /// <summary>
      /// A value of PrevApAddrNf.
      /// </summary>
      [JsonPropertyName("prevApAddrNf")]
      public Common PrevApAddrNf
      {
        get => prevApAddrNf ??= new();
        set => prevApAddrNf = value;
      }

      private Common currApAddrStatus;
      private Common prevApAddrVf;
      private Common prevApAddrNv;
      private Common prevApAddrNf;
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
    /// A value of BlankDate.
    /// </summary>
    [JsonPropertyName("blankDate")]
    public DateWorkArea BlankDate
    {
      get => blankDate ??= new();
      set => blankDate = value;
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
    /// Gets a value of Alias.
    /// </summary>
    [JsonIgnore]
    public Array<AliasGroup> Alias => alias ??= new(AliasGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Alias for json serialization.
    /// </summary>
    [JsonPropertyName("alias")]
    [Computed]
    public IList<AliasGroup> Alias_Json
    {
      get => alias;
      set => Alias.Assign(value);
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
    /// A value of SsnNotFoundFlag.
    /// </summary>
    [JsonPropertyName("ssnNotFoundFlag")]
    public Common SsnNotFoundFlag
    {
      get => ssnNotFoundFlag ??= new();
      set => ssnNotFoundFlag = value;
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
    /// A value of Passed.
    /// </summary>
    [JsonPropertyName("passed")]
    public TextWorkArea Passed
    {
      get => passed ??= new();
      set => passed = value;
    }

    /// <summary>
    /// A value of Pass.
    /// </summary>
    [JsonPropertyName("pass")]
    public DateWorkArea Pass
    {
      get => pass ??= new();
      set => pass = value;
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
    /// A value of RollingMaxVerfDt.
    /// </summary>
    [JsonPropertyName("rollingMaxVerfDt")]
    public DateWorkArea RollingMaxVerfDt
    {
      get => rollingMaxVerfDt ??= new();
      set => rollingMaxVerfDt = value;
    }

    /// <summary>
    /// A value of FormatDateCcyy.
    /// </summary>
    [JsonPropertyName("formatDateCcyy")]
    public SpTextWorkArea FormatDateCcyy
    {
      get => formatDateCcyy ??= new();
      set => formatDateCcyy = value;
    }

    /// <summary>
    /// A value of IncarceratedFlag.
    /// </summary>
    [JsonPropertyName("incarceratedFlag")]
    public Common IncarceratedFlag
    {
      get => incarceratedFlag ??= new();
      set => incarceratedFlag = value;
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
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DateWorkArea Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    /// <summary>
    /// A value of FormatDateYy.
    /// </summary>
    [JsonPropertyName("formatDateYy")]
    public CsePersonsWorkSet FormatDateYy
    {
      get => formatDateYy ??= new();
      set => formatDateYy = value;
    }

    /// <summary>
    /// A value of FormatDateDd.
    /// </summary>
    [JsonPropertyName("formatDateDd")]
    public CsePersonsWorkSet FormatDateDd
    {
      get => formatDateDd ??= new();
      set => formatDateDd = value;
    }

    /// <summary>
    /// A value of FormatDateMm.
    /// </summary>
    [JsonPropertyName("formatDateMm")]
    public CsePersonsWorkSet FormatDateMm
    {
      get => formatDateMm ??= new();
      set => formatDateMm = value;
    }

    /// <summary>
    /// A value of IncomeCalculation.
    /// </summary>
    [JsonPropertyName("incomeCalculation")]
    public Common IncomeCalculation
    {
      get => incomeCalculation ??= new();
      set => incomeCalculation = value;
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
    /// A value of ReviewType.
    /// </summary>
    [JsonPropertyName("reviewType")]
    public Infrastructure ReviewType
    {
      get => reviewType ??= new();
      set => reviewType = value;
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
    /// Gets a value of ApAddrStatus.
    /// </summary>
    [JsonPropertyName("apAddrStatus")]
    public ApAddrStatusGroup ApAddrStatus
    {
      get => apAddrStatus ?? (apAddrStatus = new());
      set => apAddrStatus = value;
    }

    /// <summary>
    /// A value of SelActivityLiteral.
    /// </summary>
    [JsonPropertyName("selActivityLiteral")]
    public WorkArea SelActivityLiteral
    {
      get => selActivityLiteral ??= new();
      set => selActivityLiteral = value;
    }

    /// <summary>
    /// A value of SelLastVerifiedEmpDate.
    /// </summary>
    [JsonPropertyName("selLastVerifiedEmpDate")]
    public IncomeSource SelLastVerifiedEmpDate
    {
      get => selLastVerifiedEmpDate ??= new();
      set => selLastVerifiedEmpDate = value;
    }

    /// <summary>
    /// A value of SelAltAlias.
    /// </summary>
    [JsonPropertyName("selAltAlias")]
    public TextWorkArea SelAltAlias
    {
      get => selAltAlias ??= new();
      set => selAltAlias = value;
    }

    /// <summary>
    /// A value of SelAddressNotVerfMsg.
    /// </summary>
    [JsonPropertyName("selAddressNotVerfMsg")]
    public SpTextWorkArea SelAddressNotVerfMsg
    {
      get => selAddressNotVerfMsg ??= new();
      set => selAddressNotVerfMsg = value;
    }

    /// <summary>
    /// A value of SelEmpIncome.
    /// </summary>
    [JsonPropertyName("selEmpIncome")]
    public Common SelEmpIncome
    {
      get => selEmpIncome ??= new();
      set => selEmpIncome = value;
    }

    /// <summary>
    /// A value of SelCompliance.
    /// </summary>
    [JsonPropertyName("selCompliance")]
    public DateWorkArea SelCompliance
    {
      get => selCompliance ??= new();
      set => selCompliance = value;
    }

    /// <summary>
    /// A value of SelStart.
    /// </summary>
    [JsonPropertyName("selStart")]
    public DateWorkArea SelStart
    {
      get => selStart ??= new();
      set => selStart = value;
    }

    /// <summary>
    /// A value of SelCommon.
    /// </summary>
    [JsonPropertyName("selCommon")]
    public Common SelCommon
    {
      get => selCommon ??= new();
      set => selCommon = value;
    }

    /// <summary>
    /// A value of SelOtherIncome.
    /// </summary>
    [JsonPropertyName("selOtherIncome")]
    public Common SelOtherIncome
    {
      get => selOtherIncome ??= new();
      set => selOtherIncome = value;
    }

    /// <summary>
    /// A value of SelSesa.
    /// </summary>
    [JsonPropertyName("selSesa")]
    public FplsLocateRequest SelSesa
    {
      get => selSesa ??= new();
      set => selSesa = value;
    }

    /// <summary>
    /// A value of SelFplsFplsLocateRequest.
    /// </summary>
    [JsonPropertyName("selFplsFplsLocateRequest")]
    public FplsLocateRequest SelFplsFplsLocateRequest
    {
      get => selFplsFplsLocateRequest ??= new();
      set => selFplsFplsLocateRequest = value;
    }

    /// <summary>
    /// A value of Seldata1099LocateRequest.
    /// </summary>
    [JsonPropertyName("seldata1099LocateRequest")]
    public Data1099LocateRequest Seldata1099LocateRequest
    {
      get => seldata1099LocateRequest ??= new();
      set => seldata1099LocateRequest = value;
    }

    /// <summary>
    /// A value of SelParolOfficer.
    /// </summary>
    [JsonPropertyName("selParolOfficer")]
    public Common SelParolOfficer
    {
      get => selParolOfficer ??= new();
      set => selParolOfficer = value;
    }

    /// <summary>
    /// A value of SelFederalCompliance.
    /// </summary>
    [JsonPropertyName("selFederalCompliance")]
    public Common SelFederalCompliance
    {
      get => selFederalCompliance ??= new();
      set => selFederalCompliance = value;
    }

    /// <summary>
    /// A value of SelDaysRemaining.
    /// </summary>
    [JsonPropertyName("selDaysRemaining")]
    public Common SelDaysRemaining
    {
      get => selDaysRemaining ??= new();
      set => selDaysRemaining = value;
    }

    /// <summary>
    /// A value of SelSeas.
    /// </summary>
    [JsonPropertyName("selSeas")]
    public Common SelSeas
    {
      get => selSeas ??= new();
      set => selSeas = value;
    }

    /// <summary>
    /// A value of SelFplsCommon.
    /// </summary>
    [JsonPropertyName("selFplsCommon")]
    public Common SelFplsCommon
    {
      get => selFplsCommon ??= new();
      set => selFplsCommon = value;
    }

    /// <summary>
    /// A value of SelMedicalInsuAvailable.
    /// </summary>
    [JsonPropertyName("selMedicalInsuAvailable")]
    public Common SelMedicalInsuAvailable
    {
      get => selMedicalInsuAvailable ??= new();
      set => selMedicalInsuAvailable = value;
    }

    /// <summary>
    /// A value of SelPersonIncomeHistory.
    /// </summary>
    [JsonPropertyName("selPersonIncomeHistory")]
    public PersonIncomeHistory SelPersonIncomeHistory
    {
      get => selPersonIncomeHistory ??= new();
      set => selPersonIncomeHistory = value;
    }

    /// <summary>
    /// A value of SelApCsePerson.
    /// </summary>
    [JsonPropertyName("selApCsePerson")]
    public CsePerson SelApCsePerson
    {
      get => selApCsePerson ??= new();
      set => selApCsePerson = value;
    }

    /// <summary>
    /// A value of SelApCsePersonAddress.
    /// </summary>
    [JsonPropertyName("selApCsePersonAddress")]
    public CsePersonAddress SelApCsePersonAddress
    {
      get => selApCsePersonAddress ??= new();
      set => selApCsePersonAddress = value;
    }

    /// <summary>
    /// A value of SelTotalChildren.
    /// </summary>
    [JsonPropertyName("selTotalChildren")]
    public Common SelTotalChildren
    {
      get => selTotalChildren ??= new();
      set => selTotalChildren = value;
    }

    /// <summary>
    /// A value of SelApCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("selApCsePersonsWorkSet")]
    public CsePersonsWorkSet SelApCsePersonsWorkSet
    {
      get => selApCsePersonsWorkSet ??= new();
      set => selApCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of SelIncarceration.
    /// </summary>
    [JsonPropertyName("selIncarceration")]
    public Incarceration SelIncarceration
    {
      get => selIncarceration ??= new();
      set => selIncarceration = value;
    }

    /// <summary>
    /// A value of SelBankruptcy.
    /// </summary>
    [JsonPropertyName("selBankruptcy")]
    public Bankruptcy SelBankruptcy
    {
      get => selBankruptcy ??= new();
      set => selBankruptcy = value;
    }

    /// <summary>
    /// A value of SelHiliteAddrField.
    /// </summary>
    [JsonPropertyName("selHiliteAddrField")]
    public Common SelHiliteAddrField
    {
      get => selHiliteAddrField ??= new();
      set => selHiliteAddrField = value;
    }

    private Array<WorkGroup> work;
    private DateWorkArea blankDate;
    private DateWorkArea openOrClosed;
    private Array<AliasGroup> alias;
    private DateWorkArea caseClosedDate;
    private Common ssnNotFoundFlag;
    private DateWorkArea current;
    private TextWorkArea passed;
    private DateWorkArea pass;
    private DateWorkArea max;
    private DateWorkArea rollingMaxVerfDt;
    private SpTextWorkArea formatDateCcyy;
    private Common incarceratedFlag;
    private Common common;
    private DateWorkArea initialized;
    private CsePersonsWorkSet formatDateYy;
    private CsePersonsWorkSet formatDateDd;
    private CsePersonsWorkSet formatDateMm;
    private Common incomeCalculation;
    private CsePersonsWorkSet csePersonsWorkSet;
    private AbendData abendData;
    private Infrastructure reviewType;
    private Common noOfApOnCase;
    private ApAddrStatusGroup apAddrStatus;
    private WorkArea selActivityLiteral;
    private IncomeSource selLastVerifiedEmpDate;
    private TextWorkArea selAltAlias;
    private SpTextWorkArea selAddressNotVerfMsg;
    private Common selEmpIncome;
    private DateWorkArea selCompliance;
    private DateWorkArea selStart;
    private Common selCommon;
    private Common selOtherIncome;
    private FplsLocateRequest selSesa;
    private FplsLocateRequest selFplsFplsLocateRequest;
    private Data1099LocateRequest seldata1099LocateRequest;
    private Common selParolOfficer;
    private Common selFederalCompliance;
    private Common selDaysRemaining;
    private Common selSeas;
    private Common selFplsCommon;
    private Common selMedicalInsuAvailable;
    private PersonIncomeHistory selPersonIncomeHistory;
    private CsePerson selApCsePerson;
    private CsePersonAddress selApCsePersonAddress;
    private Common selTotalChildren;
    private CsePersonsWorkSet selApCsePersonsWorkSet;
    private Incarceration selIncarceration;
    private Bankruptcy selBankruptcy;
    private Common selHiliteAddrField;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of LocateReview.
    /// </summary>
    [JsonPropertyName("locateReview")]
    public NarrativeDetail LocateReview
    {
      get => locateReview ??= new();
      set => locateReview = value;
    }

    /// <summary>
    /// A value of ParoleOrProbation.
    /// </summary>
    [JsonPropertyName("paroleOrProbation")]
    public Incarceration ParoleOrProbation
    {
      get => paroleOrProbation ??= new();
      set => paroleOrProbation = value;
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
    /// A value of LastLocated.
    /// </summary>
    [JsonPropertyName("lastLocated")]
    public Infrastructure LastLocated
    {
      get => lastLocated ??= new();
      set => lastLocated = value;
    }

    /// <summary>
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
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
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    /// <summary>
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CaseRole Child
    {
      get => child ??= new();
      set => child = value;
    }

    /// <summary>
    /// A value of FplsLocateRequest.
    /// </summary>
    [JsonPropertyName("fplsLocateRequest")]
    public FplsLocateRequest FplsLocateRequest
    {
      get => fplsLocateRequest ??= new();
      set => fplsLocateRequest = value;
    }

    /// <summary>
    /// A value of Data1099LocateRequest.
    /// </summary>
    [JsonPropertyName("data1099LocateRequest")]
    public Data1099LocateRequest Data1099LocateRequest
    {
      get => data1099LocateRequest ??= new();
      set => data1099LocateRequest = value;
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
    /// A value of PersonIncomeHistory.
    /// </summary>
    [JsonPropertyName("personIncomeHistory")]
    public PersonIncomeHistory PersonIncomeHistory
    {
      get => personIncomeHistory ??= new();
      set => personIncomeHistory = value;
    }

    /// <summary>
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
    }

    /// <summary>
    /// A value of ApCsePersonAddress.
    /// </summary>
    [JsonPropertyName("apCsePersonAddress")]
    public CsePersonAddress ApCsePersonAddress
    {
      get => apCsePersonAddress ??= new();
      set => apCsePersonAddress = value;
    }

    /// <summary>
    /// A value of Incarceration.
    /// </summary>
    [JsonPropertyName("incarceration")]
    public Incarceration Incarceration
    {
      get => incarceration ??= new();
      set => incarceration = value;
    }

    /// <summary>
    /// A value of Bankruptcy.
    /// </summary>
    [JsonPropertyName("bankruptcy")]
    public Bankruptcy Bankruptcy
    {
      get => bankruptcy ??= new();
      set => bankruptcy = value;
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
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
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

    private NarrativeDetail locateReview;
    private Incarceration paroleOrProbation;
    private ObligationType obligationType;
    private LegalActionCaseRole legalActionCaseRole;
    private HealthInsuranceCoverage healthInsuranceCoverage;
    private PersonalHealthInsurance personalHealthInsurance;
    private MonitoredActivity monitoredActivity;
    private Infrastructure lastLocated;
    private IncomeSource incomeSource;
    private CaseRole absentParent;
    private LegalActionDetail legalActionDetail;
    private CaseRole child;
    private FplsLocateRequest fplsLocateRequest;
    private Data1099LocateRequest data1099LocateRequest;
    private Case1 case1;
    private PersonIncomeHistory personIncomeHistory;
    private CsePerson apCsePerson;
    private CsePersonAddress apCsePersonAddress;
    private Incarceration incarceration;
    private Bankruptcy bankruptcy;
    private CsePerson csePerson;
    private LegalActionPerson legalActionPerson;
    private CsePerson provider;
  }
#endregion
}
