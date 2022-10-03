// Program: SP_DELETE_OFFICE_SERVICE_PRVDR, ID: 371784474, model: 746.
// Short name: SWE01333
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_DELETE_OFFICE_SERVICE_PRVDR.
/// </summary>
[Serializable]
public partial class SpDeleteOfficeServicePrvdr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_DELETE_OFFICE_SERVICE_PRVDR program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpDeleteOfficeServicePrvdr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpDeleteOfficeServicePrvdr.
  /// </summary>
  public SpDeleteOfficeServicePrvdr(IContext context, Import import,
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
    // ************************************************************
    // 03/23/98	Siraj Konkader			ZDEL Cleanup
    // ************************************************************
    local.Current.Date = Now().Date;
    MoveOfficeServiceProvider(import.OfficeServiceProvider,
      export.OfficeServiceProvider);
    ExitState = "ACO_NN0000_ALL_OK";

    if (!ReadOfficeServiceProvider())
    {
      ExitState = "OFFICE_SERVICE_PROVIDER_NF";

      return;
    }

    if (ReadGeneticTestAccount())
    {
      ExitState = "SERVCE_PRVDR_RELATE_GENETIC_TEST";

      return;
    }

    if (ReadOfficeServiceProvRelationship2())
    {
      ExitState = "OFFICE_SRVCE_PRVDR_RELATION_EX";

      return;
    }

    if (ReadOfficeServiceProvRelationship1())
    {
      ExitState = "OFFICE_SRVCE_PRVDR_RELATION_EX";

      return;
    }

    if (ReadOfficeCaseloadAssignment())
    {
      ExitState = "OFFICE_CASELOAD_ASSIGNMENT_T_AE";

      return;
    }

    // Check for Business Object assignment records.  If an OSP has EVER been 
    // assigned to a business object,
    // they cannot be deleted from the database.
    if (ReadPaReferralAssignment())
    {
      ExitState = "SP0000_CANT_DEL_ASGN_RECS_EXIST";

      return;
    }

    if (ReadInformationRequestAssignment())
    {
      ExitState = "SP0000_CANT_DEL_ASGN_RECS_EXIST";

      return;
    }

    if (ReadCaseAssignment())
    {
      ExitState = "SP0000_CANT_DEL_ASGN_RECS_EXIST";

      return;
    }

    if (ReadCaseUnitFunctionAssignmt())
    {
      ExitState = "SP0000_CANT_DEL_ASGN_RECS_EXIST";

      return;
    }

    if (ReadAdministrativeAppealAssignment())
    {
      ExitState = "SP0000_CANT_DEL_ASGN_RECS_EXIST";

      return;
    }

    if (ReadInterstateCaseAssignment())
    {
      ExitState = "SP0000_CANT_DEL_ASGN_RECS_EXIST";

      return;
    }

    if (ReadLegalActionAssigment())
    {
      ExitState = "SP0000_CANT_DEL_ASGN_RECS_EXIST";

      return;
    }

    if (ReadLegalReferralAssignment())
    {
      ExitState = "SP0000_CANT_DEL_ASGN_RECS_EXIST";

      return;
    }

    if (ReadMonitoredActivityAssignment())
    {
      ExitState = "SP0000_CANT_DEL_ASGN_RECS_EXIST";

      return;
    }

    if (ReadObligationAssignment())
    {
      ExitState = "SP0000_CANT_DEL_ASGN_RECS_EXIST";

      return;
    }

    if (ReadObligationAdminActionAssgn())
    {
      ExitState = "SP0000_CANT_DEL_ASGN_RECS_EXIST";

      return;
    }

    if (ReadOfficeServiceProviderAlert())
    {
      ExitState = "SP0000_CANT_DEL_ASGN_RECS_EXIST";

      return;
    }

    export.OfficeServiceProvider.Assign(entities.OfficeServiceProvider);
    DeleteOfficeServiceProvider();
  }

  private static void MoveOfficeServiceProvider(OfficeServiceProvider source,
    OfficeServiceProvider target)
  {
    target.RoleCode = source.RoleCode;
    target.WorkPhoneNumber = source.WorkPhoneNumber;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
  }

  private void DeleteOfficeServiceProvider()
  {
    bool exists;

    exists = Read("DeleteOfficeServiceProvider#1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "spdId", entities.OfficeServiceProvider.SpdGeneratedId);
        db.SetNullableInt32(
          command, "offId", entities.OfficeServiceProvider.OffGeneratedId);
        db.SetNullableString(
          command, "ospRoleCode", entities.OfficeServiceProvider.RoleCode);
        db.SetNullableDate(
          command, "ospDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
      },
      null);

    if (exists)
    {
      throw DataError("Restrict violation on table \"CKT_APPOINTMENT\".",
        "50001");
    }

    exists = Read("DeleteOfficeServiceProvider#2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "spdId", entities.OfficeServiceProvider.SpdGeneratedId);
        db.SetNullableInt32(
          command, "offId", entities.OfficeServiceProvider.OffGeneratedId);
        db.SetNullableString(
          command, "ospRoleCode", entities.OfficeServiceProvider.RoleCode);
        db.SetNullableDate(
          command, "ospDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
      },
      null);

    if (exists)
    {
      throw DataError("Restrict violation on table \"CKT_ASSGN_AAP\".", "50001");
        
    }

    exists = Read("DeleteOfficeServiceProvider#3",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "spdId", entities.OfficeServiceProvider.SpdGeneratedId);
        db.SetNullableInt32(
          command, "offId", entities.OfficeServiceProvider.OffGeneratedId);
        db.SetNullableString(
          command, "ospRoleCode", entities.OfficeServiceProvider.RoleCode);
        db.SetNullableDate(
          command, "ospDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
      },
      null);

    if (exists)
    {
      throw DataError("Restrict violation on table \"CKT_ASSGN_CASE_UNT\".",
        "50001");
    }

    exists = Read("DeleteOfficeServiceProvider#4",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "spdId", entities.OfficeServiceProvider.SpdGeneratedId);
        db.SetNullableInt32(
          command, "offId", entities.OfficeServiceProvider.OffGeneratedId);
        db.SetNullableString(
          command, "ospRoleCode", entities.OfficeServiceProvider.RoleCode);
        db.SetNullableDate(
          command, "ospDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
      },
      null);

    if (exists)
    {
      throw DataError("Restrict violation on table \"CKT_ASSGN_INFO_REQ\".",
        "50001");
    }

    exists = Read("DeleteOfficeServiceProvider#5",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "spdId", entities.OfficeServiceProvider.SpdGeneratedId);
        db.SetNullableInt32(
          command, "offId", entities.OfficeServiceProvider.OffGeneratedId);
        db.SetNullableString(
          command, "ospRoleCode", entities.OfficeServiceProvider.RoleCode);
        db.SetNullableDate(
          command, "ospDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
      },
      null);

    if (exists)
    {
      throw DataError("Restrict violation on table \"CKT_ASSGN_INT_CASE\".",
        "50001");
    }

    exists = Read("DeleteOfficeServiceProvider#6",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "spdId", entities.OfficeServiceProvider.SpdGeneratedId);
        db.SetNullableInt32(
          command, "offId", entities.OfficeServiceProvider.OffGeneratedId);
        db.SetNullableString(
          command, "ospRoleCode", entities.OfficeServiceProvider.RoleCode);
        db.SetNullableDate(
          command, "ospDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
      },
      null);

    if (exists)
    {
      throw DataError("Restrict violation on table \"CKT_ASSGN_LEG_REF\".",
        "50001");
    }

    exists = Read("DeleteOfficeServiceProvider#7",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "spdId", entities.OfficeServiceProvider.SpdGeneratedId);
        db.SetNullableInt32(
          command, "offId", entities.OfficeServiceProvider.OffGeneratedId);
        db.SetNullableString(
          command, "ospRoleCode", entities.OfficeServiceProvider.RoleCode);
        db.SetNullableDate(
          command, "ospDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
      },
      null);

    if (exists)
    {
      throw DataError("Restrict violation on table \"CKT_ASSGN_MNT_ACT\".",
        "50001");
    }

    exists = Read("DeleteOfficeServiceProvider#8",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "spdId", entities.OfficeServiceProvider.SpdGeneratedId);
        db.SetNullableInt32(
          command, "offId", entities.OfficeServiceProvider.OffGeneratedId);
        db.SetNullableString(
          command, "ospRoleCode", entities.OfficeServiceProvider.RoleCode);
        db.SetNullableDate(
          command, "ospDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
      },
      null);

    if (exists)
    {
      throw DataError("Restrict violation on table \"CKT_ASSGN_OBG\".", "50001");
        
    }

    exists = Read("DeleteOfficeServiceProvider#9",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "spdId", entities.OfficeServiceProvider.SpdGeneratedId);
        db.SetNullableInt32(
          command, "offId", entities.OfficeServiceProvider.OffGeneratedId);
        db.SetNullableString(
          command, "ospRoleCode", entities.OfficeServiceProvider.RoleCode);
        db.SetNullableDate(
          command, "ospDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
      },
      null);

    if (exists)
    {
      throw DataError("Restrict violation on table \"CKT_ASSGN_OBG_AA\".",
        "50001");
    }

    exists = Read("DeleteOfficeServiceProvider#10",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "spdId", entities.OfficeServiceProvider.SpdGeneratedId);
        db.SetNullableInt32(
          command, "offId", entities.OfficeServiceProvider.OffGeneratedId);
        db.SetNullableString(
          command, "ospRoleCode", entities.OfficeServiceProvider.RoleCode);
        db.SetNullableDate(
          command, "ospDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
      },
      null);

    if (exists)
    {
      throw DataError("Restrict violation on table \"CKT_ASSGN_PA_REF\".",
        "50001");
    }

    exists = Read("DeleteOfficeServiceProvider#11",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "spdId", entities.OfficeServiceProvider.SpdGeneratedId);
        db.SetNullableInt32(
          command, "offId", entities.OfficeServiceProvider.OffGeneratedId);
        db.SetNullableString(
          command, "ospRoleCode", entities.OfficeServiceProvider.RoleCode);
        db.SetNullableDate(
          command, "ospDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
      },
      null);

    if (exists)
    {
      throw DataError("Restrict violation on table \"CKT_ASSIGN_CASE\".",
        "50001");
    }

    Update("DeleteOfficeServiceProvider#12",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "spdId", entities.OfficeServiceProvider.SpdGeneratedId);
        db.SetNullableInt32(
          command, "offId", entities.OfficeServiceProvider.OffGeneratedId);
        db.SetNullableString(
          command, "ospRoleCode", entities.OfficeServiceProvider.RoleCode);
        db.SetNullableDate(
          command, "ospDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
      });

    Update("DeleteOfficeServiceProvider#13",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "spdId", entities.OfficeServiceProvider.SpdGeneratedId);
        db.SetNullableInt32(
          command, "offId", entities.OfficeServiceProvider.OffGeneratedId);
        db.SetNullableString(
          command, "ospRoleCode", entities.OfficeServiceProvider.RoleCode);
        db.SetNullableDate(
          command, "ospDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
      });

    Update("DeleteOfficeServiceProvider#14",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "spdId", entities.OfficeServiceProvider.SpdGeneratedId);
        db.SetNullableInt32(
          command, "offId", entities.OfficeServiceProvider.OffGeneratedId);
        db.SetNullableString(
          command, "ospRoleCode", entities.OfficeServiceProvider.RoleCode);
        db.SetNullableDate(
          command, "ospDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
      });

    exists = Read("DeleteOfficeServiceProvider#15",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "spdId", entities.OfficeServiceProvider.SpdGeneratedId);
        db.SetNullableInt32(
          command, "offId", entities.OfficeServiceProvider.OffGeneratedId);
        db.SetNullableString(
          command, "ospRoleCode", entities.OfficeServiceProvider.RoleCode);
        db.SetNullableDate(
          command, "ospDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
      },
      null);

    if (exists)
    {
      throw DataError("Restrict violation on table \"CKT_PA_REFERRAL\".",
        "50001");
    }

    exists = Read("DeleteOfficeServiceProvider#16",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "spdId", entities.OfficeServiceProvider.SpdGeneratedId);
        db.SetNullableInt32(
          command, "offId", entities.OfficeServiceProvider.OffGeneratedId);
        db.SetNullableString(
          command, "ospRoleCode", entities.OfficeServiceProvider.RoleCode);
        db.SetNullableDate(
          command, "ospDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
      },
      null);

    if (exists)
    {
      throw DataError("Restrict violation on table \"CKT_POT_RECOVERY\".",
        "50001");
    }

    exists = Read("DeleteOfficeServiceProvider#17",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "spdId", entities.OfficeServiceProvider.SpdGeneratedId);
        db.SetNullableInt32(
          command, "offId", entities.OfficeServiceProvider.OffGeneratedId);
        db.SetNullableString(
          command, "ospRoleCode", entities.OfficeServiceProvider.RoleCode);
        db.SetNullableDate(
          command, "ospDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
      },
      null);

    if (exists)
    {
      throw DataError("Restrict violation on table \"CKT_MONITORED_DOC\".",
        "50001");
    }

    Update("DeleteOfficeServiceProvider#18",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "spdId", entities.OfficeServiceProvider.SpdGeneratedId);
        db.SetNullableInt32(
          command, "offId", entities.OfficeServiceProvider.OffGeneratedId);
        db.SetNullableString(
          command, "ospRoleCode", entities.OfficeServiceProvider.RoleCode);
        db.SetNullableDate(
          command, "ospDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
      });
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
      },
      (db, reader) =>
      {
        entities.AdministrativeAppealAssignment.ReasonCode =
          db.GetString(reader, 0);
        entities.AdministrativeAppealAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.AdministrativeAppealAssignment.SpdId = db.GetInt32(reader, 2);
        entities.AdministrativeAppealAssignment.OffId = db.GetInt32(reader, 3);
        entities.AdministrativeAppealAssignment.OspCode =
          db.GetString(reader, 4);
        entities.AdministrativeAppealAssignment.OspDate = db.GetDate(reader, 5);
        entities.AdministrativeAppealAssignment.AapId = db.GetInt32(reader, 6);
        entities.AdministrativeAppealAssignment.Populated = true;
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
      },
      (db, reader) =>
      {
        entities.CaseAssignment.ReasonCode = db.GetString(reader, 0);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 2);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 3);
        entities.CaseAssignment.OspCode = db.GetString(reader, 4);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 5);
        entities.CaseAssignment.CasNo = db.GetString(reader, 6);
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
      },
      (db, reader) =>
      {
        entities.CaseUnitFunctionAssignmt.ReasonCode = db.GetString(reader, 0);
        entities.CaseUnitFunctionAssignmt.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.CaseUnitFunctionAssignmt.SpdId = db.GetInt32(reader, 2);
        entities.CaseUnitFunctionAssignmt.OffId = db.GetInt32(reader, 3);
        entities.CaseUnitFunctionAssignmt.OspCode = db.GetString(reader, 4);
        entities.CaseUnitFunctionAssignmt.OspDate = db.GetDate(reader, 5);
        entities.CaseUnitFunctionAssignmt.CsuNo = db.GetInt32(reader, 6);
        entities.CaseUnitFunctionAssignmt.CasNo = db.GetString(reader, 7);
        entities.CaseUnitFunctionAssignmt.Populated = true;
      });
  }

  private bool ReadGeneticTestAccount()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.GeneticTestAccount.Populated = false;

    return Read("ReadGeneticTestAccount",
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
      },
      (db, reader) =>
      {
        entities.GeneticTestAccount.AccountNumber = db.GetString(reader, 0);
        entities.GeneticTestAccount.OspEffectiveDate =
          db.GetNullableDate(reader, 1);
        entities.GeneticTestAccount.OspRoleCode =
          db.GetNullableString(reader, 2);
        entities.GeneticTestAccount.OffGeneratedId =
          db.GetNullableInt32(reader, 3);
        entities.GeneticTestAccount.SpdGeneratedId =
          db.GetNullableInt32(reader, 4);
        entities.GeneticTestAccount.Populated = true;
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
      },
      (db, reader) =>
      {
        entities.InformationRequestAssignment.ReasonCode =
          db.GetString(reader, 0);
        entities.InformationRequestAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.InformationRequestAssignment.SpdId = db.GetInt32(reader, 2);
        entities.InformationRequestAssignment.OffId = db.GetInt32(reader, 3);
        entities.InformationRequestAssignment.OspCode = db.GetString(reader, 4);
        entities.InformationRequestAssignment.OspDate = db.GetDate(reader, 5);
        entities.InformationRequestAssignment.InqNo = db.GetInt64(reader, 6);
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
      },
      (db, reader) =>
      {
        entities.InterstateCaseAssignment.ReasonCode = db.GetString(reader, 0);
        entities.InterstateCaseAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.InterstateCaseAssignment.SpdId = db.GetInt32(reader, 2);
        entities.InterstateCaseAssignment.OffId = db.GetInt32(reader, 3);
        entities.InterstateCaseAssignment.OspCode = db.GetString(reader, 4);
        entities.InterstateCaseAssignment.OspDate = db.GetDate(reader, 5);
        entities.InterstateCaseAssignment.IcsDate = db.GetDate(reader, 6);
        entities.InterstateCaseAssignment.IcsNo = db.GetInt64(reader, 7);
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
        entities.LegalActionAssigment.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.LegalActionAssigment.Populated = true;
      });
  }

  private bool ReadLegalReferralAssignment()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.LegalReferralAssignment.Populated = false;

    return Read("ReadLegalReferralAssignment",
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
      },
      (db, reader) =>
      {
        entities.LegalReferralAssignment.ReasonCode = db.GetString(reader, 0);
        entities.LegalReferralAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.LegalReferralAssignment.SpdId = db.GetInt32(reader, 2);
        entities.LegalReferralAssignment.OffId = db.GetInt32(reader, 3);
        entities.LegalReferralAssignment.OspCode = db.GetString(reader, 4);
        entities.LegalReferralAssignment.OspDate = db.GetDate(reader, 5);
        entities.LegalReferralAssignment.CasNo = db.GetString(reader, 6);
        entities.LegalReferralAssignment.LgrId = db.GetInt32(reader, 7);
        entities.LegalReferralAssignment.Populated = true;
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
      },
      (db, reader) =>
      {
        entities.MonitoredActivityAssignment.ReasonCode =
          db.GetString(reader, 0);
        entities.MonitoredActivityAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.MonitoredActivityAssignment.SpdId = db.GetInt32(reader, 2);
        entities.MonitoredActivityAssignment.OffId = db.GetInt32(reader, 3);
        entities.MonitoredActivityAssignment.OspCode = db.GetString(reader, 4);
        entities.MonitoredActivityAssignment.OspDate = db.GetDate(reader, 5);
        entities.MonitoredActivityAssignment.MacId = db.GetInt32(reader, 6);
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
      },
      (db, reader) =>
      {
        entities.ObligationAdminActionAssgn.ReasonCode =
          db.GetString(reader, 0);
        entities.ObligationAdminActionAssgn.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.ObligationAdminActionAssgn.SpdId = db.GetInt32(reader, 2);
        entities.ObligationAdminActionAssgn.OffId = db.GetInt32(reader, 3);
        entities.ObligationAdminActionAssgn.OspCode = db.GetString(reader, 4);
        entities.ObligationAdminActionAssgn.OspDate = db.GetDate(reader, 5);
        entities.ObligationAdminActionAssgn.OtyId = db.GetInt32(reader, 6);
        entities.ObligationAdminActionAssgn.AatType = db.GetString(reader, 7);
        entities.ObligationAdminActionAssgn.ObgId = db.GetInt32(reader, 8);
        entities.ObligationAdminActionAssgn.CspNo = db.GetString(reader, 9);
        entities.ObligationAdminActionAssgn.CpaType = db.GetString(reader, 10);
        entities.ObligationAdminActionAssgn.OaaDate = db.GetDate(reader, 11);
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
      },
      (db, reader) =>
      {
        entities.ObligationAssignment.ReasonCode = db.GetString(reader, 0);
        entities.ObligationAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.ObligationAssignment.SpdId = db.GetInt32(reader, 2);
        entities.ObligationAssignment.OffId = db.GetInt32(reader, 3);
        entities.ObligationAssignment.OspCode = db.GetString(reader, 4);
        entities.ObligationAssignment.OspDate = db.GetDate(reader, 5);
        entities.ObligationAssignment.OtyId = db.GetInt32(reader, 6);
        entities.ObligationAssignment.CpaType = db.GetString(reader, 7);
        entities.ObligationAssignment.CspNo = db.GetString(reader, 8);
        entities.ObligationAssignment.ObgId = db.GetInt32(reader, 9);
        entities.ObligationAssignment.Populated = true;
        CheckValid<ObligationAssignment>("CpaType",
          entities.ObligationAssignment.CpaType);
      });
  }

  private bool ReadOfficeCaseloadAssignment()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.OfficeCaseloadAssignment.Populated = false;

    return Read("ReadOfficeCaseloadAssignment",
      (db, command) =>
      {
        db.SetNullableString(
          command, "ospRoleCode", entities.OfficeServiceProvider.RoleCode);
        db.SetNullableDate(
          command, "ospEffectiveDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.SetNullableInt32(
          command, "offDGeneratedId",
          entities.OfficeServiceProvider.OffGeneratedId);
        db.SetNullableInt32(
          command, "spdGeneratedId",
          entities.OfficeServiceProvider.SpdGeneratedId);
      },
      (db, reader) =>
      {
        entities.OfficeCaseloadAssignment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.OfficeCaseloadAssignment.EffectiveDate = db.GetDate(reader, 1);
        entities.OfficeCaseloadAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.OfficeCaseloadAssignment.OspEffectiveDate =
          db.GetNullableDate(reader, 3);
        entities.OfficeCaseloadAssignment.OspRoleCode =
          db.GetNullableString(reader, 4);
        entities.OfficeCaseloadAssignment.OffDGeneratedId =
          db.GetNullableInt32(reader, 5);
        entities.OfficeCaseloadAssignment.SpdGeneratedId =
          db.GetNullableInt32(reader, 6);
        entities.OfficeCaseloadAssignment.Populated = true;
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
        entities.OfficeServiceProvRelationship.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvider()
  {
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGeneratedId", import.ServiceProvider.SystemGeneratedId);
        db.SetInt32(command, "offGeneratedId", import.Office.SystemGeneratedId);
        db.
          SetString(command, "roleCode", import.OfficeServiceProvider.RoleCode);
          
        db.SetDate(
          command, "effectiveDate",
          import.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.WorkPhoneNumber = db.GetInt32(reader, 4);
        entities.OfficeServiceProvider.WorkFaxNumber =
          db.GetNullableInt32(reader, 5);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.OfficeServiceProvider.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.OfficeServiceProvider.LastUpdatedDtstamp =
          db.GetNullableDateTime(reader, 8);
        entities.OfficeServiceProvider.CreatedBy = db.GetString(reader, 9);
        entities.OfficeServiceProvider.CreatedTimestamp =
          db.GetDateTime(reader, 10);
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
        entities.OfficeServiceProviderAlert.SpdId =
          db.GetNullableInt32(reader, 1);
        entities.OfficeServiceProviderAlert.OffId =
          db.GetNullableInt32(reader, 2);
        entities.OfficeServiceProviderAlert.OspCode =
          db.GetNullableString(reader, 3);
        entities.OfficeServiceProviderAlert.OspDate =
          db.GetNullableDate(reader, 4);
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
      },
      (db, reader) =>
      {
        entities.PaReferralAssignment.ReasonCode = db.GetString(reader, 0);
        entities.PaReferralAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.PaReferralAssignment.SpdId = db.GetInt32(reader, 2);
        entities.PaReferralAssignment.OffId = db.GetInt32(reader, 3);
        entities.PaReferralAssignment.OspCode = db.GetString(reader, 4);
        entities.PaReferralAssignment.OspDate = db.GetDate(reader, 5);
        entities.PaReferralAssignment.PafNo = db.GetString(reader, 6);
        entities.PaReferralAssignment.PafType = db.GetString(reader, 7);
        entities.PaReferralAssignment.PafTstamp = db.GetDateTime(reader, 8);
        entities.PaReferralAssignment.Populated = true;
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
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of CseOrganization.
    /// </summary>
    [JsonPropertyName("cseOrganization")]
    public CseOrganization CseOrganization
    {
      get => cseOrganization ??= new();
      set => cseOrganization = value;
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

    private OfficeServiceProvider officeServiceProvider;
    private CseOrganization cseOrganization;
    private Office office;
    private ServiceProvider serviceProvider;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    private OfficeServiceProvider officeServiceProvider;
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
    /// A value of OfficeServiceProviderAlert.
    /// </summary>
    [JsonPropertyName("officeServiceProviderAlert")]
    public OfficeServiceProviderAlert OfficeServiceProviderAlert
    {
      get => officeServiceProviderAlert ??= new();
      set => officeServiceProviderAlert = value;
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
    /// A value of InformationRequestAssignment.
    /// </summary>
    [JsonPropertyName("informationRequestAssignment")]
    public InformationRequestAssignment InformationRequestAssignment
    {
      get => informationRequestAssignment ??= new();
      set => informationRequestAssignment = value;
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
    /// A value of MonitoredActivityAssignment.
    /// </summary>
    [JsonPropertyName("monitoredActivityAssignment")]
    public MonitoredActivityAssignment MonitoredActivityAssignment
    {
      get => monitoredActivityAssignment ??= new();
      set => monitoredActivityAssignment = value;
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
    /// A value of InterstateCaseAssignment.
    /// </summary>
    [JsonPropertyName("interstateCaseAssignment")]
    public InterstateCaseAssignment InterstateCaseAssignment
    {
      get => interstateCaseAssignment ??= new();
      set => interstateCaseAssignment = value;
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
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
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
    /// A value of GeneticTestAccount.
    /// </summary>
    [JsonPropertyName("geneticTestAccount")]
    public GeneticTestAccount GeneticTestAccount
    {
      get => geneticTestAccount ??= new();
      set => geneticTestAccount = value;
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
    /// A value of OfficeServiceProvRelationship.
    /// </summary>
    [JsonPropertyName("officeServiceProvRelationship")]
    public OfficeServiceProvRelationship OfficeServiceProvRelationship
    {
      get => officeServiceProvRelationship ??= new();
      set => officeServiceProvRelationship = value;
    }

    /// <summary>
    /// A value of OfficeCaseloadAssignment.
    /// </summary>
    [JsonPropertyName("officeCaseloadAssignment")]
    public OfficeCaseloadAssignment OfficeCaseloadAssignment
    {
      get => officeCaseloadAssignment ??= new();
      set => officeCaseloadAssignment = value;
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
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    private OfficeServiceProviderAlert officeServiceProviderAlert;
    private PaReferralAssignment paReferralAssignment;
    private InformationRequestAssignment informationRequestAssignment;
    private ObligationAssignment obligationAssignment;
    private ObligationAdminActionAssgn obligationAdminActionAssgn;
    private MonitoredActivityAssignment monitoredActivityAssignment;
    private LegalReferralAssignment legalReferralAssignment;
    private InterstateCaseAssignment interstateCaseAssignment;
    private CaseUnitFunctionAssignmt caseUnitFunctionAssignmt;
    private AdministrativeAppealAssignment administrativeAppealAssignment;
    private CaseAssignment caseAssignment;
    private LegalReferral legalReferral;
    private GeneticTestAccount geneticTestAccount;
    private LegalActionAssigment legalActionAssigment;
    private OfficeServiceProvRelationship officeServiceProvRelationship;
    private OfficeCaseloadAssignment officeCaseloadAssignment;
    private Office office;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
  }
#endregion
}
