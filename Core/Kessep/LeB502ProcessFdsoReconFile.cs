// Program: LE_B502_PROCESS_FDSO_RECON_FILE, ID: 371012323, model: 746.
// Short name: SWEL502B
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_B502_PROCESS_FDSO_RECON_FILE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class LeB502ProcessFdsoReconFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B502_PROCESS_FDSO_RECON_FILE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB502ProcessFdsoReconFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB502ProcessFdsoReconFile.
  /// </summary>
  public LeB502ProcessFdsoReconFile(IContext context, Import import,
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
    // ************************  M A I N T E N A N C E   L O G  
    // ************************
    // * DATE		PR/WR		Developer / Description
    // ********************************************************************************
    // *10/03/00			Raj Sawant 	Initial Creation
    // *04/07/2002	PR131272	E.Shirk		Altered logic to no longer set last name on
    // adm act cert to concat of last name plus 4 digits of last name from
    // recon file when a snapshot/recon file no match occurs.  Simply set last
    // name to what fed feels is accurate.
    ExitState = "ACO_NN0000_ALL_OK";
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmChkpntRestart();
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    local.EabReportSend.ProgramName = "SWELB502";
    UseCabErrorReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered opening control report.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ************************************************
    // *Call External to Open the Flat File.          *
    // ************************************************
    local.PassArea.FileInstruction = "OPEN";
    UseLeEabProcessFdsoReconFile3();

    if (!IsEmpty(local.PassArea.TextReturnCode))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error in file open for 'le_eab_process_fdso_recon_file'.";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "FILE_OPEN_ERROR";

      return;
    }

    local.Null1.DateSent = null;
    local.ReadNextReconRecord.Flag = "N";
    local.DoNotReadReconRecord.Flag = "N";
    local.EndOfReconFile.Flag = "N";
    local.EndOfSnapshotFile.Flag = "N";
    local.FileOpened.Flag = "Y";
    local.ReconPersonExistOnFdso.Flag = "N";

    do
    {
      local.AmountDoesNotMatch.Flag = "N";
      local.EtypeAdmDoesNotMatch.Flag = "N";
      local.EtypeRetDoesNotMatch.Flag = "N";
      local.EtypeVenDoesNotMatch.Flag = "N";
      local.EtypeSalDoesNotMatch.Flag = "N";
      local.EtypeTaxDoesNotMatch.Flag = "N";
      local.EtypePasDoesNotMatch.Flag = "N";
      local.EtypeFidmDoesNotMatch.Flag = "N";
      local.SsnDoesNotMatch.Flag = "N";
      local.CodeDoesNotMatch.Flag = "N";
      local.FirstNameDoesNotMatch.Flag = "N";
      local.LastNameDoesNotMatch.Flag = "N";

      if (AsChar(local.EndOfReconFile.Flag) == 'Y')
      {
      }
      else
      {
        if (AsChar(local.DoNotReadReconRecord.Flag) == 'Y')
        {
          local.DoNotReadReconRecord.Flag = "N";

          goto Test1;
        }

        local.PassArea.FileInstruction = "READ1";
        UseLeEabProcessFdsoReconFile1();

        if (Equal(local.PassArea.TextReturnCode, "E1"))
        {
          local.EndOfReconFile.Flag = "Y";
        }

        if (!IsEmpty(local.PassArea.TextReturnCode))
        {
          if (AsChar(local.EndOfReconFile.Flag) == 'Y')
          {
          }
          else
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Error encountered reading 'le_eab_process_fdso_recon_file' for Recon file.";
              
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            ExitState = "FILE_OPEN_ERROR";

            return;
          }
        }
        else
        {
          ++local.TotalNumberOfReconRecord.Count;

          // ----------------------------------------------------
          // If a record has already created
          // under 'If Recon Case number equal
          // to Snapshot case number' and if there is
          // next record with same case number
          // as previously created , then to avoid
          // duplicate or 'FDSO already exists' exitstate,
          // check following conditions.
          // ----------------------------------------------------
          if (Equal(local.BatchFdsoReconciliationFileRecord.CaseNumber,
            local.AlreadyModifyPrev.CaseNumber) && !
            IsEmpty(local.BatchFdsoReconciliationFileRecord.CaseNumber))
          {
            continue;
          }

          local.PreviousFdsoReconciliationFileRecord.CaseNumber =
            local.BatchFdsoReconciliationFileRecord.CaseNumber;
        }
      }

Test1:

      if (Equal(local.BatchFdsoReconciliationFileRecord.RecordIdentifier, "RCT"))
        
      {
        // -------------------------------------------------------------
        // This is a last record in Reconciliation file
        // -------------------------------------------------------------
      }

      if (!Equal(local.BatchFdsoReconciliationFileRecord.RecordIdentifier, "REC"))
        
      {
        if (AsChar(local.EndOfReconFile.Flag) == 'Y')
        {
        }
        else
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "Record Identifier is in error.";
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "ACO_NN0000_ABEND_4_BATCH";

          return;
        }
      }

      if (!Equal(local.BatchFdsoReconciliationFileRecord.SubmittingState, "KS"))
      {
        if (AsChar(local.EndOfReconFile.Flag) == 'Y')
        {
        }
        else
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "Submitting state is not KS.";
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "ACO_NN0000_ABEND_4_BATCH";

          return;
        }
      }

      if (IsEmpty(local.BatchFdsoReconciliationFileRecord.CaseNumber))
      {
        if (AsChar(local.EndOfReconFile.Flag) == 'Y')
        {
          goto Test2;
        }

        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Received blank case number from Fed , SSN : " + NumberToString
          (local.BatchFdsoReconciliationFileRecord.Ssn, 15);
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        continue;
      }

Test2:

      if (AsChar(local.ReadNextReconRecord.Flag) == 'Y')
      {
        local.ReadNextReconRecord.Flag = "N";
      }
      else
      {
        if (AsChar(local.EndOfSnapshotFile.Flag) == 'Y')
        {
          goto Test3;
        }

        local.PassArea.FileInstruction = "READ2";
        UseLeEabProcessFdsoReconFile2();

        if (Equal(local.PassArea.TextReturnCode, "E2"))
        {
          local.EndOfSnapshotFile.Flag = "Y";
        }

        if (!IsEmpty(local.PassArea.TextReturnCode))
        {
          if (AsChar(local.EndOfSnapshotFile.Flag) == 'Y')
          {
          }
          else
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Error encountered reading 'le_eab_process_fdso_recon_file' for Snapshot file.";
              
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            ExitState = "FILE_OPEN_ERROR";

            return;
          }
        }
        else
        {
          ++local.TotalNumberOfSnapshotRecord.Count;

          // ----------------------------------------------------
          // If a record has already created
          // under 'If Recon Case number equal
          // to Snapshot case number' and if there is
          // next record with same case number
          // as previously created , then to avoid
          // duplicate or 'FDSO already exists' exitstate,
          // check following conditions.
          // ----------------------------------------------------
          if (Equal(local.BatchFdsoSnapshotFileRecord.CaseNumber,
            local.AlreadyModifyPrev.CaseNumber))
          {
            local.PassArea.FileInstruction = "READ2";
            UseLeEabProcessFdsoReconFile2();

            if (Equal(local.PassArea.TextReturnCode, "E2"))
            {
              local.EndOfSnapshotFile.Flag = "Y";
            }

            if (!IsEmpty(local.PassArea.TextReturnCode))
            {
              if (AsChar(local.EndOfSnapshotFile.Flag) == 'Y')
              {
                goto Test3;
              }
              else
              {
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "Error encountered reading 'le_eab_process_fdso_recon_file' for Snapshot file.";
                  
                UseCabErrorReport2();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                  return;
                }

                ExitState = "FILE_OPEN_ERROR";

                return;
              }
            }
            else
            {
              ++local.TotalNumberOfSnapshotRecord.Count;
              local.PreviousFdsoSnapshotFileRecord.CaseNumber =
                local.BatchFdsoSnapshotFileRecord.CaseNumber;

              goto Test3;
            }
          }

          local.PreviousFdsoSnapshotFileRecord.CaseNumber =
            local.BatchFdsoSnapshotFileRecord.CaseNumber;
        }
      }

Test3:

      if (AsChar(local.EndOfReconFile.Flag) == 'Y' && AsChar
        (local.EndOfSnapshotFile.Flag) != 'Y')
      {
        // ----------------------------------------------------------
        // This means we have more records than Reconciliation file.
        // Check each record, if it is 'D' delete then Fed
        // has already processed it. If it is 'M' or 'A', send this
        // person again as 'A' add.
        // ----------------------------------------------------------
        if (AsChar(local.BatchFdsoSnapshotFileRecord.TransactionType) == 'D')
        {
        }
        else if (AsChar(local.BatchFdsoSnapshotFileRecord.TransactionType) == 'M'
          || AsChar(local.BatchFdsoSnapshotFileRecord.TransactionType) == 'A')
        {
          // ----------------------------------------------------------
          // Send this person again as 'A' add.
          // ----------------------------------------------------------
          if (ReadAdministrativeActCertification1())
          {
            local.New1.AdcAmount =
              entities.AdministrativeActCertification.AdcAmount;
            local.New1.AmountOwed =
              entities.AdministrativeActCertification.AmountOwed;
            local.New1.CaseNumber =
              entities.AdministrativeActCertification.CaseNumber;
            local.New1.CaseType =
              entities.AdministrativeActCertification.CaseType;
            local.New1.CreatedBy = global.UserId;
            local.New1.CreatedTstamp = Now();
            local.New1.CurrentAmount =
              entities.AdministrativeActCertification.CurrentAmount;
            local.New1.CurrentAmountDate = Now().Date;
            local.New1.DateSent = local.Null1.DateSent;
            local.New1.EtypeAdministrativeOffset =
              entities.AdministrativeActCertification.EtypeAdministrativeOffset;
              
            local.New1.EtypeFederalRetirement =
              entities.AdministrativeActCertification.EtypeFederalRetirement;
            local.New1.EtypeFederalSalary =
              entities.AdministrativeActCertification.EtypeFederalSalary;
            local.New1.EtypeFinancialInstitution =
              entities.AdministrativeActCertification.EtypeFinancialInstitution;
              
            local.New1.EtypePassportDenial =
              entities.AdministrativeActCertification.EtypePassportDenial;
            local.New1.EtypeTaxRefund =
              entities.AdministrativeActCertification.EtypeTaxRefund;
            local.New1.EtypeVendorPaymentOrMisc =
              entities.AdministrativeActCertification.EtypeVendorPaymentOrMisc;
            local.New1.FirstName =
              entities.AdministrativeActCertification.FirstName;
            local.New1.LastName =
              entities.AdministrativeActCertification.LastName;
            local.New1.LastUpdatedBy = global.UserId;
            local.New1.LastUpdatedTstamp = Now();
            local.New1.LocalCode =
              entities.AdministrativeActCertification.LocalCode;
            local.New1.NonAdcAmount =
              entities.AdministrativeActCertification.NonAdcAmount;
            local.New1.OriginalAmount =
              entities.AdministrativeActCertification.OriginalAmount;
            local.New1.ProcessYear =
              entities.AdministrativeActCertification.ProcessYear;
            local.New1.Ssn = entities.AdministrativeActCertification.Ssn;
            local.New1.TakenDate = Now().Date;
            local.New1.TanfCode =
              entities.AdministrativeActCertification.TanfCode;
            local.New1.TransferState =
              entities.AdministrativeActCertification.TransferState;
            local.New1.TtypeAAddNewCase = "A";
            local.New1.TtypeDDeleteCertification = "";
            local.New1.TtypeLChangeSubmittingState = "";
            local.New1.TtypeMModifyAmount = "";
            local.New1.TtypeRModifyExclusion = "";
          }

          local.CsePerson.Number = local.BatchFdsoSnapshotFileRecord.CaseNumber;
          UseCreateFederalDebtSetoff();

          if (!IsExitState("ACO_NI0000_CREATE_OK"))
          {
            if (IsExitState("CSE_PERSON_ACCOUNT_NF"))
            {
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "Err-'more snap than Recon' in 'create federal debt setoff',cse_person_account_nf:" +
                local.BatchFdsoSnapshotFileRecord.CaseNumber;
              UseCabErrorReport2();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }

              ExitState = "ACO_NN0000_ABEND_4_BATCH";

              return;
            }

            if (IsExitState("FN0000_OBLIGOR_NF"))
            {
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "Err- 'A' add for 'more snap than Recon' - in 'create federal debt setoff', FN0000_OBLIGOR_NF: " +
                local.BatchFdsoSnapshotFileRecord.CaseNumber;
              UseCabErrorReport2();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }

              ExitState = "ACO_NN0000_ABEND_4_BATCH";

              return;
            }

            if (IsExitState("FEDERAL_DEBT_SETOFF_AE"))
            {
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "Err-'A' add for 'more snap than Recon'-in 'create federal debt setoff'-FEDERAL_DEBT_SETOFF_AE:" +
                local.BatchFdsoSnapshotFileRecord.CaseNumber;
              UseCabErrorReport2();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }

              ExitState = "ACO_NN0000_ABEND_4_BATCH";

              return;
            }
          }
          else
          {
            ++local.TotalNumberOfAddRecord.Count;
          }

          ++local.TotalNumberOfMismatch.Count;
        }

        do
        {
          local.PassArea.FileInstruction = "READ2";
          UseLeEabProcessFdsoReconFile2();

          if (Equal(local.PassArea.TextReturnCode, "E2"))
          {
            local.EndOfSnapshotFile.Flag = "Y";
          }

          if (!IsEmpty(local.PassArea.TextReturnCode))
          {
            if (AsChar(local.EndOfSnapshotFile.Flag) == 'Y')
            {
              goto Test4;
            }
            else
            {
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "Error encountered reading 'le_eab_process_fdso_recon_file' for Snapshot file.";
                
              UseCabErrorReport2();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }

              ExitState = "FILE_OPEN_ERROR";

              return;
            }
          }
          else
          {
            ++local.TotalNumberOfSnapshotRecord.Count;
          }

          // ----------------------------------------------------------
          // If a person is written twice in a Snapshot
          // file due to ADC/NADC amount greater than 150/500
          // or due to local code change or exclusion indicator etc
          // then to avoid creating duplicate 'A' add or exit state
          // 'FDSO already exists', please check following conditions.
          // ----------------------------------------------------------
          if (Equal(local.BatchFdsoSnapshotFileRecord.CaseNumber,
            local.PreviousFdsoSnapshotFileRecord.CaseNumber))
          {
            continue;
          }
          else
          {
            local.PreviousFdsoSnapshotFileRecord.CaseNumber =
              local.BatchFdsoSnapshotFileRecord.CaseNumber;
          }

          if (AsChar(local.BatchFdsoSnapshotFileRecord.TransactionType) == 'D')
          {
          }
          else if (AsChar(local.BatchFdsoSnapshotFileRecord.TransactionType) ==
            'M' || AsChar
            (local.BatchFdsoSnapshotFileRecord.TransactionType) == 'A')
          {
            // ----------------------------------------------------------
            // Send this person again as 'A' add.
            // ----------------------------------------------------------
            if (ReadAdministrativeActCertification1())
            {
              local.New1.AdcAmount =
                entities.AdministrativeActCertification.AdcAmount;
              local.New1.AmountOwed =
                entities.AdministrativeActCertification.AmountOwed;
              local.New1.CaseNumber =
                entities.AdministrativeActCertification.CaseNumber;
              local.New1.CaseType =
                entities.AdministrativeActCertification.CaseType;
              local.New1.CreatedBy = global.UserId;
              local.New1.CreatedTstamp = Now();
              local.New1.CurrentAmount =
                entities.AdministrativeActCertification.CurrentAmount;
              local.New1.CurrentAmountDate = Now().Date;
              local.New1.DateSent = local.Null1.DateSent;
              local.New1.EtypeAdministrativeOffset =
                entities.AdministrativeActCertification.
                  EtypeAdministrativeOffset;
              local.New1.EtypeFederalRetirement =
                entities.AdministrativeActCertification.EtypeFederalRetirement;
              local.New1.EtypeFederalSalary =
                entities.AdministrativeActCertification.EtypeFederalSalary;
              local.New1.EtypeFinancialInstitution =
                entities.AdministrativeActCertification.
                  EtypeFinancialInstitution;
              local.New1.EtypePassportDenial =
                entities.AdministrativeActCertification.EtypePassportDenial;
              local.New1.EtypeTaxRefund =
                entities.AdministrativeActCertification.EtypeTaxRefund;
              local.New1.EtypeVendorPaymentOrMisc =
                entities.AdministrativeActCertification.
                  EtypeVendorPaymentOrMisc;
              local.New1.FirstName =
                entities.AdministrativeActCertification.FirstName;
              local.New1.LastName =
                entities.AdministrativeActCertification.LastName;
              local.New1.LastUpdatedBy = global.UserId;
              local.New1.LastUpdatedTstamp = Now();
              local.New1.LocalCode =
                entities.AdministrativeActCertification.LocalCode;
              local.New1.NonAdcAmount =
                entities.AdministrativeActCertification.NonAdcAmount;
              local.New1.OriginalAmount =
                entities.AdministrativeActCertification.OriginalAmount;
              local.New1.ProcessYear =
                entities.AdministrativeActCertification.ProcessYear;
              local.New1.Ssn = entities.AdministrativeActCertification.Ssn;
              local.New1.TakenDate = Now().Date;
              local.New1.TanfCode =
                entities.AdministrativeActCertification.TanfCode;
              local.New1.TransferState =
                entities.AdministrativeActCertification.TransferState;
              local.New1.TtypeAAddNewCase = "A";
              local.New1.TtypeDDeleteCertification = "";
              local.New1.TtypeLChangeSubmittingState = "";
              local.New1.TtypeMModifyAmount = "";
              local.New1.TtypeRModifyExclusion = "";
            }

            local.CsePerson.Number =
              local.BatchFdsoSnapshotFileRecord.CaseNumber;
            UseCreateFederalDebtSetoff();

            if (!IsExitState("ACO_NI0000_CREATE_OK"))
            {
              if (IsExitState("CSE_PERSON_ACCOUNT_NF"))
              {
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "Err-'A' add for 'more snap than Recon'-in 'create federal debt setoff',cse_person_account_nf:" +
                  local.BatchFdsoSnapshotFileRecord.CaseNumber;
                UseCabErrorReport2();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                  return;
                }

                ExitState = "ACO_NN0000_ABEND_4_BATCH";

                return;
              }

              if (IsExitState("FN0000_OBLIGOR_NF"))
              {
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "Err-' A' add for 'more snap than Recon'-in 'create federal debt setoff'- FN0000_OBLIGOR_NF:" +
                  local.BatchFdsoSnapshotFileRecord.CaseNumber;
                UseCabErrorReport2();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                  return;
                }

                ExitState = "ACO_NN0000_ABEND_4_BATCH";

                return;
              }

              if (IsExitState("FEDERAL_DEBT_SETOFF_AE"))
              {
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "Err-'A' add for 'more snap than Recon' in 'create federal debt setoff'-FEDERAL_DEBT_SETOFF_AE:" +
                  local.BatchFdsoSnapshotFileRecord.CaseNumber;
                UseCabErrorReport2();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                  return;
                }

                ExitState = "ACO_NN0000_ABEND_4_BATCH";

                return;
              }
            }
            else
            {
              ++local.TotalNumberOfAddRecord.Count;
            }

            ++local.TotalNumberOfMismatch.Count;
          }
        }
        while(!Equal(local.PassArea.TextReturnCode, "E2"));
      }

