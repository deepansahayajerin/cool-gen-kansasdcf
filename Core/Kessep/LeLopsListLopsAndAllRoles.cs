// Program: LE_LOPS_LIST_LOPS_AND_ALL_ROLES, ID: 372006659, model: 746.
// Short name: SWE01671
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
/// A program: LE_LOPS_LIST_LOPS_AND_ALL_ROLES.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block will list all Persons related to the CSE Case number(s) 
/// entered. Also the relationship from Legal Action Detail to Legal Action
/// Person will be shown if it exists.
/// </para>
/// </summary>
[Serializable]
public partial class LeLopsListLopsAndAllRoles: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_LOPS_LIST_LOPS_AND_ALL_ROLES program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeLopsListLopsAndAllRoles(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeLopsListLopsAndAllRoles.
  /// </summary>
  public LeLopsListLopsAndAllRoles(IContext context, Import import,
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
    // 08/07/95	Dave Allen			Initial Code
    // 11/27/96	govind		IDCR 254. Modified to use LA PERSON LA CASE ROLE entity
    // ------------------------------------------------------------
    // ---------------------------------------------
    // Currently this does not check for case_role effective and expiry dates. 
    // If we check that, we may not be able to see it at a future point after
    // the case_role is expired.
    // The program needs to list all the case roles as well as all the 
    // legal_action_person records !!
    // ---------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;

    if (!ReadLegalActionDetailLegalAction())
    {
      ExitState = "LEGAL_ACTION_NF";

      return;
    }

    // ------------------------------------------------------------
    // Get all CSE Persons related to the Case Number(s) entered.
    // ------------------------------------------------------------
    export.ObligationPersons.Index = -1;

    for(import.CseCases.Index = 0; import.CseCases.Index < import
      .CseCases.Count; ++import.CseCases.Index)
    {
      if (export.ObligationPersons.Index + 1 >= Export
        .ObligationPersonsGroup.Capacity)
      {
        break;
      }

      if (IsEmpty(import.CseCases.Item.Cse.Number))
      {
        continue;
      }

      if (!ReadCase())
      {
        ExitState = "CASE_NF";

        return;
      }

      foreach(var item in ReadCaseRoleCsePerson1())
      {
        ++export.ObligationPersons.Index;
        export.ObligationPersons.CheckSize();

        export.ObligationPersons.Update.Case1.Number = entities.Case1.Number;
        export.ObligationPersons.Update.CaseRole.Assign(entities.CaseRole);
        local.DateWorkArea.Date =
          export.ObligationPersons.Item.CaseRole.EndDate;
        UseCabSetMaximumDiscontinueDate();
        export.ObligationPersons.Update.CaseRole.EndDate =
          local.DateWorkArea.Date;
        export.ObligationPersons.Update.ListEndReason.PromptField = "";

        // ------------------------------------------------------------
        // Read Person ADABASE table using External Action Block.
        // ------------------------------------------------------------
        local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
        UseSiReadCsePerson();
        MoveCsePersonsWorkSet(local.CsePersonsWorkSet,
          export.ObligationPersons.Update.CsePersonsWorkSet);

        if (ReadLaPersonLaCaseRoleLegalActionCaseRoleLegalActionPerson())
        {
          export.ObligationPersons.Update.LegalActionPerson.Assign(
            entities.LegalActionPerson);
          local.DateWorkArea.Date =
            export.ObligationPersons.Item.LegalActionPerson.EndDate;
          UseCabSetMaximumDiscontinueDate();
          export.ObligationPersons.Update.LegalActionPerson.EndDate =
            local.DateWorkArea.Date;
        }
      }

      // *** per IDCR 443 list all the person that have an end date
      foreach(var item in ReadCaseRoleCsePerson2())
      {
        ++export.ObligationPersons.Index;
        export.ObligationPersons.CheckSize();

        export.ObligationPersons.Update.Case1.Number = entities.Case1.Number;
        export.ObligationPersons.Update.CaseRole.Assign(entities.CaseRole);
        local.DateWorkArea.Date =
          export.ObligationPersons.Item.CaseRole.EndDate;
        UseCabSetMaximumDiscontinueDate();
        export.ObligationPersons.Update.CaseRole.EndDate =
          local.DateWorkArea.Date;
        export.ObligationPersons.Update.ListEndReason.PromptField = "";

        // ------------------------------------------------------------
        // Read Person ADABASE table using External Action Block.
        // ------------------------------------------------------------
        local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
        UseSiReadCsePerson();
        MoveCsePersonsWorkSet(local.CsePersonsWorkSet,
          export.ObligationPersons.Update.CsePersonsWorkSet);

        if (ReadLaPersonLaCaseRoleLegalActionCaseRoleLegalActionPerson())
        {
          export.ObligationPersons.Update.LegalActionPerson.Assign(
            entities.LegalActionPerson);
          local.DateWorkArea.Date =
            export.ObligationPersons.Item.LegalActionPerson.EndDate;
          UseCabSetMaximumDiscontinueDate();
          export.ObligationPersons.Update.LegalActionPerson.EndDate =
            local.DateWorkArea.Date;
        }
      }
    }

    // --- Now list the non cse persons if any exist.
    foreach(var item in ReadLegalActionPersonCsePerson())
    {
      if (export.ObligationPersons.Index + 1 >= Export
        .ObligationPersonsGroup.Capacity)
      {
        break;
      }

      foreach(var item1 in ReadLaPersonLaCaseRole())
      {
        // --- This is a case related person. So skip it.
        goto ReadEach;
      }

      // --- This person may have some cse case role but not assoc with this 
      // legal action. (legal action case role does not exist).
      ++export.ObligationPersons.Index;
      export.ObligationPersons.CheckSize();

      export.ObligationPersons.Update.ListEndReason.PromptField = "";

      // ------------------------------------------------------------
      // Read Person ADABASE table using External Action Block.
      // ------------------------------------------------------------
      local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
      UseSiReadCsePerson();
      MoveCsePersonsWorkSet(local.CsePersonsWorkSet,
        export.ObligationPersons.Update.CsePersonsWorkSet);
      export.ObligationPersons.Update.LegalActionPerson.Assign(
        entities.LegalActionPerson);
      local.DateWorkArea.Date =
        export.ObligationPersons.Item.LegalActionPerson.EndDate;
      UseCabSetMaximumDiscontinueDate();
      export.ObligationPersons.Update.LegalActionPerson.EndDate =
        local.DateWorkArea.Date;

ReadEach:
      ;
    }

    if (export.ObligationPersons.IsEmpty)
    {
      ExitState = "LE0000_CASE_ROLE_NF_PRESS_PF17";
    }
    else if (export.ObligationPersons.IsFull)
    {
      ExitState = "LE0000_LIST_FULL_SPEC_FEWER_CASE";
    }
    else
    {
      ExitState = "ACO_NN0000_ALL_OK";
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.DateWorkArea.Date = useExport.DateWorkArea.Date;
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
        db.SetString(command, "numb", import.CseCases.Item.Cse.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCaseRoleCsePerson1()
  {
    entities.CaseRole.Populated = false;
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCaseRoleCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
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
        db.SetString(command, "casNumber", entities.Case1.Number);
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

  private bool ReadLaPersonLaCaseRoleLegalActionCaseRoleLegalActionPerson()
  {
    System.Diagnostics.Debug.Assert(entities.CaseRole.Populated);
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.LaPersonLaCaseRole.Populated = false;
    entities.LegalActionCaseRole.Populated = false;
    entities.LegalActionPerson.Populated = false;

    return Read("ReadLaPersonLaCaseRoleLegalActionCaseRoleLegalActionPerson",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladRNumber", entities.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.LegalActionDetail.LgaIdentifier);
        db.SetNullableString(command, "cspNumber1", entities.CsePerson.Number);
        db.SetInt32(command, "croIdentifier", entities.CaseRole.Identifier);
        db.SetString(command, "croType", entities.CaseRole.Type1);
        db.SetString(command, "cspNumber2", entities.CaseRole.CspNumber);
        db.SetString(command, "casNumber", entities.CaseRole.CasNumber);
        db.SetInt32(command, "lgaId", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LaPersonLaCaseRole.Identifier = db.GetInt32(reader, 0);
        entities.LaPersonLaCaseRole.CroId = db.GetInt32(reader, 1);
        entities.LegalActionCaseRole.CroIdentifier = db.GetInt32(reader, 1);
        entities.LaPersonLaCaseRole.CroType = db.GetString(reader, 2);
        entities.LegalActionCaseRole.CroType = db.GetString(reader, 2);
        entities.LaPersonLaCaseRole.CspNum = db.GetString(reader, 3);
        entities.LegalActionCaseRole.CspNumber = db.GetString(reader, 3);
        entities.LaPersonLaCaseRole.CasNum = db.GetString(reader, 4);
        entities.LegalActionCaseRole.CasNumber = db.GetString(reader, 4);
        entities.LaPersonLaCaseRole.LgaId = db.GetInt32(reader, 5);
        entities.LegalActionCaseRole.LgaId = db.GetInt32(reader, 5);
        entities.LaPersonLaCaseRole.LapId = db.GetInt32(reader, 6);
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 6);
        entities.LegalActionCaseRole.CreatedBy = db.GetString(reader, 7);
        entities.LegalActionCaseRole.CreatedTstamp = db.GetDateTime(reader, 8);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 9);
        entities.LegalActionPerson.EffectiveDate = db.GetDate(reader, 10);
        entities.LegalActionPerson.EndDate = db.GetNullableDate(reader, 11);
        entities.LegalActionPerson.EndReason = db.GetNullableString(reader, 12);
        entities.LegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 13);
        entities.LegalActionPerson.LadRNumber = db.GetNullableInt32(reader, 14);
        entities.LegalActionPerson.AccountType =
          db.GetNullableString(reader, 15);
        entities.LegalActionPerson.ArrearsAmount =
          db.GetNullableDecimal(reader, 16);
        entities.LegalActionPerson.CurrentAmount =
          db.GetNullableDecimal(reader, 17);
        entities.LegalActionPerson.JudgementAmount =
          db.GetNullableDecimal(reader, 18);
        entities.LaPersonLaCaseRole.Populated = true;
        entities.LegalActionCaseRole.Populated = true;
        entities.LegalActionPerson.Populated = true;
        CheckValid<LaPersonLaCaseRole>("CroType",
          entities.LaPersonLaCaseRole.CroType);
        CheckValid<LegalActionCaseRole>("CroType",
          entities.LegalActionCaseRole.CroType);
      });
  }

  private bool ReadLegalActionDetailLegalAction()
  {
    entities.LegalAction.Populated = false;
    entities.LegalActionDetail.Populated = false;

    return Read("ReadLegalActionDetailLegalAction",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", import.LegalAction.Identifier);
        db.SetInt32(command, "laDetailNo", import.LegalActionDetail.Number);
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalAction.Populated = true;
        entities.LegalActionDetail.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalActionPersonCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.CsePerson.Populated = false;
    entities.LegalActionPerson.Populated = false;

    return ReadEach("ReadLegalActionPersonCsePerson",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladRNumber", entities.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.LegalActionDetail.LgaIdentifier);
      },
      (db, reader) =>
      {
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.LegalActionPerson.EffectiveDate = db.GetDate(reader, 2);
        entities.LegalActionPerson.EndDate = db.GetNullableDate(reader, 3);
        entities.LegalActionPerson.EndReason = db.GetNullableString(reader, 4);
        entities.LegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 5);
        entities.LegalActionPerson.LadRNumber = db.GetNullableInt32(reader, 6);
        entities.LegalActionPerson.AccountType =
          db.GetNullableString(reader, 7);
        entities.LegalActionPerson.ArrearsAmount =
          db.GetNullableDecimal(reader, 8);
        entities.LegalActionPerson.CurrentAmount =
          db.GetNullableDecimal(reader, 9);
        entities.LegalActionPerson.JudgementAmount =
          db.GetNullableDecimal(reader, 10);
        entities.CsePerson.Type1 = db.GetString(reader, 11);
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
      /// A value of Cse.
      /// </summary>
      [JsonPropertyName("cse")]
      public Case1 Cse
      {
        get => cse ??= new();
        set => cse = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Case1 cse;
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
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
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
    private LegalActionDetail legalActionDetail;
    private Array<CseCasesGroup> cseCases;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ObligationPersonsGroup group.</summary>
    [Serializable]
    public class ObligationPersonsGroup
    {
      /// <summary>
      /// A value of ListEndReason.
      /// </summary>
      [JsonPropertyName("listEndReason")]
      public Standard ListEndReason
      {
        get => listEndReason ??= new();
        set => listEndReason = value;
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
      /// A value of LegalActionPerson.
      /// </summary>
      [JsonPropertyName("legalActionPerson")]
      public LegalActionPerson LegalActionPerson
      {
        get => legalActionPerson ??= new();
        set => legalActionPerson = value;
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
      /// A value of CsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("csePersonsWorkSet")]
      public CsePersonsWorkSet CsePersonsWorkSet
      {
        get => csePersonsWorkSet ??= new();
        set => csePersonsWorkSet = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Standard listEndReason;
      private Common common;
      private LegalActionPerson legalActionPerson;
      private Case1 case1;
      private CaseRole caseRole;
      private CsePersonsWorkSet csePersonsWorkSet;
    }

    /// <summary>
    /// Gets a value of ObligationPersons.
    /// </summary>
    [JsonIgnore]
    public Array<ObligationPersonsGroup> ObligationPersons =>
      obligationPersons ??= new(ObligationPersonsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ObligationPersons for json serialization.
    /// </summary>
    [JsonPropertyName("obligationPersons")]
    [Computed]
    public IList<ObligationPersonsGroup> ObligationPersons_Json
    {
      get => obligationPersons;
      set => ObligationPersons.Assign(value);
    }

    /// <summary>
    /// A value of ZdelTribunal.
    /// </summary>
    [JsonPropertyName("zdelTribunal")]
    public Tribunal ZdelTribunal
    {
      get => zdelTribunal ??= new();
      set => zdelTribunal = value;
    }

    /// <summary>
    /// A value of ZdelFips.
    /// </summary>
    [JsonPropertyName("zdelFips")]
    public Fips ZdelFips
    {
      get => zdelFips ??= new();
      set => zdelFips = value;
    }

    private Array<ObligationPersonsGroup> obligationPersons;
    private Tribunal zdelTribunal;
    private Fips zdelFips;
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

    private DateWorkArea current;
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
    /// A value of ZdelTribunal.
    /// </summary>
    [JsonPropertyName("zdelTribunal")]
    public Tribunal ZdelTribunal
    {
      get => zdelTribunal ??= new();
      set => zdelTribunal = value;
    }

    /// <summary>
    /// A value of ZdelFips.
    /// </summary>
    [JsonPropertyName("zdelFips")]
    public Fips ZdelFips
    {
      get => zdelFips ??= new();
      set => zdelFips = value;
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

    /// <summary>
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    private LaPersonLaCaseRole laPersonLaCaseRole;
    private LegalActionCaseRole legalActionCaseRole;
    private Tribunal zdelTribunal;
    private Fips zdelFips;
    private Case1 case1;
    private CaseRole caseRole;
    private CsePerson csePerson;
    private LegalActionPerson legalActionPerson;
    private LegalAction legalAction;
    private LegalActionDetail legalActionDetail;
  }
#endregion
}
