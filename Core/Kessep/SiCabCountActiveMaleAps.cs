// Program: SI_CAB_COUNT_ACTIVE_MALE_APS, ID: 374380933, model: 746.
// Short name: SWE02869
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_CAB_COUNT_ACTIVE_MALE_APS.
/// </summary>
[Serializable]
public partial class SiCabCountActiveMaleAps: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CAB_COUNT_ACTIVE_MALE_APS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCabCountActiveMaleAps(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCabCountActiveMaleAps.
  /// </summary>
  public SiCabCountActiveMaleAps(IContext context, Import import, Export export):
    
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
    //                 M A I N T E N A N C E   L O G
    //   Date	  Developer	    Description
    // 03/16/00  C. Ott            Initial Development - This action block is 
    // used to count the number of active male APs on each case where the
    // imported person plays a child role.
    // ------------------------------------------------------------
    if (Equal(import.Current.Date, null))
    {
      local.Current.Date = Now().Date;
    }
    else
    {
      local.Current.Date = import.Current.Date;
    }

    foreach(var item in ReadCase())
    {
      export.ActiveApOnCase.Count = 0;

      foreach(var item1 in ReadCaseRoleCsePerson())
      {
        local.ActiveAp.Number = entities.ApCsePerson.Number;
        UseCabReadAdabasPerson();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (AsChar(local.ActiveAp.Sex) == 'M')
          {
            ++export.ActiveApOnCase.Count;
          }
        }
        else
        {
          return;
        }
      }

      if (export.ActiveApOnCase.Count > 1)
      {
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

    useImport.CsePersonsWorkSet.Number = local.ActiveAp.Number;

    Call(CabReadAdabasPerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.ActiveAp);
  }

  private IEnumerable<bool> ReadCase()
  {
    entities.ApCase.Populated = false;

    return ReadEach("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.Child.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ApCase.Number = db.GetString(reader, 0);
        entities.ApCase.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseRoleCsePerson()
  {
    entities.ApCsePerson.Populated = false;
    entities.ApCaseRole.Populated = false;

    return ReadEach("ReadCaseRoleCsePerson",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.ApCase.Number);
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
        entities.ApCsePerson.Populated = true;
        entities.ApCaseRole.Populated = true;

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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CsePerson Child
    {
      get => child ??= new();
      set => child = value;
    }

    private DateWorkArea current;
    private CsePerson child;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ActiveApOnCase.
    /// </summary>
    [JsonPropertyName("activeApOnCase")]
    public Common ActiveApOnCase
    {
      get => activeApOnCase ??= new();
      set => activeApOnCase = value;
    }

    private Common activeApOnCase;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ActiveAp.
    /// </summary>
    [JsonPropertyName("activeAp")]
    public CsePersonsWorkSet ActiveAp
    {
      get => activeAp ??= new();
      set => activeAp = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    private CsePersonsWorkSet activeAp;
    private DateWorkArea current;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
    }

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
    /// A value of ApCase.
    /// </summary>
    [JsonPropertyName("apCase")]
    public Case1 ApCase
    {
      get => apCase ??= new();
      set => apCase = value;
    }

    /// <summary>
    /// A value of ChCaseRole.
    /// </summary>
    [JsonPropertyName("chCaseRole")]
    public CaseRole ChCaseRole
    {
      get => chCaseRole ??= new();
      set => chCaseRole = value;
    }

    /// <summary>
    /// A value of ChCsePerson.
    /// </summary>
    [JsonPropertyName("chCsePerson")]
    public CsePerson ChCsePerson
    {
      get => chCsePerson ??= new();
      set => chCsePerson = value;
    }

    private CsePerson apCsePerson;
    private CaseRole apCaseRole;
    private Case1 apCase;
    private CaseRole chCaseRole;
    private CsePerson chCsePerson;
  }
#endregion
}
