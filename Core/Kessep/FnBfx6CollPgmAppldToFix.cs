// Program: FN_BFX6_COLL_PGM_APPLD_TO_FIX, ID: 372907165, model: 746.
// Short name: SWEFBF6B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_BFX6_COLL_PGM_APPLD_TO_FIX.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnBfx6CollPgmAppldToFix: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_BFX6_COLL_PGM_APPLD_TO_FIX program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnBfx6CollPgmAppldToFix(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnBfx6CollPgmAppldToFix.
  /// </summary>
  public FnBfx6CollPgmAppldToFix(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    ExitState = "ACO_NN0000_ALL_OK";
    local.HardcodedAccruingClass.Classification = "A";
    local.MaxDiscontinue.Date = UseCabSetMaximumDiscontinueDate();
    local.UserId.Text8 = global.UserId;
    local.Current.Date = Now().Date;
    local.Current.Timestamp = Now();
    local.EabReportSend.ProcessDate = Now().Date;
    local.EabReportSend.ProgramName = local.UserId.Text8;
    UseFnExtGetParmsThruJclSysin();

    if (local.External.NumericReturnCode != 0)
    {
      ExitState = "FN0000_SYSIN_PARM_ERROR_A";

      return;
    }

    if (IsEmpty(local.ProgramProcessingInfo.ParameterList))
    {
      ExitState = "FN0000_SYSIN_PARM_ERROR_A";

      return;
    }

    local.ProcessUpdatesInd.Flag =
      Substring(local.ProgramProcessingInfo.ParameterList, 1, 1);

    if (IsEmpty(local.ProcessUpdatesInd.Flag))
    {
      local.ProcessUpdatesInd.Flag = "N";
    }

    local.Process718BInd.Flag =
      Substring(local.ProgramProcessingInfo.ParameterList, 3, 1);

    if (IsEmpty(local.Process718BInd.Flag))
    {
      local.Process718BInd.Flag = "N";
    }

    local.ProcessReipPymntsInd.Flag =
      Substring(local.ProgramProcessingInfo.ParameterList, 5, 1);

    if (IsEmpty(local.ProcessReipPymntsInd.Flag))
    {
      local.ProcessReipPymntsInd.Flag = "N";
    }

    local.EabFileHandling.Action = "OPEN";

    // **********************************************************
    // OPEN OUTPUT CONTROL REPORT 98
    // **********************************************************
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    // **********************************************************
    // OPEN OUTPUT ERROR REPORT 99
    // **********************************************************
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_ERROR_RPT_A";

      return;
    }

    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail = "PARMS:";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.EabReportSend.RptDetail = "Perform Updates . . . . . . . : " + local
      .ProcessUpdatesInd.Flag;
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.EabReportSend.RptDetail = "Process 718B's. . . . . . . . : " + local
      .Process718BInd.Flag;
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.EabReportSend.RptDetail = "Process REIP Payments . . . . : " + local
      .ProcessReipPymntsInd.Flag;
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.EabReportSend.RptDetail =
      "ACTION      OBLIGOR     SUPP-PRSN   COLL-DATE       COLL-AMT  ORG-PGM  NEW-PGM  M/A  ERROR-TEXT";
      
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    if (!ReadCollectionAdjustmentReason())
    {
      ExitState = "FN0000_COLL_ADJUST_REASON_NF_RB";

      return;
    }

    if (!ReadCashReceiptDetailStatus())
    {
      ExitState = "FN0072_CASH_RCPT_DTL_STAT_NF_RB";

      return;
    }

    foreach(var item in ReadCsePersonCollection())
    {
      ++local.ReadCnt.Count;
      local.Original.Code = entities.ExistingCollection.ProgramAppliedTo;
      local.CollAmt.Number112 = entities.ExistingCollection.Amount;
      UseCabFormat112AmtField2();
      local.CollDate.Date = entities.ExistingCollection.CollectionDt;
      UseCabFormatDate();
      local.New1.Code = "N/A";

      if (AsChar(local.ProcessReipPymntsInd.Flag) == 'Y')
      {
        // : Process all REIP Collections.
      }
      else if (ReadCashReceiptType())
      {
        if (entities.ExistingCashReceiptType.SystemGeneratedIdentifier == 2 || entities
          .ExistingCashReceiptType.SystemGeneratedIdentifier == 7)
        {
          continue;
        }
      }
      else
      {
        ++local.ErrorCnt.Count;
        local.EabReportSend.RptDetail = "**ERROR***" + "  " + entities
          .ExistingObligor1.Number + "  " + entities
          .ExistingSupported1.Number + "  " + local.CollDateTxt.Text10 + "  " +
          local.CollAmtTxt.Text12 + "     " + local.Original.Code + "      " + local
          .New1.Code + "    " + entities
          .ExistingCollection.DistributionMethod + "  " + "CASH RECEIPT TYPE NOT FOUND";
          
        UseCabErrorReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        continue;
      }

      if (ReadObligationTypeObligation())
      {
        if (AsChar(local.Process718BInd.Flag) == 'Y')
        {
          // : Process all Collections applied to 718B's.
        }
        else if (entities.ExistingObligationType.SystemGeneratedIdentifier == 18
          )
        {
          continue;
        }
      }
      else
      {
        ++local.ErrorCnt.Count;
        local.EabReportSend.RptDetail = "**ERROR***" + "  " + entities
          .ExistingObligor1.Number + "  " + entities
          .ExistingSupported1.Number + "  " + local.CollDateTxt.Text10 + "  " +
          local.CollAmtTxt.Text12 + "     " + local.Original.Code + "      " + local
          .New1.Code + "    " + entities
          .ExistingCollection.DistributionMethod + "  " + "OB TYPE &/OR OBLIGATION NOT FOUND";
          
        UseCabErrorReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        continue;
      }

      if (!ReadCsePerson())
      {
        ++local.ErrorCnt.Count;
        local.EabReportSend.RptDetail = "**ERROR***" + "  " + entities
          .ExistingObligor1.Number + "  " + entities
          .ExistingSupported1.Number + "  " + local.CollDateTxt.Text10 + "  " +
          local.CollAmtTxt.Text12 + "     " + local.Original.Code + "      " + local
          .New1.Code + "    " + entities
          .ExistingCollection.DistributionMethod + "  " + "SUPPORTED PERSON NOT FOUND";
          
        UseCabErrorReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        continue;
      }

      if (!ReadDebtDetailDebt())
      {
        ++local.ErrorCnt.Count;
        local.EabReportSend.RptDetail = "**ERROR***" + "  " + entities
          .ExistingObligor1.Number + "  " + entities
          .ExistingSupported1.Number + "  " + local.CollDateTxt.Text10 + "  " +
          local.CollAmtTxt.Text12 + "     " + local.Original.Code + "      " + local
          .New1.Code + "    " + entities
          .ExistingCollection.DistributionMethod + "  " + "DEBT DETAIL NOT FOUND";
          
        UseCabErrorReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        continue;
      }

      local.Collection.Date = entities.ExistingCollection.CollectionDt;
      UseFnDeterminePgmForDebtDetail();

      if (Equal(entities.ExistingCollection.ProgramAppliedTo, local.New1.Code))
      {
        continue;
      }

      // : Read the Support Person Account to Prepare for the Update.
      if (!ReadSupported())
      {
        ++local.ErrorCnt.Count;
        local.EabReportSend.RptDetail = "**ERROR***" + "  " + entities
          .ExistingObligor1.Number + "  " + entities
          .ExistingSupported1.Number + "  " + local.CollDateTxt.Text10 + "  " +
          local.CollAmtTxt.Text12 + "     " + local.Original.Code + "      " + local
          .New1.Code + "    " + entities
          .ExistingCollection.DistributionMethod + "  " + "SUPPORTED PERSON ACCOUNT NOT FOUND";
          
        UseCabErrorReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        continue;
      }

      // : Incorrect Collection Applied To Program - Print Detail Line.
      ++local.UpdateCnt.Count;

      if (AsChar(entities.ExistingCollection.DistributionMethod) == 'M')
      {
        local.EabReportSend.RptDetail = "NO UPDATE " + "  " + entities
          .ExistingObligor1.Number + "  " + entities
          .ExistingSupported1.Number + "  " + local.CollDateTxt.Text10 + "  " +
          local.CollAmtTxt.Text12 + "     " + local.Original.Code + "      " + local
          .New1.Code + "    " + entities
          .ExistingCollection.DistributionMethod + "  " + "MANUALLY DISTRIBUTED COLLECTION";
          
      }
      else if (AsChar(local.ProcessUpdatesInd.Flag) == 'Y')
      {
        local.EabReportSend.RptDetail = "UPDATE    " + "  " + entities
          .ExistingObligor1.Number + "  " + entities
          .ExistingSupported1.Number + "  " + local.CollDateTxt.Text10 + "  " +
          local.CollAmtTxt.Text12 + "     " + local.Original.Code + "      " + local
          .New1.Code + "    " + entities
          .ExistingCollection.DistributionMethod + "  " + "SUCCESSFULLY UPDATED";
          
      }
      else
      {
        local.EabReportSend.RptDetail = "NO UPDATE " + "  " + entities
          .ExistingObligor1.Number + "  " + entities
          .ExistingSupported1.Number + "  " + local.CollDateTxt.Text10 + "  " +
          local.CollAmtTxt.Text12 + "     " + local.Original.Code + "      " + local
          .New1.Code + "    " + entities
          .ExistingCollection.DistributionMethod + "  " + "NO UPDATE";
      }

      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      // : Count & Sum by Program Applied to Code.
      switch(TrimEnd(entities.ExistingCollection.ProgramAppliedTo))
      {
        case "AF":
          ++local.Af.Count;
          local.Af.TotalCurrency += entities.ExistingCollection.Amount;

          break;
        case "AFI":
          ++local.Afi.Count;
          local.Afi.TotalCurrency += entities.ExistingCollection.Amount;

          break;
        case "FC":
          ++local.Fc.Count;
          local.Fc.TotalCurrency += entities.ExistingCollection.Amount;

          break;
        case "FCI":
          ++local.Fci.Count;
          local.Fci.TotalCurrency += entities.ExistingCollection.Amount;

          break;
        case "NA":
          ++local.Na.Count;
          local.Na.TotalCurrency += entities.ExistingCollection.Amount;

          break;
        case "NAI":
          ++local.Nai.Count;
          local.Nai.TotalCurrency += entities.ExistingCollection.Amount;

          break;
        case "NC":
          ++local.Nc.Count;
          local.Nc.TotalCurrency += entities.ExistingCollection.Amount;

          break;
        case "NF":
          ++local.Nf.Count;
          local.Nf.TotalCurrency += entities.ExistingCollection.Amount;

          break;
        default:
          break;
      }

      // : Perform Update - Update Trigger for Process Retro Program Changes.
      if (AsChar(local.ProcessUpdatesInd.Flag) == 'Y')
      {
        if (AsChar(entities.ExistingObligationType.Classification) == 'A')
        {
          if (!Lt(entities.ExistingDebtDetail.DueDt,
            entities.ExistingCollection.CollectionDt))
          {
            local.Tmp.Date = entities.ExistingCollection.CollectionDt;
          }
          else
          {
            local.Tmp.Date = entities.ExistingDebtDetail.DueDt;
          }
        }
        else if (!Lt(entities.ExistingDebtDetail.CoveredPrdStartDt,
          entities.ExistingCollection.CollectionDt))
        {
          local.Tmp.Date = entities.ExistingCollection.CollectionDt;
        }
        else
        {
          local.Tmp.Date = entities.ExistingDebtDetail.CoveredPrdStartDt;
        }

        if (Lt(local.Null1.Date, entities.ExistingSupported2.PgmChgEffectiveDate))
          
        {
          if (!Lt(local.Tmp.Date,
            entities.ExistingSupported2.PgmChgEffectiveDate))
          {
            continue;
          }
        }

        try
        {
          UpdateSupported();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_SUPP_PRSN_ACCT_NU_RB";

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

    UseCabTextnum10();
    local.EabReportSend.RptDetail = "Read Count . . . . . . . . . . . . : " + local
      .TmpCount.Text9;
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    UseCabTextnum11();
    local.EabReportSend.RptDetail = "Update Count . . . . . . . . . . . : " + local
      .TmpCount.Text9;
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    UseCabTextnum12();
    local.EabReportSend.RptDetail = "Error Count. . . . . . . . . . . . : " + local
      .TmpCount.Text9;
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    UseCabTextnum1();
    local.TotAmt.Number112 = local.Af.TotalCurrency;
    UseCabFormat112AmtField1();
    local.EabReportSend.RptDetail = "AF. . . . . . : " + local
      .TmpCount.Text9 + " - " + local.TotAmtTxt.Text12;
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    UseCabTextnum2();
    local.TotAmt.Number112 = local.Afi.TotalCurrency;
    UseCabFormat112AmtField1();
    local.EabReportSend.RptDetail = "AFI . . . . . : " + local
      .TmpCount.Text9 + " - " + local.TotAmtTxt.Text12;
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    UseCabTextnum3();
    local.TotAmt.Number112 = local.Fc.TotalCurrency;
    UseCabFormat112AmtField1();
    local.EabReportSend.RptDetail = "FC. . . . . . : " + local
      .TmpCount.Text9 + " - " + local.TotAmtTxt.Text12;
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    UseCabTextnum4();
    local.TotAmt.Number112 = local.Fci.TotalCurrency;
    UseCabFormat112AmtField1();
    local.EabReportSend.RptDetail = "FCI . . . . . : " + local
      .TmpCount.Text9 + " - " + local.TotAmtTxt.Text12;
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    UseCabTextnum5();
    local.TotAmt.Number112 = local.Na.TotalCurrency;
    UseCabFormat112AmtField1();
    local.EabReportSend.RptDetail = "NA. . . . . . : " + local
      .TmpCount.Text9 + " - " + local.TotAmtTxt.Text12;
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    UseCabTextnum6();
    local.TotAmt.Number112 = local.Nai.TotalCurrency;
    UseCabFormat112AmtField1();
    local.EabReportSend.RptDetail = "NAI . . . . . : " + local
      .TmpCount.Text9 + " - " + local.TotAmtTxt.Text12;
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    UseCabTextnum7();
    local.TotAmt.Number112 = local.Nc.TotalCurrency;
    UseCabFormat112AmtField1();
    local.EabReportSend.RptDetail = "NC. . . . . . : " + local
      .TmpCount.Text9 + " - " + local.TotAmtTxt.Text12;
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    UseCabTextnum8();
    local.TotAmt.Number112 = local.Nf.TotalCurrency;
    UseCabFormat112AmtField1();
    local.EabReportSend.RptDetail = "NF. . . . . . : " + local
      .TmpCount.Text9 + " - " + local.TotAmtTxt.Text12;
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.EabFileHandling.Action = "CLOSE";

    // **********************************************************
    // CLOSE OUTPUT CONTROL REPORT 98
    // **********************************************************
    UseCabControlReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    // **********************************************************
    // CLOSE OUTPUT ERROR REPORT 99
    // **********************************************************
    UseCabErrorReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_ERROR_RPT_A";
    }
  }

  private static void MoveDebtDetail(DebtDetail source, DebtDetail target)
  {
    target.DueDt = source.DueDt;
    target.CoveredPrdStartDt = source.CoveredPrdStartDt;
    target.CoveredPrdEndDt = source.CoveredPrdEndDt;
    target.PreconversionProgramCode = source.PreconversionProgramCode;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Classification = source.Classification;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport3()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabFormat112AmtField1()
  {
    var useImport = new CabFormat112AmtField.Import();
    var useExport = new CabFormat112AmtField.Export();

    useImport.Import112AmountField.Number112 = local.TotAmt.Number112;

    Call(CabFormat112AmtField.Execute, useImport, useExport);

    local.TotAmtTxt.Text12 = useExport.Formatted112AmtField.Text12;
  }

  private void UseCabFormat112AmtField2()
  {
    var useImport = new CabFormat112AmtField.Import();
    var useExport = new CabFormat112AmtField.Export();

    useImport.Import112AmountField.Number112 = local.CollAmt.Number112;

    Call(CabFormat112AmtField.Execute, useImport, useExport);

    local.CollAmtTxt.Text12 = useExport.Formatted112AmtField.Text12;
  }

  private void UseCabFormatDate()
  {
    var useImport = new CabFormatDate.Import();
    var useExport = new CabFormatDate.Export();

    useImport.Date.Date = local.CollDate.Date;

    Call(CabFormatDate.Execute, useImport, useExport);

    local.CollDateTxt.Text10 = useExport.FormattedDate.Text10;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseCabTextnum1()
  {
    var useImport = new CabTextnum9.Import();
    var useExport = new CabTextnum9.Export();

    useImport.Number.Count = local.Af.Count;

    Call(CabTextnum9.Execute, useImport, useExport);

    local.TmpCount.Text9 = useExport.TextNumber.Text9;
  }

  private void UseCabTextnum2()
  {
    var useImport = new CabTextnum9.Import();
    var useExport = new CabTextnum9.Export();

    useImport.Number.Count = local.Afi.Count;

    Call(CabTextnum9.Execute, useImport, useExport);

    local.TmpCount.Text9 = useExport.TextNumber.Text9;
  }

  private void UseCabTextnum3()
  {
    var useImport = new CabTextnum9.Import();
    var useExport = new CabTextnum9.Export();

    useImport.Number.Count = local.Fc.Count;

    Call(CabTextnum9.Execute, useImport, useExport);

    local.TmpCount.Text9 = useExport.TextNumber.Text9;
  }

  private void UseCabTextnum4()
  {
    var useImport = new CabTextnum9.Import();
    var useExport = new CabTextnum9.Export();

    useImport.Number.Count = local.Fci.Count;

    Call(CabTextnum9.Execute, useImport, useExport);

    local.TmpCount.Text9 = useExport.TextNumber.Text9;
  }

  private void UseCabTextnum5()
  {
    var useImport = new CabTextnum9.Import();
    var useExport = new CabTextnum9.Export();

    useImport.Number.Count = local.Na.Count;

    Call(CabTextnum9.Execute, useImport, useExport);

    local.TmpCount.Text9 = useExport.TextNumber.Text9;
  }

  private void UseCabTextnum6()
  {
    var useImport = new CabTextnum9.Import();
    var useExport = new CabTextnum9.Export();

    useImport.Number.Count = local.Nai.Count;

    Call(CabTextnum9.Execute, useImport, useExport);

    local.TmpCount.Text9 = useExport.TextNumber.Text9;
  }

  private void UseCabTextnum7()
  {
    var useImport = new CabTextnum9.Import();
    var useExport = new CabTextnum9.Export();

    useImport.Number.Count = local.Nc.Count;

    Call(CabTextnum9.Execute, useImport, useExport);

    local.TmpCount.Text9 = useExport.TextNumber.Text9;
  }

  private void UseCabTextnum8()
  {
    var useImport = new CabTextnum9.Import();
    var useExport = new CabTextnum9.Export();

    useImport.Number.Count = local.Nf.Count;

    Call(CabTextnum9.Execute, useImport, useExport);

    local.TmpCount.Text9 = useExport.TextNumber.Text9;
  }

  private void UseCabTextnum10()
  {
    var useImport = new CabTextnum9.Import();
    var useExport = new CabTextnum9.Export();

    useImport.Number.Count = local.ReadCnt.Count;

    Call(CabTextnum9.Execute, useImport, useExport);

    local.TmpCount.Text9 = useExport.TextNumber.Text9;
  }

  private void UseCabTextnum11()
  {
    var useImport = new CabTextnum9.Import();
    var useExport = new CabTextnum9.Export();

    useImport.Number.Count = local.UpdateCnt.Count;

    Call(CabTextnum9.Execute, useImport, useExport);

    local.TmpCount.Text9 = useExport.TextNumber.Text9;
  }

  private void UseCabTextnum12()
  {
    var useImport = new CabTextnum9.Import();
    var useExport = new CabTextnum9.Export();

    useImport.Number.Count = local.ErrorCnt.Count;

    Call(CabTextnum9.Execute, useImport, useExport);

    local.TmpCount.Text9 = useExport.TextNumber.Text9;
  }

  private void UseFnDeterminePgmForDebtDetail()
  {
    var useImport = new FnDeterminePgmForDebtDetail.Import();
    var useExport = new FnDeterminePgmForDebtDetail.Export();

    useImport.SupportedPerson.Number = entities.ExistingSupported1.Number;
    MoveObligationType(entities.ExistingObligationType, useImport.ObligationType);
      
    useImport.Obligation.OrderTypeCode =
      entities.ExistingObligation.OrderTypeCode;
    MoveDebtDetail(entities.ExistingDebtDetail, useImport.DebtDetail);
    useImport.HardcodedAccruing.Classification =
      local.HardcodedAccruingClass.Classification;
    useImport.Collection.Date = local.Collection.Date;

    Call(FnDeterminePgmForDebtDetail.Execute, useImport, useExport);

    local.New1.Code = useExport.Program.Code;
  }

  private void UseFnExtGetParmsThruJclSysin()
  {
    var useImport = new FnExtGetParmsThruJclSysin.Import();
    var useExport = new FnExtGetParmsThruJclSysin.Export();

    useExport.External.NumericReturnCode = local.External.NumericReturnCode;
    useExport.ProgramProcessingInfo.ParameterList =
      local.ProgramProcessingInfo.ParameterList;

    Call(FnExtGetParmsThruJclSysin.Execute, useImport, useExport);

    local.External.NumericReturnCode = useExport.External.NumericReturnCode;
    local.ProgramProcessingInfo.ParameterList =
      useExport.ProgramProcessingInfo.ParameterList;
  }

  private bool ReadCashReceiptDetailStatus()
  {
    entities.Released.Populated = false;

    return Read("ReadCashReceiptDetailStatus",
      null,
      (db, reader) =>
      {
        entities.Released.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Released.Populated = true;
      });
  }

  private bool ReadCashReceiptType()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCollection.Populated);
    entities.ExistingCashReceiptType.Populated = false;

    return Read("ReadCashReceiptType",
      (db, command) =>
      {
        db.SetInt32(command, "crtypeId", entities.ExistingCollection.CrtType);
      },
      (db, reader) =>
      {
        entities.ExistingCashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptType.Populated = true;
      });
  }

  private bool ReadCollectionAdjustmentReason()
  {
    entities.ExistingCollectionAdjustmentReason.Populated = false;

    return Read("ReadCollectionAdjustmentReason",
      null,
      (db, reader) =>
      {
        entities.ExistingCollectionAdjustmentReason.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCollectionAdjustmentReason.Populated = true;
      });
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCollection.Populated);
    entities.ExistingSupported1.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.ExistingCollection.OtyId);
        db.SetString(command, "obTrnTyp", entities.ExistingCollection.OtrType);
        db.SetInt32(command, "obTrnId", entities.ExistingCollection.OtrId);
        db.SetString(command, "cpaType", entities.ExistingCollection.CpaType);
        db.
          SetString(command, "cspNumber", entities.ExistingCollection.CspNumber);
          
        db.
          SetInt32(command, "obgGeneratedId", entities.ExistingCollection.ObgId);
          
      },
      (db, reader) =>
      {
        entities.ExistingSupported1.Number = db.GetString(reader, 0);
        entities.ExistingSupported1.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCsePersonCollection()
  {
    entities.ExistingCollection.Populated = false;
    entities.ExistingObligor1.Populated = false;

    return ReadEach("ReadCsePersonCollection",
      null,
      (db, reader) =>
      {
        entities.ExistingObligor1.Number = db.GetString(reader, 0);
        entities.ExistingCollection.CspNumber = db.GetString(reader, 0);
        entities.ExistingCollection.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingCollection.AppliedToCode = db.GetString(reader, 2);
        entities.ExistingCollection.CollectionDt = db.GetDate(reader, 3);
        entities.ExistingCollection.AdjustedInd =
          db.GetNullableString(reader, 4);
        entities.ExistingCollection.CrtType = db.GetInt32(reader, 5);
        entities.ExistingCollection.CstId = db.GetInt32(reader, 6);
        entities.ExistingCollection.CrvId = db.GetInt32(reader, 7);
        entities.ExistingCollection.CrdId = db.GetInt32(reader, 8);
        entities.ExistingCollection.ObgId = db.GetInt32(reader, 9);
        entities.ExistingCollection.CpaType = db.GetString(reader, 10);
        entities.ExistingCollection.OtrId = db.GetInt32(reader, 11);
        entities.ExistingCollection.OtrType = db.GetString(reader, 12);
        entities.ExistingCollection.OtyId = db.GetInt32(reader, 13);
        entities.ExistingCollection.CollectionAdjustmentDt =
          db.GetDate(reader, 14);
        entities.ExistingCollection.CollectionAdjProcessDate =
          db.GetDate(reader, 15);
        entities.ExistingCollection.LastUpdatedBy =
          db.GetNullableString(reader, 16);
        entities.ExistingCollection.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 17);
        entities.ExistingCollection.Amount = db.GetDecimal(reader, 18);
        entities.ExistingCollection.DistributionMethod =
          db.GetString(reader, 19);
        entities.ExistingCollection.ProgramAppliedTo = db.GetString(reader, 20);
        entities.ExistingCollection.CollectionAdjustmentReasonTxt =
          db.GetNullableString(reader, 21);
        entities.ExistingCollection.Populated = true;
        entities.ExistingObligor1.Populated = true;

        return true;
      });
  }

  private bool ReadDebtDetailDebt()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCollection.Populated);
    entities.ExistingDebt.Populated = false;
    entities.ExistingDebtDetail.Populated = false;

    return Read("ReadDebtDetailDebt",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.ExistingCollection.OtyId);
        db.SetString(command, "obTrnTyp", entities.ExistingCollection.OtrType);
        db.SetInt32(command, "obTrnId", entities.ExistingCollection.OtrId);
        db.SetString(command, "cpaType", entities.ExistingCollection.CpaType);
        db.
          SetString(command, "cspNumber", entities.ExistingCollection.CspNumber);
          
        db.
          SetInt32(command, "obgGeneratedId", entities.ExistingCollection.ObgId);
          
      },
      (db, reader) =>
      {
        entities.ExistingDebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingDebt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingDebtDetail.CspNumber = db.GetString(reader, 1);
        entities.ExistingDebt.CspNumber = db.GetString(reader, 1);
        entities.ExistingDebtDetail.CpaType = db.GetString(reader, 2);
        entities.ExistingDebt.CpaType = db.GetString(reader, 2);
        entities.ExistingDebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.ExistingDebt.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ExistingDebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.ExistingDebt.OtyType = db.GetInt32(reader, 4);
        entities.ExistingDebtDetail.OtrType = db.GetString(reader, 5);
        entities.ExistingDebt.Type1 = db.GetString(reader, 5);
        entities.ExistingDebtDetail.DueDt = db.GetDate(reader, 6);
        entities.ExistingDebtDetail.BalanceDueAmt = db.GetDecimal(reader, 7);
        entities.ExistingDebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 8);
        entities.ExistingDebtDetail.RetiredDt = db.GetNullableDate(reader, 9);
        entities.ExistingDebtDetail.CoveredPrdStartDt =
          db.GetNullableDate(reader, 10);
        entities.ExistingDebtDetail.CoveredPrdEndDt =
          db.GetNullableDate(reader, 11);
        entities.ExistingDebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 12);
        entities.ExistingDebtDetail.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 13);
        entities.ExistingDebtDetail.LastUpdatedBy =
          db.GetNullableString(reader, 14);
        entities.ExistingDebt.CreatedBy = db.GetString(reader, 15);
        entities.ExistingDebt.LastUpdatedBy = db.GetNullableString(reader, 16);
        entities.ExistingDebt.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 17);
        entities.ExistingDebt.DebtType = db.GetString(reader, 18);
        entities.ExistingDebt.CspSupNumber = db.GetNullableString(reader, 19);
        entities.ExistingDebt.CpaSupType = db.GetNullableString(reader, 20);
        entities.ExistingDebt.Populated = true;
        entities.ExistingDebtDetail.Populated = true;
      });
  }

  private bool ReadObligationTypeObligation()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCollection.Populated);
    entities.ExistingObligationType.Populated = false;
    entities.ExistingObligation.Populated = false;

    return Read("ReadObligationTypeObligation",
      (db, command) =>
      {
        db.
          SetInt32(command, "dtyGeneratedId", entities.ExistingCollection.OtyId);
          
        db.SetInt32(command, "obId", entities.ExistingCollection.ObgId);
        db.
          SetString(command, "cspNumber", entities.ExistingCollection.CspNumber);
          
        db.SetString(command, "cpaType", entities.ExistingCollection.CpaType);
      },
      (db, reader) =>
      {
        entities.ExistingObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingObligation.DtyGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingObligationType.Classification =
          db.GetString(reader, 1);
        entities.ExistingObligationType.SupportedPersonReqInd =
          db.GetString(reader, 2);
        entities.ExistingObligation.CpaType = db.GetString(reader, 3);
        entities.ExistingObligation.CspNumber = db.GetString(reader, 4);
        entities.ExistingObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 5);
        entities.ExistingObligation.OrderTypeCode = db.GetString(reader, 6);
        entities.ExistingObligationType.Populated = true;
        entities.ExistingObligation.Populated = true;
      });
  }

  private bool ReadSupported()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingDebt.Populated);
    entities.ExistingSupported2.Populated = false;

    return Read("ReadSupported",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", entities.ExistingSupported1.Number);
        db.SetString(command, "type", entities.ExistingDebt.CpaSupType ?? "");
        db.SetString(
          command, "cspNumber2", entities.ExistingDebt.CspSupNumber ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingSupported2.CspNumber = db.GetString(reader, 0);
        entities.ExistingSupported2.Type1 = db.GetString(reader, 1);
        entities.ExistingSupported2.LastUpdatedBy =
          db.GetNullableString(reader, 2);
        entities.ExistingSupported2.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 3);
        entities.ExistingSupported2.PgmChgEffectiveDate =
          db.GetNullableDate(reader, 4);
        entities.ExistingSupported2.Populated = true;
      });
  }

  private void UpdateSupported()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingSupported2.Populated);

    var lastUpdatedBy = local.UserId.Text8;
    var lastUpdatedTmst = local.Current.Timestamp;
    var pgmChgEffectiveDate = local.Tmp.Date;

    entities.ExistingSupported2.Populated = false;
    Update("UpdateSupported",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableDate(command, "recompBalFromDt", pgmChgEffectiveDate);
        db.
          SetString(command, "cspNumber", entities.ExistingSupported2.CspNumber);
          
        db.SetString(command, "type", entities.ExistingSupported2.Type1);
      });

    entities.ExistingSupported2.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingSupported2.LastUpdatedTmst = lastUpdatedTmst;
    entities.ExistingSupported2.PgmChgEffectiveDate = pgmChgEffectiveDate;
    entities.ExistingSupported2.Populated = true;
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
    /// A value of Tmp.
    /// </summary>
    [JsonPropertyName("tmp")]
    public DateWorkArea Tmp
    {
      get => tmp ??= new();
      set => tmp = value;
    }

    /// <summary>
    /// A value of TotAmtTxt.
    /// </summary>
    [JsonPropertyName("totAmtTxt")]
    public WorkArea TotAmtTxt
    {
      get => totAmtTxt ??= new();
      set => totAmtTxt = value;
    }

    /// <summary>
    /// A value of TotAmt.
    /// </summary>
    [JsonPropertyName("totAmt")]
    public NumericWorkSet TotAmt
    {
      get => totAmt ??= new();
      set => totAmt = value;
    }

    /// <summary>
    /// A value of TmpCount.
    /// </summary>
    [JsonPropertyName("tmpCount")]
    public WorkArea TmpCount
    {
      get => tmpCount ??= new();
      set => tmpCount = value;
    }

    /// <summary>
    /// A value of Original.
    /// </summary>
    [JsonPropertyName("original")]
    public Program Original
    {
      get => original ??= new();
      set => original = value;
    }

    /// <summary>
    /// A value of CollAmtTxt.
    /// </summary>
    [JsonPropertyName("collAmtTxt")]
    public WorkArea CollAmtTxt
    {
      get => collAmtTxt ??= new();
      set => collAmtTxt = value;
    }

    /// <summary>
    /// A value of CollAmt.
    /// </summary>
    [JsonPropertyName("collAmt")]
    public NumericWorkSet CollAmt
    {
      get => collAmt ??= new();
      set => collAmt = value;
    }

    /// <summary>
    /// A value of CollDate.
    /// </summary>
    [JsonPropertyName("collDate")]
    public DateWorkArea CollDate
    {
      get => collDate ??= new();
      set => collDate = value;
    }

    /// <summary>
    /// A value of CollDateTxt.
    /// </summary>
    [JsonPropertyName("collDateTxt")]
    public WorkArea CollDateTxt
    {
      get => collDateTxt ??= new();
      set => collDateTxt = value;
    }

    /// <summary>
    /// A value of ProcessReipPymntsInd.
    /// </summary>
    [JsonPropertyName("processReipPymntsInd")]
    public Common ProcessReipPymntsInd
    {
      get => processReipPymntsInd ??= new();
      set => processReipPymntsInd = value;
    }

    /// <summary>
    /// A value of Process718BInd.
    /// </summary>
    [JsonPropertyName("process718BInd")]
    public Common Process718BInd
    {
      get => process718BInd ??= new();
      set => process718BInd = value;
    }

    /// <summary>
    /// A value of MaxDiscontinue.
    /// </summary>
    [JsonPropertyName("maxDiscontinue")]
    public DateWorkArea MaxDiscontinue
    {
      get => maxDiscontinue ??= new();
      set => maxDiscontinue = value;
    }

    /// <summary>
    /// A value of Af.
    /// </summary>
    [JsonPropertyName("af")]
    public Common Af
    {
      get => af ??= new();
      set => af = value;
    }

    /// <summary>
    /// A value of Afi.
    /// </summary>
    [JsonPropertyName("afi")]
    public Common Afi
    {
      get => afi ??= new();
      set => afi = value;
    }

    /// <summary>
    /// A value of Fc.
    /// </summary>
    [JsonPropertyName("fc")]
    public Common Fc
    {
      get => fc ??= new();
      set => fc = value;
    }

    /// <summary>
    /// A value of Fci.
    /// </summary>
    [JsonPropertyName("fci")]
    public Common Fci
    {
      get => fci ??= new();
      set => fci = value;
    }

    /// <summary>
    /// A value of Na.
    /// </summary>
    [JsonPropertyName("na")]
    public Common Na
    {
      get => na ??= new();
      set => na = value;
    }

    /// <summary>
    /// A value of Nai.
    /// </summary>
    [JsonPropertyName("nai")]
    public Common Nai
    {
      get => nai ??= new();
      set => nai = value;
    }

    /// <summary>
    /// A value of Nc.
    /// </summary>
    [JsonPropertyName("nc")]
    public Common Nc
    {
      get => nc ??= new();
      set => nc = value;
    }

    /// <summary>
    /// A value of Nf.
    /// </summary>
    [JsonPropertyName("nf")]
    public Common Nf
    {
      get => nf ??= new();
      set => nf = value;
    }

    /// <summary>
    /// A value of ForUpdate.
    /// </summary>
    [JsonPropertyName("forUpdate")]
    public DebtDetail ForUpdate
    {
      get => forUpdate ??= new();
      set => forUpdate = value;
    }

    /// <summary>
    /// A value of HardcodedAccruingClass.
    /// </summary>
    [JsonPropertyName("hardcodedAccruingClass")]
    public ObligationType HardcodedAccruingClass
    {
      get => hardcodedAccruingClass ??= new();
      set => hardcodedAccruingClass = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public Program New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public DateWorkArea Collection
    {
      get => collection ??= new();
      set => collection = value;
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
    /// A value of ProcessUpdatesInd.
    /// </summary>
    [JsonPropertyName("processUpdatesInd")]
    public Common ProcessUpdatesInd
    {
      get => processUpdatesInd ??= new();
      set => processUpdatesInd = value;
    }

    /// <summary>
    /// A value of ReadCnt.
    /// </summary>
    [JsonPropertyName("readCnt")]
    public Common ReadCnt
    {
      get => readCnt ??= new();
      set => readCnt = value;
    }

    /// <summary>
    /// A value of UpdateCnt.
    /// </summary>
    [JsonPropertyName("updateCnt")]
    public Common UpdateCnt
    {
      get => updateCnt ??= new();
      set => updateCnt = value;
    }

    /// <summary>
    /// A value of ErrorCnt.
    /// </summary>
    [JsonPropertyName("errorCnt")]
    public Common ErrorCnt
    {
      get => errorCnt ??= new();
      set => errorCnt = value;
    }

    /// <summary>
    /// A value of UserId.
    /// </summary>
    [JsonPropertyName("userId")]
    public TextWorkArea UserId
    {
      get => userId ??= new();
      set => userId = value;
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
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    public External External
    {
      get => external ??= new();
      set => external = value;
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

    private DateWorkArea tmp;
    private WorkArea totAmtTxt;
    private NumericWorkSet totAmt;
    private WorkArea tmpCount;
    private Program original;
    private WorkArea collAmtTxt;
    private NumericWorkSet collAmt;
    private DateWorkArea collDate;
    private WorkArea collDateTxt;
    private Common processReipPymntsInd;
    private Common process718BInd;
    private DateWorkArea maxDiscontinue;
    private Common af;
    private Common afi;
    private Common fc;
    private Common fci;
    private Common na;
    private Common nai;
    private Common nc;
    private Common nf;
    private DebtDetail forUpdate;
    private ObligationType hardcodedAccruingClass;
    private Program new1;
    private DateWorkArea collection;
    private DateWorkArea null1;
    private Common processUpdatesInd;
    private Common readCnt;
    private Common updateCnt;
    private Common errorCnt;
    private TextWorkArea userId;
    private DateWorkArea current;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private External external;
    private ProgramProcessingInfo programProcessingInfo;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingCashReceipt.
    /// </summary>
    [JsonPropertyName("existingCashReceipt")]
    public CashReceipt ExistingCashReceipt
    {
      get => existingCashReceipt ??= new();
      set => existingCashReceipt = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptType.
    /// </summary>
    [JsonPropertyName("existingCashReceiptType")]
    public CashReceiptType ExistingCashReceiptType
    {
      get => existingCashReceiptType ??= new();
      set => existingCashReceiptType = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("existingCashReceiptDetailStatus")]
    public CashReceiptDetailStatus ExistingCashReceiptDetailStatus
    {
      get => existingCashReceiptDetailStatus ??= new();
      set => existingCashReceiptDetailStatus = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptDetailStatHistory.
    /// </summary>
    [JsonPropertyName("existingCashReceiptDetailStatHistory")]
    public CashReceiptDetailStatHistory ExistingCashReceiptDetailStatHistory
    {
      get => existingCashReceiptDetailStatHistory ??= new();
      set => existingCashReceiptDetailStatHistory = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public CashReceiptDetailStatHistory New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of Released.
    /// </summary>
    [JsonPropertyName("released")]
    public CashReceiptDetailStatus Released
    {
      get => released ??= new();
      set => released = value;
    }

    /// <summary>
    /// A value of ExistingCollectionAdjustmentReason.
    /// </summary>
    [JsonPropertyName("existingCollectionAdjustmentReason")]
    public CollectionAdjustmentReason ExistingCollectionAdjustmentReason
    {
      get => existingCollectionAdjustmentReason ??= new();
      set => existingCollectionAdjustmentReason = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("existingCashReceiptDetail")]
    public CashReceiptDetail ExistingCashReceiptDetail
    {
      get => existingCashReceiptDetail ??= new();
      set => existingCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of ExistingCollection.
    /// </summary>
    [JsonPropertyName("existingCollection")]
    public Collection ExistingCollection
    {
      get => existingCollection ??= new();
      set => existingCollection = value;
    }

    /// <summary>
    /// A value of ExistingObligor1.
    /// </summary>
    [JsonPropertyName("existingObligor1")]
    public CsePerson ExistingObligor1
    {
      get => existingObligor1 ??= new();
      set => existingObligor1 = value;
    }

    /// <summary>
    /// A value of ExistingObligor2.
    /// </summary>
    [JsonPropertyName("existingObligor2")]
    public CsePersonAccount ExistingObligor2
    {
      get => existingObligor2 ??= new();
      set => existingObligor2 = value;
    }

    /// <summary>
    /// A value of ExistingSupported1.
    /// </summary>
    [JsonPropertyName("existingSupported1")]
    public CsePerson ExistingSupported1
    {
      get => existingSupported1 ??= new();
      set => existingSupported1 = value;
    }

    /// <summary>
    /// A value of ExistingSupported2.
    /// </summary>
    [JsonPropertyName("existingSupported2")]
    public CsePersonAccount ExistingSupported2
    {
      get => existingSupported2 ??= new();
      set => existingSupported2 = value;
    }

    /// <summary>
    /// A value of ExistingObligationType.
    /// </summary>
    [JsonPropertyName("existingObligationType")]
    public ObligationType ExistingObligationType
    {
      get => existingObligationType ??= new();
      set => existingObligationType = value;
    }

    /// <summary>
    /// A value of ExistingObligation.
    /// </summary>
    [JsonPropertyName("existingObligation")]
    public Obligation ExistingObligation
    {
      get => existingObligation ??= new();
      set => existingObligation = value;
    }

    /// <summary>
    /// A value of ExistingDebt.
    /// </summary>
    [JsonPropertyName("existingDebt")]
    public ObligationTransaction ExistingDebt
    {
      get => existingDebt ??= new();
      set => existingDebt = value;
    }

    /// <summary>
    /// A value of ExistingDebtDetail.
    /// </summary>
    [JsonPropertyName("existingDebtDetail")]
    public DebtDetail ExistingDebtDetail
    {
      get => existingDebtDetail ??= new();
      set => existingDebtDetail = value;
    }

    private CashReceipt existingCashReceipt;
    private CashReceiptType existingCashReceiptType;
    private CashReceiptDetailStatus existingCashReceiptDetailStatus;
    private CashReceiptDetailStatHistory existingCashReceiptDetailStatHistory;
    private CashReceiptDetailStatHistory new1;
    private CashReceiptDetailStatus released;
    private CollectionAdjustmentReason existingCollectionAdjustmentReason;
    private CashReceiptDetail existingCashReceiptDetail;
    private Collection existingCollection;
    private CsePerson existingObligor1;
    private CsePersonAccount existingObligor2;
    private CsePerson existingSupported1;
    private CsePersonAccount existingSupported2;
    private ObligationType existingObligationType;
    private Obligation existingObligation;
    private ObligationTransaction existingDebt;
    private DebtDetail existingDebtDetail;
  }
#endregion
}
