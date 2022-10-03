// Program: SP_B714_WRITE_CONTROLS_AND_CLOSE, ID: 373371591, model: 746.
// Short name: SWE02763
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_B714_WRITE_CONTROLS_AND_CLOSE.
/// </summary>
[Serializable]
public partial class SpB714WriteControlsAndClose: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B714_WRITE_CONTROLS_AND_CLOSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB714WriteControlsAndClose(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB714WriteControlsAndClose.
  /// </summary>
  public SpB714WriteControlsAndClose(IContext context, Import import,
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

    // -------------------------------------------------------
    // WRITE OUTPUT CONTROL REPORT
    // -------------------------------------------------------
    local.EabFileHandling.Action = "WRITE";
    local.EabConvertNumeric.SendNonSuppressPos = 1;
    import.DocumentTotals.Index = -1;
    local.Subscript.Count = 1;

    for(var limit = local.MaxControlTotal.Count; local.Subscript.Count <= limit
      ; ++local.Subscript.Count)
    {
      local.EabReportSend.RptDetail = "";
      local.Label.Text40 = "";
      local.Label.Text20 = "";
      local.Document.Name = "";
      local.EabConvertNumeric.SendAmount = "";
      local.EabConvertNumeric.ReturnNoCommasInNonDecimal = "";

      switch(local.Subscript.Count)
      {
        case 1:
          local.Label.Text40 = "DOCUMENTS READ";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.DocsRead.Count, 15);

          break;
        case 2:
          local.Label.Text40 = "DOCUMENTS PROCESSED SUCCESSFULLY";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.DocsProcessed.Count, 15);

          break;
        case 3:
          local.Label.Text40 = "DOCUMENTS UNPROCESSED (FUTURE DATE)";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.DocsUnprocessedFuture.Count, 15);

          break;
        case 4:
          local.Label.Text40 = "DOCUMENTS UNPROCESSED (EXCEPTION)";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.DocsException.Count, 15);

          break;
        case 5:
          local.Label.Text40 = "DOCUMENTS UNPROCESSED (DATA ERROR)";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.DocsDataError.Count, 15);

          break;
        case 6:
          local.Label.Text40 = "DOCUMENTS UNPROCESSED (SYSTEM ERROR)";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.DocsSystemError.Count, 15);

          break;
        case 7:
          local.Label.Text40 = "DOCUMENTS WITH SYSTEM WARNINGS";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.DocsWarning.Count, 15);

          break;
        case 8:
          // ----------------
          // BLANK LINE
          // ----------------
          break;
        case 9:
          local.Label.Text20 = "     DOCUMENT.VERSION";
          local.EabReportSend.RptDetail = local.Label.Text20 + "           READ      PROCESSED         FUTURE      EXCEPTION     DATA ERROR   SYSTEM ERROR        WARNING";
            
          local.Label.Text20 = "";

          break;
        case 10:
          if (import.DocumentTotals.Index + 1 >= import.DocumentTotals.Count)
          {
            goto AfterCycle;
          }

          // mjr
          // ------------------------------------------
          // Repeat case next time
          // ---------------------------------------------
          --local.Subscript.Count;

          ++import.DocumentTotals.Index;
          import.DocumentTotals.CheckSize();

          MoveDocument(import.DocumentTotals.Item.G, local.Document);

          break;
        default:
          goto AfterCycle;
      }

      if (!IsEmpty(local.Label.Text40))
      {
        UseEabConvertNumeric1();
        local.EabReportSend.RptDetail = local.Label.Text40 + local
          .EabConvertNumeric.ReturnNoCommasInNonDecimal;
      }
      else if (!IsEmpty(local.Document.Name))
      {
        local.Label.Text20 = "     " + local.Document.Name + "." + local
          .Document.VersionNumber;
        local.EabConvertNumeric.SendAmount =
          NumberToString(import.DocumentTotals.Item.GimportRead.Count, 15);
        UseEabConvertNumeric1();
        local.EabReportSend.RptDetail = local.Label.Text20 + local
          .EabConvertNumeric.ReturnNoCommasInNonDecimal;
        local.EabConvertNumeric.SendAmount =
          NumberToString(import.DocumentTotals.Item.GimportProcessed.Count, 15);
          
        UseEabConvertNumeric1();
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + local
          .EabConvertNumeric.ReturnNoCommasInNonDecimal;
        local.EabConvertNumeric.SendAmount =
          NumberToString(import.DocumentTotals.Item.GimportFuture.Count, 15);
        UseEabConvertNumeric1();
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + local
          .EabConvertNumeric.ReturnNoCommasInNonDecimal;
        local.EabConvertNumeric.SendAmount =
          NumberToString(import.DocumentTotals.Item.GimportException.Count, 15);
          
        UseEabConvertNumeric1();
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + local
          .EabConvertNumeric.ReturnNoCommasInNonDecimal;
        local.EabConvertNumeric.SendAmount =
          NumberToString(import.DocumentTotals.Item.GimportDataError.Count, 15);
          
        UseEabConvertNumeric1();
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + local
          .EabConvertNumeric.ReturnNoCommasInNonDecimal;
        local.EabConvertNumeric.SendAmount =
          NumberToString(import.DocumentTotals.Item.GimportSystemError.Count, 15);
          
        UseEabConvertNumeric1();
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + local
          .EabConvertNumeric.ReturnNoCommasInNonDecimal;
        local.EabConvertNumeric.SendAmount =
          NumberToString(import.DocumentTotals.Item.GimportWarning.Count, 15);
        UseEabConvertNumeric1();
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + local
          .EabConvertNumeric.ReturnNoCommasInNonDecimal;
      }
      else
      {
      }

      UseCabControlReport1();

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

        local.EabReportSend.RptDetail = "";
      }
    }

