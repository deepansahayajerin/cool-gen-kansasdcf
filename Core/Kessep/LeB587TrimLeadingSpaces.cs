// Program: LE_B587_TRIM_LEADING_SPACES, ID: 1902480627, model: 746.
// Short name: SWE00845
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_B587_TRIM_LEADING_SPACES.
/// </summary>
[Serializable]
public partial class LeB587TrimLeadingSpaces: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B587_TRIM_LEADING_SPACES program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB587TrimLeadingSpaces(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB587TrimLeadingSpaces.
  /// </summary>
  public LeB587TrimLeadingSpaces(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    export.FieldValue.Value = import.FieldValue.Value;

    if (IsEmpty(export.FieldValue.Value))
    {
      return;
    }

    local.Common.Count = Verify(export.FieldValue.Value, " ");
    export.FieldValue.Value =
      Substring(export.FieldValue.Value, local.Common.Count, 246 -
      local.Common.Count);
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

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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
