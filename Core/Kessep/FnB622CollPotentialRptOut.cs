// Program: FN_B622_COLL_POTENTIAL_RPT_OUT, ID: 373457479, model: 746.
// Short name: SWEF622B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B622_COLL_POTENTIAL_RPT_OUT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB622CollPotentialRptOut: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B622_COLL_POTENTIAL_RPT_OUT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB622CollPotentialRptOut(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB622CollPotentialRptOut.
  /// </summary>
  public FnB622CollPotentialRptOut(IContext context, Import import,
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
    ExitState = "ACO_NN0000_ALL_OK";
    local.PageNumber.Count = 0;
    local.EabFileHandling.Action = "OPEN";
    local.NeededToOpen.ProgramName = local.ProgramProcessingInfo.Name;
    local.NeededToOpen.ProcessDate = local.Process.Date;
    local.NeededToOpen.ProgramName = global.UserId;
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.ProgramProcessingInfo.Name = "SWEFB622";
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error reading PPI table.";
      UseCabErrorReport3();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // The following logic sets the local current date work area date to either:
    // 1. If the processing info date is blank, set the BOM (Begining of Month) 
    // DATE
    // to the system current date month - 1 month and set day to the 1st, set 
    // EOM to last
    // day of the reporting month.
    // 2. If the processing info date is max date (2099-12-31), same as above
    // 3. Otherwise, use the program processing info date to calculate the BOM 
    // and EOM.
    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    if (Equal(local.ProgramProcessingInfo.ProcessDate, local.Initialized.Date))
    {
      local.Process.Date = Now().Date;
    }
    else if (Equal(local.ProgramProcessingInfo.ProcessDate, local.Max.Date))
    {
      local.Process.Date = Now().Date;
    }
    else
    {
      local.Process.Date = local.ProgramProcessingInfo.ProcessDate;
    }

    UseCabDetermineReportingDates();
    local.TextMm.Text2 = NumberToString(Month(local.Eom.Date), 2);
    local.TextDd.Text2 = NumberToString(Day(local.Eom.Date), 2);
    local.TextYyyy.Text4 = NumberToString(Year(local.Eom.Date), 4);
    local.TextDate.Text10 = local.TextMm.Text2 + "/" + local.TextDd.Text2 + "/"
      + local.TextYyyy.Text4;

    // : The report is for the month prior to the processing date, so subtract 1
    //   from the month.
    local.TextMm.Text2 =
      NumberToString(Month(AddMonths(local.Process.Date, -1)), 2);

    switch(TrimEnd(local.TextMm.Text2))
    {
      case "01":
        local.MonthName.Text9 = "January";

        break;
      case "02":
        local.MonthName.Text9 = "February";

        break;
      case "03":
        local.MonthName.Text9 = "March";

        break;
      case "04":
        local.MonthName.Text9 = "April";

        break;
      case "05":
        local.MonthName.Text9 = "May";

        break;
      case "06":
        local.MonthName.Text9 = "June";

        break;
      case "07":
        local.MonthName.Text9 = "July";

        break;
      case "08":
        local.MonthName.Text9 = "August";

        break;
      case "09":
        local.MonthName.Text9 = "September";

        break;
      case "10":
        local.MonthName.Text9 = "October";

        break;
      case "11":
        local.MonthName.Text9 = "November";

        break;
      case "12":
        local.MonthName.Text9 = "December";

        break;
      default:
        break;
    }

    local.EabFileHandling.Action = "OPEN";
    UseFnEabSwexfr17CollPotRead2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error opening input file.";
      UseCabErrorReport3();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabFileHandling.Action = "OPEN";
    UseFnB622ExternalToWriteReport();
    local.EabFileHandling.Action = "READ";

    while(Equal(local.EabFileHandling.Status, "OK"))
    {
      // : The external reads a group of  records (3 at most) with the same 
      // tribunal, jd, and county,
      //  and totals them, passing back a group view of 4 occurrences.
      UseFnEabSwexfr17CollPotRead1();

      switch(TrimEnd(local.EabFileHandling.Status))
      {
        case "OK":
          // : We will take fips county and state descriptions from occurrence 1
          // of the
          //   group view passed back from the read external.
          local.CountyLevel.Index = 0;
          local.CountyLevel.CheckSize();

          local.Fips.Assign(local.CountyLevel.Item.Fips);
          MoveTribunal(local.CountyLevel.Item.Tribunal, local.Tribunal);

          break;
        case "EF":
          // : End file, but must process last group of records.
          // : We will take fips county and state descriptions from occurrence 1
          // of the
          //   group view passed back from the read external.
          local.CountyLevel.Index = 0;
          local.CountyLevel.CheckSize();

          local.Fips.Assign(local.CountyLevel.Item.Fips);
          MoveTribunal(local.CountyLevel.Item.Tribunal, local.Tribunal);
          local.EabFileHandling.Status = "OK";
          local.EabFileHandling.Action = "LAST";

          break;
        case "DONE":
          // : Everything has been processed, and the file closed.
          goto AfterCycle;
        default:
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error reading input file.  File status is: " + local
            .EabFileHandling.Status;
          UseCabErrorReport3();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
      }

      // : Check for Control Break.
      if (Equal(local.Fips.StateAbbreviation, "YY") || Equal
        (local.Fips.StateAbbreviation, "ZZ"))
      {
      }
      else if (Equal(local.Fips.StateAbbreviation,
        local.SaveFips.StateAbbreviation))
      {
        if (Equal(local.Tribunal.JudicialDistrict,
          local.SaveTribunal.JudicialDistrict))
        {
        }
        else
        {
          // : Control break on Judicial District.
          ++local.PageNumber.Count;
          local.ReportLevel.Text5 = "JD";
          UseFnB622CreateReport2();
          local.SaveFips.Assign(local.Fips);
          MoveTribunal(local.Tribunal, local.SaveTribunal);
        }
      }
      else
      {
        // : Control break on State.
        // : If saved State is spaces, this is first time through the loop.
        if (local.SaveFips.State == 0)
        {
          local.SaveFips.Assign(local.Fips);
          local.SaveTribunal.JudicialDistrict = local.Tribunal.JudicialDistrict;
        }
        else
        {
          // : Control Break on State and Judicial District.
          ++local.PageNumber.Count;
          local.ReportLevel.Text5 = "JD";
          UseFnB622CreateReport2();
          ++local.PageNumber.Count;
          local.ReportLevel.Text5 = "STATE";
          UseFnB622CreateReport3();
          local.SaveFips.Assign(local.Fips);
          MoveTribunal(local.Tribunal, local.SaveTribunal);
        }
      }

      ++local.PageNumber.Count;
      local.ReportLevel.Text5 = "CNTY";
      UseFnB622CreateReport1();

      if (Equal(local.Fips.StateAbbreviation, "XX") || Equal
        (local.Fips.StateAbbreviation, "YY") || Equal
        (local.Fips.StateAbbreviation, "ZZ"))
      {
        // If state is set to XX, YY or ZZ, the report is tribal, out of state, 
        // or out of country.  We do not need to roll totals for these, as there
        // is only 1 set of records for each of these (only 1 report is
        // produced, as opposed to several for the state of kansas)..
      }
      else
      {
        UseFnB622RollUpTotals1();
        UseFnB622RollUpTotals2();
      }
    }

AfterCycle:

    local.EabFileHandling.Action = "CLOSE";
    UseFnB622ExternalToWriteReport();
    UseCabErrorReport1();
  }

  private static void MoveCountyLevelToGroup(Local.CountyLevelGroup source,
    FnEabSwexfr17CollPotRead.Export.GroupGroup target)
  {
    target.EtotalOrdersReferred.Count = source.LtotalOrdersReferred.Count;
    target.EnumberOfPayingOrders.Count = source.LnumberOfPayingOrders.Count;
    target.EordersInLocate.Count = source.LnumberOfOrdersInLocate.Count;
    target.Fips.Assign(source.Fips);
    MoveTribunal(source.Tribunal, target.Tribunal);
    target.LegalAction.StandardNumber = source.LegalAction.StandardNumber;
    target.EreferralType.Text2 = source.TextWorkArea.Text2;
    target.EcollCurrentState.TotalCurrency =
      source.LcollCurrentState.TotalCurrency;
    target.EcollCurrentFamily.TotalCurrency =
      source.LcollCurrentFamily.TotalCurrency;
    target.EcollCurrentIState.TotalCurrency =
      source.LcollCurrentIState.TotalCurrency;
    target.EcollCurrentIFamily.TotalCurrency =
      source.LcollCurrentIFamily.TotalCurrency;
    target.EcollArrearsState.TotalCurrency =
      source.LcollArrearsState.TotalCurrency;
    target.EcollArrearsFamily.TotalCurrency =
      source.LcollArrearsFamily.TotalCurrency;
    target.EcollArrearsIState.TotalCurrency =
      source.LcollArrearsIState.TotalCurrency;
    target.EcollArrearsIFamily.TotalCurrency =
      source.LcollArrearsIFamily.TotalCurrency;
    target.EowedCurrentState.TotalCurrency =
      source.LowedCurrentState.TotalCurrency;
    target.EowedCurrentFamily.TotalCurrency =
      source.LowedCurrentFamily.TotalCurrency;
    target.EowedCurrentIState.TotalCurrency =
      source.LowedCurrentIState.TotalCurrency;
    target.EowedCurrentIFamily.TotalCurrency =
      source.LowedCurrentIFamily.TotalCurrency;
    target.EowedArrearsState.TotalCurrency =
      source.LowedArrearsState.TotalCurrency;
    target.EowedArrearsFamily.TotalCurrency =
      source.LowedArrearsFamily.TotalCurrency;
    target.EowedArrearsIState.TotalCurrency =
      source.LowedArrearsIState.TotalCurrency;
    target.EowedArrearsIFamily.TotalCurrency =
      source.LowedArrearsIFamily.TotalCurrency;
    target.EowedCurrTotal.TotalCurrency = source.LowedCurrTotal.TotalCurrency;
    target.EowedArrearsTotal.TotalCurrency =
      source.LowedArrearsTotal.TotalCurrency;
    target.EowedTotal.TotalCurrency = source.LowedTotal.TotalCurrency;
    target.EcollCurrTotal.TotalCurrency = source.LcollCurrTotal.TotalCurrency;
    target.EcollArrearsTotal.TotalCurrency =
      source.LcollArrearsTotal.TotalCurrency;
    target.EcollectedTotal.TotalCurrency = source.LcollectedTotal.TotalCurrency;
  }

  private static void MoveCountyLevelToGroup1(Local.CountyLevelGroup source,
    FnB622RollUpTotals.Import.Group1Group target)
  {
    target.ItotalOrdersReferred.Count = source.LtotalOrdersReferred.Count;
    target.InumberOfPayingOrders.Count = source.LnumberOfPayingOrders.Count;
    target.IordersInLocate.Count = source.LnumberOfOrdersInLocate.Count;
    target.Ifips.Assign(source.Fips);
    MoveTribunal(source.Tribunal, target.Itribunal);
    target.IlegalAction.StandardNumber = source.LegalAction.StandardNumber;
    target.ItextWorkArea.Text2 = source.TextWorkArea.Text2;
    target.IcollCurrentState.TotalCurrency =
      source.LcollCurrentState.TotalCurrency;
    target.IcollCurrentFamily.TotalCurrency =
      source.LcollCurrentFamily.TotalCurrency;
    target.IcollCurrentIState.TotalCurrency =
      source.LcollCurrentIState.TotalCurrency;
    target.IcollCurrentIFamily.TotalCurrency =
      source.LcollCurrentIFamily.TotalCurrency;
    target.IcollArrearsState.TotalCurrency =
      source.LcollArrearsState.TotalCurrency;
    target.IcollArrearsFamily.TotalCurrency =
      source.LcollArrearsFamily.TotalCurrency;
    target.IcollArrearsIState.TotalCurrency =
      source.LcollArrearsIState.TotalCurrency;
    target.IcollArrearsIFamily.TotalCurrency =
      source.LcollArrearsIFamily.TotalCurrency;
    target.IowedCurrentState.TotalCurrency =
      source.LowedCurrentState.TotalCurrency;
    target.IowedCurrentFamily.TotalCurrency =
      source.LowedCurrentFamily.TotalCurrency;
    target.IowedCurrentIState.TotalCurrency =
      source.LowedCurrentIState.TotalCurrency;
    target.IowedCurrentIFamily.TotalCurrency =
      source.LowedCurrentIFamily.TotalCurrency;
    target.IowedArrearsState.TotalCurrency =
      source.LowedArrearsState.TotalCurrency;
    target.IowedArrearsFamily.TotalCurrency =
      source.LowedArrearsFamily.TotalCurrency;
    target.IowedArrearsIState.TotalCurrency =
      source.LowedArrearsIState.TotalCurrency;
    target.IowedArrearsIFamily.TotalCurrency =
      source.LowedArrearsIFamily.TotalCurrency;
    target.IowedCurrTotal.TotalCurrency = source.LowedCurrTotal.TotalCurrency;
    target.IowedArrearsTotal.TotalCurrency =
      source.LowedArrearsTotal.TotalCurrency;
    target.IowedTotal.TotalCurrency = source.LowedTotal.TotalCurrency;
    target.IcollCurrTotal.TotalCurrency = source.LcollCurrTotal.TotalCurrency;
    target.IcollArrearsTotal.TotalCurrency =
      source.LcollArrearsTotal.TotalCurrency;
    target.IcollectedTotal.TotalCurrency = source.LcollectedTotal.TotalCurrency;
  }

  private static void MoveCountyLevelToReferralType(Local.
    CountyLevelGroup source,
    FnB622CreateReport.Import.ReferralTypeGroup target)
  {
    target.ItotalOrdersReferred.Count = source.LtotalOrdersReferred.Count;
    target.InumberOfPayingOrders.Count = source.LnumberOfPayingOrders.Count;
    target.InumberOfOrdersInLocate.Count = source.LnumberOfOrdersInLocate.Count;
    target.Ifips.Assign(source.Fips);
    MoveTribunal(source.Tribunal, target.Itribunal);
    target.IlegalAction.StandardNumber = source.LegalAction.StandardNumber;
    target.IreferralType.Text2 = source.TextWorkArea.Text2;
    target.IcollCurrentState.TotalCurrency =
      source.LcollCurrentState.TotalCurrency;
    target.IcollCurrentFamily.TotalCurrency =
      source.LcollCurrentFamily.TotalCurrency;
    target.IcollCurrentIState.TotalCurrency =
      source.LcollCurrentIState.TotalCurrency;
    target.IcollCurrentIFamily.TotalCurrency =
      source.LcollCurrentIFamily.TotalCurrency;
    target.IcollArrearsState.TotalCurrency =
      source.LcollArrearsState.TotalCurrency;
    target.IcollArrearsFamily.TotalCurrency =
      source.LcollArrearsFamily.TotalCurrency;
    target.IcollArrearsIState.TotalCurrency =
      source.LcollArrearsIState.TotalCurrency;
    target.IcollArrearsIFamily.TotalCurrency =
      source.LcollArrearsIFamily.TotalCurrency;
    target.IowedCurrentState.TotalCurrency =
      source.LowedCurrentState.TotalCurrency;
    target.IowedCurrentFamily.TotalCurrency =
      source.LowedCurrentFamily.TotalCurrency;
    target.IowedCurrentIState.TotalCurrency =
      source.LowedCurrentIState.TotalCurrency;
    target.IowedCurrentIFamily.TotalCurrency =
      source.LowedCurrentIFamily.TotalCurrency;
    target.IowedArrearsState.TotalCurrency =
      source.LowedArrearsState.TotalCurrency;
    target.IowedArrearsFamily.TotalCurrency =
      source.LowedArrearsFamily.TotalCurrency;
    target.IowedArrearsIState.TotalCurrency =
      source.LowedArrearsIState.TotalCurrency;
    target.IowedArrearsIFamily.TotalCurrency =
      source.LowedArrearsIFamily.TotalCurrency;
    target.IowedCurrTotal.TotalCurrency = source.LowedCurrTotal.TotalCurrency;
    target.IowedArrearsTotal.TotalCurrency =
      source.LowedArrearsTotal.TotalCurrency;
    target.IowedTotal.TotalCurrency = source.LowedTotal.TotalCurrency;
    target.IcollCurrTotal.TotalCurrency = source.LcollCurrTotal.TotalCurrency;
    target.IcollArrearsTotal.TotalCurrency =
      source.LcollArrearsTotal.TotalCurrency;
    target.IcollectedTotal.TotalCurrency = source.LcollectedTotal.TotalCurrency;
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveGroupToCountyLevel(FnEabSwexfr17CollPotRead.Export.
    GroupGroup source, Local.CountyLevelGroup target)
  {
    target.LtotalOrdersReferred.Count = source.EtotalOrdersReferred.Count;
    target.LnumberOfPayingOrders.Count = source.EnumberOfPayingOrders.Count;
    target.LnumberOfOrdersInLocate.Count = source.EordersInLocate.Count;
    target.Fips.Assign(source.Fips);
    MoveTribunal(source.Tribunal, target.Tribunal);
    target.LegalAction.StandardNumber = source.LegalAction.StandardNumber;
    target.TextWorkArea.Text2 = source.EreferralType.Text2;
    target.LcollCurrentState.TotalCurrency =
      source.EcollCurrentState.TotalCurrency;
    target.LcollCurrentFamily.TotalCurrency =
      source.EcollCurrentFamily.TotalCurrency;
    target.LcollCurrentIState.TotalCurrency =
      source.EcollCurrentIState.TotalCurrency;
    target.LcollCurrentIFamily.TotalCurrency =
      source.EcollCurrentIFamily.TotalCurrency;
    target.LcollArrearsState.TotalCurrency =
      source.EcollArrearsState.TotalCurrency;
    target.LcollArrearsFamily.TotalCurrency =
      source.EcollArrearsFamily.TotalCurrency;
    target.LcollArrearsIState.TotalCurrency =
      source.EcollArrearsIState.TotalCurrency;
    target.LcollArrearsIFamily.TotalCurrency =
      source.EcollArrearsIFamily.TotalCurrency;
    target.LowedCurrentState.TotalCurrency =
      source.EowedCurrentState.TotalCurrency;
    target.LowedCurrentFamily.TotalCurrency =
      source.EowedCurrentFamily.TotalCurrency;
    target.LowedCurrentIState.TotalCurrency =
      source.EowedCurrentIState.TotalCurrency;
    target.LowedCurrentIFamily.TotalCurrency =
      source.EowedCurrentIFamily.TotalCurrency;
    target.LowedArrearsState.TotalCurrency =
      source.EowedArrearsState.TotalCurrency;
    target.LowedArrearsFamily.TotalCurrency =
      source.EowedArrearsFamily.TotalCurrency;
    target.LowedArrearsIState.TotalCurrency =
      source.EowedArrearsIState.TotalCurrency;
    target.LowedArrearsIFamily.TotalCurrency =
      source.EowedArrearsIFamily.TotalCurrency;
    target.LowedCurrTotal.TotalCurrency = source.EowedCurrTotal.TotalCurrency;
    target.LowedArrearsTotal.TotalCurrency =
      source.EowedArrearsTotal.TotalCurrency;
    target.LowedTotal.TotalCurrency = source.EowedTotal.TotalCurrency;
    target.LcollCurrTotal.TotalCurrency = source.EcollCurrTotal.TotalCurrency;
    target.LcollArrearsTotal.TotalCurrency =
      source.EcollArrearsTotal.TotalCurrency;
    target.LcollectedTotal.TotalCurrency = source.EcollectedTotal.TotalCurrency;
  }

  private static void MoveGroupToJdLevel(FnB622RollUpTotals.Export.
    GroupGroup source, Local.JdLevelGroup target)
  {
    target.LjdTotalOrdersReferred.Count = source.EtotalOrdersReferred.Count;
    target.LjdNumberOfPayingOrders.Count = source.EnumberOfPayingOrders.Count;
    target.LjdNumberOfOrdersInLocate.Count = source.EordersInLocate.Count;
    target.LjdFips.Assign(source.Fips);
    MoveTribunal(source.Tribunal, target.LjdTribunal);
    target.LjdLegalAction.StandardNumber = source.LegalAction.StandardNumber;
    target.LjdReferralType.Text2 = source.EreferralType.Text2;
    target.LjdCollCurrentState.TotalCurrency =
      source.EcollCurrentState.TotalCurrency;
    target.LjdCollCurrentFamily.TotalCurrency =
      source.EcollCurrentFamily.TotalCurrency;
    target.LjdCollCurrentIState.TotalCurrency =
      source.EcollCurrentIState.TotalCurrency;
    target.LjdCollCurrentIFamily.TotalCurrency =
      source.EcollCurrentIFamily.TotalCurrency;
    target.LjdCollArrearsState.TotalCurrency =
      source.EcollArrearsState.TotalCurrency;
    target.LjdCollArrearsFamily.TotalCurrency =
      source.EcollArrearsFamily.TotalCurrency;
    target.LjdCollArrearsIState.TotalCurrency =
      source.EcollArrearsIState.TotalCurrency;
    target.LjdCollArrearsIFamily.TotalCurrency =
      source.EcollArrearsIFamily.TotalCurrency;
    target.LjdOwedCurrentState.TotalCurrency =
      source.EowedCurrentState.TotalCurrency;
    target.LjdOwedCurrentFamily.TotalCurrency =
      source.EowedCurrentFamily.TotalCurrency;
    target.LjdOwedCurrentIState.TotalCurrency =
      source.EowedCurrentIState.TotalCurrency;
    target.LjdOwedCurrentIFamily.TotalCurrency =
      source.EowedCurrentIFamily.TotalCurrency;
    target.LjdOwedArrearsState.TotalCurrency =
      source.EowedArrearsState.TotalCurrency;
    target.LjdOwedArrearsFamily.TotalCurrency =
      source.EowedArrearsFamily.TotalCurrency;
    target.LjdOwedArrearsIState.TotalCurrency =
      source.EowedArrearsIState.TotalCurrency;
    target.LjdOwedArrearsIFamily.TotalCurrency =
      source.EowedArrearsIFamily.TotalCurrency;
    target.LjdOwedCurrTotal.TotalCurrency = source.EowedCurrTotal.TotalCurrency;
    target.LjdOwedArrearsTotal.TotalCurrency =
      source.EowedArrearsTotal.TotalCurrency;
    target.LjdOwedTotal.TotalCurrency = source.EowedTotal.TotalCurrency;
    target.LjdCollCurrTotal.TotalCurrency = source.EcollCurrTotal.TotalCurrency;
    target.LjdCollArrearsTotal.TotalCurrency =
      source.EcollArrearsTotal.TotalCurrency;
    target.LjdCollectedTotal.TotalCurrency =
      source.EcollectedTotal.TotalCurrency;
  }

  private static void MoveGroupToStateLevel(FnB622RollUpTotals.Export.
    GroupGroup source, Local.StateLevelGroup target)
  {
    target.LstTotalOrdersReferred.Count = source.EtotalOrdersReferred.Count;
    target.LstNumberOfPayingOrders.Count = source.EnumberOfPayingOrders.Count;
    target.LstNumberOfOrdersInLocate.Count = source.EordersInLocate.Count;
    target.LstFips.Assign(source.Fips);
    MoveTribunal(source.Tribunal, target.LstTribunal);
    target.LstLegalAction.StandardNumber = source.LegalAction.StandardNumber;
    target.LstReferralType.Text2 = source.EreferralType.Text2;
    target.LstCollCurrentState.TotalCurrency =
      source.EcollCurrentState.TotalCurrency;
    target.LstCollCurrentFamily.TotalCurrency =
      source.EcollCurrentFamily.TotalCurrency;
    target.LstCollCurrentIState.TotalCurrency =
      source.EcollCurrentIState.TotalCurrency;
    target.LstCollCurrentIFamily.TotalCurrency =
      source.EcollCurrentIFamily.TotalCurrency;
    target.LstCollArrearsState.TotalCurrency =
      source.EcollArrearsState.TotalCurrency;
    target.LstCollArrearsFamily.TotalCurrency =
      source.EcollArrearsFamily.TotalCurrency;
    target.LstCollArrearsIState.TotalCurrency =
      source.EcollArrearsIState.TotalCurrency;
    target.LstCollArrearsIFamily.TotalCurrency =
      source.EcollArrearsIFamily.TotalCurrency;
    target.LstOwedCurrentState.TotalCurrency =
      source.EowedCurrentState.TotalCurrency;
    target.LstOwedCurrentFamily.TotalCurrency =
      source.EowedCurrentFamily.TotalCurrency;
    target.LstOwedCurrentIState.TotalCurrency =
      source.EowedCurrentIState.TotalCurrency;
    target.LstOwedCurrentIFamily.TotalCurrency =
      source.EowedCurrentIFamily.TotalCurrency;
    target.LstOwedArrearsState.TotalCurrency =
      source.EowedArrearsState.TotalCurrency;
    target.LstOwedArrearsFamily.TotalCurrency =
      source.EowedArrearsFamily.TotalCurrency;
    target.LstOwedArrearsIState.TotalCurrency =
      source.EowedArrearsIState.TotalCurrency;
    target.LstOwedArrearsIFamily.TotalCurrency =
      source.EowedArrearsIFamily.TotalCurrency;
    target.LstOwedCurrTotal.TotalCurrency = source.EowedCurrTotal.TotalCurrency;
    target.LstOwedArrearsTotal.TotalCurrency =
      source.EowedArrearsTotal.TotalCurrency;
    target.LstOwedTotal.TotalCurrency = source.EowedTotal.TotalCurrency;
    target.LstCollCurrTotal.TotalCurrency = source.EcollCurrTotal.TotalCurrency;
    target.LstCollArrearsTotal.TotalCurrency =
      source.EcollArrearsTotal.TotalCurrency;
    target.LstCollectedTotal.TotalCurrency =
      source.EcollectedTotal.TotalCurrency;
  }

  private static void MoveJdLevelToGroup2(Local.JdLevelGroup source,
    FnB622RollUpTotals.Import.Group2Group target)
  {
    target.I2otalOrdersReferred.Count = source.LjdTotalOrdersReferred.Count;
    target.I2umberOfPayingOrders.Count = source.LjdNumberOfPayingOrders.Count;
    target.I2rdersInLocate.Count = source.LjdNumberOfOrdersInLocate.Count;
    target.I2ips.Assign(source.LjdFips);
    MoveTribunal(source.LjdTribunal, target.I2ribunal);
    target.I2egalAction.StandardNumber = source.LjdLegalAction.StandardNumber;
    target.I2extWorkArea.Text2 = source.LjdReferralType.Text2;
    target.I2ollCurrentState.TotalCurrency =
      source.LjdCollCurrentState.TotalCurrency;
    target.I2ollCurrentFamily.TotalCurrency =
      source.LjdCollCurrentFamily.TotalCurrency;
    target.I2ollCurrentIState.TotalCurrency =
      source.LjdCollCurrentIState.TotalCurrency;
    target.I2ollCurrentIFamily.TotalCurrency =
      source.LjdCollCurrentIFamily.TotalCurrency;
    target.I2ollArrearsState.TotalCurrency =
      source.LjdCollArrearsState.TotalCurrency;
    target.I2ollArrearsFamily.TotalCurrency =
      source.LjdCollArrearsFamily.TotalCurrency;
    target.I2ollArrearsIState.TotalCurrency =
      source.LjdCollArrearsIState.TotalCurrency;
    target.I2ollArrearsIFamily.TotalCurrency =
      source.LjdCollArrearsIFamily.TotalCurrency;
    target.I2wedCurrentState.TotalCurrency =
      source.LjdOwedCurrentState.TotalCurrency;
    target.I2wedCurrentFamily.TotalCurrency =
      source.LjdOwedCurrentFamily.TotalCurrency;
    target.I2wedCurrentIState.TotalCurrency =
      source.LjdOwedCurrentIState.TotalCurrency;
    target.I2wedCurrentIFamily.TotalCurrency =
      source.LjdOwedCurrentIFamily.TotalCurrency;
    target.I2wedArrearsState.TotalCurrency =
      source.LjdOwedArrearsState.TotalCurrency;
    target.I2wedArrearsFamily.TotalCurrency =
      source.LjdOwedArrearsFamily.TotalCurrency;
    target.I2wedArrearsIState.TotalCurrency =
      source.LjdOwedArrearsIState.TotalCurrency;
    target.I2wedArrearsIFamily.TotalCurrency =
      source.LjdOwedArrearsIFamily.TotalCurrency;
    target.I2wedCurrTotal.TotalCurrency = source.LjdOwedCurrTotal.TotalCurrency;
    target.I2wedArrearsTotal.TotalCurrency =
      source.LjdOwedArrearsTotal.TotalCurrency;
    target.I2wedTotal.TotalCurrency = source.LjdOwedTotal.TotalCurrency;
    target.I2ollCurrTotal.TotalCurrency = source.LjdCollCurrTotal.TotalCurrency;
    target.I2ollArrearsTotal.TotalCurrency =
      source.LjdCollArrearsTotal.TotalCurrency;
    target.I2ollectedTotal.TotalCurrency =
      source.LjdCollectedTotal.TotalCurrency;
  }

  private static void MoveJdLevelToReferralType(Local.JdLevelGroup source,
    FnB622CreateReport.Import.ReferralTypeGroup target)
  {
    target.ItotalOrdersReferred.Count = source.LjdTotalOrdersReferred.Count;
    target.InumberOfPayingOrders.Count = source.LjdNumberOfPayingOrders.Count;
    target.InumberOfOrdersInLocate.Count =
      source.LjdNumberOfOrdersInLocate.Count;
    target.Ifips.Assign(source.LjdFips);
    MoveTribunal(source.LjdTribunal, target.Itribunal);
    target.IlegalAction.StandardNumber = source.LjdLegalAction.StandardNumber;
    target.IreferralType.Text2 = source.LjdReferralType.Text2;
    target.IcollCurrentState.TotalCurrency =
      source.LjdCollCurrentState.TotalCurrency;
    target.IcollCurrentFamily.TotalCurrency =
      source.LjdCollCurrentFamily.TotalCurrency;
    target.IcollCurrentIState.TotalCurrency =
      source.LjdCollCurrentIState.TotalCurrency;
    target.IcollCurrentIFamily.TotalCurrency =
      source.LjdCollCurrentIFamily.TotalCurrency;
    target.IcollArrearsState.TotalCurrency =
      source.LjdCollArrearsState.TotalCurrency;
    target.IcollArrearsFamily.TotalCurrency =
      source.LjdCollArrearsFamily.TotalCurrency;
    target.IcollArrearsIState.TotalCurrency =
      source.LjdCollArrearsIState.TotalCurrency;
    target.IcollArrearsIFamily.TotalCurrency =
      source.LjdCollArrearsIFamily.TotalCurrency;
    target.IowedCurrentState.TotalCurrency =
      source.LjdOwedCurrentState.TotalCurrency;
    target.IowedCurrentFamily.TotalCurrency =
      source.LjdOwedCurrentFamily.TotalCurrency;
    target.IowedCurrentIState.TotalCurrency =
      source.LjdOwedCurrentIState.TotalCurrency;
    target.IowedCurrentIFamily.TotalCurrency =
      source.LjdOwedCurrentIFamily.TotalCurrency;
    target.IowedArrearsState.TotalCurrency =
      source.LjdOwedArrearsState.TotalCurrency;
    target.IowedArrearsFamily.TotalCurrency =
      source.LjdOwedArrearsFamily.TotalCurrency;
    target.IowedArrearsIState.TotalCurrency =
      source.LjdOwedArrearsIState.TotalCurrency;
    target.IowedArrearsIFamily.TotalCurrency =
      source.LjdOwedArrearsIFamily.TotalCurrency;
    target.IowedCurrTotal.TotalCurrency = source.LjdOwedCurrTotal.TotalCurrency;
    target.IowedArrearsTotal.TotalCurrency =
      source.LjdOwedArrearsTotal.TotalCurrency;
    target.IowedTotal.TotalCurrency = source.LjdOwedTotal.TotalCurrency;
    target.IcollCurrTotal.TotalCurrency = source.LjdCollCurrTotal.TotalCurrency;
    target.IcollArrearsTotal.TotalCurrency =
      source.LjdCollArrearsTotal.TotalCurrency;
    target.IcollectedTotal.TotalCurrency =
      source.LjdCollectedTotal.TotalCurrency;
  }

  private static void MoveReportLevelToJdLevel(FnB622CreateReport.Export.
    ReportLevelGroup source, Local.JdLevelGroup target)
  {
    target.LjdTotalOrdersReferred.Count = source.EtotalOrdersReferred.Count;
    target.LjdNumberOfPayingOrders.Count = source.EnumberOfPayingOrders.Count;
    target.LjdNumberOfOrdersInLocate.Count =
      source.EnumberOfOrdersInLocate.Count;
    target.LjdFips.Assign(source.Efips);
    MoveTribunal(source.Etribunal, target.LjdTribunal);
    target.LjdLegalAction.StandardNumber = source.ElegalAction.StandardNumber;
    target.LjdReferralType.Text2 = source.EreferralType.Text2;
    target.LjdCollCurrentState.TotalCurrency =
      source.EcollCurrentState.TotalCurrency;
    target.LjdCollCurrentFamily.TotalCurrency =
      source.EcollCurrentFamily.TotalCurrency;
    target.LjdCollCurrentIState.TotalCurrency =
      source.EcollCurrentIState.TotalCurrency;
    target.LjdCollCurrentIFamily.TotalCurrency =
      source.EcollCurrentIFamily.TotalCurrency;
    target.LjdCollArrearsState.TotalCurrency =
      source.EcollArrearsState.TotalCurrency;
    target.LjdCollArrearsFamily.TotalCurrency =
      source.EcollArrearsFamily.TotalCurrency;
    target.LjdCollArrearsIState.TotalCurrency =
      source.EcollArrearsIState.TotalCurrency;
    target.LjdCollArrearsIFamily.TotalCurrency =
      source.EcollArrearsIFamily.TotalCurrency;
    target.LjdOwedCurrentState.TotalCurrency =
      source.EowedCurrentState.TotalCurrency;
    target.LjdOwedCurrentFamily.TotalCurrency =
      source.EowedCurrentFamily.TotalCurrency;
    target.LjdOwedCurrentIState.TotalCurrency =
      source.EowedCurrentIState.TotalCurrency;
    target.LjdOwedCurrentIFamily.TotalCurrency =
      source.EowedCurrentIFamily.TotalCurrency;
    target.LjdOwedArrearsState.TotalCurrency =
      source.EowedArrearsState.TotalCurrency;
    target.LjdOwedArrearsFamily.TotalCurrency =
      source.EowedArrearsFamily.TotalCurrency;
    target.LjdOwedArrearsIState.TotalCurrency =
      source.EowedArrearsIState.TotalCurrency;
    target.LjdOwedArrearsIFamily.TotalCurrency =
      source.EowedArrearsIFamily.TotalCurrency;
    target.LjdOwedCurrTotal.TotalCurrency = source.EowedCurrTotal.TotalCurrency;
    target.LjdOwedArrearsTotal.TotalCurrency =
      source.EowedArrearsTotal.TotalCurrency;
    target.LjdOwedTotal.TotalCurrency = source.EowedTotal.TotalCurrency;
    target.LjdCollCurrTotal.TotalCurrency = source.EcollCurrTotal.TotalCurrency;
    target.LjdCollArrearsTotal.TotalCurrency =
      source.EcollArrearsTotal.TotalCurrency;
    target.LjdCollectedTotal.TotalCurrency =
      source.EcollectedTotal.TotalCurrency;
  }

  private static void MoveReportLevelToStateLevel(FnB622CreateReport.Export.
    ReportLevelGroup source, Local.StateLevelGroup target)
  {
    target.LstTotalOrdersReferred.Count = source.EtotalOrdersReferred.Count;
    target.LstNumberOfPayingOrders.Count = source.EnumberOfPayingOrders.Count;
    target.LstNumberOfOrdersInLocate.Count =
      source.EnumberOfOrdersInLocate.Count;
    target.LstFips.Assign(source.Efips);
    MoveTribunal(source.Etribunal, target.LstTribunal);
    target.LstLegalAction.StandardNumber = source.ElegalAction.StandardNumber;
    target.LstReferralType.Text2 = source.EreferralType.Text2;
    target.LstCollCurrentState.TotalCurrency =
      source.EcollCurrentState.TotalCurrency;
    target.LstCollCurrentFamily.TotalCurrency =
      source.EcollCurrentFamily.TotalCurrency;
    target.LstCollCurrentIState.TotalCurrency =
      source.EcollCurrentIState.TotalCurrency;
    target.LstCollCurrentIFamily.TotalCurrency =
      source.EcollCurrentIFamily.TotalCurrency;
    target.LstCollArrearsState.TotalCurrency =
      source.EcollArrearsState.TotalCurrency;
    target.LstCollArrearsFamily.TotalCurrency =
      source.EcollArrearsFamily.TotalCurrency;
    target.LstCollArrearsIState.TotalCurrency =
      source.EcollArrearsIState.TotalCurrency;
    target.LstCollArrearsIFamily.TotalCurrency =
      source.EcollArrearsIFamily.TotalCurrency;
    target.LstOwedCurrentState.TotalCurrency =
      source.EowedCurrentState.TotalCurrency;
    target.LstOwedCurrentFamily.TotalCurrency =
      source.EowedCurrentFamily.TotalCurrency;
    target.LstOwedCurrentIState.TotalCurrency =
      source.EowedCurrentIState.TotalCurrency;
    target.LstOwedCurrentIFamily.TotalCurrency =
      source.EowedCurrentIFamily.TotalCurrency;
    target.LstOwedArrearsState.TotalCurrency =
      source.EowedArrearsState.TotalCurrency;
    target.LstOwedArrearsFamily.TotalCurrency =
      source.EowedArrearsFamily.TotalCurrency;
    target.LstOwedArrearsIState.TotalCurrency =
      source.EowedArrearsIState.TotalCurrency;
    target.LstOwedArrearsIFamily.TotalCurrency =
      source.EowedArrearsIFamily.TotalCurrency;
    target.LstOwedCurrTotal.TotalCurrency = source.EowedCurrTotal.TotalCurrency;
    target.LstOwedArrearsTotal.TotalCurrency =
      source.EowedArrearsTotal.TotalCurrency;
    target.LstOwedTotal.TotalCurrency = source.EowedTotal.TotalCurrency;
    target.LstCollCurrTotal.TotalCurrency = source.EcollCurrTotal.TotalCurrency;
    target.LstCollArrearsTotal.TotalCurrency =
      source.EcollArrearsTotal.TotalCurrency;
    target.LstCollectedTotal.TotalCurrency =
      source.EcollectedTotal.TotalCurrency;
  }

  private static void MoveStateLevelToGroup2(Local.StateLevelGroup source,
    FnB622RollUpTotals.Import.Group2Group target)
  {
    target.I2otalOrdersReferred.Count = source.LstTotalOrdersReferred.Count;
    target.I2umberOfPayingOrders.Count = source.LstNumberOfPayingOrders.Count;
    target.I2rdersInLocate.Count = source.LstNumberOfOrdersInLocate.Count;
    target.I2ips.Assign(source.LstFips);
    MoveTribunal(source.LstTribunal, target.I2ribunal);
    target.I2egalAction.StandardNumber = source.LstLegalAction.StandardNumber;
    target.I2extWorkArea.Text2 = source.LstReferralType.Text2;
    target.I2ollCurrentState.TotalCurrency =
      source.LstCollCurrentState.TotalCurrency;
    target.I2ollCurrentFamily.TotalCurrency =
      source.LstCollCurrentFamily.TotalCurrency;
    target.I2ollCurrentIState.TotalCurrency =
      source.LstCollCurrentIState.TotalCurrency;
    target.I2ollCurrentIFamily.TotalCurrency =
      source.LstCollCurrentIFamily.TotalCurrency;
    target.I2ollArrearsState.TotalCurrency =
      source.LstCollArrearsState.TotalCurrency;
    target.I2ollArrearsFamily.TotalCurrency =
      source.LstCollArrearsFamily.TotalCurrency;
    target.I2ollArrearsIState.TotalCurrency =
      source.LstCollArrearsIState.TotalCurrency;
    target.I2ollArrearsIFamily.TotalCurrency =
      source.LstCollArrearsIFamily.TotalCurrency;
    target.I2wedCurrentState.TotalCurrency =
      source.LstOwedCurrentState.TotalCurrency;
    target.I2wedCurrentFamily.TotalCurrency =
      source.LstOwedCurrentFamily.TotalCurrency;
    target.I2wedCurrentIState.TotalCurrency =
      source.LstOwedCurrentIState.TotalCurrency;
    target.I2wedCurrentIFamily.TotalCurrency =
      source.LstOwedCurrentIFamily.TotalCurrency;
    target.I2wedArrearsState.TotalCurrency =
      source.LstOwedArrearsState.TotalCurrency;
    target.I2wedArrearsFamily.TotalCurrency =
      source.LstOwedArrearsFamily.TotalCurrency;
    target.I2wedArrearsIState.TotalCurrency =
      source.LstOwedArrearsIState.TotalCurrency;
    target.I2wedArrearsIFamily.TotalCurrency =
      source.LstOwedArrearsIFamily.TotalCurrency;
    target.I2wedCurrTotal.TotalCurrency = source.LstOwedCurrTotal.TotalCurrency;
    target.I2wedArrearsTotal.TotalCurrency =
      source.LstOwedArrearsTotal.TotalCurrency;
    target.I2wedTotal.TotalCurrency = source.LstOwedTotal.TotalCurrency;
    target.I2ollCurrTotal.TotalCurrency = source.LstCollCurrTotal.TotalCurrency;
    target.I2ollArrearsTotal.TotalCurrency =
      source.LstCollArrearsTotal.TotalCurrency;
    target.I2ollectedTotal.TotalCurrency =
      source.LstCollectedTotal.TotalCurrency;
  }

  private static void MoveStateLevelToReferralType(Local.StateLevelGroup source,
    FnB622CreateReport.Import.ReferralTypeGroup target)
  {
    target.ItotalOrdersReferred.Count = source.LstTotalOrdersReferred.Count;
    target.InumberOfPayingOrders.Count = source.LstNumberOfPayingOrders.Count;
    target.InumberOfOrdersInLocate.Count =
      source.LstNumberOfOrdersInLocate.Count;
    target.Ifips.Assign(source.LstFips);
    MoveTribunal(source.LstTribunal, target.Itribunal);
    target.IlegalAction.StandardNumber = source.LstLegalAction.StandardNumber;
    target.IreferralType.Text2 = source.LstReferralType.Text2;
    target.IcollCurrentState.TotalCurrency =
      source.LstCollCurrentState.TotalCurrency;
    target.IcollCurrentFamily.TotalCurrency =
      source.LstCollCurrentFamily.TotalCurrency;
    target.IcollCurrentIState.TotalCurrency =
      source.LstCollCurrentIState.TotalCurrency;
    target.IcollCurrentIFamily.TotalCurrency =
      source.LstCollCurrentIFamily.TotalCurrency;
    target.IcollArrearsState.TotalCurrency =
      source.LstCollArrearsState.TotalCurrency;
    target.IcollArrearsFamily.TotalCurrency =
      source.LstCollArrearsFamily.TotalCurrency;
    target.IcollArrearsIState.TotalCurrency =
      source.LstCollArrearsIState.TotalCurrency;
    target.IcollArrearsIFamily.TotalCurrency =
      source.LstCollArrearsIFamily.TotalCurrency;
    target.IowedCurrentState.TotalCurrency =
      source.LstOwedCurrentState.TotalCurrency;
    target.IowedCurrentFamily.TotalCurrency =
      source.LstOwedCurrentFamily.TotalCurrency;
    target.IowedCurrentIState.TotalCurrency =
      source.LstOwedCurrentIState.TotalCurrency;
    target.IowedCurrentIFamily.TotalCurrency =
      source.LstOwedCurrentIFamily.TotalCurrency;
    target.IowedArrearsState.TotalCurrency =
      source.LstOwedArrearsState.TotalCurrency;
    target.IowedArrearsFamily.TotalCurrency =
      source.LstOwedArrearsFamily.TotalCurrency;
    target.IowedArrearsIState.TotalCurrency =
      source.LstOwedArrearsIState.TotalCurrency;
    target.IowedArrearsIFamily.TotalCurrency =
      source.LstOwedArrearsIFamily.TotalCurrency;
    target.IowedCurrTotal.TotalCurrency = source.LstOwedCurrTotal.TotalCurrency;
    target.IowedArrearsTotal.TotalCurrency =
      source.LstOwedArrearsTotal.TotalCurrency;
    target.IowedTotal.TotalCurrency = source.LstOwedTotal.TotalCurrency;
    target.IcollCurrTotal.TotalCurrency = source.LstCollCurrTotal.TotalCurrency;
    target.IcollArrearsTotal.TotalCurrency =
      source.LstCollArrearsTotal.TotalCurrency;
    target.IcollectedTotal.TotalCurrency =
      source.LstCollectedTotal.TotalCurrency;
  }

  private static void MoveTribunal(Tribunal source, Tribunal target)
  {
    target.Name = source.Name;
    target.JudicialDistrict = source.JudicialDistrict;
  }

  private void UseCabDetermineReportingDates()
  {
    var useImport = new CabDetermineReportingDates.Import();
    var useExport = new CabDetermineReportingDates.Export();

    useImport.DateWorkArea.Date = local.Process.Date;

    Call(CabDetermineReportingDates.Execute, useImport, useExport);

    MoveDateWorkArea(useExport.Bom, local.Bom);
    MoveDateWorkArea(useExport.Eom, local.Eom);
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.Initialized.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseFnB622CreateReport1()
  {
    var useImport = new FnB622CreateReport.Import();
    var useExport = new FnB622CreateReport.Export();

    useImport.TextDate.Text10 = local.TextDate.Text10;
    useImport.TextDd.Text2 = local.TextDd.Text2;
    useImport.TextYyyy.Text4 = local.TextYyyy.Text4;
    useImport.MonthName.Text9 = local.MonthName.Text9;
    local.CountyLevel.CopyTo(
      useImport.ReferralType, MoveCountyLevelToReferralType);
    useImport.Fips.Assign(local.Fips);
    MoveTribunal(local.Tribunal, useImport.Tribunal);
    useImport.PageNumber.Count = local.PageNumber.Count;
    useImport.ReportLevel1.Text5 = local.ReportLevel.Text5;
    useImport.Eom.Date = local.Eom.Date;

    Call(FnB622CreateReport.Execute, useImport, useExport);
  }

  private void UseFnB622CreateReport2()
  {
    var useImport = new FnB622CreateReport.Import();
    var useExport = new FnB622CreateReport.Export();

    useImport.TextDate.Text10 = local.TextDate.Text10;
    useImport.TextDd.Text2 = local.TextDd.Text2;
    useImport.TextYyyy.Text4 = local.TextYyyy.Text4;
    useImport.MonthName.Text9 = local.MonthName.Text9;
    local.JdLevel.CopyTo(useImport.ReferralType, MoveJdLevelToReferralType);
    useImport.Fips.Assign(local.SaveFips);
    MoveTribunal(local.SaveTribunal, useImport.Tribunal);
    useImport.PageNumber.Count = local.PageNumber.Count;
    useImport.ReportLevel1.Text5 = local.ReportLevel.Text5;
    useImport.Eom.Date = local.Eom.Date;

    Call(FnB622CreateReport.Execute, useImport, useExport);

    useExport.ReportLevel.CopyTo(local.JdLevel, MoveReportLevelToJdLevel);
  }

  private void UseFnB622CreateReport3()
  {
    var useImport = new FnB622CreateReport.Import();
    var useExport = new FnB622CreateReport.Export();

    useImport.TextDate.Text10 = local.TextDate.Text10;
    useImport.TextDd.Text2 = local.TextDd.Text2;
    useImport.TextYyyy.Text4 = local.TextYyyy.Text4;
    useImport.MonthName.Text9 = local.MonthName.Text9;
    local.StateLevel.
      CopyTo(useImport.ReferralType, MoveStateLevelToReferralType);
    useImport.Fips.Assign(local.SaveFips);
    MoveTribunal(local.SaveTribunal, useImport.Tribunal);
    useImport.PageNumber.Count = local.PageNumber.Count;
    useImport.ReportLevel1.Text5 = local.ReportLevel.Text5;
    useImport.Eom.Date = local.Eom.Date;

    Call(FnB622CreateReport.Execute, useImport, useExport);

    useExport.ReportLevel.CopyTo(local.StateLevel, MoveReportLevelToStateLevel);
  }

  private void UseFnB622ExternalToWriteReport()
  {
    var useImport = new FnB622ExternalToWriteReport.Import();
    var useExport = new FnB622ExternalToWriteReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(FnB622ExternalToWriteReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnB622RollUpTotals1()
  {
    var useImport = new FnB622RollUpTotals.Import();
    var useExport = new FnB622RollUpTotals.Export();

    local.CountyLevel.CopyTo(useImport.Group1, MoveCountyLevelToGroup1);
    local.JdLevel.CopyTo(useImport.Group2, MoveJdLevelToGroup2);

    Call(FnB622RollUpTotals.Execute, useImport, useExport);

    useExport.Group.CopyTo(local.JdLevel, MoveGroupToJdLevel);
  }

  private void UseFnB622RollUpTotals2()
  {
    var useImport = new FnB622RollUpTotals.Import();
    var useExport = new FnB622RollUpTotals.Export();

    local.CountyLevel.CopyTo(useImport.Group1, MoveCountyLevelToGroup1);
    local.StateLevel.CopyTo(useImport.Group2, MoveStateLevelToGroup2);

    Call(FnB622RollUpTotals.Execute, useImport, useExport);

    useExport.Group.CopyTo(local.StateLevel, MoveGroupToStateLevel);
  }

  private void UseFnEabSwexfr17CollPotRead1()
  {
    var useImport = new FnEabSwexfr17CollPotRead.Import();
    var useExport = new FnEabSwexfr17CollPotRead.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    local.CountyLevel.CopyTo(useExport.Group, MoveCountyLevelToGroup);
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(FnEabSwexfr17CollPotRead.Execute, useImport, useExport);

    useExport.Group.CopyTo(local.CountyLevel, MoveGroupToCountyLevel);
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnEabSwexfr17CollPotRead2()
  {
    var useImport = new FnEabSwexfr17CollPotRead.Import();
    var useExport = new FnEabSwexfr17CollPotRead.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(FnEabSwexfr17CollPotRead.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
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
    /// <summary>
    /// A value of Eom.
    /// </summary>
    [JsonPropertyName("eom")]
    public DateWorkArea Eom
    {
      get => eom ??= new();
      set => eom = value;
    }

    private DateWorkArea eom;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A CountyLevelGroup group.</summary>
    [Serializable]
    public class CountyLevelGroup
    {
      /// <summary>
      /// A value of LtotalOrdersReferred.
      /// </summary>
      [JsonPropertyName("ltotalOrdersReferred")]
      public Common LtotalOrdersReferred
      {
        get => ltotalOrdersReferred ??= new();
        set => ltotalOrdersReferred = value;
      }

      /// <summary>
      /// A value of LnumberOfPayingOrders.
      /// </summary>
      [JsonPropertyName("lnumberOfPayingOrders")]
      public Common LnumberOfPayingOrders
      {
        get => lnumberOfPayingOrders ??= new();
        set => lnumberOfPayingOrders = value;
      }

      /// <summary>
      /// A value of LnumberOfOrdersInLocate.
      /// </summary>
      [JsonPropertyName("lnumberOfOrdersInLocate")]
      public Common LnumberOfOrdersInLocate
      {
        get => lnumberOfOrdersInLocate ??= new();
        set => lnumberOfOrdersInLocate = value;
      }

      /// <summary>
      /// A value of Fips.
      /// </summary>
      [JsonPropertyName("fips")]
      public Fips Fips
      {
        get => fips ??= new();
        set => fips = value;
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
      /// A value of TextWorkArea.
      /// </summary>
      [JsonPropertyName("textWorkArea")]
      public TextWorkArea TextWorkArea
      {
        get => textWorkArea ??= new();
        set => textWorkArea = value;
      }

      /// <summary>
      /// A value of LcollCurrentState.
      /// </summary>
      [JsonPropertyName("lcollCurrentState")]
      public Common LcollCurrentState
      {
        get => lcollCurrentState ??= new();
        set => lcollCurrentState = value;
      }

      /// <summary>
      /// A value of LcollCurrentFamily.
      /// </summary>
      [JsonPropertyName("lcollCurrentFamily")]
      public Common LcollCurrentFamily
      {
        get => lcollCurrentFamily ??= new();
        set => lcollCurrentFamily = value;
      }

      /// <summary>
      /// A value of LcollCurrentIState.
      /// </summary>
      [JsonPropertyName("lcollCurrentIState")]
      public Common LcollCurrentIState
      {
        get => lcollCurrentIState ??= new();
        set => lcollCurrentIState = value;
      }

      /// <summary>
      /// A value of LcollCurrentIFamily.
      /// </summary>
      [JsonPropertyName("lcollCurrentIFamily")]
      public Common LcollCurrentIFamily
      {
        get => lcollCurrentIFamily ??= new();
        set => lcollCurrentIFamily = value;
      }

      /// <summary>
      /// A value of LcollArrearsState.
      /// </summary>
      [JsonPropertyName("lcollArrearsState")]
      public Common LcollArrearsState
      {
        get => lcollArrearsState ??= new();
        set => lcollArrearsState = value;
      }

      /// <summary>
      /// A value of LcollArrearsFamily.
      /// </summary>
      [JsonPropertyName("lcollArrearsFamily")]
      public Common LcollArrearsFamily
      {
        get => lcollArrearsFamily ??= new();
        set => lcollArrearsFamily = value;
      }

      /// <summary>
      /// A value of LcollArrearsIState.
      /// </summary>
      [JsonPropertyName("lcollArrearsIState")]
      public Common LcollArrearsIState
      {
        get => lcollArrearsIState ??= new();
        set => lcollArrearsIState = value;
      }

      /// <summary>
      /// A value of LcollArrearsIFamily.
      /// </summary>
      [JsonPropertyName("lcollArrearsIFamily")]
      public Common LcollArrearsIFamily
      {
        get => lcollArrearsIFamily ??= new();
        set => lcollArrearsIFamily = value;
      }

      /// <summary>
      /// A value of LowedCurrentState.
      /// </summary>
      [JsonPropertyName("lowedCurrentState")]
      public Common LowedCurrentState
      {
        get => lowedCurrentState ??= new();
        set => lowedCurrentState = value;
      }

      /// <summary>
      /// A value of LowedCurrentFamily.
      /// </summary>
      [JsonPropertyName("lowedCurrentFamily")]
      public Common LowedCurrentFamily
      {
        get => lowedCurrentFamily ??= new();
        set => lowedCurrentFamily = value;
      }

      /// <summary>
      /// A value of LowedCurrentIState.
      /// </summary>
      [JsonPropertyName("lowedCurrentIState")]
      public Common LowedCurrentIState
      {
        get => lowedCurrentIState ??= new();
        set => lowedCurrentIState = value;
      }

      /// <summary>
      /// A value of LowedCurrentIFamily.
      /// </summary>
      [JsonPropertyName("lowedCurrentIFamily")]
      public Common LowedCurrentIFamily
      {
        get => lowedCurrentIFamily ??= new();
        set => lowedCurrentIFamily = value;
      }

      /// <summary>
      /// A value of LowedArrearsState.
      /// </summary>
      [JsonPropertyName("lowedArrearsState")]
      public Common LowedArrearsState
      {
        get => lowedArrearsState ??= new();
        set => lowedArrearsState = value;
      }

      /// <summary>
      /// A value of LowedArrearsFamily.
      /// </summary>
      [JsonPropertyName("lowedArrearsFamily")]
      public Common LowedArrearsFamily
      {
        get => lowedArrearsFamily ??= new();
        set => lowedArrearsFamily = value;
      }

      /// <summary>
      /// A value of LowedArrearsIState.
      /// </summary>
      [JsonPropertyName("lowedArrearsIState")]
      public Common LowedArrearsIState
      {
        get => lowedArrearsIState ??= new();
        set => lowedArrearsIState = value;
      }

      /// <summary>
      /// A value of LowedArrearsIFamily.
      /// </summary>
      [JsonPropertyName("lowedArrearsIFamily")]
      public Common LowedArrearsIFamily
      {
        get => lowedArrearsIFamily ??= new();
        set => lowedArrearsIFamily = value;
      }

      /// <summary>
      /// A value of LowedCurrTotal.
      /// </summary>
      [JsonPropertyName("lowedCurrTotal")]
      public Common LowedCurrTotal
      {
        get => lowedCurrTotal ??= new();
        set => lowedCurrTotal = value;
      }

      /// <summary>
      /// A value of LowedArrearsTotal.
      /// </summary>
      [JsonPropertyName("lowedArrearsTotal")]
      public Common LowedArrearsTotal
      {
        get => lowedArrearsTotal ??= new();
        set => lowedArrearsTotal = value;
      }

      /// <summary>
      /// A value of LowedTotal.
      /// </summary>
      [JsonPropertyName("lowedTotal")]
      public Common LowedTotal
      {
        get => lowedTotal ??= new();
        set => lowedTotal = value;
      }

      /// <summary>
      /// A value of LcollCurrTotal.
      /// </summary>
      [JsonPropertyName("lcollCurrTotal")]
      public Common LcollCurrTotal
      {
        get => lcollCurrTotal ??= new();
        set => lcollCurrTotal = value;
      }

      /// <summary>
      /// A value of LcollArrearsTotal.
      /// </summary>
      [JsonPropertyName("lcollArrearsTotal")]
      public Common LcollArrearsTotal
      {
        get => lcollArrearsTotal ??= new();
        set => lcollArrearsTotal = value;
      }

      /// <summary>
      /// A value of LcollectedTotal.
      /// </summary>
      [JsonPropertyName("lcollectedTotal")]
      public Common LcollectedTotal
      {
        get => lcollectedTotal ??= new();
        set => lcollectedTotal = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private Common ltotalOrdersReferred;
      private Common lnumberOfPayingOrders;
      private Common lnumberOfOrdersInLocate;
      private Fips fips;
      private Tribunal tribunal;
      private LegalAction legalAction;
      private TextWorkArea textWorkArea;
      private Common lcollCurrentState;
      private Common lcollCurrentFamily;
      private Common lcollCurrentIState;
      private Common lcollCurrentIFamily;
      private Common lcollArrearsState;
      private Common lcollArrearsFamily;
      private Common lcollArrearsIState;
      private Common lcollArrearsIFamily;
      private Common lowedCurrentState;
      private Common lowedCurrentFamily;
      private Common lowedCurrentIState;
      private Common lowedCurrentIFamily;
      private Common lowedArrearsState;
      private Common lowedArrearsFamily;
      private Common lowedArrearsIState;
      private Common lowedArrearsIFamily;
      private Common lowedCurrTotal;
      private Common lowedArrearsTotal;
      private Common lowedTotal;
      private Common lcollCurrTotal;
      private Common lcollArrearsTotal;
      private Common lcollectedTotal;
    }

    /// <summary>A JdLevelGroup group.</summary>
    [Serializable]
    public class JdLevelGroup
    {
      /// <summary>
      /// A value of LjdTotalOrdersReferred.
      /// </summary>
      [JsonPropertyName("ljdTotalOrdersReferred")]
      public Common LjdTotalOrdersReferred
      {
        get => ljdTotalOrdersReferred ??= new();
        set => ljdTotalOrdersReferred = value;
      }

      /// <summary>
      /// A value of LjdNumberOfPayingOrders.
      /// </summary>
      [JsonPropertyName("ljdNumberOfPayingOrders")]
      public Common LjdNumberOfPayingOrders
      {
        get => ljdNumberOfPayingOrders ??= new();
        set => ljdNumberOfPayingOrders = value;
      }

      /// <summary>
      /// A value of LjdNumberOfOrdersInLocate.
      /// </summary>
      [JsonPropertyName("ljdNumberOfOrdersInLocate")]
      public Common LjdNumberOfOrdersInLocate
      {
        get => ljdNumberOfOrdersInLocate ??= new();
        set => ljdNumberOfOrdersInLocate = value;
      }

      /// <summary>
      /// A value of LjdFips.
      /// </summary>
      [JsonPropertyName("ljdFips")]
      public Fips LjdFips
      {
        get => ljdFips ??= new();
        set => ljdFips = value;
      }

      /// <summary>
      /// A value of LjdTribunal.
      /// </summary>
      [JsonPropertyName("ljdTribunal")]
      public Tribunal LjdTribunal
      {
        get => ljdTribunal ??= new();
        set => ljdTribunal = value;
      }

      /// <summary>
      /// A value of LjdLegalAction.
      /// </summary>
      [JsonPropertyName("ljdLegalAction")]
      public LegalAction LjdLegalAction
      {
        get => ljdLegalAction ??= new();
        set => ljdLegalAction = value;
      }

      /// <summary>
      /// A value of LjdReferralType.
      /// </summary>
      [JsonPropertyName("ljdReferralType")]
      public TextWorkArea LjdReferralType
      {
        get => ljdReferralType ??= new();
        set => ljdReferralType = value;
      }

      /// <summary>
      /// A value of LjdCollCurrentState.
      /// </summary>
      [JsonPropertyName("ljdCollCurrentState")]
      public Common LjdCollCurrentState
      {
        get => ljdCollCurrentState ??= new();
        set => ljdCollCurrentState = value;
      }

      /// <summary>
      /// A value of LjdCollCurrentFamily.
      /// </summary>
      [JsonPropertyName("ljdCollCurrentFamily")]
      public Common LjdCollCurrentFamily
      {
        get => ljdCollCurrentFamily ??= new();
        set => ljdCollCurrentFamily = value;
      }

      /// <summary>
      /// A value of LjdCollCurrentIState.
      /// </summary>
      [JsonPropertyName("ljdCollCurrentIState")]
      public Common LjdCollCurrentIState
      {
        get => ljdCollCurrentIState ??= new();
        set => ljdCollCurrentIState = value;
      }

      /// <summary>
      /// A value of LjdCollCurrentIFamily.
      /// </summary>
      [JsonPropertyName("ljdCollCurrentIFamily")]
      public Common LjdCollCurrentIFamily
      {
        get => ljdCollCurrentIFamily ??= new();
        set => ljdCollCurrentIFamily = value;
      }

      /// <summary>
      /// A value of LjdCollArrearsState.
      /// </summary>
      [JsonPropertyName("ljdCollArrearsState")]
      public Common LjdCollArrearsState
      {
        get => ljdCollArrearsState ??= new();
        set => ljdCollArrearsState = value;
      }

      /// <summary>
      /// A value of LjdCollArrearsFamily.
      /// </summary>
      [JsonPropertyName("ljdCollArrearsFamily")]
      public Common LjdCollArrearsFamily
      {
        get => ljdCollArrearsFamily ??= new();
        set => ljdCollArrearsFamily = value;
      }

      /// <summary>
      /// A value of LjdCollArrearsIState.
      /// </summary>
      [JsonPropertyName("ljdCollArrearsIState")]
      public Common LjdCollArrearsIState
      {
        get => ljdCollArrearsIState ??= new();
        set => ljdCollArrearsIState = value;
      }

      /// <summary>
      /// A value of LjdCollArrearsIFamily.
      /// </summary>
      [JsonPropertyName("ljdCollArrearsIFamily")]
      public Common LjdCollArrearsIFamily
      {
        get => ljdCollArrearsIFamily ??= new();
        set => ljdCollArrearsIFamily = value;
      }

      /// <summary>
      /// A value of LjdOwedCurrentState.
      /// </summary>
      [JsonPropertyName("ljdOwedCurrentState")]
      public Common LjdOwedCurrentState
      {
        get => ljdOwedCurrentState ??= new();
        set => ljdOwedCurrentState = value;
      }

      /// <summary>
      /// A value of LjdOwedCurrentFamily.
      /// </summary>
      [JsonPropertyName("ljdOwedCurrentFamily")]
      public Common LjdOwedCurrentFamily
      {
        get => ljdOwedCurrentFamily ??= new();
        set => ljdOwedCurrentFamily = value;
      }

      /// <summary>
      /// A value of LjdOwedCurrentIState.
      /// </summary>
      [JsonPropertyName("ljdOwedCurrentIState")]
      public Common LjdOwedCurrentIState
      {
        get => ljdOwedCurrentIState ??= new();
        set => ljdOwedCurrentIState = value;
      }

      /// <summary>
      /// A value of LjdOwedCurrentIFamily.
      /// </summary>
      [JsonPropertyName("ljdOwedCurrentIFamily")]
      public Common LjdOwedCurrentIFamily
      {
        get => ljdOwedCurrentIFamily ??= new();
        set => ljdOwedCurrentIFamily = value;
      }

      /// <summary>
      /// A value of LjdOwedArrearsState.
      /// </summary>
      [JsonPropertyName("ljdOwedArrearsState")]
      public Common LjdOwedArrearsState
      {
        get => ljdOwedArrearsState ??= new();
        set => ljdOwedArrearsState = value;
      }

      /// <summary>
      /// A value of LjdOwedArrearsFamily.
      /// </summary>
      [JsonPropertyName("ljdOwedArrearsFamily")]
      public Common LjdOwedArrearsFamily
      {
        get => ljdOwedArrearsFamily ??= new();
        set => ljdOwedArrearsFamily = value;
      }

      /// <summary>
      /// A value of LjdOwedArrearsIState.
      /// </summary>
      [JsonPropertyName("ljdOwedArrearsIState")]
      public Common LjdOwedArrearsIState
      {
        get => ljdOwedArrearsIState ??= new();
        set => ljdOwedArrearsIState = value;
      }

      /// <summary>
      /// A value of LjdOwedArrearsIFamily.
      /// </summary>
      [JsonPropertyName("ljdOwedArrearsIFamily")]
      public Common LjdOwedArrearsIFamily
      {
        get => ljdOwedArrearsIFamily ??= new();
        set => ljdOwedArrearsIFamily = value;
      }

      /// <summary>
      /// A value of LjdOwedCurrTotal.
      /// </summary>
      [JsonPropertyName("ljdOwedCurrTotal")]
      public Common LjdOwedCurrTotal
      {
        get => ljdOwedCurrTotal ??= new();
        set => ljdOwedCurrTotal = value;
      }

      /// <summary>
      /// A value of LjdOwedArrearsTotal.
      /// </summary>
      [JsonPropertyName("ljdOwedArrearsTotal")]
      public Common LjdOwedArrearsTotal
      {
        get => ljdOwedArrearsTotal ??= new();
        set => ljdOwedArrearsTotal = value;
      }

      /// <summary>
      /// A value of LjdOwedTotal.
      /// </summary>
      [JsonPropertyName("ljdOwedTotal")]
      public Common LjdOwedTotal
      {
        get => ljdOwedTotal ??= new();
        set => ljdOwedTotal = value;
      }

      /// <summary>
      /// A value of LjdCollCurrTotal.
      /// </summary>
      [JsonPropertyName("ljdCollCurrTotal")]
      public Common LjdCollCurrTotal
      {
        get => ljdCollCurrTotal ??= new();
        set => ljdCollCurrTotal = value;
      }

      /// <summary>
      /// A value of LjdCollArrearsTotal.
      /// </summary>
      [JsonPropertyName("ljdCollArrearsTotal")]
      public Common LjdCollArrearsTotal
      {
        get => ljdCollArrearsTotal ??= new();
        set => ljdCollArrearsTotal = value;
      }

      /// <summary>
      /// A value of LjdCollectedTotal.
      /// </summary>
      [JsonPropertyName("ljdCollectedTotal")]
      public Common LjdCollectedTotal
      {
        get => ljdCollectedTotal ??= new();
        set => ljdCollectedTotal = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private Common ljdTotalOrdersReferred;
      private Common ljdNumberOfPayingOrders;
      private Common ljdNumberOfOrdersInLocate;
      private Fips ljdFips;
      private Tribunal ljdTribunal;
      private LegalAction ljdLegalAction;
      private TextWorkArea ljdReferralType;
      private Common ljdCollCurrentState;
      private Common ljdCollCurrentFamily;
      private Common ljdCollCurrentIState;
      private Common ljdCollCurrentIFamily;
      private Common ljdCollArrearsState;
      private Common ljdCollArrearsFamily;
      private Common ljdCollArrearsIState;
      private Common ljdCollArrearsIFamily;
      private Common ljdOwedCurrentState;
      private Common ljdOwedCurrentFamily;
      private Common ljdOwedCurrentIState;
      private Common ljdOwedCurrentIFamily;
      private Common ljdOwedArrearsState;
      private Common ljdOwedArrearsFamily;
      private Common ljdOwedArrearsIState;
      private Common ljdOwedArrearsIFamily;
      private Common ljdOwedCurrTotal;
      private Common ljdOwedArrearsTotal;
      private Common ljdOwedTotal;
      private Common ljdCollCurrTotal;
      private Common ljdCollArrearsTotal;
      private Common ljdCollectedTotal;
    }

    /// <summary>A StateLevelGroup group.</summary>
    [Serializable]
    public class StateLevelGroup
    {
      /// <summary>
      /// A value of LstTotalOrdersReferred.
      /// </summary>
      [JsonPropertyName("lstTotalOrdersReferred")]
      public Common LstTotalOrdersReferred
      {
        get => lstTotalOrdersReferred ??= new();
        set => lstTotalOrdersReferred = value;
      }

      /// <summary>
      /// A value of LstNumberOfPayingOrders.
      /// </summary>
      [JsonPropertyName("lstNumberOfPayingOrders")]
      public Common LstNumberOfPayingOrders
      {
        get => lstNumberOfPayingOrders ??= new();
        set => lstNumberOfPayingOrders = value;
      }

      /// <summary>
      /// A value of LstNumberOfOrdersInLocate.
      /// </summary>
      [JsonPropertyName("lstNumberOfOrdersInLocate")]
      public Common LstNumberOfOrdersInLocate
      {
        get => lstNumberOfOrdersInLocate ??= new();
        set => lstNumberOfOrdersInLocate = value;
      }

      /// <summary>
      /// A value of LstFips.
      /// </summary>
      [JsonPropertyName("lstFips")]
      public Fips LstFips
      {
        get => lstFips ??= new();
        set => lstFips = value;
      }

      /// <summary>
      /// A value of LstTribunal.
      /// </summary>
      [JsonPropertyName("lstTribunal")]
      public Tribunal LstTribunal
      {
        get => lstTribunal ??= new();
        set => lstTribunal = value;
      }

      /// <summary>
      /// A value of LstLegalAction.
      /// </summary>
      [JsonPropertyName("lstLegalAction")]
      public LegalAction LstLegalAction
      {
        get => lstLegalAction ??= new();
        set => lstLegalAction = value;
      }

      /// <summary>
      /// A value of LstReferralType.
      /// </summary>
      [JsonPropertyName("lstReferralType")]
      public TextWorkArea LstReferralType
      {
        get => lstReferralType ??= new();
        set => lstReferralType = value;
      }

      /// <summary>
      /// A value of LstCollCurrentState.
      /// </summary>
      [JsonPropertyName("lstCollCurrentState")]
      public Common LstCollCurrentState
      {
        get => lstCollCurrentState ??= new();
        set => lstCollCurrentState = value;
      }

      /// <summary>
      /// A value of LstCollCurrentFamily.
      /// </summary>
      [JsonPropertyName("lstCollCurrentFamily")]
      public Common LstCollCurrentFamily
      {
        get => lstCollCurrentFamily ??= new();
        set => lstCollCurrentFamily = value;
      }

      /// <summary>
      /// A value of LstCollCurrentIState.
      /// </summary>
      [JsonPropertyName("lstCollCurrentIState")]
      public Common LstCollCurrentIState
      {
        get => lstCollCurrentIState ??= new();
        set => lstCollCurrentIState = value;
      }

      /// <summary>
      /// A value of LstCollCurrentIFamily.
      /// </summary>
      [JsonPropertyName("lstCollCurrentIFamily")]
      public Common LstCollCurrentIFamily
      {
        get => lstCollCurrentIFamily ??= new();
        set => lstCollCurrentIFamily = value;
      }

      /// <summary>
      /// A value of LstCollArrearsState.
      /// </summary>
      [JsonPropertyName("lstCollArrearsState")]
      public Common LstCollArrearsState
      {
        get => lstCollArrearsState ??= new();
        set => lstCollArrearsState = value;
      }

      /// <summary>
      /// A value of LstCollArrearsFamily.
      /// </summary>
      [JsonPropertyName("lstCollArrearsFamily")]
      public Common LstCollArrearsFamily
      {
        get => lstCollArrearsFamily ??= new();
        set => lstCollArrearsFamily = value;
      }

      /// <summary>
      /// A value of LstCollArrearsIState.
      /// </summary>
      [JsonPropertyName("lstCollArrearsIState")]
      public Common LstCollArrearsIState
      {
        get => lstCollArrearsIState ??= new();
        set => lstCollArrearsIState = value;
      }

      /// <summary>
      /// A value of LstCollArrearsIFamily.
      /// </summary>
      [JsonPropertyName("lstCollArrearsIFamily")]
      public Common LstCollArrearsIFamily
      {
        get => lstCollArrearsIFamily ??= new();
        set => lstCollArrearsIFamily = value;
      }

      /// <summary>
      /// A value of LstOwedCurrentState.
      /// </summary>
      [JsonPropertyName("lstOwedCurrentState")]
      public Common LstOwedCurrentState
      {
        get => lstOwedCurrentState ??= new();
        set => lstOwedCurrentState = value;
      }

      /// <summary>
      /// A value of LstOwedCurrentFamily.
      /// </summary>
      [JsonPropertyName("lstOwedCurrentFamily")]
      public Common LstOwedCurrentFamily
      {
        get => lstOwedCurrentFamily ??= new();
        set => lstOwedCurrentFamily = value;
      }

      /// <summary>
      /// A value of LstOwedCurrentIState.
      /// </summary>
      [JsonPropertyName("lstOwedCurrentIState")]
      public Common LstOwedCurrentIState
      {
        get => lstOwedCurrentIState ??= new();
        set => lstOwedCurrentIState = value;
      }

      /// <summary>
      /// A value of LstOwedCurrentIFamily.
      /// </summary>
      [JsonPropertyName("lstOwedCurrentIFamily")]
      public Common LstOwedCurrentIFamily
      {
        get => lstOwedCurrentIFamily ??= new();
        set => lstOwedCurrentIFamily = value;
      }

      /// <summary>
      /// A value of LstOwedArrearsState.
      /// </summary>
      [JsonPropertyName("lstOwedArrearsState")]
      public Common LstOwedArrearsState
      {
        get => lstOwedArrearsState ??= new();
        set => lstOwedArrearsState = value;
      }

      /// <summary>
      /// A value of LstOwedArrearsFamily.
      /// </summary>
      [JsonPropertyName("lstOwedArrearsFamily")]
      public Common LstOwedArrearsFamily
      {
        get => lstOwedArrearsFamily ??= new();
        set => lstOwedArrearsFamily = value;
      }

      /// <summary>
      /// A value of LstOwedArrearsIState.
      /// </summary>
      [JsonPropertyName("lstOwedArrearsIState")]
      public Common LstOwedArrearsIState
      {
        get => lstOwedArrearsIState ??= new();
        set => lstOwedArrearsIState = value;
      }

      /// <summary>
      /// A value of LstOwedArrearsIFamily.
      /// </summary>
      [JsonPropertyName("lstOwedArrearsIFamily")]
      public Common LstOwedArrearsIFamily
      {
        get => lstOwedArrearsIFamily ??= new();
        set => lstOwedArrearsIFamily = value;
      }

      /// <summary>
      /// A value of LstOwedCurrTotal.
      /// </summary>
      [JsonPropertyName("lstOwedCurrTotal")]
      public Common LstOwedCurrTotal
      {
        get => lstOwedCurrTotal ??= new();
        set => lstOwedCurrTotal = value;
      }

      /// <summary>
      /// A value of LstOwedArrearsTotal.
      /// </summary>
      [JsonPropertyName("lstOwedArrearsTotal")]
      public Common LstOwedArrearsTotal
      {
        get => lstOwedArrearsTotal ??= new();
        set => lstOwedArrearsTotal = value;
      }

      /// <summary>
      /// A value of LstOwedTotal.
      /// </summary>
      [JsonPropertyName("lstOwedTotal")]
      public Common LstOwedTotal
      {
        get => lstOwedTotal ??= new();
        set => lstOwedTotal = value;
      }

      /// <summary>
      /// A value of LstCollCurrTotal.
      /// </summary>
      [JsonPropertyName("lstCollCurrTotal")]
      public Common LstCollCurrTotal
      {
        get => lstCollCurrTotal ??= new();
        set => lstCollCurrTotal = value;
      }

      /// <summary>
      /// A value of LstCollArrearsTotal.
      /// </summary>
      [JsonPropertyName("lstCollArrearsTotal")]
      public Common LstCollArrearsTotal
      {
        get => lstCollArrearsTotal ??= new();
        set => lstCollArrearsTotal = value;
      }

      /// <summary>
      /// A value of LstCollectedTotal.
      /// </summary>
      [JsonPropertyName("lstCollectedTotal")]
      public Common LstCollectedTotal
      {
        get => lstCollectedTotal ??= new();
        set => lstCollectedTotal = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private Common lstTotalOrdersReferred;
      private Common lstNumberOfPayingOrders;
      private Common lstNumberOfOrdersInLocate;
      private Fips lstFips;
      private Tribunal lstTribunal;
      private LegalAction lstLegalAction;
      private TextWorkArea lstReferralType;
      private Common lstCollCurrentState;
      private Common lstCollCurrentFamily;
      private Common lstCollCurrentIState;
      private Common lstCollCurrentIFamily;
      private Common lstCollArrearsState;
      private Common lstCollArrearsFamily;
      private Common lstCollArrearsIState;
      private Common lstCollArrearsIFamily;
      private Common lstOwedCurrentState;
      private Common lstOwedCurrentFamily;
      private Common lstOwedCurrentIState;
      private Common lstOwedCurrentIFamily;
      private Common lstOwedArrearsState;
      private Common lstOwedArrearsFamily;
      private Common lstOwedArrearsIState;
      private Common lstOwedArrearsIFamily;
      private Common lstOwedCurrTotal;
      private Common lstOwedArrearsTotal;
      private Common lstOwedTotal;
      private Common lstCollCurrTotal;
      private Common lstCollArrearsTotal;
      private Common lstCollectedTotal;
    }

    /// <summary>
    /// Gets a value of CountyLevel.
    /// </summary>
    [JsonIgnore]
    public Array<CountyLevelGroup> CountyLevel => countyLevel ??= new(
      CountyLevelGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of CountyLevel for json serialization.
    /// </summary>
    [JsonPropertyName("countyLevel")]
    [Computed]
    public IList<CountyLevelGroup> CountyLevel_Json
    {
      get => countyLevel;
      set => CountyLevel.Assign(value);
    }

    /// <summary>
    /// Gets a value of JdLevel.
    /// </summary>
    [JsonIgnore]
    public Array<JdLevelGroup> JdLevel => jdLevel ??= new(
      JdLevelGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of JdLevel for json serialization.
    /// </summary>
    [JsonPropertyName("jdLevel")]
    [Computed]
    public IList<JdLevelGroup> JdLevel_Json
    {
      get => jdLevel;
      set => JdLevel.Assign(value);
    }

    /// <summary>
    /// Gets a value of StateLevel.
    /// </summary>
    [JsonIgnore]
    public Array<StateLevelGroup> StateLevel => stateLevel ??= new(
      StateLevelGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of StateLevel for json serialization.
    /// </summary>
    [JsonPropertyName("stateLevel")]
    [Computed]
    public IList<StateLevelGroup> StateLevel_Json
    {
      get => stateLevel;
      set => StateLevel.Assign(value);
    }

    /// <summary>
    /// A value of ReportLevel.
    /// </summary>
    [JsonPropertyName("reportLevel")]
    public WorkArea ReportLevel
    {
      get => reportLevel ??= new();
      set => reportLevel = value;
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
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of SaveTribunal.
    /// </summary>
    [JsonPropertyName("saveTribunal")]
    public Tribunal SaveTribunal
    {
      get => saveTribunal ??= new();
      set => saveTribunal = value;
    }

    /// <summary>
    /// A value of SaveFips.
    /// </summary>
    [JsonPropertyName("saveFips")]
    public Fips SaveFips
    {
      get => saveFips ??= new();
      set => saveFips = value;
    }

    /// <summary>
    /// A value of PageNumber.
    /// </summary>
    [JsonPropertyName("pageNumber")]
    public Common PageNumber
    {
      get => pageNumber ??= new();
      set => pageNumber = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of TextDate.
    /// </summary>
    [JsonPropertyName("textDate")]
    public WorkArea TextDate
    {
      get => textDate ??= new();
      set => textDate = value;
    }

    /// <summary>
    /// A value of TextMm.
    /// </summary>
    [JsonPropertyName("textMm")]
    public TextWorkArea TextMm
    {
      get => textMm ??= new();
      set => textMm = value;
    }

    /// <summary>
    /// A value of TextDd.
    /// </summary>
    [JsonPropertyName("textDd")]
    public TextWorkArea TextDd
    {
      get => textDd ??= new();
      set => textDd = value;
    }

    /// <summary>
    /// A value of TextYyyy.
    /// </summary>
    [JsonPropertyName("textYyyy")]
    public TextWorkArea TextYyyy
    {
      get => textYyyy ??= new();
      set => textYyyy = value;
    }

    /// <summary>
    /// A value of NeededToOpen.
    /// </summary>
    [JsonPropertyName("neededToOpen")]
    public EabReportSend NeededToOpen
    {
      get => neededToOpen ??= new();
      set => neededToOpen = value;
    }

    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of Process.
    /// </summary>
    [JsonPropertyName("process")]
    public DateWorkArea Process
    {
      get => process ??= new();
      set => process = value;
    }

    /// <summary>
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
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
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DateWorkArea Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    /// <summary>
    /// A value of Bom.
    /// </summary>
    [JsonPropertyName("bom")]
    public DateWorkArea Bom
    {
      get => bom ??= new();
      set => bom = value;
    }

    /// <summary>
    /// A value of Eom.
    /// </summary>
    [JsonPropertyName("eom")]
    public DateWorkArea Eom
    {
      get => eom ??= new();
      set => eom = value;
    }

    /// <summary>
    /// A value of MonthName.
    /// </summary>
    [JsonPropertyName("monthName")]
    public WorkArea MonthName
    {
      get => monthName ??= new();
      set => monthName = value;
    }

    private Array<CountyLevelGroup> countyLevel;
    private Array<JdLevelGroup> jdLevel;
    private Array<StateLevelGroup> stateLevel;
    private WorkArea reportLevel;
    private Tribunal tribunal;
    private Fips fips;
    private Tribunal saveTribunal;
    private Fips saveFips;
    private Common pageNumber;
    private EabFileHandling eabFileHandling;
    private WorkArea textDate;
    private TextWorkArea textMm;
    private TextWorkArea textDd;
    private TextWorkArea textYyyy;
    private EabReportSend neededToOpen;
    private ProgramProcessingInfo programProcessingInfo;
    private DateWorkArea process;
    private EabReportSend eabReportSend;
    private DateWorkArea max;
    private DateWorkArea initialized;
    private DateWorkArea bom;
    private DateWorkArea eom;
    private WorkArea monthName;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public JobRun Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    private JobRun existing;
  }
#endregion
}
