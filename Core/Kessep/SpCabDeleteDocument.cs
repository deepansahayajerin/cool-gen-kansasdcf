// Program: SP_CAB_DELETE_DOCUMENT, ID: 372107396, model: 746.
// Short name: SWE01686
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_CAB_DELETE_DOCUMENT.
/// </para>
/// <para>
///   Performs delete of table DOCUMENT.
/// </para>
/// </summary>
[Serializable]
public partial class SpCabDeleteDocument: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_DELETE_DOCUMENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabDeleteDocument(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabDeleteDocument.
  /// </summary>
  public SpCabDeleteDocument(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (ReadDocument())
    {
      DeleteDocument();
      ExitState = "ACO_NI0000_DELETE_SUCCESSFUL";
    }
    else
    {
      ExitState = "DOCUMENT_NF";
    }
  }

  private void DeleteDocument()
  {
    bool exists;

    exists = Read("DeleteDocument#1",
      (db, command) =>
      {
        db.SetNullableString(command, "docName", entities.Document.Name);
        db.SetNullableDate(
          command, "docEffectiveDte",
          entities.Document.EffectiveDate.GetValueOrDefault());
      },
      null);

    if (exists)
    {
      throw DataError("Restrict violation on table \"CKT_OUTGOING_DOC\".",
        "50001");
    }

    Update("DeleteDocument#2",
      (db, command) =>
      {
        db.SetNullableString(command, "docName", entities.Document.Name);
        db.SetNullableDate(
          command, "docEffectiveDte",
          entities.Document.EffectiveDate.GetValueOrDefault());
      });
  }

  private bool ReadDocument()
  {
    entities.Document.Populated = false;

    return Read("ReadDocument",
      (db, command) =>
      {
        db.SetString(command, "name", import.Document.Name);
        db.SetDate(
          command, "effectiveDate",
          import.Document.EffectiveDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Document.Name = db.GetString(reader, 0);
        entities.Document.EffectiveDate = db.GetDate(reader, 1);
        entities.Document.Populated = true;
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
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
    }

    private Document document;
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
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
    }

    private Document document;
  }
#endregion
}
