// Program: CAB_LPSP_SORT_GROUP_VIEW, ID: 373524598, model: 746.
// Short name: SWE00102
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: CAB_LPSP_SORT_GROUP_VIEW.
/// </summary>
[Serializable]
public partial class CabLpspSortGroupView: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_LPSP_SORT_GROUP_VIEW program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabLpspSortGroupView(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabLpspSortGroupView.
  /// </summary>
  public CabLpspSortGroupView(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ****************************************************
    // 2/21/2000 - Kalpesh Doshi - Initial version
    // Description - CAB sorts import GV in ascending order of formatted_name.
    // 9/18/2000 - Fangman - PRWORA SEG ID A4 - Added new suppression rule 
    // attribute.
    // ****************************************************
    local.ChangedFlag.Flag = "T";

    while(AsChar(local.ChangedFlag.Flag) == 'T')
    {
      local.ChangedFlag.Flag = "F";

      import.Import1.Index = 0;
      import.Import1.CheckSize();

      while(import.Import1.Index + 1 < import.Import1.Count)
      {
        // **************************
        // Move (i)th element to local_temp1
        // **************************
        local.Temp1CollectionType.Code =
          import.Import1.Item.DetailCollectionType.Code;
        local.Temp1DisbSuppressionStatusHistory.Assign(
          import.Import1.Item.DetailDisbSuppressionStatusHistory);
        local.Temp1Common.SelectChar =
          import.Import1.Item.DetailCommon.SelectChar;
        MoveCsePersonsWorkSet(import.Import1.Item.DetailPayee,
          local.Temp1CsePersonsWorkSet);

        // **************************
        // Move (i+1)th element to local_temp2
        // **************************
        ++import.Import1.Index;
        import.Import1.CheckSize();

        local.Temp2CollectionType.Code =
          import.Import1.Item.DetailCollectionType.Code;
        local.Temp2DisbSuppressionStatusHistory.Assign(
          import.Import1.Item.DetailDisbSuppressionStatusHistory);
        local.Temp2Common.SelectChar =
          import.Import1.Item.DetailCommon.SelectChar;
        MoveCsePersonsWorkSet(import.Import1.Item.DetailPayee,
          local.Temp2CsePersonsWorkSet);

        // **************************
        // Compare and swap if necessary
        // **************************
        if (Lt(local.Temp2CsePersonsWorkSet.FormattedName,
          local.Temp1CsePersonsWorkSet.FormattedName))
        {
          local.ChangedFlag.Flag = "T";

          --import.Import1.Index;
          import.Import1.CheckSize();

          import.Import1.Update.DetailCommon.SelectChar =
            local.Temp2Common.SelectChar;
          MoveCsePersonsWorkSet(local.Temp2CsePersonsWorkSet,
            import.Import1.Update.DetailPayee);
          import.Import1.Update.DetailDisbSuppressionStatusHistory.Assign(
            local.Temp2DisbSuppressionStatusHistory);
          import.Import1.Update.DetailCollectionType.Code =
            local.Temp2CollectionType.Code;

          ++import.Import1.Index;
          import.Import1.CheckSize();

          import.Import1.Update.DetailDisbSuppressionStatusHistory.Assign(
            local.Temp1DisbSuppressionStatusHistory);
          import.Import1.Update.DetailCommon.SelectChar =
            local.Temp1Common.SelectChar;
          MoveCsePersonsWorkSet(local.Temp1CsePersonsWorkSet,
            import.Import1.Update.DetailPayee);
          import.Import1.Update.DetailCollectionType.Code =
            local.Temp1CollectionType.Code;
        }
      }
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
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
    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
      /// <summary>
      /// A value of DetailCommon.
      /// </summary>
      [JsonPropertyName("detailCommon")]
      public Common DetailCommon
      {
        get => detailCommon ??= new();
        set => detailCommon = value;
      }

      /// <summary>
      /// A value of DetailPayee.
      /// </summary>
      [JsonPropertyName("detailPayee")]
      public CsePersonsWorkSet DetailPayee
      {
        get => detailPayee ??= new();
        set => detailPayee = value;
      }

      /// <summary>
      /// A value of DetailDisbSuppressionStatusHistory.
      /// </summary>
      [JsonPropertyName("detailDisbSuppressionStatusHistory")]
      public DisbSuppressionStatusHistory DetailDisbSuppressionStatusHistory
      {
        get => detailDisbSuppressionStatusHistory ??= new();
        set => detailDisbSuppressionStatusHistory = value;
      }

      /// <summary>
      /// A value of DetailCollectionType.
      /// </summary>
      [JsonPropertyName("detailCollectionType")]
      public CollectionType DetailCollectionType
      {
        get => detailCollectionType ??= new();
        set => detailCollectionType = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 205;

      private Common detailCommon;
      private CsePersonsWorkSet detailPayee;
      private DisbSuppressionStatusHistory detailDisbSuppressionStatusHistory;
      private CollectionType detailCollectionType;
    }

    /// <summary>
    /// Gets a value of Import1.
    /// </summary>
    [JsonIgnore]
    public Array<ImportGroup> Import1 =>
      import1 ??= new(ImportGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Import1 for json serialization.
    /// </summary>
    [JsonPropertyName("import1")]
    [Computed]
    public IList<ImportGroup> Import1_Json
    {
      get => import1;
      set => Import1.Assign(value);
    }

    private Array<ImportGroup> import1;
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
    /// A value of Temp1Common.
    /// </summary>
    [JsonPropertyName("temp1Common")]
    public Common Temp1Common
    {
      get => temp1Common ??= new();
      set => temp1Common = value;
    }

    /// <summary>
    /// A value of Temp1CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("temp1CsePersonsWorkSet")]
    public CsePersonsWorkSet Temp1CsePersonsWorkSet
    {
      get => temp1CsePersonsWorkSet ??= new();
      set => temp1CsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Temp1DisbSuppressionStatusHistory.
    /// </summary>
    [JsonPropertyName("temp1DisbSuppressionStatusHistory")]
    public DisbSuppressionStatusHistory Temp1DisbSuppressionStatusHistory
    {
      get => temp1DisbSuppressionStatusHistory ??= new();
      set => temp1DisbSuppressionStatusHistory = value;
    }

    /// <summary>
    /// A value of Temp1CollectionType.
    /// </summary>
    [JsonPropertyName("temp1CollectionType")]
    public CollectionType Temp1CollectionType
    {
      get => temp1CollectionType ??= new();
      set => temp1CollectionType = value;
    }

    /// <summary>
    /// A value of Temp2Common.
    /// </summary>
    [JsonPropertyName("temp2Common")]
    public Common Temp2Common
    {
      get => temp2Common ??= new();
      set => temp2Common = value;
    }

    /// <summary>
    /// A value of Temp2CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("temp2CsePersonsWorkSet")]
    public CsePersonsWorkSet Temp2CsePersonsWorkSet
    {
      get => temp2CsePersonsWorkSet ??= new();
      set => temp2CsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Temp2DisbSuppressionStatusHistory.
    /// </summary>
    [JsonPropertyName("temp2DisbSuppressionStatusHistory")]
    public DisbSuppressionStatusHistory Temp2DisbSuppressionStatusHistory
    {
      get => temp2DisbSuppressionStatusHistory ??= new();
      set => temp2DisbSuppressionStatusHistory = value;
    }

    /// <summary>
    /// A value of Temp2CollectionType.
    /// </summary>
    [JsonPropertyName("temp2CollectionType")]
    public CollectionType Temp2CollectionType
    {
      get => temp2CollectionType ??= new();
      set => temp2CollectionType = value;
    }

    /// <summary>
    /// A value of ChangedFlag.
    /// </summary>
    [JsonPropertyName("changedFlag")]
    public Common ChangedFlag
    {
      get => changedFlag ??= new();
      set => changedFlag = value;
    }

    private Common temp1Common;
    private CsePersonsWorkSet temp1CsePersonsWorkSet;
    private DisbSuppressionStatusHistory temp1DisbSuppressionStatusHistory;
    private CollectionType temp1CollectionType;
    private Common temp2Common;
    private CsePersonsWorkSet temp2CsePersonsWorkSet;
    private DisbSuppressionStatusHistory temp2DisbSuppressionStatusHistory;
    private CollectionType temp2CollectionType;
    private Common changedFlag;
  }
#endregion
}
