// Program: SP_B707_WRITE_CONTROLS_AND_CLOSE, ID: 374481999, model: 746.
// Short name: SWE02617
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_B707_WRITE_CONTROLS_AND_CLOSE.
/// </summary>
[Serializable]
public partial class SpB707WriteControlsAndClose: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B707_WRITE_CONTROLS_AND_CLOSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB707WriteControlsAndClose(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB707WriteControlsAndClose.
  /// </summary>
  public SpB707WriteControlsAndClose(IContext context, Import import,
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
    local.Subscript.Count = 1;

    for(var limit = local.MaxControlTotal.Count; local.Subscript.Count <= limit
      ; ++local.Subscript.Count)
    {
      local.LabelColumn.Text40 = "";
      local.AllColumn.Text20 = "";
      local.ApColumn.Text20 = "";
      local.ArColumn.Text20 = "";
      local.EabConvertNumeric.SendAmount = "";
      local.EabConvertNumeric.ReturnNoCommasInNonDecimal = "";

      switch(local.Subscript.Count)
      {
        case 1:
          local.AllColumn.Text20 = "        TOTAL";
          local.ApColumn.Text20 = "         AP";
          local.ArColumn.Text20 = "         AR";

          break;
        case 2:
          local.LabelColumn.Text40 = "CASES READ";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.CasesRead.Count, 15);
          UseEabConvertNumeric1();
          local.AllColumn.Text20 =
            local.EabConvertNumeric.ReturnNoCommasInNonDecimal;

          break;
        case 3:
          local.LabelColumn.Text40 = "DOCUMENTS ATTEMPTED";
          local.EabConvertNumeric.SendAmount =
            NumberToString((long)import.CasesRead.Count * 2, 15);
          UseEabConvertNumeric1();
          local.AllColumn.Text20 =
            local.EabConvertNumeric.ReturnNoCommasInNonDecimal;
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.CasesRead.Count, 15);
          UseEabConvertNumeric1();
          local.ApColumn.Text20 =
            local.EabConvertNumeric.ReturnNoCommasInNonDecimal;
          local.ArColumn.Text20 =
            local.EabConvertNumeric.ReturnNoCommasInNonDecimal;

          break;
        case 4:
          local.LabelColumn.Text40 = "DOCUMENTS CREATED";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.AllDocCreates.Count, 15);
          UseEabConvertNumeric1();
          local.AllColumn.Text20 =
            local.EabConvertNumeric.ReturnNoCommasInNonDecimal;
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.ApDocCreates.Count, 15);
          UseEabConvertNumeric1();
          local.ApColumn.Text20 =
            local.EabConvertNumeric.ReturnNoCommasInNonDecimal;
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.ArDocCreates.Count, 15);
          UseEabConvertNumeric1();
          local.ArColumn.Text20 =
            local.EabConvertNumeric.ReturnNoCommasInNonDecimal;

          break;
        case 5:
          // Blank line on report
          break;
        case 6:
          local.LabelColumn.Text40 = "REASONS DOCUMENTS ARE SUPPRESSED:";

          break;
        case 7:
          local.LabelColumn.Text40 = "     CASE NOT IN ENFORCEMENT FUNCTION";
          local.EabConvertNumeric.SendAmount =
            NumberToString((long)import.NotEnfFunc.Count * 2, 15);
          UseEabConvertNumeric1();
          local.AllColumn.Text20 =
            local.EabConvertNumeric.ReturnNoCommasInNonDecimal;
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.NotEnfFunc.Count, 15);
          UseEabConvertNumeric1();
          local.ApColumn.Text20 =
            local.EabConvertNumeric.ReturnNoCommasInNonDecimal;
          local.ArColumn.Text20 =
            local.EabConvertNumeric.ReturnNoCommasInNonDecimal;

          break;
        case 8:
          local.LabelColumn.Text40 = "     AR MISSING FROM CASE";
          local.EabConvertNumeric.SendAmount =
            NumberToString((long)import.ArMissing.Count * 2, 15);
          UseEabConvertNumeric1();
          local.AllColumn.Text20 =
            local.EabConvertNumeric.ReturnNoCommasInNonDecimal;
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.ArMissing.Count, 15);
          UseEabConvertNumeric1();
          local.ApColumn.Text20 =
            local.EabConvertNumeric.ReturnNoCommasInNonDecimal;
          local.ArColumn.Text20 =
            local.EabConvertNumeric.ReturnNoCommasInNonDecimal;

          break;
        case 9:
          local.LabelColumn.Text40 = "     AP MISSING FROM CASE";
          local.EabConvertNumeric.SendAmount =
            NumberToString((long)import.ApMissing.Count * 2, 15);
          UseEabConvertNumeric1();
          local.AllColumn.Text20 =
            local.EabConvertNumeric.ReturnNoCommasInNonDecimal;
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.ApMissing.Count, 15);
          UseEabConvertNumeric1();
          local.ApColumn.Text20 =
            local.EabConvertNumeric.ReturnNoCommasInNonDecimal;
          local.ArColumn.Text20 =
            local.EabConvertNumeric.ReturnNoCommasInNonDecimal;

          break;
        case 10:
          local.LabelColumn.Text40 = "     NO ACTIVE CHILDREN";
          local.EabConvertNumeric.SendAmount =
            NumberToString((long)import.ChildMissing.Count * 2, 15);
          UseEabConvertNumeric1();
          local.AllColumn.Text20 =
            local.EabConvertNumeric.ReturnNoCommasInNonDecimal;
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.ChildMissing.Count, 15);
          UseEabConvertNumeric1();
          local.ApColumn.Text20 =
            local.EabConvertNumeric.ReturnNoCommasInNonDecimal;
          local.ArColumn.Text20 =
            local.EabConvertNumeric.ReturnNoCommasInNonDecimal;

          break;
        case 11:
          local.LabelColumn.Text40 = "     NO ACTIVE CS OBLIGATION";
          local.EabConvertNumeric.SendAmount =
            NumberToString((long)import.NoActiveCsOblg.Count * 2, 15);
          UseEabConvertNumeric1();
          local.AllColumn.Text20 =
            local.EabConvertNumeric.ReturnNoCommasInNonDecimal;
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.NoActiveCsOblg.Count, 15);
          UseEabConvertNumeric1();
          local.ApColumn.Text20 =
            local.EabConvertNumeric.ReturnNoCommasInNonDecimal;
          local.ArColumn.Text20 =
            local.EabConvertNumeric.ReturnNoCommasInNonDecimal;

          break;
        case 12:
          local.LabelColumn.Text40 = "     NO JOURNAL ENTRY";
          local.EabConvertNumeric.SendAmount =
            NumberToString((long)import.NoLegalActionClassJ.Count * 2, 15);
          UseEabConvertNumeric1();
          local.AllColumn.Text20 =
            local.EabConvertNumeric.ReturnNoCommasInNonDecimal;
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.NoLegalActionClassJ.Count, 15);
          UseEabConvertNumeric1();
          local.ApColumn.Text20 =
            local.EabConvertNumeric.ReturnNoCommasInNonDecimal;
          local.ArColumn.Text20 =
            local.EabConvertNumeric.ReturnNoCommasInNonDecimal;

          break;
        case 13:
          local.LabelColumn.Text40 = "     RECENT JOURNAL ENTRY FILED";
          local.EabConvertNumeric.SendAmount =
            NumberToString((long)import.RecentJClass.Count * 2, 15);
          UseEabConvertNumeric1();
          local.AllColumn.Text20 =
            local.EabConvertNumeric.ReturnNoCommasInNonDecimal;
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.RecentJClass.Count, 15);
          UseEabConvertNumeric1();
          local.ApColumn.Text20 =
            local.EabConvertNumeric.ReturnNoCommasInNonDecimal;
          local.ArColumn.Text20 =
            local.EabConvertNumeric.ReturnNoCommasInNonDecimal;

          break;
        case 14:
          local.LabelColumn.Text40 = "     INTERSTATE INVOLVEMENT FOR CASE";
          local.EabConvertNumeric.SendAmount =
            NumberToString((long)import.InterstateInvolvement.Count * 2, 15);
          UseEabConvertNumeric1();
          local.AllColumn.Text20 =
            local.EabConvertNumeric.ReturnNoCommasInNonDecimal;
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.InterstateInvolvement.Count, 15);
          UseEabConvertNumeric1();
          local.ApColumn.Text20 =
            local.EabConvertNumeric.ReturnNoCommasInNonDecimal;
          local.ArColumn.Text20 =
            local.EabConvertNumeric.ReturnNoCommasInNonDecimal;

          break;
        case 15:
          local.LabelColumn.Text40 = "     RECENT MODIFICATION REVIEW";
          local.EabConvertNumeric.SendAmount =
            NumberToString((long)import.RecentModRev.Count * 2, 15);
          UseEabConvertNumeric1();
          local.AllColumn.Text20 =
            local.EabConvertNumeric.ReturnNoCommasInNonDecimal;
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.RecentModRev.Count, 15);
          UseEabConvertNumeric1();
          local.ApColumn.Text20 =
            local.EabConvertNumeric.ReturnNoCommasInNonDecimal;
          local.ArColumn.Text20 =
            local.EabConvertNumeric.ReturnNoCommasInNonDecimal;

          break;
        case 16:
          local.LabelColumn.Text40 = "     FAMILY VIOLENCE";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.AllFv.Count, 15);
          UseEabConvertNumeric1();
          local.AllColumn.Text20 =
            local.EabConvertNumeric.ReturnNoCommasInNonDecimal;
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.ApFv.Count, 15);
          UseEabConvertNumeric1();
          local.ApColumn.Text20 =
            local.EabConvertNumeric.ReturnNoCommasInNonDecimal;
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.ArFv.Count, 15);
          UseEabConvertNumeric1();
          local.ArColumn.Text20 =
            local.EabConvertNumeric.ReturnNoCommasInNonDecimal;

          break;
        case 17:
          local.LabelColumn.Text40 = "     AR IS ORGANIZATION";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.ArOrg.Count, 15);
          UseEabConvertNumeric1();
          local.AllColumn.Text20 =
            local.EabConvertNumeric.ReturnNoCommasInNonDecimal;
          local.ArColumn.Text20 =
            local.EabConvertNumeric.ReturnNoCommasInNonDecimal;

          break;
        case 18:
          local.LabelColumn.Text40 = "     NO ACTIVE VERIFIED ADDRESS";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.AllNoAddress.Count, 15);
          UseEabConvertNumeric1();
          local.AllColumn.Text20 =
            local.EabConvertNumeric.ReturnNoCommasInNonDecimal;
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.ApNoAddress.Count, 15);
          UseEabConvertNumeric1();
          local.ApColumn.Text20 =
            local.EabConvertNumeric.ReturnNoCommasInNonDecimal;
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.ArNoAddress.Count, 15);
          UseEabConvertNumeric1();
          local.ArColumn.Text20 =
            local.EabConvertNumeric.ReturnNoCommasInNonDecimal;

          break;
        case 19:
          local.LabelColumn.Text40 = "     AR HAS GOOD CAUSE";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.ArGoodCause.Count, 15);
          UseEabConvertNumeric1();
          local.AllColumn.Text20 =
            local.EabConvertNumeric.ReturnNoCommasInNonDecimal;
          local.ArColumn.Text20 =
            local.EabConvertNumeric.ReturnNoCommasInNonDecimal;

          break;
        case 20:
          local.LabelColumn.Text40 = "     RECENT DOCUMENT SENT";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.AllRecentDoc.Count, 15);
          UseEabConvertNumeric1();
          local.AllColumn.Text20 =
            local.EabConvertNumeric.ReturnNoCommasInNonDecimal;
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.ApRecentDoc.Count, 15);
          UseEabConvertNumeric1();
          local.ApColumn.Text20 =
            local.EabConvertNumeric.ReturnNoCommasInNonDecimal;
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.ArRecentDoc.Count, 15);
          UseEabConvertNumeric1();
          local.ArColumn.Text20 =
            local.EabConvertNumeric.ReturnNoCommasInNonDecimal;

          break;
        case 21:
          local.LabelColumn.Text40 = "     QUEUED DOCUMENT";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.AllQueuedDoc.Count, 15);
          UseEabConvertNumeric1();
          local.AllColumn.Text20 =
            local.EabConvertNumeric.ReturnNoCommasInNonDecimal;
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.ApQueuedDoc.Count, 15);
          UseEabConvertNumeric1();
          local.ApColumn.Text20 =
            local.EabConvertNumeric.ReturnNoCommasInNonDecimal;
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.ArQueuedDocs.Count, 15);
          UseEabConvertNumeric1();
          local.ArColumn.Text20 =
            local.EabConvertNumeric.ReturnNoCommasInNonDecimal;

          break;
        case 22:
          local.LabelColumn.Text40 = "     ERROR CREATING DOCUMENT";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.AllDocErrors.Count, 15);
          UseEabConvertNumeric1();
          local.AllColumn.Text20 =
            local.EabConvertNumeric.ReturnNoCommasInNonDecimal;
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.ApDocErrors.Count, 15);
          UseEabConvertNumeric1();
          local.ApColumn.Text20 =
            local.EabConvertNumeric.ReturnNoCommasInNonDecimal;
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.ArDocErrors.Count, 15);
          UseEabConvertNumeric1();
          local.ArColumn.Text20 =
            local.EabConvertNumeric.ReturnNoCommasInNonDecimal;

          break;
        default:
          goto AfterCycle;
      }

      local.EabReportSend.RptDetail = local.LabelColumn.Text40 + local
        .AllColumn.Text20 + local.ApColumn.Text20 + local.ArColumn.Text20;
      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabReportSend.RptDetail = "Error writing to Control Report";
        UseCabErrorReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }
    }

