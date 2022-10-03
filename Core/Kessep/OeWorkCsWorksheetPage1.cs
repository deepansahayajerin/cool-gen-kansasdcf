// Program: OE_WORK_CS_WORKSHEET_PAGE_1, ID: 371896272, model: 746.
// Short name: SWEWORKP
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
/// A program: OE_WORK_CS_WORKSHEET_PAGE_1.
/// </para>
/// <para>
/// Resp - OBLGESTB
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OeWorkCsWorksheetPage1: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_WORK_CS_WORKSHEET_PAGE_1 program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeWorkCsWorksheetPage1(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeWorkCsWorksheetPage1.
  /// </summary>
  public OeWorkCsWorksheetPage1(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------------------------
    //        M A I N T E N A N C E   L O G
    // Date		Developer	Request #    	Description
    // 03/03/95	Sid Chowdhary			Initial Development
    // 03/03/96	Sid Chowdhary			Retrofit and Testing
    // 11/25/96	R. Marchman			Add new Security and next tran
    // 02/20/97	Sid Chowdhary			Add OSP header.
    // 12/16/1998	M Ramirez			Revised print process.
    // 12/16/1998	M Ramirez			Changed security to check on CRUD actions only.
    // 07/09/1999	Srini Ganji			Added new Exit State for not found Child Support
    // 						Adjustment record, PR # 706 in Add, Update worksheet ABs
    // 11/05/99        Srini Ganji     PR#H00078367 & 78428
    // 						Parent A should be a required field to add a worksheet.
    // 						The Parent B field should be optional
    // 01/06/2000	M Ramirez	83300		NEXT TRAN needs to be cleared
    // 						before invoking print process
    // 11/17/00 	M.Lachowicz	WR 298. 	Create header information for screens.
    // 10/10/02	K.Doshi				Fix screen help Id.
    // 05/14/04	A. Convery	PR# 203514	Change error messages on two EXIT STATEs 
    // to reflect change from "A/B"
    // 						to "Mother/Father" designation.
    // 11/19/07	M. Fan		WR318566(CQ297)	Required age groups changed. Changed to 
    // use generic names for age
    // 						groups and added the parenting time adjustment percent to imports
    // 						and exports for child support worksheet entity view,
    // 08/15/08	J. Huss		PR209970(CQ455)	Changed print return so that totals are
    // recalculated when returning
    // 						from DDOC.
    // 06/04/10	J. Huss		CQ# 18769	Reduced group_import and group_export size 
    // from 7 to 6.
    // 03/19/12       A Hockman        SR# 16297      Changes to allow for 
    // stored years of guidelines and calculate via
    //                                                
    // chosen gl yr & display guideline yr  on screen.
    // 12/08/2015     GVandy		CQ#50299	Change Mother/Father header labels to 
    // Person #/Person #.
    // 						Change column header from Mother/Father to
    // 						<first name>/<first name>.  Change current guideline
    // 						year from hardcoded 2012 to read for year from
    // 						CS VALID GUIDELINE YEAR code table.
    // 11/06/2019     GVandy		CQ#66067	2020 Guideline Year changes.
    // 		
    // -----------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    UseSpDocSetLiterals();
    export.Hidden.Assign(import.Hidden);

    // ---------------------------------------------
    // This is a 3 procedure step PRAD. The views
    // contain the details of all the 3 screens. The
    // PSADs are connected through two links and all
    // the data are passed through all the screens.
    // All the data not belonging to a particular
    // screen are kept in the hidden views.
    // ---------------------------------------------
    local.Current.Date = Now().Date;

    // -- Read for the current Guideline Year.
    if (ReadCodeValue())
    {
      local.CurrentGuidelineYear.Year =
        (int)StringToNumber(entities.CodeValue.Cdvalue);
    }

    if (Equal(global.Command, "CLEAR") || Equal(global.Command, "RDISPLAY"))
    {
      // ---------------------------------------------
      // The Screen data will be discarded and the
      // Screen Cleared.
      // ---------------------------------------------
      export.Case1.Number = import.Case1.Number;
      export.ParentACsePerson.Number = import.ParentACsePerson.Number;
      export.ParentBCsePerson.Number = import.ParentBCsePerson.Number;
      export.ParentAName.Assign(import.ParentAName);
      export.ParentBName.Assign(import.ParentBName);
      MoveLegalAction(import.LegalAction, export.LegalAction);
      export.Tribunal.JudicialDistrict = import.Tribunal.JudicialDistrict;
      export.County.Description = import.County.Description;
      export.Next.Number = import.Next.Number;
      export.ServiceProvider.LastName = import.ServiceProvider.LastName;
      MoveOffice(import.Office, export.Office);

      // 11/17/00 M.L Start
      export.HeaderLine.Text35 = import.HeaderLine.Text35;

      // 11/17/00 M.L End
      if (Equal(global.Command, "RDISPLAY"))
      {
        var field = GetField(export.Case1, "number");

        field.Error = true;

        ExitState = "CASE_NF";
      }

      export.ChildSupportWorksheet.CsGuidelineYear =
        local.CurrentGuidelineYear.Year;

      return;
    }
    else
    {
      // ---------------------------------------------
      //        Move all IMPORTs to EXPORTs.
      // ---------------------------------------------
      export.Case1.Number = import.Case1.Number;
      export.PrevCase.Number = import.PrevCase.Number;
      export.ChildSupportWorksheet.Assign(import.ChildSupportWorksheet);
      export.PrevChildSupportWorksheet.Assign(import.PrevChildSupportWorksheet);
      export.Prev2.Assign(import.Prev2);
      MoveLegalAction(import.LegalAction, export.LegalAction);
      export.Tribunal.JudicialDistrict = import.Tribunal.JudicialDistrict;
      MoveLegalAction(import.SelectedLegalAction, export.Selected);
      export.Next.Number = import.Next.Number;
      export.ServiceProvider.LastName = import.ServiceProvider.LastName;
      MoveOffice(import.Office, export.Office);

      // 11/17/00 M.L Start
      export.HeaderLine.Text35 = import.HeaderLine.Text35;

      // 11/17/00 M.L End
      export.County.Description = import.County.Description;
      export.ParentACsePerson.Number = import.ParentACsePerson.Number;
      export.ParentBCsePerson.Number = import.ParentBCsePerson.Number;
      export.ParentAName.Assign(import.ParentAName);
      export.ParentBName.Assign(import.ParentBName);
      export.PrevParentACsePerson.Number = import.PrevParentACsePerson.Number;
      export.PrevParentBCsePerson.Number = import.PrevParentBCsePerson.Number;
      export.ParentBCsePersonSupportWorksheet.Assign(
        import.ParentBCsePersonSupportWorksheet);
      export.ParentACsePersonSupportWorksheet.Assign(
        import.ParentACsePersonSupportWorksheet);
      MoveStandard(import.WorkPrev, export.WorkPrev);
      export.LastUpdtDate.EffectiveDate = import.LastUpdtDate.EffectiveDate;
      export.CommandH.Command = import.CommandH.Command;
      export.ParentBPrompt.SelectChar = import.ParentBPrompt.SelectChar;
      export.ParentAPrompt.SelectChar = import.ParentAPrompt.SelectChar;
      export.AuthorizingPrompt.SelectChar = import.AuthorizingPrompt.SelectChar;
      export.GuidelineYearPrompt.SelectChar =
        import.GuidelineYearPrompt.SelectChar;
      export.CsOblig06TotalAmt.TotalCurrency =
        import.CsObligAgrp1TotalAmt.TotalCurrency;
      export.CsOblig1618TotalAmt.TotalCurrency =
        import.CsObligAgrp3TotalInc.TotalCurrency;
      export.CsOblig715TotalAmt.TotalCurrency =
        import.CsObligAgrp2TotalAmt.TotalCurrency;
      export.CsObligTotalAmount.TotalCurrency =
        import.CsObligTotalAmount.TotalCurrency;
      export.D1otalCsInc.TotalCurrency = import.D1otalCsInc.TotalCurrency;
      export.D6otalChildSuppOblig.TotalCurrency =
        import.D6otalChildSuppOblig.TotalCurrency;
      export.ParentAB3SeGrossInc.TotalCurrency =
        import.ParentAB3SeGrossInc.TotalCurrency;
      export.ParentAC1TotGrossInc.TotalCurrency =
        import.ParentAC1TotGrossInc.TotalCurrency;
      export.ParentAC5D1TotCsInc.TotalCurrency =
        import.ParentAC5D1TotCsInc.TotalCurrency;
      export.ParentAChildCareCost.TotalCurrency =
        import.ParentAChildCareCost.TotalCurrency;
      export.ParentAD2PercentInc.TotalCurrency =
        import.ParentAD2PercentInc.TotalCurrency;
      export.ParentAD7CsOblig.TotalCurrency =
        import.ParentAD7CsOblig.TotalCurrency;
      export.ParentAD8Adjustments.TotalCurrency =
        import.ParentAD8Adjustments.TotalCurrency;
      export.ParentAD9F1NetCs.TotalCurrency =
        import.ParentAD9F1NetCs.TotalCurrency;
      export.ParentAE7F2TotAdj.TotalCurrency =
        import.ParentAE7F2TotAdjust.TotalCurrency;
      export.ParentAF2TotalCsAdj.TotalCurrency =
        import.ParentAF2TotalCsAdj.TotalCurrency;
      export.ParentAF3AdjCsOblig.TotalCurrency =
        import.ParentAF3AdjCsOblig.TotalCurrency;
      export.ParentATotalTaxCredit.TotalCurrency =
        import.ParentATotalTaxCredit.TotalCurrency;
      export.PrevParentACsePersonSupportWorksheet.Assign(
        import.PrevParentACsePersonSupportWorksheet);
      export.ParentBB3SeGrossInc.TotalCurrency =
        import.ParentBB3SeGrossInc.TotalCurrency;
      export.ParentBC1TotGrossInc.TotalCurrency =
        import.ParentBC1TotGrossInc.TotalCurrency;
      export.ParentBC5D1TotCsInc.TotalCurrency =
        import.ParentBC5D1TotCsInc.TotalCurrency;
      export.ParentBChildCareCost.TotalCurrency =
        import.ParentBChildCareCost.TotalCurrency;
      export.ParentBD2PercentInc.TotalCurrency =
        import.ParentBD2PercentInc.TotalCurrency;
      export.ParentBD7CsOblig.TotalCurrency =
        import.ParentBD7CsOblig.TotalCurrency;
      export.ParentBD8Adjustments.TotalCurrency =
        import.ParentBD8Adjustments.TotalCurrency;
      export.ParentBD9F1NetCs.TotalCurrency =
        import.ParentBD9F1NetCs.TotalCurrency;
      export.ParentBE7F2TotAdj.TotalCurrency =
        import.ParentBE7F2TotAdjust.TotalCurrency;
      export.ParentBF2TotalCsAdj.TotalCurrency =
        import.ParentBF2TotalCsAdj.TotalCurrency;
      export.ParentBF3AdjCsOblig.TotalCurrency =
        import.ParentBF3AdjCsOblig.TotalCurrency;
      export.ParentBTotalTaxCredit.TotalCurrency =
        import.ParentBTotalTaxCredit.TotalCurrency;
      export.TotalChildCareCost.TotalCurrency =
        import.TotalChildCareCost.TotalCurrency;
      export.TotalInsurancePrem.TotalCurrency =
        import.TotalInsurancePrem.TotalCurrency;
      export.PrevParentBCsePersonSupportWorksheet.Assign(
        import.PrevParentBCsePersonSupportWorksheet);
      export.CaseOpen.Flag = import.CaseOpen.Flag;
      export.CaseRoleInactive.Flag = import.CaseRoleInactive.Flag;
      export.Common.Assign(import.Common);
      export.ParentAD10F1NetCs.TotalCurrency =
        import.ParentAD10F1NetCs.TotalCurrency;
      export.ParentBD10F1NetCs.TotalCurrency =
        import.ParentBD10F1NetCs.TotalCurrency;
      export.ParentAEnfFee.TotalCurrency = import.ParentAEnfFee.TotalCurrency;
      export.ParentBEnfFee.TotalCurrency = import.ParentBEnfFee.TotalCurrency;
      export.FromCren.Flag = import.FromCren.Flag;

      // @@@
      export.Gexport2020EnterableFields.GexportAbilityToPayParent.SelectChar =
        import.Gimport2020EnterableFields.GimportAbilityToPayParent.SelectChar;
      export.Gexport2020EnterableFields.GexportParentingTime.Assign(
        import.Gimport2020EnterableFields.GimportParentingTime);
      export.Gexport2020.GexportDParentTimeAdjFlag.SelectChar =
        import.Gimport2020.GimportDParentTimeAdjFlag.SelectChar;
      export.Gexport2020.GexportD4ParAPropShare.TotalCurrency =
        import.Gimport2020.GimportD4ParAPropShare.TotalCurrency;
      export.Gexport2020.GexportD4ParBPropShare.TotalCurrency =
        import.Gimport2020.GimportD4ParBPropShare.TotalCurrency;
      export.Gexport2020.GexportD4TotalPropShare.TotalCurrency =
        import.Gimport2020.GimportD4TotalPropShare.TotalCurrency;
      export.Gexport2020.GexportD5ParAParentTimAdj.TotalCurrency =
        import.Gimport2020.GimportD5ParAParentTimAdj.TotalCurrency;
      export.Gexport2020.GexportD5ParBParentTimAdj.TotalCurrency =
        import.Gimport2020.GimportD5ParBParentTimAdj.TotalCurrency;
      export.Gexport2020.GexportD5TotalParentTimAdj.TotalCurrency =
        import.Gimport2020.GimportD5TotalParentTimAdj.TotalCurrency;
      export.Gexport2020.GexportD6ParAPsAfterPat.TotalCurrency =
        import.Gimport2020.GimportD6ParAPsAfterPat.TotalCurrency;
      export.Gexport2020.GexportD6ParBPsAfterPat.TotalCurrency =
        import.Gimport2020.GimportD6ParBPsAfterPat.TotalCurrency;
      export.Gexport2020.GexportD6TotalPsAfterPat.TotalCurrency =
        import.Gimport2020.GimportD6TotalPsAfterPat.TotalCurrency;
      export.Gexport2020.GexportD8ParAPropShrHip.TotalCurrency =
        import.Gimport2020.GimportD8ParAPropShrHip.TotalCurrency;
      export.Gexport2020.GexportD8ParBPropShrHip.TotalCurrency =
        import.Gimport2020.GimportD8ParBPropShrHip.TotalCurrency;
      export.Gexport2020.GexportD8TotalPropShrHip.TotalCurrency =
        import.Gimport2020.GimportD8TotalPropShrHip.TotalCurrency;
      export.Gexport2020.GexportD10ParAPropShrWrcc.TotalCurrency =
        import.Gimport2020.GimportD10ParAPropShrWrcc.TotalCurrency;
      export.Gexport2020.GexportD10ParBPropShrWrcc.TotalCurrency =
        import.Gimport2020.GimportD10ParBPropShrWrcc.TotalCurrency;
      export.Gexport2020.GexportD10TotalPropShrWrcc.TotalCurrency =
        import.Gimport2020.GimportD10TotalPropShrWrcc.TotalCurrency;
      export.Gexport2020.GexportD11ParAPropShrCcob.TotalCurrency =
        import.Gimport2020.GimportD11ParAPropShrCcob.TotalCurrency;
      export.Gexport2020.GexportD11ParBPropShrCcob.TotalCurrency =
        import.Gimport2020.GimportD11ParBPropShrCcob.TotalCurrency;
      export.Gexport2020.GexportD11TotalPropShrCcob.TotalCurrency =
        import.Gimport2020.GimportD11TotalPropShrCcob.TotalCurrency;
      export.Gexport2020.GexportD12TotalInsWrccPaid.TotalCurrency =
        import.Gimport2020.GimportD12TotalInsWrccPaid.TotalCurrency;
      export.Gexport2020.GexportD13ParABasicChSup.TotalCurrency =
        import.Gimport2020.GimportD13ParABasicChSup.TotalCurrency;
      export.Gexport2020.GexportD13ParBBasicChSup.TotalCurrency =
        import.Gimport2020.GimportD13ParBBasicChSup.TotalCurrency;
      export.Gexport2020.GexportF3ParAAdjSubtotal.TotalCurrency =
        import.Gimport2020.GimportF3ParAAdjSubtotal.TotalCurrency;
      export.Gexport2020.GexportF3ParBAdjSubtotal.TotalCurrency =
        import.Gimport2020.GimportF3ParBAdjSubtotal.TotalCurrency;
      export.Gexport2020.GexportF5A0Parent.SelectChar =
        import.Gimport2020.GimportF5A0Parent.SelectChar;
      export.Gexport2020.GexportF5A1CsIncome.TotalCurrency =
        import.Gimport2020.GimportF5A1CsIncome.TotalCurrency;
      export.Gexport2020.GexportF5A2PovertyLevel.TotalCurrency =
        import.Gimport2020.GimportF5A2PovertyLevel.TotalCurrency;
      export.Gexport2020.GexportF5A3AbilityToPay.TotalCurrency =
        import.Gimport2020.GimportF5A3AbilityToPay.TotalCurrency;
      export.Gexport2020.GexportF5BParASubtotal.TotalCurrency =
        import.Gimport2020.GimportF5BParASubtotal.TotalCurrency;
      export.Gexport2020.GexportF5BParBSubtotal.TotalCurrency =
        import.Gimport2020.GimportF5BParBSubtotal.TotalCurrency;
      export.Gexport2020.GexportF6BParAFinaSubtotal.TotalCurrency =
        import.Gimport2020.GimportF6BParAFinaSubtotal.TotalCurrency;
      export.Gexport2020.GexportF6BParBFinaSubtotal.TotalCurrency =
        import.Gimport2020.GimportF6BParBFinaSubtotal.TotalCurrency;
      export.Gexport2020.GexportF8ParANetCsOblig.TotalCurrency =
        import.Gimport2020.GimportF8ParANetCsOblig.TotalCurrency;
      export.Gexport2020.GexportF8ParBNetCsOblig.TotalCurrency =
        import.Gimport2020.GimportF8ParBNetCsOblig.TotalCurrency;

      // @@@z
      export.GuidelineYearChange.Flag = import.GuidelineYearChange.Flag;

      // ---------------------------------------------
      //   Move all Group IMPORTs to Group EXPORTs.
      // ---------------------------------------------
      if (!import.Import1.IsEmpty)
      {
        export.Export1.Index = 0;
        export.Export1.Clear();

        for(import.Import1.Index = 0; import.Import1.Index < import
          .Import1.Count; ++import.Import1.Index)
        {
          if (export.Export1.IsFull)
          {
            break;
          }

          export.Export1.Update.ParentA.AdjustmentAmount =
            import.Import1.Item.ParentA.AdjustmentAmount;
          export.Export1.Update.ParentB.AdjustmentAmount =
            import.Import1.Item.ParentB.AdjustmentAmount;
          export.Export1.Next();
        }
      }
    }

    if (Equal(global.Command, "FROMCSWL") || Equal(global.Command, "RETCSWL"))
    {
      if (Equal(global.Command, "FROMCSWL"))
      {
        global.Command = "DISPLAY";
      }

      if (import.ChildSupportWorksheet.CsGuidelineYear < 2020 && import
        .ChildSupportWorksheet.CsGuidelineYear > 0)
      {
        // -- Flow to the WXRK screen for worksheets prior to 2020.  The work 
        // sheet layout and calculations were changed starting in 2020.
        ExitState = "ECO_LINK_TO_WXRK";

        return;
      }
    }

    // @@@z
    if (AsChar(export.GuidelineYearChange.Flag) == 'Y')
    {
      if (Equal(global.Command, "DISPLAY") || Equal
        (global.Command, "UPDATE") || Equal(global.Command, "DELETE"))
      {
        ExitState = "OE0000_ADD_FOR_NEW_GUIDELINE_YR";

        return;
      }
    }

    // ****************************************************************
    // Force user to calculate before add or update.  They must perform a 
    // calculate and Immediately perform an add or update.  Otherwise, do not
    // allow adding or updating record.
    // ****************************************************************
    if (Equal(import.Case1.Number, import.PrevCase.Number) && import
      .ChildSupportWorksheet.AdditionalNoOfChildren.GetValueOrDefault() == import
      .PrevChildSupportWorksheet.AdditionalNoOfChildren.GetValueOrDefault() && AsChar
      (import.ChildSupportWorksheet.CostOfLivingDiffAdjInd) == AsChar
      (import.PrevChildSupportWorksheet.CostOfLivingDiffAdjInd) && AsChar
      (import.ChildSupportWorksheet.MultipleFamilyAdjInd) == AsChar
      (import.PrevChildSupportWorksheet.MultipleFamilyAdjInd) && import
      .ChildSupportWorksheet.NoOfChildrenInAgeGrp1.GetValueOrDefault() == import
      .PrevChildSupportWorksheet.NoOfChildrenInAgeGrp1.GetValueOrDefault() && import
      .ChildSupportWorksheet.NoOfChildrenInAgeGrp3.GetValueOrDefault() == import
      .PrevChildSupportWorksheet.NoOfChildrenInAgeGrp3.GetValueOrDefault() && import
      .ChildSupportWorksheet.NoOfChildrenInAgeGrp2.GetValueOrDefault() == import
      .PrevChildSupportWorksheet.NoOfChildrenInAgeGrp2.GetValueOrDefault() && AsChar
      (import.ChildSupportWorksheet.Status) == AsChar
      (import.PrevChildSupportWorksheet.Status) && import
      .ChildSupportWorksheet.CsGuidelineYear == import
      .PrevChildSupportWorksheet.CsGuidelineYear && Equal
      (import.ParentACsePerson.Number, import.PrevParentACsePerson.Number) && Equal
      (import.ParentBCsePerson.Number, import.PrevParentBCsePerson.Number) && Equal
      (import.PrevParentACsePersonSupportWorksheet.ChildSupprtPaidCourtOrderNo,
      import.ParentACsePersonSupportWorksheet.ChildSupprtPaidCourtOrderNo) && import
      .PrevParentACsePersonSupportWorksheet.CourtOrderedChildSupportPaid.
        GetValueOrDefault() == import
      .ParentACsePersonSupportWorksheet.CourtOrderedChildSupportPaid.
        GetValueOrDefault() && import
      .PrevParentACsePersonSupportWorksheet.CourtOrderedMaintenancePaid.
        GetValueOrDefault() == import
      .ParentACsePersonSupportWorksheet.CourtOrderedMaintenancePaid.
        GetValueOrDefault() && import
      .PrevParentACsePersonSupportWorksheet.CourtOrderedMaintenanceRecvd.
        GetValueOrDefault() == import
      .ParentACsePersonSupportWorksheet.CourtOrderedMaintenanceRecvd.
        GetValueOrDefault() && Equal
      (import.PrevParentACsePersonSupportWorksheet.MaintenancePaidCourtOrderNo,
      import.ParentACsePersonSupportWorksheet.MaintenancePaidCourtOrderNo) && Equal
      (import.PrevParentACsePersonSupportWorksheet.MaintenanceRecvdCourtOrderNo,
      import.ParentACsePersonSupportWorksheet.MaintenanceRecvdCourtOrderNo) && import
      .PrevParentACsePersonSupportWorksheet.ReasonableBusinessExpense.
        GetValueOrDefault() == import
      .ParentACsePersonSupportWorksheet.ReasonableBusinessExpense.
        GetValueOrDefault() && import
      .PrevParentACsePersonSupportWorksheet.SelfEmploymentGrossIncome.
        GetValueOrDefault() == import
      .ParentACsePersonSupportWorksheet.SelfEmploymentGrossIncome.
        GetValueOrDefault() && import
      .PrevParentACsePersonSupportWorksheet.WageEarnerGrossIncome.
        GetValueOrDefault() == import
      .ParentACsePersonSupportWorksheet.WageEarnerGrossIncome.
        GetValueOrDefault() && Equal
      (import.PrevParentBCsePersonSupportWorksheet.ChildSupprtPaidCourtOrderNo,
      import.ParentBCsePersonSupportWorksheet.ChildSupprtPaidCourtOrderNo) && import
      .PrevParentBCsePersonSupportWorksheet.CourtOrderedChildSupportPaid.
        GetValueOrDefault() == import
      .ParentBCsePersonSupportWorksheet.CourtOrderedChildSupportPaid.
        GetValueOrDefault() && import
      .PrevParentBCsePersonSupportWorksheet.CourtOrderedMaintenancePaid.
        GetValueOrDefault() == import
      .ParentBCsePersonSupportWorksheet.CourtOrderedMaintenancePaid.
        GetValueOrDefault() && import
      .PrevParentBCsePersonSupportWorksheet.CourtOrderedMaintenanceRecvd.
        GetValueOrDefault() == import
      .ParentBCsePersonSupportWorksheet.CourtOrderedMaintenanceRecvd.
        GetValueOrDefault() && Equal
      (import.PrevParentBCsePersonSupportWorksheet.MaintenancePaidCourtOrderNo,
      import.ParentBCsePersonSupportWorksheet.MaintenancePaidCourtOrderNo) && Equal
      (import.PrevParentBCsePersonSupportWorksheet.MaintenanceRecvdCourtOrderNo,
      import.ParentBCsePersonSupportWorksheet.MaintenanceRecvdCourtOrderNo) && import
      .PrevParentBCsePersonSupportWorksheet.ReasonableBusinessExpense.
        GetValueOrDefault() == import
      .ParentBCsePersonSupportWorksheet.ReasonableBusinessExpense.
        GetValueOrDefault() && import
      .PrevParentBCsePersonSupportWorksheet.SelfEmploymentGrossIncome.
        GetValueOrDefault() == import
      .ParentBCsePersonSupportWorksheet.SelfEmploymentGrossIncome.
        GetValueOrDefault() && import
      .PrevParentBCsePersonSupportWorksheet.WageEarnerGrossIncome.
        GetValueOrDefault() == import
      .ParentBCsePersonSupportWorksheet.WageEarnerGrossIncome.
        GetValueOrDefault())
    {
    }
    else
    {
      export.CommandH.Command = "";
    }

    if (Equal(export.CommandH.Command, "CALCULAT"))
    {
      // mjr
      // ---------------------------------------------
      // 12/23/1998
      // Added command of Print to this IF statement
      // ----------------------------------------------------------
      if (Equal(global.Command, "CREATE") || Equal
        (global.Command, "UPDATE") || Equal(global.Command, "PRINT"))
      {
      }
      else
      {
        export.CommandH.Command = "";
      }
    }

    if (IsEmpty(import.ParentACsePerson.Number))
    {
      export.ParentAName.FormattedName = "";
    }

    if (IsEmpty(import.ParentBCsePerson.Number))
    {
      export.ParentBName.FormattedName = "";
    }

    if (!IsEmpty(export.Case1.Number))
    {
      local.TextWorkArea.Text10 = export.Case1.Number;
      UseEabPadLeftWithZeros();
      export.Case1.Number = local.TextWorkArea.Text10;
    }

    if (!IsEmpty(export.ParentACsePerson.Number))
    {
      local.TextWorkArea.Text10 = export.ParentACsePerson.Number;
      UseEabPadLeftWithZeros();
      export.ParentACsePerson.Number = local.TextWorkArea.Text10;
    }

    if (!IsEmpty(export.ParentBCsePerson.Number))
    {
      local.TextWorkArea.Text10 = export.ParentBCsePerson.Number;
      UseEabPadLeftWithZeros();
      export.ParentBCsePerson.Number = local.TextWorkArea.Text10;
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      // ****
      // ****
      // this is where you would set the local next_tran_info attributes to the 
      // import view attributes for the data to be passed to the next
      // transaction
      // ****
      // ****
      export.Hidden.CaseNumber = export.Case1.Number;
      export.Hidden.CsePersonNumber = export.ParentACsePerson.Number;
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
      export.Case1.Number = export.Hidden.CaseNumber ?? Spaces(10);
      export.ParentACsePerson.Number = export.Hidden.CsePersonNumber ?? Spaces
        (10);
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    // ---------------------------------------------
    // When comming from one of the other pages,
    // move imports to exports and display the
    // screen.
    // ---------------------------------------------
    if (Equal(global.Command, "CREATE") || Equal(global.Command, "UPDATE"))
    {
      if (AsChar(export.CaseOpen.Flag) == 'N')
      {
        ExitState = "CANNOT_MODIFY_CLOSED_CASE";

        return;
      }

      if (AsChar(export.CaseRoleInactive.Flag) == 'Y')
      {
        ExitState = "CANNOT_MODIFY_INACTIVE_CASE_ROLE";

        return;
      }
    }

    if (Equal(global.Command, "DISPPAGE"))
    {
      ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";

      if (AsChar(export.CaseOpen.Flag) == 'N')
      {
        ExitState = "DISPLAY_OK_FOR_CLOSED_CASE";
      }

      return;
    }
    else if (Equal(global.Command, "DISPPAG2"))
    {
      // --This occurs if the user prompted on the guideline year from the WORK 
      // screen and
      //   selected a prior year.  WORK then flows to WXRK sending command 
      // DISPPAG2.  We set a
      //   message indicating prior guideline year rules will apply.
      // @@@z
      export.GuidelineYearChange.Flag = "Y";

      if (export.ChildSupportWorksheet.CsGuidelineYear < local
        .CurrentGuidelineYear.Year)
      {
        ExitState = "SI000_PRIOR_YEAR_GUIDELINE_RULES";
      }

      var field = GetField(export.ChildSupportWorksheet, "csGuidelineYear");

      field.Error = true;

      return;
    }

    if (Equal(global.Command, "RETURN"))
    {
      ExitState = "ACO_NE0000_RETURN";

      return;
    }

    if (Equal(global.Command, "RETCSWL"))
    {
      // ---------------------------------------------
      // This command is set when control has been
      // passed back to this procedure from a list
      // without any selection. The previous data
      // should be displayed back.
      // ---------------------------------------------
      if (IsEmpty(export.Case1.Number))
      {
        export.Case1.Number = export.PrevCase.Number;
        export.ParentACsePerson.Number = export.PrevParentACsePerson.Number;
        export.ParentBCsePerson.Number = export.PrevParentBCsePerson.Number;
        export.ChildSupportWorksheet.Assign(export.PrevChildSupportWorksheet);
        MoveChildSupportWorksheet2(export.Prev2, export.ChildSupportWorksheet);

        return;
      }
      else
      {
        global.Command = "DISPLAY";
      }
    }

    if (Equal(global.Command, "RETCOMP"))
    {
      if (AsChar(export.ParentAPrompt.SelectChar) == 'S')
      {
        if (!IsEmpty(import.SelectedFromComp.Number))
        {
          export.ParentACsePerson.Number = import.SelectedFromComp.Number;
          export.ParentAName.FormattedName =
            import.SelectedFromComp.FormattedName;
          UseSiReadCsePerson2();

          // -- Center the first name.
          export.ParentAName.FirstName = UseOeWorkCenterFirstName2();
        }

        export.ParentAPrompt.SelectChar = "";

        var field = GetField(export.ParentBCsePerson, "number");

        field.Protected = false;
        field.Focused = true;
      }
      else if (AsChar(export.ParentBPrompt.SelectChar) == 'S')
      {
        if (!IsEmpty(import.SelectedFromComp.Number))
        {
          export.ParentBCsePerson.Number = import.SelectedFromComp.Number;
          export.ParentBName.FormattedName =
            import.SelectedFromComp.FormattedName;
          UseSiReadCsePerson1();

          // -- Center the first name.
          export.ParentBName.FirstName = UseOeWorkCenterFirstName1();
        }

        export.ParentBPrompt.SelectChar = "";

        var field =
          GetField(export.ParentACsePersonSupportWorksheet,
          "wageEarnerGrossIncome");

        field.Protected = false;
        field.Focused = true;
      }

      if (!IsEmpty(export.ParentAPrompt.SelectChar))
      {
        var field = GetField(export.ParentAPrompt, "selectChar");

        field.Error = true;
      }

      if (!IsEmpty(export.ParentBPrompt.SelectChar))
      {
        var field = GetField(export.ParentBPrompt, "selectChar");

        field.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "RETLACS"))
    {
      if (import.SelectedLegalAction.Identifier != 0)
      {
        UseOeWorkReadCourtOrderDetails1();

        if (IsExitState("OE0000_COURT_ORDER_NF") || IsExitState
          ("OE0000_LEGAL_ACTION_NOT_4_PERSON"))
        {
          var field = GetField(export.LegalAction, "courtCaseNumber");

          field.Color = "red";
          field.Highlighting = Highlighting.ReverseVideo;
          field.Protected = true;
        }
      }

      return;
    }

    if (Equal(global.Command, "CSWL"))
    {
      // ---------------------------------------------
      // This allows the user to flow to the List
      // screen to select a Child Support Worksheet
      // record.
      // ---------------------------------------------
      ExitState = "ECO_LNK_TO_CSWL";
      export.PrevCase.Number = export.Case1.Number;
      export.PrevParentACsePerson.Number = export.ParentACsePerson.Number;
      export.PrevParentBCsePerson.Number = export.ParentBCsePerson.Number;
      export.PrevChildSupportWorksheet.Assign(export.ChildSupportWorksheet);
      MoveChildSupportWorksheet2(export.ChildSupportWorksheet, export.Prev2);
      export.WorkLink.Flag = "Y";

      return;
    }

    // mjr---> Changed security to check on CRUD actions only.
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "CREATE") || Equal
      (global.Command, "UPDATE") || Equal(global.Command, "DELETE") || Equal
      (global.Command, "CALCULAT") || Equal(global.Command, "PRINT"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // ---------------------------------------------
    //     P F K E Y    P R O C E S S I N G
    // ---------------------------------------------
    // ----------------------------------------------------------------------------
    // 11/05/99 Srini Ganji PR#H00078367
    // Following two validations added (IF statement block)
    // ----------------------------------------------------------------------------
    if (Equal(global.Command, "CALCULAT") || Equal
      (global.Command, "CREATE") || Equal(global.Command, "NEXT"))
    {
      if (IsEmpty(import.ParentACsePerson.Number))
      {
        var field = GetField(export.ParentACsePerson, "number");

        field.Error = true;

        ExitState = "OE0000_ENTER_PARENT_A";

        return;
      }

      if (Equal(import.ParentACsePerson.Number, import.ParentBCsePerson.Number))
      {
        var field = GetField(export.ParentBCsePerson, "number");

        field.Error = true;

        ExitState = "OE0000_ENTER_PARENT_A_B_SAME";

        return;
      }
    }

    // ----------------------------------------------------------------------------
    // End of code added by Srini Ganji on 11/05/99
    // - PR#H00078367
    // ----------------------------------------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "RETCDVL":
        // ----------------------------------------------------------------
        // Beginning Of Code
        // PR - 100471
        // Display Authorizing Authority
        // ----------------------------------------------------------------
        if (AsChar(export.GuidelineYearPrompt.SelectChar) == 'S')
        {
          export.GuidelineYearPrompt.SelectChar = "";

          if (!IsEmpty(import.SelectedCodeValue.Cdvalue))
          {
            export.ChildSupportWorksheet.CsGuidelineYear =
              (int)StringToNumber(import.SelectedCodeValue.Cdvalue);

            if (export.ChildSupportWorksheet.CsGuidelineYear < 2020)
            {
              // -- Flow to the WXRK screen for worksheets prior to 2020.  The 
              // work sheet layout and calculations were changed starting in
              // 2020.
              global.Command = "DISPPAG2";
              ExitState = "ECO_LINK_TO_WXRK";

              // --Insert the parenting time adjustment back in the "adjustment"
              // section.
              local.Local1.Index = -1;

              for(import.Import1.Index = 0; import.Import1.Index < import
                .Import1.Count; ++import.Import1.Index)
              {
                ++local.Local1.Index;
                local.Local1.CheckSize();

                local.Local1.Update.ParentA.AdjustmentAmount =
                  import.Import1.Item.ParentA.AdjustmentAmount;
                local.Local1.Update.ParentB.AdjustmentAmount =
                  import.Import1.Item.ParentB.AdjustmentAmount;
                local.Local1.Update.Work.SelectChar =
                  import.Import1.Item.Work.SelectChar;

                if (local.Local1.Index == 0)
                {
                  ++local.Local1.Index;
                  local.Local1.CheckSize();

                  local.Local1.Update.ParentA.AdjustmentAmount =
                    export.Gexport2020.GexportD5ParAParentTimAdj.TotalCurrency;
                  local.Local1.Update.ParentB.AdjustmentAmount =
                    export.Gexport2020.GexportD5ParBParentTimAdj.TotalCurrency;

                  if (local.Local1.Item.ParentA.AdjustmentAmount.
                    GetValueOrDefault() + local
                    .Local1.Item.ParentB.AdjustmentAmount.GetValueOrDefault() ==
                      0)
                  {
                    local.Local1.Update.Work.SelectChar = "N";
                  }
                  else
                  {
                    local.Local1.Update.Work.SelectChar = "Y";
                  }
                }
                else
                {
                }

                if (local.Local1.Index + 1 == Local.LocalGroup.Capacity)
                {
                  break;
                }
              }

              local.Local1.Index = -1;

              export.Export1.Index = 0;
              export.Export1.Clear();

              do
              {
                if (export.Export1.IsFull)
                {
                  break;
                }

                ++local.Local1.Index;
                local.Local1.CheckSize();

                export.Export1.Update.ParentA.AdjustmentAmount =
                  local.Local1.Item.ParentA.AdjustmentAmount;
                export.Export1.Update.ParentB.AdjustmentAmount =
                  local.Local1.Item.ParentB.AdjustmentAmount;
                export.Export1.Update.Work.SelectChar =
                  local.Local1.Item.Work.SelectChar;
                export.Export1.Next();
              }
              while(local.Local1.Index + 1 != local.Local1.Count);

              return;
            }

            if (export.ChildSupportWorksheet.CsGuidelineYear != local
              .CurrentGuidelineYear.Year)
            {
              ExitState = "SI000_PRIOR_YEAR_GUIDELINE_RULES";

              var field1 =
                GetField(export.ChildSupportWorksheet, "csGuidelineYear");

              field1.Error = true;
            }
          }
        }
        else if (AsChar(export.AuthorizingPrompt.SelectChar) == 'S')
        {
          export.AuthorizingPrompt.SelectChar = "";

          if (!IsEmpty(import.SelectedCodeValue.Cdvalue))
          {
            export.ChildSupportWorksheet.AuthorizingAuthority =
              import.SelectedCodeValue.Cdvalue;
          }
        }

        // ----------------------------------------------------------------
        // End Of Code
        // ----------------------------------------------------------------
        break;
      case "LIST":
        ExitState = "ACO_NN0000_ALL_OK";

        if (IsEmpty(export.Case1.Number))
        {
          var field1 = GetField(export.Case1, "number");

          field1.Error = true;

          if (!IsEmpty(import.AuthorizingPrompt.SelectChar))
          {
            export.AuthorizingPrompt.SelectChar = "";
          }

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }

        // ----------------------------------------------------------------
        // Beginning Of Code
        // PR - 100471
        // Display Authorizing Authority
        // ----------------------------------------------------------------
        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (AsChar(export.AuthorizingPrompt.SelectChar) == 'S')
          {
            if (IsEmpty(export.ParentACsePerson.Number) || IsEmpty
              (export.ParentBCsePerson.Number))
            {
              if (IsEmpty(export.ParentACsePerson.Number))
              {
                var field1 = GetField(export.ParentACsePerson, "number");

                field1.Error = true;

                export.AuthorizingPrompt.SelectChar = "";
                ExitState = "OE0000_ENTER_PARENT_A";

                return;
              }

              if (IsEmpty(export.ParentBCsePerson.Number))
              {
                var field1 = GetField(export.ParentBCsePerson, "number");

                field1.Error = true;

                export.AuthorizingPrompt.SelectChar = "";
                ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

                return;
              }
            }
            else
            {
              export.Required.CodeName = "AUTHORIZING AUTHORITY";
              ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

              return;
            }
          }
          else if (AsChar(export.GuidelineYearPrompt.SelectChar) == 'S')
          {
            if (IsEmpty(export.ParentACsePerson.Number) || IsEmpty
              (export.ParentBCsePerson.Number))
            {
              if (IsEmpty(export.ParentACsePerson.Number))
              {
                var field1 = GetField(export.ParentACsePerson, "number");

                field1.Error = true;

                export.GuidelineYearPrompt.SelectChar = "";
                ExitState = "OE0000_ENTER_PARENT_A";

                return;
              }

              if (IsEmpty(export.ParentBCsePerson.Number))
              {
                var field1 = GetField(export.ParentBCsePerson, "number");

                field1.Error = true;

                export.GuidelineYearPrompt.SelectChar = "";
                ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

                return;
              }
            }
            else
            {
              export.Required.CodeName = "CS VALID GUIDELINE YEARS";
              ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

              return;
            }
          }
        }

        // ----------------------------------------------------------------
        // End Of Code
        // ----------------------------------------------------------------
        if (IsEmpty(export.ParentAPrompt.SelectChar) && IsEmpty
          (export.ParentBPrompt.SelectChar))
        {
          var field1 = GetField(export.ParentAPrompt, "selectChar");

          field1.Error = true;

          var field2 = GetField(export.ParentBPrompt, "selectChar");

          field2.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
          }

          if (!IsEmpty(import.AuthorizingPrompt.SelectChar))
          {
            export.AuthorizingPrompt.SelectChar = "";
          }

          if (!IsEmpty(import.GuidelineYearPrompt.SelectChar))
          {
            export.GuidelineYearPrompt.SelectChar = "";
          }
        }

        if (!IsEmpty(export.ParentAPrompt.SelectChar) && AsChar
          (export.ParentAPrompt.SelectChar) != 'S')
        {
          var field1 = GetField(export.ParentAPrompt, "selectChar");

          field1.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
          }

          if (!IsEmpty(import.AuthorizingPrompt.SelectChar))
          {
            export.AuthorizingPrompt.SelectChar = "";
          }

          if (!IsEmpty(import.GuidelineYearPrompt.SelectChar))
          {
            export.GuidelineYearPrompt.SelectChar = "";
          }
        }

        if (!IsEmpty(export.ParentBPrompt.SelectChar) && AsChar
          (export.ParentBPrompt.SelectChar) != 'S')
        {
          var field1 = GetField(export.ParentBPrompt, "selectChar");

          field1.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
          }

          if (!IsEmpty(import.AuthorizingPrompt.SelectChar))
          {
            export.AuthorizingPrompt.SelectChar = "";
          }

          if (!IsEmpty(import.GuidelineYearPrompt.SelectChar))
          {
            export.GuidelineYearPrompt.SelectChar = "";
          }
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (AsChar(export.ParentAPrompt.SelectChar) == 'S' || AsChar
            (export.ParentBPrompt.SelectChar) == 'S')
          {
            ExitState = "ECO_LNK_TO_SI_COMP_CASE_COMP";

            if (!IsEmpty(import.AuthorizingPrompt.SelectChar))
            {
              export.AuthorizingPrompt.SelectChar = "";
            }

            if (!IsEmpty(import.GuidelineYearPrompt.SelectChar))
            {
              export.GuidelineYearPrompt.SelectChar = "";
            }
          }
        }

        return;
      case "LACS":
        if (IsEmpty(export.Case1.Number))
        {
          var field1 = GetField(export.Case1, "number");

          field1.Error = true;

          ExitState = "OE0000_CASE_NO_REQUIRED_FOR_LACS";
        }
        else
        {
          ExitState = "OE0000_MUST_FLOW_TO_LACS";
        }

        return;
      case "PRINT":
        export.CommandH.Command = "";
        local.Document.Name = "WRKSHEET";

        // mjr
        // ------------------------------------------
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
        export.Hidden.CaseNumber = export.Case1.Number;
        export.Hidden.MiscText1 = TrimEnd(local.SpDocLiteral.IdWorksheet) + NumberToString
          (export.ChildSupportWorksheet.Identifier, 6, 10);
        UseScCabNextTranPut2();

        // mjr---> DKEY's trancode = SRPD
        //  Can change this to do a READ instead of hardcoding
        global.NextTran = "SRPD PRINT";

        return;
      case "PRINTRET":
        // mjr
        // -----------------------------------------------
        // 12/16/1998
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
        local.Position.Count =
          Find(
            String(export.Hidden.MiscText1, NextTranInfo.MiscText1_MaxLength),
          TrimEnd(local.SpDocLiteral.IdWorksheet));

        if (local.Position.Count <= 0)
        {
          break;
        }

        local.BatchConvertNumToText.Number15 =
          StringToNumber(Substring(
            export.Hidden.MiscText1, 50, local.Position.Count + 9, 10));
        export.ChildSupportWorksheet.Identifier =
          local.BatchConvertNumToText.Number15;
        global.Command = "DISPLAY";

        break;
      case "":
        break;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "INVALID":
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
      case "HELP":
        ExitState = "ACO_NI0000_HELP_NOT_AVAILABLE";

        break;
      case "DISPLAY":
        // mjr
        // -----------------------------------------------------------
        // Pulled command Display out of main case of command so
        // that it can be executed after command PrintRet
        // --------------------------------------------------------------
        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "CREATE":
        ExitState = "ACO_NN0000_ALL_OK";

        if (IsEmpty(export.Case1.Number))
        {
          var field1 = GetField(export.Case1, "number");

          field1.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          return;
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseOeCabCheckCaseMember7();

          if (!IsEmpty(local.Error.Flag))
          {
            var field1 = GetField(export.Case1, "number");

            field1.Error = true;

            ExitState = "CASE_NF";

            return;
          }
        }

        if (!IsEmpty(export.ParentACsePerson.Number))
        {
          UseOeCabCheckCaseMember6();

          // -- Center the first name
          export.ParentAName.FirstName = UseOeWorkCenterFirstName2();

          if (!IsEmpty(local.Error.Flag))
          {
            var field1 = GetField(export.ParentACsePerson, "number");

            field1.Error = true;

            ExitState = "OE0000_CASE_MEMBER_NE";
          }
        }

        if (!IsEmpty(export.ParentBCsePerson.Number))
        {
          UseOeCabCheckCaseMember5();

          // -- Center the first name
          export.ParentBName.FirstName = UseOeWorkCenterFirstName1();

          if (!IsEmpty(local.Error.Flag))
          {
            var field1 = GetField(export.ParentBCsePerson, "number");

            field1.Error = true;

            ExitState = "OE0000_CASE_MEMBER_NE";
          }
        }

        if (!IsEmpty(local.Error.Flag))
        {
          return;
        }

        if (!IsEmpty(export.LegalAction.CourtCaseNumber))
        {
          UseOeWorkReadCourtOrderDetails2();

          if (IsExitState("OE0000_COURT_ORDER_NF") || IsExitState
            ("OE0000_LEGAL_ACTION_NOT_4_PERSON"))
          {
            var field1 = GetField(export.LegalAction, "courtCaseNumber");

            field1.Color = "red";
            field1.Highlighting = Highlighting.Underscore;
            field1.Protected = true;

            export.LegalAction.Identifier = 0;

            return;
          }
        }

        if (IsEmpty(export.ParentACsePerson.Number) && (
          import.ParentACsePersonSupportWorksheet.WageEarnerGrossIncome.
            GetValueOrDefault() != 0 || import
          .ParentACsePersonSupportWorksheet.SelfEmploymentGrossIncome.
            GetValueOrDefault() != 0))
        {
          var field1 = GetField(export.ParentACsePerson, "number");

          field1.Error = true;

          var field2 =
            GetField(export.ParentACsePersonSupportWorksheet,
            "selfEmploymentGrossIncome");

          field2.Error = true;

          var field3 =
            GetField(export.ParentACsePersonSupportWorksheet,
            "wageEarnerGrossIncome");

          field3.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }

        if (IsEmpty(export.ParentBCsePerson.Number) && (
          import.ParentBCsePersonSupportWorksheet.WageEarnerGrossIncome.
            GetValueOrDefault() != 0 || export
          .ParentBCsePersonSupportWorksheet.SelfEmploymentGrossIncome.
            GetValueOrDefault() != 0))
        {
          var field1 = GetField(export.ParentBCsePerson, "number");

          field1.Error = true;

          var field2 =
            GetField(export.ParentBCsePersonSupportWorksheet,
            "selfEmploymentGrossIncome");

          field2.Error = true;

          var field3 =
            GetField(export.ParentBCsePersonSupportWorksheet,
            "wageEarnerGrossIncome");

          field3.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (IsEmpty(import.ChildSupportWorksheet.Status))
        {
          var field1 = GetField(export.ChildSupportWorksheet, "status");

          field1.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }
        else if (AsChar(import.ChildSupportWorksheet.Status) != 'T' && AsChar
          (import.ChildSupportWorksheet.Status) != 'S' && AsChar
          (import.ChildSupportWorksheet.Status) != 'J')
        {
          var field1 = GetField(export.ChildSupportWorksheet, "status");

          field1.Error = true;

          ExitState = "OE0000_INVALID_STATUS";
        }

        if (AsChar(import.ChildSupportWorksheet.Status) == 'J' && import
          .Prev2.Identifier > 0)
        {
          var field1 = GetField(export.ChildSupportWorksheet, "status");

          field1.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.ChildSupportWorksheet.Status = "";
            export.Prev2.Identifier = 0;
            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }
        }

        if (IsEmpty(import.ChildSupportWorksheet.MultipleFamilyAdjInd))
        {
          export.ChildSupportWorksheet.MultipleFamilyAdjInd = "N";
        }
        else if (AsChar(import.ChildSupportWorksheet.MultipleFamilyAdjInd) != 'Y'
          && AsChar(import.ChildSupportWorksheet.MultipleFamilyAdjInd) != 'N')
        {
          var field1 =
            GetField(export.ChildSupportWorksheet, "multipleFamilyAdjInd");

          field1.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "INVALID_SWITCH";
          }
        }

        if (IsEmpty(import.ChildSupportWorksheet.MultipleFamilyAdjInd) || AsChar
          (import.ChildSupportWorksheet.MultipleFamilyAdjInd) == 'N')
        {
          if (export.ChildSupportWorksheet.AdditionalNoOfChildren.
            GetValueOrDefault() > 0)
          {
            var field1 =
              GetField(export.ChildSupportWorksheet, "multipleFamilyAdjInd");

            field1.Error = true;

            ExitState = "OE0000_WORKSHEET_FAMILY_ADJ2";
          }
        }

        if (AsChar(import.ChildSupportWorksheet.MultipleFamilyAdjInd) == 'Y'
          && import
          .ChildSupportWorksheet.AdditionalNoOfChildren.GetValueOrDefault() == 0
          )
        {
          var field1 =
            GetField(export.ChildSupportWorksheet, "additionalNoOfChildren");

          field1.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0000_NO_OF_ADD_CHILDREN_REQD2";
          }
        }

        // @@@ Changes below...
        // @@@ Changes above...
        if (IsEmpty(import.ChildSupportWorksheet.CostOfLivingDiffAdjInd))
        {
          export.ChildSupportWorksheet.CostOfLivingDiffAdjInd = "N";
        }
        else if (AsChar(import.ChildSupportWorksheet.CostOfLivingDiffAdjInd) !=
          'Y' && AsChar
          (import.ChildSupportWorksheet.CostOfLivingDiffAdjInd) != 'N')
        {
          var field1 =
            GetField(export.ChildSupportWorksheet, "costOfLivingDiffAdjInd");

          field1.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "INVALID_SWITCH";
          }
        }

        if (import.ParentBCsePersonSupportWorksheet.CourtOrderedMaintenancePaid.
          GetValueOrDefault() > 0 && IsEmpty
          (import.ParentBCsePersonSupportWorksheet.MaintenancePaidCourtOrderNo))
        {
          var field1 =
            GetField(export.ParentBCsePersonSupportWorksheet,
            "maintenancePaidCourtOrderNo");

          field1.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0000_COURT_ORDER_NO_REQD";
          }
        }

        if (import.ParentACsePersonSupportWorksheet.CourtOrderedMaintenancePaid.
          GetValueOrDefault() > 0 && IsEmpty
          (import.ParentACsePersonSupportWorksheet.MaintenancePaidCourtOrderNo))
        {
          var field1 =
            GetField(export.ParentACsePersonSupportWorksheet,
            "maintenancePaidCourtOrderNo");

          field1.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0000_COURT_ORDER_NO_REQD";
          }
        }

        if (import.ParentBCsePersonSupportWorksheet.
          CourtOrderedMaintenanceRecvd.GetValueOrDefault() > 0 && IsEmpty
          (import.ParentBCsePersonSupportWorksheet.MaintenanceRecvdCourtOrderNo))
          
        {
          var field1 =
            GetField(export.ParentBCsePersonSupportWorksheet,
            "maintenanceRecvdCourtOrderNo");

          field1.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0000_COURT_ORDER_NO_REQD";
          }
        }

        if (import.ParentACsePersonSupportWorksheet.
          CourtOrderedMaintenanceRecvd.GetValueOrDefault() > 0 && IsEmpty
          (import.ParentACsePersonSupportWorksheet.MaintenanceRecvdCourtOrderNo))
          
        {
          var field1 =
            GetField(export.ParentACsePersonSupportWorksheet,
            "maintenanceRecvdCourtOrderNo");

          field1.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0000_COURT_ORDER_NO_REQD";
          }
        }

        if (import.ParentBCsePersonSupportWorksheet.
          CourtOrderedChildSupportPaid.GetValueOrDefault() > 0 && IsEmpty
          (import.ParentBCsePersonSupportWorksheet.ChildSupprtPaidCourtOrderNo))
        {
          var field1 =
            GetField(export.ParentBCsePersonSupportWorksheet,
            "childSupprtPaidCourtOrderNo");

          field1.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0000_COURT_ORDER_NO_REQD";
          }
        }

        if (import.ParentACsePersonSupportWorksheet.
          CourtOrderedChildSupportPaid.GetValueOrDefault() > 0 && IsEmpty
          (import.ParentACsePersonSupportWorksheet.ChildSupprtPaidCourtOrderNo))
        {
          var field1 =
            GetField(export.ParentACsePersonSupportWorksheet,
            "childSupprtPaidCourtOrderNo");

          field1.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0000_COURT_ORDER_NO_REQD";
          }
        }

        if (export.ParentACsePersonSupportWorksheet.ReasonableBusinessExpense.
          GetValueOrDefault() > 0 && export
          .ParentACsePersonSupportWorksheet.SelfEmploymentGrossIncome.
            GetValueOrDefault() == 0)
        {
          var field1 =
            GetField(export.ParentACsePersonSupportWorksheet,
            "reasonableBusinessExpense");

          field1.Error = true;

          var field2 =
            GetField(export.ParentACsePersonSupportWorksheet,
            "selfEmploymentGrossIncome");

          field2.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0000_SELF_EMPL_GROSS_INCOM_REQ";
          }
        }

        if (export.ParentBCsePersonSupportWorksheet.ReasonableBusinessExpense.
          GetValueOrDefault() > 0 && export
          .ParentBCsePersonSupportWorksheet.SelfEmploymentGrossIncome.
            GetValueOrDefault() == 0)
        {
          var field1 =
            GetField(export.ParentBCsePersonSupportWorksheet,
            "selfEmploymentGrossIncome");

          field1.Error = true;

          var field2 =
            GetField(export.ParentBCsePersonSupportWorksheet,
            "reasonableBusinessExpense");

          field2.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0000_SELF_EMPL_GROSS_INCOM_REQ";
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (!Equal(export.CommandH.Command, "CALCULAT"))
        {
          ExitState = "OE0000_CALCULATE_FIRST";

          return;
        }
        else
        {
          export.CommandH.Command = "";
        }

        // ----------------------------------------------------------------
        // Beginning Of Code
        // PR - 100471
        // Display Authorizing Authority
        // ----------------------------------------------------------------
        if (AsChar(import.ChildSupportWorksheet.Status) == 'J' && (
          export.ParentAF2TotalCsAdj.TotalCurrency != 0 || export
          .ParentBF2TotalCsAdj.TotalCurrency != 0))
        {
          if (IsEmpty(export.AuthorizingPrompt.SelectChar) && IsEmpty
            (export.ChildSupportWorksheet.AuthorizingAuthority))
          {
            var field1 = GetField(export.AuthorizingPrompt, "selectChar");

            field1.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            }
          }

          if (!IsEmpty(export.AuthorizingPrompt.SelectChar) && AsChar
            (export.AuthorizingPrompt.SelectChar) != 'S')
          {
            var field1 = GetField(export.AuthorizingPrompt, "selectChar");

            field1.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
            }
          }

          if (AsChar(export.AuthorizingPrompt.SelectChar) == 'S')
          {
            var field1 = GetField(export.AuthorizingPrompt, "selectChar");

            field1.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_INVALID_SELECT_CODE2";
            }
          }
        }
        else
        {
          if (AsChar(import.ChildSupportWorksheet.Status) == 'J')
          {
            if (export.ParentAF2TotalCsAdj.TotalCurrency == 0)
            {
              if (export.ParentBF2TotalCsAdj.TotalCurrency == 0)
              {
                if (!IsEmpty(export.AuthorizingPrompt.SelectChar) && !
                  IsEmpty(import.ChildSupportWorksheet.AuthorizingAuthority))
                {
                  var field1 =
                    GetField(export.ChildSupportWorksheet,
                    "authorizingAuthority");

                  field1.Error = true;

                  var field2 = GetField(export.AuthorizingPrompt, "selectChar");

                  field2.Error = true;

                  if (IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    ExitState = "ACO_NE0000_INVALID_SELECT_CODE3";

                    goto Test1;
                  }
                }

                if (!IsEmpty(export.AuthorizingPrompt.SelectChar) && IsEmpty
                  (import.ChildSupportWorksheet.AuthorizingAuthority))
                {
                  var field1 = GetField(export.AuthorizingPrompt, "selectChar");

                  field1.Error = true;

                  if (IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    ExitState = "ACO_NE0000_INVALID_SELECT_CODE3";

                    goto Test1;
                  }
                }

                if (IsEmpty(export.AuthorizingPrompt.SelectChar) && !
                  IsEmpty(import.ChildSupportWorksheet.AuthorizingAuthority))
                {
                  var field1 =
                    GetField(export.ChildSupportWorksheet,
                    "authorizingAuthority");

                  field1.Error = true;

                  if (IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    ExitState = "ACO_NE0000_INVALID_SELECT_CODE3";

                    goto Test1;
                  }
                }

                if (IsEmpty(export.AuthorizingPrompt.SelectChar) && IsEmpty
                  (import.ChildSupportWorksheet.AuthorizingAuthority))
                {
                  goto Test1;
                }
              }
            }
          }

          if ((AsChar(import.ChildSupportWorksheet.Status) == 'T' || AsChar
            (import.ChildSupportWorksheet.Status) == 'S') && (
              !IsEmpty(export.AuthorizingPrompt.SelectChar) || !
            IsEmpty(import.ChildSupportWorksheet.AuthorizingAuthority)))
          {
            var field1 =
              GetField(export.ChildSupportWorksheet, "authorizingAuthority");

            field1.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_INVALID_SELECT_CODE4";
            }
          }
        }

