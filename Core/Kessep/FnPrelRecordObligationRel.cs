// Program: FN_PREL_RECORD_OBLIGATION_REL, ID: 372047675, model: 746.
// Short name: SWEPRELP
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_PREL_RECORD_OBLIGATION_REL.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnPrelRecordObligationRel: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_PREL_RECORD_OBLIGATION_REL program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnPrelRecordObligationRel(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnPrelRecordObligationRel.
  /// </summary>
  public FnPrelRecordObligationRel(IContext context, Import import,
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
    // ----------------------------------------------------------------
    // Date     Developer Name   Request#      Description
    // 07/16/96  Rick Delgado                  New Development
    // 09/14/96  Regan Welborn                 Testing and such
    // 04/28/97  A.Kinney		        Changed Current_date
    // 05/16/97  R. Marchman                   Add new security and next tran,
    // 07/16/97  R. Marchman                   Changed the exit state under 
    // Disassoc command.
    // 07/28/97  R. Marchman                   Fixed the Obligation Type read 
    // statement for the secondary obligation so that the Frequency would
    // display.
    // 07/28/97  R. Marchman   	        Added a check when the prompt is used to
    // see if nothing was selected and make sure that what data was already
    // displayed remains displayed.
    // 08/08/97  R. Marchman		        Added logic to move the new Obligation Rln
    // to export when two obligations are related.
    // 08/12/97  R. Marchman Added Day of Week to the frequency display.
    // 01/21/98  Adwait Phadnis                Fixed Accrual problem.
    // 10/27/98  G Sharp         Phase 2      1. Added Obligation ID to screen.
    //                                        
    // 2. Added hardcode for obligation_type
    // classification of voluntary and
    // recover.
    //                                        
    // 3. Added code to edit for NOT relating
    // voluntary and recover obligation.
    // 4/15/99 - bud adams  -  Read properties set.
    // 12/16/99 - Bud Adams  -  PR# 82855: removed opening_bal_ind
    //   from ob-tran view so it could be deleted from the model.
    // --------------------------------------------------------------
    // =================================================
    // 12/21/99 - b adams  -  PR# 83302: removed attributes
    //   Preconversion_ISN & Preconversion_Receipt_Number
    //   from the Obligation_Transaction (debt) view.  They are not
    //   used anyplace in KESSEP and are being deleted.
    // =================================================
    // ------------------------------------------------------------------------------
    // 11/07/2001 K Doshi PR131585
    // Fix screen help Id problem.
    // -----------------------------------------------------------------------------
    UseFnHardcodedDebtDistribution();
    local.Current.Date = Now().Date;
    local.Current.Timestamp = Now();
    ExitState = "ACO_NN0000_ALL_OK";

    // **** begin group A ****
    export.Hidden.Assign(import.Hidden);

    // **** end   group A ****
    export.CsePerson.Number = import.CsePerson.Number;

    if (Equal(global.Command, "CLEAR"))
    {
      export.PriPayor.PromptField = "+";
      export.SecPayor.PromptField = "+";

      return;
    }

    export.RelEst.Date = import.RelEst.Date;
    export.ObligationRln.Assign(import.ObligationRln);
    export.AccrualSuspension.Assign(import.AccrualSuspension);
    export.CsePersonAccount.Type1 = import.CsePersonAccount.Type1;
    MoveCsePersonsWorkSet(import.CsePersonsWorkSet, export.CsePersonsWorkSet);
    MoveAccrualInstructions(import.PriAccrualInstructions,
      export.PriAccrualInstructions);
    MoveAccrualInstructions(import.SecAccrualInstructions,
      export.SecAccrualInstructions);
    export.PriScreenOwedAmounts.Assign(import.PriScreenOwedAmounts);
    export.SecScreenOwedAmounts.Assign(import.SecScreenOwedAmounts);
    export.PriAccrual.Amount = import.PriAccrual.Amount;
    export.SecAccrual.Amount = import.SecAccrual.Amount;
    export.PriAccrualDue.Date = import.PriAccrualDue.Date;
    export.SecAccrualDue.Date = import.SecAccrualDue.Date;
    export.PriPayor.PromptField = import.PriPayor.PromptField;
    export.SecPayor.PromptField = import.SecPayor.PromptField;
    export.PriCsePerson.Number = import.PriCsePerson.Number;
    export.PriCsePersonAccount.Type1 = import.PriCsePersonAccount.Type1;
    MoveCsePersonsWorkSet(import.PriCsePersonsWorkSet,
      export.PriCsePersonsWorkSet);
    MoveDebtDetail(import.PriDebtDetail, export.PriDebtDetail);
    export.PriFrequencyWorkSet.Assign(import.PriFrequencyWorkSet);
    export.PriLegalAction.StandardNumber = import.PriLegalAction.StandardNumber;
    MoveObligation(import.PriObligation, export.PriObligation);
    export.PriObligationType.Assign(import.PriObligationType);
    export.SecCsePerson.Number = import.SecCsePerson.Number;
    export.SecCsePersonAccount.Type1 = import.SecCsePersonAccount.Type1;
    MoveCsePersonsWorkSet(import.SecCsePersonsWorkSet,
      export.SecCsePersonsWorkSet);
    MoveDebtDetail(import.SecDebtDetail, export.SecDebtDetail);
    export.SecFrequencyWorkSet.Assign(import.SecFrequencyWorkSet);
    export.SecLegalAction.StandardNumber = import.SecLegalAction.StandardNumber;
    MoveObligation(import.SecObligation, export.SecObligation);
    export.SecObligationType.Assign(import.SecObligationType);
    export.PriScreenOwedAmounts.Assign(import.ScreenOwedAmounts);
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveLegalAction(import.LegalAction, export.LegalAction);
    export.AccrualSuspension.Assign(import.AccrualSuspension);
    export.CsePerson.Number = import.CsePerson.Number;
    export.CsePersonAccount.Type1 = import.CsePersonAccount.Type1;
    MoveCsePersonsWorkSet(import.CsePersonsWorkSet, export.CsePersonsWorkSet);
    export.Obligation.SystemGeneratedIdentifier =
      import.Obligation.SystemGeneratedIdentifier;
    export.ObligationType.Assign(import.ObligationType);

    // **** begin group B ****
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      // ****
      // ****
      // this is where you would set the local next_tran_info attributes to the 
      // import view attributes for the data to be passed to the next
      // transaction
      // ****
      // ****
      UseScCabNextTranPut();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (Equal(global.Command, "XXNEXTXX"))
      {
        // ****
        // this is where you set your export value to the export hidden next 
        // tran values if the user is comming into this procedure on a next tran
        // action.
        // ****
        UseScCabNextTranGet();

        // *** There is NO information that can be passed by next tran to this 
        // PrAD. And the user would like a message to be displayed saying so.
        // Code added by G Sharp. ***
        ExitState = "FN0000_MUST_SEL_OB_FIRST";

        return;
      }

      if (Equal(global.Command, "XXFMMENU"))
      {
        // ****
        // this is where you set your command to do whatever is necessary to do 
        // on a flow from the menu, maybe just escape....
        // ****
        // You should get this information from the Dialog Flow Diagram.  It is 
        // the SEND CMD on the propertis for a Transfer from one  procedure to
        // another.
        // *** the statement would read COMMAND IS display   *****
        // *** if the dialog flow property was display first, just add an escape
        // completely out of the procedure  ****
        global.Command = "DISPLAY";
      }
    }
    else
    {
      return;
    }

    // **** end   group B ****
    // **** begin group C ****
    // to validate action level security
    // =================================================
    // 2/5/00 - bud adams - PR# 86702: Removed 'or command =
    //   disassoc' from test.  This command is temporarily being
    //   disabled.
    // =================================================
    if (Equal(global.Command, "RELATE") || Equal(global.Command, "DISPLAY"))
    {
      UseScCabTestSecurity();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }

    // **** end   group C ****
    if (Equal(global.Command, "RETOPAY"))
    {
      local.ReturnFromLink.Flag = "Y";

      if (AsChar(export.PriPayor.PromptField) == 'S')
      {
        if (!IsEmpty(export.ObligationType.Classification))
        {
          export.PriCsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
          export.PriCsePersonsWorkSet.FormattedName =
            export.CsePersonsWorkSet.FormattedName;
          export.PriObligation.SystemGeneratedIdentifier =
            export.Obligation.SystemGeneratedIdentifier;
          export.PriObligationType.Assign(export.ObligationType);
          export.PriCsePerson.Number = export.CsePerson.Number;
          export.PriAccrual.Amount = import.ObligationTransaction.Amount;
          export.PriLegalAction.StandardNumber =
            import.LegalAction.StandardNumber;
        }
        else
        {
          export.PriPayor.PromptField = "+";
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        }
      }

      if (AsChar(export.SecPayor.PromptField) == 'S')
      {
        if (!IsEmpty(export.ObligationType.Classification))
        {
          export.SecCsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
          export.SecCsePersonsWorkSet.FormattedName =
            export.CsePersonsWorkSet.FormattedName;
          export.SecObligation.SystemGeneratedIdentifier =
            export.Obligation.SystemGeneratedIdentifier;
          export.SecObligationType.Assign(export.ObligationType);
          export.SecCsePerson.Number = export.CsePerson.Number;
          export.SecAccrual.Amount = import.ObligationTransaction.Amount;
          export.SecLegalAction.StandardNumber =
            import.LegalAction.StandardNumber;
        }
        else
        {
          export.SecPayor.PromptField = "+";
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        }
      }

      if (!Equal(export.PriCsePerson.Number, export.SecCsePerson.Number))
      {
        if (IsEmpty(export.PriCsePerson.Number))
        {
        }
        else if (IsEmpty(export.SecCsePerson.Number))
        {
        }
        else
        {
          if (AsChar(export.PriPayor.PromptField) == 'S')
          {
            var field1 = GetField(export.PriCsePersonsWorkSet, "formattedName");

            field1.Error = true;

            var field2 = GetField(export.PriCsePersonsWorkSet, "number");

            field2.Error = true;
          }
          else
          {
            var field1 = GetField(export.SecCsePersonsWorkSet, "formattedName");

            field1.Error = true;

            var field2 = GetField(export.SecCsePersonsWorkSet, "number");

            field2.Error = true;
          }

          ExitState = "FN0000_SEC_AND_PRI_MUST_EQUAL";

          return;
        }
      }

      if (!IsEmpty(export.SecCsePerson.Number))
      {
        if (!Equal(export.PriObligationType.Code, export.SecObligationType.Code))
          
        {
          if (AsChar(export.PriPayor.PromptField) == 'S')
          {
            var field = GetField(export.PriObligationType, "code");

            field.Error = true;
          }
          else
          {
            var field = GetField(export.SecObligationType, "code");

            field.Error = true;
          }

          ExitState = "FN0000_OB_TYP_MUST_MATCH_FOR_REL";

          return;
        }
      }

      export.PriPayor.PromptField = "+";
      export.SecPayor.PromptField = "+";
      global.Command = "DISPLAY";
    }
    else if (Equal(global.Command, "FROMOPAY") || Equal
      (global.Command, "FROMOCTO"))
    {
      if (IsEmpty(export.PriObligationType.Code))
      {
        // This means that no obligation was selected from OPAY or OCTO, 
        // therefore a blank screen should be displayed.
        export.PriPayor.PromptField = "+";
        export.SecPayor.PromptField = "+";
        export.PriCsePersonsWorkSet.Number = "";
        export.PriCsePersonsWorkSet.FormattedName = "";
        ExitState = "ACO_NE0000_NO_SELECTION_MADE";

        return;
      }

      export.PriCsePersonsWorkSet.Number = export.PriCsePerson.Number;
      global.Command = "DISPLAY";
    }

    // *** Main logic. Note added by G Sharp ***
    switch(TrimEnd(global.Command))
    {
      case "LIST":
        // *** Only one selection can be made at a time. Code added by G Sharp. 
        // ***
        if (AsChar(export.PriPayor.PromptField) == 'S' && AsChar
          (export.SecPayor.PromptField) == 'S')
        {
          var field3 = GetField(export.PriPayor, "promptField");

          field3.Error = true;

          var field4 = GetField(export.SecPayor, "promptField");

          field4.Error = true;

          ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

          return;
        }

        // ***  So which has been prompt. Code added by G Sharp. ***
        if (AsChar(export.PriPayor.PromptField) == 'S')
        {
          ExitState = "ECO_LNK_LST_OBLIG_BY_AP_PAYOR";

          return;
        }

        if (AsChar(export.SecPayor.PromptField) == 'S')
        {
          ExitState = "ECO_LNK_LST_OBLIG_BY_AP_PAYOR";

          return;
        }

        // *** Something other than "S" was entered in prompt. Code added by G 
        // Sharp. ***
        var field1 = GetField(export.PriPayor, "promptField");

        field1.Error = true;

        var field2 = GetField(export.SecPayor, "promptField");

        field2.Error = true;

        ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

        break;
      case "DISASSOC":
        // =================================================
        // 2/5/00 - bud adams - PR# 86702: Command 'disassoc' is
        //   temporarily disabled until a more complex solution is in
        //   place that identifies a collection as having been applied to
        //   a Joint & Several or a Primary/Secondary obligation.
        // =================================================
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;

        // *** Disassociation Logic. Note added by G Sharp ***
        if (ReadObligationRln1())
        {
          DeleteObligationRln();
          ExitState = "ACO_NI0000_DISASSOCIATE_SCCSSFUL";

          if (ReadObligation1())
          {
            // =================================================
            // 5/20/99 - bud adams  -  zd_fn_calc_amt_owed_for_oblig
            //   replaced with this one.
            // =================================================
            UseFnComputeSummaryTotals1();

            try
            {
              UpdateObligation2();
              ExitState = "ACO_NI0000_DISASSOCIATE_SCCSSFUL";

              if (ReadObligation3())
              {
                // =================================================
                // 5/20/99 - bud adams  -  zd_fn_calc_amt_owed_for_oblig
                //   replaced with this one.
                // =================================================
                UseFnComputeSummaryTotals2();

                try
                {
                  UpdateObligation4();
                  ExitState = "ACO_NI0000_DISASSOCIATE_SCCSSFUL";
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
        else
        {
          ExitState = "FN0000_INVALID_DISASSOCIATE";
        }

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "RELATE":
        // *** RELATE command. Note added by G Sharp. ***
        // *** EDIT, this the start of editing for relating Primary/Secondary 
        // obligations. Note added by G Sharp.
        // *** Only like obligation_type can be related as Primary/Secondary. 
        // sr1. Note added by G Sharp ***
        if (export.PriObligationType.SystemGeneratedIdentifier != export
          .SecObligationType.SystemGeneratedIdentifier)
        {
          var field3 = GetField(export.PriObligationType, "code");

          field3.Error = true;

          var field4 = GetField(export.SecObligationType, "code");

          field4.Error = true;

          ExitState = "FN0000_OB_TYP_MUST_MATCH_FOR_REL";

          return;
        }

        // *** G Sharp 10/27/98. Voluntary, Gift, and Recover debts cannot be 
        // related. sr12. Compare using hardcodes ***
        if (AsChar(export.PriObligationType.Classification) == AsChar
          (local.OtCVoluntaryClassificat.Classification) || AsChar
          (export.PriObligationType.Classification) == AsChar
          (local.OtCRecoverClassificat.Classification) || export
          .PriObligationType.SystemGeneratedIdentifier == local
          .HardcodeOtGift.SystemGeneratedIdentifier)
        {
          var field3 = GetField(export.PriObligationType, "code");

          field3.Error = true;

          var field4 = GetField(export.SecObligationType, "code");

          field4.Error = true;

          ExitState = "FN0000_OB_TYPE_CANT_BE_RELATED";

          return;
        }

        // *** Obligations from different payors cannot be related. Note added 
        // by G Sharp. ***
        if (!Equal(export.PriCsePerson.Number, export.SecCsePerson.Number))
        {
          if (IsEmpty(export.PriCsePerson.Number))
          {
          }
          else if (IsEmpty(export.SecCsePerson.Number))
          {
          }
          else
          {
            ExitState = "FN0000_SEC_AND_PRI_MUST_EQUAL";

            return;
          }
        }

        // *** Check Primary obligation for 1st tied. Obligation can not be 
        // already related. Note added by G Sharp. ***
        if (ReadObligationRln2())
        {
          ExitState = "FN0000_OBLIG_ALREADY_RELATED";

          return;
        }

        // *** Check Primary obligation for 2nd tied. Obligation can not be 
        // already related. Note added by G Sharp. ***
        if (ReadObligationRln4())
        {
          ExitState = "FN0000_OBLIG_ALREADY_RELATED";

          return;
        }

        // *** Check Secondary obligation for 1st tied. Obligation can not be 
        // already related. Note added by G Sharp. ***
        if (ReadObligationRln3())
        {
          ExitState = "FN0000_OBLIG_ALREADY_RELATED";

          return;
        }

        // *** Check Secondary obligation for 2nd tied. Obligation can not be 
        // already related. Note added by G Sharp. ***
        if (ReadObligationRln5())
        {
          ExitState = "FN0000_OBLIG_ALREADY_RELATED";

          return;
        }

        if (ReadObligationRlnRsn())
        {
          if (ReadObligation2())
          {
            // =================================================
            // 5/20/99 - bud adams  -  zd_fn_calc_amt_owed_for_oblig
            //   replaced with this one.
            // =================================================
            UseFnComputeSummaryTotals1();

            if (ReadObligation4())
            {
              // *** Cannot relate an obligation to itself. Note added by G 
              // Sharp. ***
              if (entities.PrimaryObligation.SystemGeneratedIdentifier == entities
                .SecondaryObligation.SystemGeneratedIdentifier)
              {
                var field3 =
                  GetField(export.PriObligation, "systemGeneratedIdentifier");

                field3.Error = true;

                var field4 =
                  GetField(export.SecObligation, "systemGeneratedIdentifier");

                field4.Error = true;

                ExitState = "FN0000_SAME_OBLIGATION";

                return;
              }

              // *** G Sharp 10/27/98. Cannot be related as Primary/Secondary 
              // under the same court order. sr3 ***
              if (IsEmpty(export.PriLegalAction.StandardNumber))
              {
                // ***  OK  ***
              }
              else if (Equal(export.PriLegalAction.StandardNumber,
                export.SecLegalAction.StandardNumber))
              {
                var field3 = GetField(export.PriLegalAction, "standardNumber");

                field3.Error = true;

                var field4 = GetField(export.SecLegalAction, "standardNumber");

                field4.Error = true;

                ExitState = "FN0000_CANT_BE_SAME_COURT_ORDER";

                return;
              }

              // -------------------------------------------------------------------------------------
              // PR# 89089 : Do not allow  relating two obligations as primary 
              // and secondary,  if either of the obligations have collections
              // applied.
              // ---------------------------------------------------------------------------------------
              foreach(var item in ReadCollection1())
              {
                if (AsChar(entities.Collection.AdjustedInd) == 'N')
                {
                  ExitState = "FN0000_CANT_RELAT_OBLG_WITH_COLL";

                  return;
                }
                else
                {
                  // -------------------------------------------------------------------------
                  // Collection not  applied or readjusted  for this obligation.
                  // Proceed.....
                  // -----------------------------------------------------------------------
                }
              }

              foreach(var item in ReadCollection2())
              {
                if (AsChar(entities.Collection.AdjustedInd) == 'N')
                {
                  ExitState = "FN0000_CANT_RELAT_OBLG_WITH_COLL";

                  return;
                }
                else
                {
                  // -------------------------------------------------------------------------
                  // Collection not  applied or readjusted  for this obligation.
                  // Proceed.....
                  // -----------------------------------------------------------------------
                }
              }

              // =================================================
              // 5/20/99 - bud adams  -  zd_fn_calc_amt_owed_for_oblig
              //   replaced with this one.
              // =================================================
              UseFnComputeSummaryTotals2();

              try
              {
                CreateObligationRln();
                MoveObligationRln(entities.New1, export.ObligationRln);
                export.RelEst.Date = Date(entities.New1.CreatedTmst);

                try
                {
                  UpdateObligation1();

                  try
                  {
                    UpdateObligation3();
                    ExitState = "SY0000_COMMAND_SUCCESSFUL";
                  }
                  catch(Exception e2)
                  {
                    switch(GetErrorCode(e2))
                    {
                      case ErrorCode.AlreadyExists:
                        break;
                      case ErrorCode.PermittedValueViolation:
                        ExitState = "FN0000_OBL_REL_PV";

                        break;
                      case ErrorCode.DatabaseError:
                        break;
                      default:
                        throw;
                    }
                  }
                }
                catch(Exception e1)
                {
                  switch(GetErrorCode(e1))
                  {
                    case ErrorCode.AlreadyExists:
                      break;
                    case ErrorCode.PermittedValueViolation:
                      ExitState = "FN0000_OBL_REL_PV";

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
                    break;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "FN0000_OBL_REL_PV";

                    break;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }
            }
          }
        }
        else
        {
          ExitState = "SY0000_MISSING_OBL_REL_RSN";
        }

        // *** DISPLAY command. Note added by G Sharp. ***
        break;
      case "DISPLAY":
        if (!Equal(export.PriCsePerson.Number, export.SecCsePerson.Number))
        {
          if (IsEmpty(export.PriCsePerson.Number))
          {
          }
          else if (IsEmpty(export.SecCsePerson.Number))
          {
          }
          else
          {
            ExitState = "FN0000_SEC_AND_PRI_MUST_EQUAL";

            return;
          }
        }

        if (!IsEmpty(export.PriCsePersonsWorkSet.Number))
        {
          UseSiReadCsePerson1();

          if (ReadObligation1())
          {
            if (AsChar(entities.PrimaryObligation.PrimarySecondaryCode) == AsChar
              (local.HardcodeObligPrimaryConcurrnt.PrimarySecondaryCode) || IsEmpty
              (entities.PrimaryObligation.PrimarySecondaryCode))
            {
              MoveObligation(entities.PrimaryObligation, export.PriObligation);

              // =================================================
              // 5/20/99 - bud adams  -  zd_fn_calc_amt_owed_for_oblig
              //   replaced with this one.
              // =================================================
              UseFnComputeSummaryTotals3();

              if (ReadObligationPaymentSchedule1())
              {
                UseFnSetFrequencyTextField2();
              }
            }
            else if (AsChar(entities.PrimaryObligation.PrimarySecondaryCode) ==
              AsChar(local.HardcodeObligSecondaryConcrnt.PrimarySecondaryCode))
            {
              // *****************************************************************
              //   This means that, from the calling procedure, a
              //   secondary obligation was selected, therefore the
              //   primary must be read and the proper export views populated.
              //   Since a presumption is made that a passed obligation is
              //   a primary one on first entry from OPAY or OCTO, I just use
              //   this little bit of backwards looking reads to populate things
              //   correctly.  RVW 10/14/96 (Columbus Day)
              // ************************************************************
              MoveObligation(entities.PrimaryObligation, export.SecObligation);
              export.SecCsePerson.Number = export.PriCsePerson.Number;
              MoveCsePersonsWorkSet(export.PriCsePersonsWorkSet,
                export.SecCsePersonsWorkSet);
              export.SecLegalAction.StandardNumber =
                export.PriLegalAction.StandardNumber;
              export.SecObligationType.Assign(export.PriObligationType);

              // =================================================
              // 5/20/99 - bud adams  -  zd_fn_calc_amt_owed_for_oblig
              //   replaced with this one.
              // =================================================
              UseFnComputeSummaryTotals4();

              if (ReadObligationPaymentSchedule2())
              {
                UseFnSetFrequencyTextField1();
              }

              if (ReadObligationObligationRln())
              {
                if (ReadLegalAction2())
                {
                  export.PriLegalAction.StandardNumber =
                    entities.LegalAction.StandardNumber;
                }

                MoveObligation(entities.SecondaryObligation,
                  export.PriObligation);
                MoveObligationRln(entities.ObligationRln, export.ObligationRln);
                export.RelEst.Date = Date(entities.ObligationRln.CreatedTmst);

                // =================================================
                // 5/20/99 - bud adams  -  zd_fn_calc_amt_owed_for_oblig
                //   replaced with this one.
                // =================================================
                UseFnComputeSummaryTotals3();

                goto Test;
              }
            }
          }

          if (ReadObligationRlnObligation2())
          {
            MoveObligationRln(entities.ObligationRln, export.ObligationRln);
            MoveObligation(entities.SecondaryObligation, export.SecObligation);
            export.RelEst.Date = Date(entities.ObligationRln.CreatedTmst);
            export.SecObligationType.Assign(export.PriObligationType);
            MoveCsePersonsWorkSet(export.PriCsePersonsWorkSet,
              export.SecCsePersonsWorkSet);
            export.SecCsePerson.Number = export.PriCsePerson.Number;

            if (ReadLegalAction2())
            {
              export.SecLegalAction.StandardNumber =
                entities.LegalAction.StandardNumber;
            }
          }
          else if (!IsEmpty(export.SecCsePersonsWorkSet.Number))
          {
            UseSiReadCsePerson2();

            if (ReadObligation3())
            {
              MoveObligation(entities.SecondaryObligation, export.SecObligation);
                

              // =================================================
              // 5/20/99 - bud adams  -  zd_fn_calc_amt_owed_for_oblig
              //   replaced with this one.
              // =================================================
              UseFnComputeSummaryTotals4();

              if (ReadObligationRlnObligation1())
              {
                if (AsChar(local.ReturnFromLink.Flag) == 'Y')
                {
                  var field = GetField(export.SecCsePersonsWorkSet, "number");

                  field.Error = true;

                  ExitState = "FN0000_OBLIG_ALREADY_RELATED";

                  return;
                }

                MoveObligationRln(entities.ObligationRln, export.ObligationRln);
                export.RelEst.Date = Date(entities.ObligationRln.CreatedTmst);
                export.SecObligationType.Assign(entities.PrimaryObligationType);
                MoveCsePersonsWorkSet(export.PriCsePersonsWorkSet,
                  export.SecCsePersonsWorkSet);
                export.SecCsePerson.Number = export.PriCsePerson.Number;

                if (ReadLegalAction1())
                {
                  export.SecLegalAction.StandardNumber =
                    entities.LegalAction.StandardNumber;
                }
              }
            }
          }
          else
          {
          }
        }
        else if (!IsEmpty(export.SecCsePersonsWorkSet.Number))
        {
          UseSiReadCsePerson2();

          if (ReadObligation3())
          {
            MoveObligation(entities.SecondaryObligation, export.SecObligation);

            // =================================================
            // 5/20/99 - bud adams  -  zd_fn_calc_amt_owed_for_oblig
            //   replaced with this one.
            // =================================================
            UseFnComputeSummaryTotals4();
          }

          if (ReadObligationRlnObligation1())
          {
            MoveObligationRln(entities.ObligationRln, export.ObligationRln);
            MoveObligation(entities.PrimaryObligation, export.PriObligation);
            export.RelEst.Date = Date(entities.ObligationRln.CreatedTmst);
            export.SecObligationType.Assign(entities.PrimaryObligationType);
            MoveCsePersonsWorkSet(export.PriCsePersonsWorkSet,
              export.SecCsePersonsWorkSet);
            export.SecCsePerson.Number = export.PriCsePerson.Number;

            if (ReadLegalAction1())
            {
              export.PriLegalAction.StandardNumber =
                entities.LegalAction.StandardNumber;
            }
          }
        }
        else
        {
          ExitState = "FN0000_MUST_SEL_OB_FIRST";

          return;
        }

Test:

        if (entities.PrimaryObligation.Populated)
        {
          // =================================================
          // 5/20/99 - bud adams  -  zd_fn_calc_amt_owed_for_oblig
          //   replaced with this one.
          // =================================================
          UseFnComputeSummaryTotals3();

          if (ReadObligationPaymentSchedule1())
          {
            UseFnSetFrequencyTextField2();
          }

          if (AsChar(export.PriObligationType.Classification) == 'A')
          {
            // *** Accruing Type Obligation
            // ***---  cursor only.  Most of the time there will be > 1 row, but
            // ***---  they'll all have the same As_Of_Date.
            if (ReadAccrualInstructions1())
            {
              export.PriAccrualDue.Date = entities.AccrualInstructions.AsOfDt;
            }

            export.PriAccrual.Amount = 0;

            export.Group.Index = 0;
            export.Group.Clear();

            foreach(var item in ReadObligationTransaction2())
            {
              MoveObligationTransaction1(entities.ObligationTransaction,
                export.Group.Update.ObligationTransaction);
              export.PriAccrual.Amount += entities.ObligationTransaction.Amount;

              if (ReadAccrualInstructions3())
              {
                foreach(var item1 in ReadAccrualSuspension2())
                {
                  if (Lt(local.Current.Date, entities.AccrualSuspension.ResumeDt)
                    && !
                    Lt(local.Current.Date, entities.AccrualSuspension.SuspendDt))
                    
                  {
                    if (!Equal(entities.AccrualSuspension.ReductionPercentage, 0))
                      
                    {
                      export.PriAccrual.Amount -= entities.AccrualSuspension.
                        ReductionPercentage.GetValueOrDefault() * entities
                        .ObligationTransaction.Amount / 100;
                    }
                    else
                    {
                      export.PriAccrual.Amount -= entities.AccrualSuspension.
                        ReductionAmount.GetValueOrDefault();
                    }

                    break;
                  }
                }
              }

              export.Group.Next();
            }
          }
          else
          {
            // *** NON Accruing Type Obligation
            // ***---  Default read.  There may be many debt_detail rows for
            // ***---  each non-accruing debt, one for each supported person
            // ***---  but they will all have the same due-date.  Most will
            // ***---  have one only.
            if (ReadDebtDetail1())
            {
              export.PriAccrualDue.Date = entities.DebtDetail.DueDt;
            }

            export.PriAccrual.Amount = 0;

            foreach(var item in ReadObligationTransaction1())
            {
              export.PriAccrual.Amount += entities.ObligationTransaction.Amount;

              foreach(var item1 in ReadDebtAdjustment())
              {
                if (AsChar(entities.DebtAdjustment.DebtAdjustmentType) == 'I')
                {
                  export.PriAccrual.Amount += entities.DebtAdjustment.Amount;
                }
                else
                {
                  export.PriAccrual.Amount -= entities.DebtAdjustment.Amount;
                }
              }
            }
          }
        }

        if (entities.SecondaryObligation.Populated && !
          IsEmpty(export.SecCsePersonsWorkSet.FormattedName) && !
          IsEmpty(export.SecCsePersonsWorkSet.Number))
        {
          // =================================================
          // 5/20/99 - bud adams  -  zd_fn_calc_amt_owed_for_oblig
          //   replaced with this one.
          // =================================================
          UseFnComputeSummaryTotals4();

          if (ReadObligationPaymentSchedule2())
          {
            UseFnSetFrequencyTextField1();
          }

          if (AsChar(export.SecObligationType.Classification) == 'A')
          {
            // *** Accruing Type Obligation
            // ***---  cursor only.  Most of the time there will be > 1 row, but
            // ***---  they'll all have the same As_Of_Date.
            if (ReadAccrualInstructions2())
            {
              export.SecAccrualDue.Date = entities.AccrualInstructions.AsOfDt;
            }

            export.SecAccrual.Amount = 0;

            export.Group.Index = 0;
            export.Group.Clear();

            foreach(var item in ReadObligationTransaction3())
            {
              MoveObligationTransaction1(entities.ObligationTransaction,
                export.Group.Update.ObligationTransaction);
              export.SecAccrual.Amount += entities.ObligationTransaction.Amount;

              if (ReadAccrualInstructions3())
              {
                export.Group.Item.Of.Index = 0;
                export.Group.Item.Of.Clear();

                foreach(var item1 in ReadAccrualSuspension1())
                {
                  if (Lt(local.Current.Date, entities.AccrualSuspension.ResumeDt)
                    && !
                    Lt(local.Current.Date, entities.AccrualSuspension.SuspendDt))
                    
                  {
                    export.SecAccrual.Amount -= entities.AccrualSuspension.
                      ReductionPercentage.GetValueOrDefault() * entities
                      .ObligationTransaction.Amount / 100;
                    export.Group.Item.Of.Next();

                    break;
                  }

                  export.Group.Item.Of.Next();
                }
              }

              export.Group.Next();
            }
          }
          else
          {
            // *** NON Accruing Type Obligation
            // ***---  Default read.  There may be many debt_detail rows for
            // ***---  each non-accruing debt, one for each supported person
            // ***---  but they will all have the same due-date.  Most will
            // ***---  have one only.
            if (ReadDebtDetail2())
            {
              export.SecAccrualDue.Date = entities.DebtDetail.DueDt;
            }

            export.SecAccrual.Amount = 0;

            export.Group.Index = 0;
            export.Group.Clear();

            foreach(var item in ReadObligationTransaction4())
            {
              MoveObligationTransaction1(entities.ObligationTransaction,
                export.Group.Update.ObligationTransaction);
              export.SecAccrual.Amount += entities.ObligationTransaction.Amount;
              export.Group.Next();
            }
          }
        }

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveAccrualInstructions(AccrualInstructions source,
    AccrualInstructions target)
  {
    target.AsOfDt = source.AsOfDt;
    target.DiscontinueDt = source.DiscontinueDt;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveDebtDetail(DebtDetail source, DebtDetail target)
  {
    target.DueDt = source.DueDt;
    target.RetiredDt = source.RetiredDt;
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveNextTranInfo(NextTranInfo source, NextTranInfo target)
  {
    target.LegalActionIdentifier = source.LegalActionIdentifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CsePersonNumberAp = source.CsePersonNumberAp;
    target.CsePersonNumberObligee = source.CsePersonNumberObligee;
    target.CsePersonNumberObligor = source.CsePersonNumberObligor;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.ObligationId = source.ObligationId;
    target.StandardCrtOrdNumber = source.StandardCrtOrdNumber;
    target.InfrastructureId = source.InfrastructureId;
    target.MiscText1 = source.MiscText1;
    target.MiscText2 = source.MiscText2;
    target.MiscNum1 = source.MiscNum1;
    target.MiscNum2 = source.MiscNum2;
    target.MiscNum1V2 = source.MiscNum1V2;
    target.MiscNum2V2 = source.MiscNum2V2;
  }

  private static void MoveObligation(Obligation source, Obligation target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.OrderTypeCode = source.OrderTypeCode;
  }

  private static void MoveObligationPaymentSchedule(
    ObligationPaymentSchedule source, ObligationPaymentSchedule target)
  {
    target.FrequencyCode = source.FrequencyCode;
    target.DayOfWeek = source.DayOfWeek;
    target.DayOfMonth1 = source.DayOfMonth1;
    target.DayOfMonth2 = source.DayOfMonth2;
    target.PeriodInd = source.PeriodInd;
  }

  private static void MoveObligationRln(ObligationRln source,
    ObligationRln target)
  {
    target.Description = source.Description;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTmst = source.CreatedTmst;
  }

  private static void MoveObligationTransaction1(ObligationTransaction source,
    ObligationTransaction target)
  {
    target.Type1 = source.Type1;
    target.Amount = source.Amount;
  }

  private static void MoveObligationTransaction2(ObligationTransaction source,
    ObligationTransaction target)
  {
    target.Type1 = source.Type1;
    target.DebtType = source.DebtType;
  }

  private void UseFnComputeSummaryTotals1()
  {
    var useImport = new FnComputeSummaryTotals.Import();
    var useExport = new FnComputeSummaryTotals.Export();

    useImport.Filter.SystemGeneratedIdentifier =
      entities.PrimaryObligation.SystemGeneratedIdentifier;
    useImport.FilterByIdObligationType.SystemGeneratedIdentifier =
      export.PriObligationType.SystemGeneratedIdentifier;
    useImport.Obligor.Number = export.PriCsePerson.Number;

    Call(FnComputeSummaryTotals.Execute, useImport, useExport);

    export.PriScreenOwedAmounts.Assign(useExport.ScreenOwedAmounts);
  }

  private void UseFnComputeSummaryTotals2()
  {
    var useImport = new FnComputeSummaryTotals.Import();
    var useExport = new FnComputeSummaryTotals.Export();

    useImport.Filter.SystemGeneratedIdentifier =
      entities.SecondaryObligation.SystemGeneratedIdentifier;
    useImport.FilterByIdObligationType.SystemGeneratedIdentifier =
      export.SecObligationType.SystemGeneratedIdentifier;
    useImport.Obligor.Number = export.SecCsePerson.Number;

    Call(FnComputeSummaryTotals.Execute, useImport, useExport);

    export.SecScreenOwedAmounts.Assign(useExport.ScreenOwedAmounts);
  }

  private void UseFnComputeSummaryTotals3()
  {
    var useImport = new FnComputeSummaryTotals.Import();
    var useExport = new FnComputeSummaryTotals.Export();

    useImport.FilterByIdObligationType.SystemGeneratedIdentifier =
      export.PriObligationType.SystemGeneratedIdentifier;
    useImport.Obligor.Number = export.PriCsePerson.Number;
    useImport.Filter.SystemGeneratedIdentifier =
      export.PriObligation.SystemGeneratedIdentifier;

    Call(FnComputeSummaryTotals.Execute, useImport, useExport);

    export.PriScreenOwedAmounts.Assign(useExport.ScreenOwedAmounts);
  }

  private void UseFnComputeSummaryTotals4()
  {
    var useImport = new FnComputeSummaryTotals.Import();
    var useExport = new FnComputeSummaryTotals.Export();

    useImport.FilterByIdObligationType.SystemGeneratedIdentifier =
      export.SecObligationType.SystemGeneratedIdentifier;
    useImport.Obligor.Number = export.SecCsePerson.Number;
    useImport.Filter.SystemGeneratedIdentifier =
      export.SecObligation.SystemGeneratedIdentifier;

    Call(FnComputeSummaryTotals.Execute, useImport, useExport);

    export.SecScreenOwedAmounts.Assign(useExport.ScreenOwedAmounts);
  }

  private void UseFnHardcodedDebtDistribution()
  {
    var useImport = new FnHardcodedDebtDistribution.Import();
    var useExport = new FnHardcodedDebtDistribution.Export();

    Call(FnHardcodedDebtDistribution.Execute, useImport, useExport);

    local.HardcodeOtGift.SystemGeneratedIdentifier =
      useExport.OtGift.SystemGeneratedIdentifier;
    local.OtCRecoverClassificat.Classification =
      useExport.OtCRecoverClassification.Classification;
    local.OtCVoluntaryClassificat.Classification =
      useExport.OtCVoluntaryClassification.Classification;
    local.HardcodeObligSecondaryConcrnt.PrimarySecondaryCode =
      useExport.ObligSecondaryConcurrent.PrimarySecondaryCode;
    local.HardcodeObligPrimaryConcurrnt.PrimarySecondaryCode =
      useExport.ObligPrimaryConcurrent.PrimarySecondaryCode;
    local.HardcodeOtrnTDebt.Type1 = useExport.OtrnTDebt.Type1;
    local.HardcodeOrrPrimarySecondary.SequentialGeneratedIdentifier =
      useExport.OrrPrimarySecondary.SequentialGeneratedIdentifier;
    local.HardcodeCpaObligor.Type1 = useExport.CpaObligor.Type1;
    MoveObligationTransaction2(useExport.OtrnDtAccrualInstructions,
      local.HardcodeOtrnDtAccrualInstruc);
  }

  private void UseFnSetFrequencyTextField1()
  {
    var useImport = new FnSetFrequencyTextField.Import();
    var useExport = new FnSetFrequencyTextField.Export();

    MoveObligationPaymentSchedule(entities.ObligationPaymentSchedule,
      useImport.ObligationPaymentSchedule);

    Call(FnSetFrequencyTextField.Execute, useImport, useExport);

    export.SecFrequencyWorkSet.Assign(useExport.FrequencyWorkSet);
  }

  private void UseFnSetFrequencyTextField2()
  {
    var useImport = new FnSetFrequencyTextField.Import();
    var useExport = new FnSetFrequencyTextField.Export();

    MoveObligationPaymentSchedule(entities.ObligationPaymentSchedule,
      useImport.ObligationPaymentSchedule);

    Call(FnSetFrequencyTextField.Execute, useImport, useExport);

    export.PriFrequencyWorkSet.Assign(useExport.FrequencyWorkSet);
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.Hidden.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveNextTranInfo(export.Hidden, useImport.NextTranInfo);

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabSignoff()
  {
    var useImport = new ScCabSignoff.Import();
    var useExport = new ScCabSignoff.Export();

    Call(ScCabSignoff.Execute, useImport, useExport);
  }

  private void UseScCabTestSecurity()
  {
    var useImport = new ScCabTestSecurity.Import();
    var useExport = new ScCabTestSecurity.Export();

    useImport.CsePerson.Number = import.CsePerson.Number;
    useImport.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;
    MoveLegalAction(import.LegalAction, useImport.LegalAction);

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiReadCsePerson1()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.PriCsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet,
      export.PriCsePersonsWorkSet);
    export.PriCsePerson.Number = useExport.CsePerson.Number;
  }

  private void UseSiReadCsePerson2()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.SecCsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet,
      export.SecCsePersonsWorkSet);
    export.SecCsePerson.Number = useExport.CsePerson.Number;
  }

  private void CreateObligationRln()
  {
    System.Diagnostics.Debug.Assert(entities.PrimaryObligation.Populated);
    System.Diagnostics.Debug.Assert(entities.SecondaryObligation.Populated);

    var obgGeneratedId = entities.SecondaryObligation.SystemGeneratedIdentifier;
    var cspNumber = entities.SecondaryObligation.CspNumber;
    var cpaType = entities.SecondaryObligation.CpaType;
    var obgFGeneratedId = entities.PrimaryObligation.SystemGeneratedIdentifier;
    var cspFNumber = entities.PrimaryObligation.CspNumber;
    var cpaFType = entities.PrimaryObligation.CpaType;
    var orrGeneratedId =
      entities.ObligationRlnRsn.SequentialGeneratedIdentifier;
    var createdBy = global.UserId;
    var createdTmst = local.Current.Timestamp;
    var otySecondId = entities.SecondaryObligation.DtyGeneratedId;
    var otyFirstId = entities.PrimaryObligation.DtyGeneratedId;

    CheckValid<ObligationRln>("CpaType", cpaType);
    CheckValid<ObligationRln>("CpaFType", cpaFType);
    entities.New1.Populated = false;
    Update("CreateObligationRln",
      (db, command) =>
      {
        db.SetInt32(command, "obgGeneratedId", obgGeneratedId);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "cpaType", cpaType);
        db.SetInt32(command, "obgFGeneratedId", obgFGeneratedId);
        db.SetString(command, "cspFNumber", cspFNumber);
        db.SetString(command, "cpaFType", cpaFType);
        db.SetInt32(command, "orrGeneratedId", orrGeneratedId);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetInt32(command, "otySecondId", otySecondId);
        db.SetInt32(command, "otyFirstId", otyFirstId);
        db.SetString(command, "obRlnDsc", "");
      });

    entities.New1.ObgGeneratedId = obgGeneratedId;
    entities.New1.CspNumber = cspNumber;
    entities.New1.CpaType = cpaType;
    entities.New1.ObgFGeneratedId = obgFGeneratedId;
    entities.New1.CspFNumber = cspFNumber;
    entities.New1.CpaFType = cpaFType;
    entities.New1.OrrGeneratedId = orrGeneratedId;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CreatedTmst = createdTmst;
    entities.New1.OtySecondId = otySecondId;
    entities.New1.OtyFirstId = otyFirstId;
    entities.New1.Description = "";
    entities.New1.Populated = true;
  }

  private void DeleteObligationRln()
  {
    Update("DeleteObligationRln",
      (db, command) =>
      {
        db.SetInt32(
          command, "obgGeneratedId", entities.ObligationRln.ObgGeneratedId);
        db.SetString(command, "cspNumber", entities.ObligationRln.CspNumber);
        db.SetString(command, "cpaType", entities.ObligationRln.CpaType);
        db.SetInt32(
          command, "obgFGeneratedId", entities.ObligationRln.ObgFGeneratedId);
        db.SetString(command, "cspFNumber", entities.ObligationRln.CspFNumber);
        db.SetString(command, "cpaFType", entities.ObligationRln.CpaFType);
        db.SetInt32(
          command, "orrGeneratedId", entities.ObligationRln.OrrGeneratedId);
        db.SetInt32(command, "otySecondId", entities.ObligationRln.OtySecondId);
        db.SetInt32(command, "otyFirstId", entities.ObligationRln.OtyFirstId);
      });
  }

  private bool ReadAccrualInstructions1()
  {
    entities.AccrualInstructions.Populated = false;

    return Read("ReadAccrualInstructions1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.PriCsePerson.Number);
        db.SetString(command, "cpaType", local.HardcodeCpaObligor.Type1);
        db.SetInt32(
          command, "otyType",
          export.PriObligationType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "obgGeneratedId",
          export.PriObligation.SystemGeneratedIdentifier);
        db.SetString(command, "obTrnTyp", local.HardcodeOtrnTDebt.Type1);
        db.SetString(
          command, "debtTyp", local.HardcodeOtrnDtAccrualInstruc.DebtType);
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
        entities.AccrualInstructions.LastAccrualDt =
          db.GetNullableDate(reader, 8);
        entities.AccrualInstructions.Populated = true;
        CheckValid<AccrualInstructions>("OtrType",
          entities.AccrualInstructions.OtrType);
        CheckValid<AccrualInstructions>("CpaType",
          entities.AccrualInstructions.CpaType);
      });
  }

  private bool ReadAccrualInstructions2()
  {
    System.Diagnostics.Debug.Assert(entities.SecondaryObligation.Populated);
    entities.AccrualInstructions.Populated = false;

    return Read("ReadAccrualInstructions2",
      (db, command) =>
      {
        db.SetInt32(
          command, "otyId", entities.SecondaryObligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.SecondaryObligation.SystemGeneratedIdentifier);
        db.SetString(
          command, "cspNumber", entities.SecondaryObligation.CspNumber);
        db.SetString(command, "cpaType", entities.SecondaryObligation.CpaType);
        db.SetString(command, "otrType", local.HardcodeOtrnTDebt.Type1);
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
        entities.AccrualInstructions.LastAccrualDt =
          db.GetNullableDate(reader, 8);
        entities.AccrualInstructions.Populated = true;
        CheckValid<AccrualInstructions>("OtrType",
          entities.AccrualInstructions.OtrType);
        CheckValid<AccrualInstructions>("CpaType",
          entities.AccrualInstructions.CpaType);
      });
  }

  private bool ReadAccrualInstructions3()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.Existing.Populated = false;

    return Read("ReadAccrualInstructions3",
      (db, command) =>
      {
        db.SetString(command, "otrType", entities.ObligationTransaction.Type1);
        db.SetInt32(command, "otyId", entities.ObligationTransaction.OtyType);
        db.SetInt32(
          command, "otrGeneratedId",
          entities.ObligationTransaction.SystemGeneratedIdentifier);
        db.
          SetString(command, "cpaType", entities.ObligationTransaction.CpaType);
          
        db.SetString(
          command, "cspNumber", entities.ObligationTransaction.CspNumber);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ObligationTransaction.ObgGeneratedId);
      },
      (db, reader) =>
      {
        entities.Existing.OtrType = db.GetString(reader, 0);
        entities.Existing.OtyId = db.GetInt32(reader, 1);
        entities.Existing.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.Existing.CspNumber = db.GetString(reader, 3);
        entities.Existing.CpaType = db.GetString(reader, 4);
        entities.Existing.OtrGeneratedId = db.GetInt32(reader, 5);
        entities.Existing.AsOfDt = db.GetDate(reader, 6);
        entities.Existing.DiscontinueDt = db.GetNullableDate(reader, 7);
        entities.Existing.LastAccrualDt = db.GetNullableDate(reader, 8);
        entities.Existing.Populated = true;
        CheckValid<AccrualInstructions>("OtrType", entities.Existing.OtrType);
        CheckValid<AccrualInstructions>("CpaType", entities.Existing.CpaType);
      });
  }

  private IEnumerable<bool> ReadAccrualSuspension1()
  {
    System.Diagnostics.Debug.Assert(entities.Existing.Populated);

    return ReadEach("ReadAccrualSuspension1",
      (db, command) =>
      {
        db.SetInt32(command, "otrId", entities.Existing.OtrGeneratedId);
        db.SetString(command, "cpaType", entities.Existing.CpaType);
        db.SetString(command, "cspNumber", entities.Existing.CspNumber);
        db.SetInt32(command, "obgId", entities.Existing.ObgGeneratedId);
        db.SetInt32(command, "otyId", entities.Existing.OtyId);
        db.SetString(command, "otrType", entities.Existing.OtrType);
      },
      (db, reader) =>
      {
        if (export.Group.Item.Of.IsFull)
        {
          return false;
        }

        entities.AccrualSuspension.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.AccrualSuspension.SuspendDt = db.GetDate(reader, 1);
        entities.AccrualSuspension.ResumeDt = db.GetNullableDate(reader, 2);
        entities.AccrualSuspension.ReductionPercentage =
          db.GetNullableDecimal(reader, 3);
        entities.AccrualSuspension.OtrType = db.GetString(reader, 4);
        entities.AccrualSuspension.OtyId = db.GetInt32(reader, 5);
        entities.AccrualSuspension.ObgId = db.GetInt32(reader, 6);
        entities.AccrualSuspension.CspNumber = db.GetString(reader, 7);
        entities.AccrualSuspension.CpaType = db.GetString(reader, 8);
        entities.AccrualSuspension.OtrId = db.GetInt32(reader, 9);
        entities.AccrualSuspension.ReductionAmount =
          db.GetNullableDecimal(reader, 10);
        entities.AccrualSuspension.ReasonTxt = db.GetNullableString(reader, 11);
        entities.AccrualSuspension.Populated = true;
        CheckValid<AccrualSuspension>("OtrType",
          entities.AccrualSuspension.OtrType);
        CheckValid<AccrualSuspension>("CpaType",
          entities.AccrualSuspension.CpaType);

        return true;
      });
  }

  private IEnumerable<bool> ReadAccrualSuspension2()
  {
    System.Diagnostics.Debug.Assert(entities.Existing.Populated);
    entities.AccrualSuspension.Populated = false;

    return ReadEach("ReadAccrualSuspension2",
      (db, command) =>
      {
        db.SetInt32(command, "otrId", entities.Existing.OtrGeneratedId);
        db.SetString(command, "cpaType", entities.Existing.CpaType);
        db.SetString(command, "cspNumber", entities.Existing.CspNumber);
        db.SetInt32(command, "obgId", entities.Existing.ObgGeneratedId);
        db.SetInt32(command, "otyId", entities.Existing.OtyId);
        db.SetString(command, "otrType", entities.Existing.OtrType);
      },
      (db, reader) =>
      {
        entities.AccrualSuspension.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.AccrualSuspension.SuspendDt = db.GetDate(reader, 1);
        entities.AccrualSuspension.ResumeDt = db.GetNullableDate(reader, 2);
        entities.AccrualSuspension.ReductionPercentage =
          db.GetNullableDecimal(reader, 3);
        entities.AccrualSuspension.OtrType = db.GetString(reader, 4);
        entities.AccrualSuspension.OtyId = db.GetInt32(reader, 5);
        entities.AccrualSuspension.ObgId = db.GetInt32(reader, 6);
        entities.AccrualSuspension.CspNumber = db.GetString(reader, 7);
        entities.AccrualSuspension.CpaType = db.GetString(reader, 8);
        entities.AccrualSuspension.OtrId = db.GetInt32(reader, 9);
        entities.AccrualSuspension.ReductionAmount =
          db.GetNullableDecimal(reader, 10);
        entities.AccrualSuspension.ReasonTxt = db.GetNullableString(reader, 11);
        entities.AccrualSuspension.Populated = true;
        CheckValid<AccrualSuspension>("OtrType",
          entities.AccrualSuspension.OtrType);
        CheckValid<AccrualSuspension>("CpaType",
          entities.AccrualSuspension.CpaType);

        return true;
      });
  }

  private IEnumerable<bool> ReadCollection1()
  {
    System.Diagnostics.Debug.Assert(entities.PrimaryObligation.Populated);
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection1",
      (db, command) =>
      {
        db.
          SetInt32(command, "otyId", entities.PrimaryObligation.DtyGeneratedId);
          
        db.SetInt32(
          command, "obgId",
          entities.PrimaryObligation.SystemGeneratedIdentifier);
        db.
          SetString(command, "cspNumber", entities.PrimaryObligation.CspNumber);
          
        db.SetString(command, "cpaType", entities.PrimaryObligation.CpaType);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 1);
        entities.Collection.CrtType = db.GetInt32(reader, 2);
        entities.Collection.CstId = db.GetInt32(reader, 3);
        entities.Collection.CrvId = db.GetInt32(reader, 4);
        entities.Collection.CrdId = db.GetInt32(reader, 5);
        entities.Collection.ObgId = db.GetInt32(reader, 6);
        entities.Collection.CspNumber = db.GetString(reader, 7);
        entities.Collection.CpaType = db.GetString(reader, 8);
        entities.Collection.OtrId = db.GetInt32(reader, 9);
        entities.Collection.OtrType = db.GetString(reader, 10);
        entities.Collection.OtyId = db.GetInt32(reader, 11);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);

        return true;
      });
  }

  private IEnumerable<bool> ReadCollection2()
  {
    System.Diagnostics.Debug.Assert(entities.SecondaryObligation.Populated);
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection2",
      (db, command) =>
      {
        db.SetInt32(
          command, "otyId", entities.SecondaryObligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgId",
          entities.SecondaryObligation.SystemGeneratedIdentifier);
        db.SetString(
          command, "cspNumber", entities.SecondaryObligation.CspNumber);
        db.SetString(command, "cpaType", entities.SecondaryObligation.CpaType);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 1);
        entities.Collection.CrtType = db.GetInt32(reader, 2);
        entities.Collection.CstId = db.GetInt32(reader, 3);
        entities.Collection.CrvId = db.GetInt32(reader, 4);
        entities.Collection.CrdId = db.GetInt32(reader, 5);
        entities.Collection.ObgId = db.GetInt32(reader, 6);
        entities.Collection.CspNumber = db.GetString(reader, 7);
        entities.Collection.CpaType = db.GetString(reader, 8);
        entities.Collection.OtrId = db.GetInt32(reader, 9);
        entities.Collection.OtrType = db.GetString(reader, 10);
        entities.Collection.OtyId = db.GetInt32(reader, 11);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);

        return true;
      });
  }

  private IEnumerable<bool> ReadDebtAdjustment()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.DebtAdjustment.Populated = false;

    return ReadEach("ReadDebtAdjustment",
      (db, command) =>
      {
        db.SetInt32(
          command, "otyTypePrimary", entities.ObligationTransaction.OtyType);
        db.SetString(command, "otrPType", entities.ObligationTransaction.Type1);
        db.SetInt32(
          command, "otrPGeneratedId",
          entities.ObligationTransaction.SystemGeneratedIdentifier);
        db.
          SetString(command, "cpaPType", entities.ObligationTransaction.CpaType);
          
        db.SetString(
          command, "cspPNumber", entities.ObligationTransaction.CspNumber);
        db.SetInt32(
          command, "obgPGeneratedId",
          entities.ObligationTransaction.ObgGeneratedId);
      },
      (db, reader) =>
      {
        entities.DebtAdjustment.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtAdjustment.CspNumber = db.GetString(reader, 1);
        entities.DebtAdjustment.CpaType = db.GetString(reader, 2);
        entities.DebtAdjustment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.DebtAdjustment.Type1 = db.GetString(reader, 4);
        entities.DebtAdjustment.Amount = db.GetDecimal(reader, 5);
        entities.DebtAdjustment.DebtAdjustmentType = db.GetString(reader, 6);
        entities.DebtAdjustment.CspSupNumber = db.GetNullableString(reader, 7);
        entities.DebtAdjustment.CpaSupType = db.GetNullableString(reader, 8);
        entities.DebtAdjustment.OtyType = db.GetInt32(reader, 9);
        entities.DebtAdjustment.DebtAdjustmentProcessDate =
          db.GetDate(reader, 10);
        entities.DebtAdjustment.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.DebtAdjustment.CpaType);
        CheckValid<ObligationTransaction>("Type1", entities.DebtAdjustment.Type1);
          
        CheckValid<ObligationTransaction>("DebtAdjustmentType",
          entities.DebtAdjustment.DebtAdjustmentType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.DebtAdjustment.CpaSupType);

        return true;
      });
  }

  private bool ReadDebtDetail1()
  {
    entities.DebtDetail.Populated = false;

    return Read("ReadDebtDetail1",
      (db, command) =>
      {
        db.SetString(command, "otrType", local.HardcodeOtrnTDebt.Type1);
        db.SetString(command, "cspNumber", export.PriCsePerson.Number);
        db.SetString(command, "cpaType", local.HardcodeCpaObligor.Type1);
        db.SetInt32(
          command, "otyType",
          export.PriObligationType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "obgGeneratedId",
          export.PriObligation.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 7);
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
      });
  }

  private bool ReadDebtDetail2()
  {
    entities.DebtDetail.Populated = false;

    return Read("ReadDebtDetail2",
      (db, command) =>
      {
        db.SetString(command, "otrType", local.HardcodeOtrnTDebt.Type1);
        db.SetString(command, "cspNumber", export.SecCsePerson.Number);
        db.SetString(command, "cpaType", local.HardcodeCpaObligor.Type1);
        db.SetInt32(
          command, "otyType",
          export.SecObligationType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "obgGeneratedId",
          export.SecObligation.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 7);
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
      });
  }

  private bool ReadLegalAction1()
  {
    System.Diagnostics.Debug.Assert(entities.PrimaryObligation.Populated);
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction1",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          entities.PrimaryObligation.LgaId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction2()
  {
    System.Diagnostics.Debug.Assert(entities.SecondaryObligation.Populated);
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction2",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          entities.SecondaryObligation.LgaId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadObligation1()
  {
    entities.PrimaryObligation.Populated = false;

    return Read("ReadObligation1",
      (db, command) =>
      {
        db.SetInt32(
          command, "obId", export.PriObligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "dtyGeneratedId",
          export.PriObligationType.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", export.PriCsePersonsWorkSet.Number);
        db.SetString(command, "cpaType", local.HardcodeCpaObligor.Type1);
      },
      (db, reader) =>
      {
        entities.PrimaryObligation.CpaType = db.GetString(reader, 0);
        entities.PrimaryObligation.CspNumber = db.GetString(reader, 1);
        entities.PrimaryObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.PrimaryObligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.PrimaryObligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.PrimaryObligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 5);
        entities.PrimaryObligation.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.PrimaryObligation.LastUpdateTmst =
          db.GetNullableDateTime(reader, 7);
        entities.PrimaryObligation.OrderTypeCode = db.GetString(reader, 8);
        entities.PrimaryObligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.PrimaryObligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.PrimaryObligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.PrimaryObligation.OrderTypeCode);
      });
  }

  private bool ReadObligation2()
  {
    entities.PrimaryObligation.Populated = false;

    return Read("ReadObligation2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.PriCsePerson.Number);
        db.SetInt32(
          command, "dtyGeneratedId",
          export.PriObligationType.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", local.HardcodeCpaObligor.Type1);
        db.SetInt32(
          command, "obId", export.PriObligation.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.PrimaryObligation.CpaType = db.GetString(reader, 0);
        entities.PrimaryObligation.CspNumber = db.GetString(reader, 1);
        entities.PrimaryObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.PrimaryObligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.PrimaryObligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.PrimaryObligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 5);
        entities.PrimaryObligation.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.PrimaryObligation.LastUpdateTmst =
          db.GetNullableDateTime(reader, 7);
        entities.PrimaryObligation.OrderTypeCode = db.GetString(reader, 8);
        entities.PrimaryObligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.PrimaryObligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.PrimaryObligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.PrimaryObligation.OrderTypeCode);
      });
  }

  private bool ReadObligation3()
  {
    entities.SecondaryObligation.Populated = false;

    return Read("ReadObligation3",
      (db, command) =>
      {
        db.SetInt32(
          command, "obId", export.SecObligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "dtyGeneratedId",
          export.SecObligationType.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", export.SecCsePersonsWorkSet.Number);
        db.SetString(command, "cpaType", local.HardcodeCpaObligor.Type1);
      },
      (db, reader) =>
      {
        entities.SecondaryObligation.CpaType = db.GetString(reader, 0);
        entities.SecondaryObligation.CspNumber = db.GetString(reader, 1);
        entities.SecondaryObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.SecondaryObligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.SecondaryObligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.SecondaryObligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 5);
        entities.SecondaryObligation.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.SecondaryObligation.LastUpdateTmst =
          db.GetNullableDateTime(reader, 7);
        entities.SecondaryObligation.OrderTypeCode = db.GetString(reader, 8);
        entities.SecondaryObligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.SecondaryObligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.SecondaryObligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.SecondaryObligation.OrderTypeCode);
      });
  }

  private bool ReadObligation4()
  {
    entities.SecondaryObligation.Populated = false;

    return Read("ReadObligation4",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.SecCsePerson.Number);
        db.SetInt32(
          command, "dtyGeneratedId",
          export.SecObligationType.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", local.HardcodeCpaObligor.Type1);
        db.SetInt32(
          command, "obId", export.SecObligation.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.SecondaryObligation.CpaType = db.GetString(reader, 0);
        entities.SecondaryObligation.CspNumber = db.GetString(reader, 1);
        entities.SecondaryObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.SecondaryObligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.SecondaryObligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.SecondaryObligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 5);
        entities.SecondaryObligation.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.SecondaryObligation.LastUpdateTmst =
          db.GetNullableDateTime(reader, 7);
        entities.SecondaryObligation.OrderTypeCode = db.GetString(reader, 8);
        entities.SecondaryObligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.SecondaryObligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.SecondaryObligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.SecondaryObligation.OrderTypeCode);
      });
  }

  private bool ReadObligationObligationRln()
  {
    System.Diagnostics.Debug.Assert(entities.PrimaryObligation.Populated);
    entities.SecondaryObligation.Populated = false;
    entities.ObligationRln.Populated = false;

    return Read("ReadObligationObligationRln",
      (db, command) =>
      {
        db.SetInt32(
          command, "otySecondId", entities.PrimaryObligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.PrimaryObligation.SystemGeneratedIdentifier);
        db.
          SetString(command, "cspNumber", entities.PrimaryObligation.CspNumber);
          
        db.SetString(command, "cpaType", entities.PrimaryObligation.CpaType);
      },
      (db, reader) =>
      {
        entities.SecondaryObligation.CpaType = db.GetString(reader, 0);
        entities.ObligationRln.CpaFType = db.GetString(reader, 0);
        entities.SecondaryObligation.CspNumber = db.GetString(reader, 1);
        entities.ObligationRln.CspFNumber = db.GetString(reader, 1);
        entities.SecondaryObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ObligationRln.ObgFGeneratedId = db.GetInt32(reader, 2);
        entities.SecondaryObligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationRln.OtyFirstId = db.GetInt32(reader, 3);
        entities.SecondaryObligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.SecondaryObligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 5);
        entities.SecondaryObligation.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.SecondaryObligation.LastUpdateTmst =
          db.GetNullableDateTime(reader, 7);
        entities.SecondaryObligation.OrderTypeCode = db.GetString(reader, 8);
        entities.ObligationRln.ObgGeneratedId = db.GetInt32(reader, 9);
        entities.ObligationRln.CspNumber = db.GetString(reader, 10);
        entities.ObligationRln.CpaType = db.GetString(reader, 11);
        entities.ObligationRln.OrrGeneratedId = db.GetInt32(reader, 12);
        entities.ObligationRln.CreatedBy = db.GetString(reader, 13);
        entities.ObligationRln.CreatedTmst = db.GetDateTime(reader, 14);
        entities.ObligationRln.OtySecondId = db.GetInt32(reader, 15);
        entities.ObligationRln.Description = db.GetString(reader, 16);
        entities.SecondaryObligation.Populated = true;
        entities.ObligationRln.Populated = true;
        CheckValid<Obligation>("CpaType", entities.SecondaryObligation.CpaType);
        CheckValid<ObligationRln>("CpaFType", entities.ObligationRln.CpaFType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.SecondaryObligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.SecondaryObligation.OrderTypeCode);
        CheckValid<ObligationRln>("CpaType", entities.ObligationRln.CpaType);
      });
  }

  private bool ReadObligationPaymentSchedule1()
  {
    entities.ObligationPaymentSchedule.Populated = false;

    return Read("ReadObligationPaymentSchedule1",
      (db, command) =>
      {
        db.SetString(command, "obgCspNumber", export.PriCsePerson.Number);
        db.SetString(command, "obgCpaType", local.HardcodeCpaObligor.Type1);
        db.SetInt32(
          command, "obgGeneratedId",
          export.PriObligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otyType",
          export.PriObligationType.SystemGeneratedIdentifier);
        db.SetNullableDate(
          command, "endDt", local.Current.Date.GetValueOrDefault());
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
        entities.ObligationPaymentSchedule.CreatedBy = db.GetString(reader, 7);
        entities.ObligationPaymentSchedule.CreatedTmst =
          db.GetDateTime(reader, 8);
        entities.ObligationPaymentSchedule.LastUpdateBy =
          db.GetNullableString(reader, 9);
        entities.ObligationPaymentSchedule.LastUpdateTmst =
          db.GetNullableDateTime(reader, 10);
        entities.ObligationPaymentSchedule.FrequencyCode =
          db.GetString(reader, 11);
        entities.ObligationPaymentSchedule.DayOfWeek =
          db.GetNullableInt32(reader, 12);
        entities.ObligationPaymentSchedule.DayOfMonth1 =
          db.GetNullableInt32(reader, 13);
        entities.ObligationPaymentSchedule.DayOfMonth2 =
          db.GetNullableInt32(reader, 14);
        entities.ObligationPaymentSchedule.PeriodInd =
          db.GetNullableString(reader, 15);
        entities.ObligationPaymentSchedule.RepaymentLetterPrintDate =
          db.GetNullableDate(reader, 16);
        entities.ObligationPaymentSchedule.Populated = true;
        CheckValid<ObligationPaymentSchedule>("ObgCpaType",
          entities.ObligationPaymentSchedule.ObgCpaType);
        CheckValid<ObligationPaymentSchedule>("FrequencyCode",
          entities.ObligationPaymentSchedule.FrequencyCode);
        CheckValid<ObligationPaymentSchedule>("PeriodInd",
          entities.ObligationPaymentSchedule.PeriodInd);
      });
  }

  private bool ReadObligationPaymentSchedule2()
  {
    entities.ObligationPaymentSchedule.Populated = false;

    return Read("ReadObligationPaymentSchedule2",
      (db, command) =>
      {
        db.SetString(command, "obgCspNumber", export.SecCsePerson.Number);
        db.SetString(command, "obgCpaType", local.HardcodeCpaObligor.Type1);
        db.SetInt32(
          command, "otyType",
          export.SecObligationType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "obgGeneratedId",
          export.SecObligation.SystemGeneratedIdentifier);
        db.SetNullableDate(
          command, "endDt", local.Current.Date.GetValueOrDefault());
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
        entities.ObligationPaymentSchedule.CreatedBy = db.GetString(reader, 7);
        entities.ObligationPaymentSchedule.CreatedTmst =
          db.GetDateTime(reader, 8);
        entities.ObligationPaymentSchedule.LastUpdateBy =
          db.GetNullableString(reader, 9);
        entities.ObligationPaymentSchedule.LastUpdateTmst =
          db.GetNullableDateTime(reader, 10);
        entities.ObligationPaymentSchedule.FrequencyCode =
          db.GetString(reader, 11);
        entities.ObligationPaymentSchedule.DayOfWeek =
          db.GetNullableInt32(reader, 12);
        entities.ObligationPaymentSchedule.DayOfMonth1 =
          db.GetNullableInt32(reader, 13);
        entities.ObligationPaymentSchedule.DayOfMonth2 =
          db.GetNullableInt32(reader, 14);
        entities.ObligationPaymentSchedule.PeriodInd =
          db.GetNullableString(reader, 15);
        entities.ObligationPaymentSchedule.RepaymentLetterPrintDate =
          db.GetNullableDate(reader, 16);
        entities.ObligationPaymentSchedule.Populated = true;
        CheckValid<ObligationPaymentSchedule>("ObgCpaType",
          entities.ObligationPaymentSchedule.ObgCpaType);
        CheckValid<ObligationPaymentSchedule>("FrequencyCode",
          entities.ObligationPaymentSchedule.FrequencyCode);
        CheckValid<ObligationPaymentSchedule>("PeriodInd",
          entities.ObligationPaymentSchedule.PeriodInd);
      });
  }

  private bool ReadObligationRln1()
  {
    entities.ObligationRln.Populated = false;

    return Read("ReadObligationRln1",
      (db, command) =>
      {
        db.SetInt32(
          command, "orrGeneratedId",
          local.HardcodeOrrPrimarySecondary.SequentialGeneratedIdentifier);
        db.SetInt32(
          command, "obgFGeneratedId",
          export.PriObligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otyFirstId",
          export.PriObligationType.SystemGeneratedIdentifier);
        db.SetString(command, "cspFNumber", export.PriCsePerson.Number);
        db.SetString(command, "cpaFType", local.HardcodeCpaObligor.Type1);
        db.SetInt32(
          command, "otySecondId",
          export.SecObligationType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "obgGeneratedId",
          export.SecObligation.SystemGeneratedIdentifier);
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
        entities.ObligationRln.CreatedTmst = db.GetDateTime(reader, 8);
        entities.ObligationRln.OtySecondId = db.GetInt32(reader, 9);
        entities.ObligationRln.OtyFirstId = db.GetInt32(reader, 10);
        entities.ObligationRln.Description = db.GetString(reader, 11);
        entities.ObligationRln.Populated = true;
        CheckValid<ObligationRln>("CpaType", entities.ObligationRln.CpaType);
        CheckValid<ObligationRln>("CpaFType", entities.ObligationRln.CpaFType);
      });
  }

  private bool ReadObligationRln2()
  {
    entities.ObligationRln.Populated = false;

    return Read("ReadObligationRln2",
      (db, command) =>
      {
        db.SetInt32(
          command, "obgFGeneratedId",
          export.PriObligation.SystemGeneratedIdentifier);
        db.SetString(command, "debtTypCd", export.PriObligationType.Code);
        db.SetString(command, "cspFNumber", export.PriCsePerson.Number);
        db.SetString(command, "cpaFType", local.HardcodeCpaObligor.Type1);
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
        entities.ObligationRln.CreatedTmst = db.GetDateTime(reader, 8);
        entities.ObligationRln.OtySecondId = db.GetInt32(reader, 9);
        entities.ObligationRln.OtyFirstId = db.GetInt32(reader, 10);
        entities.ObligationRln.Description = db.GetString(reader, 11);
        entities.ObligationRln.Populated = true;
        CheckValid<ObligationRln>("CpaType", entities.ObligationRln.CpaType);
        CheckValid<ObligationRln>("CpaFType", entities.ObligationRln.CpaFType);
      });
  }

  private bool ReadObligationRln3()
  {
    entities.ObligationRln.Populated = false;

    return Read("ReadObligationRln3",
      (db, command) =>
      {
        db.SetInt32(
          command, "obgFGeneratedId",
          export.SecObligation.SystemGeneratedIdentifier);
        db.SetString(command, "debtTypCd", export.SecObligationType.Code);
        db.SetString(command, "cspFNumber", export.SecCsePerson.Number);
        db.SetString(command, "cpaFType", local.HardcodeCpaObligor.Type1);
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
        entities.ObligationRln.CreatedTmst = db.GetDateTime(reader, 8);
        entities.ObligationRln.OtySecondId = db.GetInt32(reader, 9);
        entities.ObligationRln.OtyFirstId = db.GetInt32(reader, 10);
        entities.ObligationRln.Description = db.GetString(reader, 11);
        entities.ObligationRln.Populated = true;
        CheckValid<ObligationRln>("CpaType", entities.ObligationRln.CpaType);
        CheckValid<ObligationRln>("CpaFType", entities.ObligationRln.CpaFType);
      });
  }

  private bool ReadObligationRln4()
  {
    entities.ObligationRln.Populated = false;

    return Read("ReadObligationRln4",
      (db, command) =>
      {
        db.SetInt32(
          command, "obgGeneratedId",
          export.PriObligation.SystemGeneratedIdentifier);
        db.SetString(command, "code", export.PriObligationType.Code);
        db.SetString(command, "cspNumber", export.PriCsePerson.Number);
        db.SetString(command, "cpaType", local.HardcodeCpaObligor.Type1);
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
        entities.ObligationRln.CreatedTmst = db.GetDateTime(reader, 8);
        entities.ObligationRln.OtySecondId = db.GetInt32(reader, 9);
        entities.ObligationRln.OtyFirstId = db.GetInt32(reader, 10);
        entities.ObligationRln.Description = db.GetString(reader, 11);
        entities.ObligationRln.Populated = true;
        CheckValid<ObligationRln>("CpaType", entities.ObligationRln.CpaType);
        CheckValid<ObligationRln>("CpaFType", entities.ObligationRln.CpaFType);
      });
  }

  private bool ReadObligationRln5()
  {
    entities.ObligationRln.Populated = false;

    return Read("ReadObligationRln5",
      (db, command) =>
      {
        db.SetInt32(
          command, "obgGeneratedId",
          export.SecObligation.SystemGeneratedIdentifier);
        db.SetString(command, "debtTypCd", export.SecObligationType.Code);
        db.SetString(command, "cspNumber", export.SecCsePerson.Number);
        db.SetString(command, "cpaType", local.HardcodeCpaObligor.Type1);
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
        entities.ObligationRln.CreatedTmst = db.GetDateTime(reader, 8);
        entities.ObligationRln.OtySecondId = db.GetInt32(reader, 9);
        entities.ObligationRln.OtyFirstId = db.GetInt32(reader, 10);
        entities.ObligationRln.Description = db.GetString(reader, 11);
        entities.ObligationRln.Populated = true;
        CheckValid<ObligationRln>("CpaType", entities.ObligationRln.CpaType);
        CheckValid<ObligationRln>("CpaFType", entities.ObligationRln.CpaFType);
      });
  }

  private bool ReadObligationRlnObligation1()
  {
    System.Diagnostics.Debug.Assert(entities.SecondaryObligation.Populated);
    entities.PrimaryObligation.Populated = false;
    entities.ObligationRln.Populated = false;

    return Read("ReadObligationRlnObligation1",
      (db, command) =>
      {
        db.SetInt32(
          command, "orrGeneratedId",
          local.HardcodeOrrPrimarySecondary.SequentialGeneratedIdentifier);
        db.SetInt32(
          command, "otySecondId", entities.SecondaryObligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.SecondaryObligation.SystemGeneratedIdentifier);
        db.SetString(
          command, "cspNumber", entities.SecondaryObligation.CspNumber);
        db.SetString(command, "cpaType", entities.SecondaryObligation.CpaType);
      },
      (db, reader) =>
      {
        entities.ObligationRln.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ObligationRln.CspNumber = db.GetString(reader, 1);
        entities.ObligationRln.CpaType = db.GetString(reader, 2);
        entities.ObligationRln.ObgFGeneratedId = db.GetInt32(reader, 3);
        entities.PrimaryObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ObligationRln.CspFNumber = db.GetString(reader, 4);
        entities.PrimaryObligation.CspNumber = db.GetString(reader, 4);
        entities.ObligationRln.CpaFType = db.GetString(reader, 5);
        entities.PrimaryObligation.CpaType = db.GetString(reader, 5);
        entities.ObligationRln.OrrGeneratedId = db.GetInt32(reader, 6);
        entities.ObligationRln.CreatedBy = db.GetString(reader, 7);
        entities.ObligationRln.CreatedTmst = db.GetDateTime(reader, 8);
        entities.ObligationRln.OtySecondId = db.GetInt32(reader, 9);
        entities.ObligationRln.OtyFirstId = db.GetInt32(reader, 10);
        entities.PrimaryObligation.DtyGeneratedId = db.GetInt32(reader, 10);
        entities.ObligationRln.Description = db.GetString(reader, 11);
        entities.PrimaryObligation.LgaId = db.GetNullableInt32(reader, 12);
        entities.PrimaryObligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 13);
        entities.PrimaryObligation.LastUpdatedBy =
          db.GetNullableString(reader, 14);
        entities.PrimaryObligation.LastUpdateTmst =
          db.GetNullableDateTime(reader, 15);
        entities.PrimaryObligation.OrderTypeCode = db.GetString(reader, 16);
        entities.PrimaryObligation.Populated = true;
        entities.ObligationRln.Populated = true;
        CheckValid<ObligationRln>("CpaType", entities.ObligationRln.CpaType);
        CheckValid<ObligationRln>("CpaFType", entities.ObligationRln.CpaFType);
        CheckValid<Obligation>("CpaType", entities.PrimaryObligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.PrimaryObligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.PrimaryObligation.OrderTypeCode);
      });
  }

  private bool ReadObligationRlnObligation2()
  {
    System.Diagnostics.Debug.Assert(entities.PrimaryObligation.Populated);
    entities.SecondaryObligation.Populated = false;
    entities.ObligationRln.Populated = false;

    return Read("ReadObligationRlnObligation2",
      (db, command) =>
      {
        db.SetInt32(
          command, "orrGeneratedId",
          local.HardcodeOrrPrimarySecondary.SequentialGeneratedIdentifier);
        db.SetInt32(
          command, "otyFirstId", entities.PrimaryObligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgFGeneratedId",
          entities.PrimaryObligation.SystemGeneratedIdentifier);
        db.
          SetString(command, "cspFNumber", entities.PrimaryObligation.CspNumber);
          
        db.SetString(command, "cpaFType", entities.PrimaryObligation.CpaType);
      },
      (db, reader) =>
      {
        entities.ObligationRln.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.SecondaryObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationRln.CspNumber = db.GetString(reader, 1);
        entities.SecondaryObligation.CspNumber = db.GetString(reader, 1);
        entities.ObligationRln.CpaType = db.GetString(reader, 2);
        entities.SecondaryObligation.CpaType = db.GetString(reader, 2);
        entities.ObligationRln.ObgFGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationRln.CspFNumber = db.GetString(reader, 4);
        entities.ObligationRln.CpaFType = db.GetString(reader, 5);
        entities.ObligationRln.OrrGeneratedId = db.GetInt32(reader, 6);
        entities.ObligationRln.CreatedBy = db.GetString(reader, 7);
        entities.ObligationRln.CreatedTmst = db.GetDateTime(reader, 8);
        entities.ObligationRln.OtySecondId = db.GetInt32(reader, 9);
        entities.SecondaryObligation.DtyGeneratedId = db.GetInt32(reader, 9);
        entities.ObligationRln.OtyFirstId = db.GetInt32(reader, 10);
        entities.ObligationRln.Description = db.GetString(reader, 11);
        entities.SecondaryObligation.LgaId = db.GetNullableInt32(reader, 12);
        entities.SecondaryObligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 13);
        entities.SecondaryObligation.LastUpdatedBy =
          db.GetNullableString(reader, 14);
        entities.SecondaryObligation.LastUpdateTmst =
          db.GetNullableDateTime(reader, 15);
        entities.SecondaryObligation.OrderTypeCode = db.GetString(reader, 16);
        entities.SecondaryObligation.Populated = true;
        entities.ObligationRln.Populated = true;
        CheckValid<ObligationRln>("CpaType", entities.ObligationRln.CpaType);
        CheckValid<Obligation>("CpaType", entities.SecondaryObligation.CpaType);
        CheckValid<ObligationRln>("CpaFType", entities.ObligationRln.CpaFType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.SecondaryObligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.SecondaryObligation.OrderTypeCode);
      });
  }

  private bool ReadObligationRlnRsn()
  {
    entities.ObligationRlnRsn.Populated = false;

    return Read("ReadObligationRlnRsn",
      (db, command) =>
      {
        db.SetInt32(
          command, "obRlnRsnId",
          local.HardcodeOrrPrimarySecondary.SequentialGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ObligationRlnRsn.SequentialGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationRlnRsn.Populated = true;
      });
  }

  private IEnumerable<bool> ReadObligationTransaction1()
  {
    System.Diagnostics.Debug.Assert(entities.PrimaryObligation.Populated);
    entities.ObligationTransaction.Populated = false;

    return ReadEach("ReadObligationTransaction1",
      (db, command) =>
      {
        db.SetInt32(
          command, "otyType", entities.PrimaryObligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.PrimaryObligation.SystemGeneratedIdentifier);
        db.
          SetString(command, "cspNumber", entities.PrimaryObligation.CspNumber);
          
        db.SetString(command, "cpaType", entities.PrimaryObligation.CpaType);
        db.SetString(command, "obTrnTyp", local.HardcodeOtrnTDebt.Type1);
        db.
          SetString(command, "debtTyp", export.PriObligationType.Classification);
          
      },
      (db, reader) =>
      {
        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 1);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 2);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 4);
        entities.ObligationTransaction.Amount = db.GetDecimal(reader, 5);
        entities.ObligationTransaction.DebtType = db.GetString(reader, 6);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 7);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 8);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 9);
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

  private IEnumerable<bool> ReadObligationTransaction2()
  {
    return ReadEach("ReadObligationTransaction2",
      (db, command) =>
      {
        db.SetString(command, "obTrnTyp", local.HardcodeOtrnTDebt.Type1);
        db.SetString(command, "cspNumber", export.PriCsePerson.Number);
        db.SetString(command, "cpaType", local.HardcodeCpaObligor.Type1);
        db.SetInt32(
          command, "otyType",
          export.PriObligationType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "obgGeneratedId",
          export.PriObligation.SystemGeneratedIdentifier);
        db.
          SetString(command, "debtTyp", export.PriObligationType.Classification);
          
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 1);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 2);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 4);
        entities.ObligationTransaction.Amount = db.GetDecimal(reader, 5);
        entities.ObligationTransaction.DebtType = db.GetString(reader, 6);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 7);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 8);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 9);
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

  private IEnumerable<bool> ReadObligationTransaction3()
  {
    return ReadEach("ReadObligationTransaction3",
      (db, command) =>
      {
        db.SetString(command, "obTrnTyp", local.HardcodeOtrnTDebt.Type1);
        db.SetString(command, "cspNumber", export.SecCsePersonsWorkSet.Number);
        db.SetString(command, "cpaType", local.HardcodeCpaObligor.Type1);
        db.SetInt32(
          command, "otyType",
          export.SecObligationType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "obgGeneratedId",
          export.SecObligation.SystemGeneratedIdentifier);
        db.
          SetString(command, "debtTyp", export.SecObligationType.Classification);
          
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 1);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 2);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 4);
        entities.ObligationTransaction.Amount = db.GetDecimal(reader, 5);
        entities.ObligationTransaction.DebtType = db.GetString(reader, 6);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 7);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 8);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 9);
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

  private IEnumerable<bool> ReadObligationTransaction4()
  {
    return ReadEach("ReadObligationTransaction4",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.SecCsePerson.Number);
        db.SetString(command, "cpaType", local.HardcodeCpaObligor.Type1);
        db.SetInt32(
          command, "otyType",
          export.SecObligationType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "obgGeneratedId",
          export.SecObligation.SystemGeneratedIdentifier);
        db.
          SetString(command, "debtTyp", export.SecObligationType.Classification);
          
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 1);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 2);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 4);
        entities.ObligationTransaction.Amount = db.GetDecimal(reader, 5);
        entities.ObligationTransaction.DebtType = db.GetString(reader, 6);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 7);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 8);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 9);
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

  private void UpdateObligation1()
  {
    System.Diagnostics.Debug.Assert(entities.PrimaryObligation.Populated);

    var primarySecondaryCode =
      local.HardcodeObligPrimaryConcurrnt.PrimarySecondaryCode ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdateTmst = local.Current.Timestamp;

    CheckValid<Obligation>("PrimarySecondaryCode", primarySecondaryCode);
    entities.PrimaryObligation.Populated = false;
    Update("UpdateObligation1",
      (db, command) =>
      {
        db.SetNullableString(command, "primSecCd", primarySecondaryCode);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdateTmst", lastUpdateTmst);
        db.SetString(command, "cpaType", entities.PrimaryObligation.CpaType);
        db.
          SetString(command, "cspNumber", entities.PrimaryObligation.CspNumber);
          
        db.SetInt32(
          command, "obId",
          entities.PrimaryObligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "dtyGeneratedId", entities.PrimaryObligation.DtyGeneratedId);
          
      });

    entities.PrimaryObligation.PrimarySecondaryCode = primarySecondaryCode;
    entities.PrimaryObligation.LastUpdatedBy = lastUpdatedBy;
    entities.PrimaryObligation.LastUpdateTmst = lastUpdateTmst;
    entities.PrimaryObligation.Populated = true;
  }

  private void UpdateObligation2()
  {
    System.Diagnostics.Debug.Assert(entities.PrimaryObligation.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdateTmst = local.Current.Timestamp;

    CheckValid<Obligation>("PrimarySecondaryCode", "");
    entities.PrimaryObligation.Populated = false;
    Update("UpdateObligation2",
      (db, command) =>
      {
        db.SetNullableString(command, "primSecCd", "");
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdateTmst", lastUpdateTmst);
        db.SetString(command, "cpaType", entities.PrimaryObligation.CpaType);
        db.
          SetString(command, "cspNumber", entities.PrimaryObligation.CspNumber);
          
        db.SetInt32(
          command, "obId",
          entities.PrimaryObligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "dtyGeneratedId", entities.PrimaryObligation.DtyGeneratedId);
          
      });

    entities.PrimaryObligation.PrimarySecondaryCode = "";
    entities.PrimaryObligation.LastUpdatedBy = lastUpdatedBy;
    entities.PrimaryObligation.LastUpdateTmst = lastUpdateTmst;
    entities.PrimaryObligation.Populated = true;
  }

  private void UpdateObligation3()
  {
    System.Diagnostics.Debug.Assert(entities.SecondaryObligation.Populated);

    var primarySecondaryCode =
      local.HardcodeObligSecondaryConcrnt.PrimarySecondaryCode ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdateTmst = local.Current.Timestamp;

    CheckValid<Obligation>("PrimarySecondaryCode", primarySecondaryCode);
    entities.SecondaryObligation.Populated = false;
    Update("UpdateObligation3",
      (db, command) =>
      {
        db.SetNullableString(command, "primSecCd", primarySecondaryCode);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdateTmst", lastUpdateTmst);
        db.SetString(command, "cpaType", entities.SecondaryObligation.CpaType);
        db.SetString(
          command, "cspNumber", entities.SecondaryObligation.CspNumber);
        db.SetInt32(
          command, "obId",
          entities.SecondaryObligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "dtyGeneratedId",
          entities.SecondaryObligation.DtyGeneratedId);
      });

    entities.SecondaryObligation.PrimarySecondaryCode = primarySecondaryCode;
    entities.SecondaryObligation.LastUpdatedBy = lastUpdatedBy;
    entities.SecondaryObligation.LastUpdateTmst = lastUpdateTmst;
    entities.SecondaryObligation.Populated = true;
  }

  private void UpdateObligation4()
  {
    System.Diagnostics.Debug.Assert(entities.SecondaryObligation.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdateTmst = local.Current.Timestamp;

    CheckValid<Obligation>("PrimarySecondaryCode", "");
    entities.SecondaryObligation.Populated = false;
    Update("UpdateObligation4",
      (db, command) =>
      {
        db.SetNullableString(command, "primSecCd", "");
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdateTmst", lastUpdateTmst);
        db.SetString(command, "cpaType", entities.SecondaryObligation.CpaType);
        db.SetString(
          command, "cspNumber", entities.SecondaryObligation.CspNumber);
        db.SetInt32(
          command, "obId",
          entities.SecondaryObligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "dtyGeneratedId",
          entities.SecondaryObligation.DtyGeneratedId);
      });

    entities.SecondaryObligation.PrimarySecondaryCode = "";
    entities.SecondaryObligation.LastUpdatedBy = lastUpdatedBy;
    entities.SecondaryObligation.LastUpdateTmst = lastUpdateTmst;
    entities.SecondaryObligation.Populated = true;
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
      /// Gets a value of Of.
      /// </summary>
      [JsonIgnore]
      public Array<OfGroup> Of => of ??= new(OfGroup.Capacity);

      /// <summary>
      /// Gets a value of Of for json serialization.
      /// </summary>
      [JsonPropertyName("of")]
      [Computed]
      public IList<OfGroup> Of_Json
      {
        get => of;
        set => Of.Assign(value);
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 8;

      private ObligationTransaction obligationTransaction;
      private AccrualInstructions accrualInstructions;
      private Array<OfGroup> of;
    }

    /// <summary>A OfGroup group.</summary>
    [Serializable]
    public class OfGroup
    {
      /// <summary>
      /// A value of Of1.
      /// </summary>
      [JsonPropertyName("of1")]
      public AccrualSuspension Of1
      {
        get => of1 ??= new();
        set => of1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private AccrualSuspension of1;
    }

    /// <summary>
    /// A value of RelEst.
    /// </summary>
    [JsonPropertyName("relEst")]
    public DateWorkArea RelEst
    {
      get => relEst ??= new();
      set => relEst = value;
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
    /// A value of PriAccrual.
    /// </summary>
    [JsonPropertyName("priAccrual")]
    public ObligationTransaction PriAccrual
    {
      get => priAccrual ??= new();
      set => priAccrual = value;
    }

    /// <summary>
    /// A value of SecAccrual.
    /// </summary>
    [JsonPropertyName("secAccrual")]
    public ObligationTransaction SecAccrual
    {
      get => secAccrual ??= new();
      set => secAccrual = value;
    }

    /// <summary>
    /// A value of SecAccrualDue.
    /// </summary>
    [JsonPropertyName("secAccrualDue")]
    public DateWorkArea SecAccrualDue
    {
      get => secAccrualDue ??= new();
      set => secAccrualDue = value;
    }

    /// <summary>
    /// A value of PriAccrualDue.
    /// </summary>
    [JsonPropertyName("priAccrualDue")]
    public DateWorkArea PriAccrualDue
    {
      get => priAccrualDue ??= new();
      set => priAccrualDue = value;
    }

    /// <summary>
    /// A value of PriScreenOwedAmounts.
    /// </summary>
    [JsonPropertyName("priScreenOwedAmounts")]
    public ScreenOwedAmounts PriScreenOwedAmounts
    {
      get => priScreenOwedAmounts ??= new();
      set => priScreenOwedAmounts = value;
    }

    /// <summary>
    /// A value of SecScreenOwedAmounts.
    /// </summary>
    [JsonPropertyName("secScreenOwedAmounts")]
    public ScreenOwedAmounts SecScreenOwedAmounts
    {
      get => secScreenOwedAmounts ??= new();
      set => secScreenOwedAmounts = value;
    }

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
    /// A value of SecAccrualInstructions.
    /// </summary>
    [JsonPropertyName("secAccrualInstructions")]
    public AccrualInstructions SecAccrualInstructions
    {
      get => secAccrualInstructions ??= new();
      set => secAccrualInstructions = value;
    }

    /// <summary>
    /// A value of PriAccrualInstructions.
    /// </summary>
    [JsonPropertyName("priAccrualInstructions")]
    public AccrualInstructions PriAccrualInstructions
    {
      get => priAccrualInstructions ??= new();
      set => priAccrualInstructions = value;
    }

    /// <summary>
    /// A value of ObligActivity.
    /// </summary>
    [JsonPropertyName("obligActivity")]
    public Common ObligActivity
    {
      get => obligActivity ??= new();
      set => obligActivity = value;
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
    /// A value of SecPayor.
    /// </summary>
    [JsonPropertyName("secPayor")]
    public Standard SecPayor
    {
      get => secPayor ??= new();
      set => secPayor = value;
    }

    /// <summary>
    /// A value of PriPayor.
    /// </summary>
    [JsonPropertyName("priPayor")]
    public Standard PriPayor
    {
      get => priPayor ??= new();
      set => priPayor = value;
    }

    /// <summary>
    /// A value of SecCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("secCsePersonsWorkSet")]
    public CsePersonsWorkSet SecCsePersonsWorkSet
    {
      get => secCsePersonsWorkSet ??= new();
      set => secCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of PriCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("priCsePersonsWorkSet")]
    public CsePersonsWorkSet PriCsePersonsWorkSet
    {
      get => priCsePersonsWorkSet ??= new();
      set => priCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of SecFrequencyWorkSet.
    /// </summary>
    [JsonPropertyName("secFrequencyWorkSet")]
    public FrequencyWorkSet SecFrequencyWorkSet
    {
      get => secFrequencyWorkSet ??= new();
      set => secFrequencyWorkSet = value;
    }

    /// <summary>
    /// A value of PriFrequencyWorkSet.
    /// </summary>
    [JsonPropertyName("priFrequencyWorkSet")]
    public FrequencyWorkSet PriFrequencyWorkSet
    {
      get => priFrequencyWorkSet ??= new();
      set => priFrequencyWorkSet = value;
    }

    /// <summary>
    /// A value of SecDebtDetail.
    /// </summary>
    [JsonPropertyName("secDebtDetail")]
    public DebtDetail SecDebtDetail
    {
      get => secDebtDetail ??= new();
      set => secDebtDetail = value;
    }

    /// <summary>
    /// A value of SecLegalAction.
    /// </summary>
    [JsonPropertyName("secLegalAction")]
    public LegalAction SecLegalAction
    {
      get => secLegalAction ??= new();
      set => secLegalAction = value;
    }

    /// <summary>
    /// A value of SecObligationType.
    /// </summary>
    [JsonPropertyName("secObligationType")]
    public ObligationType SecObligationType
    {
      get => secObligationType ??= new();
      set => secObligationType = value;
    }

    /// <summary>
    /// A value of SecCsePerson.
    /// </summary>
    [JsonPropertyName("secCsePerson")]
    public CsePerson SecCsePerson
    {
      get => secCsePerson ??= new();
      set => secCsePerson = value;
    }

    /// <summary>
    /// A value of SecCsePersonAccount.
    /// </summary>
    [JsonPropertyName("secCsePersonAccount")]
    public CsePersonAccount SecCsePersonAccount
    {
      get => secCsePersonAccount ??= new();
      set => secCsePersonAccount = value;
    }

    /// <summary>
    /// A value of SecObligation.
    /// </summary>
    [JsonPropertyName("secObligation")]
    public Obligation SecObligation
    {
      get => secObligation ??= new();
      set => secObligation = value;
    }

    /// <summary>
    /// A value of PriDebtDetail.
    /// </summary>
    [JsonPropertyName("priDebtDetail")]
    public DebtDetail PriDebtDetail
    {
      get => priDebtDetail ??= new();
      set => priDebtDetail = value;
    }

    /// <summary>
    /// A value of PriLegalAction.
    /// </summary>
    [JsonPropertyName("priLegalAction")]
    public LegalAction PriLegalAction
    {
      get => priLegalAction ??= new();
      set => priLegalAction = value;
    }

    /// <summary>
    /// A value of PriObligationType.
    /// </summary>
    [JsonPropertyName("priObligationType")]
    public ObligationType PriObligationType
    {
      get => priObligationType ??= new();
      set => priObligationType = value;
    }

    /// <summary>
    /// A value of PriCsePerson.
    /// </summary>
    [JsonPropertyName("priCsePerson")]
    public CsePerson PriCsePerson
    {
      get => priCsePerson ??= new();
      set => priCsePerson = value;
    }

    /// <summary>
    /// A value of PriCsePersonAccount.
    /// </summary>
    [JsonPropertyName("priCsePersonAccount")]
    public CsePersonAccount PriCsePersonAccount
    {
      get => priCsePersonAccount ??= new();
      set => priCsePersonAccount = value;
    }

    /// <summary>
    /// A value of PriObligation.
    /// </summary>
    [JsonPropertyName("priObligation")]
    public Obligation PriObligation
    {
      get => priObligation ??= new();
      set => priObligation = value;
    }

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
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
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of ScreenOwedAmounts.
    /// </summary>
    [JsonPropertyName("screenOwedAmounts")]
    public ScreenOwedAmounts ScreenOwedAmounts
    {
      get => screenOwedAmounts ??= new();
      set => screenOwedAmounts = value;
    }

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of FromList.
    /// </summary>
    [JsonPropertyName("fromList")]
    public CsePerson FromList
    {
      get => fromList ??= new();
      set => fromList = value;
    }

    private DateWorkArea relEst;
    private ObligationTransaction obligationTransaction;
    private Array<GroupGroup> group;
    private ObligationTransaction priAccrual;
    private ObligationTransaction secAccrual;
    private DateWorkArea secAccrualDue;
    private DateWorkArea priAccrualDue;
    private ScreenOwedAmounts priScreenOwedAmounts;
    private ScreenOwedAmounts secScreenOwedAmounts;
    private AccrualSuspension accrualSuspension;
    private AccrualInstructions secAccrualInstructions;
    private AccrualInstructions priAccrualInstructions;
    private Common obligActivity;
    private ObligationRln obligationRln;
    private Standard secPayor;
    private Standard priPayor;
    private CsePersonsWorkSet secCsePersonsWorkSet;
    private CsePersonsWorkSet priCsePersonsWorkSet;
    private FrequencyWorkSet secFrequencyWorkSet;
    private FrequencyWorkSet priFrequencyWorkSet;
    private DebtDetail secDebtDetail;
    private LegalAction secLegalAction;
    private ObligationType secObligationType;
    private CsePerson secCsePerson;
    private CsePersonAccount secCsePersonAccount;
    private Obligation secObligation;
    private DebtDetail priDebtDetail;
    private LegalAction priLegalAction;
    private ObligationType priObligationType;
    private CsePerson priCsePerson;
    private CsePersonAccount priCsePersonAccount;
    private Obligation priObligation;
    private NextTranInfo hidden;
    private CsePerson csePerson;
    private CsePersonAccount csePersonAccount;
    private CsePersonsWorkSet csePersonsWorkSet;
    private LegalAction legalAction;
    private Obligation obligation;
    private ObligationType obligationType;
    private ScreenOwedAmounts screenOwedAmounts;
    private Standard standard;
    private CsePerson fromList;
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
      /// Gets a value of Of.
      /// </summary>
      [JsonIgnore]
      public Array<OfGroup> Of => of ??= new(OfGroup.Capacity);

      /// <summary>
      /// Gets a value of Of for json serialization.
      /// </summary>
      [JsonPropertyName("of")]
      [Computed]
      public IList<OfGroup> Of_Json
      {
        get => of;
        set => Of.Assign(value);
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 8;

      private ObligationTransaction obligationTransaction;
      private AccrualInstructions accrualInstructions;
      private Array<OfGroup> of;
    }

    /// <summary>A OfGroup group.</summary>
    [Serializable]
    public class OfGroup
    {
      /// <summary>
      /// A value of Of1.
      /// </summary>
      [JsonPropertyName("of1")]
      public AccrualSuspension Of1
      {
        get => of1 ??= new();
        set => of1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private AccrualSuspension of1;
    }

    /// <summary>
    /// A value of RelEst.
    /// </summary>
    [JsonPropertyName("relEst")]
    public DateWorkArea RelEst
    {
      get => relEst ??= new();
      set => relEst = value;
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
    /// A value of PriAccrual.
    /// </summary>
    [JsonPropertyName("priAccrual")]
    public ObligationTransaction PriAccrual
    {
      get => priAccrual ??= new();
      set => priAccrual = value;
    }

    /// <summary>
    /// A value of SecAccrual.
    /// </summary>
    [JsonPropertyName("secAccrual")]
    public ObligationTransaction SecAccrual
    {
      get => secAccrual ??= new();
      set => secAccrual = value;
    }

    /// <summary>
    /// A value of SecAccrualDue.
    /// </summary>
    [JsonPropertyName("secAccrualDue")]
    public DateWorkArea SecAccrualDue
    {
      get => secAccrualDue ??= new();
      set => secAccrualDue = value;
    }

    /// <summary>
    /// A value of PriAccrualDue.
    /// </summary>
    [JsonPropertyName("priAccrualDue")]
    public DateWorkArea PriAccrualDue
    {
      get => priAccrualDue ??= new();
      set => priAccrualDue = value;
    }

    /// <summary>
    /// A value of PriScreenOwedAmounts.
    /// </summary>
    [JsonPropertyName("priScreenOwedAmounts")]
    public ScreenOwedAmounts PriScreenOwedAmounts
    {
      get => priScreenOwedAmounts ??= new();
      set => priScreenOwedAmounts = value;
    }

    /// <summary>
    /// A value of SecScreenOwedAmounts.
    /// </summary>
    [JsonPropertyName("secScreenOwedAmounts")]
    public ScreenOwedAmounts SecScreenOwedAmounts
    {
      get => secScreenOwedAmounts ??= new();
      set => secScreenOwedAmounts = value;
    }

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
    /// A value of SecAccrualInstructions.
    /// </summary>
    [JsonPropertyName("secAccrualInstructions")]
    public AccrualInstructions SecAccrualInstructions
    {
      get => secAccrualInstructions ??= new();
      set => secAccrualInstructions = value;
    }

    /// <summary>
    /// A value of PriAccrualInstructions.
    /// </summary>
    [JsonPropertyName("priAccrualInstructions")]
    public AccrualInstructions PriAccrualInstructions
    {
      get => priAccrualInstructions ??= new();
      set => priAccrualInstructions = value;
    }

    /// <summary>
    /// A value of ObigActivity.
    /// </summary>
    [JsonPropertyName("obigActivity")]
    public Common ObigActivity
    {
      get => obigActivity ??= new();
      set => obigActivity = value;
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
    /// A value of PriPayor.
    /// </summary>
    [JsonPropertyName("priPayor")]
    public Standard PriPayor
    {
      get => priPayor ??= new();
      set => priPayor = value;
    }

    /// <summary>
    /// A value of SecPayor.
    /// </summary>
    [JsonPropertyName("secPayor")]
    public Standard SecPayor
    {
      get => secPayor ??= new();
      set => secPayor = value;
    }

    /// <summary>
    /// A value of PriCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("priCsePersonsWorkSet")]
    public CsePersonsWorkSet PriCsePersonsWorkSet
    {
      get => priCsePersonsWorkSet ??= new();
      set => priCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of SecCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("secCsePersonsWorkSet")]
    public CsePersonsWorkSet SecCsePersonsWorkSet
    {
      get => secCsePersonsWorkSet ??= new();
      set => secCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of SecFrequencyWorkSet.
    /// </summary>
    [JsonPropertyName("secFrequencyWorkSet")]
    public FrequencyWorkSet SecFrequencyWorkSet
    {
      get => secFrequencyWorkSet ??= new();
      set => secFrequencyWorkSet = value;
    }

    /// <summary>
    /// A value of PriFrequencyWorkSet.
    /// </summary>
    [JsonPropertyName("priFrequencyWorkSet")]
    public FrequencyWorkSet PriFrequencyWorkSet
    {
      get => priFrequencyWorkSet ??= new();
      set => priFrequencyWorkSet = value;
    }

    /// <summary>
    /// A value of PriDebtDetail.
    /// </summary>
    [JsonPropertyName("priDebtDetail")]
    public DebtDetail PriDebtDetail
    {
      get => priDebtDetail ??= new();
      set => priDebtDetail = value;
    }

    /// <summary>
    /// A value of PriLegalAction.
    /// </summary>
    [JsonPropertyName("priLegalAction")]
    public LegalAction PriLegalAction
    {
      get => priLegalAction ??= new();
      set => priLegalAction = value;
    }

    /// <summary>
    /// A value of PriObligationType.
    /// </summary>
    [JsonPropertyName("priObligationType")]
    public ObligationType PriObligationType
    {
      get => priObligationType ??= new();
      set => priObligationType = value;
    }

    /// <summary>
    /// A value of PriCsePerson.
    /// </summary>
    [JsonPropertyName("priCsePerson")]
    public CsePerson PriCsePerson
    {
      get => priCsePerson ??= new();
      set => priCsePerson = value;
    }

    /// <summary>
    /// A value of PriCsePersonAccount.
    /// </summary>
    [JsonPropertyName("priCsePersonAccount")]
    public CsePersonAccount PriCsePersonAccount
    {
      get => priCsePersonAccount ??= new();
      set => priCsePersonAccount = value;
    }

    /// <summary>
    /// A value of PriObligation.
    /// </summary>
    [JsonPropertyName("priObligation")]
    public Obligation PriObligation
    {
      get => priObligation ??= new();
      set => priObligation = value;
    }

    /// <summary>
    /// A value of SecDebtDetail.
    /// </summary>
    [JsonPropertyName("secDebtDetail")]
    public DebtDetail SecDebtDetail
    {
      get => secDebtDetail ??= new();
      set => secDebtDetail = value;
    }

    /// <summary>
    /// A value of SecLegalAction.
    /// </summary>
    [JsonPropertyName("secLegalAction")]
    public LegalAction SecLegalAction
    {
      get => secLegalAction ??= new();
      set => secLegalAction = value;
    }

    /// <summary>
    /// A value of SecObligationType.
    /// </summary>
    [JsonPropertyName("secObligationType")]
    public ObligationType SecObligationType
    {
      get => secObligationType ??= new();
      set => secObligationType = value;
    }

    /// <summary>
    /// A value of SecCsePerson.
    /// </summary>
    [JsonPropertyName("secCsePerson")]
    public CsePerson SecCsePerson
    {
      get => secCsePerson ??= new();
      set => secCsePerson = value;
    }

    /// <summary>
    /// A value of SecCsePersonAccount.
    /// </summary>
    [JsonPropertyName("secCsePersonAccount")]
    public CsePersonAccount SecCsePersonAccount
    {
      get => secCsePersonAccount ??= new();
      set => secCsePersonAccount = value;
    }

    /// <summary>
    /// A value of SecObligation.
    /// </summary>
    [JsonPropertyName("secObligation")]
    public Obligation SecObligation
    {
      get => secObligation ??= new();
      set => secObligation = value;
    }

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
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
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
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
    /// A value of ObligationAmt.
    /// </summary>
    [JsonPropertyName("obligationAmt")]
    public Common ObligationAmt
    {
      get => obligationAmt ??= new();
      set => obligationAmt = value;
    }

    private DateWorkArea relEst;
    private Array<GroupGroup> group;
    private ObligationTransaction priAccrual;
    private ObligationTransaction secAccrual;
    private DateWorkArea secAccrualDue;
    private DateWorkArea priAccrualDue;
    private ScreenOwedAmounts priScreenOwedAmounts;
    private ScreenOwedAmounts secScreenOwedAmounts;
    private AccrualSuspension accrualSuspension;
    private AccrualInstructions secAccrualInstructions;
    private AccrualInstructions priAccrualInstructions;
    private Common obigActivity;
    private ObligationRln obligationRln;
    private Standard priPayor;
    private Standard secPayor;
    private CsePersonsWorkSet priCsePersonsWorkSet;
    private CsePersonsWorkSet secCsePersonsWorkSet;
    private FrequencyWorkSet secFrequencyWorkSet;
    private FrequencyWorkSet priFrequencyWorkSet;
    private DebtDetail priDebtDetail;
    private LegalAction priLegalAction;
    private ObligationType priObligationType;
    private CsePerson priCsePerson;
    private CsePersonAccount priCsePersonAccount;
    private Obligation priObligation;
    private DebtDetail secDebtDetail;
    private LegalAction secLegalAction;
    private ObligationType secObligationType;
    private CsePerson secCsePerson;
    private CsePersonAccount secCsePersonAccount;
    private Obligation secObligation;
    private NextTranInfo hidden;
    private CsePerson csePerson;
    private CsePersonAccount csePersonAccount;
    private CsePersonsWorkSet csePersonsWorkSet;
    private LegalAction legalAction;
    private Obligation obligation;
    private ObligationType obligationType;
    private Standard standard;
    private LegalActionDetail legalActionDetail;
    private Common obligationAmt;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of HardcodeOtrnDtAccrualInstruc.
    /// </summary>
    [JsonPropertyName("hardcodeOtrnDtAccrualInstruc")]
    public ObligationTransaction HardcodeOtrnDtAccrualInstruc
    {
      get => hardcodeOtrnDtAccrualInstruc ??= new();
      set => hardcodeOtrnDtAccrualInstruc = value;
    }

    /// <summary>
    /// A value of HardcodeObligSecondaryConcrnt.
    /// </summary>
    [JsonPropertyName("hardcodeObligSecondaryConcrnt")]
    public Obligation HardcodeObligSecondaryConcrnt
    {
      get => hardcodeObligSecondaryConcrnt ??= new();
      set => hardcodeObligSecondaryConcrnt = value;
    }

    /// <summary>
    /// A value of HardcodeObligPrimaryConcurrnt.
    /// </summary>
    [JsonPropertyName("hardcodeObligPrimaryConcurrnt")]
    public Obligation HardcodeObligPrimaryConcurrnt
    {
      get => hardcodeObligPrimaryConcurrnt ??= new();
      set => hardcodeObligPrimaryConcurrnt = value;
    }

    /// <summary>
    /// A value of HardcodeOtrnTDebt.
    /// </summary>
    [JsonPropertyName("hardcodeOtrnTDebt")]
    public ObligationTransaction HardcodeOtrnTDebt
    {
      get => hardcodeOtrnTDebt ??= new();
      set => hardcodeOtrnTDebt = value;
    }

    /// <summary>
    /// A value of HardcodeOrrPrimarySecondary.
    /// </summary>
    [JsonPropertyName("hardcodeOrrPrimarySecondary")]
    public ObligationRlnRsn HardcodeOrrPrimarySecondary
    {
      get => hardcodeOrrPrimarySecondary ??= new();
      set => hardcodeOrrPrimarySecondary = value;
    }

    /// <summary>
    /// A value of HardcodeCpaObligor.
    /// </summary>
    [JsonPropertyName("hardcodeCpaObligor")]
    public CsePersonAccount HardcodeCpaObligor
    {
      get => hardcodeCpaObligor ??= new();
      set => hardcodeCpaObligor = value;
    }

    /// <summary>
    /// A value of HardcodeOtGift.
    /// </summary>
    [JsonPropertyName("hardcodeOtGift")]
    public ObligationType HardcodeOtGift
    {
      get => hardcodeOtGift ??= new();
      set => hardcodeOtGift = value;
    }

    /// <summary>
    /// A value of OtCRecoverClassificat.
    /// </summary>
    [JsonPropertyName("otCRecoverClassificat")]
    public ObligationType OtCRecoverClassificat
    {
      get => otCRecoverClassificat ??= new();
      set => otCRecoverClassificat = value;
    }

    /// <summary>
    /// A value of OtCVoluntaryClassificat.
    /// </summary>
    [JsonPropertyName("otCVoluntaryClassificat")]
    public ObligationType OtCVoluntaryClassificat
    {
      get => otCVoluntaryClassificat ??= new();
      set => otCVoluntaryClassificat = value;
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
    /// A value of ReturnFromLink.
    /// </summary>
    [JsonPropertyName("returnFromLink")]
    public Common ReturnFromLink
    {
      get => returnFromLink ??= new();
      set => returnFromLink = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of ExpireEffectiveDateAttributes.
    /// </summary>
    [JsonPropertyName("expireEffectiveDateAttributes")]
    public ExpireEffectiveDateAttributes ExpireEffectiveDateAttributes
    {
      get => expireEffectiveDateAttributes ??= new();
      set => expireEffectiveDateAttributes = value;
    }

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    private ObligationTransaction hardcodeOtrnDtAccrualInstruc;
    private Obligation hardcodeObligSecondaryConcrnt;
    private Obligation hardcodeObligPrimaryConcurrnt;
    private ObligationTransaction hardcodeOtrnTDebt;
    private ObligationRlnRsn hardcodeOrrPrimarySecondary;
    private CsePersonAccount hardcodeCpaObligor;
    private ObligationType hardcodeOtGift;
    private ObligationType otCRecoverClassificat;
    private ObligationType otCVoluntaryClassificat;
    private DateWorkArea current;
    private Common returnFromLink;
    private DateWorkArea null1;
    private ExpireEffectiveDateAttributes expireEffectiveDateAttributes;
    private DateWorkArea dateWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public ObligationRln New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of SecondaryObligationType.
    /// </summary>
    [JsonPropertyName("secondaryObligationType")]
    public ObligationType SecondaryObligationType
    {
      get => secondaryObligationType ??= new();
      set => secondaryObligationType = value;
    }

    /// <summary>
    /// A value of PrimaryObligationType.
    /// </summary>
    [JsonPropertyName("primaryObligationType")]
    public ObligationType PrimaryObligationType
    {
      get => primaryObligationType ??= new();
      set => primaryObligationType = value;
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
    /// A value of AccrualSuspension.
    /// </summary>
    [JsonPropertyName("accrualSuspension")]
    public AccrualSuspension AccrualSuspension
    {
      get => accrualSuspension ??= new();
      set => accrualSuspension = value;
    }

    /// <summary>
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public AccrualInstructions Existing
    {
      get => existing ??= new();
      set => existing = value;
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
    /// A value of PrimaryObligation.
    /// </summary>
    [JsonPropertyName("primaryObligation")]
    public Obligation PrimaryObligation
    {
      get => primaryObligation ??= new();
      set => primaryObligation = value;
    }

    /// <summary>
    /// A value of SecondaryObligation.
    /// </summary>
    [JsonPropertyName("secondaryObligation")]
    public Obligation SecondaryObligation
    {
      get => secondaryObligation ??= new();
      set => secondaryObligation = value;
    }

    /// <summary>
    /// A value of SecondaryCsePersonAccount.
    /// </summary>
    [JsonPropertyName("secondaryCsePersonAccount")]
    public CsePersonAccount SecondaryCsePersonAccount
    {
      get => secondaryCsePersonAccount ??= new();
      set => secondaryCsePersonAccount = value;
    }

    /// <summary>
    /// A value of PrimaryCsePersonAccount.
    /// </summary>
    [JsonPropertyName("primaryCsePersonAccount")]
    public CsePersonAccount PrimaryCsePersonAccount
    {
      get => primaryCsePersonAccount ??= new();
      set => primaryCsePersonAccount = value;
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
    /// A value of DebtAdjustment.
    /// </summary>
    [JsonPropertyName("debtAdjustment")]
    public ObligationTransaction DebtAdjustment
    {
      get => debtAdjustment ??= new();
      set => debtAdjustment = value;
    }

    /// <summary>
    /// A value of ObligationTransactionRln.
    /// </summary>
    [JsonPropertyName("obligationTransactionRln")]
    public ObligationTransactionRln ObligationTransactionRln
    {
      get => obligationTransactionRln ??= new();
      set => obligationTransactionRln = value;
    }

    /// <summary>
    /// A value of Debt.
    /// </summary>
    [JsonPropertyName("debt")]
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
    }

    /// <summary>
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    private ObligationPaymentSchedule obligationPaymentSchedule;
    private AccrualInstructions accrualInstructions;
    private LegalAction legalAction;
    private CsePerson csePerson;
    private Obligation obligation;
    private ObligationRln new1;
    private ObligationType secondaryObligationType;
    private ObligationType primaryObligationType;
    private DebtDetail debtDetail;
    private AccrualSuspension accrualSuspension;
    private AccrualInstructions existing;
    private ObligationTransaction obligationTransaction;
    private Obligation primaryObligation;
    private Obligation secondaryObligation;
    private CsePersonAccount secondaryCsePersonAccount;
    private CsePersonAccount primaryCsePersonAccount;
    private ObligationRlnRsn obligationRlnRsn;
    private ObligationRln obligationRln;
    private ObligationTransaction debtAdjustment;
    private ObligationTransactionRln obligationTransactionRln;
    private ObligationTransaction debt;
    private Collection collection;
  }
#endregion
}
