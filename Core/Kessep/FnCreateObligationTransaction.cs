// Program: FN_CREATE_OBLIGATION_TRANSACTION, ID: 372086315, model: 746.
// Short name: SWE00379
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_CREATE_OBLIGATION_TRANSACTION.
/// </summary>
[Serializable]
public partial class FnCreateObligationTransaction: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CREATE_OBLIGATION_TRANSACTION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCreateObligationTransaction(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCreateObligationTransaction.
  /// </summary>
  public FnCreateObligationTransaction(IContext context, Import import,
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
    // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
    // @@
    // @@ AFTER TRAINING IS DONE, REMOVE THIS READ,
    // @@ ADD 'CLASSIFICATION' TO THE IMPORTED OB_TYPE
    // @@ VIEW, CHANGE REFERENCES TO THIS EA VIEW
    // @@ BELOW TO THE IMPORTED VIEW.
    // @@
    // @@ THIS WAS ADDED TO AVOID MIGRATING OTHER ADS
    // @@
    // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
    // ----------------------------------------------------------------
    // DATE		PROGRAMMER		REF #		DESCRIPTION
    // 03/22/1996	Holly Kennedy-MTW			Added logic to set the
    // 							Type to 'A' instead of 'D'
    // 							for accruing obligations
    // 11/03/97     RBM  Rewrote the CAB Completely
    // ----------------------------------------------------------------
    // 09/02/98   Bud Adams	   Replaced optional ASSOCIATE actions with
    // 			   more optional Create actions; performance
    // 05/30/2000  Vithal Madhira  PR# 87215  Added Preconversion_Program_Code.
    // ***********************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    // =================================================
    // 12/8/98 - B Adams  -  deleted use-fn-hardcode-legal and
    //   imported the one attribute.
    //   READ properties set
    // =================================================
    // : Jan 2002, M. Brown, WO# 020144, Pre-conversion code is now updateable.
    local.AccrualInstructions.DiscontinueDt =
      import.AccrualInstructions.DiscontinueDt;

    if (Equal(import.AccrualInstructions.DiscontinueDt, local.Initialized.Date))
    {
      local.AccrualInstructions.DiscontinueDt = import.Max.Date;
    }

    // ***** Main-Line *****
    // =================================================
    // 2/19/1999 - bud adams  -  removed Read of CSE_Person and
    //   all related persistent view logic.
    // =================================================
    // ================================================
    // Changed this Read.  The WHEN NOT FOUND section had
    // been deleted and the note: "Not Found exception will
    // cause an ABORT".
    // Deleted the READ of Obligation_Type, which was part of
    // an extended Read of Obligation.  Since Obligation will be
    // generating a cursor 'for update of', so, then, will all tables
    // that is part of the extended Read.  7/19/99 - bud adams
    // ================================================
    if (ReadObligation())
    {
      // =================================================
      // 7/20/99 - bud adams  -  Deleted code that did a Read of
      //   Legal_Action and an ASSOCIATE of Obligation.  That
      //   relationship is valued in FN_Create_Obligation.
      // =================================================
    }
    else
    {
      ExitState = "FN0000_OBLIGATION_NF_RB";

      return;
    }

    if (!IsEmpty(import.Supported.Number))
    {
      if (ReadCsePerson())
      {
        if (ReadCsePersonAccount())
        {
          // : SUPPORTED PERSON ACCOUNT already exists - Continue Processing.
        }
        else
        {
          try
          {
            CreateCsePersonAccount();

            // : SUPPORTED PERSON ACCOUNT successfully created.
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_SUPP_PERSON_ACCT_AE_RB";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_SUPP_PERSON_ACCT_PV_RB";

                break;
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
        ExitState = "SUPPORTED_PERSON_NF_RB";
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      // : Make sure the Supported Person is not already on the Obligation.
      if (AsChar(import.ObligationType.Classification) == AsChar
        (import.HcOtCVoluntaryClassif.Classification))
      {
      }
      else if (import.LegalActionDetail.Number != 0)
      {
        if (ReadLegalActionDetail())
        {
          if (!ReadLegalActionPerson1())
          {
            ExitState = "CO0000_LEGAL_ACTION_PERSON_NF_RB";
          }
        }
        else
        {
          ExitState = "LEGAL_ACTION_DETAIL_NF_RB";
        }
      }
      else
      {
        ExitState = "LEGAL_ACTN_DETAIL_NMBR_ABSENT_RB";
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      // *****
      // Create Obligation Transaction.  Loop from 1 to 5 to account for
      // possible duplicate randomly generated identifiers.
      // An accruing obligation will always have a Legal Action Person 
      // associated to it.  This causes a need for an association to be drawn.
      // The else condition will involve recovery obligations as well as Non
      // Accruing obligations.  Therefore the relationship to the Leagal Action
      // Person is not necessarily mandatory.  The above check will cause the
      // program to rollback if a Legal Action Person does not exist for a Non
      // Accruing obligation, resulting in this logic never being hit.
      // *****
      for(local.Work.Count = 1; local.Work.Count <= 5; ++local.Work.Count)
      {
        if (AsChar(import.Accruing.Flag) == 'Y')
        {
          try
          {
            CreateObligationTransaction1();
            export.ObligationTransaction.SystemGeneratedIdentifier =
              entities.ObligationTransaction.SystemGeneratedIdentifier;

            break;
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                if (local.Work.Count < 5)
                {
                  // ***  Next  ***
                }
                else
                {
                  // Duplicate exception has occurred 5 times
                  ExitState = "FN0000_OBLIG_TRANS_AE_RB";
                }

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_OBLIG_TRANS_PV_RB";

                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
        else if (AsChar(import.ObligationType.Classification) == AsChar
          (import.HcOtCVoluntaryClassif.Classification))
        {
          // =================================================
          // 5/26/99 - Bud Adams  -  New_Debt_Process_Date is being
          //   set to Current_Date for Voluntary Obligations only.  This
          //   date is never SET anyplace else, and so will make it look
          //   like there are unprocessed debts.
          //   OVOL requires at least 1 supported person, so this logic
          //   is not needed below.
          // =================================================
          if (entities.LegalActionPerson.Populated)
          {
            try
            {
              CreateObligationTransaction3();
              export.ObligationTransaction.SystemGeneratedIdentifier =
                entities.ObligationTransaction.SystemGeneratedIdentifier;

              break;
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  if (local.Work.Count < 5)
                  {
                    // ***  Next  ***
                  }
                  else
                  {
                    // Duplicate exception has occurred 5 times
                    ExitState = "FN0000_OBLIG_TRANS_AE_RB";
                  }

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "FN0000_OBLIG_TRANS_PV_RB";

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
            try
            {
              CreateObligationTransaction5();
              export.ObligationTransaction.SystemGeneratedIdentifier =
                entities.ObligationTransaction.SystemGeneratedIdentifier;

              break;
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  if (local.Work.Count < 5)
                  {
                    // ***  Next  ***
                  }
                  else
                  {
                    // Duplicate exception has occurred 5 times
                    ExitState = "FN0000_OBLIG_TRANS_AE_RB";
                  }

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "FN0000_OBLIG_TRANS_PV_RB";

                  break;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }
        }
        else if (entities.LegalActionPerson.Populated)
        {
          try
          {
            CreateObligationTransaction2();
            export.ObligationTransaction.SystemGeneratedIdentifier =
              entities.ObligationTransaction.SystemGeneratedIdentifier;

            break;
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                if (local.Work.Count < 5)
                {
                  // ***  Next  ***
                }
                else
                {
                  // Duplicate exception has occurred 5 times
                  ExitState = "FN0000_OBLIG_TRANS_AE_RB";
                }

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_OBLIG_TRANS_PV_RB";

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
          try
          {
            CreateObligationTransaction4();
            export.ObligationTransaction.SystemGeneratedIdentifier =
              entities.ObligationTransaction.SystemGeneratedIdentifier;

            break;
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                if (local.Work.Count < 5)
                {
                  // ***  Next  ***
                }
                else
                {
                  // Duplicate exception has occurred 5 times
                  ExitState = "FN0000_OBLIG_TRANS_AE_RB";
                }

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_OBLIG_TRANS_PV_RB";

                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }

      // =================================================
      // 2/11/1999 - Bud Adams  -  Replaced the cab used to determine
      //   the supported person program type.
      // =================================================
      // 3/5/1999 - Bud Adams  -  If this is being executed by an on-
      //   line debt screen, then there are many program codes that
      //   we only want to see NA on the screen.
      // =================================================
      // 3/31/1999 - B Adams  -  The debt screens will no longer try
      //   to display the program code for the supported person.  It
      //   has no value to this function and only leads to confusion.
      // 7/20/99 - B Adams  -  Removed read of obligation_
      //   transaction; was here to support persistend import view
      //   of a CAB that is no longer needed.
      // =================================================
    }
    else
    {
      // ***---  IF import_supported cse_person number = SPACES
      if (AsChar(import.ObligationType.Classification) == AsChar
        (import.HcOtCVoluntaryClassif.Classification))
      {
      }
      else if (AsChar(import.ObligationType.Classification) == AsChar
        (import.HcOtCRecoverClassific.Classification) || AsChar
        (import.ObligationType.Classification) == AsChar
        (import.HcOtCFeesClassificati.Classification))
      {
        if (import.LegalAction.Identifier != 0)
        {
          if (!ReadLegalActionPerson2())
          {
            ExitState = "CO0000_LEGAL_ACTION_PERSON_NF_RB";

            return;
          }
        }
      }
      else if (!ReadLegalActionPerson3())
      {
        ExitState = "CO0000_LEGAL_ACTION_PERSON_NF_RB";

        return;
      }

      // *****
      // : Obligation has no Supported Person.  Obligation Transaction created
      //   with no ASSOCIATE to supported CSE_Person_Account.
      // In this event the Obligation Transaction will have a relationship drawn
      // to the Legal Action Person that pertains to the obligor.  Cause the
      // relationship to be drawn in the event that the Legal Action Person is
      // populated as in the above logic.
      // *****
      for(local.Work.Count = 1; local.Work.Count <= 5; ++local.Work.Count)
      {
        if (AsChar(import.ObligationType.Classification) == AsChar
          (import.HcOtCRecoverClassific.Classification) || AsChar
          (import.ObligationType.Classification) == AsChar
          (import.HcOtCFeesClassificati.Classification))
        {
          if (import.LegalAction.Identifier == 0)
          {
            try
            {
              CreateObligationTransaction7();
              export.ObligationTransaction.SystemGeneratedIdentifier =
                entities.ObligationTransaction.SystemGeneratedIdentifier;

              goto Test;
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  if (local.Work.Count < 5)
                  {
                    // ***  Next  ***
                  }
                  else
                  {
                    // Duplicate exception has occurred 5 times
                    ExitState = "FN0000_OBLIG_TRANS_AE_RB";
                  }

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "FN0000_OBLIG_TRANS_PV_RB";

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
            try
            {
              CreateObligationTransaction6();
              export.ObligationTransaction.SystemGeneratedIdentifier =
                entities.ObligationTransaction.SystemGeneratedIdentifier;

              goto Test;
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  if (local.Work.Count < 5)
                  {
                    // ***  Next  ***
                  }
                  else
                  {
                    // Duplicate exception has occurred 5 times
                    ExitState = "FN0000_OBLIG_TRANS_AE_RB";
                  }

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "FN0000_OBLIG_TRANS_PV_RB";

                  break;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }

          // ***---  end of if this is FEE or RECOVERY
        }
        else if (entities.LegalActionPerson.Populated)
        {
          try
          {
            CreateObligationTransaction6();
            export.ObligationTransaction.SystemGeneratedIdentifier =
              entities.ObligationTransaction.SystemGeneratedIdentifier;

            break;
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                if (local.Work.Count < 5)
                {
                  // ***  Next  ***
                }
                else
                {
                  // Duplicate exception has occurred 5 times
                  ExitState = "FN0000_OBLIG_TRANS_AE_RB";
                }

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_OBLIG_TRANS_PV_RB";

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
          try
          {
            CreateObligationTransaction7();
            export.ObligationTransaction.SystemGeneratedIdentifier =
              entities.ObligationTransaction.SystemGeneratedIdentifier;

            break;
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                if (local.Work.Count < 5)
                {
                  // ***  Next  ***
                }
                else
                {
                  // Duplicate exception has occurred 5 times
                  ExitState = "FN0000_OBLIG_TRANS_AE_RB";
                }

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_OBLIG_TRANS_PV_RB";

                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }
    }

Test:

    if (AsChar(import.Hardcoded.DebtType) == AsChar
      (import.HcOtrnDtAccrual.DebtType))
    {
      // : Obligation is Accruing - create Accrual Instructions.
      try
      {
        CreateAccrualInstructions();

        // ** OK **
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_ACCRUAL_INSTRCTIONS_AE_RB";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_ACCRUAL_INSTR_PV_RB";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      // *** If the Obligation-transaction is set against a History Obligation, 
      // suspend the   accrual
      if (AsChar(import.HistoryIndicator.Flag) == 'Y')
      {
        // =================================================
        // 2/26/1999 - b adams  -  The Resume date was off by one day
        //   If it were really set to be equal to discontinue date, then the
        //   possibility exists that it COULD accrue.  And then those
        //   history obligations would cause problems.
        // =================================================
        try
        {
          CreateAccrualSuspension();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_ACCRUAL_SUSPENSION_NU_RB";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_ACCRUAL_SUSPENSION_PV_RB";

              break;
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
      local.Max.Date = import.DebtDetail.CoveredPrdEndDt;

      if (Equal(local.Max.Date, local.Initialized.Date))
      {
        local.Max.Date = import.Max.Date;
      }

      // : Obligation is Non-Accruing - create Debt Detail.
      if (import.ObligationType.SystemGeneratedIdentifier == import
        .HcOt718BUraJudgement.SystemGeneratedIdentifier)
      {
        local.PreconvPgmType.PreconversionProgramCode = "AF";
      }
      else
      {
        // : Jan 2002, M. Brown, WO# 020144, Pre-conversion code is now 
        // updateable.
        local.PreconvPgmType.PreconversionProgramCode =
          import.DebtDetail.PreconversionProgramCode ?? "";
      }

      try
      {
        CreateDebtDetail();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0210_DEBT_DETAIL_AE_RB";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0218_DEBT_DETAIL_PV_RB";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      for(local.RetryLoop.Count = 1; local.RetryLoop.Count <= 5; ++
        local.RetryLoop.Count)
      {
        // ================================================
        // Changed this Create.  The WHEN PV VIOLATION section had
        // been deleted and the note: "Permitted value violation will
        // cause an ABORT".
        // ================================================
        try
        {
          CreateDebtDetailStatusHistory();

          return;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              if (local.RetryLoop.Count < 5)
              {
              }
              else
              {
                ExitState = "DEBT_DETAIL_STATUS_HISTORY_AE_RB";
              }

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0227_DEBT_DETL_STAT_HIST_PV_RB";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }
    }
  }

  private int UseGenerate9DigitRandomNumber()
  {
    var useImport = new Generate9DigitRandomNumber.Import();
    var useExport = new Generate9DigitRandomNumber.Export();

    Call(Generate9DigitRandomNumber.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute9DigitRandomNumber;
  }

  private void CreateAccrualInstructions()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);

    var otrType = entities.ObligationTransaction.Type1;
    var otyId = entities.ObligationTransaction.OtyType;
    var obgGeneratedId = entities.ObligationTransaction.ObgGeneratedId;
    var cspNumber = entities.ObligationTransaction.CspNumber;
    var cpaType = entities.ObligationTransaction.CpaType;
    var otrGeneratedId =
      entities.ObligationTransaction.SystemGeneratedIdentifier;
    var asOfDt = import.ObligationPaymentSchedule.StartDt;
    var discontinueDt = local.AccrualInstructions.DiscontinueDt;

    CheckValid<AccrualInstructions>("OtrType", otrType);
    CheckValid<AccrualInstructions>("CpaType", cpaType);
    entities.AccrualInstructions.Populated = false;
    Update("CreateAccrualInstructions",
      (db, command) =>
      {
        db.SetString(command, "otrType", otrType);
        db.SetInt32(command, "otyId", otyId);
        db.SetInt32(command, "obgGeneratedId", obgGeneratedId);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "cpaType", cpaType);
        db.SetInt32(command, "otrGeneratedId", otrGeneratedId);
        db.SetDate(command, "asOfDt", asOfDt);
        db.SetNullableDate(command, "discontinueDt", discontinueDt);
        db.SetNullableDate(command, "lastAccrualDt", null);
      });

    entities.AccrualInstructions.OtrType = otrType;
    entities.AccrualInstructions.OtyId = otyId;
    entities.AccrualInstructions.ObgGeneratedId = obgGeneratedId;
    entities.AccrualInstructions.CspNumber = cspNumber;
    entities.AccrualInstructions.CpaType = cpaType;
    entities.AccrualInstructions.OtrGeneratedId = otrGeneratedId;
    entities.AccrualInstructions.AsOfDt = asOfDt;
    entities.AccrualInstructions.DiscontinueDt = discontinueDt;
    entities.AccrualInstructions.LastAccrualDt = null;
    entities.AccrualInstructions.Populated = true;
  }

  private void CreateAccrualSuspension()
  {
    System.Diagnostics.Debug.Assert(entities.AccrualInstructions.Populated);

    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var suspendDt = import.ObligationPaymentSchedule.StartDt;
    var resumeDt = AddDays(import.AccrualInstructions.DiscontinueDt, 1);
    var reductionPercentage = 100M;
    var createdBy = global.UserId;
    var createdTmst = import.Current.Timestamp;
    var otrType = entities.AccrualInstructions.OtrType;
    var otyId = entities.AccrualInstructions.OtyId;
    var obgId = entities.AccrualInstructions.ObgGeneratedId;
    var cspNumber = entities.AccrualInstructions.CspNumber;
    var cpaType = entities.AccrualInstructions.CpaType;
    var otrId = entities.AccrualInstructions.OtrGeneratedId;
    var reductionAmount = 0M;
    var reasonTxt =
      "The Accrual is Suspended since the related supported person is in a History Obligation";
      

    CheckValid<AccrualSuspension>("OtrType", otrType);
    CheckValid<AccrualSuspension>("CpaType", cpaType);
    entities.AccrualSuspension.Populated = false;
    Update("CreateAccrualSuspension",
      (db, command) =>
      {
        db.SetInt32(command, "frqSuspId", systemGeneratedIdentifier);
        db.SetDate(command, "suspendDt", suspendDt);
        db.SetNullableDate(command, "resumeDt", resumeDt);
        db.SetNullableDecimal(command, "redPct", reductionPercentage);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetString(command, "otrType", otrType);
        db.SetInt32(command, "otyId", otyId);
        db.SetInt32(command, "obgId", obgId);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "cpaType", cpaType);
        db.SetInt32(command, "otrId", otrId);
        db.SetNullableDecimal(command, "reductionAmount", reductionAmount);
        db.SetNullableString(command, "frqSuspRsnTxt", reasonTxt);
      });

    entities.AccrualSuspension.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.AccrualSuspension.SuspendDt = suspendDt;
    entities.AccrualSuspension.ResumeDt = resumeDt;
    entities.AccrualSuspension.ReductionPercentage = reductionPercentage;
    entities.AccrualSuspension.CreatedBy = createdBy;
    entities.AccrualSuspension.CreatedTmst = createdTmst;
    entities.AccrualSuspension.OtrType = otrType;
    entities.AccrualSuspension.OtyId = otyId;
    entities.AccrualSuspension.ObgId = obgId;
    entities.AccrualSuspension.CspNumber = cspNumber;
    entities.AccrualSuspension.CpaType = cpaType;
    entities.AccrualSuspension.OtrId = otrId;
    entities.AccrualSuspension.ReductionAmount = reductionAmount;
    entities.AccrualSuspension.ReasonTxt = reasonTxt;
    entities.AccrualSuspension.Populated = true;
  }

  private void CreateCsePersonAccount()
  {
    var cspNumber = entities.SupportedCsePerson.Number;
    var type1 = import.HcCpaSupportedPerson.Type1;
    var createdBy = global.UserId;
    var createdTmst = import.Current.Timestamp;

    CheckValid<CsePersonAccount>("Type1", type1);
    entities.SupportedCsePersonAccount.Populated = false;
    Update("CreateCsePersonAccount",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "type", type1);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetNullableDate(command, "recompBalFromDt", default(DateTime));
        db.SetNullableDecimal(command, "stdTotGiftColl", 0M);
        db.SetNullableString(command, "triggerType", "");
      });

    entities.SupportedCsePersonAccount.CspNumber = cspNumber;
    entities.SupportedCsePersonAccount.Type1 = type1;
    entities.SupportedCsePersonAccount.CreatedBy = createdBy;
    entities.SupportedCsePersonAccount.CreatedTmst = createdTmst;
    entities.SupportedCsePersonAccount.Populated = true;
  }

  private void CreateDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);

    var obgGeneratedId = entities.ObligationTransaction.ObgGeneratedId;
    var cspNumber = entities.ObligationTransaction.CspNumber;
    var cpaType = entities.ObligationTransaction.CpaType;
    var otrGeneratedId =
      entities.ObligationTransaction.SystemGeneratedIdentifier;
    var otyType = entities.ObligationTransaction.OtyType;
    var otrType = entities.ObligationTransaction.Type1;
    var dueDt = import.DebtDetail.DueDt;
    var balanceDueAmt = import.DebtDetail.BalanceDueAmt;
    var interestBalanceDueAmt =
      import.DebtDetail.InterestBalanceDueAmt.GetValueOrDefault();
    var coveredPrdStartDt = import.DebtDetail.CoveredPrdStartDt;
    var coveredPrdEndDt = local.Max.Date;
    var preconversionProgramCode =
      local.PreconvPgmType.PreconversionProgramCode ?? "";
    var createdTmst = import.Current.Timestamp;
    var createdBy = global.UserId;

    CheckValid<DebtDetail>("CpaType", cpaType);
    CheckValid<DebtDetail>("OtrType", otrType);
    entities.DebtDetail.Populated = false;
    Update("CreateDebtDetail",
      (db, command) =>
      {
        db.SetInt32(command, "obgGeneratedId", obgGeneratedId);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "cpaType", cpaType);
        db.SetInt32(command, "otrGeneratedId", otrGeneratedId);
        db.SetInt32(command, "otyType", otyType);
        db.SetString(command, "otrType", otrType);
        db.SetDate(command, "dueDt", dueDt);
        db.SetDecimal(command, "balDueAmt", balanceDueAmt);
        db.SetNullableDecimal(command, "intBalDueAmt", interestBalanceDueAmt);
        db.SetNullableDate(command, "adcDt", null);
        db.SetNullableDate(command, "retiredDt", null);
        db.SetNullableDate(command, "cvrdPrdStartDt", coveredPrdStartDt);
        db.SetNullableDate(command, "cvdPrdEndDt", coveredPrdEndDt);
        db.
          SetNullableString(command, "precnvrsnPgmCd", preconversionProgramCode);
          
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetString(command, "createdBy", createdBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetNullableString(command, "lastUpdatedBy", "");
      });

    entities.DebtDetail.ObgGeneratedId = obgGeneratedId;
    entities.DebtDetail.CspNumber = cspNumber;
    entities.DebtDetail.CpaType = cpaType;
    entities.DebtDetail.OtrGeneratedId = otrGeneratedId;
    entities.DebtDetail.OtyType = otyType;
    entities.DebtDetail.OtrType = otrType;
    entities.DebtDetail.DueDt = dueDt;
    entities.DebtDetail.BalanceDueAmt = balanceDueAmt;
    entities.DebtDetail.InterestBalanceDueAmt = interestBalanceDueAmt;
    entities.DebtDetail.AdcDt = null;
    entities.DebtDetail.RetiredDt = null;
    entities.DebtDetail.CoveredPrdStartDt = coveredPrdStartDt;
    entities.DebtDetail.CoveredPrdEndDt = coveredPrdEndDt;
    entities.DebtDetail.PreconversionProgramCode = preconversionProgramCode;
    entities.DebtDetail.CreatedTmst = createdTmst;
    entities.DebtDetail.CreatedBy = createdBy;
    entities.DebtDetail.Populated = true;
  }

  private void CreateDebtDetailStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.DebtDetail.Populated);

    #region MANUAL CODE by ADVANCED, REVIEWED by ... at ...
    // Manual fix for github issue #84
    // Original code begin    
    // var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    // Original code end

    // Manually changed code begin
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber() % 1000;
    // Manually changed code end
    #endregion
    var effectiveDt = import.DebtDetail.DueDt;
    var discontinueDt = import.Max.Date;
    var createdBy = global.UserId;
    var createdTmst = import.Current.Timestamp;
    var otrType = entities.DebtDetail.OtrType;
    var otrId = entities.DebtDetail.OtrGeneratedId;
    var cpaType = entities.DebtDetail.CpaType;
    var cspNumber = entities.DebtDetail.CspNumber;
    var obgId = entities.DebtDetail.ObgGeneratedId;
    var code = import.HcDdshActiveStatus.Code;
    var otyType = entities.DebtDetail.OtyType;

    CheckValid<DebtDetailStatusHistory>("OtrType", otrType);
    CheckValid<DebtDetailStatusHistory>("CpaType", cpaType);
    entities.DebtDetailStatusHistory.Populated = false;
    Update("CreateDebtDetailStatusHistory",
      (db, command) =>
      {
        db.SetInt32(command, "obTrnStatHstId", systemGeneratedIdentifier);
        db.SetDate(command, "effectiveDt", effectiveDt);
        db.SetNullableDate(command, "discontinueDt", discontinueDt);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetString(command, "otrType", otrType);
        db.SetInt32(command, "otrId", otrId);
        db.SetString(command, "cpaType", cpaType);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "obgId", obgId);
        db.SetString(command, "obTrnStCd", code);
        db.SetInt32(command, "otyType", otyType);
        db.SetNullableString(command, "rsnTxt", "");
      });

    entities.DebtDetailStatusHistory.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.DebtDetailStatusHistory.EffectiveDt = effectiveDt;
    entities.DebtDetailStatusHistory.DiscontinueDt = discontinueDt;
    entities.DebtDetailStatusHistory.CreatedBy = createdBy;
    entities.DebtDetailStatusHistory.CreatedTmst = createdTmst;
    entities.DebtDetailStatusHistory.OtrType = otrType;
    entities.DebtDetailStatusHistory.OtrId = otrId;
    entities.DebtDetailStatusHistory.CpaType = cpaType;
    entities.DebtDetailStatusHistory.CspNumber = cspNumber;
    entities.DebtDetailStatusHistory.ObgId = obgId;
    entities.DebtDetailStatusHistory.Code = code;
    entities.DebtDetailStatusHistory.OtyType = otyType;
    entities.DebtDetailStatusHistory.ReasonTxt = "";
    entities.DebtDetailStatusHistory.Populated = true;
  }

  private void CreateObligationTransaction1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    System.Diagnostics.Debug.
      Assert(entities.SupportedCsePersonAccount.Populated);

    var obgGeneratedId = entities.Obligation.SystemGeneratedIdentifier;
    var cspNumber = entities.Obligation.CspNumber;
    var cpaType = entities.Obligation.CpaType;
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var type1 = import.HcOtrnTDebt.Type1;
    var amount = import.ObligationTransaction.Amount;
    var createdBy = global.UserId;
    var createdTmst = import.Current.Timestamp;
    var debtType = import.HcOtrnDtAccrual.DebtType;
    var cspSupNumber = entities.SupportedCsePersonAccount.CspNumber;
    var cpaSupType = entities.SupportedCsePersonAccount.Type1;
    var otyType = entities.Obligation.DtyGeneratedId;
    var lapId = entities.LegalActionPerson.Identifier;

    CheckValid<ObligationTransaction>("CpaType", cpaType);
    CheckValid<ObligationTransaction>("Type1", type1);
    CheckValid<ObligationTransaction>("DebtType", debtType);
    CheckValid<ObligationTransaction>("CpaSupType", cpaSupType);
    entities.ObligationTransaction.Populated = false;
    Update("CreateObligationTransaction1",
      (db, command) =>
      {
        db.SetInt32(command, "obgGeneratedId", obgGeneratedId);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "cpaType", cpaType);
        db.SetInt32(command, "obTrnId", systemGeneratedIdentifier);
        db.SetString(command, "obTrnTyp", type1);
        db.SetDecimal(command, "obTrnAmt", amount);
        db.SetString(
          command, "debtAdjInd", GetImplicitValue<ObligationTransaction,
          string>("DebtAdjustmentInd"));
        db.SetString(
          command, "debtAdjTyp", GetImplicitValue<ObligationTransaction,
          string>("DebtAdjustmentType"));
        db.SetDate(command, "debAdjDt", default(DateTime));
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetString(command, "debtTyp", debtType);
        db.SetNullableInt32(command, "volPctAmt", 0);
        db.SetNullableInt32(command, "zdelPrecnvRcptN", 0);
        db.SetNullableInt32(command, "zdelPrecnvrsnIsn", 0);
        db.SetNullableString(command, "cspSupNumber", cspSupNumber);
        db.SetNullableString(command, "cpaSupType", cpaSupType);
        db.SetInt32(command, "otyType", otyType);
        db.SetString(command, "daCaProcReqInd", "");
        db.SetString(command, "rsnCd", "");
        db.SetNullableInt32(command, "lapId", lapId);
        db.SetNullableDate(command, "newDebtProcDt", null);
      });

    entities.ObligationTransaction.ObgGeneratedId = obgGeneratedId;
    entities.ObligationTransaction.CspNumber = cspNumber;
    entities.ObligationTransaction.CpaType = cpaType;
    entities.ObligationTransaction.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.ObligationTransaction.Type1 = type1;
    entities.ObligationTransaction.Amount = amount;
    entities.ObligationTransaction.CreatedBy = createdBy;
    entities.ObligationTransaction.CreatedTmst = createdTmst;
    entities.ObligationTransaction.DebtType = debtType;
    entities.ObligationTransaction.VoluntaryPercentageAmount = 0;
    entities.ObligationTransaction.CspSupNumber = cspSupNumber;
    entities.ObligationTransaction.CpaSupType = cpaSupType;
    entities.ObligationTransaction.OtyType = otyType;
    entities.ObligationTransaction.LapId = lapId;
    entities.ObligationTransaction.NewDebtProcessDate = null;
    entities.ObligationTransaction.Populated = true;
  }

  private void CreateObligationTransaction2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    System.Diagnostics.Debug.
      Assert(entities.SupportedCsePersonAccount.Populated);

    var obgGeneratedId = entities.Obligation.SystemGeneratedIdentifier;
    var cspNumber = entities.Obligation.CspNumber;
    var cpaType = entities.Obligation.CpaType;
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var type1 = import.HcOtrnTDebt.Type1;
    var amount = import.ObligationTransaction.Amount;
    var createdBy = global.UserId;
    var createdTmst = import.Current.Timestamp;
    var debtType = import.HcOtrnDtDebtDetail.DebtType;
    var cspSupNumber = entities.SupportedCsePersonAccount.CspNumber;
    var cpaSupType = entities.SupportedCsePersonAccount.Type1;
    var otyType = entities.Obligation.DtyGeneratedId;
    var lapId = entities.LegalActionPerson.Identifier;

    CheckValid<ObligationTransaction>("CpaType", cpaType);
    CheckValid<ObligationTransaction>("Type1", type1);
    CheckValid<ObligationTransaction>("DebtType", debtType);
    CheckValid<ObligationTransaction>("CpaSupType", cpaSupType);
    entities.ObligationTransaction.Populated = false;
    Update("CreateObligationTransaction2",
      (db, command) =>
      {
        db.SetInt32(command, "obgGeneratedId", obgGeneratedId);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "cpaType", cpaType);
        db.SetInt32(command, "obTrnId", systemGeneratedIdentifier);
        db.SetString(command, "obTrnTyp", type1);
        db.SetDecimal(command, "obTrnAmt", amount);
        db.SetString(
          command, "debtAdjInd", GetImplicitValue<ObligationTransaction,
          string>("DebtAdjustmentInd"));
        db.SetString(
          command, "debtAdjTyp", GetImplicitValue<ObligationTransaction,
          string>("DebtAdjustmentType"));
        db.SetDate(command, "debAdjDt", default(DateTime));
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetString(command, "debtTyp", debtType);
        db.SetNullableInt32(command, "volPctAmt", 0);
        db.SetNullableInt32(command, "zdelPrecnvRcptN", 0);
        db.SetNullableInt32(command, "zdelPrecnvrsnIsn", 0);
        db.SetNullableString(command, "cspSupNumber", cspSupNumber);
        db.SetNullableString(command, "cpaSupType", cpaSupType);
        db.SetInt32(command, "otyType", otyType);
        db.SetString(command, "daCaProcReqInd", "");
        db.SetString(command, "rsnCd", "");
        db.SetNullableInt32(command, "lapId", lapId);
        db.SetNullableDate(command, "newDebtProcDt", null);
      });

    entities.ObligationTransaction.ObgGeneratedId = obgGeneratedId;
    entities.ObligationTransaction.CspNumber = cspNumber;
    entities.ObligationTransaction.CpaType = cpaType;
    entities.ObligationTransaction.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.ObligationTransaction.Type1 = type1;
    entities.ObligationTransaction.Amount = amount;
    entities.ObligationTransaction.CreatedBy = createdBy;
    entities.ObligationTransaction.CreatedTmst = createdTmst;
    entities.ObligationTransaction.DebtType = debtType;
    entities.ObligationTransaction.VoluntaryPercentageAmount = 0;
    entities.ObligationTransaction.CspSupNumber = cspSupNumber;
    entities.ObligationTransaction.CpaSupType = cpaSupType;
    entities.ObligationTransaction.OtyType = otyType;
    entities.ObligationTransaction.LapId = lapId;
    entities.ObligationTransaction.NewDebtProcessDate = null;
    entities.ObligationTransaction.Populated = true;
  }

  private void CreateObligationTransaction3()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    System.Diagnostics.Debug.
      Assert(entities.SupportedCsePersonAccount.Populated);

    var obgGeneratedId = entities.Obligation.SystemGeneratedIdentifier;
    var cspNumber = entities.Obligation.CspNumber;
    var cpaType = entities.Obligation.CpaType;
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var type1 = import.HcOtrnTDebt.Type1;
    var amount = import.ObligationTransaction.Amount;
    var createdBy = global.UserId;
    var createdTmst = import.Current.Timestamp;
    var debtType = import.HcOtrnDtVoluntary.DebtType;
    var voluntaryPercentageAmount = import.NumberOfSupportedPrsns.Percentage;
    var cspSupNumber = entities.SupportedCsePersonAccount.CspNumber;
    var cpaSupType = entities.SupportedCsePersonAccount.Type1;
    var otyType = entities.Obligation.DtyGeneratedId;
    var lapId = entities.LegalActionPerson.Identifier;
    var newDebtProcessDate = import.Current.Date;

    CheckValid<ObligationTransaction>("CpaType", cpaType);
    CheckValid<ObligationTransaction>("Type1", type1);
    CheckValid<ObligationTransaction>("DebtType", debtType);
    CheckValid<ObligationTransaction>("CpaSupType", cpaSupType);
    entities.ObligationTransaction.Populated = false;
    Update("CreateObligationTransaction3",
      (db, command) =>
      {
        db.SetInt32(command, "obgGeneratedId", obgGeneratedId);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "cpaType", cpaType);
        db.SetInt32(command, "obTrnId", systemGeneratedIdentifier);
        db.SetString(command, "obTrnTyp", type1);
        db.SetDecimal(command, "obTrnAmt", amount);
        db.SetString(
          command, "debtAdjInd", GetImplicitValue<ObligationTransaction,
          string>("DebtAdjustmentInd"));
        db.SetString(
          command, "debtAdjTyp", GetImplicitValue<ObligationTransaction,
          string>("DebtAdjustmentType"));
        db.SetDate(command, "debAdjDt", default(DateTime));
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetString(command, "debtTyp", debtType);
        db.SetNullableInt32(command, "volPctAmt", voluntaryPercentageAmount);
        db.SetNullableInt32(command, "zdelPrecnvRcptN", 0);
        db.SetNullableInt32(command, "zdelPrecnvrsnIsn", 0);
        db.SetNullableString(command, "cspSupNumber", cspSupNumber);
        db.SetNullableString(command, "cpaSupType", cpaSupType);
        db.SetInt32(command, "otyType", otyType);
        db.SetString(command, "daCaProcReqInd", "");
        db.SetString(command, "rsnCd", "");
        db.SetNullableInt32(command, "lapId", lapId);
        db.SetNullableDate(command, "newDebtProcDt", newDebtProcessDate);
      });

    entities.ObligationTransaction.ObgGeneratedId = obgGeneratedId;
    entities.ObligationTransaction.CspNumber = cspNumber;
    entities.ObligationTransaction.CpaType = cpaType;
    entities.ObligationTransaction.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.ObligationTransaction.Type1 = type1;
    entities.ObligationTransaction.Amount = amount;
    entities.ObligationTransaction.CreatedBy = createdBy;
    entities.ObligationTransaction.CreatedTmst = createdTmst;
    entities.ObligationTransaction.DebtType = debtType;
    entities.ObligationTransaction.VoluntaryPercentageAmount =
      voluntaryPercentageAmount;
    entities.ObligationTransaction.CspSupNumber = cspSupNumber;
    entities.ObligationTransaction.CpaSupType = cpaSupType;
    entities.ObligationTransaction.OtyType = otyType;
    entities.ObligationTransaction.LapId = lapId;
    entities.ObligationTransaction.NewDebtProcessDate = newDebtProcessDate;
    entities.ObligationTransaction.Populated = true;
  }

  private void CreateObligationTransaction4()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    System.Diagnostics.Debug.
      Assert(entities.SupportedCsePersonAccount.Populated);

    var obgGeneratedId = entities.Obligation.SystemGeneratedIdentifier;
    var cspNumber = entities.Obligation.CspNumber;
    var cpaType = entities.Obligation.CpaType;
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var type1 = import.HcOtrnTDebt.Type1;
    var amount = import.ObligationTransaction.Amount;
    var createdBy = global.UserId;
    var createdTmst = import.Current.Timestamp;
    var debtType = import.HcOtrnDtDebtDetail.DebtType;
    var cspSupNumber = entities.SupportedCsePersonAccount.CspNumber;
    var cpaSupType = entities.SupportedCsePersonAccount.Type1;
    var otyType = entities.Obligation.DtyGeneratedId;

    CheckValid<ObligationTransaction>("CpaType", cpaType);
    CheckValid<ObligationTransaction>("Type1", type1);
    CheckValid<ObligationTransaction>("DebtType", debtType);
    CheckValid<ObligationTransaction>("CpaSupType", cpaSupType);
    entities.ObligationTransaction.Populated = false;
    Update("CreateObligationTransaction4",
      (db, command) =>
      {
        db.SetInt32(command, "obgGeneratedId", obgGeneratedId);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "cpaType", cpaType);
        db.SetInt32(command, "obTrnId", systemGeneratedIdentifier);
        db.SetString(command, "obTrnTyp", type1);
        db.SetDecimal(command, "obTrnAmt", amount);
        db.SetString(
          command, "debtAdjInd", GetImplicitValue<ObligationTransaction,
          string>("DebtAdjustmentInd"));
        db.SetString(
          command, "debtAdjTyp", GetImplicitValue<ObligationTransaction,
          string>("DebtAdjustmentType"));
        db.SetDate(command, "debAdjDt", default(DateTime));
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetString(command, "debtTyp", debtType);
        db.SetNullableInt32(command, "volPctAmt", 0);
        db.SetNullableInt32(command, "zdelPrecnvRcptN", 0);
        db.SetNullableInt32(command, "zdelPrecnvrsnIsn", 0);
        db.SetNullableString(command, "cspSupNumber", cspSupNumber);
        db.SetNullableString(command, "cpaSupType", cpaSupType);
        db.SetInt32(command, "otyType", otyType);
        db.SetString(command, "daCaProcReqInd", "");
        db.SetString(command, "rsnCd", "");
        db.SetNullableDate(command, "newDebtProcDt", null);
      });

    entities.ObligationTransaction.ObgGeneratedId = obgGeneratedId;
    entities.ObligationTransaction.CspNumber = cspNumber;
    entities.ObligationTransaction.CpaType = cpaType;
    entities.ObligationTransaction.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.ObligationTransaction.Type1 = type1;
    entities.ObligationTransaction.Amount = amount;
    entities.ObligationTransaction.CreatedBy = createdBy;
    entities.ObligationTransaction.CreatedTmst = createdTmst;
    entities.ObligationTransaction.DebtType = debtType;
    entities.ObligationTransaction.VoluntaryPercentageAmount = 0;
    entities.ObligationTransaction.CspSupNumber = cspSupNumber;
    entities.ObligationTransaction.CpaSupType = cpaSupType;
    entities.ObligationTransaction.OtyType = otyType;
    entities.ObligationTransaction.LapId = null;
    entities.ObligationTransaction.NewDebtProcessDate = null;
    entities.ObligationTransaction.Populated = true;
  }

  private void CreateObligationTransaction5()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    System.Diagnostics.Debug.
      Assert(entities.SupportedCsePersonAccount.Populated);

    var obgGeneratedId = entities.Obligation.SystemGeneratedIdentifier;
    var cspNumber = entities.Obligation.CspNumber;
    var cpaType = entities.Obligation.CpaType;
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var type1 = import.HcOtrnTDebt.Type1;
    var amount = import.ObligationTransaction.Amount;
    var createdBy = global.UserId;
    var createdTmst = import.Current.Timestamp;
    var debtType = import.HcOtrnDtVoluntary.DebtType;
    var voluntaryPercentageAmount = import.NumberOfSupportedPrsns.Percentage;
    var cspSupNumber = entities.SupportedCsePersonAccount.CspNumber;
    var cpaSupType = entities.SupportedCsePersonAccount.Type1;
    var otyType = entities.Obligation.DtyGeneratedId;
    var newDebtProcessDate = import.Current.Date;

    CheckValid<ObligationTransaction>("CpaType", cpaType);
    CheckValid<ObligationTransaction>("Type1", type1);
    CheckValid<ObligationTransaction>("DebtType", debtType);
    CheckValid<ObligationTransaction>("CpaSupType", cpaSupType);
    entities.ObligationTransaction.Populated = false;
    Update("CreateObligationTransaction5",
      (db, command) =>
      {
        db.SetInt32(command, "obgGeneratedId", obgGeneratedId);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "cpaType", cpaType);
        db.SetInt32(command, "obTrnId", systemGeneratedIdentifier);
        db.SetString(command, "obTrnTyp", type1);
        db.SetDecimal(command, "obTrnAmt", amount);
        db.SetString(
          command, "debtAdjInd", GetImplicitValue<ObligationTransaction,
          string>("DebtAdjustmentInd"));
        db.SetString(
          command, "debtAdjTyp", GetImplicitValue<ObligationTransaction,
          string>("DebtAdjustmentType"));
        db.SetDate(command, "debAdjDt", default(DateTime));
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetString(command, "debtTyp", debtType);
        db.SetNullableInt32(command, "volPctAmt", voluntaryPercentageAmount);
        db.SetNullableInt32(command, "zdelPrecnvRcptN", 0);
        db.SetNullableInt32(command, "zdelPrecnvrsnIsn", 0);
        db.SetNullableString(command, "cspSupNumber", cspSupNumber);
        db.SetNullableString(command, "cpaSupType", cpaSupType);
        db.SetInt32(command, "otyType", otyType);
        db.SetString(command, "daCaProcReqInd", "");
        db.SetString(command, "rsnCd", "");
        db.SetNullableDate(command, "newDebtProcDt", newDebtProcessDate);
      });

    entities.ObligationTransaction.ObgGeneratedId = obgGeneratedId;
    entities.ObligationTransaction.CspNumber = cspNumber;
    entities.ObligationTransaction.CpaType = cpaType;
    entities.ObligationTransaction.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.ObligationTransaction.Type1 = type1;
    entities.ObligationTransaction.Amount = amount;
    entities.ObligationTransaction.CreatedBy = createdBy;
    entities.ObligationTransaction.CreatedTmst = createdTmst;
    entities.ObligationTransaction.DebtType = debtType;
    entities.ObligationTransaction.VoluntaryPercentageAmount =
      voluntaryPercentageAmount;
    entities.ObligationTransaction.CspSupNumber = cspSupNumber;
    entities.ObligationTransaction.CpaSupType = cpaSupType;
    entities.ObligationTransaction.OtyType = otyType;
    entities.ObligationTransaction.LapId = null;
    entities.ObligationTransaction.NewDebtProcessDate = newDebtProcessDate;
    entities.ObligationTransaction.Populated = true;
  }

  private void CreateObligationTransaction6()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);

    var obgGeneratedId = entities.Obligation.SystemGeneratedIdentifier;
    var cspNumber = entities.Obligation.CspNumber;
    var cpaType = entities.Obligation.CpaType;
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var type1 = import.HcOtrnTDebt.Type1;
    var amount = import.ObligationTransaction.Amount;
    var createdBy = global.UserId;
    var createdTmst = import.Current.Timestamp;
    var debtType = import.HcOtrnDtDebtDetail.DebtType;
    var cpaSupType =
      GetImplicitValue<ObligationTransaction, string>("CpaSupType");
    var otyType = entities.Obligation.DtyGeneratedId;
    var lapId = entities.LegalActionPerson.Identifier;

    CheckValid<ObligationTransaction>("CpaType", cpaType);
    CheckValid<ObligationTransaction>("Type1", type1);
    CheckValid<ObligationTransaction>("DebtType", debtType);
    CheckValid<ObligationTransaction>("CpaSupType", cpaSupType);
    entities.ObligationTransaction.Populated = false;
    Update("CreateObligationTransaction6",
      (db, command) =>
      {
        db.SetInt32(command, "obgGeneratedId", obgGeneratedId);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "cpaType", cpaType);
        db.SetInt32(command, "obTrnId", systemGeneratedIdentifier);
        db.SetString(command, "obTrnTyp", type1);
        db.SetDecimal(command, "obTrnAmt", amount);
        db.SetString(
          command, "debtAdjInd", GetImplicitValue<ObligationTransaction,
          string>("DebtAdjustmentInd"));
        db.SetString(
          command, "debtAdjTyp", GetImplicitValue<ObligationTransaction,
          string>("DebtAdjustmentType"));
        db.SetDate(command, "debAdjDt", default(DateTime));
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetString(command, "debtTyp", debtType);
        db.SetNullableInt32(command, "volPctAmt", 0);
        db.SetNullableInt32(command, "zdelPrecnvRcptN", 0);
        db.SetNullableInt32(command, "zdelPrecnvrsnIsn", 0);
        db.SetNullableString(command, "cpaSupType", cpaSupType);
        db.SetInt32(command, "otyType", otyType);
        db.SetString(command, "daCaProcReqInd", "");
        db.SetString(command, "rsnCd", "");
        db.SetNullableInt32(command, "lapId", lapId);
        db.SetNullableDate(command, "newDebtProcDt", null);
      });

    entities.ObligationTransaction.ObgGeneratedId = obgGeneratedId;
    entities.ObligationTransaction.CspNumber = cspNumber;
    entities.ObligationTransaction.CpaType = cpaType;
    entities.ObligationTransaction.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.ObligationTransaction.Type1 = type1;
    entities.ObligationTransaction.Amount = amount;
    entities.ObligationTransaction.CreatedBy = createdBy;
    entities.ObligationTransaction.CreatedTmst = createdTmst;
    entities.ObligationTransaction.DebtType = debtType;
    entities.ObligationTransaction.VoluntaryPercentageAmount = 0;
    entities.ObligationTransaction.CspSupNumber = null;
    entities.ObligationTransaction.CpaSupType = cpaSupType;
    entities.ObligationTransaction.OtyType = otyType;
    entities.ObligationTransaction.LapId = lapId;
    entities.ObligationTransaction.NewDebtProcessDate = null;
    entities.ObligationTransaction.Populated = true;
  }

  private void CreateObligationTransaction7()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);

    var obgGeneratedId = entities.Obligation.SystemGeneratedIdentifier;
    var cspNumber = entities.Obligation.CspNumber;
    var cpaType = entities.Obligation.CpaType;
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var type1 = import.HcOtrnTDebt.Type1;
    var amount = import.ObligationTransaction.Amount;
    var createdBy = global.UserId;
    var createdTmst = import.Current.Timestamp;
    var debtType = import.HcOtrnDtDebtDetail.DebtType;
    var cpaSupType =
      GetImplicitValue<ObligationTransaction, string>("CpaSupType");
    var otyType = entities.Obligation.DtyGeneratedId;

    CheckValid<ObligationTransaction>("CpaType", cpaType);
    CheckValid<ObligationTransaction>("Type1", type1);
    CheckValid<ObligationTransaction>("DebtType", debtType);
    CheckValid<ObligationTransaction>("CpaSupType", cpaSupType);
    entities.ObligationTransaction.Populated = false;
    Update("CreateObligationTransaction7",
      (db, command) =>
      {
        db.SetInt32(command, "obgGeneratedId", obgGeneratedId);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "cpaType", cpaType);
        db.SetInt32(command, "obTrnId", systemGeneratedIdentifier);
        db.SetString(command, "obTrnTyp", type1);
        db.SetDecimal(command, "obTrnAmt", amount);
        db.SetString(
          command, "debtAdjInd", GetImplicitValue<ObligationTransaction,
          string>("DebtAdjustmentInd"));
        db.SetString(
          command, "debtAdjTyp", GetImplicitValue<ObligationTransaction,
          string>("DebtAdjustmentType"));
        db.SetDate(command, "debAdjDt", default(DateTime));
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetString(command, "debtTyp", debtType);
        db.SetNullableInt32(command, "volPctAmt", 0);
        db.SetNullableInt32(command, "zdelPrecnvRcptN", 0);
        db.SetNullableInt32(command, "zdelPrecnvrsnIsn", 0);
        db.SetNullableString(command, "cpaSupType", cpaSupType);
        db.SetInt32(command, "otyType", otyType);
        db.SetString(command, "daCaProcReqInd", "");
        db.SetString(command, "rsnCd", "");
        db.SetNullableDate(command, "newDebtProcDt", null);
      });

    entities.ObligationTransaction.ObgGeneratedId = obgGeneratedId;
    entities.ObligationTransaction.CspNumber = cspNumber;
    entities.ObligationTransaction.CpaType = cpaType;
    entities.ObligationTransaction.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.ObligationTransaction.Type1 = type1;
    entities.ObligationTransaction.Amount = amount;
    entities.ObligationTransaction.CreatedBy = createdBy;
    entities.ObligationTransaction.CreatedTmst = createdTmst;
    entities.ObligationTransaction.DebtType = debtType;
    entities.ObligationTransaction.VoluntaryPercentageAmount = 0;
    entities.ObligationTransaction.CspSupNumber = null;
    entities.ObligationTransaction.CpaSupType = cpaSupType;
    entities.ObligationTransaction.OtyType = otyType;
    entities.ObligationTransaction.LapId = null;
    entities.ObligationTransaction.NewDebtProcessDate = null;
    entities.ObligationTransaction.Populated = true;
  }

  private bool ReadCsePerson()
  {
    entities.SupportedCsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Supported.Number);
      },
      (db, reader) =>
      {
        entities.SupportedCsePerson.Number = db.GetString(reader, 0);
        entities.SupportedCsePerson.Populated = true;
      });
  }

  private bool ReadCsePersonAccount()
  {
    entities.SupportedCsePersonAccount.Populated = false;

    return Read("ReadCsePersonAccount",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.SupportedCsePerson.Number);
        db.SetString(command, "type", import.HcCpaSupportedPerson.Type1);
      },
      (db, reader) =>
      {
        entities.SupportedCsePersonAccount.CspNumber = db.GetString(reader, 0);
        entities.SupportedCsePersonAccount.Type1 = db.GetString(reader, 1);
        entities.SupportedCsePersonAccount.CreatedBy = db.GetString(reader, 2);
        entities.SupportedCsePersonAccount.CreatedTmst =
          db.GetDateTime(reader, 3);
        entities.SupportedCsePersonAccount.Populated = true;
        CheckValid<CsePersonAccount>("Type1",
          entities.SupportedCsePersonAccount.Type1);
      });
  }

  private bool ReadLegalActionDetail()
  {
    entities.LegalActionDetail.Populated = false;

    return Read("ReadLegalActionDetail",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", import.LegalAction.Identifier);
        db.SetInt32(command, "laDetailNo", import.LegalActionDetail.Number);
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 2);
        entities.LegalActionDetail.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);
      });
  }

  private bool ReadLegalActionPerson1()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.LegalActionPerson.Populated = false;

    return Read("ReadLegalActionPerson1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladRNumber", entities.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.LegalActionDetail.LgaIdentifier);
        db.SetNullableString(
          command, "cspNumber", entities.SupportedCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 1);
        entities.LegalActionPerson.LgaIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.LegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 3);
        entities.LegalActionPerson.LadRNumber = db.GetNullableInt32(reader, 4);
        entities.LegalActionPerson.AccountType =
          db.GetNullableString(reader, 5);
        entities.LegalActionPerson.Populated = true;
      });
  }

  private bool ReadLegalActionPerson2()
  {
    entities.LegalActionPerson.Populated = false;

    return Read("ReadLegalActionPerson2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladRNumber", import.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier", import.LegalAction.Identifier);
        db.SetNullableString(
          command, "accountType", import.HardcodeObligorLap.AccountType ?? "");
      },
      (db, reader) =>
      {
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 1);
        entities.LegalActionPerson.LgaIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.LegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 3);
        entities.LegalActionPerson.LadRNumber = db.GetNullableInt32(reader, 4);
        entities.LegalActionPerson.AccountType =
          db.GetNullableString(reader, 5);
        entities.LegalActionPerson.Populated = true;
      });
  }

  private bool ReadLegalActionPerson3()
  {
    entities.LegalActionPerson.Populated = false;

    return Read("ReadLegalActionPerson3",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", import.LegalAction.Identifier);
        db.SetNullableString(command, "cspNumber", import.Obligor.Number);
      },
      (db, reader) =>
      {
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 1);
        entities.LegalActionPerson.LgaIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.LegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 3);
        entities.LegalActionPerson.LadRNumber = db.GetNullableInt32(reader, 4);
        entities.LegalActionPerson.AccountType =
          db.GetNullableString(reader, 5);
        entities.LegalActionPerson.Populated = true;
      });
  }

  private bool ReadObligation()
  {
    entities.Obligation.Populated = false;

    return Read("ReadObligation",
      (db, command) =>
      {
        db.
          SetInt32(command, "obId", import.Obligation.SystemGeneratedIdentifier);
          
        db.SetString(command, "cspNumber", import.Obligor.Number);
        db.SetString(command, "cpaType", import.HcCpaObligor.Type1);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
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
    /// A value of HardcodeObligorLap.
    /// </summary>
    [JsonPropertyName("hardcodeObligorLap")]
    public LegalActionPerson HardcodeObligorLap
    {
      get => hardcodeObligorLap ??= new();
      set => hardcodeObligorLap = value;
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
    /// A value of HcOtrnDtAccrual.
    /// </summary>
    [JsonPropertyName("hcOtrnDtAccrual")]
    public ObligationTransaction HcOtrnDtAccrual
    {
      get => hcOtrnDtAccrual ??= new();
      set => hcOtrnDtAccrual = value;
    }

    /// <summary>
    /// A value of HcDdshActiveStatus.
    /// </summary>
    [JsonPropertyName("hcDdshActiveStatus")]
    public DebtDetailStatusHistory HcDdshActiveStatus
    {
      get => hcDdshActiveStatus ??= new();
      set => hcDdshActiveStatus = value;
    }

    /// <summary>
    /// A value of HcOtrnDtVoluntary.
    /// </summary>
    [JsonPropertyName("hcOtrnDtVoluntary")]
    public ObligationTransaction HcOtrnDtVoluntary
    {
      get => hcOtrnDtVoluntary ??= new();
      set => hcOtrnDtVoluntary = value;
    }

    /// <summary>
    /// A value of HcCpaSupportedPerson.
    /// </summary>
    [JsonPropertyName("hcCpaSupportedPerson")]
    public CsePersonAccount HcCpaSupportedPerson
    {
      get => hcCpaSupportedPerson ??= new();
      set => hcCpaSupportedPerson = value;
    }

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
    /// A value of HcOtCFeesClassificati.
    /// </summary>
    [JsonPropertyName("hcOtCFeesClassificati")]
    public ObligationType HcOtCFeesClassificati
    {
      get => hcOtCFeesClassificati ??= new();
      set => hcOtCFeesClassificati = value;
    }

    /// <summary>
    /// A value of HcOtCRecoverClassific.
    /// </summary>
    [JsonPropertyName("hcOtCRecoverClassific")]
    public ObligationType HcOtCRecoverClassific
    {
      get => hcOtCRecoverClassific ??= new();
      set => hcOtCRecoverClassific = value;
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
    /// A value of HcOt718BUraJudgement.
    /// </summary>
    [JsonPropertyName("hcOt718BUraJudgement")]
    public ObligationType HcOt718BUraJudgement
    {
      get => hcOt718BUraJudgement ??= new();
      set => hcOt718BUraJudgement = value;
    }

    /// <summary>
    /// A value of HcOtCVoluntaryClassif.
    /// </summary>
    [JsonPropertyName("hcOtCVoluntaryClassif")]
    public ObligationType HcOtCVoluntaryClassif
    {
      get => hcOtCVoluntaryClassif ??= new();
      set => hcOtCVoluntaryClassif = value;
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
    /// A value of HistoryIndicator.
    /// </summary>
    [JsonPropertyName("historyIndicator")]
    public Common HistoryIndicator
    {
      get => historyIndicator ??= new();
      set => historyIndicator = value;
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
    /// A value of Accruing.
    /// </summary>
    [JsonPropertyName("accruing")]
    public Common Accruing
    {
      get => accruing ??= new();
      set => accruing = value;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePerson Supported
    {
      get => supported ??= new();
      set => supported = value;
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
    /// A value of AccrualInstructions.
    /// </summary>
    [JsonPropertyName("accrualInstructions")]
    public AccrualInstructions AccrualInstructions
    {
      get => accrualInstructions ??= new();
      set => accrualInstructions = value;
    }

    /// <summary>
    /// A value of ObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("obligationPaymentSchedule")]
    public ObligationPaymentSchedule ObligationPaymentSchedule
    {
      get => obligationPaymentSchedule ??= new();
      set => obligationPaymentSchedule = value;
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
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    /// <summary>
    /// A value of Hardcoded.
    /// </summary>
    [JsonPropertyName("hardcoded")]
    public ObligationTransaction Hardcoded
    {
      get => hardcoded ??= new();
      set => hardcoded = value;
    }

    /// <summary>
    /// A value of NumberOfSupportedPrsns.
    /// </summary>
    [JsonPropertyName("numberOfSupportedPrsns")]
    public Common NumberOfSupportedPrsns
    {
      get => numberOfSupportedPrsns ??= new();
      set => numberOfSupportedPrsns = value;
    }

    private LegalActionPerson hardcodeObligorLap;
    private ObligationTransaction hcOtrnTDebt;
    private ObligationTransaction hcOtrnDtAccrual;
    private DebtDetailStatusHistory hcDdshActiveStatus;
    private ObligationTransaction hcOtrnDtVoluntary;
    private CsePersonAccount hcCpaSupportedPerson;
    private CsePersonAccount hcCpaObligor;
    private ObligationType hcOtCFeesClassificati;
    private ObligationType hcOtCRecoverClassific;
    private ObligationTransaction hcOtrnDtDebtDetail;
    private ObligationType hcOt718BUraJudgement;
    private ObligationType hcOtCVoluntaryClassif;
    private DateWorkArea max;
    private DateWorkArea current;
    private Common historyIndicator;
    private LegalActionDetail legalActionDetail;
    private Common accruing;
    private CsePerson obligor;
    private Obligation obligation;
    private ObligationType obligationType;
    private CsePerson supported;
    private ObligationTransaction obligationTransaction;
    private AccrualInstructions accrualInstructions;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private LegalAction legalAction;
    private DebtDetail debtDetail;
    private ObligationTransaction hardcoded;
    private Common numberOfSupportedPrsns;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
      /// <summary>
      /// A value of ZdelExportGrpDetail.
      /// </summary>
      [JsonPropertyName("zdelExportGrpDetail")]
      public Program ZdelExportGrpDetail
      {
        get => zdelExportGrpDetail ??= new();
        set => zdelExportGrpDetail = value;
      }

      /// <summary>
      /// A value of DetailSupported.
      /// </summary>
      [JsonPropertyName("detailSupported")]
      public CsePerson DetailSupported
      {
        get => detailSupported ??= new();
        set => detailSupported = value;
      }

      /// <summary>
      /// A value of SupportedDetail.
      /// </summary>
      [JsonPropertyName("supportedDetail")]
      public CsePersonsWorkSet SupportedDetail
      {
        get => supportedDetail ??= new();
        set => supportedDetail = value;
      }

      /// <summary>
      /// A value of DetailCase.
      /// </summary>
      [JsonPropertyName("detailCase")]
      public Case1 DetailCase
      {
        get => detailCase ??= new();
        set => detailCase = value;
      }

      /// <summary>
      /// A value of DetailObligationTransaction.
      /// </summary>
      [JsonPropertyName("detailObligationTransaction")]
      public ObligationTransaction DetailObligationTransaction
      {
        get => detailObligationTransaction ??= new();
        set => detailObligationTransaction = value;
      }

      /// <summary>
      /// A value of DetailDebtDetail.
      /// </summary>
      [JsonPropertyName("detailDebtDetail")]
      public DebtDetail DetailDebtDetail
      {
        get => detailDebtDetail ??= new();
        set => detailDebtDetail = value;
      }

      /// <summary>
      /// A value of DetailServiceProvider.
      /// </summary>
      [JsonPropertyName("detailServiceProvider")]
      public ServiceProvider DetailServiceProvider
      {
        get => detailServiceProvider ??= new();
        set => detailServiceProvider = value;
      }

      /// <summary>
      /// A value of DetailObligationPaymentSchedule.
      /// </summary>
      [JsonPropertyName("detailObligationPaymentSchedule")]
      public ObligationPaymentSchedule DetailObligationPaymentSchedule
      {
        get => detailObligationPaymentSchedule ??= new();
        set => detailObligationPaymentSchedule = value;
      }

      /// <summary>
      /// A value of ConcurrentDetail.
      /// </summary>
      [JsonPropertyName("concurrentDetail")]
      public ObligationTransaction ConcurrentDetail
      {
        get => concurrentDetail ??= new();
        set => concurrentDetail = value;
      }

      /// <summary>
      /// A value of PrevDetail.
      /// </summary>
      [JsonPropertyName("prevDetail")]
      public ObligationTransaction PrevDetail
      {
        get => prevDetail ??= new();
        set => prevDetail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Program zdelExportGrpDetail;
      private CsePerson detailSupported;
      private CsePersonsWorkSet supportedDetail;
      private Case1 detailCase;
      private ObligationTransaction detailObligationTransaction;
      private DebtDetail detailDebtDetail;
      private ServiceProvider detailServiceProvider;
      private ObligationPaymentSchedule detailObligationPaymentSchedule;
      private ObligationTransaction concurrentDetail;
      private ObligationTransaction prevDetail;
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
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 => export1 ??= new(ExportGroup.Capacity);

    /// <summary>
    /// Gets a value of Export1 for json serialization.
    /// </summary>
    [JsonPropertyName("export1")]
    [Computed]
    public IList<ExportGroup> Export1_Json
    {
      get => export1;
      set => Export1.Assign(value);
    }

    private ObligationTransaction obligationTransaction;
    private Array<ExportGroup> export1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ForDebtScreens.
    /// </summary>
    [JsonPropertyName("forDebtScreens")]
    public Common ForDebtScreens
    {
      get => forDebtScreens ??= new();
      set => forDebtScreens = value;
    }

    /// <summary>
    /// A value of HardcodeObligorLap.
    /// </summary>
    [JsonPropertyName("hardcodeObligorLap")]
    public LegalActionPerson HardcodeObligorLap
    {
      get => hardcodeObligorLap ??= new();
      set => hardcodeObligorLap = value;
    }

    /// <summary>
    /// A value of PreconvPgmType.
    /// </summary>
    [JsonPropertyName("preconvPgmType")]
    public DebtDetail PreconvPgmType
    {
      get => preconvPgmType ??= new();
      set => preconvPgmType = value;
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
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
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
    /// A value of AccrualInstructions.
    /// </summary>
    [JsonPropertyName("accrualInstructions")]
    public AccrualInstructions AccrualInstructions
    {
      get => accrualInstructions ??= new();
      set => accrualInstructions = value;
    }

    /// <summary>
    /// A value of Work.
    /// </summary>
    [JsonPropertyName("work")]
    public Common Work
    {
      get => work ??= new();
      set => work = value;
    }

    /// <summary>
    /// A value of RetryLoop.
    /// </summary>
    [JsonPropertyName("retryLoop")]
    public Common RetryLoop
    {
      get => retryLoop ??= new();
      set => retryLoop = value;
    }

    private Common forDebtScreens;
    private LegalActionPerson hardcodeObligorLap;
    private DebtDetail preconvPgmType;
    private DateWorkArea max;
    private DebtDetail debtDetail;
    private DateWorkArea initialized;
    private AccrualInstructions accrualInstructions;
    private Common work;
    private Common retryLoop;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of AccrualSuspension.
    /// </summary>
    [JsonPropertyName("accrualSuspension")]
    public AccrualSuspension AccrualSuspension
    {
      get => accrualSuspension ??= new();
      set => accrualSuspension = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of ObligorCsePersonAccount.
    /// </summary>
    [JsonPropertyName("obligorCsePersonAccount")]
    public CsePersonAccount ObligorCsePersonAccount
    {
      get => obligorCsePersonAccount ??= new();
      set => obligorCsePersonAccount = value;
    }

    /// <summary>
    /// A value of SupportedCsePerson.
    /// </summary>
    [JsonPropertyName("supportedCsePerson")]
    public CsePerson SupportedCsePerson
    {
      get => supportedCsePerson ??= new();
      set => supportedCsePerson = value;
    }

    /// <summary>
    /// A value of SupportedCsePersonAccount.
    /// </summary>
    [JsonPropertyName("supportedCsePersonAccount")]
    public CsePersonAccount SupportedCsePersonAccount
    {
      get => supportedCsePersonAccount ??= new();
      set => supportedCsePersonAccount = value;
    }

    /// <summary>
    /// A value of AccrualInstructions.
    /// </summary>
    [JsonPropertyName("accrualInstructions")]
    public AccrualInstructions AccrualInstructions
    {
      get => accrualInstructions ??= new();
      set => accrualInstructions = value;
    }

    /// <summary>
    /// A value of DebtDetailStatusHistory.
    /// </summary>
    [JsonPropertyName("debtDetailStatusHistory")]
    public DebtDetailStatusHistory DebtDetailStatusHistory
    {
      get => debtDetailStatusHistory ??= new();
      set => debtDetailStatusHistory = value;
    }

    /// <summary>
    /// A value of ObligorCsePerson.
    /// </summary>
    [JsonPropertyName("obligorCsePerson")]
    public CsePerson ObligorCsePerson
    {
      get => obligorCsePerson ??= new();
      set => obligorCsePerson = value;
    }

    private AccrualSuspension accrualSuspension;
    private LegalActionPerson legalActionPerson;
    private LegalAction legalAction;
    private LegalActionDetail legalActionDetail;
    private DebtDetail debtDetail;
    private ObligationTransaction obligationTransaction;
    private Obligation obligation;
    private CsePersonAccount obligorCsePersonAccount;
    private CsePerson supportedCsePerson;
    private CsePersonAccount supportedCsePersonAccount;
    private AccrualInstructions accrualInstructions;
    private DebtDetailStatusHistory debtDetailStatusHistory;
    private CsePerson obligorCsePerson;
  }
#endregion
}
