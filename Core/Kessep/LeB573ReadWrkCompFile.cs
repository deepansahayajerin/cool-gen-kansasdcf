// Program: LE_B573_READ_WRK_COMP_FILE, ID: 1902561806, model: 746.
// Short name: SWEXER26
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_B573_READ_WRK_COMP_FILE.
/// </summary>
[Serializable]
public partial class LeB573ReadWrkCompFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B573_READ_WRK_COMP_FILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB573ReadWrkCompFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB573ReadWrkCompFile.
  /// </summary>
  public LeB573ReadWrkCompFile(IContext context, Import import, Export export):
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
      "SWEXER26", context, import, export, EabOptions.Hpvp);
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

    /// <summary>
    /// A value of FieldDelimiter.
    /// </summary>
    [JsonPropertyName("fieldDelimiter")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "Text1" })]
    public WorkArea FieldDelimiter
    {
      get => fieldDelimiter ??= new();
      set => fieldDelimiter = value;
    }

    private EabFileHandling eabFileHandling;
    private WorkArea fieldDelimiter;
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
    /// A value of WorkersCompClaimFileRecord.
    /// </summary>
    [JsonPropertyName("workersCompClaimFileRecord")]
    [Member(Index = 2, AccessFields = false, Members = new[]
    {
      "NcpNumber",
      "ClaimantFirstName",
      "ClaimantLastName",
      "ClaimantMiddleName",
      "ClaimantStreet",
      "ClaimantCity",
      "ClaimantState",
      "ClaimantZip",
      "ClaimantAttorneyFirstName",
      "ClaimantAttorneyLastName",
      "ClaimantAttorneyFirmName",
      "ClaimantAttorneyStreet",
      "ClaimantAttorneyCity",
      "ClaimantAttorneyState",
      "ClaimantAttorneyZip",
      "EmployerName",
      "DocketNumber",
      "InsurerName",
      "InsurerStreet",
      "InsurerCity",
      "InsurerState",
      "InsurerZip",
      "InsurerAttorneyFirmName",
      "InsurerAttorneyStreet",
      "InsurerAttorneyCity",
      "InsurerAttorneyState",
      "InsurerAttorneyZip",
      "InsurerContactName1",
      "InsurerContactName2",
      "InsurerContactPhone",
      "PolicyNumber",
      "DateOfLoss",
      "EmployerFein",
      "EmployerStreet",
      "EmployerCity",
      "EmployerState",
      "EmployerZip",
      "DateOfAccident",
      "WageAmount",
      "AccidentCity",
      "AccidentCounty",
      "AccidentState",
      "AccidentDescription",
      "SeverityCodeDescription",
      "ReturnedToWorkDate",
      "CompensationPaidFlag",
      "CompensationPaidDate",
      "WeeklyRate",
      "DateOfDeath",
      "ThirdPartyAdministratorName",
      "AdministrativeClaimNumber",
      "ClaimFiledDate",
      "AgencyClaimNo"
    })]
    public WorkersCompClaimFileRecord WorkersCompClaimFileRecord
    {
      get => workersCompClaimFileRecord ??= new();
      set => workersCompClaimFileRecord = value;
    }

    private EabFileHandling eabFileHandling;
    private WorkersCompClaimFileRecord workersCompClaimFileRecord;
  }
#endregion
}
