// Program: READ_DOCUMENT, ID: 372106133, model: 746.
// Short name: SWE01026
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: READ_DOCUMENT.
/// </summary>
[Serializable]
public partial class ReadDocument: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the READ_DOCUMENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new ReadDocument(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of ReadDocument.
  /// </summary>
  public ReadDocument(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // mjr
    // -----------------------------------
    // 09/16/1998
    // Added expiration date check:  DOCM only lists the effective documents
    // ------------------------------------------------
    local.MaxDate.Date = new DateTime(2099, 12, 31);

    if (IsEmpty(import.Filter.Type1))
    {
      export.Export1.Index = 0;
      export.Export1.Clear();

      foreach(var item in ReadDocument2())
      {
        export.Export1.Update.G.Assign(entities.Document);
        export.Export1.Next();
      }
    }
    else
    {
      export.Export1.Index = 0;
      export.Export1.Clear();

      foreach(var item in ReadDocument1())
      {
        export.Export1.Update.G.Assign(entities.Document);
        export.Export1.Next();
      }
    }

    if (export.Export1.IsEmpty)
    {
      export.PageKey.Name = "";

      if (import.PageNumber.PageNumber == 1)
      {
        export.Standard.ScrollingMessage = "MORE";
        ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";

        return;
      }
    }

    if (export.Export1.IsFull)
    {
      export.PageKey.Name = entities.Document.Name;

      if (import.PageNumber.PageNumber > 1)
      {
        export.Standard.ScrollingMessage = "MORE - +";
      }
      else
      {
        export.Standard.ScrollingMessage = "MORE   +";
      }
    }
    else if (import.PageNumber.PageNumber <= 1)
    {
      export.Standard.ScrollingMessage = "MORE";
    }
    else
    {
      export.Standard.ScrollingMessage = "MORE -";
    }
  }

  private IEnumerable<bool> ReadDocument1()
  {
    return ReadEach("ReadDocument1",
      (db, command) =>
      {
        db.SetString(command, "name", import.PageStartKey.Name);
        db.SetString(command, "type", import.Filter.Type1);
        db.SetDate(
          command, "expirationDate", local.MaxDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.Document.Name = db.GetString(reader, 0);
        entities.Document.Type1 = db.GetString(reader, 1);
        entities.Document.CreatedBy = db.GetString(reader, 2);
        entities.Document.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.Document.LastUpdatedBy = db.GetNullableString(reader, 4);
        entities.Document.LastUpdatdTstamp = db.GetNullableDateTime(reader, 5);
        entities.Document.Description = db.GetNullableString(reader, 6);
        entities.Document.EffectiveDate = db.GetDate(reader, 7);
        entities.Document.ExpirationDate = db.GetDate(reader, 8);
        entities.Document.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadDocument2()
  {
    return ReadEach("ReadDocument2",
      (db, command) =>
      {
        db.SetString(command, "name", import.PageStartKey.Name);
        db.SetDate(
          command, "expirationDate", local.MaxDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.Document.Name = db.GetString(reader, 0);
        entities.Document.Type1 = db.GetString(reader, 1);
        entities.Document.CreatedBy = db.GetString(reader, 2);
        entities.Document.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.Document.LastUpdatedBy = db.GetNullableString(reader, 4);
        entities.Document.LastUpdatdTstamp = db.GetNullableDateTime(reader, 5);
        entities.Document.Description = db.GetNullableString(reader, 6);
        entities.Document.EffectiveDate = db.GetDate(reader, 7);
        entities.Document.ExpirationDate = db.GetDate(reader, 8);
        entities.Document.Populated = true;

        return true;
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
    /// A value of Filter.
    /// </summary>
    [JsonPropertyName("filter")]
    public Document Filter
    {
      get => filter ??= new();
      set => filter = value;
    }

    /// <summary>
    /// A value of PageStartKey.
    /// </summary>
    [JsonPropertyName("pageStartKey")]
    public Document PageStartKey
    {
      get => pageStartKey ??= new();
      set => pageStartKey = value;
    }

    /// <summary>
    /// A value of PageNumber.
    /// </summary>
    [JsonPropertyName("pageNumber")]
    public Standard PageNumber
    {
      get => pageNumber ??= new();
      set => pageNumber = value;
    }

    private Document filter;
    private Document pageStartKey;
    private Standard pageNumber;
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
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public Document G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public Common Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 13;

      private Document g;
      private Common detail;
    }

    /// <summary>
    /// A value of PageKey.
    /// </summary>
    [JsonPropertyName("pageKey")]
    public Document PageKey
    {
      get => pageKey ??= new();
      set => pageKey = value;
    }

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
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

    private Document pageKey;
    private Standard standard;
    private Array<ExportGroup> export1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public DateWorkArea MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

    private DateWorkArea maxDate;
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
