// Program: FN_B677_READ_AE_INTERFACE, ID: 371258838, model: 746.
// Short name: SWEXFE48
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B677_READ_AE_INTERFACE.
/// </summary>
[Serializable]
public partial class FnB677ReadAeInterface: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B677_READ_AE_INTERFACE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB677ReadAeInterface(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB677ReadAeInterface.
  /// </summary>
  public FnB677ReadAeInterface(IContext context, Import import, Export export):
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
      "SWEXFE48", context, import, export, EabOptions.Hpvp);
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "Action" })]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private EabFileHandling eabFileHandling;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of DisplacedPerson.
    /// </summary>
    [JsonPropertyName("displacedPerson")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "Number" })]
    public DisplacedPerson DisplacedPerson
    {
      get => displacedPerson ??= new();
      set => displacedPerson = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "Status" })]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private DisplacedPerson displacedPerson;
    private EabFileHandling eabFileHandling;
  }
#endregion
}
