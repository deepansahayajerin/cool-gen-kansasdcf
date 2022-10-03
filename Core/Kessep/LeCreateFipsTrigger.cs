// Program: LE_CREATE_FIPS_TRIGGER, ID: 374349983, model: 746.
// Short name: SWE02519
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_CREATE_FIPS_TRIGGER.
/// </summary>
[Serializable]
public partial class LeCreateFipsTrigger: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_CREATE_FIPS_TRIGGER program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeCreateFipsTrigger(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeCreateFipsTrigger.
  /// </summary>
  public LeCreateFipsTrigger(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (Equal(import.Command.Value, "DELETE"))
    {
      if (!Equal(import.Fips.StateAbbreviation, "KS"))
      {
        return;
      }
    }
    else if (!ReadFips())
    {
      return;
    }

    local.Trigger.Type1 = "FIPS";
    local.Trigger.Action = import.Command.Value;
    local.Trigger.DenormNumeric1 = import.Fips.State;
    local.Trigger.DenormNumeric2 = import.Fips.County;
    local.Trigger.DenormNumeric3 = import.Fips.Location;

    // mjr
    // ----------------------------------------------
    // 03/14/2000
    // SWELB590 - OUTGOING FIPS UPDATE processes these
    // records.  If a record related to this FIPS hasn't been
    // processed already, that record can be used for this action
    // -----------------------------------------------------------
    if (ReadProgramCheckpointRestart())
    {
      local.DateWorkArea.Timestamp =
        entities.ProgramCheckpointRestart.LastCheckpointTimestamp;
    }

    if (ReadTrigger())
    {
      switch(TrimEnd(entities.Trigger.Action))
      {
        case "ADD":
          switch(TrimEnd(local.Trigger.Action ?? ""))
          {
            case "UPDATE":
              break;
            case "DELETE":
              DeleteTrigger();

              break;
            default:
              ExitState = "SP0000_TRIGGER_PV";

              break;
          }

          break;
        case "UPDATE":
          switch(TrimEnd(local.Trigger.Action ?? ""))
          {
            case "UPDATE":
              break;
            case "DELETE":
              MoveTrigger(entities.Trigger, local.Trigger);
              local.Trigger.Action = "DELETE";
              UseSpCabUpdateTrigger();

              break;
            default:
              ExitState = "SP0000_TRIGGER_PV";

              break;
          }

          break;
        case "DELETE":
          if (Equal(local.Trigger.Action, "ADD"))
          {
            MoveTrigger(entities.Trigger, local.Trigger);
            local.Trigger.Action = "UPDATE";
            UseSpCabUpdateTrigger();
          }
          else
          {
            ExitState = "SP0000_TRIGGER_PV";
          }

          break;
        default:
          ExitState = "SP0000_TRIGGER_PV";

          break;
      }
    }
    else
    {
      UseSpCabCreateTrigger();
    }
  }

  private static void MoveTrigger(Trigger source, Trigger target)
  {
    target.Identifier = source.Identifier;
    target.Type1 = source.Type1;
    target.Action = source.Action;
    target.Status = source.Status;
    target.DenormNumeric1 = source.DenormNumeric1;
    target.DenormNumeric2 = source.DenormNumeric2;
    target.DenormNumeric3 = source.DenormNumeric3;
    target.DenormText1 = source.DenormText1;
    target.DenormText2 = source.DenormText2;
    target.DenormText3 = source.DenormText3;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private void UseSpCabCreateTrigger()
  {
    var useImport = new SpCabCreateTrigger.Import();
    var useExport = new SpCabCreateTrigger.Export();

    useImport.Trigger.Assign(local.Trigger);

    Call(SpCabCreateTrigger.Execute, useImport, useExport);

    local.Trigger.Assign(useExport.Trigger);
  }

  private void UseSpCabUpdateTrigger()
  {
    var useImport = new SpCabUpdateTrigger.Import();
    var useExport = new SpCabUpdateTrigger.Export();

    useImport.Trigger.Assign(local.Trigger);

    Call(SpCabUpdateTrigger.Execute, useImport, useExport);

    local.Trigger.Assign(useExport.Trigger);
  }

  private void DeleteTrigger()
  {
    Update("DeleteTrigger",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", entities.Trigger.Identifier);
      });
  }

  private bool ReadFips()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.SetInt32(command, "state", import.Fips.State);
        db.SetInt32(command, "county", import.Fips.County);
        db.SetInt32(command, "location", import.Fips.Location);
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.StateAbbreviation = db.GetString(reader, 3);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadProgramCheckpointRestart()
  {
    entities.ProgramCheckpointRestart.Populated = false;

    return Read("ReadProgramCheckpointRestart",
      null,
      (db, reader) =>
      {
        entities.ProgramCheckpointRestart.ProgramName = db.GetString(reader, 0);
        entities.ProgramCheckpointRestart.LastCheckpointTimestamp =
          db.GetNullableDateTime(reader, 1);
        entities.ProgramCheckpointRestart.Populated = true;
      });
  }

  private bool ReadTrigger()
  {
    entities.Trigger.Populated = false;

    return Read("ReadTrigger",
      (db, command) =>
      {
        db.SetString(command, "type", local.Trigger.Type1);
        db.SetNullableInt32(
          command, "denormNumeric1",
          local.Trigger.DenormNumeric1.GetValueOrDefault());
        db.SetNullableInt32(
          command, "denormNumeric2",
          local.Trigger.DenormNumeric2.GetValueOrDefault());
        db.SetNullableInt32(
          command, "denormNumeric3",
          local.Trigger.DenormNumeric3.GetValueOrDefault());
        db.SetNullableDateTime(
          command, "createdTimestamp",
          local.DateWorkArea.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Trigger.Identifier = db.GetInt32(reader, 0);
        entities.Trigger.Type1 = db.GetString(reader, 1);
        entities.Trigger.Action = db.GetNullableString(reader, 2);
        entities.Trigger.Status = db.GetNullableString(reader, 3);
        entities.Trigger.DenormNumeric1 = db.GetNullableInt32(reader, 4);
        entities.Trigger.DenormNumeric2 = db.GetNullableInt32(reader, 5);
        entities.Trigger.DenormNumeric3 = db.GetNullableInt32(reader, 6);
        entities.Trigger.DenormText1 = db.GetNullableString(reader, 7);
        entities.Trigger.DenormText2 = db.GetNullableString(reader, 8);
        entities.Trigger.DenormText3 = db.GetNullableString(reader, 9);
        entities.Trigger.CreatedTimestamp = db.GetNullableDateTime(reader, 10);
        entities.Trigger.Populated = true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
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
    public Command Command
    {
      get => command ??= new();
      set => command = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    private Command command;
    private Fips fips;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of Trigger.
    /// </summary>
    [JsonPropertyName("trigger")]
    public Trigger Trigger
    {
      get => trigger ??= new();
      set => trigger = value;
    }

    private DateWorkArea dateWorkArea;
    private Trigger trigger;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Trigger.
    /// </summary>
    [JsonPropertyName("trigger")]
    public Trigger Trigger
    {
      get => trigger ??= new();
      set => trigger = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    private Trigger trigger;
    private Fips fips;
    private ProgramCheckpointRestart programCheckpointRestart;
  }
#endregion
}
