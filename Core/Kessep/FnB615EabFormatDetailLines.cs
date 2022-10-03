// Program: FN_B615_EAB_FORMAT_DETAIL_LINES, ID: 373027978, model: 746.
// Short name: SWEX0045
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B615_EAB_FORMAT_DETAIL_LINES.
/// </summary>
[Serializable]
public partial class FnB615EabFormatDetailLines: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B615_EAB_FORMAT_DETAIL_LINES program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB615EabFormatDetailLines(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB615EabFormatDetailLines.
  /// </summary>
  public FnB615EabFormatDetailLines(IContext context, Import import,
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
    GetService<IEabStub>().Execute(
      "SWEX0045", context, import, export, EabOptions.Hpvp);
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of CashKpc.
    /// </summary>
    [JsonPropertyName("cashKpc")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "TotalCurrency" })
      ]
    public Common CashKpc
    {
      get => cashKpc ??= new();
      set => cashKpc = value;
    }

    /// <summary>
    /// A value of CashFdso.
    /// </summary>
    [JsonPropertyName("cashFdso")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "TotalCurrency" })
      ]
    public Common CashFdso
    {
      get => cashFdso ??= new();
      set => cashFdso = value;
    }

    /// <summary>
    /// A value of CashSdso.
    /// </summary>
    [JsonPropertyName("cashSdso")]
    [Member(Index = 3, AccessFields = false, Members = new[] { "TotalCurrency" })
      ]
    public Common CashSdso
    {
      get => cashSdso ??= new();
      set => cashSdso = value;
    }

    /// <summary>
    /// A value of CashMisc.
    /// </summary>
    [JsonPropertyName("cashMisc")]
    [Member(Index = 4, AccessFields = false, Members = new[] { "TotalCurrency" })
      ]
    public Common CashMisc
    {
      get => cashMisc ??= new();
      set => cashMisc = value;
    }

    /// <summary>
    /// A value of CashOther.
    /// </summary>
    [JsonPropertyName("cashOther")]
    [Member(Index = 5, AccessFields = false, Members = new[] { "TotalCurrency" })
      ]
    public Common CashOther
    {
      get => cashOther ??= new();
      set => cashOther = value;
    }

    /// <summary>
    /// A value of RefundAdvancement.
    /// </summary>
    [JsonPropertyName("refundAdvancement")]
    [Member(Index = 6, AccessFields = false, Members = new[] { "TotalCurrency" })
      ]
    public Common RefundAdvancement
    {
      get => refundAdvancement ??= new();
      set => refundAdvancement = value;
    }

    /// <summary>
    /// A value of DistTaf.
    /// </summary>
    [JsonPropertyName("distTaf")]
    [Member(Index = 7, AccessFields = false, Members = new[] { "TotalCurrency" })
      ]
    public Common DistTaf
    {
      get => distTaf ??= new();
      set => distTaf = value;
    }

    /// <summary>
    /// A value of DistNonTaf.
    /// </summary>
    [JsonPropertyName("distNonTaf")]
    [Member(Index = 8, AccessFields = false, Members = new[] { "TotalCurrency" })
      ]
    public Common DistNonTaf
    {
      get => distNonTaf ??= new();
      set => distNonTaf = value;
    }

    /// <summary>
    /// A value of DistTafFc.
    /// </summary>
    [JsonPropertyName("distTafFc")]
    [Member(Index = 9, AccessFields = false, Members = new[] { "TotalCurrency" })
      ]
    public Common DistTafFc
    {
      get => distTafFc ??= new();
      set => distTafFc = value;
    }

    /// <summary>
    /// A value of DistGaFc.
    /// </summary>
    [JsonPropertyName("distGaFc")]
    [Member(Index = 10, AccessFields = false, Members
      = new[] { "TotalCurrency" })]
    public Common DistGaFc
    {
      get => distGaFc ??= new();
      set => distGaFc = value;
    }

    /// <summary>
    /// A value of DistTafAdj.
    /// </summary>
    [JsonPropertyName("distTafAdj")]
    [Member(Index = 11, AccessFields = false, Members
      = new[] { "TotalCurrency" })]
    public Common DistTafAdj
    {
      get => distTafAdj ??= new();
      set => distTafAdj = value;
    }

    /// <summary>
    /// A value of DistNonTafAdj.
    /// </summary>
    [JsonPropertyName("distNonTafAdj")]
    [Member(Index = 12, AccessFields = false, Members
      = new[] { "TotalCurrency" })]
    public Common DistNonTafAdj
    {
      get => distNonTafAdj ??= new();
      set => distNonTafAdj = value;
    }

    /// <summary>
    /// A value of DistTafFcAdj.
    /// </summary>
    [JsonPropertyName("distTafFcAdj")]
    [Member(Index = 13, AccessFields = false, Members
      = new[] { "TotalCurrency" })]
    public Common DistTafFcAdj
    {
      get => distTafFcAdj ??= new();
      set => distTafFcAdj = value;
    }

    /// <summary>
    /// A value of DistGaFcAdj.
    /// </summary>
    [JsonPropertyName("distGaFcAdj")]
    [Member(Index = 14, AccessFields = false, Members
      = new[] { "TotalCurrency" })]
    public Common DistGaFcAdj
    {
      get => distGaFcAdj ??= new();
      set => distGaFcAdj = value;
    }

    /// <summary>
    /// A value of DisbPtWar.
    /// </summary>
    [JsonPropertyName("disbPtWar")]
    [Member(Index = 15, AccessFields = false, Members
      = new[] { "TotalCurrency" })]
    public Common DisbPtWar
    {
      get => disbPtWar ??= new();
      set => disbPtWar = value;
    }

    /// <summary>
    /// A value of DisbNtafWar.
    /// </summary>
    [JsonPropertyName("disbNtafWar")]
    [Member(Index = 16, AccessFields = false, Members
      = new[] { "TotalCurrency" })]
    public Common DisbNtafWar
    {
      get => disbNtafWar ??= new();
      set => disbNtafWar = value;
    }

    /// <summary>
    /// A value of DisbPtEft.
    /// </summary>
    [JsonPropertyName("disbPtEft")]
    [Member(Index = 17, AccessFields = false, Members
      = new[] { "TotalCurrency" })]
    public Common DisbPtEft
    {
      get => disbPtEft ??= new();
      set => disbPtEft = value;
    }

    /// <summary>
    /// A value of DisbNtafEft.
    /// </summary>
    [JsonPropertyName("disbNtafEft")]
    [Member(Index = 18, AccessFields = false, Members
      = new[] { "TotalCurrency" })]
    public Common DisbNtafEft
    {
      get => disbNtafEft ??= new();
      set => disbNtafEft = value;
    }

    /// <summary>
    /// A value of From.
    /// </summary>
    [JsonPropertyName("from")]
    [Member(Index = 19, AccessFields = false, Members = new[] { "Date" })]
    public DateWorkArea From
    {
      get => from ??= new();
      set => from = value;
    }

    /// <summary>
    /// A value of To.
    /// </summary>
    [JsonPropertyName("to")]
    [Member(Index = 20, AccessFields = false, Members = new[] { "Date" })]
    public DateWorkArea To
    {
      get => to ??= new();
      set => to = value;
    }

    /// <summary>
    /// A value of ReportPeriod.
    /// </summary>
    [JsonPropertyName("reportPeriod")]
    [Member(Index = 21, AccessFields = false, Members = new[] { "Text10" })]
    public WorkArea ReportPeriod
    {
      get => reportPeriod ??= new();
      set => reportPeriod = value;
    }

    /// <summary>
    /// A value of CashKsdlui.
    /// </summary>
    [JsonPropertyName("cashKsdlui")]
    [Member(Index = 22, AccessFields = false, Members
      = new[] { "TotalCurrency" })]
    public Common CashKsdlui
    {
      get => cashKsdlui ??= new();
      set => cashKsdlui = value;
    }

    /// <summary>
    /// A value of CashCssi.
    /// </summary>
    [JsonPropertyName("cashCssi")]
    [Member(Index = 23, AccessFields = false, Members
      = new[] { "TotalCurrency" })]
    public Common CashCssi
    {
      get => cashCssi ??= new();
      set => cashCssi = value;
    }

    private Common cashKpc;
    private Common cashFdso;
    private Common cashSdso;
    private Common cashMisc;
    private Common cashOther;
    private Common refundAdvancement;
    private Common distTaf;
    private Common distNonTaf;
    private Common distTafFc;
    private Common distGaFc;
    private Common distTafAdj;
    private Common distNonTafAdj;
    private Common distTafFcAdj;
    private Common distGaFcAdj;
    private Common disbPtWar;
    private Common disbNtafWar;
    private Common disbPtEft;
    private Common disbNtafEft;
    private DateWorkArea from;
    private DateWorkArea to;
    private WorkArea reportPeriod;
    private Common cashKsdlui;
    private Common cashCssi;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of ReportData.
      /// </summary>
      [JsonPropertyName("reportData")]
      [Member(Index = 1, AccessFields = false, Members = new[] { "LineText" })]
      public ReportData ReportData
      {
        get => reportData ??= new();
        set => reportData = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private ReportData reportData;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    [Member(Index = 1)]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    private Array<GroupGroup> group;
  }
#endregion
}
