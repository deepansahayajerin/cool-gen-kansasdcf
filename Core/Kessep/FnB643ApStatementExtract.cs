// Program: FN_B643_AP_STATEMENT_EXTRACT, ID: 372683700, model: 746.
// Short name: SWEF643B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B643_AP_STATEMENT_EXTRACT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB643ApStatementExtract: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B643_AP_STATEMENT_EXTRACT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB643ApStatementExtract(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB643ApStatementExtract.
  /// </summary>
  public FnB643ApStatementExtract(IContext context, Import import, Export export)
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
    // ********************************************************************
    // * DATE       NAME         REFER   DESCRIPTION
    // * ---------- ------------ ------  ------------------
    // * 08-05-1999 Ed Lyman     PR 410  No amount due on coupons
    // *
    // * 08-05-1999 Ed Lyman     PR 411  Voluntary debts with no activity
    // *
    // 
    // should not print.
    // *
    // * 08-23-1999 Ed Lyman     PR 413  Overpayment Intent not reported 
    // correctly.
    // *
    // 
    // Suppress when future payment.
    // *
    // 
    // Consolidate coupons where needed.
    // *
    // * 09-21-1999 Ed Lyman     PR 414  Do not report deactivated debts as 
    // errors.
    // *
    // * 08-26-1999 Ed Lyman     PR 415  Suppress coupons on IWO.
    // *
    // * 09-26-1999 Ed Lyman     PR 417  Joint and Several not correct.
    // *
    // * 09-26-1999 Ed Lyman     PR 418  Use real court order number.
    // *
    // * 11-05-1999 Ed Lyman     #79173  Add hard codes to Housekeeping.
    // *
    // ********************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.EabFileHandling.Action = "WRITE";
    UseFnB643Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // **********************************************************
    // Obligor is a subtype of cse_person_account.
    // Client is a subtype of cse_person.
    // **********************************************************
    foreach(var item in ReadObligorClient())
    {
      ++local.ObligorsRead.Count;
      local.CurrStmtNumThisObligor.Count = 1;
      local.PrevStmtNumThisObligor.Count = 0;
      local.Prev.StandardNumber = "";
      UseFnB643ObligorSuppression();

      if (AsChar(local.SuppressBothByObligor.Flag) == 'Y')
      {
        continue;
      }

      // ****************************************************************
      // First, process any non Court ordered obligations.  Note
      // that these are handled by Central Receivables and can
      // all go on one statement, separate from Court ordered.
      // ****************************************************************
      foreach(var item1 in ReadObligation())
      {
        if (ReadLegalAction())
        {
          continue;
        }

        UseFnB643ObligationSuppression();

        if (AsChar(local.SuppressBothByDebt.Flag) == 'Y')
        {
          continue;
        }

        if (ReadObligationType())
        {
          if (Equal(entities.ObligationType.Code, "VOL"))
          {
            local.ActivityOnVoluntary.Flag = "N";
            UseFnB643ActivityOnVoluntary();

            if (AsChar(local.ActivityOnVoluntary.Flag) == 'N')
            {
              continue;
            }
          }
          else
          {
          }
        }
        else
        {
          local.EabReportSend.RptDetail =
            "Stmt and Cpns not printed for person number = " + entities
            .Client.Number + " because OBLIGATION TYPE not found for an obligation within Court Order = " +
            entities.LegalAction.StandardNumber;
          local.EabReportSend.RptDetail =
            "Statement not printed because OBLIGATION TYPE not found for an obligation within Court Order = " +
            entities.LegalAction.StandardNumber;
          UseCabErrorReport();

          continue;
        }

        if (local.CurrStmtNumThisObligor.Count > local
          .PrevStmtNumThisObligor.Count)
        {
          // ****************************************************************
          // OBTAIN HEADING INFORMATION FOR OBLIGOR'S STATEMENT
          // ****************************************************************
          local.Obligor.Number = entities.Client.Number;
          UseFnB643StatementHeading();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            if (IsExitState("ACO_NE0000_INVALID_ACTION") || IsExitState
              ("FN0000_CSE_PERSON_UNKNOWN"))
            {
              ExitState = "ACO_NN0000_ALL_OK";

              goto ReadEach1;
            }

            goto ReadEach2;
          }

          local.PrevStmtNumThisObligor.Count =
            local.CurrStmtNumThisObligor.Count;
          UseFnB643Undistributed();
        }

        local.NewCourtOrder.Flag = "N";
        UseFnB643ObligationHeading2();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          goto ReadEach2;
        }

        UseFnB643Activity2();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          goto ReadEach2;
        }

        if (AsChar(local.SuppressCpnsByDebt.Flag) == 'Y' || AsChar
          (local.SuppressCpnsByObligor.Flag) == 'Y')
        {
          continue;
        }

        UseFnB643Coupons2();

        if (IsExitState("FN0000_MTH_OBLIGOR_SUM_NF"))
        {
          local.EabReportSend.RptDetail =
            "Coupons suppressed for person number = " + entities
            .Client.Number + " " + "monthly obligation summary is missing.";
          UseCabErrorReport();
          ExitState = "ACO_NN0000_ALL_OK";
        }
        else if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          goto ReadEach2;
        }
      }

      if (local.CurrStmtNumThisObligor.Count == local
        .PrevStmtNumThisObligor.Count)
      {
        ++local.CurrStmtNumThisObligor.Count;
      }

      // ****************************************************************
      // READ EACH LEGAL ACTION HERE AND DETERMINE STMT NUMBER
      // EACH LEGAL ACTION WITH DIFFERENT ADDRESS WILL GET A SEPARATE STATEMENT
      // ****************************************************************
      foreach(var item1 in ReadLegalActionObligation())
      {
        UseFnB643ObligationSuppression();

        if (AsChar(local.SuppressBothByDebt.Flag) == 'Y')
        {
          continue;
        }

        if (AsChar(local.SuppressCpnsByDebt.Flag) == 'Y' || AsChar
          (local.SuppressCpnsByObligor.Flag) == 'Y')
        {
        }
        else
        {
          // ****************************************************************
          // Is there an IWO associated with this Legal Action?
          // ****************************************************************
          UseFnB643IwoCouponSuppression();
        }

        // ****************************************************************
        // Create a new statement for each new legal action.
        // ****************************************************************
        if (!Equal(entities.LegalAction.StandardNumber,
          local.Prev.StandardNumber))
        {
          local.NewCourtOrder.Flag = "Y";

          if (local.CurrStmtNumThisObligor.Count == local
            .PrevStmtNumThisObligor.Count)
          {
            ++local.CurrStmtNumThisObligor.Count;
          }

          local.Prev.StandardNumber = entities.LegalAction.StandardNumber;

          if (!ReadTribunalFipsTribAddress())
          {
            local.EabReportSend.RptDetail =
              "Stmt and Cpns not printed for person number = " + entities
              .Client.Number + "because FIPS TRIBUNAL ADDRESS not found for obligation within Court Order = " +
              entities.LegalAction.StandardNumber;
            UseCabErrorReport();

            continue;
          }
        }
        else
        {
          local.NewCourtOrder.Flag = "N";
        }

        if (ReadObligationType())
        {
          if (Equal(entities.ObligationType.Code, "VOL"))
          {
            local.ActivityOnVoluntary.Flag = "N";
            UseFnB643ActivityOnVoluntary();

            if (AsChar(local.ActivityOnVoluntary.Flag) == 'N')
            {
              continue;
            }
          }
          else
          {
          }
        }
        else
        {
          local.EabReportSend.RptDetail =
            "Stmt and Cpns not printed for person number = " + entities
            .Client.Number + " because OBLIGATION TYPE not found for an obligation within Court Order = " +
            entities.LegalAction.StandardNumber;
          local.EabReportSend.RptDetail =
            "Statement not printed because OBLIGATION TYPE not found for an obligation within Court Order = " +
            entities.LegalAction.StandardNumber;
          UseCabErrorReport();

          continue;
        }

        if (local.CurrStmtNumThisObligor.Count > local
          .PrevStmtNumThisObligor.Count)
        {
          // ****************************************************************
          // OBTAIN HEADING INFORMATION FOR OBLIGOR'S STATEMENT
          // ****************************************************************
          local.Obligor.Number = entities.Client.Number;
          UseFnB643StatementHeading();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            if (IsExitState("ACO_NE0000_INVALID_ACTION") || IsExitState
              ("FN0000_CSE_PERSON_UNKNOWN"))
            {
              ExitState = "ACO_NN0000_ALL_OK";

              continue;
            }

            goto ReadEach2;
          }

          local.PrevStmtNumThisObligor.Count =
            local.CurrStmtNumThisObligor.Count;
        }

        if (AsChar(local.NewCourtOrder.Flag) == 'Y')
        {
          UseFnB643Undistributed();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            goto ReadEach2;
          }

          UseFnB643CourtOrder();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            goto ReadEach2;
          }
        }

        UseFnB643ObligationHeading1();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          goto ReadEach2;
        }

        UseFnB643Activity1();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          goto ReadEach2;
        }

        if (AsChar(local.SuppressCpnsByDebt.Flag) == 'Y' || AsChar
          (local.SuppressCpnsByObligor.Flag) == 'Y')
        {
          continue;
        }

        UseFnB643Coupons1();

        if (IsExitState("FN0000_MTH_OBLIGOR_SUM_NF"))
        {
          local.EabReportSend.RptDetail =
            "Coupons suppressed for person number = " + entities
            .Client.Number + " " + "monthly obligation summary is missing.";
          UseCabErrorReport();
          ExitState = "ACO_NN0000_ALL_OK";
        }
        else if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          goto ReadEach2;
        }
      }

