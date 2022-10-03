// Program: FN_CONVERT_TMST_TO_NUM, ID: 373392165, model: 746.
// Short name: SWE02745
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_CONVERT_TMST_TO_NUM.
/// </summary>
[Serializable]
public partial class FnConvertTmstToNum: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CONVERT_TMST_TO_NUM program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnConvertTmstToNum(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnConvertTmstToNum.
  /// </summary>
  public FnConvertTmstToNum(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    export.Common.TotalInteger =
      (long)Microsecond(import.DateWorkArea.Timestamp) + Second
      (import.DateWorkArea.Timestamp) + Minute
      (import.DateWorkArea.Timestamp) + Hour(import.DateWorkArea.Timestamp) + Day
      (import.DateWorkArea.Timestamp) + Month(import.DateWorkArea.Timestamp) + Year
      (import.DateWorkArea.Timestamp);
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
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    private DateWorkArea dateWorkArea;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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
#endregion
}
