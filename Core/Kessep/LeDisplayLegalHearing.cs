// Program: LE_DISPLAY_LEGAL_HEARING, ID: 372012089, model: 746.
// Short name: SWE00768
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
/// A program: LE_DISPLAY_LEGAL_HEARING.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block displays the Legal Hearing and all Hearing Addresses 
/// associated to it for the specified Legal Action.
/// </para>
/// </summary>
[Serializable]
public partial class LeDisplayLegalHearing: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_DISPLAY_LEGAL_HEARING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeDisplayLegalHearing(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeDisplayLegalHearing.
  /// </summary>
  public LeDisplayLegalHearing(IContext context, Import import, Export export):
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
    // 06/30/95	Dave Allen			Initial Code
    // 02/22/96	govind				Added code for HRNX and HRPV to scroll hearing
    // 11/21/98	R. Jean			        Eliminate read of LEGAL ACTION; changed LEGAL 
    // ACTION qualifiers in subsequent reads
    // ------------------------------------------------------------
    // 9/28/17   JHarden      CQ58574    allow a note to be added to the HEAR 
    // screen.
    export.Hearing2.Assign(import.Hearing);

    if (ReadTribunal())
    {
      MoveTribunal(entities.Tribunal, export.Tribunal);

      export.Addresses1.Index = 0;
      export.Addresses1.Clear();

      foreach(var item in ReadFipsTribAddress())
      {
        export.Addresses1.Update.FipsTribAddress.
          Assign(entities.FipsTribAddress);
        export.Addresses1.Next();
      }
    }
    else
    {
      ExitState = "TRIBUNAL_NF";

      return;
    }

    local.HearingFound.Flag = "N";

    switch(TrimEnd(import.UserAction.Command))
    {
      case "DISPLAY":
        foreach(var item in ReadHearing5())
        {
          if (Lt(local.InitialisedToZeros.Date, import.Hearing.ConductedDate) &&
            !
            Equal(entities.Hearing.ConductedDate, import.Hearing.ConductedDate))
          {
            continue;
          }

          export.Hearing2.Assign(entities.Hearing);
          local.HearingFound.Flag = "Y";

          break;
        }

        if (AsChar(local.HearingFound.Flag) == 'N')
        {
          ExitState = "HEARING_NF";

          return;
        }

        break;
      case "HRPV":
        if (ReadHearing4())
        {
          export.Hearing2.Assign(entities.Hearing);
          local.HearingFound.Flag = "Y";
        }

        if (AsChar(local.HearingFound.Flag) == 'N')
        {
          ExitState = "LE0000_NO_MORE_HEARING";

          return;
        }

        break;
      case "HRNX":
        if (ReadHearing2())
        {
          export.Hearing2.Assign(entities.Hearing);
          local.HearingFound.Flag = "Y";
        }

        if (AsChar(local.HearingFound.Flag) == 'N')
        {
          export.Hearing2.Assign(local.Hearing);
          ExitState = "LE0000_NO_MORE_HEARING";

          return;
        }

        break;
      default:
        break;
    }

    if (ReadHearing1())
    {
      export.Hearing1.PlusFlag = "+";
    }

    if (ReadHearing3())
    {
      export.Hearing1.MinusFlag = "-";
    }
  }

  private static void MoveTribunal(Tribunal source, Tribunal target)
  {
    target.Identifier = source.Identifier;
    target.Name = source.Name;
  }

  private IEnumerable<bool> ReadFipsTribAddress()
  {
    return ReadEach("ReadFipsTribAddress",
      (db, command) =>
      {
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        if (export.Addresses1.IsFull)
        {
          return false;
        }

        entities.FipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.FipsTribAddress.Type1 = db.GetString(reader, 1);
        entities.FipsTribAddress.Street1 = db.GetString(reader, 2);
        entities.FipsTribAddress.Street2 = db.GetNullableString(reader, 3);
        entities.FipsTribAddress.City = db.GetString(reader, 4);
        entities.FipsTribAddress.State = db.GetString(reader, 5);
        entities.FipsTribAddress.ZipCode = db.GetString(reader, 6);
        entities.FipsTribAddress.Zip4 = db.GetNullableString(reader, 7);
        entities.FipsTribAddress.Zip3 = db.GetNullableString(reader, 8);
        entities.FipsTribAddress.County = db.GetNullableString(reader, 9);
        entities.FipsTribAddress.Country = db.GetNullableString(reader, 10);
        entities.FipsTribAddress.TrbId = db.GetNullableInt32(reader, 11);
        entities.FipsTribAddress.Populated = true;

        return true;
      });
  }

  private bool ReadHearing1()
  {
    entities.Hearing.Populated = false;

    return Read("ReadHearing1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", import.LegalAction.Identifier);
        db.SetDate(
          command, "hearingDt",
          export.Hearing2.ConductedDate.GetValueOrDefault());
        db.SetTimeSpan(command, "hearingTime", export.Hearing2.ConductedTime);
      },
      (db, reader) =>
      {
        entities.Hearing.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Hearing.LgaIdentifier = db.GetNullableInt32(reader, 1);
        entities.Hearing.ConductedDate = db.GetDate(reader, 2);
        entities.Hearing.ConductedTime = db.GetTimeSpan(reader, 3);
        entities.Hearing.Type1 = db.GetNullableString(reader, 4);
        entities.Hearing.LastName = db.GetString(reader, 5);
        entities.Hearing.FirstName = db.GetString(reader, 6);
        entities.Hearing.MiddleInt = db.GetNullableString(reader, 7);
        entities.Hearing.Suffix = db.GetNullableString(reader, 8);
        entities.Hearing.Title = db.GetNullableString(reader, 9);
        entities.Hearing.OutcomeReceivedDate = db.GetNullableDate(reader, 10);
        entities.Hearing.Outcome = db.GetNullableString(reader, 11);
        entities.Hearing.Note = db.GetNullableString(reader, 12);
        entities.Hearing.Populated = true;
      });
  }

  private bool ReadHearing2()
  {
    entities.Hearing.Populated = false;

    return Read("ReadHearing2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", import.LegalAction.Identifier);
        db.SetDate(
          command, "hearingDt",
          import.Hearing.ConductedDate.GetValueOrDefault());
        db.SetTimeSpan(command, "hearingTime", import.Hearing.ConductedTime);
      },
      (db, reader) =>
      {
        entities.Hearing.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Hearing.LgaIdentifier = db.GetNullableInt32(reader, 1);
        entities.Hearing.ConductedDate = db.GetDate(reader, 2);
        entities.Hearing.ConductedTime = db.GetTimeSpan(reader, 3);
        entities.Hearing.Type1 = db.GetNullableString(reader, 4);
        entities.Hearing.LastName = db.GetString(reader, 5);
        entities.Hearing.FirstName = db.GetString(reader, 6);
        entities.Hearing.MiddleInt = db.GetNullableString(reader, 7);
        entities.Hearing.Suffix = db.GetNullableString(reader, 8);
        entities.Hearing.Title = db.GetNullableString(reader, 9);
        entities.Hearing.OutcomeReceivedDate = db.GetNullableDate(reader, 10);
        entities.Hearing.Outcome = db.GetNullableString(reader, 11);
        entities.Hearing.Note = db.GetNullableString(reader, 12);
        entities.Hearing.Populated = true;
      });
  }

  private bool ReadHearing3()
  {
    entities.Hearing.Populated = false;

    return Read("ReadHearing3",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", import.LegalAction.Identifier);
        db.SetDate(
          command, "hearingDt",
          export.Hearing2.ConductedDate.GetValueOrDefault());
        db.SetTimeSpan(command, "hearingTime", export.Hearing2.ConductedTime);
      },
      (db, reader) =>
      {
        entities.Hearing.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Hearing.LgaIdentifier = db.GetNullableInt32(reader, 1);
        entities.Hearing.ConductedDate = db.GetDate(reader, 2);
        entities.Hearing.ConductedTime = db.GetTimeSpan(reader, 3);
        entities.Hearing.Type1 = db.GetNullableString(reader, 4);
        entities.Hearing.LastName = db.GetString(reader, 5);
        entities.Hearing.FirstName = db.GetString(reader, 6);
        entities.Hearing.MiddleInt = db.GetNullableString(reader, 7);
        entities.Hearing.Suffix = db.GetNullableString(reader, 8);
        entities.Hearing.Title = db.GetNullableString(reader, 9);
        entities.Hearing.OutcomeReceivedDate = db.GetNullableDate(reader, 10);
        entities.Hearing.Outcome = db.GetNullableString(reader, 11);
        entities.Hearing.Note = db.GetNullableString(reader, 12);
        entities.Hearing.Populated = true;
      });
  }

  private bool ReadHearing4()
  {
    entities.Hearing.Populated = false;

    return Read("ReadHearing4",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", import.LegalAction.Identifier);
        db.SetDate(
          command, "hearingDt",
          import.Hearing.ConductedDate.GetValueOrDefault());
        db.SetTimeSpan(command, "hearingTime", import.Hearing.ConductedTime);
      },
      (db, reader) =>
      {
        entities.Hearing.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Hearing.LgaIdentifier = db.GetNullableInt32(reader, 1);
        entities.Hearing.ConductedDate = db.GetDate(reader, 2);
        entities.Hearing.ConductedTime = db.GetTimeSpan(reader, 3);
        entities.Hearing.Type1 = db.GetNullableString(reader, 4);
        entities.Hearing.LastName = db.GetString(reader, 5);
        entities.Hearing.FirstName = db.GetString(reader, 6);
        entities.Hearing.MiddleInt = db.GetNullableString(reader, 7);
        entities.Hearing.Suffix = db.GetNullableString(reader, 8);
        entities.Hearing.Title = db.GetNullableString(reader, 9);
        entities.Hearing.OutcomeReceivedDate = db.GetNullableDate(reader, 10);
        entities.Hearing.Outcome = db.GetNullableString(reader, 11);
        entities.Hearing.Note = db.GetNullableString(reader, 12);
        entities.Hearing.Populated = true;
      });
  }

  private IEnumerable<bool> ReadHearing5()
  {
    entities.Hearing.Populated = false;

    return ReadEach("ReadHearing5",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", import.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.Hearing.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Hearing.LgaIdentifier = db.GetNullableInt32(reader, 1);
        entities.Hearing.ConductedDate = db.GetDate(reader, 2);
        entities.Hearing.ConductedTime = db.GetTimeSpan(reader, 3);
        entities.Hearing.Type1 = db.GetNullableString(reader, 4);
        entities.Hearing.LastName = db.GetString(reader, 5);
        entities.Hearing.FirstName = db.GetString(reader, 6);
        entities.Hearing.MiddleInt = db.GetNullableString(reader, 7);
        entities.Hearing.Suffix = db.GetNullableString(reader, 8);
        entities.Hearing.Title = db.GetNullableString(reader, 9);
        entities.Hearing.OutcomeReceivedDate = db.GetNullableDate(reader, 10);
        entities.Hearing.Outcome = db.GetNullableString(reader, 11);
        entities.Hearing.Note = db.GetNullableString(reader, 12);
        entities.Hearing.Populated = true;

        return true;
      });
  }

  private bool ReadTribunal()
  {
    entities.Tribunal.Populated = false;

    return Read("ReadTribunal",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", import.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.Tribunal.Name = db.GetString(reader, 0);
        entities.Tribunal.Identifier = db.GetInt32(reader, 1);
        entities.Tribunal.Populated = true;
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
    /// A value of UserAction.
    /// </summary>
    [JsonPropertyName("userAction")]
    public Common UserAction
    {
      get => userAction ??= new();
      set => userAction = value;
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
    /// A value of Hearing.
    /// </summary>
    [JsonPropertyName("hearing")]
    public Hearing Hearing
    {
      get => hearing ??= new();
      set => hearing = value;
    }

    private Common userAction;
    private LegalAction legalAction;
    private Hearing hearing;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A Addresses1Group group.</summary>
    [Serializable]
    public class Addresses1Group
    {
      /// <summary>
      /// A value of FipsTribAddress.
      /// </summary>
      [JsonPropertyName("fipsTribAddress")]
      public FipsTribAddress FipsTribAddress
      {
        get => fipsTribAddress ??= new();
        set => fipsTribAddress = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private FipsTribAddress fipsTribAddress;
    }

    /// <summary>
    /// A value of Hearing1.
    /// </summary>
    [JsonPropertyName("hearing1")]
    public ScrollingAttributes Hearing1
    {
      get => hearing1 ??= new();
      set => hearing1 = value;
    }

    /// <summary>
    /// A value of Hearing2.
    /// </summary>
    [JsonPropertyName("hearing2")]
    public Hearing Hearing2
    {
      get => hearing2 ??= new();
      set => hearing2 = value;
    }

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
    /// Gets a value of Addresses1.
    /// </summary>
    [JsonIgnore]
    public Array<Addresses1Group> Addresses1 => addresses1 ??= new(
      Addresses1Group.Capacity);

    /// <summary>
    /// Gets a value of Addresses1 for json serialization.
    /// </summary>
    [JsonPropertyName("addresses1")]
    [Computed]
    public IList<Addresses1Group> Addresses1_Json
    {
      get => addresses1;
      set => Addresses1.Assign(value);
    }

    private ScrollingAttributes hearing1;
    private Hearing hearing2;
    private Tribunal tribunal;
    private Array<Addresses1Group> addresses1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Hearing.
    /// </summary>
    [JsonPropertyName("hearing")]
    public Hearing Hearing
    {
      get => hearing ??= new();
      set => hearing = value;
    }

    /// <summary>
    /// A value of HearingFound.
    /// </summary>
    [JsonPropertyName("hearingFound")]
    public Common HearingFound
    {
      get => hearingFound ??= new();
      set => hearingFound = value;
    }

    /// <summary>
    /// A value of InitialisedToZeros.
    /// </summary>
    [JsonPropertyName("initialisedToZeros")]
    public DateWorkArea InitialisedToZeros
    {
      get => initialisedToZeros ??= new();
      set => initialisedToZeros = value;
    }

    /// <summary>
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    private Hearing hearing;
    private Common hearingFound;
    private DateWorkArea initialisedToZeros;
    private Common common;
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
    /// A value of Hearing.
    /// </summary>
    [JsonPropertyName("hearing")]
    public Hearing Hearing
    {
      get => hearing ??= new();
      set => hearing = value;
    }

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
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
    }

    private LegalAction legalAction;
    private Hearing hearing;
    private Tribunal tribunal;
    private FipsTribAddress fipsTribAddress;
  }
#endregion
}
