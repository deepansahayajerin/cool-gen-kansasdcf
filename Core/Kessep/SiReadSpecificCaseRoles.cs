// Program: SI_READ_SPECIFIC_CASE_ROLES, ID: 371734863, model: 746.
// Short name: SWE01234
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
/// A program: SI_READ_SPECIFIC_CASE_ROLES.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This action block reads all people of the specified case role type for a 
/// given case.
/// </para>
/// </summary>
[Serializable]
public partial class SiReadSpecificCaseRoles: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_READ_SPECIFIC_CASE_ROLES program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiReadSpecificCaseRoles(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiReadSpecificCaseRoles.
  /// </summary>
  public SiReadSpecificCaseRoles(IContext context, Import import, Export export):
    
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
    // 3-6-95	Helen Sharland	        0	Initial Development
    // 4-4-97  Sid Chowdhary	Add a new group view to check for duplicates.
    // ---------------------------------------------------------
    // 05/06/99 W.Campbell        Fixed subscript
    //                            out of range (view overflow) problem.
    //                            group local_unique view subscript was
    //                            being incremented incorrectly.  Bad
    //                            statement disabled and replaced with
    //                            new set statement for subscript.  Also,
    //                            local group views were increased from
    //                            30 to 40.  Added check for subscript
    //                            greater than max.
    // -------------------------------------------------
    local.ErrOnAdabasUnavailable.Flag = "Y";
    local.SuppressZeroDob.Flag = "Y";
    export.Export1.Count = 0;
    local.Local1.Count = 0;
    local.Unique.Count = 0;
    export.Export1.Index = -1;
    local.Unique.Index = -1;
    local.Local1.Index = -1;
    local.Current.Date = Now().Date;

    // --------------------------------
    // Read all Active Roles
    // (Start Date <= Current Date <= End Date)
    // --------------------------------
    foreach(var item in ReadCaseRoleCsePerson3())
    {
      if (local.Unique.Index >= 0)
      {
        for(local.Unique.Index = 0; local.Unique.Index < local.Unique.Count; ++
          local.Unique.Index)
        {
          if (!local.Unique.CheckSize())
          {
            break;
          }

          if (Equal(local.Unique.Item.UniqueCsePerson.Number,
            entities.CsePerson.Number) && Equal
            (local.Unique.Item.UniqueCaseRole.Type1, entities.CaseRole.Type1))
          {
            goto ReadEach1;
          }
        }

        local.Unique.CheckIndex();
      }

      // --------------------------------
      // 05/06/99 W.Campbell - Fixed subscript
      // out of range (view overflow) problem.
      // group local_unique view subscript was
      // being incremented incorrectly.  Bad
      // statement disabled and replaced with
      // new set statement for subscript.  Added
      // check for subscript greater than max.
      // --------------------------------
      local.Unique.Index = local.Unique.Count;
      local.Unique.CheckSize();

      if (local.Unique.Index >= Local.UniqueGroup.Capacity)
      {
        ExitState = "OE0000_MAX_NUMBER_OF_ENTRIES_EX";

        return;
      }

      local.Unique.Update.UniqueCsePerson.Assign(entities.CsePerson);
      local.Unique.Update.UniqueCaseRole.Assign(entities.CaseRole);

      ++local.Local1.Index;
      local.Local1.CheckSize();

      if (local.Local1.Index >= Local.LocalGroup.Capacity)
      {
        ExitState = "OE0000_MAX_NUMBER_OF_ENTRIES_EX";

        return;
      }

      local.Local1.Update.Status.Text1 = "A";
      local.Local1.Update.CsePerson.Assign(entities.CsePerson);

ReadEach1:
      ;
    }

    // --------------------------------
    // Read all Pending Roles
    // (Start Date > Current Date)
    // --------------------------------
    foreach(var item in ReadCaseRoleCsePerson1())
    {
      if (local.Unique.Index >= 0)
      {
        for(local.Unique.Index = 0; local.Unique.Index < local.Unique.Count; ++
          local.Unique.Index)
        {
          if (!local.Unique.CheckSize())
          {
            break;
          }

          if (Equal(local.Unique.Item.UniqueCsePerson.Number,
            entities.CsePerson.Number) && Equal
            (local.Unique.Item.UniqueCaseRole.Type1, entities.CaseRole.Type1))
          {
            goto ReadEach2;
          }
        }

        local.Unique.CheckIndex();
      }

      // --------------------------------
      // 05/06/99 W.Campbell - Fixed subscript
      // out of range (view overflow) problem.
      // group local_unique view subscript was
      // being incremented incorrectly.  Bad
      // statement disabled and replaced with
      // new set statement for subscript.Added
      // check for subscript greater than max.
      // --------------------------------
      local.Unique.Index = local.Unique.Count;
      local.Unique.CheckSize();

      if (local.Unique.Index >= Local.UniqueGroup.Capacity)
      {
        ExitState = "OE0000_MAX_NUMBER_OF_ENTRIES_EX";

        return;
      }

      local.Unique.Update.UniqueCsePerson.Assign(entities.CsePerson);
      local.Unique.Update.UniqueCaseRole.Assign(entities.CaseRole);

      ++local.Local1.Index;
      local.Local1.CheckSize();

      if (local.Local1.Index >= Local.LocalGroup.Capacity)
      {
        ExitState = "OE0000_MAX_NUMBER_OF_ENTRIES_EX";

        return;
      }

      local.Local1.Update.Status.Text1 = "P";
      local.Local1.Update.CsePerson.Assign(entities.CsePerson);

ReadEach2:
      ;
    }

    // --------------------------------
    // Read all Inactive Roles
    // (End Date < Current Date)
    // --------------------------------
    foreach(var item in ReadCaseRoleCsePerson2())
    {
      if (local.Unique.Index >= 0)
      {
        for(local.Unique.Index = 0; local.Unique.Index < local.Unique.Count; ++
          local.Unique.Index)
        {
          if (!local.Unique.CheckSize())
          {
            break;
          }

          if (Equal(local.Unique.Item.UniqueCsePerson.Number,
            entities.CsePerson.Number) && Equal
            (local.Unique.Item.UniqueCaseRole.Type1, entities.CaseRole.Type1))
          {
            goto ReadEach3;
          }
        }

        local.Unique.CheckIndex();
      }

      // --------------------------------
      // 05/06/99 W.Campbell - Fixed subscript
      // out of range (view overflow) problem.
      // group local_unique view subscript was
      // being incremented incorrectly.  Bad
      // statement disabled and replaced with
      // new set statement for subscript. Added
      // check for subscript greater than max.
      // --------------------------------
      local.Unique.Index = local.Unique.Count;
      local.Unique.CheckSize();

      if (local.Unique.Index >= Local.UniqueGroup.Capacity)
      {
        ExitState = "OE0000_MAX_NUMBER_OF_ENTRIES_EX";

        return;
      }

      local.Unique.Update.UniqueCsePerson.Assign(entities.CsePerson);
      local.Unique.Update.UniqueCaseRole.Assign(entities.CaseRole);

      ++local.Local1.Index;
      local.Local1.CheckSize();

      if (local.Local1.Index >= Local.LocalGroup.Capacity)
      {
        ExitState = "OE0000_MAX_NUMBER_OF_ENTRIES_EX";

        return;
      }

      local.Local1.Update.Status.Text1 = "I";
      local.Local1.Update.CsePerson.Assign(entities.CsePerson);

ReadEach3:
      ;
    }

    local.Local1.Index = 0;

    for(var limit = local.Local1.Count; local.Local1.Index < limit; ++
      local.Local1.Index)
    {
      if (!local.Local1.CheckSize())
      {
        break;
      }

      // --------------------------
      // Weed out Persons that have
      // already been displayed.
      // --------------------------
      switch(AsChar(local.Local1.Item.Status.Text1))
      {
        case 'A':
          if (AsChar(import.PageKeyStatus.Text1) == 'P' || AsChar
            (import.PageKeyStatus.Text1) == 'I')
          {
            continue;
          }

          break;
        case 'P':
          if (AsChar(import.PageKeyStatus.Text1) == 'I')
          {
            continue;
          }

          break;
        case 'I':
          break;
        default:
          break;
      }

      if (AsChar(local.Local1.Item.Status.Text1) == AsChar
        (import.PageKeyStatus.Text1))
      {
        if (Lt(local.Local1.Item.CsePerson.Number, import.PageKey.Number))
        {
          continue;
        }
      }

      ++export.Export1.Index;
      export.Export1.CheckSize();

      if (export.Export1.Index >= Export.ExportGroup.Capacity)
      {
        export.NextPageStatus.Text1 = local.Local1.Item.Status.Text1;
        export.NextPage.Number = local.Local1.Item.CsePerson.Number;

        break;
      }

      export.Export1.Update.DetailWorkArea.Text1 =
        local.Local1.Item.Status.Text1;

      if (AsChar(local.Local1.Item.CsePerson.Type1) == 'O')
      {
        export.Export1.Update.DetailCsePersonsWorkSet.FormattedName =
          local.Local1.Item.CsePerson.OrganizationName ?? Spaces(33);
        export.Export1.Update.DetailCsePersonsWorkSet.Number =
          local.Local1.Item.CsePerson.Number;
      }
      else
      {
        local.Local1.Update.CsePersonsWorkSet.Number =
          local.Local1.Item.CsePerson.Number;
        local.SuppressZeroDob.Flag = "Y";
        UseSiReadCsePerson();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        export.Export1.Update.DetailCsePersonsWorkSet.Assign(
          local.Local1.Item.CsePersonsWorkSet);
      }
    }

    local.Local1.CheckIndex();

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

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.ErrOnAdabasUnavailable.Flag = local.ErrOnAdabasUnavailable.Flag;
    useImport.SuppressZeroDob.Flag = local.SuppressZeroDob.Flag;
    useImport.CsePersonsWorkSet.Number =
      local.Local1.Item.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.Local1.Update.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private IEnumerable<bool> ReadCaseRoleCsePerson1()
  {
    entities.CaseRole.Populated = false;
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCaseRoleCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", import.Case1.Number);
        db.SetString(command, "type", import.CaseRole.Type1);
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
        entities.CsePerson.Type1 = db.GetString(reader, 6);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 7);
        entities.CaseRole.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseRoleCsePerson2()
  {
    entities.CaseRole.Populated = false;
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCaseRoleCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", import.Case1.Number);
        db.SetString(command, "type", import.CaseRole.Type1);
        db.SetNullableDate(
          command, "endDate", local.Current.Date.GetValueOrDefault());
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
        entities.CsePerson.Type1 = db.GetString(reader, 6);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 7);
        entities.CaseRole.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseRoleCsePerson3()
  {
    entities.CaseRole.Populated = false;
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCaseRoleCsePerson3",
      (db, command) =>
      {
        db.SetString(command, "casNumber", import.Case1.Number);
        db.SetString(command, "type", import.CaseRole.Type1);
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
        entities.CsePerson.Type1 = db.GetString(reader, 6);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 7);
        entities.CaseRole.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

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
    /// A value of PageKeyStatus.
    /// </summary>
    [JsonPropertyName("pageKeyStatus")]
    public WorkArea PageKeyStatus
    {
      get => pageKeyStatus ??= new();
      set => pageKeyStatus = value;
    }

    /// <summary>
    /// A value of PageKey.
    /// </summary>
    [JsonPropertyName("pageKey")]
    public CsePersonsWorkSet PageKey
    {
      get => pageKey ??= new();
      set => pageKey = value;
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

    private WorkArea pageKeyStatus;
    private CsePersonsWorkSet pageKey;
    private Standard standard;
    private CaseRole caseRole;
    private Case1 case1;
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
      /// A value of DetailWorkArea.
      /// </summary>
      [JsonPropertyName("detailWorkArea")]
      public WorkArea DetailWorkArea
      {
        get => detailWorkArea ??= new();
        set => detailWorkArea = value;
      }

      /// <summary>
      /// A value of DetailCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("detailCsePersonsWorkSet")]
      public CsePersonsWorkSet DetailCsePersonsWorkSet
      {
        get => detailCsePersonsWorkSet ??= new();
        set => detailCsePersonsWorkSet = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private Common detailCommon;
      private WorkArea detailWorkArea;
      private CsePersonsWorkSet detailCsePersonsWorkSet;
    }

    /// <summary>
    /// A value of Ae.
    /// </summary>
    [JsonPropertyName("ae")]
    public Common Ae
    {
      get => ae ??= new();
      set => ae = value;
    }

    /// <summary>
    /// A value of NextPageStatus.
    /// </summary>
    [JsonPropertyName("nextPageStatus")]
    public WorkArea NextPageStatus
    {
      get => nextPageStatus ??= new();
      set => nextPageStatus = value;
    }

    /// <summary>
    /// A value of NextPage.
    /// </summary>
    [JsonPropertyName("nextPage")]
    public CsePersonsWorkSet NextPage
    {
      get => nextPage ??= new();
      set => nextPage = value;
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

    private Common ae;
    private WorkArea nextPageStatus;
    private CsePersonsWorkSet nextPage;
    private Standard standard;
    private Array<ExportGroup> export1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A UniqueGroup group.</summary>
    [Serializable]
    public class UniqueGroup
    {
      /// <summary>
      /// A value of UniqueCaseRole.
      /// </summary>
      [JsonPropertyName("uniqueCaseRole")]
      public CaseRole UniqueCaseRole
      {
        get => uniqueCaseRole ??= new();
        set => uniqueCaseRole = value;
      }

      /// <summary>
      /// A value of UniqueCsePerson.
      /// </summary>
      [JsonPropertyName("uniqueCsePerson")]
      public CsePerson UniqueCsePerson
      {
        get => uniqueCsePerson ??= new();
        set => uniqueCsePerson = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 40;

      private CaseRole uniqueCaseRole;
      private CsePerson uniqueCsePerson;
    }

    /// <summary>A LocalGroup group.</summary>
    [Serializable]
    public class LocalGroup
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

      /// <summary>
      /// A value of Status.
      /// </summary>
      [JsonPropertyName("status")]
      public WorkArea Status
      {
        get => status ??= new();
        set => status = value;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 40;

      private Common common;
      private WorkArea status;
      private CsePersonsWorkSet csePersonsWorkSet;
      private CsePerson csePerson;
    }

    /// <summary>
    /// Gets a value of Unique.
    /// </summary>
    [JsonIgnore]
    public Array<UniqueGroup> Unique => unique ??= new(UniqueGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Unique for json serialization.
    /// </summary>
    [JsonPropertyName("unique")]
    [Computed]
    public IList<UniqueGroup> Unique_Json
    {
      get => unique;
      set => Unique.Assign(value);
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

    /// <summary>
    /// Gets a value of Local1.
    /// </summary>
    [JsonIgnore]
    public Array<LocalGroup> Local1 => local1 ??= new(LocalGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Local1 for json serialization.
    /// </summary>
    [JsonPropertyName("local1")]
    [Computed]
    public IList<LocalGroup> Local1_Json
    {
      get => local1;
      set => Local1.Assign(value);
    }

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
    /// A value of ZdelLocalSubscript.
    /// </summary>
    [JsonPropertyName("zdelLocalSubscript")]
    public Common ZdelLocalSubscript
    {
      get => zdelLocalSubscript ??= new();
      set => zdelLocalSubscript = value;
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
    /// A value of Test.
    /// </summary>
    [JsonPropertyName("test")]
    public Common Test
    {
      get => test ??= new();
      set => test = value;
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
    /// A value of SuppressZeroDob.
    /// </summary>
    [JsonPropertyName("suppressZeroDob")]
    public Common SuppressZeroDob
    {
      get => suppressZeroDob ??= new();
      set => suppressZeroDob = value;
    }

    private Array<UniqueGroup> unique;
    private DateWorkArea current;
    private Array<LocalGroup> local1;
    private Common errOnAdabasUnavailable;
    private Common zdelLocalSubscript;
    private DateWorkArea dateWorkArea;
    private Common test;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common suppressZeroDob;
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
