// Program: FN_B650_APPLY_TO_POT_RCV, ID: 371188844, model: 746.
// Short name: SWE02289
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B650_APPLY_TO_POT_RCV.
/// </summary>
[Serializable]
public partial class FnB650ApplyToPotRcv: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B650_APPLY_TO_POT_RCV program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB650ApplyToPotRcv(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB650ApplyToPotRcv.
  /// </summary>
  public FnB650ApplyToPotRcv(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------
    // Date	  By	  IDCR#	 Description
    // 06/19/03  Fangman  302055   Initial code to apply a disb coll to a 
    // potential recovery obligation.
    // ---------------------------------------------
    local.EabFileHandling.Action = "WRITE";
    local.Max.Date = UseCabSetMaximumDiscontinueDate();
    export.AppliedApCollToRcvInd.Flag = "N";

    if (AsChar(import.DisplayInd.Flag) == 'Y')
    {
      local.EabReportSend.RptDetail =
        "  !!!! 1 In 650 Apply to POT RCV  for AR " + import.Ar.Number + "  !!!!*";
        
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
        export.Abort.Flag = "Y";

        return;
      }
    }

    if (Equal(import.Per.ProgramAppliedTo, "NA") || Equal
      (import.Per.ProgramAppliedTo, "NF"))
    {
      // Continue
    }
    else
    {
      if (AsChar(import.DisplayInd.Flag) == 'Y')
      {
        local.EabReportSend.RptDetail = "  Program_Applied_To of " + import
          .Per.ProgramAppliedTo + " is not NA or NF so this collection will not be checked for a RCV to apply to.";
          
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
          export.Abort.Flag = "Y";

          return;
        }
      }

      return;
    }

    if (AsChar(import.Per.AdjustedInd) == 'Y')
    {
      local.EabReportSend.RptDetail =
        "  Only positive amounts can be applied to a potential recovery obligation.";
        
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
        export.Abort.Flag = "Y";

        return;
      }

      return;
    }

    if (Equal(import.Ar.Number, import.TheStatesPersonNumber.Number) || Equal
      (import.Ar.Number, import.TheStatesPersonNumber2.Number) || Equal
      (import.Ar.Number, import.TheStatesPersonNumber3.Number))
    {
      local.EabReportSend.RptDetail = "  The state was found to be the AR.";
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
        export.Abort.Flag = "Y";

        return;
      }

      ExitState = "FN0000_STATE_CANT_BE_AR_ON_NA";

      return;
    }
    else if (Equal(import.Ar.Number, import.JuvenileJusticeAuthority.Number))
    {
      local.EabReportSend.RptDetail = "  The JJA was found to be the AR.";
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
        export.Abort.Flag = "Y";

        return;
      }

      ExitState = "FN0000_JJA_CANT_BE_AN_AR";

      return;
    }

    UseFnAbConcatCrAndCrd();
    local.RefNbr.ReferenceNumber = local.CrdCrComboNo.CrdCrCombo;
    local.ForCreateDisbCollection.ReferenceNumber =
      local.RefNbr.ReferenceNumber ?? "";

    if (AsChar(import.DisplayInd.Flag) == 'Y')
    {
      local.EabReportSend.RptDetail =
        "  !!!! 1 Collection Reference number is " + (
          local.ForCreateDisbCollection.ReferenceNumber ?? "") + "  Coll Amt: " +
        NumberToString((long)(import.Per.Amount * 100), 7, 9);
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
        export.Abort.Flag = "Y";

        return;
      }
    }

    if (ReadObligee())
    {
      // Continue
    }
    else
    {
      ExitState = "FN0000_OBLIGEE_CSE_PERSON_NF";

      return;
    }

    foreach(var item in ReadPaymentRequestPaymentStatusHistory2())
    {
      local.TotAmtForArRefNbrPmtR.Amount = 0;

      foreach(var item1 in ReadDisbursement2())
      {
        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          if (entities.NetDisbForArRefNbr.Amount < 0)
          {
            local.Sign.Text1 = "-";
          }
          else
          {
            local.Sign.Text1 = "+";
          }

          local.EabReportSend.RptDetail = "    !!!! 2 Disb ID " + NumberToString
            (entities.NetDisbForArRefNbr.SystemGeneratedIdentifier, 7, 9) + " RCV ID " +
            NumberToString
            (entities.PaymentRequest.SystemGeneratedIdentifier, 7, 9) + " Ref # " +
            entities.NetDisbForArRefNbr.ReferenceNumber + "  Disb amt:  " + local
            .Sign.Text1 + NumberToString
            ((long)(entities.NetDisbForArRefNbr.Amount * 100), 7, 9);
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
            export.Abort.Flag = "Y";

            return;
          }
        }

        local.TotAmtForArRefNbrPmtR.Amount += entities.NetDisbForArRefNbr.
          Amount;
      }

      if (AsChar(import.DisplayInd.Flag) == 'Y')
      {
        if (local.TotAmtForArRefNbrPmtR.Amount < 0)
        {
          local.Sign.Text1 = "-";
        }
        else
        {
          local.Sign.Text1 = "+";
        }

        local.EabReportSend.RptDetail = "  !!!! 3 POT RCV id: " + NumberToString
          (entities.PaymentRequest.SystemGeneratedIdentifier, 7, 9) + " RCV amt " +
          NumberToString((long)(entities.PaymentRequest.Amount * 100), 7, 9) + "  Total amt for Ref # & pmt req: " +
          local.Sign.Text1 + NumberToString
          ((long)(local.TotAmtForArRefNbrPmtR.Amount * 100), 7, 9);
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
          export.Abort.Flag = "Y";

          return;
        }
      }

      if (local.TotAmtForArRefNbrPmtR.Amount <= 0)
      {
        local.TotAmtForArRefNbr.Amount -= local.TotAmtForArRefNbrPmtR.Amount;

        if (local.TotAmtForArRefNbr.Amount < 0)
        {
          local.Sign.Text1 = "-";
        }
        else
        {
          local.Sign.Text1 = "+";
        }

        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          local.EabReportSend.RptDetail = "  !!!! 3 POT RCV id: " + NumberToString
            (entities.PaymentRequest.SystemGeneratedIdentifier, 7, 9) + " RCV amt " +
            NumberToString
            ((long)(entities.PaymentRequest.Amount * 100), 7, 9) + "  Total amt for Ref # & all pmt req: " +
            local.Sign.Text1 + NumberToString
            ((long)(local.TotAmtForArRefNbr.Amount * 100), 7, 9);
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
            export.Abort.Flag = "Y";

            return;
          }
        }
      }
    }

    if (local.TotAmtForArRefNbr.Amount > 0)
    {
      // Continue
    }
    else
    {
      if (AsChar(import.DisplayInd.Flag) == 'Y')
      {
        local.EabReportSend.RptDetail =
          "  4 No RCV found with disb to apply Coll to for AR " + import
          .Ar.Number + "*";
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
          export.Abort.Flag = "Y";

          return;
        }
      }

      return;
    }

    UseFnB651DetIfCrFeeNeeded();

    if (AsChar(local.CrFeeNeededInd.Flag) == 'Y')
    {
      UseFnB650DetCrFeePct();

      if (AsChar(import.DisplayInd.Flag) == 'Y')
      {
        local.EabReportSend.RptDetail = "  !!!! 5 CR Fee " + NumberToString
          ((long)(local.TribunalFeeInformation.Rate.GetValueOrDefault() * 100),
          7, 9);
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
          export.Abort.Flag = "Y";

          return;
        }
      }

      if (!ReadDisbursementType2())
      {
        ExitState = "FN0000_DISB_TYPE_NF";

        return;
      }
    }

    // Use CR Fee pct to determine if there is enough RCV amt to apply all of 
    // the collection.  (RCV amt >= Coll amt - (coll amt * CR Fee pct))
    if (local.TotAmtForArRefNbr.Amount < import.Per.Amount - import
      .Per.Amount * local.TribunalFeeInformation.Rate.GetValueOrDefault() / 100
      )
    {
      if (AsChar(import.DisplayInd.Flag) == 'Y')
      {
        local.EabReportSend.RptDetail = "  Coll ID " + NumberToString
          (import.Per.SystemGeneratedIdentifier, 7, 9) + "  Ref " + local
          .CrdCrComboNo.CrdCrCombo + "  Does not have enough RCV to apply to.";
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
          export.Abort.Flag = "Y";

          return;
        }
      }

      return;
    }

    local.EabReportSend.RptDetail = "  ** POT RCV: AP: " + import.Ap.Number + " AR: " +
      import.Ar.Number + " Coll: " + NumberToString
      (import.Per.SystemGeneratedIdentifier, 7, 9) + " " + (
        local.RefNbr.ReferenceNumber ?? "") + " " + NumberToString
      ((long)(import.Per.Amount * 100), 8, 8);
    UseCabErrorReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
      export.Abort.Flag = "Y";

      return;
    }

    ++import.ExpCollsEligForRcvCnt.Count;

    if (AsChar(import.ApplyApCollToRcv.Flag) == 'Y')
    {
      // Continue
    }
    else
    {
      local.EabReportSend.RptDetail =
        "  ** Maximum # of coll already applied to RCV - this coll will error off.";
        
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
        export.Abort.Flag = "Y";

        return;
      }

      return;
    }

    UseFnDetermineDisbType();

    if (ReadDisbursementType1())
    {
      if (AsChar(import.DisplayInd.Flag) == 'Y')
      {
        local.EabReportSend.RptDetail = "  !!!! 6 Disb Type " + entities
          .DisbursementType.Code;
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
          export.Abort.Flag = "Y";

          return;
        }
      }
    }
    else
    {
      ExitState = "FN0000_DISB_TYPE_NF";

      return;
    }

    if (!ReadDisbursementTranRlnRsn())
    {
      ExitState = "FN0000_DISB_TRANS_RLN_RSN_NF_RB";

      return;
    }

    if (!ReadDisbursementStatus())
    {
      ExitState = "FN0000_DISB_STAT_NF_RB";

      return;
    }

    if (AsChar(import.Per.AppliedToOrderTypeCode) == 'I')
    {
      if (ReadObligation())
      {
        if (ReadInterstateRequest())
        {
          local.ForCreateDisbursementTransaction.InterstateInd = "Y";

          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            local.EabReportSend.RptDetail =
              "  !!!! 7 Found Interstate Req w/ ID of " + entities
              .DisbursementType.Code;
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
              export.Abort.Flag = "Y";

              return;
            }
          }
        }
        else
        {
          ExitState = "FN0000_CURRENT_INTER_ST_RQST_NF";

          return;
        }
      }
      else
      {
        ExitState = "FN0000_OBLIGATION_NF";

        return;
      }
    }
    else
    {
      local.ForCreateDisbursementTransaction.InterstateInd = "N";
    }

    if (Equal(import.Per.DistPgmStateAppldTo, "UD") || Equal
      (import.Per.DistPgmStateAppldTo, "UP"))
    {
      local.ForCreateDisbursementTransaction.ExcessUraInd = "Y";
    }
    else
    {
      local.ForCreateDisbursementTransaction.ExcessUraInd = "";
    }

    local.CollAmtRemaining.Amount = import.Per.Amount;

    foreach(var item in ReadPaymentRequestPaymentStatusHistory1())
    {
      ReadDisbursement1();

      if (local.TotAmtForArRefNbrPmtR.Amount >= 0)
      {
        continue;
      }

      local.TotAmtForArRefNbrPmtR.Amount = -local.TotAmtForArRefNbrPmtR.Amount;
      local.CrAmt.Amount = local.CollAmtRemaining.Amount;
      local.CrFeeAmt.Amount = local.CrAmt.Amount * local
        .TribunalFeeInformation.Rate.GetValueOrDefault() / 100;
      local.DbAmt.Amount = local.CrAmt.Amount - local.CrFeeAmt.Amount;

      if (local.DbAmt.Amount > local.TotAmtForArRefNbrPmtR.Amount)
      {
        local.DbAmt.Amount = local.TotAmtForArRefNbrPmtR.Amount;
      }

      if (local.DbAmt.Amount > entities.PaymentRequest.Amount)
      {
        local.DbAmt.Amount = entities.PaymentRequest.Amount;
      }

      if (local.CrAmt.Amount != local.DbAmt.Amount + local.CrFeeAmt.Amount)
      {
        local.CrAmt.Amount = local.DbAmt.Amount / (1 - local
          .TribunalFeeInformation.Rate.GetValueOrDefault() / 100);
        local.CrFeeAmt.Amount = local.CrAmt.Amount - local.DbAmt.Amount;
      }

      local.CollAmtRemaining.Amount -= local.CrAmt.Amount;
      local.ForCreateDisbursementTransaction.SystemGeneratedIdentifier =
        UseFnGetDisbTranId();

      if (AsChar(import.DisplayInd.Flag) == 'Y')
      {
        local.EabReportSend.RptDetail =
          "  !! 9 about to create the disb cr " + NumberToString
          ((long)(local.CrAmt.Amount * 100), 7, 9);
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
          export.Abort.Flag = "Y";

          return;
        }
      }

      try
      {
        CreateDisbursementTransaction1();

        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          local.EabReportSend.RptDetail = "  !! 10 created the disb cr";
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
            export.Abort.Flag = "Y";

            return;
          }
        }

        if (AsChar(local.ForCreateDisbursementTransaction.InterstateInd) == 'Y')
        {
          AssociateDisbursementTransaction();
        }
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_DISB_TRANS_AE";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_DISB_TRANS_PV";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      if (AsChar(local.CrFeeNeededInd.Flag) == 'Y')
      {
        // Create CR Fee
        local.ForCreateDisbursementTransaction.SystemGeneratedIdentifier =
          UseFnGetDisbTranId();

        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          local.EabReportSend.RptDetail =
            "  !! 11 about to create the disb cr fee db " + NumberToString
            ((long)(local.CrFeeAmt.Amount * 100), 7, 9);
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
            export.Abort.Flag = "Y";

            return;
          }
        }

        try
        {
          CreateDisbursementTransaction3();

          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            local.EabReportSend.RptDetail =
              "  !! 12 created the disb cr fee db";
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
              export.Abort.Flag = "Y";

              return;
            }
          }

          if (AsChar(local.ForCreateDisbursementTransaction.InterstateInd) == 'Y'
            )
          {
            AssociateDisbursementTransaction();
          }

          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            local.EabReportSend.RptDetail =
              "  !! 13 about to create the disb stat hist for the cr fee db";
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
              export.Abort.Flag = "Y";

              return;
            }
          }

          try
          {
            CreateDisbursementStatusHistory();

            if (AsChar(import.DisplayInd.Flag) == 'Y')
            {
              local.EabReportSend.RptDetail =
                "  !! 14 created the disb stat hist for the cr fee db";
              UseCabErrorReport();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
                export.Abort.Flag = "Y";

                return;
              }
            }
          }
          catch(Exception e1)
          {
            switch(GetErrorCode(e1))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_DISB_STATUS_HISTORY_AE";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_DISB_STAT_HIST_PV";

                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            local.EabReportSend.RptDetail =
              "  !! 15 about to update recovery to tie the disb tran rln to the cr & db & rln rsn";
              
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
              export.Abort.Flag = "Y";

              return;
            }
          }

          try
          {
            CreateDisbursementTransactionRln();

            if (AsChar(import.DisplayInd.Flag) == 'Y')
            {
              local.EabReportSend.RptDetail =
                "  !! 16 finished updating recovery to tie the disb tran rln to the cr & db & rln rsn";
                
              UseCabErrorReport();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
                export.Abort.Flag = "Y";

                return;
              }
            }
          }
          catch(Exception e1)
          {
            switch(GetErrorCode(e1))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_DISB_TRANS_RLN_AE";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_DISB_TRAND_RLN_PV";

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
              ExitState = "FN0000_DISB_TRANSACTION_AE";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_DISB_TRANS_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      // Create DB
      local.ForCreateDisbursementTransaction.SystemGeneratedIdentifier =
        UseFnGetDisbTranId();

      if (AsChar(import.DisplayInd.Flag) == 'Y')
      {
        local.EabReportSend.RptDetail =
          "  !! 17 about to create the disb db " + NumberToString
          ((long)(local.DbAmt.Amount * 100), 7, 9);
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
          export.Abort.Flag = "Y";

          return;
        }
      }

      try
      {
        CreateDisbursementTransaction2();

        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          local.EabReportSend.RptDetail =
            "  !! 18 created the disb db w/ ref # " + entities
            .Db.ReferenceNumber;
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
            export.Abort.Flag = "Y";

            return;
          }
        }

        if (AsChar(local.ForCreateDisbursementTransaction.InterstateInd) == 'Y')
        {
          AssociateDisbursementTransaction();
        }

        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          local.EabReportSend.RptDetail =
            "  !! 19 about to create the disb stat hist for the db";
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
            export.Abort.Flag = "Y";

            return;
          }
        }

        try
        {
          CreateDisbursementStatusHistory();

          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            local.EabReportSend.RptDetail =
              "  !! 20 created the disb stat hist for the db";
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
              export.Abort.Flag = "Y";

              return;
            }
          }
        }
        catch(Exception e1)
        {
          switch(GetErrorCode(e1))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_DISB_STATUS_HISTORY_AE";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_DISB_STAT_HIST_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          local.EabReportSend.RptDetail =
            "  !! 21 about to update recovery to tie the disb tran rln to the cr & db & rln rsn";
            
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
            export.Abort.Flag = "Y";

            return;
          }
        }

        try
        {
          CreateDisbursementTransactionRln();

          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            local.EabReportSend.RptDetail =
              "  !! 22 finished updating recovery to tie the disb tran rln to the cr & db & rln rsn";
              
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
              export.Abort.Flag = "Y";

              return;
            }
          }
        }
        catch(Exception e1)
        {
          switch(GetErrorCode(e1))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_DISB_TRANS_RLN_AE";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_DISB_TRAND_RLN_PV";

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
            ExitState = "FN0000_DISB_TRANSACTION_AE";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_DISB_TRANS_PV";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      // Reduce the RCV amt by the coll amt.
      if (AsChar(import.DisplayInd.Flag) == 'Y')
      {
        local.EabReportSend.RptDetail =
          "  !! 23 about to update recovery from" + " " + " " + NumberToString
          ((long)(entities.PaymentRequest.Amount * 100), 7, 9);
        local.EabReportSend.RptDetail =
          "  !! 23 about to update recovery to reduce the outstanding balance & tie it to the new disb.";
          
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
          export.Abort.Flag = "Y";

          return;
        }
      }

      try
      {
        UpdatePaymentRequest();

        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          local.EabReportSend.RptDetail =
            "  !! 24 finished updating RCV to " + " " + " " + NumberToString
            ((long)(entities.PaymentRequest.Amount * 100), 7, 9);
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
            export.Abort.Flag = "Y";

            return;
          }
        }

        if (entities.PaymentRequest.Amount == 0)
        {
          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            local.EabReportSend.RptDetail =
              "  !! 25 RCV is paid off, update pmt stat hist";
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
              export.Abort.Flag = "Y";

              return;
            }
          }

          try
          {
            UpdatePaymentStatusHistory();

            if (AsChar(import.DisplayInd.Flag) == 'Y')
            {
              local.EabReportSend.RptDetail =
                "  !! 26 Pmt stat hist is updated.";
              UseCabErrorReport();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
                export.Abort.Flag = "Y";

                return;
              }
            }

            local.ForUpdatePaymentStatusHistory.SystemGeneratedIdentifier =
              entities.PaymentStatusHistory.SystemGeneratedIdentifier + 1;

            if (AsChar(import.DisplayInd.Flag) == 'Y')
            {
              local.EabReportSend.RptDetail = "  !! 27 Read Pmt stat";
              UseCabErrorReport();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
                export.Abort.Flag = "Y";

                return;
              }
            }

            if (ReadPaymentStatus())
            {
              if (AsChar(import.DisplayInd.Flag) == 'Y')
              {
                local.EabReportSend.RptDetail =
                  "  !! 28 Read Pmt stat successful.  Create pmt stat hist";
                UseCabErrorReport();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
                  export.Abort.Flag = "Y";

                  return;
                }
              }

              try
              {
                CreatePaymentStatusHistory();

                if (AsChar(import.DisplayInd.Flag) == 'Y')
                {
                  local.EabReportSend.RptDetail =
                    "  !! 29 Create pmt stat hist successful.";
                  UseCabErrorReport();

                  if (!Equal(local.EabFileHandling.Status, "OK"))
                  {
                    ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
                    export.Abort.Flag = "Y";

                    return;
                  }
                }
              }
              catch(Exception e2)
              {
                switch(GetErrorCode(e2))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "FN0000_PMNT_STAT_HIST_AE_RB";

                    return;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "FN0000_PMNT_STAT_HIST_PV_RB";

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
              ExitState = "PAYMENT_STATUS_NF";

              return;
            }
          }
          catch(Exception e1)
          {
            switch(GetErrorCode(e1))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_PMNT_STAT_HIST_NU";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_PMNT_STAT_HIST_PV_RB";

                return;
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

      export.AppliedApCollToRcvInd.Flag = "Y";

      if (local.CollAmtRemaining.Amount <= 0)
      {
        break;
      }
    }

    if (AsChar(import.DisplayInd.Flag) == 'Y')
    {
      local.EabReportSend.RptDetail =
        "  !! 30 about to update collection to mark as processed.";
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
        export.Abort.Flag = "Y";

        return;
      }
    }

    try
    {
      UpdateCollection();

      if (AsChar(import.DisplayInd.Flag) == 'Y')
      {
        local.EabReportSend.RptDetail = "  !! 31 collection updated.";
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
          export.Abort.Flag = "Y";
        }
      }
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0000_COLLECTION_NU";
          export.Abort.Flag = "Y";

          break;
        case ErrorCode.PermittedValueViolation:
          export.Abort.Flag = "Y";
          ExitState = "FN0000_COLLECTION_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private static void MoveTribunalFeeInformation(TribunalFeeInformation source,
    TribunalFeeInformation target)
  {
    target.Rate = source.Rate;
    target.Cap = source.Cap;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseFnAbConcatCrAndCrd()
  {
    var useImport = new FnAbConcatCrAndCrd.Import();
    var useExport = new FnAbConcatCrAndCrd.Export();

    useImport.CashReceipt.SequentialNumber =
      import.CashReceipt.SequentialNumber;
    useImport.CashReceiptDetail.SequentialIdentifier =
      import.CashReceiptDetail.SequentialIdentifier;

    Call(FnAbConcatCrAndCrd.Execute, useImport, useExport);

    local.CrdCrComboNo.CrdCrCombo = useExport.CrdCrComboNo.CrdCrCombo;
  }

  private void UseFnB650DetCrFeePct()
  {
    var useImport = new FnB650DetCrFeePct.Import();
    var useExport = new FnB650DetCrFeePct.Export();

    useImport.Per.Assign(import.Per);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    useImport.Max.Date = local.Max.Date;
    useImport.TestDisplay.Flag = import.DisplayInd.Flag;

    Call(FnB650DetCrFeePct.Execute, useImport, useExport);

    import.Per.Assign(useImport.Per);
    MoveTribunalFeeInformation(useExport.TribunalFeeInformation,
      local.TribunalFeeInformation);
  }

  private void UseFnB651DetIfCrFeeNeeded()
  {
    var useImport = new FnB651DetIfCrFeeNeeded.Import();
    var useExport = new FnB651DetIfCrFeeNeeded.Export();

    useImport.Collection.CollectionDt = import.Per.CollectionDt;
    useImport.Ar.Number = import.Ar.Number;
    useImport.TestDisplay.Flag = import.DisplayInd.Flag;

    Call(FnB651DetIfCrFeeNeeded.Execute, useImport, useExport);

    local.CrFeeNeededInd.Flag = useExport.CrFeeNeededInd.Flag;
  }

  private void UseFnDetermineDisbType()
  {
    var useImport = new FnDetermineDisbType.Import();
    var useExport = new FnDetermineDisbType.Export();

    useImport.Per.Assign(import.Per);
    useImport.ObligationType.SystemGeneratedIdentifier =
      import.ObligationType.SystemGeneratedIdentifier;

    Call(FnDetermineDisbType.Execute, useImport, useExport);

    local.Determined.SystemGeneratedIdentifier =
      useExport.DisbursementType.SystemGeneratedIdentifier;
  }

  private int UseFnGetDisbTranId()
  {
    var useImport = new FnGetDisbTranId.Import();
    var useExport = new FnGetDisbTranId.Export();

    useImport.CsePerson.Number = entities.Obligee1.Number;

    Call(FnGetDisbTranId.Execute, useImport, useExport);

    return useExport.DisbursementTransaction.SystemGeneratedIdentifier;
  }

  private void AssociateDisbursementTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.Cr.Populated);

    var intInterId = entities.InterstateRequest.IntHGeneratedId;

    entities.Cr.Populated = false;
    Update("AssociateDisbursementTransaction",
      (db, command) =>
      {
        db.SetNullableInt32(command, "intInterId", intInterId);
        db.SetString(command, "cpaType", entities.Cr.CpaType);
        db.SetString(command, "cspNumber", entities.Cr.CspNumber);
        db.
          SetInt32(command, "disbTranId", entities.Cr.SystemGeneratedIdentifier);
          
      });

    entities.Cr.IntInterId = intInterId;
    entities.Cr.Populated = true;
  }

  private void CreateDisbursementStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.Db.Populated);

    var dbsGeneratedId = entities.Processed.SystemGeneratedIdentifier;
    var dtrGeneratedId = entities.Db.SystemGeneratedIdentifier;
    var cspNumber = entities.Db.CspNumber;
    var cpaType = entities.Db.CpaType;
    var systemGeneratedIdentifier = 1;
    var effectiveDate = import.ProgramProcessingInfo.ProcessDate;
    var discontinueDate = local.Max.Date;
    var createdBy = global.UserId;
    var createdTimestamp = Now();

    CheckValid<DisbursementStatusHistory>("CpaType", cpaType);
    entities.DisbursementStatusHistory.Populated = false;
    Update("CreateDisbursementStatusHistory",
      (db, command) =>
      {
        db.SetInt32(command, "dbsGeneratedId", dbsGeneratedId);
        db.SetInt32(command, "dtrGeneratedId", dtrGeneratedId);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "cpaType", cpaType);
        db.SetInt32(command, "disbStatHistId", systemGeneratedIdentifier);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "reasonText", "");
        db.SetNullableString(command, "suppressionReason", "");
      });

    entities.DisbursementStatusHistory.DbsGeneratedId = dbsGeneratedId;
    entities.DisbursementStatusHistory.DtrGeneratedId = dtrGeneratedId;
    entities.DisbursementStatusHistory.CspNumber = cspNumber;
    entities.DisbursementStatusHistory.CpaType = cpaType;
    entities.DisbursementStatusHistory.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.DisbursementStatusHistory.EffectiveDate = effectiveDate;
    entities.DisbursementStatusHistory.DiscontinueDate = discontinueDate;
    entities.DisbursementStatusHistory.CreatedBy = createdBy;
    entities.DisbursementStatusHistory.CreatedTimestamp = createdTimestamp;
    entities.DisbursementStatusHistory.SuppressionReason = "";
    entities.DisbursementStatusHistory.Populated = true;
  }

  private void CreateDisbursementTransaction1()
  {
    System.Diagnostics.Debug.Assert(import.Per.Populated);
    System.Diagnostics.Debug.Assert(entities.Obligee2.Populated);

    var cpaType = entities.Obligee2.Type1;
    var cspNumber = entities.Obligee2.CspNumber;
    var systemGeneratedIdentifier =
      local.ForCreateDisbursementTransaction.SystemGeneratedIdentifier;
    var type1 = "C";
    var amount = local.CrAmt.Amount;
    var processDate = import.ProgramProcessingInfo.ProcessDate;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var cashNonCashInd =
      GetImplicitValue<DisbursementTransaction, string>("CashNonCashInd");
    var collectionDate = import.Per.CollectionDt;
    var otyId = import.Per.OtyId;
    var otrTypeDisb = import.Per.OtrType;
    var otrId = import.Per.OtrId;
    var cpaTypeDisb = import.Per.CpaType;
    var cspNumberDisb = import.Per.CspNumber;
    var obgId = import.Per.ObgId;
    var crdId = import.Per.CrdId;
    var crvId = import.Per.CrvId;
    var cstId = import.Per.CstId;
    var crtId = import.Per.CrtType;
    var colId = import.Per.SystemGeneratedIdentifier;
    var interstateInd =
      local.ForCreateDisbursementTransaction.InterstateInd ?? "";
    var referenceNumber = local.ForCreateDisbCollection.ReferenceNumber ?? "";
    var excessUraInd = local.ForCreateDisbursementTransaction.ExcessUraInd ?? ""
      ;

    CheckValid<DisbursementTransaction>("CpaType", cpaType);
    CheckValid<DisbursementTransaction>("Type1", type1);
    CheckValid<DisbursementTransaction>("CashNonCashInd", cashNonCashInd);
    CheckValid<DisbursementTransaction>("OtrTypeDisb", otrTypeDisb);
    CheckValid<DisbursementTransaction>("CpaTypeDisb", cpaTypeDisb);
    entities.Cr.Populated = false;
    Update("CreateDisbursementTransaction1",
      (db, command) =>
      {
        db.SetString(command, "cpaType", cpaType);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "disbTranId", systemGeneratedIdentifier);
        db.SetString(command, "type", type1);
        db.SetDecimal(command, "amount", amount);
        db.SetNullableDate(command, "processDate", processDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", createdBy);
        db.SetNullableDateTime(command, "lastUpdateTmst", createdTimestamp);
        db.SetNullableDate(command, "disbursementDate", null);
        db.SetString(command, "cashNonCashInd", cashNonCashInd);
        db.SetString(command, "recapturedInd", "");
        db.SetNullableDate(command, "collectionDate", collectionDate);
        db.SetDate(command, "collctnProcessDt", processDate);
        db.SetDate(command, "passthruDate", default(DateTime));
        db.SetNullableInt32(command, "otyId", otyId);
        db.SetNullableString(command, "otrTypeDisb", otrTypeDisb);
        db.SetNullableInt32(command, "otrId", otrId);
        db.SetNullableString(command, "cpaTypeDisb", cpaTypeDisb);
        db.SetNullableString(command, "cspNumberDisb", cspNumberDisb);
        db.SetNullableInt32(command, "obgId", obgId);
        db.SetNullableInt32(command, "crdId", crdId);
        db.SetNullableInt32(command, "crvId", crvId);
        db.SetNullableInt32(command, "cstId", cstId);
        db.SetNullableInt32(command, "crtId", crtId);
        db.SetNullableInt32(command, "colId", colId);
        db.SetNullableString(command, "interstateInd", interstateInd);
        db.SetNullableString(command, "designatedPayee", "");
        db.SetNullableString(command, "referenceNumber", referenceNumber);
        db.SetInt32(command, "uraExcollSnbr", 0);
        db.SetNullableString(command, "excessUraInd", excessUraInd);
      });

    entities.Cr.CpaType = cpaType;
    entities.Cr.CspNumber = cspNumber;
    entities.Cr.SystemGeneratedIdentifier = systemGeneratedIdentifier;
    entities.Cr.Type1 = type1;
    entities.Cr.Amount = amount;
    entities.Cr.ProcessDate = processDate;
    entities.Cr.CreatedBy = createdBy;
    entities.Cr.CreatedTimestamp = createdTimestamp;
    entities.Cr.LastUpdatedBy = createdBy;
    entities.Cr.LastUpdateTmst = createdTimestamp;
    entities.Cr.DisbursementDate = null;
    entities.Cr.CashNonCashInd = cashNonCashInd;
    entities.Cr.RecapturedInd = "";
    entities.Cr.CollectionDate = collectionDate;
    entities.Cr.CollectionProcessDate = processDate;
    entities.Cr.OtyId = otyId;
    entities.Cr.OtrTypeDisb = otrTypeDisb;
    entities.Cr.OtrId = otrId;
    entities.Cr.CpaTypeDisb = cpaTypeDisb;
    entities.Cr.CspNumberDisb = cspNumberDisb;
    entities.Cr.ObgId = obgId;
    entities.Cr.CrdId = crdId;
    entities.Cr.CrvId = crvId;
    entities.Cr.CstId = cstId;
    entities.Cr.CrtId = crtId;
    entities.Cr.ColId = colId;
    entities.Cr.InterstateInd = interstateInd;
    entities.Cr.ReferenceNumber = referenceNumber;
    entities.Cr.IntInterId = null;
    entities.Cr.ExcessUraInd = excessUraInd;
    entities.Cr.Populated = true;
  }

  private void CreateDisbursementTransaction2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligee2.Populated);

    var cpaType = entities.Obligee2.Type1;
    var cspNumber = entities.Obligee2.CspNumber;
    var systemGeneratedIdentifier =
      local.ForCreateDisbursementTransaction.SystemGeneratedIdentifier;
    var type1 = "D";
    var amount = local.DbAmt.Amount;
    var processDate = import.ProgramProcessingInfo.ProcessDate;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var cashNonCashInd = import.CashReceiptType.CategoryIndicator;
    var recapturedInd = "N";
    var collectionDate = entities.Cr.CollectionDate;
    var dbtGeneratedId = entities.DisbursementType.SystemGeneratedIdentifier;
    var prqGeneratedId = entities.PaymentRequest.SystemGeneratedIdentifier;
    var interstateInd = entities.Cr.InterstateInd;
    var referenceNumber = entities.Cr.ReferenceNumber;
    var excessUraInd = local.ForUpdateDisbursementTransaction.ExcessUraInd ?? ""
      ;

    CheckValid<DisbursementTransaction>("CpaType", cpaType);
    CheckValid<DisbursementTransaction>("Type1", type1);
    CheckValid<DisbursementTransaction>("CashNonCashInd", cashNonCashInd);
    entities.Db.Populated = false;
    Update("CreateDisbursementTransaction2",
      (db, command) =>
      {
        db.SetString(command, "cpaType", cpaType);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "disbTranId", systemGeneratedIdentifier);
        db.SetString(command, "type", type1);
        db.SetDecimal(command, "amount", amount);
        db.SetNullableDate(command, "processDate", processDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", createdBy);
        db.SetNullableDateTime(command, "lastUpdateTmst", createdTimestamp);
        db.SetNullableDate(command, "disbursementDate", processDate);
        db.SetString(command, "cashNonCashInd", cashNonCashInd);
        db.SetString(command, "recapturedInd", recapturedInd);
        db.SetNullableDate(command, "collectionDate", collectionDate);
        db.SetDate(command, "collctnProcessDt", null);
        db.SetDate(command, "passthruDate", default(DateTime));
        db.SetNullableInt32(command, "dbtGeneratedId", dbtGeneratedId);
        db.SetNullableInt32(command, "prqGeneratedId", prqGeneratedId);
        db.SetNullableString(
          command, "otrTypeDisb", GetImplicitValue<DisbursementTransaction,
          string>("OtrTypeDisb"));
        db.SetNullableString(
          command, "cpaTypeDisb", GetImplicitValue<DisbursementTransaction,
          string>("CpaTypeDisb"));
        db.SetNullableString(command, "interstateInd", interstateInd);
        db.SetNullableString(command, "designatedPayee", "");
        db.SetNullableString(command, "referenceNumber", referenceNumber);
        db.SetInt32(command, "uraExcollSnbr", 0);
        db.SetNullableString(command, "excessUraInd", excessUraInd);
      });

    entities.Db.CpaType = cpaType;
    entities.Db.CspNumber = cspNumber;
    entities.Db.SystemGeneratedIdentifier = systemGeneratedIdentifier;
    entities.Db.Type1 = type1;
    entities.Db.Amount = amount;
    entities.Db.ProcessDate = processDate;
    entities.Db.CreatedBy = createdBy;
    entities.Db.CreatedTimestamp = createdTimestamp;
    entities.Db.LastUpdatedBy = createdBy;
    entities.Db.LastUpdateTmst = createdTimestamp;
    entities.Db.DisbursementDate = processDate;
    entities.Db.CashNonCashInd = cashNonCashInd;
    entities.Db.RecapturedInd = recapturedInd;
    entities.Db.CollectionDate = collectionDate;
    entities.Db.CollectionProcessDate = null;
    entities.Db.DbtGeneratedId = dbtGeneratedId;
    entities.Db.PrqGeneratedId = prqGeneratedId;
    entities.Db.InterstateInd = interstateInd;
    entities.Db.ReferenceNumber = referenceNumber;
    entities.Db.ExcessUraInd = excessUraInd;
    entities.Db.Populated = true;
  }

  private void CreateDisbursementTransaction3()
  {
    System.Diagnostics.Debug.Assert(entities.Obligee2.Populated);

    var cpaType = entities.Obligee2.Type1;
    var cspNumber = entities.Obligee2.CspNumber;
    var systemGeneratedIdentifier =
      local.ForCreateDisbursementTransaction.SystemGeneratedIdentifier;
    var type1 = "D";
    var amount = local.CrFeeAmt.Amount;
    var processDate = import.ProgramProcessingInfo.ProcessDate;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var cashNonCashInd = import.CashReceiptType.CategoryIndicator;
    var recapturedInd = "N";
    var collectionDate = entities.Cr.CollectionDate;
    var dbtGeneratedId = entities.CrFee.SystemGeneratedIdentifier;
    var interstateInd = entities.Cr.InterstateInd;
    var referenceNumber = entities.Cr.ReferenceNumber;
    var excessUraInd = local.ForUpdateDisbursementTransaction.ExcessUraInd ?? ""
      ;

    CheckValid<DisbursementTransaction>("CpaType", cpaType);
    CheckValid<DisbursementTransaction>("Type1", type1);
    CheckValid<DisbursementTransaction>("CashNonCashInd", cashNonCashInd);
    entities.Db.Populated = false;
    Update("CreateDisbursementTransaction3",
      (db, command) =>
      {
        db.SetString(command, "cpaType", cpaType);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "disbTranId", systemGeneratedIdentifier);
        db.SetString(command, "type", type1);
        db.SetDecimal(command, "amount", amount);
        db.SetNullableDate(command, "processDate", processDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", createdBy);
        db.SetNullableDateTime(command, "lastUpdateTmst", createdTimestamp);
        db.SetNullableDate(command, "disbursementDate", processDate);
        db.SetString(command, "cashNonCashInd", cashNonCashInd);
        db.SetString(command, "recapturedInd", recapturedInd);
        db.SetNullableDate(command, "collectionDate", collectionDate);
        db.SetDate(command, "collctnProcessDt", null);
        db.SetDate(command, "passthruDate", default(DateTime));
        db.SetNullableInt32(command, "dbtGeneratedId", dbtGeneratedId);
        db.SetNullableString(
          command, "otrTypeDisb", GetImplicitValue<DisbursementTransaction,
          string>("OtrTypeDisb"));
        db.SetNullableString(
          command, "cpaTypeDisb", GetImplicitValue<DisbursementTransaction,
          string>("CpaTypeDisb"));
        db.SetNullableString(command, "interstateInd", interstateInd);
        db.SetNullableString(command, "designatedPayee", "");
        db.SetNullableString(command, "referenceNumber", referenceNumber);
        db.SetInt32(command, "uraExcollSnbr", 0);
        db.SetNullableString(command, "excessUraInd", excessUraInd);
      });

    entities.Db.CpaType = cpaType;
    entities.Db.CspNumber = cspNumber;
    entities.Db.SystemGeneratedIdentifier = systemGeneratedIdentifier;
    entities.Db.Type1 = type1;
    entities.Db.Amount = amount;
    entities.Db.ProcessDate = processDate;
    entities.Db.CreatedBy = createdBy;
    entities.Db.CreatedTimestamp = createdTimestamp;
    entities.Db.LastUpdatedBy = createdBy;
    entities.Db.LastUpdateTmst = createdTimestamp;
    entities.Db.DisbursementDate = processDate;
    entities.Db.CashNonCashInd = cashNonCashInd;
    entities.Db.RecapturedInd = recapturedInd;
    entities.Db.CollectionDate = collectionDate;
    entities.Db.CollectionProcessDate = null;
    entities.Db.DbtGeneratedId = dbtGeneratedId;
    entities.Db.PrqGeneratedId = null;
    entities.Db.InterstateInd = interstateInd;
    entities.Db.ReferenceNumber = referenceNumber;
    entities.Db.ExcessUraInd = excessUraInd;
    entities.Db.Populated = true;
  }

  private void CreateDisbursementTransactionRln()
  {
    System.Diagnostics.Debug.Assert(entities.Cr.Populated);
    System.Diagnostics.Debug.Assert(entities.Db.Populated);

    var systemGeneratedIdentifier = 1;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var dnrGeneratedId =
      entities.DisbursementTranRlnRsn.SystemGeneratedIdentifier;
    var cspNumber = entities.Db.CspNumber;
    var cpaType = entities.Db.CpaType;
    var dtrGeneratedId = entities.Db.SystemGeneratedIdentifier;
    var cspPNumber = entities.Cr.CspNumber;
    var cpaPType = entities.Cr.CpaType;
    var dtrPGeneratedId = entities.Cr.SystemGeneratedIdentifier;

    CheckValid<DisbursementTransactionRln>("CpaType", cpaType);
    CheckValid<DisbursementTransactionRln>("CpaPType", cpaPType);
    entities.DisbursementTransactionRln.Populated = false;
    Update("CreateDisbursementTransactionRln",
      (db, command) =>
      {
        db.SetInt32(command, "disbTranRlnId", systemGeneratedIdentifier);
        db.SetNullableString(command, "description", "");
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetInt32(command, "dnrGeneratedId", dnrGeneratedId);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "cpaType", cpaType);
        db.SetInt32(command, "dtrGeneratedId", dtrGeneratedId);
        db.SetString(command, "cspPNumber", cspPNumber);
        db.SetString(command, "cpaPType", cpaPType);
        db.SetInt32(command, "dtrPGeneratedId", dtrPGeneratedId);
      });

    entities.DisbursementTransactionRln.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.DisbursementTransactionRln.CreatedBy = createdBy;
    entities.DisbursementTransactionRln.CreatedTimestamp = createdTimestamp;
    entities.DisbursementTransactionRln.DnrGeneratedId = dnrGeneratedId;
    entities.DisbursementTransactionRln.CspNumber = cspNumber;
    entities.DisbursementTransactionRln.CpaType = cpaType;
    entities.DisbursementTransactionRln.DtrGeneratedId = dtrGeneratedId;
    entities.DisbursementTransactionRln.CspPNumber = cspPNumber;
    entities.DisbursementTransactionRln.CpaPType = cpaPType;
    entities.DisbursementTransactionRln.DtrPGeneratedId = dtrPGeneratedId;
    entities.DisbursementTransactionRln.Populated = true;
  }

  private void CreatePaymentStatusHistory()
  {
    var pstGeneratedId = entities.Denied.SystemGeneratedIdentifier;
    var prqGeneratedId = entities.PaymentRequest.SystemGeneratedIdentifier;
    var systemGeneratedIdentifier =
      local.ForUpdatePaymentStatusHistory.SystemGeneratedIdentifier;
    var effectiveDate = import.ProgramProcessingInfo.ProcessDate;
    var discontinueDate = local.Max.Date;
    var createdBy = global.UserId;
    var createdTimestamp = Now();

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
    entities.PaymentStatusHistory.Populated = true;
  }

  private bool ReadDisbursement1()
  {
    local.TotAmtForArRefNbrPmtR.Populated = false;

    return Read("ReadDisbursement1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prqGeneratedId",
          entities.PaymentRequest.SystemGeneratedIdentifier);
        db.SetNullableString(
          command, "referenceNumber", local.RefNbr.ReferenceNumber ?? "");
      },
      (db, reader) =>
      {
        local.TotAmtForArRefNbrPmtR.Amount = db.GetDecimal(reader, 0);
        local.TotAmtForArRefNbrPmtR.Populated = true;
      });
  }

  private IEnumerable<bool> ReadDisbursement2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligee2.Populated);
    entities.NetDisbForArRefNbr.Populated = false;

    return ReadEach("ReadDisbursement2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prqGeneratedId",
          entities.PaymentRequest.SystemGeneratedIdentifier);
        db.SetNullableString(
          command, "referenceNumber", local.RefNbr.ReferenceNumber ?? "");
        db.SetString(command, "cpaType", entities.Obligee2.Type1);
        db.SetString(command, "cspNumber", entities.Obligee2.CspNumber);
      },
      (db, reader) =>
      {
        entities.NetDisbForArRefNbr.CpaType = db.GetString(reader, 0);
        entities.NetDisbForArRefNbr.CspNumber = db.GetString(reader, 1);
        entities.NetDisbForArRefNbr.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.NetDisbForArRefNbr.Amount = db.GetDecimal(reader, 3);
        entities.NetDisbForArRefNbr.PrqGeneratedId =
          db.GetNullableInt32(reader, 4);
        entities.NetDisbForArRefNbr.ReferenceNumber =
          db.GetNullableString(reader, 5);
        entities.NetDisbForArRefNbr.Populated = true;

        return true;
      });
  }

  private bool ReadDisbursementStatus()
  {
    entities.Processed.Populated = false;

    return Read("ReadDisbursementStatus",
      null,
      (db, reader) =>
      {
        entities.Processed.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Processed.Populated = true;
      });
  }

  private bool ReadDisbursementTranRlnRsn()
  {
    entities.DisbursementTranRlnRsn.Populated = false;

    return Read("ReadDisbursementTranRlnRsn",
      null,
      (db, reader) =>
      {
        entities.DisbursementTranRlnRsn.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementTranRlnRsn.Populated = true;
      });
  }

  private bool ReadDisbursementType1()
  {
    entities.DisbursementType.Populated = false;

    return Read("ReadDisbursementType1",
      (db, command) =>
      {
        db.SetInt32(
          command, "disbTypeId", local.Determined.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.DisbursementType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementType.Code = db.GetString(reader, 1);
        entities.DisbursementType.Populated = true;
      });
  }

  private bool ReadDisbursementType2()
  {
    entities.CrFee.Populated = false;

    return Read("ReadDisbursementType2",
      null,
      (db, reader) =>
      {
        entities.CrFee.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.CrFee.Populated = true;
      });
  }

  private bool ReadInterstateRequest()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest",
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
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.Populated = true;
      });
  }

  private bool ReadObligation()
  {
    System.Diagnostics.Debug.Assert(import.Per.Populated);
    entities.Obligation.Populated = false;

    return Read("ReadObligation",
      (db, command) =>
      {
        db.SetInt32(command, "dtyGeneratedId", import.Per.OtyId);
        db.SetInt32(command, "obId", import.Per.ObgId);
        db.SetString(command, "cspNumber", import.Per.CspNumber);
        db.SetString(command, "cpaType", import.Per.CpaType);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.Populated = true;
      });
  }

  private bool ReadObligee()
  {
    entities.Obligee2.Populated = false;

    return Read("ReadObligee",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.Ar.Number);
      },
      (db, reader) =>
      {
        entities.Obligee2.CspNumber = db.GetString(reader, 0);
        entities.Obligee2.Type1 = db.GetString(reader, 1);
        entities.Obligee2.CreatedBy = db.GetString(reader, 2);
        entities.Obligee2.CreatedTmst = db.GetDateTime(reader, 3);
        entities.Obligee2.Populated = true;
      });
  }

  private IEnumerable<bool> ReadPaymentRequestPaymentStatusHistory1()
  {
    entities.PaymentRequest.Populated = false;
    entities.PaymentStatusHistory.Populated = false;

    return ReadEach("ReadPaymentRequestPaymentStatusHistory1",
      (db, command) =>
      {
        db.SetNullableString(command, "csePersonNumber", import.Ar.Number);
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentStatusHistory.PrqGeneratedId = db.GetInt32(reader, 0);
        entities.PaymentRequest.Amount = db.GetDecimal(reader, 1);
        entities.PaymentRequest.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.PaymentRequest.CsePersonNumber =
          db.GetNullableString(reader, 3);
        entities.PaymentRequest.RecoveryFiller = db.GetString(reader, 4);
        entities.PaymentRequest.Type1 = db.GetString(reader, 5);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 6);
        entities.PaymentStatusHistory.PstGeneratedId = db.GetInt32(reader, 7);
        entities.PaymentStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 8);
        entities.PaymentStatusHistory.EffectiveDate = db.GetDate(reader, 9);
        entities.PaymentStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 10);
        entities.PaymentStatusHistory.CreatedBy = db.GetString(reader, 11);
        entities.PaymentStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 12);
        entities.PaymentRequest.Populated = true;
        entities.PaymentStatusHistory.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadPaymentRequestPaymentStatusHistory2()
  {
    entities.PaymentRequest.Populated = false;
    entities.PaymentStatusHistory.Populated = false;

    return ReadEach("ReadPaymentRequestPaymentStatusHistory2",
      (db, command) =>
      {
        db.SetNullableString(command, "csePersonNumber", import.Ar.Number);
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentStatusHistory.PrqGeneratedId = db.GetInt32(reader, 0);
        entities.PaymentRequest.Amount = db.GetDecimal(reader, 1);
        entities.PaymentRequest.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.PaymentRequest.CsePersonNumber =
          db.GetNullableString(reader, 3);
        entities.PaymentRequest.RecoveryFiller = db.GetString(reader, 4);
        entities.PaymentRequest.Type1 = db.GetString(reader, 5);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 6);
        entities.PaymentStatusHistory.PstGeneratedId = db.GetInt32(reader, 7);
        entities.PaymentStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 8);
        entities.PaymentStatusHistory.EffectiveDate = db.GetDate(reader, 9);
        entities.PaymentStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 10);
        entities.PaymentStatusHistory.CreatedBy = db.GetString(reader, 11);
        entities.PaymentStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 12);
        entities.PaymentRequest.Populated = true;
        entities.PaymentStatusHistory.Populated = true;

        return true;
      });
  }

  private bool ReadPaymentStatus()
  {
    entities.Denied.Populated = false;

    return Read("ReadPaymentStatus",
      null,
      (db, reader) =>
      {
        entities.Denied.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Denied.Populated = true;
      });
  }

  private void UpdateCollection()
  {
    System.Diagnostics.Debug.Assert(import.Per.Populated);

    var disbursementDt = import.ProgramProcessingInfo.ProcessDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();
    var arNumber = import.Ar.Number;

    import.Per.Populated = false;
    Update("UpdateCollection",
      (db, command) =>
      {
        db.SetNullableDate(command, "disbDt", disbursementDt);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableString(command, "arNumber", arNumber);
        db.SetInt32(command, "collId", import.Per.SystemGeneratedIdentifier);
        db.SetInt32(command, "crtType", import.Per.CrtType);
        db.SetInt32(command, "cstId", import.Per.CstId);
        db.SetInt32(command, "crvId", import.Per.CrvId);
        db.SetInt32(command, "crdId", import.Per.CrdId);
        db.SetInt32(command, "obgId", import.Per.ObgId);
        db.SetString(command, "cspNumber", import.Per.CspNumber);
        db.SetString(command, "cpaType", import.Per.CpaType);
        db.SetInt32(command, "otrId", import.Per.OtrId);
        db.SetString(command, "otrType", import.Per.OtrType);
        db.SetInt32(command, "otyId", import.Per.OtyId);
      });

    import.Per.DisbursementDt = disbursementDt;
    import.Per.LastUpdatedBy = lastUpdatedBy;
    import.Per.LastUpdatedTmst = lastUpdatedTmst;
    import.Per.ArNumber = arNumber;
    import.Per.Populated = true;
  }

  private void UpdatePaymentRequest()
  {
    var amount = entities.PaymentRequest.Amount - local.DbAmt.Amount;

    entities.PaymentRequest.Populated = false;
    Update("UpdatePaymentRequest",
      (db, command) =>
      {
        db.SetDecimal(command, "amount", amount);
        db.SetInt32(
          command, "paymentRequestId",
          entities.PaymentRequest.SystemGeneratedIdentifier);
      });

    entities.PaymentRequest.Amount = amount;
    entities.PaymentRequest.Populated = true;
  }

  private void UpdatePaymentStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.PaymentStatusHistory.Populated);

    var discontinueDate = import.ProgramProcessingInfo.ProcessDate;

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
    /// A value of ExpCollsEligForRcvCnt.
    /// </summary>
    [JsonPropertyName("expCollsEligForRcvCnt")]
    public Common ExpCollsEligForRcvCnt
    {
      get => expCollsEligForRcvCnt ??= new();
      set => expCollsEligForRcvCnt = value;
    }

    /// <summary>
    /// A value of Per.
    /// </summary>
    [JsonPropertyName("per")]
    public Collection Per
    {
      get => per ??= new();
      set => per = value;
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
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
    }

    /// <summary>
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    /// <summary>
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
    }

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePerson Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of TheStatesPersonNumber.
    /// </summary>
    [JsonPropertyName("theStatesPersonNumber")]
    public CsePerson TheStatesPersonNumber
    {
      get => theStatesPersonNumber ??= new();
      set => theStatesPersonNumber = value;
    }

    /// <summary>
    /// A value of TheStatesPersonNumber2.
    /// </summary>
    [JsonPropertyName("theStatesPersonNumber2")]
    public CsePerson TheStatesPersonNumber2
    {
      get => theStatesPersonNumber2 ??= new();
      set => theStatesPersonNumber2 = value;
    }

    /// <summary>
    /// A value of TheStatesPersonNumber3.
    /// </summary>
    [JsonPropertyName("theStatesPersonNumber3")]
    public CsePerson TheStatesPersonNumber3
    {
      get => theStatesPersonNumber3 ??= new();
      set => theStatesPersonNumber3 = value;
    }

    /// <summary>
    /// A value of JuvenileJusticeAuthority.
    /// </summary>
    [JsonPropertyName("juvenileJusticeAuthority")]
    public CsePerson JuvenileJusticeAuthority
    {
      get => juvenileJusticeAuthority ??= new();
      set => juvenileJusticeAuthority = value;
    }

    /// <summary>
    /// A value of DisplayInd.
    /// </summary>
    [JsonPropertyName("displayInd")]
    public Common DisplayInd
    {
      get => displayInd ??= new();
      set => displayInd = value;
    }

    /// <summary>
    /// A value of ApplyApCollToRcv.
    /// </summary>
    [JsonPropertyName("applyApCollToRcv")]
    public Common ApplyApCollToRcv
    {
      get => applyApCollToRcv ??= new();
      set => applyApCollToRcv = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    private Common expCollsEligForRcvCnt;
    private Collection per;
    private ObligationType obligationType;
    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptType cashReceiptType;
    private CsePerson ar;
    private ProgramProcessingInfo programProcessingInfo;
    private CsePerson theStatesPersonNumber;
    private CsePerson theStatesPersonNumber2;
    private CsePerson theStatesPersonNumber3;
    private CsePerson juvenileJusticeAuthority;
    private Common displayInd;
    private Common applyApCollToRcv;
    private CsePerson ap;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of AppliedApCollToRcvInd.
    /// </summary>
    [JsonPropertyName("appliedApCollToRcvInd")]
    public Common AppliedApCollToRcvInd
    {
      get => appliedApCollToRcvInd ??= new();
      set => appliedApCollToRcvInd = value;
    }

    /// <summary>
    /// A value of Abort.
    /// </summary>
    [JsonPropertyName("abort")]
    public Common Abort
    {
      get => abort ??= new();
      set => abort = value;
    }

    private Common appliedApCollToRcvInd;
    private Common abort;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ForUpdatePaymentStatusHistory.
    /// </summary>
    [JsonPropertyName("forUpdatePaymentStatusHistory")]
    public PaymentStatusHistory ForUpdatePaymentStatusHistory
    {
      get => forUpdatePaymentStatusHistory ??= new();
      set => forUpdatePaymentStatusHistory = value;
    }

    /// <summary>
    /// A value of TotAmtForArRefNbrPmtR.
    /// </summary>
    [JsonPropertyName("totAmtForArRefNbrPmtR")]
    public DisbursementTransaction TotAmtForArRefNbrPmtR
    {
      get => totAmtForArRefNbrPmtR ??= new();
      set => totAmtForArRefNbrPmtR = value;
    }

    /// <summary>
    /// A value of TotAmtForArRefNbr.
    /// </summary>
    [JsonPropertyName("totAmtForArRefNbr")]
    public DisbursementTransaction TotAmtForArRefNbr
    {
      get => totAmtForArRefNbr ??= new();
      set => totAmtForArRefNbr = value;
    }

    /// <summary>
    /// A value of CollAmtRemaining.
    /// </summary>
    [JsonPropertyName("collAmtRemaining")]
    public DisbursementTransaction CollAmtRemaining
    {
      get => collAmtRemaining ??= new();
      set => collAmtRemaining = value;
    }

    /// <summary>
    /// A value of CrAmt.
    /// </summary>
    [JsonPropertyName("crAmt")]
    public DisbursementTransaction CrAmt
    {
      get => crAmt ??= new();
      set => crAmt = value;
    }

    /// <summary>
    /// A value of DbAmt.
    /// </summary>
    [JsonPropertyName("dbAmt")]
    public DisbursementTransaction DbAmt
    {
      get => dbAmt ??= new();
      set => dbAmt = value;
    }

    /// <summary>
    /// A value of CrFeeAmt.
    /// </summary>
    [JsonPropertyName("crFeeAmt")]
    public DisbursementTransaction CrFeeAmt
    {
      get => crFeeAmt ??= new();
      set => crFeeAmt = value;
    }

    /// <summary>
    /// A value of TribunalFeeInformation.
    /// </summary>
    [JsonPropertyName("tribunalFeeInformation")]
    public TribunalFeeInformation TribunalFeeInformation
    {
      get => tribunalFeeInformation ??= new();
      set => tribunalFeeInformation = value;
    }

    /// <summary>
    /// A value of ForCreateDisbCollection.
    /// </summary>
    [JsonPropertyName("forCreateDisbCollection")]
    public DisbursementTransaction ForCreateDisbCollection
    {
      get => forCreateDisbCollection ??= new();
      set => forCreateDisbCollection = value;
    }

    /// <summary>
    /// A value of CrFeeNeededInd.
    /// </summary>
    [JsonPropertyName("crFeeNeededInd")]
    public Common CrFeeNeededInd
    {
      get => crFeeNeededInd ??= new();
      set => crFeeNeededInd = value;
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
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
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
    /// A value of CrdCrComboNo.
    /// </summary>
    [JsonPropertyName("crdCrComboNo")]
    public CrdCrComboNo CrdCrComboNo
    {
      get => crdCrComboNo ??= new();
      set => crdCrComboNo = value;
    }

    /// <summary>
    /// A value of RefNbr.
    /// </summary>
    [JsonPropertyName("refNbr")]
    public DisbursementTransaction RefNbr
    {
      get => refNbr ??= new();
      set => refNbr = value;
    }

    /// <summary>
    /// A value of ForCreateDisbursementTransaction.
    /// </summary>
    [JsonPropertyName("forCreateDisbursementTransaction")]
    public DisbursementTransaction ForCreateDisbursementTransaction
    {
      get => forCreateDisbursementTransaction ??= new();
      set => forCreateDisbursementTransaction = value;
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
    /// A value of AfInvolvementInd.
    /// </summary>
    [JsonPropertyName("afInvolvementInd")]
    public Common AfInvolvementInd
    {
      get => afInvolvementInd ??= new();
      set => afInvolvementInd = value;
    }

    /// <summary>
    /// A value of Numeric.
    /// </summary>
    [JsonPropertyName("numeric")]
    public BatchTimestampWorkArea Numeric
    {
      get => numeric ??= new();
      set => numeric = value;
    }

    /// <summary>
    /// A value of Text.
    /// </summary>
    [JsonPropertyName("text")]
    public DateWorkArea Text
    {
      get => text ??= new();
      set => text = value;
    }

    /// <summary>
    /// A value of Text2.
    /// </summary>
    [JsonPropertyName("text2")]
    public DateWorkArea Text2
    {
      get => text2 ??= new();
      set => text2 = value;
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
    /// A value of Determined.
    /// </summary>
    [JsonPropertyName("determined")]
    public DisbursementType Determined
    {
      get => determined ??= new();
      set => determined = value;
    }

    /// <summary>
    /// A value of ForUpdateDisbursementTransaction.
    /// </summary>
    [JsonPropertyName("forUpdateDisbursementTransaction")]
    public DisbursementTransaction ForUpdateDisbursementTransaction
    {
      get => forUpdateDisbursementTransaction ??= new();
      set => forUpdateDisbursementTransaction = value;
    }

    /// <summary>
    /// A value of Sign.
    /// </summary>
    [JsonPropertyName("sign")]
    public TextWorkArea Sign
    {
      get => sign ??= new();
      set => sign = value;
    }

    private PaymentStatusHistory forUpdatePaymentStatusHistory;
    private DisbursementTransaction totAmtForArRefNbrPmtR;
    private DisbursementTransaction totAmtForArRefNbr;
    private DisbursementTransaction collAmtRemaining;
    private DisbursementTransaction crAmt;
    private DisbursementTransaction dbAmt;
    private DisbursementTransaction crFeeAmt;
    private TribunalFeeInformation tribunalFeeInformation;
    private DisbursementTransaction forCreateDisbCollection;
    private Common crFeeNeededInd;
    private DateWorkArea max;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private CrdCrComboNo crdCrComboNo;
    private DisbursementTransaction refNbr;
    private DisbursementTransaction forCreateDisbursementTransaction;
    private DateWorkArea initialized;
    private Common afInvolvementInd;
    private BatchTimestampWorkArea numeric;
    private DateWorkArea text;
    private DateWorkArea text2;
    private DateWorkArea discontinueDate;
    private DisbursementType determined;
    private DisbursementTransaction forUpdateDisbursementTransaction;
    private TextWorkArea sign;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Denied.
    /// </summary>
    [JsonPropertyName("denied")]
    public PaymentStatus Denied
    {
      get => denied ??= new();
      set => denied = value;
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
    /// A value of Obligee1.
    /// </summary>
    [JsonPropertyName("obligee1")]
    public CsePerson Obligee1
    {
      get => obligee1 ??= new();
      set => obligee1 = value;
    }

    /// <summary>
    /// A value of Cr.
    /// </summary>
    [JsonPropertyName("cr")]
    public DisbursementTransaction Cr
    {
      get => cr ??= new();
      set => cr = value;
    }

    /// <summary>
    /// A value of Db.
    /// </summary>
    [JsonPropertyName("db")]
    public DisbursementTransaction Db
    {
      get => db ??= new();
      set => db = value;
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
    /// A value of DisbursementType.
    /// </summary>
    [JsonPropertyName("disbursementType")]
    public DisbursementType DisbursementType
    {
      get => disbursementType ??= new();
      set => disbursementType = value;
    }

    /// <summary>
    /// A value of Obligee2.
    /// </summary>
    [JsonPropertyName("obligee2")]
    public CsePersonAccount Obligee2
    {
      get => obligee2 ??= new();
      set => obligee2 = value;
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
    /// A value of NetDisbForArRefNbr.
    /// </summary>
    [JsonPropertyName("netDisbForArRefNbr")]
    public DisbursementTransaction NetDisbForArRefNbr
    {
      get => netDisbForArRefNbr ??= new();
      set => netDisbForArRefNbr = value;
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
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
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
    /// A value of DisbursementStatusHistory.
    /// </summary>
    [JsonPropertyName("disbursementStatusHistory")]
    public DisbursementStatusHistory DisbursementStatusHistory
    {
      get => disbursementStatusHistory ??= new();
      set => disbursementStatusHistory = value;
    }

    /// <summary>
    /// A value of Processed.
    /// </summary>
    [JsonPropertyName("processed")]
    public DisbursementStatus Processed
    {
      get => processed ??= new();
      set => processed = value;
    }

    /// <summary>
    /// A value of CrFee.
    /// </summary>
    [JsonPropertyName("crFee")]
    public DisbursementType CrFee
    {
      get => crFee ??= new();
      set => crFee = value;
    }

    /// <summary>
    /// A value of DisbursementTranRlnRsn.
    /// </summary>
    [JsonPropertyName("disbursementTranRlnRsn")]
    public DisbursementTranRlnRsn DisbursementTranRlnRsn
    {
      get => disbursementTranRlnRsn ??= new();
      set => disbursementTranRlnRsn = value;
    }

    /// <summary>
    /// A value of DisbursementTransactionRln.
    /// </summary>
    [JsonPropertyName("disbursementTransactionRln")]
    public DisbursementTransactionRln DisbursementTransactionRln
    {
      get => disbursementTransactionRln ??= new();
      set => disbursementTransactionRln = value;
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

    private PaymentStatus denied;
    private PaymentRequest paymentRequest;
    private CsePerson obligee1;
    private DisbursementTransaction cr;
    private DisbursementTransaction db;
    private ObligationTransaction obligationTransaction;
    private Obligation obligation;
    private DisbursementType disbursementType;
    private CsePersonAccount obligee2;
    private PaymentStatusHistory paymentStatusHistory;
    private DisbursementTransaction netDisbForArRefNbr;
    private InterstateRequest interstateRequest;
    private DebtDetail debtDetail;
    private InterstateRequestObligation interstateRequestObligation;
    private DisbursementStatusHistory disbursementStatusHistory;
    private DisbursementStatus processed;
    private DisbursementType crFee;
    private DisbursementTranRlnRsn disbursementTranRlnRsn;
    private DisbursementTransactionRln disbursementTransactionRln;
    private PaymentStatus paymentStatus;
  }
#endregion
}
