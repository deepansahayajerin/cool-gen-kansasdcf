// Program: FN_OCSE157_BATCH_INITIALIZATION, ID: 371092705, model: 746.
// Short name: SWE02915
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_OCSE157_BATCH_INITIALIZATION.
/// </summary>
[Serializable]
public partial class FnOcse157BatchInitialization: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_OCSE157_BATCH_INITIALIZATION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnOcse157BatchInitialization(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnOcse157BatchInitialization.
  /// </summary>
  public FnOcse157BatchInitialization(IContext context, Import import,
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
    // -------------------------------------------------------------------------------------------------------------
    //                                     
    // C H A N G E    L O G
    // -------------------------------------------------------------------------------------------------------------
    // Date      Developer     Request #	Description
    // --------  ----------    ----------	
    // ---------------------------------------------------------------------
    // 05/??/01  KDoshi	WR10367		Initial Development
    // 07/29/01				Add display statements.
    // 					Create verification records.
    // 08/01/01				Set FY end timestamp to 7am Oct 1.
    // 					Set CY end timestamp to 7am Jan 1.
    // 08/04/01				Set FY start timestamp to 7am Oct 1.
    // 					Set CY start timestamp to 7am Jan 1.
    // 					Add more displays.
    // 08/04/01  				Set get case count per pepr flag.
    // 09/19/01				Set case universe flag.
    // 03/09/06  GVandy	WR00230751	Federally mandated changes.
    // 					1) Add parm for previous year run number (line 5a & 8a)
    // 					2) Add parm for # children BOW in KS (line 8b)
    // 01/19/07  GVandy	PR297812	Previous year run number is no longer entered 
    // on the PPI record.
    // 					The previous year run number will now be calculated automatically
    // 					in Line 5a and 8a processing.
    // 02/04/20  GVandy	CQ66220		Beginning in FY 2022, include only amounts that
    // are
    // 					both distributed and disbursed.  Export a cutoff FY
    // 					which defaults to 2022 but can be overridden with
    // 					a code table value for testing.
    // -------------------------------------------------------------------------------------------------------------
    // ---------------------------------------------------
    // Parameter List
    // Test Run - 1
    // Display Ind - 2
    // Fiscal Year - 3-6
    // Run # - 7-8
    // Line # Range (for testing)
    // From Line # - 9-10
    // To Line # - 11-12
    // Case # Range (for testing)
    // From Case # - 13-22
    // To Case # - 23-32
    // Person # Range (for testing)
    // From Person # - 33-42
    // To Person # - 43-52
    // Reporting period (for testing)
    // Start Date - 53-62
    // End date - 63-72
    // Prev Calendar year (for testing)
    // Start Date - 73-82
    // End date - 83-92
    // Program flag - 93  (when set to Y, get case count by program)
    // Case Universe flag - 94  (when set to Y, get case universe)
    // Number of children born out of wedlock in kansas (line 8b from vital 
    // statistics)  - 95-102
    // -----------------------------------------------------
    // ***** Get the run parameters for this program.
    export.ProgramProcessingInfo.Name = import.ProgramProcessingInfo.Name;
    UseReadProgramProcessingInfo();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // 02/04/20 GVandy CQ66220  Beginning in FY 2022, include only amounts that 
    // are both distributed
    // and disbursed.  Export a cutoff FY which defaults to 2022 but can be 
    // overridden with a code
    // table value for testing.
    if (ReadCodeValue())
    {
      export.Cq66220EffectiveFy.FiscalYear =
        (int?)StringToNumber(Substring(
          entities.CodeValue.Cdvalue, CodeValue.Cdvalue_MaxLength, 1, 4));
    }
    else
    {
      export.Cq66220EffectiveFy.FiscalYear = 2022;
    }

    export.TestRunInd.Flag =
      Substring(export.ProgramProcessingInfo.ParameterList, 1, 1);
    export.DisplayInd.Flag =
      Substring(export.ProgramProcessingInfo.ParameterList, 2, 1);
    export.Ocse157Verification.FiscalYear =
      (int?)StringToNumber(Substring(
        export.ProgramProcessingInfo.ParameterList, 3, 4));
    export.Ocse157Verification.RunNumber =
      (int?)StringToNumber(Substring(
        export.ProgramProcessingInfo.ParameterList, 7, 2));
    export.From.LineNumber =
      Substring(export.ProgramProcessingInfo.ParameterList, 9, 2);
    export.To.LineNumber =
      Substring(export.ProgramProcessingInfo.ParameterList, 11, 2);
    export.From.CaseNumber =
      Substring(export.ProgramProcessingInfo.ParameterList, 13, 10);
    export.To.CaseNumber =
      Substring(export.ProgramProcessingInfo.ParameterList, 23, 10);
    export.From.SuppPersonNumber =
      Substring(export.ProgramProcessingInfo.ParameterList, 33, 10);
    export.To.SuppPersonNumber =
      Substring(export.ProgramProcessingInfo.ParameterList, 43, 10);

    if (AsChar(export.TestRunInd.Flag) == 'Y')
    {
      export.ReportStartDate.Date =
        StringToDate(Substring(
          export.ProgramProcessingInfo.ParameterList, 53, 10));
      export.ReportEndDate.Date =
        StringToDate(Substring(
          export.ProgramProcessingInfo.ParameterList, 63, 10));
      export.CalendarYearStart.Date =
        StringToDate(Substring(
          export.ProgramProcessingInfo.ParameterList, 73, 10));
      export.CalendarYearEnd.Date =
        StringToDate(Substring(
          export.ProgramProcessingInfo.ParameterList, 83, 10));
    }
    else
    {
      export.ReportEndDate.Date =
        StringToDate(NumberToString(
          export.Ocse157Verification.FiscalYear.GetValueOrDefault(), 12, 4) +
        "-09-30");
      export.ReportStartDate.Date =
        AddDays(AddYears(export.ReportEndDate.Date, -1), 1);
      export.CalendarYearStart.Date =
        AddYears(StringToDate(
          NumberToString(export.Ocse157Verification.FiscalYear.
          GetValueOrDefault(), 12, 4) + "-01-01"), -1);
      export.CalendarYearEnd.Date =
        AddDays(AddYears(export.CalendarYearStart.Date, 1), -1);
    }

    UseFnBuildTimestampFrmDateTime4();

    // ---------------------------------------------------------------------
    // Set FY start and end timestamps to 7am Oct 1.
    // Eg. FY2001, start tmst - 2000-10-01-07.00.00 and
    // end tmst - 2001-10-01-07.00.00
    // --------------------------------------------------------------------
    export.ReportStartDate.Timestamp =
      AddHours(export.ReportStartDate.Timestamp, 7);
    UseFnBuildTimestampFrmDateTime3();
    export.ReportEndDate.Timestamp =
      AddHours(AddDays(export.ReportEndDate.Timestamp, 1), 7);
    UseFnBuildTimestampFrmDateTime1();

    // ---------------------------------------------------------------------
    // Set CY start and end timestamps to 7am Jan 1.
    // Eg. CY2000, start tmst - 2000-01-01-07.00.00 and
    // end tmst - 2001-01-01-07.00.00
    // --------------------------------------------------------------------
    export.CalendarYearStart.Timestamp =
      AddHours(export.CalendarYearStart.Timestamp, 7);
    UseFnBuildTimestampFrmDateTime2();
    export.CalendarYearEnd.Timestamp =
      AddHours(AddDays(export.CalendarYearEnd.Timestamp, 1), 7);
    export.GetCaseCountPerPepr.Flag =
      Substring(export.ProgramProcessingInfo.ParameterList, 93, 1);
    export.CaseUniverse.Flag =
      Substring(export.ProgramProcessingInfo.ParameterList, 94, 1);
    export.Line8B.Number =
      StringToNumber(
        Substring(export.ProgramProcessingInfo.ParameterList, 95, 8));
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = export.ProgramProcessingInfo.ProcessDate;
    local.EabReportSend.ProgramName = export.ProgramProcessingInfo.Name;

    // **********************************************************
    // OPEN OUTPUT ERROR REPORT 99
    // **********************************************************
    UseCabErrorReport2();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // **********************************************************
    // OPEN OUTPUT CONTROL REPORT 98
    // **********************************************************
    UseCabControlReport2();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // **********************************************************
    // WRITE DISPLAY STATEMENTS.
    // **********************************************************
    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail =
      Substring(
        "RUN PARAMETERS....................................................................",
      1, 50);
    UseCabControlReport1();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail =
      Substring(
        "Run Number......................................................", 1,
      30);
    local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + Substring
      (export.ProgramProcessingInfo.ParameterList, 7, 2);
    UseCabControlReport1();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail =
      Substring("Fiscal Year...............................................", 1,
      30);
    local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + NumberToString
      (DateToInt(export.ReportStartDate.Date), 8, 8) + "-";
    local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + NumberToString
      (DateToInt(export.ReportEndDate.Date), 8, 8) + " ";
    UseCabControlReport1();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    if (!Lt("09", export.From.LineNumber) && !Lt(export.To.LineNumber, "09"))
    {
      local.EabReportSend.RptDetail =
        Substring(
          "Calendar Year for 9a..........................................................",
        1, 30);
      local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + NumberToString
        (DateToInt(export.CalendarYearStart.Date), 8, 8) + "-";
      local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + NumberToString
        (DateToInt(export.CalendarYearEnd.Date), 8, 8) + "";
      UseCabControlReport1();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    local.EabReportSend.RptDetail =
      Substring("Line # range.......................................", 1, 30) +
      (export.From.LineNumber ?? "") + "-" + (export.To.LineNumber ?? "");
    UseCabControlReport1();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail =
      Substring(
        "Case # range.......................................................",
      1, 30) + (export.From.CaseNumber ?? "") + "-" + (export.To.CaseNumber ?? ""
      );
    UseCabControlReport1();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail =
      Substring("Supp Person # range.......................................", 1,
      30) + (export.From.SuppPersonNumber ?? "") + "-" + (
        export.To.SuppPersonNumber ?? "");
    UseCabControlReport1();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail =
      Substring(
        "Children BOW in KS (line 8b).....................................................",
      1, 30);
    local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + Substring
      (export.ProgramProcessingInfo.ParameterList, 95, 8);
    UseCabControlReport1();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail =
      Substring(
        "CQ66220 Fiscal Year Effective...................................", 1,
      30);
    local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + NumberToString
      (export.Cq66220EffectiveFy.FiscalYear.GetValueOrDefault(), 12, 4);
    UseCabControlReport1();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    if (AsChar(export.DisplayInd.Flag) == 'Y')
    {
      local.EabReportSend.RptDetail =
        Substring(
          "FY start tmst.......................................................",
        1, 30) + NumberToString
        (DateToInt(Date(export.ReportStartDate.Timestamp)), 8, 8) + "-" + NumberToString
        (TimeToInt(TimeOfDay(export.ReportStartDate.Timestamp)), 10, 6);
      UseCabControlReport1();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.EabReportSend.RptDetail =
        Substring(
          "FY end tmst.......................................................",
        1, 30) + NumberToString
        (DateToInt(Date(export.ReportEndDate.Timestamp)), 8, 8) + "-" + NumberToString
        (TimeToInt(TimeOfDay(export.ReportEndDate.Timestamp)), 10, 6);
      UseCabControlReport1();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      if (!Lt("09", export.From.LineNumber) && !Lt(export.To.LineNumber, "09"))
      {
        local.EabReportSend.RptDetail =
          Substring(
            "CY start tmst.......................................................",
          1, 30) + NumberToString
          (DateToInt(Date(export.CalendarYearStart.Timestamp)), 8, 8) + "-" + NumberToString
          (TimeToInt(TimeOfDay(export.CalendarYearStart.Timestamp)), 10, 6);
        UseCabControlReport1();

        if (!Equal(local.Status.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        local.EabReportSend.RptDetail =
          Substring(
            "CY end tmst.......................................................",
          1, 30) + NumberToString
          (DateToInt(Date(export.CalendarYearEnd.Timestamp)), 8, 8) + "-" + NumberToString
          (TimeToInt(TimeOfDay(export.CalendarYearEnd.Timestamp)), 10, 6);
        UseCabControlReport1();

        if (!Equal(local.Status.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }

      local.EabReportSend.RptDetail = "";
      UseCabControlReport1();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.EabReportSend.RptDetail =
        "Display indicator is on. Diagnostic information will be written to a DB2 table.";
        
      UseCabControlReport1();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.EabReportSend.RptDetail = "";
      UseCabControlReport1();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    if (AsChar(export.GetCaseCountPerPepr.Flag) == 'Y')
    {
      local.EabReportSend.RptDetail =
        "Get Case count per program flag is set. Review ocse157 data table for totals.";
        
      UseCabControlReport1();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.EabReportSend.RptDetail = "";
      UseCabControlReport1();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    if (AsChar(export.CaseUniverse.Flag) == 'Y')
    {
      local.EabReportSend.RptDetail =
        "Get case universe flag is set. Review ocse157 verifi table for details.";
        
      UseCabControlReport1();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.EabReportSend.RptDetail = "";
      UseCabControlReport1();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    // **********************************************************
    // CREATE VERIFICATION records.
    // **********************************************************
    local.ForCreate.FiscalYear =
      export.Ocse157Verification.FiscalYear.GetValueOrDefault();
    local.ForCreate.RunNumber =
      export.Ocse157Verification.RunNumber.GetValueOrDefault();
    local.ForCreate.Comment =
      Substring(
        "Fiscal Year......................................................", 1,
      15);
    local.ForCreate.Comment = TrimEnd(local.ForCreate.Comment) + NumberToString
      (DateToInt(export.ReportStartDate.Date), 8, 8) + "-";
    local.ForCreate.Comment = TrimEnd(local.ForCreate.Comment) + NumberToString
      (DateToInt(export.ReportEndDate.Date), 8, 8) + "";
    UseFnCreateOcse157Verification();

    if (!Lt("09", export.From.LineNumber) && !Lt(export.To.LineNumber, "09"))
    {
      local.ForCreate.Comment =
        Substring(
          "Calendar Year......................................................",
        1, 15);
      local.ForCreate.Comment = TrimEnd(local.ForCreate.Comment) + NumberToString
        (DateToInt(export.CalendarYearStart.Date), 8, 8) + "-";
      local.ForCreate.Comment = TrimEnd(local.ForCreate.Comment) + NumberToString
        (DateToInt(export.CalendarYearEnd.Date), 8, 8) + "";
      UseFnCreateOcse157Verification();
    }

    // **********************************************************
    // Trap Installation errors.
    // **********************************************************
    if (!Equal(global.UserId, export.ProgramProcessingInfo.Name))
    {
      local.EabReportSend.RptDetail =
        "Severe Error:  User_ID should be set to " + import
        .ProgramProcessingInfo.Name + " instead of " + global.UserId + ".  This is usually due to an error in the generation/installation.";
        
      UseCabControlReport1();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    // **********************************************************
    // Get DB2 commit frequency counts.
    // **********************************************************
    export.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseEabExtractExitStateMessage();
      local.EabReportSend.RptDetail = "Fatal error occurred, must abort.  " + local
        .ExitStateWorkArea.Message;
      UseCabErrorReport1();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
      }
    }
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

  private static void MoveOcse157Verification(Ocse157Verification source,
    Ocse157Verification target)
  {
    target.FiscalYear = source.FiscalYear;
    target.RunNumber = source.RunNumber;
    target.Comment = source.Comment;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
    target.ReadFrequencyCount = source.ReadFrequencyCount;
    target.CheckpointCount = source.CheckpointCount;
    target.LastCheckpointTimestamp = source.LastCheckpointTimestamp;
    target.RestartInd = source.RestartInd;
    target.RestartInfo = source.RestartInfo;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.Status.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    local.Status.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.Status.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.Status.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseFnBuildTimestampFrmDateTime1()
  {
    var useImport = new FnBuildTimestampFrmDateTime.Import();
    var useExport = new FnBuildTimestampFrmDateTime.Export();

    useImport.DateWorkArea.Date = export.CalendarYearStart.Date;

    Call(FnBuildTimestampFrmDateTime.Execute, useImport, useExport);

    MoveDateWorkArea(useExport.DateWorkArea, export.CalendarYearStart);
  }

  private void UseFnBuildTimestampFrmDateTime2()
  {
    var useImport = new FnBuildTimestampFrmDateTime.Import();
    var useExport = new FnBuildTimestampFrmDateTime.Export();

    useImport.DateWorkArea.Date = export.CalendarYearEnd.Date;

    Call(FnBuildTimestampFrmDateTime.Execute, useImport, useExport);

    MoveDateWorkArea(useExport.DateWorkArea, export.CalendarYearEnd);
  }

  private void UseFnBuildTimestampFrmDateTime3()
  {
    var useImport = new FnBuildTimestampFrmDateTime.Import();
    var useExport = new FnBuildTimestampFrmDateTime.Export();

    useImport.DateWorkArea.Date = export.ReportEndDate.Date;

    Call(FnBuildTimestampFrmDateTime.Execute, useImport, useExport);

    MoveDateWorkArea(useExport.DateWorkArea, export.ReportEndDate);
  }

  private void UseFnBuildTimestampFrmDateTime4()
  {
    var useImport = new FnBuildTimestampFrmDateTime.Import();
    var useExport = new FnBuildTimestampFrmDateTime.Export();

    useImport.DateWorkArea.Date = export.ReportStartDate.Date;

    Call(FnBuildTimestampFrmDateTime.Execute, useImport, useExport);

    MoveDateWorkArea(useExport.DateWorkArea, export.ReportStartDate);
  }

  private void UseFnCreateOcse157Verification()
  {
    var useImport = new FnCreateOcse157Verification.Import();
    var useExport = new FnCreateOcse157Verification.Export();

    MoveOcse157Verification(local.ForCreate, useImport.Ocse157Verification);

    Call(FnCreateOcse157Verification.Execute, useImport, useExport);
  }

  private void UseReadPgmCheckpointRestart()
  {
    var useImport = new ReadPgmCheckpointRestart.Import();
    var useExport = new ReadPgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      export.ProgramCheckpointRestart.ProgramName;

    Call(ReadPgmCheckpointRestart.Execute, useImport, useExport);

    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      export.ProgramCheckpointRestart);
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = export.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    export.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private bool ReadCodeValue()
  {
    entities.CodeValue.Populated = false;

    return Read("ReadCodeValue",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          export.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
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
    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CaseUniverse.
    /// </summary>
    [JsonPropertyName("caseUniverse")]
    public Common CaseUniverse
    {
      get => caseUniverse ??= new();
      set => caseUniverse = value;
    }

    /// <summary>
    /// A value of GetCaseCountPerPepr.
    /// </summary>
    [JsonPropertyName("getCaseCountPerPepr")]
    public Common GetCaseCountPerPepr
    {
      get => getCaseCountPerPepr ??= new();
      set => getCaseCountPerPepr = value;
    }

    /// <summary>
    /// A value of CalendarYearStart.
    /// </summary>
    [JsonPropertyName("calendarYearStart")]
    public DateWorkArea CalendarYearStart
    {
      get => calendarYearStart ??= new();
      set => calendarYearStart = value;
    }

    /// <summary>
    /// A value of CalendarYearEnd.
    /// </summary>
    [JsonPropertyName("calendarYearEnd")]
    public DateWorkArea CalendarYearEnd
    {
      get => calendarYearEnd ??= new();
      set => calendarYearEnd = value;
    }

    /// <summary>
    /// A value of From.
    /// </summary>
    [JsonPropertyName("from")]
    public Ocse157Verification From
    {
      get => from ??= new();
      set => from = value;
    }

    /// <summary>
    /// A value of To.
    /// </summary>
    [JsonPropertyName("to")]
    public Ocse157Verification To
    {
      get => to ??= new();
      set => to = value;
    }

    /// <summary>
    /// A value of Ocse157Verification.
    /// </summary>
    [JsonPropertyName("ocse157Verification")]
    public Ocse157Verification Ocse157Verification
    {
      get => ocse157Verification ??= new();
      set => ocse157Verification = value;
    }

    /// <summary>
    /// A value of ReportEndDate.
    /// </summary>
    [JsonPropertyName("reportEndDate")]
    public DateWorkArea ReportEndDate
    {
      get => reportEndDate ??= new();
      set => reportEndDate = value;
    }

    /// <summary>
    /// A value of ReportStartDate.
    /// </summary>
    [JsonPropertyName("reportStartDate")]
    public DateWorkArea ReportStartDate
    {
      get => reportStartDate ??= new();
      set => reportStartDate = value;
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
    /// A value of TestRunInd.
    /// </summary>
    [JsonPropertyName("testRunInd")]
    public Common TestRunInd
    {
      get => testRunInd ??= new();
      set => testRunInd = value;
    }

    /// <summary>
    /// A value of DisplayInd.
    /// </summary>
    [JsonPropertyName("displayInd")]
    public Common DisplayInd
    {
      get => displayInd ??= new();
      set => displayInd = value;
    }

    /// <summary>
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    /// <summary>
    /// A value of Line8B.
    /// </summary>
    [JsonPropertyName("line8B")]
    public Ocse157Data Line8B
    {
      get => line8B ??= new();
      set => line8B = value;
    }

    /// <summary>
    /// A value of Cq66220EffectiveFy.
    /// </summary>
    [JsonPropertyName("cq66220EffectiveFy")]
    public Ocse157Verification Cq66220EffectiveFy
    {
      get => cq66220EffectiveFy ??= new();
      set => cq66220EffectiveFy = value;
    }

    private Common caseUniverse;
    private Common getCaseCountPerPepr;
    private DateWorkArea calendarYearStart;
    private DateWorkArea calendarYearEnd;
    private Ocse157Verification from;
    private Ocse157Verification to;
    private Ocse157Verification ocse157Verification;
    private DateWorkArea reportEndDate;
    private DateWorkArea reportStartDate;
    private ProgramProcessingInfo programProcessingInfo;
    private Common testRunInd;
    private Common displayInd;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Ocse157Data line8B;
    private Ocse157Verification cq66220EffectiveFy;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ForCreate.
    /// </summary>
    [JsonPropertyName("forCreate")]
    public Ocse157Verification ForCreate
    {
      get => forCreate ??= new();
      set => forCreate = value;
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
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    /// <summary>
    /// A value of Status.
    /// </summary>
    [JsonPropertyName("status")]
    public EabFileHandling Status
    {
      get => status ??= new();
      set => status = value;
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

    private Ocse157Verification forCreate;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private EabFileHandling status;
    private ExitStateWorkArea exitStateWorkArea;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    private CodeValue codeValue;
    private Code code;
    private ProgramProcessingInfo programProcessingInfo;
  }
#endregion
}
