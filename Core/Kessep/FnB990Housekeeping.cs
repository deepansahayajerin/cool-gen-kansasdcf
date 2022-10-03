// Program: FN_B990_HOUSEKEEPING, ID: 371033404, model: 746.
// Short name: SWE00711
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B990_HOUSEKEEPING.
/// </summary>
[Serializable]
public partial class FnB990Housekeeping: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B990_HOUSEKEEPING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB990Housekeeping(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB990Housekeeping.
  /// </summary>
  public FnB990Housekeeping(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    ExitState = "ACO_NN0000_ALL_OK";
    export.Max.Date = UseCabSetMaximumDiscontinueDate();
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    export.FileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = export.ProgramProcessingInfo.ProcessDate;
    local.EabReportSend.ProgramName = export.ProgramProcessingInfo.Name;

    // **********************************************************
    // OPEN OUTPUT ERROR REPORT 99
    // **********************************************************
    UseCabErrorReport();

    if (!Equal(export.FileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // **********************************************************
    // OPEN OUTPUT CONTROL REPORT 98
    // **********************************************************
    UseCabControlReport2();

    if (!Equal(export.FileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.BlankLineAfterHeading = "Y";
    UseCabBusinessReport01();

    if (!Equal(export.FileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // **********************************************************
    // LOOK AT PPI PARMLIST TO SEE IF A SAMPLING FREQUENCY WAS SUPPLIED
    // **********************************************************
    if (Lt("0000", Substring(export.ProgramProcessingInfo.ParameterList, 1, 4)) &&
      !Lt("9999", Substring(export.ProgramProcessingInfo.ParameterList, 1, 4)))
    {
      export.SamplingFrequencyCommon.Count =
        (int)StringToNumber(Substring(
          export.ProgramProcessingInfo.ParameterList, 1, 4));
      export.SamplingFrequencyTextWorkArea.Text4 =
        Substring(export.ProgramProcessingInfo.ParameterList, 1, 4);

      // **********************************************************
      // WRITE TO CONTROL REPORT 98
      // **********************************************************
      export.FileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Using the requested sampling frequency of: " + export
        .SamplingFrequencyTextWorkArea.Text4 + "   ";
      UseCabControlReport1();

      if (!Equal(export.FileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.EabReportSend.RptDetail = "";
      UseCabControlReport1();

      if (!Equal(export.FileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }
    else
    {
      export.SamplingFrequencyCommon.Count = 72;
      export.SamplingFrequencyTextWorkArea.Text4 = "0072";

      // **********************************************************
      // WRITE TO CONTROL REPORT 98
      // **********************************************************
      export.FileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Using the default sampling frequency of: " + export
        .SamplingFrequencyTextWorkArea.Text4 + "   ";
      UseCabControlReport1();

      if (!Equal(export.FileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.EabReportSend.RptDetail = "";
      UseCabControlReport1();

      if (!Equal(export.FileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(export.FileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    if (!IsEmpty(Substring(export.ProgramProcessingInfo.ParameterList, 5, 1)))
    {
      export.CaseStatus.Text1 =
        Substring(export.ProgramProcessingInfo.ParameterList, 5, 1);

      switch(AsChar(export.CaseStatus.Text1))
      {
        case 'B':
          export.FileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Both open and closed cases will be sampled." + "" + "   ";
          UseCabControlReport1();

          if (!Equal(export.FileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          break;
        case 'C':
          export.FileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Only closed cases will be sampled." + "" + "   ";
          UseCabControlReport1();

          if (!Equal(export.FileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          break;
        case 'O':
          export.FileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "Only open cases will be sampled." + ""
            + "   ";
          UseCabControlReport1();

          if (!Equal(export.FileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          break;
        default:
          export.FileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Invalid case sampling option specified: " + export
            .CaseStatus.Text1 + "   ";
          UseCabControlReport1();

          if (!Equal(export.FileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
          }
          else
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
          }

          return;
      }
    }
    else
    {
      export.CaseStatus.Text1 = "B";
      export.FileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Both open and closed cases will be sampled by default." + "" + "   ";
      UseCabControlReport1();

      if (!Equal(export.FileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(export.FileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
    }
  }

  private static void MoveEabReportSend1(EabReportSend source,
    EabReportSend target)
  {
    target.BlankLineAfterHeading = source.BlankLineAfterHeading;
    target.BlankLineAfterColHead = source.BlankLineAfterColHead;
    target.NumberOfColHeadings = source.NumberOfColHeadings;
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
    target.RptHeading3 = source.RptHeading3;
    target.ColHeading1 = source.ColHeading1;
    target.ColHeading2 = source.ColHeading2;
    target.ColHeading3 = source.ColHeading3;
  }

  private static void MoveEabReportSend2(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private void UseCabBusinessReport01()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    MoveEabReportSend1(local.EabReportSend, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = export.FileHandling.Action;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    export.FileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = export.FileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    export.FileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    MoveEabReportSend2(local.EabReportSend, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = export.FileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    export.FileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    MoveEabReportSend2(local.EabReportSend, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = export.FileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    export.FileHandling.Status = useExport.EabFileHandling.Status;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    export.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
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
    /// A value of CaseStatus.
    /// </summary>
    [JsonPropertyName("caseStatus")]
    public TextWorkArea CaseStatus
    {
      get => caseStatus ??= new();
      set => caseStatus = value;
    }

    /// <summary>
    /// A value of SamplingFrequencyTextWorkArea.
    /// </summary>
    [JsonPropertyName("samplingFrequencyTextWorkArea")]
    public TextWorkArea SamplingFrequencyTextWorkArea
    {
      get => samplingFrequencyTextWorkArea ??= new();
      set => samplingFrequencyTextWorkArea = value;
    }

    /// <summary>
    /// A value of SamplingFrequencyCommon.
    /// </summary>
    [JsonPropertyName("samplingFrequencyCommon")]
    public Common SamplingFrequencyCommon
    {
      get => samplingFrequencyCommon ??= new();
      set => samplingFrequencyCommon = value;
    }

    /// <summary>
    /// A value of FileHandling.
    /// </summary>
    [JsonPropertyName("fileHandling")]
    public EabFileHandling FileHandling
    {
      get => fileHandling ??= new();
      set => fileHandling = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    private TextWorkArea caseStatus;
    private TextWorkArea samplingFrequencyTextWorkArea;
    private Common samplingFrequencyCommon;
    private EabFileHandling fileHandling;
    private DateWorkArea max;
    private ProgramProcessingInfo programProcessingInfo;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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

    private EabReportSend eabReportSend;
    private ProgramProcessingInfo programProcessingInfo;
  }
#endregion
}
