// Program: FN_REMOVE_OBLIGATION, ID: 372095435, model: 746.
// Short name: SWE00598
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
/// A program: FN_REMOVE_OBLIGATION.
/// </para>
/// <para>
/// RESP: FINANCE
/// This action block deletes an Obligation.
/// </para>
/// </summary>
[Serializable]
public partial class FnRemoveObligation: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_REMOVE_OBLIGATION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnRemoveObligation(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnRemoveObligation.
  /// </summary>
  public FnRemoveObligation(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
    this.local = context.GetData<Local>();
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ==================================================
    // ==						==
    // ==	This is used by OFEE, ONAC, & OREC	==
    // ==						==
    // ==================================================
    // ================================================
    // 9-15-98  B Adams    Imported current-date and -timestamp; fixed SET
    // 		statement which assigned the zero date to
    // 		Payment-Status-History discontinue-date
    // 12/31/1998 - bud adams  -  READ properties set
    // ================================================
    // *********************************************************************
    // A Recovery Obligation can only be DELETED if attempted on the Creation 
    // Date.
    // **********************************************************************
    // <<<RBM   02/06/98  As Per Tom Redmond, the previous criteria to allow 
    // deletion of an Obligation if attempt is made on the Date of Creation
    // irrespective of Activities is overridden.
    // The old criterion that we can delete an obligation only if no activities 
    // or cash has been tied to it, is reinforced.
    // =================================================
    // B Adams - It doesn't matter.  All Debt Procedures prevent this
    //   from being executed if the Creation Date of the Obligation is
    //   earlier than Current_Date.
    // =================================================
    ExitState = "ACO_NN0000_ALL_OK";

    if (!ReadCsePersonAccount())
    {
      ExitState = "CSE_PERSON_NOT_AN_OBLIGOR";

      return;
    }

    if (!ReadObligation3())
    {
      ExitState = "FN0000_OBLIGATION_NF";

      return;
    }

    // ***************************************************************
    // The following code was add to check to see if the obligation is
    // related a payment request.  If so the payment request will be
    // disassociated and the payment status history updated for that
    // payment request.
    // Skip Hardy  MTW  12/15/1997
    // ***************************************************************
    // ****** Read payment request ******
    if (ReadPaymentRequest())
    {
      // ****** Disassociate payment request and obligation ******
      try
      {
        UpdatePaymentRequest();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_PAYMENT_REQUEST_NU_RB";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_PAYMENT_REQUEST_PV_RB";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      // ****** Read current payment status history for payment request ******
      if (ReadPaymentStatusHistory())
      {
        // ****** Update current payment status history with discontinue date 
        // ******
        // ***************************************************************
        // discontinue-date  had been SET to a local view which only
        // had an initialized value (zero date)    9/15/98  B Adams
        // ***************************************************************
        try
        {
          UpdatePaymentStatusHistory();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_PYMNT_STAT_HIST_NU_RB";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_PYMNT_STAT_HIST_PV_RB";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        local.PaymentStatusHistoryId.Attribute3DigitRandomNumber =
          entities.PaymentStatusHistory.SystemGeneratedIdentifier + 1;
      }
      else
      {
        ExitState = "FN0000_PYMNT_STAT_NF_RB";
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      // ****** Read payment status for the status of Recovery Requested ******
      if (ReadPaymentStatus())
      {
        // ****** Create payment status history with status of Recovery Created 
        // ******
        try
        {
          CreatePaymentStatusHistory();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_PYMNT_STAT_HIST_AE_RB";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_PYMNT_STAT_HIST_PV_RB";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
      else
      {
        ExitState = "FN0000_PYMNT_STAT_NF_RB";
      }
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // =================================================
    // 1/3/00 - bud adams  -  PR# 83291: If the obligaiton being
    //   deleted is either a Primary or Secondary, disassociate
    //   the other obligation and change its Primary_Secondary_
    //   Code to SPACES.
    //   These Reads are not done as a join because primary_
    //   secondary obligation is going to be retrieved with a cursor
    //   - because that's how IEF does Updates.
    //   NOTE: The Obligation_Rln record will be deleted by the RI
    //   trigger as soon as the Obligation is deleted.
    // =================================================
    if (AsChar(entities.Obligation.PrimarySecondaryCode) == 'P' || AsChar
      (entities.Obligation.PrimarySecondaryCode) == 'S')
    {
      local.HcOrrPrimarySecondary.SequentialGeneratedIdentifier = 1;

      if (ReadObligationRln1())
      {
        if (!ReadObligation2())
        {
          ExitState = "FN0000_PRIMARY_SECONDRY_OBLIG_NF";
        }
      }
      else if (ReadObligationRln2())
      {
        if (!ReadObligation1())
        {
          ExitState = "FN0000_PRIMARY_SECONDRY_OBLIG_NF";
        }
      }
      else
      {
        ExitState = "FN0000_OBLIG_RLN_NF_RB";
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      try
      {
        UpdateObligation();

        // =================================================
        // 1/3/00 - b adams  -  NOTE: The Obligation_Rln record will be
        //   deleted by the RI trigger when the related Obligation is
        //   deleted.  When the Obligation_Rln record is deleted, the
        //   second Obligation will be disassociated from it.  (Happy
        //   Y2K New Year!)
        // =================================================
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_OBLIGATION_NU_RB";

            return;
          case ErrorCode.PermittedValueViolation:
            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      // ***---  end of Primary / Secondary processing
    }

    // **************************************************************
    // Skip Hardy  12/05/1997  Check to see if there are any Obligation_
    // Assignment records for the current Obligation.  If so delete
    // the Obligation_Assignment.
    // **************************************************************
    foreach(var item in ReadObligationAssignment())
    {
      DeleteObligationAssignment();
    }

    // =================================================
    // 1/23/99 - B Adams  -  delete rules will handle the commented
    //   out DELETE actions.  This may clear up the abend problem.
    // =================================================
    foreach(var item in ReadObligationTransaction())
    {
      if (ReadDebtDetail())
      {
        DeleteDebtDetail();
        local.DebtDetailFound.Flag = "Y";
      }
      else
      {
        // *** Continue Processing
      }

      DeleteObligationTransaction();
    }

    // =================================================
    // 11/11/1998 - B Adams  -  If this was a Recovery obligation
    //   being removed, and it was the last active one extant for the
    //   current cse-person-account, then Delete the active
    //   Recapture_Rule (Obligor_Rule) for that account, if one does
    //   exist - and it should.
    // =================================================
    if (AsChar(import.ObligationType.Classification) == AsChar
      (import.HcOtCRecoveryClassifi.Classification))
    {
      if (ReadObligationDebtDetail())
      {
        // ***  There's still at least one active Recovery debt for this account
      }
      else if (ReadRecaptureRule())
      {
        DeleteRecaptureRule();
      }
    }

    // <<< RBM   11/11/97  Raise an Event when an Obligation is Deleted >>>
    switch(AsChar(import.ObligationType.Classification))
    {
      case 'A':
        local.Infrastructure.UserId = "OACC";

        break;
      case 'N':
        local.Infrastructure.UserId = "ONAC";

        break;
      case 'R':
        local.Infrastructure.UserId = "OREC";

        break;
      case 'V':
        local.Infrastructure.UserId = "OVOL";

        break;
      default:
        local.Infrastructure.UserId = "OFEE";

        break;
    }

    // ---------------------------------------------------------------------
    // 12/29/97	Venkatesh Kamaraj   Set situation # to 0 instead of the call to 
    // get_next_situation_no because of infrastructure changes
    // ----------------------------------------------------------------------
    local.Infrastructure.SituationNumber = 0;
    local.Infrastructure.ReferenceDate = import.Current.Date;
    local.Infrastructure.CsePersonNumber = import.Obligor.Number;
    local.Infrastructure.EventId = 47;
    local.Infrastructure.BusinessObjectCd = "OBL";
    local.Infrastructure.DenormNumeric12 =
      import.Obligation.SystemGeneratedIdentifier;
    local.Infrastructure.DenormText12 = import.ObligationType.Code;
    local.ActivityType.Text4 = "DELE";
    UseFnCabRaiseEvent();
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseFnCabRaiseEvent()
  {
    var useImport = new FnCabRaiseEvent.Import();
    var useExport = new FnCabRaiseEvent.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);
    useImport.ActivityType.Text4 = local.ActivityType.Text4;
    useImport.Current.Timestamp = import.Current.Timestamp;
    useImport.ObligationType.Assign(import.ObligationType);

    Call(FnCabRaiseEvent.Execute, useImport, useExport);
  }

  private void CreatePaymentStatusHistory()
  {
    var pstGeneratedId = entities.PaymentStatus.SystemGeneratedIdentifier;
    var prqGeneratedId = entities.PaymentRequest.SystemGeneratedIdentifier;
    var systemGeneratedIdentifier =
      local.PaymentStatusHistoryId.Attribute3DigitRandomNumber;
    var effectiveDate = import.Current.Date;
    var discontinueDate = UseCabSetMaximumDiscontinueDate();
    var createdBy = global.UserId;
    var createdTimestamp = import.Current.Timestamp;

    entities.PaymentStatusHistory.Populated = false;
    Update("CreatePaymentStatusHistory",
      (db, command) =>
      {
        db.SetInt32(command, "pstGeneratedId", pstGeneratedId);
        db.SetInt32(command, "prqGeneratedId", prqGeneratedId);
        db.SetInt32(command, "pymntStatHistId", systemGeneratedIdentifier);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "reasonText", "");
      });

    entities.PaymentStatusHistory.PstGeneratedId = pstGeneratedId;
    entities.PaymentStatusHistory.PrqGeneratedId = prqGeneratedId;
    entities.PaymentStatusHistory.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.PaymentStatusHistory.EffectiveDate = effectiveDate;
    entities.PaymentStatusHistory.DiscontinueDate = discontinueDate;
    entities.PaymentStatusHistory.CreatedBy = createdBy;
    entities.PaymentStatusHistory.CreatedTimestamp = createdTimestamp;
    entities.PaymentStatusHistory.ReasonText = "";
    entities.PaymentStatusHistory.Populated = true;
  }

  private void DeleteDebtDetail()
  {
    Update("DeleteDebtDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "obgGeneratedId", entities.DebtDetail.ObgGeneratedId);
        db.SetString(command, "cspNumber", entities.DebtDetail.CspNumber);
        db.SetString(command, "cpaType", entities.DebtDetail.CpaType);
        db.SetInt32(
          command, "otrGeneratedId", entities.DebtDetail.OtrGeneratedId);
        db.SetInt32(command, "otyType", entities.DebtDetail.OtyType);
        db.SetString(command, "otrType", entities.DebtDetail.OtrType);
      });
  }

  private void DeleteObligationAssignment()
  {
    Update("DeleteObligationAssignment",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          entities.ObligationAssignment.CreatedTimestamp.GetValueOrDefault());
        db.SetInt32(command, "spdId", entities.ObligationAssignment.SpdId);
        db.SetInt32(command, "offId", entities.ObligationAssignment.OffId);
        db.SetString(command, "ospCode", entities.ObligationAssignment.OspCode);
        db.SetDate(
          command, "ospDate",
          entities.ObligationAssignment.OspDate.GetValueOrDefault());
        db.SetInt32(command, "otyId", entities.ObligationAssignment.OtyId);
        db.SetString(command, "cpaType", entities.ObligationAssignment.CpaType);
        db.SetString(command, "cspNo", entities.ObligationAssignment.CspNo);
        db.SetInt32(command, "obgId", entities.ObligationAssignment.ObgId);
      });
  }

  private void DeleteObligationTransaction()
  {
    var obgGeneratedId = entities.ObligationTransaction.ObgGeneratedId;
    var cspNumber = entities.ObligationTransaction.CspNumber;
    var cpaType = entities.ObligationTransaction.CpaType;
    var otyType = entities.ObligationTransaction.OtyType;
    bool exists;

    Update("DeleteObligationTransaction#1",
      (db, command) =>
      {
        db.SetNullableInt32(command, "obgGeneratedId1", obgGeneratedId);
        db.SetNullableString(command, "cspRNumber", cspNumber);
        db.SetNullableString(command, "cpaRType", cpaType);
        db.SetNullableInt32(
          command, "otrGeneratedId",
          entities.ObligationTransaction.SystemGeneratedIdentifier);
        db.SetNullableString(
          command, "otrType", entities.ObligationTransaction.Type1);
        db.SetNullableInt32(command, "otyType1", otyType);
      });

    Update("DeleteObligationTransaction#2",
      (db, command) =>
      {
        db.SetNullableInt32(command, "obgGeneratedId1", obgGeneratedId);
        db.SetNullableString(command, "cspRNumber", cspNumber);
        db.SetNullableString(command, "cpaRType", cpaType);
        db.SetNullableInt32(
          command, "otrGeneratedId",
          entities.ObligationTransaction.SystemGeneratedIdentifier);
        db.SetNullableString(
          command, "otrType", entities.ObligationTransaction.Type1);
        db.SetNullableInt32(command, "otyType1", otyType);
      });

    exists = Read("DeleteObligationTransaction#3",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cpaSupType", entities.ObligationTransaction.CpaSupType ?? ""
          );
        db.SetNullableString(
          command, "cspSupNumber",
          entities.ObligationTransaction.CspSupNumber ?? "");
      },
      null);

    if (!exists)
    {
      Update("DeleteObligationTransaction#4",
        (db, command) =>
        {
          db.SetNullableString(
            command, "cpaSupType",
            entities.ObligationTransaction.CpaSupType ?? "");
          db.SetNullableString(
            command, "cspSupNumber",
            entities.ObligationTransaction.CspSupNumber ?? "");
        });
    }

    exists = Read("DeleteObligationTransaction#5",
      (db, command) =>
      {
        db.SetInt32(command, "otyType2", otyType);
        db.SetInt32(command, "obgGeneratedId2", obgGeneratedId);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "cpaType", cpaType);
      },
      null);

    if (!exists)
    {
      Update("DeleteObligationTransaction#6",
        (db, command) =>
        {
          db.SetInt32(command, "otyType2", otyType);
          db.SetInt32(command, "obgGeneratedId2", obgGeneratedId);
          db.SetString(command, "cspNumber", cspNumber);
          db.SetString(command, "cpaType", cpaType);
        });

      exists = Read("DeleteObligationTransaction#7",
        (db, command) =>
        {
          db.SetInt32(command, "otyType2", otyType);
          db.SetInt32(command, "obgGeneratedId2", obgGeneratedId);
          db.SetString(command, "cspNumber", cspNumber);
          db.SetString(command, "cpaType", cpaType);
        },
        null);

      if (!exists)
      {
        Update("DeleteObligationTransaction#8",
          (db, command) =>
          {
            db.SetString(command, "cpaType", cspNumber);
            db.SetString(command, "cspNumber", cpaType);
          });
      }
    }
  }

  private void DeleteRecaptureRule()
  {
    Update("DeleteRecaptureRule",
      (db, command) =>
      {
        db.SetInt32(
          command, "recaptureRuleId",
          entities.RecaptureRule.SystemGeneratedIdentifier);
      });
  }

  private bool ReadCsePersonAccount()
  {
    entities.CsePersonAccount.Populated = false;

    return Read("ReadCsePersonAccount",
      (db, command) =>
      {
        db.SetString(command, "type", import.HcCpaObligor.Type1);
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 0);
        entities.CsePersonAccount.Type1 = db.GetString(reader, 1);
        entities.CsePersonAccount.AsOfDtRecBal =
          db.GetNullableDecimal(reader, 2);
        entities.CsePersonAccount.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.CsePersonAccount.Type1);
      });
  }

  private bool ReadDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.DebtDetail.Populated = false;

    return Read("ReadDebtDetail",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.ObligationTransaction.OtyType);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ObligationTransaction.ObgGeneratedId);
        db.SetString(command, "otrType", entities.ObligationTransaction.Type1);
        db.SetInt32(
          command, "otrGeneratedId",
          entities.ObligationTransaction.SystemGeneratedIdentifier);
        db.
          SetString(command, "cpaType", entities.ObligationTransaction.CpaType);
          
        db.SetString(
          command, "cspNumber", entities.ObligationTransaction.CspNumber);
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 6);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 7);
        entities.DebtDetail.CoveredPrdEndDt = db.GetNullableDate(reader, 8);
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
      });
  }

  private bool ReadObligation1()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationRln.Populated);
    entities.PrimarySecondary.Populated = false;

    return Read("ReadObligation1",
      (db, command) =>
      {
        db.
          SetInt32(command, "dtyGeneratedId", entities.ObligationRln.OtyFirstId);
          
        db.SetInt32(command, "obId", entities.ObligationRln.ObgFGeneratedId);
        db.SetString(command, "cspNumber", entities.ObligationRln.CspFNumber);
        db.SetString(command, "cpaType", entities.ObligationRln.CpaFType);
      },
      (db, reader) =>
      {
        entities.PrimarySecondary.CpaType = db.GetString(reader, 0);
        entities.PrimarySecondary.CspNumber = db.GetString(reader, 1);
        entities.PrimarySecondary.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.PrimarySecondary.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.PrimarySecondary.PrimarySecondaryCode =
          db.GetNullableString(reader, 4);
        entities.PrimarySecondary.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.PrimarySecondary.LastUpdateTmst =
          db.GetNullableDateTime(reader, 6);
        entities.PrimarySecondary.Populated = true;
        CheckValid<Obligation>("CpaType", entities.PrimarySecondary.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.PrimarySecondary.PrimarySecondaryCode);
      });
  }

  private bool ReadObligation2()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationRln.Populated);
    entities.PrimarySecondary.Populated = false;

    return Read("ReadObligation2",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtyGeneratedId", entities.ObligationRln.OtySecondId);
        db.SetInt32(command, "obId", entities.ObligationRln.ObgGeneratedId);
        db.SetString(command, "cspNumber", entities.ObligationRln.CspNumber);
        db.SetString(command, "cpaType", entities.ObligationRln.CpaType);
      },
      (db, reader) =>
      {
        entities.PrimarySecondary.CpaType = db.GetString(reader, 0);
        entities.PrimarySecondary.CspNumber = db.GetString(reader, 1);
        entities.PrimarySecondary.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.PrimarySecondary.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.PrimarySecondary.PrimarySecondaryCode =
          db.GetNullableString(reader, 4);
        entities.PrimarySecondary.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.PrimarySecondary.LastUpdateTmst =
          db.GetNullableDateTime(reader, 6);
        entities.PrimarySecondary.Populated = true;
        CheckValid<Obligation>("CpaType", entities.PrimarySecondary.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.PrimarySecondary.PrimarySecondaryCode);
      });
  }

  private bool ReadObligation3()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAccount.Populated);
    entities.Obligation.Populated = false;

    return Read("ReadObligation3",
      (db, command) =>
      {
        db.
          SetInt32(command, "obId", import.Obligation.SystemGeneratedIdentifier);
          
        db.SetString(command, "cpaType", entities.CsePersonAccount.Type1);
        db.SetString(command, "cspNumber", entities.CsePersonAccount.CspNumber);
        db.SetInt32(
          command, "dtyGeneratedId",
          import.ObligationType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.PrqId = db.GetNullableInt32(reader, 4);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 5);
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
      });
  }

  private IEnumerable<bool> ReadObligationAssignment()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationAssignment.Populated = false;

    return ReadEach("ReadObligationAssignment",
      (db, command) =>
      {
        db.SetInt32(
          command, "obgId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNo", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetInt32(command, "otyId", entities.Obligation.DtyGeneratedId);
      },
      (db, reader) =>
      {
        entities.ObligationAssignment.CreatedBy = db.GetString(reader, 0);
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

        return true;
      });
  }

  private bool ReadObligationDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAccount.Populated);
    entities.DebtDetail.Populated = false;
    entities.Obligation.Populated = false;

    return Read("ReadObligationDebtDetail",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.CsePersonAccount.Type1);
        db.SetString(command, "cspNumber", entities.CsePersonAccount.CspNumber);
        db.SetString(
          command, "debtTypClass", import.HcOtCRecoveryClassifi.Classification);
          
        db.SetString(command, "debtTyp", import.HcOtrnDtDebtDetail.DebtType);
        db.SetString(command, "obTrnTyp", import.HcOtrnTDebt.Type1);
        db.SetNullableDate(
          command, "cvdPrdEndDt", import.Max.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "cvrdPrdStartDt", import.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.PrqId = db.GetNullableInt32(reader, 4);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 5);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 6);
        entities.DebtDetail.CspNumber = db.GetString(reader, 7);
        entities.DebtDetail.CpaType = db.GetString(reader, 8);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 9);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 10);
        entities.DebtDetail.OtrType = db.GetString(reader, 11);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 12);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 13);
        entities.DebtDetail.CoveredPrdEndDt = db.GetNullableDate(reader, 14);
        entities.DebtDetail.Populated = true;
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
      });
  }

  private bool ReadObligationRln1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationRln.Populated = false;

    return Read("ReadObligationRln1",
      (db, command) =>
      {
        db.SetInt32(command, "otyFirstId", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgFGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspFNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaFType", entities.Obligation.CpaType);
        db.SetInt32(
          command, "orrGeneratedId",
          local.HcOrrPrimarySecondary.SequentialGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ObligationRln.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ObligationRln.CspNumber = db.GetString(reader, 1);
        entities.ObligationRln.CpaType = db.GetString(reader, 2);
        entities.ObligationRln.ObgFGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationRln.CspFNumber = db.GetString(reader, 4);
        entities.ObligationRln.CpaFType = db.GetString(reader, 5);
        entities.ObligationRln.OrrGeneratedId = db.GetInt32(reader, 6);
        entities.ObligationRln.CreatedBy = db.GetString(reader, 7);
        entities.ObligationRln.OtySecondId = db.GetInt32(reader, 8);
        entities.ObligationRln.OtyFirstId = db.GetInt32(reader, 9);
        entities.ObligationRln.Populated = true;
        CheckValid<ObligationRln>("CpaType", entities.ObligationRln.CpaType);
        CheckValid<ObligationRln>("CpaFType", entities.ObligationRln.CpaFType);
      });
  }

  private bool ReadObligationRln2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationRln.Populated = false;

    return Read("ReadObligationRln2",
      (db, command) =>
      {
        db.SetInt32(command, "otySecondId", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetInt32(
          command, "orrGeneratedId",
          local.HcOrrPrimarySecondary.SequentialGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ObligationRln.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ObligationRln.CspNumber = db.GetString(reader, 1);
        entities.ObligationRln.CpaType = db.GetString(reader, 2);
        entities.ObligationRln.ObgFGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationRln.CspFNumber = db.GetString(reader, 4);
        entities.ObligationRln.CpaFType = db.GetString(reader, 5);
        entities.ObligationRln.OrrGeneratedId = db.GetInt32(reader, 6);
        entities.ObligationRln.CreatedBy = db.GetString(reader, 7);
        entities.ObligationRln.OtySecondId = db.GetInt32(reader, 8);
        entities.ObligationRln.OtyFirstId = db.GetInt32(reader, 9);
        entities.ObligationRln.Populated = true;
        CheckValid<ObligationRln>("CpaType", entities.ObligationRln.CpaType);
        CheckValid<ObligationRln>("CpaFType", entities.ObligationRln.CpaFType);
      });
  }

  private IEnumerable<bool> ReadObligationTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationTransaction.Populated = false;

    return ReadEach("ReadObligationTransaction",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 1);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 2);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 4);
        entities.ObligationTransaction.DebtType = db.GetString(reader, 5);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 6);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 7);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 8);
        entities.ObligationTransaction.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.ObligationTransaction.CpaType);
        CheckValid<ObligationTransaction>("Type1",
          entities.ObligationTransaction.Type1);
        CheckValid<ObligationTransaction>("DebtType",
          entities.ObligationTransaction.DebtType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ObligationTransaction.CpaSupType);

        return true;
      });
  }

  private bool ReadPaymentRequest()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.PaymentRequest.Populated = false;

    return Read("ReadPaymentRequest",
      (db, command) =>
      {
        db.SetInt32(
          command, "paymentRequestId",
          entities.Obligation.PrqId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentRequest.ProcessDate = db.GetDate(reader, 1);
        entities.PaymentRequest.Type1 = db.GetString(reader, 2);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 3);
        entities.PaymentRequest.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.PaymentRequest.Type1);
      });
  }

  private bool ReadPaymentStatus()
  {
    entities.PaymentStatus.Populated = false;

    return Read("ReadPaymentStatus",
      null,
      (db, reader) =>
      {
        entities.PaymentStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentStatus.Code = db.GetString(reader, 1);
        entities.PaymentStatus.Populated = true;
      });
  }

  private bool ReadPaymentStatusHistory()
  {
    entities.PaymentStatusHistory.Populated = false;

    return Read("ReadPaymentStatusHistory",
      (db, command) =>
      {
        db.SetInt32(
          command, "prqGeneratedId",
          entities.PaymentRequest.SystemGeneratedIdentifier);
        db.SetDate(
          command, "effectiveDate", import.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PaymentStatusHistory.PstGeneratedId = db.GetInt32(reader, 0);
        entities.PaymentStatusHistory.PrqGeneratedId = db.GetInt32(reader, 1);
        entities.PaymentStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.PaymentStatusHistory.EffectiveDate = db.GetDate(reader, 3);
        entities.PaymentStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.PaymentStatusHistory.CreatedBy = db.GetString(reader, 5);
        entities.PaymentStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.PaymentStatusHistory.ReasonText =
          db.GetNullableString(reader, 7);
        entities.PaymentStatusHistory.Populated = true;
      });
  }

  private bool ReadRecaptureRule()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAccount.Populated);
    entities.RecaptureRule.Populated = false;

    return Read("ReadRecaptureRule",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cpaDType", entities.CsePersonAccount.Type1);
        db.SetNullableString(
          command, "cspDNumber", entities.CsePersonAccount.CspNumber);
        db.SetNullableDate(
          command, "discontinueDate", import.Max.Date.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate", import.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.RecaptureRule.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.RecaptureRule.CpaDType = db.GetNullableString(reader, 1);
        entities.RecaptureRule.CspDNumber = db.GetNullableString(reader, 2);
        entities.RecaptureRule.EffectiveDate = db.GetDate(reader, 3);
        entities.RecaptureRule.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.RecaptureRule.Type1 = db.GetString(reader, 5);
        entities.RecaptureRule.Populated = true;
        CheckValid<RecaptureRule>("CpaDType", entities.RecaptureRule.CpaDType);
        CheckValid<RecaptureRule>("Type1", entities.RecaptureRule.Type1);
      });
  }

  private void UpdateObligation()
  {
    System.Diagnostics.Debug.Assert(entities.PrimarySecondary.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdateTmst = import.Current.Timestamp;

    CheckValid<Obligation>("PrimarySecondaryCode", "");
    entities.PrimarySecondary.Populated = false;
    Update("UpdateObligation",
      (db, command) =>
      {
        db.SetNullableString(command, "primSecCd", "");
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdateTmst", lastUpdateTmst);
        db.SetString(command, "cpaType", entities.PrimarySecondary.CpaType);
        db.SetString(command, "cspNumber", entities.PrimarySecondary.CspNumber);
        db.SetInt32(
          command, "obId", entities.PrimarySecondary.SystemGeneratedIdentifier);
          
        db.SetInt32(
          command, "dtyGeneratedId", entities.PrimarySecondary.DtyGeneratedId);
      });

    entities.PrimarySecondary.PrimarySecondaryCode = "";
    entities.PrimarySecondary.LastUpdatedBy = lastUpdatedBy;
    entities.PrimarySecondary.LastUpdateTmst = lastUpdateTmst;
    entities.PrimarySecondary.Populated = true;
  }

  private void UpdatePaymentRequest()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);

    var processDate = import.Current.Date;
    var cpaType = entities.Obligation.CpaType;
    var cspNumber = entities.Obligation.CspNumber;

    entities.PaymentRequest.Populated = false;
    entities.Obligation.Populated = false;

    bool exists;

    Update("UpdatePaymentRequest#1",
      (db, command) =>
      {
        db.SetDate(command, "processDate", processDate);
        db.SetInt32(
          command, "paymentRequestId",
          entities.PaymentRequest.SystemGeneratedIdentifier);
      });

    Update("UpdatePaymentRequest#2",
      (db, command) =>
      {
        db.SetString(command, "cpaType1", cpaType);
        db.SetString(command, "cspNumber1", cspNumber);
        db.SetInt32(
          command, "obId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "dtyGeneratedId", entities.Obligation.DtyGeneratedId);
      });

    exists = Read("UpdatePaymentRequest#3",
      (db, command) =>
      {
        db.SetString(command, "cpaType2", cpaType);
        db.SetString(command, "cspNumber2", cspNumber);
      },
      null);

    if (!exists)
    {
      Update("UpdatePaymentRequest#4",
        (db, command) =>
        {
          db.SetString(command, "cpaType2", cpaType);
          db.SetString(command, "cspNumber2", cspNumber);
        });
    }

    entities.PaymentRequest.ProcessDate = processDate;
    entities.Obligation.PrqId = null;
    entities.PaymentRequest.Populated = true;
    entities.Obligation.Populated = true;
  }

  private void UpdatePaymentStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.PaymentStatusHistory.Populated);

    var discontinueDate = import.Current.Date;

    entities.PaymentStatusHistory.Populated = false;
    Update("UpdatePaymentStatusHistory",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetInt32(
          command, "pstGeneratedId",
          entities.PaymentStatusHistory.PstGeneratedId);
        db.SetInt32(
          command, "prqGeneratedId",
          entities.PaymentStatusHistory.PrqGeneratedId);
        db.SetInt32(
          command, "pymntStatHistId",
          entities.PaymentStatusHistory.SystemGeneratedIdentifier);
      });

    entities.PaymentStatusHistory.DiscontinueDate = discontinueDate;
    entities.PaymentStatusHistory.Populated = true;
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local;
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
    /// A value of HcCpaObligor.
    /// </summary>
    [JsonPropertyName("hcCpaObligor")]
    public CsePersonAccount HcCpaObligor
    {
      get => hcCpaObligor ??= new();
      set => hcCpaObligor = value;
    }

    /// <summary>
    /// A value of HcOtrnDtDebtDetail.
    /// </summary>
    [JsonPropertyName("hcOtrnDtDebtDetail")]
    public ObligationTransaction HcOtrnDtDebtDetail
    {
      get => hcOtrnDtDebtDetail ??= new();
      set => hcOtrnDtDebtDetail = value;
    }

    /// <summary>
    /// A value of HcOtrnTDebt.
    /// </summary>
    [JsonPropertyName("hcOtrnTDebt")]
    public ObligationTransaction HcOtrnTDebt
    {
      get => hcOtrnTDebt ??= new();
      set => hcOtrnTDebt = value;
    }

    /// <summary>
    /// A value of HcOtCRecoveryClassifi.
    /// </summary>
    [JsonPropertyName("hcOtCRecoveryClassifi")]
    public ObligationType HcOtCRecoveryClassifi
    {
      get => hcOtCRecoveryClassifi ??= new();
      set => hcOtCRecoveryClassifi = value;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    private CsePersonAccount hcCpaObligor;
    private ObligationTransaction hcOtrnDtDebtDetail;
    private ObligationTransaction hcOtrnTDebt;
    private ObligationType hcOtCRecoveryClassifi;
    private DateWorkArea max;
    private DateWorkArea current;
    private ObligationType obligationType;
    private CsePerson csePerson;
    private Obligation obligation;
    private CsePerson obligor;
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
  public class Local: IInitializable
  {
    /// <summary>
    /// A value of HcOrrPrimarySecondary.
    /// </summary>
    [JsonPropertyName("hcOrrPrimarySecondary")]
    public ObligationRlnRsn HcOrrPrimarySecondary
    {
      get => hcOrrPrimarySecondary ??= new();
      set => hcOrrPrimarySecondary = value;
    }

    /// <summary>
    /// A value of ActiveObligation.
    /// </summary>
    [JsonPropertyName("activeObligation")]
    public Common ActiveObligation
    {
      get => activeObligation ??= new();
      set => activeObligation = value;
    }

    /// <summary>
    /// A value of HardcodedAccruing.
    /// </summary>
    [JsonPropertyName("hardcodedAccruing")]
    public ObligationType HardcodedAccruing
    {
      get => hardcodedAccruing ??= new();
      set => hardcodedAccruing = value;
    }

    /// <summary>
    /// A value of DebtDetailFound.
    /// </summary>
    [JsonPropertyName("debtDetailFound")]
    public Common DebtDetailFound
    {
      get => debtDetailFound ??= new();
      set => debtDetailFound = value;
    }

    /// <summary>
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
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
    /// A value of ActivityType.
    /// </summary>
    [JsonPropertyName("activityType")]
    public TextWorkArea ActivityType
    {
      get => activityType ??= new();
      set => activityType = value;
    }

    /// <summary>
    /// A value of PaymentStatusHistoryId.
    /// </summary>
    [JsonPropertyName("paymentStatusHistoryId")]
    public SystemGenerated PaymentStatusHistoryId
    {
      get => paymentStatusHistoryId ??= new();
      set => paymentStatusHistoryId = value;
    }

    /// <para>Resets the state.</para>
    void IInitializable.Initialize()
    {
      activeObligation = null;
      hardcodedAccruing = null;
      debtDetailFound = null;
      debtDetail = null;
      infrastructure = null;
      activityType = null;
      paymentStatusHistoryId = null;
    }

    private ObligationRlnRsn hcOrrPrimarySecondary;
    private Common activeObligation;
    private ObligationType hardcodedAccruing;
    private Common debtDetailFound;
    private DebtDetail debtDetail;
    private Infrastructure infrastructure;
    private TextWorkArea activityType;
    private SystemGenerated paymentStatusHistoryId;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of PrimarySecondary.
    /// </summary>
    [JsonPropertyName("primarySecondary")]
    public Obligation PrimarySecondary
    {
      get => primarySecondary ??= new();
      set => primarySecondary = value;
    }

    /// <summary>
    /// A value of ObligationRlnRsn.
    /// </summary>
    [JsonPropertyName("obligationRlnRsn")]
    public ObligationRlnRsn ObligationRlnRsn
    {
      get => obligationRlnRsn ??= new();
      set => obligationRlnRsn = value;
    }

    /// <summary>
    /// A value of ObligationRln.
    /// </summary>
    [JsonPropertyName("obligationRln")]
    public ObligationRln ObligationRln
    {
      get => obligationRln ??= new();
      set => obligationRln = value;
    }

    /// <summary>
    /// A value of RecaptureRule.
    /// </summary>
    [JsonPropertyName("recaptureRule")]
    public RecaptureRule RecaptureRule
    {
      get => recaptureRule ??= new();
      set => recaptureRule = value;
    }

    /// <summary>
    /// A value of PaymentStatusHistory.
    /// </summary>
    [JsonPropertyName("paymentStatusHistory")]
    public PaymentStatusHistory PaymentStatusHistory
    {
      get => paymentStatusHistory ??= new();
      set => paymentStatusHistory = value;
    }

    /// <summary>
    /// A value of PaymentStatus.
    /// </summary>
    [JsonPropertyName("paymentStatus")]
    public PaymentStatus PaymentStatus
    {
      get => paymentStatus ??= new();
      set => paymentStatus = value;
    }

    /// <summary>
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
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
    /// A value of ObligationAssignment.
    /// </summary>
    [JsonPropertyName("obligationAssignment")]
    public ObligationAssignment ObligationAssignment
    {
      get => obligationAssignment ??= new();
      set => obligationAssignment = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
    }

    /// <summary>
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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

    private Obligation primarySecondary;
    private ObligationRlnRsn obligationRlnRsn;
    private ObligationRln obligationRln;
    private RecaptureRule recaptureRule;
    private PaymentStatusHistory paymentStatusHistory;
    private PaymentStatus paymentStatus;
    private PaymentRequest paymentRequest;
    private Infrastructure infrastructure;
    private ObligationAssignment obligationAssignment;
    private ObligationType obligationType;
    private ObligationTransaction obligationTransaction;
    private DebtDetail debtDetail;
    private CsePersonAccount csePersonAccount;
    private CsePerson csePerson;
    private Obligation obligation;
  }
#endregion
}
