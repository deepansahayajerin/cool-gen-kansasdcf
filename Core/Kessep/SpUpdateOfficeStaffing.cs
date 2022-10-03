// Program: SP_UPDATE_OFFICE_STAFFING, ID: 945145301, model: 746.
// Short name: SWE03097
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_UPDATE_OFFICE_STAFFING.
/// </summary>
[Serializable]
public partial class SpUpdateOfficeStaffing: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_UPDATE_OFFICE_STAFFING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpUpdateOfficeStaffing(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpUpdateOfficeStaffing.
  /// </summary>
  public SpUpdateOfficeStaffing(IContext context, Import import, Export export):
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

    if (ReadOfficeStaffing())
    {
      try
      {
        UpdateOfficeStaffing();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "OFFICE_STAFFING_NU";

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
    else
    {
      ExitState = "OFFICE_STAFFING_NF";
    }
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

  private bool ReadOfficeStaffing()
  {
    entities.OfficeStaffing.Populated = false;

    return Read("ReadOfficeStaffing",
      (db, command) =>
      {
        db.
          SetInt32(command, "offGeneratedId", entities.Office.SystemGeneratedId);
          
        db.SetInt32(command, "yearMonth", import.OfficeStaffing.YearMonth);
      },
      (db, reader) =>
      {
        entities.OfficeStaffing.YearMonth = db.GetInt32(reader, 0);
        entities.OfficeStaffing.FullTimeEquivalent =
          db.GetNullableDecimal(reader, 1);
        entities.OfficeStaffing.CreatedBy = db.GetString(reader, 2);
        entities.OfficeStaffing.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.OfficeStaffing.LastUpdatedBy = db.GetNullableString(reader, 4);
        entities.OfficeStaffing.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 5);
        entities.OfficeStaffing.OffGeneratedId = db.GetInt32(reader, 6);
        entities.OfficeStaffing.Populated = true;
      });
  }

  private void UpdateOfficeStaffing()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeStaffing.Populated);

    var fullTimeEquivalent =
      import.OfficeStaffing.FullTimeEquivalent.GetValueOrDefault();
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.OfficeStaffing.Populated = false;
    Update("UpdateOfficeStaffing",
      (db, command) =>
      {
        db.SetNullableDecimal(command, "FTE", fullTimeEquivalent);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetInt32(command, "yearMonth", entities.OfficeStaffing.YearMonth);
        db.SetInt32(
          command, "offGeneratedId", entities.OfficeStaffing.OffGeneratedId);
      });

    entities.OfficeStaffing.FullTimeEquivalent = fullTimeEquivalent;
    entities.OfficeStaffing.LastUpdatedBy = lastUpdatedBy;
    entities.OfficeStaffing.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.OfficeStaffing.Populated = true;
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
