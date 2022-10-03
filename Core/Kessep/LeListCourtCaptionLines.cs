// Program: LE_LIST_COURT_CAPTION_LINES, ID: 372010972, model: 746.
// Short name: SWE00797
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_LIST_COURT_CAPTION_LINES.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action will Display Court Caption Lines for the specified Court Case 
/// Number.
/// </para>
/// </summary>
[Serializable]
public partial class LeListCourtCaptionLines: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_LIST_COURT_CAPTION_LINES program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeListCourtCaptionLines(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeListCourtCaptionLines.
  /// </summary>
  public LeListCourtCaptionLines(IContext context, Import import, Export export):
    
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
    // 10/07/98        Dick Jean                       Remove unnecessary
    //                                                 
    // read of legal action
    // ------------------------------------------------------------
    // ------------------------------------------------------------
    // Get all Court Caption Lines associated to the initial Legal
    // Action.
    // ------------------------------------------------------------
    export.Export1.Index = 0;
    export.Export1.Clear();

    foreach(var item in ReadCourtCaption())
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

  private IEnumerable<bool> ReadCourtCaption()
  {
    return ReadEach("ReadCourtCaption",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", import.LegalAction.Identifier);
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
