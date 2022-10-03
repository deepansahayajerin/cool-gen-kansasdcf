// Program: SI_VERIFY_ONE_ACTIVE_CASE_ROLE, ID: 371785560, model: 746.
// Short name: SWE01267
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_VERIFY_ONE_ACTIVE_CASE_ROLE.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This AB checks for a current occurence of this case role within a case.  eg.
/// MO, FA or AR
/// </para>
/// </summary>
[Serializable]
public partial class SiVerifyOneActiveCaseRole: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_VERIFY_ONE_ACTIVE_CASE_ROLE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiVerifyOneActiveCaseRole(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiVerifyOneActiveCaseRole.
  /// </summary>
  public SiVerifyOneActiveCaseRole(IContext context, Import import,
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
    // -------------------------------------------------------------------
    // 		M A I N T E N A N C E   L O G
    // Date	Developer            Request #  Description
    // 08-25-95  Helen Sharland - MTW       0  Initial Dev
    // ---------------------------------------------------------------
    // 02/12/99 W.Campbell          Escape statement was
    //                              added to avoid extra reads.
    // -----------------------------------------------
    // 06/24/99 M.Lachowicz         Change property of READ EACH
    //                              to OPTIMIZE FOR 1 ROW.
    // -----------------------------------------------
    local.Common.Count = 0;

    if (ReadCaseRole())
    {
      ++local.Common.Count;

      // -----------------------------------------------
      // 02/12/99 W.Campbell - The following escape
      // statement was added to avoid extra reads.
      // -----------------------------------------------
    }

    if (local.Common.Count > 0)
    {
      export.Common.Flag = "Y";
    }
  }

  private bool ReadCaseRole()
  {
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRole",
      (db, command) =>
      {
        db.SetString(command, "type", import.CaseRole.Type1);
        db.SetNullableDate(
          command, "startDate", import.CaseRole.StartDate.GetValueOrDefault());
        db.SetString(command, "casNumber", import.Case1.Number);
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
    private CaseRole caseRole;
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
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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

    private Case1 case1;
    private CaseRole caseRole;
  }
#endregion
}
