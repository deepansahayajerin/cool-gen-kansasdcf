// Program: SP_CAB_B300_REASSIGN_BATCH, ID: 373518487, model: 746.
// Short name: SWE02597
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_B300_REASSIGN_BATCH.
/// </summary>
[Serializable]
public partial class SpCabB300ReassignBatch: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_B300_REASSIGN_BATCH program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabB300ReassignBatch(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabB300ReassignBatch.
  /// </summary>
  public SpCabB300ReassignBatch(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // --------------------------------------------------------------------------------------
    //                   M A I N T E N A N C E    L O G
    // --------------------------------------------------------------------------------------
    // Date		Developer	Request		Description
    // --------------------------------------------------------------------------------------
    // 01/25/2000	swsrchf		H00082899	Initial development,
    // 						reassign ALERT's and DMON's
    // 03/14/2000	swsrchf		H00090516	Set the Recipient User Id to the NEW
    // 						Service Provider User Id, when creating
    // 						the new OSP ALERT
    // 03/22/2000	swsrchf		H00091385	When creating an OSP ALERT, on an
    // 						ALREADY EXISTS generate a random
    // 						number.  Performance tuned the READ's
    // 03/28/2000	swsrchf		H000xxxxx	Remove all references to
    // 						RECORDED_DOCUMENT
    // 08/23/2000	M Ramirez	101309		DMON is related to an OSP not SP
    // --------------------------------------------------------------------------------------
    // *** Move the IMPORT counts to the EXPORT counts
    export.ChkpntNumbUpdates.Count = import.ChkpntNumbUpdates.Count;
    export.ChkpntNumbCreates.Count = import.ChkpntNumbCreates.Count;
    export.ChkpntNumbDeletes.Count = import.ChkpntNumbDeletes.Count;

    // *** Obtain the NEW Office Service Provider
    if (ReadOfficeServiceProvider())
    {
      // *** continue processing
    }
    else
    {
      ExitState = "SP0000_OFFICE_SERVICE_PROVIDR_NF";

      return;
    }

    // *** Obtain each "Old" Office Service Provider Alert for the CURRENT
    // *** (Persistent) "Old" Office Service Provider
    foreach(var item in ReadOfficeServiceProviderAlertInfrastructure())
    {
      // *** Save the OLD Office Service Provider Alert data
      local.Saved.Assign(entities.Old);

      // *** Obtain the Infrastructure for the CURRENT
      // *** Office Service Provider Alert, if it has one
      if (ReadInfrastructure())
      {
        // *** continue processing
      }
      else
      {
        ExitState = "INFRASTRUCTURE_NF";

        return;
      }

      // *** Delete the OLD Office Service Provider Alert
      DeleteOfficeServiceProviderAlert();

      // *** INCREMENT the Delete accumulator
      ++export.ChkpntNumbDeletes.Count;

      // *** Problem report H00091385
      // *** 03/22/00 SWSRCHF
      local.Repeat.Flag = "Y";

      do
      {
        // *** Create the NEW Office Service Provider Alert using the data
        // *** from the OLD Office Service Provider Alert
        // *** Following fields updated:
        // ***     Last_Updated_By with USER_ID
        // ***     Last_Updated_Timestamp with CURRENT Timestamp
        try
        {
          CreateOfficeServiceProviderAlert();

          // *** INCREMENT the Create accumulator
          ++export.ChkpntNumbCreates.Count;

          // *** Problem report H00091385
          // *** 03/22/00 SWSRCHF
          break;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              // *** Problem report H00091385
              // *** 03/22/00 SWSRCHF
              // *** start
              UseGenerate9DigitRandomNumber();
              local.Saved.SystemGeneratedIdentifier =
                local.SystemGenerated.Attribute9DigitRandomNumber;

              // *** end
              // *** 03/22/00 SWSRCHF
              // *** Problem report H00091385
              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "SP0000_OSP_ALERT_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
      while(AsChar(local.Repeat.Flag) != 'N');
    }

    // *** Obtain each existing Infrastructure and Monitored Document
    // *** where the Infrastructure User Id is "Old" Service Provider
    // *** User Id and the Monitored Document Actual Response Date
    // *** is '0001-01-01'
    // *** Problem report H000xxxxx
    // *** 03/28/00 SWSRCHF
    // *** change read to access Monitored Document via Outgoing Document
    // mjr
    // --------------------------------------------
    // 08/23/2000
    // PR# 101309 - DMON is related to an OSP not SP
    // Added PRINTER_OUTPUT_DESTINATION to READ EACH.
    // ---------------------------------------------------------
    foreach(var item in ReadInfrastructureMonitoredDocument())
    {
      // *** Update the Infrastructure User Id with the "New"
      // *** Service Provider User Id
      // mjr
      // --------------------------------------------
      // 08/23/2000
      // PR# 101309 - DMON is related to an OSP not SP
      // Added call to DMON reassignment CAB.
      // ---------------------------------------------------------
      ExitState = "ACO_NN0000_ALL_OK";
      UseSpCabReassignDocument();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      ++export.ChkpntNumbUpdates.Count;
    }
  }

  private static void MoveOfficeServiceProvider(OfficeServiceProvider source,
    OfficeServiceProvider target)
  {
    target.RoleCode = source.RoleCode;
    target.EffectiveDate = source.EffectiveDate;
  }

  private void UseGenerate9DigitRandomNumber()
  {
    var useImport = new Generate9DigitRandomNumber.Import();
    var useExport = new Generate9DigitRandomNumber.Export();

    Call(Generate9DigitRandomNumber.Execute, useImport, useExport);

    local.SystemGenerated.Attribute9DigitRandomNumber =
      useExport.SystemGenerated.Attribute9DigitRandomNumber;
  }

  private void UseSpCabReassignDocument()
  {
    var useImport = new SpCabReassignDocument.Import();
    var useExport = new SpCabReassignDocument.Export();

    useImport.Infrastructure.Assign(entities.ExistingInfrastructure);
    useImport.OldServiceProvider.UserId = import.Old.UserId;
    MoveOfficeServiceProvider(import.PersistentOld,
      useImport.OldOfficeServiceProvider);
    useImport.OldOffice.SystemGeneratedId = import.Office.SystemGeneratedId;
    useImport.NewServiceProvider.UserId = import.NewServiceProvider.UserId;
    MoveOfficeServiceProvider(import.NewOfficeServiceProvider,
      useImport.NewOfficeServiceProvider);
    useImport.NewOffice.SystemGeneratedId = import.Office.SystemGeneratedId;

    Call(SpCabReassignDocument.Execute, useImport, useExport);
  }

  private void CreateOfficeServiceProviderAlert()
  {
    System.Diagnostics.Debug.
      Assert(entities.NewOfficeServiceProvider.Populated);

    var systemGeneratedIdentifier = local.Saved.SystemGeneratedIdentifier;
    var typeCode = local.Saved.TypeCode;
    var message = local.Saved.Message;
    var description = local.Saved.Description ?? "";
    var distributionDate = local.Saved.DistributionDate;
    var situationIdentifier = local.Saved.SituationIdentifier;
    var prioritizationCode = local.Saved.PrioritizationCode.GetValueOrDefault();
    var optimizationInd = local.Saved.OptimizationInd ?? "";
    var optimizedFlag = local.Saved.OptimizedFlag ?? "";
    var recipientUserId = import.NewServiceProvider.UserId;
    var createdBy = local.Saved.CreatedBy;
    var createdTimestamp = local.Saved.CreatedTimestamp;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();
    var infId = entities.ExistingInfrastructure.SystemGeneratedIdentifier;
    var spdId = entities.NewOfficeServiceProvider.SpdGeneratedId;
    var offId = entities.NewOfficeServiceProvider.OffGeneratedId;
    var ospCode = entities.NewOfficeServiceProvider.RoleCode;
    var ospDate = entities.NewOfficeServiceProvider.EffectiveDate;

    entities.NewOfficeServiceProviderAlert.Populated = false;
    Update("CreateOfficeServiceProviderAlert",
      (db, command) =>
      {
        db.SetInt32(command, "systemGeneratedI", systemGeneratedIdentifier);
        db.SetString(command, "typeCode", typeCode);
        db.SetString(command, "message", message);
        db.SetNullableString(command, "description", description);
        db.SetDate(command, "distributionDate", distributionDate);
        db.SetString(command, "situationIdentifi", situationIdentifier);
        db.SetNullableInt32(command, "prioritizationCod", prioritizationCode);
        db.SetNullableString(command, "optimizationInd", optimizationInd);
        db.SetNullableString(command, "optimizedFlag", optimizedFlag);
        db.SetString(command, "userId", recipientUserId);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableInt32(command, "infId", infId);
        db.SetNullableInt32(command, "spdId", spdId);
        db.SetNullableInt32(command, "offId", offId);
        db.SetNullableString(command, "ospCode", ospCode);
        db.SetNullableDate(command, "ospDate", ospDate);
      });

    entities.NewOfficeServiceProviderAlert.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.NewOfficeServiceProviderAlert.TypeCode = typeCode;
    entities.NewOfficeServiceProviderAlert.Message = message;
    entities.NewOfficeServiceProviderAlert.Description = description;
    entities.NewOfficeServiceProviderAlert.DistributionDate = distributionDate;
    entities.NewOfficeServiceProviderAlert.SituationIdentifier =
      situationIdentifier;
    entities.NewOfficeServiceProviderAlert.PrioritizationCode =
      prioritizationCode;
    entities.NewOfficeServiceProviderAlert.OptimizationInd = optimizationInd;
    entities.NewOfficeServiceProviderAlert.OptimizedFlag = optimizedFlag;
    entities.NewOfficeServiceProviderAlert.RecipientUserId = recipientUserId;
    entities.NewOfficeServiceProviderAlert.CreatedBy = createdBy;
    entities.NewOfficeServiceProviderAlert.CreatedTimestamp = createdTimestamp;
    entities.NewOfficeServiceProviderAlert.LastUpdatedBy = lastUpdatedBy;
    entities.NewOfficeServiceProviderAlert.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.NewOfficeServiceProviderAlert.InfId = infId;
    entities.NewOfficeServiceProviderAlert.SpdId = spdId;
    entities.NewOfficeServiceProviderAlert.OffId = offId;
    entities.NewOfficeServiceProviderAlert.OspCode = ospCode;
    entities.NewOfficeServiceProviderAlert.OspDate = ospDate;
    entities.NewOfficeServiceProviderAlert.Populated = true;
  }

  private void DeleteOfficeServiceProviderAlert()
  {
    Update("DeleteOfficeServiceProviderAlert",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI", entities.Old.SystemGeneratedIdentifier);
      });
  }

  private bool ReadInfrastructure()
  {
    entities.ExistingInfrastructure.Populated = false;

    return Read("ReadInfrastructure",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          entities.ReadOnly.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingInfrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingInfrastructure.CaseNumber =
          db.GetNullableString(reader, 1);
        entities.ExistingInfrastructure.UserId = db.GetString(reader, 2);
        entities.ExistingInfrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 3);
        entities.ExistingInfrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 4);
        entities.ExistingInfrastructure.Populated = true;
      });
  }

  private IEnumerable<bool> ReadInfrastructureMonitoredDocument()
  {
    entities.ExistingInfrastructure.Populated = false;
    entities.ExistingMonitoredDocument.Populated = false;

    return ReadEach("ReadInfrastructureMonitoredDocument",
      (db, command) =>
      {
        db.SetString(command, "userId", import.Old.UserId);
        db.SetNullableString(command, "caseNumber", import.Persistent.Number);
        db.SetNullableDate(
          command, "actRespDt", local.Initialized.Date.GetValueOrDefault());
        db.SetNullableInt32(
          command, "offGenerated", import.Office.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.ExistingInfrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingInfrastructure.CaseNumber =
          db.GetNullableString(reader, 1);
        entities.ExistingInfrastructure.UserId = db.GetString(reader, 2);
        entities.ExistingInfrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 3);
        entities.ExistingInfrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 4);
        entities.ExistingMonitoredDocument.ActualResponseDate =
          db.GetNullableDate(reader, 5);
        entities.ExistingMonitoredDocument.InfId = db.GetInt32(reader, 6);
        entities.ExistingInfrastructure.Populated = true;
        entities.ExistingMonitoredDocument.Populated = true;

        return true;
      });
  }

  private bool ReadOfficeServiceProvider()
  {
    entities.NewOfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider",
      (db, command) =>
      {
        db.SetInt32(command, "offGeneratedId", import.Office.SystemGeneratedId);
        db.SetInt32(
          command, "spdGeneratedId",
          import.NewServiceProvider.SystemGeneratedId);
        db.SetString(
          command, "roleCode", import.NewOfficeServiceProvider.RoleCode);
        db.SetDate(
          command, "effectiveDate",
          import.NewOfficeServiceProvider.EffectiveDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.NewOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.NewOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 1);
        entities.NewOfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.NewOfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.NewOfficeServiceProvider.Populated = true;
      });
  }

  private IEnumerable<bool> ReadOfficeServiceProviderAlertInfrastructure()
  {
    System.Diagnostics.Debug.Assert(import.PersistentOld.Populated);
    entities.Old.Populated = false;
    entities.ReadOnly.Populated = false;

    return ReadEach("ReadOfficeServiceProviderAlertInfrastructure",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "ospDate",
          import.PersistentOld.EffectiveDate.GetValueOrDefault());
        db.SetNullableString(command, "ospCode", import.PersistentOld.RoleCode);
        db.SetNullableInt32(
          command, "offId", import.PersistentOld.OffGeneratedId);
        db.SetNullableInt32(
          command, "spdId", import.PersistentOld.SpdGeneratedId);
        db.SetNullableString(command, "caseNumber", import.Persistent.Number);
      },
      (db, reader) =>
      {
        entities.Old.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Old.TypeCode = db.GetString(reader, 1);
        entities.Old.Message = db.GetString(reader, 2);
        entities.Old.Description = db.GetNullableString(reader, 3);
        entities.Old.DistributionDate = db.GetDate(reader, 4);
        entities.Old.SituationIdentifier = db.GetString(reader, 5);
        entities.Old.PrioritizationCode = db.GetNullableInt32(reader, 6);
        entities.Old.OptimizationInd = db.GetNullableString(reader, 7);
        entities.Old.OptimizedFlag = db.GetNullableString(reader, 8);
        entities.Old.RecipientUserId = db.GetString(reader, 9);
        entities.Old.CreatedBy = db.GetString(reader, 10);
        entities.Old.CreatedTimestamp = db.GetDateTime(reader, 11);
        entities.Old.LastUpdatedBy = db.GetNullableString(reader, 12);
        entities.Old.LastUpdatedTimestamp = db.GetNullableDateTime(reader, 13);
        entities.Old.InfId = db.GetNullableInt32(reader, 14);
        entities.ReadOnly.SystemGeneratedIdentifier = db.GetInt32(reader, 14);
        entities.Old.SpdId = db.GetNullableInt32(reader, 15);
        entities.Old.OffId = db.GetNullableInt32(reader, 16);
        entities.Old.OspCode = db.GetNullableString(reader, 17);
        entities.Old.OspDate = db.GetNullableDate(reader, 18);
        entities.ReadOnly.CaseNumber = db.GetNullableString(reader, 19);
        entities.Old.Populated = true;
        entities.ReadOnly.Populated = true;

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
    /// <summary>
    /// A value of Persistent.
    /// </summary>
    [JsonPropertyName("persistent")]
    public Case1 Persistent
    {
      get => persistent ??= new();
      set => persistent = value;
    }

    /// <summary>
    /// A value of PersistentOld.
    /// </summary>
    [JsonPropertyName("persistentOld")]
    public OfficeServiceProvider PersistentOld
    {
      get => persistentOld ??= new();
      set => persistentOld = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of Old.
    /// </summary>
    [JsonPropertyName("old")]
    public ServiceProvider Old
    {
      get => old ??= new();
      set => old = value;
    }

    /// <summary>
    /// A value of NewServiceProvider.
    /// </summary>
    [JsonPropertyName("newServiceProvider")]
    public ServiceProvider NewServiceProvider
    {
      get => newServiceProvider ??= new();
      set => newServiceProvider = value;
    }

    /// <summary>
    /// A value of NewOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("newOfficeServiceProvider")]
    public OfficeServiceProvider NewOfficeServiceProvider
    {
      get => newOfficeServiceProvider ??= new();
      set => newOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of ChkpntNumbDeletes.
    /// </summary>
    [JsonPropertyName("chkpntNumbDeletes")]
    public Common ChkpntNumbDeletes
    {
      get => chkpntNumbDeletes ??= new();
      set => chkpntNumbDeletes = value;
    }

    /// <summary>
    /// A value of ChkpntNumbCreates.
    /// </summary>
    [JsonPropertyName("chkpntNumbCreates")]
    public Common ChkpntNumbCreates
    {
      get => chkpntNumbCreates ??= new();
      set => chkpntNumbCreates = value;
    }

    /// <summary>
    /// A value of ChkpntNumbUpdates.
    /// </summary>
    [JsonPropertyName("chkpntNumbUpdates")]
    public Common ChkpntNumbUpdates
    {
      get => chkpntNumbUpdates ??= new();
      set => chkpntNumbUpdates = value;
    }

    private Case1 persistent;
    private OfficeServiceProvider persistentOld;
    private Office office;
    private ServiceProvider old;
    private ServiceProvider newServiceProvider;
    private OfficeServiceProvider newOfficeServiceProvider;
    private Common chkpntNumbDeletes;
    private Common chkpntNumbCreates;
    private Common chkpntNumbUpdates;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ChkpntNumbDeletes.
    /// </summary>
    [JsonPropertyName("chkpntNumbDeletes")]
    public Common ChkpntNumbDeletes
    {
      get => chkpntNumbDeletes ??= new();
      set => chkpntNumbDeletes = value;
    }

    /// <summary>
    /// A value of ChkpntNumbCreates.
    /// </summary>
    [JsonPropertyName("chkpntNumbCreates")]
    public Common ChkpntNumbCreates
    {
      get => chkpntNumbCreates ??= new();
      set => chkpntNumbCreates = value;
    }

    /// <summary>
    /// A value of ChkpntNumbUpdates.
    /// </summary>
    [JsonPropertyName("chkpntNumbUpdates")]
    public Common ChkpntNumbUpdates
    {
      get => chkpntNumbUpdates ??= new();
      set => chkpntNumbUpdates = value;
    }

    private Common chkpntNumbDeletes;
    private Common chkpntNumbCreates;
    private Common chkpntNumbUpdates;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Saved.
    /// </summary>
    [JsonPropertyName("saved")]
    public OfficeServiceProviderAlert Saved
    {
      get => saved ??= new();
      set => saved = value;
    }

    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DateWorkArea Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    /// <summary>
    /// A value of Repeat.
    /// </summary>
    [JsonPropertyName("repeat")]
    public Common Repeat
    {
      get => repeat ??= new();
      set => repeat = value;
    }

    /// <summary>
    /// A value of SystemGenerated.
    /// </summary>
    [JsonPropertyName("systemGenerated")]
    public SystemGenerated SystemGenerated
    {
      get => systemGenerated ??= new();
      set => systemGenerated = value;
    }

    private OfficeServiceProviderAlert saved;
    private DateWorkArea initialized;
    private Common repeat;
    private SystemGenerated systemGenerated;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of PrinterOutputDestination.
    /// </summary>
    [JsonPropertyName("printerOutputDestination")]
    public PrinterOutputDestination PrinterOutputDestination
    {
      get => printerOutputDestination ??= new();
      set => printerOutputDestination = value;
    }

    /// <summary>
    /// A value of ExistingOutgoingDocument.
    /// </summary>
    [JsonPropertyName("existingOutgoingDocument")]
    public OutgoingDocument ExistingOutgoingDocument
    {
      get => existingOutgoingDocument ??= new();
      set => existingOutgoingDocument = value;
    }

    /// <summary>
    /// A value of ExistingServiceProvider.
    /// </summary>
    [JsonPropertyName("existingServiceProvider")]
    public ServiceProvider ExistingServiceProvider
    {
      get => existingServiceProvider ??= new();
      set => existingServiceProvider = value;
    }

    /// <summary>
    /// A value of ExistingOffice.
    /// </summary>
    [JsonPropertyName("existingOffice")]
    public Office ExistingOffice
    {
      get => existingOffice ??= new();
      set => existingOffice = value;
    }

    /// <summary>
    /// A value of NewOfficeServiceProviderAlert.
    /// </summary>
    [JsonPropertyName("newOfficeServiceProviderAlert")]
    public OfficeServiceProviderAlert NewOfficeServiceProviderAlert
    {
      get => newOfficeServiceProviderAlert ??= new();
      set => newOfficeServiceProviderAlert = value;
    }

    /// <summary>
    /// A value of Old.
    /// </summary>
    [JsonPropertyName("old")]
    public OfficeServiceProviderAlert Old
    {
      get => old ??= new();
      set => old = value;
    }

    /// <summary>
    /// A value of NewOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("newOfficeServiceProvider")]
    public OfficeServiceProvider NewOfficeServiceProvider
    {
      get => newOfficeServiceProvider ??= new();
      set => newOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of ExistingInfrastructure.
    /// </summary>
    [JsonPropertyName("existingInfrastructure")]
    public Infrastructure ExistingInfrastructure
    {
      get => existingInfrastructure ??= new();
      set => existingInfrastructure = value;
    }

    /// <summary>
    /// A value of ReadOnly.
    /// </summary>
    [JsonPropertyName("readOnly")]
    public Infrastructure ReadOnly
    {
      get => readOnly ??= new();
      set => readOnly = value;
    }

    /// <summary>
    /// A value of ExistingMonitoredDocument.
    /// </summary>
    [JsonPropertyName("existingMonitoredDocument")]
    public MonitoredDocument ExistingMonitoredDocument
    {
      get => existingMonitoredDocument ??= new();
      set => existingMonitoredDocument = value;
    }

    private PrinterOutputDestination printerOutputDestination;
    private OutgoingDocument existingOutgoingDocument;
    private ServiceProvider existingServiceProvider;
    private Office existingOffice;
    private OfficeServiceProviderAlert newOfficeServiceProviderAlert;
    private OfficeServiceProviderAlert old;
    private OfficeServiceProvider newOfficeServiceProvider;
    private Infrastructure existingInfrastructure;
    private Infrastructure readOnly;
    private MonitoredDocument existingMonitoredDocument;
  }
#endregion
}
