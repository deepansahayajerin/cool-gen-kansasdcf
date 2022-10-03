// Program: SI_READ_CASES_BY_PERSON, ID: 371762759, model: 746.
// Short name: SWE01206
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
/// A program: SI_READ_CASES_BY_PERSON.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This AB reads the persons name and the cases that he appears on.
/// </para>
/// </summary>
[Serializable]
public partial class SiReadCasesByPerson: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_READ_CASES_BY_PERSON program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiReadCasesByPerson(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiReadCasesByPerson.
  /// </summary>
  public SiReadCasesByPerson(IContext context, Import import, Export export):
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
    // Date	  Developer		Request #  Description
    // 09-13-95  Helen Sharland - MTW		0  Initial Dev
    // 04/29/97  JeHoward - DIR                   Current date fix
    // -------------------------------------------------------------------
    // 07/09/99  Marek Lachowicz	Change property of READ (Select Only)
    // 08/03/99  Marek Lachowicz	Added code to check if
    //                                 
    // CAB_READ_ADABAS_PERSON was
    // executed
    //                                 
    // succssfully.
    local.Current.Date = Now().Date;
    export.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;

    if (CharAt(import.CsePersonsWorkSet.Number, 10) == 'O')
    {
      // 07/09/99  Changed property of READ (Select Only)
      if (ReadCsePerson1())
      {
        export.CsePersonsWorkSet.FormattedName =
          entities.CsePerson.OrganizationName ?? Spaces(33);
        export.CsePersonsWorkSet.Number = entities.CsePerson.Number;
      }
      else
      {
        ExitState = "CSE_PERSON_NF";

        return;
      }
    }
    else
    {
      // 07/09/99  Changed property of READ (Select Only)
      if (ReadCsePerson2())
      {
        export.CsePerson.Assign(entities.CsePerson);
        export.CsePersonsWorkSet.Number = entities.CsePerson.Number;
      }
      else
      {
        ExitState = "CSE_PERSON_NF";

        return;
      }

      UseCabReadAdabasPerson();

      if (!IsEmpty(export.AbendData.Type1))
      {
        return;
      }

      // 08/03/99 M.L  Start
      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      // 08/03/99 M.L  End
      if (Equal(local.CsePersonsWorkSet.Ssn, "000000000"))
      {
        local.CsePersonsWorkSet.Ssn = "";
      }

      export.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);
      UseSiFormatCsePersonName();
      export.CsePersonsWorkSet.FormattedName =
        local.CsePersonsWorkSet.FormattedName;
    }

    export.Export1.Index = -1;

    foreach(var item in ReadCaseRoleCase())
    {
      ++export.Export1.Index;
      export.Export1.CheckSize();

      if (export.Export1.Index >= Export.ExportGroup.Capacity)
      {
        export.Page.Number = entities.Case1.Number;

        break;
      }

      export.Export1.Update.Detail.Number = entities.Case1.Number;
    }

    if (import.Standard.PageNumber == 1)
    {
      if (export.Export1.Index >= Export.ExportGroup.Capacity)
      {
        export.Standard.ScrollingMessage = "MORE +";
      }
      else
      {
        export.Standard.ScrollingMessage = "";
      }
    }
    else if (export.Export1.Index >= Export.ExportGroup.Capacity)
    {
      export.Standard.ScrollingMessage = "MORE - +";
    }
    else
    {
      export.Standard.ScrollingMessage = "MORE -";
    }
  }

  private void UseCabReadAdabasPerson()
  {
    var useImport = new CabReadAdabasPerson.Import();
    var useExport = new CabReadAdabasPerson.Export();

    useImport.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;

    Call(CabReadAdabasPerson.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    export.AbendData.Assign(useExport.AbendData);
  }

  private void UseSiFormatCsePersonName()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    useImport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    local.CsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private IEnumerable<bool> ReadCaseRoleCase()
  {
    entities.CaseRole.Populated = false;
    entities.Case1.Populated = false;

    return ReadEach("ReadCaseRoleCase",
      (db, command) =>
      {
        db.SetString(command, "casNumber", import.Page.Number);
        db.SetNullableDate(
          command, "endDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", export.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.Populated = true;
        entities.Case1.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);

        return true;
      });
  }

  private bool ReadCsePerson1()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.UnemploymentInd = db.GetNullableString(reader, 3);
        entities.CsePerson.FederalInd = db.GetNullableString(reader, 4);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePerson2()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.UnemploymentInd = db.GetNullableString(reader, 3);
        entities.CsePerson.FederalInd = db.GetNullableString(reader, 4);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of Page.
    /// </summary>
    [JsonPropertyName("page")]
    public Case1 Page
    {
      get => page ??= new();
      set => page = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private Standard standard;
    private Case1 page;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public Case1 Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private Case1 detail;
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
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
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
    /// A value of Page.
    /// </summary>
    [JsonPropertyName("page")]
    public Case1 Page
    {
      get => page ??= new();
      set => page = value;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 =>
      export1 ??= new(ExportGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Export1 for json serialization.
    /// </summary>
    [JsonPropertyName("export1")]
    [Computed]
    public IList<ExportGroup> Export1_Json
    {
      get => export1;
      set => Export1.Assign(value);
    }

    private CsePerson csePerson;
    private AbendData abendData;
    private Standard standard;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Case1 page;
    private Array<ExportGroup> export1;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    private DateWorkArea current;
    private Case1 case1;
    private CsePersonsWorkSet csePersonsWorkSet;
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
