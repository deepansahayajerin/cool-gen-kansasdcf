// Program: SP_CAB_GLOBAL_REASSIGN_INT_CAS, ID: 371335364, model: 746.
// Short name: SWE02003
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_GLOBAL_REASSIGN_INT_CAS.
/// </summary>
[Serializable]
public partial class SpCabGlobalReassignIntCas: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_GLOBAL_REASSIGN_INT_CAS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabGlobalReassignIntCas(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabGlobalReassignIntCas.
  /// </summary>
  public SpCabGlobalReassignIntCas(IContext context, Import import,
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
    // ***************************************************************************************
    // *                               
    // Maintenance Log
    // 
    // *
    // ***************************************************************************************
    // *    DATE       NAME             PR/SR #     DESCRIPTION OF THE CHANGE
    // *
    // * ----------  -----------------  ---------
    // 
    // -----------------------------------------*
    // * 11/21/2002  M.Lachowicz        20258B      Initial Coding.
    // *
    // *
    // 
    // *
    // * 03/11/2010  Raj S              CQ15981     Modified to change the 
    // current date      *
    // *
    // 
    // assignment to rundate while creating the *
    // *
    // 
    // new assignments.                         *
    // *
    // 
    // *
    // ***************************************************************************************
    // 11/20/02    M.Lachowicz WR 20258B
    //             Reassing Interstate Case when case is reassigned.
    local.Current.Date = import.DateWorkArea.Date;
    local.Max.Date = import.Max.Date;
    export.Error.Index = -1;
    export.SoftError.Flag = "N";
    export.ChkpntNumbCreates.Count = import.ChkpntNumbCreates.Count;
    export.ChkpntNumbUpdates.Count = import.ChkpntNumbUpdates.Count;

    if (ReadGlobalReassignment())
    {
      local.GlobalReassignment.AssignmentReasonCode =
        entities.GlobalReassignment.AssignmentReasonCode;
    }
    else
    {
      ExitState = "SP0000_GLOBAL_REASSIGNMENT_NF";

      return;
    }

    if (ReadOfficeServiceProvider2())
    {
      if (!ReadServiceProvider2())
      {
        ExitState = "SERVICE_PROVIDER_NF";

        return;
      }

      if (!ReadOffice1())
      {
        ExitState = "OFFICE_NF";

        return;
      }
    }
    else
    {
      ExitState = "SP0000_OFFICE_SERVICE_PROVIDR_NF";

      return;
    }

    if (ReadOfficeServiceProvider1())
    {
      if (!ReadServiceProvider1())
      {
        ExitState = "SERVICE_PROVIDER_NF";

        return;
      }

      if (!ReadOffice2())
      {
        ExitState = "OFFICE_NF";

        return;
      }
    }
    else
    {
      ExitState = "SP0000_OFFICE_SERVICE_PROVIDR_NF";

      return;
    }

    local.Commit.Count = 0;

    foreach(var item in ReadInterstateCaseAssignment())
    {
      if (AsChar(entities.GlobalReassignment.OverrideFlag) == 'Y')
      {
      }
      else if (AsChar(entities.ExistingInterstateCaseAssignment.OverrideInd) ==
        'Y')
      {
        continue;
      }

      if (ReadInterstateCase())
      {
        // Closed cases should not be transferred.
        if (AsChar(entities.InterstateCase.CaseStatus) == 'C')
        {
          continue;
        }

        try
        {
          CreateInterstateCaseAssignment();
          ++export.BusObjCount.Count;
          ++export.ChkpntNumbCreates.Count;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "SI0000_IN_CASE_ASS_AE";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "SI0000_PV_ADD_INT_CASE";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      try
      {
        UpdateInterstateCaseAssignment();
        ++export.ChkpntNumbUpdates.Count;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "SI000_INT_CASE_UPDATE_UN";

            return;
          case ErrorCode.PermittedValueViolation:
            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      ++local.Commit.Count;

      if (local.Commit.Count >= import
        .ProgramCheckpointRestart.CheckpointCount.GetValueOrDefault())
      {
        UseExtToDoACommit();

        if (export.External.NumericReturnCode != 0)
        {
          return;
        }

        local.Commit.Count = 0;
      }
    }

    // Create OSP alerts for the existing and new OSP's impacted by this 
    // occurrence of Global Reassignment. In the event that an error occurs
    // during the create OSP alert processing, a soft error is posted.  A
    // rollback is not performed since this is not a critical part of the Global
    // Reassignment process.
    if (export.BusObjCount.Count > 0)
    {
      // Create an infrastructure record to tie the OSP alerts for the  Office 
      // Service Providers.
      local.Infrastructure.SituationNumber = 0;
      local.Infrastructure.ProcessStatus = "P";
      local.Infrastructure.EventId = 5;
      local.Infrastructure.BusinessObjectCd = "INC";
      local.Infrastructure.UserId = global.UserId;
      local.Infrastructure.ReasonCode = "INTCASREFXFR";
      local.Infrastructure.ReferenceDate = local.Current.Date;
      local.Infrastructure.Detail =
        "Interstate Case  business object transferred via the GBOR batch process.";
        
      UseSpCabCreateInfrastructure();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        export.SoftError.Flag = "Y";

        ++export.Error.Index;
        export.Error.CheckSize();

        export.Error.Update.Detail.Code = "CASINFNC";
        export.Error.Update.Detail.KeyInfo = "Office: " + NumberToString
          (entities.ExistingOffice.SystemGeneratedId, 12, 4) + " OSP:";
        export.Error.Update.Detail.KeyInfo =
          TrimEnd(export.Error.Item.Detail.KeyInfo) + " " + NumberToString
          (entities.ExistingServiceProvider.SystemGeneratedId, 11, 5) + " Case:";
          
        export.Error.Update.Detail.KeyInfo =
          TrimEnd(export.Error.Item.Detail.KeyInfo) + " " + entities
          .Case1.Number + " - History record not created";
      }

      // Create the OSP Alert for the existing assigned Office Service Provider.
      local.HoldSpNum.Text15 =
        NumberToString(entities.ExistingServiceProvider.SystemGeneratedId, 15);
      local.HoldSpNum.Text15 = Substring(local.HoldSpNum.Text15, 11, 5);
      local.HoldOffcNum.Text15 =
        NumberToString(entities.ExistingOffice.SystemGeneratedId, 15);
      local.HoldOffcNum.Text15 = Substring(local.HoldOffcNum.Text15, 12, 4);
      local.OfficeServiceProviderAlert.DistributionDate = local.Current.Date;
      local.OfficeServiceProviderAlert.OptimizationInd = "N";
      local.OfficeServiceProviderAlert.OptimizedFlag = "2";
      local.OfficeServiceProviderAlert.PrioritizationCode = 1;
      local.OfficeServiceProviderAlert.TypeCode = "AUT";
      local.OfficeServiceProviderAlert.RecipientUserId =
        entities.ExistingServiceProvider.UserId;
      local.OfficeServiceProviderAlert.Description =
        "'INC' business object and associated Monitored Activities have been transferred to another SP";
        
      local.OfficeServiceProviderAlert.Message =
        entities.GlobalReassignment.BusinessObjectCode + ":";
      local.OfficeServiceProviderAlert.Message =
        TrimEnd(local.OfficeServiceProviderAlert.Message) + entities
        .GlobalReassignment.AssignmentReasonCode;
      local.OfficeServiceProviderAlert.Message =
        TrimEnd(local.OfficeServiceProviderAlert.Message) + " transfer SP/Ofc:";
        
      local.OfficeServiceProviderAlert.Message =
        TrimEnd(local.OfficeServiceProviderAlert.Message) + local
        .HoldSpNum.Text15;
      local.OfficeServiceProviderAlert.Message =
        TrimEnd(local.OfficeServiceProviderAlert.Message) + "/";
      local.OfficeServiceProviderAlert.Message =
        TrimEnd(local.OfficeServiceProviderAlert.Message) + local
        .HoldOffcNum.Text15;
      local.OfficeServiceProviderAlert.Message =
        TrimEnd(local.OfficeServiceProviderAlert.Message) + " to SP/Ofc:";
      local.HoldSpNum.Text15 =
        NumberToString(entities.NewServiceProvider.SystemGeneratedId, 15);
      local.HoldSpNum.Text15 = Substring(local.HoldSpNum.Text15, 11, 5);
      local.HoldOffcNum.Text15 =
        NumberToString(entities.NewOffice.SystemGeneratedId, 15);
      local.HoldOffcNum.Text15 = Substring(local.HoldOffcNum.Text15, 12, 4);
      local.OfficeServiceProviderAlert.Message =
        TrimEnd(local.OfficeServiceProviderAlert.Message) + local
        .HoldSpNum.Text15;
      local.OfficeServiceProviderAlert.Message =
        TrimEnd(local.OfficeServiceProviderAlert.Message) + "/";
      local.OfficeServiceProviderAlert.Message =
        TrimEnd(local.OfficeServiceProviderAlert.Message) + local
        .HoldOffcNum.Text15;
      UseSpCabCreateOfcSrvPrvdAlert2();

      if (IsExitState("SP0000_OSP_ALERT_AE"))
      {
        export.SoftError.Flag = "Y";

        ++export.Error.Index;
        export.Error.CheckSize();

        export.Error.Update.Detail.Code = "OSPAAE";
        export.Error.Update.Detail.ProgramError1 =
          "Office Service Provider Alert already exists.";
        export.Error.Update.Detail.KeyInfo = "Office: " + NumberToString
          (entities.ExistingOffice.SystemGeneratedId, 12, 4) + " OSP:";
      }
      else if (IsExitState("CONTROL_TABLE_VALUE_NU"))
      {
        export.SoftError.Flag = "Y";

        ++export.Error.Index;
        export.Error.CheckSize();

        export.Error.Update.Detail.Code = "CTVNU";
        export.Error.Update.Detail.ProgramError1 =
          "Control Table value not unique during OSP Alert create.";
        export.Error.Update.Detail.KeyInfo = "Office: " + NumberToString
          (entities.ExistingOffice.SystemGeneratedId, 12, 4) + " ReAssigned to OSP:";
          
        export.Error.Update.Detail.KeyInfo =
          TrimEnd(export.Error.Item.Detail.KeyInfo) + " " + NumberToString
          (entities.NewServiceProvider.SystemGeneratedId, 11, 5) + " Control Table value not unique during OSP Alert create.";
          
      }
      else if (IsExitState("CONTROL_TABLE_ID_NF"))
      {
        export.SoftError.Flag = "Y";

        ++export.Error.Index;
        export.Error.CheckSize();

        export.Error.Update.Detail.Code = "CTIDNF";
        export.Error.Update.Detail.ProgramError1 =
          "Control Table ID not found during OSP Alert create.";
        export.Error.Update.Detail.KeyInfo = "Office: " + NumberToString
          (entities.ExistingOffice.SystemGeneratedId, 12, 4) + " ReAssigned to OSP:";
          
        export.Error.Update.Detail.KeyInfo =
          TrimEnd(export.Error.Item.Detail.KeyInfo) + " " + NumberToString
          (entities.NewServiceProvider.SystemGeneratedId, 11, 5) + " Control Table ID not found during OSP Alert create.";
          
      }
      else if (IsExitState("CO0000_SRVC_PRVDR_NOT_W_OFFICE"))
      {
        export.SoftError.Flag = "Y";

        ++export.Error.Index;
        export.Error.CheckSize();

        export.Error.Update.Detail.Code = "OSPNF";
        export.Error.Update.Detail.ProgramError1 =
          "Office Service Provider not found during OSP Alert create.";
        export.Error.Update.Detail.KeyInfo = "Office: " + NumberToString
          (entities.ExistingOffice.SystemGeneratedId, 12, 4) + " ReAssigned to OSP:";
          
        export.Error.Update.Detail.KeyInfo =
          TrimEnd(export.Error.Item.Detail.KeyInfo) + " " + NumberToString
          (entities.NewServiceProvider.SystemGeneratedId, 11, 5) + " Office Service Provider not found during OSP Alert create.";
          
      }
      else if (IsExitState("SERVICE_PROVIDER_NF"))
      {
        export.SoftError.Flag = "Y";

        ++export.Error.Index;
        export.Error.CheckSize();

        export.Error.Update.Detail.Code = "SPNF";
        export.Error.Update.Detail.ProgramError1 =
          "Service Provider not found during OSP Alert create.";
        export.Error.Update.Detail.KeyInfo = "Office: " + NumberToString
          (entities.ExistingOffice.SystemGeneratedId, 12, 4) + " ReAssigned to OSP:";
          
        export.Error.Update.Detail.KeyInfo =
          TrimEnd(export.Error.Item.Detail.KeyInfo) + " " + NumberToString
          (entities.NewServiceProvider.SystemGeneratedId, 11, 5) + " Service Provider not found during OSP Alert create.";
          
      }
      else if (IsExitState("OFFICE_NF"))
      {
        export.SoftError.Flag = "Y";

        ++export.Error.Index;
        export.Error.CheckSize();

        export.Error.Update.Detail.Code = "OFCNF";
        export.Error.Update.Detail.ProgramError1 =
          "Office not found during OSP Alert create.";
        export.Error.Update.Detail.KeyInfo = "Office: " + NumberToString
          (entities.ExistingOffice.SystemGeneratedId, 12, 4) + " ReAssigned to OSP:";
          
        export.Error.Update.Detail.KeyInfo =
          TrimEnd(export.Error.Item.Detail.KeyInfo) + " " + NumberToString
          (entities.NewServiceProvider.SystemGeneratedId, 11, 5) + " Office not found during OSP Alert create.";
          
      }
      else if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        export.SoftError.Flag = "Y";

        ++export.Error.Index;
        export.Error.CheckSize();

        export.Error.Update.Detail.Code = "OTRWSE";
        export.Error.Update.Detail.ProgramError1 =
          "Office not found during OSP Alert create.";
      }

      // Since no errors occurred during the critical processing segment of this
      // cab,
      // reset the exit state to all_ok since the exit state will be evaluated 
      // upon return to the main batch job.
      ExitState = "ACO_NN0000_ALL_OK";

      // Create the OSP Alert for the newly assigned Office Service Provider.
      local.OfficeServiceProviderAlert.RecipientUserId =
        entities.NewServiceProvider.UserId;
      local.OfficeServiceProviderAlert.Description =
        " 'INC' business object and associated Monitored Activities have been transferred to another SP";
        
      UseSpCabCreateOfcSrvPrvdAlert1();

      if (IsExitState("SP0000_OSP_ALERT_AE"))
      {
        export.SoftError.Flag = "Y";

        ++export.Error.Index;
        export.Error.CheckSize();

        export.Error.Update.Detail.Code = "OSPAAE";
        export.Error.Update.Detail.ProgramError1 =
          "Office Service Provider Alert already exists.";
        export.Error.Update.Detail.KeyInfo = "Office: " + NumberToString
          (entities.ExistingOffice.SystemGeneratedId, 12, 4) + " ReAssigned to OSP:";
          
        export.Error.Update.Detail.KeyInfo =
          TrimEnd(export.Error.Item.Detail.KeyInfo) + " " + NumberToString
          (entities.NewServiceProvider.SystemGeneratedId, 11, 5) + " Office Service Provider Alert already exists.";
          
      }
      else if (IsExitState("CONTROL_TABLE_VALUE_NU"))
      {
        export.SoftError.Flag = "Y";

        ++export.Error.Index;
        export.Error.CheckSize();

        export.Error.Update.Detail.Code = "CTVNU";
        export.Error.Update.Detail.ProgramError1 =
          "Control Table value not unique during OSP Alert create.";
        export.Error.Update.Detail.KeyInfo = "Office: " + NumberToString
          (entities.ExistingOffice.SystemGeneratedId, 12, 4) + " ReAssigned to OSP:";
          
        export.Error.Update.Detail.KeyInfo =
          TrimEnd(export.Error.Item.Detail.KeyInfo) + " " + NumberToString
          (entities.NewServiceProvider.SystemGeneratedId, 11, 5) + " Control Table value not unique during OSP Alert create.";
          
      }
      else if (IsExitState("CONTROL_TABLE_ID_NF"))
      {
        export.SoftError.Flag = "Y";

        ++export.Error.Index;
        export.Error.CheckSize();

        export.Error.Update.Detail.Code = "CTIDNF";
        export.Error.Update.Detail.ProgramError1 =
          "Control Table ID not found during OSP Alert create.";
        export.Error.Update.Detail.KeyInfo = "Office: " + NumberToString
          (entities.ExistingOffice.SystemGeneratedId, 12, 4) + " ReAssigned to OSP:";
          
        export.Error.Update.Detail.KeyInfo =
          TrimEnd(export.Error.Item.Detail.KeyInfo) + " " + NumberToString
          (entities.NewServiceProvider.SystemGeneratedId, 11, 5) + " Control Table ID not found during OSP Alert create.";
          
      }
      else if (IsExitState("CO0000_SRVC_PRVDR_NOT_W_OFFICE"))
      {
        export.SoftError.Flag = "Y";

        ++export.Error.Index;
        export.Error.CheckSize();

        export.Error.Update.Detail.Code = "OSPNF";
        export.Error.Update.Detail.ProgramError1 =
          "Office Service Provider not found during OSP Alert create.";
        export.Error.Update.Detail.KeyInfo = "Office: " + NumberToString
          (entities.ExistingOffice.SystemGeneratedId, 12, 4) + " ReAssigned to OSP:";
          
        export.Error.Update.Detail.KeyInfo =
          TrimEnd(export.Error.Item.Detail.KeyInfo) + " " + NumberToString
          (entities.NewServiceProvider.SystemGeneratedId, 11, 5) + " Office Service Provider not found during OSP Alert create.";
          
      }
      else if (IsExitState("SERVICE_PROVIDER_NF"))
      {
        export.SoftError.Flag = "Y";

        ++export.Error.Index;
        export.Error.CheckSize();

        export.Error.Update.Detail.Code = "SPNF";
        export.Error.Update.Detail.ProgramError1 =
          "Service Provider not found during OSP Alert create.";
        export.Error.Update.Detail.KeyInfo = "Office: " + NumberToString
          (entities.ExistingOffice.SystemGeneratedId, 12, 4) + " ReAssigned to OSP:";
          
        export.Error.Update.Detail.KeyInfo =
          TrimEnd(export.Error.Item.Detail.KeyInfo) + " " + NumberToString
          (entities.NewServiceProvider.SystemGeneratedId, 11, 5) + " Service Provider not found during OSP Alert create.";
          
      }
      else if (IsExitState("OFFICE_NF"))
      {
        export.SoftError.Flag = "Y";

        ++export.Error.Index;
        export.Error.CheckSize();

        export.Error.Update.Detail.Code = "OFCNF";
        export.Error.Update.Detail.ProgramError1 =
          "Office not found during OSP Alert create.";
        export.Error.Update.Detail.KeyInfo = "Office: " + NumberToString
          (entities.ExistingOffice.SystemGeneratedId, 12, 4) + " ReAssigned to OSP:";
          
        export.Error.Update.Detail.KeyInfo =
          TrimEnd(export.Error.Item.Detail.KeyInfo) + " " + NumberToString
          (entities.NewServiceProvider.SystemGeneratedId, 11, 5) + " Office not found during OSP Alert create.";
          
      }
      else if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        export.SoftError.Flag = "Y";

        ++export.Error.Index;
        export.Error.CheckSize();

        export.Error.Update.Detail.Code = "OTRWSE";
        export.Error.Update.Detail.ProgramError1 =
          "Office not found during OSP Alert create.";
      }

      // Since no errors occurred during the critical processing segment of this
      // cab,
      // reset the exit state to all_ok since the exit state will be evaluated 
      // upon return to the main batch job.
      ExitState = "ACO_NN0000_ALL_OK";
    }
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.UserId = source.UserId;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private static void MoveOfficeServiceProvider(OfficeServiceProvider source,
    OfficeServiceProvider target)
  {
    target.RoleCode = source.RoleCode;
    target.EffectiveDate = source.EffectiveDate;
  }

  private static void MoveOfficeServiceProviderAlert(
    OfficeServiceProviderAlert source, OfficeServiceProviderAlert target)
  {
    target.TypeCode = source.TypeCode;
    target.Message = source.Message;
    target.Description = source.Description;
    target.DistributionDate = source.DistributionDate;
    target.SituationIdentifier = source.SituationIdentifier;
    target.PrioritizationCode = source.PrioritizationCode;
    target.OptimizationInd = source.OptimizationInd;
    target.OptimizedFlag = source.OptimizedFlag;
    target.RecipientUserId = source.RecipientUserId;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = export.External.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    export.External.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    MoveInfrastructure(local.Infrastructure, useImport.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    MoveInfrastructure(useExport.Infrastructure, local.Infrastructure);
  }

  private void UseSpCabCreateOfcSrvPrvdAlert1()
  {
    var useImport = new SpCabCreateOfcSrvPrvdAlert.Import();
    var useExport = new SpCabCreateOfcSrvPrvdAlert.Export();

    MoveOfficeServiceProvider(entities.NewOfficeServiceProvider,
      useImport.OfficeServiceProvider);
    useImport.ServiceProvider.SystemGeneratedId =
      entities.NewServiceProvider.SystemGeneratedId;
    useImport.Office.SystemGeneratedId = entities.NewOffice.SystemGeneratedId;
    MoveOfficeServiceProviderAlert(local.OfficeServiceProviderAlert,
      useImport.OfficeServiceProviderAlert);
    useImport.Alerts.SystemGeneratedIdentifier =
      local.Infrastructure.SystemGeneratedIdentifier;

    Call(SpCabCreateOfcSrvPrvdAlert.Execute, useImport, useExport);
  }

  private void UseSpCabCreateOfcSrvPrvdAlert2()
  {
    var useImport = new SpCabCreateOfcSrvPrvdAlert.Import();
    var useExport = new SpCabCreateOfcSrvPrvdAlert.Export();

    MoveOfficeServiceProvider(entities.ExistingOfficeServiceProvider,
      useImport.OfficeServiceProvider);
    useImport.ServiceProvider.SystemGeneratedId =
      entities.ExistingServiceProvider.SystemGeneratedId;
    useImport.Office.SystemGeneratedId =
      entities.ExistingOffice.SystemGeneratedId;
    MoveOfficeServiceProviderAlert(local.OfficeServiceProviderAlert,
      useImport.OfficeServiceProviderAlert);
    useImport.Alerts.SystemGeneratedIdentifier =
      local.Infrastructure.SystemGeneratedIdentifier;

    Call(SpCabCreateOfcSrvPrvdAlert.Execute, useImport, useExport);
  }

  private void CreateInterstateCaseAssignment()
  {
    System.Diagnostics.Debug.
      Assert(entities.NewOfficeServiceProvider.Populated);

    var reasonCode = entities.GlobalReassignment.AssignmentReasonCode;
    var overrideInd = entities.ExistingInterstateCaseAssignment.OverrideInd;
    var effectiveDate = AddDays(local.Current.Date, 1);
    var discontinueDate = import.Max.Date;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var spdId = entities.NewOfficeServiceProvider.SpdGeneratedId;
    var offId = entities.NewOfficeServiceProvider.OffGeneratedId;
    var ospCode = entities.NewOfficeServiceProvider.RoleCode;
    var ospDate = entities.NewOfficeServiceProvider.EffectiveDate;
    var icsDate = entities.InterstateCase.TransactionDate;
    var icsNo = entities.InterstateCase.TransSerialNumber;

    entities.NewInterstateCaseAssignment.Populated = false;
    Update("CreateInterstateCaseAssignment",
      (db, command) =>
      {
        db.SetString(command, "reasonCode", reasonCode);
        db.SetString(command, "overrideInd", overrideInd);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", null);
        db.SetInt32(command, "spdId", spdId);
        db.SetInt32(command, "offId", offId);
        db.SetString(command, "ospCode", ospCode);
        db.SetDate(command, "ospDate", ospDate);
        db.SetDate(command, "icsDate", icsDate);
        db.SetInt64(command, "icsNo", icsNo);
      });

    entities.NewInterstateCaseAssignment.ReasonCode = reasonCode;
    entities.NewInterstateCaseAssignment.OverrideInd = overrideInd;
    entities.NewInterstateCaseAssignment.EffectiveDate = effectiveDate;
    entities.NewInterstateCaseAssignment.DiscontinueDate = discontinueDate;
    entities.NewInterstateCaseAssignment.CreatedBy = createdBy;
    entities.NewInterstateCaseAssignment.CreatedTimestamp = createdTimestamp;
    entities.NewInterstateCaseAssignment.LastUpdatedBy = "";
    entities.NewInterstateCaseAssignment.LastUpdatedTimestamp = null;
    entities.NewInterstateCaseAssignment.SpdId = spdId;
    entities.NewInterstateCaseAssignment.OffId = offId;
    entities.NewInterstateCaseAssignment.OspCode = ospCode;
    entities.NewInterstateCaseAssignment.OspDate = ospDate;
    entities.NewInterstateCaseAssignment.IcsDate = icsDate;
    entities.NewInterstateCaseAssignment.IcsNo = icsNo;
    entities.NewInterstateCaseAssignment.Populated = true;
  }

  private bool ReadGlobalReassignment()
  {
    entities.GlobalReassignment.Populated = false;

    return Read("ReadGlobalReassignment",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          import.GlobalReassignment.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.GlobalReassignment.CreatedTimestamp =
          db.GetDateTime(reader, 0);
        entities.GlobalReassignment.CreatedBy = db.GetString(reader, 1);
        entities.GlobalReassignment.LastUpdatedBy =
          db.GetNullableString(reader, 2);
        entities.GlobalReassignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 3);
        entities.GlobalReassignment.ProcessDate = db.GetDate(reader, 4);
        entities.GlobalReassignment.StatusFlag = db.GetString(reader, 5);
        entities.GlobalReassignment.OverrideFlag = db.GetString(reader, 6);
        entities.GlobalReassignment.BusinessObjectCode =
          db.GetString(reader, 7);
        entities.GlobalReassignment.AssignmentReasonCode =
          db.GetString(reader, 8);
        entities.GlobalReassignment.SpdGeneratedId =
          db.GetNullableInt32(reader, 9);
        entities.GlobalReassignment.OffGeneratedId =
          db.GetNullableInt32(reader, 10);
        entities.GlobalReassignment.OspRoleCode =
          db.GetNullableString(reader, 11);
        entities.GlobalReassignment.OspEffectiveDate =
          db.GetNullableDate(reader, 12);
        entities.GlobalReassignment.SpdGeneratedId1 =
          db.GetNullableInt32(reader, 13);
        entities.GlobalReassignment.OffGeneratedId1 =
          db.GetNullableInt32(reader, 14);
        entities.GlobalReassignment.OspRoleCod =
          db.GetNullableString(reader, 15);
        entities.GlobalReassignment.OspEffectiveDat =
          db.GetNullableDate(reader, 16);
        entities.GlobalReassignment.BoCount = db.GetNullableInt32(reader, 17);
        entities.GlobalReassignment.MonCount = db.GetNullableInt32(reader, 18);
        entities.GlobalReassignment.Populated = true;
      });
  }

  private bool ReadInterstateCase()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingInterstateCaseAssignment.Populated);
    entities.InterstateCase.Populated = false;

    return Read("ReadInterstateCase",
      (db, command) =>
      {
        db.SetInt64(
          command, "transSerialNbr",
          entities.ExistingInterstateCaseAssignment.IcsNo);
        db.SetDate(
          command, "transactionDate",
          entities.ExistingInterstateCaseAssignment.IcsDate.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateCase.TransSerialNumber = db.GetInt64(reader, 0);
        entities.InterstateCase.TransactionDate = db.GetDate(reader, 1);
        entities.InterstateCase.KsCaseId = db.GetNullableString(reader, 2);
        entities.InterstateCase.CaseStatus = db.GetString(reader, 3);
        entities.InterstateCase.Populated = true;
      });
  }

  private IEnumerable<bool> ReadInterstateCaseAssignment()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingOfficeServiceProvider.Populated);
    entities.ExistingInterstateCaseAssignment.Populated = false;

    return ReadEach("ReadInterstateCaseAssignment",
      (db, command) =>
      {
        db.SetDate(
          command, "ospDate",
          entities.ExistingOfficeServiceProvider.EffectiveDate.
            GetValueOrDefault());
        db.SetString(
          command, "ospCode", entities.ExistingOfficeServiceProvider.RoleCode);
        db.SetInt32(
          command, "offId",
          entities.ExistingOfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdId",
          entities.ExistingOfficeServiceProvider.SpdGeneratedId);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate", import.Max.Date.GetValueOrDefault());
        db.SetString(
          command, "reasonCode", local.GlobalReassignment.AssignmentReasonCode);
          
      },
      (db, reader) =>
      {
        entities.ExistingInterstateCaseAssignment.ReasonCode =
          db.GetString(reader, 0);
        entities.ExistingInterstateCaseAssignment.OverrideInd =
          db.GetString(reader, 1);
        entities.ExistingInterstateCaseAssignment.EffectiveDate =
          db.GetDate(reader, 2);
        entities.ExistingInterstateCaseAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.ExistingInterstateCaseAssignment.CreatedBy =
          db.GetString(reader, 4);
        entities.ExistingInterstateCaseAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.ExistingInterstateCaseAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.ExistingInterstateCaseAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 7);
        entities.ExistingInterstateCaseAssignment.SpdId =
          db.GetInt32(reader, 8);
        entities.ExistingInterstateCaseAssignment.OffId =
          db.GetInt32(reader, 9);
        entities.ExistingInterstateCaseAssignment.OspCode =
          db.GetString(reader, 10);
        entities.ExistingInterstateCaseAssignment.OspDate =
          db.GetDate(reader, 11);
        entities.ExistingInterstateCaseAssignment.IcsDate =
          db.GetDate(reader, 12);
        entities.ExistingInterstateCaseAssignment.IcsNo =
          db.GetInt64(reader, 13);
        entities.ExistingInterstateCaseAssignment.Populated = true;

        return true;
      });
  }

  private bool ReadOffice1()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingOfficeServiceProvider.Populated);
    entities.ExistingOffice.Populated = false;

    return Read("ReadOffice1",
      (db, command) =>
      {
        db.SetInt32(
          command, "officeId",
          entities.ExistingOfficeServiceProvider.OffGeneratedId);
      },
      (db, reader) =>
      {
        entities.ExistingOffice.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingOffice.OffOffice = db.GetNullableInt32(reader, 1);
        entities.ExistingOffice.Populated = true;
      });
  }

  private bool ReadOffice2()
  {
    System.Diagnostics.Debug.
      Assert(entities.NewOfficeServiceProvider.Populated);
    entities.NewOffice.Populated = false;

    return Read("ReadOffice2",
      (db, command) =>
      {
        db.SetInt32(
          command, "officeId",
          entities.NewOfficeServiceProvider.OffGeneratedId);
      },
      (db, reader) =>
      {
        entities.NewOffice.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.NewOffice.OffOffice = db.GetNullableInt32(reader, 1);
        entities.NewOffice.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvider1()
  {
    System.Diagnostics.Debug.Assert(entities.GlobalReassignment.Populated);
    entities.NewOfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider1",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          entities.GlobalReassignment.OspEffectiveDate.GetValueOrDefault());
        db.SetString(
          command, "roleCode", entities.GlobalReassignment.OspRoleCode ?? "");
        db.SetInt32(
          command, "offGeneratedId",
          entities.GlobalReassignment.OffGeneratedId.GetValueOrDefault());
        db.SetInt32(
          command, "spdGeneratedId",
          entities.GlobalReassignment.SpdGeneratedId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.NewOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.NewOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 1);
        entities.NewOfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.NewOfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.NewOfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.NewOfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvider2()
  {
    System.Diagnostics.Debug.Assert(entities.GlobalReassignment.Populated);
    entities.ExistingOfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider2",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          entities.GlobalReassignment.OspEffectiveDat.GetValueOrDefault());
        db.SetString(
          command, "roleCode", entities.GlobalReassignment.OspRoleCod ?? "");
        db.SetInt32(
          command, "offGeneratedId",
          entities.GlobalReassignment.OffGeneratedId1.GetValueOrDefault());
        db.SetInt32(
          command, "spdGeneratedId",
          entities.GlobalReassignment.SpdGeneratedId1.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 1);
        entities.ExistingOfficeServiceProvider.RoleCode =
          db.GetString(reader, 2);
        entities.ExistingOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 3);
        entities.ExistingOfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.ExistingOfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProvider1()
  {
    System.Diagnostics.Debug.
      Assert(entities.NewOfficeServiceProvider.Populated);
    entities.NewServiceProvider.Populated = false;

    return Read("ReadServiceProvider1",
      (db, command) =>
      {
        db.SetInt32(
          command, "servicePrvderId",
          entities.NewOfficeServiceProvider.SpdGeneratedId);
      },
      (db, reader) =>
      {
        entities.NewServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.NewServiceProvider.UserId = db.GetString(reader, 1);
        entities.NewServiceProvider.LastName = db.GetString(reader, 2);
        entities.NewServiceProvider.FirstName = db.GetString(reader, 3);
        entities.NewServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProvider2()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingOfficeServiceProvider.Populated);
    entities.ExistingServiceProvider.Populated = false;

    return Read("ReadServiceProvider2",
      (db, command) =>
      {
        db.SetInt32(
          command, "servicePrvderId",
          entities.ExistingOfficeServiceProvider.SpdGeneratedId);
      },
      (db, reader) =>
      {
        entities.ExistingServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingServiceProvider.UserId = db.GetString(reader, 1);
        entities.ExistingServiceProvider.Populated = true;
      });
  }

  private void UpdateInterstateCaseAssignment()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingInterstateCaseAssignment.Populated);

    var discontinueDate = local.Current.Date;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.ExistingInterstateCaseAssignment.Populated = false;
    Update("UpdateInterstateCaseAssignment",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetDateTime(
          command, "createdTimestamp",
          entities.ExistingInterstateCaseAssignment.CreatedTimestamp.
            GetValueOrDefault());
        db.SetInt32(
          command, "spdId", entities.ExistingInterstateCaseAssignment.SpdId);
        db.SetInt32(
          command, "offId", entities.ExistingInterstateCaseAssignment.OffId);
        db.SetString(
          command, "ospCode",
          entities.ExistingInterstateCaseAssignment.OspCode);
        db.SetDate(
          command, "ospDate",
          entities.ExistingInterstateCaseAssignment.OspDate.
            GetValueOrDefault());
        db.SetDate(
          command, "icsDate",
          entities.ExistingInterstateCaseAssignment.IcsDate.
            GetValueOrDefault());
        db.SetInt64(
          command, "icsNo", entities.ExistingInterstateCaseAssignment.IcsNo);
      });

    entities.ExistingInterstateCaseAssignment.DiscontinueDate = discontinueDate;
    entities.ExistingInterstateCaseAssignment.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingInterstateCaseAssignment.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.ExistingInterstateCaseAssignment.Populated = true;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

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
    /// A value of GlobalReassignment.
    /// </summary>
    [JsonPropertyName("globalReassignment")]
    public GlobalReassignment GlobalReassignment
    {
      get => globalReassignment ??= new();
      set => globalReassignment = value;
    }

    private ProgramCheckpointRestart programCheckpointRestart;
    private Common chkpntNumbUpdates;
    private Common chkpntNumbCreates;
    private DateWorkArea max;
    private DateWorkArea dateWorkArea;
    private GlobalReassignment globalReassignment;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ErrorGroup group.</summary>
    [Serializable]
    public class ErrorGroup
    {
      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public ProgramError Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 200;

      private ProgramError detail;
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

    /// <summary>
    /// A value of SoftError.
    /// </summary>
    [JsonPropertyName("softError")]
    public Common SoftError
    {
      get => softError ??= new();
      set => softError = value;
    }

    /// <summary>
    /// Gets a value of Error.
    /// </summary>
    [JsonIgnore]
    public Array<ErrorGroup> Error => error ??= new(ErrorGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Error for json serialization.
    /// </summary>
    [JsonPropertyName("error")]
    [Computed]
    public IList<ErrorGroup> Error_Json
    {
      get => error;
      set => Error.Assign(value);
    }

    /// <summary>
    /// A value of BusObjCount.
    /// </summary>
    [JsonPropertyName("busObjCount")]
    public Common BusObjCount
    {
      get => busObjCount ??= new();
      set => busObjCount = value;
    }

    /// <summary>
    /// A value of MonActCount.
    /// </summary>
    [JsonPropertyName("monActCount")]
    public Common MonActCount
    {
      get => monActCount ??= new();
      set => monActCount = value;
    }

    private External external;
    private Common chkpntNumbCreates;
    private Common chkpntNumbUpdates;
    private Common softError;
    private Array<ErrorGroup> error;
    private Common busObjCount;
    private Common monActCount;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of GlobalReassignment.
    /// </summary>
    [JsonPropertyName("globalReassignment")]
    public GlobalReassignment GlobalReassignment
    {
      get => globalReassignment ??= new();
      set => globalReassignment = value;
    }

    /// <summary>
    /// A value of Commit.
    /// </summary>
    [JsonPropertyName("commit")]
    public Common Commit
    {
      get => commit ??= new();
      set => commit = value;
    }

    /// <summary>
    /// A value of ScreenIdentification.
    /// </summary>
    [JsonPropertyName("screenIdentification")]
    public Common ScreenIdentification
    {
      get => screenIdentification ??= new();
      set => screenIdentification = value;
    }

    /// <summary>
    /// A value of NewAssignment.
    /// </summary>
    [JsonPropertyName("newAssignment")]
    public Common NewAssignment
    {
      get => newAssignment ??= new();
      set => newAssignment = value;
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
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
    /// A value of HoldSpNum.
    /// </summary>
    [JsonPropertyName("holdSpNum")]
    public WorkArea HoldSpNum
    {
      get => holdSpNum ??= new();
      set => holdSpNum = value;
    }

    /// <summary>
    /// A value of HoldOffcNum.
    /// </summary>
    [JsonPropertyName("holdOffcNum")]
    public WorkArea HoldOffcNum
    {
      get => holdOffcNum ??= new();
      set => holdOffcNum = value;
    }

    /// <summary>
    /// A value of OfficeServiceProviderAlert.
    /// </summary>
    [JsonPropertyName("officeServiceProviderAlert")]
    public OfficeServiceProviderAlert OfficeServiceProviderAlert
    {
      get => officeServiceProviderAlert ??= new();
      set => officeServiceProviderAlert = value;
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
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
    }

    /// <summary>
    /// A value of SpDocKey.
    /// </summary>
    [JsonPropertyName("spDocKey")]
    public SpDocKey SpDocKey
    {
      get => spDocKey ??= new();
      set => spDocKey = value;
    }

    /// <summary>
    /// A value of OutgoingDocument.
    /// </summary>
    [JsonPropertyName("outgoingDocument")]
    public OutgoingDocument OutgoingDocument
    {
      get => outgoingDocument ??= new();
      set => outgoingDocument = value;
    }

    /// <summary>
    /// A value of FindDoc.
    /// </summary>
    [JsonPropertyName("findDoc")]
    public Common FindDoc
    {
      get => findDoc ??= new();
      set => findDoc = value;
    }

    /// <summary>
    /// A value of InterstateCaseAssignment.
    /// </summary>
    [JsonPropertyName("interstateCaseAssignment")]
    public InterstateCaseAssignment InterstateCaseAssignment
    {
      get => interstateCaseAssignment ??= new();
      set => interstateCaseAssignment = value;
    }

    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    private GlobalReassignment globalReassignment;
    private Common commit;
    private Common screenIdentification;
    private Common newAssignment;
    private DateWorkArea initialized;
    private DateWorkArea max;
    private DateWorkArea current;
    private WorkArea holdSpNum;
    private WorkArea holdOffcNum;
    private OfficeServiceProviderAlert officeServiceProviderAlert;
    private Infrastructure infrastructure;
    private Document document;
    private SpDocKey spDocKey;
    private OutgoingDocument outgoingDocument;
    private Common findDoc;
    private InterstateCaseAssignment interstateCaseAssignment;
    private InterstateCase interstateCase;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingInterstateCaseAssignment.
    /// </summary>
    [JsonPropertyName("existingInterstateCaseAssignment")]
    public InterstateCaseAssignment ExistingInterstateCaseAssignment
    {
      get => existingInterstateCaseAssignment ??= new();
      set => existingInterstateCaseAssignment = value;
    }

    /// <summary>
    /// A value of Mona.
    /// </summary>
    [JsonPropertyName("mona")]
    public OfficeServiceProvider Mona
    {
      get => mona ??= new();
      set => mona = value;
    }

    /// <summary>
    /// A value of ExistingLegalReferral.
    /// </summary>
    [JsonPropertyName("existingLegalReferral")]
    public LegalReferral ExistingLegalReferral
    {
      get => existingLegalReferral ??= new();
      set => existingLegalReferral = value;
    }

    /// <summary>
    /// A value of ExistingLegalReferralAssignment.
    /// </summary>
    [JsonPropertyName("existingLegalReferralAssignment")]
    public LegalReferralAssignment ExistingLegalReferralAssignment
    {
      get => existingLegalReferralAssignment ??= new();
      set => existingLegalReferralAssignment = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    /// <summary>
    /// A value of LegalOffice.
    /// </summary>
    [JsonPropertyName("legalOffice")]
    public Office LegalOffice
    {
      get => legalOffice ??= new();
      set => legalOffice = value;
    }

    /// <summary>
    /// A value of LegalServiceProvider.
    /// </summary>
    [JsonPropertyName("legalServiceProvider")]
    public ServiceProvider LegalServiceProvider
    {
      get => legalServiceProvider ??= new();
      set => legalServiceProvider = value;
    }

    /// <summary>
    /// A value of LegalOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("legalOfficeServiceProvider")]
    public OfficeServiceProvider LegalOfficeServiceProvider
    {
      get => legalOfficeServiceProvider ??= new();
      set => legalOfficeServiceProvider = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of NewCaseUnitFunctionAssignmt.
    /// </summary>
    [JsonPropertyName("newCaseUnitFunctionAssignmt")]
    public CaseUnitFunctionAssignmt NewCaseUnitFunctionAssignmt
    {
      get => newCaseUnitFunctionAssignmt ??= new();
      set => newCaseUnitFunctionAssignmt = value;
    }

    /// <summary>
    /// A value of ExistingCaseUnitFunctionAssignmt.
    /// </summary>
    [JsonPropertyName("existingCaseUnitFunctionAssignmt")]
    public CaseUnitFunctionAssignmt ExistingCaseUnitFunctionAssignmt
    {
      get => existingCaseUnitFunctionAssignmt ??= new();
      set => existingCaseUnitFunctionAssignmt = value;
    }

    /// <summary>
    /// A value of NewCaseAssignment.
    /// </summary>
    [JsonPropertyName("newCaseAssignment")]
    public CaseAssignment NewCaseAssignment
    {
      get => newCaseAssignment ??= new();
      set => newCaseAssignment = value;
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
    /// A value of MonitoredActivity.
    /// </summary>
    [JsonPropertyName("monitoredActivity")]
    public MonitoredActivity MonitoredActivity
    {
      get => monitoredActivity ??= new();
      set => monitoredActivity = value;
    }

    /// <summary>
    /// A value of NewMonitoredActivityAssignment.
    /// </summary>
    [JsonPropertyName("newMonitoredActivityAssignment")]
    public MonitoredActivityAssignment NewMonitoredActivityAssignment
    {
      get => newMonitoredActivityAssignment ??= new();
      set => newMonitoredActivityAssignment = value;
    }

    /// <summary>
    /// A value of ExistingMonitoredActivityAssignment.
    /// </summary>
    [JsonPropertyName("existingMonitoredActivityAssignment")]
    public MonitoredActivityAssignment ExistingMonitoredActivityAssignment
    {
      get => existingMonitoredActivityAssignment ??= new();
      set => existingMonitoredActivityAssignment = value;
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
    /// A value of ExistingOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("existingOfficeServiceProvider")]
    public OfficeServiceProvider ExistingOfficeServiceProvider
    {
      get => existingOfficeServiceProvider ??= new();
      set => existingOfficeServiceProvider = value;
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
    /// A value of ExistingServiceProvider.
    /// </summary>
    [JsonPropertyName("existingServiceProvider")]
    public ServiceProvider ExistingServiceProvider
    {
      get => existingServiceProvider ??= new();
      set => existingServiceProvider = value;
    }

    /// <summary>
    /// A value of NewOffice.
    /// </summary>
    [JsonPropertyName("newOffice")]
    public Office NewOffice
    {
      get => newOffice ??= new();
      set => newOffice = value;
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
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    /// <summary>
    /// A value of OldServiceProvider.
    /// </summary>
    [JsonPropertyName("oldServiceProvider")]
    public ServiceProvider OldServiceProvider
    {
      get => oldServiceProvider ??= new();
      set => oldServiceProvider = value;
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
    /// A value of OldInterstateCaseAssignment.
    /// </summary>
    [JsonPropertyName("oldInterstateCaseAssignment")]
    public InterstateCaseAssignment OldInterstateCaseAssignment
    {
      get => oldInterstateCaseAssignment ??= new();
      set => oldInterstateCaseAssignment = value;
    }

    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    /// <summary>
    /// A value of OldInt.
    /// </summary>
    [JsonPropertyName("oldInt")]
    public OfficeServiceProvider OldInt
    {
      get => oldInt ??= new();
      set => oldInt = value;
    }

    /// <summary>
    /// A value of IntServiceProvider.
    /// </summary>
    [JsonPropertyName("intServiceProvider")]
    public ServiceProvider IntServiceProvider
    {
      get => intServiceProvider ??= new();
      set => intServiceProvider = value;
    }

    /// <summary>
    /// A value of IntOffice.
    /// </summary>
    [JsonPropertyName("intOffice")]
    public Office IntOffice
    {
      get => intOffice ??= new();
      set => intOffice = value;
    }

    /// <summary>
    /// A value of NewInterstateCaseAssignment.
    /// </summary>
    [JsonPropertyName("newInterstateCaseAssignment")]
    public InterstateCaseAssignment NewInterstateCaseAssignment
    {
      get => newInterstateCaseAssignment ??= new();
      set => newInterstateCaseAssignment = value;
    }

    private InterstateCaseAssignment existingInterstateCaseAssignment;
    private OfficeServiceProvider mona;
    private LegalReferral existingLegalReferral;
    private LegalReferralAssignment existingLegalReferralAssignment;
    private CsePerson csePerson;
    private CaseRole caseRole;
    private Office legalOffice;
    private ServiceProvider legalServiceProvider;
    private OfficeServiceProvider legalOfficeServiceProvider;
    private CaseUnit caseUnit;
    private Case1 case1;
    private CaseUnitFunctionAssignmt newCaseUnitFunctionAssignmt;
    private CaseUnitFunctionAssignmt existingCaseUnitFunctionAssignmt;
    private CaseAssignment newCaseAssignment;
    private CaseAssignment existingCaseAssignment;
    private Infrastructure infrastructure;
    private MonitoredActivity monitoredActivity;
    private MonitoredActivityAssignment newMonitoredActivityAssignment;
    private MonitoredActivityAssignment existingMonitoredActivityAssignment;
    private GlobalReassignment globalReassignment;
    private OfficeServiceProvider newOfficeServiceProvider;
    private OfficeServiceProvider existingOfficeServiceProvider;
    private ServiceProvider newServiceProvider;
    private ServiceProvider existingServiceProvider;
    private Office newOffice;
    private Office existingOffice;
    private InterstateRequest interstateRequest;
    private ServiceProvider oldServiceProvider;
    private Office office;
    private InterstateCaseAssignment oldInterstateCaseAssignment;
    private InterstateCase interstateCase;
    private OfficeServiceProvider oldInt;
    private ServiceProvider intServiceProvider;
    private Office intOffice;
    private InterstateCaseAssignment newInterstateCaseAssignment;
  }
#endregion
}
