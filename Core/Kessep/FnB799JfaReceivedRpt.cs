// Program: FN_B799_JFA_RECEIVED_RPT, ID: 1625407826, model: 746.
// Short name: SWEB799P
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B799_JFA_RECEIVED_RPT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB799JfaReceivedRpt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B799_JFA_RECEIVED_RPT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB799JfaReceivedRpt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB799JfaReceivedRpt.
  /// </summary>
  public FnB799JfaReceivedRpt(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***********************************************************************************************
    // DATE		Developer	Description
    // 03/01/2021      DDupree   	Initial Creation - CQ68787
    // ***********************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    UseFnB799Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    UseFnBuildTimestampFrmDateTime1();
    UseFnBuildTimestampFrmDateTime2();
    local.EndDate.Timestamp =
      AddMicroseconds(AddDays(local.EndDate.Timestamp, 1), -1);
    local.NumberOfRecordsRead.Count = 0;
    local.CurrentDate.Date = local.ProgramProcessingInfo.ProcessDate;
    local.RecordsFound.Flag = "";

    foreach(var item in ReadCashReceiptDetailCashReceipt())
    {
      local.Amount.Text15 =
        NumberToString((long)(entities.CashReceiptDetail.CollectionAmount * 100),
        15);
      local.Amount.Text12 =
        Substring(local.Amount.Text15, WorkArea.Text15_MaxLength, 5, 9) + "."
        + Substring(local.Amount.Text15, WorkArea.Text15_MaxLength, 14, 2);
      local.CashEvent.Text15 =
        NumberToString(entities.CashReceipt.SequentialNumber, 15);
      local.CashEventDetail.Text15 =
        NumberToString(entities.CashReceiptDetail.SequentialIdentifier, 15);
      local.CeDetailNumber.Text15 =
        Substring(local.CashEvent.Text15, WorkArea.Text15_MaxLength, 7, 9) + "-"
        + Substring
        (local.CashEventDetail.Text15, WorkArea.Text15_MaxLength, 12, 4);
      local.TaxYear.Text4 =
        NumberToString(entities.CashReceiptDetail.OffsetTaxYear.
          GetValueOrDefault(), 12, 4);

      if (Lt(local.ZeroDate.Date, entities.CashReceiptDetail.JfaReceivedDate))
      {
        local.JfaReceivedDt.Year =
          Year(entities.CashReceiptDetail.JfaReceivedDate);
        local.WorkArea.Text15 = NumberToString(local.JfaReceivedDt.Year, 15);
        local.Year.Text4 = Substring(local.WorkArea.Text15, 12, 4);
        local.JfaReceivedDt.Month =
          Month(entities.CashReceiptDetail.JfaReceivedDate);
        local.WorkArea.Text15 = NumberToString(local.JfaReceivedDt.Month, 15);
        local.Month.Text2 = Substring(local.WorkArea.Text15, 14, 2);
        local.JfaReceivedDt.Day =
          Day(entities.CashReceiptDetail.JfaReceivedDate);
        local.WorkArea.Text15 = NumberToString(local.JfaReceivedDt.Day, 15);
        local.Day.Text2 = Substring(local.WorkArea.Text15, 14, 2);
        local.JfaReceivedDt.TextDate = local.Year.Text4 + local.Month.Text2 + local
          .Day.Text2;
        local.JfaReceivedDate.Text10 = local.Month.Text2 + "/" + local
          .Day.Text2 + "/" + local.Year.Text4;
      }
      else
      {
        local.JfaReceivedDate.Text10 = "";
      }

      if (Lt(local.ZeroDate.Date, entities.CashReceiptDetail.CruProcessedDate))
      {
        local.CruProcessDt.Year =
          Year(entities.CashReceiptDetail.CruProcessedDate);
        local.WorkArea.Text15 = NumberToString(local.CruProcessDt.Year, 15);
        local.Year.Text4 = Substring(local.WorkArea.Text15, 12, 4);
        local.CruProcessDt.Month =
          Month(entities.CashReceiptDetail.CruProcessedDate);
        local.WorkArea.Text15 = NumberToString(local.CruProcessDt.Month, 15);
        local.Month.Text2 = Substring(local.WorkArea.Text15, 14, 2);
        local.CruProcessDt.Day =
          Day(entities.CashReceiptDetail.CruProcessedDate);
        local.WorkArea.Text15 = NumberToString(local.CruProcessDt.Day, 15);
        local.Day.Text2 = Substring(local.WorkArea.Text15, 14, 2);
        local.CruProcessDt.TextDate = local.Year.Text4 + local.Month.Text2 + local
          .Day.Text2;
        local.CruProcessDate.Text10 = local.Month.Text2 + "/" + local
          .Day.Text2 + "/" + local.Year.Text4;
      }
      else
      {
        local.CruProcessDate.Text10 = "";
      }

      local.PartOne.Text35 = " " + entities
        .CashReceiptDetail.ObligorPersonNumber + ";" + local.TaxYear.Text4 + ";"
        + local.CeDetailNumber.Text15 + ";";
      local.PartOne.Text37 = local.Amount.Text12 + ";" + local
        .JfaReceivedDate.Text10 + ";" + local.CruProcessDate.Text10 + ";";
      local.EabReportSend.RptDetail = local.PartOne.Text35 + local
        .PartOne.Text37;
      local.EabFileHandling.Action = "WRITE";
      UseEabExternalReportWriter2();

      if (!Equal(local.External.TextReturnCode, "OK"))
      {
        ExitState = "FILE_READ_ERROR_WITH_RB";

        return;
      }

      ++local.NumberOfRecordsRead.Count;
      local.RecordsFound.Flag = "Y";
    }

    if (AsChar(local.RecordsFound.Flag) == 'Y')
    {
    }
    else
    {
      local.EabReportSend.RptDetail =
        "                                    ***********No records found********";
        
      local.EabFileHandling.Action = "WRITE";
      UseEabExternalReportWriter2();

      if (!Equal(local.External.TextReturnCode, "OK"))
      {
        ExitState = "FILE_READ_ERROR_WITH_RB";
      }
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseSpB737Close();
      local.EabFileHandling.Action = "CLOSE";
      UseEabExternalReportWriter1();

      if (!Equal(local.External.TextReturnCode, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      }
    }
    else
    {
      UseEabExtractExitStateMessage();
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Program abended because: " + local
        .ExitStateWorkArea.Message;
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      UseSpB737Close();
      local.EabFileHandling.Action = "CLOSE";
      UseEabExternalReportWriter1();

      if (!Equal(local.External.TextReturnCode, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveDateWorkArea1(DateWorkArea source, DateWorkArea target)
    
  {
    target.Date = source.Date;
    target.Time = source.Time;
  }

  private static void MoveDateWorkArea2(DateWorkArea source, DateWorkArea target)
    
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

  private static void MoveExternal(External source, External target)
  {
    target.NumericReturnCode = source.NumericReturnCode;
    target.TextReturnCode = source.TextReturnCode;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabExternalReportWriter1()
  {
    var useImport = new EabExternalReportWriter.Import();
    var useExport = new EabExternalReportWriter.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveExternal(local.External, useExport.External);

    Call(EabExternalReportWriter.Execute, useImport, useExport);

    MoveExternal(useExport.External, local.External);
  }

  private void UseEabExternalReportWriter2()
  {
    var useImport = new EabExternalReportWriter.Import();
    var useExport = new EabExternalReportWriter.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.EabReportSend.RptDetail = local.EabReportSend.RptDetail;
    MoveExternal(local.External, useExport.External);

    Call(EabExternalReportWriter.Execute, useImport, useExport);

    MoveExternal(useExport.External, local.External);
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseFnB799Housekeeping()
  {
    var useImport = new FnB799Housekeeping.Import();
    var useExport = new FnB799Housekeeping.Export();

    Call(FnB799Housekeeping.Execute, useImport, useExport);

    local.EndDate.Date = useExport.EndDate.Date;
    local.BeginDate.Date = useExport.BeginDate.Date;
    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseFnBuildTimestampFrmDateTime1()
  {
    var useImport = new FnBuildTimestampFrmDateTime.Import();
    var useExport = new FnBuildTimestampFrmDateTime.Export();

    useImport.DateWorkArea.Date = local.BeginDate.Date;

    Call(FnBuildTimestampFrmDateTime.Execute, useImport, useExport);

    MoveDateWorkArea2(useExport.DateWorkArea, local.BeginDate);
  }

  private void UseFnBuildTimestampFrmDateTime2()
  {
    var useImport = new FnBuildTimestampFrmDateTime.Import();
    var useExport = new FnBuildTimestampFrmDateTime.Export();

    MoveDateWorkArea1(local.EndDate, useImport.DateWorkArea);

    Call(FnBuildTimestampFrmDateTime.Execute, useImport, useExport);

    local.EndDate.Assign(useExport.DateWorkArea);
  }

  private void UseSpB737Close()
  {
    var useImport = new SpB737Close.Import();
    var useExport = new SpB737Close.Export();

    useImport.NumberOfRecordsRead.Count = local.NumberOfRecordsRead.Count;

    Call(SpB737Close.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadCashReceiptDetailCashReceipt()
  {
    entities.CashReceipt.Populated = false;
    entities.CashReceiptDetail.Populated = false;

    return ReadEach("ReadCashReceiptDetailCashReceipt",
      (db, command) =>
      {
        db.SetDateTime(
          command, "timestamp1", local.BeginDate.Timestamp.GetValueOrDefault());
          
        db.SetDateTime(
          command, "timestamp2", local.EndDate.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 4);
        entities.CashReceiptDetail.OffsetTaxYear =
          db.GetNullableInt32(reader, 5);
        entities.CashReceiptDetail.JointReturnInd =
          db.GetNullableString(reader, 6);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 7);
        entities.CashReceiptDetail.CreatedBy = db.GetString(reader, 8);
        entities.CashReceiptDetail.CreatedTmst = db.GetDateTime(reader, 9);
        entities.CashReceiptDetail.InjuredSpouseInd =
          db.GetNullableString(reader, 10);
        entities.CashReceiptDetail.JfaReceivedDate =
          db.GetNullableDate(reader, 11);
        entities.CashReceiptDetail.CruProcessedDate =
          db.GetNullableDate(reader, 12);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 13);
        entities.CashReceipt.Populated = true;
        entities.CashReceiptDetail.Populated = true;
        CheckValid<CashReceiptDetail>("JointReturnInd",
          entities.CashReceiptDetail.JointReturnInd);

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
    /// <summary>
    /// A value of CeDetailNumber.
    /// </summary>
    [JsonPropertyName("ceDetailNumber")]
    public WorkArea CeDetailNumber
    {
      get => ceDetailNumber ??= new();
      set => ceDetailNumber = value;
    }

    /// <summary>
    /// A value of CashEventDetail.
    /// </summary>
    [JsonPropertyName("cashEventDetail")]
    public WorkArea CashEventDetail
    {
      get => cashEventDetail ??= new();
      set => cashEventDetail = value;
    }

    /// <summary>
    /// A value of CashEvent.
    /// </summary>
    [JsonPropertyName("cashEvent")]
    public WorkArea CashEvent
    {
      get => cashEvent ??= new();
      set => cashEvent = value;
    }

    /// <summary>
    /// A value of Amount.
    /// </summary>
    [JsonPropertyName("amount")]
    public WorkArea Amount
    {
      get => amount ??= new();
      set => amount = value;
    }

    /// <summary>
    /// A value of TaxYear.
    /// </summary>
    [JsonPropertyName("taxYear")]
    public TextWorkArea TaxYear
    {
      get => taxYear ??= new();
      set => taxYear = value;
    }

    /// <summary>
    /// A value of BeginDate.
    /// </summary>
    [JsonPropertyName("beginDate")]
    public DateWorkArea BeginDate
    {
      get => beginDate ??= new();
      set => beginDate = value;
    }

    /// <summary>
    /// A value of EndDate.
    /// </summary>
    [JsonPropertyName("endDate")]
    public DateWorkArea EndDate
    {
      get => endDate ??= new();
      set => endDate = value;
    }

    /// <summary>
    /// A value of Write.
    /// </summary>
    [JsonPropertyName("write")]
    public External Write
    {
      get => write ??= new();
      set => write = value;
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
    /// A value of RecordsFound.
    /// </summary>
    [JsonPropertyName("recordsFound")]
    public Common RecordsFound
    {
      get => recordsFound ??= new();
      set => recordsFound = value;
    }

    /// <summary>
    /// A value of BeginningOfPeriod.
    /// </summary>
    [JsonPropertyName("beginningOfPeriod")]
    public DateWorkArea BeginningOfPeriod
    {
      get => beginningOfPeriod ??= new();
      set => beginningOfPeriod = value;
    }

    /// <summary>
    /// A value of Changed.
    /// </summary>
    [JsonPropertyName("changed")]
    public Common Changed
    {
      get => changed ??= new();
      set => changed = value;
    }

    /// <summary>
    /// A value of CruProcessDate.
    /// </summary>
    [JsonPropertyName("cruProcessDate")]
    public WorkArea CruProcessDate
    {
      get => cruProcessDate ??= new();
      set => cruProcessDate = value;
    }

    /// <summary>
    /// A value of JfaReceivedDate.
    /// </summary>
    [JsonPropertyName("jfaReceivedDate")]
    public WorkArea JfaReceivedDate
    {
      get => jfaReceivedDate ??= new();
      set => jfaReceivedDate = value;
    }

    /// <summary>
    /// A value of CruProcessDt.
    /// </summary>
    [JsonPropertyName("cruProcessDt")]
    public DateWorkArea CruProcessDt
    {
      get => cruProcessDt ??= new();
      set => cruProcessDt = value;
    }

    /// <summary>
    /// A value of BeginningOfTheMonth.
    /// </summary>
    [JsonPropertyName("beginningOfTheMonth")]
    public DateWorkArea BeginningOfTheMonth
    {
      get => beginningOfTheMonth ??= new();
      set => beginningOfTheMonth = value;
    }

    /// <summary>
    /// A value of ZeroDate.
    /// </summary>
    [JsonPropertyName("zeroDate")]
    public DateWorkArea ZeroDate
    {
      get => zeroDate ??= new();
      set => zeroDate = value;
    }

    /// <summary>
    /// A value of JfaReceivedDt.
    /// </summary>
    [JsonPropertyName("jfaReceivedDt")]
    public DateWorkArea JfaReceivedDt
    {
      get => jfaReceivedDt ??= new();
      set => jfaReceivedDt = value;
    }

    /// <summary>
    /// A value of CurrentDate.
    /// </summary>
    [JsonPropertyName("currentDate")]
    public DateWorkArea CurrentDate
    {
      get => currentDate ??= new();
      set => currentDate = value;
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
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    /// <summary>
    /// A value of NumberOfRecordsRead.
    /// </summary>
    [JsonPropertyName("numberOfRecordsRead")]
    public Common NumberOfRecordsRead
    {
      get => numberOfRecordsRead ??= new();
      set => numberOfRecordsRead = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
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
    /// A value of Year.
    /// </summary>
    [JsonPropertyName("year")]
    public TextWorkArea Year
    {
      get => year ??= new();
      set => year = value;
    }

    /// <summary>
    /// A value of Month.
    /// </summary>
    [JsonPropertyName("month")]
    public TextWorkArea Month
    {
      get => month ??= new();
      set => month = value;
    }

    /// <summary>
    /// A value of Day.
    /// </summary>
    [JsonPropertyName("day")]
    public TextWorkArea Day
    {
      get => day ??= new();
      set => day = value;
    }

    /// <summary>
    /// A value of PartOne.
    /// </summary>
    [JsonPropertyName("partOne")]
    public WorkArea PartOne
    {
      get => partOne ??= new();
      set => partOne = value;
    }

    private WorkArea ceDetailNumber;
    private WorkArea cashEventDetail;
    private WorkArea cashEvent;
    private WorkArea amount;
    private TextWorkArea taxYear;
    private DateWorkArea beginDate;
    private DateWorkArea endDate;
    private External write;
    private External external;
    private Common recordsFound;
    private DateWorkArea beginningOfPeriod;
    private Common changed;
    private WorkArea cruProcessDate;
    private WorkArea jfaReceivedDate;
    private DateWorkArea cruProcessDt;
    private DateWorkArea beginningOfTheMonth;
    private DateWorkArea zeroDate;
    private DateWorkArea jfaReceivedDt;
    private DateWorkArea currentDate;
    private WorkArea workArea;
    private AbendData abendData;
    private Common numberOfRecordsRead;
    private ExitStateWorkArea exitStateWorkArea;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private TextWorkArea year;
    private TextWorkArea month;
    private TextWorkArea day;
    private WorkArea partOne;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
    }

    /// <summary>
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
  }
#endregion
}
