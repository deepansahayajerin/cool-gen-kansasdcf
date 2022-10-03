// Program: OE_B496_FIX_COVERAGE_CODE, ID: 371173815, model: 746.
// Short name: SWE02637
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B496_FIX_COVERAGE_CODE.
/// </summary>
[Serializable]
public partial class OeB496FixCoverageCode: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B496_FIX_COVERAGE_CODE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB496FixCoverageCode(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB496FixCoverageCode.
  /// </summary>
  public OeB496FixCoverageCode(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    switch(TrimEnd(import.CovCode.ActionEntry))
    {
      case "4":
        export.CovCode.ActionEntry = "HO";

        break;
      case "5":
        export.CovCode.ActionEntry = "MD";

        break;
      case "6":
        export.CovCode.ActionEntry = "DN";

        break;
      case "7":
        export.CovCode.ActionEntry = "DR";

        break;
      case "8":
        export.CovCode.ActionEntry = "OP";

        break;
      case "X":
        export.CovCode.ActionEntry = "HO";

        break;
      case "Y":
        export.CovCode.ActionEntry = "CA";

        break;
      default:
        export.CovCode.ActionEntry = import.CovCode.ActionEntry;

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
    /// <summary>
    /// A value of CovCode.
    /// </summary>
    [JsonPropertyName("covCode")]
    public Common CovCode
    {
      get => covCode ??= new();
      set => covCode = value;
    }

    private Common covCode;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CovCode.
    /// </summary>
    [JsonPropertyName("covCode")]
    public Common CovCode
    {
      get => covCode ??= new();
      set => covCode = value;
    }

    private Common covCode;
  }
#endregion
}
