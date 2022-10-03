// Program: FN_B717_INFLATE_GV_BY_COUNTER, ID: 373350750, model: 746.
// Short name: SWE03037
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B717_INFLATE_GV_BY_COUNTER.
/// </summary>
[Serializable]
public partial class FnB717InflateGvByCounter: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B717_INFLATE_GV_BY_COUNTER program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB717InflateGvByCounter(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB717InflateGvByCounter.
  /// </summary>
  public FnB717InflateGvByCounter(IContext context, Import import, Export export)
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
    import.Group.Index = import.Subscript.Count - 1;
    import.Group.CheckSize();

    switch(import.Program.SystemGeneratedIdentifier)
    {
      case 1:
        import.Group.Update.StatsReport.Column1 =
          import.Group.Item.StatsReport.Column1.GetValueOrDefault() + import
          .Increment.Column1.GetValueOrDefault();

        break;
      case 2:
        import.Group.Update.StatsReport.Column2 =
          import.Group.Item.StatsReport.Column2.GetValueOrDefault() + import
          .Increment.Column1.GetValueOrDefault();

        break;
      case 3:
        import.Group.Update.StatsReport.Column3 =
          import.Group.Item.StatsReport.Column3.GetValueOrDefault() + import
          .Increment.Column1.GetValueOrDefault();

        break;
      case 4:
        import.Group.Update.StatsReport.Column4 =
          import.Group.Item.StatsReport.Column4.GetValueOrDefault() + import
          .Increment.Column1.GetValueOrDefault();

        break;
      case 5:
        import.Group.Update.StatsReport.Column5 =
          import.Group.Item.StatsReport.Column5.GetValueOrDefault() + import
          .Increment.Column1.GetValueOrDefault();

        break;
      case 6:
        import.Group.Update.StatsReport.Column6 =
          import.Group.Item.StatsReport.Column6.GetValueOrDefault() + import
          .Increment.Column1.GetValueOrDefault();

        break;
      case 7:
        import.Group.Update.StatsReport.Column7 =
          import.Group.Item.StatsReport.Column7.GetValueOrDefault() + import
          .Increment.Column1.GetValueOrDefault();

        break;
      case 8:
        import.Group.Update.StatsReport.Column8 =
          import.Group.Item.StatsReport.Column8.GetValueOrDefault() + import
          .Increment.Column1.GetValueOrDefault();

        break;
      case 9:
        import.Group.Update.StatsReport.Column9 =
          import.Group.Item.StatsReport.Column9.GetValueOrDefault() + import
          .Increment.Column1.GetValueOrDefault();

        break;
      case 10:
        import.Group.Update.StatsReport.Column10 =
          import.Group.Item.StatsReport.Column10.GetValueOrDefault() + import
          .Increment.Column1.GetValueOrDefault();

        break;
      case 11:
        import.Group.Update.StatsReport.Column11 =
          import.Group.Item.StatsReport.Column11.GetValueOrDefault() + import
          .Increment.Column1.GetValueOrDefault();

        break;
      case 12:
        import.Group.Update.StatsReport.Column12 =
          import.Group.Item.StatsReport.Column12.GetValueOrDefault() + import
          .Increment.Column1.GetValueOrDefault();

        break;
      case 13:
        import.Group.Update.StatsReport.Column13 =
          import.Group.Item.StatsReport.Column13.GetValueOrDefault() + import
          .Increment.Column1.GetValueOrDefault();

        break;
      case 14:
        import.Group.Update.StatsReport.Column14 =
          import.Group.Item.StatsReport.Column14.GetValueOrDefault() + import
          .Increment.Column1.GetValueOrDefault();

        break;
      case 15:
        import.Group.Update.StatsReport.Column15 =
          import.Group.Item.StatsReport.Column15.GetValueOrDefault() + import
          .Increment.Column1.GetValueOrDefault();

        break;
      default:
        break;
    }
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
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of StatsReport.
      /// </summary>
      [JsonPropertyName("statsReport")]
      public StatsReport StatsReport
      {
        get => statsReport ??= new();
        set => statsReport = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 35;

      private StatsReport statsReport;
    }

    /// <summary>
    /// A value of Increment.
    /// </summary>
    [JsonPropertyName("increment")]
    public StatsReport Increment
    {
      get => increment ??= new();
      set => increment = value;
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
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
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

    /// <summary>
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    private StatsReport increment;
    private Common subscript;
    private Array<GroupGroup> group;
    private Program program;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }
#endregion
}
