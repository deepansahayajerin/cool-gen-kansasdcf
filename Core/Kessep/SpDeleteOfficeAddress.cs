// Program: SP_DELETE_OFFICE_ADDRESS, ID: 371781919, model: 746.
// Short name: SWE01330
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_DELETE_OFFICE_ADDRESS.
/// </summary>
[Serializable]
public partial class SpDeleteOfficeAddress: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_DELETE_OFFICE_ADDRESS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpDeleteOfficeAddress(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpDeleteOfficeAddress.
  /// </summary>
  public SpDeleteOfficeAddress(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (!ReadOfficeAddress())
    {
      ExitState = "OFFICE_ADDRESS_NF";

      return;
    }

    DeleteOfficeAddress();
  }

  private void DeleteOfficeAddress()
  {
    Update("DeleteOfficeAddress",
      (db, command) =>
      {
        db.SetInt32(
          command, "offGeneratedId", entities.OfficeAddress.OffGeneratedId);
        db.SetString(command, "type", entities.OfficeAddress.Type1);
      });
  }

  private bool ReadOfficeAddress()
  {
    entities.OfficeAddress.Populated = false;

    return Read("ReadOfficeAddress",
      (db, command) =>
      {
        db.SetString(command, "type", import.OfficeAddress.Type1);
        db.SetInt32(command, "offGeneratedId", import.Office.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.OfficeAddress.OffGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeAddress.Type1 = db.GetString(reader, 1);
        entities.OfficeAddress.Populated = true;
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
    /// A value of OfficeAddress.
    /// </summary>
    [JsonPropertyName("officeAddress")]
    public OfficeAddress OfficeAddress
    {
      get => officeAddress ??= new();
      set => officeAddress = value;
    }

    private Office office;
    private OfficeAddress officeAddress;
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

    /// <summary>
    /// A value of OfficeAddress.
    /// </summary>
    [JsonPropertyName("officeAddress")]
    public OfficeAddress OfficeAddress
    {
      get => officeAddress ??= new();
      set => officeAddress = value;
    }

    private Office office;
    private OfficeAddress officeAddress;
  }
#endregion
}
