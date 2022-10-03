// Program: SI_CHECK_COURT_CASE_FOR_REFERRAL, ID: 372390696, model: 746.
// Short name: SWE01647
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
/// A program: SI_CHECK_COURT_CASE_FOR_REFERRAL.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class SiCheckCourtCaseForReferral: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CHECK_COURT_CASE_FOR_REFERRAL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCheckCourtCaseForReferral(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCheckCourtCaseForReferral.
  /// </summary>
  public SiCheckCourtCaseForReferral(IContext context, Import import,
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
    // *******************************************************
    // 04/29/97	SHERAZ MALIK	CHANGE CURRENT_DATE
    // 10/25/2002	M Ramirez	changed
    // *******************************************************
    if (Lt(local.Current.Date, import.Batch.ProcessDate))
    {
      local.Current.Date = import.Batch.ProcessDate;
    }
    else
    {
      local.Current.Date = Now().Date;
    }

    if (!IsEmpty(import.Child.Number))
    {
      if (ReadLegalActionPerson())
      {
        return;
      }

      ExitState = "CO0000_LEGAL_ACTION_PERSON_NF";
    }
    else
    {
      // mjr
      // ------------------------------------------
      // 10/25/2002
      // Currently this CAB is only called by OINR and IIOI.  IIOI was
      // changed to call this cab correctly.  When OINR is changed
      // as well, this half of the IF statement should be removed
      // -------------------------------------------------------
      export.ZdelGroupExportParticipant.Index = 0;
      export.ZdelGroupExportParticipant.Clear();

      for(import.ZdelGroupImportParticipant.Index = 0; import
        .ZdelGroupImportParticipant.Index < import
        .ZdelGroupImportParticipant.Count; ++
        import.ZdelGroupImportParticipant.Index)
      {
        if (export.ZdelGroupExportParticipant.IsFull)
        {
          break;
        }

        export.ZdelGroupExportParticipant.Update.ZdelGroupExportSelChild.
          SelectChar =
            import.ZdelGroupImportParticipant.Item.ZdelGroupImportSelChild.
            SelectChar;
        export.ZdelGroupExportParticipant.Update.ZdelGroupExportPerson.Assign(
          import.ZdelGroupImportParticipant.Item.ZdelGroupImportPerson);
        export.ZdelGroupExportParticipant.Next();
      }

      if (!ReadLegalAction())
      {
        ExitState = "SI0000_INVALID_LEGAL_ACTION_SEL";
        export.ZdelExportError.SelectChar = "Y";

        return;
      }

      foreach(var item in ReadLegalActionPersonCsePerson())
      {
        for(export.ZdelGroupExportParticipant.Index = 0; export
          .ZdelGroupExportParticipant.Index < export
          .ZdelGroupExportParticipant.Count; ++
          export.ZdelGroupExportParticipant.Index)
        {
          if (Equal(entities.CsePerson.Number,
            export.ZdelGroupExportParticipant.Item.ZdelGroupExportPerson.
              Number))
          {
            export.ZdelGroupExportParticipant.Update.ZdelGroupExportSelChild.
              SelectChar = "S";
          }
        }
      }
    }
  }

  private bool ReadLegalAction()
  {
    entities.ZdelLegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", import.LegalAction.Identifier);
        db.SetNullableDate(
          command, "filedDt", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ZdelLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ZdelLegalAction.Classification = db.GetString(reader, 1);
        entities.ZdelLegalAction.FiledDate = db.GetNullableDate(reader, 2);
        entities.ZdelLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 3);
        entities.ZdelLegalAction.EndDate = db.GetNullableDate(reader, 4);
        entities.ZdelLegalAction.Populated = true;
      });
  }

  private bool ReadLegalActionPerson()
  {
    entities.LegalActionPerson.Populated = false;

    return Read("ReadLegalActionPerson",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
        db.SetNullableInt32(
          command, "lgaIdentifier", import.LegalAction.Identifier);
        db.SetNullableString(command, "cspNumber", import.Child.Number);
      },
      (db, reader) =>
      {
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 1);
        entities.LegalActionPerson.LgaIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.LegalActionPerson.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionPerson.Role = db.GetString(reader, 4);
        entities.LegalActionPerson.EndDate = db.GetNullableDate(reader, 5);
        entities.LegalActionPerson.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalActionPersonCsePerson()
  {
    entities.CsePerson.Populated = false;
    entities.ZdelLegalActionPerson.Populated = false;

    return ReadEach("ReadLegalActionPersonCsePerson",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.ZdelLegalAction.Identifier);
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ZdelLegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.ZdelLegalActionPerson.CspNumber =
          db.GetNullableString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.ZdelLegalActionPerson.LgaIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.ZdelLegalActionPerson.EffectiveDate = db.GetDate(reader, 3);
        entities.ZdelLegalActionPerson.Role = db.GetString(reader, 4);
        entities.ZdelLegalActionPerson.EndDate = db.GetNullableDate(reader, 5);
        entities.ZdelLegalActionPerson.AccountType =
          db.GetNullableString(reader, 6);
        entities.CsePerson.Populated = true;
        entities.ZdelLegalActionPerson.Populated = true;

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
    /// <summary>A ZdelGroupImportParticipantGroup group.</summary>
    [Serializable]
    public class ZdelGroupImportParticipantGroup
    {
      /// <summary>
      /// A value of ZdelGroupImportSelChild.
      /// </summary>
      [JsonPropertyName("zdelGroupImportSelChild")]
      public Common ZdelGroupImportSelChild
      {
        get => zdelGroupImportSelChild ??= new();
        set => zdelGroupImportSelChild = value;
      }

      /// <summary>
      /// A value of ZdelGroupImportPerson.
      /// </summary>
      [JsonPropertyName("zdelGroupImportPerson")]
      public CsePersonsWorkSet ZdelGroupImportPerson
      {
        get => zdelGroupImportPerson ??= new();
        set => zdelGroupImportPerson = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 7;

      private Common zdelGroupImportSelChild;
      private CsePersonsWorkSet zdelGroupImportPerson;
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
    /// Gets a value of ZdelGroupImportParticipant.
    /// </summary>
    [JsonIgnore]
    public Array<ZdelGroupImportParticipantGroup> ZdelGroupImportParticipant =>
      zdelGroupImportParticipant ??= new(ZdelGroupImportParticipantGroup.
        Capacity);

    /// <summary>
    /// Gets a value of ZdelGroupImportParticipant for json serialization.
    /// </summary>
    [JsonPropertyName("zdelGroupImportParticipant")]
    [Computed]
    public IList<ZdelGroupImportParticipantGroup>
      ZdelGroupImportParticipant_Json
    {
      get => zdelGroupImportParticipant;
      set => ZdelGroupImportParticipant.Assign(value);
    }

    /// <summary>
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CsePersonsWorkSet Child
    {
      get => child ??= new();
      set => child = value;
    }

    /// <summary>
    /// A value of Batch.
    /// </summary>
    [JsonPropertyName("batch")]
    public ProgramProcessingInfo Batch
    {
      get => batch ??= new();
      set => batch = value;
    }

    private LegalAction legalAction;
    private Array<ZdelGroupImportParticipantGroup> zdelGroupImportParticipant;
    private CsePersonsWorkSet child;
    private ProgramProcessingInfo batch;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ZdelGroupExportParticipantGroup group.</summary>
    [Serializable]
    public class ZdelGroupExportParticipantGroup
    {
      /// <summary>
      /// A value of ZdelGroupExportSelChild.
      /// </summary>
      [JsonPropertyName("zdelGroupExportSelChild")]
      public Common ZdelGroupExportSelChild
      {
        get => zdelGroupExportSelChild ??= new();
        set => zdelGroupExportSelChild = value;
      }

      /// <summary>
      /// A value of ZdelGroupExportPerson.
      /// </summary>
      [JsonPropertyName("zdelGroupExportPerson")]
      public CsePersonsWorkSet ZdelGroupExportPerson
      {
        get => zdelGroupExportPerson ??= new();
        set => zdelGroupExportPerson = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 7;

      private Common zdelGroupExportSelChild;
      private CsePersonsWorkSet zdelGroupExportPerson;
    }

    /// <summary>
    /// A value of ZdelExportError.
    /// </summary>
    [JsonPropertyName("zdelExportError")]
    public Common ZdelExportError
    {
      get => zdelExportError ??= new();
      set => zdelExportError = value;
    }

    /// <summary>
    /// Gets a value of ZdelGroupExportParticipant.
    /// </summary>
    [JsonIgnore]
    public Array<ZdelGroupExportParticipantGroup> ZdelGroupExportParticipant =>
      zdelGroupExportParticipant ??= new(ZdelGroupExportParticipantGroup.
        Capacity);

    /// <summary>
    /// Gets a value of ZdelGroupExportParticipant for json serialization.
    /// </summary>
    [JsonPropertyName("zdelGroupExportParticipant")]
    [Computed]
    public IList<ZdelGroupExportParticipantGroup>
      ZdelGroupExportParticipant_Json
    {
      get => zdelGroupExportParticipant;
      set => ZdelGroupExportParticipant.Assign(value);
    }

    private Common zdelExportError;
    private Array<ZdelGroupExportParticipantGroup> zdelGroupExportParticipant;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    private DateWorkArea current;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of ZdelLegalActionPerson.
    /// </summary>
    [JsonPropertyName("zdelLegalActionPerson")]
    public LegalActionPerson ZdelLegalActionPerson
    {
      get => zdelLegalActionPerson ??= new();
      set => zdelLegalActionPerson = value;
    }

    /// <summary>
    /// A value of ZdelLegalAction.
    /// </summary>
    [JsonPropertyName("zdelLegalAction")]
    public LegalAction ZdelLegalAction
    {
      get => zdelLegalAction ??= new();
      set => zdelLegalAction = value;
    }

    private LegalActionPerson legalActionPerson;
    private LegalAction legalAction;
    private CsePerson csePerson;
    private LegalActionPerson zdelLegalActionPerson;
    private LegalAction zdelLegalAction;
  }
#endregion
}
