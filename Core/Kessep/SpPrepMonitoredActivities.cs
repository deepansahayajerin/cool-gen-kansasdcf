// Program: SP_PREP_MONITORED_ACTIVITIES, ID: 372068769, model: 746.
// Short name: SWE02313
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_PREP_MONITORED_ACTIVITIES.
/// </para>
/// <para>
/// Determine monitored activities to be started and ended.
/// </para>
/// </summary>
[Serializable]
public partial class SpPrepMonitoredActivities: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_PREP_MONITORED_ACTIVITIES program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpPrepMonitoredActivities(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpPrepMonitoredActivities.
  /// </summary>
  public SpPrepMonitoredActivities(IContext context, Import import,
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
    // ------------------------------------------------------------------------------------------------------
    // Date	  Developer	Request #	Description
    // ------------------------------------------------------------------------------------------------------
    // 02/15/00  SWSRPRM	PR 80211 	Added document (DOC) logic
    // 08/07/00  SWSRPRM	PR 88161 	Some errors appearing on Run 206 error report
    // do not
    // 					have messages appearing.
    // 08/15/00  SWSRPRM	PR 100583 	Certain monitored activities need to have a 
    // start and
    // 					end date for their creation.
    // 11/30/00  SWSRPRM	PR 108502 	Erroneous error 'Monitored activity not 
    // found' being
    // 					generated when looking @ activities for a court order
    // 					rather than by legal action identifier.
    // 12/08/00  SWSRPRM	PR 108456 	Duplicate information on error report; not 
    // all legal
    // 					actions are being noticed as being assigned.
    // 02/02/01  SWSRPRM			For ending activities, added checks in order to exit
    // 					READ EACHes of MONITORED ACTIVITY and skip unnecessary
    // 					later interrogations for multiple counts
    // 03/23/01  SWSRPRM	PR 115540 	Start dates for LRF (legal referral) 
    // business object
    // 					code
    // 04/12/01  SWSRPRM	PR 111742 	Do not write to the error report if the only
    // error is
    // 					the inability to find a montored activity to close
    // 05/10/04  swdphlm	PR 162116 	Added two SET statement to move current date
    // to monitored
    // 					activity closure date.  Without these only part of the
    // 					case's monitored activities were getting the closure
    // 					date set.  There were several other places within the
    // 					code where the combination of the SETting the closure
    // 					date and SETting the Reason Code to SYS is used.
    // 					These changes were made to the sr.cse.production.fix
    // 					version of the program and will be migrated to
    // 					acceptance and then to production from that source.
    // 					In addition, they will be echoed back into the development
    // 					version, where other changes for other PRs had already
    // 					been made, but were not ready for migration to acceptance
    // 					when these changes (setting the closure date) were needed.
    // 04/28/05  GVandy			Performance changes.
    // 12/14/06  Raj S		PR 251062 	Modified to fix the problem of closing wrong 
    // monitored
    // 					Activity.
    // 07/12/07  GVandy	PR 313071	Correct performance problem when closing LEA 
    // events.
    // 04/15/10  GVandy	CQ 966		Significant restructuring of READ EACHs to aid 
    // performance.
    // 					Also, return new group view containing all monitored
    // 					activities to be started/stopped instead of doing the
    // 					creates/updates in this cab.
    // 			CQ302		No monitored activity processing is required for closed
    // 					cases.
    // 			CQ299		For LEA business objects, the second criteria for finding
    // 					monitored activities to close should be by Case,
    // 					CSE Person, and Legal Action ID.
    // 10/28/10  GVandy	CQ109		Don't do any monitored activity processing on 
    // closed cases.
    // 04/12/13  GVandy	CQ32829		Currently if more than one monitored activity 
    // is found to
    // 					close then the infrastructure is set to Error status.
    // 					Now we will close all the monitored activities found.
    // ------------------------------------------------------------------------------------------------------
    local.SaveLastOfErrorGroup.Count = export.Error.Count;
    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    // 10/28/2010  GVandy  CQ109  No monitored activity processing is required 
    // for closed cases.
    if (!IsEmpty(import.Infrastructure.CaseNumber))
    {
      if (ReadCase())
      {
        return;
      }
      else
      {
        // Continue
      }
    }

    // ------------------------------------------------------------------------
    // ERROR Handling: A group view (50) is used to collect
    // errors that are not life threatening. The exit st msg is
    // extracted at this time. If the error is such that it makes
    // no sense to proceed, ESCAPE immediately.
    // ------------------------------------------------------------------------
    // ------------------------------------------------------------------------
    // 4. Process Monitored Activities
    // ------------------------------------------------------------------------
    // -----------------------------------------------------------
    // Close Monitored Activities first in case the same Event
    // and Event Detail combo is used to Start and End Mon Act.
    // This would prevent us from closing a Mon Act that was
    // just opened. This is done by sorting on Action Code
    // which can be S(start) or E(end).
    // -----------------------------------------------------------
    foreach(var item in ReadActivityStartStopActivityDetailActivity())
    {
      if (AsChar(entities.ActivityStartStop.ActionCode) == 'S')
      {
        // ------------------------------------------------------------------------
        //   4.1 Open Monitored Activities
        // ------------------------------------------------------------------------
        local.MonitoredActivity.ActivityControlNumber =
          entities.Activity.ControlNumber;
        local.MonitoredActivity.CreatedBy = global.UserId;
        local.MonitoredActivity.CreatedTimestamp = Now();
        local.MonitoredActivity.LastUpdatedBy = "";
        local.MonitoredActivity.LastUpdatedTimestamp =
          local.Linitialized.Timestamp;
        local.MonitoredActivity.FedNearNonComplDate = local.Linitialized.Date;
        local.MonitoredActivity.FedNonComplianceDate = local.Linitialized.Date;
        local.MonitoredActivity.OtherNearNonComplDate = local.Linitialized.Date;
        local.MonitoredActivity.OtherNonComplianceDate =
          local.Linitialized.Date;

        if (!Equal(entities.ActivityDetail.FedNearNonComplDays, 0))
        {
          local.MonitoredActivity.FedNearNonComplDate =
            AddDays(import.Current.Date,
            entities.ActivityDetail.FedNearNonComplDays.GetValueOrDefault());
        }

        if (!Equal(entities.ActivityDetail.FedNonComplianceDays, 0))
        {
          local.MonitoredActivity.FedNonComplianceDate =
            AddDays(import.Current.Date,
            entities.ActivityDetail.FedNonComplianceDays.GetValueOrDefault());
        }

        if (!Equal(entities.ActivityDetail.OtherNearNonComplDays, 0))
        {
          local.MonitoredActivity.OtherNearNonComplDate =
            AddDays(import.Current.Date,
            entities.ActivityDetail.OtherNearNonComplDays.GetValueOrDefault());
        }

        if (!Equal(entities.ActivityDetail.OtherNonComplianceDays, 0))
        {
          local.MonitoredActivity.OtherNonComplianceDate =
            AddDays(import.Current.Date,
            entities.ActivityDetail.OtherNonComplianceDays.GetValueOrDefault());
            
        }

        if (Equal(import.Infrastructure.BusinessObjectCd, "LEA") || Equal
          (import.Infrastructure.BusinessObjectCd, "LRF"))
        {
          // -- PR # 115540
          local.MonitoredActivity.StartDate =
            import.Infrastructure.ReferenceDate;
          UseSpEstabLegalMonaComplDates();
        }
        else
        {
          local.MonitoredActivity.StartDate = import.Current.Date;
        }

        local.MonitoredActivity.Name = entities.Activity.Name;
        local.MonitoredActivity.TypeCode = "AUT";
        local.MonitoredActivity.ClosureDate = local.Max.Date;
        local.MonitoredActivity.ClosureReasonCode = "";

        export.ImportExportStartMona.Index = export.ImportExportStartMona.Count;
        export.ImportExportStartMona.CheckSize();

        export.ImportExportStartMona.Update.StartMona.Assign(
          local.MonitoredActivity);
        export.ImportExportStartMona.Item.StartMonaAssgnmnt.Count = 0;
        export.ImportExportStartMona.Item.StartMonaAssgnmnt.Index = -1;

        foreach(var item1 in ReadActivityDistributionRule())
        {
          // ------------------------------------------------------------------------
          // Cross Ref ADR  Role and Infr info
          // This section of code will test 'Roles'. If a certain
          // distribution rule fails, we skip to the next Act Dist
          // Rule.
          // ------------------------------------------------------------------------
          if (!IsEmpty(entities.ActivityDistributionRule.CaseRoleCode) || !
            IsEmpty(entities.ActivityDistributionRule.CsePersonAcctCode) || !
            IsEmpty(entities.ActivityDistributionRule.CsenetRoleCode) || !
            IsEmpty(entities.ActivityDistributionRule.LaPersonCode))
          {
            if (IsEmpty(import.Infrastructure.CsePersonNumber))
            {
              continue;
            }

            // ------------------------------------------------------------------------
            // We have determined that Person is populated, now see
            // which ADR.Role is selected.
            // ADR Role             Need
            // ------------------------------------------------------
            // Case Role            Case
            // CSE Person Acct      Admin Appeal
            //                      Oblig Admin Action
            //                      Obligation
            // LA Person            Legal Action
            // CSENet Role Code
            // ------------------------------------------------------------------------
            if (!IsEmpty(entities.ActivityDistributionRule.CaseRoleCode) && !
              IsEmpty(import.Infrastructure.CaseNumber))
            {
              if (ReadCaseRole())
              {
                // -- continue processing
              }
              else
              {
                continue;
              }
            }
            else if (!IsEmpty(entities.ActivityDistributionRule.
              CsePersonAcctCode))
            {
              if (Equal(import.Infrastructure.BusinessObjectCd, "ADA"))
              {
                local.AdministrativeAppeal.Identifier =
                  (int)import.Infrastructure.DenormNumeric12.
                    GetValueOrDefault();

                if (ReadAdministrativeAppeal())
                {
                  // -- continue processing
                }
                else
                {
                  continue;
                }
              }
              else if (Equal(import.Infrastructure.BusinessObjectCd, "OAA"))
              {
                if (ReadObligationAdministrativeAction())
                {
                  // -- continue processing
                }
                else
                {
                  continue;
                }
              }
              else if (Equal(import.Infrastructure.BusinessObjectCd, "OBL"))
              {
                if (ReadObligation())
                {
                  // -- continue processing
                }
                else
                {
                  continue;
                }
              }
              else
              {
                continue;
              }
            }
            else if (!IsEmpty(entities.ActivityDistributionRule.CsenetRoleCode))
            {
              // -- continue processing - no logic required
            }
            else if (!IsEmpty(entities.ActivityDistributionRule.LaPersonCode) &&
              Equal(import.Infrastructure.BusinessObjectCd, "LEA"))
            {
              local.LegalAction.Identifier =
                (int)import.Infrastructure.DenormNumeric12.GetValueOrDefault();

              if (ReadLegalAction1())
              {
                // -- continue processing
              }
              else
              {
                continue;
              }
            }
            else
            {
              continue;
            }
          }

          // ------------------------------------------------------------------------
          // At this point we have passed the test of 'Roles' and
          // either the Activity Dist Rule contains no Role info
          // or the test was true. Now we are ready to create all
          // Monitored Activity Assignments.
          // ------------------------------------------------------------------------
          local.MonitoredActivityAssignment.OverrideInd = "N";

          if (Equal(entities.ActivityDistributionRule.ResponsibilityCode, "RSP"))
            
          {
            local.MonitoredActivityAssignment.ReasonCode = "RSP";
          }
          else
          {
            local.MonitoredActivityAssignment.ReasonCode = "INF";
          }

          local.MonitoredActivityAssignment.EffectiveDate = import.Current.Date;
          local.MonitoredActivityAssignment.DiscontinueDate = local.Max.Date;

          // ------------------------------------------------------------------------
          // If multiple OSP's are read when attempting to assign
          // a Mon Act for Responsibility reasons, because of the
          // activ distr rule specified by the user, then the Mon
          // Act will be assigned to the first OSP as RSP and the
          // others as INF.
          // ------------------------------------------------------------------------
          local.CreateMonaAssignment.Count = 0;

          if (Equal(entities.ActivityDistributionRule.BusinessObjectCode, "CAS") &&
            !IsEmpty(import.Infrastructure.CaseNumber))
          {
            // ------------------------------------------------------------------------
            // This is a Catch-all for the non-assignable business object
            // as activities for them can be distributed using Case or
            // Case Units only. This is for Case.
            // ------------------------------------------------------------------------
            foreach(var item2 in ReadOfficeServiceProviderOfficeServiceProvider16())
              
            {
              export.ImportExportStartMona.Item.StartMonaAssgnmnt.Index =
                export.ImportExportStartMona.Item.StartMonaAssgnmnt.Count;
              export.ImportExportStartMona.Item.StartMonaAssgnmnt.CheckSize();

              MoveMonitoredActivityAssignment(local.MonitoredActivityAssignment,
                export.ImportExportStartMona.Update.StartMonaAssgnmnt.Update.
                  StartMonaMonitoredActivityAssignment);
              MoveOfficeServiceProvider(entities.OfficeServiceProvider,
                export.ImportExportStartMona.Update.StartMonaAssgnmnt.Update.
                  StartMonaOfficeServiceProvider);
              export.ImportExportStartMona.Update.StartMonaAssgnmnt.Update.
                StartMonaOffice.SystemGeneratedId =
                  entities.Office.SystemGeneratedId;
              export.ImportExportStartMona.Update.StartMonaAssgnmnt.Update.
                StartMonaServiceProvider.SystemGeneratedId =
                  entities.ServiceProvider.SystemGeneratedId;

              // -- Reason code should be INF for any subsequent service 
              // providers to whom the monitored activity is assigned.
              local.MonitoredActivityAssignment.ReasonCode = "INF";
              ++local.CreateMonaAssignment.Count;
            }
          }
          else if (Equal(entities.ActivityDistributionRule.BusinessObjectCode,
            "CAU") && !IsEmpty(import.Infrastructure.CaseNumber) && import
            .Infrastructure.CaseUnitNumber.GetValueOrDefault() != 0)
          {
            // ------------------------------------------------------------------------
            // This is a Catch-all for the non-assignable business object
            // as activities for them can be distributed using Case or
            // Case Units only. This is for Case Unit.
            // ------------------------------------------------------------------------
            foreach(var item2 in ReadOfficeServiceProviderOfficeServiceProvider3())
              
            {
              export.ImportExportStartMona.Item.StartMonaAssgnmnt.Index =
                export.ImportExportStartMona.Item.StartMonaAssgnmnt.Count;
              export.ImportExportStartMona.Item.StartMonaAssgnmnt.CheckSize();

              MoveMonitoredActivityAssignment(local.MonitoredActivityAssignment,
                export.ImportExportStartMona.Update.StartMonaAssgnmnt.Update.
                  StartMonaMonitoredActivityAssignment);
              MoveOfficeServiceProvider(entities.OfficeServiceProvider,
                export.ImportExportStartMona.Update.StartMonaAssgnmnt.Update.
                  StartMonaOfficeServiceProvider);
              export.ImportExportStartMona.Update.StartMonaAssgnmnt.Update.
                StartMonaOffice.SystemGeneratedId =
                  entities.Office.SystemGeneratedId;
              export.ImportExportStartMona.Update.StartMonaAssgnmnt.Update.
                StartMonaServiceProvider.SystemGeneratedId =
                  entities.ServiceProvider.SystemGeneratedId;

              // -- Reason code should be INF for any subsequent service 
              // providers to whom the monitored activity is assigned.
              local.MonitoredActivityAssignment.ReasonCode = "INF";
              ++local.CreateMonaAssignment.Count;
            }
          }
          else
          {
            // ------------------------------------------------------------------------
            // All the other assignable business objects are processed here...
            // ------------------------------------------------------------------------
            switch(TrimEnd(import.Infrastructure.BusinessObjectCd))
            {
              case "CAS":
                if (IsEmpty(import.Infrastructure.CaseNumber))
                {
                  export.Error.Index = export.Error.Count;
                  export.Error.CheckSize();

                  export.Error.Update.ProgramError.ProgramError1 =
                    "SP0000: Infrastructure record must have a case number.";
                  export.Error.Update.ProgramError.KeyInfo =
                    "MonA: Specific Distribution (Case).";

                  continue;
                }

                if (Equal(entities.ActivityDistributionRule.BusinessObjectCode,
                  "LRF"))
                {
                  foreach(var item2 in ReadOfficeServiceProviderOfficeServiceProvider9())
                    
                  {
                    export.ImportExportStartMona.Item.StartMonaAssgnmnt.Index =
                      export.ImportExportStartMona.Item.StartMonaAssgnmnt.Count;
                      
                    export.ImportExportStartMona.Item.StartMonaAssgnmnt.
                      CheckSize();

                    MoveMonitoredActivityAssignment(local.
                      MonitoredActivityAssignment,
                      export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                        Update.StartMonaMonitoredActivityAssignment);
                    MoveOfficeServiceProvider(entities.OfficeServiceProvider,
                      export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                        Update.StartMonaOfficeServiceProvider);
                    export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                      Update.StartMonaOffice.SystemGeneratedId =
                        entities.Office.SystemGeneratedId;
                    export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                      Update.StartMonaServiceProvider.SystemGeneratedId =
                        entities.ServiceProvider.SystemGeneratedId;

                    // -- Reason code should be INF for any subsequent service 
                    // providers to whom the monitored activity is assigned.
                    local.MonitoredActivityAssignment.ReasonCode = "INF";
                    ++local.CreateMonaAssignment.Count;
                  }
                }
                else
                {
                  export.Error.Index = export.Error.Count;
                  export.Error.CheckSize();

                  export.Error.Update.ProgramError.ProgramError1 =
                    "SP0000: Cannot process activity distribution rule.";
                  export.Error.Update.ProgramError.KeyInfo =
                    "MonA: ADR.BO cannot be derived from I.BO (Case).";
                  export.Error.Update.ProgramError.KeyInfo =
                    TrimEnd(export.Error.Item.ProgramError.KeyInfo) + "ADR.BO = " +
                    entities.ActivityDistributionRule.BusinessObjectCode;
                  local.SpPrintWorkSet.Text15 =
                    NumberToString(entities.Activity.ControlNumber, 15);
                  export.Error.Update.ProgramError.KeyInfo =
                    TrimEnd(export.Error.Item.ProgramError.KeyInfo) + " Act Ctrl No " +
                    Substring
                    (local.SpPrintWorkSet.Text15,
                    SpPrintWorkSet.Text15_MaxLength,
                    Verify(local.SpPrintWorkSet.Text15, "0"), 16 -
                    Verify(local.SpPrintWorkSet.Text15, "0"));
                  local.SpPrintWorkSet.Text15 =
                    NumberToString(entities.ActivityDistributionRule.
                      SystemGeneratedIdentifier, 15);
                  export.Error.Update.ProgramError.KeyInfo =
                    TrimEnd(export.Error.Item.ProgramError.KeyInfo) + " Act Dist Rule Sys Gen Id " +
                    Substring
                    (local.SpPrintWorkSet.Text15,
                    SpPrintWorkSet.Text15_MaxLength,
                    Verify(local.SpPrintWorkSet.Text15, "0"), 16 -
                    Verify(local.SpPrintWorkSet.Text15, "0"));

                  continue;
                }

                break;
              case "CAU":
                if (IsEmpty(import.Infrastructure.CaseNumber))
                {
                  export.Error.Index = export.Error.Count;
                  export.Error.CheckSize();

                  export.Error.Update.ProgramError.ProgramError1 =
                    "SP0000: Infrastructure record must have a case number.";
                  export.Error.Update.ProgramError.KeyInfo =
                    "MonA: Specific Distribution (Case Unit).";

                  continue;
                }
                else if (import.Infrastructure.CaseUnitNumber.
                  GetValueOrDefault() == 0)
                {
                  export.Error.Index = export.Error.Count;
                  export.Error.CheckSize();

                  export.Error.Update.ProgramError.ProgramError1 =
                    "SP0000: Infrastructure record must have a case unit number.";
                    
                  export.Error.Update.ProgramError.KeyInfo =
                    "MonA: Specific Distribution (Case Unit).";

                  continue;
                }

                if (Equal(entities.ActivityDistributionRule.BusinessObjectCode,
                  "LRF"))
                {
                  foreach(var item2 in ReadOfficeServiceProviderOfficeServiceProvider9())
                    
                  {
                    export.ImportExportStartMona.Item.StartMonaAssgnmnt.Index =
                      export.ImportExportStartMona.Item.StartMonaAssgnmnt.Count;
                      
                    export.ImportExportStartMona.Item.StartMonaAssgnmnt.
                      CheckSize();

                    MoveMonitoredActivityAssignment(local.
                      MonitoredActivityAssignment,
                      export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                        Update.StartMonaMonitoredActivityAssignment);
                    MoveOfficeServiceProvider(entities.OfficeServiceProvider,
                      export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                        Update.StartMonaOfficeServiceProvider);
                    export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                      Update.StartMonaOffice.SystemGeneratedId =
                        entities.Office.SystemGeneratedId;
                    export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                      Update.StartMonaServiceProvider.SystemGeneratedId =
                        entities.ServiceProvider.SystemGeneratedId;

                    // -- Reason code should be INF for any subsequent service 
                    // providers to whom the monitored activity is assigned.
                    local.MonitoredActivityAssignment.ReasonCode = "INF";
                    ++local.CreateMonaAssignment.Count;
                  }
                }
                else
                {
                  export.Error.Index = export.Error.Count;
                  export.Error.CheckSize();

                  export.Error.Update.ProgramError.ProgramError1 =
                    "SP0000: Cannot process activity distribution rule.";
                  export.Error.Update.ProgramError.KeyInfo =
                    "MonA: ADR.BO cannot be derived from I.BO (CU).";
                  export.Error.Update.ProgramError.KeyInfo =
                    TrimEnd(export.Error.Item.ProgramError.KeyInfo) + "ADR.BO = " +
                    entities.ActivityDistributionRule.BusinessObjectCode;
                  local.SpPrintWorkSet.Text15 =
                    NumberToString(entities.Activity.ControlNumber, 15);
                  export.Error.Update.ProgramError.KeyInfo =
                    TrimEnd(export.Error.Item.ProgramError.KeyInfo) + " Act Ctrl No " +
                    Substring
                    (local.SpPrintWorkSet.Text15,
                    SpPrintWorkSet.Text15_MaxLength,
                    Verify(local.SpPrintWorkSet.Text15, "0"), 16 -
                    Verify(local.SpPrintWorkSet.Text15, "0"));
                  local.SpPrintWorkSet.Text15 =
                    NumberToString(entities.ActivityDistributionRule.
                      SystemGeneratedIdentifier, 15);
                  export.Error.Update.ProgramError.KeyInfo =
                    TrimEnd(export.Error.Item.ProgramError.KeyInfo) + " Act Dist Rule Sys Gen Id " +
                    Substring
                    (local.SpPrintWorkSet.Text15,
                    SpPrintWorkSet.Text15_MaxLength,
                    Verify(local.SpPrintWorkSet.Text15, "0"), 16 -
                    Verify(local.SpPrintWorkSet.Text15, "0"));

                  continue;
                }

                break;
              case "DOC":
                // --------------
                // Beg PR # 80211
                // --------------
                if (ReadOfficeServiceProviderOfficeServiceProvider1())
                {
                  export.ImportExportStartMona.Item.StartMonaAssgnmnt.Index =
                    export.ImportExportStartMona.Item.StartMonaAssgnmnt.Count;
                  export.ImportExportStartMona.Item.StartMonaAssgnmnt.
                    CheckSize();

                  MoveMonitoredActivityAssignment(local.
                    MonitoredActivityAssignment,
                    export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                      Update.StartMonaMonitoredActivityAssignment);
                  MoveOfficeServiceProvider(entities.OfficeServiceProvider,
                    export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                      Update.StartMonaOfficeServiceProvider);
                  export.ImportExportStartMona.Update.StartMonaAssgnmnt.Update.
                    StartMonaOffice.SystemGeneratedId =
                      entities.Office.SystemGeneratedId;
                  export.ImportExportStartMona.Update.StartMonaAssgnmnt.Update.
                    StartMonaServiceProvider.SystemGeneratedId =
                      entities.ServiceProvider.SystemGeneratedId;

                  // -- Reason code should be INF for any subsequent service 
                  // providers to whom the monitored activity is assigned.
                  local.MonitoredActivityAssignment.ReasonCode = "INF";
                  ++local.CreateMonaAssignment.Count;
                }

                if (!entities.OfficeServiceProvider.Populated)
                {
                  export.Error.Index = export.Error.Count;
                  export.Error.CheckSize();

                  export.Error.Update.ProgramError.ProgramError1 =
                    "Office service provider was not found.";
                  export.Error.Update.ProgramError.KeyInfo =
                    "MonA: ADR.BO cannot be derived from I.BO (DOC).";
                  export.Error.Update.ProgramError.KeyInfo =
                    TrimEnd(export.Error.Item.ProgramError.KeyInfo) + "ADR.BO = " +
                    entities.ActivityDistributionRule.BusinessObjectCode;
                }

                // --------------
                // End PR # 80211
                // --------------
                break;
              case "LRF":
                if (Equal(entities.ActivityDistributionRule.BusinessObjectCode,
                  "LRF"))
                {
                  if (import.Infrastructure.DenormNumeric12.
                    GetValueOrDefault() == 0)
                  {
                    export.Error.Index = export.Error.Count;
                    export.Error.CheckSize();

                    export.Error.Update.ProgramError.ProgramError1 =
                      "SP0000: Infrastructure record is missing business object's value.";
                      
                    export.Error.Update.ProgramError.KeyInfo =
                      "MonA: Specific open, I.BO (LRF).";

                    continue;
                  }

                  local.LegalReferral.Identifier =
                    (int)import.Infrastructure.DenormNumeric12.
                      GetValueOrDefault();

                  foreach(var item2 in ReadOfficeServiceProviderOfficeServiceProvider8())
                    
                  {
                    export.ImportExportStartMona.Item.StartMonaAssgnmnt.Index =
                      export.ImportExportStartMona.Item.StartMonaAssgnmnt.Count;
                      
                    export.ImportExportStartMona.Item.StartMonaAssgnmnt.
                      CheckSize();

                    MoveMonitoredActivityAssignment(local.
                      MonitoredActivityAssignment,
                      export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                        Update.StartMonaMonitoredActivityAssignment);
                    MoveOfficeServiceProvider(entities.OfficeServiceProvider,
                      export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                        Update.StartMonaOfficeServiceProvider);
                    export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                      Update.StartMonaOffice.SystemGeneratedId =
                        entities.Office.SystemGeneratedId;
                    export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                      Update.StartMonaServiceProvider.SystemGeneratedId =
                        entities.ServiceProvider.SystemGeneratedId;

                    // -- Reason code should be INF for any subsequent service 
                    // providers to whom the monitored activity is assigned.
                    local.MonitoredActivityAssignment.ReasonCode = "INF";
                    ++local.CreateMonaAssignment.Count;
                  }
                }
                else
                {
                  export.Error.Index = export.Error.Count;
                  export.Error.CheckSize();

                  export.Error.Update.ProgramError.ProgramError1 =
                    "SP0000: Cannot process activity distribution rule.";
                  export.Error.Update.ProgramError.KeyInfo =
                    "MonA: ADR.BO cannot be derived from I.BO (LRF).";
                  export.Error.Update.ProgramError.KeyInfo =
                    TrimEnd(export.Error.Item.ProgramError.KeyInfo) + "ADR.BO = " +
                    entities.ActivityDistributionRule.BusinessObjectCode;
                  local.SpPrintWorkSet.Text15 =
                    NumberToString(entities.Activity.ControlNumber, 15);
                  export.Error.Update.ProgramError.KeyInfo =
                    TrimEnd(export.Error.Item.ProgramError.KeyInfo) + " Act Ctrl No " +
                    Substring
                    (local.SpPrintWorkSet.Text15,
                    SpPrintWorkSet.Text15_MaxLength,
                    Verify(local.SpPrintWorkSet.Text15, "0"), 16 -
                    Verify(local.SpPrintWorkSet.Text15, "0"));
                  local.SpPrintWorkSet.Text15 =
                    NumberToString(entities.ActivityDistributionRule.
                      SystemGeneratedIdentifier, 15);
                  export.Error.Update.ProgramError.KeyInfo =
                    TrimEnd(export.Error.Item.ProgramError.KeyInfo) + " Act Dist Rule Sys Gen Id " +
                    Substring
                    (local.SpPrintWorkSet.Text15,
                    SpPrintWorkSet.Text15_MaxLength,
                    Verify(local.SpPrintWorkSet.Text15, "0"), 16 -
                    Verify(local.SpPrintWorkSet.Text15, "0"));

                  continue;
                }

                break;
              case "ADA":
                if (import.Infrastructure.DenormNumeric12.GetValueOrDefault() ==
                  0)
                {
                  export.Error.Index = export.Error.Count;
                  export.Error.CheckSize();

                  export.Error.Update.ProgramError.ProgramError1 =
                    "SP0000: Infrastructure record is missing business object's value.";
                    
                  export.Error.Update.ProgramError.KeyInfo =
                    "MonA: Specific open, I.BO (ADA).";

                  continue;
                }

                switch(TrimEnd(entities.ActivityDistributionRule.
                  BusinessObjectCode))
                {
                  case "ADA":
                    local.AdministrativeAppeal.Identifier =
                      (int)import.Infrastructure.DenormNumeric12.
                        GetValueOrDefault();

                    foreach(var item2 in ReadOfficeServiceProviderOfficeServiceProvider15())
                      
                    {
                      export.ImportExportStartMona.Item.StartMonaAssgnmnt.
                        Index =
                          export.ImportExportStartMona.Item.StartMonaAssgnmnt.
                          Count;
                      export.ImportExportStartMona.Item.StartMonaAssgnmnt.
                        CheckSize();

                      MoveMonitoredActivityAssignment(local.
                        MonitoredActivityAssignment,
                        export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                          Update.StartMonaMonitoredActivityAssignment);
                      MoveOfficeServiceProvider(entities.OfficeServiceProvider,
                        export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                          Update.StartMonaOfficeServiceProvider);
                      export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                        Update.StartMonaOffice.SystemGeneratedId =
                          entities.Office.SystemGeneratedId;
                      export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                        Update.StartMonaServiceProvider.SystemGeneratedId =
                          entities.ServiceProvider.SystemGeneratedId;

                      // -- Reason code should be INF for any subsequent service
                      // providers to whom the monitored activity is assigned.
                      local.MonitoredActivityAssignment.ReasonCode = "INF";
                      ++local.CreateMonaAssignment.Count;
                    }

                    break;
                  case "OAA":
                    local.AdministrativeAppeal.Identifier =
                      (int)import.Infrastructure.DenormNumeric12.
                        GetValueOrDefault();

                    foreach(var item2 in ReadOfficeServiceProviderOfficeServiceProvider11())
                      
                    {
                      export.ImportExportStartMona.Item.StartMonaAssgnmnt.
                        Index =
                          export.ImportExportStartMona.Item.StartMonaAssgnmnt.
                          Count;
                      export.ImportExportStartMona.Item.StartMonaAssgnmnt.
                        CheckSize();

                      MoveMonitoredActivityAssignment(local.
                        MonitoredActivityAssignment,
                        export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                          Update.StartMonaMonitoredActivityAssignment);
                      MoveOfficeServiceProvider(entities.OfficeServiceProvider,
                        export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                          Update.StartMonaOfficeServiceProvider);
                      export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                        Update.StartMonaOffice.SystemGeneratedId =
                          entities.Office.SystemGeneratedId;
                      export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                        Update.StartMonaServiceProvider.SystemGeneratedId =
                          entities.ServiceProvider.SystemGeneratedId;

                      // -- Reason code should be INF for any subsequent service
                      // providers to whom the monitored activity is assigned.
                      local.MonitoredActivityAssignment.ReasonCode = "INF";
                      ++local.CreateMonaAssignment.Count;
                    }

                    break;
                  default:
                    export.Error.Index = export.Error.Count;
                    export.Error.CheckSize();

                    export.Error.Update.ProgramError.ProgramError1 =
                      "SP0000: Cannot process activity distribution rule.";
                    export.Error.Update.ProgramError.KeyInfo =
                      "MonA: ADR.BO cannot be derived from I.BO (ADA).";
                    export.Error.Update.ProgramError.KeyInfo =
                      TrimEnd(export.Error.Item.ProgramError.KeyInfo) + "ADR.BO = " +
                      entities.ActivityDistributionRule.BusinessObjectCode;
                    local.SpPrintWorkSet.Text15 =
                      NumberToString(entities.Activity.ControlNumber, 15);
                    export.Error.Update.ProgramError.KeyInfo =
                      TrimEnd(export.Error.Item.ProgramError.KeyInfo) + " Act Ctrl No " +
                      Substring
                      (local.SpPrintWorkSet.Text15,
                      SpPrintWorkSet.Text15_MaxLength,
                      Verify(local.SpPrintWorkSet.Text15, "0"), 16 -
                      Verify(local.SpPrintWorkSet.Text15, "0"));
                    local.SpPrintWorkSet.Text15 =
                      NumberToString(entities.ActivityDistributionRule.
                        SystemGeneratedIdentifier, 15);
                    export.Error.Update.ProgramError.KeyInfo =
                      TrimEnd(export.Error.Item.ProgramError.KeyInfo) + " Act Dist Rule Sys Gen Id " +
                      Substring
                      (local.SpPrintWorkSet.Text15,
                      SpPrintWorkSet.Text15_MaxLength,
                      Verify(local.SpPrintWorkSet.Text15, "0"), 16 -
                      Verify(local.SpPrintWorkSet.Text15, "0"));

                    continue;
                }

                break;
              case "INR":
                // ---------------------------------------------------
                // Test for the present of the csenet_transaction_date
                // ---------------------------------------------------
                if (Equal(import.Infrastructure.DenormDate,
                  local.Linitialized.Date))
                {
                  export.Error.Index = export.Error.Count;
                  export.Error.CheckSize();

                  export.Error.Update.ProgramError.ProgramError1 =
                    "SP0000: Infrastructure record is missing business object's value.";
                    
                  export.Error.Update.ProgramError.KeyInfo =
                    "MonA: Specific open, I.BO (INR) CSENET Trans Date.";

                  continue;
                }

                // ---------------------------------------------------
                // Test for the present of the transaction_serial_num
                // ---------------------------------------------------
                if (import.Infrastructure.DenormNumeric12.GetValueOrDefault() ==
                  0)
                {
                  export.Error.Index = export.Error.Count;
                  export.Error.CheckSize();

                  export.Error.Update.ProgramError.ProgramError1 =
                    "SP0000: Infrastructure record is missing business object's value.";
                    
                  export.Error.Update.ProgramError.KeyInfo =
                    "MonA: Specific open, I.BO (INR) Trans Ser Num.";

                  continue;
                }

                if (Equal(entities.ActivityDistributionRule.BusinessObjectCode,
                  "INR"))
                {
                  foreach(var item2 in ReadOfficeServiceProviderOfficeServiceProvider7())
                    
                  {
                    export.ImportExportStartMona.Item.StartMonaAssgnmnt.Index =
                      export.ImportExportStartMona.Item.StartMonaAssgnmnt.Count;
                      
                    export.ImportExportStartMona.Item.StartMonaAssgnmnt.
                      CheckSize();

                    MoveMonitoredActivityAssignment(local.
                      MonitoredActivityAssignment,
                      export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                        Update.StartMonaMonitoredActivityAssignment);
                    MoveOfficeServiceProvider(entities.OfficeServiceProvider,
                      export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                        Update.StartMonaOfficeServiceProvider);
                    export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                      Update.StartMonaOffice.SystemGeneratedId =
                        entities.Office.SystemGeneratedId;
                    export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                      Update.StartMonaServiceProvider.SystemGeneratedId =
                        entities.ServiceProvider.SystemGeneratedId;

                    // -- Reason code should be INF for any subsequent service 
                    // providers to whom the monitored activity is assigned.
                    local.MonitoredActivityAssignment.ReasonCode = "INF";
                    ++local.CreateMonaAssignment.Count;
                  }
                }
                else
                {
                  export.Error.Index = export.Error.Count;
                  export.Error.CheckSize();

                  export.Error.Update.ProgramError.ProgramError1 =
                    "SP0000: Cannot process activity distribution rule.";
                  export.Error.Update.ProgramError.KeyInfo =
                    "MonA: ADR.BO cannot be derived from I.BO (INR).";
                  export.Error.Update.ProgramError.KeyInfo =
                    TrimEnd(export.Error.Item.ProgramError.KeyInfo) + "ADR.BO = " +
                    entities.ActivityDistributionRule.BusinessObjectCode;
                  local.SpPrintWorkSet.Text15 =
                    NumberToString(entities.Activity.ControlNumber, 15);
                  export.Error.Update.ProgramError.KeyInfo =
                    TrimEnd(export.Error.Item.ProgramError.KeyInfo) + " Act Ctrl No " +
                    Substring
                    (local.SpPrintWorkSet.Text15,
                    SpPrintWorkSet.Text15_MaxLength,
                    Verify(local.SpPrintWorkSet.Text15, "0"), 16 -
                    Verify(local.SpPrintWorkSet.Text15, "0"));
                  local.SpPrintWorkSet.Text15 =
                    NumberToString(entities.ActivityDistributionRule.
                      SystemGeneratedIdentifier, 15);
                  export.Error.Update.ProgramError.KeyInfo =
                    TrimEnd(export.Error.Item.ProgramError.KeyInfo) + " Act Dist Rule Sys Gen Id " +
                    Substring
                    (local.SpPrintWorkSet.Text15,
                    SpPrintWorkSet.Text15_MaxLength,
                    Verify(local.SpPrintWorkSet.Text15, "0"), 16 -
                    Verify(local.SpPrintWorkSet.Text15, "0"));

                  continue;
                }

                break;
              case "LEA":
                if (import.Infrastructure.DenormNumeric12.GetValueOrDefault() ==
                  0)
                {
                  export.Error.Index = export.Error.Count;
                  export.Error.CheckSize();

                  export.Error.Update.ProgramError.ProgramError1 =
                    "SP0000: Infrastructure record is missing business object's value.";
                    
                  export.Error.Update.ProgramError.KeyInfo =
                    "MonA: Specific open, I.BO (LEA).";

                  continue;
                }

                switch(TrimEnd(entities.ActivityDistributionRule.
                  BusinessObjectCode))
                {
                  case "LEA":
                    local.LegalAction.Identifier =
                      (int)import.Infrastructure.DenormNumeric12.
                        GetValueOrDefault();

                    foreach(var item2 in ReadOfficeServiceProviderOfficeServiceProvider14())
                      
                    {
                      export.ImportExportStartMona.Item.StartMonaAssgnmnt.
                        Index =
                          export.ImportExportStartMona.Item.StartMonaAssgnmnt.
                          Count;
                      export.ImportExportStartMona.Item.StartMonaAssgnmnt.
                        CheckSize();

                      MoveMonitoredActivityAssignment(local.
                        MonitoredActivityAssignment,
                        export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                          Update.StartMonaMonitoredActivityAssignment);
                      MoveOfficeServiceProvider(entities.OfficeServiceProvider,
                        export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                          Update.StartMonaOfficeServiceProvider);
                      export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                        Update.StartMonaOffice.SystemGeneratedId =
                          entities.Office.SystemGeneratedId;
                      export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                        Update.StartMonaServiceProvider.SystemGeneratedId =
                          entities.ServiceProvider.SystemGeneratedId;

                      // -- Reason code should be INF for any subsequent service
                      // providers to whom the monitored activity is assigned.
                      local.MonitoredActivityAssignment.ReasonCode = "INF";
                      ++local.CreateMonaAssignment.Count;
                    }

                    break;
                  case "OBL":
                    local.LegalAction.Identifier =
                      (int)import.Infrastructure.DenormNumeric12.
                        GetValueOrDefault();

                    foreach(var item2 in ReadOfficeServiceProviderOfficeServiceProvider12())
                      
                    {
                      export.ImportExportStartMona.Item.StartMonaAssgnmnt.
                        Index =
                          export.ImportExportStartMona.Item.StartMonaAssgnmnt.
                          Count;
                      export.ImportExportStartMona.Item.StartMonaAssgnmnt.
                        CheckSize();

                      MoveMonitoredActivityAssignment(local.
                        MonitoredActivityAssignment,
                        export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                          Update.StartMonaMonitoredActivityAssignment);
                      MoveOfficeServiceProvider(entities.OfficeServiceProvider,
                        export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                          Update.StartMonaOfficeServiceProvider);
                      export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                        Update.StartMonaOffice.SystemGeneratedId =
                          entities.Office.SystemGeneratedId;
                      export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                        Update.StartMonaServiceProvider.SystemGeneratedId =
                          entities.ServiceProvider.SystemGeneratedId;

                      // -- Reason code should be INF for any subsequent service
                      // providers to whom the monitored activity is assigned.
                      local.MonitoredActivityAssignment.ReasonCode = "INF";
                      ++local.CreateMonaAssignment.Count;
                    }

                    break;
                  default:
                    export.Error.Index = export.Error.Count;
                    export.Error.CheckSize();

                    export.Error.Update.ProgramError.ProgramError1 =
                      "SP0000: Cannot process activity distribution rule.";
                    export.Error.Update.ProgramError.KeyInfo =
                      "MonA: ADR.BO cannot be derived from I.BO (LEA).";
                    export.Error.Update.ProgramError.KeyInfo =
                      TrimEnd(export.Error.Item.ProgramError.KeyInfo) + "ADR.BO = " +
                      entities.ActivityDistributionRule.BusinessObjectCode;
                    local.SpPrintWorkSet.Text15 =
                      NumberToString(entities.Activity.ControlNumber, 15);
                    export.Error.Update.ProgramError.KeyInfo =
                      TrimEnd(export.Error.Item.ProgramError.KeyInfo) + " Act Ctrl No " +
                      Substring
                      (local.SpPrintWorkSet.Text15,
                      SpPrintWorkSet.Text15_MaxLength,
                      Verify(local.SpPrintWorkSet.Text15, "0"), 16 -
                      Verify(local.SpPrintWorkSet.Text15, "0"));
                    local.SpPrintWorkSet.Text15 =
                      NumberToString(entities.ActivityDistributionRule.
                        SystemGeneratedIdentifier, 15);
                    export.Error.Update.ProgramError.KeyInfo =
                      TrimEnd(export.Error.Item.ProgramError.KeyInfo) + " Act Dist Rule Sys Gen Id " +
                      Substring
                      (local.SpPrintWorkSet.Text15,
                      SpPrintWorkSet.Text15_MaxLength,
                      Verify(local.SpPrintWorkSet.Text15, "0"), 16 -
                      Verify(local.SpPrintWorkSet.Text15, "0"));

                    continue;
                }

                break;
              case "OBL":
                switch(TrimEnd(entities.ActivityDistributionRule.
                  BusinessObjectCode))
                {
                  case "OBL":
                    foreach(var item2 in ReadOfficeServiceProviderOfficeServiceProvider13())
                      
                    {
                      export.ImportExportStartMona.Item.StartMonaAssgnmnt.
                        Index =
                          export.ImportExportStartMona.Item.StartMonaAssgnmnt.
                          Count;
                      export.ImportExportStartMona.Item.StartMonaAssgnmnt.
                        CheckSize();

                      MoveMonitoredActivityAssignment(local.
                        MonitoredActivityAssignment,
                        export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                          Update.StartMonaMonitoredActivityAssignment);
                      MoveOfficeServiceProvider(entities.OfficeServiceProvider,
                        export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                          Update.StartMonaOfficeServiceProvider);
                      export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                        Update.StartMonaOffice.SystemGeneratedId =
                          entities.Office.SystemGeneratedId;
                      export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                        Update.StartMonaServiceProvider.SystemGeneratedId =
                          entities.ServiceProvider.SystemGeneratedId;

                      // -- Reason code should be INF for any subsequent service
                      // providers to whom the monitored activity is assigned.
                      local.MonitoredActivityAssignment.ReasonCode = "INF";
                      ++local.CreateMonaAssignment.Count;
                    }

                    break;
                  case "LEA":
                    foreach(var item2 in ReadOfficeServiceProviderOfficeServiceProvider4())
                      
                    {
                      export.ImportExportStartMona.Item.StartMonaAssgnmnt.
                        Index =
                          export.ImportExportStartMona.Item.StartMonaAssgnmnt.
                          Count;
                      export.ImportExportStartMona.Item.StartMonaAssgnmnt.
                        CheckSize();

                      MoveMonitoredActivityAssignment(local.
                        MonitoredActivityAssignment,
                        export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                          Update.StartMonaMonitoredActivityAssignment);
                      MoveOfficeServiceProvider(entities.OfficeServiceProvider,
                        export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                          Update.StartMonaOfficeServiceProvider);
                      export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                        Update.StartMonaOffice.SystemGeneratedId =
                          entities.Office.SystemGeneratedId;
                      export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                        Update.StartMonaServiceProvider.SystemGeneratedId =
                          entities.ServiceProvider.SystemGeneratedId;

                      // -- Reason code should be INF for any subsequent service
                      // providers to whom the monitored activity is assigned.
                      local.MonitoredActivityAssignment.ReasonCode = "INF";
                      ++local.CreateMonaAssignment.Count;
                    }

                    break;
                  default:
                    export.Error.Index = export.Error.Count;
                    export.Error.CheckSize();

                    export.Error.Update.ProgramError.ProgramError1 =
                      "SP0000: Cannot process activity distribution rule.";
                    export.Error.Update.ProgramError.KeyInfo =
                      "MonA: ADR.BO cannot be derived from I.BO (OBL).";
                    export.Error.Update.ProgramError.KeyInfo =
                      TrimEnd(export.Error.Item.ProgramError.KeyInfo) + "ADR.BO = " +
                      entities.ActivityDistributionRule.BusinessObjectCode;
                    local.SpPrintWorkSet.Text15 =
                      NumberToString(entities.Activity.ControlNumber, 15);
                    export.Error.Update.ProgramError.KeyInfo =
                      TrimEnd(export.Error.Item.ProgramError.KeyInfo) + " Act Ctrl No " +
                      Substring
                      (local.SpPrintWorkSet.Text15,
                      SpPrintWorkSet.Text15_MaxLength,
                      Verify(local.SpPrintWorkSet.Text15, "0"), 16 -
                      Verify(local.SpPrintWorkSet.Text15, "0"));
                    local.SpPrintWorkSet.Text15 =
                      NumberToString(entities.ActivityDistributionRule.
                        SystemGeneratedIdentifier, 15);
                    export.Error.Update.ProgramError.KeyInfo =
                      TrimEnd(export.Error.Item.ProgramError.KeyInfo) + " Act Dist Rule Sys Gen Id " +
                      Substring
                      (local.SpPrintWorkSet.Text15,
                      SpPrintWorkSet.Text15_MaxLength,
                      Verify(local.SpPrintWorkSet.Text15, "0"), 16 -
                      Verify(local.SpPrintWorkSet.Text15, "0"));

                    continue;
                }

                break;
              case "OAA":
                switch(TrimEnd(entities.ActivityDistributionRule.
                  BusinessObjectCode))
                {
                  case "OAA":
                    foreach(var item2 in ReadOfficeServiceProviderOfficeServiceProvider10())
                      
                    {
                      export.ImportExportStartMona.Item.StartMonaAssgnmnt.
                        Index =
                          export.ImportExportStartMona.Item.StartMonaAssgnmnt.
                          Count;
                      export.ImportExportStartMona.Item.StartMonaAssgnmnt.
                        CheckSize();

                      MoveMonitoredActivityAssignment(local.
                        MonitoredActivityAssignment,
                        export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                          Update.StartMonaMonitoredActivityAssignment);
                      MoveOfficeServiceProvider(entities.OfficeServiceProvider,
                        export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                          Update.StartMonaOfficeServiceProvider);
                      export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                        Update.StartMonaOffice.SystemGeneratedId =
                          entities.Office.SystemGeneratedId;
                      export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                        Update.StartMonaServiceProvider.SystemGeneratedId =
                          entities.ServiceProvider.SystemGeneratedId;

                      // -- Reason code should be INF for any subsequent service
                      // providers to whom the monitored activity is assigned.
                      local.MonitoredActivityAssignment.ReasonCode = "INF";
                      ++local.CreateMonaAssignment.Count;
                    }

                    break;
                  case "ADA":
                    foreach(var item2 in ReadOfficeServiceProviderOfficeServiceProvider5())
                      
                    {
                      export.ImportExportStartMona.Item.StartMonaAssgnmnt.
                        Index =
                          export.ImportExportStartMona.Item.StartMonaAssgnmnt.
                          Count;
                      export.ImportExportStartMona.Item.StartMonaAssgnmnt.
                        CheckSize();

                      MoveMonitoredActivityAssignment(local.
                        MonitoredActivityAssignment,
                        export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                          Update.StartMonaMonitoredActivityAssignment);
                      MoveOfficeServiceProvider(entities.OfficeServiceProvider,
                        export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                          Update.StartMonaOfficeServiceProvider);
                      export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                        Update.StartMonaOffice.SystemGeneratedId =
                          entities.Office.SystemGeneratedId;
                      export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                        Update.StartMonaServiceProvider.SystemGeneratedId =
                          entities.ServiceProvider.SystemGeneratedId;

                      // -- Reason code should be INF for any subsequent service
                      // providers to whom the monitored activity is assigned.
                      local.MonitoredActivityAssignment.ReasonCode = "INF";
                      ++local.CreateMonaAssignment.Count;
                    }

                    break;
                  case "OBL":
                    foreach(var item2 in ReadOfficeServiceProviderOfficeServiceProvider6())
                      
                    {
                      export.ImportExportStartMona.Item.StartMonaAssgnmnt.
                        Index =
                          export.ImportExportStartMona.Item.StartMonaAssgnmnt.
                          Count;
                      export.ImportExportStartMona.Item.StartMonaAssgnmnt.
                        CheckSize();

                      MoveMonitoredActivityAssignment(local.
                        MonitoredActivityAssignment,
                        export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                          Update.StartMonaMonitoredActivityAssignment);
                      MoveOfficeServiceProvider(entities.OfficeServiceProvider,
                        export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                          Update.StartMonaOfficeServiceProvider);
                      export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                        Update.StartMonaOffice.SystemGeneratedId =
                          entities.Office.SystemGeneratedId;
                      export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                        Update.StartMonaServiceProvider.SystemGeneratedId =
                          entities.ServiceProvider.SystemGeneratedId;

                      // -- Reason code should be INF for any subsequent service
                      // providers to whom the monitored activity is assigned.
                      local.MonitoredActivityAssignment.ReasonCode = "INF";
                      ++local.CreateMonaAssignment.Count;
                    }

                    break;
                  case "LEA":
                    foreach(var item2 in ReadOfficeServiceProviderOfficeServiceProvider2())
                      
                    {
                      export.ImportExportStartMona.Item.StartMonaAssgnmnt.
                        Index =
                          export.ImportExportStartMona.Item.StartMonaAssgnmnt.
                          Count;
                      export.ImportExportStartMona.Item.StartMonaAssgnmnt.
                        CheckSize();

                      MoveMonitoredActivityAssignment(local.
                        MonitoredActivityAssignment,
                        export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                          Update.StartMonaMonitoredActivityAssignment);
                      MoveOfficeServiceProvider(entities.OfficeServiceProvider,
                        export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                          Update.StartMonaOfficeServiceProvider);
                      export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                        Update.StartMonaOffice.SystemGeneratedId =
                          entities.Office.SystemGeneratedId;
                      export.ImportExportStartMona.Update.StartMonaAssgnmnt.
                        Update.StartMonaServiceProvider.SystemGeneratedId =
                          entities.ServiceProvider.SystemGeneratedId;

                      // -- Reason code should be INF for any subsequent service
                      // providers to whom the monitored activity is assigned.
                      local.MonitoredActivityAssignment.ReasonCode = "INF";
                      ++local.CreateMonaAssignment.Count;
                    }

                    break;
                  default:
                    export.Error.Index = export.Error.Count;
                    export.Error.CheckSize();

                    export.Error.Update.ProgramError.ProgramError1 =
                      "SP0000: Cannot process activity distribution rule.";
                    export.Error.Update.ProgramError.KeyInfo =
                      "MonA: ADR.BO cannot be derived from I.BO (OAA).";
                    export.Error.Update.ProgramError.KeyInfo =
                      TrimEnd(export.Error.Item.ProgramError.KeyInfo) + "ADR.BO = " +
                      entities.ActivityDistributionRule.BusinessObjectCode;
                    local.SpPrintWorkSet.Text15 =
                      NumberToString(entities.Activity.ControlNumber, 15);
                    export.Error.Update.ProgramError.KeyInfo =
                      TrimEnd(export.Error.Item.ProgramError.KeyInfo) + " Act Ctrl No " +
                      Substring
                      (local.SpPrintWorkSet.Text15,
                      SpPrintWorkSet.Text15_MaxLength,
                      Verify(local.SpPrintWorkSet.Text15, "0"), 16 -
                      Verify(local.SpPrintWorkSet.Text15, "0"));
                    local.SpPrintWorkSet.Text15 =
                      NumberToString(entities.ActivityDistributionRule.
                        SystemGeneratedIdentifier, 15);
                    export.Error.Update.ProgramError.KeyInfo =
                      TrimEnd(export.Error.Item.ProgramError.KeyInfo) + " Act Dist Rule Sys Gen Id " +
                      Substring
                      (local.SpPrintWorkSet.Text15,
                      SpPrintWorkSet.Text15_MaxLength,
                      Verify(local.SpPrintWorkSet.Text15, "0"), 16 -
                      Verify(local.SpPrintWorkSet.Text15, "0"));

                    continue;
                }

                break;
              default:
                export.Error.Index = export.Error.Count;
                export.Error.CheckSize();

                export.Error.Update.ProgramError.ProgramError1 =
                  "SP0000: Cannot process activity distribution rule.";
                export.Error.Update.ProgramError.KeyInfo =
                  "MonA: This I.BO is not handled explicitly and couldn't process generically.";
                  
                export.Error.Update.ProgramError.KeyInfo =
                  TrimEnd(export.Error.Item.ProgramError.KeyInfo) + "ADR.BO = " +
                  entities.ActivityDistributionRule.BusinessObjectCode;
                local.SpPrintWorkSet.Text15 =
                  NumberToString(entities.Activity.ControlNumber, 15);
                export.Error.Update.ProgramError.KeyInfo =
                  TrimEnd(export.Error.Item.ProgramError.KeyInfo) + " Act Ctrl No " +
                  Substring
                  (local.SpPrintWorkSet.Text15, SpPrintWorkSet.Text15_MaxLength,
                  Verify(local.SpPrintWorkSet.Text15, "0"), 16 -
                  Verify(local.SpPrintWorkSet.Text15, "0"));
                local.SpPrintWorkSet.Text15 =
                  NumberToString(entities.ActivityDistributionRule.
                    SystemGeneratedIdentifier, 15);
                export.Error.Update.ProgramError.KeyInfo =
                  TrimEnd(export.Error.Item.ProgramError.KeyInfo) + " Act Dist Rule Sys Gen Id " +
                  Substring
                  (local.SpPrintWorkSet.Text15, SpPrintWorkSet.Text15_MaxLength,
                  Verify(local.SpPrintWorkSet.Text15, "0"), 16 -
                  Verify(local.SpPrintWorkSet.Text15, "0"));

                continue;
            }
          }

          if (local.CreateMonaAssignment.Count == 0)
          {
            // -- There are a number of events that are not to be written to the
            // error report due to a monitored activity error.  Check if this
            // is one of them.
            local.WorkArea.Text3 =
              NumberToString(import.Event1.ControlNumber, 14, 2) + "/";

            if (Find(
              "02/05/07/20/27/29/30/31/32/36/37/38/39/40/41/42/43/95/96/97/98/99/"
              , local.WorkArea.Text3) > 0)
            {
              UseSpEventsToBypassErrorReport();

              if (AsChar(export.BypassErrorReport.Flag) == 'Y')
              {
                // -------------------------------------------------------------------
                // If the following events are older than 4 days, set the
                // infrastructure process status to "H"; otherwise set to "E" 
                // and
                // reprocess when Job # 13 is run again.
                // Per Sana Beall and Claudia Taylor (SME's responsible for the
                // Event Processor and Legal area respectively), this has been
                // done for "signifying the attempt so that the addition of a 
                // legal
                // action is reflected on the HIST screen.  The reason being
                // the worker is going to assign the Legal Action they are going
                // to assign it (as a general rule) the same day they add it to 
                // the
                // system."
                // After bringing forth the problem of setting ALL records to "
                // H"
                // in this situation, I brought the ramifications this blanket
                // change would entail.  The 4 day period was then discussed
                // and agreed upon by the two.
                // -------------------------------------------------------------------
                if (import.Infrastructure.EventId == 42 || import
                  .Infrastructure.EventId == 43 || import
                  .Infrastructure.EventId == 95)
                {
                  if (Lt(import.Infrastructure.CreatedTimestamp, Now().AddDays(-
                    4)))
                  {
                    if (AsChar(import.EventDetail.LogToDiaryInd) == 'Y')
                    {
                      export.Infrastructure.ProcessStatus = "H";
                    }
                    else
                    {
                      export.Infrastructure.ProcessStatus = "P";
                    }

                    // -- Reset the last pointer on the export error group to 
                    // eliminate any errors added in this cab.
                    //    This will insure that any errors generated during 
                    // exception routine processing are retained.
                    //    This was deliberately done because that's how the 
                    // original logic worked.
                    export.Error.Count = local.SaveLastOfErrorGroup.Count;
                  }
                  else
                  {
                    export.Infrastructure.ProcessStatus = "E";
                  }
                }
                else
                {
                  export.Infrastructure.ProcessStatus = "E";
                }

                return;
              }
              else
              {
                export.Infrastructure.ProcessStatus = "E";
              }
            }

            export.Error.Index = export.Error.Count;
            export.Error.CheckSize();

            export.Error.Update.ProgramError.ProgramError1 =
              "SP0000: Unable to create monitored activity assignment.";
            export.Error.Update.ProgramError.KeyInfo =
              "MonA: Attempt to assign failed.";

            // -- Beg PR # 108456
            export.Error.Update.ProgramError.KeyInfo =
              TrimEnd(export.Error.Item.ProgramError.KeyInfo) + "ADR.BO = " + entities
              .ActivityDistributionRule.BusinessObjectCode;
            local.SpPrintWorkSet.Text15 =
              NumberToString(entities.Activity.ControlNumber, 15);
            export.Error.Update.ProgramError.KeyInfo =
              TrimEnd(export.Error.Item.ProgramError.KeyInfo) + " Act Ctrl No " +
              Substring
              (local.SpPrintWorkSet.Text15, SpPrintWorkSet.Text15_MaxLength,
              Verify(local.SpPrintWorkSet.Text15, "0"), 16 -
              Verify(local.SpPrintWorkSet.Text15, "0"));
            local.SpPrintWorkSet.Text15 =
              NumberToString(entities.ActivityDistributionRule.
                SystemGeneratedIdentifier, 15);
            export.Error.Update.ProgramError.KeyInfo =
              TrimEnd(export.Error.Item.ProgramError.KeyInfo) + " Act Dist Rule Sys Gen Id " +
              Substring
              (local.SpPrintWorkSet.Text15, SpPrintWorkSet.Text15_MaxLength,
              Verify(local.SpPrintWorkSet.Text15, "0"), 16 -
              Verify(local.SpPrintWorkSet.Text15, "0"));
            local.SpPrintWorkSet.Text15 =
              NumberToString(entities.ActivityDetail.SystemGeneratedIdentifier,
              15);
            export.Error.Update.ProgramError.KeyInfo =
              TrimEnd(export.Error.Item.ProgramError.KeyInfo) + " Act Detail Sys Gen Id " +
              Substring
              (local.SpPrintWorkSet.Text15, SpPrintWorkSet.Text15_MaxLength,
              Verify(local.SpPrintWorkSet.Text15, "0"), 16 -
              Verify(local.SpPrintWorkSet.Text15, "0"));

            // -- End PR # 108456
            continue;
          }
        }

        // ------------------------------------------------------------------------
        // If the subscript of the monitored activity assignment group is
        // zero then we didn't find a service provider for any of the activity
        // distribution rules.
        // ------------------------------------------------------------------------
        if (export.ImportExportStartMona.Item.StartMonaAssgnmnt.Index == -1)
        {
          // -- Remove the monitored activity from the export group by 
          // decrementing LAST of the group by 1.
          export.ImportExportStartMona.Update.StartMona.Assign(local.Null1);
          --export.ImportExportStartMona.Count;

          export.Error.Index = export.Error.Count;
          export.Error.CheckSize();

          export.Error.Update.ProgramError.ProgramError1 =
            "SP0000: Unable to create monitored activity assignment.";
          export.Error.Update.ProgramError.KeyInfo =
            "MonA: No existing assign match w/any ADR.";
          export.Error.Update.ProgramError.KeyInfo =
            TrimEnd(export.Error.Item.ProgramError.KeyInfo) + "ADR.BO = " + entities
            .ActivityDistributionRule.BusinessObjectCode;
          local.SpPrintWorkSet.Text15 =
            NumberToString(entities.Activity.ControlNumber, 15);
          export.Error.Update.ProgramError.KeyInfo =
            TrimEnd(export.Error.Item.ProgramError.KeyInfo) + " Act Ctrl No " +
            Substring
            (local.SpPrintWorkSet.Text15, SpPrintWorkSet.Text15_MaxLength,
            Verify(local.SpPrintWorkSet.Text15, "0"), 16 -
            Verify(local.SpPrintWorkSet.Text15, "0"));
          local.SpPrintWorkSet.Text15 =
            NumberToString(entities.ActivityDetail.SystemGeneratedIdentifier, 15);
            
          export.Error.Update.ProgramError.KeyInfo =
            TrimEnd(export.Error.Item.ProgramError.KeyInfo) + " Act Detail Sys Gen Id " +
            Substring
            (local.SpPrintWorkSet.Text15, SpPrintWorkSet.Text15_MaxLength,
            Verify(local.SpPrintWorkSet.Text15, "0"), 16 -
            Verify(local.SpPrintWorkSet.Text15, "0"));
        }
      }
      else if (AsChar(entities.ActivityStartStop.ActionCode) == 'E')
      {
        // --------------------------------------------------
        // Close Monitored Activities for "E"nding activities
        // --------------------------------------------------
        // ------------------------------------------------------------------------
        // added a READ EACH qualified by
        // Denorm_numeric_12, Case, Person, Case Unit, Activity
        // Control number.
        // Galka Jan 20, 2000
        // ------------------------------------------------------------------------
        // ------------------------------------------------------------------------
        // Changed READ EACH qualified by Case, Person, Case Unit,
        // Activity Control number, denorm_numeric 12 is less than or
        // equal to the input value and denorm_numeric_12 is greater
        // than or equal to the input value.  Therefore, it will produce
        // the same results as an equal test.   This was done to
        // influence DB2 to choose person,case, and case unit.
        // Galka Mar 22, 2000
        // ------------------------------------------------------------------------
        // -- Additional performance changes when finding monitored activities 
        // to close.
        local.OpenMona.Index = -1;
        local.MonaExpansion1.Index = -1;
        local.MonaExpansion2.Index = -1;
        local.MonaExpansion3.Index = -1;
        local.MonaExpansion4.Index = -1;
        local.MonaExpansion5.Index = -1;
        local.MonaExpansion6.Index = -1;
        local.OpenMona.Count = 0;
        local.MonaExpansion1.Count = 0;
        local.MonaExpansion2.Count = 0;
        local.MonaExpansion3.Count = 0;
        local.MonaExpansion4.Count = 0;
        local.MonaExpansion5.Count = 0;
        local.MonaExpansion6.Count = 0;

        if (!IsEmpty(import.Infrastructure.CaseNumber))
        {
          foreach(var item1 in ReadMonitoredActivityInfrastructure())
          {
            if (Equal(entities.Infrastructure.CaseNumber,
              import.Infrastructure.CaseNumber) && Equal
              (entities.Infrastructure.CaseUnitNumber,
              import.Infrastructure.CaseUnitNumber.GetValueOrDefault()) && Equal
              (entities.Infrastructure.CsePersonNumber,
              import.Infrastructure.CsePersonNumber) && Equal
              (entities.Infrastructure.DenormNumeric12,
              import.Infrastructure.DenormNumeric12.GetValueOrDefault()))
            {
              if (local.MonaExpansion1.Index + 1 < Local
                .MonaExpansion1Group.Capacity)
              {
                ++local.MonaExpansion1.Index;
                local.MonaExpansion1.CheckSize();

                local.MonaExpansion1.Update.GlocalMonaExpansion1.
                  SystemGeneratedIdentifier =
                    entities.MonitoredActivity.SystemGeneratedIdentifier;
              }

              continue;
            }

            if (Equal(import.Infrastructure.BusinessObjectCd, "LEA") && Equal
              (entities.Infrastructure.CaseNumber,
              import.Infrastructure.CaseNumber) && Equal
              (entities.Infrastructure.CsePersonNumber,
              import.Infrastructure.CsePersonNumber) && Equal
              (entities.Infrastructure.DenormNumeric12,
              import.Infrastructure.DenormNumeric12.GetValueOrDefault()))
            {
              if (local.MonaExpansion2.Index + 1 < Local
                .MonaExpansion2Group.Capacity)
              {
                ++local.MonaExpansion2.Index;
                local.MonaExpansion2.CheckSize();

                local.MonaExpansion2.Update.GlocalMonaExpansion2.
                  SystemGeneratedIdentifier =
                    entities.MonitoredActivity.SystemGeneratedIdentifier;
              }

              continue;
            }

            if (Equal(entities.Infrastructure.CaseNumber,
              import.Infrastructure.CaseNumber) && Equal
              (entities.Infrastructure.CaseUnitNumber,
              import.Infrastructure.CaseUnitNumber.GetValueOrDefault()) && Equal
              (entities.Infrastructure.CsePersonNumber,
              import.Infrastructure.CsePersonNumber))
            {
              if (local.MonaExpansion3.Index + 1 < Local
                .MonaExpansion3Group.Capacity)
              {
                ++local.MonaExpansion3.Index;
                local.MonaExpansion3.CheckSize();

                local.MonaExpansion3.Update.GlocalMonaExpansion3.
                  SystemGeneratedIdentifier =
                    entities.MonitoredActivity.SystemGeneratedIdentifier;
              }

              continue;
            }

            if (Equal(entities.Infrastructure.CaseNumber,
              import.Infrastructure.CaseNumber) && Equal
              (entities.Infrastructure.CaseUnitNumber,
              import.Infrastructure.CaseUnitNumber.GetValueOrDefault()))
            {
              if (local.MonaExpansion4.Index + 1 < Local
                .MonaExpansion4Group.Capacity)
              {
                ++local.MonaExpansion4.Index;
                local.MonaExpansion4.CheckSize();

                local.MonaExpansion4.Update.GlocalMonaExpansion4.
                  SystemGeneratedIdentifier =
                    entities.MonitoredActivity.SystemGeneratedIdentifier;
              }

              continue;
            }

            if (Equal(entities.Infrastructure.CaseNumber,
              import.Infrastructure.CaseNumber) && Equal
              (entities.Infrastructure.CsePersonNumber,
              import.Infrastructure.CsePersonNumber))
            {
              if (local.MonaExpansion5.Index + 1 < Local
                .MonaExpansion5Group.Capacity)
              {
                ++local.MonaExpansion5.Index;
                local.MonaExpansion5.CheckSize();

                local.MonaExpansion5.Update.GlocalMonaExpansion5.
                  SystemGeneratedIdentifier =
                    entities.MonitoredActivity.SystemGeneratedIdentifier;
              }

              continue;
            }

            if (Equal(entities.Infrastructure.CaseNumber,
              import.Infrastructure.CaseNumber))
            {
              if (local.MonaExpansion6.Index + 1 < Local
                .MonaExpansion6Group.Capacity)
              {
                ++local.MonaExpansion6.Index;
                local.MonaExpansion6.CheckSize();

                local.MonaExpansion6.Update.GlocalMonaExpansion6.
                  SystemGeneratedIdentifier =
                    entities.MonitoredActivity.SystemGeneratedIdentifier;
              }

              continue;
            }
          }

          if (local.MonaExpansion1.Count == 0)
          {
            // -- No monas found for this expansion.  Continue.
          }
          else
          {
            // -- One or more MONAs found for this expansion.
            for(local.MonaExpansion1.Index = 0; local.MonaExpansion1.Index < local
              .MonaExpansion1.Count; ++local.MonaExpansion1.Index)
            {
              if (!local.MonaExpansion1.CheckSize())
              {
                break;
              }

              if (local.OpenMona.Index + 1 < Local.OpenMonaGroup.Capacity)
              {
                ++local.OpenMona.Index;
                local.OpenMona.CheckSize();

                local.OpenMona.Update.GlocalOpen.SystemGeneratedIdentifier =
                  local.MonaExpansion1.Item.GlocalMonaExpansion1.
                    SystemGeneratedIdentifier;
              }
            }

            local.MonaExpansion1.CheckIndex();

            goto Test;
          }

          if (local.MonaExpansion2.Count == 0)
          {
            // -- No monas found for this expansion.  Continue.
          }
          else
          {
            // -- One or more MONAs found for this expansion.
            for(local.MonaExpansion2.Index = 0; local.MonaExpansion2.Index < local
              .MonaExpansion2.Count; ++local.MonaExpansion2.Index)
            {
              if (!local.MonaExpansion2.CheckSize())
              {
                break;
              }

              if (local.OpenMona.Index + 1 < Local.OpenMonaGroup.Capacity)
              {
                ++local.OpenMona.Index;
                local.OpenMona.CheckSize();

                local.OpenMona.Update.GlocalOpen.SystemGeneratedIdentifier =
                  local.MonaExpansion2.Item.GlocalMonaExpansion2.
                    SystemGeneratedIdentifier;
              }
            }

            local.MonaExpansion2.CheckIndex();

            goto Test;
          }

          if (local.MonaExpansion3.Count == 0)
          {
            // -- No monas found for this expansion.  Continue.
          }
          else
          {
            // -- One or more MONAs found for this expansion.
            for(local.MonaExpansion3.Index = 0; local.MonaExpansion3.Index < local
              .MonaExpansion3.Count; ++local.MonaExpansion3.Index)
            {
              if (!local.MonaExpansion3.CheckSize())
              {
                break;
              }

              if (local.OpenMona.Index + 1 < Local.OpenMonaGroup.Capacity)
              {
                ++local.OpenMona.Index;
                local.OpenMona.CheckSize();

                local.OpenMona.Update.GlocalOpen.SystemGeneratedIdentifier =
                  local.MonaExpansion3.Item.GlocalMonaExpansion3.
                    SystemGeneratedIdentifier;
              }
            }

            local.MonaExpansion3.CheckIndex();

            goto Test;
          }

          if (local.MonaExpansion4.Count == 0)
          {
            // -- No monas found for this expansion.  Continue.
          }
          else
          {
            // -- One or more MONAs found for this expansion.
            for(local.MonaExpansion4.Index = 0; local.MonaExpansion4.Index < local
              .MonaExpansion4.Count; ++local.MonaExpansion4.Index)
            {
              if (!local.MonaExpansion4.CheckSize())
              {
                break;
              }

              if (local.OpenMona.Index + 1 < Local.OpenMonaGroup.Capacity)
              {
                ++local.OpenMona.Index;
                local.OpenMona.CheckSize();

                local.OpenMona.Update.GlocalOpen.SystemGeneratedIdentifier =
                  local.MonaExpansion4.Item.GlocalMonaExpansion4.
                    SystemGeneratedIdentifier;
              }
            }

            local.MonaExpansion4.CheckIndex();

            goto Test;
          }

          if (local.MonaExpansion5.Count == 0)
          {
            // -- No monas found for this expansion.  Continue.
          }
          else
          {
            // -- One or more MONAs found for this expansion.
            for(local.MonaExpansion5.Index = 0; local.MonaExpansion5.Index < local
              .MonaExpansion5.Count; ++local.MonaExpansion5.Index)
            {
              if (!local.MonaExpansion5.CheckSize())
              {
                break;
              }

              if (local.OpenMona.Index + 1 < Local.OpenMonaGroup.Capacity)
              {
                ++local.OpenMona.Index;
                local.OpenMona.CheckSize();

                local.OpenMona.Update.GlocalOpen.SystemGeneratedIdentifier =
                  local.MonaExpansion5.Item.GlocalMonaExpansion5.
                    SystemGeneratedIdentifier;
              }
            }

            local.MonaExpansion5.CheckIndex();

            goto Test;
          }

          if (local.MonaExpansion6.Count == 0)
          {
            // -- No monas found for this expansion.  Continue.
          }
          else
          {
            // -- One or more MONAs found for this expansion.
            for(local.MonaExpansion6.Index = 0; local.MonaExpansion6.Index < local
              .MonaExpansion6.Count; ++local.MonaExpansion6.Index)
            {
              if (!local.MonaExpansion6.CheckSize())
              {
                break;
              }

              if (local.OpenMona.Index + 1 < Local.OpenMonaGroup.Capacity)
              {
                ++local.OpenMona.Index;
                local.OpenMona.CheckSize();

                local.OpenMona.Update.GlocalOpen.SystemGeneratedIdentifier =
                  local.MonaExpansion6.Item.GlocalMonaExpansion6.
                    SystemGeneratedIdentifier;
              }
            }

            local.MonaExpansion6.CheckIndex();

            goto Test;
          }
        }

        // ------------------------------------------------------------------------
        // Could not close using general objects. Try specific ones.
        // ------------------------------------------------------------------------
        switch(TrimEnd(import.Infrastructure.BusinessObjectCd))
        {
          case "CAS":
            // -- continue processing
            break;
          case "CAU":
            // -- continue processing
            break;
          case "DOC":
            // -- Beg PR # 80211
            // ------------------------------------------------------------------------
            //    4.2 Close Monitored Activities
            // ------------------------------------------------------------------------
            // ???  this looks exactly the same as the very first attempt to 
            // find a mona...
            // ------------------------------------------------------------------------
            // Count the number of open monitored activities using Case,
            // Case Unit, CSE Person and Activity Control number.
            // ------------------------------------------------------------------------
            foreach(var item1 in ReadMonitoredActivity1())
            {
              if (local.OpenMona.Index + 1 < Local.OpenMonaGroup.Capacity)
              {
                ++local.OpenMona.Index;
                local.OpenMona.CheckSize();

                local.OpenMona.Update.GlocalOpen.SystemGeneratedIdentifier =
                  entities.MonitoredActivity.SystemGeneratedIdentifier;
              }
            }

            // --------------
            // End PR # 80211
            // --------------
            break;
          case "LRF":
            if (import.Infrastructure.DenormNumeric12.GetValueOrDefault() != 0
              && !IsEmpty(import.Infrastructure.CaseNumber))
            {
              foreach(var item1 in ReadMonitoredActivity2())
              {
                if (local.OpenMona.Index + 1 < Local.OpenMonaGroup.Capacity)
                {
                  ++local.OpenMona.Index;
                  local.OpenMona.CheckSize();

                  local.OpenMona.Update.GlocalOpen.SystemGeneratedIdentifier =
                    entities.MonitoredActivity.SystemGeneratedIdentifier;
                }
              }
            }

            break;
          case "INR":
            if (import.Infrastructure.DenormNumeric12.GetValueOrDefault() != 0
              && !
              Equal(import.Infrastructure.DenormDate, local.Linitialized.Date))
            {
              foreach(var item1 in ReadMonitoredActivity5())
              {
                if (local.OpenMona.Index + 1 < Local.OpenMonaGroup.Capacity)
                {
                  ++local.OpenMona.Index;
                  local.OpenMona.CheckSize();

                  local.OpenMona.Update.GlocalOpen.SystemGeneratedIdentifier =
                    entities.MonitoredActivity.SystemGeneratedIdentifier;
                }
              }
            }

            break;
          case "LEA":
            // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
            // øHopefully, we will never need this piece of code since    ø
            // øthis kind of monitored activities should be closed using  ø
            // øCase and Case Unit.
            // 
            // ø
            // øIn this case, we would use Court Case # as the common     ø
            // øthread.
            // 
            // ø
            // þææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
            local.LegalAction.Identifier =
              (int)import.Infrastructure.DenormNumeric12.GetValueOrDefault();

            if (ReadLegalActionTribunal())
            {
              // 07/12/2007  GVandy  PR 313071  Correct performance problem when
              // closing LEA events.
              if (IsEmpty(entities.ExistingLegalAction.CourtCaseNumber))
              {
                // -- Don't want to read new legal actions below if the legal 
                // action has no court case number.
                // Otherwise, we would incorrectly read LOTS of non related 
                // legal actions which also do not have a court case number.
                // Escape and allow logic below to set exit state indicating no 
                // monitored activity was found to close.
                goto Test;
              }
              else
              {
                // -------------------
                // continue processing
                // -------------------
              }
            }
            else
            {
              export.Error.Index = export.Error.Count;
              export.Error.CheckSize();

              export.Error.Update.ProgramError.ProgramError1 =
                "Legal action not found.";
              export.Error.Update.ProgramError.KeyInfo =
                "MonA: Attempt to update for closure failed (w/ LEA).";
              local.SpPrintWorkSet.Text15 =
                NumberToString(entities.MonitoredActivity.
                  SystemGeneratedIdentifier, 15);
              export.Error.Update.ProgramError.KeyInfo =
                TrimEnd(export.Error.Item.ProgramError.KeyInfo) + " MONA Sys Gen Id " +
                Substring
                (local.SpPrintWorkSet.Text15, SpPrintWorkSet.Text15_MaxLength,
                Verify(local.SpPrintWorkSet.Text15, "0"), 16 -
                Verify(local.SpPrintWorkSet.Text15, "0"));

              continue;
            }

            // *******************************************************************
            // added local view of infrastructure denorm numeric 12, broke
            // up the join to read each legal action first moving the identifier
            // to the local of infrastructure denorm numeric using this in the
            // reach each of monitored activity. moved the infrastructure
            // identifier to a local view for the next read of monitored 
            // activity
            // making this one more effcient also.   this should solve the
            // problem of not using indexes for this read.
            // *******************************
            // Ketterhagen 23 DEC99
            // **************
            foreach(var item1 in ReadLegalAction2())
            {
              local.Infrastructure.DenormNumeric12 = entities.New1.Identifier;

              foreach(var item2 in ReadMonitoredActivity7())
              {
                if (local.OpenMona.Index + 1 < Local.OpenMonaGroup.Capacity)
                {
                  ++local.OpenMona.Index;
                  local.OpenMona.CheckSize();

                  local.OpenMona.Update.GlocalOpen.SystemGeneratedIdentifier =
                    entities.MonitoredActivity.SystemGeneratedIdentifier;
                }
              }
            }

            break;
          case "ADA":
            foreach(var item1 in ReadMonitoredActivity6())
            {
              if (local.OpenMona.Index + 1 < Local.OpenMonaGroup.Capacity)
              {
                ++local.OpenMona.Index;
                local.OpenMona.CheckSize();

                local.OpenMona.Update.GlocalOpen.SystemGeneratedIdentifier =
                  entities.MonitoredActivity.SystemGeneratedIdentifier;
              }
            }

            break;
          case "OBG":
            foreach(var item1 in ReadMonitoredActivity3())
            {
              if (local.OpenMona.Index + 1 < Local.OpenMonaGroup.Capacity)
              {
                ++local.OpenMona.Index;
                local.OpenMona.CheckSize();

                local.OpenMona.Update.GlocalOpen.SystemGeneratedIdentifier =
                  entities.MonitoredActivity.SystemGeneratedIdentifier;
              }
            }

            break;
          case "OAA":
            foreach(var item1 in ReadMonitoredActivity4())
            {
              if (local.OpenMona.Index + 1 < Local.OpenMonaGroup.Capacity)
              {
                ++local.OpenMona.Index;
                local.OpenMona.CheckSize();

                local.OpenMona.Update.GlocalOpen.SystemGeneratedIdentifier =
                  entities.MonitoredActivity.SystemGeneratedIdentifier;
              }
            }

            break;
          default:
            // -- continue
            break;
        }
      }

Test:

      if (AsChar(entities.ActivityStartStop.ActionCode) == 'E')
      {
        if (local.OpenMona.Count == 0)
        {
          // --------------------------------------
          // No monitored activities found to close
          // --------------------------------------
          export.Error.Index = export.Error.Count;
          export.Error.CheckSize();

          export.Error.Update.ProgramError.ProgramError1 =
            "SP0000: Unable to find a monitored activity to close.";
          local.SpPrintWorkSet.Text15 =
            NumberToString(entities.Activity.ControlNumber, 15);
          export.Error.Update.ProgramError.KeyInfo = "Activity Control No. " + Substring
            (local.SpPrintWorkSet.Text15, SpPrintWorkSet.Text15_MaxLength,
            Verify(local.SpPrintWorkSet.Text15, "0"), 16 -
            Verify(local.SpPrintWorkSet.Text15, "0"));
          local.SpPrintWorkSet.Text15 =
            NumberToString(entities.ActivityDetail.SystemGeneratedIdentifier, 15);
            
          export.Error.Update.ProgramError.KeyInfo =
            TrimEnd(export.Error.Item.ProgramError.KeyInfo) + " Act Detail Sys Gen Id " +
            Substring
            (local.SpPrintWorkSet.Text15, SpPrintWorkSet.Text15_MaxLength,
            Verify(local.SpPrintWorkSet.Text15, "0"), 16 -
            Verify(local.SpPrintWorkSet.Text15, "0"));
        }
        else
        {
          // -----------------------------------------------
          // One or more monitored activity found to close
          // -----------------------------------------------
          for(local.OpenMona.Index = 0; local.OpenMona.Index < local
            .OpenMona.Count; ++local.OpenMona.Index)
          {
            if (!local.OpenMona.CheckSize())
            {
              break;
            }

            if (export.ImportExportEndMona.Index + 1 < Export
              .ImportExportEndMonaGroup.Capacity)
            {
              export.ImportExportEndMona.Index =
                export.ImportExportEndMona.Count;
              export.ImportExportEndMona.CheckSize();

              export.ImportExportEndMona.Update.End.SystemGeneratedIdentifier =
                local.OpenMona.Item.GlocalOpen.SystemGeneratedIdentifier;
              export.ImportExportEndMona.Update.End.ClosureDate =
                import.Current.Date;
              export.ImportExportEndMona.Update.End.ClosureReasonCode = "SYS";
            }
          }

          local.OpenMona.CheckIndex();
        }
      }
    }

    // -- PR # 111742
    if (export.Error.Count > 0)
    {
      for(export.Error.Index = 0; export.Error.Index < export.Error.Count; ++
        export.Error.Index)
      {
        if (!export.Error.CheckSize())
        {
          break;
        }

        if (Equal(export.Error.Item.ProgramError.ProgramError1,
          "SP0000: Unable to find a monitored activity to close."))
        {
        }
        else
        {
          // -- There is some other error besides not being able to find a 
          // monitored activity to close.
          return;
        }
      }

      export.Error.CheckIndex();

      // -- If we reach this point it means the only error encountered was not 
      // being able to find a monitored activity to close.
      //    Set the last of export group error back to 0 so that the error will 
      // not be written to the error report.
      export.Error.Count = 0;
    }
  }

  private static void MoveActivityDetail(ActivityDetail source,
    ActivityDetail target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.RegulationSourceId = source.RegulationSourceId;
  }

  private static void MoveActivityDistributionRule(
    ActivityDistributionRule source, ActivityDistributionRule target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.BusinessObjectCode = source.BusinessObjectCode;
    target.CaseUnitFunction = source.CaseUnitFunction;
    target.ReasonCode = source.ReasonCode;
    target.ResponsibilityCode = source.ResponsibilityCode;
  }

  private static void MoveInfrastructure1(Infrastructure source,
    Infrastructure target)
  {
    target.EventId = source.EventId;
    target.ReasonCode = source.ReasonCode;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormTimestamp = source.DenormTimestamp;
    target.ReferenceDate = source.ReferenceDate;
  }

  private static void MoveInfrastructure2(Infrastructure source,
    Infrastructure target)
  {
    target.DenormNumeric12 = source.DenormNumeric12;
    target.CaseNumber = source.CaseNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
  }

  private static void MoveMonitoredActivity1(MonitoredActivity source,
    MonitoredActivity target)
  {
    target.ActivityControlNumber = source.ActivityControlNumber;
    target.FedNonComplianceDate = source.FedNonComplianceDate;
    target.FedNearNonComplDate = source.FedNearNonComplDate;
    target.OtherNonComplianceDate = source.OtherNonComplianceDate;
    target.OtherNearNonComplDate = source.OtherNearNonComplDate;
  }

  private static void MoveMonitoredActivity2(MonitoredActivity source,
    MonitoredActivity target)
  {
    target.FedNonComplianceDate = source.FedNonComplianceDate;
    target.FedNearNonComplDate = source.FedNearNonComplDate;
    target.OtherNonComplianceDate = source.OtherNonComplianceDate;
    target.OtherNearNonComplDate = source.OtherNearNonComplDate;
    target.ClosureDate = source.ClosureDate;
  }

  private static void MoveMonitoredActivityAssignment(
    MonitoredActivityAssignment source, MonitoredActivityAssignment target)
  {
    target.ReasonCode = source.ReasonCode;
    target.ResponsibilityCode = source.ResponsibilityCode;
    target.EffectiveDate = source.EffectiveDate;
    target.OverrideInd = source.OverrideInd;
    target.DiscontinueDate = source.DiscontinueDate;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveOfficeServiceProvider(OfficeServiceProvider source,
    OfficeServiceProvider target)
  {
    target.RoleCode = source.RoleCode;
    target.EffectiveDate = source.EffectiveDate;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.Max.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseSpEstabLegalMonaComplDates()
  {
    var useImport = new SpEstabLegalMonaComplDates.Import();
    var useExport = new SpEstabLegalMonaComplDates.Export();

    useImport.Activity.ControlNumber = entities.Activity.ControlNumber;
    useImport.ActivityDetail.Assign(entities.ActivityDetail);
    MoveInfrastructure1(import.Infrastructure, useImport.Infrastructure);
    useImport.EventDetail.SystemGeneratedIdentifier =
      import.EventDetail.SystemGeneratedIdentifier;
    MoveMonitoredActivity1(local.MonitoredActivity, useImport.MonitoredActivity);
      

    Call(SpEstabLegalMonaComplDates.Execute, useImport, useExport);

    MoveMonitoredActivity2(useExport.MonitoredActivity, local.MonitoredActivity);
      
  }

  private void UseSpEventsToBypassErrorReport()
  {
    var useImport = new SpEventsToBypassErrorReport.Import();
    var useExport = new SpEventsToBypassErrorReport.Export();

    useImport.Activity.ControlNumber = entities.Activity.ControlNumber;
    MoveActivityDistributionRule(entities.ActivityDistributionRule,
      useImport.ActivityDistributionRule);
    MoveActivityDetail(entities.ActivityDetail, useImport.ActivityDetail);
    useImport.Event1.BusinessObjectCode = import.Event1.BusinessObjectCode;
    MoveInfrastructure2(import.Infrastructure, useImport.Infrastructure);
    useImport.Current.Date = import.Current.Date;

    Call(SpEventsToBypassErrorReport.Execute, useImport, useExport);

    export.BypassErrorReport.Flag = useExport.BypassErrorReport.Flag;
  }

  private IEnumerable<bool> ReadActivityDistributionRule()
  {
    System.Diagnostics.Debug.Assert(entities.ActivityDetail.Populated);
    entities.ActivityDistributionRule.Populated = false;

    return ReadEach("ReadActivityDistributionRule",
      (db, command) =>
      {
        db.SetInt32(command, "actControlNo", entities.ActivityDetail.ActNo);
        db.SetInt32(
          command, "acdId", entities.ActivityDetail.SystemGeneratedIdentifier);
        db.SetDate(
          command, "effectiveDate", import.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ActivityDistributionRule.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ActivityDistributionRule.BusinessObjectCode =
          db.GetString(reader, 1);
        entities.ActivityDistributionRule.CaseUnitFunction =
          db.GetNullableString(reader, 2);
        entities.ActivityDistributionRule.ReasonCode = db.GetString(reader, 3);
        entities.ActivityDistributionRule.ResponsibilityCode =
          db.GetString(reader, 4);
        entities.ActivityDistributionRule.CaseRoleCode =
          db.GetNullableString(reader, 5);
        entities.ActivityDistributionRule.CsePersonAcctCode =
          db.GetNullableString(reader, 6);
        entities.ActivityDistributionRule.CsenetRoleCode =
          db.GetNullableString(reader, 7);
        entities.ActivityDistributionRule.LaPersonCode =
          db.GetNullableString(reader, 8);
        entities.ActivityDistributionRule.EffectiveDate = db.GetDate(reader, 9);
        entities.ActivityDistributionRule.DiscontinueDate =
          db.GetDate(reader, 10);
        entities.ActivityDistributionRule.ActControlNo =
          db.GetInt32(reader, 11);
        entities.ActivityDistributionRule.AcdId = db.GetInt32(reader, 12);
        entities.ActivityDistributionRule.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadActivityStartStopActivityDetailActivity()
  {
    entities.Activity.Populated = false;
    entities.ActivityDetail.Populated = false;
    entities.ActivityStartStop.Populated = false;

    return ReadEach("ReadActivityStartStopActivityDetailActivity",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", import.Current.Date.GetValueOrDefault());
        db.SetInt32(
          command, "evdId", import.EventDetail.SystemGeneratedIdentifier);
        db.SetInt32(command, "eveNo", import.Event1.ControlNumber);
      },
      (db, reader) =>
      {
        entities.ActivityStartStop.ActionCode = db.GetString(reader, 0);
        entities.ActivityStartStop.EffectiveDate = db.GetDate(reader, 1);
        entities.ActivityStartStop.DiscontinueDate = db.GetDate(reader, 2);
        entities.ActivityStartStop.ActNo = db.GetInt32(reader, 3);
        entities.ActivityDetail.ActNo = db.GetInt32(reader, 3);
        entities.Activity.ControlNumber = db.GetInt32(reader, 3);
        entities.ActivityStartStop.AcdId = db.GetInt32(reader, 4);
        entities.ActivityDetail.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.ActivityStartStop.EveNo = db.GetInt32(reader, 5);
        entities.ActivityStartStop.EvdId = db.GetInt32(reader, 6);
        entities.ActivityDetail.FedNonComplianceDays =
          db.GetNullableInt32(reader, 7);
        entities.ActivityDetail.FedNearNonComplDays =
          db.GetNullableInt32(reader, 8);
        entities.ActivityDetail.OtherNonComplianceDays =
          db.GetNullableInt32(reader, 9);
        entities.ActivityDetail.OtherNearNonComplDays =
          db.GetNullableInt32(reader, 10);
        entities.ActivityDetail.RegulationSourceId =
          db.GetNullableString(reader, 11);
        entities.ActivityDetail.EffectiveDate = db.GetDate(reader, 12);
        entities.ActivityDetail.DiscontinueDate = db.GetDate(reader, 13);
        entities.Activity.Name = db.GetString(reader, 14);
        entities.Activity.TypeCode = db.GetString(reader, 15);
        entities.Activity.Populated = true;
        entities.ActivityDetail.Populated = true;
        entities.ActivityStartStop.Populated = true;

        return true;
      });
  }

  private bool ReadAdministrativeAppeal()
  {
    entities.AdministrativeAppeal.Populated = false;

    return Read("ReadAdministrativeAppeal",
      (db, command) =>
      {
        db.SetInt32(
          command, "adminAppealId", local.AdministrativeAppeal.Identifier);
        db.SetNullableString(
          command, "cspNumber", import.Infrastructure.CsePersonNumber ?? "");
        db.SetNullableString(
          command, "csePersonAcctCode",
          entities.ActivityDistributionRule.CsePersonAcctCode ?? "");
      },
      (db, reader) =>
      {
        entities.AdministrativeAppeal.Identifier = db.GetInt32(reader, 0);
        entities.AdministrativeAppeal.Number = db.GetNullableString(reader, 1);
        entities.AdministrativeAppeal.AatType = db.GetNullableString(reader, 2);
        entities.AdministrativeAppeal.ObgGeneratedId =
          db.GetNullableInt32(reader, 3);
        entities.AdministrativeAppeal.CspNumber =
          db.GetNullableString(reader, 4);
        entities.AdministrativeAppeal.CpaType = db.GetNullableString(reader, 5);
        entities.AdministrativeAppeal.OaaTakenDate =
          db.GetNullableDate(reader, 6);
        entities.AdministrativeAppeal.OtyId = db.GetNullableInt32(reader, 7);
        entities.AdministrativeAppeal.Populated = true;
        CheckValid<AdministrativeAppeal>("CpaType",
          entities.AdministrativeAppeal.CpaType);
      });
  }

  private bool ReadCase()
  {
    entities.ClosedCaseCheck.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Infrastructure.CaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.ClosedCaseCheck.Number = db.GetString(reader, 0);
        entities.ClosedCaseCheck.Status = db.GetNullableString(reader, 1);
        entities.ClosedCaseCheck.Populated = true;
      });
  }

  private bool ReadCaseRole()
  {
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRole",
      (db, command) =>
      {
        db.SetString(
          command, "casNumber", import.Infrastructure.CaseNumber ?? "");
        db.SetString(
          command, "cspNumber", import.Infrastructure.CsePersonNumber ?? "");
        db.SetString(
          command, "type", entities.ActivityDistributionRule.CaseRoleCode ?? ""
          );
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private bool ReadLegalAction1()
  {
    entities.ExistingLegalAction.Populated = false;

    return Read("ReadLegalAction1",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", local.LegalAction.Identifier);
        db.SetNullableString(
          command, "cspNumber", import.Infrastructure.CsePersonNumber ?? "");
        db.SetNullableString(
          command, "laPersonCode",
          entities.ActivityDistributionRule.LaPersonCode ?? "");
        db.SetDate(
          command, "effectiveDt", import.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 1);
        entities.ExistingLegalAction.TrbId = db.GetNullableInt32(reader, 2);
        entities.ExistingLegalAction.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalAction2()
  {
    entities.New1.Populated = false;

    return ReadEach("ReadLegalAction2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo",
          entities.ExistingLegalAction.CourtCaseNumber ?? "");
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.New1.Identifier = db.GetInt32(reader, 0);
        entities.New1.CourtCaseNumber = db.GetNullableString(reader, 1);
        entities.New1.TrbId = db.GetNullableInt32(reader, 2);
        entities.New1.Populated = true;

        return true;
      });
  }

  private bool ReadLegalActionTribunal()
  {
    entities.ExistingLegalAction.Populated = false;
    entities.Tribunal.Populated = false;

    return Read("ReadLegalActionTribunal",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", local.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 1);
        entities.ExistingLegalAction.TrbId = db.GetNullableInt32(reader, 2);
        entities.Tribunal.Identifier = db.GetInt32(reader, 2);
        entities.ExistingLegalAction.Populated = true;
        entities.Tribunal.Populated = true;
      });
  }

  private IEnumerable<bool> ReadMonitoredActivity1()
  {
    entities.MonitoredActivity.Populated = false;

    return ReadEach("ReadMonitoredActivity1",
      (db, command) =>
      {
        db.
          SetInt32(command, "activityCtrlNum", entities.Activity.ControlNumber);
          
        db.SetNullableDate(
          command, "closureDate", local.Max.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "caseNumber", import.Infrastructure.CaseNumber ?? "");
        db.SetNullableInt32(
          command, "caseUnitNum",
          import.Infrastructure.CaseUnitNumber.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum", import.Infrastructure.CsePersonNumber ?? ""
          );
        db.SetNullableInt64(
          command, "denormNumeric12",
          import.Infrastructure.DenormNumeric12.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.MonitoredActivity.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.MonitoredActivity.ActivityControlNumber =
          db.GetInt32(reader, 1);
        entities.MonitoredActivity.ClosureDate = db.GetNullableDate(reader, 2);
        entities.MonitoredActivity.InfSysGenId = db.GetNullableInt32(reader, 3);
        entities.MonitoredActivity.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadMonitoredActivity2()
  {
    entities.MonitoredActivity.Populated = false;

    return ReadEach("ReadMonitoredActivity2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "caseNumber", import.Infrastructure.CaseNumber ?? "");
        db.SetNullableInt64(
          command, "denormNumeric12",
          import.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetInt32(command, "controlNumber", entities.Activity.ControlNumber);
        db.SetNullableDate(
          command, "closureDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.MonitoredActivity.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.MonitoredActivity.ActivityControlNumber =
          db.GetInt32(reader, 1);
        entities.MonitoredActivity.ClosureDate = db.GetNullableDate(reader, 2);
        entities.MonitoredActivity.InfSysGenId = db.GetNullableInt32(reader, 3);
        entities.MonitoredActivity.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadMonitoredActivity3()
  {
    entities.MonitoredActivity.Populated = false;

    return ReadEach("ReadMonitoredActivity3",
      (db, command) =>
      {
        db.
          SetInt32(command, "activityCtrlNum", entities.Activity.ControlNumber);
          
        db.SetNullableDate(
          command, "closureDate", local.Max.Date.GetValueOrDefault());
        db.SetInt32(
          command, "systemGeneratedI",
          import.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.MonitoredActivity.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.MonitoredActivity.ActivityControlNumber =
          db.GetInt32(reader, 1);
        entities.MonitoredActivity.ClosureDate = db.GetNullableDate(reader, 2);
        entities.MonitoredActivity.InfSysGenId = db.GetNullableInt32(reader, 3);
        entities.MonitoredActivity.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadMonitoredActivity4()
  {
    entities.MonitoredActivity.Populated = false;

    return ReadEach("ReadMonitoredActivity4",
      (db, command) =>
      {
        db.
          SetInt32(command, "activityCtrlNum", entities.Activity.ControlNumber);
          
        db.SetNullableDate(
          command, "closureDate", local.Max.Date.GetValueOrDefault());
        db.SetInt32(
          command, "systemGeneratedI",
          import.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.MonitoredActivity.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.MonitoredActivity.ActivityControlNumber =
          db.GetInt32(reader, 1);
        entities.MonitoredActivity.ClosureDate = db.GetNullableDate(reader, 2);
        entities.MonitoredActivity.InfSysGenId = db.GetNullableInt32(reader, 3);
        entities.MonitoredActivity.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadMonitoredActivity5()
  {
    entities.MonitoredActivity.Populated = false;

    return ReadEach("ReadMonitoredActivity5",
      (db, command) =>
      {
        db.SetNullableInt64(
          command, "denormNumeric12",
          import.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableDate(
          command, "denormDate",
          import.Infrastructure.DenormDate.GetValueOrDefault());
        db.
          SetInt32(command, "activityCtrlNum", entities.Activity.ControlNumber);
          
        db.SetNullableDate(
          command, "closureDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.MonitoredActivity.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.MonitoredActivity.ActivityControlNumber =
          db.GetInt32(reader, 1);
        entities.MonitoredActivity.ClosureDate = db.GetNullableDate(reader, 2);
        entities.MonitoredActivity.InfSysGenId = db.GetNullableInt32(reader, 3);
        entities.MonitoredActivity.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadMonitoredActivity6()
  {
    entities.MonitoredActivity.Populated = false;

    return ReadEach("ReadMonitoredActivity6",
      (db, command) =>
      {
        db.
          SetInt32(command, "activityCtrlNum", entities.Activity.ControlNumber);
          
        db.SetNullableInt64(
          command, "denormNumeric12",
          import.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetNullableDate(
          command, "closureDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.MonitoredActivity.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.MonitoredActivity.ActivityControlNumber =
          db.GetInt32(reader, 1);
        entities.MonitoredActivity.ClosureDate = db.GetNullableDate(reader, 2);
        entities.MonitoredActivity.InfSysGenId = db.GetNullableInt32(reader, 3);
        entities.MonitoredActivity.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadMonitoredActivity7()
  {
    entities.MonitoredActivity.Populated = false;

    return ReadEach("ReadMonitoredActivity7",
      (db, command) =>
      {
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetInt32(command, "controlNumber", entities.Activity.ControlNumber);
        db.SetNullableDate(
          command, "closureDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.MonitoredActivity.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.MonitoredActivity.ActivityControlNumber =
          db.GetInt32(reader, 1);
        entities.MonitoredActivity.ClosureDate = db.GetNullableDate(reader, 2);
        entities.MonitoredActivity.InfSysGenId = db.GetNullableInt32(reader, 3);
        entities.MonitoredActivity.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadMonitoredActivityInfrastructure()
  {
    entities.MonitoredActivity.Populated = false;
    entities.Infrastructure.Populated = false;

    return ReadEach("ReadMonitoredActivityInfrastructure",
      (db, command) =>
      {
        db.SetNullableString(
          command, "caseNumber", import.Infrastructure.CaseNumber ?? "");
        db.SetInt32(command, "controlNumber", entities.Activity.ControlNumber);
        db.SetNullableDate(
          command, "closureDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.MonitoredActivity.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.MonitoredActivity.ActivityControlNumber =
          db.GetInt32(reader, 1);
        entities.MonitoredActivity.ClosureDate = db.GetNullableDate(reader, 2);
        entities.MonitoredActivity.InfSysGenId = db.GetNullableInt32(reader, 3);
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 4);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 5);
        entities.Infrastructure.DenormDate = db.GetNullableDate(reader, 6);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 7);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 8);
        entities.Infrastructure.CaseUnitNumber = db.GetNullableInt32(reader, 9);
        entities.Infrastructure.OtyOaaId = db.GetNullableInt32(reader, 10);
        entities.Infrastructure.AatType = db.GetNullableString(reader, 11);
        entities.Infrastructure.ObgOaaId = db.GetNullableInt32(reader, 12);
        entities.Infrastructure.CspOaaNo = db.GetNullableString(reader, 13);
        entities.Infrastructure.CpaOaaType = db.GetNullableString(reader, 14);
        entities.Infrastructure.OaaDate = db.GetNullableDate(reader, 15);
        entities.Infrastructure.OtyId = db.GetNullableInt32(reader, 16);
        entities.Infrastructure.CpaType = db.GetNullableString(reader, 17);
        entities.Infrastructure.CspNo = db.GetNullableString(reader, 18);
        entities.Infrastructure.ObgId = db.GetNullableInt32(reader, 19);
        entities.MonitoredActivity.Populated = true;
        entities.Infrastructure.Populated = true;
        CheckValid<Infrastructure>("CpaOaaType",
          entities.Infrastructure.CpaOaaType);
        CheckValid<Infrastructure>("CpaType", entities.Infrastructure.CpaType);

        return true;
      });
  }

  private bool ReadObligation()
  {
    entities.Obligation.Populated = false;

    return Read("ReadObligation",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          import.Infrastructure.SystemGeneratedIdentifier);
        db.SetString(
          command, "cspNumber", import.Infrastructure.CsePersonNumber ?? "");
        db.SetNullableString(
          command, "csePersonAcctCode",
          entities.ActivityDistributionRule.CsePersonAcctCode ?? "");
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
      });
  }

  private bool ReadObligationAdministrativeAction()
  {
    entities.ObligationAdministrativeAction.Populated = false;

    return Read("ReadObligationAdministrativeAction",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          import.Infrastructure.SystemGeneratedIdentifier);
        db.SetString(
          command, "cspNumber", import.Infrastructure.CsePersonNumber ?? "");
        db.SetNullableString(
          command, "csePersonAcctCode",
          entities.ActivityDistributionRule.CsePersonAcctCode ?? "");
      },
      (db, reader) =>
      {
        entities.ObligationAdministrativeAction.OtyType =
          db.GetInt32(reader, 0);
        entities.ObligationAdministrativeAction.AatType =
          db.GetString(reader, 1);
        entities.ObligationAdministrativeAction.ObgGeneratedId =
          db.GetInt32(reader, 2);
        entities.ObligationAdministrativeAction.CspNumber =
          db.GetString(reader, 3);
        entities.ObligationAdministrativeAction.CpaType =
          db.GetString(reader, 4);
        entities.ObligationAdministrativeAction.TakenDate =
          db.GetDate(reader, 5);
        entities.ObligationAdministrativeAction.Populated = true;
        CheckValid<ObligationAdministrativeAction>("CpaType",
          entities.ObligationAdministrativeAction.CpaType);
      });
  }

  private bool ReadOfficeServiceProviderOfficeServiceProvider1()
  {
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProviderOfficeServiceProvider1",
      (db, command) =>
      {
        db.SetString(command, "userId", import.Infrastructure.UserId);
        db.SetNullableDate(
          command, "discontinueDate", import.Current.Date.GetValueOrDefault());
        db.SetInt32(
          command, "infId", import.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 5);
        entities.ServiceProvider.UserId = db.GetString(reader, 6);
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;
        entities.ServiceProvider.Populated = true;
      });
  }

  private IEnumerable<bool> ReadOfficeServiceProviderOfficeServiceProvider10()
  {
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;

    return ReadEach("ReadOfficeServiceProviderOfficeServiceProvider10",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", import.Current.Date.GetValueOrDefault());
        db.SetString(
          command, "reasonCode", entities.ActivityDistributionRule.ReasonCode);
        db.SetInt32(
          command, "systemGeneratedI",
          import.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 5);
        entities.ServiceProvider.UserId = db.GetString(reader, 6);
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;
        entities.ServiceProvider.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOfficeServiceProviderOfficeServiceProvider11()
  {
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;

    return ReadEach("ReadOfficeServiceProviderOfficeServiceProvider11",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", import.Current.Date.GetValueOrDefault());
        db.SetString(
          command, "reasonCode", entities.ActivityDistributionRule.ReasonCode);
        db.SetInt32(
          command, "adminAppealId", local.AdministrativeAppeal.Identifier);
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 5);
        entities.ServiceProvider.UserId = db.GetString(reader, 6);
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;
        entities.ServiceProvider.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOfficeServiceProviderOfficeServiceProvider12()
  {
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;

    return ReadEach("ReadOfficeServiceProviderOfficeServiceProvider12",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", import.Current.Date.GetValueOrDefault());
        db.SetString(
          command, "reasonCode", entities.ActivityDistributionRule.ReasonCode);
        db.SetNullableInt32(command, "lgaId", local.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 5);
        entities.ServiceProvider.UserId = db.GetString(reader, 6);
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;
        entities.ServiceProvider.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOfficeServiceProviderOfficeServiceProvider13()
  {
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;

    return ReadEach("ReadOfficeServiceProviderOfficeServiceProvider13",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", import.Current.Date.GetValueOrDefault());
        db.SetString(
          command, "reasonCode", entities.ActivityDistributionRule.ReasonCode);
        db.SetInt32(
          command, "systemGeneratedI",
          import.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 5);
        entities.ServiceProvider.UserId = db.GetString(reader, 6);
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;
        entities.ServiceProvider.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOfficeServiceProviderOfficeServiceProvider14()
  {
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;

    return ReadEach("ReadOfficeServiceProviderOfficeServiceProvider14",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDt", import.Current.Date.GetValueOrDefault());
        db.SetString(
          command, "reasonCode", entities.ActivityDistributionRule.ReasonCode);
        db.SetNullableInt32(
          command, "lgaIdentifier", local.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 5);
        entities.ServiceProvider.UserId = db.GetString(reader, 6);
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;
        entities.ServiceProvider.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOfficeServiceProviderOfficeServiceProvider15()
  {
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;

    return ReadEach("ReadOfficeServiceProviderOfficeServiceProvider15",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", import.Current.Date.GetValueOrDefault());
        db.SetString(
          command, "reasonCode", entities.ActivityDistributionRule.ReasonCode);
        db.SetInt32(command, "aapId", local.AdministrativeAppeal.Identifier);
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 5);
        entities.ServiceProvider.UserId = db.GetString(reader, 6);
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;
        entities.ServiceProvider.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOfficeServiceProviderOfficeServiceProvider16()
  {
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;

    return ReadEach("ReadOfficeServiceProviderOfficeServiceProvider16",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", import.Current.Date.GetValueOrDefault());
        db.SetString(
          command, "reasonCode", entities.ActivityDistributionRule.ReasonCode);
        db.SetString(command, "casNo", import.Infrastructure.CaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 5);
        entities.ServiceProvider.UserId = db.GetString(reader, 6);
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;
        entities.ServiceProvider.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOfficeServiceProviderOfficeServiceProvider2()
  {
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;

    return ReadEach("ReadOfficeServiceProviderOfficeServiceProvider2",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDt", import.Current.Date.GetValueOrDefault());
        db.SetString(
          command, "reasonCode", entities.ActivityDistributionRule.ReasonCode);
        db.SetInt32(
          command, "systemGeneratedI",
          import.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 5);
        entities.ServiceProvider.UserId = db.GetString(reader, 6);
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;
        entities.ServiceProvider.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOfficeServiceProviderOfficeServiceProvider3()
  {
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;

    return ReadEach("ReadOfficeServiceProviderOfficeServiceProvider3",
      (db, command) =>
      {
        db.SetString(
          command, "function",
          entities.ActivityDistributionRule.CaseUnitFunction ?? "");
        db.SetDate(
          command, "effectiveDate", import.Current.Date.GetValueOrDefault());
        db.SetString(
          command, "reasonCode", entities.ActivityDistributionRule.ReasonCode);
        db.SetInt32(
          command, "csuNo",
          import.Infrastructure.CaseUnitNumber.GetValueOrDefault());
        db.SetString(command, "casNo", import.Infrastructure.CaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 5);
        entities.ServiceProvider.UserId = db.GetString(reader, 6);
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;
        entities.ServiceProvider.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOfficeServiceProviderOfficeServiceProvider4()
  {
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;

    return ReadEach("ReadOfficeServiceProviderOfficeServiceProvider4",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDt", import.Current.Date.GetValueOrDefault());
        db.SetString(
          command, "reasonCode", entities.ActivityDistributionRule.ReasonCode);
        db.SetInt32(
          command, "systemGeneratedI",
          import.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 5);
        entities.ServiceProvider.UserId = db.GetString(reader, 6);
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;
        entities.ServiceProvider.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOfficeServiceProviderOfficeServiceProvider5()
  {
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;

    return ReadEach("ReadOfficeServiceProviderOfficeServiceProvider5",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", import.Current.Date.GetValueOrDefault());
        db.SetString(
          command, "reasonCode", entities.ActivityDistributionRule.ReasonCode);
        db.SetInt32(
          command, "systemGeneratedI",
          import.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 5);
        entities.ServiceProvider.UserId = db.GetString(reader, 6);
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;
        entities.ServiceProvider.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOfficeServiceProviderOfficeServiceProvider6()
  {
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;

    return ReadEach("ReadOfficeServiceProviderOfficeServiceProvider6",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", import.Current.Date.GetValueOrDefault());
        db.SetString(
          command, "reasonCode", entities.ActivityDistributionRule.ReasonCode);
        db.SetInt32(
          command, "systemGeneratedI",
          import.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 5);
        entities.ServiceProvider.UserId = db.GetString(reader, 6);
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;
        entities.ServiceProvider.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOfficeServiceProviderOfficeServiceProvider7()
  {
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;

    return ReadEach("ReadOfficeServiceProviderOfficeServiceProvider7",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", import.Current.Date.GetValueOrDefault());
        db.SetString(
          command, "reasonCode", entities.ActivityDistributionRule.ReasonCode);
        db.SetInt64(
          command, "icsNo",
          import.Infrastructure.DenormNumeric12.GetValueOrDefault());
        db.SetDate(
          command, "icsDate",
          import.Infrastructure.DenormDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 5);
        entities.ServiceProvider.UserId = db.GetString(reader, 6);
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;
        entities.ServiceProvider.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOfficeServiceProviderOfficeServiceProvider8()
  {
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;

    return ReadEach("ReadOfficeServiceProviderOfficeServiceProvider8",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", import.Current.Date.GetValueOrDefault());
        db.SetInt32(command, "lgrId", local.LegalReferral.Identifier);
        db.SetString(command, "casNo", import.Infrastructure.CaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 5);
        entities.ServiceProvider.UserId = db.GetString(reader, 6);
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;
        entities.ServiceProvider.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOfficeServiceProviderOfficeServiceProvider9()
  {
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;

    return ReadEach("ReadOfficeServiceProviderOfficeServiceProvider9",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", import.Current.Date.GetValueOrDefault());
        db.SetString(
          command, "casNumber", import.Infrastructure.CaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 5);
        entities.ServiceProvider.UserId = db.GetString(reader, 6);
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;
        entities.ServiceProvider.Populated = true;

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
    /// A value of Event1.
    /// </summary>
    [JsonPropertyName("event1")]
    public Event1 Event1
    {
      get => event1 ??= new();
      set => event1 = value;
    }

    private DateWorkArea current;
    private Infrastructure infrastructure;
    private EventDetail eventDetail;
    private Event1 event1;
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
      /// A value of ProgramError.
      /// </summary>
      [JsonPropertyName("programError")]
      public ProgramError ProgramError
      {
        get => programError ??= new();
        set => programError = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private ProgramError programError;
    }

    /// <summary>A ImportExportStartMonaGroup group.</summary>
    [Serializable]
    public class ImportExportStartMonaGroup
    {
      /// <summary>
      /// A value of StartMona.
      /// </summary>
      [JsonPropertyName("startMona")]
      public MonitoredActivity StartMona
      {
        get => startMona ??= new();
        set => startMona = value;
      }

      /// <summary>
      /// Gets a value of StartMonaAssgnmnt.
      /// </summary>
      [JsonIgnore]
      public Array<StartMonaAssgnmntGroup> StartMonaAssgnmnt =>
        startMonaAssgnmnt ??= new(StartMonaAssgnmntGroup.Capacity, 0);

      /// <summary>
      /// Gets a value of StartMonaAssgnmnt for json serialization.
      /// </summary>
      [JsonPropertyName("startMonaAssgnmnt")]
      [Computed]
      public IList<StartMonaAssgnmntGroup> StartMonaAssgnmnt_Json
      {
        get => startMonaAssgnmnt;
        set => StartMonaAssgnmnt.Assign(value);
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private MonitoredActivity startMona;
      private Array<StartMonaAssgnmntGroup> startMonaAssgnmnt;
    }

    /// <summary>A StartMonaAssgnmntGroup group.</summary>
    [Serializable]
    public class StartMonaAssgnmntGroup
    {
      /// <summary>
      /// A value of StartMonaMonitoredActivityAssignment.
      /// </summary>
      [JsonPropertyName("startMonaMonitoredActivityAssignment")]
      public MonitoredActivityAssignment StartMonaMonitoredActivityAssignment
      {
        get => startMonaMonitoredActivityAssignment ??= new();
        set => startMonaMonitoredActivityAssignment = value;
      }

      /// <summary>
      /// A value of StartMonaOfficeServiceProvider.
      /// </summary>
      [JsonPropertyName("startMonaOfficeServiceProvider")]
      public OfficeServiceProvider StartMonaOfficeServiceProvider
      {
        get => startMonaOfficeServiceProvider ??= new();
        set => startMonaOfficeServiceProvider = value;
      }

      /// <summary>
      /// A value of StartMonaServiceProvider.
      /// </summary>
      [JsonPropertyName("startMonaServiceProvider")]
      public ServiceProvider StartMonaServiceProvider
      {
        get => startMonaServiceProvider ??= new();
        set => startMonaServiceProvider = value;
      }

      /// <summary>
      /// A value of StartMonaOffice.
      /// </summary>
      [JsonPropertyName("startMonaOffice")]
      public Office StartMonaOffice
      {
        get => startMonaOffice ??= new();
        set => startMonaOffice = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 15;

      private MonitoredActivityAssignment startMonaMonitoredActivityAssignment;
      private OfficeServiceProvider startMonaOfficeServiceProvider;
      private ServiceProvider startMonaServiceProvider;
      private Office startMonaOffice;
    }

    /// <summary>A ImportExportEndMonaGroup group.</summary>
    [Serializable]
    public class ImportExportEndMonaGroup
    {
      /// <summary>
      /// A value of End.
      /// </summary>
      [JsonPropertyName("end")]
      public MonitoredActivity End
      {
        get => end ??= new();
        set => end = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 750;

      private MonitoredActivity end;
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
    /// Gets a value of ImportExportStartMona.
    /// </summary>
    [JsonIgnore]
    public Array<ImportExportStartMonaGroup> ImportExportStartMona =>
      importExportStartMona ??= new(ImportExportStartMonaGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ImportExportStartMona for json serialization.
    /// </summary>
    [JsonPropertyName("importExportStartMona")]
    [Computed]
    public IList<ImportExportStartMonaGroup> ImportExportStartMona_Json
    {
      get => importExportStartMona;
      set => ImportExportStartMona.Assign(value);
    }

    /// <summary>
    /// A value of BypassErrorReport.
    /// </summary>
    [JsonPropertyName("bypassErrorReport")]
    public Common BypassErrorReport
    {
      get => bypassErrorReport ??= new();
      set => bypassErrorReport = value;
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
    /// Gets a value of ImportExportEndMona.
    /// </summary>
    [JsonIgnore]
    public Array<ImportExportEndMonaGroup> ImportExportEndMona =>
      importExportEndMona ??= new(ImportExportEndMonaGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ImportExportEndMona for json serialization.
    /// </summary>
    [JsonPropertyName("importExportEndMona")]
    [Computed]
    public IList<ImportExportEndMonaGroup> ImportExportEndMona_Json
    {
      get => importExportEndMona;
      set => ImportExportEndMona.Assign(value);
    }

    private Array<ErrorGroup> error;
    private Array<ImportExportStartMonaGroup> importExportStartMona;
    private Common bypassErrorReport;
    private Infrastructure infrastructure;
    private Array<ImportExportEndMonaGroup> importExportEndMona;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A OpenMonaGroup group.</summary>
    [Serializable]
    public class OpenMonaGroup
    {
      /// <summary>
      /// A value of GlocalOpen.
      /// </summary>
      [JsonPropertyName("glocalOpen")]
      public MonitoredActivity GlocalOpen
      {
        get => glocalOpen ??= new();
        set => glocalOpen = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 750;

      private MonitoredActivity glocalOpen;
    }

    /// <summary>A MonaExpansion1Group group.</summary>
    [Serializable]
    public class MonaExpansion1Group
    {
      /// <summary>
      /// A value of GlocalMonaExpansion1.
      /// </summary>
      [JsonPropertyName("glocalMonaExpansion1")]
      public MonitoredActivity GlocalMonaExpansion1
      {
        get => glocalMonaExpansion1 ??= new();
        set => glocalMonaExpansion1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 750;

      private MonitoredActivity glocalMonaExpansion1;
    }

    /// <summary>A MonaExpansion2Group group.</summary>
    [Serializable]
    public class MonaExpansion2Group
    {
      /// <summary>
      /// A value of GlocalMonaExpansion2.
      /// </summary>
      [JsonPropertyName("glocalMonaExpansion2")]
      public MonitoredActivity GlocalMonaExpansion2
      {
        get => glocalMonaExpansion2 ??= new();
        set => glocalMonaExpansion2 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 750;

      private MonitoredActivity glocalMonaExpansion2;
    }

    /// <summary>A MonaExpansion3Group group.</summary>
    [Serializable]
    public class MonaExpansion3Group
    {
      /// <summary>
      /// A value of GlocalMonaExpansion3.
      /// </summary>
      [JsonPropertyName("glocalMonaExpansion3")]
      public MonitoredActivity GlocalMonaExpansion3
      {
        get => glocalMonaExpansion3 ??= new();
        set => glocalMonaExpansion3 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 750;

      private MonitoredActivity glocalMonaExpansion3;
    }

    /// <summary>A MonaExpansion4Group group.</summary>
    [Serializable]
    public class MonaExpansion4Group
    {
      /// <summary>
      /// A value of GlocalMonaExpansion4.
      /// </summary>
      [JsonPropertyName("glocalMonaExpansion4")]
      public MonitoredActivity GlocalMonaExpansion4
      {
        get => glocalMonaExpansion4 ??= new();
        set => glocalMonaExpansion4 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 750;

      private MonitoredActivity glocalMonaExpansion4;
    }

    /// <summary>A MonaExpansion5Group group.</summary>
    [Serializable]
    public class MonaExpansion5Group
    {
      /// <summary>
      /// A value of GlocalMonaExpansion5.
      /// </summary>
      [JsonPropertyName("glocalMonaExpansion5")]
      public MonitoredActivity GlocalMonaExpansion5
      {
        get => glocalMonaExpansion5 ??= new();
        set => glocalMonaExpansion5 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 750;

      private MonitoredActivity glocalMonaExpansion5;
    }

    /// <summary>A MonaExpansion6Group group.</summary>
    [Serializable]
    public class MonaExpansion6Group
    {
      /// <summary>
      /// A value of GlocalMonaExpansion6.
      /// </summary>
      [JsonPropertyName("glocalMonaExpansion6")]
      public MonitoredActivity GlocalMonaExpansion6
      {
        get => glocalMonaExpansion6 ??= new();
        set => glocalMonaExpansion6 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 750;

      private MonitoredActivity glocalMonaExpansion6;
    }

    /// <summary>
    /// Gets a value of OpenMona.
    /// </summary>
    [JsonIgnore]
    public Array<OpenMonaGroup> OpenMona => openMona ??= new(
      OpenMonaGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of OpenMona for json serialization.
    /// </summary>
    [JsonPropertyName("openMona")]
    [Computed]
    public IList<OpenMonaGroup> OpenMona_Json
    {
      get => openMona;
      set => OpenMona.Assign(value);
    }

    /// <summary>
    /// Gets a value of MonaExpansion1.
    /// </summary>
    [JsonIgnore]
    public Array<MonaExpansion1Group> MonaExpansion1 => monaExpansion1 ??= new(
      MonaExpansion1Group.Capacity, 0);

    /// <summary>
    /// Gets a value of MonaExpansion1 for json serialization.
    /// </summary>
    [JsonPropertyName("monaExpansion1")]
    [Computed]
    public IList<MonaExpansion1Group> MonaExpansion1_Json
    {
      get => monaExpansion1;
      set => MonaExpansion1.Assign(value);
    }

    /// <summary>
    /// Gets a value of MonaExpansion2.
    /// </summary>
    [JsonIgnore]
    public Array<MonaExpansion2Group> MonaExpansion2 => monaExpansion2 ??= new(
      MonaExpansion2Group.Capacity, 0);

    /// <summary>
    /// Gets a value of MonaExpansion2 for json serialization.
    /// </summary>
    [JsonPropertyName("monaExpansion2")]
    [Computed]
    public IList<MonaExpansion2Group> MonaExpansion2_Json
    {
      get => monaExpansion2;
      set => MonaExpansion2.Assign(value);
    }

    /// <summary>
    /// Gets a value of MonaExpansion3.
    /// </summary>
    [JsonIgnore]
    public Array<MonaExpansion3Group> MonaExpansion3 => monaExpansion3 ??= new(
      MonaExpansion3Group.Capacity, 0);

    /// <summary>
    /// Gets a value of MonaExpansion3 for json serialization.
    /// </summary>
    [JsonPropertyName("monaExpansion3")]
    [Computed]
    public IList<MonaExpansion3Group> MonaExpansion3_Json
    {
      get => monaExpansion3;
      set => MonaExpansion3.Assign(value);
    }

    /// <summary>
    /// Gets a value of MonaExpansion4.
    /// </summary>
    [JsonIgnore]
    public Array<MonaExpansion4Group> MonaExpansion4 => monaExpansion4 ??= new(
      MonaExpansion4Group.Capacity, 0);

    /// <summary>
    /// Gets a value of MonaExpansion4 for json serialization.
    /// </summary>
    [JsonPropertyName("monaExpansion4")]
    [Computed]
    public IList<MonaExpansion4Group> MonaExpansion4_Json
    {
      get => monaExpansion4;
      set => MonaExpansion4.Assign(value);
    }

    /// <summary>
    /// Gets a value of MonaExpansion5.
    /// </summary>
    [JsonIgnore]
    public Array<MonaExpansion5Group> MonaExpansion5 => monaExpansion5 ??= new(
      MonaExpansion5Group.Capacity, 0);

    /// <summary>
    /// Gets a value of MonaExpansion5 for json serialization.
    /// </summary>
    [JsonPropertyName("monaExpansion5")]
    [Computed]
    public IList<MonaExpansion5Group> MonaExpansion5_Json
    {
      get => monaExpansion5;
      set => MonaExpansion5.Assign(value);
    }

    /// <summary>
    /// Gets a value of MonaExpansion6.
    /// </summary>
    [JsonIgnore]
    public Array<MonaExpansion6Group> MonaExpansion6 => monaExpansion6 ??= new(
      MonaExpansion6Group.Capacity, 0);

    /// <summary>
    /// Gets a value of MonaExpansion6 for json serialization.
    /// </summary>
    [JsonPropertyName("monaExpansion6")]
    [Computed]
    public IList<MonaExpansion6Group> MonaExpansion6_Json
    {
      get => monaExpansion6;
      set => MonaExpansion6.Assign(value);
    }

    /// <summary>
    /// A value of SaveLastOfErrorGroup.
    /// </summary>
    [JsonPropertyName("saveLastOfErrorGroup")]
    public Common SaveLastOfErrorGroup
    {
      get => saveLastOfErrorGroup ??= new();
      set => saveLastOfErrorGroup = value;
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
    /// A value of MonitoredActivity.
    /// </summary>
    [JsonPropertyName("monitoredActivity")]
    public MonitoredActivity MonitoredActivity
    {
      get => monitoredActivity ??= new();
      set => monitoredActivity = value;
    }

    /// <summary>
    /// A value of Linitialized.
    /// </summary>
    [JsonPropertyName("linitialized")]
    public DateWorkArea Linitialized
    {
      get => linitialized ??= new();
      set => linitialized = value;
    }

    /// <summary>
    /// A value of AdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("administrativeAppeal")]
    public AdministrativeAppeal AdministrativeAppeal
    {
      get => administrativeAppeal ??= new();
      set => administrativeAppeal = value;
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
    /// A value of MonitoredActivityAssignment.
    /// </summary>
    [JsonPropertyName("monitoredActivityAssignment")]
    public MonitoredActivityAssignment MonitoredActivityAssignment
    {
      get => monitoredActivityAssignment ??= new();
      set => monitoredActivityAssignment = value;
    }

    /// <summary>
    /// A value of CreateMonaAssignment.
    /// </summary>
    [JsonPropertyName("createMonaAssignment")]
    public Common CreateMonaAssignment
    {
      get => createMonaAssignment ??= new();
      set => createMonaAssignment = value;
    }

    /// <summary>
    /// A value of SpPrintWorkSet.
    /// </summary>
    [JsonPropertyName("spPrintWorkSet")]
    public SpPrintWorkSet SpPrintWorkSet
    {
      get => spPrintWorkSet ??= new();
      set => spPrintWorkSet = value;
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
    /// A value of WorkArea.
    /// </summary>
    [JsonPropertyName("workArea")]
    public WorkArea WorkArea
    {
      get => workArea ??= new();
      set => workArea = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public MonitoredActivity Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of TbdLocalOpenMonitoredActivty.
    /// </summary>
    [JsonPropertyName("tbdLocalOpenMonitoredActivty")]
    public Common TbdLocalOpenMonitoredActivty
    {
      get => tbdLocalOpenMonitoredActivty ??= new();
      set => tbdLocalOpenMonitoredActivty = value;
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

    private Array<OpenMonaGroup> openMona;
    private Array<MonaExpansion1Group> monaExpansion1;
    private Array<MonaExpansion2Group> monaExpansion2;
    private Array<MonaExpansion3Group> monaExpansion3;
    private Array<MonaExpansion4Group> monaExpansion4;
    private Array<MonaExpansion5Group> monaExpansion5;
    private Array<MonaExpansion6Group> monaExpansion6;
    private Common saveLastOfErrorGroup;
    private DateWorkArea max;
    private MonitoredActivity monitoredActivity;
    private DateWorkArea linitialized;
    private AdministrativeAppeal administrativeAppeal;
    private LegalAction legalAction;
    private MonitoredActivityAssignment monitoredActivityAssignment;
    private Common createMonaAssignment;
    private SpPrintWorkSet spPrintWorkSet;
    private LegalReferral legalReferral;
    private WorkArea workArea;
    private MonitoredActivity null1;
    private Common tbdLocalOpenMonitoredActivty;
    private Infrastructure infrastructure;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ClosedCaseCheck.
    /// </summary>
    [JsonPropertyName("closedCaseCheck")]
    public Case1 ClosedCaseCheck
    {
      get => closedCaseCheck ??= new();
      set => closedCaseCheck = value;
    }

    /// <summary>
    /// A value of Activity.
    /// </summary>
    [JsonPropertyName("activity")]
    public Activity Activity
    {
      get => activity ??= new();
      set => activity = value;
    }

    /// <summary>
    /// A value of ActivityDetail.
    /// </summary>
    [JsonPropertyName("activityDetail")]
    public ActivityDetail ActivityDetail
    {
      get => activityDetail ??= new();
      set => activityDetail = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of ActivityDistributionRule.
    /// </summary>
    [JsonPropertyName("activityDistributionRule")]
    public ActivityDistributionRule ActivityDistributionRule
    {
      get => activityDistributionRule ??= new();
      set => activityDistributionRule = value;
    }

    /// <summary>
    /// A value of AdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("administrativeAppeal")]
    public AdministrativeAppeal AdministrativeAppeal
    {
      get => administrativeAppeal ??= new();
      set => administrativeAppeal = value;
    }

    /// <summary>
    /// A value of ObligationAdministrativeAction.
    /// </summary>
    [JsonPropertyName("obligationAdministrativeAction")]
    public ObligationAdministrativeAction ObligationAdministrativeAction
    {
      get => obligationAdministrativeAction ??= new();
      set => obligationAdministrativeAction = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
    }

    /// <summary>
    /// A value of KeyOnly.
    /// </summary>
    [JsonPropertyName("keyOnly")]
    public Infrastructure KeyOnly
    {
      get => keyOnly ??= new();
      set => keyOnly = value;
    }

    /// <summary>
    /// A value of ExistingLegalAction.
    /// </summary>
    [JsonPropertyName("existingLegalAction")]
    public LegalAction ExistingLegalAction
    {
      get => existingLegalAction ??= new();
      set => existingLegalAction = value;
    }

    /// <summary>
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    /// <summary>
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
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
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
    }

    /// <summary>
    /// A value of LegalReferralAssignment.
    /// </summary>
    [JsonPropertyName("legalReferralAssignment")]
    public LegalReferralAssignment LegalReferralAssignment
    {
      get => legalReferralAssignment ??= new();
      set => legalReferralAssignment = value;
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
    /// A value of AdministrativeAppealAssignment.
    /// </summary>
    [JsonPropertyName("administrativeAppealAssignment")]
    public AdministrativeAppealAssignment AdministrativeAppealAssignment
    {
      get => administrativeAppealAssignment ??= new();
      set => administrativeAppealAssignment = value;
    }

    /// <summary>
    /// A value of ObligationAdminActionAssgn.
    /// </summary>
    [JsonPropertyName("obligationAdminActionAssgn")]
    public ObligationAdminActionAssgn ObligationAdminActionAssgn
    {
      get => obligationAdminActionAssgn ??= new();
      set => obligationAdminActionAssgn = value;
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
    /// A value of ExistingInterstateCase.
    /// </summary>
    [JsonPropertyName("existingInterstateCase")]
    public InterstateCase ExistingInterstateCase
    {
      get => existingInterstateCase ??= new();
      set => existingInterstateCase = value;
    }

    /// <summary>
    /// A value of LegalActionAssigment.
    /// </summary>
    [JsonPropertyName("legalActionAssigment")]
    public LegalActionAssigment LegalActionAssigment
    {
      get => legalActionAssigment ??= new();
      set => legalActionAssigment = value;
    }

    /// <summary>
    /// A value of ObligationAssignment.
    /// </summary>
    [JsonPropertyName("obligationAssignment")]
    public ObligationAssignment ObligationAssignment
    {
      get => obligationAssignment ??= new();
      set => obligationAssignment = value;
    }

    /// <summary>
    /// A value of ActivityStartStop.
    /// </summary>
    [JsonPropertyName("activityStartStop")]
    public ActivityStartStop ActivityStartStop
    {
      get => activityStartStop ??= new();
      set => activityStartStop = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public LegalAction New1
    {
      get => new1 ??= new();
      set => new1 = value;
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
    /// A value of Event1.
    /// </summary>
    [JsonPropertyName("event1")]
    public Event1 Event1
    {
      get => event1 ??= new();
      set => event1 = value;
    }

    private Case1 closedCaseCheck;
    private Activity activity;
    private ActivityDetail activityDetail;
    private CaseRole caseRole;
    private Case1 case1;
    private CsePerson csePerson;
    private ActivityDistributionRule activityDistributionRule;
    private AdministrativeAppeal administrativeAppeal;
    private ObligationAdministrativeAction obligationAdministrativeAction;
    private Obligation obligation;
    private CsePersonAccount csePersonAccount;
    private Infrastructure keyOnly;
    private LegalAction existingLegalAction;
    private LegalActionDetail legalActionDetail;
    private LegalActionPerson legalActionPerson;
    private OfficeServiceProvider officeServiceProvider;
    private Office office;
    private ServiceProvider serviceProvider;
    private CaseAssignment caseAssignment;
    private CaseUnitFunctionAssignmt caseUnitFunctionAssignmt;
    private CaseUnit caseUnit;
    private LegalReferralAssignment legalReferralAssignment;
    private LegalReferral legalReferral;
    private PrinterOutputDestination printerOutputDestination;
    private OutgoingDocument outgoingDocument;
    private AdministrativeAppealAssignment administrativeAppealAssignment;
    private ObligationAdminActionAssgn obligationAdminActionAssgn;
    private InterstateCaseAssignment interstateCaseAssignment;
    private InterstateCase existingInterstateCase;
    private LegalActionAssigment legalActionAssigment;
    private ObligationAssignment obligationAssignment;
    private ActivityStartStop activityStartStop;
    private MonitoredActivity monitoredActivity;
    private Infrastructure infrastructure;
    private Tribunal tribunal;
    private LegalAction new1;
    private EventDetail eventDetail;
    private Event1 event1;
  }
#endregion
}