Test4:

      if (AsChar(local.EndOfSnapshotFile.Flag) == 'Y' && AsChar
        (local.EndOfReconFile.Flag) != 'Y')
      {
        // ----------------------------------------------------------
        // This means Reconciliation file has more records
        // than Snapshot file.Check whether each
        // record/person exists on our system, if yes then
        // send a 'D' delete to Fed else write it to error report.
        // ----------------------------------------------------------
        if (ReadAdministrativeActCertification2())
        {
          local.ReconPersonExistOnFdso.Flag = "Y";
        }

        if (AsChar(local.ReconPersonExistOnFdso.Flag) == 'Y')
        {
          local.ReconPersonExistOnFdso.Flag = "N";

          // ----------------------------------------------------------
          // This Reconciliation person exists on FDSO
          // so create a 'D' delete record with 0 amount
          // and set case type to Reconciliation case type,ssn,local code,first 
          // name,last name as per Reconciliation file.
          // ----------------------------------------------------------
          local.New1.AdcAmount = 0;
          local.New1.CaseNumber =
            entities.AdministrativeActCertification.CaseNumber;
          local.New1.CaseType =
            local.BatchFdsoReconciliationFileRecord.CaseType;
          local.New1.CreatedBy = global.UserId;
          local.New1.CreatedTstamp = Now();
          local.New1.DateSent = local.Null1.DateSent;
          local.New1.FirstName =
            local.BatchFdsoReconciliationFileRecord.FirstName;
          local.New1.LastName =
            local.BatchFdsoReconciliationFileRecord.LastName;
          local.New1.LastUpdatedBy = global.UserId;
          local.New1.LastUpdatedTstamp = Now();
          local.New1.LocalCode =
            local.BatchFdsoReconciliationFileRecord.LocalCode;
          local.New1.NonAdcAmount = 0;
          local.New1.OriginalAmount =
            entities.AdministrativeActCertification.OriginalAmount;
          local.New1.ProcessYear =
            entities.AdministrativeActCertification.ProcessYear;
          local.New1.Ssn = local.BatchFdsoReconciliationFileRecord.Ssn;
          local.New1.TakenDate = Now().Date;
          local.New1.TanfCode =
            local.BatchFdsoReconciliationFileRecord.CaseType;
          local.New1.TransferState =
            entities.AdministrativeActCertification.TransferState;
          local.New1.TtypeAAddNewCase = "";
          local.New1.TtypeDDeleteCertification = "D";
          local.New1.TtypeLChangeSubmittingState = "";
          local.New1.TtypeMModifyAmount = "";
          local.New1.TtypeRModifyExclusion = "";

          // --------------------------------------------------
          // PR # 00125631
          // Beginning Of Change
          // Set Current Amount to zero. Also set other attributes
          // to proper value.
          // ---------------------------------------------------
          local.New1.AmountOwed = 0;
          local.New1.CurrentAmount = 0;
          local.New1.CurrentAmountDate = Now().Date;
          local.New1.EtypeAdministrativeOffset = "";
          local.New1.EtypeFederalRetirement = "";
          local.New1.EtypeFederalSalary = "";
          local.New1.EtypeFinancialInstitution = "";
          local.New1.EtypePassportDenial = "";
          local.New1.EtypeTaxRefund = "";
          local.New1.EtypeVendorPaymentOrMisc = "";

          // --------------------------------------------------
          // PR # 00125631
          // End Of Change.
          // ---------------------------------------------------
          local.CsePerson.Number =
            local.BatchFdsoReconciliationFileRecord.CaseNumber;
          UseCreateFederalDebtSetoff();

          if (!IsExitState("ACO_NI0000_CREATE_OK"))
          {
            if (IsExitState("CSE_PERSON_ACCOUNT_NF"))
            {
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "Err -'more Recon than snapshot' in 'create FDSO',cse_person_account_nf:" +
                local.BatchFdsoReconciliationFileRecord.CaseNumber;
              UseCabErrorReport2();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }

              ExitState = "ACO_NN0000_ABEND_4_BATCH";

              return;
            }

            if (IsExitState("FN0000_OBLIGOR_NF"))
            {
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "Err-'more Recon than snapshot' in 'create FDSO'-FN0000_OBLIGOR_NF:" +
                local.BatchFdsoReconciliationFileRecord.CaseNumber;
              UseCabErrorReport2();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }

              ExitState = "ACO_NN0000_ABEND_4_BATCH";

              return;
            }

            if (IsExitState("FEDERAL_DEBT_SETOFF_AE"))
            {
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "Err-'more Recon than snap'- 'create FDSO'-FEDERAL_DEBT_SETOFF_AE:" +
                local.BatchFdsoReconciliationFileRecord.CaseNumber;
              UseCabErrorReport2();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }

              ExitState = "ACO_NN0000_ABEND_4_BATCH";

              return;
            }
          }
          else
          {
            ++local.TotalNumberOfDeleteRecord.Count;
          }
        }
        else
        {
          // ----------------------------------------------------------
          // Check if person exists on our system.
          // ----------------------------------------------------------
          local.CsePerson.Number =
            local.BatchFdsoReconciliationFileRecord.CaseNumber;

          if (ReadCsePerson())
          {
            // ----------------------------------------------------------
            // Create a 'D' delete record.
            // ----------------------------------------------------------
            local.New1.AdcAmount = 0;
            local.New1.CaseNumber =
              local.BatchFdsoReconciliationFileRecord.CaseNumber;
            local.New1.CaseType =
              local.BatchFdsoReconciliationFileRecord.CaseType;
            local.New1.CreatedBy = global.UserId;
            local.New1.CreatedTstamp = Now();
            local.New1.DateSent = local.Null1.DateSent;
            local.New1.FirstName =
              local.BatchFdsoReconciliationFileRecord.FirstName;
            local.New1.LastName =
              local.BatchFdsoReconciliationFileRecord.LastName;
            local.New1.LastUpdatedBy = global.UserId;
            local.New1.LastUpdatedTstamp = Now();
            local.New1.LocalCode =
              local.BatchFdsoReconciliationFileRecord.LocalCode;
            local.New1.NonAdcAmount = 0;
            local.New1.OriginalAmount = 0;
            local.New1.ProcessYear =
              Year(local.ProgramProcessingInfo.ProcessDate);
            local.New1.Ssn = local.BatchFdsoReconciliationFileRecord.Ssn;
            local.New1.TakenDate = Now().Date;
            local.New1.TanfCode =
              local.BatchFdsoReconciliationFileRecord.CaseType;
            local.New1.TtypeAAddNewCase = "";
            local.New1.TtypeDDeleteCertification = "D";
            local.New1.TtypeLChangeSubmittingState = "";
            local.New1.TtypeMModifyAmount = "";
            local.New1.TtypeRModifyExclusion = "";

            // --------------------------------------------------
            // PR # 00125631
            // Beginning Of Change
            // Set Current Amount to zero. Also set other attributes
            // to proper value.
            // ---------------------------------------------------
            local.New1.AmountOwed = 0;
            local.New1.CurrentAmount = 0;
            local.New1.CurrentAmountDate = Now().Date;
            local.New1.EtypeAdministrativeOffset = "";
            local.New1.EtypeFederalRetirement = "";
            local.New1.EtypeFederalSalary = "";
            local.New1.EtypeFinancialInstitution = "";
            local.New1.EtypePassportDenial = "";
            local.New1.EtypeTaxRefund = "";
            local.New1.EtypeVendorPaymentOrMisc = "";

            // --------------------------------------------------
            // PR # 00125631
            // End Of Change.
            // ---------------------------------------------------
            local.CsePerson.Number =
              local.BatchFdsoReconciliationFileRecord.CaseNumber;
            UseCreateFederalDebtSetoff();

            if (!IsExitState("ACO_NI0000_CREATE_OK"))
            {
              if (IsExitState("CSE_PERSON_ACCOUNT_NF"))
              {
                // ----------------------------------------------------------
                // Since this person number doesn't exists
                // on FDSO table and is not a obligor so create
                // 'D' delete record and do not associate with
                // obligor and adminstrative action.
                // ----------------------------------------------------------
                try
                {
                  CreateFederalDebtSetoff1();
                  ++local.TotalNumberOfDeleteRecord.Count;
                  local.EabFileHandling.Action = "WRITE";
                  local.EabReportSend.RptDetail =
                    "This record should be deleted from FDSO table after sending it to Fed. Case_number : " +
                    local.BatchFdsoReconciliationFileRecord.CaseNumber;
                  UseCabErrorReport2();

                  if (!Equal(local.EabFileHandling.Status, "OK"))
                  {
                    ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                    return;
                  }
                }
                catch(Exception e)
                {
                  switch(GetErrorCode(e))
                  {
                    case ErrorCode.AlreadyExists:
                      local.EabFileHandling.Action = "WRITE";
                      local.EabReportSend.RptDetail =
                        "Err-'more Recon than snap'- 'under cse person acc nf'-FDSO_AE:" +
                        local.BatchFdsoReconciliationFileRecord.CaseNumber;
                      UseCabErrorReport2();

                      if (!Equal(local.EabFileHandling.Status, "OK"))
                      {
                        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                        return;
                      }

                      ExitState = "ACO_NN0000_ABEND_4_BATCH";

                      return;
                    case ErrorCode.PermittedValueViolation:
                      local.EabFileHandling.Action = "WRITE";
                      local.EabReportSend.RptDetail =
                        "Err-'more Recon than snap'- 'under cse person acc nf'- permitted value:" +
                        local.BatchFdsoReconciliationFileRecord.CaseNumber;
                      UseCabErrorReport2();

                      if (!Equal(local.EabFileHandling.Status, "OK"))
                      {
                        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                        return;
                      }

                      ExitState = "ACO_NN0000_ABEND_4_BATCH";

                      return;
                    case ErrorCode.DatabaseError:
                      break;
                    default:
                      throw;
                  }
                }
              }

              if (IsExitState("FN0000_OBLIGOR_NF"))
              {
                // ----------------------------------------------------------
                // Since this person number doesn't exists
                // on FDSO table and is not associated to administrative
                // action table so create
                // 'D' delete record and do not associate with
                // obligor and adminstrative action.
                // ----------------------------------------------------------
                try
                {
                  CreateFederalDebtSetoff1();
                  ++local.TotalNumberOfDeleteRecord.Count;
                  local.EabFileHandling.Action = "WRITE";
                  local.EabReportSend.RptDetail =
                    "This record should be deleted from FDSO table after sending it to Fed. Case_number : " +
                    local.BatchFdsoReconciliationFileRecord.CaseNumber;
                  UseCabErrorReport2();

                  if (!Equal(local.EabFileHandling.Status, "OK"))
                  {
                    ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                    return;
                  }
                }
                catch(Exception e)
                {
                  switch(GetErrorCode(e))
                  {
                    case ErrorCode.AlreadyExists:
                      local.EabFileHandling.Action = "WRITE";
                      local.EabReportSend.RptDetail =
                        "Err-'more Recon than snap'- 'under fn0000_obligor_nf'-FDSO_AE:" +
                        local.BatchFdsoReconciliationFileRecord.CaseNumber;
                      UseCabErrorReport2();

                      if (!Equal(local.EabFileHandling.Status, "OK"))
                      {
                        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                        return;
                      }

                      ExitState = "ACO_NN0000_ABEND_4_BATCH";

                      return;
                    case ErrorCode.PermittedValueViolation:
                      local.EabFileHandling.Action = "WRITE";
                      local.EabReportSend.RptDetail =
                        "Err-'more Recon than snap'- 'under fn0000_obligor_nf'- permitted value:" +
                        local.BatchFdsoReconciliationFileRecord.CaseNumber;
                      UseCabErrorReport2();

                      if (!Equal(local.EabFileHandling.Status, "OK"))
                      {
                        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                        return;
                      }

                      ExitState = "ACO_NN0000_ABEND_4_BATCH";

                      return;
                    case ErrorCode.DatabaseError:
                      break;
                    default:
                      throw;
                  }
                }
              }

              if (IsExitState("FEDERAL_DEBT_SETOFF_AE"))
              {
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "Err-'more Recon than snap'- 'create FDSO'-FDSO_AE:" + local
                  .BatchFdsoReconciliationFileRecord.CaseNumber;
                UseCabErrorReport2();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                  return;
                }

                ExitState = "ACO_NN0000_ABEND_4_BATCH";

                return;
              }
            }
            else
            {
              ++local.TotalNumberOfDeleteRecord.Count;
            }

            ++local.TotalNumberOfMismatch.Count;
          }
          else
          {
            // ----------------------------------------------------------
            // Since this person does not exists on our system,
            // write it to error report.
            // ----------------------------------------------------------
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "This cse person received from Reconciliation file does not exists on our system : " +
              local.BatchFdsoReconciliationFileRecord.CaseNumber;
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            ++local.TotalNumberOfMismatch.Count;
          }
        }

        do
        {
          local.PassArea.FileInstruction = "READ1";
          UseLeEabProcessFdsoReconFile1();

          if (Equal(local.PassArea.TextReturnCode, "E1"))
          {
            local.EndOfReconFile.Flag = "Y";
          }

          if (!IsEmpty(local.PassArea.TextReturnCode))
          {
            if (AsChar(local.EndOfReconFile.Flag) == 'Y')
            {
              goto Test5;
            }
            else
            {
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "Error encountered reading 'le_eab_process_fdso_recon_file' for Recon file.";
                
              UseCabErrorReport2();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }

              ExitState = "FILE_OPEN_ERROR";

              return;
            }
          }
          else
          {
            ++local.TotalNumberOfReconRecord.Count;
          }

          // ----------------------------------------------------------
          // If a person is written twice in a Recon
          // file due to ADC/NADC amount greater than 150/500
          // or due to local code change or exclusion indicator etc
          // then to avoid creating duplicate 'D' delete or exit state
          // 'FDSO already exists', please check following conditions.
          // ----------------------------------------------------------
          if (Equal(local.BatchFdsoReconciliationFileRecord.CaseNumber,
            local.PreviousFdsoReconciliationFileRecord.CaseNumber))
          {
            continue;
          }
          else
          {
            local.PreviousFdsoReconciliationFileRecord.CaseNumber =
              local.BatchFdsoReconciliationFileRecord.CaseNumber;
          }

          if (!Equal(local.BatchFdsoReconciliationFileRecord.RecordIdentifier,
            "REC"))
          {
            if (AsChar(local.EndOfReconFile.Flag) == 'Y')
            {
              goto Test5;
            }
            else
            {
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail = "Record Identifier is in error.";
              UseCabErrorReport2();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }

              ExitState = "ACO_NN0000_ABEND_4_BATCH";

              return;
            }
          }

          if (!Equal(local.BatchFdsoReconciliationFileRecord.SubmittingState,
            "KS"))
          {
            if (AsChar(local.EndOfReconFile.Flag) == 'Y')
            {
              goto Test5;
            }
            else
            {
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail = "Submitting state is not KS.";
              UseCabErrorReport2();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }

              ExitState = "ACO_NN0000_ABEND_4_BATCH";

              return;
            }
          }

          if (ReadAdministrativeActCertification2())
          {
            local.ReconPersonExistOnFdso.Flag = "Y";
          }

          if (AsChar(local.ReconPersonExistOnFdso.Flag) == 'Y')
          {
            local.ReconPersonExistOnFdso.Flag = "N";

            // ----------------------------------------------------------
            // This Reconciliation person exists on FDSO
            // so create a 'D' delete record with 0 amount
            // and set case type to Reconciliation case type,ssn,local code,
            // first name,last name as per Reconciliation file.
            // ----------------------------------------------------------
            local.New1.AdcAmount = 0;
            local.New1.CaseNumber =
              entities.AdministrativeActCertification.CaseNumber;
            local.New1.CaseType =
              local.BatchFdsoReconciliationFileRecord.CaseType;
            local.New1.CreatedBy = global.UserId;
            local.New1.CreatedTstamp = Now();
            local.New1.DateSent = local.Null1.DateSent;
            local.New1.FirstName =
              local.BatchFdsoReconciliationFileRecord.FirstName;
            local.New1.LastName =
              local.BatchFdsoReconciliationFileRecord.LastName;
            local.New1.LastUpdatedBy = global.UserId;
            local.New1.LastUpdatedTstamp = Now();
            local.New1.LocalCode =
              local.BatchFdsoReconciliationFileRecord.LocalCode;
            local.New1.NonAdcAmount = 0;
            local.New1.OriginalAmount =
              entities.AdministrativeActCertification.OriginalAmount;
            local.New1.ProcessYear =
              entities.AdministrativeActCertification.ProcessYear;
            local.New1.Ssn = local.BatchFdsoReconciliationFileRecord.Ssn;
            local.New1.TakenDate = Now().Date;
            local.New1.TanfCode =
              local.BatchFdsoReconciliationFileRecord.CaseType;
            local.New1.TransferState =
              entities.AdministrativeActCertification.TransferState;
            local.New1.TtypeAAddNewCase = "";
            local.New1.TtypeDDeleteCertification = "D";
            local.New1.TtypeLChangeSubmittingState = "";
            local.New1.TtypeMModifyAmount = "";
            local.New1.TtypeRModifyExclusion = "";

            // --------------------------------------------------
            // PR # 00125631
            // Beginning Of Change
            // Set Current Amount to zero. Also set other attributes
            // to proper value.
            // ---------------------------------------------------
            local.New1.AmountOwed = 0;
            local.New1.CurrentAmount = 0;
            local.New1.CurrentAmountDate = Now().Date;
            local.New1.EtypeAdministrativeOffset = "";
            local.New1.EtypeFederalRetirement = "";
            local.New1.EtypeFederalSalary = "";
            local.New1.EtypeFinancialInstitution = "";
            local.New1.EtypePassportDenial = "";
            local.New1.EtypeTaxRefund = "";
            local.New1.EtypeVendorPaymentOrMisc = "";

            // --------------------------------------------------
            // PR # 00125631
            // End Of Change.
            // ---------------------------------------------------
            local.CsePerson.Number =
              local.BatchFdsoReconciliationFileRecord.CaseNumber;
            UseCreateFederalDebtSetoff();

            if (!IsExitState("ACO_NI0000_CREATE_OK"))
            {
              if (IsExitState("CSE_PERSON_ACCOUNT_NF"))
              {
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "Err-'more Recon than snap'-'create FDSO'-cse_person_account_nf:" +
                  local.BatchFdsoReconciliationFileRecord.CaseNumber;
                UseCabErrorReport2();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                  return;
                }

                ExitState = "ACO_NN0000_ABEND_4_BATCH";

                return;
              }

              if (IsExitState("FN0000_OBLIGOR_NF"))
              {
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "Err-'more Recon than snap'-'create FDSO'-FN0000_OBLIGOR_NF:" +
                  local.BatchFdsoReconciliationFileRecord.CaseNumber;
                UseCabErrorReport2();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                  return;
                }

                ExitState = "ACO_NN0000_ABEND_4_BATCH";

                return;
              }

              if (IsExitState("FEDERAL_DEBT_SETOFF_AE"))
              {
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "Err-'more Recon than snap'-'create FDSO'-FDSO_AE:" + local
                  .BatchFdsoReconciliationFileRecord.CaseNumber;
                UseCabErrorReport2();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                  return;
                }

                ExitState = "ACO_NN0000_ABEND_4_BATCH";

                return;
              }
            }
            else
            {
              ++local.TotalNumberOfDeleteRecord.Count;
            }
          }
          else
          {
            // ----------------------------------------------------------
            // Check if person exists on our system.
            // ----------------------------------------------------------
            local.CsePerson.Number =
              local.BatchFdsoReconciliationFileRecord.CaseNumber;

            if (ReadCsePerson())
            {
              // ----------------------------------------------------------
              // Create a 'D' delete record.
              // ----------------------------------------------------------
              local.New1.AdcAmount = 0;
              local.New1.CaseNumber =
                local.BatchFdsoReconciliationFileRecord.CaseNumber;
              local.New1.CaseType =
                local.BatchFdsoReconciliationFileRecord.CaseType;
              local.New1.CreatedBy = global.UserId;
              local.New1.CreatedTstamp = Now();
              local.New1.DateSent = local.Null1.DateSent;
              local.New1.FirstName =
                local.BatchFdsoReconciliationFileRecord.FirstName;
              local.New1.LastName =
                local.BatchFdsoReconciliationFileRecord.LastName;
              local.New1.LastUpdatedBy = global.UserId;
              local.New1.LastUpdatedTstamp = Now();
              local.New1.LocalCode =
                local.BatchFdsoReconciliationFileRecord.LocalCode;
              local.New1.NonAdcAmount = 0;
              local.New1.OriginalAmount = 0;
              local.New1.ProcessYear =
                Year(local.ProgramProcessingInfo.ProcessDate);
              local.New1.Ssn = local.BatchFdsoReconciliationFileRecord.Ssn;
              local.New1.TakenDate = Now().Date;
              local.New1.TanfCode =
                local.BatchFdsoReconciliationFileRecord.CaseType;
              local.New1.TtypeAAddNewCase = "";
              local.New1.TtypeDDeleteCertification = "D";
              local.New1.TtypeLChangeSubmittingState = "";
              local.New1.TtypeMModifyAmount = "";
              local.New1.TtypeRModifyExclusion = "";

              // --------------------------------------------------
              // PR # 00125631
              // Beginning Of Change
              // Set Current Amount to zero. Also set other attributes
              // to proper value.
              // ---------------------------------------------------
              local.New1.AmountOwed = 0;
              local.New1.CurrentAmount = 0;
              local.New1.CurrentAmountDate = Now().Date;
              local.New1.EtypeAdministrativeOffset = "";
              local.New1.EtypeFederalRetirement = "";
              local.New1.EtypeFederalSalary = "";
              local.New1.EtypeFinancialInstitution = "";
              local.New1.EtypePassportDenial = "";
              local.New1.EtypeTaxRefund = "";
              local.New1.EtypeVendorPaymentOrMisc = "";

              // --------------------------------------------------
              // PR # 00125631
              // End Of Change.
              // ---------------------------------------------------
              local.CsePerson.Number =
                local.BatchFdsoReconciliationFileRecord.CaseNumber;
              UseCreateFederalDebtSetoff();

              if (!IsExitState("ACO_NI0000_CREATE_OK"))
              {
                if (IsExitState("CSE_PERSON_ACCOUNT_NF"))
                {
                  // ----------------------------------------------------------
                  // Since this person number doesn't exists
                  // on FDSO table and is not a obligor so create
                  // 'D' delete record and do not associate with
                  // obligor and adminstrative action.
                  // ----------------------------------------------------------
                  try
                  {
                    CreateFederalDebtSetoff1();
                    ++local.TotalNumberOfDeleteRecord.Count;
                    local.EabFileHandling.Action = "WRITE";
                    local.EabReportSend.RptDetail =
                      "This record should be deleted from FDSO table after sending it to Fed. Case_number : " +
                      local.BatchFdsoReconciliationFileRecord.CaseNumber;
                    UseCabErrorReport2();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                      return;
                    }
                  }
                  catch(Exception e)
                  {
                    switch(GetErrorCode(e))
                    {
                      case ErrorCode.AlreadyExists:
                        local.EabFileHandling.Action = "WRITE";
                        local.EabReportSend.RptDetail =
                          "Err-'more Recon than snap'- 'under cse person acc nf'-FDSO_AE:" +
                          local.BatchFdsoReconciliationFileRecord.CaseNumber;
                        UseCabErrorReport2();

                        if (!Equal(local.EabFileHandling.Status, "OK"))
                        {
                          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                          return;
                        }

                        ExitState = "ACO_NN0000_ABEND_4_BATCH";

                        return;
                      case ErrorCode.PermittedValueViolation:
                        local.EabFileHandling.Action = "WRITE";
                        local.EabReportSend.RptDetail =
                          "Err-'more Recon than snap'- 'under cse person acc nf'- permitted value :" +
                          local.BatchFdsoReconciliationFileRecord.CaseNumber;
                        UseCabErrorReport2();

                        if (!Equal(local.EabFileHandling.Status, "OK"))
                        {
                          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                          return;
                        }

                        ExitState = "ACO_NN0000_ABEND_4_BATCH";

                        return;
                      case ErrorCode.DatabaseError:
                        break;
                      default:
                        throw;
                    }
                  }
                }

                if (IsExitState("FN0000_OBLIGOR_NF"))
                {
                  // ----------------------------------------------------------
                  // Since this person number doesn't exists
                  // on FDSO table and is not associated to administrative
                  // action table so create
                  // 'D' delete record and do not associate with
                  // obligor and adminstrative action.
                  // ----------------------------------------------------------
                  try
                  {
                    CreateFederalDebtSetoff1();
                    ++local.TotalNumberOfDeleteRecord.Count;
                    local.EabFileHandling.Action = "WRITE";
                    local.EabReportSend.RptDetail =
                      "This record should be deleted from FDSO table after sending it to Fed. Case_number : " +
                      local.BatchFdsoReconciliationFileRecord.CaseNumber;
                    UseCabErrorReport2();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                      return;
                    }
                  }
                  catch(Exception e)
                  {
                    switch(GetErrorCode(e))
                    {
                      case ErrorCode.AlreadyExists:
                        local.EabFileHandling.Action = "WRITE";
                        local.EabReportSend.RptDetail =
                          "Err-'more Recon than snap'- 'under fn0000_obligor_nf'-FDSO_AE:" +
                          local.BatchFdsoReconciliationFileRecord.CaseNumber;
                        UseCabErrorReport2();

                        if (!Equal(local.EabFileHandling.Status, "OK"))
                        {
                          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                          return;
                        }

                        ExitState = "ACO_NN0000_ABEND_4_BATCH";

                        return;
                      case ErrorCode.PermittedValueViolation:
                        local.EabFileHandling.Action = "WRITE";
                        local.EabReportSend.RptDetail =
                          "Err-'more Recon than snap'- 'under fn0000_obligor_nf'- permitted value :" +
                          local.BatchFdsoReconciliationFileRecord.CaseNumber;
                        UseCabErrorReport2();

                        if (!Equal(local.EabFileHandling.Status, "OK"))
                        {
                          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                          return;
                        }

                        ExitState = "ACO_NN0000_ABEND_4_BATCH";

                        return;
                      case ErrorCode.DatabaseError:
                        break;
                      default:
                        throw;
                    }
                  }
                }

                if (IsExitState("FEDERAL_DEBT_SETOFF_AE"))
                {
                  local.EabFileHandling.Action = "WRITE";
                  local.EabReportSend.RptDetail =
                    "Err-'more Recon than snap'-'create FDSO'-FDSO_AE:" + local
                    .BatchFdsoReconciliationFileRecord.CaseNumber;
                  UseCabErrorReport2();

                  if (!Equal(local.EabFileHandling.Status, "OK"))
                  {
                    ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                    return;
                  }

                  ExitState = "ACO_NN0000_ABEND_4_BATCH";

                  return;
                }
              }
              else
              {
                ++local.TotalNumberOfDeleteRecord.Count;
              }

              ++local.TotalNumberOfMismatch.Count;
            }
            else
            {
              // ----------------------------------------------------------
              // Since this person does not exists on our system,
              // write it to error report.
              // ----------------------------------------------------------
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "This cse person received from Reconciliation file does not exists on our system : " +
                local.BatchFdsoReconciliationFileRecord.CaseNumber;
              UseCabErrorReport2();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }

              ++local.TotalNumberOfMismatch.Count;
            }
          }
        }
        while(!Equal(local.PassArea.TextReturnCode, "E1"));
      }

