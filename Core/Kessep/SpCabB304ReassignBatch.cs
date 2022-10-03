// Program: SP_CAB_B304_REASSIGN_BATCH, ID: 373518734, model: 746.
// Short name: SWE02598
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_B304_REASSIGN_BATCH.
/// </summary>
[Serializable]
public partial class SpCabB304ReassignBatch: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_B304_REASSIGN_BATCH program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabB304ReassignBatch(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabB304ReassignBatch.
  /// </summary>
  public SpCabB304ReassignBatch(IContext context, Import import, Export export):
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
    // 						number.
    // 03/28/2000	swsrchf		H000xxxxx	Remove all references to
    // 						RECORDED_DOCUMENT
    // 08/23/2000	M Ramirez	101309		DMON is related to an OSP not SP
    // 09/28/2007      Raj S           00318654/	Modified to add the override 
    // indicator
    //                                 
    // CQ655           process while re
    // -assigning the SP
    //                                                 
    // alerts and monitored documents.
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
      ExitState = "OFFICE_SERVICE_PROVIDER_NF";

      return;
    }

    // *** Obtain each "Old" Office Service Provider Alert for the CURRENT
    // *** (Persistent) "Old" Office Service Provider
    foreach(var item in ReadOfficeServiceProviderAlert())
    {
      // *** Save the OLD Office Service Provider Alert data
      local.Saved.Assign(entities.OldOfficeServiceProviderAlert);

      // *** Obtain the Infrastructure for the CURRENT
      // *** Office Service Provider Alert, if it has one
      if (ReadInfrastructure())
      {
        local.InfrastructureFound.Flag = "Y";

        // ---------------------------------------------------------------------------------------------------------
        // Who: Raj S   when: 09/28/2007  Ref:WR 318654  Added the following 
        // statements to use the ovrride indicator
        // processing while re-assigning the SP alerts.
        // ---------------------------------------------------------------------------------------------------------
        if (AsChar(import.GlobalReassignment.OverrideFlag) == 'Y')
        {
          // ----------------------------------------------------------------------------------------------------
          // ***  Global reassignment record indicates, ignore the overide 
          // process created in case assignment ***
          // ***  go ahead and assign all the alerts to the new service 
          // provider.                             ***
          // ----------------------------------------------------------------------------------------------------
        }
        else if (ReadCaseAssignmentCase())
        {
          if (AsChar(entities.ExistingCaseAssignment.OverrideInd) == 'Y' || AsChar
            (entities.ExistingCase.Status) == 'C')
          {
            // ----------------------------------------------------------------------------------------------------
            // ***  Overide requested in case assignment or case is closed, skip
            // the alert                      ***
            // ----------------------------------------------------------------------------------------------------
            continue;
          }
        }

        // ----------------------------------------------------------------------------------------------------
        // END:   318654 changes
        // 
        // ***
        // ----------------------------------------------------------------------------------------------------
      }
      else
      {
        local.InfrastructureFound.Flag = "N";
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

      if (AsChar(local.InfrastructureFound.Flag) == 'Y')
      {
        // *** If the OLD Office Service Provider Alert had a relationship to
        // *** an Infrastructure record, then attach the NEW Office Service
        // *** Provider Alert to the same Infrastructure record
        AssociateOfficeServiceProviderAlert();
      }
    }

    // mjr
    // --------------------------------------------
    // 08/23/2000
    // PR# 101309 - DMON is related to an OSP not SP
    // Find old office
    // ---------------------------------------------------------
    if (!ReadOffice())
    {
      ExitState = "OFFICE_NF";

      return;
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
      // ---------------------------------------------------------------------------------------------------------
      // Who: Raj S   when: 09/28/2007  Ref:WR 318654  Added the following 
      // statements to use the ovrride indicator
      // processing while re-assigning the SP alerts.
      // ---------------------------------------------------------------------------------------------------------
      if (AsChar(import.GlobalReassignment.OverrideFlag) == 'Y')
      {
        // ----------------------------------------------------------------------------------------------------
        // ***  Global reassignment record indicates, ignore the overide process
        // created in case assignment ***
        // ***  go ahead and assign all the alerts to the new service provider.
        // ***
        // ----------------------------------------------------------------------------------------------------
      }
      else if (ReadCaseAssignmentCase())
      {
        if (AsChar(entities.ExistingCaseAssignment.OverrideInd) == 'Y' || AsChar
          (entities.ExistingCase.Status) == 'C')
        {
          // ----------------------------------------------------------------------------------------------------
          // ***  Overide requested in case assignment or case is closed, skip 
          // the Monitored Document assignment                    ***
          // ----------------------------------------------------------------------------------------------------
          continue;
        }
      }

      // ----------------------------------------------------------------------------------------------------
      // END:   318654 changes
      // 
      // ***
      // ----------------------------------------------------------------------------------------------------
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

    useImport.OldOffice.SystemGeneratedId =
      entities.OldOffice.SystemGeneratedId;
    useImport.Infrastructure.Assign(entities.ExistingInfrastructure);
    useImport.NewServiceProvider.UserId =
      import.PersistentNewServiceProvider.UserId;
    useImport.NewOffice.SystemGeneratedId =
      import.PersistentNewOffice.SystemGeneratedId;
    MoveOfficeServiceProvider(import.PersistentOld,
      useImport.OldOfficeServiceProvider);
    useImport.OldServiceProvider.UserId = import.Old.UserId;
    MoveOfficeServiceProvider(import.New1, useImport.NewOfficeServiceProvider);

    Call(SpCabReassignDocument.Execute, useImport, useExport);
  }

  private void AssociateOfficeServiceProviderAlert()
  {
    var infId = entities.ExistingInfrastructure.SystemGeneratedIdentifier;

    entities.NewOfficeServiceProviderAlert.Populated = false;
    Update("AssociateOfficeServiceProviderAlert",
      (db, command) =>
      {
        db.SetNullableInt32(command, "infId", infId);
        db.SetInt32(
          command, "systemGeneratedI",
          entities.NewOfficeServiceProviderAlert.SystemGeneratedIdentifier);
      });

    entities.NewOfficeServiceProviderAlert.InfId = infId;
    entities.NewOfficeServiceProviderAlert.Populated = true;
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
    var recipientUserId = import.PersistentNewServiceProvider.UserId;
    var createdBy = local.Saved.CreatedBy;
    var createdTimestamp = local.Saved.CreatedTimestamp;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();
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
    entities.NewOfficeServiceProviderAlert.InfId = null;
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
          command, "systemGeneratedI",
          entities.OldOfficeServiceProviderAlert.SystemGeneratedIdentifier);
      });
  }

  private bool ReadCaseAssignmentCase()
  {
    System.Diagnostics.Debug.Assert(import.PersistentOld.Populated);
    entities.ExistingCase.Populated = false;
    entities.ExistingCaseAssignment.Populated = false;

    return Read("ReadCaseAssignmentCase",
      (db, command) =>
      {
        db.SetDate(
          command, "ospDate",
          import.PersistentOld.EffectiveDate.GetValueOrDefault());
        db.SetString(command, "ospCode", import.PersistentOld.RoleCode);
        db.SetInt32(command, "offId", import.PersistentOld.OffGeneratedId);
        db.SetInt32(command, "spdId", import.PersistentOld.SpdGeneratedId);
        db.SetString(
          command, "numb", entities.ExistingInfrastructure.CaseNumber ?? "");
        db.SetString(
          command, "reasonCode",
          import.GlobalReassignment.AssignmentReasonCode);
        db.SetDate(
          command, "effectiveDate",
          import.CurrentDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate", import.MaxDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCaseAssignment.ReasonCode = db.GetString(reader, 0);
        entities.ExistingCaseAssignment.OverrideInd = db.GetString(reader, 1);
        entities.ExistingCaseAssignment.EffectiveDate = db.GetDate(reader, 2);
        entities.ExistingCaseAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.ExistingCaseAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.ExistingCaseAssignment.SpdId = db.GetInt32(reader, 5);
        entities.ExistingCaseAssignment.OffId = db.GetInt32(reader, 6);
        entities.ExistingCaseAssignment.OspCode = db.GetString(reader, 7);
        entities.ExistingCaseAssignment.OspDate = db.GetDate(reader, 8);
        entities.ExistingCaseAssignment.CasNo = db.GetString(reader, 9);
        entities.ExistingCase.Number = db.GetString(reader, 9);
        entities.ExistingCase.Status = db.GetNullableString(reader, 10);
        entities.ExistingCase.Populated = true;
        entities.ExistingCaseAssignment.Populated = true;
      });
  }

  private bool ReadInfrastructure()
  {
    System.Diagnostics.Debug.Assert(
      entities.OldOfficeServiceProviderAlert.Populated);
    entities.ExistingInfrastructure.Populated = false;

    return Read("ReadInfrastructure",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          entities.OldOfficeServiceProviderAlert.InfId.GetValueOrDefault());
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
        db.SetNullableDate(
          command, "actRespDt", local.Initialized.Date.GetValueOrDefault());
        db.SetNullableInt32(
          command, "offGenerated", entities.OldOffice.SystemGeneratedId);
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

  private bool ReadOffice()
  {
    System.Diagnostics.Debug.Assert(import.PersistentOld.Populated);
    entities.OldOffice.Populated = false;

    return Read("ReadOffice",
      (db, command) =>
      {
        db.SetInt32(command, "officeId", import.PersistentOld.OffGeneratedId);
      },
      (db, reader) =>
      {
        entities.OldOffice.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OldOffice.OffOffice = db.GetNullableInt32(reader, 1);
        entities.OldOffice.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvider()
  {
    entities.NewOfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGeneratedId",
          import.PersistentNewServiceProvider.SystemGeneratedId);
        db.SetInt32(
          command, "offGeneratedId",
          import.PersistentNewOffice.SystemGeneratedId);
        db.SetString(command, "roleCode", import.New1.RoleCode);
        db.SetDate(
          command, "effectiveDate",
          import.New1.EffectiveDate.GetValueOrDefault());
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

  private IEnumerable<bool> ReadOfficeServiceProviderAlert()
  {
    System.Diagnostics.Debug.Assert(import.PersistentOld.Populated);
    entities.OldOfficeServiceProviderAlert.Populated = false;

    return ReadEach("ReadOfficeServiceProviderAlert",
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
      },
      (db, reader) =>
      {
        entities.OldOfficeServiceProviderAlert.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.OldOfficeServiceProviderAlert.TypeCode =
          db.GetString(reader, 1);
        entities.OldOfficeServiceProviderAlert.Message =
          db.GetString(reader, 2);
        entities.OldOfficeServiceProviderAlert.Description =
          db.GetNullableString(reader, 3);
        entities.OldOfficeServiceProviderAlert.DistributionDate =
          db.GetDate(reader, 4);
        entities.OldOfficeServiceProviderAlert.SituationIdentifier =
          db.GetString(reader, 5);
        entities.OldOfficeServiceProviderAlert.PrioritizationCode =
          db.GetNullableInt32(reader, 6);
        entities.OldOfficeServiceProviderAlert.OptimizationInd =
          db.GetNullableString(reader, 7);
        entities.OldOfficeServiceProviderAlert.OptimizedFlag =
          db.GetNullableString(reader, 8);
        entities.OldOfficeServiceProviderAlert.RecipientUserId =
          db.GetString(reader, 9);
        entities.OldOfficeServiceProviderAlert.CreatedBy =
          db.GetString(reader, 10);
        entities.OldOfficeServiceProviderAlert.CreatedTimestamp =
          db.GetDateTime(reader, 11);
        entities.OldOfficeServiceProviderAlert.LastUpdatedBy =
          db.GetNullableString(reader, 12);
        entities.OldOfficeServiceProviderAlert.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 13);
        entities.OldOfficeServiceProviderAlert.InfId =
          db.GetNullableInt32(reader, 14);
        entities.OldOfficeServiceProviderAlert.SpdId =
          db.GetNullableInt32(reader, 15);
        entities.OldOfficeServiceProviderAlert.OffId =
          db.GetNullableInt32(reader, 16);
        entities.OldOfficeServiceProviderAlert.OspCode =
          db.GetNullableString(reader, 17);
        entities.OldOfficeServiceProviderAlert.OspDate =
          db.GetNullableDate(reader, 18);
        entities.OldOfficeServiceProviderAlert.Populated = true;

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
    /// A value of PersistentNewServiceProvider.
    /// </summary>
    [JsonPropertyName("persistentNewServiceProvider")]
    public ServiceProvider PersistentNewServiceProvider
    {
      get => persistentNewServiceProvider ??= new();
      set => persistentNewServiceProvider = value;
    }

    /// <summary>
    /// A value of PersistentNewOffice.
    /// </summary>
    [JsonPropertyName("persistentNewOffice")]
    public Office PersistentNewOffice
    {
      get => persistentNewOffice ??= new();
      set => persistentNewOffice = value;
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
    /// A value of Old.
    /// </summary>
    [JsonPropertyName("old")]
    public ServiceProvider Old
    {
      get => old ??= new();
      set => old = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public OfficeServiceProvider New1
    {
      get => new1 ??= new();
      set => new1 = value;
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
    /// A value of ChkpntNumbDeletes.
    /// </summary>
    [JsonPropertyName("chkpntNumbDeletes")]
    public Common ChkpntNumbDeletes
    {
      get => chkpntNumbDeletes ??= new();
      set => chkpntNumbDeletes = value;
    }

    /// <summary>
    /// A value of CurrentDate.
    /// </summary>
    [JsonPropertyName("currentDate")]
    public DateWorkArea CurrentDate
    {
      get => currentDate ??= new();
      set => currentDate = value;
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
    /// A value of GlobalReassignment.
    /// </summary>
    [JsonPropertyName("globalReassignment")]
    public GlobalReassignment GlobalReassignment
    {
      get => globalReassignment ??= new();
      set => globalReassignment = value;
    }

    private ServiceProvider persistentNewServiceProvider;
    private Office persistentNewOffice;
    private OfficeServiceProvider persistentOld;
    private ServiceProvider old;
    private OfficeServiceProvider new1;
    private Common chkpntNumbUpdates;
    private Common chkpntNumbCreates;
    private Common chkpntNumbDeletes;
    private DateWorkArea currentDate;
    private DateWorkArea maxDate;
    private GlobalReassignment globalReassignment;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ChkpntNumbUpdates.
    /// </summary>
    [JsonPropertyName("chkpntNumbUpdates")]
    public Common ChkpntNumbUpdates
    {
      get => chkpntNumbUpdates ??= new();
      set => chkpntNumbUpdates = value;
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
    /// A value of ChkpntNumbDeletes.
    /// </summary>
    [JsonPropertyName("chkpntNumbDeletes")]
    public Common ChkpntNumbDeletes
    {
      get => chkpntNumbDeletes ??= new();
      set => chkpntNumbDeletes = value;
    }

    private Common chkpntNumbUpdates;
    private Common chkpntNumbCreates;
    private Common chkpntNumbDeletes;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of SystemGenerated.
    /// </summary>
    [JsonPropertyName("systemGenerated")]
    public SystemGenerated SystemGenerated
    {
      get => systemGenerated ??= new();
      set => systemGenerated = value;
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
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DateWorkArea Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

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
    /// A value of InfrastructureFound.
    /// </summary>
    [JsonPropertyName("infrastructureFound")]
    public Common InfrastructureFound
    {
      get => infrastructureFound ??= new();
      set => infrastructureFound = value;
    }

    private SystemGenerated systemGenerated;
    private Common repeat;
    private DateWorkArea initialized;
    private External external;
    private OfficeServiceProviderAlert saved;
    private Common infrastructureFound;
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
    /// A value of OldOffice.
    /// </summary>
    [JsonPropertyName("oldOffice")]
    public Office OldOffice
    {
      get => oldOffice ??= new();
      set => oldOffice = value;
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
    /// A value of ExistingInfrastructure.
    /// </summary>
    [JsonPropertyName("existingInfrastructure")]
    public Infrastructure ExistingInfrastructure
    {
      get => existingInfrastructure ??= new();
      set => existingInfrastructure = value;
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
    /// A value of OldOfficeServiceProviderAlert.
    /// </summary>
    [JsonPropertyName("oldOfficeServiceProviderAlert")]
    public OfficeServiceProviderAlert OldOfficeServiceProviderAlert
    {
      get => oldOfficeServiceProviderAlert ??= new();
      set => oldOfficeServiceProviderAlert = value;
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
    /// A value of ExistingCase.
    /// </summary>
    [JsonPropertyName("existingCase")]
    public Case1 ExistingCase
    {
      get => existingCase ??= new();
      set => existingCase = value;
    }

    /// <summary>
    /// A value of ExistingCaseAssignment.
    /// </summary>
    [JsonPropertyName("existingCaseAssignment")]
    public CaseAssignment ExistingCaseAssignment
    {
      get => existingCaseAssignment ??= new();
      set => existingCaseAssignment = value;
    }

    private PrinterOutputDestination printerOutputDestination;
    private Office oldOffice;
    private OutgoingDocument existingOutgoingDocument;
    private Infrastructure existingInfrastructure;
    private MonitoredDocument existingMonitoredDocument;
    private OfficeServiceProvider newOfficeServiceProvider;
    private OfficeServiceProviderAlert oldOfficeServiceProviderAlert;
    private OfficeServiceProviderAlert newOfficeServiceProviderAlert;
    private Case1 existingCase;
    private CaseAssignment existingCaseAssignment;
  }
#endregion
}
