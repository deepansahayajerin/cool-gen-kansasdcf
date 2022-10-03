// Program: SI_VERIFY_NOT_LAST_CH_ON_CASE, ID: 371785567, model: 746.
// Short name: SWE01949
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
/// A program: SI_VERIFY_NOT_LAST_CH_ON_CASE.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class SiVerifyNotLastChOnCase: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_VERIFY_NOT_LAST_CH_ON_CASE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiVerifyNotLastChOnCase(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiVerifyNotLastChOnCase.
  /// </summary>
  public SiVerifyNotLastChOnCase(IContext context, Import import, Export export):
    
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
    // 		M A I N T E N A N C E   L O G
    // Date	  Developer		Description
    // 03/15/97  G. Lofton - MTW	Initial development.
    // 04/29/97  JeHoward - DIR        Current date fix.
    // ------------------------------------------------------------
    local.Current.Date = Now().Date;
    export.OtherChOnCase.Flag = "N";
    export.ChOnCase.Flag = "N";

    foreach(var item in ReadCaseRoleCsePerson())
    {
      ++export.ChCnt.Count;

      if (Equal(entities.CsePerson.Number, import.CsePerson.Number))
      {
        export.ChOnCase.Flag = "Y";
      }
      else
      {
        export.OtherChOnCase.Flag = "Y";
      }
    }
  }

  private IEnumerable<bool> ReadCaseRoleCsePerson()
  {
    entities.CsePerson.Populated = false;
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCaseRoleCsePerson",
      (db, command) =>
      {
        db.SetString(command, "casNumber", import.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private Case1 case1;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ChOnCase.
    /// </summary>
    [JsonPropertyName("chOnCase")]
    public Common ChOnCase
    {
      get => chOnCase ??= new();
      set => chOnCase = value;
    }

    /// <summary>
    /// A value of OtherChOnCase.
    /// </summary>
    [JsonPropertyName("otherChOnCase")]
    public Common OtherChOnCase
    {
      get => otherChOnCase ??= new();
      set => otherChOnCase = value;
    }

    /// <summary>
    /// A value of ChCnt.
    /// </summary>
    [JsonPropertyName("chCnt")]
    public Common ChCnt
    {
      get => chCnt ??= new();
      set => chCnt = value;
    }

    private Common chOnCase;
    private Common otherChOnCase;
    private Common chCnt;
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
