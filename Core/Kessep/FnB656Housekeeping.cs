// Program: FN_B656_HOUSEKEEPING, ID: 372720555, model: 746.
// Short name: SWE02330
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B656_HOUSEKEEPING.
/// </summary>
[Serializable]
public partial class FnB656Housekeeping: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B656_HOUSEKEEPING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB656Housekeeping(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB656Housekeeping.
  /// </summary>
  public FnB656Housekeeping(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // **********************************************************
    // GET PROCESS DATE & OPTIONAL PARAMETERS
    // **********************************************************
    local.ProgramProcessingInfo.Name = "SWEFB656";
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "ZD_PROGRAM_PROCESSING_INFO_NF_AB";

      return;
    }

    local.EabReportSend.ProgramName = local.ProgramProcessingInfo.Name;
    local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;

    if (Equal(local.ProgramProcessingInfo.ProcessDate, local.Null1.Date))
    {
      export.PayTape.ProcessDate = Now().Date;
    }
    else
    {
      export.PayTape.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    }

    // : Parameter List is as follows:
    // X 999999999 999999999 999
    // 1-1   Run in Test Mode (Y/N)
    // 3-11  Starting Payment Request ID
    // 13-21 Ending Payment Request ID
    // 23-25 Max Print Lines Per Stub
    if (IsEmpty(local.ProgramProcessingInfo.ParameterList))
    {
      export.RunInTestMode.Flag = "N";
      export.Start.SystemGeneratedIdentifier = 0;
      export.End.SystemGeneratedIdentifier = 999999999;
      export.MaxPrintLinesPerStub.Count = 30;
    }
    else
    {
      export.RunInTestMode.Flag =
        Substring(local.ProgramProcessingInfo.ParameterList, 1, 1);

      if (IsEmpty(export.RunInTestMode.Flag))
      {
        export.RunInTestMode.Flag = "N";
      }

      local.Tmp.Text10 =
        Substring(local.ProgramProcessingInfo.ParameterList, 3, 9);
      export.Start.SystemGeneratedIdentifier =
        (int)StringToNumber(local.Tmp.Text10);
      local.Tmp.Text10 =
        Substring(local.ProgramProcessingInfo.ParameterList, 13, 9);
      export.End.SystemGeneratedIdentifier =
        (int)StringToNumber(local.Tmp.Text10);

      if (export.End.SystemGeneratedIdentifier == 0)
      {
        export.End.SystemGeneratedIdentifier = 999999999;
      }

      local.Tmp.Text10 =
        Substring(local.ProgramProcessingInfo.ParameterList, 23, 3);
      export.MaxPrintLinesPerStub.Count = (int)StringToNumber(local.Tmp.Text10);

      if (export.MaxPrintLinesPerStub.Count == 0)
      {
        export.MaxPrintLinesPerStub.Count = 30;
      }
    }

    UseFnExtGetParmsThruJclSysin();

    if (local.External.NumericReturnCode != 0)
    {
      ExitState = "FN0000_SYSIN_PARM_ERROR_A";

      return;
    }

    if (IsEmpty(local.ProgramProcessingInfo.ParameterList))
    {
      ExitState = "FN0000_BLANK_SYSIN_PARM_LIST_AB";

      return;
    }

    export.PayTape.VoucherNumber =
      Substring(local.ProgramProcessingInfo.ParameterList, 1, 5);

    if (IsEmpty(export.PayTape.VoucherNumber))
    {
      ExitState = "FN0000_INVALID_VOUCHER_NUMBER_A";

      return;
    }

    local.EabFileHandling.Action = "OPEN";

    // **********************************************************
    // OPEN OUTPUT CONTROL REPORT 98
    // **********************************************************
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.EabFileHandling.Action = "WRITE";
    local.NeededToWrite.RptDetail = "PARAMETERS:";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.NeededToWrite.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.NeededToWrite.RptDetail = "Voucher Number. . . . . . . . . . . : " + (
      export.PayTape.VoucherNumber ?? "");
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.NeededToWrite.RptDetail = "Starting Warrant ID . . . . . . . . : " + NumberToString
      (export.Start.SystemGeneratedIdentifier, 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.NeededToWrite.RptDetail = "Ending Warrant ID . . . . . . . . . : " + NumberToString
      (export.End.SystemGeneratedIdentifier, 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.NeededToWrite.RptDetail = "Run In Test Mode. . . . . . . . . . : " + export
      .RunInTestMode.Flag;
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.NeededToWrite.RptDetail = "Max Print Lines Per Stub. . . . . . : " + NumberToString
      (export.MaxPrintLinesPerStub.Count, 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.NeededToWrite.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    // **********************************************************
    // OPEN OUTPUT ERROR REPORT 99
    // **********************************************************
    local.EabFileHandling.Action = "OPEN";
    UseCabErrorReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_ERROR_RPT_A";

      return;
    }

    // **********************************************************
    // OPEN OUTPUT DAILY ERROR REPORT 01
    // **********************************************************
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.RptHeading3 =
      "    SRRUN186 - CREATE D OF A WARRANT FILE - DAILY ERROR REPORT";
    local.EabReportSend.BlankLineAfterHeading = "Y";
    UseCabBusinessReport01();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_ERROR_RPT_A";

      return;
    }

    local.DoaPayTape.FileInstruction = "OPEN";
    UseFnExtProcessDoaPayTape2();

    if (local.DoaPayTape.NumericReturnCode != 0)
    {
      ExitState = "FN0000_DOA_PAY_TAPE_OPEN_ERR_A";

      return;
    }

    local.DoaPayTape.FileInstruction = "WRITE";
    local.RecordType.Text4 = "1";
    local.FileId.Text4 = "521";
    UseFnExtProcessDoaPayTape1();

    if (local.DoaPayTape.NumericReturnCode != 0)
    {
      ExitState = "FN0000_DOA_PAY_TAPE_WRITE_ERR_A";
    }
  }

  private static void MoveEabReportSend1(EabReportSend source,
    EabReportSend target)
  {
    target.BlankLineAfterHeading = source.BlankLineAfterHeading;
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
    target.RptHeading3 = source.RptHeading3;
  }

  private static void MoveEabReportSend2(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MovePayTape(PayTape source, PayTape target)
  {
    target.ProcessDate = source.ProcessDate;
    target.VoucherNumber = source.VoucherNumber;
  }

  private void UseCabBusinessReport01()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend1(local.EabReportSend, useImport.NeededToOpen);

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend2(local.EabReportSend, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend2(local.EabReportSend, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnExtGetParmsThruJclSysin()
  {
    var useImport = new FnExtGetParmsThruJclSysin.Import();
    var useExport = new FnExtGetParmsThruJclSysin.Export();

    useExport.ProgramProcessingInfo.ParameterList =
      local.ProgramProcessingInfo.ParameterList;
    useExport.External.NumericReturnCode = local.External.NumericReturnCode;

    Call(FnExtGetParmsThruJclSysin.Execute, useImport, useExport);

    local.ProgramProcessingInfo.ParameterList =
      useExport.ProgramProcessingInfo.ParameterList;
    local.External.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseFnExtProcessDoaPayTape1()
  {
    var useImport = new FnExtProcessDoaPayTape.Import();
    var useExport = new FnExtProcessDoaPayTape.Export();

    useImport.RecordType.Text4 = local.RecordType.Text4;
    useImport.External.FileInstruction = local.DoaPayTape.FileInstruction;
    useImport.FileId.Text4 = local.FileId.Text4;
    MovePayTape(export.PayTape, useImport.PayTape);
    useExport.External.NumericReturnCode = local.DoaPayTape.NumericReturnCode;

    Call(FnExtProcessDoaPayTape.Execute, useImport, useExport);

    local.DoaPayTape.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseFnExtProcessDoaPayTape2()
  {
    var useImport = new FnExtProcessDoaPayTape.Import();
    var useExport = new FnExtProcessDoaPayTape.Export();

    useImport.External.FileInstruction = local.DoaPayTape.FileInstruction;
    useExport.External.NumericReturnCode = local.DoaPayTape.NumericReturnCode;

    Call(FnExtProcessDoaPayTape.Execute, useImport, useExport);

    local.DoaPayTape.NumericReturnCode = useExport.External.NumericReturnCode;
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
    /// A value of PayTape.
    /// </summary>
    [JsonPropertyName("payTape")]
    public PayTape PayTape
    {
      get => payTape ??= new();
      set => payTape = value;
    }

    /// <summary>
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    public PaymentRequest Start
    {
      get => start ??= new();
      set => start = value;
    }

    /// <summary>
    /// A value of End.
    /// </summary>
    [JsonPropertyName("end")]
    public PaymentRequest End
    {
      get => end ??= new();
      set => end = value;
    }

    /// <summary>
    /// A value of RunInTestMode.
    /// </summary>
    [JsonPropertyName("runInTestMode")]
    public Common RunInTestMode
    {
      get => runInTestMode ??= new();
      set => runInTestMode = value;
    }

    /// <summary>
    /// A value of MaxPrintLinesPerStub.
    /// </summary>
    [JsonPropertyName("maxPrintLinesPerStub")]
    public Common MaxPrintLinesPerStub
    {
      get => maxPrintLinesPerStub ??= new();
      set => maxPrintLinesPerStub = value;
    }

    private PayTape payTape;
    private PaymentRequest start;
    private PaymentRequest end;
    private Common runInTestMode;
    private Common maxPrintLinesPerStub;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of Tmp.
    /// </summary>
    [JsonPropertyName("tmp")]
    public TextWorkArea Tmp
    {
      get => tmp ??= new();
      set => tmp = value;
    }

    /// <summary>
    /// A value of RecordType.
    /// </summary>
    [JsonPropertyName("recordType")]
    public TextWorkArea RecordType
    {
      get => recordType ??= new();
      set => recordType = value;
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
    /// A value of DoaPayTape.
    /// </summary>
    [JsonPropertyName("doaPayTape")]
    public External DoaPayTape
    {
      get => doaPayTape ??= new();
      set => doaPayTape = value;
    }

    /// <summary>
    /// A value of FileId.
    /// </summary>
    [JsonPropertyName("fileId")]
    public TextWorkArea FileId
    {
      get => fileId ??= new();
      set => fileId = value;
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
    /// A value of VoucherNo.
    /// </summary>
    [JsonPropertyName("voucherNo")]
    public PayTape VoucherNo
    {
      get => voucherNo ??= new();
      set => voucherNo = value;
    }

    /// <summary>
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public EabReportSend Temp
    {
      get => temp ??= new();
      set => temp = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    private EabReportSend neededToWrite;
    private TextWorkArea tmp;
    private TextWorkArea recordType;
    private External external;
    private External doaPayTape;
    private TextWorkArea fileId;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private PayTape voucherNo;
    private EabReportSend temp;
    private DateWorkArea null1;
  }
#endregion
}
