// Program: LE_CREATE_DUPLICAT_COURT_CAPTION, ID: 371985166, model: 746.
// Short name: SWE00738
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_CREATE_DUPLICAT_COURT_CAPTION.
/// </summary>
[Serializable]
public partial class LeCreateDuplicatCourtCaption: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_CREATE_DUPLICAT_COURT_CAPTION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeCreateDuplicatCourtCaption(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeCreateDuplicatCourtCaption.
  /// </summary>
  public LeCreateDuplicatCourtCaption(IContext context, Import import,
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
    if (ReadLegalAction1())
    {
      if (!ReadTribunal())
      {
        ExitState = "TRIBUNAL_NF";

        return;
      }

      if (ReadLegalAction2())
      {
        foreach(var item in ReadCourtCaption())
        {
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
      }
    }
    else
    {
      ExitState = "LEGAL_ACTION_NF";
    }
  }

  private void CreateCourtCaption()
  {
    var lgaIdentifier = entities.NewLegalAction.Identifier;
    var number = entities.PreviousCourtCaption.Number;
    var line = entities.PreviousCourtCaption.Line;

    entities.NewCourtCaption.Populated = false;
    Update("CreateCourtCaption",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", lgaIdentifier);
        db.SetInt32(command, "courtCaptionNo", number);
        db.SetNullableString(command, "line", line);
      });

    entities.NewCourtCaption.LgaIdentifier = lgaIdentifier;
    entities.NewCourtCaption.Number = number;
    entities.NewCourtCaption.Line = line;
    entities.NewCourtCaption.Populated = true;
  }

  private IEnumerable<bool> ReadCourtCaption()
  {
    entities.PreviousCourtCaption.Populated = false;

    return ReadEach("ReadCourtCaption",
      (db, command) =>
      {
        db.SetInt32(
          command, "lgaIdentifier", entities.PreviousLegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.PreviousCourtCaption.LgaIdentifier = db.GetInt32(reader, 0);
        entities.PreviousCourtCaption.Number = db.GetInt32(reader, 1);
        entities.PreviousCourtCaption.Line = db.GetNullableString(reader, 2);
        entities.PreviousCourtCaption.Populated = true;

        return true;
      });
  }

  private bool ReadLegalAction1()
  {
    entities.NewLegalAction.Populated = false;

    return Read("ReadLegalAction1",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", import.Current.Identifier);
      },
      (db, reader) =>
      {
        entities.NewLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.NewLegalAction.TrbId = db.GetNullableInt32(reader, 1);
        entities.NewLegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction2()
  {
    entities.PreviousLegalAction.Populated = false;

    return Read("ReadLegalAction2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", import.Current.CourtCaseNumber ?? "");
        db.SetInt32(command, "legalActionId", import.Current.Identifier);
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.PreviousLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.PreviousLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 1);
        entities.PreviousLegalAction.CreatedTstamp = db.GetDateTime(reader, 2);
        entities.PreviousLegalAction.TrbId = db.GetNullableInt32(reader, 3);
        entities.PreviousLegalAction.Populated = true;
      });
  }

  private bool ReadTribunal()
  {
    System.Diagnostics.Debug.Assert(entities.NewLegalAction.Populated);
    entities.Tribunal.Populated = false;

    return Read("ReadTribunal",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.NewLegalAction.TrbId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Tribunal.Identifier = db.GetInt32(reader, 0);
        entities.Tribunal.Populated = true;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public LegalAction Current
    {
      get => current ??= new();
      set => current = value;
    }

    private LegalAction current;
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
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of PreviousCourtCaption.
    /// </summary>
    [JsonPropertyName("previousCourtCaption")]
    public CourtCaption PreviousCourtCaption
    {
      get => previousCourtCaption ??= new();
      set => previousCourtCaption = value;
    }

    /// <summary>
    /// A value of NewLegalAction.
    /// </summary>
    [JsonPropertyName("newLegalAction")]
    public LegalAction NewLegalAction
    {
      get => newLegalAction ??= new();
      set => newLegalAction = value;
    }

    /// <summary>
    /// A value of PreviousLegalAction.
    /// </summary>
    [JsonPropertyName("previousLegalAction")]
    public LegalAction PreviousLegalAction
    {
      get => previousLegalAction ??= new();
      set => previousLegalAction = value;
    }

    /// <summary>
    /// A value of NewCourtCaption.
    /// </summary>
    [JsonPropertyName("newCourtCaption")]
    public CourtCaption NewCourtCaption
    {
      get => newCourtCaption ??= new();
      set => newCourtCaption = value;
    }

    private Tribunal tribunal;
    private CourtCaption previousCourtCaption;
    private LegalAction newLegalAction;
    private LegalAction previousLegalAction;
    private CourtCaption newCourtCaption;
  }
#endregion
}
