// Program: LE_B578_PRE_EDIT_DOL_FILE, ID: 945096065, model: 746.
// Short name: SWE03075
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_B578_PRE_EDIT_DOL_FILE.
/// </summary>
[Serializable]
public partial class LeB578PreEditDolFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B578_PRE_EDIT_DOL_FILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB578PreEditDolFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB578PreEditDolFile.
  /// </summary>
  public LeB578PreEditDolFile(IContext context, Import import, Export export):
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
    // -- Open the UI offset file from KDOL.
    // ------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    UseLeEabReadKdolUiOffsetInfo2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error opening KDOL UI offset input file in pre-edit. Status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport();
      ExitState = "FILE_OPEN_ERROR_AB";

      return;
    }

    // ------------------------------------------------------------------------------
    // -- Process each withholding record returned from DOL.
    // ------------------------------------------------------------------------------
    do
    {
      // ------------------------------------------------------------------------------
      // -- Read a withholding record returned from DOL.
      // ------------------------------------------------------------------------------
      local.EabFileHandling.Action = "READ";
      UseLeEabReadKdolUiOffsetInfo1();

      switch(TrimEnd(local.EabFileHandling.Status))
      {
        case "OK":
          ++local.RecordsRead.Count;

          break;
        case "EF":
          continue;
        default:
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error reading KDOL UI withholding file in pre-edit. Status = " + local
            .EabFileHandling.Status;
          UseCabErrorReport();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
      }

      switch(TrimEnd(
        Substring(local.KdolUiInboundFile.UiWithholdingRecord, 1, 1)))
      {
        case "1":
          // -- Header record
          ++local.RecType1.Count;

          // -- Validate that the file creation date (positions 2 - 9) is 
          // numeric.
          if (Verify(Substring(
            local.KdolUiInboundFile.UiWithholdingRecord,
            KdolUiInboundFile.UiWithholdingRecord_MaxLength, 2, 8),
            "0123456789") != 0)
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "KDOL File creation date (Position 2 - 9) is not numeric: " + local
              .KdolUiInboundFile.UiWithholdingRecord;
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }
          else
          {
            // -- Export the file creation date for checkpoint/restart purposes.
            export.KdolFileCreationDate.TextDate =
              Substring(local.KdolUiInboundFile.UiWithholdingRecord, 2, 8);
            export.KdolFileCreationDate.Date =
              StringToDate(Substring(
                export.KdolFileCreationDate.TextDate,
              DateWorkArea.TextDate_MaxLength, 1, 4) + "-" + Substring
              (export.KdolFileCreationDate.TextDate,
              DateWorkArea.TextDate_MaxLength, 5, 2) + "-" + Substring
              (export.KdolFileCreationDate.TextDate,
              DateWorkArea.TextDate_MaxLength, 7, 2));
          }

          break;
        case "2":
          // -- Detail record
          ++local.RecType2.Count;

          // -- Validate that SSN (positions 2 - 10) is numeric.
          if (Verify(Substring(
            local.KdolUiInboundFile.UiWithholdingRecord,
            KdolUiInboundFile.UiWithholdingRecord_MaxLength, 2, 9),
            "0123456789") != 0)
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "SSN (Position 2 - 10) is not numeric: " + local
              .KdolUiInboundFile.UiWithholdingRecord;
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          // -- Validate that Account Number (cse person number, positions 56 - 
          // 65) is numeric.
          if (Verify(Substring(
            local.KdolUiInboundFile.UiWithholdingRecord,
            KdolUiInboundFile.UiWithholdingRecord_MaxLength, 56, 10),
            "0123456789") != 0)
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "CSE Person Number (Position 56 - 65) is not numeric: " + local
              .KdolUiInboundFile.UiWithholdingRecord;
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          // -- Validate that KDOL payment date (positions 71 - 78) is numeric.
          if (Verify(Substring(
            local.KdolUiInboundFile.UiWithholdingRecord,
            KdolUiInboundFile.UiWithholdingRecord_MaxLength, 71, 8),
            "0123456789") != 0)
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "KDOL Payment Date (Position 71 - 78) is not numeric: " + local
              .KdolUiInboundFile.UiWithholdingRecord;
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          // -- Validate that Collection Amount (positions 81 - 88) is numeric.
          if (Verify(Substring(
            local.KdolUiInboundFile.UiWithholdingRecord,
            KdolUiInboundFile.UiWithholdingRecord_MaxLength, 81, 8),
            "0123456789") != 0)
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Collection Amount (Position 81 - 88) is not numeric: " + local
              .KdolUiInboundFile.UiWithholdingRecord;
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          // -- Keep a running total of the collection amounts.
          local.RecType2.TotalInteger += StringToNumber(
            Substring(local.KdolUiInboundFile.UiWithholdingRecord,
            KdolUiInboundFile.UiWithholdingRecord_MaxLength, 81, 8));

          // -- Validate that CSE Extract Date (positions 91 - 98) is numeric.
          if (Verify(Substring(
            local.KdolUiInboundFile.UiWithholdingRecord,
            KdolUiInboundFile.UiWithholdingRecord_MaxLength, 91, 8),
            "0123456789") != 0)
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "CSE Extract Date (Position 91 - 98) is not numeric: " + local
              .KdolUiInboundFile.UiWithholdingRecord;
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
            local.KdolUiInboundFile.UiWithholdingRecord,
            KdolUiInboundFile.UiWithholdingRecord_MaxLength, 3, 10),
            "0123456789") != 0)
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Footer Record Total Detail Record Count (Position 3 - 12) is not numeric: " +
              local.KdolUiInboundFile.UiWithholdingRecord;
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          // -- Validate that Total Detail Record Amount (positions 14 - 28) is 
          // numeric.
          if (Verify(Substring(
            local.KdolUiInboundFile.UiWithholdingRecord,
            KdolUiInboundFile.UiWithholdingRecord_MaxLength, 14, 15),
            "0123456789") != 0)
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Footer Record Total Detail Record Amount (Position 14 - 28) is not numeric: " +
              local.KdolUiInboundFile.UiWithholdingRecord;
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          // -- Extract the footer total detail count and total detail amount.
          local.FooterTotal.Count =
            (int)StringToNumber(Substring(
              local.KdolUiInboundFile.UiWithholdingRecord,
            KdolUiInboundFile.UiWithholdingRecord_MaxLength, 3, 10));
          local.FooterTotal.TotalInteger =
            StringToNumber(Substring(
              local.KdolUiInboundFile.UiWithholdingRecord,
            KdolUiInboundFile.UiWithholdingRecord_MaxLength, 14, 15));

          break;
        default:
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Invalid record type (Position 1): " + local
            .KdolUiInboundFile.UiWithholdingRecord;
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
        // -- File contains one footer record.  Continue.
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
      local.EabReportSend.RptDetail = "Footer record detail count " + NumberToString
        (local.FooterTotal.Count, 1, 15) + " does not match calculated detail record count " +
        NumberToString(local.RecType2.Count, 15);
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }

    if (local.FooterTotal.TotalInteger != local.RecType2.TotalInteger)
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Footer record detail amount " + NumberToString
        (local.FooterTotal.TotalInteger, 1, 15) + " does not match calculated detail record amount " +
        NumberToString(local.RecType2.TotalInteger, 15);
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ------------------------------------------------------------------------------
    // -- Close the UI IWO Notice file from KDOL.
    // ------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseLeEabReadKdolUiOffsetInfo2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error closing KDOL UI offset input file in pre-edit. Status = " + local
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

  private void UseLeEabReadKdolUiOffsetInfo1()
  {
    var useImport = new LeEabReadKdolUiOffsetInfo.Import();
    var useExport = new LeEabReadKdolUiOffsetInfo.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;
    useExport.KdolUiInboundFile.UiWithholdingRecord =
      local.KdolUiInboundFile.UiWithholdingRecord;

    Call(LeEabReadKdolUiOffsetInfo.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    local.KdolUiInboundFile.UiWithholdingRecord =
      useExport.KdolUiInboundFile.UiWithholdingRecord;
  }

  private void UseLeEabReadKdolUiOffsetInfo2()
  {
    var useImport = new LeEabReadKdolUiOffsetInfo.Import();
    var useExport = new LeEabReadKdolUiOffsetInfo.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(LeEabReadKdolUiOffsetInfo.Execute, useImport, useExport);

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

    /// <summary>
    /// A value of Kdol.
    /// </summary>
    [JsonPropertyName("kdol")]
    public Common Kdol
    {
      get => kdol ??= new();
      set => kdol = value;
    }

    private DateWorkArea kdolFileCreationDate;
    private Common kdol;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of FooterTotal.
    /// </summary>
    [JsonPropertyName("footerTotal")]
    public Common FooterTotal
    {
      get => footerTotal ??= new();
      set => footerTotal = value;
    }

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
    /// A value of RecordsRead.
    /// </summary>
    [JsonPropertyName("recordsRead")]
    public Common RecordsRead
    {
      get => recordsRead ??= new();
      set => recordsRead = value;
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
    /// A value of RecType2.
    /// </summary>
    [JsonPropertyName("recType2")]
    public Common RecType2
    {
      get => recType2 ??= new();
      set => recType2 = value;
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

    private Common footerTotal;
    private KdolUiInboundFile kdolUiInboundFile;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private Common recordsRead;
    private Common recType1;
    private Common recType2;
    private Common recType3;
  }
#endregion
}
