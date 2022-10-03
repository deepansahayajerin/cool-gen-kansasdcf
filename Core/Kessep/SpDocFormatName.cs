// Program: SP_DOC_FORMAT_NAME, ID: 372134095, model: 746.
// Short name: SWE01651
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_DOC_FORMAT_NAME.
/// </para>
/// <para>
/// Formats name as First MI. Last or
/// First Last if no MI
/// </para>
/// </summary>
[Serializable]
public partial class SpDocFormatName: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_DOC_FORMAT_NAME program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpDocFormatName(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpDocFormatName.
  /// </summary>
  public SpDocFormatName(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (!IsEmpty(import.SpPrintWorkSet.MidInitial))
    {
      // ±æææææææææææææææææææææææææææææææææææææææææææÉ
      // ø Format name as "First MI. Last"           ø
      // þæææææææææææææææææææææææææææææææææææææææææææÊ
      export.FieldValue.Value = TrimEnd(import.SpPrintWorkSet.FirstName) + " " +
        import.SpPrintWorkSet.MidInitial;
      export.FieldValue.Value = TrimEnd(export.FieldValue.Value) + ". " + import
        .SpPrintWorkSet.LastName;
    }
    else
    {
      // ±æææææææææææææææææææææææææææææææææææææææææææÉ
      // ø Format name as "First Last"               ø
      // þæææææææææææææææææææææææææææææææææææææææææææÊ
      export.FieldValue.Value = TrimEnd(import.SpPrintWorkSet.FirstName) + " " +
        TrimEnd(import.SpPrintWorkSet.LastName);
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
    /// A value of SpPrintWorkSet.
    /// </summary>
    [JsonPropertyName("spPrintWorkSet")]
    public SpPrintWorkSet SpPrintWorkSet
    {
      get => spPrintWorkSet ??= new();
      set => spPrintWorkSet = value;
    }

    private SpPrintWorkSet spPrintWorkSet;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of FieldValue.
    /// </summary>
    [JsonPropertyName("fieldValue")]
    public FieldValue FieldValue
    {
      get => fieldValue ??= new();
      set => fieldValue = value;
    }

    private FieldValue fieldValue;
  }
#endregion
}
