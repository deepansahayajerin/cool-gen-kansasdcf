// Program: FN_READ_ACCRUING_OBLIGATION, ID: 372084587, model: 746.
// Short name: SWE00528
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
/// A program: FN_READ_ACCRUING_OBLIGATION.
/// </para>
/// <para>
/// RESP: FINANCE
/// This CAB will read the CSE Person, Obligation, Obligation Transaction (&amp;
/// related entities) and all of the support CSE Person's associated to the
/// Obligation.
/// Required Import Views:
/// 	CSE Person Number
/// 	Obligation Sys Gen Id
/// 	Obligation Transaction Sys Gen ID
/// </para>
/// </summary>
[Serializable]
public partial class FnReadAccruingObligation: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_READ_ACCRUING_OBLIGATION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnReadAccruingObligation(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnReadAccruingObligation.
  /// </summary>
  public FnReadAccruingObligation(IContext context, Import import, Export export)
    :
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
    // ***--- Sumanta - MTW  - 05/01/97
    // ***--- Changed the use of DB2 current date to IEF current date.
    // ***---
    // =================================================
    // PR# 237: 8/27/99 - bud adams  -  Case assignment not found
    //   returned from CAB on a related Case where there really was
    //   no assignment.  No need to have processed that data,
    //   so put in a test and avoided it altogether.
    // PR #78973, 77622, 78320, 77799 Remove Case # and Worker ID, add Last 
    // Accrual Date, fix -811.  E. Parker 11/01/1999
    // =================================================
    // =================================================================================
    // 06/22/2006               GVandy              WR# 230751
    // Add capability to select tribal interstate request.
    // ===================================================================================
    // : Set hardcoded values
    ExitState = "ACO_NN0000_ALL_OK";
    export.ObligationType.SystemGeneratedIdentifier =
      import.ObligationType.SystemGeneratedIdentifier;
    MoveLegalAction(import.LegalAction, export.LegalAction);
    export.LegalActionDetail.Assign(import.LegalActionDetail);

    // : Main Line Process
    if (ReadCsePerson4())
    {
      export.Obligor.Number = entities.ObligorCsePerson.Number;
    }
    else
    {
      ExitState = "FN0000_OBLIGOR_NF";

      return;
    }

    // ============================================================
    // All accruing obligations are a result of a legal_action / 
    // legal_action_detail
    // ============================================================
    if (ReadLegalAction())
    {
      export.LegalAction.Assign(entities.LegalAction);
    }
    else
    {
      ExitState = "LEGAL_ACTION_NF";

      return;
    }

    if (ReadObligationType())
    {
      export.ObligationType.Assign(entities.ObligationType);
    }
    else
    {
      ExitState = "FN0000_OBLIG_TYPE_NF";

      return;
    }

    if (import.LegalActionDetail.Number == 0)
    {
      // =================================================
      // 1/29/99 - Bud Adams  -  This is the case when coming from
      //   OPAY.  Obligation identifier will be present.
      // =================================================
      if (!ReadObligation2())
      {
        ExitState = "FN0000_OBLIGATION_NF";

        return;
      }

      if (ReadLegalActionDetail())
      {
        export.LegalActionDetail.Assign(entities.LegalActionDetail);
      }
      else
      {
        ExitState = "LEGAL_ACTION_DETAIL_NF";

        return;
      }
    }
    else
    {
      export.LegalActionDetail.Assign(import.LegalActionDetail);

      // =================================================
      // 1/29/99 - b adams  -  using new relationship between obligation
      //   and legal_action_detail and removing reference to relationship
      //   to legal_action
      // =================================================
      if (!ReadObligation1())
      {
        ExitState = "FN0000_OBLIGATION_NF";

        return;
      }
    }

    export.Obligation.Assign(entities.Obligation);

    if (Equal(export.Obligation.OtherStateAbbr, "KS"))
    {
      export.Obligation.OtherStateAbbr = "";
    }

    // : Set the Manual Distribution flag
    if (ReadManualDistributionAudit())
    {
      export.ManualDistributionInd.Flag = "Y";
    }
    else
    {
      export.ManualDistributionInd.Flag = "N";
    }

    // <<< RBM   01/27/98  Read the Interstate Case Information >>>
    if (ReadInterstateRequest())
    {
      export.InterstateRequest.Assign(entities.InterstateRequest);

      if (!IsEmpty(entities.InterstateRequest.Country))
      {
        local.Code.CodeName = "COUNTRY CODE";
        local.CodeValue.Cdvalue = entities.InterstateRequest.Country ?? Spaces
          (10);
        UseCabValidateCodeValue();
      }
      else if (!IsEmpty(entities.InterstateRequest.TribalAgency))
      {
        local.Code.CodeName = "TRIBAL IV-D AGENCIES";
        local.CodeValue.Cdvalue = entities.InterstateRequest.TribalAgency ?? Spaces
          (10);
        UseCabValidateCodeValue();
      }
    }
    else
    {
      export.InterstateRequest.OtherStateCaseId = "";
    }

    // *** Get the Alternate FIPS Code
    // ---- Sumanta - MTW - 04/28/97
    //     -* Deleted the read to FIPS ..
    //     -* Added the follwoing read to get the alternate add..
    // ***---
    // =================================================
    // 2-17-1999 - b adams  -  If alternate billing address is entered
    //   via LACT, then it cannot be changed by Debt screens and
    //   the field on the screen must be protected.  "LE" will flag
    //   this one.
    // =================================================
    if (ReadCsePerson1())
    {
      local.TextWorkArea.Text10 = entities.CsePerson.Number;
      export.Alternate.Char2 = "LE";
    }
    else if (ReadCsePerson2())
    {
      local.TextWorkArea.Text10 = entities.CsePerson.Number;
    }

    if (!IsEmpty(local.TextWorkArea.Text10))
    {
      UseEabPadLeftWithZeros();
      export.Alternate.Number = local.TextWorkArea.Text10;
    }

    // : Check for Concurrent Obligor
    if (AsChar(entities.Obligation.PrimarySecondaryCode) == 'J')
    {
      // <<< RBM  02-09-98  Use the First Relationship >>>
      if (ReadObligationRlnObligation1())
      {
        export.Concurrent.SystemGeneratedIdentifier =
          entities.Concurrent.SystemGeneratedIdentifier;
      }
      else
      {
        // : Continue Processing
        // <<< RBM  02-09-98  Use the First Relationship >>>
        if (ReadObligationRlnObligation2())
        {
          export.Concurrent.SystemGeneratedIdentifier =
            entities.Concurrent.SystemGeneratedIdentifier;
        }
        else
        {
          ExitState = "FN0000_CONCURRENT_OBLIGATION_NF";

          return;
        }
      }
    }

    // : If there is a concurrent obligation, read the concurrent obligor
    if (entities.Concurrent.Populated)
    {
      if (ReadCsePerson3())
      {
        export.ConcurrentObligorCsePerson.Number =
          entities.ConcurrentObligorCsePerson.Number;
      }
    }

    // : Get Frequency information
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
    if (ReadObligationPaymentSchedule())
    {
      export.ObligationPaymentSchedule.
        Assign(entities.ObligationPaymentSchedule);
      export.DiscontinueDate.Date = entities.ObligationPaymentSchedule.EndDt;
    }
    else
    {
      // =================================================
      // 2/23/1999 - Bud Adams  -  These need to be processed, but
      //   not in the normal way.  They will be adjusted to 0 becoming
      //   inactive, and then a new obligation with the proper amounts
      //   will be added.
      // =================================================
      if (Equal(entities.Obligation.CreatedBy, "CONVERSN"))
      {
      }
      else
      {
        ExitState = "FN0000_OBLIG_PYMNT_SCH_NF";

        return;
      }
    }

    // : Format frequency information for screen display
    UseFnSetFrequencyTextField();

    // : Check for suspension of interest
    if (AsChar(export.Obligation.HistoryInd) == 'Y')
    {
      if (ReadInterestSuppStatusHistory1())
      {
        export.InterestSuspendedInd.Flag = "Y";
      }
      else
      {
        export.InterestSuspendedInd.Flag = "N";
      }
    }
    else if (ReadInterestSuppStatusHistory2())
    {
      export.InterestSuspendedInd.Flag = "Y";
    }
    else
    {
      export.InterestSuspendedInd.Flag = "N";
    }

    // : Build the detail lines - Obligation breakdown by Supported Person
    export.Group.Index = 0;
    export.Group.Clear();

    foreach(var item in ReadObligationTransactionAccrualInstructions())
    {
      MoveAccrualInstructions(entities.AccrualInstructions,
        export.Group.Update.AccrualInstructions);
      ++local.GroupViewCount.Count;

      if (Equal(export.Group.Item.AccrualInstructions.DiscontinueDt,
        import.Max.Date))
      {
        export.Group.Update.AccrualInstructions.DiscontinueDt =
          local.Null1.Date;
      }

      if (Equal(export.Group.Item.AccrualInstructions.LastAccrualDt,
        import.Max.Date))
      {
        export.Group.Update.AccrualInstructions.LastAccrualDt =
          local.Null1.Date;
      }

      // **** Get the last accrual date ****
      if (Lt(export.LastAccrual.LastAccrualDt,
        entities.AccrualInstructions.LastAccrualDt))
      {
        export.LastAccrual.LastAccrualDt =
          entities.AccrualInstructions.LastAccrualDt;
      }

      MoveObligationTransaction(entities.ObligationTransaction,
        export.Group.Update.ObligationTransaction);

      if (Lt(import.Current.Date, entities.AccrualInstructions.DiscontinueDt) ||
        AsChar(export.Obligation.HistoryInd) == 'Y')
      {
        export.AccrualAmount.TotalCurrency += entities.ObligationTransaction.
          Amount;
      }

      // : Get the supported person
      if (ReadCsePerson5())
      {
        export.Group.Update.SupportedCsePerson.Number =
          entities.Supported2.Number;
      }
      else
      {
        ExitState = "FN0000_SUPP_PERSON_NF";
        export.Group.Next();

        return;
      }

      // **** Get the program type for the supported person ****
      // =================================================
      // 2/11/1999 - b adams  -  replaced the cab to determine the
      //   supported person program code.
      // =================================================
      // =================================================
      // 4/2/99 - bud adams  -  Program code for supported person
      //   removed from screen.  No value in displaying this, and it
      //   only brings confusion about what it represents.
      // =================================================
      // : Get the case number and worker id for the supported person
      if (AsChar(export.Obligation.HistoryInd) == 'Y')
      {
        // =================================================
        // 2/26/1999 - B Adams  -  Resume date is 1 day after the End date
        // =================================================
        local.ObligationPaymentSchedule.EndDt =
          AddDays(entities.ObligationPaymentSchedule.EndDt, 1);

        if (ReadAccrualSuspension1())
        {
          export.Group.Update.AccrualSuspended.Flag = "Y";
        }
        else
        {
          // <<< RBM  - This is true when History Payments are already entered 
          // using REIP screen and  the user has UN-SUSPENDED the Accrual for
          // this duration for the accrual to run and distribution to take place
          // for the History period. >>>
          export.Group.Update.AccrualSuspended.Flag = "N";
        }
      }
      else if (ReadAccrualSuspension2())
      {
        export.Group.Update.AccrualSuspended.Flag = "Y";
      }
      else
      {
        export.Group.Update.AccrualSuspended.Flag = "N";
      }

      export.Group.Next();
    }

    if (local.GroupViewCount.Count == 0)
    {
      ExitState = "FN0000_OBLIG_TRANS_NF";

      return;
    }

    if (export.Group.IsEmpty)
    {
      ExitState = "FN0000_SUPP_PERSON_NF";

      return;
    }

    export.Group2.Index = -1;

    for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
      export.Group.Index)
    {
      if (local.GroupViewCount.Count == local.GroupViewMoveCount.Count)
      {
        break;
      }

      ++local.GroupViewMoveCount.Count;

      ++export.Group2.Index;
      export.Group2.CheckSize();

      MoveAccrualInstructions(export.Group.Item.AccrualInstructions,
        export.Group2.Update.Grp2AccrualInstructions);
      MoveAccrualInstructions(export.Group.Item.AccrualInstructions,
        export.Group2.Update.Grp2Hidden);
      MoveCommon(export.Group.Item.Common, export.Group2.Update.Grp2Common);
      MoveObligationTransaction(export.Group.Item.ObligationTransaction,
        export.Group2.Update.Grp2ObligationTransaction);
      export.Group2.Update.ConcurrentObligor2.SystemGeneratedIdentifier =
        export.Group.Item.ConcurrentObligor.SystemGeneratedIdentifier;
      export.Group2.Update.Previous2.Amount = export.Group.Item.Previous.Amount;
      export.Group2.Update.ProratePercentage2.Percentage =
        export.Group.Item.ProratePercentage.Percentage;
      export.Group2.Update.Supported2CsePerson.Number =
        export.Group.Item.SupportedCsePerson.Number;
      MoveCsePersonsWorkSet(export.Group.Item.SupportedCsePersonsWorkSet,
        export.Group2.Update.Supported2CsePersonsWorkSet);
      export.Group2.Update.Grp2AccrualSuspended.Flag =
        export.Group.Item.AccrualSuspended.Flag;
    }

    // =================================================
    // 5/6/99 - bud adams  -  Now that ob-tran records are not being
    //   created for non-case-related persons, they aren't showing
    //   up on OACC after the obligation is created.  This code is to
    //   retrieve any persons, if they exist.
    // =================================================
    foreach(var item in ReadLegalActionPersonCsePerson())
    {
      // =================================================
      // PR# 237: 8/27/99 - Bud Adams  -  Case assignment not
      //   found from the following action diagram caused by inactive
      //   cases converted without case assignments.  It's just as
      //   well to skip this CAB in the case of a supported, case-
      //   related person.  We really only need to do this for those
      //   people who haven't been picked up as being case-related.
      // =================================================
      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        if (Equal(export.Group.Item.SupportedCsePerson.Number,
          entities.CsePerson.Number))
        {
          goto ReadEach;
        }
      }

      // =================================================
      // 3/26/1999 - bud adams  -  The Reads of Case / Case_Role
      //   specifies and End_Date for Case_Role.  We're just looking
      //   for non-case related persons, and that means that they
      //   NEVER were related to a case, ever..
      // =================================================
      local.SearchDiscontinue.Date = new DateTime(1900, 1, 1);
      UseFnReadCaseNoAndWorkerId();

      if (IsExitState("NO_CASE_RL_FOUND_FOR_SUPP_PERSON"))
      {
        ExitState = "ACO_NN0000_ALL_OK";

        ++export.Group2.Index;
        export.Group2.CheckSize();

        export.Group2.Update.Grp2ObligationTransaction.Amount =
          entities.LegalActionPerson.CurrentAmount.GetValueOrDefault();
        export.Group2.Update.Grp2ProgramScreenAttributes.ProgramTypeInd = "Z";
        export.Group2.Update.Supported2CsePerson.Number =
          entities.CsePerson.Number;

        if (Equal(entities.LegalActionPerson.EndDate, import.Max.Date))
        {
          export.Group2.Update.Grp2AccrualInstructions.DiscontinueDt =
            local.Null1.Date;
          export.Group2.Update.Grp2Hidden.DiscontinueDt = local.Null1.Date;
        }
        else
        {
          export.Group2.Update.Grp2AccrualInstructions.DiscontinueDt =
            entities.LegalActionPerson.EndDate;
          export.Group2.Update.Grp2Hidden.DiscontinueDt =
            entities.LegalActionPerson.EndDate;
        }

        if (IsEmpty(entities.LegalActionPerson.EndReason))
        {
          export.Group2.Update.Grp2Common.Flag = "U";
        }
      }

