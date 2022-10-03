// Program: SP_CREATE_CSE_ORGANIZATION, ID: 371780615, model: 746.
// Short name: SWE01309
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CREATE_CSE_ORGANIZATION.
/// </summary>
[Serializable]
public partial class SpCreateCseOrganization: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CREATE_CSE_ORGANIZATION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCreateCseOrganization(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCreateCseOrganization.
  /// </summary>
  public SpCreateCseOrganization(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ********************************************
    // **
    // **   M A I N T E N A N C E   L O G
    // **
    // **   Date	Description
    // **   5/95	Rod Grey	Update
    // **
    // ********************************************
    export.CseOrganization.Assign(import.CseOrganization);

    try
    {
      CreateCseOrganization();
      export.CseOrganization.Assign(entities.CseOrganization);
      ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "STATE_ORGANIZATION_AE";

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

  private void CreateCseOrganization()
  {
    var code = import.CseOrganization.Code;
    var type1 = import.CseOrganization.Type1;
    var name = import.CseOrganization.Name;
    var taxId = import.CseOrganization.TaxId;
    var createdTimestamp = Now();
    var createdBy = global.UserId;
    var taxSuffix = import.CseOrganization.TaxSuffix.GetValueOrDefault();

    entities.CseOrganization.Populated = false;
    Update("CreateCseOrganization",
      (db, command) =>
      {
        db.SetString(command, "organztnId", code);
        db.SetString(command, "typeCode", type1);
        db.SetString(command, "name", name);
        db.SetString(command, "taxId", taxId);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "createdBy", createdBy);
        db.SetNullableDateTime(command, "lastUpdatdTstamp", default(DateTime));
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableInt32(command, "taxSuffix", taxSuffix);
      });

    entities.CseOrganization.Code = code;
    entities.CseOrganization.Type1 = type1;
    entities.CseOrganization.Name = name;
    entities.CseOrganization.TaxId = taxId;
    entities.CseOrganization.CreatedTimestamp = createdTimestamp;
    entities.CseOrganization.CreatedBy = createdBy;
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
