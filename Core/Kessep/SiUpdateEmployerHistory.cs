// Program: SI_UPDATE_EMPLOYER_HISTORY, ID: 1902585889, model: 746.
// Short name: SWE03771
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_UPDATE_EMPLOYER_HISTORY.
/// </summary>
[Serializable]
public partial class SiUpdateEmployerHistory: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_UPDATE_EMPLOYER_HISTORY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiUpdateEmployerHistory(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiUpdateEmployerHistory.
  /// </summary>
  public SiUpdateEmployerHistory(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------------------------
    // Date        Developer	Request #	Description
    // --------    ----------	---------	
    // ---------------------------------------------
    // 02/21/2017  DDupree     CQ46988		Initial Code.
    // -------------------------------------------------------------------------------------
    MoveEmployerHistory(import.EmployerHistory, export.EmployerHistory);

    if (!ReadEmployerHistory())
    {
      ExitState = "EMPLOYER_HISTORY_NF";

      return;
    }

    try
    {
      UpdateEmployerHistory();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "EMPLOYER_HISTORY_NU";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "EMPLOYER_HISTORY_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    if (ReadEmployerHistory())
    {
      export.EmployerHistory.Assign(entities.EmployerHistory);
    }
    else
    {
      ExitState = "EMPLOYER_HISTORY_NF";
    }
  }

  private static void MoveEmployerHistory(EmployerHistory source,
    EmployerHistory target)
  {
    target.Note = source.Note;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
  }

  private bool ReadEmployerHistory()
  {
    entities.EmployerHistory.Populated = false;

    return Read("ReadEmployerHistory",
      (db, command) =>
      {
        db.SetInt32(command, "empId", import.Employer.Identifier);
        db.SetDateTime(
          command, "createdTmst",
          import.EmployerHistory.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.EmployerHistory.Note = db.GetNullableString(reader, 0);
        entities.EmployerHistory.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.EmployerHistory.LastUpdatedBy =
          db.GetNullableString(reader, 2);
        entities.EmployerHistory.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 3);
        entities.EmployerHistory.EmpId = db.GetInt32(reader, 4);
        entities.EmployerHistory.Populated = true;
      });
  }

  private void UpdateEmployerHistory()
  {
    System.Diagnostics.Debug.Assert(entities.EmployerHistory.Populated);

    var note = import.EmployerHistory.Note ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.EmployerHistory.Populated = false;
    Update("UpdateEmployerHistory",
      (db, command) =>
      {
        db.SetNullableString(command, "note", note);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetDateTime(
          command, "createdTmst",
          entities.EmployerHistory.CreatedTimestamp.GetValueOrDefault());
        db.SetInt32(command, "empId", entities.EmployerHistory.EmpId);
      });

    entities.EmployerHistory.Note = note;
    entities.EmployerHistory.LastUpdatedBy = lastUpdatedBy;
    entities.EmployerHistory.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.EmployerHistory.Populated = true;
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Entities entities = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    /// <summary>
    /// A value of EmployerHistory.
    /// </summary>
    [JsonPropertyName("employerHistory")]
    public EmployerHistory EmployerHistory
    {
      get => employerHistory ??= new();
      set => employerHistory = value;
    }

    private Employer employer;
    private EmployerHistory employerHistory;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of EmployerHistory.
    /// </summary>
    [JsonPropertyName("employerHistory")]
    public EmployerHistory EmployerHistory
    {
      get => employerHistory ??= new();
      set => employerHistory = value;
    }

    private EmployerHistory employerHistory;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    /// <summary>
    /// A value of EmployerHistory.
    /// </summary>
    [JsonPropertyName("employerHistory")]
    public EmployerHistory EmployerHistory
    {
      get => employerHistory ??= new();
      set => employerHistory = value;
    }

    private Employer employer;
    private EmployerHistory employerHistory;
  }
#endregion
}
