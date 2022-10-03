// Program: ACCESS_CONTROL_TABLE, ID: 371423231, model: 746.
// Short name: SWE00003
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: ACCESS_CONTROL_TABLE.
/// </para>
/// <para>
/// RESP: FINANCE
/// The action block will read the control table and get the next sequential 
/// number for a table.
/// </para>
/// </summary>
[Serializable]
public partial class AccessControlTable: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the ACCESS_CONTROL_TABLE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new AccessControlTable(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of AccessControlTable.
  /// </summary>
  public AccessControlTable(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (ReadControlTable())
    {
      try
      {
        UpdateControlTable();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "CONTROL_TABLE_CRNO_NU";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "CONTROL_TABLE_CRNO_PV";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    else
    {
      ExitState = "CONTROL_TABLE_CRNO_NF";

      return;
    }

    export.ControlTable.LastUsedNumber = entities.ControlTable.LastUsedNumber;
  }

  private bool ReadControlTable()
  {
    entities.ControlTable.Populated = false;

    return Read("ReadControlTable",
      (db, command) =>
      {
        db.SetString(command, "cntlTblId", import.ControlTable.Identifier);
      },
      (db, reader) =>
      {
        entities.ControlTable.Identifier = db.GetString(reader, 0);
        entities.ControlTable.LastUsedNumber = db.GetInt32(reader, 1);
        entities.ControlTable.Populated = true;
      });
  }

  private void UpdateControlTable()
  {
    var lastUsedNumber = entities.ControlTable.LastUsedNumber + 1;

    entities.ControlTable.Populated = false;
    Update("UpdateControlTable",
      (db, command) =>
      {
        db.SetInt32(command, "lastUsedNumber", lastUsedNumber);
        db.SetString(command, "cntlTblId", entities.ControlTable.Identifier);
      });

    entities.ControlTable.LastUsedNumber = lastUsedNumber;
    entities.ControlTable.Populated = true;
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
    /// A value of ControlTable.
    /// </summary>
    [JsonPropertyName("controlTable")]
    public ControlTable ControlTable
    {
      get => controlTable ??= new();
      set => controlTable = value;
    }

    private ControlTable controlTable;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ControlTable.
    /// </summary>
    [JsonPropertyName("controlTable")]
    public ControlTable ControlTable
    {
      get => controlTable ??= new();
      set => controlTable = value;
    }

    private ControlTable controlTable;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ControlTable.
    /// </summary>
    [JsonPropertyName("controlTable")]
    public ControlTable ControlTable
    {
      get => controlTable ??= new();
      set => controlTable = value;
    }

    private ControlTable controlTable;
  }
#endregion
}
