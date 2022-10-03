// Program: SP_PREP_LIFECYCLE_TRANSFORMATION, ID: 374589422, model: 746.
// Short name: SWE02121
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_PREP_LIFECYCLE_TRANSFORMATION.
/// </summary>
[Serializable]
public partial class SpPrepLifecycleTransformation: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_PREP_LIFECYCLE_TRANSFORMATION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpPrepLifecycleTransformation(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpPrepLifecycleTransformation.
  /// </summary>
  public SpPrepLifecycleTransformation(IContext context, Import import,
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
    // ------------------------------------------------------------------------------------------------------------------------
    // DATE	  DEVELOPER   REQUEST#	DESCRIPTION
    // 04/15/10  GVandy	CQ 966	Initial Development of this cab.  Logic was 
    // broken out from
    // 				sp_process_infrastructure cab.
    // ------------------------------------------------------------------------------------------------------------------------
    // ------------------------------------------------------------
    // -  Process Life Cycle Transformations                      -
    // ------------------------------------------------------------
    // -   If EvDtl.LifeCycleImpactCode = Yes                     -
    // -   Infr must contain Case# and Case Unit#                 -
    // -   Read CU to find current STATE of CU                    -
    // -   Read LCT where LCT Is Caused By current EvDtl          -
    // -        and desired LCT TransformsCurrent some LCState    -
    // -        and that LCState = CaseUni.STATE                  -
    // -   Read LCState where LCState Results from current LCT    -
    // -   Update CaseUni.STATE to current LCState                -
    // -   Read EvDtl where EvDtl GeneratedBy current LCT         -
    // -   Read Ev where Ev ----- current EvDtl                   -
    // -   Create Infrastructure, populate from current Infr      -
    // ------------------------------------------------------------
    if (!ReadEventDetail())
    {
      ExitState = "SP0000_EVENT_DETAIL_NF";

      return;
    }

    if (AsChar(entities.EventDetail.LifecycleImpactCode) != 'Y')
    {
      // -- This event detail has no case life cycle impact.
      return;
    }

    if (IsEmpty(import.Infrastructure.CaseNumber))
    {
      ExitState = "SP0000_INFR_REC_HAS_NO_CASE_NUMB";

      return;
    }
    else if (import.Infrastructure.CaseUnitNumber.GetValueOrDefault() == 0)
    {
      ExitState = "SP0000_INFR_REC_HAS_NO_CASE_UNIT";

      return;
    }

    if (!ReadCaseUnitCase())
    {
      ExitState = "CASE_UNIT_NF";

      return;
    }

    if (!ReadLifecycleTransformation())
    {
      // ------------------------------------------------------------
      // -This is not an error, eg if a locate event is raised (Addr-
      // -verified) and the case is not in locate there should not  -
      // -be any transformation.
      // 
      // -
      // ------------------------------------------------------------
      return;
    }

    if (!ReadLifecycleState())
    {
      ExitState = "SP0000_LIFECYCLE_STATE_NF";

      return;
    }

    export.ImportExportCaseUnits.Index = export.ImportExportCaseUnits.Count;
    export.ImportExportCaseUnits.CheckSize();

    export.ImportExportCaseUnits.Update.Case1.Number = entities.Case1.Number;
    MoveCaseUnit(entities.CaseUnit, export.ImportExportCaseUnits.Update.CaseUnit);
      
    export.ImportExportCaseUnits.Update.CaseUnit.State =
      entities.LifecycleState.Identifier;

    if (!ReadEventDetailEvent())
    {
      ExitState = "SP0000_EVENT_DETAIL_NF";

      return;
    }

    export.ImportExportEvents.Index = export.ImportExportEvents.Count;
    export.ImportExportEvents.CheckSize();

    export.ImportExportEvents.Update.G.BusinessObjectCd =
      entities.NextEvent.BusinessObjectCode;
    export.ImportExportEvents.Update.G.CsenetInOutCode =
      entities.NextEventDetail.CsenetInOutCode;
    export.ImportExportEvents.Update.G.InitiatingStateCode =
      entities.NextEventDetail.InitiatingStateCode;
    export.ImportExportEvents.Update.G.ProcessStatus = "Q";
    export.ImportExportEvents.Update.G.ReasonCode =
      entities.NextEventDetail.ReasonCode;
    export.ImportExportEvents.Update.G.CaseNumber = entities.Case1.Number;
    export.ImportExportEvents.Update.G.CsePersonNumber =
      import.Infrastructure.CsePersonNumber ?? "";
    export.ImportExportEvents.Update.G.Detail = "Old State: " + entities
      .CaseUnit.State + "  New State: " + entities.LifecycleState.Identifier;
    export.ImportExportEvents.Update.G.CaseUnitNumber =
      entities.CaseUnit.CuNumber;
    export.ImportExportEvents.Update.G.EventId =
      entities.NextEvent.ControlNumber;
    export.ImportExportEvents.Update.G.SituationNumber = 0;
    export.ImportExportEvents.Update.G.ReferenceDate = Now().Date;
    export.ImportExportEvents.Update.G.UserId = global.UserId;
  }

  private static void MoveCaseUnit(CaseUnit source, CaseUnit target)
  {
    target.CuNumber = source.CuNumber;
    target.State = source.State;
  }

  private bool ReadCaseUnitCase()
  {
    entities.Case1.Populated = false;
    entities.CaseUnit.Populated = false;

    return Read("ReadCaseUnitCase",
      (db, command) =>
      {
        db.SetInt32(
          command, "cuNumber",
          import.Infrastructure.CaseUnitNumber.GetValueOrDefault());
        db.SetString(command, "casNo", import.Infrastructure.CaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.State = db.GetString(reader, 1);
        entities.CaseUnit.CasNo = db.GetString(reader, 2);
        entities.Case1.Number = db.GetString(reader, 2);
        entities.Case1.Populated = true;
        entities.CaseUnit.Populated = true;
      });
  }

  private bool ReadEventDetail()
  {
    entities.EventDetail.Populated = false;

    return Read("ReadEventDetail",
      (db, command) =>
      {
        db.SetInt32(command, "eveNo", import.Event1.ControlNumber);
        db.SetInt32(
          command, "systemGeneratedI",
          import.EventDetail.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.EventDetail.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.EventDetail.LifecycleImpactCode = db.GetString(reader, 1);
        entities.EventDetail.EveNo = db.GetInt32(reader, 2);
        entities.EventDetail.Populated = true;
      });
  }

  private bool ReadEventDetailEvent()
  {
    System.Diagnostics.Debug.Assert(entities.LifecycleTransformation.Populated);
    entities.NextEventDetail.Populated = false;
    entities.NextEvent.Populated = false;

    return Read("ReadEventDetailEvent",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          entities.LifecycleTransformation.EvdLctIdSec.GetValueOrDefault());
        db.SetInt32(
          command, "eveNo",
          entities.LifecycleTransformation.EveNoSec.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.NextEventDetail.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.NextEventDetail.InitiatingStateCode = db.GetString(reader, 1);
        entities.NextEventDetail.CsenetInOutCode = db.GetString(reader, 2);
        entities.NextEventDetail.ReasonCode = db.GetString(reader, 3);
        entities.NextEventDetail.EveNo = db.GetInt32(reader, 4);
        entities.NextEvent.ControlNumber = db.GetInt32(reader, 4);
        entities.NextEvent.BusinessObjectCode = db.GetString(reader, 5);
        entities.NextEventDetail.Populated = true;
        entities.NextEvent.Populated = true;
      });
  }

  private bool ReadLifecycleState()
  {
    System.Diagnostics.Debug.Assert(entities.LifecycleTransformation.Populated);
    entities.LifecycleState.Populated = false;

    return Read("ReadLifecycleState",
      (db, command) =>
      {
        db.SetString(
          command, "identifier", entities.LifecycleTransformation.LcsLctIdSec);
      },
      (db, reader) =>
      {
        entities.LifecycleState.Identifier = db.GetString(reader, 0);
        entities.LifecycleState.Populated = true;
      });
  }

  private bool ReadLifecycleTransformation()
  {
    System.Diagnostics.Debug.Assert(entities.EventDetail.Populated);
    entities.LifecycleTransformation.Populated = false;

    return Read("ReadLifecycleTransformation",
      (db, command) =>
      {
        db.SetInt32(
          command, "evdIdPri", entities.EventDetail.SystemGeneratedIdentifier);
        db.SetInt32(command, "eveCtrlNoPri", entities.EventDetail.EveNo);
        db.SetString(command, "lcsIdPri", entities.CaseUnit.State);
      },
      (db, reader) =>
      {
        entities.LifecycleTransformation.CreatedBy = db.GetString(reader, 0);
        entities.LifecycleTransformation.LcsIdPri = db.GetString(reader, 1);
        entities.LifecycleTransformation.EveCtrlNoPri = db.GetInt32(reader, 2);
        entities.LifecycleTransformation.EvdIdPri = db.GetInt32(reader, 3);
        entities.LifecycleTransformation.LcsLctIdSec = db.GetString(reader, 4);
        entities.LifecycleTransformation.EveNoSec =
          db.GetNullableInt32(reader, 5);
        entities.LifecycleTransformation.EvdLctIdSec =
          db.GetNullableInt32(reader, 6);
        entities.LifecycleTransformation.Populated = true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
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
    /// A value of Event1.
    /// </summary>
    [JsonPropertyName("event1")]
    public Event1 Event1
    {
      get => event1 ??= new();
      set => event1 = value;
    }

    /// <summary>
    /// A value of EventDetail.
    /// </summary>
    [JsonPropertyName("eventDetail")]
    public EventDetail EventDetail
    {
      get => eventDetail ??= new();
      set => eventDetail = value;
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

    private Event1 event1;
    private EventDetail eventDetail;
    private Infrastructure infrastructure;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ImportExportCaseUnitsGroup group.</summary>
    [Serializable]
    public class ImportExportCaseUnitsGroup
    {
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Case1 case1;
      private CaseUnit caseUnit;
    }

    /// <summary>A ImportExportEventsGroup group.</summary>
    [Serializable]
    public class ImportExportEventsGroup
    {
      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public Infrastructure G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Infrastructure g;
    }

    /// <summary>
    /// Gets a value of ImportExportCaseUnits.
    /// </summary>
    [JsonIgnore]
    public Array<ImportExportCaseUnitsGroup> ImportExportCaseUnits =>
      importExportCaseUnits ??= new(ImportExportCaseUnitsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ImportExportCaseUnits for json serialization.
    /// </summary>
    [JsonPropertyName("importExportCaseUnits")]
    [Computed]
    public IList<ImportExportCaseUnitsGroup> ImportExportCaseUnits_Json
    {
      get => importExportCaseUnits;
      set => ImportExportCaseUnits.Assign(value);
    }

    /// <summary>
    /// Gets a value of ImportExportEvents.
    /// </summary>
    [JsonIgnore]
    public Array<ImportExportEventsGroup> ImportExportEvents =>
      importExportEvents ??= new(ImportExportEventsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ImportExportEvents for json serialization.
    /// </summary>
    [JsonPropertyName("importExportEvents")]
    [Computed]
    public IList<ImportExportEventsGroup> ImportExportEvents_Json
    {
      get => importExportEvents;
      set => ImportExportEvents.Assign(value);
    }

    private Array<ImportExportCaseUnitsGroup> importExportCaseUnits;
    private Array<ImportExportEventsGroup> importExportEvents;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Event1.
    /// </summary>
    [JsonPropertyName("event1")]
    public Event1 Event1
    {
      get => event1 ??= new();
      set => event1 = value;
    }

    /// <summary>
    /// A value of EventDetail.
    /// </summary>
    [JsonPropertyName("eventDetail")]
    public EventDetail EventDetail
    {
      get => eventDetail ??= new();
      set => eventDetail = value;
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
    /// A value of LifecycleTransformation.
    /// </summary>
    [JsonPropertyName("lifecycleTransformation")]
    public LifecycleTransformation LifecycleTransformation
    {
      get => lifecycleTransformation ??= new();
      set => lifecycleTransformation = value;
    }

    /// <summary>
    /// A value of LifecycleState.
    /// </summary>
    [JsonPropertyName("lifecycleState")]
    public LifecycleState LifecycleState
    {
      get => lifecycleState ??= new();
      set => lifecycleState = value;
    }

    /// <summary>
    /// A value of NextEventDetail.
    /// </summary>
    [JsonPropertyName("nextEventDetail")]
    public EventDetail NextEventDetail
    {
      get => nextEventDetail ??= new();
      set => nextEventDetail = value;
    }

    /// <summary>
    /// A value of NextEvent.
    /// </summary>
    [JsonPropertyName("nextEvent")]
    public Event1 NextEvent
    {
      get => nextEvent ??= new();
      set => nextEvent = value;
    }

    private Event1 event1;
    private EventDetail eventDetail;
    private Case1 case1;
    private CaseUnit caseUnit;
    private LifecycleTransformation lifecycleTransformation;
    private LifecycleState lifecycleState;
    private EventDetail nextEventDetail;
    private Event1 nextEvent;
  }
#endregion
}
