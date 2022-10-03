// Program: LE_EAB_READ_REFER_CASE_FUNCTION, ID: 374511076, model: 746.
// Short name: SWEXER17
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_EAB_READ_REFER_CASE_FUNCTION.
/// </summary>
[Serializable]
public partial class LeEabReadReferCaseFunction: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_EAB_READ_REFER_CASE_FUNCTION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeEabReadReferCaseFunction(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeEabReadReferCaseFunction.
  /// </summary>
  public LeEabReadReferCaseFunction(IContext context, Import import,
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
      "SWEXER17", context, import, export, EabOptions.Hpvp);
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
    /// A value of Legal.
    /// </summary>
    [JsonPropertyName("legal")]
    [Member(Index = 1, Members = new[]
    {
      "LastName",
      "FirstName",
      "SystemGeneratedId"
    })]
    public ServiceProvider Legal
    {
      get => legal ??= new();
      set => legal = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "Name" })]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of CaseFuncWorkSet.
    /// </summary>
    [JsonPropertyName("caseFuncWorkSet")]
    [Member(Index = 3, AccessFields = false, Members = new[] { "FuncText1" })]
    public CaseFuncWorkSet CaseFuncWorkSet
    {
      get => caseFuncWorkSet ??= new();
      set => caseFuncWorkSet = value;
    }

    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    [Member(Index = 4, AccessFields = false, Members = new[]
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

    private ServiceProvider legal;
    private Office office;
    private CaseFuncWorkSet caseFuncWorkSet;
    private External external;
  }
#endregion
}
