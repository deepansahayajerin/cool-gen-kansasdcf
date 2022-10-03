// Program: LE_LRES_RAISE_INFRASTRUC_EVENTS, ID: 371981364, model: 746.
// Short name: SWE01842
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_LRES_RAISE_INFRASTRUC_EVENTS.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block raises the event(s) for Infrastructure.
/// This action block is not used.
/// </para>
/// </summary>
[Serializable]
public partial class LeLresRaiseInfrastrucEvents: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_LRES_RAISE_INFRASTRUC_EVENTS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeLresRaiseInfrastrucEvents(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeLresRaiseInfrastrucEvents.
  /// </summary>
  public LeLresRaiseInfrastrucEvents(IContext context, Import import,
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
    // Date	  By	IDCR	Description
    // 11-12-96  Govind	Initial coding
    // 12-31-96  Govind	Modified based on SP
    // 			Doc 12-20-96
    // 09-29-97  R Grey	IDCR # ? Add new Events
    // 10-08-97  R Grey	Set Infrastructure Person #
    // 11-13-97  R Grey	Add new action taken events as per IDCR #
    // 01-16-98  R Grey 	Add new action taken events
    // *********************************************
    if (!ReadLegalAction())
    {
      ExitState = "ZD_LEGAL_ACTION_NF_3";

      return;
    }

    // --- All common code
    local.Infrastructure.CsenetInOutCode = "";
    local.Infrastructure.ProcessStatus = "Q";
    local.Infrastructure.UserId = "LRES";
    local.Infrastructure.BusinessObjectCd = "LEA";
    local.Infrastructure.DenormNumeric12 = entities.LegalAction.Identifier;
    local.Infrastructure.DenormText12 = entities.LegalAction.CourtCaseNumber;
    local.Infrastructure.InitiatingStateCode = "KS";
    local.Infrastructure.CreatedBy = global.UserId;
    local.Infrastructure.CreatedTimestamp = Now();
    local.Code.CodeName = "ACTION TAKEN";
    local.CodeValue.Cdvalue = entities.LegalAction.ActionTaken;
    UseCabGetCodeValueDescription();
    local.Infrastructure.Detail = local.CodeValue.Description;
    local.Infrastructure.SituationNumber = 0;
    local.Infrastructure.EventId = 39;
    local.Infrastructure.ReferenceDate =
      import.LegalActionResponse.ReceivedDate;

    switch(TrimEnd(entities.LegalAction.ActionTaken))
    {
      case "ENROLLM":
        local.Infrastructure.ReasonCode = "SA_ENROLLM";

        break;
      case "NOIIWON":
        local.Infrastructure.ReasonCode = "SA_NOIIWON";

        break;
      case "GARNO":
        local.Infrastructure.ReasonCode = "SA_GARNO";

        break;
      case "IWO":
        local.Infrastructure.ReasonCode = "SA_IWO";

        break;
      case "EMPANS":
        local.Infrastructure.ReasonCode = "SA_EMPANS";

        break;
      case "SUPPORTP":
        local.Infrastructure.ReasonCode = "SA_SUPPORTP";

        break;
      case "DET1PATP":
        local.Infrastructure.ReasonCode = "SA_DET1PATP";

        break;
      case "DET2PATP":
        local.Infrastructure.ReasonCode = "SA_DET2PATP";

        break;
      case "RIMB718P":
        local.Infrastructure.ReasonCode = "SA_RIMB718P";

        break;
      case "IWONOTKM":
        local.Infrastructure.ReasonCode = "SA_IWONOTKM";

        break;
      case "IWONOTKS":
        local.Infrastructure.ReasonCode = "SA_IWONOTKS";

        break;
      case "IWOMODM":
        local.Infrastructure.ReasonCode = "SA_IWOMODM";

        break;
      case "MEDICALA":
        local.Infrastructure.ReasonCode = "SA_MEDICALA";

        break;
      case "MWO":
        local.Infrastructure.ReasonCode = "SA_MWO";

        break;
      case "ISSMWON":
        local.Infrastructure.ReasonCode = "SA_IISSMWON";

        break;
      case "REQMWO":
        local.Infrastructure.ReasonCode = "SA_REQMWO";

        break;
      case "IWOISTN":
        local.Infrastructure.ReasonCode = "SA_IWOISTN";

        break;
      case "GARNRQW":
        local.Infrastructure.ReasonCode = "SAGARNRQW";

        break;
      case "GARNRQNW":
        local.Infrastructure.ReasonCode = "SAGARNRQNW";

        break;
      case "ORDIWO2":
        local.Infrastructure.ReasonCode = "SAORDIWO2";

        break;
      case "CSONLYP":
        local.Infrastructure.ReasonCode = "SACSONLYP";

        break;
      case "PATCSONP":
        local.Infrastructure.ReasonCode = "SAPATCSONP";

        break;
      case "IWOMODO":
        local.Infrastructure.ReasonCode = "SAIWOMODO";

        break;
      default:
        return;
    }

    // --- Now create Infrastructure records
    UseLeCabGetRelCaseUnitsFLact();

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

    useImport.LegalAction.Identifier = entities.LegalAction.Identifier;

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
    /// A value of LegalActionResponse.
    /// </summary>
    [JsonPropertyName("legalActionResponse")]
    public LegalActionResponse LegalActionResponse
    {
      get => legalActionResponse ??= new();
      set => legalActionResponse = value;
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

    private LegalActionResponse legalActionResponse;
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
