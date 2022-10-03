// Program: SI_B292_WRITE_CONTROLS_AND_CLOSE, ID: 373440607, model: 746.
// Short name: SWE02748
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B292_WRITE_CONTROLS_AND_CLOSE.
/// </summary>
[Serializable]
public partial class SiB292WriteControlsAndClose: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B292_WRITE_CONTROLS_AND_CLOSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB292WriteControlsAndClose(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB292WriteControlsAndClose.
  /// </summary>
  public SiB292WriteControlsAndClose(IContext context, Import import,
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
    local.MaxControlTotal.Count = 99;

    // -----------------------------------------------------------------
    // WRITE CONTROL REPORT
    // -----------------------------------------------------------------
    local.EabFileHandling.Action = "WRITE";
    local.EabConvertNumeric.SendNonSuppressPos = 1;
    local.Subscript.Count = 1;

    for(var limit = local.MaxControlTotal.Count; local.Subscript.Count <= limit
      ; ++local.Subscript.Count)
    {
      local.Label.Text60 = "";
      local.EabConvertNumeric.SendAmount = "";
      local.EabConvertNumeric.ReturnNoCommasInNonDecimal = "";

      switch(local.Subscript.Count)
      {
        case 1:
          local.Label.Text60 = "CSENet transactions read";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.Read.Count, 15);
          UseEabConvertNumeric1();

          break;
        case 2:
          local.Label.Text60 = "CSENet transactions processed";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.Processed.Count, 15);
          UseEabConvertNumeric1();

          break;
        case 3:
          local.Label.Text60 =
            "CSENet transactions not processed due to errors";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.Erred.Count, 15);
          UseEabConvertNumeric1();

          break;
        default:
          goto AfterCycle;
      }

      local.EabReportSend.RptDetail = local.Label.Text60 + local
        .EabConvertNumeric.ReturnNoCommasInNonDecimal;
      UseCabControlReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabReportSend.RptDetail =
          "Error encountered writing to Control Report";
        UseCabErrorReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }
    }

