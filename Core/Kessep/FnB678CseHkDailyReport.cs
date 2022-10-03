// Program: FN_B678_CSE_HK_DAILY_REPORT, ID: 371255215, model: 746.
// Short name: SWEF678B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B678_CSE_HK_DAILY_REPORT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB678CseHkDailyReport: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B678_CSE_HK_DAILY_REPORT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB678CseHkDailyReport(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB678CseHkDailyReport.
  /// </summary>
  public FnB678CseHkDailyReport(IContext context, Import import, Export export):
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
    // --  This program produces a daily report of CSE activity for clients 
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
    for(local.Counter.Count = 1; local.Counter.Count <= 3; ++
      local.Counter.Count)
    {
      switch(local.Counter.Count)
      {
        case 1:
          local.EabReportSend.RptDetail = "";

          break;
        case 2:
          local.EabReportSend.RptDetail = "Start Child Counts - Time: " + NumberToString
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
    // -- Determine number of children impacted by Hurricane Katrina.  Grouped 
    // by county of the AR address.
    // -------------------------------------------------------------------------------------------------------------------------
    foreach(var item in ReadCsePerson2())
    {
      local.CountiesReported.RptDetail = "";

      foreach(var item1 in ReadCase3())
      {
        local.ArAddress.Count = 0;

        if (ReadCsePersonAddress())
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
            while(!Equal(local.Report.Item.GlocalReportCsePersonAddress.County,
              entities.ArCsePersonAddress.County));

            local.Report.Update.GlocalReportChild.Count =
              local.Report.Item.GlocalReportChild.Count + 1;
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

      foreach(var item1 in ReadCase2())
      {
        local.ArAddress.Count = 0;

        if (ReadCsePersonAddress())
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
    foreach(var item in ReadCase1())
    {
      local.CountiesReported.RptDetail = "";
      local.ArAddress.Count = 0;

      if (ReadCsePersonAddress())
      {
        ++local.ArAddress.Count;

        if (Find(String(
          local.CountiesReported.RptDetail, EabReportSend.RptDetail_MaxLength),
          entities.ArCsePersonAddress.County + "/") != 0)
        {
          local.AlreadyReported.Flag = "*";
        }
        else
        {
          local.AlreadyReported.Flag = "";
        }

        local.EabReportSend.RptDetail = "      " + "          " + "  Case " + entities
          .Case1.Number + "  County " + entities.ArCsePersonAddress.County + local
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

        if (Find(String(
          local.CountiesReported.RptDetail, EabReportSend.RptDetail_MaxLength),
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

        local.EabReportSend.RptDetail = "      " + "          " + "  Case " + entities
          .Case1.Number + "  County " + "  " + local.AlreadyReported.Flag;
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
          // --  We already reported the case in this county, don't double count
          // them in the same county.
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
    foreach(var item in ReadSupportedCsePersonDisplacedPerson())
    {
      // -- Set start and end timestamps using the displaced person effective 
      // and end dates....
      local.Start.Timestamp =
        Add(local.NullDateWorkArea.Timestamp,
        Year(entities.DisplacedPerson.EffectiveDate),
        Month(entities.DisplacedPerson.EffectiveDate),
        Day(entities.DisplacedPerson.EffectiveDate));
      local.End.Timestamp =
        Add(local.NullDateWorkArea.Timestamp,
        Year(entities.DisplacedPerson.EndDate),
        Month(entities.DisplacedPerson.EndDate),
        Day(entities.DisplacedPerson.EndDate));
      local.End.Timestamp =
        AddMicroseconds(AddDays(local.End.Timestamp, 1), -1);
      ReadCollection();

      if (local.Collection.TotalCurrency > 0)
      {
        local.CountiesReported.RptDetail = "";

        foreach(var item1 in ReadCase4())
        {
          local.ArAddress.Count = 0;

          if (ReadCsePersonAddress())
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

            local.EabReportSend.RptDetail = "Supp  " + entities
              .CsePerson.Number + "  Case " + entities.Case1.Number + "  County " +
              entities.ArCsePersonAddress.County + local
              .AlreadyReported.Flag + "  " + NumberToString
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

              local.Report.Update.GlocalReportCollections.TotalCurrency =
                local.Report.Item.GlocalReportCollections.TotalCurrency + local
                .Collection.TotalCurrency;
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

            local.EabReportSend.RptDetail = "Supp  " + entities
              .CsePerson.Number + "  Case " + entities.Case1.Number + "  County " +
              "  " + local.AlreadyReported.Flag + "  " + NumberToString
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
              // --  We already reported the person in this county, don't double
              // count them in the same county.
              goto Test4;
            }

            // -- Report the person under the Unknown county.
            local.Report.Index = 0;
            local.Report.CheckSize();

            local.Report.Update.GlocalReportCollections.TotalCurrency =
              local.Report.Item.GlocalReportCollections.TotalCurrency + local
              .Collection.TotalCurrency;
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
    for(local.Counter.Count = 1; local.Counter.Count <= 3; ++
      local.Counter.Count)
    {
      switch(local.Counter.Count)
      {
        case 1:
          local.EabReportSend.RptDetail = "";

          break;
        case 2:
          local.EabReportSend.RptDetail =
            "County                     Cases    Children    Adults    Collections";
            

          break;
        case 3:
          local.EabReportSend.RptDetail =
            "-----------------------    ------   --------    ------    -----------";
            

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
      local.EabReportSend.RptDetail =
        local.Report.Item.GlocalReportCsePersonAddress.County ?? Spaces(132);
      local.EabReportSend.RptDetail =
        Substring(local.EabReportSend.RptDetail,
        EabReportSend.RptDetail_MaxLength, 1, 3) + Substring
        (local.Report.Item.GlocalReportCodeValue.Description, 1, 20);

      for(local.Counter.Count = 1; local.Counter.Count <= 4; ++
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
              (int)(local.Report.Item.GlocalReportCollections.TotalCurrency * 100
              );

            break;
          default:
            break;
        }

        if (local.Common.Count == 0)
        {
          if (local.Counter.Count == 4)
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

          if (local.Counter.Count == 4)
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
      "-----------------------    ------   --------    ------    -----------";
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

    for(local.Counter.Count = 1; local.Counter.Count <= 4; ++
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
            (int)(local.StatewideCollections.TotalCurrency * 100);

          break;
        default:
          break;
      }

      if (local.Common.Count == 0)
      {
        if (local.Counter.Count == 4)
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

        if (local.Counter.Count == 4)
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

  private IEnumerable<bool> ReadCase1()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase1",
      null,
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCase2()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCase3()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase3",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCase4()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase4",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
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

  private bool ReadCollection()
  {
    System.Diagnostics.Debug.Assert(entities.Supported.Populated);

    return Read("ReadCollection",
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
        local.Collection.TotalCurrency = db.GetDecimal(reader, 0);
      });
  }

  private IEnumerable<bool> ReadCsePerson1()
  {
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePerson1",
      null,
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
      null,
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;

        return true;
      });
  }

  private bool ReadCsePersonAddress()
  {
    entities.ArCsePersonAddress.Populated = false;

    return Read("ReadCsePersonAddress",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "endDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.Case1.Number);
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
        CheckValid<CsePersonAddress>("LocationType",
          entities.ArCsePersonAddress.LocationType);
      });
  }

  private IEnumerable<bool> ReadSupportedCsePersonDisplacedPerson()
  {
    entities.Supported.Populated = false;
    entities.CsePerson.Populated = false;
    entities.DisplacedPerson.Populated = false;

    return ReadEach("ReadSupportedCsePersonDisplacedPerson",
      null,
      (db, reader) =>
      {
        entities.Supported.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.Supported.Type1 = db.GetString(reader, 1);
        entities.DisplacedPerson.Number = db.GetString(reader, 2);
        entities.DisplacedPerson.EffectiveDate = db.GetDate(reader, 3);
        entities.DisplacedPerson.EndDate = db.GetNullableDate(reader, 4);
        entities.Supported.Populated = true;
        entities.CsePerson.Populated = true;
        entities.DisplacedPerson.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.Supported.Type1);

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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 200;

      private CodeValue glocalReportCodeValue;
      private CsePersonAddress glocalReportCsePersonAddress;
      private Common glocalReportChild;
      private Common glocalReportAdult;
      private Common glocalReportCase;
      private Common glocalReportCollections;
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
