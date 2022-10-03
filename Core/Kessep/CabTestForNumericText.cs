// Program: CAB_TEST_FOR_NUMERIC_TEXT, ID: 371456006, model: 746.
// Short name: SWE00097
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: CAB_TEST_FOR_NUMERIC_TEXT.
/// </summary>
[Serializable]
public partial class CabTestForNumericText: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_TEST_FOR_NUMERIC_TEXT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabTestForNumericText(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabTestForNumericText.
  /// </summary>
  public CabTestForNumericText(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    local.Numbers.Text12 = "0123456789";
    local.Common.Count = 1;

    for(var limit = Length(TrimEnd(import.WorkArea.Text12)); local
      .Common.Count <= limit; ++local.Common.Count)
    {
      if (Find(TrimEnd(local.Numbers.Text12), Substring(import.WorkArea.Text12,
        WorkArea.Text12_MaxLength, local.Common.Count, 1)) > 0)
      {
      }
      else
      {
        export.NumericText.Flag = "N";

        return;
      }
    }

    export.NumericText.Flag = "Y";
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
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of NumericText.
    /// </summary>
    [JsonPropertyName("numericText")]
    public Common NumericText
    {
      get => numericText ??= new();
      set => numericText = value;
    }

    private Common numericText;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Numbers.
    /// </summary>
    [JsonPropertyName("numbers")]
    public WorkArea Numbers
    {
      get => numbers ??= new();
      set => numbers = value;
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

    private WorkArea numbers;
    private Common common;
  }
#endregion
}