ReadEach:
      ;
    }

    ExitState = "ACO_NN0000_ALL_OK";
  }

  private static void MoveAccrualInstructions(AccrualInstructions source,
    AccrualInstructions target)
  {
    target.DiscontinueDt = source.DiscontinueDt;
    target.LastAccrualDt = source.LastAccrualDt;
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

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
  }

  private static void MoveObligationTransaction(ObligationTransaction source,
    ObligationTransaction target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Amount = source.Amount;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    export.Country.Assign(useExport.CodeValue);
  }

  private void UseEabPadLeftWithZeros()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.TextWorkArea.Text10;
    useExport.TextWorkArea.Text10 = local.TextWorkArea.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.TextWorkArea.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseFnReadCaseNoAndWorkerId()
  {
    var useImport = new FnReadCaseNoAndWorkerId.Import();
    var useExport = new FnReadCaseNoAndWorkerId.Export();

    useImport.Supported.Number = entities.CsePerson.Number;
    useImport.Obligor.Number = import.ObligorCsePerson.Number;
    useImport.SearchDiscontinue.Date = local.SearchDiscontinue.Date;

    Call(FnReadCaseNoAndWorkerId.Execute, useImport, useExport);
  }

  private void UseFnSetFrequencyTextField()
  {
    var useImport = new FnSetFrequencyTextField.Import();
    var useExport = new FnSetFrequencyTextField.Export();

    useImport.ObligationPaymentSchedule.
      Assign(export.ObligationPaymentSchedule);

    Call(FnSetFrequencyTextField.Execute, useImport, useExport);

    export.FrequencyWorkSet.Assign(useExport.FrequencyWorkSet);
  }

  private bool ReadAccrualSuspension1()
  {
    System.Diagnostics.Debug.Assert(entities.AccrualInstructions.Populated);
    entities.AccrualSuspension.Populated = false;

    return Read("ReadAccrualSuspension1",
      (db, command) =>
      {
        db.SetInt32(
          command, "otrId", entities.AccrualInstructions.OtrGeneratedId);
        db.SetString(command, "cpaType", entities.AccrualInstructions.CpaType);
        db.SetString(
          command, "cspNumber", entities.AccrualInstructions.CspNumber);
        db.SetInt32(
          command, "obgId", entities.AccrualInstructions.ObgGeneratedId);
        db.SetInt32(command, "otyId", entities.AccrualInstructions.OtyId);
        db.SetString(command, "otrType", entities.AccrualInstructions.OtrType);
        db.SetDate(
          command, "suspendDt",
          export.ObligationPaymentSchedule.StartDt.GetValueOrDefault());
        db.SetNullableDate(
          command, "resumeDt",
          local.ObligationPaymentSchedule.EndDt.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.AccrualSuspension.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.AccrualSuspension.SuspendDt = db.GetDate(reader, 1);
        entities.AccrualSuspension.ResumeDt = db.GetNullableDate(reader, 2);
        entities.AccrualSuspension.OtrType = db.GetString(reader, 3);
        entities.AccrualSuspension.OtyId = db.GetInt32(reader, 4);
        entities.AccrualSuspension.ObgId = db.GetInt32(reader, 5);
        entities.AccrualSuspension.CspNumber = db.GetString(reader, 6);
        entities.AccrualSuspension.CpaType = db.GetString(reader, 7);
        entities.AccrualSuspension.OtrId = db.GetInt32(reader, 8);
        entities.AccrualSuspension.Populated = true;
        CheckValid<AccrualSuspension>("OtrType",
          entities.AccrualSuspension.OtrType);
        CheckValid<AccrualSuspension>("CpaType",
          entities.AccrualSuspension.CpaType);
      });
  }

  private bool ReadAccrualSuspension2()
  {
    System.Diagnostics.Debug.Assert(entities.AccrualInstructions.Populated);
    entities.AccrualSuspension.Populated = false;

    return Read("ReadAccrualSuspension2",
      (db, command) =>
      {
        db.SetInt32(
          command, "otrId", entities.AccrualInstructions.OtrGeneratedId);
        db.SetString(command, "cpaType", entities.AccrualInstructions.CpaType);
        db.SetString(
          command, "cspNumber", entities.AccrualInstructions.CspNumber);
        db.SetInt32(
          command, "obgId", entities.AccrualInstructions.ObgGeneratedId);
        db.SetInt32(command, "otyId", entities.AccrualInstructions.OtyId);
        db.SetString(command, "otrType", entities.AccrualInstructions.OtrType);
        db.
          SetDate(command, "suspendDt", import.Current.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.AccrualSuspension.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.AccrualSuspension.SuspendDt = db.GetDate(reader, 1);
        entities.AccrualSuspension.ResumeDt = db.GetNullableDate(reader, 2);
        entities.AccrualSuspension.OtrType = db.GetString(reader, 3);
        entities.AccrualSuspension.OtyId = db.GetInt32(reader, 4);
        entities.AccrualSuspension.ObgId = db.GetInt32(reader, 5);
        entities.AccrualSuspension.CspNumber = db.GetString(reader, 6);
        entities.AccrualSuspension.CpaType = db.GetString(reader, 7);
        entities.AccrualSuspension.OtrId = db.GetInt32(reader, 8);
        entities.AccrualSuspension.Populated = true;
        CheckValid<AccrualSuspension>("OtrType",
          entities.AccrualSuspension.OtrType);
        CheckValid<AccrualSuspension>("CpaType",
          entities.AccrualSuspension.CpaType);
      });
  }

  private bool ReadCsePerson1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          entities.Obligation.LgaId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.Obligation.CspPNumber ?? "");
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadCsePerson3()
  {
    System.Diagnostics.Debug.Assert(entities.Concurrent.Populated);
    entities.ConcurrentObligorCsePerson.Populated = false;

    return Read("ReadCsePerson3",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.Concurrent.CspNumber);
        db.SetString(command, "cpaType", entities.Concurrent.CpaType);
        db.SetString(command, "type", import.HcCpaObligor.Type1);
      },
      (db, reader) =>
      {
        entities.ConcurrentObligorCsePerson.Number = db.GetString(reader, 0);
        entities.ConcurrentObligorCsePerson.Populated = true;
      });
  }

  private bool ReadCsePerson4()
  {
    entities.ObligorCsePerson.Populated = false;

    return Read("ReadCsePerson4",
      (db, command) =>
      {
        db.SetString(command, "numb", import.ObligorCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ObligorCsePerson.Number = db.GetString(reader, 0);
        entities.ObligorCsePerson.Type1 = db.GetString(reader, 1);
        entities.ObligorCsePerson.OrganizationName =
          db.GetNullableString(reader, 2);
        entities.ObligorCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ObligorCsePerson.Type1);
      });
  }

  private bool ReadCsePerson5()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.Supported2.Populated = false;

    return Read("ReadCsePerson5",
      (db, command) =>
      {
        db.SetString(
          command, "numb", entities.ObligationTransaction.CspSupNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Supported2.Number = db.GetString(reader, 0);
        entities.Supported2.Type1 = db.GetString(reader, 1);
        entities.Supported2.OrganizationName = db.GetNullableString(reader, 2);
        entities.Supported2.Populated = true;
        CheckValid<CsePerson>("Type1", entities.Supported2.Type1);
      });
  }

  private bool ReadInterestSuppStatusHistory1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.InterestSuppStatusHistory.Populated = false;

    return Read("ReadInterestSuppStatusHistory1",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          export.ObligationPaymentSchedule.StartDt.GetValueOrDefault());
        db.SetDate(
          command, "discontinueDate",
          export.ObligationPaymentSchedule.EndDt.GetValueOrDefault());
        db.SetInt32(
          command, "obgId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetInt32(command, "otyId", entities.Obligation.DtyGeneratedId);
      },
      (db, reader) =>
      {
        entities.InterestSuppStatusHistory.ObgId = db.GetInt32(reader, 0);
        entities.InterestSuppStatusHistory.CspNumber = db.GetString(reader, 1);
        entities.InterestSuppStatusHistory.CpaType = db.GetString(reader, 2);
        entities.InterestSuppStatusHistory.OtyId = db.GetInt32(reader, 3);
        entities.InterestSuppStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.InterestSuppStatusHistory.EffectiveDate =
          db.GetDate(reader, 5);
        entities.InterestSuppStatusHistory.DiscontinueDate =
          db.GetDate(reader, 6);
        entities.InterestSuppStatusHistory.ReasonText =
          db.GetNullableString(reader, 7);
        entities.InterestSuppStatusHistory.Populated = true;
        CheckValid<InterestSuppStatusHistory>("CpaType",
          entities.InterestSuppStatusHistory.CpaType);
      });
  }

  private bool ReadInterestSuppStatusHistory2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.InterestSuppStatusHistory.Populated = false;

    return Read("ReadInterestSuppStatusHistory2",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", import.Current.Date.GetValueOrDefault());
        db.SetInt32(
          command, "obgId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetInt32(command, "otyId", entities.Obligation.DtyGeneratedId);
      },
      (db, reader) =>
      {
        entities.InterestSuppStatusHistory.ObgId = db.GetInt32(reader, 0);
        entities.InterestSuppStatusHistory.CspNumber = db.GetString(reader, 1);
        entities.InterestSuppStatusHistory.CpaType = db.GetString(reader, 2);
        entities.InterestSuppStatusHistory.OtyId = db.GetInt32(reader, 3);
        entities.InterestSuppStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.InterestSuppStatusHistory.EffectiveDate =
          db.GetDate(reader, 5);
        entities.InterestSuppStatusHistory.DiscontinueDate =
          db.GetDate(reader, 6);
        entities.InterestSuppStatusHistory.ReasonText =
          db.GetNullableString(reader, 7);
        entities.InterestSuppStatusHistory.Populated = true;
        CheckValid<InterestSuppStatusHistory>("CpaType",
          entities.InterestSuppStatusHistory.CpaType);
      });
  }

  private bool ReadInterstateRequest()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "orderEffDate", import.Current.Date.GetValueOrDefault());
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 3);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 4);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 5);
        entities.InterstateRequest.Populated = true;
      });
  }

  private bool ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", import.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.CspNumber = db.GetNullableString(reader, 3);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalActionDetail()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.LegalActionDetail.Populated = false;

    return Read("ReadLegalActionDetail",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier1", entities.LegalAction.Identifier);
        db.SetInt32(
          command, "laDetailNo",
          entities.Obligation.LadNumber.GetValueOrDefault());
        db.SetInt32(
          command, "lgaIdentifier2",
          entities.Obligation.LgaIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.CurrentAmount =
          db.GetNullableDecimal(reader, 2);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 3);
        entities.LegalActionDetail.FreqPeriodCode =
          db.GetNullableString(reader, 4);
        entities.LegalActionDetail.DayOfWeek = db.GetNullableInt32(reader, 5);
        entities.LegalActionDetail.DayOfMonth1 = db.GetNullableInt32(reader, 6);
        entities.LegalActionDetail.DayOfMonth2 = db.GetNullableInt32(reader, 7);
        entities.LegalActionDetail.PeriodInd = db.GetNullableString(reader, 8);
        entities.LegalActionDetail.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);
      });
  }

  private IEnumerable<bool> ReadLegalActionPersonCsePerson()
  {
    entities.LegalActionPerson.Populated = false;
    entities.CsePerson.Populated = false;

    return ReadEach("ReadLegalActionPersonCsePerson",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladRNumber", export.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.LegalActionPerson.EndDate = db.GetNullableDate(reader, 2);
        entities.LegalActionPerson.EndReason = db.GetNullableString(reader, 3);
        entities.LegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 4);
        entities.LegalActionPerson.LadRNumber = db.GetNullableInt32(reader, 5);
        entities.LegalActionPerson.AccountType =
          db.GetNullableString(reader, 6);
        entities.LegalActionPerson.CurrentAmount =
          db.GetNullableDecimal(reader, 7);
        entities.LegalActionPerson.Populated = true;
        entities.CsePerson.Populated = true;

        return true;
      });
  }

  private bool ReadManualDistributionAudit()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ManualDistributionAudit.Populated = false;

    return Read("ReadManualDistributionAudit",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDt", import.Current.Date.GetValueOrDefault());
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        entities.ManualDistributionAudit.OtyType = db.GetInt32(reader, 0);
        entities.ManualDistributionAudit.ObgGeneratedId =
          db.GetInt32(reader, 1);
        entities.ManualDistributionAudit.CspNumber = db.GetString(reader, 2);
        entities.ManualDistributionAudit.CpaType = db.GetString(reader, 3);
        entities.ManualDistributionAudit.EffectiveDt = db.GetDate(reader, 4);
        entities.ManualDistributionAudit.DiscontinueDt =
          db.GetNullableDate(reader, 5);
        entities.ManualDistributionAudit.Populated = true;
        CheckValid<ManualDistributionAudit>("CpaType",
          entities.ManualDistributionAudit.CpaType);
      });
  }

  private bool ReadObligation1()
  {
    entities.Obligation.Populated = false;

    return Read("ReadObligation1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ObligorCsePerson.Number);
        db.SetString(command, "cpaType", import.HcCpaObligor.Type1);
        db.SetInt32(
          command, "dtyGeneratedId",
          entities.ObligationType.SystemGeneratedIdentifier);
        db.SetNullableInt32(
          command, "ladNumber", export.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.LegalAction.Identifier);
        db.SetInt32(
          command, "obId", import.ObligorObligation.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.Obligation.CspPNumber = db.GetNullableString(reader, 5);
        entities.Obligation.OtherStateAbbr = db.GetNullableString(reader, 6);
        entities.Obligation.Description = db.GetNullableString(reader, 7);
        entities.Obligation.HistoryInd = db.GetNullableString(reader, 8);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 9);
        entities.Obligation.CreatedBy = db.GetString(reader, 10);
        entities.Obligation.CreatedTmst = db.GetDateTime(reader, 11);
        entities.Obligation.LastUpdatedBy = db.GetNullableString(reader, 12);
        entities.Obligation.LastUpdateTmst = db.GetNullableDateTime(reader, 13);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 14);
        entities.Obligation.LgaIdentifier = db.GetNullableInt32(reader, 15);
        entities.Obligation.LadNumber = db.GetNullableInt32(reader, 16);
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
    entities.Obligation.Populated = false;

    return Read("ReadObligation2",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtyGeneratedId",
          entities.ObligationType.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.ObligorCsePerson.Number);
        db.SetString(command, "cpaType", import.HcCpaObligor.Type1);
        db.SetInt32(
          command, "obId", import.ObligorObligation.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.Obligation.CspPNumber = db.GetNullableString(reader, 5);
        entities.Obligation.OtherStateAbbr = db.GetNullableString(reader, 6);
        entities.Obligation.Description = db.GetNullableString(reader, 7);
        entities.Obligation.HistoryInd = db.GetNullableString(reader, 8);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 9);
        entities.Obligation.CreatedBy = db.GetString(reader, 10);
        entities.Obligation.CreatedTmst = db.GetDateTime(reader, 11);
        entities.Obligation.LastUpdatedBy = db.GetNullableString(reader, 12);
        entities.Obligation.LastUpdateTmst = db.GetNullableDateTime(reader, 13);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 14);
        entities.Obligation.LgaIdentifier = db.GetNullableInt32(reader, 15);
        entities.Obligation.LadNumber = db.GetNullableInt32(reader, 16);
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.Obligation.OrderTypeCode);
      });
  }

  private bool ReadObligationPaymentSchedule()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationPaymentSchedule.Populated = false;

    return Read("ReadObligationPaymentSchedule",
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
        entities.ObligationPaymentSchedule.FrequencyCode =
          db.GetString(reader, 7);
        entities.ObligationPaymentSchedule.DayOfWeek =
          db.GetNullableInt32(reader, 8);
        entities.ObligationPaymentSchedule.DayOfMonth1 =
          db.GetNullableInt32(reader, 9);
        entities.ObligationPaymentSchedule.DayOfMonth2 =
          db.GetNullableInt32(reader, 10);
        entities.ObligationPaymentSchedule.PeriodInd =
          db.GetNullableString(reader, 11);
        entities.ObligationPaymentSchedule.Populated = true;
        CheckValid<ObligationPaymentSchedule>("ObgCpaType",
          entities.ObligationPaymentSchedule.ObgCpaType);
        CheckValid<ObligationPaymentSchedule>("FrequencyCode",
          entities.ObligationPaymentSchedule.FrequencyCode);
        CheckValid<ObligationPaymentSchedule>("PeriodInd",
          entities.ObligationPaymentSchedule.PeriodInd);
      });
  }

  private bool ReadObligationRlnObligation1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.Concurrent.Populated = false;
    entities.ObligationRln.Populated = false;

    return Read("ReadObligationRlnObligation1",
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
          import.HcOrrJointSeveralOblg.SequentialGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ObligationRln.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Concurrent.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.ObligationRln.CspNumber = db.GetString(reader, 1);
        entities.Concurrent.CspNumber = db.GetString(reader, 1);
        entities.ObligationRln.CpaType = db.GetString(reader, 2);
        entities.Concurrent.CpaType = db.GetString(reader, 2);
        entities.ObligationRln.ObgFGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationRln.CspFNumber = db.GetString(reader, 4);
        entities.ObligationRln.CpaFType = db.GetString(reader, 5);
        entities.ObligationRln.OrrGeneratedId = db.GetInt32(reader, 6);
        entities.ObligationRln.OtySecondId = db.GetInt32(reader, 7);
        entities.Concurrent.DtyGeneratedId = db.GetInt32(reader, 7);
        entities.ObligationRln.OtyFirstId = db.GetInt32(reader, 8);
        entities.ObligationRln.Description = db.GetString(reader, 9);
        entities.Concurrent.PrimarySecondaryCode =
          db.GetNullableString(reader, 10);
        entities.Concurrent.Populated = true;
        entities.ObligationRln.Populated = true;
        CheckValid<ObligationRln>("CpaType", entities.ObligationRln.CpaType);
        CheckValid<Obligation>("CpaType", entities.Concurrent.CpaType);
        CheckValid<ObligationRln>("CpaFType", entities.ObligationRln.CpaFType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Concurrent.PrimarySecondaryCode);
      });
  }

  private bool ReadObligationRlnObligation2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.Concurrent.Populated = false;
    entities.ObligationRln.Populated = false;

    return Read("ReadObligationRlnObligation2",
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
          import.HcOrrJointSeveralOblg.SequentialGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ObligationRln.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ObligationRln.CspNumber = db.GetString(reader, 1);
        entities.ObligationRln.CpaType = db.GetString(reader, 2);
        entities.ObligationRln.ObgFGeneratedId = db.GetInt32(reader, 3);
        entities.Concurrent.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.ObligationRln.CspFNumber = db.GetString(reader, 4);
        entities.Concurrent.CspNumber = db.GetString(reader, 4);
        entities.ObligationRln.CpaFType = db.GetString(reader, 5);
        entities.Concurrent.CpaType = db.GetString(reader, 5);
        entities.ObligationRln.OrrGeneratedId = db.GetInt32(reader, 6);
        entities.ObligationRln.OtySecondId = db.GetInt32(reader, 7);
        entities.ObligationRln.OtyFirstId = db.GetInt32(reader, 8);
        entities.Concurrent.DtyGeneratedId = db.GetInt32(reader, 8);
        entities.ObligationRln.Description = db.GetString(reader, 9);
        entities.Concurrent.PrimarySecondaryCode =
          db.GetNullableString(reader, 10);
        entities.Concurrent.Populated = true;
        entities.ObligationRln.Populated = true;
        CheckValid<ObligationRln>("CpaType", entities.ObligationRln.CpaType);
        CheckValid<ObligationRln>("CpaFType", entities.ObligationRln.CpaFType);
        CheckValid<Obligation>("CpaType", entities.Concurrent.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Concurrent.PrimarySecondaryCode);
      });
  }

  private IEnumerable<bool> ReadObligationTransactionAccrualInstructions()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);

    return ReadEach("ReadObligationTransactionAccrualInstructions",
      (db, command) =>
      {
        db.SetString(command, "obTrnTyp", import.HcOtrnTDebt.Type1);
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.AccrualInstructions.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 1);
        entities.AccrualInstructions.CspNumber = db.GetString(reader, 1);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 2);
        entities.AccrualInstructions.CpaType = db.GetString(reader, 2);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.AccrualInstructions.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 4);
        entities.AccrualInstructions.OtrType = db.GetString(reader, 4);
        entities.ObligationTransaction.Amount = db.GetDecimal(reader, 5);
        entities.ObligationTransaction.DebtType = db.GetString(reader, 6);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 7);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 8);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 9);
        entities.AccrualInstructions.OtyId = db.GetInt32(reader, 9);
        entities.AccrualInstructions.AsOfDt = db.GetDate(reader, 10);
        entities.AccrualInstructions.DiscontinueDt =
          db.GetNullableDate(reader, 11);
        entities.AccrualInstructions.LastAccrualDt =
          db.GetNullableDate(reader, 12);
        entities.ObligationTransaction.Populated = true;
        entities.AccrualInstructions.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.ObligationTransaction.CpaType);
        CheckValid<AccrualInstructions>("CpaType",
          entities.AccrualInstructions.CpaType);
        CheckValid<ObligationTransaction>("Type1",
          entities.ObligationTransaction.Type1);
        CheckValid<AccrualInstructions>("OtrType",
          entities.AccrualInstructions.OtrType);
        CheckValid<ObligationTransaction>("DebtType",
          entities.ObligationTransaction.DebtType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ObligationTransaction.CpaSupType);

        return true;
      });
  }

  private bool ReadObligationType()
  {
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetInt32(
          command, "debtTypId",
          import.ObligationType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Classification = db.GetString(reader, 2);
        entities.ObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
      });
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
    /// A value of HcOrrJointSeveralOblg.
    /// </summary>
    [JsonPropertyName("hcOrrJointSeveralOblg")]
    public ObligationRlnRsn HcOrrJointSeveralOblg
    {
      get => hcOrrJointSeveralOblg ??= new();
      set => hcOrrJointSeveralOblg = value;
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

    /// <summary>
    /// A value of ObligorCsePerson.
    /// </summary>
    [JsonPropertyName("obligorCsePerson")]
    public CsePerson ObligorCsePerson
    {
      get => obligorCsePerson ??= new();
      set => obligorCsePerson = value;
    }

    /// <summary>
    /// A value of ObligorObligation.
    /// </summary>
    [JsonPropertyName("obligorObligation")]
    public Obligation ObligorObligation
    {
      get => obligorObligation ??= new();
      set => obligorObligation = value;
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

    private CsePersonAccount hcCpaObligor;
    private ObligationRlnRsn hcOrrJointSeveralOblg;
    private ObligationTransaction hcOtrnTDebt;
    private DateWorkArea max;
    private DateWorkArea current;
    private LegalActionDetail legalActionDetail;
    private LegalAction legalAction;
    private CsePerson obligorCsePerson;
    private Obligation obligorObligation;
    private ObligationType obligationType;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A Group2Group group.</summary>
    [Serializable]
    public class Group2Group
    {
      /// <summary>
      /// A value of ZdelExportGrp2.
      /// </summary>
      [JsonPropertyName("zdelExportGrp2")]
      public Program ZdelExportGrp2
      {
        get => zdelExportGrp2 ??= new();
        set => zdelExportGrp2 = value;
      }

      /// <summary>
      /// A value of Grp2Common.
      /// </summary>
      [JsonPropertyName("grp2Common")]
      public Common Grp2Common
      {
        get => grp2Common ??= new();
        set => grp2Common = value;
      }

      /// <summary>
      /// A value of Supported2CsePerson.
      /// </summary>
      [JsonPropertyName("supported2CsePerson")]
      public CsePerson Supported2CsePerson
      {
        get => supported2CsePerson ??= new();
        set => supported2CsePerson = value;
      }

      /// <summary>
      /// A value of Grp2AccrualSuspended.
      /// </summary>
      [JsonPropertyName("grp2AccrualSuspended")]
      public Common Grp2AccrualSuspended
      {
        get => grp2AccrualSuspended ??= new();
        set => grp2AccrualSuspended = value;
      }

      /// <summary>
      /// A value of Supported2CsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("supported2CsePersonsWorkSet")]
      public CsePersonsWorkSet Supported2CsePersonsWorkSet
      {
        get => supported2CsePersonsWorkSet ??= new();
        set => supported2CsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of Grp2Case.
      /// </summary>
      [JsonPropertyName("grp2Case")]
      public Case1 Grp2Case
      {
        get => grp2Case ??= new();
        set => grp2Case = value;
      }

      /// <summary>
      /// A value of ProratePercentage2.
      /// </summary>
      [JsonPropertyName("proratePercentage2")]
      public Common ProratePercentage2
      {
        get => proratePercentage2 ??= new();
        set => proratePercentage2 = value;
      }

      /// <summary>
      /// A value of Grp2ObligationTransaction.
      /// </summary>
      [JsonPropertyName("grp2ObligationTransaction")]
      public ObligationTransaction Grp2ObligationTransaction
      {
        get => grp2ObligationTransaction ??= new();
        set => grp2ObligationTransaction = value;
      }

      /// <summary>
      /// A value of Grp2ProgramScreenAttributes.
      /// </summary>
      [JsonPropertyName("grp2ProgramScreenAttributes")]
      public ProgramScreenAttributes Grp2ProgramScreenAttributes
      {
        get => grp2ProgramScreenAttributes ??= new();
        set => grp2ProgramScreenAttributes = value;
      }

      /// <summary>
      /// A value of Grp2ServiceProvider.
      /// </summary>
      [JsonPropertyName("grp2ServiceProvider")]
      public ServiceProvider Grp2ServiceProvider
      {
        get => grp2ServiceProvider ??= new();
        set => grp2ServiceProvider = value;
      }

      /// <summary>
      /// A value of Grp2AccrualInstructions.
      /// </summary>
      [JsonPropertyName("grp2AccrualInstructions")]
      public AccrualInstructions Grp2AccrualInstructions
      {
        get => grp2AccrualInstructions ??= new();
        set => grp2AccrualInstructions = value;
      }

      /// <summary>
      /// A value of ConcurrentObligor2.
      /// </summary>
      [JsonPropertyName("concurrentObligor2")]
      public ObligationTransaction ConcurrentObligor2
      {
        get => concurrentObligor2 ??= new();
        set => concurrentObligor2 = value;
      }

      /// <summary>
      /// A value of Previous2.
      /// </summary>
      [JsonPropertyName("previous2")]
      public ObligationTransaction Previous2
      {
        get => previous2 ??= new();
        set => previous2 = value;
      }

      /// <summary>
      /// A value of Zdel.
      /// </summary>
      [JsonPropertyName("zdel")]
      public CsePersonsWorkSet Zdel
      {
        get => zdel ??= new();
        set => zdel = value;
      }

      /// <summary>
      /// A value of Grp2Hidden.
      /// </summary>
      [JsonPropertyName("grp2Hidden")]
      public AccrualInstructions Grp2Hidden
      {
        get => grp2Hidden ??= new();
        set => grp2Hidden = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private Program zdelExportGrp2;
      private Common grp2Common;
      private CsePerson supported2CsePerson;
      private Common grp2AccrualSuspended;
      private CsePersonsWorkSet supported2CsePersonsWorkSet;
      private Case1 grp2Case;
      private Common proratePercentage2;
      private ObligationTransaction grp2ObligationTransaction;
      private ProgramScreenAttributes grp2ProgramScreenAttributes;
      private ServiceProvider grp2ServiceProvider;
      private AccrualInstructions grp2AccrualInstructions;
      private ObligationTransaction concurrentObligor2;
      private ObligationTransaction previous2;
      private CsePersonsWorkSet zdel;
      private AccrualInstructions grp2Hidden;
    }

    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
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
      /// A value of AccrualSuspended.
      /// </summary>
      [JsonPropertyName("accrualSuspended")]
      public Common AccrualSuspended
      {
        get => accrualSuspended ??= new();
        set => accrualSuspended = value;
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
      /// A value of ConcurrentObligor.
      /// </summary>
      [JsonPropertyName("concurrentObligor")]
      public ObligationTransaction ConcurrentObligor
      {
        get => concurrentObligor ??= new();
        set => concurrentObligor = value;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private Common common;
      private CsePerson supportedCsePerson;
      private Common accrualSuspended;
      private CsePersonsWorkSet supportedCsePersonsWorkSet;
      private Case1 case1;
      private Common proratePercentage;
      private ObligationTransaction obligationTransaction;
      private ServiceProvider serviceProvider;
      private AccrualInstructions accrualInstructions;
      private ObligationTransaction concurrentObligor;
      private ObligationTransaction previous;
    }

    /// <summary>
    /// A value of Country.
    /// </summary>
    [JsonPropertyName("country")]
    public CodeValue Country
    {
      get => country ??= new();
      set => country = value;
    }

    /// <summary>
    /// Gets a value of Group2.
    /// </summary>
    [JsonIgnore]
    public Array<Group2Group> Group2 => group2 ??= new(Group2Group.Capacity, 0);

    /// <summary>
    /// Gets a value of Group2 for json serialization.
    /// </summary>
    [JsonPropertyName("group2")]
    [Computed]
    public IList<Group2Group> Group2_Json
    {
      get => group2;
      set => Group2.Assign(value);
    }

    /// <summary>
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    /// <summary>
    /// A value of Alternate.
    /// </summary>
    [JsonPropertyName("alternate")]
    public CsePersonsWorkSet Alternate
    {
      get => alternate ??= new();
      set => alternate = value;
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
    /// A value of LastAccrual.
    /// </summary>
    [JsonPropertyName("lastAccrual")]
    public AccrualInstructions LastAccrual
    {
      get => lastAccrual ??= new();
      set => lastAccrual = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonsWorkSet Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of ConcurrentObligorCsePerson.
    /// </summary>
    [JsonPropertyName("concurrentObligorCsePerson")]
    public CsePerson ConcurrentObligorCsePerson
    {
      get => concurrentObligorCsePerson ??= new();
      set => concurrentObligorCsePerson = value;
    }

    /// <summary>
    /// A value of ConcurrentObligorCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("concurrentObligorCsePersonsWorkSet")]
    public CsePersonsWorkSet ConcurrentObligorCsePersonsWorkSet
    {
      get => concurrentObligorCsePersonsWorkSet ??= new();
      set => concurrentObligorCsePersonsWorkSet = value;
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
    /// A value of Concurrent.
    /// </summary>
    [JsonPropertyName("concurrent")]
    public Obligation Concurrent
    {
      get => concurrent ??= new();
      set => concurrent = value;
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
    /// A value of ObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("obligationPaymentSchedule")]
    public ObligationPaymentSchedule ObligationPaymentSchedule
    {
      get => obligationPaymentSchedule ??= new();
      set => obligationPaymentSchedule = value;
    }

    /// <summary>
    /// A value of FrequencyWorkSet.
    /// </summary>
    [JsonPropertyName("frequencyWorkSet")]
    public FrequencyWorkSet FrequencyWorkSet
    {
      get => frequencyWorkSet ??= new();
      set => frequencyWorkSet = value;
    }

    /// <summary>
    /// A value of DiscontinueDate.
    /// </summary>
    [JsonPropertyName("discontinueDate")]
    public DateWorkArea DiscontinueDate
    {
      get => discontinueDate ??= new();
      set => discontinueDate = value;
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
    /// A value of AccrualAmount.
    /// </summary>
    [JsonPropertyName("accrualAmount")]
    public Common AccrualAmount
    {
      get => accrualAmount ??= new();
      set => accrualAmount = value;
    }

    /// <summary>
    /// A value of AccrualSuspendedInd.
    /// </summary>
    [JsonPropertyName("accrualSuspendedInd")]
    public Common AccrualSuspendedInd
    {
      get => accrualSuspendedInd ??= new();
      set => accrualSuspendedInd = value;
    }

    /// <summary>
    /// A value of InterestSuspendedInd.
    /// </summary>
    [JsonPropertyName("interestSuspendedInd")]
    public Common InterestSuspendedInd
    {
      get => interestSuspendedInd ??= new();
      set => interestSuspendedInd = value;
    }

    /// <summary>
    /// A value of ManualDistributionInd.
    /// </summary>
    [JsonPropertyName("manualDistributionInd")]
    public Common ManualDistributionInd
    {
      get => manualDistributionInd ??= new();
      set => manualDistributionInd = value;
    }

    /// <summary>
    /// A value of ObligationActiveInd.
    /// </summary>
    [JsonPropertyName("obligationActiveInd")]
    public Common ObligationActiveInd
    {
      get => obligationActiveInd ??= new();
      set => obligationActiveInd = value;
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

    private CodeValue country;
    private Array<Group2Group> group2;
    private InterstateRequest interstateRequest;
    private CsePersonsWorkSet alternate;
    private LegalActionDetail legalActionDetail;
    private AccrualInstructions lastAccrual;
    private CsePersonsWorkSet obligor;
    private CsePerson concurrentObligorCsePerson;
    private CsePersonsWorkSet concurrentObligorCsePersonsWorkSet;
    private Obligation obligation;
    private Obligation concurrent;
    private ObligationType obligationType;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private FrequencyWorkSet frequencyWorkSet;
    private DateWorkArea discontinueDate;
    private LegalAction legalAction;
    private Common accrualAmount;
    private Common accrualSuspendedInd;
    private Common interestSuspendedInd;
    private Common manualDistributionInd;
    private Common obligationActiveInd;
    private Array<GroupGroup> group;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local: IInitializable
  {
    /// <summary>
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

    /// <summary>
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
    }

    /// <summary>
    /// A value of GroupViewMoveCount.
    /// </summary>
    [JsonPropertyName("groupViewMoveCount")]
    public Common GroupViewMoveCount
    {
      get => groupViewMoveCount ??= new();
      set => groupViewMoveCount = value;
    }

    /// <summary>
    /// A value of GroupViewCount.
    /// </summary>
    [JsonPropertyName("groupViewCount")]
    public Common GroupViewCount
    {
      get => groupViewCount ??= new();
      set => groupViewCount = value;
    }

    /// <summary>
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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
    /// A value of SearchDiscontinue.
    /// </summary>
    [JsonPropertyName("searchDiscontinue")]
    public DateWorkArea SearchDiscontinue
    {
      get => searchDiscontinue ??= new();
      set => searchDiscontinue = value;
    }

    /// <para>Resets the state.</para>
    void IInitializable.Initialize()
    {
      code = null;
      codeValue = null;
      groupViewMoveCount = null;
      groupViewCount = null;
      textWorkArea = null;
      null1 = null;
      csePersonsWorkSet = null;
      searchDiscontinue = null;
    }

    private Code code;
    private CodeValue codeValue;
    private Common groupViewMoveCount;
    private Common groupViewCount;
    private TextWorkArea textWorkArea;
    private DateWorkArea null1;
    private CsePersonsWorkSet csePersonsWorkSet;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private DateWorkArea searchDiscontinue;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    /// <summary>
    /// A value of InterstateRequestObligation.
    /// </summary>
    [JsonPropertyName("interstateRequestObligation")]
    public InterstateRequestObligation InterstateRequestObligation
    {
      get => interstateRequestObligation ??= new();
      set => interstateRequestObligation = value;
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
    /// A value of InterestSuppStatusHistory.
    /// </summary>
    [JsonPropertyName("interestSuppStatusHistory")]
    public InterestSuppStatusHistory InterestSuppStatusHistory
    {
      get => interestSuppStatusHistory ??= new();
      set => interestSuppStatusHistory = value;
    }

    /// <summary>
    /// A value of Supported1.
    /// </summary>
    [JsonPropertyName("supported1")]
    public CsePersonAccount Supported1
    {
      get => supported1 ??= new();
      set => supported1 = value;
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
    /// A value of ConcurrentObligorCsePerson.
    /// </summary>
    [JsonPropertyName("concurrentObligorCsePerson")]
    public CsePerson ConcurrentObligorCsePerson
    {
      get => concurrentObligorCsePerson ??= new();
      set => concurrentObligorCsePerson = value;
    }

    /// <summary>
    /// A value of ConcurrentObligorCsePersonAccount.
    /// </summary>
    [JsonPropertyName("concurrentObligorCsePersonAccount")]
    public CsePersonAccount ConcurrentObligorCsePersonAccount
    {
      get => concurrentObligorCsePersonAccount ??= new();
      set => concurrentObligorCsePersonAccount = value;
    }

    /// <summary>
    /// A value of Supported2.
    /// </summary>
    [JsonPropertyName("supported2")]
    public CsePerson Supported2
    {
      get => supported2 ??= new();
      set => supported2 = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of Concurrent.
    /// </summary>
    [JsonPropertyName("concurrent")]
    public Obligation Concurrent
    {
      get => concurrent ??= new();
      set => concurrent = value;
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
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
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
    /// A value of AccrualSuspension.
    /// </summary>
    [JsonPropertyName("accrualSuspension")]
    public AccrualSuspension AccrualSuspension
    {
      get => accrualSuspension ??= new();
      set => accrualSuspension = value;
    }

    /// <summary>
    /// A value of ManualDistributionAudit.
    /// </summary>
    [JsonPropertyName("manualDistributionAudit")]
    public ManualDistributionAudit ManualDistributionAudit
    {
      get => manualDistributionAudit ??= new();
      set => manualDistributionAudit = value;
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

    private LegalActionPerson legalActionPerson;
    private InterstateRequest interstateRequest;
    private InterstateRequestObligation interstateRequestObligation;
    private CsePerson csePerson;
    private InterestSuppStatusHistory interestSuppStatusHistory;
    private CsePersonAccount supported1;
    private CsePerson obligorCsePerson;
    private CsePersonAccount obligorCsePersonAccount;
    private CsePerson concurrentObligorCsePerson;
    private CsePersonAccount concurrentObligorCsePersonAccount;
    private CsePerson supported2;
    private ObligationType obligationType;
    private Obligation obligation;
    private Obligation concurrent;
    private ObligationRlnRsn obligationRlnRsn;
    private ObligationRln obligationRln;
    private ObligationTransaction obligationTransaction;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private AccrualInstructions accrualInstructions;
    private AccrualSuspension accrualSuspension;
    private ManualDistributionAudit manualDistributionAudit;
    private LegalAction legalAction;
    private LegalActionDetail legalActionDetail;
  }
#endregion
}
