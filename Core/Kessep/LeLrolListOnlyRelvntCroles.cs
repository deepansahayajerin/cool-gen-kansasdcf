// Program: LE_LROL_LIST_ONLY_RELVNT_CROLES, ID: 371998955, model: 746.
// Short name: SWE01772
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
/// A program: LE_LROL_LIST_ONLY_RELVNT_CROLES.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block will list all Persons related to the CSE Case number 
/// entered. Also the relationship from Legal Action to CSE Person will be shown
/// if it exists.
/// This action block is superseded by LE LROL LIST ONLY RELVNT CROLES that uses
/// LA PERSON LA CASE ROLE entity. Once LROL is tested and accepted, this
/// action block must be deleted.
/// </para>
/// </summary>
[Serializable]
public partial class LeLrolListOnlyRelvntCroles: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_LROL_LIST_ONLY_RELVNT_CROLES program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeLrolListOnlyRelvntCroles(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeLrolListOnlyRelvntCroles.
  /// </summary>
  public LeLrolListOnlyRelvntCroles(IContext context, Import import,
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
    // 10/08/96	Govindaraj	Initial code. List only relevant cse case roles.
    // 11/27/96	Govindaraj	IDCR 254	Modified to use LA PERSON LA CASE ROLE 
    // entity
    // ------------------------------------------------------------
    export.LegalAction.Identifier = import.LegalAction.Identifier;

    if (ReadLegalAction())
    {
      export.LegalAction.Assign(entities.LegalAction);

      if (ReadTribunal())
      {
        export.Tribunal.Assign(entities.Tribunal);

        if (ReadFips())
        {
          export.Fips.Assign(entities.Fips);
        }
      }
    }
    else
    {
      ExitState = "LEGAL_ACTION_NF";

      return;
    }

    export.Role.Index = -1;

    export.ListEndReason.Index = export.Role.Index;
    export.ListEndReason.CheckSize();

    // --- First list all cse case related persons
    local.CountMultiCasesInvolved.Count = 0;

    foreach(var item in ReadLegalActionPersonLaPersonLaCaseRoleLegalActionCaseRole())
      
    {
      if (export.Role.Index + 1 >= Export.RoleGroup.Capacity)
      {
        break;
      }

      if (!import.CseCasesList.IsEmpty)
      {
        // --- List only the case roles for the case numbers specified.
        // --- RCG determine if the legal action is related to persons
        // --- involved in multiple cases.
        local.NonBlankCaseNoSpecifd.Flag = "N";

        for(import.CseCasesList.Index = 0; import.CseCasesList.Index < import
          .CseCasesList.Count; ++import.CseCasesList.Index)
        {
          if (!IsEmpty(import.CseCasesList.Item.DetailListOnly.Number))
          {
            local.NonBlankCaseNoSpecifd.Flag = "Y";

            if (Equal(entities.Case1.Number,
              import.CseCasesList.Item.DetailListOnly.Number))
            {
              // --- The  case number matches that specified. So the case role 
              // must be listed.
              ++local.CountMultiCasesInvolved.Count;

              goto Test;
            }
          }
        }

        if (AsChar(local.NonBlankCaseNoSpecifd.Flag) == 'Y')
        {
          // --- The  case number did not match any of the case numbers 
          // specified. So the case role must be skipped.
          continue;
        }
      }

Test:

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
      local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
      UseSiReadCsePerson();
      local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
      export.Role.Update.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);
      export.Role.Update.LegalActionPerson.Assign(entities.LegalActionPerson);
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
        // --- This is a case related person. So skip it.
        goto ReadEach;
      }

      // --- i.e. The cse person may have some cse case role but not assoc with 
      // this legal action (legal action case role does not exist).
      ++export.Role.Index;
      export.Role.CheckSize();

      export.ListEndReason.Index = export.Role.Index;
      export.ListEndReason.CheckSize();

      export.Role.Update.LegalActionPerson.Assign(entities.LegalActionPerson);

      // ------------------------------------------------------------
      // Read Person ADABASE table using External Action Block.
      // ------------------------------------------------------------
      local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
      UseSiReadCsePerson();
      local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
      export.Role.Update.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);
      export.ListEndReason.Update.DetailListEndRsn.PromptField = "";

