// Program: SP_DELETE_OFFICE, ID: 371781888, model: 746.
// Short name: SWE01329
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_DELETE_OFFICE.
/// </summary>
[Serializable]
public partial class SpDeleteOffice: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_DELETE_OFFICE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpDeleteOffice(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpDeleteOffice.
  /// </summary>
  public SpDeleteOffice(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (ReadOffice())
    {
      DeleteOffice();
    }
    else
    {
      ExitState = "FN0000_OFFICE_NF";
    }
  }

  private void DeleteOffice()
  {
    bool exists;

    exists = Read("DeleteOffice#1",
      (db, command) =>
      {
        db.SetInt32(command, "offId", entities.Office.SystemGeneratedId);
      },
      null);

    if (exists)
    {
      throw DataError("Restrict violation on table \"CKT_CON_FEE_INFO\".",
        "50001");
    }

    Update("DeleteOffice#2",
      (db, command) =>
      {
        db.SetInt32(command, "offId", entities.Office.SystemGeneratedId);
      });

    exists = Read("DeleteOffice#3",
      (db, command) =>
      {
        db.SetInt32(command, "offId", entities.Office.SystemGeneratedId);
      },
      null);

    if (exists)
    {
      throw DataError("Restrict violation on table \"CKT_OFFICE\".", "50001");
    }

    Update("DeleteOffice#4",
      (db, command) =>
      {
        db.SetInt32(command, "offId", entities.Office.SystemGeneratedId);
      });
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    private Office office;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    private Office office;
  }
#endregion
}
