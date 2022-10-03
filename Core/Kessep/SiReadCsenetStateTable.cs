// Program: SI_READ_CSENET_STATE_TABLE, ID: 372390698, model: 746.
// Short name: SWE02339
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_READ_CSENET_STATE_TABLE.
/// </summary>
[Serializable]
public partial class SiReadCsenetStateTable: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_READ_CSENET_STATE_TABLE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiReadCsenetStateTable(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiReadCsenetStateTable.
  /// </summary>
  public SiReadCsenetStateTable(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------
    //           M A I N T E N A N C E   L O G
    // Date	  Developer		Description
    // 03/15/1999   Carl Ott		Initial creation
    // ------------------------------------------------------------
    if (ReadCsenetStateTable())
    {
      export.CsenetStateTable.Assign(entities.CsenetStateTable);
    }
    else
    {
      ExitState = "CO0000_CSENET_STATE_NF";
    }
  }

  private bool ReadCsenetStateTable()
  {
    entities.CsenetStateTable.Populated = false;

    return Read("ReadCsenetStateTable",
      (db, command) =>
      {
        db.SetString(command, "stateCode", import.CsenetStateTable.StateCode);
      },
      (db, reader) =>
      {
        entities.CsenetStateTable.StateCode = db.GetString(reader, 0);
        entities.CsenetStateTable.CsenetReadyInd = db.GetString(reader, 1);
        entities.CsenetStateTable.RecStateInd = db.GetString(reader, 2);
        entities.CsenetStateTable.QuickLocate = db.GetString(reader, 3);
        entities.CsenetStateTable.LastUpdatedBy = db.GetString(reader, 4);
        entities.CsenetStateTable.LastUpdatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.CsenetStateTable.CreatedBy = db.GetString(reader, 6);
        entities.CsenetStateTable.CreatedTimestamp = db.GetDateTime(reader, 7);
        entities.CsenetStateTable.Populated = true;
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
    /// A value of CsenetStateTable.
    /// </summary>
    [JsonPropertyName("csenetStateTable")]
    public CsenetStateTable CsenetStateTable
    {
      get => csenetStateTable ??= new();
      set => csenetStateTable = value;
    }

    private CsenetStateTable csenetStateTable;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CsenetStateTable.
    /// </summary>
    [JsonPropertyName("csenetStateTable")]
    public CsenetStateTable CsenetStateTable
    {
      get => csenetStateTable ??= new();
      set => csenetStateTable = value;
    }

    private CsenetStateTable csenetStateTable;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CsenetStateTable.
    /// </summary>
    [JsonPropertyName("csenetStateTable")]
    public CsenetStateTable CsenetStateTable
    {
      get => csenetStateTable ??= new();
      set => csenetStateTable = value;
    }

    private CsenetStateTable csenetStateTable;
  }
#endregion
}
