﻿// Program: SP_CAB_GLOBAL_REASSIGN_LRF, ID: 372783720, model: 746.
// Short name: SWE02200
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_GLOBAL_REASSIGN_LRF.
/// </summary>
[Serializable]
public partial class SpCabGlobalReassignLrf: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_GLOBAL_REASSIGN_LRF program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabGlobalReassignLrf(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabGlobalReassignLrf.
  /// </summary>
  public SpCabGlobalReassignLrf(IContext context, Import import, Export export):
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
    // 11/09/1999  SWSRCHF  H00078787  Alert wording incorrect for LRF's
    // ----------------------------------------------------------------------------
    // 11/18/1999  SWSRCHF  H00080696  Removed check for a closed CASE
    // ----------------------------------------------------------------------------
    // 05/08/2000  SWSRCHF  H00094567  changed the reads to 'Select Only'
    // ----------------------------------------------------------------------------
    // 06/13/2000  SWSRCHF  H00097134  Changed the Discontinue Date check to 
    // check for
    //                                 
    // MAX date on the Legal Referral
    // Assignment and
    //                                 
    // Monitored Activity Assignment
    // reads
    // -------------------------------------------------------------------
    // 11/26/2002  SWSRPDP  PRODFIX  changed the read to NOT use Denor-12 key - 
    // instead use CASE key
    // ----------------------------------------------------------------------------
    // 7/14/2010 swdpjlh  cq20565 Performance change
    // ---------------------------------------------------------------
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

    if (ReadOfficeServiceProvider1())
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

    if (ReadOfficeServiceProvider2())
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
    foreach(var item in ReadLegalReferralAssignment())
    {
      if (AsChar(entities.GlobalReassignment.OverrideFlag) == 'Y')
      {
      }
      else if (AsChar(entities.ExistingLegalReferralAssignment.OverrideInd) == 'Y'
        )
      {
        continue;
      }

      if (ReadLegalReferral())
      {
        if (ReadCase())
        {
          // *** Problem report H00080696
          // *** 11/18/99 SWSRCHF
          // *** start
          // end
          // *** 11/18/99 SWSRCHF
          // *** Problem report H00080696
        }
        else
        {
          // Since no updates have occurred for this occurrence of Legal 
          // Referral assignment,
          // create a local error group program error occurrence then go get the
          // next Legal
          // Referral Assignment. A rollback is not needed.
          export.SoftError.Flag = "Y";

          ++export.Error.Index;
          export.Error.CheckSize();

          export.Error.Update.Detail.Code = "CASNF";
          export.Error.Update.Detail.ProgramError1 =
            "Case for Legal Referral not found.";
          export.Error.Update.Detail.KeyInfo = "Office: " + NumberToString
            (entities.ExistingOffice.SystemGeneratedId, 12, 4) + " OSP:";
          export.Error.Update.Detail.KeyInfo =
            TrimEnd(export.Error.Item.Detail.KeyInfo) + " " + NumberToString
            (entities.ExistingServiceProvider.SystemGeneratedId, 11, 5) + " Case:";
            
          export.Error.Update.Detail.KeyInfo =
            TrimEnd(export.Error.Item.Detail.KeyInfo) + " " + entities
            .Case1.Number + " - Case for Legal Referral not found.";

          continue;
        }

        // 11/26/2002  SWSRPDP  PRODFIX  changed the read to NOT use Denor-12 
        // key - instead use CASE key
        // ----------------------------------------------------------------------------
        // REMOVED - AND DESIRED infrastructure denorm_numeric_12 IS EQUAL TO 
        // CURRENT legal_referral identifier from READ EACH
        local.Test.DenormNumeric12 = entities.LegalReferral.Identifier;
      }
      else
      {
        // Since no updates have occurred for this occurrence of Legal Referral 
        // assignment,
        // create a local error group program error occurrence then go get the 
        // next Legal
        // Referral Assignment. A rollback is not needed.
        export.SoftError.Flag = "Y";

        ++export.Error.Index;
        export.Error.CheckSize();

        export.Error.Update.Detail.Code = "LRFNF";
        export.Error.Update.Detail.ProgramError1 = "Legal Referral not found.";
        export.Error.Update.Detail.KeyInfo = "Office: " + NumberToString
          (entities.ExistingOffice.SystemGeneratedId, 12, 4) + " OSP:";
        export.Error.Update.Detail.KeyInfo =
          TrimEnd(export.Error.Item.Detail.KeyInfo) + " " + NumberToString
          (entities.ExistingServiceProvider.SystemGeneratedId, 11, 5) + " Legal Referral not found.";
          

        continue;
      }

      try
      {
        UpdateLegalReferralAssignment();
        ++export.ChkpntNumbUpdates.Count;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "LEGAL_REFERRAL_ASSIGNMENT_NU";

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
        CreateLegalReferralAssignment();
        ++export.BusObjCount.Count;
        ++export.ChkpntNumbCreates.Count;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "LEGAL_REFERRAL_ASSIGNMENT_NU";

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

      // *** 06/13/2000 SWSRCHF
      // *** problem report H00097134
      // *** changed Discontinue date >= Current date
      // ***           to  Discontinue date = Max date
      // 07/12/2010    CQ20565   swdpjlh  Performance change
      foreach(var item1 in ReadInfrastructureMonitoredActivity())
      {
        if (!ReadMonitoredActivityAssignment())
        {
          continue;

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
      local.Infrastructure.EventId = 42;
      local.Infrastructure.BusinessObjectCd = "LRF";
      local.Infrastructure.UserId = global.UserId;
      local.Infrastructure.ReasonCode = "LRFXFR";
      local.Infrastructure.ReferenceDate = local.Current.Date;

      // *** Problem report H00078787
      // *** 11/09/99 SWSRCHF
      local.Infrastructure.Detail =
        "LRF business object transferred via the GBOR batch process.";
      UseSpCabCreateInfrastructure();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        export.SoftError.Flag = "Y";

        ++export.Error.Index;
        export.Error.CheckSize();

        export.Error.Update.Detail.Code = "LRFINFNC";
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
        "'LRF' business object and associated Monitored Activities have been transferred to another SP";
        
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
        "'LRF' business object and associated Monitored Activities have been transferred from another SP";
        
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

  private void CreateLegalReferralAssignment()
  {
    System.Diagnostics.Debug.Assert(entities.LegalReferral.Populated);
    System.Diagnostics.Debug.
      Assert(entities.NewOfficeServiceProvider.Populated);

    var reasonCode = entities.GlobalReassignment.AssignmentReasonCode;
    var overrideInd = entities.ExistingLegalReferralAssignment.OverrideInd;
    var effectiveDate = AddDays(local.Current.Date, 1);
    var discontinueDate = local.Max.Date;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var spdId = entities.NewOfficeServiceProvider.SpdGeneratedId;
    var offId = entities.NewOfficeServiceProvider.OffGeneratedId;
    var ospCode = entities.NewOfficeServiceProvider.RoleCode;
    var ospDate = entities.NewOfficeServiceProvider.EffectiveDate;
    var casNo = entities.LegalReferral.CasNumber;
    var lgrId = entities.LegalReferral.Identifier;

    entities.NewLegalReferralAssignment.Populated = false;
    Update("CreateLegalReferralAssignment",
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
        db.SetString(command, "casNo", casNo);
        db.SetInt32(command, "lgrId", lgrId);
      });

    entities.NewLegalReferralAssignment.ReasonCode = reasonCode;
    entities.NewLegalReferralAssignment.OverrideInd = overrideInd;
    entities.NewLegalReferralAssignment.EffectiveDate = effectiveDate;
    entities.NewLegalReferralAssignment.DiscontinueDate = discontinueDate;
    entities.NewLegalReferralAssignment.CreatedBy = createdBy;
    entities.NewLegalReferralAssignment.CreatedTimestamp = createdTimestamp;
    entities.NewLegalReferralAssignment.LastUpdatedBy = "";
    entities.NewLegalReferralAssignment.LastUpdatedTimestamp = null;
    entities.NewLegalReferralAssignment.SpdId = spdId;
    entities.NewLegalReferralAssignment.OffId = offId;
    entities.NewLegalReferralAssignment.OspCode = ospCode;
    entities.NewLegalReferralAssignment.OspDate = ospDate;
    entities.NewLegalReferralAssignment.CasNo = casNo;
    entities.NewLegalReferralAssignment.LgrId = lgrId;
    entities.NewLegalReferralAssignment.Populated = true;
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
    var macId = entities.MonitoredActivity.SystemGeneratedIdentifier;

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
        db.SetNullableDateTime(command, "lastUpdatedTmst", null);
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
    entities.NewMonitoredActivityAssignment.LastUpdatedBy = "";
    entities.NewMonitoredActivityAssignment.LastUpdatedTimestamp = null;
    entities.NewMonitoredActivityAssignment.SpdId = spdId;
    entities.NewMonitoredActivityAssignment.OffId = offId;
    entities.NewMonitoredActivityAssignment.OspCode = ospCode;
    entities.NewMonitoredActivityAssignment.OspDate = ospDate;
    entities.NewMonitoredActivityAssignment.MacId = macId;
    entities.NewMonitoredActivityAssignment.Populated = true;
  }

  private bool ReadCase()
  {
    System.Diagnostics.Debug.Assert(entities.LegalReferral.Populated);
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.LegalReferral.CasNumber);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.Populated = true;
      });
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

  private IEnumerable<bool> ReadInfrastructureMonitoredActivity()
  {
    entities.Infrastructure.Populated = false;
    entities.MonitoredActivity.Populated = false;

    return ReadEach("ReadInfrastructureMonitoredActivity",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", entities.Case1.Number);
        db.SetString(
          command, "businessObjectCd",
          entities.GlobalReassignment.BusinessObjectCode);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.MonitoredActivity.InfSysGenId = db.GetNullableInt32(reader, 0);
        entities.Infrastructure.SituationNumber = db.GetInt32(reader, 1);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 2);
        entities.Infrastructure.EventId = db.GetInt32(reader, 3);
        entities.Infrastructure.EventType = db.GetString(reader, 4);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 5);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 6);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 7);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 8);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 9);
        entities.Infrastructure.DenormDate = db.GetNullableDate(reader, 10);
        entities.Infrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.Infrastructure.InitiatingStateCode = db.GetString(reader, 12);
        entities.Infrastructure.CsenetInOutCode = db.GetString(reader, 13);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 14);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 15);
        entities.Infrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 16);
        entities.Infrastructure.UserId = db.GetString(reader, 17);
        entities.Infrastructure.CreatedBy = db.GetString(reader, 18);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.Infrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 20);
        entities.Infrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.Infrastructure.Function = db.GetNullableString(reader, 23);
        entities.Infrastructure.CaseUnitState =
          db.GetNullableString(reader, 24);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 25);
        entities.MonitoredActivity.SystemGeneratedIdentifier =
          db.GetInt32(reader, 26);
        entities.MonitoredActivity.ClosureReasonCode =
          db.GetNullableString(reader, 27);
        entities.MonitoredActivity.LastUpdatedBy =
          db.GetNullableString(reader, 28);
        entities.MonitoredActivity.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 29);
        entities.Infrastructure.Populated = true;
        entities.MonitoredActivity.Populated = true;

        return true;
      });
  }

  private bool ReadLegalReferral()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingLegalReferralAssignment.Populated);
    entities.LegalReferral.Populated = false;

    return Read("ReadLegalReferral",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.ExistingLegalReferralAssignment.LgrId);
        db.SetString(
          command, "casNumber", entities.ExistingLegalReferralAssignment.CasNo);
          
      },
      (db, reader) =>
      {
        entities.LegalReferral.CasNumber = db.GetString(reader, 0);
        entities.LegalReferral.Identifier = db.GetInt32(reader, 1);
        entities.LegalReferral.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalReferralAssignment()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingOfficeServiceProvider.Populated);
    entities.ExistingLegalReferralAssignment.Populated = false;

    return ReadEach("ReadLegalReferralAssignment",
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
          command, "reasonCode",
          entities.GlobalReassignment.AssignmentReasonCode);
      },
      (db, reader) =>
      {
        entities.ExistingLegalReferralAssignment.ReasonCode =
          db.GetString(reader, 0);
        entities.ExistingLegalReferralAssignment.OverrideInd =
          db.GetString(reader, 1);
        entities.ExistingLegalReferralAssignment.EffectiveDate =
          db.GetDate(reader, 2);
        entities.ExistingLegalReferralAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.ExistingLegalReferralAssignment.CreatedBy =
          db.GetString(reader, 4);
        entities.ExistingLegalReferralAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.ExistingLegalReferralAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.ExistingLegalReferralAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 7);
        entities.ExistingLegalReferralAssignment.SpdId = db.GetInt32(reader, 8);
        entities.ExistingLegalReferralAssignment.OffId = db.GetInt32(reader, 9);
        entities.ExistingLegalReferralAssignment.OspCode =
          db.GetString(reader, 10);
        entities.ExistingLegalReferralAssignment.OspDate =
          db.GetDate(reader, 11);
        entities.ExistingLegalReferralAssignment.CasNo =
          db.GetString(reader, 12);
        entities.ExistingLegalReferralAssignment.LgrId =
          db.GetInt32(reader, 13);
        entities.ExistingLegalReferralAssignment.Populated = true;

        return true;
      });
  }

  private bool ReadMonitoredActivityAssignment()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingOfficeServiceProvider.Populated);
    entities.ExistingMonitoredActivityAssignment.Populated = false;

    return Read("ReadMonitoredActivityAssignment",
      (db, command) =>
      {
        db.SetInt32(
          command, "macId",
          entities.MonitoredActivity.SystemGeneratedIdentifier);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate", import.Max.Date.GetValueOrDefault());
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
      },
      (db, reader) =>
      {
        entities.ExistingMonitoredActivityAssignment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingMonitoredActivityAssignment.ReasonCode =
          db.GetString(reader, 1);
        entities.ExistingMonitoredActivityAssignment.ResponsibilityCode =
          db.GetString(reader, 2);
        entities.ExistingMonitoredActivityAssignment.EffectiveDate =
          db.GetDate(reader, 3);
        entities.ExistingMonitoredActivityAssignment.OverrideInd =
          db.GetString(reader, 4);
        entities.ExistingMonitoredActivityAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.ExistingMonitoredActivityAssignment.CreatedBy =
          db.GetString(reader, 6);
        entities.ExistingMonitoredActivityAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.ExistingMonitoredActivityAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.ExistingMonitoredActivityAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 9);
        entities.ExistingMonitoredActivityAssignment.SpdId =
          db.GetInt32(reader, 10);
        entities.ExistingMonitoredActivityAssignment.OffId =
          db.GetInt32(reader, 11);
        entities.ExistingMonitoredActivityAssignment.OspCode =
          db.GetString(reader, 12);
        entities.ExistingMonitoredActivityAssignment.OspDate =
          db.GetDate(reader, 13);
        entities.ExistingMonitoredActivityAssignment.MacId =
          db.GetInt32(reader, 14);
        entities.ExistingMonitoredActivityAssignment.Populated = true;
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
    entities.ExistingOfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider1",
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

  private bool ReadOfficeServiceProvider2()
  {
    System.Diagnostics.Debug.Assert(entities.GlobalReassignment.Populated);
    entities.NewOfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider2",
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
        entities.NewServiceProvider.LastName = db.GetString(reader, 2);
        entities.NewServiceProvider.FirstName = db.GetString(reader, 3);
        entities.NewServiceProvider.Populated = true;
      });
  }

  private void UpdateLegalReferralAssignment()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingLegalReferralAssignment.Populated);

    var discontinueDate = local.Current.Date;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.ExistingLegalReferralAssignment.Populated = false;
    Update("UpdateLegalReferralAssignment",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetDateTime(
          command, "createdTimestamp",
          entities.ExistingLegalReferralAssignment.CreatedTimestamp.
            GetValueOrDefault());
        db.SetInt32(
          command, "spdId", entities.ExistingLegalReferralAssignment.SpdId);
        db.SetInt32(
          command, "offId", entities.ExistingLegalReferralAssignment.OffId);
        db.SetString(
          command, "ospCode", entities.ExistingLegalReferralAssignment.OspCode);
          
        db.SetDate(
          command, "ospDate",
          entities.ExistingLegalReferralAssignment.OspDate.GetValueOrDefault());
          
        db.SetString(
          command, "casNo", entities.ExistingLegalReferralAssignment.CasNo);
        db.SetInt32(
          command, "lgrId", entities.ExistingLegalReferralAssignment.LgrId);
      });

    entities.ExistingLegalReferralAssignment.DiscontinueDate = discontinueDate;
    entities.ExistingLegalReferralAssignment.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingLegalReferralAssignment.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.ExistingLegalReferralAssignment.Populated = true;
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
    /// A value of Test.
    /// </summary>
    [JsonPropertyName("test")]
    public Infrastructure Test
    {
      get => test ??= new();
      set => test = value;
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
    /// A value of Commit.
    /// </summary>
    [JsonPropertyName("commit")]
    public Common Commit
    {
      get => commit ??= new();
      set => commit = value;
    }

    private Infrastructure test;
    private DateWorkArea initialized;
    private DateWorkArea max;
    private DateWorkArea current;
    private WorkArea holdSpNum;
    private WorkArea holdOffcNum;
    private OfficeServiceProviderAlert officeServiceProviderAlert;
    private Infrastructure infrastructure;
    private Common commit;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
    /// A value of LegalReferral.
    /// </summary>
    [JsonPropertyName("legalReferral")]
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
    }

    /// <summary>
    /// A value of NewLegalReferralAssignment.
    /// </summary>
    [JsonPropertyName("newLegalReferralAssignment")]
    public LegalReferralAssignment NewLegalReferralAssignment
    {
      get => newLegalReferralAssignment ??= new();
      set => newLegalReferralAssignment = value;
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

    private Case1 case1;
    private LegalReferral legalReferral;
    private LegalReferralAssignment newLegalReferralAssignment;
    private LegalReferralAssignment existingLegalReferralAssignment;
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
  }
#endregion
}
