// Program: FN_DISPLAY_OBLIG_BY_COURT_ORDER, ID: 371739417, model: 746.
// Short name: SWE00445
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
/// A program: FN_DISPLAY_OBLIG_BY_COURT_ORDER.
/// </para>
/// <para>
/// RESP: FINANCE
/// Get display group for prad list obligations by court order
/// </para>
/// </summary>
[Serializable]
public partial class FnDisplayObligByCourtOrder: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DISPLAY_OBLIG_BY_COURT_ORDER program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDisplayObligByCourtOrder(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDisplayObligByCourtOrder.
  /// </summary>
  public FnDisplayObligByCourtOrder(IContext context, Import import,
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
    // -----------------------------------------------------------------
    //   Developer		Mod Date
    //   D.M. NILSEN		07/10/95              List Obligations by
    //                                               
    // court order.
    //   JeHoward              04/29/97              Current date fix.
    //   Adwait Phadnis        11/12/97             Changed the logic to 
    // calculate undistributed amount
    // E. Parker		10/28/1998		Removed logic that bypassed total calculation on 
    // prim/sec code = "J" as it was incorrect; made "UNPROCESSED..." message
    // display red reverse video; prevented "Summary..." message from displaying
    // ;  Fixed case of prim/sec code logic to calculate totals properly.
    // E. Parker		02/19/1999		Changed logic to display same as OPAY Mthly Due in
    // the 'Payment' field instead of Current Owed.  Also corrected logic to
    // display Monthly Due Totals for P, S, and J Obligations.
    // E. Parker		06/19/1999		Changed logic to use 
    // fn_read_case_no_and_worker_id.  Also changed logic to set error msg by
    // person instead of by screen.
    // B Adams		7/6/99		Read properties set
    // ------------------------------------------------------------------
    // : MBrown, pr# 77675, Oct 14, 1999 - Fixed problem where we were getting
    //  'supported person nf' message for a deactivated obligation.  Do not want
    //  to display this message or call the cab to get case and  worker if a
    //  supported person is not found for a deactivated obligation.
    // : 10/25/99, pr# 77622, M Brown - Add current owed to OCTO screen.
    local.CurrentDate.Date = Now().Date;
    local.Current.Date = local.CurrentDate.Date;
    UseFnHardcodedDebtDistribution();
    local.Tmp.Date = local.CurrentDate.Date;
    local.Current.YearMonth = UseCabGetYearMonthFromDate();
    local.Maximum.Date = UseCabSetMaximumDiscontinueDate();
    local.FirstDayOfMonth.Date =
      AddDays(AddDays(local.Tmp.Date, -Day(local.Tmp.Date)), 1);
    local.FirstDayOfNextMonth.Date = AddMonths(local.Tmp.Date, 1);
    local.FirstDayOfNextMonth.Date =
      AddDays(AddDays(
        local.FirstDayOfNextMonth.Date, -
      Day(local.FirstDayOfNextMonth.Date)), 1);
    export.LegalAction.Assign(import.Search);

    // ***** MAINLINE *****
    // : Verify Court Order is a valid Legal Action Standard Number.
    if (IsEmpty(import.Search.CourtCaseNumber) && IsEmpty
      (import.Search.StandardNumber))
    {
      ExitState = "FN0000_COURT_ORDER_REQ_FOR_FUNC";

      return;
    }

    if (import.Search.Identifier != 0)
    {
      if (ReadLegalAction2())
      {
        MoveLegalAction(entities.LegalAction, local.LegalAction);
      }
      else
      {
        ExitState = "LEGAL_ACTION_NF";

        return;
      }
    }
    else
    {
      MoveLegalAction(import.Search, local.LegalAction);

      if (!IsEmpty(import.Search.StandardNumber))
      {
        if (!ReadLegalAction1())
        {
          ExitState = "LEGAL_ACTION_NF";

          return;
        }
      }
    }

    // ***---  included obligation_type with the Read Each - b adams - 7/6/99
    // =================================================
    // PR# 73246: 9/14/99 - b adams  -  Included LAD since it's
    //   required by CAB changed below.
    // =================================================
    export.Group.Index = 0;
    export.Group.Clear();

    foreach(var item in ReadLegalActionObligationObligationTypeLegalActionDetail())
      
    {
      export.LegalAction.Assign(entities.LegalAction);

      // ***---  there can be many o_p_s rows per obigation
      if (ReadObligationPaymentSchedule())
      {
        // : Continue Processing.
      }
      else if (AsChar(entities.ObligationType.Classification) == 'A')
      {
        ExitState = "FN0000_OBLIG_PYMNT_SCH_NF";
        export.Group.Next();

        return;
      }
      else
      {
        // : Continue Processing.
      }

      if (ReadCsePersonCsePersonAccount())
      {
        local.CsePerson.Number = entities.ObligorCsePerson.Number;

        // : Continue Processing.
      }
      else
      {
        ExitState = "FN0000_OBLIGOR_NF";
        export.Group.Next();

        return;
      }

      UseFnGetObligationStatus();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.Group.Next();

        return;
      }

      if (AsChar(import.ShowDeactivedObligation.SelectChar) != 'Y')
      {
        if (AsChar(local.ScreenObligationStatus.ObligationStatus) == 'D')
        {
          export.Group.Next();

          continue;
        }
      }

      if (!IsEmpty(entities.Obligation.PrimarySecondaryCode))
      {
        if (ReadObligationRlnRsn1())
        {
          // : Continue Processing.
        }
        else if (ReadObligationRlnRsn2())
        {
          // : Continue Processing.
        }
        else
        {
          ExitState = "FN0000_OBLIG_RLN_RSN_NF";
          export.Group.Next();

          return;
        }
      }

      // Oct 19, 1999, M Brown : Removed retrieval of case and worker -
      // they are being removed from the screen.
      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.Group.Next();

        return;
      }

      UseFnCalcAmtsDueForObligation();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.Group.Next();

        return;
      }

      local.OmitUndistAmtInd.Flag = "Y";
      UseFnComputeSummaryTotals();

      if (IsExitState("FN0000_UNPROCESSED_TRANS_EXIST"))
      {
        ExitState = "ACO_NN0000_ALL_OK";
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.Group.Next();

        return;
      }

      local.CsePersonsWorkSet.Number = entities.ObligorCsePerson.Number;
      UseSiReadCsePerson();
      export.Group.Update.CsePerson.Number = entities.ObligorCsePerson.Number;
      export.Group.Update.ObligationType.Assign(entities.ObligationType);
      MoveObligation(entities.Obligation, export.Group.Update.Obligation);

      if (entities.ObligationPaymentSchedule.Populated)
      {
        export.Group.Update.ObligationPaymentSchedule.FrequencyCode =
          entities.ObligationPaymentSchedule.FrequencyCode;
      }

      // ---------------------------------------------
      // Read Manual Distribution Instructions and  set the indicator 
      // accordingly
      // ---------------------------------------------
      if (ReadManualDistributionAudit())
      {
        local.ManualDistInstExists.OneChar = "M";
      }
      else
      {
        local.ManualDistInstExists.OneChar = "";
      }

      // =================================================
      // 7/6/99 - bud adams  -  Deleted persistent read of Obligation.
      //   View was not used anyplace.
      // =================================================
      MoveCsePersonsWorkSet(local.CsePersonsWorkSet,
        export.Group.Update.CsePersonsWorkSet);
      export.Group.Update.LegalAction.Identifier =
        entities.LegalAction.Identifier;

      // : 10/25/99, pr# 77622, M Brown - Add current owed to OCTO screen.
      // Changed the next stmt as part of this.
      export.Group.Update.ScreenOwedAmounts.CurrentAmountOwed =
        local.ScreenOwedAmounts.CurrentAmountOwed;
      export.Group.Update.ScreenOwedAmounts.ArrearsAmountOwed =
        local.ScreenOwedAmounts.ArrearsAmountOwed;
      export.Group.Update.ScreenOwedAmounts.InterestAmountOwed =
        local.ScreenOwedAmounts.InterestAmountOwed;
      export.Group.Update.ScreenOwedAmounts.TotalAmountOwed =
        local.ScreenOwedAmounts.TotalAmountOwed;

      // : 10/25/99, pr# 77622, M Brown - Add current owed to OCTO screen.
      export.Group.Update.ScreenDueAmounts.TotalAmountDue =
        local.ScreenDueAmounts.TotalAmountDue;
      export.Group.Update.ScreenOwedAmounts.ErrorInformationLine =
        local.ScreenOwedAmounts.ErrorInformationLine;
      export.Group.Update.ScreenObligationStatus.ObligationStatus =
        local.ScreenObligationStatus.ObligationStatus;
      export.Group.Update.ServiceProvider.UserId = local.ServiceProvider.UserId;
      export.Group.Update.ScreenObMutliSvcPrvdr.MultiServiceProviderInd =
        local.ScreenObMutliSvcPrvdr.MultiServiceProviderInd;
      export.Group.Update.DetailConcatInds.Text8 =
        export.Group.Item.ObligationType.Classification + export
        .Group.Item.ScreenObligationStatus.ObligationStatus + (
          export.Group.Item.Obligation.PrimarySecondaryCode ?? "") + export
        .Group.Item.Obligation.OrderTypeCode + local
        .ManualDistInstExists.OneChar;

      switch(AsChar(entities.Obligation.PrimarySecondaryCode))
      {
        case 'P':
          export.ScreenOwedAmounts.ArrearsAmountOwed += local.ScreenOwedAmounts.
            ArrearsAmountOwed;
          export.ScreenOwedAmounts.CurrentAmountOwed += local.ScreenOwedAmounts.
            CurrentAmountOwed;
          export.ScreenOwedAmounts.InterestAmountOwed += local.
            ScreenOwedAmounts.InterestAmountOwed;
          export.ScreenOwedAmounts.TotalAmountOwed += local.ScreenOwedAmounts.
            TotalAmountOwed;
          export.ScreenDueAmounts.CurrentAmountDue += local.ScreenDueAmounts.
            CurrentAmountDue;
          export.ScreenDueAmounts.PeriodicAmountDue += local.ScreenDueAmounts.
            PeriodicAmountDue;
          export.ScreenDueAmounts.TotalAmountDue += local.ScreenDueAmounts.
            TotalAmountDue;

          break;
        case 'S':
          export.ScreenDueAmounts.CurrentAmountDue += local.ScreenDueAmounts.
            CurrentAmountDue;
          export.ScreenDueAmounts.PeriodicAmountDue += local.ScreenDueAmounts.
            PeriodicAmountDue;
          export.ScreenDueAmounts.TotalAmountDue += local.ScreenDueAmounts.
            TotalAmountDue;

          break;
        case 'J':
          // ****************************************************************
          // Madhu Kumar                   08/01/2000
          //    Code changed such that the summary is displayed
          //  just for one person in case of a joint several and not
          //  for both of them,which is erronous.
          // ****************************************************************
          if (ReadObligationRln())
          {
            export.ScreenOwedAmounts.ArrearsAmountOwed += local.
              ScreenOwedAmounts.ArrearsAmountOwed;
            export.ScreenOwedAmounts.CurrentAmountOwed += local.
              ScreenOwedAmounts.CurrentAmountOwed;
            export.ScreenOwedAmounts.InterestAmountOwed += local.
              ScreenOwedAmounts.InterestAmountOwed;
            export.ScreenOwedAmounts.TotalAmountOwed += local.ScreenOwedAmounts.
              TotalAmountOwed;
            export.ScreenDueAmounts.CurrentAmountDue += local.ScreenDueAmounts.
              CurrentAmountDue;
            export.ScreenDueAmounts.PeriodicAmountDue += local.ScreenDueAmounts.
              PeriodicAmountDue;
            export.ScreenDueAmounts.TotalAmountDue += local.ScreenDueAmounts.
              TotalAmountDue;
          }
          else
          {
            // *********************************************************
            //  No exit state necessary as we would reach this
            //  condition for all the obligation which are joint
            //  several and which are associated to the
            //  second person.
            // *********************************************************
          }

          break;
        default:
          export.ScreenOwedAmounts.ArrearsAmountOwed += local.ScreenOwedAmounts.
            ArrearsAmountOwed;
          export.ScreenOwedAmounts.CurrentAmountOwed += local.ScreenOwedAmounts.
            CurrentAmountOwed;
          export.ScreenOwedAmounts.InterestAmountOwed += local.
            ScreenOwedAmounts.InterestAmountOwed;
          export.ScreenOwedAmounts.TotalAmountOwed += local.ScreenOwedAmounts.
            TotalAmountOwed;
          export.ScreenDueAmounts.CurrentAmountDue += local.ScreenDueAmounts.
            CurrentAmountDue;
          export.ScreenDueAmounts.PeriodicAmountDue += local.ScreenDueAmounts.
            PeriodicAmountDue;
          export.ScreenDueAmounts.TotalAmountDue += local.ScreenDueAmounts.
            TotalAmountDue;

          break;
      }

      export.Group.Next();
    }

    // Verify Court Order for Multiple Payee situation
    for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
      export.Group.Index)
    {
      local.CsePerson.Number = export.Group.Item.CsePerson.Number;

      local.Test.Index = 0;
      local.Test.CheckSize();

      local.Test.Update.Test1.Number = export.Group.Item.CsePerson.Number;

      break;
    }

    if (IsEmpty(local.CsePerson.Number))
    {
      ExitState = "LEGAL_ACTION_NF";

      return;
    }

    for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
      export.Group.Index)
    {
      if (!Equal(export.Group.Item.CsePerson.Number, local.CsePerson.Number))
      {
        export.MultiPayor.Flag = "Y";

        break;
      }
    }

    // : calculate the undistributed amount for each Obligor.
    local.LoopFlag.Count = 2;

    for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
      export.Group.Index)
    {
      if (IsEmpty(export.Group.Item.CsePerson.Number))
      {
        break;
      }

      local.LoopFlag.Flag = "N";

      for(local.Test.Index = 0; local.Test.Index < Local.TestGroup.Capacity; ++
        local.Test.Index)
      {
        if (!local.Test.CheckSize())
        {
          break;
        }

        if (IsEmpty(local.Test.Item.Test1.Number))
        {
          break;
        }

        if (Equal(export.Group.Item.CsePerson.Number,
          local.Test.Item.Test1.Number))
        {
          goto Next;
        }
        else
        {
          local.LoopFlag.Flag = "Y";
        }
      }

      local.Test.CheckIndex();

      if (AsChar(local.LoopFlag.Flag) == 'Y')
      {
        local.Test.Index = local.LoopFlag.Count - 1;
        local.Test.CheckSize();

        local.Test.Update.Test1.Number = export.Group.Item.CsePerson.Number;
        ++local.LoopFlag.Count;
      }

Next:
      ;
    }

    for(local.Test.Index = 0; local.Test.Index < local.Test.Count; ++
      local.Test.Index)
    {
      if (!local.Test.CheckSize())
      {
        break;
      }

      if (IsEmpty(local.Test.Item.Test1.Number))
      {
        return;
      }

      UseFnComputeUndistributedAmount();
      export.Undistributed.TotalCurrency += local.UndistributedAmount.
        TotalCurrency;
    }

    local.Test.CheckIndex();
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
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveObligation(Obligation source, Obligation target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.PrimarySecondaryCode = source.PrimarySecondaryCode;
    target.OrderTypeCode = source.OrderTypeCode;
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Classification = source.Classification;
  }

  private int UseCabGetYearMonthFromDate()
  {
    var useImport = new CabGetYearMonthFromDate.Import();
    var useExport = new CabGetYearMonthFromDate.Export();

    useImport.DateWorkArea.Date = local.Tmp.Date;

    Call(CabGetYearMonthFromDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.YearMonth;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.Maximum.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseFnCalcAmtsDueForObligation()
  {
    var useImport = new FnCalcAmtsDueForObligation.Import();
    var useExport = new FnCalcAmtsDueForObligation.Export();

    useImport.CsePerson.Number = entities.ObligorCsePerson.Number;
    MoveObligationType(entities.ObligationType, useImport.ObligationType);
    useImport.Obligation.SystemGeneratedIdentifier =
      entities.Obligation.SystemGeneratedIdentifier;

    Call(FnCalcAmtsDueForObligation.Execute, useImport, useExport);

    local.ScreenDueAmounts.Assign(useExport.ScreenDueAmounts);
  }

  private void UseFnComputeSummaryTotals()
  {
    var useImport = new FnComputeSummaryTotals.Import();
    var useExport = new FnComputeSummaryTotals.Export();

    useImport.Obligor.Number = entities.ObligorCsePerson.Number;
    useImport.FilterByIdObligationType.SystemGeneratedIdentifier =
      entities.ObligationType.SystemGeneratedIdentifier;
    useImport.Filter.SystemGeneratedIdentifier =
      entities.Obligation.SystemGeneratedIdentifier;
    useImport.OmitUndistAmtInd.Flag = local.OmitUndistAmtInd.Flag;

    Call(FnComputeSummaryTotals.Execute, useImport, useExport);

    local.ScreenOwedAmounts.Assign(useExport.ScreenOwedAmounts);
    local.UndistributedAmount.TotalCurrency = useExport.UndistAmt.TotalCurrency;
  }

  private void UseFnComputeUndistributedAmount()
  {
    var useImport = new FnComputeUndistributedAmount.Import();
    var useExport = new FnComputeUndistributedAmount.Export();

    useImport.CsePerson.Number = local.Test.Item.Test1.Number;

    Call(FnComputeUndistributedAmount.Execute, useImport, useExport);

    local.UndistributedAmount.TotalCurrency =
      useExport.UndistAmount.TotalCurrency;
  }

  private void UseFnGetObligationStatus()
  {
    var useImport = new FnGetObligationStatus.Import();
    var useExport = new FnGetObligationStatus.Export();

    useImport.ObligationType.Assign(entities.ObligationType);
    useImport.Current.Date = local.CurrentDate.Date;
    useImport.CsePersonAccount.Type1 = local.HardcodeObligorType.Type1;
    useImport.CsePerson.Number = entities.ObligorCsePerson.Number;
    useImport.Obligation.SystemGeneratedIdentifier =
      entities.Obligation.SystemGeneratedIdentifier;
    useImport.HcOtCAccruing.Classification =
      local.HardcodeOtCAccruing.Classification;
    useImport.HcOtCVoluntary.Classification =
      local.HcOtCVoluntary.Classification;

    Call(FnGetObligationStatus.Execute, useImport, useExport);

    local.ScreenObligationStatus.ObligationStatus =
      useExport.ScreenObligationStatus.ObligationStatus;
  }

  private void UseFnHardcodedDebtDistribution()
  {
    var useImport = new FnHardcodedDebtDistribution.Import();
    var useExport = new FnHardcodedDebtDistribution.Export();

    Call(FnHardcodedDebtDistribution.Execute, useImport, useExport);

    local.HardcodeObligorType.Type1 = useExport.CpaObligor.Type1;
    local.HcOtCVoluntary.Classification =
      useExport.OtCVoluntaryClassification.Classification;
    local.HardcodeOtCAccruing.Classification =
      useExport.OtCAccruingClassification.Classification;
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
  }

  private bool ReadCsePersonCsePersonAccount()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligorCsePerson.Populated = false;
    entities.ObligorCsePersonAccount.Populated = false;

    return Read("ReadCsePersonCsePersonAccount",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetString(command, "type", local.HardcodeObligorType.Type1);
      },
      (db, reader) =>
      {
        entities.ObligorCsePerson.Number = db.GetString(reader, 0);
        entities.ObligorCsePerson.Type1 = db.GetString(reader, 1);
        entities.ObligorCsePerson.OrganizationName =
          db.GetNullableString(reader, 2);
        entities.ObligorCsePersonAccount.CspNumber = db.GetString(reader, 3);
        entities.ObligorCsePersonAccount.Type1 = db.GetString(reader, 4);
        entities.ObligorCsePerson.Populated = true;
        entities.ObligorCsePersonAccount.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ObligorCsePerson.Type1);
        CheckValid<CsePersonAccount>("Type1",
          entities.ObligorCsePersonAccount.Type1);
      });
  }

  private bool ReadLegalAction1()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", import.Search.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction2()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction2",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", import.Search.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.Populated = true;
      });
  }

  private IEnumerable<bool>
    ReadLegalActionObligationObligationTypeLegalActionDetail()
  {
    return ReadEach("ReadLegalActionObligationObligationTypeLegalActionDetail",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", local.LegalAction.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 2);
        entities.Obligation.CpaType = db.GetString(reader, 3);
        entities.Obligation.CspNumber = db.GetString(reader, 4);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 5);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 6);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 6);
        entities.Obligation.Description = db.GetNullableString(reader, 7);
        entities.Obligation.HistoryInd = db.GetNullableString(reader, 8);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 9);
        entities.Obligation.PreConversionDebtNumber =
          db.GetNullableInt32(reader, 10);
        entities.Obligation.PreConversionCaseNumber =
          db.GetNullableString(reader, 11);
        entities.Obligation.AsOfDtNadArrBal = db.GetNullableDecimal(reader, 12);
        entities.Obligation.AsOfDtNadIntBal = db.GetNullableDecimal(reader, 13);
        entities.Obligation.AsOfDtAdcArrBal = db.GetNullableDecimal(reader, 14);
        entities.Obligation.AsOfDtAdcIntBal = db.GetNullableDecimal(reader, 15);
        entities.Obligation.AsOfDtRecBal = db.GetNullableDecimal(reader, 16);
        entities.Obligation.AsOdDtRecIntBal = db.GetNullableDecimal(reader, 17);
        entities.Obligation.AsOfDtFeeBal = db.GetNullableDecimal(reader, 18);
        entities.Obligation.AsOfDtFeeIntBal = db.GetNullableDecimal(reader, 19);
        entities.Obligation.AsOfDtTotBalCurrArr =
          db.GetNullableDecimal(reader, 20);
        entities.Obligation.TillDtCsCollCurrArr =
          db.GetNullableDecimal(reader, 21);
        entities.Obligation.TillDtSpCollCurrArr =
          db.GetNullableDecimal(reader, 22);
        entities.Obligation.TillDtMsCollCurrArr =
          db.GetNullableDecimal(reader, 23);
        entities.Obligation.TillDtNadArrColl =
          db.GetNullableDecimal(reader, 24);
        entities.Obligation.TillDtNadIntColl =
          db.GetNullableDecimal(reader, 25);
        entities.Obligation.TillDtAdcArrColl =
          db.GetNullableDecimal(reader, 26);
        entities.Obligation.TillDtAdcIntColl =
          db.GetNullableDecimal(reader, 27);
        entities.Obligation.AsOfDtTotRecColl =
          db.GetNullableDecimal(reader, 28);
        entities.Obligation.AsOfDtTotRecIntColl =
          db.GetNullableDecimal(reader, 29);
        entities.Obligation.AsOfDtTotFeeColl =
          db.GetNullableDecimal(reader, 30);
        entities.Obligation.AsOfDtTotFeeIntColl =
          db.GetNullableDecimal(reader, 31);
        entities.Obligation.AsOfDtTotCollAll =
          db.GetNullableDecimal(reader, 32);
        entities.Obligation.LastCollAmt = db.GetNullableDecimal(reader, 33);
        entities.Obligation.LastCollDt = db.GetNullableDate(reader, 34);
        entities.Obligation.CreatedBy = db.GetString(reader, 35);
        entities.Obligation.CreatedTmst = db.GetDateTime(reader, 36);
        entities.Obligation.LastUpdatedBy = db.GetNullableString(reader, 37);
        entities.Obligation.LastUpdateTmst = db.GetNullableDateTime(reader, 38);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 39);
        entities.Obligation.LgaIdentifier = db.GetNullableInt32(reader, 40);
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 40);
        entities.Obligation.LadNumber = db.GetNullableInt32(reader, 41);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 41);
        entities.ObligationType.Code = db.GetString(reader, 42);
        entities.ObligationType.Classification = db.GetString(reader, 43);
        entities.ObligationType.SupportedPersonReqInd =
          db.GetString(reader, 44);
        entities.ObligationType.Populated = true;
        entities.Obligation.Populated = true;
        entities.LegalAction.Populated = true;
        entities.LegalActionDetail.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.Obligation.OrderTypeCode);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
        CheckValid<ObligationType>("SupportedPersonReqInd",
          entities.ObligationType.SupportedPersonReqInd);

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
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
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
        entities.ObligationPaymentSchedule.EndDt =
          db.GetNullableDate(reader, 5);
        entities.ObligationPaymentSchedule.FrequencyCode =
          db.GetString(reader, 6);
        entities.ObligationPaymentSchedule.Populated = true;
        CheckValid<ObligationPaymentSchedule>("ObgCpaType",
          entities.ObligationPaymentSchedule.ObgCpaType);
        CheckValid<ObligationPaymentSchedule>("FrequencyCode",
          entities.ObligationPaymentSchedule.FrequencyCode);
      });
  }

  private bool ReadObligationRln()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationRln.Populated = false;

    return Read("ReadObligationRln",
      (db, command) =>
      {
        db.SetInt32(command, "otyFirstId", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgFGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspFNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaFType", entities.Obligation.CpaType);
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
        entities.ObligationRln.CreatedTmst = db.GetDateTime(reader, 7);
        entities.ObligationRln.OtySecondId = db.GetInt32(reader, 8);
        entities.ObligationRln.OtyFirstId = db.GetInt32(reader, 9);
        entities.ObligationRln.Populated = true;
        CheckValid<ObligationRln>("CpaType", entities.ObligationRln.CpaType);
        CheckValid<ObligationRln>("CpaFType", entities.ObligationRln.CpaFType);
      });
  }

  private bool ReadObligationRlnRsn1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationRlnRsn.Populated = false;

    return Read("ReadObligationRlnRsn1",
      (db, command) =>
      {
        db.SetInt32(command, "otyFirstId", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgFGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspFNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaFType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        entities.ObligationRlnRsn.SequentialGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationRlnRsn.Code = db.GetString(reader, 1);
        entities.ObligationRlnRsn.Populated = true;
      });
  }

  private bool ReadObligationRlnRsn2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationRlnRsn.Populated = false;

    return Read("ReadObligationRlnRsn2",
      (db, command) =>
      {
        db.SetInt32(command, "otySecondId", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        entities.ObligationRlnRsn.SequentialGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationRlnRsn.Code = db.GetString(reader, 1);
        entities.ObligationRlnRsn.Populated = true;
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
    /// A value of ShowDeactivedObligation.
    /// </summary>
    [JsonPropertyName("showDeactivedObligation")]
    public Common ShowDeactivedObligation
    {
      get => showDeactivedObligation ??= new();
      set => showDeactivedObligation = value;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public LegalAction Search
    {
      get => search ??= new();
      set => search = value;
    }

    private Common showDeactivedObligation;
    private LegalAction search;
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
      /// A value of DetailConcatInds.
      /// </summary>
      [JsonPropertyName("detailConcatInds")]
      public TextWorkArea DetailConcatInds
      {
        get => detailConcatInds ??= new();
        set => detailConcatInds = value;
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
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
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
      /// A value of CsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("csePersonsWorkSet")]
      public CsePersonsWorkSet CsePersonsWorkSet
      {
        get => csePersonsWorkSet ??= new();
        set => csePersonsWorkSet = value;
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
      /// A value of ScreenObligationStatus.
      /// </summary>
      [JsonPropertyName("screenObligationStatus")]
      public ScreenObligationStatus ScreenObligationStatus
      {
        get => screenObligationStatus ??= new();
        set => screenObligationStatus = value;
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
      /// A value of ServiceProvider.
      /// </summary>
      [JsonPropertyName("serviceProvider")]
      public ServiceProvider ServiceProvider
      {
        get => serviceProvider ??= new();
        set => serviceProvider = value;
      }

      /// <summary>
      /// A value of ScreenObMutliSvcPrvdr.
      /// </summary>
      [JsonPropertyName("screenObMutliSvcPrvdr")]
      public ScreenObMutliSvcPrvdr ScreenObMutliSvcPrvdr
      {
        get => screenObMutliSvcPrvdr ??= new();
        set => screenObMutliSvcPrvdr = value;
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
      /// A value of HiddenAmtOwedUnavl.
      /// </summary>
      [JsonPropertyName("hiddenAmtOwedUnavl")]
      public Common HiddenAmtOwedUnavl
      {
        get => hiddenAmtOwedUnavl ??= new();
        set => hiddenAmtOwedUnavl = value;
      }

      /// <summary>
      /// A value of ScreenDueAmounts.
      /// </summary>
      [JsonPropertyName("screenDueAmounts")]
      public ScreenDueAmounts ScreenDueAmounts
      {
        get => screenDueAmounts ??= new();
        set => screenDueAmounts = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 80;

      private TextWorkArea detailConcatInds;
      private LegalAction legalAction;
      private Common common;
      private CsePerson csePerson;
      private CsePersonsWorkSet csePersonsWorkSet;
      private ObligationType obligationType;
      private Obligation obligation;
      private ScreenObligationStatus screenObligationStatus;
      private ObligationPaymentSchedule obligationPaymentSchedule;
      private ServiceProvider serviceProvider;
      private ScreenObMutliSvcPrvdr screenObMutliSvcPrvdr;
      private ScreenOwedAmounts screenOwedAmounts;
      private Common hiddenAmtOwedUnavl;
      private ScreenDueAmounts screenDueAmounts;
    }

    /// <summary>
    /// A value of MultiPayor.
    /// </summary>
    [JsonPropertyName("multiPayor")]
    public Common MultiPayor
    {
      get => multiPayor ??= new();
      set => multiPayor = value;
    }

    /// <summary>
    /// A value of ScreenDueAmounts.
    /// </summary>
    [JsonPropertyName("screenDueAmounts")]
    public ScreenDueAmounts ScreenDueAmounts
    {
      get => screenDueAmounts ??= new();
      set => screenDueAmounts = value;
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
    /// A value of Undistributed.
    /// </summary>
    [JsonPropertyName("undistributed")]
    public Common Undistributed
    {
      get => undistributed ??= new();
      set => undistributed = value;
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

    private Common multiPayor;
    private ScreenDueAmounts screenDueAmounts;
    private LegalAction legalAction;
    private Common undistributed;
    private ScreenOwedAmounts screenOwedAmounts;
    private Array<GroupGroup> group;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A TestGroup group.</summary>
    [Serializable]
    public class TestGroup
    {
      /// <summary>
      /// A value of Test1.
      /// </summary>
      [JsonPropertyName("test1")]
      public CsePerson Test1
      {
        get => test1 ??= new();
        set => test1 = value;
      }

      /// <summary>
      /// A value of TestGrpProcessedInd.
      /// </summary>
      [JsonPropertyName("testGrpProcessedInd")]
      public Common TestGrpProcessedInd
      {
        get => testGrpProcessedInd ??= new();
        set => testGrpProcessedInd = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 96;

      private CsePerson test1;
      private Common testGrpProcessedInd;
    }

    /// <summary>
    /// A value of HcOtCVoluntary.
    /// </summary>
    [JsonPropertyName("hcOtCVoluntary")]
    public ObligationType HcOtCVoluntary
    {
      get => hcOtCVoluntary ??= new();
      set => hcOtCVoluntary = value;
    }

    /// <summary>
    /// A value of HardcodeOtCAccruing.
    /// </summary>
    [JsonPropertyName("hardcodeOtCAccruing")]
    public ObligationType HardcodeOtCAccruing
    {
      get => hardcodeOtCAccruing ??= new();
      set => hardcodeOtCAccruing = value;
    }

    /// <summary>
    /// A value of OmitUndistAmtInd.
    /// </summary>
    [JsonPropertyName("omitUndistAmtInd")]
    public Common OmitUndistAmtInd
    {
      get => omitUndistAmtInd ??= new();
      set => omitUndistAmtInd = value;
    }

    /// <summary>
    /// A value of Multi.
    /// </summary>
    [JsonPropertyName("multi")]
    public Common Multi
    {
      get => multi ??= new();
      set => multi = value;
    }

    /// <summary>
    /// A value of LoopFlag.
    /// </summary>
    [JsonPropertyName("loopFlag")]
    public Common LoopFlag
    {
      get => loopFlag ??= new();
      set => loopFlag = value;
    }

    /// <summary>
    /// A value of Subscript.
    /// </summary>
    [JsonPropertyName("subscript")]
    public Common Subscript
    {
      get => subscript ??= new();
      set => subscript = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// Gets a value of Test.
    /// </summary>
    [JsonIgnore]
    public Array<TestGroup> Test => test ??= new(TestGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Test for json serialization.
    /// </summary>
    [JsonPropertyName("test")]
    [Computed]
    public IList<TestGroup> Test_Json
    {
      get => test;
      set => Test.Assign(value);
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of ScreenObligationStatus.
    /// </summary>
    [JsonPropertyName("screenObligationStatus")]
    public ScreenObligationStatus ScreenObligationStatus
    {
      get => screenObligationStatus ??= new();
      set => screenObligationStatus = value;
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
    /// A value of ScreenDueAmounts.
    /// </summary>
    [JsonPropertyName("screenDueAmounts")]
    public ScreenDueAmounts ScreenDueAmounts
    {
      get => screenDueAmounts ??= new();
      set => screenDueAmounts = value;
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
    /// A value of ScreenObMutliSvcPrvdr.
    /// </summary>
    [JsonPropertyName("screenObMutliSvcPrvdr")]
    public ScreenObMutliSvcPrvdr ScreenObMutliSvcPrvdr
    {
      get => screenObMutliSvcPrvdr ??= new();
      set => screenObMutliSvcPrvdr = value;
    }

    /// <summary>
    /// A value of UndistributedAmount.
    /// </summary>
    [JsonPropertyName("undistributedAmount")]
    public Common UndistributedAmount
    {
      get => undistributedAmount ??= new();
      set => undistributedAmount = value;
    }

    /// <summary>
    /// A value of HardcodeObligorType.
    /// </summary>
    [JsonPropertyName("hardcodeObligorType")]
    public CsePersonAccount HardcodeObligorType
    {
      get => hardcodeObligorType ??= new();
      set => hardcodeObligorType = value;
    }

    /// <summary>
    /// A value of HardcodeJointAndSeveralType.
    /// </summary>
    [JsonPropertyName("hardcodeJointAndSeveralType")]
    public ObligationRlnRsn HardcodeJointAndSeveralType
    {
      get => hardcodeJointAndSeveralType ??= new();
      set => hardcodeJointAndSeveralType = value;
    }

    /// <summary>
    /// A value of Tmp.
    /// </summary>
    [JsonPropertyName("tmp")]
    public DateWorkArea Tmp
    {
      get => tmp ??= new();
      set => tmp = value;
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
    /// A value of FirstDayOfNextMonth.
    /// </summary>
    [JsonPropertyName("firstDayOfNextMonth")]
    public DateWorkArea FirstDayOfNextMonth
    {
      get => firstDayOfNextMonth ??= new();
      set => firstDayOfNextMonth = value;
    }

    /// <summary>
    /// A value of FirstDayOfMonth.
    /// </summary>
    [JsonPropertyName("firstDayOfMonth")]
    public DateWorkArea FirstDayOfMonth
    {
      get => firstDayOfMonth ??= new();
      set => firstDayOfMonth = value;
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
    /// A value of ManualDistInstExists.
    /// </summary>
    [JsonPropertyName("manualDistInstExists")]
    public Standard ManualDistInstExists
    {
      get => manualDistInstExists ??= new();
      set => manualDistInstExists = value;
    }

    private ObligationType hcOtCVoluntary;
    private ObligationType hardcodeOtCAccruing;
    private Common omitUndistAmtInd;
    private Common multi;
    private Common loopFlag;
    private Common subscript;
    private DateWorkArea currentDate;
    private LegalAction legalAction;
    private Array<TestGroup> test;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePerson csePerson;
    private ScreenObligationStatus screenObligationStatus;
    private ServiceProvider serviceProvider;
    private ScreenDueAmounts screenDueAmounts;
    private ScreenOwedAmounts screenOwedAmounts;
    private ScreenObMutliSvcPrvdr screenObMutliSvcPrvdr;
    private Common undistributedAmount;
    private CsePersonAccount hardcodeObligorType;
    private ObligationRlnRsn hardcodeJointAndSeveralType;
    private DateWorkArea tmp;
    private DateWorkArea current;
    private DateWorkArea firstDayOfNextMonth;
    private DateWorkArea firstDayOfMonth;
    private DateWorkArea maximum;
    private Standard manualDistInstExists;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
    }

    /// <summary>
    /// A value of CsePersonLegalAction.
    /// </summary>
    [JsonPropertyName("csePersonLegalAction")]
    public LegalAction CsePersonLegalAction
    {
      get => csePersonLegalAction ??= new();
      set => csePersonLegalAction = value;
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
    /// A value of ObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("obligationPaymentSchedule")]
    public ObligationPaymentSchedule ObligationPaymentSchedule
    {
      get => obligationPaymentSchedule ??= new();
      set => obligationPaymentSchedule = value;
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
    /// A value of ManualDistributionAudit.
    /// </summary>
    [JsonPropertyName("manualDistributionAudit")]
    public ManualDistributionAudit ManualDistributionAudit
    {
      get => manualDistributionAudit ??= new();
      set => manualDistributionAudit = value;
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

    private CsePersonAccount supportedCsePersonAccount;
    private CsePerson supportedCsePerson;
    private CsePersonAccount csePersonAccount;
    private LegalAction csePersonLegalAction;
    private CsePerson obligorCsePerson;
    private CsePersonAccount obligorCsePersonAccount;
    private ObligationType obligationType;
    private Obligation obligation;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private ObligationRlnRsn obligationRlnRsn;
    private ObligationRln obligationRln;
    private LegalAction legalAction;
    private LegalActionDetail legalActionDetail;
    private ManualDistributionAudit manualDistributionAudit;
    private ObligationTransaction obligationTransaction;
  }
#endregion
}
