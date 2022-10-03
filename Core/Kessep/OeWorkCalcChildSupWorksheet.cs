// Program: OE_WORK_CALC_CHILD_SUP_WORKSHEET, ID: 371897918, model: 746.
// Short name: SWE00976
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_WORK_CALC_CHILD_SUP_WORKSHEET.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// WRITTEN BY:SID
/// </para>
/// </summary>
[Serializable]
public partial class OeWorkCalcChildSupWorksheet: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_WORK_CALC_CHILD_SUP_WORKSHEET program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeWorkCalcChildSupWorksheet(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeWorkCalcChildSupWorksheet.
  /// </summary>
  public OeWorkCalcChildSupWorksheet(IContext context, Import import,
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
    // -------------------------------------------------------------------------
    // Date      Author          Description
    // 03/03/95  Sid           Initial Creation
    // 03/27/95  Sid         Changes in the rounding
    //           off values. All amount are to be
    // 	  rounded off to the Dollar i.e. no
    // 	  decimals and all percentages are to
    // 	  rounded off to one decimal place.
    // 	  Local views are used for this.
    // 4/29/97	  SHERAZ	CHANGE CURRENT_DATE
    // 3/15/04    Andrew Convery  PR#198090/201861
    //            Changed required entry of "A" or "B" on page 3
    //            of the WORK screen to "M" and "F".
    // 4/4/12  A Hockman  changes to deal with multiple guideline
    //              years being stored.  SR 16297
    // 12/9/15  GVandy  Change Enforcement Fee values from M/F to 1/2. CQ50299
    // 11/6/19  GVandy  2020 Worksheet Changes        CQ66067
    // 1/9/20   GVandy  Additional 2020 Modifications  CQ67085
    // -------------------------------------------------------------------------
    // -------------------------------------------------------------------------
    // This action block is used to calculate the Child Support Obligation.
    // -------------------------------------------------------------------------
    local.Current.Date = Now().Date;
    ExitState = "ACO_NN0000_ALL_OK";

    // ---------------------------------------------
    //        Move all IMPORTs to EXPORTs.
    // ---------------------------------------------
    export.ChildSupportWorksheet.Assign(import.ChildSupportWorksheet);
    export.ParentB.Assign(import.ParentB);
    export.ParentA.Assign(import.ParentA);

    if (import.ChildSupportWorksheet.CsGuidelineYear >= 2020)
    {
      // ---------------------------------------------
      // Calculate B3: Domestic Gross Income-Self Employed
      // ---------------------------------------------
      local.ParentAB3SeGrossInc.Count =
        (int)Math.Round(
          import.ParentA.SelfEmploymentGrossIncome.GetValueOrDefault() -
        import.ParentA.ReasonableBusinessExpense.GetValueOrDefault(),
        MidpointRounding.AwayFromZero);
      export.ParentAB3SeGrossInc.TotalCurrency =
        local.ParentAB3SeGrossInc.Count;

      if (export.ParentAB3SeGrossInc.TotalCurrency < 0)
      {
        export.ParentAB3SeGrossInc.TotalCurrency = 0;
      }

      local.ParentBB3SeGrossInc.Count =
        (int)Math.Round(
          import.ParentB.SelfEmploymentGrossIncome.GetValueOrDefault() -
        import.ParentB.ReasonableBusinessExpense.GetValueOrDefault(),
        MidpointRounding.AwayFromZero);
      export.ParentBB3SeGrossInc.TotalCurrency =
        local.ParentBB3SeGrossInc.Count;

      if (export.ParentBB3SeGrossInc.TotalCurrency < 0)
      {
        export.ParentBB3SeGrossInc.TotalCurrency = 0;
      }

      // ---------------------------------------------
      // Calculate C1: Domestic Gross Income
      // ---------------------------------------------
      local.ParentAC1TotGrossInc.Count =
        (int)Math.Round(
          import.ParentA.WageEarnerGrossIncome.GetValueOrDefault() +
        export.ParentAB3SeGrossInc.TotalCurrency,
        MidpointRounding.AwayFromZero);
      export.ParentAC1TotGrossInc.TotalCurrency =
        local.ParentAC1TotGrossInc.Count;
      local.ParentBC1TotGrassInc.Count =
        (int)Math.Round(
          import.ParentB.WageEarnerGrossIncome.GetValueOrDefault() +
        export.ParentBB3SeGrossInc.TotalCurrency,
        MidpointRounding.AwayFromZero);
      export.ParentBC1TotGrossInc.TotalCurrency =
        local.ParentBC1TotGrassInc.Count;

      // ---------------------------------------------
      // Calculate C5/D1: Total Child Support Income
      // ---------------------------------------------
      local.ParentAC5D1TotCsInc.Count =
        (int)Math.Round(
          export.ParentAC1TotGrossInc.TotalCurrency - import
        .ParentA.CourtOrderedChildSupportPaid.GetValueOrDefault() - import
        .ParentA.CourtOrderedMaintenancePaid.GetValueOrDefault() +
        import.ParentA.CourtOrderedMaintenanceRecvd.GetValueOrDefault(),
        MidpointRounding.AwayFromZero);
      export.ParentAC5D1TotCsInc.TotalCurrency =
        local.ParentAC5D1TotCsInc.Count;

      if (export.ParentAC5D1TotCsInc.TotalCurrency < 0)
      {
        export.ParentAC5D1TotCsInc.TotalCurrency = 0;
      }

      local.ParentBC5D1TotCsInc.Count =
        (int)Math.Round(
          export.ParentBC1TotGrossInc.TotalCurrency - import
        .ParentB.CourtOrderedChildSupportPaid.GetValueOrDefault() - import
        .ParentB.CourtOrderedMaintenancePaid.GetValueOrDefault() +
        import.ParentB.CourtOrderedMaintenanceRecvd.GetValueOrDefault(),
        MidpointRounding.AwayFromZero);
      export.ParentBC5D1TotCsInc.TotalCurrency =
        local.ParentBC5D1TotCsInc.Count;

      if (export.ParentBC5D1TotCsInc.TotalCurrency < 0)
      {
        export.ParentBC5D1TotCsInc.TotalCurrency = 0;
      }

      export.D1otalCsInc.TotalCurrency =
        export.ParentAC5D1TotCsInc.TotalCurrency + export
        .ParentBC5D1TotCsInc.TotalCurrency;

      if (export.D1otalCsInc.TotalCurrency <= 0)
      {
        ExitState = "OE0000_CALCULATE_UNSUCCESSFUL";

        return;
      }

      // ---------------------------------------------
      // Calculate D2: Proportionate Share of Tot Income
      // ---------------------------------------------
      if (export.D1otalCsInc.TotalCurrency != 0)
      {
        local.ParentAD2PercentInc.Percent =
          Math.Round(
            export.ParentAC5D1TotCsInc.TotalCurrency * 100
          / export.D1otalCsInc.TotalCurrency, 1, MidpointRounding.AwayFromZero);
          
        export.ParentAD2PercentInc.TotalCurrency =
          local.ParentAD2PercentInc.Percent;
        local.ParentBD2PercentInc.Percent =
          Math.Round(
            export.ParentBC5D1TotCsInc.TotalCurrency * 100
          / export.D1otalCsInc.TotalCurrency, 1, MidpointRounding.AwayFromZero);
          
        export.ParentBD2PercentInc.TotalCurrency =
          local.ParentBD2PercentInc.Percent;
      }

      // *********************************************
      // Calculate Basic Child Support Obligation.
      // *********************************************
      // ---------------------------------------------
      // If the Multiple Family Adjustment Indicator
      // is used then include the additional children
      // in the family.
      // ---------------------------------------------
      if (import.ChildSupportWorksheet.NoOfChildrenInAgeGrp1.
        GetValueOrDefault() == 0 && import
        .ChildSupportWorksheet.NoOfChildrenInAgeGrp3.GetValueOrDefault() == 0
        && import
        .ChildSupportWorksheet.NoOfChildrenInAgeGrp2.GetValueOrDefault() == 0)
      {
        ExitState = "OE0000_NO_OF_CHILDREN_REQD";

        return;
      }

      if (AsChar(import.ChildSupportWorksheet.MultipleFamilyAdjInd) == 'Y')
      {
        local.TotalNoOfChildren.Count =
          import.ChildSupportWorksheet.AdditionalNoOfChildren.
            GetValueOrDefault() + import
          .ChildSupportWorksheet.NoOfChildrenInAgeGrp1.GetValueOrDefault() + import
          .ChildSupportWorksheet.NoOfChildrenInAgeGrp2.GetValueOrDefault() + import
          .ChildSupportWorksheet.NoOfChildrenInAgeGrp3.GetValueOrDefault();

        // ---------------------------------------------
        // The maximum no of Children in a family for
        // the Child Support Schedule Table is 6.
        // ---------------------------------------------
        if (local.TotalNoOfChildren.Count > 6)
        {
          local.TotalNoOfChildren.Count = 6;
        }

        // ---------------------------------------------
        // If the utilization of the Multiple family
        // Adjustment results in a basic child support
        // obligation which is below the poverty level,
        // discard the adjustment.
        // ---------------------------------------------
        if (AsChar(import.ChildSupportWorksheet.MultipleFamilyAdjInd) == 'Y')
        {
          if (ReadChildSupportSchedule())
          {
            if (export.D1otalCsInc.TotalCurrency <= entities
              .ChildSupportSchedule.MonthlyIncomePovertyLevelInd)
            {
              local.TotalNoOfChildren.Count =
                import.ChildSupportWorksheet.NoOfChildrenInAgeGrp1.
                  GetValueOrDefault() + import
                .ChildSupportWorksheet.NoOfChildrenInAgeGrp2.
                  GetValueOrDefault() + import
                .ChildSupportWorksheet.NoOfChildrenInAgeGrp3.
                  GetValueOrDefault();

              // ---------------------------------------------
              // The maximum no of Children in a family for
              // the Child Support Schedule Table is 6.
              // ---------------------------------------------
              if (local.TotalNoOfChildren.Count > 6)
              {
                local.TotalNoOfChildren.Count = 6;
              }
            }
          }
          else
          {
            ExitState = "CHILD_SUPPORT_SCHEDULE_NF";

            return;

            // -----     Will not arise     -----
          }
        }
      }
      else
      {
        local.TotalNoOfChildren.Count =
          import.ChildSupportWorksheet.NoOfChildrenInAgeGrp1.
            GetValueOrDefault() + import
          .ChildSupportWorksheet.NoOfChildrenInAgeGrp2.GetValueOrDefault() + import
          .ChildSupportWorksheet.NoOfChildrenInAgeGrp3.GetValueOrDefault();

        // ---------------------------------------------
        // The maximum no of Children in a family for
        // the Child Support Schedule Table is 6.
        // ---------------------------------------------
        if (local.TotalNoOfChildren.Count > 6)
        {
          local.TotalNoOfChildren.Count = 6;
        }
      }

      // ---------------------------------------------
      // Get the maximum Combined Gross Monthly Income
      // for which the Child Support Schedule can be
      // Used.
      // ---------------------------------------------
      if (!ReadChildSupportSchedule())
      {
        ExitState = "CHILD_SUPPORT_SCHEDULE_NF";

        return;

        // -----     Will not arise     -----
      }

      ReadCsGrossMonthlyIncSched5();

      // ---------------------------------------------
      // Calculate the Support Amt. for different Age.
      // ---------------------------------------------
      if (export.D1otalCsInc.TotalCurrency > entities
        .CsGrossMonthlyIncSched.CombinedGrossMnthlyIncomeAmt)
      {
        // ---------------------------------------------
        // Use the formula to calculate the Support Amt.
        // ---------------------------------------------
        export.IncomeBeyondSchedule.Flag = "Y";
        local.Local1618SupportAmt.TotalCurrency =
          (decimal)Math.Round(
            Math.Pow((double)export.D1otalCsInc.TotalCurrency,
          (double)entities.ChildSupportSchedule.IncomeExponent) *
          (double)entities.ChildSupportSchedule.IncomeMultiplier, 2,
          MidpointRounding.AwayFromZero);

        if (ReadAgeGroupSupportSchedule2())
        {
          local.Local1618SupportAmt.Count =
            (int)Math.Round(
              local.Local1618SupportAmt.TotalCurrency *
            entities.AgeGroupSupportSchedule.AgeGroupFactor,
            MidpointRounding.AwayFromZero);
        }
        else
        {
          // -----     Will not arise     -----
        }

        if (ReadAgeGroupSupportSchedule1())
        {
          local.Local715SupportAmt.Count =
            (int)Math.Round(
              local.Local1618SupportAmt.TotalCurrency *
            entities.AgeGroupSupportSchedule.AgeGroupFactor,
            MidpointRounding.AwayFromZero);
        }
        else
        {
          // -----     Will not arise     -----
        }

        if (ReadAgeGroupSupportSchedule3())
        {
          local.Local06SupportAmt.Count =
            (int)Math.Round(
              local.Local1618SupportAmt.TotalCurrency *
            entities.AgeGroupSupportSchedule.AgeGroupFactor,
            MidpointRounding.AwayFromZero);
        }
        else
        {
          // -----     Will not arise     -----
        }
      }
      else
      {
        // ---------------------------------------------
        // Check if the Gross Child Support Income is
        // exactly defined in the Child Support Schedule
        // Combined Gross Monthly Income. If not, round
        // it of to the nearest value.
        // To check take the table for the age group
        // from 16-18.
        // ---------------------------------------------
        if (ReadCsGrossMonthlyIncSched1())
        {
          // ---------------------------------------------
          // Exact match is found for the conbined salary.
          // ---------------------------------------------
          local.D1otalCsInc.TotalCurrency = export.D1otalCsInc.TotalCurrency;
        }
        else
        {
          // ---------------------------------------------
          //   Get the Next and Previous Salary range.
          // ---------------------------------------------
          if (ReadCsGrossMonthlyIncSched2())
          {
            local.NextToTotalCsInc.CombinedGrossMnthlyIncomeAmt =
              entities.CsGrossMonthlyIncSched.CombinedGrossMnthlyIncomeAmt;
          }

          if (ReadCsGrossMonthlyIncSched4())
          {
            local.PrevToTotalCsInc.CombinedGrossMnthlyIncomeAmt =
              entities.CsGrossMonthlyIncSched.CombinedGrossMnthlyIncomeAmt;
          }

          if (local.NextToTotalCsInc.CombinedGrossMnthlyIncomeAmt - export
            .D1otalCsInc.TotalCurrency > export.D1otalCsInc.TotalCurrency - local
            .PrevToTotalCsInc.CombinedGrossMnthlyIncomeAmt)
          {
            local.D1otalCsInc.TotalCurrency =
              local.PrevToTotalCsInc.CombinedGrossMnthlyIncomeAmt;
          }
          else
          {
            local.D1otalCsInc.TotalCurrency =
              local.NextToTotalCsInc.CombinedGrossMnthlyIncomeAmt;
          }
        }

        // ---------------------------------------------
        // Use the Support tables to find the Support Amt.
        // ---------------------------------------------
        if (ReadAgeGroupSupportSchedule2())
        {
          if (ReadCsGrossMonthlyIncSched3())
          {
            local.Local1618SupportAmt.Count =
              entities.CsGrossMonthlyIncSched.PerChildSupportAmount;
          }
        }
        else
        {
          // -----     Will not arise     -----
        }

        if (ReadAgeGroupSupportSchedule1())
        {
          if (ReadCsGrossMonthlyIncSched3())
          {
            local.Local715SupportAmt.Count =
              entities.CsGrossMonthlyIncSched.PerChildSupportAmount;
          }
        }
        else
        {
          // -----     Will not arise     -----
        }

        if (ReadAgeGroupSupportSchedule3())
        {
          if (ReadCsGrossMonthlyIncSched3())
          {
            local.Local06SupportAmt.Count =
              entities.CsGrossMonthlyIncSched.PerChildSupportAmount;
          }
        }
        else
        {
          // -----     Will not arise     -----
        }
      }

      // ---------------------------------------------
      // Calculate the Total Support Amt for different
      // Age Groups.
      // ---------------------------------------------
      export.CsOblig06TotalAmt.TotalCurrency =
        (long)local.Local06SupportAmt.Count * import
        .ChildSupportWorksheet.NoOfChildrenInAgeGrp1.GetValueOrDefault();
      export.CsOblig715TotalAmt.TotalCurrency =
        (long)local.Local715SupportAmt.Count * import
        .ChildSupportWorksheet.NoOfChildrenInAgeGrp2.GetValueOrDefault();
      export.CsOblig1618TotalAmt.TotalCurrency =
        (long)local.Local1618SupportAmt.Count * import
        .ChildSupportWorksheet.NoOfChildrenInAgeGrp3.GetValueOrDefault();
      export.CsObligTotalAmount.TotalCurrency =
        export.CsOblig06TotalAmt.TotalCurrency + export
        .CsOblig715TotalAmt.TotalCurrency + export
        .CsOblig1618TotalAmt.TotalCurrency;

      // ---------------------------------------------
      // Calculate D4: Proportionate Share of Gross Child Support Obligation
      // ---------------------------------------------
      local.Common.Count =
        (int)Math.Round(
          export.CsObligTotalAmount.TotalCurrency * export
        .ParentAD2PercentInc.TotalCurrency /
        100, MidpointRounding.AwayFromZero);
      export.Gexport2020Calculations.GexportD4ParAPropShare.TotalCurrency =
        local.Common.Count;
      local.Common.Count =
        (int)Math.Round(
          export.CsObligTotalAmount.TotalCurrency * export
        .ParentBD2PercentInc.TotalCurrency /
        100, MidpointRounding.AwayFromZero);
      export.Gexport2020Calculations.GexportD4ParBPropShare.TotalCurrency =
        local.Common.Count;
      export.Gexport2020Calculations.GexportD4TotalPropShare.TotalCurrency =
        export.Gexport2020Calculations.GexportD4ParAPropShare.TotalCurrency + export
        .Gexport2020Calculations.GexportD4ParBPropShare.TotalCurrency;

      // ---------------------------------------------
      // Calculate D5: Total Parenting Time Adjustment
      // ---------------------------------------------
      // @@@ Start here...
      if (import.Gimport2020EnterableFields.GimportParentingTime.
        TotalCurrency == 0)
      {
        export.Gexport2020Calculations.GexportD5ParAParentTimAdj.TotalCurrency =
          0;
        export.Gexport2020Calculations.GexportD5ParBParentTimAdj.TotalCurrency =
          0;
      }
      else
      {
      }

      if (AsChar(import.Gimport2020EnterableFields.GimportParentingTime.
        SelectChar) == '1')
      {
        local.Common.Count =
          (int)Math.Round(
            -(export.Gexport2020Calculations.GexportD4ParAPropShare.
            TotalCurrency * import
          .Gimport2020EnterableFields.GimportParentingTime.TotalCurrency / 100
          ), MidpointRounding.AwayFromZero);
        export.Gexport2020Calculations.GexportD5ParAParentTimAdj.TotalCurrency =
          local.Common.Count;
      }
      else
      {
        export.Gexport2020Calculations.GexportD5ParAParentTimAdj.TotalCurrency =
          0;
      }

      if (AsChar(import.Gimport2020EnterableFields.GimportParentingTime.
        SelectChar) == '2')
      {
        local.Common.Count =
          (int)Math.Round(
            -(export.Gexport2020Calculations.GexportD4ParBPropShare.
            TotalCurrency * import
          .Gimport2020EnterableFields.GimportParentingTime.TotalCurrency / 100
          ), MidpointRounding.AwayFromZero);
        export.Gexport2020Calculations.GexportD5ParBParentTimAdj.TotalCurrency =
          local.Common.Count;
      }
      else
      {
        export.Gexport2020Calculations.GexportD5ParBParentTimAdj.TotalCurrency =
          0;
      }

      export.Gexport2020Calculations.GexportD5TotalParentTimAdj.TotalCurrency =
        export.Gexport2020Calculations.GexportD5ParAParentTimAdj.TotalCurrency +
        export.Gexport2020Calculations.GexportD5ParBParentTimAdj.TotalCurrency;

      // ---------------------------------------------
      // Calculate D6: Proportionate Shares After Parenting Time Adjustment
      // ---------------------------------------------
      export.Gexport2020Calculations.GexportD6ParAPsAfterPat.TotalCurrency =
        export.Gexport2020Calculations.GexportD4ParAPropShare.TotalCurrency + export
        .Gexport2020Calculations.GexportD5ParAParentTimAdj.TotalCurrency;
      export.Gexport2020Calculations.GexportD6ParBPsAfterPat.TotalCurrency =
        export.Gexport2020Calculations.GexportD4ParBPropShare.TotalCurrency + export
        .Gexport2020Calculations.GexportD5ParBParentTimAdj.TotalCurrency;
      export.Gexport2020Calculations.GexportD6TotalPsAfterPat.TotalCurrency =
        export.Gexport2020Calculations.GexportD4TotalPropShare.TotalCurrency + export
        .Gexport2020Calculations.GexportD5TotalParentTimAdj.TotalCurrency;

      // ---------------------------------------------
      // Calculate D7: Health and Dental Insurance Premium
      // ---------------------------------------------
      export.TotalInsurancePrem.TotalCurrency =
        export.ParentA.HealthAndDentalInsurancePrem.GetValueOrDefault() + export
        .ParentB.HealthAndDentalInsurancePrem.GetValueOrDefault();

      // ---------------------------------------------
      // Calculate D8: Proportionate Shares of Health Insurance Premium
      // ---------------------------------------------
      local.Common.Count =
        (int)Math.Round(
          export.TotalInsurancePrem.TotalCurrency * export
        .ParentAD2PercentInc.TotalCurrency /
        100, MidpointRounding.AwayFromZero);
      export.Gexport2020Calculations.GexportD8ParAPropShrHip.TotalCurrency =
        local.Common.Count;
      local.Common.Count =
        (int)Math.Round(
          export.TotalInsurancePrem.TotalCurrency * export
        .ParentBD2PercentInc.TotalCurrency /
        100, MidpointRounding.AwayFromZero);
      export.Gexport2020Calculations.GexportD8ParBPropShrHip.TotalCurrency =
        local.Common.Count;
      export.Gexport2020Calculations.GexportD8TotalPropShrHip.TotalCurrency =
        export.Gexport2020Calculations.GexportD8ParAPropShrHip.TotalCurrency + export
        .Gexport2020Calculations.GexportD8ParBPropShrHip.TotalCurrency;

      // --------------------------------------------
      // Calculate D9: Work Related Child Care Cost.
      // --------------------------------------------
      export.TotalChildCareCost.TotalCurrency =
        export.ParentA.WorkRelatedChildCareCosts.GetValueOrDefault() + export
        .ParentB.WorkRelatedChildCareCosts.GetValueOrDefault() + export
        .ParentBChildCareCost.TotalCurrency;

      // ---------------------------------------------
      // Calculate D10: Proportionate Shares Work Related Child Care Costs
      // ---------------------------------------------
      local.Common.Count =
        (int)Math.Round((
          export.ParentA.WorkRelatedChildCareCosts.GetValueOrDefault() + export
        .ParentB.WorkRelatedChildCareCosts.GetValueOrDefault()) * export
        .ParentAD2PercentInc.TotalCurrency /
        100, MidpointRounding.AwayFromZero);
      export.Gexport2020Calculations.GexportD10ParAPropShrWrcc.TotalCurrency =
        local.Common.Count;
      local.Common.Count =
        (int)Math.Round((
          export.ParentA.WorkRelatedChildCareCosts.GetValueOrDefault() + export
        .ParentB.WorkRelatedChildCareCosts.GetValueOrDefault()) * export
        .ParentBD2PercentInc.TotalCurrency /
        100, MidpointRounding.AwayFromZero);
      export.Gexport2020Calculations.GexportD10ParBPropShrWrcc.TotalCurrency =
        local.Common.Count;
      export.Gexport2020Calculations.GexportD10TotalPropShrWrcc.TotalCurrency =
        export.Gexport2020Calculations.GexportD10ParAPropShrWrcc.TotalCurrency +
        export.Gexport2020Calculations.GexportD10ParBPropShrWrcc.TotalCurrency;

      // ---------------------------------------------
      // Calculate D11: Proportionate Child Support Obligation
      // ---------------------------------------------
      export.Gexport2020Calculations.GexportD11ParAPropShrCcob.TotalCurrency =
        export.Gexport2020Calculations.GexportD6ParAPsAfterPat.TotalCurrency + export
        .Gexport2020Calculations.GexportD8ParAPropShrHip.TotalCurrency + export
        .Gexport2020Calculations.GexportD10ParAPropShrWrcc.TotalCurrency;
      export.Gexport2020Calculations.GexportD11ParBPropShrCcob.TotalCurrency =
        export.Gexport2020Calculations.GexportD6ParBPsAfterPat.TotalCurrency + export
        .Gexport2020Calculations.GexportD8ParBPropShrHip.TotalCurrency + export
        .Gexport2020Calculations.GexportD10ParBPropShrWrcc.TotalCurrency;
      export.Gexport2020Calculations.GexportD11TotalPropShrCcob.TotalCurrency =
        export.Gexport2020Calculations.GexportD11ParAPropShrCcob.TotalCurrency +
        export.Gexport2020Calculations.GexportD11ParBPropShrCcob.TotalCurrency;

      // ---------------------------------------------
      // Calculate D12: Credit for Insurance or Work Related Child Care Paid
      // ---------------------------------------------
      export.ParentA.InsuranceWorkRelatedCcCredit =
        export.ParentA.HealthAndDentalInsurancePrem.GetValueOrDefault() + export
        .ParentA.WorkRelatedChildCareCosts.GetValueOrDefault();
      export.ParentB.InsuranceWorkRelatedCcCredit =
        export.ParentB.HealthAndDentalInsurancePrem.GetValueOrDefault() + export
        .ParentB.WorkRelatedChildCareCosts.GetValueOrDefault();
      export.Gexport2020Calculations.GexportD12TotalInsWrccPaid.TotalCurrency =
        export.ParentA.InsuranceWorkRelatedCcCredit.GetValueOrDefault() + export
        .ParentB.InsuranceWorkRelatedCcCredit.GetValueOrDefault();

      // ---------------------------------------------
      // Calculate D13: Basic Parental Child Support Obligation
      // ---------------------------------------------
      export.Gexport2020Calculations.GexportD13ParABasicChSup.TotalCurrency =
        export.Gexport2020Calculations.GexportD11ParAPropShrCcob.TotalCurrency -
        export.ParentA.InsuranceWorkRelatedCcCredit.GetValueOrDefault();
      export.Gexport2020Calculations.GexportD13ParBBasicChSup.TotalCurrency =
        export.Gexport2020Calculations.GexportD11ParBPropShrCcob.TotalCurrency -
        export.ParentB.InsuranceWorkRelatedCcCredit.GetValueOrDefault();

      // --------------------------------------------
      // Calculate Total Adjustment from all Categories.
      // --------------------------------------------
      for(import.Import1.Index = 0; import.Import1.Index < import
        .Import1.Count; ++import.Import1.Index)
      {
        export.ParentAE7F2TotAdj.TotalCurrency += import.Import1.Item.ParentA.
          AdjustmentAmount.GetValueOrDefault();
        export.ParentBE7F2TotAdj.TotalCurrency += import.Import1.Item.ParentB.
          AdjustmentAmount.GetValueOrDefault();
      }

      local.ParentAE7F2TotAdj.Count =
        (int)Math.Round(
          export.ParentAE7F2TotAdj.TotalCurrency,
        MidpointRounding.AwayFromZero);
      export.ParentAE7F2TotAdj.TotalCurrency = local.ParentAE7F2TotAdj.Count;
      local.ParentBE7F2TotAdj.Count =
        (int)Math.Round(
          export.ParentBE7F2TotAdj.TotalCurrency,
        MidpointRounding.AwayFromZero);
      export.ParentBE7F2TotAdj.TotalCurrency = local.ParentBE7F2TotAdj.Count;
      export.ParentAF2TotalCsAdj.TotalCurrency =
        export.ParentAE7F2TotAdj.TotalCurrency;
      export.ParentBF2TotalCsAdj.TotalCurrency =
        export.ParentBE7F2TotAdj.TotalCurrency;

      // --------------------------------------------
      // Calculate Adjusted Child Support Obligation.
      // --------------------------------------------
      export.ParentAF3AdjCsOblig.TotalCurrency =
        export.Gexport2020Calculations.GexportD13ParABasicChSup.TotalCurrency +
        export.ParentAF2TotalCsAdj.TotalCurrency;

      if (export.ParentAF3AdjCsOblig.TotalCurrency < 0)
      {
        export.ParentAF3AdjCsOblig.TotalCurrency = 0;
      }

      export.ParentBF3AdjCsOblig.TotalCurrency =
        export.Gexport2020Calculations.GexportD13ParBBasicChSup.TotalCurrency +
        export.ParentBF2TotalCsAdj.TotalCurrency;

      if (export.ParentBF3AdjCsOblig.TotalCurrency < 0)
      {
        export.ParentBF3AdjCsOblig.TotalCurrency = 0;
      }

      // ---------------------------------------------
      // Calculate F5a: Ability to Pay
      // ---------------------------------------------
      if (IsEmpty(import.Gimport2020EnterableFields.GimportAbilityToPayParent.
        SelectChar))
      {
        export.Gexport2020Calculations.GexportF5A1CsIncome.TotalCurrency = 0;
        export.Gexport2020Calculations.GexportF5A2PovertyLevel.TotalCurrency =
          0;
        export.Gexport2020Calculations.GexportF5A3AbilityToPay.TotalCurrency =
          0;
      }
      else
      {
        if (AsChar(import.Gimport2020EnterableFields.GimportAbilityToPayParent.
          SelectChar) == '1')
        {
          export.Gexport2020Calculations.GexportF5A1CsIncome.TotalCurrency =
            export.ParentAC5D1TotCsInc.TotalCurrency;
        }

        if (AsChar(import.Gimport2020EnterableFields.GimportAbilityToPayParent.
          SelectChar) == '2')
        {
          export.Gexport2020Calculations.GexportF5A1CsIncome.TotalCurrency =
            export.ParentBC5D1TotCsInc.TotalCurrency;
        }

        if (ReadCodeValue())
        {
          export.Gexport2020Calculations.GexportF5A2PovertyLevel.TotalCurrency =
            StringToNumber(TrimEnd(entities.CodeValue.Description));
        }

        export.Gexport2020Calculations.GexportF5A3AbilityToPay.TotalCurrency =
          export.Gexport2020Calculations.GexportF5A1CsIncome.TotalCurrency - export
          .Gexport2020Calculations.GexportF5A2PovertyLevel.TotalCurrency;

        if (export.Gexport2020Calculations.GexportF5A3AbilityToPay.
          TotalCurrency < 0)
        {
          export.Gexport2020Calculations.GexportF5A3AbilityToPay.TotalCurrency =
            0;
        }
      }

      // ---------------------------------------------
      // Calculate F5b: Subtotal
      // ---------------------------------------------
      if (AsChar(import.Gimport2020EnterableFields.GimportAbilityToPayParent.
        SelectChar) == '1')
      {
        if (export.ParentA.EqualParentingTimeObligation.GetValueOrDefault() > 0)
        {
          if (export.ParentA.EqualParentingTimeObligation.
            GetValueOrDefault() < export
            .Gexport2020Calculations.GexportF5A3AbilityToPay.TotalCurrency)
          {
            export.Gexport2020Calculations.GexportF5BParASubtotal.
              TotalCurrency =
                export.ParentA.EqualParentingTimeObligation.GetValueOrDefault();
              
          }
          else
          {
            export.Gexport2020Calculations.GexportF5BParASubtotal.
              TotalCurrency =
                export.Gexport2020Calculations.GexportF5A3AbilityToPay.
                TotalCurrency;
          }
        }
        else if (export.ParentAF3AdjCsOblig.TotalCurrency < export
          .Gexport2020Calculations.GexportF5A3AbilityToPay.TotalCurrency)
        {
          export.Gexport2020Calculations.GexportF5BParASubtotal.TotalCurrency =
            export.ParentAF3AdjCsOblig.TotalCurrency;
        }
        else
        {
          export.Gexport2020Calculations.GexportF5BParASubtotal.TotalCurrency =
            export.Gexport2020Calculations.GexportF5A3AbilityToPay.
              TotalCurrency;
        }
      }
      else if (export.ParentA.EqualParentingTimeObligation.
        GetValueOrDefault() > 0)
      {
        export.Gexport2020Calculations.GexportF5BParASubtotal.TotalCurrency =
          export.ParentA.EqualParentingTimeObligation.GetValueOrDefault();
      }
      else
      {
        export.Gexport2020Calculations.GexportF5BParASubtotal.TotalCurrency =
          export.ParentAF3AdjCsOblig.TotalCurrency;
      }

      if (AsChar(import.Gimport2020EnterableFields.GimportAbilityToPayParent.
        SelectChar) == '2')
      {
        if (export.ParentB.EqualParentingTimeObligation.GetValueOrDefault() > 0)
        {
          if (export.ParentB.EqualParentingTimeObligation.
            GetValueOrDefault() < export
            .Gexport2020Calculations.GexportF5A3AbilityToPay.TotalCurrency)
          {
            export.Gexport2020Calculations.GexportF5BParBSubtotal.
              TotalCurrency =
                export.ParentB.EqualParentingTimeObligation.GetValueOrDefault();
              
          }
          else
          {
            export.Gexport2020Calculations.GexportF5BParBSubtotal.
              TotalCurrency =
                export.Gexport2020Calculations.GexportF5A3AbilityToPay.
                TotalCurrency;
          }
        }
        else if (export.ParentBF3AdjCsOblig.TotalCurrency < export
          .Gexport2020Calculations.GexportF5A3AbilityToPay.TotalCurrency)
        {
          export.Gexport2020Calculations.GexportF5BParBSubtotal.TotalCurrency =
            export.ParentBF3AdjCsOblig.TotalCurrency;
        }
        else
        {
          export.Gexport2020Calculations.GexportF5BParBSubtotal.TotalCurrency =
            export.Gexport2020Calculations.GexportF5A3AbilityToPay.
              TotalCurrency;
        }
      }
      else if (export.ParentB.EqualParentingTimeObligation.
        GetValueOrDefault() > 0)
      {
        export.Gexport2020Calculations.GexportF5BParBSubtotal.TotalCurrency =
          export.ParentB.EqualParentingTimeObligation.GetValueOrDefault();
      }
      else
      {
        export.Gexport2020Calculations.GexportF5BParBSubtotal.TotalCurrency =
          export.ParentBF3AdjCsOblig.TotalCurrency;
      }

      // ---------------------------------------------
      // Calculate F6b: Final Subtotal
      // ---------------------------------------------
      export.Gexport2020Calculations.GexportF6BParAFinaSubtotal.TotalCurrency =
        export.Gexport2020Calculations.GexportF5BParASubtotal.TotalCurrency - export
        .ParentA.SocialSecDependentBenefit.GetValueOrDefault();

      if (export.Gexport2020Calculations.GexportF6BParAFinaSubtotal.
        TotalCurrency < 0)
      {
        export.Gexport2020Calculations.GexportF6BParAFinaSubtotal.
          TotalCurrency = 0;
      }

      export.Gexport2020Calculations.GexportF6BParBFinaSubtotal.TotalCurrency =
        export.Gexport2020Calculations.GexportF5BParBSubtotal.TotalCurrency - export
        .ParentB.SocialSecDependentBenefit.GetValueOrDefault();

      if (export.Gexport2020Calculations.GexportF6BParBFinaSubtotal.
        TotalCurrency < 0)
      {
        export.Gexport2020Calculations.GexportF6BParBFinaSubtotal.
          TotalCurrency = 0;
      }

      export.ParentA.NetAdjParentalChildSuppAmt =
        export.Gexport2020Calculations.GexportF6BParAFinaSubtotal.TotalCurrency;
        
      export.ParentB.NetAdjParentalChildSuppAmt =
        export.Gexport2020Calculations.GexportF6BParBFinaSubtotal.TotalCurrency;
        

      // ---------------------------------------------
      // Calculate the enforcement fee allowance
      // ---------------------------------------------
      export.ParentA.EnforcementFeeType = "";
      export.ParentA.EnforcementFeeAllowance = 0;
      export.ParentB.EnforcementFeeType = "";
      export.ParentB.EnforcementFeeAllowance = 0;

      // CQ50299 12/9/2015  GVandy Changed "M" to "1".
      if (AsChar(import.Common.SelectChar) == '1')
      {
        if (import.Common.Percentage > 0)
        {
          export.ParentA.EnforcementFeeType = "P";
          export.ParentA.EnforcementFeeAllowance = import.Common.Percentage;
          local.Common.Count =
            (int)Math.Round(
              export.Gexport2020Calculations.GexportF6BParAFinaSubtotal.
              TotalCurrency * import.Common.Percentage / 100
            * 0.5M, MidpointRounding.AwayFromZero);
          export.ParentAEnfFee.TotalCurrency = local.Common.Count;
        }
        else
        {
          export.ParentA.EnforcementFeeType = "F";
          export.ParentA.EnforcementFeeAllowance =
            (int?)import.Common.TotalCurrency;
          local.Common.Count =
            (int)Math.Round(
              import.Common.TotalCurrency *
            0.5M, MidpointRounding.AwayFromZero);
          export.ParentAEnfFee.TotalCurrency = local.Common.Count;
        }

        // CQ50299 12/9/2015  GVandy Changed "F" to "2".
      }
      else if (AsChar(import.Common.SelectChar) == '2')
      {
        if (import.Common.Percentage > 0)
        {
          export.ParentB.EnforcementFeeType = "P";
          export.ParentB.EnforcementFeeAllowance = import.Common.Percentage;
          local.Common.Count =
            (int)Math.Round(
              export.Gexport2020Calculations.GexportF6BParBFinaSubtotal.
              TotalCurrency * import.Common.Percentage / 100
            * 0.5M, MidpointRounding.AwayFromZero);
          export.ParentBEnfFee.TotalCurrency = local.Common.Count;
        }
        else
        {
          export.ParentB.EnforcementFeeType = "F";
          export.ParentB.EnforcementFeeAllowance =
            (int?)import.Common.TotalCurrency;
          local.Common.Count =
            (int)Math.Round(
              import.Common.TotalCurrency *
            0.5M, MidpointRounding.AwayFromZero);
          export.ParentBEnfFee.TotalCurrency = local.Common.Count;
        }
      }

      // ---------------------------------------------
      // Calculate F8: Net Parental Child Support Obligation
      // ---------------------------------------------
      export.Gexport2020Calculations.GexportF8ParANetCsOblig.TotalCurrency =
        export.Gexport2020Calculations.GexportF6BParAFinaSubtotal.
          TotalCurrency + export.ParentAEnfFee.TotalCurrency;
      export.Gexport2020Calculations.GexportF8ParBNetCsOblig.TotalCurrency =
        export.Gexport2020Calculations.GexportF6BParBFinaSubtotal.
          TotalCurrency + export.ParentBEnfFee.TotalCurrency;

      if (export.Gexport2020Calculations.GexportF6BParAFinaSubtotal.
        TotalCurrency != 0 || export
        .Gexport2020Calculations.GexportF6BParBFinaSubtotal.TotalCurrency != 0)
      {
        ExitState = "OE0000_CALCULATE_SUCCESSFUL";
      }
      else
      {
        ExitState = "OE0000_CALCULATE_UNSUCCESSFUL";
      }
    }
    else
    {
      // ---------------------------------------------
      // Calculate B3: Domestic Gross Income-Self Employed
      // ---------------------------------------------
      local.ParentAB3SeGrossInc.Count =
        (int)Math.Round(
          import.ParentA.SelfEmploymentGrossIncome.GetValueOrDefault() -
        import.ParentA.ReasonableBusinessExpense.GetValueOrDefault(),
        MidpointRounding.AwayFromZero);
      export.ParentAB3SeGrossInc.TotalCurrency =
        local.ParentAB3SeGrossInc.Count;

      if (export.ParentAB3SeGrossInc.TotalCurrency < 0)
      {
        export.ParentAB3SeGrossInc.TotalCurrency = 0;
      }

      local.ParentBB3SeGrossInc.Count =
        (int)Math.Round(
          import.ParentB.SelfEmploymentGrossIncome.GetValueOrDefault() -
        import.ParentB.ReasonableBusinessExpense.GetValueOrDefault(),
        MidpointRounding.AwayFromZero);
      export.ParentBB3SeGrossInc.TotalCurrency =
        local.ParentBB3SeGrossInc.Count;

      if (export.ParentBB3SeGrossInc.TotalCurrency < 0)
      {
        export.ParentBB3SeGrossInc.TotalCurrency = 0;
      }

      // ---------------------------------------------
      // Calculate C1: Domestic Gross Income
      // ---------------------------------------------
      local.ParentAC1TotGrossInc.Count =
        (int)Math.Round(
          import.ParentA.WageEarnerGrossIncome.GetValueOrDefault() +
        export.ParentAB3SeGrossInc.TotalCurrency,
        MidpointRounding.AwayFromZero);
      export.ParentAC1TotGrossInc.TotalCurrency =
        local.ParentAC1TotGrossInc.Count;
      local.ParentBC1TotGrassInc.Count =
        (int)Math.Round(
          import.ParentB.WageEarnerGrossIncome.GetValueOrDefault() +
        export.ParentBB3SeGrossInc.TotalCurrency,
        MidpointRounding.AwayFromZero);
      export.ParentBC1TotGrossInc.TotalCurrency =
        local.ParentBC1TotGrassInc.Count;

      // ---------------------------------------------
      // Calculate C5/D1: Total Child Support Income
      // ---------------------------------------------
      local.ParentAC5D1TotCsInc.Count =
        (int)Math.Round(
          export.ParentAC1TotGrossInc.TotalCurrency - import
        .ParentA.CourtOrderedChildSupportPaid.GetValueOrDefault() - import
        .ParentA.CourtOrderedMaintenancePaid.GetValueOrDefault() +
        import.ParentA.CourtOrderedMaintenanceRecvd.GetValueOrDefault(),
        MidpointRounding.AwayFromZero);
      export.ParentAC5D1TotCsInc.TotalCurrency =
        local.ParentAC5D1TotCsInc.Count;

      if (export.ParentAC5D1TotCsInc.TotalCurrency < 0)
      {
        export.ParentAC5D1TotCsInc.TotalCurrency = 0;
      }

      local.ParentBC5D1TotCsInc.Count =
        (int)Math.Round(
          export.ParentBC1TotGrossInc.TotalCurrency - import
        .ParentB.CourtOrderedChildSupportPaid.GetValueOrDefault() - import
        .ParentB.CourtOrderedMaintenancePaid.GetValueOrDefault() +
        import.ParentB.CourtOrderedMaintenanceRecvd.GetValueOrDefault(),
        MidpointRounding.AwayFromZero);
      export.ParentBC5D1TotCsInc.TotalCurrency =
        local.ParentBC5D1TotCsInc.Count;

      if (export.ParentBC5D1TotCsInc.TotalCurrency < 0)
      {
        export.ParentBC5D1TotCsInc.TotalCurrency = 0;
      }

      export.D1otalCsInc.TotalCurrency =
        export.ParentAC5D1TotCsInc.TotalCurrency + export
        .ParentBC5D1TotCsInc.TotalCurrency;

      if (export.D1otalCsInc.TotalCurrency <= 0)
      {
        ExitState = "OE0000_CALCULATE_UNSUCCESSFUL";

        return;
      }

      // ---------------------------------------------
      // Calculate D2: Proportionate Share of Tot Income
      // ---------------------------------------------
      if (export.D1otalCsInc.TotalCurrency != 0)
      {
        local.ParentAD2PercentInc.Percent =
          Math.Round(
            export.ParentAC5D1TotCsInc.TotalCurrency * 100
          / export.D1otalCsInc.TotalCurrency, 1, MidpointRounding.AwayFromZero);
          
        export.ParentAD2PercentInc.TotalCurrency =
          local.ParentAD2PercentInc.Percent;
        local.ParentBD2PercentInc.Percent =
          Math.Round(
            export.ParentBC5D1TotCsInc.TotalCurrency * 100
          / export.D1otalCsInc.TotalCurrency, 1, MidpointRounding.AwayFromZero);
          
        export.ParentBD2PercentInc.TotalCurrency =
          local.ParentBD2PercentInc.Percent;
      }

      // *********************************************
      // Calculate Basic Child Support Obligation.
      // *********************************************
      // ---------------------------------------------
      // If the Multiple Family Adjustment Indicator
      // is used then include the additional children
      // in the family.
      // ---------------------------------------------
      if (import.ChildSupportWorksheet.NoOfChildrenInAgeGrp1.
        GetValueOrDefault() == 0 && import
        .ChildSupportWorksheet.NoOfChildrenInAgeGrp3.GetValueOrDefault() == 0
        && import
        .ChildSupportWorksheet.NoOfChildrenInAgeGrp2.GetValueOrDefault() == 0)
      {
        ExitState = "OE0000_NO_OF_CHILDREN_REQD";

        return;
      }

      if (AsChar(import.ChildSupportWorksheet.MultipleFamilyAdjInd) == 'Y')
      {
        local.TotalNoOfChildren.Count =
          import.ChildSupportWorksheet.AdditionalNoOfChildren.
            GetValueOrDefault() + import
          .ChildSupportWorksheet.NoOfChildrenInAgeGrp1.GetValueOrDefault() + import
          .ChildSupportWorksheet.NoOfChildrenInAgeGrp2.GetValueOrDefault() + import
          .ChildSupportWorksheet.NoOfChildrenInAgeGrp3.GetValueOrDefault();

        // ---------------------------------------------
        // The maximum no of Children in a family for
        // the Child Support Schedule Table is 6.
        // ---------------------------------------------
        if (local.TotalNoOfChildren.Count > 6)
        {
          local.TotalNoOfChildren.Count = 6;
        }

        // ---------------------------------------------
        // If the utilization of the Multiple family
        // Adjustment results in a basic child support
        // obligation which is below the poverty level,
        // discard the adjustment.
        // ---------------------------------------------
        if (AsChar(import.ChildSupportWorksheet.MultipleFamilyAdjInd) == 'Y')
        {
          if (ReadChildSupportSchedule())
          {
            if (export.D1otalCsInc.TotalCurrency <= entities
              .ChildSupportSchedule.MonthlyIncomePovertyLevelInd)
            {
              local.TotalNoOfChildren.Count =
                import.ChildSupportWorksheet.NoOfChildrenInAgeGrp1.
                  GetValueOrDefault() + import
                .ChildSupportWorksheet.NoOfChildrenInAgeGrp2.
                  GetValueOrDefault() + import
                .ChildSupportWorksheet.NoOfChildrenInAgeGrp3.
                  GetValueOrDefault();

              // ---------------------------------------------
              // The maximum no of Children in a family for
              // the Child Support Schedule Table is 6.
              // ---------------------------------------------
              if (local.TotalNoOfChildren.Count > 6)
              {
                local.TotalNoOfChildren.Count = 6;
              }
            }
          }
          else
          {
            ExitState = "CHILD_SUPPORT_SCHEDULE_NF";

            return;

            // -----     Will not arise     -----
          }
        }
      }
      else
      {
        local.TotalNoOfChildren.Count =
          import.ChildSupportWorksheet.NoOfChildrenInAgeGrp1.
            GetValueOrDefault() + import
          .ChildSupportWorksheet.NoOfChildrenInAgeGrp2.GetValueOrDefault() + import
          .ChildSupportWorksheet.NoOfChildrenInAgeGrp3.GetValueOrDefault();

        // ---------------------------------------------
        // The maximum no of Children in a family for
        // the Child Support Schedule Table is 6.
        // ---------------------------------------------
        if (local.TotalNoOfChildren.Count > 6)
        {
          local.TotalNoOfChildren.Count = 6;
        }
      }

      // ---------------------------------------------
      // Get the maximum Combined Gross Monthly Income
      // for which the Child Support Schedule can be
      // Used.
      // ---------------------------------------------
      if (!ReadChildSupportSchedule())
      {
        ExitState = "CHILD_SUPPORT_SCHEDULE_NF";

        return;

        // -----     Will not arise     -----
      }

      ReadCsGrossMonthlyIncSched5();

      // ---------------------------------------------
      // Calculate the Support Amt. for different Age.
      // ---------------------------------------------
      if (export.D1otalCsInc.TotalCurrency > entities
        .CsGrossMonthlyIncSched.CombinedGrossMnthlyIncomeAmt)
      {
        // ---------------------------------------------
        // Use the formula to calculate the Support Amt.
        // ---------------------------------------------
        local.Local1618SupportAmt.TotalCurrency =
          (decimal)Math.Round(
            Math.Pow((double)export.D1otalCsInc.TotalCurrency,
          (double)entities.ChildSupportSchedule.IncomeExponent) *
          (double)entities.ChildSupportSchedule.IncomeMultiplier, 2,
          MidpointRounding.AwayFromZero);

        if (ReadAgeGroupSupportSchedule2())
        {
          local.Local1618SupportAmt.Count =
            (int)Math.Round(
              local.Local1618SupportAmt.TotalCurrency *
            entities.AgeGroupSupportSchedule.AgeGroupFactor,
            MidpointRounding.AwayFromZero);
        }
        else
        {
          // -----     Will not arise     -----
        }

        if (ReadAgeGroupSupportSchedule1())
        {
          local.Local715SupportAmt.Count =
            (int)Math.Round(
              local.Local1618SupportAmt.TotalCurrency *
            entities.AgeGroupSupportSchedule.AgeGroupFactor,
            MidpointRounding.AwayFromZero);
        }
        else
        {
          // -----     Will not arise     -----
        }

        if (ReadAgeGroupSupportSchedule3())
        {
          local.Local06SupportAmt.Count =
            (int)Math.Round(
              local.Local1618SupportAmt.TotalCurrency *
            entities.AgeGroupSupportSchedule.AgeGroupFactor,
            MidpointRounding.AwayFromZero);
        }
        else
        {
          // -----     Will not arise     -----
        }
      }
      else
      {
        // ---------------------------------------------
        // Check if the Gross Child Support Income is
        // exactly defined in the Child Support Schedule
        // Combined Gross Monthly Income. If not, round
        // it of to the nearest value.
        // To check take the table for the age group
        // from 16-18.
        // ---------------------------------------------
        if (ReadCsGrossMonthlyIncSched1())
        {
          // ---------------------------------------------
          // Exact match is found for the conbined salary.
          // ---------------------------------------------
          local.D1otalCsInc.TotalCurrency = export.D1otalCsInc.TotalCurrency;
        }
        else
        {
          // ---------------------------------------------
          //   Get the Next and Previous Salary range.
          // ---------------------------------------------
          if (ReadCsGrossMonthlyIncSched2())
          {
            local.NextToTotalCsInc.CombinedGrossMnthlyIncomeAmt =
              entities.CsGrossMonthlyIncSched.CombinedGrossMnthlyIncomeAmt;
          }

          if (ReadCsGrossMonthlyIncSched4())
          {
            local.PrevToTotalCsInc.CombinedGrossMnthlyIncomeAmt =
              entities.CsGrossMonthlyIncSched.CombinedGrossMnthlyIncomeAmt;
          }

          if (local.NextToTotalCsInc.CombinedGrossMnthlyIncomeAmt - export
            .D1otalCsInc.TotalCurrency > export.D1otalCsInc.TotalCurrency - local
            .PrevToTotalCsInc.CombinedGrossMnthlyIncomeAmt)
          {
            local.D1otalCsInc.TotalCurrency =
              local.PrevToTotalCsInc.CombinedGrossMnthlyIncomeAmt;
          }
          else
          {
            local.D1otalCsInc.TotalCurrency =
              local.NextToTotalCsInc.CombinedGrossMnthlyIncomeAmt;
          }
        }

        // ---------------------------------------------
        // Use the Support tables to find the Support Amt.
        // ---------------------------------------------
        if (ReadAgeGroupSupportSchedule2())
        {
          if (ReadCsGrossMonthlyIncSched3())
          {
            local.Local1618SupportAmt.Count =
              entities.CsGrossMonthlyIncSched.PerChildSupportAmount;
          }
        }
        else
        {
          // -----     Will not arise     -----
        }

        if (ReadAgeGroupSupportSchedule1())
        {
          if (ReadCsGrossMonthlyIncSched3())
          {
            local.Local715SupportAmt.Count =
              entities.CsGrossMonthlyIncSched.PerChildSupportAmount;
          }
        }
        else
        {
          // -----     Will not arise     -----
        }

        if (ReadAgeGroupSupportSchedule3())
        {
          if (ReadCsGrossMonthlyIncSched3())
          {
            local.Local06SupportAmt.Count =
              entities.CsGrossMonthlyIncSched.PerChildSupportAmount;
          }
        }
        else
        {
          // -----     Will not arise     -----
        }
      }

      // ---------------------------------------------
      // Calculate the Total Support Amt for different
      // Age Groups.
      // ---------------------------------------------
      export.CsOblig06TotalAmt.TotalCurrency =
        (long)local.Local06SupportAmt.Count * import
        .ChildSupportWorksheet.NoOfChildrenInAgeGrp1.GetValueOrDefault();
      export.CsOblig715TotalAmt.TotalCurrency =
        (long)local.Local715SupportAmt.Count * import
        .ChildSupportWorksheet.NoOfChildrenInAgeGrp2.GetValueOrDefault();
      export.CsOblig1618TotalAmt.TotalCurrency =
        (long)local.Local1618SupportAmt.Count * import
        .ChildSupportWorksheet.NoOfChildrenInAgeGrp3.GetValueOrDefault();
      export.CsObligTotalAmount.TotalCurrency =
        export.CsOblig06TotalAmt.TotalCurrency + export
        .CsOblig715TotalAmt.TotalCurrency + export
        .CsOblig1618TotalAmt.TotalCurrency;

      // ---------------------------------------------
      // Calculate the Total Insurance Premium.
      // ---------------------------------------------
      local.TotalInsurancePrem.Count =
        (int)Math.Round(
          import.ParentA.HealthAndDentalInsurancePrem.GetValueOrDefault() +
        import.ParentB.HealthAndDentalInsurancePrem.GetValueOrDefault(),
        MidpointRounding.AwayFromZero);
      export.TotalInsurancePrem.TotalCurrency = local.TotalInsurancePrem.Count;

      // ---------------------------------------------
      // Calculate the Eligible Tax Credit for Parent A.
      // ---------------------------------------------
      local.Temp.AdjustedGrossIncomeMinimum =
        (int)(export.ParentAC5D1TotCsInc.TotalCurrency * 12);

      if (ReadChildCareTaxCreditFactors())
      {
        if (AsChar(import.ParentA.EligibleForFederalTaxCredit) == 'Y')
        {
          local.ParentATotalTaxCredit.Count =
            (int)Math.Round(
              export.ParentA.WorkRelatedChildCareCosts.GetValueOrDefault() * entities
            .ChildCareTaxCreditFactors.FederalTaxCreditPercent /
            100, MidpointRounding.AwayFromZero);
          export.ParentATotalTaxCredit.TotalCurrency =
            local.ParentATotalTaxCredit.Count;

          // ---------------------------------------------
          //  Check if the Federal Child Care Credit
          //  exceeds the maximum monthly credit.
          // ---------------------------------------------
          switch(import.ParentA.NoOfChildrenInDayCare.GetValueOrDefault())
          {
            case 0:
              // --- No child in day care. Credit does not apply ---
              break;
            case 1:
              // --- Check the maximum Federal Child Care
              // 	    Credit for One Child	 ---
              if (entities.ChildCareTaxCreditFactors.MaxMonthlyCredit1Child < export
                .ParentATotalTaxCredit.TotalCurrency)
              {
                export.ParentATotalTaxCredit.TotalCurrency =
                  entities.ChildCareTaxCreditFactors.MaxMonthlyCredit1Child;
              }

              break;
            default:
              // --- Check the maximum Federal Child Care
              //       Credit for Two or more Children	 ---
              if (entities.ChildCareTaxCreditFactors.
                MaxMonthlyCreditMultChildren < export
                .ParentATotalTaxCredit.TotalCurrency)
              {
                export.ParentATotalTaxCredit.TotalCurrency =
                  entities.ChildCareTaxCreditFactors.
                    MaxMonthlyCreditMultChildren;
              }

              break;
          }

          // --- Calculate the Kansas Child Care Credit ---
          if (AsChar(import.ParentA.EligibleForKansasTaxCredit) == 'Y')
          {
            local.ParentATotalTaxCredit.Count =
              (int)Math.Round(
                export.ParentATotalTaxCredit.TotalCurrency * (1 + entities
              .ChildCareTaxCreditFactors.KansasTaxCreditPercent / 100
              ), MidpointRounding.AwayFromZero);
            export.ParentATotalTaxCredit.TotalCurrency =
              local.ParentATotalTaxCredit.Count;
          }
        }
      }
      else
      {
        // -----     Will not arise     -----
      }

      // ----------------------------------------------
      // Calculate the Eligible Tax Credit for Parent B.
      // ----------------------------------------------
      local.Temp.AdjustedGrossIncomeMinimum =
        (int)(export.ParentBC5D1TotCsInc.TotalCurrency * 12);

      if (ReadChildCareTaxCreditFactors())
      {
        if (AsChar(import.ParentB.EligibleForFederalTaxCredit) == 'Y')
        {
          local.ParentBTotalTaxCredit.Count =
            (int)Math.Round(
              export.ParentB.WorkRelatedChildCareCosts.GetValueOrDefault() * entities
            .ChildCareTaxCreditFactors.FederalTaxCreditPercent /
            100, MidpointRounding.AwayFromZero);
          export.ParentBTotalTaxCredit.TotalCurrency =
            local.ParentBTotalTaxCredit.Count;

          // ---------------------------------------------
          //  Check if the Federal Child Care Credit
          //  exceeds the maximum monthly credit.
          // ---------------------------------------------
          switch(export.ParentB.NoOfChildrenInDayCare.GetValueOrDefault())
          {
            case 0:
              // --- No child in day care. Credit does not apply ---
              break;
            case 1:
              // --- Check the maximum Federal Child Care
              // 	    Credit for One Child	 ---
              if (entities.ChildCareTaxCreditFactors.MaxMonthlyCredit1Child < export
                .ParentBTotalTaxCredit.TotalCurrency)
              {
                export.ParentBTotalTaxCredit.TotalCurrency =
                  entities.ChildCareTaxCreditFactors.MaxMonthlyCredit1Child;
              }

              break;
            default:
              // --- Check the maximum Federal Child Care
              //       Credit for Two or more Children	 ---
              if (entities.ChildCareTaxCreditFactors.
                MaxMonthlyCreditMultChildren < export
                .ParentBTotalTaxCredit.TotalCurrency)
              {
                export.ParentBTotalTaxCredit.TotalCurrency =
                  entities.ChildCareTaxCreditFactors.
                    MaxMonthlyCreditMultChildren;
              }

              break;
          }

          // --- Calculate the Kansas Child Care Credit ---
          if (AsChar(import.ParentB.EligibleForKansasTaxCredit) == 'Y')
          {
            local.ParentBTotalTaxCredit.Count =
              (int)Math.Round(
                export.ParentBTotalTaxCredit.TotalCurrency * (1 + entities
              .ChildCareTaxCreditFactors.KansasTaxCreditPercent / 100
              ), MidpointRounding.AwayFromZero);
            export.ParentBTotalTaxCredit.TotalCurrency =
              local.ParentBTotalTaxCredit.Count;
          }
        }
      }
      else
      {
        // -----     Will not arise     -----
      }

      // --------------------------------------------
      // Calculate the Work Related Child Care Cost.
      // --------------------------------------------
      local.ParentAChildCareCost.Count =
        (int)Math.Round(
          import.ParentA.WorkRelatedChildCareCosts.GetValueOrDefault() -
        export.ParentATotalTaxCredit.TotalCurrency,
        MidpointRounding.AwayFromZero);
      export.ParentAChildCareCost.TotalCurrency =
        local.ParentAChildCareCost.Count;
      local.ParentBChildCareCost.Count =
        (int)Math.Round(
          import.ParentB.WorkRelatedChildCareCosts.GetValueOrDefault() -
        export.ParentBTotalTaxCredit.TotalCurrency,
        MidpointRounding.AwayFromZero);
      export.ParentBChildCareCost.TotalCurrency =
        local.ParentBChildCareCost.Count;
      export.TotalChildCareCost.TotalCurrency =
        export.ParentAChildCareCost.TotalCurrency + export
        .ParentBChildCareCost.TotalCurrency;

      // --------------------------------------------
      // Calculate the Total Child Support Obligation.
      // --------------------------------------------
      export.D6otalChildSuppOblig.TotalCurrency =
        export.CsObligTotalAmount.TotalCurrency + export
        .TotalInsurancePrem.TotalCurrency + export
        .TotalChildCareCost.TotalCurrency;

      // ------------------------------------------------
      // Calculate the Parental Child Support Obligation.
      // ------------------------------------------------
      local.ParentAD7CsOblig.Count =
        (int)Math.Round(
          export.D6otalChildSuppOblig.TotalCurrency * export
        .ParentAD2PercentInc.TotalCurrency /
        100, MidpointRounding.AwayFromZero);
      export.ParentAD7CsOblig.TotalCurrency = local.ParentAD7CsOblig.Count;
      local.ParentBD7CsOblig.Count =
        (int)Math.Round(
          export.D6otalChildSuppOblig.TotalCurrency * export
        .ParentBD2PercentInc.TotalCurrency /
        100, MidpointRounding.AwayFromZero);
      export.ParentBD7CsOblig.TotalCurrency = local.ParentBD7CsOblig.Count;

      // -----------------------------------------------
      // Calculate the adjustments for Insurance and
      // Child Care.
      // -----------------------------------------------
      export.ParentAD8Adjustments.TotalCurrency =
        import.ParentA.HealthAndDentalInsurancePrem.GetValueOrDefault() + export
        .ParentAChildCareCost.TotalCurrency;
      export.ParentBD8Adjustments.TotalCurrency =
        import.ParentB.HealthAndDentalInsurancePrem.GetValueOrDefault() + export
        .ParentBChildCareCost.TotalCurrency;

      // --------------------------------------------
      //   Calculate the Net Parental Child Support.
      // --------------------------------------------
      local.ParentAD9F1NetCs.Count =
        (int)Math.Round(
          export.ParentAD7CsOblig.TotalCurrency -
        export.ParentAD8Adjustments.TotalCurrency,
        MidpointRounding.AwayFromZero);
      export.ParentAD9F1NetCs.TotalCurrency = local.ParentAD9F1NetCs.Count;

      if (export.ParentAD9F1NetCs.TotalCurrency < 0)
      {
        export.ParentAD9F1NetCs.TotalCurrency = 0;
      }

      local.ParentBD9F1NetCs.Count =
        (int)Math.Round(
          export.ParentBD7CsOblig.TotalCurrency -
        export.ParentBD8Adjustments.TotalCurrency,
        MidpointRounding.AwayFromZero);
      export.ParentBD9F1NetCs.TotalCurrency = local.ParentBD9F1NetCs.Count;

      if (export.ParentBD9F1NetCs.TotalCurrency < 0)
      {
        export.ParentBD9F1NetCs.TotalCurrency = 0;
      }

      // --------------------------------------------
      // Calculate Total Adjustment from all Categories.
      // --------------------------------------------
      for(import.Import1.Index = 0; import.Import1.Index < import
        .Import1.Count; ++import.Import1.Index)
      {
        export.ParentAE7F2TotAdj.TotalCurrency += import.Import1.Item.ParentA.
          AdjustmentAmount.GetValueOrDefault();
        export.ParentBE7F2TotAdj.TotalCurrency += import.Import1.Item.ParentB.
          AdjustmentAmount.GetValueOrDefault();
      }

      local.ParentAE7F2TotAdj.Count =
        (int)Math.Round(
          export.ParentAE7F2TotAdj.TotalCurrency,
        MidpointRounding.AwayFromZero);
      export.ParentAE7F2TotAdj.TotalCurrency = local.ParentAE7F2TotAdj.Count;
      local.ParentBE7F2TotAdj.Count =
        (int)Math.Round(
          export.ParentBE7F2TotAdj.TotalCurrency,
        MidpointRounding.AwayFromZero);
      export.ParentBE7F2TotAdj.TotalCurrency = local.ParentBE7F2TotAdj.Count;
      export.ParentAF2TotalCsAdj.TotalCurrency =
        export.ParentAE7F2TotAdj.TotalCurrency;
      export.ParentBF2TotalCsAdj.TotalCurrency =
        export.ParentBE7F2TotAdj.TotalCurrency;

      // --------------------------------------------
      // Calculate Adjusted Child Support Obligation.
      // --------------------------------------------
      export.ParentAF3AdjCsOblig.TotalCurrency =
        export.ParentAD9F1NetCs.TotalCurrency + export
        .ParentAF2TotalCsAdj.TotalCurrency;

      if (export.ParentAF3AdjCsOblig.TotalCurrency < 0)
      {
        export.ParentAF3AdjCsOblig.TotalCurrency = 0;
      }

      export.ParentBF3AdjCsOblig.TotalCurrency =
        export.ParentBD9F1NetCs.TotalCurrency + export
        .ParentBF2TotalCsAdj.TotalCurrency;

      if (export.ParentBF3AdjCsOblig.TotalCurrency < 0)
      {
        export.ParentBF3AdjCsOblig.TotalCurrency = 0;
      }

      export.ParentA.NetAdjParentalChildSuppAmt =
        export.ParentAF3AdjCsOblig.TotalCurrency;
      export.ParentB.NetAdjParentalChildSuppAmt =
        export.ParentBF3AdjCsOblig.TotalCurrency;
      export.ParentA.EnforcementFeeType = "";
      export.ParentA.EnforcementFeeAllowance = 0;
      export.ParentB.EnforcementFeeType = "";
      export.ParentB.EnforcementFeeAllowance = 0;

      // CQ50299 12/9/2015  GVandy Changed "M" to "1".
      if (AsChar(import.Common.SelectChar) == '1')
      {
        if (import.Common.Percentage > 0)
        {
          export.ParentA.EnforcementFeeType = "P";
          export.ParentA.EnforcementFeeAllowance = import.Common.Percentage;
          export.ParentAEnfFee.TotalCurrency =
            Math.Round(
              export.ParentAF3AdjCsOblig.TotalCurrency * import
            .Common.Percentage / 100 * 0.5M, 2, MidpointRounding.AwayFromZero);
        }
        else
        {
          export.ParentA.EnforcementFeeType = "F";
          export.ParentA.EnforcementFeeAllowance =
            (int?)import.Common.TotalCurrency;
          export.ParentAEnfFee.TotalCurrency =
            Math.Round(
              import.Common.TotalCurrency *
            0.5M, 2, MidpointRounding.AwayFromZero);
        }

        // CQ50299 12/9/2015  GVandy Changed "F" to "2".
      }
      else if (AsChar(import.Common.SelectChar) == '2')
      {
        if (import.Common.Percentage > 0)
        {
          export.ParentB.EnforcementFeeType = "P";
          export.ParentB.EnforcementFeeAllowance = import.Common.Percentage;
          export.ParentBEnfFee.TotalCurrency =
            Math.Round(
              export.ParentBF3AdjCsOblig.TotalCurrency * import
            .Common.Percentage / 100 * 0.5M, 2, MidpointRounding.AwayFromZero);
        }
        else
        {
          export.ParentB.EnforcementFeeType = "F";
          export.ParentB.EnforcementFeeAllowance =
            (int?)import.Common.TotalCurrency;
          export.ParentBEnfFee.TotalCurrency =
            Math.Round(
              import.Common.TotalCurrency *
            0.5M, 2, MidpointRounding.AwayFromZero);
        }
      }

      export.ParentAD10F1NetCs.TotalCurrency =
        export.ParentAF3AdjCsOblig.TotalCurrency + export
        .ParentAEnfFee.TotalCurrency;
      export.ParentBD10F1NetCs.TotalCurrency =
        export.ParentBF3AdjCsOblig.TotalCurrency + export
        .ParentBEnfFee.TotalCurrency;

      if (export.ParentAF3AdjCsOblig.TotalCurrency != 0 || export
        .ParentBF3AdjCsOblig.TotalCurrency != 0)
      {
        ExitState = "OE0000_CALCULATE_SUCCESSFUL";
      }
      else
      {
        ExitState = "OE0000_CALCULATE_UNSUCCESSFUL";
      }
    }
  }

  private bool ReadAgeGroupSupportSchedule1()
  {
    entities.AgeGroupSupportSchedule.Populated = false;

    return Read("ReadAgeGroupSupportSchedule1",
      (db, command) =>
      {
        db.SetInt32(
          command, "cssIdentifier", entities.ChildSupportSchedule.Identifier);
        db.SetInt32(
          command, "cssGuidelineYr",
          entities.ChildSupportSchedule.CsGuidelineYear);
      },
      (db, reader) =>
      {
        entities.AgeGroupSupportSchedule.CssIdentifier = db.GetInt32(reader, 0);
        entities.AgeGroupSupportSchedule.MaximumAgeInARange =
          db.GetInt32(reader, 1);
        entities.AgeGroupSupportSchedule.AgeGroupFactor =
          db.GetDecimal(reader, 2);
        entities.AgeGroupSupportSchedule.CssGuidelineYr =
          db.GetInt32(reader, 3);
        entities.AgeGroupSupportSchedule.Populated = true;
      });
  }

  private bool ReadAgeGroupSupportSchedule2()
  {
    entities.AgeGroupSupportSchedule.Populated = false;

    return Read("ReadAgeGroupSupportSchedule2",
      (db, command) =>
      {
        db.SetInt32(
          command, "cssIdentifier", entities.ChildSupportSchedule.Identifier);
        db.SetInt32(
          command, "cssGuidelineYr",
          entities.ChildSupportSchedule.CsGuidelineYear);
      },
      (db, reader) =>
      {
        entities.AgeGroupSupportSchedule.CssIdentifier = db.GetInt32(reader, 0);
        entities.AgeGroupSupportSchedule.MaximumAgeInARange =
          db.GetInt32(reader, 1);
        entities.AgeGroupSupportSchedule.AgeGroupFactor =
          db.GetDecimal(reader, 2);
        entities.AgeGroupSupportSchedule.CssGuidelineYr =
          db.GetInt32(reader, 3);
        entities.AgeGroupSupportSchedule.Populated = true;
      });
  }

  private bool ReadAgeGroupSupportSchedule3()
  {
    entities.AgeGroupSupportSchedule.Populated = false;

    return Read("ReadAgeGroupSupportSchedule3",
      (db, command) =>
      {
        db.SetInt32(
          command, "cssIdentifier", entities.ChildSupportSchedule.Identifier);
        db.SetInt32(
          command, "cssGuidelineYr",
          entities.ChildSupportSchedule.CsGuidelineYear);
      },
      (db, reader) =>
      {
        entities.AgeGroupSupportSchedule.CssIdentifier = db.GetInt32(reader, 0);
        entities.AgeGroupSupportSchedule.MaximumAgeInARange =
          db.GetInt32(reader, 1);
        entities.AgeGroupSupportSchedule.AgeGroupFactor =
          db.GetDecimal(reader, 2);
        entities.AgeGroupSupportSchedule.CssGuidelineYr =
          db.GetInt32(reader, 3);
        entities.AgeGroupSupportSchedule.Populated = true;
      });
  }

  private bool ReadChildCareTaxCreditFactors()
  {
    entities.ChildCareTaxCreditFactors.Populated = false;

    return Read("ReadChildCareTaxCreditFactors",
      (db, command) =>
      {
        db.SetInt32(
          command, "adjGrossIncMax", local.Temp.AdjustedGrossIncomeMinimum);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ChildCareTaxCreditFactors.Identifier = db.GetInt32(reader, 0);
        entities.ChildCareTaxCreditFactors.ExpirationDate =
          db.GetDate(reader, 1);
        entities.ChildCareTaxCreditFactors.EffectiveDate =
          db.GetDate(reader, 2);
        entities.ChildCareTaxCreditFactors.AdjustedGrossIncomeMaximum =
          db.GetInt32(reader, 3);
        entities.ChildCareTaxCreditFactors.AdjustedGrossIncomeMinimum =
          db.GetInt32(reader, 4);
        entities.ChildCareTaxCreditFactors.KansasTaxCreditPercent =
          db.GetDecimal(reader, 5);
        entities.ChildCareTaxCreditFactors.FederalTaxCreditPercent =
          db.GetDecimal(reader, 6);
        entities.ChildCareTaxCreditFactors.MaxMonthlyCreditMultChildren =
          db.GetInt32(reader, 7);
        entities.ChildCareTaxCreditFactors.MaxMonthlyCredit1Child =
          db.GetInt32(reader, 8);
        entities.ChildCareTaxCreditFactors.Populated = true;
      });
  }

  private bool ReadChildSupportSchedule()
  {
    entities.ChildSupportSchedule.Populated = false;

    return Read("ReadChildSupportSchedule",
      (db, command) =>
      {
        db.SetInt32(command, "count", local.TotalNoOfChildren.Count);
        db.SetInt32(
          command, "csGuidelineYear",
          import.ChildSupportWorksheet.CsGuidelineYear);
      },
      (db, reader) =>
      {
        entities.ChildSupportSchedule.Identifier = db.GetInt32(reader, 0);
        entities.ChildSupportSchedule.ExpirationDate = db.GetDate(reader, 1);
        entities.ChildSupportSchedule.EffectiveDate =
          db.GetNullableDate(reader, 2);
        entities.ChildSupportSchedule.MonthlyIncomePovertyLevelInd =
          db.GetInt32(reader, 3);
        entities.ChildSupportSchedule.IncomeMultiplier =
          db.GetDecimal(reader, 4);
        entities.ChildSupportSchedule.IncomeExponent = db.GetDecimal(reader, 5);
        entities.ChildSupportSchedule.NumberOfChildrenInFamily =
          db.GetInt32(reader, 6);
        entities.ChildSupportSchedule.CsGuidelineYear = db.GetInt32(reader, 7);
        entities.ChildSupportSchedule.Populated = true;
      });
  }

  private bool ReadCodeValue()
  {
    entities.CodeValue.Populated = false;

    return Read("ReadCodeValue",
      null,
      (db, reader) =>
      {
        entities.CodeValue.Id = db.GetInt32(reader, 0);
        entities.CodeValue.CodId = db.GetNullableInt32(reader, 1);
        entities.CodeValue.Cdvalue = db.GetString(reader, 2);
        entities.CodeValue.Description = db.GetString(reader, 3);
        entities.CodeValue.Populated = true;
      });
  }

  private bool ReadCsGrossMonthlyIncSched1()
  {
    entities.CsGrossMonthlyIncSched.Populated = false;

    return Read("ReadCsGrossMonthlyIncSched1",
      (db, command) =>
      {
        db.
          SetDecimal(command, "totalCurrency", export.D1otalCsInc.TotalCurrency);
          
        db.SetInt32(
          command, "cssIdentifier", entities.ChildSupportSchedule.Identifier);
        db.SetInt32(
          command, "cssGuidelineYr",
          entities.ChildSupportSchedule.CsGuidelineYear);
      },
      (db, reader) =>
      {
        entities.CsGrossMonthlyIncSched.CssIdentifier = db.GetInt32(reader, 0);
        entities.CsGrossMonthlyIncSched.AgsMaxAgeRange = db.GetInt32(reader, 1);
        entities.CsGrossMonthlyIncSched.CombinedGrossMnthlyIncomeAmt =
          db.GetInt32(reader, 2);
        entities.CsGrossMonthlyIncSched.PerChildSupportAmount =
          db.GetInt32(reader, 3);
        entities.CsGrossMonthlyIncSched.CssGuidelineYr = db.GetInt32(reader, 4);
        entities.CsGrossMonthlyIncSched.Populated = true;
      });
  }

  private bool ReadCsGrossMonthlyIncSched2()
  {
    entities.CsGrossMonthlyIncSched.Populated = false;

    return Read("ReadCsGrossMonthlyIncSched2",
      (db, command) =>
      {
        db.SetInt32(
          command, "cssIdentifier", entities.ChildSupportSchedule.Identifier);
        db.SetInt32(
          command, "cssGuidelineYr",
          entities.ChildSupportSchedule.CsGuidelineYear);
        db.
          SetDecimal(command, "totalCurrency", export.D1otalCsInc.TotalCurrency);
          
      },
      (db, reader) =>
      {
        entities.CsGrossMonthlyIncSched.CssIdentifier = db.GetInt32(reader, 0);
        entities.CsGrossMonthlyIncSched.AgsMaxAgeRange = db.GetInt32(reader, 1);
        entities.CsGrossMonthlyIncSched.CombinedGrossMnthlyIncomeAmt =
          db.GetInt32(reader, 2);
        entities.CsGrossMonthlyIncSched.PerChildSupportAmount =
          db.GetInt32(reader, 3);
        entities.CsGrossMonthlyIncSched.CssGuidelineYr = db.GetInt32(reader, 4);
        entities.CsGrossMonthlyIncSched.Populated = true;
      });
  }

  private bool ReadCsGrossMonthlyIncSched3()
  {
    System.Diagnostics.Debug.Assert(entities.AgeGroupSupportSchedule.Populated);
    entities.CsGrossMonthlyIncSched.Populated = false;

    return Read("ReadCsGrossMonthlyIncSched3",
      (db, command) =>
      {
        db.SetInt32(
          command, "agsMaxAgeRange",
          entities.AgeGroupSupportSchedule.MaximumAgeInARange);
        db.SetInt32(
          command, "cssIdentifier",
          entities.AgeGroupSupportSchedule.CssIdentifier);
        db.SetInt32(
          command, "cssGuidelineYr",
          entities.AgeGroupSupportSchedule.CssGuidelineYr);
        db.
          SetDecimal(command, "totalCurrency", local.D1otalCsInc.TotalCurrency);
          
      },
      (db, reader) =>
      {
        entities.CsGrossMonthlyIncSched.CssIdentifier = db.GetInt32(reader, 0);
        entities.CsGrossMonthlyIncSched.AgsMaxAgeRange = db.GetInt32(reader, 1);
        entities.CsGrossMonthlyIncSched.CombinedGrossMnthlyIncomeAmt =
          db.GetInt32(reader, 2);
        entities.CsGrossMonthlyIncSched.PerChildSupportAmount =
          db.GetInt32(reader, 3);
        entities.CsGrossMonthlyIncSched.CssGuidelineYr = db.GetInt32(reader, 4);
        entities.CsGrossMonthlyIncSched.Populated = true;
      });
  }

  private bool ReadCsGrossMonthlyIncSched4()
  {
    entities.CsGrossMonthlyIncSched.Populated = false;

    return Read("ReadCsGrossMonthlyIncSched4",
      (db, command) =>
      {
        db.SetInt32(
          command, "cssIdentifier", entities.ChildSupportSchedule.Identifier);
        db.SetInt32(
          command, "cssGuidelineYr",
          entities.ChildSupportSchedule.CsGuidelineYear);
        db.
          SetDecimal(command, "totalCurrency", export.D1otalCsInc.TotalCurrency);
          
      },
      (db, reader) =>
      {
        entities.CsGrossMonthlyIncSched.CssIdentifier = db.GetInt32(reader, 0);
        entities.CsGrossMonthlyIncSched.AgsMaxAgeRange = db.GetInt32(reader, 1);
        entities.CsGrossMonthlyIncSched.CombinedGrossMnthlyIncomeAmt =
          db.GetInt32(reader, 2);
        entities.CsGrossMonthlyIncSched.PerChildSupportAmount =
          db.GetInt32(reader, 3);
        entities.CsGrossMonthlyIncSched.CssGuidelineYr = db.GetInt32(reader, 4);
        entities.CsGrossMonthlyIncSched.Populated = true;
      });
  }

  private bool ReadCsGrossMonthlyIncSched5()
  {
    entities.CsGrossMonthlyIncSched.Populated = false;

    return Read("ReadCsGrossMonthlyIncSched5",
      (db, command) =>
      {
        db.SetInt32(
          command, "cssIdentifier", entities.ChildSupportSchedule.Identifier);
        db.SetInt32(
          command, "cssGuidelineYr",
          entities.ChildSupportSchedule.CsGuidelineYear);
      },
      (db, reader) =>
      {
        entities.CsGrossMonthlyIncSched.CssIdentifier = db.GetInt32(reader, 0);
        entities.CsGrossMonthlyIncSched.AgsMaxAgeRange = db.GetInt32(reader, 1);
        entities.CsGrossMonthlyIncSched.CombinedGrossMnthlyIncomeAmt =
          db.GetInt32(reader, 2);
        entities.CsGrossMonthlyIncSched.PerChildSupportAmount =
          db.GetInt32(reader, 3);
        entities.CsGrossMonthlyIncSched.CssGuidelineYr = db.GetInt32(reader, 4);
        entities.CsGrossMonthlyIncSched.Populated = true;
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
      public const int Capacity = 7;

      private Common work;
      private CsePersonSupportAdjustment parentB;
      private CsePersonSupportAdjustment parentA;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
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
    /// A value of ChildSupportWorksheet.
    /// </summary>
    [JsonPropertyName("childSupportWorksheet")]
    public ChildSupportWorksheet ChildSupportWorksheet
    {
      get => childSupportWorksheet ??= new();
      set => childSupportWorksheet = value;
    }

    /// <summary>
    /// A value of ParentB.
    /// </summary>
    [JsonPropertyName("parentB")]
    public CsePersonSupportWorksheet ParentB
    {
      get => parentB ??= new();
      set => parentB = value;
    }

    /// <summary>
    /// A value of ParentA.
    /// </summary>
    [JsonPropertyName("parentA")]
    public CsePersonSupportWorksheet ParentA
    {
      get => parentA ??= new();
      set => parentA = value;
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

    private Common common;
    private Array<ImportGroup> import1;
    private ChildSupportWorksheet childSupportWorksheet;
    private CsePersonSupportWorksheet parentB;
    private CsePersonSupportWorksheet parentA;
    private Gimport2020EnterableFieldsGroup gimport2020EnterableFields;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A Gexport2020CalculationsGroup group.</summary>
    [Serializable]
    public class Gexport2020CalculationsGroup
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
    /// A value of ParentB.
    /// </summary>
    [JsonPropertyName("parentB")]
    public CsePersonSupportWorksheet ParentB
    {
      get => parentB ??= new();
      set => parentB = value;
    }

    /// <summary>
    /// A value of ParentA.
    /// </summary>
    [JsonPropertyName("parentA")]
    public CsePersonSupportWorksheet ParentA
    {
      get => parentA ??= new();
      set => parentA = value;
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
    /// Gets a value of Gexport2020Calculations.
    /// </summary>
    [JsonPropertyName("gexport2020Calculations")]
    public Gexport2020CalculationsGroup Gexport2020Calculations
    {
      get => gexport2020Calculations ?? (gexport2020Calculations = new());
      set => gexport2020Calculations = value;
    }

    /// <summary>
    /// A value of IncomeBeyondSchedule.
    /// </summary>
    [JsonPropertyName("incomeBeyondSchedule")]
    public Common IncomeBeyondSchedule
    {
      get => incomeBeyondSchedule ??= new();
      set => incomeBeyondSchedule = value;
    }

    private Common parentBF2TotalCsAdj;
    private Common parentAF2TotalCsAdj;
    private Common d1otalCsInc;
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
    private CsePersonSupportWorksheet parentB;
    private CsePersonSupportWorksheet parentA;
    private Common parentAD10F1NetCs;
    private Common parentBD10F1NetCs;
    private Common parentAEnfFee;
    private Common parentBEnfFee;
    private Gexport2020CalculationsGroup gexport2020Calculations;
    private Common incomeBeyondSchedule;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public ChildCareTaxCreditFactors Temp
    {
      get => temp ??= new();
      set => temp = value;
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
    /// A value of ParentBE7F2TotAdj.
    /// </summary>
    [JsonPropertyName("parentBE7F2TotAdj")]
    public Common ParentBE7F2TotAdj
    {
      get => parentBE7F2TotAdj ??= new();
      set => parentBE7F2TotAdj = value;
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
    /// A value of ParentBD9F1NetCs.
    /// </summary>
    [JsonPropertyName("parentBD9F1NetCs")]
    public Common ParentBD9F1NetCs
    {
      get => parentBD9F1NetCs ??= new();
      set => parentBD9F1NetCs = value;
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
    /// A value of ParentBChildCareCost.
    /// </summary>
    [JsonPropertyName("parentBChildCareCost")]
    public Common ParentBChildCareCost
    {
      get => parentBChildCareCost ??= new();
      set => parentBChildCareCost = value;
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
    /// A value of ParentBD2PercentInc.
    /// </summary>
    [JsonPropertyName("parentBD2PercentInc")]
    public OeWorkGroup ParentBD2PercentInc
    {
      get => parentBD2PercentInc ??= new();
      set => parentBD2PercentInc = value;
    }

    /// <summary>
    /// A value of ParentAD2PercentInc.
    /// </summary>
    [JsonPropertyName("parentAD2PercentInc")]
    public OeWorkGroup ParentAD2PercentInc
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
    /// A value of ParentBC1TotGrassInc.
    /// </summary>
    [JsonPropertyName("parentBC1TotGrassInc")]
    public Common ParentBC1TotGrassInc
    {
      get => parentBC1TotGrassInc ??= new();
      set => parentBC1TotGrassInc = value;
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
    /// A value of NextToTotalCsInc.
    /// </summary>
    [JsonPropertyName("nextToTotalCsInc")]
    public CsGrossMonthlyIncSched NextToTotalCsInc
    {
      get => nextToTotalCsInc ??= new();
      set => nextToTotalCsInc = value;
    }

    /// <summary>
    /// A value of PrevToTotalCsInc.
    /// </summary>
    [JsonPropertyName("prevToTotalCsInc")]
    public CsGrossMonthlyIncSched PrevToTotalCsInc
    {
      get => prevToTotalCsInc ??= new();
      set => prevToTotalCsInc = value;
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
    /// A value of ParentBFedTaxCredit.
    /// </summary>
    [JsonPropertyName("parentBFedTaxCredit")]
    public Common ParentBFedTaxCredit
    {
      get => parentBFedTaxCredit ??= new();
      set => parentBFedTaxCredit = value;
    }

    /// <summary>
    /// A value of ParentAFedTaxCredit.
    /// </summary>
    [JsonPropertyName("parentAFedTaxCredit")]
    public Common ParentAFedTaxCredit
    {
      get => parentAFedTaxCredit ??= new();
      set => parentAFedTaxCredit = value;
    }

    /// <summary>
    /// A value of ParentBKsTaxCredit.
    /// </summary>
    [JsonPropertyName("parentBKsTaxCredit")]
    public Common ParentBKsTaxCredit
    {
      get => parentBKsTaxCredit ??= new();
      set => parentBKsTaxCredit = value;
    }

    /// <summary>
    /// A value of ParentAKsTaxCredit.
    /// </summary>
    [JsonPropertyName("parentAKsTaxCredit")]
    public Common ParentAKsTaxCredit
    {
      get => parentAKsTaxCredit ??= new();
      set => parentAKsTaxCredit = value;
    }

    /// <summary>
    /// A value of TotalNoOfChildren.
    /// </summary>
    [JsonPropertyName("totalNoOfChildren")]
    public Common TotalNoOfChildren
    {
      get => totalNoOfChildren ??= new();
      set => totalNoOfChildren = value;
    }

    /// <summary>
    /// A value of Local1618SupportAmt.
    /// </summary>
    [JsonPropertyName("local1618SupportAmt")]
    public Common Local1618SupportAmt
    {
      get => local1618SupportAmt ??= new();
      set => local1618SupportAmt = value;
    }

    /// <summary>
    /// A value of Local715SupportAmt.
    /// </summary>
    [JsonPropertyName("local715SupportAmt")]
    public Common Local715SupportAmt
    {
      get => local715SupportAmt ??= new();
      set => local715SupportAmt = value;
    }

    /// <summary>
    /// A value of Local06SupportAmt.
    /// </summary>
    [JsonPropertyName("local06SupportAmt")]
    public Common Local06SupportAmt
    {
      get => local06SupportAmt ??= new();
      set => local06SupportAmt = value;
    }

    private Common common;
    private ChildCareTaxCreditFactors temp;
    private DateWorkArea current;
    private Common parentBE7F2TotAdj;
    private Common parentAE7F2TotAdj;
    private Common parentBD9F1NetCs;
    private Common parentAD9F1NetCs;
    private Common parentBD7CsOblig;
    private Common parentAD7CsOblig;
    private Common parentBChildCareCost;
    private Common parentAChildCareCost;
    private Common parentBTotalTaxCredit;
    private Common parentATotalTaxCredit;
    private Common totalInsurancePrem;
    private OeWorkGroup parentBD2PercentInc;
    private OeWorkGroup parentAD2PercentInc;
    private Common parentBC5D1TotCsInc;
    private Common parentAC5D1TotCsInc;
    private Common parentBC1TotGrassInc;
    private Common parentAC1TotGrossInc;
    private Common parentBB3SeGrossInc;
    private Common parentAB3SeGrossInc;
    private CsGrossMonthlyIncSched nextToTotalCsInc;
    private CsGrossMonthlyIncSched prevToTotalCsInc;
    private Common d1otalCsInc;
    private Common parentBFedTaxCredit;
    private Common parentAFedTaxCredit;
    private Common parentBKsTaxCredit;
    private Common parentAKsTaxCredit;
    private Common totalNoOfChildren;
    private Common local1618SupportAmt;
    private Common local715SupportAmt;
    private Common local06SupportAmt;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ChildCareTaxCreditFactors.
    /// </summary>
    [JsonPropertyName("childCareTaxCreditFactors")]
    public ChildCareTaxCreditFactors ChildCareTaxCreditFactors
    {
      get => childCareTaxCreditFactors ??= new();
      set => childCareTaxCreditFactors = value;
    }

    /// <summary>
    /// A value of CsGrossMonthlyIncSched.
    /// </summary>
    [JsonPropertyName("csGrossMonthlyIncSched")]
    public CsGrossMonthlyIncSched CsGrossMonthlyIncSched
    {
      get => csGrossMonthlyIncSched ??= new();
      set => csGrossMonthlyIncSched = value;
    }

    /// <summary>
    /// A value of ChildSupportSchedule.
    /// </summary>
    [JsonPropertyName("childSupportSchedule")]
    public ChildSupportSchedule ChildSupportSchedule
    {
      get => childSupportSchedule ??= new();
      set => childSupportSchedule = value;
    }

    /// <summary>
    /// A value of AgeGroupSupportSchedule.
    /// </summary>
    [JsonPropertyName("ageGroupSupportSchedule")]
    public AgeGroupSupportSchedule AgeGroupSupportSchedule
    {
      get => ageGroupSupportSchedule ??= new();
      set => ageGroupSupportSchedule = value;
    }

    private CodeValue codeValue;
    private Code code;
    private CsePerson csePerson;
    private Case1 case1;
    private CaseRole caseRole;
    private ChildCareTaxCreditFactors childCareTaxCreditFactors;
    private CsGrossMonthlyIncSched csGrossMonthlyIncSched;
    private ChildSupportSchedule childSupportSchedule;
    private AgeGroupSupportSchedule ageGroupSupportSchedule;
  }
#endregion
}
