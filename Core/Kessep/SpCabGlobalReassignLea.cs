// Program: SP_CAB_GLOBAL_REASSIGN_LEA, ID: 372783721, model: 746.
// Short name: SWE02199
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_GLOBAL_REASSIGN_LEA.
/// </summary>
[Serializable]
public partial class SpCabGlobalReassignLea: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_GLOBAL_REASSIGN_LEA program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabGlobalReassignLea(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabGlobalReassignLea.
  /// </summary>
  public SpCabGlobalReassignLea(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ----------------------------------------------------------------------------
    //                       M A I N T E N A N C E    L O G
    // ----------------------------------------------------------------------------
    // Date        Person   Problem #  Description
    // ----------------------------------------------------------------------------
    // February 2, 1998 - Initial development - J.Rookard, MTW
    // 09/30/1999  SWSRCHF  H00073391  Expanded the code to allow the commit 
    // point
    //                                 
    // to be table driven
    // ----------------------------------------------------------------------------
    // 11/18/1999  SWSRCHF  H00080686  Removed check for Legal_Action end_date
    // ----------------------------------------------------------------------------
    // 06/13/2000  SWSRCHF  H00097134  Changed the Discontinue Date check to 
    // check for
    //                                 
    // MAX date on the Legal Action
    // Assignment and
    //                                 
    // Monitored Activity Assignment
    // reads
    // -------------------------------------------------------------------
    // 10/11/2004  SWSRGAV  180473	Performance change.
    // -------------------------------------------------------------------
    local.Current.Date = import.DateWorkArea.Date;
    local.Max.Date = import.Max.Date;
    export.Error.Index = -1;
    export.SoftError.Flag = "N";
    export.ChkpntNumbCreates.Count = import.ChkpntNumbCreates.Count;
    export.ChkpntNumbUpdates.Count = import.ChkpntNumbUpdates.Count;

    if (!ReadGlobalReassignment())
    {
      ExitState = "SP0000_GLOBAL_REASSIGNMENT_NF";

      return;
    }

    if (ReadOfficeServiceProvider2())
    {
      if (!ReadServiceProvider1())
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

    if (ReadOfficeServiceProvider3())
    {
      if (!ReadServiceProvider2())
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

    // *** 09/30/1999 SWSRCHF
    // *** problem report H00073391
    local.Commit.Count = 0;

    // *** 06/13/2000 SWSRCHF
    // *** problem report H00097134
    // *** changed Discontinue date >= Current date
    // ***           to  Discontinue date = Max date
    foreach(var item in ReadLegalActionAssigment())
    {
      if (AsChar(entities.GlobalReassignment.OverrideFlag) == 'Y')
      {
      }
      else if (AsChar(entities.ExistingLegalActionAssigment.OverrideInd) == 'Y')
      {
        continue;
      }

      if (!ReadLegalAction())
      {
        // Since no updates have occurred for this occurrence of Legal Action 
        // assignment,
        // create a local error group program error occurrence then go get the 
        // next Legal
        // Action Assignment. A rollback is not needed.
        export.SoftError.Flag = "Y";

        ++export.Error.Index;
        export.Error.CheckSize();

        export.Error.Update.Detail.Code = "LEANF";
        export.Error.Update.Detail.ProgramError1 = "Legal Action not found.";
        export.Error.Update.Detail.KeyInfo = "Office: " + NumberToString
          (entities.ExistingOffice.SystemGeneratedId, 12, 4) + " OSP:";
        export.Error.Update.Detail.KeyInfo =
          TrimEnd(export.Error.Item.Detail.KeyInfo) + " " + NumberToString
          (entities.ExistingServiceProvider.SystemGeneratedId, 11, 5) + " Legal Action Not Found";
          

        continue;
      }

      // *** Problem report H00080696
      // *** 11/18/99 SWSRCHF
      // *** start
      // end
      // *** 11/18/99 SWSRCHF
      // *** Problem report H00080696
      try
      {
        UpdateLegalActionAssigment();
        ++export.ChkpntNumbUpdates.Count;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "LEGAL_ACTION_ASSIGNMENT_NU";

            return;
          case ErrorCode.PermittedValueViolation:
            // Not possible.  No permitted values on this entity.
            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      try
      {
        CreateLegalActionAssigment();
        ++export.BusObjCount.Count;
        ++export.ChkpntNumbCreates.Count;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "SP0000_LEGAL_ACTION_ASSIGN_AE";

            return;
          case ErrorCode.PermittedValueViolation:
            // Not possible.  No permitted values on this entity.
            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      local.LegalAction.DenormNumeric12 = entities.LegalAction.Identifier;

      // *** 06/13/2000 SWSRCHF
      // *** problem report H00097134
      // *** changed Discontinue date >= Current date
      // ***           to  Discontinue date = Max date
      // 10/11/2004 SWSRGAV  PR 180473  Performance change.  Break apart joins 
      // using a poorly performing index.
      foreach(var item1 in ReadInfrastructure())
      {
        foreach(var item2 in ReadMonitoredActivity())
        {
          foreach(var item3 in ReadMonitoredActivityAssignment())
          {
            if (!ReadOfficeServiceProvider1())
            {
              // -- The service provider on the monitored activity is not the 
              // same as on the legal action assignment.  Don't change this
              // monitored activity assignment.
              continue;
            }

            try
            {
              UpdateMonitoredActivityAssignment();
              ++export.ChkpntNumbUpdates.Count;
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "SP0000_MONITORED_ACT_ASSGN_NU";

                  return;
                case ErrorCode.PermittedValueViolation:
                  // Not possible.  No permitted values on this entity.
                  break;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }

            try
            {
              CreateMonitoredActivityAssignment();
              ++export.MonActCount.Count;
              ++export.ChkpntNumbCreates.Count;
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "SP0000_MONITORED_ACT_ASSGN_AE";

                  return;
                case ErrorCode.PermittedValueViolation:
                  // Not possible.  No permitted values on this entity.
                  break;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }
        }
      }

      // *** 09/30/1999 SWSRCHF
      // *** problem report H00073391
      // *** start
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

      // *** end
      // *** 09/30/1999 SWSRCHF
      // *** problem report H00073391
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
      local.Infrastructure.EventId = 30;
      local.Infrastructure.BusinessObjectCd = "LEA";
      local.Infrastructure.UserId = global.UserId;
      local.Infrastructure.ReasonCode = "LEAXFR";
      local.Infrastructure.ReferenceDate = local.Current.Date;
      local.Infrastructure.Detail =
        "LEA business object transferred via the GBOR batch process.";
      UseSpCabCreateInfrastructure();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        export.SoftError.Flag = "Y";

        ++export.Error.Index;
        export.Error.CheckSize();

        export.Error.Update.Detail.Code = "LEAINFNC";
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
        "'LEA' business object and associated Monitored Activities have been transferred to another SP";
        
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
        export.Error.Update.Detail.KeyInfo =
          TrimEnd(export.Error.Item.Detail.KeyInfo) + " " + NumberToString
          (entities.ExistingServiceProvider.SystemGeneratedId, 11, 5) + " Office Service Provider Alert already exists.";
          
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
          (entities.ExistingOffice.SystemGeneratedId, 12, 4) + " OSP:";
        export.Error.Update.Detail.KeyInfo =
          TrimEnd(export.Error.Item.Detail.KeyInfo) + " " + NumberToString
          (entities.ExistingServiceProvider.SystemGeneratedId, 11, 5) + " Control Table value not unique during OSP Alert create.";
          
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
          (entities.ExistingOffice.SystemGeneratedId, 12, 4) + " OSP:";
        export.Error.Update.Detail.KeyInfo =
          TrimEnd(export.Error.Item.Detail.KeyInfo) + " " + NumberToString
          (entities.ExistingServiceProvider.SystemGeneratedId, 11, 5) + " Control Table ID not found during OSP Alert create.";
          
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
          (entities.ExistingOffice.SystemGeneratedId, 12, 4) + " OSP:";
        export.Error.Update.Detail.KeyInfo =
          TrimEnd(export.Error.Item.Detail.KeyInfo) + " " + NumberToString
          (entities.ExistingServiceProvider.SystemGeneratedId, 11, 5) + " Office Service Provider not found during OSP Alert create.";
          
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
          (entities.ExistingOffice.SystemGeneratedId, 12, 4) + " OSP:";
        export.Error.Update.Detail.KeyInfo =
          TrimEnd(export.Error.Item.Detail.KeyInfo) + " " + NumberToString
          (entities.ExistingServiceProvider.SystemGeneratedId, 11, 5) + " Service Provider not found during OSP Alert create.";
          
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
          (entities.ExistingOffice.SystemGeneratedId, 12, 4) + " OSP:";
        export.Error.Update.Detail.KeyInfo =
          TrimEnd(export.Error.Item.Detail.KeyInfo) + " " + NumberToString
          (entities.ExistingServiceProvider.SystemGeneratedId, 11, 5) + " Office not found during OSP Alert create.";
          
      }
      else
      {
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
        "'LEA' business object and associated Monitored Activities have been transferred from another SP";
        
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
      else
      {
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

  private void CreateLegalActionAssigment()
  {
    System.Diagnostics.Debug.
      Assert(entities.NewOfficeServiceProvider.Populated);

    var lgaIdentifier = entities.LegalAction.Identifier;
    var ospEffectiveDate = entities.NewOfficeServiceProvider.EffectiveDate;
    var ospRoleCode = entities.NewOfficeServiceProvider.RoleCode;
    var offGeneratedId = entities.NewOfficeServiceProvider.OffGeneratedId;
    var spdGeneratedId = entities.NewOfficeServiceProvider.SpdGeneratedId;
    var effectiveDate = AddDays(local.Current.Date, 1);
    var discontinueDate = local.Max.Date;
    var reasonCode = entities.GlobalReassignment.AssignmentReasonCode;
    var overrideInd = entities.ExistingLegalActionAssigment.OverrideInd;
    var createdBy = global.UserId;
    var createdTimestamp = Now();

    entities.NewLegalActionAssigment.Populated = false;
    Update("CreateLegalActionAssigment",
      (db, command) =>
      {
        db.SetNullableInt32(command, "lgaIdentifier", lgaIdentifier);
        db.SetNullableDate(command, "ospEffectiveDate", ospEffectiveDate);
        db.SetNullableString(command, "ospRoleCode", ospRoleCode);
        db.SetNullableInt32(command, "offGeneratedId", offGeneratedId);
        db.SetNullableInt32(command, "spdGeneratedId", spdGeneratedId);
        db.SetDate(command, "effectiveDt", effectiveDate);
        db.SetNullableDate(command, "endDt", discontinueDate);
        db.SetString(command, "reasonCode", reasonCode);
        db.SetString(command, "overrideInd", overrideInd);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
      });

    entities.NewLegalActionAssigment.LgaIdentifier = lgaIdentifier;
    entities.NewLegalActionAssigment.OspEffectiveDate = ospEffectiveDate;
    entities.NewLegalActionAssigment.OspRoleCode = ospRoleCode;
    entities.NewLegalActionAssigment.OffGeneratedId = offGeneratedId;
    entities.NewLegalActionAssigment.SpdGeneratedId = spdGeneratedId;
    entities.NewLegalActionAssigment.EffectiveDate = effectiveDate;
    entities.NewLegalActionAssigment.DiscontinueDate = discontinueDate;
    entities.NewLegalActionAssigment.ReasonCode = reasonCode;
    entities.NewLegalActionAssigment.OverrideInd = overrideInd;
    entities.NewLegalActionAssigment.CreatedBy = createdBy;
    entities.NewLegalActionAssigment.CreatedTimestamp = createdTimestamp;
    entities.NewLegalActionAssigment.Populated = true;
  }

  private void CreateMonitoredActivityAssignment()
  {
    System.Diagnostics.Debug.
      Assert(entities.NewOfficeServiceProvider.Populated);

    var reasonCode = entities.ExistingMonitoredActivityAssignment.ReasonCode;
    var effectiveDate = AddDays(local.Current.Date, 1);
    var overrideInd = entities.ExistingMonitoredActivityAssignment.OverrideInd;
    var discontinueDate = import.Max.Date;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var spdId = entities.NewOfficeServiceProvider.SpdGeneratedId;
    var offId = entities.NewOfficeServiceProvider.OffGeneratedId;
    var ospCode = entities.NewOfficeServiceProvider.RoleCode;
    var ospDate = entities.NewOfficeServiceProvider.EffectiveDate;
    var macId = entities.MonitoredActivity2.SystemGeneratedIdentifier;

    entities.NewMonitoredActivityAssignment.Populated = false;
    Update("CreateMonitoredActivityAssignment",
      (db, command) =>
      {
        db.SetInt32(command, "systemGeneratedI", 0);
        db.SetString(command, "reasonCode", reasonCode);
        db.SetString(command, "responsibilityCod", "");
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetString(command, "overrideInd", overrideInd);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetInt32(command, "spdId", spdId);
        db.SetInt32(command, "offId", offId);
        db.SetString(command, "ospCode", ospCode);
        db.SetDate(command, "ospDate", ospDate);
        db.SetInt32(command, "macId", macId);
      });

    entities.NewMonitoredActivityAssignment.SystemGeneratedIdentifier = 0;
    entities.NewMonitoredActivityAssignment.ReasonCode = reasonCode;
    entities.NewMonitoredActivityAssignment.ResponsibilityCode = "";
    entities.NewMonitoredActivityAssignment.EffectiveDate = effectiveDate;
    entities.NewMonitoredActivityAssignment.OverrideInd = overrideInd;
    entities.NewMonitoredActivityAssignment.DiscontinueDate = discontinueDate;
    entities.NewMonitoredActivityAssignment.CreatedBy = createdBy;
    entities.NewMonitoredActivityAssignment.CreatedTimestamp = createdTimestamp;
    entities.NewMonitoredActivityAssignment.SpdId = spdId;
    entities.NewMonitoredActivityAssignment.OffId = offId;
    entities.NewMonitoredActivityAssignment.OspCode = ospCode;
    entities.NewMonitoredActivityAssignment.OspDate = ospDate;
    entities.NewMonitoredActivityAssignment.MacId = macId;
    entities.NewMonitoredActivityAssignment.Populated = true;
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
        entities.GlobalReassignment.OverrideFlag = db.GetString(reader, 1);
        entities.GlobalReassignment.BusinessObjectCode =
          db.GetString(reader, 2);
        entities.GlobalReassignment.AssignmentReasonCode =
          db.GetString(reader, 3);
        entities.GlobalReassignment.SpdGeneratedId =
          db.GetNullableInt32(reader, 4);
        entities.GlobalReassignment.OffGeneratedId =
          db.GetNullableInt32(reader, 5);
        entities.GlobalReassignment.OspRoleCode =
          db.GetNullableString(reader, 6);
        entities.GlobalReassignment.OspEffectiveDate =
          db.GetNullableDate(reader, 7);
        entities.GlobalReassignment.SpdGeneratedId1 =
          db.GetNullableInt32(reader, 8);
        entities.GlobalReassignment.OffGeneratedId1 =
          db.GetNullableInt32(reader, 9);
        entities.GlobalReassignment.OspRoleCod =
          db.GetNullableString(reader, 10);
        entities.GlobalReassignment.OspEffectiveDat =
          db.GetNullableDate(reader, 11);
        entities.GlobalReassignment.Populated = true;
      });
  }

  private IEnumerable<bool> ReadInfrastructure()
  {
    entities.Infrastructure.Populated = false;

    return ReadEach("ReadInfrastructure",
      (db, command) =>
      {
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.LegalAction.DenormNumeric12.GetValueOrDefault());
        db.SetString(
          command, "businessObjectCd",
          entities.GlobalReassignment.BusinessObjectCode);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 1);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 2);
        entities.Infrastructure.Populated = true;

        return true;
      });
  }

  private bool ReadLegalAction()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingLegalActionAssigment.Populated);
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          entities.ExistingLegalActionAssigment.LgaIdentifier.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 1);
        entities.LegalAction.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalActionAssigment()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingOfficeServiceProvider.Populated);
    entities.ExistingLegalActionAssigment.Populated = false;

    return ReadEach("ReadLegalActionAssigment",
      (db, command) =>
      {
        db.SetNullableString(
          command, "ospRoleCode",
          entities.ExistingOfficeServiceProvider.RoleCode);
        db.SetNullableDate(
          command, "ospEffectiveDate",
          entities.ExistingOfficeServiceProvider.EffectiveDate.
            GetValueOrDefault());
        db.SetNullableInt32(
          command, "offGeneratedId",
          entities.ExistingOfficeServiceProvider.OffGeneratedId);
        db.SetNullableInt32(
          command, "spdGeneratedId",
          entities.ExistingOfficeServiceProvider.SpdGeneratedId);
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
        db.
          SetNullableDate(command, "endDt", import.Max.Date.GetValueOrDefault());
          
        db.SetString(
          command, "reasonCode",
          import.GlobalReassignment.AssignmentReasonCode);
      },
      (db, reader) =>
      {
        entities.ExistingLegalActionAssigment.LgaIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.ExistingLegalActionAssigment.OspEffectiveDate =
          db.GetNullableDate(reader, 1);
        entities.ExistingLegalActionAssigment.OspRoleCode =
          db.GetNullableString(reader, 2);
        entities.ExistingLegalActionAssigment.OffGeneratedId =
          db.GetNullableInt32(reader, 3);
        entities.ExistingLegalActionAssigment.SpdGeneratedId =
          db.GetNullableInt32(reader, 4);
        entities.ExistingLegalActionAssigment.EffectiveDate =
          db.GetDate(reader, 5);
        entities.ExistingLegalActionAssigment.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.ExistingLegalActionAssigment.ReasonCode =
          db.GetString(reader, 7);
        entities.ExistingLegalActionAssigment.OverrideInd =
          db.GetString(reader, 8);
        entities.ExistingLegalActionAssigment.CreatedTimestamp =
          db.GetDateTime(reader, 9);
        entities.ExistingLegalActionAssigment.LastUpdatedBy =
          db.GetNullableString(reader, 10);
        entities.ExistingLegalActionAssigment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.ExistingLegalActionAssigment.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadMonitoredActivity()
  {
    entities.MonitoredActivity2.Populated = false;

    return ReadEach("ReadMonitoredActivity",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "infSysGenId",
          entities.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.MonitoredActivity2.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.MonitoredActivity2.ClosureReasonCode =
          db.GetNullableString(reader, 1);
        entities.MonitoredActivity2.InfSysGenId =
          db.GetNullableInt32(reader, 2);
        entities.MonitoredActivity2.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadMonitoredActivityAssignment()
  {
    entities.ExistingMonitoredActivityAssignment.Populated = false;

    return ReadEach("ReadMonitoredActivityAssignment",
      (db, command) =>
      {
        db.SetInt32(
          command, "macId",
          entities.MonitoredActivity2.SystemGeneratedIdentifier);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate", import.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingMonitoredActivityAssignment.ReasonCode =
          db.GetString(reader, 0);
        entities.ExistingMonitoredActivityAssignment.EffectiveDate =
          db.GetDate(reader, 1);
        entities.ExistingMonitoredActivityAssignment.OverrideInd =
          db.GetString(reader, 2);
        entities.ExistingMonitoredActivityAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.ExistingMonitoredActivityAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.ExistingMonitoredActivityAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.ExistingMonitoredActivityAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.ExistingMonitoredActivityAssignment.SpdId =
          db.GetInt32(reader, 7);
        entities.ExistingMonitoredActivityAssignment.OffId =
          db.GetInt32(reader, 8);
        entities.ExistingMonitoredActivityAssignment.OspCode =
          db.GetString(reader, 9);
        entities.ExistingMonitoredActivityAssignment.OspDate =
          db.GetDate(reader, 10);
        entities.ExistingMonitoredActivityAssignment.MacId =
          db.GetInt32(reader, 11);
        entities.ExistingMonitoredActivityAssignment.Populated = true;

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
    System.Diagnostics.Debug.Assert(
      entities.ExistingMonitoredActivityAssignment.Populated);
    entities.MonitoredActivity1.Populated = false;

    return Read("ReadOfficeServiceProvider1",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          entities.ExistingMonitoredActivityAssignment.OspDate.
            GetValueOrDefault());
        db.SetString(
          command, "roleCode",
          entities.ExistingMonitoredActivityAssignment.OspCode);
        db.SetInt32(
          command, "offGeneratedId",
          entities.ExistingMonitoredActivityAssignment.OffId);
        db.SetInt32(
          command, "spdGeneratedId",
          entities.ExistingMonitoredActivityAssignment.SpdId);
      },
      (db, reader) =>
      {
        entities.MonitoredActivity1.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.MonitoredActivity1.OffGeneratedId = db.GetInt32(reader, 1);
        entities.MonitoredActivity1.RoleCode = db.GetString(reader, 2);
        entities.MonitoredActivity1.EffectiveDate = db.GetDate(reader, 3);
        entities.MonitoredActivity1.Populated = true;
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
        entities.ExistingOfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvider3()
  {
    System.Diagnostics.Debug.Assert(entities.GlobalReassignment.Populated);
    entities.NewOfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider3",
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
        entities.NewOfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProvider1()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingOfficeServiceProvider.Populated);
    entities.ExistingServiceProvider.Populated = false;

    return Read("ReadServiceProvider1",
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

  private bool ReadServiceProvider2()
  {
    System.Diagnostics.Debug.
      Assert(entities.NewOfficeServiceProvider.Populated);
    entities.NewServiceProvider.Populated = false;

    return Read("ReadServiceProvider2",
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
        entities.NewServiceProvider.Populated = true;
      });
  }

  private void UpdateLegalActionAssigment()
  {
    var discontinueDate = local.Current.Date;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.ExistingLegalActionAssigment.Populated = false;
    Update("UpdateLegalActionAssigment",
      (db, command) =>
      {
        db.SetNullableDate(command, "endDt", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetDateTime(
          command, "createdTimestamp",
          entities.ExistingLegalActionAssigment.CreatedTimestamp.
            GetValueOrDefault());
      });

    entities.ExistingLegalActionAssigment.DiscontinueDate = discontinueDate;
    entities.ExistingLegalActionAssigment.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingLegalActionAssigment.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.ExistingLegalActionAssigment.Populated = true;
  }

  private void UpdateMonitoredActivityAssignment()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingMonitoredActivityAssignment.Populated);

    var discontinueDate = local.Current.Date;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.ExistingMonitoredActivityAssignment.Populated = false;
    Update("UpdateMonitoredActivityAssignment",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetDateTime(
          command, "createdTimestamp",
          entities.ExistingMonitoredActivityAssignment.CreatedTimestamp.
            GetValueOrDefault());
        db.SetInt32(
          command, "spdId", entities.ExistingMonitoredActivityAssignment.SpdId);
          
        db.SetInt32(
          command, "offId", entities.ExistingMonitoredActivityAssignment.OffId);
          
        db.SetString(
          command, "ospCode",
          entities.ExistingMonitoredActivityAssignment.OspCode);
        db.SetDate(
          command, "ospDate",
          entities.ExistingMonitoredActivityAssignment.OspDate.
            GetValueOrDefault());
        db.SetInt32(
          command, "macId", entities.ExistingMonitoredActivityAssignment.MacId);
          
      });

    entities.ExistingMonitoredActivityAssignment.DiscontinueDate =
      discontinueDate;
    entities.ExistingMonitoredActivityAssignment.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingMonitoredActivityAssignment.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.ExistingMonitoredActivityAssignment.Populated = true;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public Infrastructure LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of HoldOffcNum.
    /// </summary>
    [JsonPropertyName("holdOffcNum")]
    public WorkArea HoldOffcNum
    {
      get => holdOffcNum ??= new();
      set => holdOffcNum = value;
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
    /// A value of OfficeServiceProviderAlert.
    /// </summary>
    [JsonPropertyName("officeServiceProviderAlert")]
    public OfficeServiceProviderAlert OfficeServiceProviderAlert
    {
      get => officeServiceProviderAlert ??= new();
      set => officeServiceProviderAlert = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    private Infrastructure legalAction;
    private Common commit;
    private WorkArea holdOffcNum;
    private WorkArea holdSpNum;
    private OfficeServiceProviderAlert officeServiceProviderAlert;
    private DateWorkArea initialized;
    private DateWorkArea max;
    private DateWorkArea current;
    private Infrastructure infrastructure;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of MonitoredActivity1.
    /// </summary>
    [JsonPropertyName("monitoredActivity1")]
    public OfficeServiceProvider MonitoredActivity1
    {
      get => monitoredActivity1 ??= new();
      set => monitoredActivity1 = value;
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
    /// A value of MonitoredActivity2.
    /// </summary>
    [JsonPropertyName("monitoredActivity2")]
    public MonitoredActivity MonitoredActivity2
    {
      get => monitoredActivity2 ??= new();
      set => monitoredActivity2 = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of NewLegalActionAssigment.
    /// </summary>
    [JsonPropertyName("newLegalActionAssigment")]
    public LegalActionAssigment NewLegalActionAssigment
    {
      get => newLegalActionAssigment ??= new();
      set => newLegalActionAssigment = value;
    }

    /// <summary>
    /// A value of ExistingLegalActionAssigment.
    /// </summary>
    [JsonPropertyName("existingLegalActionAssigment")]
    public LegalActionAssigment ExistingLegalActionAssigment
    {
      get => existingLegalActionAssigment ??= new();
      set => existingLegalActionAssigment = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    private OfficeServiceProvider monitoredActivity1;
    private Infrastructure infrastructure;
    private MonitoredActivity monitoredActivity2;
    private MonitoredActivityAssignment newMonitoredActivityAssignment;
    private MonitoredActivityAssignment existingMonitoredActivityAssignment;
    private LegalAction legalAction;
    private LegalActionAssigment newLegalActionAssigment;
    private LegalActionAssigment existingLegalActionAssigment;
    private GlobalReassignment globalReassignment;
    private OfficeServiceProvider newOfficeServiceProvider;
    private OfficeServiceProvider existingOfficeServiceProvider;
    private ServiceProvider newServiceProvider;
    private ServiceProvider existingServiceProvider;
    private Office newOffice;
    private Office existingOffice;
    private Case1 case1;
  }
#endregion
}
