// Program: OE_LGRQ_UPDATE_LEGAL_REQUEST, ID: 371913158, model: 746.
// Short name: SWE00938
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
/// A program: OE_LGRQ_UPDATE_LEGAL_REQUEST.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This action block updates legal request details.
/// </para>
/// </summary>
[Serializable]
public partial class OeLgrqUpdateLegalRequest: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_LGRQ_UPDATE_LEGAL_REQUEST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeLgrqUpdateLegalRequest(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeLgrqUpdateLegalRequest.
  /// </summary>
  public OeLgrqUpdateLegalRequest(IContext context, Import import, Export export)
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
    // ----------------------------------------------------------------------------------------------
    // 		M A I N T E N A N C E   L O G
    // Date	  Developer	Request#	Description
    // 01/31/97  Raju - MTW			update legal referral now includes reason codes 2-
    // 5
    // 04/24/01  GVandy	WR251		Removed unnecessary views and general cleanup.
    // 04/27/01  GVandy	PR 118468	Ignore legal referral assignment not found 
    // when closing,
    // 					rejecting, or withdrawing a legal referral.
    // 09/03/02  GVandy	PR152797/	Correct performance issue when closing 
    // monitored
    // 			PR154552	activities.
    // 10/23/02  GVandy	PR160857	Additional changes to help performance when 
    // closing
    // 					monitored activities.
    // 11/27/02  GVandy	PR164315	Additional performance changes when reading 
    // infrastructure
    // 					for legal referrals.
    // 03/10/03  GVandy	PR152793	Close legal action assignments to the attorney 
    // when all their
    // 					legal referrals for the case are ended.
    // 12/03/10  GVandy	CQ109		Remove referral reason 5 from all views.
    // ----------------------------------------------------------------------------------------------
    local.Current.Date = Now().Date;
    export.LegalReferral.Assign(import.LegalReferral);

    if (!ReadCase1())
    {
      ExitState = "CASE_NF";

      return;
    }

    if (!ReadLegalReferral1())
    {
      ExitState = "LEGAL_REFERRAL_NF";

      return;
    }

    // ---------------------------------------------
    // Raju ( 01/31/97:1130 hrs CST )
    //   - added reason 2-5 in create statement
    // ---------------------------------------------
    try
    {
      UpdateLegalReferral();
      MoveLegalReferral(entities.LegalReferral, export.LegalReferral);
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "LEGAL_REFERRAL_NU";

          return;
        case ErrorCode.PermittedValueViolation:
          ExitState = "LEGAL_REFERRAL_PV";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    if (AsChar(entities.LegalReferral.Status) == 'C' || AsChar
      (entities.LegalReferral.Status) == 'R' || AsChar
      (entities.LegalReferral.Status) == 'W')
    {
      // @@@ - New (PR160857)
      foreach(var item in ReadLegalReferralAssignment2())
      {
        if (!Lt(local.Current.Date,
          entities.LegalReferralAssignment.EffectiveDate) && !
          Lt(entities.LegalReferralAssignment.DiscontinueDate,
          local.Current.Date))
        {
          if (ReadOfficeServiceProvider())
          {
            break;
          }
        }
      }

      if (!entities.OfficeServiceProvider.Populated)
      {
        goto Test;
      }

      // @@@ - End (PR160857)
      // --   7/11/01  KCole   WO 010350      End the legal service provider 
      // assignment on legal actions related to the legal request being closed,
      // rejected, or withdrawn. Also, end the assignment for any montiored
      // activities for the legal action or legal request.
      try
      {
        UpdateLegalReferralAssignment();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "LEGAL_REFERRAL_ASSIGNMENT_NU";

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

      if (!ReadCase2())
      {
        ExitState = "CASE_NF";

        return;
      }

      // -- PR 152793  Correct logic which determines if any open or sent 
      // referrals are still assigned
      // to this service provider.
      foreach(var item in ReadLegalReferral2())
      {
        if (ReadLegalReferralAssignment1())
        {
          // -- There is still an Open or Sent referral assigned to the service 
          // provider.
          // Do not close legal action assignments to the service provider.
          goto Test;
        }
        else
        {
          // -- Continue.
        }
      }

      foreach(var item in ReadLegalAction())
      {
        if (AsChar(entities.LegalAction.Classification) == 'N' || AsChar
          (entities.LegalAction.Classification) == 'U')
        {
          continue;
        }

        // @@@ - New (PR160857)
        foreach(var item1 in ReadLegalActionAssigment())
        {
          if (!Lt(local.Current.Date,
            entities.LegalActionAssigment.EffectiveDate) && !
            Lt(entities.LegalActionAssigment.DiscontinueDate, local.Current.Date)
            && Equal(entities.LegalActionAssigment.ReasonCode, "RSP"))
          {
            try
            {
              UpdateLegalActionAssigment();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "LEGAL_ACTION_ASSIGNMENT_NU";

                  return;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "LEGAL_ACTION_ASSIGNMENT_PV";

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }
        }

        // @@@ - End (PR160857)
        local.Infrastructure.DenormNumeric12 = entities.LegalAction.Identifier;

        // @@@ - New (PR160857)
        local.Infrastructure.CaseNumber = entities.Case1.Number;

        foreach(var item1 in ReadInfrastructure1())
        {
          if (!Equal(entities.Infrastructure.BusinessObjectCd, "LEA"))
          {
            continue;
          }

          foreach(var item2 in ReadMonitoredActivity())
          {
            if (!IsEmpty(entities.MonitoredActivity.ClosureReasonCode))
            {
              continue;
            }

            foreach(var item3 in ReadMonitoredActivityAssignment())
            {
              if (!Lt(local.Current.Date,
                entities.MonitoredActivityAssignment.EffectiveDate) && !
                Lt(entities.MonitoredActivityAssignment.DiscontinueDate,
                local.Current.Date))
              {
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
                      ExitState = "MONITORED_ACTIVITY_ASSIGNMEN_PV";

                      return;
                    case ErrorCode.DatabaseError:
                      break;
                    default:
                      throw;
                  }
                }
              }
              else
              {
                continue;
              }
            }
          }
        }

        // @@@ - End (PR160857)
      }

      // @@@ - New (PR160857)
      local.Infrastructure.DenormNumeric12 = entities.LegalReferral.Identifier;
      local.Infrastructure.CaseNumber = entities.Case1.Number;

      // -- 11/27/02  GVandy  PR164315  Removed denorm numeric 12 from the where
      // clause and now check for equality inside the read each.  With denorm
      // numeric 12 in the where clause it uses index CKI09717 which is ordered
      // first by denorm numeric 12.  However, for legal referrals the denorm
      // numeric 12 attribute contains a sequence number instead of a unique
      // key.  By removing denorm numeric 12 it will use index CKI02717 which is
      // ordered first by case number.
      foreach(var item in ReadInfrastructure2())
      {
        if (!Equal(entities.Infrastructure.DenormNumeric12,
          local.Infrastructure.DenormNumeric12.GetValueOrDefault()))
        {
          continue;
        }

        if (!Equal(entities.Infrastructure.BusinessObjectCd, "LRF"))
        {
          continue;
        }

        foreach(var item1 in ReadMonitoredActivity())
        {
          if (!IsEmpty(entities.MonitoredActivity.ClosureReasonCode))
          {
            continue;
          }

          foreach(var item2 in ReadMonitoredActivityAssignment())
          {
            if (!Lt(local.Current.Date,
              entities.MonitoredActivityAssignment.EffectiveDate) && !
              Lt(entities.MonitoredActivityAssignment.DiscontinueDate,
              local.Current.Date))
            {
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
                    ExitState = "MONITORED_ACTIVITY_ASSIGNMEN_PV";

                    return;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }
            }
            else
            {
              continue;
            }
          }
        }
      }

      // @@@ - End (PR160857)
    }

Test:

    foreach(var item in ReadLegalReferralComment())
    {
      DeleteLegalReferralComment();
    }

    local.NextSequenceNo.Count = 0;

    for(import.LrCommentLines.Index = 0; import.LrCommentLines.Index < import
      .LrCommentLines.Count; ++import.LrCommentLines.Index)
    {
      if (IsEmpty(import.LrCommentLines.Item.Detail.CommentLine))
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

    foreach(var item in ReadLegalReferralAttachment())
    {
      DeleteLegalReferralAttachment();
    }

    local.NextSequenceNo.Count = 0;

    for(import.LrAttachmts.Index = 0; import.LrAttachmts.Index < import
      .LrAttachmts.Count; ++import.LrAttachmts.Index)
    {
      if (IsEmpty(import.LrAttachmts.Item.DetailLrAttachmts.CommentLine))
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

  private static void MoveLegalReferral(LegalReferral source,
    LegalReferral target)
  {
    target.ReferredByUserId = source.ReferredByUserId;
    target.Identifier = source.Identifier;
    target.ReferralDate = source.ReferralDate;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.StatusDate = source.StatusDate;
    target.ReferralReason1 = source.ReferralReason1;
    target.ReferralReason2 = source.ReferralReason2;
    target.ReferralReason3 = source.ReferralReason3;
    target.ReferralReason4 = source.ReferralReason4;
    target.Status = source.Status;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.TribunalId = source.TribunalId;
  }

  private void CreateLegalReferralAttachment()
  {
    System.Diagnostics.Debug.Assert(entities.LegalReferral.Populated);

    var lgrIdentifier = entities.LegalReferral.Identifier;
    var casNumber = entities.LegalReferral.CasNumber;
    var lineNo = local.NextSequenceNo.Count;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var commentLine = import.LrAttachmts.Item.DetailLrAttachmts.CommentLine;

    entities.LegalReferralAttachment.Populated = false;
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

    entities.LegalReferralAttachment.LgrIdentifier = lgrIdentifier;
    entities.LegalReferralAttachment.CasNumber = casNumber;
    entities.LegalReferralAttachment.LineNo = lineNo;
    entities.LegalReferralAttachment.CreatedBy = createdBy;
    entities.LegalReferralAttachment.CreatedTimestamp = createdTimestamp;
    entities.LegalReferralAttachment.LastUpdatedBy = createdBy;
    entities.LegalReferralAttachment.LastUpdatedTimestamp = createdTimestamp;
    entities.LegalReferralAttachment.CommentLine = commentLine;
    entities.LegalReferralAttachment.Populated = true;
  }

  private void CreateLegalReferralComment()
  {
    System.Diagnostics.Debug.Assert(entities.LegalReferral.Populated);

    var lgrIdentifier = entities.LegalReferral.Identifier;
    var casNumber = entities.LegalReferral.CasNumber;
    var lineNo = local.NextSequenceNo.Count;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var commentLine = import.LrCommentLines.Item.Detail.CommentLine;

    entities.LegalReferralComment.Populated = false;
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

    entities.LegalReferralComment.LgrIdentifier = lgrIdentifier;
    entities.LegalReferralComment.CasNumber = casNumber;
    entities.LegalReferralComment.LineNo = lineNo;
    entities.LegalReferralComment.CreatedBy = createdBy;
    entities.LegalReferralComment.CreatedTimestamp = createdTimestamp;
    entities.LegalReferralComment.LastUpdatedBy = createdBy;
    entities.LegalReferralComment.LastUpdatedTimestamp = createdTimestamp;
    entities.LegalReferralComment.CommentLine = commentLine;
    entities.LegalReferralComment.Populated = true;
  }

  private void DeleteLegalReferralAttachment()
  {
    Update("DeleteLegalReferralAttachment",
      (db, command) =>
      {
        db.SetInt32(
          command, "lgrIdentifier",
          entities.LegalReferralAttachment.LgrIdentifier);
        db.SetString(
          command, "casNumber", entities.LegalReferralAttachment.CasNumber);
        db.SetInt32(command, "lineNo", entities.LegalReferralAttachment.LineNo);
      });
  }

  private void DeleteLegalReferralComment()
  {
    Update("DeleteLegalReferralComment",
      (db, command) =>
      {
        db.SetInt32(
          command, "lgrIdentifier",
          entities.LegalReferralComment.LgrIdentifier);
        db.SetString(
          command, "casNumber", entities.LegalReferralComment.CasNumber);
        db.SetInt32(command, "lineNo", entities.LegalReferralComment.LineNo);
      });
  }

  private bool ReadCase1()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase1",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCase2()
  {
    System.Diagnostics.Debug.Assert(entities.LegalReferral.Populated);
    entities.Case1.Populated = false;

    return Read("ReadCase2",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.LegalReferral.CasNumber);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;
      });
  }

  private IEnumerable<bool> ReadInfrastructure1()
  {
    entities.Infrastructure.Populated = false;

    return ReadEach("ReadInfrastructure1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "caseNumber", local.Infrastructure.CaseNumber ?? "");
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Infrastructure.DenormNumeric12.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 1);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 2);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 3);
        entities.Infrastructure.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadInfrastructure2()
  {
    entities.Infrastructure.Populated = false;

    return ReadEach("ReadInfrastructure2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "caseNumber", local.Infrastructure.CaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 1);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 2);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 3);
        entities.Infrastructure.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalAction",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "endDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionAssigment()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.LegalActionAssigment.Populated = false;

    return ReadEach("ReadLegalActionAssigment",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.LegalAction.Identifier);
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
        entities.LegalActionAssigment.LgaIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.LegalActionAssigment.OspEffectiveDate =
          db.GetNullableDate(reader, 1);
        entities.LegalActionAssigment.OspRoleCode =
          db.GetNullableString(reader, 2);
        entities.LegalActionAssigment.OffGeneratedId =
          db.GetNullableInt32(reader, 3);
        entities.LegalActionAssigment.SpdGeneratedId =
          db.GetNullableInt32(reader, 4);
        entities.LegalActionAssigment.EffectiveDate = db.GetDate(reader, 5);
        entities.LegalActionAssigment.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.LegalActionAssigment.ReasonCode = db.GetString(reader, 7);
        entities.LegalActionAssigment.CreatedTimestamp =
          db.GetDateTime(reader, 8);
        entities.LegalActionAssigment.LastUpdatedBy =
          db.GetNullableString(reader, 9);
        entities.LegalActionAssigment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 10);
        entities.LegalActionAssigment.Populated = true;

        return true;
      });
  }

  private bool ReadLegalReferral1()
  {
    entities.LegalReferral.Populated = false;

    return Read("ReadLegalReferral1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetInt32(command, "identifier", import.LegalReferral.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalReferral.CasNumber = db.GetString(reader, 0);
        entities.LegalReferral.Identifier = db.GetInt32(reader, 1);
        entities.LegalReferral.ReferredByUserId = db.GetString(reader, 2);
        entities.LegalReferral.StatusDate = db.GetNullableDate(reader, 3);
        entities.LegalReferral.Status = db.GetNullableString(reader, 4);
        entities.LegalReferral.ReferralDate = db.GetDate(reader, 5);
        entities.LegalReferral.LastUpdatedBy = db.GetString(reader, 6);
        entities.LegalReferral.LastUpdatedTimestamp = db.GetDateTime(reader, 7);
        entities.LegalReferral.ReferralReason1 = db.GetString(reader, 8);
        entities.LegalReferral.ReferralReason2 = db.GetString(reader, 9);
        entities.LegalReferral.ReferralReason3 = db.GetString(reader, 10);
        entities.LegalReferral.ReferralReason4 = db.GetString(reader, 11);
        entities.LegalReferral.CourtCaseNumber =
          db.GetNullableString(reader, 12);
        entities.LegalReferral.TribunalId = db.GetNullableInt32(reader, 13);
        entities.LegalReferral.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalReferral2()
  {
    entities.Other.Populated = false;

    return ReadEach("ReadLegalReferral2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Other.CasNumber = db.GetString(reader, 0);
        entities.Other.Identifier = db.GetInt32(reader, 1);
        entities.Other.Status = db.GetNullableString(reader, 2);
        entities.Other.Populated = true;

        return true;
      });
  }

  private bool ReadLegalReferralAssignment1()
  {
    System.Diagnostics.Debug.Assert(entities.Other.Populated);
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.LegalReferralAssignment.Populated = false;

    return Read("ReadLegalReferralAssignment1",
      (db, command) =>
      {
        db.SetInt32(command, "lgrId", entities.Other.Identifier);
        db.SetString(command, "casNo", entities.Other.CasNumber);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
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
        entities.LegalReferralAssignment.EffectiveDate = db.GetDate(reader, 0);
        entities.LegalReferralAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 1);
        entities.LegalReferralAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 2);
        entities.LegalReferralAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 3);
        entities.LegalReferralAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 4);
        entities.LegalReferralAssignment.SpdId = db.GetInt32(reader, 5);
        entities.LegalReferralAssignment.OffId = db.GetInt32(reader, 6);
        entities.LegalReferralAssignment.OspCode = db.GetString(reader, 7);
        entities.LegalReferralAssignment.OspDate = db.GetDate(reader, 8);
        entities.LegalReferralAssignment.CasNo = db.GetString(reader, 9);
        entities.LegalReferralAssignment.LgrId = db.GetInt32(reader, 10);
        entities.LegalReferralAssignment.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalReferralAssignment2()
  {
    System.Diagnostics.Debug.Assert(entities.LegalReferral.Populated);
    entities.LegalReferralAssignment.Populated = false;

    return ReadEach("ReadLegalReferralAssignment2",
      (db, command) =>
      {
        db.SetInt32(command, "lgrId", entities.LegalReferral.Identifier);
        db.SetString(command, "casNo", entities.LegalReferral.CasNumber);
      },
      (db, reader) =>
      {
        entities.LegalReferralAssignment.EffectiveDate = db.GetDate(reader, 0);
        entities.LegalReferralAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 1);
        entities.LegalReferralAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 2);
        entities.LegalReferralAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 3);
        entities.LegalReferralAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 4);
        entities.LegalReferralAssignment.SpdId = db.GetInt32(reader, 5);
        entities.LegalReferralAssignment.OffId = db.GetInt32(reader, 6);
        entities.LegalReferralAssignment.OspCode = db.GetString(reader, 7);
        entities.LegalReferralAssignment.OspDate = db.GetDate(reader, 8);
        entities.LegalReferralAssignment.CasNo = db.GetString(reader, 9);
        entities.LegalReferralAssignment.LgrId = db.GetInt32(reader, 10);
        entities.LegalReferralAssignment.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalReferralAttachment()
  {
    System.Diagnostics.Debug.Assert(entities.LegalReferral.Populated);
    entities.LegalReferralAttachment.Populated = false;

    return ReadEach("ReadLegalReferralAttachment",
      (db, command) =>
      {
        db.
          SetInt32(command, "lgrIdentifier", entities.LegalReferral.Identifier);
          
        db.SetString(command, "casNumber", entities.LegalReferral.CasNumber);
      },
      (db, reader) =>
      {
        entities.LegalReferralAttachment.LgrIdentifier = db.GetInt32(reader, 0);
        entities.LegalReferralAttachment.CasNumber = db.GetString(reader, 1);
        entities.LegalReferralAttachment.LineNo = db.GetInt32(reader, 2);
        entities.LegalReferralAttachment.CreatedBy = db.GetString(reader, 3);
        entities.LegalReferralAttachment.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.LegalReferralAttachment.LastUpdatedBy =
          db.GetString(reader, 5);
        entities.LegalReferralAttachment.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.LegalReferralAttachment.CommentLine = db.GetString(reader, 7);
        entities.LegalReferralAttachment.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalReferralComment()
  {
    System.Diagnostics.Debug.Assert(entities.LegalReferral.Populated);
    entities.LegalReferralComment.Populated = false;

    return ReadEach("ReadLegalReferralComment",
      (db, command) =>
      {
        db.
          SetInt32(command, "lgrIdentifier", entities.LegalReferral.Identifier);
          
        db.SetString(command, "casNumber", entities.LegalReferral.CasNumber);
      },
      (db, reader) =>
      {
        entities.LegalReferralComment.LgrIdentifier = db.GetInt32(reader, 0);
        entities.LegalReferralComment.CasNumber = db.GetString(reader, 1);
        entities.LegalReferralComment.LineNo = db.GetInt32(reader, 2);
        entities.LegalReferralComment.CreatedBy = db.GetString(reader, 3);
        entities.LegalReferralComment.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.LegalReferralComment.LastUpdatedBy = db.GetString(reader, 5);
        entities.LegalReferralComment.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.LegalReferralComment.CommentLine = db.GetString(reader, 7);
        entities.LegalReferralComment.Populated = true;

        return true;
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
      },
      (db, reader) =>
      {
        entities.MonitoredActivity.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.MonitoredActivity.ClosureReasonCode =
          db.GetNullableString(reader, 1);
        entities.MonitoredActivity.InfSysGenId = db.GetNullableInt32(reader, 2);
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
      },
      (db, reader) =>
      {
        entities.MonitoredActivityAssignment.EffectiveDate =
          db.GetDate(reader, 0);
        entities.MonitoredActivityAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 1);
        entities.MonitoredActivityAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 2);
        entities.MonitoredActivityAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 3);
        entities.MonitoredActivityAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 4);
        entities.MonitoredActivityAssignment.SpdId = db.GetInt32(reader, 5);
        entities.MonitoredActivityAssignment.OffId = db.GetInt32(reader, 6);
        entities.MonitoredActivityAssignment.OspCode = db.GetString(reader, 7);
        entities.MonitoredActivityAssignment.OspDate = db.GetDate(reader, 8);
        entities.MonitoredActivityAssignment.MacId = db.GetInt32(reader, 9);
        entities.MonitoredActivityAssignment.Populated = true;

        return true;
      });
  }

  private bool ReadOfficeServiceProvider()
  {
    System.Diagnostics.Debug.Assert(entities.LegalReferralAssignment.Populated);
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          entities.LegalReferralAssignment.OspDate.GetValueOrDefault());
        db.SetString(
          command, "roleCode", entities.LegalReferralAssignment.OspCode);
        db.SetInt32(
          command, "offGeneratedId", entities.LegalReferralAssignment.OffId);
        db.SetInt32(
          command, "spdGeneratedId", entities.LegalReferralAssignment.SpdId);
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

  private void UpdateLegalActionAssigment()
  {
    var discontinueDate = local.Current.Date;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.LegalActionAssigment.Populated = false;
    Update("UpdateLegalActionAssigment",
      (db, command) =>
      {
        db.SetNullableDate(command, "endDt", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetDateTime(
          command, "createdTimestamp",
          entities.LegalActionAssigment.CreatedTimestamp.GetValueOrDefault());
      });

    entities.LegalActionAssigment.DiscontinueDate = discontinueDate;
    entities.LegalActionAssigment.LastUpdatedBy = lastUpdatedBy;
    entities.LegalActionAssigment.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.LegalActionAssigment.Populated = true;
  }

  private void UpdateLegalReferral()
  {
    System.Diagnostics.Debug.Assert(entities.LegalReferral.Populated);

    var referredByUserId = import.LegalReferral.ReferredByUserId;
    var statusDate = import.LegalReferral.StatusDate;
    var status = import.LegalReferral.Status ?? "";
    var referralDate = import.LegalReferral.ReferralDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();
    var referralReason1 = import.LegalReferral.ReferralReason1;
    var referralReason2 = import.LegalReferral.ReferralReason2;
    var referralReason3 = import.LegalReferral.ReferralReason3;
    var referralReason4 = import.LegalReferral.ReferralReason4;
    var courtCaseNumber = import.LegalReferral.CourtCaseNumber ?? "";
    var tribunalId = import.LegalReferral.TribunalId.GetValueOrDefault();

    entities.LegalReferral.Populated = false;
    Update("UpdateLegalReferral",
      (db, command) =>
      {
        db.SetString(command, "refByUserId", referredByUserId);
        db.SetNullableDate(command, "statusDate", statusDate);
        db.SetNullableString(command, "status", status);
        db.SetDate(command, "referralDate", referralDate);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetString(command, "referralReason1", referralReason1);
        db.SetString(command, "referralReason2", referralReason2);
        db.SetString(command, "referralReason3", referralReason3);
        db.SetString(command, "referralReason4", referralReason4);
        db.SetNullableString(command, "courtCaseNo", courtCaseNumber);
        db.SetNullableInt32(command, "trbId", tribunalId);
        db.SetString(command, "casNumber", entities.LegalReferral.CasNumber);
        db.SetInt32(command, "identifier", entities.LegalReferral.Identifier);
      });

    entities.LegalReferral.ReferredByUserId = referredByUserId;
    entities.LegalReferral.StatusDate = statusDate;
    entities.LegalReferral.Status = status;
    entities.LegalReferral.ReferralDate = referralDate;
    entities.LegalReferral.LastUpdatedBy = lastUpdatedBy;
    entities.LegalReferral.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.LegalReferral.ReferralReason1 = referralReason1;
    entities.LegalReferral.ReferralReason2 = referralReason2;
    entities.LegalReferral.ReferralReason3 = referralReason3;
    entities.LegalReferral.ReferralReason4 = referralReason4;
    entities.LegalReferral.CourtCaseNumber = courtCaseNumber;
    entities.LegalReferral.TribunalId = tribunalId;
    entities.LegalReferral.Populated = true;
  }

  private void UpdateLegalReferralAssignment()
  {
    System.Diagnostics.Debug.Assert(entities.LegalReferralAssignment.Populated);

    var discontinueDate = local.Current.Date;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.LegalReferralAssignment.Populated = false;
    Update("UpdateLegalReferralAssignment",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetDateTime(
          command, "createdTimestamp",
          entities.LegalReferralAssignment.CreatedTimestamp.
            GetValueOrDefault());
        db.SetInt32(command, "spdId", entities.LegalReferralAssignment.SpdId);
        db.SetInt32(command, "offId", entities.LegalReferralAssignment.OffId);
        db.SetString(
          command, "ospCode", entities.LegalReferralAssignment.OspCode);
        db.SetDate(
          command, "ospDate",
          entities.LegalReferralAssignment.OspDate.GetValueOrDefault());
        db.SetString(command, "casNo", entities.LegalReferralAssignment.CasNo);
        db.SetInt32(command, "lgrId", entities.LegalReferralAssignment.LgrId);
      });

    entities.LegalReferralAssignment.DiscontinueDate = discontinueDate;
    entities.LegalReferralAssignment.LastUpdatedBy = lastUpdatedBy;
    entities.LegalReferralAssignment.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.LegalReferralAssignment.Populated = true;
  }

  private void UpdateMonitoredActivityAssignment()
  {
    System.Diagnostics.Debug.Assert(
      entities.MonitoredActivityAssignment.Populated);

    var discontinueDate = local.Current.Date;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.MonitoredActivityAssignment.Populated = false;
    Update("UpdateMonitoredActivityAssignment",
      (db, command) =>
      {
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
    /// <summary>A LrCommentLinesGroup group.</summary>
    [Serializable]
    public class LrCommentLinesGroup
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

    /// <summary>A LrAttachmtsGroup group.</summary>
    [Serializable]
    public class LrAttachmtsGroup
    {
      /// <summary>
      /// A value of DetailLrAttachmts.
      /// </summary>
      [JsonPropertyName("detailLrAttachmts")]
      public LegalReferralAttachment DetailLrAttachmts
      {
        get => detailLrAttachmts ??= new();
        set => detailLrAttachmts = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private LegalReferralAttachment detailLrAttachmts;
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
    /// A value of LegalReferral.
    /// </summary>
    [JsonPropertyName("legalReferral")]
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
    }

    /// <summary>
    /// Gets a value of LrCommentLines.
    /// </summary>
    [JsonIgnore]
    public Array<LrCommentLinesGroup> LrCommentLines => lrCommentLines ??= new(
      LrCommentLinesGroup.Capacity);

    /// <summary>
    /// Gets a value of LrCommentLines for json serialization.
    /// </summary>
    [JsonPropertyName("lrCommentLines")]
    [Computed]
    public IList<LrCommentLinesGroup> LrCommentLines_Json
    {
      get => lrCommentLines;
      set => LrCommentLines.Assign(value);
    }

    /// <summary>
    /// Gets a value of LrAttachmts.
    /// </summary>
    [JsonIgnore]
    public Array<LrAttachmtsGroup> LrAttachmts => lrAttachmts ??= new(
      LrAttachmtsGroup.Capacity);

    /// <summary>
    /// Gets a value of LrAttachmts for json serialization.
    /// </summary>
    [JsonPropertyName("lrAttachmts")]
    [Computed]
    public IList<LrAttachmtsGroup> LrAttachmts_Json
    {
      get => lrAttachmts;
      set => LrAttachmts.Assign(value);
    }

    private Case1 case1;
    private LegalReferral legalReferral;
    private Array<LrCommentLinesGroup> lrCommentLines;
    private Array<LrAttachmtsGroup> lrAttachmts;
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
    /// A value of TbdLocalLegalReferral.
    /// </summary>
    [JsonPropertyName("tbdLocalLegalReferral")]
    public Infrastructure TbdLocalLegalReferral
    {
      get => tbdLocalLegalReferral ??= new();
      set => tbdLocalLegalReferral = value;
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
    /// A value of NumberOfReferrals.
    /// </summary>
    [JsonPropertyName("numberOfReferrals")]
    public Common NumberOfReferrals
    {
      get => numberOfReferrals ??= new();
      set => numberOfReferrals = value;
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
    /// A value of NextSequenceNo.
    /// </summary>
    [JsonPropertyName("nextSequenceNo")]
    public Common NextSequenceNo
    {
      get => nextSequenceNo ??= new();
      set => nextSequenceNo = value;
    }

    private Infrastructure tbdLocalLegalReferral;
    private Infrastructure infrastructure;
    private Common numberOfReferrals;
    private DateWorkArea current;
    private Common nextSequenceNo;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Other.
    /// </summary>
    [JsonPropertyName("other")]
    public LegalReferral Other
    {
      get => other ??= new();
      set => other = value;
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
    /// A value of MonitoredActivityAssignment.
    /// </summary>
    [JsonPropertyName("monitoredActivityAssignment")]
    public MonitoredActivityAssignment MonitoredActivityAssignment
    {
      get => monitoredActivityAssignment ??= new();
      set => monitoredActivityAssignment = value;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
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
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
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
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
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
    /// A value of LegalReferralAttachment.
    /// </summary>
    [JsonPropertyName("legalReferralAttachment")]
    public LegalReferralAttachment LegalReferralAttachment
    {
      get => legalReferralAttachment ??= new();
      set => legalReferralAttachment = value;
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
    /// A value of LegalReferralComment.
    /// </summary>
    [JsonPropertyName("legalReferralComment")]
    public LegalReferralComment LegalReferralComment
    {
      get => legalReferralComment ??= new();
      set => legalReferralComment = value;
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

    private LegalReferral other;
    private Infrastructure infrastructure;
    private MonitoredActivityAssignment monitoredActivityAssignment;
    private MonitoredActivity monitoredActivity;
    private CaseRole caseRole;
    private LegalActionAssigment legalActionAssigment;
    private LegalActionCaseRole legalActionCaseRole;
    private LegalAction legalAction;
    private OfficeServiceProvider officeServiceProvider;
    private LegalReferralAssignment legalReferralAssignment;
    private LegalReferralAttachment legalReferralAttachment;
    private LegalReferral legalReferral;
    private LegalReferralComment legalReferralComment;
    private Case1 case1;
  }
#endregion
}
