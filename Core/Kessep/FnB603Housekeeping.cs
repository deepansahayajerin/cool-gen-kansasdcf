// Program: FN_B603_HOUSEKEEPING, ID: 945058036, model: 746.
// Short name: SWE03132
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B603_HOUSEKEEPING.
/// </summary>
[Serializable]
public partial class FnB603Housekeeping: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B603_HOUSEKEEPING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB603Housekeeping(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB603Housekeeping.
  /// </summary>
  public FnB603Housekeeping(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // Date		Developer	Request		Desc
    // ----------------------------------------------------------------------------------------------------
    // 12/09/2010	J Huss		CQ# 9690	Initial Development
    // 01/24/2010	Raj S		CQ# 9690	Modified to prefix the record type value "1" 
    // to
    //                                                 
    // represent that record is a Header Record
    // ----------------------------------------------------------------------------------------------------
    // *********************************************************************
    // This action determines the processing details for this run, opens the 
    // extract
    // file and control report, writes the processing date to the extract file, 
    // and
    // writes the column headers to the control report.
    // *********************************************************************
    local.ProgramProcessingInfo.Name = global.UserId;

    // Read the processing details for this run
    UseReadProgramProcessingInfo();

    if (IsExitState("ZD_PROGRAM_PROCESSING_INFO_NF_AB"))
    {
      return;
    }

    export.LastRunDate.Date =
      StringToDate(local.ProgramProcessingInfo.ParameterList);
    export.ProcessingDate.Date = local.ProgramProcessingInfo.ProcessDate;
    local.NeededToOpen.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    local.NeededToOpen.ProgramName = local.ProgramProcessingInfo.Name;
    local.EabFileHandling.Action = "OPEN";

    // Open the control report
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_OPENING_EXT_FILE";

      return;
    }

    // Open the error report
    UseCabErrorReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_OPENING_EXT_FILE";

      return;
    }

    // Open the extract file
    UseFnB603EabExtractFile1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_OPENING_EXT_FILE";

      return;
    }

    local.EabFileHandling.Action = "WRITE";

    // Write the run date to the extract file
    local.NeededToWrite.RptDetail =
      NumberToString(DateToInt(local.ProgramProcessingInfo.ProcessDate), 8, 8);

    // Since this is a header record Prefix Record Type "1" before the record 
    // before Write to the extract file
    local.NeededToWrite.RptDetail = "1" + TrimEnd
      (local.NeededToWrite.RptDetail);
    UseFnB603EabExtractFile2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ERROR_WRITING_TO_FILE_AB";
    }
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnB603EabExtractFile1()
  {
    var useImport = new FnB603EabExtractFile.Import();
    var useExport = new FnB603EabExtractFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(FnB603EabExtractFile.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnB603EabExtractFile2()
  {
    var useImport = new FnB603EabExtractFile.Import();
    var useExport = new FnB603EabExtractFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.EabReportSend.RptDetail = local.NeededToWrite.RptDetail;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(FnB603EabExtractFile.Execute, useImport, useExport);

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
    /// A value of LastRunDate.
    /// </summary>
    [JsonPropertyName("lastRunDate")]
    public DateWorkArea LastRunDate
    {
      get => lastRunDate ??= new();
      set => lastRunDate = value;
    }

    /// <summary>
    /// A value of ProcessingDate.
    /// </summary>
    [JsonPropertyName("processingDate")]
    public DateWorkArea ProcessingDate
    {
      get => processingDate ??= new();
      set => processingDate = value;
    }

    private DateWorkArea lastRunDate;
    private DateWorkArea processingDate;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of NeededToOpen.
    /// </summary>
    [JsonPropertyName("neededToOpen")]
    public EabReportSend NeededToOpen
    {
      get => neededToOpen ??= new();
      set => neededToOpen = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    private EabFileHandling eabFileHandling;
    private EabReportSend neededToOpen;
    private EabReportSend neededToWrite;
    private ProgramProcessingInfo programProcessingInfo;
  }
#endregion
}