Test5:

      if (AsChar(local.EndOfReconFile.Flag) == 'Y' && AsChar
        (local.EndOfSnapshotFile.Flag) == 'Y')
      {
        local.FileOpened.Flag = "N";

        break;
      }

      if (!Equal(local.BatchFdsoReconciliationFileRecord.CaseNumber,
        local.BatchFdsoSnapshotFileRecord.CaseNumber))
      {
        if (Lt(local.BatchFdsoSnapshotFileRecord.CaseNumber,
          local.BatchFdsoReconciliationFileRecord.CaseNumber))
        {
          // ---------------------------------------------------------------
          // This means Fed does not have this person on their file/database.
          // ---------------------------------------------------------------
          if (AsChar(local.BatchFdsoSnapshotFileRecord.TransactionType) == 'D')
          {
            // ---------------------------------------------------------------
            // This means Fed has already got this delete record and we don't 
            // have to do anything, read next snapshot record.
            // ---------------------------------------------------------------
          }
          else if (AsChar(local.BatchFdsoSnapshotFileRecord.TransactionType) ==
            'M' || AsChar
            (local.BatchFdsoSnapshotFileRecord.TransactionType) == 'A')
          {
            // ---------------------------------------------------------------
            // Send this person again as add.
            // Read next Snapshot record.
            // ---------------------------------------------------------------
            if (ReadAdministrativeActCertification1())
            {
              local.New1.AdcAmount =
                entities.AdministrativeActCertification.AdcAmount;
              local.New1.AmountOwed =
                entities.AdministrativeActCertification.AmountOwed;
              local.New1.CaseNumber =
                entities.AdministrativeActCertification.CaseNumber;
              local.New1.CaseType =
                entities.AdministrativeActCertification.CaseType;
              local.New1.CreatedBy = global.UserId;
              local.New1.CreatedTstamp = Now();
              local.New1.CurrentAmount =
                entities.AdministrativeActCertification.CurrentAmount;
              local.New1.CurrentAmountDate = Now().Date;
              local.New1.DateSent = local.Null1.DateSent;
              local.New1.EtypeAdministrativeOffset =
                entities.AdministrativeActCertification.
                  EtypeAdministrativeOffset;
              local.New1.EtypeFederalRetirement =
                entities.AdministrativeActCertification.EtypeFederalRetirement;
              local.New1.EtypeFederalSalary =
                entities.AdministrativeActCertification.EtypeFederalSalary;
              local.New1.EtypeFinancialInstitution =
                entities.AdministrativeActCertification.
                  EtypeFinancialInstitution;
              local.New1.EtypePassportDenial =
                entities.AdministrativeActCertification.EtypePassportDenial;
              local.New1.EtypeTaxRefund =
                entities.AdministrativeActCertification.EtypeTaxRefund;
              local.New1.EtypeVendorPaymentOrMisc =
                entities.AdministrativeActCertification.
                  EtypeVendorPaymentOrMisc;
              local.New1.FirstName =
                entities.AdministrativeActCertification.FirstName;
              local.New1.LastName =
                entities.AdministrativeActCertification.LastName;
              local.New1.LastUpdatedBy = global.UserId;
              local.New1.LastUpdatedTstamp = Now();
              local.New1.LocalCode =
                entities.AdministrativeActCertification.LocalCode;
              local.New1.NonAdcAmount =
                entities.AdministrativeActCertification.NonAdcAmount;
              local.New1.OriginalAmount =
                entities.AdministrativeActCertification.OriginalAmount;
              local.New1.ProcessYear =
                entities.AdministrativeActCertification.ProcessYear;
              local.New1.Ssn = entities.AdministrativeActCertification.Ssn;
              local.New1.TakenDate = Now().Date;
              local.New1.TanfCode =
                entities.AdministrativeActCertification.TanfCode;
              local.New1.TransferState =
                entities.AdministrativeActCertification.TransferState;
              local.New1.TtypeAAddNewCase = "A";
              local.New1.TtypeDDeleteCertification = "";
              local.New1.TtypeLChangeSubmittingState = "";
              local.New1.TtypeMModifyAmount = "";
              local.New1.TtypeRModifyExclusion = "";
            }

            local.CsePerson.Number =
              local.BatchFdsoSnapshotFileRecord.CaseNumber;
            UseCreateFederalDebtSetoff();

            if (!IsExitState("ACO_NI0000_CREATE_OK"))
            {
              if (IsExitState("CSE_PERSON_ACCOUNT_NF"))
              {
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "Err-'snap which is not on Recon file'- 'create federal debt setoff'-cse_person_account_nf:" +
                  local.BatchFdsoSnapshotFileRecord.CaseNumber;
                UseCabErrorReport2();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                  return;
                }

                ExitState = "ACO_NN0000_ABEND_4_BATCH";

                return;
              }

              if (IsExitState("FN0000_OBLIGOR_NF"))
              {
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "Err-'snap which is not on Recon file'-'create federal debt setoff'-FN0000_OBLIGOR_NF:" +
                  local.BatchFdsoSnapshotFileRecord.CaseNumber;
                UseCabErrorReport2();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                  return;
                }

                ExitState = "ACO_NN0000_ABEND_4_BATCH";

                return;
              }

              if (IsExitState("FEDERAL_DEBT_SETOFF_AE"))
              {
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "Err-'snap which is not on Recon file'-'create federal debt setoff'-FEDERAL_DEBT_SETOFF_AE:" +
                  local.BatchFdsoSnapshotFileRecord.CaseNumber;
                UseCabErrorReport2();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                  return;
                }

                ExitState = "ACO_NN0000_ABEND_4_BATCH";

                return;
              }
            }
            else
            {
              ++local.TotalNumberOfAddRecord.Count;
            }

            ++local.TotalNumberOfMismatch.Count;
          }

          do
          {
            local.PassArea.FileInstruction = "READ2";
            UseLeEabProcessFdsoReconFile2();

            if (Equal(local.PassArea.TextReturnCode, "E2"))
            {
              local.EndOfSnapshotFile.Flag = "Y";

              // -------------------------------------------------
              // Since this is end of Snapshot file but we haven't
              // processed Recon record yet so set do not read
              // recon record flag to Y and escape out so that
              // this recon record will be retained and logic will
              // enter into 'end of Snapshot file = y and end of Recon
              // file not = y'.
              // -------------------------------------------------
              local.DoNotReadReconRecord.Flag = "Y";
            }

            if (!IsEmpty(local.PassArea.TextReturnCode))
            {
              if (AsChar(local.EndOfSnapshotFile.Flag) == 'Y')
              {
                goto Test7;
              }
              else
              {
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "Error encountered reading 'le_eab_process_fdso_recon_file' for Snapshot file.";
                  
                UseCabErrorReport2();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                  return;
                }

                ExitState = "FILE_OPEN_ERROR";

                return;
              }
            }
            else
            {
              ++local.TotalNumberOfSnapshotRecord.Count;
            }

            // ----------------------------------------------------------
            // If a person is written twice in a Snapshot
            // file due to ADC/NADC amount greater than 150/500
            // or due to local code change or exclusion indicator etc
            // then to avoid creating duplicate 'A' add or exit state
            // 'FDSO already exists', please check following conditions.
            // ----------------------------------------------------------
            if (Equal(local.BatchFdsoSnapshotFileRecord.CaseNumber,
              local.PreviousFdsoSnapshotFileRecord.CaseNumber))
            {
              continue;
            }
            else
            {
              local.PreviousFdsoSnapshotFileRecord.CaseNumber =
                local.BatchFdsoSnapshotFileRecord.CaseNumber;
            }

            if (Lt(local.BatchFdsoSnapshotFileRecord.CaseNumber,
              local.BatchFdsoReconciliationFileRecord.CaseNumber))
            {
              // ---------------------------------------------------------------
              // This means Fed does not have this person on their file/
              // database.
              // ---------------------------------------------------------------
              if (AsChar(local.BatchFdsoSnapshotFileRecord.TransactionType) == 'D'
                )
              {
                // ---------------------------------------------------------------
                // This means Fed has already got this delete record and we don'
                // t have to do anything, read next snapshot record.
                // ---------------------------------------------------------------
              }
              else if (AsChar(local.BatchFdsoSnapshotFileRecord.TransactionType) ==
                'M' || AsChar
                (local.BatchFdsoSnapshotFileRecord.TransactionType) == 'A')
              {
                // ---------------------------------------------------------------
                // Send this person again as add.
                // Read next Snapshot record.
                // ---------------------------------------------------------------
                if (ReadAdministrativeActCertification1())
                {
                  local.New1.AdcAmount =
                    entities.AdministrativeActCertification.AdcAmount;
                  local.New1.AmountOwed =
                    entities.AdministrativeActCertification.AmountOwed;
                  local.New1.CaseNumber =
                    entities.AdministrativeActCertification.CaseNumber;
                  local.New1.CaseType =
                    entities.AdministrativeActCertification.CaseType;
                  local.New1.CreatedBy = global.UserId;
                  local.New1.CreatedTstamp = Now();
                  local.New1.CurrentAmount =
                    entities.AdministrativeActCertification.CurrentAmount;
                  local.New1.CurrentAmountDate = Now().Date;
                  local.New1.DateSent = local.Null1.DateSent;
                  local.New1.EtypeAdministrativeOffset =
                    entities.AdministrativeActCertification.
                      EtypeAdministrativeOffset;
                  local.New1.EtypeFederalRetirement =
                    entities.AdministrativeActCertification.
                      EtypeFederalRetirement;
                  local.New1.EtypeFederalSalary =
                    entities.AdministrativeActCertification.EtypeFederalSalary;
                  local.New1.EtypeFinancialInstitution =
                    entities.AdministrativeActCertification.
                      EtypeFinancialInstitution;
                  local.New1.EtypePassportDenial =
                    entities.AdministrativeActCertification.EtypePassportDenial;
                    
                  local.New1.EtypeTaxRefund =
                    entities.AdministrativeActCertification.EtypeTaxRefund;
                  local.New1.EtypeVendorPaymentOrMisc =
                    entities.AdministrativeActCertification.
                      EtypeVendorPaymentOrMisc;
                  local.New1.FirstName =
                    entities.AdministrativeActCertification.FirstName;
                  local.New1.LastName =
                    entities.AdministrativeActCertification.LastName;
                  local.New1.LastUpdatedBy = global.UserId;
                  local.New1.LastUpdatedTstamp = Now();
                  local.New1.LocalCode =
                    entities.AdministrativeActCertification.LocalCode;
                  local.New1.NonAdcAmount =
                    entities.AdministrativeActCertification.NonAdcAmount;
                  local.New1.OriginalAmount =
                    entities.AdministrativeActCertification.OriginalAmount;
                  local.New1.ProcessYear =
                    entities.AdministrativeActCertification.ProcessYear;
                  local.New1.Ssn = entities.AdministrativeActCertification.Ssn;
                  local.New1.TakenDate = Now().Date;
                  local.New1.TanfCode =
                    entities.AdministrativeActCertification.TanfCode;
                  local.New1.TransferState =
                    entities.AdministrativeActCertification.TransferState;
                  local.New1.TtypeAAddNewCase = "A";
                  local.New1.TtypeDDeleteCertification = "";
                  local.New1.TtypeLChangeSubmittingState = "";
                  local.New1.TtypeMModifyAmount = "";
                  local.New1.TtypeRModifyExclusion = "";
                }

                local.CsePerson.Number =
                  local.BatchFdsoSnapshotFileRecord.CaseNumber;
                UseCreateFederalDebtSetoff();

                if (!IsExitState("ACO_NI0000_CREATE_OK"))
                {
                  if (IsExitState("CSE_PERSON_ACCOUNT_NF"))
                  {
                    local.EabFileHandling.Action = "WRITE";
                    local.EabReportSend.RptDetail =
                      "Err-'snap which is not on Recon file'-'create federal debt setoff'-cse_person_account_nf:" +
                      local.BatchFdsoSnapshotFileRecord.CaseNumber;
                    UseCabErrorReport2();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                      return;
                    }

                    ExitState = "ACO_NN0000_ABEND_4_BATCH";

                    return;
                  }

                  if (IsExitState("FN0000_OBLIGOR_NF"))
                  {
                    local.EabFileHandling.Action = "WRITE";
                    local.EabReportSend.RptDetail =
                      "Err-'snap which is not on Recon file'- 'create federal debt setoff'-FN0000_OBLIGOR_NF:" +
                      local.BatchFdsoSnapshotFileRecord.CaseNumber;
                    UseCabErrorReport2();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                      return;
                    }

                    ExitState = "ACO_NN0000_ABEND_4_BATCH";

                    return;
                  }

                  if (IsExitState("FEDERAL_DEBT_SETOFF_AE"))
                  {
                    local.EabFileHandling.Action = "WRITE";
                    local.EabReportSend.RptDetail =
                      "Err-'snap which is not on Recon file'-'create federal debt setoff'-FEDERAL_DEBT_SETOFF_AE:" +
                      local.BatchFdsoSnapshotFileRecord.CaseNumber;
                    UseCabErrorReport2();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                      return;
                    }

                    ExitState = "ACO_NN0000_ABEND_4_BATCH";

                    return;
                  }
                }
                else
                {
                  ++local.TotalNumberOfAddRecord.Count;
                }

                ++local.TotalNumberOfMismatch.Count;
              }
            }

            if (Equal(local.BatchFdsoSnapshotFileRecord.CaseNumber,
              local.BatchFdsoReconciliationFileRecord.CaseNumber))
            {
              goto Test7;
            }

            if (Lt(local.BatchFdsoReconciliationFileRecord.CaseNumber,
              local.BatchFdsoSnapshotFileRecord.CaseNumber))
            {
              goto Test6;
            }
          }
          while(!Equal(global.Command, "PERFECT"));
        }

