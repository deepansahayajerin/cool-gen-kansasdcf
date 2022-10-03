// Program: LE_SERV_RAISE_INFRASTRUC_EVENTS, ID: 372015479, model: 746.
// Short name: SWE01843
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_SERV_RAISE_INFRASTRUC_EVENTS.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block raises the event(s) for Infrastructure.
/// This action block has been superseded.
/// </para>
/// </summary>
[Serializable]
public partial class LeServRaiseInfrastrucEvents: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_SERV_RAISE_INFRASTRUC_EVENTS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeServRaiseInfrastrucEvents(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeServRaiseInfrastrucEvents.
  /// </summary>
  public LeServRaiseInfrastrucEvents(IContext context, Import import,
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
    // *********************************************************
    // Date	  By	IDCR #	Description
    // 11-12-96  govind	Initial Coding
    // 12-31-96  govind	Modified based on doc
    //                         dated 12-20-96.
    // 09-29-97  R Grey	New Event Details / IDCR #
    // 10-08-97  R Grey	Set Infrastructure Person #
    // 11/18/98  R.Jean        Remove SET CREATED...
    // 			statements; view match with
    // 			IMPORT LEGAL ACTION
    // 08/21/2000	PMcElderry
    // PR # 100583 - Event 41, Detail numbers 340, 341, 342,
    // 345, and 640 need to have the end date on their moniitored
    // activity referenced by the service date.
    // *********************************************************
    if (!ReadLegalAction())
    {
      ExitState = "LEGAL_ACTION_NF";

      return;
    }

    // --- All common code
    local.Infrastructure.CsenetInOutCode = "";
    local.Infrastructure.ProcessStatus = "Q";
    local.Infrastructure.UserId = "SERV";
    local.Infrastructure.BusinessObjectCd = "LEA";
    local.Infrastructure.DenormNumeric12 = entities.LegalAction.Identifier;
    local.Infrastructure.DenormText12 = entities.LegalAction.CourtCaseNumber;
    local.Infrastructure.InitiatingStateCode = "KS";
    local.Code.CodeName = "ACTION TAKEN";
    local.CodeValue.Cdvalue = entities.LegalAction.ActionTaken;
    UseCabGetCodeValueDescription();
    local.Infrastructure.Detail = local.CodeValue.Description;
    local.Infrastructure.SituationNumber = 0;

    // ----------------------------------------------------------
    // Check if it is a Service Request or the document has been
    // served.
    // ----------------------------------------------------------
    if (Equal(import.ServiceProcess.ServiceDate,
      local.InitializedToZeros.ServiceDate))
    {
      // ---------------------------------------------
      // Service is requested. It has not been served
      // ---------------------------------------------
      local.Infrastructure.EventId = 40;
      local.Infrastructure.ReferenceDate =
        import.ServiceProcess.ServiceRequestDate;

      if (Equal(global.Command, "ADD"))
      {
        // ---------------------------------------------
        // Dont raise this event if an Update is done
        // ---------------------------------------------
        switch(TrimEnd(entities.LegalAction.ActionTaken))
        {
          case "MWO":
            local.Infrastructure.ReasonCode = "SRQ_MWO";

            break;
          case "AIDOEXMO":
            local.Infrastructure.ReasonCode = "SRQ_AIDOEXMO";

            break;
          case "IWO":
            local.Infrastructure.ReasonCode = "SRQ_IWO";

            break;
          case "IWOMODO":
            local.Infrastructure.ReasonCode = "SRQ_IWOMODO";

            break;
          case "IWOTERM":
            local.Infrastructure.ReasonCode = "SRQ_IWOTERM";

            break;
          case "DET1PATP":
            local.Infrastructure.ReasonCode = "SRQ_DET1PATP";

            break;
          case "DET2PATP":
            local.Infrastructure.ReasonCode = "SRQ_DET2PATP";

            break;
          case "RIMB718P":
            local.Infrastructure.ReasonCode = "SRQ_RIMB718P";

            break;
          case "CONTEMPT":
            local.Infrastructure.ReasonCode = "SRQ_CONTEMPT";

            break;
          case "SUPPORTP":
            local.Infrastructure.ReasonCode = "SRQ_SUPPORTP";

            break;
          case "IWOISTN":
            local.Infrastructure.ReasonCode = "SRQ_IWOISTN";

            break;
          case "NOIIWON":
            local.Infrastructure.ReasonCode = "SRQ_NOIIWON";

            break;
          case "IISSMWON":
            local.Infrastructure.ReasonCode = "SRQ_IISSMWON";

            break;
          case "MEDICALA":
            local.Infrastructure.ReasonCode = "SRQ_MEDICALA";

            break;
          case "EMPANS":
            local.Infrastructure.ReasonCode = "SRQ_EMPANS";

            break;
          case "GENWRITM":
            local.Infrastructure.ReasonCode = "SRQ_GENWRITM";

            break;
          case "REVIVORM":
            local.Infrastructure.ReasonCode = "SRQ_REVIVORM";

            break;
          case "GARNRQW":
            local.Infrastructure.ReasonCode = "SRGARNRQW";

            break;
          case "GARNRQNW":
            local.Infrastructure.ReasonCode = "SRGARNRQNW";

            break;
          case "ORDIWO2":
            local.Infrastructure.ReasonCode = "SRORDIWO2";

            break;
          case "TERMMWOO":
            local.Infrastructure.ReasonCode = "SRTERMMWOO";

            break;
          case "CSONLYP":
            local.Infrastructure.ReasonCode = "SRCSONLYP";

            break;
          case "PATCSONP":
            local.Infrastructure.ReasonCode = "SRPATCSONP";

            break;
          default:
            // ***********************
            // * RAISE EVENT 29
            // ***********************
            local.Infrastructure.EventId = 29;

            switch(TrimEnd(entities.LegalAction.ActionTaken))
            {
              case "COMPENIM":
                local.Infrastructure.ReasonCode = "NOTCOMPENIM";

                break;
              case "CONSOLDM":
                local.Infrastructure.ReasonCode = "NOTCONSOLDM";

                break;
              case "CSMODM":
                local.Infrastructure.ReasonCode = "NOTCSMODM";

                break;
              case "DEFJPATM":
                local.Infrastructure.ReasonCode = "NOTDEFJPATM";

                break;
              case "DELTSUPM":
                local.Infrastructure.ReasonCode = "NOTDELTSUPM";

                break;
              case "DISMISSM":
                local.Infrastructure.ReasonCode = "NOTDISMISSM";

                break;
              case "GALAMFM":
                local.Infrastructure.ReasonCode = "NOTGALAMFM";

                break;
              case "GARDADLM":
                local.Infrastructure.ReasonCode = "NOTGARDADLM";

                break;
              case "GENETICM":
                local.Infrastructure.ReasonCode = "NOTGENETICM";

                break;
              case "JDAGEMPM":
                local.Infrastructure.ReasonCode = "NOTJDAGEMPM";

                break;
              case "JUDMEXPM":
                local.Infrastructure.ReasonCode = "NOTJUDMEXPM";

                break;
              case "MEDESTBM":
                local.Infrastructure.ReasonCode = "NOTMEDESTBM";

                break;
              case "MODMSOM":
                local.Infrastructure.ReasonCode = "NOTMODMSOM";

                break;
              case "MOTIONKS":
                local.Infrastructure.ReasonCode = "NOTMOTIONKS";

                break;
              case "718BDEFM":
                local.Infrastructure.ReasonCode = "NOT718BDEFM";

                break;
              case "COMPELM":
                local.Infrastructure.ReasonCode = "NOTCOMPELM";

                break;
              default:
                return;
            }

            break;
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
    }
    else
    {
      // -------------------------------------------------
      // Valid Service Date is supplied; Service completed
      // -------------------------------------------------
      local.Infrastructure.EventId = 41;
      local.Infrastructure.ReferenceDate = import.ServiceProcess.ServiceDate;
      local.Infrastructure.DenormTimestamp =
        import.ServiceProcess.CreatedTstamp;

      switch(TrimEnd(entities.LegalAction.ActionTaken))
      {
        case "MWO":
          local.Infrastructure.ReasonCode = "SC_MWO";

          break;
        case "IWOMODM":
          local.Infrastructure.ReasonCode = "SC_IWOMODM";

          break;
        case "IWO":
          local.Infrastructure.ReasonCode = "SC_IWO";

          break;
        case "IWOMODO":
          local.Infrastructure.ReasonCode = "SC_IWOMODO";

          break;
        case "IWOTERM":
          local.Infrastructure.ReasonCode = "SC_IWOTERM";

          break;
        case "DET1PATP":
          local.Infrastructure.ReasonCode = "SC_DET1PATP";

          break;
        case "DET2PATP":
          local.Infrastructure.ReasonCode = "SC_DET2PATP";

          break;
        case "RIMB718P":
          local.Infrastructure.ReasonCode = "SC_RIMB718P";

          break;
        case "AIDOEXMO":
          local.Infrastructure.ReasonCode = "SC_AIDOEXMO";

          break;
        case "SUPPORTP":
          local.Infrastructure.ReasonCode = "SC_SUPPORTP";

          break;
        case "IWOISTN":
          local.Infrastructure.ReasonCode = "SC_IWOISTN";

          break;
        case "NOIIWON":
          local.Infrastructure.ReasonCode = "SC_NOIIWON";

          break;
        case "IISSMWON":
          local.Infrastructure.ReasonCode = "SC_IISSMWON";

          break;
        case "EMPANS":
          local.Infrastructure.ReasonCode = "SC_EMPANS";

          break;
        case "MEDICALA":
          local.Infrastructure.ReasonCode = "SC_MEDICALA";

          break;
        case "CONTEMPT":
          local.Infrastructure.ReasonCode = "SC_CONTEMPT";

          break;
        case "REVIVORM":
          local.Infrastructure.ReasonCode = "SC_REVIVORM";

          break;
        case "GENWRITM":
          local.Infrastructure.ReasonCode = "SC_GENWRITM";

          break;
        case "GARNRQW":
          local.Infrastructure.ReasonCode = "SCGARNRQW";

          break;
        case "GARNRQNW":
          local.Infrastructure.ReasonCode = "SCGARNRQNW";

          break;
        case "ORDIWO2":
          local.Infrastructure.ReasonCode = "SCORDIWO2";

          break;
        case "TERMMWOO":
          local.Infrastructure.ReasonCode = "SCTERMMWOO";

          break;
        case "CSONLYP":
          local.Infrastructure.ReasonCode = "SCCSONLYP";

          break;
        case "PATCSONP":
          local.Infrastructure.ReasonCode = "SCPATCSONP";

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
    target.DenormTimestamp = source.DenormTimestamp;
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
    /// A value of ServiceProcess.
    /// </summary>
    [JsonPropertyName("serviceProcess")]
    public ServiceProcess ServiceProcess
    {
      get => serviceProcess ??= new();
      set => serviceProcess = value;
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

    private ServiceProcess serviceProcess;
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
    /// A value of InitializedToZeros.
    /// </summary>
    [JsonPropertyName("initializedToZeros")]
    public ServiceProcess InitializedToZeros
    {
      get => initializedToZeros ??= new();
      set => initializedToZeros = value;
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

    private ServiceProcess initializedToZeros;
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