ReadEach1:
      ;
    }

ReadEach2:

    // **********************************************************
    // CLOSE ADABAS IS AVAILABLE
    // **********************************************************
    local.Obligor.Number = "CLOSE";
    UseEabReadCsePersonBatch();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseEabExtractExitStateMessage();
      local.EabReportSend.RptDetail =
        "Error message follows for person number = " + entities
        .Client.Number + " " + local.ExitStateWorkArea.Message;
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ALL_OK";
      UseFnB643WriteControlsAndClose();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
    else
    {
      UseFnB643WriteControlsAndClose();
    }
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.Code = source.Code;
    target.Name = source.Name;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseEabReadCsePersonBatch()
  {
    var useImport = new EabReadCsePersonBatch.Import();
    var useExport = new EabReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.Obligor.Number;
    useExport.AbendData.Assign(local.AbendData);

    Call(EabReadCsePersonBatch.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
  }

  private void UseFnB643Activity1()
  {
    var useImport = new FnB643Activity.Import();
    var useExport = new FnB643Activity.Export();

    useImport.Beginning.YearMonth = local.Beginning.YearMonth;
    useImport.Ending.YearMonth = local.Ending.YearMonth;
    useImport.Obligor.Type1 = entities.Obligor.Type1;
    useImport.StmtBeginTextWorkArea.Text10 = local.StmtBegin.Text10;
    MoveDateWorkArea(local.BeginStmt, useImport.StmtBeginDateWorkArea);
    useImport.StmtEndTextWorkArea.Text10 = local.StmtEnd.Text10;
    MoveDateWorkArea(local.EndStmt, useImport.StmtEndDateWorkArea);
    useImport.StmtNumber.Count = local.CurrStmtNumThisObligor.Count;
    useImport.VendorFileRecordCount.Count = local.VendorFileRecordCount.Count;
    useImport.SortSequenceNumber.Count = local.SortSequenceNumber.Count;
    useImport.ProcessDate.Date = local.Process.Date;
    useImport.ObligationType.Assign(entities.ObligationType);
    useImport.CsePerson.Assign(entities.Client);
    useImport.Obligation.Assign(entities.Obligation);
    useImport.SuppressCouponsByDebt.Flag = local.SuppressCpnsByDebt.Flag;
    useImport.CpnsSuppressedByDebt.Count = local.StmtSuppressedByDebt.Count;

    Call(FnB643Activity.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    local.VendorFileRecordCount.Count = useExport.VendorFileRecordCount.Count;
    local.SortSequenceNumber.Count = useExport.SortSequenceNumber.Count;
    local.SuppressCpnsByDebt.Flag = useExport.SuppressCouponsByDebt.Flag;
    local.CpnsSuppressedByDebt.Count = useExport.CpnsSuppressedByDebt.Count;
  }

  private void UseFnB643Activity2()
  {
    var useImport = new FnB643Activity.Import();
    var useExport = new FnB643Activity.Export();

    useImport.Beginning.YearMonth = local.Beginning.YearMonth;
    useImport.Ending.YearMonth = local.Ending.YearMonth;
    useImport.Obligor.Type1 = entities.Obligor.Type1;
    useImport.StmtBeginTextWorkArea.Text10 = local.StmtBegin.Text10;
    MoveDateWorkArea(local.BeginStmt, useImport.StmtBeginDateWorkArea);
    useImport.StmtEndTextWorkArea.Text10 = local.StmtEnd.Text10;
    MoveDateWorkArea(local.EndStmt, useImport.StmtEndDateWorkArea);
    useImport.StmtNumber.Count = local.CurrStmtNumThisObligor.Count;
    useImport.VendorFileRecordCount.Count = local.VendorFileRecordCount.Count;
    useImport.SortSequenceNumber.Count = local.SortSequenceNumber.Count;
    useImport.ProcessDate.Date = local.Process.Date;
    useImport.ObligationType.Assign(entities.ObligationType);
    useImport.CsePerson.Assign(entities.Client);
    useImport.Obligation.Assign(entities.Obligation);
    useImport.SuppressCouponsByDebt.Flag = local.SuppressCpnsByDebt.Flag;
    useImport.CpnsSuppressedByDebt.Count = local.CpnsSuppressedByDebt.Count;

    Call(FnB643Activity.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    local.VendorFileRecordCount.Count = useExport.VendorFileRecordCount.Count;
    local.SortSequenceNumber.Count = useExport.SortSequenceNumber.Count;
    local.SuppressCpnsByDebt.Flag = useExport.SuppressCouponsByDebt.Flag;
    local.CpnsSuppressedByDebt.Count = useExport.CpnsSuppressedByDebt.Count;
  }

  private void UseFnB643ActivityOnVoluntary()
  {
    var useImport = new FnB643ActivityOnVoluntary.Import();
    var useExport = new FnB643ActivityOnVoluntary.Export();

    useImport.Obligation.Assign(entities.Obligation);
    MoveDateWorkArea(local.BeginStmt, useImport.StmtBegin);
    MoveDateWorkArea(local.EndStmt, useImport.StmtEnd);

    Call(FnB643ActivityOnVoluntary.Execute, useImport, useExport);

    local.ActivityOnVoluntary.Flag = useExport.ActivityFound.Flag;
  }

  private void UseFnB643Coupons1()
  {
    var useImport = new FnB643Coupons.Import();
    var useExport = new FnB643Coupons.Export();

    useImport.CouponsCreated.Count = local.CouponsCreated.Count;
    useImport.SortSequenceNumber.Count = local.SortSequenceNumber.Count;
    useImport.StmtEndDate.Text10 = local.StmtEnd.Text10;
    useImport.CpnReportingMonthYear.Text30 = local.CpnReportingMonthYear.Text30;
    useImport.LegalAction.Assign(entities.LegalAction);
    useImport.Obligation.Assign(entities.Obligation);
    useImport.Client.Assign(entities.Client);
    useImport.VendorFileRecordCount.Count = local.VendorFileRecordCount.Count;
    useImport.StmtNumber.Count = local.CurrStmtNumThisObligor.Count;
    useImport.CouponBegin.Date = local.BeginCoupon.Date;
    useImport.CouponEnd.Date = local.EndCoupon.Date;
    useImport.Ending.YearMonth = local.Ending.YearMonth;

    Call(FnB643Coupons.Execute, useImport, useExport);

    local.CouponsCreated.Count = useExport.CouponsCreated.Count;
    local.SortSequenceNumber.Count = useExport.SortSequenceNumber.Count;
    local.VendorFileRecordCount.Count = useExport.VendorFileRecordCount.Count;
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnB643Coupons2()
  {
    var useImport = new FnB643Coupons.Import();
    var useExport = new FnB643Coupons.Export();

    useImport.CouponsCreated.Count = local.CouponsCreated.Count;
    useImport.SortSequenceNumber.Count = local.SortSequenceNumber.Count;
    useImport.StmtEndDate.Text10 = local.StmtEnd.Text10;
    useImport.CpnReportingMonthYear.Text30 = local.CpnReportingMonthYear.Text30;
    useImport.Obligation.Assign(entities.Obligation);
    useImport.Client.Assign(entities.Client);
    useImport.VendorFileRecordCount.Count = local.VendorFileRecordCount.Count;
    useImport.StmtNumber.Count = local.CurrStmtNumThisObligor.Count;
    useImport.CouponBegin.Date = local.BeginCoupon.Date;
    useImport.CouponEnd.Date = local.EndCoupon.Date;
    useImport.Ending.YearMonth = local.Ending.YearMonth;

    Call(FnB643Coupons.Execute, useImport, useExport);

    local.CouponsCreated.Count = useExport.CouponsCreated.Count;
    local.SortSequenceNumber.Count = useExport.SortSequenceNumber.Count;
    local.VendorFileRecordCount.Count = useExport.VendorFileRecordCount.Count;
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnB643CourtOrder()
  {
    var useImport = new FnB643CourtOrder.Import();
    var useExport = new FnB643CourtOrder.Export();

    useImport.SortSequenceNumber.Count = local.SortSequenceNumber.Count;
    useImport.VendorFileRecordCount.Count = local.VendorFileRecordCount.Count;
    useImport.FipsTribAddress.Assign(entities.FipsTribAddress);
    MoveLegalAction(entities.LegalAction, useImport.LegalAction);
    useImport.County.Id = local.County.Id;
    MoveObligationType(entities.ObligationType, useImport.ObligationType);
    useImport.CsePerson.Assign(entities.Client);
    useImport.StmtNumber.Count = local.CurrStmtNumThisObligor.Count;

    Call(FnB643CourtOrder.Execute, useImport, useExport);

    local.SortSequenceNumber.Count = useExport.SortSequenceNumber.Count;
    local.VendorFileRecordCount.Count = useExport.VendorFileRecordCount.Count;
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnB643Housekeeping()
  {
    var useImport = new FnB643Housekeeping.Import();
    var useExport = new FnB643Housekeeping.Export();

    Call(FnB643Housekeeping.Execute, useImport, useExport);

    local.County.Id = useExport.County.Id;
    local.GlobalStatementMessage.TextArea =
      useExport.GlobalStatementMessage.TextArea;
    local.StmtBegin.Text10 = useExport.StatementBegin.Text10;
    local.StmtEnd.Text10 = useExport.StatementEnd.Text10;
    local.StmtReportingMonthYear.Text30 =
      useExport.StmtReportingMonthYear.Text30;
    local.CpnReportingMonthYear.Text30 = useExport.CpnReportingMonthYear.Text30;
    local.StatementDate.Text10 = useExport.StatementDate.Text10;
    local.Beginning.YearMonth = useExport.Beginning.YearMonth;
    local.Ending.YearMonth = useExport.Ending.YearMonth;
    local.VendorFileRecordCount.Count = useExport.VendorFileRecordCount.Count;
    local.SortSequenceNumber.Count = useExport.SortSequenceNumber.Count;
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    local.Process.Date = useExport.Process.Date;
    local.ProcessDate.Text10 = useExport.ProcessDate.Text10;
    MoveDateWorkArea(useExport.BeginStmt, local.BeginStmt);
    MoveDateWorkArea(useExport.EndStmt, local.EndStmt);
    local.BeginCoupon.Date = useExport.BeginCoupon.Date;
    local.EndCoupon.Date = useExport.EndCoupon.Date;
    local.FdirPmt.SystemGeneratedIdentifier =
      useExport.FdirPmt.SystemGeneratedIdentifier;
    local.FcrtRec.SystemGeneratedIdentifier =
      useExport.FcrtRec.SystemGeneratedIdentifier;
    local.Iwo.SequentialIdentifier = useExport.Iwo.SequentialIdentifier;
  }

  private void UseFnB643IwoCouponSuppression()
  {
    var useImport = new FnB643IwoCouponSuppression.Import();
    var useExport = new FnB643IwoCouponSuppression.Export();

    useImport.Client.Assign(entities.Client);
    useImport.Obligation.Assign(entities.Obligation);
    useImport.CpnsSuppressedByDebt.Count = local.CpnsSuppressedByDebt.Count;
    useImport.EndCoupon.Date = local.EndCoupon.Date;
    useImport.Iwo.SequentialIdentifier = local.Iwo.SequentialIdentifier;
    MoveDateWorkArea(local.BeginStmt, useImport.StmtBegin);
    MoveDateWorkArea(local.EndStmt, useImport.StmtEnd);

    Call(FnB643IwoCouponSuppression.Execute, useImport, useExport);

    local.SuppressCpnsByDebt.Flag = useExport.SuppressCouponsByDebt.Flag;
    local.CpnsSuppressedByDebt.Count = useExport.CpnsSuppressedByDebt.Count;
  }

  private void UseFnB643ObligationHeading1()
  {
    var useImport = new FnB643ObligationHeading.Import();
    var useExport = new FnB643ObligationHeading.Export();

    useImport.County.Id = local.County.Id;
    useImport.FipsTribAddress.Assign(entities.FipsTribAddress);
    useImport.Tribunal.Assign(entities.Tribunal);
    MoveObligationType(entities.ObligationType, useImport.ObligationType);
    useImport.NewCourtOrder.Flag = local.NewCourtOrder.Flag;
    MoveLegalAction(entities.LegalAction, useImport.LegalAction);
    useImport.CsePerson.Assign(entities.Client);
    useImport.StmtNumber.Count = local.CurrStmtNumThisObligor.Count;
    useImport.VendorFileRecordCount.Count = local.VendorFileRecordCount.Count;
    useImport.SortSequenceNumber.Count = local.SortSequenceNumber.Count;

    Call(FnB643ObligationHeading.Execute, useImport, useExport);

    local.SortSequenceNumber.Count = useExport.SortSequenceNumber.Count;
    local.VendorFileRecordCount.Count = useExport.VendorFileRecordCount.Count;
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnB643ObligationHeading2()
  {
    var useImport = new FnB643ObligationHeading.Import();
    var useExport = new FnB643ObligationHeading.Export();

    useImport.County.Id = local.County.Id;
    useImport.FipsTribAddress.Assign(entities.FipsTribAddress);
    useImport.Tribunal.Assign(entities.Tribunal);
    MoveObligationType(entities.ObligationType, useImport.ObligationType);
    useImport.NewCourtOrder.Flag = local.NewCourtOrder.Flag;
    useImport.CsePerson.Assign(entities.Client);
    useImport.StmtNumber.Count = local.CurrStmtNumThisObligor.Count;
    useImport.VendorFileRecordCount.Count = local.VendorFileRecordCount.Count;
    useImport.SortSequenceNumber.Count = local.SortSequenceNumber.Count;

    Call(FnB643ObligationHeading.Execute, useImport, useExport);

    local.SortSequenceNumber.Count = useExport.SortSequenceNumber.Count;
    local.VendorFileRecordCount.Count = useExport.VendorFileRecordCount.Count;
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnB643ObligationSuppression()
  {
    var useImport = new FnB643ObligationSuppression.Import();
    var useExport = new FnB643ObligationSuppression.Export();

    useImport.StmtSuppressedSecondary.Count =
      local.StmtSuppressedSecondary.Count;
    useImport.StmtSuppressedDormant.Count = local.StmtSuppressedDormant.Count;
    useImport.StmtSuppressedInterstat.Count =
      local.StmtSuppressedInterstate.Count;
    useImport.StmtSuppressedByDebt.Count = local.StmtSuppressedByDebt.Count;
    useImport.CpnsSuppressedByDebt.Count = local.CpnsSuppressedByDebt.Count;
    useImport.Process.Date = local.Process.Date;
    useImport.Client.Assign(entities.Client);
    useImport.Obligation.Assign(entities.Obligation);
    MoveDateWorkArea(local.EndStmt, useImport.StmtEndDate);
    useImport.FdirPmt.SystemGeneratedIdentifier =
      local.FdirPmt.SystemGeneratedIdentifier;
    useImport.FcrtRec.SystemGeneratedIdentifier =
      local.FcrtRec.SystemGeneratedIdentifier;
    MoveDateWorkArea(local.BeginStmt, useImport.StmtBeginDate);

    Call(FnB643ObligationSuppression.Execute, useImport, useExport);

    local.StmtSuppressedSecondary.Count =
      useExport.StmtSuppressedSecondary.Count;
    local.StmtSuppressedDormant.Count = useExport.StmtSuppressedDormant.Count;
    local.StmtSuppressedInterstate.Count =
      useExport.StmtSuppressedInterstat.Count;
    local.StmtSuppressedByDebt.Count = useExport.StmtSuppressedByDebt.Count;
    local.CpnsSuppressedByDebt.Count = useExport.CpnsSuppressedByDebt.Count;
    local.SuppressBothByDebt.Flag = useExport.SuppressStmtAndCpns.Flag;
    local.SuppressCpnsByDebt.Flag = useExport.SuppressCouponsByDebt.Flag;
  }

  private void UseFnB643ObligorSuppression()
  {
    var useImport = new FnB643ObligorSuppression.Import();
    var useExport = new FnB643ObligorSuppression.Export();

    useImport.Client.Assign(entities.Client);
    useImport.CpnsSuppressByObligor.Count = local.CpnsSuppressedByObligor.Count;
    useImport.StmtSuppressByObligor.Count = local.StmtSuppressedByObligor.Count;
    useImport.Process.Date = local.Process.Date;
    useImport.Obligor.Assign(entities.Obligor);
    useImport.FdirPmt.SystemGeneratedIdentifier =
      local.FdirPmt.SystemGeneratedIdentifier;
    useImport.FcrtRec.SystemGeneratedIdentifier =
      local.FcrtRec.SystemGeneratedIdentifier;

    Call(FnB643ObligorSuppression.Execute, useImport, useExport);

    local.SuppressCpnsByObligor.Flag = useExport.SuppressCpnsByObligor.Flag;
    local.SuppressBothByObligor.Flag = useExport.SuppressBothByObligor.Flag;
    local.CpnsSuppressedByObligor.Count = useExport.CpnsSuppressByObligor.Count;
    local.StmtSuppressedByObligor.Count = useExport.StmtSuppressByObligor.Count;
  }

  private void UseFnB643StatementHeading()
  {
    var useImport = new FnB643StatementHeading.Import();
    var useExport = new FnB643StatementHeading.Export();

    useImport.StmtsCreated.Count = local.StmtsCreated.Count;
    useImport.ReportingMonthYear.Text30 = local.StmtReportingMonthYear.Text30;
    useImport.StatementDate.Text10 = local.StatementDate.Text10;
    useImport.Client.Assign(entities.Client);
    useImport.Obligor1.Assign(entities.Obligor);
    useImport.StmtNumber.Count = local.CurrStmtNumThisObligor.Count;
    useImport.ProcessDate.Date = local.Process.Date;
    useImport.VendorFileRecordCount.Count = local.VendorFileRecordCount.Count;
    useImport.SortSequenceNumber.Count = local.SortSequenceNumber.Count;
    useImport.Obligor2.Number = local.Obligor.Number;
    useImport.Obligation.Assign(entities.Obligation);
    useImport.ObligationType.Assign(entities.ObligationType);
    useImport.GlobalStatementMessage.TextArea =
      local.GlobalStatementMessage.TextArea;

    Call(FnB643StatementHeading.Execute, useImport, useExport);

    local.SortSequenceNumber.Count = useExport.SortSequenceNumber.Count;
    local.AdabasReturn.Assign(useExport.Obligor);
    local.StmtsCreated.Count = useExport.StmtsCreated.Count;
    local.VendorFileRecordCount.Count = useExport.VendorFileRecordCount.Count;
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    local.AbendData.Assign(useExport.AbendData);
  }

  private void UseFnB643Undistributed()
  {
    var useImport = new FnB643Undistributed.Import();
    var useExport = new FnB643Undistributed.Export();

    useImport.VendorFileRecordCount.Count = local.VendorFileRecordCount.Count;
    useImport.SortSequenceNumber.Count = local.SortSequenceNumber.Count;
    useImport.CsePerson.Assign(entities.Client);
    MoveDateWorkArea(local.BeginStmt, useImport.StmtBegin);
    MoveDateWorkArea(local.EndStmt, useImport.StmtEndDateWorkArea);
    useImport.StmtEndTextWorkArea.Text10 = local.StmtEnd.Text10;
    useImport.StmtNumber.Count = local.CurrStmtNumThisObligor.Count;

    Call(FnB643Undistributed.Execute, useImport, useExport);

    local.VendorFileRecordCount.Count = useExport.VendorFileRecordCount.Count;
    local.SortSequenceNumber.Count = useExport.SortSequenceNumber.Count;
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnB643WriteControlsAndClose()
  {
    var useImport = new FnB643WriteControlsAndClose.Import();
    var useExport = new FnB643WriteControlsAndClose.Export();

    useImport.SortSequenceNumber.Count = local.SortSequenceNumber.Count;
    useImport.VendorFileRecordCount.Count = local.VendorFileRecordCount.Count;
    useImport.ObligorsRead.Count = local.ObligorsRead.Count;
    useImport.StmtsCreated.Count = local.StmtsCreated.Count;
    useImport.CpnsCreated.Count = local.CouponsCreated.Count;
    useImport.StmtsSuppressByObligor.Count =
      local.StmtSuppressedByObligor.Count;
    useImport.CpnsSuppressByObligor.Count = local.CpnsSuppressedByObligor.Count;
    useImport.StmtsSuppressByDebt.Count = local.StmtSuppressedByDebt.Count;
    useImport.CpnsSuppressByDebt.Count = local.CpnsSuppressedByDebt.Count;

    Call(FnB643WriteControlsAndClose.Execute, useImport, useExport);

    local.SortSequenceNumber.Count = useExport.SortSequenceNumber.Count;
    local.VendorFileRecordCount.Count = useExport.VendorFileRecordCount.Count;
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private bool ReadLegalAction()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          entities.Obligation.LgaId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 3);
        entities.LegalAction.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalActionObligation()
  {
    System.Diagnostics.Debug.Assert(entities.Obligor.Populated);
    entities.LegalAction.Populated = false;
    entities.Obligation.Populated = false;

    return ReadEach("ReadLegalActionObligation",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.Obligor.Type1);
        db.SetString(command, "cspNumber", entities.Obligor.CspNumber);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 0);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 3);
        entities.Obligation.CpaType = db.GetString(reader, 4);
        entities.Obligation.CspNumber = db.GetString(reader, 5);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 6);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 7);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 8);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 9);
        entities.Obligation.DormantInd = db.GetNullableString(reader, 10);
        entities.LegalAction.Populated = true;
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.Obligation.OrderTypeCode);

        return true;
      });
  }

  private IEnumerable<bool> ReadObligation()
  {
    System.Diagnostics.Debug.Assert(entities.Obligor.Populated);
    entities.Obligation.Populated = false;

    return ReadEach("ReadObligation",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.Obligor.Type1);
        db.SetString(command, "cspNumber", entities.Obligor.CspNumber);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 5);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 6);
        entities.Obligation.DormantInd = db.GetNullableString(reader, 7);
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.Obligation.OrderTypeCode);

        return true;
      });
  }

  private bool ReadObligationType()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetInt32(command, "debtTypId", entities.Obligation.DtyGeneratedId);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Name = db.GetString(reader, 2);
        entities.ObligationType.Populated = true;
      });
  }

  private IEnumerable<bool> ReadObligorClient()
  {
    entities.Client.Populated = false;
    entities.Obligor.Populated = false;

    return ReadEach("ReadObligorClient",
      null,
      (db, reader) =>
      {
        entities.Obligor.CspNumber = db.GetString(reader, 0);
        entities.Client.Number = db.GetString(reader, 0);
        entities.Obligor.Type1 = db.GetString(reader, 1);
        entities.Client.DateOfDeath = db.GetNullableDate(reader, 2);
        entities.Client.Populated = true;
        entities.Obligor.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.Obligor.Type1);

        return true;
      });
  }

  private bool ReadTribunalFipsTribAddress()
  {
    System.Diagnostics.Debug.Assert(entities.LegalAction.Populated);
    entities.FipsTribAddress.Populated = false;
    entities.Tribunal.Populated = false;

    return Read("ReadTribunalFipsTribAddress",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.LegalAction.TrbId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Tribunal.Name = db.GetString(reader, 0);
        entities.Tribunal.Identifier = db.GetInt32(reader, 1);
        entities.FipsTribAddress.TrbId = db.GetNullableInt32(reader, 1);
        entities.FipsTribAddress.Identifier = db.GetInt32(reader, 2);
        entities.FipsTribAddress.City = db.GetString(reader, 3);
        entities.FipsTribAddress.State = db.GetString(reader, 4);
        entities.FipsTribAddress.County = db.GetNullableString(reader, 5);
        entities.FipsTribAddress.Populated = true;
        entities.Tribunal.Populated = true;
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
  public class Local
  {
    /// <summary>
    /// A value of FdirPmt.
    /// </summary>
    [JsonPropertyName("fdirPmt")]
    public CashReceiptType FdirPmt
    {
      get => fdirPmt ??= new();
      set => fdirPmt = value;
    }

    /// <summary>
    /// A value of FcrtRec.
    /// </summary>
    [JsonPropertyName("fcrtRec")]
    public CashReceiptType FcrtRec
    {
      get => fcrtRec ??= new();
      set => fcrtRec = value;
    }

    /// <summary>
    /// A value of Iwo.
    /// </summary>
    [JsonPropertyName("iwo")]
    public CollectionType Iwo
    {
      get => iwo ??= new();
      set => iwo = value;
    }

    /// <summary>
    /// A value of ActivityOnVoluntary.
    /// </summary>
    [JsonPropertyName("activityOnVoluntary")]
    public Common ActivityOnVoluntary
    {
      get => activityOnVoluntary ??= new();
      set => activityOnVoluntary = value;
    }

    /// <summary>
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
    }

    /// <summary>
    /// A value of SortSequenceNumber.
    /// </summary>
    [JsonPropertyName("sortSequenceNumber")]
    public Common SortSequenceNumber
    {
      get => sortSequenceNumber ??= new();
      set => sortSequenceNumber = value;
    }

    /// <summary>
    /// A value of CpnReportingMonthYear.
    /// </summary>
    [JsonPropertyName("cpnReportingMonthYear")]
    public TextWorkArea CpnReportingMonthYear
    {
      get => cpnReportingMonthYear ??= new();
      set => cpnReportingMonthYear = value;
    }

    /// <summary>
    /// A value of County.
    /// </summary>
    [JsonPropertyName("county")]
    public Code County
    {
      get => county ??= new();
      set => county = value;
    }

    /// <summary>
    /// A value of ProcessDate.
    /// </summary>
    [JsonPropertyName("processDate")]
    public TextWorkArea ProcessDate
    {
      get => processDate ??= new();
      set => processDate = value;
    }

    /// <summary>
    /// A value of GlobalStatementMessage.
    /// </summary>
    [JsonPropertyName("globalStatementMessage")]
    public GlobalStatementMessage GlobalStatementMessage
    {
      get => globalStatementMessage ??= new();
      set => globalStatementMessage = value;
    }

    /// <summary>
    /// A value of StmtEnd.
    /// </summary>
    [JsonPropertyName("stmtEnd")]
    public TextWorkArea StmtEnd
    {
      get => stmtEnd ??= new();
      set => stmtEnd = value;
    }

    /// <summary>
    /// A value of StmtBegin.
    /// </summary>
    [JsonPropertyName("stmtBegin")]
    public TextWorkArea StmtBegin
    {
      get => stmtBegin ??= new();
      set => stmtBegin = value;
    }

    /// <summary>
    /// A value of StatementDate.
    /// </summary>
    [JsonPropertyName("statementDate")]
    public TextWorkArea StatementDate
    {
      get => statementDate ??= new();
      set => statementDate = value;
    }

    /// <summary>
    /// A value of StmtReportingMonthYear.
    /// </summary>
    [JsonPropertyName("stmtReportingMonthYear")]
    public TextWorkArea StmtReportingMonthYear
    {
      get => stmtReportingMonthYear ??= new();
      set => stmtReportingMonthYear = value;
    }

    /// <summary>
    /// A value of Ending.
    /// </summary>
    [JsonPropertyName("ending")]
    public MonthlyObligorSummary Ending
    {
      get => ending ??= new();
      set => ending = value;
    }

    /// <summary>
    /// A value of Beginning.
    /// </summary>
    [JsonPropertyName("beginning")]
    public MonthlyObligorSummary Beginning
    {
      get => beginning ??= new();
      set => beginning = value;
    }

    /// <summary>
    /// A value of NewCourtOrder.
    /// </summary>
    [JsonPropertyName("newCourtOrder")]
    public Common NewCourtOrder
    {
      get => newCourtOrder ??= new();
      set => newCourtOrder = value;
    }

    /// <summary>
    /// A value of CurrStmtNumThisObligor.
    /// </summary>
    [JsonPropertyName("currStmtNumThisObligor")]
    public Common CurrStmtNumThisObligor
    {
      get => currStmtNumThisObligor ??= new();
      set => currStmtNumThisObligor = value;
    }

    /// <summary>
    /// A value of PrevStmtNumThisObligor.
    /// </summary>
    [JsonPropertyName("prevStmtNumThisObligor")]
    public Common PrevStmtNumThisObligor
    {
      get => prevStmtNumThisObligor ??= new();
      set => prevStmtNumThisObligor = value;
    }

    /// <summary>
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public LegalAction Prev
    {
      get => prev ??= new();
      set => prev = value;
    }

    /// <summary>
    /// A value of SuppressCpnsByObligor.
    /// </summary>
    [JsonPropertyName("suppressCpnsByObligor")]
    public Common SuppressCpnsByObligor
    {
      get => suppressCpnsByObligor ??= new();
      set => suppressCpnsByObligor = value;
    }

    /// <summary>
    /// A value of SuppressCpnsByDebt.
    /// </summary>
    [JsonPropertyName("suppressCpnsByDebt")]
    public Common SuppressCpnsByDebt
    {
      get => suppressCpnsByDebt ??= new();
      set => suppressCpnsByDebt = value;
    }

    /// <summary>
    /// A value of SuppressBothByObligor.
    /// </summary>
    [JsonPropertyName("suppressBothByObligor")]
    public Common SuppressBothByObligor
    {
      get => suppressBothByObligor ??= new();
      set => suppressBothByObligor = value;
    }

    /// <summary>
    /// A value of SuppressBothByDebt.
    /// </summary>
    [JsonPropertyName("suppressBothByDebt")]
    public Common SuppressBothByDebt
    {
      get => suppressBothByDebt ??= new();
      set => suppressBothByDebt = value;
    }

    /// <summary>
    /// A value of StmtSuppressedInterstate.
    /// </summary>
    [JsonPropertyName("stmtSuppressedInterstate")]
    public Common StmtSuppressedInterstate
    {
      get => stmtSuppressedInterstate ??= new();
      set => stmtSuppressedInterstate = value;
    }

    /// <summary>
    /// A value of StmtSuppressedDormant.
    /// </summary>
    [JsonPropertyName("stmtSuppressedDormant")]
    public Common StmtSuppressedDormant
    {
      get => stmtSuppressedDormant ??= new();
      set => stmtSuppressedDormant = value;
    }

    /// <summary>
    /// A value of StmtSuppressedSecondary.
    /// </summary>
    [JsonPropertyName("stmtSuppressedSecondary")]
    public Common StmtSuppressedSecondary
    {
      get => stmtSuppressedSecondary ??= new();
      set => stmtSuppressedSecondary = value;
    }

    /// <summary>
    /// A value of AdabasReturn.
    /// </summary>
    [JsonPropertyName("adabasReturn")]
    public CsePersonsWorkSet AdabasReturn
    {
      get => adabasReturn ??= new();
      set => adabasReturn = value;
    }

    /// <summary>
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    /// <summary>
    /// A value of VendorFileRecordCount.
    /// </summary>
    [JsonPropertyName("vendorFileRecordCount")]
    public Common VendorFileRecordCount
    {
      get => vendorFileRecordCount ??= new();
      set => vendorFileRecordCount = value;
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
    /// A value of ObligorsRead.
    /// </summary>
    [JsonPropertyName("obligorsRead")]
    public Common ObligorsRead
    {
      get => obligorsRead ??= new();
      set => obligorsRead = value;
    }

    /// <summary>
    /// A value of StmtsCreated.
    /// </summary>
    [JsonPropertyName("stmtsCreated")]
    public Common StmtsCreated
    {
      get => stmtsCreated ??= new();
      set => stmtsCreated = value;
    }

    /// <summary>
    /// A value of CouponsCreated.
    /// </summary>
    [JsonPropertyName("couponsCreated")]
    public Common CouponsCreated
    {
      get => couponsCreated ??= new();
      set => couponsCreated = value;
    }

    /// <summary>
    /// A value of StmtSuppressedByObligor.
    /// </summary>
    [JsonPropertyName("stmtSuppressedByObligor")]
    public Common StmtSuppressedByObligor
    {
      get => stmtSuppressedByObligor ??= new();
      set => stmtSuppressedByObligor = value;
    }

    /// <summary>
    /// A value of CpnsSuppressedByObligor.
    /// </summary>
    [JsonPropertyName("cpnsSuppressedByObligor")]
    public Common CpnsSuppressedByObligor
    {
      get => cpnsSuppressedByObligor ??= new();
      set => cpnsSuppressedByObligor = value;
    }

    /// <summary>
    /// A value of StmtSuppressedByDebt.
    /// </summary>
    [JsonPropertyName("stmtSuppressedByDebt")]
    public Common StmtSuppressedByDebt
    {
      get => stmtSuppressedByDebt ??= new();
      set => stmtSuppressedByDebt = value;
    }

    /// <summary>
    /// A value of CpnsSuppressedByDebt.
    /// </summary>
    [JsonPropertyName("cpnsSuppressedByDebt")]
    public Common CpnsSuppressedByDebt
    {
      get => cpnsSuppressedByDebt ??= new();
      set => cpnsSuppressedByDebt = value;
    }

    /// <summary>
    /// A value of Process.
    /// </summary>
    [JsonPropertyName("process")]
    public DateWorkArea Process
    {
      get => process ??= new();
      set => process = value;
    }

    /// <summary>
    /// A value of BeginStmt.
    /// </summary>
    [JsonPropertyName("beginStmt")]
    public DateWorkArea BeginStmt
    {
      get => beginStmt ??= new();
      set => beginStmt = value;
    }

    /// <summary>
    /// A value of EndStmt.
    /// </summary>
    [JsonPropertyName("endStmt")]
    public DateWorkArea EndStmt
    {
      get => endStmt ??= new();
      set => endStmt = value;
    }

    /// <summary>
    /// A value of BeginCoupon.
    /// </summary>
    [JsonPropertyName("beginCoupon")]
    public DateWorkArea BeginCoupon
    {
      get => beginCoupon ??= new();
      set => beginCoupon = value;
    }

    /// <summary>
    /// A value of EndCoupon.
    /// </summary>
    [JsonPropertyName("endCoupon")]
    public DateWorkArea EndCoupon
    {
      get => endCoupon ??= new();
      set => endCoupon = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    private CashReceiptType fdirPmt;
    private CashReceiptType fcrtRec;
    private CollectionType iwo;
    private Common activityOnVoluntary;
    private ExitStateWorkArea exitStateWorkArea;
    private Common sortSequenceNumber;
    private TextWorkArea cpnReportingMonthYear;
    private Code county;
    private TextWorkArea processDate;
    private GlobalStatementMessage globalStatementMessage;
    private TextWorkArea stmtEnd;
    private TextWorkArea stmtBegin;
    private TextWorkArea statementDate;
    private TextWorkArea stmtReportingMonthYear;
    private MonthlyObligorSummary ending;
    private MonthlyObligorSummary beginning;
    private Common newCourtOrder;
    private Common currStmtNumThisObligor;
    private Common prevStmtNumThisObligor;
    private LegalAction prev;
    private Common suppressCpnsByObligor;
    private Common suppressCpnsByDebt;
    private Common suppressBothByObligor;
    private Common suppressBothByDebt;
    private Common stmtSuppressedInterstate;
    private Common stmtSuppressedDormant;
    private Common stmtSuppressedSecondary;
    private CsePersonsWorkSet adabasReturn;
    private AbendData abendData;
    private Common vendorFileRecordCount;
    private CsePersonsWorkSet obligor;
    private Common obligorsRead;
    private Common stmtsCreated;
    private Common couponsCreated;
    private Common stmtSuppressedByObligor;
    private Common cpnsSuppressedByObligor;
    private Common stmtSuppressedByDebt;
    private Common cpnsSuppressedByDebt;
    private DateWorkArea process;
    private DateWorkArea beginStmt;
    private DateWorkArea endStmt;
    private DateWorkArea beginCoupon;
    private DateWorkArea endCoupon;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
    }

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of Client.
    /// </summary>
    [JsonPropertyName("client")]
    public CsePerson Client
    {
      get => client ??= new();
      set => client = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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

    private FipsTribAddress fipsTribAddress;
    private Tribunal tribunal;
    private CsePerson client;
    private CsePersonAccount obligor;
    private LegalAction legalAction;
    private ObligationType obligationType;
    private Obligation obligation;
  }
#endregion
}
