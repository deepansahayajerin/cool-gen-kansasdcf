// Program: OE_WXR3_PREV_YR_CS_WS_-574380401, ID: 1625360974, model: 746.
// Short name: SWEWXR3P
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
/// A program: OE_WXR3_PREV_YR_CS_WS_-574380401.
/// </para>
/// <para>
/// Resp - OBLGESTB
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OeWxr3PrevYrCsWs574380401: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_WXR3_PREV_YR_CS_WS_-574380401 program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeWxr3PrevYrCsWs574380401(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeWxr3PrevYrCsWs574380401.
  /// </summary>
  public OeWxr3PrevYrCsWs574380401(IContext context, Import import,
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
    // ---------------------------------------------------------------
    //        M A I N T E N A N C E   L O G
    // Date	Developer	request #	Description
    // 03/03/95 Sid Chowdhary			Initial Development
    // 03/03/96 Sid Chowdhary			Retrofit and Testing
    // 11/25/96 R. Marchman			Add new security and next tran
    // 02/26/01 M. Kumar	PR #  113201   	Rounding the percentage amount 
    // corrected.
    // 03/15/04 A. Convery	PR#198090/201861 Changed required entry of "A" or "B"
    // on page 3
    // 					of the WORK screen to "M" and "F".
    // 11/19/07 M. Fan 	WR318566(CQ297)	Required age groups changed. Changed to 
    // use generic names for age groups
    // 					and added the parenting time adjustment percent to imports and 
    // exports
    // 					for child support worksheet entity view.
    // 06/04/10 J. Huss	CQ# 18769	Reduced group_import and group_export size 
    // from 7 to 6.
    // 03/20/12  A Hockman	sr#16297	Changes to allow for multiple years of
    // 					guidelines.  Also added GL Year to screen.
    // 12/09/15 GVandy		CQ50299		Change enforcement fee values from M/F to 1/2.
    // ---------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // **** begin group A ****
    export.Hidden.Assign(import.Hidden);

    // **** end   group A ****
    // ---------------------------------------------
    // This is a 3 procedure step PRAD. The views
    // contain the details of all the 3 screens. The
    // PSADs are connected through two links and all
    // the data are passed through all the screens.
    // All the data not belonging to a particular
    // screen are kept in the hidden views.
    // ---------------------------------------------
    // ---------------------------------------------
    //        Move all IMPORTs to EXPORTs.
    // ---------------------------------------------
    export.Case1.Number = import.Case1.Number;
    export.PrevCase.Number = import.PrevCase.Number;
    export.ChildSupportWorksheet.Assign(import.ChildSupportWorksheet);
    MoveChildSupportWorksheet2(import.PrevChildSupportWorksheet,
      export.PrevChildSupportWorksheet);
    MoveLegalAction(import.LegalAction, export.LegalAction);
    export.Tribunal.JudicialDistrict = import.Tribunal.JudicialDistrict;
    export.County.Description = import.County.Description;
    export.ParentACsePerson.Number = import.ParentACsePerson.Number;
    export.ParentBCsePerson.Number = import.ParentBCsePerson.Number;
    MoveCsePersonsWorkSet(import.ParentAName, export.ParentAName);
    MoveCsePersonsWorkSet(import.ParentBName, export.ParentBName);
    export.PrevParentA.Number = import.PrevParentA.Number;
    export.PrevParentB.Number = import.PrevParentB.Number;
    export.Common.Assign(import.Common);
    export.ParentBCsePersonSupportWorksheet.Assign(
      import.ParentBCsePersonSupportWorksheet);
    export.ParentACsePersonSupportWorksheet.Assign(
      import.ParentACsePersonSupportWorksheet);
    export.LastUpdtDate.EffectiveDate = import.LastUpdtDate.EffectiveDate;
    export.CsObligAgrp1TotalAmt.TotalCurrency =
      import.CsObligAgrp1TotalAmt.TotalCurrency;
    export.CsObligAgrp3TotalAmt.TotalCurrency =
      import.CsObligAgrp3TotalInc.TotalCurrency;
    export.CsObligAgrp2TotalAmt.TotalCurrency =
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
    export.ParentAD10F1NetCs.TotalCurrency =
      import.ParentAD10F1NetCs.TotalCurrency;
    export.ParentBD10F1NetCs.TotalCurrency =
      import.ParentBD10F1NetCs.TotalCurrency;
    export.ParentAEnfFee.TotalCurrency = import.ParentAEnfFee.TotalCurrency;
    export.ParentBEnfFee.TotalCurrency = import.ParentBEnfFee.TotalCurrency;
    export.CaseOpen.Flag = import.CaseOpen.Flag;
    export.CaseRoleInactive.Flag = import.CaseRoleInactive.Flag;
    export.Next.Number = import.Next.Number;
    export.ServiceProvider.LastName = import.ServiceProvider.LastName;
    MoveOffice(import.Office, export.Office);
    export.FromCren.Flag = import.FromCren.Flag;

    // ---------------------------------------------
    //   Move all Group IMPORTs to Group EXPORTs.
    // ---------------------------------------------
    for(import.Import1.Index = 0; import.Import1.Index < Import
      .ImportGroup.Capacity; ++import.Import1.Index)
    {
      if (!import.Import1.CheckSize())
      {
        break;
      }

      ++local.Subscript.Count;

      export.Export1.Index = local.Subscript.Count - 1;
      export.Export1.CheckSize();

      export.Export1.Update.ParentA.AdjustmentAmount =
        import.Import1.Item.ParentA.AdjustmentAmount;
      export.Export1.Update.ParentB.AdjustmentAmount =
        import.Import1.Item.ParentB.AdjustmentAmount;
    }

    import.Import1.CheckIndex();

    // **** begin group B ****
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

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();
      global.Command = "DISPLAY";

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";

      return;
    }

    // **** end   group B ****
    // ---------------------------------------------
    // When comming from one of the other pages,
    // move imports to exports and display the
    // screen.
    // ---------------------------------------------
    if (Equal(global.Command, "DISPPAGE"))
    {
      ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";

      if (AsChar(export.CaseOpen.Flag) == 'N')
      {
        ExitState = "DISPLAY_OK_FOR_CLOSED_CASE";
      }

      local.ParentAEnfFee.TotalInteger =
        (long)Math.Round(
          export.ParentAEnfFee.TotalCurrency, MidpointRounding.AwayFromZero);
      local.ParentBEnfFee.TotalInteger =
        (long)Math.Round(
          export.ParentBEnfFee.TotalCurrency, MidpointRounding.AwayFromZero);
      export.ParentAEnfFee.TotalCurrency = local.ParentAEnfFee.TotalInteger;
      export.ParentBEnfFee.TotalCurrency = local.ParentBEnfFee.TotalInteger;
      local.ParentAD10F1NetCs.TotalInteger =
        (long)Math.Round(
          export.ParentAD10F1NetCs.TotalCurrency,
        MidpointRounding.AwayFromZero);
      local.ParentBD10F1NetCs.TotalInteger =
        (long)Math.Round(
          export.ParentBD10F1NetCs.TotalCurrency,
        MidpointRounding.AwayFromZero);
      export.ParentAD10F1NetCs.TotalCurrency =
        local.ParentAD10F1NetCs.TotalInteger;
      export.ParentBD10F1NetCs.TotalCurrency =
        local.ParentBD10F1NetCs.TotalInteger;

      return;
    }

    // **** begin group C ****
    // to validate action level security
    UseScCabTestSecurity();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    // **** end   group C ****
    // 11/21/07    M. Fan       WR318566(CQ297)- Added the parenting time 
    // adjustment percent validation.
    if (export.ChildSupportWorksheet.ParentingTimeAdjPercent.
      GetValueOrDefault() > 100)
    {
      var field =
        GetField(export.ChildSupportWorksheet, "parentingTimeAdjPercent");

      field.Error = true;

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        ExitState = "FN0012_PCT_GT_100";
      }
    }

    // **** begin group D ****
    // ****
    // for the cases where you link from 1 procedure to another procedure, you 
    // must set the
    //  export_hidden security link_indicator to "L". This will tell the called 
    // procedure that
    //  we are on a link and not a transfer.  Don't forget to do the view 
    // matching on the dialog design screen.
    // ****
    // **** end   group D ****
    // PR#198090/201861 Changed required entry of "A" or "B" to "M" or "F".
    //                  Also changed text of error message in EXIT STATE
    //                  oe0180_wksht3_parent_a_b.
    //                  04/03/15 - Andrew Convery
    // @@@  change M/F to 1/2
    if (AsChar(export.Common.SelectChar) == '1' || AsChar
      (export.Common.SelectChar) == '2' || IsEmpty(export.Common.SelectChar))
    {
    }
    else
    {
      var field = GetField(export.Common, "selectChar");

      field.Error = true;

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        ExitState = "OE0000_WKSHT3_PARENT_1_2";
      }
    }

    if ((export.Common.Percentage > 0 || export.Common.TotalCurrency > 0) && IsEmpty
      (export.Common.SelectChar))
    {
      var field = GetField(export.Common, "selectChar");

      field.Error = true;

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        ExitState = "OE0180_WKSHT3_PARENT_A_B";
      }
    }

    if (export.Common.Percentage > 100)
    {
      var field = GetField(export.Common, "percentage");

      field.Error = true;

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        ExitState = "FN0012_PCT_GT_100";
      }
    }

    // @@@  change M/F to 1/2
    if ((AsChar(export.Common.SelectChar) == '1' || AsChar
      (export.Common.SelectChar) == '2') && export.Common.Percentage == 0 && export
      .Common.TotalCurrency == 0)
    {
      var field1 = GetField(export.Common, "percentage");

      field1.Error = true;

      var field2 = GetField(export.Common, "totalCurrency");

      field2.Error = true;

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        ExitState = "OE0014_MANDATORY_FIELD_MISSING";
      }
    }

    if (export.Common.Percentage > 0 && export.Common.TotalCurrency > 0)
    {
      var field1 = GetField(export.Common, "percentage");

      field1.Error = true;

      var field2 = GetField(export.Common, "totalCurrency");

      field2.Error = true;

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        ExitState = "OE0179_WKSHT3_PRNT_FLAT_FEE";
      }
    }

    // ---------------------------------------------
    //     P F K E Y    P R O C E S S I N G
    // ---------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "SIGNOFF":
        // **** begin group F ****
        UseScCabSignoff();

        // **** end   group F ****
        break;
      case "NEXT":
        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (AsChar(import.FromCren.Flag) == 'Y')
          {
            ExitState = "OE0185_WORKSHEET_FROM_CREN";
          }
          else
          {
            ExitState = "ECO_LNK_TO_CS_WORKSHEET_2";
            global.Command = "PAGE1";
          }
        }

        break;
      case "HELP":
        ExitState = "ACO_NI0000_HELP_NOT_AVAILABLE";

        break;
      case "PREV":
        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ECO_LNK_TO_CS_WORKSHEET_2";
          global.Command = "DISPPAGE";
        }

        break;
      case "CALCULAT":
        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseOeWorkCalcChildSupWorksheet();

          if (AsChar(export.CaseOpen.Flag) == 'N')
          {
            ExitState = "DISPLAY_OK_FOR_CLOSED_CASE";
          }

          local.ParentAEnfFee.TotalInteger =
            (long)Math.Round(
              export.ParentAEnfFee.TotalCurrency,
            MidpointRounding.AwayFromZero);
          local.ParentBEnfFee.TotalInteger =
            (long)Math.Round(
              export.ParentBEnfFee.TotalCurrency,
            MidpointRounding.AwayFromZero);
          export.ParentAEnfFee.TotalCurrency = local.ParentAEnfFee.TotalInteger;
          export.ParentBEnfFee.TotalCurrency = local.ParentBEnfFee.TotalInteger;
          local.ParentAD10F1NetCs.TotalInteger =
            (long)Math.Round(
              export.ParentAD10F1NetCs.TotalCurrency,
            MidpointRounding.AwayFromZero);
          local.ParentBD10F1NetCs.TotalInteger =
            (long)Math.Round(
              export.ParentBD10F1NetCs.TotalCurrency,
            MidpointRounding.AwayFromZero);
          export.ParentAD10F1NetCs.TotalCurrency =
            local.ParentAD10F1NetCs.TotalInteger;
          export.ParentBD10F1NetCs.TotalCurrency =
            local.ParentBD10F1NetCs.TotalInteger;
        }

        break;
      default:
        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NE0000_INVALID_PF_KEY";
        }

        break;
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
  }

  private static void MoveCsePersonSupportWorksheet(
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
    target.EnforcementFeeType = source.EnforcementFeeType;
    target.EnforcementFeeAllowance = source.EnforcementFeeAllowance;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.FirstName = source.FirstName;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveExport1ToImport1(Export.ExportGroup source,
    OeWorkCalcChildSupWorksheet.Import.ImportGroup target)
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

  private void UseOeWorkCalcChildSupWorksheet()
  {
    var useImport = new OeWorkCalcChildSupWorksheet.Import();
    var useExport = new OeWorkCalcChildSupWorksheet.Export();

    useImport.Common.Assign(import.Common);
    export.Export1.CopyTo(useImport.Import1, MoveExport1ToImport1);
    useImport.ChildSupportWorksheet.Assign(export.ChildSupportWorksheet);
    MoveCsePersonSupportWorksheet(export.ParentBCsePersonSupportWorksheet,
      useImport.ParentB);
    MoveCsePersonSupportWorksheet(export.ParentACsePersonSupportWorksheet,
      useImport.ParentA);

    Call(OeWorkCalcChildSupWorksheet.Execute, useImport, useExport);

    export.ParentAEnfFee.TotalCurrency = useExport.ParentAEnfFee.TotalCurrency;
    export.ParentBEnfFee.TotalCurrency = useExport.ParentBEnfFee.TotalCurrency;
    export.ParentAD10F1NetCs.TotalCurrency =
      useExport.ParentAD10F1NetCs.TotalCurrency;
    export.ParentBD10F1NetCs.TotalCurrency =
      useExport.ParentBD10F1NetCs.TotalCurrency;
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
    export.CsObligAgrp3TotalAmt.TotalCurrency =
      useExport.CsOblig1618TotalAmt.TotalCurrency;
    export.CsObligAgrp2TotalAmt.TotalCurrency =
      useExport.CsOblig715TotalAmt.TotalCurrency;
    export.CsObligAgrp1TotalAmt.TotalCurrency =
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
    export.ParentAF2TotalCsAdj.TotalCurrency =
      useExport.ParentAF2TotalCsAdj.TotalCurrency;
    export.ParentBF2TotalCsAdj.TotalCurrency =
      useExport.ParentBF2TotalCsAdj.TotalCurrency;
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

    useImport.CsePerson.Number = import.ParentACsePerson.Number;
    useImport.Case1.Number = import.Case1.Number;
    MoveLegalAction(import.LegalAction, useImport.LegalAction);

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
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
    /// A value of D1otalCsInc.
    /// </summary>
    [JsonPropertyName("d1otalCsInc")]
    public Common D1otalCsInc
    {
      get => d1otalCsInc ??= new();
      set => d1otalCsInc = value;
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
    /// A value of ParentACsePersonSupportWorksheet.
    /// </summary>
    [JsonPropertyName("parentACsePersonSupportWorksheet")]
    public CsePersonSupportWorksheet ParentACsePersonSupportWorksheet
    {
      get => parentACsePersonSupportWorksheet ??= new();
      set => parentACsePersonSupportWorksheet = value;
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
    /// A value of PrevParentB.
    /// </summary>
    [JsonPropertyName("prevParentB")]
    public CsePerson PrevParentB
    {
      get => prevParentB ??= new();
      set => prevParentB = value;
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
    /// A value of PrevParentA.
    /// </summary>
    [JsonPropertyName("prevParentA")]
    public CsePerson PrevParentA
    {
      get => prevParentA ??= new();
      set => prevParentA = value;
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
    /// A value of ParentAF2TotalCsAdj.
    /// </summary>
    [JsonPropertyName("parentAF2TotalCsAdj")]
    public Common ParentAF2TotalCsAdj
    {
      get => parentAF2TotalCsAdj ??= new();
      set => parentAF2TotalCsAdj = value;
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
    /// A value of LastUpdtDate.
    /// </summary>
    [JsonPropertyName("lastUpdtDate")]
    public Code LastUpdtDate
    {
      get => lastUpdtDate ??= new();
      set => lastUpdtDate = value;
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
    /// A value of ParentBName.
    /// </summary>
    [JsonPropertyName("parentBName")]
    public CsePersonsWorkSet ParentBName
    {
      get => parentBName ??= new();
      set => parentBName = value;
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
    /// A value of FromCren.
    /// </summary>
    [JsonPropertyName("fromCren")]
    public Common FromCren
    {
      get => fromCren ??= new();
      set => fromCren = value;
    }

    private Common parentAEnfFee;
    private Common parentBEnfFee;
    private Common parentAD10F1NetCs;
    private Common parentBD10F1NetCs;
    private Common common;
    private ChildSupportWorksheet prevChildSupportWorksheet;
    private Common d1otalCsInc;
    private Array<ImportGroup> import1;
    private Common parentBF3AdjCsOblig;
    private Common parentBE7F2TotAdjust;
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
    private Common parentBC5D1TotCsInc;
    private Common parentAC5D1TotCsInc;
    private Common parentBC1TotGrossInc;
    private Common parentAC1TotGrossInc;
    private Common parentBB3SeGrossInc;
    private Common parentAB3SeGrossInc;
    private ChildSupportWorksheet childSupportWorksheet;
    private CsePersonSupportWorksheet parentBCsePersonSupportWorksheet;
    private CsePersonSupportWorksheet parentACsePersonSupportWorksheet;
    private CsePerson parentBCsePerson;
    private CsePerson prevParentB;
    private CsePerson parentACsePerson;
    private CsePerson prevParentA;
    private Case1 case1;
    private Case1 prevCase;
    private Tribunal tribunal;
    private LegalAction legalAction;
    private Common parentAF2TotalCsAdj;
    private Common parentBF2TotalCsAdj;
    private Code lastUpdtDate;
    private CsePersonsWorkSet parentAName;
    private CsePersonsWorkSet parentBName;
    private CodeValue county;
    private NextTranInfo hidden;
    private Standard standard;
    private Case1 next;
    private ServiceProvider serviceProvider;
    private Office office;
    private Common caseOpen;
    private Common caseRoleInactive;
    private Common fromCren;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
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
    /// A value of CsObligAgrp3TotalAmt.
    /// </summary>
    [JsonPropertyName("csObligAgrp3TotalAmt")]
    public Common CsObligAgrp3TotalAmt
    {
      get => csObligAgrp3TotalAmt ??= new();
      set => csObligAgrp3TotalAmt = value;
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
    /// A value of ParentACsePersonSupportWorksheet.
    /// </summary>
    [JsonPropertyName("parentACsePersonSupportWorksheet")]
    public CsePersonSupportWorksheet ParentACsePersonSupportWorksheet
    {
      get => parentACsePersonSupportWorksheet ??= new();
      set => parentACsePersonSupportWorksheet = value;
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
    /// A value of PrevParentB.
    /// </summary>
    [JsonPropertyName("prevParentB")]
    public CsePerson PrevParentB
    {
      get => prevParentB ??= new();
      set => prevParentB = value;
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
    /// A value of PrevParentA.
    /// </summary>
    [JsonPropertyName("prevParentA")]
    public CsePerson PrevParentA
    {
      get => prevParentA ??= new();
      set => prevParentA = value;
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
    /// A value of ParentAF2TotalCsAdj.
    /// </summary>
    [JsonPropertyName("parentAF2TotalCsAdj")]
    public Common ParentAF2TotalCsAdj
    {
      get => parentAF2TotalCsAdj ??= new();
      set => parentAF2TotalCsAdj = value;
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
    /// A value of LastUpdtDate.
    /// </summary>
    [JsonPropertyName("lastUpdtDate")]
    public Code LastUpdtDate
    {
      get => lastUpdtDate ??= new();
      set => lastUpdtDate = value;
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
    /// A value of ParentBName.
    /// </summary>
    [JsonPropertyName("parentBName")]
    public CsePersonsWorkSet ParentBName
    {
      get => parentBName ??= new();
      set => parentBName = value;
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
    /// A value of FromCren.
    /// </summary>
    [JsonPropertyName("fromCren")]
    public Common FromCren
    {
      get => fromCren ??= new();
      set => fromCren = value;
    }

    private Common parentAEnfFee;
    private Common parentBEnfFee;
    private Common parentAD10F1NetCs;
    private Common parentBD10F1NetCs;
    private Common common;
    private ChildSupportWorksheet prevChildSupportWorksheet;
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
    private Common csObligAgrp3TotalAmt;
    private Common csObligAgrp2TotalAmt;
    private Common csObligAgrp1TotalAmt;
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
    private CsePersonSupportWorksheet parentACsePersonSupportWorksheet;
    private CsePerson parentBCsePerson;
    private CsePerson prevParentB;
    private CsePerson parentACsePerson;
    private CsePerson prevParentA;
    private Case1 case1;
    private Case1 prevCase;
    private Tribunal tribunal;
    private LegalAction legalAction;
    private Common parentAF2TotalCsAdj;
    private Common parentBF2TotalCsAdj;
    private Code lastUpdtDate;
    private CsePersonsWorkSet parentAName;
    private CsePersonsWorkSet parentBName;
    private CodeValue county;
    private NextTranInfo hidden;
    private Standard standard;
    private Case1 next;
    private ServiceProvider serviceProvider;
    private Office office;
    private Common caseOpen;
    private Common caseRoleInactive;
    private Common fromCren;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of ParentAD10F1NetCs.
    /// </summary>
    [JsonPropertyName("parentAD10F1NetCs")]
    public Common ParentAD10F1NetCs
    {
      get => parentAD10F1NetCs ??= new();
      set => parentAD10F1NetCs = value;
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
    /// A value of ParentAEnfFee.
    /// </summary>
    [JsonPropertyName("parentAEnfFee")]
    public Common ParentAEnfFee
    {
      get => parentAEnfFee ??= new();
      set => parentAEnfFee = value;
    }

    /// <summary>
    /// A value of Subscript.
    /// </summary>
    [JsonPropertyName("subscript")]
    public Common Subscript
    {
      get => subscript ??= new();
      set => subscript = value;
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

    private Common parentBD10F1NetCs;
    private Common parentAD10F1NetCs;
    private Common parentBEnfFee;
    private Common parentAEnfFee;
    private Common subscript;
    private Common workError;
  }
#endregion
}
