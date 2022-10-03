// Program: OE_HICV_DELETE_HINS_COV_VIABLTY, ID: 371850665, model: 746.
// Short name: SWE00929
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
/// A program: OE_HICV_DELETE_HINS_COV_VIABLTY.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This action block deletes HEALTH INSURANCE VIABILITY record.
/// </para>
/// </summary>
[Serializable]
public partial class OeHicvDeleteHinsCovViablty: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_HICV_DELETE_HINS_COV_VIABLTY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeHicvDeleteHinsCovViablty(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeHicvDeleteHinsCovViablty.
  /// </summary>
  public OeHicvDeleteHinsCovViablty(IContext context, Import import,
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
    // *********************************************
    // SYSTEM:		KESSEP
    // MODULE:
    // MODULE TYPE:
    // DESCRIPTION:
    // This action block DELETEs HEALTH INSURANCE VIABILITY record
    // PROCESSING:
    // ACTION BLOCKS:
    // ENTITY TYPES USED:
    //  CSE_PERSON			- R - -
    //  CASE				- R - -
    //  CASE_ROLE CHILD                - R - -
    //  HEALTH_INSURANCE_VIABILITY     - R - D
    // DATABASE FILES USED:
    // CREATED BY:	Govindaraj
    // DATE CREATED:	05/16/1995
    // *********************************************
    // *********************************************
    // MAINTENANCE LOG
    // AUTHOR	DATE		CHG REQ#	DESCRIPTION
    // govind	05/16/95			Initial coding
    // G P Kim	04/30/97			Change Current Date
    //         
    // *********************************************
    // 	
    local.Current.Date = Now().Date;

    export.HicvNote.Index = 0;
    export.HicvNote.Clear();

    for(import.HicvNote.Index = 0; import.HicvNote.Index < import
      .HicvNote.Count; ++import.HicvNote.Index)
    {
      if (export.HicvNote.IsFull)
      {
        break;
      }

      MoveHinsViabNote(import.HicvNote.Item.Detail,
        export.HicvNote.Update.Detail);
      export.HicvNote.Next();
    }

    if (!ReadCsePerson2())
    {
      ExitState = "OE0056_NF_CHILD_CSE_PERSON";

      return;
    }

    if (!ReadCase())
    {
      ExitState = "CASE_NF";

      return;
    }

    if (!ReadCsePerson1())
    {
      ExitState = "CO0000_ABSENT_PARENT_NF";

      return;
    }

    if (!ReadChild())
    {
      ExitState = "OE0065_NF_CASE_ROLE_CHILD";

      return;
    }

    if (ReadHealthInsuranceViability())
    {
      export.HealthInsuranceViability.Assign(
        entities.ExistingHealthInsuranceViability);

      if (entities.ExistingHealthInsuranceViability.Identifier != import
        .HealthInsuranceViability.Identifier)
      {
        ExitState = "OE0116_NOT_THE_LATEST_HINSVIAB";

        return;
      }

      if (!Equal(entities.ExistingHealthInsuranceViability.
        HinsViableIndWorkerId, global.UserId))
      {
        ExitState = "OE0117_HICV_NOT_CREATED_BY_USER";

        return;
      }

      DeleteHealthInsuranceViability();
    }

    if (export.HealthInsuranceViability.Identifier == 0)
    {
      ExitState = "HINS_VIABILITY_NF";

      return;
    }

    export.HealthInsuranceViability.Assign(local.InitialisedToBlanks);

    if (ReadHealthInsuranceViability())
    {
      export.HealthInsuranceViability.Assign(
        entities.ExistingHealthInsuranceViability);

      export.HicvNote.Index = 0;
      export.HicvNote.Clear();

      foreach(var item in ReadHinsViabNote())
      {
        MoveHinsViabNote(entities.ExistingHinsViabNote,
          export.HicvNote.Update.Detail);
        export.HicvNote.Next();
      }
    }
  }

  private static void MoveHinsViabNote(HinsViabNote source, HinsViabNote target)
  {
    target.Identifier = source.Identifier;
    target.Note = source.Note;
  }

  private void DeleteHealthInsuranceViability()
  {
    Update("DeleteHealthInsuranceViability",
      (db, command) =>
      {
        db.SetString(
          command, "croType",
          entities.ExistingHealthInsuranceViability.CroType);
        db.SetString(
          command, "cspNumber",
          entities.ExistingHealthInsuranceViability.CspNumber);
        db.SetString(
          command, "casNumber",
          entities.ExistingHealthInsuranceViability.CasNumber);
        db.SetInt32(
          command, "croIdentifier",
          entities.ExistingHealthInsuranceViability.CroIdentifier);
        db.SetInt32(
          command, "hinsvId",
          entities.ExistingHealthInsuranceViability.Identifier);
      });
  }

  private bool ReadCase()
  {
    entities.ExistingCase.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCase.Number = db.GetString(reader, 0);
        entities.ExistingCase.Populated = true;
      });
  }

  private bool ReadChild()
  {
    entities.ExistingChild1.Populated = false;

    return Read("ReadChild",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
        db.SetString(command, "cspNumber", entities.ExistingChild2.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingChild1.CasNumber = db.GetString(reader, 0);
        entities.ExistingChild1.CspNumber = db.GetString(reader, 1);
        entities.ExistingChild1.Type1 = db.GetString(reader, 2);
        entities.ExistingChild1.Identifier = db.GetInt32(reader, 3);
        entities.ExistingChild1.StartDate = db.GetNullableDate(reader, 4);
        entities.ExistingChild1.EndDate = db.GetNullableDate(reader, 5);
        entities.ExistingChild1.ArWaivedInsurance =
          db.GetNullableString(reader, 6);
        entities.ExistingChild1.DateOfEmancipation =
          db.GetNullableDate(reader, 7);
        entities.ExistingChild1.Over18AndInSchool =
          db.GetNullableString(reader, 8);
        entities.ExistingChild1.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingChild1.Type1);
      });
  }

  private bool ReadCsePerson1()
  {
    entities.ExistingAp.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Ap.Number);
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingAp.Number = db.GetString(reader, 0);
        entities.ExistingAp.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    entities.ExistingChild2.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Child1.Number);
      },
      (db, reader) =>
      {
        entities.ExistingChild2.Number = db.GetString(reader, 0);
        entities.ExistingChild2.Populated = true;
      });
  }

  private bool ReadHealthInsuranceViability()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingChild1.Populated);
    entities.ExistingHealthInsuranceViability.Populated = false;

    return Read("ReadHealthInsuranceViability",
      (db, command) =>
      {
        db.
          SetInt32(command, "croIdentifier", entities.ExistingChild1.Identifier);
          
        db.SetString(command, "croType", entities.ExistingChild1.Type1);
        db.SetString(command, "casNumber", entities.ExistingChild1.CasNumber);
        db.SetString(command, "cspNumber", entities.ExistingChild1.CspNumber);
        db.SetNullableString(command, "cspNum", entities.ExistingAp.Number);
      },
      (db, reader) =>
      {
        entities.ExistingHealthInsuranceViability.CroType =
          db.GetString(reader, 0);
        entities.ExistingHealthInsuranceViability.CspNumber =
          db.GetString(reader, 1);
        entities.ExistingHealthInsuranceViability.CasNumber =
          db.GetString(reader, 2);
        entities.ExistingHealthInsuranceViability.CroIdentifier =
          db.GetInt32(reader, 3);
        entities.ExistingHealthInsuranceViability.Identifier =
          db.GetInt32(reader, 4);
        entities.ExistingHealthInsuranceViability.HinsViableInd =
          db.GetNullableString(reader, 5);
        entities.ExistingHealthInsuranceViability.HinsViableIndWorkerId =
          db.GetNullableString(reader, 6);
        entities.ExistingHealthInsuranceViability.HinsViableIndUpdatedDate =
          db.GetNullableDate(reader, 7);
        entities.ExistingHealthInsuranceViability.CreatedBy =
          db.GetString(reader, 8);
        entities.ExistingHealthInsuranceViability.CreatedTimestamp =
          db.GetDateTime(reader, 9);
        entities.ExistingHealthInsuranceViability.LastUpdatedBy =
          db.GetString(reader, 10);
        entities.ExistingHealthInsuranceViability.LastUpdatedTimestamp =
          db.GetDateTime(reader, 11);
        entities.ExistingHealthInsuranceViability.CspNum =
          db.GetNullableString(reader, 12);
        entities.ExistingHealthInsuranceViability.Populated = true;
        CheckValid<HealthInsuranceViability>("CroType",
          entities.ExistingHealthInsuranceViability.CroType);
      });
  }

  private IEnumerable<bool> ReadHinsViabNote()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingHealthInsuranceViability.Populated);

    return ReadEach("ReadHinsViabNote",
      (db, command) =>
      {
        db.SetInt32(
          command, "hivId",
          entities.ExistingHealthInsuranceViability.Identifier);
        db.SetString(
          command, "casNumber",
          entities.ExistingHealthInsuranceViability.CasNumber);
        db.SetInt32(
          command, "croId",
          entities.ExistingHealthInsuranceViability.CroIdentifier);
        db.SetString(
          command, "cspNumber",
          entities.ExistingHealthInsuranceViability.CspNumber);
        db.SetString(
          command, "croType",
          entities.ExistingHealthInsuranceViability.CroType);
      },
      (db, reader) =>
      {
        if (export.HicvNote.IsFull)
        {
          return false;
        }

        entities.ExistingHinsViabNote.Identifier = db.GetInt32(reader, 0);
        entities.ExistingHinsViabNote.Note = db.GetNullableString(reader, 1);
        entities.ExistingHinsViabNote.CroId = db.GetInt32(reader, 2);
        entities.ExistingHinsViabNote.CroType = db.GetString(reader, 3);
        entities.ExistingHinsViabNote.CspNumber = db.GetString(reader, 4);
        entities.ExistingHinsViabNote.CasNumber = db.GetString(reader, 5);
        entities.ExistingHinsViabNote.HivId = db.GetInt32(reader, 6);
        entities.ExistingHinsViabNote.Populated = true;
        CheckValid<HinsViabNote>("CroType",
          entities.ExistingHinsViabNote.CroType);

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
    /// <summary>A HicvNoteGroup group.</summary>
    [Serializable]
    public class HicvNoteGroup
    {
      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public HinsViabNote Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private HinsViabNote detail;
    }

    /// <summary>
    /// Gets a value of HicvNote.
    /// </summary>
    [JsonIgnore]
    public Array<HicvNoteGroup> HicvNote => hicvNote ??= new(
      HicvNoteGroup.Capacity);

    /// <summary>
    /// Gets a value of HicvNote for json serialization.
    /// </summary>
    [JsonPropertyName("hicvNote")]
    [Computed]
    public IList<HicvNoteGroup> HicvNote_Json
    {
      get => hicvNote;
      set => HicvNote.Assign(value);
    }

    /// <summary>
    /// A value of HealthInsuranceViability.
    /// </summary>
    [JsonPropertyName("healthInsuranceViability")]
    public HealthInsuranceViability HealthInsuranceViability
    {
      get => healthInsuranceViability ??= new();
      set => healthInsuranceViability = value;
    }

    /// <summary>
    /// A value of Child1.
    /// </summary>
    [JsonPropertyName("child1")]
    public CsePerson Child1
    {
      get => child1 ??= new();
      set => child1 = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of Child2.
    /// </summary>
    [JsonPropertyName("child2")]
    public CaseRole Child2
    {
      get => child2 ??= new();
      set => child2 = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    private Array<HicvNoteGroup> hicvNote;
    private HealthInsuranceViability healthInsuranceViability;
    private CsePerson child1;
    private Case1 case1;
    private CaseRole child2;
    private CsePerson ap;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A HicvNoteGroup group.</summary>
    [Serializable]
    public class HicvNoteGroup
    {
      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public HinsViabNote Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private HinsViabNote detail;
    }

    /// <summary>
    /// A value of HealthInsuranceViability.
    /// </summary>
    [JsonPropertyName("healthInsuranceViability")]
    public HealthInsuranceViability HealthInsuranceViability
    {
      get => healthInsuranceViability ??= new();
      set => healthInsuranceViability = value;
    }

    /// <summary>
    /// Gets a value of HicvNote.
    /// </summary>
    [JsonIgnore]
    public Array<HicvNoteGroup> HicvNote => hicvNote ??= new(
      HicvNoteGroup.Capacity);

    /// <summary>
    /// Gets a value of HicvNote for json serialization.
    /// </summary>
    [JsonPropertyName("hicvNote")]
    [Computed]
    public IList<HicvNoteGroup> HicvNote_Json
    {
      get => hicvNote;
      set => HicvNote.Assign(value);
    }

    private HealthInsuranceViability healthInsuranceViability;
    private Array<HicvNoteGroup> hicvNote;
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

    /// <summary>
    /// A value of InitialisedToBlanks.
    /// </summary>
    [JsonPropertyName("initialisedToBlanks")]
    public HealthInsuranceViability InitialisedToBlanks
    {
      get => initialisedToBlanks ??= new();
      set => initialisedToBlanks = value;
    }

    private DateWorkArea current;
    private HealthInsuranceViability initialisedToBlanks;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingHealthInsuranceViability.
    /// </summary>
    [JsonPropertyName("existingHealthInsuranceViability")]
    public HealthInsuranceViability ExistingHealthInsuranceViability
    {
      get => existingHealthInsuranceViability ??= new();
      set => existingHealthInsuranceViability = value;
    }

    /// <summary>
    /// A value of ExistingChild1.
    /// </summary>
    [JsonPropertyName("existingChild1")]
    public CaseRole ExistingChild1
    {
      get => existingChild1 ??= new();
      set => existingChild1 = value;
    }

    /// <summary>
    /// A value of ExistingCase.
    /// </summary>
    [JsonPropertyName("existingCase")]
    public Case1 ExistingCase
    {
      get => existingCase ??= new();
      set => existingCase = value;
    }

    /// <summary>
    /// A value of ExistingChild2.
    /// </summary>
    [JsonPropertyName("existingChild2")]
    public CsePerson ExistingChild2
    {
      get => existingChild2 ??= new();
      set => existingChild2 = value;
    }

    /// <summary>
    /// A value of ExistingHinsViabNote.
    /// </summary>
    [JsonPropertyName("existingHinsViabNote")]
    public HinsViabNote ExistingHinsViabNote
    {
      get => existingHinsViabNote ??= new();
      set => existingHinsViabNote = value;
    }

    /// <summary>
    /// A value of ExistingAp.
    /// </summary>
    [JsonPropertyName("existingAp")]
    public CsePerson ExistingAp
    {
      get => existingAp ??= new();
      set => existingAp = value;
    }

    /// <summary>
    /// A value of ExistingAbsentParent.
    /// </summary>
    [JsonPropertyName("existingAbsentParent")]
    public CaseRole ExistingAbsentParent
    {
      get => existingAbsentParent ??= new();
      set => existingAbsentParent = value;
    }

    private HealthInsuranceViability existingHealthInsuranceViability;
    private CaseRole existingChild1;
    private Case1 existingCase;
    private CsePerson existingChild2;
    private HinsViabNote existingHinsViabNote;
    private CsePerson existingAp;
    private CaseRole existingAbsentParent;
  }
#endregion
}
