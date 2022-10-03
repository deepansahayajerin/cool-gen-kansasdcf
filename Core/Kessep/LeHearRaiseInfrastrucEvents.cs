// Program: LE_HEAR_RAISE_INFRASTRUC_EVENTS, ID: 372012090, model: 746.
// Short name: SWE01704
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_HEAR_RAISE_INFRASTRUC_EVENTS.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block raises the event(s) for Infrastructure.
/// </para>
/// </summary>
[Serializable]
public partial class LeHearRaiseInfrastrucEvents: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_HEAR_RAISE_INFRASTRUC_EVENTS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeHearRaiseInfrastrucEvents(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeHearRaiseInfrastrucEvents.
  /// </summary>
  public LeHearRaiseInfrastrucEvents(IContext context, Import import,
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
    // *********************************************
    // Date	  By	  IDCR	Description
    // 11-13-96  Govind	Initial Coding
    // 12-31-96  Govind	Modified based on SP
    // 			doc 12-20-96
    // 09-29-97  R Grey	Add new Events, IDCR #
    // 10-08-97  R Grey	Set Infrastructure Person #
    // 11-10-97  R Grey	Add more new Events, IDCR #
    // 01-16-98  R Grey	Add more new Events
    // 11/21/98  R. Jean	Change view matching
    // *********************************************
    if (!Lt(local.InitialisedToZeros.ConductedDate, import.Hearing.ConductedDate))
      
    {
      // --- Hearing Date has not been set. So do nothing.
      return;
    }

    if (!ReadLegalAction())
    {
      ExitState = "LEGAL_ACTION_NF";

      return;
    }

    // --- All common code
    local.Infrastructure.EventId = 29;
    local.Infrastructure.CsenetInOutCode = "";
    local.Infrastructure.ProcessStatus = "Q";
    local.Infrastructure.UserId = "HEAR";
    local.Infrastructure.BusinessObjectCd = "LEA";
    local.Infrastructure.DenormNumeric12 = entities.LegalAction.Identifier;
    local.Infrastructure.DenormText12 = entities.LegalAction.CourtCaseNumber;
    local.Infrastructure.ReferenceDate = import.Hearing.ConductedDate;
    local.Infrastructure.InitiatingStateCode = "KS";
    local.Code.CodeName = "ACTION TAKEN";
    local.CodeValue.Cdvalue = entities.LegalAction.ActionTaken;
    UseCabGetCodeValueDescription();
    local.Infrastructure.Detail = local.CodeValue.Description;

    if (!Lt(local.InitialisedToZeros.OutcomeReceivedDate,
      import.Hearing.OutcomeReceivedDate))
    {
      // --- Hearing Date has been set but Outcome has not been received.
      switch(TrimEnd(entities.LegalAction.ActionTaken))
      {
        case "CONTINUM":
          local.Infrastructure.ReasonCode = "HDTCONTINUM";

          break;
        case "DELTSUPM":
          local.Infrastructure.ReasonCode = "HDTDELTSUPM";

          break;
        case "CONTEMPT":
          return;
        case "DISMISSM":
          local.Infrastructure.ReasonCode = "HDTDISMISSM";

          break;
        case "COMPENIM":
          local.Infrastructure.ReasonCode = "HDTCOMPENIM";

          break;
        case "DEFJPATM":
          local.Infrastructure.ReasonCode = "HDTDEFJPATM";

          break;
        case "MEDESTBM":
          local.Infrastructure.ReasonCode = "HDTMEDESTBM";

          break;
        case "JUDMEXPM":
          local.Infrastructure.ReasonCode = "HDTJUDMEXPM";

          break;
        case "AIDOEXMO":
          local.Infrastructure.ReasonCode = "HDTAIDOEXMO";

          break;
        case "REVIVORM":
          return;
        case "GALAMFM":
          local.Infrastructure.ReasonCode = "HDTGALAMFM";

          break;
        case "GARDADLM":
          local.Infrastructure.ReasonCode = "HDTGARDADLM";

          break;
        case "JDAGEMPM":
          local.Infrastructure.ReasonCode = "HDTJDAGEMPM";

          break;
        case "MODMSOM":
          local.Infrastructure.ReasonCode = "HDTMODMSOM";

          break;
        case "CSMODM":
          local.Infrastructure.ReasonCode = "HDTCSMODM";

          break;
        case "GENETICM":
          local.Infrastructure.ReasonCode = "HDTGENETICM";

          break;
        case "NUNCPROM":
          local.Infrastructure.ReasonCode = "HDTNUNCPROM";

          break;
        case "CONSOLDM":
          local.Infrastructure.ReasonCode = "HDTCONSOLDM";

          break;
        case "MOTOSTAY":
          local.Infrastructure.ReasonCode = "HDTMOTOSTAY";

          break;
        case "718BDEFM":
          local.Infrastructure.ReasonCode = "HDT718BDEFM";

          break;
        case "JEF":
          local.Infrastructure.ReasonCode = "HDTJEF";

          break;
        case "MOTIONKS":
          local.Infrastructure.ReasonCode = "HDTMOTIONKS";

          break;
        case "COMPELM":
          local.Infrastructure.ReasonCode = "HDTCOMPELM";

          break;
        case "LIMINEM":
          local.Infrastructure.ReasonCode = "HDTLIMINE";

          break;
        case "PROTECTM":
          local.Infrastructure.ReasonCode = "HDTPROTECTM";

          break;
        case "QUASHM":
          local.Infrastructure.ReasonCode = "HDTQUASHM";

          break;
        default:
          return;
      }

      // --- Now create Infrastructure records
      UseLeCabGetRelCaseUnitsFLact();
      local.Infrastructure.SituationNumber = 0;

      for(local.RelatedCaseUnits.Index = 0; local.RelatedCaseUnits.Index < local
        .RelatedCaseUnits.Count; ++local.RelatedCaseUnits.Index)
      {
        if (!local.RelatedCaseUnits.CheckSize())
        {
          break;
        }

        local.Infrastructure.CaseNumber =
          local.RelatedCaseUnits.Item.DetailRelatedCase.Number;
        local.Infrastructure.CaseUnitNumber =
          local.RelatedCaseUnits.Item.DetailRelatedCaseUnit.CuNumber;
        local.Infrastructure.CsePersonNumber =
          local.RelatedCaseUnits.Item.DtlRelatedObligor.Number;
        UseSpCabCreateInfrastructure();
      }

      local.RelatedCaseUnits.CheckIndex();
    }
    else
    {
      // --- Hearing outcome received. i.e. Hearing has been conducted.
      switch(TrimEnd(entities.LegalAction.ActionTaken))
      {
        case "COMPELM":
          local.Infrastructure.ReasonCode = "HEARCOMPELM";

          break;
        case "DELTSUPM":
          local.Infrastructure.ReasonCode = "HEARDELTSUPM";

          break;
        case "CONTEMPT":
          local.Infrastructure.ReasonCode = "HEARCONTEMPT";

          break;
        case "DISMISSM":
          local.Infrastructure.ReasonCode = "HEARDISMISSM";

          break;
        case "COMPENIM":
          local.Infrastructure.ReasonCode = "HEARCOMPENIM";

          break;
        case "DEFJPATM":
          local.Infrastructure.ReasonCode = "HEARDEFJPATM";

          break;
        case "MEDESTBM":
          local.Infrastructure.ReasonCode = "HEARMEDESTBM";

          break;
        case "JUDMEXPM":
          local.Infrastructure.ReasonCode = "HEARJUDMEXPM";

          break;
        case "AIDOEXMO":
          local.Infrastructure.ReasonCode = "HEARAIDOEXMO";

          break;
        case "REVIVORM":
          local.Infrastructure.ReasonCode = "HEARREVIVORM";

          break;
        case "GALAMFM":
          local.Infrastructure.ReasonCode = "HEARGALAMFM";

          break;
        case "GARDADLM":
          local.Infrastructure.ReasonCode = "HEARGARDADLM";

          break;
        case "JDAGEMPM":
          local.Infrastructure.ReasonCode = "HEARJDAGEMPM";

          break;
        case "MODMSOM":
          local.Infrastructure.ReasonCode = "HEARMODMSOM";

          break;
        case "CSMODM":
          local.Infrastructure.ReasonCode = "HEARCSMODM";

          break;
        case "GENETICM":
          local.Infrastructure.ReasonCode = "HEARGENETICM";

          break;
        case "SETARRSM":
          local.Infrastructure.ReasonCode = "HEARSETARRSM";

          break;
        case "CONSOLDM":
          local.Infrastructure.ReasonCode = "HEARCONSOLDM";

          break;
        case "MOTOSTAY":
          local.Infrastructure.ReasonCode = "HEARMOTOSTAY";

          break;
        case "718BDEFM":
          local.Infrastructure.ReasonCode = "HEAR718BDEFM";

          break;
        case "MOTIONKS":
          local.Infrastructure.ReasonCode = "HEARMOTIONKS";

          break;
        case "EXTRA1":
          break;
        case "EXTRA2":
          break;
        case "EXTRA3":
          break;
        case "EXTRA4":
          break;
        default:
          return;
      }

      // --- Now create Infrastructure records
      UseLeCabGetRelCaseUnitsFLact();
      local.Infrastructure.SituationNumber = 0;

      for(local.RelatedCaseUnits.Index = 0; local.RelatedCaseUnits.Index < local
        .RelatedCaseUnits.Count; ++local.RelatedCaseUnits.Index)
      {
        if (!local.RelatedCaseUnits.CheckSize())
        {
          break;
        }

        local.Infrastructure.CaseNumber =
          local.RelatedCaseUnits.Item.DetailRelatedCase.Number;
        local.Infrastructure.CaseUnitNumber =
          local.RelatedCaseUnits.Item.DetailRelatedCaseUnit.CuNumber;
        local.Infrastructure.CsePersonNumber =
          local.RelatedCaseUnits.Item.DtlRelatedObligor.Number;
        UseSpCabCreateInfrastructure();
      }

      local.RelatedCaseUnits.CheckIndex();
    }
  }

  private static void MoveCode(Code source, Code target)
  {
    target.Id = source.Id;
    target.CodeName = source.CodeName;
  }

  private static void MoveCodeValue(CodeValue source, CodeValue target)
  {
    target.Cdvalue = source.Cdvalue;
    target.Description = source.Description;
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.EventDetailName = source.EventDetailName;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormText12 = source.DenormText12;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.CsenetInOutCode = source.CsenetInOutCode;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.UserId = source.UserId;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private static void MoveRelatedCaseUnits(LeCabGetRelCaseUnitsFLact.Export.
    RelatedCaseUnitsGroup source, Local.RelatedCaseUnitsGroup target)
  {
    target.DetailRelatedCase.Number = source.DetailRelatedCase.Number;
    target.DetailRelatedCaseUnit.CuNumber =
      source.DetailRelatedCaseUnit.CuNumber;
    target.DtlRelatedObligor.Number = source.DtlRelatedObligor.Number;
  }

  private void UseCabGetCodeValueDescription()
  {
    var useImport = new CabGetCodeValueDescription.Import();
    var useExport = new CabGetCodeValueDescription.Export();

    MoveCode(local.Code, useImport.Code);
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabGetCodeValueDescription.Execute, useImport, useExport);

    MoveCodeValue(useExport.CodeValue, local.CodeValue);
  }

  private void UseLeCabGetRelCaseUnitsFLact()
  {
    var useImport = new LeCabGetRelCaseUnitsFLact.Import();
    var useExport = new LeCabGetRelCaseUnitsFLact.Export();

    useImport.LegalAction.Identifier = import.LegalAction.Identifier;

    Call(LeCabGetRelCaseUnitsFLact.Execute, useImport, useExport);

    useExport.RelatedCaseUnits.CopyTo(
      local.RelatedCaseUnits, MoveRelatedCaseUnits);
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    MoveInfrastructure(local.Infrastructure, useImport.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    local.Infrastructure.Assign(useExport.Infrastructure);
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
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.InitiatingState = db.GetNullableString(reader, 4);
        entities.LegalAction.RespondingState = db.GetNullableString(reader, 5);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 6);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 7);
        entities.LegalAction.Populated = true;
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
    /// A value of Hearing.
    /// </summary>
    [JsonPropertyName("hearing")]
    public Hearing Hearing
    {
      get => hearing ??= new();
      set => hearing = value;
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

    private Hearing hearing;
    private LegalAction legalAction;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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
    /// A value of InitialisedToZeros.
    /// </summary>
    [JsonPropertyName("initialisedToZeros")]
    public Hearing InitialisedToZeros
    {
      get => initialisedToZeros ??= new();
      set => initialisedToZeros = value;
    }

    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

    /// <summary>
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
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

    /// <summary>
    /// A value of ControlTable.
    /// </summary>
    [JsonPropertyName("controlTable")]
    public ControlTable ControlTable
    {
      get => controlTable ??= new();
      set => controlTable = value;
    }

    private Hearing initialisedToZeros;
    private Infrastructure infrastructure;
    private Code code;
    private CodeValue codeValue;
    private Array<RelatedCaseUnitsGroup> relatedCaseUnits;
    private ControlTable controlTable;
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

    private LegalAction legalAction;
  }
#endregion
}
