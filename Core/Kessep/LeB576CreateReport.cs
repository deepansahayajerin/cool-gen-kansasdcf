// Program: LE_B576_CREATE_REPORT, ID: 371411824, model: 746.
// Short name: SWE02076
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_B576_CREATE_REPORT.
/// </summary>
[Serializable]
public partial class LeB576CreateReport: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B576_CREATE_REPORT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB576CreateReport(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB576CreateReport.
  /// </summary>
  public LeB576CreateReport(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (import.EabReportReturn.LineNumber == 3 || import
      .EabReportReturn.LinesRemaining == 0 || Equal
      (import.TypeOfRecord.Text2, "RN") || Equal
      (import.TypeOfRecord.Text2, "ST"))
    {
      // : If there is no enough lines to print on the page OR If it is the 
      // first page OR If New Region Name OR Statewide total has to be printed.
      if (import.EabReportReturn.LinesRemaining == 0 || Equal
        (import.TypeOfRecord.Text2, "RN") && import
        .EabReportReturn.PageNumber != 1 || Equal
        (import.TypeOfRecord.Text2, "RN") && import
        .EabReportReturn.PageNumber == 1 && import
        .EabReportReturn.LineNumber != 3 || Equal
        (import.TypeOfRecord.Text2, "ST"))
      {
        // : New Region and Statewide information should be written on a New 
        // page.
        local.EabFileHandling.Action = "NEWPAGE";
        UseCabBusinessReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail = "Error writing to the Report File.";
          UseCabErrorReport();
          ExitState = "ACO_AE0000_BATCH_ABEND";

          return;
        }
      }

      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail = "";
      UseCabBusinessReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail = "Error writing to the Report File.";
        UseCabErrorReport();
        ExitState = "ACO_AE0000_BATCH_ABEND";

        return;
      }

      // : Report column heading has to be printed on all the pages.
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "                                 Collection Officer               # of Cases Referred   # of Cases in Caseload   % of Cases Referred";
        
      UseCabBusinessReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail = "Error writing to the Report File.";
        UseCabErrorReport();
        ExitState = "ACO_AE0000_BATCH_ABEND";

        return;
      }

      if (!Equal(import.TypeOfRecord.Text2, "RN") && !
        Equal(import.TypeOfRecord.Text2, "ST") && import
        .EabReportReturn.PageNumber != 1)
      {
        // : This is to print the Region Name with cont'd text if there are any 
        // more details remaining to be written to the report file.
        if (IsEmpty(import.RegionInfo.Name))
        {
          local.ReportRegionName.Text50 = "Not Assigned";
        }
        else
        {
          local.ReportRegionName.Text50 = import.RegionInfo.Name;
        }

        local.NeededToWrite.RptDetail = "Region: " + TrimEnd
          (local.ReportRegionName.Text50) + " (cont'd...)";
        local.EabFileHandling.Action = "WRITE";
        UseCabBusinessReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail = "Error writing to the Report File.";
          UseCabErrorReport();
          ExitState = "ACO_AE0000_BATCH_ABEND";

          return;
        }

        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail = "";
        UseCabBusinessReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail = "Error writing to the Report File.";
          UseCabErrorReport();
          ExitState = "ACO_AE0000_BATCH_ABEND";

          return;
        }
      }

      if (Equal(import.TypeOfRecord.Text2, "BL"))
      {
        // : Do not print Blank line on the report file, if it is the first line
        // on the page.
        return;
      }
    }

    if (Equal(import.TypeOfRecord.Text2, "ON") || Equal
      (import.TypeOfRecord.Text2, "RN"))
    {
      // : New Office Name or Region Name has to be written to the report file.
      if (Equal(import.TypeOfRecord.Text2, "ON"))
      {
        if (IsEmpty(import.OfficeInfo.Name))
        {
          local.ReportOfficeName.Text50 = "Not Assigned";
        }
        else
        {
          local.ReportOfficeName.Text50 = import.OfficeInfo.Name;
        }

        local.NeededToWrite.RptDetail = "       Office: " + local
          .ReportOfficeName.Text50;
      }
      else if (Equal(import.TypeOfRecord.Text2, "RN"))
      {
        if (IsEmpty(import.RegionInfo.Name))
        {
          local.ReportRegionName.Text50 = "Not Assigned";
        }
        else
        {
          local.ReportRegionName.Text50 = import.RegionInfo.Name;
        }

        local.NeededToWrite.RptDetail = "Region: " + local
          .ReportRegionName.Text50;
      }
    }
    else if (Equal(import.TypeOfRecord.Text2, "CT") || Equal
      (import.TypeOfRecord.Text2, "OT") || Equal
      (import.TypeOfRecord.Text2, "RT") || Equal
      (import.TypeOfRecord.Text2, "ST"))
    {
      // : Write the Collection Officer Total, Office Total, Region Total and 
      // Statewide Total to the report file.
      local.NoCents.Flag = "Y";
      UseFnCabCurrencyToText1();
      UseFnCabCurrencyToText2();
      local.RoundedPercent.Number31 =
        Math.Round((long)import.ReferredCnt.Count * 100
        / (decimal)import.CaseloadCnt.Count, 1, MidpointRounding.AwayFromZero);
      local.TenthPlacePercentRound.TotalCurrency =
        local.RoundedPercent.Number31;
      UseFnCabCurrencyToText3();
      local.ReportPercent.Text5 =
        Substring(local.PercentWithDecimal.Text15, 9, 5);

      if (Equal(import.TypeOfRecord.Text2, "CT"))
      {
        local.ReportSpFullName.Text33 = TrimEnd(import.SpInfo.LastName) + ", " +
          TrimEnd(import.SpInfo.FirstName) + " " + import.SpInfo.MiddleInitial;
        local.NeededToWrite.RptDetail = "                                 " + local
          .ReportSpFullName.Text33 + "            " + Substring
          (local.ReportReferredCnt.Text15, WorkArea.Text15_MaxLength, 9, 7) + "                  " +
          Substring
          (local.ReportCaseloadCnt.Text15, WorkArea.Text15_MaxLength, 9, 7) + "                " +
          local.ReportPercent.Text5 + "%";
      }
      else if (Equal(import.TypeOfRecord.Text2, "OT"))
      {
        if (IsEmpty(import.OfficeInfo.Name))
        {
          local.ReportOfficeName.Text50 = "Not Assigned";
        }
        else
        {
          local.ReportOfficeName.Text50 = import.OfficeInfo.Name;
        }

        local.NeededToWrite.RptDetail = "       Office Total: " + local
          .ReportOfficeName.Text50 + "       " + Substring
          (local.ReportReferredCnt.Text15, WorkArea.Text15_MaxLength, 9, 7) + "                  " +
          Substring
          (local.ReportCaseloadCnt.Text15, WorkArea.Text15_MaxLength, 9, 7) + "                " +
          local.ReportPercent.Text5 + "%";
      }
      else if (Equal(import.TypeOfRecord.Text2, "RT"))
      {
        if (IsEmpty(import.RegionInfo.Name))
        {
          local.ReportRegionName.Text50 = "Not Assigned";
        }
        else
        {
          local.ReportRegionName.Text50 = import.RegionInfo.Name;
        }

        local.NeededToWrite.RptDetail = "Region Total: " + local
          .ReportRegionName.Text50 + "              " + Substring
          (local.ReportReferredCnt.Text15, WorkArea.Text15_MaxLength, 9, 7) + "                  " +
          Substring
          (local.ReportCaseloadCnt.Text15, WorkArea.Text15_MaxLength, 9, 7) + "                " +
          local.ReportPercent.Text5 + "%";
      }
      else if (Equal(import.TypeOfRecord.Text2, "ST"))
      {
        local.NeededToWrite.RptDetail = "Statewide Total" + "                                                               " +
          Substring
          (local.ReportReferredCnt.Text15, WorkArea.Text15_MaxLength, 9, 7) + "                  " +
          Substring
          (local.ReportCaseloadCnt.Text15, WorkArea.Text15_MaxLength, 9, 7) + "                " +
          local.ReportPercent.Text5 + "%";
      }
    }

    local.EabFileHandling.Action = "WRITE";
    UseCabBusinessReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail = "Error writing to the Report File.";
      UseCabErrorReport();
      ExitState = "ACO_AE0000_BATCH_ABEND";
    }
  }

  private void UseCabBusinessReport1()
  {
    var useImport = new CabBusinessReport11.Import();
    var useExport = new CabBusinessReport11.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabBusinessReport11.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    export.EabReportReturn.Assign(useExport.EabReportReturn);
  }

  private void UseCabBusinessReport2()
  {
    var useImport = new CabBusinessReport11.Import();
    var useExport = new CabBusinessReport11.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabBusinessReport11.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    export.EabReportReturn.Assign(useExport.EabReportReturn);
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnCabCurrencyToText1()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.Common.Count = import.CaseloadCnt.Count;
    useImport.ReturnAnInteger.Flag = local.NoCents.Flag;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    local.ReportCaseloadCnt.Text15 = useExport.WorkArea.Text15;
  }

  private void UseFnCabCurrencyToText2()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.Common.Count = import.ReferredCnt.Count;
    useImport.ReturnAnInteger.Flag = local.NoCents.Flag;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    local.ReportReferredCnt.Text15 = useExport.WorkArea.Text15;
  }

  private void UseFnCabCurrencyToText3()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.Common.TotalCurrency = local.TenthPlacePercentRound.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    local.PercentWithDecimal.Text15 = useExport.WorkArea.Text15;
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
    /// A value of TypeOfRecord.
    /// </summary>
    [JsonPropertyName("typeOfRecord")]
    public TextWorkArea TypeOfRecord
    {
      get => typeOfRecord ??= new();
      set => typeOfRecord = value;
    }

    /// <summary>
    /// A value of RegionInfo.
    /// </summary>
    [JsonPropertyName("regionInfo")]
    public CseOrganization RegionInfo
    {
      get => regionInfo ??= new();
      set => regionInfo = value;
    }

    /// <summary>
    /// A value of OfficeInfo.
    /// </summary>
    [JsonPropertyName("officeInfo")]
    public Office OfficeInfo
    {
      get => officeInfo ??= new();
      set => officeInfo = value;
    }

    /// <summary>
    /// A value of SpInfo.
    /// </summary>
    [JsonPropertyName("spInfo")]
    public ServiceProvider SpInfo
    {
      get => spInfo ??= new();
      set => spInfo = value;
    }

    /// <summary>
    /// A value of CaseloadCnt.
    /// </summary>
    [JsonPropertyName("caseloadCnt")]
    public Common CaseloadCnt
    {
      get => caseloadCnt ??= new();
      set => caseloadCnt = value;
    }

    /// <summary>
    /// A value of ReferredCnt.
    /// </summary>
    [JsonPropertyName("referredCnt")]
    public Common ReferredCnt
    {
      get => referredCnt ??= new();
      set => referredCnt = value;
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
    private CseOrganization regionInfo;
    private Office officeInfo;
    private ServiceProvider spInfo;
    private Common caseloadCnt;
    private Common referredCnt;
    private EabReportReturn eabReportReturn;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of EabReportReturn.
    /// </summary>
    [JsonPropertyName("eabReportReturn")]
    public EabReportReturn EabReportReturn
    {
      get => eabReportReturn ??= new();
      set => eabReportReturn = value;
    }

    private EabReportReturn eabReportReturn;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ReportRegionName.
    /// </summary>
    [JsonPropertyName("reportRegionName")]
    public WorkArea ReportRegionName
    {
      get => reportRegionName ??= new();
      set => reportRegionName = value;
    }

    /// <summary>
    /// A value of ReportOfficeName.
    /// </summary>
    [JsonPropertyName("reportOfficeName")]
    public WorkArea ReportOfficeName
    {
      get => reportOfficeName ??= new();
      set => reportOfficeName = value;
    }

    /// <summary>
    /// A value of ReportSpFullName.
    /// </summary>
    [JsonPropertyName("reportSpFullName")]
    public WorkArea ReportSpFullName
    {
      get => reportSpFullName ??= new();
      set => reportSpFullName = value;
    }

    /// <summary>
    /// A value of ReportReferredCnt.
    /// </summary>
    [JsonPropertyName("reportReferredCnt")]
    public WorkArea ReportReferredCnt
    {
      get => reportReferredCnt ??= new();
      set => reportReferredCnt = value;
    }

    /// <summary>
    /// A value of ReportCaseloadCnt.
    /// </summary>
    [JsonPropertyName("reportCaseloadCnt")]
    public WorkArea ReportCaseloadCnt
    {
      get => reportCaseloadCnt ??= new();
      set => reportCaseloadCnt = value;
    }

    /// <summary>
    /// A value of NoCents.
    /// </summary>
    [JsonPropertyName("noCents")]
    public Common NoCents
    {
      get => noCents ??= new();
      set => noCents = value;
    }

    /// <summary>
    /// A value of RoundedPercent.
    /// </summary>
    [JsonPropertyName("roundedPercent")]
    public WorkDecimalNumbers RoundedPercent
    {
      get => roundedPercent ??= new();
      set => roundedPercent = value;
    }

    /// <summary>
    /// A value of ReportPercent.
    /// </summary>
    [JsonPropertyName("reportPercent")]
    public WorkArea ReportPercent
    {
      get => reportPercent ??= new();
      set => reportPercent = value;
    }

    /// <summary>
    /// A value of TenthPlacePercentRound.
    /// </summary>
    [JsonPropertyName("tenthPlacePercentRound")]
    public Common TenthPlacePercentRound
    {
      get => tenthPlacePercentRound ??= new();
      set => tenthPlacePercentRound = value;
    }

    /// <summary>
    /// A value of PercentWithDecimal.
    /// </summary>
    [JsonPropertyName("percentWithDecimal")]
    public WorkArea PercentWithDecimal
    {
      get => percentWithDecimal ??= new();
      set => percentWithDecimal = value;
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

    private WorkArea reportRegionName;
    private WorkArea reportOfficeName;
    private WorkArea reportSpFullName;
    private WorkArea reportReferredCnt;
    private WorkArea reportCaseloadCnt;
    private Common noCents;
    private WorkDecimalNumbers roundedPercent;
    private WorkArea reportPercent;
    private Common tenthPlacePercentRound;
    private WorkArea percentWithDecimal;
    private EabFileHandling eabFileHandling;
    private EabReportSend neededToWrite;
  }
#endregion
}
