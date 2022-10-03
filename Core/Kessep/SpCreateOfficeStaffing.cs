// Program: SP_CREATE_OFFICE_STAFFING, ID: 945145300, model: 746.
// Short name: SWE03096
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CREATE_OFFICE_STAFFING.
/// </summary>
[Serializable]
public partial class SpCreateOfficeStaffing: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CREATE_OFFICE_STAFFING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCreateOfficeStaffing(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCreateOfficeStaffing.
  /// </summary>
  public SpCreateOfficeStaffing(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (!ReadOffice())
    {
      ExitState = "OFFICE_NF";

      return;
    }

    try
    {
      CreateOfficeStaffing();
      export.OfficeStaffing.Assign(entities.OfficeStaffing);
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "OFFICE_STAFFING_AE";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "OFFICE_STAFFING_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private void CreateOfficeStaffing()
  {
    var yearMonth = import.OfficeStaffing.YearMonth;
    var fullTimeEquivalent =
      import.OfficeStaffing.FullTimeEquivalent.GetValueOrDefault();
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var offGeneratedId = entities.Office.SystemGeneratedId;

    entities.OfficeStaffing.Populated = false;
    Update("CreateOfficeStaffing",
      (db, command) =>
      {
        db.SetInt32(command, "yearMonth", yearMonth);
        db.SetNullableDecimal(command, "FTE", fullTimeEquivalent);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", null);
        db.SetInt32(command, "offGeneratedId", offGeneratedId);
      });

    entities.OfficeStaffing.YearMonth = yearMonth;
    entities.OfficeStaffing.FullTimeEquivalent = fullTimeEquivalent;
    entities.OfficeStaffing.CreatedBy = createdBy;
    entities.OfficeStaffing.CreatedTimestamp = createdTimestamp;
    entities.OfficeStaffing.LastUpdatedBy = "";
    entities.OfficeStaffing.LastUpdatedTimestamp = null;
    entities.OfficeStaffing.OffGeneratedId = offGeneratedId;
    entities.OfficeStaffing.Populated = true;
  }

  private bool ReadOffice()
  {
    entities.Office.Populated = false;

    return Read("ReadOffice",
      (db, command) =>
      {
        db.SetInt32(command, "officeId", import.Office.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 1);
        entities.Office.Populated = true;
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
    /// A value of OfficeStaffing.
    /// </summary>
    [JsonPropertyName("officeStaffing")]
    public OfficeStaffing OfficeStaffing
    {
      get => officeStaffing ??= new();
      set => officeStaffing = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    private OfficeStaffing officeStaffing;
    private Office office;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of OfficeStaffing.
    /// </summary>
    [JsonPropertyName("officeStaffing")]
    public OfficeStaffing OfficeStaffing
    {
      get => officeStaffing ??= new();
      set => officeStaffing = value;
    }

    private OfficeStaffing officeStaffing;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of OfficeStaffing.
    /// </summary>
    [JsonPropertyName("officeStaffing")]
    public OfficeStaffing OfficeStaffing
    {
      get => officeStaffing ??= new();
      set => officeStaffing = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    private OfficeStaffing officeStaffing;
    private Office office;
  }
#endregion
}
