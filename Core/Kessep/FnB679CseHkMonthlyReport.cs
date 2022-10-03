// Program: FN_B679_CSE_HK_MONTHLY_REPORT, ID: 371260118, model: 746.
// Short name: SWEF679B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B679_CSE_HK_MONTHLY_REPORT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB679CseHkMonthlyReport: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B679_CSE_HK_MONTHLY_REPORT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB679CseHkMonthlyReport(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB679CseHkMonthlyReport.
  /// </summary>
  public FnB679CseHkMonthlyReport(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------------------------------------------------------------
    // Date      Developer	Request #	Description
    // --------  ----------	----------	
    // ---------------------------------------------------------------------------------
    // 09/09/05  GVandy	WR00256682	Initial Development.
    // -------------------------------------------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------------------------------------------
    // --  This program produces a monthly report of CSE activity for clients 
    // impacted by Hurricane Katrina.
    // -------------------------------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.Write.Action = "WRITE";
    local.Close.Action = "CLOSE";
    local.Open.Action = "OPEN";

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Read the PPI Record.
    // -------------------------------------------------------------------------------------------------------------------------
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Open the Error Report.
    // -------------------------------------------------------------------------------------------------------------------------
    local.EabReportSend.ProgramName = global.UserId;
    local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    UseCabErrorReport3();

    if (!Equal(local.Open.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_OPENING_ERROR_RPT";

      return;
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      // -- This could have resulted from not finding the PPI record.
      // -- Extract the exit state message and write to the error report.
      UseEabExtractExitStateMessage();
      local.EabReportSend.RptDetail = "Initialization Error..." + local
        .ExitStateWorkArea.Message;
      UseCabErrorReport1();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Open the Control Report.
    // -------------------------------------------------------------------------------------------------------------------------
    UseCabControlReport3();

    if (!Equal(local.Open.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_OPENING_CONTROL_RPT";

      return;
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Open the Hurricane Katrina Report.
    // -------------------------------------------------------------------------------------------------------------------------
    UseCabBusinessReport3();

    if (!Equal(local.Open.Status, "OK"))
    {
      ExitState = "FILE_OPEN_ERROR";

      return;
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // -- Set start and end timestamps....
    // -------------------------------------------------------------------------------------------------------------------------
    local.Start.Timestamp =
      Add(local.NullDateWorkArea.Timestamp,
      Year(local.ProgramProcessingInfo.ProcessDate),
      Month(local.ProgramProcessingInfo.ProcessDate), 1);
    local.End.Timestamp = AddMicroseconds(local.Start.Timestamp, -1);
    local.Start.Timestamp = AddMonths(local.Start.Timestamp, -1);
    local.Start.Date = Date(local.Start.Timestamp);
    local.End.Date = Date(local.End.Timestamp);

    switch(Month(local.Start.Date))
    {
      case 1:
        local.ReportMonth.RptDetail =
          "                   Reporting Period:  January " + NumberToString
          (Year(local.Start.Date), 12, 4);

        break;
      case 2:
        local.ReportMonth.RptDetail =
          "                   Reporting Period:  February " + NumberToString
          (Year(local.Start.Date), 12, 4);

        break;
      case 3:
        local.ReportMonth.RptDetail =
          "                   Reporting Period:  March " + NumberToString
          (Year(local.Start.Date), 12, 4);

        break;
      case 4:
        local.ReportMonth.RptDetail =
          "                   Reporting Period:  April " + NumberToString
          (Year(local.Start.Date), 12, 4);

        break;
      case 5:
        local.ReportMonth.RptDetail =
          "                   Reporting Period:  May " + NumberToString
          (Year(local.Start.Date), 12, 4);

        break;
      case 6:
        local.ReportMonth.RptDetail =
          "                   Reporting Period:  June " + NumberToString
          (Year(local.Start.Date), 12, 4);

        break;
      case 7:
        local.ReportMonth.RptDetail =
          "                   Reporting Period:  July " + NumberToString
          (Year(local.Start.Date), 12, 4);

        break;
      case 8:
        local.ReportMonth.RptDetail =
          "                   Reporting Period:  August " + NumberToString
          (Year(local.Start.Date), 12, 4);

        break;
      case 9:
        local.ReportMonth.RptDetail =
          "                   Reporting Period:  September " + NumberToString
          (Year(local.Start.Date), 12, 4);

        break;
      case 10:
        local.ReportMonth.RptDetail =
          "                   Reporting Period:  October " + NumberToString
          (Year(local.Start.Date), 12, 4);

        break;
      case 11:
        local.ReportMonth.RptDetail =
          "                   Reporting Period:  November " + NumberToString
          (Year(local.Start.Date), 12, 4);

        break;
      case 12:
        local.ReportMonth.RptDetail =
          "                   Reporting Period:  December " + NumberToString
          (Year(local.Start.Date), 12, 4);

        break;
      default:
        break;
    }

    // -- Populate the report group with all county codes and descriptions.  The
    // first entry will be for county code "  " which is used when no county
    // code was on the ARs address.
    local.Report.Index = 0;
    local.Report.CheckSize();

    local.Report.Update.GlocalReportCsePersonAddress.County = "";
    local.Report.Update.GlocalReportCodeValue.Description = "<UKNOWN>";

    foreach(var item in ReadCodeValue())
    {
      ++local.Report.Index;
      local.Report.CheckSize();

      local.Report.Update.GlocalReportCodeValue.Description =
        entities.CodeValue.Description;
      local.Report.Update.GlocalReportCsePersonAddress.County =
        entities.CodeValue.Cdvalue;
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Perform calculations for the report.
    // -------------------------------------------------------------------------------------------------------------------------
    for(local.Counter.Count = 1; local.Counter.Count <= 5; ++
      local.Counter.Count)
    {
      switch(local.Counter.Count)
      {
        case 1:
          local.EabReportSend.RptDetail = "";

          break;
        case 2:
          local.EabReportSend.RptDetail = local.ReportMonth.RptDetail;

          break;
        case 3:
          local.EabReportSend.RptDetail = "";

          break;
        case 4:
          local.EabReportSend.RptDetail = "Start Child Counts - Time: " + NumberToString
            (TimeToInt(Time(Now())), 15);

          break;
        case 5:
          local.EabReportSend.RptDetail = "";

          break;
        default:
          break;
      }

      UseCabControlReport1();

      if (!Equal(local.Write.Status, "OK"))
      {
        // -- Write to the error report.
        local.Write.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "(04) Error Writing Control Report...  Returned Status = " + local
          .Write.Status;
        UseCabErrorReport1();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // -- Determine number of children impacted by Hurricane Katrina.  Grouped 
    // by county of the AR address.
    // -------------------------------------------------------------------------------------------------------------------------
    foreach(var item in ReadCsePerson2())
    {
      local.CountiesReported.RptDetail = "";

      foreach(var item1 in ReadCaseCaseRole2())
      {
        local.ArAddress.Count = 0;

        if (ReadApplicantRecipient())
        {
          if (ReadCsePersonAddress2())
          {
            ++local.ArAddress.Count;

            if (Find(String(
              local.CountiesReported.RptDetail,
              EabReportSend.RptDetail_MaxLength),
              entities.ArCsePersonAddress.County + "/") != 0)
            {
              local.AlreadyReported.Flag = "*";
            }
            else
            {
              local.AlreadyReported.Flag = "";
            }

            local.EabReportSend.RptDetail = "Child " + entities
              .CsePerson.Number + "  Case " + entities.Case1.Number + "  County " +
              entities.ArCsePersonAddress.County + local.AlreadyReported.Flag;
            UseCabControlReport1();

            if (!Equal(local.Write.Status, "OK"))
            {
              // -- Write to the error report.
              local.Write.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "(04) Error Writing Control Report...  Returned Status = " + local
                .Write.Status;
              UseCabErrorReport1();

              // -- Set Abort exit state and escape...
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            if (Find(String(
              local.CountiesReported.RptDetail,
              EabReportSend.RptDetail_MaxLength),
              entities.ArCsePersonAddress.County + "/") != 0)
            {
              // --  We already reported the person in this county, don't double
              // count them in the same county.
            }
            else
            {
              local.CountiesReported.RptDetail =
                TrimEnd(local.CountiesReported.RptDetail) + entities
                .ArCsePersonAddress.County + "/";

              // -- Count the child in the county.
              local.Report.Index = -1;

              do
              {
                ++local.Report.Index;
                local.Report.CheckSize();
              }
              while(!Equal(local.Report.Item.GlocalReportCsePersonAddress.
                County, entities.ArCsePersonAddress.County));

              local.Report.Update.GlocalReportChild.Count =
                local.Report.Item.GlocalReportChild.Count + 1;
            }
          }
        }

        if (local.ArAddress.Count == 0)
        {
          if (Find(local.CountiesReported.RptDetail, "  /") != 0)
          {
            local.AlreadyReported.Flag = "*";
          }
          else
          {
            local.AlreadyReported.Flag = "";
          }

          local.EabReportSend.RptDetail = "Child " + entities
            .CsePerson.Number + "  Case " + entities.Case1.Number + "  County " +
            "  " + local.AlreadyReported.Flag;
          UseCabControlReport1();

          if (!Equal(local.Write.Status, "OK"))
          {
            // -- Write to the error report.
            local.Write.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "(04) Error Writing Control Report...  Returned Status = " + local
              .Write.Status;
            UseCabErrorReport1();

            // -- Set Abort exit state and escape...
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          if (Find(local.CountiesReported.RptDetail, "  /") != 0)
          {
            // --  We already reported the person in this county, don't double 
            // count them in the same county.
            goto Test1;
          }

          // -- Report the person under the Unknown county.
          local.Report.Index = 0;
          local.Report.CheckSize();

          local.Report.Update.GlocalReportChild.Count =
            local.Report.Item.GlocalReportChild.Count + 1;
          local.CountiesReported.RptDetail =
            TrimEnd(local.CountiesReported.RptDetail) + "  /";
        }

Test1:
        ;
      }
    }

    for(local.Counter.Count = 1; local.Counter.Count <= 3; ++
      local.Counter.Count)
    {
      switch(local.Counter.Count)
      {
        case 1:
          local.EabReportSend.RptDetail = "";

          break;
        case 2:
          local.EabReportSend.RptDetail = "Start Adult Counts - Time: " + NumberToString
            (TimeToInt(Time(Now())), 15);

          break;
        case 3:
          local.EabReportSend.RptDetail = "";

          break;
        default:
          break;
      }

      UseCabControlReport1();

      if (!Equal(local.Write.Status, "OK"))
      {
        // -- Write to the error report.
        local.Write.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "(04) Error Writing Control Report...  Returned Status = " + local
          .Write.Status;
        UseCabErrorReport1();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // -- Determine number of adults impacted by Hurricane Katrina.  Grouped by 
    // county of the AR address.
    // -------------------------------------------------------------------------------------------------------------------------
    foreach(var item in ReadCsePerson1())
    {
      local.CountiesReported.RptDetail = "";

      foreach(var item1 in ReadCaseCaseRole1())
      {
        local.ArAddress.Count = 0;

        if (ReadCsePersonAddress3())
        {
          ++local.ArAddress.Count;

          if (Find(String(
            local.CountiesReported.RptDetail,
            EabReportSend.RptDetail_MaxLength),
            entities.ArCsePersonAddress.County + "/") != 0)
          {
            local.AlreadyReported.Flag = "*";
          }
          else
          {
            local.AlreadyReported.Flag = "";
          }

          local.EabReportSend.RptDetail = "Adult " + entities
            .CsePerson.Number + "  Case " + entities.Case1.Number + "  County " +
            entities.ArCsePersonAddress.County + local.AlreadyReported.Flag;
          UseCabControlReport1();

          if (!Equal(local.Write.Status, "OK"))
          {
            // -- Write to the error report.
            local.Write.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "(04) Error Writing Control Report...  Returned Status = " + local
              .Write.Status;
            UseCabErrorReport1();

            // -- Set Abort exit state and escape...
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          if (Find(String(
            local.CountiesReported.RptDetail,
            EabReportSend.RptDetail_MaxLength),
            entities.ArCsePersonAddress.County + "/") != 0)
          {
            // --  We already reported the person in this county, don't double 
            // count them in the same county.
          }
          else
          {
            local.CountiesReported.RptDetail =
              TrimEnd(local.CountiesReported.RptDetail) + entities
              .ArCsePersonAddress.County + "/";

            // -- Count the adult in the county.
            local.Report.Index = -1;

            do
            {
              ++local.Report.Index;
              local.Report.CheckSize();
            }
            while(!Equal(local.Report.Item.GlocalReportCsePersonAddress.County,
              entities.ArCsePersonAddress.County));

            local.Report.Update.GlocalReportAdult.Count =
              local.Report.Item.GlocalReportAdult.Count + 1;
          }
        }

        if (local.ArAddress.Count == 0)
        {
          if (Find(local.CountiesReported.RptDetail, "  /") != 0)
          {
            local.AlreadyReported.Flag = "*";
          }
          else
          {
            local.AlreadyReported.Flag = "";
          }

          local.EabReportSend.RptDetail = "Adult " + entities
            .CsePerson.Number + "  Case " + entities.Case1.Number + "  County " +
            "  " + local.AlreadyReported.Flag;
          UseCabControlReport1();

          if (!Equal(local.Write.Status, "OK"))
          {
            // -- Write to the error report.
            local.Write.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "(04) Error Writing Control Report...  Returned Status = " + local
              .Write.Status;
            UseCabErrorReport1();

            // -- Set Abort exit state and escape...
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          if (Find(local.CountiesReported.RptDetail, "  /") != 0)
          {
            // --  We already reported the person in this county, don't double 
            // count them in the same county.
            goto Test2;
          }

          // -- Report the person under the Unknown county.
          local.Report.Index = 0;
          local.Report.CheckSize();

          local.Report.Update.GlocalReportAdult.Count =
            local.Report.Item.GlocalReportAdult.Count + 1;
          local.CountiesReported.RptDetail =
            TrimEnd(local.CountiesReported.RptDetail) + "  /";
        }

Test2:
        ;
      }
    }

    for(local.Counter.Count = 1; local.Counter.Count <= 3; ++
      local.Counter.Count)
    {
      switch(local.Counter.Count)
      {
        case 1:
          local.EabReportSend.RptDetail = "";

          break;
        case 2:
          local.EabReportSend.RptDetail = "Start Case Counts  - Time: " + NumberToString
            (TimeToInt(Time(Now())), 15);

          break;
        case 3:
          local.EabReportSend.RptDetail = "";

          break;
        default:
          break;
      }

      UseCabControlReport1();

      if (!Equal(local.Write.Status, "OK"))
      {
        // -- Write to the error report.
        local.Write.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "(04) Error Writing Control Report...  Returned Status = " + local
          .Write.Status;
        UseCabErrorReport1();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // -- Determine number of cse cases impacted by Hurricane Katrina.  Grouped 
    // by county of the AR address.
    // -------------------------------------------------------------------------------------------------------------------------
    foreach(var item in ReadCase())
    {
      local.CountiesReported.RptDetail = "";
      local.ArAddress.Count = 0;

      foreach(var item1 in ReadCaseRole())
      {
        local.ArAddress.Count = 0;

        if (Equal(entities.CaseRole.Type1, "CH"))
        {
          if (ReadApplicantRecipient())
          {
            if (ReadCsePersonAddress2())
            {
              ++local.ArAddress.Count;

              if (Find(String(
                local.CountiesReported.RptDetail,
                EabReportSend.RptDetail_MaxLength),
                entities.ArCsePersonAddress.County + "/") != 0)
              {
                local.AlreadyReported.Flag = "*";
              }
              else
              {
                local.AlreadyReported.Flag = "";
              }

              local.EabReportSend.RptDetail = "      " + "          " + "  Case " +
                entities.Case1.Number + "  County " + entities
                .ArCsePersonAddress.County + local.AlreadyReported.Flag;
              UseCabControlReport1();

              if (!Equal(local.Write.Status, "OK"))
              {
                // -- Write to the error report.
                local.Write.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "(04) Error Writing Control Report...  Returned Status = " + local
                  .Write.Status;
                UseCabErrorReport1();

                // -- Set Abort exit state and escape...
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }

              if (Find(String(
                local.CountiesReported.RptDetail,
                EabReportSend.RptDetail_MaxLength),
                entities.ArCsePersonAddress.County + "/") != 0)
              {
                // --  We already reported the case in this county, don't double
                // count them in the same county.
              }
              else
              {
                local.CountiesReported.RptDetail =
                  TrimEnd(local.CountiesReported.RptDetail) + entities
                  .ArCsePersonAddress.County + "/";

                // -- Count the case in the county.
                local.Report.Index = -1;

                do
                {
                  ++local.Report.Index;
                  local.Report.CheckSize();
                }
                while(!Equal(local.Report.Item.GlocalReportCsePersonAddress.
                  County, entities.ArCsePersonAddress.County));

                local.Report.Update.GlocalReportCase.Count =
                  local.Report.Item.GlocalReportCase.Count + 1;
              }
            }
          }
        }
        else if (ReadCsePersonAddress1())
        {
          ++local.ArAddress.Count;

          if (Find(String(
            local.CountiesReported.RptDetail,
            EabReportSend.RptDetail_MaxLength),
            entities.ArCsePersonAddress.County + "/") != 0)
          {
            local.AlreadyReported.Flag = "*";
          }
          else
          {
            local.AlreadyReported.Flag = "";
          }

          local.EabReportSend.RptDetail = "      " + "          " + "  Case " +
            entities.Case1.Number + "  County " + entities
            .ArCsePersonAddress.County + local.AlreadyReported.Flag;
          UseCabControlReport1();

          if (!Equal(local.Write.Status, "OK"))
          {
            // -- Write to the error report.
            local.Write.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "(04) Error Writing Control Report...  Returned Status = " + local
              .Write.Status;
            UseCabErrorReport1();

            // -- Set Abort exit state and escape...
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          if (Find(String(
            local.CountiesReported.RptDetail,
            EabReportSend.RptDetail_MaxLength),
            entities.ArCsePersonAddress.County + "/") != 0)
          {
            // --  We already reported the case in this county, don't double 
            // count them in the same county.
          }
          else
          {
            local.CountiesReported.RptDetail =
              TrimEnd(local.CountiesReported.RptDetail) + entities
              .ArCsePersonAddress.County + "/";

            // -- Count the case in the county.
            local.Report.Index = -1;

            do
            {
              ++local.Report.Index;
              local.Report.CheckSize();
            }
            while(!Equal(local.Report.Item.GlocalReportCsePersonAddress.County,
              entities.ArCsePersonAddress.County));

            local.Report.Update.GlocalReportCase.Count =
              local.Report.Item.GlocalReportCase.Count + 1;
          }
        }

        if (local.ArAddress.Count == 0)
        {
          if (Find(local.CountiesReported.RptDetail, "  /") != 0)
          {
            local.AlreadyReported.Flag = "*";
          }
          else
          {
            local.AlreadyReported.Flag = "";
          }

          local.EabReportSend.RptDetail = "      " + "          " + "  Case " +
            entities.Case1.Number + "  County " + "  " + local
            .AlreadyReported.Flag;
          UseCabControlReport1();

          if (!Equal(local.Write.Status, "OK"))
          {
            // -- Write to the error report.
            local.Write.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "(04) Error Writing Control Report...  Returned Status = " + local
              .Write.Status;
            UseCabErrorReport1();

            // -- Set Abort exit state and escape...
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          if (Find(local.CountiesReported.RptDetail, "  /") != 0)
          {
            // --  We already reported the case in this county, don't double 
            // count them in the same county.
            goto Test3;
          }

          // -- Report the case under the Unknown county.
          local.Report.Index = 0;
          local.Report.CheckSize();

          local.Report.Update.GlocalReportCase.Count =
            local.Report.Item.GlocalReportCase.Count + 1;
          local.CountiesReported.RptDetail =
            TrimEnd(local.CountiesReported.RptDetail) + "  /";
        }

Test3:
        ;
      }
    }

    for(local.Counter.Count = 1; local.Counter.Count <= 3; ++
      local.Counter.Count)
    {
      switch(local.Counter.Count)
      {
        case 1:
          local.EabReportSend.RptDetail = "";

          break;
        case 2:
          local.EabReportSend.RptDetail = "Start Collections  - Time: " + NumberToString
            (TimeToInt(Time(Now())), 15);

          break;
        case 3:
          local.EabReportSend.RptDetail = "";

          break;
        default:
          break;
      }

      UseCabControlReport1();

      if (!Equal(local.Write.Status, "OK"))
      {
        // -- Write to the error report.
        local.Write.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "(04) Error Writing Control Report...  Returned Status = " + local
          .Write.Status;
        UseCabErrorReport1();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // -- Determine amount of collections during the reporting period for 
    // supported persons impacted by Hurricane Katrina.  Grouped by county of
    // the AR address.
    // -------------------------------------------------------------------------------------------------------------------------
    foreach(var item in ReadSupportedCsePerson())
    {
      local.Collection.TotalCurrency = 0;
      local.AfFcCollection.TotalCurrency = 0;
      local.NfNcCollection.TotalCurrency = 0;
      local.NaCollection.TotalCurrency = 0;

      foreach(var item1 in ReadCollection())
      {
        if (Equal(entities.Collection.ProgramAppliedTo, "AF") || Equal
          (entities.Collection.ProgramAppliedTo, "FC") || Equal
          (entities.Collection.ProgramAppliedTo, "AFI") || Equal
          (entities.Collection.ProgramAppliedTo, "FCI"))
        {
          local.AfFcCollection.TotalCurrency += entities.Collection.Amount;
        }
        else if (Equal(entities.Collection.ProgramAppliedTo, "NF") || Equal
          (entities.Collection.ProgramAppliedTo, "NC"))
        {
          local.NfNcCollection.TotalCurrency += entities.Collection.Amount;
        }
        else if (Equal(entities.Collection.ProgramAppliedTo, "NA") || Equal
          (entities.Collection.ProgramAppliedTo, "NAI"))
        {
          local.NaCollection.TotalCurrency += entities.Collection.Amount;
        }
        else
        {
          // Skip any program not listed above.
          continue;
        }

        local.Collection.TotalCurrency += entities.Collection.Amount;
      }

      if (local.Collection.TotalCurrency > 0)
      {
        local.CountiesReported.RptDetail = "";
        local.ArAddress.Count = 0;

        foreach(var item1 in ReadCaseRoleCase())
        {
          local.ArAddress.Count = 0;

          if (Equal(entities.CaseRole.Type1, "CH"))
          {
            if (ReadApplicantRecipient())
            {
              if (ReadCsePersonAddress2())
              {
                ++local.ArAddress.Count;

                if (Find(String(
                  local.CountiesReported.RptDetail,
                  EabReportSend.RptDetail_MaxLength),
                  entities.ArCsePersonAddress.County + "/") != 0)
                {
                  local.AlreadyReported.Flag = "*";
                }
                else
                {
                  local.AlreadyReported.Flag = "";
                }

                // -- log af/fc, na, nf/nc amounts individually...
                local.EabReportSend.RptDetail = "Supp  " + entities
                  .CsePerson.Number + "  Case " + entities.Case1.Number + "  County " +
                  entities.ArCsePersonAddress.County + local
                  .AlreadyReported.Flag + "  " + NumberToString
                  ((long)(local.AfFcCollection.TotalCurrency * 100), 15);
                local.EabReportSend.RptDetail =
                  TrimEnd(local.EabReportSend.RptDetail) + "   " + NumberToString
                  ((long)(local.NfNcCollection.TotalCurrency * 100), 15);
                local.EabReportSend.RptDetail =
                  TrimEnd(local.EabReportSend.RptDetail) + "   " + NumberToString
                  ((long)(local.NaCollection.TotalCurrency * 100), 15);
                local.EabReportSend.RptDetail =
                  TrimEnd(local.EabReportSend.RptDetail) + "   " + NumberToString
                  ((long)(local.Collection.TotalCurrency * 100), 15);
                UseCabControlReport1();

                if (!Equal(local.Write.Status, "OK"))
                {
                  // -- Write to the error report.
                  local.Write.Action = "WRITE";
                  local.EabReportSend.RptDetail =
                    "(04) Error Writing Control Report...  Returned Status = " +
                    local.Write.Status;
                  UseCabErrorReport1();

                  // -- Set Abort exit state and escape...
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                }

                if (Find(String(
                  local.CountiesReported.RptDetail,
                  EabReportSend.RptDetail_MaxLength),
                  entities.ArCsePersonAddress.County + "/") != 0)
                {
                  // --  We already reported the collections in this county, 
                  // don't double count them in the same county.
                }
                else
                {
                  local.CountiesReported.RptDetail =
                    TrimEnd(local.CountiesReported.RptDetail) + entities
                    .ArCsePersonAddress.County + "/";

                  // -- Count the collections in the county.
                  local.Report.Index = -1;

                  do
                  {
                    ++local.Report.Index;
                    local.Report.CheckSize();
                  }
                  while(!Equal(local.Report.Item.GlocalReportCsePersonAddress.
                    County, entities.ArCsePersonAddress.County));

                  local.Report.Update.GlocalReportCollections.TotalCurrency =
                    local.Report.Item.GlocalReportCollections.TotalCurrency + local
                    .Collection.TotalCurrency;
                  local.Report.Update.GlocalReportAfFcCollections.
                    TotalCurrency =
                      local.Report.Item.GlocalReportAfFcCollections.
                      TotalCurrency + local.AfFcCollection.TotalCurrency;
                  local.Report.Update.GlocalReportNaCollections.TotalCurrency =
                    local.Report.Item.GlocalReportNaCollections.TotalCurrency +
                    local.NaCollection.TotalCurrency;
                  local.Report.Update.GlocalReportNfNcCollections.
                    TotalCurrency =
                      local.Report.Item.GlocalReportNfNcCollections.
                      TotalCurrency + local.NfNcCollection.TotalCurrency;
                }
              }
            }
          }
          else if (ReadCsePersonAddress1())
          {
            ++local.ArAddress.Count;

            if (Find(String(
              local.CountiesReported.RptDetail,
              EabReportSend.RptDetail_MaxLength),
              entities.ArCsePersonAddress.County + "/") != 0)
            {
              local.AlreadyReported.Flag = "*";
            }
            else
            {
              local.AlreadyReported.Flag = "";
            }

            // -- log af/fc, na, nf/nc amounts individually...
            local.EabReportSend.RptDetail = "Supp  " + entities
              .CsePerson.Number + "  Case " + entities.Case1.Number + "  County " +
              entities.ArCsePersonAddress.County + local
              .AlreadyReported.Flag + "  " + NumberToString
              ((long)(local.AfFcCollection.TotalCurrency * 100), 15);
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "   " + NumberToString
              ((long)(local.NfNcCollection.TotalCurrency * 100), 15);
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "   " + NumberToString
              ((long)(local.NaCollection.TotalCurrency * 100), 15);
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "   " + NumberToString
              ((long)(local.Collection.TotalCurrency * 100), 15);
            UseCabControlReport1();

            if (!Equal(local.Write.Status, "OK"))
            {
              // -- Write to the error report.
              local.Write.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "(04) Error Writing Control Report...  Returned Status = " + local
                .Write.Status;
              UseCabErrorReport1();

              // -- Set Abort exit state and escape...
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            if (Find(String(
              local.CountiesReported.RptDetail,
              EabReportSend.RptDetail_MaxLength),
              entities.ArCsePersonAddress.County + "/") != 0)
            {
              // --  We already reported the collections in this county, don't 
              // double count them in the same county.
            }
            else
            {
              local.CountiesReported.RptDetail =
                TrimEnd(local.CountiesReported.RptDetail) + entities
                .ArCsePersonAddress.County + "/";

              // -- Count the collections in the county.
              local.Report.Index = -1;

              do
              {
                ++local.Report.Index;
                local.Report.CheckSize();
              }
              while(!Equal(local.Report.Item.GlocalReportCsePersonAddress.
                County, entities.ArCsePersonAddress.County));

              local.Report.Update.GlocalReportCollections.TotalCurrency =
                local.Report.Item.GlocalReportCollections.TotalCurrency + local
                .Collection.TotalCurrency;
              local.Report.Update.GlocalReportAfFcCollections.TotalCurrency =
                local.Report.Item.GlocalReportAfFcCollections.TotalCurrency + local
                .AfFcCollection.TotalCurrency;
              local.Report.Update.GlocalReportNaCollections.TotalCurrency =
                local.Report.Item.GlocalReportNaCollections.TotalCurrency + local
                .NaCollection.TotalCurrency;
              local.Report.Update.GlocalReportNfNcCollections.TotalCurrency =
                local.Report.Item.GlocalReportNfNcCollections.TotalCurrency + local
                .NfNcCollection.TotalCurrency;
            }
          }

          if (local.ArAddress.Count == 0)
          {
            if (Find(local.CountiesReported.RptDetail, "  /") != 0)
            {
              local.AlreadyReported.Flag = "*";
            }
            else
            {
              local.AlreadyReported.Flag = "";
            }

            // -- log af/fc, na, nf/nc amounts individually...
            local.EabReportSend.RptDetail = "Supp  " + entities
              .CsePerson.Number + "  Case " + entities.Case1.Number + "  County " +
              "  " + local.AlreadyReported.Flag + "  " + NumberToString
              ((long)(local.AfFcCollection.TotalCurrency * 100), 15);
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "   " + NumberToString
              ((long)(local.NfNcCollection.TotalCurrency * 100), 15);
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "   " + NumberToString
              ((long)(local.NaCollection.TotalCurrency * 100), 15);
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "   " + NumberToString
              ((long)(local.Collection.TotalCurrency * 100), 15);
            UseCabControlReport1();

            if (!Equal(local.Write.Status, "OK"))
            {
              // -- Write to the error report.
              local.Write.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "(04) Error Writing Control Report...  Returned Status = " + local
                .Write.Status;
              UseCabErrorReport1();

              // -- Set Abort exit state and escape...
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            if (Find(local.CountiesReported.RptDetail, "  /") != 0)
            {
              // --  We already reported the collections in this county, don't 
              // double count them in the same county.
              goto Test4;
            }

            // -- Report the collections under the Unknown county.
            local.Report.Index = 0;
            local.Report.CheckSize();

            local.Report.Update.GlocalReportCollections.TotalCurrency =
              local.Report.Item.GlocalReportCollections.TotalCurrency + local
              .Collection.TotalCurrency;
            local.Report.Update.GlocalReportAfFcCollections.TotalCurrency =
              local.Report.Item.GlocalReportAfFcCollections.TotalCurrency + local
              .AfFcCollection.TotalCurrency;
            local.Report.Update.GlocalReportNaCollections.TotalCurrency =
              local.Report.Item.GlocalReportNaCollections.TotalCurrency + local
              .NaCollection.TotalCurrency;
            local.Report.Update.GlocalReportNfNcCollections.TotalCurrency =
              local.Report.Item.GlocalReportNfNcCollections.TotalCurrency + local
              .NfNcCollection.TotalCurrency;
            local.CountiesReported.RptDetail =
              TrimEnd(local.CountiesReported.RptDetail) + "  /";
          }

Test4:
          ;
        }
      }
    }

    for(local.Counter.Count = 1; local.Counter.Count <= 3; ++
      local.Counter.Count)
    {
      switch(local.Counter.Count)
      {
        case 1:
          local.EabReportSend.RptDetail = "";

          break;
        case 2:
          local.EabReportSend.RptDetail = "Start Report       - Time: " + NumberToString
            (TimeToInt(Time(Now())), 15);

          break;
        case 3:
          local.EabReportSend.RptDetail = "";

          break;
        default:
          break;
      }

      UseCabControlReport1();

      if (!Equal(local.Write.Status, "OK"))
      {
        // -- Write to the error report.
        local.Write.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "(04) Error Writing Control Report...  Returned Status = " + local
          .Write.Status;
        UseCabErrorReport1();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    // -- Write report header.
    for(local.Counter.Count = 1; local.Counter.Count <= 7; ++
      local.Counter.Count)
    {
      switch(local.Counter.Count)
      {
        case 1:
          local.EabReportSend.RptDetail = "";

          break;
        case 2:
          local.EabReportSend.RptDetail = local.ReportMonth.RptDetail;

          break;
        case 3:
          local.EabReportSend.RptDetail = "";

          break;
        case 4:
          local.EabReportSend.RptDetail = "";

          break;
        case 5:
          local.EabReportSend.RptDetail =
            "                                                            AF & FC       NF & NC          NA          Total";
            

          break;
        case 6:
          local.EabReportSend.RptDetail =
            "County                     Cases    Children    Adults    Collections   Collections   Collections   Collections";
            

          break;
        case 7:
          local.EabReportSend.RptDetail =
            "-----------------------    ------   --------    ------    -----------   -----------   -----------   -----------";
            

          break;
        default:
          break;
      }

      UseCabBusinessReport1();

      if (!Equal(local.Write.Status, "OK"))
      {
        // -- Write to the error report.
        local.Write.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "(05) Error Writing Business Report...  Returned Status = " + local
          .Write.Status;
        UseCabErrorReport1();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    // -- Write counts for all counties to the report.
    local.Report.Index = 0;

    for(var limit = local.Report.Count; local.Report.Index < limit; ++
      local.Report.Index)
    {
      if (!local.Report.CheckSize())
      {
        break;
      }

      // --  Keep a running statewide total of the number of cases, adults, 
      // children, and collections.
      local.StatewideAdult.Count += local.Report.Item.GlocalReportAdult.Count;
      local.StatewideCase.Count += local.Report.Item.GlocalReportCase.Count;
      local.StatewideChild.Count += local.Report.Item.GlocalReportChild.Count;
      local.StatewideCollections.TotalCurrency += local.Report.Item.
        GlocalReportCollections.TotalCurrency;
      local.StatewideAfFcCollection.TotalCurrency += local.Report.Item.
        GlocalReportAfFcCollections.TotalCurrency;
      local.StatewideNfNcCollection.TotalCurrency += local.Report.Item.
        GlocalReportNfNcCollections.TotalCurrency;
      local.StatewideNaCollection.TotalCurrency += local.Report.Item.
        GlocalReportNaCollections.TotalCurrency;
      local.EabReportSend.RptDetail =
        local.Report.Item.GlocalReportCsePersonAddress.County ?? Spaces(132);
      local.EabReportSend.RptDetail =
        Substring(local.EabReportSend.RptDetail,
        EabReportSend.RptDetail_MaxLength, 1, 3) + Substring
        (local.Report.Item.GlocalReportCodeValue.Description, 1, 20);

      for(local.Counter.Count = 1; local.Counter.Count <= 7; ++
        local.Counter.Count)
      {
        switch(local.Counter.Count)
        {
          case 1:
            local.Common.Count = local.Report.Item.GlocalReportCase.Count;

            break;
          case 2:
            local.Common.Count = local.Report.Item.GlocalReportChild.Count;

            break;
          case 3:
            local.Common.Count = local.Report.Item.GlocalReportAdult.Count;

            break;
          case 4:
            local.Common.Count =
              (int)(local.Report.Item.GlocalReportAfFcCollections.
                TotalCurrency * 100);

            break;
          case 5:
            local.Common.Count =
              (int)(local.Report.Item.GlocalReportNfNcCollections.
                TotalCurrency * 100);

            break;
          case 6:
            local.Common.Count =
              (int)(local.Report.Item.GlocalReportNaCollections.TotalCurrency *
              100);

            break;
          case 7:
            local.Common.Count =
              (int)(local.Report.Item.GlocalReportCollections.TotalCurrency * 100
              );

            break;
          default:
            break;
        }

        if (local.Common.Count == 0)
        {
          if (local.Counter.Count >= 4)
          {
            local.TextWorkArea.Text10 = "$     0.00";
          }
          else
          {
            local.TextWorkArea.Text10 = "         0";
          }
        }
        else
        {
          local.TextWorkArea.Text10 = NumberToString(local.Common.Count, 6, 10);
          local.Common.Count = Verify(local.TextWorkArea.Text10, "0");
          local.TextWorkArea.Text10 =
            Substring(local.NullTextWorkArea.Text10,
            TextWorkArea.Text10_MaxLength, 1, local.Common.Count - 1) + Substring
            (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength,
            local.Common.Count, 10 - local.Common.Count + 1);

          if (local.Counter.Count >= 4)
          {
            local.TextWorkArea.Text10 =
              Substring(local.TextWorkArea.Text10,
              TextWorkArea.Text10_MaxLength, 2, 7) + "." + Substring
              (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength, 9, 2);
          }
        }

        switch(local.Counter.Count)
        {
          case 1:
            local.EabReportSend.RptDetail =
              Substring(local.EabReportSend.RptDetail,
              EabReportSend.RptDetail_MaxLength, 1, 23) + local
              .TextWorkArea.Text10;

            break;
          case 2:
            local.EabReportSend.RptDetail =
              Substring(local.EabReportSend.RptDetail,
              EabReportSend.RptDetail_MaxLength, 1, 34) + local
              .TextWorkArea.Text10;

            break;
          case 3:
            local.EabReportSend.RptDetail =
              Substring(local.EabReportSend.RptDetail,
              EabReportSend.RptDetail_MaxLength, 1, 44) + local
              .TextWorkArea.Text10;

            break;
          case 4:
            local.EabReportSend.RptDetail =
              Substring(local.EabReportSend.RptDetail,
              EabReportSend.RptDetail_MaxLength, 1, 59) + "$" + Substring
              (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength, 2, 9);

            break;
          case 5:
            local.EabReportSend.RptDetail =
              Substring(local.EabReportSend.RptDetail,
              EabReportSend.RptDetail_MaxLength, 1, 73) + "$" + Substring
              (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength, 2, 9);

            break;
          case 6:
            local.EabReportSend.RptDetail =
              Substring(local.EabReportSend.RptDetail,
              EabReportSend.RptDetail_MaxLength, 1, 87) + "$" + Substring
              (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength, 2, 9);

            break;
          case 7:
            local.EabReportSend.RptDetail =
              Substring(local.EabReportSend.RptDetail,
              EabReportSend.RptDetail_MaxLength, 1, 101) + "$" + Substring
              (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength, 2, 9);

            break;
          default:
            break;
        }
      }

      UseCabBusinessReport1();

      if (!Equal(local.Write.Status, "OK"))
      {
        // -- Write to the error report.
        local.Write.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "(05) Error Writing Business Report...  Returned Status = " + local
          .Write.Status;
        UseCabErrorReport1();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    local.Report.CheckIndex();

    // -- Write report footer.
    local.EabReportSend.RptDetail =
      "-----------------------    ------   --------    ------    -----------   -----------   -----------   -----------";
      
    UseCabBusinessReport1();

    if (!Equal(local.Write.Status, "OK"))
    {
      // -- Write to the error report.
      local.Write.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "(05) Error Writing Business Report...  Returned Status = " + local
        .Write.Status;
      UseCabErrorReport1();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "TOTAL";

    for(local.Counter.Count = 1; local.Counter.Count <= 7; ++
      local.Counter.Count)
    {
      switch(local.Counter.Count)
      {
        case 1:
          local.Common.Count = local.StatewideCase.Count;

          break;
        case 2:
          local.Common.Count = local.StatewideChild.Count;

          break;
        case 3:
          local.Common.Count = local.StatewideAdult.Count;

          break;
        case 4:
          local.Common.Count =
            (int)(local.StatewideAfFcCollection.TotalCurrency * 100);

          break;
        case 5:
          local.Common.Count =
            (int)(local.StatewideNfNcCollection.TotalCurrency * 100);

          break;
        case 6:
          local.Common.Count =
            (int)(local.StatewideNaCollection.TotalCurrency * 100);

          break;
        case 7:
          local.Common.Count =
            (int)(local.StatewideCollections.TotalCurrency * 100);

          break;
        default:
          break;
      }

      if (local.Common.Count == 0)
      {
        if (local.Counter.Count >= 4)
        {
          local.TextWorkArea.Text10 = "$     0.00";
        }
        else
        {
          local.TextWorkArea.Text10 = "         0";
        }
      }
      else
      {
        local.TextWorkArea.Text10 = NumberToString(local.Common.Count, 6, 10);
        local.Common.Count = Verify(local.TextWorkArea.Text10, "0");
        local.TextWorkArea.Text10 =
          Substring(local.NullTextWorkArea.Text10,
          TextWorkArea.Text10_MaxLength, 1, local.Common.Count - 1) + Substring
          (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength,
          local.Common.Count, 10 - local.Common.Count + 1);

        if (local.Counter.Count >= 4)
        {
          local.TextWorkArea.Text10 =
            Substring(local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength,
            2, 7) + "." + Substring
            (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength, 9, 2);
        }
      }

      switch(local.Counter.Count)
      {
        case 1:
          local.EabReportSend.RptDetail =
            Substring(local.EabReportSend.RptDetail,
            EabReportSend.RptDetail_MaxLength, 1, 23) + local
            .TextWorkArea.Text10;

          break;
        case 2:
          local.EabReportSend.RptDetail =
            Substring(local.EabReportSend.RptDetail,
            EabReportSend.RptDetail_MaxLength, 1, 34) + local
            .TextWorkArea.Text10;

          break;
        case 3:
          local.EabReportSend.RptDetail =
            Substring(local.EabReportSend.RptDetail,
            EabReportSend.RptDetail_MaxLength, 1, 44) + local
            .TextWorkArea.Text10;

          break;
        case 4:
          local.EabReportSend.RptDetail =
            Substring(local.EabReportSend.RptDetail,
            EabReportSend.RptDetail_MaxLength, 1, 59) + "$" + Substring
            (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength, 2, 9);

          break;
        case 5:
          local.EabReportSend.RptDetail =
            Substring(local.EabReportSend.RptDetail,
            EabReportSend.RptDetail_MaxLength, 1, 73) + "$" + Substring
            (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength, 2, 9);

          break;
        case 6:
          local.EabReportSend.RptDetail =
            Substring(local.EabReportSend.RptDetail,
            EabReportSend.RptDetail_MaxLength, 1, 87) + "$" + Substring
            (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength, 2, 9);

          break;
        case 7:
          local.EabReportSend.RptDetail =
            Substring(local.EabReportSend.RptDetail,
            EabReportSend.RptDetail_MaxLength, 1, 101) + "$" + Substring
            (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength, 2, 9);

          break;
        default:
          break;
      }
    }

    UseCabBusinessReport1();

    if (!Equal(local.Write.Status, "OK"))
    {
      // -- Write to the error report.
      local.Write.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "(05) Error Writing Business Report...  Returned Status = " + local
        .Write.Status;
      UseCabErrorReport1();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Close the Business Report.
    // -------------------------------------------------------------------------------------------------------------------------
    UseCabBusinessReport2();

    if (!Equal(local.Close.Status, "OK"))
    {
      // -- Write to the error report.
      local.Write.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error Closing Business Report...  Returned Status = " + local
        .Write.Status;
      UseCabErrorReport1();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Close the Control Report.
    // -------------------------------------------------------------------------------------------------------------------------
    UseCabControlReport2();

    if (!Equal(local.Close.Status, "OK"))
    {
      // -- Write to the error report.
      local.EabReportSend.RptDetail =
        "Error Closing Control Report...  Returned Status = " + local
        .Close.Status;
      UseCabErrorReport1();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Close the Error Report.
    // -------------------------------------------------------------------------------------------------------------------------
    UseCabErrorReport2();

    if (!Equal(local.Close.Status, "OK"))
    {
      ExitState = "SI0000_ERR_CLOSING_ERR_RPT_FILE";

      return;
    }

    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private void UseCabBusinessReport1()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.Write.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.Write.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport2()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.Close.Action;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.Close.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport3()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.Open.Action;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.Open.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.Write.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.Write.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.Close.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.Close.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport3()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.Open.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.Open.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.Write.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.Write.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.Close.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.Close.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.Open.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.Open.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private bool ReadApplicantRecipient()
  {
    entities.ApplicantRecipient.Populated = false;

    return Read("ReadApplicantRecipient",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.End.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDate", local.Start.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ApplicantRecipient.CasNumber = db.GetString(reader, 0);
        entities.ApplicantRecipient.CspNumber = db.GetString(reader, 1);
        entities.ApplicantRecipient.Type1 = db.GetString(reader, 2);
        entities.ApplicantRecipient.Identifier = db.GetInt32(reader, 3);
        entities.ApplicantRecipient.StartDate = db.GetNullableDate(reader, 4);
        entities.ApplicantRecipient.EndDate = db.GetNullableDate(reader, 5);
        entities.ApplicantRecipient.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCase()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase",
      null,
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseCaseRole1()
  {
    entities.CaseRole.Populated = false;
    entities.Case1.Populated = false;

    return ReadEach("ReadCaseCaseRole1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.End.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDate", local.Start.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.Populated = true;
        entities.Case1.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseCaseRole2()
  {
    entities.CaseRole.Populated = false;
    entities.Case1.Populated = false;

    return ReadEach("ReadCaseCaseRole2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.End.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDate", local.Start.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.Populated = true;
        entities.Case1.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseRole()
  {
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCaseRole",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.End.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDate", local.Start.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseRoleCase()
  {
    entities.CaseRole.Populated = false;
    entities.Case1.Populated = false;

    return ReadEach("ReadCaseRoleCase",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.End.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDate", local.Start.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.Populated = true;
        entities.Case1.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCodeValue()
  {
    entities.CodeValue.Populated = false;

    return ReadEach("ReadCodeValue",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CodeValue.Id = db.GetInt32(reader, 0);
        entities.CodeValue.CodId = db.GetNullableInt32(reader, 1);
        entities.CodeValue.Cdvalue = db.GetString(reader, 2);
        entities.CodeValue.EffectiveDate = db.GetDate(reader, 3);
        entities.CodeValue.ExpirationDate = db.GetDate(reader, 4);
        entities.CodeValue.Description = db.GetString(reader, 5);
        entities.CodeValue.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCollection()
  {
    System.Diagnostics.Debug.Assert(entities.Supported.Populated);
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection",
      (db, command) =>
      {
        db.SetDateTime(
          command, "timestamp1", local.Start.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2", local.End.Timestamp.GetValueOrDefault());
        db.SetNullableString(command, "cpaSupType", entities.Supported.Type1);
        db.SetNullableString(
          command, "cspSupNumber", entities.Supported.CspNumber);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.CollectionDt = db.GetDate(reader, 1);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 2);
        entities.Collection.CrtType = db.GetInt32(reader, 3);
        entities.Collection.CstId = db.GetInt32(reader, 4);
        entities.Collection.CrvId = db.GetInt32(reader, 5);
        entities.Collection.CrdId = db.GetInt32(reader, 6);
        entities.Collection.ObgId = db.GetInt32(reader, 7);
        entities.Collection.CspNumber = db.GetString(reader, 8);
        entities.Collection.CpaType = db.GetString(reader, 9);
        entities.Collection.OtrId = db.GetInt32(reader, 10);
        entities.Collection.OtrType = db.GetString(reader, 11);
        entities.Collection.OtyId = db.GetInt32(reader, 12);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 13);
        entities.Collection.Amount = db.GetDecimal(reader, 14);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 15);
        entities.Collection.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePerson1()
  {
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePerson1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.End.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDate", local.Start.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePerson2()
  {
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePerson2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.End.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDate", local.Start.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;

        return true;
      });
  }

  private bool ReadCsePersonAddress1()
  {
    System.Diagnostics.Debug.Assert(entities.CaseRole.Populated);
    entities.ArCsePersonAddress.Populated = false;

    return Read("ReadCsePersonAddress1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "endDate", local.Start.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.CaseRole.CspNumber);
      },
      (db, reader) =>
      {
        entities.ArCsePersonAddress.Identifier = db.GetDateTime(reader, 0);
        entities.ArCsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.ArCsePersonAddress.Type1 = db.GetNullableString(reader, 2);
        entities.ArCsePersonAddress.EndDate = db.GetNullableDate(reader, 3);
        entities.ArCsePersonAddress.LocationType = db.GetString(reader, 4);
        entities.ArCsePersonAddress.County = db.GetNullableString(reader, 5);
        entities.ArCsePersonAddress.Populated = true;
      });
  }

  private bool ReadCsePersonAddress2()
  {
    System.Diagnostics.Debug.Assert(entities.ApplicantRecipient.Populated);
    entities.ArCsePersonAddress.Populated = false;

    return Read("ReadCsePersonAddress2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "endDate", local.Start.Date.GetValueOrDefault());
        db.
          SetString(command, "cspNumber", entities.ApplicantRecipient.CspNumber);
          
      },
      (db, reader) =>
      {
        entities.ArCsePersonAddress.Identifier = db.GetDateTime(reader, 0);
        entities.ArCsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.ArCsePersonAddress.Type1 = db.GetNullableString(reader, 2);
        entities.ArCsePersonAddress.EndDate = db.GetNullableDate(reader, 3);
        entities.ArCsePersonAddress.LocationType = db.GetString(reader, 4);
        entities.ArCsePersonAddress.County = db.GetNullableString(reader, 5);
        entities.ArCsePersonAddress.Populated = true;
      });
  }

  private bool ReadCsePersonAddress3()
  {
    entities.ArCsePersonAddress.Populated = false;

    return Read("ReadCsePersonAddress3",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "endDate", local.Start.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ArCsePersonAddress.Identifier = db.GetDateTime(reader, 0);
        entities.ArCsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.ArCsePersonAddress.Type1 = db.GetNullableString(reader, 2);
        entities.ArCsePersonAddress.EndDate = db.GetNullableDate(reader, 3);
        entities.ArCsePersonAddress.LocationType = db.GetString(reader, 4);
        entities.ArCsePersonAddress.County = db.GetNullableString(reader, 5);
        entities.ArCsePersonAddress.Populated = true;
      });
  }

  private IEnumerable<bool> ReadSupportedCsePerson()
  {
    entities.Supported.Populated = false;
    entities.CsePerson.Populated = false;

    return ReadEach("ReadSupportedCsePerson",
      null,
      (db, reader) =>
      {
        entities.Supported.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.Supported.Type1 = db.GetString(reader, 1);
        entities.Supported.Populated = true;
        entities.CsePerson.Populated = true;

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
    /// <summary>A ReportGroup group.</summary>
    [Serializable]
    public class ReportGroup
    {
      /// <summary>
      /// A value of GlocalReportCodeValue.
      /// </summary>
      [JsonPropertyName("glocalReportCodeValue")]
      public CodeValue GlocalReportCodeValue
      {
        get => glocalReportCodeValue ??= new();
        set => glocalReportCodeValue = value;
      }

      /// <summary>
      /// A value of GlocalReportCsePersonAddress.
      /// </summary>
      [JsonPropertyName("glocalReportCsePersonAddress")]
      public CsePersonAddress GlocalReportCsePersonAddress
      {
        get => glocalReportCsePersonAddress ??= new();
        set => glocalReportCsePersonAddress = value;
      }

      /// <summary>
      /// A value of GlocalReportChild.
      /// </summary>
      [JsonPropertyName("glocalReportChild")]
      public Common GlocalReportChild
      {
        get => glocalReportChild ??= new();
        set => glocalReportChild = value;
      }

      /// <summary>
      /// A value of GlocalReportAdult.
      /// </summary>
      [JsonPropertyName("glocalReportAdult")]
      public Common GlocalReportAdult
      {
        get => glocalReportAdult ??= new();
        set => glocalReportAdult = value;
      }

      /// <summary>
      /// A value of GlocalReportCase.
      /// </summary>
      [JsonPropertyName("glocalReportCase")]
      public Common GlocalReportCase
      {
        get => glocalReportCase ??= new();
        set => glocalReportCase = value;
      }

      /// <summary>
      /// A value of GlocalReportCollections.
      /// </summary>
      [JsonPropertyName("glocalReportCollections")]
      public Common GlocalReportCollections
      {
        get => glocalReportCollections ??= new();
        set => glocalReportCollections = value;
      }

      /// <summary>
      /// A value of GlocalReportAfFcCollections.
      /// </summary>
      [JsonPropertyName("glocalReportAfFcCollections")]
      public Common GlocalReportAfFcCollections
      {
        get => glocalReportAfFcCollections ??= new();
        set => glocalReportAfFcCollections = value;
      }

      /// <summary>
      /// A value of GlocalReportNfNcCollections.
      /// </summary>
      [JsonPropertyName("glocalReportNfNcCollections")]
      public Common GlocalReportNfNcCollections
      {
        get => glocalReportNfNcCollections ??= new();
        set => glocalReportNfNcCollections = value;
      }

      /// <summary>
      /// A value of GlocalReportNaCollections.
      /// </summary>
      [JsonPropertyName("glocalReportNaCollections")]
      public Common GlocalReportNaCollections
      {
        get => glocalReportNaCollections ??= new();
        set => glocalReportNaCollections = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 200;

      private CodeValue glocalReportCodeValue;
      private CsePersonAddress glocalReportCsePersonAddress;
      private Common glocalReportChild;
      private Common glocalReportAdult;
      private Common glocalReportCase;
      private Common glocalReportCollections;
      private Common glocalReportAfFcCollections;
      private Common glocalReportNfNcCollections;
      private Common glocalReportNaCollections;
    }

    /// <summary>
    /// A value of ReportMonth.
    /// </summary>
    [JsonPropertyName("reportMonth")]
    public EabReportSend ReportMonth
    {
      get => reportMonth ??= new();
      set => reportMonth = value;
    }

    /// <summary>
    /// A value of StatewideAfFcCollection.
    /// </summary>
    [JsonPropertyName("statewideAfFcCollection")]
    public Common StatewideAfFcCollection
    {
      get => statewideAfFcCollection ??= new();
      set => statewideAfFcCollection = value;
    }

    /// <summary>
    /// A value of StatewideNfNcCollection.
    /// </summary>
    [JsonPropertyName("statewideNfNcCollection")]
    public Common StatewideNfNcCollection
    {
      get => statewideNfNcCollection ??= new();
      set => statewideNfNcCollection = value;
    }

    /// <summary>
    /// A value of StatewideNaCollection.
    /// </summary>
    [JsonPropertyName("statewideNaCollection")]
    public Common StatewideNaCollection
    {
      get => statewideNaCollection ??= new();
      set => statewideNaCollection = value;
    }

    /// <summary>
    /// A value of NaCollection.
    /// </summary>
    [JsonPropertyName("naCollection")]
    public Common NaCollection
    {
      get => naCollection ??= new();
      set => naCollection = value;
    }

    /// <summary>
    /// A value of NfNcCollection.
    /// </summary>
    [JsonPropertyName("nfNcCollection")]
    public Common NfNcCollection
    {
      get => nfNcCollection ??= new();
      set => nfNcCollection = value;
    }

    /// <summary>
    /// A value of AfFcCollection.
    /// </summary>
    [JsonPropertyName("afFcCollection")]
    public Common AfFcCollection
    {
      get => afFcCollection ??= new();
      set => afFcCollection = value;
    }

    /// <summary>
    /// A value of AlreadyReported.
    /// </summary>
    [JsonPropertyName("alreadyReported")]
    public Common AlreadyReported
    {
      get => alreadyReported ??= new();
      set => alreadyReported = value;
    }

    /// <summary>
    /// A value of ArAddress.
    /// </summary>
    [JsonPropertyName("arAddress")]
    public Common ArAddress
    {
      get => arAddress ??= new();
      set => arAddress = value;
    }

    /// <summary>
    /// A value of CountiesReported.
    /// </summary>
    [JsonPropertyName("countiesReported")]
    public EabReportSend CountiesReported
    {
      get => countiesReported ??= new();
      set => countiesReported = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public CsePersonAddress Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Common Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of NullDateWorkArea.
    /// </summary>
    [JsonPropertyName("nullDateWorkArea")]
    public DateWorkArea NullDateWorkArea
    {
      get => nullDateWorkArea ??= new();
      set => nullDateWorkArea = value;
    }

    /// <summary>
    /// A value of NullTextWorkArea.
    /// </summary>
    [JsonPropertyName("nullTextWorkArea")]
    public TextWorkArea NullTextWorkArea
    {
      get => nullTextWorkArea ??= new();
      set => nullTextWorkArea = value;
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
    /// A value of End.
    /// </summary>
    [JsonPropertyName("end")]
    public DateWorkArea End
    {
      get => end ??= new();
      set => end = value;
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
    /// A value of StatewideCollections.
    /// </summary>
    [JsonPropertyName("statewideCollections")]
    public Common StatewideCollections
    {
      get => statewideCollections ??= new();
      set => statewideCollections = value;
    }

    /// <summary>
    /// A value of StatewideAdult.
    /// </summary>
    [JsonPropertyName("statewideAdult")]
    public Common StatewideAdult
    {
      get => statewideAdult ??= new();
      set => statewideAdult = value;
    }

    /// <summary>
    /// A value of StatewideChild.
    /// </summary>
    [JsonPropertyName("statewideChild")]
    public Common StatewideChild
    {
      get => statewideChild ??= new();
      set => statewideChild = value;
    }

    /// <summary>
    /// A value of StatewideCase.
    /// </summary>
    [JsonPropertyName("statewideCase")]
    public Common StatewideCase
    {
      get => statewideCase ??= new();
      set => statewideCase = value;
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
    /// A value of Write.
    /// </summary>
    [JsonPropertyName("write")]
    public EabFileHandling Write
    {
      get => write ??= new();
      set => write = value;
    }

    /// <summary>
    /// A value of Close.
    /// </summary>
    [JsonPropertyName("close")]
    public EabFileHandling Close
    {
      get => close ??= new();
      set => close = value;
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
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
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
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    /// <summary>
    /// A value of Counter.
    /// </summary>
    [JsonPropertyName("counter")]
    public Common Counter
    {
      get => counter ??= new();
      set => counter = value;
    }

    /// <summary>
    /// A value of Open.
    /// </summary>
    [JsonPropertyName("open")]
    public EabFileHandling Open
    {
      get => open ??= new();
      set => open = value;
    }

    private EabReportSend reportMonth;
    private Common statewideAfFcCollection;
    private Common statewideNfNcCollection;
    private Common statewideNaCollection;
    private Common naCollection;
    private Common nfNcCollection;
    private Common afFcCollection;
    private Common alreadyReported;
    private Common arAddress;
    private EabReportSend countiesReported;
    private CsePersonAddress previous;
    private Common collection;
    private DateWorkArea nullDateWorkArea;
    private TextWorkArea nullTextWorkArea;
    private Common common;
    private Array<ReportGroup> report;
    private DateWorkArea end;
    private DateWorkArea start;
    private Common statewideCollections;
    private Common statewideAdult;
    private Common statewideChild;
    private Common statewideCase;
    private TextWorkArea textWorkArea;
    private EabFileHandling write;
    private EabFileHandling close;
    private ProgramProcessingInfo programProcessingInfo;
    private ExitStateWorkArea exitStateWorkArea;
    private EabReportSend eabReportSend;
    private External external;
    private Common counter;
    private EabFileHandling open;
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
    /// A value of ArCsePerson.
    /// </summary>
    [JsonPropertyName("arCsePerson")]
    public CsePerson ArCsePerson
    {
      get => arCsePerson ??= new();
      set => arCsePerson = value;
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
    /// A value of ArCsePersonAddress.
    /// </summary>
    [JsonPropertyName("arCsePersonAddress")]
    public CsePersonAddress ArCsePersonAddress
    {
      get => arCsePersonAddress ??= new();
      set => arCsePersonAddress = value;
    }

    /// <summary>
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePersonAccount Supported
    {
      get => supported ??= new();
      set => supported = value;
    }

    /// <summary>
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
    }

    /// <summary>
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of DisplacedPerson.
    /// </summary>
    [JsonPropertyName("displacedPerson")]
    public DisplacedPerson DisplacedPerson
    {
      get => displacedPerson ??= new();
      set => displacedPerson = value;
    }

    private CodeValue codeValue;
    private Code code;
    private CsePerson arCsePerson;
    private CaseRole applicantRecipient;
    private CsePersonAddress arCsePersonAddress;
    private CsePersonAccount supported;
    private ObligationTransaction obligationTransaction;
    private Collection collection;
    private CaseRole caseRole;
    private Case1 case1;
    private CsePerson csePerson;
    private DisplacedPerson displacedPerson;
  }
#endregion
}
