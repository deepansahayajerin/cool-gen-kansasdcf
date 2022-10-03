// Program: SP_EAB_ARCHIVE_FIELD_VALUES_RS, ID: 372968385, model: 746.
// Short name: SWEXPE05
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_EAB_ARCHIVE_FIELD_VALUES_RS.
/// </summary>
[Serializable]
public partial class SpEabArchiveFieldValuesRs: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_EAB_ARCHIVE_FIELD_VALUES_RS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpEabArchiveFieldValuesRs(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpEabArchiveFieldValuesRs.
  /// </summary>
  public SpEabArchiveFieldValuesRs(IContext context, Import import,
    Export export):
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
      "SWEXPE05", context, import, export, EabOptions.Hpvp);
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

    /// <summary>
    /// A value of WsFieldValues.
    /// </summary>
    [JsonPropertyName("wsFieldValues")]
    [Member(Index = 2, AccessFields = false, Members = new[]
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
    /// A value of LocalRecCount.
    /// </summary>
    [JsonPropertyName("localRecCount")]
    [Member(Index = 3, AccessFields = false, Members = new[] { "Count" })]
    public Common LocalRecCount
    {
      get => localRecCount ??= new();
      set => localRecCount = value;
    }

    private External external;
    private WsFieldValues wsFieldValues;
    private Common localRecCount;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    [Member(Index = 1, AccessFields = false, Members = new[]
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

    private External external;
  }
#endregion
}