AfterCycle:

    // -------------------------------------------------------
    // CLOSE OUTPUT CONTROL REPORT
    // -------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabReportSend.RptDetail = "Error closing Control Report";
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    // --------------------------------------------------------
    // CLOSE OUTPUT ERROR REPORT
    // --------------------------------------------------------
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabReportSend.RptDetail = "Error closing Error Report";
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
      }
    }
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
    /// A value of CasesRead.
    /// </summary>
    [JsonPropertyName("casesRead")]
    public Common CasesRead
    {
      get => casesRead ??= new();
      set => casesRead = value;
    }

    /// <summary>
    /// A value of NotEnfFunc.
    /// </summary>
    [JsonPropertyName("notEnfFunc")]
    public Common NotEnfFunc
    {
      get => notEnfFunc ??= new();
      set => notEnfFunc = value;
    }

    /// <summary>
    /// A value of ArMissing.
    /// </summary>
    [JsonPropertyName("arMissing")]
    public Common ArMissing
    {
      get => arMissing ??= new();
      set => arMissing = value;
    }

    /// <summary>
    /// A value of ApMissing.
    /// </summary>
    [JsonPropertyName("apMissing")]
    public Common ApMissing
    {
      get => apMissing ??= new();
      set => apMissing = value;
    }

    /// <summary>
    /// A value of ChildMissing.
    /// </summary>
    [JsonPropertyName("childMissing")]
    public Common ChildMissing
    {
      get => childMissing ??= new();
      set => childMissing = value;
    }

    /// <summary>
    /// A value of NoActiveCsOblg.
    /// </summary>
    [JsonPropertyName("noActiveCsOblg")]
    public Common NoActiveCsOblg
    {
      get => noActiveCsOblg ??= new();
      set => noActiveCsOblg = value;
    }

    /// <summary>
    /// A value of NoLegalActionClassJ.
    /// </summary>
    [JsonPropertyName("noLegalActionClassJ")]
    public Common NoLegalActionClassJ
    {
      get => noLegalActionClassJ ??= new();
      set => noLegalActionClassJ = value;
    }

    /// <summary>
    /// A value of InterstateInvolvement.
    /// </summary>
    [JsonPropertyName("interstateInvolvement")]
    public Common InterstateInvolvement
    {
      get => interstateInvolvement ??= new();
      set => interstateInvolvement = value;
    }

    /// <summary>
    /// A value of RecentJClass.
    /// </summary>
    [JsonPropertyName("recentJClass")]
    public Common RecentJClass
    {
      get => recentJClass ??= new();
      set => recentJClass = value;
    }

    /// <summary>
    /// A value of RecentModRev.
    /// </summary>
    [JsonPropertyName("recentModRev")]
    public Common RecentModRev
    {
      get => recentModRev ??= new();
      set => recentModRev = value;
    }

    /// <summary>
    /// A value of AllFv.
    /// </summary>
    [JsonPropertyName("allFv")]
    public Common AllFv
    {
      get => allFv ??= new();
      set => allFv = value;
    }

    /// <summary>
    /// A value of AllNoAddress.
    /// </summary>
    [JsonPropertyName("allNoAddress")]
    public Common AllNoAddress
    {
      get => allNoAddress ??= new();
      set => allNoAddress = value;
    }

    /// <summary>
    /// A value of AllRecentDoc.
    /// </summary>
    [JsonPropertyName("allRecentDoc")]
    public Common AllRecentDoc
    {
      get => allRecentDoc ??= new();
      set => allRecentDoc = value;
    }

    /// <summary>
    /// A value of AllQueuedDoc.
    /// </summary>
    [JsonPropertyName("allQueuedDoc")]
    public Common AllQueuedDoc
    {
      get => allQueuedDoc ??= new();
      set => allQueuedDoc = value;
    }

    /// <summary>
    /// A value of AllDocErrors.
    /// </summary>
    [JsonPropertyName("allDocErrors")]
    public Common AllDocErrors
    {
      get => allDocErrors ??= new();
      set => allDocErrors = value;
    }

    /// <summary>
    /// A value of AllDocCreates.
    /// </summary>
    [JsonPropertyName("allDocCreates")]
    public Common AllDocCreates
    {
      get => allDocCreates ??= new();
      set => allDocCreates = value;
    }

    /// <summary>
    /// A value of ApFv.
    /// </summary>
    [JsonPropertyName("apFv")]
    public Common ApFv
    {
      get => apFv ??= new();
      set => apFv = value;
    }

    /// <summary>
    /// A value of ApNoAddress.
    /// </summary>
    [JsonPropertyName("apNoAddress")]
    public Common ApNoAddress
    {
      get => apNoAddress ??= new();
      set => apNoAddress = value;
    }

    /// <summary>
    /// A value of ApRecentDoc.
    /// </summary>
    [JsonPropertyName("apRecentDoc")]
    public Common ApRecentDoc
    {
      get => apRecentDoc ??= new();
      set => apRecentDoc = value;
    }

    /// <summary>
    /// A value of ApQueuedDoc.
    /// </summary>
    [JsonPropertyName("apQueuedDoc")]
    public Common ApQueuedDoc
    {
      get => apQueuedDoc ??= new();
      set => apQueuedDoc = value;
    }

    /// <summary>
    /// A value of ApDocErrors.
    /// </summary>
    [JsonPropertyName("apDocErrors")]
    public Common ApDocErrors
    {
      get => apDocErrors ??= new();
      set => apDocErrors = value;
    }

    /// <summary>
    /// A value of ApDocCreates.
    /// </summary>
    [JsonPropertyName("apDocCreates")]
    public Common ApDocCreates
    {
      get => apDocCreates ??= new();
      set => apDocCreates = value;
    }

    /// <summary>
    /// A value of ArFv.
    /// </summary>
    [JsonPropertyName("arFv")]
    public Common ArFv
    {
      get => arFv ??= new();
      set => arFv = value;
    }

    /// <summary>
    /// A value of ArNoAddress.
    /// </summary>
    [JsonPropertyName("arNoAddress")]
    public Common ArNoAddress
    {
      get => arNoAddress ??= new();
      set => arNoAddress = value;
    }

    /// <summary>
    /// A value of ArRecentDoc.
    /// </summary>
    [JsonPropertyName("arRecentDoc")]
    public Common ArRecentDoc
    {
      get => arRecentDoc ??= new();
      set => arRecentDoc = value;
    }

    /// <summary>
    /// A value of ArQueuedDocs.
    /// </summary>
    [JsonPropertyName("arQueuedDocs")]
    public Common ArQueuedDocs
    {
      get => arQueuedDocs ??= new();
      set => arQueuedDocs = value;
    }

    /// <summary>
    /// A value of ArDocErrors.
    /// </summary>
    [JsonPropertyName("arDocErrors")]
    public Common ArDocErrors
    {
      get => arDocErrors ??= new();
      set => arDocErrors = value;
    }

    /// <summary>
    /// A value of ArDocCreates.
    /// </summary>
    [JsonPropertyName("arDocCreates")]
    public Common ArDocCreates
    {
      get => arDocCreates ??= new();
      set => arDocCreates = value;
    }

    /// <summary>
    /// A value of ArOrg.
    /// </summary>
    [JsonPropertyName("arOrg")]
    public Common ArOrg
    {
      get => arOrg ??= new();
      set => arOrg = value;
    }

    /// <summary>
    /// A value of ArGoodCause.
    /// </summary>
    [JsonPropertyName("arGoodCause")]
    public Common ArGoodCause
    {
      get => arGoodCause ??= new();
      set => arGoodCause = value;
    }

    private Common casesRead;
    private Common notEnfFunc;
    private Common arMissing;
    private Common apMissing;
    private Common childMissing;
    private Common noActiveCsOblg;
    private Common noLegalActionClassJ;
    private Common interstateInvolvement;
    private Common recentJClass;
    private Common recentModRev;
    private Common allFv;
    private Common allNoAddress;
    private Common allRecentDoc;
    private Common allQueuedDoc;
    private Common allDocErrors;
    private Common allDocCreates;
    private Common apFv;
    private Common apNoAddress;
    private Common apRecentDoc;
    private Common apQueuedDoc;
    private Common apDocErrors;
    private Common apDocCreates;
    private Common arFv;
    private Common arNoAddress;
    private Common arRecentDoc;
    private Common arQueuedDocs;
    private Common arDocErrors;
    private Common arDocCreates;
    private Common arOrg;
    private Common arGoodCause;
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
    /// A value of LabelColumn.
    /// </summary>
    [JsonPropertyName("labelColumn")]
    public WorkArea LabelColumn
    {
      get => labelColumn ??= new();
      set => labelColumn = value;
    }

    /// <summary>
    /// A value of AllColumn.
    /// </summary>
    [JsonPropertyName("allColumn")]
    public WorkArea AllColumn
    {
      get => allColumn ??= new();
      set => allColumn = value;
    }

    /// <summary>
    /// A value of ApColumn.
    /// </summary>
    [JsonPropertyName("apColumn")]
    public WorkArea ApColumn
    {
      get => apColumn ??= new();
      set => apColumn = value;
    }

    /// <summary>
    /// A value of ArColumn.
    /// </summary>
    [JsonPropertyName("arColumn")]
    public WorkArea ArColumn
    {
      get => arColumn ??= new();
      set => arColumn = value;
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

    private WorkArea labelColumn;
    private WorkArea allColumn;
    private WorkArea apColumn;
    private WorkArea arColumn;
    private EabConvertNumeric2 eabConvertNumeric;
    private Common maxControlTotal;
    private Common subscript;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
  }
#endregion
}
