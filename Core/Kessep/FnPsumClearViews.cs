// Program: FN_PSUM_CLEAR_VIEWS, ID: 374421448, model: 746.
// Short name: SWE02879
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_PSUM_CLEAR_VIEWS.
/// </summary>
[Serializable]
public partial class FnPsumClearViews: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_PSUM_CLEAR_VIEWS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnPsumClearViews(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnPsumClearViews.
  /// </summary>
  public FnPsumClearViews(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
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
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
      /// <summary>
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
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

      /// <summary>
      /// A value of Yy.
      /// </summary>
      [JsonPropertyName("yy")]
      public TextWorkArea Yy
      {
        get => yy ??= new();
        set => yy = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 13;

      private Common common;
      private MonthlyObligeeSummary monthlyObligeeSummary;
      private TextWorkArea yy;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 =>
      export1 ??= new(ExportGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Export1 for json serialization.
    /// </summary>
    [JsonPropertyName("export1")]
    [Computed]
    public IList<ExportGroup> Export1_Json
    {
      get => export1;
      set => Export1.Assign(value);
    }

    private Array<ExportGroup> export1;
  }
#endregion
}
