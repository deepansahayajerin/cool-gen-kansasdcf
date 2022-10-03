// Program: SI_READ_CASE_ROLES_BY_CSE_PERSON, ID: 371759452, model: 746.
// Short name: SWE01205
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
/// A program: SI_READ_CASE_ROLES_BY_CSE_PERSON.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This action block reads all of the case roles and their associated cases for
/// a given CSE Person.
/// </para>
/// </summary>
[Serializable]
public partial class SiReadCaseRolesByCsePerson: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_READ_CASE_ROLES_BY_CSE_PERSON program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiReadCaseRolesByCsePerson(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiReadCaseRolesByCsePerson.
  /// </summary>
  public SiReadCaseRolesByCsePerson(IContext context, Import import,
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
    // ---------------------------------------------------------
    //           M A I N T E N A N C E   L O G
    //  Date	   Developer	Request #	Description
    // 3-4-95	Helen Sharland	        0	Initial Development
    // ---------------------------------------------------------
    local.ErrOnAdabasUnavailable.Flag = "Y";
    UseCabSetMaximumDiscontinueDate();
    export.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;

    if (ReadCsePerson())
    {
      export.CsePerson.Number = entities.CsePerson.Number;

      if (CharAt(entities.CsePerson.Number, 10) != 'O')
      {
        local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
        export.CsePersonsWorkSet.Number = entities.CsePerson.Number;
        UseCabReadAdabasPerson();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
        export.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);
        UseSiFormatCsePersonName();
        export.CsePersonsWorkSet.FormattedName =
          local.CsePersonsWorkSet.FormattedName;
      }
      else
      {
        export.CsePersonsWorkSet.FormattedName =
          entities.CsePerson.OrganizationName ?? Spaces(33);
      }
    }
    else
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    export.Export1.Index = -1;

    // ---------------------------------------------
    // Read all the case roles and the associated cases for the specified CSE 
    // Person.
    // ---------------------------------------------
    foreach(var item in ReadCaseCaseRole())
    {
      if (Lt(entities.Case1.Number, import.PageKeyCase.Number) && Equal
        (entities.CaseRole.EndDate, import.PageKeyCaseRole.EndDate))
      {
        continue;
      }

      ++export.Export1.Index;
      export.Export1.CheckSize();

      if (export.Export1.Index >= Export.ExportGroup.Capacity)
      {
        export.NextPageKeyCase.Number = entities.Case1.Number;
        export.NextPageKeyCaseRole.EndDate = entities.CaseRole.EndDate;

        break;
      }

      MoveCase1(entities.Case1, export.Export1.Update.DetailCase);
      export.Export1.Update.DetailCaseRole.Assign(entities.CaseRole);

      if (Equal(entities.CaseRole.EndDate, local.DateWorkArea.Date))
      {
        export.Export1.Update.DetailCaseRole.EndDate = null;
      }
    }

    // ---------------------------------------------
    // This piece of logic determines what the scrolling message will be.
    // ---------------------------------------------
    if (import.Standard.PageNumber == 1)
    {
      if (export.Export1.Index + 1 == Export.ExportGroup.Capacity)
      {
        export.Standard.ScrollingMessage = "MORE +";
      }
      else
      {
        export.Standard.ScrollingMessage = "";
      }
    }
    else if (export.Export1.Index + 1 == Export.ExportGroup.Capacity)
    {
      export.Standard.ScrollingMessage = "MORE - +";
    }
    else
    {
      export.Standard.ScrollingMessage = "MORE -";
    }
  }

  private static void MoveCase1(Case1 source, Case1 target)
  {
    target.Number = source.Number;
    target.Status = source.Status;
  }

  private void UseCabReadAdabasPerson()
  {
    var useImport = new CabReadAdabasPerson.Import();
    var useExport = new CabReadAdabasPerson.Export();

    useImport.ErrOnAdabasUnavailable.Flag = local.ErrOnAdabasUnavailable.Flag;
    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(CabReadAdabasPerson.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    export.AbendData.Assign(useExport.AbendData);
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.DateWorkArea.Date = useExport.DateWorkArea.Date;
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

  private IEnumerable<bool> ReadCaseCaseRole()
  {
    entities.CaseRole.Populated = false;
    entities.Case1.Populated = false;

    return ReadEach("ReadCaseCaseRole",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "endDate",
          import.PageKeyCaseRole.EndDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.CaseRole.CspNumber = db.GetString(reader, 2);
        entities.CaseRole.Type1 = db.GetString(reader, 3);
        entities.CaseRole.Identifier = db.GetInt32(reader, 4);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 6);
        entities.CaseRole.Populated = true;
        entities.Case1.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);

        return true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
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
    /// A value of PageKeyCaseRole.
    /// </summary>
    [JsonPropertyName("pageKeyCaseRole")]
    public CaseRole PageKeyCaseRole
    {
      get => pageKeyCaseRole ??= new();
      set => pageKeyCaseRole = value;
    }

    /// <summary>
    /// A value of PageKeyCase.
    /// </summary>
    [JsonPropertyName("pageKeyCase")]
    public Case1 PageKeyCase
    {
      get => pageKeyCase ??= new();
      set => pageKeyCase = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private Standard standard;
    private CaseRole pageKeyCaseRole;
    private Case1 pageKeyCase;
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
      /// A value of DetailCommon.
      /// </summary>
      [JsonPropertyName("detailCommon")]
      public Common DetailCommon
      {
        get => detailCommon ??= new();
        set => detailCommon = value;
      }

      /// <summary>
      /// A value of DetailCase.
      /// </summary>
      [JsonPropertyName("detailCase")]
      public Case1 DetailCase
      {
        get => detailCase ??= new();
        set => detailCase = value;
      }

      /// <summary>
      /// A value of DetailCaseRole.
      /// </summary>
      [JsonPropertyName("detailCaseRole")]
      public CaseRole DetailCaseRole
      {
        get => detailCaseRole ??= new();
        set => detailCaseRole = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 14;

      private Common detailCommon;
      private Case1 detailCase;
      private CaseRole detailCaseRole;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of NextPageKeyCaseRole.
    /// </summary>
    [JsonPropertyName("nextPageKeyCaseRole")]
    public CaseRole NextPageKeyCaseRole
    {
      get => nextPageKeyCaseRole ??= new();
      set => nextPageKeyCaseRole = value;
    }

    /// <summary>
    /// A value of NextPageKeyCase.
    /// </summary>
    [JsonPropertyName("nextPageKeyCase")]
    public Case1 NextPageKeyCase
    {
      get => nextPageKeyCase ??= new();
      set => nextPageKeyCase = value;
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

    private AbendData abendData;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePerson csePerson;
    private Standard standard;
    private CaseRole nextPageKeyCaseRole;
    private Case1 nextPageKeyCase;
    private Array<ExportGroup> export1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ErrOnAdabasUnavailable.
    /// </summary>
    [JsonPropertyName("errOnAdabasUnavailable")]
    public Common ErrOnAdabasUnavailable
    {
      get => errOnAdabasUnavailable ??= new();
      set => errOnAdabasUnavailable = value;
    }

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
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

    private Common errOnAdabasUnavailable;
    private DateWorkArea dateWorkArea;
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

    private CaseRole caseRole;
    private Case1 case1;
    private CsePerson csePerson;
  }
#endregion
}
