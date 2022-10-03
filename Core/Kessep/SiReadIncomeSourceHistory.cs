// Program: SI_READ_INCOME_SOURCE_HISTORY, ID: 371766296, model: 746.
// Short name: SWE01222
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_READ_INCOME_SOURCE_HISTORY.
/// </summary>
[Serializable]
public partial class SiReadIncomeSourceHistory: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_READ_INCOME_SOURCE_HISTORY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiReadIncomeSourceHistory(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiReadIncomeSourceHistory.
  /// </summary>
  public SiReadIncomeSourceHistory(IContext context, Import import,
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
    export.IncomeHistory.Index = -1;

    foreach(var item in ReadPersonIncomeHistory())
    {
      if (Equal(entities.PersonIncomeHistory.IncomeEffDt,
        import.PageKey.IncomeEffDt))
      {
        if (Lt(entities.PersonIncomeHistory.Identifier,
          import.PageKey.Identifier))
        {
          continue;
        }
      }

      ++export.IncomeHistory.Index;
      export.IncomeHistory.CheckSize();

      if (export.IncomeHistory.Index >= Export.IncomeHistoryGroup.Capacity)
      {
        MovePersonIncomeHistory(entities.PersonIncomeHistory, export.NextPageKey);
          

        break;
      }

      export.IncomeHistory.Update.PersonIncomeHistory.Assign(
        entities.PersonIncomeHistory);
    }

    if (import.Standard.PageNumber == 1)
    {
      if (export.IncomeHistory.Index >= Export.IncomeHistoryGroup.Capacity)
      {
        export.Standard.ScrollingMessage = "MORE +";
      }
      else
      {
        export.Standard.ScrollingMessage = "";
      }
    }
    else if (export.IncomeHistory.Index >= Export.IncomeHistoryGroup.Capacity)
    {
      export.Standard.ScrollingMessage = "MORE - +";
    }
    else
    {
      export.Standard.ScrollingMessage = "MORE -";
    }
  }

  private static void MovePersonIncomeHistory(PersonIncomeHistory source,
    PersonIncomeHistory target)
  {
    target.Identifier = source.Identifier;
    target.IncomeEffDt = source.IncomeEffDt;
  }

  private IEnumerable<bool> ReadPersonIncomeHistory()
  {
    entities.PersonIncomeHistory.Populated = false;

    return ReadEach("ReadPersonIncomeHistory",
      (db, command) =>
      {
        db.SetDateTime(
          command, "isrIdentifier",
          import.IncomeSource.Identifier.GetValueOrDefault());
        db.SetNullableDate(
          command, "incomeEffDt",
          import.PageKey.IncomeEffDt.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonIncomeHistory.CspNumber = db.GetString(reader, 0);
        entities.PersonIncomeHistory.IsrIdentifier = db.GetDateTime(reader, 1);
        entities.PersonIncomeHistory.Identifier = db.GetDateTime(reader, 2);
        entities.PersonIncomeHistory.IncomeEffDt =
          db.GetNullableDate(reader, 3);
        entities.PersonIncomeHistory.IncomeAmt =
          db.GetNullableDecimal(reader, 4);
        entities.PersonIncomeHistory.Freq = db.GetNullableString(reader, 5);
        entities.PersonIncomeHistory.WorkerId = db.GetNullableString(reader, 6);
        entities.PersonIncomeHistory.VerifiedDt = db.GetNullableDate(reader, 7);
        entities.PersonIncomeHistory.CspINumber = db.GetString(reader, 8);
        entities.PersonIncomeHistory.MilitaryBaqAllotment =
          db.GetNullableDecimal(reader, 9);
        entities.PersonIncomeHistory.Populated = true;

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
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
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
    /// A value of PageKey.
    /// </summary>
    [JsonPropertyName("pageKey")]
    public PersonIncomeHistory PageKey
    {
      get => pageKey ??= new();
      set => pageKey = value;
    }

    private IncomeSource incomeSource;
    private Standard standard;
    private PersonIncomeHistory pageKey;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A IncomeHistoryGroup group.</summary>
    [Serializable]
    public class IncomeHistoryGroup
    {
      /// <summary>
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>
      /// A value of PersonIncomeHistory.
      /// </summary>
      [JsonPropertyName("personIncomeHistory")]
      public PersonIncomeHistory PersonIncomeHistory
      {
        get => personIncomeHistory ??= new();
        set => personIncomeHistory = value;
      }

      /// <summary>
      /// A value of FreqPrompt.
      /// </summary>
      [JsonPropertyName("freqPrompt")]
      public Common FreqPrompt
      {
        get => freqPrompt ??= new();
        set => freqPrompt = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Common common;
      private PersonIncomeHistory personIncomeHistory;
      private Common freqPrompt;
    }

    /// <summary>
    /// A value of NextPageKey.
    /// </summary>
    [JsonPropertyName("nextPageKey")]
    public PersonIncomeHistory NextPageKey
    {
      get => nextPageKey ??= new();
      set => nextPageKey = value;
    }

    /// <summary>
    /// Gets a value of IncomeHistory.
    /// </summary>
    [JsonIgnore]
    public Array<IncomeHistoryGroup> IncomeHistory => incomeHistory ??= new(
      IncomeHistoryGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of IncomeHistory for json serialization.
    /// </summary>
    [JsonPropertyName("incomeHistory")]
    [Computed]
    public IList<IncomeHistoryGroup> IncomeHistory_Json
    {
      get => incomeHistory;
      set => IncomeHistory.Assign(value);
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

    private PersonIncomeHistory nextPageKey;
    private Array<IncomeHistoryGroup> incomeHistory;
    private Standard standard;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of PersonIncomeHistory.
    /// </summary>
    [JsonPropertyName("personIncomeHistory")]
    public PersonIncomeHistory PersonIncomeHistory
    {
      get => personIncomeHistory ??= new();
      set => personIncomeHistory = value;
    }

    /// <summary>
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
    }

    private PersonIncomeHistory personIncomeHistory;
    private IncomeSource incomeSource;
  }
#endregion
}