AfterCycle:

    // -----------------------------------------------------------------
    // OPEN BUSINESS REPORT 01
    // -----------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    local.NeededToOpen.ProgramName = import.ProgramProcessingInfo.Name;
    local.NeededToOpen.ProcessDate = import.ProgramProcessingInfo.ProcessDate;
    local.NeededToOpen.BlankLineAfterHeading = "Y";
    local.NeededToOpen.BlankLineAfterColHead = "N";
    local.NeededToOpen.NumberOfColHeadings = 2;
    local.Label.Text11 = "";
    local.Column1.Text11 = "   COUNT";
    local.Column2.Text11 = "    CASE";
    local.Column3.Text11 = "   AP ID";
    local.Column4.Text11 = "   AP LOC";
    local.Column5.Text11 = "    PART";
    local.Column6.Text11 = "   ORDER";
    local.Column7.Text11 = "    COL";
    local.Column8.Text11 = "    MISC";
    local.NeededToOpen.ColHeading1 = local.Label.Text11 + local
      .Column1.Text11 + local.Column2.Text11 + local.Column3.Text11 + local
      .Column4.Text11 + local.Column5.Text11 + local.Column6.Text11 + local
      .Column7.Text11 + local.Column8.Text11;
    local.Label.Text11 = "----------";
    local.Column1.Text11 = "----------";
    local.Column2.Text11 = "----------";
    local.Column3.Text11 = "----------";
    local.Column4.Text11 = "----------";
    local.Column5.Text11 = "----------";
    local.Column6.Text11 = "----------";
    local.Column7.Text11 = "----------";
    local.Column8.Text11 = "----------";
    local.NeededToOpen.ColHeading2 = local.Label.Text11 + local
      .Column1.Text11 + local.Column2.Text11 + local.Column3.Text11 + local
      .Column4.Text11 + local.Column5.Text11 + local.Column6.Text11 + local
      .Column7.Text11 + local.Column8.Text11;
    UseCabBusinessReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabReportSend.RptDetail =
        "Error encountered opening Business Report 01";
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }
    else
    {
      // -----------------------------------------------------------------
      // WRITE BUSINESS REPORT 01
      // -----------------------------------------------------------------
      import.Totals.Index = 0;

      for(var limit = import.Totals.Count; import.Totals.Index < limit; ++
        import.Totals.Index)
      {
        if (!import.Totals.CheckSize())
        {
          break;
        }

        // -----------------------------------------------------------------
        // WRITE FUNCTIONAL TYPE CODE AND ACTION CODE (ENF-R)
        // -----------------------------------------------------------------
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          import.Totals.Item.GimportControls.FunctionalTypeCode + "-" + import
          .Totals.Item.GimportControls.ActionCode;
        UseCabBusinessReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "Error writing to Business Report 01";
          UseCabErrorReport1();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }

        // -----------------------------------------------------------------
        // WRITE NUMBER OF READS FOR THAT TYPE OF TRANSACTION (ENF-R)
        // -----------------------------------------------------------------
        local.Label.Text11 = "      READ";
        local.EabConvertNumeric.SendAmount =
          NumberToString(import.Totals.Item.GimportReads.Count, 15);
        UseEabConvertNumeric1();
        local.Column1.Text11 =
          Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 6, 10);
        local.EabReportSend.RptDetail = local.Label.Text11 + local
          .Column1.Text11;
        UseCabBusinessReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "Error writing to Business Report 01";
          UseCabErrorReport1();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }

        // -----------------------------------------------------------------
        // WRITE NUMBER OF RECORDS PROCESSED FOR THAT TYPE OF TRANSACTION (ENF-R
        // )
        // -----------------------------------------------------------------
        local.Label.Text11 = "   PROCESS";
        local.EabConvertNumeric.SendAmount =
          NumberToString(import.Totals.Item.GimportProcessed.Count, 15);
        UseEabConvertNumeric1();
        local.Column1.Text11 =
          Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 6, 10);
        local.EabReportSend.RptDetail = local.Label.Text11 + local
          .Column1.Text11;
        UseCabBusinessReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "Error writing to Business Report 01";
          UseCabErrorReport1();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }

        // -----------------------------------------------------------------
        // WRITE NUMBER OF RECORDS THAT ERRED FOR THAT TYPE OF TRANSACTION (ENF-
        // R)
        // -----------------------------------------------------------------
        local.Label.Text11 = "     ERROR";
        local.EabConvertNumeric.SendAmount =
          NumberToString(import.Totals.Item.GimportErrors.Count, 15);
        UseEabConvertNumeric1();
        local.Column1.Text11 =
          Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 6, 10);
        local.EabReportSend.RptDetail = local.Label.Text11 + local
          .Column1.Text11;
        UseCabBusinessReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "Error writing to Business Report 01";
          UseCabErrorReport1();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }

        // -----------------------------------------------------------------
        // WRITE BLANK LINE
        // -----------------------------------------------------------------
        local.EabReportSend.RptDetail = "";
        UseCabBusinessReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "Error writing to Business Report 01";
          UseCabErrorReport1();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }

        import.Totals.Item.SubGroupTotals.Index = -1;

        // -----------------------------------------------------------------
        // WRITE TOTAL CREATES PER DATABLOCK FOR THAT TYPE OF TRANSACTION (ENF-R
        // )
        // -----------------------------------------------------------------
        local.Label.Text11 = "    CREATE";
        local.Column1.Text11 = "";
        local.Column2.Text11 = "";

        for(local.Subscript.Count = 3; local.Subscript.Count <= 8; ++
          local.Subscript.Count)
        {
          ++import.Totals.Item.SubGroupTotals.Index;
          import.Totals.Item.SubGroupTotals.CheckSize();

          local.EabConvertNumeric.SendAmount =
            NumberToString(import.Totals.Item.SubGroupTotals.Item.GimportSub.
              Count, 15);
          UseEabConvertNumeric1();

          switch(local.Subscript.Count)
          {
            case 3:
              local.Column3.Text11 =
                Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 6,
                10);

              break;
            case 4:
              local.Column4.Text11 =
                Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 6,
                10);

              break;
            case 5:
              local.Column5.Text11 =
                Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 6,
                10);

              break;
            case 6:
              local.Column6.Text11 =
                Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 6,
                10);

              break;
            case 7:
              local.Column7.Text11 =
                Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 6,
                10);

              break;
            case 8:
              local.Column8.Text11 =
                Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 6,
                10);

              break;
            default:
              break;
          }
        }

        local.EabReportSend.RptDetail = local.Label.Text11 + local
          .Column1.Text11 + local.Column2.Text11 + local.Column3.Text11 + local
          .Column4.Text11 + local.Column5.Text11 + local.Column6.Text11 + local
          .Column7.Text11 + local.Column8.Text11;
        UseCabBusinessReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "Error writing to Business Report 01";
          UseCabErrorReport1();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }

        // -----------------------------------------------------------------
        // WRITE TOTAL UPDATES PER DATABLOCK FOR THAT TYPE OF TRANSACTION (ENF-R
        // )
        // -----------------------------------------------------------------
        local.Label.Text11 = "    UPDATE";
        local.Column1.Text11 = "";

        for(local.Subscript.Count = 2; local.Subscript.Count <= 8; ++
          local.Subscript.Count)
        {
          ++import.Totals.Item.SubGroupTotals.Index;
          import.Totals.Item.SubGroupTotals.CheckSize();

          local.EabConvertNumeric.SendAmount =
            NumberToString(import.Totals.Item.SubGroupTotals.Item.GimportSub.
              Count, 15);
          UseEabConvertNumeric1();

          switch(local.Subscript.Count)
          {
            case 2:
              local.Column2.Text11 =
                Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 6,
                10);

              break;
            case 3:
              local.Column3.Text11 =
                Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 6,
                10);

              break;
            case 4:
              local.Column4.Text11 =
                Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 6,
                10);

              break;
            case 5:
              local.Column5.Text11 =
                Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 6,
                10);

              break;
            case 6:
              local.Column6.Text11 =
                Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 6,
                10);

              break;
            case 7:
              local.Column7.Text11 =
                Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 6,
                10);

              break;
            case 8:
              local.Column8.Text11 =
                Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 6,
                10);

              break;
            default:
              break;
          }
        }

        local.EabReportSend.RptDetail = local.Label.Text11 + local
          .Column1.Text11 + local.Column2.Text11 + local.Column3.Text11 + local
          .Column4.Text11 + local.Column5.Text11 + local.Column6.Text11 + local
          .Column7.Text11 + local.Column8.Text11;
        UseCabBusinessReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "Error writing to Business Report 01";
          UseCabErrorReport1();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }

        // -----------------------------------------------------------------
        // WRITE TOTAL DELETES PER DATABLOCK FOR THAT TYPE OF TRANSACTION (ENF-R
        // )
        // -----------------------------------------------------------------
        local.Label.Text11 = "    DELETE";
        local.Column1.Text11 = "";
        local.Column2.Text11 = "";

        for(local.Subscript.Count = 3; local.Subscript.Count <= 8; ++
          local.Subscript.Count)
        {
          ++import.Totals.Item.SubGroupTotals.Index;
          import.Totals.Item.SubGroupTotals.CheckSize();

          local.EabConvertNumeric.SendAmount =
            NumberToString(import.Totals.Item.SubGroupTotals.Item.GimportSub.
              Count, 15);
          UseEabConvertNumeric1();

          switch(local.Subscript.Count)
          {
            case 3:
              local.Column3.Text11 =
                Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 6,
                10);

              break;
            case 4:
              local.Column4.Text11 =
                Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 6,
                10);

              break;
            case 5:
              local.Column5.Text11 =
                Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 6,
                10);

              break;
            case 6:
              local.Column6.Text11 =
                Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 6,
                10);

              break;
            case 7:
              local.Column7.Text11 =
                Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 6,
                10);

              break;
            case 8:
              local.Column8.Text11 =
                Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 6,
                10);

              break;
            default:
              break;
          }
        }

        local.EabReportSend.RptDetail = local.Label.Text11 + local
          .Column1.Text11 + local.Column2.Text11 + local.Column3.Text11 + local
          .Column4.Text11 + local.Column5.Text11 + local.Column6.Text11 + local
          .Column7.Text11 + local.Column8.Text11;
        UseCabBusinessReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "Error writing to Business Report 01";
          UseCabErrorReport1();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }

        // -----------------------------------------------------------------
        // WRITE A PAGE BREAK UNLESS THIS IS THE LAST PAGE
        // -----------------------------------------------------------------
        if (import.Totals.Index + 1 < import.Totals.Count)
        {
          local.EabFileHandling.Action = "NEWPAGE";
          UseCabBusinessReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Error writing NEWPAGE to Business Report 01";
            UseCabErrorReport1();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }
          }
        }
      }

      import.Totals.CheckIndex();

      // -----------------------------------------------------------------
      // CLOSE BUSINESS REPORT 01
      // -----------------------------------------------------------------
      local.EabFileHandling.Action = "CLOSE";
      UseCabBusinessReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "Error closing Business Report 01";
        UseCabErrorReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }
    }

    // -----------------------------------------------------------------
    // CLOSE ADABAS
    // -----------------------------------------------------------------
    local.CsePersonsWorkSet.Number = "CLOSE";
    UseEabReadCsePersonBatch();

    // -----------------------------------------------------------------
    // CLOSE CONTROL REPORT
    // -----------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error closing Control Report";
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    // -----------------------------------------------------------------
    // CLOSE ERROR REPORT
    // -----------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_CLOSING_EXT_FILE";
    }
  }

  private static void MoveEabConvertNumeric2(EabConvertNumeric2 source,
    EabConvertNumeric2 target)
  {
    target.SendAmount = source.SendAmount;
    target.SendNonSuppressPos = source.SendNonSuppressPos;
  }

  private void UseCabBusinessReport1()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport2()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport3()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToOpen.Assign(local.NeededToOpen);

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
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

  private void UseEabConvertNumeric1()
  {
    var useImport = new EabConvertNumeric1.Import();
    var useExport = new EabConvertNumeric1.Export();

    MoveEabConvertNumeric2(local.EabConvertNumeric, useImport.EabConvertNumeric);
      
    useExport.EabConvertNumeric.ReturnNoCommasInNonDecimal =
      local.EabConvertNumeric.ReturnNoCommasInNonDecimal;

    Call(EabConvertNumeric1.Execute, useImport, useExport);

    local.EabConvertNumeric.ReturnNoCommasInNonDecimal =
      useExport.EabConvertNumeric.ReturnNoCommasInNonDecimal;
  }

  private void UseEabReadCsePersonBatch()
  {
    var useImport = new EabReadCsePersonBatch.Import();
    var useExport = new EabReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(EabReadCsePersonBatch.Execute, useImport, useExport);
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
    /// <summary>A TotalsGroup group.</summary>
    [Serializable]
    public class TotalsGroup
    {
      /// <summary>
      /// A value of GimportReads.
      /// </summary>
      [JsonPropertyName("gimportReads")]
      public Common GimportReads
      {
        get => gimportReads ??= new();
        set => gimportReads = value;
      }

      /// <summary>
      /// A value of GimportProcessed.
      /// </summary>
      [JsonPropertyName("gimportProcessed")]
      public Common GimportProcessed
      {
        get => gimportProcessed ??= new();
        set => gimportProcessed = value;
      }

      /// <summary>
      /// A value of GimportErrors.
      /// </summary>
      [JsonPropertyName("gimportErrors")]
      public Common GimportErrors
      {
        get => gimportErrors ??= new();
        set => gimportErrors = value;
      }

      /// <summary>
      /// A value of GimportControls.
      /// </summary>
      [JsonPropertyName("gimportControls")]
      public InterstateCase GimportControls
      {
        get => gimportControls ??= new();
        set => gimportControls = value;
      }

      /// <summary>
      /// Gets a value of SubGroupTotals.
      /// </summary>
      [JsonIgnore]
      public Array<SubGroupTotalsGroup> SubGroupTotals =>
        subGroupTotals ??= new(SubGroupTotalsGroup.Capacity, 0);

      /// <summary>
      /// Gets a value of SubGroupTotals for json serialization.
      /// </summary>
      [JsonPropertyName("subGroupTotals")]
      [Computed]
      public IList<SubGroupTotalsGroup> SubGroupTotals_Json
      {
        get => subGroupTotals;
        set => SubGroupTotals.Assign(value);
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 40;

      private Common gimportReads;
      private Common gimportProcessed;
      private Common gimportErrors;
      private InterstateCase gimportControls;
      private Array<SubGroupTotalsGroup> subGroupTotals;
    }

    /// <summary>A SubGroupTotalsGroup group.</summary>
    [Serializable]
    public class SubGroupTotalsGroup
    {
      /// <summary>
      /// A value of GimportSub.
      /// </summary>
      [JsonPropertyName("gimportSub")]
      public Common GimportSub
      {
        get => gimportSub ??= new();
        set => gimportSub = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private Common gimportSub;
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
    /// A value of Read.
    /// </summary>
    [JsonPropertyName("read")]
    public Common Read
    {
      get => read ??= new();
      set => read = value;
    }

    /// <summary>
    /// A value of Processed.
    /// </summary>
    [JsonPropertyName("processed")]
    public Common Processed
    {
      get => processed ??= new();
      set => processed = value;
    }

    /// <summary>
    /// A value of Erred.
    /// </summary>
    [JsonPropertyName("erred")]
    public Common Erred
    {
      get => erred ??= new();
      set => erred = value;
    }

    /// <summary>
    /// Gets a value of Totals.
    /// </summary>
    [JsonIgnore]
    public Array<TotalsGroup> Totals => totals ??= new(TotalsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Totals for json serialization.
    /// </summary>
    [JsonPropertyName("totals")]
    [Computed]
    public IList<TotalsGroup> Totals_Json
    {
      get => totals;
      set => Totals.Assign(value);
    }

    private ProgramProcessingInfo programProcessingInfo;
    private Common read;
    private Common processed;
    private Common erred;
    private Array<TotalsGroup> totals;
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

    /// <summary>
    /// A value of EabConvertNumeric.
    /// </summary>
    [JsonPropertyName("eabConvertNumeric")]
    public EabConvertNumeric2 EabConvertNumeric
    {
      get => eabConvertNumeric ??= new();
      set => eabConvertNumeric = value;
    }

    /// <summary>
    /// A value of Textnum.
    /// </summary>
    [JsonPropertyName("textnum")]
    public WorkArea Textnum
    {
      get => textnum ??= new();
      set => textnum = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of MaxControlTotal.
    /// </summary>
    [JsonPropertyName("maxControlTotal")]
    public Common MaxControlTotal
    {
      get => maxControlTotal ??= new();
      set => maxControlTotal = value;
    }

    /// <summary>
    /// A value of Subscript.
    /// </summary>
    [JsonPropertyName("subscript")]
    public Common Subscript
    {
      get => subscript ??= new();
      set => subscript = value;
    }

    /// <summary>
    /// A value of Label.
    /// </summary>
    [JsonPropertyName("label")]
    public WorkArea Label
    {
      get => label ??= new();
      set => label = value;
    }

    /// <summary>
    /// A value of Column1.
    /// </summary>
    [JsonPropertyName("column1")]
    public WorkArea Column1
    {
      get => column1 ??= new();
      set => column1 = value;
    }

    /// <summary>
    /// A value of Column2.
    /// </summary>
    [JsonPropertyName("column2")]
    public WorkArea Column2
    {
      get => column2 ??= new();
      set => column2 = value;
    }

    /// <summary>
    /// A value of Column3.
    /// </summary>
    [JsonPropertyName("column3")]
    public WorkArea Column3
    {
      get => column3 ??= new();
      set => column3 = value;
    }

    /// <summary>
    /// A value of Column4.
    /// </summary>
    [JsonPropertyName("column4")]
    public WorkArea Column4
    {
      get => column4 ??= new();
      set => column4 = value;
    }

    /// <summary>
    /// A value of Column5.
    /// </summary>
    [JsonPropertyName("column5")]
    public WorkArea Column5
    {
      get => column5 ??= new();
      set => column5 = value;
    }

    /// <summary>
    /// A value of Column6.
    /// </summary>
    [JsonPropertyName("column6")]
    public WorkArea Column6
    {
      get => column6 ??= new();
      set => column6 = value;
    }

    /// <summary>
    /// A value of Column7.
    /// </summary>
    [JsonPropertyName("column7")]
    public WorkArea Column7
    {
      get => column7 ??= new();
      set => column7 = value;
    }

    /// <summary>
    /// A value of Column8.
    /// </summary>
    [JsonPropertyName("column8")]
    public WorkArea Column8
    {
      get => column8 ??= new();
      set => column8 = value;
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

    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private EabConvertNumeric2 eabConvertNumeric;
    private WorkArea textnum;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common maxControlTotal;
    private Common subscript;
    private WorkArea label;
    private WorkArea column1;
    private WorkArea column2;
    private WorkArea column3;
    private WorkArea column4;
    private WorkArea column5;
    private WorkArea column6;
    private WorkArea column7;
    private WorkArea column8;
    private EabReportSend neededToOpen;
  }
#endregion
}
