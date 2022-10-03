// Program: FN_B665_DEBT_COURT_SUM_REPORT, ID: 371130074, model: 746.
// Short name: SWEF665B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B665_DEBT_COURT_SUM_REPORT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB665DebtCourtSumReport: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B665_DEBT_COURT_SUM_REPORT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB665DebtCourtSumReport(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB665DebtCourtSumReport.
  /// </summary>
  public FnB665DebtCourtSumReport(IContext context, Import import, Export export)
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
    // ************************************************
    // Date	 Developer	Description
    // 11/11/99 KRP     	Initial Creation
    // 02/08/01 M. Brown Changed this program so that it can be used by
    // the new online print function. Optimized the code a bit.
    // Also added logic to only include j/s debt activity once.
    // ************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.HardcodeJointAndSeveral.PrimarySecondaryCode = "J";
    local.HardcodeConcurrent.SequentialGeneratedIdentifier = 4;
    local.EabReportSend.ProcessDate = Now().Date;
    local.EabReportSend.ProgramName = global.UserId;
    local.EabFileHandling.Action = "OPEN";
    UseCabErrorReport();
    local.EabFileHandling.Action = "WRITE";

    // *****************************************************************
    // Get the SYSIN Parm Values
    // *****************************************************************
    UseFnExtGetParmsThruJclSysin();

    if (local.External.NumericReturnCode != 0)
    {
      local.NeededToWrite.RptDetail = "Error reading sysin parm data.";
      UseCabErrorReport();
      ExitState = "FN0000_SYSIN_PARM_ERROR_A";

      return;
    }

    if (IsEmpty(local.Sysin.ParameterList))
    {
      local.NeededToWrite.RptDetail = "Sysin Parm list is blank";
      UseCabErrorReport();
      ExitState = "FN0000_BLANK_SYSIN_PARM_LIST_AB";

      return;
    }

    local.Job.Name = Substring(local.Sysin.ParameterList, 1, 8);

    if (!ReadJob())
    {
      local.NeededToWrite.RptDetail = "Job Not Found";
      UseCabErrorReport();
      ExitState = "CO0000_JOB_NF_AB";

      return;
    }

    local.JobRun.SystemGenId =
      (int)StringToNumber(Substring(local.Sysin.ParameterList, 10, 9));

    if (!ReadJobRun())
    {
      local.NeededToWrite.RptDetail = "Job Run Not Found";
      UseCabErrorReport();
      ExitState = "CO0000_JOB_RUN_NF_AB";

      return;
    }

    // *****************************************************************
    // Update the Status of the Report in JOB_RUN.
    // *****************************************************************
    // **** SET status TO "PROCESSING"
    try
    {
      UpdateJobRun2();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          local.NeededToWrite.RptDetail = TrimEnd("Job Run Not Unique: ") + "  Parm: " +
            entities.ExistingJobRun.ParmInfo;
          UseCabErrorReport();
          ExitState = "CO0000_JOB_RUN_NU_AB";

          return;
        case ErrorCode.PermittedValueViolation:
          local.NeededToWrite.RptDetail = TrimEnd("Job Run PV: ") + "  Parm: " +
            entities.ExistingJobRun.ParmInfo;
          UseCabErrorReport();
          ExitState = "CO0000_JOB_RUN_NU_AB";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    // : Perform a DB2 Commit to Free Up the JOB_RUN row.
    UseExtToDoACommit();

    if (local.External.NumericReturnCode != 0)
    {
      local.NeededToWrite.RptDetail = "Commit eab returned error: " + NumberToString
        (local.External.NumericReturnCode, 15);
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // *****************************************************************
    // Extract Filters from Parameter Information
    // *****************************************************************
    // **** IF local job_run parameter_information = SPACES ........
    //      ERROR: Invalid Parm Information!!!!
    //      Update JOB_RUN Error Message & Get Out!!!!
    // : Mandatory Parm Values.
    local.ObligorCsePerson.Number =
      Substring(entities.ExistingJobRun.ParmInfo, 1, 10);
    local.LegalAction.StandardNumber =
      Substring(entities.ExistingJobRun.ParmInfo, 12, 20);

    if (ReadCsePerson3())
    {
      MoveCsePerson(entities.Obligor1, local.ObligorCsePerson);
      local.ObligorCsePersonsWorkSet.Number = entities.Obligor1.Number;
      UseSiReadCsePersonBatch1();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        ExitState = "ACO_NN0000_ALL_OK";
        local.ObligorCsePersonsWorkSet.FormattedName = "ERROR: ADABAS problem";
      }
    }
    else
    {
      local.NeededToWrite.RptDetail =
        TrimEnd("Obligor CSE person not found  :") + "   Person #: " + local
        .ObligorCsePerson.Number;
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ===============================================
    // Read all Legal Actions for the input Standard Nbr
    // by Filed Dt where
    // Legal Action Classification = J (Judgement)
    // Filed Dt not low date
    // Read FIPS to get County and State values
    // ===============================================
    foreach(var item in ReadLegalAction())
    {
      local.LegalAction.Assign(entities.LegalAction);

      if (ReadFips())
      {
        MoveFips(entities.Fips, local.Fips);
      }

      // ===============================================
      // Read Respondant Person (only one)
      // Legal Action Person  = R (Respondant)
      // ===============================================
      if (ReadCsePerson2())
      {
        local.Lrespondant.Assign(entities.CsePerson);
        local.Respondant.Number = entities.CsePerson.Number;
        local.ObligorCsePersonsWorkSet.Number = entities.CsePerson.Number;
        UseSiReadCsePersonBatch2();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NN0000_ALL_OK";
          local.Respondant.FormattedName = "ERROR: ADABAS problem";
        }
      }
      else
      {
        local.Respondant.LastName = "NO RESPONDANT";
        local.PetitionerCommon.Count = 0;
      }

      // ===============================================
      // Read Petitioner Person (First one)
      // Legal Action Person  = P (Petitioner)
      // ===============================================
      if (ReadCsePerson1())
      {
        local.PetitionerCsePerson.Assign(entities.CsePerson);
        local.FirstPetitioner.Assign(entities.CsePerson);
        local.PetitionerCsePersonsWorkSet.Number = entities.CsePerson.Number;
        UseSiReadCsePersonBatch3();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NN0000_ALL_OK";
          local.PetitionerCsePersonsWorkSet.FormattedName =
            "ERROR: ADABAS problem";
        }

        local.PetitionerCommon.Count = 1;
      }
      else
      {
        local.PetitionerCsePersonsWorkSet.LastName = "NO PETITIONER";
        local.PetitionerCommon.Count = 0;
      }

      local.Common.ActionEntry = "01";
      UseFnDebtEabWriteRecords3();

      // ===============================================
      // Multiple Petitioners are possible
      // Read Each Petitioner Person (Mutliple)
      // Legal Action Person  = P (Petitioner)
      // and not = to first Petitioner found
      // ===============================================
      if (local.PetitionerCommon.Count == 1)
      {
        local.Lrespondant.Assign(local.LnullCsePerson);
        local.Respondant.Assign(local.LnullCsePersonsWorkSet);

        foreach(var item1 in ReadCsePerson4())
        {
          local.PetitionerCsePerson.Assign(entities.CsePerson);
          local.PetitionerCsePersonsWorkSet.Number = entities.CsePerson.Number;
          UseSiReadCsePersonBatch3();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NN0000_ALL_OK";
            local.PetitionerCsePersonsWorkSet.FormattedName =
              "ERROR: ADABAS problem";
          }

          local.Common.ActionEntry = "02";
          UseFnDebtEabWriteRecords7();
        }
      }

      // ===============================================
      // Read all Legal Action Details and Legal Action Persons
      // by Obligation Type where
      // Legal Action Detail  Detail Type = F (Financial Records)
      // Legal Action Person Account Type = S (Supported Person)
      // Call ADABAS and format the person name
      // ===============================================
      foreach(var item1 in ReadLegalActionDetailObligationType())
      {
        if (Equal(entities.LegalActionDetail.CurrentAmount, 0) && Equal
          (entities.LegalActionDetail.JudgementAmount, 0))
        {
          continue;
        }

        MoveLegalActionDetail(entities.LegalActionDetail,
          local.LegalActionDetail);
        local.ObligationType.Assign(entities.ObligationType);
        local.ObligationPaymentSchedule.Assign(
          local.NullObligationPaymentSchedule);

        foreach(var item2 in ReadLegalActionPersonCsePerson())
        {
          local.LegalActionPerson.Assign(entities.LegalActionPerson);
          MoveCsePerson(entities.CsePerson, local.ObligorCsePerson);
          local.ObligorCsePersonsWorkSet.Number = entities.CsePerson.Number;
          UseSiReadCsePersonBatch1();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NN0000_ALL_OK";
            local.ObligorCsePersonsWorkSet.Number = "ERROR: ADABAS problem";
          }

          if (AsChar(local.ObligationType.Classification) == 'A')
          {
            if (ReadAccrualInstructions())
            {
              local.LegalActionPerson.EndDate =
                entities.AccrualInstructions.DiscontinueDt;
            }
          }

          local.Common.ActionEntry = "03";
          UseFnDebtEabWriteRecords5();
        }

        // ===============================================
        // Read all Legal Action Details and Legal Action Persons
        // by Obligation Type where
        // Legal Action Detail  Detail Type = F (Financial Records)
        // Legal Action Person Account Type = S (Supported Person)
        // Call ADABAS and format the person name
        // ===============================================
      }

      // ===============================================
      // Read all Debt Details (Amt Due) for all obligations associated with
      // the current legal action.
      // ===============================================
      foreach(var item1 in ReadObligationTransactionObligationObligationType1())
      {
        if (AsChar(entities.Obligation.PrimarySecondaryCode) == AsChar
          (local.HardcodeJointAndSeveral.PrimarySecondaryCode))
        {
          if (!ReadObligationRlnRsn())
          {
            continue;
          }
        }

        local.ObligationType.Assign(entities.ObligationType);
        MoveObligation(entities.Obligation, local.Obligation);
        local.ObligationTransaction.Assign(entities.ObligationTransaction);
        local.DebtDetail.Assign(entities.DebtDetail);
        local.Common.ActionEntry = "05";
        UseFnDebtEabWriteRecords6();
      }

      foreach(var item1 in ReadObligationTransaction())
      {
        local.Common.ActionEntry = "06";
        UseFnDebtEabWriteRecords1();
      }

      // ===============================================
      // Read all Collections (Amt Paid), set record type depending
      // on whether or not the collection was adjusted.
      // ===============================================
      foreach(var item1 in ReadObligationTransactionObligationObligationType2())
      {
        if (AsChar(entities.Obligation.PrimarySecondaryCode) == AsChar
          (local.HardcodeJointAndSeveral.PrimarySecondaryCode))
        {
          if (!ReadObligationRlnRsn())
          {
            continue;
          }
        }

        local.ObligationType.Assign(entities.ObligationType);
        MoveObligation(entities.Obligation, local.Obligation);
        local.ObligationTransaction.Assign(entities.ObligationTransaction);
        local.Collection.Assign(entities.Collection);

        if (AsChar(entities.Collection.AdjustedInd) == 'N')
        {
          local.Common.ActionEntry = "07";
        }
        else
        {
          local.Common.ActionEntry = "08";
        }

        UseFnDebtEabWriteRecords4();
      }
    }

    // *****************************************************************
    // Update the Status of the Report in JOB_RUN.
    // *****************************************************************
    if (Equal(entities.ExistingJobRun.OutputType, "ONLINE"))
    {
      try
      {
        UpdateJobRun1();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "CO0000_JOB_RUN_NU_AB";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "CO0000_JOB_RUN_PV_AB";

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
      try
      {
        UpdateJobRun3();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "CO0000_JOB_RUN_NU_AB";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "CO0000_JOB_RUN_PV_AB";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    local.EabFileHandling.Action = "CLOSE";
    UseFnDebtEabWriteRecords2();
    UseCabErrorReport();
    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
  }

  private static void MoveEabFileHandling(EabFileHandling source,
    EabFileHandling target)
  {
    target.Action = source.Action;
    target.Status = source.Status;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveFips(Fips source, Fips target)
  {
    target.StateAbbreviation = source.StateAbbreviation;
    target.CountyDescription = source.CountyDescription;
  }

  private static void MoveLegalActionDetail(LegalActionDetail source,
    LegalActionDetail target)
  {
    target.FreqPeriodCode = source.FreqPeriodCode;
    target.DayOfWeek = source.DayOfWeek;
    target.DayOfMonth1 = source.DayOfMonth1;
    target.DayOfMonth2 = source.DayOfMonth2;
    target.PeriodInd = source.PeriodInd;
    target.Number = source.Number;
    target.DetailType = source.DetailType;
    target.EndDate = source.EndDate;
    target.EffectiveDate = source.EffectiveDate;
    target.BondAmount = source.BondAmount;
    target.ArrearsAmount = source.ArrearsAmount;
    target.CurrentAmount = source.CurrentAmount;
    target.JudgementAmount = source.JudgementAmount;
  }

  private static void MoveObligation(Obligation source, Obligation target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.OrderTypeCode = source.OrderTypeCode;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.External.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.External.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseFnDebtEabWriteRecords1()
  {
    var useImport = new FnDebtEabWriteRecords.Import();
    var useExport = new FnDebtEabWriteRecords.Export();

    useImport.IobligationTransaction.Assign(entities.DebtAdjustment);
    MoveEabFileHandling(local.EabFileHandling, useImport.IeabFileHandling);
    useImport.IlegalAction.Assign(local.LegalAction);
    useImport.Icommon.ActionEntry = local.Common.ActionEntry;
    useImport.IobligationType.Assign(local.ObligationType);
    MoveObligation(local.Obligation, useImport.Iobligation);
    useImport.IdebtDetail.Assign(local.DebtDetail);

    Call(FnDebtEabWriteRecords.Execute, useImport, useExport);

    MoveEabFileHandling(useImport.IeabFileHandling, local.EabFileHandling);
  }

  private void UseFnDebtEabWriteRecords2()
  {
    var useImport = new FnDebtEabWriteRecords.Import();
    var useExport = new FnDebtEabWriteRecords.Export();

    MoveEabFileHandling(local.EabFileHandling, useImport.IeabFileHandling);

    Call(FnDebtEabWriteRecords.Execute, useImport, useExport);

    MoveEabFileHandling(useImport.IeabFileHandling, local.EabFileHandling);
  }

  private void UseFnDebtEabWriteRecords3()
  {
    var useImport = new FnDebtEabWriteRecords.Import();
    var useExport = new FnDebtEabWriteRecords.Export();

    MoveEabFileHandling(local.EabFileHandling, useImport.IeabFileHandling);
    MoveCsePerson(local.ObligorCsePerson, useImport.IcsePerson);
    useImport.IlegalAction.Assign(local.LegalAction);
    useImport.Iperson.Assign(local.ObligorCsePersonsWorkSet);
    MoveFips(local.Fips, useImport.Ifips);
    useImport.IrespondantCsePersonsWorkSet.Assign(local.Respondant);
    MoveCsePerson(local.Lrespondant, useImport.IrespondantCsePerson);
    useImport.IpetitionerCsePersonsWorkSet.Assign(
      local.PetitionerCsePersonsWorkSet);
    MoveCsePerson(local.PetitionerCsePerson, useImport.IpetitionerCsePerson);
    useImport.Icommon.ActionEntry = local.Common.ActionEntry;

    Call(FnDebtEabWriteRecords.Execute, useImport, useExport);

    MoveEabFileHandling(useImport.IeabFileHandling, local.EabFileHandling);
  }

  private void UseFnDebtEabWriteRecords4()
  {
    var useImport = new FnDebtEabWriteRecords.Import();
    var useExport = new FnDebtEabWriteRecords.Export();

    MoveEabFileHandling(local.EabFileHandling, useImport.IeabFileHandling);
    MoveCsePerson(local.ObligorCsePerson, useImport.IcsePerson);
    useImport.IlegalAction.Assign(local.LegalAction);
    useImport.Iperson.Assign(local.ObligorCsePersonsWorkSet);
    useImport.Icommon.ActionEntry = local.Common.ActionEntry;
    useImport.IlegalActionPerson.Assign(local.LegalActionPerson);
    useImport.IlegalActionDetail.Assign(local.LegalActionDetail);
    useImport.IobligationType.Assign(local.ObligationType);
    MoveObligation(local.Obligation, useImport.Iobligation);
    useImport.IobligationTransaction.Assign(local.ObligationTransaction);
    useImport.Icollection.Assign(local.Collection);

    Call(FnDebtEabWriteRecords.Execute, useImport, useExport);

    MoveEabFileHandling(useImport.IeabFileHandling, local.EabFileHandling);
  }

  private void UseFnDebtEabWriteRecords5()
  {
    var useImport = new FnDebtEabWriteRecords.Import();
    var useExport = new FnDebtEabWriteRecords.Export();

    MoveEabFileHandling(local.EabFileHandling, useImport.IeabFileHandling);
    MoveCsePerson(local.ObligorCsePerson, useImport.IcsePerson);
    useImport.Iperson.Assign(local.ObligorCsePersonsWorkSet);
    useImport.Icommon.ActionEntry = local.Common.ActionEntry;
    useImport.IlegalActionPerson.Assign(local.LegalActionPerson);
    useImport.IlegalActionDetail.Assign(local.LegalActionDetail);
    useImport.IobligationType.Assign(local.ObligationType);

    Call(FnDebtEabWriteRecords.Execute, useImport, useExport);

    MoveEabFileHandling(useImport.IeabFileHandling, local.EabFileHandling);
  }

  private void UseFnDebtEabWriteRecords6()
  {
    var useImport = new FnDebtEabWriteRecords.Import();
    var useExport = new FnDebtEabWriteRecords.Export();

    MoveEabFileHandling(local.EabFileHandling, useImport.IeabFileHandling);
    useImport.IlegalAction.Assign(local.LegalAction);
    useImport.Icommon.ActionEntry = local.Common.ActionEntry;
    useImport.IobligationType.Assign(local.ObligationType);
    MoveObligation(local.Obligation, useImport.Iobligation);
    useImport.IobligationTransaction.Assign(local.ObligationTransaction);
    useImport.IdebtDetail.Assign(local.DebtDetail);

    Call(FnDebtEabWriteRecords.Execute, useImport, useExport);

    MoveEabFileHandling(useImport.IeabFileHandling, local.EabFileHandling);
  }

  private void UseFnDebtEabWriteRecords7()
  {
    var useImport = new FnDebtEabWriteRecords.Import();
    var useExport = new FnDebtEabWriteRecords.Export();

    MoveEabFileHandling(local.EabFileHandling, useImport.IeabFileHandling);
    useImport.IpetitionerCsePersonsWorkSet.Assign(
      local.PetitionerCsePersonsWorkSet);
    MoveCsePerson(local.PetitionerCsePerson, useImport.IpetitionerCsePerson);
    useImport.Icommon.ActionEntry = local.Common.ActionEntry;
    useImport.IlegalActionPerson.Assign(local.LegalActionPerson);
    useImport.IlegalActionDetail.Assign(local.LegalActionDetail);
    useImport.IobligationType.Assign(local.ObligationType);
    MoveObligation(local.Obligation, useImport.Iobligation);

    Call(FnDebtEabWriteRecords.Execute, useImport, useExport);

    MoveEabFileHandling(useImport.IeabFileHandling, local.EabFileHandling);
  }

  private void UseFnExtGetParmsThruJclSysin()
  {
    var useImport = new FnExtGetParmsThruJclSysin.Import();
    var useExport = new FnExtGetParmsThruJclSysin.Export();

    useExport.ProgramProcessingInfo.ParameterList = local.Sysin.ParameterList;
    useExport.External.NumericReturnCode = local.External.NumericReturnCode;

    Call(FnExtGetParmsThruJclSysin.Execute, useImport, useExport);

    local.Sysin.ParameterList = useExport.ProgramProcessingInfo.ParameterList;
    local.External.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseSiReadCsePersonBatch1()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.ObligorCsePersonsWorkSet.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

    local.ObligorCsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseSiReadCsePersonBatch2()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.Respondant.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

    local.Respondant.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseSiReadCsePersonBatch3()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number =
      local.PetitionerCsePersonsWorkSet.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

    local.PetitionerCsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private bool ReadAccrualInstructions()
  {
    entities.AccrualInstructions.Populated = false;

    return Read("ReadAccrualInstructions",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lapId", entities.LegalActionPerson.Identifier);
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
      });
  }

  private bool ReadCsePerson1()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.AeCaseNumber = db.GetNullableString(reader, 2);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 3);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.AeCaseNumber = db.GetNullableString(reader, 2);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 3);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadCsePerson3()
  {
    entities.Obligor1.Populated = false;

    return Read("ReadCsePerson3",
      (db, command) =>
      {
        db.SetString(command, "numb", local.ObligorCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Obligor1.Number = db.GetString(reader, 0);
        entities.Obligor1.Type1 = db.GetString(reader, 1);
        entities.Obligor1.AeCaseNumber = db.GetNullableString(reader, 2);
        entities.Obligor1.OrganizationName = db.GetNullableString(reader, 3);
        entities.Obligor1.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCsePerson4()
  {
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePerson4",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.LegalAction.Identifier);
        db.SetString(command, "numb", local.FirstPetitioner.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.AeCaseNumber = db.GetNullableString(reader, 2);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 3);
        entities.CsePerson.Populated = true;

        return true;
      });
  }

  private bool ReadFips()
  {
    System.Diagnostics.Debug.Assert(entities.LegalAction.Populated);
    entities.Fips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.LegalAction.TrbId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.CountyDescription = db.GetNullableString(reader, 3);
        entities.Fips.StateAbbreviation = db.GetString(reader, 4);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadJob()
  {
    entities.ExistingJob.Populated = false;

    return Read("ReadJob",
      (db, command) =>
      {
        db.SetString(command, "name", local.Job.Name);
      },
      (db, reader) =>
      {
        entities.ExistingJob.Name = db.GetString(reader, 0);
        entities.ExistingJob.Populated = true;
      });
  }

  private bool ReadJobRun()
  {
    entities.ExistingJobRun.Populated = false;

    return Read("ReadJobRun",
      (db, command) =>
      {
        db.SetString(command, "jobName", entities.ExistingJob.Name);
        db.SetInt32(command, "systemGenId", local.JobRun.SystemGenId);
      },
      (db, reader) =>
      {
        entities.ExistingJobRun.StartTimestamp = db.GetDateTime(reader, 0);
        entities.ExistingJobRun.EndTimestamp =
          db.GetNullableDateTime(reader, 1);
        entities.ExistingJobRun.Status = db.GetString(reader, 2);
        entities.ExistingJobRun.OutputType = db.GetString(reader, 3);
        entities.ExistingJobRun.ErrorMsg = db.GetNullableString(reader, 4);
        entities.ExistingJobRun.ParmInfo = db.GetNullableString(reader, 5);
        entities.ExistingJobRun.JobName = db.GetString(reader, 6);
        entities.ExistingJobRun.SystemGenId = db.GetInt32(reader, 7);
        entities.ExistingJobRun.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalAction",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", local.LegalAction.StandardNumber ?? "");
        db.SetNullableDate(
          command, "filedDt",
          local.NullLegalAction.FiledDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 2);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 3);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 4);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 5);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 6);
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionDetailObligationType()
  {
    entities.LegalActionDetail.Populated = false;
    entities.ObligationType.Populated = false;

    return ReadEach("ReadLegalActionDetailObligationType",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 2);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionDetail.BondAmount =
          db.GetNullableDecimal(reader, 4);
        entities.LegalActionDetail.CreatedBy = db.GetString(reader, 5);
        entities.LegalActionDetail.CreatedTstamp = db.GetDateTime(reader, 6);
        entities.LegalActionDetail.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.LegalActionDetail.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 8);
        entities.LegalActionDetail.ArrearsAmount =
          db.GetNullableDecimal(reader, 9);
        entities.LegalActionDetail.CurrentAmount =
          db.GetNullableDecimal(reader, 10);
        entities.LegalActionDetail.JudgementAmount =
          db.GetNullableDecimal(reader, 11);
        entities.LegalActionDetail.Limit = db.GetNullableInt32(reader, 12);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 13);
        entities.LegalActionDetail.FreqPeriodCode =
          db.GetNullableString(reader, 14);
        entities.LegalActionDetail.DayOfWeek = db.GetNullableInt32(reader, 15);
        entities.LegalActionDetail.DayOfMonth1 =
          db.GetNullableInt32(reader, 16);
        entities.LegalActionDetail.DayOfMonth2 =
          db.GetNullableInt32(reader, 17);
        entities.LegalActionDetail.PeriodInd = db.GetNullableString(reader, 18);
        entities.LegalActionDetail.Description = db.GetString(reader, 19);
        entities.LegalActionDetail.OtyId = db.GetNullableInt32(reader, 20);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 20);
        entities.ObligationType.Code = db.GetString(reader, 21);
        entities.ObligationType.Name = db.GetString(reader, 22);
        entities.ObligationType.Classification = db.GetString(reader, 23);
        entities.ObligationType.SupportedPersonReqInd =
          db.GetString(reader, 24);
        entities.LegalActionDetail.Populated = true;
        entities.ObligationType.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionPersonCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.CsePerson.Populated = false;
    entities.LegalActionPerson.Populated = false;

    return ReadEach("ReadLegalActionPersonCsePerson",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladRNumber", entities.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.LegalActionDetail.LgaIdentifier);
      },
      (db, reader) =>
      {
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.LegalActionPerson.LgaIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.LegalActionPerson.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionPerson.Role = db.GetString(reader, 4);
        entities.LegalActionPerson.EndDate = db.GetNullableDate(reader, 5);
        entities.LegalActionPerson.EndReason = db.GetNullableString(reader, 6);
        entities.LegalActionPerson.CreatedTstamp = db.GetDateTime(reader, 7);
        entities.LegalActionPerson.CreatedBy = db.GetString(reader, 8);
        entities.LegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 9);
        entities.LegalActionPerson.LadRNumber = db.GetNullableInt32(reader, 10);
        entities.LegalActionPerson.AccountType =
          db.GetNullableString(reader, 11);
        entities.LegalActionPerson.ArrearsAmount =
          db.GetNullableDecimal(reader, 12);
        entities.LegalActionPerson.CurrentAmount =
          db.GetNullableDecimal(reader, 13);
        entities.LegalActionPerson.JudgementAmount =
          db.GetNullableDecimal(reader, 14);
        entities.CsePerson.Type1 = db.GetString(reader, 15);
        entities.CsePerson.AeCaseNumber = db.GetNullableString(reader, 16);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 17);
        entities.CsePerson.Populated = true;
        entities.LegalActionPerson.Populated = true;

        return true;
      });
  }

  private bool ReadObligationRlnRsn()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationRlnRsn.Populated = false;

    return Read("ReadObligationRlnRsn",
      (db, command) =>
      {
        db.SetInt32(command, "otyFirstId", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgFGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspFNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaFType", entities.Obligation.CpaType);
        db.SetInt32(
          command, "obRlnRsnId",
          local.HardcodeConcurrent.SequentialGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ObligationRlnRsn.SequentialGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationRlnRsn.Populated = true;
      });
  }

  private IEnumerable<bool> ReadObligationTransaction()
  {
    entities.DebtAdjustment.Populated = false;

    return ReadEach("ReadObligationTransaction",
      (db, command) =>
      {
        db.SetNullableInt32(command, "lgaId", entities.LegalAction.Identifier);
        db.SetString(command, "cspNumber", entities.Obligor1.Number);
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
        entities.DebtAdjustment.DebtAdjustmentInd = db.GetString(reader, 6);
        entities.DebtAdjustment.DebtAdjustmentType = db.GetString(reader, 7);
        entities.DebtAdjustment.DebtAdjustmentDt = db.GetDate(reader, 8);
        entities.DebtAdjustment.CreatedBy = db.GetString(reader, 9);
        entities.DebtAdjustment.CreatedTmst = db.GetDateTime(reader, 10);
        entities.DebtAdjustment.LastUpdatedBy =
          db.GetNullableString(reader, 11);
        entities.DebtAdjustment.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 12);
        entities.DebtAdjustment.DebtType = db.GetString(reader, 13);
        entities.DebtAdjustment.VoluntaryPercentageAmount =
          db.GetNullableInt32(reader, 14);
        entities.DebtAdjustment.CspSupNumber = db.GetNullableString(reader, 15);
        entities.DebtAdjustment.CpaSupType = db.GetNullableString(reader, 16);
        entities.DebtAdjustment.OtyType = db.GetInt32(reader, 17);
        entities.DebtAdjustment.DebtAdjustmentProcessDate =
          db.GetDate(reader, 18);
        entities.DebtAdjustment.DebtAdjCollAdjProcReqInd =
          db.GetString(reader, 19);
        entities.DebtAdjustment.DebtAdjCollAdjProcDt =
          db.GetNullableDate(reader, 20);
        entities.DebtAdjustment.ReasonCode = db.GetString(reader, 21);
        entities.DebtAdjustment.NewDebtProcessDate =
          db.GetNullableDate(reader, 22);
        entities.DebtAdjustment.ReverseCollectionsInd =
          db.GetNullableString(reader, 23);
        entities.DebtAdjustment.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadObligationTransactionObligationObligationType1()
  {
    entities.ObligationType.Populated = false;
    entities.ObligationTransaction.Populated = false;
    entities.Obligation.Populated = false;
    entities.DebtDetail.Populated = false;

    return ReadEach("ReadObligationTransactionObligationObligationType1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Obligor1.Number);
        db.SetNullableInt32(command, "lgaId", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 1);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 2);
        entities.Obligation.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 4);
        entities.ObligationTransaction.Amount = db.GetDecimal(reader, 5);
        entities.ObligationTransaction.DebtAdjustmentInd =
          db.GetString(reader, 6);
        entities.ObligationTransaction.DebtAdjustmentType =
          db.GetString(reader, 7);
        entities.ObligationTransaction.DebtAdjustmentDt = db.GetDate(reader, 8);
        entities.ObligationTransaction.CreatedBy = db.GetString(reader, 9);
        entities.ObligationTransaction.CreatedTmst = db.GetDateTime(reader, 10);
        entities.ObligationTransaction.LastUpdatedBy =
          db.GetNullableString(reader, 11);
        entities.ObligationTransaction.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 12);
        entities.ObligationTransaction.DebtType = db.GetString(reader, 13);
        entities.ObligationTransaction.VoluntaryPercentageAmount =
          db.GetNullableInt32(reader, 14);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 15);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 16);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 17);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 17);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 17);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 17);
        entities.ObligationTransaction.DebtAdjustmentProcessDate =
          db.GetDate(reader, 18);
        entities.ObligationTransaction.DebtAdjCollAdjProcReqInd =
          db.GetString(reader, 19);
        entities.ObligationTransaction.DebtAdjCollAdjProcDt =
          db.GetNullableDate(reader, 20);
        entities.ObligationTransaction.ReasonCode = db.GetString(reader, 21);
        entities.ObligationTransaction.LapId = db.GetNullableInt32(reader, 22);
        entities.ObligationTransaction.NewDebtProcessDate =
          db.GetNullableDate(reader, 23);
        entities.ObligationTransaction.ReverseCollectionsInd =
          db.GetNullableString(reader, 24);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 25);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 26);
        entities.Obligation.LastUpdatedBy = db.GetNullableString(reader, 27);
        entities.Obligation.LastUpdateTmst = db.GetNullableDateTime(reader, 28);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 29);
        entities.Obligation.DelinquentInd = db.GetNullableString(reader, 30);
        entities.ObligationType.Code = db.GetString(reader, 31);
        entities.ObligationType.Name = db.GetString(reader, 32);
        entities.ObligationType.Classification = db.GetString(reader, 33);
        entities.ObligationType.SupportedPersonReqInd =
          db.GetString(reader, 34);
        entities.DebtDetail.DueDt = db.GetDate(reader, 35);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 36);
        entities.DebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 37);
        entities.DebtDetail.AdcDt = db.GetNullableDate(reader, 38);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 39);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 40);
        entities.DebtDetail.CoveredPrdEndDt = db.GetNullableDate(reader, 41);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 42);
        entities.DebtDetail.CreatedTmst = db.GetDateTime(reader, 43);
        entities.ObligationType.Populated = true;
        entities.ObligationTransaction.Populated = true;
        entities.Obligation.Populated = true;
        entities.DebtDetail.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadObligationTransactionObligationObligationType2()
  {
    entities.ObligationType.Populated = false;
    entities.ObligationTransaction.Populated = false;
    entities.Obligation.Populated = false;
    entities.Collection.Populated = false;

    return ReadEach("ReadObligationTransactionObligationObligationType2",
      (db, command) =>
      {
        db.SetNullableInt32(command, "lgaId", entities.LegalAction.Identifier);
        db.SetString(command, "cspNumber", entities.Obligor1.Number);
      },
      (db, reader) =>
      {
        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.ObgId = db.GetInt32(reader, 0);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 1);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Collection.CspNumber = db.GetString(reader, 1);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 2);
        entities.Obligation.CpaType = db.GetString(reader, 2);
        entities.Collection.CpaType = db.GetString(reader, 2);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.Collection.OtrId = db.GetInt32(reader, 3);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 4);
        entities.Collection.OtrType = db.GetString(reader, 4);
        entities.ObligationTransaction.Amount = db.GetDecimal(reader, 5);
        entities.ObligationTransaction.DebtAdjustmentInd =
          db.GetString(reader, 6);
        entities.ObligationTransaction.DebtAdjustmentType =
          db.GetString(reader, 7);
        entities.ObligationTransaction.DebtAdjustmentDt = db.GetDate(reader, 8);
        entities.ObligationTransaction.CreatedBy = db.GetString(reader, 9);
        entities.ObligationTransaction.CreatedTmst = db.GetDateTime(reader, 10);
        entities.ObligationTransaction.LastUpdatedBy =
          db.GetNullableString(reader, 11);
        entities.ObligationTransaction.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 12);
        entities.ObligationTransaction.DebtType = db.GetString(reader, 13);
        entities.ObligationTransaction.VoluntaryPercentageAmount =
          db.GetNullableInt32(reader, 14);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 15);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 16);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 17);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 17);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 17);
        entities.Collection.OtyId = db.GetInt32(reader, 17);
        entities.ObligationTransaction.DebtAdjustmentProcessDate =
          db.GetDate(reader, 18);
        entities.ObligationTransaction.DebtAdjCollAdjProcReqInd =
          db.GetString(reader, 19);
        entities.ObligationTransaction.DebtAdjCollAdjProcDt =
          db.GetNullableDate(reader, 20);
        entities.ObligationTransaction.ReasonCode = db.GetString(reader, 21);
        entities.ObligationTransaction.LapId = db.GetNullableInt32(reader, 22);
        entities.ObligationTransaction.NewDebtProcessDate =
          db.GetNullableDate(reader, 23);
        entities.ObligationTransaction.ReverseCollectionsInd =
          db.GetNullableString(reader, 24);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 25);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 26);
        entities.Obligation.LastUpdatedBy = db.GetNullableString(reader, 27);
        entities.Obligation.LastUpdateTmst = db.GetNullableDateTime(reader, 28);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 29);
        entities.Obligation.DelinquentInd = db.GetNullableString(reader, 30);
        entities.ObligationType.Code = db.GetString(reader, 31);
        entities.ObligationType.Name = db.GetString(reader, 32);
        entities.ObligationType.Classification = db.GetString(reader, 33);
        entities.ObligationType.SupportedPersonReqInd =
          db.GetString(reader, 34);
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 35);
        entities.Collection.AppliedToCode = db.GetString(reader, 36);
        entities.Collection.CollectionDt = db.GetDate(reader, 37);
        entities.Collection.DisbursementDt = db.GetNullableDate(reader, 38);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 39);
        entities.Collection.ConcurrentInd = db.GetString(reader, 40);
        entities.Collection.DisbursementAdjProcessDate = db.GetDate(reader, 41);
        entities.Collection.CrtType = db.GetInt32(reader, 42);
        entities.Collection.CstId = db.GetInt32(reader, 43);
        entities.Collection.CrvId = db.GetInt32(reader, 44);
        entities.Collection.CrdId = db.GetInt32(reader, 45);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 46);
        entities.Collection.CollectionAdjProcessDate = db.GetDate(reader, 47);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 48);
        entities.Collection.Amount = db.GetDecimal(reader, 49);
        entities.Collection.DisbursementProcessingNeedInd =
          db.GetNullableString(reader, 50);
        entities.Collection.DistributionMethod = db.GetString(reader, 51);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 52);
        entities.Collection.AppliedToOrderTypeCode = db.GetString(reader, 53);
        entities.ObligationType.Populated = true;
        entities.ObligationTransaction.Populated = true;
        entities.Obligation.Populated = true;
        entities.Collection.Populated = true;

        return true;
      });
  }

  private void UpdateJobRun1()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingJobRun.Populated);

    var status = "COMPLETE";

    entities.ExistingJobRun.Populated = false;
    Update("UpdateJobRun1",
      (db, command) =>
      {
        db.SetString(command, "status", status);
        db.SetString(command, "jobName", entities.ExistingJobRun.JobName);
        db.
          SetInt32(command, "systemGenId", entities.ExistingJobRun.SystemGenId);
          
      });

    entities.ExistingJobRun.Status = status;
    entities.ExistingJobRun.Populated = true;
  }

  private void UpdateJobRun2()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingJobRun.Populated);

    var status = "PROCESSING";

    entities.ExistingJobRun.Populated = false;
    Update("UpdateJobRun2",
      (db, command) =>
      {
        db.SetString(command, "status", status);
        db.SetString(command, "jobName", entities.ExistingJobRun.JobName);
        db.
          SetInt32(command, "systemGenId", entities.ExistingJobRun.SystemGenId);
          
      });

    entities.ExistingJobRun.Status = status;
    entities.ExistingJobRun.Populated = true;
  }

  private void UpdateJobRun3()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingJobRun.Populated);

    var status = "WAIT";

    entities.ExistingJobRun.Populated = false;
    Update("UpdateJobRun3",
      (db, command) =>
      {
        db.SetString(command, "status", status);
        db.SetString(command, "jobName", entities.ExistingJobRun.JobName);
        db.
          SetInt32(command, "systemGenId", entities.ExistingJobRun.SystemGenId);
          
      });

    entities.ExistingJobRun.Status = status;
    entities.ExistingJobRun.Populated = true;
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
    /// A value of HardcodeJointAndSeveral.
    /// </summary>
    [JsonPropertyName("hardcodeJointAndSeveral")]
    public Obligation HardcodeJointAndSeveral
    {
      get => hardcodeJointAndSeveral ??= new();
      set => hardcodeJointAndSeveral = value;
    }

    /// <summary>
    /// A value of HardcodeConcurrent.
    /// </summary>
    [JsonPropertyName("hardcodeConcurrent")]
    public ObligationRlnRsn HardcodeConcurrent
    {
      get => hardcodeConcurrent ??= new();
      set => hardcodeConcurrent = value;
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
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
    }

    /// <summary>
    /// A value of NeededToOpen.
    /// </summary>
    [JsonPropertyName("neededToOpen")]
    public EabReportSend NeededToOpen
    {
      get => neededToOpen ??= new();
      set => neededToOpen = value;
    }

    /// <summary>
    /// A value of Sysin.
    /// </summary>
    [JsonPropertyName("sysin")]
    public ProgramProcessingInfo Sysin
    {
      get => sysin ??= new();
      set => sysin = value;
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
    /// A value of Job.
    /// </summary>
    [JsonPropertyName("job")]
    public Job Job
    {
      get => job ??= new();
      set => job = value;
    }

    /// <summary>
    /// A value of JobRun.
    /// </summary>
    [JsonPropertyName("jobRun")]
    public JobRun JobRun
    {
      get => jobRun ??= new();
      set => jobRun = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of ObligorCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("obligorCsePersonsWorkSet")]
    public CsePersonsWorkSet ObligorCsePersonsWorkSet
    {
      get => obligorCsePersonsWorkSet ??= new();
      set => obligorCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of Respondant.
    /// </summary>
    [JsonPropertyName("respondant")]
    public CsePersonsWorkSet Respondant
    {
      get => respondant ??= new();
      set => respondant = value;
    }

    /// <summary>
    /// A value of PetitionerCommon.
    /// </summary>
    [JsonPropertyName("petitionerCommon")]
    public Common PetitionerCommon
    {
      get => petitionerCommon ??= new();
      set => petitionerCommon = value;
    }

    /// <summary>
    /// A value of Lrespondant.
    /// </summary>
    [JsonPropertyName("lrespondant")]
    public CsePerson Lrespondant
    {
      get => lrespondant ??= new();
      set => lrespondant = value;
    }

    /// <summary>
    /// A value of PetitionerCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("petitionerCsePersonsWorkSet")]
    public CsePersonsWorkSet PetitionerCsePersonsWorkSet
    {
      get => petitionerCsePersonsWorkSet ??= new();
      set => petitionerCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of PetitionerCsePerson.
    /// </summary>
    [JsonPropertyName("petitionerCsePerson")]
    public CsePerson PetitionerCsePerson
    {
      get => petitionerCsePerson ??= new();
      set => petitionerCsePerson = value;
    }

    /// <summary>
    /// A value of FirstPetitioner.
    /// </summary>
    [JsonPropertyName("firstPetitioner")]
    public CsePerson FirstPetitioner
    {
      get => firstPetitioner ??= new();
      set => firstPetitioner = value;
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
    /// A value of LnullCsePerson.
    /// </summary>
    [JsonPropertyName("lnullCsePerson")]
    public CsePerson LnullCsePerson
    {
      get => lnullCsePerson ??= new();
      set => lnullCsePerson = value;
    }

    /// <summary>
    /// A value of LnullCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("lnullCsePersonsWorkSet")]
    public CsePersonsWorkSet LnullCsePersonsWorkSet
    {
      get => lnullCsePersonsWorkSet ??= new();
      set => lnullCsePersonsWorkSet = value;
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
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
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
    /// A value of NullObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("nullObligationPaymentSchedule")]
    public ObligationPaymentSchedule NullObligationPaymentSchedule
    {
      get => nullObligationPaymentSchedule ??= new();
      set => nullObligationPaymentSchedule = value;
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
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of NullLegalAction.
    /// </summary>
    [JsonPropertyName("nullLegalAction")]
    public LegalAction NullLegalAction
    {
      get => nullLegalAction ??= new();
      set => nullLegalAction = value;
    }

    private Obligation hardcodeJointAndSeveral;
    private ObligationRlnRsn hardcodeConcurrent;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private EabReportSend neededToWrite;
    private EabReportSend neededToOpen;
    private ProgramProcessingInfo sysin;
    private External external;
    private Job job;
    private JobRun jobRun;
    private CsePerson obligorCsePerson;
    private LegalAction legalAction;
    private CsePersonsWorkSet obligorCsePersonsWorkSet;
    private Fips fips;
    private CsePersonsWorkSet respondant;
    private Common petitionerCommon;
    private CsePerson lrespondant;
    private CsePersonsWorkSet petitionerCsePersonsWorkSet;
    private CsePerson petitionerCsePerson;
    private CsePerson firstPetitioner;
    private Common common;
    private CsePerson lnullCsePerson;
    private CsePersonsWorkSet lnullCsePersonsWorkSet;
    private LegalActionPerson legalActionPerson;
    private LegalActionDetail legalActionDetail;
    private ObligationType obligationType;
    private Obligation obligation;
    private ObligationPaymentSchedule nullObligationPaymentSchedule;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private ObligationTransaction obligationTransaction;
    private DebtDetail debtDetail;
    private Collection collection;
    private LegalAction nullLegalAction;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingJob.
    /// </summary>
    [JsonPropertyName("existingJob")]
    public Job ExistingJob
    {
      get => existingJob ??= new();
      set => existingJob = value;
    }

    /// <summary>
    /// A value of ExistingJobRun.
    /// </summary>
    [JsonPropertyName("existingJobRun")]
    public JobRun ExistingJobRun
    {
      get => existingJobRun ??= new();
      set => existingJobRun = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
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
    /// A value of Obligor2.
    /// </summary>
    [JsonPropertyName("obligor2")]
    public CsePersonAccount Obligor2
    {
      get => obligor2 ??= new();
      set => obligor2 = value;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    private Job existingJob;
    private JobRun existingJobRun;
    private CsePerson obligor1;
    private LegalAction legalAction;
    private Fips fips;
    private Tribunal tribunal;
    private CsePerson csePerson;
    private LegalActionPerson legalActionPerson;
    private LegalActionDetail legalActionDetail;
    private ObligationType obligationType;
    private AccrualInstructions accrualInstructions;
    private ObligationTransaction obligationTransaction;
    private ObligationRlnRsn obligationRlnRsn;
    private ObligationRln obligationRln;
    private Obligation obligation;
    private DebtDetail debtDetail;
    private CsePersonAccount obligor2;
    private ObligationTransaction debtAdjustment;
    private Collection collection;
  }
#endregion
}
