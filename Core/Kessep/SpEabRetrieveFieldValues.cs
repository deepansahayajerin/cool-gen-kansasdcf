// Program: SP_EAB_RETRIEVE_FIELD_VALUES, ID: 372952341, model: 746.
// Short name: SWEXPE06
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_EAB_RETRIEVE_FIELD_VALUES.
/// </summary>
[Serializable]
public partial class SpEabRetrieveFieldValues: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_EAB_RETRIEVE_FIELD_VALUES program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpEabRetrieveFieldValues(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpEabRetrieveFieldValues.
  /// </summary>
  public SpEabRetrieveFieldValues(IContext context, Import import, Export export)
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
      "SWEXPE06", context, import, export, EabOptions.Hpvp);
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
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    [Member(Index = 1, AccessFields = false, Members
      = new[] { "FileInstruction" })]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    private External external;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of WsFieldValues.
    /// </summary>
    [JsonPropertyName("wsFieldValues")]
    [Member(Index = 1, AccessFields = false, Members = new[]
    {
      "ArchiveDate",
      "InfId",
      "DocName",
      "DocEffectiveDate",
      "FldName",
      "Valu",
      "CreatedBy",
      "CreatedTimestamp",
      "LastUpdatedBy",
      "LastUpdatedTstamp"
    })]
    public WsFieldValues WsFieldValues
    {
      get => wsFieldValues ??= new();
      set => wsFieldValues = value;
    }

    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    [Member(Index = 2, AccessFields = false, Members = new[]
    {
      "FileInstruction",
      "NumericReturnCode",
      "TextReturnCode",
      "TextLine80"
    })]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    private WsFieldValues wsFieldValues;
    private External external;
  }
#endregion
}
