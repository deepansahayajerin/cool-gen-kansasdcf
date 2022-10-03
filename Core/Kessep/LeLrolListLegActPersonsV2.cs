// Program: LE_LROL_LIST_LEG_ACT_PERSONS_V2, ID: 371998949, model: 746.
// Short name: SWE01667
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
/// A program: LE_LROL_LIST_LEG_ACT_PERSONS_V2.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block will list all Persons related to the CSE Case number 
/// entered. Also the relationship from Legal Action to CSE Person will be shown
/// if it exists.
/// </para>
/// </summary>
[Serializable]
public partial class LeLrolListLegActPersonsV2: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_LROL_LIST_LEG_ACT_PERSONS_V2 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeLrolListLegActPersonsV2(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeLrolListLegActPersonsV2.
  /// </summary>
  public LeLrolListLegActPersonsV2(IContext context, Import import,
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
    // ------------------------------------------------------------
    // Date		Developer	Request #	Description
    // 05/22/95	Dave Allen			Initial Code
    // 10/08/96	Govindaraj	IDCR 215,221,222	To provide for non cse persons and 
    // relate legal action with cse cases
    // 12/17/98 P. Sharp   Removed unused view from group view that was causing 
    // a consistency check error. Removed the read of fips and tribunal.
    // ------------------------------------------------------------
    export.LegalAction.Assign(import.LegalAction);

    if (!ReadLegalAction())
    {
      ExitState = "LEGAL_ACTION_NF";

      return;
    }

    // ------------------------------------------------------------
    // For each CSE Case specified
    // ------------------------------------------------------------
    export.Role.Index = -1;

    export.ListEndReason.Index = export.Role.Index;
    export.ListEndReason.CheckSize();

    for(import.CseCases.Index = 0; import.CseCases.Index < import
      .CseCases.Count; ++import.CseCases.Index)
    {
      if (export.Role.Index + 1 >= Export.RoleGroup.Capacity)
      {
        break;
      }

      if (IsEmpty(import.CseCases.Item.Detail.Number))
      {
        continue;
      }

      if (!ReadCase())
      {
        ExitState = "CASE_NF";

        return;
      }

      // ------------------------------------------------------------
      // Get all CSE Persons related to the Case Number entered.
      // ------------------------------------------------------------
      // --- Select AP, AR and CH roles. Dont list FA and MO roles.
      foreach(var item in ReadCaseRoleCsePerson())
      {
        ++export.Role.Index;
        export.Role.CheckSize();

        export.ListEndReason.Index = export.Role.Index;
        export.ListEndReason.CheckSize();

        export.Role.Update.Detail.Number = entities.Case1.Number;
        export.Role.Update.CaseRole.Assign(entities.CaseRole);
        export.ListEndReason.Update.DetailListEndRsn.PromptField = "";

        // ------------------------------------------------------------
        // Read Person ADABASE table using External Action Block.
        // ------------------------------------------------------------
        ExitState = "ACO_NN0000_ALL_OK";
        local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
        UseSiReadCsePerson();
        local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
        export.Role.Update.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);

        if (ReadLegalActionCaseRoleLaPersonLaCaseRoleLegalActionPerson())
        {
          // ---- Read and display Legal Action Person, i.e. Petitioner 
          // Respondent roles, only if Legal Action Case Role exists. This will
          // avoid displaying P/R/C role details for other case roles for the
          // same cse person.
          export.Role.Update.LegalActionPerson.
            Assign(entities.LegalActionPerson);
        }
      }
    }

    // --- Now list the non cse persons if any exist
    foreach(var item in ReadLegalActionPersonCsePerson())
    {
      if (export.Role.Index + 1 >= Export.RoleGroup.Capacity)
      {
        break;
      }

      foreach(var item1 in ReadLaPersonLaCaseRole())
      {
        // --- This cse person is not a non cse person. So skip this person.
        goto ReadEach;
      }

      // --- i.e. The cse person may have some cse case role but not assoc with 
      // this legal action (legal action case role does not exist).
      ++export.Role.Index;
      export.Role.CheckSize();

      export.ListEndReason.Index = export.Role.Index;
      export.ListEndReason.CheckSize();

      export.ListEndReason.Update.DetailListEndRsn.PromptField = "";
      export.Role.Update.LegalActionPerson.Assign(entities.LegalActionPerson);

      // ------------------------------------------------------------
      // Read Person ADABASE table using External Action Block.
      // ------------------------------------------------------------
      ExitState = "ACO_NN0000_ALL_OK";
      local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
      UseSiReadCsePerson();
      local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
      export.Role.Update.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);

ReadEach:
      ;
    }

    if (export.Role.IsEmpty)
    {
      ExitState = "LE0000_NO_LROL_CSE_PERSN_TO_DISP";
    }
    else if (export.Role.IsFull)
    {
      ExitState = "LE0000_LIST_FULL_SPEC_FEWER_CASE";
    }
    else
    {
      ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
    }
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CseCases.Item.Detail.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCaseRoleCsePerson()
  {
    entities.CaseRole.Populated = false;
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCaseRoleCsePerson",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
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
        entities.CaseRole.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadLaPersonLaCaseRole()
  {
    entities.LaPersonLaCaseRole.Populated = false;

    return ReadEach("ReadLaPersonLaCaseRole",
      (db, command) =>
      {
        db.SetInt32(command, "lapId", entities.LegalActionPerson.Identifier);
      },
      (db, reader) =>
      {
        entities.LaPersonLaCaseRole.Identifier = db.GetInt32(reader, 0);
        entities.LaPersonLaCaseRole.CroId = db.GetInt32(reader, 1);
        entities.LaPersonLaCaseRole.CroType = db.GetString(reader, 2);
        entities.LaPersonLaCaseRole.CspNum = db.GetString(reader, 3);
        entities.LaPersonLaCaseRole.CasNum = db.GetString(reader, 4);
        entities.LaPersonLaCaseRole.LgaId = db.GetInt32(reader, 5);
        entities.LaPersonLaCaseRole.LapId = db.GetInt32(reader, 6);
        entities.LaPersonLaCaseRole.Populated = true;
        CheckValid<LaPersonLaCaseRole>("CroType",
          entities.LaPersonLaCaseRole.CroType);

        return true;
      });
  }

  private bool ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", import.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalActionCaseRoleLaPersonLaCaseRoleLegalActionPerson()
  {
    System.Diagnostics.Debug.Assert(entities.CaseRole.Populated);
    entities.LaPersonLaCaseRole.Populated = false;
    entities.LegalActionCaseRole.Populated = false;
    entities.LegalActionPerson.Populated = false;

    return Read("ReadLegalActionCaseRoleLaPersonLaCaseRoleLegalActionPerson",
      (db, command) =>
      {
        db.SetInt32(command, "lgaId", entities.LegalAction.Identifier);
        db.SetInt32(command, "croIdentifier", entities.CaseRole.Identifier);
        db.SetString(command, "croType", entities.CaseRole.Type1);
        db.SetString(command, "cspNumber1", entities.CaseRole.CspNumber);
        db.SetString(command, "casNumber", entities.CaseRole.CasNumber);
        db.SetNullableInt32(
          command, "lgaIdentifier", import.LegalAction.Identifier);
        db.SetNullableString(command, "cspNumber2", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.LegalActionCaseRole.CasNumber = db.GetString(reader, 0);
        entities.LaPersonLaCaseRole.CasNum = db.GetString(reader, 0);
        entities.LegalActionCaseRole.CspNumber = db.GetString(reader, 1);
        entities.LaPersonLaCaseRole.CspNum = db.GetString(reader, 1);
        entities.LegalActionCaseRole.CroType = db.GetString(reader, 2);
        entities.LaPersonLaCaseRole.CroType = db.GetString(reader, 2);
        entities.LegalActionCaseRole.CroIdentifier = db.GetInt32(reader, 3);
        entities.LaPersonLaCaseRole.CroId = db.GetInt32(reader, 3);
        entities.LegalActionCaseRole.LgaId = db.GetInt32(reader, 4);
        entities.LaPersonLaCaseRole.LgaId = db.GetInt32(reader, 4);
        entities.LegalActionCaseRole.CreatedBy = db.GetString(reader, 5);
        entities.LaPersonLaCaseRole.Identifier = db.GetInt32(reader, 6);
        entities.LaPersonLaCaseRole.LapId = db.GetInt32(reader, 7);
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 7);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 8);
        entities.LegalActionPerson.LgaIdentifier =
          db.GetNullableInt32(reader, 9);
        entities.LegalActionPerson.EffectiveDate = db.GetDate(reader, 10);
        entities.LegalActionPerson.Role = db.GetString(reader, 11);
        entities.LegalActionPerson.EndDate = db.GetNullableDate(reader, 12);
        entities.LegalActionPerson.EndReason = db.GetNullableString(reader, 13);
        entities.LaPersonLaCaseRole.Populated = true;
        entities.LegalActionCaseRole.Populated = true;
        entities.LegalActionPerson.Populated = true;
        CheckValid<LegalActionCaseRole>("CroType",
          entities.LegalActionCaseRole.CroType);
        CheckValid<LaPersonLaCaseRole>("CroType",
          entities.LaPersonLaCaseRole.CroType);
      });
  }

  private IEnumerable<bool> ReadLegalActionPersonCsePerson()
  {
    entities.CsePerson.Populated = false;
    entities.LegalActionPerson.Populated = false;

    return ReadEach("ReadLegalActionPersonCsePerson",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.LegalActionPerson.LgaIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.LegalActionPerson.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionPerson.Role = db.GetString(reader, 4);
        entities.LegalActionPerson.EndDate = db.GetNullableDate(reader, 5);
        entities.LegalActionPerson.EndReason = db.GetNullableString(reader, 6);
        entities.CsePerson.Type1 = db.GetString(reader, 7);
        entities.CsePerson.Populated = true;
        entities.LegalActionPerson.Populated = true;
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
    /// <summary>A CseCasesGroup group.</summary>
    [Serializable]
    public class CseCasesGroup
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
      public const int Capacity = 50;

      private Case1 detail;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// Gets a value of CseCases.
    /// </summary>
    [JsonIgnore]
    public Array<CseCasesGroup> CseCases => cseCases ??= new(
      CseCasesGroup.Capacity);

    /// <summary>
    /// Gets a value of CseCases for json serialization.
    /// </summary>
    [JsonPropertyName("cseCases")]
    [Computed]
    public IList<CseCasesGroup> CseCases_Json
    {
      get => cseCases;
      set => CseCases.Assign(value);
    }

    private LegalAction legalAction;
    private Array<CseCasesGroup> cseCases;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ListEndReasonGroup group.</summary>
    [Serializable]
    public class ListEndReasonGroup
    {
      /// <summary>
      /// A value of DetailListEndRsn.
      /// </summary>
      [JsonPropertyName("detailListEndRsn")]
      public Standard DetailListEndRsn
      {
        get => detailListEndRsn ??= new();
        set => detailListEndRsn = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 60;

      private Standard detailListEndRsn;
    }

    /// <summary>A RoleGroup group.</summary>
    [Serializable]
    public class RoleGroup
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
      /// A value of CaseRole.
      /// </summary>
      [JsonPropertyName("caseRole")]
      public CaseRole CaseRole
      {
        get => caseRole ??= new();
        set => caseRole = value;
      }

      /// <summary>
      /// A value of LegalActionPerson.
      /// </summary>
      [JsonPropertyName("legalActionPerson")]
      public LegalActionPerson LegalActionPerson
      {
        get => legalActionPerson ??= new();
        set => legalActionPerson = value;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 60;

      private Case1 detail;
      private Common common;
      private CaseRole caseRole;
      private LegalActionPerson legalActionPerson;
      private CsePersonsWorkSet csePersonsWorkSet;
    }

    /// <summary>
    /// Gets a value of ListEndReason.
    /// </summary>
    [JsonIgnore]
    public Array<ListEndReasonGroup> ListEndReason => listEndReason ??= new(
      ListEndReasonGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ListEndReason for json serialization.
    /// </summary>
    [JsonPropertyName("listEndReason")]
    [Computed]
    public IList<ListEndReasonGroup> ListEndReason_Json
    {
      get => listEndReason;
      set => ListEndReason.Assign(value);
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// Gets a value of Role.
    /// </summary>
    [JsonIgnore]
    public Array<RoleGroup> Role => role ??= new(RoleGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Role for json serialization.
    /// </summary>
    [JsonPropertyName("role")]
    [Computed]
    public IList<RoleGroup> Role_Json
    {
      get => role;
      set => Role.Assign(value);
    }

    private Array<ListEndReasonGroup> listEndReason;
    private LegalAction legalAction;
    private Array<RoleGroup> role;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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

    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of LaPersonLaCaseRole.
    /// </summary>
    [JsonPropertyName("laPersonLaCaseRole")]
    public LaPersonLaCaseRole LaPersonLaCaseRole
    {
      get => laPersonLaCaseRole ??= new();
      set => laPersonLaCaseRole = value;
    }

    /// <summary>
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
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
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    private LaPersonLaCaseRole laPersonLaCaseRole;
    private LegalActionCaseRole legalActionCaseRole;
    private Case1 case1;
    private CaseRole caseRole;
    private CsePerson csePerson;
    private LegalActionPerson legalActionPerson;
    private LegalAction legalAction;
  }
#endregion
}
