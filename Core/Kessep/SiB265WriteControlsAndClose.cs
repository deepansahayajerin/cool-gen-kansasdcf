// Program: SI_B265_WRITE_CONTROLS_AND_CLOSE, ID: 371082977, model: 746.
// Short name: SWE02619
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B265_WRITE_CONTROLS_AND_CLOSE.
/// </summary>
[Serializable]
public partial class SiB265WriteControlsAndClose: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B265_WRITE_CONTROLS_AND_CLOSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB265WriteControlsAndClose(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB265WriteControlsAndClose.
  /// </summary>
  public SiB265WriteControlsAndClose(IContext context, Import import,
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
    // WRITE OUTPUT CONTROL REPORT
    // -----------------------------------------------------------------
    local.EabFileHandling.Action = "WRITE";
    local.EabConvertNumeric.SendNonSuppressPos = 1;
    local.Subscript.Count = 1;

    for(var limit = local.MaxControlTotal.Count; local.Subscript.Count <= limit
      ; ++local.Subscript.Count)
    {
      local.EabReportSend.RptDetail = "";
      local.Label.Text60 = "";
      local.EabConvertNumeric.SendAmount = "";

      switch(local.Subscript.Count)
      {
        case 1:
          local.Indent.Count = 0;
          local.Label.Text60 = "Incoming CSI transactions read";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.TotRead.Count, 15);

          break;
        case 2:
          local.Label.Text60 = "Outgoing CSI transactions sent";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.TotProcessed.Count, 15);

          break;
        case 3:
          local.Indent.Count = 1;
          local.Label.Text60 = "Successful CSI P (FSINF)";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.TotFsinf.Count, 15);

          break;
        case 4:
          local.Label.Text60 =
            "Unsuccessful CSI P (FUINF) due to Family Violence";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.TotFuinfFv.Count, 15);

          break;
        case 5:
          local.Label.Text60 =
            "Unsuccessful CSI P (FUINF) due to Invalid KS Case";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.TotFuinfInvalidCase.Count, 15);

          break;
        case 6:
          local.Label.Text60 =
            "Unsuccessful CSI P (FUINF) due to Missing KS Case";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.TotFuinfMissingCase.Count, 15);

          break;
        case 7:
          local.Indent.Count = 0;
          local.Label.Text60 = "Outgoing Transactions not sent due to error";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.TotErred.Count, 15);

          break;
        case 8:
          local.Indent.Count = 1;
          local.Label.Text60 = "Rollback errors";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.TotRollbacks.Count, 15);

          break;
        case 9:
          break;
        case 10:
          local.Indent.Count = 0;
          local.Label.Text60 = "Transaction Envelopes created";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.TotTransaction.Count, 15);

          break;
        case 11:
          local.Label.Text60 = "Case DB created";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.TotCaseDb.Count, 15);

          break;
        case 12:
          local.Label.Text60 = "AP Identification DB created";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.TotApidDb.Count, 15);

          break;
        case 13:
          local.Label.Text60 = "AP Locate DB created";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.TotApLocateDb.Count, 15);

          break;
        case 14:
          local.Label.Text60 = "Participant DB created";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.TotParticipantDb.Count, 15);

          break;
        case 15:
          local.Label.Text60 = "Order DB created";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.TotOrderDb.Count, 15);

          break;
        case 16:
          local.Label.Text60 = "Misc DB created";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.TotMiscDb.Count, 15);

          break;
        default:
          goto AfterCycle;
      }

      // ------------------------------------------------
      // Format number
      // ------------------------------------------------
      if (!IsEmpty(local.EabConvertNumeric.SendAmount))
      {
        UseEabConvertNumeric1();
      }

      // ------------------------------------------------
      // Format detail line
      // ------------------------------------------------
      if (!IsEmpty(local.Label.Text60))
      {
        if (!IsEmpty(local.EabConvertNumeric.ReturnNoCommasInNonDecimal))
        {
          local.EabReportSend.RptDetail = local.Label.Text60 + local
            .EabConvertNumeric.ReturnNoCommasInNonDecimal;
        }
        else
        {
          local.EabReportSend.RptDetail = local.Label.Text60;
        }

        // ------------------------------------------------
        // Indent line 5 * indent_count
        // ------------------------------------------------
        if (local.Indent.Count > 0)
        {
          local.EabReportSend.RptDetail =
            Substring(local.Null1.Text30, TextWorkArea.Text30_MaxLength, 1,
            (int)((long)local.Indent.Count * 5)) + local
            .EabReportSend.RptDetail;
        }
      }

      // ------------------------------------------------
      // Write detail line
      // ------------------------------------------------
      UseCabControlReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabReportSend.RptDetail =
          "Error encountered writing to Control Report";
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }
    }

