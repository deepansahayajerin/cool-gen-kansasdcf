// Program: FN_B622_CREATE_REPORT, ID: 373457553, model: 746.
// Short name: SWE00215
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B622_CREATE_REPORT.
/// </summary>
[Serializable]
public partial class FnB622CreateReport: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B622_CREATE_REPORT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB622CreateReport(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB622CreateReport.
  /// </summary>
  public FnB622CreateReport(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ** SWCOAMX - Arun Changed on 01-03-2008 the transalation of the 
    // Abbrevaition **
    // ** SWCOAMX - Arun Changed on 05-28-2008 to calculate the State Collection
    // percentage correctly by multiplying 100 **
    local.CommasRequired.Flag = "Y";
    local.EabFileHandling.Action = "WRITE";
    local.LineOfDashes.RptDetail =
      "                      -------------  -------------  -------------  -------------";
      

    local.Report.Index = 0;
    local.Report.CheckSize();

    local.Report.Update.EabReportSend.RptDetail = " SRRUN183" + Substring
      (local.Null1.LineText, ReportData.LineText_MaxLength, 1, 31) + "STATE OF KANSAS" +
      Substring(local.Null1.LineText, ReportData.LineText_MaxLength, 1, 18) + "PAGE:"
      + NumberToString(import.PageNumber.Count, 13, 3);

    ++local.Report.Index;
    local.Report.CheckSize();

    local.Report.Update.EabReportSend.RptDetail = " RUN DATE: " + import
      .TextDate.Text10 + Substring
      (local.Null1.LineText, ReportData.LineText_MaxLength, 1, 4) + "DEPARTMENT OF SOCIAL AND REHABILITATION SERVICES";
      

    ++local.Report.Index;
    local.Report.CheckSize();

    local.Report.Update.EabReportSend.RptDetail =
      Substring(local.Null1.LineText, ReportData.LineText_MaxLength, 1, 36) + "KAECSES - CHILD SUPPORT";
      

    ++local.Report.Index;
    local.Report.CheckSize();

    local.Report.Update.EabReportSend.RptDetail =
      Substring(local.Null1.LineText, ReportData.LineText_MaxLength, 1, 22) + "COLLECTION POTENTIAL For Month Ending " +
      TrimEnd(import.MonthName.Text9) + " " + import.TextDd.Text2 + " " + import
      .TextYyyy.Text4;

    ++local.Report.Index;
    local.Report.CheckSize();

    local.Report.Update.EabReportSend.RptDetail = "";

    switch(TrimEnd(import.Fips.StateAbbreviation))
    {
      case "XX":
        if (!Equal(import.ReportLevel1.Text5, "STATE"))
        {
          ++local.Report.Index;
          local.Report.CheckSize();

          // ** SWCOAMX Changed the translation **
          // *** Changes Begin Here ***
          local.Report.Update.EabReportSend.RptDetail = " INTERSTATE";

          // *** Changes End   Here ***
        }

        break;
      case "YY":
        if (!Equal(import.ReportLevel1.Text5, "STATE"))
        {
          ++local.Report.Index;
          local.Report.CheckSize();

          // ** SWCOAMX Changed the translation **
          // *** Changes Begin Here ***
          local.Report.Update.EabReportSend.RptDetail = " TRIBAL";

          // *** Changes End   Here ***
        }

        break;
      case "ZZ":
        if (!Equal(import.ReportLevel1.Text5, "STATE"))
        {
          ++local.Report.Index;
          local.Report.CheckSize();

          local.Report.Update.EabReportSend.RptDetail = " OUT OF COUNTRY";
        }

        break;
      default:
        if (ReadFips())
        {
          local.Fips.StateDescription = entities.Fips.StateDescription;
        }
        else
        {
          local.Fips.StateDescription = "NOT FOUND";
        }

        // : Don't print headings for control break levels below the level for 
        // which the report
        //   is being generated.
        ++local.Report.Index;
        local.Report.CheckSize();

        local.Report.Update.EabReportSend.RptDetail = " State:             " + (
          local.Fips.StateDescription ?? "") + "";

        if (!Equal(import.ReportLevel1.Text5, "STATE"))
        {
          ++local.Report.Index;
          local.Report.CheckSize();

          local.Report.Update.EabReportSend.RptDetail =
            " Judicial District: " + import.Tribunal.JudicialDistrict + "";
        }

        if (Equal(import.ReportLevel1.Text5, "CNTY"))
        {
          ++local.Report.Index;
          local.Report.CheckSize();

          local.Report.Update.EabReportSend.RptDetail =
            " County:            " + entities.Fips.CountyDescription + "";
        }

        break;
    }

    ++local.Report.Index;
    local.Report.CheckSize();

    local.Report.Update.EabReportSend.RptDetail = "";

    ++local.Report.Index;
    local.Report.CheckSize();

    local.Report.Update.EabReportSend.RptDetail =
      "                       SRS ATTORNEY     K-ATTORNEY   NOT REFERRED          TOTAL";
      

    ++local.Report.Index;
    local.Report.CheckSize();

    local.Report.Update.EabReportSend.RptDetail = "";

    // *** TOTAL ORDERS REFERRED REPORT LINE
    local.NoCents.Flag = "Y";

    import.ReferralType.Index = 0;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText1();

    import.ReferralType.Index = 1;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText4();

    import.ReferralType.Index = 2;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText7();

    import.ReferralType.Index = 3;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText10();

    ++local.Report.Index;
    local.Report.CheckSize();

    local.Report.Update.EabReportSend.RptDetail = " Total Orders Referred" + local
      .ReportField1.Text13 + "  " + local.ReportField2.Text13 + "  " + local
      .ReportField3.Text13 + "  " + local.ReportField4.Text13;

    // *** TOTAL OWED REPORT LINE
    import.ReferralType.Index = 0;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText35();

    import.ReferralType.Index = 1;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText57();

    import.ReferralType.Index = 2;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText79();

    import.ReferralType.Index = 3;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText101();

    ++local.Report.Index;
    local.Report.CheckSize();

    local.Report.Update.EabReportSend.RptDetail = " Total Owed           " + local
      .ReportField1.Text14 + " " + local.ReportField2.Text14 + " " + local
      .ReportField3.Text14 + " " + local.ReportField4.Text14;

    // *** TOTAL COLLECTIONS REPORT LINE
    import.ReferralType.Index = 0;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText38();

    import.ReferralType.Index = 1;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText60();

    import.ReferralType.Index = 2;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText82();

    import.ReferralType.Index = 3;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText104();

    ++local.Report.Index;
    local.Report.CheckSize();

    local.Report.Update.EabReportSend.RptDetail = " Total Collections    " + local
      .ReportField1.Text14 + " " + local.ReportField2.Text14 + " " + local
      .ReportField3.Text14 + " " + local.ReportField4.Text14;

    // *** PERCENT CURRENT COLLECTED REPORT LINE
    local.NoCents.Flag = "Y";

    import.ReferralType.Index = 0;
    import.ReferralType.CheckSize();

    if (import.ReferralType.Item.IowedCurrTotal.TotalCurrency > 0 && import
      .ReferralType.Item.IcollCurrTotal.TotalCurrency > 0)
    {
      local.CalcField1.TotalCurrency =
        Math.Round(
          import.ReferralType.Item.IcollCurrTotal.TotalCurrency / import
        .ReferralType.Item.IowedCurrTotal.TotalCurrency *
        100, 2, MidpointRounding.AwayFromZero);
    }
    else
    {
      local.CalcField1.TotalCurrency = 0;
    }

    UseFnCabCurrencyToText13();

    import.ReferralType.Index = 1;
    import.ReferralType.CheckSize();

    if (import.ReferralType.Item.IowedCurrTotal.TotalCurrency > 0 && import
      .ReferralType.Item.IcollCurrTotal.TotalCurrency > 0)
    {
      local.CalcField2.TotalCurrency =
        Math.Round(
          import.ReferralType.Item.IcollCurrTotal.TotalCurrency / import
        .ReferralType.Item.IowedCurrTotal.TotalCurrency *
        100, 2, MidpointRounding.AwayFromZero);
    }
    else
    {
      local.CalcField2.TotalCurrency = 0;
    }

    UseFnCabCurrencyToText14();

    import.ReferralType.Index = 2;
    import.ReferralType.CheckSize();

    if (import.ReferralType.Item.IowedCurrTotal.TotalCurrency > 0 && import
      .ReferralType.Item.IcollCurrTotal.TotalCurrency > 0)
    {
      local.CalcField3.TotalCurrency =
        Math.Round(
          import.ReferralType.Item.IcollCurrTotal.TotalCurrency / import
        .ReferralType.Item.IowedCurrTotal.TotalCurrency *
        100, 2, MidpointRounding.AwayFromZero);
    }
    else
    {
      local.CalcField3.TotalCurrency = 0;
    }

    UseFnCabCurrencyToText15();

    import.ReferralType.Index = 3;
    import.ReferralType.CheckSize();

    if (import.ReferralType.Item.IowedCurrTotal.TotalCurrency > 0 && import
      .ReferralType.Item.IcollCurrTotal.TotalCurrency > 0)
    {
      local.CalcField4.TotalCurrency =
        Math.Round(
          import.ReferralType.Item.IcollCurrTotal.TotalCurrency / import
        .ReferralType.Item.IowedCurrTotal.TotalCurrency *
        100, 2, MidpointRounding.AwayFromZero);
    }
    else
    {
      local.CalcField4.TotalCurrency = 0;
    }

    UseFnCabCurrencyToText16();

    ++local.Report.Index;
    local.Report.CheckSize();

    local.Report.Update.EabReportSend.RptDetail = " % Current Coll       " + local
      .ReportField1.Text13 + "  " + local.ReportField2.Text13 + "  " + local
      .ReportField3.Text13 + "  " + local.ReportField4.Text13;

    // *** PERCENT CURRENT COLLECTED REPORT LINE - STATE
    import.ReferralType.Index = 0;
    import.ReferralType.CheckSize();

    if (import.ReferralType.Item.IowedCurrentState.TotalCurrency > 0 && import
      .ReferralType.Item.IcollCurrentState.TotalCurrency > 0)
    {
      // *** Added the code to Multiply 100 **
      local.CalcField1.TotalCurrency =
        Math.Round(
          import.ReferralType.Item.IcollCurrentState.TotalCurrency / import
        .ReferralType.Item.IowedCurrentState.TotalCurrency *
        100, 2, MidpointRounding.AwayFromZero);
    }
    else
    {
      local.CalcField1.TotalCurrency = 0;
    }

    UseFnCabCurrencyToText13();

    import.ReferralType.Index = 1;
    import.ReferralType.CheckSize();

    if (import.ReferralType.Item.IowedCurrentState.TotalCurrency > 0 && import
      .ReferralType.Item.IcollCurrentState.TotalCurrency > 0)
    {
      local.CalcField2.TotalCurrency =
        Math.Round(
          import.ReferralType.Item.IcollCurrentState.TotalCurrency / import
        .ReferralType.Item.IowedCurrentState.TotalCurrency *
        100, 2, MidpointRounding.AwayFromZero);
    }
    else
    {
      local.CalcField2.TotalCurrency = 0;
    }

    UseFnCabCurrencyToText14();

    import.ReferralType.Index = 2;
    import.ReferralType.CheckSize();

    if (import.ReferralType.Item.IowedCurrentState.TotalCurrency > 0 && import
      .ReferralType.Item.IcollCurrentState.TotalCurrency > 0)
    {
      local.CalcField3.TotalCurrency =
        Math.Round(
          import.ReferralType.Item.IcollCurrentState.TotalCurrency / import
        .ReferralType.Item.IowedCurrentState.TotalCurrency *
        100, 2, MidpointRounding.AwayFromZero);
    }
    else
    {
      local.CalcField3.TotalCurrency = 0;
    }

    UseFnCabCurrencyToText15();

    import.ReferralType.Index = 3;
    import.ReferralType.CheckSize();

    if (import.ReferralType.Item.IowedCurrentState.TotalCurrency > 0 && import
      .ReferralType.Item.IcollCurrentState.TotalCurrency > 0)
    {
      local.CalcField4.TotalCurrency =
        Math.Round(
          import.ReferralType.Item.IcollCurrentState.TotalCurrency / import
        .ReferralType.Item.IowedCurrentState.TotalCurrency *
        100, 2, MidpointRounding.AwayFromZero);
    }
    else
    {
      local.CalcField4.TotalCurrency = 0;
    }

    UseFnCabCurrencyToText16();

    ++local.Report.Index;
    local.Report.CheckSize();

    local.Report.Update.EabReportSend.RptDetail = "         % State      " + local
      .ReportField1.Text13 + "  " + local.ReportField2.Text13 + "  " + local
      .ReportField3.Text13 + "  " + local.ReportField4.Text13;

    // *** PERCENT CURRENT COLLECTED REPORT LINE - FAMILY.
    import.ReferralType.Index = 0;
    import.ReferralType.CheckSize();

    if (import.ReferralType.Item.IowedCurrentFamily.TotalCurrency > 0 && import
      .ReferralType.Item.IcollCurrentFamily.TotalCurrency > 0)
    {
      local.CalcField1.TotalCurrency =
        Math.Round(
          import.ReferralType.Item.IcollCurrentFamily.TotalCurrency / import
        .ReferralType.Item.IowedCurrentFamily.TotalCurrency *
        100, 2, MidpointRounding.AwayFromZero);
    }
    else
    {
      local.CalcField1.TotalCurrency = 0;
    }

    UseFnCabCurrencyToText13();

    import.ReferralType.Index = 1;
    import.ReferralType.CheckSize();

    if (import.ReferralType.Item.IowedCurrentFamily.TotalCurrency > 0 && import
      .ReferralType.Item.IcollCurrentFamily.TotalCurrency > 0)
    {
      local.CalcField2.TotalCurrency =
        Math.Round(
          import.ReferralType.Item.IcollCurrentFamily.TotalCurrency / import
        .ReferralType.Item.IowedCurrentFamily.TotalCurrency *
        100, 2, MidpointRounding.AwayFromZero);
    }
    else
    {
      local.CalcField2.TotalCurrency = 0;
    }

    UseFnCabCurrencyToText14();

    import.ReferralType.Index = 2;
    import.ReferralType.CheckSize();

    if (import.ReferralType.Item.IowedCurrentFamily.TotalCurrency > 0 && import
      .ReferralType.Item.IcollCurrentFamily.TotalCurrency > 0)
    {
      local.CalcField3.TotalCurrency =
        Math.Round(
          import.ReferralType.Item.IcollCurrentFamily.TotalCurrency / import
        .ReferralType.Item.IowedCurrentFamily.TotalCurrency *
        100, 2, MidpointRounding.AwayFromZero);
    }
    else
    {
      local.CalcField3.TotalCurrency = 0;
    }

    UseFnCabCurrencyToText15();

    import.ReferralType.Index = 3;
    import.ReferralType.CheckSize();

    if (import.ReferralType.Item.IowedCurrentFamily.TotalCurrency > 0 && import
      .ReferralType.Item.IcollCurrentFamily.TotalCurrency > 0)
    {
      local.CalcField4.TotalCurrency =
        Math.Round(
          import.ReferralType.Item.IcollCurrentFamily.TotalCurrency / import
        .ReferralType.Item.IowedCurrentFamily.TotalCurrency *
        100, 2, MidpointRounding.AwayFromZero);
    }
    else
    {
      local.CalcField4.TotalCurrency = 0;
    }

    UseFnCabCurrencyToText16();

    ++local.Report.Index;
    local.Report.CheckSize();

    local.Report.Update.EabReportSend.RptDetail = "         % Family     " + local
      .ReportField1.Text13 + "  " + local.ReportField2.Text13 + "  " + local
      .ReportField3.Text13 + "  " + local.ReportField4.Text13;

    // *** PERCENT CURRENT COLLECTED REPORT LINE - INTERSTATE
    import.ReferralType.Index = 0;
    import.ReferralType.CheckSize();

    if (import.ReferralType.Item.IowedCurrentIState.TotalCurrency > 0 && import
      .ReferralType.Item.IcollCurrentIState.TotalCurrency > 0)
    {
      local.CalcField1.TotalCurrency =
        Math.Round(
          import.ReferralType.Item.IcollCurrentIState.TotalCurrency / import
        .ReferralType.Item.IowedCurrentIState.TotalCurrency *
        100, 2, MidpointRounding.AwayFromZero);
    }
    else
    {
      local.CalcField1.TotalCurrency = 0;
    }

    UseFnCabCurrencyToText13();

    import.ReferralType.Index = 1;
    import.ReferralType.CheckSize();

    if (import.ReferralType.Item.IowedCurrentIState.TotalCurrency > 0 && import
      .ReferralType.Item.IcollCurrentIState.TotalCurrency > 0)
    {
      local.CalcField2.TotalCurrency =
        Math.Round(
          import.ReferralType.Item.IcollCurrentIState.TotalCurrency / import
        .ReferralType.Item.IowedCurrentIState.TotalCurrency *
        100, 2, MidpointRounding.AwayFromZero);
    }
    else
    {
      local.CalcField2.TotalCurrency = 0;
    }

    UseFnCabCurrencyToText14();

    import.ReferralType.Index = 2;
    import.ReferralType.CheckSize();

    if (import.ReferralType.Item.IowedCurrentIState.TotalCurrency > 0 && import
      .ReferralType.Item.IcollCurrentIState.TotalCurrency > 0)
    {
      local.CalcField3.TotalCurrency =
        Math.Round(
          import.ReferralType.Item.IcollCurrentIState.TotalCurrency / import
        .ReferralType.Item.IowedCurrentIState.TotalCurrency *
        100, 2, MidpointRounding.AwayFromZero);
    }
    else
    {
      local.CalcField3.TotalCurrency = 0;
    }

    UseFnCabCurrencyToText15();

    import.ReferralType.Index = 3;
    import.ReferralType.CheckSize();

    if (import.ReferralType.Item.IowedCurrentIState.TotalCurrency > 0 && import
      .ReferralType.Item.IcollCurrentIState.TotalCurrency > 0)
    {
      local.CalcField4.TotalCurrency =
        Math.Round(
          import.ReferralType.Item.IcollCurrentIState.TotalCurrency / import
        .ReferralType.Item.IowedCurrentIState.TotalCurrency *
        100, 2, MidpointRounding.AwayFromZero);
    }
    else
    {
      local.CalcField4.TotalCurrency = 0;
    }

    UseFnCabCurrencyToText16();

    ++local.Report.Index;
    local.Report.CheckSize();

    local.Report.Update.EabReportSend.RptDetail = "         % I-State    " + local
      .ReportField1.Text13 + "  " + local.ReportField2.Text13 + "  " + local
      .ReportField3.Text13 + "  " + local.ReportField4.Text13;

    // *** PERCENT CURRENT COLLECTED REPORT LINE - INTERSTATE FAMILY.
    import.ReferralType.Index = 0;
    import.ReferralType.CheckSize();

    if (import.ReferralType.Item.IowedCurrentIFamily.TotalCurrency > 0 && import
      .ReferralType.Item.IcollCurrentIFamily.TotalCurrency > 0)
    {
      local.CalcField1.TotalCurrency =
        Math.Round(
          import.ReferralType.Item.IcollCurrentIFamily.TotalCurrency / import
        .ReferralType.Item.IowedCurrentIFamily.TotalCurrency *
        100, 2, MidpointRounding.AwayFromZero);
    }
    else
    {
      local.CalcField1.TotalCurrency = 0;
    }

    UseFnCabCurrencyToText13();

    import.ReferralType.Index = 1;
    import.ReferralType.CheckSize();

    if (import.ReferralType.Item.IowedCurrentIFamily.TotalCurrency > 0 && import
      .ReferralType.Item.IcollCurrentIFamily.TotalCurrency > 0)
    {
      local.CalcField2.TotalCurrency =
        Math.Round(
          import.ReferralType.Item.IcollCurrentIFamily.TotalCurrency / import
        .ReferralType.Item.IowedCurrentIFamily.TotalCurrency *
        100, 2, MidpointRounding.AwayFromZero);
    }
    else
    {
      local.CalcField2.TotalCurrency = 0;
    }

    UseFnCabCurrencyToText14();

    import.ReferralType.Index = 2;
    import.ReferralType.CheckSize();

    if (import.ReferralType.Item.IowedCurrentIFamily.TotalCurrency > 0 && import
      .ReferralType.Item.IcollCurrentIFamily.TotalCurrency > 0)
    {
      local.CalcField3.TotalCurrency =
        Math.Round(
          import.ReferralType.Item.IcollCurrentIFamily.TotalCurrency / import
        .ReferralType.Item.IowedCurrentIFamily.TotalCurrency *
        100, 2, MidpointRounding.AwayFromZero);
    }
    else
    {
      local.CalcField3.TotalCurrency = 0;
    }

    UseFnCabCurrencyToText15();

    import.ReferralType.Index = 3;
    import.ReferralType.CheckSize();

    if (import.ReferralType.Item.IowedCurrentIFamily.TotalCurrency > 0 && import
      .ReferralType.Item.IcollCurrentIFamily.TotalCurrency > 0)
    {
      local.CalcField4.TotalCurrency =
        Math.Round(
          import.ReferralType.Item.IcollCurrentIFamily.TotalCurrency / import
        .ReferralType.Item.IowedCurrentIFamily.TotalCurrency *
        100, 2, MidpointRounding.AwayFromZero);
    }
    else
    {
      local.CalcField4.TotalCurrency = 0;
    }

    UseFnCabCurrencyToText16();

    ++local.Report.Index;
    local.Report.CheckSize();

    local.Report.Update.EabReportSend.RptDetail = "         % I-Family   " + local
      .ReportField1.Text13 + "  " + local.ReportField2.Text13 + "  " + local
      .ReportField3.Text13 + "  " + local.ReportField4.Text13;

    // *** NUMBER OF PAYING ORDERS.
    import.ReferralType.Index = 0;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText2();

    import.ReferralType.Index = 1;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText5();

    import.ReferralType.Index = 2;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText8();

    import.ReferralType.Index = 3;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText11();

    ++local.Report.Index;
    local.Report.CheckSize();

    local.Report.Update.EabReportSend.RptDetail = " # Paying Orders      " + local
      .ReportField1.Text13 + "  " + local.ReportField2.Text13 + "  " + local
      .ReportField3.Text13 + "  " + local.ReportField4.Text13;

    // *** AVERAGE PER PAYING ORDER.
    import.ReferralType.Index = 0;
    import.ReferralType.CheckSize();

    if (import.ReferralType.Item.InumberOfPayingOrders.Count > 0)
    {
      local.CalcField1.TotalCurrency =
        Math.Round(
          import.ReferralType.Item.IcollectedTotal.TotalCurrency /
        import.ReferralType.Item.InumberOfPayingOrders.Count, 2,
        MidpointRounding.AwayFromZero);
    }
    else
    {
      local.CalcField1.TotalCurrency = 0;
    }

    UseFnCabCurrencyToText105();

    import.ReferralType.Index = 1;
    import.ReferralType.CheckSize();

    if (import.ReferralType.Item.InumberOfPayingOrders.Count > 0)
    {
      local.CalcField2.TotalCurrency =
        Math.Round(
          import.ReferralType.Item.IcollectedTotal.TotalCurrency /
        import.ReferralType.Item.InumberOfPayingOrders.Count, 2,
        MidpointRounding.AwayFromZero);
    }
    else
    {
      local.CalcField2.TotalCurrency = 0;
    }

    UseFnCabCurrencyToText106();

    import.ReferralType.Index = 2;
    import.ReferralType.CheckSize();

    if (import.ReferralType.Item.InumberOfPayingOrders.Count > 0)
    {
      local.CalcField3.TotalCurrency =
        Math.Round(
          import.ReferralType.Item.IcollectedTotal.TotalCurrency /
        import.ReferralType.Item.InumberOfPayingOrders.Count, 2,
        MidpointRounding.AwayFromZero);
    }
    else
    {
      local.CalcField3.TotalCurrency = 0;
    }

    UseFnCabCurrencyToText107();

    import.ReferralType.Index = 3;
    import.ReferralType.CheckSize();

    if (import.ReferralType.Item.InumberOfPayingOrders.Count > 0)
    {
      local.CalcField4.TotalCurrency =
        Math.Round(
          import.ReferralType.Item.IcollectedTotal.TotalCurrency /
        import.ReferralType.Item.InumberOfPayingOrders.Count, 2,
        MidpointRounding.AwayFromZero);
    }
    else
    {
      local.CalcField4.TotalCurrency = 0;
    }

    UseFnCabCurrencyToText108();

    ++local.Report.Index;
    local.Report.CheckSize();

    local.Report.Update.EabReportSend.RptDetail = " Average/Paying Order  " + local
      .ReportField1.Text13 + "  " + local.ReportField2.Text13 + "  " + local
      .ReportField3.Text13 + "  " + local.ReportField4.Text13;

    // *** PERCENTAGE OF PAYING ORDERS.
    import.ReferralType.Index = 0;
    import.ReferralType.CheckSize();

    if (import.ReferralType.Item.ItotalOrdersReferred.Count > 0)
    {
      local.CalcField1.TotalCurrency =
        Math.Round((decimal)import.ReferralType.Item.InumberOfPayingOrders.
          Count / import
        .ReferralType.Item.ItotalOrdersReferred.Count *
        100, 2, MidpointRounding.AwayFromZero);
    }
    else
    {
      local.CalcField1.TotalCurrency = 0;
    }

    UseFnCabCurrencyToText13();

    import.ReferralType.Index = 1;
    import.ReferralType.CheckSize();

    if (import.ReferralType.Item.ItotalOrdersReferred.Count > 0)
    {
      local.CalcField2.TotalCurrency =
        Math.Round((decimal)import.ReferralType.Item.InumberOfPayingOrders.
          Count / import
        .ReferralType.Item.ItotalOrdersReferred.Count *
        100, 2, MidpointRounding.AwayFromZero);
    }
    else
    {
      local.CalcField2.TotalCurrency = 0;
    }

    UseFnCabCurrencyToText14();

    import.ReferralType.Index = 2;
    import.ReferralType.CheckSize();

    if (import.ReferralType.Item.ItotalOrdersReferred.Count > 0)
    {
      local.CalcField3.TotalCurrency =
        Math.Round((decimal)import.ReferralType.Item.InumberOfPayingOrders.
          Count / import
        .ReferralType.Item.ItotalOrdersReferred.Count *
        100, 2, MidpointRounding.AwayFromZero);
    }
    else
    {
      local.CalcField3.TotalCurrency = 0;
    }

    UseFnCabCurrencyToText15();

    import.ReferralType.Index = 3;
    import.ReferralType.CheckSize();

    if (import.ReferralType.Item.ItotalOrdersReferred.Count > 0)
    {
      local.CalcField4.TotalCurrency =
        Math.Round((decimal)import.ReferralType.Item.InumberOfPayingOrders.
          Count / import
        .ReferralType.Item.ItotalOrdersReferred.Count *
        100, 2, MidpointRounding.AwayFromZero);
    }
    else
    {
      local.CalcField4.TotalCurrency = 0;
    }

    UseFnCabCurrencyToText16();

    ++local.Report.Index;
    local.Report.CheckSize();

    local.Report.Update.EabReportSend.RptDetail = " % Paying Orders      " + local
      .ReportField1.Text13 + "  " + local.ReportField2.Text13 + "  " + local
      .ReportField3.Text13 + "  " + local.ReportField4.Text13;

    // *** NUMBER OF ORDERS IN LOCATE STATUS.
    import.ReferralType.Index = 0;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText3();

    import.ReferralType.Index = 1;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText6();

    import.ReferralType.Index = 2;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText9();

    import.ReferralType.Index = 3;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText12();

    ++local.Report.Index;
    local.Report.CheckSize();

    local.Report.Update.EabReportSend.RptDetail = " # Orders in Locate   " + local
      .ReportField1.Text13 + "  " + local.ReportField2.Text13 + "  " + local
      .ReportField3.Text13 + "  " + local.ReportField4.Text13;

    // *** PERCENTAGE OF ORDERS IN LOCATE STATUS.
    import.ReferralType.Index = 0;
    import.ReferralType.CheckSize();

    if (import.ReferralType.Item.ItotalOrdersReferred.Count > 0)
    {
      local.CalcField1.TotalCurrency =
        Math.Round((decimal)import.ReferralType.Item.InumberOfOrdersInLocate.
          Count / import
        .ReferralType.Item.ItotalOrdersReferred.Count *
        100, 2, MidpointRounding.AwayFromZero);
    }
    else
    {
      local.CalcField1.TotalCurrency = 0;
    }

    UseFnCabCurrencyToText13();

    import.ReferralType.Index = 1;
    import.ReferralType.CheckSize();

    if (import.ReferralType.Item.ItotalOrdersReferred.Count > 0)
    {
      local.CalcField2.TotalCurrency =
        Math.Round((decimal)import.ReferralType.Item.InumberOfOrdersInLocate.
          Count / import
        .ReferralType.Item.ItotalOrdersReferred.Count *
        100, 2, MidpointRounding.AwayFromZero);
    }
    else
    {
      local.CalcField2.TotalCurrency = 0;
    }

    UseFnCabCurrencyToText14();

    import.ReferralType.Index = 2;
    import.ReferralType.CheckSize();

    if (import.ReferralType.Item.ItotalOrdersReferred.Count > 0)
    {
      local.CalcField3.TotalCurrency =
        Math.Round((decimal)import.ReferralType.Item.InumberOfOrdersInLocate.
          Count / import
        .ReferralType.Item.ItotalOrdersReferred.Count *
        100, 2, MidpointRounding.AwayFromZero);
    }
    else
    {
      local.CalcField3.TotalCurrency = 0;
    }

    UseFnCabCurrencyToText15();

    import.ReferralType.Index = 3;
    import.ReferralType.CheckSize();

    if (import.ReferralType.Item.ItotalOrdersReferred.Count > 0)
    {
      local.CalcField4.TotalCurrency =
        Math.Round((decimal)import.ReferralType.Item.InumberOfOrdersInLocate.
          Count / import
        .ReferralType.Item.ItotalOrdersReferred.Count *
        100, 2, MidpointRounding.AwayFromZero);
    }
    else
    {
      local.CalcField4.TotalCurrency = 0;
    }

    UseFnCabCurrencyToText16();

    ++local.Report.Index;
    local.Report.CheckSize();

    local.Report.Update.EabReportSend.RptDetail = " % Orders in Locate   " + local
      .ReportField1.Text13 + "  " + local.ReportField2.Text13 + "  " + local
      .ReportField3.Text13 + "  " + local.ReportField4.Text13;

    ++local.Report.Index;
    local.Report.CheckSize();

    local.Report.Update.EabReportSend.RptDetail = "";

    ++local.Report.Index;
    local.Report.CheckSize();

    local.Report.Update.EabReportSend.RptDetail = " OWED";

    // *** CURRENT OWED - STATE
    import.ReferralType.Index = 0;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText25();

    import.ReferralType.Index = 1;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText47();

    import.ReferralType.Index = 2;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText69();

    import.ReferralType.Index = 3;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText91();

    ++local.Report.Index;
    local.Report.CheckSize();

    local.Report.Update.EabReportSend.RptDetail = " Current State        " + local
      .ReportField1.Text14 + " " + local.ReportField2.Text14 + " " + local
      .ReportField3.Text14 + " " + local.ReportField4.Text14;

    // *** CURRENT OWED - FAMILY.
    import.ReferralType.Index = 0;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText26();

    import.ReferralType.Index = 1;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText48();

    import.ReferralType.Index = 2;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText70();

    import.ReferralType.Index = 3;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText92();

    ++local.Report.Index;
    local.Report.CheckSize();

    local.Report.Update.EabReportSend.RptDetail = " Current Family       " + local
      .ReportField1.Text14 + " " + local.ReportField2.Text14 + " " + local
      .ReportField3.Text14 + " " + local.ReportField4.Text14;

    // *** CURRENT OWED - INTERSTATE.
    import.ReferralType.Index = 0;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText27();

    import.ReferralType.Index = 1;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText49();

    import.ReferralType.Index = 2;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText71();

    import.ReferralType.Index = 3;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText93();

    ++local.Report.Index;
    local.Report.CheckSize();

    local.Report.Update.EabReportSend.RptDetail = " Current I-State      " + local
      .ReportField1.Text14 + " " + local.ReportField2.Text14 + " " + local
      .ReportField3.Text14 + " " + local.ReportField4.Text14;

    // *** CURRENT OWED - INTERSTATE FAMILY.
    import.ReferralType.Index = 0;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText28();

    import.ReferralType.Index = 1;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText50();

    import.ReferralType.Index = 2;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText72();

    import.ReferralType.Index = 3;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText94();

    ++local.Report.Index;
    local.Report.CheckSize();

    local.Report.Update.EabReportSend.RptDetail = " Current I-Family     " + local
      .ReportField1.Text14 + " " + local.ReportField2.Text14 + " " + local
      .ReportField3.Text14 + " " + local.ReportField4.Text14;

    // *** CURRENT OWED - TOTAL
    import.ReferralType.Index = 0;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText33();

    import.ReferralType.Index = 1;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText55();

    import.ReferralType.Index = 2;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText77();

    import.ReferralType.Index = 3;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText99();

    ++local.Report.Index;
    local.Report.CheckSize();

    local.Report.Update.EabReportSend.RptDetail = " CURRENT TOTAL        " + local
      .ReportField1.Text14 + " " + local.ReportField2.Text14 + " " + local
      .ReportField3.Text14 + " " + local.ReportField4.Text14;

    ++local.Report.Index;
    local.Report.CheckSize();

    local.Report.Update.EabReportSend.RptDetail = "";

    // *** ARREARS OWED - STATE
    import.ReferralType.Index = 0;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText29();

    import.ReferralType.Index = 1;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText51();

    import.ReferralType.Index = 2;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText73();

    import.ReferralType.Index = 3;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText95();

    ++local.Report.Index;
    local.Report.CheckSize();

    local.Report.Update.EabReportSend.RptDetail = " Arrears State        " + local
      .ReportField1.Text14 + " " + local.ReportField2.Text14 + " " + local
      .ReportField3.Text14 + " " + local.ReportField4.Text14;

    // *** ARREARS OWED - FAMILY.
    import.ReferralType.Index = 0;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText30();

    import.ReferralType.Index = 1;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText52();

    import.ReferralType.Index = 2;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText74();

    import.ReferralType.Index = 3;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText96();

    ++local.Report.Index;
    local.Report.CheckSize();

    local.Report.Update.EabReportSend.RptDetail = " Arrears Family       " + local
      .ReportField1.Text14 + " " + local.ReportField2.Text14 + " " + local
      .ReportField3.Text14 + " " + local.ReportField4.Text14;

    // *** ARREARS OWED - INTERSTATE.
    import.ReferralType.Index = 0;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText31();

    import.ReferralType.Index = 1;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText53();

    import.ReferralType.Index = 2;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText75();

    import.ReferralType.Index = 3;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText97();

    ++local.Report.Index;
    local.Report.CheckSize();

    local.Report.Update.EabReportSend.RptDetail = " Arrears I-State      " + local
      .ReportField1.Text14 + " " + local.ReportField2.Text14 + " " + local
      .ReportField3.Text14 + " " + local.ReportField4.Text14;

    // *** ARREARS OWED - INTERSTATE FAMILY.
    import.ReferralType.Index = 0;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText32();

    import.ReferralType.Index = 1;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText54();

    import.ReferralType.Index = 2;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText76();

    import.ReferralType.Index = 3;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText98();

    ++local.Report.Index;
    local.Report.CheckSize();

    local.Report.Update.EabReportSend.RptDetail = " Arrears I-Family     " + local
      .ReportField1.Text14 + " " + local.ReportField2.Text14 + " " + local
      .ReportField3.Text14 + " " + local.ReportField4.Text14;

    // *** ARREARS OWED - TOTAL
    import.ReferralType.Index = 0;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText34();

    import.ReferralType.Index = 1;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText56();

    import.ReferralType.Index = 2;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText78();

    import.ReferralType.Index = 3;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText100();

    ++local.Report.Index;
    local.Report.CheckSize();

    local.Report.Update.EabReportSend.RptDetail = " ARREARS TOTAL        " + local
      .ReportField1.Text14 + " " + local.ReportField2.Text14 + " " + local
      .ReportField3.Text14 + " " + local.ReportField4.Text14;

    ++local.Report.Index;
    local.Report.CheckSize();

    local.Report.Update.EabReportSend.RptDetail = local.LineOfDashes.RptDetail;

    // *** OWED - TOTAL
    import.ReferralType.Index = 0;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText35();

    import.ReferralType.Index = 1;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText57();

    import.ReferralType.Index = 2;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText79();

    import.ReferralType.Index = 3;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText101();

    ++local.Report.Index;
    local.Report.CheckSize();

    local.Report.Update.EabReportSend.RptDetail = " TOTAL OWED           " + local
      .ReportField1.Text14 + " " + local.ReportField2.Text14 + " " + local
      .ReportField3.Text14 + " " + local.ReportField4.Text14;

    ++local.Report.Index;
    local.Report.CheckSize();

    local.Report.Update.EabReportSend.RptDetail = "";

    ++local.Report.Index;
    local.Report.CheckSize();

    local.Report.Update.EabReportSend.RptDetail = " COLLECTED";

    // *** CURRENT COLLECTED - STATE
    import.ReferralType.Index = 0;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText17();

    import.ReferralType.Index = 1;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText39();

    import.ReferralType.Index = 2;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText61();

    import.ReferralType.Index = 3;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText83();

    ++local.Report.Index;
    local.Report.CheckSize();

    local.Report.Update.EabReportSend.RptDetail = " Current State        " + local
      .ReportField1.Text14 + " " + local.ReportField2.Text14 + " " + local
      .ReportField3.Text14 + " " + local.ReportField4.Text14;

    // *** CURRENT COLLECTED - FAMILY.
    import.ReferralType.Index = 0;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText18();

    import.ReferralType.Index = 1;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText40();

    import.ReferralType.Index = 2;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText62();

    import.ReferralType.Index = 3;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText84();

    ++local.Report.Index;
    local.Report.CheckSize();

    local.Report.Update.EabReportSend.RptDetail = " Current Family       " + local
      .ReportField1.Text14 + " " + local.ReportField2.Text14 + " " + local
      .ReportField3.Text14 + " " + local.ReportField4.Text14;

    // *** CURRENT COLLECTED - INTERSTATE.
    import.ReferralType.Index = 0;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText19();

    import.ReferralType.Index = 1;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText41();

    import.ReferralType.Index = 2;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText63();

    import.ReferralType.Index = 3;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText85();

    ++local.Report.Index;
    local.Report.CheckSize();

    local.Report.Update.EabReportSend.RptDetail = " Current I-State      " + local
      .ReportField1.Text14 + " " + local.ReportField2.Text14 + " " + local
      .ReportField3.Text14 + " " + local.ReportField4.Text14;

    // *** CURRENT COLLECTED - INTERSTATE FAMILY.
    import.ReferralType.Index = 0;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText20();

    import.ReferralType.Index = 1;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText42();

    import.ReferralType.Index = 2;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText64();

    import.ReferralType.Index = 3;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText86();

    ++local.Report.Index;
    local.Report.CheckSize();

    local.Report.Update.EabReportSend.RptDetail = " Current I-Family     " + local
      .ReportField1.Text14 + " " + local.ReportField2.Text14 + " " + local
      .ReportField3.Text14 + " " + local.ReportField4.Text14;

    // *** CURRENT COLLECTED - TOTAL
    import.ReferralType.Index = 0;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText36();

    import.ReferralType.Index = 1;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText58();

    import.ReferralType.Index = 2;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText80();

    import.ReferralType.Index = 3;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText102();

    ++local.Report.Index;
    local.Report.CheckSize();

    local.Report.Update.EabReportSend.RptDetail = " CURRENT TOTAL        " + local
      .ReportField1.Text14 + " " + local.ReportField2.Text14 + " " + local
      .ReportField3.Text14 + " " + local.ReportField4.Text14;

    ++local.Report.Index;
    local.Report.CheckSize();

    local.Report.Update.EabReportSend.RptDetail = "";

    // *** ARREARS COLLECTED - STATE
    import.ReferralType.Index = 0;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText21();

    import.ReferralType.Index = 1;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText43();

    import.ReferralType.Index = 2;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText65();

    import.ReferralType.Index = 3;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText87();

    ++local.Report.Index;
    local.Report.CheckSize();

    local.Report.Update.EabReportSend.RptDetail = " Arrears State        " + local
      .ReportField1.Text14 + " " + local.ReportField2.Text14 + " " + local
      .ReportField3.Text14 + " " + local.ReportField4.Text14;

    // *** ARREARS COLLECTED - FAMILY.
    import.ReferralType.Index = 0;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText22();

    import.ReferralType.Index = 1;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText44();

    import.ReferralType.Index = 2;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText66();

    import.ReferralType.Index = 3;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText88();

    ++local.Report.Index;
    local.Report.CheckSize();

    local.Report.Update.EabReportSend.RptDetail = " Arrears Family       " + local
      .ReportField1.Text14 + " " + local.ReportField2.Text14 + " " + local
      .ReportField3.Text14 + " " + local.ReportField4.Text14;

    // *** ARREARS COLLECTED - INTERSTATE.
    import.ReferralType.Index = 0;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText23();

    import.ReferralType.Index = 1;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText45();

    import.ReferralType.Index = 2;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText67();

    import.ReferralType.Index = 3;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText89();

    ++local.Report.Index;
    local.Report.CheckSize();

    local.Report.Update.EabReportSend.RptDetail = " Arrears I-State      " + local
      .ReportField1.Text14 + " " + local.ReportField2.Text14 + " " + local
      .ReportField3.Text14 + " " + local.ReportField4.Text14;

    // *** ARREARS COLLECTED - INTERSTATE FAMILY.
    import.ReferralType.Index = 0;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText24();

    import.ReferralType.Index = 1;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText46();

    import.ReferralType.Index = 2;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText68();

    import.ReferralType.Index = 3;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText90();

    ++local.Report.Index;
    local.Report.CheckSize();

    local.Report.Update.EabReportSend.RptDetail = " Arrears I-Family     " + local
      .ReportField1.Text14 + " " + local.ReportField2.Text14 + " " + local
      .ReportField3.Text14 + " " + local.ReportField4.Text14;

    // *** ARREARS COLLECTED - TOTAL
    import.ReferralType.Index = 0;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText37();

    import.ReferralType.Index = 1;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText59();

    import.ReferralType.Index = 2;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText81();

    import.ReferralType.Index = 3;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText103();

    ++local.Report.Index;
    local.Report.CheckSize();

    local.Report.Update.EabReportSend.RptDetail = " ARREARS TOTAL        " + local
      .ReportField1.Text14 + " " + local.ReportField2.Text14 + " " + local
      .ReportField3.Text14 + " " + local.ReportField4.Text14;

    // *** COLLECTED - TOTAL
    ++local.Report.Index;
    local.Report.CheckSize();

    local.Report.Update.EabReportSend.RptDetail = local.LineOfDashes.RptDetail;

    import.ReferralType.Index = 0;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText38();

    import.ReferralType.Index = 1;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText60();

    import.ReferralType.Index = 2;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText82();

    import.ReferralType.Index = 3;
    import.ReferralType.CheckSize();

    UseFnCabCurrencyToText104();

    ++local.Report.Index;
    local.Report.CheckSize();

    local.Report.Update.EabReportSend.RptDetail = " TOTAL COLLECTIONS    " + local
      .ReportField1.Text14 + " " + local.ReportField2.Text14 + " " + local
      .ReportField3.Text14 + " " + local.ReportField4.Text14;

    // *** WRITE THE REPORT FROM LOCAL GROUP VIEW OF REPORT LINES.
    // : 'WPAGE' tells the external to start a new page before writing.
    local.EabFileHandling.Action = "WPAGE";

    for(local.Report.Index = 0; local.Report.Index < local.Report.Count; ++
      local.Report.Index)
    {
      if (!local.Report.CheckSize())
      {
        break;
      }

      UseFnB622ExternalToWriteReport();
      local.EabFileHandling.Action = "WRITE";
    }

    local.Report.CheckIndex();

    // : If report level is higher than county, reset totals to zero.
    if (!Equal(import.ReportLevel1.Text5, "CNTY"))
    {
      export.ReportLevel.Update.EtotalOrdersReferred.Count = 0;
      export.ReportLevel.Update.EnumberOfOrdersInLocate.Count = 0;
      export.ReportLevel.Update.EnumberOfPayingOrders.Count = 0;
      export.ReportLevel.Update.EcollArrearsFamily.TotalCurrency = 0;
      export.ReportLevel.Update.EcollArrearsIFamily.TotalCurrency = 0;
      export.ReportLevel.Update.EcollArrearsIState.TotalCurrency = 0;
      export.ReportLevel.Update.EcollArrearsState.TotalCurrency = 0;
      export.ReportLevel.Update.EcollArrearsTotal.TotalCurrency = 0;
      export.ReportLevel.Update.EcollCurrTotal.TotalCurrency = 0;
      export.ReportLevel.Update.EcollCurrentFamily.TotalCurrency = 0;
      export.ReportLevel.Update.EcollCurrentIFamily.TotalCurrency = 0;
      export.ReportLevel.Update.EcollCurrentIState.TotalCurrency = 0;
      export.ReportLevel.Update.EcollCurrentState.TotalCurrency = 0;
      export.ReportLevel.Update.EcollectedTotal.TotalCurrency = 0;
      export.ReportLevel.Update.EowedArrearsFamily.TotalCurrency = 0;
      export.ReportLevel.Update.EowedArrearsIFamily.TotalCurrency = 0;
      export.ReportLevel.Update.EowedArrearsIState.TotalCurrency = 0;
      export.ReportLevel.Update.EowedArrearsState.TotalCurrency = 0;
      export.ReportLevel.Update.EowedArrearsTotal.TotalCurrency = 0;
      export.ReportLevel.Update.EowedCurrTotal.TotalCurrency = 0;
      export.ReportLevel.Update.EowedCurrentFamily.TotalCurrency = 0;
      export.ReportLevel.Update.EowedCurrentIFamily.TotalCurrency = 0;
      export.ReportLevel.Update.EowedCurrentIState.TotalCurrency = 0;
      export.ReportLevel.Update.EowedCurrentState.TotalCurrency = 0;
      export.ReportLevel.Update.EowedTotal.TotalCurrency = 0;
    }
  }

  private static void MoveWorkArea(WorkArea source, WorkArea target)
  {
    target.Text13 = source.Text13;
    target.Text14 = source.Text14;
  }

  private void UseFnB622ExternalToWriteReport()
  {
    var useImport = new FnB622ExternalToWriteReport.Import();
    var useExport = new FnB622ExternalToWriteReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.EabReportSend.RptDetail =
      local.Report.Item.EabReportSend.RptDetail;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(FnB622ExternalToWriteReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnCabCurrencyToText1()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.Count =
      import.ReferralType.Item.ItotalOrdersReferred.Count;
    useImport.ReturnAnInteger.Flag = local.NoCents.Flag;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField1);
  }

  private void UseFnCabCurrencyToText2()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.Count =
      import.ReferralType.Item.InumberOfPayingOrders.Count;
    useImport.ReturnAnInteger.Flag = local.NoCents.Flag;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField1);
  }

  private void UseFnCabCurrencyToText3()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.Count =
      import.ReferralType.Item.InumberOfOrdersInLocate.Count;
    useImport.ReturnAnInteger.Flag = local.NoCents.Flag;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField1);
  }

  private void UseFnCabCurrencyToText4()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.Count =
      import.ReferralType.Item.ItotalOrdersReferred.Count;
    useImport.ReturnAnInteger.Flag = local.NoCents.Flag;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField2);
  }

  private void UseFnCabCurrencyToText5()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.Count =
      import.ReferralType.Item.InumberOfPayingOrders.Count;
    useImport.ReturnAnInteger.Flag = local.NoCents.Flag;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField2);
  }

  private void UseFnCabCurrencyToText6()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.Count =
      import.ReferralType.Item.InumberOfOrdersInLocate.Count;
    useImport.ReturnAnInteger.Flag = local.NoCents.Flag;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField2);
  }

  private void UseFnCabCurrencyToText7()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.Count =
      import.ReferralType.Item.ItotalOrdersReferred.Count;
    useImport.ReturnAnInteger.Flag = local.NoCents.Flag;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField3);
  }

  private void UseFnCabCurrencyToText8()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.Count =
      import.ReferralType.Item.InumberOfPayingOrders.Count;
    useImport.ReturnAnInteger.Flag = local.NoCents.Flag;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField3);
  }

  private void UseFnCabCurrencyToText9()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.Count =
      import.ReferralType.Item.InumberOfOrdersInLocate.Count;
    useImport.ReturnAnInteger.Flag = local.NoCents.Flag;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField3);
  }

  private void UseFnCabCurrencyToText10()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.Count =
      import.ReferralType.Item.ItotalOrdersReferred.Count;
    useImport.ReturnAnInteger.Flag = local.NoCents.Flag;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField4);
  }

  private void UseFnCabCurrencyToText11()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.Count =
      import.ReferralType.Item.InumberOfPayingOrders.Count;
    useImport.ReturnAnInteger.Flag = local.NoCents.Flag;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField4);
  }

  private void UseFnCabCurrencyToText12()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.Count =
      import.ReferralType.Item.InumberOfOrdersInLocate.Count;
    useImport.ReturnAnInteger.Flag = local.NoCents.Flag;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField4);
  }

  private void UseFnCabCurrencyToText13()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.Common.TotalCurrency = local.CalcField1.TotalCurrency;
    useImport.ReturnAnInteger.Flag = local.NoCents.Flag;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField1);
  }

  private void UseFnCabCurrencyToText14()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.Common.TotalCurrency = local.CalcField2.TotalCurrency;
    useImport.ReturnAnInteger.Flag = local.NoCents.Flag;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField2);
  }

  private void UseFnCabCurrencyToText15()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.Common.TotalCurrency = local.CalcField3.TotalCurrency;
    useImport.ReturnAnInteger.Flag = local.NoCents.Flag;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField3);
  }

  private void UseFnCabCurrencyToText16()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.Common.TotalCurrency = local.CalcField4.TotalCurrency;
    useImport.ReturnAnInteger.Flag = local.NoCents.Flag;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField4);
  }

  private void UseFnCabCurrencyToText17()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IcollCurrentState.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField1);
  }

  private void UseFnCabCurrencyToText18()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IcollCurrentFamily.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField1);
  }

  private void UseFnCabCurrencyToText19()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IcollCurrentIState.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField1);
  }

  private void UseFnCabCurrencyToText20()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IcollCurrentIFamily.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField1);
  }

  private void UseFnCabCurrencyToText21()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IcollArrearsState.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField1);
  }

  private void UseFnCabCurrencyToText22()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IcollArrearsFamily.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField1);
  }

  private void UseFnCabCurrencyToText23()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IcollArrearsIState.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField1);
  }

  private void UseFnCabCurrencyToText24()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IcollArrearsIFamily.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField1);
  }

  private void UseFnCabCurrencyToText25()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IowedCurrentState.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField1);
  }

  private void UseFnCabCurrencyToText26()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IowedCurrentFamily.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField1);
  }

  private void UseFnCabCurrencyToText27()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IowedCurrentIState.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField1);
  }

  private void UseFnCabCurrencyToText28()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IowedCurrentIFamily.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField1);
  }

  private void UseFnCabCurrencyToText29()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IowedArrearsState.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField1);
  }

  private void UseFnCabCurrencyToText30()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IowedArrearsFamily.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField1);
  }

  private void UseFnCabCurrencyToText31()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IowedArrearsIState.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField1);
  }

  private void UseFnCabCurrencyToText32()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IowedArrearsIFamily.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField1);
  }

  private void UseFnCabCurrencyToText33()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IowedCurrTotal.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField1);
  }

  private void UseFnCabCurrencyToText34()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IowedArrearsTotal.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField1);
  }

  private void UseFnCabCurrencyToText35()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IowedTotal.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField1);
  }

  private void UseFnCabCurrencyToText36()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IcollCurrTotal.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField1);
  }

  private void UseFnCabCurrencyToText37()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IcollArrearsTotal.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField1);
  }

  private void UseFnCabCurrencyToText38()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IcollectedTotal.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField1);
  }

  private void UseFnCabCurrencyToText39()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IcollCurrentState.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField2);
  }

  private void UseFnCabCurrencyToText40()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IcollCurrentFamily.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField2);
  }

  private void UseFnCabCurrencyToText41()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IcollCurrentIState.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField2);
  }

  private void UseFnCabCurrencyToText42()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IcollCurrentIFamily.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField2);
  }

  private void UseFnCabCurrencyToText43()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IcollArrearsState.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField2);
  }

  private void UseFnCabCurrencyToText44()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IcollArrearsFamily.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField2);
  }

  private void UseFnCabCurrencyToText45()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IcollArrearsIState.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField2);
  }

  private void UseFnCabCurrencyToText46()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IcollArrearsIFamily.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField2);
  }

  private void UseFnCabCurrencyToText47()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IowedCurrentState.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField2);
  }

  private void UseFnCabCurrencyToText48()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IowedCurrentFamily.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField2);
  }

  private void UseFnCabCurrencyToText49()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IowedCurrentIState.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField2);
  }

  private void UseFnCabCurrencyToText50()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IowedCurrentIFamily.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField2);
  }

  private void UseFnCabCurrencyToText51()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IowedArrearsState.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField2);
  }

  private void UseFnCabCurrencyToText52()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IowedArrearsFamily.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField2);
  }

  private void UseFnCabCurrencyToText53()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IowedArrearsIState.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField2);
  }

  private void UseFnCabCurrencyToText54()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IowedArrearsIFamily.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField2);
  }

  private void UseFnCabCurrencyToText55()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IowedCurrTotal.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField2);
  }

  private void UseFnCabCurrencyToText56()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IowedArrearsTotal.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField2);
  }

  private void UseFnCabCurrencyToText57()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IowedTotal.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField2);
  }

  private void UseFnCabCurrencyToText58()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IcollCurrTotal.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField2);
  }

  private void UseFnCabCurrencyToText59()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IcollArrearsTotal.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField2);
  }

  private void UseFnCabCurrencyToText60()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IcollectedTotal.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField2);
  }

  private void UseFnCabCurrencyToText61()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IcollCurrentState.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField3);
  }

  private void UseFnCabCurrencyToText62()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IcollCurrentFamily.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField3);
  }

  private void UseFnCabCurrencyToText63()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IcollCurrentIState.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField3);
  }

  private void UseFnCabCurrencyToText64()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IcollCurrentIFamily.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField3);
  }

  private void UseFnCabCurrencyToText65()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IcollArrearsState.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField3);
  }

  private void UseFnCabCurrencyToText66()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IcollArrearsFamily.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField3);
  }

  private void UseFnCabCurrencyToText67()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IcollArrearsIState.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField3);
  }

  private void UseFnCabCurrencyToText68()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IcollArrearsIFamily.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField3);
  }

  private void UseFnCabCurrencyToText69()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IowedCurrentState.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField3);
  }

  private void UseFnCabCurrencyToText70()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IowedCurrentFamily.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField3);
  }

  private void UseFnCabCurrencyToText71()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IowedCurrentIState.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField3);
  }

  private void UseFnCabCurrencyToText72()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IowedCurrentIFamily.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField3);
  }

  private void UseFnCabCurrencyToText73()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IowedArrearsState.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField3);
  }

  private void UseFnCabCurrencyToText74()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IowedArrearsFamily.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField3);
  }

  private void UseFnCabCurrencyToText75()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IowedArrearsIState.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField3);
  }

  private void UseFnCabCurrencyToText76()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IowedArrearsIFamily.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField3);
  }

  private void UseFnCabCurrencyToText77()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IowedCurrTotal.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField3);
  }

  private void UseFnCabCurrencyToText78()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IowedArrearsTotal.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField3);
  }

  private void UseFnCabCurrencyToText79()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IowedTotal.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField3);
  }

  private void UseFnCabCurrencyToText80()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IcollCurrTotal.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField3);
  }

  private void UseFnCabCurrencyToText81()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IcollArrearsTotal.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField3);
  }

  private void UseFnCabCurrencyToText82()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IcollectedTotal.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField3);
  }

  private void UseFnCabCurrencyToText83()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IcollCurrentState.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField4);
  }

  private void UseFnCabCurrencyToText84()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IcollCurrentFamily.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField4);
  }

  private void UseFnCabCurrencyToText85()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IcollCurrentIState.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField4);
  }

  private void UseFnCabCurrencyToText86()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IcollCurrentIFamily.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField4);
  }

  private void UseFnCabCurrencyToText87()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IcollArrearsState.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField4);
  }

  private void UseFnCabCurrencyToText88()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IcollArrearsFamily.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField4);
  }

  private void UseFnCabCurrencyToText89()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IcollArrearsIState.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField4);
  }

  private void UseFnCabCurrencyToText90()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IcollArrearsIFamily.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField4);
  }

  private void UseFnCabCurrencyToText91()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IowedCurrentState.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField4);
  }

  private void UseFnCabCurrencyToText92()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IowedCurrentFamily.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField4);
  }

  private void UseFnCabCurrencyToText93()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IowedCurrentIState.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField4);
  }

  private void UseFnCabCurrencyToText94()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IowedCurrentIFamily.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField4);
  }

  private void UseFnCabCurrencyToText95()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IowedArrearsState.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField4);
  }

  private void UseFnCabCurrencyToText96()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IowedArrearsFamily.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField4);
  }

  private void UseFnCabCurrencyToText97()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IowedArrearsIState.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField4);
  }

  private void UseFnCabCurrencyToText98()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IowedArrearsIFamily.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField4);
  }

  private void UseFnCabCurrencyToText99()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IowedCurrTotal.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField4);
  }

  private void UseFnCabCurrencyToText100()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IowedArrearsTotal.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField4);
  }

  private void UseFnCabCurrencyToText101()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IowedTotal.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField4);
  }

  private void UseFnCabCurrencyToText102()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IcollCurrTotal.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField4);
  }

  private void UseFnCabCurrencyToText103()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IcollArrearsTotal.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField4);
  }

  private void UseFnCabCurrencyToText104()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommasRequired.Flag;
    useImport.Common.TotalCurrency =
      import.ReferralType.Item.IcollectedTotal.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField4);
  }

  private void UseFnCabCurrencyToText105()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.Common.TotalCurrency = local.CalcField1.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField1);
  }

  private void UseFnCabCurrencyToText106()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.Common.TotalCurrency = local.CalcField2.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField2);
  }

  private void UseFnCabCurrencyToText107()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.Common.TotalCurrency = local.CalcField3.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField3);
  }

  private void UseFnCabCurrencyToText108()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.Common.TotalCurrency = local.CalcField4.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    MoveWorkArea(useExport.WorkArea, local.ReportField4);
  }

  private bool ReadFips()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.
          SetString(command, "stateAbbreviation", import.Fips.StateAbbreviation);
          
        db.SetNullableString(
          command, "countyAbbr", import.Fips.CountyAbbreviation ?? "");
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.StateDescription = db.GetNullableString(reader, 3);
        entities.Fips.CountyDescription = db.GetNullableString(reader, 4);
        entities.Fips.StateAbbreviation = db.GetString(reader, 5);
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 6);
        entities.Fips.Populated = true;
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
    /// <summary>A ReferralTypeGroup group.</summary>
    [Serializable]
    public class ReferralTypeGroup
    {
      /// <summary>
      /// A value of ItotalOrdersReferred.
      /// </summary>
      [JsonPropertyName("itotalOrdersReferred")]
      public Common ItotalOrdersReferred
      {
        get => itotalOrdersReferred ??= new();
        set => itotalOrdersReferred = value;
      }

      /// <summary>
      /// A value of InumberOfPayingOrders.
      /// </summary>
      [JsonPropertyName("inumberOfPayingOrders")]
      public Common InumberOfPayingOrders
      {
        get => inumberOfPayingOrders ??= new();
        set => inumberOfPayingOrders = value;
      }

      /// <summary>
      /// A value of InumberOfOrdersInLocate.
      /// </summary>
      [JsonPropertyName("inumberOfOrdersInLocate")]
      public Common InumberOfOrdersInLocate
      {
        get => inumberOfOrdersInLocate ??= new();
        set => inumberOfOrdersInLocate = value;
      }

      /// <summary>
      /// A value of Ifips.
      /// </summary>
      [JsonPropertyName("ifips")]
      public Fips Ifips
      {
        get => ifips ??= new();
        set => ifips = value;
      }

      /// <summary>
      /// A value of Itribunal.
      /// </summary>
      [JsonPropertyName("itribunal")]
      public Tribunal Itribunal
      {
        get => itribunal ??= new();
        set => itribunal = value;
      }

      /// <summary>
      /// A value of IlegalAction.
      /// </summary>
      [JsonPropertyName("ilegalAction")]
      public LegalAction IlegalAction
      {
        get => ilegalAction ??= new();
        set => ilegalAction = value;
      }

      /// <summary>
      /// A value of IreferralType.
      /// </summary>
      [JsonPropertyName("ireferralType")]
      public TextWorkArea IreferralType
      {
        get => ireferralType ??= new();
        set => ireferralType = value;
      }

      /// <summary>
      /// A value of IcollCurrentState.
      /// </summary>
      [JsonPropertyName("icollCurrentState")]
      public Common IcollCurrentState
      {
        get => icollCurrentState ??= new();
        set => icollCurrentState = value;
      }

      /// <summary>
      /// A value of IcollCurrentFamily.
      /// </summary>
      [JsonPropertyName("icollCurrentFamily")]
      public Common IcollCurrentFamily
      {
        get => icollCurrentFamily ??= new();
        set => icollCurrentFamily = value;
      }

      /// <summary>
      /// A value of IcollCurrentIState.
      /// </summary>
      [JsonPropertyName("icollCurrentIState")]
      public Common IcollCurrentIState
      {
        get => icollCurrentIState ??= new();
        set => icollCurrentIState = value;
      }

      /// <summary>
      /// A value of IcollCurrentIFamily.
      /// </summary>
      [JsonPropertyName("icollCurrentIFamily")]
      public Common IcollCurrentIFamily
      {
        get => icollCurrentIFamily ??= new();
        set => icollCurrentIFamily = value;
      }

      /// <summary>
      /// A value of IcollArrearsState.
      /// </summary>
      [JsonPropertyName("icollArrearsState")]
      public Common IcollArrearsState
      {
        get => icollArrearsState ??= new();
        set => icollArrearsState = value;
      }

      /// <summary>
      /// A value of IcollArrearsFamily.
      /// </summary>
      [JsonPropertyName("icollArrearsFamily")]
      public Common IcollArrearsFamily
      {
        get => icollArrearsFamily ??= new();
        set => icollArrearsFamily = value;
      }

      /// <summary>
      /// A value of IcollArrearsIState.
      /// </summary>
      [JsonPropertyName("icollArrearsIState")]
      public Common IcollArrearsIState
      {
        get => icollArrearsIState ??= new();
        set => icollArrearsIState = value;
      }

      /// <summary>
      /// A value of IcollArrearsIFamily.
      /// </summary>
      [JsonPropertyName("icollArrearsIFamily")]
      public Common IcollArrearsIFamily
      {
        get => icollArrearsIFamily ??= new();
        set => icollArrearsIFamily = value;
      }

      /// <summary>
      /// A value of IowedCurrentState.
      /// </summary>
      [JsonPropertyName("iowedCurrentState")]
      public Common IowedCurrentState
      {
        get => iowedCurrentState ??= new();
        set => iowedCurrentState = value;
      }

      /// <summary>
      /// A value of IowedCurrentFamily.
      /// </summary>
      [JsonPropertyName("iowedCurrentFamily")]
      public Common IowedCurrentFamily
      {
        get => iowedCurrentFamily ??= new();
        set => iowedCurrentFamily = value;
      }

      /// <summary>
      /// A value of IowedCurrentIState.
      /// </summary>
      [JsonPropertyName("iowedCurrentIState")]
      public Common IowedCurrentIState
      {
        get => iowedCurrentIState ??= new();
        set => iowedCurrentIState = value;
      }

      /// <summary>
      /// A value of IowedCurrentIFamily.
      /// </summary>
      [JsonPropertyName("iowedCurrentIFamily")]
      public Common IowedCurrentIFamily
      {
        get => iowedCurrentIFamily ??= new();
        set => iowedCurrentIFamily = value;
      }

      /// <summary>
      /// A value of IowedArrearsState.
      /// </summary>
      [JsonPropertyName("iowedArrearsState")]
      public Common IowedArrearsState
      {
        get => iowedArrearsState ??= new();
        set => iowedArrearsState = value;
      }

      /// <summary>
      /// A value of IowedArrearsFamily.
      /// </summary>
      [JsonPropertyName("iowedArrearsFamily")]
      public Common IowedArrearsFamily
      {
        get => iowedArrearsFamily ??= new();
        set => iowedArrearsFamily = value;
      }

      /// <summary>
      /// A value of IowedArrearsIState.
      /// </summary>
      [JsonPropertyName("iowedArrearsIState")]
      public Common IowedArrearsIState
      {
        get => iowedArrearsIState ??= new();
        set => iowedArrearsIState = value;
      }

      /// <summary>
      /// A value of IowedArrearsIFamily.
      /// </summary>
      [JsonPropertyName("iowedArrearsIFamily")]
      public Common IowedArrearsIFamily
      {
        get => iowedArrearsIFamily ??= new();
        set => iowedArrearsIFamily = value;
      }

      /// <summary>
      /// A value of IowedCurrTotal.
      /// </summary>
      [JsonPropertyName("iowedCurrTotal")]
      public Common IowedCurrTotal
      {
        get => iowedCurrTotal ??= new();
        set => iowedCurrTotal = value;
      }

      /// <summary>
      /// A value of IowedArrearsTotal.
      /// </summary>
      [JsonPropertyName("iowedArrearsTotal")]
      public Common IowedArrearsTotal
      {
        get => iowedArrearsTotal ??= new();
        set => iowedArrearsTotal = value;
      }

      /// <summary>
      /// A value of IowedTotal.
      /// </summary>
      [JsonPropertyName("iowedTotal")]
      public Common IowedTotal
      {
        get => iowedTotal ??= new();
        set => iowedTotal = value;
      }

      /// <summary>
      /// A value of IcollCurrTotal.
      /// </summary>
      [JsonPropertyName("icollCurrTotal")]
      public Common IcollCurrTotal
      {
        get => icollCurrTotal ??= new();
        set => icollCurrTotal = value;
      }

      /// <summary>
      /// A value of IcollArrearsTotal.
      /// </summary>
      [JsonPropertyName("icollArrearsTotal")]
      public Common IcollArrearsTotal
      {
        get => icollArrearsTotal ??= new();
        set => icollArrearsTotal = value;
      }

      /// <summary>
      /// A value of IcollectedTotal.
      /// </summary>
      [JsonPropertyName("icollectedTotal")]
      public Common IcollectedTotal
      {
        get => icollectedTotal ??= new();
        set => icollectedTotal = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private Common itotalOrdersReferred;
      private Common inumberOfPayingOrders;
      private Common inumberOfOrdersInLocate;
      private Fips ifips;
      private Tribunal itribunal;
      private LegalAction ilegalAction;
      private TextWorkArea ireferralType;
      private Common icollCurrentState;
      private Common icollCurrentFamily;
      private Common icollCurrentIState;
      private Common icollCurrentIFamily;
      private Common icollArrearsState;
      private Common icollArrearsFamily;
      private Common icollArrearsIState;
      private Common icollArrearsIFamily;
      private Common iowedCurrentState;
      private Common iowedCurrentFamily;
      private Common iowedCurrentIState;
      private Common iowedCurrentIFamily;
      private Common iowedArrearsState;
      private Common iowedArrearsFamily;
      private Common iowedArrearsIState;
      private Common iowedArrearsIFamily;
      private Common iowedCurrTotal;
      private Common iowedArrearsTotal;
      private Common iowedTotal;
      private Common icollCurrTotal;
      private Common icollArrearsTotal;
      private Common icollectedTotal;
    }

    /// <summary>
    /// Gets a value of ReferralType.
    /// </summary>
    [JsonIgnore]
    public Array<ReferralTypeGroup> ReferralType => referralType ??= new(
      ReferralTypeGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ReferralType for json serialization.
    /// </summary>
    [JsonPropertyName("referralType")]
    [Computed]
    public IList<ReferralTypeGroup> ReferralType_Json
    {
      get => referralType;
      set => ReferralType.Assign(value);
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
    /// A value of PageNumber.
    /// </summary>
    [JsonPropertyName("pageNumber")]
    public Common PageNumber
    {
      get => pageNumber ??= new();
      set => pageNumber = value;
    }

    /// <summary>
    /// A value of ReportLevel1.
    /// </summary>
    [JsonPropertyName("reportLevel1")]
    public WorkArea ReportLevel1
    {
      get => reportLevel1 ??= new();
      set => reportLevel1 = value;
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

    private Array<ReferralTypeGroup> referralType;
    private Fips fips;
    private Tribunal tribunal;
    private Common pageNumber;
    private WorkArea reportLevel1;
    private DateWorkArea eom;
    private WorkArea monthName;
    private WorkArea textDate;
    private TextWorkArea textDd;
    private TextWorkArea textYyyy;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ReportLevelGroup group.</summary>
    [Serializable]
    public class ReportLevelGroup
    {
      /// <summary>
      /// A value of EtotalOrdersReferred.
      /// </summary>
      [JsonPropertyName("etotalOrdersReferred")]
      public Common EtotalOrdersReferred
      {
        get => etotalOrdersReferred ??= new();
        set => etotalOrdersReferred = value;
      }

      /// <summary>
      /// A value of EnumberOfPayingOrders.
      /// </summary>
      [JsonPropertyName("enumberOfPayingOrders")]
      public Common EnumberOfPayingOrders
      {
        get => enumberOfPayingOrders ??= new();
        set => enumberOfPayingOrders = value;
      }

      /// <summary>
      /// A value of EnumberOfOrdersInLocate.
      /// </summary>
      [JsonPropertyName("enumberOfOrdersInLocate")]
      public Common EnumberOfOrdersInLocate
      {
        get => enumberOfOrdersInLocate ??= new();
        set => enumberOfOrdersInLocate = value;
      }

      /// <summary>
      /// A value of Efips.
      /// </summary>
      [JsonPropertyName("efips")]
      public Fips Efips
      {
        get => efips ??= new();
        set => efips = value;
      }

      /// <summary>
      /// A value of Etribunal.
      /// </summary>
      [JsonPropertyName("etribunal")]
      public Tribunal Etribunal
      {
        get => etribunal ??= new();
        set => etribunal = value;
      }

      /// <summary>
      /// A value of ElegalAction.
      /// </summary>
      [JsonPropertyName("elegalAction")]
      public LegalAction ElegalAction
      {
        get => elegalAction ??= new();
        set => elegalAction = value;
      }

      /// <summary>
      /// A value of EreferralType.
      /// </summary>
      [JsonPropertyName("ereferralType")]
      public TextWorkArea EreferralType
      {
        get => ereferralType ??= new();
        set => ereferralType = value;
      }

      /// <summary>
      /// A value of EcollCurrentState.
      /// </summary>
      [JsonPropertyName("ecollCurrentState")]
      public Common EcollCurrentState
      {
        get => ecollCurrentState ??= new();
        set => ecollCurrentState = value;
      }

      /// <summary>
      /// A value of EcollCurrentFamily.
      /// </summary>
      [JsonPropertyName("ecollCurrentFamily")]
      public Common EcollCurrentFamily
      {
        get => ecollCurrentFamily ??= new();
        set => ecollCurrentFamily = value;
      }

      /// <summary>
      /// A value of EcollCurrentIState.
      /// </summary>
      [JsonPropertyName("ecollCurrentIState")]
      public Common EcollCurrentIState
      {
        get => ecollCurrentIState ??= new();
        set => ecollCurrentIState = value;
      }

      /// <summary>
      /// A value of EcollCurrentIFamily.
      /// </summary>
      [JsonPropertyName("ecollCurrentIFamily")]
      public Common EcollCurrentIFamily
      {
        get => ecollCurrentIFamily ??= new();
        set => ecollCurrentIFamily = value;
      }

      /// <summary>
      /// A value of EcollArrearsState.
      /// </summary>
      [JsonPropertyName("ecollArrearsState")]
      public Common EcollArrearsState
      {
        get => ecollArrearsState ??= new();
        set => ecollArrearsState = value;
      }

      /// <summary>
      /// A value of EcollArrearsFamily.
      /// </summary>
      [JsonPropertyName("ecollArrearsFamily")]
      public Common EcollArrearsFamily
      {
        get => ecollArrearsFamily ??= new();
        set => ecollArrearsFamily = value;
      }

      /// <summary>
      /// A value of EcollArrearsIState.
      /// </summary>
      [JsonPropertyName("ecollArrearsIState")]
      public Common EcollArrearsIState
      {
        get => ecollArrearsIState ??= new();
        set => ecollArrearsIState = value;
      }

      /// <summary>
      /// A value of EcollArrearsIFamily.
      /// </summary>
      [JsonPropertyName("ecollArrearsIFamily")]
      public Common EcollArrearsIFamily
      {
        get => ecollArrearsIFamily ??= new();
        set => ecollArrearsIFamily = value;
      }

      /// <summary>
      /// A value of EowedCurrentState.
      /// </summary>
      [JsonPropertyName("eowedCurrentState")]
      public Common EowedCurrentState
      {
        get => eowedCurrentState ??= new();
        set => eowedCurrentState = value;
      }

      /// <summary>
      /// A value of EowedCurrentFamily.
      /// </summary>
      [JsonPropertyName("eowedCurrentFamily")]
      public Common EowedCurrentFamily
      {
        get => eowedCurrentFamily ??= new();
        set => eowedCurrentFamily = value;
      }

      /// <summary>
      /// A value of EowedCurrentIState.
      /// </summary>
      [JsonPropertyName("eowedCurrentIState")]
      public Common EowedCurrentIState
      {
        get => eowedCurrentIState ??= new();
        set => eowedCurrentIState = value;
      }

      /// <summary>
      /// A value of EowedCurrentIFamily.
      /// </summary>
      [JsonPropertyName("eowedCurrentIFamily")]
      public Common EowedCurrentIFamily
      {
        get => eowedCurrentIFamily ??= new();
        set => eowedCurrentIFamily = value;
      }

      /// <summary>
      /// A value of EowedArrearsState.
      /// </summary>
      [JsonPropertyName("eowedArrearsState")]
      public Common EowedArrearsState
      {
        get => eowedArrearsState ??= new();
        set => eowedArrearsState = value;
      }

      /// <summary>
      /// A value of EowedArrearsFamily.
      /// </summary>
      [JsonPropertyName("eowedArrearsFamily")]
      public Common EowedArrearsFamily
      {
        get => eowedArrearsFamily ??= new();
        set => eowedArrearsFamily = value;
      }

      /// <summary>
      /// A value of EowedArrearsIState.
      /// </summary>
      [JsonPropertyName("eowedArrearsIState")]
      public Common EowedArrearsIState
      {
        get => eowedArrearsIState ??= new();
        set => eowedArrearsIState = value;
      }

      /// <summary>
      /// A value of EowedArrearsIFamily.
      /// </summary>
      [JsonPropertyName("eowedArrearsIFamily")]
      public Common EowedArrearsIFamily
      {
        get => eowedArrearsIFamily ??= new();
        set => eowedArrearsIFamily = value;
      }

      /// <summary>
      /// A value of EowedCurrTotal.
      /// </summary>
      [JsonPropertyName("eowedCurrTotal")]
      public Common EowedCurrTotal
      {
        get => eowedCurrTotal ??= new();
        set => eowedCurrTotal = value;
      }

      /// <summary>
      /// A value of EowedArrearsTotal.
      /// </summary>
      [JsonPropertyName("eowedArrearsTotal")]
      public Common EowedArrearsTotal
      {
        get => eowedArrearsTotal ??= new();
        set => eowedArrearsTotal = value;
      }

      /// <summary>
      /// A value of EowedTotal.
      /// </summary>
      [JsonPropertyName("eowedTotal")]
      public Common EowedTotal
      {
        get => eowedTotal ??= new();
        set => eowedTotal = value;
      }

      /// <summary>
      /// A value of EcollCurrTotal.
      /// </summary>
      [JsonPropertyName("ecollCurrTotal")]
      public Common EcollCurrTotal
      {
        get => ecollCurrTotal ??= new();
        set => ecollCurrTotal = value;
      }

      /// <summary>
      /// A value of EcollArrearsTotal.
      /// </summary>
      [JsonPropertyName("ecollArrearsTotal")]
      public Common EcollArrearsTotal
      {
        get => ecollArrearsTotal ??= new();
        set => ecollArrearsTotal = value;
      }

      /// <summary>
      /// A value of EcollectedTotal.
      /// </summary>
      [JsonPropertyName("ecollectedTotal")]
      public Common EcollectedTotal
      {
        get => ecollectedTotal ??= new();
        set => ecollectedTotal = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private Common etotalOrdersReferred;
      private Common enumberOfPayingOrders;
      private Common enumberOfOrdersInLocate;
      private Fips efips;
      private Tribunal etribunal;
      private LegalAction elegalAction;
      private TextWorkArea ereferralType;
      private Common ecollCurrentState;
      private Common ecollCurrentFamily;
      private Common ecollCurrentIState;
      private Common ecollCurrentIFamily;
      private Common ecollArrearsState;
      private Common ecollArrearsFamily;
      private Common ecollArrearsIState;
      private Common ecollArrearsIFamily;
      private Common eowedCurrentState;
      private Common eowedCurrentFamily;
      private Common eowedCurrentIState;
      private Common eowedCurrentIFamily;
      private Common eowedArrearsState;
      private Common eowedArrearsFamily;
      private Common eowedArrearsIState;
      private Common eowedArrearsIFamily;
      private Common eowedCurrTotal;
      private Common eowedArrearsTotal;
      private Common eowedTotal;
      private Common ecollCurrTotal;
      private Common ecollArrearsTotal;
      private Common ecollectedTotal;
    }

    /// <summary>
    /// Gets a value of ReportLevel.
    /// </summary>
    [JsonIgnore]
    public Array<ReportLevelGroup> ReportLevel => reportLevel ??= new(
      ReportLevelGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ReportLevel for json serialization.
    /// </summary>
    [JsonPropertyName("reportLevel")]
    [Computed]
    public IList<ReportLevelGroup> ReportLevel_Json
    {
      get => reportLevel;
      set => ReportLevel.Assign(value);
    }

    private Array<ReportLevelGroup> reportLevel;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A ReportGroup group.</summary>
    [Serializable]
    public class ReportGroup
    {
      /// <summary>
      /// A value of EabReportSend.
      /// </summary>
      [JsonPropertyName("eabReportSend")]
      public EabReportSend EabReportSend
      {
        get => eabReportSend ??= new();
        set => eabReportSend = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private EabReportSend eabReportSend;
    }

    /// <summary>A CurrtextFieldsGroup group.</summary>
    [Serializable]
    public class CurrtextFieldsGroup
    {
      /// <summary>
      /// A value of Textcurr.
      /// </summary>
      [JsonPropertyName("textcurr")]
      public WorkArea Textcurr
      {
        get => textcurr ??= new();
        set => textcurr = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private WorkArea textcurr;
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
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of LineOfDashes.
    /// </summary>
    [JsonPropertyName("lineOfDashes")]
    public EabReportSend LineOfDashes
    {
      get => lineOfDashes ??= new();
      set => lineOfDashes = value;
    }

    /// <summary>
    /// A value of NoCents.
    /// </summary>
    [JsonPropertyName("noCents")]
    public Common NoCents
    {
      get => noCents ??= new();
      set => noCents = value;
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
    /// A value of CommasRequired.
    /// </summary>
    [JsonPropertyName("commasRequired")]
    public Common CommasRequired
    {
      get => commasRequired ??= new();
      set => commasRequired = value;
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
    /// Gets a value of Report.
    /// </summary>
    [JsonIgnore]
    public Array<ReportGroup> Report => report ??= new(ReportGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Report for json serialization.
    /// </summary>
    [JsonPropertyName("report")]
    [Computed]
    public IList<ReportGroup> Report_Json
    {
      get => report;
      set => Report.Assign(value);
    }

    /// <summary>
    /// Gets a value of CurrtextFields.
    /// </summary>
    [JsonIgnore]
    public Array<CurrtextFieldsGroup> CurrtextFields => currtextFields ??= new(
      CurrtextFieldsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of CurrtextFields for json serialization.
    /// </summary>
    [JsonPropertyName("currtextFields")]
    [Computed]
    public IList<CurrtextFieldsGroup> CurrtextFields_Json
    {
      get => currtextFields;
      set => CurrtextFields.Assign(value);
    }

    /// <summary>
    /// A value of ReportField1.
    /// </summary>
    [JsonPropertyName("reportField1")]
    public WorkArea ReportField1
    {
      get => reportField1 ??= new();
      set => reportField1 = value;
    }

    /// <summary>
    /// A value of ReportField2.
    /// </summary>
    [JsonPropertyName("reportField2")]
    public WorkArea ReportField2
    {
      get => reportField2 ??= new();
      set => reportField2 = value;
    }

    /// <summary>
    /// A value of ReportField3.
    /// </summary>
    [JsonPropertyName("reportField3")]
    public WorkArea ReportField3
    {
      get => reportField3 ??= new();
      set => reportField3 = value;
    }

    /// <summary>
    /// A value of ReportField4.
    /// </summary>
    [JsonPropertyName("reportField4")]
    public WorkArea ReportField4
    {
      get => reportField4 ??= new();
      set => reportField4 = value;
    }

    /// <summary>
    /// A value of CalcField1.
    /// </summary>
    [JsonPropertyName("calcField1")]
    public Common CalcField1
    {
      get => calcField1 ??= new();
      set => calcField1 = value;
    }

    /// <summary>
    /// A value of CalcField2.
    /// </summary>
    [JsonPropertyName("calcField2")]
    public Common CalcField2
    {
      get => calcField2 ??= new();
      set => calcField2 = value;
    }

    /// <summary>
    /// A value of CalcField3.
    /// </summary>
    [JsonPropertyName("calcField3")]
    public Common CalcField3
    {
      get => calcField3 ??= new();
      set => calcField3 = value;
    }

    /// <summary>
    /// A value of CalcField4.
    /// </summary>
    [JsonPropertyName("calcField4")]
    public Common CalcField4
    {
      get => calcField4 ??= new();
      set => calcField4 = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public ReportData Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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

    private ProgramProcessingInfo programProcessingInfo;
    private Fips fips;
    private EabReportSend lineOfDashes;
    private Common noCents;
    private WorkArea monthName;
    private EabFileHandling eabFileHandling;
    private Common commasRequired;
    private EabReportSend eabReportSend;
    private Array<ReportGroup> report;
    private Array<CurrtextFieldsGroup> currtextFields;
    private WorkArea reportField1;
    private WorkArea reportField2;
    private WorkArea reportField3;
    private WorkArea reportField4;
    private Common calcField1;
    private Common calcField2;
    private Common calcField3;
    private Common calcField4;
    private ReportData null1;
    private EabReportSend neededToOpen;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    private Fips fips;
  }
#endregion
}