AfterCycle:

    // -------------------------------------------------------
    // CLOSE OUTPUT REPORT 01 & 02
    // -------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseSpEabWriteDocument();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_CLOSING_EXT_FILE";

      return;
    }

    // -------------------------------------------------------
    // CLOSE OUTPUT CONTROL REPORT
    // -------------------------------------------------------
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_CLOSING_EXT_FILE";

      return;
    }

    // --------------------------------------------------------
    // CLOSE OUTPUT ERROR REPORT
    // --------------------------------------------------------
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_CLOSING_EXT_FILE";
    }
  }

  private static void MoveDocument(Document source, Document target)
  {
    target.Name = source.Name;
    target.VersionNumber = source.VersionNumber;
  }

  private static void MoveEabConvertNumeric2(EabConvertNumeric2 source,
    EabConvertNumeric2 target)
  {
    target.SendAmount = source.SendAmount;
    target.SendNonSuppressPos = source.SendNonSuppressPos;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

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

  private void UseSpEabWriteDocument()
  {
    var useImport = new SpEabWriteDocument.Import();
    var useExport = new SpEabWriteDocument.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(SpEabWriteDocument.Execute, useImport, useExport);

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
    /// <summary>A DocumentTotalsGroup group.</summary>
    [Serializable]
    public class DocumentTotalsGroup
    {
      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public Document G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>
      /// A value of GimportRead.
      /// </summary>
      [JsonPropertyName("gimportRead")]
      public Common GimportRead
      {
        get => gimportRead ??= new();
        set => gimportRead = value;
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
      /// A value of GimportFuture.
      /// </summary>
      [JsonPropertyName("gimportFuture")]
      public Common GimportFuture
      {
        get => gimportFuture ??= new();
        set => gimportFuture = value;
      }

      /// <summary>
      /// A value of GimportException.
      /// </summary>
      [JsonPropertyName("gimportException")]
      public Common GimportException
      {
        get => gimportException ??= new();
        set => gimportException = value;
      }

      /// <summary>
      /// A value of GimportDataError.
      /// </summary>
      [JsonPropertyName("gimportDataError")]
      public Common GimportDataError
      {
        get => gimportDataError ??= new();
        set => gimportDataError = value;
      }

      /// <summary>
      /// A value of GimportSystemError.
      /// </summary>
      [JsonPropertyName("gimportSystemError")]
      public Common GimportSystemError
      {
        get => gimportSystemError ??= new();
        set => gimportSystemError = value;
      }

      /// <summary>
      /// A value of GimportWarning.
      /// </summary>
      [JsonPropertyName("gimportWarning")]
      public Common GimportWarning
      {
        get => gimportWarning ??= new();
        set => gimportWarning = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Document g;
      private Common gimportRead;
      private Common gimportProcessed;
      private Common gimportFuture;
      private Common gimportException;
      private Common gimportDataError;
      private Common gimportSystemError;
      private Common gimportWarning;
    }

    /// <summary>
    /// A value of DocsWarning.
    /// </summary>
    [JsonPropertyName("docsWarning")]
    public Common DocsWarning
    {
      get => docsWarning ??= new();
      set => docsWarning = value;
    }

    /// <summary>
    /// A value of DocsRead.
    /// </summary>
    [JsonPropertyName("docsRead")]
    public Common DocsRead
    {
      get => docsRead ??= new();
      set => docsRead = value;
    }

    /// <summary>
    /// A value of DocsProcessed.
    /// </summary>
    [JsonPropertyName("docsProcessed")]
    public Common DocsProcessed
    {
      get => docsProcessed ??= new();
      set => docsProcessed = value;
    }

    /// <summary>
    /// A value of DocsDataError.
    /// </summary>
    [JsonPropertyName("docsDataError")]
    public Common DocsDataError
    {
      get => docsDataError ??= new();
      set => docsDataError = value;
    }

    /// <summary>
    /// A value of DocsSystemError.
    /// </summary>
    [JsonPropertyName("docsSystemError")]
    public Common DocsSystemError
    {
      get => docsSystemError ??= new();
      set => docsSystemError = value;
    }

    /// <summary>
    /// A value of DocsUnprocessedFuture.
    /// </summary>
    [JsonPropertyName("docsUnprocessedFuture")]
    public Common DocsUnprocessedFuture
    {
      get => docsUnprocessedFuture ??= new();
      set => docsUnprocessedFuture = value;
    }

    /// <summary>
    /// A value of DocsException.
    /// </summary>
    [JsonPropertyName("docsException")]
    public Common DocsException
    {
      get => docsException ??= new();
      set => docsException = value;
    }

    /// <summary>
    /// Gets a value of DocumentTotals.
    /// </summary>
    [JsonIgnore]
    public Array<DocumentTotalsGroup> DocumentTotals => documentTotals ??= new(
      DocumentTotalsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of DocumentTotals for json serialization.
    /// </summary>
    [JsonPropertyName("documentTotals")]
    [Computed]
    public IList<DocumentTotalsGroup> DocumentTotals_Json
    {
      get => documentTotals;
      set => DocumentTotals.Assign(value);
    }

    private Common docsWarning;
    private Common docsRead;
    private Common docsProcessed;
    private Common docsDataError;
    private Common docsSystemError;
    private Common docsUnprocessedFuture;
    private Common docsException;
    private Array<DocumentTotalsGroup> documentTotals;
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
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
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
    /// A value of EabConvertNumeric.
    /// </summary>
    [JsonPropertyName("eabConvertNumeric")]
    public EabConvertNumeric2 EabConvertNumeric
    {
      get => eabConvertNumeric ??= new();
      set => eabConvertNumeric = value;
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

    private Document document;
    private WorkArea label;
    private EabConvertNumeric2 eabConvertNumeric;
    private Common maxControlTotal;
    private Common subscript;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
  }
#endregion
}
