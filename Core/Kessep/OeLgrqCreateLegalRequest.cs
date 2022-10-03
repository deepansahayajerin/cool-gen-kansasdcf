// Program: OE_LGRQ_CREATE_LEGAL_REQUEST, ID: 371913155, model: 746.
// Short name: SWE00935
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
/// A program: OE_LGRQ_CREATE_LEGAL_REQUEST.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This action block creates legal request details.
/// </para>
/// </summary>
[Serializable]
public partial class OeLgrqCreateLegalRequest: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_LGRQ_CREATE_LEGAL_REQUEST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeLgrqCreateLegalRequest(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeLgrqCreateLegalRequest.
  /// </summary>
  public OeLgrqCreateLegalRequest(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***************************************************************************************************************
    //                         		M A I N T E N A N C E   L O G
    // Date	  Developer		Request#	Description
    // ??/??/??  					Initial Development
    // 12/26/96  G. Lofton - MTW			Add relation to ODRA
    // 01/31/97  Raju - MTW				create legal referral now includes reason codes 2
    // -5
    // 04/30/97  G P Kim				Change Current Date
    // 07/21/99  David Lowry		PR710		Removed an associate to resolve PR710
    // 01/26/00  Carl Galka		PR83111 	allow entry of Court Case number
    // 04/17/01  GVandy		WR 251 		Create Legal Referral Assignment.
    // 12/03/10  GVandy		CQ109		Remove referral reason 5 from all views.
    // 05/18/21  Raj S			CQ68844		Added code to generate HIST/worker Alert 
    // record for newly
    // 						Created Modification Request.
    // ***************************************************************************************************************
    local.Current.Date = Now().Date;

    if (!ReadCase())
    {
      ExitState = "CASE_NF";

      return;
    }

    if (!ReadOfficeServiceProvider())
    {
      ExitState = "OFFICE_SERVICE_PROVIDER_NF";

      return;
    }

    ReadLegalReferral();

    // ---------------------------------------------
    // Raju ( 01/31/97:1130 hrs CST )
    //   - added reason 2-5 in create statement
    // ---------------------------------------------
    try
    {
      CreateLegalReferral();
      export.LegalReferral.Assign(entities.NewLegalReferral);
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "LEGAL_REFERRAL_AE";

          return;
        case ErrorCode.PermittedValueViolation:
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
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "LEGAL_REFERRAL_ASSIGNMENT_AE";

          return;
        case ErrorCode.PermittedValueViolation:
          ExitState = "LEGAL_REFERRAL_ASSIGNMENT_PV";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    // ***    July 21, 1999  David Lowry  PR710
    // Removed a read each of the out_doc_rtrn_addr and associated them with  
    // legal_referral follwed by an escape.
    // **************************************************************
    // *** August 5, 1999  David Lowry  PR714
    // Fully qualified the read for the case role to use the full key structure 
    // and added the already exists exist state.  The old read was returning the
    // same row every time and not creating a new legal_referral_case_role.
    // **************************************************************
    for(import.CaseRole.Index = 0; import.CaseRole.Index < import
      .CaseRole.Count; ++import.CaseRole.Index)
    {
      if (ReadCsePersonCaseRole1())
      {
        try
        {
          CreateLegalReferralCaseRole();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "OE0000_LEGAL_REF_CASE_ROLE_AE";

              break;
            case ErrorCode.PermittedValueViolation:
              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
      else if (ReadCsePersonCaseRole2())
      {
        try
        {
          CreateLegalReferralCaseRole();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "OE0000_LEGAL_REF_CASE_ROLE_AE";

              break;
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

    local.NextSequenceNo.Count = 0;

    for(import.ReferralComments.Index = 0; import.ReferralComments.Index < import
      .ReferralComments.Count; ++import.ReferralComments.Index)
    {
      if (IsEmpty(import.ReferralComments.Item.Detail.CommentLine))
      {
        continue;
      }

      ++local.NextSequenceNo.Count;

      try
      {
        CreateLegalReferralComment();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "LEGAL_REFERRAL_COMMENT_AE";

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

    local.NextSequenceNo.Count = 0;

    for(import.LrAttchmts.Index = 0; import.LrAttchmts.Index < import
      .LrAttchmts.Count; ++import.LrAttchmts.Index)
    {
      if (IsEmpty(import.LrAttchmts.Item.DetailLrAttchmts.CommentLine))
      {
        continue;
      }

      ++local.NextSequenceNo.Count;

      try
      {
        CreateLegalReferralAttachment();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "LEGAL_REFERRAL_ATTACHMENT_AE";

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

    // ****************************************************************************************************************
    // CQ68844: Generate Event for Legal Request Modification Requests are 
    // created.
    // ****************************************************************************************************************
    if (AsChar(import.ModRequestExistsFlag.Flag) == 'Y')
    {
      // ****************************************************************************************************************
      // Assign global infrastructure attribute values.
      // ****************************************************************************************************************
      local.Infrastructure.EventId = 123;
      local.Infrastructure.DenormNumeric12 =
        entities.NewLegalReferral.Identifier;
      local.Infrastructure.ReferenceDate = Now().Date;
      local.Infrastructure.UserId = "LGRQ";
      local.Infrastructure.BusinessObjectCd = "LRF";
      local.Infrastructure.CreatedBy = global.UserId;
      local.Infrastructure.LastUpdatedBy = "";
      local.Infrastructure.ProcessStatus = "Q";
      local.Infrastructure.CaseNumber = import.Case1.Number;
      local.Infrastructure.CsePersonNumber = import.ArPerson.Number;
      local.Infrastructure.ReasonCode = "LEREFMODRCVD";
      local.Infrastructure.Detail =
        "LEGAL REFRL MODIFICATION REQ RECVD FROM CP/NCP/ICP/INCP/OTH ON " + NumberToString
        (DateToInt(Now().Date), 8, 8);
      UseSpCabCreateInfrastructure();
    }
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
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormText12 = source.DenormText12;
    target.DenormDate = source.DenormDate;
    target.DenormTimestamp = source.DenormTimestamp;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.CsenetInOutCode = source.CsenetInOutCode;
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

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    MoveInfrastructure(useExport.Infrastructure, local.Infrastructure);
  }

  private void CreateLegalReferral()
  {
    var casNumber = entities.Existing.Number;
    var identifier = entities.ExistingLast.Identifier + 1;
    var referredByUserId = import.LegalReferral.ReferredByUserId;
    var statusDate = local.Current.Date;
    var status = "S";
    var referralDate = import.LegalReferral.ReferralDate;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var referralReason1 = import.LegalReferral.ReferralReason1;
    var referralReason2 = import.LegalReferral.ReferralReason2;
    var referralReason3 = import.LegalReferral.ReferralReason3;
    var referralReason4 = import.LegalReferral.ReferralReason4;
    var courtCaseNumber = import.LegalReferral.CourtCaseNumber ?? "";
    var tribunalId = import.LegalReferral.TribunalId.GetValueOrDefault();

    entities.NewLegalReferral.Populated = false;
    Update("CreateLegalReferral",
      (db, command) =>
      {
        db.SetString(command, "casNumber", casNumber);
        db.SetInt32(command, "identifier", identifier);
        db.SetString(command, "refByUserId", referredByUserId);
        db.SetNullableDate(command, "statusDate", statusDate);
        db.SetNullableString(command, "status", status);
        db.SetDate(command, "referralDate", referralDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", createdBy);
        db.SetDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetString(command, "referralReason1", referralReason1);
        db.SetString(command, "referralReason2", referralReason2);
        db.SetString(command, "referralReason3", referralReason3);
        db.SetString(command, "referralReason4", referralReason4);
        db.SetString(command, "referralReason5", "");
        db.SetNullableString(command, "courtCaseNo", courtCaseNumber);
        db.SetNullableInt32(command, "trbId", tribunalId);
      });

    entities.NewLegalReferral.CasNumber = casNumber;
    entities.NewLegalReferral.Identifier = identifier;
    entities.NewLegalReferral.ReferredByUserId = referredByUserId;
    entities.NewLegalReferral.StatusDate = statusDate;
    entities.NewLegalReferral.Status = status;
    entities.NewLegalReferral.ReferralDate = referralDate;
    entities.NewLegalReferral.CreatedBy = createdBy;
    entities.NewLegalReferral.CreatedTimestamp = createdTimestamp;
    entities.NewLegalReferral.LastUpdatedBy = createdBy;
    entities.NewLegalReferral.LastUpdatedTimestamp = createdTimestamp;
    entities.NewLegalReferral.ReferralReason1 = referralReason1;
    entities.NewLegalReferral.ReferralReason2 = referralReason2;
    entities.NewLegalReferral.ReferralReason3 = referralReason3;
    entities.NewLegalReferral.ReferralReason4 = referralReason4;
    entities.NewLegalReferral.CourtCaseNumber = courtCaseNumber;
    entities.NewLegalReferral.TribunalId = tribunalId;
    entities.NewLegalReferral.Populated = true;
  }

  private void CreateLegalReferralAssignment()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    System.Diagnostics.Debug.Assert(entities.NewLegalReferral.Populated);

    var reasonCode = "RSP";
    var overrideInd = "N";
    var effectiveDate = local.Current.Date;
    var discontinueDate = UseCabSetMaximumDiscontinueDate();
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var spdId = entities.OfficeServiceProvider.SpdGeneratedId;
    var offId = entities.OfficeServiceProvider.OffGeneratedId;
    var ospCode = entities.OfficeServiceProvider.RoleCode;
    var ospDate = entities.OfficeServiceProvider.EffectiveDate;
    var casNo = entities.NewLegalReferral.CasNumber;
    var lgrId = entities.NewLegalReferral.Identifier;

    entities.LegalReferralAssignment.Populated = false;
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

    entities.LegalReferralAssignment.ReasonCode = reasonCode;
    entities.LegalReferralAssignment.OverrideInd = overrideInd;
    entities.LegalReferralAssignment.EffectiveDate = effectiveDate;
    entities.LegalReferralAssignment.DiscontinueDate = discontinueDate;
    entities.LegalReferralAssignment.CreatedBy = createdBy;
    entities.LegalReferralAssignment.CreatedTimestamp = createdTimestamp;
    entities.LegalReferralAssignment.LastUpdatedBy = "";
    entities.LegalReferralAssignment.LastUpdatedTimestamp = null;
    entities.LegalReferralAssignment.SpdId = spdId;
    entities.LegalReferralAssignment.OffId = offId;
    entities.LegalReferralAssignment.OspCode = ospCode;
    entities.LegalReferralAssignment.OspDate = ospDate;
    entities.LegalReferralAssignment.CasNo = casNo;
    entities.LegalReferralAssignment.LgrId = lgrId;
    entities.LegalReferralAssignment.Populated = true;
  }

  private void CreateLegalReferralAttachment()
  {
    System.Diagnostics.Debug.Assert(entities.NewLegalReferral.Populated);

    var lgrIdentifier = entities.NewLegalReferral.Identifier;
    var casNumber = entities.NewLegalReferral.CasNumber;
    var lineNo = local.NextSequenceNo.Count;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var commentLine = import.LrAttchmts.Item.DetailLrAttchmts.CommentLine;

    entities.NewLegalReferralAttachment.Populated = false;
    Update("CreateLegalReferralAttachment",
      (db, command) =>
      {
        db.SetInt32(command, "lgrIdentifier", lgrIdentifier);
        db.SetString(command, "casNumber", casNumber);
        db.SetInt32(command, "lineNo", lineNo);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", createdBy);
        db.SetDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetString(command, "commentLine", commentLine);
      });

    entities.NewLegalReferralAttachment.LgrIdentifier = lgrIdentifier;
    entities.NewLegalReferralAttachment.CasNumber = casNumber;
    entities.NewLegalReferralAttachment.LineNo = lineNo;
    entities.NewLegalReferralAttachment.CreatedBy = createdBy;
    entities.NewLegalReferralAttachment.CreatedTimestamp = createdTimestamp;
    entities.NewLegalReferralAttachment.LastUpdatedBy = createdBy;
    entities.NewLegalReferralAttachment.LastUpdatedTimestamp = createdTimestamp;
    entities.NewLegalReferralAttachment.CommentLine = commentLine;
    entities.NewLegalReferralAttachment.Populated = true;
  }

  private void CreateLegalReferralCaseRole()
  {
    System.Diagnostics.Debug.Assert(entities.CaseRole1.Populated);
    System.Diagnostics.Debug.Assert(entities.NewLegalReferral.Populated);

    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var casNumber = entities.NewLegalReferral.CasNumber;
    var lgrId = entities.NewLegalReferral.Identifier;
    var casNumberRole = entities.CaseRole1.CasNumber;
    var cspNumber = entities.CaseRole1.CspNumber;
    var croType = entities.CaseRole1.Type1;
    var croId = entities.CaseRole1.Identifier;

    CheckValid<LegalReferralCaseRole>("CroType", croType);
    entities.LegalReferralCaseRole.Populated = false;
    Update("CreateLegalReferralCaseRole",
      (db, command) =>
      {
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "casNumber", casNumber);
        db.SetInt32(command, "lgrId", lgrId);
        db.SetString(command, "casNumberRole", casNumberRole);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "croType", croType);
        db.SetInt32(command, "croId", croId);
      });

    entities.LegalReferralCaseRole.CreatedBy = createdBy;
    entities.LegalReferralCaseRole.CreatedTimestamp = createdTimestamp;
    entities.LegalReferralCaseRole.CasNumber = casNumber;
    entities.LegalReferralCaseRole.LgrId = lgrId;
    entities.LegalReferralCaseRole.CasNumberRole = casNumberRole;
    entities.LegalReferralCaseRole.CspNumber = cspNumber;
    entities.LegalReferralCaseRole.CroType = croType;
    entities.LegalReferralCaseRole.CroId = croId;
    entities.LegalReferralCaseRole.Populated = true;
  }

  private void CreateLegalReferralComment()
  {
    System.Diagnostics.Debug.Assert(entities.NewLegalReferral.Populated);

    var lgrIdentifier = entities.NewLegalReferral.Identifier;
    var casNumber = entities.NewLegalReferral.CasNumber;
    var lineNo = local.NextSequenceNo.Count;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var commentLine = import.ReferralComments.Item.Detail.CommentLine;

    entities.NewLegalReferralComment.Populated = false;
    Update("CreateLegalReferralComment",
      (db, command) =>
      {
        db.SetInt32(command, "lgrIdentifier", lgrIdentifier);
        db.SetString(command, "casNumber", casNumber);
        db.SetInt32(command, "lineNo", lineNo);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", createdBy);
        db.SetDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetString(command, "commentLine", commentLine);
      });

    entities.NewLegalReferralComment.LgrIdentifier = lgrIdentifier;
    entities.NewLegalReferralComment.CasNumber = casNumber;
    entities.NewLegalReferralComment.LineNo = lineNo;
    entities.NewLegalReferralComment.CreatedBy = createdBy;
    entities.NewLegalReferralComment.CreatedTimestamp = createdTimestamp;
    entities.NewLegalReferralComment.LastUpdatedBy = createdBy;
    entities.NewLegalReferralComment.LastUpdatedTimestamp = createdTimestamp;
    entities.NewLegalReferralComment.CommentLine = commentLine;
    entities.NewLegalReferralComment.Populated = true;
  }

  private bool ReadCase()
  {
    entities.Existing.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Existing.Number = db.GetString(reader, 0);
        entities.Existing.Populated = true;
      });
  }

  private bool ReadCsePersonCaseRole1()
  {
    entities.CsePerson.Populated = false;
    entities.CaseRole1.Populated = false;

    return Read("ReadCsePersonCaseRole1",
      (db, command) =>
      {
        db.
          SetString(command, "cspNumber", import.CaseRole.Item.CsePerson.Number);
          
        db.SetString(command, "casNumber", entities.Existing.Number);
        db.SetInt32(
          command, "caseRoleId", import.CaseRole.Item.CaseRole1.Identifier);
        db.SetString(command, "type", import.CaseRole.Item.CaseRole1.Type1);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CaseRole1.CspNumber = db.GetString(reader, 0);
        entities.CaseRole1.CasNumber = db.GetString(reader, 1);
        entities.CaseRole1.Type1 = db.GetString(reader, 2);
        entities.CaseRole1.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole1.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole1.EndDate = db.GetNullableDate(reader, 5);
        entities.CsePerson.Populated = true;
        entities.CaseRole1.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole1.Type1);
      });
  }

  private bool ReadCsePersonCaseRole2()
  {
    entities.CsePerson.Populated = false;
    entities.CaseRole1.Populated = false;

    return Read("ReadCsePersonCaseRole2",
      (db, command) =>
      {
        db.
          SetString(command, "cspNumber", import.CaseRole.Item.CsePerson.Number);
          
        db.SetString(command, "casNumber", entities.Existing.Number);
        db.SetInt32(
          command, "caseRoleId", import.CaseRole.Item.CaseRole1.Identifier);
        db.SetString(command, "type", import.CaseRole.Item.CaseRole1.Type1);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CaseRole1.CspNumber = db.GetString(reader, 0);
        entities.CaseRole1.CasNumber = db.GetString(reader, 1);
        entities.CaseRole1.Type1 = db.GetString(reader, 2);
        entities.CaseRole1.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole1.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole1.EndDate = db.GetNullableDate(reader, 5);
        entities.CsePerson.Populated = true;
        entities.CaseRole1.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole1.Type1);
      });
  }

  private bool ReadLegalReferral()
  {
    entities.ExistingLast.Populated = false;

    return Read("ReadLegalReferral",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Existing.Number);
      },
      (db, reader) =>
      {
        entities.ExistingLast.CasNumber = db.GetString(reader, 0);
        entities.ExistingLast.Identifier = db.GetInt32(reader, 1);
        entities.ExistingLast.Populated = true;
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
          
        db.SetInt32(command, "offGeneratedId", import.Office.SystemGeneratedId);
        db.SetInt32(
          command, "spdGeneratedId", import.ServiceProvider.SystemGeneratedId);
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
    /// <summary>A CaseRoleGroup group.</summary>
    [Serializable]
    public class CaseRoleGroup
    {
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
      /// A value of CsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("csePersonsWorkSet")]
      public CsePersonsWorkSet CsePersonsWorkSet
      {
        get => csePersonsWorkSet ??= new();
        set => csePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of CaseRole1.
      /// </summary>
      [JsonPropertyName("caseRole1")]
      public CaseRole CaseRole1
      {
        get => caseRole1 ??= new();
        set => caseRole1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private CsePerson csePerson;
      private CsePersonsWorkSet csePersonsWorkSet;
      private CaseRole caseRole1;
    }

    /// <summary>A LrAttchmtsGroup group.</summary>
    [Serializable]
    public class LrAttchmtsGroup
    {
      /// <summary>
      /// A value of DetailLrAttchmts.
      /// </summary>
      [JsonPropertyName("detailLrAttchmts")]
      public LegalReferralAttachment DetailLrAttchmts
      {
        get => detailLrAttchmts ??= new();
        set => detailLrAttchmts = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private LegalReferralAttachment detailLrAttchmts;
    }

    /// <summary>A ReferralCommentsGroup group.</summary>
    [Serializable]
    public class ReferralCommentsGroup
    {
      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public LegalReferralComment Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private LegalReferralComment detail;
    }

    /// <summary>
    /// Gets a value of CaseRole.
    /// </summary>
    [JsonIgnore]
    public Array<CaseRoleGroup> CaseRole => caseRole ??= new(
      CaseRoleGroup.Capacity);

    /// <summary>
    /// Gets a value of CaseRole for json serialization.
    /// </summary>
    [JsonPropertyName("caseRole")]
    [Computed]
    public IList<CaseRoleGroup> CaseRole_Json
    {
      get => caseRole;
      set => CaseRole.Assign(value);
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
    /// A value of LegalReferral.
    /// </summary>
    [JsonPropertyName("legalReferral")]
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
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
    /// Gets a value of LrAttchmts.
    /// </summary>
    [JsonIgnore]
    public Array<LrAttchmtsGroup> LrAttchmts => lrAttchmts ??= new(
      LrAttchmtsGroup.Capacity);

    /// <summary>
    /// Gets a value of LrAttchmts for json serialization.
    /// </summary>
    [JsonPropertyName("lrAttchmts")]
    [Computed]
    public IList<LrAttchmtsGroup> LrAttchmts_Json
    {
      get => lrAttchmts;
      set => LrAttchmts.Assign(value);
    }

    /// <summary>
    /// Gets a value of ReferralComments.
    /// </summary>
    [JsonIgnore]
    public Array<ReferralCommentsGroup> ReferralComments =>
      referralComments ??= new(ReferralCommentsGroup.Capacity);

    /// <summary>
    /// Gets a value of ReferralComments for json serialization.
    /// </summary>
    [JsonPropertyName("referralComments")]
    [Computed]
    public IList<ReferralCommentsGroup> ReferralComments_Json
    {
      get => referralComments;
      set => ReferralComments.Assign(value);
    }

    /// <summary>
    /// A value of ModRequestExistsFlag.
    /// </summary>
    [JsonPropertyName("modRequestExistsFlag")]
    public Common ModRequestExistsFlag
    {
      get => modRequestExistsFlag ??= new();
      set => modRequestExistsFlag = value;
    }

    /// <summary>
    /// A value of ArPerson.
    /// </summary>
    [JsonPropertyName("arPerson")]
    public CsePersonsWorkSet ArPerson
    {
      get => arPerson ??= new();
      set => arPerson = value;
    }

    private Array<CaseRoleGroup> caseRole;
    private OfficeServiceProvider officeServiceProvider;
    private Office office;
    private ServiceProvider serviceProvider;
    private LegalReferral legalReferral;
    private Case1 case1;
    private Array<LrAttchmtsGroup> lrAttchmts;
    private Array<ReferralCommentsGroup> referralComments;
    private Common modRequestExistsFlag;
    private CsePersonsWorkSet arPerson;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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

    private LegalReferral legalReferral;
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

    /// <summary>
    /// A value of NextSequenceNo.
    /// </summary>
    [JsonPropertyName("nextSequenceNo")]
    public Common NextSequenceNo
    {
      get => nextSequenceNo ??= new();
      set => nextSequenceNo = value;
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

    private DateWorkArea current;
    private Common nextSequenceNo;
    private Infrastructure infrastructure;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of LegalReferralCaseRole.
    /// </summary>
    [JsonPropertyName("legalReferralCaseRole")]
    public LegalReferralCaseRole LegalReferralCaseRole
    {
      get => legalReferralCaseRole ??= new();
      set => legalReferralCaseRole = value;
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
    /// A value of CaseRole1.
    /// </summary>
    [JsonPropertyName("caseRole1")]
    public CaseRole CaseRole1
    {
      get => caseRole1 ??= new();
      set => caseRole1 = value;
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
    /// A value of NewLegalReferralAttachment.
    /// </summary>
    [JsonPropertyName("newLegalReferralAttachment")]
    public LegalReferralAttachment NewLegalReferralAttachment
    {
      get => newLegalReferralAttachment ??= new();
      set => newLegalReferralAttachment = value;
    }

    /// <summary>
    /// A value of ExistingLast.
    /// </summary>
    [JsonPropertyName("existingLast")]
    public LegalReferral ExistingLast
    {
      get => existingLast ??= new();
      set => existingLast = value;
    }

    /// <summary>
    /// A value of NewLegalReferral.
    /// </summary>
    [JsonPropertyName("newLegalReferral")]
    public LegalReferral NewLegalReferral
    {
      get => newLegalReferral ??= new();
      set => newLegalReferral = value;
    }

    /// <summary>
    /// A value of NewLegalReferralComment.
    /// </summary>
    [JsonPropertyName("newLegalReferralComment")]
    public LegalReferralComment NewLegalReferralComment
    {
      get => newLegalReferralComment ??= new();
      set => newLegalReferralComment = value;
    }

    /// <summary>
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public Case1 Existing
    {
      get => existing ??= new();
      set => existing = value;
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

    private LegalReferralAssignment legalReferralAssignment;
    private LegalReferralCaseRole legalReferralCaseRole;
    private CsePerson csePerson;
    private CaseRole caseRole1;
    private OfficeServiceProvider officeServiceProvider;
    private Office office;
    private ServiceProvider serviceProvider;
    private LegalReferralAttachment newLegalReferralAttachment;
    private LegalReferral existingLast;
    private LegalReferral newLegalReferral;
    private LegalReferralComment newLegalReferralComment;
    private Case1 existing;
    private Case1 case1;
  }
#endregion
}
