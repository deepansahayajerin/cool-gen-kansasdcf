// Program: SP_MDBG_MAINTAIN_DASHBOARD_GOALS, ID: 945142195, model: 746.
// Short name: SWEMDBGP
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
/// A program: SP_MDBG_MAINTAIN_DASHBOARD_GOALS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpMdbgMaintainDashboardGoals: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_MDBG_MAINTAIN_DASHBOARD_GOALS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpMdbgMaintainDashboardGoals(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpMdbgMaintainDashboardGoals.
  /// </summary>
  public SpMdbgMaintainDashboardGoals(IContext context, Import import,
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
    // ---------------------------------------------------------------------------------------------------
    // ---
    // 
    // ---
    // ---
    // 
    // Maintain Full Time Equivalent
    // --
    // -
    // ---
    // 
    // ---
    // ---------------------------------------------------------------------------------------------------
    // ---------------------------------------------------------------------------------------------------
    //                                     
    // C H A N G E    L O G
    // ---------------------------------------------------------------------------------------------------
    // Date      Developer     Request #	Description
    // --------  ----------    ----------	
    // -----------------------------------------------------------
    // 06/10/13  GVandy	CQ36547		Initial Development.  Created from a copy of 
    // ASLM.
    // 			Segment B	
    // ---------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    if (Equal(global.Command, "CLEAR"))
    {
      // ---------------------------------------------
      // Clear scrolling group if command=clear.
      // ---------------------------------------------
      ExitState = "ACO_NI0000_FIELDS_CLEARED";

      return;
    }

    MoveCseOrganization(import.CseOrganization, export.CseOrganization);
    export.HiddenCseOrganization.Code = import.HiddenCseOrganization.Code;
    export.ServiceProvider.Assign(import.ServiceProvider);
    export.WorkerName.Text50 = import.WorkerName.Text50;
    export.HiddenServiceProvider.UserId = import.HiddenServiceProvider.UserId;
    MoveWorkArea2(import.YearMonth, export.YearMonth);
    MoveWorkArea2(import.HiddenYearMonth, export.YearMonthHidden);
    export.Jd.SelectChar = import.Jd.SelectChar;
    export.Worker.SelectChar = import.Worker.SelectChar;
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
      import.Group.Index)
    {
      if (!import.Group.CheckSize())
      {
        break;
      }

      export.Group.Index = import.Group.Index;
      export.Group.CheckSize();

      export.Group.Update.WorkArea.Assign(import.Group.Item.WorkArea);
      export.Group.Update.Hidden.Text9 = import.Group.Item.Hidden.Text9;
      export.Group.Update.NumberOfDecimals.Count =
        import.Group.Item.NumberOfDecimals.Count;
    }

    import.Group.CheckIndex();

    // **** All valid commands for this AB is validated in the following CASE OF
    // ****
    switch(TrimEnd(global.Command))
    {
      case "ENTER":
        if (!IsEmpty(import.Standard.NextTransaction))
        {
          // ---------------------------------------------
          // User is going out of this screen to another
          // ---------------------------------------------
          local.NextTranInfo.Assign(import.HiddenNextTranInfo);

          // ---------------------------------------------
          // Set up local next_tran_info for saving the current values for the 
          // next screen
          // ---------------------------------------------
          UseScCabNextTranPut();

          return;
        }

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "EXIT":
        ExitState = "ECO_XFR_TO_MENU";

        return;
      case "NEXT":
        ExitState = "CO0000_SCROLLED_BEYOND_LAST_PG";

        break;
      case "PREV":
        ExitState = "CO0000_SCROLLED_BEYOND_1ST_PG";

        break;
      case "DISPLAY":
        // Display logic is located at bottom of PrAD.
        break;
      case "ADD":
        // **** Common logic for ADD, Delete, List and Update commands located 
        // below. ****
        break;
      case "DELETE":
        // **** Common logic for ADD, Delete, List and Update commands located 
        // below. ****
        break;
      case "LIST":
        // **** Common logic for ADD, Delete, List and Update commands located 
        // below. ****
        break;
      case "UPDATE":
        // **** Common logic for ADD, Delete, List and Update commands located 
        // below. ****
        break;
      case "XXNEXTXX":
        // ---------------------------------------------
        // User entered this screen from another screen
        // ---------------------------------------------
        UseScCabNextTranGet();

        // ---------------------------------------------
        // Populate export views from local next_tran_info view read from the 
        // data base
        // Set command to initial command required or ESCAPE
        // ---------------------------------------------
        export.HiddenNextTranInfo.Assign(local.NextTranInfo);

        return;
      case "FROMMENU":
        // -- Display screen.  No additional processing required.
        return;
      case "RETCSOR":
        export.Jd.SelectChar = "";
        export.ServiceProvider.Assign(local.NullServiceProvider);
        export.HiddenServiceProvider.UserId = local.NullServiceProvider.UserId;
        export.WorkerName.Text50 = "";

        if (AsChar(import.FromCsor.Type1) == 'J')
        {
          MoveCseOrganization(import.FromCsor, export.CseOrganization);
          global.Command = "DISPLAY";
        }
        else if (!IsEmpty(import.FromCsor.Code))
        {
          ExitState = "SP0000_MUST_SELECT_JD";

          return;
        }

        break;
      case "RETSVPL":
        export.Worker.SelectChar = "";
        export.ServiceProvider.Assign(import.FromSvpl);
        MoveCseOrganization(local.NullCseOrganization, export.CseOrganization);
        export.HiddenCseOrganization.Code = local.NullCseOrganization.Code;
        export.WorkerName.Text50 = TrimEnd(export.ServiceProvider.LastName) + ","
          + TrimEnd(export.ServiceProvider.FirstName) + " " + export
          .ServiceProvider.MiddleInitial;
        global.Command = "DISPLAY";

        break;
      default:
        ExitState = "ZD_ACO_NE0000_INVALID_PF_KEY1";

        return;
    }

    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "ADD") || Equal
      (global.Command, "DELETE") || Equal(global.Command, "UPDATE"))
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

    // ---------------------------------------------
    // Must display before maintenance.
    // ---------------------------------------------
    if (Equal(global.Command, "UPDATE") || Equal(global.Command, "DELETE"))
    {
      if (Equal(import.YearMonth.Text2, import.HiddenYearMonth.Text2) && Equal
        (import.YearMonth.Text4, import.HiddenYearMonth.Text4) && Equal
        (export.CseOrganization.Code, export.HiddenCseOrganization.Code) && Equal
        (export.ServiceProvider.UserId, export.HiddenServiceProvider.UserId))
      {
      }
      else
      {
        var field1 = GetField(export.ServiceProvider, "userId");

        field1.Error = true;

        var field2 = GetField(export.CseOrganization, "code");

        field2.Error = true;

        ExitState = "ACO_NE0000_DISPLAY_FIRST";

        return;
      }
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE") || Equal
      (global.Command, "DELETE") || Equal(global.Command, "DISPLAY"))
    {
      if (IsEmpty(export.YearMonth.Text2))
      {
        // -- Default to current month.
        export.YearMonth.Text2 = NumberToString(Now().Date.Month, 14, 2);
      }

      if (Verify(export.YearMonth.Text2, " 0123456789") > 0)
      {
        var field = GetField(export.YearMonth, "text2");

        field.Error = true;

        ExitState = "INVALID_MONTH_ENTERED";
      }
      else
      {
        local.StaffingMonth.Month = (int)StringToNumber(export.YearMonth.Text2);

        if (local.StaffingMonth.Month < 1 || local.StaffingMonth.Month > 12)
        {
          var field = GetField(export.YearMonth, "text2");

          field.Error = true;

          ExitState = "INVALID_MONTH_ENTERED";
        }
      }

      if (IsEmpty(export.YearMonth.Text4))
      {
        // -- Default to current year.
        export.YearMonth.Text4 = NumberToString(Now().Date.Year, 12, 4);
      }

      if (Lt(export.YearMonth.Text4, "0000") || Lt
        ("9999", export.YearMonth.Text4) || Verify
        (export.YearMonth.Text4, "0123456789") > 0)
      {
        var field = GetField(export.YearMonth, "text4");

        field.Error = true;

        ExitState = "OE0000_INVALID_YEAR";
      }
      else
      {
        local.StaffingMonth.Year = (int)StringToNumber(export.YearMonth.Text4);
      }

      local.StaffingMonth.YearMonth = local.StaffingMonth.Year * 100 + local
        .StaffingMonth.Month;

      if (!IsEmpty(export.CseOrganization.Code) && !
        IsEmpty(export.ServiceProvider.UserId))
      {
        var field1 = GetField(export.ServiceProvider, "userId");

        field1.Error = true;

        var field2 = GetField(export.CseOrganization, "code");

        field2.Error = true;

        ExitState = "SP0000_ENTER_JD_OR_WORKER";
        export.Group.Count = 0;
      }
      else if (!IsEmpty(export.CseOrganization.Code))
      {
        local.JdDashboardPerformanceMetrics.Type1 = "GOAL";
        local.JdDashboardPerformanceMetrics.ReportLevel = "JD";
        local.JdDashboardPerformanceMetrics.ReportLevelId =
          export.CseOrganization.Code;
        local.JdDashboardOutputMetrics.Type1 = "GOAL";
        local.JdDashboardOutputMetrics.ReportLevel = "JD";
        local.JdDashboardOutputMetrics.ReportLevelId =
          export.CseOrganization.Code;
        local.JdDashboardPerformanceMetrics.ReportMonth =
          local.StaffingMonth.YearMonth;
        local.JdDashboardOutputMetrics.ReportMonth =
          local.StaffingMonth.YearMonth;

        if (ReadCseOrganization())
        {
          MoveCseOrganization(entities.CseOrganization, export.CseOrganization);
        }
        else
        {
          export.CseOrganization.Name = "";

          var field = GetField(export.CseOrganization, "code");

          field.Error = true;

          ExitState = "SP0000_JUDICIAL_DISTRICT_NF";
          export.Group.Count = 0;
        }

        export.ServiceProvider.Assign(local.NullServiceProvider);
        export.HiddenServiceProvider.UserId = local.NullServiceProvider.UserId;
        export.WorkerName.Text50 = "";
      }
      else if (!IsEmpty(export.ServiceProvider.UserId))
      {
        local.Worker.Type1 = "GOAL";
        local.Worker.ReportLevel = "CW";
        local.Worker.ReportLevelId = export.ServiceProvider.UserId;
        local.Worker.ReportMonth = local.StaffingMonth.YearMonth;

        if (ReadServiceProvider())
        {
          export.ServiceProvider.Assign(entities.ServiceProvider);
          export.WorkerName.Text50 =
            TrimEnd(export.ServiceProvider.LastName) + "," + TrimEnd
            (export.ServiceProvider.FirstName) + " " + export
            .ServiceProvider.MiddleInitial;
        }
        else
        {
          var field = GetField(export.ServiceProvider, "userId");

          field.Error = true;

          ExitState = "SERVICE_PROVIDER_NF";
          export.WorkerName.Text50 = "";
          export.Group.Count = 0;
        }

        MoveCseOrganization(local.NullCseOrganization, export.CseOrganization);
        export.HiddenCseOrganization.Code = local.NullCseOrganization.Code;
      }
      else
      {
        var field1 = GetField(export.ServiceProvider, "userId");

        field1.Error = true;

        var field2 = GetField(export.CseOrganization, "code");

        field2.Error = true;

        ExitState = "SP0000_ENTER_JD_OR_WORKER";
        export.Group.Count = 0;
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      if (Equal(global.Command, "UPDATE") || Equal(global.Command, "DELETE"))
      {
        local.CurrentMonth.Text2 = NumberToString(Now().Date.Month, 14, 2);
        local.CurrentMonth.Text4 = NumberToString(Now().Date.Year, 12, 4);

        // -- Cannot update or delete goals for previous months.
        if (Lt(export.YearMonth.Text4, local.CurrentMonth.Text4) || Equal
          (export.YearMonth.Text4, local.CurrentMonth.Text4) && Lt
          (export.YearMonth.Text2, local.CurrentMonth.Text2))
        {
          var field1 = GetField(export.YearMonth, "text2");

          field1.Error = true;

          var field2 = GetField(export.YearMonth, "text4");

          field2.Error = true;

          ExitState = "SP0000_CANNOT_UPDATE_PRIOR_GOALS";
        }
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
    {
      // ---------------------------------------------
      // Perform data validation.
      // ---------------------------------------------
      // -- Formatting edits and numeric conversion...
      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        if (!export.Group.CheckSize())
        {
          break;
        }

        // -- Validate there are no characters other than numbers zero through 
        // 9, decimal point, negative sign, and space.
        local.Temp.TotalInteger =
          Verify(export.Group.Item.WorkArea.Text9, " 0123456789.+-");

        if (local.Temp.TotalInteger > 0)
        {
          var field = GetField(export.Group.Item.WorkArea, "text9");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NON_NUMERIC_DATA_ENTERED";
          }

          continue;
        }

        local.ValidNumber.Flag = "Y";
        local.DecimalFound.Flag = "N";
        local.NegativeSignFound.Flag = "N";
        local.PositiveSignFound.Flag = "N";
        local.NumericCharacterFound.Flag = "N";
        local.ForConversion.TotalReal = 0;
        local.Common.Count = 1;

        for(var limit = Length(TrimEnd(export.Group.Item.WorkArea.Text9)); local
          .Common.Count <= limit; ++local.Common.Count)
        {
          local.WorkArea.Text1 =
            Substring(export.Group.Item.WorkArea.Text9, local.Common.Count, 1);

          switch(AsChar(local.WorkArea.Text1))
          {
            case '-':
              if (AsChar(local.NegativeSignFound.Flag) == 'Y' || AsChar
                (local.PositiveSignFound.Flag) == 'Y' || AsChar
                (local.DecimalFound.Flag) == 'Y' || AsChar
                (local.NumericCharacterFound.Flag) == 'Y')
              {
                local.ValidNumber.Flag = "N";

                goto AfterCycle1;
              }
              else
              {
                local.NegativeSignFound.Flag = "Y";
              }

              break;
            case '.':
              if (AsChar(local.DecimalFound.Flag) == 'Y')
              {
                local.ValidNumber.Flag = "N";

                goto AfterCycle1;
              }
              else
              {
                local.DecimalFound.Count = 1;
                local.DecimalFound.Flag = "Y";
              }

              break;
            case '+':
              if (AsChar(local.NegativeSignFound.Flag) == 'Y' || AsChar
                (local.PositiveSignFound.Flag) == 'Y' || AsChar
                (local.DecimalFound.Flag) == 'Y' || AsChar
                (local.NumericCharacterFound.Flag) == 'Y')
              {
                local.ValidNumber.Flag = "N";

                goto AfterCycle1;
              }
              else
              {
                local.PositiveSignFound.Flag = "Y";
              }

              break;
            case ' ':
              if (AsChar(local.DecimalFound.Flag) == 'Y' || AsChar
                (local.NumericCharacterFound.Flag) == 'Y')
              {
                local.ValidNumber.Flag = "N";

                goto AfterCycle1;
              }

              break;
            default:
              local.NumericCharacterFound.Flag = "Y";

              if (AsChar(local.DecimalFound.Flag) == 'Y')
              {
                local.DecimalFound.Count =
                  (int)((long)local.DecimalFound.Count * 10);
                local.ForConversion.TotalReal += StringToNumber(
                  local.WorkArea.Text1) / (decimal)local.DecimalFound.Count;
              }
              else
              {
                local.ForConversion.TotalReal =
                  local.ForConversion.TotalReal * 10 + StringToNumber
                  (local.WorkArea.Text1);
              }

              break;
          }
        }

AfterCycle1:

        if (AsChar(local.ValidNumber.Flag) == 'N')
        {
          var field = GetField(export.Group.Item.WorkArea, "text9");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "SP0000_INVALID_NUMBER";
          }
        }

        // -- Validate that whole number values do not have decimal places.
        if (AsChar(local.DecimalFound.Flag) == 'Y' && export
          .Group.Item.NumberOfDecimals.Count == 0)
        {
          var field = GetField(export.Group.Item.WorkArea, "text9");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "SP0000_DECIMAL_PLACES_INVALID";
          }
        }

        if (AsChar(local.NegativeSignFound.Flag) == 'Y')
        {
          local.ForConversion.TotalReal = -local.ForConversion.TotalReal;
        }

        // -- Insure numeric value does not exceed the maximum display size.
        //    Cannot exceed 9 characters including negative sign, decimal point,
        // and all
        //    decimal places.
        switch(export.Group.Item.NumberOfDecimals.Count)
        {
          case 0:
            if (local.ForConversion.TotalReal < -99999999 || local
              .ForConversion.TotalReal > 999999999)
            {
              var field = GetField(export.Group.Item.WorkArea, "text9");

              field.Error = true;

              ExitState = "SP0000_NUMERIC_OUT_OF_RANGE";
            }

            break;
          case 1:
            if (local.ForConversion.TotalReal < -999999 || local
              .ForConversion.TotalReal > 9999999)
            {
              var field = GetField(export.Group.Item.WorkArea, "text9");

              field.Error = true;

              ExitState = "SP0000_NUMERIC_OUT_OF_RANGE";
            }

            break;
          case 2:
            if (local.ForConversion.TotalReal < -99999 || local
              .ForConversion.TotalReal > 999999)
            {
              var field = GetField(export.Group.Item.WorkArea, "text9");

              field.Error = true;

              ExitState = "SP0000_NUMERIC_OUT_OF_RANGE";
            }

            break;
          case 3:
            if (local.ForConversion.TotalReal < -9999 || local
              .ForConversion.TotalReal > 99999)
            {
              var field = GetField(export.Group.Item.WorkArea, "text9");

              field.Error = true;

              ExitState = "SP0000_NUMERIC_OUT_OF_RANGE";
            }

            break;
          default:
            break;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          continue;
        }

        // -- Set appropiate local group attribute to the converted numeric 
        // value.
        // -- Also, reconvert back to text for standardized display.
        if (!IsEmpty(export.CseOrganization.Code))
        {
          // -- Judicial District Goals
          switch(export.Group.Index + 1)
          {
            case 1:
              // -- IV-D PEP (FFYTD)
              local.JdDashboardPerformanceMetrics.PepPercent =
                local.ForConversion.TotalReal / 100;

              break;
            case 2:
              // -- % Cases under order
              local.JdDashboardPerformanceMetrics.CasesUnderOrderPercent =
                local.ForConversion.TotalReal / 100;

              break;
            case 3:
              // -- % of current support paid (in-month)
              local.JdDashboardPerformanceMetrics.CurrentSupportPaidMthPer =
                local.ForConversion.TotalReal / 100;

              break;
            case 4:
              // -- % of current support paid (FFYTD)
              local.JdDashboardPerformanceMetrics.CurrentSupportPaidFfytdPer =
                local.ForConversion.TotalReal / 100;

              break;
            case 5:
              // -- % of cases paying on arrears (FFYTD)
              local.JdDashboardPerformanceMetrics.CasesPayingArrearsPercent =
                local.ForConversion.TotalReal / 100;

              break;
            case 6:
              // -- Total Collections in-month actual
              local.JdDashboardPerformanceMetrics.CollectionsInMonthActual =
                local.ForConversion.TotalReal;

              break;
            case 7:
              // -- Total Collections in-month previous year
              local.JdDashboardPerformanceMetrics.CollectionsInMonthPriorYear =
                local.ForConversion.TotalReal;

              break;
            case 8:
              // -- Total Collections in-month % change
              local.JdDashboardPerformanceMetrics.CollectionsInMonthPercentChg =
                local.ForConversion.TotalReal / 100;

              break;
            case 9:
              // -- Total Collections FFYTD actual
              local.JdDashboardPerformanceMetrics.CollectionsFfytdActual =
                local.ForConversion.TotalReal;

              break;
            case 10:
              // -- Total Collections FFYTD previous year
              local.JdDashboardPerformanceMetrics.CollectionsFfytdPriorYear =
                local.ForConversion.TotalReal;

              break;
            case 11:
              // -- Total Collections FFYTD % change
              local.JdDashboardPerformanceMetrics.
                CollectionsFfytdPercentChange =
                  local.ForConversion.TotalReal / 100;

              break;
            case 12:
              // -- Arrears Distributed In-month
              local.JdDashboardPerformanceMetrics.
                ArrearsDistributedMonthActual = local.ForConversion.TotalReal;

              break;
            case 13:
              // -- Arrears Distributed FFYTD
              local.JdDashboardPerformanceMetrics.
                ArrearsDistributedFfytdActual = local.ForConversion.TotalReal;

              break;
            case 14:
              // -- Arrears Due
              local.JdDashboardPerformanceMetrics.ArrearsDueActual =
                local.ForConversion.TotalReal;

              break;
            case 15:
              // -- Collections per cases under order
              local.JdDashboardPerformanceMetrics.CollectionsPerObligCaseAvg =
                local.ForConversion.TotalReal;

              break;
            case 16:
              // -- Income withholding per cases under order
              local.JdDashboardPerformanceMetrics.IwoPerObligCaseAverage =
                local.ForConversion.TotalReal;

              break;
            case 17:
              // -- Cases per FTE
              local.JdDashboardPerformanceMetrics.CasesPerFteAverage =
                local.ForConversion.TotalReal;

              break;
            case 18:
              // -- Collections per FTE
              local.JdDashboardPerformanceMetrics.CollectionsPerFteAverage =
                local.ForConversion.TotalReal;

              break;
            case 19:
              // -- Priority 3 - Judicial District Outputs - Starts Here...
              // -- New Cases Opened with Orders
              local.JdDashboardOutputMetrics.CasesOpenedWithOrder =
                (int?)local.ForConversion.TotalReal;

              break;
            case 20:
              // -- New Cases Opened without Orders
              local.JdDashboardOutputMetrics.CasesOpenedWithoutOrders =
                (int?)local.ForConversion.TotalReal;

              break;
            case 21:
              // -- Cases Closed with Orders
              local.JdDashboardOutputMetrics.CasesClosedWithOrders =
                (int?)local.ForConversion.TotalReal;

              break;
            case 22:
              // -- Cases Closed without Orders
              local.JdDashboardOutputMetrics.CasesClosedWithoutOrders =
                (int?)local.ForConversion.TotalReal;

              break;
            case 23:
              // -- New Orders Established
              local.JdDashboardOutputMetrics.NewOrdersEstablished =
                (int?)local.ForConversion.TotalReal;

              break;
            case 24:
              // -- Paternities Established
              local.JdDashboardOutputMetrics.PaternitiesEstablished =
                (int?)local.ForConversion.TotalReal;

              break;
            case 25:
              // -- Modification Reviews
              local.JdDashboardOutputMetrics.Modifications =
                (int?)local.ForConversion.TotalReal;

              break;
            case 26:
              // -- Income Withholdings Issued
              local.JdDashboardOutputMetrics.IncomeWithholdingsIssued =
                (int?)local.ForConversion.TotalReal;

              break;
            case 27:
              // -- Contempt Motion Filings
              local.JdDashboardOutputMetrics.ContemptMotionFilings =
                (int?)local.ForConversion.TotalReal;

              break;
            case 28:
              // -- Contempt Order Filings
              local.JdDashboardOutputMetrics.ContemptOrderFilings =
                (int?)local.ForConversion.TotalReal;

              break;
            case 29:
              // -- Referrals to Legal for Establishment
              local.JdDashboardOutputMetrics.ReferralsToLegalForEst =
                (int?)local.ForConversion.TotalReal;

              break;
            case 30:
              // -- Referrals to Legal for Enforcement
              local.JdDashboardOutputMetrics.ReferralsToLegalForEnf =
                (int?)local.ForConversion.TotalReal;

              break;
            case 31:
              // -- Days from Referral to Return of Service
              local.JdDashboardOutputMetrics.DaysToReturnOfServiceAvg =
                local.ForConversion.TotalReal;

              break;
            case 32:
              // -- Days from Referral to Order Establishment
              local.JdDashboardOutputMetrics.DaysToOrderEstblshmntAvg =
                local.ForConversion.TotalReal;

              break;
            case 33:
              // -- Unprocessed Legal Referrals, 60-90 Days
              local.JdDashboardOutputMetrics.ReferralAging60To90Days =
                (int?)local.ForConversion.TotalReal;

              break;
            case 34:
              // -- Unprocessed Legal Referrals, 91-120 Days
              local.JdDashboardOutputMetrics.ReferralAging91To120Days =
                (int?)local.ForConversion.TotalReal;

              break;
            case 35:
              // -- Unprocessed Legal Referrals, 121-150 Days
              local.JdDashboardOutputMetrics.ReferralAging121To150Days =
                (int?)local.ForConversion.TotalReal;

              break;
            case 36:
              // -- Unprocessed Legal Referrals, 150+ Days
              local.JdDashboardOutputMetrics.ReferralAging151PlusDays =
                (int?)local.ForConversion.TotalReal;

              break;
            case 37:
              // -- Average Days from IWO to IWO Payment
              local.JdDashboardOutputMetrics.DaysToIwoPaymentAverage =
                local.ForConversion.TotalReal;

              break;
            default:
              break;
          }
        }
        else
        {
          // -- Worker/Attorney Goals
          switch(export.Group.Index + 1)
          {
            case 1:
              // -- Cases Opened
              local.Worker.CasesOpened = (int?)local.ForConversion.TotalReal;

              break;
            case 2:
              // -- Cases Closed
              local.Worker.CaseClosures = (int?)local.ForConversion.TotalReal;

              break;
            case 3:
              // -- Case Reviews
              local.Worker.CaseReviews = (int?)local.ForConversion.TotalReal;

              break;
            case 4:
              // -- NCP Locates by Address
              local.Worker.NcpLocatesByAddress =
                (int?)local.ForConversion.TotalReal;

              break;
            case 5:
              // -- NCP Locates by Employer
              local.Worker.NcpLocatesByEmployer =
                (int?)local.ForConversion.TotalReal;

              break;
            case 6:
              // -- Referrals to Legal for EST
              local.Worker.ReferralsToLegalForEst =
                (int?)local.ForConversion.TotalReal;

              break;
            case 7:
              // -- Referrals to Legal for ENF
              local.Worker.ReferralsToLegalForEnf =
                (int?)local.ForConversion.TotalReal;

              break;
            case 8:
              // -- New Orders Established
              local.Worker.NewOrdersEstablished =
                (int?)local.ForConversion.TotalReal;

              break;
            case 9:
              // -- Paternities Established
              local.Worker.PaternitiesEstablished =
                (int?)local.ForConversion.TotalReal;

              break;
            case 10:
              // -- Total Collections
              local.Worker.TotalCollectionAmount =
                local.ForConversion.TotalReal;

              break;
            case 11:
              // -- Modification Reviews
              local.Worker.Modifications = (int?)local.ForConversion.TotalReal;

              break;
            case 12:
              // -- Income Withholdings Issued
              local.Worker.IncomeWithholdingsIssued =
                (int?)local.ForConversion.TotalReal;

              break;
            case 13:
              // -- Contempt Motion Filings
              local.Worker.ContemptMotionFilings =
                (int?)local.ForConversion.TotalReal;

              break;
            case 14:
              // -- Contempt Order Filings
              local.Worker.ContemptOrderFilings =
                (int?)local.ForConversion.TotalReal;

              break;
            case 15:
              // -- Days from Referral to Order Establishment
              local.Worker.DaysToOrderEstblshmntAvg =
                local.ForConversion.TotalReal;

              break;
            case 16:
              // -- Days from Referral to Return of Service
              local.Worker.DaysToReturnOfServiceAvg =
                local.ForConversion.TotalReal;

              break;
            case 17:
              // -- Unprocessed Legal Referrals 60-90 Days
              local.Worker.ReferralAging60To90Days =
                (int?)local.ForConversion.TotalReal;

              break;
            case 18:
              // -- Unprocessed Legal Referrals 91-120 Days
              local.Worker.ReferralAging91To120Days =
                (int?)local.ForConversion.TotalReal;

              break;
            case 19:
              // -- Unprocessed Legal Referrals 120-150 Days
              local.Worker.ReferralAging121To150Days =
                (int?)local.ForConversion.TotalReal;

              break;
            case 20:
              // -- Unprocessed Legal Referrals 150+ Days
              local.Worker.ReferralAging151PlusDays =
                (int?)local.ForConversion.TotalReal;

              break;
            case 21:
              // -- Average Days from IWO to IWO Payment
              local.Worker.DaysToIwoPaymentAverage =
                local.ForConversion.TotalReal;

              break;
            default:
              break;
          }
        }
      }

      export.Group.CheckIndex();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        // Display logic is located at bottom of PrAD.
        break;
      case "ADD":
        // ---------------------------------------------
        // Data has passed validation. Create.
        // ---------------------------------------------
        UseSpCreateDbGoal();

        if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
          ("ACO_NI0000_ADD_SUCCESSFUL"))
        {
          ExitState = "ACO_NI0000_ADD_SUCCESSFUL";
        }
        else
        {
          return;
        }

        if (!IsEmpty(export.CseOrganization.Code))
        {
          export.HiddenCseOrganization.Code = export.CseOrganization.Code;
        }
        else if (!IsEmpty(export.ServiceProvider.UserId))
        {
          export.HiddenServiceProvider.UserId = export.ServiceProvider.UserId;
        }

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          export.Group.Update.Hidden.Text9 = export.Group.Item.WorkArea.Text9;
        }

        export.Group.CheckIndex();

        break;
      case "UPDATE":
        // ---------------------------------------------
        // Data has passed validation. Update.
        // ---------------------------------------------
        UseSpUpdateDbGoal();

        if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
          ("ACO_NI0000_UPDATE_SUCCESSFUL"))
        {
          ExitState = "ACO_NI0000_UPDATE_SUCCESSFUL";
        }
        else
        {
          return;
        }

        if (!IsEmpty(export.CseOrganization.Code))
        {
          export.HiddenCseOrganization.Code = export.CseOrganization.Code;
        }
        else if (!IsEmpty(export.ServiceProvider.UserId))
        {
          export.HiddenServiceProvider.UserId = export.ServiceProvider.UserId;
        }

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          export.Group.Update.Hidden.Text9 = export.Group.Item.WorkArea.Text9;
        }

        export.Group.CheckIndex();

        break;
      case "DELETE":
        // ---------------------------------------------
        // Delete.
        // ---------------------------------------------
        UseSpDeleteDbGoal();

        if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
          ("ACO_NI0000_DELETE_SUCCESSFUL"))
        {
          ExitState = "ACO_NI0000_DELETE_SUCCESSFUL";

          if (!IsEmpty(export.CseOrganization.Code))
          {
            export.HiddenCseOrganization.Code = local.NullCseOrganization.Code;
          }
          else if (!IsEmpty(export.ServiceProvider.UserId))
          {
            export.HiddenServiceProvider.UserId =
              local.NullServiceProvider.UserId;
          }

          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            if (!export.Group.CheckSize())
            {
              break;
            }

            MoveWorkArea1(local.NullWorkArea, export.Group.Update.WorkArea);
            export.Group.Update.Hidden.Text9 = local.NullWorkArea.Text9;
          }

          export.Group.CheckIndex();
        }
        else
        {
          return;
        }

        break;
      case "LIST":
        switch(AsChar(export.Jd.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            ++local.Prompt.Count;

            break;
          default:
            var field = GetField(export.Jd, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            break;
        }

        switch(AsChar(export.Worker.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            ++local.Prompt.Count;

            break;
          default:
            var field = GetField(export.Worker, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            break;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        switch(local.Prompt.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            var field1 = GetField(export.Jd, "selectChar");

            field1.Error = true;

            var field2 = GetField(export.Worker, "selectChar");

            field2.Error = true;

            break;
          case 1:
            if (AsChar(export.Jd.SelectChar) == 'S')
            {
              export.ToCsor.Type1 = "J";
              ExitState = "ECO_LNK_TO_CSE_ORGANIZATION";
            }
            else if (AsChar(export.Worker.SelectChar) == 'S')
            {
              ExitState = "ECO_LNK_TO_LIST_SERVICE_PROVIDER";
            }

            break;
          default:
            ExitState = "MULTIPLE_SELECTION_NOT_ALLOWED";

            var field3 = GetField(export.Jd, "selectChar");

            field3.Error = true;

            var field4 = GetField(export.Worker, "selectChar");

            field4.Error = true;

            break;
        }

        return;
      default:
        break;
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      if (!IsEmpty(export.CseOrganization.Code))
      {
        export.HiddenCseOrganization.Code = export.CseOrganization.Code;
        export.ServiceProvider.Assign(local.NullServiceProvider);
        export.HiddenServiceProvider.UserId = local.NullServiceProvider.UserId;
        export.WorkerName.Text50 = "";
        export.Group.Count = 0;

        // -- Setup Export group views.
        // -- Judicial District Goals
        for(export.Group.Index = 0; export.Group.Index < 37; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          MoveWorkArea1(local.NullWorkArea, export.Group.Update.WorkArea);

          switch(export.Group.Index + 1)
          {
            case 1:
              // -- IV-D PEP (FFYTD)
              export.Group.Update.WorkArea.Text1 = "%";
              export.Group.Update.NumberOfDecimals.Count = 1;
              export.Group.Update.WorkArea.Text50 = "IV-D PEP (FFYTD)";

              break;
            case 2:
              // -- % Cases under order
              export.Group.Update.WorkArea.Text1 = "%";
              export.Group.Update.NumberOfDecimals.Count = 1;
              export.Group.Update.WorkArea.Text50 = "% Cases Under Order";

              break;
            case 3:
              // -- % of current support paid (in-month)
              export.Group.Update.WorkArea.Text1 = "%";
              export.Group.Update.NumberOfDecimals.Count = 1;
              export.Group.Update.WorkArea.Text50 =
                "% of Current Support Paid (In-Month)";

              break;
            case 4:
              // -- % of current support paid (FFYTD)
              export.Group.Update.WorkArea.Text1 = "%";
              export.Group.Update.NumberOfDecimals.Count = 1;
              export.Group.Update.WorkArea.Text50 =
                "% of Current Support Paid (FFYTD)";

              break;
            case 5:
              // -- % of cases paying on arrears (FFYTD)
              export.Group.Update.WorkArea.Text1 = "%";
              export.Group.Update.NumberOfDecimals.Count = 1;
              export.Group.Update.WorkArea.Text50 =
                "% of Cases Paying on Arrears (FFYTD)";

              break;
            case 6:
              // -- Total Collections in-month actual
              export.Group.Update.WorkArea.Text1 = "9";
              export.Group.Update.NumberOfDecimals.Count = 0;
              export.Group.Update.WorkArea.Text50 =
                "Total Collections in Month (Actual)";

              break;
            case 7:
              // -- Total Collections in-month previous year
              export.Group.Update.WorkArea.Text1 = "9";
              export.Group.Update.NumberOfDecimals.Count = 0;
              export.Group.Update.WorkArea.Text50 =
                "Total Collections in Month (Previous Year)";

              break;
            case 8:
              // -- Total Collections in-month % change
              export.Group.Update.WorkArea.Text1 = "%";
              export.Group.Update.NumberOfDecimals.Count = 1;
              export.Group.Update.WorkArea.Text50 =
                "Total Collections in Month (% Change)";

              break;
            case 9:
              // -- Total Collections FFYTD actual
              export.Group.Update.WorkArea.Text1 = "9";
              export.Group.Update.NumberOfDecimals.Count = 0;
              export.Group.Update.WorkArea.Text50 =
                "Total Collections FFYTD (Actual)";

              break;
            case 10:
              // -- Total Collections FFYTD previous year
              export.Group.Update.WorkArea.Text1 = "9";
              export.Group.Update.NumberOfDecimals.Count = 0;
              export.Group.Update.WorkArea.Text50 =
                "Total Collections FFYTD (Previous Year)";

              break;
            case 11:
              // -- Total Collections FFYTD % change
              export.Group.Update.WorkArea.Text1 = "%";
              export.Group.Update.NumberOfDecimals.Count = 1;
              export.Group.Update.WorkArea.Text50 =
                "Total Collections FFYTD (% Change)";

              break;
            case 12:
              // -- Arrears Distributed In-month
              export.Group.Update.WorkArea.Text1 = "9";
              export.Group.Update.NumberOfDecimals.Count = 0;
              export.Group.Update.WorkArea.Text50 =
                "Arrears Distributed In-Month";

              break;
            case 13:
              // -- Arrears Distributed FFYTD
              export.Group.Update.WorkArea.Text1 = "9";
              export.Group.Update.NumberOfDecimals.Count = 0;
              export.Group.Update.WorkArea.Text50 = "Arrears Distributed FFYTD";

              break;
            case 14:
              // -- Arrears Due
              export.Group.Update.WorkArea.Text1 = "9";
              export.Group.Update.NumberOfDecimals.Count = 0;
              export.Group.Update.WorkArea.Text50 = "Arrears Due";

              break;
            case 15:
              // -- Collections per cases under order
              export.Group.Update.WorkArea.Text1 = "9";
              export.Group.Update.NumberOfDecimals.Count = 2;
              export.Group.Update.WorkArea.Text50 =
                "Collections per Cases Under Order";

              break;
            case 16:
              // -- Income withholding per cases under order
              export.Group.Update.WorkArea.Text1 = "9";
              export.Group.Update.NumberOfDecimals.Count = 2;
              export.Group.Update.WorkArea.Text50 =
                "Income Withholdings per Cases Under Order";

              break;
            case 17:
              // -- Cases per FTE
              export.Group.Update.WorkArea.Text1 = "9";
              export.Group.Update.NumberOfDecimals.Count = 0;
              export.Group.Update.WorkArea.Text50 = "Cases per FTE";

              break;
            case 18:
              // -- Collections per FTE
              export.Group.Update.WorkArea.Text1 = "9";
              export.Group.Update.NumberOfDecimals.Count = 0;
              export.Group.Update.WorkArea.Text50 = "Collections per FTE";

              break;
            case 19:
              // -- Priority 3 - Judicial District Outputs - Starts Here...
              // -- New Cases Opened with Orders
              export.Group.Update.WorkArea.Text1 = "9";
              export.Group.Update.NumberOfDecimals.Count = 0;
              export.Group.Update.WorkArea.Text50 =
                "New Cases Opened w/ Orders";

              break;
            case 20:
              // -- New Cases Opened without Orders
              export.Group.Update.WorkArea.Text1 = "9";
              export.Group.Update.NumberOfDecimals.Count = 0;
              export.Group.Update.WorkArea.Text50 =
                "New Cases Opened w/o Orders";

              break;
            case 21:
              // -- Cases Closed with Orders
              export.Group.Update.WorkArea.Text1 = "9";
              export.Group.Update.NumberOfDecimals.Count = 0;
              export.Group.Update.WorkArea.Text50 = "Cases Closed w/ Orders";

              break;
            case 22:
              // -- Cases Closed without Orders
              export.Group.Update.WorkArea.Text1 = "9";
              export.Group.Update.NumberOfDecimals.Count = 0;
              export.Group.Update.WorkArea.Text50 = "Cases Closed w/o Orders";

              break;
            case 23:
              // -- New Orders Established
              export.Group.Update.WorkArea.Text1 = "9";
              export.Group.Update.NumberOfDecimals.Count = 0;
              export.Group.Update.WorkArea.Text50 = "New Orders Established";

              break;
            case 24:
              // -- Paternities Established
              export.Group.Update.WorkArea.Text1 = "9";
              export.Group.Update.NumberOfDecimals.Count = 0;
              export.Group.Update.WorkArea.Text50 = "Paternities Established";

              break;
            case 25:
              // -- Modification Reviews
              export.Group.Update.WorkArea.Text1 = "9";
              export.Group.Update.NumberOfDecimals.Count = 0;
              export.Group.Update.WorkArea.Text50 = "Modification Reviews";

              break;
            case 26:
              // -- Income Withholdings Issued
              export.Group.Update.WorkArea.Text1 = "9";
              export.Group.Update.NumberOfDecimals.Count = 0;
              export.Group.Update.WorkArea.Text50 =
                "Income Withholdings Issued";

              break;
            case 27:
              // -- Contempt Motion Filings
              export.Group.Update.WorkArea.Text1 = "9";
              export.Group.Update.NumberOfDecimals.Count = 0;
              export.Group.Update.WorkArea.Text50 = "Contempt Motion Filings";

              break;
            case 28:
              // -- Contempt Order Filings
              export.Group.Update.WorkArea.Text1 = "9";
              export.Group.Update.NumberOfDecimals.Count = 0;
              export.Group.Update.WorkArea.Text50 = "Contempt Order Filings";

              break;
            case 29:
              // -- Referrals to Legal for Establishment
              export.Group.Update.WorkArea.Text1 = "9";
              export.Group.Update.NumberOfDecimals.Count = 0;
              export.Group.Update.WorkArea.Text50 =
                "Referrals to Legal for EST";

              break;
            case 30:
              // -- Referrals to Legal for Enforcement
              export.Group.Update.WorkArea.Text1 = "9";
              export.Group.Update.NumberOfDecimals.Count = 0;
              export.Group.Update.WorkArea.Text50 =
                "Referrals to Legal for ENF";

              break;
            case 31:
              // -- Days from Referral to Return of Service
              export.Group.Update.WorkArea.Text1 = "9";
              export.Group.Update.NumberOfDecimals.Count = 0;
              export.Group.Update.WorkArea.Text50 =
                "Days from Referral to Return of Service";

              break;
            case 32:
              // -- Days from Referral to Order Establishment
              export.Group.Update.WorkArea.Text1 = "9";
              export.Group.Update.NumberOfDecimals.Count = 0;
              export.Group.Update.WorkArea.Text50 =
                "Days from Referral to Order Establishment";

              break;
            case 33:
              // -- Unprocessed Legal Referrals, 60-90 Days
              export.Group.Update.WorkArea.Text1 = "9";
              export.Group.Update.NumberOfDecimals.Count = 0;
              export.Group.Update.WorkArea.Text50 =
                "Unprocessed Legal Referrals, 60-90 Days";

              break;
            case 34:
              // -- Unprocessed Legal Referrals, 91-120 Days
              export.Group.Update.WorkArea.Text1 = "9";
              export.Group.Update.NumberOfDecimals.Count = 0;
              export.Group.Update.WorkArea.Text50 =
                "Unprocessed Legal Referrals, 91-120 Days";

              break;
            case 35:
              // -- Unprocessed Legal Referrals, 121-150 Days
              export.Group.Update.WorkArea.Text1 = "9";
              export.Group.Update.NumberOfDecimals.Count = 0;
              export.Group.Update.WorkArea.Text50 =
                "Unprocessed Legal Referrals, 121-150 Days";

              break;
            case 36:
              // -- Unprocessed Legal Referrals, 150+ Days
              export.Group.Update.WorkArea.Text1 = "9";
              export.Group.Update.NumberOfDecimals.Count = 0;
              export.Group.Update.WorkArea.Text50 =
                "Unprocessed Legal Referrals, 150+ Days";

              break;
            case 37:
              // -- Average Days from IWO to IWO Payment
              export.Group.Update.WorkArea.Text1 = "9";
              export.Group.Update.NumberOfDecimals.Count = 0;
              export.Group.Update.WorkArea.Text50 =
                "Average Days from IWO to IWO Payment";

              break;
            default:
              goto AfterCycle2;
          }

          if (Equal(global.Command, "DISPLAY") || Equal
            (global.Command, "CLEAR"))
          {
          }
          else
          {
            import.Group.Index = export.Group.Index;
            import.Group.CheckSize();

            export.Group.Update.WorkArea.Text9 =
              import.Group.Item.WorkArea.Text9;
            export.Group.Update.Hidden.Text9 = import.Group.Item.Hidden.Text9;
          }
        }

AfterCycle2:

        export.Group.CheckIndex();

        if (ReadDashboardPerformanceMetrics())
        {
          local.JdDashboardPerformanceMetrics.Assign(
            entities.DashboardPerformanceMetrics);
        }
        else
        {
          // --  Set a message indicating nothing found...
          ExitState = "DASHBOARD_GOAL_NF";

          return;
        }

        if (ReadDashboardOutputMetrics1())
        {
          MoveDashboardOutputMetrics2(entities.DashboardOutputMetrics,
            local.JdDashboardOutputMetrics);
        }
        else
        {
          // --  Set a message indicating nothing found...
          ExitState = "DASHBOARD_GOAL_NF";

          return;
        }
      }
      else if (!IsEmpty(export.ServiceProvider.UserId))
      {
        export.HiddenServiceProvider.UserId = export.ServiceProvider.UserId;
        MoveCseOrganization(local.NullCseOrganization, export.CseOrganization);
        export.HiddenCseOrganization.Code = local.NullCseOrganization.Code;
        export.WorkerName.Text50 = "";
        export.Group.Count = 0;

        // -- Setup Export group views.
        // -- Worker/Attorney Goals
        for(export.Group.Index = 0; export.Group.Index < 21; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          MoveWorkArea1(local.NullWorkArea, export.Group.Update.WorkArea);

          switch(export.Group.Index + 1)
          {
            case 1:
              // -- Cases Opened
              export.Group.Update.WorkArea.Text1 = "9";
              export.Group.Update.NumberOfDecimals.Count = 0;
              export.Group.Update.WorkArea.Text50 = "Cases Opened";

              break;
            case 2:
              // -- Cases Closed
              export.Group.Update.WorkArea.Text1 = "9";
              export.Group.Update.NumberOfDecimals.Count = 0;
              export.Group.Update.WorkArea.Text50 = "Cases Closed";

              break;
            case 3:
              // -- Case Reviews
              export.Group.Update.WorkArea.Text1 = "9";
              export.Group.Update.NumberOfDecimals.Count = 0;
              export.Group.Update.WorkArea.Text50 = "Case Reviews";

              break;
            case 4:
              // -- NCP Locates by Address
              export.Group.Update.WorkArea.Text1 = "9";
              export.Group.Update.NumberOfDecimals.Count = 0;
              export.Group.Update.WorkArea.Text50 = "NCP Locates by Address";

              break;
            case 5:
              // -- NCP Locates by Employer
              export.Group.Update.WorkArea.Text1 = "9";
              export.Group.Update.NumberOfDecimals.Count = 0;
              export.Group.Update.WorkArea.Text50 = "NCP Locates by Employer";

              break;
            case 6:
              // -- Referrals to Legal for EST
              export.Group.Update.WorkArea.Text1 = "9";
              export.Group.Update.NumberOfDecimals.Count = 0;
              export.Group.Update.WorkArea.Text50 =
                "Referrals to Legal for EST";

              break;
            case 7:
              // -- Referrals to Legal for ENF
              export.Group.Update.WorkArea.Text1 = "9";
              export.Group.Update.NumberOfDecimals.Count = 0;
              export.Group.Update.WorkArea.Text50 =
                "Referrals to Legal for ENF";

              break;
            case 8:
              // -- New Orders Established
              export.Group.Update.WorkArea.Text1 = "9";
              export.Group.Update.NumberOfDecimals.Count = 0;
              export.Group.Update.WorkArea.Text50 = "New Orders Established";

              break;
            case 9:
              // -- Paternities Established
              export.Group.Update.WorkArea.Text1 = "9";
              export.Group.Update.NumberOfDecimals.Count = 0;
              export.Group.Update.WorkArea.Text50 = "Paternities Established";

              break;
            case 10:
              // -- Total Collections
              export.Group.Update.WorkArea.Text1 = "9";
              export.Group.Update.NumberOfDecimals.Count = 0;
              export.Group.Update.WorkArea.Text50 = "Total Collections";

              break;
            case 11:
              // -- Modification Reviews
              export.Group.Update.WorkArea.Text1 = "9";
              export.Group.Update.NumberOfDecimals.Count = 0;
              export.Group.Update.WorkArea.Text50 = "Modification Reviews";

              break;
            case 12:
              // -- Income Withholdings Issued
              export.Group.Update.WorkArea.Text1 = "9";
              export.Group.Update.NumberOfDecimals.Count = 0;
              export.Group.Update.WorkArea.Text50 =
                "Income Withholdings Issued";

              break;
            case 13:
              // -- Contempt Motion Filings
              export.Group.Update.WorkArea.Text1 = "9";
              export.Group.Update.NumberOfDecimals.Count = 0;
              export.Group.Update.WorkArea.Text50 = "Contempt Motion Filings";

              break;
            case 14:
              // -- Contempt Order Filings
              export.Group.Update.WorkArea.Text1 = "9";
              export.Group.Update.NumberOfDecimals.Count = 0;
              export.Group.Update.WorkArea.Text50 = "Contempt Order Filings";

              break;
            case 15:
              // -- Days from Referral to Order Establishment
              export.Group.Update.WorkArea.Text1 = "9";
              export.Group.Update.NumberOfDecimals.Count = 0;
              export.Group.Update.WorkArea.Text50 =
                "Days from Referral to Order Establishment";

              break;
            case 16:
              // -- Days from Referral to Return of Service
              export.Group.Update.WorkArea.Text1 = "9";
              export.Group.Update.NumberOfDecimals.Count = 0;
              export.Group.Update.WorkArea.Text50 =
                "Days from Referral to Return of Service";

              break;
            case 17:
              // -- Unprocessed Legal Referrals 60-90 Days
              export.Group.Update.WorkArea.Text1 = "9";
              export.Group.Update.NumberOfDecimals.Count = 0;
              export.Group.Update.WorkArea.Text50 =
                "Unprocessed Legal Referrals 60-90 Days";

              break;
            case 18:
              // -- Unprocessed Legal Referrals 91-120 Days
              export.Group.Update.WorkArea.Text1 = "9";
              export.Group.Update.NumberOfDecimals.Count = 0;
              export.Group.Update.WorkArea.Text50 =
                "Unprocessed Legal Referrals 91-120 Days";

              break;
            case 19:
              // -- Unprocessed Legal Referrals 121-150 Days
              export.Group.Update.WorkArea.Text1 = "9";
              export.Group.Update.NumberOfDecimals.Count = 0;
              export.Group.Update.WorkArea.Text50 =
                "Unprocessed Legal Referrals 121-150 Days";

              break;
            case 20:
              // -- Unprocessed Legal Referrals 151+ Days
              export.Group.Update.WorkArea.Text1 = "9";
              export.Group.Update.NumberOfDecimals.Count = 0;
              export.Group.Update.WorkArea.Text50 =
                "Unprocessed Legal Referrals 151+ Days";

              break;
            case 21:
              // -- Average Days from IWO to IWO Payment
              export.Group.Update.WorkArea.Text1 = "9";
              export.Group.Update.NumberOfDecimals.Count = 0;
              export.Group.Update.WorkArea.Text50 =
                "Average Days from IWO to IWO Payment";

              break;
            default:
              goto AfterCycle3;
          }

          if (Equal(global.Command, "DISPLAY") || Equal
            (global.Command, "CLEAR"))
          {
          }
          else
          {
            import.Group.Index = export.Group.Index;
            import.Group.CheckSize();

            export.Group.Update.WorkArea.Text9 =
              import.Group.Item.WorkArea.Text9;
            export.Group.Update.Hidden.Text9 = import.Group.Item.Hidden.Text9;
          }
        }

AfterCycle3:

        export.Group.CheckIndex();

        if (ReadServiceProvider())
        {
          export.ServiceProvider.Assign(entities.ServiceProvider);
          export.WorkerName.Text50 =
            TrimEnd(export.ServiceProvider.LastName) + "," + TrimEnd
            (export.ServiceProvider.FirstName) + " " + export
            .ServiceProvider.MiddleInitial;
        }
        else
        {
          var field = GetField(export.ServiceProvider, "userId");

          field.Error = true;

          ExitState = "SERVICE_PROVIDER_NF";

          return;
        }

        if (ReadDashboardOutputMetrics2())
        {
          MoveDashboardOutputMetrics3(entities.DashboardOutputMetrics,
            local.Worker);
        }
        else
        {
          // --  Set a message indicating nothing found...
          ExitState = "DASHBOARD_GOAL_NF";

          return;
        }
      }

      MoveWorkArea2(export.YearMonth, export.YearMonthHidden);

      if (export.Group.IsEmpty)
      {
        ExitState = "ACO_NI0000_GROUP_VIEW_IS_EMPTY";
      }
      else
      {
        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
      }
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE") || Equal
      (global.Command, "DISPLAY"))
    {
      // -- Convert values for display...
      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        if (!export.Group.CheckSize())
        {
          break;
        }

        if (!IsEmpty(export.CseOrganization.Code))
        {
          // -- Judicial District Goals
          switch(export.Group.Index + 1)
          {
            case 1:
              // -- IV-D PEP (FFYTD)
              local.Temp.TotalInteger =
                (long)(local.JdDashboardPerformanceMetrics.PepPercent.
                  GetValueOrDefault() * 1000);

              break;
            case 2:
              // -- % Cases under order
              local.Temp.TotalInteger =
                (long)(local.JdDashboardPerformanceMetrics.
                  CasesUnderOrderPercent.GetValueOrDefault() * 1000);

              break;
            case 3:
              // -- % of current support paid (in-month)
              local.Temp.TotalInteger =
                (long)(local.JdDashboardPerformanceMetrics.
                  CurrentSupportPaidMthPer.GetValueOrDefault() * 1000);

              break;
            case 4:
              // -- % of current support paid (FFYTD)
              local.Temp.TotalInteger =
                (long)(local.JdDashboardPerformanceMetrics.
                  CurrentSupportPaidFfytdPer.GetValueOrDefault() * 1000);

              break;
            case 5:
              // -- % of cases paying on arrears (FFYTD)
              local.Temp.TotalInteger =
                (long)(local.JdDashboardPerformanceMetrics.
                  CasesPayingArrearsPercent.GetValueOrDefault() * 1000);

              break;
            case 6:
              // -- Total Collections in-month actual
              local.Temp.TotalInteger =
                (long)local.JdDashboardPerformanceMetrics.
                  CollectionsInMonthActual.GetValueOrDefault();

              break;
            case 7:
              // -- Total Collections in-month previous year
              local.Temp.TotalInteger =
                (long)local.JdDashboardPerformanceMetrics.
                  CollectionsInMonthPriorYear.GetValueOrDefault();

              break;
            case 8:
              // -- Total Collections in-month % change
              local.Temp.TotalInteger =
                (long)(local.JdDashboardPerformanceMetrics.
                  CollectionsInMonthPercentChg.GetValueOrDefault() * 1000);

              break;
            case 9:
              // -- Total Collections FFYTD actual
              local.Temp.TotalInteger =
                (long)local.JdDashboardPerformanceMetrics.
                  CollectionsFfytdActual.GetValueOrDefault();

              break;
            case 10:
              // -- Total Collections FFYTD previous year
              local.Temp.TotalInteger =
                (long)local.JdDashboardPerformanceMetrics.
                  CollectionsFfytdPriorYear.GetValueOrDefault();

              break;
            case 11:
              // -- Total Collections FFYTD % change
              local.Temp.TotalInteger =
                (long)(local.JdDashboardPerformanceMetrics.
                  CollectionsFfytdPercentChange.GetValueOrDefault() * 1000);

              break;
            case 12:
              // -- Arrears Distributed In-month
              local.Temp.TotalInteger =
                (long)local.JdDashboardPerformanceMetrics.
                  ArrearsDistributedMonthActual.GetValueOrDefault();

              break;
            case 13:
              // -- Arrears Distributed FFYTD
              local.Temp.TotalInteger =
                (long)local.JdDashboardPerformanceMetrics.
                  ArrearsDistributedFfytdActual.GetValueOrDefault();

              break;
            case 14:
              // -- Arrears Due
              local.Temp.TotalInteger =
                (long)local.JdDashboardPerformanceMetrics.ArrearsDueActual.
                  GetValueOrDefault();

              break;
            case 15:
              // -- Collections per cases under order
              local.Temp.TotalInteger =
                (long)(local.JdDashboardPerformanceMetrics.
                  CollectionsPerObligCaseAvg.GetValueOrDefault() * 100);

              break;
            case 16:
              // -- Income withholding per cases under order
              local.Temp.TotalInteger =
                (long)(local.JdDashboardPerformanceMetrics.
                  IwoPerObligCaseAverage.GetValueOrDefault() * 100);

              break;
            case 17:
              // -- Cases per FTE
              local.Temp.TotalInteger =
                (long)local.JdDashboardPerformanceMetrics.CasesPerFteAverage.
                  GetValueOrDefault();

              break;
            case 18:
              // -- Collections per FTE
              local.Temp.TotalInteger =
                (long)local.JdDashboardPerformanceMetrics.
                  CollectionsPerFteAverage.GetValueOrDefault();

              break;
            case 19:
              // -- Priority 3 - Judicial District Outputs - Starts Here...
              // -- New Cases Opened with Orders
              local.Temp.TotalInteger =
                local.JdDashboardOutputMetrics.CasesOpenedWithOrder.
                  GetValueOrDefault();

              break;
            case 20:
              // -- New Cases Opened without Orders
              local.Temp.TotalInteger =
                local.JdDashboardOutputMetrics.CasesOpenedWithoutOrders.
                  GetValueOrDefault();

              break;
            case 21:
              // -- Cases Closed with Orders
              local.Temp.TotalInteger =
                local.JdDashboardOutputMetrics.CasesClosedWithOrders.
                  GetValueOrDefault();

              break;
            case 22:
              // -- Cases Closed without Orders
              local.Temp.TotalInteger =
                local.JdDashboardOutputMetrics.CasesClosedWithoutOrders.
                  GetValueOrDefault();

              break;
            case 23:
              // -- New Orders Established
              local.Temp.TotalInteger =
                local.JdDashboardOutputMetrics.NewOrdersEstablished.
                  GetValueOrDefault();

              break;
            case 24:
              // -- Paternities Established
              local.Temp.TotalInteger =
                local.JdDashboardOutputMetrics.PaternitiesEstablished.
                  GetValueOrDefault();

              break;
            case 25:
              // -- Modification Reviews
              local.Temp.TotalInteger =
                local.JdDashboardOutputMetrics.Modifications.
                  GetValueOrDefault();

              break;
            case 26:
              // -- Income Withholdings Issued
              local.Temp.TotalInteger =
                local.JdDashboardOutputMetrics.IncomeWithholdingsIssued.
                  GetValueOrDefault();

              break;
            case 27:
              // -- Contempt Motion Filings
              local.Temp.TotalInteger =
                local.JdDashboardOutputMetrics.ContemptMotionFilings.
                  GetValueOrDefault();

              break;
            case 28:
              // -- Contempt Order Filings
              local.Temp.TotalInteger =
                local.JdDashboardOutputMetrics.ContemptOrderFilings.
                  GetValueOrDefault();

              break;
            case 29:
              // -- Referrals to Legal for Establishment
              local.Temp.TotalInteger =
                local.JdDashboardOutputMetrics.ReferralsToLegalForEst.
                  GetValueOrDefault();

              break;
            case 30:
              // -- Referrals to Legal for Enforcement
              local.Temp.TotalInteger =
                local.JdDashboardOutputMetrics.ReferralsToLegalForEnf.
                  GetValueOrDefault();

              break;
            case 31:
              // -- Days from Referral to Return of Service
              local.Temp.TotalInteger =
                (long)local.JdDashboardOutputMetrics.DaysToReturnOfServiceAvg.
                  GetValueOrDefault();

              break;
            case 32:
              // -- Days from Referral to Order Establishment
              local.Temp.TotalInteger =
                (long)local.JdDashboardOutputMetrics.DaysToOrderEstblshmntAvg.
                  GetValueOrDefault();

              break;
            case 33:
              // -- Unprocessed Legal Referrals, 60-90 Days
              local.Temp.TotalInteger =
                local.JdDashboardOutputMetrics.ReferralAging60To90Days.
                  GetValueOrDefault();

              break;
            case 34:
              // -- Unprocessed Legal Referrals, 91-120 Days
              local.Temp.TotalInteger =
                local.JdDashboardOutputMetrics.ReferralAging91To120Days.
                  GetValueOrDefault();

              break;
            case 35:
              // -- Unprocessed Legal Referrals, 121-150 Days
              local.Temp.TotalInteger =
                local.JdDashboardOutputMetrics.ReferralAging121To150Days.
                  GetValueOrDefault();

              break;
            case 36:
              // -- Unprocessed Legal Referrals, 150+ Days
              local.Temp.TotalInteger =
                local.JdDashboardOutputMetrics.ReferralAging151PlusDays.
                  GetValueOrDefault();

              break;
            case 37:
              // -- Average Days from IWO to IWO Payment
              local.Temp.TotalInteger =
                (long)local.JdDashboardOutputMetrics.DaysToIwoPaymentAverage.
                  GetValueOrDefault();

              break;
            default:
              return;
          }
        }
        else
        {
          // -- Worker/Attorney Goals
          switch(export.Group.Index + 1)
          {
            case 1:
              // -- Cases Opened
              local.Temp.TotalInteger =
                local.Worker.CasesOpened.GetValueOrDefault();

              break;
            case 2:
              // -- Cases Closed
              local.Temp.TotalInteger =
                local.Worker.CaseClosures.GetValueOrDefault();

              break;
            case 3:
              // -- Case Reviews
              local.Temp.TotalInteger =
                local.Worker.CaseReviews.GetValueOrDefault();

              break;
            case 4:
              // -- NCP Locates by Address
              local.Temp.TotalInteger =
                local.Worker.NcpLocatesByAddress.GetValueOrDefault();

              break;
            case 5:
              // -- NCP Locates by Employer
              local.Temp.TotalInteger =
                local.Worker.NcpLocatesByEmployer.GetValueOrDefault();

              break;
            case 6:
              // -- Referrals to Legal for EST
              local.Temp.TotalInteger =
                local.Worker.ReferralsToLegalForEst.GetValueOrDefault();

              break;
            case 7:
              // -- Referrals to Legal for ENF
              local.Temp.TotalInteger =
                local.Worker.ReferralsToLegalForEnf.GetValueOrDefault();

              break;
            case 8:
              // -- New Orders Established
              local.Temp.TotalInteger =
                local.Worker.NewOrdersEstablished.GetValueOrDefault();

              break;
            case 9:
              // -- Paternities Established
              local.Temp.TotalInteger =
                local.Worker.PaternitiesEstablished.GetValueOrDefault();

              break;
            case 10:
              // -- Total Collections
              local.Temp.TotalInteger =
                (long)local.Worker.TotalCollectionAmount.GetValueOrDefault();

              break;
            case 11:
              // -- Modification Reviews
              local.Temp.TotalInteger =
                local.Worker.Modifications.GetValueOrDefault();

              break;
            case 12:
              // -- Income Withholdings Issued
              local.Temp.TotalInteger =
                local.Worker.IncomeWithholdingsIssued.GetValueOrDefault();

              break;
            case 13:
              // -- Contempt Motion Filings
              local.Temp.TotalInteger =
                local.Worker.ContemptMotionFilings.GetValueOrDefault();

              break;
            case 14:
              // -- Contempt Order Filings
              local.Temp.TotalInteger =
                local.Worker.ContemptOrderFilings.GetValueOrDefault();

              break;
            case 15:
              // -- Days from Referral to Order Establishment
              local.Temp.TotalInteger =
                (long)local.Worker.DaysToOrderEstblshmntAvg.GetValueOrDefault();
                

              break;
            case 16:
              // -- Days from Referral to Return of Service
              local.Temp.TotalInteger =
                (long)local.Worker.DaysToReturnOfServiceAvg.GetValueOrDefault();
                

              break;
            case 17:
              // -- Unprocessed Legal Referrals 60-90 Days
              local.Temp.TotalInteger =
                local.Worker.ReferralAging60To90Days.GetValueOrDefault();

              break;
            case 18:
              // -- Unprocessed Legal Referrals 91-120 Days
              local.Temp.TotalInteger =
                local.Worker.ReferralAging91To120Days.GetValueOrDefault();

              break;
            case 19:
              // -- Unprocessed Legal Referrals 120-150 Days
              local.Temp.TotalInteger =
                local.Worker.ReferralAging121To150Days.GetValueOrDefault();

              break;
            case 20:
              // -- Unprocessed Legal Referrals 150+ Days
              local.Temp.TotalInteger =
                local.Worker.ReferralAging151PlusDays.GetValueOrDefault();

              break;
            case 21:
              // -- Average Days from IWO to IWO Payment
              local.Temp.TotalInteger =
                (long)local.Worker.DaysToIwoPaymentAverage.GetValueOrDefault();

              break;
            default:
              return;
          }
        }

        if (local.Temp.TotalInteger == 0)
        {
          export.Group.Update.WorkArea.Text9 = "";

          continue;
        }

        // -- Convert number to text for display
        if (export.Group.Item.NumberOfDecimals.Count == 0)
        {
          export.Group.Update.WorkArea.Text9 =
            NumberToString(local.Temp.TotalInteger, 7, 9);
        }
        else
        {
          export.Group.Update.WorkArea.Text9 =
            NumberToString(local.Temp.TotalInteger, 8, 8 -
            export.Group.Item.NumberOfDecimals.Count);
          export.Group.Update.WorkArea.Text9 =
            Substring(export.Group.Item.WorkArea.Text9,
            WorkArea.Text9_MaxLength, 1, 8 -
            export.Group.Item.NumberOfDecimals.Count) + "." + NumberToString
            (local.Temp.TotalInteger, 16 -
            export.Group.Item.NumberOfDecimals.Count,
            export.Group.Item.NumberOfDecimals.Count);
        }

        // -- If necessary add negative sign.
        if (local.Temp.TotalInteger < 0)
        {
          local.Temp.TotalInteger =
            Verify(export.Group.Item.WorkArea.Text9, "0");
          export.Group.Update.WorkArea.Text9 =
            Substring(export.Group.Item.WorkArea.Text9,
            WorkArea.Text9_MaxLength, 1, (int)(local.Temp.TotalInteger - 2)) + "-"
            + Substring
            (export.Group.Item.WorkArea.Text9, WorkArea.Text9_MaxLength,
            (int)local.Temp.TotalInteger, (int)(16 - local.Temp.TotalInteger));
        }

        // -- Remove unneccessary leading zeros.
        local.Temp.TotalInteger = Verify(export.Group.Item.WorkArea.Text9, "0");
        export.Group.Update.WorkArea.Text9 =
          Substring(local.Blank.Text9, WorkArea.Text9_MaxLength, 1,
          (int)(local.Temp.TotalInteger - 1)) + Substring
          (export.Group.Item.WorkArea.Text9, WorkArea.Text9_MaxLength,
          (int)local.Temp.TotalInteger, (int)(15 - local.Temp.TotalInteger));

        // -- Move export group to export group hidden
        export.Group.Update.Hidden.Text9 = export.Group.Item.WorkArea.Text9;
      }

      export.Group.CheckIndex();
    }
  }

  private static void MoveCseOrganization(CseOrganization source,
    CseOrganization target)
  {
    target.Code = source.Code;
    target.Name = source.Name;
  }

  private static void MoveDashboardOutputMetrics1(DashboardOutputMetrics source,
    DashboardOutputMetrics target)
  {
    target.ReportMonth = source.ReportMonth;
    target.ReportLevel = source.ReportLevel;
    target.ReportLevelId = source.ReportLevelId;
    target.Type1 = source.Type1;
  }

  private static void MoveDashboardOutputMetrics2(DashboardOutputMetrics source,
    DashboardOutputMetrics target)
  {
    target.ReportMonth = source.ReportMonth;
    target.ReportLevel = source.ReportLevel;
    target.ReportLevelId = source.ReportLevelId;
    target.Type1 = source.Type1;
    target.AsOfDate = source.AsOfDate;
    target.NewOrdersEstablished = source.NewOrdersEstablished;
    target.PaternitiesEstablished = source.PaternitiesEstablished;
    target.CasesOpenedWithOrder = source.CasesOpenedWithOrder;
    target.CasesOpenedWithoutOrders = source.CasesOpenedWithoutOrders;
    target.CasesClosedWithOrders = source.CasesClosedWithOrders;
    target.CasesClosedWithoutOrders = source.CasesClosedWithoutOrders;
    target.Modifications = source.Modifications;
    target.IncomeWithholdingsIssued = source.IncomeWithholdingsIssued;
    target.ContemptMotionFilings = source.ContemptMotionFilings;
    target.ContemptOrderFilings = source.ContemptOrderFilings;
    target.DaysToOrderEstblshmntAvg = source.DaysToOrderEstblshmntAvg;
    target.DaysToReturnOfServiceAvg = source.DaysToReturnOfServiceAvg;
    target.ReferralAging60To90Days = source.ReferralAging60To90Days;
    target.ReferralAging91To120Days = source.ReferralAging91To120Days;
    target.ReferralAging121To150Days = source.ReferralAging121To150Days;
    target.ReferralAging151PlusDays = source.ReferralAging151PlusDays;
    target.DaysToIwoPaymentAverage = source.DaysToIwoPaymentAverage;
    target.ReferralsToLegalForEst = source.ReferralsToLegalForEst;
    target.ReferralsToLegalForEnf = source.ReferralsToLegalForEnf;
  }

  private static void MoveDashboardOutputMetrics3(DashboardOutputMetrics source,
    DashboardOutputMetrics target)
  {
    target.ReportMonth = source.ReportMonth;
    target.ReportLevel = source.ReportLevel;
    target.ReportLevelId = source.ReportLevelId;
    target.Type1 = source.Type1;
    target.AsOfDate = source.AsOfDate;
    target.NewOrdersEstablished = source.NewOrdersEstablished;
    target.PaternitiesEstablished = source.PaternitiesEstablished;
    target.Modifications = source.Modifications;
    target.IncomeWithholdingsIssued = source.IncomeWithholdingsIssued;
    target.ContemptMotionFilings = source.ContemptMotionFilings;
    target.ContemptOrderFilings = source.ContemptOrderFilings;
    target.TotalCollectionAmount = source.TotalCollectionAmount;
    target.DaysToOrderEstblshmntAvg = source.DaysToOrderEstblshmntAvg;
    target.DaysToReturnOfServiceAvg = source.DaysToReturnOfServiceAvg;
    target.ReferralAging60To90Days = source.ReferralAging60To90Days;
    target.ReferralAging91To120Days = source.ReferralAging91To120Days;
    target.ReferralAging121To150Days = source.ReferralAging121To150Days;
    target.ReferralAging151PlusDays = source.ReferralAging151PlusDays;
    target.DaysToIwoPaymentAverage = source.DaysToIwoPaymentAverage;
    target.ReferralsToLegalForEst = source.ReferralsToLegalForEst;
    target.ReferralsToLegalForEnf = source.ReferralsToLegalForEnf;
    target.CasesOpened = source.CasesOpened;
    target.NcpLocatesByAddress = source.NcpLocatesByAddress;
    target.NcpLocatesByEmployer = source.NcpLocatesByEmployer;
    target.CaseClosures = source.CaseClosures;
    target.CaseReviews = source.CaseReviews;
  }

  private static void MoveDashboardPerformanceMetrics(
    DashboardPerformanceMetrics source, DashboardPerformanceMetrics target)
  {
    target.ReportMonth = source.ReportMonth;
    target.ReportLevel = source.ReportLevel;
    target.ReportLevelId = source.ReportLevelId;
    target.Type1 = source.Type1;
    target.CasesUnderOrderPercent = source.CasesUnderOrderPercent;
    target.PepPercent = source.PepPercent;
    target.CasesPayingArrearsPercent = source.CasesPayingArrearsPercent;
    target.CurrentSupportPaidMthPer = source.CurrentSupportPaidMthPer;
    target.CurrentSupportPaidFfytdPer = source.CurrentSupportPaidFfytdPer;
    target.CollectionsFfytdActual = source.CollectionsFfytdActual;
    target.CollectionsFfytdPriorYear = source.CollectionsFfytdPriorYear;
    target.CollectionsFfytdPercentChange = source.CollectionsFfytdPercentChange;
    target.CollectionsInMonthActual = source.CollectionsInMonthActual;
    target.CollectionsInMonthPriorYear = source.CollectionsInMonthPriorYear;
    target.CollectionsInMonthPercentChg = source.CollectionsInMonthPercentChg;
    target.ArrearsDistributedMonthActual = source.ArrearsDistributedMonthActual;
    target.ArrearsDistributedFfytdActual = source.ArrearsDistributedFfytdActual;
    target.ArrearsDueActual = source.ArrearsDueActual;
    target.CollectionsPerObligCaseAvg = source.CollectionsPerObligCaseAvg;
    target.IwoPerObligCaseAverage = source.IwoPerObligCaseAverage;
    target.CasesPerFteAverage = source.CasesPerFteAverage;
    target.CollectionsPerFteAverage = source.CollectionsPerFteAverage;
    target.CasesPayingPercent = source.CasesPayingPercent;
  }

  private static void MoveWorkArea1(WorkArea source, WorkArea target)
  {
    target.Text1 = source.Text1;
    target.Text9 = source.Text9;
  }

  private static void MoveWorkArea2(WorkArea source, WorkArea target)
  {
    target.Text2 = source.Text2;
    target.Text4 = source.Text4;
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    local.NextTranInfo.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(local.NextTranInfo);

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

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSpCreateDbGoal()
  {
    var useImport = new SpCreateDbGoal.Import();
    var useExport = new SpCreateDbGoal.Export();

    MoveDashboardPerformanceMetrics(local.JdDashboardPerformanceMetrics,
      useImport.JdDashboardPerformanceMetrics);
    useImport.Worker.Assign(local.Worker);
    useImport.JdDashboardOutputMetrics.Assign(local.JdDashboardOutputMetrics);

    Call(SpCreateDbGoal.Execute, useImport, useExport);
  }

  private void UseSpDeleteDbGoal()
  {
    var useImport = new SpDeleteDbGoal.Import();
    var useExport = new SpDeleteDbGoal.Export();

    MoveDashboardPerformanceMetrics(local.JdDashboardPerformanceMetrics,
      useImport.JdDashboardPerformanceMetrics);
    MoveDashboardOutputMetrics1(local.Worker, useImport.Worker);
    MoveDashboardOutputMetrics1(local.JdDashboardOutputMetrics,
      useImport.JdDashboardOutputMetrics);

    Call(SpDeleteDbGoal.Execute, useImport, useExport);
  }

  private void UseSpUpdateDbGoal()
  {
    var useImport = new SpUpdateDbGoal.Import();
    var useExport = new SpUpdateDbGoal.Export();

    MoveDashboardPerformanceMetrics(local.JdDashboardPerformanceMetrics,
      useImport.JdDashboardPerformanceMetrics);
    useImport.Worker.Assign(local.Worker);
    useImport.JdDashboardOutputMetrics.Assign(local.JdDashboardOutputMetrics);

    Call(SpUpdateDbGoal.Execute, useImport, useExport);
  }

  private bool ReadCseOrganization()
  {
    entities.CseOrganization.Populated = false;

    return Read("ReadCseOrganization",
      (db, command) =>
      {
        db.SetString(command, "organztnId", export.CseOrganization.Code);
      },
      (db, reader) =>
      {
        entities.CseOrganization.Code = db.GetString(reader, 0);
        entities.CseOrganization.Type1 = db.GetString(reader, 1);
        entities.CseOrganization.Name = db.GetString(reader, 2);
        entities.CseOrganization.Populated = true;
      });
  }

  private bool ReadDashboardOutputMetrics1()
  {
    entities.DashboardOutputMetrics.Populated = false;

    return Read("ReadDashboardOutputMetrics1",
      (db, command) =>
      {
        db.SetString(
          command, "reportLevel", local.JdDashboardOutputMetrics.ReportLevel);
        db.SetString(
          command, "reportLevelId",
          local.JdDashboardOutputMetrics.ReportLevelId);
        db.SetInt32(command, "reportMonth", local.StaffingMonth.YearMonth);
        db.SetString(command, "type", local.JdDashboardOutputMetrics.Type1);
      },
      (db, reader) =>
      {
        entities.DashboardOutputMetrics.ReportMonth = db.GetInt32(reader, 0);
        entities.DashboardOutputMetrics.ReportLevel = db.GetString(reader, 1);
        entities.DashboardOutputMetrics.ReportLevelId = db.GetString(reader, 2);
        entities.DashboardOutputMetrics.Type1 = db.GetString(reader, 3);
        entities.DashboardOutputMetrics.AsOfDate =
          db.GetNullableDate(reader, 4);
        entities.DashboardOutputMetrics.CasesWithEstReferral =
          db.GetNullableInt32(reader, 5);
        entities.DashboardOutputMetrics.CasesWithEnfReferral =
          db.GetNullableInt32(reader, 6);
        entities.DashboardOutputMetrics.FullTimeEquivalent =
          db.GetNullableDecimal(reader, 7);
        entities.DashboardOutputMetrics.NewOrdersEstablished =
          db.GetNullableInt32(reader, 8);
        entities.DashboardOutputMetrics.PaternitiesEstablished =
          db.GetNullableInt32(reader, 9);
        entities.DashboardOutputMetrics.CasesOpenedWithOrder =
          db.GetNullableInt32(reader, 10);
        entities.DashboardOutputMetrics.CasesOpenedWithoutOrders =
          db.GetNullableInt32(reader, 11);
        entities.DashboardOutputMetrics.CasesClosedWithOrders =
          db.GetNullableInt32(reader, 12);
        entities.DashboardOutputMetrics.CasesClosedWithoutOrders =
          db.GetNullableInt32(reader, 13);
        entities.DashboardOutputMetrics.Modifications =
          db.GetNullableInt32(reader, 14);
        entities.DashboardOutputMetrics.IncomeWithholdingsIssued =
          db.GetNullableInt32(reader, 15);
        entities.DashboardOutputMetrics.ContemptMotionFilings =
          db.GetNullableInt32(reader, 16);
        entities.DashboardOutputMetrics.ContemptOrderFilings =
          db.GetNullableInt32(reader, 17);
        entities.DashboardOutputMetrics.StypeCollectionAmount =
          db.GetNullableDecimal(reader, 18);
        entities.DashboardOutputMetrics.StypePercentOfTotal =
          db.GetNullableDecimal(reader, 19);
        entities.DashboardOutputMetrics.FtypeCollectionAmount =
          db.GetNullableDecimal(reader, 20);
        entities.DashboardOutputMetrics.FtypePercentOfTotal =
          db.GetNullableDecimal(reader, 21);
        entities.DashboardOutputMetrics.ItypeCollectionAmount =
          db.GetNullableDecimal(reader, 22);
        entities.DashboardOutputMetrics.ItypePercentOfTotal =
          db.GetNullableDecimal(reader, 23);
        entities.DashboardOutputMetrics.UtypeCollectionAmount =
          db.GetNullableDecimal(reader, 24);
        entities.DashboardOutputMetrics.UtypePercentOfTotal =
          db.GetNullableDecimal(reader, 25);
        entities.DashboardOutputMetrics.CtypeCollectionAmount =
          db.GetNullableDecimal(reader, 26);
        entities.DashboardOutputMetrics.CtypePercentOfTotal =
          db.GetNullableDecimal(reader, 27);
        entities.DashboardOutputMetrics.TotalCollectionAmount =
          db.GetNullableDecimal(reader, 28);
        entities.DashboardOutputMetrics.DaysToOrderEstblshmntNumer =
          db.GetNullableInt32(reader, 29);
        entities.DashboardOutputMetrics.DaysToOrderEstblshmntDenom =
          db.GetNullableInt32(reader, 30);
        entities.DashboardOutputMetrics.DaysToOrderEstblshmntAvg =
          db.GetNullableDecimal(reader, 31);
        entities.DashboardOutputMetrics.DaysToReturnOfSrvcNumer =
          db.GetNullableInt32(reader, 32);
        entities.DashboardOutputMetrics.DaysToReturnOfServiceDenom =
          db.GetNullableInt32(reader, 33);
        entities.DashboardOutputMetrics.DaysToReturnOfServiceAvg =
          db.GetNullableDecimal(reader, 34);
        entities.DashboardOutputMetrics.ReferralAging60To90Days =
          db.GetNullableInt32(reader, 35);
        entities.DashboardOutputMetrics.ReferralAging91To120Days =
          db.GetNullableInt32(reader, 36);
        entities.DashboardOutputMetrics.ReferralAging121To150Days =
          db.GetNullableInt32(reader, 37);
        entities.DashboardOutputMetrics.ReferralAging151PlusDays =
          db.GetNullableInt32(reader, 38);
        entities.DashboardOutputMetrics.DaysToIwoPaymentNumerator =
          db.GetNullableInt32(reader, 39);
        entities.DashboardOutputMetrics.DaysToIwoPaymentDenominator =
          db.GetNullableInt32(reader, 40);
        entities.DashboardOutputMetrics.DaysToIwoPaymentAverage =
          db.GetNullableDecimal(reader, 41);
        entities.DashboardOutputMetrics.ReferralsToLegalForEst =
          db.GetNullableInt32(reader, 42);
        entities.DashboardOutputMetrics.ReferralsToLegalForEnf =
          db.GetNullableInt32(reader, 43);
        entities.DashboardOutputMetrics.CaseloadCount =
          db.GetNullableInt32(reader, 44);
        entities.DashboardOutputMetrics.CasesOpened =
          db.GetNullableInt32(reader, 45);
        entities.DashboardOutputMetrics.NcpLocatesByAddress =
          db.GetNullableInt32(reader, 46);
        entities.DashboardOutputMetrics.NcpLocatesByEmployer =
          db.GetNullableInt32(reader, 47);
        entities.DashboardOutputMetrics.CaseClosures =
          db.GetNullableInt32(reader, 48);
        entities.DashboardOutputMetrics.CaseReviews =
          db.GetNullableInt32(reader, 49);
        entities.DashboardOutputMetrics.Populated = true;
      });
  }

  private bool ReadDashboardOutputMetrics2()
  {
    entities.DashboardOutputMetrics.Populated = false;

    return Read("ReadDashboardOutputMetrics2",
      (db, command) =>
      {
        db.SetString(command, "reportLevel", local.Worker.ReportLevel);
        db.SetString(command, "reportLevelId", local.Worker.ReportLevelId);
        db.SetInt32(command, "reportMonth", local.StaffingMonth.YearMonth);
        db.SetString(command, "type", local.Worker.Type1);
      },
      (db, reader) =>
      {
        entities.DashboardOutputMetrics.ReportMonth = db.GetInt32(reader, 0);
        entities.DashboardOutputMetrics.ReportLevel = db.GetString(reader, 1);
        entities.DashboardOutputMetrics.ReportLevelId = db.GetString(reader, 2);
        entities.DashboardOutputMetrics.Type1 = db.GetString(reader, 3);
        entities.DashboardOutputMetrics.AsOfDate =
          db.GetNullableDate(reader, 4);
        entities.DashboardOutputMetrics.CasesWithEstReferral =
          db.GetNullableInt32(reader, 5);
        entities.DashboardOutputMetrics.CasesWithEnfReferral =
          db.GetNullableInt32(reader, 6);
        entities.DashboardOutputMetrics.FullTimeEquivalent =
          db.GetNullableDecimal(reader, 7);
        entities.DashboardOutputMetrics.NewOrdersEstablished =
          db.GetNullableInt32(reader, 8);
        entities.DashboardOutputMetrics.PaternitiesEstablished =
          db.GetNullableInt32(reader, 9);
        entities.DashboardOutputMetrics.CasesOpenedWithOrder =
          db.GetNullableInt32(reader, 10);
        entities.DashboardOutputMetrics.CasesOpenedWithoutOrders =
          db.GetNullableInt32(reader, 11);
        entities.DashboardOutputMetrics.CasesClosedWithOrders =
          db.GetNullableInt32(reader, 12);
        entities.DashboardOutputMetrics.CasesClosedWithoutOrders =
          db.GetNullableInt32(reader, 13);
        entities.DashboardOutputMetrics.Modifications =
          db.GetNullableInt32(reader, 14);
        entities.DashboardOutputMetrics.IncomeWithholdingsIssued =
          db.GetNullableInt32(reader, 15);
        entities.DashboardOutputMetrics.ContemptMotionFilings =
          db.GetNullableInt32(reader, 16);
        entities.DashboardOutputMetrics.ContemptOrderFilings =
          db.GetNullableInt32(reader, 17);
        entities.DashboardOutputMetrics.StypeCollectionAmount =
          db.GetNullableDecimal(reader, 18);
        entities.DashboardOutputMetrics.StypePercentOfTotal =
          db.GetNullableDecimal(reader, 19);
        entities.DashboardOutputMetrics.FtypeCollectionAmount =
          db.GetNullableDecimal(reader, 20);
        entities.DashboardOutputMetrics.FtypePercentOfTotal =
          db.GetNullableDecimal(reader, 21);
        entities.DashboardOutputMetrics.ItypeCollectionAmount =
          db.GetNullableDecimal(reader, 22);
        entities.DashboardOutputMetrics.ItypePercentOfTotal =
          db.GetNullableDecimal(reader, 23);
        entities.DashboardOutputMetrics.UtypeCollectionAmount =
          db.GetNullableDecimal(reader, 24);
        entities.DashboardOutputMetrics.UtypePercentOfTotal =
          db.GetNullableDecimal(reader, 25);
        entities.DashboardOutputMetrics.CtypeCollectionAmount =
          db.GetNullableDecimal(reader, 26);
        entities.DashboardOutputMetrics.CtypePercentOfTotal =
          db.GetNullableDecimal(reader, 27);
        entities.DashboardOutputMetrics.TotalCollectionAmount =
          db.GetNullableDecimal(reader, 28);
        entities.DashboardOutputMetrics.DaysToOrderEstblshmntNumer =
          db.GetNullableInt32(reader, 29);
        entities.DashboardOutputMetrics.DaysToOrderEstblshmntDenom =
          db.GetNullableInt32(reader, 30);
        entities.DashboardOutputMetrics.DaysToOrderEstblshmntAvg =
          db.GetNullableDecimal(reader, 31);
        entities.DashboardOutputMetrics.DaysToReturnOfSrvcNumer =
          db.GetNullableInt32(reader, 32);
        entities.DashboardOutputMetrics.DaysToReturnOfServiceDenom =
          db.GetNullableInt32(reader, 33);
        entities.DashboardOutputMetrics.DaysToReturnOfServiceAvg =
          db.GetNullableDecimal(reader, 34);
        entities.DashboardOutputMetrics.ReferralAging60To90Days =
          db.GetNullableInt32(reader, 35);
        entities.DashboardOutputMetrics.ReferralAging91To120Days =
          db.GetNullableInt32(reader, 36);
        entities.DashboardOutputMetrics.ReferralAging121To150Days =
          db.GetNullableInt32(reader, 37);
        entities.DashboardOutputMetrics.ReferralAging151PlusDays =
          db.GetNullableInt32(reader, 38);
        entities.DashboardOutputMetrics.DaysToIwoPaymentNumerator =
          db.GetNullableInt32(reader, 39);
        entities.DashboardOutputMetrics.DaysToIwoPaymentDenominator =
          db.GetNullableInt32(reader, 40);
        entities.DashboardOutputMetrics.DaysToIwoPaymentAverage =
          db.GetNullableDecimal(reader, 41);
        entities.DashboardOutputMetrics.ReferralsToLegalForEst =
          db.GetNullableInt32(reader, 42);
        entities.DashboardOutputMetrics.ReferralsToLegalForEnf =
          db.GetNullableInt32(reader, 43);
        entities.DashboardOutputMetrics.CaseloadCount =
          db.GetNullableInt32(reader, 44);
        entities.DashboardOutputMetrics.CasesOpened =
          db.GetNullableInt32(reader, 45);
        entities.DashboardOutputMetrics.NcpLocatesByAddress =
          db.GetNullableInt32(reader, 46);
        entities.DashboardOutputMetrics.NcpLocatesByEmployer =
          db.GetNullableInt32(reader, 47);
        entities.DashboardOutputMetrics.CaseClosures =
          db.GetNullableInt32(reader, 48);
        entities.DashboardOutputMetrics.CaseReviews =
          db.GetNullableInt32(reader, 49);
        entities.DashboardOutputMetrics.Populated = true;
      });
  }

  private bool ReadDashboardPerformanceMetrics()
  {
    entities.DashboardPerformanceMetrics.Populated = false;

    return Read("ReadDashboardPerformanceMetrics",
      (db, command) =>
      {
        db.SetString(
          command, "reportLevel",
          local.JdDashboardPerformanceMetrics.ReportLevel);
        db.SetString(
          command, "reportLevelId",
          local.JdDashboardPerformanceMetrics.ReportLevelId);
        db.SetInt32(command, "reportMonth", local.StaffingMonth.YearMonth);
        db.
          SetString(command, "type", local.JdDashboardPerformanceMetrics.Type1);
          
      },
      (db, reader) =>
      {
        entities.DashboardPerformanceMetrics.ReportMonth =
          db.GetInt32(reader, 0);
        entities.DashboardPerformanceMetrics.ReportLevel =
          db.GetString(reader, 1);
        entities.DashboardPerformanceMetrics.ReportLevelId =
          db.GetString(reader, 2);
        entities.DashboardPerformanceMetrics.Type1 = db.GetString(reader, 3);
        entities.DashboardPerformanceMetrics.CasesUnderOrderPercent =
          db.GetNullableDecimal(reader, 4);
        entities.DashboardPerformanceMetrics.PepPercent =
          db.GetNullableDecimal(reader, 5);
        entities.DashboardPerformanceMetrics.CasesPayingArrearsPercent =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardPerformanceMetrics.CurrentSupportPaidMthPer =
          db.GetNullableDecimal(reader, 7);
        entities.DashboardPerformanceMetrics.CurrentSupportPaidFfytdPer =
          db.GetNullableDecimal(reader, 8);
        entities.DashboardPerformanceMetrics.CollectionsFfytdActual =
          db.GetNullableDecimal(reader, 9);
        entities.DashboardPerformanceMetrics.CollectionsFfytdPriorYear =
          db.GetNullableDecimal(reader, 10);
        entities.DashboardPerformanceMetrics.CollectionsFfytdPercentChange =
          db.GetNullableDecimal(reader, 11);
        entities.DashboardPerformanceMetrics.CollectionsInMonthActual =
          db.GetNullableDecimal(reader, 12);
        entities.DashboardPerformanceMetrics.CollectionsInMonthPriorYear =
          db.GetNullableDecimal(reader, 13);
        entities.DashboardPerformanceMetrics.CollectionsInMonthPercentChg =
          db.GetNullableDecimal(reader, 14);
        entities.DashboardPerformanceMetrics.ArrearsDistributedMonthActual =
          db.GetNullableDecimal(reader, 15);
        entities.DashboardPerformanceMetrics.ArrearsDistributedFfytdActual =
          db.GetNullableDecimal(reader, 16);
        entities.DashboardPerformanceMetrics.ArrearsDueActual =
          db.GetNullableDecimal(reader, 17);
        entities.DashboardPerformanceMetrics.CollectionsPerObligCaseAvg =
          db.GetNullableDecimal(reader, 18);
        entities.DashboardPerformanceMetrics.IwoPerObligCaseAverage =
          db.GetNullableDecimal(reader, 19);
        entities.DashboardPerformanceMetrics.CasesPerFteAverage =
          db.GetNullableDecimal(reader, 20);
        entities.DashboardPerformanceMetrics.CollectionsPerFteAverage =
          db.GetNullableDecimal(reader, 21);
        entities.DashboardPerformanceMetrics.CasesPayingPercent =
          db.GetNullableDecimal(reader, 22);
        entities.DashboardPerformanceMetrics.Populated = true;
      });
  }

  private bool ReadServiceProvider()
  {
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider",
      (db, command) =>
      {
        db.SetString(command, "userId", export.ServiceProvider.UserId);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.UserId = db.GetString(reader, 1);
        entities.ServiceProvider.LastName = db.GetString(reader, 2);
        entities.ServiceProvider.FirstName = db.GetString(reader, 3);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 4);
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
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of NumberOfDecimals.
      /// </summary>
      [JsonPropertyName("numberOfDecimals")]
      public Common NumberOfDecimals
      {
        get => numberOfDecimals ??= new();
        set => numberOfDecimals = value;
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
      /// A value of Hidden.
      /// </summary>
      [JsonPropertyName("hidden")]
      public WorkArea Hidden
      {
        get => hidden ??= new();
        set => hidden = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 165;

      private Common numberOfDecimals;
      private WorkArea workArea;
      private WorkArea hidden;
    }

    /// <summary>
    /// A value of CseOrganization.
    /// </summary>
    [JsonPropertyName("cseOrganization")]
    public CseOrganization CseOrganization
    {
      get => cseOrganization ??= new();
      set => cseOrganization = value;
    }

    /// <summary>
    /// A value of HiddenCseOrganization.
    /// </summary>
    [JsonPropertyName("hiddenCseOrganization")]
    public CseOrganization HiddenCseOrganization
    {
      get => hiddenCseOrganization ??= new();
      set => hiddenCseOrganization = value;
    }

    /// <summary>
    /// A value of Jd.
    /// </summary>
    [JsonPropertyName("jd")]
    public Common Jd
    {
      get => jd ??= new();
      set => jd = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    /// <summary>
    /// A value of YearMonth.
    /// </summary>
    [JsonPropertyName("yearMonth")]
    public WorkArea YearMonth
    {
      get => yearMonth ??= new();
      set => yearMonth = value;
    }

    /// <summary>
    /// A value of HiddenYearMonth.
    /// </summary>
    [JsonPropertyName("hiddenYearMonth")]
    public WorkArea HiddenYearMonth
    {
      get => hiddenYearMonth ??= new();
      set => hiddenYearMonth = value;
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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    /// <summary>
    /// A value of FromCsor.
    /// </summary>
    [JsonPropertyName("fromCsor")]
    public CseOrganization FromCsor
    {
      get => fromCsor ??= new();
      set => fromCsor = value;
    }

    /// <summary>
    /// A value of HiddenServiceProvider.
    /// </summary>
    [JsonPropertyName("hiddenServiceProvider")]
    public ServiceProvider HiddenServiceProvider
    {
      get => hiddenServiceProvider ??= new();
      set => hiddenServiceProvider = value;
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
    /// A value of WorkerName.
    /// </summary>
    [JsonPropertyName("workerName")]
    public WorkArea WorkerName
    {
      get => workerName ??= new();
      set => workerName = value;
    }

    /// <summary>
    /// A value of Worker.
    /// </summary>
    [JsonPropertyName("worker")]
    public Common Worker
    {
      get => worker ??= new();
      set => worker = value;
    }

    /// <summary>
    /// A value of FromSvpl.
    /// </summary>
    [JsonPropertyName("fromSvpl")]
    public ServiceProvider FromSvpl
    {
      get => fromSvpl ??= new();
      set => fromSvpl = value;
    }

    private CseOrganization cseOrganization;
    private CseOrganization hiddenCseOrganization;
    private Common jd;
    private Array<GroupGroup> group;
    private WorkArea yearMonth;
    private WorkArea hiddenYearMonth;
    private Standard standard;
    private NextTranInfo hiddenNextTranInfo;
    private CseOrganization fromCsor;
    private ServiceProvider hiddenServiceProvider;
    private ServiceProvider serviceProvider;
    private WorkArea workerName;
    private Common worker;
    private ServiceProvider fromSvpl;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of NumberOfDecimals.
      /// </summary>
      [JsonPropertyName("numberOfDecimals")]
      public Common NumberOfDecimals
      {
        get => numberOfDecimals ??= new();
        set => numberOfDecimals = value;
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
      /// A value of Hidden.
      /// </summary>
      [JsonPropertyName("hidden")]
      public WorkArea Hidden
      {
        get => hidden ??= new();
        set => hidden = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 165;

      private Common numberOfDecimals;
      private WorkArea workArea;
      private WorkArea hidden;
    }

    /// <summary>
    /// A value of ToCsor.
    /// </summary>
    [JsonPropertyName("toCsor")]
    public CseOrganization ToCsor
    {
      get => toCsor ??= new();
      set => toCsor = value;
    }

    /// <summary>
    /// A value of CseOrganization.
    /// </summary>
    [JsonPropertyName("cseOrganization")]
    public CseOrganization CseOrganization
    {
      get => cseOrganization ??= new();
      set => cseOrganization = value;
    }

    /// <summary>
    /// A value of HiddenCseOrganization.
    /// </summary>
    [JsonPropertyName("hiddenCseOrganization")]
    public CseOrganization HiddenCseOrganization
    {
      get => hiddenCseOrganization ??= new();
      set => hiddenCseOrganization = value;
    }

    /// <summary>
    /// A value of Jd.
    /// </summary>
    [JsonPropertyName("jd")]
    public Common Jd
    {
      get => jd ??= new();
      set => jd = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    /// <summary>
    /// A value of YearMonth.
    /// </summary>
    [JsonPropertyName("yearMonth")]
    public WorkArea YearMonth
    {
      get => yearMonth ??= new();
      set => yearMonth = value;
    }

    /// <summary>
    /// A value of YearMonthHidden.
    /// </summary>
    [JsonPropertyName("yearMonthHidden")]
    public WorkArea YearMonthHidden
    {
      get => yearMonthHidden ??= new();
      set => yearMonthHidden = value;
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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    /// <summary>
    /// A value of HiddenServiceProvider.
    /// </summary>
    [JsonPropertyName("hiddenServiceProvider")]
    public ServiceProvider HiddenServiceProvider
    {
      get => hiddenServiceProvider ??= new();
      set => hiddenServiceProvider = value;
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
    /// A value of WorkerName.
    /// </summary>
    [JsonPropertyName("workerName")]
    public WorkArea WorkerName
    {
      get => workerName ??= new();
      set => workerName = value;
    }

    /// <summary>
    /// A value of Worker.
    /// </summary>
    [JsonPropertyName("worker")]
    public Common Worker
    {
      get => worker ??= new();
      set => worker = value;
    }

    private CseOrganization toCsor;
    private CseOrganization cseOrganization;
    private CseOrganization hiddenCseOrganization;
    private Common jd;
    private Array<GroupGroup> group;
    private WorkArea yearMonth;
    private WorkArea yearMonthHidden;
    private Standard standard;
    private NextTranInfo hiddenNextTranInfo;
    private ServiceProvider hiddenServiceProvider;
    private ServiceProvider serviceProvider;
    private WorkArea workerName;
    private Common worker;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of NullCseOrganization.
    /// </summary>
    [JsonPropertyName("nullCseOrganization")]
    public CseOrganization NullCseOrganization
    {
      get => nullCseOrganization ??= new();
      set => nullCseOrganization = value;
    }

    /// <summary>
    /// A value of NullServiceProvider.
    /// </summary>
    [JsonPropertyName("nullServiceProvider")]
    public ServiceProvider NullServiceProvider
    {
      get => nullServiceProvider ??= new();
      set => nullServiceProvider = value;
    }

    /// <summary>
    /// A value of Prompt.
    /// </summary>
    [JsonPropertyName("prompt")]
    public Common Prompt
    {
      get => prompt ??= new();
      set => prompt = value;
    }

    /// <summary>
    /// A value of Worker.
    /// </summary>
    [JsonPropertyName("worker")]
    public DashboardOutputMetrics Worker
    {
      get => worker ??= new();
      set => worker = value;
    }

    /// <summary>
    /// A value of JdDashboardOutputMetrics.
    /// </summary>
    [JsonPropertyName("jdDashboardOutputMetrics")]
    public DashboardOutputMetrics JdDashboardOutputMetrics
    {
      get => jdDashboardOutputMetrics ??= new();
      set => jdDashboardOutputMetrics = value;
    }

    /// <summary>
    /// A value of CurrentMonth.
    /// </summary>
    [JsonPropertyName("currentMonth")]
    public WorkArea CurrentMonth
    {
      get => currentMonth ??= new();
      set => currentMonth = value;
    }

    /// <summary>
    /// A value of ValidNumber.
    /// </summary>
    [JsonPropertyName("validNumber")]
    public Common ValidNumber
    {
      get => validNumber ??= new();
      set => validNumber = value;
    }

    /// <summary>
    /// A value of NumericCharacterFound.
    /// </summary>
    [JsonPropertyName("numericCharacterFound")]
    public Common NumericCharacterFound
    {
      get => numericCharacterFound ??= new();
      set => numericCharacterFound = value;
    }

    /// <summary>
    /// A value of PositiveSignFound.
    /// </summary>
    [JsonPropertyName("positiveSignFound")]
    public Common PositiveSignFound
    {
      get => positiveSignFound ??= new();
      set => positiveSignFound = value;
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
    /// A value of NegativeSignFound.
    /// </summary>
    [JsonPropertyName("negativeSignFound")]
    public Common NegativeSignFound
    {
      get => negativeSignFound ??= new();
      set => negativeSignFound = value;
    }

    /// <summary>
    /// A value of DecimalFound.
    /// </summary>
    [JsonPropertyName("decimalFound")]
    public Common DecimalFound
    {
      get => decimalFound ??= new();
      set => decimalFound = value;
    }

    /// <summary>
    /// A value of ForConversion.
    /// </summary>
    [JsonPropertyName("forConversion")]
    public Common ForConversion
    {
      get => forConversion ??= new();
      set => forConversion = value;
    }

    /// <summary>
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public WorkArea Blank
    {
      get => blank ??= new();
      set => blank = value;
    }

    /// <summary>
    /// A value of NullWorkArea.
    /// </summary>
    [JsonPropertyName("nullWorkArea")]
    public WorkArea NullWorkArea
    {
      get => nullWorkArea ??= new();
      set => nullWorkArea = value;
    }

    /// <summary>
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public Common Temp
    {
      get => temp ??= new();
      set => temp = value;
    }

    /// <summary>
    /// A value of JdDashboardPerformanceMetrics.
    /// </summary>
    [JsonPropertyName("jdDashboardPerformanceMetrics")]
    public DashboardPerformanceMetrics JdDashboardPerformanceMetrics
    {
      get => jdDashboardPerformanceMetrics ??= new();
      set => jdDashboardPerformanceMetrics = value;
    }

    /// <summary>
    /// A value of StaffingMonth.
    /// </summary>
    [JsonPropertyName("staffingMonth")]
    public DateWorkArea StaffingMonth
    {
      get => staffingMonth ??= new();
      set => staffingMonth = value;
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
    /// A value of NextTranInfo.
    /// </summary>
    [JsonPropertyName("nextTranInfo")]
    public NextTranInfo NextTranInfo
    {
      get => nextTranInfo ??= new();
      set => nextTranInfo = value;
    }

    private CseOrganization nullCseOrganization;
    private ServiceProvider nullServiceProvider;
    private Common prompt;
    private DashboardOutputMetrics worker;
    private DashboardOutputMetrics jdDashboardOutputMetrics;
    private WorkArea currentMonth;
    private Common validNumber;
    private Common numericCharacterFound;
    private Common positiveSignFound;
    private WorkArea workArea;
    private Common negativeSignFound;
    private Common decimalFound;
    private Common forConversion;
    private WorkArea blank;
    private WorkArea nullWorkArea;
    private Common temp;
    private DashboardPerformanceMetrics jdDashboardPerformanceMetrics;
    private DateWorkArea staffingMonth;
    private Common common;
    private NextTranInfo nextTranInfo;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of TbdOld.
    /// </summary>
    [JsonPropertyName("tbdOld")]
    public DashboardOutputMetrics TbdOld
    {
      get => tbdOld ??= new();
      set => tbdOld = value;
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
    /// A value of DashboardOutputMetrics.
    /// </summary>
    [JsonPropertyName("dashboardOutputMetrics")]
    public DashboardOutputMetrics DashboardOutputMetrics
    {
      get => dashboardOutputMetrics ??= new();
      set => dashboardOutputMetrics = value;
    }

    /// <summary>
    /// A value of CseOrganization.
    /// </summary>
    [JsonPropertyName("cseOrganization")]
    public CseOrganization CseOrganization
    {
      get => cseOrganization ??= new();
      set => cseOrganization = value;
    }

    /// <summary>
    /// A value of DashboardPerformanceMetrics.
    /// </summary>
    [JsonPropertyName("dashboardPerformanceMetrics")]
    public DashboardPerformanceMetrics DashboardPerformanceMetrics
    {
      get => dashboardPerformanceMetrics ??= new();
      set => dashboardPerformanceMetrics = value;
    }

    private DashboardOutputMetrics tbdOld;
    private ServiceProvider serviceProvider;
    private DashboardOutputMetrics dashboardOutputMetrics;
    private CseOrganization cseOrganization;
    private DashboardPerformanceMetrics dashboardPerformanceMetrics;
  }
#endregion
}
