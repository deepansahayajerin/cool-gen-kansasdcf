// Program: SP_CAB_REASSIGN_CASE, ID: 372318282, model: 746.
// Short name: SWE01918
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_REASSIGN_CASE.
/// </summary>
[Serializable]
public partial class SpCabReassignCase: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_REASSIGN_CASE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabReassignCase(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabReassignCase.
  /// </summary>
  public SpCabReassignCase(IContext context, Import import, Export export):
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
    // Date	By	IDCR#	Description
    // ?????	Rick D		Initial Creation
    // 012897	govind		Rewritten to fix.
    // 052797  Siraj           More fixes (overlapping assignment)
    // 102797	Siraj		Performance tuning, DB2 locking on extended READs, 
    // performance of persistent views.
    // Rewrote logic on reassigning monitored activities based on logic used in 
    // from SP_CAB_BATCH_ASSSOC_CAS_TO_COORD
    // ********************************************
    // -------------------------------------------------------------------
    //   Date   Developer   Request #   Description
    // -------------------------------------------------------------------
    // 02/09/00 SWSRCHF     H00082899   ALERT(s) and DMON(s) are not being
    //                                  
    // transferred with Case
    // 03/14/00 SWSRCHF     H00090516   Set the Recipient User Id to the
    //                                  
    // NEW Service Provider User Id,
    //                                  
    // when creating the new OSP ALERT
    // 03/22/00 swsrchf    H00091385  When creating an OSP ALERT, on an
    //                                   
    // ALREADY EXISTS generate a random
    //                                   
    // number
    // 03/28/00 swsrchf    H000xxxxx  Remove all references to RECORDED_DOCUMENT
    // 08/23/2000	M Ramirez	101309		DMON is related to an OSP not an SP
    // -------------------------------------------------------------------
    // *********************************************
    // * CAB Logic by Siraj
    // *
    // * Import: New Case Assignment, new OSP
    // *
    // * 1. Get currency for Case Assignment, OSP, office and service   provider
    // assigned currently to the case for the given reason code.
    // *
    // * 2. Read for active case assignment, given reason.
    // *	If found, need to expire it with discontinue date, one day less than 
    // the new effective date. Check to see if this discont dt is not prior to
    // current effective date.
    // *
    // * 4. Create new case assignment
    // *
    // * 5. Read all active Case Unit Assignments, if any belong to the existing
    // Case Coordinator then discontinue them and assign them to the new OSP.
    // *
    // * 6. Do the same for monitored activities.
    // Only one OSP can be assigned to a case at any point in time.
    // ********************************************
    // 07/27/01 M.Lachowicz PR 124228	 Improve performance.
    // 08/12/02 M.Lachowicz WR 20258A
    // Reassing Interstate Case when case is reassigned.
    // 07/27/2001 M.Lachowicz Start
    local.Current.Timestamp = Now();

    // 07/27/2001 M.Lachowicz End
    // Initialize Local attribute to DB2 Max date
    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    // 07/27/2001 M.Lachowicz
    // Add CASE entity to this READ Statment
    if (ReadCaseAssignmentCase())
    {
      // 07/27/2001 M.Lachowicz Start
      // 07/27/2001 M.Lachowicz End
      if (ReadOfficeServiceProviderOfficeServiceProvider())
      {
        MoveOfficeServiceProvider(entities.OfficeServiceProvider,
          local.CurrOspForCaseOfficeServiceProvider);
        local.CurrOspForCaseOffice.SystemGeneratedId =
          entities.Office.SystemGeneratedId;
        MoveServiceProvider(entities.ServiceProvider,
          local.CurrOspForCaseServiceProvider);
      }
      else
      {
        ExitState = "OFFICE_SERVICE_PROVIDER_NF";

        return;
      }

      // ---------------------------------------------------------
      // An active case assignment exists. Expire it.
      // ---------------------------------------------------------
      if (Lt(AddDays(import.New1.EffectiveDate, -1),
        entities.CaseAssignment.EffectiveDate))
      {
        ExitState = "SP0000_NW_EFF_DATE_LE_PRV_EFF_DT";

        return;
      }

      MoveCaseAssignment(entities.CaseAssignment, local.CaseAssignment);
      local.CaseAssignment.DiscontinueDate =
        AddDays(import.New1.EffectiveDate, -1);

      // 07/27/2001 M.Lachowicz Start
      try
      {
        UpdateCaseAssignment();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "CASE_ASSIGNMENT_NU";

            return;
          case ErrorCode.PermittedValueViolation:
            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      if (!ReadOfficeServiceProvider1())
      {
        ExitState = "OFFICE_SERVICE_PROVIDER_NF";

        return;
      }

      try
      {
        CreateCaseAssignment();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "SP0000_CASE_ASSIGNMENT_AE";

            return;
          case ErrorCode.PermittedValueViolation:
            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      // 07/27/2001 M.Lachowicz End
      // *** Problem report H00082899
      // *** 02/09/00 SWSRCHF
      // *** start
      // *** Obtain each OLD Office Service Provider Alert for the CURRENT
      // *** Office Service Provider
      // 07/27/2001 M.Lachowicz
      // Change property of this READ EACH and generate cursor
      // without OPTIMIZE clause.
      foreach(var item in ReadOfficeServiceProviderAlertInfrastructure())
      {
        // *** Obtain the Infrastructure for the CURRENT
        // *** Office Service Provider Alert, if it has one
        if (ReadInfrastructure1())
        {
          // *** continue processing
        }
        else
        {
          ExitState = "INFRASTRUCTURE_NF";

          return;
        }

        // *** Obtain the NEW Office Service Provider
        // 07/27/2001 M.Lachowicz Start
        // *** save the OLD Office Service Provider Alert info
        local.Saved.Assign(entities.OldOfficeServiceProviderAlert);

        // *** DELETE the OLD Office Service Provider Alert
        DeleteOfficeServiceProviderAlert();

        // 07/27/2001 M.Lachowicz End
        // *** Problem report H00091385
        // *** 03/22/00 SWSRCHF
        local.Repeat.Flag = "Y";

        do
        {
          // *** Create the NEW Office Service Provider Alert using the data
          // *** from the OLD Office Service Provider Alert
          // *** Following fields updated:
          // ***          Last_Updated_By with USER_ID
          // ***          Last_Updated_Timestamp with CURRENT Timestamp
          try
          {
            CreateOfficeServiceProviderAlert();

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

      // *** reassign any Monitored Documents (DMON's) to the NEW
      // *** Service Provider
      // *** Obtain each existing Infrastructure and Monitored Document for
      // *** the OLD Service Provider
      // *** Problem report H000xxxxx
      // *** 03/28/00 SWSRCHF
      // *** change read to access Monitored Document via Outgoing Document
      // mjr
      // --------------------------------------------
      // 08/23/2000
      // PR# 101309 - DMON is related to an OSP not SP
      // Added PRINTER_OUTPUT_DESTINATION to READ EACH.
      // ---------------------------------------------------------
      // 07/27/2001 M.Lachowicz
      // Change property of this READ EACH and generate cursor
      // without Distinct clause.
      foreach(var item in ReadInfrastructureMonitoredDocument())
      {
        // *** UPDATE the Infrastructure User Id with the NEW
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
      }

      // *** end
      // *** 02/09/00 SWSRCHF
      // *** Problem report H00082899
      if (ReadServiceProvider())
      {
        local.Infrastructure.SituationNumber = 0;
        local.Infrastructure.ProcessStatus = "Q";
        local.Infrastructure.EventId = 5;
        local.Infrastructure.BusinessObjectCd = "CAS";
        local.Infrastructure.UserId = "ASIN";
        local.Infrastructure.ReasonCode = "CASEXFR_ASIN";
        local.Infrastructure.CaseNumber = entities.Case1.Number;
        local.Infrastructure.CaseUnitNumber = 0;
        local.Infrastructure.ReferenceDate = import.Current.Date;
        local.Infrastructure.Detail = "Case has been transfered from :";
        local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + entities
          .ServiceProvider.UserId;
        local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + " to :";
          
        local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + entities
          .NewServiceProvider.UserId;

        if (ReadInterstateRequest())
        {
          local.Infrastructure.InitiatingStateCode = "OS";
        }
        else
        {
          local.Infrastructure.InitiatingStateCode = "KS";
        }

        UseSpCabCreateInfrastructure();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }
    }
    else
    {
      // ---------------------------------------------------------
      // As of current design (10/31/97) this should not happen
      // ---------------------------------------------------------
      ExitState = "CASE_ASSIGNMENT_NF_FOR_CASE";

      return;
    }

    // 08/12/2002 M.Lachowicz Start
    local.InterstateCase.KsCaseId = import.Case1.Number;

    foreach(var item in ReadInterstateCase())
    {
      if (ReadInterstateCaseAssignment1())
      {
        MoveInterstateCaseAssignment(entities.OldInterstateCaseAssignment,
          local.InterstateCaseAssignment);

        if (AsChar(entities.OldInterstateCaseAssignment.OverrideInd) == 'N')
        {
          if (ReadOfficeServiceProvider3())
          {
            // Interstate case is already assigned to the case worker.
            continue;
          }
        }
        else if (!ReadOfficeServiceProvider2())
        {
          continue;
        }

        if (Lt(local.CaseAssignment.DiscontinueDate,
          entities.OldInterstateCaseAssignment.EffectiveDate))
        {
          DeleteInterstateCaseAssignment();

          if (!ReadInterstateCaseAssignment2())
          {
            try
            {
              CreateInterstateCaseAssignment();

              continue;
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
        }
        else
        {
        }

        try
        {
          UpdateInterstateCaseAssignment();
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

          continue;
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
    }

    // 08/12/2002 M.Lachowicz End
    // 07/27/2001 M.Lachowicz
    // Use current CASE in this statment and do not generate DISTINCT clause in 
    // cursor.
    foreach(var item in ReadCaseUnit())
    {
      // ---------------------------------------------
      // Read the latest case unit assignment.
      //    If it was assigned to a different OSP
      //       Skip the case unit and read next CU
      //    If it was assigned to the same OSP as that of the case
      //       If the case unit assignment is active
      //           expire it.
      // ---------------------------------------------
      // 07/27/2001 M.Lachowicz
      // Do not generate DISTINCT clause in cursor.
      foreach(var item1 in ReadCaseUnitFunctionAssignmt())
      {
        if (Lt(AddDays(import.New1.EffectiveDate, -1),
          entities.CaseUnitFunctionAssignmt.EffectiveDate))
        {
          ExitState = "SP0000_NEW_CUA_EFF_DT_LT_PRV_EFF";

          return;
        }

        local.New1.Assign(entities.CaseUnitFunctionAssignmt);
        local.New1.DiscontinueDate = AddDays(import.New1.EffectiveDate, -1);

        // 07/27/2001 M.Lachowicz Start
        try
        {
          UpdateCaseUnitFunctionAssignmt();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "CASE_UNIT_FUNCTION_ASSIGNMT_NU";

              return;
            case ErrorCode.PermittedValueViolation:
              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        // 07/27/2001 M.Lachowicz End
        local.New1.EffectiveDate = import.New1.EffectiveDate;
        local.New1.DiscontinueDate = import.New1.DiscontinueDate;
        local.New1.ReasonCode = entities.CaseUnitFunctionAssignmt.ReasonCode;
        local.New1.OverrideInd = entities.CaseUnitFunctionAssignmt.OverrideInd;
        local.New1.Function = entities.CaseUnitFunctionAssignmt.Function;

        // 07/27/2001 M.Lachowicz Start
        try
        {
          CreateCaseUnitFunctionAssignmt();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "SP0000_CASE_UNIT_FUNC_ASSGN_AE";

              return;
            case ErrorCode.PermittedValueViolation:
              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        // 07/27/2001 M.Lachowicz End
      }
    }

    // --- Assign monitored activities at case level (and case unit level).
    // ===============================================
    // 4/27/00 - bud adams  -  PR# 93896: Performance.
    //   Combined Monitored_Activity and Infrastructure in this
    //   Read Each, along with selection criteria for mon-act.  This
    //   will reduce the amount of data in the results table, also
    //   reducing the number of fetches
    // ===============================================
    // 07/30/2001 M.Lachowicz Start
    foreach(var item in ReadInfrastructure2())
    {
      foreach(var item1 in ReadMonitoredActivity())
      {
        if (Equal(entities.MonitoredActivity.TypeCode, "MAN"))
        {
          continue;
        }

        switch(TrimEnd(entities.Infrastructure.BusinessObjectCd))
        {
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
          case "CUF":
            break;
          case "LEA":
            if (entities.MonitoredActivity.ActivityControlNumber == 68 || entities
              .MonitoredActivity.ActivityControlNumber == 45)
            {
            }
            else
            {
              continue;
            }

            break;
          default:
            continue;
        }

        foreach(var item2 in ReadMonitoredActivityAssignment())
        {
          if (Lt(AddDays(import.New1.EffectiveDate, -1),
            entities.MonitoredActivityAssignment.EffectiveDate))
          {
            ExitState = "SP0000_NEW_MONAA_EFF_DT_LT_PREV";

            return;
          }

          local.MonitoredActivityAssignment.Assign(
            entities.MonitoredActivityAssignment);
          local.MonitoredActivityAssignment.DiscontinueDate =
            AddDays(import.New1.EffectiveDate, -1);

          // 07/27/2001 M.Lachowicz Start
          try
          {
            UpdateMonitoredActivityAssignment();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "MONITORED_ACTIVITY_ASSIGNMEN_NU";

                return;
              case ErrorCode.PermittedValueViolation:
                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          local.MonitoredActivityAssignment.EffectiveDate =
            import.New1.EffectiveDate;
          local.MonitoredActivityAssignment.DiscontinueDate =
            import.New1.DiscontinueDate;

          // 07/27/2001 M.Lachowicz Start
          try
          {
            CreateMonitoredActivityAssignment();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "SP0000_MONITORED_ACTVTY_ASGN_AE";

                return;
              case ErrorCode.PermittedValueViolation:
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

    // 07/30/2001 M.Lachowicz End
  }

  private static void MoveCaseAssignment(CaseAssignment source,
    CaseAssignment target)
  {
    target.ReasonCode = source.ReasonCode;
    target.OverrideInd = source.OverrideInd;
    target.DiscontinueDate = source.DiscontinueDate;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.Function = source.Function;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.EventType = source.EventType;
    target.EventDetailName = source.EventDetailName;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.UserId = source.UserId;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
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

  private static void MoveServiceProvider(ServiceProvider source,
    ServiceProvider target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.UserId = source.UserId;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseGenerate9DigitRandomNumber()
  {
    var useImport = new Generate9DigitRandomNumber.Import();
    var useExport = new Generate9DigitRandomNumber.Export();

    Call(Generate9DigitRandomNumber.Execute, useImport, useExport);

    local.SystemGenerated.Attribute9DigitRandomNumber =
      useExport.SystemGenerated.Attribute9DigitRandomNumber;
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    MoveInfrastructure(local.Infrastructure, useImport.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    local.Infrastructure.Assign(useExport.Infrastructure);
  }

  private void UseSpCabReassignDocument()
  {
    var useImport = new SpCabReassignDocument.Import();
    var useExport = new SpCabReassignDocument.Export();

    useImport.Infrastructure.Assign(entities.Infrastructure);
    useImport.NewServiceProvider.UserId = import.ServiceProvider.UserId;
    useImport.NewOffice.SystemGeneratedId = import.Office.SystemGeneratedId;
    MoveOfficeServiceProvider(import.OfficeServiceProvider,
      useImport.NewOfficeServiceProvider);
    useImport.OldServiceProvider.UserId =
      local.CurrOspForCaseServiceProvider.UserId;
    useImport.OldOffice.SystemGeneratedId =
      local.CurrOspForCaseOffice.SystemGeneratedId;
    MoveOfficeServiceProvider(local.CurrOspForCaseOfficeServiceProvider,
      useImport.OldOfficeServiceProvider);

    Call(SpCabReassignDocument.Execute, useImport, useExport);
  }

  private void CreateCaseAssignment()
  {
    System.Diagnostics.Debug.
      Assert(entities.NewOfficeServiceProvider.Populated);

    var reasonCode = import.New1.ReasonCode;
    var overrideInd = import.New1.OverrideInd;
    var effectiveDate = import.New1.EffectiveDate;
    var discontinueDate = import.New1.DiscontinueDate;
    var createdBy = global.UserId;
    var createdTimestamp = local.Current.Timestamp;
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
    System.Diagnostics.Debug.
      Assert(entities.NewOfficeServiceProvider.Populated);
    System.Diagnostics.Debug.Assert(entities.CaseUnit.Populated);

    var reasonCode = local.New1.ReasonCode;
    var overrideInd = local.New1.OverrideInd;
    var effectiveDate = local.New1.EffectiveDate;
    var discontinueDate = local.New1.DiscontinueDate;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var spdId = entities.NewOfficeServiceProvider.SpdGeneratedId;
    var offId = entities.NewOfficeServiceProvider.OffGeneratedId;
    var ospCode = entities.NewOfficeServiceProvider.RoleCode;
    var ospDate = entities.NewOfficeServiceProvider.EffectiveDate;
    var csuNo = entities.CaseUnit.CuNumber;
    var casNo = entities.CaseUnit.CasNo;
    var function = local.New1.Function;

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
    var effectiveDate = import.New1.EffectiveDate;
    var discontinueDate = local.Max.Date;
    var createdBy = global.UserId;
    var createdTimestamp = local.Current.Timestamp;
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

    var reasonCode = local.MonitoredActivityAssignment.ReasonCode;
    var responsibilityCode =
      local.MonitoredActivityAssignment.ResponsibilityCode;
    var effectiveDate = local.MonitoredActivityAssignment.EffectiveDate;
    var overrideInd = local.MonitoredActivityAssignment.OverrideInd;
    var discontinueDate = local.MonitoredActivityAssignment.DiscontinueDate;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var lastUpdatedBy = local.MonitoredActivityAssignment.LastUpdatedBy ?? "";
    var lastUpdatedTimestamp =
      local.MonitoredActivityAssignment.LastUpdatedTimestamp;
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
        db.SetString(command, "responsibilityCod", responsibilityCode);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetString(command, "overrideInd", overrideInd);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetInt32(command, "spdId", spdId);
        db.SetInt32(command, "offId", offId);
        db.SetString(command, "ospCode", ospCode);
        db.SetDate(command, "ospDate", ospDate);
        db.SetInt32(command, "macId", macId);
      });

    entities.NewMonitoredActivityAssignment.SystemGeneratedIdentifier = 0;
    entities.NewMonitoredActivityAssignment.ReasonCode = reasonCode;
    entities.NewMonitoredActivityAssignment.ResponsibilityCode =
      responsibilityCode;
    entities.NewMonitoredActivityAssignment.EffectiveDate = effectiveDate;
    entities.NewMonitoredActivityAssignment.OverrideInd = overrideInd;
    entities.NewMonitoredActivityAssignment.DiscontinueDate = discontinueDate;
    entities.NewMonitoredActivityAssignment.CreatedBy = createdBy;
    entities.NewMonitoredActivityAssignment.CreatedTimestamp = createdTimestamp;
    entities.NewMonitoredActivityAssignment.LastUpdatedBy = lastUpdatedBy;
    entities.NewMonitoredActivityAssignment.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.NewMonitoredActivityAssignment.SpdId = spdId;
    entities.NewMonitoredActivityAssignment.OffId = offId;
    entities.NewMonitoredActivityAssignment.OspCode = ospCode;
    entities.NewMonitoredActivityAssignment.OspDate = ospDate;
    entities.NewMonitoredActivityAssignment.MacId = macId;
    entities.NewMonitoredActivityAssignment.Populated = true;
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
    var recipientUserId = import.ServiceProvider.UserId;
    var createdBy = local.Saved.CreatedBy;
    var createdTimestamp = local.Saved.CreatedTimestamp;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = import.Current.Timestamp;
    var infId = entities.Infrastructure.SystemGeneratedIdentifier;
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

  private void DeleteInterstateCaseAssignment()
  {
    Update("DeleteInterstateCaseAssignment",
      (db, command) =>
      {
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
    entities.Case1.Populated = false;
    entities.CaseAssignment.Populated = false;

    return Read("ReadCaseAssignmentCase",
      (db, command) =>
      {
        db.SetString(command, "casNo", import.Case1.Number);
        db.SetString(command, "reasonCode", import.New1.ReasonCode);
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseAssignment.ReasonCode = db.GetString(reader, 0);
        entities.CaseAssignment.OverrideInd = db.GetString(reader, 1);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 2);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 3);
        entities.CaseAssignment.CreatedBy = db.GetString(reader, 4);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.CaseAssignment.LastUpdatedBy = db.GetNullableString(reader, 6);
        entities.CaseAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 7);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 8);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 9);
        entities.CaseAssignment.OspCode = db.GetString(reader, 10);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 11);
        entities.CaseAssignment.CasNo = db.GetString(reader, 12);
        entities.Case1.Number = db.GetString(reader, 12);
        entities.Case1.Populated = true;
        entities.CaseAssignment.Populated = true;
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
          SetDate(command, "startDate", import.Current.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.StartDate = db.GetDate(reader, 1);
        entities.CaseUnit.ClosureDate = db.GetNullableDate(reader, 2);
        entities.CaseUnit.CasNo = db.GetString(reader, 3);
        entities.CaseUnit.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseUnitFunctionAssignmt()
  {
    System.Diagnostics.Debug.Assert(entities.CaseUnit.Populated);
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.CaseUnitFunctionAssignmt.Populated = false;

    return ReadEach("ReadCaseUnitFunctionAssignmt",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.CaseUnit.CasNo);
        db.SetInt32(command, "csuNo", entities.CaseUnit.CuNumber);
        db.SetDate(
          command, "effectiveDate", import.Current.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
        db.SetDate(
          command, "ospDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.
          SetString(command, "ospCode", entities.OfficeServiceProvider.RoleCode);
          
        db.SetInt32(
          command, "offId", entities.OfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdId", entities.OfficeServiceProvider.SpdGeneratedId);
      },
      (db, reader) =>
      {
        entities.CaseUnitFunctionAssignmt.ReasonCode = db.GetString(reader, 0);
        entities.CaseUnitFunctionAssignmt.OverrideInd = db.GetString(reader, 1);
        entities.CaseUnitFunctionAssignmt.EffectiveDate = db.GetDate(reader, 2);
        entities.CaseUnitFunctionAssignmt.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.CaseUnitFunctionAssignmt.CreatedBy = db.GetString(reader, 4);
        entities.CaseUnitFunctionAssignmt.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.CaseUnitFunctionAssignmt.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.CaseUnitFunctionAssignmt.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 7);
        entities.CaseUnitFunctionAssignmt.SpdId = db.GetInt32(reader, 8);
        entities.CaseUnitFunctionAssignmt.OffId = db.GetInt32(reader, 9);
        entities.CaseUnitFunctionAssignmt.OspCode = db.GetString(reader, 10);
        entities.CaseUnitFunctionAssignmt.OspDate = db.GetDate(reader, 11);
        entities.CaseUnitFunctionAssignmt.CsuNo = db.GetInt32(reader, 12);
        entities.CaseUnitFunctionAssignmt.CasNo = db.GetString(reader, 13);
        entities.CaseUnitFunctionAssignmt.Function = db.GetString(reader, 14);
        entities.CaseUnitFunctionAssignmt.Populated = true;

        return true;
      });
  }

  private bool ReadInfrastructure1()
  {
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure1",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          entities.ReadOnly.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.EventId = db.GetInt32(reader, 1);
        entities.Infrastructure.EventType = db.GetString(reader, 2);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 3);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 4);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 5);
        entities.Infrastructure.UserId = db.GetString(reader, 6);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 7);
        entities.Infrastructure.LastUpdatedBy = db.GetNullableString(reader, 8);
        entities.Infrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 9);
        entities.Infrastructure.Populated = true;
      });
  }

  private IEnumerable<bool> ReadInfrastructure2()
  {
    entities.Infrastructure.Populated = false;

    return ReadEach("ReadInfrastructure2",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.EventId = db.GetInt32(reader, 1);
        entities.Infrastructure.EventType = db.GetString(reader, 2);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 3);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 4);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 5);
        entities.Infrastructure.UserId = db.GetString(reader, 6);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 7);
        entities.Infrastructure.LastUpdatedBy = db.GetNullableString(reader, 8);
        entities.Infrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 9);
        entities.Infrastructure.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadInfrastructureMonitoredDocument()
  {
    entities.MonitoredDocument.Populated = false;
    entities.Infrastructure.Populated = false;

    return ReadEach("ReadInfrastructureMonitoredDocument",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", import.Case1.Number);
        db.SetString(command, "userId", entities.ServiceProvider.UserId);
        db.SetNullableDate(
          command, "actRespDt", local.NullDate.Date.GetValueOrDefault());
        db.SetNullableInt32(
          command, "offGenerated", entities.Office.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.EventId = db.GetInt32(reader, 1);
        entities.Infrastructure.EventType = db.GetString(reader, 2);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 3);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 4);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 5);
        entities.Infrastructure.UserId = db.GetString(reader, 6);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 7);
        entities.Infrastructure.LastUpdatedBy = db.GetNullableString(reader, 8);
        entities.Infrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 9);
        entities.MonitoredDocument.RequiredResponseDate =
          db.GetDate(reader, 10);
        entities.MonitoredDocument.ActualResponseDate =
          db.GetNullableDate(reader, 11);
        entities.MonitoredDocument.ClosureDate = db.GetNullableDate(reader, 12);
        entities.MonitoredDocument.ClosureReasonCode =
          db.GetNullableString(reader, 13);
        entities.MonitoredDocument.CreatedBy = db.GetString(reader, 14);
        entities.MonitoredDocument.CreatedTimestamp =
          db.GetDateTime(reader, 15);
        entities.MonitoredDocument.LastUpdatedBy =
          db.GetNullableString(reader, 16);
        entities.MonitoredDocument.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 17);
        entities.MonitoredDocument.InfId = db.GetInt32(reader, 18);
        entities.MonitoredDocument.Populated = true;
        entities.Infrastructure.Populated = true;

        return true;
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

  private bool ReadInterstateCaseAssignment1()
  {
    entities.OldInterstateCaseAssignment.Populated = false;

    return Read("ReadInterstateCaseAssignment1",
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

  private bool ReadInterstateCaseAssignment2()
  {
    entities.OldInterstateCaseAssignment.Populated = false;

    return Read("ReadInterstateCaseAssignment2",
      (db, command) =>
      {
        db.
          SetInt64(command, "icsNo", entities.InterstateCase.TransSerialNumber);
          
        db.SetDate(
          command, "icsDate",
          entities.InterstateCase.TransactionDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate",
          import.New1.EffectiveDate.GetValueOrDefault());
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

  private bool ReadInterstateRequest()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 1);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 2);
        entities.InterstateRequest.Populated = true;
      });
  }

  private IEnumerable<bool> ReadMonitoredActivity()
  {
    entities.MonitoredActivity.Populated = false;

    return ReadEach("ReadMonitoredActivity",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "infSysGenId",
          entities.Infrastructure.SystemGeneratedIdentifier);
        db.SetNullableDate(
          command, "closureDate", import.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.MonitoredActivity.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.MonitoredActivity.Name = db.GetString(reader, 1);
        entities.MonitoredActivity.ActivityControlNumber =
          db.GetInt32(reader, 2);
        entities.MonitoredActivity.TypeCode = db.GetNullableString(reader, 3);
        entities.MonitoredActivity.FedNonComplianceDate =
          db.GetNullableDate(reader, 4);
        entities.MonitoredActivity.FedNearNonComplDate =
          db.GetNullableDate(reader, 5);
        entities.MonitoredActivity.OtherNonComplianceDate =
          db.GetNullableDate(reader, 6);
        entities.MonitoredActivity.OtherNearNonComplDate =
          db.GetNullableDate(reader, 7);
        entities.MonitoredActivity.StartDate = db.GetDate(reader, 8);
        entities.MonitoredActivity.ClosureDate = db.GetNullableDate(reader, 9);
        entities.MonitoredActivity.ClosureReasonCode =
          db.GetNullableString(reader, 10);
        entities.MonitoredActivity.CaseUnitClosedInd = db.GetString(reader, 11);
        entities.MonitoredActivity.CreatedBy = db.GetString(reader, 12);
        entities.MonitoredActivity.CreatedTimestamp =
          db.GetDateTime(reader, 13);
        entities.MonitoredActivity.LastUpdatedBy =
          db.GetNullableString(reader, 14);
        entities.MonitoredActivity.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 15);
        entities.MonitoredActivity.InfSysGenId =
          db.GetNullableInt32(reader, 16);
        entities.MonitoredActivity.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadMonitoredActivityAssignment()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.MonitoredActivityAssignment.Populated = false;

    return ReadEach("ReadMonitoredActivityAssignment",
      (db, command) =>
      {
        db.SetInt32(
          command, "macId",
          entities.MonitoredActivity.SystemGeneratedIdentifier);
        db.SetDate(
          command, "ospDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.
          SetString(command, "ospCode", entities.OfficeServiceProvider.RoleCode);
          
        db.SetInt32(
          command, "offId", entities.OfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdId", entities.OfficeServiceProvider.SpdGeneratedId);
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.MonitoredActivityAssignment.ReasonCode =
          db.GetString(reader, 0);
        entities.MonitoredActivityAssignment.ResponsibilityCode =
          db.GetString(reader, 1);
        entities.MonitoredActivityAssignment.EffectiveDate =
          db.GetDate(reader, 2);
        entities.MonitoredActivityAssignment.OverrideInd =
          db.GetString(reader, 3);
        entities.MonitoredActivityAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.MonitoredActivityAssignment.CreatedBy =
          db.GetString(reader, 5);
        entities.MonitoredActivityAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.MonitoredActivityAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.MonitoredActivityAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 8);
        entities.MonitoredActivityAssignment.SpdId = db.GetInt32(reader, 9);
        entities.MonitoredActivityAssignment.OffId = db.GetInt32(reader, 10);
        entities.MonitoredActivityAssignment.OspCode = db.GetString(reader, 11);
        entities.MonitoredActivityAssignment.OspDate = db.GetDate(reader, 12);
        entities.MonitoredActivityAssignment.MacId = db.GetInt32(reader, 13);
        entities.MonitoredActivityAssignment.Populated = true;

        return true;
      });
  }

  private bool ReadOfficeServiceProvider1()
  {
    entities.NewOfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider1",
      (db, command) =>
      {
        db.SetInt32(command, "offGeneratedId", import.Office.SystemGeneratedId);
        db.SetInt32(
          command, "spdGeneratedId", import.ServiceProvider.SystemGeneratedId);
        db.SetDate(
          command, "effectiveDate",
          import.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.
          SetString(command, "roleCode", import.OfficeServiceProvider.RoleCode);
          
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

  private bool ReadOfficeServiceProvider2()
  {
    System.Diagnostics.Debug.Assert(
      entities.OldInterstateCaseAssignment.Populated);
    entities.OldInt.Populated = false;

    return Read("ReadOfficeServiceProvider2",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          entities.OldInterstateCaseAssignment.OspDate.GetValueOrDefault());
        db.SetString(
          command, "roleCode", entities.OldInterstateCaseAssignment.OspCode);
        db.SetInt32(
          command, "offGeneratedId1",
          entities.OldInterstateCaseAssignment.OffId);
        db.SetInt32(
          command, "spdGeneratedId1",
          entities.OldInterstateCaseAssignment.SpdId);
        db.SetInt32(
          command, "spdGeneratedId2",
          entities.ServiceProvider.SystemGeneratedId);
        db.SetInt32(
          command, "offGeneratedId2", entities.Office.SystemGeneratedId);
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

  private bool ReadOfficeServiceProvider3()
  {
    System.Diagnostics.Debug.Assert(
      entities.OldInterstateCaseAssignment.Populated);
    entities.OldInt.Populated = false;

    return Read("ReadOfficeServiceProvider3",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          entities.OldInterstateCaseAssignment.OspDate.GetValueOrDefault());
        db.SetString(
          command, "roleCode", entities.OldInterstateCaseAssignment.OspCode);
        db.SetInt32(
          command, "offGeneratedId1",
          entities.OldInterstateCaseAssignment.OffId);
        db.SetInt32(
          command, "spdGeneratedId1",
          entities.OldInterstateCaseAssignment.SpdId);
        db.SetInt32(
          command, "spdGeneratedId2", import.ServiceProvider.SystemGeneratedId);
          
        db.
          SetInt32(command, "offGeneratedId2", import.Office.SystemGeneratedId);
          
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

  private IEnumerable<bool> ReadOfficeServiceProviderAlertInfrastructure()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.ReadOnly.Populated = false;
    entities.OldOfficeServiceProviderAlert.Populated = false;

    return ReadEach("ReadOfficeServiceProviderAlertInfrastructure",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "ospDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.SetNullableString(
          command, "ospCode", entities.OfficeServiceProvider.RoleCode);
        db.SetNullableInt32(
          command, "offId", entities.OfficeServiceProvider.OffGeneratedId);
        db.SetNullableInt32(
          command, "spdId", entities.OfficeServiceProvider.SpdGeneratedId);
        db.SetNullableString(command, "caseNumber", import.Case1.Number);
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
        entities.ReadOnly.SystemGeneratedIdentifier = db.GetInt32(reader, 14);
        entities.OldOfficeServiceProviderAlert.SpdId =
          db.GetNullableInt32(reader, 15);
        entities.OldOfficeServiceProviderAlert.OffId =
          db.GetNullableInt32(reader, 16);
        entities.OldOfficeServiceProviderAlert.OspCode =
          db.GetNullableString(reader, 17);
        entities.OldOfficeServiceProviderAlert.OspDate =
          db.GetNullableDate(reader, 18);
        entities.ReadOnly.CaseNumber = db.GetNullableString(reader, 19);
        entities.ReadOnly.Populated = true;
        entities.OldOfficeServiceProviderAlert.Populated = true;

        return true;
      });
  }

  private bool ReadOfficeServiceProviderOfficeServiceProvider()
  {
    System.Diagnostics.Debug.Assert(entities.CaseAssignment.Populated);
    entities.ServiceProvider.Populated = false;
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;

    return Read("ReadOfficeServiceProviderOfficeServiceProvider",
      (db, command) =>
      {
        db.SetInt32(command, "spdId", entities.CaseAssignment.SpdId);
        db.SetInt32(command, "offId", entities.CaseAssignment.OffId);
        db.SetString(command, "ospCode", entities.CaseAssignment.OspCode);
        db.SetDate(
          command, "ospDate",
          entities.CaseAssignment.OspDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 4);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 5);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 6);
        entities.ServiceProvider.UserId = db.GetString(reader, 7);
        entities.ServiceProvider.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;
      });
  }

  private bool ReadServiceProvider()
  {
    entities.NewServiceProvider.Populated = false;

    return Read("ReadServiceProvider",
      (db, command) =>
      {
        db.SetInt32(
          command, "servicePrvderId", import.ServiceProvider.SystemGeneratedId);
          
      },
      (db, reader) =>
      {
        entities.NewServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.NewServiceProvider.UserId = db.GetString(reader, 1);
        entities.NewServiceProvider.Populated = true;
      });
  }

  private void UpdateCaseAssignment()
  {
    System.Diagnostics.Debug.Assert(entities.CaseAssignment.Populated);

    var reasonCode = local.CaseAssignment.ReasonCode;
    var overrideInd = local.CaseAssignment.OverrideInd;
    var discontinueDate = local.CaseAssignment.DiscontinueDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = local.Current.Timestamp;

    entities.CaseAssignment.Populated = false;
    Update("UpdateCaseAssignment",
      (db, command) =>
      {
        db.SetString(command, "reasonCode", reasonCode);
        db.SetString(command, "overrideInd", overrideInd);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetDateTime(
          command, "createdTimestamp",
          entities.CaseAssignment.CreatedTimestamp.GetValueOrDefault());
        db.SetInt32(command, "spdId", entities.CaseAssignment.SpdId);
        db.SetInt32(command, "offId", entities.CaseAssignment.OffId);
        db.SetString(command, "ospCode", entities.CaseAssignment.OspCode);
        db.SetDate(
          command, "ospDate",
          entities.CaseAssignment.OspDate.GetValueOrDefault());
        db.SetString(command, "casNo", entities.CaseAssignment.CasNo);
      });

    entities.CaseAssignment.ReasonCode = reasonCode;
    entities.CaseAssignment.OverrideInd = overrideInd;
    entities.CaseAssignment.DiscontinueDate = discontinueDate;
    entities.CaseAssignment.LastUpdatedBy = lastUpdatedBy;
    entities.CaseAssignment.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CaseAssignment.Populated = true;
  }

  private void UpdateCaseUnitFunctionAssignmt()
  {
    System.Diagnostics.Debug.
      Assert(entities.CaseUnitFunctionAssignmt.Populated);

    var discontinueDate = local.New1.DiscontinueDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = local.Current.Timestamp;

    entities.CaseUnitFunctionAssignmt.Populated = false;
    Update("UpdateCaseUnitFunctionAssignmt",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetDateTime(
          command, "createdTimestamp",
          entities.CaseUnitFunctionAssignmt.CreatedTimestamp.
            GetValueOrDefault());
        db.SetInt32(command, "spdId", entities.CaseUnitFunctionAssignmt.SpdId);
        db.SetInt32(command, "offId", entities.CaseUnitFunctionAssignmt.OffId);
        db.SetString(
          command, "ospCode", entities.CaseUnitFunctionAssignmt.OspCode);
        db.SetDate(
          command, "ospDate",
          entities.CaseUnitFunctionAssignmt.OspDate.GetValueOrDefault());
        db.SetInt32(command, "csuNo", entities.CaseUnitFunctionAssignmt.CsuNo);
        db.SetString(command, "casNo", entities.CaseUnitFunctionAssignmt.CasNo);
      });

    entities.CaseUnitFunctionAssignmt.DiscontinueDate = discontinueDate;
    entities.CaseUnitFunctionAssignmt.LastUpdatedBy = lastUpdatedBy;
    entities.CaseUnitFunctionAssignmt.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.CaseUnitFunctionAssignmt.Populated = true;
  }

  private void UpdateInterstateCaseAssignment()
  {
    System.Diagnostics.Debug.Assert(
      entities.OldInterstateCaseAssignment.Populated);

    var discontinueDate = local.CaseAssignment.DiscontinueDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = local.Current.Timestamp;

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
      entities.MonitoredActivityAssignment.Populated);

    var reasonCode = local.MonitoredActivityAssignment.ReasonCode;
    var overrideInd = local.MonitoredActivityAssignment.OverrideInd;
    var discontinueDate = local.MonitoredActivityAssignment.DiscontinueDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = local.Current.Timestamp;

    entities.MonitoredActivityAssignment.Populated = false;
    Update("UpdateMonitoredActivityAssignment",
      (db, command) =>
      {
        db.SetString(command, "reasonCode", reasonCode);
        db.SetString(command, "overrideInd", overrideInd);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetDateTime(
          command, "createdTimestamp",
          entities.MonitoredActivityAssignment.CreatedTimestamp.
            GetValueOrDefault());
        db.
          SetInt32(command, "spdId", entities.MonitoredActivityAssignment.SpdId);
          
        db.
          SetInt32(command, "offId", entities.MonitoredActivityAssignment.OffId);
          
        db.SetString(
          command, "ospCode", entities.MonitoredActivityAssignment.OspCode);
        db.SetDate(
          command, "ospDate",
          entities.MonitoredActivityAssignment.OspDate.GetValueOrDefault());
        db.
          SetInt32(command, "macId", entities.MonitoredActivityAssignment.MacId);
          
      });

    entities.MonitoredActivityAssignment.ReasonCode = reasonCode;
    entities.MonitoredActivityAssignment.OverrideInd = overrideInd;
    entities.MonitoredActivityAssignment.DiscontinueDate = discontinueDate;
    entities.MonitoredActivityAssignment.LastUpdatedBy = lastUpdatedBy;
    entities.MonitoredActivityAssignment.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.MonitoredActivityAssignment.Populated = true;
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
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
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
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
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public CaseAssignment New1
    {
      get => new1 ??= new();
      set => new1 = value;
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

    private ServiceProvider serviceProvider;
    private Office office;
    private OfficeServiceProvider officeServiceProvider;
    private Case1 case1;
    private CaseAssignment new1;
    private DateWorkArea current;
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
    /// A value of InterstateCaseAssignment.
    /// </summary>
    [JsonPropertyName("interstateCaseAssignment")]
    public InterstateCaseAssignment InterstateCaseAssignment
    {
      get => interstateCaseAssignment ??= new();
      set => interstateCaseAssignment = value;
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
    /// A value of Saved.
    /// </summary>
    [JsonPropertyName("saved")]
    public OfficeServiceProviderAlert Saved
    {
      get => saved ??= new();
      set => saved = value;
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
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
    }

    /// <summary>
    /// A value of ZdelLocalCurrent.
    /// </summary>
    [JsonPropertyName("zdelLocalCurrent")]
    public DateWorkArea ZdelLocalCurrent
    {
      get => zdelLocalCurrent ??= new();
      set => zdelLocalCurrent = value;
    }

    /// <summary>
    /// A value of CurrOspForCaseServiceProvider.
    /// </summary>
    [JsonPropertyName("currOspForCaseServiceProvider")]
    public ServiceProvider CurrOspForCaseServiceProvider
    {
      get => currOspForCaseServiceProvider ??= new();
      set => currOspForCaseServiceProvider = value;
    }

    /// <summary>
    /// A value of CurrOspForCaseOffice.
    /// </summary>
    [JsonPropertyName("currOspForCaseOffice")]
    public Office CurrOspForCaseOffice
    {
      get => currOspForCaseOffice ??= new();
      set => currOspForCaseOffice = value;
    }

    /// <summary>
    /// A value of CurrOspForCaseOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("currOspForCaseOfficeServiceProvider")]
    public OfficeServiceProvider CurrOspForCaseOfficeServiceProvider
    {
      get => currOspForCaseOfficeServiceProvider ??= new();
      set => currOspForCaseOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of MonitoredActivityAssignment.
    /// </summary>
    [JsonPropertyName("monitoredActivityAssignment")]
    public MonitoredActivityAssignment MonitoredActivityAssignment
    {
      get => monitoredActivityAssignment ??= new();
      set => monitoredActivityAssignment = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public CaseUnitFunctionAssignmt New1
    {
      get => new1 ??= new();
      set => new1 = value;
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
    /// A value of NullDate.
    /// </summary>
    [JsonPropertyName("nullDate")]
    public DateWorkArea NullDate
    {
      get => nullDate ??= new();
      set => nullDate = value;
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

    private InterstateCase interstateCase;
    private InterstateCaseAssignment interstateCaseAssignment;
    private DateWorkArea current;
    private OfficeServiceProviderAlert saved;
    private Infrastructure infrastructure;
    private CaseAssignment caseAssignment;
    private DateWorkArea zdelLocalCurrent;
    private ServiceProvider currOspForCaseServiceProvider;
    private Office currOspForCaseOffice;
    private OfficeServiceProvider currOspForCaseOfficeServiceProvider;
    private MonitoredActivityAssignment monitoredActivityAssignment;
    private CaseUnitFunctionAssignmt new1;
    private DateWorkArea max;
    private DateWorkArea nullDate;
    private DateWorkArea dateWorkArea;
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
    /// A value of OldInt.
    /// </summary>
    [JsonPropertyName("oldInt")]
    public OfficeServiceProvider OldInt
    {
      get => oldInt ??= new();
      set => oldInt = value;
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
    /// A value of NewCaseUnitFunctionAssignmt.
    /// </summary>
    [JsonPropertyName("newCaseUnitFunctionAssignmt")]
    public CaseUnitFunctionAssignmt NewCaseUnitFunctionAssignmt
    {
      get => newCaseUnitFunctionAssignmt ??= new();
      set => newCaseUnitFunctionAssignmt = value;
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
    /// A value of NewCaseAssignment.
    /// </summary>
    [JsonPropertyName("newCaseAssignment")]
    public CaseAssignment NewCaseAssignment
    {
      get => newCaseAssignment ??= new();
      set => newCaseAssignment = value;
    }

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
    /// A value of OutgoingDocument.
    /// </summary>
    [JsonPropertyName("outgoingDocument")]
    public OutgoingDocument OutgoingDocument
    {
      get => outgoingDocument ??= new();
      set => outgoingDocument = value;
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
    /// A value of NewOffice.
    /// </summary>
    [JsonPropertyName("newOffice")]
    public Office NewOffice
    {
      get => newOffice ??= new();
      set => newOffice = value;
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
    /// A value of MonitoredDocument.
    /// </summary>
    [JsonPropertyName("monitoredDocument")]
    public MonitoredDocument MonitoredDocument
    {
      get => monitoredDocument ??= new();
      set => monitoredDocument = value;
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
    /// A value of OldOfficeServiceProviderAlert.
    /// </summary>
    [JsonPropertyName("oldOfficeServiceProviderAlert")]
    public OfficeServiceProviderAlert OldOfficeServiceProviderAlert
    {
      get => oldOfficeServiceProviderAlert ??= new();
      set => oldOfficeServiceProviderAlert = value;
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
    /// A value of NewServiceProvider.
    /// </summary>
    [JsonPropertyName("newServiceProvider")]
    public ServiceProvider NewServiceProvider
    {
      get => newServiceProvider ??= new();
      set => newServiceProvider = value;
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
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
    }

    /// <summary>
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
    }

    /// <summary>
    /// A value of CaseUnitFunctionAssignmt.
    /// </summary>
    [JsonPropertyName("caseUnitFunctionAssignmt")]
    public CaseUnitFunctionAssignmt CaseUnitFunctionAssignmt
    {
      get => caseUnitFunctionAssignmt ??= new();
      set => caseUnitFunctionAssignmt = value;
    }

    /// <summary>
    /// A value of MonitoredActivityAssignment.
    /// </summary>
    [JsonPropertyName("monitoredActivityAssignment")]
    public MonitoredActivityAssignment MonitoredActivityAssignment
    {
      get => monitoredActivityAssignment ??= new();
      set => monitoredActivityAssignment = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
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

    private ServiceProvider intServiceProvider;
    private Office intOffice;
    private OfficeServiceProvider oldInt;
    private InterstateCaseAssignment newInterstateCaseAssignment;
    private InterstateCaseAssignment oldInterstateCaseAssignment;
    private InterstateCase interstateCase;
    private CaseUnitFunctionAssignmt newCaseUnitFunctionAssignmt;
    private MonitoredActivityAssignment newMonitoredActivityAssignment;
    private CaseAssignment newCaseAssignment;
    private PrinterOutputDestination printerOutputDestination;
    private OutgoingDocument outgoingDocument;
    private Infrastructure readOnly;
    private Office newOffice;
    private OfficeServiceProvider newOfficeServiceProvider;
    private MonitoredDocument monitoredDocument;
    private OfficeServiceProviderAlert newOfficeServiceProviderAlert;
    private OfficeServiceProviderAlert oldOfficeServiceProviderAlert;
    private InterstateRequest interstateRequest;
    private ServiceProvider newServiceProvider;
    private Case1 case1;
    private Infrastructure infrastructure;
    private MonitoredActivity monitoredActivity;
    private CaseUnit caseUnit;
    private CaseAssignment caseAssignment;
    private CaseUnitFunctionAssignmt caseUnitFunctionAssignmt;
    private MonitoredActivityAssignment monitoredActivityAssignment;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private Office office;
  }
#endregion
}
