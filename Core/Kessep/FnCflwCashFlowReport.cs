// Program: FN_CFLW_CASH_FLOW_REPORT, ID: 371216703, model: 746.
// Short name: SWECFLWP
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_CFLW_CASH_FLOW_REPORT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnCflwCashFlowReport: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CFLW_CASH_FLOW_REPORT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCflwCashFlowReport(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCflwCashFlowReport.
  /// </summary>
  public FnCflwCashFlowReport(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    ExitState = "ACO_NN0000_ALL_OK";
    export.SearchFrom.Date = import.SearchFrom.Date;
    export.SearchTo.Date = import.SearchTo.Date;
    export.ProcessCashInd.Flag = import.ProcessCashInd.Flag;
    export.ProcessDisbInd.Flag = import.ProcessDisbInd.Flag;
    export.ProcessDistInd.Flag = import.ProcessDistInd.Flag;

    export.Group.Index = 0;
    export.Group.Clear();

    for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
      import.Group.Index)
    {
      if (export.Group.IsFull)
      {
        break;
      }

      export.Group.Update.Label.Text10 = import.Group.Item.Label.Text10;
      export.Group.Update.Common.TotalCurrency =
        import.Group.Item.Common.TotalCurrency;
      export.Group.Next();
    }

    if (Equal(export.SearchFrom.Date, local.Null1.Date))
    {
      export.SearchFrom.Date = Now().Date;
    }

    if (Equal(export.SearchTo.Date, local.Null1.Date))
    {
      export.SearchTo.Date = Now().Date;
    }

    if (Lt(export.SearchTo.Date, export.SearchFrom.Date))
    {
      ExitState = "FROM_DATE_GREATER_THAN_TO_DATE";

      return;
    }

    if (AsChar(export.ProcessCashInd.Flag) != 'Y')
    {
      export.ProcessCashInd.Flag = "N";
    }

    if (AsChar(export.ProcessDisbInd.Flag) != 'Y')
    {
      export.ProcessDisbInd.Flag = "N";
    }

    if (AsChar(export.ProcessDistInd.Flag) != 'Y')
    {
      export.ProcessDistInd.Flag = "N";
    }

    switch(TrimEnd(global.Command))
    {
      case "PROCESS":
        local.EnvCd.Text10 = "ONLINE";
        UseFnB615CashFlowCab();

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "NEXT":
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveOnlineToGroup(FnB615CashFlowCab.Export.
    OnlineGroup source, Export.GroupGroup target)
  {
    target.Label.Text10 = source.LabelOnline.Text10;
    target.Common.TotalCurrency = source.Online1.TotalCurrency;
  }

  private void UseFnB615CashFlowCab()
  {
    var useImport = new FnB615CashFlowCab.Import();
    var useExport = new FnB615CashFlowCab.Export();

    useImport.EnvCd.Text10 = local.EnvCd.Text10;
    useImport.SearchTo.Date = export.SearchTo.Date;
    useImport.SearchFrom.Date = export.SearchFrom.Date;
    useImport.ProcessCashInd.Flag = export.ProcessCashInd.Flag;
    useImport.ProcessDistInd.Flag = export.ProcessDistInd.Flag;
    useImport.ProcessDisbInd.Flag = export.ProcessDisbInd.Flag;

    Call(FnB615CashFlowCab.Execute, useImport, useExport);

    useExport.Online.CopyTo(export.Group, MoveOnlineToGroup);
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
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of Label.
      /// </summary>
      [JsonPropertyName("label")]
      public TextWorkArea Label
      {
        get => label ??= new();
        set => label = value;
      }

      /// <summary>
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private TextWorkArea label;
      private Common common;
    }

    /// <summary>
    /// A value of SearchFrom.
    /// </summary>
    [JsonPropertyName("searchFrom")]
    public DateWorkArea SearchFrom
    {
      get => searchFrom ??= new();
      set => searchFrom = value;
    }

    /// <summary>
    /// A value of SearchTo.
    /// </summary>
    [JsonPropertyName("searchTo")]
    public DateWorkArea SearchTo
    {
      get => searchTo ??= new();
      set => searchTo = value;
    }

    /// <summary>
    /// A value of ProcessCashInd.
    /// </summary>
    [JsonPropertyName("processCashInd")]
    public Common ProcessCashInd
    {
      get => processCashInd ??= new();
      set => processCashInd = value;
    }

    /// <summary>
    /// A value of ProcessDistInd.
    /// </summary>
    [JsonPropertyName("processDistInd")]
    public Common ProcessDistInd
    {
      get => processDistInd ??= new();
      set => processDistInd = value;
    }

    /// <summary>
    /// A value of ProcessDisbInd.
    /// </summary>
    [JsonPropertyName("processDisbInd")]
    public Common ProcessDisbInd
    {
      get => processDisbInd ??= new();
      set => processDisbInd = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

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

    private DateWorkArea searchFrom;
    private DateWorkArea searchTo;
    private Common processCashInd;
    private Common processDistInd;
    private Common processDisbInd;
    private Array<GroupGroup> group;
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
      /// A value of Label.
      /// </summary>
      [JsonPropertyName("label")]
      public TextWorkArea Label
      {
        get => label ??= new();
        set => label = value;
      }

      /// <summary>
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private TextWorkArea label;
      private Common common;
    }

    /// <summary>
    /// A value of SearchTo.
    /// </summary>
    [JsonPropertyName("searchTo")]
    public DateWorkArea SearchTo
    {
      get => searchTo ??= new();
      set => searchTo = value;
    }

    /// <summary>
    /// A value of SearchFrom.
    /// </summary>
    [JsonPropertyName("searchFrom")]
    public DateWorkArea SearchFrom
    {
      get => searchFrom ??= new();
      set => searchFrom = value;
    }

    /// <summary>
    /// A value of ProcessCashInd.
    /// </summary>
    [JsonPropertyName("processCashInd")]
    public Common ProcessCashInd
    {
      get => processCashInd ??= new();
      set => processCashInd = value;
    }

    /// <summary>
    /// A value of ProcessDistInd.
    /// </summary>
    [JsonPropertyName("processDistInd")]
    public Common ProcessDistInd
    {
      get => processDistInd ??= new();
      set => processDistInd = value;
    }

    /// <summary>
    /// A value of ProcessDisbInd.
    /// </summary>
    [JsonPropertyName("processDisbInd")]
    public Common ProcessDisbInd
    {
      get => processDisbInd ??= new();
      set => processDisbInd = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

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

    private DateWorkArea searchTo;
    private DateWorkArea searchFrom;
    private Common processCashInd;
    private Common processDistInd;
    private Common processDisbInd;
    private Array<GroupGroup> group;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of EnvCd.
    /// </summary>
    [JsonPropertyName("envCd")]
    public WorkArea EnvCd
    {
      get => envCd ??= new();
      set => envCd = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    private WorkArea envCd;
    private DateWorkArea null1;
    private DateWorkArea max;
    private DateWorkArea current;
  }
#endregion
}
