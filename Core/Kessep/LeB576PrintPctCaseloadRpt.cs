// Program: LE_B576_PRINT_PCT_CASELOAD_RPT, ID: 371411663, model: 746.
// Short name: SWEL576B
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_B576_PRINT_PCT_CASELOAD_RPT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class LeB576PrintPctCaseloadRpt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B576_PRINT_PCT_CASELOAD_RPT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB576PrintPctCaseloadRpt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB576PrintPctCaseloadRpt.
  /// </summary>
  public LeB576PrintPctCaseloadRpt(IContext context, Import import,
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
    // ------------------------------------------------------------
    //           M A I N T E N A N C E   L O G
    // Date	    Developer		        Description
    // 04-20-2009  SWDPLSS - Linda Smith       Initial Development
    // END of   M A I N T E N A N C E   L O G
    // ------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // ***** Get the run parameters for this program.
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // ok, continue processing
      // : The reporting end of the month is the last day of the prior 
      // processing month.
      local.ReportingEom.Date =
        AddDays(local.ProgramProcessingInfo.ProcessDate, -
        Day(local.ProgramProcessingInfo.ProcessDate));
    }
    else
    {
      // : Abort exitstate already set.
      return;
    }

    // *****************************************************************
    // * Setup of batch error handling
    // 
    // *
    // *****************************************************************
    // *****************************************************************
    // * Open the ERROR RPT. DDNAME=RPT99.                             *
    // *****************************************************************
    local.EabFileHandling.Action = "OPEN";
    local.NeededToOpen.ProgramName = local.ProgramProcessingInfo.Name;
    local.NeededToOpen.ProcessDate = local.ReportingEom.Date;
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_AE0000_BATCH_ABEND";

      return;
    }

    // *****************************************************************
    // * End of Batch error handling setup                             *
    // *****************************************************************
    // : Open Control Report file
    local.EabFileHandling.Action = "OPEN";
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered opening control report.";
      UseCabErrorReport3();
      ExitState = "ACO_AE0000_BATCH_ABEND";

      return;
    }

    // : Open the Report 01
    local.EabFileHandling.Action = "OPEN";
    UseCabBusinessReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail = "Error encountered opening report file.";
      UseCabErrorReport3();
      ExitState = "ACO_AE0000_BATCH_ABEND";

      return;
    }

    // : Open the input file
    local.EabFileHandling.Action = "OPEN";
    UseLeEabReadB576FileExtract2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered opening the Input File.";
      UseCabErrorReport3();
      ExitState = "ACO_AE0000_BATCH_ABEND";

      return;
    }

    // *********** Main Processing Logic Starts Here **********
    local.FirstRecord.Flag = "Y";

    do
    {
      local.EabFileHandling.Action = "READ";
      UseLeEabReadB576FileExtract1();

      switch(TrimEnd(local.EabFileHandling.Status))
      {
        case "OK":
          ++local.TotalRecordsRead.Count;

          break;
        case "EF":
          local.EndOfInputFile.Flag = "Y";

          if (AsChar(local.FirstRecord.Flag) == 'Y')
          {
            continue;
          }

          break;
        default:
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "Error encountered while reading the Input File.";
          UseCabErrorReport3();
          ExitState = "ACO_AE0000_BATCH_ABEND";

          return;
      }

      if (AsChar(local.FirstRecord.Flag) == 'Y')
      {
        MoveCseOrganization(local.FileCurr, local.FilePrevRegionInfo);
        MoveOffice(local.FileCurrOfficeInfo, local.FilePrevOfficeInfo);
      }

      if (!Equal(local.FileCurr.Name, local.FilePrevRegionInfo.Name) || AsChar
        (local.EndOfInputFile.Flag) == 'Y')
      {
        // : 'BL' is for Blank Record.
        local.TypeOfRecord.Text2 = "BL";
        UseLeB576CreateReport4();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // : 'OT' is for Office Total
        local.TypeOfRecord.Text2 = "OT";
        UseLeB576CreateReport3();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // : 'BL' is for Blank Record.
        local.TypeOfRecord.Text2 = "BL";
        UseLeB576CreateReport4();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // : 'RT' is for Region Total
        local.TypeOfRecord.Text2 = "RT";
        UseLeB576CreateReport2();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (AsChar(local.EndOfInputFile.Flag) == 'Y')
        {
          // : 'ST' is for Statewide Total
          local.TypeOfRecord.Text2 = "ST";
          UseLeB576CreateReport1();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          continue;
        }

        // : 'RN' is for printing the heading for New Region Name on a New page.
        local.TypeOfRecord.Text2 = "RN";
        UseLeB576CreateReport6();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // : 'BL' is for Blank Record.
        local.TypeOfRecord.Text2 = "BL";
        UseLeB576CreateReport7();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // : 'ON' is for printing the heading for New Office Name.
        local.TypeOfRecord.Text2 = "ON";
        UseLeB576CreateReport6();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // : 'BL' is for Blank Record.
        local.TypeOfRecord.Text2 = "BL";
        UseLeB576CreateReport7();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        local.OfficeCaseloadTotal.Count = 0;
        local.OfficeReferredTotal.Count = 0;
        local.RegionCaseloadTotal.Count = 0;
        local.RegionReferredTotal.Count = 0;
        MoveCseOrganization(local.FileCurr, local.FilePrevRegionInfo);
        MoveOffice(local.FileCurrOfficeInfo, local.FilePrevOfficeInfo);
      }
      else if (!Equal(local.FileCurrOfficeInfo.Name,
        local.FilePrevOfficeInfo.Name))
      {
        // : 'BL' is for Blank Record.
        local.TypeOfRecord.Text2 = "BL";
        UseLeB576CreateReport4();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // : 'OT' is for Office Total
        local.TypeOfRecord.Text2 = "OT";
        UseLeB576CreateReport3();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // : 'BL' is for Blank Record.
        local.TypeOfRecord.Text2 = "BL";
        UseLeB576CreateReport4();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // : 'ON' is for printing the heading for New Office Name.
        local.TypeOfRecord.Text2 = "ON";
        UseLeB576CreateReport6();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // : 'BL' is for Blank Record.
        local.TypeOfRecord.Text2 = "BL";
        UseLeB576CreateReport4();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        local.OfficeCaseloadTotal.Count = 0;
        local.OfficeReferredTotal.Count = 0;
        MoveOffice(local.FileCurrOfficeInfo, local.FilePrevOfficeInfo);
      }

      if (AsChar(local.FirstRecord.Flag) == 'Y')
      {
        // : 'RN' is for printing the heading for New Region Name.
        local.TypeOfRecord.Text2 = "RN";
        UseLeB576CreateReport6();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // : 'BL' is for Blank Record.
        local.TypeOfRecord.Text2 = "BL";
        UseLeB576CreateReport7();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // : 'ON' is for printing the heading for New Office Name.
        local.TypeOfRecord.Text2 = "ON";
        UseLeB576CreateReport6();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // : 'BL' is for Blank Record.
        local.TypeOfRecord.Text2 = "BL";
        UseLeB576CreateReport7();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        local.FirstRecord.Flag = "N";
      }

      // : 'CT' is for Collection Officer total and this total was already 
      // calculated before executing this program.
      local.TypeOfRecord.Text2 = "CT";
      UseLeB576CreateReport5();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      local.OfficeCaseloadTotal.Count += local.FileCurrCoCaseloadCnt.Count;
      local.OfficeReferredTotal.Count += local.FileCurrCoReferredCnt.Count;
      local.RegionCaseloadTotal.Count += local.FileCurrCoCaseloadCnt.Count;
      local.RegionReferredTotal.Count += local.FileCurrCoReferredCnt.Count;
      local.StatewideCaseloadTotal.Count += local.FileCurrCoCaseloadCnt.Count;
      local.StatewideReferredTotal.Count += local.FileCurrCoReferredCnt.Count;
    }
    while(AsChar(local.EndOfInputFile.Flag) != 'Y');

    // *********** Main Processing Logic Ends Here **********
    // END OF PROCESSING
    // **** START OF REPORT DSN CLOSE PROCESS ****
    // : Close the input file
    local.EabFileHandling.Action = "CLOSE";
    UseLeEabReadB576FileExtract2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.NeededToWrite.RptDetail =
        "Error encountered closing the Extract File.";
      UseCabErrorReport3();
      ExitState = "ACO_AE0000_BATCH_ABEND";

      return;
    }

    // **** END OF REPORT DSN CLOSE PROCESS ****
    // : Close Report 01
    local.EabFileHandling.Action = "CLOSE";
    UseCabBusinessReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.NeededToWrite.RptDetail =
        "Error encountered closing the Report File.";
      UseCabErrorReport3();
      ExitState = "ACO_AE0000_BATCH_ABEND";

      return;
    }

    // : Write Control Totals
    local.EabFileHandling.Action = "WRITE";
    local.NeededToWrite.RptDetail = "Total Records Read          :";
    local.NeededToWrite.RptDetail = TrimEnd(local.NeededToWrite.RptDetail) + " " +
      NumberToString(local.TotalRecordsRead.Count, 7, 9);
    UseCabControlReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.NeededToWrite.RptDetail =
        "Close of Control Report file was unsuccessful.";
      UseCabErrorReport3();
      ExitState = "ACO_AE0000_BATCH_ABEND";

      return;
    }

    // : Close Control Report file
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.NeededToWrite.RptDetail =
        "Close of Control Report file was unsuccessful.";
      UseCabErrorReport3();
      ExitState = "ACO_AE0000_BATCH_ABEND";

      return;
    }

    // *****************************************************************
    // * Close the ERROR RPT. DDNAME=RPT99.                             *
    // *****************************************************************
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport1();
    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private static void MoveCseOrganization(CseOrganization source,
    CseOrganization target)
  {
    target.Code = source.Code;
    target.Name = source.Name;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveOffice(Office source, Office target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.Name = source.Name;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private void UseCabBusinessReport1()
  {
    var useImport = new CabBusinessReport11.Import();
    var useExport = new CabBusinessReport11.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabBusinessReport11.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport2()
  {
    var useImport = new CabBusinessReport11.Import();
    var useExport = new CabBusinessReport11.Export();

    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabBusinessReport11.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    local.EabReportReturn.Assign(useExport.EabReportReturn);
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport3()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseLeB576CreateReport1()
  {
    var useImport = new LeB576CreateReport.Import();
    var useExport = new LeB576CreateReport.Export();

    useImport.TypeOfRecord.Text2 = local.TypeOfRecord.Text2;
    useImport.CaseloadCnt.Count = local.StatewideCaseloadTotal.Count;
    useImport.ReferredCnt.Count = local.StatewideReferredTotal.Count;
    useImport.EabReportReturn.Assign(local.EabReportReturn);

    Call(LeB576CreateReport.Execute, useImport, useExport);

    local.EabReportReturn.Assign(useExport.EabReportReturn);
  }

  private void UseLeB576CreateReport2()
  {
    var useImport = new LeB576CreateReport.Import();
    var useExport = new LeB576CreateReport.Export();

    useImport.TypeOfRecord.Text2 = local.TypeOfRecord.Text2;
    MoveCseOrganization(local.FilePrevRegionInfo, useImport.RegionInfo);
    MoveOffice(local.FilePrevOfficeInfo, useImport.OfficeInfo);
    useImport.CaseloadCnt.Count = local.RegionCaseloadTotal.Count;
    useImport.ReferredCnt.Count = local.RegionReferredTotal.Count;
    useImport.EabReportReturn.Assign(local.EabReportReturn);

    Call(LeB576CreateReport.Execute, useImport, useExport);

    local.EabReportReturn.Assign(useExport.EabReportReturn);
  }

  private void UseLeB576CreateReport3()
  {
    var useImport = new LeB576CreateReport.Import();
    var useExport = new LeB576CreateReport.Export();

    useImport.TypeOfRecord.Text2 = local.TypeOfRecord.Text2;
    MoveCseOrganization(local.FilePrevRegionInfo, useImport.RegionInfo);
    MoveOffice(local.FilePrevOfficeInfo, useImport.OfficeInfo);
    useImport.CaseloadCnt.Count = local.OfficeCaseloadTotal.Count;
    useImport.ReferredCnt.Count = local.OfficeReferredTotal.Count;
    useImport.EabReportReturn.Assign(local.EabReportReturn);

    Call(LeB576CreateReport.Execute, useImport, useExport);

    local.EabReportReturn.Assign(useExport.EabReportReturn);
  }

  private void UseLeB576CreateReport4()
  {
    var useImport = new LeB576CreateReport.Import();
    var useExport = new LeB576CreateReport.Export();

    useImport.TypeOfRecord.Text2 = local.TypeOfRecord.Text2;
    MoveCseOrganization(local.FilePrevRegionInfo, useImport.RegionInfo);
    useImport.EabReportReturn.Assign(local.EabReportReturn);

    Call(LeB576CreateReport.Execute, useImport, useExport);

    local.EabReportReturn.Assign(useExport.EabReportReturn);
  }

  private void UseLeB576CreateReport5()
  {
    var useImport = new LeB576CreateReport.Import();
    var useExport = new LeB576CreateReport.Export();

    useImport.TypeOfRecord.Text2 = local.TypeOfRecord.Text2;
    MoveCseOrganization(local.FileCurr, useImport.RegionInfo);
    MoveOffice(local.FileCurrOfficeInfo, useImport.OfficeInfo);
    useImport.SpInfo.Assign(local.FileCurrSpInfo);
    useImport.CaseloadCnt.Count = local.FileCurrCoCaseloadCnt.Count;
    useImport.ReferredCnt.Count = local.FileCurrCoReferredCnt.Count;
    useImport.EabReportReturn.Assign(local.EabReportReturn);

    Call(LeB576CreateReport.Execute, useImport, useExport);

    local.EabReportReturn.Assign(useExport.EabReportReturn);
  }

  private void UseLeB576CreateReport6()
  {
    var useImport = new LeB576CreateReport.Import();
    var useExport = new LeB576CreateReport.Export();

    useImport.TypeOfRecord.Text2 = local.TypeOfRecord.Text2;
    MoveCseOrganization(local.FileCurr, useImport.RegionInfo);
    MoveOffice(local.FileCurrOfficeInfo, useImport.OfficeInfo);
    useImport.EabReportReturn.Assign(local.EabReportReturn);

    Call(LeB576CreateReport.Execute, useImport, useExport);

    local.EabReportReturn.Assign(useExport.EabReportReturn);
  }

  private void UseLeB576CreateReport7()
  {
    var useImport = new LeB576CreateReport.Import();
    var useExport = new LeB576CreateReport.Export();

    useImport.TypeOfRecord.Text2 = local.TypeOfRecord.Text2;
    MoveCseOrganization(local.FileCurr, useImport.RegionInfo);
    useImport.EabReportReturn.Assign(local.EabReportReturn);

    Call(LeB576CreateReport.Execute, useImport, useExport);

    local.EabReportReturn.Assign(useExport.EabReportReturn);
  }

  private void UseLeEabReadB576FileExtract1()
  {
    var useImport = new LeEabReadB576FileExtract.Import();
    var useExport = new LeEabReadB576FileExtract.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;
    MoveCseOrganization(local.FileCurr, useExport.RegionInfo);
    MoveOffice(local.FileCurrOfficeInfo, useExport.OfficeInfo);
    useExport.SpInfo.Assign(local.FileCurrSpInfo);
    useExport.CoCaseloadCnt.Count = local.FileCurrCoCaseloadCnt.Count;
    useExport.CoReferredCnt.Count = local.FileCurrCoReferredCnt.Count;

    Call(LeEabReadB576FileExtract.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    MoveCseOrganization(useExport.RegionInfo, local.FileCurr);
    MoveOffice(useExport.OfficeInfo, local.FileCurrOfficeInfo);
    local.FileCurrSpInfo.Assign(useExport.SpInfo);
    local.FileCurrCoCaseloadCnt.Count = useExport.CoCaseloadCnt.Count;
    local.FileCurrCoReferredCnt.Count = useExport.CoReferredCnt.Count;
  }

  private void UseLeEabReadB576FileExtract2()
  {
    var useImport = new LeEabReadB576FileExtract.Import();
    var useExport = new LeEabReadB576FileExtract.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(LeEabReadB576FileExtract.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    MoveProgramProcessingInfo(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
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
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of TypeOfRecord.
    /// </summary>
    [JsonPropertyName("typeOfRecord")]
    public TextWorkArea TypeOfRecord
    {
      get => typeOfRecord ??= new();
      set => typeOfRecord = value;
    }

    /// <summary>
    /// A value of StatewideCaseloadTotal.
    /// </summary>
    [JsonPropertyName("statewideCaseloadTotal")]
    public Common StatewideCaseloadTotal
    {
      get => statewideCaseloadTotal ??= new();
      set => statewideCaseloadTotal = value;
    }

    /// <summary>
    /// A value of StatewideReferredTotal.
    /// </summary>
    [JsonPropertyName("statewideReferredTotal")]
    public Common StatewideReferredTotal
    {
      get => statewideReferredTotal ??= new();
      set => statewideReferredTotal = value;
    }

    /// <summary>
    /// A value of RegionCaseloadTotal.
    /// </summary>
    [JsonPropertyName("regionCaseloadTotal")]
    public Common RegionCaseloadTotal
    {
      get => regionCaseloadTotal ??= new();
      set => regionCaseloadTotal = value;
    }

    /// <summary>
    /// A value of RegionReferredTotal.
    /// </summary>
    [JsonPropertyName("regionReferredTotal")]
    public Common RegionReferredTotal
    {
      get => regionReferredTotal ??= new();
      set => regionReferredTotal = value;
    }

    /// <summary>
    /// A value of OfficeCaseloadTotal.
    /// </summary>
    [JsonPropertyName("officeCaseloadTotal")]
    public Common OfficeCaseloadTotal
    {
      get => officeCaseloadTotal ??= new();
      set => officeCaseloadTotal = value;
    }

    /// <summary>
    /// A value of OfficeReferredTotal.
    /// </summary>
    [JsonPropertyName("officeReferredTotal")]
    public Common OfficeReferredTotal
    {
      get => officeReferredTotal ??= new();
      set => officeReferredTotal = value;
    }

    /// <summary>
    /// A value of FilePrevRegionInfo.
    /// </summary>
    [JsonPropertyName("filePrevRegionInfo")]
    public CseOrganization FilePrevRegionInfo
    {
      get => filePrevRegionInfo ??= new();
      set => filePrevRegionInfo = value;
    }

    /// <summary>
    /// A value of FilePrevOfficeInfo.
    /// </summary>
    [JsonPropertyName("filePrevOfficeInfo")]
    public Office FilePrevOfficeInfo
    {
      get => filePrevOfficeInfo ??= new();
      set => filePrevOfficeInfo = value;
    }

    /// <summary>
    /// A value of ReportingEom.
    /// </summary>
    [JsonPropertyName("reportingEom")]
    public DateWorkArea ReportingEom
    {
      get => reportingEom ??= new();
      set => reportingEom = value;
    }

    /// <summary>
    /// A value of FirstRecord.
    /// </summary>
    [JsonPropertyName("firstRecord")]
    public Common FirstRecord
    {
      get => firstRecord ??= new();
      set => firstRecord = value;
    }

    /// <summary>
    /// A value of EndOfInputFile.
    /// </summary>
    [JsonPropertyName("endOfInputFile")]
    public Common EndOfInputFile
    {
      get => endOfInputFile ??= new();
      set => endOfInputFile = value;
    }

    /// <summary>
    /// A value of FileCurr.
    /// </summary>
    [JsonPropertyName("fileCurr")]
    public CseOrganization FileCurr
    {
      get => fileCurr ??= new();
      set => fileCurr = value;
    }

    /// <summary>
    /// A value of FileCurrOfficeInfo.
    /// </summary>
    [JsonPropertyName("fileCurrOfficeInfo")]
    public Office FileCurrOfficeInfo
    {
      get => fileCurrOfficeInfo ??= new();
      set => fileCurrOfficeInfo = value;
    }

    /// <summary>
    /// A value of FileCurrSpInfo.
    /// </summary>
    [JsonPropertyName("fileCurrSpInfo")]
    public ServiceProvider FileCurrSpInfo
    {
      get => fileCurrSpInfo ??= new();
      set => fileCurrSpInfo = value;
    }

    /// <summary>
    /// A value of FileCurrCoCaseloadCnt.
    /// </summary>
    [JsonPropertyName("fileCurrCoCaseloadCnt")]
    public Common FileCurrCoCaseloadCnt
    {
      get => fileCurrCoCaseloadCnt ??= new();
      set => fileCurrCoCaseloadCnt = value;
    }

    /// <summary>
    /// A value of FileCurrCoReferredCnt.
    /// </summary>
    [JsonPropertyName("fileCurrCoReferredCnt")]
    public Common FileCurrCoReferredCnt
    {
      get => fileCurrCoReferredCnt ??= new();
      set => fileCurrCoReferredCnt = value;
    }

    /// <summary>
    /// A value of TotalRecordsRead.
    /// </summary>
    [JsonPropertyName("totalRecordsRead")]
    public Common TotalRecordsRead
    {
      get => totalRecordsRead ??= new();
      set => totalRecordsRead = value;
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
    /// A value of EabReportReturn.
    /// </summary>
    [JsonPropertyName("eabReportReturn")]
    public EabReportReturn EabReportReturn
    {
      get => eabReportReturn ??= new();
      set => eabReportReturn = value;
    }

    private TextWorkArea typeOfRecord;
    private Common statewideCaseloadTotal;
    private Common statewideReferredTotal;
    private Common regionCaseloadTotal;
    private Common regionReferredTotal;
    private Common officeCaseloadTotal;
    private Common officeReferredTotal;
    private CseOrganization filePrevRegionInfo;
    private Office filePrevOfficeInfo;
    private DateWorkArea reportingEom;
    private Common firstRecord;
    private Common endOfInputFile;
    private CseOrganization fileCurr;
    private Office fileCurrOfficeInfo;
    private ServiceProvider fileCurrSpInfo;
    private Common fileCurrCoCaseloadCnt;
    private Common fileCurrCoReferredCnt;
    private Common totalRecordsRead;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend neededToOpen;
    private EabReportSend neededToWrite;
    private EabReportReturn eabReportReturn;
  }
#endregion
}
