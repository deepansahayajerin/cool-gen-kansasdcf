// Program: FN_B795_CONVERT_NUM_TO_TEXT, ID: 1902457338, model: 746.
// Short name: SWE03737
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B795_CONVERT_NUM_TO_TEXT.
/// </summary>
[Serializable]
public partial class FnB795ConvertNumToText: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B795_CONVERT_NUM_TO_TEXT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB795ConvertNumToText(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB795ConvertNumToText.
  /// </summary>
  public FnB795ConvertNumToText(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // Convert currency values (numeric 15 with 2 decimal places) to 15 
    // character text
    local.WorkArea.Text15 =
      NumberToString((long)(import.Common.TotalCurrency * 100), 15);
    export.WorkArea.Text15 =
      Substring(local.WorkArea.Text15, WorkArea.Text15_MaxLength, 2, 12) + "."
      + Substring(local.WorkArea.Text15, WorkArea.Text15_MaxLength, 14, 2);

    if (import.Common.TotalCurrency < 0)
    {
      export.WorkArea.Text15 = "-" + Substring
        (export.WorkArea.Text15, WorkArea.Text15_MaxLength, 2, 14);
    }
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    private Common common;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of WorkArea.
    /// </summary>
    [JsonPropertyName("workArea")]
    public WorkArea WorkArea
    {
      get => workArea ??= new();
      set => workArea = value;
    }

    private WorkArea workArea;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of WorkArea.
    /// </summary>
    [JsonPropertyName("workArea")]
    public WorkArea WorkArea
    {
      get => workArea ??= new();
      set => workArea = value;
    }

    private WorkArea workArea;
  }
#endregion
}
