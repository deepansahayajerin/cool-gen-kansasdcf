// Program: LE_EAB_WRITE_FDSO_SNAPSHOT_FILE, ID: 370999093, model: 746.
// Short name: SWEXLE89
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_EAB_WRITE_FDSO_SNAPSHOT_FILE.
/// </para>
/// <para>
/// This is a external to write FDSO snapshot file.
/// </para>
/// </summary>
[Serializable]
public partial class LeEabWriteFdsoSnapshotFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_EAB_WRITE_FDSO_SNAPSHOT_FILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeEabWriteFdsoSnapshotFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeEabWriteFdsoSnapshotFile.
  /// </summary>
  public LeEabWriteFdsoSnapshotFile(IContext context, Import import,
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
      "SWEXLE89", context, import, export, EabOptions.Hpvp);
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
    /// A value of FdsoSnapshotFileRecord.
    /// </summary>
    [JsonPropertyName("fdsoSnapshotFileRecord")]
    [Member(Index = 1, AccessFields = false, Members = new[]
    {
      "SubmittingState",
      "LocalCode",
      "Ssn",
      "CaseNumber",
      "LastName",
      "FirstName",
      "CurrentArrearageAmount",
      "CaseType",
      "EtypeAdministrativeOffset",
      "EtypeFederalRetirement",
      "EtypeVendorPaymentOrMisc",
      "EtypeFederalSalary",
      "EtypeTaxRefund",
      "EtypePassportDenial",
      "EtypeFinancialInstitution",
      "TransactionType"
    })]
    public FdsoSnapshotFileRecord FdsoSnapshotFileRecord
    {
      get => fdsoSnapshotFileRecord ??= new();
      set => fdsoSnapshotFileRecord = value;
    }

    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    [Member(Index = 2, AccessFields = false, Members
      = new[] { "FileInstruction" })]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    private FdsoSnapshotFileRecord fdsoSnapshotFileRecord;
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
    [Member(Index = 1, AccessFields = false, Members = new[]
    {
      "FileInstruction",
      "NumericReturnCode",
      "TextReturnCode"
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
