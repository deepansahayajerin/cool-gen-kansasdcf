// Program: LE_CAB_GET_REL_CASE_UNITS_F_LDET, ID: 371986349, model: 746.
// Short name: SWE01717
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
/// A program: LE_CAB_GET_REL_CASE_UNITS_F_LDET.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This common action block returns a group view of related case units for a 
/// given legal action.
/// </para>
/// </summary>
[Serializable]
public partial class LeCabGetRelCaseUnitsFLdet: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_CAB_GET_REL_CASE_UNITS_F_LDET program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeCabGetRelCaseUnitsFLdet(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeCabGetRelCaseUnitsFLdet.
  /// </summary>
  public LeCabGetRelCaseUnitsFLdet(IContext context, Import import,
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
    // Date		By	Change Req
    // Description
    // 11-08-1996	Govind
    // Initial Creation
    // 10-08-97	R Grey	IDCR #
    // Set Infrastructure person number to obligor person number.
    // 02-10-98	R Grey	H00037888
    // Add CAU Supported Person to group view to capture
    // financial amounts.
    // 06-24-98	DJean
    // Add special reads of CASE, CASE UNIT for FEE and spousal
    // support obligations.
    // 10-01-1999  Anand Katuri PR# H00073165
    // 
    // Check for active Legal Roles and not active
    // Case Units
    // 04/04/00 DJean -  WO# 160R
    // Add flag to read all case units
    // 05/23/00 JMagat - PR#95740
    // For paternity search, include inactive children.
    // ------------------------------------------------------------
    // ---------------------------------------------------------------
    // This action block imports legal action identifier, Legal Action Detail 
    // number
    // and returns the case and case units related to the legal action detail 
    // with active obligors.
    // For paternity search, all cases and related case units will be returned.
    // For non-paternity search, all cases and related case units excluding 
    // inactive children.
    // ---------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;

    if (!ReadLegalAction())
    {
      ExitState = "LEGAL_ACTION_NF";

      return;
    }

    if (!ReadLegalActionDetail())
    {
      ExitState = "LEGAL_ACTION_DETAIL_NF";

      return;
    }

    if (AsChar(entities.LegalActionDetail.DetailType) == 'F')
    {
      if (!ReadObligationType())
      {
        // ---------------------------------------------------------------
        // Invalid obligation type. Cannot occur. If it occurs skip and
        // read the next obligation.
        // ---------------------------------------------------------------
        return;
      }
    }
    else
    {
      // ---------------------------------------------------------------
      // HIC & EP are the only non fin obligations. If any other non
      // fin oblig is added later that dont require supported person,
      // make necessary modification here.
      // ---------------------------------------------------------------
    }

    export.RelatedCaseUnits.Index = -1;

    foreach(var item in ReadLegalActionPersonCsePerson1())
    {
      if (!Equal(entities.ObligationType.Code, "FEE") && !
        Equal(entities.ObligationType.Code, "BDCK RC") && !
        Equal(entities.ObligationType.Code, "IRS NEG") && !
        Equal(entities.ObligationType.Code, "MIS AR") && !
        Equal(entities.ObligationType.Code, "MIS AP"))
      {
        if (AsChar(import.PaternitySearch.Flag) == 'Y')
        {
          // *** Paternity-related.
          // Get all case units including inactive children.
          foreach(var item1 in ReadLegalActionPersonCsePerson3())
          {
            if (Equal(entities.ExistingObligorCsePerson.Number,
              local.PreviousObligor.Number) && Equal
              (entities.ExistingSupportedCsePerson.Number,
              local.PreviousSupported.Number))
            {
              goto ReadEach;
            }

            local.Supported.CurrentAmount =
              entities.ExistingSupportedLegalActionPerson.CurrentAmount;

            foreach(var item2 in ReadCaseCaseUnit2())
            {
              if (export.RelatedCaseUnits.Index + 1 >= Export
                .RelatedCaseUnitsGroup.Capacity)
              {
                return;
              }

              ++export.RelatedCaseUnits.Index;
              export.RelatedCaseUnits.CheckSize();

              export.RelatedCaseUnits.Update.DetailRelatedCase.Number =
                entities.Case1.Number;
              export.RelatedCaseUnits.Update.DetailRelatedCaseUnit.CuNumber =
                entities.CaseUnit.CuNumber;
              local.PreviousObligor.Number =
                entities.ExistingObligorCsePerson.Number;
              export.RelatedCaseUnits.Update.DtlRelatedObligor.Number =
                entities.ExistingObligorCsePerson.Number;
              local.PreviousSupported.Number =
                entities.ExistingSupportedCsePerson.Number;
              export.RelatedCaseUnits.Update.DtlCauSupported.Number =
                entities.ExistingSupportedCsePerson.Number;
              export.RelatedCaseUnits.Update.CauSupported.CurrentAmount =
                local.Supported.CurrentAmount;
            }

            foreach(var item2 in ReadCaseCaseUnit1())
            {
              if (export.RelatedCaseUnits.Index + 1 >= Export
                .RelatedCaseUnitsGroup.Capacity)
              {
                return;
              }

              ++export.RelatedCaseUnits.Index;
              export.RelatedCaseUnits.CheckSize();

              export.RelatedCaseUnits.Update.DetailRelatedCase.Number =
                entities.Case1.Number;
              export.RelatedCaseUnits.Update.DetailRelatedCaseUnit.CuNumber =
                entities.CaseUnit.CuNumber;
              local.PreviousObligor.Number =
                entities.ExistingObligorCsePerson.Number;
              export.RelatedCaseUnits.Update.DtlRelatedObligor.Number =
                entities.ExistingObligorCsePerson.Number;
              local.PreviousSupported.Number =
                entities.ExistingSupportedCsePerson.Number;
              export.RelatedCaseUnits.Update.DtlCauSupported.Number =
                entities.ExistingSupportedCsePerson.Number;
              export.RelatedCaseUnits.Update.CauSupported.CurrentAmount =
                local.Supported.CurrentAmount;
            }
          }
        }
        else
        {
          // *** Non-paternity-related.
          foreach(var item1 in ReadLegalActionPersonCsePerson2())
          {
            if (Equal(entities.ExistingObligorCsePerson.Number,
              local.PreviousObligor.Number) && Equal
              (entities.ExistingSupportedCsePerson.Number,
              local.PreviousSupported.Number))
            {
              goto ReadEach;
            }

            local.Supported.CurrentAmount =
              entities.ExistingSupportedLegalActionPerson.CurrentAmount;
            local.CaseUnitFound.Flag = "N";

            foreach(var item2 in ReadCaseCaseUnit2())
            {
              local.CaseUnitFound.Flag = "Y";

              if (export.RelatedCaseUnits.Index + 1 >= Export
                .RelatedCaseUnitsGroup.Capacity)
              {
                return;
              }

              ++export.RelatedCaseUnits.Index;
              export.RelatedCaseUnits.CheckSize();

              export.RelatedCaseUnits.Update.DetailRelatedCase.Number =
                entities.Case1.Number;
              export.RelatedCaseUnits.Update.DetailRelatedCaseUnit.CuNumber =
                entities.CaseUnit.CuNumber;
              local.PreviousObligor.Number =
                entities.ExistingObligorCsePerson.Number;
              export.RelatedCaseUnits.Update.DtlRelatedObligor.Number =
                entities.ExistingObligorCsePerson.Number;
              local.PreviousSupported.Number =
                entities.ExistingSupportedCsePerson.Number;
              export.RelatedCaseUnits.Update.DtlCauSupported.Number =
                entities.ExistingSupportedCsePerson.Number;
              export.RelatedCaseUnits.Update.CauSupported.CurrentAmount =
                local.Supported.CurrentAmount;
            }

            if (AsChar(local.CaseUnitFound.Flag) == 'Y')
            {
              return;
            }

            foreach(var item2 in ReadCaseCaseUnit1())
            {
              local.CaseUnitFound.Flag = "Y";

              if (export.RelatedCaseUnits.Index + 1 >= Export
                .RelatedCaseUnitsGroup.Capacity)
              {
                return;
              }

              ++export.RelatedCaseUnits.Index;
              export.RelatedCaseUnits.CheckSize();

              export.RelatedCaseUnits.Update.DetailRelatedCase.Number =
                entities.Case1.Number;
              export.RelatedCaseUnits.Update.DetailRelatedCaseUnit.CuNumber =
                entities.CaseUnit.CuNumber;
              local.PreviousObligor.Number =
                entities.ExistingObligorCsePerson.Number;
              export.RelatedCaseUnits.Update.DtlRelatedObligor.Number =
                entities.ExistingObligorCsePerson.Number;
              local.PreviousSupported.Number =
                entities.ExistingSupportedCsePerson.Number;
              export.RelatedCaseUnits.Update.DtlCauSupported.Number =
                entities.ExistingSupportedCsePerson.Number;
              export.RelatedCaseUnits.Update.CauSupported.CurrentAmount =
                local.Supported.CurrentAmount;
            }
          }
        }
      }
      else
      {
        // ****************************************************************
        // * No supported person for FEEs or Bad Check Recoveries
        // ****************************************************************
        if (Equal(entities.ExistingObligorCsePerson.Number,
          local.PreviousObligor.Number))
        {
          continue;
        }

        // 10/28/99   Anand   PR# H00077727
        // Get multiple Cases associated with any Court Case Number
        // for generating Infrastructure records.
        foreach(var item1 in ReadCaseUnitCase())
        {
          if (export.RelatedCaseUnits.Index + 1 >= Export
            .RelatedCaseUnitsGroup.Capacity)
          {
            return;
          }

          ++export.RelatedCaseUnits.Index;
          export.RelatedCaseUnits.CheckSize();

          export.RelatedCaseUnits.Update.DetailRelatedCase.Number =
            entities.Case1.Number;
          export.RelatedCaseUnits.Update.DetailRelatedCaseUnit.CuNumber =
            entities.CaseUnit.CuNumber;
          local.PreviousObligor.Number =
            entities.ExistingObligorCsePerson.Number;
          export.RelatedCaseUnits.Update.DtlRelatedObligor.Number =
            entities.ExistingObligorCsePerson.Number;
          local.PreviousSupported.Number =
            entities.ExistingSupportedCsePerson.Number;
          export.RelatedCaseUnits.Update.DtlCauSupported.Number =
            entities.ExistingSupportedCsePerson.Number;
          export.RelatedCaseUnits.Update.CauSupported.CurrentAmount =
            local.Supported.CurrentAmount;
        }
      }

ReadEach:
      ;
    }
  }

  private IEnumerable<bool> ReadCaseCaseUnit1()
  {
    entities.Case1.Populated = false;
    entities.CaseUnit.Populated = false;

    return ReadEach("ReadCaseCaseUnit1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspNoAr", entities.ExistingSupportedCsePerson.Number);
        db.SetNullableString(
          command, "cspNoAp", entities.ExistingObligorCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseUnit.CasNo = db.GetString(reader, 0);
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 1);
        entities.CaseUnit.StartDate = db.GetDate(reader, 2);
        entities.CaseUnit.ClosureDate = db.GetNullableDate(reader, 3);
        entities.CaseUnit.CspNoAr = db.GetNullableString(reader, 4);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 5);
        entities.CaseUnit.CspNoChild = db.GetNullableString(reader, 6);
        entities.Case1.Populated = true;
        entities.CaseUnit.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseCaseUnit2()
  {
    entities.Case1.Populated = false;
    entities.CaseUnit.Populated = false;

    return ReadEach("ReadCaseCaseUnit2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspNoChild", entities.ExistingSupportedCsePerson.Number);
        db.SetNullableString(
          command, "cspNoAp", entities.ExistingObligorCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseUnit.CasNo = db.GetString(reader, 0);
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 1);
        entities.CaseUnit.StartDate = db.GetDate(reader, 2);
        entities.CaseUnit.ClosureDate = db.GetNullableDate(reader, 3);
        entities.CaseUnit.CspNoAr = db.GetNullableString(reader, 4);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 5);
        entities.CaseUnit.CspNoChild = db.GetNullableString(reader, 6);
        entities.Case1.Populated = true;
        entities.CaseUnit.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseUnitCase()
  {
    entities.Case1.Populated = false;
    entities.CaseUnit.Populated = false;

    return ReadEach("ReadCaseUnitCase",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspNoAp", entities.ExistingObligorCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.StartDate = db.GetDate(reader, 1);
        entities.CaseUnit.ClosureDate = db.GetNullableDate(reader, 2);
        entities.CaseUnit.CasNo = db.GetString(reader, 3);
        entities.Case1.Number = db.GetString(reader, 3);
        entities.CaseUnit.CspNoAr = db.GetNullableString(reader, 4);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 5);
        entities.CaseUnit.CspNoChild = db.GetNullableString(reader, 6);
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
        entities.LegalAction.ActionTaken = db.GetString(reader, 1);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 2);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 3);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalActionDetail()
  {
    entities.LegalActionDetail.Populated = false;

    return Read("ReadLegalActionDetail",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
        db.SetInt32(command, "laDetailNo", import.LegalActionDetail.Number);
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 2);
        entities.LegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 3);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 4);
        entities.LegalActionDetail.OtyId = db.GetNullableInt32(reader, 5);
        entities.LegalActionDetail.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);
      });
  }

  private IEnumerable<bool> ReadLegalActionPersonCsePerson1()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.ExistingObligorCsePerson.Populated = false;
    entities.ExistingObligorLegalActionPerson.Populated = false;

    return ReadEach("ReadLegalActionPersonCsePerson1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladRNumber", entities.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.LegalActionDetail.LgaIdentifier);
        db.SetNullableDate(
          command, "endDt", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingObligorLegalActionPerson.Identifier =
          db.GetInt32(reader, 0);
        entities.ExistingObligorLegalActionPerson.CspNumber =
          db.GetNullableString(reader, 1);
        entities.ExistingObligorCsePerson.Number = db.GetString(reader, 1);
        entities.ExistingObligorLegalActionPerson.EndDate =
          db.GetNullableDate(reader, 2);
        entities.ExistingObligorLegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 3);
        entities.ExistingObligorLegalActionPerson.LadRNumber =
          db.GetNullableInt32(reader, 4);
        entities.ExistingObligorLegalActionPerson.AccountType =
          db.GetNullableString(reader, 5);
        entities.ExistingObligorCsePerson.Populated = true;
        entities.ExistingObligorLegalActionPerson.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionPersonCsePerson2()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.ExistingSupportedCsePerson.Populated = false;
    entities.ExistingSupportedLegalActionPerson.Populated = false;

    return ReadEach("ReadLegalActionPersonCsePerson2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladRNumber", entities.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.LegalActionDetail.LgaIdentifier);
        db.SetNullableDate(
          command, "endDt", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingSupportedLegalActionPerson.Identifier =
          db.GetInt32(reader, 0);
        entities.ExistingSupportedLegalActionPerson.CspNumber =
          db.GetNullableString(reader, 1);
        entities.ExistingSupportedCsePerson.Number = db.GetString(reader, 1);
        entities.ExistingSupportedLegalActionPerson.Role =
          db.GetString(reader, 2);
        entities.ExistingSupportedLegalActionPerson.EndDate =
          db.GetNullableDate(reader, 3);
        entities.ExistingSupportedLegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 4);
        entities.ExistingSupportedLegalActionPerson.LadRNumber =
          db.GetNullableInt32(reader, 5);
        entities.ExistingSupportedLegalActionPerson.AccountType =
          db.GetNullableString(reader, 6);
        entities.ExistingSupportedLegalActionPerson.ArrearsAmount =
          db.GetNullableDecimal(reader, 7);
        entities.ExistingSupportedLegalActionPerson.CurrentAmount =
          db.GetNullableDecimal(reader, 8);
        entities.ExistingSupportedLegalActionPerson.JudgementAmount =
          db.GetNullableDecimal(reader, 9);
        entities.ExistingSupportedCsePerson.Populated = true;
        entities.ExistingSupportedLegalActionPerson.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionPersonCsePerson3()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.ExistingSupportedCsePerson.Populated = false;
    entities.ExistingSupportedLegalActionPerson.Populated = false;

    return ReadEach("ReadLegalActionPersonCsePerson3",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladRNumber", entities.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.LegalActionDetail.LgaIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingSupportedLegalActionPerson.Identifier =
          db.GetInt32(reader, 0);
        entities.ExistingSupportedLegalActionPerson.CspNumber =
          db.GetNullableString(reader, 1);
        entities.ExistingSupportedCsePerson.Number = db.GetString(reader, 1);
        entities.ExistingSupportedLegalActionPerson.Role =
          db.GetString(reader, 2);
        entities.ExistingSupportedLegalActionPerson.EndDate =
          db.GetNullableDate(reader, 3);
        entities.ExistingSupportedLegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 4);
        entities.ExistingSupportedLegalActionPerson.LadRNumber =
          db.GetNullableInt32(reader, 5);
        entities.ExistingSupportedLegalActionPerson.AccountType =
          db.GetNullableString(reader, 6);
        entities.ExistingSupportedLegalActionPerson.ArrearsAmount =
          db.GetNullableDecimal(reader, 7);
        entities.ExistingSupportedLegalActionPerson.CurrentAmount =
          db.GetNullableDecimal(reader, 8);
        entities.ExistingSupportedLegalActionPerson.JudgementAmount =
          db.GetNullableDecimal(reader, 9);
        entities.ExistingSupportedCsePerson.Populated = true;
        entities.ExistingSupportedLegalActionPerson.Populated = true;

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
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.SupportedPersonReqInd = db.GetString(reader, 2);
        entities.ObligationType.Populated = true;
        CheckValid<ObligationType>("SupportedPersonReqInd",
          entities.ObligationType.SupportedPersonReqInd);
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
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
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
    /// A value of PaternitySearch.
    /// </summary>
    [JsonPropertyName("paternitySearch")]
    public Common PaternitySearch
    {
      get => paternitySearch ??= new();
      set => paternitySearch = value;
    }

    private LegalActionDetail legalActionDetail;
    private LegalAction legalAction;
    private Common paternitySearch;
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

      /// <summary>
      /// A value of DtlCauSupported.
      /// </summary>
      [JsonPropertyName("dtlCauSupported")]
      public CsePerson DtlCauSupported
      {
        get => dtlCauSupported ??= new();
        set => dtlCauSupported = value;
      }

      /// <summary>
      /// A value of CauSupported.
      /// </summary>
      [JsonPropertyName("cauSupported")]
      public LegalActionPerson CauSupported
      {
        get => cauSupported ??= new();
        set => cauSupported = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Case1 detailRelatedCase;
      private CaseUnit detailRelatedCaseUnit;
      private CsePerson dtlRelatedObligor;
      private CsePerson dtlCauSupported;
      private LegalActionPerson cauSupported;
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
    /// A value of CaseUnitFound.
    /// </summary>
    [JsonPropertyName("caseUnitFound")]
    public Common CaseUnitFound
    {
      get => caseUnitFound ??= new();
      set => caseUnitFound = value;
    }

    /// <summary>
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public LegalActionPerson Supported
    {
      get => supported ??= new();
      set => supported = value;
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
    /// A value of InitialisedToZeros.
    /// </summary>
    [JsonPropertyName("initialisedToZeros")]
    public CaseUnit InitialisedToZeros
    {
      get => initialisedToZeros ??= new();
      set => initialisedToZeros = value;
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
    /// A value of PreviousObligor.
    /// </summary>
    [JsonPropertyName("previousObligor")]
    public CsePerson PreviousObligor
    {
      get => previousObligor ??= new();
      set => previousObligor = value;
    }

    private Common caseUnitFound;
    private LegalActionPerson supported;
    private DateWorkArea current;
    private CaseUnit initialisedToZeros;
    private CsePerson previousSupported;
    private CsePerson previousObligor;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ExistingSupportedCsePerson.
    /// </summary>
    [JsonPropertyName("existingSupportedCsePerson")]
    public CsePerson ExistingSupportedCsePerson
    {
      get => existingSupportedCsePerson ??= new();
      set => existingSupportedCsePerson = value;
    }

    /// <summary>
    /// A value of ExistingObligorCsePerson.
    /// </summary>
    [JsonPropertyName("existingObligorCsePerson")]
    public CsePerson ExistingObligorCsePerson
    {
      get => existingObligorCsePerson ??= new();
      set => existingObligorCsePerson = value;
    }

    /// <summary>
    /// A value of ExistingSupportedLegalActionPerson.
    /// </summary>
    [JsonPropertyName("existingSupportedLegalActionPerson")]
    public LegalActionPerson ExistingSupportedLegalActionPerson
    {
      get => existingSupportedLegalActionPerson ??= new();
      set => existingSupportedLegalActionPerson = value;
    }

    /// <summary>
    /// A value of ExistingObligorLegalActionPerson.
    /// </summary>
    [JsonPropertyName("existingObligorLegalActionPerson")]
    public LegalActionPerson ExistingObligorLegalActionPerson
    {
      get => existingObligorLegalActionPerson ??= new();
      set => existingObligorLegalActionPerson = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    private ObligationType obligationType;
    private CsePerson existingSupportedCsePerson;
    private CsePerson existingObligorCsePerson;
    private LegalActionPerson existingSupportedLegalActionPerson;
    private LegalActionPerson existingObligorLegalActionPerson;
    private LegalActionDetail legalActionDetail;
    private Case1 case1;
    private CaseUnit caseUnit;
    private LegalAction legalAction;
  }
#endregion
}
