// Program: LE_UPDATE_COURT_CAPTION_LINES, ID: 372010970, model: 746.
// Short name: SWE00827
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_UPDATE_COURT_CAPTION_LINES.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action will Update Court Caption Lines for the specified Court Case 
/// Number.
/// </para>
/// </summary>
[Serializable]
public partial class LeUpdateCourtCaptionLines: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_UPDATE_COURT_CAPTION_LINES program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeUpdateCourtCaptionLines(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeUpdateCourtCaptionLines.
  /// </summary>
  public LeUpdateCourtCaptionLines(IContext context, Import import,
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
    // ------------------------------------------------------------
    // Date		Developer	Request #	Description
    // 05/10/95	Dave Allen			Initial Code
    // 10/07/98        Dick Jean                       Remove case of delete, 
    // qualify delte of court caption lnes only for command of update; change
    // exit state invalid_command_2
    // ------------------------------------------------------------
    export.Export1.Index = 0;
    export.Export1.Clear();

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (export.Export1.IsFull)
      {
        break;
      }

      MoveCourtCaption(import.Import1.Item.Cc, export.Export1.Update.Cc);
      export.Export1.Next();
    }

    if (!ReadLegalAction())
    {
      ExitState = "LEGAL_ACTION_NF";

      return;
    }

    switch(TrimEnd(global.Command))
    {
      case "ADD":
        if (ReadCourtCaption1())
        {
          ExitState = "LE0000_CAPT_AE_USE_UPDATE_KEY";

          return;
        }

        break;
      case "UPDATE":
        if (!ReadCourtCaption1())
        {
          ExitState = "LE0000_CAPT_NF_NOTHING_TO_DELETE";

          return;
        }

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
    }

    // ------------------------------------------------------------
    // Delete all current caption lines.
    // ------------------------------------------------------------
    if (Equal(global.Command, "UPDATE"))
    {
      foreach(var item in ReadCourtCaption2())
      {
        DeleteCourtCaption();
      }
    }

    // ------------------------------------------------------------
    // Re-create new Caption lines.
    // ------------------------------------------------------------
    local.LineCount.Count = 0;

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (IsEmpty(import.Import1.Item.Cc.Line))
      {
        continue;
      }

      ++local.LineCount.Count;

      try
      {
        CreateCourtCaption();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "CO0000_COURT_CAPTION_AE";

            break;
          case ErrorCode.PermittedValueViolation:
            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    // ---------------------------------------------
    // Now read and move the lines again since one or more lines might have been
    // dropped.
    // ---------------------------------------------
    export.Export1.Index = 0;
    export.Export1.Clear();

    foreach(var item in ReadCourtCaption3())
    {
      MoveCourtCaption(entities.CourtCaption, export.Export1.Update.Cc);
      export.Export1.Next();
    }
  }

  private static void MoveCourtCaption(CourtCaption source, CourtCaption target)
  {
    target.Number = source.Number;
    target.Line = source.Line;
  }

  private void CreateCourtCaption()
  {
    var lgaIdentifier = entities.LegalAction.Identifier;
    var number = local.LineCount.Count;
    var line = import.Import1.Item.Cc.Line ?? "";

    entities.CourtCaption.Populated = false;
    Update("CreateCourtCaption",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", lgaIdentifier);
        db.SetInt32(command, "courtCaptionNo", number);
        db.SetNullableString(command, "line", line);
      });

    entities.CourtCaption.LgaIdentifier = lgaIdentifier;
    entities.CourtCaption.Number = number;
    entities.CourtCaption.Line = line;
    entities.CourtCaption.Populated = true;
  }

  private void DeleteCourtCaption()
  {
    Update("DeleteCourtCaption",
      (db, command) =>
      {
        db.SetInt32(
          command, "lgaIdentifier", entities.CourtCaption.LgaIdentifier);
        db.SetInt32(command, "courtCaptionNo", entities.CourtCaption.Number);
      });
  }

  private bool ReadCourtCaption1()
  {
    entities.CourtCaption.Populated = false;

    return Read("ReadCourtCaption1",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.CourtCaption.LgaIdentifier = db.GetInt32(reader, 0);
        entities.CourtCaption.Number = db.GetInt32(reader, 1);
        entities.CourtCaption.Line = db.GetNullableString(reader, 2);
        entities.CourtCaption.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCourtCaption2()
  {
    entities.CourtCaption.Populated = false;

    return ReadEach("ReadCourtCaption2",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.CourtCaption.LgaIdentifier = db.GetInt32(reader, 0);
        entities.CourtCaption.Number = db.GetInt32(reader, 1);
        entities.CourtCaption.Line = db.GetNullableString(reader, 2);
        entities.CourtCaption.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCourtCaption3()
  {
    return ReadEach("ReadCourtCaption3",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.CourtCaption.LgaIdentifier = db.GetInt32(reader, 0);
        entities.CourtCaption.Number = db.GetInt32(reader, 1);
        entities.CourtCaption.Line = db.GetNullableString(reader, 2);
        entities.CourtCaption.Populated = true;

        return true;
      });
  }

  private bool ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", import.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 1);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.Populated = true;
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
    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
      /// <summary>
      /// A value of Cc.
      /// </summary>
      [JsonPropertyName("cc")]
      public CourtCaption Cc
      {
        get => cc ??= new();
        set => cc = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 105;

      private CourtCaption cc;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// Gets a value of Import1.
    /// </summary>
    [JsonIgnore]
    public Array<ImportGroup> Import1 => import1 ??= new(ImportGroup.Capacity);

    /// <summary>
    /// Gets a value of Import1 for json serialization.
    /// </summary>
    [JsonPropertyName("import1")]
    [Computed]
    public IList<ImportGroup> Import1_Json
    {
      get => import1;
      set => Import1.Assign(value);
    }

    private LegalAction legalAction;
    private Array<ImportGroup> import1;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
      /// <summary>
      /// A value of Cc.
      /// </summary>
      [JsonPropertyName("cc")]
      public CourtCaption Cc
      {
        get => cc ??= new();
        set => cc = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 105;

      private CourtCaption cc;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 => export1 ??= new(ExportGroup.Capacity);

    /// <summary>
    /// Gets a value of Export1 for json serialization.
    /// </summary>
    [JsonPropertyName("export1")]
    [Computed]
    public IList<ExportGroup> Export1_Json
    {
      get => export1;
      set => Export1.Assign(value);
    }

    private Array<ExportGroup> export1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of LineCount.
    /// </summary>
    [JsonPropertyName("lineCount")]
    public Common LineCount
    {
      get => lineCount ??= new();
      set => lineCount = value;
    }

    private Common lineCount;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of CourtCaption.
    /// </summary>
    [JsonPropertyName("courtCaption")]
    public CourtCaption CourtCaption
    {
      get => courtCaption ??= new();
      set => courtCaption = value;
    }

    private LegalAction legalAction;
    private CourtCaption courtCaption;
  }
#endregion
}
