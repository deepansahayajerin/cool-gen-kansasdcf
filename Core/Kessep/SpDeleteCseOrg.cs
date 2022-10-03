// Program: SP_DELETE_CSE_ORG, ID: 371780614, model: 746.
// Short name: SWE01325
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_DELETE_CSE_ORG.
/// </summary>
[Serializable]
public partial class SpDeleteCseOrg: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_DELETE_CSE_ORG program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpDeleteCseOrg(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpDeleteCseOrg.
  /// </summary>
  public SpDeleteCseOrg(IContext context, Import import, Export export):
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
    // **   Date 	Description
    // **   5/95	Rod Grey	Update
    // **
    // *******************************************
    if (ReadCseOrganization())
    {
      DeleteCseOrganization();
      ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";
    }
    else
    {
      ExitState = "STATE_ORGANIZATION_NF";
    }
  }

  private void DeleteCseOrganization()
  {
    bool exists;

    exists = Read("DeleteCseOrganization#1",
      (db, command) =>
      {
        db.SetNullableString(command, "cogCode", entities.CseOrganization.Code);
        db.SetNullableString(
          command, "cogTypeCode", entities.CseOrganization.Type1);
      },
      null);

    if (exists)
    {
      throw DataError("Restrict violation on table \"CKT_OFFICE\".", "50001");
    }

    Update("DeleteCseOrganization#2",
      (db, command) =>
      {
        db.SetNullableString(command, "cogCode", entities.CseOrganization.Code);
        db.SetNullableString(
          command, "cogTypeCode", entities.CseOrganization.Type1);
      });
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
        entities.CseOrganization.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.CseOrganization.CreatedBy = db.GetString(reader, 5);
        entities.CseOrganization.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 6);
        entities.CseOrganization.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.CseOrganization.Populated = true;
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
