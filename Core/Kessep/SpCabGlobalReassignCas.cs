// Program: SP_CAB_GLOBAL_REASSIGN_CAS, ID: 372783715, model: 746.
// Short name: SWE02205
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_GLOBAL_REASSIGN_CAS.
/// </summary>
[Serializable]
public partial class SpCabGlobalReassignCas: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_GLOBAL_REASSIGN_CAS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabGlobalReassignCas(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabGlobalReassignCas.
  /// </summary>
  public SpCabGlobalReassignCas(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------
    //                      M A I N T E N A N C E    L O G
    // -------------------------------------------------------------------
    // Date	        Developer	Request       Description
    // -------------------------------------------------------------------
    // 02/02/1998	J.Rookard	              Initial development
    // 08/25/1999	M Ramirez	516           Pass in infrastructure
    //                                               
    // reference_date to
    //                                               
    // sp_create_document_infrastruct
    //                                               
    // for CASETXFR document so that
    //                                               
    // it will be printed when the
    //                                               
    // assignment is effective.
    // 09/17/1999	M Ramirez	H00073450     Don't send the document if
    // 				              the AR is not a client
    // -------------------------------------------------------------------
    // 09/30/1999  SWSRCHF  H00073391  Expanded the code to allow the
    //                                 
    // commit point to be table driven
    // 02/23/2000  SWSRCHF  H00089087  Allow all MONA's stemming from an
    //                                 
    // event that has a business object
    // of
    //                                 
    // LEA to be transferred
    // 06/13/2000  SWSRCHF  H00097134  Changed the Discontinue Date check to 
    // check for
    //                                 
    // MAX date on the Case Assignment,
    // Case Unit Function
    //                                 
    // Assignment and Monitored
    // Activity Assignment reads
    // 10/13/2000  SWDPARM   H95478    Added LRF to the Case of  infrastructure 
    // business
    //                                 
    // object code
    // 03/01/2002  Mashworth PR138467  Added use of cab sp determine interstate 
    // doc.
    //                                 
    // PR 138467 - Do not send casetxfr
    // letter if
    //                                 
    // incoming interstate case
    // 06/05/2002  M.Lachowicz PR144106 Do not transfer Monitored Activitity
    //                                 
    // Assignment for LRF if Legal
    //                                 
    // Referral Assignment is different
    // than
    //                                 
    // new office service provider.
    // 06/19/2002  M.Lachowicz PR148287 Do not transfer Monitored Activitity
    //                                 
    // Assignment for LEA if old office
    //                                 
    // service provider Monitored
    //                                 
    // Activity Assignment is different
    // than
    //                                 
    // this one in Global Reassignment.
    // 11/20/2002  M.Lachowicz WR20258B Reassigning Interstate Case when case is
    //                         PR267661  reassigned.
    // -------------------------------------------------------------------
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
    // 11/17/2011  GVandy	CQ30161    1) Do not trigger CASETXFR letters.  The 
    // letters will now be
    // 				   triggered by new batch program SP_B703_CASETXFR_GENERATION.
    // 				   2) Always raise CASEXFR event.
    // 11/04/2013  GVandy	CQ41883    Interstate case re-assignment logic is not 
    // reading role code
    // 				   and effective date when determining if the office service
    // 				   provider is changing.
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

    if (ReadOfficeServiceProvider4())
    {
      if (ReadServiceProvider2())
      {
        local.ExistingServiceProvider.SystemGeneratedId =
          entities.ExistingServiceProvider.SystemGeneratedId;
      }
      else
      {
        ExitState = "SERVICE_PROVIDER_NF";

        return;
      }

      if (ReadOffice1())
      {
        local.ExistingOffice.SystemGeneratedId =
          entities.ExistingOffice.SystemGeneratedId;
      }
      else
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
      if (ReadServiceProvider1())
      {
        local.NewServiceProvider.SystemGeneratedId =
          entities.NewServiceProvider.SystemGeneratedId;
      }
      else
      {
        ExitState = "SERVICE_PROVIDER_NF";

        return;
      }

      if (ReadOffice2())
      {
        local.NewOffice.SystemGeneratedId =
          entities.NewOffice.SystemGeneratedId;
      }
      else
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
    foreach(var item in ReadCaseAssignment())
    {
      if (AsChar(entities.GlobalReassignment.OverrideFlag) == 'Y')
      {
      }
      else if (AsChar(entities.ExistingCaseAssignment.OverrideInd) == 'Y')
      {
        continue;
      }

      if (ReadCase())
      {
        if (AsChar(entities.Case1.Status) == 'C')
        {
          continue;
        }
      }
      else
      {
        // Since no updates have occurred for this occurrence of Case 
        // assignment,
        // create a local error group program error occurrence then go get the 
        // next Case
        // Assignment. A rollback is not needed.
        export.SoftError.Flag = "Y";

        ++export.Error.Index;
        export.Error.CheckSize();

        export.Error.Update.Detail.Code = "CASNF";
        export.Error.Update.Detail.KeyInfo = "Office: " + NumberToString
          (entities.ExistingOffice.SystemGeneratedId, 12, 4) + " OSP:";
        export.Error.Update.Detail.KeyInfo =
          TrimEnd(export.Error.Item.Detail.KeyInfo) + " " + NumberToString
          (entities.ExistingServiceProvider.SystemGeneratedId, 11, 5) + " Case:";
          
        export.Error.Update.Detail.KeyInfo =
          TrimEnd(export.Error.Item.Detail.KeyInfo) + " " + entities
          .Case1.Number + " - Case Not Found";

        continue;
      }

      try
      {
        UpdateCaseAssignment();
        ++export.ChkpntNumbUpdates.Count;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "CASE_ASSIGNMENT_NU";

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
        CreateCaseAssignment();
        ++export.BusObjCount.Count;
        ++export.ChkpntNumbCreates.Count;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "SP0000_CASE_ASSIGNMENT_AE";

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

      // 11/17/2011 GVandy  CQ30161 Raise the CASEXFR event for all cases.
      local.Infrastructure.SituationNumber = 0;
      local.Infrastructure.ProcessStatus = "Q";
      local.Infrastructure.EventId = 5;
      local.Infrastructure.BusinessObjectCd = "CAS";
      local.Infrastructure.UserId = global.UserId;
      local.Infrastructure.ReasonCode = "CASEXFR";
      local.Infrastructure.CaseNumber = entities.Case1.Number;
      local.Infrastructure.ReferenceDate = local.Current.Date;
      local.Infrastructure.Detail = "Case transferred from :";
      local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + entities
        .ExistingServiceProvider.UserId + " to " + entities
        .NewServiceProvider.UserId;
      local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + " in Office: " +
        NumberToString(entities.NewOffice.SystemGeneratedId, 12, 4);

      if (ReadInterstateRequest1())
      {
        local.Infrastructure.InitiatingStateCode = "OS";
      }
      else
      {
        local.Infrastructure.InitiatingStateCode = "KS";
      }

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

      // 11/17/2011 GVandy  CQ30161 Do not trigger CASETXFR letters.  The 
      // letters will now be
      // triggered by new batch program SP_B703_CASETXFR_GENERATION.
      // --------------------
      // CSEnet functionality
      // --------------------
      if (ReadInterstateRequest2())
      {
        // ------------------------------
        // USE SI PROCESS EVENT TRANS FOR OC IC
        // ------------------------------
        local.ScreenIdentification.Command = "ASIN";
        UseSiCreateAutoCsenetTrans();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          return;
        }
      }
      else
      {
        // ------------------------------
        // CSEnet processing not required
        // ------------------------------
      }

      foreach(var item1 in ReadCaseUnit())
      {
        // *** 06/13/2000 SWSRCHF
        // *** problem report H00097134
        // *** changed Discontinue date >= Current date
        // ***           to  Discontinue date = Max date
        foreach(var item2 in ReadCaseUnitFunctionAssignmt())
        {
          if (AsChar(entities.GlobalReassignment.OverrideFlag) == 'Y')
          {
          }
          else if (AsChar(entities.ExistingCaseUnitFunctionAssignmt.OverrideInd) ==
            'Y')
          {
            continue;
          }

          try
          {
            UpdateCaseUnitFunctionAssignmt();
            ++export.ChkpntNumbUpdates.Count;
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "CASE_UNIT_FUNCTION_ASSIGNMT_NU";

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
            CreateCaseUnitFunctionAssignmt();
            ++export.BusObjCount.Count;
            ++export.ChkpntNumbCreates.Count;
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "CASE_UNIT_FUNCTION_ASSIGNMT_NU";

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

      // *** 06/13/2000 SWSRCHF
      // *** problem report H00097134
      // *** changed Discontinue date >= Current date
      // ***           to  Discontinue date = Max date
      foreach(var item1 in ReadMonitoredActivityAssignmentMonitoredActivity())
      {
        if (ReadInfrastructure())
        {
          switch(TrimEnd(entities.Infrastructure.BusinessObjectCd))
          {
            case "LRF":
              // 06/05/2002 M.L Start
              if (!ReadLegalReferralAssignment())
              {
                continue;
              }

              // 06/05/2002 M.L End
              break;
            case "CAS":
              break;
            case "CAU":
              break;
            case "CSP":
              break;
            case "PHI":
              break;
            case "INC":
              break;
            case "CPA":
              break;
            case "ICS":
              break;
            case "BKR":
              break;
            case "PPR":
              break;
            case "CPR":
              break;
            case "CSW":
              break;
            case "GNT":
              break;
            case "PGT":
              break;
            case "CON":
              break;
            case "MIL":
              break;
            case "PAR":
              break;
            case "PIH":
              break;
            case "HIN":
              break;
            case "FPL":
              break;
            case "LEA":
              // *** Problem Report H00089087
              // *** 02/23/00 SWSRCHF
              // *** Start
              // *** End
              // *** 02/23/00 SWSRCHF
              // *** Problem Report H00089087
              // 06/19/2002 M. Lachowicz Start
              if (!ReadOfficeServiceProvider1())
              {
                continue;
              }

              // 06/19/2002 M. Lachowicz End
              break;
            default:
              continue;
          }
        }
        else
        {
          ExitState = "INFRASTRUCTURE_NF";

          return;
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

      // 11/20/2002 M.Lachowicz Start
      // **********************************************************************************
      // Inter State Case Assignments.
      // **********************************************************************************
      local.InterstateCase.KsCaseId = entities.Case1.Number;

      foreach(var item1 in ReadInterstateCase())
      {
        if (!ReadInterstateCaseAssignment())
        {
          continue;
        }

        MoveInterstateCaseAssignment(entities.OldInterstateCaseAssignment,
          local.InterstateCaseAssignment);

        if (AsChar(entities.OldInterstateCaseAssignment.OverrideInd) == 'N' || AsChar
          (entities.GlobalReassignment.OverrideFlag) == 'Y')
        {
          // 11/04/2013  GVandy  CQ41883 Interstate case re-assignment logic is 
          // not reading role code
          // and effective date when determining if the office service provider 
          // is changing.
          if (ReadOfficeServiceProvider3())
          {
            // **********************************************************************************
            // Interstate case is already assigned to the case worker. Read Next
            // Interstate Case.
            // **********************************************************************************
            continue;
          }
        }
        else if (AsChar(entities.OldInterstateCaseAssignment.OverrideInd) == 'Y'
          )
        {
          continue;
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
              ExitState = "SI0000_INT_CASE_UPDATE_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        try
        {
          CreateInterstateCaseAssignment();
          ++export.MonActCount.Count;
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

      // 11/20/2002 M.Lachowicz End
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
      local.Infrastructure.EventId = 5;
      local.Infrastructure.BusinessObjectCd = "CAS";
      local.Infrastructure.UserId = global.UserId;
      local.Infrastructure.ReasonCode = "CASEXFR";
      local.Infrastructure.ReferenceDate = local.Current.Date;
      local.Infrastructure.Detail =
        "CASE business object transferred via the GBOR batch process.";
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
        "'CAS' & 'CUF' business objects and associated Monitored Activities have been transferred to another SP";
        
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
        "'CAS' & 'CUF' business objects and associated Monitored Activities have been transferred from another SP";
        
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

  private static void MoveCase1(Case1 source, Case1 target)
  {
    target.Number = source.Number;
    target.Status = source.Status;
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

  private static void MoveInterstateCaseAssignment(
    InterstateCaseAssignment source, InterstateCaseAssignment target)
  {
    target.ReasonCode = source.ReasonCode;
    target.OverrideInd = source.OverrideInd;
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

  private void UseSiCreateAutoCsenetTrans()
  {
    var useImport = new SiCreateAutoCsenetTrans.Import();
    var useExport = new SiCreateAutoCsenetTrans.Export();

    MoveCase1(entities.Case1, useImport.Case1);
    useImport.ScreenIdentification.Command = local.ScreenIdentification.Command;

    Call(SiCreateAutoCsenetTrans.Execute, useImport, useExport);
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

  private void CreateCaseAssignment()
  {
    System.Diagnostics.Debug.
      Assert(entities.NewOfficeServiceProvider.Populated);

    var reasonCode = entities.GlobalReassignment.AssignmentReasonCode;
    var overrideInd = entities.ExistingCaseAssignment.OverrideInd;
    var effectiveDate = AddDays(local.Current.Date, 1);
    var discontinueDate = import.Max.Date;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var spdId = entities.NewOfficeServiceProvider.SpdGeneratedId;
    var offId = entities.NewOfficeServiceProvider.OffGeneratedId;
    var ospCode = entities.NewOfficeServiceProvider.RoleCode;
    var ospDate = entities.NewOfficeServiceProvider.EffectiveDate;
    var casNo = entities.Case1.Number;

    entities.NewCaseAssignment.Populated = false;
    Update("CreateCaseAssignment",
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
      });

    entities.NewCaseAssignment.ReasonCode = reasonCode;
    entities.NewCaseAssignment.OverrideInd = overrideInd;
    entities.NewCaseAssignment.EffectiveDate = effectiveDate;
    entities.NewCaseAssignment.DiscontinueDate = discontinueDate;
    entities.NewCaseAssignment.CreatedBy = createdBy;
    entities.NewCaseAssignment.CreatedTimestamp = createdTimestamp;
    entities.NewCaseAssignment.LastUpdatedBy = "";
    entities.NewCaseAssignment.LastUpdatedTimestamp = null;
    entities.NewCaseAssignment.SpdId = spdId;
    entities.NewCaseAssignment.OffId = offId;
    entities.NewCaseAssignment.OspCode = ospCode;
    entities.NewCaseAssignment.OspDate = ospDate;
    entities.NewCaseAssignment.CasNo = casNo;
    entities.NewCaseAssignment.Populated = true;
  }

  private void CreateCaseUnitFunctionAssignmt()
  {
    System.Diagnostics.Debug.Assert(entities.CaseUnit.Populated);
    System.Diagnostics.Debug.
      Assert(entities.NewOfficeServiceProvider.Populated);

    var reasonCode = entities.GlobalReassignment.AssignmentReasonCode;
    var overrideInd = entities.ExistingCaseUnitFunctionAssignmt.OverrideInd;
    var effectiveDate = AddDays(local.Current.Date, 1);
    var discontinueDate = import.Max.Date;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var spdId = entities.NewOfficeServiceProvider.SpdGeneratedId;
    var offId = entities.NewOfficeServiceProvider.OffGeneratedId;
    var ospCode = entities.NewOfficeServiceProvider.RoleCode;
    var ospDate = entities.NewOfficeServiceProvider.EffectiveDate;
    var csuNo = entities.CaseUnit.CuNumber;
    var casNo = entities.CaseUnit.CasNo;
    var function = entities.ExistingCaseUnitFunctionAssignmt.Function;

    entities.NewCaseUnitFunctionAssignmt.Populated = false;
    Update("CreateCaseUnitFunctionAssignmt",
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
        db.SetInt32(command, "csuNo", csuNo);
        db.SetString(command, "casNo", casNo);
        db.SetString(command, "function", function);
      });

    entities.NewCaseUnitFunctionAssignmt.ReasonCode = reasonCode;
    entities.NewCaseUnitFunctionAssignmt.OverrideInd = overrideInd;
    entities.NewCaseUnitFunctionAssignmt.EffectiveDate = effectiveDate;
    entities.NewCaseUnitFunctionAssignmt.DiscontinueDate = discontinueDate;
    entities.NewCaseUnitFunctionAssignmt.CreatedBy = createdBy;
    entities.NewCaseUnitFunctionAssignmt.CreatedTimestamp = createdTimestamp;
    entities.NewCaseUnitFunctionAssignmt.LastUpdatedBy = "";
    entities.NewCaseUnitFunctionAssignmt.LastUpdatedTimestamp = null;
    entities.NewCaseUnitFunctionAssignmt.SpdId = spdId;
    entities.NewCaseUnitFunctionAssignmt.OffId = offId;
    entities.NewCaseUnitFunctionAssignmt.OspCode = ospCode;
    entities.NewCaseUnitFunctionAssignmt.OspDate = ospDate;
    entities.NewCaseUnitFunctionAssignmt.CsuNo = csuNo;
    entities.NewCaseUnitFunctionAssignmt.CasNo = casNo;
    entities.NewCaseUnitFunctionAssignmt.Function = function;
    entities.NewCaseUnitFunctionAssignmt.Populated = true;
  }

  private void CreateInterstateCaseAssignment()
  {
    System.Diagnostics.Debug.
      Assert(entities.NewOfficeServiceProvider.Populated);

    var reasonCode = local.InterstateCaseAssignment.ReasonCode;
    var overrideInd = local.InterstateCaseAssignment.OverrideInd;
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
    System.Diagnostics.Debug.Assert(entities.ExistingCaseAssignment.Populated);
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.ExistingCaseAssignment.CasNo);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCaseAssignment()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingOfficeServiceProvider.Populated);
    entities.ExistingCaseAssignment.Populated = false;

    return ReadEach("ReadCaseAssignment",
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
        entities.ExistingCaseAssignment.ReasonCode = db.GetString(reader, 0);
        entities.ExistingCaseAssignment.OverrideInd = db.GetString(reader, 1);
        entities.ExistingCaseAssignment.EffectiveDate = db.GetDate(reader, 2);
        entities.ExistingCaseAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.ExistingCaseAssignment.CreatedBy = db.GetString(reader, 4);
        entities.ExistingCaseAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.ExistingCaseAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.ExistingCaseAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 7);
        entities.ExistingCaseAssignment.SpdId = db.GetInt32(reader, 8);
        entities.ExistingCaseAssignment.OffId = db.GetInt32(reader, 9);
        entities.ExistingCaseAssignment.OspCode = db.GetString(reader, 10);
        entities.ExistingCaseAssignment.OspDate = db.GetDate(reader, 11);
        entities.ExistingCaseAssignment.CasNo = db.GetString(reader, 12);
        entities.ExistingCaseAssignment.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseUnit()
  {
    entities.CaseUnit.Populated = false;

    return ReadEach("ReadCaseUnit",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
        db.
          SetDate(command, "startDate", local.Current.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.StartDate = db.GetDate(reader, 1);
        entities.CaseUnit.ClosureDate = db.GetNullableDate(reader, 2);
        entities.CaseUnit.ClosureReasonCode = db.GetNullableString(reader, 3);
        entities.CaseUnit.CasNo = db.GetString(reader, 4);
        entities.CaseUnit.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseUnitFunctionAssignmt()
  {
    System.Diagnostics.Debug.Assert(entities.CaseUnit.Populated);
    entities.ExistingCaseUnitFunctionAssignmt.Populated = false;

    return ReadEach("ReadCaseUnitFunctionAssignmt",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.CaseUnit.CasNo);
        db.SetInt32(command, "csuNo", entities.CaseUnit.CuNumber);
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
        entities.ExistingCaseUnitFunctionAssignmt.ReasonCode =
          db.GetString(reader, 0);
        entities.ExistingCaseUnitFunctionAssignmt.OverrideInd =
          db.GetString(reader, 1);
        entities.ExistingCaseUnitFunctionAssignmt.EffectiveDate =
          db.GetDate(reader, 2);
        entities.ExistingCaseUnitFunctionAssignmt.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.ExistingCaseUnitFunctionAssignmt.CreatedBy =
          db.GetString(reader, 4);
        entities.ExistingCaseUnitFunctionAssignmt.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.ExistingCaseUnitFunctionAssignmt.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.ExistingCaseUnitFunctionAssignmt.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 7);
        entities.ExistingCaseUnitFunctionAssignmt.SpdId =
          db.GetInt32(reader, 8);
        entities.ExistingCaseUnitFunctionAssignmt.OffId =
          db.GetInt32(reader, 9);
        entities.ExistingCaseUnitFunctionAssignmt.OspCode =
          db.GetString(reader, 10);
        entities.ExistingCaseUnitFunctionAssignmt.OspDate =
          db.GetDate(reader, 11);
        entities.ExistingCaseUnitFunctionAssignmt.CsuNo =
          db.GetInt32(reader, 12);
        entities.ExistingCaseUnitFunctionAssignmt.CasNo =
          db.GetString(reader, 13);
        entities.ExistingCaseUnitFunctionAssignmt.Function =
          db.GetString(reader, 14);
        entities.ExistingCaseUnitFunctionAssignmt.Populated = true;

        return true;
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

  private bool ReadInfrastructure()
  {
    System.Diagnostics.Debug.Assert(entities.MonitoredActivity.Populated);
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          entities.MonitoredActivity.InfSysGenId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
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
        entities.Infrastructure.Populated = true;
      });
  }

  private IEnumerable<bool> ReadInterstateCase()
  {
    entities.InterstateCase.Populated = false;

    return ReadEach("ReadInterstateCase",
      (db, command) =>
      {
        db.SetNullableString(
          command, "ksCaseId", local.InterstateCase.KsCaseId ?? "");
      },
      (db, reader) =>
      {
        entities.InterstateCase.TransSerialNumber = db.GetInt64(reader, 0);
        entities.InterstateCase.TransactionDate = db.GetDate(reader, 1);
        entities.InterstateCase.KsCaseId = db.GetNullableString(reader, 2);
        entities.InterstateCase.Populated = true;

        return true;
      });
  }

  private bool ReadInterstateCaseAssignment()
  {
    entities.OldInterstateCaseAssignment.Populated = false;

    return Read("ReadInterstateCaseAssignment",
      (db, command) =>
      {
        db.
          SetInt64(command, "icsNo", entities.InterstateCase.TransSerialNumber);
          
        db.SetDate(
          command, "icsDate",
          entities.InterstateCase.TransactionDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.OldInterstateCaseAssignment.ReasonCode =
          db.GetString(reader, 0);
        entities.OldInterstateCaseAssignment.OverrideInd =
          db.GetString(reader, 1);
        entities.OldInterstateCaseAssignment.EffectiveDate =
          db.GetDate(reader, 2);
        entities.OldInterstateCaseAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.OldInterstateCaseAssignment.CreatedBy =
          db.GetString(reader, 4);
        entities.OldInterstateCaseAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.OldInterstateCaseAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.OldInterstateCaseAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 7);
        entities.OldInterstateCaseAssignment.SpdId = db.GetInt32(reader, 8);
        entities.OldInterstateCaseAssignment.OffId = db.GetInt32(reader, 9);
        entities.OldInterstateCaseAssignment.OspCode = db.GetString(reader, 10);
        entities.OldInterstateCaseAssignment.OspDate = db.GetDate(reader, 11);
        entities.OldInterstateCaseAssignment.IcsDate = db.GetDate(reader, 12);
        entities.OldInterstateCaseAssignment.IcsNo = db.GetInt64(reader, 13);
        entities.OldInterstateCaseAssignment.Populated = true;
      });
  }

  private bool ReadInterstateRequest1()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest1",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 1);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 2);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 3);
        entities.InterstateRequest.Populated = true;
      });
  }

  private bool ReadInterstateRequest2()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest2",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 1);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 2);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 3);
        entities.InterstateRequest.Populated = true;
      });
  }

  private bool ReadLegalReferralAssignment()
  {
    System.Diagnostics.Debug.
      Assert(entities.NewOfficeServiceProvider.Populated);
    entities.ExistingLegalReferralAssignment.Populated = false;

    return Read("ReadLegalReferralAssignment",
      (db, command) =>
      {
        db.SetDate(
          command, "ospDate",
          entities.NewOfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.SetString(
          command, "ospCode", entities.NewOfficeServiceProvider.RoleCode);
        db.SetInt32(
          command, "offId", entities.NewOfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdId", entities.NewOfficeServiceProvider.SpdGeneratedId);
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
        db.SetString(command, "casNo", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ExistingLegalReferralAssignment.ReasonCode =
          db.GetString(reader, 0);
        entities.ExistingLegalReferralAssignment.EffectiveDate =
          db.GetDate(reader, 1);
        entities.ExistingLegalReferralAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.ExistingLegalReferralAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.ExistingLegalReferralAssignment.SpdId = db.GetInt32(reader, 4);
        entities.ExistingLegalReferralAssignment.OffId = db.GetInt32(reader, 5);
        entities.ExistingLegalReferralAssignment.OspCode =
          db.GetString(reader, 6);
        entities.ExistingLegalReferralAssignment.OspDate =
          db.GetDate(reader, 7);
        entities.ExistingLegalReferralAssignment.CasNo =
          db.GetString(reader, 8);
        entities.ExistingLegalReferralAssignment.LgrId = db.GetInt32(reader, 9);
        entities.ExistingLegalReferralAssignment.Populated = true;
      });
  }

  private IEnumerable<bool> ReadMonitoredActivityAssignmentMonitoredActivity()
  {
    entities.MonitoredActivity.Populated = false;
    entities.ExistingMonitoredActivityAssignment.Populated = false;

    return ReadEach("ReadMonitoredActivityAssignmentMonitoredActivity",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate", import.Max.Date.GetValueOrDefault());
        db.SetNullableString(command, "caseNumber", entities.Case1.Number);
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
        entities.MonitoredActivity.SystemGeneratedIdentifier =
          db.GetInt32(reader, 14);
        entities.MonitoredActivity.ActivityControlNumber =
          db.GetInt32(reader, 15);
        entities.MonitoredActivity.TypeCode = db.GetNullableString(reader, 16);
        entities.MonitoredActivity.ClosureReasonCode =
          db.GetNullableString(reader, 17);
        entities.MonitoredActivity.LastUpdatedBy =
          db.GetNullableString(reader, 18);
        entities.MonitoredActivity.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 19);
        entities.MonitoredActivity.InfSysGenId =
          db.GetNullableInt32(reader, 20);
        entities.MonitoredActivity.Populated = true;
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
    entities.Mona.Populated = false;

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
          command, "offGeneratedId1",
          entities.ExistingMonitoredActivityAssignment.OffId);
        db.SetInt32(
          command, "spdGeneratedId1",
          entities.ExistingMonitoredActivityAssignment.SpdId);
        db.SetInt32(
          command, "offGeneratedId2",
          entities.ExistingOffice.SystemGeneratedId);
        db.SetInt32(
          command, "spdGeneratedId2",
          entities.ExistingServiceProvider.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.Mona.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.Mona.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Mona.RoleCode = db.GetString(reader, 2);
        entities.Mona.EffectiveDate = db.GetDate(reader, 3);
        entities.Mona.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.Mona.Populated = true;
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

  private bool ReadOfficeServiceProvider3()
  {
    System.Diagnostics.Debug.Assert(
      entities.OldInterstateCaseAssignment.Populated);
    entities.OldInt.Populated = false;

    return Read("ReadOfficeServiceProvider3",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate1",
          entities.OldInterstateCaseAssignment.OspDate.GetValueOrDefault());
        db.SetString(
          command, "roleCode1", entities.OldInterstateCaseAssignment.OspCode);
        db.SetInt32(
          command, "offGeneratedId1",
          entities.OldInterstateCaseAssignment.OffId);
        db.SetInt32(
          command, "spdGeneratedId1",
          entities.OldInterstateCaseAssignment.SpdId);
        db.SetInt32(
          command, "spdGeneratedId2",
          local.NewServiceProvider.SystemGeneratedId);
        db.SetInt32(
          command, "offGeneratedId2", local.NewOffice.SystemGeneratedId);
        db.SetString(
          command, "roleCode2", entities.NewOfficeServiceProvider.RoleCode);
        db.SetDate(
          command, "effectiveDate2",
          entities.NewOfficeServiceProvider.EffectiveDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.OldInt.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.OldInt.OffGeneratedId = db.GetInt32(reader, 1);
        entities.OldInt.RoleCode = db.GetString(reader, 2);
        entities.OldInt.EffectiveDate = db.GetDate(reader, 3);
        entities.OldInt.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.OldInt.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvider4()
  {
    System.Diagnostics.Debug.Assert(entities.GlobalReassignment.Populated);
    entities.ExistingOfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider4",
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

  private void UpdateCaseAssignment()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCaseAssignment.Populated);

    var discontinueDate = local.Current.Date;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.ExistingCaseAssignment.Populated = false;
    Update("UpdateCaseAssignment",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetDateTime(
          command, "createdTimestamp",
          entities.ExistingCaseAssignment.CreatedTimestamp.GetValueOrDefault());
          
        db.SetInt32(command, "spdId", entities.ExistingCaseAssignment.SpdId);
        db.SetInt32(command, "offId", entities.ExistingCaseAssignment.OffId);
        db.
          SetString(command, "ospCode", entities.ExistingCaseAssignment.OspCode);
          
        db.SetDate(
          command, "ospDate",
          entities.ExistingCaseAssignment.OspDate.GetValueOrDefault());
        db.SetString(command, "casNo", entities.ExistingCaseAssignment.CasNo);
      });

    entities.ExistingCaseAssignment.DiscontinueDate = discontinueDate;
    entities.ExistingCaseAssignment.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingCaseAssignment.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.ExistingCaseAssignment.Populated = true;
  }

  private void UpdateCaseUnitFunctionAssignmt()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingCaseUnitFunctionAssignmt.Populated);

    var discontinueDate = local.Current.Date;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.ExistingCaseUnitFunctionAssignmt.Populated = false;
    Update("UpdateCaseUnitFunctionAssignmt",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetDateTime(
          command, "createdTimestamp",
          entities.ExistingCaseUnitFunctionAssignmt.CreatedTimestamp.
            GetValueOrDefault());
        db.SetInt32(
          command, "spdId", entities.ExistingCaseUnitFunctionAssignmt.SpdId);
        db.SetInt32(
          command, "offId", entities.ExistingCaseUnitFunctionAssignmt.OffId);
        db.SetString(
          command, "ospCode",
          entities.ExistingCaseUnitFunctionAssignmt.OspCode);
        db.SetDate(
          command, "ospDate",
          entities.ExistingCaseUnitFunctionAssignmt.OspDate.
            GetValueOrDefault());
        db.SetInt32(
          command, "csuNo", entities.ExistingCaseUnitFunctionAssignmt.CsuNo);
        db.SetString(
          command, "casNo", entities.ExistingCaseUnitFunctionAssignmt.CasNo);
      });

    entities.ExistingCaseUnitFunctionAssignmt.DiscontinueDate = discontinueDate;
    entities.ExistingCaseUnitFunctionAssignmt.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingCaseUnitFunctionAssignmt.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.ExistingCaseUnitFunctionAssignmt.Populated = true;
  }

  private void UpdateInterstateCaseAssignment()
  {
    System.Diagnostics.Debug.Assert(
      entities.OldInterstateCaseAssignment.Populated);

    var discontinueDate = local.Current.Date;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.OldInterstateCaseAssignment.Populated = false;
    Update("UpdateInterstateCaseAssignment",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetDateTime(
          command, "createdTimestamp",
          entities.OldInterstateCaseAssignment.CreatedTimestamp.
            GetValueOrDefault());
        db.
          SetInt32(command, "spdId", entities.OldInterstateCaseAssignment.SpdId);
          
        db.
          SetInt32(command, "offId", entities.OldInterstateCaseAssignment.OffId);
          
        db.SetString(
          command, "ospCode", entities.OldInterstateCaseAssignment.OspCode);
        db.SetDate(
          command, "ospDate",
          entities.OldInterstateCaseAssignment.OspDate.GetValueOrDefault());
        db.SetDate(
          command, "icsDate",
          entities.OldInterstateCaseAssignment.IcsDate.GetValueOrDefault());
        db.
          SetInt64(command, "icsNo", entities.OldInterstateCaseAssignment.IcsNo);
          
      });

    entities.OldInterstateCaseAssignment.DiscontinueDate = discontinueDate;
    entities.OldInterstateCaseAssignment.LastUpdatedBy = lastUpdatedBy;
    entities.OldInterstateCaseAssignment.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.OldInterstateCaseAssignment.Populated = true;
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
    /// A value of NewServiceProvider.
    /// </summary>
    [JsonPropertyName("newServiceProvider")]
    public ServiceProvider NewServiceProvider
    {
      get => newServiceProvider ??= new();
      set => newServiceProvider = value;
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
    private ServiceProvider existingServiceProvider;
    private Office existingOffice;
    private ServiceProvider newServiceProvider;
    private Office newOffice;
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
