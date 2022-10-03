// Program: EAB_DATE_VALIDATION_ROUTINE, ID: 373496223, model: 746.
// Short name: SWEXEE29
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: EAB_DATE_VALIDATION_ROUTINE.
/// </summary>
[Serializable]
public partial class EabDateValidationRoutine: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_DATE_VALIDATION_ROUTINE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabDateValidationRoutine(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabDateValidationRoutine.
  /// </summary>
  public EabDateValidationRoutine(IContext context, Import import, Export export)
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
    GetService<IEabStub>().Execute(
      "SWEXEE29", context, import, export, EabOptions.Hpvp);
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
    [Member(Index = 1, AccessFields = false, Members = new[] { "TextDate" })]
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
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "Date" })]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of Validity.
    /// </summary>
    [JsonPropertyName("validity")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "ActionEntry" })]
    public Common Validity
    {
      get => validity ??= new();
      set => validity = value;
    }

    private DateWorkArea dateWorkArea;
    private Common validity;
  }
#endregion
}
