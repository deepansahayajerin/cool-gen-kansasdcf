// Program: FN_BFX1_FIX_ACCRUAL_INSTR, ID: 372875088, model: 746.
// Short name: SWEFFX1B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_BFX1_FIX_ACCRUAL_INSTR.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnBfx1FixAccrualInstr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_BFX1_FIX_ACCRUAL_INSTR program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnBfx1FixAccrualInstr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnBfx1FixAccrualInstr.
  /// </summary>
  public FnBfx1FixAccrualInstr(IContext context, Import import, Export export):
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
    local.UserId.Text8 = global.UserId;
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

    local.StartObligor.Number =
      Substring(local.ProgramProcessingInfo.ParameterList, 1, 10);
    local.EndObligor.Number =
      Substring(local.ProgramProcessingInfo.ParameterList, 12, 10);
    local.ProcessCntRlnOnly.Count =
      (int)StringToNumber(Substring(
        local.ProgramProcessingInfo.ParameterList, 23, 5));
    local.ProcessCntAll.Count =
      (int)StringToNumber(Substring(
        local.ProgramProcessingInfo.ParameterList, 29, 5));
    local.TmpDate.Text10 =
      Substring(local.ProgramProcessingInfo.ParameterList, 35, 10);
    local.Conversion.Date = StringToDate(local.TmpDate.Text10);
    local.CommitFrequency.Count =
      (int)StringToNumber(Substring(
        local.ProgramProcessingInfo.ParameterList, 46, 5));

    if (local.CommitFrequency.Count == 0)
    {
      local.CommitFrequency.Count = 100;
    }

    local.EabFileHandling.Action = "OPEN";

    // **********************************************************
    // OPEN OUTPUT CONTROL REPORT 98
    // **********************************************************
    UseCabControlReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    // **********************************************************
    // OPEN OUTPUT ERROR REPORT 99
    // **********************************************************
    UseCabErrorReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_ERROR_RPT_A";

      return;
    }

    if (!ReadObligationTransactionRlnRsn())
    {
      ExitState = "FN0000_OBLIG_TRANS_RLN_RSN_NF";

      return;
    }

    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail = "PARM LIST";
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.EabReportSend.RptDetail = "Starting Obligor . . . . . . . . . :" + local
      .StartObligor.Number;
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.EabReportSend.RptDetail = "Ending Obligor . . . . . . . . . . :" + local
      .EndObligor.Number;
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.EabReportSend.RptDetail = "RLN ONLY Proc Cnt. . . . . . . . . :" + NumberToString
      (local.ProcessCntRlnOnly.Count, 15);
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.EabReportSend.RptDetail = "ALL Proc Cnt . . . . . . . . . . . :" + NumberToString
      (local.ProcessCntAll.Count, 15);
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.EabReportSend.RptDetail = "Conversion Date. . . . . . . . . . :" + NumberToString
      (DateToInt(local.Conversion.Date), 15);
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.EabReportSend.RptDetail = "Commit Frequency . . . . . . . . . :" + NumberToString
      (local.CommitFrequency.Count, 15);
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    foreach(var item in ReadCsePersonObligationObligationType())
    {
      if (local.CommitCnt.Count >= local.CommitFrequency.Count)
      {
        local.CommitCnt.Count = 0;
        UseExtToDoACommit();

        if (local.ForCommit.NumericReturnCode != 0)
        {
          ExitState = "FN0000_INVALID_EXT_COMMIT_RTN_CD";

          return;
        }
      }

      ++local.NoOfObligationsRead.Count;

      if (!ReadObligationPaymentSchedule())
      {
        // : ERROR - Payment Schedule Not Found.
        local.EabReportSend.RptDetail =
          "ERROR - Payment Schedule Not Found, Obligor ID . : " + entities
          .Obligor1.Number + ", Ob ID : " + NumberToString
          (entities.Obligation.SystemGeneratedIdentifier, 15) + ", Ob Type : " +
          NumberToString
          (entities.ObligationType.SystemGeneratedIdentifier, 15);
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        continue;
      }

      local.SupPrsnCompare.Number = "";

      foreach(var item1 in ReadCsePersonDebt())
      {
        if (Equal(entities.Supported1.Number, local.SupPrsnCompare.Number))
        {
          local.EabReportSend.RptDetail =
            "ERROR - Multiple Debt Details Found for SupPrsn. : " + entities
            .Obligor1.Number + ", Obligation ID : " + NumberToString
            (entities.Obligation.SystemGeneratedIdentifier, 15) + ", Obligation Type : " +
            NumberToString
            (entities.ObligationType.SystemGeneratedIdentifier, 15);
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          goto ReadEach1;
        }
        else
        {
          local.SupPrsnCompare.Number = entities.Supported1.Number;
        }
      }

      local.DebtFound.Flag = "N";

      foreach(var item1 in ReadCsePersonSupportedDebtLegalActionPerson())
      {
        local.DebtFound.Flag = "Y";
        ++local.NoOfDebtDtlDebtsRead.Count;
        ReadDebt2();

        if (local.Common.Count > 1)
        {
          local.EabReportSend.RptDetail =
            "ERROR - Multiple Accrual Instr Found for SupPrsn : " + entities
            .Obligor1.Number + ", Obligation ID : " + NumberToString
            (entities.Obligation.SystemGeneratedIdentifier, 15) + ", Obligation Type : " +
            NumberToString
            (entities.ObligationType.SystemGeneratedIdentifier, 15);
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          goto ReadEach1;
        }

        if (ReadDebt3())
        {
          if (local.ProcessCntRlnOnly.Count == 0)
          {
            goto ReadEach1;
          }

          if (ReadObligationTransactionRln())
          {
            goto ReadEach1;
          }
          else
          {
            for(local.CreateLoop.Count = 1; local.CreateLoop.Count <= 10; ++
              local.CreateLoop.Count)
            {
              try
              {
                CreateObligationTransactionRln1();
                ++local.CommitCnt.Count;

                break;
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    if (local.CreateLoop.Count == 10)
                    {
                      ExitState = "FN0000_OBLIG_TRANS_RLN_AE_RB";

                      return;
                    }

                    continue;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "FN0000_OBLIG_TRANS_RLN_PV_RB";

                    return;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }
            }

            ++local.CommitCnt.Count;
            ++local.NoOfAiAddedRlnOnly.Count;

            // : Successfully created new Accrual Instructions to Debt Detail 
            // Relationship.
            local.EabReportSend.RptDetail =
              "New Accrual Instructions RLN ONLY, Olbigor ID. . : " + entities
              .Obligor1.Number + ", Ob ID : " + NumberToString
              (entities.Obligation.SystemGeneratedIdentifier, 15) + ", Ob Type : " +
              NumberToString
              (entities.ObligationType.SystemGeneratedIdentifier, 15) + ", SP : " +
              entities.Supported1.Number;
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            if (local.NoOfAiAddedRlnOnly.Count > local.ProcessCntRlnOnly.Count)
            {
              goto ReadEach2;
            }
          }
        }
        else
        {
          if (local.ProcessCntAll.Count == 0)
          {
            goto ReadEach1;
          }

          // : Build the Accrual Instructions for the Obligation.
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

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_OBLIG_PYMNT_SCH_PV_RB";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          for(local.CreateLoop.Count = 1; local.CreateLoop.Count <= 10; ++
            local.CreateLoop.Count)
          {
            try
            {
              CreateDebt();

              break;
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  if (local.CreateLoop.Count == 10)
                  {
                    ExitState = "FN0000_OBLIG_TRANS_AE_RB";

                    return;
                  }

                  continue;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "FN0000_OBLIG_TRANS_PV_RB";

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }

          try
          {
            CreateAccrualInstructions();
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

          for(local.CreateLoop.Count = 1; local.CreateLoop.Count <= 10; ++
            local.CreateLoop.Count)
          {
            try
            {
              CreateObligationTransactionRln2();

              break;
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  if (local.CreateLoop.Count == 10)
                  {
                    ExitState = "FN0000_OBLIG_TRANS_RLN_AE_RB";

                    return;
                  }

                  continue;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "FN0000_OBLIG_TRANS_RLN_PV_RB";

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }

          ++local.CommitCnt.Count;
          ++local.NoOfAiAddedAll.Count;

          // : Successfully created new Accrual Instructions and supporting 
          // structure.
          local.EabReportSend.RptDetail =
            "New Accrual Instructions ALL, Olbigor ID . . . . : " + entities
            .Obligor1.Number + ", Ob ID : " + NumberToString
            (entities.Obligation.SystemGeneratedIdentifier, 15) + ", Ob Type : " +
            NumberToString
            (entities.ObligationType.SystemGeneratedIdentifier, 15) + ", SP : " +
            entities.Supported1.Number;
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          if (local.NoOfAiAddedAll.Count > local.ProcessCntAll.Count)
          {
            goto ReadEach2;
          }
        }
      }

      if (AsChar(local.DebtFound.Flag) == 'N')
      {
        if (!ReadDebt1())
        {
          local.EabReportSend.RptDetail =
            "ERROR - OB Tran does not exist for Obligation. . : " + entities
            .Obligor1.Number + ", Obligation ID : " + NumberToString
            (entities.Obligation.SystemGeneratedIdentifier, 15) + ", Obligation Type : " +
            NumberToString
            (entities.ObligationType.SystemGeneratedIdentifier, 15);
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }
        }
      }

ReadEach1:
      ;
    }

ReadEach2:

    local.EabReportSend.RptDetail = "";
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.EabReportSend.RptDetail =
      "No of Obligations Read . . . . . . . . . . . : " + NumberToString
      (local.NoOfObligationsRead.Count, 15);
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.EabReportSend.RptDetail =
      "No of Debt Dtl Debts Read. . . . . . . . . . : " + NumberToString
      (local.NoOfDebtDtlDebtsRead.Count, 15);
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.EabReportSend.RptDetail =
      "No of Accrual Instr Added Read ALL . . . . . : " + NumberToString
      (local.NoOfAiAddedAll.Count, 15);
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.EabReportSend.RptDetail =
      "No of Accrual Instr Added Read RLN ONLY. . . : " + NumberToString
      (local.NoOfAiAddedRlnOnly.Count, 15);
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.EabFileHandling.Action = "CLOSE";

    // **********************************************************
    // CLOSE OUTPUT CONTROL REPORT 98
    // **********************************************************
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    // **********************************************************
    // CLOSE OUTPUT ERROR REPORT 99
    // **********************************************************
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_ERROR_RPT_A";
    }
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport3()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.ForCommit.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.ForCommit.NumericReturnCode = useExport.External.NumericReturnCode;
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

  private int UseGenerate9DigitRandomNumber()
  {
    var useImport = new Generate9DigitRandomNumber.Import();
    var useExport = new Generate9DigitRandomNumber.Export();

    Call(Generate9DigitRandomNumber.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute9DigitRandomNumber;
  }

  private void CreateAccrualInstructions()
  {
    System.Diagnostics.Debug.Assert(entities.NewAccrualInstructions2.Populated);

    var otrType = entities.NewAccrualInstructions2.Type1;
    var otyId = entities.NewAccrualInstructions2.OtyType;
    var obgGeneratedId = entities.NewAccrualInstructions2.ObgGeneratedId;
    var cspNumber = entities.NewAccrualInstructions2.CspNumber;
    var cpaType = entities.NewAccrualInstructions2.CpaType;
    var otrGeneratedId =
      entities.NewAccrualInstructions2.SystemGeneratedIdentifier;
    var asOfDt = local.Conversion.Date;

    CheckValid<AccrualInstructions>("OtrType", otrType);
    CheckValid<AccrualInstructions>("CpaType", cpaType);
    entities.NewAccrualInstructions1.Populated = false;
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
        db.SetNullableDate(command, "discontinueDt", asOfDt);
        db.SetNullableDate(command, "lastAccrualDt", asOfDt);
      });

    entities.NewAccrualInstructions1.OtrType = otrType;
    entities.NewAccrualInstructions1.OtyId = otyId;
    entities.NewAccrualInstructions1.ObgGeneratedId = obgGeneratedId;
    entities.NewAccrualInstructions1.CspNumber = cspNumber;
    entities.NewAccrualInstructions1.CpaType = cpaType;
    entities.NewAccrualInstructions1.OtrGeneratedId = otrGeneratedId;
    entities.NewAccrualInstructions1.AsOfDt = asOfDt;
    entities.NewAccrualInstructions1.DiscontinueDt = asOfDt;
    entities.NewAccrualInstructions1.LastAccrualDt = asOfDt;
    entities.NewAccrualInstructions1.Populated = true;
  }

  private void CreateDebt()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    System.Diagnostics.Debug.Assert(entities.Supported2.Populated);

    var obgGeneratedId = entities.Obligation.SystemGeneratedIdentifier;
    var cspNumber = entities.Obligation.CspNumber;
    var cpaType = entities.Obligation.CpaType;
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var type1 = GetImplicitValue<ObligationTransaction, string>("Type1");
    var amount = entities.DebtDetail.Amount;
    var debtAdjustmentInd = "N";
    var createdBy = local.UserId.Text8;
    var createdTmst = local.Current.Timestamp;
    var debtType = "A";
    var cspSupNumber = entities.Supported2.CspNumber;
    var cpaSupType = entities.Supported2.Type1;
    var otyType = entities.Obligation.DtyGeneratedId;
    var lapId = entities.LegalActionPerson.Identifier;
    var newDebtProcessDate = local.Conversion.Date;

    CheckValid<ObligationTransaction>("CpaType", cpaType);
    CheckValid<ObligationTransaction>("Type1", type1);
    CheckValid<ObligationTransaction>("DebtAdjustmentInd", debtAdjustmentInd);
    CheckValid<ObligationTransaction>("DebtType", debtType);
    CheckValid<ObligationTransaction>("CpaSupType", cpaSupType);
    entities.NewAccrualInstructions2.Populated = false;
    Update("CreateDebt",
      (db, command) =>
      {
        db.SetInt32(command, "obgGeneratedId", obgGeneratedId);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "cpaType", cpaType);
        db.SetInt32(command, "obTrnId", systemGeneratedIdentifier);
        db.SetString(command, "obTrnTyp", type1);
        db.SetDecimal(command, "obTrnAmt", amount);
        db.SetString(command, "debtAdjInd", debtAdjustmentInd);
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
        db.SetNullableDate(command, "newDebtProcDt", newDebtProcessDate);
      });

    entities.NewAccrualInstructions2.ObgGeneratedId = obgGeneratedId;
    entities.NewAccrualInstructions2.CspNumber = cspNumber;
    entities.NewAccrualInstructions2.CpaType = cpaType;
    entities.NewAccrualInstructions2.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.NewAccrualInstructions2.Type1 = type1;
    entities.NewAccrualInstructions2.Amount = amount;
    entities.NewAccrualInstructions2.DebtAdjustmentInd = debtAdjustmentInd;
    entities.NewAccrualInstructions2.CreatedBy = createdBy;
    entities.NewAccrualInstructions2.CreatedTmst = createdTmst;
    entities.NewAccrualInstructions2.DebtType = debtType;
    entities.NewAccrualInstructions2.VoluntaryPercentageAmount = 0;
    entities.NewAccrualInstructions2.CspSupNumber = cspSupNumber;
    entities.NewAccrualInstructions2.CpaSupType = cpaSupType;
    entities.NewAccrualInstructions2.OtyType = otyType;
    entities.NewAccrualInstructions2.LapId = lapId;
    entities.NewAccrualInstructions2.NewDebtProcessDate = newDebtProcessDate;
    entities.NewAccrualInstructions2.Populated = true;
  }

  private void CreateObligationTransactionRln1()
  {
    System.Diagnostics.Debug.Assert(entities.AccrualInstructions.Populated);
    System.Diagnostics.Debug.Assert(entities.DebtDetail.Populated);

    var onrGeneratedId = entities.Accrual.SystemGeneratedIdentifier;
    var otrType = entities.DebtDetail.Type1;
    var otrGeneratedId = entities.DebtDetail.SystemGeneratedIdentifier;
    var cpaType = entities.DebtDetail.CpaType;
    var cspNumber = entities.DebtDetail.CspNumber;
    var obgGeneratedId = entities.DebtDetail.ObgGeneratedId;
    var otrPType = entities.AccrualInstructions.Type1;
    var otrPGeneratedId =
      entities.AccrualInstructions.SystemGeneratedIdentifier;
    var cpaPType = entities.AccrualInstructions.CpaType;
    var cspPNumber = entities.AccrualInstructions.CspNumber;
    var obgPGeneratedId = entities.AccrualInstructions.ObgGeneratedId;
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var createdBy = local.UserId.Text8;
    var createdTmst = local.Current.Timestamp;
    var otyTypePrimary = entities.AccrualInstructions.OtyType;
    var otyTypeSecondary = entities.DebtDetail.OtyType;

    CheckValid<ObligationTransactionRln>("OtrType", otrType);
    CheckValid<ObligationTransactionRln>("CpaType", cpaType);
    CheckValid<ObligationTransactionRln>("OtrPType", otrPType);
    CheckValid<ObligationTransactionRln>("CpaPType", cpaPType);
    entities.NewObligationTransactionRln.Populated = false;
    Update("CreateObligationTransactionRln1",
      (db, command) =>
      {
        db.SetInt32(command, "onrGeneratedId", onrGeneratedId);
        db.SetString(command, "otrType", otrType);
        db.SetInt32(command, "otrGeneratedId", otrGeneratedId);
        db.SetString(command, "cpaType", cpaType);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "obgGeneratedId", obgGeneratedId);
        db.SetString(command, "otrPType", otrPType);
        db.SetInt32(command, "otrPGeneratedId", otrPGeneratedId);
        db.SetString(command, "cpaPType", cpaPType);
        db.SetString(command, "cspPNumber", cspPNumber);
        db.SetInt32(command, "obgPGeneratedId", obgPGeneratedId);
        db.SetInt32(command, "obTrnRlnId", systemGeneratedIdentifier);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetInt32(command, "otyTypePrimary", otyTypePrimary);
        db.SetInt32(command, "otyTypeSecondary", otyTypeSecondary);
        db.SetNullableString(command, "obTrnRlnDsc", "");
      });

    entities.NewObligationTransactionRln.OnrGeneratedId = onrGeneratedId;
    entities.NewObligationTransactionRln.OtrType = otrType;
    entities.NewObligationTransactionRln.OtrGeneratedId = otrGeneratedId;
    entities.NewObligationTransactionRln.CpaType = cpaType;
    entities.NewObligationTransactionRln.CspNumber = cspNumber;
    entities.NewObligationTransactionRln.ObgGeneratedId = obgGeneratedId;
    entities.NewObligationTransactionRln.OtrPType = otrPType;
    entities.NewObligationTransactionRln.OtrPGeneratedId = otrPGeneratedId;
    entities.NewObligationTransactionRln.CpaPType = cpaPType;
    entities.NewObligationTransactionRln.CspPNumber = cspPNumber;
    entities.NewObligationTransactionRln.ObgPGeneratedId = obgPGeneratedId;
    entities.NewObligationTransactionRln.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.NewObligationTransactionRln.CreatedBy = createdBy;
    entities.NewObligationTransactionRln.CreatedTmst = createdTmst;
    entities.NewObligationTransactionRln.OtyTypePrimary = otyTypePrimary;
    entities.NewObligationTransactionRln.OtyTypeSecondary = otyTypeSecondary;
    entities.NewObligationTransactionRln.Description = "";
    entities.NewObligationTransactionRln.Populated = true;
  }

  private void CreateObligationTransactionRln2()
  {
    System.Diagnostics.Debug.Assert(entities.DebtDetail.Populated);
    System.Diagnostics.Debug.Assert(entities.NewAccrualInstructions2.Populated);

    var onrGeneratedId = entities.Accrual.SystemGeneratedIdentifier;
    var otrType = entities.DebtDetail.Type1;
    var otrGeneratedId = entities.DebtDetail.SystemGeneratedIdentifier;
    var cpaType = entities.DebtDetail.CpaType;
    var cspNumber = entities.DebtDetail.CspNumber;
    var obgGeneratedId = entities.DebtDetail.ObgGeneratedId;
    var otrPType = entities.NewAccrualInstructions2.Type1;
    var otrPGeneratedId =
      entities.NewAccrualInstructions2.SystemGeneratedIdentifier;
    var cpaPType = entities.NewAccrualInstructions2.CpaType;
    var cspPNumber = entities.NewAccrualInstructions2.CspNumber;
    var obgPGeneratedId = entities.NewAccrualInstructions2.ObgGeneratedId;
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var createdBy = local.UserId.Text8;
    var createdTmst = local.Current.Timestamp;
    var otyTypePrimary = entities.NewAccrualInstructions2.OtyType;
    var otyTypeSecondary = entities.DebtDetail.OtyType;

    CheckValid<ObligationTransactionRln>("OtrType", otrType);
    CheckValid<ObligationTransactionRln>("CpaType", cpaType);
    CheckValid<ObligationTransactionRln>("OtrPType", otrPType);
    CheckValid<ObligationTransactionRln>("CpaPType", cpaPType);
    entities.NewObligationTransactionRln.Populated = false;
    Update("CreateObligationTransactionRln2",
      (db, command) =>
      {
        db.SetInt32(command, "onrGeneratedId", onrGeneratedId);
        db.SetString(command, "otrType", otrType);
        db.SetInt32(command, "otrGeneratedId", otrGeneratedId);
        db.SetString(command, "cpaType", cpaType);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "obgGeneratedId", obgGeneratedId);
        db.SetString(command, "otrPType", otrPType);
        db.SetInt32(command, "otrPGeneratedId", otrPGeneratedId);
        db.SetString(command, "cpaPType", cpaPType);
        db.SetString(command, "cspPNumber", cspPNumber);
        db.SetInt32(command, "obgPGeneratedId", obgPGeneratedId);
        db.SetInt32(command, "obTrnRlnId", systemGeneratedIdentifier);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetInt32(command, "otyTypePrimary", otyTypePrimary);
        db.SetInt32(command, "otyTypeSecondary", otyTypeSecondary);
        db.SetNullableString(command, "obTrnRlnDsc", "");
      });

    entities.NewObligationTransactionRln.OnrGeneratedId = onrGeneratedId;
    entities.NewObligationTransactionRln.OtrType = otrType;
    entities.NewObligationTransactionRln.OtrGeneratedId = otrGeneratedId;
    entities.NewObligationTransactionRln.CpaType = cpaType;
    entities.NewObligationTransactionRln.CspNumber = cspNumber;
    entities.NewObligationTransactionRln.ObgGeneratedId = obgGeneratedId;
    entities.NewObligationTransactionRln.OtrPType = otrPType;
    entities.NewObligationTransactionRln.OtrPGeneratedId = otrPGeneratedId;
    entities.NewObligationTransactionRln.CpaPType = cpaPType;
    entities.NewObligationTransactionRln.CspPNumber = cspPNumber;
    entities.NewObligationTransactionRln.ObgPGeneratedId = obgPGeneratedId;
    entities.NewObligationTransactionRln.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.NewObligationTransactionRln.CreatedBy = createdBy;
    entities.NewObligationTransactionRln.CreatedTmst = createdTmst;
    entities.NewObligationTransactionRln.OtyTypePrimary = otyTypePrimary;
    entities.NewObligationTransactionRln.OtyTypeSecondary = otyTypeSecondary;
    entities.NewObligationTransactionRln.Description = "";
    entities.NewObligationTransactionRln.Populated = true;
  }

  private IEnumerable<bool> ReadCsePersonDebt()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.Supported1.Populated = false;
    entities.DebtDetail.Populated = false;

    return ReadEach("ReadCsePersonDebt",
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
        entities.Supported1.Number = db.GetString(reader, 0);
        entities.DebtDetail.CspSupNumber = db.GetNullableString(reader, 0);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 1);
        entities.DebtDetail.CspNumber = db.GetString(reader, 2);
        entities.DebtDetail.CpaType = db.GetString(reader, 3);
        entities.DebtDetail.SystemGeneratedIdentifier = db.GetInt32(reader, 4);
        entities.DebtDetail.Type1 = db.GetString(reader, 5);
        entities.DebtDetail.Amount = db.GetDecimal(reader, 6);
        entities.DebtDetail.CreatedBy = db.GetString(reader, 7);
        entities.DebtDetail.DebtType = db.GetString(reader, 8);
        entities.DebtDetail.CpaSupType = db.GetNullableString(reader, 9);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 10);
        entities.DebtDetail.LapId = db.GetNullableInt32(reader, 11);
        entities.Supported1.Populated = true;
        entities.DebtDetail.Populated = true;
        CheckValid<ObligationTransaction>("CpaType", entities.DebtDetail.CpaType);
          
        CheckValid<ObligationTransaction>("Type1", entities.DebtDetail.Type1);
        CheckValid<ObligationTransaction>("DebtType",
          entities.DebtDetail.DebtType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.DebtDetail.CpaSupType);

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePersonObligationObligationType()
  {
    entities.Obligor1.Populated = false;
    entities.Obligation.Populated = false;
    entities.ObligationType.Populated = false;

    return ReadEach("ReadCsePersonObligationObligationType",
      (db, command) =>
      {
        db.SetString(command, "number1", local.StartObligor.Number);
        db.SetString(command, "number2", local.EndObligor.Number);
      },
      (db, reader) =>
      {
        entities.Obligor1.Number = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 0);
        entities.Obligation.CpaType = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.Obligor1.Populated = true;
        entities.Obligation.Populated = true;
        entities.ObligationType.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePersonSupportedDebtLegalActionPerson()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.Supported1.Populated = false;
    entities.DebtDetail.Populated = false;
    entities.Supported2.Populated = false;
    entities.LegalActionPerson.Populated = false;

    return ReadEach("ReadCsePersonSupportedDebtLegalActionPerson",
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
        entities.Supported1.Number = db.GetString(reader, 0);
        entities.Supported2.CspNumber = db.GetString(reader, 0);
        entities.DebtDetail.CspSupNumber = db.GetNullableString(reader, 0);
        entities.DebtDetail.CspSupNumber = db.GetNullableString(reader, 0);
        entities.Supported2.Type1 = db.GetString(reader, 1);
        entities.DebtDetail.CpaSupType = db.GetNullableString(reader, 1);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.DebtDetail.CspNumber = db.GetString(reader, 3);
        entities.DebtDetail.CpaType = db.GetString(reader, 4);
        entities.DebtDetail.SystemGeneratedIdentifier = db.GetInt32(reader, 5);
        entities.DebtDetail.Type1 = db.GetString(reader, 6);
        entities.DebtDetail.Amount = db.GetDecimal(reader, 7);
        entities.DebtDetail.CreatedBy = db.GetString(reader, 8);
        entities.DebtDetail.DebtType = db.GetString(reader, 9);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 10);
        entities.DebtDetail.LapId = db.GetNullableInt32(reader, 11);
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 11);
        entities.Supported1.Populated = true;
        entities.DebtDetail.Populated = true;
        entities.Supported2.Populated = true;
        entities.LegalActionPerson.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.Supported2.Type1);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.DebtDetail.CpaSupType);
        CheckValid<ObligationTransaction>("CpaType", entities.DebtDetail.CpaType);
          
        CheckValid<ObligationTransaction>("Type1", entities.DebtDetail.Type1);
        CheckValid<ObligationTransaction>("DebtType",
          entities.DebtDetail.DebtType);

        return true;
      });
  }

  private bool ReadDebt1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.AccrualInstructions.Populated = false;

    return Read("ReadDebt1",
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
        entities.AccrualInstructions.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.AccrualInstructions.CspNumber = db.GetString(reader, 1);
        entities.AccrualInstructions.CpaType = db.GetString(reader, 2);
        entities.AccrualInstructions.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.AccrualInstructions.Type1 = db.GetString(reader, 4);
        entities.AccrualInstructions.DebtType = db.GetString(reader, 5);
        entities.AccrualInstructions.CspSupNumber =
          db.GetNullableString(reader, 6);
        entities.AccrualInstructions.CpaSupType =
          db.GetNullableString(reader, 7);
        entities.AccrualInstructions.OtyType = db.GetInt32(reader, 8);
        entities.AccrualInstructions.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.AccrualInstructions.CpaType);
        CheckValid<ObligationTransaction>("Type1",
          entities.AccrualInstructions.Type1);
        CheckValid<ObligationTransaction>("DebtType",
          entities.AccrualInstructions.DebtType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.AccrualInstructions.CpaSupType);
      });
  }

  private bool ReadDebt2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    System.Diagnostics.Debug.Assert(entities.Supported2.Populated);

    return Read("ReadDebt2",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetNullableString(command, "cpaSupType", entities.Supported2.Type1);
        db.SetNullableString(
          command, "cspSupNumber", entities.Supported2.CspNumber);
      },
      (db, reader) =>
      {
        local.Common.Count = db.GetInt32(reader, 0);
      });
  }

  private bool ReadDebt3()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    System.Diagnostics.Debug.Assert(entities.Supported2.Populated);
    entities.AccrualInstructions.Populated = false;

    return Read("ReadDebt3",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetNullableString(command, "cpaSupType", entities.Supported2.Type1);
        db.SetNullableString(
          command, "cspSupNumber", entities.Supported2.CspNumber);
      },
      (db, reader) =>
      {
        entities.AccrualInstructions.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.AccrualInstructions.CspNumber = db.GetString(reader, 1);
        entities.AccrualInstructions.CpaType = db.GetString(reader, 2);
        entities.AccrualInstructions.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.AccrualInstructions.Type1 = db.GetString(reader, 4);
        entities.AccrualInstructions.DebtType = db.GetString(reader, 5);
        entities.AccrualInstructions.CspSupNumber =
          db.GetNullableString(reader, 6);
        entities.AccrualInstructions.CpaSupType =
          db.GetNullableString(reader, 7);
        entities.AccrualInstructions.OtyType = db.GetInt32(reader, 8);
        entities.AccrualInstructions.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.AccrualInstructions.CpaType);
        CheckValid<ObligationTransaction>("Type1",
          entities.AccrualInstructions.Type1);
        CheckValid<ObligationTransaction>("DebtType",
          entities.AccrualInstructions.DebtType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.AccrualInstructions.CpaSupType);
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
        entities.ObligationPaymentSchedule.LastUpdateBy =
          db.GetNullableString(reader, 6);
        entities.ObligationPaymentSchedule.LastUpdateTmst =
          db.GetNullableDateTime(reader, 7);
        entities.ObligationPaymentSchedule.Populated = true;
        CheckValid<ObligationPaymentSchedule>("ObgCpaType",
          entities.ObligationPaymentSchedule.ObgCpaType);
      });
  }

  private bool ReadObligationTransactionRln()
  {
    System.Diagnostics.Debug.Assert(entities.AccrualInstructions.Populated);
    System.Diagnostics.Debug.Assert(entities.DebtDetail.Populated);
    entities.ObligationTransactionRln.Populated = false;

    return Read("ReadObligationTransactionRln",
      (db, command) =>
      {
        db.SetInt32(
          command, "otyTypePrimary", entities.AccrualInstructions.OtyType);
        db.SetString(command, "otrPType", entities.AccrualInstructions.Type1);
        db.SetInt32(
          command, "otrPGeneratedId",
          entities.AccrualInstructions.SystemGeneratedIdentifier);
        db.SetString(command, "cpaPType", entities.AccrualInstructions.CpaType);
        db.SetString(
          command, "cspPNumber", entities.AccrualInstructions.CspNumber);
        db.SetInt32(
          command, "obgPGeneratedId",
          entities.AccrualInstructions.ObgGeneratedId);
        db.SetInt32(command, "otyTypeSecondary", entities.DebtDetail.OtyType);
        db.SetString(command, "otrType", entities.DebtDetail.Type1);
        db.SetInt32(
          command, "otrGeneratedId",
          entities.DebtDetail.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", entities.DebtDetail.CpaType);
        db.SetString(command, "cspNumber", entities.DebtDetail.CspNumber);
        db.SetInt32(
          command, "obgGeneratedId", entities.DebtDetail.ObgGeneratedId);
        db.SetInt32(
          command, "onrGeneratedId",
          entities.Accrual.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ObligationTransactionRln.OnrGeneratedId =
          db.GetInt32(reader, 0);
        entities.ObligationTransactionRln.OtrType = db.GetString(reader, 1);
        entities.ObligationTransactionRln.OtrGeneratedId =
          db.GetInt32(reader, 2);
        entities.ObligationTransactionRln.CpaType = db.GetString(reader, 3);
        entities.ObligationTransactionRln.CspNumber = db.GetString(reader, 4);
        entities.ObligationTransactionRln.ObgGeneratedId =
          db.GetInt32(reader, 5);
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
        entities.ObligationTransactionRln.Populated = true;
        CheckValid<ObligationTransactionRln>("OtrType",
          entities.ObligationTransactionRln.OtrType);
        CheckValid<ObligationTransactionRln>("CpaType",
          entities.ObligationTransactionRln.CpaType);
        CheckValid<ObligationTransactionRln>("OtrPType",
          entities.ObligationTransactionRln.OtrPType);
        CheckValid<ObligationTransactionRln>("CpaPType",
          entities.ObligationTransactionRln.CpaPType);
      });
  }

  private bool ReadObligationTransactionRlnRsn()
  {
    entities.Accrual.Populated = false;

    return Read("ReadObligationTransactionRlnRsn",
      null,
      (db, reader) =>
      {
        entities.Accrual.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Accrual.Populated = true;
      });
  }

  private void UpdateObligationPaymentSchedule()
  {
    System.Diagnostics.Debug.
      Assert(entities.ObligationPaymentSchedule.Populated);

    var endDt = local.Conversion.Date;
    var lastUpdateBy = local.UserId.Text8;
    var lastUpdateTmst = local.Current.Timestamp;

    entities.ObligationPaymentSchedule.Populated = false;
    Update("UpdateObligationPaymentSchedule",
      (db, command) =>
      {
        db.SetNullableDate(command, "endDt", endDt);
        db.SetNullableString(command, "lastUpdateBy", lastUpdateBy);
        db.SetNullableDateTime(command, "lastUpdateTmst", lastUpdateTmst);
        db.SetInt32(
          command, "otyType", entities.ObligationPaymentSchedule.OtyType);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ObligationPaymentSchedule.ObgGeneratedId);
        db.SetString(
          command, "obgCspNumber",
          entities.ObligationPaymentSchedule.ObgCspNumber);
        db.SetString(
          command, "obgCpaType", entities.ObligationPaymentSchedule.ObgCpaType);
          
        db.SetDate(
          command, "startDt",
          entities.ObligationPaymentSchedule.StartDt.GetValueOrDefault());
      });

    entities.ObligationPaymentSchedule.EndDt = endDt;
    entities.ObligationPaymentSchedule.LastUpdateBy = lastUpdateBy;
    entities.ObligationPaymentSchedule.LastUpdateTmst = lastUpdateTmst;
    entities.ObligationPaymentSchedule.Populated = true;
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
    /// A value of ForCommit.
    /// </summary>
    [JsonPropertyName("forCommit")]
    public External ForCommit
    {
      get => forCommit ??= new();
      set => forCommit = value;
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
    /// A value of CommitFrequency.
    /// </summary>
    [JsonPropertyName("commitFrequency")]
    public Common CommitFrequency
    {
      get => commitFrequency ??= new();
      set => commitFrequency = value;
    }

    /// <summary>
    /// A value of SupPrsnCompare.
    /// </summary>
    [JsonPropertyName("supPrsnCompare")]
    public CsePerson SupPrsnCompare
    {
      get => supPrsnCompare ??= new();
      set => supPrsnCompare = value;
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
    /// A value of TmpDate.
    /// </summary>
    [JsonPropertyName("tmpDate")]
    public TextWorkArea TmpDate
    {
      get => tmpDate ??= new();
      set => tmpDate = value;
    }

    /// <summary>
    /// A value of Conversion.
    /// </summary>
    [JsonPropertyName("conversion")]
    public DateWorkArea Conversion
    {
      get => conversion ??= new();
      set => conversion = value;
    }

    /// <summary>
    /// A value of DebtFound.
    /// </summary>
    [JsonPropertyName("debtFound")]
    public Common DebtFound
    {
      get => debtFound ??= new();
      set => debtFound = value;
    }

    /// <summary>
    /// A value of ProcessCntAll.
    /// </summary>
    [JsonPropertyName("processCntAll")]
    public Common ProcessCntAll
    {
      get => processCntAll ??= new();
      set => processCntAll = value;
    }

    /// <summary>
    /// A value of ProcessCntRlnOnly.
    /// </summary>
    [JsonPropertyName("processCntRlnOnly")]
    public Common ProcessCntRlnOnly
    {
      get => processCntRlnOnly ??= new();
      set => processCntRlnOnly = value;
    }

    /// <summary>
    /// A value of StartObligor.
    /// </summary>
    [JsonPropertyName("startObligor")]
    public CsePerson StartObligor
    {
      get => startObligor ??= new();
      set => startObligor = value;
    }

    /// <summary>
    /// A value of EndObligor.
    /// </summary>
    [JsonPropertyName("endObligor")]
    public CsePerson EndObligor
    {
      get => endObligor ??= new();
      set => endObligor = value;
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

    /// <summary>
    /// A value of CreateLoop.
    /// </summary>
    [JsonPropertyName("createLoop")]
    public Common CreateLoop
    {
      get => createLoop ??= new();
      set => createLoop = value;
    }

    /// <summary>
    /// A value of NoOfObligationsRead.
    /// </summary>
    [JsonPropertyName("noOfObligationsRead")]
    public Common NoOfObligationsRead
    {
      get => noOfObligationsRead ??= new();
      set => noOfObligationsRead = value;
    }

    /// <summary>
    /// A value of NoOfDebtDtlDebtsRead.
    /// </summary>
    [JsonPropertyName("noOfDebtDtlDebtsRead")]
    public Common NoOfDebtDtlDebtsRead
    {
      get => noOfDebtDtlDebtsRead ??= new();
      set => noOfDebtDtlDebtsRead = value;
    }

    /// <summary>
    /// A value of NoOfAiAddedRlnOnly.
    /// </summary>
    [JsonPropertyName("noOfAiAddedRlnOnly")]
    public Common NoOfAiAddedRlnOnly
    {
      get => noOfAiAddedRlnOnly ??= new();
      set => noOfAiAddedRlnOnly = value;
    }

    /// <summary>
    /// A value of NoOfAiAddedAll.
    /// </summary>
    [JsonPropertyName("noOfAiAddedAll")]
    public Common NoOfAiAddedAll
    {
      get => noOfAiAddedAll ??= new();
      set => noOfAiAddedAll = value;
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

    private External forCommit;
    private Common commitCnt;
    private Common commitFrequency;
    private CsePerson supPrsnCompare;
    private Common common;
    private TextWorkArea tmpDate;
    private DateWorkArea conversion;
    private Common debtFound;
    private Common processCntAll;
    private Common processCntRlnOnly;
    private CsePerson startObligor;
    private CsePerson endObligor;
    private External external;
    private ProgramProcessingInfo programProcessingInfo;
    private Common createLoop;
    private Common noOfObligationsRead;
    private Common noOfDebtDtlDebtsRead;
    private Common noOfAiAddedRlnOnly;
    private Common noOfAiAddedAll;
    private TextWorkArea userId;
    private DateWorkArea current;
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
    /// A value of Supported1.
    /// </summary>
    [JsonPropertyName("supported1")]
    public CsePerson Supported1
    {
      get => supported1 ??= new();
      set => supported1 = value;
    }

    /// <summary>
    /// A value of Obligor1.
    /// </summary>
    [JsonPropertyName("obligor1")]
    public CsePerson Obligor1
    {
      get => obligor1 ??= new();
      set => obligor1 = value;
    }

    /// <summary>
    /// A value of Obligor2.
    /// </summary>
    [JsonPropertyName("obligor2")]
    public CsePersonAccount Obligor2
    {
      get => obligor2 ??= new();
      set => obligor2 = value;
    }

    /// <summary>
    /// A value of NewAccrualInstructions1.
    /// </summary>
    [JsonPropertyName("newAccrualInstructions1")]
    public AccrualInstructions NewAccrualInstructions1
    {
      get => newAccrualInstructions1 ??= new();
      set => newAccrualInstructions1 = value;
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
    public ObligationTransaction AccrualInstructions
    {
      get => accrualInstructions ??= new();
      set => accrualInstructions = value;
    }

    /// <summary>
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public ObligationTransaction DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
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
    /// A value of Supported2.
    /// </summary>
    [JsonPropertyName("supported2")]
    public CsePersonAccount Supported2
    {
      get => supported2 ??= new();
      set => supported2 = value;
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
    /// A value of NewAccrualInstructions2.
    /// </summary>
    [JsonPropertyName("newAccrualInstructions2")]
    public ObligationTransaction NewAccrualInstructions2
    {
      get => newAccrualInstructions2 ??= new();
      set => newAccrualInstructions2 = value;
    }

    /// <summary>
    /// A value of Accrual.
    /// </summary>
    [JsonPropertyName("accrual")]
    public ObligationTransactionRlnRsn Accrual
    {
      get => accrual ??= new();
      set => accrual = value;
    }

    /// <summary>
    /// A value of NewObligationTransactionRln.
    /// </summary>
    [JsonPropertyName("newObligationTransactionRln")]
    public ObligationTransactionRln NewObligationTransactionRln
    {
      get => newObligationTransactionRln ??= new();
      set => newObligationTransactionRln = value;
    }

    private CsePerson supported1;
    private CsePerson obligor1;
    private CsePersonAccount obligor2;
    private AccrualInstructions newAccrualInstructions1;
    private Obligation obligation;
    private ObligationType obligationType;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private ObligationTransaction accrualInstructions;
    private ObligationTransaction debtDetail;
    private ObligationTransactionRln obligationTransactionRln;
    private CsePersonAccount supported2;
    private LegalActionPerson legalActionPerson;
    private ObligationTransaction newAccrualInstructions2;
    private ObligationTransactionRlnRsn accrual;
    private ObligationTransactionRln newObligationTransactionRln;
  }
#endregion
}
