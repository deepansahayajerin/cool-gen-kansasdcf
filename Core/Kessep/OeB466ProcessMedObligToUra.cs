// Program: OE_B466_PROCESS_MED_OBLIG_TO_URA, ID: 374470285, model: 746.
// Short name: SWEE466B
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
/// A program: OE_B466_PROCESS_MED_OBLIG_TO_URA.
/// </para>
/// <para>
/// RESP:  FINANCE
/// DESC:  This PrAD distributes all of the undistributed or partially 
/// distributed cash receipts to the eligible debts, as determined by the
/// distribution policy rule.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class OeB466ProcessMedObligToUra: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B466_PROCESS_MED_OBLIG_TO_URA program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB466ProcessMedObligToUra(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB466ProcessMedObligToUra.
  /// </summary>
  public OeB466ProcessMedObligToUra(IContext context, Import import,
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
    // Initial Version :- SWSRKXD 07/13/2000
    // This utility will apply MJ debt to URA.
    // 08/09/2000 - Set URA trigger when flag is set.
    // 08/12/2000 - Do not ABEND when case is nf.
    // 09/27/2000 - Unmatch export infrastructure.
    // ----------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    UseOeB466Initialization();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ----------------------------------------------------------------
    // Initialize values to be used later in the program.
    // ----------------------------------------------------------------
    local.ServiceProviderRequired.Flag = "N";
    local.Infrastructure.SituationNumber = 0;
    local.Infrastructure.EventId = 45;
    local.Infrastructure.ProcessStatus = "Q";
    local.Infrastructure.UserId = global.UserId;
    local.Infrastructure.ReasonCode = "ADDMJASURA";
    local.Infrastructure.InitiatingStateCode = "KS";
    local.Infrastructure.BusinessObjectCd = "IMS";
    local.Infrastructure.ReferenceDate =
      local.ProgramProcessingInfo.ProcessDate;
    local.ProcessDate.Date = local.ProgramProcessingInfo.ProcessDate;
    local.EabFileHandling.Action = "WRITE";
    local.UserId.Text8 = global.UserId;
    local.Current.Timestamp = Now();
    UseFnHardcodedDebtDistribution();

    // ------------------------------------
    // Process only 'MJ' obligation_type.
    // ------------------------------------
    foreach(var item in ReadCsePersonCsePersonObligationTypeObligation())
    {
      ++local.TotRecsRead.Count;
      local.TotAmtRead.TotalCurrency += entities.ExistingDebt.Amount;
      local.DueDate.Month = Month(entities.ExistingDebtDetail.DueDt);
      local.DueDate.Year = Year(entities.ExistingDebtDetail.DueDt);

      try
      {
        UpdateDebt();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_OBLIG_TRANS_NU_RB";

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

      // -----------------------------------------------
      // Read Oblg Typ to retrieve non-key attributes.
      // ----------------------------------------------
      if (!ReadObligationType())
      {
        ExitState = "OE0000_OBLIGATION_TYPE_NF";

        break;
      }

      // -------------------------------------------------------------
      // No need to pass hardcoded accruing OT. CAB sets it when
      // not supplied.
      // -------------------------------------------------------------
      UseFnDeterminePgmForDebtDetail();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();

        if (IsEmpty(local.ExitStateWorkArea.Message))
        {
          local.EabReportSend.RptDetail =
            "Unknown error has occurred in fn_determine_pgm_for_debt_detail.";
        }
        else
        {
          local.EabReportSend.RptDetail =
            "Exit State returned from fn_determine_pgm: " + local
            .ExitStateWorkArea.Message;
        }

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

        ExitState = "ACO_NN0000_ALL_OK";

        continue;
      }

      if (!Equal(local.Program.Code, "AF") && !Equal(local.Program.Code, "FC"))
      {
        ++local.NonAfRecsSkipped.Count;
        local.TotNonAfAmtSkipped.TotalCurrency += entities.ExistingDebt.Amount;

        continue;
      }

      local.UpdateMthSummSuccessful.Flag = "N";

      if (ReadImHouseholdMbrMnthlySumImHousehold1())
      {
        try
        {
          UpdateImHouseholdMbrMnthlySum();
          ++local.TotRecsProcessed.Count;
          local.TotAmtProcessed.TotalCurrency += entities.ExistingDebt.Amount;
          local.UpdateMthSummSuccessful.Flag = "Y";
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_IM_HH_MBR_MNTH_SUM_NU_RB";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_IM_HH_MBR_MNTH_SUM_PV_RB";

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
        foreach(var item1 in ReadImHouseholdMbrMnthlySumImHousehold2())
        {
          try
          {
            UpdateImHouseholdMbrMnthlySum();
            ++local.TotRecsProcessed.Count;
            local.TotAmtProcessed.TotalCurrency += entities.ExistingDebt.Amount;
            local.UpdateMthSummSuccessful.Flag = "Y";

            goto Read;
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_IM_HH_MBR_MNTH_SUM_NU_RB";

                goto ReadEach;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_IM_HH_MBR_MNTH_SUM_PV_RB";

                goto ReadEach;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }

        ++local.TotRecsErrored.Count;
        local.TotAmtErrored.TotalCurrency += entities.ExistingDebt.Amount;

        // : Print an Error Line.
        local.EabReportSend.RptDetail = "Obligor #:" + entities
          .ExistingObligor1.Number;
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + "; Oblg #:";
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + NumberToString
          (entities.ExistingObligation.SystemGeneratedIdentifier, 13, 3);
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + "; Oblg Type: ";
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + entities
          .ObligationType.Code;
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + "; Supp Pers #: ";
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + entities
          .ExistingSuppPerson.Number;
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + "; Debt Amt:$ ";
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + NumberToString
          ((long)(entities.ExistingDebt.Amount * 100), 5, 11);
        UseCabErrorReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        // ***********************
        // Retrieve case # to create alert.
        // ***********************
        UseFnDetCaseNoAndWrkrForDbt();

        // ***********************
        // CAB sets no exit state
        // ***********************
        if (IsEmpty(local.Case1.Number))
        {
          // : Print an Error Line.
          local.EabReportSend.RptDetail =
            "Case was not found for this debt. No alert was created! " + " ";
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

          // ----------------------------------------------------------------
          // SWSRKXD 08/12/2000 - Do not ABEND when case is nf.
          // ----------------------------------------------------------------
          ExitState = "ACO_NN0000_ALL_OK";

          goto Read;
        }

        local.Infrastructure.CaseNumber = local.Case1.Number;
        local.Infrastructure.CsePersonNumber =
          entities.ExistingSuppPerson.Number;
        local.Infrastructure.Detail =
          "Please setup Medical Judgement using UHMM screen.";

        // ----------------------------------------------------------------
        // SWSRKXD 09/27/2000 - Unmatch export infrastructure.
        // ----------------------------------------------------------------
        UseSpCabCreateInfrastructure();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        local.EabReportSend.RptDetail =
          "Alert was sent to worker for CSE Case #  " + " ";
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + " " + local.Case1.Number;
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
      }

Read:

      // ----------------------------------------------
      // 08/09/00
      // Set URA trigger if flag is set.
      // ----------------------------------------------
      if (AsChar(local.SetUraTrigger.Flag) == 'Y' && AsChar
        (local.UpdateMthSummSuccessful.Flag) == 'Y')
      {
        local.FirstDayOfMsum.Date =
          StringToDate(NumberToString(
            entities.ExistingImHouseholdMbrMnthlySum.Year, 12, 4) + "-" + NumberToString
          (entities.ExistingImHouseholdMbrMnthlySum.Month, 14, 2) + "-01");

        foreach(var item1 in ReadSupported())
        {
          if (Equal(entities.ExistingSupported.PgmChgEffectiveDate,
            local.Null1.Date) || Lt
            (local.FirstDayOfMsum.Date,
            entities.ExistingSupported.PgmChgEffectiveDate))
          {
            UpdateSupported();
          }
        }
      }

      ++local.CommitCount.Count;

      // *******
      // Commit only on change of Obligor.
      // *******
      if (!Equal(entities.ExistingObligor1.Number, local.PrevObligor.Number) &&
        local.CommitCount.Count > local
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault() && AsChar
        (local.TestRun.Flag) != 'Y')
      {
        local.CommitCnt.Count = 0;
        UseExtToDoACommit();

        if (local.ForCommit.NumericReturnCode != 0)
        {
          ExitState = "FN0000_INVALID_EXT_COMMIT_RTN_CD";

          break;
        }
      }

      local.PrevObligor.Number = entities.ExistingObligor1.Number;

      if (AsChar(local.DisplayInd.Flag) == 'Y')
      {
        local.EabReportSend.RptDetail =
          "*************Display for testing ******************";
        UseCabErrorReport2();

        if (!Equal(local.Status.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        local.EabReportSend.RptDetail = "Obligor #:" + entities
          .ExistingObligor1.Number;
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + "; Obligation #:";
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + NumberToString
          (entities.ExistingObligation.SystemGeneratedIdentifier, 13, 3);
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + "; Oblg Type Code: ";
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + entities
          .ObligationType.Code;
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + "; Supp Pers #: ";
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + entities
          .ExistingSuppPerson.Number;
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + "; Debt Amt:$ ";
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + NumberToString
          ((long)(entities.ExistingDebt.Amount * 100), 7, 9);
        UseCabErrorReport2();

        if (!Equal(local.Status.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }
    }

ReadEach:

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseOeB466CloseDown();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }
    else
    {
      UseOeB466CloseDown();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    if (AsChar(local.TestRun.Flag) == 'Y')
    {
      ExitState = "ACO_NN000_ROLLBACK_FOR_BATCH_TST";
    }
    else
    {
      ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
    }
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveObligationType1(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveObligationType2(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Classification = source.Classification;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.Status.Status = useExport.EabFileHandling.Status;
  }

  private string UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    return useExport.ExitStateWorkArea.Message;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.ForCommit.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.ForCommit.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseFnDetCaseNoAndWrkrForDbt()
  {
    var useImport = new FnDetCaseNoAndWrkrForDbt.Import();
    var useExport = new FnDetCaseNoAndWrkrForDbt.Export();

    MoveObligationType1(entities.ObligationType, useImport.ObligationType);
    useImport.Obligor.Number = entities.ExistingObligor1.Number;
    useImport.Supported.Number = entities.ExistingSuppPerson.Number;
    useImport.DebtDetail.DueDt = entities.ExistingDebtDetail.DueDt;

    Call(FnDetCaseNoAndWrkrForDbt.Execute, useImport, useExport);

    local.Case1.Number = useExport.Case1.Number;
  }

  private void UseFnDeterminePgmForDebtDetail()
  {
    var useImport = new FnDeterminePgmForDebtDetail.Import();
    var useExport = new FnDeterminePgmForDebtDetail.Export();

    useImport.SupportedPerson.Number = entities.ExistingSuppPerson.Number;
    MoveObligationType2(entities.ExistingObligationType,
      useImport.ObligationType);
    useImport.Obligation.OrderTypeCode =
      entities.ExistingObligation.OrderTypeCode;
    useImport.DebtDetail.Assign(entities.ExistingDebtDetail);
    useImport.Collection.Date = local.ProcessDate.Date;

    Call(FnDeterminePgmForDebtDetail.Execute, useImport, useExport);

    local.Program.Code = useExport.Program.Code;
  }

  private void UseFnHardcodedDebtDistribution()
  {
    var useImport = new FnHardcodedDebtDistribution.Import();
    var useExport = new FnHardcodedDebtDistribution.Export();

    Call(FnHardcodedDebtDistribution.Execute, useImport, useExport);

    local.HardcodedMjType.SystemGeneratedIdentifier =
      useExport.OtMedicalJudgement.SystemGeneratedIdentifier;
  }

  private void UseOeB466CloseDown()
  {
    var useImport = new OeB466CloseDown.Import();
    var useExport = new OeB466CloseDown.Export();

    useImport.TotAmtSkipped.TotalCurrency =
      local.TotNonAfAmtSkipped.TotalCurrency;
    useImport.RecsSkipped.Count = local.NonAfRecsSkipped.Count;
    useImport.TotAmtRead.TotalCurrency = local.TotAmtRead.TotalCurrency;
    useImport.TotAmtProcessed.TotalCurrency =
      local.TotAmtProcessed.TotalCurrency;
    useImport.TotAmtErrored.TotalCurrency = local.TotAmtErrored.TotalCurrency;
    useImport.RecsRead.Count = local.TotRecsRead.Count;
    useImport.RecsProcessed.Count = local.TotRecsProcessed.Count;
    useImport.RecsErrored.Count = local.TotRecsErrored.Count;

    Call(OeB466CloseDown.Execute, useImport, useExport);
  }

  private void UseOeB466Initialization()
  {
    var useImport = new OeB466Initialization.Import();
    var useExport = new OeB466Initialization.Export();

    Call(OeB466Initialization.Execute, useImport, useExport);

    local.SetUraTrigger.Flag = useExport.SetUraTrigger.Flag;
    local.Start.Number = useExport.Start.Number;
    local.End.Number = useExport.End.Number;
    MoveProgramProcessingInfo(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
    local.TestRun.Flag = useExport.TestRunInd.Flag;
    local.DisplayInd.Flag = useExport.DisplayInd.Flag;
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadCsePersonCsePersonObligationTypeObligation()
  {
    entities.ExistingObligor1.Populated = false;
    entities.ExistingSuppPerson.Populated = false;
    entities.ExistingObligationType.Populated = false;
    entities.ExistingObligation.Populated = false;
    entities.ExistingDebt.Populated = false;
    entities.ExistingDebtDetail.Populated = false;

    return ReadEach("ReadCsePersonCsePersonObligationTypeObligation",
      (db, command) =>
      {
        db.SetString(command, "number1", local.Start.Number);
        db.SetString(command, "number2", local.End.Number);
        db.SetInt32(
          command, "debtTypId",
          local.HardcodedMjType.SystemGeneratedIdentifier);
        db.SetNullableDate(
          command, "uraUpdProcDate", local.Null1.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingObligor1.Number = db.GetString(reader, 0);
        entities.ExistingObligation.CspNumber = db.GetString(reader, 0);
        entities.ExistingDebt.CspNumber = db.GetString(reader, 0);
        entities.ExistingDebt.CspNumber = db.GetString(reader, 0);
        entities.ExistingDebtDetail.CspNumber = db.GetString(reader, 0);
        entities.ExistingSuppPerson.Number = db.GetString(reader, 1);
        entities.ExistingDebt.CspSupNumber = db.GetNullableString(reader, 1);
        entities.ExistingObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingObligation.DtyGeneratedId = db.GetInt32(reader, 2);
        entities.ExistingDebt.OtyType = db.GetInt32(reader, 2);
        entities.ExistingDebtDetail.OtyType = db.GetInt32(reader, 2);
        entities.ExistingObligationType.Classification =
          db.GetString(reader, 3);
        entities.ExistingObligation.CpaType = db.GetString(reader, 4);
        entities.ExistingDebt.CpaType = db.GetString(reader, 4);
        entities.ExistingDebtDetail.CpaType = db.GetString(reader, 4);
        entities.ExistingObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 5);
        entities.ExistingDebt.ObgGeneratedId = db.GetInt32(reader, 5);
        entities.ExistingDebtDetail.ObgGeneratedId = db.GetInt32(reader, 5);
        entities.ExistingObligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 6);
        entities.ExistingObligation.OrderTypeCode = db.GetString(reader, 7);
        entities.ExistingDebt.SystemGeneratedIdentifier =
          db.GetInt32(reader, 8);
        entities.ExistingDebtDetail.OtrGeneratedId = db.GetInt32(reader, 8);
        entities.ExistingDebt.Type1 = db.GetString(reader, 9);
        entities.ExistingDebtDetail.OtrType = db.GetString(reader, 9);
        entities.ExistingDebt.Amount = db.GetDecimal(reader, 10);
        entities.ExistingDebt.LastUpdatedBy = db.GetNullableString(reader, 11);
        entities.ExistingDebt.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 12);
        entities.ExistingDebt.CpaSupType = db.GetNullableString(reader, 13);
        entities.ExistingDebt.UraUpdateProcDate =
          db.GetNullableDate(reader, 14);
        entities.ExistingDebtDetail.DueDt = db.GetDate(reader, 15);
        entities.ExistingDebtDetail.CoveredPrdStartDt =
          db.GetNullableDate(reader, 16);
        entities.ExistingDebtDetail.CoveredPrdEndDt =
          db.GetNullableDate(reader, 17);
        entities.ExistingDebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 18);
        entities.ExistingObligor1.Populated = true;
        entities.ExistingSuppPerson.Populated = true;
        entities.ExistingObligationType.Populated = true;
        entities.ExistingObligation.Populated = true;
        entities.ExistingDebt.Populated = true;
        entities.ExistingDebtDetail.Populated = true;

        return true;
      });
  }

  private bool ReadImHouseholdMbrMnthlySumImHousehold1()
  {
    entities.ExistingImHousehold.Populated = false;
    entities.ExistingImHouseholdMbrMnthlySum.Populated = false;

    return Read("ReadImHouseholdMbrMnthlySumImHousehold1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingSuppPerson.Number);
        db.SetInt32(command, "year0", local.DueDate.Year);
        db.SetInt32(command, "month0", local.DueDate.Month);
      },
      (db, reader) =>
      {
        entities.ExistingImHouseholdMbrMnthlySum.Year = db.GetInt32(reader, 0);
        entities.ExistingImHouseholdMbrMnthlySum.Month = db.GetInt32(reader, 1);
        entities.ExistingImHouseholdMbrMnthlySum.GrantMedicalAmount =
          db.GetNullableDecimal(reader, 2);
        entities.ExistingImHouseholdMbrMnthlySum.UraMedicalAmount =
          db.GetNullableDecimal(reader, 3);
        entities.ExistingImHouseholdMbrMnthlySum.LastUpdatedBy =
          db.GetNullableString(reader, 4);
        entities.ExistingImHouseholdMbrMnthlySum.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 5);
        entities.ExistingImHouseholdMbrMnthlySum.ImhAeCaseNo =
          db.GetString(reader, 6);
        entities.ExistingImHousehold.AeCaseNo = db.GetString(reader, 6);
        entities.ExistingImHouseholdMbrMnthlySum.CspNumber =
          db.GetString(reader, 7);
        entities.ExistingImHousehold.Populated = true;
        entities.ExistingImHouseholdMbrMnthlySum.Populated = true;
      });
  }

  private IEnumerable<bool> ReadImHouseholdMbrMnthlySumImHousehold2()
  {
    entities.ExistingImHousehold.Populated = false;
    entities.ExistingImHouseholdMbrMnthlySum.Populated = false;

    return ReadEach("ReadImHouseholdMbrMnthlySumImHousehold2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingSuppPerson.Number);
        db.SetInt32(command, "year0", local.DueDate.Year);
        db.SetInt32(command, "month0", local.DueDate.Month);
      },
      (db, reader) =>
      {
        entities.ExistingImHouseholdMbrMnthlySum.Year = db.GetInt32(reader, 0);
        entities.ExistingImHouseholdMbrMnthlySum.Month = db.GetInt32(reader, 1);
        entities.ExistingImHouseholdMbrMnthlySum.GrantMedicalAmount =
          db.GetNullableDecimal(reader, 2);
        entities.ExistingImHouseholdMbrMnthlySum.UraMedicalAmount =
          db.GetNullableDecimal(reader, 3);
        entities.ExistingImHouseholdMbrMnthlySum.LastUpdatedBy =
          db.GetNullableString(reader, 4);
        entities.ExistingImHouseholdMbrMnthlySum.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 5);
        entities.ExistingImHouseholdMbrMnthlySum.ImhAeCaseNo =
          db.GetString(reader, 6);
        entities.ExistingImHousehold.AeCaseNo = db.GetString(reader, 6);
        entities.ExistingImHouseholdMbrMnthlySum.CspNumber =
          db.GetString(reader, 7);
        entities.ExistingImHousehold.Populated = true;
        entities.ExistingImHouseholdMbrMnthlySum.Populated = true;

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
          entities.ExistingObligationType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Populated = true;
      });
  }

  private IEnumerable<bool> ReadSupported()
  {
    entities.ExistingSupported.Populated = false;

    return ReadEach("ReadSupported",
      (db, command) =>
      {
        db.SetInt32(
          command, "year0", entities.ExistingImHouseholdMbrMnthlySum.Year);
        db.SetInt32(
          command, "month0", entities.ExistingImHouseholdMbrMnthlySum.Month);
        db.SetString(
          command, "imhAeCaseNo", entities.ExistingImHousehold.AeCaseNo);
      },
      (db, reader) =>
      {
        entities.ExistingSupported.CspNumber = db.GetString(reader, 0);
        entities.ExistingSupported.Type1 = db.GetString(reader, 1);
        entities.ExistingSupported.LastUpdatedBy =
          db.GetNullableString(reader, 2);
        entities.ExistingSupported.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 3);
        entities.ExistingSupported.PgmChgEffectiveDate =
          db.GetNullableDate(reader, 4);
        entities.ExistingSupported.TriggerType =
          db.GetNullableString(reader, 5);
        entities.ExistingSupported.Populated = true;

        return true;
      });
  }

  private void UpdateDebt()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingDebt.Populated);

    var lastUpdatedBy = local.UserId.Text8;
    var lastUpdatedTmst = local.Current.Timestamp;
    var uraUpdateProcDate = local.ProgramProcessingInfo.ProcessDate;

    entities.ExistingDebt.Populated = false;
    Update("UpdateDebt",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableDate(command, "uraUpdProcDate", uraUpdateProcDate);
        db.SetInt32(
          command, "obgGeneratedId", entities.ExistingDebt.ObgGeneratedId);
        db.SetString(command, "cspNumber", entities.ExistingDebt.CspNumber);
        db.SetString(command, "cpaType", entities.ExistingDebt.CpaType);
        db.SetInt32(
          command, "obTrnId", entities.ExistingDebt.SystemGeneratedIdentifier);
        db.SetString(command, "obTrnTyp", entities.ExistingDebt.Type1);
        db.SetInt32(command, "otyType", entities.ExistingDebt.OtyType);
      });

    entities.ExistingDebt.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingDebt.LastUpdatedTmst = lastUpdatedTmst;
    entities.ExistingDebt.UraUpdateProcDate = uraUpdateProcDate;
    entities.ExistingDebt.Populated = true;
  }

  private void UpdateImHouseholdMbrMnthlySum()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingImHouseholdMbrMnthlySum.Populated);

    var grantMedicalAmount =
      entities.ExistingImHouseholdMbrMnthlySum.GrantMedicalAmount.
        GetValueOrDefault() + entities.ExistingDebt.Amount;
    var uraMedicalAmount =
      entities.ExistingImHouseholdMbrMnthlySum.UraMedicalAmount.
        GetValueOrDefault() + entities.ExistingDebt.Amount;
    var lastUpdatedBy = local.UserId.Text8;
    var lastUpdatedTmst = local.Current.Timestamp;

    entities.ExistingImHouseholdMbrMnthlySum.Populated = false;
    Update("UpdateImHouseholdMbrMnthlySum",
      (db, command) =>
      {
        db.SetNullableDecimal(command, "grantMedAmt", grantMedicalAmount);
        db.SetNullableDecimal(command, "uraMedicalAmount", uraMedicalAmount);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetInt32(
          command, "year0", entities.ExistingImHouseholdMbrMnthlySum.Year);
        db.SetInt32(
          command, "month0", entities.ExistingImHouseholdMbrMnthlySum.Month);
        db.SetString(
          command, "imhAeCaseNo",
          entities.ExistingImHouseholdMbrMnthlySum.ImhAeCaseNo);
        db.SetString(
          command, "cspNumber",
          entities.ExistingImHouseholdMbrMnthlySum.CspNumber);
      });

    entities.ExistingImHouseholdMbrMnthlySum.GrantMedicalAmount =
      grantMedicalAmount;
    entities.ExistingImHouseholdMbrMnthlySum.UraMedicalAmount =
      uraMedicalAmount;
    entities.ExistingImHouseholdMbrMnthlySum.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingImHouseholdMbrMnthlySum.LastUpdatedTmst = lastUpdatedTmst;
    entities.ExistingImHouseholdMbrMnthlySum.Populated = true;
  }

  private void UpdateSupported()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingSupported.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();
    var pgmChgEffectiveDate = local.FirstDayOfMsum.Date;
    var triggerType = "U";

    entities.ExistingSupported.Populated = false;
    Update("UpdateSupported",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableDate(command, "recompBalFromDt", pgmChgEffectiveDate);
        db.SetNullableString(command, "triggerType", triggerType);
        db.
          SetString(command, "cspNumber", entities.ExistingSupported.CspNumber);
          
        db.SetString(command, "type", entities.ExistingSupported.Type1);
      });

    entities.ExistingSupported.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingSupported.LastUpdatedTmst = lastUpdatedTmst;
    entities.ExistingSupported.PgmChgEffectiveDate = pgmChgEffectiveDate;
    entities.ExistingSupported.TriggerType = triggerType;
    entities.ExistingSupported.Populated = true;
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
    /// A value of UpdateMthSummSuccessful.
    /// </summary>
    [JsonPropertyName("updateMthSummSuccessful")]
    public Common UpdateMthSummSuccessful
    {
      get => updateMthSummSuccessful ??= new();
      set => updateMthSummSuccessful = value;
    }

    /// <summary>
    /// A value of FirstDayOfMsum.
    /// </summary>
    [JsonPropertyName("firstDayOfMsum")]
    public DateWorkArea FirstDayOfMsum
    {
      get => firstDayOfMsum ??= new();
      set => firstDayOfMsum = value;
    }

    /// <summary>
    /// A value of SetUraTrigger.
    /// </summary>
    [JsonPropertyName("setUraTrigger")]
    public Common SetUraTrigger
    {
      get => setUraTrigger ??= new();
      set => setUraTrigger = value;
    }

    /// <summary>
    /// A value of ServiceProviderRequired.
    /// </summary>
    [JsonPropertyName("serviceProviderRequired")]
    public Common ServiceProviderRequired
    {
      get => serviceProviderRequired ??= new();
      set => serviceProviderRequired = value;
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
    /// A value of TotNonAfAmtSkipped.
    /// </summary>
    [JsonPropertyName("totNonAfAmtSkipped")]
    public Common TotNonAfAmtSkipped
    {
      get => totNonAfAmtSkipped ??= new();
      set => totNonAfAmtSkipped = value;
    }

    /// <summary>
    /// A value of NonAfRecsSkipped.
    /// </summary>
    [JsonPropertyName("nonAfRecsSkipped")]
    public Common NonAfRecsSkipped
    {
      get => nonAfRecsSkipped ??= new();
      set => nonAfRecsSkipped = value;
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
    /// A value of ProcessDate.
    /// </summary>
    [JsonPropertyName("processDate")]
    public DateWorkArea ProcessDate
    {
      get => processDate ??= new();
      set => processDate = value;
    }

    /// <summary>
    /// A value of PrevObligor.
    /// </summary>
    [JsonPropertyName("prevObligor")]
    public CsePerson PrevObligor
    {
      get => prevObligor ??= new();
      set => prevObligor = value;
    }

    /// <summary>
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    public CsePerson Start
    {
      get => start ??= new();
      set => start = value;
    }

    /// <summary>
    /// A value of End.
    /// </summary>
    [JsonPropertyName("end")]
    public CsePerson End
    {
      get => end ??= new();
      set => end = value;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    /// <summary>
    /// A value of DueDate.
    /// </summary>
    [JsonPropertyName("dueDate")]
    public DateWorkArea DueDate
    {
      get => dueDate ??= new();
      set => dueDate = value;
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
    /// A value of TotAmtRead.
    /// </summary>
    [JsonPropertyName("totAmtRead")]
    public Common TotAmtRead
    {
      get => totAmtRead ??= new();
      set => totAmtRead = value;
    }

    /// <summary>
    /// A value of TotAmtProcessed.
    /// </summary>
    [JsonPropertyName("totAmtProcessed")]
    public Common TotAmtProcessed
    {
      get => totAmtProcessed ??= new();
      set => totAmtProcessed = value;
    }

    /// <summary>
    /// A value of TotAmtErrored.
    /// </summary>
    [JsonPropertyName("totAmtErrored")]
    public Common TotAmtErrored
    {
      get => totAmtErrored ??= new();
      set => totAmtErrored = value;
    }

    /// <summary>
    /// A value of TotRecsRead.
    /// </summary>
    [JsonPropertyName("totRecsRead")]
    public Common TotRecsRead
    {
      get => totRecsRead ??= new();
      set => totRecsRead = value;
    }

    /// <summary>
    /// A value of TotRecsProcessed.
    /// </summary>
    [JsonPropertyName("totRecsProcessed")]
    public Common TotRecsProcessed
    {
      get => totRecsProcessed ??= new();
      set => totRecsProcessed = value;
    }

    /// <summary>
    /// A value of TotRecsErrored.
    /// </summary>
    [JsonPropertyName("totRecsErrored")]
    public Common TotRecsErrored
    {
      get => totRecsErrored ??= new();
      set => totRecsErrored = value;
    }

    /// <summary>
    /// A value of ReadForCommit.
    /// </summary>
    [JsonPropertyName("readForCommit")]
    public Common ReadForCommit
    {
      get => readForCommit ??= new();
      set => readForCommit = value;
    }

    /// <summary>
    /// A value of UpdForCommit.
    /// </summary>
    [JsonPropertyName("updForCommit")]
    public Common UpdForCommit
    {
      get => updForCommit ??= new();
      set => updForCommit = value;
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
    /// A value of HardcodedMsType.
    /// </summary>
    [JsonPropertyName("hardcodedMsType")]
    public ObligationType HardcodedMsType
    {
      get => hardcodedMsType ??= new();
      set => hardcodedMsType = value;
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
    /// A value of TestRun.
    /// </summary>
    [JsonPropertyName("testRun")]
    public Common TestRun
    {
      get => testRun ??= new();
      set => testRun = value;
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
    /// A value of CommitCount.
    /// </summary>
    [JsonPropertyName("commitCount")]
    public Common CommitCount
    {
      get => commitCount ??= new();
      set => commitCount = value;
    }

    /// <summary>
    /// A value of CommitCnt.
    /// </summary>
    [JsonPropertyName("commitCnt")]
    public Common CommitCnt
    {
      get => commitCnt ??= new();
      set => commitCnt = value;
    }

    /// <summary>
    /// A value of ForCommit.
    /// </summary>
    [JsonPropertyName("forCommit")]
    public External ForCommit
    {
      get => forCommit ??= new();
      set => forCommit = value;
    }

    /// <summary>
    /// A value of NbrOfCheckpoint.
    /// </summary>
    [JsonPropertyName("nbrOfCheckpoint")]
    public Common NbrOfCheckpoint
    {
      get => nbrOfCheckpoint ??= new();
      set => nbrOfCheckpoint = value;
    }

    /// <summary>
    /// A value of Status.
    /// </summary>
    [JsonPropertyName("status")]
    public EabFileHandling Status
    {
      get => status ??= new();
      set => status = value;
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

    private Common updateMthSummSuccessful;
    private DateWorkArea firstDayOfMsum;
    private Common setUraTrigger;
    private Common serviceProviderRequired;
    private Case1 case1;
    private Common totNonAfAmtSkipped;
    private Common nonAfRecsSkipped;
    private Program program;
    private DateWorkArea processDate;
    private CsePerson prevObligor;
    private CsePerson start;
    private CsePerson end;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private DateWorkArea dueDate;
    private DateWorkArea null1;
    private Common totAmtRead;
    private Common totAmtProcessed;
    private Common totAmtErrored;
    private Common totRecsRead;
    private Common totRecsProcessed;
    private Common totRecsErrored;
    private Common readForCommit;
    private Common updForCommit;
    private ObligationType hardcodedMjType;
    private ObligationType hardcodedMsType;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private ExitStateWorkArea exitStateWorkArea;
    private TextWorkArea userId;
    private DateWorkArea current;
    private Common testRun;
    private Common displayInd;
    private Common commitCount;
    private Common commitCnt;
    private External forCommit;
    private Common nbrOfCheckpoint;
    private EabFileHandling status;
    private Infrastructure infrastructure;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ExistingSuppPerson.
    /// </summary>
    [JsonPropertyName("existingSuppPerson")]
    public CsePerson ExistingSuppPerson
    {
      get => existingSuppPerson ??= new();
      set => existingSuppPerson = value;
    }

    /// <summary>
    /// A value of ExistingSupported.
    /// </summary>
    [JsonPropertyName("existingSupported")]
    public CsePersonAccount ExistingSupported
    {
      get => existingSupported ??= new();
      set => existingSupported = value;
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

    /// <summary>
    /// A value of ExistingImHousehold.
    /// </summary>
    [JsonPropertyName("existingImHousehold")]
    public ImHousehold ExistingImHousehold
    {
      get => existingImHousehold ??= new();
      set => existingImHousehold = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public ImHousehold New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of ExistingImHouseholdMbrMnthlySum.
    /// </summary>
    [JsonPropertyName("existingImHouseholdMbrMnthlySum")]
    public ImHouseholdMbrMnthlySum ExistingImHouseholdMbrMnthlySum
    {
      get => existingImHouseholdMbrMnthlySum ??= new();
      set => existingImHouseholdMbrMnthlySum = value;
    }

    /// <summary>
    /// A value of ImHouseholdMbrMnthlySum.
    /// </summary>
    [JsonPropertyName("imHouseholdMbrMnthlySum")]
    public ImHouseholdMbrMnthlySum ImHouseholdMbrMnthlySum
    {
      get => imHouseholdMbrMnthlySum ??= new();
      set => imHouseholdMbrMnthlySum = value;
    }

    private ObligationType obligationType;
    private CsePerson existingObligor1;
    private CsePersonAccount existingObligor2;
    private CsePerson existingSuppPerson;
    private CsePersonAccount existingSupported;
    private ObligationType existingObligationType;
    private Obligation existingObligation;
    private ObligationTransaction existingDebt;
    private DebtDetail existingDebtDetail;
    private ImHousehold existingImHousehold;
    private ImHousehold new1;
    private ImHouseholdMbrMnthlySum existingImHouseholdMbrMnthlySum;
    private ImHouseholdMbrMnthlySum imHouseholdMbrMnthlySum;
  }
#endregion
}
