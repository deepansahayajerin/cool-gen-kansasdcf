// Program: OE_B465_WRITE_CONTROL_TOTALS, ID: 374471563, model: 746.
// Short name: SWE00011
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_B465_WRITE_CONTROL_TOTALS.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This module to write each control total accumulated throughout program 
/// processing onto a control report
/// </para>
/// </summary>
[Serializable]
public partial class OeB465WriteControlTotals: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B465_WRITE_CONTROL_TOTALS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB465WriteControlTotals(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB465WriteControlTotals.
  /// </summary>
  public OeB465WriteControlTotals(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // *********************************************************************************
    // Date          Developer	Description
    // 05/04/2000      SWSRDCV    	Initial Creation
    // *********************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.ProcessDate = Now().Date;
    local.EabReportSend.ProgramName = global.UserId;
    local.EabReportSend.RptDetail = "";
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RC_AB0008_INVALID_RETURN_CD";

      return;
    }

    for(local.GrpIdx.Count = 1; local.GrpIdx.Count <= 10; ++local.GrpIdx.Count)
    {
      switch(local.GrpIdx.Count)
      {
        case 1:
          local.ProgramControlTotal.Name =
            "Number of Grants Read                          : ";
          local.ProgramControlTotal.Value =
            import.TotalGrantsRead.Value.GetValueOrDefault();

          break;
        case 2:
          local.ProgramControlTotal.Name =
            "Value of Grants Read                           : ";
          local.ProgramControlTotal.Value =
            import.TotalGrantAmt.TotalCurrency * 100;

          break;
        case 3:
          local.ProgramControlTotal.Name =
            "Number of Households Added                     : ";
          local.ProgramControlTotal.Value =
            import.HhRecsAdded.Value.GetValueOrDefault();

          break;
        case 4:
          local.ProgramControlTotal.Name =
            "Number of Member Monthly Summaries Added       : ";
          local.ProgramControlTotal.Value =
            import.HhMbrMsumRecsAdded.Value.GetValueOrDefault();

          break;
        case 5:
          local.ProgramControlTotal.Name =
            "Number of Member Monthly Summaries Updated     : ";
          local.ProgramControlTotal.Value =
            import.HhMbrMsumRecsUpdated.Value.GetValueOrDefault();

          break;
        case 6:
          local.ProgramControlTotal.Name =
            "Number of CSE Persons Added                    : ";
          local.ProgramControlTotal.Value =
            import.PersRecsAdded.Value.GetValueOrDefault();

          break;
        case 7:
          local.ProgramControlTotal.Name =
            "Number of Triggers Set                         : ";
          local.ProgramControlTotal.Value =
            import.TriggerRecsSet.Value.GetValueOrDefault();

          break;
        case 8:
          local.ProgramControlTotal.Name =
            "                                               : ";
          local.ProgramControlTotal.Value = 0;

          break;
        case 9:
          local.ProgramControlTotal.Name =
            "Number of Grants processed during restart      : ";

          if (import.TotalGrantsRead.Value.GetValueOrDefault() == import
            .TotalRestartGrantsRead.Value.GetValueOrDefault())
          {
            local.ProgramControlTotal.Value = 0;
          }
          else
          {
            local.ProgramControlTotal.Value =
              import.TotalRestartGrantsRead.Value.GetValueOrDefault();
          }

          break;
        case 10:
          local.ProgramControlTotal.Name =
            "Value of Grants processed during restart       : ";

          if (import.TotalGrantAmt.TotalCurrency == import
            .TotalRestartGrantAmt.TotalCurrency)
          {
            local.ProgramControlTotal.Value = 0;
          }
          else
          {
            local.ProgramControlTotal.Value =
              import.TotalRestartGrantAmt.TotalCurrency * 100;
          }

          break;
        default:
          break;
      }

      local.EabReportSend.RptDetail = (local.ProgramControlTotal.Name ?? "") + NumberToString
        ((long)local.ProgramControlTotal.Value.GetValueOrDefault(), 15);
      UseCabControlReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_RC_AB0008_INVALID_RETURN_CD";

        return;
      }
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RC_AB0008_INVALID_RETURN_CD";
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
    /// A value of TotalRestartGrantAmt.
    /// </summary>
    [JsonPropertyName("totalRestartGrantAmt")]
    public Common TotalRestartGrantAmt
    {
      get => totalRestartGrantAmt ??= new();
      set => totalRestartGrantAmt = value;
    }

    /// <summary>
    /// A value of TotalGrantAmt.
    /// </summary>
    [JsonPropertyName("totalGrantAmt")]
    public Common TotalGrantAmt
    {
      get => totalGrantAmt ??= new();
      set => totalGrantAmt = value;
    }

    /// <summary>
    /// A value of TotalGrantsRead.
    /// </summary>
    [JsonPropertyName("totalGrantsRead")]
    public ProgramControlTotal TotalGrantsRead
    {
      get => totalGrantsRead ??= new();
      set => totalGrantsRead = value;
    }

    /// <summary>
    /// A value of HhRecsAdded.
    /// </summary>
    [JsonPropertyName("hhRecsAdded")]
    public ProgramControlTotal HhRecsAdded
    {
      get => hhRecsAdded ??= new();
      set => hhRecsAdded = value;
    }

    /// <summary>
    /// A value of PersRecsAdded.
    /// </summary>
    [JsonPropertyName("persRecsAdded")]
    public ProgramControlTotal PersRecsAdded
    {
      get => persRecsAdded ??= new();
      set => persRecsAdded = value;
    }

    /// <summary>
    /// A value of HhMbrMsumRecsUpdated.
    /// </summary>
    [JsonPropertyName("hhMbrMsumRecsUpdated")]
    public ProgramControlTotal HhMbrMsumRecsUpdated
    {
      get => hhMbrMsumRecsUpdated ??= new();
      set => hhMbrMsumRecsUpdated = value;
    }

    /// <summary>
    /// A value of HhMbrMsumRecsAdded.
    /// </summary>
    [JsonPropertyName("hhMbrMsumRecsAdded")]
    public ProgramControlTotal HhMbrMsumRecsAdded
    {
      get => hhMbrMsumRecsAdded ??= new();
      set => hhMbrMsumRecsAdded = value;
    }

    /// <summary>
    /// A value of TriggerRecsSet.
    /// </summary>
    [JsonPropertyName("triggerRecsSet")]
    public ProgramControlTotal TriggerRecsSet
    {
      get => triggerRecsSet ??= new();
      set => triggerRecsSet = value;
    }

    /// <summary>
    /// A value of TotalRestartGrantsRead.
    /// </summary>
    [JsonPropertyName("totalRestartGrantsRead")]
    public ProgramControlTotal TotalRestartGrantsRead
    {
      get => totalRestartGrantsRead ??= new();
      set => totalRestartGrantsRead = value;
    }

    /// <summary>
    /// A value of IssueDateBypass.
    /// </summary>
    [JsonPropertyName("issueDateBypass")]
    public ProgramControlTotal IssueDateBypass
    {
      get => issueDateBypass ??= new();
      set => issueDateBypass = value;
    }

    /// <summary>
    /// A value of Overpayments.
    /// </summary>
    [JsonPropertyName("overpayments")]
    public ProgramControlTotal Overpayments
    {
      get => overpayments ??= new();
      set => overpayments = value;
    }

    /// <summary>
    /// A value of Unused.
    /// </summary>
    [JsonPropertyName("unused")]
    public ProgramControlTotal Unused
    {
      get => unused ??= new();
      set => unused = value;
    }

    private Common totalRestartGrantAmt;
    private Common totalGrantAmt;
    private ProgramControlTotal totalGrantsRead;
    private ProgramControlTotal hhRecsAdded;
    private ProgramControlTotal persRecsAdded;
    private ProgramControlTotal hhMbrMsumRecsUpdated;
    private ProgramControlTotal hhMbrMsumRecsAdded;
    private ProgramControlTotal triggerRecsSet;
    private ProgramControlTotal totalRestartGrantsRead;
    private ProgramControlTotal issueDateBypass;
    private ProgramControlTotal overpayments;
    private ProgramControlTotal unused;
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
    /// A value of GrpIdx.
    /// </summary>
    [JsonPropertyName("grpIdx")]
    public Common GrpIdx
    {
      get => grpIdx ??= new();
      set => grpIdx = value;
    }

    /// <summary>
    /// A value of ProgramControlTotal.
    /// </summary>
    [JsonPropertyName("programControlTotal")]
    public ProgramControlTotal ProgramControlTotal
    {
      get => programControlTotal ??= new();
      set => programControlTotal = value;
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

    private Common grpIdx;
    private ProgramControlTotal programControlTotal;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
  }
#endregion
}
