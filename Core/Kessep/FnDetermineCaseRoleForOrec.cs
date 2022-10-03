// Program: FN_DETERMINE_CASE_ROLE_FOR_OREC, ID: 372164427, model: 746.
// Short name: SWE00002
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
/// A program: FN_DETERMINE_CASE_ROLE_FOR_OREC.
/// </para>
/// <para>
/// Determines if a CSE_PERSON is an AP, an AR, or both for Address processing.
/// If the person is both, then for purposes of address processing, they will
/// be considered to be AP.  This will result in a Post Master Letter being
/// generated and the Verified_Date not being SET.
/// </para>
/// </summary>
[Serializable]
public partial class FnDetermineCaseRoleForOrec: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DETERMINE_CASE_ROLE_FOR_OREC program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDetermineCaseRoleForOrec(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDetermineCaseRoleForOrec.
  /// </summary>
  public FnDetermineCaseRoleForOrec(IContext context, Import import,
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
    // =================================================
    // 1/26/99 - Bud Adams  -  Created
    // =================================================
    // =================================================
    // 1/26/99 - B Adams  -  For the supplied CSE_Person Number
    //   return the value of their active Case_Role Type, regardless
    //   of the Case.  If the person has more than one Case_Role
    //   Type in KESSEP, then return the value of AP.
    // =================================================
    foreach(var item in ReadCaseRoleCsePersonCase())
    {
      if (IsEmpty(export.CaseRole.Type1))
      {
        export.CaseRole.Type1 = entities.CaseRole.Type1;
      }

      if (Equal(export.CaseRole.Type1, entities.CaseRole.Type1))
      {
      }
      else
      {
        export.CaseRole.Type1 = "AP";

        return;
      }
    }
  }

  private IEnumerable<bool> ReadCaseRoleCsePersonCase()
  {
    entities.Case1.Populated = false;
    entities.CsePerson.Populated = false;
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCaseRoleCsePersonCase",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.Obligor.Number);
        db.SetNullableDate(
          command, "startDate", import.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.Case1.ClosureReason = db.GetNullableString(reader, 6);
        entities.Case1.Populated = true;
        entities.CsePerson.Populated = true;
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);

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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    private DateWorkArea current;
    private CsePerson obligor;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    private CaseRole caseRole;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    private Case1 case1;
    private CsePerson csePerson;
    private CaseRole caseRole;
  }
#endregion
}
