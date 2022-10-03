// Program: SI_EAB_WRITE_PCG_MEDICAID_MATCH, ID: 370991935, model: 746.
// Short name: SWEXSI10
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_EAB_WRITE_PCG_MEDICAID_MATCH.
/// </summary>
[Serializable]
public partial class SiEabWritePcgMedicaidMatch: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_EAB_WRITE_PCG_MEDICAID_MATCH program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiEabWritePcgMedicaidMatch(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiEabWritePcgMedicaidMatch.
  /// </summary>
  public SiEabWritePcgMedicaidMatch(IContext context, Import import,
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
      "SWEXSI10", context, import, export, EabOptions.Hpvp);
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
    /// A value of ApCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("apCsePersonsWorkSet")]
    [Member(Index = 1, AccessFields = false, Members = new[]
    {
      "Number",
      "Dob",
      "Ssn",
      "FormattedName"
    })]
    public CsePersonsWorkSet ApCsePersonsWorkSet
    {
      get => apCsePersonsWorkSet ??= new();
      set => apCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Ch.
    /// </summary>
    [JsonPropertyName("ch")]
    [Member(Index = 2, AccessFields = false, Members = new[]
    {
      "Number",
      "Dob",
      "Ssn",
      "FormattedName"
    })]
    public CsePersonsWorkSet Ch
    {
      get => ch ??= new();
      set => ch = value;
    }

    /// <summary>
    /// A value of ApCase.
    /// </summary>
    [JsonPropertyName("apCase")]
    [Member(Index = 3, AccessFields = false, Members = new[] { "Number" })]
    public Case1 ApCase
    {
      get => apCase ??= new();
      set => apCase = value;
    }

    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    [Member(Index = 4, AccessFields = false, Members
      = new[] { "FileInstruction" })]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    private CsePersonsWorkSet apCsePersonsWorkSet;
    private CsePersonsWorkSet ch;
    private Case1 apCase;
    private External external;
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
    [Member(Index = 1, AccessFields = false, Members
      = new[] { "FileInstruction", "TextReturnCode" })]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    private External external;
  }
#endregion
}
