// Program: EAB_READ_FEDERAL_EMPLOYER_FILE, ID: 373411379, model: 746.
// Short name: SWEXIC06
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: EAB_READ_FEDERAL_EMPLOYER_FILE.
/// </summary>
[Serializable]
public partial class EabReadFederalEmployerFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_READ_FEDERAL_EMPLOYER_FILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabReadFederalEmployerFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabReadFederalEmployerFile.
  /// </summary>
  public EabReadFederalEmployerFile(IContext context, Import import,
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
      "SWEXIC06", context, import, export, EabOptions.Hpvp);
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
    /// A value of ImportEmployer.
    /// </summary>
    [JsonPropertyName("importEmployer")]
    [Member(Index = 1, AccessFields = false, Members = new[]
    {
      "Ein",
      "Name",
      "PhoneNo",
      "AreaCode"
    })]
    public Employer ImportEmployer
    {
      get => importEmployer ??= new();
      set => importEmployer = value;
    }

    /// <summary>
    /// A value of ImportEmployerAddress.
    /// </summary>
    [JsonPropertyName("importEmployerAddress")]
    [Member(Index = 2, AccessFields = false, Members = new[]
    {
      "LocationType",
      "Street1",
      "Street2",
      "Street3",
      "Street4",
      "City",
      "State",
      "ZipCode",
      "Zip4",
      "Note"
    })]
    public EmployerAddress ImportEmployerAddress
    {
      get => importEmployerAddress ??= new();
      set => importEmployerAddress = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    [Member(Index = 3, AccessFields = false, Members = new[] { "Status" })]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private Employer importEmployer;
    private EmployerAddress importEmployerAddress;
    private EabFileHandling eabFileHandling;
  }
#endregion
}
