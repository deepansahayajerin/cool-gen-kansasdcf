// Program: FN_MARK_AR_AS_PROCESSED, ID: 372275528, model: 746.
// Short name: SWE01646
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_MARK_AR_AS_PROCESSED.
/// </summary>
[Serializable]
public partial class FnMarkArAsProcessed: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_MARK_AR_AS_PROCESSED program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnMarkArAsProcessed(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnMarkArAsProcessed.
  /// </summary>
  public FnMarkArAsProcessed(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    try
    {
      UpdateCaseRole();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "CASE_ROLE_NU";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "CASE_ROLE_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private void UpdateCaseRole()
  {
    System.Diagnostics.Debug.Assert(import.Persistent.Populated);

    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = import.ProgramProcessingInfo.Name;
    var arChgProcessedDate = import.ProgramProcessingInfo.ProcessDate;

    import.Persistent.Populated = false;
    Update("UpdateCaseRole",
      (db, command) =>
      {
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDate(command, "arChgProcDt", arChgProcessedDate);
        db.SetString(command, "casNumber", import.Persistent.CasNumber);
        db.SetString(command, "cspNumber", import.Persistent.CspNumber);
        db.SetString(command, "type", import.Persistent.Type1);
        db.SetInt32(command, "caseRoleId", import.Persistent.Identifier);
      });

    import.Persistent.LastUpdatedTimestamp = lastUpdatedTimestamp;
    import.Persistent.LastUpdatedBy = lastUpdatedBy;
    import.Persistent.ArChgProcessedDate = arChgProcessedDate;
    import.Persistent.Populated = true;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of Persistent.
    /// </summary>
    [JsonPropertyName("persistent")]
    public CaseRole Persistent
    {
      get => persistent ??= new();
      set => persistent = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
    private CaseRole persistent;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }
#endregion
}
