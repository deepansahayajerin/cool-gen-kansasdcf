// Program: LE_B501_WRITE_FDSO_SNAPSHOT_FILE, ID: 370999129, model: 746.
// Short name: SWEL501B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_B501_WRITE_FDSO_SNAPSHOT_FILE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class LeB501WriteFdsoSnapshotFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B501_WRITE_FDSO_SNAPSHOT_FILE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB501WriteFdsoSnapshotFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB501WriteFdsoSnapshotFile.
  /// </summary>
  public LeB501WriteFdsoSnapshotFile(IContext context, Import import,
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
    // -------------------------------------------------------------------
    // Date		PR/WR#		Developer	Description
    // 09/18/00         		 Raj Sawant        Initial Creation
    // 10/18/2001	PR130127	E.Shirk		Removed hardcoded 150/500 FDSO threshold 
    // amounts and restructered code to eliminate duplicate logic.	
    // -------------------------------------------------------------------
    // ** 01/13/2006	WR 258945	M.J. Quinn		Allow for FDSO certification of 
    // bankruptcy
    ExitState = "ACO_NN0000_ALL_OK";

    // ******************************************
    // **    Get PPI Info
    // ******************************************
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // **************************************************
    // **    Get checkpoint restart information
    // **************************************************
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmChkpntRestart();

    // **************************************************
    // **   Open error report
    // **************************************************
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    local.EabReportSend.ProgramName = "SWELB501";
    UseCabErrorReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // **************************************************
    // **   Open Control Report
    // **************************************************
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
    UseLeEabWriteFdsoSnapshotFile1();

    if (!IsEmpty(local.PassArea.TextReturnCode))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error in file open for 'le_eab_write_fdso_snapshot_file'.";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "FILE_OPEN_ERROR";

      return;
    }

    foreach(var item in ReadAdministrativeActCertification())
    {
      // **************************************************************
      // **   Only process the latest FDSO certification information.  *
      // **************************************************************
      if (Equal(entities.AdministrativeActCertification.CaseNumber,
        local.AdministrativeActCertification.CaseNumber))
      {
        continue;
      }

      // **************************************************************
      // **   Bypass rows that have not been sent to the feds.
      // **************************************************************
      if (Equal(entities.AdministrativeActCertification.DateSent,
        local.Null1.DateSent))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "This record is not yet sent to Fed , CSE Person Number : " + entities
          .AdministrativeActCertification.CaseNumber;
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        continue;
      }

      // **************************************************************
      // **  Load snapshot export view.
      // *************************************************************
      export.FdsoSnapshotFileRecord.EtypeAdministrativeOffset = "";
      export.FdsoSnapshotFileRecord.EtypeFederalRetirement = "";
      export.FdsoSnapshotFileRecord.EtypeVendorPaymentOrMisc = "";
      export.FdsoSnapshotFileRecord.EtypeFederalSalary = "";
      export.FdsoSnapshotFileRecord.EtypeTaxRefund = "";
      export.FdsoSnapshotFileRecord.EtypePassportDenial = "";
      export.FdsoSnapshotFileRecord.EtypeFinancialInstitution = "";
      export.FdsoSnapshotFileRecord.SubmittingState = "KS";
      export.FdsoSnapshotFileRecord.LocalCode =
        entities.AdministrativeActCertification.LocalCode ?? Spaces(3);
      export.FdsoSnapshotFileRecord.Ssn =
        entities.AdministrativeActCertification.Ssn;
      export.FdsoSnapshotFileRecord.CaseNumber =
        entities.AdministrativeActCertification.CaseNumber;
      export.FdsoSnapshotFileRecord.LastName =
        entities.AdministrativeActCertification.LastName;
      export.FdsoSnapshotFileRecord.FirstName =
        entities.AdministrativeActCertification.FirstName;

      if (!IsEmpty(entities.AdministrativeActCertification.EtypeAdmBankrupt) ||
        !
        IsEmpty(entities.AdministrativeActCertification.
          EtypeAdministrativeOffset))
      {
        export.FdsoSnapshotFileRecord.EtypeAdministrativeOffset = "ADM";
      }

      if (!IsEmpty(entities.AdministrativeActCertification.
        EtypeFederalRetirement))
      {
        export.FdsoSnapshotFileRecord.EtypeFederalRetirement = "RET";
      }

      if (!IsEmpty(entities.AdministrativeActCertification.
        EtypeVendorPaymentOrMisc))
      {
        export.FdsoSnapshotFileRecord.EtypeVendorPaymentOrMisc = "VEN";
      }

      if (!IsEmpty(entities.AdministrativeActCertification.EtypeFederalSalary))
      {
        export.FdsoSnapshotFileRecord.EtypeFederalSalary = "SAL";
      }

      if (!IsEmpty(entities.AdministrativeActCertification.EtypeTaxRefund))
      {
        export.FdsoSnapshotFileRecord.EtypeTaxRefund = "TAX";
      }

      if (!IsEmpty(entities.AdministrativeActCertification.EtypePassportDenial))
      {
        export.FdsoSnapshotFileRecord.EtypePassportDenial = "PAS";
      }

      if (!IsEmpty(entities.AdministrativeActCertification.
        EtypeFinancialInstitution))
      {
        export.FdsoSnapshotFileRecord.EtypeFinancialInstitution = "FIN";
      }

      if (!IsEmpty(entities.AdministrativeActCertification.TtypeAAddNewCase) ||
        !
        IsEmpty(entities.AdministrativeActCertification.
          TtypeDDeleteCertification) || !
        IsEmpty(entities.AdministrativeActCertification.
          TtypeLChangeSubmittingState) || !
        IsEmpty(entities.AdministrativeActCertification.TtypeMModifyAmount) || !
        IsEmpty(entities.AdministrativeActCertification.TtypeRModifyExclusion))
      {
        if (AsChar(entities.AdministrativeActCertification.
          TtypeDDeleteCertification) == 'D')
        {
          export.FdsoSnapshotFileRecord.TransactionType = "D";

          goto Test;
        }

        if (AsChar(entities.AdministrativeActCertification.TtypeAAddNewCase) ==
          'A')
        {
          export.FdsoSnapshotFileRecord.TransactionType = "A";

          goto Test;
        }

        if (AsChar(entities.AdministrativeActCertification.
          TtypeLChangeSubmittingState) == 'L' || AsChar
          (entities.AdministrativeActCertification.TtypeMModifyAmount) == 'M'
          || AsChar
          (entities.AdministrativeActCertification.TtypeRModifyExclusion) == 'R'
          )
        {
          export.FdsoSnapshotFileRecord.TransactionType = "M";
        }
      }

