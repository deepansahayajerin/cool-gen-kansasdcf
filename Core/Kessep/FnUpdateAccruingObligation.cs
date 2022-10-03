// Program: FN_UPDATE_ACCRUING_OBLIGATION, ID: 372084588, model: 746.
// Short name: SWE00626
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
/// A program: FN_UPDATE_ACCRUING_OBLIGATION.
/// </para>
/// <para>
/// RESP: FINANCE
/// This action block performs all processing associated with updating an 
/// Accruing Obligation and its details.
/// </para>
/// </summary>
[Serializable]
public partial class FnUpdateAccruingObligation: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_UPDATE_ACCRUING_OBLIGATION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnUpdateAccruingObligation(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnUpdateAccruingObligation.
  /// </summary>
  public FnUpdateAccruingObligation(IContext context, Import import,
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
    // =================================================
    // Bud Adams          8/25/98  Added work view to Import_Group view so it 
    // will match correctly.  Also, imported current-date attributes and deleted
    // CURRENT_DATE and CURRENT_TIMESTAMP functions.
    // =================================================
    // 12/17/98 - B Adams  -  changed the way Interstate_Request_Obligation
    //   was being updated.  See below.
    //   Added Exception conditions to CRUDs where they had been
    //   intentionally removed and had NOTEs added saying that a
    //   not found condition would cause an abort (e.g.)
    //   Changed Read properties (select only)
    // =================================================
    ExitState = "ACO_NN0000_ALL_OK";
    local.HardcodeConcurr.SystemGeneratedIdentifier = 13;

    export.Group.Index = 0;
    export.Group.Clear();

    for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
      import.Group.Index)
    {
      if (export.Group.IsFull)
      {
        break;
      }

      export.Group.Update.AccrualInstructions.DiscontinueDt =
        import.Group.Item.AccrualInstructions.DiscontinueDt;
      export.Group.Update.Hidden.DiscontinueDt =
        import.Group.Item.Hidden.DiscontinueDt;
      export.Group.Update.Case1.Number = import.Group.Item.Case1.Number;
      MoveObligationTransaction(import.Group.Item.ObligationTransaction,
        export.Group.Update.ObligationTransaction);
      export.Group.Update.Program.Code = import.Group.Item.Program.Code;
      export.Group.Update.ProgramScreenAttributes.ProgramTypeInd =
        import.Group.Item.ProgramScreenAttributes.ProgramTypeInd;
      export.Group.Update.ServiceProvider.UserId =
        import.Group.Item.ServiceProvider.UserId;
      export.Group.Update.HiddenConcurrent.SystemGeneratedIdentifier =
        import.Group.Item.HiddenConcurrent.SystemGeneratedIdentifier;
      export.Group.Update.Previous.Amount = import.Group.Item.Previous.Amount;
      export.Group.Update.Prompt.Flag = import.Group.Item.Prompt.Flag;
      export.Group.Update.ProratePercentage.Percentage =
        import.Group.Item.ProratePercentage.Percentage;
      MoveCommon(import.Group.Item.Sel, export.Group.Update.Sel);
      export.Group.Update.SupportedCsePerson.Number =
        import.Group.Item.SupportedCsePerson.Number;
      MoveCsePersonsWorkSet(import.Group.Item.SupportedCsePersonsWorkSet,
        export.Group.Update.SupportedCsePersonsWorkSet);
      export.Group.Update.SuspendAccrual.
        Assign(import.Group.Item.SuspendAccrual);
      export.Group.Next();
    }

    // =================================================
    // 10-23-98 - B Adams  -  Deleted read of Obligation_Type.  It
    //   has already been read in previous action diagram.  Replaced
    //   other references to use SOME / THAT logic.
    //   Also, deleted read of CSE_Person.
    // =================================================
    // If the obligation is active, the only updatable field is
    // discontinue date on Obligation Payment Schedule
    // ***---  CURSOR ONLY.  That's what IEF uses for Updating.
    if (ReadObligation1())
    {
      // : IF the obligation is active, bypass the Obligation Update.
      try
      {
        UpdateObligation1();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_OBLIGATION_NU_RB";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_OBLIGATION_PV_RB";

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
      ExitState = "FN0000_OBLIGATION_NF_RB";
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ***--- Sumanta - MTW - 04/29/1997
    // ***--- cab_set_alternate address has been removed.
    //        The following IF .. associate statements
    //        has been added.
    // ***---
    // =================================================
    // 12/16/98 - B Adams  -  user must be able to change alternate
    //   billing address to <SPACES>.  Also, there's no need to do
    //   any database writing if there are no changes.
    //   Changed the IF construct
    // =================================================
    if (AsChar(import.AltAdd.Flag) == 'C')
    {
      UseFnUpdateAlternateAddress2();

      if (IsExitState("FN0000_CSE_PERSON_ACCOUNT_NF_RB"))
      {
        export.AltBillLocError.SelectChar = "Y";
      }
      else if (IsExitState("FN0000_ALTERNATE_ADDR_NF_RB"))
      {
        export.AltBillLocError.SelectChar = "Y";
      }
      else
      {
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // ***--- RBM - MTW - 02/26/98
    // ***--- Update the associated interstate req oblig ..
    // ***---
    if (AsChar(import.UpdateInterstateInfo.Flag) == 'Y')
    {
      // =================================================
      // 2/23/1999 - b adams  -  The existing logic was incorrect.
      //   Replaced it all with this CAB which will now be used by
      //   all Debt PrADS.
      // =================================================
      UseFnUpdateInterstateRqstOblign2();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // : Update Obligation Payment Schedule.  If the Obligation is active, 
    // update End_Date only.
    // =================================================
    // +++++++++++++++++++++++++++++++++++++++++++++++++
    // 2/5/1999 - bud adams  -  Although the data model allows one
    //   obligation to have many obligation-payment-schedule rows
    //   this is NOT the case for accruing obligations.  (These debts
    //   do not really have payment schedules, but that entity type
    //   has been co-opted.)
    //   Each time frame is really a new obligation.  So, the READ
    //   actions here, although they LOOK as if they are not fully
    //   qualified, they are in the case of accruing obligations.
    // +++++++++++++++++++++++++++++++++++++++++++++++++
    // =================================================
    if (!ReadObligationPaymentSchedule3())
    {
      // =================================================
      // 2/23/1999 - B Adams  -  converted debts, if they're inactive,
      //   may have no obligation-payment-schedule, and that's OK
      //   because they're not going to be accrued anyway.  They
      //   will have their amount adjusted down to 0, and then a new
      //   debt will be created with the correct amounts.
      // =================================================
      if (Equal(entities.Obligation.CreatedBy, "CONVERSN"))
      {
      }
      else
      {
        ExitState = "FN0000_OBLIG_PYMNT_SCH_NF_RB";

        return;
      }
    }

    // =================================================
    // 2/4/99 - B Adams  -  The day a debt is created, the Start_Dt
    //   can be changed.  But, Start_Dt is an identifier so the 'old'
    //   one has to be deleted and a new one added.
    //   Since IEF does CURRENT OF CURSOR updates only, we
    //   want to limit the number of Cursors opened and locks taken
    //   to only when necessary.   These dates may not be updated
    //   very often.
    // =================================================
    if (!Equal(import.Discontinue.Date, entities.ObligationPaymentSchedule.EndDt)
      && Equal
      (import.ObligationPaymentSchedule.StartDt,
      entities.ObligationPaymentSchedule.StartDt))
    {
      if (ReadObligationPaymentSchedule1())
      {
        try
        {
          UpdateObligationPaymentSchedule();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_OBLIG_PYMNT_SCH_NU_RB";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_OBLIG_PYMNT_SCH_PV_RB";

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
        ExitState = "FN0000_OBLIG_PYMNT_SCH_NF_RB";
      }
    }
    else if (!Equal(import.ObligationPaymentSchedule.StartDt,
      entities.ObligationPaymentSchedule.StartDt))
    {
      // =================================================
      // 2/4/1999 - B Adams  -  This can happen ONLY on the day the
      //   obligation was created.  After that, accruals have run and
      //   we are not allowing the Start_Date to be changed.
      //   This rule is being enforced by the procedure.
      // =================================================
      if (ReadObligationPaymentSchedule5())
      {
        DeleteObligationPaymentSchedule();

        // =================================================
        // PR# 75247: Bud Adams - 9/28/99  -  Created_By attributes
        //   were not being SET and that info was being lost.
        // =================================================
        try
        {
          CreateObligationPaymentSchedule1();

          // !!!!
          // =================================================
          // PR# 75247: bud adams - 9/28/99  -  When Accrual Start Date
          //   is changed on the screen, then the Accrual_Instructions
          //   As_Of_Date must also be updated.
          // =================================================
          foreach(var item in ReadAccrualInstructions3())
          {
            try
            {
              UpdateAccrualInstructions1();
            }
            catch(Exception e1)
            {
              switch(GetErrorCode(e1))
              {
                case ErrorCode.AlreadyExists:
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
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_OBLIG_PYMNT_SCH_AE_RB";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_OBLIG_PYMNT_SCH_PV_RB";

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
        ExitState = "FN0000_OBLIG_PYMNT_SCH_NF_RB";
      }
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (!IsEmpty(import.ConcurrentCsePerson.Number))
    {
      // ***---  CURSOR ONLY - updates are cursor only in IEF
      if (ReadObligation2())
      {
        try
        {
          UpdateObligation2();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_CONCURRENT_OBLIG_NU_RB";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_CONCURRENT_OBLIG_PV_RB";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        // ***--- Sumanta - MTW - 04/29/1997
        // ***--- cab_set_alternate address has been removed.
        //        The following IF .. associate statements
        //        has been added.
        // ***---
        // =================================================
        // 2/23/1999 - b adams  -  The logic was totally wrong and so
        //   was the IF construct.  Replaced logic with this CAB.
        // =================================================
        if (AsChar(import.AltAdd.Flag) == 'C')
        {
          UseFnUpdateAlternateAddress1();

          if (IsExitState("FN0000_CSE_PERSON_ACCOUNT_NF_RB"))
          {
            export.AltBillLocError.SelectChar = "Y";
          }
          else if (IsExitState("FN0000_ALTERNATE_ADDR_NF_RB"))
          {
            export.AltBillLocError.SelectChar = "Y";
          }
          else
          {
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }
        }

        if (AsChar(import.UpdateInterstateInfo.Flag) == 'Y')
        {
          // =================================================
          // 2/23/1999 - b adams  -  The existing logic was incorrect.
          //   Replaced it all with this CAB which will now be used by
          //   all Debt PrADS.  Also, the IF construct was wrong.
          // =================================================
          UseFnUpdateInterstateRqstOblign1();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }
        }

        // : Update Obligation Payment Schedule.  If the Obligation is active, 
        // update End_Date only.
        if (!ReadObligationPaymentSchedule4())
        {
          // =================================================
          // 2/23/1999 - B Adams  -  converted debts, if they're inactive,
          //   may have no obligation-payment-schedule, and that's OK
          //   because they're not going to be accrued anyway.  They
          //   will have their amount adjusted down to 0, and then a new
          //   debt will be created with the correct amounts.
          // =================================================
          if (Equal(entities.Obligation.CreatedBy, "CONVERSN"))
          {
          }
          else
          {
            ExitState = "FN0000_OBLIG_PYMNT_SCH_NF_RB";

            return;
          }
        }

        // =================================================
        // 2/4/99 - B Adams  -  The day a debt is created, the Start_Dt
        //   can be changed.  But, Start_Dt is an identifier so the 'old'
        //   one has to be deleted and a new one added.
        //   Since IEF does CURRENT OF CURSOR updates only, we
        //   want to limit the number of Cursors opened and locks taken
        //   to only when necessary.   These dates may not be updated
        //   very often.
        // =================================================
        if (!Equal(import.Discontinue.Date,
          entities.ConcurrentObligationPaymentSchedule.EndDt) && Equal
          (import.ObligationPaymentSchedule.StartDt,
          entities.ConcurrentObligationPaymentSchedule.StartDt))
        {
          if (ReadObligationPaymentSchedule2())
          {
            try
            {
              UpdateObligationPaymentSchedule();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "FN0000_OBLIG_PYMNT_SCH_NU_RB";

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "FN0000_OBLIG_PYMNT_SCH_PV_RB";

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
            ExitState = "FN0000_OBLIG_PYMNT_SCH_NF_RB";
          }
        }
        else if (!Equal(import.ObligationPaymentSchedule.StartDt,
          entities.ConcurrentObligationPaymentSchedule.StartDt))
        {
          if (ReadObligationPaymentSchedule6())
          {
            DeleteObligationPaymentSchedule();

            // =================================================
            // PR# 75247: Bud Adams - 9/28/99  -  Created_By attributes
            //   were not being SET and that info was being lost.
            // =================================================
            try
            {
              CreateObligationPaymentSchedule2();

              // !!!!
              // =================================================
              // PR# 75247: bud adams - 9/28/99  -  When Accrual Start Date
              //   is changed on the screen, then the Accrual_Instructions
              //   As_Of_Date must also be updated.
              // =================================================
              foreach(var item in ReadAccrualInstructions4())
              {
                try
                {
                  UpdateAccrualInstructions1();
                }
                catch(Exception e1)
                {
                  switch(GetErrorCode(e1))
                  {
                    case ErrorCode.AlreadyExists:
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
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "FN0000_OBLIG_PYMNT_SCH_AE_RB";

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "FN0000_OBLIG_PYMNT_SCH_PV_RB";

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
            ExitState = "FN0000_OBLIG_PYMNT_SCH_NF_RB";
          }
        }
      }
      else
      {
        ExitState = "FN0000_CONCURRENT_OBLIG_NF_RB";
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // $$$
    for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
      export.Group.Index)
    {
      // =================================================
      // 6/1/99 - bud adams  -  A non-case related person should not
      //   be selectable on the screen, but if the discontinue date for
      //   the obligation is changed, then discontinue dates for all the
      //   supported persons will be changed.  BUT, this is out of
      //   context when non-case related persons are involved.
      // =================================================
      if (Equal(export.Group.Item.ProgramScreenAttributes.ProgramTypeInd, "Z"))
      {
        continue;
      }

      if (AsChar(export.Group.Item.Sel.SelectChar) == 'S')
      {
        if (Equal(export.Group.Item.AccrualInstructions.DiscontinueDt,
          local.ZeroDate.Date))
        {
          export.Group.Update.AccrualInstructions.DiscontinueDt =
            import.Maximum.Date;
        }

        // Obligation is Active - update discontinue date only
        if (!ReadCsePersonCsePersonAccount())
        {
          continue;
        }

        // =================================================
        // 2/10/1999 - bud adams  -  If the Supported_Person Discontinue
        //   Date is earlier than the Debt Discontinue Date, then the
        //   Supported_Person date is the one that matters.
        //   Otherwise, the Debt Discontinue Date is used for the Accrual
        //   Instructions End_Date.
        //   (Obligation_Payment_Schedule End_Date, at this point, is
        //    the same as the Debt Discontinue_Date.)
        // =================================================
        if (ReadAccrualInstructions1())
        {
          // =================================================
          // 6/7/99 - b adams  -  this will be used to update the legal_
          //   action_person end_date below, if appropriate.
          // =================================================
          local.AccrualInstructions.DiscontinueDt =
            export.Group.Item.AccrualInstructions.DiscontinueDt;

          if (!Lt(import.ObligationPaymentSchedule.EndDt,
            export.Group.Item.AccrualInstructions.DiscontinueDt) || Equal
            (export.Group.Item.AccrualInstructions.DiscontinueDt,
            import.Maximum.Date))
          {
            if (!Equal(export.Group.Item.AccrualInstructions.DiscontinueDt,
              entities.AccrualInstructions.DiscontinueDt))
            {
              try
              {
                UpdateAccrualInstructions3();
                ExitState = "ACO_NN0000_ALL_OK";
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "FN0000_ACCRUAL_INSTRUCTION_NU_RB";

                    break;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "FN0000_ACCRUAL_INSTR_PV_RB";

                    break;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }
            }
          }
          else if (Equal(entities.AccrualInstructions.DiscontinueDt,
            entities.ObligationPaymentSchedule.EndDt) || Lt
            (entities.AccrualInstructions.DiscontinueDt,
            entities.ObligationPaymentSchedule.EndDt) && Lt
            (export.Group.Item.AccrualInstructions.DiscontinueDt,
            entities.AccrualInstructions.DiscontinueDt))
          {
            try
            {
              UpdateAccrualInstructions4();

              // =================================================
              // 6/7/99 - b adams  -  this will be used to update the legal_
              //   action_person end_date below, if appropriate.
              // =================================================
              local.AccrualInstructions.DiscontinueDt =
                import.ObligationPaymentSchedule.EndDt;
              export.Group.Update.AccrualInstructions.DiscontinueDt =
                import.ObligationPaymentSchedule.EndDt;
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "FN0000_ACCRUAL_INSTRUCTION_NU_RB";

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "FN0000_ACCRUAL_INSTR_PV_RB";

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
          ExitState = "FN0000_ACCRUAL_INSTRUCTION_NF_RB";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (entities.ConcurrentObligation.Populated)
        {
          if (ReadAccrualInstructions2())
          {
            if (Lt(export.Group.Item.AccrualInstructions.DiscontinueDt,
              import.ObligationPaymentSchedule.EndDt) || Equal
              (export.Group.Item.AccrualInstructions.DiscontinueDt,
              import.Maximum.Date))
            {
              if (!Equal(export.Group.Item.AccrualInstructions.DiscontinueDt,
                entities.AccrualInstructions.DiscontinueDt))
              {
                try
                {
                  UpdateAccrualInstructions2();
                }
                catch(Exception e)
                {
                  switch(GetErrorCode(e))
                  {
                    case ErrorCode.AlreadyExists:
                      ExitState = "FN0000_ACCRUAL_INSTRUCTION_NU_RB";

                      break;
                    case ErrorCode.PermittedValueViolation:
                      ExitState = "FN0000_ACCRUAL_INSTR_PV_RB";

                      break;
                    case ErrorCode.DatabaseError:
                      break;
                    default:
                      throw;
                  }
                }
              }
            }
            else if (Equal(entities.AccrualInstructions.DiscontinueDt,
              entities.ConcurrentObligationPaymentSchedule.EndDt) || Lt
              (entities.AccrualInstructions.DiscontinueDt,
              entities.ConcurrentObligationPaymentSchedule.EndDt) && Lt
              (export.Group.Item.AccrualInstructions.DiscontinueDt,
              entities.AccrualInstructions.DiscontinueDt))
            {
              try
              {
                UpdateAccrualInstructions4();
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "FN0000_ACCRUAL_INSTRUCTION_NU_RB";

                    break;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "FN0000_ACCRUAL_INSTR_PV_RB";

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
            ExitState = "FN0000_ACCRUAL_INSTRUCTION_NF_RB";
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }
        }

        // =================================================
        // 1/30/99 - Bud Adams  -  Supported Person Discontinue Date
        //   on the screen can be updated sometimes.  If so, then the
        //   Legal_Action_Person End_Date must be updated with that
        //   value.
        // 6/16/099 - Bud Adams  -  Jan Brigham determined that this
        //   should not be done.  LAP reflects the court order information
        //   and should only be changed by 'legal' screens.
        // 12/9/99 - b adams  -  deleted the commented out code.
        // =================================================
      }
      else
      {
      }
    }
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Flag = source.Flag;
    target.SelectChar = source.SelectChar;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveObligationTransaction(ObligationTransaction source,
    ObligationTransaction target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Amount = source.Amount;
  }

  private void UseFnUpdateAlternateAddress1()
  {
    var useImport = new FnUpdateAlternateAddress.Import();
    var useExport = new FnUpdateAlternateAddress.Export();

    useImport.HcCpaObligor.Type1 = import.HcCpaObligor.Type1;
    useImport.AlternateBillingAddress.Number = import.AltAdd.Number;
    useImport.ObligationType.SystemGeneratedIdentifier =
      import.ObligationType.SystemGeneratedIdentifier;
    useImport.Obligor.Number = import.ConcurrentCsePerson.Number;
    useImport.Obligation.SystemGeneratedIdentifier =
      import.ConcurrentObligation.SystemGeneratedIdentifier;

    Call(FnUpdateAlternateAddress.Execute, useImport, useExport);
  }

  private void UseFnUpdateAlternateAddress2()
  {
    var useImport = new FnUpdateAlternateAddress.Import();
    var useExport = new FnUpdateAlternateAddress.Export();

    useImport.HcCpaObligor.Type1 = import.HcCpaObligor.Type1;
    useImport.AlternateBillingAddress.Number = import.AltAdd.Number;
    useImport.ObligationType.SystemGeneratedIdentifier =
      import.ObligationType.SystemGeneratedIdentifier;
    useImport.Obligor.Number = import.Obligor.Number;
    useImport.Obligation.SystemGeneratedIdentifier =
      import.Obligation.SystemGeneratedIdentifier;

    Call(FnUpdateAlternateAddress.Execute, useImport, useExport);
  }

  private void UseFnUpdateInterstateRqstOblign1()
  {
    var useImport = new FnUpdateInterstateRqstOblign.Import();
    var useExport = new FnUpdateInterstateRqstOblign.Export();

    useImport.Current.Date = import.CurrentDate.Date;
    useImport.Max.Date = import.Maximum.Date;
    useImport.Old.IntHGeneratedId = import.Old.IntHGeneratedId;
    useImport.New1.IntHGeneratedId = import.New1.IntHGeneratedId;
    useImport.Obligation.SystemGeneratedIdentifier =
      import.ConcurrentObligation.SystemGeneratedIdentifier;
    useImport.ObligationType.SystemGeneratedIdentifier =
      import.ObligationType.SystemGeneratedIdentifier;
    useImport.CsePersonAccount.Type1 = import.HcCpaObligor.Type1;
    useImport.CsePerson.Number = import.ConcurrentCsePerson.Number;

    Call(FnUpdateInterstateRqstOblign.Execute, useImport, useExport);
  }

  private void UseFnUpdateInterstateRqstOblign2()
  {
    var useImport = new FnUpdateInterstateRqstOblign.Import();
    var useExport = new FnUpdateInterstateRqstOblign.Export();

    useImport.Current.Date = import.CurrentDate.Date;
    useImport.Max.Date = import.Maximum.Date;
    useImport.Old.IntHGeneratedId = import.Old.IntHGeneratedId;
    useImport.New1.IntHGeneratedId = import.New1.IntHGeneratedId;
    useImport.Obligation.SystemGeneratedIdentifier =
      import.Obligation.SystemGeneratedIdentifier;
    useImport.ObligationType.SystemGeneratedIdentifier =
      import.ObligationType.SystemGeneratedIdentifier;
    useImport.CsePersonAccount.Type1 = import.HcCpaObligor.Type1;
    useImport.CsePerson.Number = import.Obligor.Number;

    Call(FnUpdateInterstateRqstOblign.Execute, useImport, useExport);
  }

  private void CreateObligationPaymentSchedule1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);

    var otyType = entities.Obligation.DtyGeneratedId;
    var obgGeneratedId = entities.Obligation.SystemGeneratedIdentifier;
    var obgCspNumber = entities.Obligation.CspNumber;
    var obgCpaType = entities.Obligation.CpaType;
    var startDt = import.ObligationPaymentSchedule.StartDt;
    var amount = entities.ObligationPaymentSchedule.Amount;
    var endDt = entities.ObligationPaymentSchedule.EndDt;
    var createdBy = entities.UpdateOnly.CreatedBy;
    var createdTmst = entities.UpdateOnly.CreatedTmst;
    var lastUpdateBy = entities.ObligationPaymentSchedule.LastUpdateBy;
    var lastUpdateTmst = entities.ObligationPaymentSchedule.LastUpdateTmst;
    var frequencyCode = entities.ObligationPaymentSchedule.FrequencyCode;
    var dayOfWeek = entities.ObligationPaymentSchedule.DayOfWeek;
    var dayOfMonth1 = entities.ObligationPaymentSchedule.DayOfMonth1;
    var dayOfMonth2 = entities.ObligationPaymentSchedule.DayOfMonth2;
    var periodInd = entities.ObligationPaymentSchedule.PeriodInd;

    CheckValid<ObligationPaymentSchedule>("ObgCpaType", obgCpaType);
    CheckValid<ObligationPaymentSchedule>("FrequencyCode", frequencyCode);
    CheckValid<ObligationPaymentSchedule>("PeriodInd", periodInd);
    entities.UpdateOnly.Populated = false;
    Update("CreateObligationPaymentSchedule1",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", otyType);
        db.SetInt32(command, "obgGeneratedId", obgGeneratedId);
        db.SetString(command, "obgCspNumber", obgCspNumber);
        db.SetString(command, "obgCpaType", obgCpaType);
        db.SetDate(command, "startDt", startDt);
        db.SetNullableDecimal(command, "obligPschAmt", amount);
        db.SetNullableDate(command, "endDt", endDt);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdateBy", lastUpdateBy);
        db.SetNullableDateTime(command, "lastUpdateTmst", lastUpdateTmst);
        db.SetString(command, "frqPrdCd", frequencyCode);
        db.SetNullableInt32(command, "dayOfWeek", dayOfWeek);
        db.SetNullableInt32(command, "dayOfMonth1", dayOfMonth1);
        db.SetNullableInt32(command, "dayOfMonth2", dayOfMonth2);
        db.SetNullableString(command, "periodInd", periodInd);
        db.SetNullableDate(command, "repymtLtrPrtDt", default(DateTime));
      });

    entities.UpdateOnly.OtyType = otyType;
    entities.UpdateOnly.ObgGeneratedId = obgGeneratedId;
    entities.UpdateOnly.ObgCspNumber = obgCspNumber;
    entities.UpdateOnly.ObgCpaType = obgCpaType;
    entities.UpdateOnly.StartDt = startDt;
    entities.UpdateOnly.Amount = amount;
    entities.UpdateOnly.EndDt = endDt;
    entities.UpdateOnly.LastUpdateBy = lastUpdateBy;
    entities.UpdateOnly.LastUpdateTmst = lastUpdateTmst;
    entities.UpdateOnly.FrequencyCode = frequencyCode;
    entities.UpdateOnly.DayOfWeek = dayOfWeek;
    entities.UpdateOnly.DayOfMonth1 = dayOfMonth1;
    entities.UpdateOnly.DayOfMonth2 = dayOfMonth2;
    entities.UpdateOnly.PeriodInd = periodInd;
    entities.UpdateOnly.Populated = true;
  }

  private void CreateObligationPaymentSchedule2()
  {
    System.Diagnostics.Debug.Assert(entities.ConcurrentObligation.Populated);

    var otyType = entities.ConcurrentObligation.DtyGeneratedId;
    var obgGeneratedId =
      entities.ConcurrentObligation.SystemGeneratedIdentifier;
    var obgCspNumber = entities.ConcurrentObligation.CspNumber;
    var obgCpaType = entities.ConcurrentObligation.CpaType;
    var startDt = import.ObligationPaymentSchedule.StartDt;
    var amount = entities.ConcurrentObligationPaymentSchedule.Amount;
    var endDt = entities.ConcurrentObligationPaymentSchedule.EndDt;
    var createdBy = entities.UpdateOnly.CreatedBy;
    var createdTmst = entities.UpdateOnly.CreatedTmst;
    var lastUpdateBy =
      entities.ConcurrentObligationPaymentSchedule.LastUpdateBy;
    var lastUpdateTmst =
      entities.ConcurrentObligationPaymentSchedule.LastUpdateTmst;
    var frequencyCode =
      entities.ConcurrentObligationPaymentSchedule.FrequencyCode;
    var dayOfWeek = entities.ConcurrentObligationPaymentSchedule.DayOfWeek;
    var dayOfMonth1 = entities.ConcurrentObligationPaymentSchedule.DayOfMonth1;
    var dayOfMonth2 = entities.ConcurrentObligationPaymentSchedule.DayOfMonth2;
    var periodInd = entities.ConcurrentObligationPaymentSchedule.PeriodInd;

    CheckValid<ObligationPaymentSchedule>("ObgCpaType", obgCpaType);
    CheckValid<ObligationPaymentSchedule>("FrequencyCode", frequencyCode);
    CheckValid<ObligationPaymentSchedule>("PeriodInd", periodInd);
    entities.UpdateOnly.Populated = false;
    Update("CreateObligationPaymentSchedule2",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", otyType);
        db.SetInt32(command, "obgGeneratedId", obgGeneratedId);
        db.SetString(command, "obgCspNumber", obgCspNumber);
        db.SetString(command, "obgCpaType", obgCpaType);
        db.SetDate(command, "startDt", startDt);
        db.SetNullableDecimal(command, "obligPschAmt", amount);
        db.SetNullableDate(command, "endDt", endDt);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdateBy", lastUpdateBy);
        db.SetNullableDateTime(command, "lastUpdateTmst", lastUpdateTmst);
        db.SetString(command, "frqPrdCd", frequencyCode);
        db.SetNullableInt32(command, "dayOfWeek", dayOfWeek);
        db.SetNullableInt32(command, "dayOfMonth1", dayOfMonth1);
        db.SetNullableInt32(command, "dayOfMonth2", dayOfMonth2);
        db.SetNullableString(command, "periodInd", periodInd);
        db.SetNullableDate(command, "repymtLtrPrtDt", default(DateTime));
      });

    entities.UpdateOnly.OtyType = otyType;
    entities.UpdateOnly.ObgGeneratedId = obgGeneratedId;
    entities.UpdateOnly.ObgCspNumber = obgCspNumber;
    entities.UpdateOnly.ObgCpaType = obgCpaType;
    entities.UpdateOnly.StartDt = startDt;
    entities.UpdateOnly.Amount = amount;
    entities.UpdateOnly.EndDt = endDt;
    entities.UpdateOnly.LastUpdateBy = lastUpdateBy;
    entities.UpdateOnly.LastUpdateTmst = lastUpdateTmst;
    entities.UpdateOnly.FrequencyCode = frequencyCode;
    entities.UpdateOnly.DayOfWeek = dayOfWeek;
    entities.UpdateOnly.DayOfMonth1 = dayOfMonth1;
    entities.UpdateOnly.DayOfMonth2 = dayOfMonth2;
    entities.UpdateOnly.PeriodInd = periodInd;
    entities.UpdateOnly.Populated = true;
  }

  private void DeleteObligationPaymentSchedule()
  {
    Update("DeleteObligationPaymentSchedule",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.UpdateOnly.OtyType);
        db.SetInt32(
          command, "obgGeneratedId", entities.UpdateOnly.ObgGeneratedId);
        db.SetString(command, "obgCspNumber", entities.UpdateOnly.ObgCspNumber);
        db.SetString(command, "obgCpaType", entities.UpdateOnly.ObgCpaType);
        db.SetDate(
          command, "startDt", entities.UpdateOnly.StartDt.GetValueOrDefault());
      });
  }

  private bool ReadAccrualInstructions1()
  {
    System.Diagnostics.Debug.
      Assert(entities.SupportedCsePersonAccount.Populated);
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.AccrualInstructions.Populated = false;

    return Read("ReadAccrualInstructions1",
      (db, command) =>
      {
        db.SetString(command, "obTrnTyp", import.HcOtDebtType.Type1);
        db.SetNullableString(
          command, "cpaSupType", entities.SupportedCsePersonAccount.Type1);
        db.SetNullableString(
          command, "cspSupNumber",
          entities.SupportedCsePersonAccount.CspNumber);
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        entities.AccrualInstructions.OtrType = db.GetString(reader, 0);
        entities.AccrualInstructions.OtyId = db.GetInt32(reader, 1);
        entities.AccrualInstructions.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.AccrualInstructions.CspNumber = db.GetString(reader, 3);
        entities.AccrualInstructions.CpaType = db.GetString(reader, 4);
        entities.AccrualInstructions.OtrGeneratedId = db.GetInt32(reader, 5);
        entities.AccrualInstructions.AsOfDt = db.GetDate(reader, 6);
        entities.AccrualInstructions.DiscontinueDt =
          db.GetNullableDate(reader, 7);
        entities.AccrualInstructions.Populated = true;
        CheckValid<AccrualInstructions>("OtrType",
          entities.AccrualInstructions.OtrType);
        CheckValid<AccrualInstructions>("CpaType",
          entities.AccrualInstructions.CpaType);
      });
  }

  private bool ReadAccrualInstructions2()
  {
    System.Diagnostics.Debug.
      Assert(entities.SupportedCsePersonAccount.Populated);
    System.Diagnostics.Debug.Assert(entities.ConcurrentObligation.Populated);
    entities.AccrualInstructions.Populated = false;

    return Read("ReadAccrualInstructions2",
      (db, command) =>
      {
        db.SetString(command, "obTrnTyp", import.HcOtDebtType.Type1);
        db.SetNullableString(
          command, "cpaSupType", entities.SupportedCsePersonAccount.Type1);
        db.SetNullableString(
          command, "cspSupNumber",
          entities.SupportedCsePersonAccount.CspNumber);
        db.SetInt32(
          command, "otyType", entities.ConcurrentObligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ConcurrentObligation.SystemGeneratedIdentifier);
        db.SetString(
          command, "cspNumber", entities.ConcurrentObligation.CspNumber);
        db.SetString(command, "cpaType", entities.ConcurrentObligation.CpaType);
      },
      (db, reader) =>
      {
        entities.AccrualInstructions.OtrType = db.GetString(reader, 0);
        entities.AccrualInstructions.OtyId = db.GetInt32(reader, 1);
        entities.AccrualInstructions.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.AccrualInstructions.CspNumber = db.GetString(reader, 3);
        entities.AccrualInstructions.CpaType = db.GetString(reader, 4);
        entities.AccrualInstructions.OtrGeneratedId = db.GetInt32(reader, 5);
        entities.AccrualInstructions.AsOfDt = db.GetDate(reader, 6);
        entities.AccrualInstructions.DiscontinueDt =
          db.GetNullableDate(reader, 7);
        entities.AccrualInstructions.Populated = true;
        CheckValid<AccrualInstructions>("OtrType",
          entities.AccrualInstructions.OtrType);
        CheckValid<AccrualInstructions>("CpaType",
          entities.AccrualInstructions.CpaType);
      });
  }

  private IEnumerable<bool> ReadAccrualInstructions3()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.AccrualInstructions.Populated = false;

    return ReadEach("ReadAccrualInstructions3",
      (db, command) =>
      {
        db.SetInt32(command, "otyId", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        entities.AccrualInstructions.OtrType = db.GetString(reader, 0);
        entities.AccrualInstructions.OtyId = db.GetInt32(reader, 1);
        entities.AccrualInstructions.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.AccrualInstructions.CspNumber = db.GetString(reader, 3);
        entities.AccrualInstructions.CpaType = db.GetString(reader, 4);
        entities.AccrualInstructions.OtrGeneratedId = db.GetInt32(reader, 5);
        entities.AccrualInstructions.AsOfDt = db.GetDate(reader, 6);
        entities.AccrualInstructions.DiscontinueDt =
          db.GetNullableDate(reader, 7);
        entities.AccrualInstructions.Populated = true;
        CheckValid<AccrualInstructions>("OtrType",
          entities.AccrualInstructions.OtrType);
        CheckValid<AccrualInstructions>("CpaType",
          entities.AccrualInstructions.CpaType);

        return true;
      });
  }

  private IEnumerable<bool> ReadAccrualInstructions4()
  {
    System.Diagnostics.Debug.Assert(entities.ConcurrentObligation.Populated);
    entities.AccrualInstructions.Populated = false;

    return ReadEach("ReadAccrualInstructions4",
      (db, command) =>
      {
        db.SetInt32(
          command, "otyId", entities.ConcurrentObligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ConcurrentObligation.SystemGeneratedIdentifier);
        db.SetString(
          command, "cspNumber", entities.ConcurrentObligation.CspNumber);
        db.SetString(command, "cpaType", entities.ConcurrentObligation.CpaType);
      },
      (db, reader) =>
      {
        entities.AccrualInstructions.OtrType = db.GetString(reader, 0);
        entities.AccrualInstructions.OtyId = db.GetInt32(reader, 1);
        entities.AccrualInstructions.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.AccrualInstructions.CspNumber = db.GetString(reader, 3);
        entities.AccrualInstructions.CpaType = db.GetString(reader, 4);
        entities.AccrualInstructions.OtrGeneratedId = db.GetInt32(reader, 5);
        entities.AccrualInstructions.AsOfDt = db.GetDate(reader, 6);
        entities.AccrualInstructions.DiscontinueDt =
          db.GetNullableDate(reader, 7);
        entities.AccrualInstructions.Populated = true;
        CheckValid<AccrualInstructions>("OtrType",
          entities.AccrualInstructions.OtrType);
        CheckValid<AccrualInstructions>("CpaType",
          entities.AccrualInstructions.CpaType);

        return true;
      });
  }

  private bool ReadCsePersonCsePersonAccount()
  {
    entities.SupportedCsePersonAccount.Populated = false;
    entities.SupportedCsePerson.Populated = false;

    return Read("ReadCsePersonCsePersonAccount",
      (db, command) =>
      {
        db.SetString(command, "type", import.HcCpaSupported.Type1);
        db.SetString(
          command, "cspNumber", export.Group.Item.SupportedCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.SupportedCsePerson.Number = db.GetString(reader, 0);
        entities.SupportedCsePersonAccount.CspNumber = db.GetString(reader, 0);
        entities.SupportedCsePersonAccount.Type1 = db.GetString(reader, 1);
        entities.SupportedCsePersonAccount.Populated = true;
        entities.SupportedCsePerson.Populated = true;
        CheckValid<CsePersonAccount>("Type1",
          entities.SupportedCsePersonAccount.Type1);
      });
  }

  private bool ReadObligation1()
  {
    entities.Obligation.Populated = false;

    return Read("ReadObligation1",
      (db, command) =>
      {
        db.
          SetInt32(command, "obId", import.Obligation.SystemGeneratedIdentifier);
          
        db.SetString(command, "cpaType", import.HcCpaObligor.Type1);
        db.SetString(command, "cspNumber", import.Obligor.Number);
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
        entities.Obligation.OtherStateAbbr = db.GetNullableString(reader, 4);
        entities.Obligation.Description = db.GetNullableString(reader, 5);
        entities.Obligation.HistoryInd = db.GetNullableString(reader, 6);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 7);
        entities.Obligation.PreConversionDebtNumber =
          db.GetNullableInt32(reader, 8);
        entities.Obligation.PreConversionCaseNumber =
          db.GetNullableString(reader, 9);
        entities.Obligation.CreatedBy = db.GetString(reader, 10);
        entities.Obligation.CreatedTmst = db.GetDateTime(reader, 11);
        entities.Obligation.LastUpdatedBy = db.GetNullableString(reader, 12);
        entities.Obligation.LastUpdateTmst = db.GetNullableDateTime(reader, 13);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 14);
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.Obligation.OrderTypeCode);
      });
  }

  private bool ReadObligation2()
  {
    entities.ConcurrentObligation.Populated = false;

    return Read("ReadObligation2",
      (db, command) =>
      {
        db.SetInt32(
          command, "obId",
          import.ConcurrentObligation.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", import.HcCpaObligor.Type1);
        db.SetString(command, "cspNumber", import.ConcurrentCsePerson.Number);
        db.SetInt32(
          command, "dtyGeneratedId",
          import.ObligationType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ConcurrentObligation.CpaType = db.GetString(reader, 0);
        entities.ConcurrentObligation.CspNumber = db.GetString(reader, 1);
        entities.ConcurrentObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ConcurrentObligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ConcurrentObligation.OtherStateAbbr =
          db.GetNullableString(reader, 4);
        entities.ConcurrentObligation.Description =
          db.GetNullableString(reader, 5);
        entities.ConcurrentObligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 6);
        entities.ConcurrentObligation.CreatedBy = db.GetString(reader, 7);
        entities.ConcurrentObligation.CreatedTmst = db.GetDateTime(reader, 8);
        entities.ConcurrentObligation.LastUpdatedBy =
          db.GetNullableString(reader, 9);
        entities.ConcurrentObligation.LastUpdateTmst =
          db.GetNullableDateTime(reader, 10);
        entities.ConcurrentObligation.OrderTypeCode = db.GetString(reader, 11);
        entities.ConcurrentObligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.ConcurrentObligation.CpaType);
          
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.ConcurrentObligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.ConcurrentObligation.OrderTypeCode);
      });
  }

  private bool ReadObligationPaymentSchedule1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.UpdateOnly.Populated = false;

    return Read("ReadObligationPaymentSchedule1",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "obgCspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "obgCpaType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        entities.UpdateOnly.OtyType = db.GetInt32(reader, 0);
        entities.UpdateOnly.ObgGeneratedId = db.GetInt32(reader, 1);
        entities.UpdateOnly.ObgCspNumber = db.GetString(reader, 2);
        entities.UpdateOnly.ObgCpaType = db.GetString(reader, 3);
        entities.UpdateOnly.StartDt = db.GetDate(reader, 4);
        entities.UpdateOnly.Amount = db.GetNullableDecimal(reader, 5);
        entities.UpdateOnly.EndDt = db.GetNullableDate(reader, 6);
        entities.UpdateOnly.CreatedBy = db.GetString(reader, 7);
        entities.UpdateOnly.CreatedTmst = db.GetDateTime(reader, 8);
        entities.UpdateOnly.LastUpdateBy = db.GetNullableString(reader, 9);
        entities.UpdateOnly.LastUpdateTmst = db.GetNullableDateTime(reader, 10);
        entities.UpdateOnly.FrequencyCode = db.GetString(reader, 11);
        entities.UpdateOnly.DayOfWeek = db.GetNullableInt32(reader, 12);
        entities.UpdateOnly.DayOfMonth1 = db.GetNullableInt32(reader, 13);
        entities.UpdateOnly.DayOfMonth2 = db.GetNullableInt32(reader, 14);
        entities.UpdateOnly.PeriodInd = db.GetNullableString(reader, 15);
        entities.UpdateOnly.Populated = true;
        CheckValid<ObligationPaymentSchedule>("ObgCpaType",
          entities.UpdateOnly.ObgCpaType);
        CheckValid<ObligationPaymentSchedule>("FrequencyCode",
          entities.UpdateOnly.FrequencyCode);
        CheckValid<ObligationPaymentSchedule>("PeriodInd",
          entities.UpdateOnly.PeriodInd);
      });
  }

  private bool ReadObligationPaymentSchedule2()
  {
    System.Diagnostics.Debug.Assert(entities.ConcurrentObligation.Populated);
    entities.UpdateOnly.Populated = false;

    return Read("ReadObligationPaymentSchedule2",
      (db, command) =>
      {
        db.SetInt32(
          command, "otyType", entities.ConcurrentObligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ConcurrentObligation.SystemGeneratedIdentifier);
        db.SetString(
          command, "obgCspNumber", entities.ConcurrentObligation.CspNumber);
        db.SetString(
          command, "obgCpaType", entities.ConcurrentObligation.CpaType);
      },
      (db, reader) =>
      {
        entities.UpdateOnly.OtyType = db.GetInt32(reader, 0);
        entities.UpdateOnly.ObgGeneratedId = db.GetInt32(reader, 1);
        entities.UpdateOnly.ObgCspNumber = db.GetString(reader, 2);
        entities.UpdateOnly.ObgCpaType = db.GetString(reader, 3);
        entities.UpdateOnly.StartDt = db.GetDate(reader, 4);
        entities.UpdateOnly.Amount = db.GetNullableDecimal(reader, 5);
        entities.UpdateOnly.EndDt = db.GetNullableDate(reader, 6);
        entities.UpdateOnly.CreatedBy = db.GetString(reader, 7);
        entities.UpdateOnly.CreatedTmst = db.GetDateTime(reader, 8);
        entities.UpdateOnly.LastUpdateBy = db.GetNullableString(reader, 9);
        entities.UpdateOnly.LastUpdateTmst = db.GetNullableDateTime(reader, 10);
        entities.UpdateOnly.FrequencyCode = db.GetString(reader, 11);
        entities.UpdateOnly.DayOfWeek = db.GetNullableInt32(reader, 12);
        entities.UpdateOnly.DayOfMonth1 = db.GetNullableInt32(reader, 13);
        entities.UpdateOnly.DayOfMonth2 = db.GetNullableInt32(reader, 14);
        entities.UpdateOnly.PeriodInd = db.GetNullableString(reader, 15);
        entities.UpdateOnly.Populated = true;
        CheckValid<ObligationPaymentSchedule>("ObgCpaType",
          entities.UpdateOnly.ObgCpaType);
        CheckValid<ObligationPaymentSchedule>("FrequencyCode",
          entities.UpdateOnly.FrequencyCode);
        CheckValid<ObligationPaymentSchedule>("PeriodInd",
          entities.UpdateOnly.PeriodInd);
      });
  }

  private bool ReadObligationPaymentSchedule3()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationPaymentSchedule.Populated = false;

    return Read("ReadObligationPaymentSchedule3",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "obgCspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "obgCpaType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        entities.ObligationPaymentSchedule.OtyType = db.GetInt32(reader, 0);
        entities.ObligationPaymentSchedule.ObgGeneratedId =
          db.GetInt32(reader, 1);
        entities.ObligationPaymentSchedule.ObgCspNumber =
          db.GetString(reader, 2);
        entities.ObligationPaymentSchedule.ObgCpaType = db.GetString(reader, 3);
        entities.ObligationPaymentSchedule.StartDt = db.GetDate(reader, 4);
        entities.ObligationPaymentSchedule.Amount =
          db.GetNullableDecimal(reader, 5);
        entities.ObligationPaymentSchedule.EndDt =
          db.GetNullableDate(reader, 6);
        entities.ObligationPaymentSchedule.LastUpdateBy =
          db.GetNullableString(reader, 7);
        entities.ObligationPaymentSchedule.LastUpdateTmst =
          db.GetNullableDateTime(reader, 8);
        entities.ObligationPaymentSchedule.FrequencyCode =
          db.GetString(reader, 9);
        entities.ObligationPaymentSchedule.DayOfWeek =
          db.GetNullableInt32(reader, 10);
        entities.ObligationPaymentSchedule.DayOfMonth1 =
          db.GetNullableInt32(reader, 11);
        entities.ObligationPaymentSchedule.DayOfMonth2 =
          db.GetNullableInt32(reader, 12);
        entities.ObligationPaymentSchedule.PeriodInd =
          db.GetNullableString(reader, 13);
        entities.ObligationPaymentSchedule.Populated = true;
        CheckValid<ObligationPaymentSchedule>("ObgCpaType",
          entities.ObligationPaymentSchedule.ObgCpaType);
        CheckValid<ObligationPaymentSchedule>("FrequencyCode",
          entities.ObligationPaymentSchedule.FrequencyCode);
        CheckValid<ObligationPaymentSchedule>("PeriodInd",
          entities.ObligationPaymentSchedule.PeriodInd);
      });
  }

  private bool ReadObligationPaymentSchedule4()
  {
    System.Diagnostics.Debug.Assert(entities.ConcurrentObligation.Populated);
    entities.ConcurrentObligationPaymentSchedule.Populated = false;

    return Read("ReadObligationPaymentSchedule4",
      (db, command) =>
      {
        db.SetInt32(
          command, "otyType", entities.ConcurrentObligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ConcurrentObligation.SystemGeneratedIdentifier);
        db.SetString(
          command, "obgCspNumber", entities.ConcurrentObligation.CspNumber);
        db.SetString(
          command, "obgCpaType", entities.ConcurrentObligation.CpaType);
      },
      (db, reader) =>
      {
        entities.ConcurrentObligationPaymentSchedule.OtyType =
          db.GetInt32(reader, 0);
        entities.ConcurrentObligationPaymentSchedule.ObgGeneratedId =
          db.GetInt32(reader, 1);
        entities.ConcurrentObligationPaymentSchedule.ObgCspNumber =
          db.GetString(reader, 2);
        entities.ConcurrentObligationPaymentSchedule.ObgCpaType =
          db.GetString(reader, 3);
        entities.ConcurrentObligationPaymentSchedule.StartDt =
          db.GetDate(reader, 4);
        entities.ConcurrentObligationPaymentSchedule.Amount =
          db.GetNullableDecimal(reader, 5);
        entities.ConcurrentObligationPaymentSchedule.EndDt =
          db.GetNullableDate(reader, 6);
        entities.ConcurrentObligationPaymentSchedule.LastUpdateBy =
          db.GetNullableString(reader, 7);
        entities.ConcurrentObligationPaymentSchedule.LastUpdateTmst =
          db.GetNullableDateTime(reader, 8);
        entities.ConcurrentObligationPaymentSchedule.FrequencyCode =
          db.GetString(reader, 9);
        entities.ConcurrentObligationPaymentSchedule.DayOfWeek =
          db.GetNullableInt32(reader, 10);
        entities.ConcurrentObligationPaymentSchedule.DayOfMonth1 =
          db.GetNullableInt32(reader, 11);
        entities.ConcurrentObligationPaymentSchedule.DayOfMonth2 =
          db.GetNullableInt32(reader, 12);
        entities.ConcurrentObligationPaymentSchedule.PeriodInd =
          db.GetNullableString(reader, 13);
        entities.ConcurrentObligationPaymentSchedule.Populated = true;
        CheckValid<ObligationPaymentSchedule>("ObgCpaType",
          entities.ConcurrentObligationPaymentSchedule.ObgCpaType);
        CheckValid<ObligationPaymentSchedule>("FrequencyCode",
          entities.ConcurrentObligationPaymentSchedule.FrequencyCode);
        CheckValid<ObligationPaymentSchedule>("PeriodInd",
          entities.ConcurrentObligationPaymentSchedule.PeriodInd);
      });
  }

  private bool ReadObligationPaymentSchedule5()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.UpdateOnly.Populated = false;

    return Read("ReadObligationPaymentSchedule5",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "obgCspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "obgCpaType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        entities.UpdateOnly.OtyType = db.GetInt32(reader, 0);
        entities.UpdateOnly.ObgGeneratedId = db.GetInt32(reader, 1);
        entities.UpdateOnly.ObgCspNumber = db.GetString(reader, 2);
        entities.UpdateOnly.ObgCpaType = db.GetString(reader, 3);
        entities.UpdateOnly.StartDt = db.GetDate(reader, 4);
        entities.UpdateOnly.Amount = db.GetNullableDecimal(reader, 5);
        entities.UpdateOnly.EndDt = db.GetNullableDate(reader, 6);
        entities.UpdateOnly.CreatedBy = db.GetString(reader, 7);
        entities.UpdateOnly.CreatedTmst = db.GetDateTime(reader, 8);
        entities.UpdateOnly.LastUpdateBy = db.GetNullableString(reader, 9);
        entities.UpdateOnly.LastUpdateTmst = db.GetNullableDateTime(reader, 10);
        entities.UpdateOnly.FrequencyCode = db.GetString(reader, 11);
        entities.UpdateOnly.DayOfWeek = db.GetNullableInt32(reader, 12);
        entities.UpdateOnly.DayOfMonth1 = db.GetNullableInt32(reader, 13);
        entities.UpdateOnly.DayOfMonth2 = db.GetNullableInt32(reader, 14);
        entities.UpdateOnly.PeriodInd = db.GetNullableString(reader, 15);
        entities.UpdateOnly.Populated = true;
        CheckValid<ObligationPaymentSchedule>("ObgCpaType",
          entities.UpdateOnly.ObgCpaType);
        CheckValid<ObligationPaymentSchedule>("FrequencyCode",
          entities.UpdateOnly.FrequencyCode);
        CheckValid<ObligationPaymentSchedule>("PeriodInd",
          entities.UpdateOnly.PeriodInd);
      });
  }

  private bool ReadObligationPaymentSchedule6()
  {
    System.Diagnostics.Debug.Assert(entities.ConcurrentObligation.Populated);
    entities.UpdateOnly.Populated = false;

    return Read("ReadObligationPaymentSchedule6",
      (db, command) =>
      {
        db.SetInt32(
          command, "otyType", entities.ConcurrentObligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ConcurrentObligation.SystemGeneratedIdentifier);
        db.SetString(
          command, "obgCspNumber", entities.ConcurrentObligation.CspNumber);
        db.SetString(
          command, "obgCpaType", entities.ConcurrentObligation.CpaType);
      },
      (db, reader) =>
      {
        entities.UpdateOnly.OtyType = db.GetInt32(reader, 0);
        entities.UpdateOnly.ObgGeneratedId = db.GetInt32(reader, 1);
        entities.UpdateOnly.ObgCspNumber = db.GetString(reader, 2);
        entities.UpdateOnly.ObgCpaType = db.GetString(reader, 3);
        entities.UpdateOnly.StartDt = db.GetDate(reader, 4);
        entities.UpdateOnly.Amount = db.GetNullableDecimal(reader, 5);
        entities.UpdateOnly.EndDt = db.GetNullableDate(reader, 6);
        entities.UpdateOnly.CreatedBy = db.GetString(reader, 7);
        entities.UpdateOnly.CreatedTmst = db.GetDateTime(reader, 8);
        entities.UpdateOnly.LastUpdateBy = db.GetNullableString(reader, 9);
        entities.UpdateOnly.LastUpdateTmst = db.GetNullableDateTime(reader, 10);
        entities.UpdateOnly.FrequencyCode = db.GetString(reader, 11);
        entities.UpdateOnly.DayOfWeek = db.GetNullableInt32(reader, 12);
        entities.UpdateOnly.DayOfMonth1 = db.GetNullableInt32(reader, 13);
        entities.UpdateOnly.DayOfMonth2 = db.GetNullableInt32(reader, 14);
        entities.UpdateOnly.PeriodInd = db.GetNullableString(reader, 15);
        entities.UpdateOnly.Populated = true;
        CheckValid<ObligationPaymentSchedule>("ObgCpaType",
          entities.UpdateOnly.ObgCpaType);
        CheckValid<ObligationPaymentSchedule>("FrequencyCode",
          entities.UpdateOnly.FrequencyCode);
        CheckValid<ObligationPaymentSchedule>("PeriodInd",
          entities.UpdateOnly.PeriodInd);
      });
  }

  private void UpdateAccrualInstructions1()
  {
    System.Diagnostics.Debug.Assert(entities.AccrualInstructions.Populated);

    var asOfDt = import.ObligationPaymentSchedule.StartDt;

    entities.AccrualInstructions.Populated = false;
    Update("UpdateAccrualInstructions1",
      (db, command) =>
      {
        db.SetDate(command, "asOfDt", asOfDt);
        db.SetString(command, "otrType", entities.AccrualInstructions.OtrType);
        db.SetInt32(command, "otyId", entities.AccrualInstructions.OtyId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.AccrualInstructions.ObgGeneratedId);
        db.SetString(
          command, "cspNumber", entities.AccrualInstructions.CspNumber);
        db.SetString(command, "cpaType", entities.AccrualInstructions.CpaType);
        db.SetInt32(
          command, "otrGeneratedId",
          entities.AccrualInstructions.OtrGeneratedId);
      });

    entities.AccrualInstructions.AsOfDt = asOfDt;
    entities.AccrualInstructions.Populated = true;
  }

  private void UpdateAccrualInstructions2()
  {
    System.Diagnostics.Debug.Assert(entities.AccrualInstructions.Populated);

    var discontinueDt = export.Group.Item.AccrualInstructions.DiscontinueDt;

    entities.AccrualInstructions.Populated = false;
    Update("UpdateAccrualInstructions2",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDt", discontinueDt);
        db.SetString(command, "otrType", entities.AccrualInstructions.OtrType);
        db.SetInt32(command, "otyId", entities.AccrualInstructions.OtyId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.AccrualInstructions.ObgGeneratedId);
        db.SetString(
          command, "cspNumber", entities.AccrualInstructions.CspNumber);
        db.SetString(command, "cpaType", entities.AccrualInstructions.CpaType);
        db.SetInt32(
          command, "otrGeneratedId",
          entities.AccrualInstructions.OtrGeneratedId);
      });

    entities.AccrualInstructions.DiscontinueDt = discontinueDt;
    entities.AccrualInstructions.Populated = true;
  }

  private void UpdateAccrualInstructions3()
  {
    System.Diagnostics.Debug.Assert(entities.AccrualInstructions.Populated);

    var discontinueDt = local.AccrualInstructions.DiscontinueDt;

    entities.AccrualInstructions.Populated = false;
    Update("UpdateAccrualInstructions3",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDt", discontinueDt);
        db.SetString(command, "otrType", entities.AccrualInstructions.OtrType);
        db.SetInt32(command, "otyId", entities.AccrualInstructions.OtyId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.AccrualInstructions.ObgGeneratedId);
        db.SetString(
          command, "cspNumber", entities.AccrualInstructions.CspNumber);
        db.SetString(command, "cpaType", entities.AccrualInstructions.CpaType);
        db.SetInt32(
          command, "otrGeneratedId",
          entities.AccrualInstructions.OtrGeneratedId);
      });

    entities.AccrualInstructions.DiscontinueDt = discontinueDt;
    entities.AccrualInstructions.Populated = true;
  }

  private void UpdateAccrualInstructions4()
  {
    System.Diagnostics.Debug.Assert(entities.AccrualInstructions.Populated);

    var discontinueDt = import.ObligationPaymentSchedule.EndDt;

    entities.AccrualInstructions.Populated = false;
    Update("UpdateAccrualInstructions4",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDt", discontinueDt);
        db.SetString(command, "otrType", entities.AccrualInstructions.OtrType);
        db.SetInt32(command, "otyId", entities.AccrualInstructions.OtyId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.AccrualInstructions.ObgGeneratedId);
        db.SetString(
          command, "cspNumber", entities.AccrualInstructions.CspNumber);
        db.SetString(command, "cpaType", entities.AccrualInstructions.CpaType);
        db.SetInt32(
          command, "otrGeneratedId",
          entities.AccrualInstructions.OtrGeneratedId);
      });

    entities.AccrualInstructions.DiscontinueDt = discontinueDt;
    entities.AccrualInstructions.Populated = true;
  }

  private void UpdateObligation1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);

    var otherStateAbbr = import.Obligation.OtherStateAbbr ?? "";
    var description = import.Obligation.Description ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdateTmst = import.CurrentDate.Timestamp;
    var orderTypeCode = import.Obligation.OrderTypeCode;

    CheckValid<Obligation>("OrderTypeCode", orderTypeCode);
    entities.Obligation.Populated = false;
    Update("UpdateObligation1",
      (db, command) =>
      {
        db.SetNullableString(command, "otherStateAbbr", otherStateAbbr);
        db.SetNullableString(command, "obDsc", description);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdateTmst", lastUpdateTmst);
        db.SetString(command, "ordTypCd", orderTypeCode);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetInt32(
          command, "obId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "dtyGeneratedId", entities.Obligation.DtyGeneratedId);
      });

    entities.Obligation.OtherStateAbbr = otherStateAbbr;
    entities.Obligation.Description = description;
    entities.Obligation.LastUpdatedBy = lastUpdatedBy;
    entities.Obligation.LastUpdateTmst = lastUpdateTmst;
    entities.Obligation.OrderTypeCode = orderTypeCode;
    entities.Obligation.Populated = true;
  }

  private void UpdateObligation2()
  {
    System.Diagnostics.Debug.Assert(entities.ConcurrentObligation.Populated);

    var otherStateAbbr = import.Obligation.OtherStateAbbr ?? "";
    var description = import.Obligation.Description ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdateTmst = import.CurrentDate.Timestamp;
    var orderTypeCode = import.Obligation.OrderTypeCode;

    CheckValid<Obligation>("OrderTypeCode", orderTypeCode);
    entities.ConcurrentObligation.Populated = false;
    Update("UpdateObligation2",
      (db, command) =>
      {
        db.SetNullableString(command, "otherStateAbbr", otherStateAbbr);
        db.SetNullableString(command, "obDsc", description);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdateTmst", lastUpdateTmst);
        db.SetString(command, "ordTypCd", orderTypeCode);
        db.SetString(command, "cpaType", entities.ConcurrentObligation.CpaType);
        db.SetString(
          command, "cspNumber", entities.ConcurrentObligation.CspNumber);
        db.SetInt32(
          command, "obId",
          entities.ConcurrentObligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "dtyGeneratedId",
          entities.ConcurrentObligation.DtyGeneratedId);
      });

    entities.ConcurrentObligation.OtherStateAbbr = otherStateAbbr;
    entities.ConcurrentObligation.Description = description;
    entities.ConcurrentObligation.LastUpdatedBy = lastUpdatedBy;
    entities.ConcurrentObligation.LastUpdateTmst = lastUpdateTmst;
    entities.ConcurrentObligation.OrderTypeCode = orderTypeCode;
    entities.ConcurrentObligation.Populated = true;
  }

  private void UpdateObligationPaymentSchedule()
  {
    System.Diagnostics.Debug.Assert(entities.UpdateOnly.Populated);

    var endDt = import.Discontinue.Date;
    var lastUpdateBy = global.UserId;
    var lastUpdateTmst = import.CurrentDate.Timestamp;

    entities.UpdateOnly.Populated = false;
    Update("UpdateObligationPaymentSchedule",
      (db, command) =>
      {
        db.SetNullableDate(command, "endDt", endDt);
        db.SetNullableString(command, "lastUpdateBy", lastUpdateBy);
        db.SetNullableDateTime(command, "lastUpdateTmst", lastUpdateTmst);
        db.SetInt32(command, "otyType", entities.UpdateOnly.OtyType);
        db.SetInt32(
          command, "obgGeneratedId", entities.UpdateOnly.ObgGeneratedId);
        db.SetString(command, "obgCspNumber", entities.UpdateOnly.ObgCspNumber);
        db.SetString(command, "obgCpaType", entities.UpdateOnly.ObgCpaType);
        db.SetDate(
          command, "startDt", entities.UpdateOnly.StartDt.GetValueOrDefault());
      });

    entities.UpdateOnly.EndDt = endDt;
    entities.UpdateOnly.LastUpdateBy = lastUpdateBy;
    entities.UpdateOnly.LastUpdateTmst = lastUpdateTmst;
    entities.UpdateOnly.Populated = true;
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
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of Program.
      /// </summary>
      [JsonPropertyName("program")]
      public Program Program
      {
        get => program ??= new();
        set => program = value;
      }

      /// <summary>
      /// A value of Sel.
      /// </summary>
      [JsonPropertyName("sel")]
      public Common Sel
      {
        get => sel ??= new();
        set => sel = value;
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
      /// A value of Prompt.
      /// </summary>
      [JsonPropertyName("prompt")]
      public Common Prompt
      {
        get => prompt ??= new();
        set => prompt = value;
      }

      /// <summary>
      /// A value of SupportedCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("supportedCsePersonsWorkSet")]
      public CsePersonsWorkSet SupportedCsePersonsWorkSet
      {
        get => supportedCsePersonsWorkSet ??= new();
        set => supportedCsePersonsWorkSet = value;
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
      /// A value of ProratePercentage.
      /// </summary>
      [JsonPropertyName("proratePercentage")]
      public Common ProratePercentage
      {
        get => proratePercentage ??= new();
        set => proratePercentage = value;
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
      /// A value of ProgramScreenAttributes.
      /// </summary>
      [JsonPropertyName("programScreenAttributes")]
      public ProgramScreenAttributes ProgramScreenAttributes
      {
        get => programScreenAttributes ??= new();
        set => programScreenAttributes = value;
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
      /// A value of AccrualInstructions.
      /// </summary>
      [JsonPropertyName("accrualInstructions")]
      public AccrualInstructions AccrualInstructions
      {
        get => accrualInstructions ??= new();
        set => accrualInstructions = value;
      }

      /// <summary>
      /// A value of HiddenConcurrent.
      /// </summary>
      [JsonPropertyName("hiddenConcurrent")]
      public ObligationTransaction HiddenConcurrent
      {
        get => hiddenConcurrent ??= new();
        set => hiddenConcurrent = value;
      }

      /// <summary>
      /// A value of Previous.
      /// </summary>
      [JsonPropertyName("previous")]
      public ObligationTransaction Previous
      {
        get => previous ??= new();
        set => previous = value;
      }

      /// <summary>
      /// A value of SuspendAccrual.
      /// </summary>
      [JsonPropertyName("suspendAccrual")]
      public CsePersonsWorkSet SuspendAccrual
      {
        get => suspendAccrual ??= new();
        set => suspendAccrual = value;
      }

      /// <summary>
      /// A value of Hidden.
      /// </summary>
      [JsonPropertyName("hidden")]
      public AccrualInstructions Hidden
      {
        get => hidden ??= new();
        set => hidden = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private Program program;
      private Common sel;
      private CsePerson supportedCsePerson;
      private Common prompt;
      private CsePersonsWorkSet supportedCsePersonsWorkSet;
      private Case1 case1;
      private Common proratePercentage;
      private ObligationTransaction obligationTransaction;
      private ProgramScreenAttributes programScreenAttributes;
      private ServiceProvider serviceProvider;
      private AccrualInstructions accrualInstructions;
      private ObligationTransaction hiddenConcurrent;
      private ObligationTransaction previous;
      private CsePersonsWorkSet suspendAccrual;
      private AccrualInstructions hidden;
    }

    /// <summary>
    /// A value of HcOtDebtType.
    /// </summary>
    [JsonPropertyName("hcOtDebtType")]
    public ObligationTransaction HcOtDebtType
    {
      get => hcOtDebtType ??= new();
      set => hcOtDebtType = value;
    }

    /// <summary>
    /// A value of HcCpaSupported.
    /// </summary>
    [JsonPropertyName("hcCpaSupported")]
    public CsePersonAccount HcCpaSupported
    {
      get => hcCpaSupported ??= new();
      set => hcCpaSupported = value;
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
    /// A value of Maximum.
    /// </summary>
    [JsonPropertyName("maximum")]
    public DateWorkArea Maximum
    {
      get => maximum ??= new();
      set => maximum = value;
    }

    /// <summary>
    /// A value of UpdateInterstateInfo.
    /// </summary>
    [JsonPropertyName("updateInterstateInfo")]
    public Common UpdateInterstateInfo
    {
      get => updateInterstateInfo ??= new();
      set => updateInterstateInfo = value;
    }

    /// <summary>
    /// A value of AltAdd.
    /// </summary>
    [JsonPropertyName("altAdd")]
    public CsePersonsWorkSet AltAdd
    {
      get => altAdd ??= new();
      set => altAdd = value;
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
    /// A value of ConcurrentCsePerson.
    /// </summary>
    [JsonPropertyName("concurrentCsePerson")]
    public CsePerson ConcurrentCsePerson
    {
      get => concurrentCsePerson ??= new();
      set => concurrentCsePerson = value;
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
    /// A value of ConcurrentObligation.
    /// </summary>
    [JsonPropertyName("concurrentObligation")]
    public Obligation ConcurrentObligation
    {
      get => concurrentObligation ??= new();
      set => concurrentObligation = value;
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
    /// A value of CurrentDate.
    /// </summary>
    [JsonPropertyName("currentDate")]
    public DateWorkArea CurrentDate
    {
      get => currentDate ??= new();
      set => currentDate = value;
    }

    /// <summary>
    /// A value of Discontinue.
    /// </summary>
    [JsonPropertyName("discontinue")]
    public DateWorkArea Discontinue
    {
      get => discontinue ??= new();
      set => discontinue = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
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
    public InterstateRequest New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of Old.
    /// </summary>
    [JsonPropertyName("old")]
    public InterstateRequest Old
    {
      get => old ??= new();
      set => old = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    private ObligationTransaction hcOtDebtType;
    private CsePersonAccount hcCpaSupported;
    private CsePersonAccount hcCpaObligor;
    private DateWorkArea maximum;
    private Common updateInterstateInfo;
    private CsePersonsWorkSet altAdd;
    private ObligationType obligationType;
    private CsePerson concurrentCsePerson;
    private CsePerson obligor;
    private Obligation obligation;
    private Obligation concurrentObligation;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private DateWorkArea currentDate;
    private DateWorkArea discontinue;
    private Array<GroupGroup> group;
    private Case1 case1;
    private InterstateRequest new1;
    private InterstateRequest old;
    private LegalActionDetail legalActionDetail;
    private LegalAction legalAction;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of Program.
      /// </summary>
      [JsonPropertyName("program")]
      public Program Program
      {
        get => program ??= new();
        set => program = value;
      }

      /// <summary>
      /// A value of Sel.
      /// </summary>
      [JsonPropertyName("sel")]
      public Common Sel
      {
        get => sel ??= new();
        set => sel = value;
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
      /// A value of Prompt.
      /// </summary>
      [JsonPropertyName("prompt")]
      public Common Prompt
      {
        get => prompt ??= new();
        set => prompt = value;
      }

      /// <summary>
      /// A value of SupportedCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("supportedCsePersonsWorkSet")]
      public CsePersonsWorkSet SupportedCsePersonsWorkSet
      {
        get => supportedCsePersonsWorkSet ??= new();
        set => supportedCsePersonsWorkSet = value;
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
      /// A value of ProratePercentage.
      /// </summary>
      [JsonPropertyName("proratePercentage")]
      public Common ProratePercentage
      {
        get => proratePercentage ??= new();
        set => proratePercentage = value;
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
      /// A value of ProgramScreenAttributes.
      /// </summary>
      [JsonPropertyName("programScreenAttributes")]
      public ProgramScreenAttributes ProgramScreenAttributes
      {
        get => programScreenAttributes ??= new();
        set => programScreenAttributes = value;
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
      /// A value of AccrualInstructions.
      /// </summary>
      [JsonPropertyName("accrualInstructions")]
      public AccrualInstructions AccrualInstructions
      {
        get => accrualInstructions ??= new();
        set => accrualInstructions = value;
      }

      /// <summary>
      /// A value of HiddenConcurrent.
      /// </summary>
      [JsonPropertyName("hiddenConcurrent")]
      public ObligationTransaction HiddenConcurrent
      {
        get => hiddenConcurrent ??= new();
        set => hiddenConcurrent = value;
      }

      /// <summary>
      /// A value of Previous.
      /// </summary>
      [JsonPropertyName("previous")]
      public ObligationTransaction Previous
      {
        get => previous ??= new();
        set => previous = value;
      }

      /// <summary>
      /// A value of SuspendAccrual.
      /// </summary>
      [JsonPropertyName("suspendAccrual")]
      public CsePersonsWorkSet SuspendAccrual
      {
        get => suspendAccrual ??= new();
        set => suspendAccrual = value;
      }

      /// <summary>
      /// A value of Hidden.
      /// </summary>
      [JsonPropertyName("hidden")]
      public AccrualInstructions Hidden
      {
        get => hidden ??= new();
        set => hidden = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private Program program;
      private Common sel;
      private CsePerson supportedCsePerson;
      private Common prompt;
      private CsePersonsWorkSet supportedCsePersonsWorkSet;
      private Case1 case1;
      private Common proratePercentage;
      private ObligationTransaction obligationTransaction;
      private ProgramScreenAttributes programScreenAttributes;
      private ServiceProvider serviceProvider;
      private AccrualInstructions accrualInstructions;
      private ObligationTransaction hiddenConcurrent;
      private ObligationTransaction previous;
      private CsePersonsWorkSet suspendAccrual;
      private AccrualInstructions hidden;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    /// <summary>
    /// A value of AltBillLocError.
    /// </summary>
    [JsonPropertyName("altBillLocError")]
    public Common AltBillLocError
    {
      get => altBillLocError ??= new();
      set => altBillLocError = value;
    }

    private Array<GroupGroup> group;
    private Common altBillLocError;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of HardcodeConcurr.
    /// </summary>
    [JsonPropertyName("hardcodeConcurr")]
    public ObligationTransactionRlnRsn HardcodeConcurr
    {
      get => hardcodeConcurr ??= new();
      set => hardcodeConcurr = value;
    }

    /// <summary>
    /// A value of ZeroDate.
    /// </summary>
    [JsonPropertyName("zeroDate")]
    public DateWorkArea ZeroDate
    {
      get => zeroDate ??= new();
      set => zeroDate = value;
    }

    private AccrualInstructions accrualInstructions;
    private ObligationTransactionRlnRsn hardcodeConcurr;
    private DateWorkArea zeroDate;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ConcurrentObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("concurrentObligationPaymentSchedule")]
    public ObligationPaymentSchedule ConcurrentObligationPaymentSchedule
    {
      get => concurrentObligationPaymentSchedule ??= new();
      set => concurrentObligationPaymentSchedule = value;
    }

    /// <summary>
    /// A value of UpdateOnly.
    /// </summary>
    [JsonPropertyName("updateOnly")]
    public ObligationPaymentSchedule UpdateOnly
    {
      get => updateOnly ??= new();
      set => updateOnly = value;
    }

    /// <summary>
    /// A value of AltAdd.
    /// </summary>
    [JsonPropertyName("altAdd")]
    public CsePerson AltAdd
    {
      get => altAdd ??= new();
      set => altAdd = value;
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
    /// A value of SupportedCsePerson.
    /// </summary>
    [JsonPropertyName("supportedCsePerson")]
    public CsePerson SupportedCsePerson
    {
      get => supportedCsePerson ??= new();
      set => supportedCsePerson = value;
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of ConcurrentObligation.
    /// </summary>
    [JsonPropertyName("concurrentObligation")]
    public Obligation ConcurrentObligation
    {
      get => concurrentObligation ??= new();
      set => concurrentObligation = value;
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
    /// A value of AccrualInstructions.
    /// </summary>
    [JsonPropertyName("accrualInstructions")]
    public AccrualInstructions AccrualInstructions
    {
      get => accrualInstructions ??= new();
      set => accrualInstructions = value;
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

    private ObligationPaymentSchedule concurrentObligationPaymentSchedule;
    private ObligationPaymentSchedule updateOnly;
    private CsePerson altAdd;
    private CsePersonAccount supportedCsePersonAccount;
    private CsePerson supportedCsePerson;
    private ObligationType obligationType;
    private CsePerson obligor;
    private CsePersonAccount csePersonAccount;
    private Obligation obligation;
    private Obligation concurrentObligation;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private AccrualInstructions accrualInstructions;
    private ObligationTransaction obligationTransaction;
    private LegalAction legalAction;
    private LegalActionDetail legalActionDetail;
  }
#endregion
}
