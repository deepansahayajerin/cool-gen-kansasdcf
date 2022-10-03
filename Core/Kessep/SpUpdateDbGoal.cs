// Program: SP_UPDATE_DB_GOAL, ID: 945142410, model: 746.
// Short name: SWE03101
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_UPDATE_DB_GOAL.
/// </summary>
[Serializable]
public partial class SpUpdateDbGoal: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_UPDATE_DB_GOAL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpUpdateDbGoal(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpUpdateDbGoal.
  /// </summary>
  public SpUpdateDbGoal(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (!IsEmpty(import.JdDashboardPerformanceMetrics.ReportLevelId))
    {
      if (!ReadDashboardPerformanceMetrics())
      {
        ExitState = "DASHBOARD_GOAL_NF";

        return;
      }

      if (!ReadDashboardOutputMetrics1())
      {
        ExitState = "DASHBOARD_GOAL_NF";

        return;
      }

      try
      {
        UpdateDashboardPerformanceMetrics();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "DASHBOARD_GOAL_NU";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "DASHBOARD_GOAL_PV";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      try
      {
        UpdateDashboardOutputMetrics1();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "DASHBOARD_GOAL_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "DASHBOARD_GOAL_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    else
    {
      if (!ReadDashboardOutputMetrics2())
      {
        ExitState = "DASHBOARD_GOAL_NF";

        return;
      }

      try
      {
        UpdateDashboardOutputMetrics2();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "DASHBOARD_GOAL_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "DASHBOARD_GOAL_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
  }

  private bool ReadDashboardOutputMetrics1()
  {
    entities.JdDashboardOutputMetrics.Populated = false;

    return Read("ReadDashboardOutputMetrics1",
      (db, command) =>
      {
        db.SetInt32(
          command, "reportMonth", import.JdDashboardOutputMetrics.ReportMonth);
        db.SetString(
          command, "reportLevel", import.JdDashboardOutputMetrics.ReportLevel);
        db.SetString(
          command, "reportLevelId",
          import.JdDashboardOutputMetrics.ReportLevelId);
        db.SetString(command, "type", import.JdDashboardOutputMetrics.Type1);
      },
      (db, reader) =>
      {
        entities.JdDashboardOutputMetrics.ReportMonth = db.GetInt32(reader, 0);
        entities.JdDashboardOutputMetrics.ReportLevel = db.GetString(reader, 1);
        entities.JdDashboardOutputMetrics.ReportLevelId =
          db.GetString(reader, 2);
        entities.JdDashboardOutputMetrics.Type1 = db.GetString(reader, 3);
        entities.JdDashboardOutputMetrics.AsOfDate =
          db.GetNullableDate(reader, 4);
        entities.JdDashboardOutputMetrics.NewOrdersEstablished =
          db.GetNullableInt32(reader, 5);
        entities.JdDashboardOutputMetrics.PaternitiesEstablished =
          db.GetNullableInt32(reader, 6);
        entities.JdDashboardOutputMetrics.CasesOpenedWithOrder =
          db.GetNullableInt32(reader, 7);
        entities.JdDashboardOutputMetrics.CasesOpenedWithoutOrders =
          db.GetNullableInt32(reader, 8);
        entities.JdDashboardOutputMetrics.CasesClosedWithOrders =
          db.GetNullableInt32(reader, 9);
        entities.JdDashboardOutputMetrics.CasesClosedWithoutOrders =
          db.GetNullableInt32(reader, 10);
        entities.JdDashboardOutputMetrics.Modifications =
          db.GetNullableInt32(reader, 11);
        entities.JdDashboardOutputMetrics.IncomeWithholdingsIssued =
          db.GetNullableInt32(reader, 12);
        entities.JdDashboardOutputMetrics.ContemptMotionFilings =
          db.GetNullableInt32(reader, 13);
        entities.JdDashboardOutputMetrics.ContemptOrderFilings =
          db.GetNullableInt32(reader, 14);
        entities.JdDashboardOutputMetrics.DaysToOrderEstblshmntAvg =
          db.GetNullableDecimal(reader, 15);
        entities.JdDashboardOutputMetrics.DaysToReturnOfServiceAvg =
          db.GetNullableDecimal(reader, 16);
        entities.JdDashboardOutputMetrics.ReferralAging60To90Days =
          db.GetNullableInt32(reader, 17);
        entities.JdDashboardOutputMetrics.ReferralAging91To120Days =
          db.GetNullableInt32(reader, 18);
        entities.JdDashboardOutputMetrics.ReferralAging121To150Days =
          db.GetNullableInt32(reader, 19);
        entities.JdDashboardOutputMetrics.ReferralAging151PlusDays =
          db.GetNullableInt32(reader, 20);
        entities.JdDashboardOutputMetrics.DaysToIwoPaymentAverage =
          db.GetNullableDecimal(reader, 21);
        entities.JdDashboardOutputMetrics.ReferralsToLegalForEst =
          db.GetNullableInt32(reader, 22);
        entities.JdDashboardOutputMetrics.ReferralsToLegalForEnf =
          db.GetNullableInt32(reader, 23);
        entities.JdDashboardOutputMetrics.Populated = true;
      });
  }

  private bool ReadDashboardOutputMetrics2()
  {
    entities.Worker.Populated = false;

    return Read("ReadDashboardOutputMetrics2",
      (db, command) =>
      {
        db.SetInt32(command, "reportMonth", import.Worker.ReportMonth);
        db.SetString(command, "reportLevel", import.Worker.ReportLevel);
        db.SetString(command, "reportLevelId", import.Worker.ReportLevelId);
        db.SetString(command, "type", import.Worker.Type1);
      },
      (db, reader) =>
      {
        entities.Worker.ReportMonth = db.GetInt32(reader, 0);
        entities.Worker.ReportLevel = db.GetString(reader, 1);
        entities.Worker.ReportLevelId = db.GetString(reader, 2);
        entities.Worker.Type1 = db.GetString(reader, 3);
        entities.Worker.AsOfDate = db.GetNullableDate(reader, 4);
        entities.Worker.NewOrdersEstablished = db.GetNullableInt32(reader, 5);
        entities.Worker.PaternitiesEstablished = db.GetNullableInt32(reader, 6);
        entities.Worker.Modifications = db.GetNullableInt32(reader, 7);
        entities.Worker.IncomeWithholdingsIssued =
          db.GetNullableInt32(reader, 8);
        entities.Worker.ContemptMotionFilings = db.GetNullableInt32(reader, 9);
        entities.Worker.ContemptOrderFilings = db.GetNullableInt32(reader, 10);
        entities.Worker.TotalCollectionAmount =
          db.GetNullableDecimal(reader, 11);
        entities.Worker.DaysToOrderEstblshmntAvg =
          db.GetNullableDecimal(reader, 12);
        entities.Worker.DaysToReturnOfServiceAvg =
          db.GetNullableDecimal(reader, 13);
        entities.Worker.ReferralAging60To90Days =
          db.GetNullableInt32(reader, 14);
        entities.Worker.ReferralAging91To120Days =
          db.GetNullableInt32(reader, 15);
        entities.Worker.ReferralAging121To150Days =
          db.GetNullableInt32(reader, 16);
        entities.Worker.ReferralAging151PlusDays =
          db.GetNullableInt32(reader, 17);
        entities.Worker.DaysToIwoPaymentAverage =
          db.GetNullableDecimal(reader, 18);
        entities.Worker.ReferralsToLegalForEst =
          db.GetNullableInt32(reader, 19);
        entities.Worker.ReferralsToLegalForEnf =
          db.GetNullableInt32(reader, 20);
        entities.Worker.CasesOpened = db.GetNullableInt32(reader, 21);
        entities.Worker.NcpLocatesByAddress = db.GetNullableInt32(reader, 22);
        entities.Worker.NcpLocatesByEmployer = db.GetNullableInt32(reader, 23);
        entities.Worker.CaseClosures = db.GetNullableInt32(reader, 24);
        entities.Worker.CaseReviews = db.GetNullableInt32(reader, 25);
        entities.Worker.Populated = true;
      });
  }

  private bool ReadDashboardPerformanceMetrics()
  {
    entities.JdDashboardPerformanceMetrics.Populated = false;

    return Read("ReadDashboardPerformanceMetrics",
      (db, command) =>
      {
        db.SetInt32(
          command, "reportMonth",
          import.JdDashboardPerformanceMetrics.ReportMonth);
        db.SetString(
          command, "reportLevel",
          import.JdDashboardPerformanceMetrics.ReportLevel);
        db.SetString(
          command, "reportLevelId",
          import.JdDashboardPerformanceMetrics.ReportLevelId);
        db.
          SetString(command, "type", import.JdDashboardPerformanceMetrics.Type1);
          
      },
      (db, reader) =>
      {
        entities.JdDashboardPerformanceMetrics.ReportMonth =
          db.GetInt32(reader, 0);
        entities.JdDashboardPerformanceMetrics.ReportLevel =
          db.GetString(reader, 1);
        entities.JdDashboardPerformanceMetrics.ReportLevelId =
          db.GetString(reader, 2);
        entities.JdDashboardPerformanceMetrics.Type1 = db.GetString(reader, 3);
        entities.JdDashboardPerformanceMetrics.AsOfDate =
          db.GetNullableDate(reader, 4);
        entities.JdDashboardPerformanceMetrics.CasesUnderOrderNumerator =
          db.GetNullableInt32(reader, 5);
        entities.JdDashboardPerformanceMetrics.CasesUnderOrderDenominator =
          db.GetNullableInt32(reader, 6);
        entities.JdDashboardPerformanceMetrics.CasesUnderOrderPercent =
          db.GetNullableDecimal(reader, 7);
        entities.JdDashboardPerformanceMetrics.CasesUnderOrderRank =
          db.GetNullableInt32(reader, 8);
        entities.JdDashboardPerformanceMetrics.PepNumerator =
          db.GetNullableInt32(reader, 9);
        entities.JdDashboardPerformanceMetrics.PepDenominator =
          db.GetNullableInt32(reader, 10);
        entities.JdDashboardPerformanceMetrics.PepPercent =
          db.GetNullableDecimal(reader, 11);
        entities.JdDashboardPerformanceMetrics.CasesPayingArrearsNumerator =
          db.GetNullableInt32(reader, 12);
        entities.JdDashboardPerformanceMetrics.CasesPayingArrearsDenominator =
          db.GetNullableInt32(reader, 13);
        entities.JdDashboardPerformanceMetrics.CasesPayingArrearsPercent =
          db.GetNullableDecimal(reader, 14);
        entities.JdDashboardPerformanceMetrics.CasesPayingArrearsRank =
          db.GetNullableInt32(reader, 15);
        entities.JdDashboardPerformanceMetrics.CurrentSupportPaidMthNum =
          db.GetNullableDecimal(reader, 16);
        entities.JdDashboardPerformanceMetrics.CurrentSupportPaidMthDen =
          db.GetNullableDecimal(reader, 17);
        entities.JdDashboardPerformanceMetrics.CurrentSupportPaidMthPer =
          db.GetNullableDecimal(reader, 18);
        entities.JdDashboardPerformanceMetrics.CurrentSupportPaidMthRnk =
          db.GetNullableInt32(reader, 19);
        entities.JdDashboardPerformanceMetrics.CurrentSupportPaidFfytdNum =
          db.GetNullableDecimal(reader, 20);
        entities.JdDashboardPerformanceMetrics.CurrentSupportPaidFfytdDen =
          db.GetNullableDecimal(reader, 21);
        entities.JdDashboardPerformanceMetrics.CurrentSupportPaidFfytdPer =
          db.GetNullableDecimal(reader, 22);
        entities.JdDashboardPerformanceMetrics.CurrentSupportPaidFfytdRnk =
          db.GetNullableInt32(reader, 23);
        entities.JdDashboardPerformanceMetrics.CollectionsFfytdToPriorMonth =
          db.GetNullableDecimal(reader, 24);
        entities.JdDashboardPerformanceMetrics.CollectionsFfytdActual =
          db.GetNullableDecimal(reader, 25);
        entities.JdDashboardPerformanceMetrics.CollectionsFfytdPriorYear =
          db.GetNullableDecimal(reader, 26);
        entities.JdDashboardPerformanceMetrics.CollectionsFfytdPercentChange =
          db.GetNullableDecimal(reader, 27);
        entities.JdDashboardPerformanceMetrics.CollectionsFfytdRnk =
          db.GetNullableInt32(reader, 28);
        entities.JdDashboardPerformanceMetrics.CollectionsInMonthActual =
          db.GetNullableDecimal(reader, 29);
        entities.JdDashboardPerformanceMetrics.CollectionsInMonthPriorYear =
          db.GetNullableDecimal(reader, 30);
        entities.JdDashboardPerformanceMetrics.CollectionsInMonthPercentChg =
          db.GetNullableDecimal(reader, 31);
        entities.JdDashboardPerformanceMetrics.CollectionsInMonthRnk =
          db.GetNullableInt32(reader, 32);
        entities.JdDashboardPerformanceMetrics.ArrearsDistributedMonthActual =
          db.GetNullableDecimal(reader, 33);
        entities.JdDashboardPerformanceMetrics.ArrearsDistributedMonthRnk =
          db.GetNullableInt32(reader, 34);
        entities.JdDashboardPerformanceMetrics.ArrearsDistributedFfytdActual =
          db.GetNullableDecimal(reader, 35);
        entities.JdDashboardPerformanceMetrics.ArrearsDistrubutedFfytdRnk =
          db.GetNullableInt32(reader, 36);
        entities.JdDashboardPerformanceMetrics.ArrearsDueActual =
          db.GetNullableDecimal(reader, 37);
        entities.JdDashboardPerformanceMetrics.ArrearsDueRnk =
          db.GetNullableInt32(reader, 38);
        entities.JdDashboardPerformanceMetrics.CollectionsPerObligCaseNumer =
          db.GetNullableDecimal(reader, 39);
        entities.JdDashboardPerformanceMetrics.CollectionsPerObligCaseDenom =
          db.GetNullableDecimal(reader, 40);
        entities.JdDashboardPerformanceMetrics.CollectionsPerObligCaseAvg =
          db.GetNullableDecimal(reader, 41);
        entities.JdDashboardPerformanceMetrics.CollectionsPerObligCaseRnk =
          db.GetNullableInt32(reader, 42);
        entities.JdDashboardPerformanceMetrics.IwoPerObligCaseNumerator =
          db.GetNullableInt32(reader, 43);
        entities.JdDashboardPerformanceMetrics.IwoPerObligCaseDenominator =
          db.GetNullableInt32(reader, 44);
        entities.JdDashboardPerformanceMetrics.IwoPerObligCaseAverage =
          db.GetNullableDecimal(reader, 45);
        entities.JdDashboardPerformanceMetrics.IwoPerObligCaseRnk =
          db.GetNullableInt32(reader, 46);
        entities.JdDashboardPerformanceMetrics.CasesPerFteNumerator =
          db.GetNullableInt32(reader, 47);
        entities.JdDashboardPerformanceMetrics.CasesPerFteDenominator =
          db.GetNullableDecimal(reader, 48);
        entities.JdDashboardPerformanceMetrics.CasesPerFteAverage =
          db.GetNullableDecimal(reader, 49);
        entities.JdDashboardPerformanceMetrics.CasesPerFteRank =
          db.GetNullableInt32(reader, 50);
        entities.JdDashboardPerformanceMetrics.CollectionsPerFteNumerator =
          db.GetNullableDecimal(reader, 51);
        entities.JdDashboardPerformanceMetrics.CollectionsPerFteDenominator =
          db.GetNullableDecimal(reader, 52);
        entities.JdDashboardPerformanceMetrics.CollectionsPerFteAverage =
          db.GetNullableDecimal(reader, 53);
        entities.JdDashboardPerformanceMetrics.CollectionsPerFteRank =
          db.GetNullableInt32(reader, 54);
        entities.JdDashboardPerformanceMetrics.CasesPayingNumerator =
          db.GetNullableInt32(reader, 55);
        entities.JdDashboardPerformanceMetrics.CasesPayingDenominator =
          db.GetNullableInt32(reader, 56);
        entities.JdDashboardPerformanceMetrics.CasesPayingPercent =
          db.GetNullableDecimal(reader, 57);
        entities.JdDashboardPerformanceMetrics.CasesPayingRank =
          db.GetNullableInt32(reader, 58);
        entities.JdDashboardPerformanceMetrics.PepRank =
          db.GetNullableInt32(reader, 59);
        entities.JdDashboardPerformanceMetrics.Populated = true;
      });
  }

  private void UpdateDashboardOutputMetrics1()
  {
    var asOfDate = import.JdDashboardOutputMetrics.AsOfDate;
    var newOrdersEstablished =
      import.JdDashboardOutputMetrics.NewOrdersEstablished.GetValueOrDefault();
    var paternitiesEstablished =
      import.JdDashboardOutputMetrics.PaternitiesEstablished.
        GetValueOrDefault();
    var casesOpenedWithOrder =
      import.JdDashboardOutputMetrics.CasesOpenedWithOrder.GetValueOrDefault();
    var casesOpenedWithoutOrders =
      import.JdDashboardOutputMetrics.CasesOpenedWithoutOrders.
        GetValueOrDefault();
    var casesClosedWithOrders =
      import.JdDashboardOutputMetrics.CasesClosedWithOrders.GetValueOrDefault();
      
    var casesClosedWithoutOrders =
      import.JdDashboardOutputMetrics.CasesClosedWithoutOrders.
        GetValueOrDefault();
    var modifications =
      import.JdDashboardOutputMetrics.Modifications.GetValueOrDefault();
    var incomeWithholdingsIssued =
      import.JdDashboardOutputMetrics.IncomeWithholdingsIssued.
        GetValueOrDefault();
    var contemptMotionFilings =
      import.JdDashboardOutputMetrics.ContemptMotionFilings.GetValueOrDefault();
      
    var contemptOrderFilings =
      import.JdDashboardOutputMetrics.ContemptOrderFilings.GetValueOrDefault();
    var daysToOrderEstblshmntAvg =
      import.JdDashboardOutputMetrics.DaysToOrderEstblshmntAvg.
        GetValueOrDefault();
    var daysToReturnOfServiceAvg =
      import.JdDashboardOutputMetrics.DaysToReturnOfServiceAvg.
        GetValueOrDefault();
    var referralAging60To90Days =
      import.JdDashboardOutputMetrics.ReferralAging60To90Days.
        GetValueOrDefault();
    var referralAging91To120Days =
      import.JdDashboardOutputMetrics.ReferralAging91To120Days.
        GetValueOrDefault();
    var referralAging121To150Days =
      import.JdDashboardOutputMetrics.ReferralAging121To150Days.
        GetValueOrDefault();
    var referralAging151PlusDays =
      import.JdDashboardOutputMetrics.ReferralAging151PlusDays.
        GetValueOrDefault();
    var daysToIwoPaymentAverage =
      import.JdDashboardOutputMetrics.DaysToIwoPaymentAverage.
        GetValueOrDefault();
    var referralsToLegalForEst =
      import.JdDashboardOutputMetrics.ReferralsToLegalForEst.
        GetValueOrDefault();
    var referralsToLegalForEnf =
      import.JdDashboardOutputMetrics.ReferralsToLegalForEnf.
        GetValueOrDefault();

    entities.JdDashboardOutputMetrics.Populated = false;
    Update("UpdateDashboardOutputMetrics1",
      (db, command) =>
      {
        db.SetNullableDate(command, "asOfDate", asOfDate);
        db.SetNullableInt32(command, "newOrdEst", newOrdersEstablished);
        db.SetNullableInt32(command, "paternitiesEst", paternitiesEstablished);
        db.SetNullableInt32(command, "casesOpnWOrder", casesOpenedWithOrder);
        db.
          SetNullableInt32(command, "casesOpnWoOrder", casesOpenedWithoutOrders);
          
        db.SetNullableInt32(command, "casesClsWOrder", casesClosedWithOrders);
        db.
          SetNullableInt32(command, "casesClsWoOrder", casesClosedWithoutOrders);
          
        db.SetNullableInt32(command, "modifications", modifications);
        db.SetNullableInt32(command, "iwIssued", incomeWithholdingsIssued);
        db.SetNullableInt32(command, "cntmptMtnFiled", contemptMotionFilings);
        db.SetNullableInt32(command, "cntmptOrdFiled", contemptOrderFilings);
        db.
          SetNullableDecimal(command, "ordEstDaysAvg", daysToOrderEstblshmntAvg);
          
        db.SetNullableDecimal(
          command, "retServDaysAvg", daysToReturnOfServiceAvg);
        db.SetNullableInt32(command, "refAge60To90", referralAging60To90Days);
        db.SetNullableInt32(command, "refAge91To120", referralAging91To120Days);
        db.
          SetNullableInt32(command, "refAge121To150", referralAging121To150Days);
          
        db.SetNullableInt32(command, "refAge151Plus", referralAging151PlusDays);
        db.
          SetNullableDecimal(command, "iwoPmtDaysAvg", daysToIwoPaymentAverage);
          
        db.SetNullableInt32(command, "estRefToLegal", referralsToLegalForEst);
        db.SetNullableInt32(command, "enfRefToLegal", referralsToLegalForEnf);
        db.SetInt32(
          command, "reportMonth",
          entities.JdDashboardOutputMetrics.ReportMonth);
        db.SetString(
          command, "reportLevel",
          entities.JdDashboardOutputMetrics.ReportLevel);
        db.SetString(
          command, "reportLevelId",
          entities.JdDashboardOutputMetrics.ReportLevelId);
        db.SetString(command, "type", entities.JdDashboardOutputMetrics.Type1);
      });

    entities.JdDashboardOutputMetrics.AsOfDate = asOfDate;
    entities.JdDashboardOutputMetrics.NewOrdersEstablished =
      newOrdersEstablished;
    entities.JdDashboardOutputMetrics.PaternitiesEstablished =
      paternitiesEstablished;
    entities.JdDashboardOutputMetrics.CasesOpenedWithOrder =
      casesOpenedWithOrder;
    entities.JdDashboardOutputMetrics.CasesOpenedWithoutOrders =
      casesOpenedWithoutOrders;
    entities.JdDashboardOutputMetrics.CasesClosedWithOrders =
      casesClosedWithOrders;
    entities.JdDashboardOutputMetrics.CasesClosedWithoutOrders =
      casesClosedWithoutOrders;
    entities.JdDashboardOutputMetrics.Modifications = modifications;
    entities.JdDashboardOutputMetrics.IncomeWithholdingsIssued =
      incomeWithholdingsIssued;
    entities.JdDashboardOutputMetrics.ContemptMotionFilings =
      contemptMotionFilings;
    entities.JdDashboardOutputMetrics.ContemptOrderFilings =
      contemptOrderFilings;
    entities.JdDashboardOutputMetrics.DaysToOrderEstblshmntAvg =
      daysToOrderEstblshmntAvg;
    entities.JdDashboardOutputMetrics.DaysToReturnOfServiceAvg =
      daysToReturnOfServiceAvg;
    entities.JdDashboardOutputMetrics.ReferralAging60To90Days =
      referralAging60To90Days;
    entities.JdDashboardOutputMetrics.ReferralAging91To120Days =
      referralAging91To120Days;
    entities.JdDashboardOutputMetrics.ReferralAging121To150Days =
      referralAging121To150Days;
    entities.JdDashboardOutputMetrics.ReferralAging151PlusDays =
      referralAging151PlusDays;
    entities.JdDashboardOutputMetrics.DaysToIwoPaymentAverage =
      daysToIwoPaymentAverage;
    entities.JdDashboardOutputMetrics.ReferralsToLegalForEst =
      referralsToLegalForEst;
    entities.JdDashboardOutputMetrics.ReferralsToLegalForEnf =
      referralsToLegalForEnf;
    entities.JdDashboardOutputMetrics.Populated = true;
  }

  private void UpdateDashboardOutputMetrics2()
  {
    var asOfDate = import.Worker.AsOfDate;
    var newOrdersEstablished =
      import.Worker.NewOrdersEstablished.GetValueOrDefault();
    var paternitiesEstablished =
      import.Worker.PaternitiesEstablished.GetValueOrDefault();
    var modifications = import.Worker.Modifications.GetValueOrDefault();
    var incomeWithholdingsIssued =
      import.Worker.IncomeWithholdingsIssued.GetValueOrDefault();
    var contemptMotionFilings =
      import.Worker.ContemptMotionFilings.GetValueOrDefault();
    var contemptOrderFilings =
      import.Worker.ContemptOrderFilings.GetValueOrDefault();
    var totalCollectionAmount =
      import.Worker.TotalCollectionAmount.GetValueOrDefault();
    var daysToOrderEstblshmntAvg =
      import.Worker.DaysToOrderEstblshmntAvg.GetValueOrDefault();
    var daysToReturnOfServiceAvg =
      import.Worker.DaysToReturnOfServiceAvg.GetValueOrDefault();
    var referralAging60To90Days =
      import.Worker.ReferralAging60To90Days.GetValueOrDefault();
    var referralAging91To120Days =
      import.Worker.ReferralAging91To120Days.GetValueOrDefault();
    var referralAging121To150Days =
      import.Worker.ReferralAging121To150Days.GetValueOrDefault();
    var referralAging151PlusDays =
      import.Worker.ReferralAging151PlusDays.GetValueOrDefault();
    var daysToIwoPaymentAverage =
      import.Worker.DaysToIwoPaymentAverage.GetValueOrDefault();
    var referralsToLegalForEst =
      import.Worker.ReferralsToLegalForEst.GetValueOrDefault();
    var referralsToLegalForEnf =
      import.Worker.ReferralsToLegalForEnf.GetValueOrDefault();
    var casesOpened = import.Worker.CasesOpened.GetValueOrDefault();
    var ncpLocatesByAddress =
      import.Worker.NcpLocatesByAddress.GetValueOrDefault();
    var ncpLocatesByEmployer =
      import.Worker.NcpLocatesByEmployer.GetValueOrDefault();
    var caseClosures = import.Worker.CaseClosures.GetValueOrDefault();
    var caseReviews = import.Worker.CaseReviews.GetValueOrDefault();

    entities.Worker.Populated = false;
    Update("UpdateDashboardOutputMetrics2",
      (db, command) =>
      {
        db.SetNullableDate(command, "asOfDate", asOfDate);
        db.SetNullableInt32(command, "newOrdEst", newOrdersEstablished);
        db.SetNullableInt32(command, "paternitiesEst", paternitiesEstablished);
        db.SetNullableInt32(command, "modifications", modifications);
        db.SetNullableInt32(command, "iwIssued", incomeWithholdingsIssued);
        db.SetNullableInt32(command, "cntmptMtnFiled", contemptMotionFilings);
        db.SetNullableInt32(command, "cntmptOrdFiled", contemptOrderFilings);
        db.SetNullableDecimal(command, "totalCollAmt", totalCollectionAmount);
        db.
          SetNullableDecimal(command, "ordEstDaysAvg", daysToOrderEstblshmntAvg);
          
        db.SetNullableDecimal(
          command, "retServDaysAvg", daysToReturnOfServiceAvg);
        db.SetNullableInt32(command, "refAge60To90", referralAging60To90Days);
        db.SetNullableInt32(command, "refAge91To120", referralAging91To120Days);
        db.
          SetNullableInt32(command, "refAge121To150", referralAging121To150Days);
          
        db.SetNullableInt32(command, "refAge151Plus", referralAging151PlusDays);
        db.
          SetNullableDecimal(command, "iwoPmtDaysAvg", daysToIwoPaymentAverage);
          
        db.SetNullableInt32(command, "estRefToLegal", referralsToLegalForEst);
        db.SetNullableInt32(command, "enfRefToLegal", referralsToLegalForEnf);
        db.SetNullableInt32(command, "casesOpened", casesOpened);
        db.SetNullableInt32(command, "ncpLocByAdrss", ncpLocatesByAddress);
        db.SetNullableInt32(command, "ncpLocByEmp", ncpLocatesByEmployer);
        db.SetNullableInt32(command, "caseClosures", caseClosures);
        db.SetNullableInt32(command, "caseReviews", caseReviews);
        db.SetInt32(command, "reportMonth", entities.Worker.ReportMonth);
        db.SetString(command, "reportLevel", entities.Worker.ReportLevel);
        db.SetString(command, "reportLevelId", entities.Worker.ReportLevelId);
        db.SetString(command, "type", entities.Worker.Type1);
      });

    entities.Worker.AsOfDate = asOfDate;
    entities.Worker.NewOrdersEstablished = newOrdersEstablished;
    entities.Worker.PaternitiesEstablished = paternitiesEstablished;
    entities.Worker.Modifications = modifications;
    entities.Worker.IncomeWithholdingsIssued = incomeWithholdingsIssued;
    entities.Worker.ContemptMotionFilings = contemptMotionFilings;
    entities.Worker.ContemptOrderFilings = contemptOrderFilings;
    entities.Worker.TotalCollectionAmount = totalCollectionAmount;
    entities.Worker.DaysToOrderEstblshmntAvg = daysToOrderEstblshmntAvg;
    entities.Worker.DaysToReturnOfServiceAvg = daysToReturnOfServiceAvg;
    entities.Worker.ReferralAging60To90Days = referralAging60To90Days;
    entities.Worker.ReferralAging91To120Days = referralAging91To120Days;
    entities.Worker.ReferralAging121To150Days = referralAging121To150Days;
    entities.Worker.ReferralAging151PlusDays = referralAging151PlusDays;
    entities.Worker.DaysToIwoPaymentAverage = daysToIwoPaymentAverage;
    entities.Worker.ReferralsToLegalForEst = referralsToLegalForEst;
    entities.Worker.ReferralsToLegalForEnf = referralsToLegalForEnf;
    entities.Worker.CasesOpened = casesOpened;
    entities.Worker.NcpLocatesByAddress = ncpLocatesByAddress;
    entities.Worker.NcpLocatesByEmployer = ncpLocatesByEmployer;
    entities.Worker.CaseClosures = caseClosures;
    entities.Worker.CaseReviews = caseReviews;
    entities.Worker.Populated = true;
  }

  private void UpdateDashboardPerformanceMetrics()
  {
    var asOfDate = import.JdDashboardPerformanceMetrics.AsOfDate;
    var casesUnderOrderNumerator =
      import.JdDashboardPerformanceMetrics.CasesUnderOrderNumerator.
        GetValueOrDefault();
    var casesUnderOrderDenominator =
      import.JdDashboardPerformanceMetrics.CasesUnderOrderDenominator.
        GetValueOrDefault();
    var casesUnderOrderPercent =
      import.JdDashboardPerformanceMetrics.CasesUnderOrderPercent.
        GetValueOrDefault();
    var casesUnderOrderRank =
      import.JdDashboardPerformanceMetrics.CasesUnderOrderRank.
        GetValueOrDefault();
    var pepNumerator =
      import.JdDashboardPerformanceMetrics.PepNumerator.GetValueOrDefault();
    var pepDenominator =
      import.JdDashboardPerformanceMetrics.PepDenominator.GetValueOrDefault();
    var pepPercent =
      import.JdDashboardPerformanceMetrics.PepPercent.GetValueOrDefault();
    var casesPayingArrearsNumerator =
      import.JdDashboardPerformanceMetrics.CasesPayingArrearsNumerator.
        GetValueOrDefault();
    var casesPayingArrearsDenominator =
      import.JdDashboardPerformanceMetrics.CasesPayingArrearsDenominator.
        GetValueOrDefault();
    var casesPayingArrearsPercent =
      import.JdDashboardPerformanceMetrics.CasesPayingArrearsPercent.
        GetValueOrDefault();
    var casesPayingArrearsRank =
      import.JdDashboardPerformanceMetrics.CasesPayingArrearsRank.
        GetValueOrDefault();
    var currentSupportPaidMthNum =
      import.JdDashboardPerformanceMetrics.CurrentSupportPaidMthNum.
        GetValueOrDefault();
    var currentSupportPaidMthDen =
      import.JdDashboardPerformanceMetrics.CurrentSupportPaidMthDen.
        GetValueOrDefault();
    var currentSupportPaidMthPer =
      import.JdDashboardPerformanceMetrics.CurrentSupportPaidMthPer.
        GetValueOrDefault();
    var currentSupportPaidMthRnk =
      import.JdDashboardPerformanceMetrics.CurrentSupportPaidMthRnk.
        GetValueOrDefault();
    var currentSupportPaidFfytdNum =
      import.JdDashboardPerformanceMetrics.CurrentSupportPaidFfytdNum.
        GetValueOrDefault();
    var currentSupportPaidFfytdDen =
      import.JdDashboardPerformanceMetrics.CurrentSupportPaidFfytdDen.
        GetValueOrDefault();
    var currentSupportPaidFfytdPer =
      import.JdDashboardPerformanceMetrics.CurrentSupportPaidFfytdPer.
        GetValueOrDefault();
    var currentSupportPaidFfytdRnk =
      import.JdDashboardPerformanceMetrics.CurrentSupportPaidFfytdRnk.
        GetValueOrDefault();
    var collectionsFfytdToPriorMonth =
      import.JdDashboardPerformanceMetrics.CollectionsFfytdToPriorMonth.
        GetValueOrDefault();
    var collectionsFfytdActual =
      import.JdDashboardPerformanceMetrics.CollectionsFfytdActual.
        GetValueOrDefault();
    var collectionsFfytdPriorYear =
      import.JdDashboardPerformanceMetrics.CollectionsFfytdPriorYear.
        GetValueOrDefault();
    var collectionsFfytdPercentChange =
      import.JdDashboardPerformanceMetrics.CollectionsFfytdPercentChange.
        GetValueOrDefault();
    var collectionsFfytdRnk =
      import.JdDashboardPerformanceMetrics.CollectionsFfytdRnk.
        GetValueOrDefault();
    var collectionsInMonthActual =
      import.JdDashboardPerformanceMetrics.CollectionsInMonthActual.
        GetValueOrDefault();
    var collectionsInMonthPriorYear =
      import.JdDashboardPerformanceMetrics.CollectionsInMonthPriorYear.
        GetValueOrDefault();
    var collectionsInMonthPercentChg =
      import.JdDashboardPerformanceMetrics.CollectionsInMonthPercentChg.
        GetValueOrDefault();
    var collectionsInMonthRnk =
      import.JdDashboardPerformanceMetrics.CollectionsInMonthRnk.
        GetValueOrDefault();
    var arrearsDistributedMonthActual =
      import.JdDashboardPerformanceMetrics.ArrearsDistributedMonthActual.
        GetValueOrDefault();
    var arrearsDistributedMonthRnk =
      import.JdDashboardPerformanceMetrics.ArrearsDistributedMonthRnk.
        GetValueOrDefault();
    var arrearsDistributedFfytdActual =
      import.JdDashboardPerformanceMetrics.ArrearsDistributedFfytdActual.
        GetValueOrDefault();
    var arrearsDistrubutedFfytdRnk =
      import.JdDashboardPerformanceMetrics.ArrearsDistrubutedFfytdRnk.
        GetValueOrDefault();
    var arrearsDueActual =
      import.JdDashboardPerformanceMetrics.ArrearsDueActual.GetValueOrDefault();
      
    var arrearsDueRnk =
      import.JdDashboardPerformanceMetrics.ArrearsDueRnk.GetValueOrDefault();
    var collectionsPerObligCaseNumer =
      import.JdDashboardPerformanceMetrics.CollectionsPerObligCaseNumer.
        GetValueOrDefault();
    var collectionsPerObligCaseDenom =
      import.JdDashboardPerformanceMetrics.CollectionsPerObligCaseDenom.
        GetValueOrDefault();
    var collectionsPerObligCaseAvg =
      import.JdDashboardPerformanceMetrics.CollectionsPerObligCaseAvg.
        GetValueOrDefault();
    var collectionsPerObligCaseRnk =
      import.JdDashboardPerformanceMetrics.CollectionsPerObligCaseRnk.
        GetValueOrDefault();
    var iwoPerObligCaseNumerator =
      import.JdDashboardPerformanceMetrics.IwoPerObligCaseNumerator.
        GetValueOrDefault();
    var iwoPerObligCaseDenominator =
      import.JdDashboardPerformanceMetrics.IwoPerObligCaseDenominator.
        GetValueOrDefault();
    var iwoPerObligCaseAverage =
      import.JdDashboardPerformanceMetrics.IwoPerObligCaseAverage.
        GetValueOrDefault();
    var iwoPerObligCaseRnk =
      import.JdDashboardPerformanceMetrics.IwoPerObligCaseRnk.
        GetValueOrDefault();
    var casesPerFteNumerator =
      import.JdDashboardPerformanceMetrics.CasesPerFteNumerator.
        GetValueOrDefault();
    var casesPerFteDenominator =
      import.JdDashboardPerformanceMetrics.CasesPerFteDenominator.
        GetValueOrDefault();
    var casesPerFteAverage =
      import.JdDashboardPerformanceMetrics.CasesPerFteAverage.
        GetValueOrDefault();
    var casesPerFteRank =
      import.JdDashboardPerformanceMetrics.CasesPerFteRank.GetValueOrDefault();
    var collectionsPerFteNumerator =
      import.JdDashboardPerformanceMetrics.CollectionsPerFteNumerator.
        GetValueOrDefault();
    var collectionsPerFteDenominator =
      import.JdDashboardPerformanceMetrics.CollectionsPerFteDenominator.
        GetValueOrDefault();
    var collectionsPerFteAverage =
      import.JdDashboardPerformanceMetrics.CollectionsPerFteAverage.
        GetValueOrDefault();
    var collectionsPerFteRank =
      import.JdDashboardPerformanceMetrics.CollectionsPerFteRank.
        GetValueOrDefault();
    var casesPayingNumerator =
      import.JdDashboardPerformanceMetrics.CasesPayingNumerator.
        GetValueOrDefault();
    var casesPayingDenominator =
      import.JdDashboardPerformanceMetrics.CasesPayingDenominator.
        GetValueOrDefault();
    var casesPayingPercent =
      import.JdDashboardPerformanceMetrics.CasesPayingPercent.
        GetValueOrDefault();
    var casesPayingRank =
      import.JdDashboardPerformanceMetrics.CasesPayingRank.GetValueOrDefault();
    var pepRank =
      import.JdDashboardPerformanceMetrics.PepRank.GetValueOrDefault();

    entities.JdDashboardPerformanceMetrics.Populated = false;
    Update("UpdateDashboardPerformanceMetrics",
      (db, command) =>
      {
        db.SetNullableDate(command, "asOfDt", asOfDate);
        db.
          SetNullableInt32(command, "casUnderOrdNum", casesUnderOrderNumerator);
          
        db.SetNullableInt32(
          command, "casUnderOrdDen", casesUnderOrderDenominator);
        db.
          SetNullableDecimal(command, "casUnderOrdPert", casesUnderOrderPercent);
          
        db.SetNullableInt32(command, "casUnderOrdRnk", casesUnderOrderRank);
        db.SetNullableInt32(command, "pepNum", pepNumerator);
        db.SetNullableInt32(command, "pepDen", pepDenominator);
        db.SetNullableDecimal(command, "pepPer", pepPercent);
        db.SetNullableInt32(
          command, "casPayingArrNum", casesPayingArrearsNumerator);
        db.SetNullableInt32(
          command, "casPayingArrDen", casesPayingArrearsDenominator);
        db.SetNullableDecimal(
          command, "casPayingArrPer", casesPayingArrearsPercent);
        db.SetNullableInt32(command, "casPayingArrRnk", casesPayingArrearsRank);
        db.SetNullableDecimal(
          command, "curSupPdMthNum", currentSupportPaidMthNum);
        db.SetNullableDecimal(
          command, "curSupPdMthDen", currentSupportPaidMthDen);
        db.SetNullableDecimal(
          command, "curSupPdMthPer", currentSupportPaidMthPer);
        db.
          SetNullableInt32(command, "curSupPdMthRnk", currentSupportPaidMthRnk);
          
        db.SetNullableDecimal(
          command, "curSupPdYtdNum", currentSupportPaidFfytdNum);
        db.SetNullableDecimal(
          command, "curSupPdYtdDen", currentSupportPaidFfytdDen);
        db.SetNullableDecimal(
          command, "curSupPdYtdPer", currentSupportPaidFfytdPer);
        db.SetNullableInt32(
          command, "curSupPdYtdRnk", currentSupportPaidFfytdRnk);
        db.SetNullableDecimal(
          command, "collYtdToPriMo", collectionsFfytdToPriorMonth);
        db.SetNullableDecimal(command, "collYtdAct", collectionsFfytdActual);
        db.
          SetNullableDecimal(command, "collYtdPriYr", collectionsFfytdPriorYear);
          
        db.SetNullableDecimal(
          command, "collYtdPerChg", collectionsFfytdPercentChange);
        db.SetNullableInt32(command, "collYtdRnk", collectionsFfytdRnk);
        db.
          SetNullableDecimal(command, "collInMthAct", collectionsInMonthActual);
          
        db.SetNullableDecimal(
          command, "collInMthPriYr", collectionsInMonthPriorYear);
        db.SetNullableDecimal(
          command, "collInMthPerCh", collectionsInMonthPercentChg);
        db.SetNullableInt32(command, "collInMthRnk", collectionsInMonthRnk);
        db.SetNullableDecimal(
          command, "arrDistMthAct", arrearsDistributedMonthActual);
        db.
          SetNullableInt32(command, "arrDistMthRnk", arrearsDistributedMonthRnk);
          
        db.SetNullableDecimal(
          command, "arrDistYtdAct", arrearsDistributedFfytdActual);
        db.
          SetNullableInt32(command, "arrDistYtdRnk", arrearsDistrubutedFfytdRnk);
          
        db.SetNullableDecimal(command, "arrDueAct", arrearsDueActual);
        db.SetNullableInt32(command, "arrDueRnk", arrearsDueRnk);
        db.SetNullableDecimal(
          command, "collOblCasNum", collectionsPerObligCaseNumer);
        db.SetNullableDecimal(
          command, "collOblCasDen", collectionsPerObligCaseDenom);
        db.SetNullableDecimal(
          command, "collOblCasAvg", collectionsPerObligCaseAvg);
        db.
          SetNullableInt32(command, "collOblCasRnk", collectionsPerObligCaseRnk);
          
        db.SetNullableInt32(command, "iwoOblCasNum", iwoPerObligCaseNumerator);
        db.
          SetNullableInt32(command, "iwoOblCasDen", iwoPerObligCaseDenominator);
          
        db.SetNullableDecimal(command, "iwoOblCasAvg", iwoPerObligCaseAverage);
        db.SetNullableInt32(command, "iwoOblCasRnk", iwoPerObligCaseRnk);
        db.SetNullableInt32(command, "casPerFteNum", casesPerFteNumerator);
        db.SetNullableDecimal(command, "casPerFteDen", casesPerFteDenominator);
        db.SetNullableDecimal(command, "casPerFteAvg", casesPerFteAverage);
        db.SetNullableInt32(command, "casPerFteRnk", casesPerFteRank);
        db.SetNullableDecimal(
          command, "collPerFteNum", collectionsPerFteNumerator);
        db.SetNullableDecimal(
          command, "collPerFteDen", collectionsPerFteDenominator);
        db.
          SetNullableDecimal(command, "collPerFteAvg", collectionsPerFteAverage);
          
        db.SetNullableInt32(command, "collPerFteRnk", collectionsPerFteRank);
        db.SetNullableInt32(command, "casPayingNum", casesPayingNumerator);
        db.SetNullableInt32(command, "casPayingDen", casesPayingDenominator);
        db.SetNullableDecimal(command, "casPayingPer", casesPayingPercent);
        db.SetNullableInt32(command, "casPayingRnk", casesPayingRank);
        db.SetNullableInt32(command, "pepRank", pepRank);
        db.SetInt32(
          command, "reportMonth",
          entities.JdDashboardPerformanceMetrics.ReportMonth);
        db.SetString(
          command, "reportLevel",
          entities.JdDashboardPerformanceMetrics.ReportLevel);
        db.SetString(
          command, "reportLevelId",
          entities.JdDashboardPerformanceMetrics.ReportLevelId);
        db.SetString(
          command, "type", entities.JdDashboardPerformanceMetrics.Type1);
      });

    entities.JdDashboardPerformanceMetrics.AsOfDate = asOfDate;
    entities.JdDashboardPerformanceMetrics.CasesUnderOrderNumerator =
      casesUnderOrderNumerator;
    entities.JdDashboardPerformanceMetrics.CasesUnderOrderDenominator =
      casesUnderOrderDenominator;
    entities.JdDashboardPerformanceMetrics.CasesUnderOrderPercent =
      casesUnderOrderPercent;
    entities.JdDashboardPerformanceMetrics.CasesUnderOrderRank =
      casesUnderOrderRank;
    entities.JdDashboardPerformanceMetrics.PepNumerator = pepNumerator;
    entities.JdDashboardPerformanceMetrics.PepDenominator = pepDenominator;
    entities.JdDashboardPerformanceMetrics.PepPercent = pepPercent;
    entities.JdDashboardPerformanceMetrics.CasesPayingArrearsNumerator =
      casesPayingArrearsNumerator;
    entities.JdDashboardPerformanceMetrics.CasesPayingArrearsDenominator =
      casesPayingArrearsDenominator;
    entities.JdDashboardPerformanceMetrics.CasesPayingArrearsPercent =
      casesPayingArrearsPercent;
    entities.JdDashboardPerformanceMetrics.CasesPayingArrearsRank =
      casesPayingArrearsRank;
    entities.JdDashboardPerformanceMetrics.CurrentSupportPaidMthNum =
      currentSupportPaidMthNum;
    entities.JdDashboardPerformanceMetrics.CurrentSupportPaidMthDen =
      currentSupportPaidMthDen;
    entities.JdDashboardPerformanceMetrics.CurrentSupportPaidMthPer =
      currentSupportPaidMthPer;
    entities.JdDashboardPerformanceMetrics.CurrentSupportPaidMthRnk =
      currentSupportPaidMthRnk;
    entities.JdDashboardPerformanceMetrics.CurrentSupportPaidFfytdNum =
      currentSupportPaidFfytdNum;
    entities.JdDashboardPerformanceMetrics.CurrentSupportPaidFfytdDen =
      currentSupportPaidFfytdDen;
    entities.JdDashboardPerformanceMetrics.CurrentSupportPaidFfytdPer =
      currentSupportPaidFfytdPer;
    entities.JdDashboardPerformanceMetrics.CurrentSupportPaidFfytdRnk =
      currentSupportPaidFfytdRnk;
    entities.JdDashboardPerformanceMetrics.CollectionsFfytdToPriorMonth =
      collectionsFfytdToPriorMonth;
    entities.JdDashboardPerformanceMetrics.CollectionsFfytdActual =
      collectionsFfytdActual;
    entities.JdDashboardPerformanceMetrics.CollectionsFfytdPriorYear =
      collectionsFfytdPriorYear;
    entities.JdDashboardPerformanceMetrics.CollectionsFfytdPercentChange =
      collectionsFfytdPercentChange;
    entities.JdDashboardPerformanceMetrics.CollectionsFfytdRnk =
      collectionsFfytdRnk;
    entities.JdDashboardPerformanceMetrics.CollectionsInMonthActual =
      collectionsInMonthActual;
    entities.JdDashboardPerformanceMetrics.CollectionsInMonthPriorYear =
      collectionsInMonthPriorYear;
    entities.JdDashboardPerformanceMetrics.CollectionsInMonthPercentChg =
      collectionsInMonthPercentChg;
    entities.JdDashboardPerformanceMetrics.CollectionsInMonthRnk =
      collectionsInMonthRnk;
    entities.JdDashboardPerformanceMetrics.ArrearsDistributedMonthActual =
      arrearsDistributedMonthActual;
    entities.JdDashboardPerformanceMetrics.ArrearsDistributedMonthRnk =
      arrearsDistributedMonthRnk;
    entities.JdDashboardPerformanceMetrics.ArrearsDistributedFfytdActual =
      arrearsDistributedFfytdActual;
    entities.JdDashboardPerformanceMetrics.ArrearsDistrubutedFfytdRnk =
      arrearsDistrubutedFfytdRnk;
    entities.JdDashboardPerformanceMetrics.ArrearsDueActual = arrearsDueActual;
    entities.JdDashboardPerformanceMetrics.ArrearsDueRnk = arrearsDueRnk;
    entities.JdDashboardPerformanceMetrics.CollectionsPerObligCaseNumer =
      collectionsPerObligCaseNumer;
    entities.JdDashboardPerformanceMetrics.CollectionsPerObligCaseDenom =
      collectionsPerObligCaseDenom;
    entities.JdDashboardPerformanceMetrics.CollectionsPerObligCaseAvg =
      collectionsPerObligCaseAvg;
    entities.JdDashboardPerformanceMetrics.CollectionsPerObligCaseRnk =
      collectionsPerObligCaseRnk;
    entities.JdDashboardPerformanceMetrics.IwoPerObligCaseNumerator =
      iwoPerObligCaseNumerator;
    entities.JdDashboardPerformanceMetrics.IwoPerObligCaseDenominator =
      iwoPerObligCaseDenominator;
    entities.JdDashboardPerformanceMetrics.IwoPerObligCaseAverage =
      iwoPerObligCaseAverage;
    entities.JdDashboardPerformanceMetrics.IwoPerObligCaseRnk =
      iwoPerObligCaseRnk;
    entities.JdDashboardPerformanceMetrics.CasesPerFteNumerator =
      casesPerFteNumerator;
    entities.JdDashboardPerformanceMetrics.CasesPerFteDenominator =
      casesPerFteDenominator;
    entities.JdDashboardPerformanceMetrics.CasesPerFteAverage =
      casesPerFteAverage;
    entities.JdDashboardPerformanceMetrics.CasesPerFteRank = casesPerFteRank;
    entities.JdDashboardPerformanceMetrics.CollectionsPerFteNumerator =
      collectionsPerFteNumerator;
    entities.JdDashboardPerformanceMetrics.CollectionsPerFteDenominator =
      collectionsPerFteDenominator;
    entities.JdDashboardPerformanceMetrics.CollectionsPerFteAverage =
      collectionsPerFteAverage;
    entities.JdDashboardPerformanceMetrics.CollectionsPerFteRank =
      collectionsPerFteRank;
    entities.JdDashboardPerformanceMetrics.CasesPayingNumerator =
      casesPayingNumerator;
    entities.JdDashboardPerformanceMetrics.CasesPayingDenominator =
      casesPayingDenominator;
    entities.JdDashboardPerformanceMetrics.CasesPayingPercent =
      casesPayingPercent;
    entities.JdDashboardPerformanceMetrics.CasesPayingRank = casesPayingRank;
    entities.JdDashboardPerformanceMetrics.PepRank = pepRank;
    entities.JdDashboardPerformanceMetrics.Populated = true;
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Entities entities = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
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
    /// A value of JdDashboardOutputMetrics.
    /// </summary>
    [JsonPropertyName("jdDashboardOutputMetrics")]
    public DashboardOutputMetrics JdDashboardOutputMetrics
    {
      get => jdDashboardOutputMetrics ??= new();
      set => jdDashboardOutputMetrics = value;
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

    private DashboardPerformanceMetrics jdDashboardPerformanceMetrics;
    private DashboardOutputMetrics jdDashboardOutputMetrics;
    private DashboardOutputMetrics worker;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of JdDashboardOutputMetrics.
    /// </summary>
    [JsonPropertyName("jdDashboardOutputMetrics")]
    public DashboardOutputMetrics JdDashboardOutputMetrics
    {
      get => jdDashboardOutputMetrics ??= new();
      set => jdDashboardOutputMetrics = value;
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

    private DashboardPerformanceMetrics jdDashboardPerformanceMetrics;
    private DashboardOutputMetrics jdDashboardOutputMetrics;
    private DashboardOutputMetrics worker;
  }
#endregion
}
