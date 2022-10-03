// Program: FN_B615_CASH_FLOW_REPORT, ID: 373028127, model: 746.
// Short name: SWEF615B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B615_CASH_FLOW_REPORT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB615CashFlowReport: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B615_CASH_FLOW_REPORT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB615CashFlowReport(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB615CashFlowReport.
  /// </summary>
  public FnB615CashFlowReport(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // Date    Developer    Request #   Description
    // --------------------------------------------------------------------
    // 12/15/00  Maureen Brown              Initial Development
    // --------------------------------------------------------------------
    // *****************************************************************
    // resp: Finance
    // *****************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    // *****************************************************************
    // Housekeeping
    // *****************************************************************
    // : This field sets the maximum number of reports that can be produced in 1
    // run.
    local.MaxReports.Count = 12;
    local.Current.Date = Now().Date;
    local.Current.Timestamp = Now();
    local.Max.Date = new DateTime(2099, 12, 31);
    local.EnvCd.Text10 = "BATCH";
    local.EabFileHandling.Action = "OPEN";
    UseCabErrorReport1();
    local.EabFileHandling.Action = "WRITE";

    // *****************************************************************
    // Get the SYSIN Parm Values
    // *****************************************************************
    UseFnExtGetParmsThruJclSysin();

    if (local.SysinForSweeb800.NumericReturnCode != 0)
    {
      ExitState = "FN0000_SYSIN_PARM_ERROR_A";

      return;
    }

    if (IsEmpty(local.Sysin.ParameterList))
    {
      ExitState = "FN0000_BLANK_SYSIN_PARM_LIST_AB";

      return;
    }

    local.Job.Name = Substring(local.Sysin.ParameterList, 1, 8);

    if (!ReadJob())
    {
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail = local.Sysin.ParameterList ?? Spaces(132);
      UseCabErrorReport2();
      ExitState = "CO0000_JOB_NF_AB";

      return;
    }

    // : We are using the online print facility, but do not have this report 
    // available on screen yet.  Normally the online program creates the job run
    // entity, using parameters entered by the user or retrieved from a screen.
    // Until this is available online, we create a new job_run in this
    // program.
    local.JobRun.StartTimestamp = Now();

    try
    {
      CreateJobRun();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "CO0000_JOB_RUN_AE_AB";

          return;
        case ErrorCode.PermittedValueViolation:
          ExitState = "CO0000_JOB_RUN_PV_AB";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    // : Perform a DB2 Commit to Free Up the JOB_RUN row.
    UseExtToDoACommit();

    // : Create the sysin for program SWEEB800.
    local.SysinForSweeb800.TextLine80 = local.Job.Name + " " + NumberToString
      (entities.JobRun.SystemGenId, 7, 9);

    // : The next eab creates a sysin with jobname and timestamp to pass to 
    // SWEEB800,
    //   which generates the report.
    UseEabWriteSysoutFile();

    if (local.SysinForSweeb800.NumericReturnCode != 0)
    {
      ExitState = "FN0000_SYSIN_PARM_ERROR_A";

      return;
    }

    // *****************************************************************
    // Extract report Criteria from Parameter Information
    // *****************************************************************
    // : All parms are optional for this job.
    // : Check for a date range - if not there, default the report to quarterly.
    // Input parameters are:
    //    - Report period (ie quarterly, monthly, yearly, etc)
    //    - From date
    //    - To date
    // If the date range encompasses more report periods than 1, a report will 
    // be produced for each report period within the date range.
    local.FromTextDate.Text10 = Substring(local.Sysin.ParameterList, 21, 10);

    if (IsEmpty(local.FromTextDate.Text10))
    {
      local.ReportFrom.Date = Now().Date;
    }
    else
    {
      local.ReportFrom.Date = StringToDate(local.FromTextDate.Text10);
    }

    local.ToTextDate.Text10 = Substring(local.Sysin.ParameterList, 32, 10);

    if (IsEmpty(local.ToTextDate.Text10))
    {
    }
    else
    {
      local.ReportTo.Date = StringToDate(local.ToTextDate.Text10);
    }

    local.ReportPeriod.Text10 = Substring(local.Sysin.ParameterList, 10, 10);

    if (IsEmpty(local.ReportPeriod.Text10))
    {
      if (IsEmpty(local.FromTextDate.Text10) || IsEmpty
        (local.ToTextDate.Text10))
      {
        local.ReportPeriod.Text10 = "QUARTER";
      }
    }

    if (!IsEmpty(local.ReportPeriod.Text10))
    {
      // : The 'to date' needs to be saved because the cab about to be called 
      // calculates a 'to date' based on the report period.  If the 'to date'
      // entered in the parms is greater than this calculated date, we will
      // create multiple cash flow reports using the requested period, until
      // this final 'to date' is reached.
      local.SaveToDate.Date = local.ReportTo.Date;
    }

    // : This cab will calculate the from and to date of the report based on 
    // what
    //   report period was entered.  It will also set timestamp values for 
    // these, so
    //   we call this cab even if a report period was not entered.
    UseFnCabDetermineRptDateRange();

    if (Equal(local.SaveToDate.Date, local.NullDateWorkArea.Date))
    {
      local.SaveToDate.Date = local.ReportTo.Date;
    }

    // *****************************************************************
    // Mainline Process
    // *****************************************************************
    // : Format Header Lines and create Report Data for them.
    local.Header.Index = 0;
    local.Header.CheckSize();

    local.Header.Update.Header1.FirstPageOnlyInd = "N";
    local.Header.Update.Header1.LineControl = "";
    local.Header.Update.Header1.LineText = "SRRUN275" + Substring
      (local.NullReportData.LineText, ReportData.LineText_MaxLength, 1, 58) + "STATE OF KANSAS" +
      Substring
      (local.NullReportData.LineText, ReportData.LineText_MaxLength, 1, 41) + "PAGE:     1";
      
    local.TextMm.Text2 =
      NumberToString(Month(entities.JobRun.StartTimestamp), 14, 2);
    local.TextDd.Text2 =
      NumberToString(Day(entities.JobRun.StartTimestamp), 14, 2);
    local.TextYyyy.Text4 =
      NumberToString(Year(entities.JobRun.StartTimestamp), 12, 4);
    local.RunDate.Text10 = local.TextMm.Text2 + "/" + local.TextDd.Text2 + "/"
      + local.TextYyyy.Text4;

    local.Header.Index = 1;
    local.Header.CheckSize();

    local.Header.Update.Header1.FirstPageOnlyInd = "N";
    local.Header.Update.Header1.LineControl = "";
    local.Header.Update.Header1.LineText = "RUN DATE: " + local
      .RunDate.Text10 + Substring
      (local.NullReportData.LineText, ReportData.LineText_MaxLength, 1, 37) + "DEPARTMENT OF CHILDREN AND FAMILIES";
      

    local.Header.Index = 2;
    local.Header.CheckSize();

    local.Header.Update.Header1.FirstPageOnlyInd = "N";
    local.Header.Update.Header1.LineControl = "";
    local.Header.Update.Header1.LineText =
      Substring(local.NullReportData.LineText, ReportData.LineText_MaxLength, 1,
      62) + "KAECSES - CHILD SUPPORT";

    local.Header.Index = 3;
    local.Header.CheckSize();

    local.Header.Update.Header1.FirstPageOnlyInd = "N";
    local.Header.Update.Header1.LineControl = "";
    local.Header.Update.Header1.LineText =
      Substring(local.NullReportData.LineText, ReportData.LineText_MaxLength, 1,
      64) + "CASH FLOW STATEMENT";

    for(local.Header.Index = 0; local.Header.Index < local.Header.Count; ++
      local.Header.Index)
    {
      if (!local.Header.CheckSize())
      {
        break;
      }

      try
      {
        CreateReportData2();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "CO0000_REPORT_DATA_AE_AB";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "CO0000_REPORT_DATA_PV_AB";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    local.Header.CheckIndex();
    local.NumberOfReports.Count = 0;

    do
    {
      // : Keep track of the number of reports generated, so that this job does 
      // not
      //   end up taking forever to run.  Max is set to 12.
      if (local.NumberOfReports.Count >= local.MaxReports.Count)
      {
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail = "Report max exceeded.  " + NumberToString
          (local.MaxReports.Count, 15) + " were produced.";
        UseCabErrorReport2();

        return;
      }

      UseFnB615CashFlowCab();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        UseEabExtractExitStateMessage();
        local.NeededToWrite.RptDetail = local.ExitStateWorkArea.Message;
        UseCabErrorReport2();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      // *****************************************************************
      // Format Report and Add to the Report Repository
      // *****************************************************************
      if (local.Group.IsEmpty)
      {
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail = "No Data Retrieved for the report.";
        UseCabErrorReport2();
      }
      else
      {
        // : The following is to output 2 blank lines at the bottom of the 
        // report so that
        //   the page breaks occur properly.
        if (local.NumberOfReports.Count == 2 || local.NumberOfReports.Count == 5
          || local.NumberOfReports.Count == 8)
        {
          local.Group.Index = local.Group.Count;
          local.Group.CheckSize();

          local.Group.Update.ReportData.LineText = "";

          local.Group.Index = local.Group.Count;
          local.Group.CheckSize();

          local.Group.Update.ReportData.LineText = "";
        }

        for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
          local.Group.Index)
        {
          if (!local.Group.CheckSize())
          {
            break;
          }

          // : Double space for certain lines on the report (use subscript to 
          // identify these lines).
          if (local.Group.Index == 1 || local.Group.Index == 10 || local
            .Group.Index == 13)
          {
            local.ReportData.LineControl = "01";
          }
          else
          {
            // : Single space.
            local.ReportData.LineControl = "";
          }

          // : Leave 2 spaces between each statement (there are 3 statements per
          // page).
          if (local.Group.Index == 0)
          {
            local.ReportData.LineControl = "02";
          }

          // : sequence number needs to be unique and ascending, but it does not
          // matter if it increments by more than 1, and this will occur on
          // this report.
          try
          {
            CreateReportData1();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "CO0000_REPORT_DATA_AE_AB";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "CO0000_REPORT_DATA_PV_AB";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }

        local.Group.CheckIndex();
      }

      // : Keep track of the number of reports generated, so that this job does 
      // not
      //   end up taking forever to run.  Max is set to 12.
      ++local.NumberOfReports.Count;

      switch(TrimEnd(local.ReportPeriod.Text10))
      {
        case "QUARTER":
          local.ReportFrom.Date = AddMonths(local.ReportFrom.Date, 3);
          local.ReportFrom.Timestamp = AddMonths(local.ReportFrom.Timestamp, 3);
          local.ReportTo.Date =
            AddDays(AddMonths(local.ReportFrom.Date, 3), -1);
          local.ReportTo.Timestamp =
            AddDays(AddMonths(
              AddSeconds(
              AddMinutes(AddHours(local.ReportFrom.Timestamp, 23), 59), 59), 3),
            -1);

          break;
        case "MONTH":
          local.ReportFrom.Date = AddMonths(local.ReportFrom.Date, 1);
          local.ReportFrom.Timestamp = AddMonths(local.ReportFrom.Timestamp, 1);
          local.ReportTo.Date =
            AddDays(AddMonths(local.ReportFrom.Date, 1), -1);
          local.ReportTo.Timestamp =
            AddSeconds(AddMinutes(
              AddHours(AddDays(AddMonths(local.ReportFrom.Timestamp, 1), -1), 23)
            , 59), 59);

          break;
        case "YEAR":
          local.ReportFrom.Date = AddYears(local.ReportFrom.Date, 1);
          local.ReportFrom.Timestamp = AddYears(local.ReportFrom.Timestamp, 1);
          local.ReportTo.Date = AddYears(local.ReportTo.Date, 1);
          local.ReportTo.Timestamp = AddYears(local.ReportTo.Timestamp, 1);

          break;
        default:
          goto AfterCycle;
      }
    }
    while(!Lt(local.SaveToDate.Date, local.ReportTo.Date));

AfterCycle:

    // *****************************************************************
    // Update the Status of the Report in JOB_RUN.
    // *****************************************************************
    if (Equal(entities.JobRun.OutputType, "ONLINE"))
    {
      try
      {
        UpdateJobRun1();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "CO0000_JOB_RUN_NU_AB";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "CO0000_JOB_RUN_PV_AB";

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
      try
      {
        UpdateJobRun2();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "CO0000_JOB_RUN_NU_AB";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "CO0000_JOB_RUN_PV_AB";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
  }

  private static void MoveBatchToGroup(FnB615CashFlowCab.Export.
    BatchGroup source, Local.GroupGroup target)
  {
    target.ReportData.LineText = source.Batch1.LineText;
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseEabWriteSysoutFile()
  {
    var useImport = new EabWriteSysoutFile.Import();
    var useExport = new EabWriteSysoutFile.Export();

    useImport.SysoutInfo.TextLine80 = local.SysinForSweeb800.TextLine80;
    useExport.ReturnCode.NumericReturnCode =
      local.SysinForSweeb800.NumericReturnCode;

    Call(EabWriteSysoutFile.Execute, useImport, useExport);

    local.SysinForSweeb800.NumericReturnCode =
      useExport.ReturnCode.NumericReturnCode;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode =
      local.SysinForSweeb800.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.SysinForSweeb800.NumericReturnCode =
      useExport.External.NumericReturnCode;
  }

  private void UseFnB615CashFlowCab()
  {
    var useImport = new FnB615CashFlowCab.Import();
    var useExport = new FnB615CashFlowCab.Export();

    useImport.EnvCd.Text10 = local.EnvCd.Text10;
    useImport.ReportPeriod.Text10 = local.ReportPeriod.Text10;
    useImport.SearchFrom.Date = local.ReportFrom.Date;
    useImport.SearchTo.Date = local.ReportTo.Date;

    Call(FnB615CashFlowCab.Execute, useImport, useExport);

    useExport.Batch.CopyTo(local.Group, MoveBatchToGroup);
  }

  private void UseFnCabDetermineRptDateRange()
  {
    var useImport = new FnCabDetermineRptDateRange.Import();
    var useExport = new FnCabDetermineRptDateRange.Export();

    useImport.From.Date = local.ReportFrom.Date;
    useImport.To.Date = local.ReportTo.Date;
    useImport.ReportPeriod.Text10 = local.ReportPeriod.Text10;

    Call(FnCabDetermineRptDateRange.Execute, useImport, useExport);

    MoveDateWorkArea(useExport.From, local.ReportFrom);
    MoveDateWorkArea(useExport.To, local.ReportTo);
  }

  private void UseFnExtGetParmsThruJclSysin()
  {
    var useImport = new FnExtGetParmsThruJclSysin.Import();
    var useExport = new FnExtGetParmsThruJclSysin.Export();

    useExport.ProgramProcessingInfo.ParameterList = local.Sysin.ParameterList;
    useExport.External.NumericReturnCode =
      local.SysinForSweeb800.NumericReturnCode;

    Call(FnExtGetParmsThruJclSysin.Execute, useImport, useExport);

    local.Sysin.ParameterList = useExport.ProgramProcessingInfo.ParameterList;
    local.SysinForSweeb800.NumericReturnCode =
      useExport.External.NumericReturnCode;
  }

  private int UseGenerate9DigitRandomNumber()
  {
    var useImport = new Generate9DigitRandomNumber.Import();
    var useExport = new Generate9DigitRandomNumber.Export();

    Call(Generate9DigitRandomNumber.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute9DigitRandomNumber;
  }

  private void CreateJobRun()
  {
    var startTimestamp = local.Current.Timestamp;
    var status = "PROCESSING";
    var outputType = "PRINTER";
    var parmInfo = Substring(local.Sysin.ParameterList, 10, 50);
    var jobName = entities.ExistingJob.Name;
    var systemGenId = UseGenerate9DigitRandomNumber();

    entities.JobRun.Populated = false;
    Update("CreateJobRun",
      (db, command) =>
      {
        db.SetDateTime(command, "startTimestamp", startTimestamp);
        db.SetNullableDateTime(command, "endTimestamp", null);
        db.SetNullableString(command, "zdelUserId", "");
        db.SetNullableString(command, "zdelPersonNumber", "");
        db.SetNullableInt32(command, "zdelLegActionId", 0);
        db.SetString(command, "status", status);
        db.SetString(command, "outputType", outputType);
        db.SetNullableString(command, "errorMsg", "");
        db.SetNullableString(command, "emailAddress", "");
        db.SetNullableString(command, "parmInfo", parmInfo);
        db.SetString(command, "jobName", jobName);
        db.SetInt32(command, "systemGenId", systemGenId);
      });

    entities.JobRun.StartTimestamp = startTimestamp;
    entities.JobRun.EndTimestamp = null;
    entities.JobRun.Status = status;
    entities.JobRun.OutputType = outputType;
    entities.JobRun.ErrorMsg = "";
    entities.JobRun.ParmInfo = parmInfo;
    entities.JobRun.JobName = jobName;
    entities.JobRun.SystemGenId = systemGenId;
    entities.JobRun.Populated = true;
  }

  private void CreateReportData1()
  {
    System.Diagnostics.Debug.Assert(entities.JobRun.Populated);

    var type1 = "D";
    var sequenceNumber = Local.GroupGroup.Capacity * local
      .NumberOfReports.Count + local.Group.Index + 1;
    var firstPageOnlyInd = "N";
    var lineControl = local.ReportData.LineControl;
    var lineText = local.Group.Item.ReportData.LineText;
    var jobName = entities.JobRun.JobName;
    var jruSystemGenId = entities.JobRun.SystemGenId;

    entities.New1.Populated = false;
    Update("CreateReportData1",
      (db, command) =>
      {
        db.SetString(command, "type", type1);
        db.SetInt32(command, "sequenceNumber", sequenceNumber);
        db.SetNullableString(command, "firstPageOnlyIn", firstPageOnlyInd);
        db.SetString(command, "lineControl", lineControl);
        db.SetString(command, "lineText", lineText);
        db.SetString(command, "jobName", jobName);
        db.SetInt32(command, "jruSystemGenId", jruSystemGenId);
      });

    entities.New1.Type1 = type1;
    entities.New1.SequenceNumber = sequenceNumber;
    entities.New1.FirstPageOnlyInd = firstPageOnlyInd;
    entities.New1.LineControl = lineControl;
    entities.New1.LineText = lineText;
    entities.New1.JobName = jobName;
    entities.New1.JruSystemGenId = jruSystemGenId;
    entities.New1.Populated = true;
  }

  private void CreateReportData2()
  {
    System.Diagnostics.Debug.Assert(entities.JobRun.Populated);

    var type1 = "H";
    var sequenceNumber = local.Header.Index + 1;
    var firstPageOnlyInd = local.Header.Item.Header1.FirstPageOnlyInd ?? "";
    var lineControl = local.Header.Item.Header1.LineControl;
    var lineText = local.Header.Item.Header1.LineText;
    var jobName = entities.JobRun.JobName;
    var jruSystemGenId = entities.JobRun.SystemGenId;

    entities.New1.Populated = false;
    Update("CreateReportData2",
      (db, command) =>
      {
        db.SetString(command, "type", type1);
        db.SetInt32(command, "sequenceNumber", sequenceNumber);
        db.SetNullableString(command, "firstPageOnlyIn", firstPageOnlyInd);
        db.SetString(command, "lineControl", lineControl);
        db.SetString(command, "lineText", lineText);
        db.SetString(command, "jobName", jobName);
        db.SetInt32(command, "jruSystemGenId", jruSystemGenId);
      });

    entities.New1.Type1 = type1;
    entities.New1.SequenceNumber = sequenceNumber;
    entities.New1.FirstPageOnlyInd = firstPageOnlyInd;
    entities.New1.LineControl = lineControl;
    entities.New1.LineText = lineText;
    entities.New1.JobName = jobName;
    entities.New1.JruSystemGenId = jruSystemGenId;
    entities.New1.Populated = true;
  }

  private bool ReadJob()
  {
    entities.ExistingJob.Populated = false;

    return Read("ReadJob",
      (db, command) =>
      {
        db.SetString(command, "name", local.Job.Name);
      },
      (db, reader) =>
      {
        entities.ExistingJob.Name = db.GetString(reader, 0);
        entities.ExistingJob.Populated = true;
      });
  }

  private void UpdateJobRun1()
  {
    System.Diagnostics.Debug.Assert(entities.JobRun.Populated);

    var endTimestamp = Now();
    var status = "COMPLETE";

    entities.JobRun.Populated = false;
    Update("UpdateJobRun1",
      (db, command) =>
      {
        db.SetNullableDateTime(command, "endTimestamp", endTimestamp);
        db.SetString(command, "status", status);
        db.SetString(command, "jobName", entities.JobRun.JobName);
        db.SetInt32(command, "systemGenId", entities.JobRun.SystemGenId);
      });

    entities.JobRun.EndTimestamp = endTimestamp;
    entities.JobRun.Status = status;
    entities.JobRun.Populated = true;
  }

  private void UpdateJobRun2()
  {
    System.Diagnostics.Debug.Assert(entities.JobRun.Populated);

    var status = "WAIT";

    entities.JobRun.Populated = false;
    Update("UpdateJobRun2",
      (db, command) =>
      {
        db.SetString(command, "status", status);
        db.SetString(command, "jobName", entities.JobRun.JobName);
        db.SetInt32(command, "systemGenId", entities.JobRun.SystemGenId);
      });

    entities.JobRun.Status = status;
    entities.JobRun.Populated = true;
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
    /// <summary>A HeaderGroup group.</summary>
    [Serializable]
    public class HeaderGroup
    {
      /// <summary>
      /// A value of Header1.
      /// </summary>
      [JsonPropertyName("header1")]
      public ReportData Header1
      {
        get => header1 ??= new();
        set => header1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private ReportData header1;
    }

    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of ReportData.
      /// </summary>
      [JsonPropertyName("reportData")]
      public ReportData ReportData
      {
        get => reportData ??= new();
        set => reportData = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private ReportData reportData;
    }

    /// <summary>
    /// A value of EnvCd.
    /// </summary>
    [JsonPropertyName("envCd")]
    public WorkArea EnvCd
    {
      get => envCd ??= new();
      set => envCd = value;
    }

    /// <summary>
    /// A value of BatchTimestampWorkArea.
    /// </summary>
    [JsonPropertyName("batchTimestampWorkArea")]
    public BatchTimestampWorkArea BatchTimestampWorkArea
    {
      get => batchTimestampWorkArea ??= new();
      set => batchTimestampWorkArea = value;
    }

    /// <summary>
    /// A value of Sysout.
    /// </summary>
    [JsonPropertyName("sysout")]
    public ReportParms Sysout
    {
      get => sysout ??= new();
      set => sysout = value;
    }

    /// <summary>
    /// A value of MaxReports.
    /// </summary>
    [JsonPropertyName("maxReports")]
    public Common MaxReports
    {
      get => maxReports ??= new();
      set => maxReports = value;
    }

    /// <summary>
    /// A value of NumberOfReports.
    /// </summary>
    [JsonPropertyName("numberOfReports")]
    public Common NumberOfReports
    {
      get => numberOfReports ??= new();
      set => numberOfReports = value;
    }

    /// <summary>
    /// A value of Sysin.
    /// </summary>
    [JsonPropertyName("sysin")]
    public ProgramProcessingInfo Sysin
    {
      get => sysin ??= new();
      set => sysin = value;
    }

    /// <summary>
    /// A value of Job.
    /// </summary>
    [JsonPropertyName("job")]
    public Job Job
    {
      get => job ??= new();
      set => job = value;
    }

    /// <summary>
    /// A value of JobRun.
    /// </summary>
    [JsonPropertyName("jobRun")]
    public JobRun JobRun
    {
      get => jobRun ??= new();
      set => jobRun = value;
    }

    /// <summary>
    /// A value of ReportData.
    /// </summary>
    [JsonPropertyName("reportData")]
    public ReportData ReportData
    {
      get => reportData ??= new();
      set => reportData = value;
    }

    /// <summary>
    /// A value of NullReportData.
    /// </summary>
    [JsonPropertyName("nullReportData")]
    public ReportData NullReportData
    {
      get => nullReportData ??= new();
      set => nullReportData = value;
    }

    /// <summary>
    /// A value of LoopControl.
    /// </summary>
    [JsonPropertyName("loopControl")]
    public DateWorkArea LoopControl
    {
      get => loopControl ??= new();
      set => loopControl = value;
    }

    /// <summary>
    /// A value of ReportFrom.
    /// </summary>
    [JsonPropertyName("reportFrom")]
    public DateWorkArea ReportFrom
    {
      get => reportFrom ??= new();
      set => reportFrom = value;
    }

    /// <summary>
    /// A value of ReportTo.
    /// </summary>
    [JsonPropertyName("reportTo")]
    public DateWorkArea ReportTo
    {
      get => reportTo ??= new();
      set => reportTo = value;
    }

    /// <summary>
    /// A value of ToTextDate.
    /// </summary>
    [JsonPropertyName("toTextDate")]
    public WorkArea ToTextDate
    {
      get => toTextDate ??= new();
      set => toTextDate = value;
    }

    /// <summary>
    /// A value of FromTextDate.
    /// </summary>
    [JsonPropertyName("fromTextDate")]
    public WorkArea FromTextDate
    {
      get => fromTextDate ??= new();
      set => fromTextDate = value;
    }

    /// <summary>
    /// A value of SaveToDate.
    /// </summary>
    [JsonPropertyName("saveToDate")]
    public DateWorkArea SaveToDate
    {
      get => saveToDate ??= new();
      set => saveToDate = value;
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
    /// A value of NullDateWorkArea.
    /// </summary>
    [JsonPropertyName("nullDateWorkArea")]
    public DateWorkArea NullDateWorkArea
    {
      get => nullDateWorkArea ??= new();
      set => nullDateWorkArea = value;
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of SysinForSweeb800.
    /// </summary>
    [JsonPropertyName("sysinForSweeb800")]
    public External SysinForSweeb800
    {
      get => sysinForSweeb800 ??= new();
      set => sysinForSweeb800 = value;
    }

    /// <summary>
    /// A value of ReportPeriod.
    /// </summary>
    [JsonPropertyName("reportPeriod")]
    public WorkArea ReportPeriod
    {
      get => reportPeriod ??= new();
      set => reportPeriod = value;
    }

    /// <summary>
    /// A value of RunDate.
    /// </summary>
    [JsonPropertyName("runDate")]
    public WorkArea RunDate
    {
      get => runDate ??= new();
      set => runDate = value;
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
    }

    /// <summary>
    /// A value of SysoutForSweeb800.
    /// </summary>
    [JsonPropertyName("sysoutForSweeb800")]
    public EabReportSend SysoutForSweeb800
    {
      get => sysoutForSweeb800 ??= new();
      set => sysoutForSweeb800 = value;
    }

    /// <summary>
    /// Gets a value of Header.
    /// </summary>
    [JsonIgnore]
    public Array<HeaderGroup> Header => header ??= new(HeaderGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Header for json serialization.
    /// </summary>
    [JsonPropertyName("header")]
    [Computed]
    public IList<HeaderGroup> Header_Json
    {
      get => header;
      set => Header.Assign(value);
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

    private WorkArea envCd;
    private BatchTimestampWorkArea batchTimestampWorkArea;
    private ReportParms sysout;
    private Common maxReports;
    private Common numberOfReports;
    private ProgramProcessingInfo sysin;
    private Job job;
    private JobRun jobRun;
    private ReportData reportData;
    private ReportData nullReportData;
    private DateWorkArea loopControl;
    private DateWorkArea reportFrom;
    private DateWorkArea reportTo;
    private WorkArea toTextDate;
    private WorkArea fromTextDate;
    private DateWorkArea saveToDate;
    private DateWorkArea current;
    private DateWorkArea nullDateWorkArea;
    private TextWorkArea textMm;
    private TextWorkArea textDd;
    private TextWorkArea textYyyy;
    private DateWorkArea max;
    private External sysinForSweeb800;
    private WorkArea reportPeriod;
    private WorkArea runDate;
    private ExitStateWorkArea exitStateWorkArea;
    private EabFileHandling eabFileHandling;
    private EabReportSend neededToWrite;
    private EabReportSend sysoutForSweeb800;
    private Array<HeaderGroup> header;
    private Array<GroupGroup> group;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingReportData.
    /// </summary>
    [JsonPropertyName("existingReportData")]
    public ReportData ExistingReportData
    {
      get => existingReportData ??= new();
      set => existingReportData = value;
    }

    /// <summary>
    /// A value of JobRun.
    /// </summary>
    [JsonPropertyName("jobRun")]
    public JobRun JobRun
    {
      get => jobRun ??= new();
      set => jobRun = value;
    }

    /// <summary>
    /// A value of ExistingJob.
    /// </summary>
    [JsonPropertyName("existingJob")]
    public Job ExistingJob
    {
      get => existingJob ??= new();
      set => existingJob = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public ReportData New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    private ReportData existingReportData;
    private JobRun jobRun;
    private Job existingJob;
    private ReportData new1;
  }
#endregion
}
