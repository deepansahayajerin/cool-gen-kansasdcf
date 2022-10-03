// Program: SI_B286_READ_DL_ERROR_FILE, ID: 1625319359, model: 746.
// Short name: SWEXIE04
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B286_READ_DL_ERROR_FILE.
/// </summary>
[Serializable]
public partial class SiB286ReadDlErrorFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B286_READ_DL_ERROR_FILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB286ReadDlErrorFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB286ReadDlErrorFile.
  /// </summary>
  public SiB286ReadDlErrorFile(IContext context, Import import, Export export):
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
      "SWEXIE04", context, import, export, EabOptions.Hpvp);
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
    /// A value of KdorDlErrorRecord.
    /// </summary>
    [JsonPropertyName("kdorDlErrorRecord")]
    [Member(Index = 2, AccessFields = false, Members = new[]
    {
      "LastName",
      "FirstName",
      "Ssn",
      "DateOfBirth",
      "PersonNumber",
      "DriversLicenseNumber",
      "Status",
      "ErrorReason"
    })]
    public KdorDlErrorRecord KdorDlErrorRecord
    {
      get => kdorDlErrorRecord ??= new();
      set => kdorDlErrorRecord = value;
    }

    private EabFileHandling eabFileHandling;
    private KdorDlErrorRecord kdorDlErrorRecord;
  }
#endregion
}
