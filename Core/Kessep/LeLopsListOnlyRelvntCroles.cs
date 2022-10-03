// Program: LE_LOPS_LIST_ONLY_RELVNT_CROLES, ID: 372006661, model: 746.
// Short name: SWE01771
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
/// A program: LE_LOPS_LIST_ONLY_RELVNT_CROLES.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block will list all Persons related to the CSE Case number(s) 
/// entered. Also the relationship from Legal Action Detail to Legal Action
/// Person will be shown if it exists.
/// This action block has been superseded by the one that uses LA PERSON LA CASE
/// ROLE.
/// </para>
/// </summary>
[Serializable]
public partial class LeLopsListOnlyRelvntCroles: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_LOPS_LIST_ONLY_RELVNT_CROLES program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeLopsListOnlyRelvntCroles(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeLopsListOnlyRelvntCroles.
  /// </summary>
  public LeLopsListOnlyRelvntCroles(IContext context, Import import,
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
    // 11/27/96	govind		IDCR 254	Modified to list only relevant case roles
    // 01/29/99 P. Sharp  Made changes per Phase II assessment.
    // ------------------------------------------------------------
    // *********************************************
    // RCG	02/25/98	Priority # 19 issues
    // Count number of supported persons so that when all supported persons are 
    // end-dated, LOPS and LDET record end dates are unprotected.
    // *********************************************
    // ---------------------------------------------
    // Currently this does not check for case_role effective and expiry dates. 
    // If we check that, we may not be able to see it at a future point after
    // the case_role is expired.
    // The program needs to list all the case roles as well as all the 
    // legal_action_person records !!
    // ---------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.CountTotalSupported.Count = 0;

    if (!ReadLegalActionDetailLegalAction())
    {
      ExitState = "LEGAL_ACTION_NF";

      return;
    }

    // ------------------------------------------------------------
    // Get all case related CSE Persons related to the legal action via legal 
    // action case role
    // ------------------------------------------------------------
    export.ObligationPersons.Index = -1;

    foreach(var item in ReadLegalActionPersonCsePersonLaPersonLaCaseRole())
    {
      if (export.ObligationPersons.Index + 1 >= Export
        .ObligationPersonsGroup.Capacity)
      {
        return;
      }

      if (!import.CseCases.IsEmpty)
      {
        // --- List only those case roles belonging to the given cse case 
        // numbers
        local.NonBlankCaseNoSpecfd.Flag = "N";

        for(import.CseCases.Index = 0; import.CseCases.Index < import
          .CseCases.Count; ++import.CseCases.Index)
        {
          if (!IsEmpty(import.CseCases.Item.Detail.Number))
          {
            local.NonBlankCaseNoSpecfd.Flag = "Y";

            if (Equal(entities.Case1.Number, import.CseCases.Item.Detail.Number))
              
            {
              // --- We need to list the case roles for this cse case
              goto Test;
            }
          }
        }

        if (AsChar(local.NonBlankCaseNoSpecfd.Flag) == 'Y')
        {
          // --- The case was not specified by the user. So skip this case role 
          // and read next.
          continue;
        }
      }
      else
      {
        // --- No cse case was specified. So list the record read.
      }

Test:

      ++export.ObligationPersons.Index;
      export.ObligationPersons.CheckSize();

      export.ObligationPersons.Update.Case1.Number = entities.Case1.Number;
      export.ObligationPersons.Update.CaseRole.Assign(entities.CaseRole);
      local.DateWorkArea.Date = export.ObligationPersons.Item.CaseRole.EndDate;
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
      export.ObligationPersons.Update.LegalActionPerson.Assign(
        entities.LegalActionPerson);

      if (AsChar(entities.LegalActionPerson.AccountType) == 'S')
      {
        ++export.CountTotalSupported.Count;
      }

      local.DateWorkArea.Date =
        export.ObligationPersons.Item.LegalActionPerson.EndDate;
      UseCabSetMaximumDiscontinueDate();
      export.ObligationPersons.Update.LegalActionPerson.EndDate =
        local.DateWorkArea.Date;
    }

    // --- Now list the non cse persons if any exist.
    // --- Non cse person is a person who plays no case role and therefore does 
    // not participate in a case.
    foreach(var item in ReadLegalActionPersonCsePerson())
    {
      if (export.ObligationPersons.Index + 1 >= Export
        .ObligationPersonsGroup.Capacity)
      {
        break;
      }

      foreach(var item1 in ReadLaPersonLaCaseRole())
      {
        // --- This cse person is not a non cse person. So skip it.
        goto ReadEach;
      }

      ++export.ObligationPersons.Index;
      export.ObligationPersons.CheckSize();

      export.ObligationPersons.Update.LegalActionPerson.Assign(
        entities.LegalActionPerson);

      if (AsChar(entities.LegalActionPerson.AccountType) == 'S')
      {
        ++export.CountTotalSupported.Count;
      }

      // ------------------------------------------------------------
      // Read Person ADABASE table using External Action Block.
      // ------------------------------------------------------------
      local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
      UseSiReadCsePerson();
      MoveCsePersonsWorkSet(local.CsePersonsWorkSet,
        export.ObligationPersons.Update.CsePersonsWorkSet);
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
      ExitState = "LE0000_NO_RECORDS_PRESS_PF17";
    }
    else if (export.ObligationPersons.IsFull)
    {
      ExitState = "LE0000_LIST_FULL_SPEC_FEWER_CASE";
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

  private IEnumerable<bool> ReadLegalActionPersonCsePersonLaPersonLaCaseRole()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.LaPersonLaCaseRole.Populated = false;
    entities.LegalActionCaseRole.Populated = false;
    entities.Case1.Populated = false;
    entities.CaseRole.Populated = false;
    entities.CsePerson.Populated = false;
    entities.LegalActionPerson.Populated = false;

    return ReadEach("ReadLegalActionPersonCsePersonLaPersonLaCaseRole",
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
        entities.LaPersonLaCaseRole.LapId = db.GetInt32(reader, 0);
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
        entities.LaPersonLaCaseRole.Identifier = db.GetInt32(reader, 12);
        entities.LaPersonLaCaseRole.CroId = db.GetInt32(reader, 13);
        entities.LegalActionCaseRole.CroIdentifier = db.GetInt32(reader, 13);
        entities.CaseRole.Identifier = db.GetInt32(reader, 13);
        entities.LaPersonLaCaseRole.CroType = db.GetString(reader, 14);
        entities.LegalActionCaseRole.CroType = db.GetString(reader, 14);
        entities.CaseRole.Type1 = db.GetString(reader, 14);
        entities.LaPersonLaCaseRole.CspNum = db.GetString(reader, 15);
        entities.LegalActionCaseRole.CspNumber = db.GetString(reader, 15);
        entities.CaseRole.CspNumber = db.GetString(reader, 15);
        entities.LaPersonLaCaseRole.CasNum = db.GetString(reader, 16);
        entities.LegalActionCaseRole.CasNumber = db.GetString(reader, 16);
        entities.CaseRole.CasNumber = db.GetString(reader, 16);
        entities.Case1.Number = db.GetString(reader, 16);
        entities.Case1.Number = db.GetString(reader, 16);
        entities.LaPersonLaCaseRole.LgaId = db.GetInt32(reader, 17);
        entities.LegalActionCaseRole.LgaId = db.GetInt32(reader, 17);
        entities.LegalActionCaseRole.CreatedBy = db.GetString(reader, 18);
        entities.LegalActionCaseRole.CreatedTstamp = db.GetDateTime(reader, 19);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 20);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 21);
        entities.LaPersonLaCaseRole.Populated = true;
        entities.LegalActionCaseRole.Populated = true;
        entities.Case1.Populated = true;
        entities.CaseRole.Populated = true;
        entities.CsePerson.Populated = true;
        entities.LegalActionPerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
        CheckValid<LaPersonLaCaseRole>("CroType",
          entities.LaPersonLaCaseRole.CroType);
        CheckValid<LegalActionCaseRole>("CroType",
          entities.LegalActionCaseRole.CroType);
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

    private Array<CseCasesGroup> cseCases;
    private LegalAction legalAction;
    private LegalActionDetail legalActionDetail;
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
    /// A value of CountTotalSupported.
    /// </summary>
    [JsonPropertyName("countTotalSupported")]
    public Common CountTotalSupported
    {
      get => countTotalSupported ??= new();
      set => countTotalSupported = value;
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

    private Common countTotalSupported;
    private Tribunal zdelTribunal;
    private Fips zdelFips;
    private Array<ObligationPersonsGroup> obligationPersons;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of NonBlankCaseNoSpecfd.
    /// </summary>
    [JsonPropertyName("nonBlankCaseNoSpecfd")]
    public Common NonBlankCaseNoSpecfd
    {
      get => nonBlankCaseNoSpecfd ??= new();
      set => nonBlankCaseNoSpecfd = value;
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

    private Common nonBlankCaseNoSpecfd;
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
