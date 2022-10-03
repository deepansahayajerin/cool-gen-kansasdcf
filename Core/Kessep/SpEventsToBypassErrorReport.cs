// Program: SP_EVENTS_TO_BYPASS_ERROR_REPORT, ID: 371047280, model: 746.
// Short name: SWE02882
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_EVENTS_TO_BYPASS_ERROR_REPORT.
/// </para>
/// <para>
/// Interogates legitimacy of selected events.  This is required due to the 
/// amount
/// of time normally taken for the rollback and reopening of the cursor 
/// afterwards
/// when the number of unprocessed &quot;Q&quot; records builds up to over 20,
/// 000.  The
/// vast majority of erred events are numbers 5, 7, 29, 30, 40, and 95.
/// As the system requires, more may need to be added.
/// Also prevents specific, non Federally required ('O') regulation sources from
/// being written to the error report if a valid assignment does not exist.
/// Any activity / Activity Detail / Activity Distribution Rule combination 
/// falling into
/// the list will not only not have the error written to program error table but
/// will not
/// have the corresponding INFRASTRUCTURE record's Process_Status Indicator set 
/// to &quot;E&quot; anymore; it will be set to &quot;H&quot;.
/// The list of corresponding Event, Event Detail, and Start/Stop code for the
///  Activity, Activity Detail, and Activity Distribution Rule pertaining to WR 
/// # 225
/// are listed at the bottom of the program.
/// </para>
/// </summary>
[Serializable]
public partial class SpEventsToBypassErrorReport: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_EVENTS_TO_BYPASS_ERROR_REPORT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpEventsToBypassErrorReport(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpEventsToBypassErrorReport.
  /// </summary>
  public SpEventsToBypassErrorReport(IContext context, Import import,
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
    // ---------------------------------------------------------------
    // CHANGE LOG
    // 01/18/2001	SWSRPRM
    // PR # 111279 => Initial coding
    // 03/28/2001	SWSRPRM
    // WR # 225-A => Prevent certain event/activity combinations
    // from appearing on error report
    // 04/14/2001	SWSRPRM
    // WR # 225-B => Federal Legal Monitored Activities:  If there
    // is no one assigned to the business object, determine the
    // worker assigned to the case business object and assign the
    // monitored activity to them.  If the condition is met, logic will
    // exit this program and proceed to SP_PROCESS_
    // MONITORED_ACTIVITIES where the proper assignment will
    // take place.
    // 04/02/2003	E.SHIRK
    // WR225	Corrected two activity/activity detail/dist rule id
    // combinations that are to be exempted from the error report.
    // 04/15/2010	GVandy		CQ 966
    // Significant restructuring to improve readability and simplify
    // logic. Action block renamed from SP_EVENT_PRE_EDIT to 
    // SP_EVENTS_TO_BYPASS_ERROR_REPORT.
    // 05/27/2014	GVandy		CQ39638
    // Add processing for Activities 36,75,77,79 for LRF events.
    // ---------------------------------------------------------------
    export.BypassErrorReport.Flag = "N";

    if (!Equal(import.ActivityDistributionRule.ResponsibilityCode, "RSP"))
    {
      // ---------------------------------------------------------
      // no processing required @ present for (INF) informational
      // ---------------------------------------------------------
      return;
    }

    if (((import.Activity.ControlNumber == 13 || import
      .Activity.ControlNumber == 43 || import.Activity.ControlNumber == 52 || import
      .Activity.ControlNumber == 53 || import.Activity.ControlNumber == 87 || import
      .Activity.ControlNumber == 88 || import.Activity.ControlNumber == 89 || import
      .Activity.ControlNumber == 90 || import.Activity.ControlNumber == 91 || import
      .Activity.ControlNumber == 93 || import.Activity.ControlNumber == 94 || import
      .Activity.ControlNumber == 95 || import.Activity.ControlNumber == 96 || import
      .Activity.ControlNumber == 101 || import.Activity.ControlNumber == 102
      ) && import.ActivityDetail.SystemGeneratedIdentifier == 1 && import
      .ActivityDistributionRule.SystemGeneratedIdentifier == 1 || (
        import.Activity.ControlNumber == 10 || import
      .Activity.ControlNumber == 11 || import.Activity.ControlNumber == 12 || import
      .Activity.ControlNumber == 66 || import.Activity.ControlNumber == 67 || import
      .Activity.ControlNumber == 91 || import.Activity.ControlNumber == 97) && import
      .ActivityDetail.SystemGeneratedIdentifier == 2 && import
      .ActivityDistributionRule.SystemGeneratedIdentifier == 1 || import
      .Activity.ControlNumber == 14 && import
      .ActivityDetail.SystemGeneratedIdentifier == 1 && import
      .ActivityDistributionRule.SystemGeneratedIdentifier == 2 || (
        import.Activity.ControlNumber == 41 || import
      .Activity.ControlNumber == 42) && import
      .ActivityDetail.SystemGeneratedIdentifier == 1 && import
      .ActivityDistributionRule.SystemGeneratedIdentifier == 3 || (
        import.Activity.ControlNumber == 38 || import
      .Activity.ControlNumber == 49 || import.Activity.ControlNumber == 52 || import
      .Activity.ControlNumber == 61) && import
      .ActivityDetail.SystemGeneratedIdentifier == 1 && import
      .ActivityDistributionRule.SystemGeneratedIdentifier == 4 || import
      .Activity.ControlNumber == 92 && (
        import.ActivityDetail.SystemGeneratedIdentifier == 1 || import
      .ActivityDetail.SystemGeneratedIdentifier == 2 || import
      .ActivityDetail.SystemGeneratedIdentifier == 3 || import
      .ActivityDetail.SystemGeneratedIdentifier == 4) && import
      .ActivityDistributionRule.SystemGeneratedIdentifier == 1 || import
      .Activity.ControlNumber == 35 && import
      .ActivityDetail.SystemGeneratedIdentifier == 1 && import
      .ActivityDistributionRule.SystemGeneratedIdentifier == 5) && Equal
      (import.Event1.BusinessObjectCode, "LEA") && AsChar
      (import.ActivityDetail.RegulationSourceId) == 'O')
    {
      // -- This is the list of Legal Action activity/activity detail/activity 
      // distribution rule combinations that are eligible for exclusion from the
      // error report.
    }
    else if ((import.Activity.ControlNumber == 75 || import
      .Activity.ControlNumber == 77 || import.Activity.ControlNumber == 79 || import
      .Activity.ControlNumber == 36) && import
      .ActivityDetail.SystemGeneratedIdentifier == 1 && Equal
      (import.Event1.BusinessObjectCode, "LRF") && AsChar
      (import.ActivityDetail.RegulationSourceId) == 'O')
    {
      // -- This is the list of Legal Referral activity/activity detail/activity
      // distribution rule combinations that are eligible for exclusion from
      // the error report.
    }
    else
    {
      return;
    }

    switch(TrimEnd(import.ActivityDistributionRule.BusinessObjectCode))
    {
      case "LEA":
        export.BypassErrorReport.Flag = "Y";
        local.LegalAction.Identifier =
          (int)import.Infrastructure.DenormNumeric12.GetValueOrDefault();

        if (ReadLegalActionAssigment())
        {
          export.BypassErrorReport.Flag = "N";
        }

        break;
      case "CAS":
        export.BypassErrorReport.Flag = "Y";

        if (ReadCaseAssignment())
        {
          export.BypassErrorReport.Flag = "N";
        }

        break;
      case "CAU":
        export.BypassErrorReport.Flag = "Y";

        if (ReadCaseAssignment())
        {
          export.BypassErrorReport.Flag = "N";

          return;
        }

        if (ReadCaseUnitFunctionAssignmt())
        {
          export.BypassErrorReport.Flag = "N";
        }

        break;
      case "LRF":
        export.BypassErrorReport.Flag = "Y";
        local.LegalReferral.Identifier =
          (int)import.Infrastructure.DenormNumeric12.GetValueOrDefault();

        if (ReadLegalReferralAssignment())
        {
          export.BypassErrorReport.Flag = "N";
        }

        break;
      default:
        break;
    }
  }

  private bool ReadCaseAssignment()
  {
    entities.CaseAssignment.Populated = false;

    return Read("ReadCaseAssignment",
      (db, command) =>
      {
        db.SetString(command, "casNo", import.Infrastructure.CaseNumber ?? "");
        db.SetDate(
          command, "effectiveDate", import.Current.Date.GetValueOrDefault());
        db.SetString(
          command, "reasonCode", import.ActivityDistributionRule.ReasonCode);
      },
      (db, reader) =>
      {
        entities.CaseAssignment.ReasonCode = db.GetString(reader, 0);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 1);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 4);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 5);
        entities.CaseAssignment.OspCode = db.GetString(reader, 6);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 7);
        entities.CaseAssignment.CasNo = db.GetString(reader, 8);
        entities.CaseAssignment.Populated = true;
      });
  }

  private bool ReadCaseUnitFunctionAssignmt()
  {
    entities.CaseUnitFunctionAssignmt.Populated = false;

    return Read("ReadCaseUnitFunctionAssignmt",
      (db, command) =>
      {
        db.SetString(command, "casNo", import.Infrastructure.CaseNumber ?? "");
        db.SetInt32(
          command, "csuNo",
          import.Infrastructure.CaseUnitNumber.GetValueOrDefault());
        db.SetString(
          command, "function",
          import.ActivityDistributionRule.CaseUnitFunction ?? "");
        db.SetDate(
          command, "effectiveDate", import.Current.Date.GetValueOrDefault());
        db.SetString(
          command, "reasonCode", import.ActivityDistributionRule.ReasonCode);
      },
      (db, reader) =>
      {
        entities.CaseUnitFunctionAssignmt.ReasonCode = db.GetString(reader, 0);
        entities.CaseUnitFunctionAssignmt.EffectiveDate = db.GetDate(reader, 1);
        entities.CaseUnitFunctionAssignmt.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.CaseUnitFunctionAssignmt.CreatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.CaseUnitFunctionAssignmt.SpdId = db.GetInt32(reader, 4);
        entities.CaseUnitFunctionAssignmt.OffId = db.GetInt32(reader, 5);
        entities.CaseUnitFunctionAssignmt.OspCode = db.GetString(reader, 6);
        entities.CaseUnitFunctionAssignmt.OspDate = db.GetDate(reader, 7);
        entities.CaseUnitFunctionAssignmt.CsuNo = db.GetInt32(reader, 8);
        entities.CaseUnitFunctionAssignmt.CasNo = db.GetString(reader, 9);
        entities.CaseUnitFunctionAssignmt.Function = db.GetString(reader, 10);
        entities.CaseUnitFunctionAssignmt.Populated = true;
      });
  }

  private bool ReadLegalActionAssigment()
  {
    entities.LegalActionAssigment.Populated = false;

    return Read("ReadLegalActionAssigment",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", local.LegalAction.Identifier);
        db.SetDate(
          command, "effectiveDt", import.Current.Date.GetValueOrDefault());
        db.SetString(
          command, "reasonCode", import.ActivityDistributionRule.ReasonCode);
      },
      (db, reader) =>
      {
        entities.LegalActionAssigment.LgaIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.LegalActionAssigment.EffectiveDate = db.GetDate(reader, 1);
        entities.LegalActionAssigment.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.LegalActionAssigment.ReasonCode = db.GetString(reader, 3);
        entities.LegalActionAssigment.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.LegalActionAssigment.Populated = true;
      });
  }

  private bool ReadLegalReferralAssignment()
  {
    entities.LegalReferralAssignment.Populated = false;

    return Read("ReadLegalReferralAssignment",
      (db, command) =>
      {
        db.SetInt32(command, "lgrId", local.LegalReferral.Identifier);
        db.SetString(command, "casNo", import.Infrastructure.CaseNumber ?? "");
        db.SetDate(
          command, "effectiveDate", import.Current.Date.GetValueOrDefault());
        db.SetString(
          command, "reasonCode", import.ActivityDistributionRule.ReasonCode);
      },
      (db, reader) =>
      {
        entities.LegalReferralAssignment.ReasonCode = db.GetString(reader, 0);
        entities.LegalReferralAssignment.EffectiveDate = db.GetDate(reader, 1);
        entities.LegalReferralAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.LegalReferralAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.LegalReferralAssignment.SpdId = db.GetInt32(reader, 4);
        entities.LegalReferralAssignment.OffId = db.GetInt32(reader, 5);
        entities.LegalReferralAssignment.OspCode = db.GetString(reader, 6);
        entities.LegalReferralAssignment.OspDate = db.GetDate(reader, 7);
        entities.LegalReferralAssignment.CasNo = db.GetString(reader, 8);
        entities.LegalReferralAssignment.LgrId = db.GetInt32(reader, 9);
        entities.LegalReferralAssignment.Populated = true;
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
    /// A value of Activity.
    /// </summary>
    [JsonPropertyName("activity")]
    public Activity Activity
    {
      get => activity ??= new();
      set => activity = value;
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
    /// A value of ActivityDetail.
    /// </summary>
    [JsonPropertyName("activityDetail")]
    public ActivityDetail ActivityDetail
    {
      get => activityDetail ??= new();
      set => activityDetail = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    private Activity activity;
    private ActivityDistributionRule activityDistributionRule;
    private ActivityDetail activityDetail;
    private Event1 event1;
    private Infrastructure infrastructure;
    private DateWorkArea current;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of BypassErrorReport.
    /// </summary>
    [JsonPropertyName("bypassErrorReport")]
    public Common BypassErrorReport
    {
      get => bypassErrorReport ??= new();
      set => bypassErrorReport = value;
    }

    private Common bypassErrorReport;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    private LegalReferral legalReferral;
    private LegalAction legalAction;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of LegalReferralAssignment.
    /// </summary>
    [JsonPropertyName("legalReferralAssignment")]
    public LegalReferralAssignment LegalReferralAssignment
    {
      get => legalReferralAssignment ??= new();
      set => legalReferralAssignment = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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

    private LegalReferral legalReferral;
    private LegalReferralAssignment legalReferralAssignment;
    private LegalActionAssigment legalActionAssigment;
    private LegalAction legalAction;
    private CaseAssignment caseAssignment;
    private Case1 case1;
    private CaseUnitFunctionAssignmt caseUnitFunctionAssignmt;
    private CaseUnit caseUnit;
  }
#endregion
}