ReadEach:
      ;
    }

    if (export.Role.IsEmpty)
    {
      ExitState = "LE0000_NO_LROL_CSE_PERS_2_DISP_2";
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

  private bool ReadFips()
  {
    System.Diagnostics.Debug.Assert(entities.Tribunal.Populated);
    entities.Fips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.SetInt32(
          command, "location",
          entities.Tribunal.FipLocation.GetValueOrDefault());
        db.SetInt32(
          command, "county", entities.Tribunal.FipCounty.GetValueOrDefault());
        db.SetInt32(
          command, "state", entities.Tribunal.FipState.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.CountyDescription = db.GetNullableString(reader, 3);
        entities.Fips.StateAbbreviation = db.GetString(reader, 4);
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 5);
        entities.Fips.Populated = true;
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
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 3);
        entities.LegalAction.Populated = true;
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

  private IEnumerable<bool>
    ReadLegalActionPersonLaPersonLaCaseRoleLegalActionCaseRole()
  {
    entities.LaPersonLaCaseRole.Populated = false;
    entities.LegalActionCaseRole.Populated = false;
    entities.Case1.Populated = false;
    entities.CaseRole.Populated = false;
    entities.CsePerson.Populated = false;
    entities.LegalActionPerson.Populated = false;

    return ReadEach(
      "ReadLegalActionPersonLaPersonLaCaseRoleLegalActionCaseRole",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.LaPersonLaCaseRole.LapId = db.GetInt32(reader, 0);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.LegalActionPerson.LgaIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.LegalActionPerson.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionPerson.Role = db.GetString(reader, 4);
        entities.LegalActionPerson.EndDate = db.GetNullableDate(reader, 5);
        entities.LegalActionPerson.EndReason = db.GetNullableString(reader, 6);
        entities.LaPersonLaCaseRole.Identifier = db.GetInt32(reader, 7);
        entities.LaPersonLaCaseRole.CroId = db.GetInt32(reader, 8);
        entities.LegalActionCaseRole.CroIdentifier = db.GetInt32(reader, 8);
        entities.CaseRole.Identifier = db.GetInt32(reader, 8);
        entities.LaPersonLaCaseRole.CroType = db.GetString(reader, 9);
        entities.LegalActionCaseRole.CroType = db.GetString(reader, 9);
        entities.CaseRole.Type1 = db.GetString(reader, 9);
        entities.LaPersonLaCaseRole.CspNum = db.GetString(reader, 10);
        entities.LegalActionCaseRole.CspNumber = db.GetString(reader, 10);
        entities.CaseRole.CspNumber = db.GetString(reader, 10);
        entities.CsePerson.Number = db.GetString(reader, 10);
        entities.LaPersonLaCaseRole.CasNum = db.GetString(reader, 11);
        entities.LegalActionCaseRole.CasNumber = db.GetString(reader, 11);
        entities.CaseRole.CasNumber = db.GetString(reader, 11);
        entities.Case1.Number = db.GetString(reader, 11);
        entities.Case1.Number = db.GetString(reader, 11);
        entities.LaPersonLaCaseRole.LgaId = db.GetInt32(reader, 12);
        entities.LegalActionCaseRole.LgaId = db.GetInt32(reader, 12);
        entities.LegalActionCaseRole.CreatedBy = db.GetString(reader, 13);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 14);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 15);
        entities.CsePerson.Type1 = db.GetString(reader, 16);
        entities.LaPersonLaCaseRole.Populated = true;
        entities.LegalActionCaseRole.Populated = true;
        entities.Case1.Populated = true;
        entities.CaseRole.Populated = true;
        entities.CsePerson.Populated = true;
        entities.LegalActionPerson.Populated = true;
        CheckValid<LaPersonLaCaseRole>("CroType",
          entities.LaPersonLaCaseRole.CroType);
        CheckValid<LegalActionCaseRole>("CroType",
          entities.LegalActionCaseRole.CroType);
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private bool ReadTribunal()
  {
    System.Diagnostics.Debug.Assert(entities.LegalAction.Populated);
    entities.Tribunal.Populated = false;

    return Read("ReadTribunal",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.LegalAction.TrbId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Tribunal.JudicialDivision = db.GetNullableString(reader, 0);
        entities.Tribunal.Name = db.GetString(reader, 1);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 2);
        entities.Tribunal.JudicialDistrict = db.GetString(reader, 3);
        entities.Tribunal.Identifier = db.GetInt32(reader, 4);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 5);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 6);
        entities.Tribunal.Populated = true;
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
    /// <summary>A CseCasesListGroup group.</summary>
    [Serializable]
    public class CseCasesListGroup
    {
      /// <summary>
      /// A value of DetailListOnly.
      /// </summary>
      [JsonPropertyName("detailListOnly")]
      public Case1 DetailListOnly
      {
        get => detailListOnly ??= new();
        set => detailListOnly = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Case1 detailListOnly;
    }

    /// <summary>
    /// Gets a value of CseCasesList.
    /// </summary>
    [JsonIgnore]
    public Array<CseCasesListGroup> CseCasesList => cseCasesList ??= new(
      CseCasesListGroup.Capacity);

    /// <summary>
    /// Gets a value of CseCasesList for json serialization.
    /// </summary>
    [JsonPropertyName("cseCasesList")]
    [Computed]
    public IList<CseCasesListGroup> CseCasesList_Json
    {
      get => cseCasesList;
      set => CseCasesList.Assign(value);
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

    private Array<CseCasesListGroup> cseCasesList;
    private LegalAction legalAction;
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
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
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

    private Tribunal tribunal;
    private Fips fips;
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
    /// A value of CountMultiCasesInvolved.
    /// </summary>
    [JsonPropertyName("countMultiCasesInvolved")]
    public Common CountMultiCasesInvolved
    {
      get => countMultiCasesInvolved ??= new();
      set => countMultiCasesInvolved = value;
    }

    /// <summary>
    /// A value of NonBlankCaseNoSpecifd.
    /// </summary>
    [JsonPropertyName("nonBlankCaseNoSpecifd")]
    public Common NonBlankCaseNoSpecifd
    {
      get => nonBlankCaseNoSpecifd ??= new();
      set => nonBlankCaseNoSpecifd = value;
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

    private Common countMultiCasesInvolved;
    private Common nonBlankCaseNoSpecifd;
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
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
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
    private Tribunal tribunal;
    private Fips fips;
    private Case1 case1;
    private CaseRole caseRole;
    private CsePerson csePerson;
    private LegalActionPerson legalActionPerson;
    private LegalAction legalAction;
  }
#endregion
}