Test:

      // **************************************************************
      // **  Write ADC snapshot row via external.
      // *************************************************************
      if (Lt(0, entities.AdministrativeActCertification.AdcAmount))
      {
        export.FdsoSnapshotFileRecord.CaseType = "A";
        export.FdsoSnapshotFileRecord.CurrentArrearageAmount =
          (int)entities.AdministrativeActCertification.AdcAmount.
            GetValueOrDefault();
        local.PassArea.FileInstruction = "WRITED";
        UseLeEabWriteFdsoSnapshotFile2();

        if (!IsEmpty(local.PassArea.TextReturnCode))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error in writing record in external file  for 'le_eab_write_fdso_snapshot_file'.";
            
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
        else
        {
          ++local.TotalAdcRecordCount.Count;
        }
      }

      // **************************************************************
      // **  Write non-ADC snapshot row via external.
      // *************************************************************
      if (Lt(0, entities.AdministrativeActCertification.NonAdcAmount))
      {
        export.FdsoSnapshotFileRecord.CaseType = "N";
        export.FdsoSnapshotFileRecord.CurrentArrearageAmount =
          (int)entities.AdministrativeActCertification.NonAdcAmount.
            GetValueOrDefault();
        local.PassArea.FileInstruction = "WRITED";
        UseLeEabWriteFdsoSnapshotFile2();

        if (!IsEmpty(local.PassArea.TextReturnCode))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error in writing record in external file  for 'le_eab_write_fdso_snapshot_file'.";
            
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
        else
        {
          ++local.TotalNadcRecordCount.Count;
        }
      }

      // **************************************************************
      // **  Write delete transaction to snapshot file via external.
      // *************************************************************
      if (Equal(entities.AdministrativeActCertification.AdcAmount, 0) && Equal
        (entities.AdministrativeActCertification.NonAdcAmount, 0) && AsChar
        (entities.AdministrativeActCertification.TtypeDDeleteCertification) == 'D'
        )
      {
        export.FdsoSnapshotFileRecord.CurrentArrearageAmount = 0;
        export.FdsoSnapshotFileRecord.CaseType =
          entities.AdministrativeActCertification.CaseType;
        local.PassArea.FileInstruction = "WRITED";
        UseLeEabWriteFdsoSnapshotFile2();

        if (!IsEmpty(local.PassArea.TextReturnCode))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error in writing record in external file  for 'le_eab_write_fdso_snapshot_file'.";
            
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
        else if (AsChar(entities.AdministrativeActCertification.CaseType) == 'A'
          )
        {
          ++local.TotalDeleteAdcRecordCount.Count;
        }
        else if (AsChar(entities.AdministrativeActCertification.CaseType) == 'N'
          )
        {
          ++local.TotalDeleteNadcRecordCount.Count;
        }
      }

      local.AdministrativeActCertification.CaseNumber =
        entities.AdministrativeActCertification.CaseNumber;
    }

    // **************************************************************
    // **  Write audit totals to control file.
    // *************************************************************
    local.EabReportSend.RptDetail = "Total number of ADC records written : " + NumberToString
      (local.TotalAdcRecordCount.Count, 15);
    local.EabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while writting control report(Total number of ADC records written).";
        
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail =
      "Total number of NADC records written : " + NumberToString
      (local.TotalNadcRecordCount.Count, 15);
    local.EabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while writting control report(Total number of NADC records written).";
        
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail =
      "Total number of DELETE - ADC records written : " + NumberToString
      (local.TotalDeleteAdcRecordCount.Count, 15);
    local.EabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while writting control report(Total number of DELETE - ADC records written).";
        
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail =
      "Total number of DELETE - NADC records written : " + NumberToString
      (local.TotalDeleteNadcRecordCount.Count, 15);
    local.EabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while writting control report(Total number of DELETE - NADC records written).";
        
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ***********************************************************
    // **  Close snapshot, error and control report files.    *
    // ***********************************************************
    local.PassArea.FileInstruction = "CLOSE";
    UseLeEabWriteFdsoSnapshotFile1();

    if (!IsEmpty(local.PassArea.TextReturnCode))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error in closing external file  for 'le_eab_write_fdso_snapshot_file'.";
        
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "FILE_CLOSE_ERROR";

      return;
    }

    // *******************************************
    // **  Close error report
    // *******************************************
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // *******************************************
    // **  Close control report
    // *******************************************
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

    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
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

  private void UseLeEabWriteFdsoSnapshotFile1()
  {
    var useImport = new LeEabWriteFdsoSnapshotFile.Import();
    var useExport = new LeEabWriteFdsoSnapshotFile.Export();

    useImport.External.FileInstruction = local.PassArea.FileInstruction;
    useExport.External.Assign(local.PassArea);

    Call(LeEabWriteFdsoSnapshotFile.Execute, useImport, useExport);

    MoveExternal(useExport.External, local.PassArea);
  }

  private void UseLeEabWriteFdsoSnapshotFile2()
  {
    var useImport = new LeEabWriteFdsoSnapshotFile.Import();
    var useExport = new LeEabWriteFdsoSnapshotFile.Export();

    useImport.External.FileInstruction = local.PassArea.FileInstruction;
    useImport.FdsoSnapshotFileRecord.Assign(export.FdsoSnapshotFileRecord);
    useExport.External.Assign(local.PassArea);

    Call(LeEabWriteFdsoSnapshotFile.Execute, useImport, useExport);

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

  private IEnumerable<bool> ReadAdministrativeActCertification()
  {
    entities.AdministrativeActCertification.Populated = false;

    return ReadEach("ReadAdministrativeActCertification",
      null,
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
        entities.AdministrativeActCertification.EtypeAdmBankrupt =
          db.GetNullableString(reader, 45);
        entities.AdministrativeActCertification.DecertificationReason =
          db.GetNullableString(reader, 46);
        entities.AdministrativeActCertification.Populated = true;

        return true;
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
    /// <summary>
    /// A value of FdsoSnapshotFileRecord.
    /// </summary>
    [JsonPropertyName("fdsoSnapshotFileRecord")]
    public FdsoSnapshotFileRecord FdsoSnapshotFileRecord
    {
      get => fdsoSnapshotFileRecord ??= new();
      set => fdsoSnapshotFileRecord = value;
    }

    private FdsoSnapshotFileRecord fdsoSnapshotFileRecord;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of TotalDeleteNadcRecordCount.
    /// </summary>
    [JsonPropertyName("totalDeleteNadcRecordCount")]
    public Common TotalDeleteNadcRecordCount
    {
      get => totalDeleteNadcRecordCount ??= new();
      set => totalDeleteNadcRecordCount = value;
    }

    /// <summary>
    /// A value of TotalDeleteAdcRecordCount.
    /// </summary>
    [JsonPropertyName("totalDeleteAdcRecordCount")]
    public Common TotalDeleteAdcRecordCount
    {
      get => totalDeleteAdcRecordCount ??= new();
      set => totalDeleteAdcRecordCount = value;
    }

    /// <summary>
    /// A value of TotalNadcRecordCount.
    /// </summary>
    [JsonPropertyName("totalNadcRecordCount")]
    public Common TotalNadcRecordCount
    {
      get => totalNadcRecordCount ??= new();
      set => totalNadcRecordCount = value;
    }

    /// <summary>
    /// A value of TotalAdcRecordCount.
    /// </summary>
    [JsonPropertyName("totalAdcRecordCount")]
    public Common TotalAdcRecordCount
    {
      get => totalAdcRecordCount ??= new();
      set => totalAdcRecordCount = value;
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
    /// A value of AdministrativeActCertification.
    /// </summary>
    [JsonPropertyName("administrativeActCertification")]
    public AdministrativeActCertification AdministrativeActCertification
    {
      get => administrativeActCertification ??= new();
      set => administrativeActCertification = value;
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

    private Common totalDeleteNadcRecordCount;
    private Common totalDeleteAdcRecordCount;
    private Common totalNadcRecordCount;
    private Common totalAdcRecordCount;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private External passArea;
    private AdministrativeActCertification administrativeActCertification;
    private AdministrativeActCertification null1;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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

    /// <summary>
    /// A value of AdministrativeActCertification.
    /// </summary>
    [JsonPropertyName("administrativeActCertification")]
    public AdministrativeActCertification AdministrativeActCertification
    {
      get => administrativeActCertification ??= new();
      set => administrativeActCertification = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
    private AdministrativeActCertification administrativeActCertification;
  }
#endregion
}
