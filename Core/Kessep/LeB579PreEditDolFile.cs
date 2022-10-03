// Program: LE_B579_PRE_EDIT_DOL_FILE, ID: 945100901, model: 746.
// Short name: SWE03074
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_B579_PRE_EDIT_DOL_FILE.
/// </summary>
[Serializable]
public partial class LeB579PreEditDolFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B579_PRE_EDIT_DOL_FILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB579PreEditDolFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB579PreEditDolFile.
  /// </summary>
  public LeB579PreEditDolFile(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------------------------
    // Date      Developer         Request #  Description
    // --------  ----------------  ---------  ------------------------
    // 05/14/12  GVandy            CQ33628    Initial Development
    // -------------------------------------------------------------------------------------
    // ------------------------------------------------------------------------------
    // -- Open the UI IWO Notice file from KDOL.
    // ------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    UseLeEabReadKdolIwoNoticeInfo();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error opening KDOL IWO Notice file in pre-edit. Status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport();
      ExitState = "FILE_OPEN_ERROR_AB";

      return;
    }

    // ------------------------------------------------------------------------------
    // -- Process each IWO Notice record from DOL.
    // ------------------------------------------------------------------------------
    do
    {
      // ------------------------------------------------------------------------------
      // -- Read an IWO Notice record from DOL.
      // ------------------------------------------------------------------------------
      local.EabFileHandling.Action = "READ";
      UseLeEabReadKdolIwoNoticeInfo();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        if (Equal(local.EabFileHandling.Status, "EF"))
        {
          continue;
        }

        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error reading KDOL IWO Notice file in pre-edit. Status = " + local
          .EabFileHandling.Status;
        UseCabErrorReport();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      switch(TrimEnd(Substring(local.KdolUiInboundFile.NewClaimantRecord, 1, 1)))
        
      {
        case "1":
          // -- Header record
          ++local.RecType1.Count;

          // -- Validate that the file creation date (positions 2 - 9) is 
          // numeric.
          if (Verify(Substring(
            local.KdolUiInboundFile.NewClaimantRecord,
            KdolUiInboundFile.NewClaimantRecord_MaxLength, 2, 8),
            "0123456789") != 0)
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "File Creation Date (Position 2 - 9) in KDOL file is not numeric: " +
              local.KdolUiInboundFile.NewClaimantRecord;
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }
          else
          {
            // -- Export the file creation date for checkpoint/restart purposes.
            export.KdolFileCreationDate.TextDate =
              Substring(local.KdolUiInboundFile.NewClaimantRecord, 2, 8);
          }

          break;
        case "2":
          // -- Detail record
          ++local.RecType2.Count;

          // -- Validate that SSN (positions 2 - 10) is numeric.
          if (Verify(Substring(
            local.KdolUiInboundFile.NewClaimantRecord,
            KdolUiInboundFile.NewClaimantRecord_MaxLength, 2, 9),
            "0123456789") != 0)
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "SSN (Position 2 - 10) in KDOL file is not numeric: " + local
              .KdolUiInboundFile.NewClaimantRecord;
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          break;
        case "3":
          // -- Footer record
          ++local.RecType3.Count;

          // -- Validate that Total Detail Record Count (positions 3 - 12) is 
          // numeric.
          if (Verify(Substring(
            local.KdolUiInboundFile.NewClaimantRecord,
            KdolUiInboundFile.NewClaimantRecord_MaxLength, 3, 10),
            "0123456789") != 0)
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Footer Record Total Detail Record Count (Position 3 - 12) in KDOL file is not numeric: " +
              local.KdolUiInboundFile.NewClaimantRecord;
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          // -- Extract the footer total detail count and total detail amount.
          local.FooterTotal.Count =
            (int)StringToNumber(Substring(
              local.KdolUiInboundFile.NewClaimantRecord,
            KdolUiInboundFile.NewClaimantRecord_MaxLength, 3, 10));

          break;
        default:
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Invalid record type (Position 1) in KDOL file: " + local
            .KdolUiInboundFile.NewClaimantRecord;
          UseCabErrorReport();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
      }
    }
    while(!Equal(local.EabFileHandling.Status, "EF"));

    // -- Check for an empty file.
    if (local.RecType1.Count + local.RecType2.Count + local.RecType3.Count == 0)
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "KDOL file contains no records.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -- Validate there was only one header record in the file.
    switch(local.RecType1.Count)
    {
      case 0:
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "KDOL file does not contain a header record.";
        UseCabErrorReport();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      case 1:
        // -- File contains one header record.  Continue.
        break;
      default:
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "KDOL file contains multiple header records.";
        UseCabErrorReport();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
    }

    // -- Validate there was only one footer record in the file.
    switch(local.RecType3.Count)
    {
      case 0:
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "KDOL file does not contain a footer record.";
        UseCabErrorReport();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      case 1:
        break;
      default:
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "KDOL file contains multiple footer records.";
        UseCabErrorReport();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
    }

    // -- Validate the footer record totals match the values calculated from the
    // detail records.
    if (local.FooterTotal.Count != local.RecType2.Count)
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "KDOL footer record detail count " + NumberToString
        (local.FooterTotal.Count, 1, 15) + " does not match calculated detail record count " +
        NumberToString(local.RecType2.Count, 15);
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ------------------------------------------------------------------------------
    // -- Close the UI IWO Notice file from KDOL.
    // ------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseLeEabReadKdolIwoNoticeInfo();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error closing KDOL IWO Notice file in pre-edit. Status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport();
      ExitState = "FILE_OPEN_ERROR_AB";
    }
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

  private void UseLeEabReadKdolIwoNoticeInfo()
  {
    var useImport = new LeEabReadKdolIwoNoticeInfo.Import();
    var useExport = new LeEabReadKdolIwoNoticeInfo.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;
    useExport.KdolUiInboundFile.NewClaimantRecord =
      local.KdolUiInboundFile.NewClaimantRecord;

    Call(LeEabReadKdolIwoNoticeInfo.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    local.KdolUiInboundFile.NewClaimantRecord =
      useExport.KdolUiInboundFile.NewClaimantRecord;
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
    /// A value of KdolFileCreationDate.
    /// </summary>
    [JsonPropertyName("kdolFileCreationDate")]
    public DateWorkArea KdolFileCreationDate
    {
      get => kdolFileCreationDate ??= new();
      set => kdolFileCreationDate = value;
    }

    private DateWorkArea kdolFileCreationDate;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of KdolUiInboundFile.
    /// </summary>
    [JsonPropertyName("kdolUiInboundFile")]
    public KdolUiInboundFile KdolUiInboundFile
    {
      get => kdolUiInboundFile ??= new();
      set => kdolUiInboundFile = value;
    }

    /// <summary>
    /// A value of RecType3.
    /// </summary>
    [JsonPropertyName("recType3")]
    public Common RecType3
    {
      get => recType3 ??= new();
      set => recType3 = value;
    }

    /// <summary>
    /// A value of RecType2.
    /// </summary>
    [JsonPropertyName("recType2")]
    public Common RecType2
    {
      get => recType2 ??= new();
      set => recType2 = value;
    }

    /// <summary>
    /// A value of RecType1.
    /// </summary>
    [JsonPropertyName("recType1")]
    public Common RecType1
    {
      get => recType1 ??= new();
      set => recType1 = value;
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
    /// A value of FooterTotal.
    /// </summary>
    [JsonPropertyName("footerTotal")]
    public Common FooterTotal
    {
      get => footerTotal ??= new();
      set => footerTotal = value;
    }

    private KdolUiInboundFile kdolUiInboundFile;
    private Common recType3;
    private Common recType2;
    private Common recType1;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private Common footerTotal;
  }
#endregion
}