Test6:

        if (Lt(local.BatchFdsoReconciliationFileRecord.CaseNumber,
          local.BatchFdsoSnapshotFileRecord.CaseNumber))
        {
          // ---------------------------------------------------------------
          // This means Fed has this person and we don't,so
          // check whether this person exists on our system, if yes
          // then send 'D' delete record else write it to error report and
          // let the logic fall down to read next Reconciliation record.
          // ---------------------------------------------------------------
          if (ReadAdministrativeActCertification2())
          {
            local.ReconPersonExistOnFdso.Flag = "Y";
          }

          if (AsChar(local.ReconPersonExistOnFdso.Flag) == 'Y')
          {
            local.ReconPersonExistOnFdso.Flag = "N";

            // ----------------------------------------------------------
            // This Reconciliation person exists on FDSO
            // so create a 'D' delete record with 0 amount
            // and set case type to Reconciliation case type,ssn,local code,
            // first name,last name as per Reconciliation file.
            // ----------------------------------------------------------
            local.New1.AdcAmount = 0;
            local.New1.CaseNumber =
              entities.AdministrativeActCertification.CaseNumber;
            local.New1.CaseType =
              local.BatchFdsoReconciliationFileRecord.CaseType;
            local.New1.CreatedBy = global.UserId;
            local.New1.CreatedTstamp = Now();
            local.New1.DateSent = local.Null1.DateSent;
            local.New1.FirstName =
              local.BatchFdsoReconciliationFileRecord.FirstName;
            local.New1.LastName =
              local.BatchFdsoReconciliationFileRecord.LastName;
            local.New1.LastUpdatedBy = global.UserId;
            local.New1.LastUpdatedTstamp = Now();
            local.New1.LocalCode =
              local.BatchFdsoReconciliationFileRecord.LocalCode;
            local.New1.NonAdcAmount = 0;
            local.New1.OriginalAmount =
              entities.AdministrativeActCertification.OriginalAmount;
            local.New1.ProcessYear =
              entities.AdministrativeActCertification.ProcessYear;
            local.New1.Ssn = local.BatchFdsoReconciliationFileRecord.Ssn;
            local.New1.TakenDate = Now().Date;
            local.New1.TanfCode =
              local.BatchFdsoReconciliationFileRecord.CaseType;
            local.New1.TransferState =
              entities.AdministrativeActCertification.TransferState;
            local.New1.TtypeAAddNewCase = "";
            local.New1.TtypeDDeleteCertification = "D";
            local.New1.TtypeLChangeSubmittingState = "";
            local.New1.TtypeMModifyAmount = "";
            local.New1.TtypeRModifyExclusion = "";

            // --------------------------------------------------
            // PR # 00125631
            // Beginning Of Change
            // Set Current Amount to zero. Also set other attributes
            // to proper value.
            // ---------------------------------------------------
            local.New1.AmountOwed = 0;
            local.New1.CurrentAmount = 0;
            local.New1.CurrentAmountDate = Now().Date;
            local.New1.EtypeAdministrativeOffset = "";
            local.New1.EtypeFederalRetirement = "";
            local.New1.EtypeFederalSalary = "";
            local.New1.EtypeFinancialInstitution = "";
            local.New1.EtypePassportDenial = "";
            local.New1.EtypeTaxRefund = "";
            local.New1.EtypeVendorPaymentOrMisc = "";

            // --------------------------------------------------
            // PR # 00125631
            // End Of Change.
            // ---------------------------------------------------
            local.CsePerson.Number =
              local.BatchFdsoReconciliationFileRecord.CaseNumber;
            UseCreateFederalDebtSetoff();

            if (!IsExitState("ACO_NI0000_CREATE_OK"))
            {
              if (IsExitState("CSE_PERSON_ACCOUNT_NF"))
              {
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "Err-'person existing on Recon file but not on snap'-'create federal debt setoff'-cse_person_account_nf:" +
                  local.BatchFdsoSnapshotFileRecord.CaseNumber;
                UseCabErrorReport2();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                  return;
                }

                ExitState = "ACO_NN0000_ABEND_4_BATCH";

                return;
              }

              if (IsExitState("FN0000_OBLIGOR_NF"))
              {
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "Err-'person existing on Recon file but not on snap'-'create federal debt setoff',-FN0000_OBLIGOR_NF:" +
                  local.BatchFdsoSnapshotFileRecord.CaseNumber;
                UseCabErrorReport2();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                  return;
                }

                ExitState = "ACO_NN0000_ABEND_4_BATCH";

                return;
              }

              if (IsExitState("FEDERAL_DEBT_SETOFF_AE"))
              {
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "Err-'person existing on Recon file but not on snap'-'create federal debt setoff'-FEDERAL_DEBT_SETOFF_AE:" +
                  local.BatchFdsoSnapshotFileRecord.CaseNumber;
                UseCabErrorReport2();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                  return;
                }

                ExitState = "ACO_NN0000_ABEND_4_BATCH";

                return;
              }
            }
            else
            {
              ++local.TotalNumberOfDeleteRecord.Count;
            }
          }
          else
          {
            // ----------------------------------------------------------
            // Check if person exists on our system.
            // ----------------------------------------------------------
            local.CsePerson.Number =
              local.BatchFdsoReconciliationFileRecord.CaseNumber;

            if (ReadCsePerson())
            {
              // ----------------------------------------------------------
              // Create a 'D' delete record.
              // ----------------------------------------------------------
              local.New1.AdcAmount = 0;
              local.New1.CaseNumber =
                local.BatchFdsoReconciliationFileRecord.CaseNumber;
              local.New1.CaseType =
                local.BatchFdsoReconciliationFileRecord.CaseType;
              local.New1.CreatedBy = global.UserId;
              local.New1.CreatedTstamp = Now();
              local.New1.DateSent = local.Null1.DateSent;
              local.New1.FirstName =
                local.BatchFdsoReconciliationFileRecord.FirstName;
              local.New1.LastName =
                local.BatchFdsoReconciliationFileRecord.LastName;
              local.New1.LastUpdatedBy = global.UserId;
              local.New1.LastUpdatedTstamp = Now();
              local.New1.LocalCode =
                local.BatchFdsoReconciliationFileRecord.LocalCode;
              local.New1.NonAdcAmount = 0;
              local.New1.OriginalAmount = 0;
              local.New1.ProcessYear =
                Year(local.ProgramProcessingInfo.ProcessDate);
              local.New1.Ssn = local.BatchFdsoReconciliationFileRecord.Ssn;
              local.New1.TakenDate = Now().Date;
              local.New1.TanfCode =
                local.BatchFdsoReconciliationFileRecord.CaseType;
              local.New1.TtypeAAddNewCase = "";
              local.New1.TtypeDDeleteCertification = "D";
              local.New1.TtypeLChangeSubmittingState = "";
              local.New1.TtypeMModifyAmount = "";
              local.New1.TtypeRModifyExclusion = "";

              // --------------------------------------------------
              // PR # 00125631
              // Beginning Of Change
              // Set Current Amount to zero. Also set other attributes
              // to proper value.
              // ---------------------------------------------------
              local.New1.AmountOwed = 0;
              local.New1.CurrentAmount = 0;
              local.New1.CurrentAmountDate = Now().Date;
              local.New1.EtypeAdministrativeOffset = "";
              local.New1.EtypeFederalRetirement = "";
              local.New1.EtypeFederalSalary = "";
              local.New1.EtypeFinancialInstitution = "";
              local.New1.EtypePassportDenial = "";
              local.New1.EtypeTaxRefund = "";
              local.New1.EtypeVendorPaymentOrMisc = "";

              // --------------------------------------------------
              // PR # 00125631
              // End Of Change.
              // ---------------------------------------------------
              local.CsePerson.Number =
                local.BatchFdsoReconciliationFileRecord.CaseNumber;
              UseCreateFederalDebtSetoff();

              if (!IsExitState("ACO_NI0000_CREATE_OK"))
              {
                if (IsExitState("CSE_PERSON_ACCOUNT_NF"))
                {
                  // ----------------------------------------------------------
                  // Since this person number doesn't exists
                  // on FDSO table and is not a obligor so create
                  // 'D' delete record and do not associate with
                  // obligor and adminstrative action.
                  // ----------------------------------------------------------
                  try
                  {
                    CreateFederalDebtSetoff1();
                    ++local.TotalNumberOfDeleteRecord.Count;
                    local.EabFileHandling.Action = "WRITE";
                    local.EabReportSend.RptDetail =
                      "This record should be deleted from FDSO table after sending it to Fed. Case_number : " +
                      local.BatchFdsoReconciliationFileRecord.CaseNumber;
                    UseCabErrorReport2();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                      return;
                    }
                  }
                  catch(Exception e)
                  {
                    switch(GetErrorCode(e))
                    {
                      case ErrorCode.AlreadyExists:
                        local.EabFileHandling.Action = "WRITE";
                        local.EabReportSend.RptDetail =
                          "Err-'person on Recon but not on snap'- 'under cse person acc nf'-FDSO_AE:" +
                          local.BatchFdsoSnapshotFileRecord.CaseNumber;
                        UseCabErrorReport2();

                        if (!Equal(local.EabFileHandling.Status, "OK"))
                        {
                          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                          return;
                        }

                        ExitState = "ACO_NN0000_ABEND_4_BATCH";

                        return;
                      case ErrorCode.PermittedValueViolation:
                        local.EabFileHandling.Action = "WRITE";
                        local.EabReportSend.RptDetail =
                          "Err-'person on Recon but not on snap'- 'under cse person acc nf'- permitted value:" +
                          local.BatchFdsoSnapshotFileRecord.CaseNumber;
                        UseCabErrorReport2();

                        if (!Equal(local.EabFileHandling.Status, "OK"))
                        {
                          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                          return;
                        }

                        ExitState = "ACO_NN0000_ABEND_4_BATCH";

                        return;
                      case ErrorCode.DatabaseError:
                        break;
                      default:
                        throw;
                    }
                  }
                }

                if (IsExitState("FN0000_OBLIGOR_NF"))
                {
                  // ----------------------------------------------------------
                  // Since this person number doesn't exists
                  // on FDSO table and is not associated to administrative
                  // action table so create
                  // 'D' delete record and do not associate with
                  // obligor and adminstrative action.
                  // ----------------------------------------------------------
                  try
                  {
                    CreateFederalDebtSetoff1();
                    ++local.TotalNumberOfDeleteRecord.Count;
                    local.EabFileHandling.Action = "WRITE";
                    local.EabReportSend.RptDetail =
                      "This record should be deleted from FDSO table after sending it to Fed. Case_number : " +
                      local.BatchFdsoReconciliationFileRecord.CaseNumber;
                    UseCabErrorReport2();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                      return;
                    }
                  }
                  catch(Exception e)
                  {
                    switch(GetErrorCode(e))
                    {
                      case ErrorCode.AlreadyExists:
                        local.EabFileHandling.Action = "WRITE";
                        local.EabReportSend.RptDetail =
                          "Err-'person on Recon but not on snap'- 'under fn0000_obligor_nf'-FDSO_AE:" +
                          local.BatchFdsoSnapshotFileRecord.CaseNumber;
                        UseCabErrorReport2();

                        if (!Equal(local.EabFileHandling.Status, "OK"))
                        {
                          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                          return;
                        }

                        ExitState = "ACO_NN0000_ABEND_4_BATCH";

                        return;
                      case ErrorCode.PermittedValueViolation:
                        local.EabFileHandling.Action = "WRITE";
                        local.EabReportSend.RptDetail =
                          "Err-'person on Recon but not on snap'- 'under fn0000_obligor_nf'- permitted value:" +
                          local.BatchFdsoSnapshotFileRecord.CaseNumber;
                        UseCabErrorReport2();

                        if (!Equal(local.EabFileHandling.Status, "OK"))
                        {
                          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                          return;
                        }

                        ExitState = "ACO_NN0000_ABEND_4_BATCH";

                        return;
                      case ErrorCode.DatabaseError:
                        break;
                      default:
                        throw;
                    }
                  }
                }

                if (IsExitState("FEDERAL_DEBT_SETOFF_AE"))
                {
                  local.EabFileHandling.Action = "WRITE";
                  local.EabReportSend.RptDetail =
                    "Err-'person existing on Recon file but not on snap'-'create federal debt setoff'-FEDERAL_DEBT_SETOFF_AE:" +
                    local.BatchFdsoSnapshotFileRecord.CaseNumber;
                  UseCabErrorReport2();

                  if (!Equal(local.EabFileHandling.Status, "OK"))
                  {
                    ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                    return;
                  }

                  ExitState = "ACO_NN0000_ABEND_4_BATCH";

                  return;
                }
              }
              else
              {
                ++local.TotalNumberOfDeleteRecord.Count;
              }

              ++local.TotalNumberOfMismatch.Count;
            }
            else
            {
              // ----------------------------------------------------------
              // Since this person does not exists on our system,
              // write it to error report.
              // ----------------------------------------------------------
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "This cse person received from Reconciliation file does not exists on our system : " +
                local.BatchFdsoReconciliationFileRecord.CaseNumber;
              UseCabErrorReport2();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }

              ++local.TotalNumberOfMismatch.Count;
            }
          }

          local.ReadNextReconRecord.Flag = "Y";
        }
      }

