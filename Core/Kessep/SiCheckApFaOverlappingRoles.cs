// Program: SI_CHECK_AP_FA_OVERLAPPING_ROLES, ID: 373333478, model: 746.
// Short name: SWE00376
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_CHECK_AP_FA_OVERLAPPING_ROLES.
/// </summary>
[Serializable]
public partial class SiCheckApFaOverlappingRoles: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CHECK_AP_FA_OVERLAPPING_ROLES program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCheckApFaOverlappingRoles(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCheckApFaOverlappingRoles.
  /// </summary>
  public SiCheckApFaOverlappingRoles(IContext context, Import import,
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
    foreach(var item in ReadCaseRoleCsePerson())
    {
      local.CsePersonsWorkSet.Number = entities.ApCsePerson.Number;
      UseCabReadAdabasPerson();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      if (AsChar(local.CsePersonsWorkSet.Sex) == 'M')
      {
        ExitState = "SI0000_OVERLAPPING_FA_AND_AP";

        return;
      }
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Sex = source.Sex;
  }

  private void UseCabReadAdabasPerson()
  {
    var useImport = new CabReadAdabasPerson.Import();
    var useExport = new CabReadAdabasPerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(CabReadAdabasPerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
  }

  private IEnumerable<bool> ReadCaseRoleCsePerson()
  {
    entities.ApCaseRole.Populated = false;
    entities.ApCsePerson.Populated = false;

    return ReadEach("ReadCaseRoleCsePerson",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate",
          import.FaCaseRole.StartDate.GetValueOrDefault());
        db.SetString(command, "casNumber", import.Case1.Number);
        db.SetString(command, "cspNumber", import.FaCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.ApCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ApCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ApCsePerson.Number = db.GetString(reader, 1);
        entities.ApCaseRole.Type1 = db.GetString(reader, 2);
        entities.ApCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ApCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ApCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ApCaseRole.Populated = true;
        entities.ApCsePerson.Populated = true;

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
    /// A value of FaCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("faCsePersonsWorkSet")]
    public CsePersonsWorkSet FaCsePersonsWorkSet
    {
      get => faCsePersonsWorkSet ??= new();
      set => faCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of FaCaseRole.
    /// </summary>
    [JsonPropertyName("faCaseRole")]
    public CaseRole FaCaseRole
    {
      get => faCaseRole ??= new();
      set => faCaseRole = value;
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

    private CsePersonsWorkSet faCsePersonsWorkSet;
    private CaseRole faCaseRole;
    private Case1 case1;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ApCaseRole.
    /// </summary>
    [JsonPropertyName("apCaseRole")]
    public CaseRole ApCaseRole
    {
      get => apCaseRole ??= new();
      set => apCaseRole = value;
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
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
    }

    private CaseRole apCaseRole;
    private Case1 case1;
    private CsePerson apCsePerson;
  }
#endregion
}
