// Program: LE_DISC_RAISE_INFRASTRUC_EVENTS, ID: 372025877, model: 746.
// Short name: SWE01840
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
/// A program: LE_DISC_RAISE_INFRASTRUC_EVENTS.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block raises the event(s) for Infrastructure.
/// This action block has been superseded and no longer used
/// </para>
/// </summary>
[Serializable]
public partial class LeDiscRaiseInfrastrucEvents: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_DISC_RAISE_INFRASTRUC_EVENTS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeDiscRaiseInfrastrucEvents(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeDiscRaiseInfrastrucEvents.
  /// </summary>
  public LeDiscRaiseInfrastrucEvents(IContext context, Import import,
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
    // Date	  By	 IDCR #	Description
    // 11-12-96  Govind	Initial Coding.
    // 12-31-96  Govind	Modified based on SP
    //                         doc dated 12-20-96
    // 09-29-97  R Grey	Add new Events, IDCR #
    // 10-08-97  R Grey	Set Infrastructure Person #
    // 11-13-97  R Grey	Add new Events, IDCR #
    // 11/04/98  R. Jean       Eliminated unnecessary reads
    // 05/29/2007   G. Pan   PR167327  Modified to set requeted_date to
    //                                      
    // reference_date in Infrastructure
    // table
    //                                      
    // when F6 update.
    // *************************************************************
    if (!ReadLegalAction())
    {
      ExitState = "LEGAL_ACTION_NF";

      return;
    }

    // --- All common code
    local.Infrastructure.CsenetInOutCode = "";
    local.Infrastructure.ProcessStatus = "Q";
    local.Infrastructure.UserId = "DISC";
    local.Infrastructure.BusinessObjectCd = "LEA";
    local.Infrastructure.DenormNumeric12 = entities.LegalAction.Identifier;
    local.Infrastructure.DenormText12 = entities.LegalAction.CourtCaseNumber;
    local.Infrastructure.InitiatingStateCode = "KS";
    local.Code.CodeName = "ACTION TAKEN";
    local.CodeValue.Cdvalue = entities.LegalAction.ActionTaken;
    UseCabGetCodeValueDescription();
    local.Infrastructure.Detail = local.CodeValue.Description;
    local.Infrastructure.SituationNumber = 0;

    switch(AsChar(import.Discovery.RequestedByCseInd))
    {
      case 'Y':
        local.Infrastructure.EventId = 36;

        if (Lt(local.InitialisedToZeros.ResponseDate,
          import.Discovery.ResponseDate))
        {
          local.Infrastructure.ReferenceDate = import.Discovery.RequestedDate;

          // ********************************************
          // A response to an existing discovery document which was filed by 
          // Kansas CSE is being recorded.
          // ********************************************
          switch(TrimEnd(entities.LegalAction.ActionTaken))
          {
            case "COMPELM":
              local.Infrastructure.ReasonCode = "COMPELM_RSP";

              break;
            case "PRODDOCD":
              local.Infrastructure.ReasonCode = "PRODDOCD_RSP";

              break;
            case "ADMREQD":
              local.Infrastructure.ReasonCode = "ADMREQD_RSP";

              break;
            case "INTEROGD":
              local.Infrastructure.ReasonCode = "INTEROGD_RSP";

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
        }
        else
        {
          // **** THIS PROCESSING WILL NOW BE HANDLED IN LACT
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

        break;
      case 'N':
        local.Infrastructure.EventId = 37;

        // ****************************************
        // 05/29/2007   G. Pan  PR167327
        // ****************************************
        local.Infrastructure.ReferenceDate = import.Discovery.RequestedDate;

        if (Lt(local.InitialisedToZeros.ResponseDate,
          import.Discovery.ResponseDate))
        {
          // ********************************************
          // A response to an existing discovery document which was filed by
          // NON Kansas CSE (or private attorney) is being recorded.
          // ********************************************
          switch(TrimEnd(entities.LegalAction.ActionTaken))
          {
            case "INTERPET":
              local.Infrastructure.ReasonCode = "RESINTERPET";

              break;
            case "INTERSP":
              local.Infrastructure.ReasonCode = "RESINTERSP";

              break;
            case "INTERST":
              local.Infrastructure.ReasonCode = "RESINTERST";

              break;
            case "PROTECTM":
              local.Infrastructure.ReasonCode = "RESPROTECTM";

              break;
            case "QUASHM":
              local.Infrastructure.ReasonCode = "RESQUASHM";

              break;
            case "LIMINEM":
              local.Infrastructure.ReasonCode = "RESLIMINEM";

              break;
            case "COMPDISM":
              local.Infrastructure.ReasonCode = "RESCOMPDISM";

              break;
            case "PRODDOCR":
              local.Infrastructure.ReasonCode = "RESPRODDOCR";

              break;
            case "AMREQNK":
              local.Infrastructure.ReasonCode = "RESAMREQNK";

              break;
            case "DEPOREQ":
              local.Infrastructure.ReasonCode = "RESDEPOREQ";

              break;
            case "SUBPOENA":
              local.Infrastructure.ReasonCode = "RESSUBPOENA";

              break;
            case "EXTRA1":
              break;
            case "EXTRA2":
              break;
            case "EXTRA3":
              break;
            default:
              return;
          }
        }
        else
        {
          // ********************************************
          // The discovery legal action is filed on LACT but the Discovery_Filed
          // Event is raised from DISC when the discovery request record is
          // created.
          // ********************************************
          if (Equal(entities.LegalAction.ActionTaken, "INTERPET") || Equal
            (entities.LegalAction.ActionTaken, "INTERSP") || Equal
            (entities.LegalAction.ActionTaken, "INTERST") || Equal
            (entities.LegalAction.ActionTaken, "PROTECTM") || Equal
            (entities.LegalAction.ActionTaken, "QUASHM") || Equal
            (entities.LegalAction.ActionTaken, "LIMINEM") || Equal
            (entities.LegalAction.ActionTaken, "COMPDISM") || Equal
            (entities.LegalAction.ActionTaken, "AMREQNK") || Equal
            (entities.LegalAction.ActionTaken, "DEPOREQ") || Equal
            (entities.LegalAction.ActionTaken, "SUBPOENA") || Equal
            (entities.LegalAction.ActionTaken, "PRODDOCR"))
          {
            local.Infrastructure.ReasonCode = "F" + TrimEnd
              (entities.LegalAction.ActionTaken);
          }
          else
          {
            return;
          }
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

        break;
      default:
        ExitState = "LE0000_INVALID_RESP_REQ_BY_CSE";

        break;
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
    /// A value of Discovery.
    /// </summary>
    [JsonPropertyName("discovery")]
    public Discovery Discovery
    {
      get => discovery ??= new();
      set => discovery = value;
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

    private Discovery discovery;
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
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public DateWorkArea MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
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
    /// A value of InitialisedToZeros.
    /// </summary>
    [JsonPropertyName("initialisedToZeros")]
    public Discovery InitialisedToZeros
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
    /// A value of ControlTable.
    /// </summary>
    [JsonPropertyName("controlTable")]
    public ControlTable ControlTable
    {
      get => controlTable ??= new();
      set => controlTable = value;
    }

    private DateWorkArea maxDate;
    private Array<RelatedCaseUnitsGroup> relatedCaseUnits;
    private Discovery initialisedToZeros;
    private Infrastructure infrastructure;
    private Code code;
    private CodeValue codeValue;
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
