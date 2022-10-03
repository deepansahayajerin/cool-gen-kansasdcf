// Program: LE_DELETE_COURT_CAPTION_LINES, ID: 372010971, model: 746.
// Short name: SWE00752
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_DELETE_COURT_CAPTION_LINES.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action will Delete Court Caption Lines for the specified Court Case 
/// Number.
/// </para>
/// </summary>
[Serializable]
public partial class LeDeleteCourtCaptionLines: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_DELETE_COURT_CAPTION_LINES program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeDeleteCourtCaptionLines(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeDeleteCourtCaptionLines.
  /// </summary>
  public LeDeleteCourtCaptionLines(IContext context, Import import,
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
    // 10/07/98        Dick Jean                       Move read/delete
    //                                                 
    // under when successful
    // ------------------------------------------------------------
    if (ReadLegalAction())
    {
      // ------------------------------------------------------------
      // Delete all Court Caption Lines associated with the initial
      // Legal Action.
      // ------------------------------------------------------------
      foreach(var item in ReadCourtCaption())
      {
        DeleteCourtCaption();
      }
    }
    else
    {
      ExitState = "LEGAL_ACTION_NF";
    }
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

  private IEnumerable<bool> ReadCourtCaption()
  {
    entities.CourtCaption.Populated = false;

    return ReadEach("ReadCourtCaption",
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    private LegalAction legalAction;
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
