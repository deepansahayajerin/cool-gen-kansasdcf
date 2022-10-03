// Program: FN_B700_BATCH_INITIALIZATION, ID: 373315427, model: 746.
// Short name: SWE02982
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B700_BATCH_INITIALIZATION.
/// </summary>
[Serializable]
public partial class FnB700BatchInitialization: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B700_BATCH_INITIALIZATION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB700BatchInitialization(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB700BatchInitialization.
  /// </summary>
  public FnB700BatchInitialization(IContext context, Import import,
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
    // ****************************************************************************
    // **                 M A I N T E N A N C E   L O G
    // ****************************************************************************
    // ** Date		WR/PR	Developer	Description
    // ****************************************************************************
    // ** 02/2002		K Doshi		Initial code.
    // ** 12/05/2003	040134	E.Shirk		Federally mandated OCSE34 report changes.
    // ***************************************************************************
    // ***********************************************************
    // Parameter List
    // Display Ind - 1
    // FMAP rate - 3-6
    // Step # Range (for testing)
    // From - 8-9
    // To   - 10-11
    // Cash Receipt # Range (for testing)
    // From - 13-21
    // To   - 22-30
    // AP Person # Range (for testing)
    // From - 33-42
    // To   - 43-52
    // ---------------------------------------------
    // ***** Get the run parameters for this program.
    export.ProgramProcessingInfo.Name = import.ProgramProcessingInfo.Name;
    UseReadProgramProcessingInfo();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      local.EabReportSend.RptDetail = "** Bad return from Read PPI cab.";
      UseCabErrorReport1();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    export.DisplayInd.Flag =
      Substring(export.ProgramProcessingInfo.ParameterList, 1, 1);

    if (!IsEmpty(Substring(export.ProgramProcessingInfo.ParameterList, 8, 2)))
    {
      export.FromOcse157Verification.LineNumber =
        Substring(export.ProgramProcessingInfo.ParameterList, 8, 2);
    }
    else
    {
      export.FromOcse157Verification.LineNumber = "000";
    }

    if (!IsEmpty(Substring(export.ProgramProcessingInfo.ParameterList, 10, 2)))
    {
      export.ToOcse157Verification.LineNumber =
        Substring(export.ProgramProcessingInfo.ParameterList, 10, 2);
    }
    else
    {
      export.ToOcse157Verification.LineNumber = "999";
    }

    if (!IsEmpty(Substring(export.ProgramProcessingInfo.ParameterList, 13, 9)))
    {
      export.FromCashReceipt.SequentialNumber =
        (int)StringToNumber(Substring(
          export.ProgramProcessingInfo.ParameterList, 13, 9));
    }

    if (!IsEmpty(Substring(export.ProgramProcessingInfo.ParameterList, 22, 9)))
    {
      export.ToCashReceipt.SequentialNumber =
        (int)StringToNumber(Substring(
          export.ProgramProcessingInfo.ParameterList, 22, 9));
    }
    else
    {
      export.ToCashReceipt.SequentialNumber = 999999999;
    }

    if (!IsEmpty(Substring(export.ProgramProcessingInfo.ParameterList, 33, 10)))
    {
      export.FromOcse157Verification.ObligorPersonNbr =
        Substring(export.ProgramProcessingInfo.ParameterList, 33, 10);
    }
    else
    {
      export.FromOcse157Verification.ObligorPersonNbr = "0000000000";
    }

    if (!IsEmpty(Substring(export.ProgramProcessingInfo.ParameterList, 43, 10)))
    {
      export.ToOcse157Verification.ObligorPersonNbr =
        Substring(export.ProgramProcessingInfo.ParameterList, 43, 10);
    }
    else
    {
      export.ToOcse157Verification.ObligorPersonNbr = "9999999999";
    }

    // *****************************************************************
    // ***           Set Processing Quarter Value
    // *****************************************************************
    if (Month(export.ProgramProcessingInfo.ProcessDate) <= 3)
    {
      // *** Reporting for 4th QUARTER
      export.Curr.Period =
        (int)StringToNumber(NumberToString(
          Year(AddYears(export.ProgramProcessingInfo.ProcessDate, -1)), 12, 4) +
        "04");
      export.Prev.Period = export.Curr.Period - 1;
    }
    else if (Month(export.ProgramProcessingInfo.ProcessDate) <= 6)
    {
      // *** Reporting for 1st QUARTER
      export.Curr.Period =
        (int)StringToNumber(NumberToString(
          Year(export.ProgramProcessingInfo.ProcessDate), 12, 4) + "01");
      export.Prev.Period =
        (int)StringToNumber(NumberToString(
          Year(AddYears(export.ProgramProcessingInfo.ProcessDate, -1)), 12, 4) +
        "04");
    }
    else if (Month(export.ProgramProcessingInfo.ProcessDate) <= 9)
    {
      // *** Reporting for 2nd QUARTER
      export.Curr.Period =
        (int)StringToNumber(NumberToString(
          Year(export.ProgramProcessingInfo.ProcessDate), 12, 4) + "02");
      export.Prev.Period = export.Curr.Period - 1;
    }
    else
    {
      // *** Reporting for 3rd QUARTER
      export.Curr.Period =
        (int)StringToNumber(NumberToString(
          Year(export.ProgramProcessingInfo.ProcessDate), 12, 4) + "03");
      export.Prev.Period = export.Curr.Period - 1;
    }

    local.Ocse34FoundInd.Flag = "N";

    if (ReadOcse34())
    {
      local.Ocse34FoundInd.Flag = "Y";
    }

    if (AsChar(local.Ocse34FoundInd.Flag) == 'N')
    {
      ExitState = "FN0000_OCSE34_NF";

      return;
    }

    export.Curr.Assign(entities.Ocse34);
    export.Curr.FmapRate =
      StringToNumber(Substring(export.ProgramProcessingInfo.ParameterList, 3, 4))
      / (decimal)10000;
    export.Curr.ReportingPeriodBeginDate =
      entities.Ocse34.ReportingPeriodBeginDate;
    export.Curr.ReportingPeriodEndDate = entities.Ocse34.ReportingPeriodEndDate;
    export.RptPrdBeginDt.Date = entities.Ocse34.ReportingPeriodBeginDate;
    export.RptPrdEndDt.Date = entities.Ocse34.ReportingPeriodEndDate;
    export.NonPayReqRptBeginDt.Date =
      entities.Ocse34.AltReportingPeriodBeginDate;
    export.NonPayReqRptEndDt.Date = entities.Ocse34.AltReportingPeriodEndDate;
    export.Ocse157Verification.FiscalYear =
      (int?)StringToNumber(NumberToString(export.Curr.Period, 10, 4));
    export.Ocse157Verification.RunNumber =
      (int?)StringToNumber(NumberToString(export.Curr.Period, 14, 2));

    // --------------------------------------------------------------------
    // Set start and end timestamps
    // --------------------------------------------------------------------
    UseFnBuildTimestampFrmDateTime3();
    UseFnBuildTimestampFrmDateTime4();
    export.RptPrdEndDt.Timestamp =
      AddMicroseconds(AddDays(export.RptPrdEndDt.Timestamp, 1), -1);
    UseFnBuildTimestampFrmDateTime1();
    UseFnBuildTimestampFrmDateTime2();
    export.NonPayReqRptEndDt.Timestamp =
      AddMicroseconds(AddDays(export.NonPayReqRptEndDt.Timestamp, 1), -1);
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
      Substring("Report Quarter...............................................",
      1, 30);
    local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + NumberToString
      (export.Curr.Period, 10, 6);
    UseCabControlReport1();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail =
      Substring("Report Period...............................................",
      1, 30);
    local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + NumberToString
      (DateToInt(export.RptPrdBeginDt.Date), 8, 8) + "-";
    local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + NumberToString
      (DateToInt(export.RptPrdEndDt.Date), 8, 8) + " ";
    UseCabControlReport1();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail =
      Substring("Alt Report Period...........................................",
      1, 30);
    local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + NumberToString
      (DateToInt(export.NonPayReqRptBeginDt.Date), 8, 8) + "-";
    local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + NumberToString
      (DateToInt(export.NonPayReqRptEndDt.Date), 8, 8) + " ";
    UseCabControlReport1();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail =
      Substring("FMAP Rate...............................................", 1,
      31);
    local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + Substring
      (export.ProgramProcessingInfo.ParameterList, 3, 4);
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
          "Previous Quarter...............................................", 1,
        30);
      local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + NumberToString
        (export.Prev.Period, 10, 6);
      UseCabControlReport1();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.EabReportSend.RptDetail =
        Substring("CR# Range...............................................", 1,
        30);
      local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + NumberToString
        (export.FromCashReceipt.SequentialNumber, 9, 7);
      local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + "-"
        ;
      local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + NumberToString
        (export.ToCashReceipt.SequentialNumber, 9, 7);
      UseCabControlReport1();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.EabReportSend.RptDetail =
        Substring("AP# Range...............................................", 1,
        30);
      local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + (
        export.FromOcse157Verification.ObligorPersonNbr ?? "");
      local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + "-"
        ;
      local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + (
        export.ToOcse157Verification.ObligorPersonNbr ?? "");
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

    useImport.DateWorkArea.Date = export.NonPayReqRptBeginDt.Date;

    Call(FnBuildTimestampFrmDateTime.Execute, useImport, useExport);

    MoveDateWorkArea(useExport.DateWorkArea, export.NonPayReqRptBeginDt);
  }

  private void UseFnBuildTimestampFrmDateTime2()
  {
    var useImport = new FnBuildTimestampFrmDateTime.Import();
    var useExport = new FnBuildTimestampFrmDateTime.Export();

    useImport.DateWorkArea.Date = export.NonPayReqRptEndDt.Date;

    Call(FnBuildTimestampFrmDateTime.Execute, useImport, useExport);

    MoveDateWorkArea(useExport.DateWorkArea, export.NonPayReqRptEndDt);
  }

  private void UseFnBuildTimestampFrmDateTime3()
  {
    var useImport = new FnBuildTimestampFrmDateTime.Import();
    var useExport = new FnBuildTimestampFrmDateTime.Export();

    useImport.DateWorkArea.Date = export.RptPrdBeginDt.Date;

    Call(FnBuildTimestampFrmDateTime.Execute, useImport, useExport);

    MoveDateWorkArea(useExport.DateWorkArea, export.RptPrdBeginDt);
  }

  private void UseFnBuildTimestampFrmDateTime4()
  {
    var useImport = new FnBuildTimestampFrmDateTime.Import();
    var useExport = new FnBuildTimestampFrmDateTime.Export();

    useImport.DateWorkArea.Date = export.RptPrdEndDt.Date;

    Call(FnBuildTimestampFrmDateTime.Execute, useImport, useExport);

    MoveDateWorkArea(useExport.DateWorkArea, export.RptPrdEndDt);
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

  private bool ReadOcse34()
  {
    entities.Ocse34.Populated = false;

    return Read("ReadOcse34",
      (db, command) =>
      {
        db.SetInt32(command, "period", export.Curr.Period);
      },
      (db, reader) =>
      {
        entities.Ocse34.Period = db.GetInt32(reader, 0);
        entities.Ocse34.PreviousUndistribAmount = db.GetInt32(reader, 1);
        entities.Ocse34.AdjustmentsAmount = db.GetNullableInt32(reader, 2);
        entities.Ocse34.NonIvdCasesAmount = db.GetNullableInt32(reader, 3);
        entities.Ocse34.UndistributedAmount = db.GetNullableInt32(reader, 4);
        entities.Ocse34.IncentivePaymentAmount = db.GetNullableInt32(reader, 5);
        entities.Ocse34.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.Ocse34.FmapRate = db.GetDecimal(reader, 7);
        entities.Ocse34.CseDisbCreditAmt = db.GetNullableInt32(reader, 8);
        entities.Ocse34.CseDisbDebitAmt = db.GetNullableInt32(reader, 9);
        entities.Ocse34.CseWarrantAmt = db.GetNullableInt32(reader, 10);
        entities.Ocse34.CsePaymentAmt = db.GetNullableInt32(reader, 11);
        entities.Ocse34.CseInterstateAmt = db.GetNullableInt32(reader, 12);
        entities.Ocse34.CseCshRcptDtlSuspAmt = db.GetNullableInt32(reader, 13);
        entities.Ocse34.CseDisbSuppressAmt = db.GetNullableInt32(reader, 14);
        entities.Ocse34.ReportingPeriodBeginDate =
          db.GetNullableDate(reader, 15);
        entities.Ocse34.ReportingPeriodEndDate = db.GetNullableDate(reader, 16);
        entities.Ocse34.KpcNon4DIwoCollAmt = db.GetNullableInt32(reader, 17);
        entities.Ocse34.KpcIvdNonIwoCollAmt = db.GetNullableInt32(reader, 18);
        entities.Ocse34.KpcNonIvdIwoForwCollAmt =
          db.GetNullableInt32(reader, 19);
        entities.Ocse34.KpcStaleDateAmt = db.GetNullableInt32(reader, 20);
        entities.Ocse34.KpcHeldDisbAmt = db.GetNullableInt32(reader, 21);
        entities.Ocse34.UiIvdNonIwoIntAmt = db.GetNullableInt32(reader, 22);
        entities.Ocse34.KpcUiIvdNonIwoNonIntAmt =
          db.GetNullableInt32(reader, 23);
        entities.Ocse34.KpcUiIvdNivdIwoAmt = db.GetNullableInt32(reader, 24);
        entities.Ocse34.KpcUiNonIvdIwoAmt = db.GetNullableInt32(reader, 25);
        entities.Ocse34.KpcNivdIwoLda = db.GetNullableInt32(reader, 26);
        entities.Ocse34.AltReportingPeriodBeginDate =
          db.GetNullableDate(reader, 27);
        entities.Ocse34.AltReportingPeriodEndDate =
          db.GetNullableDate(reader, 28);
        entities.Ocse34.FdsoDsbSuppAmt = db.GetNullableInt32(reader, 29);
        entities.Ocse34.Populated = true;
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
    /// A value of Curr.
    /// </summary>
    [JsonPropertyName("curr")]
    public Ocse34 Curr
    {
      get => curr ??= new();
      set => curr = value;
    }

    /// <summary>
    /// A value of FromCashReceipt.
    /// </summary>
    [JsonPropertyName("fromCashReceipt")]
    public CashReceipt FromCashReceipt
    {
      get => fromCashReceipt ??= new();
      set => fromCashReceipt = value;
    }

    /// <summary>
    /// A value of ToCashReceipt.
    /// </summary>
    [JsonPropertyName("toCashReceipt")]
    public CashReceipt ToCashReceipt
    {
      get => toCashReceipt ??= new();
      set => toCashReceipt = value;
    }

    /// <summary>
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public Ocse34 Prev
    {
      get => prev ??= new();
      set => prev = value;
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
    /// A value of DisplayInd.
    /// </summary>
    [JsonPropertyName("displayInd")]
    public Common DisplayInd
    {
      get => displayInd ??= new();
      set => displayInd = value;
    }

    /// <summary>
    /// A value of RptPrdBeginDt.
    /// </summary>
    [JsonPropertyName("rptPrdBeginDt")]
    public DateWorkArea RptPrdBeginDt
    {
      get => rptPrdBeginDt ??= new();
      set => rptPrdBeginDt = value;
    }

    /// <summary>
    /// A value of RptPrdEndDt.
    /// </summary>
    [JsonPropertyName("rptPrdEndDt")]
    public DateWorkArea RptPrdEndDt
    {
      get => rptPrdEndDt ??= new();
      set => rptPrdEndDt = value;
    }

    /// <summary>
    /// A value of NonPayReqRptBeginDt.
    /// </summary>
    [JsonPropertyName("nonPayReqRptBeginDt")]
    public DateWorkArea NonPayReqRptBeginDt
    {
      get => nonPayReqRptBeginDt ??= new();
      set => nonPayReqRptBeginDt = value;
    }

    /// <summary>
    /// A value of NonPayReqRptEndDt.
    /// </summary>
    [JsonPropertyName("nonPayReqRptEndDt")]
    public DateWorkArea NonPayReqRptEndDt
    {
      get => nonPayReqRptEndDt ??= new();
      set => nonPayReqRptEndDt = value;
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
    /// A value of Ocse157Verification.
    /// </summary>
    [JsonPropertyName("ocse157Verification")]
    public Ocse157Verification Ocse157Verification
    {
      get => ocse157Verification ??= new();
      set => ocse157Verification = value;
    }

    /// <summary>
    /// A value of FromOcse157Verification.
    /// </summary>
    [JsonPropertyName("fromOcse157Verification")]
    public Ocse157Verification FromOcse157Verification
    {
      get => fromOcse157Verification ??= new();
      set => fromOcse157Verification = value;
    }

    /// <summary>
    /// A value of ToOcse157Verification.
    /// </summary>
    [JsonPropertyName("toOcse157Verification")]
    public Ocse157Verification ToOcse157Verification
    {
      get => toOcse157Verification ??= new();
      set => toOcse157Verification = value;
    }

    private Ocse34 curr;
    private CashReceipt fromCashReceipt;
    private CashReceipt toCashReceipt;
    private Ocse34 prev;
    private ProgramProcessingInfo programProcessingInfo;
    private Common displayInd;
    private DateWorkArea rptPrdBeginDt;
    private DateWorkArea rptPrdEndDt;
    private DateWorkArea nonPayReqRptBeginDt;
    private DateWorkArea nonPayReqRptEndDt;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Ocse157Verification ocse157Verification;
    private Ocse157Verification fromOcse157Verification;
    private Ocse157Verification toOcse157Verification;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Ocse34FoundInd.
    /// </summary>
    [JsonPropertyName("ocse34FoundInd")]
    public Common Ocse34FoundInd
    {
      get => ocse34FoundInd ??= new();
      set => ocse34FoundInd = value;
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

    private Common ocse34FoundInd;
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
    /// A value of Ocse34.
    /// </summary>
    [JsonPropertyName("ocse34")]
    public Ocse34 Ocse34
    {
      get => ocse34 ??= new();
      set => ocse34 = value;
    }

    private Ocse34 ocse34;
  }
#endregion
}