Test1:

        // ----------------------------------------------------------------
        // End Of Code
        // ----------------------------------------------------------------
        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // ----------------------------------------------------------------
        // Beginning Of Code
        // PR - 100471
        // Display Authorizing Authority
        // ----------------------------------------------------------------
        if (!Equal(import.ChildSupportWorksheet.AuthorizingAuthority, "JUDGE") &&
          !
          Equal(import.ChildSupportWorksheet.AuthorizingAuthority, "HEARO") && !
          Equal(import.ChildSupportWorksheet.AuthorizingAuthority, "OTHER") && !
          IsEmpty(import.ChildSupportWorksheet.AuthorizingAuthority))
        {
          var field1 =
            GetField(export.ChildSupportWorksheet, "authorizingAuthority");

          field1.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE5";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // ----------------------------------------------------------------
        // End Of Code
        // ----------------------------------------------------------------
        UseOeWorkAddChildSupWorksheet();

        if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
        {
          export.PrevCase.Number = export.Case1.Number;
          export.PrevChildSupportWorksheet.Assign(export.ChildSupportWorksheet);
          MoveChildSupportWorksheet2(export.ChildSupportWorksheet, export.Prev2);
            
          export.PrevParentACsePerson.Number = export.ParentACsePerson.Number;
          export.PrevParentBCsePerson.Number = export.ParentBCsePerson.Number;
          MoveCsePersonSupportWorksheet2(export.
            ParentACsePersonSupportWorksheet,
            export.PrevParentACsePersonSupportWorksheet);
          MoveCsePersonSupportWorksheet2(export.
            ParentBCsePersonSupportWorksheet,
            export.PrevParentBCsePersonSupportWorksheet);

          // @@@z
          export.GuidelineYearChange.Flag = "";
        }

        break;
      case "UPDATE":
        ExitState = "ACO_NN0000_ALL_OK";

        if (import.PrevChildSupportWorksheet.Identifier == 0)
        {
          ExitState = "OE0013_DISP_REC_BEFORE_UPD";

          return;
        }

        if (export.ChildSupportWorksheet.CsGuidelineYear != export
          .Prev2.CsGuidelineYear)
        {
          ExitState = "OE0000_CANNOT_CHANGE_GL_YEAR";

          return;
        }

        if (AsChar(export.Prev2.Status) == 'J')
        {
          ExitState = "OE0000_CANNOT_MODIFY_WORKSHEET";

          var field1 = GetField(export.ChildSupportWorksheet, "status");

          field1.Error = true;

          export.ChildSupportWorksheet.Status = import.Prev2.Status;
          export.PrevChildSupportWorksheet.Status = import.Prev2.Status;

          return;
        }

        if (!Equal(export.Case1.Number, export.PrevCase.Number))
        {
          var field1 = GetField(export.Case1, "number");

          field1.Error = true;

          export.Case1.Number = export.PrevCase.Number;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_NEW_KEY_ON_UPDATE";
          }
        }

        if (!IsEmpty(import.PrevParentACsePerson.Number) && !
          Equal(import.PrevParentACsePerson.Number,
          import.ParentACsePerson.Number))
        {
          var field1 = GetField(export.ParentACsePerson, "number");

          field1.Error = true;

          export.ParentACsePerson.Number = export.PrevParentACsePerson.Number;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_NEW_KEY_ON_UPDATE";
          }
        }

        if (!IsEmpty(export.PrevParentBCsePerson.Number) && !
          Equal(export.PrevParentBCsePerson.Number,
          export.ParentBCsePerson.Number))
        {
          var field1 = GetField(export.ParentBCsePerson, "number");

          field1.Error = true;

          export.ParentBCsePerson.Number = export.PrevParentBCsePerson.Number;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_NEW_KEY_ON_UPDATE";
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (!IsEmpty(export.ParentACsePerson.Number) && IsEmpty
          (export.PrevParentACsePerson.Number))
        {
          UseOeCabCheckCaseMember6();

          // -- Center the first name
          export.ParentAName.FirstName = UseOeWorkCenterFirstName2();

          if (!IsEmpty(local.Error.Flag))
          {
            var field1 = GetField(export.ParentACsePerson, "number");

            field1.Error = true;

            ExitState = "OE0000_CASE_MEMBER_NE";
          }
        }

        if (!IsEmpty(export.ParentBCsePerson.Number) && IsEmpty
          (export.PrevParentBCsePerson.Number))
        {
          UseOeCabCheckCaseMember5();

          // -- Center the first name
          export.ParentBName.FirstName = UseOeWorkCenterFirstName1();

          if (!IsEmpty(local.Error.Flag))
          {
            var field1 = GetField(export.ParentBCsePerson, "number");

            field1.Error = true;

            ExitState = "OE0000_CASE_MEMBER_NE";
          }
        }

        if (!IsEmpty(local.Error.Flag))
        {
          return;
        }

        if (!IsEmpty(export.LegalAction.CourtCaseNumber))
        {
          UseOeWorkReadCourtOrderDetails2();

          if (IsExitState("OE0000_COURT_ORDER_NF") || IsExitState
            ("OE0000_LEGAL_ACTION_NOT_4_PERSON"))
          {
            var field1 = GetField(export.LegalAction, "courtCaseNumber");

            field1.Color = "red";
            field1.Highlighting = Highlighting.Underscore;
            field1.Protected = true;

            export.LegalAction.Identifier = 0;

            return;
          }
        }

        if (IsEmpty(export.ParentACsePerson.Number) && (
          import.ParentACsePersonSupportWorksheet.WageEarnerGrossIncome.
            GetValueOrDefault() != 0 || import
          .ParentACsePersonSupportWorksheet.SelfEmploymentGrossIncome.
            GetValueOrDefault() != 0))
        {
          var field1 = GetField(export.ParentACsePerson, "number");

          field1.Error = true;

          var field2 =
            GetField(export.ParentACsePersonSupportWorksheet,
            "selfEmploymentGrossIncome");

          field2.Error = true;

          var field3 =
            GetField(export.ParentACsePersonSupportWorksheet,
            "wageEarnerGrossIncome");

          field3.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }
        }

        if (IsEmpty(export.ParentBCsePerson.Number) && (
          import.ParentBCsePersonSupportWorksheet.WageEarnerGrossIncome.
            GetValueOrDefault() != 0 || export
          .ParentBCsePersonSupportWorksheet.SelfEmploymentGrossIncome.
            GetValueOrDefault() != 0))
        {
          var field1 = GetField(export.ParentBCsePerson, "number");

          field1.Error = true;

          var field2 =
            GetField(export.ParentBCsePersonSupportWorksheet,
            "selfEmploymentGrossIncome");

          field2.Error = true;

          var field3 =
            GetField(export.ParentBCsePersonSupportWorksheet,
            "wageEarnerGrossIncome");

          field3.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (AsChar(export.ChildSupportWorksheet.Status) != AsChar
          (export.Prev2.Status))
        {
          if (AsChar(import.Prev2.Status) == 'S' && AsChar
            (export.ChildSupportWorksheet.Status) != 'J')
          {
            var field1 = GetField(export.ChildSupportWorksheet, "status");

            field1.Error = true;

            export.ChildSupportWorksheet.Status =
              export.PrevChildSupportWorksheet.Status;
            ExitState = "OE0000_STATUS_CANNOT_BE_REVERSED";
          }

          if (AsChar(export.Prev2.Status) == 'J')
          {
            var field1 = GetField(export.ChildSupportWorksheet, "status");

            field1.Error = true;

            export.ChildSupportWorksheet.Status =
              export.PrevChildSupportWorksheet.Status;
            ExitState = "OE0000_STATUS_CANNOT_BE_REVERSED";
          }
        }

        if (IsEmpty(import.ChildSupportWorksheet.Status))
        {
          var field1 = GetField(export.ChildSupportWorksheet, "status");

          field1.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }
        else if (AsChar(import.ChildSupportWorksheet.Status) != 'T' && AsChar
          (import.ChildSupportWorksheet.Status) != 'S' && AsChar
          (import.ChildSupportWorksheet.Status) != 'J')
        {
          var field1 = GetField(export.ChildSupportWorksheet, "status");

          field1.Error = true;

          ExitState = "OE0000_INVALID_STATUS";
        }

        if (AsChar(import.ChildSupportWorksheet.Status) == 'J')
        {
          if (IsEmpty(import.LegalAction.CourtCaseNumber))
          {
            var field1 = GetField(export.ChildSupportWorksheet, "status");

            field1.Error = true;

            ExitState = "OE0000_COURT_CASE_REQD";
          }
          else if (export.LegalAction.Identifier == 0)
          {
            var field1 = GetField(export.LegalAction, "courtCaseNumber");

            field1.Color = "red";
            field1.Highlighting = Highlighting.Underscore;
            field1.Protected = true;

            ExitState = "OE0000_MUST_FLOW_TO_LACS";
          }
        }

        if (IsEmpty(import.ChildSupportWorksheet.MultipleFamilyAdjInd))
        {
          export.ChildSupportWorksheet.MultipleFamilyAdjInd = "N";
        }
        else if (AsChar(import.ChildSupportWorksheet.MultipleFamilyAdjInd) != 'Y'
          && AsChar(import.ChildSupportWorksheet.MultipleFamilyAdjInd) != 'N')
        {
          var field1 =
            GetField(export.ChildSupportWorksheet, "multipleFamilyAdjInd");

          field1.Error = true;

          ExitState = "INVALID_SWITCH";
        }

        if (IsEmpty(import.ChildSupportWorksheet.MultipleFamilyAdjInd) || AsChar
          (import.ChildSupportWorksheet.MultipleFamilyAdjInd) == 'N')
        {
          if (export.ChildSupportWorksheet.AdditionalNoOfChildren.
            GetValueOrDefault() > 0)
          {
            var field1 =
              GetField(export.ChildSupportWorksheet, "multipleFamilyAdjInd");

            field1.Error = true;

            ExitState = "OE0000_WORKSHEET_FAMILY_ADJ2";
          }
        }

        if (AsChar(import.ChildSupportWorksheet.MultipleFamilyAdjInd) == 'Y'
          && import
          .ChildSupportWorksheet.AdditionalNoOfChildren.GetValueOrDefault() == 0
          )
        {
          var field1 =
            GetField(export.ChildSupportWorksheet, "additionalNoOfChildren");

          field1.Error = true;

          ExitState = "OE0000_NO_OF_ADD_CHILDREN_REQD2";
        }

        // @@@ Changes below...
        // @@@ Changes above...
        if (IsEmpty(import.ChildSupportWorksheet.CostOfLivingDiffAdjInd))
        {
          export.ChildSupportWorksheet.CostOfLivingDiffAdjInd = "N";
        }
        else if (AsChar(import.ChildSupportWorksheet.CostOfLivingDiffAdjInd) !=
          'Y' && AsChar
          (import.ChildSupportWorksheet.CostOfLivingDiffAdjInd) != 'N')
        {
          var field1 =
            GetField(export.ChildSupportWorksheet, "costOfLivingDiffAdjInd");

          field1.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "INVALID_SWITCH";
          }
        }

        if (import.ParentBCsePersonSupportWorksheet.CourtOrderedMaintenancePaid.
          GetValueOrDefault() > 0 && IsEmpty
          (import.ParentBCsePersonSupportWorksheet.MaintenancePaidCourtOrderNo))
        {
          var field1 =
            GetField(export.ParentBCsePersonSupportWorksheet,
            "maintenancePaidCourtOrderNo");

          field1.Error = true;

          ExitState = "OE0000_COURT_ORDER_NO_REQD";
        }

        if (import.ParentACsePersonSupportWorksheet.CourtOrderedMaintenancePaid.
          GetValueOrDefault() > 0 && IsEmpty
          (import.ParentACsePersonSupportWorksheet.MaintenancePaidCourtOrderNo))
        {
          var field1 =
            GetField(export.ParentACsePersonSupportWorksheet,
            "maintenancePaidCourtOrderNo");

          field1.Error = true;

          ExitState = "OE0000_COURT_ORDER_NO_REQD";
        }

        if (import.ParentBCsePersonSupportWorksheet.
          CourtOrderedMaintenanceRecvd.GetValueOrDefault() > 0 && IsEmpty
          (import.ParentBCsePersonSupportWorksheet.MaintenanceRecvdCourtOrderNo))
          
        {
          var field1 =
            GetField(export.ParentBCsePersonSupportWorksheet,
            "maintenanceRecvdCourtOrderNo");

          field1.Error = true;

          ExitState = "OE0000_COURT_ORDER_NO_REQD";
        }

        if (import.ParentACsePersonSupportWorksheet.
          CourtOrderedMaintenanceRecvd.GetValueOrDefault() > 0 && IsEmpty
          (import.ParentACsePersonSupportWorksheet.MaintenanceRecvdCourtOrderNo))
          
        {
          var field1 =
            GetField(export.ParentACsePersonSupportWorksheet,
            "maintenanceRecvdCourtOrderNo");

          field1.Error = true;

          ExitState = "OE0000_COURT_ORDER_NO_REQD";
        }

        if (import.ParentBCsePersonSupportWorksheet.
          CourtOrderedChildSupportPaid.GetValueOrDefault() > 0 && IsEmpty
          (import.ParentBCsePersonSupportWorksheet.ChildSupprtPaidCourtOrderNo))
        {
          var field1 =
            GetField(export.ParentBCsePersonSupportWorksheet,
            "childSupprtPaidCourtOrderNo");

          field1.Error = true;

          ExitState = "OE0000_COURT_ORDER_NO_REQD";
        }

        if (import.ParentACsePersonSupportWorksheet.
          CourtOrderedChildSupportPaid.GetValueOrDefault() > 0 && IsEmpty
          (import.ParentACsePersonSupportWorksheet.ChildSupprtPaidCourtOrderNo))
        {
          var field1 =
            GetField(export.ParentACsePersonSupportWorksheet,
            "childSupprtPaidCourtOrderNo");

          field1.Error = true;

          ExitState = "OE0000_COURT_ORDER_NO_REQD";
        }

        if (export.ParentACsePersonSupportWorksheet.ReasonableBusinessExpense.
          GetValueOrDefault() > 0 && export
          .ParentACsePersonSupportWorksheet.SelfEmploymentGrossIncome.
            GetValueOrDefault() == 0)
        {
          var field1 =
            GetField(export.ParentACsePersonSupportWorksheet,
            "reasonableBusinessExpense");

          field1.Error = true;

          var field2 =
            GetField(export.ParentACsePersonSupportWorksheet,
            "selfEmploymentGrossIncome");

          field2.Error = true;

          ExitState = "OE0000_SELF_EMPL_GROSS_INCOM_REQ";
        }

        if (export.ParentBCsePersonSupportWorksheet.ReasonableBusinessExpense.
          GetValueOrDefault() > 0 && export
          .ParentBCsePersonSupportWorksheet.SelfEmploymentGrossIncome.
            GetValueOrDefault() == 0)
        {
          var field1 =
            GetField(export.ParentBCsePersonSupportWorksheet,
            "selfEmploymentGrossIncome");

          field1.Error = true;

          var field2 =
            GetField(export.ParentBCsePersonSupportWorksheet,
            "reasonableBusinessExpense");

          field2.Error = true;

          ExitState = "OE0000_SELF_EMPL_GROSS_INCOM_REQ";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (!Equal(export.CommandH.Command, "CALCULAT"))
        {
          ExitState = "OE0000_CALCULATE_FIRST";

          return;
        }
        else
        {
          export.CommandH.Command = "";
        }

        // ----------------------------------------------------------------
        // Beginning Of Code
        // PR - 100471
        // Display Authorizing Authority
        // ----------------------------------------------------------------
        if (AsChar(import.ChildSupportWorksheet.Status) == 'J' && (
          export.ParentAF2TotalCsAdj.TotalCurrency != 0 || export
          .ParentBF2TotalCsAdj.TotalCurrency != 0))
        {
          if (IsEmpty(export.AuthorizingPrompt.SelectChar) && IsEmpty
            (export.ChildSupportWorksheet.AuthorizingAuthority))
          {
            var field1 = GetField(export.AuthorizingPrompt, "selectChar");

            field1.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            }
          }

          if (!IsEmpty(export.AuthorizingPrompt.SelectChar) && AsChar
            (export.AuthorizingPrompt.SelectChar) != 'S')
          {
            var field1 = GetField(export.AuthorizingPrompt, "selectChar");

            field1.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
            }
          }

          if (AsChar(export.AuthorizingPrompt.SelectChar) == 'S')
          {
            var field1 = GetField(export.AuthorizingPrompt, "selectChar");

            field1.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_INVALID_SELECT_CODE2";
            }
          }
        }
        else
        {
          if (AsChar(import.ChildSupportWorksheet.Status) == 'J')
          {
            if (export.ParentAF2TotalCsAdj.TotalCurrency == 0)
            {
              if (export.ParentBF2TotalCsAdj.TotalCurrency == 0)
              {
                if (!IsEmpty(export.AuthorizingPrompt.SelectChar) && !
                  IsEmpty(import.ChildSupportWorksheet.AuthorizingAuthority))
                {
                  var field1 =
                    GetField(export.ChildSupportWorksheet,
                    "authorizingAuthority");

                  field1.Error = true;

                  var field2 = GetField(export.AuthorizingPrompt, "selectChar");

                  field2.Error = true;

                  if (IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    ExitState = "ACO_NE0000_INVALID_SELECT_CODE3";

                    goto Test2;
                  }
                }

                if (!IsEmpty(export.AuthorizingPrompt.SelectChar) && IsEmpty
                  (import.ChildSupportWorksheet.AuthorizingAuthority))
                {
                  var field1 = GetField(export.AuthorizingPrompt, "selectChar");

                  field1.Error = true;

                  if (IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    ExitState = "ACO_NE0000_INVALID_SELECT_CODE3";

                    goto Test2;
                  }
                }

                if (IsEmpty(export.AuthorizingPrompt.SelectChar) && !
                  IsEmpty(import.ChildSupportWorksheet.AuthorizingAuthority))
                {
                  var field1 =
                    GetField(export.ChildSupportWorksheet,
                    "authorizingAuthority");

                  field1.Error = true;

                  if (IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    ExitState = "ACO_NE0000_INVALID_SELECT_CODE3";

                    goto Test2;
                  }
                }

                if (IsEmpty(export.AuthorizingPrompt.SelectChar) && IsEmpty
                  (import.ChildSupportWorksheet.AuthorizingAuthority))
                {
                  goto Test2;
                }
              }
            }
          }

          if ((AsChar(import.ChildSupportWorksheet.Status) == 'T' || AsChar
            (import.ChildSupportWorksheet.Status) == 'S') && (
              !IsEmpty(export.AuthorizingPrompt.SelectChar) || !
            IsEmpty(import.ChildSupportWorksheet.AuthorizingAuthority)))
          {
            var field1 =
              GetField(export.ChildSupportWorksheet, "authorizingAuthority");

            field1.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_INVALID_SELECT_CODE4";
            }
          }
        }

