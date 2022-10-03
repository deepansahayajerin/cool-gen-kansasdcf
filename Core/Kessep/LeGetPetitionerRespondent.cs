// Program: LE_GET_PETITIONER_RESPONDENT, ID: 371973994, model: 746.
// Short name: SWE00779
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_GET_PETITIONER_RESPONDENT.
/// </summary>
[Serializable]
public partial class LeGetPetitionerRespondent: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_GET_PETITIONER_RESPONDENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeGetPetitionerRespondent(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeGetPetitionerRespondent.
  /// </summary>
  public LeGetPetitionerRespondent(IContext context, Import import,
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
    // ********************************************
    // 11/19/97  R Grey	H00032603 Non CSE Petitioner display as Petitioner (
    // overwrite with petitioner if exists)
    // ********************************************
    local.Current.Date = Now().Date;
    local.Zero.EndDate = null;
    local.PetitionerRespondentCommon.Count = 0;
    export.PetitionerRespondentDetails.MorePetitionerInd = "";

    if (ReadLegalAction())
    {
      if (!IsEmpty(entities.LegalAction.NonCsePetitioner))
      {
        export.PetitionerRespondentDetails.PetitionerName =
          entities.LegalAction.NonCsePetitioner ?? Spaces(33);
        local.NonCsePetitionerMoved.Flag = "Y";
      }
    }
    else
    {
      ExitState = "ZD_LEGAL_ACTION_NF_2";

      return;
    }

    foreach(var item in ReadLegalActionPersonCsePerson1())
    {
      ++local.PetitionerRespondentCommon.Count;

      if (local.PetitionerRespondentCommon.Count == 1)
      {
        local.PetitionerRespondentCsePerson.Number = entities.CsePerson.Number;
      }

      if (local.PetitionerRespondentCommon.Count > 1)
      {
        export.PetitionerRespondentDetails.MorePetitionerInd = "+";

        break;
      }
    }

    if (local.PetitionerRespondentCommon.Count > 0)
    {
      // ------------------------------------------------------------
      // Read Person ADABASE table using External Action Block.
      // ------------------------------------------------------------
      local.PetitionerRespondentCsePersonsWorkSet.Number =
        local.PetitionerRespondentCsePerson.Number;
      UseSiReadCsePerson();
      export.PetitionerRespondentDetails.PetitionerName =
        local.PetitionerRespondentCsePersonsWorkSet.FormattedName;
    }

    local.PetitionerRespondentCommon.Count = 0;
    export.PetitionerRespondentDetails.MoreRespondentInd = "";

    foreach(var item in ReadLegalActionPersonCsePerson2())
    {
      ++local.PetitionerRespondentCommon.Count;

      if (local.PetitionerRespondentCommon.Count == 1)
      {
        local.PetitionerRespondentCsePerson.Number = entities.CsePerson.Number;
      }

      if (local.PetitionerRespondentCommon.Count > 1)
      {
        export.PetitionerRespondentDetails.MoreRespondentInd = "+";

        break;
      }
    }

    if (local.PetitionerRespondentCommon.Count > 0)
    {
      // ------------------------------------------------------------
      // Read Person ADABASE table using External Action Block.
      // ------------------------------------------------------------
      local.PetitionerRespondentCsePersonsWorkSet.Number =
        local.PetitionerRespondentCsePerson.Number;
      UseSiReadCsePerson();
      export.PetitionerRespondentDetails.RespondentName =
        local.PetitionerRespondentCsePersonsWorkSet.FormattedName;
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number =
      local.PetitionerRespondentCsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet,
      local.PetitionerRespondentCsePersonsWorkSet);
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
        entities.LegalAction.NonCsePetitioner = db.GetNullableString(reader, 1);
        entities.LegalAction.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalActionPersonCsePerson1()
  {
    entities.CsePerson.Populated = false;
    entities.LegalActionPerson.Populated = false;

    return ReadEach("ReadLegalActionPersonCsePerson1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", import.LegalAction.Identifier);
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDt", local.Zero.EndDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.LegalActionPerson.LgaIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.LegalActionPerson.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionPerson.Role = db.GetString(reader, 4);
        entities.LegalActionPerson.EndDate = db.GetNullableDate(reader, 5);
        entities.CsePerson.Type1 = db.GetString(reader, 6);
        entities.CsePerson.Populated = true;
        entities.LegalActionPerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionPersonCsePerson2()
  {
    entities.CsePerson.Populated = false;
    entities.LegalActionPerson.Populated = false;

    return ReadEach("ReadLegalActionPersonCsePerson2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", import.LegalAction.Identifier);
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDt", local.Zero.EndDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.LegalActionPerson.LgaIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.LegalActionPerson.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionPerson.Role = db.GetString(reader, 4);
        entities.LegalActionPerson.EndDate = db.GetNullableDate(reader, 5);
        entities.CsePerson.Type1 = db.GetString(reader, 6);
        entities.CsePerson.Populated = true;
        entities.LegalActionPerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

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
    /// <summary>
    /// A value of PetitionerRespondentDetails.
    /// </summary>
    [JsonPropertyName("petitionerRespondentDetails")]
    public PetitionerRespondentDetails PetitionerRespondentDetails
    {
      get => petitionerRespondentDetails ??= new();
      set => petitionerRespondentDetails = value;
    }

    private PetitionerRespondentDetails petitionerRespondentDetails;
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
    /// A value of NonCsePetitionerMoved.
    /// </summary>
    [JsonPropertyName("nonCsePetitionerMoved")]
    public Common NonCsePetitionerMoved
    {
      get => nonCsePetitionerMoved ??= new();
      set => nonCsePetitionerMoved = value;
    }

    /// <summary>
    /// A value of PetitionerRespondentCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("petitionerRespondentCsePersonsWorkSet")]
    public CsePersonsWorkSet PetitionerRespondentCsePersonsWorkSet
    {
      get => petitionerRespondentCsePersonsWorkSet ??= new();
      set => petitionerRespondentCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of PetitionerRespondentCsePerson.
    /// </summary>
    [JsonPropertyName("petitionerRespondentCsePerson")]
    public CsePerson PetitionerRespondentCsePerson
    {
      get => petitionerRespondentCsePerson ??= new();
      set => petitionerRespondentCsePerson = value;
    }

    /// <summary>
    /// A value of Respondent.
    /// </summary>
    [JsonPropertyName("respondent")]
    public CsePerson Respondent
    {
      get => respondent ??= new();
      set => respondent = value;
    }

    /// <summary>
    /// A value of PetitionerRespondentCommon.
    /// </summary>
    [JsonPropertyName("petitionerRespondentCommon")]
    public Common PetitionerRespondentCommon
    {
      get => petitionerRespondentCommon ??= new();
      set => petitionerRespondentCommon = value;
    }

    /// <summary>
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public LegalActionPerson Zero
    {
      get => zero ??= new();
      set => zero = value;
    }

    private DateWorkArea current;
    private Common nonCsePetitionerMoved;
    private CsePersonsWorkSet petitionerRespondentCsePersonsWorkSet;
    private CsePerson petitionerRespondentCsePerson;
    private CsePerson respondent;
    private Common petitionerRespondentCommon;
    private LegalActionPerson zero;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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

    private CsePerson csePerson;
    private LegalActionPerson legalActionPerson;
    private LegalAction legalAction;
  }
#endregion
}
