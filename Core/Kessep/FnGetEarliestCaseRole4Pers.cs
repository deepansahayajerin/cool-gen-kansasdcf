// Program: FN_GET_EARLIEST_CASE_ROLE_4_PERS, ID: 371111052, model: 746.
// Short name: SWE02950
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_GET_EARLIEST_CASE_ROLE_4_PERS.
/// </summary>
[Serializable]
public partial class FnGetEarliestCaseRole4Pers: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_GET_EARLIEST_CASE_ROLE_4_PERS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnGetEarliestCaseRole4Pers(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnGetEarliestCaseRole4Pers.
  /// </summary>
  public FnGetEarliestCaseRole4Pers(IContext context, Import import,
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
    // -------------------------------------
    // Initial Version - 8/2001
    // -------------------------------------
    ReadCaseRole();
  }

  private bool ReadCaseRole()
  {
    export.Earliest.Populated = false;

    return Read("ReadCaseRole",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        export.Earliest.StartDate = db.GetNullableDate(reader, 0);
        export.Earliest.Populated = true;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Earliest.
    /// </summary>
    [JsonPropertyName("earliest")]
    public CaseRole Earliest
    {
      get => earliest ??= new();
      set => earliest = value;
    }

    private CaseRole earliest;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    private CsePerson csePerson;
    private CaseRole caseRole;
  }
#endregion
}
