// Program: SP_DELETE_OFFICE_CASELOAD_ASSIGN, ID: 372559323, model: 746.
// Short name: SWE01332
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_DELETE_OFFICE_CASELOAD_ASSIGN.
/// </summary>
[Serializable]
public partial class SpDeleteOfficeCaseloadAssign: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_DELETE_OFFICE_CASELOAD_ASSIGN program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpDeleteOfficeCaseloadAssign(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpDeleteOfficeCaseloadAssign.
  /// </summary>
  public SpDeleteOfficeCaseloadAssign(IContext context, Import import,
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
    if (ReadOfficeCaseloadAssignment())
    {
      export.OfficeCaseloadAssignment.Assign(entities.OfficeCaseloadAssignment);
      DeleteOfficeCaseloadAssignment();
      ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";
    }
    else
    {
      ExitState = "OFFICE_CASELOAD_ASSIGNMENT_T_NF";
    }
  }

  private void DeleteOfficeCaseloadAssignment()
  {
    Update("DeleteOfficeCaseloadAssignment",
      (db, command) =>
      {
        db.SetInt32(
          command, "ofceCsldAssgnId",
          entities.OfficeCaseloadAssignment.SystemGeneratedIdentifier);
      });
  }

  private bool ReadOfficeCaseloadAssignment()
  {
    entities.OfficeCaseloadAssignment.Populated = false;

    return Read("ReadOfficeCaseloadAssignment",
      (db, command) =>
      {
        db.SetInt32(
          command, "ofceCsldAssgnId",
          import.OfficeCaseloadAssignment.SystemGeneratedIdentifier);
        db.SetNullableInt32(
          command, "offGeneratedId", import.Office.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.OfficeCaseloadAssignment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.OfficeCaseloadAssignment.EndingAlpha = db.GetString(reader, 1);
        entities.OfficeCaseloadAssignment.BeginingAlpha =
          db.GetString(reader, 2);
        entities.OfficeCaseloadAssignment.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeCaseloadAssignment.Priority = db.GetInt32(reader, 4);
        entities.OfficeCaseloadAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.OfficeCaseloadAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.OfficeCaseloadAssignment.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 7);
        entities.OfficeCaseloadAssignment.CreatedBy = db.GetString(reader, 8);
        entities.OfficeCaseloadAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 9);
        entities.OfficeCaseloadAssignment.OffGeneratedId =
          db.GetNullableInt32(reader, 10);
        entities.OfficeCaseloadAssignment.Populated = true;
      });
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of OfficeCaseloadAssignment.
    /// </summary>
    [JsonPropertyName("officeCaseloadAssignment")]
    public OfficeCaseloadAssignment OfficeCaseloadAssignment
    {
      get => officeCaseloadAssignment ??= new();
      set => officeCaseloadAssignment = value;
    }

    private Office office;
    private OfficeCaseloadAssignment officeCaseloadAssignment;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of OfficeCaseloadAssignment.
    /// </summary>
    [JsonPropertyName("officeCaseloadAssignment")]
    public OfficeCaseloadAssignment OfficeCaseloadAssignment
    {
      get => officeCaseloadAssignment ??= new();
      set => officeCaseloadAssignment = value;
    }

    private OfficeCaseloadAssignment officeCaseloadAssignment;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of OfficeCaseloadAssignment.
    /// </summary>
    [JsonPropertyName("officeCaseloadAssignment")]
    public OfficeCaseloadAssignment OfficeCaseloadAssignment
    {
      get => officeCaseloadAssignment ??= new();
      set => officeCaseloadAssignment = value;
    }

    private Office office;
    private OfficeCaseloadAssignment officeCaseloadAssignment;
  }
#endregion
}
