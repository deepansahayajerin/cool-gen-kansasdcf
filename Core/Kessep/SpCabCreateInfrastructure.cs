// Program: SP_CAB_CREATE_INFRASTRUCTURE, ID: 371728350, model: 746.
// Short name: SWE01691
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_CREATE_INFRASTRUCTURE.
/// </summary>
[Serializable]
public partial class SpCabCreateInfrastructure: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_CREATE_INFRASTRUCTURE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabCreateInfrastructure(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabCreateInfrastructure.
  /// </summary>
  public SpCabCreateInfrastructure(IContext context, Import import,
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
    // 11/19/97     J.Rookard     Removed read and update calls to Control 
    // Table.  Infrastructure Sys_Gen_Id is
    // now set by using the 9 digit random number generator cab.
    export.Infrastructure.Assign(import.Infrastructure);
    local.InfraCreateOk.Flag = "N";

    // ******************************************************************************
    // Following code is added to set the additional values for Event and Event 
    // Details.
    // ******************************************************************************
    if (ReadEventEventDetail())
    {
      export.Infrastructure.Function = entities.ExistingEventDetail.Function;
      export.Infrastructure.EventType = entities.ExistingEvent.Type1;
      export.Infrastructure.EventDetailName =
        entities.ExistingEventDetail.DetailName;
    }
    else
    {
      ExitState = "SP0000_EVENT_DETAIL_NF";

      return;
    }

    do
    {
      export.Infrastructure.SystemGeneratedIdentifier =
        UseGenerate9DigitRandomNumber();

      if (import.Infrastructure.SituationNumber == 0)
      {
        export.Infrastructure.SituationNumber =
          export.Infrastructure.SystemGeneratedIdentifier;
      }

      try
      {
        CreateInfrastructure();
        export.Infrastructure.Assign(entities.New1);
        local.InfraCreateOk.Flag = "Y";
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            break;
          case ErrorCode.PermittedValueViolation:
            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    while(AsChar(local.InfraCreateOk.Flag) != 'Y');
  }

  private int UseGenerate9DigitRandomNumber()
  {
    var useImport = new Generate9DigitRandomNumber.Import();
    var useExport = new Generate9DigitRandomNumber.Export();

    Call(Generate9DigitRandomNumber.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute9DigitRandomNumber;
  }

  private void CreateInfrastructure()
  {
    var systemGeneratedIdentifier =
      export.Infrastructure.SystemGeneratedIdentifier;
    var situationNumber = export.Infrastructure.SituationNumber;
    var processStatus = export.Infrastructure.ProcessStatus;
    var eventId = export.Infrastructure.EventId;
    var eventType = export.Infrastructure.EventType;
    var eventDetailName = export.Infrastructure.EventDetailName;
    var reasonCode = export.Infrastructure.ReasonCode;
    var businessObjectCd = export.Infrastructure.BusinessObjectCd;
    var denormNumeric12 =
      export.Infrastructure.DenormNumeric12.GetValueOrDefault();
    var denormText12 = export.Infrastructure.DenormText12 ?? "";
    var denormDate = export.Infrastructure.DenormDate;
    var denormTimestamp = export.Infrastructure.DenormTimestamp;
    var initiatingStateCode = export.Infrastructure.InitiatingStateCode;
    var csenetInOutCode = export.Infrastructure.CsenetInOutCode;
    var caseNumber = export.Infrastructure.CaseNumber ?? "";
    var csePersonNumber = export.Infrastructure.CsePersonNumber ?? "";
    var caseUnitNumber =
      export.Infrastructure.CaseUnitNumber.GetValueOrDefault();
    var userId = export.Infrastructure.UserId;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var lastUpdatedBy = export.Infrastructure.LastUpdatedBy ?? "";
    var lastUpdatedTimestamp = export.Infrastructure.LastUpdatedTimestamp;
    var referenceDate = export.Infrastructure.ReferenceDate;
    var function = export.Infrastructure.Function ?? "";
    var detail = export.Infrastructure.Detail ?? "";

    entities.New1.Populated = false;
    Update("CreateInfrastructure",
      (db, command) =>
      {
        db.SetInt32(command, "systemGeneratedI", systemGeneratedIdentifier);
        db.SetInt32(command, "situationNumber", situationNumber);
        db.SetString(command, "processStatus", processStatus);
        db.SetInt32(command, "eventId", eventId);
        db.SetString(command, "eventType", eventType);
        db.SetString(command, "eventDetailName", eventDetailName);
        db.SetString(command, "reasonCode", reasonCode);
        db.SetString(command, "businessObjectCd", businessObjectCd);
        db.SetNullableInt64(command, "denormNumeric12", denormNumeric12);
        db.SetNullableString(command, "denormText12", denormText12);
        db.SetNullableDate(command, "denormDate", denormDate);
        db.SetNullableDateTime(command, "denormTimestamp", denormTimestamp);
        db.SetString(command, "initiatingStCd", initiatingStateCode);
        db.SetString(command, "csenetInOutCode", csenetInOutCode);
        db.SetNullableString(command, "caseNumber", caseNumber);
        db.SetNullableString(command, "csePersonNum", csePersonNumber);
        db.SetNullableInt32(command, "caseUnitNum", caseUnitNumber);
        db.SetString(command, "userId", userId);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableDate(command, "referenceDate", referenceDate);
        db.SetNullableString(
          command, "cpaOaaType", GetImplicitValue<Infrastructure,
          string>("CpaOaaType"));
        db.SetNullableString(
          command, "cpaType", GetImplicitValue<Infrastructure,
          string>("CpaType"));
        db.SetNullableString(command, "function", function);
        db.SetNullableString(command, "caseUnitState", "");
        db.SetNullableString(command, "detail", detail);
      });

    entities.New1.SystemGeneratedIdentifier = systemGeneratedIdentifier;
    entities.New1.SituationNumber = situationNumber;
    entities.New1.ProcessStatus = processStatus;
    entities.New1.EventId = eventId;
    entities.New1.EventType = eventType;
    entities.New1.EventDetailName = eventDetailName;
    entities.New1.ReasonCode = reasonCode;
    entities.New1.BusinessObjectCd = businessObjectCd;
    entities.New1.DenormNumeric12 = denormNumeric12;
    entities.New1.DenormText12 = denormText12;
    entities.New1.DenormDate = denormDate;
    entities.New1.DenormTimestamp = denormTimestamp;
    entities.New1.InitiatingStateCode = initiatingStateCode;
    entities.New1.CsenetInOutCode = csenetInOutCode;
    entities.New1.CaseNumber = caseNumber;
    entities.New1.CsePersonNumber = csePersonNumber;
    entities.New1.CaseUnitNumber = caseUnitNumber;
    entities.New1.UserId = userId;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CreatedTimestamp = createdTimestamp;
    entities.New1.LastUpdatedBy = lastUpdatedBy;
    entities.New1.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.New1.ReferenceDate = referenceDate;
    entities.New1.Function = function;
    entities.New1.Detail = detail;
    entities.New1.Populated = true;
  }

  private bool ReadEventEventDetail()
  {
    entities.ExistingEventDetail.Populated = false;
    entities.ExistingEvent.Populated = false;

    return Read("ReadEventEventDetail",
      (db, command) =>
      {
        db.SetInt32(command, "controlNumber", export.Infrastructure.EventId);
        db.SetString(command, "reasonCode", export.Infrastructure.ReasonCode);
      },
      (db, reader) =>
      {
        entities.ExistingEvent.ControlNumber = db.GetInt32(reader, 0);
        entities.ExistingEventDetail.EveNo = db.GetInt32(reader, 0);
        entities.ExistingEvent.Name = db.GetString(reader, 1);
        entities.ExistingEvent.Type1 = db.GetString(reader, 2);
        entities.ExistingEvent.Description = db.GetNullableString(reader, 3);
        entities.ExistingEvent.BusinessObjectCode = db.GetString(reader, 4);
        entities.ExistingEvent.CreatedBy = db.GetString(reader, 5);
        entities.ExistingEvent.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.ExistingEvent.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.ExistingEvent.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 8);
        entities.ExistingEventDetail.SystemGeneratedIdentifier =
          db.GetInt32(reader, 9);
        entities.ExistingEventDetail.DetailName = db.GetString(reader, 10);
        entities.ExistingEventDetail.Description =
          db.GetNullableString(reader, 11);
        entities.ExistingEventDetail.InitiatingStateCode =
          db.GetString(reader, 12);
        entities.ExistingEventDetail.CsenetInOutCode = db.GetString(reader, 13);
        entities.ExistingEventDetail.ReasonCode = db.GetString(reader, 14);
        entities.ExistingEventDetail.ProcedureName =
          db.GetNullableString(reader, 15);
        entities.ExistingEventDetail.LifecycleImpactCode =
          db.GetString(reader, 16);
        entities.ExistingEventDetail.LogToDiaryInd = db.GetString(reader, 17);
        entities.ExistingEventDetail.DateMonitorDays =
          db.GetNullableInt32(reader, 18);
        entities.ExistingEventDetail.NextEventId =
          db.GetNullableInt32(reader, 19);
        entities.ExistingEventDetail.NextEventDetailId =
          db.GetNullableString(reader, 20);
        entities.ExistingEventDetail.NextInitiatingState =
          db.GetString(reader, 21);
        entities.ExistingEventDetail.NextCsenetInOut =
          db.GetNullableString(reader, 22);
        entities.ExistingEventDetail.NextReason =
          db.GetNullableString(reader, 23);
        entities.ExistingEventDetail.EffectiveDate = db.GetDate(reader, 24);
        entities.ExistingEventDetail.DiscontinueDate = db.GetDate(reader, 25);
        entities.ExistingEventDetail.CreatedBy = db.GetString(reader, 26);
        entities.ExistingEventDetail.CreatedTimestamp =
          db.GetDateTime(reader, 27);
        entities.ExistingEventDetail.LastUpdatedBy =
          db.GetNullableString(reader, 28);
        entities.ExistingEventDetail.LastUpdatedDtstamp =
          db.GetNullableDateTime(reader, 29);
        entities.ExistingEventDetail.Function =
          db.GetNullableString(reader, 30);
        entities.ExistingEventDetail.Populated = true;
        entities.ExistingEvent.Populated = true;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    private Infrastructure infrastructure;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    private Infrastructure infrastructure;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of InfraCreateOk.
    /// </summary>
    [JsonPropertyName("infraCreateOk")]
    public Common InfraCreateOk
    {
      get => infraCreateOk ??= new();
      set => infraCreateOk = value;
    }

    /// <summary>
    /// A value of Hold.
    /// </summary>
    [JsonPropertyName("hold")]
    public Infrastructure Hold
    {
      get => hold ??= new();
      set => hold = value;
    }

    /// <summary>
    /// A value of Local1StChar.
    /// </summary>
    [JsonPropertyName("local1StChar")]
    public Infrastructure Local1StChar
    {
      get => local1StChar ??= new();
      set => local1StChar = value;
    }

    private Common infraCreateOk;
    private Infrastructure hold;
    private Infrastructure local1StChar;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingEventDetail.
    /// </summary>
    [JsonPropertyName("existingEventDetail")]
    public EventDetail ExistingEventDetail
    {
      get => existingEventDetail ??= new();
      set => existingEventDetail = value;
    }

    /// <summary>
    /// A value of ExistingEvent.
    /// </summary>
    [JsonPropertyName("existingEvent")]
    public Event1 ExistingEvent
    {
      get => existingEvent ??= new();
      set => existingEvent = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public Infrastructure New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    private EventDetail existingEventDetail;
    private Event1 existingEvent;
    private Infrastructure new1;
  }
#endregion
}
