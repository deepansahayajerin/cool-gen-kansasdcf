// Program: FN_BF12_SUM_BY_PROCESS_DATE, ID: 373334838, model: 746.
// Short name: SWE02741
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_BF12_SUM_BY_PROCESS_DATE.
/// </summary>
[Serializable]
public partial class FnBf12SumByProcessDate: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_BF12_SUM_BY_PROCESS_DATE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnBf12SumByProcessDate(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnBf12SumByProcessDate.
  /// </summary>
  public FnBf12SumByProcessDate(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***************************************************
    // 2002-01-25  WR 000235  Fangman - New AB to table up the amount of 
    // recaptures and total payments by process date.
    // ***************************************************
    // Convert process date to an index.  index = (process year - 1977) * 12 + 
    // process month
    local.ProcessDtIndex.Number4 =
      (Year(import.Disbursement.ProcessDate) - 1977) * 12 + Month
      (import.Disbursement.ProcessDate);

    if (local.ProcessDtIndex.Number4 > Export.ProcessDtTblGroup.Capacity)
    {
      ExitState = "FN0000_GROUP_VIEW_LIMIT_EXCEEDED";

      return;
    }

    export.ProcessDtTbl.Index = local.ProcessDtIndex.Number4 - 1;
    export.ProcessDtTbl.CheckSize();

    if (import.DisbursementType.SystemGeneratedIdentifier == 72)
    {
      export.ProcessDtTbl.Update.MonthlyObligeeSummary.PassthruRecapAmt =
        export.ProcessDtTbl.Item.MonthlyObligeeSummary.PassthruRecapAmt.
          GetValueOrDefault() + import.Disbursement.Amount;
    }
    else if (AsChar(import.DisbursementType.CurrentArrearsInd) == 'C')
    {
      export.ProcessDtTbl.Update.MonthlyObligeeSummary.NaCurrRecapAmt =
        export.ProcessDtTbl.Item.MonthlyObligeeSummary.NaCurrRecapAmt.
          GetValueOrDefault() + import.Disbursement.Amount;
    }
    else
    {
      export.ProcessDtTbl.Update.MonthlyObligeeSummary.NaArrearsRecapAmt =
        export.ProcessDtTbl.Item.MonthlyObligeeSummary.NaArrearsRecapAmt.
          GetValueOrDefault() + import.Disbursement.Amount;
    }

    export.ProcessDtTbl.Update.MoSumTblUpdatedInd.Flag = "N";

    if (AsChar(import.TestDisplayInd.Flag) == 'Y')
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "  Recap index " + NumberToString
        (local.ProcessDtIndex.Number4, 15) + "  PT " + NumberToString
        ((long)(export.ProcessDtTbl.Item.MonthlyObligeeSummary.PassthruRecapAmt.
          GetValueOrDefault() * 100), 15) + "  Curr " + NumberToString
        ((long)(export.ProcessDtTbl.Item.MonthlyObligeeSummary.NaCurrRecapAmt.
          GetValueOrDefault() * 100), 15) + "  Arrears " + NumberToString
        ((long)(export.ProcessDtTbl.Item.MonthlyObligeeSummary.
          NaArrearsRecapAmt.GetValueOrDefault() * 100), 15);
      UseCabControlReport();
    }
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

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
    /// <summary>
    /// A value of Disbursement.
    /// </summary>
    [JsonPropertyName("disbursement")]
    public DisbursementTransaction Disbursement
    {
      get => disbursement ??= new();
      set => disbursement = value;
    }

    /// <summary>
    /// A value of DisbursementType.
    /// </summary>
    [JsonPropertyName("disbursementType")]
    public DisbursementType DisbursementType
    {
      get => disbursementType ??= new();
      set => disbursementType = value;
    }

    /// <summary>
    /// A value of TestDisplayInd.
    /// </summary>
    [JsonPropertyName("testDisplayInd")]
    public Common TestDisplayInd
    {
      get => testDisplayInd ??= new();
      set => testDisplayInd = value;
    }

    private DisbursementTransaction disbursement;
    private DisbursementType disbursementType;
    private Common testDisplayInd;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ProcessDtTblGroup group.</summary>
    [Serializable]
    public class ProcessDtTblGroup
    {
      /// <summary>
      /// A value of MoSumTblUpdatedInd.
      /// </summary>
      [JsonPropertyName("moSumTblUpdatedInd")]
      public Common MoSumTblUpdatedInd
      {
        get => moSumTblUpdatedInd ??= new();
        set => moSumTblUpdatedInd = value;
      }

      /// <summary>
      /// A value of MonthlyObligeeSummary.
      /// </summary>
      [JsonPropertyName("monthlyObligeeSummary")]
      public MonthlyObligeeSummary MonthlyObligeeSummary
      {
        get => monthlyObligeeSummary ??= new();
        set => monthlyObligeeSummary = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 360;

      private Common moSumTblUpdatedInd;
      private MonthlyObligeeSummary monthlyObligeeSummary;
    }

    /// <summary>
    /// Gets a value of ProcessDtTbl.
    /// </summary>
    [JsonIgnore]
    public Array<ProcessDtTblGroup> ProcessDtTbl => processDtTbl ??= new(
      ProcessDtTblGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ProcessDtTbl for json serialization.
    /// </summary>
    [JsonPropertyName("processDtTbl")]
    [Computed]
    public IList<ProcessDtTblGroup> ProcessDtTbl_Json
    {
      get => processDtTbl;
      set => ProcessDtTbl.Assign(value);
    }

    private Array<ProcessDtTblGroup> processDtTbl;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ProcessDtIndex.
    /// </summary>
    [JsonPropertyName("processDtIndex")]
    public NumericWorkSet ProcessDtIndex
    {
      get => processDtIndex ??= new();
      set => processDtIndex = value;
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

    private NumericWorkSet processDtIndex;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
  }
#endregion
}