Test7:

      if (Equal(local.BatchFdsoReconciliationFileRecord.CaseNumber,
        local.BatchFdsoSnapshotFileRecord.CaseNumber))
      {
        if (ReadAdministrativeActCertification1())
        {
          // --------------------------------------------------------------
          // Since adc and nadc amount has decimal (cents),
          // set it to different local views not rounded.
          // --------------------------------------------------------------
          local.FdsoCertificationTotalRecord.TanfAmount =
            (long)entities.AdministrativeActCertification.AdcAmount.
              GetValueOrDefault();
          local.FdsoCertificationTotalRecord.NontanfAmount =
            (long)entities.AdministrativeActCertification.NonAdcAmount.
              GetValueOrDefault();
        }

        // --------------------------------------------------------------
        // PR # 00121434
        // Beginning of Change
        // If Pre-offset Notice Date is blank and Pre-offset Hold Indicator = H,
        // that means this person is put on 'Hold' by OCSE.
        // Check whether this person has been 'Decertified' from our system, if 
        // yes
        // then do nothing, else send this person as'Add'.
        // --------------------------------------------------------------
        if (IsEmpty(local.BatchFdsoReconciliationFileRecord.PreoffsetNoticeDate) &&
          AsChar(local.BatchFdsoReconciliationFileRecord.PreoffsetHoldInd) == 'H'
          )
        {
          if (local.BatchFdsoSnapshotFileRecord.CurrentArrearageAmount == 0 && Equal
            (entities.AdministrativeActCertification.AdcAmount, 0) && Equal
            (entities.AdministrativeActCertification.NonAdcAmount, 0) && AsChar
            (entities.AdministrativeActCertification.TtypeDDeleteCertification) ==
              'D')
          {
            goto Test8;
          }
          else
          {
            // --------------------------------------------------------------
            // Re-send this person as Add
            // --------------------------------------------------------------
            local.New1.AdcAmount =
              entities.AdministrativeActCertification.AdcAmount;
            local.New1.AmountOwed =
              entities.AdministrativeActCertification.AmountOwed;
            local.New1.CaseNumber =
              entities.AdministrativeActCertification.CaseNumber;
            local.New1.CaseType =
              entities.AdministrativeActCertification.CaseType;
            local.New1.CreatedBy = global.UserId;
            local.New1.CreatedTstamp = Now();
            local.New1.CurrentAmount =
              entities.AdministrativeActCertification.CurrentAmount;
            local.New1.CurrentAmountDate = Now().Date;
            local.New1.DateSent = local.Null1.DateSent;
            local.New1.EtypeAdministrativeOffset =
              entities.AdministrativeActCertification.EtypeAdministrativeOffset;
              
            local.New1.EtypeFederalRetirement =
              entities.AdministrativeActCertification.EtypeFederalRetirement;
            local.New1.EtypeFederalSalary =
              entities.AdministrativeActCertification.EtypeFederalSalary;
            local.New1.EtypeFinancialInstitution =
              entities.AdministrativeActCertification.EtypeFinancialInstitution;
              
            local.New1.EtypePassportDenial =
              entities.AdministrativeActCertification.EtypePassportDenial;
            local.New1.EtypeTaxRefund =
              entities.AdministrativeActCertification.EtypeTaxRefund;
            local.New1.EtypeVendorPaymentOrMisc =
              entities.AdministrativeActCertification.EtypeVendorPaymentOrMisc;
            local.New1.FirstName =
              entities.AdministrativeActCertification.FirstName;
            local.New1.LastName =
              entities.AdministrativeActCertification.LastName;
            local.New1.LastUpdatedBy = global.UserId;
            local.New1.LastUpdatedTstamp = Now();
            local.New1.LocalCode =
              entities.AdministrativeActCertification.LocalCode;
            local.New1.NonAdcAmount =
              entities.AdministrativeActCertification.NonAdcAmount;
            local.New1.OriginalAmount =
              entities.AdministrativeActCertification.OriginalAmount;
            local.New1.ProcessYear =
              entities.AdministrativeActCertification.ProcessYear;
            local.New1.Ssn = entities.AdministrativeActCertification.Ssn;
            local.New1.TakenDate = Now().Date;
            local.New1.TanfCode =
              entities.AdministrativeActCertification.TanfCode;
            local.New1.TransferState =
              entities.AdministrativeActCertification.TransferState;
            local.New1.TtypeAAddNewCase = "A";
            local.New1.TtypeDDeleteCertification = "";
            local.New1.TtypeLChangeSubmittingState = "";
            local.New1.TtypeMModifyAmount = "";
            local.New1.TtypeRModifyExclusion = "";
            local.CsePerson.Number =
              local.BatchFdsoSnapshotFileRecord.CaseNumber;
            UseCreateFederalDebtSetoff();

            if (!IsExitState("ACO_NI0000_CREATE_OK"))
            {
              if (IsExitState("CSE_PERSON_ACCOUNT_NF"))
              {
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "Error while creating Add for Hold type of cases. cse_person_account_nf:" +
                  local.BatchFdsoSnapshotFileRecord.CaseNumber;
                UseCabErrorReport2();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                  return;
                }

                ExitState = "ACO_NN0000_ABEND_4_BATCH";

                return;
              }

              if (IsExitState("FN0000_OBLIGOR_NF"))
              {
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "Error while creating Add for Hold type of cases. FN0000_OBLIGOR_NF:" +
                  local.BatchFdsoSnapshotFileRecord.CaseNumber;
                UseCabErrorReport2();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                  return;
                }

                ExitState = "ACO_NN0000_ABEND_4_BATCH";

                return;
              }

              if (IsExitState("FEDERAL_DEBT_SETOFF_AE"))
              {
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "Error while creating Add for Hold type of cases. FDSO_AE:" +
                  local.BatchFdsoSnapshotFileRecord.CaseNumber;
                UseCabErrorReport2();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                  return;
                }

                ExitState = "ACO_NN0000_ABEND_4_BATCH";

                return;
              }
            }
            else
            {
              local.AlreadyModifyPrev.CaseNumber =
                local.BatchFdsoSnapshotFileRecord.CaseNumber;
              ++local.TotalNumberOfAddRecord.Count;

              goto Test8;
            }
          }
        }

        // --------------------------------------------------------------
        // PR # 00121434
        // End of Change
        // --------------------------------------------------------------
        if (AsChar(local.BatchFdsoReconciliationFileRecord.CaseType) == 'A')
        {
          if (local.BatchFdsoReconciliationFileRecord.CurrentArrearageAmount !=
            local.BatchFdsoSnapshotFileRecord.CurrentArrearageAmount)
          {
            if (local.BatchFdsoSnapshotFileRecord.CurrentArrearageAmount == local
              .FdsoCertificationTotalRecord.TanfAmount)
            {
              if (local.BatchFdsoSnapshotFileRecord.CurrentArrearageAmount == 0
                && Equal
                (entities.AdministrativeActCertification.AdcAmount, 0) && Equal
                (entities.AdministrativeActCertification.NonAdcAmount, 0) && AsChar
                (entities.AdministrativeActCertification.
                  TtypeDDeleteCertification) == 'D')
              {
                // --------------------------------------------------------------
                // Re-send this Delete person again to Fed.
                // --------------------------------------------------------------
                local.New1.AdcAmount =
                  entities.AdministrativeActCertification.AdcAmount;
                local.New1.AmountOwed =
                  entities.AdministrativeActCertification.AmountOwed;
                local.New1.CaseNumber =
                  entities.AdministrativeActCertification.CaseNumber;
                local.New1.CaseType = "A";
                local.New1.CreatedBy = global.UserId;
                local.New1.CreatedTstamp = Now();
                local.New1.CurrentAmount =
                  entities.AdministrativeActCertification.CurrentAmount;
                local.New1.CurrentAmountDate = Now().Date;
                local.New1.DateSent = local.Null1.DateSent;
                local.New1.EtypeAdministrativeOffset =
                  entities.AdministrativeActCertification.
                    EtypeAdministrativeOffset;
                local.New1.EtypeFederalRetirement =
                  entities.AdministrativeActCertification.
                    EtypeFederalRetirement;
                local.New1.EtypeFederalSalary =
                  entities.AdministrativeActCertification.EtypeFederalSalary;
                local.New1.EtypeFinancialInstitution =
                  entities.AdministrativeActCertification.
                    EtypeFinancialInstitution;
                local.New1.EtypePassportDenial =
                  entities.AdministrativeActCertification.EtypePassportDenial;
                local.New1.EtypeTaxRefund =
                  entities.AdministrativeActCertification.EtypeTaxRefund;
                local.New1.EtypeVendorPaymentOrMisc =
                  entities.AdministrativeActCertification.
                    EtypeVendorPaymentOrMisc;
                local.New1.FirstName =
                  entities.AdministrativeActCertification.FirstName;
                local.New1.LastName =
                  entities.AdministrativeActCertification.LastName;
                local.New1.LastUpdatedBy = global.UserId;
                local.New1.LastUpdatedTstamp = Now();
                local.New1.LocalCode =
                  entities.AdministrativeActCertification.LocalCode;
                local.New1.NonAdcAmount =
                  entities.AdministrativeActCertification.NonAdcAmount;
                local.New1.OriginalAmount =
                  entities.AdministrativeActCertification.OriginalAmount;
                local.New1.ProcessYear =
                  entities.AdministrativeActCertification.ProcessYear;
                local.New1.Ssn = entities.AdministrativeActCertification.Ssn;
                local.New1.TakenDate = Now().Date;
                local.New1.TanfCode = "A";
                local.New1.TransferState =
                  entities.AdministrativeActCertification.TransferState;
                local.New1.TtypeAAddNewCase =
                  entities.AdministrativeActCertification.TtypeAAddNewCase;
                local.New1.TtypeDDeleteCertification =
                  entities.AdministrativeActCertification.
                    TtypeDDeleteCertification;
                local.New1.TtypeLChangeSubmittingState =
                  entities.AdministrativeActCertification.
                    TtypeLChangeSubmittingState;
                local.New1.TtypeMModifyAmount =
                  entities.AdministrativeActCertification.TtypeMModifyAmount;
                local.New1.TtypeRModifyExclusion =
                  entities.AdministrativeActCertification.TtypeRModifyExclusion;
                  
                local.CsePerson.Number =
                  local.BatchFdsoSnapshotFileRecord.CaseNumber;
                UseCreateFederalDebtSetoff();

                if (!IsExitState("ACO_NI0000_CREATE_OK"))
                {
                  if (IsExitState("CSE_PERSON_ACCOUNT_NF"))
                  {
                    local.EabFileHandling.Action = "WRITE";
                    local.EabReportSend.RptDetail =
                      "Err-'create delete'-'create federal debt setoff'-cse_person_account_nf:" +
                      local.BatchFdsoSnapshotFileRecord.CaseNumber;
                    UseCabErrorReport2();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                      return;
                    }

                    ExitState = "ACO_NN0000_ABEND_4_BATCH";

                    return;
                  }

                  if (IsExitState("FN0000_OBLIGOR_NF"))
                  {
                    local.EabFileHandling.Action = "WRITE";
                    local.EabReportSend.RptDetail =
                      "Err-'create delete'- 'create federal debt setoff'-FN0000_OBLIGOR_NF:" +
                      local.BatchFdsoSnapshotFileRecord.CaseNumber;
                    UseCabErrorReport2();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                      return;
                    }

                    ExitState = "ACO_NN0000_ABEND_4_BATCH";

                    return;
                  }

                  if (IsExitState("FEDERAL_DEBT_SETOFF_AE"))
                  {
                    local.EabFileHandling.Action = "WRITE";
                    local.EabReportSend.RptDetail =
                      "Err-'create delete'-'create federal debt setoff'-FDSO_AE:" +
                      local.BatchFdsoSnapshotFileRecord.CaseNumber;
                    UseCabErrorReport2();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                      return;
                    }

                    ExitState = "ACO_NN0000_ABEND_4_BATCH";

                    return;
                  }
                }
                else
                {
                  local.AlreadyModifyPrev.CaseNumber =
                    local.BatchFdsoSnapshotFileRecord.CaseNumber;
                  ++local.TotalNumberOfDeleteResend.Count;

                  goto Test8;
                }
              }

              // --------------------------------------------------------------
              // Re-send this person again to Fed as 'M' modify.
              // --------------------------------------------------------------
              local.AmountDoesNotMatch.Flag = "Y";
              ++local.TotalNumberOfMismatch.Count;
            }
            else
            {
              // --------------------------------------------------------------
              // This means the transaction has been sent to Fed
              // since the target date to update the arrearage amount.
              // --------------------------------------------------------------
            }
          }
        }

        if (AsChar(local.BatchFdsoReconciliationFileRecord.CaseType) == 'N')
        {
          if (local.BatchFdsoReconciliationFileRecord.CurrentArrearageAmount !=
            local.BatchFdsoSnapshotFileRecord.CurrentArrearageAmount)
          {
            if (local.BatchFdsoSnapshotFileRecord.CurrentArrearageAmount == local
              .FdsoCertificationTotalRecord.NontanfAmount)
            {
              if (local.BatchFdsoSnapshotFileRecord.CurrentArrearageAmount == 0
                && Equal
                (entities.AdministrativeActCertification.AdcAmount, 0) && Equal
                (entities.AdministrativeActCertification.NonAdcAmount, 0) && AsChar
                (entities.AdministrativeActCertification.
                  TtypeDDeleteCertification) == 'D')
              {
                // --------------------------------------------------------------
                // Re-send this Delete person again to Fed.
                // --------------------------------------------------------------
                local.New1.AdcAmount =
                  entities.AdministrativeActCertification.AdcAmount;
                local.New1.AmountOwed =
                  entities.AdministrativeActCertification.AmountOwed;
                local.New1.CaseNumber =
                  entities.AdministrativeActCertification.CaseNumber;
                local.New1.CaseType = "N";
                local.New1.CreatedBy = global.UserId;
                local.New1.CreatedTstamp = Now();
                local.New1.CurrentAmount =
                  entities.AdministrativeActCertification.CurrentAmount;
                local.New1.CurrentAmountDate = Now().Date;
                local.New1.DateSent = local.Null1.DateSent;
                local.New1.EtypeAdministrativeOffset =
                  entities.AdministrativeActCertification.
                    EtypeAdministrativeOffset;
                local.New1.EtypeFederalRetirement =
                  entities.AdministrativeActCertification.
                    EtypeFederalRetirement;
                local.New1.EtypeFederalSalary =
                  entities.AdministrativeActCertification.EtypeFederalSalary;
                local.New1.EtypeFinancialInstitution =
                  entities.AdministrativeActCertification.
                    EtypeFinancialInstitution;
                local.New1.EtypePassportDenial =
                  entities.AdministrativeActCertification.EtypePassportDenial;
                local.New1.EtypeTaxRefund =
                  entities.AdministrativeActCertification.EtypeTaxRefund;
                local.New1.EtypeVendorPaymentOrMisc =
                  entities.AdministrativeActCertification.
                    EtypeVendorPaymentOrMisc;
                local.New1.FirstName =
                  entities.AdministrativeActCertification.FirstName;
                local.New1.LastName =
                  entities.AdministrativeActCertification.LastName;
                local.New1.LastUpdatedBy = global.UserId;
                local.New1.LastUpdatedTstamp = Now();
                local.New1.LocalCode =
                  entities.AdministrativeActCertification.LocalCode;
                local.New1.NonAdcAmount =
                  entities.AdministrativeActCertification.NonAdcAmount;
                local.New1.OriginalAmount =
                  entities.AdministrativeActCertification.OriginalAmount;
                local.New1.ProcessYear =
                  entities.AdministrativeActCertification.ProcessYear;
                local.New1.Ssn = entities.AdministrativeActCertification.Ssn;
                local.New1.TakenDate = Now().Date;
                local.New1.TanfCode = "N";
                local.New1.TransferState =
                  entities.AdministrativeActCertification.TransferState;
                local.New1.TtypeAAddNewCase =
                  entities.AdministrativeActCertification.TtypeAAddNewCase;
                local.New1.TtypeDDeleteCertification =
                  entities.AdministrativeActCertification.
                    TtypeDDeleteCertification;
                local.New1.TtypeLChangeSubmittingState =
                  entities.AdministrativeActCertification.
                    TtypeLChangeSubmittingState;
                local.New1.TtypeMModifyAmount =
                  entities.AdministrativeActCertification.TtypeMModifyAmount;
                local.New1.TtypeRModifyExclusion =
                  entities.AdministrativeActCertification.TtypeRModifyExclusion;
                  
                local.CsePerson.Number =
                  local.BatchFdsoSnapshotFileRecord.CaseNumber;
                UseCreateFederalDebtSetoff();

                if (!IsExitState("ACO_NI0000_CREATE_OK"))
                {
                  if (IsExitState("CSE_PERSON_ACCOUNT_NF"))
                  {
                    local.EabFileHandling.Action = "WRITE";
                    local.EabReportSend.RptDetail =
                      "Err-'create delete'-'create federal debt setoff'-cse_person_account_nf:" +
                      local.BatchFdsoSnapshotFileRecord.CaseNumber;
                    UseCabErrorReport2();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                      return;
                    }

                    ExitState = "ACO_NN0000_ABEND_4_BATCH";

                    return;
                  }

                  if (IsExitState("FN0000_OBLIGOR_NF"))
                  {
                    local.EabFileHandling.Action = "WRITE";
                    local.EabReportSend.RptDetail =
                      "Err-'create delete'- 'create federal debt setoff'-FN0000_OBLIGOR_NF:" +
                      local.BatchFdsoSnapshotFileRecord.CaseNumber;
                    UseCabErrorReport2();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                      return;
                    }

                    ExitState = "ACO_NN0000_ABEND_4_BATCH";

                    return;
                  }

                  if (IsExitState("FEDERAL_DEBT_SETOFF_AE"))
                  {
                    local.EabFileHandling.Action = "WRITE";
                    local.EabReportSend.RptDetail =
                      "Err-'create delete'-'create federal debt setoff'-FDSO_AE:" +
                      local.BatchFdsoSnapshotFileRecord.CaseNumber;
                    UseCabErrorReport2();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                      return;
                    }

                    ExitState = "ACO_NN0000_ABEND_4_BATCH";

                    return;
                  }
                }
                else
                {
                  local.AlreadyModifyPrev.CaseNumber =
                    local.BatchFdsoSnapshotFileRecord.CaseNumber;
                  ++local.TotalNumberOfDeleteResend.Count;

                  goto Test8;
                }
              }

              // --------------------------------------------------------------
              // Re-send this person again to Fed as 'M' modify.
              // --------------------------------------------------------------
              local.AmountDoesNotMatch.Flag = "Y";
              ++local.TotalNumberOfMismatch.Count;
            }
            else
            {
              // --------------------------------------------------------------
              // This means the transaction has been sent to Fed
              // since the target date to update the arrearage amount.
              // --------------------------------------------------------------
            }
          }
        }

        if (!Equal(local.BatchFdsoReconciliationFileRecord.
          EtypeAdministrativeOffset,
          local.BatchFdsoSnapshotFileRecord.EtypeAdministrativeOffset))
        {
          if (!IsEmpty(local.BatchFdsoSnapshotFileRecord.
            EtypeAdministrativeOffset))
          {
            local.BatchFdsoSnapshotFileRecord.EtypeAdministrativeOffset = "Y";
          }

          if (Equal(local.BatchFdsoSnapshotFileRecord.EtypeAdministrativeOffset,
            entities.AdministrativeActCertification.EtypeAdministrativeOffset))
          {
            // --------------------------------------------------------------
            // Re-send this person again to Fed as 'M' modify.
            // --------------------------------------------------------------
            local.EtypeAdmDoesNotMatch.Flag = "Y";
            ++local.TotalNumberOfMismatch.Count;
          }
          else
          {
            // --------------------------------------------------------------
            // This means the transaction has been sent to Fed
            // since the target date to update the ADM exclusion indicator.
            // --------------------------------------------------------------
          }
        }

        if (!Equal(local.BatchFdsoReconciliationFileRecord.
          EtypeFederalRetirement,
          local.BatchFdsoSnapshotFileRecord.EtypeFederalRetirement))
        {
          if (!IsEmpty(local.BatchFdsoSnapshotFileRecord.EtypeFederalRetirement))
            
          {
            local.BatchFdsoSnapshotFileRecord.EtypeFederalRetirement = "Y";
          }

          if (Equal(local.BatchFdsoSnapshotFileRecord.EtypeFederalRetirement,
            entities.AdministrativeActCertification.EtypeFederalRetirement))
          {
            // --------------------------------------------------------------
            // Re-send this person again to Fed as 'M' modify.
            // --------------------------------------------------------------
            local.EtypeRetDoesNotMatch.Flag = "Y";
            ++local.TotalNumberOfMismatch.Count;
          }
          else
          {
            // --------------------------------------------------------------
            // This means the transaction has been sent to Fed
            // since the target date to update the RET exclusion indicator.
            // --------------------------------------------------------------
          }
        }

        if (!Equal(local.BatchFdsoReconciliationFileRecord.
          EtypeVendorPaymentOrMisc,
          local.BatchFdsoSnapshotFileRecord.EtypeVendorPaymentOrMisc))
        {
          if (!IsEmpty(local.BatchFdsoSnapshotFileRecord.
            EtypeVendorPaymentOrMisc))
          {
            local.BatchFdsoSnapshotFileRecord.EtypeVendorPaymentOrMisc = "Y";
          }

          if (Equal(local.BatchFdsoSnapshotFileRecord.EtypeVendorPaymentOrMisc,
            entities.AdministrativeActCertification.EtypeVendorPaymentOrMisc))
          {
            // --------------------------------------------------------------
            // Re-send this person again to Fed as 'M' modify.
            // --------------------------------------------------------------
            local.EtypeVenDoesNotMatch.Flag = "Y";
            ++local.TotalNumberOfMismatch.Count;
          }
          else
          {
            // --------------------------------------------------------------
            // This means the transaction has been sent to Fed
            // since the target date to update the VEN exclusion indicator.
            // --------------------------------------------------------------
          }
        }

        if (!Equal(local.BatchFdsoReconciliationFileRecord.EtypeFederalSalary,
          local.BatchFdsoSnapshotFileRecord.EtypeFederalSalary))
        {
          if (!IsEmpty(local.BatchFdsoSnapshotFileRecord.EtypeFederalSalary))
          {
            local.BatchFdsoSnapshotFileRecord.EtypeFederalSalary = "Y";
          }

          if (Equal(local.BatchFdsoSnapshotFileRecord.EtypeFederalSalary,
            entities.AdministrativeActCertification.EtypeFederalSalary))
          {
            // --------------------------------------------------------------
            // Re-send this person again to Fed as 'M' modify.
            // --------------------------------------------------------------
            local.EtypeSalDoesNotMatch.Flag = "Y";
            ++local.TotalNumberOfMismatch.Count;
          }
          else
          {
            // --------------------------------------------------------------
            // This means the transaction has been sent to Fed
            // since the target date to update the SAL exclusion indicator.
            // --------------------------------------------------------------
          }
        }

        if (!Equal(local.BatchFdsoReconciliationFileRecord.EtypeTaxRefund,
          local.BatchFdsoSnapshotFileRecord.EtypeTaxRefund))
        {
          if (!IsEmpty(local.BatchFdsoSnapshotFileRecord.EtypeTaxRefund))
          {
            local.BatchFdsoSnapshotFileRecord.EtypeTaxRefund = "Y";
          }

          if (Equal(local.BatchFdsoSnapshotFileRecord.EtypeTaxRefund,
            entities.AdministrativeActCertification.EtypeTaxRefund))
          {
            // --------------------------------------------------------------
            // Re-send this person again to Fed as 'M' modify.
            // --------------------------------------------------------------
            local.EtypeTaxDoesNotMatch.Flag = "Y";
            ++local.TotalNumberOfMismatch.Count;
          }
          else
          {
            // --------------------------------------------------------------
            // This means the transaction has been sent to Fed
            // since the target date to update the TAX exclusion indicator.
            // --------------------------------------------------------------
          }
        }

        if (!Equal(local.BatchFdsoReconciliationFileRecord.EtypePassportDenial,
          local.BatchFdsoSnapshotFileRecord.EtypePassportDenial))
        {
          if (!IsEmpty(local.BatchFdsoSnapshotFileRecord.EtypePassportDenial))
          {
            local.BatchFdsoSnapshotFileRecord.EtypePassportDenial = "Y";
          }

          if (Equal(local.BatchFdsoSnapshotFileRecord.EtypePassportDenial,
            entities.AdministrativeActCertification.EtypePassportDenial))
          {
            // --------------------------------------------------------------
            // Re-send this person again to Fed as 'M' modify.
            // --------------------------------------------------------------
            local.EtypePasDoesNotMatch.Flag = "Y";
            ++local.TotalNumberOfMismatch.Count;
          }
          else
          {
            // --------------------------------------------------------------
            // This means the transaction has been sent to Fed
            // since the target date to update the PAS exclusion indicator.
            // --------------------------------------------------------------
          }
        }

        if (!Equal(local.BatchFdsoReconciliationFileRecord.
          EtypeFinancialInstitution,
          local.BatchFdsoSnapshotFileRecord.EtypeFinancialInstitution))
        {
          if (!IsEmpty(local.BatchFdsoSnapshotFileRecord.
            EtypeFinancialInstitution))
          {
            local.BatchFdsoSnapshotFileRecord.EtypeFinancialInstitution = "Y";
          }

          if (Equal(local.BatchFdsoSnapshotFileRecord.EtypeFinancialInstitution,
            entities.AdministrativeActCertification.EtypeFinancialInstitution))
          {
            // --------------------------------------------------------------
            // Re-send this person again to Fed as 'M' modify.
            // --------------------------------------------------------------
            local.EtypeFidmDoesNotMatch.Flag = "Y";
            ++local.TotalNumberOfMismatch.Count;
          }
          else
          {
            // --------------------------------------------------------------
            // This means the transaction has been sent to Fed
            // since the target date to update the FIN exclusion indicator.
            // --------------------------------------------------------------
          }
        }

        if (local.BatchFdsoReconciliationFileRecord.Ssn != local
          .BatchFdsoSnapshotFileRecord.Ssn)
        {
          // --------------------------------------------------------------
          // Write error to error report with description
          // 'SSN does not match with Reconciliation SSN'.
          // --------------------------------------------------------------
          local.SsnDoesNotMatch.Flag = "Y";
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "SSN does not match with Reconciliation SSN , cse_person : " + local
            .BatchFdsoReconciliationFileRecord.CaseNumber;
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }

        if (!Equal(local.BatchFdsoReconciliationFileRecord.LocalCode,
          local.BatchFdsoSnapshotFileRecord.LocalCode))
        {
          if (Equal(local.BatchFdsoSnapshotFileRecord.LocalCode,
            entities.AdministrativeActCertification.LocalCode))
          {
            // --------------------------------------------------------------
            // Re-send this person again to Fed as 'M' modify.
            // --------------------------------------------------------------
            local.CodeDoesNotMatch.Flag = "Y";
            ++local.TotalNumberOfMismatch.Count;
          }
          else
          {
            // --------------------------------------------------------------
            // This means the transaction has been sent to Fed
            // since the target date to update the Local Code.
            // --------------------------------------------------------------
          }
        }

        if (!Equal(local.BatchFdsoReconciliationFileRecord.LastName, 1, 4,
          local.BatchFdsoSnapshotFileRecord.LastName, 1, 4))
        {
          // --------------------------------------------------------------
          // Write error to error report with description
          // 'Last name does not match OCSE certified Last name'.
          // --------------------------------------------------------------
          local.LastNameDoesNotMatch.Flag = "Y";
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Last Name does not match OCSE certified Last name , cse_person : " +
            local.BatchFdsoReconciliationFileRecord.CaseNumber;
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }

        if (!Equal(local.BatchFdsoReconciliationFileRecord.FirstName,
          local.BatchFdsoSnapshotFileRecord.FirstName))
        {
          // --------------------------------------------------------------
          // Write error to error report with description
          // 'First name does not match OCSE certified First name'.
          // --------------------------------------------------------------
          local.FirstNameDoesNotMatch.Flag = "Y";
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "First Name does not match OCSE certified First name , cse_person : " +
            local.BatchFdsoReconciliationFileRecord.CaseNumber;
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }

        if (AsChar(local.AmountDoesNotMatch.Flag) == 'Y' || AsChar
          (local.EtypeAdmDoesNotMatch.Flag) == 'Y' || AsChar
          (local.EtypeRetDoesNotMatch.Flag) == 'Y' || AsChar
          (local.EtypeVenDoesNotMatch.Flag) == 'Y' || AsChar
          (local.EtypeSalDoesNotMatch.Flag) == 'Y' || AsChar
          (local.EtypeTaxDoesNotMatch.Flag) == 'Y' || AsChar
          (local.EtypePasDoesNotMatch.Flag) == 'Y' || AsChar
          (local.EtypeFidmDoesNotMatch.Flag) == 'Y' || AsChar
          (local.SsnDoesNotMatch.Flag) == 'Y' || AsChar
          (local.CodeDoesNotMatch.Flag) == 'Y' || AsChar
          (local.FirstNameDoesNotMatch.Flag) == 'Y' || AsChar
          (local.LastNameDoesNotMatch.Flag) == 'Y')
        {
          if ((AsChar(local.FirstNameDoesNotMatch.Flag) == 'Y' || AsChar
            (local.SsnDoesNotMatch.Flag) == 'Y') && AsChar
            (local.AmountDoesNotMatch.Flag) != 'Y' && AsChar
            (local.EtypeAdmDoesNotMatch.Flag) != 'Y' && AsChar
            (local.EtypeRetDoesNotMatch.Flag) != 'Y' && AsChar
            (local.EtypeVenDoesNotMatch.Flag) != 'Y' && AsChar
            (local.EtypeSalDoesNotMatch.Flag) != 'Y' && AsChar
            (local.EtypeTaxDoesNotMatch.Flag) != 'Y' && AsChar
            (local.EtypePasDoesNotMatch.Flag) != 'Y' && AsChar
            (local.EtypeFidmDoesNotMatch.Flag) != 'Y' && AsChar
            (local.CodeDoesNotMatch.Flag) != 'Y' && AsChar
            (local.LastNameDoesNotMatch.Flag) != 'Y')
          {
          }
          else
          {
            local.New1.AdcAmount =
              entities.AdministrativeActCertification.AdcAmount;
            local.New1.AmountOwed =
              entities.AdministrativeActCertification.AmountOwed;
            local.New1.CaseNumber =
              entities.AdministrativeActCertification.CaseNumber;
            local.New1.CaseType =
              entities.AdministrativeActCertification.CaseType;
            local.New1.CreatedBy = global.UserId;
            local.New1.CreatedTstamp = Now();
            local.New1.CurrentAmount =
              entities.AdministrativeActCertification.CurrentAmount;
            local.New1.CurrentAmountDate = Now().Date;
            local.New1.DateSent = local.Null1.DateSent;
            local.New1.EtypeAdministrativeOffset =
              entities.AdministrativeActCertification.EtypeAdministrativeOffset;
              
            local.New1.EtypeFederalRetirement =
              entities.AdministrativeActCertification.EtypeFederalRetirement;
            local.New1.EtypeFederalSalary =
              entities.AdministrativeActCertification.EtypeFederalSalary;
            local.New1.EtypeFinancialInstitution =
              entities.AdministrativeActCertification.EtypeFinancialInstitution;
              
            local.New1.EtypePassportDenial =
              entities.AdministrativeActCertification.EtypePassportDenial;
            local.New1.EtypeTaxRefund =
              entities.AdministrativeActCertification.EtypeTaxRefund;
            local.New1.EtypeVendorPaymentOrMisc =
              entities.AdministrativeActCertification.EtypeVendorPaymentOrMisc;
            local.New1.FirstName =
              entities.AdministrativeActCertification.FirstName;

            // *********************************************************************************
            // ***  Sync-up our last name with what the fed believes is the real
            // last name.
            // ***  The name on the fed system can not be changed once the AP is
            // originally certified.
            // *********************************************************************************
            if (AsChar(local.LastNameDoesNotMatch.Flag) == 'Y')
            {
              local.New1.LastName =
                local.BatchFdsoReconciliationFileRecord.LastName;
            }
            else
            {
              local.New1.LastName =
                entities.AdministrativeActCertification.LastName;
            }

            local.New1.LastUpdatedBy = global.UserId;
            local.New1.LastUpdatedTstamp = Now();
            local.New1.LocalCode =
              entities.AdministrativeActCertification.LocalCode;
            local.New1.NonAdcAmount =
              entities.AdministrativeActCertification.NonAdcAmount;
            local.New1.OriginalAmount =
              entities.AdministrativeActCertification.OriginalAmount;
            local.New1.ProcessYear =
              entities.AdministrativeActCertification.ProcessYear;
            local.New1.Ssn = entities.AdministrativeActCertification.Ssn;
            local.New1.TakenDate = Now().Date;
            local.New1.TanfCode =
              entities.AdministrativeActCertification.TanfCode;
            local.New1.TransferState =
              entities.AdministrativeActCertification.TransferState;

            if (AsChar(local.LastNameDoesNotMatch.Flag) == 'Y' && AsChar
              (local.AmountDoesNotMatch.Flag) != 'Y' && AsChar
              (local.EtypeAdmDoesNotMatch.Flag) != 'Y' && AsChar
              (local.EtypeRetDoesNotMatch.Flag) != 'Y' && AsChar
              (local.EtypeVenDoesNotMatch.Flag) != 'Y' && AsChar
              (local.EtypeSalDoesNotMatch.Flag) != 'Y' && AsChar
              (local.EtypeTaxDoesNotMatch.Flag) != 'Y' && AsChar
              (local.EtypePasDoesNotMatch.Flag) != 'Y' && AsChar
              (local.EtypeFidmDoesNotMatch.Flag) != 'Y' && AsChar
              (local.CodeDoesNotMatch.Flag) != 'Y')
            {
              try
              {
                UpdateAdministrativeActCertification();
                ++local.NoLastNameUpdated.Count;
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    local.EabFileHandling.Action = "WRITE";
                    local.EabReportSend.RptDetail =
                      "Error Updating FDSO - Not Unique , cse_person : " + local
                      .BatchFdsoReconciliationFileRecord.CaseNumber;
                    UseCabErrorReport2();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                      return;
                    }

                    ExitState = "ACO_NE0000_FDSO_NOT_UNIQUE";

                    return;
                  case ErrorCode.PermittedValueViolation:
                    local.EabFileHandling.Action = "WRITE";
                    local.EabReportSend.RptDetail =
                      "Error Updating FDSO - Permitted Value Violation , cse_person : " +
                      local.BatchFdsoReconciliationFileRecord.CaseNumber;
                    UseCabErrorReport2();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                      return;
                    }

                    ExitState = "ACO_NE0000_FDSO_PVV";

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
              local.New1.TtypeMModifyAmount = "";
              local.New1.TtypeLChangeSubmittingState = "";
              local.New1.TtypeRModifyExclusion = "";

              if (AsChar(local.AmountDoesNotMatch.Flag) == 'Y')
              {
                local.New1.TtypeMModifyAmount = "M";
              }

              if (AsChar(local.CodeDoesNotMatch.Flag) == 'Y')
              {
                local.New1.TtypeLChangeSubmittingState = "L";
              }

              if (AsChar(local.EtypeAdmDoesNotMatch.Flag) == 'Y' || AsChar
                (local.EtypeRetDoesNotMatch.Flag) == 'Y' || AsChar
                (local.EtypeVenDoesNotMatch.Flag) == 'Y' || AsChar
                (local.EtypeSalDoesNotMatch.Flag) == 'Y' || AsChar
                (local.EtypeTaxDoesNotMatch.Flag) == 'Y' || AsChar
                (local.EtypePasDoesNotMatch.Flag) == 'Y' || AsChar
                (local.EtypeFidmDoesNotMatch.Flag) == 'Y')
              {
                local.New1.TtypeRModifyExclusion = "R";
              }

              local.New1.TtypeAAddNewCase = "";
              local.New1.TtypeDDeleteCertification = "";
              local.CsePerson.Number =
                local.BatchFdsoSnapshotFileRecord.CaseNumber;
              UseCreateFederalDebtSetoff();

              if (!IsExitState("ACO_NI0000_CREATE_OK"))
              {
                if (IsExitState("CSE_PERSON_ACCOUNT_NF"))
                {
                  local.EabFileHandling.Action = "WRITE";
                  local.EabReportSend.RptDetail =
                    "Err-'create modify'-'create federal debt setoff'-cse_person_account_nf:" +
                    local.BatchFdsoSnapshotFileRecord.CaseNumber;
                  UseCabErrorReport2();

                  if (!Equal(local.EabFileHandling.Status, "OK"))
                  {
                    ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                    return;
                  }

                  ExitState = "ACO_NN0000_ABEND_4_BATCH";

                  return;
                }

                if (IsExitState("FN0000_OBLIGOR_NF"))
                {
                  local.EabFileHandling.Action = "WRITE";
                  local.EabReportSend.RptDetail =
                    "Err-'create modify'- 'create federal debt setoff'-FN0000_OBLIGOR_NF:" +
                    local.BatchFdsoSnapshotFileRecord.CaseNumber;
                  UseCabErrorReport2();

                  if (!Equal(local.EabFileHandling.Status, "OK"))
                  {
                    ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                    return;
                  }

                  ExitState = "ACO_NN0000_ABEND_4_BATCH";

                  return;
                }

                if (IsExitState("FEDERAL_DEBT_SETOFF_AE"))
                {
                  local.EabFileHandling.Action = "WRITE";
                  local.EabReportSend.RptDetail =
                    "Err-'create modify'-'create federal debt setoff'-FEDERAL_DEBT_SETOFF_AE:" +
                    local.BatchFdsoSnapshotFileRecord.CaseNumber;
                  UseCabErrorReport2();

                  if (!Equal(local.EabFileHandling.Status, "OK"))
                  {
                    ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                    return;
                  }

                  ExitState = "ACO_NN0000_ABEND_4_BATCH";

                  return;
                }
              }
              else
              {
                local.AlreadyModifyPrev.CaseNumber =
                  local.BatchFdsoSnapshotFileRecord.CaseNumber;
                ++local.TotalNumberOfModifyRecord.Count;
              }
            }
          }
        }
        else
        {
          ++local.TotalNumberOfMatchRecords.Count;
        }
      }

