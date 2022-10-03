// Program: SI_GET_ACTIVE_APS_FOR_CASE, ID: 372540168, model: 746.
// Short name: SWE02557
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_GET_ACTIVE_APS_FOR_CASE.
/// </summary>
[Serializable]
public partial class SiGetActiveApsForCase: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_GET_ACTIVE_APS_FOR_CASE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiGetActiveApsForCase(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiGetActiveApsForCase.
  /// </summary>
  public SiGetActiveApsForCase(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------
    // 		M A I N T E N A N C E   L O G
    //  Date    Developer         Description
    // 04-07-99 W.Campbell        Initial Development.
    // -------------------------------------------------------------------
    // -------------------------------------------------------------------
    // The purpose of this CAB is to READ EACH
    // of the active AP(s) on the import Case
    // number and return them to the USEing
    // Action Block in the Export repeating group view.
    // -------------------------------------------------------------------
    local.Current.Date = Now().Date;
    export.Grp.Count = 0;
    export.Grp.Index = -1;

    foreach(var item in ReadCsePerson())
    {
      ++export.Grp.Index;
      export.Grp.CheckSize();

      if (export.Grp.Index >= Export.GrpGroup.Capacity)
      {
        ExitState = "NUMBER_OF_APS_EXCEED_MAX";

        return;
      }

      export.Grp.Update.Ap.Number = entities.CsePerson.Number;
    }
  }

  private IEnumerable<bool> ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePerson",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;

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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    private Case1 case1;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A GrpGroup group.</summary>
    [Serializable]
    public class GrpGroup
    {
      /// <summary>
      /// A value of Ap.
      /// </summary>
      [JsonPropertyName("ap")]
      public CsePerson Ap
      {
        get => ap ??= new();
        set => ap = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 30;

      private CsePerson ap;
    }

    /// <summary>
    /// Gets a value of Grp.
    /// </summary>
    [JsonIgnore]
    public Array<GrpGroup> Grp => grp ??= new(GrpGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Grp for json serialization.
    /// </summary>
    [JsonPropertyName("grp")]
    [Computed]
    public IList<GrpGroup> Grp_Json
    {
      get => grp;
      set => Grp.Assign(value);
    }

    private Array<GrpGroup> grp;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    private CaseRole caseRole;
    private CsePerson csePerson;
    private Case1 case1;
  }
#endregion
}
