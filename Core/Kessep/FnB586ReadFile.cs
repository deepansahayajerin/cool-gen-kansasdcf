// Program: FN_B586_READ_FILE, ID: 1902467262, model: 746.
// Short name: SWEXER22
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B586_READ_FILE.
/// </summary>
[Serializable]
public partial class FnB586ReadFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B586_READ_FILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB586ReadFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB586ReadFile.
  /// </summary>
  public FnB586ReadFile(IContext context, Import import, Export export):
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
      "SWEXER22", context, import, export, EabOptions.Hpvp);
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
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    [Member(Index = 1, AccessFields = false, Members = new[]
    {
      "Ein",
      "Name",
      "PhoneNo",
      "AreaCode",
      "EiwoEndDate"
    })]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    /// <summary>
    /// A value of EmployerAddress.
    /// </summary>
    [JsonPropertyName("employerAddress")]
    [Member(Index = 2, AccessFields = false, Members = new[]
    {
      "LocationType",
      "Street1",
      "Street2",
      "City",
      "State",
      "ZipCode",
      "Zip4"
    })]
    public EmployerAddress EmployerAddress
    {
      get => employerAddress ??= new();
      set => employerAddress = value;
    }

    /// <summary>
    /// A value of ActiveInd.
    /// </summary>
    [JsonPropertyName("activeInd")]
    [Member(Index = 3, AccessFields = false, Members = new[] { "Text1" })]
    public WorkArea ActiveInd
    {
      get => activeInd ??= new();
      set => activeInd = value;
    }

    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    [Member(Index = 4, AccessFields = false, Members = new[]
    {
      "NumericReturnCode",
      "TextReturnCode",
      "TextLine80"
    })]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    private Employer employer;
    private EmployerAddress employerAddress;
    private WorkArea activeInd;
    private External external;
  }
#endregion
}
