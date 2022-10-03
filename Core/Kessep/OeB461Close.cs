// Program: OE_B461_CLOSE, ID: 371393496, model: 746.
// Short name: SWE03619
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B461_CLOSE.
/// </summary>
[Serializable]
public partial class OeB461Close: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B461_CLOSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB461Close(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB461Close.
  /// </summary>
  public OeB461Close(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // **********************************************************************************************
    // 01/12/2008                  DDupree                 WR 280420
    // Initial programming.
    // **********************************************************************************************
    local.EabFileHandling.Action = "WRITE";

    do
    {
      switch(local.Sub.Count)
      {
        case 2:
          local.EabReportSend.RptDetail =
            "Total number of records processed          :" + NumberToString
            (import.TotalNumProcessed.Count, 15);

          break;
        case 3:
          local.EabReportSend.RptDetail =
            "Total number of obligors restricted        :" + NumberToString
            (import.NumLicenseRestricted.Count, 15);

          break;
        case 4:
          local.EabReportSend.RptDetail =
            "Total number of restricted denied records  :" + NumberToString
            (import.NumRestrictDeniedRecs.Count, 15);

          break;
        case 5:
          local.EabReportSend.RptDetail =
            "Total number of obligors reinstated        :" + NumberToString
            (import.NumLicenseReinstated.Count, 15);

          break;
        case 6:
          local.EabReportSend.RptDetail =
            "Total number of reinstated denied records  :" + NumberToString
            (import.NumReinstateDeniedRecs.Count, 15);

          break;
        case 7:
          local.EabReportSend.RptDetail =
            "Total number of error records              :" + NumberToString
            (import.NumberOfErrorRecords.Count, 15);

          break;
        case 8:
          local.EabReportSend.RptDetail =
            "Total number of missing restricted APs     :" + NumberToString
            (import.NumMissingRestrictAps.Count, 15);

          break;
        case 9:
          local.EabReportSend.RptDetail =
            "Total number of missing reinstated APs     :" + NumberToString
            (import.NumMissingReinstateAps.Count, 15);

          break;
        case 10:
          import.ProcessDates.Index = 0;

          for(var limit = import.ProcessDates.Count; import
            .ProcessDates.Index < limit; ++import.ProcessDates.Index)
          {
            if (!import.ProcessDates.CheckSize())
            {
              break;
            }

            if (AsChar(import.ProcessDates.Item.KdmvFile.FileType) == '1' || AsChar
              (import.ProcessDates.Item.KdmvFile.FileType) == '2')
            {
              local.EabReportSend.RptDetail =
                "Total # of restricted AP's not processed : " + NumberToString
                (import.ProcessDates.Item.NumMissRestrictAp.Count, 15) + " ; from process date : " +
                import.ProcessDates.Item.ProcessDate.TextDate;
            }
            else
            {
              local.EabReportSend.RptDetail =
                "Total # of reinstated AP's not processed : " + NumberToString
                (import.ProcessDates.Item.NumMissReinstAps.Count, 15) + " ; from process date : " +
                import.ProcessDates.Item.ProcessDate.TextDate;
            }

            UseCabControlReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "Error encountered while writting control report";
              UseCabErrorReport1();
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            local.EabReportSend.RptDetail = "";
          }

          import.ProcessDates.CheckIndex();

          break;
        default:
          local.EabReportSend.RptDetail = "";

          break;
      }

      UseCabControlReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error encountered while writting control report";
        UseCabErrorReport1();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      ++local.Sub.Count;
      local.EabReportSend.RptDetail = "";
    }
    while(local.Sub.Count <= 9);

    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
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
    /// <summary>A ProcessDatesGroup group.</summary>
    [Serializable]
    public class ProcessDatesGroup
    {
      /// <summary>
      /// A value of NumMissRestrictAp.
      /// </summary>
      [JsonPropertyName("numMissRestrictAp")]
      public Common NumMissRestrictAp
      {
        get => numMissRestrictAp ??= new();
        set => numMissRestrictAp = value;
      }

      /// <summary>
      /// A value of NumMissReinstAps.
      /// </summary>
      [JsonPropertyName("numMissReinstAps")]
      public Common NumMissReinstAps
      {
        get => numMissReinstAps ??= new();
        set => numMissReinstAps = value;
      }

      /// <summary>
      /// A value of ProcessDate.
      /// </summary>
      [JsonPropertyName("processDate")]
      public DateWorkArea ProcessDate
      {
        get => processDate ??= new();
        set => processDate = value;
      }

      /// <summary>
      /// A value of KdmvFile.
      /// </summary>
      [JsonPropertyName("kdmvFile")]
      public KdmvFile KdmvFile
      {
        get => kdmvFile ??= new();
        set => kdmvFile = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 25;

      private Common numMissRestrictAp;
      private Common numMissReinstAps;
      private DateWorkArea processDate;
      private KdmvFile kdmvFile;
    }

    /// <summary>
    /// A value of TotalNumProcessed.
    /// </summary>
    [JsonPropertyName("totalNumProcessed")]
    public Common TotalNumProcessed
    {
      get => totalNumProcessed ??= new();
      set => totalNumProcessed = value;
    }

    /// <summary>
    /// A value of NumLicenseRestricted.
    /// </summary>
    [JsonPropertyName("numLicenseRestricted")]
    public Common NumLicenseRestricted
    {
      get => numLicenseRestricted ??= new();
      set => numLicenseRestricted = value;
    }

    /// <summary>
    /// A value of NumRestrictDeniedRecs.
    /// </summary>
    [JsonPropertyName("numRestrictDeniedRecs")]
    public Common NumRestrictDeniedRecs
    {
      get => numRestrictDeniedRecs ??= new();
      set => numRestrictDeniedRecs = value;
    }

    /// <summary>
    /// A value of NumberOfErrorRecords.
    /// </summary>
    [JsonPropertyName("numberOfErrorRecords")]
    public Common NumberOfErrorRecords
    {
      get => numberOfErrorRecords ??= new();
      set => numberOfErrorRecords = value;
    }

    /// <summary>
    /// A value of NumMissingRestrictAps.
    /// </summary>
    [JsonPropertyName("numMissingRestrictAps")]
    public Common NumMissingRestrictAps
    {
      get => numMissingRestrictAps ??= new();
      set => numMissingRestrictAps = value;
    }

    /// <summary>
    /// A value of NumLicenseReinstated.
    /// </summary>
    [JsonPropertyName("numLicenseReinstated")]
    public Common NumLicenseReinstated
    {
      get => numLicenseReinstated ??= new();
      set => numLicenseReinstated = value;
    }

    /// <summary>
    /// A value of NumReinstateDeniedRecs.
    /// </summary>
    [JsonPropertyName("numReinstateDeniedRecs")]
    public Common NumReinstateDeniedRecs
    {
      get => numReinstateDeniedRecs ??= new();
      set => numReinstateDeniedRecs = value;
    }

    /// <summary>
    /// A value of NumMissingReinstateAps.
    /// </summary>
    [JsonPropertyName("numMissingReinstateAps")]
    public Common NumMissingReinstateAps
    {
      get => numMissingReinstateAps ??= new();
      set => numMissingReinstateAps = value;
    }

    /// <summary>
    /// Gets a value of ProcessDates.
    /// </summary>
    [JsonIgnore]
    public Array<ProcessDatesGroup> ProcessDates => processDates ??= new(
      ProcessDatesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ProcessDates for json serialization.
    /// </summary>
    [JsonPropertyName("processDates")]
    [Computed]
    public IList<ProcessDatesGroup> ProcessDates_Json
    {
      get => processDates;
      set => ProcessDates.Assign(value);
    }

    private Common totalNumProcessed;
    private Common numLicenseRestricted;
    private Common numRestrictDeniedRecs;
    private Common numberOfErrorRecords;
    private Common numMissingRestrictAps;
    private Common numLicenseReinstated;
    private Common numReinstateDeniedRecs;
    private Common numMissingReinstateAps;
    private Array<ProcessDatesGroup> processDates;
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
    /// A value of Sub.
    /// </summary>
    [JsonPropertyName("sub")]
    public Common Sub
    {
      get => sub ??= new();
      set => sub = value;
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private Common sub;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
  }
#endregion
}