Test8:
      ;
    }
    while(!Equal(local.PassArea.TextReturnCode, "EF"));

    local.EabReportSend.RptDetail =
      "Total number of Snapshot records read : " + NumberToString
      (local.TotalNumberOfSnapshotRecord.Count, 15);
    local.EabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while writting control report(Total number of Snapshot records read).";
        
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "Total number of Recon records read : " + NumberToString
      (local.TotalNumberOfReconRecord.Count, 15);
    local.EabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while writting control report(Total number of Recon records read).";
        
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "Total number of ADD records created : " + NumberToString
      (local.TotalNumberOfAddRecord.Count, 15);
    local.EabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while writting control report(Total number of Add records created).";
        
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail =
      "Total number of DELETE records created : " + NumberToString
      (local.TotalNumberOfDeleteRecord.Count, 15);
    local.EabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while writting control report(Total number of DELETE records created).";
        
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail =
      "Total number of M / L / R  records created : " + NumberToString
      (local.TotalNumberOfModifyRecord.Count, 15);
    local.EabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while writting control report(Total number of M / L / R records created).";
        
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "Total number of match records : " + NumberToString
      (local.TotalNumberOfMatchRecords.Count, 15);
    local.EabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while writting control report(Total number of match records).";
        
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail =
      "Total number of only Last Name Updated : " + NumberToString
      (local.NoLastNameUpdated.Count, 15);
    local.EabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while writting control report(Total number of only last name updated).";
        
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "Total number of 'Delete' Resend : " + NumberToString
      (local.TotalNumberOfDeleteResend.Count, 15);
    local.EabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while writting control report(Total number of 'Delete' Resend).";
        
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    if (AsChar(local.FileOpened.Flag) == 'Y')
    {
      // -------------------------------------------------------
      // Close External FDSO Reconciliation File
      // -------------------------------------------------------
      local.PassArea.FileInstruction = "CLOSE1";
      UseLeEabProcessFdsoReconFile3();

      if (!IsEmpty(local.PassArea.TextReturnCode))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error encountered while closing Reconciliation file.";
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        ExitState = "FILE_OPEN_ERROR";

        return;
      }

      // -------------------------------------------
      // Close External FDSO Snapshot File
      // -------------------------------------------
      local.PassArea.FileInstruction = "CLOSE2";
      UseLeEabProcessFdsoReconFile3();

      if (!IsEmpty(local.PassArea.TextReturnCode))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error encountered while closing Snapshot file'.";
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        ExitState = "FILE_OPEN_ERROR";

        return;
      }
    }

    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while closing control report.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private static void MoveAdministrativeActCertification(
    AdministrativeActCertification source,
    AdministrativeActCertification target)
  {
    target.TanfCode = source.TanfCode;
    target.TakenDate = source.TakenDate;
    target.OriginalAmount = source.OriginalAmount;
    target.CurrentAmount = source.CurrentAmount;
    target.CurrentAmountDate = source.CurrentAmountDate;
    target.DecertifiedDate = source.DecertifiedDate;
    target.NotificationDate = source.NotificationDate;
    target.NotifiedBy = source.NotifiedBy;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTstamp = source.CreatedTstamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.DecertificationReason = source.DecertificationReason;
    target.AdcAmount = source.AdcAmount;
    target.NonAdcAmount = source.NonAdcAmount;
    target.InjuredSpouseDate = source.InjuredSpouseDate;
    target.LocalCode = source.LocalCode;
    target.Ssn = source.Ssn;
    target.CaseNumber = source.CaseNumber;
    target.LastName = source.LastName;
    target.FirstName = source.FirstName;
    target.AmountOwed = source.AmountOwed;
    target.CaseType = source.CaseType;
    target.TransferState = source.TransferState;
    target.LocalForTransfer = source.LocalForTransfer;
    target.ProcessYear = source.ProcessYear;
    target.TtypeAAddNewCase = source.TtypeAAddNewCase;
    target.TtypeDDeleteCertification = source.TtypeDDeleteCertification;
    target.TtypeLChangeSubmittingState = source.TtypeLChangeSubmittingState;
    target.TtypeMModifyAmount = source.TtypeMModifyAmount;
    target.TtypeRModifyExclusion = source.TtypeRModifyExclusion;
    target.TtypeSStatePayment = source.TtypeSStatePayment;
    target.TtypeTTransferAdminReview = source.TtypeTTransferAdminReview;
    target.EtypeAdministrativeOffset = source.EtypeAdministrativeOffset;
    target.EtypeFederalRetirement = source.EtypeFederalRetirement;
    target.EtypeFederalSalary = source.EtypeFederalSalary;
    target.EtypeTaxRefund = source.EtypeTaxRefund;
    target.EtypeVendorPaymentOrMisc = source.EtypeVendorPaymentOrMisc;
    target.EtypePassportDenial = source.EtypePassportDenial;
    target.EtypeFinancialInstitution = source.EtypeFinancialInstitution;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveExternal(External source, External target)
  {
    target.FileInstruction = source.FileInstruction;
    target.NumericReturnCode = source.NumericReturnCode;
    target.TextReturnCode = source.TextReturnCode;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
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

  private void UseCreateFederalDebtSetoff()
  {
    var useImport = new CreateFederalDebtSetoff.Import();
    var useExport = new CreateFederalDebtSetoff.Export();

    useImport.CsePerson.Number = local.CsePerson.Number;
    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;
    MoveAdministrativeActCertification(local.New1, useImport.Export1);

    Call(CreateFederalDebtSetoff.Execute, useImport, useExport);

    MoveAdministrativeActCertification(useImport.Export1, local.New1);
  }

  private void UseLeEabProcessFdsoReconFile1()
  {
    var useImport = new LeEabProcessFdsoReconFile.Import();
    var useExport = new LeEabProcessFdsoReconFile.Export();

    useImport.External.FileInstruction = local.PassArea.FileInstruction;
    useExport.FdsoReconciliationFileRecord.Assign(
      local.BatchFdsoReconciliationFileRecord);
    useExport.External.Assign(local.PassArea);

    Call(LeEabProcessFdsoReconFile.Execute, useImport, useExport);

    local.BatchFdsoReconciliationFileRecord.Assign(
      useExport.FdsoReconciliationFileRecord);
    MoveExternal(useExport.External, local.PassArea);
  }

  private void UseLeEabProcessFdsoReconFile2()
  {
    var useImport = new LeEabProcessFdsoReconFile.Import();
    var useExport = new LeEabProcessFdsoReconFile.Export();

    useImport.External.FileInstruction = local.PassArea.FileInstruction;
    useExport.FdsoSnapshotFileRecord.Assign(local.BatchFdsoSnapshotFileRecord);
    useExport.External.Assign(local.PassArea);

    Call(LeEabProcessFdsoReconFile.Execute, useImport, useExport);

    local.BatchFdsoSnapshotFileRecord.Assign(useExport.FdsoSnapshotFileRecord);
    MoveExternal(useExport.External, local.PassArea);
  }

  private void UseLeEabProcessFdsoReconFile3()
  {
    var useImport = new LeEabProcessFdsoReconFile.Import();
    var useExport = new LeEabProcessFdsoReconFile.Export();

    useImport.External.FileInstruction = local.PassArea.FileInstruction;
    useExport.External.Assign(local.PassArea);

    Call(LeEabProcessFdsoReconFile.Execute, useImport, useExport);

    MoveExternal(useExport.External, local.PassArea);
  }

  private void UseReadPgmChkpntRestart()
  {
    var useImport = new ReadPgmChkpntRestart.Import();
    var useExport = new ReadPgmChkpntRestart.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      local.ProgramCheckpointRestart.ProgramName;

    Call(ReadPgmChkpntRestart.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void CreateFederalDebtSetoff1()
  {
    var cpaType =
      GetImplicitValue<AdministrativeActCertification, string>("CpaType");
    var type1 = "FDSO";
    var takenDate = local.New1.TakenDate;
    var originalAmount = 0M;
    var createdBy = local.New1.CreatedBy;
    var createdTstamp = local.New1.CreatedTstamp;
    var lastUpdatedBy = local.New1.LastUpdatedBy ?? "";
    var lastUpdatedTstamp = local.New1.LastUpdatedTstamp;
    var adcAmount = local.New1.AdcAmount.GetValueOrDefault();
    var nonAdcAmount = local.New1.NonAdcAmount.GetValueOrDefault();
    var dateSent = local.New1.DateSent;
    var localCode = local.New1.LocalCode ?? "";
    var ssn = local.New1.Ssn;
    var caseNumber = local.New1.CaseNumber;
    var lastName = local.New1.LastName;
    var firstName = local.New1.FirstName;
    var ttypeAAddNewCase = local.New1.TtypeAAddNewCase ?? "";
    var caseType = local.New1.CaseType;
    var processYear = local.New1.ProcessYear.GetValueOrDefault();
    var tanfCode = local.New1.TanfCode;
    var ttypeDDeleteCertification = local.New1.TtypeDDeleteCertification ?? "";
    var ttypeLChangeSubmittingState =
      local.New1.TtypeLChangeSubmittingState ?? "";
    var ttypeMModifyAmount = local.New1.TtypeMModifyAmount ?? "";
    var ttypeRModifyExclusion = local.New1.TtypeRModifyExclusion ?? "";

    CheckValid<AdministrativeActCertification>("CpaType", cpaType);
    CheckValid<AdministrativeActCertification>("Type1", type1);
    entities.New1.Populated = false;
    Update("CreateFederalDebtSetoff",
      (db, command) =>
      {
        db.SetString(command, "cpaType", cpaType);
        db.SetString(command, "cspNumber", "");
        db.SetString(command, "type", type1);
        db.SetDate(command, "takenDt", takenDate);
        db.SetNullableDecimal(command, "originalAmt", originalAmount);
        db.SetNullableDecimal(command, "currentAmt", originalAmount);
        db.SetNullableDate(command, "currentAmtDt", null);
        db.SetNullableDate(command, "decertifiedDt", null);
        db.SetNullableDate(command, "notificationDt", null);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetNullableDate(command, "referralDt", default(DateTime));
        db.SetNullableDecimal(command, "recoveryAmt", originalAmount);
        db.SetNullableDecimal(command, "adcAmt", adcAmount);
        db.SetNullableDecimal(command, "nonAdcAmt", nonAdcAmount);
        db.SetNullableDate(command, "injuredSpouseDt", null);
        db.SetString(command, "witness", "");
        db.SetNullableString(command, "notifiedBy", "");
        db.SetNullableString(command, "reasonWithdraw", "");
        db.SetNullableString(command, "denialReason", "");
        db.SetNullableDate(command, "dateSent", dateSent);
        db.SetNullableString(command, "etypeAdminOffset", "");
        db.SetNullableString(command, "localCode", localCode);
        db.SetInt32(command, "ssn", ssn);
        db.SetString(command, "caseNumber", caseNumber);
        db.SetString(command, "lastName", lastName);
        db.SetString(command, "firstName", firstName);
        db.SetInt32(command, "amountOwed", 0);
        db.SetNullableString(command, "ttypeAddNewCase", ttypeAAddNewCase);
        db.SetString(command, "caseType", caseType);
        db.SetNullableString(command, "transferState", "");
        db.SetNullableInt32(command, "localForTransfer", 0);
        db.SetNullableInt32(command, "processYear", processYear);
        db.SetString(command, "tanfCode", tanfCode);
        db.SetNullableString(
          command, "ttypeDeleteCert", ttypeDDeleteCertification);
        db.SetNullableString(
          command, "ttypeChngSubSt", ttypeLChangeSubmittingState);
        db.SetNullableString(command, "ttypeModifyAmnt", ttypeMModifyAmount);
        db.SetNullableString(command, "ttypeModifyExcl", ttypeRModifyExclusion);
        db.SetNullableString(command, "ttypeStatePymnt", "");
        db.SetNullableString(command, "ttypeXferAdmRvw", "");
        db.SetNullableString(command, "ttypeNameChange", "");
        db.SetNullableString(command, "etypeFedRetrmnt", "");
        db.SetNullableString(command, "etypeFedSalary", "");
        db.SetNullableString(command, "etypeTaxRefund", "");
        db.SetNullableString(command, "etypeVndrPymntM", "");
        db.SetNullableString(command, "etypePsprtDenial", "");
        db.SetNullableString(command, "etypeFinInst", "");
        db.SetNullableString(command, "returnStatus", "");
        db.SetNullableDate(command, "returnStatusDate", null);
        db.SetNullableString(command, "decertifyReason", "");
        db.SetNullableString(command, "addressStreet1", "");
        db.SetNullableString(command, "addressCity", "");
        db.SetNullableString(command, "addressState", "");
        db.SetNullableString(command, "addressZip", "");
        db.SetNullableInt32(command, "numCourtOrders", 0);
      });

    entities.New1.CpaType = cpaType;
    entities.New1.CspNumber = "";
    entities.New1.Type1 = type1;
    entities.New1.TakenDate = takenDate;
    entities.New1.OriginalAmount = originalAmount;
    entities.New1.CurrentAmount = originalAmount;
    entities.New1.CurrentAmountDate = null;
    entities.New1.DecertifiedDate = null;
    entities.New1.NotificationDate = null;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CreatedTstamp = createdTstamp;
    entities.New1.LastUpdatedBy = lastUpdatedBy;
    entities.New1.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.New1.AdcAmount = adcAmount;
    entities.New1.NonAdcAmount = nonAdcAmount;
    entities.New1.InjuredSpouseDate = null;
    entities.New1.NotifiedBy = "";
    entities.New1.DateSent = dateSent;
    entities.New1.EtypeAdministrativeOffset = "";
    entities.New1.LocalCode = localCode;
    entities.New1.Ssn = ssn;
    entities.New1.CaseNumber = caseNumber;
    entities.New1.LastName = lastName;
    entities.New1.FirstName = firstName;
    entities.New1.AmountOwed = 0;
    entities.New1.TtypeAAddNewCase = ttypeAAddNewCase;
    entities.New1.CaseType = caseType;
    entities.New1.TransferState = "";
    entities.New1.LocalForTransfer = 0;
    entities.New1.ProcessYear = processYear;
    entities.New1.TanfCode = tanfCode;
    entities.New1.TtypeDDeleteCertification = ttypeDDeleteCertification;
    entities.New1.TtypeLChangeSubmittingState = ttypeLChangeSubmittingState;
    entities.New1.TtypeMModifyAmount = ttypeMModifyAmount;
    entities.New1.TtypeRModifyExclusion = ttypeRModifyExclusion;
    entities.New1.TtypeSStatePayment = "";
    entities.New1.TtypeTTransferAdminReview = "";
    entities.New1.EtypeFederalRetirement = "";
    entities.New1.EtypeFederalSalary = "";
    entities.New1.EtypeTaxRefund = "";
    entities.New1.EtypeVendorPaymentOrMisc = "";
    entities.New1.EtypePassportDenial = "";
    entities.New1.EtypeFinancialInstitution = "";
    entities.New1.ReturnStatus = "";
    entities.New1.ReturnStatusDate = null;
    entities.New1.DecertificationReason = "";
    entities.New1.Populated = true;
  }

  private bool ReadAdministrativeActCertification1()
  {
    entities.AdministrativeActCertification.Populated = false;

    return Read("ReadAdministrativeActCertification1",
      (db, command) =>
      {
        db.SetString(
          command, "caseNumber", local.BatchFdsoSnapshotFileRecord.CaseNumber);
      },
      (db, reader) =>
      {
        entities.AdministrativeActCertification.CpaType =
          db.GetString(reader, 0);
        entities.AdministrativeActCertification.CspNumber =
          db.GetString(reader, 1);
        entities.AdministrativeActCertification.Type1 = db.GetString(reader, 2);
        entities.AdministrativeActCertification.TakenDate =
          db.GetDate(reader, 3);
        entities.AdministrativeActCertification.OriginalAmount =
          db.GetNullableDecimal(reader, 4);
        entities.AdministrativeActCertification.CurrentAmount =
          db.GetNullableDecimal(reader, 5);
        entities.AdministrativeActCertification.CurrentAmountDate =
          db.GetNullableDate(reader, 6);
        entities.AdministrativeActCertification.DecertifiedDate =
          db.GetNullableDate(reader, 7);
        entities.AdministrativeActCertification.NotificationDate =
          db.GetNullableDate(reader, 8);
        entities.AdministrativeActCertification.CreatedBy =
          db.GetString(reader, 9);
        entities.AdministrativeActCertification.CreatedTstamp =
          db.GetDateTime(reader, 10);
        entities.AdministrativeActCertification.LastUpdatedBy =
          db.GetNullableString(reader, 11);
        entities.AdministrativeActCertification.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 12);
        entities.AdministrativeActCertification.AdcAmount =
          db.GetNullableDecimal(reader, 13);
        entities.AdministrativeActCertification.NonAdcAmount =
          db.GetNullableDecimal(reader, 14);
        entities.AdministrativeActCertification.InjuredSpouseDate =
          db.GetNullableDate(reader, 15);
        entities.AdministrativeActCertification.NotifiedBy =
          db.GetNullableString(reader, 16);
        entities.AdministrativeActCertification.DateSent =
          db.GetNullableDate(reader, 17);
        entities.AdministrativeActCertification.EtypeAdministrativeOffset =
          db.GetNullableString(reader, 18);
        entities.AdministrativeActCertification.LocalCode =
          db.GetNullableString(reader, 19);
        entities.AdministrativeActCertification.Ssn = db.GetInt32(reader, 20);
        entities.AdministrativeActCertification.CaseNumber =
          db.GetString(reader, 21);
        entities.AdministrativeActCertification.LastName =
          db.GetString(reader, 22);
        entities.AdministrativeActCertification.FirstName =
          db.GetString(reader, 23);
        entities.AdministrativeActCertification.AmountOwed =
          db.GetInt32(reader, 24);
        entities.AdministrativeActCertification.TtypeAAddNewCase =
          db.GetNullableString(reader, 25);
        entities.AdministrativeActCertification.CaseType =
          db.GetString(reader, 26);
        entities.AdministrativeActCertification.TransferState =
          db.GetNullableString(reader, 27);
        entities.AdministrativeActCertification.LocalForTransfer =
          db.GetNullableInt32(reader, 28);
        entities.AdministrativeActCertification.ProcessYear =
          db.GetNullableInt32(reader, 29);
        entities.AdministrativeActCertification.TanfCode =
          db.GetString(reader, 30);
        entities.AdministrativeActCertification.TtypeDDeleteCertification =
          db.GetNullableString(reader, 31);
        entities.AdministrativeActCertification.TtypeLChangeSubmittingState =
          db.GetNullableString(reader, 32);
        entities.AdministrativeActCertification.TtypeMModifyAmount =
          db.GetNullableString(reader, 33);
        entities.AdministrativeActCertification.TtypeRModifyExclusion =
          db.GetNullableString(reader, 34);
        entities.AdministrativeActCertification.TtypeSStatePayment =
          db.GetNullableString(reader, 35);
        entities.AdministrativeActCertification.TtypeTTransferAdminReview =
          db.GetNullableString(reader, 36);
        entities.AdministrativeActCertification.EtypeFederalRetirement =
          db.GetNullableString(reader, 37);
        entities.AdministrativeActCertification.EtypeFederalSalary =
          db.GetNullableString(reader, 38);
        entities.AdministrativeActCertification.EtypeTaxRefund =
          db.GetNullableString(reader, 39);
        entities.AdministrativeActCertification.EtypeVendorPaymentOrMisc =
          db.GetNullableString(reader, 40);
        entities.AdministrativeActCertification.EtypePassportDenial =
          db.GetNullableString(reader, 41);
        entities.AdministrativeActCertification.EtypeFinancialInstitution =
          db.GetNullableString(reader, 42);
        entities.AdministrativeActCertification.ReturnStatus =
          db.GetNullableString(reader, 43);
        entities.AdministrativeActCertification.ReturnStatusDate =
          db.GetNullableDate(reader, 44);
        entities.AdministrativeActCertification.DecertificationReason =
          db.GetNullableString(reader, 45);
        entities.AdministrativeActCertification.Populated = true;
      });
  }

  private bool ReadAdministrativeActCertification2()
  {
    entities.AdministrativeActCertification.Populated = false;

    return Read("ReadAdministrativeActCertification2",
      (db, command) =>
      {
        db.SetString(
          command, "caseNumber",
          local.BatchFdsoReconciliationFileRecord.CaseNumber);
      },
      (db, reader) =>
      {
        entities.AdministrativeActCertification.CpaType =
          db.GetString(reader, 0);
        entities.AdministrativeActCertification.CspNumber =
          db.GetString(reader, 1);
        entities.AdministrativeActCertification.Type1 = db.GetString(reader, 2);
        entities.AdministrativeActCertification.TakenDate =
          db.GetDate(reader, 3);
        entities.AdministrativeActCertification.OriginalAmount =
          db.GetNullableDecimal(reader, 4);
        entities.AdministrativeActCertification.CurrentAmount =
          db.GetNullableDecimal(reader, 5);
        entities.AdministrativeActCertification.CurrentAmountDate =
          db.GetNullableDate(reader, 6);
        entities.AdministrativeActCertification.DecertifiedDate =
          db.GetNullableDate(reader, 7);
        entities.AdministrativeActCertification.NotificationDate =
          db.GetNullableDate(reader, 8);
        entities.AdministrativeActCertification.CreatedBy =
          db.GetString(reader, 9);
        entities.AdministrativeActCertification.CreatedTstamp =
          db.GetDateTime(reader, 10);
        entities.AdministrativeActCertification.LastUpdatedBy =
          db.GetNullableString(reader, 11);
        entities.AdministrativeActCertification.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 12);
        entities.AdministrativeActCertification.AdcAmount =
          db.GetNullableDecimal(reader, 13);
        entities.AdministrativeActCertification.NonAdcAmount =
          db.GetNullableDecimal(reader, 14);
        entities.AdministrativeActCertification.InjuredSpouseDate =
          db.GetNullableDate(reader, 15);
        entities.AdministrativeActCertification.NotifiedBy =
          db.GetNullableString(reader, 16);
        entities.AdministrativeActCertification.DateSent =
          db.GetNullableDate(reader, 17);
        entities.AdministrativeActCertification.EtypeAdministrativeOffset =
          db.GetNullableString(reader, 18);
        entities.AdministrativeActCertification.LocalCode =
          db.GetNullableString(reader, 19);
        entities.AdministrativeActCertification.Ssn = db.GetInt32(reader, 20);
        entities.AdministrativeActCertification.CaseNumber =
          db.GetString(reader, 21);
        entities.AdministrativeActCertification.LastName =
          db.GetString(reader, 22);
        entities.AdministrativeActCertification.FirstName =
          db.GetString(reader, 23);
        entities.AdministrativeActCertification.AmountOwed =
          db.GetInt32(reader, 24);
        entities.AdministrativeActCertification.TtypeAAddNewCase =
          db.GetNullableString(reader, 25);
        entities.AdministrativeActCertification.CaseType =
          db.GetString(reader, 26);
        entities.AdministrativeActCertification.TransferState =
          db.GetNullableString(reader, 27);
        entities.AdministrativeActCertification.LocalForTransfer =
          db.GetNullableInt32(reader, 28);
        entities.AdministrativeActCertification.ProcessYear =
          db.GetNullableInt32(reader, 29);
        entities.AdministrativeActCertification.TanfCode =
          db.GetString(reader, 30);
        entities.AdministrativeActCertification.TtypeDDeleteCertification =
          db.GetNullableString(reader, 31);
        entities.AdministrativeActCertification.TtypeLChangeSubmittingState =
          db.GetNullableString(reader, 32);
        entities.AdministrativeActCertification.TtypeMModifyAmount =
          db.GetNullableString(reader, 33);
        entities.AdministrativeActCertification.TtypeRModifyExclusion =
          db.GetNullableString(reader, 34);
        entities.AdministrativeActCertification.TtypeSStatePayment =
          db.GetNullableString(reader, 35);
        entities.AdministrativeActCertification.TtypeTTransferAdminReview =
          db.GetNullableString(reader, 36);
        entities.AdministrativeActCertification.EtypeFederalRetirement =
          db.GetNullableString(reader, 37);
        entities.AdministrativeActCertification.EtypeFederalSalary =
          db.GetNullableString(reader, 38);
        entities.AdministrativeActCertification.EtypeTaxRefund =
          db.GetNullableString(reader, 39);
        entities.AdministrativeActCertification.EtypeVendorPaymentOrMisc =
          db.GetNullableString(reader, 40);
        entities.AdministrativeActCertification.EtypePassportDenial =
          db.GetNullableString(reader, 41);
        entities.AdministrativeActCertification.EtypeFinancialInstitution =
          db.GetNullableString(reader, 42);
        entities.AdministrativeActCertification.ReturnStatus =
          db.GetNullableString(reader, 43);
        entities.AdministrativeActCertification.ReturnStatusDate =
          db.GetNullableDate(reader, 44);
        entities.AdministrativeActCertification.DecertificationReason =
          db.GetNullableString(reader, 45);
        entities.AdministrativeActCertification.Populated = true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.Existing.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", local.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Existing.Number = db.GetString(reader, 0);
        entities.Existing.Populated = true;
      });
  }

  private void UpdateAdministrativeActCertification()
  {
    System.Diagnostics.Debug.Assert(
      entities.AdministrativeActCertification.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatedTstamp = Now();
    var lastName = local.New1.LastName;

    entities.AdministrativeActCertification.Populated = false;
    Update("UpdateAdministrativeActCertification",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetString(command, "lastName", lastName);
        db.SetString(
          command, "cpaType", entities.AdministrativeActCertification.CpaType);
        db.SetString(
          command, "cspNumber",
          entities.AdministrativeActCertification.CspNumber);
        db.SetString(
          command, "type", entities.AdministrativeActCertification.Type1);
        db.SetDate(
          command, "takenDt",
          entities.AdministrativeActCertification.TakenDate.
            GetValueOrDefault());
        db.SetString(
          command, "tanfCode",
          entities.AdministrativeActCertification.TanfCode);
      });

    entities.AdministrativeActCertification.LastUpdatedBy = lastUpdatedBy;
    entities.AdministrativeActCertification.LastUpdatedTstamp =
      lastUpdatedTstamp;
    entities.AdministrativeActCertification.LastName = lastName;
    entities.AdministrativeActCertification.Populated = true;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
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
    /// A value of TotalNumberOfDeleteResend.
    /// </summary>
    [JsonPropertyName("totalNumberOfDeleteResend")]
    public Common TotalNumberOfDeleteResend
    {
      get => totalNumberOfDeleteResend ??= new();
      set => totalNumberOfDeleteResend = value;
    }

    /// <summary>
    /// A value of AlreadyModifyPrev.
    /// </summary>
    [JsonPropertyName("alreadyModifyPrev")]
    public FdsoSnapshotFileRecord AlreadyModifyPrev
    {
      get => alreadyModifyPrev ??= new();
      set => alreadyModifyPrev = value;
    }

    /// <summary>
    /// A value of DoNotReadReconRecord.
    /// </summary>
    [JsonPropertyName("doNotReadReconRecord")]
    public Common DoNotReadReconRecord
    {
      get => doNotReadReconRecord ??= new();
      set => doNotReadReconRecord = value;
    }

    /// <summary>
    /// A value of PreviousFdsoReconciliationFileRecord.
    /// </summary>
    [JsonPropertyName("previousFdsoReconciliationFileRecord")]
    public FdsoReconciliationFileRecord PreviousFdsoReconciliationFileRecord
    {
      get => previousFdsoReconciliationFileRecord ??= new();
      set => previousFdsoReconciliationFileRecord = value;
    }

    /// <summary>
    /// A value of PreviousFdsoSnapshotFileRecord.
    /// </summary>
    [JsonPropertyName("previousFdsoSnapshotFileRecord")]
    public FdsoSnapshotFileRecord PreviousFdsoSnapshotFileRecord
    {
      get => previousFdsoSnapshotFileRecord ??= new();
      set => previousFdsoSnapshotFileRecord = value;
    }

    /// <summary>
    /// A value of NoLastNameUpdated.
    /// </summary>
    [JsonPropertyName("noLastNameUpdated")]
    public Common NoLastNameUpdated
    {
      get => noLastNameUpdated ??= new();
      set => noLastNameUpdated = value;
    }

    /// <summary>
    /// A value of TotalNumberOfModifyRecord.
    /// </summary>
    [JsonPropertyName("totalNumberOfModifyRecord")]
    public Common TotalNumberOfModifyRecord
    {
      get => totalNumberOfModifyRecord ??= new();
      set => totalNumberOfModifyRecord = value;
    }

    /// <summary>
    /// A value of TotalNumberOfAddRecord.
    /// </summary>
    [JsonPropertyName("totalNumberOfAddRecord")]
    public Common TotalNumberOfAddRecord
    {
      get => totalNumberOfAddRecord ??= new();
      set => totalNumberOfAddRecord = value;
    }

    /// <summary>
    /// A value of TotalNumberOfDeleteRecord.
    /// </summary>
    [JsonPropertyName("totalNumberOfDeleteRecord")]
    public Common TotalNumberOfDeleteRecord
    {
      get => totalNumberOfDeleteRecord ??= new();
      set => totalNumberOfDeleteRecord = value;
    }

    /// <summary>
    /// A value of TotalNumberOfSnapshotRecord.
    /// </summary>
    [JsonPropertyName("totalNumberOfSnapshotRecord")]
    public Common TotalNumberOfSnapshotRecord
    {
      get => totalNumberOfSnapshotRecord ??= new();
      set => totalNumberOfSnapshotRecord = value;
    }

    /// <summary>
    /// A value of TotalNumberOfReconRecord.
    /// </summary>
    [JsonPropertyName("totalNumberOfReconRecord")]
    public Common TotalNumberOfReconRecord
    {
      get => totalNumberOfReconRecord ??= new();
      set => totalNumberOfReconRecord = value;
    }

    /// <summary>
    /// A value of FdsoCertificationTotalRecord.
    /// </summary>
    [JsonPropertyName("fdsoCertificationTotalRecord")]
    public FdsoCertificationTotalRecord FdsoCertificationTotalRecord
    {
      get => fdsoCertificationTotalRecord ??= new();
      set => fdsoCertificationTotalRecord = value;
    }

    /// <summary>
    /// A value of LastNameDoesNotMatch.
    /// </summary>
    [JsonPropertyName("lastNameDoesNotMatch")]
    public Common LastNameDoesNotMatch
    {
      get => lastNameDoesNotMatch ??= new();
      set => lastNameDoesNotMatch = value;
    }

    /// <summary>
    /// A value of FirstNameDoesNotMatch.
    /// </summary>
    [JsonPropertyName("firstNameDoesNotMatch")]
    public Common FirstNameDoesNotMatch
    {
      get => firstNameDoesNotMatch ??= new();
      set => firstNameDoesNotMatch = value;
    }

    /// <summary>
    /// A value of ReconPersonExistOnFdso.
    /// </summary>
    [JsonPropertyName("reconPersonExistOnFdso")]
    public Common ReconPersonExistOnFdso
    {
      get => reconPersonExistOnFdso ??= new();
      set => reconPersonExistOnFdso = value;
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
    /// A value of AdministrativeActCertification.
    /// </summary>
    [JsonPropertyName("administrativeActCertification")]
    public AdministrativeActCertification AdministrativeActCertification
    {
      get => administrativeActCertification ??= new();
      set => administrativeActCertification = value;
    }

    /// <summary>
    /// A value of ReadNextReconRecord.
    /// </summary>
    [JsonPropertyName("readNextReconRecord")]
    public Common ReadNextReconRecord
    {
      get => readNextReconRecord ??= new();
      set => readNextReconRecord = value;
    }

    /// <summary>
    /// A value of TotalNumberOfMatchRecords.
    /// </summary>
    [JsonPropertyName("totalNumberOfMatchRecords")]
    public Common TotalNumberOfMatchRecords
    {
      get => totalNumberOfMatchRecords ??= new();
      set => totalNumberOfMatchRecords = value;
    }

    /// <summary>
    /// A value of CodeDoesNotMatch.
    /// </summary>
    [JsonPropertyName("codeDoesNotMatch")]
    public Common CodeDoesNotMatch
    {
      get => codeDoesNotMatch ??= new();
      set => codeDoesNotMatch = value;
    }

    /// <summary>
    /// A value of SsnDoesNotMatch.
    /// </summary>
    [JsonPropertyName("ssnDoesNotMatch")]
    public Common SsnDoesNotMatch
    {
      get => ssnDoesNotMatch ??= new();
      set => ssnDoesNotMatch = value;
    }

    /// <summary>
    /// A value of EtypeFidmDoesNotMatch.
    /// </summary>
    [JsonPropertyName("etypeFidmDoesNotMatch")]
    public Common EtypeFidmDoesNotMatch
    {
      get => etypeFidmDoesNotMatch ??= new();
      set => etypeFidmDoesNotMatch = value;
    }

    /// <summary>
    /// A value of EtypePasDoesNotMatch.
    /// </summary>
    [JsonPropertyName("etypePasDoesNotMatch")]
    public Common EtypePasDoesNotMatch
    {
      get => etypePasDoesNotMatch ??= new();
      set => etypePasDoesNotMatch = value;
    }

    /// <summary>
    /// A value of EtypeTaxDoesNotMatch.
    /// </summary>
    [JsonPropertyName("etypeTaxDoesNotMatch")]
    public Common EtypeTaxDoesNotMatch
    {
      get => etypeTaxDoesNotMatch ??= new();
      set => etypeTaxDoesNotMatch = value;
    }

    /// <summary>
    /// A value of EtypeSalDoesNotMatch.
    /// </summary>
    [JsonPropertyName("etypeSalDoesNotMatch")]
    public Common EtypeSalDoesNotMatch
    {
      get => etypeSalDoesNotMatch ??= new();
      set => etypeSalDoesNotMatch = value;
    }

    /// <summary>
    /// A value of EtypeVenDoesNotMatch.
    /// </summary>
    [JsonPropertyName("etypeVenDoesNotMatch")]
    public Common EtypeVenDoesNotMatch
    {
      get => etypeVenDoesNotMatch ??= new();
      set => etypeVenDoesNotMatch = value;
    }

    /// <summary>
    /// A value of EtypeRetDoesNotMatch.
    /// </summary>
    [JsonPropertyName("etypeRetDoesNotMatch")]
    public Common EtypeRetDoesNotMatch
    {
      get => etypeRetDoesNotMatch ??= new();
      set => etypeRetDoesNotMatch = value;
    }

    /// <summary>
    /// A value of EtypeAdmDoesNotMatch.
    /// </summary>
    [JsonPropertyName("etypeAdmDoesNotMatch")]
    public Common EtypeAdmDoesNotMatch
    {
      get => etypeAdmDoesNotMatch ??= new();
      set => etypeAdmDoesNotMatch = value;
    }

    /// <summary>
    /// A value of AmountDoesNotMatch.
    /// </summary>
    [JsonPropertyName("amountDoesNotMatch")]
    public Common AmountDoesNotMatch
    {
      get => amountDoesNotMatch ??= new();
      set => amountDoesNotMatch = value;
    }

    /// <summary>
    /// A value of TotalReconMoreThanSnap.
    /// </summary>
    [JsonPropertyName("totalReconMoreThanSnap")]
    public Common TotalReconMoreThanSnap
    {
      get => totalReconMoreThanSnap ??= new();
      set => totalReconMoreThanSnap = value;
    }

    /// <summary>
    /// A value of TotalSnapshotMoreThanRecon.
    /// </summary>
    [JsonPropertyName("totalSnapshotMoreThanRecon")]
    public Common TotalSnapshotMoreThanRecon
    {
      get => totalSnapshotMoreThanRecon ??= new();
      set => totalSnapshotMoreThanRecon = value;
    }

    /// <summary>
    /// A value of TotalNumberOfMismatch.
    /// </summary>
    [JsonPropertyName("totalNumberOfMismatch")]
    public Common TotalNumberOfMismatch
    {
      get => totalNumberOfMismatch ??= new();
      set => totalNumberOfMismatch = value;
    }

    /// <summary>
    /// A value of EndOfSnapshotFile.
    /// </summary>
    [JsonPropertyName("endOfSnapshotFile")]
    public Common EndOfSnapshotFile
    {
      get => endOfSnapshotFile ??= new();
      set => endOfSnapshotFile = value;
    }

    /// <summary>
    /// A value of EndOfReconFile.
    /// </summary>
    [JsonPropertyName("endOfReconFile")]
    public Common EndOfReconFile
    {
      get => endOfReconFile ??= new();
      set => endOfReconFile = value;
    }

    /// <summary>
    /// A value of BatchFdsoReconciliationFileRecord.
    /// </summary>
    [JsonPropertyName("batchFdsoReconciliationFileRecord")]
    public FdsoReconciliationFileRecord BatchFdsoReconciliationFileRecord
    {
      get => batchFdsoReconciliationFileRecord ??= new();
      set => batchFdsoReconciliationFileRecord = value;
    }

    /// <summary>
    /// A value of BatchFdsoSnapshotFileRecord.
    /// </summary>
    [JsonPropertyName("batchFdsoSnapshotFileRecord")]
    public FdsoSnapshotFileRecord BatchFdsoSnapshotFileRecord
    {
      get => batchFdsoSnapshotFileRecord ??= new();
      set => batchFdsoSnapshotFileRecord = value;
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
    /// A value of PassArea.
    /// </summary>
    [JsonPropertyName("passArea")]
    public External PassArea
    {
      get => passArea ??= new();
      set => passArea = value;
    }

    /// <summary>
    /// A value of FileOpened.
    /// </summary>
    [JsonPropertyName("fileOpened")]
    public Common FileOpened
    {
      get => fileOpened ??= new();
      set => fileOpened = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public AdministrativeActCertification Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public AdministrativeActCertification New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    private Common totalNumberOfDeleteResend;
    private FdsoSnapshotFileRecord alreadyModifyPrev;
    private Common doNotReadReconRecord;
    private FdsoReconciliationFileRecord previousFdsoReconciliationFileRecord;
    private FdsoSnapshotFileRecord previousFdsoSnapshotFileRecord;
    private Common noLastNameUpdated;
    private Common totalNumberOfModifyRecord;
    private Common totalNumberOfAddRecord;
    private Common totalNumberOfDeleteRecord;
    private Common totalNumberOfSnapshotRecord;
    private Common totalNumberOfReconRecord;
    private FdsoCertificationTotalRecord fdsoCertificationTotalRecord;
    private Common lastNameDoesNotMatch;
    private Common firstNameDoesNotMatch;
    private Common reconPersonExistOnFdso;
    private CsePerson csePerson;
    private AdministrativeActCertification administrativeActCertification;
    private Common readNextReconRecord;
    private Common totalNumberOfMatchRecords;
    private Common codeDoesNotMatch;
    private Common ssnDoesNotMatch;
    private Common etypeFidmDoesNotMatch;
    private Common etypePasDoesNotMatch;
    private Common etypeTaxDoesNotMatch;
    private Common etypeSalDoesNotMatch;
    private Common etypeVenDoesNotMatch;
    private Common etypeRetDoesNotMatch;
    private Common etypeAdmDoesNotMatch;
    private Common amountDoesNotMatch;
    private Common totalReconMoreThanSnap;
    private Common totalSnapshotMoreThanRecon;
    private Common totalNumberOfMismatch;
    private Common endOfSnapshotFile;
    private Common endOfReconFile;
    private FdsoReconciliationFileRecord batchFdsoReconciliationFileRecord;
    private FdsoSnapshotFileRecord batchFdsoSnapshotFileRecord;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private External passArea;
    private Common fileOpened;
    private AdministrativeActCertification null1;
    private AdministrativeActCertification new1;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public AdministrativeActCertification New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public CsePerson Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    /// <summary>
    /// A value of AdministrativeActCertification.
    /// </summary>
    [JsonPropertyName("administrativeActCertification")]
    public AdministrativeActCertification AdministrativeActCertification
    {
      get => administrativeActCertification ??= new();
      set => administrativeActCertification = value;
    }

    private AdministrativeActCertification new1;
    private CsePerson existing;
    private AdministrativeActCertification administrativeActCertification;
  }
#endregion
}
