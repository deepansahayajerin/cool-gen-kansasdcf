// Program: SI_CHECK_CASE_ROLE_COMBINATIONS, ID: 371785562, model: 746.
// Short name: SWE01116
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_CHECK_CASE_ROLE_COMBINATIONS.
/// </para>
/// <para>
/// RESP: SRVINT
/// This action block checks that a person doesn't have an invalid combination 
/// of roles on a case.
/// </para>
/// </summary>
[Serializable]
public partial class SiCheckCaseRoleCombinations: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CHECK_CASE_ROLE_COMBINATIONS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCheckCaseRoleCombinations(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCheckCaseRoleCombinations.
  /// </summary>
  public SiCheckCaseRoleCombinations(IContext context, Import import,
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
    // -----------------------------------------------------------
    //                 M A I N T E N A N C E   L O G
    //   Date      Developer       Request #       Description
    // 09/02/95  Helen Sharland                   Initial Development
    // ----------------------------------------------------------
    if (ReadCaseRole())
    {
      export.Common.Flag = "Y";
    }
    else
    {
      // -----------------------------------------------
      // No conflict of roles
      // -----------------------------------------------
    }
  }

  private bool ReadCaseRole()
  {
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRole",
      (db, command) =>
      {
        db.SetString(command, "casNumber", import.Case1.Number);
        db.SetString(command, "type", import.Verify.Type1);
        db.SetNullableDate(
          command, "startDate", import.New1.StartDate.GetValueOrDefault());
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
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
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public CaseRole New1
    {
      get => new1 ??= new();
      set => new1 = value;
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

    /// <summary>
    /// A value of Verify.
    /// </summary>
    [JsonPropertyName("verify")]
    public CaseRole Verify
    {
      get => verify ??= new();
      set => verify = value;
    }

    private CaseRole new1;
    private CsePerson csePerson;
    private Case1 case1;
    private CaseRole verify;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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

    private Common common;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    private Case1 case1;
    private CaseRole caseRole;
  }
#endregion
}
