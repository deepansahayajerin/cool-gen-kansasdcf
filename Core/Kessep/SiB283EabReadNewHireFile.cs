// Program: SI_B283_EAB_READ_NEW_HIRE_FILE, ID: 1902519444, model: 746.
// Short name: SWEXIE02
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B283_EAB_READ_NEW_HIRE_FILE.
/// </summary>
[Serializable]
public partial class SiB283EabReadNewHireFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B283_EAB_READ_NEW_HIRE_FILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB283EabReadNewHireFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB283EabReadNewHireFile.
  /// </summary>
  public SiB283EabReadNewHireFile(IContext context, Import import, Export export)
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
      "SWEXIE02", context, import, export, EabOptions.Hpvp);
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "Status" })]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of NewHireInitiativeRecord.
    /// </summary>
    [JsonPropertyName("newHireInitiativeRecord")]
    [Member(Index = 2, AccessFields = false, Members = new[]
    {
      "Fein",
      "KansasId",
      "EmployerName",
      "AddressLine1",
      "AddressLine2",
      "City",
      "State",
      "ZipCode",
      "ZipExtension",
      "PersonNumber",
      "HireDate"
    })]
    public NewHireInitiativeRecord NewHireInitiativeRecord
    {
      get => newHireInitiativeRecord ??= new();
      set => newHireInitiativeRecord = value;
    }

    private EabFileHandling eabFileHandling;
    private NewHireInitiativeRecord newHireInitiativeRecord;
  }
#endregion
}
