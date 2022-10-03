// Program: LE_BFX1_GET_REL_CASE_UNITS, ID: 373502264, model: 746.
// Short name: SWE02006
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_BFX1_GET_REL_CASE_UNITS.
/// </summary>
[Serializable]
public partial class LeBfx1GetRelCaseUnits: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_BFX1_GET_REL_CASE_UNITS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeBfx1GetRelCaseUnits(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeBfx1GetRelCaseUnits.
  /// </summary>
  public LeBfx1GetRelCaseUnits(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // --------------------------------------------------------------
    // PURPOSE: Given the legal action identifier, it returns
    // only active roles and all related case and case units.
    // It will use LOPS Legal Obligation Person info, if available.
    // Otherwise, the Legal ROLE info is used.
    // ---------------------------------------------------------------
    local.Current.Date = import.DateWorkArea.Date;

    if (!ReadLegalAction())
    {
      ExitState = "LEGAL_ACTION_NF";

      return;
    }

    local.LopsDetailsExist.Flag = "N";
    export.RelatedCaseUnits.Index = -1;

    foreach(var item in ReadLegalActionPersonLegalActionPersonCsePerson())
    {
      local.LopsDetailsExist.Flag = "Y";

      if (Equal(entities.ObligorCsePerson.Number, local.PreviousObligor.Number) &&
        Equal
        (entities.SupportedCsePerson.Number, local.PreviousSupported.Number))
      {
        // *** Duplicate obligor-supported person combination.
        continue;
      }

      if (AsChar(entities.LegalActionDetail.DetailType) == 'F')
      {
        if (ReadObligationType())
        {
          if (AsChar(entities.ObligationType.SupportedPersonReqInd) == 'N')
          {
            // *** No supported person is required for this obligation type.
            continue;
          }
        }
        else
        {
          // *** Invalid obligation type.
          continue;
        }
      }
      else
      {
        // *** HIC,EP are the only non financial obligation.
      }

      // *** PR#H00077727 - Identified one obligor-supported person.
      // Get the corresponding case/case unit.
      foreach(var item1 in ReadCaseUnitCase())
      {
        if (export.RelatedCaseUnits.Index + 1 >= Export
          .RelatedCaseUnitsGroup.Capacity)
        {
          goto ReadEach;
        }

        ++export.RelatedCaseUnits.Index;
        export.RelatedCaseUnits.CheckSize();

        export.RelatedCaseUnits.Update.DetailRelatedCase.Number =
          entities.Case1.Number;
        export.RelatedCaseUnits.Update.DetailRelatedCaseUnit.CuNumber =
          entities.CaseUnit.CuNumber;
        local.PreviousObligor.Number = entities.ObligorCsePerson.Number;
        export.RelatedCaseUnits.Update.DtlRelatedObligor.Number =
          entities.ObligorCsePerson.Number;
        local.PreviousSupported.Number = entities.SupportedCsePerson.Number;
      }
    }

ReadEach:

    if (AsChar(local.LopsDetailsExist.Flag) == 'Y')
    {
      // *** LOPS info retrieved.
      return;
    }

    // *** The legal action does not have Legal Obligation Persons specified.
    // Derive Case Units using Legal Action Case Roles associated with AP and CH
    // roles.
    foreach(var item in ReadLegalActionCaseRoleCsePersonLegalActionCaseRole())
    {
      if (export.RelatedCaseUnits.Index + 1 >= Export
        .RelatedCaseUnitsGroup.Capacity)
      {
        return;
      }

      // *** A supported person is assigned a unique case and
      // case_unit combination. No need for case_unit check.
      if (Equal(entities.ObligorCsePerson.Number, local.PreviousObligor.Number) &&
        Equal
        (entities.SupportedCsePerson.Number, local.PreviousSupported.Number) &&
        Equal(entities.Case1.Number, local.Previous.Number))
      {
        // *** Skip duplicate.
        continue;
      }

      ++export.RelatedCaseUnits.Index;
      export.RelatedCaseUnits.CheckSize();

      export.RelatedCaseUnits.Update.DetailRelatedCase.Number =
        entities.Case1.Number;
      export.RelatedCaseUnits.Update.DetailRelatedCaseUnit.CuNumber =
        entities.CaseUnit.CuNumber;
      local.Previous.Number = entities.Case1.Number;
      local.PreviousObligor.Number = entities.ObligorCsePerson.Number;
      export.RelatedCaseUnits.Update.DtlRelatedObligor.Number =
        entities.ObligorCsePerson.Number;
      local.PreviousSupported.Number = entities.SupportedCsePerson.Number;
    }
  }

  private IEnumerable<bool> ReadCaseUnitCase()
  {
    entities.Case1.Populated = false;
    entities.CaseUnit.Populated = false;

    return ReadEach("ReadCaseUnitCase",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspNoChild", entities.SupportedCsePerson.Number);
        db.SetNullableString(
          command, "cspNoAp", entities.ObligorCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.CasNo = db.GetString(reader, 1);
        entities.Case1.Number = db.GetString(reader, 1);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 2);
        entities.CaseUnit.CspNoChild = db.GetNullableString(reader, 3);
        entities.Case1.Populated = true;
        entities.CaseUnit.Populated = true;

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
        entities.LegalAction.Populated = true;
      });
  }

  private IEnumerable<bool>
    ReadLegalActionCaseRoleCsePersonLegalActionCaseRole()
  {
    entities.ObligorCsePerson.Populated = false;
    entities.SupportedCsePerson.Populated = false;
    entities.Case1.Populated = false;
    entities.CaseUnit.Populated = false;
    entities.ApLegalActionCaseRole.Populated = false;
    entities.SupportedLegalActionCaseRole.Populated = false;
    entities.ApCaseRole.Populated = false;
    entities.Child.Populated = false;

    return ReadEach("ReadLegalActionCaseRoleCsePersonLegalActionCaseRole",
      (db, command) =>
      {
        db.SetInt32(command, "lgaId", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.ApLegalActionCaseRole.CasNumber = db.GetString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 0);
        entities.ApCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ApCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ApLegalActionCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ObligorCsePerson.Number = db.GetString(reader, 1);
        entities.ApCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ApCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ApLegalActionCaseRole.CroType = db.GetString(reader, 2);
        entities.ApCaseRole.Type1 = db.GetString(reader, 2);
        entities.ApLegalActionCaseRole.CroIdentifier = db.GetInt32(reader, 3);
        entities.ApCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ApLegalActionCaseRole.LgaId = db.GetInt32(reader, 4);
        entities.ApLegalActionCaseRole.CreatedTstamp =
          db.GetDateTime(reader, 5);
        entities.SupportedLegalActionCaseRole.CasNumber =
          db.GetString(reader, 6);
        entities.Child.CasNumber = db.GetString(reader, 6);
        entities.SupportedLegalActionCaseRole.CspNumber =
          db.GetString(reader, 7);
        entities.SupportedCsePerson.Number = db.GetString(reader, 7);
        entities.Child.CspNumber = db.GetString(reader, 7);
        entities.Child.CspNumber = db.GetString(reader, 7);
        entities.SupportedLegalActionCaseRole.CroType = db.GetString(reader, 8);
        entities.Child.Type1 = db.GetString(reader, 8);
        entities.SupportedLegalActionCaseRole.CroIdentifier =
          db.GetInt32(reader, 9);
        entities.Child.Identifier = db.GetInt32(reader, 9);
        entities.SupportedLegalActionCaseRole.LgaId = db.GetInt32(reader, 10);
        entities.SupportedLegalActionCaseRole.CreatedTstamp =
          db.GetDateTime(reader, 11);
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 12);
        entities.CaseUnit.CasNo = db.GetString(reader, 13);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 14);
        entities.CaseUnit.CspNoChild = db.GetNullableString(reader, 15);
        entities.ObligorCsePerson.Populated = true;
        entities.SupportedCsePerson.Populated = true;
        entities.Case1.Populated = true;
        entities.CaseUnit.Populated = true;
        entities.ApLegalActionCaseRole.Populated = true;
        entities.SupportedLegalActionCaseRole.Populated = true;
        entities.ApCaseRole.Populated = true;
        entities.Child.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionPersonLegalActionPersonCsePerson()
  {
    entities.ObligorCsePerson.Populated = false;
    entities.SupportedCsePerson.Populated = false;
    entities.LegalActionDetail.Populated = false;
    entities.SupportedLegalActionPerson.Populated = false;
    entities.ObligorLegalActionPerson.Populated = false;

    return ReadEach("ReadLegalActionPersonLegalActionPersonCsePerson",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
        db.SetNullableDate(
          command, "endDt", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.SupportedLegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.SupportedLegalActionPerson.CspNumber =
          db.GetNullableString(reader, 1);
        entities.SupportedCsePerson.Number = db.GetString(reader, 1);
        entities.SupportedLegalActionPerson.EndDate =
          db.GetNullableDate(reader, 2);
        entities.SupportedLegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 3);
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 3);
        entities.SupportedLegalActionPerson.LadRNumber =
          db.GetNullableInt32(reader, 4);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 4);
        entities.SupportedLegalActionPerson.AccountType =
          db.GetNullableString(reader, 5);
        entities.ObligorLegalActionPerson.Identifier = db.GetInt32(reader, 6);
        entities.ObligorLegalActionPerson.CspNumber =
          db.GetNullableString(reader, 7);
        entities.ObligorCsePerson.Number = db.GetString(reader, 7);
        entities.ObligorLegalActionPerson.EndDate =
          db.GetNullableDate(reader, 8);
        entities.ObligorLegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 9);
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 9);
        entities.ObligorLegalActionPerson.LadRNumber =
          db.GetNullableInt32(reader, 10);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 10);
        entities.ObligorLegalActionPerson.AccountType =
          db.GetNullableString(reader, 11);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 12);
        entities.LegalActionDetail.OtyId = db.GetNullableInt32(reader, 13);
        entities.ObligorCsePerson.Populated = true;
        entities.SupportedCsePerson.Populated = true;
        entities.LegalActionDetail.Populated = true;
        entities.SupportedLegalActionPerson.Populated = true;
        entities.ObligorLegalActionPerson.Populated = true;

        return true;
      });
  }

  private bool ReadObligationType()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetInt32(
          command, "debtTypId",
          entities.LegalActionDetail.OtyId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.SupportedPersonReqInd = db.GetString(reader, 1);
        entities.ObligationType.Populated = true;
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
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
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

    private DateWorkArea dateWorkArea;
    private LegalAction legalAction;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A RelatedCaseUnitsGroup group.</summary>
    [Serializable]
    public class RelatedCaseUnitsGroup
    {
      /// <summary>
      /// A value of DetailRelatedCase.
      /// </summary>
      [JsonPropertyName("detailRelatedCase")]
      public Case1 DetailRelatedCase
      {
        get => detailRelatedCase ??= new();
        set => detailRelatedCase = value;
      }

      /// <summary>
      /// A value of DetailRelatedCaseUnit.
      /// </summary>
      [JsonPropertyName("detailRelatedCaseUnit")]
      public CaseUnit DetailRelatedCaseUnit
      {
        get => detailRelatedCaseUnit ??= new();
        set => detailRelatedCaseUnit = value;
      }

      /// <summary>
      /// A value of DtlRelatedObligor.
      /// </summary>
      [JsonPropertyName("dtlRelatedObligor")]
      public CsePerson DtlRelatedObligor
      {
        get => dtlRelatedObligor ??= new();
        set => dtlRelatedObligor = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Case1 detailRelatedCase;
      private CaseUnit detailRelatedCaseUnit;
      private CsePerson dtlRelatedObligor;
    }

    /// <summary>
    /// Gets a value of RelatedCaseUnits.
    /// </summary>
    [JsonIgnore]
    public Array<RelatedCaseUnitsGroup> RelatedCaseUnits =>
      relatedCaseUnits ??= new(RelatedCaseUnitsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of RelatedCaseUnits for json serialization.
    /// </summary>
    [JsonPropertyName("relatedCaseUnits")]
    [Computed]
    public IList<RelatedCaseUnitsGroup> RelatedCaseUnits_Json
    {
      get => relatedCaseUnits;
      set => RelatedCaseUnits.Assign(value);
    }

    private Array<RelatedCaseUnitsGroup> relatedCaseUnits;
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
    /// A value of LopsDetailsExist.
    /// </summary>
    [JsonPropertyName("lopsDetailsExist")]
    public Common LopsDetailsExist
    {
      get => lopsDetailsExist ??= new();
      set => lopsDetailsExist = value;
    }

    /// <summary>
    /// A value of PreviousObligor.
    /// </summary>
    [JsonPropertyName("previousObligor")]
    public CsePerson PreviousObligor
    {
      get => previousObligor ??= new();
      set => previousObligor = value;
    }

    /// <summary>
    /// A value of PreviousSupported.
    /// </summary>
    [JsonPropertyName("previousSupported")]
    public CsePerson PreviousSupported
    {
      get => previousSupported ??= new();
      set => previousSupported = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public Case1 Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    private DateWorkArea current;
    private Common lopsDetailsExist;
    private CsePerson previousObligor;
    private CsePerson previousSupported;
    private Case1 previous;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ObligorCsePerson.
    /// </summary>
    [JsonPropertyName("obligorCsePerson")]
    public CsePerson ObligorCsePerson
    {
      get => obligorCsePerson ??= new();
      set => obligorCsePerson = value;
    }

    /// <summary>
    /// A value of SupportedCsePerson.
    /// </summary>
    [JsonPropertyName("supportedCsePerson")]
    public CsePerson SupportedCsePerson
    {
      get => supportedCsePerson ??= new();
      set => supportedCsePerson = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
    }

    /// <summary>
    /// A value of SupportedLegalActionPerson.
    /// </summary>
    [JsonPropertyName("supportedLegalActionPerson")]
    public LegalActionPerson SupportedLegalActionPerson
    {
      get => supportedLegalActionPerson ??= new();
      set => supportedLegalActionPerson = value;
    }

    /// <summary>
    /// A value of ObligorLegalActionPerson.
    /// </summary>
    [JsonPropertyName("obligorLegalActionPerson")]
    public LegalActionPerson ObligorLegalActionPerson
    {
      get => obligorLegalActionPerson ??= new();
      set => obligorLegalActionPerson = value;
    }

    /// <summary>
    /// A value of ApLegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("apLegalActionCaseRole")]
    public LegalActionCaseRole ApLegalActionCaseRole
    {
      get => apLegalActionCaseRole ??= new();
      set => apLegalActionCaseRole = value;
    }

    /// <summary>
    /// A value of SupportedLegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("supportedLegalActionCaseRole")]
    public LegalActionCaseRole SupportedLegalActionCaseRole
    {
      get => supportedLegalActionCaseRole ??= new();
      set => supportedLegalActionCaseRole = value;
    }

    /// <summary>
    /// A value of ApCaseRole.
    /// </summary>
    [JsonPropertyName("apCaseRole")]
    public CaseRole ApCaseRole
    {
      get => apCaseRole ??= new();
      set => apCaseRole = value;
    }

    /// <summary>
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CaseRole Child
    {
      get => child ??= new();
      set => child = value;
    }

    private LegalAction legalAction;
    private CsePerson obligorCsePerson;
    private CsePerson supportedCsePerson;
    private ObligationType obligationType;
    private LegalActionDetail legalActionDetail;
    private Case1 case1;
    private CaseUnit caseUnit;
    private LegalActionPerson supportedLegalActionPerson;
    private LegalActionPerson obligorLegalActionPerson;
    private LegalActionCaseRole apLegalActionCaseRole;
    private LegalActionCaseRole supportedLegalActionCaseRole;
    private CaseRole apCaseRole;
    private CaseRole child;
  }
#endregion
}
