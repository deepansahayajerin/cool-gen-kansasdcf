// Program: FN_B643_HOUSEKEEPING, ID: 372683786, model: 746.
// Short name: SWE02383
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B643_HOUSEKEEPING.
/// </summary>
[Serializable]
public partial class FnB643Housekeeping: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B643_HOUSEKEEPING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB643Housekeeping(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB643Housekeeping.
  /// </summary>
  public FnB643Housekeeping(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    export.VendorFileRecordCount.Count = 1;
    export.SortSequenceNumber.Count = 1;

    // **********************************************************
    // GET PROCESS DATE & OPTIONAL PARAMETERS
    // **********************************************************
    local.ProgramProcessingInfo.Name = "SWEFB643";

    if (ReadProgramProcessingInfo())
    {
      export.Process.Date = entities.ProgramProcessingInfo.ProcessDate;
      local.PpiFound.Flag = "Y";
    }

    if (AsChar(local.PpiFound.Flag) != 'Y')
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    if (IsEmpty(entities.ProgramProcessingInfo.ParameterList))
    {
      // **********************************************************
      // CALCULATE STATEMENT BEGIN DATE
      // If PPI Process_Date = 11/15/1997, then statement date = 10/31/1997
      // Only activities will be listed from 10/01/97 to 10/31/97
      // Coupons will be produced for due dates between 12/01/97 and 12/31/97
      // **********************************************************
      local.ProcessMonth.Count =
        Month(entities.ProgramProcessingInfo.ProcessDate);
      local.ProcessYear.Count =
        Year(entities.ProgramProcessingInfo.ProcessDate);
      export.BeginStmt.Date = IntToDate((int)((long)local.ProcessYear.Count * 10000
        + (long)local.ProcessMonth.Count * 100 + 1));
      export.BeginStmt.Date = AddMonths(export.BeginStmt.Date, -1);
      export.BeginStmt.Timestamp =
        Timestamp(NumberToString(Year(export.BeginStmt.Date), 12, 4) + "-" + NumberToString
        (Month(export.BeginStmt.Date), 14, 2) + "-" + NumberToString
        (Day(export.BeginStmt.Date), 14, 2) + "-00.00.00.000001");
      export.StatementBegin.Text10 =
        NumberToString(Month(export.BeginStmt.Date), 14, 2) + "/" + NumberToString
        (Day(export.BeginStmt.Date), 14, 2) + "/" + NumberToString
        (Year(export.BeginStmt.Date), 12, 4);

      // **********************************************************
      // CALCULATE STATEMENT END DATE
      // **********************************************************
      export.EndStmt.Date = AddMonths(export.BeginStmt.Date, 1);
      export.EndStmt.Date = AddDays(export.EndStmt.Date, -1);
      export.EndStmt.Timestamp =
        Timestamp(NumberToString(Year(export.EndStmt.Date), 12, 4) + "-" + NumberToString
        (Month(export.EndStmt.Date), 14, 2) + "-" + NumberToString
        (Day(export.EndStmt.Date), 14, 2) + "-23.59.59.999999");
      export.StatementDate.Text10 =
        NumberToString(Month(export.EndStmt.Date), 14, 2) + "/" + NumberToString
        (Day(export.EndStmt.Date), 14, 2) + "/" + NumberToString
        (Year(export.EndStmt.Date), 12, 4);
      export.StatementEnd.Text10 =
        NumberToString(Month(export.EndStmt.Date), 14, 2) + "/" + NumberToString
        (Day(export.EndStmt.Date), 14, 2) + "/" + NumberToString
        (Year(export.EndStmt.Date), 12, 4);

      // **********************************************************
      // CALCULATE STATEMENT MONTH YEAR
      // **********************************************************
      local.StmtReportingYear.Count = Year(export.EndStmt.Date);
      local.StmtReportingMonth.Count = Month(export.EndStmt.Date);

      switch(local.StmtReportingMonth.Count)
      {
        case 1:
          export.StmtReportingMonthYear.Text30 = "January " + NumberToString
            (local.StmtReportingYear.Count, 12, 4);

          break;
        case 2:
          export.StmtReportingMonthYear.Text30 = "February " + NumberToString
            (local.StmtReportingYear.Count, 12, 4);

          break;
        case 3:
          export.StmtReportingMonthYear.Text30 = "March " + NumberToString
            (local.StmtReportingYear.Count, 12, 4);

          break;
        case 4:
          export.StmtReportingMonthYear.Text30 = "April " + NumberToString
            (local.StmtReportingYear.Count, 12, 4);

          break;
        case 5:
          export.StmtReportingMonthYear.Text30 = "May " + NumberToString
            (local.StmtReportingYear.Count, 12, 4);

          break;
        case 6:
          export.StmtReportingMonthYear.Text30 = "June " + NumberToString
            (local.StmtReportingYear.Count, 12, 4);

          break;
        case 7:
          export.StmtReportingMonthYear.Text30 = "July " + NumberToString
            (local.StmtReportingYear.Count, 12, 4);

          break;
        case 8:
          export.StmtReportingMonthYear.Text30 = "August " + NumberToString
            (local.StmtReportingYear.Count, 12, 4);

          break;
        case 9:
          export.StmtReportingMonthYear.Text30 = "September " + NumberToString
            (local.StmtReportingYear.Count, 12, 4);

          break;
        case 10:
          export.StmtReportingMonthYear.Text30 = "October " + NumberToString
            (local.StmtReportingYear.Count, 12, 4);

          break;
        case 11:
          export.StmtReportingMonthYear.Text30 = "November " + NumberToString
            (local.StmtReportingYear.Count, 12, 4);

          break;
        case 12:
          export.StmtReportingMonthYear.Text30 = "December " + NumberToString
            (local.StmtReportingYear.Count, 12, 4);

          break;
        default:
          export.StmtReportingMonthYear.Text30 = "Unknown " + NumberToString
            (local.StmtReportingYear.Count, 12, 4);

          break;
      }

      // **********************************************************
      // CALCULATE OBLIGOR SUMMARY BEGIN DATE
      // **********************************************************
      local.PreviousMonth.Date = AddDays(export.BeginStmt.Date, -1);
      export.Beginning.YearMonth = Year(local.PreviousMonth.Date) * 100 + Month
        (local.PreviousMonth.Date);

      // **********************************************************
      // CALCULATE OBLIGOR SUMMARY END DATE
      // **********************************************************
      export.Ending.YearMonth = Year(export.EndStmt.Date) * 100 + Month
        (export.EndStmt.Date);

      // **********************************************************
      // CALCULATE COUPON BEGIN AND END DATES
      // **********************************************************
      export.BeginCoupon.Date = AddMonths(export.BeginStmt.Date, 2);
      export.EndCoupon.Date = AddMonths(export.BeginCoupon.Date, 1);
      export.EndCoupon.Date = AddDays(export.EndCoupon.Date, -1);

      // **********************************************************
      // CALCULATE COUPON MONTH YEAR
      // **********************************************************
      local.StmtReportingYear.Count = Year(export.EndCoupon.Date);
      local.StmtReportingMonth.Count = Month(export.EndCoupon.Date);

      switch(local.StmtReportingMonth.Count)
      {
        case 1:
          export.CpnReportingMonthYear.Text30 = "January " + NumberToString
            (local.StmtReportingYear.Count, 12, 4);

          break;
        case 2:
          export.CpnReportingMonthYear.Text30 = "February " + NumberToString
            (local.StmtReportingYear.Count, 12, 4);

          break;
        case 3:
          export.CpnReportingMonthYear.Text30 = "March " + NumberToString
            (local.StmtReportingYear.Count, 12, 4);

          break;
        case 4:
          export.CpnReportingMonthYear.Text30 = "April " + NumberToString
            (local.StmtReportingYear.Count, 12, 4);

          break;
        case 5:
          export.CpnReportingMonthYear.Text30 = "May " + NumberToString
            (local.StmtReportingYear.Count, 12, 4);

          break;
        case 6:
          export.CpnReportingMonthYear.Text30 = "June " + NumberToString
            (local.StmtReportingYear.Count, 12, 4);

          break;
        case 7:
          export.CpnReportingMonthYear.Text30 = "July " + NumberToString
            (local.StmtReportingYear.Count, 12, 4);

          break;
        case 8:
          export.CpnReportingMonthYear.Text30 = "August " + NumberToString
            (local.StmtReportingYear.Count, 12, 4);

          break;
        case 9:
          export.CpnReportingMonthYear.Text30 = "September " + NumberToString
            (local.StmtReportingYear.Count, 12, 4);

          break;
        case 10:
          export.CpnReportingMonthYear.Text30 = "October " + NumberToString
            (local.StmtReportingYear.Count, 12, 4);

          break;
        case 11:
          export.CpnReportingMonthYear.Text30 = "November " + NumberToString
            (local.StmtReportingYear.Count, 12, 4);

          break;
        case 12:
          export.CpnReportingMonthYear.Text30 = "December " + NumberToString
            (local.StmtReportingYear.Count, 12, 4);

          break;
        default:
          export.CpnReportingMonthYear.Text30 = "Unknown " + NumberToString
            (local.StmtReportingYear.Count, 12, 4);

          break;
      }
    }
    else
    {
      // **********************************************************
      // EXTRACT STMT BEGIN AND END DATES FROM PARMLIST AND VALIDATE
      // **********************************************************
    }

    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = export.Process.Date;
    local.EabReportSend.ProgramName = entities.ProgramProcessingInfo.Name;

    // **********************************************************
    // OPEN OUTPUT ERROR REPORT 99
    // **********************************************************
    UseCabErrorReport2();

    if (!Equal(export.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // **********************************************************
    // OPEN OUTPUT CONTROL REPORT 98
    // **********************************************************
    UseCabControlReport2();

    if (!Equal(export.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // **********************************************************
    // OPEN OUTPUT STATEMENT EXTRACT FILE TO BE SENT TO VENDOR
    // **********************************************************
    UseEabCreateVendorFile2();

    if (!Equal(export.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // **********************************************************
    // WRITE FILE HEADER TO STATEMENT EXTRACT FILE
    // **********************************************************
    local.EabFileHandling.Action = "WRITE";
    local.RecordType.ActionEntry = "00";
    export.ProcessDate.Text10 =
      NumberToString(Month(export.Process.Date), 14, 2) + "/" + NumberToString
      (Day(export.Process.Date), 14, 2) + "/" + NumberToString
      (Year(export.Process.Date), 12, 4);
    local.DateWorkArea.Time = Time(Now());
    local.ProcessTime.Text10 =
      NumberToString(local.DateWorkArea.Time.Hours, 14, 2) + NumberToString
      (local.DateWorkArea.Time.Minutes, 14, 2) + NumberToString
      (local.DateWorkArea.Time.Seconds, 14, 2);
    UseEabCreateVendorFile1();

    if (!Equal(export.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // **********************************************************
    // WRITE STATEMENT & COUPON DATE RANGES TO CONTROL REPORT 98
    // **********************************************************
    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail = "Statement Date Range:" + NumberToString
      (Month(export.BeginStmt.Date), 14, 2) + "/" + NumberToString
      (Day(export.BeginStmt.Date), 14, 2) + "/" + NumberToString
      (Year(export.BeginStmt.Date), 12, 4) + " thru " + NumberToString
      (Month(export.EndStmt.Date), 14, 2) + "/" + NumberToString
      (Day(export.EndStmt.Date), 14, 2) + "/" + NumberToString
      (Year(export.EndStmt.Date), 12, 4);
    UseCabControlReport1();

    if (!Equal(export.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(export.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "Coupon Date Range:" + NumberToString
      (Month(export.BeginCoupon.Date), 14, 2) + "/" + NumberToString
      (Day(export.BeginCoupon.Date), 14, 2) + "/" + NumberToString
      (Year(export.BeginCoupon.Date), 12, 4) + " thru " + NumberToString
      (Month(export.EndCoupon.Date), 14, 2) + "/" + NumberToString
      (Day(export.EndCoupon.Date), 14, 2) + "/" + NumberToString
      (Year(export.EndCoupon.Date), 12, 4);
    UseCabControlReport1();

    if (!Equal(export.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(export.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // **********************************************************
    // The Global Statement Message has an effective month = 12
    // and an  effective year = 2099.  Other statement messages
    // are obligor specific.
    // **********************************************************
    if (ReadGlobalStatementMessage())
    {
      export.GlobalStatementMessage.TextArea =
        entities.GlobalStatementMessage.TextArea;
    }
    else
    {
      local.EabReportSend.RptDetail =
        "Global Statement Message was not found.  Entry in needed in table for 12/2099.";
        
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // **********************************************************
    // Obtain Hardcoded Values
    // **********************************************************
    UseFnB643Hardcode();
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabCreateVendorFile1()
  {
    var useImport = new EabCreateVendorFile.Import();
    var useExport = new EabCreateVendorFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.RecordType.ActionEntry = local.RecordType.ActionEntry;
    useImport.DateOne.Text10 = export.ProcessDate.Text10;
    useImport.DateTwo.Text10 = local.ProcessTime.Text10;
    useExport.EabFileHandling.Status = export.EabFileHandling.Status;

    Call(EabCreateVendorFile.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabCreateVendorFile2()
  {
    var useImport = new EabCreateVendorFile.Import();
    var useExport = new EabCreateVendorFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = export.EabFileHandling.Status;

    Call(EabCreateVendorFile.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnB643Hardcode()
  {
    var useImport = new FnB643Hardcode.Import();
    var useExport = new FnB643Hardcode.Export();

    Call(FnB643Hardcode.Execute, useImport, useExport);

    export.County.Id = useExport.County.Id;
    export.FdirPmt.SystemGeneratedIdentifier =
      useExport.FdirPmt.SystemGeneratedIdentifier;
    export.FcrtRec.SystemGeneratedIdentifier =
      useExport.FcrtRec.SystemGeneratedIdentifier;
    export.Iwo.SequentialIdentifier =
      useExport.CollectionType.SequentialIdentifier;
  }

  private bool ReadGlobalStatementMessage()
  {
    entities.GlobalStatementMessage.Populated = false;

    return Read("ReadGlobalStatementMessage",
      null,
      (db, reader) =>
      {
        entities.GlobalStatementMessage.EffectiveMonth = db.GetInt32(reader, 0);
        entities.GlobalStatementMessage.EffectiveYear = db.GetInt32(reader, 1);
        entities.GlobalStatementMessage.TextArea =
          db.GetNullableString(reader, 2);
        entities.GlobalStatementMessage.Populated = true;
      });
  }

  private bool ReadProgramProcessingInfo()
  {
    entities.ProgramProcessingInfo.Populated = false;

    return Read("ReadProgramProcessingInfo",
      (db, command) =>
      {
        db.SetString(command, "name", local.ProgramProcessingInfo.Name);
      },
      (db, reader) =>
      {
        entities.ProgramProcessingInfo.Name = db.GetString(reader, 0);
        entities.ProgramProcessingInfo.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.ProgramProcessingInfo.ProcessDate =
          db.GetNullableDate(reader, 2);
        entities.ProgramProcessingInfo.ParameterList =
          db.GetNullableString(reader, 3);
        entities.ProgramProcessingInfo.Populated = true;
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
    /// <summary>
    /// A value of FdirPmt.
    /// </summary>
    [JsonPropertyName("fdirPmt")]
    public CashReceiptType FdirPmt
    {
      get => fdirPmt ??= new();
      set => fdirPmt = value;
    }

    /// <summary>
    /// A value of FcrtRec.
    /// </summary>
    [JsonPropertyName("fcrtRec")]
    public CashReceiptType FcrtRec
    {
      get => fcrtRec ??= new();
      set => fcrtRec = value;
    }

    /// <summary>
    /// A value of Iwo.
    /// </summary>
    [JsonPropertyName("iwo")]
    public CollectionType Iwo
    {
      get => iwo ??= new();
      set => iwo = value;
    }

    /// <summary>
    /// A value of County.
    /// </summary>
    [JsonPropertyName("county")]
    public Code County
    {
      get => county ??= new();
      set => county = value;
    }

    /// <summary>
    /// A value of GlobalStatementMessage.
    /// </summary>
    [JsonPropertyName("globalStatementMessage")]
    public GlobalStatementMessage GlobalStatementMessage
    {
      get => globalStatementMessage ??= new();
      set => globalStatementMessage = value;
    }

    /// <summary>
    /// A value of StatementBegin.
    /// </summary>
    [JsonPropertyName("statementBegin")]
    public TextWorkArea StatementBegin
    {
      get => statementBegin ??= new();
      set => statementBegin = value;
    }

    /// <summary>
    /// A value of StatementEnd.
    /// </summary>
    [JsonPropertyName("statementEnd")]
    public TextWorkArea StatementEnd
    {
      get => statementEnd ??= new();
      set => statementEnd = value;
    }

    /// <summary>
    /// A value of StmtReportingMonthYear.
    /// </summary>
    [JsonPropertyName("stmtReportingMonthYear")]
    public TextWorkArea StmtReportingMonthYear
    {
      get => stmtReportingMonthYear ??= new();
      set => stmtReportingMonthYear = value;
    }

    /// <summary>
    /// A value of CpnReportingMonthYear.
    /// </summary>
    [JsonPropertyName("cpnReportingMonthYear")]
    public TextWorkArea CpnReportingMonthYear
    {
      get => cpnReportingMonthYear ??= new();
      set => cpnReportingMonthYear = value;
    }

    /// <summary>
    /// A value of StatementDate.
    /// </summary>
    [JsonPropertyName("statementDate")]
    public TextWorkArea StatementDate
    {
      get => statementDate ??= new();
      set => statementDate = value;
    }

    /// <summary>
    /// A value of Beginning.
    /// </summary>
    [JsonPropertyName("beginning")]
    public MonthlyObligorSummary Beginning
    {
      get => beginning ??= new();
      set => beginning = value;
    }

    /// <summary>
    /// A value of Ending.
    /// </summary>
    [JsonPropertyName("ending")]
    public MonthlyObligorSummary Ending
    {
      get => ending ??= new();
      set => ending = value;
    }

    /// <summary>
    /// A value of VendorFileRecordCount.
    /// </summary>
    [JsonPropertyName("vendorFileRecordCount")]
    public Common VendorFileRecordCount
    {
      get => vendorFileRecordCount ??= new();
      set => vendorFileRecordCount = value;
    }

    /// <summary>
    /// A value of SortSequenceNumber.
    /// </summary>
    [JsonPropertyName("sortSequenceNumber")]
    public Common SortSequenceNumber
    {
      get => sortSequenceNumber ??= new();
      set => sortSequenceNumber = value;
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
    /// A value of Process.
    /// </summary>
    [JsonPropertyName("process")]
    public DateWorkArea Process
    {
      get => process ??= new();
      set => process = value;
    }

    /// <summary>
    /// A value of ProcessDate.
    /// </summary>
    [JsonPropertyName("processDate")]
    public TextWorkArea ProcessDate
    {
      get => processDate ??= new();
      set => processDate = value;
    }

    /// <summary>
    /// A value of BeginStmt.
    /// </summary>
    [JsonPropertyName("beginStmt")]
    public DateWorkArea BeginStmt
    {
      get => beginStmt ??= new();
      set => beginStmt = value;
    }

    /// <summary>
    /// A value of EndStmt.
    /// </summary>
    [JsonPropertyName("endStmt")]
    public DateWorkArea EndStmt
    {
      get => endStmt ??= new();
      set => endStmt = value;
    }

    /// <summary>
    /// A value of BeginCoupon.
    /// </summary>
    [JsonPropertyName("beginCoupon")]
    public DateWorkArea BeginCoupon
    {
      get => beginCoupon ??= new();
      set => beginCoupon = value;
    }

    /// <summary>
    /// A value of EndCoupon.
    /// </summary>
    [JsonPropertyName("endCoupon")]
    public DateWorkArea EndCoupon
    {
      get => endCoupon ??= new();
      set => endCoupon = value;
    }

    private CashReceiptType fdirPmt;
    private CashReceiptType fcrtRec;
    private CollectionType iwo;
    private Code county;
    private GlobalStatementMessage globalStatementMessage;
    private TextWorkArea statementBegin;
    private TextWorkArea statementEnd;
    private TextWorkArea stmtReportingMonthYear;
    private TextWorkArea cpnReportingMonthYear;
    private TextWorkArea statementDate;
    private MonthlyObligorSummary beginning;
    private MonthlyObligorSummary ending;
    private Common vendorFileRecordCount;
    private Common sortSequenceNumber;
    private EabFileHandling eabFileHandling;
    private DateWorkArea process;
    private TextWorkArea processDate;
    private DateWorkArea beginStmt;
    private DateWorkArea endStmt;
    private DateWorkArea beginCoupon;
    private DateWorkArea endCoupon;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of ProcessTime.
    /// </summary>
    [JsonPropertyName("processTime")]
    public TextWorkArea ProcessTime
    {
      get => processTime ??= new();
      set => processTime = value;
    }

    /// <summary>
    /// A value of PpiFound.
    /// </summary>
    [JsonPropertyName("ppiFound")]
    public Common PpiFound
    {
      get => ppiFound ??= new();
      set => ppiFound = value;
    }

    /// <summary>
    /// A value of PreviousMonth.
    /// </summary>
    [JsonPropertyName("previousMonth")]
    public DateWorkArea PreviousMonth
    {
      get => previousMonth ??= new();
      set => previousMonth = value;
    }

    /// <summary>
    /// A value of RecordType.
    /// </summary>
    [JsonPropertyName("recordType")]
    public Common RecordType
    {
      get => recordType ??= new();
      set => recordType = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of ProcessMonth.
    /// </summary>
    [JsonPropertyName("processMonth")]
    public Common ProcessMonth
    {
      get => processMonth ??= new();
      set => processMonth = value;
    }

    /// <summary>
    /// A value of ProcessYear.
    /// </summary>
    [JsonPropertyName("processYear")]
    public Common ProcessYear
    {
      get => processYear ??= new();
      set => processYear = value;
    }

    /// <summary>
    /// A value of StmtReportingMonth.
    /// </summary>
    [JsonPropertyName("stmtReportingMonth")]
    public Common StmtReportingMonth
    {
      get => stmtReportingMonth ??= new();
      set => stmtReportingMonth = value;
    }

    /// <summary>
    /// A value of StmtReportingYear.
    /// </summary>
    [JsonPropertyName("stmtReportingYear")]
    public Common StmtReportingYear
    {
      get => stmtReportingYear ??= new();
      set => stmtReportingYear = value;
    }

    private DateWorkArea dateWorkArea;
    private TextWorkArea processTime;
    private Common ppiFound;
    private DateWorkArea previousMonth;
    private Common recordType;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private ProgramProcessingInfo programProcessingInfo;
    private Common processMonth;
    private Common processYear;
    private Common stmtReportingMonth;
    private Common stmtReportingYear;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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

    /// <summary>
    /// A value of GlobalStatementMessage.
    /// </summary>
    [JsonPropertyName("globalStatementMessage")]
    public GlobalStatementMessage GlobalStatementMessage
    {
      get => globalStatementMessage ??= new();
      set => globalStatementMessage = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
    private GlobalStatementMessage globalStatementMessage;
  }
#endregion
}
