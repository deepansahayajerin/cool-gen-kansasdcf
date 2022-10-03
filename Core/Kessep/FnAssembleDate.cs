// Program: FN_ASSEMBLE_DATE, ID: 371969655, model: 746.
// Short name: SWE00264
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_ASSEMBLE_DATE.
/// </para>
/// <para>
/// RESP: FINANCE
/// This CAB is defines to receive as separate attributes the month, day and 
/// year and assemble them into a IEF defined date attribute.
/// </para>
/// </summary>
[Serializable]
public partial class FnAssembleDate: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_ASSEMBLE_DATE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnAssembleDate(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnAssembleDate.
  /// </summary>
  public FnAssembleDate(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    export.DateWorkArea.Date = IntToDate(import.DateValidation.Year * 10000 + import
      .DateValidation.Month * 100 + import.DateValidation.Day);
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
    /// A value of DateValidation.
    /// </summary>
    [JsonPropertyName("dateValidation")]
    public DateValidation DateValidation
    {
      get => dateValidation ??= new();
      set => dateValidation = value;
    }

    private DateValidation dateValidation;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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
#endregion
}
