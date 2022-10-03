// Program: SP_UPDATE_CSE_ORGANIZATION, ID: 371780616, model: 746.
// Short name: SWE01438
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_UPDATE_CSE_ORGANIZATION.
/// </summary>
[Serializable]
public partial class SpUpdateCseOrganization: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_UPDATE_CSE_ORGANIZATION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpUpdateCseOrganization(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpUpdateCseOrganization.
  /// </summary>
  public SpUpdateCseOrganization(IContext context, Import import, Export export):
    
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
    // **   M A I N T E N A N C E    L O G
    // **
    // **   Date	Description
    // **   5/95	R Grey		Update
    // **
    // *******************************************
    // ********************************************************************
    // **
    // **   6/12/14	LSS  CQ43611    Changed READ to include TYPE for an
    // **
    // 
    // UPDATE to cse_organization
    // because
    // **
    // 
    // CODE is not unique to a TYPE
    // which
    // **
    // 
    // results in updating the first
    // record
    // **
    // 
    // found which isn't always the
    // correct
    // **
    // 
    // record.
    // **
    // ********************************************************************
    MoveCseOrganization(import.CseOrganization, export.CseOrganization);

    if (ReadCseOrganization())
    {
      try
      {
        UpdateCseOrganization();
        export.CseOrganization.Assign(entities.CseOrganization);
        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "STATE_ORGANIZATION_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "STATE_ORGANIZATION_PV";

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
      ExitState = "STATE_ORGANIZATION_NF";
    }
  }

  private static void MoveCseOrganization(CseOrganization source,
    CseOrganization target)
  {
    target.TaxSuffix = source.TaxSuffix;
    target.Code = source.Code;
    target.Type1 = source.Type1;
    target.Name = source.Name;
    target.TaxId = source.TaxId;
  }

  private bool ReadCseOrganization()
  {
    entities.CseOrganization.Populated = false;

    return Read("ReadCseOrganization",
      (db, command) =>
      {
        db.SetString(command, "typeCode", import.CseOrganization.Type1);
        db.SetString(command, "organztnId", import.CseOrganization.Code);
      },
      (db, reader) =>
      {
        entities.CseOrganization.Code = db.GetString(reader, 0);
        entities.CseOrganization.Type1 = db.GetString(reader, 1);
        entities.CseOrganization.Name = db.GetString(reader, 2);
        entities.CseOrganization.TaxId = db.GetString(reader, 3);
        entities.CseOrganization.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 4);
        entities.CseOrganization.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.CseOrganization.TaxSuffix = db.GetNullableInt32(reader, 6);
        entities.CseOrganization.Populated = true;
      });
  }

  private void UpdateCseOrganization()
  {
    var name = import.CseOrganization.Name;
    var taxId = import.CseOrganization.TaxId;
    var lastUpdatdTstamp = Now();
    var lastUpdatedBy = global.UserId;
    var taxSuffix = import.CseOrganization.TaxSuffix.GetValueOrDefault();

    entities.CseOrganization.Populated = false;
    Update("UpdateCseOrganization",
      (db, command) =>
      {
        db.SetString(command, "name", name);
        db.SetString(command, "taxId", taxId);
        db.SetNullableDateTime(command, "lastUpdatdTstamp", lastUpdatdTstamp);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableInt32(command, "taxSuffix", taxSuffix);
        db.SetString(command, "organztnId", entities.CseOrganization.Code);
        db.SetString(command, "typeCode", entities.CseOrganization.Type1);
      });

    entities.CseOrganization.Name = name;
    entities.CseOrganization.TaxId = taxId;
    entities.CseOrganization.LastUpdatdTstamp = lastUpdatdTstamp;
    entities.CseOrganization.LastUpdatedBy = lastUpdatedBy;
    entities.CseOrganization.TaxSuffix = taxSuffix;
    entities.CseOrganization.Populated = true;
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
    /// A value of CseOrganization.
    /// </summary>
    [JsonPropertyName("cseOrganization")]
    public CseOrganization CseOrganization
    {
      get => cseOrganization ??= new();
      set => cseOrganization = value;
    }

    private CseOrganization cseOrganization;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CseOrganization.
    /// </summary>
    [JsonPropertyName("cseOrganization")]
    public CseOrganization CseOrganization
    {
      get => cseOrganization ??= new();
      set => cseOrganization = value;
    }

    private CseOrganization cseOrganization;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CseOrganization.
    /// </summary>
    [JsonPropertyName("cseOrganization")]
    public CseOrganization CseOrganization
    {
      get => cseOrganization ??= new();
      set => cseOrganization = value;
    }

    private CseOrganization cseOrganization;
  }
#endregion
}
