// Program: SP_CREATE_GLOBAL_REASSIGNMENT, ID: 372453375, model: 746.
// Short name: SWE02196
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_CREATE_GLOBAL_REASSIGNMENT.
/// </para>
/// <para>
/// This action block creates a new occurrence of the entity Global 
/// Reassignment.
/// </para>
/// </summary>
[Serializable]
public partial class SpCreateGlobalReassignment: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CREATE_GLOBAL_REASSIGNMENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCreateGlobalReassignment(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCreateGlobalReassignment.
  /// </summary>
  public SpCreateGlobalReassignment(IContext context, Import import,
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
    // ** Initial development - J. Rookard, MTW  01/27/98
    // ** 11/08/18 R.Mathews   CQ61772  Change GBOR to be able to select a 
    // caseload
    // **
    // 
    // type (CAS, LEA, PAR) rather
    // than each
    // **
    // 
    // individual business object
    // type.  Default
    // **
    // 
    // assignment reason code to RSP.
    if (!ReadOffice1())
    {
      ExitState = "OFFICE_NF";

      return;
    }

    if (!ReadServiceProvider1())
    {
      ExitState = "SERVICE_PROVIDER_NF";

      return;
    }

    if (!ReadOfficeServiceProvider1())
    {
      ExitState = "SP0000_OFFICE_SERVICE_PROVIDR_NF";

      return;
    }

    if (ReadOffice2())
    {
      export.NewOffice.SystemGeneratedId = entities.NewOffice.SystemGeneratedId;
    }
    else
    {
      ExitState = "OFFICE_NF";

      return;
    }

    if (ReadServiceProvider2())
    {
      export.NewServiceProvider.SystemGeneratedId =
        entities.NewServiceProvider.SystemGeneratedId;
    }
    else
    {
      ExitState = "SERVICE_PROVIDER_NF";

      return;
    }

    if (ReadOfficeServiceProvider2())
    {
      MoveOfficeServiceProvider(entities.NewOfficeServiceProvider,
        export.NewOfficeServiceProvider);
    }
    else
    {
      ExitState = "SP0000_OFFICE_SERVICE_PROVIDR_NF";

      return;
    }

    try
    {
      CreateGlobalReassignment1();
      MoveGlobalReassignment(entities.GlobalReassignment,
        export.GlobalReassignment);
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "SP0000_GLOBAL_REASSIGNMENT_AE";

          return;
        case ErrorCode.PermittedValueViolation:
          ExitState = "SP0000_GLOBAL_REASSIGNMENT_PV";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    if (Equal(import.GlobalReassignment.BusinessObjectCode, "CAS"))
    {
      try
      {
        CreateGlobalReassignment4();

        try
        {
          CreateGlobalReassignment3();
        }
        catch(Exception e1)
        {
          switch(GetErrorCode(e1))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "SP0000_GLOBAL_REASSIGNMENT_AE";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "SP0000_GLOBAL_REASSIGNMENT_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "SP0000_GLOBAL_REASSIGNMENT_AE";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "SP0000_GLOBAL_REASSIGNMENT_PV";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    if (Equal(import.GlobalReassignment.BusinessObjectCode, "LEA"))
    {
      try
      {
        CreateGlobalReassignment5();

        try
        {
          CreateGlobalReassignment2();
        }
        catch(Exception e1)
        {
          switch(GetErrorCode(e1))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "SP0000_GLOBAL_REASSIGNMENT_AE";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "SP0000_GLOBAL_REASSIGNMENT_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "SP0000_GLOBAL_REASSIGNMENT_AE";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "SP0000_GLOBAL_REASSIGNMENT_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
  }

  private static void MoveGlobalReassignment(GlobalReassignment source,
    GlobalReassignment target)
  {
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.CreatedBy = source.CreatedBy;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.ProcessDate = source.ProcessDate;
    target.StatusFlag = source.StatusFlag;
    target.OverrideFlag = source.OverrideFlag;
    target.BusinessObjectCode = source.BusinessObjectCode;
    target.AssignmentReasonCode = source.AssignmentReasonCode;
    target.BoCount = source.BoCount;
    target.MonCount = source.MonCount;
  }

  private static void MoveOfficeServiceProvider(OfficeServiceProvider source,
    OfficeServiceProvider target)
  {
    target.RoleCode = source.RoleCode;
    target.EffectiveDate = source.EffectiveDate;
  }

  private void CreateGlobalReassignment1()
  {
    System.Diagnostics.Debug.
      Assert(entities.NewOfficeServiceProvider.Populated);
    System.Diagnostics.Debug.Assert(
      entities.ExistingOfficeServiceProvider.Populated);

    var createdTimestamp = Now();
    var createdBy = global.UserId;
    var processDate = import.GlobalReassignment.ProcessDate;
    var statusFlag = "Q";
    var overrideFlag = import.GlobalReassignment.OverrideFlag;
    var businessObjectCode = import.GlobalReassignment.BusinessObjectCode;
    var assignmentReasonCode = import.GlobalReassignment.AssignmentReasonCode;
    var spdGeneratedId = entities.NewOfficeServiceProvider.SpdGeneratedId;
    var offGeneratedId = entities.NewOfficeServiceProvider.OffGeneratedId;
    var ospRoleCode = entities.NewOfficeServiceProvider.RoleCode;
    var ospEffectiveDate = entities.NewOfficeServiceProvider.EffectiveDate;
    var spdGeneratedId1 = entities.ExistingOfficeServiceProvider.SpdGeneratedId;
    var offGeneratedId1 = entities.ExistingOfficeServiceProvider.OffGeneratedId;
    var ospRoleCod = entities.ExistingOfficeServiceProvider.RoleCode;
    var ospEffectiveDat = entities.ExistingOfficeServiceProvider.EffectiveDate;

    entities.GlobalReassignment.Populated = false;
    Update("CreateGlobalReassignment1",
      (db, command) =>
      {
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "createdBy", createdBy);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", null);
        db.SetDate(command, "processDate", processDate);
        db.SetString(command, "statusFlag", statusFlag);
        db.SetString(command, "overrideFlag", overrideFlag);
        db.SetString(command, "businessObjCode", businessObjectCode);
        db.SetString(command, "assignReasonCode", assignmentReasonCode);
        db.SetNullableInt32(command, "spdGeneratedId", spdGeneratedId);
        db.SetNullableInt32(command, "offGeneratedId", offGeneratedId);
        db.SetNullableString(command, "ospRoleCode", ospRoleCode);
        db.SetNullableDate(command, "ospEffectiveDate", ospEffectiveDate);
        db.SetNullableInt32(command, "spdGeneratedId1", spdGeneratedId1);
        db.SetNullableInt32(command, "offGeneratedId1", offGeneratedId1);
        db.SetNullableString(command, "ospRoleCod", ospRoleCod);
        db.SetNullableDate(command, "ospEffectiveDat", ospEffectiveDat);
        db.SetNullableInt32(command, "boCount", 0);
        db.SetNullableInt32(command, "monCount", 0);
      });

    entities.GlobalReassignment.CreatedTimestamp = createdTimestamp;
    entities.GlobalReassignment.CreatedBy = createdBy;
    entities.GlobalReassignment.LastUpdatedBy = "";
    entities.GlobalReassignment.LastUpdatedTimestamp = null;
    entities.GlobalReassignment.ProcessDate = processDate;
    entities.GlobalReassignment.StatusFlag = statusFlag;
    entities.GlobalReassignment.OverrideFlag = overrideFlag;
    entities.GlobalReassignment.BusinessObjectCode = businessObjectCode;
    entities.GlobalReassignment.AssignmentReasonCode = assignmentReasonCode;
    entities.GlobalReassignment.SpdGeneratedId = spdGeneratedId;
    entities.GlobalReassignment.OffGeneratedId = offGeneratedId;
    entities.GlobalReassignment.OspRoleCode = ospRoleCode;
    entities.GlobalReassignment.OspEffectiveDate = ospEffectiveDate;
    entities.GlobalReassignment.SpdGeneratedId1 = spdGeneratedId1;
    entities.GlobalReassignment.OffGeneratedId1 = offGeneratedId1;
    entities.GlobalReassignment.OspRoleCod = ospRoleCod;
    entities.GlobalReassignment.OspEffectiveDat = ospEffectiveDat;
    entities.GlobalReassignment.BoCount = 0;
    entities.GlobalReassignment.MonCount = 0;
    entities.GlobalReassignment.Populated = true;
  }

  private void CreateGlobalReassignment2()
  {
    System.Diagnostics.Debug.
      Assert(entities.NewOfficeServiceProvider.Populated);
    System.Diagnostics.Debug.Assert(
      entities.ExistingOfficeServiceProvider.Populated);

    var createdTimestamp = Now();
    var createdBy = global.UserId;
    var processDate = import.GlobalReassignment.ProcessDate;
    var statusFlag = "Q";
    var overrideFlag = import.GlobalReassignment.OverrideFlag;
    var businessObjectCode = "ADA";
    var assignmentReasonCode = import.GlobalReassignment.AssignmentReasonCode;
    var spdGeneratedId = entities.NewOfficeServiceProvider.SpdGeneratedId;
    var offGeneratedId = entities.NewOfficeServiceProvider.OffGeneratedId;
    var ospRoleCode = entities.NewOfficeServiceProvider.RoleCode;
    var ospEffectiveDate = entities.NewOfficeServiceProvider.EffectiveDate;
    var spdGeneratedId1 = entities.ExistingOfficeServiceProvider.SpdGeneratedId;
    var offGeneratedId1 = entities.ExistingOfficeServiceProvider.OffGeneratedId;
    var ospRoleCod = entities.ExistingOfficeServiceProvider.RoleCode;
    var ospEffectiveDat = entities.ExistingOfficeServiceProvider.EffectiveDate;

    entities.GlobalReassignment.Populated = false;
    Update("CreateGlobalReassignment2",
      (db, command) =>
      {
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "createdBy", createdBy);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", null);
        db.SetDate(command, "processDate", processDate);
        db.SetString(command, "statusFlag", statusFlag);
        db.SetString(command, "overrideFlag", overrideFlag);
        db.SetString(command, "businessObjCode", businessObjectCode);
        db.SetString(command, "assignReasonCode", assignmentReasonCode);
        db.SetNullableInt32(command, "spdGeneratedId", spdGeneratedId);
        db.SetNullableInt32(command, "offGeneratedId", offGeneratedId);
        db.SetNullableString(command, "ospRoleCode", ospRoleCode);
        db.SetNullableDate(command, "ospEffectiveDate", ospEffectiveDate);
        db.SetNullableInt32(command, "spdGeneratedId1", spdGeneratedId1);
        db.SetNullableInt32(command, "offGeneratedId1", offGeneratedId1);
        db.SetNullableString(command, "ospRoleCod", ospRoleCod);
        db.SetNullableDate(command, "ospEffectiveDat", ospEffectiveDat);
        db.SetNullableInt32(command, "boCount", 0);
        db.SetNullableInt32(command, "monCount", 0);
      });

    entities.GlobalReassignment.CreatedTimestamp = createdTimestamp;
    entities.GlobalReassignment.CreatedBy = createdBy;
    entities.GlobalReassignment.LastUpdatedBy = "";
    entities.GlobalReassignment.LastUpdatedTimestamp = null;
    entities.GlobalReassignment.ProcessDate = processDate;
    entities.GlobalReassignment.StatusFlag = statusFlag;
    entities.GlobalReassignment.OverrideFlag = overrideFlag;
    entities.GlobalReassignment.BusinessObjectCode = businessObjectCode;
    entities.GlobalReassignment.AssignmentReasonCode = assignmentReasonCode;
    entities.GlobalReassignment.SpdGeneratedId = spdGeneratedId;
    entities.GlobalReassignment.OffGeneratedId = offGeneratedId;
    entities.GlobalReassignment.OspRoleCode = ospRoleCode;
    entities.GlobalReassignment.OspEffectiveDate = ospEffectiveDate;
    entities.GlobalReassignment.SpdGeneratedId1 = spdGeneratedId1;
    entities.GlobalReassignment.OffGeneratedId1 = offGeneratedId1;
    entities.GlobalReassignment.OspRoleCod = ospRoleCod;
    entities.GlobalReassignment.OspEffectiveDat = ospEffectiveDat;
    entities.GlobalReassignment.BoCount = 0;
    entities.GlobalReassignment.MonCount = 0;
    entities.GlobalReassignment.Populated = true;
  }

  private void CreateGlobalReassignment3()
  {
    System.Diagnostics.Debug.
      Assert(entities.NewOfficeServiceProvider.Populated);
    System.Diagnostics.Debug.Assert(
      entities.ExistingOfficeServiceProvider.Populated);

    var createdTimestamp = Now();
    var createdBy = global.UserId;
    var processDate = import.GlobalReassignment.ProcessDate;
    var statusFlag = "Q";
    var overrideFlag = import.GlobalReassignment.OverrideFlag;
    var businessObjectCode = "INC";
    var assignmentReasonCode = import.GlobalReassignment.AssignmentReasonCode;
    var spdGeneratedId = entities.NewOfficeServiceProvider.SpdGeneratedId;
    var offGeneratedId = entities.NewOfficeServiceProvider.OffGeneratedId;
    var ospRoleCode = entities.NewOfficeServiceProvider.RoleCode;
    var ospEffectiveDate = entities.NewOfficeServiceProvider.EffectiveDate;
    var spdGeneratedId1 = entities.ExistingOfficeServiceProvider.SpdGeneratedId;
    var offGeneratedId1 = entities.ExistingOfficeServiceProvider.OffGeneratedId;
    var ospRoleCod = entities.ExistingOfficeServiceProvider.RoleCode;
    var ospEffectiveDat = entities.ExistingOfficeServiceProvider.EffectiveDate;

    entities.GlobalReassignment.Populated = false;
    Update("CreateGlobalReassignment3",
      (db, command) =>
      {
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "createdBy", createdBy);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", null);
        db.SetDate(command, "processDate", processDate);
        db.SetString(command, "statusFlag", statusFlag);
        db.SetString(command, "overrideFlag", overrideFlag);
        db.SetString(command, "businessObjCode", businessObjectCode);
        db.SetString(command, "assignReasonCode", assignmentReasonCode);
        db.SetNullableInt32(command, "spdGeneratedId", spdGeneratedId);
        db.SetNullableInt32(command, "offGeneratedId", offGeneratedId);
        db.SetNullableString(command, "ospRoleCode", ospRoleCode);
        db.SetNullableDate(command, "ospEffectiveDate", ospEffectiveDate);
        db.SetNullableInt32(command, "spdGeneratedId1", spdGeneratedId1);
        db.SetNullableInt32(command, "offGeneratedId1", offGeneratedId1);
        db.SetNullableString(command, "ospRoleCod", ospRoleCod);
        db.SetNullableDate(command, "ospEffectiveDat", ospEffectiveDat);
        db.SetNullableInt32(command, "boCount", 0);
        db.SetNullableInt32(command, "monCount", 0);
      });

    entities.GlobalReassignment.CreatedTimestamp = createdTimestamp;
    entities.GlobalReassignment.CreatedBy = createdBy;
    entities.GlobalReassignment.LastUpdatedBy = "";
    entities.GlobalReassignment.LastUpdatedTimestamp = null;
    entities.GlobalReassignment.ProcessDate = processDate;
    entities.GlobalReassignment.StatusFlag = statusFlag;
    entities.GlobalReassignment.OverrideFlag = overrideFlag;
    entities.GlobalReassignment.BusinessObjectCode = businessObjectCode;
    entities.GlobalReassignment.AssignmentReasonCode = assignmentReasonCode;
    entities.GlobalReassignment.SpdGeneratedId = spdGeneratedId;
    entities.GlobalReassignment.OffGeneratedId = offGeneratedId;
    entities.GlobalReassignment.OspRoleCode = ospRoleCode;
    entities.GlobalReassignment.OspEffectiveDate = ospEffectiveDate;
    entities.GlobalReassignment.SpdGeneratedId1 = spdGeneratedId1;
    entities.GlobalReassignment.OffGeneratedId1 = offGeneratedId1;
    entities.GlobalReassignment.OspRoleCod = ospRoleCod;
    entities.GlobalReassignment.OspEffectiveDat = ospEffectiveDat;
    entities.GlobalReassignment.BoCount = 0;
    entities.GlobalReassignment.MonCount = 0;
    entities.GlobalReassignment.Populated = true;
  }

  private void CreateGlobalReassignment4()
  {
    System.Diagnostics.Debug.
      Assert(entities.NewOfficeServiceProvider.Populated);
    System.Diagnostics.Debug.Assert(
      entities.ExistingOfficeServiceProvider.Populated);

    var createdTimestamp = Now();
    var createdBy = global.UserId;
    var processDate = import.GlobalReassignment.ProcessDate;
    var statusFlag = "Q";
    var overrideFlag = import.GlobalReassignment.OverrideFlag;
    var businessObjectCode = "LEA";
    var assignmentReasonCode = import.GlobalReassignment.AssignmentReasonCode;
    var spdGeneratedId = entities.NewOfficeServiceProvider.SpdGeneratedId;
    var offGeneratedId = entities.NewOfficeServiceProvider.OffGeneratedId;
    var ospRoleCode = entities.NewOfficeServiceProvider.RoleCode;
    var ospEffectiveDate = entities.NewOfficeServiceProvider.EffectiveDate;
    var spdGeneratedId1 = entities.ExistingOfficeServiceProvider.SpdGeneratedId;
    var offGeneratedId1 = entities.ExistingOfficeServiceProvider.OffGeneratedId;
    var ospRoleCod = entities.ExistingOfficeServiceProvider.RoleCode;
    var ospEffectiveDat = entities.ExistingOfficeServiceProvider.EffectiveDate;

    entities.GlobalReassignment.Populated = false;
    Update("CreateGlobalReassignment4",
      (db, command) =>
      {
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "createdBy", createdBy);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", null);
        db.SetDate(command, "processDate", processDate);
        db.SetString(command, "statusFlag", statusFlag);
        db.SetString(command, "overrideFlag", overrideFlag);
        db.SetString(command, "businessObjCode", businessObjectCode);
        db.SetString(command, "assignReasonCode", assignmentReasonCode);
        db.SetNullableInt32(command, "spdGeneratedId", spdGeneratedId);
        db.SetNullableInt32(command, "offGeneratedId", offGeneratedId);
        db.SetNullableString(command, "ospRoleCode", ospRoleCode);
        db.SetNullableDate(command, "ospEffectiveDate", ospEffectiveDate);
        db.SetNullableInt32(command, "spdGeneratedId1", spdGeneratedId1);
        db.SetNullableInt32(command, "offGeneratedId1", offGeneratedId1);
        db.SetNullableString(command, "ospRoleCod", ospRoleCod);
        db.SetNullableDate(command, "ospEffectiveDat", ospEffectiveDat);
        db.SetNullableInt32(command, "boCount", 0);
        db.SetNullableInt32(command, "monCount", 0);
      });

    entities.GlobalReassignment.CreatedTimestamp = createdTimestamp;
    entities.GlobalReassignment.CreatedBy = createdBy;
    entities.GlobalReassignment.LastUpdatedBy = "";
    entities.GlobalReassignment.LastUpdatedTimestamp = null;
    entities.GlobalReassignment.ProcessDate = processDate;
    entities.GlobalReassignment.StatusFlag = statusFlag;
    entities.GlobalReassignment.OverrideFlag = overrideFlag;
    entities.GlobalReassignment.BusinessObjectCode = businessObjectCode;
    entities.GlobalReassignment.AssignmentReasonCode = assignmentReasonCode;
    entities.GlobalReassignment.SpdGeneratedId = spdGeneratedId;
    entities.GlobalReassignment.OffGeneratedId = offGeneratedId;
    entities.GlobalReassignment.OspRoleCode = ospRoleCode;
    entities.GlobalReassignment.OspEffectiveDate = ospEffectiveDate;
    entities.GlobalReassignment.SpdGeneratedId1 = spdGeneratedId1;
    entities.GlobalReassignment.OffGeneratedId1 = offGeneratedId1;
    entities.GlobalReassignment.OspRoleCod = ospRoleCod;
    entities.GlobalReassignment.OspEffectiveDat = ospEffectiveDat;
    entities.GlobalReassignment.BoCount = 0;
    entities.GlobalReassignment.MonCount = 0;
    entities.GlobalReassignment.Populated = true;
  }

  private void CreateGlobalReassignment5()
  {
    System.Diagnostics.Debug.
      Assert(entities.NewOfficeServiceProvider.Populated);
    System.Diagnostics.Debug.Assert(
      entities.ExistingOfficeServiceProvider.Populated);

    var createdTimestamp = Now();
    var createdBy = global.UserId;
    var processDate = import.GlobalReassignment.ProcessDate;
    var statusFlag = "Q";
    var overrideFlag = import.GlobalReassignment.OverrideFlag;
    var businessObjectCode = "LRF";
    var assignmentReasonCode = import.GlobalReassignment.AssignmentReasonCode;
    var spdGeneratedId = entities.NewOfficeServiceProvider.SpdGeneratedId;
    var offGeneratedId = entities.NewOfficeServiceProvider.OffGeneratedId;
    var ospRoleCode = entities.NewOfficeServiceProvider.RoleCode;
    var ospEffectiveDate = entities.NewOfficeServiceProvider.EffectiveDate;
    var spdGeneratedId1 = entities.ExistingOfficeServiceProvider.SpdGeneratedId;
    var offGeneratedId1 = entities.ExistingOfficeServiceProvider.OffGeneratedId;
    var ospRoleCod = entities.ExistingOfficeServiceProvider.RoleCode;
    var ospEffectiveDat = entities.ExistingOfficeServiceProvider.EffectiveDate;

    entities.GlobalReassignment.Populated = false;
    Update("CreateGlobalReassignment5",
      (db, command) =>
      {
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "createdBy", createdBy);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", null);
        db.SetDate(command, "processDate", processDate);
        db.SetString(command, "statusFlag", statusFlag);
        db.SetString(command, "overrideFlag", overrideFlag);
        db.SetString(command, "businessObjCode", businessObjectCode);
        db.SetString(command, "assignReasonCode", assignmentReasonCode);
        db.SetNullableInt32(command, "spdGeneratedId", spdGeneratedId);
        db.SetNullableInt32(command, "offGeneratedId", offGeneratedId);
        db.SetNullableString(command, "ospRoleCode", ospRoleCode);
        db.SetNullableDate(command, "ospEffectiveDate", ospEffectiveDate);
        db.SetNullableInt32(command, "spdGeneratedId1", spdGeneratedId1);
        db.SetNullableInt32(command, "offGeneratedId1", offGeneratedId1);
        db.SetNullableString(command, "ospRoleCod", ospRoleCod);
        db.SetNullableDate(command, "ospEffectiveDat", ospEffectiveDat);
        db.SetNullableInt32(command, "boCount", 0);
        db.SetNullableInt32(command, "monCount", 0);
      });

    entities.GlobalReassignment.CreatedTimestamp = createdTimestamp;
    entities.GlobalReassignment.CreatedBy = createdBy;
    entities.GlobalReassignment.LastUpdatedBy = "";
    entities.GlobalReassignment.LastUpdatedTimestamp = null;
    entities.GlobalReassignment.ProcessDate = processDate;
    entities.GlobalReassignment.StatusFlag = statusFlag;
    entities.GlobalReassignment.OverrideFlag = overrideFlag;
    entities.GlobalReassignment.BusinessObjectCode = businessObjectCode;
    entities.GlobalReassignment.AssignmentReasonCode = assignmentReasonCode;
    entities.GlobalReassignment.SpdGeneratedId = spdGeneratedId;
    entities.GlobalReassignment.OffGeneratedId = offGeneratedId;
    entities.GlobalReassignment.OspRoleCode = ospRoleCode;
    entities.GlobalReassignment.OspEffectiveDate = ospEffectiveDate;
    entities.GlobalReassignment.SpdGeneratedId1 = spdGeneratedId1;
    entities.GlobalReassignment.OffGeneratedId1 = offGeneratedId1;
    entities.GlobalReassignment.OspRoleCod = ospRoleCod;
    entities.GlobalReassignment.OspEffectiveDat = ospEffectiveDat;
    entities.GlobalReassignment.BoCount = 0;
    entities.GlobalReassignment.MonCount = 0;
    entities.GlobalReassignment.Populated = true;
  }

  private bool ReadOffice1()
  {
    entities.ExistingOffice.Populated = false;

    return Read("ReadOffice1",
      (db, command) =>
      {
        db.
          SetInt32(command, "officeId", import.ExistingOffice.SystemGeneratedId);
          
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
    entities.NewOffice.Populated = false;

    return Read("ReadOffice2",
      (db, command) =>
      {
        db.SetInt32(command, "officeId", import.NewOffice.SystemGeneratedId);
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
    entities.ExistingOfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider1",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGeneratedId",
          entities.ExistingServiceProvider.SystemGeneratedId);
        db.SetInt32(
          command, "offGeneratedId", entities.ExistingOffice.SystemGeneratedId);
          
        db.SetDate(
          command, "effectiveDate",
          import.ExistingOfficeServiceProvider.EffectiveDate.
            GetValueOrDefault());
        db.SetString(
          command, "roleCode", import.ExistingOfficeServiceProvider.RoleCode);
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
    entities.NewOfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider2",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGeneratedId",
          entities.NewServiceProvider.SystemGeneratedId);
        db.SetInt32(
          command, "offGeneratedId", entities.NewOffice.SystemGeneratedId);
        db.SetDate(
          command, "effectiveDate",
          import.NewOfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.SetString(
          command, "roleCode", import.NewOfficeServiceProvider.RoleCode);
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
    entities.ExistingServiceProvider.Populated = false;

    return Read("ReadServiceProvider1",
      (db, command) =>
      {
        db.SetInt32(
          command, "servicePrvderId",
          import.ExistingServiceProvider.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.ExistingServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProvider2()
  {
    entities.NewServiceProvider.Populated = false;

    return Read("ReadServiceProvider2",
      (db, command) =>
      {
        db.SetInt32(
          command, "servicePrvderId",
          import.NewServiceProvider.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.NewServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.NewServiceProvider.Populated = true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
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
    /// A value of NewOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("newOfficeServiceProvider")]
    public OfficeServiceProvider NewOfficeServiceProvider
    {
      get => newOfficeServiceProvider ??= new();
      set => newOfficeServiceProvider = value;
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
    /// A value of ExistingOffice.
    /// </summary>
    [JsonPropertyName("existingOffice")]
    public Office ExistingOffice
    {
      get => existingOffice ??= new();
      set => existingOffice = value;
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
    /// A value of ExistingOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("existingOfficeServiceProvider")]
    public OfficeServiceProvider ExistingOfficeServiceProvider
    {
      get => existingOfficeServiceProvider ??= new();
      set => existingOfficeServiceProvider = value;
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

    private OfficeServiceProvider newOfficeServiceProvider;
    private ServiceProvider newServiceProvider;
    private Office newOffice;
    private Office existingOffice;
    private ServiceProvider existingServiceProvider;
    private OfficeServiceProvider existingOfficeServiceProvider;
    private GlobalReassignment globalReassignment;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of NewServiceProvider.
    /// </summary>
    [JsonPropertyName("newServiceProvider")]
    public ServiceProvider NewServiceProvider
    {
      get => newServiceProvider ??= new();
      set => newServiceProvider = value;
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
    /// A value of GlobalReassignment.
    /// </summary>
    [JsonPropertyName("globalReassignment")]
    public GlobalReassignment GlobalReassignment
    {
      get => globalReassignment ??= new();
      set => globalReassignment = value;
    }

    private Office newOffice;
    private ServiceProvider newServiceProvider;
    private OfficeServiceProvider newOfficeServiceProvider;
    private GlobalReassignment globalReassignment;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
    /// A value of ExistingOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("existingOfficeServiceProvider")]
    public OfficeServiceProvider ExistingOfficeServiceProvider
    {
      get => existingOfficeServiceProvider ??= new();
      set => existingOfficeServiceProvider = value;
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

    private GlobalReassignment globalReassignment;
    private Office newOffice;
    private OfficeServiceProvider newOfficeServiceProvider;
    private ServiceProvider newServiceProvider;
    private ServiceProvider existingServiceProvider;
    private OfficeServiceProvider existingOfficeServiceProvider;
    private Office existingOffice;
  }
#endregion
}
