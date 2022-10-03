// Program: SP_UPDT_DEL_CNTY_SERV_ASSIGNMT, ID: 371782313, model: 746.
// Short name: SWE01450
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_UPDT_DEL_CNTY_SERV_ASSIGNMT.
/// </para>
/// <para>
/// RESP: SRVPLAN
/// Update or delete a County Service Assignment.
/// </para>
/// </summary>
[Serializable]
public partial class SpUpdtDelCntyServAssignmt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_UPDT_DEL_CNTY_SERV_ASSIGNMT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpUpdtDelCntyServAssignmt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpUpdtDelCntyServAssignmt.
  /// </summary>
  public SpUpdtDelCntyServAssignmt(IContext context, Import import,
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
    // *******************************************
    // **
    // **   M A I N T E N A N C E   L O G
    // **
    // **   Date 	Description
    // **   6/95	R Grey		Update
    // **   4/30	R Grey		Add delete capability
    // **
    // ********************************************
    MoveCountyService(import.CountyService, export.CountyService);
    export.Command.Command = import.Command.Command;

    if (ReadCountyService())
    {
      if (Equal(export.Command.Command, "DELETE"))
      {
        DeleteCountyService();
        ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";
      }
      else
      {
        try
        {
          UpdateCountyService();
          export.CountyService.Assign(entities.CountyService);
          ExitState = "ACO_NI0000_UPDATE_SUCCESSFUL";
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "COUNTY_SERVICES_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "COUNTY_SERVICES_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
    }
    else
    {
      ExitState = "COUNTY_SERVICES_NF";
    }
  }

  private static void MoveCountyService(CountyService source,
    CountyService target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Type1 = source.Type1;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
  }

  private void DeleteCountyService()
  {
    Update("DeleteCountyService",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratdId",
          entities.CountyService.SystemGeneratedIdentifier);
      });
  }

  private bool ReadCountyService()
  {
    entities.CountyService.Populated = false;

    return Read("ReadCountyService",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratdId",
          import.CountyService.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CountyService.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CountyService.Type1 = db.GetString(reader, 1);
        entities.CountyService.EffectiveDate = db.GetDate(reader, 2);
        entities.CountyService.DiscontinueDate = db.GetDate(reader, 3);
        entities.CountyService.LastUpdatedBy = db.GetNullableString(reader, 4);
        entities.CountyService.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 5);
        entities.CountyService.Populated = true;
      });
  }

  private void UpdateCountyService()
  {
    var effectiveDate = import.CountyService.EffectiveDate;
    var discontinueDate = import.CountyService.DiscontinueDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatdTstamp = Now();

    entities.CountyService.Populated = false;
    Update("UpdateCountyService",
      (db, command) =>
      {
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatdTstamp", lastUpdatdTstamp);
        db.SetInt32(
          command, "systemGeneratdId",
          entities.CountyService.SystemGeneratedIdentifier);
      });

    entities.CountyService.EffectiveDate = effectiveDate;
    entities.CountyService.DiscontinueDate = discontinueDate;
    entities.CountyService.LastUpdatedBy = lastUpdatedBy;
    entities.CountyService.LastUpdatdTstamp = lastUpdatdTstamp;
    entities.CountyService.Populated = true;
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
    /// A value of Command.
    /// </summary>
    [JsonPropertyName("command")]
    public Common Command
    {
      get => command ??= new();
      set => command = value;
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

    /// <summary>
    /// A value of CseOrganization.
    /// </summary>
    [JsonPropertyName("cseOrganization")]
    public CseOrganization CseOrganization
    {
      get => cseOrganization ??= new();
      set => cseOrganization = value;
    }

    /// <summary>
    /// A value of CountyService.
    /// </summary>
    [JsonPropertyName("countyService")]
    public CountyService CountyService
    {
      get => countyService ??= new();
      set => countyService = value;
    }

    private Common command;
    private Office office;
    private CseOrganization cseOrganization;
    private CountyService countyService;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Command.
    /// </summary>
    [JsonPropertyName("command")]
    public Common Command
    {
      get => command ??= new();
      set => command = value;
    }

    /// <summary>
    /// A value of CountyService.
    /// </summary>
    [JsonPropertyName("countyService")]
    public CountyService CountyService
    {
      get => countyService ??= new();
      set => countyService = value;
    }

    private Common command;
    private CountyService countyService;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CountyService.
    /// </summary>
    [JsonPropertyName("countyService")]
    public CountyService CountyService
    {
      get => countyService ??= new();
      set => countyService = value;
    }

    private CountyService countyService;
  }
#endregion
}
