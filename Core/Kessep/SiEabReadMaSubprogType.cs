// Program: SI_EAB_READ_MA_SUBPROG_TYPE, ID: 373425591, model: 746.
// Short name: SWEXIR70
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_EAB_READ_MA_SUBPROG_TYPE.
/// </summary>
[Serializable]
public partial class SiEabReadMaSubprogType: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_EAB_READ_MA_SUBPROG_TYPE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiEabReadMaSubprogType(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiEabReadMaSubprogType.
  /// </summary>
  public SiEabReadMaSubprogType(IContext context, Import import, Export export):
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
      "SWEXIR70", context, import, export, EabOptions.NoAS);
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "Number" })]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "Code" })]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    /// <summary>
    /// A value of Processing.
    /// </summary>
    [JsonPropertyName("processing")]
    [Member(Index = 3, AccessFields = false, Members = new[] { "Date" })]
    public DateWorkArea Processing
    {
      get => processing ??= new();
      set => processing = value;
    }

    private CsePerson csePerson;
    private Program program;
    private DateWorkArea processing;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of PersonProgram.
    /// </summary>
    [JsonPropertyName("personProgram")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "MedType" })]
    public PersonProgram PersonProgram
    {
      get => personProgram ??= new();
      set => personProgram = value;
    }

    private PersonProgram personProgram;
  }
#endregion
}
