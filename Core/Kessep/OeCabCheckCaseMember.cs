// Program: OE_CAB_CHECK_CASE_MEMBER, ID: 371794504, model: 746.
// Short name: SWE00880
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_CAB_CHECK_CASE_MEMBER.
/// </para>
/// <para>
/// RESP: OBLGESTB.
/// WRITTEN BY:SID.
/// </para>
/// </summary>
[Serializable]
public partial class OeCabCheckCaseMember: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_CAB_CHECK_CASE_MEMBER program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeCabCheckCaseMember(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeCabCheckCaseMember.
  /// </summary>
  public OeCabCheckCaseMember(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------
    // 01/01/95      Sid       Creation.
    // ---------------------------------------------
    // *********************************************
    // It validates that a CSE Person is present on
    // a Case.(see desc).
    // *********************************************
    export.Work.Flag = "";
    export.CaseOpen.Flag = "";
    export.CaseRoleInactive.Flag = "";
    local.Current.Date = Now().Date;
    export.CsePerson.Number = import.CsePerson.Number;

    if (!IsEmpty(import.Case1.Number))
    {
      if (ReadCase())
      {
        if (AsChar(entities.Case1.Status) == 'O')
        {
          export.CaseOpen.Flag = "Y";
        }
        else if (AsChar(entities.Case1.Status) == 'C')
        {
          export.CaseOpen.Flag = "N";
        }
      }
      else
      {
        export.Work.Flag = "C";

        return;
      }
    }

    if (!IsEmpty(import.CsePerson.Number))
    {
      if (ReadCsePerson())
      {
        export.CsePerson.Number = entities.CsePerson.Number;
        export.CsePersonsWorkSet.Number = export.CsePerson.Number;
      }
      else
      {
        export.Work.Flag = "P";
        export.WorkName.FormattedNameText = "";

        return;
      }

      UseSiReadCsePerson();
      export.WorkName.FormattedNameText =
        export.CsePersonsWorkSet.FormattedName;
    }

    if (!IsEmpty(import.CsePerson.Number) && !IsEmpty(import.Case1.Number))
    {
      if (AsChar(export.CaseOpen.Flag) == 'Y')
      {
        if (ReadCaseRole1())
        {
          export.CaseRoleInactive.Flag = "N";
        }
        else if (ReadCaseRole3())
        {
          export.CaseRoleInactive.Flag = "Y";
        }
        else
        {
          export.Work.Flag = "R";
        }
      }
      else if (AsChar(export.CaseOpen.Flag) == 'N')
      {
        if (!ReadCaseRole2())
        {
          export.Work.Flag = "R";
        }
      }
    }
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseRole1()
  {
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRole1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.HealthInsuranceIndicator =
          db.GetNullableString(reader, 6);
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
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "endDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.HealthInsuranceIndicator =
          db.GetNullableString(reader, 6);
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private bool ReadCaseRole3()
  {
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRole3",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.HealthInsuranceIndicator =
          db.GetNullableString(reader, 6);
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
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
    /// A value of WorkName.
    /// </summary>
    [JsonPropertyName("workName")]
    public OeWorkGroup WorkName
    {
      get => workName ??= new();
      set => workName = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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
    /// A value of Work.
    /// </summary>
    [JsonPropertyName("work")]
    public Common Work
    {
      get => work ??= new();
      set => work = value;
    }

    /// <summary>
    /// A value of CaseOpen.
    /// </summary>
    [JsonPropertyName("caseOpen")]
    public Common CaseOpen
    {
      get => caseOpen ??= new();
      set => caseOpen = value;
    }

    /// <summary>
    /// A value of CaseRoleInactive.
    /// </summary>
    [JsonPropertyName("caseRoleInactive")]
    public Common CaseRoleInactive
    {
      get => caseRoleInactive ??= new();
      set => caseRoleInactive = value;
    }

    private OeWorkGroup workName;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePerson csePerson;
    private Common work;
    private Common caseOpen;
    private Common caseRoleInactive;
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
