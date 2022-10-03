// Program: SI_PADS_VALIDATE_ROLE_OTHER_CASE, ID: 371758853, model: 746.
// Short name: SWE01261
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_PADS_VALIDATE_ROLE_OTHER_CASE.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class SiPadsValidateRoleOtherCase: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_PADS_VALIDATE_ROLE_OTHER_CASE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiPadsValidateRoleOtherCase(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiPadsValidateRoleOtherCase.
  /// </summary>
  public SiPadsValidateRoleOtherCase(IContext context, Import import,
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
    // **********************************************************
    // MAINTENANCE LOG:
    // 04/30/97  JF.Caillouet		Change Current Date
    // 06/27/97  Sid Chowdhary		Modify Read.
    // **********************************************************
    // 06/22/99 W.Campbell             Modified the properties
    //                                 
    // of 2 READ statements to
    //                                 
    // Select Only.
    // ---------------------------------------------------------
    // 06/30/99 W.Campbell             Modified the properties
    //                                 
    // of 2 READ statements to
    //                                 
    // RESET them back to the
    //                                 
    // default of both Select and
    //                                 
    // Cursor, as it is OK for more
    //                                 
    // than one row to satisfy
    //                                 
    // these READs  - we do not
    //                                 
    // want the -811 SQL error.
    // ---------------------------------------------------------
    // ************************************************************
    // Check to see if the parent is an AP/AR on another case
    // ************************************************************
    local.Current.Date = Now().Date;

    // ---------------------------------------------------------
    // 06/22/99 W.Campbell - Modified the properties
    // of the following READ statement to Select Only.
    // ---------------------------------------------------------
    // ---------------------------------------------------------
    // 06/30/99 W.Campbell - Modified the properties
    // of the following READ statement to RESET it
    // back to the default of both Select and Cursor as
    // it is OK for more than one row to satisfy the
    // following READ  - we do not want the
    // -811 SQL error.
    // ---------------------------------------------------------
    if (ReadCaseRole1())
    {
      export.Common.Flag = "Y";

      return;
    }
    else
    {
      export.Common.Flag = "N";
    }

    // ---------------------------------------------------------
    // 06/22/99 W.Campbell - Modified the properties
    // of the following READ statement to Select Only.
    // ---------------------------------------------------------
    // ---------------------------------------------------------
    // 06/30/99 W.Campbell - Modified the properties
    // of the following READ statement to RESET it
    // back to the default of both Select and Cursor as
    // it is OK for more than one row to satisfy the
    // following READ  - we do not want the
    // -811 SQL error.
    // ---------------------------------------------------------
    if (ReadCaseRole2())
    {
      export.Common.Flag = "Y";
    }
    else
    {
      export.Common.Flag = "N";
    }
  }

  private bool ReadCaseRole1()
  {
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRole1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", import.CsePerson.Number);
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

  private bool ReadCaseRole2()
  {
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRole2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", import.CsePerson.Number);
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

    private CsePerson csePerson;
    private Case1 case1;
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
