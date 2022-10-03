// Program: SP_CAB_VALIDATE_FOR_OSP_ASSIGNS, ID: 371791036, model: 746.
// Short name: SWE01871
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_CAB_VALIDATE_FOR_OSP_ASSIGNS.
/// </para>
/// <para>
/// This action block validates update (for discontinue) and delete processing 
/// for Office Service Providers.  When an Office Service Provider is selected
/// for discontinuance or deletion, this action block determines if any open
/// assignments to assignable business objects exist.  If assignments exist, the
/// discontinue or delete is disallowed.
/// </para>
/// </summary>
[Serializable]
public partial class SpCabValidateForOspAssigns: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_VALIDATE_FOR_OSP_ASSIGNS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabValidateForOspAssigns(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabValidateForOspAssigns.
  /// </summary>
  public SpCabValidateForOspAssigns(IContext context, Import import,
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
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;

    if (ReadOffice())
    {
      // Currency on Office acquired.
    }
    else
    {
      ExitState = "OFFICE_NF";

      return;
    }

    if (ReadServiceProvider())
    {
      // Currency on Service Provider acquired.
    }
    else
    {
      ExitState = "SERVICE_PROVIDER_NF";

      return;
    }

    if (ReadOfficeServiceProvider())
    {
      // Currency on Office Service Provider acquired.
    }
    else
    {
      ExitState = "OFFICE_SERVICE_PROVIDER_NF";

      return;
    }

    if (ReadCaseAssignment())
    {
      // Only one occurrence needs to be found to invalidate the discontinuance 
      // or deletion of the target Office Service Provider.
      ExitState = "SP0000_CANT_PROCESS_DUE_TO_ASGN";

      return;
    }

    if (ReadAdministrativeAppealAssignment())
    {
      // Only one occurrence needs to be found to invalidate the discontinuance 
      // or deletion of the target Office Service Provider.
      ExitState = "SP0000_CANT_PROCESS_DUE_TO_ASGN";

      return;
    }

    if (ReadCaseUnitFunctionAssignmt())
    {
      // Only one occurrence needs to be found to invalidate the discontinuance 
      // or deletion of the target Office Service Provider.
      ExitState = "SP0000_CANT_PROCESS_DUE_TO_ASGN";

      return;
    }

    if (ReadInformationRequestAssignment())
    {
      // Only one occurrence needs to be found to invalidate the discontinuance 
      // or deletion of the target Office Service Provider.
      ExitState = "SP0000_CANT_PROCESS_DUE_TO_ASGN";

      return;
    }

    if (ReadInterstateCaseAssignment())
    {
      // Only one occurrence needs to be found to invalidate the discontinuance 
      // or deletion of the target Office Service Provider.
      ExitState = "SP0000_CANT_PROCESS_DUE_TO_ASGN";

      return;
    }

    if (ReadLegalActionAssigment())
    {
      // Only one occurrence needs to be found to invalidate the discontinuance 
      // or deletion of the target Office Service Provider.  Note that on this
      // read it is not further qualified by a check on the reason code for
      // assignment.
      ExitState = "SP0000_CANT_PROCESS_DUE_TO_ASGN";

      return;
    }

    if (ReadMonitoredActivityAssignment())
    {
      // Only one occurrence needs to be found to invalidate the discontinuance 
      // or deletion of the target Office Service Provider.  Note that on this
      // read it is not further qualified by a check on the reason code for
      // assignment.
      ExitState = "SP0000_CANT_PROCESS_DUE_TO_ASGN";

      return;
    }

    if (ReadObligationAdminActionAssgn())
    {
      // Only one occurrence needs to be found to invalidate the discontinuance 
      // or deletion of the target Office Service Provider.  Note that on this
      // read it is not further qualified by a check on the reason code for
      // assignment.
      ExitState = "SP0000_CANT_PROCESS_DUE_TO_ASGN";

      return;
    }

    if (ReadObligationAssignment())
    {
      // Only one occurrence needs to be found to invalidate the discontinuance 
      // or deletion of the target Office Service Provider.  Note that on this
      // read it is not further qualified by a check on the reason code for
      // assignment.
      ExitState = "SP0000_CANT_PROCESS_DUE_TO_ASGN";

      return;
    }

    if (ReadPaReferralAssignment())
    {
      // Only one occurrence needs to be found to invalidate the discontinuance 
      // or deletion of the target Office Service Provider.  Note that on this
      // read it is not further qualified by a check on the reason code for
      // assignment.
      ExitState = "SP0000_CANT_PROCESS_DUE_TO_ASGN";

      return;
    }

    if (ReadOfficeServiceProvRelationship2())
    {
      // ********************************************************
      //      Office service prov relationship found, cannot
      //  be deleted.
      // ********************************************************
      ExitState = "SP0000_CANT_PROCESS_DUE_TO_ASGN";

      return;
    }

    if (ReadOfficeServiceProvRelationship1())
    {
      // ********************************************************
      //      Office service prov relationship found, cannot
      //  be deleted.
      // ********************************************************
      ExitState = "SP0000_CANT_PROCESS_DUE_TO_ASGN";

      return;
    }

    if (ReadOfficeServiceProviderAlert())
    {
      // ********************************************************
      //      Office service provider alert found , cannot
      //  be deleted.
      // ********************************************************
      ExitState = "SP0000_CANT_PROCESS_DUE_TO_ALRT";

      return;
    }

    // -- 01/17/2013 GVandy  CQ33617  Do not allow delete or end date if the 
    // service
    // --  provider is specified on an alert distribution rule.
    if (ReadAlertDistributionRule())
    {
      ExitState = "AP0000_CANT_PROCESS_DUE_TO_ADR";
    }
  }

  private bool ReadAdministrativeAppealAssignment()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.AdministrativeAppealAssignment.Populated = false;

    return Read("ReadAdministrativeAppealAssignment",
      (db, command) =>
      {
        db.SetDate(
          command, "ospDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.
          SetString(command, "ospCode", entities.OfficeServiceProvider.RoleCode);
          
        db.SetInt32(
          command, "offId", entities.OfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdId", entities.OfficeServiceProvider.SpdGeneratedId);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.AdministrativeAppealAssignment.ReasonCode =
          db.GetString(reader, 0);
        entities.AdministrativeAppealAssignment.EffectiveDate =
          db.GetDate(reader, 1);
        entities.AdministrativeAppealAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.AdministrativeAppealAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.AdministrativeAppealAssignment.SpdId = db.GetInt32(reader, 4);
        entities.AdministrativeAppealAssignment.OffId = db.GetInt32(reader, 5);
        entities.AdministrativeAppealAssignment.OspCode =
          db.GetString(reader, 6);
        entities.AdministrativeAppealAssignment.OspDate = db.GetDate(reader, 7);
        entities.AdministrativeAppealAssignment.AapId = db.GetInt32(reader, 8);
        entities.AdministrativeAppealAssignment.Populated = true;
      });
  }

  private bool ReadAlertDistributionRule()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.AlertDistributionRule.Populated = false;

    return Read("ReadAlertDistributionRule",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ospGeneratedId",
          entities.OfficeServiceProvider.SpdGeneratedId);
        db.SetNullableInt32(
          command, "offGeneratedId",
          entities.OfficeServiceProvider.OffGeneratedId);
        db.SetNullableString(
          command, "ospRoleCode", entities.OfficeServiceProvider.RoleCode);
        db.SetNullableDate(
          command, "ospEffectiveDt",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.AlertDistributionRule.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.AlertDistributionRule.EveNo = db.GetInt32(reader, 1);
        entities.AlertDistributionRule.EvdId = db.GetInt32(reader, 2);
        entities.AlertDistributionRule.OspGeneratedId =
          db.GetNullableInt32(reader, 3);
        entities.AlertDistributionRule.OffGeneratedId =
          db.GetNullableInt32(reader, 4);
        entities.AlertDistributionRule.OspRoleCode =
          db.GetNullableString(reader, 5);
        entities.AlertDistributionRule.OspEffectiveDt =
          db.GetNullableDate(reader, 6);
        entities.AlertDistributionRule.Populated = true;
      });
  }

  private bool ReadCaseAssignment()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.CaseAssignment.Populated = false;

    return Read("ReadCaseAssignment",
      (db, command) =>
      {
        db.SetDate(
          command, "ospDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.
          SetString(command, "ospCode", entities.OfficeServiceProvider.RoleCode);
          
        db.SetInt32(
          command, "offId", entities.OfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdId", entities.OfficeServiceProvider.SpdGeneratedId);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseAssignment.ReasonCode = db.GetString(reader, 0);
        entities.CaseAssignment.OverrideInd = db.GetString(reader, 1);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 2);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 3);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 5);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 6);
        entities.CaseAssignment.OspCode = db.GetString(reader, 7);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 8);
        entities.CaseAssignment.CasNo = db.GetString(reader, 9);
        entities.CaseAssignment.Populated = true;
      });
  }

  private bool ReadCaseUnitFunctionAssignmt()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.CaseUnitFunctionAssignmt.Populated = false;

    return Read("ReadCaseUnitFunctionAssignmt",
      (db, command) =>
      {
        db.SetDate(
          command, "ospDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.
          SetString(command, "ospCode", entities.OfficeServiceProvider.RoleCode);
          
        db.SetInt32(
          command, "offId", entities.OfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdId", entities.OfficeServiceProvider.SpdGeneratedId);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
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
        entities.CaseUnitFunctionAssignmt.Populated = true;
      });
  }

  private bool ReadInformationRequestAssignment()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.InformationRequestAssignment.Populated = false;

    return Read("ReadInformationRequestAssignment",
      (db, command) =>
      {
        db.SetDate(
          command, "ospDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.
          SetString(command, "ospCode", entities.OfficeServiceProvider.RoleCode);
          
        db.SetInt32(
          command, "offId", entities.OfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdId", entities.OfficeServiceProvider.SpdGeneratedId);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InformationRequestAssignment.ReasonCode =
          db.GetString(reader, 0);
        entities.InformationRequestAssignment.EffectiveDate =
          db.GetDate(reader, 1);
        entities.InformationRequestAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.InformationRequestAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.InformationRequestAssignment.SpdId = db.GetInt32(reader, 4);
        entities.InformationRequestAssignment.OffId = db.GetInt32(reader, 5);
        entities.InformationRequestAssignment.OspCode = db.GetString(reader, 6);
        entities.InformationRequestAssignment.OspDate = db.GetDate(reader, 7);
        entities.InformationRequestAssignment.InqNo = db.GetInt64(reader, 8);
        entities.InformationRequestAssignment.Populated = true;
      });
  }

  private bool ReadInterstateCaseAssignment()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.InterstateCaseAssignment.Populated = false;

    return Read("ReadInterstateCaseAssignment",
      (db, command) =>
      {
        db.SetDate(
          command, "ospDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.
          SetString(command, "ospCode", entities.OfficeServiceProvider.RoleCode);
          
        db.SetInt32(
          command, "offId", entities.OfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdId", entities.OfficeServiceProvider.SpdGeneratedId);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateCaseAssignment.ReasonCode = db.GetString(reader, 0);
        entities.InterstateCaseAssignment.EffectiveDate = db.GetDate(reader, 1);
        entities.InterstateCaseAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.InterstateCaseAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.InterstateCaseAssignment.SpdId = db.GetInt32(reader, 4);
        entities.InterstateCaseAssignment.OffId = db.GetInt32(reader, 5);
        entities.InterstateCaseAssignment.OspCode = db.GetString(reader, 6);
        entities.InterstateCaseAssignment.OspDate = db.GetDate(reader, 7);
        entities.InterstateCaseAssignment.IcsDate = db.GetDate(reader, 8);
        entities.InterstateCaseAssignment.IcsNo = db.GetInt64(reader, 9);
        entities.InterstateCaseAssignment.Populated = true;
      });
  }

  private bool ReadLegalActionAssigment()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.LegalActionAssigment.Populated = false;

    return Read("ReadLegalActionAssigment",
      (db, command) =>
      {
        db.SetNullableString(
          command, "ospRoleCode", entities.OfficeServiceProvider.RoleCode);
        db.SetNullableDate(
          command, "ospEffectiveDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.SetNullableInt32(
          command, "offGeneratedId",
          entities.OfficeServiceProvider.OffGeneratedId);
        db.SetNullableInt32(
          command, "spdGeneratedId",
          entities.OfficeServiceProvider.SpdGeneratedId);
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalActionAssigment.OspEffectiveDate =
          db.GetNullableDate(reader, 0);
        entities.LegalActionAssigment.OspRoleCode =
          db.GetNullableString(reader, 1);
        entities.LegalActionAssigment.OffGeneratedId =
          db.GetNullableInt32(reader, 2);
        entities.LegalActionAssigment.SpdGeneratedId =
          db.GetNullableInt32(reader, 3);
        entities.LegalActionAssigment.EffectiveDate = db.GetDate(reader, 4);
        entities.LegalActionAssigment.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.LegalActionAssigment.ReasonCode = db.GetString(reader, 6);
        entities.LegalActionAssigment.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.LegalActionAssigment.Populated = true;
      });
  }

  private bool ReadMonitoredActivityAssignment()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.MonitoredActivityAssignment.Populated = false;

    return Read("ReadMonitoredActivityAssignment",
      (db, command) =>
      {
        db.SetDate(
          command, "ospDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.
          SetString(command, "ospCode", entities.OfficeServiceProvider.RoleCode);
          
        db.SetInt32(
          command, "offId", entities.OfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdId", entities.OfficeServiceProvider.SpdGeneratedId);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.MonitoredActivityAssignment.ReasonCode =
          db.GetString(reader, 0);
        entities.MonitoredActivityAssignment.EffectiveDate =
          db.GetDate(reader, 1);
        entities.MonitoredActivityAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.MonitoredActivityAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.MonitoredActivityAssignment.SpdId = db.GetInt32(reader, 4);
        entities.MonitoredActivityAssignment.OffId = db.GetInt32(reader, 5);
        entities.MonitoredActivityAssignment.OspCode = db.GetString(reader, 6);
        entities.MonitoredActivityAssignment.OspDate = db.GetDate(reader, 7);
        entities.MonitoredActivityAssignment.MacId = db.GetInt32(reader, 8);
        entities.MonitoredActivityAssignment.Populated = true;
      });
  }

  private bool ReadObligationAdminActionAssgn()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.ObligationAdminActionAssgn.Populated = false;

    return Read("ReadObligationAdminActionAssgn",
      (db, command) =>
      {
        db.SetDate(
          command, "ospDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.
          SetString(command, "ospCode", entities.OfficeServiceProvider.RoleCode);
          
        db.SetInt32(
          command, "offId", entities.OfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdId", entities.OfficeServiceProvider.SpdGeneratedId);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligationAdminActionAssgn.ReasonCode =
          db.GetString(reader, 0);
        entities.ObligationAdminActionAssgn.EffectiveDate =
          db.GetDate(reader, 1);
        entities.ObligationAdminActionAssgn.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.ObligationAdminActionAssgn.CreatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.ObligationAdminActionAssgn.SpdId = db.GetInt32(reader, 4);
        entities.ObligationAdminActionAssgn.OffId = db.GetInt32(reader, 5);
        entities.ObligationAdminActionAssgn.OspCode = db.GetString(reader, 6);
        entities.ObligationAdminActionAssgn.OspDate = db.GetDate(reader, 7);
        entities.ObligationAdminActionAssgn.OtyId = db.GetInt32(reader, 8);
        entities.ObligationAdminActionAssgn.AatType = db.GetString(reader, 9);
        entities.ObligationAdminActionAssgn.ObgId = db.GetInt32(reader, 10);
        entities.ObligationAdminActionAssgn.CspNo = db.GetString(reader, 11);
        entities.ObligationAdminActionAssgn.CpaType = db.GetString(reader, 12);
        entities.ObligationAdminActionAssgn.OaaDate = db.GetDate(reader, 13);
        entities.ObligationAdminActionAssgn.Populated = true;
        CheckValid<ObligationAdminActionAssgn>("CpaType",
          entities.ObligationAdminActionAssgn.CpaType);
      });
  }

  private bool ReadObligationAssignment()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.ObligationAssignment.Populated = false;

    return Read("ReadObligationAssignment",
      (db, command) =>
      {
        db.SetDate(
          command, "ospDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.
          SetString(command, "ospCode", entities.OfficeServiceProvider.RoleCode);
          
        db.SetInt32(
          command, "offId", entities.OfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdId", entities.OfficeServiceProvider.SpdGeneratedId);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligationAssignment.ReasonCode = db.GetString(reader, 0);
        entities.ObligationAssignment.EffectiveDate = db.GetDate(reader, 1);
        entities.ObligationAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.ObligationAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.ObligationAssignment.SpdId = db.GetInt32(reader, 4);
        entities.ObligationAssignment.OffId = db.GetInt32(reader, 5);
        entities.ObligationAssignment.OspCode = db.GetString(reader, 6);
        entities.ObligationAssignment.OspDate = db.GetDate(reader, 7);
        entities.ObligationAssignment.OtyId = db.GetInt32(reader, 8);
        entities.ObligationAssignment.CpaType = db.GetString(reader, 9);
        entities.ObligationAssignment.CspNo = db.GetString(reader, 10);
        entities.ObligationAssignment.ObgId = db.GetInt32(reader, 11);
        entities.ObligationAssignment.Populated = true;
        CheckValid<ObligationAssignment>("CpaType",
          entities.ObligationAssignment.CpaType);
      });
  }

  private bool ReadOffice()
  {
    entities.Office.Populated = false;

    return Read("ReadOffice",
      (db, command) =>
      {
        db.SetInt32(command, "officeId", import.Office.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 1);
        entities.Office.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvRelationship1()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.OfficeServiceProvRelationship.Populated = false;

    return Read("ReadOfficeServiceProvRelationship1",
      (db, command) =>
      {
        db.SetString(
          command, "ospRoleCode", entities.OfficeServiceProvider.RoleCode);
        db.SetDate(
          command, "ospEffectiveDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.SetInt32(
          command, "offGeneratedId",
          entities.OfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdGeneratedId",
          entities.OfficeServiceProvider.SpdGeneratedId);
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvRelationship.OspEffectiveDate =
          db.GetDate(reader, 0);
        entities.OfficeServiceProvRelationship.OspRoleCode =
          db.GetString(reader, 1);
        entities.OfficeServiceProvRelationship.OffGeneratedId =
          db.GetInt32(reader, 2);
        entities.OfficeServiceProvRelationship.SpdGeneratedId =
          db.GetInt32(reader, 3);
        entities.OfficeServiceProvRelationship.OspREffectiveDt =
          db.GetDate(reader, 4);
        entities.OfficeServiceProvRelationship.OspRRoleCode =
          db.GetString(reader, 5);
        entities.OfficeServiceProvRelationship.OffRGeneratedId =
          db.GetInt32(reader, 6);
        entities.OfficeServiceProvRelationship.SpdRGeneratedId =
          db.GetInt32(reader, 7);
        entities.OfficeServiceProvRelationship.ReasonCode =
          db.GetString(reader, 8);
        entities.OfficeServiceProvRelationship.CreatedBy =
          db.GetString(reader, 9);
        entities.OfficeServiceProvRelationship.CreatedDtstamp =
          db.GetDateTime(reader, 10);
        entities.OfficeServiceProvRelationship.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvRelationship2()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.OfficeServiceProvRelationship.Populated = false;

    return Read("ReadOfficeServiceProvRelationship2",
      (db, command) =>
      {
        db.SetString(
          command, "ospRRoleCode", entities.OfficeServiceProvider.RoleCode);
        db.SetDate(
          command, "ospREffectiveDt",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.SetInt32(
          command, "offRGeneratedId",
          entities.OfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdRGeneratedId",
          entities.OfficeServiceProvider.SpdGeneratedId);
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvRelationship.OspEffectiveDate =
          db.GetDate(reader, 0);
        entities.OfficeServiceProvRelationship.OspRoleCode =
          db.GetString(reader, 1);
        entities.OfficeServiceProvRelationship.OffGeneratedId =
          db.GetInt32(reader, 2);
        entities.OfficeServiceProvRelationship.SpdGeneratedId =
          db.GetInt32(reader, 3);
        entities.OfficeServiceProvRelationship.OspREffectiveDt =
          db.GetDate(reader, 4);
        entities.OfficeServiceProvRelationship.OspRRoleCode =
          db.GetString(reader, 5);
        entities.OfficeServiceProvRelationship.OffRGeneratedId =
          db.GetInt32(reader, 6);
        entities.OfficeServiceProvRelationship.SpdRGeneratedId =
          db.GetInt32(reader, 7);
        entities.OfficeServiceProvRelationship.ReasonCode =
          db.GetString(reader, 8);
        entities.OfficeServiceProvRelationship.CreatedBy =
          db.GetString(reader, 9);
        entities.OfficeServiceProvRelationship.CreatedDtstamp =
          db.GetDateTime(reader, 10);
        entities.OfficeServiceProvRelationship.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvider()
  {
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          import.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.
          SetString(command, "roleCode", import.OfficeServiceProvider.RoleCode);
          
        db.SetInt32(
          command, "spdGeneratedId",
          entities.ServiceProvider.SystemGeneratedId);
        db.
          SetInt32(command, "offGeneratedId", entities.Office.SystemGeneratedId);
          
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadOfficeServiceProviderAlert()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.OfficeServiceProviderAlert.Populated = false;

    return Read("ReadOfficeServiceProviderAlert",
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
      },
      (db, reader) =>
      {
        entities.OfficeServiceProviderAlert.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.OfficeServiceProviderAlert.TypeCode = db.GetString(reader, 1);
        entities.OfficeServiceProviderAlert.Message = db.GetString(reader, 2);
        entities.OfficeServiceProviderAlert.SpdId =
          db.GetNullableInt32(reader, 3);
        entities.OfficeServiceProviderAlert.OffId =
          db.GetNullableInt32(reader, 4);
        entities.OfficeServiceProviderAlert.OspCode =
          db.GetNullableString(reader, 5);
        entities.OfficeServiceProviderAlert.OspDate =
          db.GetNullableDate(reader, 6);
        entities.OfficeServiceProviderAlert.Populated = true;
      });
  }

  private bool ReadPaReferralAssignment()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.PaReferralAssignment.Populated = false;

    return Read("ReadPaReferralAssignment",
      (db, command) =>
      {
        db.SetDate(
          command, "ospDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.
          SetString(command, "ospCode", entities.OfficeServiceProvider.RoleCode);
          
        db.SetInt32(
          command, "offId", entities.OfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdId", entities.OfficeServiceProvider.SpdGeneratedId);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PaReferralAssignment.ReasonCode = db.GetString(reader, 0);
        entities.PaReferralAssignment.EffectiveDate = db.GetDate(reader, 1);
        entities.PaReferralAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.PaReferralAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.PaReferralAssignment.SpdId = db.GetInt32(reader, 4);
        entities.PaReferralAssignment.OffId = db.GetInt32(reader, 5);
        entities.PaReferralAssignment.OspCode = db.GetString(reader, 6);
        entities.PaReferralAssignment.OspDate = db.GetDate(reader, 7);
        entities.PaReferralAssignment.PafNo = db.GetString(reader, 8);
        entities.PaReferralAssignment.PafType = db.GetString(reader, 9);
        entities.PaReferralAssignment.PafTstamp = db.GetDateTime(reader, 10);
        entities.PaReferralAssignment.Populated = true;
      });
  }

  private bool ReadServiceProvider()
  {
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider",
      (db, command) =>
      {
        db.SetInt32(
          command, "servicePrvderId", import.ServiceProvider.SystemGeneratedId);
          
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.Populated = true;
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

    private ServiceProvider serviceProvider;
    private Office office;
    private OfficeServiceProvider officeServiceProvider;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    private DateWorkArea current;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of AlertDistributionRule.
    /// </summary>
    [JsonPropertyName("alertDistributionRule")]
    public AlertDistributionRule AlertDistributionRule
    {
      get => alertDistributionRule ??= new();
      set => alertDistributionRule = value;
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
    /// A value of OfficeServiceProvRelationship.
    /// </summary>
    [JsonPropertyName("officeServiceProvRelationship")]
    public OfficeServiceProvRelationship OfficeServiceProvRelationship
    {
      get => officeServiceProvRelationship ??= new();
      set => officeServiceProvRelationship = value;
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
    /// A value of PaReferralAssignment.
    /// </summary>
    [JsonPropertyName("paReferralAssignment")]
    public PaReferralAssignment PaReferralAssignment
    {
      get => paReferralAssignment ??= new();
      set => paReferralAssignment = value;
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
    /// A value of ObligationAdminActionAssgn.
    /// </summary>
    [JsonPropertyName("obligationAdminActionAssgn")]
    public ObligationAdminActionAssgn ObligationAdminActionAssgn
    {
      get => obligationAdminActionAssgn ??= new();
      set => obligationAdminActionAssgn = value;
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
    /// A value of InterstateCaseAssignment.
    /// </summary>
    [JsonPropertyName("interstateCaseAssignment")]
    public InterstateCaseAssignment InterstateCaseAssignment
    {
      get => interstateCaseAssignment ??= new();
      set => interstateCaseAssignment = value;
    }

    /// <summary>
    /// A value of InformationRequestAssignment.
    /// </summary>
    [JsonPropertyName("informationRequestAssignment")]
    public InformationRequestAssignment InformationRequestAssignment
    {
      get => informationRequestAssignment ??= new();
      set => informationRequestAssignment = value;
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
    /// A value of AdministrativeAppealAssignment.
    /// </summary>
    [JsonPropertyName("administrativeAppealAssignment")]
    public AdministrativeAppealAssignment AdministrativeAppealAssignment
    {
      get => administrativeAppealAssignment ??= new();
      set => administrativeAppealAssignment = value;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
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
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    private AlertDistributionRule alertDistributionRule;
    private OfficeServiceProviderAlert officeServiceProviderAlert;
    private OfficeServiceProvRelationship officeServiceProvRelationship;
    private MonitoredActivityAssignment monitoredActivityAssignment;
    private PaReferralAssignment paReferralAssignment;
    private ObligationAssignment obligationAssignment;
    private ObligationAdminActionAssgn obligationAdminActionAssgn;
    private LegalActionAssigment legalActionAssigment;
    private InterstateCaseAssignment interstateCaseAssignment;
    private InformationRequestAssignment informationRequestAssignment;
    private CaseUnitFunctionAssignmt caseUnitFunctionAssignmt;
    private AdministrativeAppealAssignment administrativeAppealAssignment;
    private ServiceProvider serviceProvider;
    private Office office;
    private CaseAssignment caseAssignment;
    private OfficeServiceProvider officeServiceProvider;
  }
#endregion
}