Test2:

        // ----------------------------------------------------------------
        // End Of Code
        // ----------------------------------------------------------------
        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // ----------------------------------------------------------------
        // Beginning Of Code
        // PR - 100471
        // Display Authorizing Authority
        // ----------------------------------------------------------------
        if (!Equal(import.ChildSupportWorksheet.AuthorizingAuthority, "JUDGE") &&
          !
          Equal(import.ChildSupportWorksheet.AuthorizingAuthority, "HEARO") && !
          Equal(import.ChildSupportWorksheet.AuthorizingAuthority, "OTHER") && !
          IsEmpty(import.ChildSupportWorksheet.AuthorizingAuthority))
        {
          var field1 =
            GetField(export.ChildSupportWorksheet, "authorizingAuthority");

          field1.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE5";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // ----------------------------------------------------------------
        // End Of Code
        // ----------------------------------------------------------------
        UseOeWorkUpdtChildSupWorksheet();

        if (IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE"))
        {
          export.LastUpdtDate.EffectiveDate = Now().Date;
          export.PrevCase.Number = export.Case1.Number;
          export.PrevChildSupportWorksheet.Assign(export.ChildSupportWorksheet);
          MoveChildSupportWorksheet2(export.ChildSupportWorksheet, export.Prev2);
            
          export.PrevParentACsePerson.Number = export.ParentACsePerson.Number;
          export.PrevParentBCsePerson.Number = export.ParentBCsePerson.Number;
          MoveCsePersonSupportWorksheet2(export.
            ParentACsePersonSupportWorksheet,
            export.PrevParentACsePersonSupportWorksheet);
          MoveCsePersonSupportWorksheet2(export.
            ParentBCsePersonSupportWorksheet,
            export.PrevParentBCsePersonSupportWorksheet);
        }

        break;
      case "NEXT":
        ExitState = "ACO_NN0000_ALL_OK";

        if (IsEmpty(export.Case1.Number))
        {
          var field1 = GetField(export.Case1, "number");

          field1.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }
        else
        {
        }

        if (IsEmpty(export.ParentACsePerson.Number) && IsEmpty
          (export.ParentBCsePerson.Number))
        {
          var field1 = GetField(export.ParentACsePerson, "number");

          field1.Error = true;

          var field2 = GetField(export.ParentBCsePerson, "number");

          field2.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }
        }

        if (!IsEmpty(export.ParentAPrompt.SelectChar))
        {
          var field1 = GetField(export.ParentAPrompt, "selectChar");

          field1.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0174_WORKSHEET_SCROLL_ERROR";
          }
        }

        if (!IsEmpty(export.ParentBPrompt.SelectChar))
        {
          var field1 = GetField(export.ParentBPrompt, "selectChar");

          field1.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0174_WORKSHEET_SCROLL_ERROR";
          }
        }

        if (export.ChildSupportWorksheet.CsGuidelineYear == 0)
        {
          var field1 =
            GetField(export.ChildSupportWorksheet, "csGuidelineYear");

          field1.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        UseOeCabCheckCaseMember4();

        if (!IsEmpty(local.WorkError.Flag))
        {
          ExitState = "CASE_NF";

          var field1 = GetField(export.Case1, "number");

          field1.Error = true;

          return;
        }

        if (!IsEmpty(export.ParentACsePerson.Number))
        {
          UseOeCabCheckCaseMember3();

          // -- Center the first name
          export.ParentAName.FirstName = UseOeWorkCenterFirstName2();

          if (AsChar(local.WorkError.Flag) == 'P')
          {
            ExitState = "CSE_PERSON_NF";

            var field1 = GetField(export.ParentACsePerson, "number");

            field1.Error = true;
          }
          else if (AsChar(local.WorkError.Flag) == 'R')
          {
            ExitState = "OE0000_CASE_MEMBER_NE";

            var field1 = GetField(export.ParentACsePerson, "number");

            field1.Error = true;

            var field2 = GetField(export.Case1, "number");

            field2.Error = true;
          }
        }

        if (!IsEmpty(export.ParentBCsePerson.Number))
        {
          UseOeCabCheckCaseMember2();

          // -- Center the first name
          export.ParentBName.FirstName = UseOeWorkCenterFirstName1();

          if (AsChar(local.WorkError.Flag) == 'P')
          {
            ExitState = "CSE_PERSON_NF";

            var field1 = GetField(export.ParentBCsePerson, "number");

            field1.Error = true;
          }
          else if (AsChar(local.WorkError.Flag) == 'R')
          {
            ExitState = "OE0000_CASE_MEMBER_NE";

            var field1 = GetField(export.ParentBCsePerson, "number");

            field1.Error = true;

            var field2 = GetField(export.Case1, "number");

            field2.Error = true;
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (AsChar(import.ChildSupportWorksheet.Status) == 'J' || AsChar
          (import.ChildSupportWorksheet.Status) == 'N')
        {
          if (IsEmpty(import.LegalAction.CourtCaseNumber))
          {
            var field1 = GetField(export.LegalAction, "courtCaseNumber");

            field1.Color = "red";
            field1.Highlighting = Highlighting.Underscore;
            field1.Protected = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "OE0000_COURT_CASE_REQD";
            }
          }
          else if (export.LegalAction.Identifier == 0)
          {
            var field1 = GetField(export.LegalAction, "courtCaseNumber");

            field1.Color = "red";
            field1.Highlighting = Highlighting.Underscore;
            field1.Protected = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "OE0000_MUST_FLOW_TO_LACS";
            }
          }
        }

        if (IsEmpty(export.ParentACsePerson.Number) && (
          export.ParentACsePersonSupportWorksheet.WageEarnerGrossIncome.
            GetValueOrDefault() != 0 || export
          .ParentACsePersonSupportWorksheet.SelfEmploymentGrossIncome.
            GetValueOrDefault() != 0))
        {
          var field1 = GetField(export.ParentACsePerson, "number");

          field1.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }
        }

        if (IsEmpty(export.ParentBCsePerson.Number) && (
          export.ParentBCsePersonSupportWorksheet.WageEarnerGrossIncome.
            GetValueOrDefault() != 0 || export
          .ParentBCsePersonSupportWorksheet.SelfEmploymentGrossIncome.
            GetValueOrDefault() != 0))
        {
          var field1 = GetField(export.ParentBCsePerson, "number");

          field1.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }
        }

        if (export.ParentACsePersonSupportWorksheet.ReasonableBusinessExpense.
          GetValueOrDefault() > 0 && export
          .ParentACsePersonSupportWorksheet.SelfEmploymentGrossIncome.
            GetValueOrDefault() == 0)
        {
          var field1 =
            GetField(export.ParentACsePersonSupportWorksheet,
            "reasonableBusinessExpense");

          field1.Error = true;

          var field2 =
            GetField(export.ParentACsePersonSupportWorksheet,
            "selfEmploymentGrossIncome");

          field2.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0000_SELF_EMPL_GROSS_INCOM_REQ";
          }
        }

        if (import.ParentBCsePersonSupportWorksheet.ReasonableBusinessExpense.
          GetValueOrDefault() > import
          .ParentBCsePersonSupportWorksheet.SelfEmploymentGrossIncome.
            GetValueOrDefault())
        {
          var field1 =
            GetField(export.ParentBCsePersonSupportWorksheet,
            "selfEmploymentGrossIncome");

          field1.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0000_INVALID_BUSINESS_EXPENSE";
          }
        }

        if (export.ParentBCsePersonSupportWorksheet.ReasonableBusinessExpense.
          GetValueOrDefault() > 0 && export
          .ParentBCsePersonSupportWorksheet.SelfEmploymentGrossIncome.
            GetValueOrDefault() == 0)
        {
          var field1 =
            GetField(export.ParentBCsePersonSupportWorksheet,
            "selfEmploymentGrossIncome");

          field1.Error = true;

          var field2 =
            GetField(export.ParentBCsePersonSupportWorksheet,
            "reasonableBusinessExpense");

          field2.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0000_SELF_EMPL_GROSS_INCOM_REQ";
          }
        }

        if (import.ParentACsePersonSupportWorksheet.
          CourtOrderedChildSupportPaid.GetValueOrDefault() > 0 && IsEmpty
          (import.ParentACsePersonSupportWorksheet.ChildSupprtPaidCourtOrderNo))
        {
          var field1 =
            GetField(export.ParentACsePersonSupportWorksheet,
            "childSupprtPaidCourtOrderNo");

          field1.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0000_COURT_ORDER_NO_REQD";
          }
        }

        if (import.ParentBCsePersonSupportWorksheet.
          CourtOrderedChildSupportPaid.GetValueOrDefault() > 0 && IsEmpty
          (import.ParentBCsePersonSupportWorksheet.ChildSupprtPaidCourtOrderNo))
        {
          var field1 =
            GetField(export.ParentBCsePersonSupportWorksheet,
            "childSupprtPaidCourtOrderNo");

          field1.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0000_COURT_ORDER_NO_REQD";
          }
        }

        if (import.ParentACsePersonSupportWorksheet.CourtOrderedMaintenancePaid.
          GetValueOrDefault() > 0 && IsEmpty
          (import.ParentACsePersonSupportWorksheet.MaintenancePaidCourtOrderNo))
        {
          var field1 =
            GetField(export.ParentACsePersonSupportWorksheet,
            "maintenancePaidCourtOrderNo");

          field1.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0000_COURT_ORDER_NO_REQD";
          }
        }

        if (import.ParentBCsePersonSupportWorksheet.CourtOrderedMaintenancePaid.
          GetValueOrDefault() > 0 && IsEmpty
          (import.ParentBCsePersonSupportWorksheet.MaintenancePaidCourtOrderNo))
        {
          var field1 =
            GetField(export.ParentBCsePersonSupportWorksheet,
            "maintenancePaidCourtOrderNo");

          field1.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0000_COURT_ORDER_NO_REQD";
          }
        }

        if (import.ParentACsePersonSupportWorksheet.
          CourtOrderedMaintenanceRecvd.GetValueOrDefault() > 0 && IsEmpty
          (import.ParentACsePersonSupportWorksheet.MaintenanceRecvdCourtOrderNo))
          
        {
          var field1 =
            GetField(export.ParentACsePersonSupportWorksheet,
            "maintenanceRecvdCourtOrderNo");

          field1.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0000_COURT_ORDER_NO_REQD";
          }
        }

        if (import.ParentBCsePersonSupportWorksheet.
          CourtOrderedMaintenanceRecvd.GetValueOrDefault() > 0 && IsEmpty
          (import.ParentBCsePersonSupportWorksheet.MaintenanceRecvdCourtOrderNo))
          
        {
          var field1 =
            GetField(export.ParentBCsePersonSupportWorksheet,
            "maintenanceRecvdCourtOrderNo");

          field1.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0000_COURT_ORDER_NO_REQD";
          }
        }

        if (IsEmpty(import.ChildSupportWorksheet.CostOfLivingDiffAdjInd))
        {
          export.ChildSupportWorksheet.CostOfLivingDiffAdjInd = "N";
        }
        else if (AsChar(import.ChildSupportWorksheet.CostOfLivingDiffAdjInd) !=
          'Y' && AsChar
          (import.ChildSupportWorksheet.CostOfLivingDiffAdjInd) != 'N')
        {
          var field1 =
            GetField(export.ChildSupportWorksheet, "costOfLivingDiffAdjInd");

          field1.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "INVALID_SWITCH";
          }
        }

        if (IsEmpty(import.ChildSupportWorksheet.MultipleFamilyAdjInd))
        {
          export.ChildSupportWorksheet.MultipleFamilyAdjInd = "N";
        }
        else if (AsChar(import.ChildSupportWorksheet.MultipleFamilyAdjInd) != 'Y'
          && AsChar(import.ChildSupportWorksheet.MultipleFamilyAdjInd) != 'N')
        {
          var field1 =
            GetField(export.ChildSupportWorksheet, "multipleFamilyAdjInd");

          field1.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "INVALID_SWITCH";
          }
        }

        if (IsEmpty(import.ChildSupportWorksheet.MultipleFamilyAdjInd) || AsChar
          (import.ChildSupportWorksheet.MultipleFamilyAdjInd) == 'N')
        {
          if (export.ChildSupportWorksheet.AdditionalNoOfChildren.
            GetValueOrDefault() > 0)
          {
            var field1 =
              GetField(export.ChildSupportWorksheet, "multipleFamilyAdjInd");

            field1.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "OE0000_WORKSHEET_FAMILY_ADJ2";
            }
          }
        }

        if (AsChar(import.ChildSupportWorksheet.MultipleFamilyAdjInd) == 'Y'
          && import
          .ChildSupportWorksheet.AdditionalNoOfChildren.GetValueOrDefault() == 0
          )
        {
          var field1 =
            GetField(export.ChildSupportWorksheet, "additionalNoOfChildren");

          field1.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0000_NO_OF_ADD_CHILDREN_REQD2";
          }
        }

        // @@@ Changes below...
        // @@@ Changes above...
        if (IsEmpty(import.ChildSupportWorksheet.CostOfLivingDiffAdjInd))
        {
          export.ChildSupportWorksheet.CostOfLivingDiffAdjInd = "N";
        }
        else if (AsChar(import.ChildSupportWorksheet.CostOfLivingDiffAdjInd) !=
          'Y' && AsChar
          (import.ChildSupportWorksheet.CostOfLivingDiffAdjInd) != 'N')
        {
          var field1 =
            GetField(export.ChildSupportWorksheet, "costOfLivingDiffAdjInd");

          field1.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "INVALID_SWITCH";
          }
        }

        if (IsEmpty(import.ChildSupportWorksheet.Status))
        {
          var field1 = GetField(export.ChildSupportWorksheet, "status");

          field1.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }
        }
        else if (AsChar(import.ChildSupportWorksheet.Status) != 'T' && AsChar
          (import.ChildSupportWorksheet.Status) != 'S' && AsChar
          (import.ChildSupportWorksheet.Status) != 'J')
        {
          var field1 = GetField(export.ChildSupportWorksheet, "status");

          field1.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0000_INVALID_STATUS";
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // ----------------------------------------------------------------
        // Beginning Of Code
        // PR - 100471
        // Display Authorizing Authority
        // ----------------------------------------------------------------
        if (!IsEmpty(import.ChildSupportWorksheet.AuthorizingAuthority))
        {
          export.ChildSupportWorksheet.AuthorizingAuthority =
            import.ChildSupportWorksheet.AuthorizingAuthority ?? "";
        }

        // ----------------------------------------------------------------
        // End Of Code
        // ----------------------------------------------------------------
        ExitState = "ECO_LNK_TO_CS_WORKSHEET_2";
        global.Command = "DISPPAGE";

        break;
      case "DELETE":
        if (import.PrevChildSupportWorksheet.Identifier == 0)
        {
          ExitState = "OE0012_DISP_REC_BEFORE_DELETE";

          return;
        }

        if (AsChar(export.Prev2.Status) == 'J')
        {
          ExitState = "OE0000_CANNOT_DELETE_WORKSHEET";

          return;
        }

        if (!Equal(export.Case1.Number, export.PrevCase.Number))
        {
          var field1 = GetField(export.Case1, "number");

          field1.Error = true;

          export.Case1.Number = export.PrevCase.Number;
          ExitState = "ACO_NE0000_NEW_KEY_ON_DELETE";

          return;
        }

        if (!IsEmpty(import.PrevParentACsePerson.Number) && !
          Equal(import.PrevParentACsePerson.Number,
          import.ParentACsePerson.Number))
        {
          var field1 = GetField(export.ParentACsePerson, "number");

          field1.Error = true;

          export.ParentACsePerson.Number = export.PrevParentACsePerson.Number;
          ExitState = "ACO_NE0000_NEW_KEY_ON_DELETE";

          return;
        }

        if (!IsEmpty(import.PrevParentBCsePerson.Number) && !
          Equal(import.PrevParentBCsePerson.Number,
          import.ParentBCsePerson.Number))
        {
          var field1 = GetField(export.ParentACsePerson, "number");

          field1.Error = true;

          export.ParentBCsePerson.Number = export.PrevParentBCsePerson.Number;
          ExitState = "ACO_NE0000_NEW_KEY_ON_DELETE";

          return;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";

        // ---------------------------------------------
        // 	CALL Delete Action Block.
        // ---------------------------------------------
        UseOeWorkDelChildSupWorksheet();
        export.WorkPrev.DeleteConfirmation = "";

        var field = GetField(export.WorkPrev, "deleteConfirmation");

        field.Protected = true;

        export.PrevChildSupportWorksheet.Identifier = 0;
        export.Prev2.Identifier = 0;
        export.LastUpdtDate.EffectiveDate = null;
        UseOeWorkReadChildSupWorksheet2();

        // -----------------------------------------------
        // The Child Support Worksheet was not found.
        // Clear up all protected screen fields.
        // -----------------------------------------------
        UseOeWorkCalcChildSupWorksheet2();
        ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";

        break;
      default:
        break;
    }

    // mjr
    // -----------------------------------------------------------
    // Pulled command Display out of main case of command so
    // that it can be executed after command PrintRet
    // --------------------------------------------------------------
    if (Equal(global.Command, "DISPLAY"))
    {
      ExitState = "ACO_NN0000_ALL_OK";

      // *****************************************************************
      // Check for required fields for display function
      // *****************************************************************
      if (IsEmpty(export.Case1.Number))
      {
        var field = GetField(export.Case1, "number");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        export.ChildSupportWorksheet.CsGuidelineYear =
          local.CurrentGuidelineYear.Year;

        return;
      }

      if (!IsEmpty(export.ParentACsePerson.Number))
      {
        if (IsEmpty(export.ParentAName.FormattedName))
        {
          UseOeCabCheckCaseMember8();

          // -- Center the first name
          export.ParentAName.FirstName = UseOeWorkCenterFirstName2();
        }
      }

      if (!IsEmpty(export.ParentBCsePerson.Number))
      {
        if (IsEmpty(export.ParentBName.FormattedName))
        {
          UseOeCabCheckCaseMember1();

          // -- Center the first name
          export.ParentBName.FirstName = UseOeWorkCenterFirstName1();

          if (AsChar(local.CaseRoleInactive.Flag) == 'Y')
          {
            export.CaseRoleInactive.Flag = local.CaseRoleInactive.Flag;
          }
        }
      }

      UseSiReadOfficeOspHeader();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        // *******************************************************
        // Must clear everything when case_nf
        // *******************************************************
        if (IsExitState("CASE_NF"))
        {
          ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";
        }

        return;
      }
      else
      {
        export.Next.Number = export.Case1.Number;
      }

      // ---------------------------------------------
      // Check if the Child Support Worksheet Identifier
      // has been brought over from the List Screen.
      // ---------------------------------------------
      if (export.ChildSupportWorksheet.Identifier == 0)
      {
        UseOeWorkReadChildSupWorksheet3();

        // -----------------------------------------------
        // The Child Support Worksheet was not found.
        // Clear up all protected screen fields.
        // -----------------------------------------------
        UseOeWorkCalcChildSupWorksheet2();

        if (export.ChildSupportWorksheet.CsGuidelineYear == 0)
        {
          export.ChildSupportWorksheet.CsGuidelineYear =
            local.CurrentGuidelineYear.Year;
        }

        ExitState = "OE0000_SELECT_WORKSHEET_FRM_CSWL";

        return;
      }
      else
      {
        // mjr---> Changed from importing Import CSW to importing Export CSW
        UseOeWorkReadChildSupWorksheet1();

        // -- Center first names
        export.ParentAName.FirstName = UseOeWorkCenterFirstName2();
        export.ParentBName.FirstName = UseOeWorkCenterFirstName1();
      }

      if (IsExitState("ACO_NI0000_SUCCESSFUL_DISPLAY"))
      {
        export.PrevChildSupportWorksheet.Assign(export.ChildSupportWorksheet);
        MoveChildSupportWorksheet2(export.ChildSupportWorksheet, export.Prev2);
        export.PrevCase.Number = export.Case1.Number;
        export.PrevParentACsePerson.Number = export.ParentACsePerson.Number;
        export.PrevParentBCsePerson.Number = export.ParentBCsePerson.Number;
        MoveCsePersonSupportWorksheet2(export.ParentACsePersonSupportWorksheet,
          export.PrevParentACsePersonSupportWorksheet);
        MoveCsePersonSupportWorksheet2(export.ParentBCsePersonSupportWorksheet,
          export.PrevParentBCsePersonSupportWorksheet);

        // mjr
        // -----------------------------------------------
        // 12/16/1998
        // Added check for an exitstate returned from Print
        // ------------------------------------------------------------
        // 08/15/2008	JHuss	Moved print code decoding to end of statement so 
        // that it's not
        // 			overwritten by calculate.
        local.Position.Count =
          Find(
            String(export.Hidden.MiscText2, NextTranInfo.MiscText2_MaxLength),
          TrimEnd(local.SpDocLiteral.IdDocument));

        if (local.Position.Count <= 0)
        {
          local.Display.Flag = "Y";
        }

        // 08/15/2008	JHuss	Set command to calculate in every instance so that 
        // totals are recalculated.
        global.Command = "CALCULAT";
        ExitState = "ACO_NN0000_ALL_OK";
      }
      else if (IsExitState("CASE_NF"))
      {
        // ***************************************************************
        // Must clear everything but the case number
        // ***************************************************************
        ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

        return;
      }
      else
      {
      }
    }

    if (Equal(global.Command, "CALCULAT"))
    {
      if (IsEmpty(export.Case1.Number))
      {
        var field = GetField(export.Case1, "number");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
      }
      else
      {
        UseOeCabCheckCaseMember4();

        if (AsChar(local.WorkError.Flag) == 'C')
        {
          ExitState = "CASE_NF";

          var field = GetField(export.Case1, "number");

          field.Error = true;

          return;
        }
      }

      if (IsEmpty(export.ParentACsePerson.Number) && IsEmpty
        (export.ParentBCsePerson.Number))
      {
        var field1 = GetField(export.ParentACsePerson, "number");

        field1.Error = true;

        var field2 = GetField(export.ParentBCsePerson, "number");

        field2.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
      }
      else
      {
        if (!IsEmpty(export.ParentACsePerson.Number))
        {
          UseOeCabCheckCaseMember3();

          // -- Center first name
          export.ParentAName.FirstName = UseOeWorkCenterFirstName2();

          if (AsChar(local.WorkError.Flag) == 'P')
          {
            ExitState = "CSE_PERSON_NF";

            var field = GetField(export.ParentACsePerson, "number");

            field.Error = true;

            export.ParentAName.FormattedName = "";
          }
          else if (AsChar(local.WorkError.Flag) == 'R')
          {
            ExitState = "OE0000_CASE_MEMBER_NE";

            var field1 = GetField(export.ParentACsePerson, "number");

            field1.Error = true;

            var field2 = GetField(export.Case1, "number");

            field2.Error = true;
          }
        }

        if (!IsEmpty(export.ParentBCsePerson.Number))
        {
          UseOeCabCheckCaseMember2();

          // -- Center first name
          export.ParentBName.FirstName = UseOeWorkCenterFirstName1();

          if (AsChar(local.WorkError.Flag) == 'P')
          {
            ExitState = "CSE_PERSON_NF";

            var field = GetField(export.ParentBCsePerson, "number");

            field.Error = true;

            export.ParentBName.FormattedName = "";
          }
          else if (AsChar(local.WorkError.Flag) == 'R')
          {
            ExitState = "OE0000_CASE_MEMBER_NE";

            var field1 = GetField(export.ParentBCsePerson, "number");

            field1.Error = true;

            var field2 = GetField(export.Case1, "number");

            field2.Error = true;
          }
        }
      }

      if (export.ChildSupportWorksheet.CsGuidelineYear == 0)
      {
        var field = GetField(export.ChildSupportWorksheet, "csGuidelineYear");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      if (!Equal(export.Case1.Number, export.PrevCase.Number) && !
        IsEmpty(export.PrevCase.Number))
      {
        var field = GetField(export.Case1, "number");

        field.Error = true;

        export.Case1.Number = export.PrevCase.Number;
        ExitState = "ACO_NE0000_NEW_KEY_ON_UPDATE";
      }

      // *** added a check for attempt to change guideline year, cannot change 
      // guideline year on an existing worksheet.
      // @@@ Change this so the worker can pull up a 2016 worksheet and then 
      // save it as a 2020 worksheet.
      if (export.PrevChildSupportWorksheet.CsGuidelineYear != export
        .ChildSupportWorksheet.CsGuidelineYear && export
        .PrevChildSupportWorksheet.CsGuidelineYear != 0)
      {
        var field = GetField(export.ChildSupportWorksheet, "csGuidelineYear");

        field.Color = "red";
        field.Protected = true;

        export.ChildSupportWorksheet.CsGuidelineYear =
          export.PrevChildSupportWorksheet.CsGuidelineYear;
        ExitState = "OE000_CANNOT_MODIFY_GUIDELINE_YR";
      }

      if (!IsEmpty(export.PrevParentACsePerson.Number) && !
        Equal(export.PrevParentACsePerson.Number, export.ParentACsePerson.Number))
        
      {
        var field = GetField(export.ParentACsePerson, "number");

        field.Error = true;

        export.ParentACsePerson.Number = export.PrevParentACsePerson.Number;
        ExitState = "OE0000_KEY_CHANGE_NA";
      }

      if (!IsEmpty(export.PrevParentBCsePerson.Number) && !
        Equal(export.PrevParentBCsePerson.Number, export.ParentBCsePerson.Number))
        
      {
        var field = GetField(export.ParentBCsePerson, "number");

        field.Error = true;

        export.ParentBCsePerson.Number = export.PrevParentBCsePerson.Number;
        ExitState = "OE0000_KEY_CHANGE_NA";
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      if (IsEmpty(export.ParentACsePerson.Number) && (
        export.ParentACsePersonSupportWorksheet.WageEarnerGrossIncome.
          GetValueOrDefault() != 0 || export
        .ParentACsePersonSupportWorksheet.SelfEmploymentGrossIncome.
          GetValueOrDefault() != 0))
      {
        var field = GetField(export.ParentACsePerson, "number");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
      }

      if (IsEmpty(export.ParentBCsePerson.Number) && (
        export.ParentBCsePersonSupportWorksheet.WageEarnerGrossIncome.
          GetValueOrDefault() != 0 || export
        .ParentBCsePersonSupportWorksheet.SelfEmploymentGrossIncome.
          GetValueOrDefault() != 0))
      {
        var field = GetField(export.ParentBCsePerson, "number");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
      }

      if (AsChar(export.ChildSupportWorksheet.MultipleFamilyAdjInd) == 'Y' && export
        .ChildSupportWorksheet.AdditionalNoOfChildren.GetValueOrDefault() == 0)
      {
        var field =
          GetField(export.ChildSupportWorksheet, "additionalNoOfChildren");

        field.Error = true;

        ExitState = "OE0000_NO_OF_ADD_CHILDREN_REQD2";
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      if (IsEmpty(export.ChildSupportWorksheet.Status))
      {
      }
      else if (AsChar(export.ChildSupportWorksheet.Status) != 'T' && AsChar
        (export.ChildSupportWorksheet.Status) != 'S' && AsChar
        (export.ChildSupportWorksheet.Status) != 'J')
      {
        var field = GetField(export.ChildSupportWorksheet, "status");

        field.Error = true;

        ExitState = "OE0000_INVALID_STATUS";
      }

      if (IsEmpty(export.ChildSupportWorksheet.MultipleFamilyAdjInd))
      {
        export.ChildSupportWorksheet.MultipleFamilyAdjInd = "N";
      }
      else if (AsChar(export.ChildSupportWorksheet.MultipleFamilyAdjInd) != 'Y'
        && AsChar(export.ChildSupportWorksheet.MultipleFamilyAdjInd) != 'N')
      {
        var field =
          GetField(export.ChildSupportWorksheet, "multipleFamilyAdjInd");

        field.Error = true;

        ExitState = "INVALID_SWITCH";
      }

      if (IsEmpty(export.ChildSupportWorksheet.MultipleFamilyAdjInd) || AsChar
        (export.ChildSupportWorksheet.MultipleFamilyAdjInd) == 'N')
      {
        if (export.ChildSupportWorksheet.AdditionalNoOfChildren.
          GetValueOrDefault() > 0)
        {
          var field =
            GetField(export.ChildSupportWorksheet, "multipleFamilyAdjInd");

          field.Error = true;

          ExitState = "OE0000_WORKSHEET_FAMILY_ADJ2";
        }
      }

      if (AsChar(export.ChildSupportWorksheet.MultipleFamilyAdjInd) == 'Y' && export
        .ChildSupportWorksheet.AdditionalNoOfChildren.GetValueOrDefault() == 0)
      {
        var field =
          GetField(export.ChildSupportWorksheet, "additionalNoOfChildren");

        field.Error = true;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "OE0000_NO_OF_ADD_CHILDREN_REQD2";
        }
      }

      // @@@ Changes below...
      // @@@ Changes above...
      if (IsEmpty(export.ChildSupportWorksheet.CostOfLivingDiffAdjInd))
      {
        export.ChildSupportWorksheet.CostOfLivingDiffAdjInd = "N";
      }
      else if (AsChar(export.ChildSupportWorksheet.CostOfLivingDiffAdjInd) != 'Y'
        && AsChar(export.ChildSupportWorksheet.CostOfLivingDiffAdjInd) != 'N')
      {
        var field =
          GetField(export.ChildSupportWorksheet, "costOfLivingDiffAdjInd");

        field.Error = true;

        ExitState = "INVALID_SWITCH";
      }

      if (export.ParentACsePersonSupportWorksheet.ReasonableBusinessExpense.
        GetValueOrDefault() > 0 && export
        .ParentACsePersonSupportWorksheet.SelfEmploymentGrossIncome.
          GetValueOrDefault() == 0)
      {
        var field1 =
          GetField(export.ParentACsePersonSupportWorksheet,
          "reasonableBusinessExpense");

        field1.Error = true;

        var field2 =
          GetField(export.ParentACsePersonSupportWorksheet,
          "selfEmploymentGrossIncome");

        field2.Error = true;

        ExitState = "OE0000_SELF_EMPL_GROSS_INCOM_REQ";
      }

      if (export.ParentBCsePersonSupportWorksheet.ReasonableBusinessExpense.
        GetValueOrDefault() > 0 && export
        .ParentBCsePersonSupportWorksheet.SelfEmploymentGrossIncome.
          GetValueOrDefault() == 0)
      {
        var field1 =
          GetField(export.ParentBCsePersonSupportWorksheet,
          "selfEmploymentGrossIncome");

        field1.Error = true;

        var field2 =
          GetField(export.ParentBCsePersonSupportWorksheet,
          "reasonableBusinessExpense");

        field2.Error = true;

        ExitState = "OE0000_SELF_EMPL_GROSS_INCOM_REQ";
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      UseOeWorkCalcChildSupWorksheet1();

      if (IsExitState("OE0000_CALCULATE_SUCCESSFUL"))
      {
        export.PrevCase.Number = export.Case1.Number;
        export.PrevChildSupportWorksheet.Assign(export.ChildSupportWorksheet);
        export.PrevParentACsePerson.Number = export.ParentACsePerson.Number;
        export.PrevParentBCsePerson.Number = export.ParentBCsePerson.Number;
        MoveCsePersonSupportWorksheet2(export.ParentACsePersonSupportWorksheet,
          export.PrevParentACsePersonSupportWorksheet);
        MoveCsePersonSupportWorksheet2(export.ParentBCsePersonSupportWorksheet,
          export.PrevParentBCsePersonSupportWorksheet);

        if (AsChar(export.CaseOpen.Flag) == 'N')
        {
          ExitState = "DISPLAY_OK_FOR_CLOSED_CASE";
        }

        // *****************************************************************
        // If the user Press PF2, it should display a message "Displayed 
        // Successfully"
        // *****************************************************************
        if (AsChar(local.Display.Flag) == 'Y')
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }
        else
        {
          export.CommandH.Command = global.Command;

          // mjr---> Determines the appropriate exitstate for the Print process
          // 08/15/2008	JHuss	Moved print code decoding to end of statement so 
          // that it's not
          // 			overwritten by calculate.
          local.WorkArea.Text50 = export.Hidden.MiscText2 ?? Spaces(50);
          UseSpPrintDecodeReturnCode();
          export.Hidden.MiscText2 = local.WorkArea.Text50;
        }
      }
      else if (IsExitState("OE0000_CALCULATE_UNSUCCESSFUL"))
      {
        if (!IsEmpty(import.ParentACsePerson.Number))
        {
          var field =
            GetField(export.ParentACsePersonSupportWorksheet,
            "wageEarnerGrossIncome");

          field.Error = true;
        }
        else if (!IsEmpty(import.ParentBCsePerson.Number))
        {
          var field =
            GetField(export.ParentBCsePersonSupportWorksheet,
            "wageEarnerGrossIncome");

          field.Error = true;
        }
      }
      else
      {
      }
    }
  }

  private static void MoveChildSupportWorksheet1(ChildSupportWorksheet source,
    ChildSupportWorksheet target)
  {
    target.NoOfChildrenInAgeGrp3 = source.NoOfChildrenInAgeGrp3;
    target.NoOfChildrenInAgeGrp2 = source.NoOfChildrenInAgeGrp2;
    target.NoOfChildrenInAgeGrp1 = source.NoOfChildrenInAgeGrp1;
    target.AdditionalNoOfChildren = source.AdditionalNoOfChildren;
    target.Status = source.Status;
    target.CostOfLivingDiffAdjInd = source.CostOfLivingDiffAdjInd;
    target.Identifier = source.Identifier;
    target.MultipleFamilyAdjInd = source.MultipleFamilyAdjInd;
    target.AuthorizingAuthority = source.AuthorizingAuthority;
    target.CsGuidelineYear = source.CsGuidelineYear;
  }

  private static void MoveChildSupportWorksheet2(ChildSupportWorksheet source,
    ChildSupportWorksheet target)
  {
    target.Status = source.Status;
    target.Identifier = source.Identifier;
    target.CsGuidelineYear = source.CsGuidelineYear;
  }

  private static void MoveCsePersonSupportWorksheet1(
    CsePersonSupportWorksheet source, CsePersonSupportWorksheet target)
  {
    target.NoOfChildrenInDayCare = source.NoOfChildrenInDayCare;
    target.WorkRelatedChildCareCosts = source.WorkRelatedChildCareCosts;
    target.Identifer = source.Identifer;
    target.WageEarnerGrossIncome = source.WageEarnerGrossIncome;
    target.SelfEmploymentGrossIncome = source.SelfEmploymentGrossIncome;
    target.ReasonableBusinessExpense = source.ReasonableBusinessExpense;
    target.CourtOrderedChildSupportPaid = source.CourtOrderedChildSupportPaid;
    target.ChildSupprtPaidCourtOrderNo = source.ChildSupprtPaidCourtOrderNo;
    target.CourtOrderedMaintenancePaid = source.CourtOrderedMaintenancePaid;
    target.MaintenancePaidCourtOrderNo = source.MaintenancePaidCourtOrderNo;
    target.CourtOrderedMaintenanceRecvd = source.CourtOrderedMaintenanceRecvd;
    target.MaintenanceRecvdCourtOrderNo = source.MaintenanceRecvdCourtOrderNo;
    target.HealthAndDentalInsurancePrem = source.HealthAndDentalInsurancePrem;
    target.EligibleForFederalTaxCredit = source.EligibleForFederalTaxCredit;
    target.EligibleForKansasTaxCredit = source.EligibleForKansasTaxCredit;
    target.NetAdjParentalChildSuppAmt = source.NetAdjParentalChildSuppAmt;
    target.InsuranceWorkRelatedCcCredit = source.InsuranceWorkRelatedCcCredit;
    target.AbilityToPay = source.AbilityToPay;
    target.EqualParentingTimeObligation = source.EqualParentingTimeObligation;
    target.SocialSecDependentBenefit = source.SocialSecDependentBenefit;
  }

  private static void MoveCsePersonSupportWorksheet2(
    CsePersonSupportWorksheet source, CsePersonSupportWorksheet target)
  {
    target.WageEarnerGrossIncome = source.WageEarnerGrossIncome;
    target.SelfEmploymentGrossIncome = source.SelfEmploymentGrossIncome;
    target.ReasonableBusinessExpense = source.ReasonableBusinessExpense;
    target.CourtOrderedChildSupportPaid = source.CourtOrderedChildSupportPaid;
    target.ChildSupprtPaidCourtOrderNo = source.ChildSupprtPaidCourtOrderNo;
    target.CourtOrderedMaintenancePaid = source.CourtOrderedMaintenancePaid;
    target.MaintenancePaidCourtOrderNo = source.MaintenancePaidCourtOrderNo;
    target.CourtOrderedMaintenanceRecvd = source.CourtOrderedMaintenanceRecvd;
    target.MaintenanceRecvdCourtOrderNo = source.MaintenanceRecvdCourtOrderNo;
  }

  private static void MoveExport1(OeWorkReadChildSupWorksheet.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    target.Work.SelectChar = source.Work.SelectChar;
    target.ParentB.AdjustmentAmount = source.ParentB.AdjustmentAmount;
    target.ParentA.AdjustmentAmount = source.ParentA.AdjustmentAmount;
  }

  private static void MoveExport1ToImport2(Export.ExportGroup source,
    OeWorkCalcChildSupWorksheet.Import.ImportGroup target)
  {
    target.Work.SelectChar = source.Work.SelectChar;
    target.ParentB.AdjustmentAmount = source.ParentB.AdjustmentAmount;
    target.ParentA.AdjustmentAmount = source.ParentA.AdjustmentAmount;
  }

  private static void MoveExport1ToImport3(Export.ExportGroup source,
    OeWorkAddChildSupWorksheet.Import.ImportGroup target)
  {
    System.Diagnostics.Trace.TraceWarning(
      "INFO: source and target of the move fit weakly.");
    target.ParentB.AdjustmentAmount = source.ParentB.AdjustmentAmount;
    target.ParentA.AdjustmentAmount = source.ParentA.AdjustmentAmount;
  }

  private static void MoveGexport2020Calculations(OeWorkCalcChildSupWorksheet.
    Export.Gexport2020CalculationsGroup source, Export.Gexport2020Group target)
  {
    target.GexportDParentTimeAdjFlag.SelectChar =
      source.GexportDParentTimeAdjFlag.SelectChar;
    target.GexportD4ParAPropShare.TotalCurrency =
      source.GexportD4ParAPropShare.TotalCurrency;
    target.GexportD4ParBPropShare.TotalCurrency =
      source.GexportD4ParBPropShare.TotalCurrency;
    target.GexportD4TotalPropShare.TotalCurrency =
      source.GexportD4TotalPropShare.TotalCurrency;
    target.GexportD5ParAParentTimAdj.TotalCurrency =
      source.GexportD5ParAParentTimAdj.TotalCurrency;
    target.GexportD5ParBParentTimAdj.TotalCurrency =
      source.GexportD5ParBParentTimAdj.TotalCurrency;
    target.GexportD5TotalParentTimAdj.TotalCurrency =
      source.GexportD5TotalParentTimAdj.TotalCurrency;
    target.GexportD6ParAPsAfterPat.TotalCurrency =
      source.GexportD6ParAPsAfterPat.TotalCurrency;
    target.GexportD6ParBPsAfterPat.TotalCurrency =
      source.GexportD6ParBPsAfterPat.TotalCurrency;
    target.GexportD6TotalPsAfterPat.TotalCurrency =
      source.GexportD6TotalPsAfterPat.TotalCurrency;
    target.GexportD8ParAPropShrHip.TotalCurrency =
      source.GexportD8ParAPropShrHip.TotalCurrency;
    target.GexportD8ParBPropShrHip.TotalCurrency =
      source.GexportD8ParBPropShrHip.TotalCurrency;
    target.GexportD8TotalPropShrHip.TotalCurrency =
      source.GexportD8TotalPropShrHip.TotalCurrency;
    target.GexportD10ParAPropShrWrcc.TotalCurrency =
      source.GexportD10ParAPropShrWrcc.TotalCurrency;
    target.GexportD10ParBPropShrWrcc.TotalCurrency =
      source.GexportD10ParBPropShrWrcc.TotalCurrency;
    target.GexportD10TotalPropShrWrcc.TotalCurrency =
      source.GexportD10TotalPropShrWrcc.TotalCurrency;
    target.GexportD11ParAPropShrCcob.TotalCurrency =
      source.GexportD11ParAPropShrCcob.TotalCurrency;
    target.GexportD11ParBPropShrCcob.TotalCurrency =
      source.GexportD11ParBPropShrCcob.TotalCurrency;
    target.GexportD11TotalPropShrCcob.TotalCurrency =
      source.GexportD11TotalPropShrCcob.TotalCurrency;
    target.GexportD12TotalInsWrccPaid.TotalCurrency =
      source.GexportD12TotalInsWrccPaid.TotalCurrency;
    target.GexportD13ParABasicChSup.TotalCurrency =
      source.GexportD13ParABasicChSup.TotalCurrency;
    target.GexportD13ParBBasicChSup.TotalCurrency =
      source.GexportD13ParBBasicChSup.TotalCurrency;

    target.GexportF5A0Parent.SelectChar = source.GexportF5A0Parent.SelectChar;
    target.GexportF5A1CsIncome.TotalCurrency =
      source.GexportF5A1CsIncome.TotalCurrency;
    target.GexportF5A2PovertyLevel.TotalCurrency =
      source.GexportF5A2PovertyLevel.TotalCurrency;
    target.GexportF5A3AbilityToPay.TotalCurrency =
      source.GexportF5A3AbilityToPay.TotalCurrency;
    target.GexportF5BParASubtotal.TotalCurrency =
      source.GexportF5BParASubtotal.TotalCurrency;
    target.GexportF5BParBSubtotal.TotalCurrency =
      source.GexportF5BParBSubtotal.TotalCurrency;
    target.GexportF6BParAFinaSubtotal.TotalCurrency =
      source.GexportF6BParAFinaSubtotal.TotalCurrency;
    target.GexportF6BParBFinaSubtotal.TotalCurrency =
      source.GexportF6BParBFinaSubtotal.TotalCurrency;
    target.GexportF8ParANetCsOblig.TotalCurrency =
      source.GexportF8ParANetCsOblig.TotalCurrency;
    target.GexportF8ParBNetCsOblig.TotalCurrency =
      source.GexportF8ParBNetCsOblig.TotalCurrency;
  }

  private static void MoveImport1(Import.ImportGroup source,
    OeWorkUpdtChildSupWorksheet.Import.ImportGroup target)
  {
    target.Work.SelectChar = source.Work.SelectChar;
    target.ParentB.AdjustmentAmount = source.ParentB.AdjustmentAmount;
    target.ParentA.AdjustmentAmount = source.ParentA.AdjustmentAmount;
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
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
    target.IdDocument = source.IdDocument;
    target.IdWorksheet = source.IdWorksheet;
  }

  private static void MoveStandard(Standard source, Standard target)
  {
    target.Command = source.Command;
    target.DeleteConfirmation = source.DeleteConfirmation;
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

  private void UseOeCabCheckCaseMember1()
  {
    var useImport = new OeCabCheckCaseMember.Import();
    var useExport = new OeCabCheckCaseMember.Export();

    useImport.CsePerson.Number = export.ParentBCsePerson.Number;
    useImport.Case1.Number = export.Case1.Number;

    Call(OeCabCheckCaseMember.Execute, useImport, useExport);

    local.CaseRoleInactive.Flag = useExport.CaseRoleInactive.Flag;
    export.ParentBName.Assign(useExport.CsePersonsWorkSet);
    export.CaseOpen.Flag = useExport.CaseOpen.Flag;
  }

  private void UseOeCabCheckCaseMember2()
  {
    var useImport = new OeCabCheckCaseMember.Import();
    var useExport = new OeCabCheckCaseMember.Export();

    useImport.CsePerson.Number = export.ParentBCsePerson.Number;
    useImport.Case1.Number = export.Case1.Number;

    Call(OeCabCheckCaseMember.Execute, useImport, useExport);

    local.WorkError.Flag = useExport.Work.Flag;
    export.ParentBName.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseOeCabCheckCaseMember3()
  {
    var useImport = new OeCabCheckCaseMember.Import();
    var useExport = new OeCabCheckCaseMember.Export();

    useImport.CsePerson.Number = export.ParentACsePerson.Number;
    useImport.Case1.Number = export.Case1.Number;

    Call(OeCabCheckCaseMember.Execute, useImport, useExport);

    local.WorkError.Flag = useExport.Work.Flag;
    export.ParentAName.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseOeCabCheckCaseMember4()
  {
    var useImport = new OeCabCheckCaseMember.Import();
    var useExport = new OeCabCheckCaseMember.Export();

    useImport.Case1.Number = export.Case1.Number;

    Call(OeCabCheckCaseMember.Execute, useImport, useExport);

    local.WorkError.Flag = useExport.Work.Flag;
  }

  private void UseOeCabCheckCaseMember5()
  {
    var useImport = new OeCabCheckCaseMember.Import();
    var useExport = new OeCabCheckCaseMember.Export();

    useImport.CsePerson.Number = export.ParentBCsePerson.Number;
    useImport.Case1.Number = export.Case1.Number;

    Call(OeCabCheckCaseMember.Execute, useImport, useExport);

    local.Error.Flag = useExport.Work.Flag;
    export.ParentBName.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseOeCabCheckCaseMember6()
  {
    var useImport = new OeCabCheckCaseMember.Import();
    var useExport = new OeCabCheckCaseMember.Export();

    useImport.CsePerson.Number = export.ParentACsePerson.Number;
    useImport.Case1.Number = export.Case1.Number;

    Call(OeCabCheckCaseMember.Execute, useImport, useExport);

    local.Error.Flag = useExport.Work.Flag;
    export.ParentAName.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseOeCabCheckCaseMember7()
  {
    var useImport = new OeCabCheckCaseMember.Import();
    var useExport = new OeCabCheckCaseMember.Export();

    useImport.Case1.Number = export.Case1.Number;

    Call(OeCabCheckCaseMember.Execute, useImport, useExport);

    local.Error.Flag = useExport.Work.Flag;
  }

  private void UseOeCabCheckCaseMember8()
  {
    var useImport = new OeCabCheckCaseMember.Import();
    var useExport = new OeCabCheckCaseMember.Export();

    useImport.CsePerson.Number = export.ParentACsePerson.Number;
    useImport.Case1.Number = export.Case1.Number;

    Call(OeCabCheckCaseMember.Execute, useImport, useExport);

    export.ParentAName.Assign(useExport.CsePersonsWorkSet);
    export.CaseOpen.Flag = useExport.CaseOpen.Flag;
    export.CaseRoleInactive.Flag = useExport.CaseRoleInactive.Flag;
  }

  private void UseOeWorkAddChildSupWorksheet()
  {
    var useImport = new OeWorkAddChildSupWorksheet.Import();
    var useExport = new OeWorkAddChildSupWorksheet.Export();

    MoveLegalAction(export.LegalAction, useImport.LegalAction);
    useImport.ParentBCsePerson.Number = export.ParentBCsePerson.Number;
    useImport.ParentBCsePersonSupportWorksheet.Assign(
      export.ParentBCsePersonSupportWorksheet);
    useImport.Case1.Number = export.Case1.Number;
    useImport.ParentACsePerson.Number = export.ParentACsePerson.Number;
    useImport.ParentACsePersonSupportWorksheet.Assign(
      export.ParentACsePersonSupportWorksheet);
    useImport.ChildSupportWorksheet.Assign(export.ChildSupportWorksheet);
    export.Export1.CopyTo(useImport.Import1, MoveExport1ToImport3);
    useImport.Gimport2020EnterableFields.GimportParentingTime.Assign(
      export.Gexport2020EnterableFields.GexportParentingTime);
    useImport.Gimport2020EnterableFields.GimportAbilityToPayParent.SelectChar =
      export.Gexport2020EnterableFields.GexportAbilityToPayParent.SelectChar;

    Call(OeWorkAddChildSupWorksheet.Execute, useImport, useExport);

    export.ParentBCsePersonSupportWorksheet.Assign(useExport.ParentB);
    export.ParentACsePersonSupportWorksheet.Assign(useExport.ParentA);
    export.ChildSupportWorksheet.Assign(useExport.ChildSupportWorksheet);
  }

  private void UseOeWorkCalcChildSupWorksheet1()
  {
    var useImport = new OeWorkCalcChildSupWorksheet.Import();
    var useExport = new OeWorkCalcChildSupWorksheet.Export();

    useImport.Common.Assign(export.Common);
    export.Export1.CopyTo(useImport.Import1, MoveExport1ToImport2);
    useImport.ChildSupportWorksheet.Assign(export.ChildSupportWorksheet);
    useImport.ParentB.Assign(export.ParentBCsePersonSupportWorksheet);
    useImport.ParentA.Assign(export.ParentACsePersonSupportWorksheet);
    useImport.Gimport2020EnterableFields.GimportParentingTime.Assign(
      export.Gexport2020EnterableFields.GexportParentingTime);
    useImport.Gimport2020EnterableFields.GimportAbilityToPayParent.SelectChar =
      export.Gexport2020EnterableFields.GexportAbilityToPayParent.SelectChar;

    Call(OeWorkCalcChildSupWorksheet.Execute, useImport, useExport);

    export.ParentBF2TotalCsAdj.TotalCurrency =
      useExport.ParentBF2TotalCsAdj.TotalCurrency;
    export.ParentAF2TotalCsAdj.TotalCurrency =
      useExport.ParentAF2TotalCsAdj.TotalCurrency;
    export.D1otalCsInc.TotalCurrency = useExport.D1otalCsInc.TotalCurrency;
    export.ParentBF3AdjCsOblig.TotalCurrency =
      useExport.ParentBF3AdjCsOblig.TotalCurrency;
    export.ParentBE7F2TotAdj.TotalCurrency =
      useExport.ParentBE7F2TotAdj.TotalCurrency;
    export.ParentAF3AdjCsOblig.TotalCurrency =
      useExport.ParentAF3AdjCsOblig.TotalCurrency;
    export.ParentAE7F2TotAdj.TotalCurrency =
      useExport.ParentAE7F2TotAdj.TotalCurrency;
    export.ParentAD9F1NetCs.TotalCurrency =
      useExport.ParentAD9F1NetCs.TotalCurrency;
    export.ParentBD9F1NetCs.TotalCurrency =
      useExport.ParentBD9F1NetCs.TotalCurrency;
    export.ParentBD8Adjustments.TotalCurrency =
      useExport.ParentBD8Adjustments.TotalCurrency;
    export.ParentAD8Adjustments.TotalCurrency =
      useExport.ParentAD8Adjustments.TotalCurrency;
    export.ParentBD7CsOblig.TotalCurrency =
      useExport.ParentBD7CsOblig.TotalCurrency;
    export.ParentAD7CsOblig.TotalCurrency =
      useExport.ParentAD7CsOblig.TotalCurrency;
    export.D6otalChildSuppOblig.TotalCurrency =
      useExport.D6otalChildSuppOblig.TotalCurrency;
    export.TotalChildCareCost.TotalCurrency =
      useExport.TotalChildCareCost.TotalCurrency;
    export.ParentAChildCareCost.TotalCurrency =
      useExport.ParentAChildCareCost.TotalCurrency;
    export.ParentBChildCareCost.TotalCurrency =
      useExport.ParentBChildCareCost.TotalCurrency;
    export.ParentBTotalTaxCredit.TotalCurrency =
      useExport.ParentBTotalTaxCredit.TotalCurrency;
    export.ParentATotalTaxCredit.TotalCurrency =
      useExport.ParentATotalTaxCredit.TotalCurrency;
    export.TotalInsurancePrem.TotalCurrency =
      useExport.TotalInsurancePrem.TotalCurrency;
    export.CsObligTotalAmount.TotalCurrency =
      useExport.CsObligTotalAmount.TotalCurrency;
    export.CsOblig1618TotalAmt.TotalCurrency =
      useExport.CsOblig1618TotalAmt.TotalCurrency;
    export.CsOblig715TotalAmt.TotalCurrency =
      useExport.CsOblig715TotalAmt.TotalCurrency;
    export.CsOblig06TotalAmt.TotalCurrency =
      useExport.CsOblig06TotalAmt.TotalCurrency;
    export.ParentBD2PercentInc.TotalCurrency =
      useExport.ParentBD2PercentInc.TotalCurrency;
    export.ParentAD2PercentInc.TotalCurrency =
      useExport.ParentAD2PercentInc.TotalCurrency;
    export.ParentBC5D1TotCsInc.TotalCurrency =
      useExport.ParentBC5D1TotCsInc.TotalCurrency;
    export.ParentAC5D1TotCsInc.TotalCurrency =
      useExport.ParentAC5D1TotCsInc.TotalCurrency;
    export.ParentBC1TotGrossInc.TotalCurrency =
      useExport.ParentBC1TotGrossInc.TotalCurrency;
    export.ParentAC1TotGrossInc.TotalCurrency =
      useExport.ParentAC1TotGrossInc.TotalCurrency;
    export.ParentBB3SeGrossInc.TotalCurrency =
      useExport.ParentBB3SeGrossInc.TotalCurrency;
    export.ParentAB3SeGrossInc.TotalCurrency =
      useExport.ParentAB3SeGrossInc.TotalCurrency;
    MoveChildSupportWorksheet1(useExport.ChildSupportWorksheet,
      export.ChildSupportWorksheet);
    export.ParentBCsePersonSupportWorksheet.Assign(useExport.ParentB);
    export.ParentACsePersonSupportWorksheet.Assign(useExport.ParentA);
    export.ParentAD10F1NetCs.TotalCurrency =
      useExport.ParentAD10F1NetCs.TotalCurrency;
    export.ParentBD10F1NetCs.TotalCurrency =
      useExport.ParentBD10F1NetCs.TotalCurrency;
    export.ParentAEnfFee.TotalCurrency = useExport.ParentAEnfFee.TotalCurrency;
    export.ParentBEnfFee.TotalCurrency = useExport.ParentBEnfFee.TotalCurrency;
    MoveGexport2020Calculations(useExport.Gexport2020Calculations,
      export.Gexport2020);
  }

  private void UseOeWorkCalcChildSupWorksheet2()
  {
    var useImport = new OeWorkCalcChildSupWorksheet.Import();
    var useExport = new OeWorkCalcChildSupWorksheet.Export();

    Call(OeWorkCalcChildSupWorksheet.Execute, useImport, useExport);

    export.ParentBF2TotalCsAdj.TotalCurrency =
      useExport.ParentBF2TotalCsAdj.TotalCurrency;
    export.ParentAF2TotalCsAdj.TotalCurrency =
      useExport.ParentAF2TotalCsAdj.TotalCurrency;
    export.D1otalCsInc.TotalCurrency = useExport.D1otalCsInc.TotalCurrency;
    export.ParentBF3AdjCsOblig.TotalCurrency =
      useExport.ParentBF3AdjCsOblig.TotalCurrency;
    export.ParentBE7F2TotAdj.TotalCurrency =
      useExport.ParentBE7F2TotAdj.TotalCurrency;
    export.ParentAF3AdjCsOblig.TotalCurrency =
      useExport.ParentAF3AdjCsOblig.TotalCurrency;
    export.ParentAE7F2TotAdj.TotalCurrency =
      useExport.ParentAE7F2TotAdj.TotalCurrency;
    export.ParentAD9F1NetCs.TotalCurrency =
      useExport.ParentAD9F1NetCs.TotalCurrency;
    export.ParentBD9F1NetCs.TotalCurrency =
      useExport.ParentBD9F1NetCs.TotalCurrency;
    export.ParentBD8Adjustments.TotalCurrency =
      useExport.ParentBD8Adjustments.TotalCurrency;
    export.ParentAD8Adjustments.TotalCurrency =
      useExport.ParentAD8Adjustments.TotalCurrency;
    export.ParentBD7CsOblig.TotalCurrency =
      useExport.ParentBD7CsOblig.TotalCurrency;
    export.ParentAD7CsOblig.TotalCurrency =
      useExport.ParentAD7CsOblig.TotalCurrency;
    export.D6otalChildSuppOblig.TotalCurrency =
      useExport.D6otalChildSuppOblig.TotalCurrency;
    export.TotalChildCareCost.TotalCurrency =
      useExport.TotalChildCareCost.TotalCurrency;
    export.ParentAChildCareCost.TotalCurrency =
      useExport.ParentAChildCareCost.TotalCurrency;
    export.ParentBChildCareCost.TotalCurrency =
      useExport.ParentBChildCareCost.TotalCurrency;
    export.ParentBTotalTaxCredit.TotalCurrency =
      useExport.ParentBTotalTaxCredit.TotalCurrency;
    export.ParentATotalTaxCredit.TotalCurrency =
      useExport.ParentATotalTaxCredit.TotalCurrency;
    export.TotalInsurancePrem.TotalCurrency =
      useExport.TotalInsurancePrem.TotalCurrency;
    export.CsObligTotalAmount.TotalCurrency =
      useExport.CsObligTotalAmount.TotalCurrency;
    export.CsOblig1618TotalAmt.TotalCurrency =
      useExport.CsOblig1618TotalAmt.TotalCurrency;
    export.CsOblig715TotalAmt.TotalCurrency =
      useExport.CsOblig715TotalAmt.TotalCurrency;
    export.CsOblig06TotalAmt.TotalCurrency =
      useExport.CsOblig06TotalAmt.TotalCurrency;
    export.ParentBD2PercentInc.TotalCurrency =
      useExport.ParentBD2PercentInc.TotalCurrency;
    export.ParentAD2PercentInc.TotalCurrency =
      useExport.ParentAD2PercentInc.TotalCurrency;
    export.ParentBC5D1TotCsInc.TotalCurrency =
      useExport.ParentBC5D1TotCsInc.TotalCurrency;
    export.ParentAC5D1TotCsInc.TotalCurrency =
      useExport.ParentAC5D1TotCsInc.TotalCurrency;
    export.ParentBC1TotGrossInc.TotalCurrency =
      useExport.ParentBC1TotGrossInc.TotalCurrency;
    export.ParentAC1TotGrossInc.TotalCurrency =
      useExport.ParentAC1TotGrossInc.TotalCurrency;
    export.ParentBB3SeGrossInc.TotalCurrency =
      useExport.ParentBB3SeGrossInc.TotalCurrency;
    export.ParentAB3SeGrossInc.TotalCurrency =
      useExport.ParentAB3SeGrossInc.TotalCurrency;
    MoveChildSupportWorksheet1(useExport.ChildSupportWorksheet,
      export.ChildSupportWorksheet);
    export.ParentBCsePersonSupportWorksheet.Assign(useExport.ParentB);
    export.ParentACsePersonSupportWorksheet.Assign(useExport.ParentA);
    MoveGexport2020Calculations(useExport.Gexport2020Calculations,
      export.Gexport2020);
  }

  private string UseOeWorkCenterFirstName1()
  {
    var useImport = new OeWorkCenterFirstName.Import();
    var useExport = new OeWorkCenterFirstName.Export();

    useImport.CsePersonsWorkSet.FirstName = export.ParentBName.FirstName;

    Call(OeWorkCenterFirstName.Execute, useImport, useExport);

    return useExport.CsePersonsWorkSet.FirstName;
  }

  private string UseOeWorkCenterFirstName2()
  {
    var useImport = new OeWorkCenterFirstName.Import();
    var useExport = new OeWorkCenterFirstName.Export();

    useImport.CsePersonsWorkSet.FirstName = export.ParentAName.FirstName;

    Call(OeWorkCenterFirstName.Execute, useImport, useExport);

    return useExport.CsePersonsWorkSet.FirstName;
  }

  private void UseOeWorkDelChildSupWorksheet()
  {
    var useImport = new OeWorkDelChildSupWorksheet.Import();
    var useExport = new OeWorkDelChildSupWorksheet.Export();

    useImport.ChildSupportWorksheet.Identifier =
      import.ChildSupportWorksheet.Identifier;

    Call(OeWorkDelChildSupWorksheet.Execute, useImport, useExport);
  }

  private void UseOeWorkReadChildSupWorksheet1()
  {
    var useImport = new OeWorkReadChildSupWorksheet.Import();
    var useExport = new OeWorkReadChildSupWorksheet.Export();

    useImport.ChildSupportWorksheet.Identifier =
      export.ChildSupportWorksheet.Identifier;

    Call(OeWorkReadChildSupWorksheet.Execute, useImport, useExport);

    export.Common.Assign(useExport.Common);
    export.H.County = useExport.FipsTribAddress.County;
    export.County.Description = useExport.County.Description;
    useExport.Export1.CopyTo(export.Export1, MoveExport1);
    export.ChildSupportWorksheet.Assign(useExport.ChildSupportWorksheet);
    MoveCsePersonSupportWorksheet1(useExport.ParentBCsePersonSupportWorksheet,
      export.ParentBCsePersonSupportWorksheet);
    MoveCsePersonSupportWorksheet1(useExport.ParentACsePersonSupportWorksheet,
      export.ParentACsePersonSupportWorksheet);
    export.ParentBCsePerson.Number = useExport.ParentBCsePerson.Number;
    export.ParentACsePerson.Number = useExport.ParentACsePerson.Number;
    export.Case1.Number = useExport.Case1.Number;
    export.Tribunal.JudicialDistrict = useExport.Tribunal.JudicialDistrict;
    MoveLegalAction(useExport.LegalAction, export.LegalAction);
    export.ParentAName.Assign(useExport.ParentAName);
    export.ParentBName.Assign(useExport.ParentBName);
    export.Gexport2020EnterableFields.GexportParentingTime.Assign(
      useExport.Gexport2020EnterableFields.GexportParentingTime);
    export.Gexport2020EnterableFields.GexportAbilityToPayParent.SelectChar =
      useExport.Gexport2020EnterableFields.GexportAbilityToPayParent.SelectChar;
      
  }

  private void UseOeWorkReadChildSupWorksheet2()
  {
    var useImport = new OeWorkReadChildSupWorksheet.Import();
    var useExport = new OeWorkReadChildSupWorksheet.Export();

    Call(OeWorkReadChildSupWorksheet.Execute, useImport, useExport);

    export.Common.Assign(useExport.Common);
    export.H.County = useExport.FipsTribAddress.County;
    useExport.Export1.CopyTo(export.Export1, MoveExport1);
    export.ChildSupportWorksheet.Assign(useExport.ChildSupportWorksheet);
    MoveCsePersonSupportWorksheet1(useExport.ParentBCsePersonSupportWorksheet,
      export.ParentBCsePersonSupportWorksheet);
    MoveCsePersonSupportWorksheet1(useExport.ParentACsePersonSupportWorksheet,
      export.ParentACsePersonSupportWorksheet);
    export.PrevParentBCsePerson.Number = useExport.PrevParentB.Number;
    export.PrevParentACsePerson.Number = useExport.PrevParentA.Number;
    export.PrevCase.Number = useExport.Prev.Number;
    export.Gexport2020EnterableFields.GexportParentingTime.Assign(
      useExport.Gexport2020EnterableFields.GexportParentingTime);
    export.Gexport2020EnterableFields.GexportAbilityToPayParent.SelectChar =
      useExport.Gexport2020EnterableFields.GexportAbilityToPayParent.SelectChar;
      
  }

  private void UseOeWorkReadChildSupWorksheet3()
  {
    var useImport = new OeWorkReadChildSupWorksheet.Import();
    var useExport = new OeWorkReadChildSupWorksheet.Export();

    Call(OeWorkReadChildSupWorksheet.Execute, useImport, useExport);

    export.Common.Assign(useExport.Common);
    useExport.Export1.CopyTo(export.Export1, MoveExport1);
    export.ChildSupportWorksheet.Assign(useExport.ChildSupportWorksheet);
    MoveCsePersonSupportWorksheet1(useExport.ParentBCsePersonSupportWorksheet,
      export.ParentBCsePersonSupportWorksheet);
    MoveCsePersonSupportWorksheet1(useExport.ParentACsePersonSupportWorksheet,
      export.ParentACsePersonSupportWorksheet);
    export.PrevParentBCsePerson.Number = useExport.PrevParentB.Number;
    export.PrevParentACsePerson.Number = useExport.PrevParentA.Number;
    export.PrevCase.Number = useExport.Prev.Number;
    export.Gexport2020EnterableFields.GexportParentingTime.Assign(
      useExport.Gexport2020EnterableFields.GexportParentingTime);
    export.Gexport2020EnterableFields.GexportAbilityToPayParent.SelectChar =
      useExport.Gexport2020EnterableFields.GexportAbilityToPayParent.SelectChar;
      
  }

  private void UseOeWorkReadCourtOrderDetails1()
  {
    var useImport = new OeWorkReadCourtOrderDetails.Import();
    var useExport = new OeWorkReadCourtOrderDetails.Export();

    useImport.ParentB.Number = import.ParentBCsePerson.Number;
    useImport.ParentA.Number = import.ParentACsePerson.Number;
    useImport.LegalAction.Identifier = import.SelectedLegalAction.Identifier;

    Call(OeWorkReadCourtOrderDetails.Execute, useImport, useExport);

    export.H.County = useExport.FipsTribAddress.County;
    export.County.Description = useExport.County.Description;
    export.Tribunal.JudicialDistrict = useExport.Tribunal.JudicialDistrict;
    MoveLegalAction(useExport.LegalAction, export.LegalAction);
  }

  private void UseOeWorkReadCourtOrderDetails2()
  {
    var useImport = new OeWorkReadCourtOrderDetails.Import();
    var useExport = new OeWorkReadCourtOrderDetails.Export();

    useImport.ParentB.Number = import.ParentBCsePerson.Number;
    useImport.ParentA.Number = import.ParentACsePerson.Number;
    useImport.LegalAction.Identifier = export.LegalAction.Identifier;

    Call(OeWorkReadCourtOrderDetails.Execute, useImport, useExport);
  }

  private void UseOeWorkUpdtChildSupWorksheet()
  {
    var useImport = new OeWorkUpdtChildSupWorksheet.Import();
    var useExport = new OeWorkUpdtChildSupWorksheet.Export();

    MoveLegalAction(import.LegalAction, useImport.LegalAction);
    useImport.ParentBCsePerson.Number = export.ParentBCsePerson.Number;
    useImport.ParentBCsePersonSupportWorksheet.Assign(
      import.ParentBCsePersonSupportWorksheet);
    useImport.Case1.Number = export.Case1.Number;
    useImport.ParentACsePerson.Number = export.ParentACsePerson.Number;
    useImport.ParentACsePersonSupportWorksheet.Assign(
      import.ParentACsePersonSupportWorksheet);
    useImport.ChildSupportWorksheet.Assign(import.ChildSupportWorksheet);
    import.Import1.CopyTo(useImport.Import1, MoveImport1);
    useImport.Gimport2020EnterableFields.GimportParentingTime.Assign(
      export.Gexport2020EnterableFields.GexportParentingTime);
    useImport.Gimport2020EnterableFields.GimportAbilityToPayParent.SelectChar =
      export.Gexport2020EnterableFields.GexportAbilityToPayParent.SelectChar;

    Call(OeWorkUpdtChildSupWorksheet.Execute, useImport, useExport);

    export.ParentBCsePersonSupportWorksheet.Assign(useExport.ParentB);
    export.ParentACsePersonSupportWorksheet.Assign(useExport.ParentA);
    export.ChildSupportWorksheet.Assign(useExport.ChildSupportWorksheet);
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

    MoveNextTranInfo(export.Hidden, useImport.NextTranInfo);
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

    useImport.CsePerson.Number = import.ParentACsePerson.Number;
    useImport.Case1.Number = import.Case1.Number;
    MoveLegalAction(import.LegalAction, useImport.LegalAction);

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiReadCsePerson1()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = import.SelectedFromComp.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    export.ParentBName.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseSiReadCsePerson2()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = import.SelectedFromComp.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    export.ParentAName.Assign(useExport.CsePersonsWorkSet);
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

  private bool ReadCodeValue()
  {
    entities.CodeValue.Populated = false;

    return Read("ReadCodeValue",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CodeValue.Id = db.GetInt32(reader, 0);
        entities.CodeValue.CodId = db.GetNullableInt32(reader, 1);
        entities.CodeValue.Cdvalue = db.GetString(reader, 2);
        entities.CodeValue.EffectiveDate = db.GetDate(reader, 3);
        entities.CodeValue.ExpirationDate = db.GetDate(reader, 4);
        entities.CodeValue.Populated = true;
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
      /// A value of Work.
      /// </summary>
      [JsonPropertyName("work")]
      public Common Work
      {
        get => work ??= new();
        set => work = value;
      }

      /// <summary>
      /// A value of ParentB.
      /// </summary>
      [JsonPropertyName("parentB")]
      public CsePersonSupportAdjustment ParentB
      {
        get => parentB ??= new();
        set => parentB = value;
      }

      /// <summary>
      /// A value of ParentA.
      /// </summary>
      [JsonPropertyName("parentA")]
      public CsePersonSupportAdjustment ParentA
      {
        get => parentA ??= new();
        set => parentA = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private Common work;
      private CsePersonSupportAdjustment parentB;
      private CsePersonSupportAdjustment parentA;
    }

    /// <summary>A Gimport2020Group group.</summary>
    [Serializable]
    public class Gimport2020Group
    {
      /// <summary>
      /// A value of GimportDParentTimeAdjFlag.
      /// </summary>
      [JsonPropertyName("gimportDParentTimeAdjFlag")]
      public Common GimportDParentTimeAdjFlag
      {
        get => gimportDParentTimeAdjFlag ??= new();
        set => gimportDParentTimeAdjFlag = value;
      }

      /// <summary>
      /// A value of GimportD4ParAPropShare.
      /// </summary>
      [JsonPropertyName("gimportD4ParAPropShare")]
      public Common GimportD4ParAPropShare
      {
        get => gimportD4ParAPropShare ??= new();
        set => gimportD4ParAPropShare = value;
      }

      /// <summary>
      /// A value of GimportD4ParBPropShare.
      /// </summary>
      [JsonPropertyName("gimportD4ParBPropShare")]
      public Common GimportD4ParBPropShare
      {
        get => gimportD4ParBPropShare ??= new();
        set => gimportD4ParBPropShare = value;
      }

      /// <summary>
      /// A value of GimportD4TotalPropShare.
      /// </summary>
      [JsonPropertyName("gimportD4TotalPropShare")]
      public Common GimportD4TotalPropShare
      {
        get => gimportD4TotalPropShare ??= new();
        set => gimportD4TotalPropShare = value;
      }

      /// <summary>
      /// A value of GimportD5ParAParentTimAdj.
      /// </summary>
      [JsonPropertyName("gimportD5ParAParentTimAdj")]
      public Common GimportD5ParAParentTimAdj
      {
        get => gimportD5ParAParentTimAdj ??= new();
        set => gimportD5ParAParentTimAdj = value;
      }

      /// <summary>
      /// A value of GimportD5ParBParentTimAdj.
      /// </summary>
      [JsonPropertyName("gimportD5ParBParentTimAdj")]
      public Common GimportD5ParBParentTimAdj
      {
        get => gimportD5ParBParentTimAdj ??= new();
        set => gimportD5ParBParentTimAdj = value;
      }

      /// <summary>
      /// A value of GimportD5TotalParentTimAdj.
      /// </summary>
      [JsonPropertyName("gimportD5TotalParentTimAdj")]
      public Common GimportD5TotalParentTimAdj
      {
        get => gimportD5TotalParentTimAdj ??= new();
        set => gimportD5TotalParentTimAdj = value;
      }

      /// <summary>
      /// A value of GimportD6ParAPsAfterPat.
      /// </summary>
      [JsonPropertyName("gimportD6ParAPsAfterPat")]
      public Common GimportD6ParAPsAfterPat
      {
        get => gimportD6ParAPsAfterPat ??= new();
        set => gimportD6ParAPsAfterPat = value;
      }

      /// <summary>
      /// A value of GimportD6ParBPsAfterPat.
      /// </summary>
      [JsonPropertyName("gimportD6ParBPsAfterPat")]
      public Common GimportD6ParBPsAfterPat
      {
        get => gimportD6ParBPsAfterPat ??= new();
        set => gimportD6ParBPsAfterPat = value;
      }

      /// <summary>
      /// A value of GimportD6TotalPsAfterPat.
      /// </summary>
      [JsonPropertyName("gimportD6TotalPsAfterPat")]
      public Common GimportD6TotalPsAfterPat
      {
        get => gimportD6TotalPsAfterPat ??= new();
        set => gimportD6TotalPsAfterPat = value;
      }

      /// <summary>
      /// A value of GimportD8ParAPropShrHip.
      /// </summary>
      [JsonPropertyName("gimportD8ParAPropShrHip")]
      public Common GimportD8ParAPropShrHip
      {
        get => gimportD8ParAPropShrHip ??= new();
        set => gimportD8ParAPropShrHip = value;
      }

      /// <summary>
      /// A value of GimportD8ParBPropShrHip.
      /// </summary>
      [JsonPropertyName("gimportD8ParBPropShrHip")]
      public Common GimportD8ParBPropShrHip
      {
        get => gimportD8ParBPropShrHip ??= new();
        set => gimportD8ParBPropShrHip = value;
      }

      /// <summary>
      /// A value of GimportD8TotalPropShrHip.
      /// </summary>
      [JsonPropertyName("gimportD8TotalPropShrHip")]
      public Common GimportD8TotalPropShrHip
      {
        get => gimportD8TotalPropShrHip ??= new();
        set => gimportD8TotalPropShrHip = value;
      }

      /// <summary>
      /// A value of GimportD10ParAPropShrWrcc.
      /// </summary>
      [JsonPropertyName("gimportD10ParAPropShrWrcc")]
      public Common GimportD10ParAPropShrWrcc
      {
        get => gimportD10ParAPropShrWrcc ??= new();
        set => gimportD10ParAPropShrWrcc = value;
      }

      /// <summary>
      /// A value of GimportD10ParBPropShrWrcc.
      /// </summary>
      [JsonPropertyName("gimportD10ParBPropShrWrcc")]
      public Common GimportD10ParBPropShrWrcc
      {
        get => gimportD10ParBPropShrWrcc ??= new();
        set => gimportD10ParBPropShrWrcc = value;
      }

      /// <summary>
      /// A value of GimportD10TotalPropShrWrcc.
      /// </summary>
      [JsonPropertyName("gimportD10TotalPropShrWrcc")]
      public Common GimportD10TotalPropShrWrcc
      {
        get => gimportD10TotalPropShrWrcc ??= new();
        set => gimportD10TotalPropShrWrcc = value;
      }

      /// <summary>
      /// A value of GimportD11ParAPropShrCcob.
      /// </summary>
      [JsonPropertyName("gimportD11ParAPropShrCcob")]
      public Common GimportD11ParAPropShrCcob
      {
        get => gimportD11ParAPropShrCcob ??= new();
        set => gimportD11ParAPropShrCcob = value;
      }

      /// <summary>
      /// A value of GimportD11ParBPropShrCcob.
      /// </summary>
      [JsonPropertyName("gimportD11ParBPropShrCcob")]
      public Common GimportD11ParBPropShrCcob
      {
        get => gimportD11ParBPropShrCcob ??= new();
        set => gimportD11ParBPropShrCcob = value;
      }

      /// <summary>
      /// A value of GimportD11TotalPropShrCcob.
      /// </summary>
      [JsonPropertyName("gimportD11TotalPropShrCcob")]
      public Common GimportD11TotalPropShrCcob
      {
        get => gimportD11TotalPropShrCcob ??= new();
        set => gimportD11TotalPropShrCcob = value;
      }

      /// <summary>
      /// A value of GimportD12TotalInsWrccPaid.
      /// </summary>
      [JsonPropertyName("gimportD12TotalInsWrccPaid")]
      public Common GimportD12TotalInsWrccPaid
      {
        get => gimportD12TotalInsWrccPaid ??= new();
        set => gimportD12TotalInsWrccPaid = value;
      }

      /// <summary>
      /// A value of GimportD13ParABasicChSup.
      /// </summary>
      [JsonPropertyName("gimportD13ParABasicChSup")]
      public Common GimportD13ParABasicChSup
      {
        get => gimportD13ParABasicChSup ??= new();
        set => gimportD13ParABasicChSup = value;
      }

      /// <summary>
      /// A value of GimportD13ParBBasicChSup.
      /// </summary>
      [JsonPropertyName("gimportD13ParBBasicChSup")]
      public Common GimportD13ParBBasicChSup
      {
        get => gimportD13ParBBasicChSup ??= new();
        set => gimportD13ParBBasicChSup = value;
      }

      /// <summary>
      /// A value of GimportF3ParAAdjSubtotal.
      /// </summary>
      [JsonPropertyName("gimportF3ParAAdjSubtotal")]
      public Common GimportF3ParAAdjSubtotal
      {
        get => gimportF3ParAAdjSubtotal ??= new();
        set => gimportF3ParAAdjSubtotal = value;
      }

      /// <summary>
      /// A value of GimportF3ParBAdjSubtotal.
      /// </summary>
      [JsonPropertyName("gimportF3ParBAdjSubtotal")]
      public Common GimportF3ParBAdjSubtotal
      {
        get => gimportF3ParBAdjSubtotal ??= new();
        set => gimportF3ParBAdjSubtotal = value;
      }

      /// <summary>
      /// A value of GimportF5A0Parent.
      /// </summary>
      [JsonPropertyName("gimportF5A0Parent")]
      public Common GimportF5A0Parent
      {
        get => gimportF5A0Parent ??= new();
        set => gimportF5A0Parent = value;
      }

      /// <summary>
      /// A value of GimportF5A1CsIncome.
      /// </summary>
      [JsonPropertyName("gimportF5A1CsIncome")]
      public Common GimportF5A1CsIncome
      {
        get => gimportF5A1CsIncome ??= new();
        set => gimportF5A1CsIncome = value;
      }

      /// <summary>
      /// A value of GimportF5A2PovertyLevel.
      /// </summary>
      [JsonPropertyName("gimportF5A2PovertyLevel")]
      public Common GimportF5A2PovertyLevel
      {
        get => gimportF5A2PovertyLevel ??= new();
        set => gimportF5A2PovertyLevel = value;
      }

      /// <summary>
      /// A value of GimportF5A3AbilityToPay.
      /// </summary>
      [JsonPropertyName("gimportF5A3AbilityToPay")]
      public Common GimportF5A3AbilityToPay
      {
        get => gimportF5A3AbilityToPay ??= new();
        set => gimportF5A3AbilityToPay = value;
      }

      /// <summary>
      /// A value of GimportF5BParASubtotal.
      /// </summary>
      [JsonPropertyName("gimportF5BParASubtotal")]
      public Common GimportF5BParASubtotal
      {
        get => gimportF5BParASubtotal ??= new();
        set => gimportF5BParASubtotal = value;
      }

      /// <summary>
      /// A value of GimportF5BParBSubtotal.
      /// </summary>
      [JsonPropertyName("gimportF5BParBSubtotal")]
      public Common GimportF5BParBSubtotal
      {
        get => gimportF5BParBSubtotal ??= new();
        set => gimportF5BParBSubtotal = value;
      }

      /// <summary>
      /// A value of GimportF6BParAFinaSubtotal.
      /// </summary>
      [JsonPropertyName("gimportF6BParAFinaSubtotal")]
      public Common GimportF6BParAFinaSubtotal
      {
        get => gimportF6BParAFinaSubtotal ??= new();
        set => gimportF6BParAFinaSubtotal = value;
      }

      /// <summary>
      /// A value of GimportF6BParBFinaSubtotal.
      /// </summary>
      [JsonPropertyName("gimportF6BParBFinaSubtotal")]
      public Common GimportF6BParBFinaSubtotal
      {
        get => gimportF6BParBFinaSubtotal ??= new();
        set => gimportF6BParBFinaSubtotal = value;
      }

      /// <summary>
      /// A value of GimportF8ParANetCsOblig.
      /// </summary>
      [JsonPropertyName("gimportF8ParANetCsOblig")]
      public Common GimportF8ParANetCsOblig
      {
        get => gimportF8ParANetCsOblig ??= new();
        set => gimportF8ParANetCsOblig = value;
      }

      /// <summary>
      /// A value of GimportF8ParBNetCsOblig.
      /// </summary>
      [JsonPropertyName("gimportF8ParBNetCsOblig")]
      public Common GimportF8ParBNetCsOblig
      {
        get => gimportF8ParBNetCsOblig ??= new();
        set => gimportF8ParBNetCsOblig = value;
      }

      private Common gimportDParentTimeAdjFlag;
      private Common gimportD4ParAPropShare;
      private Common gimportD4ParBPropShare;
      private Common gimportD4TotalPropShare;
      private Common gimportD5ParAParentTimAdj;
      private Common gimportD5ParBParentTimAdj;
      private Common gimportD5TotalParentTimAdj;
      private Common gimportD6ParAPsAfterPat;
      private Common gimportD6ParBPsAfterPat;
      private Common gimportD6TotalPsAfterPat;
      private Common gimportD8ParAPropShrHip;
      private Common gimportD8ParBPropShrHip;
      private Common gimportD8TotalPropShrHip;
      private Common gimportD10ParAPropShrWrcc;
      private Common gimportD10ParBPropShrWrcc;
      private Common gimportD10TotalPropShrWrcc;
      private Common gimportD11ParAPropShrCcob;
      private Common gimportD11ParBPropShrCcob;
      private Common gimportD11TotalPropShrCcob;
      private Common gimportD12TotalInsWrccPaid;
      private Common gimportD13ParABasicChSup;
      private Common gimportD13ParBBasicChSup;
      private Common gimportF3ParAAdjSubtotal;
      private Common gimportF3ParBAdjSubtotal;
      private Common gimportF5A0Parent;
      private Common gimportF5A1CsIncome;
      private Common gimportF5A2PovertyLevel;
      private Common gimportF5A3AbilityToPay;
      private Common gimportF5BParASubtotal;
      private Common gimportF5BParBSubtotal;
      private Common gimportF6BParAFinaSubtotal;
      private Common gimportF6BParBFinaSubtotal;
      private Common gimportF8ParANetCsOblig;
      private Common gimportF8ParBNetCsOblig;
    }

    /// <summary>A Gimport2020EnterableFieldsGroup group.</summary>
    [Serializable]
    public class Gimport2020EnterableFieldsGroup
    {
      /// <summary>
      /// A value of GimportParentingTime.
      /// </summary>
      [JsonPropertyName("gimportParentingTime")]
      public Common GimportParentingTime
      {
        get => gimportParentingTime ??= new();
        set => gimportParentingTime = value;
      }

      /// <summary>
      /// A value of GimportAbilityToPayParent.
      /// </summary>
      [JsonPropertyName("gimportAbilityToPayParent")]
      public Common GimportAbilityToPayParent
      {
        get => gimportAbilityToPayParent ??= new();
        set => gimportAbilityToPayParent = value;
      }

      private Common gimportParentingTime;
      private Common gimportAbilityToPayParent;
    }

    /// <summary>
    /// A value of GuidelineYearPrompt.
    /// </summary>
    [JsonPropertyName("guidelineYearPrompt")]
    public Common GuidelineYearPrompt
    {
      get => guidelineYearPrompt ??= new();
      set => guidelineYearPrompt = value;
    }

    /// <summary>
    /// A value of SelectedCodeValue.
    /// </summary>
    [JsonPropertyName("selectedCodeValue")]
    public CodeValue SelectedCodeValue
    {
      get => selectedCodeValue ??= new();
      set => selectedCodeValue = value;
    }

    /// <summary>
    /// A value of Required.
    /// </summary>
    [JsonPropertyName("required")]
    public Code Required
    {
      get => required ??= new();
      set => required = value;
    }

    /// <summary>
    /// A value of AuthorizingPrompt.
    /// </summary>
    [JsonPropertyName("authorizingPrompt")]
    public Common AuthorizingPrompt
    {
      get => authorizingPrompt ??= new();
      set => authorizingPrompt = value;
    }

    /// <summary>
    /// A value of FromCren.
    /// </summary>
    [JsonPropertyName("fromCren")]
    public Common FromCren
    {
      get => fromCren ??= new();
      set => fromCren = value;
    }

    /// <summary>
    /// A value of SelectedFromComp.
    /// </summary>
    [JsonPropertyName("selectedFromComp")]
    public CsePersonsWorkSet SelectedFromComp
    {
      get => selectedFromComp ??= new();
      set => selectedFromComp = value;
    }

    /// <summary>
    /// A value of ParentBPrompt.
    /// </summary>
    [JsonPropertyName("parentBPrompt")]
    public Common ParentBPrompt
    {
      get => parentBPrompt ??= new();
      set => parentBPrompt = value;
    }

    /// <summary>
    /// A value of ParentAPrompt.
    /// </summary>
    [JsonPropertyName("parentAPrompt")]
    public Common ParentAPrompt
    {
      get => parentAPrompt ??= new();
      set => parentAPrompt = value;
    }

    /// <summary>
    /// A value of SelectedLegalAction.
    /// </summary>
    [JsonPropertyName("selectedLegalAction")]
    public LegalAction SelectedLegalAction
    {
      get => selectedLegalAction ??= new();
      set => selectedLegalAction = value;
    }

    /// <summary>
    /// A value of CommandH.
    /// </summary>
    [JsonPropertyName("commandH")]
    public Common CommandH
    {
      get => commandH ??= new();
      set => commandH = value;
    }

    /// <summary>
    /// A value of County.
    /// </summary>
    [JsonPropertyName("county")]
    public CodeValue County
    {
      get => county ??= new();
      set => county = value;
    }

    /// <summary>
    /// A value of H.
    /// </summary>
    [JsonPropertyName("h")]
    public FipsTribAddress H
    {
      get => h ??= new();
      set => h = value;
    }

    /// <summary>
    /// A value of ParentBName.
    /// </summary>
    [JsonPropertyName("parentBName")]
    public CsePersonsWorkSet ParentBName
    {
      get => parentBName ??= new();
      set => parentBName = value;
    }

    /// <summary>
    /// A value of ParentAName.
    /// </summary>
    [JsonPropertyName("parentAName")]
    public CsePersonsWorkSet ParentAName
    {
      get => parentAName ??= new();
      set => parentAName = value;
    }

    /// <summary>
    /// A value of LastUpdtDate.
    /// </summary>
    [JsonPropertyName("lastUpdtDate")]
    public Code LastUpdtDate
    {
      get => lastUpdtDate ??= new();
      set => lastUpdtDate = value;
    }

    /// <summary>
    /// A value of WorkPrev.
    /// </summary>
    [JsonPropertyName("workPrev")]
    public Standard WorkPrev
    {
      get => workPrev ??= new();
      set => workPrev = value;
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
    /// A value of ParentBF3AdjCsOblig.
    /// </summary>
    [JsonPropertyName("parentBF3AdjCsOblig")]
    public Common ParentBF3AdjCsOblig
    {
      get => parentBF3AdjCsOblig ??= new();
      set => parentBF3AdjCsOblig = value;
    }

    /// <summary>
    /// A value of ParentBE7F2TotAdjust.
    /// </summary>
    [JsonPropertyName("parentBE7F2TotAdjust")]
    public Common ParentBE7F2TotAdjust
    {
      get => parentBE7F2TotAdjust ??= new();
      set => parentBE7F2TotAdjust = value;
    }

    /// <summary>
    /// A value of ParentBF2TotalCsAdj.
    /// </summary>
    [JsonPropertyName("parentBF2TotalCsAdj")]
    public Common ParentBF2TotalCsAdj
    {
      get => parentBF2TotalCsAdj ??= new();
      set => parentBF2TotalCsAdj = value;
    }

    /// <summary>
    /// A value of ParentAF2TotalCsAdj.
    /// </summary>
    [JsonPropertyName("parentAF2TotalCsAdj")]
    public Common ParentAF2TotalCsAdj
    {
      get => parentAF2TotalCsAdj ??= new();
      set => parentAF2TotalCsAdj = value;
    }

    /// <summary>
    /// A value of ParentAF3AdjCsOblig.
    /// </summary>
    [JsonPropertyName("parentAF3AdjCsOblig")]
    public Common ParentAF3AdjCsOblig
    {
      get => parentAF3AdjCsOblig ??= new();
      set => parentAF3AdjCsOblig = value;
    }

    /// <summary>
    /// A value of ParentAE7F2TotAdjust.
    /// </summary>
    [JsonPropertyName("parentAE7F2TotAdjust")]
    public Common ParentAE7F2TotAdjust
    {
      get => parentAE7F2TotAdjust ??= new();
      set => parentAE7F2TotAdjust = value;
    }

    /// <summary>
    /// A value of ParentAD9F1NetCs.
    /// </summary>
    [JsonPropertyName("parentAD9F1NetCs")]
    public Common ParentAD9F1NetCs
    {
      get => parentAD9F1NetCs ??= new();
      set => parentAD9F1NetCs = value;
    }

    /// <summary>
    /// A value of ParentBD9F1NetCs.
    /// </summary>
    [JsonPropertyName("parentBD9F1NetCs")]
    public Common ParentBD9F1NetCs
    {
      get => parentBD9F1NetCs ??= new();
      set => parentBD9F1NetCs = value;
    }

    /// <summary>
    /// A value of ParentBD8Adjustments.
    /// </summary>
    [JsonPropertyName("parentBD8Adjustments")]
    public Common ParentBD8Adjustments
    {
      get => parentBD8Adjustments ??= new();
      set => parentBD8Adjustments = value;
    }

    /// <summary>
    /// A value of ParentAD8Adjustments.
    /// </summary>
    [JsonPropertyName("parentAD8Adjustments")]
    public Common ParentAD8Adjustments
    {
      get => parentAD8Adjustments ??= new();
      set => parentAD8Adjustments = value;
    }

    /// <summary>
    /// A value of ParentBD7CsOblig.
    /// </summary>
    [JsonPropertyName("parentBD7CsOblig")]
    public Common ParentBD7CsOblig
    {
      get => parentBD7CsOblig ??= new();
      set => parentBD7CsOblig = value;
    }

    /// <summary>
    /// A value of ParentAD7CsOblig.
    /// </summary>
    [JsonPropertyName("parentAD7CsOblig")]
    public Common ParentAD7CsOblig
    {
      get => parentAD7CsOblig ??= new();
      set => parentAD7CsOblig = value;
    }

    /// <summary>
    /// A value of D6otalChildSuppOblig.
    /// </summary>
    [JsonPropertyName("d6otalChildSuppOblig")]
    public Common D6otalChildSuppOblig
    {
      get => d6otalChildSuppOblig ??= new();
      set => d6otalChildSuppOblig = value;
    }

    /// <summary>
    /// A value of TotalChildCareCost.
    /// </summary>
    [JsonPropertyName("totalChildCareCost")]
    public Common TotalChildCareCost
    {
      get => totalChildCareCost ??= new();
      set => totalChildCareCost = value;
    }

    /// <summary>
    /// A value of ParentAChildCareCost.
    /// </summary>
    [JsonPropertyName("parentAChildCareCost")]
    public Common ParentAChildCareCost
    {
      get => parentAChildCareCost ??= new();
      set => parentAChildCareCost = value;
    }

    /// <summary>
    /// A value of ParentBChildCareCost.
    /// </summary>
    [JsonPropertyName("parentBChildCareCost")]
    public Common ParentBChildCareCost
    {
      get => parentBChildCareCost ??= new();
      set => parentBChildCareCost = value;
    }

    /// <summary>
    /// A value of ParentBTotalTaxCredit.
    /// </summary>
    [JsonPropertyName("parentBTotalTaxCredit")]
    public Common ParentBTotalTaxCredit
    {
      get => parentBTotalTaxCredit ??= new();
      set => parentBTotalTaxCredit = value;
    }

    /// <summary>
    /// A value of ParentATotalTaxCredit.
    /// </summary>
    [JsonPropertyName("parentATotalTaxCredit")]
    public Common ParentATotalTaxCredit
    {
      get => parentATotalTaxCredit ??= new();
      set => parentATotalTaxCredit = value;
    }

    /// <summary>
    /// A value of TotalInsurancePrem.
    /// </summary>
    [JsonPropertyName("totalInsurancePrem")]
    public Common TotalInsurancePrem
    {
      get => totalInsurancePrem ??= new();
      set => totalInsurancePrem = value;
    }

    /// <summary>
    /// A value of CsObligTotalAmount.
    /// </summary>
    [JsonPropertyName("csObligTotalAmount")]
    public Common CsObligTotalAmount
    {
      get => csObligTotalAmount ??= new();
      set => csObligTotalAmount = value;
    }

    /// <summary>
    /// A value of CsObligAgrp3TotalInc.
    /// </summary>
    [JsonPropertyName("csObligAgrp3TotalInc")]
    public Common CsObligAgrp3TotalInc
    {
      get => csObligAgrp3TotalInc ??= new();
      set => csObligAgrp3TotalInc = value;
    }

    /// <summary>
    /// A value of CsObligAgrp2TotalAmt.
    /// </summary>
    [JsonPropertyName("csObligAgrp2TotalAmt")]
    public Common CsObligAgrp2TotalAmt
    {
      get => csObligAgrp2TotalAmt ??= new();
      set => csObligAgrp2TotalAmt = value;
    }

    /// <summary>
    /// A value of CsObligAgrp1TotalAmt.
    /// </summary>
    [JsonPropertyName("csObligAgrp1TotalAmt")]
    public Common CsObligAgrp1TotalAmt
    {
      get => csObligAgrp1TotalAmt ??= new();
      set => csObligAgrp1TotalAmt = value;
    }

    /// <summary>
    /// A value of ParentBD2PercentInc.
    /// </summary>
    [JsonPropertyName("parentBD2PercentInc")]
    public Common ParentBD2PercentInc
    {
      get => parentBD2PercentInc ??= new();
      set => parentBD2PercentInc = value;
    }

    /// <summary>
    /// A value of ParentAD2PercentInc.
    /// </summary>
    [JsonPropertyName("parentAD2PercentInc")]
    public Common ParentAD2PercentInc
    {
      get => parentAD2PercentInc ??= new();
      set => parentAD2PercentInc = value;
    }

    /// <summary>
    /// A value of D1otalCsInc.
    /// </summary>
    [JsonPropertyName("d1otalCsInc")]
    public Common D1otalCsInc
    {
      get => d1otalCsInc ??= new();
      set => d1otalCsInc = value;
    }

    /// <summary>
    /// A value of ParentBC5D1TotCsInc.
    /// </summary>
    [JsonPropertyName("parentBC5D1TotCsInc")]
    public Common ParentBC5D1TotCsInc
    {
      get => parentBC5D1TotCsInc ??= new();
      set => parentBC5D1TotCsInc = value;
    }

    /// <summary>
    /// A value of ParentAC5D1TotCsInc.
    /// </summary>
    [JsonPropertyName("parentAC5D1TotCsInc")]
    public Common ParentAC5D1TotCsInc
    {
      get => parentAC5D1TotCsInc ??= new();
      set => parentAC5D1TotCsInc = value;
    }

    /// <summary>
    /// A value of ParentBC1TotGrossInc.
    /// </summary>
    [JsonPropertyName("parentBC1TotGrossInc")]
    public Common ParentBC1TotGrossInc
    {
      get => parentBC1TotGrossInc ??= new();
      set => parentBC1TotGrossInc = value;
    }

    /// <summary>
    /// A value of ParentAC1TotGrossInc.
    /// </summary>
    [JsonPropertyName("parentAC1TotGrossInc")]
    public Common ParentAC1TotGrossInc
    {
      get => parentAC1TotGrossInc ??= new();
      set => parentAC1TotGrossInc = value;
    }

    /// <summary>
    /// A value of ParentBB3SeGrossInc.
    /// </summary>
    [JsonPropertyName("parentBB3SeGrossInc")]
    public Common ParentBB3SeGrossInc
    {
      get => parentBB3SeGrossInc ??= new();
      set => parentBB3SeGrossInc = value;
    }

    /// <summary>
    /// A value of ParentAB3SeGrossInc.
    /// </summary>
    [JsonPropertyName("parentAB3SeGrossInc")]
    public Common ParentAB3SeGrossInc
    {
      get => parentAB3SeGrossInc ??= new();
      set => parentAB3SeGrossInc = value;
    }

    /// <summary>
    /// A value of ChildSupportWorksheet.
    /// </summary>
    [JsonPropertyName("childSupportWorksheet")]
    public ChildSupportWorksheet ChildSupportWorksheet
    {
      get => childSupportWorksheet ??= new();
      set => childSupportWorksheet = value;
    }

    /// <summary>
    /// A value of PrevChildSupportWorksheet.
    /// </summary>
    [JsonPropertyName("prevChildSupportWorksheet")]
    public ChildSupportWorksheet PrevChildSupportWorksheet
    {
      get => prevChildSupportWorksheet ??= new();
      set => prevChildSupportWorksheet = value;
    }

    /// <summary>
    /// A value of Prev2.
    /// </summary>
    [JsonPropertyName("prev2")]
    public ChildSupportWorksheet Prev2
    {
      get => prev2 ??= new();
      set => prev2 = value;
    }

    /// <summary>
    /// A value of ParentBCsePersonSupportWorksheet.
    /// </summary>
    [JsonPropertyName("parentBCsePersonSupportWorksheet")]
    public CsePersonSupportWorksheet ParentBCsePersonSupportWorksheet
    {
      get => parentBCsePersonSupportWorksheet ??= new();
      set => parentBCsePersonSupportWorksheet = value;
    }

    /// <summary>
    /// A value of PrevParentBCsePersonSupportWorksheet.
    /// </summary>
    [JsonPropertyName("prevParentBCsePersonSupportWorksheet")]
    public CsePersonSupportWorksheet PrevParentBCsePersonSupportWorksheet
    {
      get => prevParentBCsePersonSupportWorksheet ??= new();
      set => prevParentBCsePersonSupportWorksheet = value;
    }

    /// <summary>
    /// A value of ParentACsePersonSupportWorksheet.
    /// </summary>
    [JsonPropertyName("parentACsePersonSupportWorksheet")]
    public CsePersonSupportWorksheet ParentACsePersonSupportWorksheet
    {
      get => parentACsePersonSupportWorksheet ??= new();
      set => parentACsePersonSupportWorksheet = value;
    }

    /// <summary>
    /// A value of PrevParentACsePersonSupportWorksheet.
    /// </summary>
    [JsonPropertyName("prevParentACsePersonSupportWorksheet")]
    public CsePersonSupportWorksheet PrevParentACsePersonSupportWorksheet
    {
      get => prevParentACsePersonSupportWorksheet ??= new();
      set => prevParentACsePersonSupportWorksheet = value;
    }

    /// <summary>
    /// A value of ParentBCsePerson.
    /// </summary>
    [JsonPropertyName("parentBCsePerson")]
    public CsePerson ParentBCsePerson
    {
      get => parentBCsePerson ??= new();
      set => parentBCsePerson = value;
    }

    /// <summary>
    /// A value of PrevParentBCsePerson.
    /// </summary>
    [JsonPropertyName("prevParentBCsePerson")]
    public CsePerson PrevParentBCsePerson
    {
      get => prevParentBCsePerson ??= new();
      set => prevParentBCsePerson = value;
    }

    /// <summary>
    /// A value of ParentACsePerson.
    /// </summary>
    [JsonPropertyName("parentACsePerson")]
    public CsePerson ParentACsePerson
    {
      get => parentACsePerson ??= new();
      set => parentACsePerson = value;
    }

    /// <summary>
    /// A value of PrevParentACsePerson.
    /// </summary>
    [JsonPropertyName("prevParentACsePerson")]
    public CsePerson PrevParentACsePerson
    {
      get => prevParentACsePerson ??= new();
      set => prevParentACsePerson = value;
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
    /// A value of PrevCase.
    /// </summary>
    [JsonPropertyName("prevCase")]
    public Case1 PrevCase
    {
      get => prevCase ??= new();
      set => prevCase = value;
    }

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
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
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Case1 Next
    {
      get => next ??= new();
      set => next = value;
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
    /// A value of CaseOpen.
    /// </summary>
    [JsonPropertyName("caseOpen")]
    public Common CaseOpen
    {
      get => caseOpen ??= new();
      set => caseOpen = value;
    }

    /// <summary>
    /// A value of CaseRoleInactive.
    /// </summary>
    [JsonPropertyName("caseRoleInactive")]
    public Common CaseRoleInactive
    {
      get => caseRoleInactive ??= new();
      set => caseRoleInactive = value;
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
    /// A value of ParentAD10F1NetCs.
    /// </summary>
    [JsonPropertyName("parentAD10F1NetCs")]
    public Common ParentAD10F1NetCs
    {
      get => parentAD10F1NetCs ??= new();
      set => parentAD10F1NetCs = value;
    }

    /// <summary>
    /// A value of ParentBD10F1NetCs.
    /// </summary>
    [JsonPropertyName("parentBD10F1NetCs")]
    public Common ParentBD10F1NetCs
    {
      get => parentBD10F1NetCs ??= new();
      set => parentBD10F1NetCs = value;
    }

    /// <summary>
    /// A value of ParentAEnfFee.
    /// </summary>
    [JsonPropertyName("parentAEnfFee")]
    public Common ParentAEnfFee
    {
      get => parentAEnfFee ??= new();
      set => parentAEnfFee = value;
    }

    /// <summary>
    /// A value of ParentBEnfFee.
    /// </summary>
    [JsonPropertyName("parentBEnfFee")]
    public Common ParentBEnfFee
    {
      get => parentBEnfFee ??= new();
      set => parentBEnfFee = value;
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
    /// Gets a value of Gimport2020.
    /// </summary>
    [JsonPropertyName("gimport2020")]
    public Gimport2020Group Gimport2020
    {
      get => gimport2020 ?? (gimport2020 = new());
      set => gimport2020 = value;
    }

    /// <summary>
    /// Gets a value of Gimport2020EnterableFields.
    /// </summary>
    [JsonPropertyName("gimport2020EnterableFields")]
    public Gimport2020EnterableFieldsGroup Gimport2020EnterableFields
    {
      get => gimport2020EnterableFields ?? (gimport2020EnterableFields = new());
      set => gimport2020EnterableFields = value;
    }

    /// <summary>
    /// A value of GuidelineYearChange.
    /// </summary>
    [JsonPropertyName("guidelineYearChange")]
    public Common GuidelineYearChange
    {
      get => guidelineYearChange ??= new();
      set => guidelineYearChange = value;
    }

    private Common guidelineYearPrompt;
    private CodeValue selectedCodeValue;
    private Code required;
    private Common authorizingPrompt;
    private Common fromCren;
    private CsePersonsWorkSet selectedFromComp;
    private Common parentBPrompt;
    private Common parentAPrompt;
    private LegalAction selectedLegalAction;
    private Common commandH;
    private CodeValue county;
    private FipsTribAddress h;
    private CsePersonsWorkSet parentBName;
    private CsePersonsWorkSet parentAName;
    private Code lastUpdtDate;
    private Standard workPrev;
    private Array<ImportGroup> import1;
    private Common parentBF3AdjCsOblig;
    private Common parentBE7F2TotAdjust;
    private Common parentBF2TotalCsAdj;
    private Common parentAF2TotalCsAdj;
    private Common parentAF3AdjCsOblig;
    private Common parentAE7F2TotAdjust;
    private Common parentAD9F1NetCs;
    private Common parentBD9F1NetCs;
    private Common parentBD8Adjustments;
    private Common parentAD8Adjustments;
    private Common parentBD7CsOblig;
    private Common parentAD7CsOblig;
    private Common d6otalChildSuppOblig;
    private Common totalChildCareCost;
    private Common parentAChildCareCost;
    private Common parentBChildCareCost;
    private Common parentBTotalTaxCredit;
    private Common parentATotalTaxCredit;
    private Common totalInsurancePrem;
    private Common csObligTotalAmount;
    private Common csObligAgrp3TotalInc;
    private Common csObligAgrp2TotalAmt;
    private Common csObligAgrp1TotalAmt;
    private Common parentBD2PercentInc;
    private Common parentAD2PercentInc;
    private Common d1otalCsInc;
    private Common parentBC5D1TotCsInc;
    private Common parentAC5D1TotCsInc;
    private Common parentBC1TotGrossInc;
    private Common parentAC1TotGrossInc;
    private Common parentBB3SeGrossInc;
    private Common parentAB3SeGrossInc;
    private ChildSupportWorksheet childSupportWorksheet;
    private ChildSupportWorksheet prevChildSupportWorksheet;
    private ChildSupportWorksheet prev2;
    private CsePersonSupportWorksheet parentBCsePersonSupportWorksheet;
    private CsePersonSupportWorksheet prevParentBCsePersonSupportWorksheet;
    private CsePersonSupportWorksheet parentACsePersonSupportWorksheet;
    private CsePersonSupportWorksheet prevParentACsePersonSupportWorksheet;
    private CsePerson parentBCsePerson;
    private CsePerson prevParentBCsePerson;
    private CsePerson parentACsePerson;
    private CsePerson prevParentACsePerson;
    private Case1 case1;
    private Case1 prevCase;
    private Tribunal tribunal;
    private LegalAction legalAction;
    private NextTranInfo hidden;
    private Standard standard;
    private Case1 next;
    private ServiceProvider serviceProvider;
    private Office office;
    private Common caseOpen;
    private Common caseRoleInactive;
    private Common common;
    private Common parentAD10F1NetCs;
    private Common parentBD10F1NetCs;
    private Common parentAEnfFee;
    private Common parentBEnfFee;
    private WorkArea headerLine;
    private Gimport2020Group gimport2020;
    private Gimport2020EnterableFieldsGroup gimport2020EnterableFields;
    private Common guidelineYearChange;
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
      /// A value of Work.
      /// </summary>
      [JsonPropertyName("work")]
      public Common Work
      {
        get => work ??= new();
        set => work = value;
      }

      /// <summary>
      /// A value of ParentB.
      /// </summary>
      [JsonPropertyName("parentB")]
      public CsePersonSupportAdjustment ParentB
      {
        get => parentB ??= new();
        set => parentB = value;
      }

      /// <summary>
      /// A value of ParentA.
      /// </summary>
      [JsonPropertyName("parentA")]
      public CsePersonSupportAdjustment ParentA
      {
        get => parentA ??= new();
        set => parentA = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private Common work;
      private CsePersonSupportAdjustment parentB;
      private CsePersonSupportAdjustment parentA;
    }

    /// <summary>A Gexport2020Group group.</summary>
    [Serializable]
    public class Gexport2020Group
    {
      /// <summary>
      /// A value of GexportDParentTimeAdjFlag.
      /// </summary>
      [JsonPropertyName("gexportDParentTimeAdjFlag")]
      public Common GexportDParentTimeAdjFlag
      {
        get => gexportDParentTimeAdjFlag ??= new();
        set => gexportDParentTimeAdjFlag = value;
      }

      /// <summary>
      /// A value of GexportD4ParAPropShare.
      /// </summary>
      [JsonPropertyName("gexportD4ParAPropShare")]
      public Common GexportD4ParAPropShare
      {
        get => gexportD4ParAPropShare ??= new();
        set => gexportD4ParAPropShare = value;
      }

      /// <summary>
      /// A value of GexportD4ParBPropShare.
      /// </summary>
      [JsonPropertyName("gexportD4ParBPropShare")]
      public Common GexportD4ParBPropShare
      {
        get => gexportD4ParBPropShare ??= new();
        set => gexportD4ParBPropShare = value;
      }

      /// <summary>
      /// A value of GexportD4TotalPropShare.
      /// </summary>
      [JsonPropertyName("gexportD4TotalPropShare")]
      public Common GexportD4TotalPropShare
      {
        get => gexportD4TotalPropShare ??= new();
        set => gexportD4TotalPropShare = value;
      }

      /// <summary>
      /// A value of GexportD5ParAParentTimAdj.
      /// </summary>
      [JsonPropertyName("gexportD5ParAParentTimAdj")]
      public Common GexportD5ParAParentTimAdj
      {
        get => gexportD5ParAParentTimAdj ??= new();
        set => gexportD5ParAParentTimAdj = value;
      }

      /// <summary>
      /// A value of GexportD5ParBParentTimAdj.
      /// </summary>
      [JsonPropertyName("gexportD5ParBParentTimAdj")]
      public Common GexportD5ParBParentTimAdj
      {
        get => gexportD5ParBParentTimAdj ??= new();
        set => gexportD5ParBParentTimAdj = value;
      }

      /// <summary>
      /// A value of GexportD5TotalParentTimAdj.
      /// </summary>
      [JsonPropertyName("gexportD5TotalParentTimAdj")]
      public Common GexportD5TotalParentTimAdj
      {
        get => gexportD5TotalParentTimAdj ??= new();
        set => gexportD5TotalParentTimAdj = value;
      }

      /// <summary>
      /// A value of GexportD6ParAPsAfterPat.
      /// </summary>
      [JsonPropertyName("gexportD6ParAPsAfterPat")]
      public Common GexportD6ParAPsAfterPat
      {
        get => gexportD6ParAPsAfterPat ??= new();
        set => gexportD6ParAPsAfterPat = value;
      }

      /// <summary>
      /// A value of GexportD6ParBPsAfterPat.
      /// </summary>
      [JsonPropertyName("gexportD6ParBPsAfterPat")]
      public Common GexportD6ParBPsAfterPat
      {
        get => gexportD6ParBPsAfterPat ??= new();
        set => gexportD6ParBPsAfterPat = value;
      }

      /// <summary>
      /// A value of GexportD6TotalPsAfterPat.
      /// </summary>
      [JsonPropertyName("gexportD6TotalPsAfterPat")]
      public Common GexportD6TotalPsAfterPat
      {
        get => gexportD6TotalPsAfterPat ??= new();
        set => gexportD6TotalPsAfterPat = value;
      }

      /// <summary>
      /// A value of GexportD8ParAPropShrHip.
      /// </summary>
      [JsonPropertyName("gexportD8ParAPropShrHip")]
      public Common GexportD8ParAPropShrHip
      {
        get => gexportD8ParAPropShrHip ??= new();
        set => gexportD8ParAPropShrHip = value;
      }

      /// <summary>
      /// A value of GexportD8ParBPropShrHip.
      /// </summary>
      [JsonPropertyName("gexportD8ParBPropShrHip")]
      public Common GexportD8ParBPropShrHip
      {
        get => gexportD8ParBPropShrHip ??= new();
        set => gexportD8ParBPropShrHip = value;
      }

      /// <summary>
      /// A value of GexportD8TotalPropShrHip.
      /// </summary>
      [JsonPropertyName("gexportD8TotalPropShrHip")]
      public Common GexportD8TotalPropShrHip
      {
        get => gexportD8TotalPropShrHip ??= new();
        set => gexportD8TotalPropShrHip = value;
      }

      /// <summary>
      /// A value of GexportD10ParAPropShrWrcc.
      /// </summary>
      [JsonPropertyName("gexportD10ParAPropShrWrcc")]
      public Common GexportD10ParAPropShrWrcc
      {
        get => gexportD10ParAPropShrWrcc ??= new();
        set => gexportD10ParAPropShrWrcc = value;
      }

      /// <summary>
      /// A value of GexportD10ParBPropShrWrcc.
      /// </summary>
      [JsonPropertyName("gexportD10ParBPropShrWrcc")]
      public Common GexportD10ParBPropShrWrcc
      {
        get => gexportD10ParBPropShrWrcc ??= new();
        set => gexportD10ParBPropShrWrcc = value;
      }

      /// <summary>
      /// A value of GexportD10TotalPropShrWrcc.
      /// </summary>
      [JsonPropertyName("gexportD10TotalPropShrWrcc")]
      public Common GexportD10TotalPropShrWrcc
      {
        get => gexportD10TotalPropShrWrcc ??= new();
        set => gexportD10TotalPropShrWrcc = value;
      }

      /// <summary>
      /// A value of GexportD11ParAPropShrCcob.
      /// </summary>
      [JsonPropertyName("gexportD11ParAPropShrCcob")]
      public Common GexportD11ParAPropShrCcob
      {
        get => gexportD11ParAPropShrCcob ??= new();
        set => gexportD11ParAPropShrCcob = value;
      }

      /// <summary>
      /// A value of GexportD11ParBPropShrCcob.
      /// </summary>
      [JsonPropertyName("gexportD11ParBPropShrCcob")]
      public Common GexportD11ParBPropShrCcob
      {
        get => gexportD11ParBPropShrCcob ??= new();
        set => gexportD11ParBPropShrCcob = value;
      }

      /// <summary>
      /// A value of GexportD11TotalPropShrCcob.
      /// </summary>
      [JsonPropertyName("gexportD11TotalPropShrCcob")]
      public Common GexportD11TotalPropShrCcob
      {
        get => gexportD11TotalPropShrCcob ??= new();
        set => gexportD11TotalPropShrCcob = value;
      }

      /// <summary>
      /// A value of GexportD12TotalInsWrccPaid.
      /// </summary>
      [JsonPropertyName("gexportD12TotalInsWrccPaid")]
      public Common GexportD12TotalInsWrccPaid
      {
        get => gexportD12TotalInsWrccPaid ??= new();
        set => gexportD12TotalInsWrccPaid = value;
      }

      /// <summary>
      /// A value of GexportD13ParABasicChSup.
      /// </summary>
      [JsonPropertyName("gexportD13ParABasicChSup")]
      public Common GexportD13ParABasicChSup
      {
        get => gexportD13ParABasicChSup ??= new();
        set => gexportD13ParABasicChSup = value;
      }

      /// <summary>
      /// A value of GexportD13ParBBasicChSup.
      /// </summary>
      [JsonPropertyName("gexportD13ParBBasicChSup")]
      public Common GexportD13ParBBasicChSup
      {
        get => gexportD13ParBBasicChSup ??= new();
        set => gexportD13ParBBasicChSup = value;
      }

      /// <summary>
      /// A value of GexportF3ParAAdjSubtotal.
      /// </summary>
      [JsonPropertyName("gexportF3ParAAdjSubtotal")]
      public Common GexportF3ParAAdjSubtotal
      {
        get => gexportF3ParAAdjSubtotal ??= new();
        set => gexportF3ParAAdjSubtotal = value;
      }

      /// <summary>
      /// A value of GexportF3ParBAdjSubtotal.
      /// </summary>
      [JsonPropertyName("gexportF3ParBAdjSubtotal")]
      public Common GexportF3ParBAdjSubtotal
      {
        get => gexportF3ParBAdjSubtotal ??= new();
        set => gexportF3ParBAdjSubtotal = value;
      }

      /// <summary>
      /// A value of GexportF5A0Parent.
      /// </summary>
      [JsonPropertyName("gexportF5A0Parent")]
      public Common GexportF5A0Parent
      {
        get => gexportF5A0Parent ??= new();
        set => gexportF5A0Parent = value;
      }

      /// <summary>
      /// A value of GexportF5A1CsIncome.
      /// </summary>
      [JsonPropertyName("gexportF5A1CsIncome")]
      public Common GexportF5A1CsIncome
      {
        get => gexportF5A1CsIncome ??= new();
        set => gexportF5A1CsIncome = value;
      }

      /// <summary>
      /// A value of GexportF5A2PovertyLevel.
      /// </summary>
      [JsonPropertyName("gexportF5A2PovertyLevel")]
      public Common GexportF5A2PovertyLevel
      {
        get => gexportF5A2PovertyLevel ??= new();
        set => gexportF5A2PovertyLevel = value;
      }

      /// <summary>
      /// A value of GexportF5A3AbilityToPay.
      /// </summary>
      [JsonPropertyName("gexportF5A3AbilityToPay")]
      public Common GexportF5A3AbilityToPay
      {
        get => gexportF5A3AbilityToPay ??= new();
        set => gexportF5A3AbilityToPay = value;
      }

      /// <summary>
      /// A value of GexportF5BParASubtotal.
      /// </summary>
      [JsonPropertyName("gexportF5BParASubtotal")]
      public Common GexportF5BParASubtotal
      {
        get => gexportF5BParASubtotal ??= new();
        set => gexportF5BParASubtotal = value;
      }

      /// <summary>
      /// A value of GexportF5BParBSubtotal.
      /// </summary>
      [JsonPropertyName("gexportF5BParBSubtotal")]
      public Common GexportF5BParBSubtotal
      {
        get => gexportF5BParBSubtotal ??= new();
        set => gexportF5BParBSubtotal = value;
      }

      /// <summary>
      /// A value of GexportF6BParAFinaSubtotal.
      /// </summary>
      [JsonPropertyName("gexportF6BParAFinaSubtotal")]
      public Common GexportF6BParAFinaSubtotal
      {
        get => gexportF6BParAFinaSubtotal ??= new();
        set => gexportF6BParAFinaSubtotal = value;
      }

      /// <summary>
      /// A value of GexportF6BParBFinaSubtotal.
      /// </summary>
      [JsonPropertyName("gexportF6BParBFinaSubtotal")]
      public Common GexportF6BParBFinaSubtotal
      {
        get => gexportF6BParBFinaSubtotal ??= new();
        set => gexportF6BParBFinaSubtotal = value;
      }

      /// <summary>
      /// A value of GexportF8ParANetCsOblig.
      /// </summary>
      [JsonPropertyName("gexportF8ParANetCsOblig")]
      public Common GexportF8ParANetCsOblig
      {
        get => gexportF8ParANetCsOblig ??= new();
        set => gexportF8ParANetCsOblig = value;
      }

      /// <summary>
      /// A value of GexportF8ParBNetCsOblig.
      /// </summary>
      [JsonPropertyName("gexportF8ParBNetCsOblig")]
      public Common GexportF8ParBNetCsOblig
      {
        get => gexportF8ParBNetCsOblig ??= new();
        set => gexportF8ParBNetCsOblig = value;
      }

      private Common gexportDParentTimeAdjFlag;
      private Common gexportD4ParAPropShare;
      private Common gexportD4ParBPropShare;
      private Common gexportD4TotalPropShare;
      private Common gexportD5ParAParentTimAdj;
      private Common gexportD5ParBParentTimAdj;
      private Common gexportD5TotalParentTimAdj;
      private Common gexportD6ParAPsAfterPat;
      private Common gexportD6ParBPsAfterPat;
      private Common gexportD6TotalPsAfterPat;
      private Common gexportD8ParAPropShrHip;
      private Common gexportD8ParBPropShrHip;
      private Common gexportD8TotalPropShrHip;
      private Common gexportD10ParAPropShrWrcc;
      private Common gexportD10ParBPropShrWrcc;
      private Common gexportD10TotalPropShrWrcc;
      private Common gexportD11ParAPropShrCcob;
      private Common gexportD11ParBPropShrCcob;
      private Common gexportD11TotalPropShrCcob;
      private Common gexportD12TotalInsWrccPaid;
      private Common gexportD13ParABasicChSup;
      private Common gexportD13ParBBasicChSup;
      private Common gexportF3ParAAdjSubtotal;
      private Common gexportF3ParBAdjSubtotal;
      private Common gexportF5A0Parent;
      private Common gexportF5A1CsIncome;
      private Common gexportF5A2PovertyLevel;
      private Common gexportF5A3AbilityToPay;
      private Common gexportF5BParASubtotal;
      private Common gexportF5BParBSubtotal;
      private Common gexportF6BParAFinaSubtotal;
      private Common gexportF6BParBFinaSubtotal;
      private Common gexportF8ParANetCsOblig;
      private Common gexportF8ParBNetCsOblig;
    }

    /// <summary>A Gexport2020EnterableFieldsGroup group.</summary>
    [Serializable]
    public class Gexport2020EnterableFieldsGroup
    {
      /// <summary>
      /// A value of GexportParentingTime.
      /// </summary>
      [JsonPropertyName("gexportParentingTime")]
      public Common GexportParentingTime
      {
        get => gexportParentingTime ??= new();
        set => gexportParentingTime = value;
      }

      /// <summary>
      /// A value of GexportAbilityToPayParent.
      /// </summary>
      [JsonPropertyName("gexportAbilityToPayParent")]
      public Common GexportAbilityToPayParent
      {
        get => gexportAbilityToPayParent ??= new();
        set => gexportAbilityToPayParent = value;
      }

      private Common gexportParentingTime;
      private Common gexportAbilityToPayParent;
    }

    /// <summary>
    /// A value of GuidelineYearPrompt.
    /// </summary>
    [JsonPropertyName("guidelineYearPrompt")]
    public Common GuidelineYearPrompt
    {
      get => guidelineYearPrompt ??= new();
      set => guidelineYearPrompt = value;
    }

    /// <summary>
    /// A value of Required.
    /// </summary>
    [JsonPropertyName("required")]
    public Code Required
    {
      get => required ??= new();
      set => required = value;
    }

    /// <summary>
    /// A value of AuthorizingPrompt.
    /// </summary>
    [JsonPropertyName("authorizingPrompt")]
    public Common AuthorizingPrompt
    {
      get => authorizingPrompt ??= new();
      set => authorizingPrompt = value;
    }

    /// <summary>
    /// A value of FromCren.
    /// </summary>
    [JsonPropertyName("fromCren")]
    public Common FromCren
    {
      get => fromCren ??= new();
      set => fromCren = value;
    }

    /// <summary>
    /// A value of ParentBPrompt.
    /// </summary>
    [JsonPropertyName("parentBPrompt")]
    public Common ParentBPrompt
    {
      get => parentBPrompt ??= new();
      set => parentBPrompt = value;
    }

    /// <summary>
    /// A value of ParentAPrompt.
    /// </summary>
    [JsonPropertyName("parentAPrompt")]
    public Common ParentAPrompt
    {
      get => parentAPrompt ??= new();
      set => parentAPrompt = value;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public LegalAction Selected
    {
      get => selected ??= new();
      set => selected = value;
    }

    /// <summary>
    /// A value of CommandH.
    /// </summary>
    [JsonPropertyName("commandH")]
    public Common CommandH
    {
      get => commandH ??= new();
      set => commandH = value;
    }

    /// <summary>
    /// A value of County.
    /// </summary>
    [JsonPropertyName("county")]
    public CodeValue County
    {
      get => county ??= new();
      set => county = value;
    }

    /// <summary>
    /// A value of H.
    /// </summary>
    [JsonPropertyName("h")]
    public FipsTribAddress H
    {
      get => h ??= new();
      set => h = value;
    }

    /// <summary>
    /// A value of ParentBName.
    /// </summary>
    [JsonPropertyName("parentBName")]
    public CsePersonsWorkSet ParentBName
    {
      get => parentBName ??= new();
      set => parentBName = value;
    }

    /// <summary>
    /// A value of ParentAName.
    /// </summary>
    [JsonPropertyName("parentAName")]
    public CsePersonsWorkSet ParentAName
    {
      get => parentAName ??= new();
      set => parentAName = value;
    }

    /// <summary>
    /// A value of LastUpdtDate.
    /// </summary>
    [JsonPropertyName("lastUpdtDate")]
    public Code LastUpdtDate
    {
      get => lastUpdtDate ??= new();
      set => lastUpdtDate = value;
    }

    /// <summary>
    /// A value of WorkLink.
    /// </summary>
    [JsonPropertyName("workLink")]
    public Common WorkLink
    {
      get => workLink ??= new();
      set => workLink = value;
    }

    /// <summary>
    /// A value of WorkPrev.
    /// </summary>
    [JsonPropertyName("workPrev")]
    public Standard WorkPrev
    {
      get => workPrev ??= new();
      set => workPrev = value;
    }

    /// <summary>
    /// A value of PrevChildSupportWorksheet.
    /// </summary>
    [JsonPropertyName("prevChildSupportWorksheet")]
    public ChildSupportWorksheet PrevChildSupportWorksheet
    {
      get => prevChildSupportWorksheet ??= new();
      set => prevChildSupportWorksheet = value;
    }

    /// <summary>
    /// A value of Prev2.
    /// </summary>
    [JsonPropertyName("prev2")]
    public ChildSupportWorksheet Prev2
    {
      get => prev2 ??= new();
      set => prev2 = value;
    }

    /// <summary>
    /// A value of ParentBF2TotalCsAdj.
    /// </summary>
    [JsonPropertyName("parentBF2TotalCsAdj")]
    public Common ParentBF2TotalCsAdj
    {
      get => parentBF2TotalCsAdj ??= new();
      set => parentBF2TotalCsAdj = value;
    }

    /// <summary>
    /// A value of ParentAF2TotalCsAdj.
    /// </summary>
    [JsonPropertyName("parentAF2TotalCsAdj")]
    public Common ParentAF2TotalCsAdj
    {
      get => parentAF2TotalCsAdj ??= new();
      set => parentAF2TotalCsAdj = value;
    }

    /// <summary>
    /// A value of D1otalCsInc.
    /// </summary>
    [JsonPropertyName("d1otalCsInc")]
    public Common D1otalCsInc
    {
      get => d1otalCsInc ??= new();
      set => d1otalCsInc = value;
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
    /// A value of ParentBF3AdjCsOblig.
    /// </summary>
    [JsonPropertyName("parentBF3AdjCsOblig")]
    public Common ParentBF3AdjCsOblig
    {
      get => parentBF3AdjCsOblig ??= new();
      set => parentBF3AdjCsOblig = value;
    }

    /// <summary>
    /// A value of ParentBE7F2TotAdj.
    /// </summary>
    [JsonPropertyName("parentBE7F2TotAdj")]
    public Common ParentBE7F2TotAdj
    {
      get => parentBE7F2TotAdj ??= new();
      set => parentBE7F2TotAdj = value;
    }

    /// <summary>
    /// A value of ParentAF3AdjCsOblig.
    /// </summary>
    [JsonPropertyName("parentAF3AdjCsOblig")]
    public Common ParentAF3AdjCsOblig
    {
      get => parentAF3AdjCsOblig ??= new();
      set => parentAF3AdjCsOblig = value;
    }

    /// <summary>
    /// A value of ParentAE7F2TotAdj.
    /// </summary>
    [JsonPropertyName("parentAE7F2TotAdj")]
    public Common ParentAE7F2TotAdj
    {
      get => parentAE7F2TotAdj ??= new();
      set => parentAE7F2TotAdj = value;
    }

    /// <summary>
    /// A value of ParentAD9F1NetCs.
    /// </summary>
    [JsonPropertyName("parentAD9F1NetCs")]
    public Common ParentAD9F1NetCs
    {
      get => parentAD9F1NetCs ??= new();
      set => parentAD9F1NetCs = value;
    }

    /// <summary>
    /// A value of ParentBD9F1NetCs.
    /// </summary>
    [JsonPropertyName("parentBD9F1NetCs")]
    public Common ParentBD9F1NetCs
    {
      get => parentBD9F1NetCs ??= new();
      set => parentBD9F1NetCs = value;
    }

    /// <summary>
    /// A value of ParentBD8Adjustments.
    /// </summary>
    [JsonPropertyName("parentBD8Adjustments")]
    public Common ParentBD8Adjustments
    {
      get => parentBD8Adjustments ??= new();
      set => parentBD8Adjustments = value;
    }

    /// <summary>
    /// A value of ParentAD8Adjustments.
    /// </summary>
    [JsonPropertyName("parentAD8Adjustments")]
    public Common ParentAD8Adjustments
    {
      get => parentAD8Adjustments ??= new();
      set => parentAD8Adjustments = value;
    }

    /// <summary>
    /// A value of ParentBD7CsOblig.
    /// </summary>
    [JsonPropertyName("parentBD7CsOblig")]
    public Common ParentBD7CsOblig
    {
      get => parentBD7CsOblig ??= new();
      set => parentBD7CsOblig = value;
    }

    /// <summary>
    /// A value of ParentAD7CsOblig.
    /// </summary>
    [JsonPropertyName("parentAD7CsOblig")]
    public Common ParentAD7CsOblig
    {
      get => parentAD7CsOblig ??= new();
      set => parentAD7CsOblig = value;
    }

    /// <summary>
    /// A value of D6otalChildSuppOblig.
    /// </summary>
    [JsonPropertyName("d6otalChildSuppOblig")]
    public Common D6otalChildSuppOblig
    {
      get => d6otalChildSuppOblig ??= new();
      set => d6otalChildSuppOblig = value;
    }

    /// <summary>
    /// A value of TotalChildCareCost.
    /// </summary>
    [JsonPropertyName("totalChildCareCost")]
    public Common TotalChildCareCost
    {
      get => totalChildCareCost ??= new();
      set => totalChildCareCost = value;
    }

    /// <summary>
    /// A value of ParentAChildCareCost.
    /// </summary>
    [JsonPropertyName("parentAChildCareCost")]
    public Common ParentAChildCareCost
    {
      get => parentAChildCareCost ??= new();
      set => parentAChildCareCost = value;
    }

    /// <summary>
    /// A value of ParentBChildCareCost.
    /// </summary>
    [JsonPropertyName("parentBChildCareCost")]
    public Common ParentBChildCareCost
    {
      get => parentBChildCareCost ??= new();
      set => parentBChildCareCost = value;
    }

    /// <summary>
    /// A value of ParentBTotalTaxCredit.
    /// </summary>
    [JsonPropertyName("parentBTotalTaxCredit")]
    public Common ParentBTotalTaxCredit
    {
      get => parentBTotalTaxCredit ??= new();
      set => parentBTotalTaxCredit = value;
    }

    /// <summary>
    /// A value of ParentATotalTaxCredit.
    /// </summary>
    [JsonPropertyName("parentATotalTaxCredit")]
    public Common ParentATotalTaxCredit
    {
      get => parentATotalTaxCredit ??= new();
      set => parentATotalTaxCredit = value;
    }

    /// <summary>
    /// A value of TotalInsurancePrem.
    /// </summary>
    [JsonPropertyName("totalInsurancePrem")]
    public Common TotalInsurancePrem
    {
      get => totalInsurancePrem ??= new();
      set => totalInsurancePrem = value;
    }

    /// <summary>
    /// A value of CsObligTotalAmount.
    /// </summary>
    [JsonPropertyName("csObligTotalAmount")]
    public Common CsObligTotalAmount
    {
      get => csObligTotalAmount ??= new();
      set => csObligTotalAmount = value;
    }

    /// <summary>
    /// A value of CsOblig1618TotalAmt.
    /// </summary>
    [JsonPropertyName("csOblig1618TotalAmt")]
    public Common CsOblig1618TotalAmt
    {
      get => csOblig1618TotalAmt ??= new();
      set => csOblig1618TotalAmt = value;
    }

    /// <summary>
    /// A value of CsOblig715TotalAmt.
    /// </summary>
    [JsonPropertyName("csOblig715TotalAmt")]
    public Common CsOblig715TotalAmt
    {
      get => csOblig715TotalAmt ??= new();
      set => csOblig715TotalAmt = value;
    }

    /// <summary>
    /// A value of CsOblig06TotalAmt.
    /// </summary>
    [JsonPropertyName("csOblig06TotalAmt")]
    public Common CsOblig06TotalAmt
    {
      get => csOblig06TotalAmt ??= new();
      set => csOblig06TotalAmt = value;
    }

    /// <summary>
    /// A value of ParentBD2PercentInc.
    /// </summary>
    [JsonPropertyName("parentBD2PercentInc")]
    public Common ParentBD2PercentInc
    {
      get => parentBD2PercentInc ??= new();
      set => parentBD2PercentInc = value;
    }

    /// <summary>
    /// A value of ParentAD2PercentInc.
    /// </summary>
    [JsonPropertyName("parentAD2PercentInc")]
    public Common ParentAD2PercentInc
    {
      get => parentAD2PercentInc ??= new();
      set => parentAD2PercentInc = value;
    }

    /// <summary>
    /// A value of ParentBC5D1TotCsInc.
    /// </summary>
    [JsonPropertyName("parentBC5D1TotCsInc")]
    public Common ParentBC5D1TotCsInc
    {
      get => parentBC5D1TotCsInc ??= new();
      set => parentBC5D1TotCsInc = value;
    }

    /// <summary>
    /// A value of ParentAC5D1TotCsInc.
    /// </summary>
    [JsonPropertyName("parentAC5D1TotCsInc")]
    public Common ParentAC5D1TotCsInc
    {
      get => parentAC5D1TotCsInc ??= new();
      set => parentAC5D1TotCsInc = value;
    }

    /// <summary>
    /// A value of ParentBC1TotGrossInc.
    /// </summary>
    [JsonPropertyName("parentBC1TotGrossInc")]
    public Common ParentBC1TotGrossInc
    {
      get => parentBC1TotGrossInc ??= new();
      set => parentBC1TotGrossInc = value;
    }

    /// <summary>
    /// A value of ParentAC1TotGrossInc.
    /// </summary>
    [JsonPropertyName("parentAC1TotGrossInc")]
    public Common ParentAC1TotGrossInc
    {
      get => parentAC1TotGrossInc ??= new();
      set => parentAC1TotGrossInc = value;
    }

    /// <summary>
    /// A value of ParentBB3SeGrossInc.
    /// </summary>
    [JsonPropertyName("parentBB3SeGrossInc")]
    public Common ParentBB3SeGrossInc
    {
      get => parentBB3SeGrossInc ??= new();
      set => parentBB3SeGrossInc = value;
    }

    /// <summary>
    /// A value of ParentAB3SeGrossInc.
    /// </summary>
    [JsonPropertyName("parentAB3SeGrossInc")]
    public Common ParentAB3SeGrossInc
    {
      get => parentAB3SeGrossInc ??= new();
      set => parentAB3SeGrossInc = value;
    }

    /// <summary>
    /// A value of ChildSupportWorksheet.
    /// </summary>
    [JsonPropertyName("childSupportWorksheet")]
    public ChildSupportWorksheet ChildSupportWorksheet
    {
      get => childSupportWorksheet ??= new();
      set => childSupportWorksheet = value;
    }

    /// <summary>
    /// A value of ParentBCsePersonSupportWorksheet.
    /// </summary>
    [JsonPropertyName("parentBCsePersonSupportWorksheet")]
    public CsePersonSupportWorksheet ParentBCsePersonSupportWorksheet
    {
      get => parentBCsePersonSupportWorksheet ??= new();
      set => parentBCsePersonSupportWorksheet = value;
    }

    /// <summary>
    /// A value of PrevParentBCsePersonSupportWorksheet.
    /// </summary>
    [JsonPropertyName("prevParentBCsePersonSupportWorksheet")]
    public CsePersonSupportWorksheet PrevParentBCsePersonSupportWorksheet
    {
      get => prevParentBCsePersonSupportWorksheet ??= new();
      set => prevParentBCsePersonSupportWorksheet = value;
    }

    /// <summary>
    /// A value of ParentACsePersonSupportWorksheet.
    /// </summary>
    [JsonPropertyName("parentACsePersonSupportWorksheet")]
    public CsePersonSupportWorksheet ParentACsePersonSupportWorksheet
    {
      get => parentACsePersonSupportWorksheet ??= new();
      set => parentACsePersonSupportWorksheet = value;
    }

    /// <summary>
    /// A value of PrevParentACsePersonSupportWorksheet.
    /// </summary>
    [JsonPropertyName("prevParentACsePersonSupportWorksheet")]
    public CsePersonSupportWorksheet PrevParentACsePersonSupportWorksheet
    {
      get => prevParentACsePersonSupportWorksheet ??= new();
      set => prevParentACsePersonSupportWorksheet = value;
    }

    /// <summary>
    /// A value of ParentBCsePerson.
    /// </summary>
    [JsonPropertyName("parentBCsePerson")]
    public CsePerson ParentBCsePerson
    {
      get => parentBCsePerson ??= new();
      set => parentBCsePerson = value;
    }

    /// <summary>
    /// A value of PrevParentBCsePerson.
    /// </summary>
    [JsonPropertyName("prevParentBCsePerson")]
    public CsePerson PrevParentBCsePerson
    {
      get => prevParentBCsePerson ??= new();
      set => prevParentBCsePerson = value;
    }

    /// <summary>
    /// A value of ParentACsePerson.
    /// </summary>
    [JsonPropertyName("parentACsePerson")]
    public CsePerson ParentACsePerson
    {
      get => parentACsePerson ??= new();
      set => parentACsePerson = value;
    }

    /// <summary>
    /// A value of PrevParentACsePerson.
    /// </summary>
    [JsonPropertyName("prevParentACsePerson")]
    public CsePerson PrevParentACsePerson
    {
      get => prevParentACsePerson ??= new();
      set => prevParentACsePerson = value;
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
    /// A value of PrevCase.
    /// </summary>
    [JsonPropertyName("prevCase")]
    public Case1 PrevCase
    {
      get => prevCase ??= new();
      set => prevCase = value;
    }

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
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
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Case1 Next
    {
      get => next ??= new();
      set => next = value;
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
    /// A value of CaseOpen.
    /// </summary>
    [JsonPropertyName("caseOpen")]
    public Common CaseOpen
    {
      get => caseOpen ??= new();
      set => caseOpen = value;
    }

    /// <summary>
    /// A value of CaseRoleInactive.
    /// </summary>
    [JsonPropertyName("caseRoleInactive")]
    public Common CaseRoleInactive
    {
      get => caseRoleInactive ??= new();
      set => caseRoleInactive = value;
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
    /// A value of ParentAD10F1NetCs.
    /// </summary>
    [JsonPropertyName("parentAD10F1NetCs")]
    public Common ParentAD10F1NetCs
    {
      get => parentAD10F1NetCs ??= new();
      set => parentAD10F1NetCs = value;
    }

    /// <summary>
    /// A value of ParentBD10F1NetCs.
    /// </summary>
    [JsonPropertyName("parentBD10F1NetCs")]
    public Common ParentBD10F1NetCs
    {
      get => parentBD10F1NetCs ??= new();
      set => parentBD10F1NetCs = value;
    }

    /// <summary>
    /// A value of ParentAEnfFee.
    /// </summary>
    [JsonPropertyName("parentAEnfFee")]
    public Common ParentAEnfFee
    {
      get => parentAEnfFee ??= new();
      set => parentAEnfFee = value;
    }

    /// <summary>
    /// A value of ParentBEnfFee.
    /// </summary>
    [JsonPropertyName("parentBEnfFee")]
    public Common ParentBEnfFee
    {
      get => parentBEnfFee ??= new();
      set => parentBEnfFee = value;
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
    /// Gets a value of Gexport2020.
    /// </summary>
    [JsonPropertyName("gexport2020")]
    public Gexport2020Group Gexport2020
    {
      get => gexport2020 ?? (gexport2020 = new());
      set => gexport2020 = value;
    }

    /// <summary>
    /// Gets a value of Gexport2020EnterableFields.
    /// </summary>
    [JsonPropertyName("gexport2020EnterableFields")]
    public Gexport2020EnterableFieldsGroup Gexport2020EnterableFields
    {
      get => gexport2020EnterableFields ?? (gexport2020EnterableFields = new());
      set => gexport2020EnterableFields = value;
    }

    /// <summary>
    /// A value of GuidelineYearChange.
    /// </summary>
    [JsonPropertyName("guidelineYearChange")]
    public Common GuidelineYearChange
    {
      get => guidelineYearChange ??= new();
      set => guidelineYearChange = value;
    }

    private Common guidelineYearPrompt;
    private Code required;
    private Common authorizingPrompt;
    private Common fromCren;
    private Common parentBPrompt;
    private Common parentAPrompt;
    private LegalAction selected;
    private Common commandH;
    private CodeValue county;
    private FipsTribAddress h;
    private CsePersonsWorkSet parentBName;
    private CsePersonsWorkSet parentAName;
    private Code lastUpdtDate;
    private Common workLink;
    private Standard workPrev;
    private ChildSupportWorksheet prevChildSupportWorksheet;
    private ChildSupportWorksheet prev2;
    private Common parentBF2TotalCsAdj;
    private Common parentAF2TotalCsAdj;
    private Common d1otalCsInc;
    private Array<ExportGroup> export1;
    private Common parentBF3AdjCsOblig;
    private Common parentBE7F2TotAdj;
    private Common parentAF3AdjCsOblig;
    private Common parentAE7F2TotAdj;
    private Common parentAD9F1NetCs;
    private Common parentBD9F1NetCs;
    private Common parentBD8Adjustments;
    private Common parentAD8Adjustments;
    private Common parentBD7CsOblig;
    private Common parentAD7CsOblig;
    private Common d6otalChildSuppOblig;
    private Common totalChildCareCost;
    private Common parentAChildCareCost;
    private Common parentBChildCareCost;
    private Common parentBTotalTaxCredit;
    private Common parentATotalTaxCredit;
    private Common totalInsurancePrem;
    private Common csObligTotalAmount;
    private Common csOblig1618TotalAmt;
    private Common csOblig715TotalAmt;
    private Common csOblig06TotalAmt;
    private Common parentBD2PercentInc;
    private Common parentAD2PercentInc;
    private Common parentBC5D1TotCsInc;
    private Common parentAC5D1TotCsInc;
    private Common parentBC1TotGrossInc;
    private Common parentAC1TotGrossInc;
    private Common parentBB3SeGrossInc;
    private Common parentAB3SeGrossInc;
    private ChildSupportWorksheet childSupportWorksheet;
    private CsePersonSupportWorksheet parentBCsePersonSupportWorksheet;
    private CsePersonSupportWorksheet prevParentBCsePersonSupportWorksheet;
    private CsePersonSupportWorksheet parentACsePersonSupportWorksheet;
    private CsePersonSupportWorksheet prevParentACsePersonSupportWorksheet;
    private CsePerson parentBCsePerson;
    private CsePerson prevParentBCsePerson;
    private CsePerson parentACsePerson;
    private CsePerson prevParentACsePerson;
    private Case1 case1;
    private Case1 prevCase;
    private Tribunal tribunal;
    private LegalAction legalAction;
    private NextTranInfo hidden;
    private Standard standard;
    private Case1 next;
    private ServiceProvider serviceProvider;
    private Office office;
    private Common caseOpen;
    private Common caseRoleInactive;
    private Common common;
    private Common parentAD10F1NetCs;
    private Common parentBD10F1NetCs;
    private Common parentAEnfFee;
    private Common parentBEnfFee;
    private WorkArea headerLine;
    private Gexport2020Group gexport2020;
    private Gexport2020EnterableFieldsGroup gexport2020EnterableFields;
    private Common guidelineYearChange;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A LocalGroup group.</summary>
    [Serializable]
    public class LocalGroup
    {
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
      /// A value of ParentB.
      /// </summary>
      [JsonPropertyName("parentB")]
      public CsePersonSupportAdjustment ParentB
      {
        get => parentB ??= new();
        set => parentB = value;
      }

      /// <summary>
      /// A value of ParentA.
      /// </summary>
      [JsonPropertyName("parentA")]
      public CsePersonSupportAdjustment ParentA
      {
        get => parentA ??= new();
        set => parentA = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private Common work;
      private CsePersonSupportAdjustment parentB;
      private CsePersonSupportAdjustment parentA;
    }

    /// <summary>
    /// Gets a value of Local1.
    /// </summary>
    [JsonIgnore]
    public Array<LocalGroup> Local1 => local1 ??= new(LocalGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Local1 for json serialization.
    /// </summary>
    [JsonPropertyName("local1")]
    [Computed]
    public IList<LocalGroup> Local1_Json
    {
      get => local1;
      set => Local1.Assign(value);
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
    /// A value of FirstNameLength.
    /// </summary>
    [JsonPropertyName("firstNameLength")]
    public Common FirstNameLength
    {
      get => firstNameLength ??= new();
      set => firstNameLength = value;
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
    /// A value of CurrentGuidelineYear.
    /// </summary>
    [JsonPropertyName("currentGuidelineYear")]
    public DateWorkArea CurrentGuidelineYear
    {
      get => currentGuidelineYear ??= new();
      set => currentGuidelineYear = value;
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
    /// A value of SpDocLiteral.
    /// </summary>
    [JsonPropertyName("spDocLiteral")]
    public SpDocLiteral SpDocLiteral
    {
      get => spDocLiteral ??= new();
      set => spDocLiteral = value;
    }

    /// <summary>
    /// A value of BatchConvertNumToText.
    /// </summary>
    [JsonPropertyName("batchConvertNumToText")]
    public BatchConvertNumToText BatchConvertNumToText
    {
      get => batchConvertNumToText ??= new();
      set => batchConvertNumToText = value;
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
    /// A value of Position.
    /// </summary>
    [JsonPropertyName("position")]
    public Common Position
    {
      get => position ??= new();
      set => position = value;
    }

    /// <summary>
    /// A value of Display.
    /// </summary>
    [JsonPropertyName("display")]
    public Common Display
    {
      get => display ??= new();
      set => display = value;
    }

    /// <summary>
    /// A value of CaseRoleInactive.
    /// </summary>
    [JsonPropertyName("caseRoleInactive")]
    public Common CaseRoleInactive
    {
      get => caseRoleInactive ??= new();
      set => caseRoleInactive = value;
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
    /// A value of WorkError.
    /// </summary>
    [JsonPropertyName("workError")]
    public Common WorkError
    {
      get => workError ??= new();
      set => workError = value;
    }

    /// <summary>
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public Common Error
    {
      get => error ??= new();
      set => error = value;
    }

    private Array<LocalGroup> local1;
    private DateWorkArea current;
    private Common firstNameLength;
    private Common common;
    private DateWorkArea currentGuidelineYear;
    private NextTranInfo null1;
    private SpDocLiteral spDocLiteral;
    private BatchConvertNumToText batchConvertNumToText;
    private WorkArea workArea;
    private Common position;
    private Common display;
    private Common caseRoleInactive;
    private TextWorkArea textWorkArea;
    private Document document;
    private Common workError;
    private Common error;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ChildSupportWorksheet.
    /// </summary>
    [JsonPropertyName("childSupportWorksheet")]
    public ChildSupportWorksheet ChildSupportWorksheet
    {
      get => childSupportWorksheet ??= new();
      set => childSupportWorksheet = value;
    }

    /// <summary>
    /// A value of Zxx.
    /// </summary>
    [JsonPropertyName("zxx")]
    public LegalAction Zxx
    {
      get => zxx ??= new();
      set => zxx = value;
    }

    private Code code;
    private CodeValue codeValue;
    private ChildSupportWorksheet childSupportWorksheet;
    private LegalAction zxx;
  }
#endregion
}
