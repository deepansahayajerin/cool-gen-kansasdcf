// Program: LE_B510_CERTIFY_SDSO, ID: 372662714, model: 746.
// Short name: SWEL510B
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
/// A program: LE_B510_CERTIFY_SDSO.
/// </para>
/// <para>
/// This skeleton uses:
/// A DB2 table to drive processing
/// An external to do DB2 commits
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class LeB510CertifySdso: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B510_CERTIFY_SDSO program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB510CertifySdso(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB510CertifySdso.
  /// </summary>
  public LeB510CertifySdso(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // --------------------------------------------------------------
    // Date	By
    // Description
    // --------------------------------------------------------------
    // ??????	H.Hooks
    // Initial code
    // 120397	govind
    // Fixed bug: On invalid SSN it was not writing to program
    // error. It was ending the program.
    // Removed the usage of persistent views on program run.
    // 01/07-20/99	P McElderry
    // Added functionality commiserate w/new requirements.
    // Deleted unneccesary logic.
    // Added control and error logic.
    // PMcElderry	08/16/99
    // Logic to close adabase
    // PMcElderry	07/24/2000
    // PR # 100333-A:  terminate processing if ADABAS
    // unavailable.
    // Removed recursive flow
    // WR000269	04/16/01		E.Shirk
    // Corrected inconsistency with business rules related to a valid SSN 
    // existing on the system prior to reporting for SDSO.   Previously the
    // CAB_VALIDATE_SSN was being called but the returned ind was never checked.
    // Now the ind is checked and the Obligor will be bypassed if no SSN on
    // the AE system exists.
    // 06/25/2003	E.Shirk			Prod Fix
    // Added logic to process Adabas_Read_Unsuccessful exit state in pstep.   
    // Added logic to write ap number with the invalid ssn to the error report.
    // WR296917 Part A 09/11/07                M Fan
    // AP's in bankruptcy must be certified for SDSO  due to federal bankruptcy 
    // law changes.
    // Commented out the codes that were bankruptcy related.
    // CQ45957		11/06/2014 		GVandy
    // Correct error logic when returning from SSN validation.
    // ----------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;
    local.Current.Timestamp = Now();

    // --------------------------------
    // Get run parameters for program
    // --------------------------------
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.ProcessDate.ProcMonth = local.ProgramProcessingInfo.ProcessDate;
      local.ProgramCheckpointRestart.ProgramName = global.UserId;
      local.IncludeBankruptcy.Flag =
        Substring(local.ProgramProcessingInfo.ParameterList, 1, 1);

      // --------------------------------
      // Get DB2 commit frequency counts
      // --------------------------------
      UseReadPgmCheckpointRestart();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
        {
          local.CheckpointRestartKey.Number =
            TrimEnd(local.ProgramCheckpointRestart.RestartInfo);
        }
        else
        {
          local.CheckpointRestartKey.Number = "";
        }

        local.ReportEabReportSend.ProcessDate = local.Current.Date;
        local.ReportEabReportSend.ProgramName = "SWELB510";
        local.ReportEabFileHandling.Action = "OPEN";
        UseCabControlReport2();

        if (Equal(local.ReportEabFileHandling.Status, "OK"))
        {
          // -------------------
          // continue processing
          // -------------------
        }
        else
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        local.ReportEabFileHandling.Action = "OPEN";
        UseCabErrorReport2();

        if (Equal(local.ReportEabFileHandling.Status, "OK"))
        {
          // -------------------
          // continue processing
          // -------------------
        }
        else
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }
      else
      {
        // --------------------
        // terminable situation
        // --------------------
        return;
      }
    }
    else
    {
      // --------------------
      // terminable situation
      // --------------------
      return;
    }

    // ---------------
    // Main processing
    // ---------------
    foreach(var item in ReadCsePersonObligor())
    {
      ExitState = "ACO_NN0000_ALL_OK";
      UseCabValidateSsn();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        // -- Continue
      }
      else
      {
        if (IsExitState("ACO_ADABAS_UNAVAILABLE") || IsExitState
          ("ACO_NE0000_CICS_UNAVAILABLE"))
        {
          local.ReportEabReportSend.RptDetail =
            "Program aborted; ADABAS unavailable; CSE person is " + entities
            .CsePerson.Number;
          local.ReportEabFileHandling.Action = "WRITE";
          UseCabErrorReport1();

          if (Equal(local.ReportEabFileHandling.Status, "OK"))
          {
            local.ReportEabFileHandling.Action = "CLOSE";
            UseCabErrorReport2();

            if (Equal(local.ReportEabFileHandling.Status, "OK"))
            {
              // -------------------
              // continue processing
              // -------------------
            }
            else
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            local.ReportEabFileHandling.Action = "CLOSE";
            UseCabControlReport2();

            if (Equal(local.ReportEabFileHandling.Status, "OK"))
            {
              // -------------------
              // continue processing
              // -------------------
            }
            else
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            ExitState = "LE0000_ADABAS_UNAVAILABLE_ABORT";

            return;
          }
          else
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }
        else
        {
          local.ReportEabFileHandling.Action = "WRITE";

          if (IsExitState("LE0000_SSN_IS_BLANK") || IsExitState
            ("LE0000_SSN_HAS_LT_9_CHARS") || IsExitState
            ("LE0000_SSN_CONTAINS_NONNUM") || IsExitState
            ("ACO_NE0000_INVALID_SSN2") || IsExitState
            ("ACO_NE0000_INVALID_SSN4") || IsExitState
            ("LE0000_SSN_1ST_3_INVALID_CHAR"))
          {
            // -- SSN must be on the AE system prior to certifying for SDSO.
            local.ReportEabReportSend.RptDetail =
              "SDSO certification bypassed due to invalid SSN for AP number: " +
              entities.CsePerson.Number;
          }
          else
          {
            local.ReportEabReportSend.RptDetail =
              "Error returning from cab_validate_ssn for AP number  " + entities
              .CsePerson.Number;
          }

          UseCabErrorReport1();

          if (Equal(local.ReportEabFileHandling.Status, "OK"))
          {
            ++local.TotalNoOfErrorsWritten.Count;
          }
          else
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          local.Temp.Message = UseEabExtractExitStateMessage();
          local.ReportEabReportSend.RptDetail = "Exit state message is : " + local
            .Temp.Message;
          UseCabErrorReport1();

          if (Equal(local.ReportEabFileHandling.Status, "OK"))
          {
          }
          else
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }

        ExitState = "ACO_NN0000_ALL_OK";

        continue;
      }

      if (ReadAdministrativeActCertification())
      {
        continue;
      }

      UseLeCreateSdsoCertificationV2();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        ++local.CheckpointNumbOfUpdates.Count;
      }
      else
      {
        if (IsExitState("FN0000_OBLIGATION_NF"))
        {
          local.SelectCsePersonId.Flag = "Y";
        }
        else if (IsExitState("BANKRUPTCY_FILED"))
        {
          // M Fan WR296917 Part A - 9/11/2007  Commented out(Disabled) the 
          // following SET statement.
        }
        else if (IsExitState("CSE_PERSON_NOT_OBLIGOR"))
        {
          local.SelectCsePersonId.Flag = "Y";
        }
        else if (IsExitState("FN0000_OBLIGATION_TYPE_NF"))
        {
          // ---------------------------------------------------------
          // Needed information exists in "local from sdso key info"
          // ---------------------------------------------------------
        }
        else if (IsExitState("STATE_DEBT_SETOFF_AE"))
        {
          // ---------------------------------------------------------
          // Needed information exists in "local from sdso key info"
          // ---------------------------------------------------------
        }
        else if (IsExitState("ADMINISTRATIVE_ACTION_NF"))
        {
          // ---------------------------------------------------------
          // terminate immediately - wasting cpu time
          // ---------------------------------------------------------
          return;
        }
        else if (IsExitState("STATE_DEBT_SETOFF_NU"))
        {
          // ---------------------------------------------------------
          // Needed information exists in "local from sdso key info"
          // ---------------------------------------------------------
        }
        else if (IsExitState("LE0000_GOOD_CAUSE_FOR_OBLIGOR"))
        {
          local.SelectCsePersonId.Flag = "Y";
        }
        else
        {
        }

        local.Temp.Message = UseEabExtractExitStateMessage();

        if (AsChar(local.SelectCsePersonId.Flag) == 'Y')
        {
          local.ProgramErrorCode.RptDetail = " CSE Person ID " + entities
            .CsePerson.Number;
          local.ExitStateWorkArea.Message = TrimEnd(local.Temp.Message) + local
            .ProgramErrorCode.RptDetail;
          local.SelectCsePersonId.Flag = "";
        }
        else
        {
          local.ExitStateWorkArea.Message = TrimEnd(local.Temp.Message) + local
            .FromCreateSdsoKeyInfo.RptDetail;
        }

        local.ErrorFound.Flag = "Y";
        ExitState = "ACO_NN0000_ALL_OK";
      }

      if (local.CheckpointNumbOfUpdates.Count >= local
        .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault() || AsChar
        (local.ErrorFound.Flag) == 'Y')
      {
        // ---------------------------------------------------------
        // Record the number of checkpoints and the last checkpoint
        // time and set the restart indicator to yes.
        // Also return the checkpoint frequency counts in case they
        // been changed since the last read.
        // ---------------------------------------------------------
        local.ProgramCheckpointRestart.RestartInfo = entities.CsePerson.Number;
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.LastCheckpointTimestamp =
          local.Current.Timestamp;
        ExitState = "ACO_NN0000_ALL_OK";
        UseUpdatePgmCheckpointRestart2();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (AsChar(local.ErrorFound.Flag) != 'Y')
          {
            // -------------------------------------------------------------
            // Call an external that does a DB2 commit using a Cobol program
            // -------------------------------------------------------------
            UseExtToDoACommit();

            if (local.PassArea.NumericReturnCode != 0)
            {
              // --------------------
              // terminable situation
              // --------------------
              ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

              return;
            }
            else
            {
              local.CheckpointNumbOfUpdates.Count = 0;
            }
          }
        }
        else
        {
          // --------------------
          // terminable situation
          // --------------------
          return;
        }
      }
      else
      {
        // -------------------
        // continue processing
        // -------------------
      }

      if (AsChar(local.ErrorFound.Flag) != 'Y' && AsChar
        (local.SdsoRecordCreated.Flag) == 'Y')
      {
        ++local.TotalNoOfRecsWritten.Count;
        local.SdsoRecordCreated.Flag = "";
      }
      else if (AsChar(local.ErrorFound.Flag) == 'Y')
      {
        // M Fan WR296917 Part A - 9/11/2007  Commented out(Disabled) the 
        // following statements.
        local.ReportEabReportSend.RptDetail = "Exit state is " + local
          .ExitStateWorkArea.Message;
        local.ReportEabFileHandling.Action = "WRITE";
        UseCabErrorReport1();

        if (Equal(local.ReportEabFileHandling.Status, "OK"))
        {
          ++local.TotalNoOfErrorsWritten.Count;
        }
        else
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        local.ReportEabReportSend.RptDetail = "";
        local.ReportEabFileHandling.Action = "WRITE";
        UseCabErrorReport1();

        if (Equal(local.ReportEabFileHandling.Status, "OK"))
        {
          // -------------------
          // continue processing
          // -------------------
        }
        else
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }
      else
      {
        // ---------------------------------------------------------
        // continue processing - uncritical problem w/program.
        // i.e. obligor has obls exempted or debts < threshold.
        // ---------------------------------------------------------
      }

      // ------------
      // Reinitialize
      // ------------
      local.ErrorFound.Flag = "";
      local.ProgramErrorCode.RptDetail = "";
      local.ReportEabReportSend.RptDetail = "";
      local.ExitStateWorkArea.Message = "";
      local.Temp.Message = "";
      local.SelectCsePersonId.Flag = "";
      ExitState = "ACO_NN0000_ALL_OK";
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }
    else
    {
      // -------------------
      // continue processing
      // -------------------
    }

    local.TotalAllRecordsWritten.Count = local.TotalNoOfErrorsWritten.Count + local
      .TotalNoOfRecsWritten.Count;
    local.ReportEabReportSend.RptDetail =
      "Total Number of Records Processed = " + NumberToString
      (local.TotalAllRecordsWritten.Count, 15);
    local.ReportEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportEabFileHandling.Status, "OK"))
    {
      // -------------------
      // continue processing
      // -------------------
    }
    else
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.ReportEabReportSend.RptDetail = "";
    local.ReportEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportEabFileHandling.Status, "OK"))
    {
      // -------------------
      // continue processing
      // -------------------
    }
    else
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.ReportEabReportSend.RptDetail =
      "Total Number of Records Written = " + NumberToString
      (local.TotalNoOfRecsWritten.Count, 15);
    local.ReportEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportEabFileHandling.Status, "OK"))
    {
      // -------------------
      // continue processing
      // -------------------
    }
    else
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.ReportEabReportSend.RptDetail = "";
    local.ReportEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportEabFileHandling.Status, "OK"))
    {
      // -------------------
      // continue processing
      // -------------------
    }
    else
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.ReportEabReportSend.RptDetail =
      "Total Number of Errors Written = " + NumberToString
      (local.TotalNoOfErrorsWritten.Count, 15);
    local.ReportEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportEabFileHandling.Status, "OK"))
    {
      // -------------------
      // continue processing
      // -------------------
    }
    else
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.ReportEabFileHandling.Action = "CLOSE";
    UseCabControlReport2();

    if (Equal(local.ReportEabFileHandling.Status, "OK"))
    {
      // -------------------
      // continue processing
      // -------------------
    }
    else
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.ReportEabFileHandling.Action = "CLOSE";
    UseCabErrorReport2();

    if (Equal(local.ReportEabFileHandling.Status, "OK"))
    {
      // -------------------
      // continue processing
      // -------------------
    }
    else
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.ProgramCheckpointRestart.RestartInfo = "";
    local.ProgramCheckpointRestart.RestartInd = "N";
    UseUpdatePgmCheckpointRestart1();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // -------------------
      // continue processing
      // -------------------
    }
    else
    {
      return;
    }

    ExitState = "ACO_NN0000_ALL_OK";
    UseSiCloseAdabas();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
    }
    else
    {
      // -----------------------------------------------
      // this is a soft error so processing can continue
      // -----------------------------------------------
    }
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
    target.ReadFrequencyCount = source.ReadFrequencyCount;
    target.CheckpointCount = source.CheckpointCount;
    target.LastCheckpointTimestamp = source.LastCheckpointTimestamp;
    target.RestartInd = source.RestartInd;
    target.RestartInfo = source.RestartInfo;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.ReportEabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.ReportEabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.ReportEabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.ReportEabFileHandling.Action;
    MoveEabReportSend(local.ReportEabReportSend, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    local.ReportEabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.ReportEabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.ReportEabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.ReportEabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.ReportEabFileHandling.Action;
    MoveEabReportSend(local.ReportEabReportSend, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.ReportEabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabValidateSsn()
  {
    var useImport = new CabValidateSsn.Import();
    var useExport = new CabValidateSsn.Export();

    useImport.CsePerson.Number = entities.CsePerson.Number;

    Call(CabValidateSsn.Execute, useImport, useExport);

    local.ValidSsn.Flag = useExport.ValidSsn.Flag;
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

    useExport.External.NumericReturnCode = local.PassArea.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.PassArea.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseLeCreateSdsoCertificationV2()
  {
    var useImport = new LeCreateSdsoCertificationV2.Import();
    var useExport = new LeCreateSdsoCertificationV2.Export();

    useImport.CsePerson.Number = entities.CsePerson.Number;
    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;
    useImport.ExcludeBankruptcy.Flag = local.IncludeBankruptcy.Flag;

    Call(LeCreateSdsoCertificationV2.Execute, useImport, useExport);

    local.FromCreateSdsoKeyInfo.RptDetail = useExport.KeyInfo.RptDetail;
    local.SdsoRecordCreated.Flag = useExport.SdsoRecordCreated.Flag;
  }

  private void UseReadPgmCheckpointRestart()
  {
    var useImport = new ReadPgmCheckpointRestart.Import();
    var useExport = new ReadPgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      local.ProgramCheckpointRestart.ProgramName;

    Call(ReadPgmCheckpointRestart.Execute, useImport, useExport);

    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseSiCloseAdabas()
  {
    var useImport = new SiCloseAdabas.Import();
    var useExport = new SiCloseAdabas.Export();

    Call(SiCloseAdabas.Execute, useImport, useExport);
  }

  private void UseUpdatePgmCheckpointRestart1()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);
  }

  private void UseUpdatePgmCheckpointRestart2()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }

  private bool ReadAdministrativeActCertification()
  {
    System.Diagnostics.Debug.Assert(entities.Obligor.Populated);
    entities.AdministrativeActCertification.Populated = false;

    return Read("ReadAdministrativeActCertification",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.Obligor.Type1);
        db.SetString(command, "cspNumber", entities.Obligor.CspNumber);
        db.SetDate(
          command, "takenDt",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
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
        entities.AdministrativeActCertification.TanfCode =
          db.GetString(reader, 4);
        entities.AdministrativeActCertification.Populated = true;
        CheckValid<AdministrativeActCertification>("CpaType",
          entities.AdministrativeActCertification.CpaType);
        CheckValid<AdministrativeActCertification>("Type1",
          entities.AdministrativeActCertification.Type1);
      });
  }

  private IEnumerable<bool> ReadCsePersonObligor()
  {
    entities.CsePerson.Populated = false;
    entities.Obligor.Populated = false;

    return ReadEach("ReadCsePersonObligor",
      (db, command) =>
      {
        db.SetString(command, "numb", local.CheckpointRestartKey.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.Obligor.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.Obligor.Type1 = db.GetString(reader, 2);
        entities.CsePerson.Populated = true;
        entities.Obligor.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
        CheckValid<CsePersonAccount>("Type1", entities.Obligor.Type1);

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
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of BankruptcyError.
    /// </summary>
    [JsonPropertyName("bankruptcyError")]
    public Common BankruptcyError
    {
      get => bankruptcyError ??= new();
      set => bankruptcyError = value;
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
    /// A value of SelectCsePersonId.
    /// </summary>
    [JsonPropertyName("selectCsePersonId")]
    public Common SelectCsePersonId
    {
      get => selectCsePersonId ??= new();
      set => selectCsePersonId = value;
    }

    /// <summary>
    /// A value of ProgramErrorCode.
    /// </summary>
    [JsonPropertyName("programErrorCode")]
    public EabReportSend ProgramErrorCode
    {
      get => programErrorCode ??= new();
      set => programErrorCode = value;
    }

    /// <summary>
    /// A value of FromCreateSdsoExitstate.
    /// </summary>
    [JsonPropertyName("fromCreateSdsoExitstate")]
    public EabReportSend FromCreateSdsoExitstate
    {
      get => fromCreateSdsoExitstate ??= new();
      set => fromCreateSdsoExitstate = value;
    }

    /// <summary>
    /// A value of FromCreateSdsoKeyInfo.
    /// </summary>
    [JsonPropertyName("fromCreateSdsoKeyInfo")]
    public EabReportSend FromCreateSdsoKeyInfo
    {
      get => fromCreateSdsoKeyInfo ??= new();
      set => fromCreateSdsoKeyInfo = value;
    }

    /// <summary>
    /// A value of SdsoRecordCreated.
    /// </summary>
    [JsonPropertyName("sdsoRecordCreated")]
    public Common SdsoRecordCreated
    {
      get => sdsoRecordCreated ??= new();
      set => sdsoRecordCreated = value;
    }

    /// <summary>
    /// A value of TotalNoOfRecsWritten.
    /// </summary>
    [JsonPropertyName("totalNoOfRecsWritten")]
    public Common TotalNoOfRecsWritten
    {
      get => totalNoOfRecsWritten ??= new();
      set => totalNoOfRecsWritten = value;
    }

    /// <summary>
    /// A value of TotalNoOfErrorsWritten.
    /// </summary>
    [JsonPropertyName("totalNoOfErrorsWritten")]
    public Common TotalNoOfErrorsWritten
    {
      get => totalNoOfErrorsWritten ??= new();
      set => totalNoOfErrorsWritten = value;
    }

    /// <summary>
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public ExitStateWorkArea Temp
    {
      get => temp ??= new();
      set => temp = value;
    }

    /// <summary>
    /// A value of ErrorFilesOpened.
    /// </summary>
    [JsonPropertyName("errorFilesOpened")]
    public Common ErrorFilesOpened
    {
      get => errorFilesOpened ??= new();
      set => errorFilesOpened = value;
    }

    /// <summary>
    /// A value of ReportFilesOpened.
    /// </summary>
    [JsonPropertyName("reportFilesOpened")]
    public Common ReportFilesOpened
    {
      get => reportFilesOpened ??= new();
      set => reportFilesOpened = value;
    }

    /// <summary>
    /// A value of ValidSsn.
    /// </summary>
    [JsonPropertyName("validSsn")]
    public Common ValidSsn
    {
      get => validSsn ??= new();
      set => validSsn = value;
    }

    /// <summary>
    /// A value of ErrorFound.
    /// </summary>
    [JsonPropertyName("errorFound")]
    public Common ErrorFound
    {
      get => errorFound ??= new();
      set => errorFound = value;
    }

    /// <summary>
    /// A value of TotalAllRecordsWritten.
    /// </summary>
    [JsonPropertyName("totalAllRecordsWritten")]
    public Common TotalAllRecordsWritten
    {
      get => totalAllRecordsWritten ??= new();
      set => totalAllRecordsWritten = value;
    }

    /// <summary>
    /// A value of ReportEabFileHandling.
    /// </summary>
    [JsonPropertyName("reportEabFileHandling")]
    public EabFileHandling ReportEabFileHandling
    {
      get => reportEabFileHandling ??= new();
      set => reportEabFileHandling = value;
    }

    /// <summary>
    /// A value of ReportEabReportSend.
    /// </summary>
    [JsonPropertyName("reportEabReportSend")]
    public EabReportSend ReportEabReportSend
    {
      get => reportEabReportSend ??= new();
      set => reportEabReportSend = value;
    }

    /// <summary>
    /// A value of AbortProgramIndicator.
    /// </summary>
    [JsonPropertyName("abortProgramIndicator")]
    public Common AbortProgramIndicator
    {
      get => abortProgramIndicator ??= new();
      set => abortProgramIndicator = value;
    }

    /// <summary>
    /// A value of ProcessOption.
    /// </summary>
    [JsonPropertyName("processOption")]
    public Common ProcessOption
    {
      get => processOption ??= new();
      set => processOption = value;
    }

    /// <summary>
    /// A value of ProcessDate.
    /// </summary>
    [JsonPropertyName("processDate")]
    public OblgWork ProcessDate
    {
      get => processDate ??= new();
      set => processDate = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
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
    /// A value of CheckpointNumbOfUpdates.
    /// </summary>
    [JsonPropertyName("checkpointNumbOfUpdates")]
    public Common CheckpointNumbOfUpdates
    {
      get => checkpointNumbOfUpdates ??= new();
      set => checkpointNumbOfUpdates = value;
    }

    /// <summary>
    /// A value of HoldForSafeKeeping.
    /// </summary>
    [JsonPropertyName("holdForSafeKeeping")]
    public ProgramError HoldForSafeKeeping
    {
      get => holdForSafeKeeping ??= new();
      set => holdForSafeKeeping = value;
    }

    /// <summary>
    /// A value of CheckpointRestartKey.
    /// </summary>
    [JsonPropertyName("checkpointRestartKey")]
    public CsePerson CheckpointRestartKey
    {
      get => checkpointRestartKey ??= new();
      set => checkpointRestartKey = value;
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
    /// A value of IncludeBankruptcy.
    /// </summary>
    [JsonPropertyName("includeBankruptcy")]
    public Common IncludeBankruptcy
    {
      get => includeBankruptcy ??= new();
      set => includeBankruptcy = value;
    }

    private Common bankruptcyError;
    private DateWorkArea current;
    private Common selectCsePersonId;
    private EabReportSend programErrorCode;
    private EabReportSend fromCreateSdsoExitstate;
    private EabReportSend fromCreateSdsoKeyInfo;
    private Common sdsoRecordCreated;
    private Common totalNoOfRecsWritten;
    private Common totalNoOfErrorsWritten;
    private ExitStateWorkArea temp;
    private Common errorFilesOpened;
    private Common reportFilesOpened;
    private Common validSsn;
    private Common errorFound;
    private Common totalAllRecordsWritten;
    private EabFileHandling reportEabFileHandling;
    private EabReportSend reportEabReportSend;
    private Common abortProgramIndicator;
    private Common processOption;
    private OblgWork processDate;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
    private External passArea;
    private Common checkpointNumbOfUpdates;
    private ProgramError holdForSafeKeeping;
    private CsePerson checkpointRestartKey;
    private ExitStateWorkArea exitStateWorkArea;
    private Common includeBankruptcy;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of AdministrativeActCertification.
    /// </summary>
    [JsonPropertyName("administrativeActCertification")]
    public AdministrativeActCertification AdministrativeActCertification
    {
      get => administrativeActCertification ??= new();
      set => administrativeActCertification = value;
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    private CsePersonAccount csePersonAccount;
    private AdministrativeActCertification administrativeActCertification;
    private CsePerson csePerson;
    private CsePersonAccount obligor;
  }
#endregion
}