AfterCycle:

    // -----------------------------------------------------------------
    // CLOSE CONTROL REPORT
    // -----------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_CLOSING_EXT_FILE";

      return;
    }

    // -----------------------------------------------------------------
    // CLOSE ERROR REPORT
    // -----------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport();

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

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport()
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
    /// A value of TotRead.
    /// </summary>
    [JsonPropertyName("totRead")]
    public Common TotRead
    {
      get => totRead ??= new();
      set => totRead = value;
    }

    /// <summary>
    /// A value of TotProcessed.
    /// </summary>
    [JsonPropertyName("totProcessed")]
    public Common TotProcessed
    {
      get => totProcessed ??= new();
      set => totProcessed = value;
    }

    /// <summary>
    /// A value of TotErred.
    /// </summary>
    [JsonPropertyName("totErred")]
    public Common TotErred
    {
      get => totErred ??= new();
      set => totErred = value;
    }

    /// <summary>
    /// A value of TotRollbacks.
    /// </summary>
    [JsonPropertyName("totRollbacks")]
    public Common TotRollbacks
    {
      get => totRollbacks ??= new();
      set => totRollbacks = value;
    }

    /// <summary>
    /// A value of TotFsinf.
    /// </summary>
    [JsonPropertyName("totFsinf")]
    public Common TotFsinf
    {
      get => totFsinf ??= new();
      set => totFsinf = value;
    }

    /// <summary>
    /// A value of TotFuinfFv.
    /// </summary>
    [JsonPropertyName("totFuinfFv")]
    public Common TotFuinfFv
    {
      get => totFuinfFv ??= new();
      set => totFuinfFv = value;
    }

    /// <summary>
    /// A value of TotFuinfInvalidCase.
    /// </summary>
    [JsonPropertyName("totFuinfInvalidCase")]
    public Common TotFuinfInvalidCase
    {
      get => totFuinfInvalidCase ??= new();
      set => totFuinfInvalidCase = value;
    }

    /// <summary>
    /// A value of TotFuinfMissingCase.
    /// </summary>
    [JsonPropertyName("totFuinfMissingCase")]
    public Common TotFuinfMissingCase
    {
      get => totFuinfMissingCase ??= new();
      set => totFuinfMissingCase = value;
    }

    /// <summary>
    /// A value of TotTransaction.
    /// </summary>
    [JsonPropertyName("totTransaction")]
    public Common TotTransaction
    {
      get => totTransaction ??= new();
      set => totTransaction = value;
    }

    /// <summary>
    /// A value of TotCaseDb.
    /// </summary>
    [JsonPropertyName("totCaseDb")]
    public Common TotCaseDb
    {
      get => totCaseDb ??= new();
      set => totCaseDb = value;
    }

    /// <summary>
    /// A value of TotApidDb.
    /// </summary>
    [JsonPropertyName("totApidDb")]
    public Common TotApidDb
    {
      get => totApidDb ??= new();
      set => totApidDb = value;
    }

    /// <summary>
    /// A value of TotApLocateDb.
    /// </summary>
    [JsonPropertyName("totApLocateDb")]
    public Common TotApLocateDb
    {
      get => totApLocateDb ??= new();
      set => totApLocateDb = value;
    }

    /// <summary>
    /// A value of TotParticipantDb.
    /// </summary>
    [JsonPropertyName("totParticipantDb")]
    public Common TotParticipantDb
    {
      get => totParticipantDb ??= new();
      set => totParticipantDb = value;
    }

    /// <summary>
    /// A value of TotOrderDb.
    /// </summary>
    [JsonPropertyName("totOrderDb")]
    public Common TotOrderDb
    {
      get => totOrderDb ??= new();
      set => totOrderDb = value;
    }

    /// <summary>
    /// A value of TotMiscDb.
    /// </summary>
    [JsonPropertyName("totMiscDb")]
    public Common TotMiscDb
    {
      get => totMiscDb ??= new();
      set => totMiscDb = value;
    }

    private Common totRead;
    private Common totProcessed;
    private Common totErred;
    private Common totRollbacks;
    private Common totFsinf;
    private Common totFuinfFv;
    private Common totFuinfInvalidCase;
    private Common totFuinfMissingCase;
    private Common totTransaction;
    private Common totCaseDb;
    private Common totApidDb;
    private Common totApLocateDb;
    private Common totParticipantDb;
    private Common totOrderDb;
    private Common totMiscDb;
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
    /// A value of EabConvertNumeric.
    /// </summary>
    [JsonPropertyName("eabConvertNumeric")]
    public EabConvertNumeric2 EabConvertNumeric
    {
      get => eabConvertNumeric ??= new();
      set => eabConvertNumeric = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public TextWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of Indent.
    /// </summary>
    [JsonPropertyName("indent")]
    public Common Indent
    {
      get => indent ??= new();
      set => indent = value;
    }

    /// <summary>
    /// A value of ZdelLocalTextnum.
    /// </summary>
    [JsonPropertyName("zdelLocalTextnum")]
    public WorkArea ZdelLocalTextnum
    {
      get => zdelLocalTextnum ??= new();
      set => zdelLocalTextnum = value;
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
    /// A value of Subscript.
    /// </summary>
    [JsonPropertyName("subscript")]
    public Common Subscript
    {
      get => subscript ??= new();
      set => subscript = value;
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

    private EabConvertNumeric2 eabConvertNumeric;
    private TextWorkArea null1;
    private Common indent;
    private WorkArea zdelLocalTextnum;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private WorkArea label;
    private Common subscript;
    private Common maxControlTotal;
  }
#endregion
}
