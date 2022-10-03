// Program: FN_BFXN_CALCULATE_SFY_TOTALS, ID: 1625329388, model: 746.
// Short name: SWE01981
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_BFXN_CALCULATE_SFY_TOTALS.
/// </summary>
[Serializable]
public partial class FnBfxnCalculateSfyTotals: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_BFXN_CALCULATE_SFY_TOTALS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnBfxnCalculateSfyTotals(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnBfxnCalculateSfyTotals.
  /// </summary>
  public FnBfxnCalculateSfyTotals(IContext context, Import import, Export export)
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
    local.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);
    local.HardcodedFType.SequentialIdentifier = 3;
    local.HardcodedNaDprProgram.ProgramState = "NA";
    local.HardcodedPa.ProgramState = "PA";
    local.HardcodedTa.ProgramState = "TA";
    local.HardcodedCa.ProgramState = "CA";
    local.HardcodedUd.ProgramState = "UD";
    local.HardcodedUp.ProgramState = "UP";
    local.HardcodedUk.ProgramState = "UK";
    UseFnHardcodedCashReceipting();
    UseFnHardcodedDebtDistribution();
    UseFnBuildProgramValues();
    local.MaximumDiscontinue.Date = new DateTime(2099, 12, 31);
    local.SfyEndCashReceiptDetail.CollectionDate = import.SfyEnd.Date;

    local.OfPgms.Index = 0;
    local.OfPgms.Clear();

    foreach(var item in ReadProgram())
    {
      local.OfPgms.Update.OfPgms1.Assign(entities.Existing);
      local.OfPgms.Next();
    }

    local.SfyEndDateWorkArea.Timestamp =
      Timestamp(NumberToString(Year(import.SfyEnd.Date), 12, 4) + "-" + NumberToString
      (Month(import.SfyEnd.Date), 14, 2) + "-" + NumberToString
      (Day(import.SfyEnd.Date), 14, 2) + "-23.59.59.999999");
    local.CursorOpen.Flag = "N";

    foreach(var item in ReadDebtDetailCsePersonObligationTransactionObligation())
      
    {
      if (AsChar(local.CursorOpen.Flag) == 'N')
      {
        local.CursorOpen.Flag = "Y";

        for(local.Common.Count = 1; local.Common.Count <= 3; ++
          local.Common.Count)
        {
          if (local.Common.Count == 1)
          {
            local.EabReportSend.RptDetail = "SFY " + NumberToString
              (import.SfyEnd.Year, 12, 4) + " cursor opened " + NumberToString
              (Time(Now()).Hours, 14, 2) + ":" + NumberToString
              (Time(Now()).Minutes, 14, 2);
          }
          else
          {
            local.EabReportSend.RptDetail = "";
          }

          local.EabFileHandling.Action = "WRITE";
          UseCabControlReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            // -- Write to the error report.
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "(01) Error Writing Control Report...  Returned Status = " + local
              .EabFileHandling.Status;
            UseCabErrorReport();

            // -- Set Abort exit state and escape...
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }
        }
      }

      if (!Equal(entities.CsePerson.Number, local.Previous.Number))
      {
        ++local.CspSinceCommit.Count;

        if (local.CspSinceCommit.Count >= import
          .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault())
        {
          // -- Checkpoint.
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (!export.Export1.CheckSize())
            {
              break;
            }

            local.Ocse157Data.LineNumber =
              NumberToString(export.Export1.Index + 1, 13, 3);

            if (ReadOcse157Data())
            {
              try
              {
                UpdateOcse157Data();
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "FN0000_OCSE34_NU";

                    break;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "OCSE34_PV";

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
                CreateOcse157Data();
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "OCSE34_AE";

                    break;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "OCSE34_PV";

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

          export.Export1.CheckIndex();

          // -------------------------------------------------------------------------------------
          //  Checkpoint Info...
          // 	Position  Description
          // 	--------  
          // ---------------------------------------------------------
          // 	001-010   Last Person Number Processed
          //         010-010   blank
          //         011-014   Year being processed
          // 	
          // -------------------------------------------------------------------------------------
          local.ProgramCheckpointRestart.RestartInd = "Y";
          local.ProgramCheckpointRestart.RestartInfo = local.Previous.Number + " " +
            NumberToString(import.SfyStart.Year, 12, 4);
          UseUpdateCheckpointRstAndCommit();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = "Error committing.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          local.CspSinceCommit.Count = 0;
        }

        local.Previous.Number = entities.CsePerson.Number;
        UseFnBuildProgramHistoryTable();
      }
      else
      {
      }

      if (!ReadObligationType())
      {
        continue;
      }

      if (ReadCsePerson())
      {
        local.Supported.Number = entities.SupportedCsePerson.Number;
      }
      else
      {
        local.Supported.Number = "";
      }

      local.Tmp.Index = 0;
      local.Tmp.Clear();

      for(local.Null1.Index = 0; local.Null1.Index < local.Null1.Count; ++
        local.Null1.Index)
      {
        if (local.Tmp.IsFull)
        {
          break;
        }

        local.Tmp.Next();
      }

      for(local.PgmHist.Index = 0; local.PgmHist.Index < local.PgmHist.Count; ++
        local.PgmHist.Index)
      {
        if (Equal(local.PgmHist.Item.PgmHistSuppPrsn.Number,
          local.Supported.Number))
        {
          local.Tmp.Index = 0;
          local.Tmp.Clear();

          for(local.PgmHist.Item.PgmHistDtl.Index = 0; local
            .PgmHist.Item.PgmHistDtl.Index < local
            .PgmHist.Item.PgmHistDtl.Count; ++
            local.PgmHist.Item.PgmHistDtl.Index)
          {
            if (local.Tmp.IsFull)
            {
              break;
            }

            local.Tmp.Update.TmpProgram.Assign(
              local.PgmHist.Item.PgmHistDtl.Item.PgmHistDtlProgram);
            MovePersonProgram(local.PgmHist.Item.PgmHistDtl.Item.
              PgmHistDtlPersonProgram, local.Tmp.Update.TmpPersonProgram);
            local.Tmp.Next();
          }
        }
      }

      // --Make the obligation a secondary obligation so that URA won't change 
      // the derived program code.
      local.Obligation.Assign(entities.Obligation);
      local.Obligation.PrimarySecondaryCode =
        local.HardcodedSecondary.PrimarySecondaryCode ?? "";
      UseFnDeterminePgmForDbtDist2();

      if (Equal(local.Program.Code, "AF"))
      {
        export.Export1.Index =
          entities.ObligationType.SystemGeneratedIdentifier - 1;
        export.Export1.CheckSize();

        export.Export1.Update.G.TotalCurrency =
          export.Export1.Item.G.TotalCurrency + entities
          .DebtDetail.BalanceDueAmt;

        foreach(var item1 in ReadCollection())
        {
          if (!Lt(local.SfyEndDateWorkArea.Timestamp,
            entities.Collection.CreatedTmst))
          {
            export.Export1.Update.G.TotalCurrency =
              export.Export1.Item.G.TotalCurrency - entities.Collection.Amount;
          }
          else
          {
            export.Export1.Update.G.TotalCurrency =
              export.Export1.Item.G.TotalCurrency + entities.Collection.Amount;
          }
        }

        foreach(var item1 in ReadObligationTransactionRlnObligationTransaction())
          
        {
          switch(AsChar(entities.Adjustment.DebtAdjustmentType))
          {
            case 'I':
              export.Export1.Update.G.TotalCurrency =
                export.Export1.Item.G.TotalCurrency - entities
                .Adjustment.Amount;

              break;
            case 'D':
              export.Export1.Update.G.TotalCurrency =
                export.Export1.Item.G.TotalCurrency + entities
                .Adjustment.Amount;

              break;
            default:
              break;
          }
        }
      }
    }

    // -- Checkpoint.
    for(export.Export1.Index = 0; export.Export1.Index < export.Export1.Count; ++
      export.Export1.Index)
    {
      if (!export.Export1.CheckSize())
      {
        break;
      }

      local.Ocse157Data.LineNumber =
        NumberToString(export.Export1.Index + 1, 13, 3);

      if (ReadOcse157Data())
      {
        try
        {
          UpdateOcse157Data();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_OCSE34_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "OCSE34_PV";

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
          CreateOcse157Data();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "OCSE34_AE";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "OCSE34_PV";

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

    export.Export1.CheckIndex();

    // -------------------------------------------------------------------------------------
    //  Checkpoint Info...
    // 	Position  Description
    // 	--------  
    // ---------------------------------------------------------
    // 	001-010   Last Person Number Processed
    //         010-010   blank
    //         011-014   Year being processed
    // 	
    // -------------------------------------------------------------------------------------
    local.ProgramCheckpointRestart.RestartInd = "Y";
    local.ProgramCheckpointRestart.RestartInfo = "0000000000" + " " + NumberToString
      (import.SfyStart.Year + 1, 12, 4);
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error committing.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveCashReceiptType(CashReceiptType source,
    CashReceiptType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.CategoryIndicator = source.CategoryIndicator;
  }

  private static void MoveDebtDetail(DebtDetail source, DebtDetail target)
  {
    target.DueDt = source.DueDt;
    target.CoveredPrdStartDt = source.CoveredPrdStartDt;
    target.CoveredPrdEndDt = source.CoveredPrdEndDt;
    target.PreconversionProgramCode = source.PreconversionProgramCode;
  }

  private static void MoveObligation(Obligation source, Obligation target)
  {
    target.PrimarySecondaryCode = source.PrimarySecondaryCode;
    target.OrderTypeCode = source.OrderTypeCode;
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Classification = source.Classification;
  }

  private static void MoveOfPgms(Local.OfPgmsGroup source,
    FnBuildProgramHistoryTable.Import.OfPgmsGroup target)
  {
    target.OfPgms1.Assign(source.OfPgms1);
  }

  private static void MovePersonProgram(PersonProgram source,
    PersonProgram target)
  {
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
  }

  private static void MovePgmHist(FnBuildProgramHistoryTable.Export.
    PgmHistGroup source, Local.PgmHistGroup target)
  {
    target.PgmHistSuppPrsn.Number = source.PgmHistSuppPrsn.Number;
    source.PgmHistDtl.CopyTo(target.PgmHistDtl, MovePgmHistDtl);
    target.TafInd.Flag = source.TafInd.Flag;
  }

  private static void MovePgmHistDtl(FnBuildProgramHistoryTable.Export.
    PgmHistDtlGroup source, Local.PgmHistDtlGroup target)
  {
    target.PgmHistDtlProgram.Assign(source.PgmHistDtlProgram);
    MovePersonProgram(source.PgmHistDtlPersonProgram,
      target.PgmHistDtlPersonProgram);
  }

  private static void MoveProgram(Program source, Program target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveTmpToPgmHistDtl(Local.TmpGroup source,
    FnDeterminePgmForDbtDist2.Import.PgmHistDtlGroup target)
  {
    target.PgmHistDtlProgram.Assign(source.TmpProgram);
    MovePersonProgram(source.TmpPersonProgram, target.PgmHistDtlPersonProgram);
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
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

  private void UseFnBuildProgramHistoryTable()
  {
    var useImport = new FnBuildProgramHistoryTable.Import();
    var useExport = new FnBuildProgramHistoryTable.Export();

    useImport.Obligor.Number = entities.CsePerson.Number;
    useImport.CashReceiptDetail.CollectionDate =
      local.SfyEndCashReceiptDetail.CollectionDate;
    useImport.MaximumDiscontinue.Date = local.MaximumDiscontinue.Date;
    local.OfPgms.CopyTo(useImport.OfPgms, MoveOfPgms);

    Call(FnBuildProgramHistoryTable.Execute, useImport, useExport);

    useExport.PgmHist.CopyTo(local.PgmHist, MovePgmHist);
  }

  private void UseFnBuildProgramValues()
  {
    var useImport = new FnBuildProgramValues.Import();
    var useExport = new FnBuildProgramValues.Export();

    Call(FnBuildProgramValues.Execute, useImport, useExport);

    local.HardcodedAf.Assign(useExport.HardcodedAf);
    local.HardcodedAfi.Assign(useExport.HardcodedAfi);
    local.HardcodedFc.Assign(useExport.HardcodedFc);
    local.HardcodedFci.Assign(useExport.HardcodedFci);
    local.HardcodedNaProgram.Assign(useExport.HardcodedNa);
    local.HardcodedNai.Assign(useExport.HardcodedNai);
    local.HardcodedNc.Assign(useExport.HardcodedNc);
    local.HardcodedNf.Assign(useExport.HardcodedNf);
    local.HardcodedMai.Assign(useExport.HardcodedMai);
  }

  private void UseFnDeterminePgmForDbtDist2()
  {
    var useImport = new FnDeterminePgmForDbtDist2.Import();
    var useExport = new FnDeterminePgmForDbtDist2.Export();

    MoveObligationType(entities.ObligationType, useImport.ObligationType);
    MoveDebtDetail(entities.DebtDetail, useImport.DebtDetail);
    useImport.Collection.Date = import.SfyEnd.Date;
    useImport.SuppPrsn.Number = local.Supported.Number;
    MoveObligation(local.Obligation, useImport.Obligation);
    local.Tmp.CopyTo(useImport.PgmHistDtl, MoveTmpToPgmHistDtl);
    useImport.HardcodedAccruingClass.Classification =
      local.HardcodedAccruingClass.Classification;
    useImport.HardcodedSecondary.PrimarySecondaryCode =
      local.HardcodedSecondary.PrimarySecondaryCode;
    useImport.Hardcoded718B.SystemGeneratedIdentifier =
      local.Hardcoded718BType.SystemGeneratedIdentifier;
    useImport.HardcodedMsType.SystemGeneratedIdentifier =
      local.HardcodedMsType.SystemGeneratedIdentifier;
    useImport.HardcodedMcType.SystemGeneratedIdentifier =
      local.HardcodedMcType.SystemGeneratedIdentifier;
    useImport.HardcodedMjType.SystemGeneratedIdentifier =
      local.HardcodedMjType.SystemGeneratedIdentifier;
    useImport.HardcodedAf.Assign(local.HardcodedAf);
    useImport.HardcodedAfi.Assign(local.HardcodedAfi);
    useImport.HardcodedFc.Assign(local.HardcodedFc);
    useImport.HardcodedFci.Assign(local.HardcodedFci);
    useImport.HardcodedNaProgram.Assign(local.HardcodedNaProgram);
    useImport.HardcodedNai.Assign(local.HardcodedNai);
    useImport.HardcodedNc.Assign(local.HardcodedNc);
    useImport.HardcodedNf.Assign(local.HardcodedNf);
    useImport.HardcodedMai.Assign(local.HardcodedMai);
    useImport.HardcodedNaDprProgram.ProgramState =
      local.HardcodedNaDprProgram.ProgramState;
    useImport.HardcodedPa.ProgramState = local.HardcodedPa.ProgramState;
    useImport.HardcodedTa.ProgramState = local.HardcodedTa.ProgramState;
    useImport.HardcodedCa.ProgramState = local.HardcodedCa.ProgramState;
    useImport.HardcodedUd.ProgramState = local.HardcodedUd.ProgramState;
    useImport.HardcodedUp.ProgramState = local.HardcodedUp.ProgramState;
    useImport.HardcodedUk.ProgramState = local.HardcodedUk.ProgramState;
    useImport.HardcodedFFedType.SequentialIdentifier =
      local.HardcodedFType.SequentialIdentifier;

    Call(FnDeterminePgmForDbtDist2.Execute, useImport, useExport);

    local.DprProgram.ProgramState = useExport.DprProgram.ProgramState;
    MoveProgram(useExport.Program, local.Program);
  }

  private void UseFnHardcodedCashReceipting()
  {
    var useImport = new FnHardcodedCashReceipting.Import();
    var useExport = new FnHardcodedCashReceipting.Export();

    Call(FnHardcodedCashReceipting.Execute, useImport, useExport);

    local.HardcodedCashType.CategoryIndicator =
      useExport.CrtCategory.CrtCatCash.CategoryIndicator;
    MoveCashReceiptType(useExport.CrtSystemId.CrtIdFcrtRec,
      local.HardcodedFcourtPmt);
    MoveCashReceiptType(useExport.CrtSystemId.CrtIdFdirPmt,
      local.HardcodedFdirPmt);
    local.HardcodedReleased.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdReleased.SystemGeneratedIdentifier;
    local.HardcodedRefunded.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdRefunded.SystemGeneratedIdentifier;
    local.HardcodedDistributed.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdDistributed.SystemGeneratedIdentifier;
    local.HardcodedSuspended.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdSuspended.SystemGeneratedIdentifier;
    local.HardcodedRecorded.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdRecorded.SystemGeneratedIdentifier;
  }

  private void UseFnHardcodedDebtDistribution()
  {
    var useImport = new FnHardcodedDebtDistribution.Import();
    var useExport = new FnHardcodedDebtDistribution.Export();

    Call(FnHardcodedDebtDistribution.Execute, useImport, useExport);

    local.HardcodedPriSec.SequentialGeneratedIdentifier =
      useExport.OrrPrimarySecondary.SequentialGeneratedIdentifier;
    local.HardcodedAccrual.SystemGeneratedIdentifier =
      useExport.OtrrAccrual.SystemGeneratedIdentifier;
    local.HardcodedArrears.AppliedToCode = useExport.CollArrears.AppliedToCode;
    local.HardcodedCurrent.AppliedToCode = useExport.CollCurrent.AppliedToCode;
    local.HardcodedDeactivatedStat.Code = useExport.DdshDeactivedStatus.Code;
    local.HardcodedActiveStatus.Code = useExport.DdshActiveStatus.Code;
    local.HardcodedJointSeveralObligation.PrimarySecondaryCode =
      useExport.ObligJointSeveralConcurrent.PrimarySecondaryCode;
    local.HardcodedJointSeveralObligationRlnRsn.SequentialGeneratedIdentifier =
      useExport.OrrJointSeveral.SequentialGeneratedIdentifier;
    local.HardcodedWrongAcct.SystemGeneratedIdentifier =
      useExport.CarPostedToTheWrongAcct.SystemGeneratedIdentifier;
    local.HardcodedVolClass.Classification =
      useExport.OtCVoluntaryClassification.Classification;
    local.HardcodedAccruingClass.Classification =
      useExport.OtCAccruingClassification.Classification;
    local.HardcodedSecondary.PrimarySecondaryCode =
      useExport.ObligSecondaryConcurrent.PrimarySecondaryCode;
    local.HardcodedPrimary.PrimarySecondaryCode =
      useExport.ObligPrimaryConcurrent.PrimarySecondaryCode;
    local.HardcodedCsType.SystemGeneratedIdentifier =
      useExport.OtChildSupport.SystemGeneratedIdentifier;
    local.Hardcoded718BType.SystemGeneratedIdentifier =
      useExport.Ot718BUraJudgement.SystemGeneratedIdentifier;
    local.HardcodedMsType.SystemGeneratedIdentifier =
      useExport.OtMedicalSupport.SystemGeneratedIdentifier;
    local.HardcodedMcType.SystemGeneratedIdentifier =
      useExport.OtMedicalSupportForCash.SystemGeneratedIdentifier;
    local.HardcodedMjType.SystemGeneratedIdentifier =
      useExport.OtMedicalJudgement.SystemGeneratedIdentifier;
  }

  private void UseUpdateCheckpointRstAndCommit()
  {
    var useImport = new UpdateCheckpointRstAndCommit.Import();
    var useExport = new UpdateCheckpointRstAndCommit.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdateCheckpointRstAndCommit.Execute, useImport, useExport);
  }

  private void CreateOcse157Data()
  {
    var fiscalYear = import.SfyEnd.Year;
    var runNumber = 1;
    var lineNumber = local.Ocse157Data.LineNumber ?? "";
    var createdTimestamp = Now();
    var number = (long?)export.Export1.Item.G.TotalCurrency;

    entities.Ocse157Data.Populated = false;
    Update("CreateOcse157Data",
      (db, command) =>
      {
        db.SetNullableInt32(command, "fiscalYear", fiscalYear);
        db.SetNullableInt32(command, "runNumber", runNumber);
        db.SetNullableString(command, "lineNumber", lineNumber);
        db.SetNullableString(command, "column0", "");
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableInt64(command, "number", number);
      });

    entities.Ocse157Data.FiscalYear = fiscalYear;
    entities.Ocse157Data.RunNumber = runNumber;
    entities.Ocse157Data.LineNumber = lineNumber;
    entities.Ocse157Data.Column = "";
    entities.Ocse157Data.CreatedTimestamp = createdTimestamp;
    entities.Ocse157Data.Number = number;
    entities.Ocse157Data.Populated = true;
  }

  private IEnumerable<bool> ReadCollection()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection",
      (db, command) =>
      {
        db.SetInt32(command, "otyId", entities.ObligationTransaction.OtyType);
        db.SetString(command, "otrType", entities.ObligationTransaction.Type1);
        db.SetInt32(
          command, "otrId",
          entities.ObligationTransaction.SystemGeneratedIdentifier);
        db.
          SetString(command, "cpaType", entities.ObligationTransaction.CpaType);
          
        db.SetString(
          command, "cspNumber", entities.ObligationTransaction.CspNumber);
        db.SetInt32(
          command, "obgId", entities.ObligationTransaction.ObgGeneratedId);
        db.SetDateTime(
          command, "createdTmst",
          local.SfyEndDateWorkArea.Timestamp.GetValueOrDefault());
        db.
          SetDate(command, "collAdjDt", import.SfyEnd.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.CollectionDt = db.GetDate(reader, 1);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 2);
        entities.Collection.ConcurrentInd = db.GetString(reader, 3);
        entities.Collection.CrtType = db.GetInt32(reader, 4);
        entities.Collection.CstId = db.GetInt32(reader, 5);
        entities.Collection.CrvId = db.GetInt32(reader, 6);
        entities.Collection.CrdId = db.GetInt32(reader, 7);
        entities.Collection.ObgId = db.GetInt32(reader, 8);
        entities.Collection.CspNumber = db.GetString(reader, 9);
        entities.Collection.CpaType = db.GetString(reader, 10);
        entities.Collection.OtrId = db.GetInt32(reader, 11);
        entities.Collection.OtrType = db.GetString(reader, 12);
        entities.Collection.OtyId = db.GetInt32(reader, 13);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 14);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 15);
        entities.Collection.Amount = db.GetDecimal(reader, 16);
        entities.Collection.Populated = true;

        return true;
      });
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.SupportedCsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(
          command, "numb", entities.ObligationTransaction.CspSupNumber ?? "");
      },
      (db, reader) =>
      {
        entities.SupportedCsePerson.Number = db.GetString(reader, 0);
        entities.SupportedCsePerson.Populated = true;
      });
  }

  private IEnumerable<bool>
    ReadDebtDetailCsePersonObligationTransactionObligation()
  {
    entities.CsePerson.Populated = false;
    entities.Obligation.Populated = false;
    entities.ObligationTransaction.Populated = false;
    entities.DebtDetail.Populated = false;

    return ReadEach("ReadDebtDetailCsePersonObligationTransactionObligation",
      (db, command) =>
      {
        db.SetDate(command, "dueDt", import.SfyEnd.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "retiredDt", local.Null2.Date.GetValueOrDefault());
        db.SetString(command, "numb", import.Restart.Number);
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 1);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 2);
        entities.Obligation.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 4);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 5);
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 7);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 8);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 9);
        entities.DebtDetail.CoveredPrdEndDt = db.GetNullableDate(reader, 10);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 11);
        entities.CsePerson.Number = db.GetString(reader, 12);
        entities.CsePerson.Type1 = db.GetString(reader, 13);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 14);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 15);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 16);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 17);
        entities.CsePerson.Populated = true;
        entities.Obligation.Populated = true;
        entities.ObligationTransaction.Populated = true;
        entities.DebtDetail.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadObligationTransactionRlnObligationTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.ObligationTransactionRln.Populated = false;
    entities.Adjustment.Populated = false;

    return ReadEach("ReadObligationTransactionRlnObligationTransaction",
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
        db.SetDate(command, "debAdjDt", import.SfyEnd.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligationTransactionRln.OnrGeneratedId =
          db.GetInt32(reader, 0);
        entities.ObligationTransactionRln.OtrType = db.GetString(reader, 1);
        entities.Adjustment.Type1 = db.GetString(reader, 1);
        entities.ObligationTransactionRln.OtrGeneratedId =
          db.GetInt32(reader, 2);
        entities.Adjustment.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.ObligationTransactionRln.CpaType = db.GetString(reader, 3);
        entities.Adjustment.CpaType = db.GetString(reader, 3);
        entities.ObligationTransactionRln.CspNumber = db.GetString(reader, 4);
        entities.Adjustment.CspNumber = db.GetString(reader, 4);
        entities.ObligationTransactionRln.ObgGeneratedId =
          db.GetInt32(reader, 5);
        entities.Adjustment.ObgGeneratedId = db.GetInt32(reader, 5);
        entities.ObligationTransactionRln.OtrPType = db.GetString(reader, 6);
        entities.ObligationTransactionRln.OtrPGeneratedId =
          db.GetInt32(reader, 7);
        entities.ObligationTransactionRln.CpaPType = db.GetString(reader, 8);
        entities.ObligationTransactionRln.CspPNumber = db.GetString(reader, 9);
        entities.ObligationTransactionRln.ObgPGeneratedId =
          db.GetInt32(reader, 10);
        entities.ObligationTransactionRln.SystemGeneratedIdentifier =
          db.GetInt32(reader, 11);
        entities.ObligationTransactionRln.OtyTypePrimary =
          db.GetInt32(reader, 12);
        entities.ObligationTransactionRln.OtyTypeSecondary =
          db.GetInt32(reader, 13);
        entities.Adjustment.OtyType = db.GetInt32(reader, 13);
        entities.Adjustment.Amount = db.GetDecimal(reader, 14);
        entities.Adjustment.DebtAdjustmentType = db.GetString(reader, 15);
        entities.Adjustment.DebtAdjustmentDt = db.GetDate(reader, 16);
        entities.Adjustment.CspSupNumber = db.GetNullableString(reader, 17);
        entities.Adjustment.CpaSupType = db.GetNullableString(reader, 18);
        entities.Adjustment.ReasonCode = db.GetString(reader, 19);
        entities.ObligationTransactionRln.Populated = true;
        entities.Adjustment.Populated = true;

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
        entities.ObligationType.Classification = db.GetString(reader, 2);
        entities.ObligationType.Populated = true;
      });
  }

  private bool ReadOcse157Data()
  {
    entities.Ocse157Data.Populated = false;

    return Read("ReadOcse157Data",
      (db, command) =>
      {
        db.SetNullableInt32(command, "fiscalYear", import.SfyEnd.Year);
        db.SetNullableString(
          command, "lineNumber", local.Ocse157Data.LineNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Ocse157Data.FiscalYear = db.GetNullableInt32(reader, 0);
        entities.Ocse157Data.RunNumber = db.GetNullableInt32(reader, 1);
        entities.Ocse157Data.LineNumber = db.GetNullableString(reader, 2);
        entities.Ocse157Data.Column = db.GetNullableString(reader, 3);
        entities.Ocse157Data.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.Ocse157Data.Number = db.GetNullableInt64(reader, 5);
        entities.Ocse157Data.Populated = true;
      });
  }

  private IEnumerable<bool> ReadProgram()
  {
    return ReadEach("ReadProgram",
      null,
      (db, reader) =>
      {
        if (local.OfPgms.IsFull)
        {
          return false;
        }

        entities.Existing.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Existing.Code = db.GetString(reader, 1);
        entities.Existing.InterstateIndicator = db.GetString(reader, 2);
        entities.Existing.Populated = true;

        return true;
      });
  }

  private void UpdateOcse157Data()
  {
    var number = (long?)export.Export1.Item.G.TotalCurrency;

    entities.Ocse157Data.Populated = false;
    Update("UpdateOcse157Data",
      (db, command) =>
      {
        db.SetNullableInt64(command, "number", number);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.Ocse157Data.CreatedTimestamp.GetValueOrDefault());
      });

    entities.Ocse157Data.Number = number;
    entities.Ocse157Data.Populated = true;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    /// <summary>
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public CsePerson Restart
    {
      get => restart ??= new();
      set => restart = value;
    }

    /// <summary>
    /// A value of SfyEnd.
    /// </summary>
    [JsonPropertyName("sfyEnd")]
    public DateWorkArea SfyEnd
    {
      get => sfyEnd ??= new();
      set => sfyEnd = value;
    }

    /// <summary>
    /// A value of SfyStart.
    /// </summary>
    [JsonPropertyName("sfyStart")]
    public DateWorkArea SfyStart
    {
      get => sfyStart ??= new();
      set => sfyStart = value;
    }

    private ProgramCheckpointRestart programCheckpointRestart;
    private CsePerson restart;
    private DateWorkArea sfyEnd;
    private DateWorkArea sfyStart;
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
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public Common G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 23;

      private Common g;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 =>
      export1 ??= new(ExportGroup.Capacity, 0);

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

    private Array<ExportGroup> export1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A TmpGroup group.</summary>
    [Serializable]
    public class TmpGroup
    {
      /// <summary>
      /// A value of TmpProgram.
      /// </summary>
      [JsonPropertyName("tmpProgram")]
      public Program TmpProgram
      {
        get => tmpProgram ??= new();
        set => tmpProgram = value;
      }

      /// <summary>
      /// A value of TmpPersonProgram.
      /// </summary>
      [JsonPropertyName("tmpPersonProgram")]
      public PersonProgram TmpPersonProgram
      {
        get => tmpPersonProgram ??= new();
        set => tmpPersonProgram = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 500;

      private Program tmpProgram;
      private PersonProgram tmpPersonProgram;
    }

    /// <summary>A NullGroup group.</summary>
    [Serializable]
    public class NullGroup
    {
      /// <summary>
      /// A value of Null2.
      /// </summary>
      [JsonPropertyName("null2")]
      public TextWorkArea Null2
      {
        get => null2 ??= new();
        set => null2 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 500;

      private TextWorkArea null2;
    }

    /// <summary>A PgmHistGroup group.</summary>
    [Serializable]
    public class PgmHistGroup
    {
      /// <summary>
      /// A value of PgmHistSuppPrsn.
      /// </summary>
      [JsonPropertyName("pgmHistSuppPrsn")]
      public CsePerson PgmHistSuppPrsn
      {
        get => pgmHistSuppPrsn ??= new();
        set => pgmHistSuppPrsn = value;
      }

      /// <summary>
      /// Gets a value of PgmHistDtl.
      /// </summary>
      [JsonIgnore]
      public Array<PgmHistDtlGroup> PgmHistDtl => pgmHistDtl ??= new(
        PgmHistDtlGroup.Capacity);

      /// <summary>
      /// Gets a value of PgmHistDtl for json serialization.
      /// </summary>
      [JsonPropertyName("pgmHistDtl")]
      [Computed]
      public IList<PgmHistDtlGroup> PgmHistDtl_Json
      {
        get => pgmHistDtl;
        set => PgmHistDtl.Assign(value);
      }

      /// <summary>
      /// A value of TafInd.
      /// </summary>
      [JsonPropertyName("tafInd")]
      public Common TafInd
      {
        get => tafInd ??= new();
        set => tafInd = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private CsePerson pgmHistSuppPrsn;
      private Array<PgmHistDtlGroup> pgmHistDtl;
      private Common tafInd;
    }

    /// <summary>A PgmHistDtlGroup group.</summary>
    [Serializable]
    public class PgmHistDtlGroup
    {
      /// <summary>
      /// A value of PgmHistDtlProgram.
      /// </summary>
      [JsonPropertyName("pgmHistDtlProgram")]
      public Program PgmHistDtlProgram
      {
        get => pgmHistDtlProgram ??= new();
        set => pgmHistDtlProgram = value;
      }

      /// <summary>
      /// A value of PgmHistDtlPersonProgram.
      /// </summary>
      [JsonPropertyName("pgmHistDtlPersonProgram")]
      public PersonProgram PgmHistDtlPersonProgram
      {
        get => pgmHistDtlPersonProgram ??= new();
        set => pgmHistDtlPersonProgram = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 500;

      private Program pgmHistDtlProgram;
      private PersonProgram pgmHistDtlPersonProgram;
    }

    /// <summary>A OfPgmsGroup group.</summary>
    [Serializable]
    public class OfPgmsGroup
    {
      /// <summary>
      /// A value of OfPgms1.
      /// </summary>
      [JsonPropertyName("ofPgms1")]
      public Program OfPgms1
      {
        get => ofPgms1 ??= new();
        set => ofPgms1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 200;

      private Program ofPgms1;
    }

    /// <summary>
    /// A value of SfyEndDateWorkArea.
    /// </summary>
    [JsonPropertyName("sfyEndDateWorkArea")]
    public DateWorkArea SfyEndDateWorkArea
    {
      get => sfyEndDateWorkArea ??= new();
      set => sfyEndDateWorkArea = value;
    }

    /// <summary>
    /// A value of Null2.
    /// </summary>
    [JsonPropertyName("null2")]
    public DateWorkArea Null2
    {
      get => null2 ??= new();
      set => null2 = value;
    }

    /// <summary>
    /// A value of CursorOpen.
    /// </summary>
    [JsonPropertyName("cursorOpen")]
    public Common CursorOpen
    {
      get => cursorOpen ??= new();
      set => cursorOpen = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// Gets a value of Tmp.
    /// </summary>
    [JsonIgnore]
    public Array<TmpGroup> Tmp => tmp ??= new(TmpGroup.Capacity);

    /// <summary>
    /// Gets a value of Tmp for json serialization.
    /// </summary>
    [JsonPropertyName("tmp")]
    [Computed]
    public IList<TmpGroup> Tmp_Json
    {
      get => tmp;
      set => Tmp.Assign(value);
    }

    /// <summary>
    /// Gets a value of Null1.
    /// </summary>
    [JsonIgnore]
    public Array<NullGroup> Null1 => null1 ??= new(NullGroup.Capacity);

    /// <summary>
    /// Gets a value of Null1 for json serialization.
    /// </summary>
    [JsonPropertyName("null1")]
    [Computed]
    public IList<NullGroup> Null1_Json
    {
      get => null1;
      set => Null1.Assign(value);
    }

    /// <summary>
    /// A value of Ocse157Data.
    /// </summary>
    [JsonPropertyName("ocse157Data")]
    public Ocse157Data Ocse157Data
    {
      get => ocse157Data ??= new();
      set => ocse157Data = value;
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
    /// A value of CspSinceCommit.
    /// </summary>
    [JsonPropertyName("cspSinceCommit")]
    public Common CspSinceCommit
    {
      get => cspSinceCommit ??= new();
      set => cspSinceCommit = value;
    }

    /// <summary>
    /// A value of DprProgram.
    /// </summary>
    [JsonPropertyName("dprProgram")]
    public DprProgram DprProgram
    {
      get => dprProgram ??= new();
      set => dprProgram = value;
    }

    /// <summary>
    /// Gets a value of PgmHist.
    /// </summary>
    [JsonIgnore]
    public Array<PgmHistGroup> PgmHist =>
      pgmHist ??= new(PgmHistGroup.Capacity);

    /// <summary>
    /// Gets a value of PgmHist for json serialization.
    /// </summary>
    [JsonPropertyName("pgmHist")]
    [Computed]
    public IList<PgmHistGroup> PgmHist_Json
    {
      get => pgmHist;
      set => PgmHist.Assign(value);
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public CsePerson Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of SfyEndCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("sfyEndCashReceiptDetail")]
    public CashReceiptDetail SfyEndCashReceiptDetail
    {
      get => sfyEndCashReceiptDetail ??= new();
      set => sfyEndCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of MaximumDiscontinue.
    /// </summary>
    [JsonPropertyName("maximumDiscontinue")]
    public DateWorkArea MaximumDiscontinue
    {
      get => maximumDiscontinue ??= new();
      set => maximumDiscontinue = value;
    }

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
    /// Gets a value of OfPgms.
    /// </summary>
    [JsonIgnore]
    public Array<OfPgmsGroup> OfPgms => ofPgms ??= new(OfPgmsGroup.Capacity);

    /// <summary>
    /// Gets a value of OfPgms for json serialization.
    /// </summary>
    [JsonPropertyName("ofPgms")]
    [Computed]
    public IList<OfPgmsGroup> OfPgms_Json
    {
      get => ofPgms;
      set => OfPgms.Assign(value);
    }

    /// <summary>
    /// A value of HardcodedCashType.
    /// </summary>
    [JsonPropertyName("hardcodedCashType")]
    public CashReceiptType HardcodedCashType
    {
      get => hardcodedCashType ??= new();
      set => hardcodedCashType = value;
    }

    /// <summary>
    /// A value of HardcodedFcourtPmt.
    /// </summary>
    [JsonPropertyName("hardcodedFcourtPmt")]
    public CashReceiptType HardcodedFcourtPmt
    {
      get => hardcodedFcourtPmt ??= new();
      set => hardcodedFcourtPmt = value;
    }

    /// <summary>
    /// A value of HardcodedFdirPmt.
    /// </summary>
    [JsonPropertyName("hardcodedFdirPmt")]
    public CashReceiptType HardcodedFdirPmt
    {
      get => hardcodedFdirPmt ??= new();
      set => hardcodedFdirPmt = value;
    }

    /// <summary>
    /// A value of HardcodedReleased.
    /// </summary>
    [JsonPropertyName("hardcodedReleased")]
    public CashReceiptDetailStatus HardcodedReleased
    {
      get => hardcodedReleased ??= new();
      set => hardcodedReleased = value;
    }

    /// <summary>
    /// A value of HardcodedRefunded.
    /// </summary>
    [JsonPropertyName("hardcodedRefunded")]
    public CashReceiptDetailStatus HardcodedRefunded
    {
      get => hardcodedRefunded ??= new();
      set => hardcodedRefunded = value;
    }

    /// <summary>
    /// A value of HardcodedDistributed.
    /// </summary>
    [JsonPropertyName("hardcodedDistributed")]
    public CashReceiptDetailStatus HardcodedDistributed
    {
      get => hardcodedDistributed ??= new();
      set => hardcodedDistributed = value;
    }

    /// <summary>
    /// A value of HardcodedSuspended.
    /// </summary>
    [JsonPropertyName("hardcodedSuspended")]
    public CashReceiptDetailStatus HardcodedSuspended
    {
      get => hardcodedSuspended ??= new();
      set => hardcodedSuspended = value;
    }

    /// <summary>
    /// A value of HardcodedRecorded.
    /// </summary>
    [JsonPropertyName("hardcodedRecorded")]
    public CashReceiptDetailStatus HardcodedRecorded
    {
      get => hardcodedRecorded ??= new();
      set => hardcodedRecorded = value;
    }

    /// <summary>
    /// A value of HardcodedPriSec.
    /// </summary>
    [JsonPropertyName("hardcodedPriSec")]
    public ObligationRlnRsn HardcodedPriSec
    {
      get => hardcodedPriSec ??= new();
      set => hardcodedPriSec = value;
    }

    /// <summary>
    /// A value of HardcodedAccrual.
    /// </summary>
    [JsonPropertyName("hardcodedAccrual")]
    public ObligationTransactionRlnRsn HardcodedAccrual
    {
      get => hardcodedAccrual ??= new();
      set => hardcodedAccrual = value;
    }

    /// <summary>
    /// A value of HardcodedArrears.
    /// </summary>
    [JsonPropertyName("hardcodedArrears")]
    public Collection HardcodedArrears
    {
      get => hardcodedArrears ??= new();
      set => hardcodedArrears = value;
    }

    /// <summary>
    /// A value of HardcodedCurrent.
    /// </summary>
    [JsonPropertyName("hardcodedCurrent")]
    public Collection HardcodedCurrent
    {
      get => hardcodedCurrent ??= new();
      set => hardcodedCurrent = value;
    }

    /// <summary>
    /// A value of HardcodedDeactivatedStat.
    /// </summary>
    [JsonPropertyName("hardcodedDeactivatedStat")]
    public DebtDetailStatusHistory HardcodedDeactivatedStat
    {
      get => hardcodedDeactivatedStat ??= new();
      set => hardcodedDeactivatedStat = value;
    }

    /// <summary>
    /// A value of HardcodedActiveStatus.
    /// </summary>
    [JsonPropertyName("hardcodedActiveStatus")]
    public DebtDetailStatusHistory HardcodedActiveStatus
    {
      get => hardcodedActiveStatus ??= new();
      set => hardcodedActiveStatus = value;
    }

    /// <summary>
    /// A value of HardcodedJointSeveralObligation.
    /// </summary>
    [JsonPropertyName("hardcodedJointSeveralObligation")]
    public Obligation HardcodedJointSeveralObligation
    {
      get => hardcodedJointSeveralObligation ??= new();
      set => hardcodedJointSeveralObligation = value;
    }

    /// <summary>
    /// A value of HardcodedJointSeveralObligationRlnRsn.
    /// </summary>
    [JsonPropertyName("hardcodedJointSeveralObligationRlnRsn")]
    public ObligationRlnRsn HardcodedJointSeveralObligationRlnRsn
    {
      get => hardcodedJointSeveralObligationRlnRsn ??= new();
      set => hardcodedJointSeveralObligationRlnRsn = value;
    }

    /// <summary>
    /// A value of HardcodedWrongAcct.
    /// </summary>
    [JsonPropertyName("hardcodedWrongAcct")]
    public CollectionAdjustmentReason HardcodedWrongAcct
    {
      get => hardcodedWrongAcct ??= new();
      set => hardcodedWrongAcct = value;
    }

    /// <summary>
    /// A value of HardcodedVolClass.
    /// </summary>
    [JsonPropertyName("hardcodedVolClass")]
    public ObligationType HardcodedVolClass
    {
      get => hardcodedVolClass ??= new();
      set => hardcodedVolClass = value;
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
    /// A value of HardcodedSecondary.
    /// </summary>
    [JsonPropertyName("hardcodedSecondary")]
    public Obligation HardcodedSecondary
    {
      get => hardcodedSecondary ??= new();
      set => hardcodedSecondary = value;
    }

    /// <summary>
    /// A value of HardcodedPrimary.
    /// </summary>
    [JsonPropertyName("hardcodedPrimary")]
    public Obligation HardcodedPrimary
    {
      get => hardcodedPrimary ??= new();
      set => hardcodedPrimary = value;
    }

    /// <summary>
    /// A value of HardcodedCsType.
    /// </summary>
    [JsonPropertyName("hardcodedCsType")]
    public ObligationType HardcodedCsType
    {
      get => hardcodedCsType ??= new();
      set => hardcodedCsType = value;
    }

    /// <summary>
    /// A value of Hardcoded718BType.
    /// </summary>
    [JsonPropertyName("hardcoded718BType")]
    public ObligationType Hardcoded718BType
    {
      get => hardcoded718BType ??= new();
      set => hardcoded718BType = value;
    }

    /// <summary>
    /// A value of HardcodedMsType.
    /// </summary>
    [JsonPropertyName("hardcodedMsType")]
    public ObligationType HardcodedMsType
    {
      get => hardcodedMsType ??= new();
      set => hardcodedMsType = value;
    }

    /// <summary>
    /// A value of HardcodedMcType.
    /// </summary>
    [JsonPropertyName("hardcodedMcType")]
    public ObligationType HardcodedMcType
    {
      get => hardcodedMcType ??= new();
      set => hardcodedMcType = value;
    }

    /// <summary>
    /// A value of HardcodedMjType.
    /// </summary>
    [JsonPropertyName("hardcodedMjType")]
    public ObligationType HardcodedMjType
    {
      get => hardcodedMjType ??= new();
      set => hardcodedMjType = value;
    }

    /// <summary>
    /// A value of HardcodedAf.
    /// </summary>
    [JsonPropertyName("hardcodedAf")]
    public Program HardcodedAf
    {
      get => hardcodedAf ??= new();
      set => hardcodedAf = value;
    }

    /// <summary>
    /// A value of HardcodedAfi.
    /// </summary>
    [JsonPropertyName("hardcodedAfi")]
    public Program HardcodedAfi
    {
      get => hardcodedAfi ??= new();
      set => hardcodedAfi = value;
    }

    /// <summary>
    /// A value of HardcodedFc.
    /// </summary>
    [JsonPropertyName("hardcodedFc")]
    public Program HardcodedFc
    {
      get => hardcodedFc ??= new();
      set => hardcodedFc = value;
    }

    /// <summary>
    /// A value of HardcodedFci.
    /// </summary>
    [JsonPropertyName("hardcodedFci")]
    public Program HardcodedFci
    {
      get => hardcodedFci ??= new();
      set => hardcodedFci = value;
    }

    /// <summary>
    /// A value of HardcodedNaProgram.
    /// </summary>
    [JsonPropertyName("hardcodedNaProgram")]
    public Program HardcodedNaProgram
    {
      get => hardcodedNaProgram ??= new();
      set => hardcodedNaProgram = value;
    }

    /// <summary>
    /// A value of HardcodedNai.
    /// </summary>
    [JsonPropertyName("hardcodedNai")]
    public Program HardcodedNai
    {
      get => hardcodedNai ??= new();
      set => hardcodedNai = value;
    }

    /// <summary>
    /// A value of HardcodedNc.
    /// </summary>
    [JsonPropertyName("hardcodedNc")]
    public Program HardcodedNc
    {
      get => hardcodedNc ??= new();
      set => hardcodedNc = value;
    }

    /// <summary>
    /// A value of HardcodedNf.
    /// </summary>
    [JsonPropertyName("hardcodedNf")]
    public Program HardcodedNf
    {
      get => hardcodedNf ??= new();
      set => hardcodedNf = value;
    }

    /// <summary>
    /// A value of HardcodedMai.
    /// </summary>
    [JsonPropertyName("hardcodedMai")]
    public Program HardcodedMai
    {
      get => hardcodedMai ??= new();
      set => hardcodedMai = value;
    }

    /// <summary>
    /// A value of HardcodedNaDprProgram.
    /// </summary>
    [JsonPropertyName("hardcodedNaDprProgram")]
    public DprProgram HardcodedNaDprProgram
    {
      get => hardcodedNaDprProgram ??= new();
      set => hardcodedNaDprProgram = value;
    }

    /// <summary>
    /// A value of HardcodedPa.
    /// </summary>
    [JsonPropertyName("hardcodedPa")]
    public DprProgram HardcodedPa
    {
      get => hardcodedPa ??= new();
      set => hardcodedPa = value;
    }

    /// <summary>
    /// A value of HardcodedTa.
    /// </summary>
    [JsonPropertyName("hardcodedTa")]
    public DprProgram HardcodedTa
    {
      get => hardcodedTa ??= new();
      set => hardcodedTa = value;
    }

    /// <summary>
    /// A value of HardcodedCa.
    /// </summary>
    [JsonPropertyName("hardcodedCa")]
    public DprProgram HardcodedCa
    {
      get => hardcodedCa ??= new();
      set => hardcodedCa = value;
    }

    /// <summary>
    /// A value of HardcodedUd.
    /// </summary>
    [JsonPropertyName("hardcodedUd")]
    public DprProgram HardcodedUd
    {
      get => hardcodedUd ??= new();
      set => hardcodedUd = value;
    }

    /// <summary>
    /// A value of HardcodedUp.
    /// </summary>
    [JsonPropertyName("hardcodedUp")]
    public DprProgram HardcodedUp
    {
      get => hardcodedUp ??= new();
      set => hardcodedUp = value;
    }

    /// <summary>
    /// A value of HardcodedUk.
    /// </summary>
    [JsonPropertyName("hardcodedUk")]
    public DprProgram HardcodedUk
    {
      get => hardcodedUk ??= new();
      set => hardcodedUk = value;
    }

    /// <summary>
    /// A value of HardcodedFType.
    /// </summary>
    [JsonPropertyName("hardcodedFType")]
    public CollectionType HardcodedFType
    {
      get => hardcodedFType ??= new();
      set => hardcodedFType = value;
    }

    /// <summary>
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
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

    private DateWorkArea sfyEndDateWorkArea;
    private DateWorkArea null2;
    private Common cursorOpen;
    private CsePerson supported;
    private Obligation obligation;
    private Array<TmpGroup> tmp;
    private Array<NullGroup> null1;
    private Ocse157Data ocse157Data;
    private Common common;
    private Common cspSinceCommit;
    private DprProgram dprProgram;
    private Array<PgmHistGroup> pgmHist;
    private CsePerson previous;
    private CashReceiptDetail sfyEndCashReceiptDetail;
    private DateWorkArea maximumDiscontinue;
    private Program program;
    private Array<OfPgmsGroup> ofPgms;
    private CashReceiptType hardcodedCashType;
    private CashReceiptType hardcodedFcourtPmt;
    private CashReceiptType hardcodedFdirPmt;
    private CashReceiptDetailStatus hardcodedReleased;
    private CashReceiptDetailStatus hardcodedRefunded;
    private CashReceiptDetailStatus hardcodedDistributed;
    private CashReceiptDetailStatus hardcodedSuspended;
    private CashReceiptDetailStatus hardcodedRecorded;
    private ObligationRlnRsn hardcodedPriSec;
    private ObligationTransactionRlnRsn hardcodedAccrual;
    private Collection hardcodedArrears;
    private Collection hardcodedCurrent;
    private DebtDetailStatusHistory hardcodedDeactivatedStat;
    private DebtDetailStatusHistory hardcodedActiveStatus;
    private Obligation hardcodedJointSeveralObligation;
    private ObligationRlnRsn hardcodedJointSeveralObligationRlnRsn;
    private CollectionAdjustmentReason hardcodedWrongAcct;
    private ObligationType hardcodedVolClass;
    private ObligationType hardcodedAccruingClass;
    private Obligation hardcodedSecondary;
    private Obligation hardcodedPrimary;
    private ObligationType hardcodedCsType;
    private ObligationType hardcoded718BType;
    private ObligationType hardcodedMsType;
    private ObligationType hardcodedMcType;
    private ObligationType hardcodedMjType;
    private Program hardcodedAf;
    private Program hardcodedAfi;
    private Program hardcodedFc;
    private Program hardcodedFci;
    private Program hardcodedNaProgram;
    private Program hardcodedNai;
    private Program hardcodedNc;
    private Program hardcodedNf;
    private Program hardcodedMai;
    private DprProgram hardcodedNaDprProgram;
    private DprProgram hardcodedPa;
    private DprProgram hardcodedTa;
    private DprProgram hardcodedCa;
    private DprProgram hardcodedUd;
    private DprProgram hardcodedUp;
    private DprProgram hardcodedUk;
    private CollectionType hardcodedFType;
    private ProgramCheckpointRestart programCheckpointRestart;
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
    /// A value of ObligationTransactionRln.
    /// </summary>
    [JsonPropertyName("obligationTransactionRln")]
    public ObligationTransactionRln ObligationTransactionRln
    {
      get => obligationTransactionRln ??= new();
      set => obligationTransactionRln = value;
    }

    /// <summary>
    /// A value of Ocse157Data.
    /// </summary>
    [JsonPropertyName("ocse157Data")]
    public Ocse157Data Ocse157Data
    {
      get => ocse157Data ??= new();
      set => ocse157Data = value;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
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
    /// A value of Adjustment.
    /// </summary>
    [JsonPropertyName("adjustment")]
    public ObligationTransaction Adjustment
    {
      get => adjustment ??= new();
      set => adjustment = value;
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
    /// A value of Debt.
    /// </summary>
    [JsonPropertyName("debt")]
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
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
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public Program Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    private ObligationTransactionRln obligationTransactionRln;
    private Ocse157Data ocse157Data;
    private CsePersonAccount supportedCsePersonAccount;
    private CsePerson supportedCsePerson;
    private Collection collection;
    private ObligationType obligationType;
    private CsePerson csePerson;
    private CsePersonAccount csePersonAccount;
    private Obligation obligation;
    private ObligationTransaction adjustment;
    private ObligationTransaction obligationTransaction;
    private ObligationTransaction debt;
    private DebtDetail debtDetail;
    private Program existing;
  }
#endregion
}
