// Program: FN_B603_WRITE_CONTROLS_AND_CLOSE, ID: 945058078, model: 746.
// Short name: SWE03133
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B603_WRITE_CONTROLS_AND_CLOSE.
/// </summary>
[Serializable]
public partial class FnB603WriteControlsAndClose: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B603_WRITE_CONTROLS_AND_CLOSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB603WriteControlsAndClose(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB603WriteControlsAndClose.
  /// </summary>
  public FnB603WriteControlsAndClose(IContext context, Import import,
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
    // Date		Developer	Request		Desc
    // ----------------------------------------------------------------------------------------------------
    // 12/09/2010	J Huss		CQ# 9690	Initial Development
    // 01/24/2010	Raj S		CQ# 9690	Modified to prefix the record type value "9" 
    // to
    //                                                 
    // represent that record is a Footer Record
    // ----------------------------------------------------------------------------------------------------
    // *********************************************************************
    // This action block updates the PPI record in preparation for the next
    // processing cycle, writes totals to both the extract file and the control
    // report, and closes both the file and the report.
    // *********************************************************************
    // Format the processing date as YYYY-MM-DD
    local.Year.Text4 =
      NumberToString(DateToInt(import.ProcessingDate.Date), 8, 4);
    local.Month.Text2 =
      NumberToString(DateToInt(import.ProcessingDate.Date), 12, 2);
    local.Day.Text2 =
      NumberToString(DateToInt(import.ProcessingDate.Date), 14, 2);
    local.Date.Text10 = local.Year.Text4 + "-" + local.Month.Text2 + "-" + local
      .Day.Text2;
    local.ProgramProcessingInfo.Name = global.UserId;
    local.ProgramProcessingInfo.ParameterList = local.Date.Text10;
    local.ProgramProcessingInfo.ProcessDate = import.ProcessingDate.Date;

    // Update the PPI parameter with the current processing date
    UseUpdateProgramProcessingInfo();
    local.EabFileHandling.Action = "WRITE";

    // Write the record counts to the extract file
    local.EabReportSend.RptDetail = NumberToString(import.Add.Count, 15) + NumberToString
      (import.Remove.Count, 15) + NumberToString(import.Total.Count, 15);

    // Since this is a Footer record Prefix Record Type "9" before the record 
    // before Write to the extract file
    local.EabReportSend.RptDetail = "9" + TrimEnd
      (local.EabReportSend.RptDetail);
    UseFnB603EabExtractFile();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ERROR_WRITING_TO_FILE_AB";

      return;
    }

    // Add a blank line to the control report
    local.EabReportSend.RptDetail = "";
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ERROR_WRITING_TO_REPORT_AB";

      return;
    }

    // Write the record counts to the control report
    for(local.Loop.Count = 1; local.Loop.Count <= 3; ++local.Loop.Count)
    {
      switch(local.Loop.Count)
      {
        case 1:
          local.EabReportSend.RptDetail = "Total adds:     " + NumberToString
            (import.Add.Count, 15);

          break;
        case 2:
          local.EabReportSend.RptDetail = "Total removes:  " + NumberToString
            (import.Remove.Count, 15);

          break;
        case 3:
          local.EabReportSend.RptDetail = "Total records:  " + NumberToString
            (import.Total.Count, 15);

          break;
        default:
          break;
      }

      UseCabControlReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ERROR_WRITING_TO_REPORT_AB";

        return;
      }
    }

    // Close the extract file
    local.EabFileHandling.Action = "CLOSE";
    UseFnB603EabExtractFile();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ERROR_CLOSING_FILE_AB";

      return;
    }

    // Close the control report
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ERROR_CLOSING_FILE_AB";

      return;
    }

    // Close the error report
    UseCabErrorReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ERROR_CLOSING_FILE_AB";
    }
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
    target.ParameterList = source.ParameterList;
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnB603EabExtractFile()
  {
    var useImport = new FnB603EabExtractFile.Import();
    var useExport = new FnB603EabExtractFile.Export();

    useImport.EabReportSend.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(FnB603EabExtractFile.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseUpdateProgramProcessingInfo()
  {
    var useImport = new UpdateProgramProcessingInfo.Import();
    var useExport = new UpdateProgramProcessingInfo.Export();

    MoveProgramProcessingInfo(local.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);

    Call(UpdateProgramProcessingInfo.Execute, useImport, useExport);
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
    /// <summary>
    /// A value of Total.
    /// </summary>
    [JsonPropertyName("total")]
    public Common Total
    {
      get => total ??= new();
      set => total = value;
    }

    /// <summary>
    /// A value of Add.
    /// </summary>
    [JsonPropertyName("add")]
    public Common Add
    {
      get => add ??= new();
      set => add = value;
    }

    /// <summary>
    /// A value of Remove.
    /// </summary>
    [JsonPropertyName("remove")]
    public Common Remove
    {
      get => remove ??= new();
      set => remove = value;
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

    private Common total;
    private Common add;
    private Common remove;
    private DateWorkArea processingDate;
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
    /// A value of Date.
    /// </summary>
    [JsonPropertyName("date")]
    public WorkArea Date
    {
      get => date ??= new();
      set => date = value;
    }

    /// <summary>
    /// A value of Day.
    /// </summary>
    [JsonPropertyName("day")]
    public WorkArea Day
    {
      get => day ??= new();
      set => day = value;
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
    /// A value of Loop.
    /// </summary>
    [JsonPropertyName("loop")]
    public Common Loop
    {
      get => loop ??= new();
      set => loop = value;
    }

    /// <summary>
    /// A value of Month.
    /// </summary>
    [JsonPropertyName("month")]
    public WorkArea Month
    {
      get => month ??= new();
      set => month = value;
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
    /// A value of Year.
    /// </summary>
    [JsonPropertyName("year")]
    public WorkArea Year
    {
      get => year ??= new();
      set => year = value;
    }

    private WorkArea date;
    private WorkArea day;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private Common loop;
    private WorkArea month;
    private ProgramProcessingInfo programProcessingInfo;
    private WorkArea year;
  }
#endregion
}
