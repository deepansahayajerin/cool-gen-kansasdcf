// Program: SI_B287_READ_DL_MATCH_FILE, ID: 1625320902, model: 746.
// Short name: SWEXIE05
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B287_READ_DL_MATCH_FILE.
/// </summary>
[Serializable]
public partial class SiB287ReadDlMatchFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B287_READ_DL_MATCH_FILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB287ReadDlMatchFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB287ReadDlMatchFile.
  /// </summary>
  public SiB287ReadDlMatchFile(IContext context, Import import, Export export):
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
      "SWEXIE05", context, import, export, EabOptions.Hpvp);
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
    /// A value of KdorDlMatchRecord.
    /// </summary>
    [JsonPropertyName("kdorDlMatchRecord")]
    [Member(Index = 2, AccessFields = false, Members = new[]
    {
      "LastName",
      "FirstName",
      "Ssn",
      "DateOfBirth",
      "PersonNumber",
      "DriversLicenseNumber",
      "DlClass",
      "MotocycleClass",
      "CdlClass",
      "ExpirationDate",
      "Gender",
      "AddressLine1",
      "AddressLine2",
      "City",
      "State",
      "ZipCode",
      "HeightFt",
      "HeightIn",
      "Weight",
      "EyeColor"
    })]
    public KdorDlMatchRecord KdorDlMatchRecord
    {
      get => kdorDlMatchRecord ??= new();
      set => kdorDlMatchRecord = value;
    }

    private EabFileHandling eabFileHandling;
    private KdorDlMatchRecord kdorDlMatchRecord;
  }
#endregion
}
